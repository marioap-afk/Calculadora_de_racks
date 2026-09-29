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

function Update-PackageSums([string]$Root) {
    $sums = Get-ChildItem -LiteralPath $Root -Recurse -File | Where-Object { $_.Name -ne 'SHA256SUMS' -and $_.FullName -notlike (Join-Path $Root 'out\*') } | Sort-Object FullName | ForEach-Object {
        '{0}  {1}' -f (Get-FileHash -Algorithm SHA256 -LiteralPath $_.FullName).Hash, $_.FullName.Substring($Root.Length + 1).Replace('\', '/')
    }
    [IO.File]::WriteAllLines((Join-Path $Root 'SHA256SUMS'), $sums)
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
    $summary = @($selftest | Where-Object { $_ -like 'SELFTEST *' })
    Assert-That 'T00' 'pure logic self-test (FILEDIA record + transaction-end rules): every case passes' ($LASTEXITCODE -eq 0 -and @($selftest | Where-Object { $_ -like 'FAIL *' }).Count -eq 0 -and $summary.Count -eq 1 -and $summary[0] -match '^SELFTEST (\d+)/\1$' -and [int]($summary[0] -replace '^SELFTEST (\d+)/.*', '$1') -ge 20) ($selftest -join ' | ')

    Write-Host '== exit semantics and the record'
    $p, $r = Fresh 'T01-pass'
    $x = Invoke-Launcher $p $r
    $fdText = Read-Text (Join-Path $x.Out 'filedia.txt')
    Assert-That 'T01' 'PASS verdict + valid launch => exit 0' ($x.Exit -eq 0) "exit=$($x.Exit)"
    Assert-That 'T01' 'RUN_RESULT = PASS is the last line' ($x.Output.Trim().EndsWith('RUN_RESULT = PASS')) $x.Output
    Assert-That 'T01' 'record: launchValid, runResult PASS, every check true' ($x.Record.launchValid -eq $true -and $x.Record.runResult -eq 'PASS' -and -not ($x.Record.checks.PSObject.Properties.Value -contains $false))
    $ruledChecks = @('processExitedCleanly', 'noOtherAcadStarted', 'scratchUnchanged', 'blankTemplateUnchanged', 'packageUnchangedAfterRun', 'filediaRecorded', 'filediaRestored',
        'evidenceExists', 'evidenceParseable', 'evidenceSchema', 'evidenceFromThisProcess', 'evidenceBoundToPackageSums', 'evidencePluginHash', 'evidenceApplicationHash',
        'evidenceDomainHash', 'evidenceHarnessHash', 'evidenceProcessStart', 'evidenceOwnerNoTouchDeclared', 'evidenceScratchIdentity', 'evidenceEarlyIdentityClean',
        'evidenceVerdictConsistent', 'evidenceHarnessShaEqualsMetadata', 'evidenceImplementationShaEqualsMetadata', 'evidenceTreesEqualMetadata',
        'evidenceFilediaMatchesRecord', 'evidenceFinal', 'evidenceRunShape', 'evidenceControlSet', 'evidenceRollbackCasesDocument')
    $actualChecks = @($x.Record.checks.PSObject.Properties | ForEach-Object { $_.Name } | Sort-Object)
    Assert-That 'T01' 'the launcher evaluates EXACTLY the ruled set of 29 checks (a deleted check would otherwise go unnoticed: "all true" cannot see a missing one)' (($actualChecks -join ',') -eq (($ruledChecks | Sort-Object) -join ',')) ("missing: " + (($ruledChecks | Where-Object { $actualChecks -notcontains $_ }) -join ',') + "; extra: " + (($actualChecks | Where-Object { $ruledChecks -notcontains $_ }) -join ','))
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
    Assert-That 'T04' 'every evidence-derived check is false' ($null -ne $x.Record -and -not $x.Record.checks.evidenceSchema -and -not $x.Record.checks.evidencePluginHash -and -not $x.Record.checks.evidenceFinal)
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
    Assert-That 'T11' 'harness start: the FILEDIA stop is machine-readable (stopKind = filedia)' ($ev.stopKind -eq 'filedia')
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
    Assert-That 'T27' 'RUN-1 reproduced: the HV-00 stop is machine-readable (stopKind = hv00) and stays INVALID' ($ev.stopKind -eq 'hv00' -and $x.Record.checks.evidenceRunShape -eq $false)
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

    Write-Host '== classification (F-3): a governed stop after real AUTH-15 work is FAIL, not INVALID'
    $p, $r = Fresh 'T30-deviation-continue'
    $x = Invoke-Launcher $p $r @{ I52_OFFLINE_SCENARIO = 'deviation-continue' }
    $ev = (Read-Text (Join-Path $x.Out 'hostval-evidence.json')) | ConvertFrom-Json
    Assert-That 'T30' 'HV-08 deviation: recorded, the run CONTINUES (HV-09..HV-14 executed), completed, stopKind none' ($ev.completed -eq $true -and $ev.stopKind -eq 'none' -and @($ev.deviations) -contains 'HV-08' -and @($ev.cases | Where-Object { $_.id -in 'HV-09', 'HV-10', 'HV-11', 'HV-12', 'HV-13', 'HV-14' -and $_.result -eq 'PASS' }).Count -eq 6)
    Assert-That 'T30' 'valid launch + FAIL => RUN_RESULT FAIL, exit 3' ($x.Exit -eq 3 -and $x.Record.launchValid -eq $true -and $x.Record.runResult -eq 'FAIL')

    $p, $r = Fresh 'T31-deviation-stop'
    $x = Invoke-Launcher $p $r @{ I52_OFFLINE_SCENARIO = 'deviation-stop' }
    $ev = (Read-Text (Join-Path $x.Out 'hostval-evidence.json')) | ConvertFrom-Json
    Assert-That 'T31' 'the RUN-2 shape (completed=false, stopKind=deviation, FAIL after real cases, HV-09.. NOT_RUN) is a VALID FAIL, not INVALID' ($ev.completed -eq $false -and $ev.stopKind -eq 'deviation' -and $x.Exit -eq 3 -and $x.Record.launchValid -eq $true -and $x.Record.runResult -eq 'FAIL' -and $x.Record.checks.evidenceRunShape -eq $true -and $x.Output.Trim().EndsWith('RUN_RESULT = FAIL'))

    $p, $r = Fresh 'T32-exception-stop'
    $x = Invoke-Launcher $p $r @{ I52_OFFLINE_SCENARIO = 'exception-stop' }
    Assert-That 'T32' 'an unfinished run stopped by an exception/environment problem stays INVALID even with a FAIL verdict' ($x.Exit -eq 2 -and $x.Record.runResult -eq 'INVALID' -and $x.Record.checks.evidenceRunShape -eq $false)

    $p, $r = Fresh 'T33-unknown-stop'
    $x = Invoke-Launcher $p $r @{ I52_OFFLINE_SCENARIO = 'unknown-stop' }
    Assert-That 'T33' 'an unfinished run whose verdict is not FAIL cannot borrow the governed-stop exception: INVALID' ($x.Exit -eq 2 -and $x.Record.runResult -eq 'INVALID')

    $p, $r = Fresh 'T34-forged-fail'
    $x = Invoke-Launcher $p $r @{ I52_OFFLINE_SCENARIO = 'forged-fail' }
    Assert-That 'T34' 'a FAIL verdict with no failing case and no deviation behind it is not believed: INVALID' ($x.Exit -eq 2 -and $x.Record.checks.evidenceVerdictConsistent -eq $false)

    Write-Host '== controls, labels and abort instrumentation (evidence shape)'
    $p, $r = Fresh 'T35-controls-leak'
    $x = Invoke-Launcher $p $r @{ I52_OFFLINE_SCENARIO = 'controls-leak' }
    $ev = (Read-Text (Join-Path $x.Out 'hostval-evidence.json')) | ConvertFrom-Json
    $rb01 = @($ev.controls | Where-Object { $_.id -eq 'RB-01' })
    $rb01d = @($ev.controls | Where-Object { $_.id -eq 'RB-01D' })
    Assert-That 'T35' 'a control that leaks is FAIL with its leaked keys and handles enumerated; RB-01D stays PASS' ($rb01.Count -eq 1 -and $rb01[0].result -eq 'FAIL' -and $rb01[0].dbKind -eq 'SIDE-DB' -and @($rb01[0].leaks).Count -eq 2 -and $rb01d.Count -eq 1 -and $rb01d[0].result -eq 'PASS' -and $rb01d[0].dbKind -eq 'DOCUMENT-AUTHORITY')
    Assert-That 'T35' 'control leaks are listed at the top level (controlLeaks), stay out of the HV leaks, and the failing control makes the campaign FAIL (controls govern the verdict)' (@($ev.controlLeaks).Count -eq 2 -and @($ev.leaks).Count -eq 0 -and $ev.verdict -eq 'FAIL' -and $x.Exit -eq 3)
    Assert-That 'T35' 'every control of the ruling is present: RB-01, RB-01V, RB-01D, RB-02a/b/c, RB-03, RB-05' ((@($ev.controls | ForEach-Object { $_.id } | Sort-Object -Unique) -join ',') -eq 'RB-01,RB-01D,RB-01V,RB-02a,RB-02b,RB-02c,RB-03,RB-05')
    Assert-That 'T35' 'RB-02a..05 run on BOTH the side database and the document database' (@($ev.controls | Where-Object { $_.id -eq 'RB-03' } | ForEach-Object { $_.dbKind } | Sort-Object) -join ',' -eq 'DOCUMENT-AUTHORITY,SIDE-DB')

    $p, $r = Fresh 'T36-abort-throws'
    $x = Invoke-Launcher $p $r @{ I52_OFFLINE_SCENARIO = 'abort-throws' }
    $ev = (Read-Text (Join-Path $x.Out 'hostval-evidence.json')) | ConvertFrom-Json
    $rb01 = @($ev.controls | Where-Object { $_.id -eq 'RB-01' })[0]
    Assert-That 'T36' 'an Abort that throws is captured (attempted, not succeeded, the exception text) and FAILS the control' ($rb01.result -eq 'FAIL' -and $rb01.tx[0].abortAttempted -eq $true -and $rb01.tx[0].abortSucceeded -eq $false -and $rb01.tx[0].abortError -match 'eInvalidInput' -and @($rb01.assertions | Where-Object { $_.name -like '*Abort threw*' -and $_.result -eq 'FAIL' }).Count -eq 1)
    Assert-That 'T36' 'the transaction outcome serializes every recorded field' ($rb01.tx[0].PSObject.Properties.Name -contains 'disposeAttempted' -and $rb01.tx[0].PSObject.Properties.Name -contains 'disposeSucceeded' -and $rb01.tx[0].PSObject.Properties.Name -contains 'isDisposedAfter' -and $rb01.tx[0].PSObject.Properties.Name -contains 'activeBefore' -and $rb01.tx[0].PSObject.Properties.Name -contains 'activeAfter' -and $rb01.tx[0].PSObject.Properties.Name -contains 'topAfter')

    $p, $r = Fresh 'T37-labels'
    $x = Invoke-Launcher $p $r
    $ev = (Read-Text (Join-Path $x.Out 'hostval-evidence.json')) | ConvertFrom-Json
    $authority = @($ev.cases | Where-Object { $_.dbKind -eq 'DOCUMENT-AUTHORITY' } | ForEach-Object { $_.id } | Sort-Object)
    Assert-That 'T37' 'the rollback-sensitive cases are labelled DOCUMENT-AUTHORITY and the others SIDE-DB' (($authority -join ',') -eq 'HV-01,HV-02,HV-03,HV-05,HV-08,HV-10,HV-12,HV-13,HV-14' -and @($ev.cases | Where-Object { $_.dbKind -eq 'SIDE-DB' }).Count -eq 6)
    Assert-That 'T37' 'their side-database runs are kept apart, labelled SIDE-DB-CHARACTERIZATION, and are not among the 15 verdict cases' (@($ev.sideCharacterizations).Count -eq 9 -and @($ev.sideCharacterizations | Where-Object { $_.dbKind -ne 'SIDE-DB-CHARACTERIZATION' }).Count -eq 0 -and @($ev.cases).Count -eq 15)
    Assert-That 'T37' 'the document-authority state is recorded' ($ev.documentAuthority.available -eq $true)
    Assert-That 'T37' 'the launcher provided an untouched blank template for the document cases, and it is unchanged' ((Test-Path (Join-Path $x.Out 'blank-template.dwg')) -and $x.Record.checks.blankTemplateUnchanged -eq $true -and $x.Record.blankTemplate.sha256Before -eq $x.Record.blankTemplate.sha256After -and $x.Record.blankTemplate.sha256Before -eq $x.Record.scratch.sha256Before)

    Write-Host '== AUTH-15 effective-name postcondition (source audit; the behaviour itself is proven only on the host)'
    $creatorPath = Join-Path (Split-Path (Split-Path (Split-Path $harnessDir))) 'src\RackCad.Plugin\Systems\Shared\RackDefinitionCreator.cs'
    function Test-CreatorSource([string]$Text) {
        $code = (($Text -split "`n") | ForEach-Object { $_ -replace '//.*$', '' }) -join "`n"
        $problems = New-Object System.Collections.Generic.List[string]
        $entries = @([regex]::Matches($code, 'internal static RackDefinitionCreationResult CreateInTransaction\('))
        if ($entries.Count -ne 2) { $problems.Add("expected 2 CreateInTransaction entries, found $($entries.Count)"); return , @($problems) }
        $first = $code.Substring($entries[0].Index, $entries[1].Index - $entries[0].Index)
        $second = $code.Substring($entries[1].Index, [Math]::Min(2500, $code.Length - $entries[1].Index))
        foreach ($pair in @(@('HeaderRun', $first), @('Cantilever', $second))) {
            $at = $pair[1].IndexOf('UnusableEffectiveName(')
            $envelope = $pair[1].IndexOf('Envelope(')
            if ($at -lt 0) { $problems.Add("$($pair[0]): the effective name is never validated") }
            elseif ($envelope -ge 0 -and $at -gt $envelope) { $problems.Add("$($pair[0]): the effective name is validated AFTER the envelope") }
        }
        if ($first.IndexOf('UnusableEffectiveName(') -gt $first.IndexOf('HasMissingBlocks')) { $problems.Add('HeaderRun: the effective name is validated after the missing-blocks handling') }
        $check = [regex]::Match($code, 'UnusableEffectiveName\(string effectiveName\)(.*?)\n        \}', 'Singleline').Value
        if ($check -notmatch 'IsNullOrWhiteSpace\(effectiveName\)' -or $check -notmatch 'RackDefinitionCreationFailure\.WriteFailed') { $problems.Add('the postcondition does not return WriteFailed for a blank effective name') }
        foreach ($forbidden in 'BlockNaming', 'Sanitize', 'UniqueBlockName', '.Replace(', '.Trim(') { if ($code.Contains($forbidden)) { $problems.Add("AUTH-15 references '$forbidden' (a second naming policy)") } }
        return , @($problems)
    }
    $creatorText = Get-Content -Raw $creatorPath
    Assert-That 'T38' 'AUTH-15 source: the effective name is validated in BOTH paths, before the envelope (and before the missing-blocks handling), as WriteFailed, without deriving a name' ((Test-CreatorSource $creatorText).Count -eq 0) ((Test-CreatorSource $creatorText) -join ' | ')
    Assert-That 'T38' 'audit self-test: a creator that never validates the name is caught' ((Test-CreatorSource ($creatorText.Replace('UnusableEffectiveName(', 'Neutral('))).Count -ge 1)
    Assert-That 'T38' 'audit self-test: a creator that derives a name (Trim) is caught' ((Test-CreatorSource ($creatorText.Replace('IsNullOrWhiteSpace(effectiveName)', 'IsNullOrWhiteSpace(effectiveName.Trim())'))).Count -ge 1)

    Write-Host '== HV-08 continue rule (harness source)'
    $commandText = Get-Content -Raw (Join-Path $harnessDir 'HostValidationCommand.cs')
    Assert-That 'T39' 'the runner records a deviation and does NOT stop on it (no "produced a NEW DEVIATION" stop remains)' ($commandText -match 'doc\.Deviations\.Add\(record\.Id\)' -and $commandText -notmatch 'produced a NEW DEVIATION')
    Assert-That 'T39' 'the only ways to stop are the ruled ones (hv00, filedia, an explicit StopRun, an untrustworthy environment)' ($commandText -match 'StopKind\.Hv00' -and $commandText -match 'StopKind\.Filedia' -and $commandText -match 'record\.StopRun' -and $commandText -match 'EnvironmentTrustworthy')

    Write-Host '== the rollback CONTROLS govern the verdict (Architect MAJOR-1) and the launcher recomputes it'
    $expectedNonPass = [ordered]@{
        'ctl-missing'  = 'UNKNOWN'   # one control record deleted
        'ctl-dup'      = 'UNKNOWN'   # one control record duplicated
        'ctl-kind'     = 'UNKNOWN'   # RB-02c DOCUMENT-AUTHORITY recorded as SIDE-DB
        'ctl-unknown'  = 'UNKNOWN'   # one control UNKNOWN
        'ctl-leak'     = 'UNKNOWN'   # a control leak listed
        'docauth-false' = 'UNKNOWN'  # document authority unavailable
        'hv10-side'    = 'UNKNOWN'   # HV-10 governed by a SIDE-DB run
        'ctl-fail'     = 'FAIL'      # one control FAIL
        'controls-leak' = 'FAIL'
        'tx-disposed-before' = 'FAIL'
        'tx-active-delta' = 'FAIL'
        'tx-active-unreadable' = 'FAIL'
    }
    foreach ($scenario in $expectedNonPass.Keys) {
        $expect = $expectedNonPass[$scenario]
        $p, $r = Fresh ('T40-' + $scenario)
        $x = Invoke-Launcher $p $r @{ I52_OFFLINE_SCENARIO = $scenario }
        $ev = (Read-Text (Join-Path $x.Out 'hostval-evidence.json')) | ConvertFrom-Json
        Assert-That 'T40' "$scenario => harness verdict $expect, never PASS (exit 3, RUN_RESULT = $expect)" ($ev.verdict -eq $expect -and $x.Exit -eq 3 -and $x.Output.Trim().EndsWith("RUN_RESULT = $expect") -and $x.Record.launchValid -eq $true) "verdict=$($ev.verdict) exit=$($x.Exit)"
    }

    $p, $r = Fresh 'T41-control-set'
    $x = Invoke-Launcher $p $r
    $ev = (Read-Text (Join-Path $x.Out 'hostval-evidence.json')) | ConvertFrom-Json
    $keys = @($ev.controls | ForEach-Object { '{0}|{1}' -f $_.id, $_.dbKind })
    Assert-That 'T41' 'a good run records exactly the 14 governing controls (id + dbKind), each PASS, no control leak, document authority available' (@($ev.controls).Count -eq 14 -and @($keys | Sort-Object -Unique).Count -eq 14 -and @($ev.controls | Where-Object { $_.result -ne 'PASS' }).Count -eq 0 -and @($ev.controlLeaks).Count -eq 0 -and $ev.documentAuthority.available -eq $true -and $x.Exit -eq 0 -and $x.Record.checks.evidenceControlSet -eq $true -and $x.Record.checks.evidenceRollbackCasesDocument -eq $true)
    Assert-That 'T41' 'the 14 are RB-01 (side), RB-01V (both), RB-01D (document) and RB-02a/02b/02c/03/05 (both)' ((($keys | Sort-Object) -join ',') -eq (('RB-01|SIDE-DB', 'RB-01V|SIDE-DB', 'RB-01V|DOCUMENT-AUTHORITY', 'RB-01D|DOCUMENT-AUTHORITY', 'RB-02a|SIDE-DB', 'RB-02a|DOCUMENT-AUTHORITY', 'RB-02b|SIDE-DB', 'RB-02b|DOCUMENT-AUTHORITY', 'RB-02c|SIDE-DB', 'RB-02c|DOCUMENT-AUTHORITY', 'RB-03|SIDE-DB', 'RB-03|DOCUMENT-AUTHORITY', 'RB-05|SIDE-DB', 'RB-05|DOCUMENT-AUTHORITY' | Sort-Object) -join ','))

    # A LYING harness: the same broken evidence, verdict rewritten to PASS. The launcher must refuse it on its own recomputation.
    $forgeChecks = [ordered]@{
        'ctl-missing'   = 'evidenceControlSet'
        'ctl-dup'       = 'evidenceControlSet'
        'ctl-kind'      = 'evidenceControlSet'
        'ctl-unknown'   = 'evidenceControlSet'
        'ctl-fail'      = 'evidenceControlSet'
        'ctl-leak'      = 'evidenceControlSet'
        'docauth-false' = 'evidenceControlSet'
        'hv10-side'     = 'evidenceRollbackCasesDocument'
    }
    foreach ($scenario in $forgeChecks.Keys) {
        $check = $forgeChecks[$scenario]
        $p, $r = Fresh ('T42-' + $scenario)
        $x = Invoke-Launcher $p $r @{ I52_OFFLINE_SCENARIO = "$scenario+forge" }
        Assert-That 'T42' "forged PASS over '$scenario' => INVALID, exit 2, $check = false" ($x.Exit -eq 2 -and $x.Record.verdict -eq 'PASS' -and $x.Record.runResult -eq 'INVALID' -and $x.Record.checks.$check -eq $false) "exit=$($x.Exit) $check=$($x.Record.checks.$check)"
    }

    # The tests above must be able to FAIL: with the launcher's control check neutralised, the very same forged evidence gets through.
    # (Sums are rewritten after the deliberate mutation so the package itself stays valid; this is a mutation test, NOT host evidence.)
    $mutations = @(
        @{ Name = 'launcher control-set check removed'; Scenario = 'ctl-missing+forge'; Find = "`$checks['evidenceControlSet'] = `$parseable -and (Test-Check {"; Replace = "`$checks['evidenceControlSet'] = `$true -or (Test-Check {" },
        @{ Name = 'launcher rollback-case label check removed'; Scenario = 'hv10-side+forge'; Find = "`$checks['evidenceRollbackCasesDocument'] = `$parseable -and (Test-Check {"; Replace = "`$checks['evidenceRollbackCasesDocument'] = `$true -or (Test-Check {" }
    )
    foreach ($mutation in $mutations) {
        $p, $r = Fresh ('T43-' + ($mutation.Name -replace '[^a-z]', ''))
        $launcherPath = Join-Path $p 'launcher\run-hostval.ps1'
        $text = Get-Content -Raw -LiteralPath $launcherPath
        $mutated = $text.Replace($mutation.Find, $mutation.Replace)
        Assert-That 'T43' "mutation applies ($($mutation.Name))" ($mutated -ne $text)
        [IO.File]::WriteAllText($launcherPath, $mutated, (New-Object Text.UTF8Encoding($false)))
        Update-PackageSums $p
        $x = Invoke-Launcher $p $r @{ I52_OFFLINE_SCENARIO = $mutation.Scenario }
        Assert-That 'T43' "with the $($mutation.Name), the forged PASS is ACCEPTED (exit 0): T42 would catch it, so the check is load-bearing" ($x.Exit -eq 0 -and $x.Record.runResult -eq 'PASS') "exit=$($x.Exit)"
    }

    Write-Host '== classification: stopKind = deviation + completed = false is still a VALID FAIL (launcher support kept, exercised offline)'
    $p, $r = Fresh 'T44-deviation-stop'
    $x = Invoke-Launcher $p $r @{ I52_OFFLINE_SCENARIO = 'deviation-stop' }
    $ev = (Read-Text (Join-Path $x.Out 'hostval-evidence.json')) | ConvertFrom-Json
    Assert-That 'T44' 'a governed deviation stop (completed = false, stopKind = deviation, FAIL backed by a deviation, a case beyond HV-00 ran) => launch valid, FAIL, exit 3' ($ev.completed -eq $false -and $ev.stopKind -eq 'deviation' -and $ev.verdict -eq 'FAIL' -and $x.Record.launchValid -eq $true -and $x.Record.runResult -eq 'FAIL' -and $x.Exit -eq 3)

    Write-Host '== transaction end rules and active-transaction deltas (harness source)'
    Assert-That 'T45' 'EndCore records the ended transaction identity and asserts the active bookkeeping only for rollback-sensitive cases and controls' ($commandText -match 'outcome\.EndedId' -and $commandText -match 'strictActive: StrictTx && expectRollback' -and $commandText -match 'StrictTx = entry\.RollbackSensitive' -and $commandText -match 'StrictTx = true;')
    Assert-That 'T45' 'the strict flag is always cleared afterwards, so a later non-rollback case (HV-06 OpenCloseTransaction) is never judged by it' ((([regex]::Matches($commandText, 'StrictTx = false;')).Count) -ge 2)

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
