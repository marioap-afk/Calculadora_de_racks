param([Parameter(Mandatory)] [string] $Dir)
# Sonda desechable 9 (I-68, R-15/R-16). Parte 1: cada archivo en instancia limpia. Parte 2: varios libros encadenados en una instancia.
# Una dinamica sin refrescar conserva refreshedDate del archivo (2023). Instancias OCULTAS propias; lectura; nada se guarda.
$ErrorActionPreference = 'Stop'
$Dir = (Resolve-Path -LiteralPath $Dir).ProviderPath
$log = New-Object System.Text.StringBuilder
function Say($s) { [void]$log.AppendLine("$s"); Write-Host "$s" }
function Retry([scriptblock]$Action) {
    for ($k = 0; $k -lt 80; $k++) {
        try { return & $Action }
        catch { if ($_.Exception.Message -notmatch '0x80010001|0x8001010A|valores null|null-valued') { throw }; Start-Sleep -Milliseconds 500 }
    }
    throw 'Excel no respondio en 40 s'
}
function Settle($xl) { for ($w = 0; $w -lt 40; $w++) { $ready = $false; try { $ready = [bool]$xl.Ready } catch { }; if ($ready) { break }; Start-Sleep -Milliseconds 250 }; Start-Sleep -Milliseconds 2500 }
function PtOf($wb) { $r = Retry { $wb.Worksheets.Item('Resumen') }; Retry { (Retry { $r.PivotTables() }).Item(1) } }
function RefreshedYear($pt) { (Retry { $pt.RefreshDate }).Year }
function ItemCount($pt) { try { $f = Retry { $pt.PivotFields('Rack') }; $i = Retry { $f.PivotItems() }; Retry { $i.Count } } catch { "error" } }
function Labels($wb) {
    $r = Retry { $wb.Worksheets.Item('Resumen') }
    $out = @()
    for ($i = 5; $i -le 30; $i++) { $a = Retry { $r.Cells.Item($i, 1).Value2 }; $b = Retry { $r.Cells.Item($i, 2).Value2 }; if ($null -ne $a -or $null -ne $b) { $out += "$a=$b" } }
    $out -join ' ; '
}
function NewXl { $x = New-Object -ComObject Excel.Application; Retry { $x.Visible = $false }; Retry { $x.DisplayAlerts = $false }; Retry { $x.AutomationSecurity = 3 }; $x }
function Kill-Xl($xl, $before) {
    try { while ($xl.Workbooks.Count -gt 0) { $xl.Workbooks.Item(1).Close($false) } } catch { }
    try { $xl.Quit() } catch { }
    [void][Runtime.InteropServices.Marshal]::ReleaseComObject($xl)
    [GC]::Collect(); [GC]::WaitForPendingFinalizers(); Start-Sleep -Milliseconds 1500
    foreach ($id in @(Get-Process EXCEL -ErrorAction SilentlyContinue | ForEach-Object Id)) { if ($before -notcontains $id) { Stop-Process -Id $id -Force } }
}

Say '##### PARTE 1: un archivo por instancia limpia'
foreach ($name in 't0_control_sin_containsInteger', 't1_base', 't2_acentos', 't3_mayus_listada', 't4_mayus_unificada', 't5_rack_mayus_listada', 't9_header_only', 't6_min_casos_rotos', 't7_min_celdas_sobrantes', 't8_min_longitudes', 'a1_acentos') {
    $errBefore = @(Get-ChildItem -Path $env:TEMP -Filter 'error*.xml' -ErrorAction SilentlyContinue | ForEach-Object FullName)
    $before = @(Get-Process EXCEL -ErrorAction SilentlyContinue | ForEach-Object Id)
    $xl = NewXl
    try {
        $wb = Retry { $xl.Workbooks.Open((Join-Path $Dir "$name.xlsx"), 0, $true) }
        Settle $xl
        $state = ''
        try { $pt = PtOf $wb; $state = "refresco_anio=$(RefreshedYear $pt) items_rack=$(ItemCount $pt)" } catch { $state = 'sin_dinamica' }
        Say "[$name] ABRE titulo=$(Retry { $wb.Windows.Item(1).Caption }) $state :: $(Labels $wb)"
    }
    catch { Say "[$name] NO_ABRE = $($_.Exception.Message)" }
    finally { Kill-Xl $xl $before }
    $n = @(Get-ChildItem -Path $env:TEMP -Filter 'error*.xml' -ErrorAction SilentlyContinue | Where-Object { $errBefore -notcontains $_.FullName }).Count
    if ($n -gt 0) { Say "   registros_de_reparacion=$n" }
}

