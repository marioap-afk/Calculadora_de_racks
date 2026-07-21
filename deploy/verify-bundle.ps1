<#
.SYNOPSIS
  Fail-closed verification of an assembled RackCad Autoloader bundle (I-12).

.DESCRIPTION
  Verifies structure, names, paths, versions and exact content of a RackCad.bundle and proves that
  it distributes ONLY authorized RackCad files and permitted data (ADR-0003 constraints 7, 8 and 11:
  zero Autodesk assemblies in the bundle or artifacts). Every check throws on violation, so the
  script exits non-zero unless the bundle is fully compliant.

  Enforced (fail-closed allowlist — anything not on it is a violation):
    - PackageContents.xml at the bundle root;
    - Contents\ with exactly the four RackCad DLLs (Plugin, Application, Domain, UI);
    - Contents\catalogs\ with at least one CSV/JSON catalog (only .csv/.json/.dwg allowed there);
    - no other file anywhere in the bundle; in particular no Autodesk-named DLL.
  Manifest checks: AppVersion, ComponentEntry Version, ModuleName, SeriesMin and SeriesMax match the
  central source (Directory.Build.props) or the values passed in.

  It also prints a SHA-256 inventory of every file. -InventoryOutPath writes it to a file;
  -BaselineInventoryPath compares against a previous inventory to prove reproducibility (two
  generations from the same inputs must yield the same paths and hashes).

.PARAMETER BundlePath
  Path to the assembled RackCad.bundle folder to verify.

.PARAMETER ExpectedVersion
  Product version the manifest must declare. Defaults to $(RackCadVersion) read from Directory.Build.props.

.PARAMETER ExpectedSeriesMin / -ExpectedSeriesMax
  AutoCAD series the manifest must declare. Default to the central RackCadAutoCADSeriesMin/Max.

.PARAMETER PropsPath
  Directory.Build.props to read the expected values from. Defaults to the repository copy next to deploy\.

.PARAMETER InventoryOutPath
  Optional file to write the "<sha256>  <relative/path>" inventory to (sorted, deterministic).

.PARAMETER BaselineInventoryPath
  Optional previous inventory file to compare against for a reproducibility check.

.EXAMPLE
  pwsh deploy\verify-bundle.ps1 -BundlePath src\RackCad.Plugin\bin\Release\net8.0-windows\publish\RackCad.bundle
#>
#Requires -Version 7.0
[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)][string]$BundlePath,
    [string]$ExpectedVersion,
    [string]$ExpectedSeriesMin,
    [string]$ExpectedSeriesMax,
    [string]$PropsPath,
    [string]$InventoryOutPath,
    [string]$BaselineInventoryPath
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$script:RackCadDlls = @(
    "RackCad.Application.dll",
    "RackCad.Domain.dll",
    "RackCad.Plugin.dll",
    "RackCad.UI.dll")
$script:CatalogExtensions = @(".csv", ".json", ".dwg")
# Matches every Autodesk assembly shipped with AutoCAD 2025 (AcCoreMgd, AcDbMgd, AcMgd, AcTcMgd,
# AdWindows, AdUiMgd, acdbmgdbrep, AcMNUParser, ...). RackCad.*.dll never matches this.
$script:AutodeskDllPattern = '(?i)^(Ac[A-Z]|Ad[A-Z]|Autodesk|acdb).+\.dll$'

$script:assertionCount = 0

function Assert-True {
    param(
        [Parameter(Mandatory = $true)][bool]$Condition,
        [Parameter(Mandatory = $true)][string]$Message)

    if (-not $Condition) {
        throw "FALLO: $Message"
    }
    $script:assertionCount++
}

