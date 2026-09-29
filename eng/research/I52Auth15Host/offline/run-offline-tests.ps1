#Requires -Version 7.0
[CmdletBinding()]
param(
    [string]$Work = '',
    [string]$Dotnet = ''
)

# OFFLINE launcher characterization. No AutoCAD is started: a stand-in (offlineacad.exe: a mini script/LISP interpreter that runs the
# REAL run.scr and plays the harness with the real FilediaRecord/EvidenceDoc) is launched by the REAL run-hostval.ps1, against
# synthetic packages and a private test registry key. NOTHING here is host evidence and none of it ever goes into a package.
$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$here = $PSScriptRoot
$harnessDir = Split-Path $here
if (-not $Dotnet) {
    $user = Join-Path $env:LOCALAPPDATA 'Microsoft\dotnet\dotnet.exe'
    $Dotnet = if (Test-Path -LiteralPath $user) { $user } else { 'dotnet' }
}
if (-not $Work) { $Work = Join-Path ([IO.Path]::GetTempPath()) ('i52auth15-offline-' + [guid]::NewGuid().ToString('N').Substring(0, 8)) }
New-Item -ItemType Directory -Force $Work | Out-Null

& $Dotnet build (Join-Path $here 'OfflineAcad.csproj') -c Debug -nologo -v:minimal | Out-Null
if ($LASTEXITCODE -ne 0) { throw 'offlineacad build failed' }
$acad = Join-Path $here 'bin\Debug\net8.0\offlineacad.exe'

$script:Results = New-Object System.Collections.Generic.List[object]
function Assert-That([string]$Test, [string]$Name, [bool]$Condition, [string]$Detail = '') {
    $script:Results.Add([pscustomobject]@{ Test = $Test; Name = $Name; Pass = $Condition; Detail = $Detail })
    $mark = if ($Condition) { 'PASS' } else { 'FAIL' }
    Write-Host ("  [{0}] {1}: {2}{3}" -f $mark, $Test, $Name, $(if (-not $Condition -and $Detail) { "  <- $Detail" } else { '' }))
}

# ---------------------------------------------------------------- fixtures
$regBase = 'Software\I52Auth15OfflineTest-' + [guid]::NewGuid().ToString('N').Substring(0, 8)
$dwg = Join-Path $Work 'blank.dwg'
[IO.File]::WriteAllBytes($dwg, ([Text.Encoding]::ASCII.GetBytes('AC1032') + (New-Object byte[] 200)))
$notDwg = Join-Path $Work 'not-a-drawing.dwg'
Set-Content -LiteralPath $notDwg -Value 'this is text, not a DWG'
$txt = Join-Path $Work 'blank.txt'
[IO.File]::WriteAllBytes($txt, ([Text.Encoding]::ASCII.GetBytes('AC1032') + (New-Object byte[] 50)))

