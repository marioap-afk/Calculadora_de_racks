param(
    [Parameter(Mandatory)] [string] $Path,
    [Parameter(Mandatory)] [string] $StalePath,
    [Parameter(Mandatory)] [string] $ExpectedPath
)
# Sonda desechable 2, partes D y E. Solo lectura, instancia oculta y propia, nada se guarda.
$ErrorActionPreference = 'Stop'
$Path = (Resolve-Path -LiteralPath $Path).ProviderPath
$StalePath = (Resolve-Path -LiteralPath $StalePath).ProviderPath
$exp = Get-Content -LiteralPath $ExpectedPath -Raw -Encoding utf8 | ConvertFrom-Json
$before = @(Get-Process EXCEL -ErrorAction SilentlyContinue | ForEach-Object Id)
$errBefore = @(Get-ChildItem -Path $env:TEMP -Filter 'error*.xml' -ErrorAction SilentlyContinue | ForEach-Object FullName)

function Retry([scriptblock]$Action) {
    for ($k = 0; $k -lt 60; $k++) {
        try { return & $Action }
        catch { if ($_.Exception.Message -notmatch '0x80010001|0x8001010A|valores null|null-valued') { throw }; Start-Sleep -Milliseconds 500 }
    }
    throw 'Excel no respondio en 30 s'
}
function Read-Values($sheet, [int]$top, [int]$count) {
    $out = @()
    for ($i = 1; $i -le $count; $i++) {
        $r = $top + $i
        $out += ,@((Retry { $sheet.Cells.Item($r, 1).Value2 }), (Retry { $sheet.Cells.Item($r, 2).Value2 }))
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

$xl = New-Object -ComObject Excel.Application
try {
    Retry { $xl.Visible = $false }
    Retry { $xl.DisplayAlerts = $false }
    Retry { $xl.AutomationSecurity = 3 }
    $top = [int]$exp.top; $n = $exp.rows.Count

    # D) El archivo trae celdas precalculadas FALSAS (x1000). Solo se corrigen si la apertura normal refresca sola.
    $wb2 = Retry { $xl.Workbooks.Open($StalePath, 0, $true) }
    "D.CAPTION = " + (Retry { $wb2.Windows.Item(1).Caption })
    $s2 = Retry { $wb2.Worksheets.Item('Resumen') }
    Compare-Summary 'D.STALE_tras_abrir_vs_ESPERADO' (Read-Values $s2 $top $n) $exp.rows
    Retry { $wb2.Close($false) }

    # E) Vista protegida del archivo bueno
    $pv = Retry { $xl.ProtectedViewWindows.Open($Path) }
    $s = Retry { $pv.Workbook.Worksheets.Item('Resumen') }
    "E.PV_USED = " + (Retry { $s.UsedRange.Address($false, $false) })
    $v = Read-Values $s $top $n
    for ($i = 0; $i -lt $v.Count; $i++) { "   $($top + $i + 1) | $($v[$i][0]) | $($v[$i][1])" }
    "E.PV_B1 = " + (Retry { $s.Range('B1').Value2 })
    Compare-Summary 'E.PV_RESUMEN_vs_ESPERADO' $v $exp.rows
    $sd = Retry { $pv.Workbook.Worksheets.Item('Por rack') }
    "E.PV_J2_valor_cacheado = " + (Retry { $sd.Range('J2').Value2 }) + " esperado=6"
    Retry { $pv.Close() }
}
finally {
    try { while ($xl.Workbooks.Count -gt 0) { $xl.Workbooks.Item(1).Close($false) } } catch { }
    try { $xl.Quit() } catch { }
    [void][Runtime.InteropServices.Marshal]::ReleaseComObject($xl)
    [GC]::Collect(); [GC]::WaitForPendingFinalizers()
    Start-Sleep -Milliseconds 1500
    foreach ($id in @(Get-Process EXCEL -ErrorAction SilentlyContinue | ForEach-Object Id)) { if ($before -notcontains $id) { Stop-Process -Id $id -Force; "CLEANUP: detenida instancia propia $id" } }
}
$newErr = @(Get-ChildItem -Path $env:TEMP -Filter 'error*.xml' -ErrorAction SilentlyContinue | Where-Object { $errBefore -notcontains $_.FullName })
"REPAIR_LOGS_NUEVOS = $($newErr.Count)"
foreach ($f in $newErr) { Get-Content -LiteralPath $f.FullName -Raw }
