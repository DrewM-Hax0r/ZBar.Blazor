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
            const canvasContext = canvas.getContext('2d');
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
        const canvasContext = canvas.getContext('2d');
        const imageData = canvasContext.getImageData(0, 0, image.naturalWidth, image.naturalHeight);
        scanner.scan(canvas, imageData, scannerContext.verbose).then(function (results) {
            dotNetScanner.invokeMethodAsync('OnAfterScan', results);
        });
    }
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