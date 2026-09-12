#!/usr/bin/env bash
# Self-contained, single-file publish for iskolaraktarBackend + iskolaraktarFrontend (Windows x64 + Linux x64).
# Output nem igényel telepített .NET futtatókörnyezetet a célgépen.
set -euo pipefail

CONFIGURATION="Release"
OUTPUT_ROOT="publish"
RUNTIMES=("win-x64" "linux-x64")
declare -A PROJECTS=(
    ["backend"]="iskolaraktarBackend/iskolaraktarBackend.csproj"
    ["frontend"]="iskolaraktarFrontend/iskolaraktarFrontend.csproj"
)

for APP in "${!PROJECTS[@]}"; do
    PROJECT="${PROJECTS[$APP]}"
    for RID in "${RUNTIMES[@]}"; do
        OUT_DIR="${OUTPUT_ROOT}/${RID}/${APP}"
        echo "==> Publishing ${APP} for ${RID} -> ${OUT_DIR}"
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
done

echo "==> Kész. Bináriso(ka)t a(z) '${OUTPUT_ROOT}/' mappa tartalmazza."
