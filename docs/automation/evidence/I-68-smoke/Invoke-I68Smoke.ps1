param(
    [Parameter(Mandatory)] [string] $Dir,
    [Parameter(Mandatory)] [string] $LogPath
)
# I-68: humo en Excel real, S-01..S-12 de la Proposal V6 §11. Instancias OCULTAS propias; solo lectura; nada se guarda.
# Los esperados se calculan desde el rango de la tabla, no desde la dinamica. Salida: una linea PASS o FAIL por comprobacion.
$ErrorActionPreference = 'Stop'
$Dir = (Resolve-Path -LiteralPath $Dir).ProviderPath
$log = New-Object System.Text.StringBuilder
$script:passed = 0; $script:failed = 0
function Say($s) { [void]$log.AppendLine("$s"); Write-Host "$s" }
function Check($id, $label, $actual, $expected) {
    $ok = "$actual" -ceq "$expected"
    if ($ok) { $script:passed++ } else { $script:failed++ }
    Say ("{0} {1} {2}: real=[{3}] esperado=[{4}]" -f $(if ($ok) { 'PASS' } else { 'FAIL' }), $id, $label, $actual, $expected)
}
function Retry([scriptblock]$Action) {
    for ($k = 0; $k -lt 120; $k++) {
        try { return & $Action }
        catch { if ($_.Exception.Message -notmatch '0x80010001|0x8001010A|valores null|null-valued') { throw }; Start-Sleep -Milliseconds 250 }
    }
    throw 'Excel no respondio'
}
function PtOf($wb) { $r = Retry { $wb.Worksheets.Item('Resumen') }; Retry { (Retry { $r.PivotTables() }).Item(1) } }
function Items($pt) { try { $f = Retry { $pt.PivotFields('Rack') }; $i = Retry { $f.PivotItems() }; Retry { $i.Count } } catch { 0 } }
function WaitRefreshed($pt, [int]$seconds) {
    $sw = [Diagnostics.Stopwatch]::StartNew()
    while ($sw.Elapsed.TotalSeconds -lt $seconds) { if ((Items $pt) -gt 0) { return [math]::Round($sw.Elapsed.TotalSeconds, 1) }; Start-Sleep -Milliseconds 500 }
    return -1
}
function ItemsNames($pt) { $f = Retry { $pt.PivotFields('Rack') }; $i = Retry { $f.PivotItems() }; $c = Retry { $i.Count }; @(1..$c | ForEach-Object { $it = Retry { $i.Item([int]$_) }; Retry { $it.Name } }) }
function Grand($pt) { Retry { $pt.GetPivotData('Suma de Cantidad total').Value2 } }
function Detail($lo) {
    $v = Retry { , $lo.DataBodyRange.Value2 }
    $n = $v.GetLength(0)
    $out = @()
    for ($i = 1; $i -le $n; $i++) {
        $out += [pscustomobject]@{ Rack = [string]$v[$i, 1]; Nivel = [string]$v[$i, 4]; Desc = [string]$v[$i, 7]; Len = [double]$v[$i, 8]; Total = [double]$v[$i, 10] }
    }
    return $out
}
function SumOf($rows) { $s = 0.0; foreach ($r in $rows) { $s += $r.Total }; $s }
function Repairs() { @(Get-ChildItem -Path $env:TEMP -Filter 'error*.xml' -ErrorAction SilentlyContinue | ForEach-Object FullName) }
function Scenario($name, [scriptblock]$Body) {
    Say "===== $name"
    $errBefore = Repairs
    $before = @(Get-Process EXCEL -ErrorAction SilentlyContinue | ForEach-Object Id)
    $xl = New-Object -ComObject Excel.Application
    try {
        Retry { $xl.Visible = $false }; Retry { $xl.DisplayAlerts = $false }; Retry { $xl.AutomationSecurity = 3 }
        & $Body $xl
    }
    catch { $script:failed++; Say "FAIL $name excepcion: $($_.Exception.Message)" }
    finally {
        try { while ($xl.Workbooks.Count -gt 0) { $xl.Workbooks.Item(1).Close($false) } } catch { }
        try { $xl.Quit() } catch { }
        [void][Runtime.InteropServices.Marshal]::ReleaseComObject($xl)
        [GC]::Collect(); [GC]::WaitForPendingFinalizers(); Start-Sleep -Milliseconds 1500
        foreach ($id in @(Get-Process EXCEL -ErrorAction SilentlyContinue | ForEach-Object Id)) { if ($before -notcontains $id) { Stop-Process -Id $id -Force } }
    }
    Check 'S-01' "$name sin registros de reparacion" @(Repairs | Where-Object { $errBefore -notcontains $_ }).Count 0
}
$Notes = @(
    'Atención: con Nivel en (Todas) la suma no representa necesariamente un total físico.',
    'Pueden coexistir componentes y piezas relacionadas; usa el filtro Nivel para verlos por separado.',
    'Las camas de Dinámico y Push Back solo aparecen como Componente. Si los filtros salen vacíos, usa Datos > Actualizar todo.')
