
// THIS CODE IS USED TO GET THE REAR CAMERA ON PHONE DEVICES, IMPLEMENT LATER
//navigator.mediaDevices.getUserMedia({
//    audio: false,
    //video: {
    //    facingMode: 'environment'
    //}
//})
//    .then(stream => vid.srcObject = stream)
//    .catch(console.error);

//THIS CODE IS USED TO GET ALL THE AVAILABLE CAMERAS
// enumerate devices and select the first camera (mostly the back one)
//navigator.mediaDevices.enumerateDevices().then(function (devices) {
//    for (var i = 0; i !== devices.length; ++i) {
//        if (devices[i].kind === 'videoinput') {
//            console.log('Camera found: ', devices[i].label || 'label not found', devices[i].deviceId || 'id no found');
//            videoConstraints.deviceId = { exact: devices[i].deviceId }
//        }
//    }
//});

//<----------------------------------------------------->
let width=0;
let height=0;
function startVideo(src, RearCamera) {
    if (RearCamera == true) {
        if (navigator.mediaDevices && navigator.mediaDevices.getUserMedia) {
            navigator.mediaDevices.getUserMedia({
                video: {
                    facingMode: 'environment'
                }
}).then(function (stream) {
                let video = document.getElementById(src);
                if ("srcObject" in video) {
                    video.srcObject = stream;
                } else {
                    video.src = window.URL.createObjectURL(stream);
                }
                video.onloadedmetadata = function (e) {
                    video.play();
                };
                const track = stream.getVideoTracks()[0];
                const settings = track.getSettings();

                // Extract width and height from the stream settings
                width = settings.width;
                height = settings.height;
                //mirror image
                video.style.webkitTransform = "scaleX(-1)";
    video.style.transform = "scaleX(-1)";
            });
        }
    }
    else {
        if (navigator.mediaDevices && navigator.mediaDevices.getUserMedia) {
            navigator.mediaDevices.getUserMedia({ video: true }).then(function (stream) {
                let video = document.getElementById(src);
                if ("srcObject" in video) {
                    video.srcObject = stream;
                } else {
                    video.src = window.URL.createObjectURL(stream);
                }
                video.onloadedmetadata = function (e) {
                    video.play();
                };
                const track = stream.getVideoTracks()[0];
                const settings = track.getSettings();

                // Extract width and height from the stream settings
                width = settings.width;
                height = settings.height;
                console.log("width "+width);
                console.log("height "+height);
                //mirror image
                video.style.webkitTransform = "scaleX(-1)";
                video.style.transform = "scaleX(-1)";
            });
        }
    }
}
function getFrame(src, dest) {
    let video = document.getElementById(src);
    let canvas = document.getElementById(dest);
    canvas.getContext('2d').drawImage(video, 0, 0, width, height);
    // Stop the webcam stream
    const stream = video.srcObject;

}
function getFrame(src, dest, dotNetHelper) {
    let video = document.getElementById(src);
    let canvas = document.getElementById(dest);
    canvas.width = width;
    canvas.height = height;
    canvas.getContext('2d').drawImage(video, 0, 0, width, height);
    CloseStream(src, dest);
    let dataUrl = canvas.toDataURL("image/png");
    dotNetHelper.invokeMethodAsync('ProcessImage', dataUrl);

}
function CloseStream(src, dest) {
    let video = document.getElementById(src);
    let canvas = document.getElementById(dest);
    if (video.srcObject != null) {
        const stream = video.srcObject;
        if (stream) {
            const tracks = stream.getTracks();
            tracks.forEach(track => track.stop());
            video.srcObject = null;
        }
    }
}