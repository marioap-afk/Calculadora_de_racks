<#
.SYNOPSIS
  Build (optional) and install the RackCad Autoloader bundle so AutoCAD loads RackCad
  automatically at startup — no NETLOAD.

.DESCRIPTION
  Installs the assembled RackCad.bundle into the per-user ApplicationPlugins folder
  (%AppData%\Autodesk\ApplicationPlugins\) using staging plus rollback. Product files,
  including CSV/JSON catalogs, come from the new bundle. An existing
  Contents\catalogs\blocks-library.dwg is preserved byte-for-byte.

  Requires AutoCAD 2025+ (.NET 8). Close AutoCAD before running: it locks the plugin
  DLL and an update cannot be made safely while it is open.

.PARAMETER Configuration
  Build configuration to take the bundle from: Debug or Release (default: Release).

.PARAMETER Build
  Also run `dotnet publish -c <Configuration>` (the canonical bundle flow) before installing.

.PARAMETER SourceBundlePath
  Optional assembled bundle path. Intended for controlled deployment and tests; by
  default it is resolved from the Plugin publish output for the selected configuration.

.PARAMETER TargetBundlePath
  Optional installation path. By default it is the per-user ApplicationPlugins path.

.EXAMPLE
  pwsh deploy\install-bundle.ps1 -Build
#>
param(
    [ValidateSet("Debug", "Release")]
    [string]$Configuration = "Release",
    [switch]$Build,
    [string]$SourceBundlePath,
    [string]$TargetBundlePath
)

function Get-CanonicalPath {
    param([Parameter(Mandatory = $true)][string]$Path)

    return [System.IO.Path]::GetFullPath($Path)
}

