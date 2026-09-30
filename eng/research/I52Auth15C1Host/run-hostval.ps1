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
    # AutoCAD profile NAME for /p (default: the current profile). A profile FILE (.arg) cannot be inspected and is refused.
    [string]$Profile = '',
    [int]$TimeoutSeconds = 900,
    # Where the profile registry is READ from (never written). The default is the real AutoCAD 2025 key; the offline tests
    # point it at a private test key.
    [string]$AutoCadRegistryRoot = 'HKCU:\Software\Autodesk\AutoCAD\R25.0'
)

# I-52-AUTH15-C1 host validation launcher (adapted from the I-52-AUTH15 launcher fcca6e6c). ONE AutoCAD process, ONE run, no retry, no automatic rerun. It never writes
# SECURELOAD, TRUSTEDPATHS, FILEDIA or any other AutoCAD setting: the Owner trusts the run folder beforehand and run.scr restores
# FILEDIA itself. It does not patch anything.
#
# EXIT CODES (explicit, never inherited from a previous command):
#   0 = the launch was valid AND the evidence verdict is PASS
#   2 = INVALID: refusal before launch, or any launch / binding / package / evidence / process condition failed
#   3 = the launch was valid but the harness verdict is FAIL or UNKNOWN
# The last line printed is always: RUN_RESULT = PASS | INVALID | FAIL | UNKNOWN
#
# CLASSIFICATION (post RUN-2). "The run stopped early" is NOT "the run cannot be trusted". A run whose environment, package, process,
# binding, FILEDIA and evidence identity all check out is a VALID EXECUTION; its verdict is then PASS / FAIL / UNKNOWN. The evidence
# must be final and, to be valid without finishing all 15 cases, must record a GOVERNED stop (stopKind = deviation) with a FAIL verdict
# after at least one case beyond HV-00 ran. A stop before any AUTH-15 call (stopKind hv00), a FILEDIA stop, an exception, a timeout,
# a non-zero AutoCAD exit, malformed evidence or any identity mismatch stays INVALID.
#
# A PASS is also recomputed here from the rollback controls: exactly the 7 (id, dbKind) control records (all DOCUMENT-AUTHORITY), all PASS, no control leak,
# DOCUMENT-AUTHORITY available, and every rollback-sensitive HV case labelled DOCUMENT-AUTHORITY (evidenceControlSet /
# evidenceRollbackCasesDocument). A FAIL may be backed by a FAIL control.
$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

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

# Evaluate a check; any error inside it is a failed check, never a crash.
function Test-Check([scriptblock]$Block) {
    try { return [bool](& $Block) } catch { return $false }
}

# ---------------------------------------------------------------- FILEDIA record written by run.scr (strict, mirrors the harness)
function Read-Filedia([string]$path) {
    $r = [ordered]@{ present = $false; parseable = $false; before = $null; during = $null; after = $null; error = $null }
    if (-not (Test-Path -LiteralPath $path)) { return $r }
    $r.present = $true
    $r.parseable = $true
    foreach ($raw in (Get-Content -LiteralPath $path)) {
        $line = $raw.Trim()
        if ($line.Length -eq 0) { continue }
        if ($line -notmatch '^(before|during|after)=(-?\d+)$') { $r.parseable = $false; $r.error = "unparseable line: $line"; return $r }
        $key = $Matches[1]
        if ($null -ne $r[$key]) { $r.parseable = $false; $r.error = "repeated key: $line"; return $r }
        $r[$key] = [int]$Matches[2]
    }
    return $r
}

