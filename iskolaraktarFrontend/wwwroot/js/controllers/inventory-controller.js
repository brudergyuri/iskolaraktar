import { InventoryService } from "../services/InventoryService.js";
import { SessionService } from "../services/SessionService.js";

export class InventoryController {
    constructor(inventoryService, sessionService) {
        this.inventoryService = inventoryService;
        this.sessionService = sessionService;

        this.tableSelect = document.getElementById("tableSelect");
        this.errorElement = document.getElementById("inventoryError");
        this.successElement = document.getElementById("inventorySuccess");

        this.loadingElement = document.getElementById("inventoryLoading");

        this.itemFormContainer = document.getElementById("itemFormContainer");
        this.itemForm = document.getElementById("itemForm");
        this.itemFormFields = document.getElementById("itemFormFields");
        this.createItemButton = document.getElementById("createItemButton");

        this.inventoryTable = document.getElementById("inventoryTable");
        this.inventoryTableHead = document.getElementById("inventoryTableHead");
        this.inventoryTableBody = document.getElementById("inventoryTableBody");

        this.currentColumns = [];
        this.editingItemId = null;

        this.cancelEditButton = null;
    }

    async init() {
        if (!this.tableSelect) {
            return;
        }

        if (!this.sessionService.isLoggedIn()) {
            window.location.href = "/Login";
            return;
        }

        this.createCancelEditButton();

        this.tableSelect.addEventListener("change", () => {
            this.handleTableChange();
        });

        this.itemForm?.addEventListener("submit", (event) => {
            this.handleSaveItem(event);
        });

        await this.loadTables();
    }

    createCancelEditButton() {
        if (!this.createItemButton) {
            return;
        }

        const buttonContainer = this.createItemButton.parentElement;

        if (!buttonContainer) {
            return;
        }

        this.cancelEditButton = document.createElement("button");

        this.cancelEditButton.type = "button";
        this.cancelEditButton.classList.add(
            "btn",
            "btn-secondary",
            "ms-2",
            "d-none"
        );

        this.cancelEditButton.textContent = "Mégse";

        this.cancelEditButton.addEventListener("click", () => {
            this.cancelEdit();
        });

        buttonContainer.appendChild(this.cancelEditButton);
    }

    async loadTables() {
        try {
            this.hideError();

            const tables = await this.inventoryService.getTables();

            const accessibleTables = tables.filter((tableName) =>
                this.sessionService.canRead(tableName)
            );

            this.tableSelect.innerHTML = `
                <option value="">
                    Válassz leltárkörzetet...
                </option>
            `;

            accessibleTables.forEach((tableName) => {
                const option = document.createElement("option");

                option.value = tableName;
                option.textContent = tableName;

                this.tableSelect.appendChild(option);
            });

            if (accessibleTables.length === 0) {
                this.tableSelect.innerHTML = `
                    <option value="">
                        Nincs elérhető leltárkörzet
                    </option>
                `;

                this.tableSelect.disabled = true;
            }
        }
        catch (error) {
            this.showError(error.message);

            this.tableSelect.innerHTML = `
                <option value="">
                    Nem sikerült betölteni
                </option>
            `;
        }
    }

    async handleTableChange() {
        const tableName = this.tableSelect.value;

        this.hideError();
        this.hideSuccess();
        this.cancelEdit();

        if (!tableName) {
            this.currentColumns = [];

            this.hideItemForm();
            this.hideInventoryTable();

            return;
        }

        if (!this.sessionService.canRead(tableName)) {
            this.hideItemForm();
            this.hideInventoryTable();

            this.showError(
                "Nincs jogosultságod ehhez a leltárkörzethez."
            );

            return;
        }

        await this.loadInventory(tableName);
    }

    async loadInventory(tableName) {
        try {
            this.hideError();
            this.showLoading();
            this.hideInventoryTable();
            this.hideItemForm();

            const [columns, items] = await Promise.all([
                this.inventoryService.getColumns(tableName),
                this.inventoryService.getItems(tableName)
            ]);

            this.currentColumns = columns;

            const canWrite =
                this.sessionService.canWrite(tableName);

            if (canWrite) {
                this.renderItemForm(columns);
                this.showItemForm();
            }
            else {
                this.hideItemForm();
            }

            this.renderTable(
                columns,
                items,
                canWrite
            );
        }
        catch (error) {
            this.showError(error.message);
        }
        finally {
            this.hideLoading();
        }
    }

