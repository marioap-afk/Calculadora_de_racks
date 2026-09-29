#Requires -Version 7.0
[CmdletBinding()]
param(
    # The exact commit the package is built from (the harness commit). HEAD must be this SHA.
    [Parameter(Mandatory = $true)][string]$HarnessSha,
    # The AUTH-15 implementation SHA the host evidence binds to. src/ and tests/ must be byte-identical to it.
    [string]$ImplementationSha = '2d10de705fffee3dc03473c2d4c76bff489fc13e',
    [string]$OutputRoot = '',
    [string]$AutoCadInstallDir = 'C:\Program Files\Autodesk\AutoCAD 2025',
    [string]$Dotnet = ''
)

# I-52-AUTH15 host-validation package (build side). Never starts AutoCAD, never runs an AUTH-15 call.
# Builds RackCad.Plugin from the exact implementation content (src/ and tests/ verified identical to the
# implementation SHA), builds the harness, and writes a versioned package OUTSIDE the repository.
$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$repo = (Resolve-Path (Join-Path $PSScriptRoot '..\..\..')).Path
$harnessDir = Join-Path $repo 'eng\research\I52Auth15Host'

if ($HarnessSha -notmatch '^[0-9a-f]{40}$') { throw 'HarnessSha must be the full 40-hex commit SHA.' }
if ($ImplementationSha -notmatch '^[0-9a-f]{40}$') { throw 'ImplementationSha must be the full 40-hex commit SHA.' }
$short = $HarnessSha.Substring(0, 8)
if (-not $OutputRoot) { $OutputRoot = "D:\I52-AUTH15-HV\$short" }

if (-not $Dotnet) {
    # The user-level SDK resolves global.json; the PATH dotnet may not (project memory).
    $userDotnet = Join-Path $env:LOCALAPPDATA 'Microsoft\dotnet\dotnet.exe'
    $Dotnet = if (Test-Path -LiteralPath $userDotnet) { $userDotnet } else { 'dotnet' }
}

# NOT named Git: a function called Git would shadow git.exe (PowerShell command lookup is case-insensitive) and recurse.
function Invoke-GitChecked { & git.exe -C $repo @args; if ($LASTEXITCODE -ne 0) { throw "git $args failed" } }

# 1. Exact committed source.
$head = (Invoke-GitChecked rev-parse HEAD).Trim()
if ($head -ne $HarnessSha) { throw "HEAD $head is not the requested harness SHA $HarnessSha" }
if (& git.exe -C $repo status --porcelain) { throw 'Working tree is not clean: the package is built only from committed source.' }

# IGNORED source/config files are invisible to `git status` yet would still be compiled or read by the build (globbing ignores
# .gitignore). Normal ignored build output (bin/obj) is fine; anything else that looks like source or configuration is not.
$ignoredPaths = @('src', 'tests', 'eng/research/I52Auth15Host', 'Directory.Build.props', 'Directory.Build.targets', 'global.json', 'NuGet.Config', 'RackCad.sln')
$ignoredSuspect = @(& git.exe -C $repo ls-files --others --ignored --exclude-standard -- @ignoredPaths |
    Where-Object { $_ -notmatch '(^|/)(bin|obj)/' -and $_ -match '\.(cs|csproj|props|targets|ps1|scr|json|config|sln)$' })
if ($ignoredSuspect.Count -gt 0) { throw "STOP: ignored source/config files exist outside bin/obj and would be built or read: $($ignoredSuspect -join ', ')" }
Invoke-GitChecked cat-file -e "$ImplementationSha^{commit}"
Invoke-GitChecked merge-base --is-ancestor $ImplementationSha $HarnessSha

# 2. BINDING RULE: src/ and tests/ byte-identical to the implementation SHA. If not: STOP.
& git.exe -C $repo diff --quiet $ImplementationSha $HarnessSha -- src tests
if ($LASTEXITCODE -ne 0) { throw "STOP: src/ or tests/ differ from implementation SHA $ImplementationSha." }
$implSrc = (Invoke-GitChecked rev-parse "${ImplementationSha}:src").Trim()
$implTests = (Invoke-GitChecked rev-parse "${ImplementationSha}:tests").Trim()
$harnessSrc = (Invoke-GitChecked rev-parse "${HarnessSha}:src").Trim()
$harnessTests = (Invoke-GitChecked rev-parse "${HarnessSha}:tests").Trim()
if ($implSrc -ne $harnessSrc -or $implTests -ne $harnessTests) { throw 'STOP: tree hashes of src/ or tests/ differ from the implementation SHA.' }
$treesEqual = $true