# ---------------------------------------------------------------- read-only TRUSTEDPATHS preflight
function Get-TrustState([string]$RegistryRoot, [string]$ProfileName, [string]$Folder) {
    function Result($state, $detail) { [pscustomobject]@{ State = $state; Detail = $detail } }
    if (-not (Test-Path -LiteralPath $RegistryRoot)) { return Result 'UNDETERMINED' "registry root not found: $RegistryRoot" }
    $products = @(Get-ChildItem -LiteralPath $RegistryRoot | Where-Object { $_.PSChildName -like 'ACAD-*' })
    if ($products.Count -ne 1) { return Result 'UNDETERMINED' "expected exactly one ACAD-* product key under $RegistryRoot, found $($products.Count)" }
    $profilesKey = Join-Path $products[0].PSPath 'Profiles'
    if (-not (Test-Path -LiteralPath $profilesKey)) { return Result 'UNDETERMINED' 'no Profiles key' }
    if (-not $ProfileName) { $ProfileName = [string](Get-Item -LiteralPath $profilesKey).GetValue('') }
    if (-not $ProfileName) { return Result 'UNDETERMINED' 'the current profile name is not recorded' }
    if ($ProfileName -match '[\\/]' -or $ProfileName -like '*.arg') { return Result 'UNDETERMINED' "profile '$ProfileName' is a file, not a named profile" }
    $variables = Join-Path (Join-Path $profilesKey $ProfileName) 'Variables'
    if (-not (Test-Path -LiteralPath $variables)) { return Result 'UNDETERMINED' "profile '$ProfileName' has no Variables key" }
    $key = Get-Item -LiteralPath $variables
    $secureLoad = $key.GetValue('SECURELOAD')
    $secureLoadText = if ($null -eq $secureLoad) { '1 (default: not stored)' } else { [string]$secureLoad }
    if ($null -ne $secureLoad -and [int]$secureLoad -eq 0) { return Result 'NOT_REQUIRED' "profile '$ProfileName': SECURELOAD=0" }
    $entries = @(([string]$key.GetValue('TRUSTEDPATHS')).Split(';') | ForEach-Object { $_.Trim() } | Where-Object { $_ })
    $target = [IO.Path]::GetFullPath($Folder).TrimEnd('\')
    foreach ($entry in $entries) {
        $recursive = $entry.EndsWith('...')
        $base = if ($recursive) { $entry.Substring(0, $entry.Length - 3) } else { $entry }
        $base = [IO.Path]::GetFullPath([Environment]::ExpandEnvironmentVariables($base)).TrimEnd('\')
        if ([string]::Equals($base, $target, [StringComparison]::OrdinalIgnoreCase)) { return Result 'TRUSTED' "profile '$ProfileName' (SECURELOAD=$secureLoadText): exact entry '$entry'" }
        if ($recursive -and $target.StartsWith($base + '\', [StringComparison]::OrdinalIgnoreCase)) { return Result 'TRUSTED' "profile '$ProfileName' (SECURELOAD=$secureLoadText): recursive entry '$entry'" }
    }
    return Result 'NOT_TRUSTED' "profile '$ProfileName' (SECURELOAD=$secureLoadText) lists $($entries.Count) trusted path(s), none covers $target"
}

function Invoke-Launch {
    if (-not $OwnerConfirmsNoTouch) {
        throw 'the run is only valid if nobody touches the computer from launch until the process exits. Re-run with -OwnerConfirmsNoTouch once the Owner has confirmed that.'
    }

    $root = (Resolve-Path -LiteralPath $Package).Path
    $run = Join-Path $root 'run'
    $out = Join-Path $root 'out'
    $sumsPath = Join-Path $root 'SHA256SUMS'
    $metaPath = Join-Path $root 'TRANSFER-METADATA.json'
    $template = Join-Path $root 'launcher\run.scr'
    foreach ($required in $run, $sumsPath, $metaPath, $template) { if (-not (Test-Path -LiteralPath $required)) { throw "package incomplete, missing: $required" } }
    if (-not (Test-Path -LiteralPath $Acad)) { throw "acad.exe not found: $Acad" }
    if (-not (Test-Path -LiteralPath $ScratchDrawing)) { throw "scratch drawing not found: $ScratchDrawing" }

    # 1. The package is exactly what was built: every SHA256SUMS entry matches and run\ holds nothing else.
    $expected = [ordered]@{}
    foreach ($line in Get-Content -LiteralPath $sumsPath) {
        $at = $line.IndexOf('  ')
        if ($at -gt 0) { $expected[$line.Substring($at + 2).Trim()] = $line.Substring(0, $at).Trim() }
    }
    if ($expected.Count -eq 0) { throw 'SHA256SUMS is empty or unreadable' }
    $drift = @()
    foreach ($entry in $expected.Keys) {
        $file = Join-Path $root ($entry.Replace('/', '\'))
        if (-not (Test-Path -LiteralPath $file) -or (Sha256 $file) -ne $expected[$entry]) { $drift += $entry }
    }
    $runFiles = @(Get-ChildItem -LiteralPath $run -Recurse -File | ForEach-Object { 'run/' + $_.FullName.Substring($run.Length + 1).Replace('\', '/') })
    $extra = @($runFiles | Where-Object { -not $expected.Contains($_) })
    if ($drift.Count -gt 0) { throw "package files differ from SHA256SUMS: $($drift -join ', ')" }
    if ($extra.Count -gt 0) { throw "run\ holds files that are not in SHA256SUMS: $($extra -join ', ')" }

    $meta = Get-Content -Raw -LiteralPath $metaPath | ConvertFrom-Json
    if ($meta.treesEqual -ne $true -or -not $meta.implementationSha -or -not $meta.harnessSha) { throw 'TRANSFER-METADATA.json does not declare treesEqual=true for an implementation SHA and a harness SHA.' }

    # 2. Environment prerequisites (read-only checks; nothing is changed).
    $acadName = [IO.Path]::GetFileNameWithoutExtension($Acad)
    if (Get-Process -Name $acadName -ErrorAction SilentlyContinue) { throw "$acadName.exe is already running. The run needs its own dedicated, untouched process (a running instance could also swallow the launch)." }

    $bundleRoots = @((Join-Path $env:APPDATA 'Autodesk\ApplicationPlugins'), (Join-Path $env:ProgramData 'Autodesk\ApplicationPlugins')) | Where-Object { $_ -and (Test-Path -LiteralPath $_) }
    $bundles = @($bundleRoots | ForEach-Object { Get-ChildItem -LiteralPath $_ -Directory -Filter '*RackCad*' -ErrorAction SilentlyContinue })
    if ($bundles.Count -gt 0) { throw "ENVIRONMENT_PREREQUISITE = RACKCAD_AUTOLOAD_PRESENT ($($bundles.FullName -join '; ')). An autoloaded RackCad would load a second Plugin." }

    # The scratch drawing: a real .dwg (signature bytes), copied so the original is never opened.
    if ([IO.Path]::GetExtension($ScratchDrawing).ToLowerInvariant() -ne '.dwg') { throw 'the scratch drawing must be a .dwg' }
    $head = New-Object byte[] 6
    $stream = [IO.File]::OpenRead((Resolve-Path -LiteralPath $ScratchDrawing).Path)
    try { [void]$stream.Read($head, 0, 6) } finally { $stream.Dispose() }
    $magic = [Text.Encoding]::ASCII.GetString($head)
    if ($magic -notmatch '^AC10\d\d$') { throw "the scratch drawing does not carry a DWG signature (first bytes '$magic', expected AC10xx)" }

    # TRUSTEDPATHS: the exact run folder must already be trusted (or SECURELOAD is 0). Without this an untrusted NETLOAD shows a
    # modal dialog and the unattended run just hangs until the timeout. READ-ONLY: this script never changes either setting.
    $trust = Get-TrustState $AutoCadRegistryRoot $Profile $run
    Write-Host "TRUSTEDPATHS preflight: $($trust.State) - $($trust.Detail)"
    if ($trust.State -eq 'NOT_TRUSTED') { throw "ENVIRONMENT_PREREQUISITE = TRUSTEDPATH_REQUIRED: the Owner must add $run to TRUSTEDPATHS ($($trust.Detail))." }
    if ($trust.State -eq 'UNDETERMINED') { throw "ENVIRONMENT_PREREQUISITE = TRUSTEDPATH_UNDETERMINED: cannot verify that $run is trusted ($($trust.Detail)). The launcher does not guess." }

    if (Test-Path -LiteralPath (Join-Path $out 'hostval-evidence.json')) { throw 'out\ already holds evidence: a run is never repeated or overwritten without authorization.' }
    if ($out -match '["]') { throw 'the package path must not contain a double quote' }

    New-Item -ItemType Directory -Force $out | Out-Null
    foreach ($leftover in 'scratch.dwg', 'blank-template.dwg', 'doc-cases', 'run.scr', 'filedia.txt', 'launcher-record.json') {
        if (Test-Path -LiteralPath (Join-Path $out $leftover)) { throw "out\$leftover already exists: a run is never repeated or overwritten without authorization." }
    }
    $scratch = Join-Path $out 'scratch.dwg'
    Copy-Item -LiteralPath $ScratchDrawing $scratch
    $scratchBefore = Sha256 $scratch

    # The blank TEMPLATE is a second copy that is never opened: the harness copies it once per document-authority case and opens the
    # copy (then closes it with discard), so the anchor scratch drawing above stays untouched whatever a rollback does.
    $blankTemplate = Join-Path $out 'blank-template.dwg'
    Copy-Item -LiteralPath $ScratchDrawing $blankTemplate
    $blankBefore = Sha256 $blankTemplate

    # 3. DRIVER script from the template. The template placeholders are the only variables; anything else is refused.
    $script = Join-Path $out 'run.scr'
    $text = (Get-Content -Raw -LiteralPath $template).Replace('{RUN}', $run).Replace('{OUT_FWD}', $out.Replace('\', '/')).Replace("`r`n", "`n")
    if ($text -match '\{[A-Z_]+\}') { throw "run.scr still holds an unresolved placeholder: $($Matches[0])" }
    [IO.File]::WriteAllText($script, $text, (New-Object Text.UTF8Encoding($false)))

    $arguments = "`"$scratch`" /nologo /nossm" + $(if ($Profile) { " /p `"$Profile`"" } else { '' }) + " /b `"$script`""
    $psi = New-Object Diagnostics.ProcessStartInfo($Acad, $arguments)
    $psi.UseShellExecute = $false
    $psi.WorkingDirectory = $out
    $psi.Environment['I52_AUTH15_HV_OUT'] = $out
    $psi.Environment['I52_AUTH15_HV_OWNER_NOTOUCH'] = '1'          # the Owner's -OwnerConfirmsNoTouch, declared to the harness
    $psi.Environment['I52_AUTH15_HV_SCRATCH_SHA256'] = $scratchBefore

    $startedUtc = (Get-Date).ToUniversalTime()
    $process = [Diagnostics.Process]::Start($psi)
    $pidLaunched = $process.Id
    $processStartUtc = $null
    try { $processStartUtc = $process.StartTime.ToUniversalTime() } catch { $processStartUtc = $null }
    Write-Host "LAUNCHED pid=$pidLaunched at $($startedUtc.ToString('o')); waiting up to $TimeoutSeconds s. Do not touch the computer."

    $timedOut = -not $process.WaitForExit($TimeoutSeconds * 1000)
    if ($timedOut) {
        & taskkill.exe /PID $pidLaunched /T /F 2>&1 | Out-Null
        [void]$process.WaitForExit(30000)
    }
    $exitCode = $null
    if ($process.HasExited) { $exitCode = $process.ExitCode }
    $endedUtc = (Get-Date).ToUniversalTime()
    $stillPresent = [bool](Get-Process -Id $pidLaunched -ErrorAction SilentlyContinue)
    $others = @(Get-Process -Name $acadName -ErrorAction SilentlyContinue | Where-Object { $_.Id -ne $pidLaunched -and $_.StartTime.ToUniversalTime() -ge $startedUtc })

    # 4. Verify. Every check is defensive: an error inside a check is a FAILED check, never a crash, and the launcher record
    #    is always written.
    $checks = [ordered]@{}
    $checks['processExitedCleanly'] = (-not $timedOut) -and (-not $stillPresent) -and ($exitCode -eq 0)
    $checks['noOtherAcadStarted'] = ($others.Count -eq 0)
    $checks['scratchUnchanged'] = Test-Check { ((Sha256 $scratch) -eq $scratchBefore) -and -not (Get-ChildItem -LiteralPath $out -Filter 'scratch.*' | Where-Object { $_.Extension -in '.bak', '.sv$' }) }
    $checks['blankTemplateUnchanged'] = Test-Check { (Sha256 $blankTemplate) -eq $blankBefore }
    $checks['packageUnchangedAfterRun'] = Test-Check {
        $postDrift = @($expected.Keys | Where-Object { $f = Join-Path $root ($_.Replace('/', '\')); -not (Test-Path -LiteralPath $f) -or (Sha256 $f) -ne $expected[$_] })
        $postDrift.Count -eq 0
    }

    # FILEDIA: the original must have been restored exactly (FILEDIA is the Owner's preference and is never left changed).
    $fd = Read-Filedia (Join-Path $out 'filedia.txt')
    $checks['filediaRecorded'] = ($fd.present -and $fd.parseable -and $null -ne $fd.before -and $null -ne $fd.during -and $null -ne $fd.after)
    $checks['filediaRestored'] = ($checks['filediaRecorded'] -and $fd.during -eq 0 -and $fd.after -eq $fd.before)

    $evidencePath = Join-Path $out 'hostval-evidence.json'
    $evidence = $null
    $parseError = $null
    $exists = Test-Path -LiteralPath $evidencePath
    $parseable = $false
    if ($exists) {
        try {
            $evidence = Get-Content -Raw -LiteralPath $evidencePath | ConvertFrom-Json -ErrorAction Stop
            $parseable = ($null -ne $evidence) -and ($evidence -isnot [string])
        } catch {
            $parseError = $_.Exception.Message
            $evidence = $null
            $parseable = $false
        }
    }
    $checks['evidenceExists'] = $exists
    $checks['evidenceParseable'] = $parseable

    $sumOf = { param($name) $expected["run/$name"] }
    $checks['evidenceSchema'] = $parseable -and ((Prop $evidence 'schema') -eq 'I52-AUTH15-HV/1')
    $checks['evidenceFromThisProcess'] = $parseable -and ((Prop $evidence 'host', 'pid') -eq $pidLaunched)
    $checks['evidenceBoundToPackageSums'] = $parseable -and (Test-Check { (Prop $evidence 'package', 'sha256SumsDigest') -eq (Sha256 $sumsPath) })
    # Per assembly: the EXPECTED hash recorded before HV-00, SHA256SUMS, TRANSFER-METADATA and the LOADED hash observed by HV-00 must
    # all be the same value. A missing loaded fact (HV-00 never got there) is a failed check.
    foreach ($pair in @(@('Plugin', 'RackCad.Plugin.dll'), @('Application', 'RackCad.Application.dll'), @('Domain', 'RackCad.Domain.dll'), @('Harness', 'I52Auth15.HostHarness.dll'))) {
        $short = $pair[0]
        $file = $pair[1]
        $checks["evidence${short}Hash"] = $parseable -and (Test-Check {
            $sum = & $sumOf $file
            (-not [string]::IsNullOrEmpty($sum)) -and ((Prop $evidence 'package', "expected${short}Sha256") -eq $sum) -and ($meta.dlls.$file.sha256 -eq $sum) -and ((Prop $evidence 'binding', "loaded${short}Sha256") -eq $sum)
        })
    }
    $checks['evidenceProcessStart'] = $parseable -and (Test-Check {
        [math]::Abs(([datetime](Prop $evidence 'host', 'processStartUtc')).ToUniversalTime().Ticks - $processStartUtc.Ticks) -lt 20000000
    })
    $checks['evidenceOwnerNoTouchDeclared'] = $parseable -and ((Prop $evidence 'host', 'ownerNoTouchDeclaredByLauncher') -eq $true)
    $checks['evidenceScratchIdentity'] = $parseable -and (Test-Check {
        ((Prop $evidence 'host', 'scratchSha256DeclaredByLauncher') -eq $scratchBefore) -and ((Prop $evidence 'host', 'scratchSha256AtHarnessStart') -eq $scratchBefore)
    })
    $checks['evidenceEarlyIdentityClean'] = $parseable -and (Test-Check {
        $hostSection = Prop $evidence 'host'
        ($null -ne $hostSection.PSObject.Properties['earlyIdentityErrors']) -and (@($hostSection.earlyIdentityErrors).Count -eq 0)
    })
    # Defence in depth: the launcher does not take the harness's word for its verdict. PASS must be backed by exactly the cases HV-00..HV-15,
    # all PASS, no problems, no leaks, no deviation and no stop. FAIL must be backed by a FAIL case, a FAIL control or a recorded deviation.
    $checks['evidenceVerdictConsistent'] = $parseable -and (Test-Check {
        $verdictText = [string](Prop $evidence 'verdict')
        $cases = @($evidence.cases)
        if ($verdictText -eq 'PASS') {
            $expectedIds = (0..15 | ForEach-Object { 'HV-{0:D2}' -f $_ }) -join ','
            return ($cases.Count -eq 16) -and ((($cases | ForEach-Object { $_.id } | Sort-Object) -join ',') -eq $expectedIds) -and (@($cases | Where-Object { $_.result -ne 'PASS' }).Count -eq 0) -and (@($cases | Where-Object { $_.deviation -eq $true }).Count -eq 0) -and (@($evidence.problems).Count -eq 0) -and (@($evidence.leaks).Count -eq 0) -and (@($evidence.deviations).Count -eq 0) -and ([string](Prop $evidence 'stopKind') -eq 'none')
        }
        if ($verdictText -eq 'FAIL') {
            return (@($cases | Where-Object { $_.result -eq 'FAIL' }).Count -gt 0) -or (@($evidence.controls | Where-Object { $_.result -eq 'FAIL' }).Count -gt 0) -or (@($evidence.deviations).Count -gt 0)
        }
        return $true
    })

    # The rollback CONTROLS govern a PASS, and the launcher recomputes that itself: exactly the 7 (id, dbKind) records, none missing, none
    # duplicated, none of another kind, every one PASS, no control leak, and DOCUMENT-AUTHORITY available. A SIDE-DB record never stands in
    # for a DOCUMENT-AUTHORITY one because the pairing is part of the key. (Only a claimed PASS is judged here: a FAIL or UNKNOWN verdict
    # already refuses to be published as a PASS, and evidenceVerdictConsistent above requires a FAIL to be backed.)
    $expectedControlKeys = @('RB-01V|DOCUMENT-AUTHORITY', 'RB-01D|DOCUMENT-AUTHORITY', 'RB-02a|DOCUMENT-AUTHORITY', 'RB-02b|DOCUMENT-AUTHORITY',
        'RB-02c|DOCUMENT-AUTHORITY', 'RB-03|DOCUMENT-AUTHORITY', 'RB-05|DOCUMENT-AUTHORITY')
    $checks['evidenceControlSet'] = $parseable -and (Test-Check {
        if ([string](Prop $evidence 'verdict') -ne 'PASS') { return $true }
        $controls = @($evidence.controls)
        $keys = @($controls | ForEach-Object { '{0}|{1}' -f $_.id, $_.dbKind })
        ($controls.Count -eq 7) -and ((($keys | Sort-Object) -join ',') -eq (($expectedControlKeys | Sort-Object) -join ',')) -and
            (@($controls | Where-Object { $_.result -ne 'PASS' }).Count -eq 0) -and
            (@($controls | Where-Object { @($_.leaks).Count -gt 0 }).Count -eq 0) -and
            (@($evidence.controlLeaks).Count -eq 0) -and
            ((Prop $evidence 'documentAuthority', 'available') -eq $true)
    })
    # The rollback-sensitive HV cases are governed by their DOCUMENT-AUTHORITY run; a SIDE-DB label (or a side characterization) never satisfies them.
    $checks['evidenceRollbackCasesDocument'] = $parseable -and (Test-Check {
        if ([string](Prop $evidence 'verdict') -ne 'PASS') { return $true }
        $cases = @($evidence.cases)
        $ok = $true
        foreach ($id in 'HV-01', 'HV-02', 'HV-03', 'HV-05', 'HV-08', 'HV-10', 'HV-12', 'HV-13', 'HV-14', 'HV-15') {
            $mine = @($cases | Where-Object { $_.id -eq $id })
            if (($mine.Count -ne 1) -or ($mine[0].dbKind -ne 'DOCUMENT-AUTHORITY')) { $ok = $false }
        }
        $ok
    })
    $checks['evidenceHarnessShaEqualsMetadata'] = $parseable -and ((Prop $evidence 'harnessSha') -eq $meta.harnessSha)
    $checks['evidenceImplementationShaEqualsMetadata'] = $parseable -and ((Prop $evidence 'implementationSha') -eq $meta.implementationSha)
    $checks['evidenceTreesEqualMetadata'] = $parseable -and (Test-Check { ((Prop $evidence 'treesEqual') -eq $true) -and ((Prop $evidence 'srcTree') -eq $meta.implementationSrcTree) -and ((Prop $evidence 'testsTree') -eq $meta.implementationTestsTree) -and ((Prop $evidence 'harnessSrcTree') -eq $meta.harnessSrcTree) -and ((Prop $evidence 'harnessTestsTree') -eq $meta.harnessTestsTree) })
    $checks['evidenceFilediaMatchesRecord'] = $parseable -and $checks['filediaRecorded'] -and (Test-Check { ((Prop $evidence 'host', 'filediaBefore') -eq $fd.before) -and ((Prop $evidence 'host', 'filediaDuringNetload') -eq $fd.during) -and ((Prop $evidence 'host', 'filediaAfter') -eq $fd.after) -and ((Prop $evidence 'host', 'filediaLiveAtHarnessStart') -eq $fd.before) -and ((Prop $evidence 'host', 'filediaLiveAtHarnessEnd') -eq $fd.before) })
    $checks['evidenceFinal'] = $parseable -and ((Prop $evidence 'state') -eq 'final')
    # Completed, OR a governed stop that produced a FAIL after real AUTH-15 work. Anything else that did not finish is untrustworthy.
    $checks['evidenceRunShape'] = $parseable -and (Test-Check {
        if ((Prop $evidence 'completed') -eq $true) { return $true }
        $beyondHv00 = @($evidence.cases | Where-Object { $_.id -ne 'HV-00' -and $_.result -ne 'NOT_RUN' }).Count -gt 0
        ([string](Prop $evidence 'stopKind') -eq 'deviation') -and ([string](Prop $evidence 'verdict') -eq 'FAIL') -and $beyondHv00
    })
    foreach ($key in @($checks.Keys)) { $checks[$key] = [bool]$checks[$key] }

    $launchValid = -not ($checks.Values -contains $false)
    $verdict = if ($parseable) { [string](Prop $evidence 'verdict') } elseif ($exists) { 'MALFORMED_EVIDENCE' } else { 'NO_EVIDENCE' }
    $runResult = if (-not $launchValid) { 'INVALID' } elseif ($verdict -eq 'PASS') { 'PASS' } elseif ($verdict -eq 'FAIL') { 'FAIL' } else { 'UNKNOWN' }
    $launcherExit = switch ($runResult) { 'PASS' { 0 } 'INVALID' { 2 } default { 3 } }

    $record = [ordered]@{
        schema = 'I52-AUTH15-HV-LAUNCH/2'
        package = $root
        implementationSha = $meta.implementationSha
        harnessSha = $meta.harnessSha
        acad = [ordered]@{ path = $Acad; sha256 = $(try { Sha256 $Acad } catch { $null }); fileVersion = $(try { (Get-Item -LiteralPath $Acad).VersionInfo.FileVersion } catch { $null }) }
        arguments = $arguments
        profile = $(if ($Profile) { $Profile } else { '<current>' })
        trustedPaths = [ordered]@{ state = $trust.State; detail = $trust.Detail }
        pid = $pidLaunched
        processStartUtc = $(if ($processStartUtc) { $processStartUtc.ToString('o') } else { $null })
        startUtc = $startedUtc.ToString('o')
        endUtc = $endedUtc.ToString('o')
        timeoutSeconds = $TimeoutSeconds
        timedOut = $timedOut
        exitCode = $exitCode
        scratch = [ordered]@{ signature = $magic; sha256Before = $scratchBefore; sha256After = $(try { Sha256 $scratch } catch { $null }) }
        filedia = $fd
        blankTemplate = [ordered]@{ sha256Before = $blankBefore; sha256After = $(try { Sha256 $blankTemplate } catch { $null }) }
        stopKind = $(if ($parseable) { [string](Prop $evidence 'stopKind') } else { $null })
        evidenceSha256 = $(if ($exists) { Sha256 $evidencePath } else { $null })
        evidenceParseError = $parseError
        checks = $checks
        launchValid = $launchValid
        verdict = $verdict
        runResult = $runResult
        launcherExitCode = $launcherExit
        ownerConfirmedNoTouch = $true
        note = 'This record and hostval-evidence.json are the host evidence. Nothing produced by the offline tests is.'
    }
    $record | ConvertTo-Json -Depth 8 | Out-File (Join-Path $out 'launcher-record.json') -Encoding utf8

    Write-Host "LAUNCH_VALID=$launchValid"
    Write-Host "HOST_VALIDATION_RESULT=$verdict"
    foreach ($key in $checks.Keys) { Write-Host "  $key = $($checks[$key])" }

    if ($timedOut -or -not $checks['filediaRestored']) {
        $original = if ($null -ne $fd.before) { "$($fd.before)" } else { 'UNKNOWN (filedia.txt holds no before= line)' }
        Write-Host ''
        Write-Host '!!! FILEDIA RECOVERY !!!' -ForegroundColor Red
        Write-Host "The ORIGINAL FILEDIA captured by run.scr is: $original" -ForegroundColor Red
        Write-Host "run.scr sets FILEDIA to 0 around NETLOAD. If the run was cut short, AutoCAD's FILEDIA may still be 0. In a new AutoCAD session type: FILEDIA $original" -ForegroundColor Red
        Write-Host 'This launcher never writes FILEDIA (or any AutoCAD setting) itself.' -ForegroundColor Red
        Write-Host ''
    }
    if (-not $exists) {
        Write-Host 'No evidence was written. If the harness never ran, the usual cause is that NETLOAD was refused (run folder not in TRUSTEDPATHS) or the script stopped early. See out\hostval.log and out\filedia.txt.'
    }
    elseif (-not $parseable) {
        Write-Host "The evidence file exists but cannot be parsed ($parseError). The run is INVALID; launcher-record.json was written."
    }

    Write-Host "RUN_RESULT = $runResult"
    return [int]$launcherExit
}

$finalExit = 2
try {
    $finalExit = [int](Invoke-Launch | Select-Object -Last 1)
}
catch {
    Write-Host "STOP: $($_.Exception.Message)"
    Write-Host 'LAUNCH_VALID=False'
    Write-Host 'RUN_RESULT = INVALID'
    $finalExit = 2
}
exit $finalExit
