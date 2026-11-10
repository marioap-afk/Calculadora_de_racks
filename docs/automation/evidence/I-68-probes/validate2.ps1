param(
    [Parameter(Mandatory)] [string] $Path,
    [Parameter(Mandatory)] [string] $StalePath,
    [Parameter(Mandatory)] [string] $ExpectedPath
)
# Sonda desechable 2: instancia OCULTA y propia de Excel, solo lectura, nada se guarda.
$ErrorActionPreference = 'Stop'
$Path = (Resolve-Path -LiteralPath $Path).ProviderPath
$StalePath = (Resolve-Path -LiteralPath $StalePath).ProviderPath
$exp = Get-Content -LiteralPath $ExpectedPath -Raw -Encoding utf8 | ConvertFrom-Json
$before = @(Get-Process EXCEL -ErrorAction SilentlyContinue | ForEach-Object Id)
$errBefore = @(Get-ChildItem -Path $env:TEMP -Filter 'error*.xml' -ErrorAction SilentlyContinue | ForEach-Object FullName)

function Read-Summary($sheet, [int]$top, [int]$count) {
    $out = @()
    for ($i = 1; $i -le $count; $i++) {
        $r = $top + $i
        $out += ,@($sheet.Cells.Item($r, 1).Value2, $sheet.Cells.Item($r, 2).Value2)
    }
    return ,$out
}
function Compare-Summary($label, $actual, $expected) {
    $bad = 0
    for ($i = 0; $i -lt $expected.Count; $i++) {
        $a = $actual[$i]; $e = $expected[$i]
        if ("$($a[0])" -ne "$($e[0])" -or "$($a[1])" -ne "$($e[1])") { $bad++; "   DIF fila $($i + 1): real='$($a[0])|$($a[1])' esperado='$($e[0])|$($e[1])'" }
    }
    "$label = " + $(if ($bad -eq 0) { 'IGUAL' } else { "DIFERENTE ($bad)" })
}
function Show($sheet, [int]$n) { for ($r = 1; $r -le $n; $r++) { "   $r | $($sheet.Cells.Item($r,1).Value2) | $($sheet.Cells.Item($r,2).Value2)" } }

