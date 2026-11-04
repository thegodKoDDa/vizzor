param([string]$SourceDirectory = (Join-Path $PSScriptRoot '..\dist'), [string]$Destination = 'C:\Vizzor')
$ErrorActionPreference = 'Stop'
$id = [Security.Principal.WindowsIdentity]::GetCurrent()
if (-not ([Security.Principal.WindowsPrincipal]::new($id)).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) {
    throw 'Execute o instalador como Administrador para registrar a tarefa e acessar os sensores.'
}
$sourceExe = Join-Path $SourceDirectory 'Vizzor.exe'
if (-not (Test-Path $sourceExe)) { throw "Executavel ausente: $sourceExe" }
# Valida o pacote antes de encerrar a instalacao existente.
$assembly = [Reflection.Assembly]::ReflectionOnlyLoadFrom($sourceExe)
foreach ($dependency in 'LibreHardwareMonitorLib.dll','HidSharp.dll','RAMSPDToolkit-NDD.dll','System.CodeDom.dll') {
    if ($assembly.GetManifestResourceNames() -notcontains $dependency) { throw "Dependencia nao embutida: $dependency" }
}
New-Item -ItemType Directory -Path $Destination -Force | Out-Null
Get-Process Vizzor,VizzorApp,Vizzor_Standalone -ErrorAction SilentlyContinue | Stop-Process -Force
foreach ($name in 'Vizzor.exe','Vizzor.exe.config','VizzorApp.exe','LibreHardwareMonitorLib.dll','HidSharp.dll','RAMSPDToolkit-NDD.dll','System.CodeDom.dll','start.bat') {
    $path = Join-Path $Destination $name
    if (Test-Path $path) { Remove-Item -LiteralPath $path -Force }
}
Copy-Item -LiteralPath $sourceExe -Destination (Join-Path $Destination 'Vizzor.exe') -Force
if (Test-Path (Join-Path $SourceDirectory 'PawnIO_setup.exe')) {
    if (-not (Test-Path "$env:ProgramFiles\PawnIO\PawnIO.sys")) {
        $driver = Start-Process (Join-Path $SourceDirectory 'PawnIO_setup.exe') -ArgumentList '/SILENT' -Wait -PassThru
        if ($driver.ExitCode -notin 0,3010) { throw "Instalacao PawnIO falhou: $($driver.ExitCode)" }
    }
}
$registration = Start-Process (Join-Path $Destination 'Vizzor.exe') -ArgumentList '--enable-autostart' -Wait -PassThru
if ($registration.ExitCode -ne 0) { throw 'Falha ao registrar inicializacao automatica.' }
Start-ScheduledTask -TaskName ('Vizzor-' + $id.User.Value)
"Instalado em $Destination."
