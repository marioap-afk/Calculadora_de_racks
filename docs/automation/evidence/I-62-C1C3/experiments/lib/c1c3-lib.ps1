# c1c3-lib.ps1 - biblioteca de los experimentos desechables de I-62 C1-C3 (diseno, sin implementacion).
# Todo se ejecuta en procesos hijos y en repositorios desechables bajo D:\r62-c1c3\work.
# No toca ningun checkout real, ninguna configuracion de Git global/sistema, ni el PATH del sistema.
Set-StrictMode -Version 3
$ErrorActionPreference = 'Stop'
[Console]::OutputEncoding = [Text.UTF8Encoding]::new($false)

$script:Root = 'D:\r62-c1c3'

# ---------- saneamiento (sin nombre de usuario ni correos en las salidas) ----------
function Get-ShortUserSegment {
    $t = [IO.Path]::GetTempPath()
    $parts = $t -split '\\'
    if ($parts.Count -gt 2 -and $parts[1] -ieq 'Users') { return $parts[2] }
    return $env:USERNAME
}
$script:SanUserProfile = $env:USERPROFILE
$script:SanUserName = $env:USERNAME
$script:SanShort = Get-ShortUserSegment

function ConvertTo-Sanitized([AllowNull()][string]$s) {
    if ($null -eq $s) { return $null }
    $r = $s
    $r = [regex]::Replace($r, [regex]::Escape($script:SanUserProfile), '%USERPROFILE%', 'IgnoreCase')
    $r = [regex]::Replace($r, [regex]::Escape($script:SanUserProfile.Replace('\', '/')), '%USERPROFILE%', 'IgnoreCase')
    $r = [regex]::Replace($r, [regex]::Escape($script:SanUserProfile.Replace('\', '\\')), '%USERPROFILE%', 'IgnoreCase')
    foreach ($tok in @($script:SanShort, $script:SanUserName) | Where-Object { $_ } | Select-Object -Unique) {
        $r = [regex]::Replace($r, [regex]::Escape($tok), '<usuario>', 'IgnoreCase')
    }
    # correos con forma usuario@dominio
    $r = [regex]::Replace($r, '[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,}', '<correo>')
    return $r
}

function Write-JsonResult([string]$Path, $Object) {
    $json = $Object | ConvertTo-Json -Depth 30
    $json = (ConvertTo-Sanitized $json) -replace "`r`n", "`n"
    [IO.File]::WriteAllText($Path, $json + "`n", [Text.UTF8Encoding]::new($false))
    return (Get-FileHash -LiteralPath $Path -Algorithm SHA256).Hash.ToLowerInvariant()
}

# ---------- registro de pasos ----------
function New-StepLog { return ,([System.Collections.Generic.List[object]]::new()) }
function Add-Step($Log, [string]$Step, [string]$Command, [string]$Cwd, $ExitCode, $Result) {
    $Log.Add([ordered]@{ Step = $Step; Command = (ConvertTo-Sanitized $Command); Cwd = (ConvertTo-Sanitized $Cwd); ExitCode = $ExitCode; Result = $Result })
}

# ---------- Git aislado del entorno del usuario (solo en este proceso) ----------
function Initialize-IsolatedGit {
    $empty = Join-Path $script:Root 'lib\gitconfig-empty'
    if (-not (Test-Path -LiteralPath $empty)) { [IO.File]::WriteAllText($empty, '') }
    $env:GIT_CONFIG_NOSYSTEM = '1'
    $env:GIT_CONFIG_GLOBAL = $empty
    $env:GIT_TERMINAL_PROMPT = '0'
    $env:GIT_OPTIONAL_LOCKS = $null
}
# Envoltorio de git para escrituras del fixture (identidad sintetica, sin correo)
function G {
    & git -c user.name=fixture -c user.email= -c core.safecrlf=false -c core.fsmonitor=false @args
}

# ---------- fixture: "checkout principal" + worktree de la unidad + worktree hermano ----------
function New-FixturePair([string]$Dir, [switch]$AutoCrlf) {
    if (Test-Path -LiteralPath $Dir) { Remove-Item -LiteralPath $Dir -Recurse -Force }
    New-Item -ItemType Directory -Force $Dir | Out-Null
    $main = Join-Path $Dir 'main'
    G init -q -b main $main | Out-Null
    if ($AutoCrlf) { G -C $main config core.autocrlf true | Out-Null }
    $utf8bom = [Text.UTF8Encoding]::new($true)
    $sln = @(
        'Microsoft Visual Studio Solution File, Format Version 12.00',
        'Project("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}") = "RackCad.Core", "src\RackCad.Core\RackCad.Core.csproj", "{11111111-1111-1111-1111-111111111111}"',
        'EndProject',
        'Global',
        'EndGlobal'
    ) -join "`r`n"
    [IO.File]::WriteAllText((Join-Path $main 'RackCad.sln'), $sln + "`r`n", $utf8bom)
    New-Item -ItemType Directory -Force (Join-Path $main 'src'), (Join-Path $main 'docs') | Out-Null
    [IO.File]::WriteAllText((Join-Path $main 'src\A.cs'), "namespace Fixture { class A { } }`r`n")
    [IO.File]::WriteAllText((Join-Path $main 'docs\x.md'), "# fixture`r`n")
    [IO.File]::WriteAllText((Join-Path $main '.gitignore'), "bin/`nobj/`n.vs/`n*.user`nartifacts/`n")
    G -C $main add -A | Out-Null
    G -C $main commit -q -m 'fixture: estado inicial' | Out-Null
    $unit = Join-Path $Dir 'wt-unit'
    $sib = Join-Path $Dir 'wt-sibling'
    G -C $main worktree add -q -b unit $unit 2>$null | Out-Null
    G -C $main worktree add -q -b sibling $sib 2>$null | Out-Null
    return [ordered]@{ Root = $Dir; Main = $main; Unit = $unit; Sibling = $sib }
}

# ---------- C1: enumeracion de checkouts ----------
function Get-CheckoutList([string]$AnyCheckout) {
    $raw = & git -C $AnyCheckout --no-optional-locks worktree list --porcelain
    $list = [System.Collections.Generic.List[object]]::new()
    $cur = $null
    foreach ($line in $raw) {
        if ($line -like 'worktree *') { if ($cur) { $list.Add($cur) }; $cur = [ordered]@{ Path = $line.Substring(9).Replace('/', '\'); Head = $null; Branch = $null; Flags = @() } }
        elseif ($line -like 'HEAD *') { $cur.Head = $line.Substring(5) }
        elseif ($line -like 'branch *') { $cur.Branch = $line.Substring(7) }
        elseif ($line -ne '') { $cur.Flags += $line }
    }
    if ($cur) { $list.Add($cur) }
    for ($i = 0; $i -lt $list.Count; $i++) { $list[$i].IsMainWorkingTree = ($i -eq 0) }
    return $list
}

# ---------- C1: fotografia de un checkout ----------
function Get-FsSweep([string]$Path, [switch]$WithChangeTime) {
    $files = [System.Collections.Generic.SortedDictionary[string, object]]::new([StringComparer]::OrdinalIgnoreCase)
    $dirs = [System.Collections.Generic.SortedDictionary[string, object]]::new([StringComparer]::OrdinalIgnoreCase)
    $rootInfo = [IO.DirectoryInfo]::new($Path)
    $dirs['.'] = [ordered]@{ MtimeTicks = $rootInfo.LastWriteTimeUtc.Ticks }
    $opt = [IO.EnumerationOptions]::new(); $opt.RecurseSubdirectories = $false; $opt.AttributesToSkip = 0; $opt.IgnoreInaccessible = $true
    $stack = [System.Collections.Generic.Stack[string]]::new(); $stack.Push($Path)
    while ($stack.Count -gt 0) {
        $d = $stack.Pop()
        foreach ($e in [IO.DirectoryInfo]::new($d).EnumerateFileSystemInfos('*', $opt)) {
            $rel = [IO.Path]::GetRelativePath($Path, $e.FullName).Replace('\', '/')
            if ($rel -ieq '.git' -or $rel -like '.git/*') { continue }   # interno de Git: fuera del barrido
            $isDir = ($e.Attributes -band [IO.FileAttributes]::Directory) -ne 0
            $isReparse = ($e.Attributes -band [IO.FileAttributes]::ReparsePoint) -ne 0
            if ($isDir) {
                $dirs[$rel] = [ordered]@{ MtimeTicks = $e.LastWriteTimeUtc.Ticks }
                if (-not $isReparse) { $stack.Push($e.FullName) }
            } else {
                $row = [ordered]@{ Length = ([IO.FileInfo]$e).Length; MtimeTicks = $e.LastWriteTimeUtc.Ticks }
                if ($WithChangeTime) { $row.ChangeTicks = Get-NtfsChangeTimeTicks $e.FullName }
                $files[$rel] = $row
            }
        }
    }
    return [ordered]@{ Files = $files; Dirs = $dirs }
}

function Get-CheckoutSnapshot([string]$Path, [string]$Role, [switch]$WithChangeTime, [switch]$HashAllTracked) {
    $sw = [Diagnostics.Stopwatch]::StartNew()
    $head = (& git -C $Path --no-optional-locks rev-parse HEAD).Trim()
    $branch = & git -C $Path --no-optional-locks symbolic-ref -q --short HEAD
    $raw = & git -C $Path --no-optional-locks -c core.fsmonitor=false -c core.untrackedCache=false status --porcelain=v1 -z --untracked-files=all
    $joined = ($raw -join "`n")
    $entries = [System.Collections.Generic.List[string]]::new()
    if ($joined) {
        $tok = $joined.Split([char]0, [StringSplitOptions]::RemoveEmptyEntries)
        for ($i = 0; $i -lt $tok.Count; $i++) {
            $e = $tok[$i]
            if ($e.Length -ge 3 -and ($e[0] -eq 'R' -or $e[0] -eq 'C')) { $entries.Add($e + ' <- ' + $tok[$i + 1]); $i++ }
            else { $entries.Add($e) }
        }
    }
    $hashes = [ordered]@{}
    foreach ($e in ($entries | Sort-Object)) {
        $p = $e.Substring(3); if ($p -like '* <- *') { $p = $p.Split(' <- ')[0] }
        $full = Join-Path $Path $p
        if (Test-Path -LiteralPath $full -PathType Leaf) { $hashes[$p] = (Get-FileHash -LiteralPath $full -Algorithm SHA256).Hash.ToLowerInvariant() }
        else { $hashes[$p] = 'ABSENT' }
    }
    $trackedHashes = $null
    if ($HashAllTracked) {
        $trackedHashes = [ordered]@{}
        foreach ($f in (& git -C $Path --no-optional-locks ls-files)) {
            $full = Join-Path $Path $f
            if (Test-Path -LiteralPath $full -PathType Leaf) { $trackedHashes[$f] = (Get-FileHash -LiteralPath $full -Algorithm SHA256).Hash.ToLowerInvariant() }
        }
    }
    $sweep = Get-FsSweep -Path $Path -WithChangeTime:$WithChangeTime
    $sw.Stop()
    return [ordered]@{
        Path = $Path; Role = $Role; Head = $head; Branch = $branch
        Status = @($entries | Sort-Object); StatusHashes = $hashes; TrackedHashes = $trackedHashes
        Sweep = $sweep; FileCount = $sweep.Files.Count; DirCount = $sweep.Dirs.Count; DurationMs = $sw.ElapsedMilliseconds
    }
}

function Get-RepoSnapshot([string]$AnyCheckout, [hashtable]$Roles, [switch]$WithChangeTime, [switch]$HashAllTracked) {
    $out = [ordered]@{ TakenUtc = [DateTime]::UtcNow.ToString('o'); Checkouts = [System.Collections.Generic.List[object]]::new() }
    foreach ($c in (Get-CheckoutList $AnyCheckout)) {
        $role = if ($Roles.ContainsKey($c.Path)) { $Roles[$c.Path] } elseif ($c.IsMainWorkingTree) { 'MAIN' } else { 'QUIESCENT_SIBLING' }
        $out.Checkouts.Add((Get-CheckoutSnapshot -Path $c.Path -Role $role -WithChangeTime:$WithChangeTime -HashAllTracked:$HashAllTracked))
    }
    return $out
}

# ---------- C1: comparacion salida/entrada y clasificacion ----------
$script:DefaultNoise = @('.vs/', '*.dwl', '*.dwl2', '~$*', '*.swp', '*.tmp')
function Test-NoisePath([string]$Rel, [string[]]$Patterns) {
    foreach ($p in $Patterns) {
        if ($p.EndsWith('/')) { if ($Rel.StartsWith($p, [StringComparison]::OrdinalIgnoreCase)) { return $true } }
        else { $leaf = ($Rel -split '/')[-1]; if ($leaf -like $p) { return $true } }
    }
    return $false
}

function Compare-CheckoutSnapshot($A, $B, [string[]]$NoisePatterns = $script:DefaultNoise) {
    $r = [ordered]@{ Path = $A.Path; Role = $A.Role }
    $r.HeadChanged = ($A.Head -ne $B.Head)
    $r.BranchChanged = ("$($A.Branch)" -ne "$($B.Branch)")
    $sa = [Collections.Generic.HashSet[string]]::new([string[]]@($A.Status)); $sb = [Collections.Generic.HashSet[string]]::new([string[]]@($B.Status))
    $r.StatusAdded = @($B.Status | Where-Object { -not $sa.Contains($_) })
    $r.StatusRemoved = @($A.Status | Where-Object { -not $sb.Contains($_) })
    $r.HashChanged = @($B.StatusHashes.Keys | Where-Object { $A.StatusHashes.Contains($_) -and $A.StatusHashes[$_] -ne $B.StatusHashes[$_] })
    $r.TrackedHashChanged = @()
    if ($A.TrackedHashes -and $B.TrackedHashes) { $r.TrackedHashChanged = @($B.TrackedHashes.Keys | Where-Object { $A.TrackedHashes.Contains($_) -and $A.TrackedHashes[$_] -ne $B.TrackedHashes[$_] }) }
    $fa = $A.Sweep.Files; $fb = $B.Sweep.Files; $da = $A.Sweep.Dirs; $db = $B.Sweep.Dirs
    $newF = @($fb.Keys | Where-Object { -not $fa.ContainsKey($_) })
    $delF = @($fa.Keys | Where-Object { -not $fb.ContainsKey($_) })
    $modF = @($fb.Keys | Where-Object { $fa.ContainsKey($_) -and ($fa[$_].Length -ne $fb[$_].Length -or $fa[$_].MtimeTicks -ne $fb[$_].MtimeTicks) })
    $ctF = @()
    if ($fb.Count -gt 0 -and ($fb.Values | Select-Object -First 1).Contains('ChangeTicks')) {
        $ctF = @($fb.Keys | Where-Object { $fa.ContainsKey($_) -and $fa[$_].Contains('ChangeTicks') -and $fa[$_].ChangeTicks -ne $fb[$_].ChangeTicks })
    }
    $newD = @($db.Keys | Where-Object { -not $da.ContainsKey($_) })
    $delD = @($da.Keys | Where-Object { -not $db.ContainsKey($_) })
    $modD = @($db.Keys | Where-Object { $da.ContainsKey($_) -and $da[$_].MtimeTicks -ne $db[$_].MtimeTicks })
    $statusPaths = [Collections.Generic.HashSet[string]]::new([StringComparer]::OrdinalIgnoreCase)
    foreach ($e in @($A.Status) + @($B.Status)) { if ($e) { $p = $e.Substring(3); if ($p -like '* <- *') { $p = $p.Split(' <- ')[0] }; [void]$statusPaths.Add($p) } }
    # archivos nuevos/modificados/borrados que no explica git status: ignorados o solo-fecha
    $unexplained = @(@($newF) + @($delF) + @($modF) | Where-Object { $_ -and -not $statusPaths.Contains($_) } | Sort-Object -Unique)
    $ignored = @()
    if ($unexplained.Count -gt 0) {
        # rutas como argumentos (no por stdin: la tuberia de PowerShell anade CR al final de cada linea)
        $ignored = @()
        for ($k = 0; $k -lt $unexplained.Count; $k += 100) {
            $chunk = @($unexplained[$k..([Math]::Min($k + 99, $unexplained.Count - 1))])
            $ignored += @(& git -C $B.Path --no-optional-locks -c core.quotePath=false check-ignore --no-index -- @chunk 2>$null | ForEach-Object { $_.Trim().Replace('\', '/') })
        }
    }
    $ignSet = [Collections.Generic.HashSet[string]]::new([string[]]$ignored, [StringComparer]::OrdinalIgnoreCase)
    $touch = @($modF | Where-Object { -not $statusPaths.Contains($_) -and -not $ignSet.Contains($_) -and $fa[$_].Length -eq $fb[$_].Length })
    # directorios con fecha cambiada sin entradas netas nuevas/borradas directamente dentro: creacion+borrado transitorio
    $parentsWithNet = [Collections.Generic.HashSet[string]]::new([StringComparer]::OrdinalIgnoreCase)
    foreach ($p in @($newF) + @($delF) + @($newD) + @($delD)) { if ($p) { $par = if ($p.Contains('/')) { $p.Substring(0, $p.LastIndexOf('/')) } else { '.' }; [void]$parentsWithNet.Add($par) } }
    $churn = @($modD | Where-Object { -not $parentsWithNet.Contains($_) })
    $r.Sweep = [ordered]@{ NewFiles = $newF; DeletedFiles = $delF; ModifiedFiles = $modF; ChangeTimeOnly = @($ctF | Where-Object { $modF -notcontains $_ }); NewDirs = $newD; DeletedDirs = $delD; DirMtimeChanged = $modD }
    $r.IgnoredChanged = $ignored
    $r.TouchOnly = $touch
    $r.DirEntryChurn = $churn
    # ruido declarado: se lista, no se descarta
    $allPaths = @(@($r.StatusAdded | ForEach-Object { $_.Substring(3) }) + $ignored + $touch | Where-Object { $_ })
    $r.NoiseAllowlisted = @($allPaths | Where-Object { Test-NoisePath $_ $NoisePatterns } | Sort-Object -Unique)
    $noiseSet = [Collections.Generic.HashSet[string]]::new([string[]]@($r.NoiseAllowlisted), [StringComparer]::OrdinalIgnoreCase)
    $classes = [System.Collections.Generic.List[string]]::new()
    $contentPaths = @(@($r.StatusAdded | ForEach-Object { $_.Substring(3) }) + @($r.StatusRemoved | ForEach-Object { $_.Substring(3) }) + $r.HashChanged + $r.TrackedHashChanged | Where-Object { $_ -and -not $noiseSet.Contains($_) })
    if ($r.HeadChanged -or $r.BranchChanged -or $contentPaths.Count -gt 0) { $classes.Add('CONTENT_CHANGED') }
    if (@($ignored | Where-Object { -not $noiseSet.Contains($_) }).Count -gt 0) { $classes.Add('IGNORED_CHANGED') }
    if ($churn.Count -gt 0) { $classes.Add('DIR_ENTRY_CHURN') }
    if (@($touch | Where-Object { -not $noiseSet.Contains($_) }).Count -gt 0) { $classes.Add('TOUCH_ONLY') }
    if ($r.NoiseAllowlisted.Count -gt 0) { $classes.Add('NOISE_ALLOWLISTED') }
    if ($ctF.Count -gt 0 -and @($ctF | Where-Object { $modF -notcontains $_ }).Count -gt 0) { $classes.Add('CHANGETIME_ONLY') }
    if ($classes.Count -eq 0) { $classes.Add('UNCHANGED') }
    $r.Classes = @($classes)
    $r.Disposition = Get-C1Disposition -Role $A.Role -Classes $classes
    return $r
}

# Politica de diseno (no implementada en el protocolo): ver c1c3-design.md, C1.
function Get-C1Disposition([string]$Role, $Classes) {
    $stopClasses = @('CONTENT_CHANGED', 'IGNORED_CHANGED', 'DIR_ENTRY_CHURN', 'CHANGETIME_ONLY')
    $hasStop = @($Classes | Where-Object { $stopClasses -contains $_ }).Count -gt 0
    switch ($Role) {
        'UNIT' { return 'NOT_C1 (worktree propio: Scope/CleanTree)' }
        'CONCURRENT_SIBLING' { if ($hasStop) { return 'ATTRIBUTION_REQUIRED' } elseif ($Classes -contains 'TOUCH_ONLY') { return 'SIGNAL' } else { return 'NONE' } }
        default { if ($hasStop) { return 'STOP' } elseif ($Classes -contains 'TOUCH_ONLY') { return 'SIGNAL' } else { return 'NONE' } }
    }
}

function Compare-RepoSnapshot($Exit, $Entry) {
    $res = [System.Collections.Generic.List[object]]::new()
    foreach ($a in $Exit.Checkouts) {
        $b = $Entry.Checkouts | Where-Object { $_.Path -eq $a.Path } | Select-Object -First 1
        if (-not $b) { $res.Add([ordered]@{ Path = $a.Path; Role = $a.Role; Classes = @('CHECKOUT_REMOVED'); Disposition = 'STOP' }); continue }
        $res.Add((Compare-CheckoutSnapshot $a $b))
    }
    foreach ($b in $Entry.Checkouts) {
        if (-not ($Exit.Checkouts | Where-Object { $_.Path -eq $b.Path })) { $res.Add([ordered]@{ Path = $b.Path; Role = $b.Role; Classes = @('CHECKOUT_ADDED'); Disposition = 'STOP' }) }
    }
    $overall = if (@($res | Where-Object { $_.Disposition -eq 'STOP' }).Count -gt 0) { 'STOP' } elseif (@($res | Where-Object { $_.Disposition -eq 'ATTRIBUTION_REQUIRED' }).Count -gt 0) { 'ATTRIBUTION_REQUIRED' } elseif (@($res | Where-Object { $_.Disposition -eq 'SIGNAL' }).Count -gt 0) { 'SIGNAL' } else { 'NONE' }
    return [ordered]@{ Overall = $overall; Checkouts = $res }
}

# Resumen compacto de una fotografia (para los JSON de resultados)
function Get-SnapshotSummary($Snap) {
    return @($Snap.Checkouts | ForEach-Object { [ordered]@{ Path = $_.Path; Role = $_.Role; Head = $_.Head.Substring(0, 12); Branch = $_.Branch; Status = $_.Status; StatusHashes = $_.StatusHashes; FileCount = $_.FileCount; DirCount = $_.DirCount; DurationMs = $_.DurationMs } })
}

# ---------- NTFS ChangeTime (FILE_BASIC_INFO) ----------
$script:NtfsLoaded = $false
function Get-NtfsChangeTimeTicks([string]$Path) {
    if (-not $script:NtfsLoaded) {
        Add-Type -Namespace R62 -Name Ntfs -MemberDefinition @'
[StructLayout(LayoutKind.Sequential)] public struct FILE_BASIC_INFO { public long CreationTime; public long LastAccessTime; public long LastWriteTime; public long ChangeTime; public uint FileAttributes; }
[DllImport("kernel32.dll", SetLastError=true)] public static extern bool GetFileInformationByHandleEx(Microsoft.Win32.SafeHandles.SafeFileHandle h, int cls, out FILE_BASIC_INFO info, uint size);
public static long ChangeTime(string path) {
  using (var fs = new System.IO.FileStream(path, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.ReadWrite | System.IO.FileShare.Delete)) {
    FILE_BASIC_INFO i; if (!GetFileInformationByHandleEx(fs.SafeFileHandle, 0, out i, (uint)System.Runtime.InteropServices.Marshal.SizeOf(typeof(FILE_BASIC_INFO)))) return -1; return i.ChangeTime; }
}
'@
        $script:NtfsLoaded = $true
    }
    return [R62.Ntfs]::ChangeTime($Path)
}

# ---------- procesos hijos con entorno propio ----------
$script:PwshExe = (Get-Process -Id $PID).Path
function Invoke-Child([string]$Exe, [string[]]$ArgList, [string]$Cwd, [hashtable]$EnvOverride = @{}, [int]$TimeoutSec = 60) {
    $psi = [Diagnostics.ProcessStartInfo]::new($Exe)
    foreach ($a in $ArgList) { $psi.ArgumentList.Add($a) }
    $psi.WorkingDirectory = $Cwd
    $psi.UseShellExecute = $false
    $psi.RedirectStandardOutput = $true; $psi.RedirectStandardError = $true; $psi.RedirectStandardInput = $true
    $psi.StandardOutputEncoding = [Text.UTF8Encoding]::new($false); $psi.StandardErrorEncoding = [Text.UTF8Encoding]::new($false)
    foreach ($k in $EnvOverride.Keys) { if ($null -eq $EnvOverride[$k]) { [void]$psi.Environment.Remove($k) } else { $psi.Environment[$k] = $EnvOverride[$k] } }
    $p = [Diagnostics.Process]::Start($psi)
    $p.StandardInput.Close()
    $outT = $p.StandardOutput.ReadToEndAsync(); $errT = $p.StandardError.ReadToEndAsync()
    if (-not $p.WaitForExit($TimeoutSec * 1000)) { try { $p.Kill($true) } catch {}; return [ordered]@{ ExitCode = 'TIMEOUT'; Stdout = ''; Stderr = '' } }
    $p.WaitForExit()
    return [ordered]@{ ExitCode = $p.ExitCode; Stdout = $outT.Result; Stderr = $errT.Result }
}
function Invoke-ChildPwsh([string]$Command, [string]$Cwd, [hashtable]$EnvOverride = @{}) {
    return Invoke-Child -Exe $script:PwshExe -ArgList @('-NoProfile', '-NonInteractive', '-Command', $Command) -Cwd $Cwd -EnvOverride $EnvOverride
}
