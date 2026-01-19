class BrowserNotificationsManager {
    Cancel(notificationId) {
        console.log(`BrowserNotificationsManager.Cancel: ${notificationId}`);
    }
    ClearAll() {
        console.log(`BrowserNotificationsManager.ClearAll`);
    }

    Register() {
        console.log(`BrowserNotificationsManager.Register`);
    }

    Send(message) {
        alert(message);
        console.log(`BrowserNotificationsManager.Send: ${message}`);
    }

    Unregister() {
        console.log(`BrowserNotificationsManager.Unregister`);
    }

    Update(id) {
        console.log(`BrowserNotificationsManager.Update: ${id}`);
    }
}

globalThis.Unify = {
    Notifications: {
        Push: new BrowserNotificationsManager(),
    }
};