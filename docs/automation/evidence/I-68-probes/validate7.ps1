param([Parameter(Mandatory)] [string] $Dir)
# Sonda desechable 7 (I-68): mide el diseno recomendado. Instancia OCULTA propia por escenario; lectura, nada se guarda.
$ErrorActionPreference = 'Stop'
$Dir = (Resolve-Path -LiteralPath $Dir).ProviderPath
$exp = Get-Content -LiteralPath (Join-Path $Dir 'expected.json') -Raw -Encoding utf8 | ConvertFrom-Json
$log = New-Object System.Text.StringBuilder
function Say($s) { [void]$log.AppendLine("$s"); "$s" }
function Retry([scriptblock]$Action) {
    for ($k = 0; $k -lt 80; $k++) {
        try { return & $Action }
        catch { if ($_.Exception.Message -notmatch '0x80010001|0x8001010A|valores null|null-valued') { throw }; Start-Sleep -Milliseconds 500 }
    }
    throw 'Excel no respondio en 40 s'
}
function Settle($xl) { for ($w = 0; $w -lt 40; $w++) { $ready = $false; try { $ready = [bool]$xl.Ready } catch { }; if ($ready) { break }; Start-Sleep -Milliseconds 250 }; Start-Sleep -Milliseconds 2500 }
function Check($label, $actual, $expected) { $ok = "$actual" -eq "$expected"; Say ("{0} {1} real={2} esperado={3}" -f $(if ($ok) { 'PASS' } else { 'FAIL' }), $label, $actual, $expected) }
function Grand($pt) { Retry { $pt.GetPivotData('Suma de Cantidad total').Value2 } }
function PtOf($wb) { $r = Retry { $wb.Worksheets.Item('Resumen') }; Retry { (Retry { $r.PivotTables() }).Item(1) } }
function ItemsOf($pt, $field) { $f = Retry { $pt.PivotFields($field) }; $i = Retry { $f.PivotItems() }; $c = Retry { $i.Count }; @(1..$c | ForEach-Object { $it = Retry { $i.Item([int]$_) }; Retry { $it.Name } }) }
function Scenario($name, [scriptblock]$Body) {
    Say "===== $name"
    $errBefore = @(Get-ChildItem -Path $env:TEMP -Filter 'error*.xml' -ErrorAction SilentlyContinue | ForEach-Object FullName)
    $before = @(Get-Process EXCEL -ErrorAction SilentlyContinue | ForEach-Object Id)
    $xl = New-Object -ComObject Excel.Application
    try {
        Retry { $xl.Visible = $false }; Retry { $xl.DisplayAlerts = $false }; Retry { $xl.AutomationSecurity = 3 }
        & $Body $xl
    }
    catch { Say "ERROR = $($_.Exception.Message)" }
    finally {
        try { while ($xl.Workbooks.Count -gt 0) { $xl.Workbooks.Item(1).Close($false) } } catch { }
        try { $xl.Quit() } catch { }
        [void][Runtime.InteropServices.Marshal]::ReleaseComObject($xl)
        [GC]::Collect(); [GC]::WaitForPendingFinalizers(); Start-Sleep -Milliseconds 1500
        foreach ($id in @(Get-Process EXCEL -ErrorAction SilentlyContinue | ForEach-Object Id)) { if ($before -notcontains $id) { Stop-Process -Id $id -Force } }
    }
    $newErr = @(Get-ChildItem -Path $env:TEMP -Filter 'error*.xml' -ErrorAction SilentlyContinue | Where-Object { $errBefore -notcontains $_.FullName })
    Say "REPAIR_LOGS_NUEVOS = $($newErr.Count)"
}

