class PushNotificationAction {
    public action: string;
    public title: string;
    public icon?: string;
}

class PushNotification {
    private $notification: Notification | undefined;

    private $onClickAction: Function;
    private $onErrorAction: Function;
    private $onCloseAction: Function;
    private $onShowAction: Function;

    // Name of the button (action) that "clicked" the notification
    private activatedButtonId?: string;

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

    /**
     * Creates the notification, or retrieves the existing instance.
     */
    public toNotification() {
        if (this.$notification == undefined) {
            // Create it :)
            this.createNotification();
        }
        return this.$notification;
    }

    private createNotification(update: boolean = false): void {
        this.$notification = new Notification(
            this.title,
            {
                //renotify: update,
                tag: this.group,
                data: {
                    id: this.id,
                },

                //actions: this.actions ?? [],

                body: this.text,

                //image: this.imageUrl,
                //icon: this.iconUrl,
                //badge: this.iconUrl,

                // Optional stuff
                //timestamp: this.timeStamp,
                requireInteraction: this.isPersistent,
                silent: this.isSilent,
            }
        );

        this.$notification.onclick = () => this.$onClickAction();
        this.$notification.onerror = () => this.$onErrorAction();
        this.$notification.onclose = () => this.$onCloseAction();
        this.$notification.onshow = () => this.$onShowAction();
    }
}

class NotificationManager {
    /**
     * Notifications being managed, mapped by their tag and id.
     */
    private $notifications: Map<string, PushNotification> = new Map<string, PushNotification>;

    register() {
        return new Promise((res) => {
            try {
                if (Notification.permission == "granted") {
                    res("granted"); // All good here!
                }
                Notification.requestPermission(res);
            } catch (e) {
                console.error(
                    "Something horrible has happend attempting to register the notification manager..."
                );
                console.error(e);
                res("default");
            }
        });
    }

    public cancel(id: string, group: string): boolean {
        const identifier = this.getNotificationIdentifier(id, group);
        try {
            const pushNotification = this.$notifications.get(identifier);
            pushNotification.toNotification().close();
            return true;
        } catch (e) {
            console.error("Failed to delete notification!");
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

    getNotificationActivationReason(id: string, group: string) {
        return '';
    }

    send(
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
                    this.getNotificationIdentifier(pushNotification.id, pushNotification.group),
                    pushNotification
                );

                // Create/Send
                const _ = pushNotification.toNotification();

                // Give it a second to display - quite some time, haha
                setTimeout(() => res(false), 1000);
            } catch (e) {
                console.error("Failed to send notification!");
                console.error(e);
                res(false);
            }
        });
    }

    private getNotificationIdentifier(id: string, group: string) {
        return btoa(`${group ?? "default"}::${id}`);
    }
};

export default new NotificationManager();