    renderItemForm(columns) {
        this.itemFormFields.innerHTML = "";

        const excludedColumns = [
            "Id",
            "QrGuid",
            "LastInventoryDate"
        ];

        const editableColumns = columns.filter((column) =>
            !excludedColumns.includes(column.name)
        );

        editableColumns.forEach((column) => {
            const wrapper = document.createElement("div");
            wrapper.classList.add("col-md-6");

            const label = document.createElement("label");

            label.classList.add("form-label");
            label.htmlFor = `field-${column.name}`;
            label.textContent = column.name;

            const input = document.createElement("input");

            input.classList.add("form-control");
            input.id = `field-${column.name}`;
            input.name = column.name;
            input.dataset.columnName = column.name;
            input.dataset.dataType = column.dataType;

            input.type = this.getInputType(column.dataType);

            if (!column.isNullable) {
                input.required = true;
            }

            wrapper.appendChild(label);
            wrapper.appendChild(input);

            this.itemFormFields.appendChild(wrapper);
        });
    }

    getInputType(dataType) {
        const normalizedType = dataType.toLowerCase();

        if (
            normalizedType === "int" ||
            normalizedType === "bigint" ||
            normalizedType === "smallint" ||
            normalizedType === "tinyint" ||
            normalizedType === "decimal" ||
            normalizedType === "double" ||
            normalizedType === "float"
        ) {
            return "number";
        }

        if (normalizedType === "date") {
            return "date";
        }

        if (
            normalizedType === "datetime" ||
            normalizedType === "timestamp"
        ) {
            return "datetime-local";
        }

        return "text";
    }

    async handleSaveItem(event) {
        event.preventDefault();

        const tableName = this.tableSelect.value;

        if (!tableName) {
            this.showError("Előbb válassz leltárkörzetet.");
            return;
        }

        if (!this.sessionService.canWrite(tableName)) {
            this.showError(
                "Nincs módosítási jogosultságod ehhez a leltárkörzethez."
            );

            return;
        }

        this.hideError();
        this.hideSuccess();
        this.setCreateButtonLoading(true);

        try {
            const item = this.collectFormData();

            if (this.editingItemId === null) {
                await this.inventoryService.createItem(
                    tableName,
                    item
                );

                this.showSuccess(
                    "A leltári tétel sikeresen hozzáadásra került."
                );
            }
            else {
                await this.inventoryService.updateItem(
                    tableName,
                    this.editingItemId,
                    item
                );

                this.showSuccess(
                    "A leltári tétel sikeresen módosításra került."
                );
            }

            this.resetForm();

            await this.loadInventory(tableName);
        }
        catch (error) {
            this.showError(error.message);
        }
        finally {
            this.setCreateButtonLoading(false);
        }
    }

    collectFormData() {
        const item = {};

        const inputs =
            this.itemFormFields.querySelectorAll("[data-column-name]");

        inputs.forEach((input) => {
            const columnName = input.dataset.columnName;
            const dataType = input.dataset.dataType;

            item[columnName] =
                this.convertInputValue(input.value, dataType);
        });

        return item;
    }

    convertInputValue(value, dataType) {
        if (value === "") {
            return null;
        }

        const normalizedType = dataType.toLowerCase();

        if (
            normalizedType === "int" ||
            normalizedType === "bigint" ||
            normalizedType === "smallint" ||
            normalizedType === "tinyint"
        ) {
            return Number.parseInt(value, 10);
        }

        if (
            normalizedType === "decimal" ||
            normalizedType === "double" ||
            normalizedType === "float"
        ) {
            return Number.parseFloat(value);
        }

        return value;
    }

