export class NavigationController {
    constructor() {
        this.loginNavItem = document.getElementById("loginNavItem");
        this.userNavItem = document.getElementById("userNavItem");
        this.logoutNavItem = document.getElementById("logoutNavItem");

        this.currentUsername = document.getElementById("currentUsername");
        this.logoutButton = document.getElementById("logoutButton");
    }

    init() {
        const user = this.getCurrentUser();

        if (user) {
            this.showLoggedInState(user);
        }
        else {
            this.showLoggedOutState();
        }

        this.logoutButton?.addEventListener("click", () => {
            this.logout();
        });
    }

    getCurrentUser() {
        const storedUser = sessionStorage.getItem("currentUser");

        if (!storedUser) {
            return null;
        }

        try {
            return JSON.parse(storedUser);
        }
        catch {
            sessionStorage.removeItem("currentUser");
            return null;
        }
    }

    showLoggedInState(user) {
        this.loginNavItem?.classList.add("d-none");

        this.userNavItem?.classList.remove("d-none");
        this.logoutNavItem?.classList.remove("d-none");

        if (this.currentUsername) {
            this.currentUsername.textContent =
                user.isAdmin
                    ? `${user.username} (admin)`
                    : user.username;
        }
    }

    showLoggedOutState() {
        this.loginNavItem?.classList.remove("d-none");

        this.userNavItem?.classList.add("d-none");
        this.logoutNavItem?.classList.add("d-none");
    }

    logout() {
        sessionStorage.removeItem("currentUser");

        window.location.href = "/Login";
    }
}

const navigationController = new NavigationController();

navigationController.init();