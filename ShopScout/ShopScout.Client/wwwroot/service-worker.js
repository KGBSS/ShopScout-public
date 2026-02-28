self.addEventListener("install", event => {
    console.log("Service worker installing...");
    self.skipWaiting();
});

self.addEventListener("activate", event => {
    console.log("Service worker activating...");
});

self.addEventListener("fetch", event => {
    event.respondWith(
        caches.open("blazor-pwa-cache").then(cache =>
            cache.match(event.request).then(response =>
                response || fetch(event.request).then(networkResponse => {
                    cache.put(event.request, networkResponse.clone());
                    return networkResponse;
                })
            )
        )
    );
});