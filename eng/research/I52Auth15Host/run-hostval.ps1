#Requires -Version 7.0
[CmdletBinding()]
param(
    # The versioned package folder produced by build-hostval.ps1 (contains run\, out\, SHA256SUMS, TRANSFER-METADATA.json).
    [Parameter(Mandatory = $true)][string]$Package,
    # A fresh, blank .dwg. It is COPIED into out\ and that copy is opened: the scratch document only anchors
    # HostApplicationServices.WorkingDatabase and is never written.
    [Parameter(Mandatory = $true)][string]$ScratchDrawing,
    # The Owner's explicit statement that nobody will touch the computer from launch until the process exits.
    [switch]$OwnerConfirmsNoTouch,
    [string]$Acad = 'C:\Program Files\Autodesk\AutoCAD 2025\acad.exe',
    [string]$Profile = '',
    [int]$TimeoutSeconds = 900
)

# I-52-AUTH15 host validation launcher. ONE AutoCAD process, ONE run, no retry. It does not touch SECURELOAD,
# TRUSTEDPATHS or any other security setting: the Owner trusts the run folder beforehand. It does not patch anything.
$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

if (-not $OwnerConfirmsNoTouch) {
    throw 'STOP: the run is only valid if nobody touches the computer from launch until the process exits. Re-run with -OwnerConfirmsNoTouch once the Owner has confirmed that.'
}

$root = (Resolve-Path -LiteralPath $Package).Path
$run = Join-Path $root 'run'
$out = Join-Path $root 'out'
$sumsPath = Join-Path $root 'SHA256SUMS'
$metaPath = Join-Path $root 'TRANSFER-METADATA.json'
$template = Join-Path $root 'launcher\run.scr'
foreach ($required in $run, $sumsPath, $metaPath, $template) { if (-not (Test-Path -LiteralPath $required)) { throw "Package incomplete, missing: $required" } }
if (-not (Test-Path -LiteralPath $Acad)) { throw "acad.exe not found: $Acad" }
if (-not (Test-Path -LiteralPath $ScratchDrawing)) { throw "Scratch drawing not found: $ScratchDrawing" }
if ([IO.Path]::GetExtension($ScratchDrawing).ToLowerInvariant() -ne '.dwg') { throw 'The scratch drawing must be a .dwg' }

function Sha256([string]$path) { (Get-FileHash -Algorithm SHA256 -LiteralPath $path).Hash }

# StrictMode-safe navigation: a missing property is $null, never an error (the evidence of a stopped run is partial).
function Prop($object, [string[]]$names) {
    foreach ($name in $names) {
        if ($null -eq $object) { return $null }
        $property = $object.PSObject.Properties[$name]
        if ($null -eq $property) { return $null }
        $object = $property.Value
    }
    return $object
}

