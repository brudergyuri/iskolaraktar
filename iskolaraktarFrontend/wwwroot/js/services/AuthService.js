import { ApiService } from "./ApiService.js";

export class AuthService {
    constructor(baseUrl) {
        this.apiService = new ApiService(baseUrl);
    }

    async getStatus() {
        const response = await this.apiService.get("/api/auth/status");

        if (!response.ok) {
            throw new Error("Nem sikerült lekérni a rendszer állapotát.");
        }

        return await response.json();
    }

    async login(username, password) {
        const response = await this.apiService.post("/api/auth/login", {
            username,
            password
        });

        if (response.status === 401) {
            throw new Error("Hibás felhasználónév vagy jelszó.");
        }

        if (!response.ok) {
            const errorText = await response.text();

            throw new Error(
                errorText || "Hiba történt a bejelentkezés során."
            );
        }

        return await response.json();
    }
}