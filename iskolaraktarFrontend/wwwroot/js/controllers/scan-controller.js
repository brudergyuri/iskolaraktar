const scanResult =
    document.getElementById("scanResult");

async function scanItem() {
    const pathParts =
        window.location.pathname
            .split("/")
            .filter(Boolean);

    const qrGuid =
        pathParts[pathParts.length - 1];

    if (!qrGuid) {
        showError(
            "A QR-azonosító nem található."
        );

        return;
    }

    try {
        const response = await fetch(
            `/api/scan/${encodeURIComponent(qrGuid)}`,
            {
                method: "POST"
            }
        );

        if (response.status === 404) {
            showError(
                "Nem található leltári eszköz ezzel a QR-kóddal."
            );

            return;
        }

        if (!response.ok) {
            throw new Error(
                "A QR-kód feldolgozása sikertelen."
            );
        }

        const result =
            await response.json();

        showSuccess(result);
    }
    catch (error) {
        showError(error.message);
    }
}

function showSuccess(result) {
    if (!scanResult) {
        return;
    }

    const item =
        result.item ?? {};

    scanResult.className =
        "alert alert-success mt-4";

    scanResult.innerHTML = `
        <h2 class="h4">
            Sikeres leltározás
        </h2>

        <p class="mb-1">
            <strong>Leltárkörzet:</strong>
            ${escapeHtml(result.tableName ?? "")}
        </p>

        <p class="mb-1">
            <strong>Eszközazonosító:</strong>
            ${escapeHtml(item.AssetCode ?? "")}
        </p>

        <p class="mb-1">
            <strong>Név:</strong>
            ${escapeHtml(item.Name ?? "")}
        </p>

        <p class="mb-1">
            <strong>Hely:</strong>
            ${escapeHtml(item.Location ?? "")}
        </p>

        <p class="mb-0">
            <strong>Utolsó leltározás:</strong>
            ${escapeHtml(formatDate(item.LastInventoryDate))}
        </p>
    `;
}

function showError(message) {
    if (!scanResult) {
        return;
    }

    scanResult.className =
        "alert alert-danger mt-4";

    scanResult.textContent =
        message;
}

function formatDate(value) {
    if (!value) {
        return "";
    }

    const date =
        new Date(value);

    if (Number.isNaN(date.getTime())) {
        return value;
    }

    return date.toLocaleString("hu-HU");
}

function escapeHtml(value) {
    return String(value)
        .replaceAll("&", "&amp;")
        .replaceAll("<", "&lt;")
        .replaceAll(">", "&gt;")
        .replaceAll('"', "&quot;")
        .replaceAll("'", "&#039;");
}

scanItem();