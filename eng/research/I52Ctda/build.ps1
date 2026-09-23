[CmdletBinding()]
param(
    [string]$ObjectArxSdkRoot = 'D:\Downloads\CDROM1'
)

$ErrorActionPreference = 'Stop'
$root = $PSScriptRoot
$msbuild = 'C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\amd64\MSBuild.exe'
$native = Join-Path $root 'native\I52CtdaNative.vcxproj'
$managed = Join-Path $root 'harness\I52Ctda.Harness.csproj'

if (-not (Test-Path -LiteralPath $msbuild)) { throw "MSBuild missing: $msbuild" }
if (-not (Test-Path -LiteralPath (Join-Path $ObjectArxSdkRoot 'inc\rxregsvc.h'))) { throw "ObjectARX SDK invalid: $ObjectArxSdkRoot" }

& $msbuild $native /m /restore /p:Configuration=Release /p:Platform=x64 "/p:ObjectArxSdkRoot=$ObjectArxSdkRoot" /v:minimal
if ($LASTEXITCODE -ne 0) { throw "Native helper build failed: $LASTEXITCODE" }

dotnet build $managed -c Release -v:minimal
if ($LASTEXITCODE -ne 0) { throw "Managed harness build failed: $LASTEXITCODE" }