function Open-Main($xl, $file) { Retry { $xl.Workbooks.Open((Join-Path $Dir $file), 0, $true) } }

Scenario 'principal' {
    param($xl)
    $wb = Open-Main $xl 'smoke-main.xlsx'
    Check 'S-01' 'titulo sin reparacion' (Retry { $wb.Windows.Item(1).Caption }) 'smoke-main.xlsx'
    $d = Retry { $wb.Worksheets.Item('Por rack') }; $res = Retry { $wb.Worksheets.Item('Resumen') }
    $lo = Retry { $d.ListObjects.Item(1) }
    $pt = PtOf $wb
    if ((WaitRefreshed $pt 15) -lt 0) { throw 'la dinamica no refresco en 15 s' }
    $rows = Detail $lo
    Check 'S-02' 'tabla nativa' (Retry { $lo.Name }) 'RackCadBOM'
    Check 'S-02' 'autofiltro y rayas' ("$(Retry { $lo.ShowAutoFilter })/$(Retry { $lo.ShowTableStyleRowStripes })") 'True/True'
    Check 'S-02' 'dinamica nativa' (Retry { $pt.Name }) 'ResumenBOM'
    Check 'S-02' 'fuente = tabla' (Retry { $pt.SourceData }) 'RackCadBOM'
    for ($i = 1; $i -le 3; $i++) { Check 'S-02' "D$i conservada" (Retry { $res.Cells.Item($i, 4).Value2 }) $Notes[$i - 1] }
    Check 'S-02' 'campos de fila' (@(1, 2 | ForEach-Object { Retry { $pt.RowFields([int]$_).Name } }) -join ' -> ') 'Descripcion -> Longitud (in)'
    Check 'S-02' 'filtros' (@(1, 2 | ForEach-Object { Retry { $pt.PageFields([int]$_).Name } }) -join ' + ') 'Rack + Nivel'
    $df = Retry { $pt.DataFields(1) }
    Check 'S-08' 'agregacion suma' (Retry { $df.Function }) -4157
    Check 'S-08' 'origen del valor' (Retry { $df.SourceName }) 'Cantidad total'
    $total = SumOf $rows
    Check 'S-07' 'total = SUM de la columna' (Grand $pt) $total
    $rack = Retry { $pt.PivotFields('Rack') }; $nivel = Retry { $pt.PivotFields('Nivel') }
    Check 'S-04' 'Nivel abre en (All)' (Retry { $nivel.CurrentPage.Name }) '(All)'
    Check 'S-04' 'Rack abre en (All)' (Retry { $rack.CurrentPage.Name }) '(All)'
    Retry { $rack.CurrentPage = 'R-A' }
    Check 'S-03' 'Rack R-A' (Grand $pt) (SumOf ($rows | Where-Object Rack -EQ 'R-A'))
    Retry { $nivel.CurrentPage = 'Pieza' }
    Check 'S-05' 'R-A + Pieza' (Grand $pt) (SumOf ($rows | Where-Object { $_.Rack -eq 'R-A' -and $_.Nivel -eq 'Pieza' }))
    Retry { $rack.ClearAllFilters() }
    Check 'S-04' 'Nivel Pieza' (Grand $pt) (SumOf ($rows | Where-Object Nivel -EQ 'Pieza'))
    Retry { $nivel.CurrentPage = 'Componente' }
    Check 'S-04' 'Nivel Componente' (Grand $pt) (SumOf ($rows | Where-Object Nivel -EQ 'Componente'))
    Retry { $nivel.ClearAllFilters() }
    Retry { $rack.EnableMultiplePageItems = $true }
    $item = Retry { $rack.PivotItems('R-C') }; Retry { $item.Visible = $false }
    Check 'S-03' 'varios racks, sin R-C' (Grand $pt) (SumOf ($rows | Where-Object Rack -NE 'R-C'))
    Retry { $rack.ClearAllFilters() }; Retry { $rack.EnableMultiplePageItems = $false }

    foreach ($g in ($rows | Where-Object Desc -NE '' | Group-Object { $_.Desc + '|' + $_.Len })) {
        $first = $g.Group[0]
        Check 'S-06' "suma $($g.Name)" (Retry { $pt.GetPivotData('Suma de Cantidad total', 'Descripcion', $first.Desc, 'Longitud (in)', $first.Len).Value2 }) (SumOf $g.Group)
    }
    $found = @(); for ($i = 5; $i -le 40; $i++) { if ((Retry { $res.Cells.Item($i, 1).Value2 }) -eq 'Poste C3') { for ($j = $i + 1; $j -le 40; $j++) { $v = Retry { $res.Cells.Item($j, 1).Value2 }; if ($v -is [double]) { $found += $v } else { break } }; break } }
    Check 'S-06' 'orden numerico de Poste C3' ($found -join ',') ((@($rows | Where-Object Desc -EQ 'Poste C3' | ForEach-Object Len) | Sort-Object -Unique) -join ',')

    $target = $null
    for ($i = 1; $i -le (Retry { $lo.ListRows.Count }); $i++) { $lr = Retry { $lo.ListRows.Item($i) }; if ((Retry { $lr.Range.Cells.Item(1, 7).Value2 }) -eq 'Rodillo') { $target = $lr } }
    Retry { $target.Range.Cells.Item(1, 10).Value2 = 100 }; [void](Retry { $wb.RefreshAll() })
    Start-Sleep -Milliseconds 1500
    $edited = $total - 30 + 100
    Check 'S-09' 'editar Cantidad total y actualizar' (Grand $pt) $edited
    Retry { $target.Range.Cells.Item(1, 3).Value2 = 9 }; Retry { $target.Range.Cells.Item(1, 9).Value2 = 77 }
    Check 'S-09' 'editar Copias y Cantidad no recalcula' (Retry { $target.Range.Cells.Item(1, 10).Value2 }) 100
    [void](Retry { $wb.RefreshAll() }); Start-Sleep -Milliseconds 1500
    Check 'S-09' 'resumen igual tras editar solo Copias y Cantidad' (Grand $pt) $edited
    $new = Retry { $lo.ListRows.Add() }; $n = $new.Range
    Retry { $n.Cells.Item(1, 1).Value2 = 'R-E' }; Retry { $n.Cells.Item(1, 2).Value2 = 'Cama' }; Retry { $n.Cells.Item(1, 3).Value2 = 1 }; Retry { $n.Cells.Item(1, 4).Value2 = 'Pieza' }
    Retry { $n.Cells.Item(1, 5).Value2 = 'X' }; Retry { $n.Cells.Item(1, 6).Value2 = 'X' }; Retry { $n.Cells.Item(1, 7).Value2 = 'Zeta' }; Retry { $n.Cells.Item(1, 8).Value2 = 7 }; Retry { $n.Cells.Item(1, 9).Value2 = 3 }
    Check 'S-09' 'fila nueva sin formula' ("" + (Retry { $n.Cells.Item(1, 10).Value2 })) ''
    Retry { $n.Cells.Item(1, 10).Value2 = 3 }
    [void](Retry { $wb.RefreshAll() }); Start-Sleep -Milliseconds 1500
    Check 'S-09' 'fila nueva con valor y actualizar' (Grand $pt) ($edited + 3)
    Check 'S-09' 'fuente sigue siendo la tabla' (Retry { $pt.SourceData }) 'RackCadBOM'
    Check 'S-09' 'filtro Rack incluye la fila nueva' (((ItemsNames $pt) -contains 'R-E')) 'True'
}