$xl = New-Object -ComObject Excel.Application
try {
    $xl.Visible = $false
    $xl.DisplayAlerts = $false
    $xl.AutomationSecurity = 3
    $waited = 0
    while (($null -eq $xl.Workbooks) -and $waited -lt 30) { Start-Sleep -Milliseconds 500; $waited++ }
    "EXCEL_LISTO tras $($waited * 500) ms; workbooks_nulo=$($null -eq $xl.Workbooks) ready=$($xl.Ready)"
    "EXCEL = $($xl.Version) build $($xl.Build) idioma_ui=$($xl.LanguageSettings.LanguageID(2))"
    $top = [int]$exp.top
    $n = $exp.rows.Count

    # A) Apertura normal
    $wb = $xl.Workbooks.Open($Path, 0, $true)
    if ($null -eq $wb) { "A.OPEN_NULO Workbooks=$($xl.Workbooks.Count) ProtectedViewWindows=$($xl.ProtectedViewWindows.Count)"; throw 'Workbooks.Open devolvio null' }
    "A.CAPTION = $($wb.Windows.Item(1).Caption)"
    $data = $wb.Worksheets.Item('Por rack'); $res = $wb.Worksheets.Item('Resumen')
    $lo = $data.ListObjects.Item(1)
    "A.TABLE = $($lo.Name) $($lo.Range.Address($false,$false)) formula_J2='$($data.Range('J2').Formula)'"
    $pt = $res.PivotTables().Item(1)
    "A.PIVOT = $($pt.Name) fuente=$($pt.SourceData) rango=$($pt.TableRange2.Address($false,$false))"
    "A.FILTROS = " + (@(1, 2 | ForEach-Object { $f = $pt.PageFields([int]$_); "$($f.Name)=$($f.CurrentPage.Name)" }) -join ' ; ')
    "A.FILAS = " + (@(1, 2 | ForEach-Object { $pt.RowFields([int]$_).Name }) -join ' -> ')
    $df1 = $pt.DataFields(1)
    "A.VALORES = $($df1.Name) funcion=$($df1.Function) (-4157=xlSum) origen=$($df1.SourceName)"
    Show $res ($top + $n)
    Compare-Summary 'A.RESUMEN_vs_ESPERADO' (Read-Summary $res $top $n) $exp.rows
    "A.GRAND = $($pt.GetPivotData('Suma de Cantidad total').Value2) esperado=$($exp.grand)"

    # B) Filtros
    $rack = $pt.PivotFields('Rack'); $nivel = $pt.PivotFields('Nivel')
    $rack.CurrentPage = 'R-A'
    "B.RACK_R-A = $($pt.GetPivotData('Suma de Cantidad total').Value2) esperado=78"
    $nivel.CurrentPage = 'Pieza'
    "B.RACK_R-A_NIVEL_Pieza = $($pt.GetPivotData('Suma de Cantidad total').Value2) esperado=72"
    $rack.ClearAllFilters(); $nivel.ClearAllFilters()
    "B.SIN_FILTROS = $($pt.GetPivotData('Suma de Cantidad total').Value2) esperado=$($exp.grand)"

    # C) Vinculo a la Table: cambiar datos, agregar fila, actualizar
    $rodillo = $null
    for ($i = 1; $i -le $lo.ListRows.Count; $i++) { if ($lo.ListRows.Item($i).Range.Cells.Item(1, 7).Value2 -eq 'Rodillo') { $rodillo = $lo.ListRows.Item($i) } }
    $rodillo.Range.Cells.Item(1, 3).Value2 = 5
    "C.ROD_CANTIDAD_TOTAL_recalculada = $($rodillo.Range.Cells.Item(1, 10).Value2) esperado=150"
    $new = $lo.ListRows.Add()
    $nc = $new.Range.Cells
    $nc.Item(1, 1).Value2 = 'R-D'; $nc.Item(1, 2).Value2 = 'Cama'; $nc.Item(1, 3).Value2 = 2; $nc.Item(1, 4).Value2 = 'Pieza'
    $nc.Item(1, 5).Value2 = 'Rodillo'; $nc.Item(1, 6).Value2 = 'RN'; $nc.Item(1, 7).Value2 = 'Zeta nuevo'; $nc.Item(1, 8).Value2 = 12; $nc.Item(1, 9).Value2 = 7
    "C.FILA_NUEVA_cantidad_total_autollenada = $($new.Range.Cells.Item(1, 10).Value2) esperado=14 formula='$($new.Range.Cells.Item(1, 10).Formula)'"
    "C.TABLA_AMPLIADA = $($lo.Range.Address($false,$false))"
    "C.ANTES_DE_ACTUALIZAR = $($pt.GetPivotData('Suma de Cantidad total').Value2)"
    [void]$wb.RefreshAll()
    "C.TRAS_RefreshAll = $($pt.GetPivotData('Suma de Cantidad total').Value2) esperado=276 fuente=$($pt.SourceData) rango=$($pt.TableRange2.Address($false,$false))"
    Show $res ($top + $n + 3)
    $wb.Close($false)

    # D) Celdas precalculadas falsas (x1000): si la apertura normal refresca, deben reemplazarse solas
    $wb2 = $xl.Workbooks.Open($StalePath, 0, $true)
    "D.CAPTION = $($wb2.Windows.Item(1).Caption)"
    Compare-Summary 'D.STALE_tras_abrir_vs_ESPERADO' (Read-Summary $wb2.Worksheets.Item('Resumen') $top $n) $exp.rows
    $wb2.Close($false)

    # E) Vista protegida
    try {
        $pv = $xl.ProtectedViewWindows.Open($Path)
        $s = $pv.Workbook.Worksheets.Item('Resumen')
        "E.PV_USED = $($s.UsedRange.Address($false,$false))"
        Show $s ($top + $n)
        Compare-Summary 'E.PV_RESUMEN_vs_ESPERADO' (Read-Summary $s $top $n) $exp.rows
        $sd = $pv.Workbook.Worksheets.Item('Por rack')
        "E.PV_J2_valor_cacheado = $($sd.Range('J2').Value2) esperado=6"
        $pv.Close()
    } catch { "E.PV_ERROR = $($_.Exception.Message)" }
}
finally {
    try { while ($xl.Workbooks.Count -gt 0) { $xl.Workbooks.Item(1).Close($false) } } catch { }
    $xl.Quit()
    [void][Runtime.InteropServices.Marshal]::ReleaseComObject($xl)
    [GC]::Collect(); [GC]::WaitForPendingFinalizers()
    Start-Sleep -Milliseconds 1500
    foreach ($id in @(Get-Process EXCEL -ErrorAction SilentlyContinue | ForEach-Object Id)) { if ($before -notcontains $id) { Stop-Process -Id $id -Force; "CLEANUP: detenida instancia propia $id" } }
}
$newErr = @(Get-ChildItem -Path $env:TEMP -Filter 'error*.xml' -ErrorAction SilentlyContinue | Where-Object { $errBefore -notcontains $_.FullName })
"REPAIR_LOGS_NUEVOS = $($newErr.Count)"
foreach ($f in $newErr) { Get-Content -LiteralPath $f.FullName -Raw }