    renderTable(columns, items, canWrite) {
        this.inventoryTableHead.innerHTML = "";
        this.inventoryTableBody.innerHTML = "";

        const headerRow = document.createElement("tr");

        columns.forEach((column) => {
            const headerCell = document.createElement("th");

            headerCell.textContent = column.name;

            headerRow.appendChild(headerCell);
        });

        if (canWrite) {
            const actionsHeader = document.createElement("th");

            actionsHeader.textContent = "Műveletek";

            headerRow.appendChild(actionsHeader);
        }

        this.inventoryTableHead.appendChild(headerRow);

        if (items.length === 0) {
            const row = document.createElement("tr");
            const cell = document.createElement("td");

            cell.colSpan =
                columns.length + (canWrite ? 1 : 0);

            cell.textContent =
                "Ebben a leltárkörzetben még nincs egyetlen tétel sem.";

            cell.classList.add(
                "text-center",
                "text-muted"
            );

            row.appendChild(cell);

            this.inventoryTableBody.appendChild(row);
        }
        else {
            items.forEach((item) => {
                const row = document.createElement("tr");

                columns.forEach((column) => {
                    const cell = document.createElement("td");

                    const value = item[column.name];

                    cell.textContent =
                        value === null || value === undefined
                            ? ""
                            : value;

                    row.appendChild(cell);
                });

                if (canWrite) {
                    const actionsCell =
                        document.createElement("td");

                    actionsCell.classList.add(
                        "text-nowrap"
                    );

                    const editButton =
                        document.createElement("button");

                    editButton.type = "button";

                    editButton.classList.add(
                        "btn",
                        "btn-sm",
                        "btn-outline-primary",
                        "me-2"
                    );

                    editButton.textContent = "Módosítás";

                    editButton.addEventListener(
                        "click",
                        () => {
                            this.startEdit(item);
                        }
                    );

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
                            this.handleDeleteItem(item);
                        }
                    );

                    actionsCell.appendChild(editButton);
                    actionsCell.appendChild(deleteButton);

                    row.appendChild(actionsCell);
                }

                this.inventoryTableBody.appendChild(row);
            });
        }

        this.inventoryTable.classList.remove("d-none");
    }

    startEdit(item) {
        const tableName = this.tableSelect.value;

        if (!this.sessionService.canWrite(tableName)) {
            this.showError(
                "Nincs módosítási jogosultságod ehhez a leltárkörzethez."
            );

            return;
        }

        this.hideError();
        this.hideSuccess();

        this.editingItemId = item.Id;

        const inputs =
            this.itemFormFields.querySelectorAll("[data-column-name]");

        inputs.forEach((input) => {
            const columnName = input.dataset.columnName;
            const value = item[columnName];

            input.value =
                value === null || value === undefined
                    ? ""
                    : value;
        });

        if (this.createItemButton) {
            this.createItemButton.textContent =
                "Módosítás mentése";
        }

        this.cancelEditButton?.classList.remove("d-none");

        this.itemFormContainer?.scrollIntoView({
            behavior: "smooth",
            block: "start"
        });
    }

    cancelEdit() {
        this.resetForm();
    }

    resetForm() {
        this.editingItemId = null;

        this.itemForm?.reset();

        if (this.createItemButton) {
            this.createItemButton.textContent =
                "Tétel hozzáadása";
        }

        this.cancelEditButton?.classList.add("d-none");
    }

    async handleDeleteItem(item) {
        const tableName = this.tableSelect.value;

        if (!tableName) {
            return;
        }

        if (!this.sessionService.canWrite(tableName)) {
            this.showError(
                "Nincs törlési jogosultságod ehhez a leltárkörzethez."
            );

            return;
        }

        const confirmed = window.confirm(
            `Biztosan törölni szeretnéd ezt a tételt?\n\nEszközazonosító: ${item.AssetCode ?? item.Id}`
        );

        if (!confirmed) {
            return;
        }

        this.hideError();
        this.hideSuccess();

        try {
            await this.inventoryService.deleteItem(
                tableName,
                item.Id
            );

            if (this.editingItemId === item.Id) {
                this.resetForm();
            }

            await this.loadInventory(tableName);

            this.showSuccess(
                "A leltári tétel sikeresen törlésre került."
            );
        }
        catch (error) {
            this.showError(error.message);
        }
    }

    showItemForm() {
        this.itemFormContainer?.classList.remove("d-none");
    }

    hideItemForm() {
        this.itemFormContainer?.classList.add("d-none");
    }

    hideInventoryTable() {
        this.inventoryTable?.classList.add("d-none");
    }

    showLoading() {
        this.loadingElement?.classList.remove("d-none");
    }

    hideLoading() {
        this.loadingElement?.classList.add("d-none");
    }

    setCreateButtonLoading(isLoading) {
        if (!this.createItemButton) {
            return;
        }

        this.createItemButton.disabled = isLoading;

        if (isLoading) {
            this.createItemButton.textContent = "Mentés...";
            return;
        }

        this.createItemButton.textContent =
            this.editingItemId === null
                ? "Tétel hozzáadása"
                : "Módosítás mentése";
    }

    showError(message) {
        if (!this.errorElement) {
            return;
        }

        this.errorElement.textContent = message;
        this.errorElement.classList.remove("d-none");
    }

    hideError() {
        if (!this.errorElement) {
            return;
        }

        this.errorElement.textContent = "";
        this.errorElement.classList.add("d-none");
    }

    showSuccess(message) {
        if (!this.successElement) {
            return;
        }

        this.successElement.textContent = message;
        this.successElement.classList.remove("d-none");
    }

    hideSuccess() {
        if (!this.successElement) {
            return;
        }

        this.successElement.textContent = "";
        this.successElement.classList.add("d-none");
    }
}

const inventoryService =
    new InventoryService("https://localhost:7273");

const sessionService =
    new SessionService();

const inventoryController =
    new InventoryController(
        inventoryService,
        sessionService
    );

inventoryController.init();