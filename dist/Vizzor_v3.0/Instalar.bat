@echo off
chcp 65001 >nul
echo ========================================================
echo             VIZZOR - Instalador do Visor
echo        Rise Mode Aura Ice 360 (RM-WAIB-06-ARGB)
echo ========================================================
echo.

:: 1. Verificacao de Privilegios de Administrador
net session >nul 2>&1
if errorlevel 1 (
    echo [AVISO] Solicitando permissoes de Administrador...
    powershell -NoProfile -Command "Start-Process cmd.exe -Verb RunAs -ArgumentList '/c \"\"%~f0\"\"'"
    exit /b
)

set "DEST=C:\Vizzor"
echo [1/4] Criando diretorio de instalacao em %DEST%...
if not exist "%DEST%" mkdir "%DEST%"

echo [2/4] Verificando driver de sensores (PawnIO)...
if not exist "C:\Program Files\PawnIO\PawnIO.sys" (
    echo [INFO] Driver PawnIO nao encontrado.
    echo Instalando PawnIO para leitura dos sensores de temperatura da CPU...
    if exist "%~dp0PawnIO_setup.exe" (
        start /wait "" "%~dp0PawnIO_setup.exe" /SILENT
        echo Driver PawnIO instalado com sucesso!
    ) else (
        echo [AVISO] PawnIO_setup.exe nao encontrado nesta pasta.
    )
) else (
    echo [OK] Driver PawnIO ja esta instalado no sistema.
)
echo.

echo [3/4] Copiando arquivos do Vizzor...
taskkill /F /IM Vizzor.exe 2>nul
taskkill /F /IM VizzorApp.exe 2>nul
timeout /t 1 /nobreak >nul

copy /Y "%~dp0Vizzor.exe" "%DEST%\" >nul
copy /Y "%~dp0LibreHardwareMonitorLib.dll" "%DEST%\" >nul
if exist "%~dp0PawnIO_setup.exe" copy /Y "%~dp0PawnIO_setup.exe" "%DEST%\" >nul

echo [4/4] Configurando inicializacao automatica com o Windows...
reg add "HKCU\Software\Microsoft\Windows\CurrentVersion\Run" /v "Vizzor" /t REG_SZ /d "\"%DEST%\Vizzor.exe\"" /f >nul

echo.
echo ========================================================
echo Instalacao concluida com sucesso!
echo Iniciando o Vizzor na bandeja do sistema...
echo ========================================================
start "" "%DEST%\Vizzor.exe"
timeout /t 2 /nobreak >nul
