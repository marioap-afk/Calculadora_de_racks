param([Parameter(Mandatory)] [string] $Dir)
# Sonda desechable 8 (I-68): estado inicial de Nivel y nota visible. Instancia limpia por archivo. Solo lectura.
$ErrorActionPreference = 'Stop'
$Dir = (Resolve-Path -LiteralPath $Dir).ProviderPath
$log = New-Object System.Text.StringBuilder
function Say($s) { [void]$log.AppendLine("$s"); "$s" }
function Retry([scriptblock]$Action) {
    for ($k = 0; $k -lt 80; $k++) {
        try { return & $Action }
        catch { if ($_.Exception.Message -notmatch '0x80010001|0x8001010A|valores null|null-valued') { throw }; Start-Sleep -Milliseconds 500 }
    }
    throw 'Excel no respondio en 40 s'
}
foreach ($name in 'n1_todas_con_nota.xlsx', 'n2_pieza_inicial.xlsx', 'n3_pieza_inicial_vacio.xlsx') {
    Say "===== $name"
    $errBefore = @(Get-ChildItem -Path $env:TEMP -Filter 'error*.xml' -ErrorAction SilentlyContinue | ForEach-Object FullName)
    $before = @(Get-Process EXCEL -ErrorAction SilentlyContinue | ForEach-Object Id)
    $xl = New-Object -ComObject Excel.Application
    try {
        Retry { $xl.Visible = $false }; Retry { $xl.DisplayAlerts = $false }; Retry { $xl.AutomationSecurity = 3 }
        $wb = Retry { $xl.Workbooks.Open((Join-Path $Dir $name), 0, $true) }
        for ($w = 0; $w -lt 40; $w++) { $ready = $false; try { $ready = [bool]$xl.Ready } catch { }; if ($ready) { break }; Start-Sleep -Milliseconds 250 }
        Start-Sleep -Milliseconds 2500
        Say ("CAPTION = " + (Retry { $wb.Windows.Item(1).Caption }))
        $r = Retry { $wb.Worksheets.Item('Resumen') }
        $pt = Retry { (Retry { $r.PivotTables() }).Item(1) }
        $n = Retry { $pt.PivotFields('Nivel') }
        Say ("FILTRO_NIVEL = " + (Retry { $n.CurrentPage.Name }) + " rango=" + (Retry { $pt.TableRange2.Address($false, $false) }) + " RefreshDate=" + (Retry { $pt.RefreshDate }))
        Say ("NOTA_D1 = " + (Retry { $r.Range('D1').Value2 }))
        for ($i = 1; $i -le 12; $i++) { $a = Retry { $r.Cells.Item($i, 1).Value2 }; $b = Retry { $r.Cells.Item($i, 2).Value2 }; if ($null -ne $a -or $null -ne $b) { Say "   $i | $a | $b" } }
        Retry { $wb.Close($false) }
    }
    catch { Say "ERROR = $($_.Exception.Message)" }
    finally {
        try { while ($xl.Workbooks.Count -gt 0) { $xl.Workbooks.Item(1).Close($false) } } catch { }
        try { $xl.Quit() } catch { }
        [void][Runtime.InteropServices.Marshal]::ReleaseComObject($xl)
        [GC]::Collect(); [GC]::WaitForPendingFinalizers(); Start-Sleep -Milliseconds 1500
        foreach ($id in @(Get-Process EXCEL -ErrorAction SilentlyContinue | ForEach-Object Id)) { if ($before -notcontains $id) { Stop-Process -Id $id -Force } }
    }
    Say ("REPAIR_LOGS_NUEVOS = " + @(Get-ChildItem -Path $env:TEMP -Filter 'error*.xml' -ErrorAction SilentlyContinue | Where-Object { $errBefore -notcontains $_.FullName }).Count)
}
[IO.File]::WriteAllText((Join-Path $Dir 'results.txt'), $log.ToString(), [Text.UTF8Encoding]::new($false))
