function scrollToBottom(messagesAreaId) {
    const messagesArea = document.getElementById(messagesAreaId);

    if (messagesArea) {
        messagesArea.scrollTop = messagesArea.scrollHeight;
    }
}

window.addScrollListener = async (dotnetHelper, messagesAreaId) => {
    const messagesArea = document.getElementById(messagesAreaId);
    const verticalScrollPosition = messagesArea.scrollHeight;
    let isLoading = false; // Flag to track loading state

    // Remove any existing scroll event listeners
    messagesArea.removeEventListener('scroll', handleScroll);

    // Add the new scroll event listener
    messagesArea.addEventListener('scroll', handleScroll);

    async function handleScroll() {
        if (messagesArea.scrollTop === 0 && !isLoading) {
            isLoading = true; // Set loading flag
            try {
                await dotnetHelper.invokeMethodAsync('HandleLoadingPreviousMessagesAsync');
            } catch (error) {
                console.error('Error loading previous messages:', error);
            } finally {
                isLoading = false; // Reset loading flag
            }
        }
    }

    // Scroll to the last message after loading completes
    window.scrollToLastMessage = () => {
        messagesArea.scrollTop = verticalScrollPosition;
    };
};