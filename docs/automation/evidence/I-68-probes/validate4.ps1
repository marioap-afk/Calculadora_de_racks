param([Parameter(Mandatory)] [string] $Dir)
# Sonda desechable 4: abre cada archivo t*.xlsx y vuelca la tabla dinamica tras el refresco de apertura. Solo lectura.
$ErrorActionPreference = 'Stop'
$Dir = (Resolve-Path -LiteralPath $Dir).ProviderPath
$before = @(Get-Process EXCEL -ErrorAction SilentlyContinue | ForEach-Object Id)
function Retry([scriptblock]$Action) {
    for ($k = 0; $k -lt 60; $k++) {
        try { return & $Action }
        catch { if ($_.Exception.Message -notmatch '0x80010001|0x8001010A|valores null|null-valued') { throw }; Start-Sleep -Milliseconds 500 }
    }
    throw 'Excel no respondio en 30 s'
}
$o = New-Object System.Text.StringBuilder
$xl = New-Object -ComObject Excel.Application
try {
    Retry { $xl.Visible = $false }; Retry { $xl.DisplayAlerts = $false }; Retry { $xl.AutomationSecurity = 3 }
    foreach ($f in Get-ChildItem -LiteralPath $Dir -Filter 't*.xlsx' | Sort-Object Name) {
        [void]$o.AppendLine("===== $($f.Name)")
        try {
            $wb = Retry { $xl.Workbooks.Open($f.FullName, 0, $true) }
            [void]$o.AppendLine("CAPTION = " + (Retry { $wb.Windows.Item(1).Caption }))
            for ($w = 0; $w -lt 40; $w++) { $ready = $false; try { $ready = [bool]$xl.Ready } catch { }; if ($ready) { break }; Start-Sleep -Milliseconds 250 }
            Start-Sleep -Milliseconds 2500
            $r = Retry { $wb.Worksheets.Item('Resumen') }
            $pts = Retry { $r.PivotTables() }
            if ((Retry { $pts.Count }) -gt 0) { $pt = Retry { $pts.Item(1) }; [void]$o.AppendLine("PIVOT fuente=" + (Retry { $pt.SourceData }) + " rango=" + (Retry { $pt.TableRange2.Address($false, $false) }) + " grand=" + (Retry { $pt.GetPivotData('Suma de Cantidad total').Value2 })) }
            for ($i = 1; $i -le 30; $i++) {
                $a = Retry { $r.Cells.Item($i, 1).Value2 }; $b = Retry { $r.Cells.Item($i, 2).Value2 }
                if ($null -ne $a -or $null -ne $b) { [void]$o.AppendLine("  $i | $a | $b") }
            }
            Retry { $wb.Close($false) }
        }
        catch { [void]$o.AppendLine("ERROR_ABRIR = $($_.Exception.Message)") }
    }
}
finally {
    try { while ($xl.Workbooks.Count -gt 0) { $xl.Workbooks.Item(1).Close($false) } } catch { }
    try { $xl.Quit() } catch { }
    [void][Runtime.InteropServices.Marshal]::ReleaseComObject($xl)
    [GC]::Collect(); [GC]::WaitForPendingFinalizers(); Start-Sleep -Milliseconds 1500
    foreach ($id in @(Get-Process EXCEL -ErrorAction SilentlyContinue | ForEach-Object Id)) { if ($before -notcontains $id) { Stop-Process -Id $id -Force } }
    [IO.File]::WriteAllText((Join-Path $Dir 'p4_result.txt'), $o.ToString(), [Text.UTF8Encoding]::new($false))
}
