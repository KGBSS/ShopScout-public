window.BarcodeScannerModule = (function () {
    let isFlashlightOn = false;

    function initializeElements() {
        const flashlightBtn = document.querySelector('.btn-flashlight');
        const flashlightIcon = document.querySelector('.flashlight-icon');
        const dropdown = document.getElementById('cameraDropdown');

        if (!dropdown) return false;

        const dropdownTrigger = dropdown.querySelector('.dropdown-trigger');
        const dropdownContent = dropdown.querySelector('.dropdown-content');
        const dropdownBackdrop = dropdown.querySelector('.dropdown-backdrop');
        const dropdownText = dropdown.querySelector('.dropdown-text');

        return {
            flashlightBtn,
            flashlightIcon,
            dropdown,
            dropdownTrigger,
            dropdownContent,
            dropdownBackdrop,
            dropdownText
        };
    }

    function toggleDropdown() {
        const elements = initializeElements();
        if (!elements) return;

        const isOpen = elements.dropdownTrigger.classList.contains('open');
        if (isOpen) {
            closeDropdown();
        } else {
            openDropdown();
        }
    }

    function openDropdown() {
        const elements = initializeElements();
        if (!elements) return;

        elements.dropdownTrigger.classList.add('open');
        elements.dropdownContent.classList.add('open');
        elements.dropdownBackdrop.classList.add('open');

        document.addEventListener('click', handleClickOutside);
    }

    function closeDropdown() {
        const elements = initializeElements();
        if (!elements) return;

        elements.dropdownTrigger.classList.remove('open');
        elements.dropdownContent.classList.remove('open');
        elements.dropdownBackdrop.classList.remove('open');

        document.removeEventListener('click', handleClickOutside);
    }

    function handleClickOutside(event) {
        const elements = initializeElements();
        if (!elements) return;

        if (!elements.dropdown.contains(event.target)) {
            closeDropdown();
        }
    }

    function selectCamera(e) {
        const elements = initializeElements();
        if (!elements) return;

        let displayName = e.target.innerHTML;
        let value = e.target.value;

        elements.dropdownText.textContent = displayName;

        elements.dropdown.querySelectorAll('.dropdown-content > div').forEach(option => {
            option.classList.remove('selected');
            if (option.value === value) {
                option.classList.add('selected');
            }
        });

        if (isFlashlightOn) handleFlashlightToggle();

        closeDropdown();
    }

    function handleFlashlightToggle() {
        const elements = initializeElements();
        if (!elements) return;

        isFlashlightOn = !isFlashlightOn;

        if (isFlashlightOn) {
            elements.flashlightBtn.classList.add('active');
            elements.flashlightIcon.innerHTML = `
                <path d="M18 6c0 2-2 2-2 4v10a2 2 0 0 1-2 2h-4a2 2 0 0 1-2-2V10c0-2-2-2-2-4V2h12z"/>
                <line x1="6" x2="18" y1="6" y2="6"/>
                <line x1="12" x2="12" y1="12" y2="12"/>
            `;
        } else {
            elements.flashlightBtn.classList.remove('active');
            elements.flashlightIcon.innerHTML = `
                <path d="M18 6c0 2-2 2-2 4v10a2 2 0 0 1-2 2h-4a2 2 0 0 1-2-2V10c0-2-2-2-2-4V2h12z"/>
                <line x1="6" x2="18" y1="6" y2="6"/>
                <line x1="12" x2="12" y1="12" y2="12"/>
                <line x1="2" x2="22" y1="2" y2="22"/>
            `;
        }

        // Hooked into the refactored core logic via the global window object
        if (typeof window.toggleFlashlight === 'function') {
            window.toggleFlashlight(isFlashlightOn);
        }
    }

    return {
        toggleDropdown,
        closeDropdown,
        selectCamera,
        handleFlashlightToggle
    };
})();

// Make functions globally available for the DOM onclick events
window.toggleDropdown = window.BarcodeScannerModule.toggleDropdown;
window.closeDropdown = window.BarcodeScannerModule.closeDropdown;
window.selectCamera = window.BarcodeScannerModule.selectCamera;
window.handleFlashlightToggle = window.BarcodeScannerModule.handleFlashlightToggle;