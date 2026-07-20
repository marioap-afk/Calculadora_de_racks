#Requires -Version 7.0

[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [string]$RepositoryRoot,

    [Parameter(Mandatory)]
    [string]$NuGetPackages,

    [Parameter(Mandatory)]
    [string]$EmptyAutoCADDir,

    [Parameter(Mandatory)]
    [string]$NuGetConfig,

    [Parameter(Mandatory)]
    [string]$LockFile
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$script:ExpectedPackages = [ordered]@{
    "AutoCAD.NET" = "25.0.1"
    "AutoCAD.NET.Core" = "25.0.0"
    "AutoCAD.NET.Model" = "25.0.0"
}
$script:ExpectedPrincipalAssemblies = [ordered]@{
    "AcCoreMgd.dll" = "AutoCAD.NET.Core"
    "AcDbMgd.dll" = "AutoCAD.NET.Model"
    "AcMgd.dll" = "AutoCAD.NET"
}
$script:NuGetOrg = "https://api.nuget.org/v3/index.json"

function Get-CanonicalPath {
    param([Parameter(Mandatory)][string]$Path)

    return [IO.Path]::GetFullPath($Path).TrimEnd([char[]]@('\', '/'))
}

function Test-SamePath {
    param(
        [Parameter(Mandatory)][string]$Left,
        [Parameter(Mandatory)][string]$Right
    )

    return [string]::Equals(
        (Get-CanonicalPath $Left),
        (Get-CanonicalPath $Right),
        [StringComparison]::OrdinalIgnoreCase)
}

function Test-PathWithin {
    param(
        [Parameter(Mandatory)][string]$Candidate,
        [Parameter(Mandatory)][string]$Root
    )

    $candidatePath = Get-CanonicalPath $Candidate
    $rootPath = Get-CanonicalPath $Root
    return $candidatePath.StartsWith(
        $rootPath + [IO.Path]::DirectorySeparatorChar,
        [StringComparison]::OrdinalIgnoreCase)
}

function Get-SafePath {
    param([Parameter(Mandatory)][string]$Path)

    $safe = $Path
    $replacementRoots = [ordered]@{}
    $replacementRoots[(Get-CanonicalPath $RepositoryRoot)] = "<REPOSITORY_ROOT>"
    $replacementRoots[(Get-CanonicalPath $NuGetPackages)] = "<NUGET_PACKAGES>"
    if ($env:RUNNER_TEMP) {
        $replacementRoots[(Get-CanonicalPath $env:RUNNER_TEMP)] = "<RUNNER_TEMP>"
    }
    foreach ($replacement in $replacementRoots.GetEnumerator()) {
        $safe = $safe.Replace($replacement.Key, $replacement.Value, [StringComparison]::OrdinalIgnoreCase)
    }
    return $safe
}

function Split-PropertyList {
    param([AllowEmptyString()][string]$Value)

    return @($Value -split ';' | ForEach-Object { $_.Trim() } | Where-Object { $_ })
}

function Invoke-DotNet {
    param([Parameter(Mandatory)][string[]]$Arguments)

    $output = @(& dotnet @Arguments 2>&1 | ForEach-Object { [string]$_ })
    return [pscustomobject]@{
        ExitCode = $LASTEXITCODE
        Output = $output
    }
}

function Assert-DotNetSuccess {
    param(
        [Parameter(Mandatory)][pscustomobject]$Result,
        [Parameter(Mandatory)][string]$Operation,
        [switch]$NoWarnings
    )

    if ($Result.ExitCode -ne 0) {
        $Result.Output | Select-Object -Last 50 | ForEach-Object { Write-Host $_ }
        throw "$Operation failed with exit code $($Result.ExitCode)."
    }
    if ($NoWarnings -and
        @($Result.Output | Select-String -Pattern '\bwarning\s+(MSB|CS|NU|NETSDK)\d+\b').Count -ne 0) {
        $Result.Output | Select-Object -Last 50 | ForEach-Object { Write-Host $_ }
        throw "$Operation introduced a compiler, MSBuild, NuGet, or SDK warning."
    }
}

function Assert-NoSignals {
    param([AllowEmptyCollection()][object[]]$Signals)

    $present = @($Signals | Where-Object { $null -ne $_ -and -not [string]::IsNullOrWhiteSpace([string]$_) })
    if ($present.Count -ne 0) {
        throw "AutoCAD signal detected on a runner that must be clean."
    }
}

function Assert-ExecutionEnvironment {
    if (-not $IsWindows) {
        throw "The AutoCAD reference build requires Windows."
    }

    $versionResult = Invoke-DotNet -Arguments @('--version')
    Assert-DotNetSuccess $versionResult "Identify .NET SDK"
    $version = $null
    if (-not [Version]::TryParse($versionResult.Output[-1], [ref]$version) -or $version.Major -ne 8) {
        throw "Expected a compatible .NET 8 SDK."
    }
    Write-Host "Environment: Windows with .NET SDK $version."
}

function Assert-CleanRunner {
    $signals = @(
        @(
            "C:\Program Files\Autodesk\AutoCAD 2025",
            "C:\Program Files\Autodesk\AutoCAD 2025\acad.exe",
            "HKLM:\SOFTWARE\Autodesk\AutoCAD\R25.0",
            "HKLM:\SOFTWARE\WOW6432Node\Autodesk\AutoCAD\R25.0"
        ) | Where-Object { Test-Path -LiteralPath $_ }
        Get-Command acad.exe -ErrorAction SilentlyContinue | ForEach-Object { $_.Source }
    )
    Assert-NoSignals $signals
    Write-Host "Runner check: AutoCAD 2025 is absent."
}

function Assert-TransientPaths {
    $repository = Get-CanonicalPath $RepositoryRoot
    foreach ($path in @($NuGetPackages, $EmptyAutoCADDir, $NuGetConfig, $LockFile)) {
        if (-not [IO.Path]::IsPathRooted($path) -or
            (Test-SamePath $path $repository) -or
            (Test-PathWithin $path $repository)) {
            throw "Transient CI paths must be absolute and outside the repository."
        }
    }
    if (-not (Test-Path -LiteralPath $EmptyAutoCADDir -PathType Container) -or
        @(Get-ChildItem -LiteralPath $EmptyAutoCADDir -Force).Count -ne 0) {
        throw "AutoCADInstallDir must be an existing empty directory."
    }
}

function Assert-EmptyPackageCache {
    param([Parameter(Mandatory)][string]$Path)

    if (-not (Test-Path -LiteralPath $Path -PathType Container)) {
        throw "The isolated NuGet package cache does not exist."
    }
    if (@(Get-ChildItem -LiteralPath $Path -Force).Count -ne 0) {
        throw "The isolated NuGet package cache must be empty at the start."
    }
}

function Assert-NuGetConfiguration {
    [xml]$configuration = Get-Content -LiteralPath $NuGetConfig -Raw
    $clearNodes = @($configuration.SelectNodes('/configuration/packageSources/clear'))
    $sources = @($configuration.SelectNodes('/configuration/packageSources/add'))
    if ($clearNodes.Count -ne 1 -or $sources.Count -ne 1 -or
        [string]$sources[0].key -ne "nuget.org" -or
        [string]$sources[0].value -ne $script:NuGetOrg) {
        throw "The temporary NuGet.Config must clear sources and enable only nuget.org."
    }
}

function Assert-NegativeControl {
    $project = Join-Path $RepositoryRoot "src/RackCad.Plugin/RackCad.Plugin.csproj"
    $properties = @(
        "-p:UseAutoCADNuGetReferences=false",
        "-p:AutoCADInstallDir=$EmptyAutoCADDir",
        "-p:RestorePackagesPath=$NuGetPackages",
        "-p:Configuration=Release"
    )

    $restore = Invoke-DotNet -Arguments (@('restore', $project, '--configfile', $NuGetConfig, '--no-cache', '--force') + $properties + @('-v:minimal'))
    Assert-DotNetSuccess $restore "Negative-control restore"

    $build = Invoke-DotNet -Arguments (@('build', $project, '-c', 'Release', '--no-restore') + $properties + @('-v:minimal'))
    $text = $build.Output -join [Environment]::NewLine
    if ($build.ExitCode -eq 0 -or
        $text -match '(?im)\berror\s+(MSB|NU|NETSDK)\d+\b' -or
        $text -notmatch '(?im)\berror\s+CS0(234|246)\b.*\bAutodesk\b') {
        $build.Output | Select-Object -Last 50 | ForEach-Object { Write-Host $_ }
        throw "Negative control did not fail exclusively because Autodesk references were absent."
    }
    if (@(Get-ChildItem -LiteralPath $NuGetPackages -Recurse -File -Force).Count -ne 0) {
        throw "Negative control populated the isolated package cache."
    }
    Write-Host "Negative control: expected Autodesk compiler failure observed."
}

function Get-LockGraph {
    param([Parameter(Mandatory)][object]$Lock)

    $frameworks = @($Lock.dependencies.PSObject.Properties)
    if ($frameworks.Count -ne 1) {
        throw "Expected exactly one target framework in the temporary lock."
    }
    return @($frameworks[0].Value.PSObject.Properties)
}

function Assert-ExpectedPackageGraph {
    param([Parameter(Mandatory)][object]$Lock)

    $entries = Get-LockGraph $Lock
    $packages = @($entries | Where-Object { [string]$_.Value.type -ne 'Project' })
    $actualNames = @($packages.Name | Sort-Object)
    $expectedNames = @($script:ExpectedPackages.Keys | Sort-Object)
    if (@(Compare-Object $expectedNames $actualNames).Count -ne 0) {
        throw "The temporary lock contains unexpected NuGet packages."
    }

    foreach ($entry in $packages) {
        if ([string]$entry.Value.resolved -ne $script:ExpectedPackages[$entry.Name]) {
            throw "Unexpected Autodesk package version for $($entry.Name)."
        }
        $expectedType = if ($entry.Name -eq 'AutoCAD.NET') { 'Direct' } else { 'Transitive' }
        if ([string]$entry.Value.type -ne $expectedType -or
            [string]::IsNullOrWhiteSpace([string]$entry.Value.contentHash)) {
            throw "Unexpected lock metadata for $($entry.Name)."
        }
    }
    return $entries
}

function Assert-UnchangedFile {
    param(
        [Parameter(Mandatory)][string]$Path,
        [Parameter(Mandatory)][string]$ExpectedHash
    )

    $actualHash = (Get-FileHash -LiteralPath $Path -Algorithm SHA256).Hash
    if ($actualHash -ne $ExpectedHash) {
        throw "The temporary lock changed during locked restore."
    }
}

function Invoke-PositiveRestore {
    $project = Join-Path $RepositoryRoot "src/RackCad.Plugin/RackCad.Plugin.csproj"
    $properties = @(
        "-p:UseAutoCADNuGetReferences=true",
        "-p:AutoCADInstallDir=$EmptyAutoCADDir",
        "-p:RestorePackagesPath=$NuGetPackages",
        "-p:Configuration=Release"
    )

    $restore = Invoke-DotNet -Arguments (@('restore', $project, '--configfile', $NuGetConfig, '--no-cache', '--force', '--use-lock-file', '--lock-file-path', $LockFile) + $properties + @('-v:minimal'))
    Assert-DotNetSuccess $restore "Positive restore"
    if (-not (Test-Path -LiteralPath $LockFile -PathType Leaf)) {
        throw "Positive restore did not create the temporary lock."
    }

    $lock = Get-Content -LiteralPath $LockFile -Raw | ConvertFrom-Json
    [void](Assert-ExpectedPackageGraph $lock)
    $lockHash = (Get-FileHash -LiteralPath $LockFile -Algorithm SHA256).Hash

    $locked = Invoke-DotNet -Arguments (@('restore', $project, '--configfile', $NuGetConfig, '--no-cache', '--locked-mode', '--lock-file-path', $LockFile) + $properties + @('-p:RestoreRecursive=false', '-v:minimal'))
    Assert-DotNetSuccess $locked "Locked positive restore"
    Assert-UnchangedFile $LockFile $lockHash
    Write-Host "Restore: exact Autodesk graph restored twice; temporary lock unchanged."
    return $lock
}

function Get-MSBuildRestoreProperties {
    $project = Join-Path $RepositoryRoot "src/RackCad.Plugin/RackCad.Plugin.csproj"
    $arguments = @(
        'msbuild', $project,
        "-p:UseAutoCADNuGetReferences=true",
        "-p:AutoCADInstallDir=$EmptyAutoCADDir",
        "-p:RestorePackagesPath=$NuGetPackages",
        '-p:Configuration=Release',
        "-p:RestoreConfigFile=$NuGetConfig",
        '-getProperty:RestoreSources,RestoreAdditionalProjectSources,RestoreFallbackFolders,RestoreAdditionalProjectFallbackFolders,MSBuildSDKsPath'
    )
    $result = Invoke-DotNet -Arguments $arguments
    Assert-DotNetSuccess $result "Read effective restore properties"
    $text = $result.Output -join [Environment]::NewLine
    $start = $text.IndexOf('{')
    $end = $text.LastIndexOf('}')
    if ($start -lt 0 -or $end -le $start) {
        throw "MSBuild did not return structured restore properties."
    }
    return ($text.Substring($start, $end - $start + 1) | ConvertFrom-Json).Properties
}

function Assert-LibraryPacks {
    param([Parameter(Mandatory)][string]$Path)

    $expected = Get-CanonicalPath (Join-Path $env:DOTNET_ROOT 'library-packs')
    if (-not (Test-SamePath $Path $expected) -or
        -not (Test-SamePath (Split-Path -Parent (Get-CanonicalPath $Path)) (Get-CanonicalPath $env:DOTNET_ROOT)) -or
        -not (Test-Path -LiteralPath $Path -PathType Container)) {
        throw "library-packs is not the canonical direct child of DOTNET_ROOT."
    }
    $item = Get-Item -LiteralPath $Path -Force
    if (($item.Attributes -band [IO.FileAttributes]::ReparsePoint) -ne 0 -or $item.LinkType -or $item.Target) {
        throw "library-packs must not be a link or reparse point."
    }

    Add-Type -AssemblyName System.IO.Compression.FileSystem
    foreach ($package in @(Get-ChildItem -LiteralPath $Path -Force)) {
        if ($package -isnot [IO.FileInfo] -or $package.Extension -ne '.nupkg') {
            throw "library-packs may contain only first-level nupkg files."
        }
        if ($package.Name -match '(?i)(AutoCAD|Autodesk)') {
            throw "Autodesk packages are forbidden in library-packs."
        }
        $archive = [IO.Compression.ZipFile]::OpenRead($package.FullName)
        try {
            if (@($archive.Entries | ForEach-Object { [IO.Path]::GetFileName($_.FullName) } |
                    Where-Object { $_ -match '(?i)^(Ac[A-Z]|Ad[A-Z]|Autodesk).+\.dll$' }).Count -ne 0) {
                throw "Autodesk DLLs are forbidden in library-packs."
            }
        } finally {
            $archive.Dispose()
        }
    }
}

function Assert-SourcePolicy {
    param(
        [Parameter(Mandatory)][string[]]$Sources,
        [AllowEmptyString()][string]$RestoreSources,
        [AllowEmptyString()][string]$AdditionalSources,
        [AllowEmptyString()][string]$FallbackFolders,
        [AllowEmptyString()][string]$AdditionalFallbackFolders,
        [AllowEmptyCollection()][string[]]$AssetFallbackFolders,
        [Parameter(Mandatory)][string]$DotNetRoot
    )

    $expectedLibraryPacks = Get-CanonicalPath (Join-Path $DotNetRoot 'library-packs')
    $additional = @(Split-PropertyList $AdditionalSources)
    $libraryPacksDeclared = $additional.Count -eq 1 -and
        [IO.Path]::IsPathRooted($additional[0]) -and
        (Test-SamePath $additional[0] $expectedLibraryPacks)

    if ($additional.Count -gt 0 -and -not $libraryPacksDeclared) {
        throw "Unexpected RestoreAdditionalProjectSources value."
    }
    if (@(Split-PropertyList $FallbackFolders).Count -ne 0 -or
        @(Split-PropertyList $AdditionalFallbackFolders).Count -ne 0 -or
        @($AssetFallbackFolders | Where-Object { $_ }).Count -ne 0) {
        throw "Fallback package folders are forbidden."
    }

    $declaredRestoreSources = @(Split-PropertyList $RestoreSources)
    if ($declaredRestoreSources.Count -gt 1 -or
        ($declaredRestoreSources.Count -eq 1 -and $declaredRestoreSources[0].TrimEnd('/') -ne $script:NuGetOrg.TrimEnd('/'))) {
        throw "RestoreSources contains an unexpected source."
    }

    $expectedSources = @($script:NuGetOrg)
    if ($libraryPacksDeclared) { $expectedSources += $expectedLibraryPacks }
    $normalizedActual = @($Sources | ForEach-Object {
        if ([IO.Path]::IsPathRooted($_)) { Get-CanonicalPath $_ } else { $_.TrimEnd('/') }
    } | Sort-Object -Unique)
    $normalizedExpected = @($expectedSources | ForEach-Object {
        if ([IO.Path]::IsPathRooted($_)) { Get-CanonicalPath $_ } else { $_.TrimEnd('/') }
    } | Sort-Object -Unique)
    if (@(Compare-Object $normalizedExpected $normalizedActual).Count -ne 0) {
        throw "Effective restore sources are not limited to nuget.org and canonical library-packs."
    }

    if ($libraryPacksDeclared) {
        Assert-LibraryPacks $expectedLibraryPacks
    }
    return $libraryPacksDeclared
}

function Assert-RestoreAssets {
    $assetsPath = Join-Path $RepositoryRoot "src/RackCad.Plugin/obj/project.assets.json"
    $assets = Get-Content -LiteralPath $assetsPath -Raw | ConvertFrom-Json
    $properties = Get-MSBuildRestoreProperties
    if ([string]::IsNullOrWhiteSpace($env:DOTNET_ROOT) -or -not [IO.Path]::IsPathRooted($env:DOTNET_ROOT)) {
        throw "DOTNET_ROOT must identify the active SDK root."
    }

    $sources = @($assets.project.restore.sources.PSObject.Properties.Name)
    $fallbackProperty = $assets.project.restore.PSObject.Properties['fallbackFolders']
    $assetFallbacks = if ($null -eq $fallbackProperty) {
        @()
    } else {
        @($fallbackProperty.Value.PSObject.Properties.Name)
    }
    $libraryPacksDeclared = Assert-SourcePolicy `
        $sources `
        ([string]$properties.RestoreSources) `
        ([string]$properties.RestoreAdditionalProjectSources) `
        ([string]$properties.RestoreFallbackFolders) `
        ([string]$properties.RestoreAdditionalProjectFallbackFolders) `
        $assetFallbacks `
        $env:DOTNET_ROOT

    $packageFolders = @($assets.packageFolders.PSObject.Properties.Name)
    if ($packageFolders.Count -ne 1 -or -not (Test-SamePath $packageFolders[0] $NuGetPackages)) {
        throw "project.assets.json does not use only the isolated package cache."
    }
    Write-Host "Sources: nuget.org$(if ($libraryPacksDeclared) { ' plus validated SDK library-packs' } else { '' })."
    return $(if ($libraryPacksDeclared) { Get-CanonicalPath (Join-Path $env:DOTNET_ROOT 'library-packs') } else { $null })
}

function Assert-PackageMetadata {
    param(
        [Parameter(Mandatory)][string]$PackageId,
        [Parameter(Mandatory)][string]$Version,
        [Parameter(Mandatory)][string]$ExpectedContentHash,
        [AllowNull()][string]$LibraryPacks
    )

    $packageRoot = Join-Path $NuGetPackages "$($PackageId.ToLowerInvariant())\$Version"
    if (-not (Test-PathWithin $packageRoot $NuGetPackages) -or
        -not (Test-Path -LiteralPath $packageRoot -PathType Container)) {
        throw "Expected Autodesk package directory is missing from the isolated cache."
    }
    $metadataPath = Join-Path $packageRoot '.nupkg.metadata'
    $metadata = Get-Content -LiteralPath $metadataPath -Raw | ConvertFrom-Json
    if ([string]$metadata.source -ne $script:NuGetOrg -or
        [string]$metadata.contentHash -ne $ExpectedContentHash) {
        throw "Autodesk package metadata does not match nuget.org and the temporary lock."
    }
    if ($LibraryPacks -and
        @(Get-ChildItem -LiteralPath $LibraryPacks -File -Filter "$PackageId.$Version.nupkg").Count -ne 0) {
        throw "An Autodesk package was sourced or duplicated in library-packs."
    }
    return Get-CanonicalPath $packageRoot
}

function Get-PackageInventory {
    param(
        [Parameter(Mandatory)][object]$Lock,
        [AllowNull()][string]$LibraryPacks
    )

    $entries = Assert-ExpectedPackageGraph $Lock
    $entryByName = @{}
    foreach ($entry in $entries) { $entryByName[$entry.Name] = $entry.Value }

    $roots = [ordered]@{}
    $assemblyOwners = @{}
    foreach ($packageId in $script:ExpectedPackages.Keys) {
        $version = $script:ExpectedPackages[$packageId]
        $root = Assert-PackageMetadata $packageId $version ([string]$entryByName[$packageId].contentHash) $LibraryPacks
        $roots[$packageId] = $root
        foreach ($dll in @(Get-ChildItem -LiteralPath $root -Recurse -File -Filter '*.dll')) {
            if ($assemblyOwners.ContainsKey($dll.Name)) {
                throw "Duplicate Autodesk assembly name across the expected packages."
            }
            $assemblyOwners[$dll.Name] = $packageId
        }
    }
    if ($assemblyOwners.Count -ne 13) {
        throw "Expected 13 Autodesk DLLs across the three exact packages."
    }
    foreach ($principal in $script:ExpectedPrincipalAssemblies.Keys) {
        if ($assemblyOwners[$principal] -ne $script:ExpectedPrincipalAssemblies[$principal]) {
            throw "Principal Autodesk assembly inventory is inconsistent."
        }
    }
    Write-Host "Packages: three exact nuget.org packages; 13 Autodesk DLLs inventoried."
    return [pscustomobject]@{ Roots = $roots; AssemblyOwners = $assemblyOwners }
}

function Get-ItemMetadata {
    param(
        [Parameter(Mandatory)][object]$Item,
        [Parameter(Mandatory)][string]$Name
    )

    $property = $Item.PSObject.Properties[$Name]
    return $(if ($null -eq $property) { '' } else { [string]$property.Value })
}

function Convert-ReferenceItem {
    param([Parameter(Mandatory)][object]$Item)

    $fullPath = Get-ItemMetadata $Item 'FullPath'
    return [pscustomobject]@{
        Path = $(if ($fullPath) { $fullPath } else { [string]$Item.Identity })
        PackageId = Get-ItemMetadata $Item 'NuGetPackageId'
        Private = Get-ItemMetadata $Item 'Private'
        CopyLocal = Get-ItemMetadata $Item 'CopyLocal'
    }
}

function Assert-ResolvedReferenceItems {
    param(
        [Parameter(Mandatory)][object[]]$References,
        [AllowEmptyCollection()][object[]]$CopyLocalReferences,
        [Parameter(Mandatory)][Collections.IDictionary]$PackageRoots,
        [Parameter(Mandatory)][Collections.IDictionary]$AssemblyOwners
    )

    $autodesk = @($References | Where-Object {
        $name = [IO.Path]::GetFileName($_.Path)
        $script:ExpectedPackages.Contains($_.PackageId) -or $AssemblyOwners.Contains($name)
    })
    if ($autodesk.Count -ne 13) {
        throw "ResolveReferences did not return exactly 13 Autodesk compile references."
    }

    foreach ($reference in $autodesk) {
        $name = [IO.Path]::GetFileName($reference.Path)
        $expectedPackage = $AssemblyOwners[$name]
        if (-not $expectedPackage -or $reference.PackageId -ne $expectedPackage -or
            -not (Test-PathWithin $reference.Path $PackageRoots[$expectedPackage]) -or
            $reference.Path -match '(?i)Program Files[\\/]Autodesk|AutoCAD 2025' -or
            $reference.Private.ToLowerInvariant() -ne 'false' -or
            $reference.CopyLocal.ToLowerInvariant() -ne 'false') {
            throw "An Autodesk reference has unexpected origin or copy-local metadata."
        }
    }
    foreach ($principal in $script:ExpectedPrincipalAssemblies.Keys) {
        if (@($autodesk | Where-Object {
                    [IO.Path]::GetFileName($_.Path) -ceq $principal -and
                    $_.PackageId -eq $script:ExpectedPrincipalAssemblies[$principal]
                }).Count -ne 1) {
            throw "Principal Autodesk reference $principal was not resolved exactly once."
        }
    }

    $forbiddenCopyLocal = @($CopyLocalReferences | Where-Object {
        $name = [IO.Path]::GetFileName($_.Path)
        $script:ExpectedPackages.Contains($_.PackageId) -or $AssemblyOwners.Contains($name)
    })
    if ($forbiddenCopyLocal.Count -ne 0) {
        throw "ReferenceCopyLocalPaths contains Autodesk assemblies."
    }
}

function Assert-ResolvedReferences {
    param([Parameter(Mandatory)][pscustomobject]$Inventory)

    $project = Join-Path $RepositoryRoot "src/RackCad.Plugin/RackCad.Plugin.csproj"
    $arguments = @(
        'msbuild', $project, '-t:ResolveReferences',
        '-p:Configuration=Release',
        '-p:UseAutoCADNuGetReferences=true',
        "-p:AutoCADInstallDir=$EmptyAutoCADDir",
        "-p:RestorePackagesPath=$NuGetPackages",
        '-getProperty:TargetFramework,AutoCADInstallDir,UseAutoCADNuGetReferences',
        '-getItem:ReferencePath,ReferenceCopyLocalPaths'
    )
    $result = Invoke-DotNet -Arguments $arguments
    Assert-DotNetSuccess $result "ResolveReferences"
    $text = $result.Output -join [Environment]::NewLine
    $start = $text.IndexOf('{')
    $end = $text.LastIndexOf('}')
    if ($start -lt 0 -or $end -le $start) {
        throw "ResolveReferences did not return structured output."
    }
    $resolved = $text.Substring($start, $end - $start + 1) | ConvertFrom-Json
    $references = @($resolved.Items.ReferencePath | ForEach-Object { Convert-ReferenceItem $_ })
    $copyLocal = @($resolved.Items.ReferenceCopyLocalPaths | ForEach-Object { Convert-ReferenceItem $_ })
    Assert-ResolvedReferenceItems $references $copyLocal $Inventory.Roots $Inventory.AssemblyOwners
    Write-Host "ResolveReferences: 13 cache-backed Autodesk references; CopyLocal=false; 0 copy-local entries."
}

function Invoke-ReleaseBuilds {
    $properties = @(
        "-p:UseAutoCADNuGetReferences=true",
        "-p:AutoCADInstallDir=$EmptyAutoCADDir",
        "-p:RestorePackagesPath=$NuGetPackages"
    )
    $plugin = Join-Path $RepositoryRoot "src/RackCad.Plugin/RackCad.Plugin.csproj"
    $solution = Join-Path $RepositoryRoot "RackCad.sln"

    $pluginBuild = Invoke-DotNet -Arguments (@('build', $plugin, '-c', 'Release', '--no-restore') + $properties + @('-v:minimal'))
    Assert-DotNetSuccess $pluginBuild "Plugin Release build" -NoWarnings

    $solutionRestore = Invoke-DotNet -Arguments (@('restore', $solution, '--configfile', $NuGetConfig, '--no-cache', '--force') + $properties + @('-p:Configuration=Release', '-v:minimal'))
    Assert-DotNetSuccess $solutionRestore "Solution Release restore" -NoWarnings
    $solutionBuild = Invoke-DotNet -Arguments (@('build', $solution, '-c', 'Release', '--no-restore') + $properties + @('-v:minimal'))
    Assert-DotNetSuccess $solutionBuild "Solution Release build" -NoWarnings
    Write-Host "Builds: Plugin Release and solution Release succeeded without warnings."
}

function Assert-NoAutodeskOutputs {
    param(
        [Parameter(Mandatory)][string]$Root,
        [Parameter(Mandatory)][Collections.IDictionary]$AssemblyOwners
    )

    foreach ($file in @(Get-ChildItem -LiteralPath $Root -Recurse -File -Force)) {
        if ($file.FullName -match '[\\/]\.git[\\/]' ) { continue }
        if ($file.Extension -ieq '.zip') {
            throw "Generated ZIP files are forbidden in the Plugin CI verification."
        }
        if ($AssemblyOwners.Contains($file.Name)) {
            throw "An Autodesk DLL escaped the isolated package cache: $(Get-SafePath $file.FullName)"
        }
    }
}

function Assert-BundleContract {
    param(
        [Parameter(Mandatory)][string]$Root,
        [Parameter(Mandatory)][Collections.IDictionary]$AssemblyOwners
    )

    $bundle = Join-Path $Root "src/RackCad.Plugin/bin/Release/net8.0-windows/RackCad.bundle"
    $contents = Join-Path $bundle 'Contents'
    if (-not (Test-Path -LiteralPath (Join-Path $bundle 'PackageContents.xml') -PathType Leaf) -or
        -not (Test-Path -LiteralPath (Join-Path $contents 'catalogs') -PathType Container)) {
        throw "The generated bundle is missing its manifest or catalogs."
    }
    $actual = @(Get-ChildItem -LiteralPath $contents -Recurse -File -Filter '*.dll' | Select-Object -ExpandProperty Name | Sort-Object)
    $expected = @('RackCad.Application.dll', 'RackCad.Domain.dll', 'RackCad.Plugin.dll', 'RackCad.UI.dll') | Sort-Object
    if (@(Compare-Object $expected $actual).Count -ne 0) {
        throw "The bundle DLL contract changed."
    }
    if (@($actual | Where-Object { $AssemblyOwners.Contains($_) }).Count -ne 0) {
        throw "The bundle contains an Autodesk DLL."
    }
    Write-Host "Outputs: no Autodesk DLLs or ZIPs; bundle contains exactly four RackCad DLLs."
}

function Invoke-AutoCADReferenceVerificationCore {
    $previousLocation = Get-Location
    try {
        Set-Location -LiteralPath $RepositoryRoot
        Assert-TransientPaths
        Assert-EmptyPackageCache $NuGetPackages
        Assert-NuGetConfiguration
        Assert-NegativeControl
        $lock = Invoke-PositiveRestore
        $libraryPacks = Assert-RestoreAssets
        $inventory = Get-PackageInventory $lock $libraryPacks
        Assert-ResolvedReferences $inventory
        Invoke-ReleaseBuilds
        Assert-NoAutodeskOutputs $RepositoryRoot $inventory.AssemblyOwners
        Assert-BundleContract $RepositoryRoot $inventory.AssemblyOwners
    } finally {
        Set-Location -LiteralPath $previousLocation
    }
}

# Dot-sourcing is used only by local, non-versioned test harnesses for the individual guards.
if ($MyInvocation.InvocationName -eq '.') {
    return
}

$RepositoryRoot = Get-CanonicalPath $RepositoryRoot
$NuGetPackages = Get-CanonicalPath $NuGetPackages
$EmptyAutoCADDir = Get-CanonicalPath $EmptyAutoCADDir
$NuGetConfig = Get-CanonicalPath $NuGetConfig
$LockFile = Get-CanonicalPath $LockFile

Assert-ExecutionEnvironment
Assert-CleanRunner
Invoke-AutoCADReferenceVerificationCore
Write-Host "AutoCAD compile-reference verification passed. No artifacts were published."
