let deferredPrompt;

window.addEventListener('beforeinstallprompt', (e) => {
    e.preventDefault();
    deferredPrompt = e;
    console.log(deferredPrompt);
});

window.isPwa = () => {
    return window.matchMedia('(display-mode: fullscreen)').matches || window.matchMedia('(display-mode: standalone)').matches;
};

window.promptPWAInstall = async () => {
    if (!deferredPrompt) {
        window.location.href = '/account/manage/pwa-installation'
        console.log('kul')
        return;
    }
    deferredPrompt.prompt();
    const choiceResult = await deferredPrompt.userChoice;
    deferredPrompt = null;
};