Scenario 'vista protegida' {
    param($xl)
    $pv = Retry { $xl.ProtectedViewWindows.Open((Join-Path $Dir 'smoke-main.xlsx')) }
    Start-Sleep -Milliseconds 1500
    $s = Retry { $pv.Workbook.Worksheets.Item('Resumen') }
    Check 'S-10' 'B1 visible en Vista protegida' (Retry { $s.Range('B1').Value2 }) '(Todas)'
    $lastLabel = $null; $lastValue = $null
    for ($i = 5; $i -le 40; $i++) { $a = Retry { $s.Cells.Item($i, 1).Value2 }; if ($a -eq 'Total general') { $lastLabel = $a; $lastValue = Retry { $s.Cells.Item($i, 2).Value2 }; break } }
    $d = Retry { $pv.Workbook.Worksheets.Item('Por rack') }
    $sum = 0.0; for ($i = 2; $i -le 40; $i++) { $v = Retry { $d.Cells.Item($i, 10).Value2 }; if ($null -eq $v) { break }; $sum += $v }
    Check 'S-10' 'resumen visible = SUM del detalle' $lastValue $sum
    $wb = Retry { $pv.Edit() }
    $pt = PtOf $wb
    if ((WaitRefreshed $pt 15) -lt 0) { throw 'no refresco tras habilitar edicion' }
    Check 'S-10' 'al habilitar edicion refresca' (Grand $pt) $sum
}

