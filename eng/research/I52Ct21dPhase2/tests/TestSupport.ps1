# Test support for the phase 2 scripts: a tiny assertion harness (no Pester dependency), temp folders, a declared-set fixture and
# fake providers. The tests run WITHOUT AutoCAD. This file may start a benign stand-in child process and create/remove files under a
# temp folder of its own; the production scripts (scanned by StaticScan.ps1) do none of that.

Set-StrictMode -Version Latest

$script:ToolRoot = Split-Path -Parent $PSScriptRoot
. (Join-Path -Path $script:ToolRoot -ChildPath 'lib/Phase2Common.ps1')

$script:TestState = @{ Pass = 0; Fail = 0; Failures = (New-Object 'System.Collections.Generic.List[string]'); Section = '' }

function Start-TestSection {
    param([string]$Name)
    $script:TestState.Section = $Name
    Write-Host ('--- ' + $Name)
}

function Assert-True {
    param([string]$Name, $Condition, [string]$Info = '')
    if ($Condition -eq $true) { $script:TestState.Pass++ }
    else {
        $script:TestState.Fail++
        $script:TestState.Failures.Add($script:TestState.Section + ' :: ' + $Name + $(if ($Info) { ' :: ' + $Info } else { '' }))
        Write-Host ('FAIL ' + $Name + $(if ($Info) { ' :: ' + $Info } else { '' }))
    }
}

function Assert-Equal {
    param([string]$Name, $Expected, $Actual)
    Assert-True -Name $Name -Condition ($Expected -ceq $Actual) -Info ('expected [' + $Expected + '] actual [' + $Actual + ']')
}

# Expects a REFUSED exception whose message contains $Like.
function Assert-Refused {
    param([string]$Name, [scriptblock]$Block, [string]$Like)
    $msg = $null
    try { & $Block | Out-Null } catch { $msg = $_.Exception.Message }
    Assert-True -Name $Name -Condition ($null -ne $msg -and $msg.StartsWith('REFUSED:') -and $msg.Contains($Like)) -Info ('message [' + $msg + ']')
}

$script:TmpRoot = Join-Path -Path ([System.IO.Path]::GetTempPath()) -ChildPath ('ct21d-phase2-tests-' + [guid]::NewGuid().ToString('N').Substring(0, 12))
[void][System.IO.Directory]::CreateDirectory($script:TmpRoot)
$script:CaseCounter = 0

function New-CaseDir {
    $script:CaseCounter++
    $d = Join-Path -Path $script:TmpRoot -ChildPath ('case' + $script:CaseCounter)
    [void][System.IO.Directory]::CreateDirectory($d)
    return $d
}

function Remove-TestTree {
    # junctions are removed as links first, so a recursive delete never follows them
    foreach ($d in (Get-ChildItem -LiteralPath $script:TmpRoot -Recurse -Force -Directory -ErrorAction SilentlyContinue)) {
        if (($d.Attributes -band [System.IO.FileAttributes]::ReparsePoint) -ne 0) { [System.IO.Directory]::Delete($d.FullName) }
    }
    Remove-Item -LiteralPath $script:TmpRoot -Recurse -Force -ErrorAction SilentlyContinue
}

function Write-TestFile {
    param([string]$Path, [byte[]]$Bytes)
    [System.IO.File]::WriteAllBytes($Path, $Bytes)
}

function Get-TestUtf8 { param([string]$Text) return , ((New-Object System.Text.UTF8Encoding($false)).GetBytes($Text)) }

# A declared set like the real one: four DLL files, four .pin files (lowercase sha256 + LF), declared-set.json.
function New-TestDeclaredSet {
    param([string]$Dir)
    $folder = Join-Path -Path $Dir -ChildPath 'declared-set'
    [void][System.IO.Directory]::CreateDirectory($folder)
    $entries = @()
    foreach ($n in 'Fake.Core.dll', 'Fake.R0.dll', 'Fake.Rs.Core.dll', 'Fake.Rs.dll') {
        $bytes = Get-TestUtf8 ('dummy bytes of ' + $n)
        $path = Join-Path -Path $folder -ChildPath $n
        Write-TestFile -Path $path -Bytes $bytes
        $sha = Get-Phase2BytesSha256 -Bytes $bytes
        Write-TestFile -Path ($path + '.pin') -Bytes (Get-TestUtf8 ($sha + "`n"))
        $entries += , ([ordered]@{ path = $path; sha256 = $sha })
    }
    $json = Join-Path -Path $Dir -ChildPath 'declared-set.json'
    Write-TestFile -Path $json -Bytes (Get-TestUtf8 (ConvertTo-Phase2CanonicalJson -Value $entries))
    return [pscustomobject]@{ Folder = $folder; Json = $json; Sha256 = (Get-Phase2FileSha256 -Path $json); Entries = $entries }
}

