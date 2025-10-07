// SERVICE WORKER for PWA - Simple Implementation for Learning
// This service worker provides offline functionality and caching for Blazor app

// STEP 1: Import Blazor assets (auto-generated file with list of all app files)
self.importScripts('./service-worker-assets.js');

// STEP 2: Define cache settings
const CACHE_NAME = 'malevolent-shrine-cache-v1';
const OFFLINE_URL = '/offline.html';

// Cache name helpers from Blazor
const cacheNamePrefix = 'offline-cache-';
const cacheName = `${cacheNamePrefix}${self.assetsManifest.version}`;
const offlineAssetsInclude = [ /\.dll$/, /\.pdb$/, /\.wasm/, /\.html/, /\.js$/, /\.json$/, /\.css$/, /\.woff$/, /\.png$/, /\.jpe?g$/, /\.gif$/, /\.ico$/, /\.blat$/, /\.dat$/ ];
const offlineAssetsExclude = [ /^service-worker\.js$/ ];

// STEP 3: INSTALL EVENT - Download and cache all app files
self.addEventListener('install', event => event.waitUntil(onInstall(event)));

async function onInstall(event) {
    console.log('[Service Worker] Installing and caching assets...');

    // Cache the offline page first
    const offlineCache = await caches.open(CACHE_NAME);
    await offlineCache.add(new Request(OFFLINE_URL, { cache: 'reload' }));

    // Cache all Blazor app files from manifest
    const assetsRequests = self.assetsManifest.assets
        .filter(asset => offlineAssetsInclude.some(pattern => pattern.test(asset.url)))
        .filter(asset => !offlineAssetsExclude.some(pattern => pattern.test(asset.url)))
        .map(asset => new Request(asset.url, { integrity: asset.hash, cache: 'no-cache' }));
    
    await caches.open(cacheName).then(cache => cache.addAll(assetsRequests));
    
    console.log('[Service Worker] Install complete!');
}

// STEP 4: ACTIVATE EVENT - Clean up old caches
self.addEventListener('activate', event => event.waitUntil(onActivate(event)));

async function onActivate(event) {
    console.log('[Service Worker] Activating...');

    // Delete old cache versions
    const cacheKeys = await caches.keys();
    await Promise.all(cacheKeys
        .filter(key => key.startsWith(cacheNamePrefix) && key !== cacheName)
        .map(key => {
            console.log('[Service Worker] Deleting old cache:', key);
            return caches.delete(key);
        }));
    
    console.log('[Service Worker] Activation complete!');
}

// STEP 5: FETCH EVENT - Serve cached files or fetch from network
self.addEventListener('fetch', event => event.respondWith(onFetch(event)));

async function onFetch(event) {
    const request = event.request;
    
    // Only handle GET requests
    if (request.method !== 'GET') {
        return fetch(request);
    }

    // STRATEGY 1: For page navigation - Network First, then Cache, then Offline page
    if (request.mode === 'navigate') {
        try {
            const response = await fetch(request);
            return response;
        } catch (error) {
            console.log('[Service Worker] Network failed, trying cache...');
            
            const cachedResponse = await caches.match(request);
            if (cachedResponse) {
                return cachedResponse;
            }
            
            // If nothing in cache, show offline page
            return caches.match(OFFLINE_URL);
        }
    }

    // STRATEGY 2: For assets (JS, CSS, images) - Cache First, then Network
    const cachedResponse = await caches.match(request);
    if (cachedResponse) {
        return cachedResponse;
    }

    // Not in cache - fetch from network
    try {
        const response = await fetch(request);
        
        // Cache successful responses for next time
        if (response && response.status === 200) {
            const cache = await caches.open(cacheName);
            cache.put(request, response.clone());
        }
        
        return response;
    } catch (error) {
        console.log('[Service Worker] Fetch failed:', error);
        
        // Return a simple offline response
        return new Response('Offline - Content not available', {
            status: 503,
            statusText: 'Service Unavailable',
            headers: new Headers({
                'Content-Type': 'text/plain'
            })
        });
    }
}

// STEP 6: MESSAGE HANDLER - For commands from the app
self.addEventListener('message', event => {
    // Force update to new service worker
    if (event.data && event.data.type === 'SKIP_WAITING') {
        self.skipWaiting();
    }
});
