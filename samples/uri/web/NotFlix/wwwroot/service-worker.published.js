// Caution! Be sure you understand the caveats before publishing an application with
// offline support. See https://aka.ms/blazor-offline-considerations

const CACHE_VERSION = 1.1;
const BASE_PATH = '/';

self.importScripts('./service-worker-assets.js');
self.addEventListener('install', event => event.waitUntil(onInstall(event)));
self.addEventListener('activate', event => event.waitUntil(onActivate(event)));
self.addEventListener('fetch', event => onFetch(event));

const cacheNamePrefix = 'offline-cache-';
const cacheName = `${cacheNamePrefix}${self.assetsManifest.version}`;
const offlineAssetsInclude = [ /\.dll$/, /\.pdb$/, /\.wasm/, /\.html/, /\.js$/, /\.json$/, /\.css$/, /\.woff$/, /\.png$/, /\.jpe?g$/, /\.gif$/, /\.ico$/, /\.blat$/, /\.dat$/ ];
const offlineAssetsExclude = [ /^service-worker\.js$/ ];

// Replace with your base path if you are hosting on a subfolder. Ensure there is a trailing '/'.
const base = "/";
const baseUrl = new URL(base, self.origin);
const manifestUrlList = self.assetsManifest.assets.map(asset => new URL(asset.url, baseUrl).href);

async function onInstall(event) {
    console.info('Service worker: Install');

    self.skipWaiting();

    // Fetch and cache all matching items from the assets manifest
    const assetsRequests = self.assetsManifest.assets
        .filter(asset => offlineAssetsInclude.some(pattern => pattern.test(asset.url)))
        .filter(asset => !offlineAssetsExclude.some(pattern => pattern.test(asset.url)))
        .map(asset => new Request(asset.url, { integrity: asset.hash, cache: 'no-cache' }));
    await caches.open(cacheName).then(cache => cache.addAll(assetsRequests));
}

async function onActivate(event) {
    console.info('Service worker: Activate');

    // Delete unused caches
    const cacheKeys = await caches.keys();
    await Promise.all(cacheKeys
        .filter(key => key.startsWith(cacheNamePrefix) && key !== cacheName)
        .map(key => caches.delete(key)));
}

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

async function onFetch(event) {

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

    let cachedResponse = null;
    if (event.request.method === 'GET') {
        // For all navigation requests, try to serve index.html from cache,
        // unless that request is for an offline resource.
        // If you need some URLs to be server-rendered, edit the following check to exclude those URLs
        const shouldServeIndexHtml = event.request.mode === 'navigate'
            && !manifestUrlList.some(url => url === event.request.url);

        const request = shouldServeIndexHtml ? 'index.html' : event.request;
        const cache = await caches.open(cacheName);
        cachedResponse = await cache.match(request);
    }

    return event.respondWith(cachedResponse || fetch(event.request));
}
