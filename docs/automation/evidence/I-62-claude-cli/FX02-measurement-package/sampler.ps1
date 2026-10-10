# I-62 / FX-02 - medicion unica de claude-cli: fuente CIM del arbol de procesos (operaciones 7 y 8).
# Get-CimInstance Win32_Process (ProcessId, ParentProcessId, Name, ExecutablePath, CreationDate, CommandLine).
# La configuracion llega por stdin (una linea JSON), nunca por la linea de ordenes: asi ni el session id ni la ruta del clon aparecen en la
# linea de ordenes de este proceso (no se autodetecta como participante).
#   Mode Loop: cada PeriodMs, hasta StopFile o MaxSeconds; arbol del PID de PidFile por identidad (PID + CreationDate; hijo >= padre);
#              escribe una linea JSON por muestra en OutFile: miembros del arbol presentes, procesos de la lista cerrada creados en la
#              ventana y fuera del arbol, y procesos ajenos al arbol cuya linea de ordenes contiene la ruta del clon.
#   Mode Once: una instantanea: procesos con el session id en la linea de ordenes, procesos con la ruta del clon, identidades del arbol vivas
#              y procesos de la lista cerrada creados en la ventana con su padre (para la regla de huerfanos).
# Compatible con Windows PowerShell 5.1 y PowerShell 7. Nunca ejecuta claude. Solo lectura.
$ErrorActionPreference = 'Stop'
$cfg = [Console]::In.ReadLine() | ConvertFrom-Json
$utf8 = New-Object System.Text.UTF8Encoding($false)
$sha = [System.Security.Cryptography.SHA256]::Create()
$closed = @($cfg.Closed | ForEach-Object { $_.ToLowerInvariant() })
$tol = 20000  # 2 ms en ticks de 100 ns

