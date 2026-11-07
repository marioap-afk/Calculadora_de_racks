#Requires -Version 7.0
# I-64 preparacion tras 785a. Solo lectura: monitores y ventanas visibles de nivel superior en orden Z (de arriba abajo), con proceso y
# rectangulo en pixeles fisicos. No lee titulos ni contenido de ninguna ventana. No mueve ni toca nada.
Add-Type -TypeDefinition @'
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
public static class TopWin {
  [StructLayout(LayoutKind.Sequential)] public struct RECT { public int L, T, R, B; }
  public delegate bool EnumProc(IntPtr h, IntPtr l);
  [DllImport("user32.dll")] static extern bool EnumWindows(EnumProc p, IntPtr l);
  [DllImport("user32.dll")] static extern bool IsWindowVisible(IntPtr h);
  [DllImport("user32.dll")] static extern bool IsIconic(IntPtr h);
  [DllImport("user32.dll")] static extern int GetWindowLong(IntPtr h, int i);
  [DllImport("user32.dll")] static extern uint GetWindowThreadProcessId(IntPtr h, out uint p);
  [DllImport("dwmapi.dll")] static extern int DwmGetWindowAttribute(IntPtr h, int a, out RECT r, int s);
  [DllImport("dwmapi.dll")] static extern int DwmGetWindowAttribute(IntPtr h, int a, out int v, int s);
  [DllImport("user32.dll")] public static extern bool SetProcessDPIAware();
  public static List<string> List() {
    var o = new List<string>();
    EnumWindows((h, l) => {
      if (!IsWindowVisible(h)) return true;
      int cloaked; DwmGetWindowAttribute(h, 14, out cloaked, 4);
      if (cloaked != 0) return true;
      RECT r; if (DwmGetWindowAttribute(h, 9, out r, 16) != 0) return true;
      if (r.R - r.L < 2 || r.B - r.T < 2) return true;
      uint p; GetWindowThreadProcessId(h, out p);
      o.Add(p + "|" + r.L + "|" + r.T + "|" + r.R + "|" + r.B + "|" + (IsIconic(h) ? "1" : "0"));
      return true;
    }, IntPtr.Zero);
    return o;
  }
}
'@
[void][TopWin]::SetProcessDPIAware()
Add-Type -AssemblyName System.Windows.Forms
"UTC $([DateTime]::UtcNow.ToString('o'))"
"Monitores:"
[System.Windows.Forms.Screen]::AllScreens | ForEach-Object { "  primario=$($_.Primary) x=$($_.Bounds.X) y=$($_.Bounds.Y) w=$($_.Bounds.Width) h=$($_.Bounds.Height)" }
"Ventanas visibles, de arriba abajo (proceso, rectangulo fisico; en marco 1456x819 entre corchetes):"
$f = 1456 / 1920
[TopWin]::List() | ForEach-Object {
    $a = $_ -split '\|'
    $n = try { (Get-Process -Id ([int]$a[0]) -ErrorAction Stop).ProcessName } catch { '?' }
    $min = if ($a[5] -eq '1') { ' minimizada' } else { '' }
    "  {0,-22} x {1}..{2} y {3}..{4}  [{5}..{6} , {7}..{8}]{9}" -f $n, $a[1], $a[3], $a[2], $a[4], [int]($a[1] * $f), [int]($a[3] * $f), [int]($a[2] * $f), [int]($a[4] * $f), $min
}
"acad.exe: " + (@(Get-Process -Name acad -ErrorAction SilentlyContinue | ForEach-Object Id) -join ',')
"RAMElements.exe: " + (@(Get-Process -Name RAMElements -ErrorAction SilentlyContinue | ForEach-Object Id) -join ',')
