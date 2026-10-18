@echo off
if not exist "%~dp0build\win-x64\SistemaInventario.Presentacion.exe" (
    echo No se encontro el ejecutable. Extraiga todo el ZIP o ejecute compilar.ps1.
    pause
    exit /b 1
)
start "" "%~dp0build\win-x64\SistemaInventario.Presentacion.exe"