Scenario 'S1_apertura_filtros_edicion' {
    param($xl)
    $wb = Retry { $xl.Workbooks.Open((Join-Path $Dir 'm1.xlsx'), 0, $true) }
    Settle $xl
    Say ("CAPTION = " + (Retry { $wb.Windows.Item(1).Caption }))
    $d = Retry { $wb.Worksheets.Item('Por rack') }
    $lo = Retry { $d.ListObjects.Item(1) }
    Say ("TABLE = " + (Retry { $lo.Name }) + " " + (Retry { $lo.Range.Address($false, $false) }) + " autofiltro=" + (Retry { $lo.ShowAutoFilter }) + " estilo=" + (Retry { $lo.TableStyle.Name }) + " rayas=" + (Retry { $lo.ShowTableStyleRowStripes }))
    $pt = PtOf $wb
    Say ("PIVOT = " + (Retry { $pt.Name }) + " fuente=" + (Retry { $pt.SourceData }) + " rango=" + (Retry { $pt.TableRange2.Address($false, $false) }))
    Say ("FILTROS = " + (@(1, 2 | ForEach-Object { $f = Retry { $pt.PageFields([int]$_) }; (Retry { $f.Name }) + '=' + (Retry { $f.CurrentPage.Name }) }) -join ' ; '))
    Say ("FILAS = " + (@(1, 2 | ForEach-Object { Retry { $pt.RowFields([int]$_).Name } }) -join ' -> '))
    $df = Retry { $pt.DataFields(1) }
    Say ("VALOR = " + (Retry { $df.Name }) + " funcion=" + (Retry { $df.Function }) + " (-4157=SUM) origen=" + (Retry { $df.SourceName }))
    $col = Retry { $lo.ListColumns.Item('Cantidad total') }
    $sum = Retry { $xl.WorksheetFunction.Sum($col.DataBodyRange) }
    Check 'AC-07 SUM(columna)=total pivote' (Grand $pt) $sum
    Check 'grand' (Grand $pt) $exp.grand
    $res = Retry { $wb.Worksheets.Item('Resumen') }
    for ($i = 4; $i -le 26; $i++) { $a = Retry { $res.Cells.Item($i, 1).Value2 }; $b = Retry { $res.Cells.Item($i, 2).Value2 }; if ($null -ne $a -or $null -ne $b) { Say "   $i | $a | $b" } }
    $rowsField = Retry { $pt.PivotFields('Longitud (in)') }
    $poste = @(); for ($i = 5; $i -le 26; $i++) { if ((Retry { $res.Cells.Item($i, 1).Value2 }) -eq 'Poste C3') { for ($j = $i + 1; $j -le 26; $j++) { $v = Retry { $res.Cells.Item($j, 1).Value2 }; if ($v -is [double]) { $poste += $v } else { break } }; break } }
    Check 'AC-03/AC-08 orden numerico Poste' ($poste -join ',') ($exp.poste_longitudes -join ',')
    Check 'AC-02 Poste C3/144 suma' (Retry { $pt.GetPivotData('Suma de Cantidad total', 'Descripcion', 'Poste C3', 'Longitud (in)', 144).Value2 }) $exp.poste_144
    $rack = Retry { $pt.PivotFields('Rack') }; $nivel = Retry { $pt.PivotFields('Nivel') }
    Say ("ITEMS_RACK = " + ((ItemsOf $pt 'Rack') -join ', ') + " ; ITEMS_NIVEL = " + ((ItemsOf $pt 'Nivel') -join ', '))
    Retry { $rack.CurrentPage = 'R-A' }; Check 'AC-04 Rack=R-A' (Grand $pt) $exp.rack_RA
    Retry { $nivel.CurrentPage = 'Pieza' }; Check 'AC-06 Rack=R-A y Nivel=Pieza' (Grand $pt) $exp.ra_pieza
    Retry { $rack.ClearAllFilters() }; Check 'AC-05 Nivel=Pieza' (Grand $pt) $exp.nivel_pieza
    Retry { $nivel.CurrentPage = 'Componente' }; Check 'AC-05 Nivel=Componente' (Grand $pt) $exp.nivel_componente
    Retry { $nivel.ClearAllFilters() }
    Retry { $rack.EnableMultiplePageItems = $true }
    $rcItem = Retry { $rack.PivotItems('R-C') }; Retry { $rcItem.Visible = $false }
    Check 'AC-04 varios racks (sin R-C)' (Grand $pt) $exp.sin_rc
    Retry { $rack.ClearAllFilters() }; Retry { $rack.EnableMultiplePageItems = $false }
    Check 'sin filtros' (Grand $pt) $exp.grand

    # Edicion del detalle: Cantidad total es un VALOR
    $rowOf = { param($t) for ($i = 1; $i -le (Retry { $lo.ListRows.Count }); $i++) { $lr = Retry { $lo.ListRows.Item($i) }; if ((Retry { $lr.Range.Cells.Item(1, 7).Value2 }) -eq $t) { return $lr } } }
    $rod = & $rowOf 'Rodillo'
    Retry { $rod.Range.Cells.Item(1, 10).Value2 = 100 }; [void](Retry { $wb.RefreshAll() }); Settle $xl
    Check 'AC-15 editar Cantidad total y actualizar' (Grand $pt) ([int]$exp.grand - 30 + 100)
    Retry { $rod.Range.Cells.Item(1, 3).Value2 = 9 }; Retry { $rod.Range.Cells.Item(1, 9).Value2 = 77 }
    Check 'R-01 Cantidad total NO cambia al editar Copias/Cantidad' (Retry { $rod.Range.Cells.Item(1, 10).Value2 }) 100
    [void](Retry { $wb.RefreshAll() }); Settle $xl
    Check 'R-01 pivote igual tras editar solo Copias/Cantidad' (Grand $pt) ([int]$exp.grand - 30 + 100)
    $new = Retry { $lo.ListRows.Add() }
    $n = $new.Range
    Retry { $n.Cells.Item(1, 1).Value2 = 'R-E' }; Retry { $n.Cells.Item(1, 2).Value2 = 'Cama' }; Retry { $n.Cells.Item(1, 3).Value2 = 1 }; Retry { $n.Cells.Item(1, 4).Value2 = 'Pieza' }
    Retry { $n.Cells.Item(1, 5).Value2 = 'X' }; Retry { $n.Cells.Item(1, 6).Value2 = 'X' }; Retry { $n.Cells.Item(1, 7).Value2 = 'Zeta' }; Retry { $n.Cells.Item(1, 8).Value2 = 7 }; Retry { $n.Cells.Item(1, 9).Value2 = 3 }
    Check 'R-01 fila nueva: Cantidad total queda vacia' ("" + (Retry { $n.Cells.Item(1, 10).Value2 })) ''
    Retry { $n.Cells.Item(1, 10).Value2 = 3 }
    Say ("TABLA_AMPLIADA = " + (Retry { $lo.Range.Address($false, $false) }))
    [void](Retry { $wb.RefreshAll() }); Settle $xl
    Check 'AC-15 fila nueva y actualizar' (Grand $pt) ([int]$exp.grand - 30 + 100 + 3)
    Say ("FUENTE_TRAS_AMPLIAR = " + (Retry { $pt.SourceData }) + " rango=" + (Retry { $pt.TableRange2.Address($false, $false) }) + " items_rack=" + ((ItemsOf $pt 'Rack') -join ', '))
}

