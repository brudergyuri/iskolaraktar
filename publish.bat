@echo off
REM Self-contained, single-file publish for iskolaraktarBackend + iskolaraktarFrontend (Windows x64 + Linux x64).
REM Output nem igenyel telepitett .NET futtatokornyezetet a celgepen.
setlocal enabledelayedexpansion

set "CONFIGURATION=Release"
set "OUTPUT_ROOT=publish"
set "APPS=backend frontend"
set "PROJECT_backend=iskolaraktarBackend\iskolaraktarBackend.csproj"
set "PROJECT_frontend=iskolaraktarFrontend\iskolaraktarFrontend.csproj"

for %%A in (%APPS%) do (
    for %%R in (win-x64 linux-x64) do (
        set "OUT_DIR=%OUTPUT_ROOT%\%%R\%%A"
        echo ==^> Publishing %%A for %%R -^> !OUT_DIR!
        if exist "!OUT_DIR!" rmdir /s /q "!OUT_DIR!"
        call set "PROJECT=%%PROJECT_%%A%%"
        dotnet publish "!PROJECT!" ^
            -c "%CONFIGURATION%" ^
            -r %%R ^
            --self-contained true ^
            -p:PublishSingleFile=true ^
            -p:IncludeNativeLibrariesForSelfExtract=true ^
            -p:PublishTrimmed=false ^
            -o "!OUT_DIR!"
        if errorlevel 1 (
            echo Publish failed for %%A / %%R
            exit /b 1
        )
    )
)

echo ==^> Kesz. Binariso^(ka^)t a^(z^) '%OUTPUT_ROOT%\' mappa tartalmazza.
endlocal
