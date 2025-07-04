import scanner from "./scanner.js";

const activeVideoStreams = {};
const activeVideoRefreshIntervals = {};
const activeImageScannerContexts = {};

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
        });
    });
}

export function startVideoFeed(dotNetScanner, video, canvas, deviceId, autoScan, scanInterval, scannerOptions, verbose) {
    // Use the provided device, or fall back to system default
    const constraints = { video: deviceId ? { deviceId: { exact: deviceId } } : true };

    navigator.mediaDevices.getUserMedia(constraints).then(function (stream) {
        scanner.createNew(video, scannerOptions, verbose).then(function () {
            let scannerContext = createScannerContext(deviceId, verbose);
            activeImageScannerContexts[video] = scannerContext;

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

export function scanOnce(dotNetScanner, video, canvas) {
    let canvasContext = getCanvasContext(canvas);
    scanVideoFeed(dotNetScanner, video, canvas, canvasContext);
}

export function enableAutoScan(dotNetScanner, video, canvas, scanInterval) {
    if (activeImageScannerContexts[video] && !activeVideoRefreshIntervals[video]) {
        setAutoScanInterval(dotNetScanner, video, canvas, scanInterval);

        if (activeImageScannerContexts[video].verbose) {
            console.log('Auto Scan enabled for video feed (deviceId:' + activeImageScannerContexts[video].deviceId + ') with scan interval of ' + scanInterval + 'ms');
        }
    }
}

export function disableAutoScan(video) {
    if (activeVideoRefreshIntervals[video]) {
        removeAutoScanInterval(video);
        if (activeImageScannerContexts[video].verbose) {
            console.log('Auto Scan disabled for video feed (deviceId:' + activeImageScannerContexts[video].deviceId + ')');
        }
    }
}

export function updateScanInterval(dotNetScanner, video, canvas, scanInterval) {
    if (activeVideoRefreshIntervals[video]) {
        removeAutoScanInterval(video);

        if (activeImageScannerContexts[video]) {
            setAutoScanInterval(dotNetScanner, video, canvas, scanInterval);

            if (activeImageScannerContexts[video].verbose) {
                console.log('Auto Scan interval updated to ' + scanInterval + 'ms for video feed (deviceId:' + activeImageScannerContexts[video].deviceId + ')');
            }
        }
    }
}

export function updateVerbosity(video, value) {
    if (activeImageScannerContexts[video]) {
        activeImageScannerContexts[video].verbose = value;
    }
}

function scanVideoFeed(dotNetScanner, video, canvas, canvasContext) {
    const scannerContext = activeImageScannerContexts[video];
    if (scannerContext && video.videoWidth) {
        if (scannerContext.verbose) {
            console.log('Scanning video feed (deviceId:' + scannerContext.deviceId + ')');
        }

        canvas.width = video.videoWidth;
        canvas.height = video.videoHeight;

        canvasContext.drawImage(video, 0, 0, video.videoWidth, video.videoHeight);
        const imageData = canvasContext.getImageData(0, 0, video.videoWidth, video.videoHeight);

        scanner.scan(video, imageData, scannerContext.verbose).then(function (results) {
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

            scanner.destroy(video);
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

function createScannerContext(deviceId, verbose) {
    return {
        deviceId: deviceId,
        verbose: verbose
    };
}

function setAutoScanInterval(dotNetScanner, video, canvas, scanInterval) {
    let canvasContext = getCanvasContext(canvas);
    activeVideoRefreshIntervals[video] = setInterval(function () {
        scanVideoFeed(dotNetScanner, video, canvas, canvasContext);
    }, scanInterval);
}

function removeAutoScanInterval(video) {
    clearInterval(activeVideoRefreshIntervals[video]);
    delete activeVideoRefreshIntervals[video];
}