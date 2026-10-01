# CT-21D PHASE 2 RECORD VALIDATOR -- OFFLINE (no process access). REVIEW MATERIAL. NOT RUN ON ANY HOST BY ANYONE YET.
#
# Re-checks a record written by Capture-Phase2.ps1: strict bytes, the .sha256 sidecar, the closed schema, canonical form, the capture
# binding derivation, the run-id / session rule, the fixed check list re-derived from the record's own data, the verdict, the
# INDEPENDENT expected tuple inputs (including the CAD-manager-assigned sessionId), the tool identity (the capture AND this validator and
# the library it loaded) against the hash ledger, and (optional) the declared set on disk now, the module baseline file and the
# composed designation file. Reads files only.
#
# IMPORTANT: the checks are re-derived from the record's OWN data, so a forged but self-consistent record passes them. What catches a
# forgery is the independent material: -ExpectedBuildTupleInputs (from the SIGNED phase 1 record), -HashesLedger (the reviewed tool
# hashes) and the sidecar. They are therefore MANDATORY here (the script refuses to run without them). -DeclaredSetJson is MANDATORY
# too (round 2): it re-lists the declared-set folder NOW, so a designation placed between the capture and this validation is caught
# (R3-2). The -Schema and the expected-inputs schema files are hashed and pinned to the ledger (a validator run against another schema
# is invalid). -ModuleBaselineJson and -DesignationJson are optional but each one adds a check that the record alone cannot give.
# The input paths of the record, the declared set, the baseline and the designation must be absolute local fixed-drive paths without
# reparse points (the same guard as the capture). The designation file, if composed before this validation, must be staged OUTSIDE the
# declared-set folder. The SHA-256 of the ledger is printed (LEDGER_SHA256): compare it with the value reported in the review hand-off.
#
# Exit codes: 0 = valid; 2 = invalid; 3 = valid ONLY as a TEST HOOK record (stand-in process; never a valid attestation); 1 = usage.
#
# Usage:
#   pwsh -NoProfile -File .\Validate-Phase2.ps1 <record.json> -Schema .\schemas\ct21d.phase2.v1.json `
#        -ExpectedBuildTupleInputs <expected-inputs.json> -HashesLedger .\HASHES-PHASE2.txt `
#        -DeclaredSetJson <declared-set.json> [-ModuleBaselineJson <baseline.json>] [-DesignationJson <run-designation.json>]
#
# <expected-inputs.json> follows schemas/ct21d.phase2.expected-inputs.v1.json and is written by the CAD manager from the signed
# PHASE 1 tuple record, independently of the capture (so the record cannot vouch for itself).

