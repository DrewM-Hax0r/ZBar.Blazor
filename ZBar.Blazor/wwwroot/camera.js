let activeVideoStreams = {};
let activeVideoRefreshIntervals = {};
let activeImageScannerContexts = {};

export function startVideoFeed(dotNetScanner, video, canvas, deviceId, autoScan, scanInterval, scannerOptions, verbose) {
    // Use the provided device, or fall back to system default
    const constraints = { video: deviceId ? { deviceId: { exact: deviceId } } : true };

    navigator.mediaDevices.getUserMedia(constraints).then(function (stream) {
        window.zbar.ZBarScanner.create().then(function (scanner) {
            let scannerContext = createScannerContext(scanner, deviceId, verbose);
            activeImageScannerContexts[video] = scannerContext;
            configureScanner(scannerContext, scannerOptions);

            if (!activeVideoStreams[video]) {
                video.srcObject = activeVideoStreams[video] = stream;
                video.addEventListener('ended', onVideoEnded);
                video.addEventListener('loadedmetadata', video.play);

                if (verbose) {
                    console.log('Video feed started (deviceId:' + deviceId + ')');
                }

                if (autoScan) {
                    enableAutoScan(dotNetScanner, video, canvas, scanInterval);
                }
            }
        });
    }).catch(function (error) {
        console.log(error);
        reject(new Error(error));
    });
}

export function setVerbosity(video, value) {
    if (activeImageScannerContexts[video]) {
        activeImageScannerContexts[video].verbose = value;
    }
}

export function scanOnce(dotNetScanner, video, canvas) {
    let canvasContext = getCanvasContext(canvas);
    scanVideoFeed(dotNetScanner, video, canvas, canvasContext);
}

export function enableAutoScan(dotNetScanner, video, canvas, scanInterval) {
    if (activeImageScannerContexts[video] && !activeVideoRefreshIntervals[video]) {
        let canvasContext = getCanvasContext(canvas);
        activeVideoRefreshIntervals[video] = setInterval(function () {
            scanVideoFeed(dotNetScanner, video, canvas, canvasContext);
        }, scanInterval);

        if (activeImageScannerContexts[video].verbose) {
            console.log('Auto Scan enabled for video feed (deviceId:' + activeImageScannerContexts[video].deviceId + ') with scan interval of ' + scanInterval + 'ms');
        }
    }
}

export function disableAutoScan(video) {
    if (activeVideoRefreshIntervals[video]) {
        clearInterval(activeVideoRefreshIntervals[video]);
        delete activeVideoRefreshIntervals[video];

        if (activeImageScannerContexts[video].verbose) {
            console.log('Auto Scan disabled for video feed (deviceId:' + activeImageScannerContexts[video].deviceId + ')');
        }
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

function scanVideoFeed(dotNetScanner, video, canvas, canvasContext) {
    let scannerContext = activeImageScannerContexts[video];
    if (scannerContext && video.videoWidth) {
        if (scannerContext.verbose) {
            console.log('Scanning video feed (deviceId:' + scannerContext.deviceId + ')');
        }

        canvas.width = video.videoWidth;
        canvas.height = video.videoHeight;

        canvasContext.drawImage(video, 0, 0, video.videoWidth, video.videoHeight);
        const imageData = canvasContext.getImageData(0, 0, video.videoWidth, video.videoHeight);
        window.zbar.scanImageData(imageData, scannerContext.scanner).then(function (symbols) {
            let results = [];
            symbols.forEach(function (symbol) {
                symbol.rawValue = symbol.decode();
                results.push(symbol);
                if (scannerContext.verbose) {
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

        let verbose = false;
        let deviceId = null;
        if (activeImageScannerContexts[video]) {
            verbose = activeImageScannerContexts[video].verbose;
            deviceId = activeImageScannerContexts[video].deviceId;

            activeImageScannerContexts[video].scanner.destroy();
            delete activeImageScannerContexts[video];
        }

        delete activeVideoStreams[video];

        if (verbose) {
            console.log('Video feed ended (deviceId:' + deviceId + ')');
        }
    }
}

function getCanvasContext(canvas) {
    return canvas.getContext('2d', { willReadFrequently: true });
}

function createScannerContext(scanner, deviceId, verbose) {
    return {
        scanner: scanner,
        deviceId: deviceId,
        verbose: verbose
    };
}

function configureScanner(scannerContext, scannerOptions) {
    scannerOptions.forEach(function (symbolOption) {
        let type = zbar.ZBarSymbolType[symbolOption.symbolType];
        symbolOption.configOptions.forEach(function (configOption) {
            setConfig(scannerContext, type, zbar.ZBarConfigType[configOption.configType], configOption.value);
        });
    });
}

function setConfig(scannerContext, type, config, value) {
    const result = scannerContext.scanner.setConfig(type, config, value);
    if (scannerContext.verbose) {
        console.log('Set ' + zbar.ZBarSymbolType[type] + ' w/ ' + zbar.ZBarConfigType[config] + ' to ' + value + ' with result ' + result);
    }
}