function Chain($title, [string[]]$files, [scriptblock]$AfterOpen, [scriptblock]$AtEnd = $null) {
    Say "##### CADENA: $title"
    $before = @(Get-Process EXCEL -ErrorAction SilentlyContinue | ForEach-Object Id)
    $xl = NewXl
    try {
        $n = 0
        foreach ($f in $files) {
            $n++
            try {
                $wb = Retry { $xl.Workbooks.Open((Join-Path $Dir "$f.xlsx"), 0, $true) }
                Settle $xl
                & $AfterOpen $xl $wb $f $n
            }
            catch { Say "  [$n $f] ERROR = $($_.Exception.Message)" }
        }
        if ($null -ne $AtEnd) { & $AtEnd $xl }
    }
    finally { Kill-Xl $xl $before }
}
function Report($xl, $wb, $f, $n, [switch]$Recover) {
    $pt = PtOf $wb
    $y = RefreshedYear $pt
    Say "  [$n $f] refresco_anio=$y items_rack=$(ItemCount $pt)"
    if ($Recover -and $y -lt 2026) {
        [void](Retry { $pt.RefreshTable() }); Settle $xl
        Say "     tras Actualizar: refresco_anio=$(RefreshedYear $pt) items_rack=$(ItemCount $pt) :: $(Labels $wb)"
    }
    return $pt
}

$held = New-Object System.Collections.ArrayList
Chain 'leer y cerrar, objetos COM vivos' @('c1', 'c2', 'c3', 'c4', 'c5', 'c6') { param($xl, $wb, $f, $n) $p = Report $xl $wb $f $n -Recover; [void]$held.Add($p); Retry { $wb.Close($false) } }
$held.Clear()
Chain 'leer y cerrar, liberando COM' @('c1', 'c2', 'c3', 'c4', 'c5', 'c6') { param($xl, $wb, $f, $n) $p = Report $xl $wb $f $n; [void][Runtime.InteropServices.Marshal]::FinalReleaseComObject($p); $p = $null; [GC]::Collect(); [GC]::WaitForPendingFinalizers(); Retry { $wb.Close($false) }; [GC]::Collect() }
Chain 'sin cerrar, todos abiertos a la vez' @('c1', 'c2', 'c3', 'c4', 'c5', 'c6') { param($xl, $wb, $f, $n) [void](Report $xl $wb $f $n) }
Chain 'patron exacto del fallo: t6 t7 t8 a1, objetos vivos' @('t6_min_casos_rotos', 't7_min_celdas_sobrantes', 't8_min_longitudes', 'a1_acentos') { param($xl, $wb, $f, $n) $p = Report $xl $wb $f $n -Recover; [void]$held.Add($p); Retry { $wb.Close($false) } }
$held.Clear()
$open = New-Object System.Collections.ArrayList
Chain 'sin cerrar, espera larga y relectura final' @('c1', 'c2', 'c3', 'c4', 'c5', 'c6') { param($xl, $wb, $f, $n) Start-Sleep -Seconds 6; [void](Report $xl $wb $f $n); [void]$open.Add(@($f, $wb)) } { param($xl) Start-Sleep -Seconds 12; Say '  -- relectura tras 12 s --'; foreach ($o in $open) { $pt = PtOf $o[1]; Say "  [$($o[0])] refresco_anio=$(RefreshedYear $pt) items_rack=$(ItemCount $pt) celdas=$(Labels $o[1])" } }
[IO.File]::WriteAllText((Join-Path $Dir 'results.txt'), $log.ToString(), [Text.UTF8Encoding]::new($false))
