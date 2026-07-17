<#
.SYNOPSIS
  Reproducible, filesystem-only regression harness for install-bundle.ps1.

.DESCRIPTION
  Creates fake RackCad bundles under a unique temporary directory. It never reads or
  writes the real ApplicationPlugins installation. No Pester module is required.
#>
param(
    [string]$InstallerPath = (Join-Path $PSScriptRoot "install-bundle.ps1")
)

$ErrorActionPreference = "Stop"
$installerFullPath = [System.IO.Path]::GetFullPath($InstallerPath)
if (-not (Test-Path -LiteralPath $installerFullPath -PathType Leaf)) {
    throw "No se encontro el instalador: $installerFullPath"
}

# Dot-source only defines the installer's transaction functions; its real entry point is guarded.
. $installerFullPath

$script:assertionCount = 0

function Assert-True {
    param(
        [Parameter(Mandatory = $true)][bool]$Condition,
        [Parameter(Mandatory = $true)][string]$Message
    )

    if (-not $Condition) {
        throw "FALLO: $Message"
    }
    $script:assertionCount++
}

function Assert-Throws {
    param(
        [Parameter(Mandatory = $true)][scriptblock]$Action,
        [Parameter(Mandatory = $true)][string]$Message
    )

    $threw = $false
    try {
        & $Action
    }
    catch {
        $threw = $true
    }

    Assert-True -Condition $threw -Message $Message
}

function New-FakeBundle {
    param(
        [Parameter(Mandatory = $true)][string]$Path,
        [Parameter(Mandatory = $true)][string]$CatalogValue,
        [byte[]]$LibraryBytes
    )

    $catalogs = Join-Path $Path "Contents\catalogs"
    New-Item -ItemType Directory -Force -Path $catalogs | Out-Null
    [System.IO.File]::WriteAllText((Join-Path $Path "PackageContents.xml"), "<ApplicationPackage />")

    foreach ($assembly in @(
            "RackCad.Plugin.dll",
            "RackCad.Application.dll",
            "RackCad.Domain.dll",
            "RackCad.UI.dll")) {
        [System.IO.File]::WriteAllBytes((Join-Path $Path "Contents\$assembly"), [byte[]](1, 2, 3))
    }

    [System.IO.File]::WriteAllText((Join-Path $catalogs "blocks.csv"), $CatalogValue)
    [System.IO.File]::WriteAllText((Join-Path $catalogs "defaults.json"), "{}")
    if ($null -ne $LibraryBytes) {
        [System.IO.File]::WriteAllBytes((Join-Path $catalogs "blocks-library.dwg"), $LibraryBytes)
    }
}

function New-CasePaths {
    param(
        [Parameter(Mandatory = $true)][string]$Root,
        [Parameter(Mandatory = $true)][string]$Name
    )

    $caseRoot = Join-Path $Root $Name
    return @{
        Source = Join-Path $caseRoot "source RackCad.bundle"
        Target = Join-Path $caseRoot "Application Plugins\RackCad.bundle"
    }
}

function Assert-NoOperationDirectories {
    param([Parameter(Mandatory = $true)][string]$Target)

    $parent = Split-Path -Parent $Target
    $targetName = Split-Path -Leaf $Target
    $residue = @()
    if (Test-Path -LiteralPath $parent -PathType Container) {
        $residue = @(Get-ChildItem -LiteralPath $parent -Force |
            Where-Object { $_.Name -like ".$targetName.stage-*" -or
                $_.Name -like ".$targetName.backup-*" -or
                $_.Name -like ".$targetName.failed-*" })
    }
    Assert-True -Condition ($residue.Count -eq 0) -Message "No deben quedar carpetas temporales en $parent"
}

$temporaryParent = [System.IO.Path]::GetFullPath([System.IO.Path]::GetTempPath())
$temporaryRoot = Join-Path $temporaryParent ("RackCad install harness " + [guid]::NewGuid().ToString("N"))
New-Item -ItemType Directory -Path $temporaryRoot | Out-Null