# Fake providers. $St is a hashtable the test mutates before the capture.
function New-FakeState {
    param([string]$Dir, [string]$ProcessName = 'acad')
    $modDir = Join-Path -Path $Dir -ChildPath 'mods'
    [void][System.IO.Directory]::CreateDirectory($modDir)
    $acad = Join-Path -Path $modDir -ChildPath 'acad.exe'
    Write-TestFile -Path $acad -Bytes (Get-TestUtf8 'fake acad main module')
    $mods = @()
    foreach ($n in 'ntdll.dll', 'Kernel32.DLL', 'acdb25.dll', 'AcMgd.dll') {
        $p = Join-Path -Path $modDir -ChildPath $n
        Write-TestFile -Path $p -Bytes (Get-TestUtf8 ('fake module ' + $n))
        $mods += , ([pscustomobject]@{ Path = $p; FileVersion = '1.0.0.0' })
    }
    $mods += , ([pscustomobject]@{ Path = $acad; FileVersion = '25.0.171.0.0' })
    $me = [System.Security.Principal.WindowsIdentity]::GetCurrent().Name
    return @{
        Now      = '2026-10-01T15:00:00.000Z'
        User     = $me
        Process  = [pscustomobject]@{ Pid = 4242; Name = $ProcessName; StartUtc = '2026-10-01T14:59:00.123Z'; MainStatus = 'OBSERVED'; MainPath = $acad; FileVersion = '25.0.171.0.0'; ProductVersion = '25.0.171.0.0' }
        Modules  = [pscustomobject]@{ Ok = $true; Items = $mods }
        Processes = [pscustomobject]@{ Ok = $true; Items = @(
                [pscustomobject]@{ Pid = 4; Name = 'System'; StartUtc = $null },
                [pscustomobject]@{ Pid = 4242; Name = $ProcessName; StartUtc = '2026-10-01T14:59:00.123Z' },
                [pscustomobject]@{ Pid = 900; Name = 'explorer'; StartUtc = '2026-10-01T10:00:00.000Z' }) }
        Command  = [pscustomobject]@{ Status = 'OBSERVED'; Value = ('"' + $acad + '" /nologo') }
        Owner    = [pscustomobject]@{ Status = 'OBSERVED'; Value = $me }
        Registry = [pscustomobject]@{ Status = 'OBSERVED'; Value = '<<Unnamed Profile>>' }
        Signer   = [pscustomobject]@{ Status = 'Valid'; Subject = 'CN=Fake' }
        ProfileVars = @{
            SECURELOAD   = [pscustomobject]@{ Status = 'OBSERVED'; Kind = 'String'; Value = '1' }
            TRUSTEDPATHS = [pscustomobject]@{ Status = 'OBSERVED'; Kind = 'String'; Value = 'FAKE-ONE;FAKE-TWO' }
        }
        AcadSha  = (Get-Phase2FileSha256 -Path $acad)
        ProfileVarCalls = (New-Object 'System.Collections.Generic.List[string]')
    }
}

function New-FakeProviders {
    param([hashtable]$St)
    $p = @{}
    $p.UtcNow = { $St.Now }.GetNewClosure()
    $p.InvokingUser = { $St.User }.GetNewClosure()
    $p.Process = { param($i) $St.Process }.GetNewClosure()
    $p.Modules = { param($i) $St.Modules }.GetNewClosure()
    $p.Processes = { $St.Processes }.GetNewClosure()
    $p.CommandLine = { param($i) $St.Command }.GetNewClosure()
    $p.Owner = { param($i) $St.Owner }.GetNewClosure()
    $p.RegistryDefault = { param($k) $St.Registry }.GetNewClosure()
    $p.Signer = { param($x) $St.Signer }.GetNewClosure()
    $p.ProfileVariable = { param($k, $n) $St.ProfileVarCalls.Add($k + '|' + $n); $St.ProfileVars[$n] }.GetNewClosure()
    return $p
}

function New-TestParams {
    param([string]$Dir, $Ds, [hashtable]$St, [string]$Session = 'S1-A')
    $run = 'HGP-H' + $script:Phase2SessionActivity[$Session] + '-20261001T150000Z-01'
    $root = Join-Path -Path $Dir -ChildPath $run
    [void][System.IO.Directory]::CreateDirectory($root)
    return @{
        ProcessId = 4242; Session = $Session; SessionId = 'SES-S1-20261001-0001'; RunId = $run; EvidenceRoot = $root; DeclaredSetJson = $Ds.Json
        TranscriptPath = $null; RequireTranscript = $false
        ModuleBaselineJson = $null; ExpectedModuleBaselineSha256 = $null; RequireModuleBaseline = $false
        ExpectedDeclaredSetSha256 = $Ds.Sha256; ExpectedAcadSha256 = $St.AcadSha; ExpectedProfile = '<<Unnamed Profile>>'
        ProfilesSubKey = $script:Phase2DefaultProfilesSubKey; ExpectedProcessName = 'acad'; TestHookName = ''
        ScriptSha256 = ('1' * 64); LibrarySha256 = ('2' * 64); SchemaSha256 = ('3' * 64); PowerShellVersion = '7.test'
    }
}

