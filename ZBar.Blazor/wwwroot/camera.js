let activeVideoStreams = {};
let activeVideoRefreshIntervals = {};
let activeImageScanners = {};

export function startVideoFeed(dotNetScanner, video, canvas, deviceId, autoScan, scanInterval, scannerOptions, verbose) {
    // Use the provided device, or fall back to system default
    const constraints = { video: deviceId ? { deviceId: { exact: deviceId } } : true };

    navigator.mediaDevices.getUserMedia(constraints).then(function (stream) {
        window.zbar.ZBarScanner.create().then(function (scanner) {
            activeImageScanners[video] = scanner;
            configureScanner(scanner, scannerOptions, verbose);

            if (!activeVideoStreams[video]) {
                video.srcObject = activeVideoStreams[video] = stream;
                video.addEventListener('ended', onVideoEnded);
                video.addEventListener('loadedmetadata', video.play);

                if (autoScan) {
                    enableAutoScan(dotNetScanner, video, canvas, scanInterval, verbose);
                }
            }
        });
    }).catch(function (error) {
        console.log(error);
    });
}

export function enableAutoScan(dotNetScanner, video, canvas, scanInterval, verbose) {
    if (activeImageScanners[video] && !activeVideoRefreshIntervals[video]) {
        let context = canvas.getContext('2d', { willReadFrequently: true });
        activeVideoRefreshIntervals[video] = setInterval(function () {
            scanVideoFeed(dotNetScanner, video, canvas, context, verbose);
        }, scanInterval);
    }
}

export function disableAutoScan(video) {
    if (activeVideoRefreshIntervals[video]) {
        clearInterval(activeVideoRefreshIntervals[video]);
        delete activeVideoRefreshIntervals[video];
    }
}

export function endVideoFeed(video) {
    if (activeVideoStreams[video]) {
        let stream = video.srcObject;
        let tracks = stream.getTracks();

        tracks.forEach(function (track) {
            track.stop();
        });

        releaseVideoResources(video);
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

function scanVideoFeed(dotNetScanner, video, canvas, context, verbose) {
    let scanner = activeImageScanners[video];
    if (scanner && video.videoWidth) {
        canvas.width = video.videoWidth;
        canvas.height = video.videoHeight;
        context.drawImage(video, 0, 0, video.videoWidth, video.videoHeight);

        const imageData = context.getImageData(0, 0, video.videoWidth, video.videoHeight);
        window.zbar.scanImageData(imageData, scanner).then(function (symbols) {
            let results = [];
            symbols.forEach(function (symbol) {
                symbol.rawValue = symbol.decode();
                results.push(symbol);
                if (verbose) {
                    console.log(symbol);
                }
            });
            dotNetScanner.invokeMethodAsync('OnAfterScan', results);
        });
    }
}

function onVideoEnded(event) {
    releaseVideoResources(event.target);
}

function releaseVideoResources(video) {
    if (activeVideoStreams[video]) {
        video.srcObject = null;
        video.removeEventListener('ended', onVideoEnded);
        video.removeEventListener('loadedmetadata', video.play);

        disableAutoScan(video);

        if (activeImageScanners[video]) {
            activeImageScanners[video].destroy();
            delete activeImageScanners[video];
        }

        delete activeVideoStreams[video];
    }
}

function configureScanner(scanner, scannerOptions, verbose) {
    scannerOptions.forEach(function (symbolOption) {
        let type = zbar.ZBarSymbolType[symbolOption.symbolType];
        symbolOption.configOptions.forEach(function (configOption) {
            setConfig(scanner, type, zbar.ZBarConfigType[configOption.configType], configOption.value, verbose);
        });
    });
}

function setConfig(scanner, type, config, value, verbose) {
    const result = scanner.setConfig(type, config, value);
    if (verbose) {
        console.log('Set ' + zbar.ZBarSymbolType[type] + ' w/ ' + zbar.ZBarConfigType[config] + ' to ' + value + ' with result ' + result);
    }
}