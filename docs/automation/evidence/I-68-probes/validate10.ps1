param([Parameter(Mandatory)] [string] $Dir)
# Sonda desechable 10 (I-68, R-15): observa el MISMO libro sin Actualizar: pasa de 0 items a refrescado? Instancia oculta propia; lectura.
$ErrorActionPreference = 'Stop'
$Dir = (Resolve-Path -LiteralPath $Dir).ProviderPath
$log = New-Object System.Text.StringBuilder
function Say($s) { [void]$log.AppendLine("$s"); Write-Host "$s" }
function Retry([scriptblock]$Action) {
    for ($k = 0; $k -lt 80; $k++) {
        try { return & $Action }
        catch { if ($_.Exception.Message -notmatch '0x80010001|0x8001010A|valores null|null-valued') { throw }; Start-Sleep -Milliseconds 250 }
    }
    throw 'Excel no respondio'
}
function PtOf($wb) { $r = Retry { $wb.Worksheets.Item('Resumen') }; Retry { (Retry { $r.PivotTables() }).Item(1) } }
function Items($pt) { $f = Retry { $pt.PivotFields('Rack') }; $i = Retry { $f.PivotItems() }; Retry { $i.Count } }
function Run($title, [bool]$close) {
    Say "##### $title"
    $before = @(Get-Process EXCEL -ErrorAction SilentlyContinue | ForEach-Object Id)
    $xl = New-Object -ComObject Excel.Application
    $late = 0; $stuck = 0; $first = 0
    try {
        Retry { $xl.Visible = $false }; Retry { $xl.DisplayAlerts = $false }; Retry { $xl.AutomationSecurity = 3 }
        $n = 0
        foreach ($f in 'c1', 'c2', 'c3', 'c4', 'c5', 'c6') {
            $n++
            $sw = [Diagnostics.Stopwatch]::StartNew()
            $wb = Retry { $xl.Workbooks.Open((Join-Path $Dir "$f.xlsx"), 0, $true) }
            $openS = [math]::Round($sw.Elapsed.TotalSeconds, 1)
            for ($w = 0; $w -lt 40; $w++) { $ready = $false; try { $ready = [bool]$xl.Ready } catch { }; if ($ready) { break }; Start-Sleep -Milliseconds 250 }
            Start-Sleep -Milliseconds 2500
            $pt = PtOf $wb
            $c = Items $pt
            $t1 = [math]::Round($sw.Elapsed.TotalSeconds, 1)
            if ($c -gt 0) { $first++; Say "  [$n $f] open=${openS}s primera_lectura t=${t1}s items=$c refrescado" }
            else {
                $done = $false
                while ($sw.Elapsed.TotalSeconds -lt ($t1 + 20)) {
                    Start-Sleep -Milliseconds 1000
                    $c = Items $pt
                    if ($c -gt 0) { $done = $true; break }
                }
                $t2 = [math]::Round($sw.Elapsed.TotalSeconds, 1)
                if ($done) { $late++; Say "  [$n $f] open=${openS}s primera_lectura t=${t1}s items=0 ; SIN Actualizar paso a items=$c en t=${t2}s" }
                else { $stuck++; Say "  [$n $f] open=${openS}s primera_lectura t=${t1}s items=0 ; SIGUE EN 0 tras t=${t2}s" }
            }
            if ($close) { Retry { $wb.Close($false) } }
        }
        Say "  RESUMEN primera_lectura=$first tardio_sin_actualizar=$late sigue_en_0=$stuck"
    }
    finally {
        try { while ($xl.Workbooks.Count -gt 0) { $xl.Workbooks.Item(1).Close($false) } } catch { }
        try { $xl.Quit() } catch { }
        [void][Runtime.InteropServices.Marshal]::ReleaseComObject($xl)
        [GC]::Collect(); [GC]::WaitForPendingFinalizers(); Start-Sleep -Milliseconds 1500
        foreach ($id in @(Get-Process EXCEL -ErrorAction SilentlyContinue | ForEach-Object Id)) { if ($before -notcontains $id) { Stop-Process -Id $id -Force } }
    }
}
Run 'leer y cerrar, corrida 1' $true
Run 'leer y cerrar, corrida 2' $true
Run 'leer y cerrar, corrida 3' $true
Run 'sin cerrar, corrida 1' $false
Run 'sin cerrar, corrida 2' $false
[IO.File]::WriteAllText((Join-Path $Dir 'results-polling.txt'), $log.ToString(), [Text.UTF8Encoding]::new($false))
