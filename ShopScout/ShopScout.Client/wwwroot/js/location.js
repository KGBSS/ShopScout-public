export function getLocation() {
    return new Promise((resolve, reject) => {
        if (!navigator.geolocation) {
            reject(new Error('Geolocation not supported'));
            return;
        }

        navigator.geolocation.getCurrentPosition(
            position => {
                resolve({
                    latitude: position.coords.latitude,
                    longitude: position.coords.longitude,
                    accuracy: position.coords.accuracy
                });
            },
            error => {
                let errorMsg = 'Unknown error';
                switch (error.code) {
                    case error.PERMISSION_DENIED:
                        errorMsg = 'User denied location access';
                        break;
                    case error.POSITION_UNAVAILABLE:
                        errorMsg = 'Location unavailable';
                        break;
                    case error.TIMEOUT:
                        errorMsg = 'Location request timed out';
                        break;
                }
                reject(new Error(errorMsg));
            },
            {
                enableHighAccuracy: true,
                timeout: 10000,
                maximumAge: 0
            }
        );
    });
};