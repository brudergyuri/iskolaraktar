#!/usr/bin/env bash
# Self-contained, single-file publish for iskolaraktarBackend (Windows x64 + Linux x64).
# Output nem igényel telepített .NET futtatókörnyezetet a célgépen.
set -euo pipefail

PROJECT="iskolaraktarBackend/iskolaraktarBackend.csproj"
CONFIGURATION="Release"
OUTPUT_ROOT="publish"
RUNTIMES=("win-x64" "linux-x64")

for RID in "${RUNTIMES[@]}"; do
    OUT_DIR="${OUTPUT_ROOT}/${RID}"
    echo "==> Publishing for ${RID} -> ${OUT_DIR}"
    rm -rf "${OUT_DIR}"
    dotnet publish "${PROJECT}" \
        -c "${CONFIGURATION}" \
        -r "${RID}" \
        --self-contained true \
        -p:PublishSingleFile=true \
        -p:IncludeNativeLibrariesForSelfExtract=true \
        -p:PublishTrimmed=false \
        -o "${OUT_DIR}"
done

echo "==> Kész. Bináriso(ka)t a(z) '${OUTPUT_ROOT}/' mappa tartalmazza."
