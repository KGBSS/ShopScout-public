let _dotNetRef;
let listenersAdded = false;
let handler;
let isThrottled = false;
let isProcessing = false;
let hasMoreProducts = true; // Add a flag to track if we should keep asking
let el;

export function onScroll(elementId, dotNetRef) {
    if (listenersAdded) return;
    listenersAdded = true;
    el = document.getElementById(elementId);
    if (!el) return;
    _dotNetRef = dotNetRef;

    // Reset the flag whenever we initialize the listener
    hasMoreProducts = true;

    const handleScroll = () => {
        if (!isThrottled) {
            window.requestAnimationFrame(() => {
                const atBottom = el.scrollTop + el.clientHeight >= el.scrollHeight - 1;

                if (atBottom && !isProcessing && hasMoreProducts) {
                    isProcessing = true;

                    _dotNetRef.invokeMethodAsync('OnScrollBottom').then((hasMore) => {
                        hasMoreProducts = hasMore;
                        isProcessing = false;
                    });
                }
                isThrottled = false;
            });
            isThrottled = true;
        }
    }
    handler = handleScroll;
    el.addEventListener('scroll', handleScroll);
}

export function offScroll() {
    if (handler) {
        el.removeEventListener('scroll', handler);
        listenersAdded = false;
        _dotNetRef = null;
        handler = null;
        isThrottled = false;
        isProcessing = false;
        hasMoreProducts = true; // Reset cleanup
        el = null;
    }
}