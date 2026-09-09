import { ApiService } from "./ApiService.js";

export class AdminService {
    constructor(baseUrl) {
        this.apiService = new ApiService(baseUrl);
    }

    async getUsers() {
        const response = await this.apiService.get(
            "/api/auth/users"
        );

        if (!response.ok) {
            throw new Error(
                "Nem sikerült lekérni a felhasználókat."
            );
        }

        return await response.json();
    }

    async createUser(username, password, isAdmin) {
        const response = await this.apiService.post(
            "/api/auth/users",
            {
                username,
                password,
                isAdmin
            }
        );

        if (!response.ok) {
            const errorText = await response.text();

            throw new Error(
                errorText ||
                "Nem sikerült létrehozni a felhasználót."
            );
        }

        return await response.json();
    }

    async deleteUser(username) {
        const encodedUsername =
            encodeURIComponent(username);

        const response = await this.apiService.delete(
            `/api/auth/users/${encodedUsername}`
        );

        if (!response.ok) {
            const errorText = await response.text();

            throw new Error(
                errorText ||
                "Nem sikerült törölni a felhasználót."
            );
        }
    }

    async setPermission(username, tableName, access) {
        const encodedUsername =
            encodeURIComponent(username);

        const encodedTableName =
            encodeURIComponent(tableName);

        const response = await this.apiService.put(
            `/api/auth/users/${encodedUsername}/permissions/${encodedTableName}`,
            {
                access
            }
        );

        if (!response.ok) {
            const errorText = await response.text();

            throw new Error(
                errorText ||
                "Nem sikerült módosítani a jogosultságot."
            );
        }

        return await response.json();
    }

    async getTables() {
        const response = await this.apiService.get(
            "/api/tables"
        );

        if (!response.ok) {
            throw new Error(
                "Nem sikerült lekérni a leltárkörzeteket."
            );
        }

        return await response.json();
    }

    async createTable(name) {
        const columns = [
            {
                name: "Name",
                sqlType: "VARCHAR(255)",
                isNullable: false
            },
            {
                name: "Location",
                sqlType: "VARCHAR(255)",
                isNullable: true
            },
            {
                name: "Description",
                sqlType: "VARCHAR(255)",
                isNullable: true
            }
        ];

        const response = await this.apiService.post(
            "/api/tables",
            {
                name,
                columns
            }
        );

        if (!response.ok) {
            const errorText = await response.text();

            throw new Error(
                errorText ||
                "Nem sikerült létrehozni a leltárkörzetet."
            );
        }
    }

    async deleteTable(tableName) {
        const encodedTableName =
            encodeURIComponent(tableName);

        const response = await this.apiService.delete(
            `/api/tables/${encodedTableName}`
        );

        if (!response.ok) {
            const errorText = await response.text();

            throw new Error(
                errorText ||
                "Nem sikerült törölni a leltárkörzetet."
            );
        }
    }
}