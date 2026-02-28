import '/lib/zxing/zxing.min.js';

const scanners = new Map();

// --- Core Lifecycle Methods ---

export function init(_instance, _element, _elementid, _options, _deviceid) {
    const state = {
        id: _elementid,
        instance: _instance,
        element: _element,
        options: _options || {},
        selectedDeviceId: _deviceid,
        codeReader: null,
        track: null,
        isDisposed: false,
        debug: _options?.debug || false,
        supportsVibrate: "vibrate" in navigator
    };

    if (scanners.has(_elementid)) destroy(_elementid);
    scanners.set(_elementid, state);

    if (state.debug) console.log(`[${_elementid}] Initializing scanner`);

    bindButtons(state);

    // Run asynchronously to prevent Blazor SignalR circuit timeouts
    setupCameraFlow(state).catch(err => {
        logError(state, `Setup flow failed: ${err.message}`);
        safeInvoke(state, 'GetError', `Camera setup failed: ${err.message}`);
    });
}

export function reload(elementid) {
    const state = scanners.get(elementid);
    if (state) setupCameraFlow(state).catch(console.error);
}

export function start(elementid) {
    const state = scanners.get(elementid);
    if (!state || !state.codeReader) return;

    if (state.debug) console.log(`[${elementid}] Starting decode`);

    if (state.options.decodeonce) {
        state.codeReader.decodeOnceFromVideoDevice(state.selectedDeviceId, 'video')
            .then(result => handleSuccess(state, result))
            .catch(err => handleError(state, err));
    } else {
        state.codeReader.decodeFromVideoDevice(state.selectedDeviceId, 'video', (result, err) => {
            if (result) handleSuccess(state, result);
            if (err) handleError(state, err);
        });
    }
}

export function stop(elementid) {
    const state = scanners.get(elementid);
    if (state && state.codeReader) {
        state.codeReader.reset();
        if (state.debug) console.log(`[${elementid}] Stopped decoding`);
    }
}

export function destroy(elementid) {
    const state = scanners.get(elementid);
    if (!state) return;

    state.isDisposed = true;

    if (state.track) {
        toggleFlashlight(elementid, false);
        state.track.stop();
        state.track = null;
    }
    if (state.codeReader) {
        state.codeReader.reset();
        state.codeReader = null;
    }

    scanners.delete(elementid);
    if (state.debug) console.log(`[${elementid}] Destroyed scanner`);
}

// --- Private / Internal Flow Methods ---

async function setupCameraFlow(state) {
    if (state.isDisposed) return;

    const hasAccess = await ensureCameraAccess(state);
    if (!hasAccess || state.isDisposed) {
        safeInvoke(state, 'GetError', 'Camera access denied or unavailable.');
        return;
    }

    state.codeReader = createCodeReader(state);

    if (!navigator.mediaDevices?.enumerateDevices) {
        throw new Error("enumerateDevices() not supported by this browser.");
    }

    const devices = await navigator.mediaDevices.enumerateDevices();
    const videoInputDevices = devices.filter(d => d.kind === 'videoinput');

    if (videoInputDevices.length === 0) throw new Error('No video input devices found');

    if (!state.selectedDeviceId) {
        state.selectedDeviceId = videoInputDevices.length > 1
            ? videoInputDevices[1].deviceId
            : videoInputDevices[0].deviceId;
    }

    buildCameraSelector(state, videoInputDevices);
    await startCameraStream(state);
}

async function ensureCameraAccess(state) {
    try {
        const stream = await navigator.mediaDevices.getUserMedia({ video: true });
        stream.getTracks().forEach(track => track.stop());
        return true;
    } catch (error) {
        logError(state, `Camera access denied: ${error.message}`);
        return false;
    }
}

async function startCameraStream(state) {
    if (state.isDisposed) return;
    if (state.track) state.track.stop();

    const constraints = {
        video: {
            deviceId: state.selectedDeviceId ? { exact: state.selectedDeviceId } : undefined,
            width: { ideal: state.options.width || 1920 },
            height: { ideal: state.options.height || 1080 },
            facingMode: "environment",
            focusMode: "continuous",
        },
        audio: false
    };

    try {
        const stream = await navigator.mediaDevices.getUserMedia(constraints);
        if (state.isDisposed) {
            stream.getTracks().forEach(t => t.stop());
            return;
        }

        state.track = stream.getVideoTracks()[0];
        updateFlashlightUI(state);
        start(state.id);

    } catch (err) {
        logError(state, `Failed to start stream: ${err.message}`);
        safeInvoke(state, 'GetError', `Failed to start stream: ${err.message}`);
    }
}

export function QRCodeSvg(instance, input, element, tobase64, size = 300) {
    const codeWriter = new ZXing.BrowserQRCodeSvgWriter()

    if (tobase64) {
        const elementTemp = document.createElement('elementTemp');
        codeWriter.writeToDom(elementTemp, input, size, size)
        let svgElement = elementTemp.firstChild
        const svgData = (new XMLSerializer()).serializeToString(svgElement)
        //const blob = new Blob([svgData])
        instance.invokeMethodAsync("GetQRCode", svgData);
    } else {
        codeWriter.writeToDom(element.querySelector("[data-action=result]"), input, size, size)
    }
}

