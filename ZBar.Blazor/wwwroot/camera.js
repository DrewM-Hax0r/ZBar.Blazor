if (!window.zBarBlazor) { window.zBarBlazor = {}; }
window.zBarBlazor.camera = {};

window.zBarBlazor.camera.startVideoFeed = function (video) {
    if (navigator.mediaDevices && navigator.mediaDevices.getUserMedia) {
        navigator.mediaDevices.getUserMedia({ video: true }).then(function (stream) {
            if ("srcObject" in video) {
                video.srcObject = stream;
            } else {
                video.src = window.URL.createObjectURL(stream);
            }
            video.onloadedmetadata = function (e) {
                video.play();
            };
            //mirror image
            //video.style.webkitTransform = "scaleX(-1)";
            //video.style.transform = "scaleX(-1)";
        });
    }
};

window.zBarBlazor.camera.endVideoFeed = function (video) {

};

window.zBarBlazor.camera.CaptureImage = function (video, canvas, width, height) {
    canvas.getContext('2d').drawImage(video, 0, 0, width, height);
};