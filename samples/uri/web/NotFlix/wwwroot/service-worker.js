// Copyright (C) Microsoft Corporation. All rights reserved.

const CACHE_NAME = '1.1';
const BASE_PATH = '/';

// Install event - cache assets
self.addEventListener('install', event => {
});

// Activate event - clean up old caches
self.addEventListener('activate', event => {
    event.waitUntil(
        caches.keys().then(keys => Promise.all(
            keys.filter(key => key !== CACHE_NAME)
                .map(key => caches.delete(key))
        ))
    );
});

async function redirectRequest(evt) {
    try {
        const formData = await evt.request.clone().formData();
        let formUrl = formData.get('url') || '';

        const protocol = "web+notflix://";
        const encodedProtocol = encodeURIComponent(protocol);
        formUrl = formUrl.replace(protocol, encodedProtocol);

        console.log('Redirecting to:', formUrl);
        return Response.redirect(`${BASE_PATH}?share=true&protocol=${formUrl}`);
    } catch (error) {
        console.log('Error in redirectRequest:', error);
        return Response.redirect(`${BASE_PATH}?share=true`);
    }
}

// Fetch event - serve from cache or network
self.addEventListener('fetch', event => {
    // All actions that include a value set get a /share-target/ request
    if (event.request.url.includes('/share-target/')) {
        event.respondWith(redirectRequest(event));

        event.waitUntil(
            (async () => {
                try {
                    const formData = await event.request.formData();

                    // Extract text data
                    const data = {
                        title: formData.get('title') || '',
                        text: formData.get('text') || '',
                        url: formData.get('url') || ''
                    };

                    // Get the client and send the data
                    const client = await self.clients.get(event.resultingClientId);
                    if (client) {
                        // Send the data using structured clone algorithm with transferables
                        client.postMessage({
                            type: 'SHARE_TARGET_DATA',
                            data: data,
                            files: null
                        }, null);

                        console.log('Sent to client:', {
                            data
                        });
                    } else {
                        console.error('No client found to send the data to');
                    }
                } catch (error) {
                    console.error('Error processing share target:', error);
                }
            })()
        );
        return;
    }

    // Handle normal requests
    event.respondWith(
        caches.match(event.request)
        .then(response => response || fetch(event.request))
    );
});
