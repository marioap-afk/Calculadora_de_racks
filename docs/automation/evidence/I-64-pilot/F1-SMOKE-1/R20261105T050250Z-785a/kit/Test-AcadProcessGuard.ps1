#Requires -Version 7.0
<#
  I-64 Smoke-1: guarda de procesos ligada a AUTHORIZED_AUTOCAD_PID (BORRADOR propuesto tras el STOP de R20261104T194513Z-a578; entra en
  el kit de una ocurrencia solo si el Coordinator la autoriza). La ejecuta la SESION, nunca AutoCAD. SOLO LECTURA: lee la lista de
  procesos y el ejecutable; escribe solo en ProcessDir (registro de la PID, bitacora y marcas).

  Modos:
    Register  justo despues de que el Owner abre AutoCAD: exige exactamente 1 proceso; registra PID, ruta, SHA-256 y hora de inicio en
              authorized-autocad-pid.json. No se repite.
    Check     en cada punto de control (-Checkpoint): exige exactamente 1 proceso, con la misma PID, hora de inicio y ruta. Con
              -Checkpoint PRE-CLOSE, si pasa, escribe pre-close.ok (el Owner ya puede cerrar AutoCAD).
    Closed    tras el cierre normal: exige 0 procesos y que la PID autorizada no exista; si pasa, escribe closed.ok.
    Watch     vigilancia continua desde Register hasta la POST: sondea cada -IntervalMs y anota cada cambio en acad-watch.log.
              Sale con 1 si aparece otra PID (o cualquier proceso tras el cierre), con 2 si la PID autorizada desaparece sin pre-close.ok
              y con 0 cuando existe watch-stop.flag (lo crea la sesion tras la POST).
  Codigos: 0 PASS; 1 STOP; 2 STOP (salida inesperada de la PID autorizada); 3 entrada invalida.
#>
param(
    [Parameter(Mandatory = $true)][ValidateSet('Register', 'Check', 'Closed', 'Watch')][string]$Mode,
    [Parameter(Mandatory = $true)][string]$ProcessDir,
    [string]$Checkpoint = '',
    [string]$ProcessName = 'acad',
    [string]$ExpectedExeSha256 = '2A75996FD2A5C5EE0376FD5BA7CCED227A098193FF495E8CEEDA3A96FE326422',
    [int]$IntervalMs = 1000
)
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
if (-not (Test-Path -LiteralPath $ProcessDir -PathType Container)) { Write-Output "INVALID: no existe ProcessDir"; exit 3 }
$reg = Join-Path $ProcessDir 'authorized-autocad-pid.json'
$log = Join-Path $ProcessDir 'guard-log.txt'
$watchLog = Join-Path $ProcessDir 'acad-watch.log'
$Utf8 = [System.Text.UTF8Encoding]::new($false)
function Now { [DateTime]::UtcNow.ToString('o') }
function Append([string]$path, [string]$t) { [System.IO.File]::AppendAllText($path, $t + "`n", $Utf8) }
function Say([string]$t) { Append $log $t; Write-Output $t }
function Procs { @(Get-Process -Name $ProcessName -ErrorAction SilentlyContinue) }
function StartUtc($p) { $p.StartTime.ToUniversalTime().ToString('o') }
function StartTicks($p) { [long]$p.StartTime.ToUniversalTime().Ticks }
# La identidad de la instancia se compara por PID + ticks UTC de inicio + ruta. ConvertFrom-Json convierte las fechas ISO en DateTime,
# asi que nunca se compara la cadena StartUtc del registro.
function SameInstance($p, $a) { $p.Id -eq [int]$a.Pid -and (StartTicks $p) -eq [long]$a.StartUtcTicks -and [string]$p.Path -eq [string]$a.ExecutablePath }
function Describe($ps) { ($ps | ForEach-Object { '{0}@{1}' -f $_.Id, (StartUtc $_) }) -join ',' }
function LoadReg {
    if (-not (Test-Path -LiteralPath $reg)) { Write-Output "INVALID: no hay AUTHORIZED_AUTOCAD_PID registrado"; exit 3 }
    Get-Content -LiteralPath $reg -Raw -Encoding UTF8 | ConvertFrom-Json
}

