self.addEventListener("install", (event) => {
    console.debug("[NotMan::SW] #install");
});

self.addEventListener("activate", (event) => {
    console.debug("[NotMan::SW] #activate");
});

self.addEventListener("fetch", (event) => {
    event.respondWith(
        caches.match(event.request).then(async (response) => {
            if (response) {
                return response;
            }

            try {
                const newResponse = await fetch(event.request);
                return newResponse;
            } catch (ex) {
                console.debug(`[NotMan::SW] #fetch: failed to fetch request {event.request}`, ex);
            }
        }),
    );
});

self.addEventListener("notificationclick", (event) => {
    const action = event.action;
    const notification = event.notification;

    event.notification.close();

    event.waitUntil(
        globalThis.clients.matchAll({ type: "window", includeUncontrolled: true })
            .then(clients => {
                for (const client of clients) {
                    client.postMessage(
                        {
                            notificationUuid: notification.data.uuid,
                            actionId: action
                        }
                    );
                }
            })
    );
});