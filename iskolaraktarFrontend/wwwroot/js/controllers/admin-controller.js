import { AdminService } from "../services/AdminService.js";
import { SessionService } from "../services/SessionService.js";

export class AdminController {
    constructor(adminService, sessionService) {
        this.adminService = adminService;
        this.sessionService = sessionService;

        this.errorElement = document.getElementById("adminError");
        this.successElement = document.getElementById("adminSuccess");

        this.userForm = document.getElementById("userForm");
        this.newUsername = document.getElementById("newUsername");
        this.newPassword = document.getElementById("newPassword");
        this.newUserIsAdmin = document.getElementById("newUserIsAdmin");

        this.usersTableBody =
            document.getElementById("usersTableBody");

        this.tableForm = document.getElementById("tableForm");
        this.newTableName = document.getElementById("newTableName");

        this.adminTablesTableBody =
            document.getElementById("adminTablesTableBody");

        this.permissionPanel =
            document.getElementById("permissionPanel");

        this.permissionUsername =
            document.getElementById("permissionUsername");

        this.permissionsTableBody =
            document.getElementById("permissionsTableBody");

        this.users = [];
        this.tables = [];
        this.selectedUsername = null;
    }

    async init() {
        const currentUser =
            this.sessionService.getCurrentUser();

        if (!currentUser) {
            window.location.href = "/Login";
            return;
        }

        if (!currentUser.isAdmin) {
            window.location.href = "/";
            return;
        }

        this.userForm?.addEventListener("submit", (event) => {
            this.handleCreateUser(event);
        });

        this.tableForm?.addEventListener("submit", (event) => {
            this.handleCreateTable(event);
        });

        await this.loadData();
    }

    async loadData() {
        try {
            this.hideError();

            const [users, tables] = await Promise.all([
                this.adminService.getUsers(),
                this.adminService.getTables()
            ]);

            this.users = users;
            this.tables = tables;

            this.renderUsers();
            this.renderTables();

            if (this.selectedUsername) {
                this.renderPermissions(this.selectedUsername);
            }
        }
        catch (error) {
            this.showError(error.message);
        }
    }

    renderUsers() {
        this.usersTableBody.innerHTML = "";

        const currentUser =
            this.sessionService.getCurrentUser();

        this.users.forEach((user) => {
            const row = document.createElement("tr");

            const usernameCell =
                document.createElement("td");

            usernameCell.textContent = user.username;

            const adminCell =
                document.createElement("td");

            adminCell.textContent =
                user.isAdmin ? "Igen" : "Nem";

            const actionsCell =
                document.createElement("td");

            actionsCell.classList.add("text-nowrap");

            if (!user.isAdmin) {
                const permissionButton =
                    document.createElement("button");

                permissionButton.type = "button";

                permissionButton.classList.add(
                    "btn",
                    "btn-sm",
                    "btn-outline-primary",
                    "me-2"
                );

                permissionButton.textContent =
                    "Jogosultságok";

                permissionButton.addEventListener(
                    "click",
                    () => {
                        this.renderPermissions(
                            user.username
                        );
                    }
                );

                actionsCell.appendChild(
                    permissionButton
                );
            }

            const deleteButton =
                document.createElement("button");

            deleteButton.type = "button";

            deleteButton.classList.add(
                "btn",
                "btn-sm",
                "btn-outline-danger"
            );

            deleteButton.textContent = "Törlés";

            if (
                currentUser &&
                currentUser.username === user.username
            ) {
                deleteButton.disabled = true;
                deleteButton.title =
                    "A saját felhasználó nem törölhető.";
            }
            else {
                deleteButton.addEventListener(
                    "click",
                    () => {
                        this.handleDeleteUser(user);
                    }
                );
            }

            actionsCell.appendChild(deleteButton);

            row.appendChild(usernameCell);
            row.appendChild(adminCell);
            row.appendChild(actionsCell);

            this.usersTableBody.appendChild(row);
        });
    }

    renderTables() {
        this.adminTablesTableBody.innerHTML = "";

        this.tables.forEach((tableName) => {
            const row = document.createElement("tr");

            const nameCell =
                document.createElement("td");

            nameCell.textContent = tableName;

            const actionsCell =
                document.createElement("td");

            const deleteButton =
                document.createElement("button");

            deleteButton.type = "button";

            deleteButton.classList.add(
                "btn",
                "btn-sm",
                "btn-outline-danger"
            );

            deleteButton.textContent = "Törlés";

            deleteButton.addEventListener(
                "click",
                () => {
                    this.handleDeleteTable(tableName);
                }
            );

            actionsCell.appendChild(deleteButton);

            row.appendChild(nameCell);
            row.appendChild(actionsCell);

            this.adminTablesTableBody.appendChild(row);
        });
    }

