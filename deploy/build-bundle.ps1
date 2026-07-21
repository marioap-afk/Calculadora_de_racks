<#
.SYNOPSIS
  Canonical, reproducible RackCad Autoloader bundle build (I-12): `dotnet publish` + fail-closed verify.

.DESCRIPTION
  Publishes RackCad.Plugin; the AssembleAutoloaderBundle target emits RackCad.bundle into the publish
  output (PackageContents.xml generated from the template with the central version, plus Contents\ with
  the four RackCad DLLs and catalogs\). Then runs deploy\verify-bundle.ps1 fail-closed on the result.

  This is THE supported way to produce a shippable bundle. A plain `dotnet build` no longer emits one.
  Autodesk compile references stay out of the output (ADR-0003); the bundle carries only RackCad.

  Requires AutoCAD 2025 installed for the local compile references. Close AutoCAD first: it locks
  RackCad.Plugin.dll and the publish copy step would fail.

.PARAMETER Configuration
  Build configuration to publish: Debug or Release (default Release).

.PARAMETER Output
  Optional publish output directory (absolute recommended). Default: the Plugin's publish folder.

.PARAMETER DotNet
  Optional path to a dotnet executable whose SDK matches global.json. Default: auto-resolved from PATH,
  then the per-user SDK (%LOCALAPPDATA%\Microsoft\dotnet), then %ProgramFiles%\dotnet.

.PARAMETER SkipVerify
  Publish without running the bundle verification (not recommended).

.PARAMETER InventoryOutPath
  Optional file to receive the bundle's "<sha256>  <relative/path>" inventory.

.PARAMETER BaselineInventoryPath
  Optional previous inventory to compare against, proving reproducibility.

.EXAMPLE
  pwsh deploy\build-bundle.ps1
#>
#Requires -Version 7.0
[CmdletBinding()]
param(
    [ValidateSet("Debug", "Release")][string]$Configuration = "Release",
    [string]$Output,
    [string]$DotNet,
    [switch]$SkipVerify,
    [string]$InventoryOutPath,
    [string]$BaselineInventoryPath
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

function Test-DotNetSdk {
    param(
        [Parameter(Mandatory = $true)][string]$Executable,
        [Parameter(Mandatory = $true)][string]$RepositoryRoot)

    if (-not (Test-Path -LiteralPath $Executable -PathType Leaf) -and
        -not (Get-Command $Executable -ErrorAction SilentlyContinue)) {
        return $false
    }
    Push-Location $RepositoryRoot
    try {
        & $Executable --version *> $null
        return ($LASTEXITCODE -eq 0)
    }
    catch { return $false }
    finally { Pop-Location }
}

function Resolve-DotNetExecutable {
    param([Parameter(Mandatory = $true)][string]$RepositoryRoot)

    if ($DotNet) {
        if (Test-DotNetSdk -Executable $DotNet -RepositoryRoot $RepositoryRoot) { return $DotNet }
        throw "El dotnet indicado no resuelve el SDK de global.json: $DotNet"
    }

    $candidates = @(Get-Command dotnet -All -CommandType Application -ErrorAction SilentlyContinue |
        Select-Object -ExpandProperty Source -Unique)
    if ($env:LOCALAPPDATA) { $candidates += (Join-Path $env:LOCALAPPDATA "Microsoft\dotnet\dotnet.exe") }
    if (${env:ProgramFiles}) { $candidates += (Join-Path ${env:ProgramFiles} "dotnet\dotnet.exe") }

    foreach ($candidate in ($candidates | Where-Object { $_ } | Select-Object -Unique)) {
        if (Test-DotNetSdk -Executable $candidate -RepositoryRoot $RepositoryRoot) { return $candidate }
    }
    throw "No se encontro un dotnet compatible con el SDK fijado por global.json."
}

function Assert-AutoCadClosed {
    if (Get-Process -Name acad -ErrorAction SilentlyContinue) {
        throw "AutoCAD esta abierto. Cierralo antes de compilar el bundle (bloquea RackCad.Plugin.dll)."
    }
}

function Invoke-BuildBundle {
    $repo = Split-Path -Parent $PSScriptRoot
    $project = Join-Path $repo "src\RackCad.Plugin\RackCad.Plugin.csproj"

    Assert-AutoCadClosed
    $dotnet = Resolve-DotNetExecutable -RepositoryRoot $repo
    Write-Host "Publicando RackCad ($Configuration) con: $dotnet"

    $publishArgs = @("publish", $project, "-c", $Configuration, "-v:minimal")
    if ($Output) { $publishArgs += @("-o", ([System.IO.Path]::GetFullPath($Output))) }
    & $dotnet @publishArgs
    if ($LASTEXITCODE -ne 0) { throw "El publish fallo con codigo $LASTEXITCODE." }

    $bundle = if ($Output) {
        Join-Path ([System.IO.Path]::GetFullPath($Output)) "RackCad.bundle"
    }
    else {
        Join-Path $repo "src\RackCad.Plugin\bin\$Configuration\net8.0-windows\publish\RackCad.bundle"
    }
    if (-not (Test-Path -LiteralPath $bundle -PathType Container)) {
        throw "El publish no genero el bundle esperado: $bundle"
    }
    Write-Host "Bundle generado: $bundle"

    if ($SkipVerify) {
        Write-Host "Verificacion omitida por -SkipVerify."
        return
    }

    $verify = Join-Path $PSScriptRoot "verify-bundle.ps1"
    $verifyArgs = @("-NoProfile", "-File", $verify, "-BundlePath", $bundle)
    if ($InventoryOutPath) { $verifyArgs += @("-InventoryOutPath", $InventoryOutPath) }
    if ($BaselineInventoryPath) { $verifyArgs += @("-BaselineInventoryPath", $BaselineInventoryPath) }
    $pwshExe = [System.Environment]::ProcessPath
    & $pwshExe @verifyArgs
    if ($LASTEXITCODE -ne 0) { throw "La verificacion del bundle fallo (codigo $LASTEXITCODE)." }

    Write-Host "Bundle canonico listo y verificado: $bundle"
}

if ($MyInvocation.InvocationName -ne '.') {
    try {
        Invoke-BuildBundle
    }
    catch {
        Write-Error "No se genero el bundle. $($_.Exception.Message)"
        exit 1
    }
}
