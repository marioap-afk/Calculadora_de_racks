<#
.SYNOPSIS
  Persistent regression harness for deploy\verify-bundle.ps1 (I-12).

.DESCRIPTION
  Takes one real, valid RackCad.bundle plus its publish output and the authorized catalog source,
  then verifies that verify-bundle.ps1 ACCEPTS the untouched bundle and REJECTS every tampering,
  each for the right reason:
    - a valid bundle;
    - an Autodesk DLL smuggled into Contents;
    - a modified RackCad DLL (no longer matching the publish copy);
    - an extra catalog with an allowed extension;
    - a missing authorized catalog;
    - a modified catalog (no longer matching assets\catalogs);
    - a catalog in an unexpected sub-folder;
    - a disallowed file;
    - a wrong manifest version and a wrong AutoCAD series.
  Every case works on a throwaway COPY under %TEMP%; the real bundle, publish and repo are never
  mutated. The CI guard (eng\ci\verify-autocad-references.ps1) runs this after publishing, so the
  cases are covered in CI without editing .github\workflows\ci.yml.

.PARAMETER SourceBundle
  A real, valid RackCad.bundle to clone for each case.

.PARAMETER PublishDir
  Publish output that produced SourceBundle (the DLL SHA-256 source of truth).

.PARAMETER VerifyScript
  Path to verify-bundle.ps1. Defaults to the copy next to this script.

.PARAMETER CatalogsSourceDir
  Authorized catalog source. Defaults to assets\catalogs in the repository.

.PARAMETER PropsPath
  Directory.Build.props for the expected version/series. Defaults to the repository copy.
#>
#Requires -Version 7.0
[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)][string]$SourceBundle,
    [Parameter(Mandatory = $true)][string]$PublishDir,
    [string]$VerifyScript,
    [string]$CatalogsSourceDir,
    [string]$PropsPath
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

if (-not $VerifyScript) { $VerifyScript = Join-Path $PSScriptRoot "verify-bundle.ps1" }
if (-not $CatalogsSourceDir) { $CatalogsSourceDir = Join-Path (Split-Path -Parent $PSScriptRoot) "assets\catalogs" }
if (-not $PropsPath) { $PropsPath = Join-Path (Split-Path -Parent $PSScriptRoot) "Directory.Build.props" }

foreach ($required in @($SourceBundle, $PublishDir, $VerifyScript, $CatalogsSourceDir, $PropsPath)) {
    if (-not (Test-Path -LiteralPath $required)) {
        throw "No se encontro una entrada requerida del harness: $required"
    }
}

$script:pwshExe = [System.Environment]::ProcessPath
$script:passed = 0

function Invoke-Verify {
    param(
        [Parameter(Mandatory = $true)][string]$BundlePath,
        [AllowEmptyCollection()][string[]]$ExtraArgs = @())

    $arguments = @(
        "-NoProfile", "-File", $VerifyScript,
        "-BundlePath", $BundlePath,
        "-PublishDir", $PublishDir,
        "-CatalogsSourceDir", $CatalogsSourceDir,
        "-PropsPath", $PropsPath) + $ExtraArgs
    $output = @(& $script:pwshExe @arguments 2>&1 | ForEach-Object { [string]$_ })
    return [pscustomobject]@{ ExitCode = $LASTEXITCODE; Output = ($output -join [Environment]::NewLine) }
}

function New-BundleCopy {
    param(
        [Parameter(Mandatory = $true)][string]$Root,
        [Parameter(Mandatory = $true)][string]$Name)

    $dest = Join-Path $Root $Name
    Copy-Item -LiteralPath $SourceBundle -Destination $dest -Recurse -Force
    return $dest
}

function Assert-Pass {
    param([Parameter(Mandatory = $true)][string]$Name, [Parameter(Mandatory = $true)][string]$BundlePath)

    $result = Invoke-Verify -BundlePath $BundlePath
    if ($result.ExitCode -ne 0) {
        throw "FALLO [$Name]: se esperaba exito, exit=$($result.ExitCode).`n$($result.Output)"
    }
    $script:passed++
    Write-Host "  OK  [$Name] bundle valido aceptado (exit 0)"
}

function Assert-Reject {
    param(
        [Parameter(Mandatory = $true)][string]$Name,
        [Parameter(Mandatory = $true)][string]$BundlePath,
        [Parameter(Mandatory = $true)][string]$ExpectSubstring,
        [AllowEmptyCollection()][string[]]$ExtraArgs = @())

    $result = Invoke-Verify -BundlePath $BundlePath -ExtraArgs $ExtraArgs
    if ($result.ExitCode -eq 0) {
        throw "FALLO [$Name]: se esperaba rechazo, pero exit=0.`n$($result.Output)"
    }
    if ($result.Output -notmatch [regex]::Escape($ExpectSubstring)) {
        throw "FALLO [$Name]: rechazado (exit $($result.ExitCode)) pero sin el motivo esperado '$ExpectSubstring'.`n$($result.Output)"
    }
    $script:passed++
    Write-Host "  OK  [$Name] rechazado (exit $($result.ExitCode)): $ExpectSubstring"
}

function Add-FileBytes {
    param([Parameter(Mandatory = $true)][string]$Path)

    $bytes = [System.IO.File]::ReadAllBytes($Path)
    [System.IO.File]::WriteAllBytes($Path, ($bytes + [byte]0))
}

# A representative catalog name, derived from the real bundle (never hardcoded).
$script:sampleCatalog = (Get-ChildItem -LiteralPath (Join-Path $SourceBundle "Contents\catalogs") -File |
    Where-Object { $_.Extension -in @(".csv", ".json") } | Select-Object -First 1).Name
