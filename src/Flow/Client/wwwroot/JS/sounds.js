const sentMessageSound = new Audio("../sounds/sent-message-sound.mp3");
const notificationSound = new Audio("../sounds/notification-sound.mp3");
const requestArrivedSound = new Audio("../sounds/request-arrived-sound.mp3");

function playMessageSound(isMessageSent) {
    if(isMessageSent) {
        playAndRelease(sentMessageSound);
    }
    else {
        playAndRelease(notificationSound)
    }
}

function playRequestSound() {
    playAndRelease(requestArrivedSound);
}

function playAndRelease(audio) {
    audio.play().onended.then(() => {
        // Release the resources associated with the Audio object
        audio.pause();
        audio.currentTime = 0;
    });
}