function New-TestPackage([string]$Name) {
    $root = Join-Path $Work $Name
    $run = Join-Path $root 'run'
    New-Item -ItemType Directory -Force $run, (Join-Path $root 'launcher'), (Join-Path $root 'logs') | Out-Null
    $dlls = [ordered]@{}
    foreach ($n in 'I52Auth15.HostHarness.dll', 'RackCad.Plugin.dll', 'RackCad.Application.dll', 'RackCad.Domain.dll', 'RackCad.UI.dll') {
        $bytes = [Text.Encoding]::UTF8.GetBytes("synthetic $n for $Name " + [guid]::NewGuid())
        [IO.File]::WriteAllBytes((Join-Path $run $n), $bytes)
        $dlls[$n] = [ordered]@{ file = $n; bytes = $bytes.Length; sha256 = (Get-FileHash -Algorithm SHA256 -LiteralPath (Join-Path $run $n)).Hash }
    }
    Set-Content -LiteralPath (Join-Path $root 'logs\build.log') -Value 'synthetic'
    Copy-Item (Join-Path $harnessDir 'run-hostval.ps1'), (Join-Path $harnessDir 'run.scr') (Join-Path $root 'launcher')
    ([ordered]@{
        schemaVersion = 1; unit = 'I-52-AUTH15'
        implementationSha = ('a' * 40); harnessSha = ('b' * 40)
        implementationSrcTree = ('c' * 40); implementationTestsTree = ('d' * 40); harnessSrcTree = ('c' * 40); harnessTestsTree = ('d' * 40)
        treesEqual = $true; dlls = $dlls
    } | ConvertTo-Json -Depth 6) | Out-File (Join-Path $root 'TRANSFER-METADATA.json') -Encoding utf8
    $sums = Get-ChildItem -LiteralPath $root -Recurse -File | Where-Object { $_.Name -ne 'SHA256SUMS' } | Sort-Object FullName | ForEach-Object {
        '{0}  {1}' -f (Get-FileHash -Algorithm SHA256 -LiteralPath $_.FullName).Hash, $_.FullName.Substring($root.Length + 1).Replace('\', '/')
    }
    [IO.File]::WriteAllLines((Join-Path $root 'SHA256SUMS'), $sums)
    return $root
}

# A private registry key with the same shape as the real one. Only keys created here are ever written or removed.
function New-TestRegistry([string]$Name, [string]$Trusted, [Nullable[int]]$SecureLoad = 1) {
    $path = "$regBase\$Name"
    $product = [Microsoft.Win32.Registry]::CurrentUser.CreateSubKey("$path\ACAD-TEST:409")
    $profiles = $product.CreateSubKey('Profiles')
    $profiles.SetValue('', 'TestProfile')
    $vars = $profiles.CreateSubKey('TestProfile\Variables')
    if ($null -ne $Trusted) { $vars.SetValue('TRUSTEDPATHS', $Trusted) }
    if ($null -ne $SecureLoad) { $vars.SetValue('SECURELOAD', [int]$SecureLoad, 'DWord') }
    return "HKCU:\$path"
}

function Invoke-Launcher([string]$Package, [string]$Registry, [hashtable]$Env = @{}, [string]$Drawing = $dwg, [int]$Timeout = 60, [bool]$Confirm = $true) {
    $saved = @{}
    foreach ($k in 'I52_OFFLINE_SCENARIO', 'I52_OFFLINE_FILEDIA', 'I52_OFFLINE_STICKY') { $saved[$k] = [Environment]::GetEnvironmentVariable($k); [Environment]::SetEnvironmentVariable($k, $null) }
    foreach ($k in $Env.Keys) { [Environment]::SetEnvironmentVariable($k, [string]$Env[$k]) }
    try {
        $arguments = @('-NoProfile', '-File', (Join-Path $Package 'launcher\run-hostval.ps1'), '-Package', $Package, '-ScratchDrawing', $Drawing, '-Acad', $acad, '-AutoCadRegistryRoot', $Registry, '-TimeoutSeconds', $Timeout)
        if ($Confirm) { $arguments += '-OwnerConfirmsNoTouch' }
        $output = (& pwsh @arguments 2>&1 | Out-String)
        $code = $LASTEXITCODE
    }
    finally {
        foreach ($k in $saved.Keys) { [Environment]::SetEnvironmentVariable($k, $saved[$k]) }
    }
    $recordPath = Join-Path $Package 'out\launcher-record.json'
    [pscustomobject]@{
        Exit = $code; Output = $output
        Record = $(if (Test-Path -LiteralPath $recordPath) { Get-Content -Raw -LiteralPath $recordPath | ConvertFrom-Json } else { $null })
        Out = Join-Path $Package 'out'
    }
}

# Text with CR removed, or '<missing>' - a missing file is a failed assertion, never a crash of the test rig.
function Read-Text([string]$path) {
    if (-not (Test-Path -LiteralPath $path)) { return '<missing>' }
    return ((Get-Content -Raw -LiteralPath $path) -replace "`r", '')
}

# STATIC AUDIT of every system variable the harness source may read. `$Sources` is name -> C# text. Returns the violations
# (empty = clean) and, via -Literals, the literal names used. Comments are stripped first.
function Invoke-SysVarAudit([hashtable]$Sources, [string[]]$Allowed, [string[]]$KnownInvalid, [ref]$Literals) {
    $violations = New-Object System.Collections.Generic.List[string]
    $used = New-Object System.Collections.Generic.List[string]
    $direct = 0
    foreach ($file in $Sources.Keys) {
        $code = (($Sources[$file] -split "`n") | ForEach-Object { $_ -replace '//.*$', '' }) -join "`n"
        # 1. GetSystemVariable may be called from exactly ONE place (the audited helper) and only with the parameter `name`.
        foreach ($m in [regex]::Matches($code, 'GetSystemVariable\s*\(([^)]*)\)')) {
            $direct++
            if ($m.Groups[1].Value.Trim() -ne 'name') { $violations.Add("$file`: GetSystemVariable called with '$($m.Groups[1].Value.Trim())' instead of the audited helper's 'name'") }
        }
        # 2. Every qualified SysVar.Read / SysVar.ReadInt takes a LITERAL that is in the audited catalog.
        foreach ($m in [regex]::Matches($code, 'SysVar\.(Read|ReadInt)\s*\(([^)]*)\)')) {
            $argument = $m.Groups[2].Value.Trim()
            if ($argument -match '^"([A-Za-z0-9_]+)"$') {
                $used.Add($Matches[1])
                if ($Allowed -notcontains $Matches[1]) { $violations.Add("$file`: SysVar reads '$($Matches[1])', which is not in SysVarCatalog.Audited") }
            }
            else { $violations.Add("$file`: SysVar.$($m.Groups[1].Value) called with a non-literal '$argument'") }
        }
        # 3. A name known to be invalid may not appear as a string literal anywhere in code.
        foreach ($bad in $KnownInvalid) {
            if ($code -match ('"' + [regex]::Escape($bad) + '"') -and $file -ne 'SysVarCatalog.cs') { $violations.Add("$file`: uses the known-INVALID system variable '$bad'") }
        }
    }
    if ($direct -ne 1) { $violations.Add("GetSystemVariable is called in $direct places; it must be exactly 1 (the audited helper)") }
    if ($Literals) { $Literals.Value = @($used | Sort-Object -Unique) }
    return , @($violations)   # a single array object, so an empty result still has .Count under StrictMode
}

function Fresh([string]$Name, [string]$Trusted = '', $SecureLoad = 1) {
    $package = New-TestPackage $Name
    $registry = New-TestRegistry $Name $(if ($Trusted) { $Trusted } else { Join-Path $package 'run' }) $SecureLoad
    return $package, $registry
}

try {
    Write-Host '== pure FILEDIA logic (offlineacad --selftest, real FilediaRecord)'
    $selftest = & $acad --selftest
    Assert-That 'T00' 'FILEDIA parser/validator self-test (13 cases)' ($LASTEXITCODE -eq 0 -and @($selftest | Where-Object { $_ -like 'PASS *' }).Count -eq 13) ($selftest -join ' | ')

    Write-Host '== exit semantics and the record'
    $p, $r = Fresh 'T01-pass'
    $x = Invoke-Launcher $p $r
    $fdText = Read-Text (Join-Path $x.Out 'filedia.txt')
    Assert-That 'T01' 'PASS verdict + valid launch => exit 0' ($x.Exit -eq 0) "exit=$($x.Exit)"
    Assert-That 'T01' 'RUN_RESULT = PASS is the last line' ($x.Output.Trim().EndsWith('RUN_RESULT = PASS')) $x.Output
    Assert-That 'T01' 'record: launchValid, runResult PASS, every check true' ($x.Record.launchValid -eq $true -and $x.Record.runResult -eq 'PASS' -and -not ($x.Record.checks.PSObject.Properties.Value -contains $false))
    Assert-That 'T01' 'filedia.txt is exactly before/during/after with the original restored' ($fdText -eq "before=1`nduring=0`nafter=1`n") $fdText

    $p, $r = Fresh 'T02-fail'
    $x = Invoke-Launcher $p $r @{ I52_OFFLINE_SCENARIO = 'fail' }
    Assert-That 'T02' 'valid launch + FAIL => exit 3, RUN_RESULT = FAIL' ($x.Exit -eq 3 -and $x.Output.Trim().EndsWith('RUN_RESULT = FAIL')) "exit=$($x.Exit)"

    $p, $r = Fresh 'T03-unknown'
    $x = Invoke-Launcher $p $r @{ I52_OFFLINE_SCENARIO = 'unknown' }
    Assert-That 'T03' 'valid launch + UNKNOWN => exit 3, RUN_RESULT = UNKNOWN' ($x.Exit -eq 3 -and $x.Output.Trim().EndsWith('RUN_RESULT = UNKNOWN')) "exit=$($x.Exit)"

    $p, $r = Fresh 'T04-malformed'
    $x = Invoke-Launcher $p $r @{ I52_OFFLINE_SCENARIO = 'malformed' }
    Assert-That 'T04' 'malformed evidence => launcher-record.json STILL written' ($null -ne $x.Record)
    Assert-That 'T04' 'malformed evidence => evidenceParseable=false, launchValid=false, INVALID, exit 2' ($null -ne $x.Record -and $x.Record.checks.evidenceParseable -eq $false -and $x.Record.launchValid -eq $false -and $x.Record.runResult -eq 'INVALID' -and $x.Exit -eq 2) "exit=$($x.Exit)"
    Assert-That 'T04' 'every evidence-derived check is false' ($null -ne $x.Record -and -not $x.Record.checks.evidenceSchema -and -not $x.Record.checks.evidencePluginHash -and -not $x.Record.checks.evidenceFinalAndCompleted)
    Assert-That 'T04' 'LAUNCH_VALID is printed' ($x.Output -match 'LAUNCH_VALID=False')

    $p, $r = Fresh 'T05-noevidence'
    $x = Invoke-Launcher $p $r @{ I52_OFFLINE_SCENARIO = 'noevidence' }
    Assert-That 'T05' 'no evidence => exit 2, INVALID, record written' ($x.Exit -eq 2 -and $null -ne $x.Record -and $x.Record.verdict -eq 'NO_EVIDENCE' -and $x.Output.Trim().EndsWith('RUN_RESULT = INVALID'))

    $p, $r = Fresh 'T06-hash'
    $x = Invoke-Launcher $p $r @{ I52_OFFLINE_SCENARIO = 'hashmismatch' }
    Assert-That 'T06' 'evidence Plugin hash mismatch => exit 2' ($x.Exit -eq 2 -and $x.Record.checks.evidencePluginHash -eq $false)

    $p, $r = Fresh 'T07-nonzero'
    $x = Invoke-Launcher $p $r @{ I52_OFFLINE_SCENARIO = 'nonzero-exit' }
    Assert-That 'T07' 'non-zero AutoCAD exit is INVALID even though the evidence says PASS' ($x.Exit -eq 2 -and $x.Record.verdict -eq 'PASS' -and $x.Record.runResult -eq 'INVALID' -and $x.Record.checks.processExitedCleanly -eq $false)

    Write-Host '== timeout, FILEDIA'
    $p, $r = Fresh 'T08-timeout'
    $x = Invoke-Launcher $p $r @{ I52_OFFLINE_SCENARIO = 'timeout' } -Timeout 6
    $fdText = Read-Text (Join-Path $x.Out 'filedia.txt')
    Assert-That 'T08' 'timeout => killed, timedOut=true, INVALID, exit 2' ($x.Exit -eq 2 -and $x.Record.timedOut -eq $true -and $x.Record.runResult -eq 'INVALID')
    Assert-That 'T08' 'filedia.txt shows before and during only (no after)' ($fdText -eq "before=1`nduring=0`n") $fdText
    Assert-That 'T08' 'the original FILEDIA is printed prominently, and the launcher wrote nothing itself' ($x.Output -match 'FILEDIA RECOVERY' -and $x.Output -match 'captured by run\.scr is: 1' -and $x.Output -match 'never writes FILEDIA')

    $p, $r = Fresh 'T09-sticky'
    $x = Invoke-Launcher $p $r @{ I52_OFFLINE_STICKY = 'lock0' }
    $fdText = Read-Text (Join-Path $x.Out 'filedia.txt')
    Assert-That 'T09' 'restore fails (stays 0): run.scr records after=0 and does NOT invoke the harness command' ($fdText -eq "before=1`nduring=0`nafter=0`n" -and -not (Test-Path (Join-Path $x.Out 'hostval-evidence.json'))) $fdText
    Assert-That 'T09' 'launcher: filediaRestored=false, INVALID, exit 2, recovery text names the original (1)' ($x.Exit -eq 2 -and $x.Record.checks.filediaRestored -eq $false -and $x.Output -match 'captured by run\.scr is: 1')

    foreach ($original in 0, 2) {
        $p, $r = Fresh "T10-orig$original"
        $x = Invoke-Launcher $p $r @{ I52_OFFLINE_FILEDIA = $original }
        $fdText = Read-Text (Join-Path $x.Out 'filedia.txt')
        Assert-That 'T10' "original FILEDIA=$original is restored exactly (never assumed 1) => exit 0" ($x.Exit -eq 0 -and $fdText -eq "before=$original`nduring=0`nafter=$original`n") $fdText
    }

    $p, $r = Fresh 'T11-start-shift'
    $x = Invoke-Launcher $p $r @{ I52_OFFLINE_SCENARIO = 'live-start-shift' }
    $ev = (Read-Text (Join-Path $x.Out 'hostval-evidence.json')) | ConvertFrom-Json
    Assert-That 'T11' 'harness start: live FILEDIA != before => NO case runs, StoppedBy set, INVALID' ($x.Exit -eq 2 -and $ev.stoppedBy -eq 'FILEDIA not restored: INVALID RUN' -and @($ev.cases | Where-Object { $_.result -ne 'NOT_RUN' }).Count -eq 0)

    $p, $r = Fresh 'T12-end-shift'
    $x = Invoke-Launcher $p $r @{ I52_OFFLINE_SCENARIO = 'live-end-shift' }
    Assert-That 'T12' 'harness end: live FILEDIA != before => INVALID' ($x.Exit -eq 2 -and $x.Record.checks.evidenceFilediaMatchesRecord -eq $false)

    Write-Host '== refusals before launch (no process is started)'
    $p, $r = Fresh 'T13-nondwg'
    $x = Invoke-Launcher $p $r -Drawing $notDwg
    Assert-That 'T13' 'scratch with no DWG signature => refused, exit 2, nothing launched' ($x.Exit -eq 2 -and $x.Output -match 'DWG signature' -and -not (Test-Path (Join-Path $p 'out\run.scr')))
    $x = Invoke-Launcher $p $r -Drawing $txt
    Assert-That 'T13' 'scratch that is not a .dwg => refused' ($x.Exit -eq 2 -and $x.Output -match 'must be a \.dwg')
    $x = Invoke-Launcher $p $r -Drawing (Join-Path $Work 'missing.dwg')
    Assert-That 'T13' 'scratch that does not exist => refused' ($x.Exit -eq 2 -and $x.Output -match 'not found')

    $p = New-TestPackage 'T14-untrusted'
    $r = New-TestRegistry 'T14-untrusted' 'C:\somewhere\else;D:\other\...'
    $x = Invoke-Launcher $p $r
    Assert-That 'T14' 'run folder not in TRUSTEDPATHS => TRUSTEDPATH_REQUIRED, exit 2, nothing launched' ($x.Exit -eq 2 -and $x.Output -match 'TRUSTEDPATH_REQUIRED' -and -not (Test-Path (Join-Path $p 'out\run.scr')))

    $p = New-TestPackage 'T15-undetermined'
    $x = Invoke-Launcher $p 'HKCU:\Software\I52Auth15OfflineTest-DOES-NOT-EXIST'
    Assert-That 'T15' 'registry cannot be read => TRUSTEDPATH_UNDETERMINED, exit 2 (the launcher does not guess)' ($x.Exit -eq 2 -and $x.Output -match 'TRUSTEDPATH_UNDETERMINED')

    $p = New-TestPackage 'T16-secureload0'
    $r = New-TestRegistry 'T16-secureload0' 'C:\nowhere' 0
    $x = Invoke-Launcher $p $r
    Assert-That 'T16' 'SECURELOAD=0 => trust not required, run proceeds' ($x.Exit -eq 0 -and $x.Output -match 'NOT_REQUIRED')

    $p = New-TestPackage 'T17-recursive'
    $r = New-TestRegistry 'T17-recursive' ($Work + '\...')
    $x = Invoke-Launcher $p $r
    Assert-That 'T17' 'a recursive (...) entry above the run folder counts as trusted' ($x.Exit -eq 0 -and $x.Output -match 'recursive entry')

    $p, $r = Fresh 'T18-noconfirm'
    $x = Invoke-Launcher $p $r -Confirm $false
    Assert-That 'T18' 'without -OwnerConfirmsNoTouch => refused, exit 2' ($x.Exit -eq 2 -and $x.Output -match 'nobody touches' -and -not (Test-Path (Join-Path $p 'out\run.scr')))

    $p, $r = Fresh 'T19-drift'
    [IO.File]::AppendAllText((Join-Path $p 'run\RackCad.Domain.dll'), 'tamper')
    $x = Invoke-Launcher $p $r
    Assert-That 'T19' 'package file changed after SHA256SUMS => refused, exit 2' ($x.Exit -eq 2 -and $x.Output -match 'differ from SHA256SUMS')
    $p, $r = Fresh 'T19b-extra'
    Set-Content -LiteralPath (Join-Path $p 'run\extra.txt') -Value 'x'
    $x = Invoke-Launcher $p $r
    Assert-That 'T19' 'unlisted file in run\ => refused, exit 2' ($x.Exit -eq 2 -and $x.Output -match 'not in SHA256SUMS')

    $p, $r = Fresh 'T20-running'
    $psi = New-Object Diagnostics.ProcessStartInfo($acad)
    $psi.UseShellExecute = $false
    $psi.Environment['I52_OFFLINE_SCENARIO'] = 'sleep'
    $background = [Diagnostics.Process]::Start($psi)
    try {
        Start-Sleep -Milliseconds 800
        $x = Invoke-Launcher $p $r
        Assert-That 'T20' 'an already-running AutoCAD process => refused, exit 2, nothing launched' ($x.Exit -eq 2 -and $x.Output -match 'already running' -and -not (Test-Path (Join-Path $p 'out\run.scr')))
    }
    finally { if (-not $background.HasExited) { $background.Kill($true) } }

    $p, $r = Fresh 'T21-leftover'
    New-Item -ItemType Directory -Force (Join-Path $p 'out') | Out-Null
    Set-Content -LiteralPath (Join-Path $p 'out\hostval-evidence.json') -Value '{}'
    $x = Invoke-Launcher $p $r
    Assert-That 'T21' 'existing evidence => refused (a run is never repeated or overwritten)' ($x.Exit -eq 2 -and $x.Output -match 'already holds evidence')

    Write-Host '== system variables (HC-5 / HC-6): RUN-1 was PROFILENAME -> eInvalidInput'
    $catalogText = Get-Content -Raw (Join-Path $harnessDir 'SysVarCatalog.cs')
    $catalogBlock = [regex]::Match($catalogText, 'Audited\s*=\s*\{(.*?)\};', 'Singleline').Groups[1].Value
    $auditedNames = @([regex]::Matches(($catalogBlock -split "`n" | ForEach-Object { $_ -replace '//.*$', '' } | Out-String), '"([A-Z0-9_]+)"') | ForEach-Object { $_.Groups[1].Value } | Sort-Object)
    $invalidBlock = [regex]::Match($catalogText, 'KnownInvalid\s*=\s*\{(.*?)\};', 'Singleline').Groups[1].Value
    $invalidNames = @([regex]::Matches(($invalidBlock -split "`n" | ForEach-Object { $_ -replace '//.*$', '' } | Out-String), '"([A-Z0-9_]+)"') | ForEach-Object { $_.Groups[1].Value })

    # The reviewed list. A NEW system-variable name must be added HERE (and to SysVarCatalog) by a reviewer; nothing else passes.
    $reviewed = @('ACADVER', 'CPROFILE', 'FILEDIA')
    Assert-That 'T23' 'SysVarCatalog.Audited equals the reviewed list exactly' (($auditedNames -join ',') -eq (($reviewed | Sort-Object) -join ',')) ($auditedNames -join ',')
    Assert-That 'T23' 'PROFILENAME is recorded as known-invalid and is NOT audited' ($invalidNames -contains 'PROFILENAME' -and $auditedNames -notcontains 'PROFILENAME')

    $sources = @{}
    Get-ChildItem -LiteralPath $harnessDir -Filter '*.cs' | ForEach-Object { $sources[$_.Name] = Get-Content -Raw $_.FullName }
    $literals = $null
    $violations = Invoke-SysVarAudit $sources $auditedNames $invalidNames ([ref]$literals)
    Assert-That 'T24' 'static audit of the real harness source: no violations' ($violations.Count -eq 0) ($violations -join ' | ')
    Assert-That 'T24' 'every system-variable literal the harness passes is in the reviewed list, and only through SysVar.Read/ReadInt' (@($literals | Where-Object { $reviewed -notcontains $_ }).Count -eq 0) ($literals -join ',')
    Assert-That 'T24' 'the harness reads exactly the reviewed system variables (ACADVER, CPROFILE, FILEDIA): no more, no fewer' (($literals -join ',') -eq (($reviewed | Sort-Object) -join ',')) ($literals -join ',')

    # Self-test of the audit itself: it must catch the RUN-1 class of defect and accept the fixed one.
    $good = @{ 'SysVarCatalog.cs' = $catalogText; 'Helper.cs' = 'class SysVar { static object R(string name) { return X.GetSystemVariable(name); } } class C { void M() { var p = SysVar.Read("CPROFILE"); var f = SysVar.ReadInt("FILEDIA"); } }' }
    Assert-That 'T25' 'audit self-test: CPROFILE through the helper => clean' ((Invoke-SysVarAudit $good $auditedNames $invalidNames ([ref]$null)).Count -eq 0)
    $runOne = @{ 'Helper.cs' = 'class SysVar { static object R(string name) { return X.GetSystemVariable(name); } } class C { void M() { var p = Convert.ToString(Y.GetSystemVariable("PROFILENAME")); } }' }
    Assert-That 'T25' 'audit self-test: the RUN-1 code (direct GetSystemVariable("PROFILENAME")) => caught' ((Invoke-SysVarAudit $runOne $auditedNames $invalidNames ([ref]$null)).Count -ge 2)
    $viaHelper = @{ 'Helper.cs' = 'class SysVar { static object R(string name) { return X.GetSystemVariable(name); } } class C { void M() { var p = SysVar.Read("PROFILENAME"); } }' }
    Assert-That 'T25' 'audit self-test: PROFILENAME even through the helper => caught (not audited, known-invalid)' ((Invoke-SysVarAudit $viaHelper $auditedNames $invalidNames ([ref]$null)).Count -ge 1)
    $unknown = @{ 'Helper.cs' = 'class SysVar { static object R(string name) { return X.GetSystemVariable(name); } } class C { void M() { var p = SysVar.Read("BOGUSVAR"); } }' }
    Assert-That 'T25' 'audit self-test: an unknown new name => caught until reviewed' ((Invoke-SysVarAudit $unknown $auditedNames $invalidNames ([ref]$null)).Count -ge 1)
    $nonLiteral = @{ 'Helper.cs' = 'class SysVar { static object R(string name) { return X.GetSystemVariable(name); } } class C { void M(string v) { var p = SysVar.Read(v); } }' }
    Assert-That 'T25' 'audit self-test: a computed name => caught' ((Invoke-SysVarAudit $nonLiteral $auditedNames $invalidNames ([ref]$null)).Count -ge 1)
    $second = @{ 'Helper.cs' = 'class SysVar { static object R(string name) { return X.GetSystemVariable(name); } } class C { void M() { var p = Z.GetSystemVariable(name); } }' }
    Assert-That 'T25' 'audit self-test: a second GetSystemVariable call site => caught' ((Invoke-SysVarAudit $second $auditedNames $invalidNames ([ref]$null)).Count -ge 1)

    foreach ($name in $auditedNames) {
        & $acad --sysvar $name | Out-Null
        Assert-That 'T26' "mock AutoCAD accepts the audited name $name" ($LASTEXITCODE -eq 0)
    }
    foreach ($name in $invalidNames + 'BOGUSVAR') {
        $text = (& $acad --sysvar $name) -join ' '
        Assert-That 'T26' "mock AutoCAD rejects $name with eInvalidInput (characterization failure if the harness read it)" ($LASTEXITCODE -eq 1 -and $text -match 'eInvalidInput') $text
    }

    Write-Host '== early identity (HC-7) and RUN-1 reproduction'
    $p, $r = Fresh 'T27-hv00-throws'
    $x = Invoke-Launcher $p $r @{ I52_OFFLINE_SCENARIO = 'hv00-throws' }
    $ev = (Read-Text (Join-Path $x.Out 'hostval-evidence.json')) | ConvertFrom-Json
    Assert-That 'T27' 'RUN-1 reproduced: HV-00 throws eInvalidInput => INVALID, exit 2, HV-01..14 NOT_RUN' ($x.Exit -eq 2 -and $ev.cases[0].result -eq 'UNKNOWN' -and $ev.cases[0].exception -match 'eInvalidInput' -and @($ev.cases | Where-Object { $_.result -eq 'NOT_RUN' }).Count -eq 14)
    Assert-That 'T27' 'yet the immutable run identity WAS persisted before HV-00: pid, process start, run folder, harness DLL, scratch hashes, expected package hashes, Owner flag' ($ev.host.pid -eq $x.Record.pid -and $ev.host.processStartUtc -and $ev.host.runFolder -and $ev.host.harnessDllSha256 -and $ev.host.scratchSha256AtHarnessStart -and $ev.host.scratchSha256DeclaredByLauncher -and $ev.host.ownerNoTouchDeclaredByLauncher -eq $true -and $ev.package.expectedPluginSha256 -and $ev.package.expectedApplicationSha256 -and $ev.package.expectedDomainSha256 -and $ev.package.expectedHarnessSha256)
    Assert-That 'T27' 'the launcher ties that early evidence to this launch (pid and process start) even though HV-00 failed' ($x.Record.checks.evidenceFromThisProcess -eq $true -and $x.Record.checks.evidenceProcessStart -eq $true -and $x.Record.checks.evidenceScratchIdentity -eq $true -and $x.Record.checks.evidenceOwnerNoTouchDeclared -eq $true)
    Assert-That 'T27' 'loaded-assembly identity is NOT fabricated: no loaded* facts exist, so the hash checks fail' (@($ev.binding.PSObject.Properties | Where-Object { $_.Name -like 'loaded*' }).Count -eq 0 -and $x.Record.checks.evidencePluginHash -eq $false)
    Assert-That 'T27' 'FILEDIA was still recorded and verified' ($ev.host.filediaBefore -eq 1 -and $ev.host.filediaAfter -eq 1 -and $x.Record.checks.filediaRestored -eq $true)

    $p, $r = Fresh 'T28-layout'
    $x = Invoke-Launcher $p $r
    $ev = (Read-Text (Join-Path $x.Out 'hostval-evidence.json')) | ConvertFrom-Json
    Assert-That 'T28' 'expected package hashes and loaded assembly hashes are distinct evidence keys, and equal on a good run' ($x.Exit -eq 0 -and $ev.package.expectedPluginSha256 -eq $ev.binding.loadedPluginSha256 -and $ev.package.expectedHarnessSha256 -eq $ev.binding.loadedHarnessSha256 -and -not $ev.package.PSObject.Properties['pluginSha256'])
    Assert-That 'T28' 'the offline rig stamps its evidence offlineRig=true (never mistakable for host evidence)' ($ev.offlineRig -eq $true)

    $p, $r = Fresh 'T29-forged-pass'
    $x = Invoke-Launcher $p $r @{ I52_OFFLINE_SCENARIO = 'forged-pass' }
    Assert-That 'T29' 'a PASS verdict not backed by the cases (a FAIL case, a leak) => launcher recomputes, INVALID, exit 2' ($x.Exit -eq 2 -and $x.Record.verdict -eq 'PASS' -and $x.Record.checks.evidenceVerdictConsistent -eq $false -and $x.Record.runResult -eq 'INVALID')

    Write-Host '== run.scr'
    $template = Get-Content -Raw (Join-Path $harnessDir 'run.scr')
    Assert-That 'T22' 'run.scr never hard-codes a restore value: no "FILEDIA 1", no (setvar "FILEDIA" 1)' ($template -notmatch '(?im)^\s*_?\.?FILEDIA\s*$' -and $template -notmatch '\(setvar "FILEDIA" 1\)')
    Assert-That 'T22' 'run.scr restores from the captured variable and gates the harness command on after == before' ($template -match '\(setvar "FILEDIA" i52hv-fd-before\)' -and $template -match '\(if \(= i52hv-fd-after i52hv-fd-before\) \(command "I52AUTH15_HOSTVAL"\)\)')
    Assert-That 'T22' 'run.scr LISP parses (balanced, terminated) and the real file runs through the interpreter (T01/T10)' (@($script:Results | Where-Object { ($_.Test -eq 'T01' -or $_.Test -eq 'T10') -and -not $_.Pass }).Count -eq 0)
}
finally {
    try { [Microsoft.Win32.Registry]::CurrentUser.DeleteSubKeyTree($regBase, $false) } catch { }
}

$failed = @($script:Results | Where-Object { -not $_.Pass })
$summary = "OFFLINE_LAUNCHER_TESTS: $($script:Results.Count - $failed.Count)/$($script:Results.Count) assertions PASS, $($failed.Count) FAIL  (NON-HOST-EVIDENCE; work dir $Work)"
Write-Host $summary
$summary | Out-File (Join-Path $Work 'offline-results.txt') -Encoding utf8
$script:Results | ForEach-Object { '{0} {1} {2}' -f $(if ($_.Pass) { 'PASS' } else { 'FAIL' }), $_.Test, $_.Name } | Out-File (Join-Path $Work 'offline-results.txt') -Append -Encoding utf8
if ($failed.Count -gt 0) { exit 1 }
exit 0