# The harness commit may only ADD the harness and touch docs.
$outside = @(Invoke-GitChecked diff --name-only $ImplementationSha $HarnessSha | Where-Object { $_ -notmatch '^(eng/research/I52Auth15Host/|docs/)' })
if ($outside.Count -gt 0) { throw "STOP: the harness commit changes files outside eng/research/I52Auth15Host/ and docs/: $($outside -join ', ')" }

if (Test-Path -LiteralPath $OutputRoot) { throw "Output $OutputRoot already exists: prior packages are never overwritten." }
foreach ($required in (Join-Path $AutoCadInstallDir 'AcDbMgd.dll'), (Join-Path $AutoCadInstallDir 'acad.exe')) {
    if (-not (Test-Path -LiteralPath $required)) { throw "Missing: $required" }
}
# Building never starts or touches AutoCAD. A running instance is only a warning here (it matters at RUN time, where
# run-hostval.ps1 refuses it); if it locks a Plugin DLL, the build itself fails.
$acadRunningDuringBuild = [bool](Get-Process acad -ErrorAction SilentlyContinue)
if ($acadRunningDuringBuild) { Write-Warning 'acad.exe is running; it is left alone. The build fails by itself if it locks an output DLL. (The LAUNCH still refuses any running acad.exe.)' }

$run = Join-Path $OutputRoot 'run'
$out = Join-Path $OutputRoot 'out'
$logs = Join-Path $OutputRoot 'logs'
$launcher = Join-Path $OutputRoot 'launcher'
New-Item -ItemType Directory -Force $run, $out, $logs, $launcher | Out-Null

# 3. Build RackCad.Plugin (and its Application/Domain/UI) from the exact content.
& $Dotnet build (Join-Path $repo 'src\RackCad.Plugin\RackCad.Plugin.csproj') -c Debug --no-incremental -nologo -v:minimal "-p:AutoCADInstallDir=$AutoCadInstallDir" |
    Out-File (Join-Path $logs 'plugin-build.log') -Encoding utf8
if ($LASTEXITCODE -ne 0) { throw 'RackCad.Plugin build failed (see logs\plugin-build.log)' }
$pluginBin = Join-Path $repo 'src\RackCad.Plugin\bin\Debug\net8.0-windows'

