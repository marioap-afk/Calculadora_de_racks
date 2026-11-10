param([Parameter(Mandatory)] [string] $Dir)
# Sonda desechable 5: cuando se dispara el refresco al abrir. Cada escenario usa su propia instancia OCULTA de Excel. Solo lectura.
$ErrorActionPreference = 'Stop'
$Dir = (Resolve-Path -LiteralPath $Dir).ProviderPath
$a = Join-Path $Dir 't6_min_casos_rotos.xlsx'
$b = Join-Path $Dir 't8_min_longitudes.xlsx'
function Retry([scriptblock]$Action) {
    for ($k = 0; $k -lt 60; $k++) {
        try { return & $Action }
        catch { if ($_.Exception.Message -notmatch '0x80010001|0x8001010A|valores null|null-valued') { throw }; Start-Sleep -Milliseconds 500 }
    }
    throw 'Excel no respondio en 30 s'
}
function Settle($xl) { for ($w = 0; $w -lt 40; $w++) { $ready = $false; try { $ready = [bool]$xl.Ready } catch { }; if ($ready) { break }; Start-Sleep -Milliseconds 250 }; Start-Sleep -Milliseconds 2500 }
function Report($label, $xl, $wb) {
    Settle $xl
    $r = Retry { $wb.Worksheets.Item('Resumen') }
    $pt = Retry { (Retry { $r.PivotTables() }).Item(1) }
    $rack = Retry { $pt.PivotFields('Rack') }
    $itemsCount = Retry { (Retry { $rack.PivotItems() }).Count }
    $total = Retry { $r.Range('B10').Value2 }
    "$label refreshed=$((Retry { $pt.RefreshDate })) items_Rack=$itemsCount grand=$(Retry { $pt.GetPivotData('Suma de Cantidad total').Value2}) B10=$total"
}
function Scenario($name, [scriptblock]$Body) {
    $before = @(Get-Process EXCEL -ErrorAction SilentlyContinue | ForEach-Object Id)
    $xl = New-Object -ComObject Excel.Application
    try {
        Retry { $xl.Visible = $false }; Retry { $xl.DisplayAlerts = $false }; Retry { $xl.AutomationSecurity = 3 }
        & $Body $xl $name
    }
    catch { "$name ERROR = $($_.Exception.Message)" }
    finally {
        try { while ($xl.Workbooks.Count -gt 0) { $xl.Workbooks.Item(1).Close($false) } } catch { }
        try { $xl.Quit() } catch { }
        [void][Runtime.InteropServices.Marshal]::ReleaseComObject($xl)
        [GC]::Collect(); [GC]::WaitForPendingFinalizers(); Start-Sleep -Milliseconds 1500
        foreach ($id in @(Get-Process EXCEL -ErrorAction SilentlyContinue | ForEach-Object Id)) { if ($before -notcontains $id) { Stop-Process -Id $id -Force } }
    }
}

Scenario 'A_instancia_nueva' { param($xl, $n) $wb = Retry { $xl.Workbooks.Open($b, 0, $true) }; Report $n $xl $wb }
Scenario 'B_con_libro_en_blanco_abierto' { param($xl, $n) [void](Retry { $xl.Workbooks.Add() }); $wb = Retry { $xl.Workbooks.Open($b, 0, $true) }; Report $n $xl $wb }
Scenario 'C_despues_de_cerrar_otro' { param($xl, $n) $w1 = Retry { $xl.Workbooks.Open($a, 0, $true) }; Settle $xl; Retry { $w1.Close($false) }; $wb = Retry { $xl.Workbooks.Open($b, 0, $true) }; Report $n $xl $wb }
Scenario 'D_con_otro_pivote_abierto' { param($xl, $n) $w1 = Retry { $xl.Workbooks.Open($a, 0, $true) }; Settle $xl; $wb = Retry { $xl.Workbooks.Open($b, 0, $true) }; Report $n $xl $wb }
Scenario 'E_lectura_escritura' { param($xl, $n) $wb = Retry { $xl.Workbooks.Open($b, 0, $false) }; Report $n $xl $wb }
