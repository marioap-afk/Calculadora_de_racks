#Requires -Version 7.0
# I-64 preparacion tras 785a. Solo lectura (RegQueryInfoKey): posicion de ventana que AutoCAD guardo al salir, para saber en que monitor
# abrira, y LastWrite de la clave. No escribe nada en el registro. Con la posicion a la izquierda de x = 0, AutoCAD abre en el monitor secundario.
$sig = @'
using System;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;
public static class KeyLw2 {
  [DllImport("advapi32.dll", CharSet = CharSet.Unicode)]
  static extern int RegQueryInfoKey(SafeRegistryHandle hKey, IntPtr cls, IntPtr clsLen, IntPtr reserved,
    IntPtr subKeys, IntPtr maxSubKeyLen, IntPtr maxClassLen, IntPtr values, IntPtr maxValueNameLen,
    IntPtr maxValueLen, IntPtr secDesc, out long lastWrite);
  public static string Get(Microsoft.Win32.RegistryKey k) {
    long ft; int r = RegQueryInfoKey(k.Handle, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero,
      IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, out ft);
    if (r != 0) return "ERR" + r;
    return DateTime.FromFileTimeUtc(ft).ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
  }
}
'@
Add-Type -TypeDefinition $sig
"UTC $([DateTime]::UtcNow.ToString('o')); acad = $(@(Get-Process -Name acad -ErrorAction SilentlyContinue).Count)"
$key = [Microsoft.Win32.Registry]::CurrentUser.OpenSubKey('Software\Autodesk\AutoCAD\R25.0\ACAD-8101:409\FixedProfile\Application Window', $false)
if ($null -eq $key) { 'clave no encontrada'; return }
"LastWrite = $([KeyLw2]::Get($key))"
foreach ($n in $key.GetValueNames()) { "$n = $($key.GetValue($n))" }
$key.Dispose()
