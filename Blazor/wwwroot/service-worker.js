// In development, provide simple offline support so interns can test easily.
// This caches a few core files (including offline.html) and serves offline.html
// on navigation when the network is unavailable.

const DEV_CACHE = 'ms-dev-cache-v1';
const OFFLINE_URL = 'offline.html';
const PRECACHE = [
  OFFLINE_URL,
  'index.html',
  'manifest.webmanifest',
  'icon-192.png',
  'icon-512.png',
  'css/app.css',
  'lib/bootstrap/dist/css/bootstrap.min.css'
];

self.addEventListener('install', event => {
  event.waitUntil(
    caches.open(DEV_CACHE)
      .then(cache => cache.addAll(PRECACHE))
      .then(() => self.skipWaiting())
  );
});

self.addEventListener('activate', event => {
  event.waitUntil(
    (async () => {
      const keys = await caches.keys();
      await Promise.all(keys.filter(k => k.startsWith('ms-dev-cache-') && k !== DEV_CACHE).map(k => caches.delete(k)));
      await self.clients.claim();
    })()
  );
});

self.addEventListener('fetch', event => {
  const req = event.request;

  // Only handle GET
  if (req.method !== 'GET') return;

  // For navigations: try network first, fall back to cache, then offline page
  if (req.mode === 'navigate') {
    event.respondWith(
      (async () => {
        try {
          const net = await fetch(req);
          return net;
        } catch {
          const cached = await caches.match(req);
          return cached || caches.match(OFFLINE_URL);
        }
      })()
    );
    return;
  }

  // For assets: cache-first, then network; cache successful responses
  event.respondWith(
    (async () => {
      const cached = await caches.match(req);
      if (cached) return cached;
      try {
        const resp = await fetch(req);
        if (resp && resp.status === 200) {
          const cache = await caches.open(DEV_CACHE);
          cache.put(req, resp.clone());
        }
        return resp;
      } catch {
        // As a last resort, show offline page for HTML requests
        if (req.headers.get('Accept')?.includes('text/html')) {
          return caches.match(OFFLINE_URL);
        }
        return new Response('Offline', { status: 503, headers: { 'Content-Type': 'text/plain' } });
      }
    })()
  );
});
