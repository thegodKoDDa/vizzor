@echo off
chcp 65001 >nul
echo ========================================================
echo             VIZZOR - Desinstalador
echo ========================================================
echo.

echo [1/3] Encerrando processos do Vizzor...
taskkill /F /IM Vizzor.exe 2>nul
taskkill /F /IM VizzorApp.exe 2>nul

echo [2/3] Removendo inicializacao com o Windows...
reg delete "HKCU\Software\Microsoft\Windows\CurrentVersion\Run" /v "Vizzor" /f 2>nul
reg delete "HKCU\Software\Vizzor" /f 2>nul

echo [3/3] Removendo arquivos de instalacao...
if exist "C:\Vizzor" (
    rd /s /q "C:\Vizzor" 2>nul
)

echo.
echo ========================================================
echo Vizzor foi completamente removido do sistema.
echo ========================================================
pause
