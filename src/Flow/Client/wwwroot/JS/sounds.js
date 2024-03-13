const sentMessageSound = new Audio("../sounds/sent-message-sound.mp3");
const notificationSound = new Audio("../sounds/notification-sound.mp3");

function playMessageSound(isMessageSent) {
    if(isMessageSent) {
        sentMessageSound.play();
    }
    else {
        notificationSound.play();
    }
}