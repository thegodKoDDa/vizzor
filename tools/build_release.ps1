param(
    [string]$OutputDirectory = (Join-Path $PSScriptRoot '..\dist'),
    [switch]$Archive
)
$ErrorActionPreference = 'Stop'
$project = Join-Path $PSScriptRoot '..\src\Vizzor\Vizzor.csproj'
$build = Join-Path ([IO.Path]::GetTempPath()) ('vizzor-build-' + [Guid]::NewGuid())
try {
    dotnet build $project -c Release --artifacts-path $build -o (Join-Path $build 'output') --nologo
    if ($LASTEXITCODE -ne 0) { throw 'Falha na compilacao.' }
    New-Item -ItemType Directory -Path $OutputDirectory -Force | Out-Null
    Copy-Item (Join-Path $build 'output\Vizzor.exe') $OutputDirectory -Force
    Copy-Item (Join-Path $PSScriptRoot 'install.ps1') $OutputDirectory -Force
    Copy-Item (Join-Path $PSScriptRoot 'release\*') $OutputDirectory -Force
    # O instalador PawnIO fica somente em dist; nao e copiado de releases antigos.
    if ($Archive) {
        Compress-Archive -Path (Join-Path $OutputDirectory '*') -DestinationPath ($OutputDirectory.TrimEnd('\','/') + '.zip') -Force
    }
} finally {
    if (Test-Path $build) { Remove-Item $build -Recurse -Force }
}
