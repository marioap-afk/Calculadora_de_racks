[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)][string]$SourceSha,
    [string]$OutputRoot = '',
    [string]$ObjectArxSdkRoot = 'D:\CDROM1',
    [string]$MSBuild = 'D:\Visual Studio\Community\MSBuild\Current\Bin\amd64\MSBuild.exe',
    [string]$VcToolsVersion = '14.44.35207'
)

# I-52 R3 canonical build (build machine only; never starts AutoCAD, never executes a ProbeId). Builds R-NATIVE-ARX,
# R-PAYLOAD-ARX, R-MANAGED-OBSERVER and the managed harness/control plane from the exact committed source SHA, records
# the new BUILD_MACHINE_TOOLCHAIN_TUPLE and writes a versioned host-transfer package outside the repository.
$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
$repo = (Resolve-Path (Join-Path $PSScriptRoot '..\..\..')).Path
$research = Join-Path $repo 'eng\research\I52Ctda'
$short = $SourceSha.Substring(0, 8)
if (-not $OutputRoot) { $OutputRoot = "D:\I52-CTDA-R3\$short" }

# Exact committed source: HEAD is the SHA and the tree is clean.
$head = (git -C $repo rev-parse HEAD).Trim()
if ($head -ne $SourceSha) { throw "HEAD $head is not the requested source SHA $SourceSha" }
if (git -C $repo status --porcelain) { throw 'Working tree is not clean: canonical artifacts are built only from committed source.' }
if (Test-Path -LiteralPath $OutputRoot) { throw "Output $OutputRoot already exists: prior packages are never overwritten." }
foreach ($tool in $MSBuild, (Join-Path $ObjectArxSdkRoot 'inc\rxregsvc.h')) { if (-not (Test-Path -LiteralPath $tool)) { throw "Missing: $tool" } }
$vc = "D:\Visual Studio\Community\VC\Tools\MSVC\$VcToolsVersion\bin\Hostx64\x64"
$dumpbin = Join-Path $vc 'dumpbin.exe'

New-Item -ItemType Directory -Force (Join-Path $OutputRoot 'run'), (Join-Path $OutputRoot 'logs'), (Join-Path $OutputRoot 'evidence'), (Join-Path $OutputRoot 'harness') | Out-Null

# V35 freeze and generated authority must hold before anything is built.
dotnet build (Join-Path $research 'harness\I52Ctda.Harness.csproj') -c Release -v:minimal -nologo | Out-File (Join-Path $OutputRoot 'logs\harness-prebuild.log') -Encoding utf8
if ($LASTEXITCODE -ne 0) { throw 'harness prebuild failed' }
dotnet run --project (Join-Path $research 'harness') -c Release --no-build -- freeze-v35 $repo | Out-File (Join-Path $OutputRoot 'evidence\freeze-v35.json') -Encoding utf8
if ($LASTEXITCODE -ne 0) { throw 'V35 freeze does not hold: STOP' }
dotnet run --project (Join-Path $research 'harness') -c Release --no-build -- check-generated-v35 $repo | Out-File (Join-Path $OutputRoot 'evidence\check-generated-v35.json') -Encoding utf8
if ($LASTEXITCODE -ne 0) { throw 'generated V35 authority drifted: STOP' }

# Native modules: Rebuild, Release|x64, v143, Hostx64 tools, exact ObjectARX 2025 SDK.
foreach ($project in @(@{ Name = 'I52CtdaNative'; Path = 'native\I52CtdaNative.vcxproj'; Dir = 'native' }, @{ Name = 'I52CtdaPayload'; Path = 'payload\I52CtdaPayload.vcxproj'; Dir = 'payload' })) {
    $log = Join-Path $OutputRoot "logs\$($project.Name).msbuild.log"
    $binlog = Join-Path $OutputRoot "logs\$($project.Name).binlog"
    & $MSBuild (Join-Path $research $project.Path) /t:Rebuild /m /p:Configuration=Release /p:Platform=x64 /p:PlatformToolset=v143 /p:PreferredToolArchitecture=x64 "/p:ObjectArxSdkRoot=$ObjectArxSdkRoot" "/p:VCToolsVersion=$VcToolsVersion" /v:normal /nologo "/bl:$binlog" | Out-File $log -Encoding utf8
    if ($LASTEXITCODE -ne 0) { throw "$($project.Name) build failed" }
    $bin = Join-Path $research "$($project.Dir)\bin\x64\Release"
    Copy-Item (Join-Path $bin "$($project.Name).arx"), (Join-Path $bin "$($project.Name).pdb") (Join-Path $OutputRoot 'run')
    Copy-Item (Join-Path $research "$($project.Dir)\obj\x64\Release\$($project.Name).tlog\link.read.1.tlog") (Join-Path $OutputRoot "evidence\$($project.Name).link.read.tlog")
    Copy-Item (Join-Path $research "$($project.Dir)\obj\x64\Release\$($project.Name).tlog\link.command.1.tlog") (Join-Path $OutputRoot "evidence\$($project.Name).link.command.tlog")
    $arx = Join-Path $OutputRoot "run\$($project.Name).arx"
    foreach ($kind in 'headers', 'imports', 'exports', 'dependents') {
        & $dumpbin /nologo "/$kind" $arx | Out-File (Join-Path $OutputRoot "evidence\$($project.Name).$kind.txt") -Encoding utf8
    }
}