if (-not $script:sampleCatalog) { throw "El bundle fuente no tiene catalogos CSV/JSON para el harness." }

$temporaryParent = [System.IO.Path]::GetFullPath([System.IO.Path]::GetTempPath())
$temporaryRoot = Join-Path $temporaryParent ("RackCad verify harness " + [guid]::NewGuid().ToString("N"))
New-Item -ItemType Directory -Path $temporaryRoot | Out-Null

try {
    Write-Host "Harness del verificador en: $temporaryRoot"
    Write-Host "Catalogo de muestra: $script:sampleCatalog"

    # 1. Valid bundle is accepted.
    $b = New-BundleCopy -Root $temporaryRoot -Name "01-valido"
    Assert-Pass -Name "valido" -BundlePath $b

    # 2. Autodesk DLL smuggled in.
    $b = New-BundleCopy -Root $temporaryRoot -Name "02-autodesk-dll"
    [System.IO.File]::WriteAllBytes((Join-Path $b "Contents\AcDbMgd.dll"), [byte[]](1, 2, 3))
    Assert-Reject -Name "dll-autodesk" -BundlePath $b -ExpectSubstring "Archivo Autodesk prohibido"

    # 3. Modified RackCad DLL (no longer matches the publish copy).
    $b = New-BundleCopy -Root $temporaryRoot -Name "03-dll-modificado"
    Add-FileBytes -Path (Join-Path $b "Contents\RackCad.Domain.dll")
    Assert-Reject -Name "dll-modificado" -BundlePath $b -ExpectSubstring "no coincide (SHA-256) con su copia del publish"

    # 4. Extra catalog with an allowed extension.
    $b = New-BundleCopy -Root $temporaryRoot -Name "04-catalogo-extra"
    Set-Content -LiteralPath (Join-Path $b "Contents\catalogs\intruso.csv") -Value "col`n1" -Encoding utf8
    Assert-Reject -Name "catalogo-extra" -BundlePath $b -ExpectSubstring "Catalogo no autorizado en el bundle"

    # 5. Missing authorized catalog.
    $b = New-BundleCopy -Root $temporaryRoot -Name "05-catalogo-ausente"
    Remove-Item -LiteralPath (Join-Path $b "Contents\catalogs\$script:sampleCatalog") -Force
    Assert-Reject -Name "catalogo-ausente" -BundlePath $b -ExpectSubstring "Falta en el bundle un catalogo autorizado"

    # 6. Modified catalog (no longer matches assets\catalogs).
    $b = New-BundleCopy -Root $temporaryRoot -Name "06-catalogo-modificado"
    Add-FileBytes -Path (Join-Path $b "Contents\catalogs\$script:sampleCatalog")
    Assert-Reject -Name "catalogo-modificado" -BundlePath $b -ExpectSubstring "Catalogo modificado respecto a assets/catalogs"

    # 7. Catalog placed in an unexpected sub-folder.
    $b = New-BundleCopy -Root $temporaryRoot -Name "07-catalogo-subcarpeta"
    New-Item -ItemType Directory -Path (Join-Path $b "Contents\catalogs\sub") | Out-Null
    Copy-Item -LiteralPath (Join-Path $b "Contents\catalogs\$script:sampleCatalog") `
        -Destination (Join-Path $b "Contents\catalogs\sub\$script:sampleCatalog") -Force
    Assert-Reject -Name "catalogo-subcarpeta" -BundlePath $b -ExpectSubstring "subcarpeta inesperada"

    # 8. Disallowed file anywhere in the bundle.
    $b = New-BundleCopy -Root $temporaryRoot -Name "08-archivo-extrano"
    Set-Content -LiteralPath (Join-Path $b "Contents\notas.txt") -Value "x" -Encoding utf8
    Assert-Reject -Name "archivo-extrano" -BundlePath $b -ExpectSubstring "Archivo no autorizado en el bundle"

    # 9. Wrong manifest version.
    $b = New-BundleCopy -Root $temporaryRoot -Name "09-version-incorrecta"
    Assert-Reject -Name "version-incorrecta" -BundlePath $b -ExpectSubstring "AppVersion del manifiesto" `
        -ExtraArgs @("-ExpectedVersion", "9.9.9")

    # 10. Wrong AutoCAD series.
    $b = New-BundleCopy -Root $temporaryRoot -Name "10-series-incorrecta"
    Assert-Reject -Name "series-incorrecta" -BundlePath $b -ExpectSubstring "SeriesMax del manifiesto" `
        -ExtraArgs @("-ExpectedSeriesMax", "R26.0")

    Write-Host ""
    Write-Host "OK: harness del verificador — $script:passed casos superados (1 valido + 9 negativos)."
}
catch {
    Write-Error "Harness del verificador fallido. $($_.Exception.Message)"
    exit 1
}
finally {
    $rootFull = [System.IO.Path]::GetFullPath($temporaryRoot)
    $rootParent = Split-Path -Parent $rootFull
    $rootLeaf = Split-Path -Leaf $rootFull
    if ([string]::Equals($rootParent.TrimEnd('\', '/'), $temporaryParent.TrimEnd('\', '/'),
            [System.StringComparison]::OrdinalIgnoreCase) -and
        $rootLeaf.StartsWith("RackCad verify harness ", [System.StringComparison]::Ordinal)) {
        Remove-Item -LiteralPath $rootFull -Recurse -Force -ErrorAction SilentlyContinue
    }
}