# Runs a fake capture; $Mutate receives ($St, $Params, $Ds, $Dir) before the capture. Returns the record.
function Invoke-FakeCapture {
    param([scriptblock]$Mutate = $null, [string]$Session = 'S1-A')
    $dir = New-CaseDir
    $ds = New-TestDeclaredSet -Dir $dir
    $st = New-FakeState -Dir $dir
    $params = New-TestParams -Dir $dir -Ds $ds -St $st -Session $Session
    if ($null -ne $Mutate) { & $Mutate $st $params $ds $dir }
    return (Invoke-Phase2Capture -Params $params -P (New-FakeProviders -St $st))
}

function Get-CheckResult {
    param($Record, [string]$Name)
    foreach ($c in $Record['validation']['checks']) { if ($c['name'] -eq $Name) { return [string]$c['result'] } }
    return 'MISSING'
}

function ConvertTo-RecordBytes { param($Record) return , (ConvertTo-Phase2Utf8Bytes -Text (ConvertTo-Phase2CanonicalJson -Value $Record)) }

# A module baseline file equal to the given module items (shared by the functional and the remediation tests).
function New-BaselineFile {
    param([string]$Dir, $Items, [string]$Name = 'baseline.json')
    $entries = @()
    foreach ($m in $Items) { $entries += , ([ordered]@{ path = $m.Path; sha256 = (Get-Phase2FileSha256 -Path $m.Path) }) }
    $path = Join-Path $Dir $Name
    Write-TestFile -Path $path -Bytes (Get-TestUtf8 (ConvertTo-Phase2CanonicalJson -Value $entries))
    return $path
}

# A capture mutation that supplies a baseline equal to the loaded modules (required for every session except S1-A, round 2).
$script:WithBaseline = { param($st, $p, $ds, $dir) $b = New-BaselineFile $dir $st.Modules.Items; $p.ModuleBaselineJson = $b; $p.ExpectedModuleBaselineSha256 = (Get-Phase2FileSha256 -Path $b) }

# The ledger lines the validator pins with the schema hashes (round 2): script, library, schema and expected-inputs schema, plus the validator.
function New-TestLedgerText {
    param([string]$Capture = ('1' * 64), [string]$Library = ('2' * 64), [string]$Schema = ('3' * 64), [string]$ExpectedSchema = ('5' * 64), [string]$Validator = ('4' * 64))
    return ($Capture + "  Capture-Phase2.ps1`n" + $Library + "  lib/Phase2Common.ps1`n" + $Schema + "  schemas/ct21d.phase2.v1.json`n" + $ExpectedSchema + "  schemas/ct21d.phase2.expected-inputs.v1.json`n" + $Validator + "  Validate-Phase2.ps1`n")
}

# Corpus of transcript lines for the load-command detection (round 2, review 1 M2). Shared by the functional tests (Get-Phase2LoadLines)
# and the static scan tests (rule NETLOAD_OUTSIDE_TRANSCRIPT_READER): the two regular expressions must agree on the SHARED lines.
$script:LoadLinesBoth = @(
    'Command: NETLOAD', 'Command: _NETLOAD', '_netload', '._netload', '^C^C_netload', '(command "_netload" "C:\x\Evil.dll")', '(command "netload" "x.dll")',
    '_APPLOAD', 'Command: _appload', '_ARXLOAD', '_ARXUNLOAD', '_DBXLOAD', '_VLLOAD', '_FASLOAD', '_VLRUN', '_VBALOAD', '_-VBARUN', '._vbarun', '-NETLOAD', '_.netload',
    '(load "x.lsp")', ' ( LOAD "a")', '(arxload "x.arx")', '(autoload "x" (list "c:y"))', '(vl-load-all "x.lsp")', '(vl-vbaload "a.dvb")', '(vl-vbarun "m")', '(startapp "calc.exe")',
    '_SCRIPT', '_RUNSCRIPT', '._rscript', '^C^C_script start.scr', '(command "_.script" "s.scr")'
)
# Lines only the transcript reader detects (a bare SCRIPT command echo cannot be a rule over production source: "$script:").
$script:LoadLinesTranscriptOnly = @('Command: SCRIPT', 'SCRIPT', '(command "script" "x.scr")', 'Command: runscript')
$script:LoadLinesClean = @(
    'Enter assembly file name: C:\x\Evil.dll', 'overload', 'unloaded', 'x NETLOADED', 'NOTNETLOAD x', '(loader x)', 'Command: LINE', 'postscript', 'javascript',
    'Enter script file name:', 'a transcript of the session', 'Command: ', 'loaded 4 modules', 'description-of-the-script', '(setq load-count 3)'
)
