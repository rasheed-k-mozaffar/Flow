function scrollToBottom(messagesAreaId) {
    const area = document.getElementById(messagesAreaId);
    if(area) {
        area.scrollTop = area.scrollHeight;
    }
}