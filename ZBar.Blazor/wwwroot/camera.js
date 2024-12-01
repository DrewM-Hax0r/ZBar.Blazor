let activeVideoStreams = {};
let activeVideoRefreshIntervals = {};
let activeImageScanners = {};

export function startVideoFeed(video, canvas, deviceId, scanInterval, scannerOptions) {
    // Use the provided device, or fall back to system default
    const constraints = { video: deviceId ? { deviceId: { exact: deviceId } } : true };

    navigator.mediaDevices.getUserMedia(constraints).then(function (stream) {
        window.zbar.ZBarScanner.create().then(function (scanner) {
            activeImageScanners[video] = scanner;
            configureScanner(scanner, scannerOptions);

            if (!activeVideoStreams[video]) {
                video.srcObject = activeVideoStreams[video] = stream;
                video.addEventListener('suspend', releaseVideoResources);
                video.addEventListener('loadedmetadata', video.play);

                let context = canvas.getContext('2d', { willReadFrequently: true });
                activeVideoRefreshIntervals[video] = setInterval(function () {
                    context.drawImage(video, 0, 0, canvas.width, canvas.height);

                    const imageData = context.getImageData(0, 0, canvas.width, canvas.height);
                    window.zbar.scanImageData(imageData, scanner).then(function (symbols) {
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

function releaseVideoResources(event) {
    const video = event.target;
    video.srcObject = null;
    video.removeEventListener('suspend', releaseVideoResources);
    video.removeEventListener('loadedmetadata', video.play);

    clearInterval(activeVideoRefreshIntervals[video]);
    delete activeVideoRefreshIntervals[video];

    if (activeImageScanners[video]) {
        activeImageScanners[video].destroy();
        delete activeImageScanners[video];
    }

    delete activeVideoStreams[video];
}

function configureScanner(scanner, scannerOptions) {
    scannerOptions.forEach(function (symbolOption) {
        let type = zbar.ZBarSymbolType[symbolOption.symbolType];
        symbolOption.configOptions.forEach(function (configOption) {
            setConfigWithLogging(scanner, type, zbar.ZBarConfigType[configOption.configType], configOption.value);
        });

        // Defaults
        //setConfigWithLogging(scanner, type, zbar.ZBarConfigType['ZBAR_CFG_ADD_CHECK'], 1);
        //setConfigWithLogging(scanner, type, zbar.ZBarConfigType['ZBAR_CFG_EMIT_CHECK'], 1);
    });

    // Global Defaults
    //setConfigWithLogging(scanner, zbar.ZBarSymbolType['ZBAR_NONE'], zbar.ZBarConfigType['ZBAR_CFG_X_DENSITY'], 0); // skip 0 pixles
    //setConfigWithLogging(scanner, zbar.ZBarSymbolType['ZBAR_NONE'], zbar.ZBarConfigType['ZBAR_CFG_Y_DENSITY'], 0); // skip 0 pixles
}

function setConfigWithLogging(scanner, type, config, value) {
    const result = scanner.setConfig(type, config, value);
    console.log('Set ' + zbar.ZBarSymbolType[type] + ' w/ ' + zbar.ZBarConfigType[config] + ' to ' + value + ' with result ' + result);
}

// For temporary reference

// ZBAR_CFG_ENABLE
// ZBAR_CFG_MIN_LEN
// ZBAR_CFG_MAX_LEN
// ZBAR_CFG_ASCII
// ZBAR_CFG_UNCERTAINTY

// ZBAR_CFG_ADD_CHECK
// ZBAR_CFG_EMIT_CHECK
// ZBAR_CFG_BINARY
// ZBAR_CFG_NUM
// ZBAR_CFG_POSITION
// ZBAR_CFG_X_DENSITY
// ZBAR_CFG_Y_DENSITY

// "ZBAR_NONE"
// "ZBAR_PARTIAL"

// "ZBAR_EAN2"
// "ZBAR_EAN5"
// "ZBAR_EAN8"
// "ZBAR_EAN13"
// "ZBAR_UPCE"
// "ZBAR_UPCA"
// "ZBAR_ISBN10"
// "ZBAR_ISBN13"
// "ZBAR_COMPOSITE"
// "ZBAR_I25"
// "ZBAR_DATABAR"
// "ZBAR_DATABAR_EXP"
// "ZBAR_CODABAR"
// "ZBAR_CODE39"
// "ZBAR_PDF417"
// "ZBAR_QRCODE"
// "ZBAR_SQCODE"
// "ZBAR_CODE93"
// "ZBAR_CODE128"

// "ZBAR_SYMBOL"
// "ZBAR_ADDON2"
// "ZBAR_ADDON5"
// "ZBAR_ADDON"