let activeVideoStreams = {};
let activeVideoRefreshIntervals = {};

export function startVideoFeed (video, canvas, scanInterval) {
    if (navigator.mediaDevices && navigator.mediaDevices.getUserMedia) {
        navigator.mediaDevices.getUserMedia({ video: true }).then(function (stream) {
            video.srcObject = activeVideoStreams[video] = stream;
            video.addEventListener('loadedmetadata', video.play);

            activeVideoRefreshIntervals[video] = setInterval(function () {
                const context = canvas.getContext('2d');
                context.drawImage(video, 0, 0, canvas.width, canvas.height);

                const imageData = context.getImageData(0, 0, canvas.width, canvas.height);
                window.zbar.scanImageData(imageData).then(function (symbols) {
                    console.log(symbols);
                });

            }, scanInterval);
        }).catch(function (error) {
            console.log(error);
        });
    }
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
        delete activeVideoStreams[video];
    }
}