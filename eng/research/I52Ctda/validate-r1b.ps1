[CmdletBinding()]
param([string]$Repository = (Resolve-Path "$PSScriptRoot\..\..\..").Path)

$ErrorActionPreference = 'Stop'
$harness = Join-Path $PSScriptRoot 'harness\I52Ctda.Harness.csproj'
$tests = Join-Path $PSScriptRoot 'tests\I52Ctda.Tests.csproj'

dotnet build $harness -c Release -v:minimal
if ($LASTEXITCODE -ne 0) { throw "Managed harness build failed: $LASTEXITCODE" }

# R3: the V34 dispatch table is superseded; the frozen V35 authority is verified and the generated plans must not drift.
dotnet run --project $harness -c Release -- freeze-v35 $Repository
if ($LASTEXITCODE -ne 0) { throw "V35 freeze does not hold: $LASTEXITCODE" }

dotnet run --project $harness -c Release -- check-generated-v35 $Repository
if ($LASTEXITCODE -ne 0) { throw "Generated V35 authority drifted: $LASTEXITCODE" }

dotnet run --project $harness -c Release -- conformance-v35 $Repository
if ($LASTEXITCODE -ne 0) { throw "R3 static conformance failed: $LASTEXITCODE" }

dotnet run --project $harness -c Release -- validate $Repository
if ($LASTEXITCODE -ne 0) { throw "Registry validation failed: $LASTEXITCODE" }

dotnet run --project $harness -c Release -- static-native $Repository
if ($LASTEXITCODE -ne 0) { throw "Static native validation failed: $LASTEXITCODE" }

dotnet run --project $harness -c Release -- headers $Repository
if ($LASTEXITCODE -ne 0) { throw "ObjectARX header authority validation failed: $LASTEXITCODE" }

dotnet run --project $tests -c Release -- $Repository
if ($LASTEXITCODE -ne 0) { throw "Research tests failed: $LASTEXITCODE" }