Scenario 'libros encadenados' {
    param($xl)
    foreach ($n in 1..6) {
        $wb = Retry { $xl.Workbooks.Open((Join-Path $Dir "smoke-c$n.xlsx"), 0, $true) }
        $pt = PtOf $wb
        $t = WaitRefreshed $pt 15
        Say "   smoke-c$n refresco en $t s"
        Check 'S-11' "smoke-c$n con valores en el filtro antes de 15 s" ($t -ge 0) 'True'
        Retry { $wb.Close($false) }
    }
}

Scenario 'simulacion sin refresco' {
    param($xl)
    $copy = Join-Path $Dir 'smoke-sin-refresco.xlsx'
    Copy-Item -LiteralPath (Join-Path $Dir 'smoke-main.xlsx') -Destination $copy -Force
    Add-Type -AssemblyName System.IO.Compression.FileSystem
    $zip = [IO.Compression.ZipFile]::Open($copy, 'Update')
    try {
        $name = 'xl/pivotCache/pivotCacheDefinition1.xml'
        $entry = $zip.GetEntry($name)
        $reader = New-Object IO.StreamReader($entry.Open(), [Text.Encoding]::UTF8); $text = $reader.ReadToEnd(); $reader.Dispose()
        if ($text -notmatch 'refreshOnLoad="1"') { throw 'la salida no trae refreshOnLoad=1' }
        $entry.Delete()
        $newEntry = $zip.CreateEntry($name)
        $writer = New-Object IO.StreamWriter($newEntry.Open(), [Text.UTF8Encoding]::new($false)); $writer.Write($text.Replace('refreshOnLoad="1"', 'refreshOnLoad="0"')); $writer.Dispose()
    }
    finally { $zip.Dispose() }
    $wb = Retry { $xl.Workbooks.Open($copy, 0, $true) }
    Start-Sleep -Milliseconds 3000
    $pt = PtOf $wb
    Check 'S-11' 'simulacion: filtro Rack sin valores' (Items $pt) 0
    $s = Retry { $wb.Worksheets.Item('Resumen') }
    Check 'S-11' 'simulacion: celdas visibles' (Retry { $s.Range('B4').Value2 }) 'Suma de Cantidad total'
    [void](Retry { $wb.RefreshAll() }); Start-Sleep -Milliseconds 1500
    $d = Retry { $wb.Worksheets.Item('Por rack') }; $lo = Retry { $d.ListObjects.Item(1) }
    Check 'S-11' 'simulacion: RefreshAll recupera' (Grand $pt) (SumOf (Detail $lo))
}

foreach ($file in 'smoke-empty', 'smoke-rack-vacio') {
    Scenario "BOM vacio $file" {
        param($xl)
        $wb = Retry { $xl.Workbooks.Open((Join-Path $Dir "$file.xlsx"), 0, $true) }
        Check 'S-12' "$file sin reparacion" (Retry { $wb.Windows.Item(1).Caption }) "$file.xlsx"
        $d = Retry { $wb.Worksheets.Item('Por rack') }; $lo = Retry { $d.ListObjects.Item(1) }
        Check 'S-12' "$file tabla con una fila en blanco" (Retry { $lo.Range.Address($false, $false) }) 'A1:J2'
        $pt = PtOf $wb
        Check 'S-12' "$file dinamica presente" (Retry { $pt.Name }) 'ResumenBOM'
    }
}

if (Test-Path -LiteralPath (Join-Path $Dir 'smoke-sistemas.xlsx')) {
    Scenario 'sistemas reales' {
        param($xl)
        $wb = Open-Main $xl 'smoke-sistemas.xlsx'
        Check 'S-01' 'titulo sin reparacion' (Retry { $wb.Windows.Item(1).Caption }) 'smoke-sistemas.xlsx'
        $d = Retry { $wb.Worksheets.Item('Por rack') }; $lo = Retry { $d.ListObjects.Item(1) }
        $pt = PtOf $wb
        if ((WaitRefreshed $pt 15) -lt 0) { throw 'la dinamica no refresco en 15 s' }
        $rows = Detail $lo
        Check 'S-07' 'total = SUM de la columna' (Grand $pt) (SumOf $rows)
        $rack = Retry { $pt.PivotFields('Rack') }
        foreach ($g in ($rows | Group-Object Rack)) {
            Retry { $rack.CurrentPage = $g.Name }
            Check 'S-03' "Rack $($g.Name)" (Grand $pt) (SumOf $g.Group)
        }
    }
}

Say "RESULT: PASS=$script:passed FAIL=$script:failed"
[IO.File]::WriteAllText($LogPath, $log.ToString(), [Text.UTF8Encoding]::new($false))
exit $script:failed