#Requires -Version 7.0
[CmdletBinding()]
param(
    [Parameter(Position = 0)][string]$Record,
    [string]$Schema,
    [string]$ExpectedBuildTupleInputs,
    [string]$HashesLedger,
    [string]$DeclaredSetJson,
    [string]$ModuleBaselineJson,
    [string]$DesignationJson
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

function Exit-Phase2ValidateUsage {
    param([string]$Message)
    [Console]::Error.WriteLine('USAGE: ' + $Message)
    exit 1
}

try {
    . (Join-Path -Path $PSScriptRoot -ChildPath 'lib/Phase2Common.ps1')
    if ([string]::IsNullOrEmpty($Record)) { Exit-Phase2ValidateUsage 'record path is missing (first positional argument)' }
    if ([string]::IsNullOrEmpty($Schema)) { Exit-Phase2ValidateUsage '-Schema is missing' }
    if ([string]::IsNullOrEmpty($ExpectedBuildTupleInputs)) { Exit-Phase2ValidateUsage '-ExpectedBuildTupleInputs is missing (mandatory: the record cannot vouch for itself)' }
    if ([string]::IsNullOrEmpty($HashesLedger)) { Exit-Phase2ValidateUsage '-HashesLedger is missing (mandatory: tool identity)' }
    if ([string]::IsNullOrEmpty($DeclaredSetJson)) { Exit-Phase2ValidateUsage '-DeclaredSetJson is missing (mandatory: the declared-set folder is listed again at validation time, R3-2)' }
    foreach ($pair in @(@($Record, 'record'), @($Schema, 'schema'), @($ExpectedBuildTupleInputs, 'expected inputs'), @($HashesLedger, 'hashes ledger'))) {
        if (-not [System.IO.File]::Exists($pair[0])) { Exit-Phase2ValidateUsage ($pair[1] + ' file does not exist') }
    }
    # local fixed-drive, reparse-free, existing files (same guard as the capture); a refusal here is a usage error (exit 1)
    $Record = Assert-Phase2InputFile -Path $Record -What 'record'
    $Schema = Assert-Phase2InputFile -Path $Schema -What '-Schema'
    $ExpectedBuildTupleInputs = Assert-Phase2InputFile -Path $ExpectedBuildTupleInputs -What '-ExpectedBuildTupleInputs'
    $HashesLedger = Assert-Phase2InputFile -Path $HashesLedger -What '-HashesLedger'
    $DeclaredSetJson = Assert-Phase2InputFile -Path $DeclaredSetJson -What '-DeclaredSetJson'
    if ($ModuleBaselineJson) { $ModuleBaselineJson = Assert-Phase2InputFile -Path $ModuleBaselineJson -What '-ModuleBaselineJson' }
    if ($DesignationJson) { $DesignationJson = Assert-Phase2InputFile -Path $DesignationJson -What '-DesignationJson' }

    $strict = New-Object System.Text.UTF8Encoding($false, $true)
    # the schemas are read ONCE as bytes: the bytes that are hashed are the bytes that are parsed
    $schemaBytes = Read-Phase2FileBytes -Path $Schema -MaxBytes 4194304
    $schemaFileSha = Get-Phase2BytesSha256 -Bytes $schemaBytes
    $schemaObj = ConvertFrom-Phase2Json -Text ($strict.GetString($schemaBytes))
    $expectedSchemaPath = Join-Path -Path $PSScriptRoot -ChildPath 'schemas/ct21d.phase2.expected-inputs.v1.json'
    $expectedSchemaBytes = Read-Phase2FileBytes -Path $expectedSchemaPath -MaxBytes 4194304
    $expectedSchemaFileSha = Get-Phase2BytesSha256 -Bytes $expectedSchemaBytes
    $expectedSchema = ConvertFrom-Phase2Json -Text ($strict.GetString($expectedSchemaBytes))
    $expected = ConvertFrom-Phase2Json -Text ([System.IO.File]::ReadAllText($ExpectedBuildTupleInputs, $strict))
    $expErrors = Test-Phase2AgainstSchema -Value $expected -Schema $expectedSchema
    if ($expErrors.Count -gt 0) { Exit-Phase2ValidateUsage ('expected inputs do not match their schema: ' + ($expErrors -join ' ; ')) }

    $bytes = [System.IO.File]::ReadAllBytes($Record)
    $sidecarPath = $Record + '.sha256'
    $sidecarText = $null
    if ([System.IO.File]::Exists($sidecarPath)) { $sidecarText = [System.IO.File]::ReadAllText($sidecarPath, $strict) }
    $validatorSha = Get-Phase2FileSha256 -Path $PSCommandPath
    $libraryFile = Join-Path -Path $PSScriptRoot -ChildPath 'lib/Phase2Common.ps1'
    $librarySha = Get-Phase2FileSha256 -Path $libraryFile
    if ($null -eq $validatorSha -or $null -eq $librarySha) { Exit-Phase2ValidateUsage 'the validator files cannot be hashed (tool identity unavailable)' }
    $res = Test-Phase2RecordBytes -RecordBytes $bytes -Schema $schemaObj -Expected $expected -DeclaredSetJson $DeclaredSetJson -HashesLedgerPath $HashesLedger `
        -CheckSidecar -SidecarText $sidecarText -RecordFileName ([System.IO.Path]::GetFileName($Record)) `
        -ValidatorScriptSha256 $validatorSha -ValidatorLibrarySha256 $librarySha -DesignationJson $DesignationJson -ModuleBaselineJson $ModuleBaselineJson `
        -SchemaSha256 $schemaFileSha -ExpectedInputsSchemaSha256 $expectedSchemaFileSha
    Write-Output ('LEDGER_SHA256 ' + (Get-Phase2FileSha256 -Path $HashesLedger))
    foreach ($l in $res.Lines) { Write-Output $l }
    if ($res.Valid -and $res.StandIn) { Write-Output 'RESULT VALID_TEST_STANDIN_ONLY (a test hook record is never a valid attestation)'; exit 3 }
    if ($res.Valid) { Write-Output 'RESULT VALID'; exit 0 }
    Write-Output 'RESULT INVALID'
    exit 2
} catch {
    [Console]::Error.WriteLine('USAGE: ' + $_.Exception.Message)
    exit 1
}
