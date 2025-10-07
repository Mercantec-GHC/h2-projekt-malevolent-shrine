

### 1️⃣ **Offline Support** 
When internet is lost, user sees a beautiful offline page instead of browser error.

**File:** `wwwroot/offline.html`
- Simple HTML page with nice design
- Shows "You're Offline" message
- Has "Retry" button to reload

### 2️⃣ **Smart Caching**
App saves files in browser cache for fast loading.

**File:** `wwwroot/service-worker.published.js`
- Caches all app files (JS, CSS, images)
- Uses 2 strategies:
  - **Pages:** Network First → Cache → Offline page
  - **Assets:** Cache First → Network

### 3️⃣ **App Manifest**
Tells browser how to install the app.

**File:** `wwwroot/manifest.webmanifest`
- App name: "Malevolent Shrine Hotel Manager"
- Icons for home screen
- Colors and display mode

### 4️⃣ **Auto-Update Detection**
When new version is deployed, user gets notification.

**File:** `wwwroot/index.html`
- Shows green popup: "New version available!"
- User can update immediately or later

---

## How Service Worker Works (Simple Explanation)

```
┌─────────────┐
│   Browser   │
└──────┬──────┘
       │
       ▼
┌─────────────────┐      ┌──────────────┐
│ Service Worker  │◄────►│ Cache Storage│
│  (Middleman)    │      └──────────────┘
└─────────┬───────┘
          │
          ▼
    ┌──────────┐
    │ Network  │
    └──────────┘
```

**Step by Step:**
1. User opens page
2. Service Worker intercepts request
3. Check if file is in cache
4. If YES → serve from cache (fast! ⚡)
5. If NO → download from network → save to cache

---

## Testing Your PWA

### Test 1: Install App
1. Open site in Chrome
2. Look for install icon in address bar (⊕)
3. Click "Install"
4. App appears like native app!

### Test 2: Offline Mode
1. Open DevTools (F12)
2. Go to "Network" tab
3. Enable "Offline" checkbox
4. Refresh page → see offline page!

### Test 3: Check Cache
1. Open DevTools (F12)
2. Go to "Application" tab
3. Look at "Cache Storage" → see cached files
4. Look at "Service Workers" → should be "activated"

---

## Project Structure

```
Blazor/wwwroot/
├── offline.html              ← Offline page (NEW!)
├── manifest.webmanifest      ← App info (UPDATED!)
├── service-worker.js         ← Dev version (empty)
├── service-worker.published.js  ← Production version (NEW!)
└── index.html               ← Registration code (UPDATED!)
```

---

## Key Concepts to Learn

### 1. Service Worker Lifecycle
```
Install → Activate → Fetch (intercept requests)
```

### 2. Caching Strategies
- **Cache First:** Fast, but may show old content
- **Network First:** Always fresh, but slower
- **We use BOTH!** Pages → Network, Assets → Cache

### 3. Cache API
```javascript
// Save to cache
caches.open('my-cache').then(cache => {
    cache.add('/file.js');
});

// Get from cache
caches.match('/file.js').then(response => {
    return response;
});
```

---

## Common Questions

**Q: Why do we need offline.html?**
A: When network fails and page not in cache, we show this instead of error.

**Q: What is service-worker-assets.js?**
A: Auto-generated file with list of all app files. Blazor creates it during build.

**Q: Why two service worker files?**
A: `service-worker.js` = development (does nothing)
   `service-worker.published.js` = production (full PWA)

**Q: How to clear cache for testing?**
A: DevTools → Application → Clear Storage → Clear site data

---

## Build & Deploy

### Build with PWA
```bash
dotnet build
```
→ Creates `service-worker-assets.js` automatically

### Publish
```bash
dotnet publish -c Release
```
→ Uses `service-worker.published.js` in production

