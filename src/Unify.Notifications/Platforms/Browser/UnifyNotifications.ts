class PushNotificationAction {
    public action: string;
    public title: string;
    public icon?: string;
}

class PushNotification {
    private $uuid: string;
    private $notification: Notification | undefined;
    private $hasEvented: boolean = false; // Prevent onclick then onclose calling.

    private readonly $onClickAction: Function;
    private readonly $onErrorAction: Function;
    private readonly $onCloseAction: Function;
    private readonly $onShowAction: Function;

    // Name of the button (action) that "clicked" the notification
    private $activatedButtonId?: string;
    public get activatedButtonId() {
        return this.$activatedButtonId;
    }

    public id: string;
    public group: string;
    public title: string;

    public text: string;
    public iconUrl?: string;
    public imageUrl?: string;
    public actions?: PushNotificationAction[];

    public isPersistent: boolean;
    public isSilent: boolean;

    /**
     * Epoc based timestamp.
     */
    public timeStamp?: number;

    constructor(
        pushNotificationJson: string,
        onClick: Function,
        onClose: Function,
        onError: Function,
        onShow: Function
    ) {
        const jsonObject = JSON.parse(pushNotificationJson);
        Object.assign(this, jsonObject);

        this.$onClickAction = onClick;
        this.$onErrorAction = onError;
        this.$onCloseAction = onClose;
        this.$onShowAction = onShow;
    }

    public activated(
        actionId: string
    ) {
        if (!this.$hasEvented) {
            this.$activatedButtonId = actionId;
            this.$hasEvented = true;
            this.$onClickAction();
        }
    }

    /**
     * Creates the notification, or retrieves the existing instance.
     */
    public toNotification(
        serviceWorker?: ServiceWorkerRegistration,
        update: boolean = false
    ) {
        if (this.$notification == undefined) {
            // Create it :)
            this.createNotification(serviceWorker, update);
        }
        return this.$notification;
    }

    private async createNotification(
        registration?: ServiceWorkerRegistration,
        update: boolean = false
    ): Promise<void> {
        this.$uuid = NotificationManager.getNotificationIdentifier(this.id, this.group);

        const notificationOptions = {
            //renotify: update,
            tag: this.group,
            data: {
                id: this.id,
                uuid: this.$uuid,
            },

            actions: this.actions ?? [],

            body: this.text,

            image: this.imageUrl,
            icon: this.iconUrl,
            badge: this.iconUrl,

            // Optional stuff
            timestamp: this.timeStamp,
            requireInteraction: this.isPersistent,
            silent: this.isSilent,
        };

        if (registration == undefined) {
            this.$notification = new Notification(
                this.title,
                notificationOptions
            );

            this.$notification.onclick = () => {
                if (!this.$hasEvented) {
                    this.$hasEvented = true;
                    this.$onClickAction();
                }
            }
        } else {
            await registration.showNotification(
                this.title,
                notificationOptions
            );

            const notifications = await registration.getNotifications({ tag: this.group });
            this.$notification = notifications.find((notification) => notification.data.uuid == this.$uuid);
        }

        // onclick is handled by the service worker.

        this.$notification.onerror = () => {
            if (!this.$hasEvented) {
                this.$hasEvented = true;
                this.$onErrorAction();
            }
        }
        this.$notification.onclose = () => {
            if (!this.$hasEvented) {
                this.$hasEvented = true;
                this.$onCloseAction();
            }
        }
        this.$notification.onshow = () => this.$onShowAction();
    }
}

class NotificationManager {
    /**
     * Notifications being managed, mapped by their tag and id.
     */
    private readonly $notifications: Map<string, PushNotification> = new Map<string, PushNotification>;

    private $serviceWorker: ServiceWorkerRegistration;

    public static getNotificationIdentifier(id: string, group: string) {
        return btoa(`${group ?? "default"}::${id}`);
    }

    public register() {
        return new Promise((res) => {
            try {
                if (Notification.permission == "granted") {
                    res("granted"); // All good here!
                }
                Notification.requestPermission(res);
            } catch (e) {
                console.error(
                    "[NotMan] Something horrible has happend attempting to register the notification manager..."
                );
                console.error(e);
                res("default");
            }
        });
    }

    public unregister(): Promise<boolean> {
        return new Promise((res) => {
            if (this.$serviceWorker == undefined) {
                res(true);
            }

            return this.$serviceWorker.unregister();
        });
    }

    public cancel(id: string, group: string): boolean {
        const identifier = NotificationManager.getNotificationIdentifier(id, group);
        try {
            const pushNotification = this.$notifications.get(identifier);
            pushNotification.toNotification(this.$serviceWorker).close();
            return true;
        } catch (e) {
            console.error("[NotMan] Failed to delete notification!");
            console.error(e);
            return false;
        } finally {
            this.$notifications.delete(identifier);
        }
    }

    public clearAll() {
        this.$notifications.forEach((pushNotification, key) => {
            let _ = this.cancel(pushNotification.id, pushNotification.group);
        });
    }

    public getNotificationActivationReason(id: string, group: string): string {
        try {
            const notification = this.$notifications.get(NotificationManager.getNotificationIdentifier(id, group));
            return notification.activatedButtonId;
        } catch (e) {
            console.error("[NotMan] Cannot get activation reason for push notification!", e);
            return '';
        }
    }

    public getMaxNotificationActions(): number {
        return (Notification as any).maxActions;
    }

    public send(
        pushNotificationJson: string,
        onClick: Function,
        onClose: Function,
        onError: Function,
    ) {
        return new Promise((res) => {
            try {
                const pushNotification = new PushNotification(
                    pushNotificationJson,
                    onClick,
                    onClose,
                    onError,
                    () => {
                        res(true);
                    }
                );
                this.$notifications.set(
                    NotificationManager.getNotificationIdentifier(pushNotification.id, pushNotification.group),
                    pushNotification
                );

                // Create/Send
                const _ = pushNotification.toNotification(this.$serviceWorker);

                // Give it a second to display - quite some time, haha
                setTimeout(() => res(false), 1000);
            } catch (e) {
                console.error("[NotMan] Failed to send notification!");
                console.error(e);
                res(false);
            }
        });
    }

    public loadServiceWorker(serviceWorkerUrl: string): Promise<boolean> {
        return new Promise((res) => {
            navigator.serviceWorker.register(serviceWorkerUrl).then(() => {
                navigator.serviceWorker.ready.then((registration) => {
                    navigator.serviceWorker.addEventListener('message', (event) => {
                        this.handleServiceWorkerMessage(event);
                    });
                    this.$serviceWorker = registration;
                    console.log("[NotMan] Service worker registered!");
                    res(true);
                });
            }).catch((err) => {
                console.warn("[NotMan] Service worker failed to register! Error:", err);
                res(false)
            });
        });
    }

    private handleServiceWorkerMessage(event: any): void {
        try {
            const notification = this.$notifications.get(event.data.notificationUuid);
            notification.activated(event.data.actionId);
        }
        catch {
            console.error("[NotMan] Cannot handle activation for push notification!", event?.data);
            // weird
        }
    }
};

export const NotMan = new NotificationManager();