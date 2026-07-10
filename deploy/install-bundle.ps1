<#
.SYNOPSIS
  Build (optional) and install the RackCad Autoloader bundle so AutoCAD loads RackCad
  automatically at startup — no NETLOAD.

.DESCRIPTION
  Copies the assembled RackCad.bundle into the per-user ApplicationPlugins folder
  (%AppData%\Autodesk\ApplicationPlugins\). Requires AutoCAD 2025+ (.NET 8). Close AutoCAD
  before running: it locks the plugin DLL, so an in-place update fails while it is open.

.PARAMETER Configuration
  Build configuration to take the bundle from (default: Release).

.PARAMETER Build
  Also run `dotnet build -c <Configuration>` before installing.

.EXAMPLE
  pwsh deploy\install-bundle.ps1 -Build
#>
param(
    [string]$Configuration = "Release",
    [switch]$Build
)

$ErrorActionPreference = "Stop"
$repo = Split-Path -Parent $PSScriptRoot   # deploy\ -> repo root

if (Get-Process -Name acad -ErrorAction SilentlyContinue) {
    Write-Warning "AutoCAD esta abierto: cierralo antes de instalar/actualizar (bloquea RackCad.Plugin.dll)."
    return
}

if ($Build) {
    & dotnet build (Join-Path $repo "src\RackCad.Plugin\RackCad.Plugin.csproj") -c $Configuration
    if ($LASTEXITCODE -ne 0) { throw "El build fallo." }
}

$source = Join-Path $repo "src\RackCad.Plugin\bin\$Configuration\net8.0-windows\RackCad.bundle"
if (-not (Test-Path $source)) {
    throw "No se encontro el bundle: $source. Compila primero (usa -Build) con AutoCAD cerrado."
}

$target = Join-Path $env:APPDATA "Autodesk\ApplicationPlugins\RackCad.bundle"
if (Test-Path $target) { Remove-Item -Recurse -Force $target }
New-Item -ItemType Directory -Force -Path (Split-Path -Parent $target) | Out-Null
Copy-Item -Recurse -Force $source $target

Write-Host "RackCad instalado en: $target"
Write-Host "Abre AutoCAD 2025+; los comandos (RACKCAD, RACKSELECTIVO, ...) quedan disponibles sin NETLOAD."