switch ($Mode) {
    'Register' {
        if (Test-Path -LiteralPath $reg) { Say "$(Now) REGISTER STOP: ya existe un AUTHORIZED_AUTOCAD_PID (no se vuelve a registrar)"; exit 1 }
        $ps = @(Procs)
        if ($ps.Count -ne 1) { Say "$(Now) REGISTER STOP: $ProcessName count = $($ps.Count) (debe ser 1) [$(Describe $ps)]"; exit 1 }
        $p = $ps[0]; $path = $p.Path
        $sha = (Get-FileHash -LiteralPath $path -Algorithm SHA256).Hash
        if ($sha -ne $ExpectedExeSha256) { Say "$(Now) REGISTER STOP: SHA-256 del ejecutable $sha distinto del esperado"; exit 1 }
        $o = [ordered]@{ Name = 'AUTHORIZED_AUTOCAD_PID'; ProcessName = $ProcessName; Pid = $p.Id; ExecutablePath = $path
                         ExeSha256 = $sha; StartUtc = (StartUtc $p); StartUtcTicks = (StartTicks $p); RegisteredUtc = (Now) }
        [System.IO.File]::WriteAllText($reg, ($o | ConvertTo-Json) + "`n", $Utf8)
        $h = (Get-FileHash -LiteralPath $reg -Algorithm SHA256).Hash
        [System.IO.File]::WriteAllText("$reg.sha256", "$h  authorized-autocad-pid.json`n", $Utf8)
        Say "$(Now) REGISTER PASS: AUTHORIZED_AUTOCAD_PID = $($p.Id); inicio $($o.StartUtc); exe sha256 $sha"
        exit 0
    }
    'Check' {
        if ([string]::IsNullOrWhiteSpace($Checkpoint)) { Write-Output "INVALID: falta -Checkpoint"; exit 3 }
        $a = LoadReg; $ps = @(Procs); $why = [System.Collections.Generic.List[string]]::new()
        if ($ps.Count -ne 1) { $why.Add("$ProcessName count = $($ps.Count) (debe ser 1)") }
        if (-not ($ps | Where-Object { $_.Id -eq $a.Pid })) { $why.Add("la PID autorizada $($a.Pid) ya no existe") }
        foreach ($p in $ps) {
            if ($p.Id -ne $a.Pid) { $why.Add("otra PID: $($p.Id)") }
            elseif (-not (SameInstance $p $a)) { $why.Add("la PID $($p.Id) no es la misma instancia (inicio o ruta distintos)") }
        }
        if ($why.Count) { Say "$(Now) CHECK $Checkpoint STOP: $($why -join '; ') [$(Describe $ps)]"; exit 1 }
        if ($Checkpoint -eq 'PRE-CLOSE') { [System.IO.File]::WriteAllText((Join-Path $ProcessDir 'pre-close.ok'), "$(Now)`n", $Utf8) }
        Say "$(Now) CHECK $Checkpoint PASS: unica $ProcessName = AUTHORIZED_AUTOCAD_PID $($a.Pid)"
        exit 0
    }
    'Closed' {
        $a = LoadReg; $ps = @(Procs)
        if ($ps.Count -ne 0) { Say "$(Now) CLOSED STOP: $ProcessName count = $($ps.Count) (debe ser 0) [$(Describe $ps)]"; exit 1 }
        [System.IO.File]::WriteAllText((Join-Path $ProcessDir 'closed.ok'), "$(Now)`n", $Utf8)
        Say "$(Now) CLOSED PASS: $ProcessName count = 0; la PID autorizada $($a.Pid) ya no existe"
        exit 0
    }
    'Watch' {
        $a = LoadReg; $seen = '<inicio>'; $authGone = $false
        Append $watchLog "$(Now) WATCH inicio: AUTHORIZED_AUTOCAD_PID $($a.Pid) (ticks de inicio $($a.StartUtcTicks)); intervalo $IntervalMs ms"
        while ($true) {
            if (Test-Path -LiteralPath (Join-Path $ProcessDir 'watch-stop.flag')) { Append $watchLog "$(Now) WATCH fin por watch-stop.flag"; exit 0 }
            $ps = @(Procs); $d = Describe $ps
            if ($d -ne $seen) { Append $watchLog "$(Now) procesos $ProcessName = [$d]"; $seen = $d }
            foreach ($p in $ps) {
                if (-not (SameInstance $p $a)) { Append $watchLog "$(Now) WATCH STOP: otra instancia $($p.Id)@$(StartUtc $p)"; exit 1 }
            }
            if (-not $authGone -and -not ($ps | Where-Object { $_.Id -eq $a.Pid })) {
                $authGone = $true
                if (-not (Test-Path -LiteralPath (Join-Path $ProcessDir 'pre-close.ok'))) { Append $watchLog "$(Now) WATCH STOP: la PID autorizada salio sin PRE-CLOSE"; exit 2 }
                Append $watchLog "$(Now) WATCH: la PID autorizada salio tras PRE-CLOSE (cierre normal); sigue la vigilancia hasta la POST"
            }
            Start-Sleep -Milliseconds $IntervalMs
        }
    }
}