function Iso($d) { if ($null -eq $d) { return $null }; return $d.ToUniversalTime().ToString('o') }
function Ticks($d) { if ($null -eq $d) { return $null }; return $d.ToUniversalTime().Ticks }
function ParseUtc($s) {
    # PowerShell 7 convierte en DateTime las cadenas ISO al leer el JSON; 5.1 las deja como texto: se aceptan ambas formas.
    if ($null -eq $s -or ($s -is [string] -and $s -eq '')) { return $null }
    if ($s -is [DateTime]) { return $s.ToUniversalTime().Ticks }
    return ([DateTime]::Parse([string]$s, [Globalization.CultureInfo]::InvariantCulture, [Globalization.DateTimeStyles]::RoundtripKind)).ToUniversalTime().Ticks
}
function Hex($s) { if ($null -eq $s) { return $null }; return (($sha.ComputeHash($utf8.GetBytes($s)) | ForEach-Object { $_.ToString('x2') }) -join '') }
function HasClone($cmd) {
    if (-not $cfg.ClonePath -or -not $cmd) { return $false }
    $c = $cmd.ToLowerInvariant().Replace('\', '/')
    $n = ([string]$cfg.ClonePath).ToLowerInvariant().Replace('\', '/').TrimEnd('/')
    return $c.Contains($n)
}
function Snap() {
    return @(Get-CimInstance -ClassName Win32_Process -Property ProcessId, ParentProcessId, Name, ExecutablePath, CreationDate, CommandLine)
}

if ($cfg.Mode -eq 'Once') {
    $procs = Snap
    $exclude = @($PID) + @($cfg.ExcludePids)
    $byPid = @{}
    foreach ($p in $procs) { $byPid[[int]$p.ProcessId] = $p }
    $sid = [string]$cfg.Sid
    $sidProcs = @(); $cloneRefs = @(); $aliveTree = @(); $closedInWindow = @()
    $ws = ParseUtc $cfg.WindowStartUtc; $we = ParseUtc $cfg.WindowEndUtc
    foreach ($p in $procs) {
        $pidv = [int]$p.ProcessId
        if ($exclude -contains $pidv) { continue }
        $cmd = [string]$p.CommandLine
        if ($sid -and $cmd -and $cmd.Contains($sid)) { $sidProcs += [pscustomobject]@{ Pid = $pidv; Name = $p.Name } }
        if (HasClone $cmd) { $cloneRefs += [pscustomobject]@{ Pid = $pidv; Name = $p.Name } }
        $ct = Ticks $p.CreationDate
        if ($closed -contains ([string]$p.Name).ToLowerInvariant() -and $null -ne $ws -and $null -ne $ct -and $ct -ge $ws -and ($null -eq $we -or $ct -le $we)) {
            $par = $byPid[[int]$p.ParentProcessId]
            $closedInWindow += [pscustomobject]@{ Pid = $pidv; Ppid = [int]$p.ParentProcessId; Name = $p.Name; CreationUtc = (Iso $p.CreationDate);
                ParentExists = ($null -ne $par); ParentCreationUtc = $(if ($par) { Iso $par.CreationDate } else { $null }) }
        }
    }
    foreach ($t in @($cfg.TreeIdentities)) {
        if ($null -eq $t) { continue }
        $p = $byPid[[int]$t.Pid]
        if ($p) {
            $d = [Math]::Abs((Ticks $p.CreationDate) - (ParseUtc $t.CreationUtc))
            if ($d -le $tol) { $aliveTree += [pscustomobject]@{ Pid = [int]$t.Pid; Name = $p.Name; CreationUtc = (Iso $p.CreationDate) } }
        }
    }
    $out = [pscustomobject]@{ Mode = 'Once'; Utc = (Get-Date).ToUniversalTime().ToString('o'); Total = $procs.Count; SelfPid = $PID;
        SidProcs = $sidProcs; CloneRefs = $cloneRefs; AliveTree = $aliveTree; ClosedInWindow = $closedInWindow }
    [IO.File]::WriteAllText([string]$cfg.OutFile, ($out | ConvertTo-Json -Compress -Depth 6), $utf8)
    exit 0
}

# ---------------------------------------------------------------- Mode Loop
$tree = @{}      # clave "pid|ticks" -> @{Pid; Ticks; ...}
$rootKnown = $false
$seq = 0
$ws = ParseUtc $cfg.WindowStartUtc
$deadline = (Get-Date).AddSeconds([int]$cfg.MaxSeconds)
while ($true) {
    if (Test-Path -LiteralPath ([string]$cfg.StopFile)) { break }
    if ((Get-Date) -gt $deadline) { break }
    $sw = [Diagnostics.Stopwatch]::StartNew()
    $seq++
    try {
        if (-not $rootKnown -and (Test-Path -LiteralPath ([string]$cfg.PidFile))) {
            $pf = Get-Content -Raw -LiteralPath ([string]$cfg.PidFile) | ConvertFrom-Json
            $tree[('{0}|{1}' -f [int]$pf.Pid, (ParseUtc $pf.CreationUtc))] = @{ Pid = [int]$pf.Pid; Ticks = (ParseUtc $pf.CreationUtc); Root = $true }
            $rootKnown = $true
        }
        $procs = Snap
        $utc = (Get-Date).ToUniversalTime().ToString('o')
        $members = @{}
        $changed = $true
        while ($changed) {
            $changed = $false
            foreach ($p in $procs) {
                $pidv = [int]$p.ProcessId; $ct = Ticks $p.CreationDate
                if ($null -eq $ct) { continue }
                $k = $null
                foreach ($key in @($tree.Keys)) {
                    $m = $tree[$key]
                    if ($m.Pid -eq $pidv -and [Math]::Abs($m.Ticks - $ct) -le $tol) { $k = $key; break }
                }
                if ($k) { if (-not $members.ContainsKey($k)) { $members[$k] = $p; $changed = $true }; continue }
                foreach ($key in @($tree.Keys)) {
                    $m = $tree[$key]
                    if ($m.Pid -eq [int]$p.ParentProcessId -and $m.Ticks -le $ct) {
                        $nk = '{0}|{1}' -f $pidv, $ct
                        $tree[$nk] = @{ Pid = $pidv; Ticks = $ct; Root = $false }
                        $members[$nk] = $p; $changed = $true; break
                    }
                }
            }
        }
        $rows = @()
        foreach ($k in $members.Keys) {
            $p = $members[$k]; $cmd = [string]$p.CommandLine
            $rows += [pscustomobject]@{ Pid = [int]$p.ProcessId; Ppid = [int]$p.ParentProcessId; Name = $p.Name; Path = $p.ExecutablePath;
                CreationUtc = (Iso $p.CreationDate); Root = [bool]$tree[$k].Root; CmdLen = $cmd.Length; CmdSha256 = (Hex $cmd);
                CmdHead = $(if ($cmd.Length -gt 300) { $cmd.Substring(0, 300) } else { $cmd }) }
        }
        $treePids = @{}
        foreach ($k in $members.Keys) { $treePids[[int]$members[$k].ProcessId] = $true }
        $closedNew = @(); $cloneRefs = @()
        foreach ($p in $procs) {
            $pidv = [int]$p.ProcessId
            if ($treePids.ContainsKey($pidv) -or $pidv -eq $PID) { continue }
            $ct = Ticks $p.CreationDate
            if ($closed -contains ([string]$p.Name).ToLowerInvariant() -and $null -ne $ws -and $null -ne $ct -and $ct -ge $ws) {
                $closedNew += [pscustomobject]@{ Pid = $pidv; Ppid = [int]$p.ParentProcessId; Name = $p.Name; CreationUtc = (Iso $p.CreationDate) }
            }
            if (HasClone ([string]$p.CommandLine)) { $cloneRefs += [pscustomobject]@{ Pid = $pidv; Name = $p.Name } }
        }
        $line = [pscustomobject]@{ Seq = $seq; Utc = $utc; DurMs = $sw.ElapsedMilliseconds; Total = $procs.Count; RootKnown = $rootKnown;
            Tree = $rows; ClosedNew = $closedNew; CloneRefs = $cloneRefs } | ConvertTo-Json -Compress -Depth 6
    } catch {
        $line = [pscustomobject]@{ Seq = $seq; Utc = (Get-Date).ToUniversalTime().ToString('o'); Error = [string]$_.Exception.Message } | ConvertTo-Json -Compress
    }
    [IO.File]::AppendAllText([string]$cfg.OutFile, $line + "`n", $utf8)
    $left = [int]$cfg.PeriodMs - $sw.ElapsedMilliseconds
    if ($left -gt 0) { Start-Sleep -Milliseconds $left }
}
exit 0
