
// THIS CODE IS USED TO GET THE REAR CAMERA ON PHONE DEVICES, IMPLEMENT LATER
//navigator.mediaDevices.getUserMedia({
//    audio: false,
//    video: {
//        facingMode: 'environment'
//    }
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

function startVideo(src) {
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
            //mirror image
            video.style.webkitTransform = "scaleX(-1)";
            video.style.transform = "scaleX(-1)";
        });
    }
}
function getFrame(src, dest) {
    let video = document.getElementById(src);
    let canvas = document.getElementById(dest);
    canvas.getContext('2d').drawImage(video, 0, 0, 1280, 720);
}
function getFrame(src, dest, dotNetHelper) {
    let video = document.getElementById(src);
    let canvas = document.getElementById(dest);
    canvas.getContext('2d').drawImage(video, 0, 0, 1280, 720);

    let dataUrl = canvas.toDataURL("image/png");
    dotNetHelper.invokeMethodAsync('ProcessImage', dataUrl);
}