function Test-PathContains {
    param(
        [Parameter(Mandatory = $true)][string]$Parent,
        [Parameter(Mandatory = $true)][string]$Candidate
    )

    $separator = [System.IO.Path]::DirectorySeparatorChar
    $parentPrefix = $Parent.TrimEnd('\', '/') + $separator
    return $Candidate.StartsWith($parentPrefix, [System.StringComparison]::OrdinalIgnoreCase)
}

function Assert-SafeInstallPaths {
    param(
        [Parameter(Mandatory = $true)][string]$Source,
        [Parameter(Mandatory = $true)][string]$Target
    )

    if ([string]::Equals($Source, $Target, [System.StringComparison]::OrdinalIgnoreCase) -or
        (Test-PathContains -Parent $Source -Candidate $Target) -or
        (Test-PathContains -Parent $Target -Candidate $Source)) {
        throw "El origen y el destino del bundle no pueden coincidir ni contenerse entre si."
    }

    $targetRoot = [System.IO.Path]::GetPathRoot($Target)
    if ([string]::Equals(
            $Target.TrimEnd('\', '/'),
            $targetRoot.TrimEnd('\', '/'),
            [System.StringComparison]::OrdinalIgnoreCase)) {
        throw "El destino no puede ser la raiz de una unidad: $Target"
    }

    if (-not [System.IO.Path]::GetFileName($Target).EndsWith(
            ".bundle", [System.StringComparison]::OrdinalIgnoreCase)) {
        throw "El destino debe ser una carpeta con extension .bundle: $Target"
    }
}

function Assert-RackCadBundle {
    param([Parameter(Mandatory = $true)][string]$Path)

    if (-not (Test-Path -LiteralPath $Path -PathType Container)) {
        throw "No se encontro el bundle: $Path"
    }

    $requiredFiles = @(
        "PackageContents.xml",
        "Contents\RackCad.Plugin.dll",
        "Contents\RackCad.Application.dll",
        "Contents\RackCad.Domain.dll",
        "Contents\RackCad.UI.dll"
    )

    foreach ($relativePath in $requiredFiles) {
        $requiredPath = Join-Path $Path $relativePath
        if (-not (Test-Path -LiteralPath $requiredPath -PathType Leaf)) {
            throw "El bundle esta incompleto; falta: $requiredPath"
        }
    }

    $catalogsPath = Join-Path $Path "Contents\catalogs"
    if (-not (Test-Path -LiteralPath $catalogsPath -PathType Container)) {
        throw "El bundle esta incompleto; falta la carpeta de catalogos: $catalogsPath"
    }

    $catalogFiles = @(Get-ChildItem -LiteralPath $catalogsPath -File -ErrorAction Stop |
        Where-Object { $_.Extension -in @(".csv", ".json") })
    if ($catalogFiles.Count -eq 0) {
        throw "El bundle no contiene catalogos CSV/JSON: $catalogsPath"
    }
}

function Get-FileSha256 {
    param([Parameter(Mandatory = $true)][string]$Path)

    return (Get-FileHash -LiteralPath $Path -Algorithm SHA256 -ErrorAction Stop).Hash
}

function Remove-OperationDirectory {
    param(
        [Parameter(Mandatory = $true)][string]$Path,
        [Parameter(Mandatory = $true)][string]$ExpectedParent,
        [Parameter(Mandatory = $true)][string]$OperationId
    )

    if (-not (Test-Path -LiteralPath $Path)) {
        return
    }

    $actualParent = Split-Path -Parent (Get-CanonicalPath $Path)
    $leaf = Split-Path -Leaf $Path
    if (-not [string]::Equals(
            $actualParent,
            $ExpectedParent,
            [System.StringComparison]::OrdinalIgnoreCase) -or
        -not $leaf.EndsWith($OperationId, [System.StringComparison]::OrdinalIgnoreCase)) {
        throw "Se rechazo borrar una carpeta temporal fuera de la operacion actual: $Path"
    }

    Remove-Item -LiteralPath $Path -Recurse -Force -ErrorAction Stop
}

function Invoke-RackCadBundleInstall {
    param(
        [Parameter(Mandatory = $true)][string]$Source,
        [Parameter(Mandatory = $true)][string]$Target,
        [scriptblock]$BeforeActivate
    )

    $ErrorActionPreference = "Stop"
    $sourceFull = Get-CanonicalPath $Source
    $targetFull = Get-CanonicalPath $Target
    Assert-SafeInstallPaths -Source $sourceFull -Target $targetFull
    Assert-RackCadBundle -Path $sourceFull

    if (Test-Path -LiteralPath $targetFull) {
        if (-not (Test-Path -LiteralPath $targetFull -PathType Container)) {
            throw "El destino existe pero no es una carpeta: $targetFull"
        }
    }

    $targetParent = Split-Path -Parent $targetFull
    New-Item -ItemType Directory -Force -Path $targetParent | Out-Null

    $operationId = [guid]::NewGuid().ToString("N")
    $targetName = Split-Path -Leaf $targetFull
    $stage = Join-Path $targetParent ".$targetName.stage-$operationId"
    $backup = Join-Path $targetParent ".$targetName.backup-$operationId"
    $failed = Join-Path $targetParent ".$targetName.failed-$operationId"
    $libraryRelativePath = "Contents\catalogs\blocks-library.dwg"
    $existingLibrary = Join-Path $targetFull $libraryRelativePath
    $stagedLibrary = Join-Path $stage $libraryRelativePath

    $backupCreated = $false
    $activated = $false
    $preservedLibraryHash = $null

    Write-Host "Preparando bundle nuevo en: $stage"

    try {
        New-Item -ItemType Directory -Path $stage | Out-Null
        Get-ChildItem -LiteralPath $sourceFull -Force |
            Copy-Item -Destination $stage -Recurse -Force -ErrorAction Stop
        Assert-RackCadBundle -Path $stage

        if (Test-Path -LiteralPath $existingLibrary) {
            if (-not (Test-Path -LiteralPath $existingLibrary -PathType Leaf)) {
                throw "La ruta de la biblioteca existente no es un archivo: $existingLibrary"
            }

            $preservedLibraryHash = Get-FileSha256 -Path $existingLibrary
            Write-Host "Preservando biblioteca de bloques: $existingLibrary"
            Copy-Item -LiteralPath $existingLibrary -Destination $stagedLibrary -Force -ErrorAction Stop
            if ((Get-FileSha256 -Path $stagedLibrary) -ne $preservedLibraryHash) {
                throw "La biblioteca preservada no coincide con el archivo original."
            }
        }

        if (Test-Path -LiteralPath $targetFull) {
            Write-Host "Creando respaldo recuperable en: $backup"
            Move-Item -LiteralPath $targetFull -Destination $backup -ErrorAction Stop
            $backupCreated = $true
        }

        if ($null -ne $BeforeActivate) {
            & $BeforeActivate
        }

        Write-Host "Activando bundle nuevo en: $targetFull"
        Move-Item -LiteralPath $stage -Destination $targetFull -ErrorAction Stop
        $activated = $true

        Assert-RackCadBundle -Path $targetFull
        if ($null -ne $preservedLibraryHash) {
            $installedLibrary = Join-Path $targetFull $libraryRelativePath
            if (-not (Test-Path -LiteralPath $installedLibrary -PathType Leaf) -or
                (Get-FileSha256 -Path $installedLibrary) -ne $preservedLibraryHash) {
                throw "La verificacion final de blocks-library.dwg fallo."
            }
            Write-Host "Biblioteca restaurada y verificada."
        }

        if ($backupCreated) {
            try {
                Remove-OperationDirectory -Path $backup -ExpectedParent $targetParent -OperationId $operationId
                $backupCreated = $false
                Write-Host "Respaldo temporal eliminado despues de verificar la instalacion."
            }
            catch {
                Write-Warning "La instalacion termino correctamente, pero no se pudo borrar el respaldo: $backup"
                Write-Warning $_.Exception.Message
            }
        }

        Write-Host "RackCad instalado en: $targetFull"
        Write-Host "Los catalogos CSV/JSON corresponden al bundle nuevo; no se fusionan con la instalacion anterior."
        Write-Host "Abre AutoCAD 2025+; los comandos quedan disponibles sin NETLOAD."
    }
    catch {
        $installError = $_
        Write-Warning "La instalacion no se completo: $($installError.Exception.Message)"

        try {
            if ($activated -and (Test-Path -LiteralPath $targetFull)) {
                Write-Warning "Apartando el bundle fallido en: $failed"
                Move-Item -LiteralPath $targetFull -Destination $failed -ErrorAction Stop
                $activated = $false
            }

            if ($backupCreated -and -not (Test-Path -LiteralPath $targetFull)) {
                Write-Host "Restaurando la instalacion anterior desde: $backup"
                Move-Item -LiteralPath $backup -Destination $targetFull -ErrorAction Stop
                $backupCreated = $false
                Write-Host "La instalacion anterior fue restaurada."
            }
        }
        catch {
            Write-Warning "El rollback automatico fallo: $($_.Exception.Message)"
        }

        if (Test-Path -LiteralPath $backup) {
            Write-Warning "Respaldo recuperable conservado en: $backup"
        }
        if (Test-Path -LiteralPath $failed) {
            Write-Warning "Bundle fallido conservado para diagnostico en: $failed"
        }

        throw $installError
    }
    finally {
        if (Test-Path -LiteralPath $stage) {
            try {
                Remove-OperationDirectory -Path $stage -ExpectedParent $targetParent -OperationId $operationId
            }
            catch {
                Write-Warning "No se pudo limpiar el staging: $stage"
            }
        }
    }
}

function Assert-AutoCadClosed {
    if (Get-Process -Name acad -ErrorAction SilentlyContinue) {
        throw "AutoCAD esta abierto. Cierralo antes de instalar o actualizar RackCad."
    }
}

function Resolve-DotNetExecutable {
    param([Parameter(Mandatory = $true)][string]$RepositoryRoot)

    $candidates = @(Get-Command dotnet -All -CommandType Application -ErrorAction SilentlyContinue |
        Select-Object -ExpandProperty Source -Unique)
    # Fall back to the per-user SDK and the machine-wide install when dotnet is not on PATH
    # (the documented RackCad setup builds via %LOCALAPPDATA%\Microsoft\dotnet).
    if ($env:LOCALAPPDATA) { $candidates += (Join-Path $env:LOCALAPPDATA "Microsoft\dotnet\dotnet.exe") }
    if (${env:ProgramFiles}) { $candidates += (Join-Path ${env:ProgramFiles} "dotnet\dotnet.exe") }

    foreach ($command in ($candidates | Where-Object { $_ } | Select-Object -Unique)) {
        if (-not (Test-Path -LiteralPath $command -PathType Leaf) -and
            -not (Get-Command $command -ErrorAction SilentlyContinue)) {
            continue
        }
        Push-Location $RepositoryRoot
        try {
            & $command --version *> $null
            if ($LASTEXITCODE -eq 0) {
                return $command
            }
        }
        catch { }
        finally {
            Pop-Location
        }
    }

    throw "No se encontro un dotnet compatible con el SDK fijado por global.json."
}

function Invoke-InstallBundleScript {
    $ErrorActionPreference = "Stop"
    $repo = Split-Path -Parent $PSScriptRoot

    Assert-AutoCadClosed

    if ($Build) {
        $dotnet = Resolve-DotNetExecutable -RepositoryRoot $repo
        Write-Host "Publicando RackCad ($Configuration) con: $dotnet"
        & $dotnet publish (Join-Path $repo "src\RackCad.Plugin\RackCad.Plugin.csproj") -c $Configuration
        if ($LASTEXITCODE -ne 0) {
            throw "El publish fallo con codigo $LASTEXITCODE."
        }
    }

    $source = if ([string]::IsNullOrWhiteSpace($SourceBundlePath)) {
        Join-Path $repo "src\RackCad.Plugin\bin\$Configuration\net8.0-windows\publish\RackCad.bundle"
    }
    else {
        $SourceBundlePath
    }

    $target = if ([string]::IsNullOrWhiteSpace($TargetBundlePath)) {
        if ([string]::IsNullOrWhiteSpace($env:APPDATA)) {
            throw "APPDATA no esta definido; especifica TargetBundlePath."
        }
        Join-Path $env:APPDATA "Autodesk\ApplicationPlugins\RackCad.bundle"
    }
    else {
        $TargetBundlePath
    }

    Invoke-RackCadBundleInstall -Source $source -Target $target -BeforeActivate {
        Assert-AutoCadClosed
    }
}

# Dot-sourcing exposes the transaction function to the reproducible harness without
# running a real installation. Normal invocation executes the installer.
if ($MyInvocation.InvocationName -ne '.') {
    try {
        Invoke-InstallBundleScript
    }
    catch {
        Write-Error "RackCad no se instalo. $($_.Exception.Message)"
        throw
    }
}
