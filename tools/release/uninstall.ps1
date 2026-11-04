$ErrorActionPreference = 'Stop'
if (Test-Path 'C:\Vizzor\Vizzor.exe') {
    $result = Start-Process 'C:\Vizzor\Vizzor.exe' -ArgumentList '--remove-autostart' -Wait -PassThru
    if ($result.ExitCode -ne 0) { throw 'Falha ao remover tarefa agendada.' }
}
Get-Process Vizzor,VizzorApp,Vizzor_Standalone -ErrorAction SilentlyContinue | Stop-Process -Force
Remove-ItemProperty 'HKCU:\Software\Microsoft\Windows\CurrentVersion\Run' -Name Vizzor -ErrorAction SilentlyContinue
Remove-Item 'C:\Vizzor' -Recurse -Force -ErrorAction SilentlyContinue
