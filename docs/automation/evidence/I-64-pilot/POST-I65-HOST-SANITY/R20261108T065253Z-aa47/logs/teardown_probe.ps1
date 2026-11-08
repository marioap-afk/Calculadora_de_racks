#Requires -Version 7.0
# Sonda inocua de la causa probable de la linea WATCH de aa47: lanza procesos ping de vida corta y los sondea como la guarda
# (Get-Process por nombre: Id, StartTime, Path). Cuenta las muestras en las que el proceso aun da Id y StartTime pero Path ya no
# coincide con la ruta leida al principio (vacia o ilegible), es decir, lo que la guarda leeria como «otra instancia».
$ErrorActionPreference = 'SilentlyContinue'
$runs = 25; $mismatch = 0; $samples = 0; $examples = @()
for ($i = 0; $i -lt $runs; $i++) {
    $p0 = Start-Process -FilePath "$env:SystemRoot\System32\PING.EXE" -ArgumentList '-n', '2', '127.0.0.1' -WindowStyle Hidden -PassThru
    Start-Sleep -Milliseconds 300
    $path0 = (Get-Process -Id $p0.Id).Path
    while ($true) {
        $ps = @(Get-Process -Name PING | Where-Object { $_.Id -eq $p0.Id })
        if ($ps.Count -eq 0) { break }
        $p = $ps[0]; $samples++
        $st = try { $p.StartTime.ToUniversalTime().Ticks } catch { 0 }
        if ($st -ne 0 -and [string]$p.Path -ne [string]$path0) { $mismatch++; if ($examples.Count -lt 3) { $examples += "pid $($p.Id) start ticks $st path='$([string]$p.Path)' (esperada '$path0')" } }
    }
}
"ejecuciones $runs; muestras $samples; muestras con Id e inicio legibles pero ruta distinta de la inicial = $mismatch"
$examples
