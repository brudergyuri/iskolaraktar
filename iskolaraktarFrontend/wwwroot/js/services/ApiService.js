export class ApiService {
    constructor() {
    }

    async request(endpoint, options = {}) {
        const response = await fetch(endpoint, {
            ...options,
            headers: {
                "Content-Type": "application/json",
                ...options.headers
            }
        });

        return response;
    }

    async get(endpoint) {
        return this.request(endpoint, {
            method: "GET"
        });
    }

    async post(endpoint, data) {
        return this.request(endpoint, {
            method: "POST",
            body: JSON.stringify(data)
        });
    }

    async put(endpoint, data) {
        return this.request(endpoint, {
            method: "PUT",
            body: JSON.stringify(data)
        });
    }

    async delete(endpoint) {
        return this.request(endpoint, {
            method: "DELETE"
        });
    }
}