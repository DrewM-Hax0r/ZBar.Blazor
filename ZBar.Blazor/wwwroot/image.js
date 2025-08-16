import scanner from "./scanner.js";

const activeImageScannerContexts = {};

export async function loadFromStream(dotnetImage, streamWrapper, canvas, scannerOptions, verbose) {
    let imageData = await streamWrapper.arrayBuffer();
    let blob = new Blob([imageData]);

    let imageUrl = URL.createObjectURL(blob);
    let image = new Image();

    image.addEventListener('error', function () {
        dotnetImage.invokeMethodAsync('OnImageLoadFailure');
    });

    image.src = imageUrl;
    image.onload = () => {
        URL.revokeObjectURL(imageUrl);
        updateScannerContext(canvas, image, scannerOptions, verbose).then(function () {
            const canvasContext = getCanvasContext(canvas);
            canvas.width = image.naturalWidth;
            canvas.height = image.naturalHeight;
            canvasContext.drawImage(image, 0, 0, image.naturalWidth, image.naturalHeight);
            dotnetImage.invokeMethodAsync('OnImageLoadSuccess');
        });
    }
}

export async function loadFromUrl(dotnetImage, imageUrl, canvas, scannerOptions, verbose) {
    let image = new Image();

    image.addEventListener('error', function () {
        dotnetImage.invokeMethodAsync('OnImageLoadFailure');
    });

    image.src = imageUrl;
    image.onload = () => {
        updateScannerContext(canvas, image, scannerOptions, verbose).then(function () {
            const canvasContext = getCanvasContext(canvas);
            canvas.width = image.naturalWidth;
            canvas.height = image.naturalHeight;
            canvasContext.drawImage(image, 0, 0, image.naturalWidth, image.naturalHeight);
            dotnetImage.invokeMethodAsync('OnImageLoadSuccess');
        });
    }
}

export function scanImage(dotNetScanner, canvas) {
    const scannerContext = activeImageScannerContexts[canvas];
    if (scannerContext && scannerContext.image) {
        const canvasContext = getCanvasContext(canvas);
        const imageData = canvasContext.getImageData(0, 0, scannerContext.image.naturalWidth, scannerContext.image.naturalHeight);
        scanner.scan(canvas, imageData, scannerContext.verbose).then(function (results) {
            dotNetScanner.invokeMethodAsync('OnAfterScan', results);
        });
    }
}

export function clearCanvas(canvas) {
    const canvasContext = getCanvasContext(canvas);
    canvasContext.clearRect(0, 0, canvas.width, canvas.height);
}

export function updateVerbosity(canvas, value) {
    if (activeImageScannerContexts[canvas]) {
        activeImageScannerContexts[canvas].verbose = value;
    }
}

function getCanvasContext(canvas) {
    return canvas.getContext('2d', { willReadFrequently: true });
}

function updateScannerContext(canvas, image, scannerOptions, verbose) {
    const scannerContext = activeImageScannerContexts[canvas];
    if (!scannerContext) {
        return scanner.createNew(canvas, scannerOptions, verbose).then(function () {
            let context = activeImageScannerContexts[canvas] = {
                image: image,
                verbose: verbose
            };

            if (verbose) {
                console.log('Image scanner created');
            }

            return context;
        });
    } else {
        scannerContext.image = image;
        return Promise.resolve(scannerContext);
    }
}