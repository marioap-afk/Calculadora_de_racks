[CmdletBinding()]
param(
    [string]$MSBuild = 'D:\Visual Studio\Community\MSBuild\Current\Bin\amd64\MSBuild.exe',
    [string]$VcToolsVersion = '14.44.35207'
)

# Build-machine native regression tests (no AutoCAD, no ObjectARX): D-2 token CommandIdentity lifetime over the real
# LOG-SEQ-01 sources, for rows with completion tokens. Every governed row's token records must carry I52CTDA_PROBE.
$ErrorActionPreference = 'Stop'
$project = Join-Path $PSScriptRoot 'I52CtdaNativeTests.vcxproj'
& $MSBuild $project /t:Rebuild /p:Configuration=Debug /p:Platform=x64 "/p:VCToolsVersion=$VcToolsVersion" /v:minimal /nologo
if ($LASTEXITCODE -ne 0) { throw "native test build failed: $LASTEXITCODE" }
$exe = Join-Path $PSScriptRoot 'bin\x64\Debug\I52CtdaNativeTests.exe'
foreach ($probe in '02NDBMOD-S', '09N-B', 'CTRENDED16SND-ALL') {
    $log = Join-Path ([IO.Path]::GetTempPath()) "i52ctda-d2-$probe-$PID.jsonl"
    try {
        & $exe write $probe $log
        if ($LASTEXITCODE -ne 0) { throw "native D-2 token writer failed for $probe" }
        & $exe check $probe $log
        if ($LASTEXITCODE -ne 0) { throw "native D-2 token test failed for $probe" }
    }
    finally { Remove-Item -LiteralPath $log -ErrorAction SilentlyContinue }
}
