[CmdletBinding()]
param([string]$Repository = (Resolve-Path "$PSScriptRoot\..\..\..").Path)

$ErrorActionPreference = 'Stop'
$harness = Join-Path $PSScriptRoot 'harness\I52Ctda.Harness.csproj'
$tests = Join-Path $PSScriptRoot 'tests\I52Ctda.Tests.csproj'

dotnet build $harness -c Release -v:minimal
if ($LASTEXITCODE -ne 0) { throw "Managed harness build failed: $LASTEXITCODE" }

dotnet run --project $harness -c Release -- generate $Repository eng/research/I52Ctda/traceability-v34.json eng/research/I52Ctda/native/ProbeDispatchTable.inc
if ($LASTEXITCODE -ne 0) { throw "Traceability generation failed: $LASTEXITCODE" }

dotnet run --project $harness -c Release -- validate $Repository
if ($LASTEXITCODE -ne 0) { throw "Registry validation failed: $LASTEXITCODE" }

dotnet run --project $harness -c Release -- static-native $Repository
if ($LASTEXITCODE -ne 0) { throw "Static native validation failed: $LASTEXITCODE" }

dotnet run --project $harness -c Release -- headers $Repository
if ($LASTEXITCODE -ne 0) { throw "ObjectARX header authority validation failed: $LASTEXITCODE" }

dotnet run --project $tests -c Release -- $Repository
if ($LASTEXITCODE -ne 0) { throw "Research tests failed: $LASTEXITCODE" }
