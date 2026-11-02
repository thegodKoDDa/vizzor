@echo off
chcp 65001 >nul
echo ========================================================
echo        Desativando Software Antigo da Rise Mode
echo ========================================================
echo.

echo [1/3] Parando servico tifSer...
net stop tifSer 2>nul
sc config tifSer start=disabled 2>nul

echo [2/3] Encerrando processos antigos...
taskkill /F /IM "SendTemp.exe" 2>nul
taskkill /F /IM "CPU Server.exe" 2>nul
taskkill /F /IM "Rise Mode Temp CPU Driver R2.2.exe" 2>nul

echo [3/3] Removendo inicializacao automatica antiga...
reg delete "HKCU\Software\Microsoft\Windows\CurrentVersion\Run" /v "Rise Mode Temp CPU Driver R2.2" /f 2>nul
reg delete "HKLM\Software\Microsoft\Windows\CurrentVersion\Run" /v "Rise Mode Temp CPU Driver R2.2" /f 2>nul

echo.
echo ========================================================
echo Concluido! O software antigo foi desativado com sucesso.
echo ========================================================
pause