# 1. The package is exactly what was built: every SHA256SUMS entry matches and run\ holds nothing else.
$expected = [ordered]@{}
foreach ($line in Get-Content -LiteralPath $sumsPath) {
    $at = $line.IndexOf('  ')
    if ($at -gt 0) { $expected[$line.Substring($at + 2).Trim()] = $line.Substring(0, $at).Trim() }
}
$drift = @()
foreach ($entry in $expected.Keys) {
    $file = Join-Path $root ($entry.Replace('/', '\'))
    if (-not (Test-Path -LiteralPath $file) -or (Sha256 $file) -ne $expected[$entry]) { $drift += $entry }
}
$runFiles = @(Get-ChildItem -LiteralPath $run -Recurse -File | ForEach-Object { 'run/' + $_.FullName.Substring($run.Length + 1).Replace('\', '/') })
$extra = @($runFiles | Where-Object { -not $expected.Contains($_) })
if ($drift.Count -gt 0) { throw "STOP: package files differ from SHA256SUMS: $($drift -join ', ')" }
if ($extra.Count -gt 0) { throw "STOP: run\ holds files that are not in SHA256SUMS: $($extra -join ', ')" }

$meta = Get-Content -Raw -LiteralPath $metaPath | ConvertFrom-Json
if ($meta.treesEqual -ne $true -or -not $meta.implementationSha) { throw 'STOP: TRANSFER-METADATA.json does not declare treesEqual=true for an implementation SHA.' }

# 2. Environment prerequisites (read-only checks; nothing is changed).
if (Get-Process acad -ErrorAction SilentlyContinue) { throw 'STOP: acad.exe is already running. The run needs its own dedicated, untouched process.' }
$bundles = @(
    Join-Path $env:APPDATA 'Autodesk\ApplicationPlugins'
    Join-Path $env:ProgramData 'Autodesk\ApplicationPlugins'
) | Where-Object { Test-Path -LiteralPath $_ } | ForEach-Object { Get-ChildItem -LiteralPath $_ -Directory -Filter '*RackCad*' -ErrorAction SilentlyContinue }
if (@($bundles).Count -gt 0) { throw "STOP: ENVIRONMENT_PREREQUISITE = RACKCAD_AUTOLOAD_PRESENT ($(@($bundles).FullName -join '; ')). An autoloaded RackCad would load a second Plugin." }
if (Test-Path -LiteralPath (Join-Path $out 'hostval-evidence.json')) { throw 'STOP: out\ already holds evidence: a run is never repeated or overwritten without authorization.' }

New-Item -ItemType Directory -Force $out | Out-Null
$scratch = Join-Path $out 'scratch.dwg'
if (Test-Path -LiteralPath $scratch) { throw 'STOP: out\scratch.dwg already exists.' }
Copy-Item -LiteralPath $ScratchDrawing $scratch
$scratchBefore = Sha256 $scratch

# 3. DRIVER script from the template (the run folder is the only variable).
$script = Join-Path $out 'run.scr'
$text = (Get-Content -Raw -LiteralPath $template).Replace('{RUN}', $run).Replace("`r`n", "`n")
[IO.File]::WriteAllText($script, $text, (New-Object Text.UTF8Encoding($false)))

$arguments = "`"$scratch`" /nologo /nossm" + $(if ($Profile) { " /p `"$Profile`"" } else { '' }) + " /b `"$script`""
$psi = New-Object Diagnostics.ProcessStartInfo($Acad, $arguments)
$psi.UseShellExecute = $false
$psi.WorkingDirectory = $out
$psi.Environment['I52_AUTH15_HV_OUT'] = $out

$startedUtc = (Get-Date).ToUniversalTime()
$process = [Diagnostics.Process]::Start($psi)
$pidLaunched = $process.Id
"LAUNCHED pid=$pidLaunched at $($startedUtc.ToString('o')); waiting up to $TimeoutSeconds s. Do not touch the computer."

$timedOut = -not $process.WaitForExit($TimeoutSeconds * 1000)
if ($timedOut) {
    & taskkill /PID $pidLaunched /T /F | Out-Null
    $process.WaitForExit(30000) | Out-Null
}
$exitCode = if ($process.HasExited) { $process.ExitCode } else { $null }
$endedUtc = (Get-Date).ToUniversalTime()
$stillPresent = [bool](Get-Process -Id $pidLaunched -ErrorAction SilentlyContinue)
$others = @(Get-Process acad -ErrorAction SilentlyContinue | Where-Object { $_.Id -ne $pidLaunched -and $_.StartTime.ToUniversalTime() -ge $startedUtc })

# 4. Verify: process, scratch, package integrity, evidence and its binding to the package.
$checks = [ordered]@{}
$checks['processExitedCleanly'] = (-not $timedOut) -and (-not $stillPresent) -and ($exitCode -eq 0)
$checks['noOtherAcadStarted'] = ($others.Count -eq 0)
$checks['scratchUnchanged'] = ((Sha256 $scratch) -eq $scratchBefore) -and -not (Get-ChildItem -LiteralPath $out -Filter 'scratch.*' | Where-Object { $_.Extension -in '.bak', '.sv$' })

$postDrift = @()
foreach ($entry in $expected.Keys) {
    $file = Join-Path $root ($entry.Replace('/', '\'))
    if (-not (Test-Path -LiteralPath $file) -or (Sha256 $file) -ne $expected[$entry]) { $postDrift += $entry }
}
$checks['packageUnchangedAfterRun'] = ($postDrift.Count -eq 0)

$evidencePath = Join-Path $out 'hostval-evidence.json'
$evidence = $null
$checks['evidenceExists'] = Test-Path -LiteralPath $evidencePath
if ($checks['evidenceExists']) {
    $evidence = Get-Content -Raw -LiteralPath $evidencePath | ConvertFrom-Json
    $checks['evidenceSchema'] = ((Prop $evidence 'schema') -eq 'I52-AUTH15-HV/1')
    $checks['evidenceFromThisProcess'] = ((Prop $evidence 'host','pid') -eq $pidLaunched)
    $checks['evidenceBoundToPackageSums'] = ((Prop $evidence 'package','sha256SumsDigest') -eq (Sha256 $sumsPath))
    $checks['evidencePluginHashEqualsPackage'] = ((Prop $evidence 'binding','loadedSha256') -eq $expected['run/RackCad.Plugin.dll'])
    $checks['evidenceImplementationShaEqualsMetadata'] = ((Prop $evidence 'implementationSha') -eq $meta.implementationSha)
    $checks['evidenceTreesEqual'] = ((Prop $evidence 'treesEqual') -eq $true)
    $checks['evidenceCompleted'] = ((Prop $evidence 'completed') -eq $true)
}

$launchValid = -not ($checks.Values -contains $false)
$verdict = if ($evidence) { [string](Prop $evidence 'verdict') } else { 'NO_EVIDENCE' }
$record = [ordered]@{
    schema = 'I52-AUTH15-HV-LAUNCH/1'
    package = $root
    implementationSha = $meta.implementationSha
    harnessSha = $meta.harnessSha
    acad = [ordered]@{ path = $Acad; sha256 = (Sha256 $Acad); fileVersion = (Get-Item -LiteralPath $Acad).VersionInfo.FileVersion }
    arguments = $arguments
    profile = $(if ($Profile) { $Profile } else { '<default>' })
    pid = $pidLaunched
    startUtc = $startedUtc.ToString('o')
    endUtc = $endedUtc.ToString('o')
    timeoutSeconds = $TimeoutSeconds
    timedOut = $timedOut
    exitCode = $exitCode
    scratchSha256Before = $scratchBefore
    scratchSha256After = (Sha256 $scratch)
    evidenceSha256 = $(if ($checks['evidenceExists']) { Sha256 $evidencePath } else { $null })
    checks = $checks
    launchValid = $launchValid
    verdict = $verdict
    ownerConfirmedNoTouch = $true
}
$record | ConvertTo-Json -Depth 6 | Out-File (Join-Path $out 'launcher-record.json') -Encoding utf8

"LAUNCH_VALID=$launchValid"
"HOST_VALIDATION_RESULT=$verdict"
foreach ($key in $checks.Keys) { "  $key = $($checks[$key])" }
if (-not $checks['evidenceExists']) {
    'No evidence was written. If the harness never ran, the usual cause is that the run folder is not in TRUSTEDPATHS (ENVIRONMENT_PREREQUISITE = TRUSTEDPATH_REQUIRED): the Owner adds it, this script never does. See out\hostval.log.'
}
if (-not $launchValid) { exit 2 }
