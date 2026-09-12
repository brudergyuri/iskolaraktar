import { AuthService } from "../services/AuthService.js";

export class LoginController {
    constructor(authService) {
        this.authService = authService;

        this.form = document.getElementById("loginForm");
        this.usernameInput = document.getElementById("username");
        this.passwordInput = document.getElementById("password");
        this.errorElement = document.getElementById("loginError");
        this.submitButton = this.form?.querySelector('button[type="submit"]');
    }

    init() {
        if (!this.form) {
            return;
        }

        this.form.addEventListener("submit", (event) => {
            this.handleSubmit(event);
        });
    }

    async handleSubmit(event) {
        event.preventDefault();

        this.hideError();
        this.setLoading(true);

        const username = this.usernameInput.value.trim();
        const password = this.passwordInput.value;

        try {
            const user = await this.authService.login(
                username,
                password
            );

            sessionStorage.setItem(
                "currentUser",
                JSON.stringify(user)
            );

            window.location.href = "/";
        }
        catch (error) {
            this.showError(error.message);
        }
        finally {
            this.setLoading(false);
        }
    }

    showError(message) {
        this.errorElement.textContent = message;
        this.errorElement.style.display = "block";
    }

    hideError() {
        this.errorElement.textContent = "";
        this.errorElement.style.display = "none";
    }

    setLoading(isLoading) {
        if (!this.submitButton) {
            return;
        }

        this.submitButton.disabled = isLoading;

        this.submitButton.textContent =
            isLoading
                ? "Bejelentkezés..."
                : "Bejelentkezés";
    }
}

const authService =
    new AuthService("");

const loginController =
    new LoginController(authService);

loginController.init();