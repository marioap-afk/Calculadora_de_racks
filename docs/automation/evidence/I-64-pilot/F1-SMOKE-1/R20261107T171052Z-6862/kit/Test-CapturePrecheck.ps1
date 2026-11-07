#Requires -Version 7.0
<#
  I-64 Smoke-1: precheck de capturas ANTES de SM-00a, con acad.exe = 0 (orden del Coordinator, POST-SMOKE-1 NOT PASS DISPOSITION).
  Sin AutoCAD no hay paleta: esta parte prueba las REGIONES donde aparecera y el MECANISMO de captura. La paleta real se comprueba en SM-03.

  1. Geometria: ninguna ventana del proceso «claude» corta el monitor principal.
  2. Abre en el monitor principal una ventana NORMAL (no siempre encima) de un solo color, en el lugar de AutoCAD.
  3. Captura cada region de T3D con Save-ScreenRegion.ps1 y comprueba que TODOS los pixeles son de ese color: la captura registra lo que
     hay a la vista y no hay nada encima. Las capturas de color liso no contienen datos.
  4. Control negativo: pinta un marco de otro color dentro de una region y exige que la comprobacion lo detecte solo ahi. Asi se demuestra
     que la prueba discrimina.
  No toca el registro, el perfil, AutoCAD ni otras ventanas. Escribe solo OutFile y borra sus PNG temporales.
#>
param(
    [Parameter(Mandatory = $true)][string]$CaptureScript,
    [Parameter(Mandatory = $true)][string]$OutFile
)
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Windows.Forms
Add-Type -AssemblyName System.Drawing
Add-Type -TypeDefinition @'
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
public static class Pc {
  [StructLayout(LayoutKind.Sequential)] public struct RECT { public int L, T, R, B; }
  public delegate bool EnumProc(IntPtr h, IntPtr l);
  [DllImport("user32.dll")] static extern bool EnumWindows(EnumProc p, IntPtr l);
  [DllImport("user32.dll")] static extern bool IsWindowVisible(IntPtr h);
  [DllImport("user32.dll")] static extern uint GetWindowThreadProcessId(IntPtr h, out uint p);
  [DllImport("dwmapi.dll")] static extern int DwmGetWindowAttribute(IntPtr h, int a, out RECT r, int s);
  [DllImport("dwmapi.dll")] static extern int DwmGetWindowAttribute(IntPtr h, int a, out int v, int s);
  [DllImport("user32.dll")] public static extern bool SetProcessDPIAware();
  public static List<string> Windows() {
    var o = new List<string>();
    EnumWindows((h, l) => {
      if (!IsWindowVisible(h)) return true;
      int c; DwmGetWindowAttribute(h, 14, out c, 4); if (c != 0) return true;
      RECT r; if (DwmGetWindowAttribute(h, 9, out r, 16) != 0) return true;
      uint p; GetWindowThreadProcessId(h, out p);
      o.Add(p + "|" + r.L + "|" + r.T + "|" + r.R + "|" + r.B);
      return true;
    }, IntPtr.Zero);
    return o;
  }
}
'@
[void][Pc]::SetProcessDPIAware()
$lines = [System.Collections.Generic.List[string]]::new()
function Say([string]$t) { $lines.Add($t); Write-Output $t }
$primary = [System.Windows.Forms.Screen]::PrimaryScreen.Bounds
Say "capture-precheck UTC $([DateTime]::UtcNow.ToString('o'))"
Say "monitor principal x=$($primary.X) y=$($primary.Y) w=$($primary.Width) h=$($primary.Height)"
$acad = @(Get-Process -Name acad -ErrorAction SilentlyContinue).Count
Say "acad.exe = $acad"
if ($acad -ne 0) { Say 'RESULTADO FAIL: este precheck exige acad.exe = 0'; $lines | Set-Content -LiteralPath $OutFile -Encoding utf8NoBOM; exit 2 }

# 1. Geometria
$bad = 0; $seen = 0
foreach ($w in [Pc]::Windows()) {
    $a = $w -split '\|'
    $n = try { (Get-Process -Id ([int]$a[0]) -ErrorAction Stop).ProcessName } catch { '' }
    if ($n -ne 'claude') { continue }
    $seen++
    $cut = ([int]$a[1] -lt $primary.Right) -and ([int]$a[3] -gt $primary.Left) -and ([int]$a[2] -lt $primary.Bottom) -and ([int]$a[4] -gt $primary.Top)
    Say "ventana claude x $($a[1])..$($a[3]) y $($a[2])..$($a[4]) corta el principal = $cut"
    if ($cut) { $bad++ }
}
Say "1 geometria: ventanas claude = $seen; en el principal = $bad -> $(if ($bad -eq 0 -and $seen -gt 0) { 'PASS' } else { 'FAIL' })"