Scenario 'S2_vista_protegida_y_habilitar_edicion' {
    param($xl)
    $pv = Retry { $xl.ProtectedViewWindows.Open((Join-Path $Dir 'm2.xlsx')) }
    Start-Sleep -Milliseconds 1500
    $s = Retry { $pv.Workbook.Worksheets.Item('Resumen') }
    Say ("PV used=" + (Retry { $s.UsedRange.Address($false, $false) }) + " A5='" + (Retry { $s.Range('A5').Value2 }) + "' B5=" + (Retry { $s.Range('B5').Value2 }))
    $wb = Retry { $pv.Edit() }
    Settle $xl
    $pt = PtOf $wb
    Check 'R-02 tras habilitar edicion' (Grand $pt) $exp.grand
    Say ("TRAS_EDIT RefreshDate=" + (Retry { $pt.RefreshDate }) + " items_rack=" + ((ItemsOf $pt 'Rack') -join ', '))
    $res = Retry { $wb.Worksheets.Item('Resumen') }
    for ($i = 5; $i -le 8; $i++) { Say ("   $i | " + (Retry { $res.Cells.Item($i, 1).Value2 }) + " | " + (Retry { $res.Cells.Item($i, 2).Value2 })) }
}

Scenario 'S3_excel_ya_abierto_encadenado_con_objetos_vivos' {
    param($xl)
    $w1 = Retry { $xl.Workbooks.Open((Join-Path $Dir 'm3.xlsx'), 0, $true) }
    Settle $xl
    $r1 = Retry { $w1.Worksheets.Item('Resumen') }; $pts1 = Retry { $r1.PivotTables() }; $pt1 = Retry { $pts1.Item(1) }
    Say ("LIBRO1 grand=" + (Grand $pt1))
    Retry { $w1.Close($false) }
    $w2 = Retry { $xl.Workbooks.Open((Join-Path $Dir 'm4.xlsx'), 0, $true) }
    Settle $xl
    $pt2 = PtOf $w2
    $g0 = Grand $pt2
    $items0 = (ItemsOf $pt2 'Rack') -join ', '
    Say ("LIBRO2 sin_tocar grand=$g0 items_rack=[$items0] RefreshDate=" + (Retry { $pt2.RefreshDate }))
    [void](Retry { $pt2.RefreshTable() }); Settle $xl
    Check 'R-03 Actualizar manual recupera' (Grand $pt2) $exp.grand
    Say ("LIBRO2 tras_actualizar items_rack=" + ((ItemsOf $pt2 'Rack') -join ', '))
}

Scenario 'S4_sin_refresco_al_abrir_y_Actualizar_manual' {
    param($xl)
    $wb = Retry { $xl.Workbooks.Open((Join-Path $Dir 'm5_sin_refresh.xlsx'), 0, $true) }
    Settle $xl
    Say ("CAPTION = " + (Retry { $wb.Windows.Item(1).Caption }))
    $res = Retry { $wb.Worksheets.Item('Resumen') }
    Say ("ESTADO_SIN_REFRESCO celdas: A5='" + (Retry { $res.Range('A5').Value2 }) + "' B5=" + (Retry { $res.Range('B5').Value2 }) + " B16=" + (Retry { $res.Range('B16').Value2 }))
    $pt = PtOf $wb
    $count = 0; try { $count = (ItemsOf $pt 'Rack').Count } catch { $count = "error: $($_.Exception.Message)" }
    Say "ESTADO_SIN_REFRESCO items_rack=$count"
    [void](Retry { $pt.RefreshTable() }); Settle $xl
    Check 'R-03 Actualizar manual recupera desde sin refresco' (Grand $pt) $exp.grand
    Say ("TRAS_ACTUALIZAR items_rack=" + ((ItemsOf $pt 'Rack') -join ', '))
}

[IO.File]::WriteAllText((Join-Path $Dir 'results.txt'), $log.ToString(), [Text.UTF8Encoding]::new($false))