function Get-RelativePath {
    param(
        [Parameter(Mandatory = $true)][string]$Root,
        [Parameter(Mandatory = $true)][string]$FullPath)

    $rootFull = [System.IO.Path]::GetFullPath($Root).TrimEnd('\', '/')
    $full = [System.IO.Path]::GetFullPath($FullPath)
    return $full.Substring($rootFull.Length + 1).Replace('\', '/')
}

function Get-BuildProperty {
    param(
        [Parameter(Mandatory = $true)][string]$PropsFile,
        [Parameter(Mandatory = $true)][string]$Name)

    if (-not (Test-Path -LiteralPath $PropsFile -PathType Leaf)) {
        throw "No se encontro Directory.Build.props para leer '$Name': $PropsFile"
    }
    [xml]$props = Get-Content -LiteralPath $PropsFile -Raw
    $node = $props.SelectSingleNode("//$Name")
    if ($null -eq $node -or [string]::IsNullOrWhiteSpace($node.InnerText)) {
        throw "Directory.Build.props no define '$Name': $PropsFile"
    }
    return $node.InnerText.Trim()
}

function Resolve-ExpectedValues {
    if (-not $PropsPath) {
        $PropsPath = Join-Path (Split-Path -Parent $PSScriptRoot) "Directory.Build.props"
    }
    if (-not $ExpectedVersion) { $script:ExpectedVersion = Get-BuildProperty $PropsPath "RackCadVersion" }
    else { $script:ExpectedVersion = $ExpectedVersion }
    if (-not $ExpectedSeriesMin) { $script:ExpectedSeriesMin = Get-BuildProperty $PropsPath "RackCadAutoCADSeriesMin" }
    else { $script:ExpectedSeriesMin = $ExpectedSeriesMin }
    if (-not $ExpectedSeriesMax) { $script:ExpectedSeriesMax = Get-BuildProperty $PropsPath "RackCadAutoCADSeriesMax" }
    else { $script:ExpectedSeriesMax = $ExpectedSeriesMax }
}

function Assert-Structure {
    param([Parameter(Mandatory = $true)][string]$Root)

    Assert-True (Test-Path -LiteralPath $Root -PathType Container) "El bundle debe ser una carpeta: $Root"
    Assert-True (Test-Path -LiteralPath (Join-Path $Root "PackageContents.xml") -PathType Leaf) `
        "Falta PackageContents.xml en la raiz del bundle"
    Assert-True (Test-Path -LiteralPath (Join-Path $Root "Contents") -PathType Container) `
        "Falta la carpeta Contents"
    Assert-True (Test-Path -LiteralPath (Join-Path $Root "Contents\catalogs") -PathType Container) `
        "Falta la carpeta Contents\catalogs"
}

function Assert-Allowlist {
    param([Parameter(Mandatory = $true)][string]$Root)

    # Fail-closed: classify EVERY file; anything outside the allowlist is a violation.
    foreach ($file in @(Get-ChildItem -LiteralPath $Root -Recurse -File -Force)) {
        $rel = Get-RelativePath -Root $Root -FullPath $file.FullName
        Assert-True (-not ($file.Name -match $script:AutodeskDllPattern)) `
            "Archivo Autodesk prohibido en el bundle (ADR-0003): $rel"

        $allowed = $false
        if ($rel -ieq "PackageContents.xml") {
            $allowed = $true
        }
        elseif ($rel -imatch '^Contents/([^/]+\.dll)$') {
            $allowed = $script:RackCadDlls -contains $Matches[1]
        }
        elseif ($rel -imatch '^Contents/catalogs/') {
            $allowed = $script:CatalogExtensions -contains $file.Extension.ToLowerInvariant()
        }
        Assert-True $allowed "Archivo no autorizado en el bundle (solo RackCad + datos permitidos): $rel"
    }

    # Every one of the four RackCad DLLs must be present.
    foreach ($dll in $script:RackCadDlls) {
        Assert-True (Test-Path -LiteralPath (Join-Path $Root "Contents\$dll") -PathType Leaf) `
            "Falta el ensamblado RackCad en el bundle: Contents\$dll"
    }

    # There must be no *.dll under Contents beyond the four RackCad assemblies.
    $dllNames = @(Get-ChildItem -LiteralPath (Join-Path $Root "Contents") -Recurse -File -Filter *.dll |
        Select-Object -ExpandProperty Name | Sort-Object)
    $expectedDlls = @($script:RackCadDlls | Sort-Object)
    Assert-True (@(Compare-Object $expectedDlls $dllNames).Count -eq 0) `
        "El contrato de DLLs del bundle cambio (deben ser exactamente los cuatro de RackCad)"

    # At least one product catalog must ship.
    $catalogs = @(Get-ChildItem -LiteralPath (Join-Path $Root "Contents\catalogs") -File |
        Where-Object { $_.Extension -in @(".csv", ".json") })
    Assert-True ($catalogs.Count -gt 0) "El bundle no contiene catalogos CSV/JSON de producto"
}

function Assert-Manifest {
    param([Parameter(Mandatory = $true)][string]$Root)

    [xml]$manifest = Get-Content -LiteralPath (Join-Path $Root "PackageContents.xml") -Raw
    $package = $manifest.ApplicationPackage
    Assert-True ($null -ne $package) "PackageContents.xml no es un ApplicationPackage valido"
    Assert-True ([string]$package.Name -eq "RackCad") "El manifiesto debe declarar Name=RackCad"
    Assert-True ([string]$package.AppVersion -eq $script:ExpectedVersion) `
        "AppVersion del manifiesto ($([string]$package.AppVersion)) != version central ($script:ExpectedVersion)"

    $requirements = $package.Components.RuntimeRequirements
    Assert-True ([string]$requirements.SeriesMin -eq $script:ExpectedSeriesMin) `
        "SeriesMin del manifiesto ($([string]$requirements.SeriesMin)) != $script:ExpectedSeriesMin (ADR-0004)"
    Assert-True ([string]$requirements.SeriesMax -eq $script:ExpectedSeriesMax) `
        "SeriesMax del manifiesto ($([string]$requirements.SeriesMax)) != $script:ExpectedSeriesMax (ADR-0004)"

    $entry = $package.Components.ComponentEntry
    Assert-True ([string]$entry.Version -eq $script:ExpectedVersion) `
        "ComponentEntry Version ($([string]$entry.Version)) != version central ($script:ExpectedVersion)"
    $moduleName = [string]$entry.ModuleName
    Assert-True ($moduleName -eq "./Contents/RackCad.Plugin.dll") `
        "ModuleName inesperado en el manifiesto: $moduleName"
    $modulePath = Join-Path $Root ($moduleName -replace '^\./', '' -replace '/', '\')
    Assert-True (Test-Path -LiteralPath $modulePath -PathType Leaf) `
        "ModuleName apunta a un archivo ausente: $moduleName"
}

function Get-Inventory {
    param([Parameter(Mandatory = $true)][string]$Root)

    $lines = @()
    foreach ($file in @(Get-ChildItem -LiteralPath $Root -Recurse -File -Force | Sort-Object FullName)) {
        $rel = Get-RelativePath -Root $Root -FullPath $file.FullName
        $hash = (Get-FileHash -LiteralPath $file.FullName -Algorithm SHA256).Hash.ToLowerInvariant()
        $lines += "$hash  $rel"
    }
    return @($lines | Sort-Object)
}

function Compare-Inventory {
    param(
        [Parameter(Mandatory = $true)][string[]]$Current,
        [Parameter(Mandatory = $true)][string]$BaselinePath)

    Assert-True (Test-Path -LiteralPath $BaselinePath -PathType Leaf) `
        "No se encontro el inventario base para reproducibilidad: $BaselinePath"
    $baseline = @(Get-Content -LiteralPath $BaselinePath | Where-Object { $_.Trim() } | Sort-Object)
    $diff = @(Compare-Object $baseline $Current)
    if ($diff.Count -ne 0) {
        Write-Host "Diferencias de reproducibilidad (< base, > actual):"
        $diff | ForEach-Object { Write-Host ("  {0} {1}" -f $_.SideIndicator, $_.InputObject) }
    }
    Assert-True ($diff.Count -eq 0) `
        "El bundle no es reproducible: el inventario o los hashes difieren del base"
}

function Invoke-VerifyBundle {
    $root = [System.IO.Path]::GetFullPath($BundlePath)
    Resolve-ExpectedValues
    Write-Host "Verificando bundle: $root"
    Write-Host "Esperado: version=$script:ExpectedVersion, SeriesMin=$script:ExpectedSeriesMin, SeriesMax=$script:ExpectedSeriesMax"

    Assert-Structure -Root $root
    Assert-Allowlist -Root $root
    Assert-Manifest -Root $root

    $inventory = Get-Inventory -Root $root
    Write-Host ""
    Write-Host "Inventario del bundle (sha256  ruta):"
    $inventory | ForEach-Object { Write-Host "  $_" }

    if ($InventoryOutPath) {
        $inventoryFull = [System.IO.Path]::GetFullPath($InventoryOutPath)
        New-Item -ItemType Directory -Force -Path (Split-Path -Parent $inventoryFull) | Out-Null
        Set-Content -LiteralPath $inventoryFull -Value $inventory -Encoding utf8
        Write-Host "Inventario escrito en: $inventoryFull"
    }

    if ($BaselineInventoryPath) {
        Compare-Inventory -Current $inventory -BaselinePath $BaselineInventoryPath
        Write-Host "Reproducibilidad: el inventario y los hashes coinciden con el base."
    }

    Write-Host ""
    Write-Host "OK: bundle verificado ($script:assertionCount comprobaciones). Solo se distribuyen archivos RackCad y datos permitidos; cero DLL Autodesk."
}

# Dot-sourcing exposes the functions to a harness without verifying a real bundle.
if ($MyInvocation.InvocationName -ne '.') {
    try {
        Invoke-VerifyBundle
    }
    catch {
        Write-Error "Verificacion del bundle fallida. $($_.Exception.Message)"
        exit 1
    }
}
