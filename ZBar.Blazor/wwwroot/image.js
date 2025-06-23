export async function loadFromStream(dotnetImage, canvas, streamWrapper) {
    let imageData = await streamWrapper.arrayBuffer();
    let blob = new Blob([imageData]);

    let imageUrl = URL.createObjectURL(blob);
    let image = new Image();

    image.addEventListener('error', function (event) {
        dotnetImage.invokeMethodAsync('OnImageLoadFailure');
    });

    image.src = imageUrl;
    image.onload = () => {
        URL.revokeObjectURL(imageUrl);
        let context = canvas.getContext('2d');
        canvas.width = image.naturalWidth;
        canvas.height = image.naturalHeight;
        context.drawImage(image, 0, 0, image.naturalWidth, image.naturalHeight);
        dotnetImage.invokeMethodAsync('OnImageLoadSuccess');
    }
}