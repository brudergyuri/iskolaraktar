@echo off
REM Self-contained, single-file publish for iskolaraktarBackend (Windows x64 + Linux x64).
REM Output nem igenyel telepitett .NET futtatokornyezetet a celgepen.
setlocal enabledelayedexpansion

set "PROJECT=iskolaraktarBackend\iskolaraktarBackend.csproj"
set "CONFIGURATION=Release"
set "OUTPUT_ROOT=publish"

for %%R in (win-x64 linux-x64) do (
    set "OUT_DIR=%OUTPUT_ROOT%\%%R"
    echo ==^> Publishing for %%R -^> !OUT_DIR!
    if exist "!OUT_DIR!" rmdir /s /q "!OUT_DIR!"
    dotnet publish "%PROJECT%" ^
        -c "%CONFIGURATION%" ^
        -r %%R ^
        --self-contained true ^
        -p:PublishSingleFile=true ^
        -p:IncludeNativeLibrariesForSelfExtract=true ^
        -p:PublishTrimmed=false ^
        -o "!OUT_DIR!"
    if errorlevel 1 (
        echo Publish failed for %%R
        exit /b 1
    )
)

echo ==^> Kesz. Binariso^(ka^)t a^(z^) '%OUTPUT_ROOT%\' mappa tartalmazza.
endlocal
