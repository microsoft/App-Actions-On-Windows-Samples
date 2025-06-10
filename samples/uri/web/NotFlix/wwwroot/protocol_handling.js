// Copyright (C) Microsoft Corporation. All rights reserved.

// Handle protocol handler
function handleProtocolLaunch() {
    const params = new URLSearchParams(window.location.search);
    const protocolData = params.get('protocol');

    if (protocolData) {
        console.log(`Protocol: ${protocolData}`);
    }
}

// Handle shared data
function handleSharedData(data) {
    try {
        // Display text content
        let textContent = '';
        if (data.title || data.text || data.url) {
            if (data.title) textContent += `Title: ${data.title}`;
            if (data.text) textContent += `Text: ${data.text}`;
            if (data.url) textContent += `URL: ${data.url}`;
        }
        console.log(textContent);
    } catch (error) {
        console.error('Error handling shared data:', error);
    }
}

// Initialize
document.addEventListener('DOMContentLoaded', () => {
    handleProtocolLaunch();
});

// Register service worker
if ('serviceWorker' in navigator) {
    navigator.serviceWorker.register('./service-worker.js', { updateViaCache: 'none' })
        .then(registration => console.log('ServiceWorker registered'))
        .catch(err => console.error('ServiceWorker registration failed:', err));
}

// Listen for messages from service worker
navigator.serviceWorker.addEventListener('message', event => {
    if (event.data.type === 'SHARE_TARGET_DATA') {
        console.log('Received message from service worker:', event.data);

        // Handle text data
        handleSharedData(event.data.data);

        // Cleanup on page unload
        window.addEventListener('unload', () => {
            try {
                const elements = document.querySelectorAll('img[src^="blob:"], video[src^="blob:"]');
                elements.forEach(element => {
                    if (element.src.startsWith('blob:')) {
                        URL.revokeObjectURL(element.src);
                    }
                });
            } catch (error) {
                console.error('Error cleaning up blob URLs:', error);
            }
        });
    }
});

// Register protocol handler
if ('registerProtocolHandler' in navigator) {
    try {
        navigator.registerProtocolHandler('web+notflix',
            `${window.location.origin}/?protocol=%s`,
            'PWA Protocol Handler'
        );
    } catch (err) {
        console.error('Protocol handler registration failed:', err);
    }
}
