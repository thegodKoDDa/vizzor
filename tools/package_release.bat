@echo off
chcp 65001 >nul
echo ========================================================
echo        VIZZOR - Gerador de Pacote de Release
echo ========================================================
echo.

set "DIST_DIR=%~dp0..\dist\Vizzor_v3.0"
if exist "%DIST_DIR%" rd /s /q "%DIST_DIR%"
mkdir "%DIST_DIR%"

echo [1/3] Copiando arquivos executaveis e dependencias...
copy /Y "C:\Vizzor\VizzorApp.exe" "%DIST_DIR%\Vizzor.exe" >nul
copy /Y "C:\Vizzor\LibreHardwareMonitorLib.dll" "%DIST_DIR%\" >nul

echo [2/3] Criando scripts de instalacao e desinstalacao...
(
echo @echo off
echo chcp 65001 ^>nul
echo echo Instalando Vizzor...
echo set "DEST=C:\Vizzor"
echo mkdir "%%DEST%%" 2^>nul
echo copy /Y "%%~dp0Vizzor.exe" "%%DEST%%\" ^>nul
echo copy /Y "%%~dp0LibreHardwareMonitorLib.dll" "%%DEST%%\" ^>nul
echo powershell -NoProfile -Command "$ws = New-Object -ComObject WScript.Shell; $s = $ws.CreateShortcut('%%USERPROFILE%%\Desktop\Vizzor.lnk'); $s.TargetPath = '%%DEST%%\Vizzor.exe'; $s.WorkingDirectory = '%%DEST%%'; $s.Save()"
echo start "" "%%DEST%%\Vizzor.exe"
echo echo Instalacao concluida! Atalho criado na Area de Trabalho.
echo pause
) > "%DIST_DIR%\Instalar.bat"

(
echo @echo off
echo chcp 65001 ^>nul
echo echo Desinstalando Vizzor...
echo taskkill /F /IM Vizzor.exe 2^>nul
echo reg delete "HKCU\Software\Microsoft\Windows\CurrentVersion\Run" /v "Vizzor" /f 2^>nul
echo reg delete "HKCU\Software\Vizzor" /f 2^>nul
echo del "%%USERPROFILE%%\Desktop\Vizzor.lnk" 2^>nul
echo echo Vizzor desinstalado com sucesso.
echo pause
) > "%DIST_DIR%\Desinstalar.bat"

(
echo ========================================================
echo        VIZZOR - Software Alternativo para Visor
echo        Rise Mode Aura Ice ARGB 360 mm (RM-WAIB-06-ARGB)
echo ========================================================
echo.
echo COMO USAR:
echo 1. Feche o software original da Rise Mode.
echo 2. Execute 'Instalar.bat' como Administrador.
echo 3. O Vizzor iniciara automaticamente na bandeja do sistema
echo    ao lado do relogio.
echo.
echo RECURSOS:
echo - Suporte a temperatura da CPU e GPU (NVIDIA / AMD)
echo - Painel de Controle Dark Mode (2 cliques no icone da bandeja)
echo - Icone Dinamico com temperatura ao vivo na barra de tarefas
echo - Animacao de Boot ao iniciar
echo - Modo Relogio e Modo Stealth (apagar visor)
echo.
echo Projeto Open-Source sob licenca MIT.
) > "%DIST_DIR%\LEIA-ME.txt"

echo [3/3] Compactando pacote zip de distribuicao...
powershell -NoProfile -Command "Compress-Archive -Path '%DIST_DIR%\*' -DestinationPath '%~dp0..\dist\Vizzor_v3.0_Portable.zip' -Force"

echo.
echo ========================================================
echo Pacote de Release gerado com sucesso em:
echo dist\Vizzor_v3.0_Portable.zip
echo ========================================================
pause
