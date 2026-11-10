param([Parameter(Mandatory)] [string] $Dir)
# Sonda desechable 6: pivote minimo en instancia limpia. t7 = celdas previas sobrantes; t6 = Vista protegida. Solo lectura.
$ErrorActionPreference = 'Stop'
$Dir = (Resolve-Path -LiteralPath $Dir).ProviderPath
function Retry([scriptblock]$Action) {
    for ($k = 0; $k -lt 60; $k++) {
        try { return & $Action }
        catch { if ($_.Exception.Message -notmatch '0x80010001|0x8001010A|valores null|null-valued') { throw }; Start-Sleep -Milliseconds 500 }
    }
    throw 'Excel no respondio en 30 s'
}
$before = @(Get-Process EXCEL -ErrorAction SilentlyContinue | ForEach-Object Id)
$o = New-Object System.Text.StringBuilder
$xl = New-Object -ComObject Excel.Application
try {
    Retry { $xl.Visible = $false }; Retry { $xl.DisplayAlerts = $false }; Retry { $xl.AutomationSecurity = 3 }
    $wb = Retry { $xl.Workbooks.Open((Join-Path $Dir 't7_min_celdas_sobrantes.xlsx'), 0, $true) }
    for ($w = 0; $w -lt 40; $w++) { $ready = $false; try { $ready = [bool]$xl.Ready } catch { }; if ($ready) { break }; Start-Sleep -Milliseconds 250 }
    Start-Sleep -Milliseconds 2500
    $r = Retry { $wb.Worksheets.Item('Resumen') }
    $pt = Retry { (Retry { $r.PivotTables() }).Item(1) }
    [void]$o.AppendLine("T7 rango=" + (Retry { $pt.TableRange2.Address($false, $false) }) + " grand=" + (Retry { $pt.GetPivotData('Suma de Cantidad total').Value2 }) + " used=" + (Retry { $r.UsedRange.Address($false, $false) }))
    for ($i = 5; $i -le 23; $i++) {
        $a = Retry { $r.Cells.Item($i, 1).Value2 }; $b = Retry { $r.Cells.Item($i, 2).Value2 }
        if ($null -ne $a -or $null -ne $b) { [void]$o.AppendLine("  $i | $a | $b") }
    }
    Retry { $wb.Close($false) }
    $pv = Retry { $xl.ProtectedViewWindows.Open((Join-Path $Dir 't6_min_casos_rotos.xlsx')) }
    $s = Retry { $pv.Workbook.Worksheets.Item('Resumen') }
    [void]$o.AppendLine("T6_PV used=" + (Retry { $s.UsedRange.Address($false, $false) }) + " A5='" + (Retry { $s.Range('A5').Value2 }) + "' B19=" + (Retry { $s.Range('B19').Value2 }))
    [void](Retry { $pv.Close() })
}
finally {
    try { while ($xl.Workbooks.Count -gt 0) { $xl.Workbooks.Item(1).Close($false) } } catch { }
    try { $xl.Quit() } catch { }
    [void][Runtime.InteropServices.Marshal]::ReleaseComObject($xl)
    [GC]::Collect(); [GC]::WaitForPendingFinalizers(); Start-Sleep -Milliseconds 1500
    foreach ($id in @(Get-Process EXCEL -ErrorAction SilentlyContinue | ForEach-Object Id)) { if ($before -notcontains $id) { Stop-Process -Id $id -Force } }
    [IO.File]::WriteAllText((Join-Path $Dir 'p6_result.txt'), $o.ToString(), [Text.UTF8Encoding]::new($false))
}
