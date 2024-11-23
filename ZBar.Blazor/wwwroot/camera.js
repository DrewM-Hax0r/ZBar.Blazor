let activeVideoStreams = {};
let activeVideoRefreshIntervals = {};
let activeImageScanners = {};

export function startVideoFeed(video, canvas, deviceId, scanInterval) {
    // Use the provided device, or fall back to system default
    const constraints = { video: deviceId ? { deviceId: { exact: deviceId } } : true };

    navigator.mediaDevices.getUserMedia(constraints).then(function (stream) {
        window.zbar.ZBarScanner.create().then(function (scanner) {
            activeImageScanners[video] = scanner;

            // scanner config example
            scanner.setConfig('ZBAR_UPCA', 'ZBAR_CFG_ENABLE', 1);
            //scanner.setConfig('ZBAR_CFG_ASCII', 'ZBAR_CFG_ENABLE', 1);

            scanner.setConfig('ZBAR_QRCODE', 'ZBAR_CFG_ENABLE', 0);
            scanner.setConfig('ZBAR_EAN8', 'ZBAR_CFG_ENABLE', 0);
            scanner.setConfig('ZBAR_UPCE', 'ZBAR_CFG_ENABLE', 0);
            scanner.setConfig('ZBAR_ISBN10', 'ZBAR_CFG_ENABLE', 0);
            scanner.setConfig('ZBAR_EAN13', 'ZBAR_CFG_ENABLE', 0);
            scanner.setConfig('ZBAR_ISBN13', 'ZBAR_CFG_ENABLE', 0);
            scanner.setConfig('ZBAR_I25', 'ZBAR_CFG_ENABLE', 0);
            scanner.setConfig('ZBAR_CODE39', 'ZBAR_CFG_ENABLE', 0);
            scanner.setConfig('ZBAR_PDF417', 'ZBAR_CFG_ENABLE', 0);
            scanner.setConfig('ZBAR_CODE128', 'ZBAR_CFG_ENABLE', 0);

            if (!activeVideoStreams[video]) {
                video.srcObject = activeVideoStreams[video] = stream;
                video.addEventListener('loadedmetadata', video.play);

                let context = canvas.getContext('2d', { willReadFrequently: true });
                activeVideoRefreshIntervals[video] = setInterval(function () {
                    context.drawImage(video, 0, 0, canvas.width, canvas.height);

                    const imageData = context.getImageData(0, 0, canvas.width, canvas.height);
                    window.zbar.scanImageData(imageData).then(function (symbols) {
                        symbols.forEach(function (symbol) {
                            symbol.rawValue = symbol.decode();
                            console.log(symbol);
                        });
                    });

                }, scanInterval);
            }

        });


    }).catch(function (error) {
        console.log(error);
    });
}

export function endVideoFeed(video) {
    if (activeVideoStreams[video]) {
        let stream = video.srcObject;
        let tracks = stream.getTracks();

        tracks.forEach(function (track) {
            track.stop();
        });
        video.srcObject = null;
        video.removeEventListener('loadedmetadata', video.play);

        clearInterval(activeVideoRefreshIntervals[video]);
        delete activeVideoRefreshIntervals[video];
        
        if (activeImageScanners[video]) {
            activeImageScanners[video].destroy();
            delete activeImageScanners[video];
        }

        delete activeVideoStreams[video];
    }
}

export function getAvailableCameras() {
    return new Promise(function (resolve, reject) {
        // Request access to video devices so that all device info is available
        // See: https://developer.mozilla.org/en-US/docs/Web/API/MediaDeviceInfo
        navigator.mediaDevices.getUserMedia({ video: true }).then(function () {
            navigator.mediaDevices.enumerateDevices().then((devices) => {
                const cameras = devices.filter(function (device) { return device.kind === 'videoinput' });
                resolve(cameras.map(function (camera) {
                    return { id: camera.deviceId, name: camera.label };
                }));
            });
        }).catch(function (error) {
            console.log(error);
            reject(new Error(error));
        });
    });
}