// --- UI & Helper Methods ---

function bindButtons(state) {
    const btnStart = state.element.querySelector("[data-action=startButton]");
    const btnReset = state.element.querySelector("[data-action=resetButton]");
    const btnClose = state.element.querySelector("[data-action=closeButton]");

    if (btnStart) btnStart.addEventListener('click', () => start(state.id));
    if (btnReset) btnReset.addEventListener('click', () => stop(state.id));
    if (btnClose) btnClose.addEventListener('click', () => {
        stop(state.id);
        safeInvoke(state, "CloseScan");
    });
}

function buildCameraSelector(state, videoInputDevices) {
    const sourceSelect = state.element.querySelector(".dropdown-content") || document.querySelector(".dropdown-content");
    if (!sourceSelect || videoInputDevices.length <= 1) return;

    sourceSelect.innerHTML = '';

    videoInputDevices.forEach((device, index) => {
        const option = document.createElement('div');
        option.innerHTML = device.label || `Camera ${index + 1}`;
        option.value = device.deviceId;

        if (device.deviceId === state.selectedDeviceId) {
            option.classList.add('selected');
            const dropdownText = document.querySelector('.dropdown-text');
            if (dropdownText) dropdownText.textContent = option.innerHTML;
        }

        option.addEventListener('click', async (e) => {
            state.selectedDeviceId = e.target.value;
            safeInvoke(state, 'SelectDeviceID', state.selectedDeviceId, e.target.innerHTML);

            // Sync with frontend UI script
            if (typeof window.selectCamera === 'function') {
                window.selectCamera(e);
            }

            if (state.codeReader) state.codeReader.reset();
            await startCameraStream(state);
        });

        sourceSelect.appendChild(option);
    });

    setTimeout(() => {
        if (!state.isDisposed) {
            const container = document.querySelector('.scanner-container');
            if (container) container.classList.add('show');
        }
    }, 400);
}

export async function toggleFlashlight(elementid, torchOn) {
    // Allows UI script to call this globally with just the boolean state
    if (typeof elementid === 'boolean') {
        torchOn = elementid;
        elementid = scanners.keys().next().value;
    }

    const state = scanners.get(elementid);
    if (!state || !state.track) return false;

    const capabilities = state.track.getCapabilities();
    if (!capabilities.torch) return false;

    try {
        await state.track.applyConstraints({ advanced: [{ torch: torchOn }] });
        return true;
    } catch (err) {
        logError(state, `Flashlight toggle failed: ${err.message}`);
        return false;
    }
}
// Expose to window for the frontend UI to reach it
window.toggleFlashlight = toggleFlashlight;

function updateFlashlightUI(state) {
    const flashlightBtn = document.querySelector(".btn-flashlight");
    if (flashlightBtn && state.track) {
        const capabilities = state.track.getCapabilities();
        flashlightBtn.style.visibility = capabilities.torch ? 'visible' : 'hidden';
    }
}

// --- ZXing Configuration & Error Handling ---

function createCodeReader(state) {
    const hints = new Map();
    const opt = state.options;

    if (opt.TRY_HARDER) hints.set(ZXing.DecodeHintType.TRY_HARDER, opt.TRY_HARDER);
    if (opt.ASSUME_CODE_39_CHECK_DIGIT) hints.set(ZXing.DecodeHintType.ASSUME_CODE_39_CHECK_DIGIT, opt.ASSUME_CODE_39_CHECK_DIGIT);
    if (opt.ASSUME_GS1) hints.set(ZXing.DecodeHintType.ASSUME_GS1, opt.ASSUME_GS1);

    let reader = opt.pdf417
        ? new ZXing.BrowserPDF417Reader(hints)
        : new ZXing.BrowserMultiFormatReader(hints);

    if (opt.decodeAllFormats && !opt.pdf417) {
        hints.set(ZXing.DecodeHintType.POSSIBLE_FORMATS, opt.formats);
    }

    reader.timeBetweenDecodingAttempts = opt.timeBetweenDecodingAttempts || 500;
    return reader;
}

function handleSuccess(state, result) {
    if (state.isDisposed) return;
    if (state.supportsVibrate) try { navigator.vibrate(200); } catch { }

    if (state.debug) console.log(`[${state.id}] Decoded:`, result.text);

    if (state.options.decodeonce) state.codeReader.reset();
    safeInvoke(state, "GetResult", result.text);
}

function handleError(state, err) {
    if (state.isDisposed || err instanceof ZXing.NotFoundException) return;
    logError(state, `Decode error: ${err.message || err}`);
    safeInvoke(state, "GetError", err.toString());
}

function safeInvoke(state, method, ...args) {
    if (!state.isDisposed && state.instance) {
        try { state.instance.invokeMethodAsync(method, ...args); }
        catch (err) { console.error(`Blazor invoke failed (${method}):`, err); }
    }
}

function logError(state, msg) {
    if (state.debug) console.error(`[${state.id}] ${msg}`);
}