@echo off
chcp 65001 >nul
echo ========================================================
echo       VIZZOR - Teste de Envio ao Visor Rise Mode
echo ========================================================
echo.

echo [1/3] Fechando processos concorrentes do software original...
taskkill /F /IM "SendTemp.exe" 2>nul
taskkill /F /IM "Rise Mode Temp CPU Driver R2.2.exe" 2>nul
net stop tifSer 2>nul
echo.

echo [2/3] Verificando dependencias Python...
python -c "import hid" 2>nul
if errorlevel 1 (
    echo Instalando biblioteca hidapi...
    pip install hidapi
    if errorlevel 1 (
        echo [ERRO] Falha ao instalar hidapi. Verifique o Python do Windows.
        pause
        exit /b 1
    )
)
echo Dependencias OK.
echo.

echo [3/3] Enviando valor de teste para o display (CPU = 77 C)...
echo.
python "%~dp0test_send.py" --cpu 77 --gpu 45 --cpu-load 30 --count 10
echo.

echo ========================================================
echo Teste concluido! Olhe para o visor do water cooler.
echo Ele exibiu 77 C?
echo ========================================================
pause
