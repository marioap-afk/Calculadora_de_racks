param([Parameter(Mandatory)] [string] $Dir)
# Sonda desechable 3: abre cada XLSX en una instancia OCULTA y propia de Excel, solo lectura, nada se guarda.
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
function Repairs() { @(Get-ChildItem -Path $env:TEMP -Filter 'error*.xml' -ErrorAction SilentlyContinue | ForEach-Object FullName) }

$xl = New-Object -ComObject Excel.Application
try {
    Retry { $xl.Visible = $false }
    Retry { $xl.DisplayAlerts = $false }
    Retry { $xl.AutomationSecurity = 3 }
    foreach ($name in 'e0_header_only.xlsx', 'e1_blank_row_pivot.xlsx', 'e2_blank_row_nopivot.xlsx', 'b1_sin_nivel.xlsx', 'v1_valor_estatico.xlsx') {
        "===== $name"
        $errBefore = Repairs
        try {
            $wb = Retry { $xl.Workbooks.Open((Join-Path $Dir $name), 0, $true) }
            "CAPTION = " + (Retry { $wb.Windows.Item(1).Caption })
            $d = Retry { $wb.Worksheets.Item('Por rack') }
            $count = Retry { $d.ListObjects.Count }
            "LISTOBJECTS = $count"
            if ($count -gt 0) { $lo = Retry { $d.ListObjects.Item(1) }; "TABLE = " + (Retry { $lo.Name }) + " " + (Retry { $lo.Range.Address($false, $false) }) }
            $r = Retry { $wb.Worksheets.Item('Resumen') }
            $pts = Retry { $r.PivotTables() }
            $pc = Retry { $pts.Count }
            "PIVOTS = $pc"
            if ($pc -gt 0) {
                $pt = Retry { $pts.Item(1) }
                "SOURCE = " + (Retry { $pt.SourceData }) + "  RANGE = " + (Retry { $pt.TableRange2.Address($false, $false) })
            }
            for ($i = 1; $i -le 14; $i++) {
                $a = Retry { $r.Cells.Item($i, 1).Value2 }; $b = Retry { $r.Cells.Item($i, 2).Value2 }
                if ($null -ne $a -or $null -ne $b) { "   $i | $a | $b" }
            }
            if ($name -eq 'b1_sin_nivel.xlsx') {
                $n = Retry { $pt.PivotFields('Nivel') }
                $items = Retry { $n.PivotItems() }
                $c = Retry { $items.Count }
                "NIVEL_ITEMS = " + (@(1..$c | ForEach-Object { $it = Retry { $items.Item([int]$_) }; "'" + (Retry { $it.Name }) + "'" }) -join ', ')
                "GRAND = " + (Retry { $pt.GetPivotData('Suma de Cantidad total').Value2 }) + " esperado=74"
                $lastItem = $null
                for ($j = 1; $j -le $c; $j++) { $it = Retry { $items.Item([int]$j) }; $nm = Retry { $it.Name }; if ($nm -notin 'Componente', 'Pieza') { $lastItem = $nm } }
                if ($null -ne $lastItem) { Retry { $n.CurrentPage = $lastItem }; "GRAND_ITEM_SIN_NIVEL('$lastItem') = " + (Retry { $pt.GetPivotData('Suma de Cantidad total').Value2 }) + " esperado=10"; Retry { $n.ClearAllFilters() } }
            }
            if ($name -eq 'v1_valor_estatico.xlsx') {
                $lo = Retry { $d.ListObjects.Item(1) }
                $rows = Retry { $lo.ListRows.Count }
                $row = Retry { $lo.ListRows.Item($rows) }
                $cell = Retry { $row.Range.Cells.Item(1, 10) }
                Retry { $cell.Value2 = 100 }
                [void](Retry { $wb.RefreshAll() })
                $pt = Retry { $pts.Item(1) }
                "TRAS_EDITAR_CANTIDAD_TOTAL_Y_RefreshAll = " + (Retry { $pt.GetPivotData('Suma de Cantidad total').Value2 }) + " esperado=134"
            }
            Retry { $wb.Close($false) }
        }
        catch { "ERROR_ABRIR = $($_.Exception.Message)" }
        $newErr = @(Repairs | Where-Object { $errBefore -notcontains $_ })
        "REPAIR_LOGS_NUEVOS = $($newErr.Count)"
        foreach ($f in $newErr) { (Get-Content -LiteralPath $f -Raw) -replace '<[^>]+>', ' ' -replace '\s+', ' ' }
    }
}
finally {
    try { while ($xl.Workbooks.Count -gt 0) { $xl.Workbooks.Item(1).Close($false) } } catch { }
    try { $xl.Quit() } catch { }
    [void][Runtime.InteropServices.Marshal]::ReleaseComObject($xl)
    [GC]::Collect(); [GC]::WaitForPendingFinalizers()
    Start-Sleep -Milliseconds 1500
    foreach ($id in @(Get-Process EXCEL -ErrorAction SilentlyContinue | ForEach-Object Id)) { if ($before -notcontains $id) { Stop-Process -Id $id -Force; "CLEANUP: detenida instancia propia $id" } }
}
