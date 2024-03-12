function scrollToBottom(messagesAreaId) {
    const messagesArea = document.getElementById(messagesAreaId);

    if(messagesArea) {
        messagesArea.scrollTop = messagesArea.scrollHeight;
    }
}