# Regiones de T3D en el marco 1456x819 (x0,y0,x1,y1)
$regions = [ordered]@{
    'paleta-flotante-arriba-izquierda' = 60, 60, 306, 486
    'paleta-acoplada-izquierda'        = 0, 152, 246, 756
    'paleta-acoplada-derecha'          = 1210, 152, 1456, 756
    'paleta-flotante-centro'           = 630, 292, 876, 718
    'linea-de-ordenes'                 = 0, 678, 760, 757
    'linea-de-ordenes-con-paleta-izq'  = 245, 678, 1005, 757
}
$f = $primary.Width / 1456
$color = [System.Drawing.Color]::FromArgb(255, 12, 34, 56)
$mark = [System.Drawing.Color]::FromArgb(255, 200, 40, 40)
$tmp = Join-Path ([IO.Path]::GetDirectoryName($OutFile)) 'precheck-tmp'
New-Item -ItemType Directory -Force -Path $tmp | Out-Null

function Test-Regions([string]$label) {
    $res = [ordered]@{}
    foreach ($k in $regions.Keys) {
        $r = $regions[$k]
        $x = [int][math]::Round($r[0] * $f); $y = [int][math]::Round($r[1] * $f)
        $wd = [int][math]::Round(($r[2] - $r[0]) * $f); $ht = [int][math]::Round(($r[3] - $r[1]) * $f)
        $png = Join-Path $tmp "$label-$k.png"
        & pwsh -NoProfile -File $CaptureScript -X $x -Y $y -Width $wd -Height $ht -OutFile $png | Out-Null
        $bmp = [System.Drawing.Bitmap]::FromFile($png)
        $diff = 0
        for ($j = 0; $j -lt $bmp.Height; $j += 2) { for ($i = 0; $i -lt $bmp.Width; $i += 2) { if ($bmp.GetPixel($i, $j).ToArgb() -ne $color.ToArgb()) { $diff++ } } }
        $bmp.Dispose()
        $res[$k] = $diff
    }
    return $res
}

$form = [System.Windows.Forms.Form]::new()
$form.FormBorderStyle = 'None'
$form.StartPosition = 'Manual'
$form.Bounds = $primary
$form.BackColor = $color
$form.ShowInTaskbar = $false
$form.TopMost = $false
$panel = [System.Windows.Forms.Panel]::new()
$panel.BackColor = $mark
$cx = [int][math]::Round(1300 * $f); $cy = [int][math]::Round(300 * $f)
$panel.SetBounds($cx, $cy, 60, 60)
$panel.Visible = $false
$form.Controls.Add($panel)
$form.Show()
$form.Activate()
[System.Windows.Forms.Application]::DoEvents()
Start-Sleep -Milliseconds 700
[System.Windows.Forms.Application]::DoEvents()
try {
    # 3. Comprobacion
    $r1 = Test-Regions 'limpia'
    $clean = $true
    foreach ($k in $r1.Keys) { Say "3 region $k : pixeles distintos = $($r1[$k])"; if ($r1[$k] -ne 0) { $clean = $false } }
    Say "3 regiones limpias -> $(if ($clean) { 'PASS' } else { 'FAIL' })"
    # 4. Control negativo
    $panel.Visible = $true
    $form.Refresh(); [System.Windows.Forms.Application]::DoEvents(); Start-Sleep -Milliseconds 400; [System.Windows.Forms.Application]::DoEvents()
    $r2 = Test-Regions 'control'
    $ok = $true
    foreach ($k in $r2.Keys) {
        $expect = ($k -eq 'paleta-acoplada-derecha')
        $hit = ($r2[$k] -gt 0)
        Say "4 control $k : pixeles distintos = $($r2[$k]); esperado distinto = $expect"
        if ($hit -ne $expect) { $ok = $false }
    }
    Say "4 control negativo -> $(if ($ok) { 'PASS' } else { 'FAIL' })"
} finally {
    $form.Close(); $form.Dispose()
    Remove-Item -LiteralPath $tmp -Recurse -Force -ErrorAction SilentlyContinue
}
$pass = ($bad -eq 0 -and $seen -gt 0 -and $clean -and $ok)
Say "RESULTADO $(if ($pass) { 'PASS' } else { 'FAIL' })"
$lines | Set-Content -LiteralPath $OutFile -Encoding utf8NoBOM
if (-not $pass) { exit 1 }
