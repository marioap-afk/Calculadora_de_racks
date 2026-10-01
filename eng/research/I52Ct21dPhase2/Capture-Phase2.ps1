# CT-21D PHASE 2 SESSION-BINDING CAPTURE -- REVIEW MATERIAL FOR THE CAD MANAGER. NOT RUN ON ANY HOST BY ANYONE YET.
#
# Authority: I-52 decisions sections 237-248 (section 248: authorization to DRAFT these scripts for review, without running them);
# package v3 sections 3.5.3 (tuple record phase 2), 3.7 (order of events) and 5 (stop conditions); ruling (e) (attestations).
# Governing: FALSE. Phase: 2. This script is NOT an instrument and loads nothing into AutoCAD.
#
# What it does (READ ONLY on the system):
#   * observes the ALREADY STARTED AutoCAD process given by -ProcessId: pid, start time (UTC), process name (must be acad), the build
#     (file version, path and SHA-256 of the main module), the effective profile (command line /p, else the registry default value of
#     the profile key of the invoking user), the loaded-module inventory (path, SHA-256, version; the Authenticode status goes to volatile),
#     the process list, and (informational) the AT-REST SECURELOAD / TRUSTEDPATHS values of the effective profile in the registry;
#   * compares the loaded modules with the phase 1 module baseline when -ModuleBaselineJson is given;
#   * verifies the declared set: declared-set.json hash against -ExpectedDeclaredSetSha256, every DLL against its entry and its .pin;
#   * hashes a transcript file the CAD manager provides (it never runs NETLOAD) and extracts its load-command lines verbatim;
#   * writes a record and its hash sidecar as NEW flat files inside -EvidenceRoot (create-new; refuses to overwrite).
#
# What it never does: start a process, start or script AutoCAD, run NETLOAD, write the registry, change an ACL or an attribute,
# touch TRUSTEDPATHS / SECURELOAD, or write any file outside the evidence root.
#
# The verdict PHASE2_EXTERNAL_OBSERVATIONS_VALID covers ONLY the external observations of this tool (see the coverage section of the
# record): it is not the template 3.5.3 validation (CPROFILE / SECURELOAD / TRUSTEDPATHS in the process, runtimeIdentities and the
# CAD manager's human fields stay HOST-TO-CONFIRM).
#
# Exit codes: 0 = record written, verdict PHASE2_EXTERNAL_OBSERVATIONS_VALID; 2 = record written, verdict PHASE2_INVALID;
#             3 = record written with the TEST HOOK active (stand-in process, tests only; never a valid attestation);
#             4 = PARTIAL WRITE: the record was written but its .sha256 sidecar could not be (nothing is deleted; the run is failed and the
#                 evidence root is NOT reusable; see README section 6);
#             1 = usage or refusal (before the record was written: nothing written).
#
# Usage (reviewed and typed by the CAD manager, in PowerShell 7, AFTER acad.exe has started and BEFORE the designation is placed):
#   pwsh -NoProfile -File .\Capture-Phase2.ps1 -ProcessId 1234 -Session S1-A -SessionId <assigned in phase 1> `
#        -RunId HGP-H1-20261001T150000Z-01 `
#        -EvidenceRoot 'D:\I52-CT21D-HOST\evidence\HGP-H1-20261001T150000Z-01' -AllowedEvidenceParent 'D:\I52-CT21D-HOST\evidence' `
#        -DeclaredSetJson 'D:\I52-CT21D-HOST\canonical-642ba392\declared-set.json' `
#        -ExpectedDeclaredSetSha256 <64 hex from the tuple record> -ExpectedAcadSha256 <64 hex from the tuple record> `
#        -ExpectedProfile '<<Unnamed Profile>>' [-TranscriptPath <file>] [-RequireTranscript] `
#        [-ModuleBaselineJson <file> -ExpectedModuleBaselineSha256 <64 hex from the tuple record>] [-RequireModuleBaseline]
#
# Test hook: the environment variable CT21D_PHASE2_TEST_STANDIN names a stand-in process (tests only). Without it the process name
# must be acad. A record made with the hook can never be PHASE2_EXTERNAL_OBSERVATIONS_VALID and the validator refuses it unless the
# same variable is set.

