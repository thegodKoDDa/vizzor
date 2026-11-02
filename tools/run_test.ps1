<#
.SYNOPSIS
    Script PowerShell para teste direto do visor Rise Mode Aura Ice no Windows.
#>

[Console]::OutputEncoding = [System.Text.Encoding]::UTF8

Write-Host "========================================================" -ForegroundColor Cyan
Write-Host "      VIZZOR - Teste de Envio ao Visor Rise Mode" -ForegroundColor Cyan
Write-Host "========================================================" -ForegroundColor Cyan
Write-Host ""

Write-Host "[1/3] Encerrando processos originais..." -ForegroundColor Yellow
Stop-Process -Name "SendTemp" -Force -ErrorAction SilentlyContinue
Stop-Process -Name "Rise Mode Temp CPU Driver R2.2" -Force -ErrorAction SilentlyContinue
Stop-Service -Name "tifSer" -Force -ErrorAction SilentlyContinue

Write-Host "[2/3] Verificando biblioteca hidapi..." -ForegroundColor Yellow
$hasHid = python -c "import hid; print('OK')" 2>$null
if ($hasHid -notmatch "OK") {
    Write-Host "Instalando hidapi via pip..." -ForegroundColor Cyan
    pip install hidapi
}

Write-Host "[3/3] Enviando valor de teste para o display (CPU = 77°C)..." -ForegroundColor Green
$scriptPath = Join-Path $PSScriptRoot "test_send.py"
python $scriptPath --cpu 77 --gpu 45 --cpu-load 30 --count 10

Write-Host ""
Write-Host "========================================================" -ForegroundColor Cyan
Write-Host "Teste concluido! Verifique o visor do water cooler." -ForegroundColor Cyan
Write-Host "========================================================" -ForegroundColor Cyan