# Managed observer (net8.0-windows, x64) and the managed harness/control plane.
dotnet build (Join-Path $research 'managed-observer\I52Ctda.ManagedObserver.csproj') -c Release --no-incremental -v:normal -nologo "-p:ObjectArxSdkRoot=$ObjectArxSdkRoot" | Out-File (Join-Path $OutputRoot 'logs\I52Ctda.ManagedObserver.build.log') -Encoding utf8
if ($LASTEXITCODE -ne 0) { throw 'managed observer build failed' }
$observer = Join-Path $research 'managed-observer\bin\Release\net8.0-windows'
Copy-Item (Join-Path $observer 'I52Ctda.ManagedObserver.dll'), (Join-Path $observer 'I52Ctda.ManagedObserver.pdb'), (Join-Path $observer 'I52Ctda.ManagedObserver.deps.json') (Join-Path $OutputRoot 'run')
dotnet publish (Join-Path $research 'harness\I52Ctda.Harness.csproj') -c Release -o (Join-Path $OutputRoot 'harness') -v:minimal -nologo | Out-File (Join-Path $OutputRoot 'logs\I52Ctda.Harness.publish.log') -Encoding utf8
if ($LASTEXITCODE -ne 0) { throw 'harness publish failed' }
Copy-Item (Join-Path $research 'runtime-plan-v35.json') (Join-Path $OutputRoot 'evidence\runtime-plan-v35.json')

# Toolchain identity (compiler, linker, MSBuild, .NET SDK, ObjectARX SDK).
function Identity([string]$path) {
    $item = Get-Item -LiteralPath $path
    [ordered]@{ path = $item.FullName; bytes = $item.Length; sha256 = (Get-FileHash -Algorithm SHA256 -LiteralPath $path).Hash; version = $item.VersionInfo.FileVersion }
}
$toolchain = [ordered]@{
    compiler = Identity (Join-Path $vc 'cl.exe')
    linker = Identity (Join-Path $vc 'link.exe')
    msbuild = Identity $MSBuild
    platformToolset = 'v143'
    vcToolsVersion = $VcToolsVersion
    preferredToolArchitecture = 'x64'
    configuration = 'Release|x64'
    dotnetSdk = (dotnet --version).Trim()
    objectArxSdk = [ordered]@{ root = $ObjectArxSdkRoot; version = '25.0.58.0'; headers = @('aced.h', 'acdocman.h', 'dbmain.h', 'dbtrans.h', 'rxdlinkr.h', 'acedads.h', 'rxregsvc.h', 'AcApDocLockmode.h') | ForEach-Object { Identity (Join-Path $ObjectArxSdkRoot "inc\$_") }; managedReferences = @('AcCoreMgd.dll', 'AcDbMgd.dll', 'AcMgd.dll') | ForEach-Object { Identity (Join-Path $ObjectArxSdkRoot "inc\$_") } }
}
$toolchain | ConvertTo-Json -Depth 8 | Out-File (Join-Path $OutputRoot 'evidence\toolchain.json') -Encoding utf8

# New build tuple, then transfer metadata and checksums.
# The tuple hashes logs\, so its own output is written only after the command exits and outside logs\.
$tupleOutput = & (Join-Path $OutputRoot 'harness\I52Ctda.Harness.exe') tuple-v35 $repo $OutputRoot $SourceSha 2>&1
$tupleExit = $LASTEXITCODE
$tupleOutput | Out-File (Join-Path $OutputRoot 'evidence\tuple-command.txt') -Encoding utf8
if ($tupleExit -ne 0) { throw 'tuple collection failed' }
$tupleHash = (($tupleOutput | Select-String 'NEW_BUILD_MACHINE_TOOLCHAIN_TUPLE_HASH=') -replace '.*=', '').Trim()
$metadata = [ordered]@{
    schemaVersion = 1
    initiative = 'I-52'
    milestone = 'R3 build side (V35 frozen contract); host smoke pending'
    v35FreezePackageHash = '43DCE809AA5B124E73B67E2B8B76EB78DC921FCE0BE961B2906BC21BE9D3B6DF'
    sourceSha = $SourceSha
    buildTupleHash = $tupleHash
    runDirectory = 'run (R-NATIVE-ARX, R-PAYLOAD-ARX and R-MANAGED-OBSERVER must stay together: NL-ARX-PATH and NETLOAD resolve from this directory)'
    nextGate = 'R3 HOST SMOKE / ZERO PROBES: harness\I52Ctda.Harness.exe smoke-v35 <repo> <out> run\I52CtdaNative.arx <fresh scratch.dwg> [profile with TRUSTEDPATHS containing run\]'
    governingProbesExecuted = 0
    autoCadStarted = $false
}
$metadata | ConvertTo-Json -Depth 4 | Out-File (Join-Path $OutputRoot 'TRANSFER-METADATA.json') -Encoding utf8
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
"NEW_BUILD_MACHINE_TOOLCHAIN_TUPLE_HASH=$tupleHash"