# 4. Build the harness against those exact assemblies.
$binArg = ($pluginBin.Replace('\', '/')).TrimEnd('/') + '/'
& $Dotnet build (Join-Path $harnessDir 'I52Auth15.HostHarness.csproj') -c Debug --no-incremental -nologo -v:minimal "-p:AutoCADInstallDir=$AutoCadInstallDir" "-p:RackCadBin=$binArg" |
    Out-File (Join-Path $logs 'harness-build.log') -Encoding utf8
if ($LASTEXITCODE -ne 0) { throw 'harness build failed (see logs\harness-build.log)' }
$harnessBin = Join-Path $harnessDir 'bin\Debug\net8.0-windows'

# 5. Only the runtime assemblies the run needs, plus the eight structural-section catalog files the Cantilever
#    fixture reads (data, not code).
foreach ($name in 'RackCad.Domain', 'RackCad.Application', 'RackCad.Plugin', 'RackCad.UI') {
    Copy-Item (Join-Path $pluginBin "$name.dll"), (Join-Path $pluginBin "$name.pdb") $run
}
foreach ($name in 'I52Auth15.HostHarness.dll', 'I52Auth15.HostHarness.pdb', 'I52Auth15.HostHarness.deps.json') {
    Copy-Item (Join-Path $harnessBin $name) $run
}
$catalogs = Join-Path $run 'catalogs'
New-Item -ItemType Directory -Force $catalogs | Out-Null
foreach ($file in 'structural-section-sources.csv', 'structural-section-status.csv', 'structural-sections-c.csv', 'structural-sections-hss-rect.csv',
    'structural-sections-l.csv', 'structural-sections-s.csv', 'structural-sections-w.csv', 'structural-sections-manifest.json') {
    Copy-Item (Join-Path $repo "assets\catalogs\$file") $catalogs
}
Copy-Item (Join-Path $harnessDir 'run-hostval.ps1'), (Join-Path $harnessDir 'run.scr') $launcher

# 6. Metadata, checksums, transfer zip.
function Identity([string]$path) {
    $item = Get-Item -LiteralPath $path
    [ordered]@{ file = $item.Name; bytes = $item.Length; sha256 = (Get-FileHash -Algorithm SHA256 -LiteralPath $path).Hash; version = $item.VersionInfo.FileVersion }
}
$dlls = [ordered]@{}
foreach ($file in Get-ChildItem -LiteralPath $run -File | Where-Object { $_.Extension -eq '.dll' } | Sort-Object Name) { $dlls[$file.Name] = Identity $file.FullName }
$acdb = Identity (Join-Path $AutoCadInstallDir 'AcDbMgd.dll')
$acad = Identity (Join-Path $AutoCadInstallDir 'acad.exe')
$metadata = [ordered]@{
    schemaVersion = 1
    initiative = 'I-52'
    unit = 'I-52-AUTH15'
    milestone = 'AUTH-15 host-validation package (temporary harness; reverted before Candidate)'
    implementationSha = $ImplementationSha
    harnessSha = $HarnessSha
    implementationSrcTree = $implSrc
    implementationTestsTree = $implTests
    harnessSrcTree = $harnessSrc
    harnessTestsTree = $harnessTests
    treesEqual = $treesEqual
    dlls = $dlls
    pluginInformationalVersion = (Get-Item (Join-Path $run 'RackCad.Plugin.dll')).VersionInfo.ProductVersion
    buildTimestampUtc = (Get-Date).ToUniversalTime().ToString('o')
    toolchain = [ordered]@{
        dotnetSdk = (& $Dotnet --version).Trim()
        dotnetHost = $Dotnet
        autoCadInstallDir = $AutoCadInstallDir
        acadExe = $acad
        autoCadCompileReferenceAcDbMgd = $acdb
        configuration = 'Debug'
    }
    runDirectory = 'run (harness + RackCad assemblies + catalogs; NETLOAD only I52Auth15.HostHarness.dll)'
    autoCadStarted = $false
    acadRunningDuringBuild = $acadRunningDuringBuild
    hostValidation = 'NOT RUN'
    command = 'I52AUTH15_HOSTVAL'
    nextGate = 'COORDINATOR AUTHORIZATION TO RUN AUTH-15 HOST VALIDATION: launcher\run-hostval.ps1 -Package <this folder> -ScratchDrawing <fresh blank .dwg> -OwnerConfirmsNoTouch'
}
$metadata | ConvertTo-Json -Depth 8 | Out-File (Join-Path $OutputRoot 'TRANSFER-METADATA.json') -Encoding utf8

$sums = Get-ChildItem -LiteralPath $OutputRoot -Recurse -File | Where-Object { $_.Name -ne 'SHA256SUMS' } | Sort-Object FullName | ForEach-Object {
    '{0}  {1}' -f (Get-FileHash -Algorithm SHA256 -LiteralPath $_.FullName).Hash, $_.FullName.Substring($OutputRoot.Length + 1).Replace('\', '/')
}
[IO.File]::WriteAllLines((Join-Path $OutputRoot 'SHA256SUMS'), $sums)

$zip = "$OutputRoot.zip"
if (Test-Path -LiteralPath $zip) { throw "$zip already exists" }
Compress-Archive -Path (Join-Path $OutputRoot '*') -DestinationPath $zip

"PACKAGE=$OutputRoot"
"ZIP=$zip"
"ZIP_SHA256=$((Get-FileHash -Algorithm SHA256 -LiteralPath $zip).Hash)"
"SHA256SUMS_SHA256=$((Get-FileHash -Algorithm SHA256 -LiteralPath (Join-Path $OutputRoot 'SHA256SUMS')).Hash)"
"IMPLEMENTATION_SHA=$ImplementationSha"
"HARNESS_SHA=$HarnessSha"
"SRC_TREE_IMPL=$implSrc"
"SRC_TREE_HARNESS=$harnessSrc"
"TESTS_TREE_IMPL=$implTests"
"TESTS_TREE_HARNESS=$harnessTests"
"TREES_EQUAL=$treesEqual"
