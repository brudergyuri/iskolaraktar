export class SessionService {
    constructor(storageKey = "currentUser") {
        this.storageKey = storageKey;
    }

    getCurrentUser() {
        const storedUser =
            sessionStorage.getItem(this.storageKey);

        if (!storedUser) {
            return null;
        }

        try {
            return JSON.parse(storedUser);
        }
        catch {
            sessionStorage.removeItem(this.storageKey);
            return null;
        }
    }

    isLoggedIn() {
        return this.getCurrentUser() !== null;
    }

    logout() {
        sessionStorage.removeItem(this.storageKey);
    }

    getAccessLevel(tableName) {
        const user = this.getCurrentUser();

        if (!user) {
            return "None";
        }

        if (user.isAdmin) {
            return "ReadWrite";
        }

        const permissions = user.permissions ?? {};

        return permissions[tableName]
            ?? permissions["*"]
            ?? "None";
    }

    canRead(tableName) {
        const accessLevel =
            this.getAccessLevel(tableName);

        return accessLevel === "Read"
            || accessLevel === "ReadWrite";
    }

    canWrite(tableName) {
        return this.getAccessLevel(tableName)
            === "ReadWrite";
    }
}