    renderPermissions(username) {
        const user = this.users.find(
            (item) => item.username === username
        );

        if (!user || user.isAdmin) {
            this.hidePermissionPanel();
            return;
        }

        this.selectedUsername = username;

        this.permissionUsername.textContent =
            username;

        this.permissionsTableBody.innerHTML = "";

        this.tables.forEach((tableName) => {
            const row = document.createElement("tr");

            const tableCell =
                document.createElement("td");

            tableCell.textContent = tableName;

            const accessCell =
                document.createElement("td");

            const select =
                document.createElement("select");

            select.classList.add("form-select");

            const permissions =
                user.permissions ?? {};

            const currentAccess =
                permissions[tableName]
                ?? permissions["*"]
                ?? "None";

            [
                {
                    value: "None",
                    text: "Nincs hozzáférés"
                },
                {
                    value: "Read",
                    text: "Megtekintés"
                },
                {
                    value: "ReadWrite",
                    text: "Módosítás"
                }
            ].forEach((accessOption) => {
                const option =
                    document.createElement("option");

                option.value =
                    accessOption.value;

                option.textContent =
                    accessOption.text;

                option.selected =
                    accessOption.value ===
                    currentAccess;

                select.appendChild(option);
            });

            select.addEventListener(
                "change",
                async () => {
                    await this.handlePermissionChange(
                        username,
                        tableName,
                        select.value
                    );
                }
            );

            accessCell.appendChild(select);

            row.appendChild(tableCell);
            row.appendChild(accessCell);

            this.permissionsTableBody.appendChild(row);
        });

        this.permissionPanel.classList.remove(
            "d-none"
        );
    }

    async handleCreateUser(event) {
        event.preventDefault();

        this.hideError();
        this.hideSuccess();

        const username =
            this.newUsername.value.trim();

        const password =
            this.newPassword.value;

        const isAdmin =
            this.newUserIsAdmin.checked;

        try {
            await this.adminService.createUser(
                username,
                password,
                isAdmin
            );

            this.userForm.reset();

            await this.loadData();

            this.showSuccess(
                "A felhasználó sikeresen létrejött."
            );
        }
        catch (error) {
            this.showError(error.message);
        }
    }

    async handleDeleteUser(user) {
        const confirmed = window.confirm(
            `Biztosan törölni szeretnéd a(z) "${user.username}" felhasználót?`
        );

        if (!confirmed) {
            return;
        }

        this.hideError();
        this.hideSuccess();

        try {
            await this.adminService.deleteUser(
                user.username
            );

            if (
                this.selectedUsername ===
                user.username
            ) {
                this.selectedUsername = null;
                this.hidePermissionPanel();
            }

            await this.loadData();

            this.showSuccess(
                "A felhasználó sikeresen törlésre került."
            );
        }
        catch (error) {
            this.showError(error.message);
        }
    }

    async handlePermissionChange(
        username,
        tableName,
        access
    ) {
        this.hideError();
        this.hideSuccess();

        try {
            await this.adminService.setPermission(
                username,
                tableName,
                access
            );

            await this.loadData();

            this.showSuccess(
                "A jogosultság sikeresen módosításra került."
            );
        }
        catch (error) {
            this.showError(error.message);
        }
    }

    async handleCreateTable(event) {
        event.preventDefault();

        this.hideError();
        this.hideSuccess();

        const tableName =
            this.newTableName.value.trim();

        try {
            await this.adminService.createTable(
                tableName
            );

            this.tableForm.reset();

            await this.loadData();

            this.showSuccess(
                "A leltárkörzet sikeresen létrejött."
            );
        }
        catch (error) {
            this.showError(error.message);
        }
    }

    async handleDeleteTable(tableName) {
        const confirmed = window.confirm(
            `Biztosan törölni szeretnéd a(z) "${tableName}" leltárkörzetet?\n\nA benne lévő összes leltári tétel is törlődik.`
        );

        if (!confirmed) {
            return;
        }

        this.hideError();
        this.hideSuccess();

        try {
            await this.adminService.deleteTable(
                tableName
            );

            await this.loadData();

            this.showSuccess(
                "A leltárkörzet sikeresen törlésre került."
            );
        }
        catch (error) {
            this.showError(error.message);
        }
    }

    hidePermissionPanel() {
        this.permissionPanel?.classList.add(
            "d-none"
        );
    }

    showError(message) {
        if (!this.errorElement) {
            return;
        }

        this.errorElement.textContent = message;
        this.errorElement.classList.remove(
            "d-none"
        );
    }

    hideError() {
        if (!this.errorElement) {
            return;
        }

        this.errorElement.textContent = "";
        this.errorElement.classList.add(
            "d-none"
        );
    }

    showSuccess(message) {
        if (!this.successElement) {
            return;
        }

        this.successElement.textContent = message;
        this.successElement.classList.remove(
            "d-none"
        );
    }

    hideSuccess() {
        if (!this.successElement) {
            return;
        }

        this.successElement.textContent = "";
        this.successElement.classList.add(
            "d-none"
        );
    }
}

const adminService =
    new AdminService("https://localhost:7273");

const sessionService =
    new SessionService();

const adminController =
    new AdminController(
        adminService,
        sessionService
    );

adminController.init();