#Requires -Version 7.0
[CmdletBinding()]
param(
    [string]$ProcessId,
    [string]$EvidenceRoot,
    [string]$AllowedEvidenceParent,
    [string]$RunId,
    [string]$Session,
    [string]$SessionId,
    [string]$DeclaredSetJson,
    [string]$TranscriptPath,
    [string]$ExpectedDeclaredSetSha256,
    [string]$ExpectedAcadSha256,
    [string]$ExpectedProfile,
    [string]$ModuleBaselineJson,
    [string]$ExpectedModuleBaselineSha256,
    [switch]$RequireTranscript,
    [switch]$RequireModuleBaseline,
    [string]$ProfilesSubKey = 'Software\Autodesk\AutoCAD\R25.0\ACAD-8101:409\Profiles'
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

function Exit-Phase2Usage {
    param([string]$Message)
    [Console]::Error.WriteLine('USAGE/REFUSED: ' + $Message)
    exit 1
}

try {
    . (Join-Path -Path $PSScriptRoot -ChildPath 'lib/Phase2Common.ps1')

    # ---- fail-closed parameter validation (no prompts: every parameter is checked here) --------------------------------------
    if ([string]::IsNullOrEmpty($ProcessId) -or $ProcessId -cnotmatch '^[1-9][0-9]{0,9}$') { Exit-Phase2Usage 'ProcessId is missing or not a positive integer' }
    if ([string]::IsNullOrEmpty($RunId) -or $RunId -cnotmatch $script:Phase2RunIdPattern) { Exit-Phase2Usage 'RunId is missing or not HGP-H<digits>-<yyyymmddThhmmssZ>-<nn>' }
    if ([string]::IsNullOrEmpty($Session) -or (@($script:Phase2SessionActivity.Keys) -cnotcontains $Session)) { Exit-Phase2Usage 'Session is missing or not one of S1-A S1-B S1-C S2 S3 S4' }
    if ([string]::IsNullOrEmpty($SessionId) -or $SessionId -cnotmatch $script:Phase2AssignedSessionIdPattern) { Exit-Phase2Usage 'SessionId is missing or is not a well formed identifier (the one the CAD manager assigned in phase 1)' }
    if ($RunId -cnotmatch ('^HGP-H' + $script:Phase2SessionActivity[$Session] + '-')) { Exit-Phase2Usage 'RunId activity number does not match the session (decisions section 247)' }
    if ([string]::IsNullOrEmpty($EvidenceRoot)) { Exit-Phase2Usage 'EvidenceRoot is missing' }
    if ([string]::IsNullOrEmpty($DeclaredSetJson)) { Exit-Phase2Usage 'DeclaredSetJson is missing' }
    if ($ExpectedDeclaredSetSha256 -cnotmatch '^[0-9a-f]{64}$') { Exit-Phase2Usage 'ExpectedDeclaredSetSha256 is missing or not 64 lowercase hex' }
    if ($ExpectedAcadSha256 -cnotmatch '^[0-9a-f]{64}$') { Exit-Phase2Usage 'ExpectedAcadSha256 is missing or not 64 lowercase hex' }
    if ([string]::IsNullOrEmpty($ExpectedProfile)) { Exit-Phase2Usage 'ExpectedProfile is missing' }
    if ($ProfilesSubKey -cnotmatch '^Software\\[A-Za-z0-9 _.:<>\\-]+$') { Exit-Phase2Usage 'ProfilesSubKey has an unexpected shape' }
    if (-not [string]::IsNullOrEmpty($ModuleBaselineJson) -and $ExpectedModuleBaselineSha256 -cnotmatch '^[0-9a-f]{64}$') { Exit-Phase2Usage 'ExpectedModuleBaselineSha256 (64 lowercase hex, from the signed phase 1 record) is required with ModuleBaselineJson' }
    if ([string]::IsNullOrEmpty($ModuleBaselineJson) -and -not [string]::IsNullOrEmpty($ExpectedModuleBaselineSha256)) { Exit-Phase2Usage 'ExpectedModuleBaselineSha256 was given without ModuleBaselineJson' }
    if ([string]::IsNullOrEmpty($AllowedEvidenceParent)) { Exit-Phase2Usage '-AllowedEvidenceParent is missing (mandatory: the parent folder of the evidence root, given by the CAD manager)' }

    $expectedName = 'acad'
    $hookName = [System.Environment]::GetEnvironmentVariable('CT21D_PHASE2_TEST_STANDIN')
    if (-not [string]::IsNullOrEmpty($hookName)) {
        if ($hookName -cnotmatch '^[A-Za-z0-9_.-]{1,64}$') { Exit-Phase2Usage 'CT21D_PHASE2_TEST_STANDIN has an unexpected shape' }
        $expectedName = $hookName
        [Console]::Error.WriteLine('WARNING: TEST HOOK ACTIVE (CT21D_PHASE2_TEST_STANDIN). This record cannot be a valid attestation (exit 3).')
    }

    $root = Assert-Phase2EvidenceRoot -Path $EvidenceRoot -RunId $RunId
    [void](Assert-Phase2AllowedEvidenceParent -RootFullPath $root -ParentPath $AllowedEvidenceParent)
    $declaredFull = Assert-Phase2InputFile -Path $DeclaredSetJson -What 'DeclaredSetJson'
    $transcriptFull = $null
    if (-not [string]::IsNullOrEmpty($TranscriptPath)) { $transcriptFull = Assert-Phase2InputFile -Path $TranscriptPath -What 'TranscriptPath' }
    $baselineFull = $null
    if (-not [string]::IsNullOrEmpty($ModuleBaselineJson)) { $baselineFull = Assert-Phase2InputFile -Path $ModuleBaselineJson -What 'ModuleBaselineJson' }

    $recordName = 'phase2-record-' + $RunId + '.json'
    $sidecarName = $recordName + '.sha256'
    foreach ($n in @($recordName, $sidecarName)) {
        $t = [System.IO.Path]::Combine($root, $n)
        if ([System.IO.File]::Exists($t) -or [System.IO.Directory]::Exists($t)) { Exit-Phase2Usage ('output already exists, refusing to overwrite: ' + $n) }
    }

    # a fresh EMPTY child root per run (package v3 3.7 step 1 / 3.8): checked after the output names (that refusal has its own message)
    Assert-Phase2EvidenceRootEmpty -FullPath $root

    $schemaPath = Join-Path -Path $PSScriptRoot -ChildPath 'schemas/ct21d.phase2.v1.json'
    $libPath = Join-Path -Path $PSScriptRoot -ChildPath 'lib/Phase2Common.ps1'
    $scriptSha = Get-Phase2FileSha256 -Path $PSCommandPath
    $libSha = Get-Phase2FileSha256 -Path $libPath
    $schemaSha = Get-Phase2FileSha256 -Path $schemaPath
    if ($null -eq $scriptSha -or $null -eq $libSha -or $null -eq $schemaSha) { Exit-Phase2Usage 'the tool files cannot be hashed (tool identity unavailable)' }

    $params = @{
        ProcessId = [int]$ProcessId; Session = $Session; SessionId = $SessionId; RunId = $RunId; EvidenceRoot = $root; DeclaredSetJson = $declaredFull
        TranscriptPath = $transcriptFull; RequireTranscript = [bool]$RequireTranscript
        ModuleBaselineJson = $baselineFull; ExpectedModuleBaselineSha256 = $ExpectedModuleBaselineSha256; RequireModuleBaseline = [bool]$RequireModuleBaseline
        ExpectedDeclaredSetSha256 = $ExpectedDeclaredSetSha256; ExpectedAcadSha256 = $ExpectedAcadSha256; ExpectedProfile = $ExpectedProfile
        ProfilesSubKey = $ProfilesSubKey; ExpectedProcessName = $expectedName; TestHookName = $hookName
        ScriptSha256 = $scriptSha; LibrarySha256 = $libSha; SchemaSha256 = $schemaSha; PowerShellVersion = $PSVersionTable.PSVersion.ToString()
    }
    $record = Invoke-Phase2Capture -Params $params -P (New-Phase2RealProviders)

    # The record must satisfy its own schema before anything is written (a failure here is a tool defect: nothing is written).
    $schema = ConvertFrom-Phase2Json -Text ([System.IO.File]::ReadAllText($schemaPath, (New-Object System.Text.UTF8Encoding($false, $true))))
    $errs = Test-Phase2AgainstSchema -Value $record -Schema $schema
    if ($errs.Count -gt 0) { Exit-Phase2Usage ('internal: the record does not match its schema: ' + ($errs -join ' ; ')) }

    # The tool files are hashed again just before writing: a file changed while the capture ran (the hashes recorded would not be the
    # ones of the bytes that executed) makes the capture refuse. This detects a change during the run; it cannot prove that the text
    # loaded at the start equals the bytes hashed at the start (documented residual).
    $scriptSha2 = Get-Phase2FileSha256 -Path $PSCommandPath
    $libSha2 = Get-Phase2FileSha256 -Path $libPath
    $schemaSha2 = Get-Phase2FileSha256 -Path $schemaPath
    if ($scriptSha2 -cne $scriptSha -or $libSha2 -cne $libSha -or $schemaSha2 -cne $schemaSha) { Exit-Phase2Usage 'a tool file changed while the capture was running (tool identity not stable); nothing written' }

    $bytes = ConvertTo-Phase2Utf8Bytes -Text (ConvertTo-Phase2CanonicalJson -Value $record)
    $written = Write-Phase2RecordAndSidecar -RootFullPath $root -RunId $RunId -RecordBytes $bytes
    $recordSha = $written.RecordSha

    Write-Output ('RECORD ' + [System.IO.Path]::Combine($root, $written.RecordName))
    Write-Output ('SHA256 ' + $recordSha)
    if ($written.Partial) {
        [Console]::Error.WriteLine('PARTIAL WRITE: the record was written but its sidecar was not (' + $written.Error + '). Nothing was deleted. This run FAILED: the evidence root is not reusable; keep it as evidence and start a new run id (README section 6).')
        exit 4
    }
    Write-Output ('SESSION_ID_ASSIGNED ' + $record['tupleInputs']['sessionId'])
    Write-Output ('CAPTURE_BINDING_ID ' + $record['captureBinding']['captureBindingId'])
    foreach ($c in $record['validation']['checks']) { Write-Output ('CHECK ' + $c['name'] + ' ' + $c['result'] + ' ' + $c['detail']) }
    Write-Output ('COVERAGE module baseline: ' + $record['coverage']['moduleBaselineComparison'] + '; sessionProfileFacts and runtimeIdentities: HOST-TO-CONFIRM (not covered by this verdict)')
    Write-Output ('VERDICT ' + $record['validation']['verdict'])
    if ($record['validation']['verdict'] -eq $script:Phase2VerdictInvalid) { exit 2 }
    if ($record['validation']['verdict'] -eq $script:Phase2VerdictStandIn) { exit 3 }
    exit 0
} catch {
    [Console]::Error.WriteLine($_.Exception.Message)
    exit 1
}
