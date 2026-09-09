import { ApiService } from "./ApiService.js";

export class InventoryService {
    constructor(baseUrl) {
        this.apiService = new ApiService(baseUrl);
    }

    async getTables() {
        const response = await this.apiService.get("/api/tables");

        if (!response.ok) {
            throw new Error("Nem sikerült lekérni a leltárkörzeteket.");
        }

        return await response.json();
    }

    async getColumns(tableName) {
        const encodedTableName = encodeURIComponent(tableName);

        const response = await this.apiService.get(
            `/api/tables/${encodedTableName}/columns`
        );

        if (!response.ok) {
            throw new Error("Nem sikerült lekérni a leltárkörzet oszlopait.");
        }

        return await response.json();
    }

    async getItems(tableName) {
        const encodedTableName = encodeURIComponent(tableName);

        const response = await this.apiService.get(
            `/api/data/${encodedTableName}`
        );

        if (!response.ok) {
            throw new Error("Nem sikerült lekérni a leltári tételeket.");
        }

        return await response.json();
    }

    async createItem(tableName, item) {
        const encodedTableName = encodeURIComponent(tableName);

        const response = await this.apiService.post(
            `/api/data/${encodedTableName}`,
            item
        );

        if (!response.ok) {
            const errorText = await response.text();

            throw new Error(
                errorText || "Nem sikerült létrehozni a leltári tételt."
            );
        }

        return await response.json();
    }

    async updateItem(tableName, id, item) {
        const encodedTableName = encodeURIComponent(tableName);

        const response = await this.apiService.put(
            `/api/data/${encodedTableName}/${id}`,
            item
        );

        if (!response.ok) {
            const errorText = await response.text();

            throw new Error(
                errorText || "Nem sikerült módosítani a leltári tételt."
            );
        }
    }

    async deleteItem(tableName, id) {
        const encodedTableName = encodeURIComponent(tableName);

        const response = await this.apiService.delete(
            `/api/data/${encodedTableName}/${id}`
        );

        if (!response.ok) {
            const errorText = await response.text();

            throw new Error(
                errorText || "Nem sikerült törölni a leltári tételt."
            );
        }
    }
}