try {
    Write-Host "Harness temporal: $temporaryRoot"

    # 1. First installation.
    $case = New-CasePaths -Root $temporaryRoot -Name "01 primera instalacion"
    New-FakeBundle -Path $case.Source -CatalogValue "producto-v1"
    Invoke-RackCadBundleInstall -Source $case.Source -Target $case.Target
    Assert-True -Condition (Test-Path -LiteralPath (Join-Path $case.Target "PackageContents.xml")) `
        -Message "La primera instalacion debe activar el bundle"
    Assert-NoOperationDirectories -Target $case.Target

    # 2. Product catalogs replace the previous installation; old-only files are removed.
    $case = New-CasePaths -Root $temporaryRoot -Name "02 catalogos de producto"
    New-FakeBundle -Path $case.Source -CatalogValue "producto-nuevo"
    New-FakeBundle -Path $case.Target -CatalogValue "catalogo-personalizado"
    [System.IO.File]::WriteAllText((Join-Path $case.Target "Contents\catalogs\obsoleto.csv"), "viejo")
    Invoke-RackCadBundleInstall -Source $case.Source -Target $case.Target
    Assert-True -Condition (([System.IO.File]::ReadAllText(
                (Join-Path $case.Target "Contents\catalogs\blocks.csv"))) -eq "producto-nuevo") `
        -Message "Los catalogos CSV/JSON deben venir del bundle nuevo"
    Assert-True -Condition (-not (Test-Path -LiteralPath (
                Join-Path $case.Target "Contents\catalogs\obsoleto.csv"))) `
        -Message "Un catalogo obsoleto no debe sobrevivir a la actualizacion"
    Assert-NoOperationDirectories -Target $case.Target

    # 3. Existing blocks-library.dwg wins over an incoming library and remains byte-identical.
    $case = New-CasePaths -Root $temporaryRoot -Name "03 biblioteca personalizada"
    $existingLibraryBytes = [byte[]](82, 97, 99, 107, 67, 97, 100, 0, 255, 17)
    New-FakeBundle -Path $case.Source -CatalogValue "producto-nuevo" -LibraryBytes ([byte[]](9, 9, 9))
    New-FakeBundle -Path $case.Target -CatalogValue "producto-viejo" -LibraryBytes $existingLibraryBytes
    $expectedLibraryHash = Get-FileSha256 -Path (Join-Path $case.Target "Contents\catalogs\blocks-library.dwg")
    Invoke-RackCadBundleInstall -Source $case.Source -Target $case.Target
    $actualLibraryHash = Get-FileSha256 -Path (Join-Path $case.Target "Contents\catalogs\blocks-library.dwg")
    Assert-True -Condition ($actualLibraryHash -eq $expectedLibraryHash) `
        -Message "blocks-library.dwg debe preservarse byte por byte"
    Assert-True -Condition (([System.IO.File]::ReadAllText(
                (Join-Path $case.Target "Contents\catalogs\blocks.csv"))) -eq "producto-nuevo") `
        -Message "Preservar el DWG no debe preservar los CSV antiguos"
    Assert-NoOperationDirectories -Target $case.Target

    # 4. A partial destination is repaired while preserving its library.
    $case = New-CasePaths -Root $temporaryRoot -Name "04 destino parcial"
    New-FakeBundle -Path $case.Source -CatalogValue "producto-completo"
    $partialCatalogs = Join-Path $case.Target "Contents\catalogs"
    New-Item -ItemType Directory -Force -Path $partialCatalogs | Out-Null
    [System.IO.File]::WriteAllBytes((Join-Path $partialCatalogs "blocks-library.dwg"), [byte[]](7, 6, 5, 4))
    $partialLibraryHash = Get-FileSha256 -Path (Join-Path $partialCatalogs "blocks-library.dwg")
    Invoke-RackCadBundleInstall -Source $case.Source -Target $case.Target
    Assert-RackCadBundle -Path $case.Target
    Assert-True -Condition ((Get-FileSha256 -Path (
                Join-Path $partialCatalogs "blocks-library.dwg")) -eq $partialLibraryHash) `
        -Message "La biblioteca de un destino parcial debe recuperarse"
    Assert-NoOperationDirectories -Target $case.Target

    # 5. A locked library fails before the old installation is moved.
    $case = New-CasePaths -Root $temporaryRoot -Name "05 archivo bloqueado"
    New-FakeBundle -Path $case.Source -CatalogValue "producto-nuevo"
    New-FakeBundle -Path $case.Target -CatalogValue "producto-anterior" -LibraryBytes ([byte[]](1, 3, 3, 7))
    $lockedLibrary = Join-Path $case.Target "Contents\catalogs\blocks-library.dwg"
    $stream = [System.IO.File]::Open(
        $lockedLibrary,
        [System.IO.FileMode]::Open,
        [System.IO.FileAccess]::Read,
        [System.IO.FileShare]::None)
    try {
        Assert-Throws -Action {
            Invoke-RackCadBundleInstall -Source $case.Source -Target $case.Target
        } -Message "Una biblioteca bloqueada debe hacer fallar la instalacion"
    }
    finally {
        $stream.Dispose()
    }
    Assert-True -Condition (([System.IO.File]::ReadAllText(
                (Join-Path $case.Target "Contents\catalogs\blocks.csv"))) -eq "producto-anterior") `
        -Message "Un fallo antes del cambio debe dejar intacta la instalacion anterior"
    Assert-NoOperationDirectories -Target $case.Target

    # 6. Deterministic failure after backup exercises automatic rollback.
    $case = New-CasePaths -Root $temporaryRoot -Name "06 rollback"
    New-FakeBundle -Path $case.Source -CatalogValue "producto-nuevo"
    New-FakeBundle -Path $case.Target -CatalogValue "producto-anterior" -LibraryBytes ([byte[]](4, 2))
    $rollbackLibraryHash = Get-FileSha256 -Path (
        Join-Path $case.Target "Contents\catalogs\blocks-library.dwg")
    Assert-Throws -Action {
        Invoke-RackCadBundleInstall -Source $case.Source -Target $case.Target -BeforeActivate {
            throw "Fallo inyectado despues del respaldo"
        }
    } -Message "El fallo posterior al respaldo debe propagarse"
    Assert-True -Condition (([System.IO.File]::ReadAllText(
                (Join-Path $case.Target "Contents\catalogs\blocks.csv"))) -eq "producto-anterior") `
        -Message "El rollback debe restaurar los archivos de producto anteriores"
    Assert-True -Condition ((Get-FileSha256 -Path (
                Join-Path $case.Target "Contents\catalogs\blocks-library.dwg")) -eq $rollbackLibraryHash) `
        -Message "El rollback debe conservar la biblioteca anterior"
    Assert-NoOperationDirectories -Target $case.Target

    # 7. Consecutive execution is idempotent.
    $case = New-CasePaths -Root $temporaryRoot -Name "07 segunda ejecucion"
    New-FakeBundle -Path $case.Source -CatalogValue "producto-estable"
    Invoke-RackCadBundleInstall -Source $case.Source -Target $case.Target
    Invoke-RackCadBundleInstall -Source $case.Source -Target $case.Target
    Assert-True -Condition (([System.IO.File]::ReadAllText(
                (Join-Path $case.Target "Contents\catalogs\blocks.csv"))) -eq "producto-estable") `
        -Message "La segunda ejecucion debe producir el mismo contenido"
    Assert-NoOperationDirectories -Target $case.Target

    # 8. Incomplete source is rejected before touching the destination.
    $case = New-CasePaths -Root $temporaryRoot -Name "08 origen incompleto"
    New-Item -ItemType Directory -Force -Path $case.Source | Out-Null
    New-FakeBundle -Path $case.Target -CatalogValue "instalacion-valida"
    Assert-Throws -Action {
        Invoke-RackCadBundleInstall -Source $case.Source -Target $case.Target
    } -Message "Un origen incompleto debe rechazarse"
    Assert-True -Condition (([System.IO.File]::ReadAllText(
                (Join-Path $case.Target "Contents\catalogs\blocks.csv"))) -eq "instalacion-valida") `
        -Message "Rechazar el origen no debe tocar el destino"

    # 9. The public script entry point accepts explicit safe paths and returns success.
    $case = New-CasePaths -Root $temporaryRoot -Name "09 punto de entrada"
    New-FakeBundle -Path $case.Source -CatalogValue "producto-entrypoint"
    $powerShell = [System.Environment]::ProcessPath
    Assert-True -Condition (-not [string]::IsNullOrWhiteSpace($powerShell)) `
        -Message "Debe poder resolverse el ejecutable del proceso PowerShell actual"
    & $powerShell -NoProfile -File $installerFullPath `
        -SourceBundlePath $case.Source -TargetBundlePath $case.Target
    Assert-True -Condition ($LASTEXITCODE -eq 0) `
        -Message "El punto de entrada publico debe terminar con codigo cero"
    Assert-True -Condition (([System.IO.File]::ReadAllText(
                (Join-Path $case.Target "Contents\catalogs\blocks.csv"))) -eq "producto-entrypoint") `
        -Message "El punto de entrada debe instalar el origen indicado"
    Assert-NoOperationDirectories -Target $case.Target

    Write-Host "OK: $script:assertionCount verificaciones superadas."
}
finally {
    $rootFull = [System.IO.Path]::GetFullPath($temporaryRoot)
    $rootParent = Split-Path -Parent $rootFull
    $rootLeaf = Split-Path -Leaf $rootFull
    if ([string]::Equals(
            $rootParent.TrimEnd('\', '/'),
            $temporaryParent.TrimEnd('\', '/'),
            [System.StringComparison]::OrdinalIgnoreCase) -and
        $rootLeaf.StartsWith("RackCad install harness ", [System.StringComparison]::Ordinal)) {
        Remove-Item -LiteralPath $rootFull -Recurse -Force -ErrorAction SilentlyContinue
    }
    else {
        Write-Warning "Se rechazo limpiar una ruta temporal inesperada: $rootFull"
    }
}
