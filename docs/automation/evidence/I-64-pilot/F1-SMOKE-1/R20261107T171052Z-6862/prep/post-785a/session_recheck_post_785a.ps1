#Requires -Version 7.0
# I-64: recheck de host de SOLO LECTURA tras cerrar AutoCAD, por la orden POST-SMOKE-1 NOT PASS DISPOSITION («obtain a fresh host recheck after closure»).
# No escribe en el registro, el perfil ni el host; solo escribe OutFile (nunca lo sobrescribe). Las rutas locales se redactan.
param([Parameter(Mandatory = $true)][string]$OutFile)
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
Add-Type -TypeDefinition @'
using System;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;
public static class KeyLw3 {
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
$wt = Join-Path $env:USERPROFILE '<worktree-de-I-64>'
$candidate = 'cc7503e3e4e56bff3a2fc9de7ca30a5322b57ea6'
$lines = [System.Collections.Generic.List[string]]::new()
function Say([string]$t) { $lines.Add($t); Write-Output $t }
function Redact([string]$p) { if ($null -eq $p) { return '' }; $p.Replace($wt, '<worktree>').Replace($env:USERPROFILE, '<USERPROFILE>') }
$Utf8 = [System.Text.UTF8Encoding]::new($false)

Say "session-recheck-post-785a UTC $([DateTime]::UtcNow.ToString('o'))"
$acad = @(Get-Process -Name acad -ErrorAction SilentlyContinue)
$ram = @(Get-Process -Name RAMElements -ErrorAction SilentlyContinue)
Say "1 acad.exe count = $($acad.Count); RAMElements.exe count = $($ram.Count)"
foreach ($p in $acad) { Say "  acad pid $($p.Id) start UTC $($p.StartTime.ToUniversalTime().ToString('o'))" }

$head = (git -C $wt rev-parse HEAD).Trim(); $origin = (git -C $wt rev-parse origin/architecture/workspace-persistente-rackcad).Trim()
$porc = @(git -C $wt status --porcelain).Count
$prod = @(git -C $wt diff --name-only $candidate HEAD -- src tests).Count
$proj = @(git -C $wt diff --name-only $candidate HEAD | Where-Object { $_ -match '(?i)\.(csproj|sln|props|targets)$' -or $_ -match '(?i)(^|/)global\.json$' }).Count
Say "2 HEAD=$head origin=$origin porcelain=$porc"
Say "2 diferencia frente al Candidato $candidate : src+tests=$prod proyecto=$proj"
$bin = Join-Path $wt 'src\RackCad.Plugin\bin\Debug\net8.0-windows'
foreach ($n in 'RackCad.Plugin.dll', 'RackCad.Application.dll', 'RackCad.Domain.dll', 'RackCad.UI.dll') {
    $f = Get-Item -LiteralPath (Join-Path $bin $n)
    Say ("2 {0} sha256={1} bytes={2} lastWriteUtc={3}" -f $n, (Get-FileHash -LiteralPath $f.FullName -Algorithm SHA256).Hash, $f.Length, $f.LastWriteTimeUtc.ToString('o'))
}
$pv = (Get-Item -LiteralPath (Join-Path $bin 'RackCad.Plugin.dll')).VersionInfo.ProductVersion
Say "2 RackCad.Plugin.dll ProductVersion=$pv"
$plugins = @(Get-ChildItem -LiteralPath $wt -Recurse -File -Filter 'RackCad.Plugin.dll' -ErrorAction SilentlyContinue | Where-Object { $_.FullName -notmatch '\\obj\\' })
Say "2 RackCad.Plugin.dll fuera de obj en el worktree = $($plugins.Count)"
foreach ($f in $plugins) { Say ("  {0} {1}" -f (Redact $f.FullName), (Get-FileHash -LiteralPath $f.FullName -Algorithm SHA256).Hash) }

$acadExe = '<carpeta-de-AutoCAD-2025>\acad.exe'
if (Test-Path -LiteralPath $acadExe) {
    $vi = (Get-Item -LiteralPath $acadExe).VersionInfo
    Say ("3 acad.exe version={0} sha256={1}" -f $vi.FileVersion, (Get-FileHash -LiteralPath $acadExe -Algorithm SHA256).Hash)
} else { Say "3 acad.exe AUSENTE en la ruta esperada" }

$profKey = 'HKCU\Software\Autodesk\AutoCAD\R25.0\ACAD-8101:409\Profiles'
$profOut = (reg query $profKey /ve 2>&1 | Out-String)
$active = if ($profOut -match '\(\S+\)\s+REG_SZ\s+(.+)') { $Matches[1].Trim() } else { '' }
Say "4 perfil activo = $active"
$vk = [Microsoft.Win32.Registry]::CurrentUser.OpenSubKey("Software\Autodesk\AutoCAD\R25.0\ACAD-8101:409\Profiles\$active\Variables", $false)
$sl = $vk.GetValue('SECURELOAD'); $tp = [string]$vk.GetValue('TRUSTEDPATHS'); $vk.Close()
$tpSha = [System.Convert]::ToHexString([System.Security.Cryptography.SHA256]::HashData($Utf8.GetBytes($tp))).ToLowerInvariant()
$tpCount = @($tp.Split(';') | Where-Object { $_ -ne '' }).Count
Say "4 SECURELOAD = $sl; TRUSTEDPATHS entradas = $tpCount; sha256(utf8 del valor) = $tpSha"

$root = 'Software\Autodesk\AutoCAD\R25.0\ACAD-8101:409'
foreach ($rel in 'Applications', 'Loaded', 'InstalledApplications') {
    $k = [Microsoft.Win32.Registry]::CurrentUser.OpenSubKey("$root\$rel", $false)
    if (-not $k) { Say "5 HKCU $rel AUSENTE"; continue }
    $rc = @($k.GetSubKeyNames() | Where-Object { $_ -match '(?i)rackcad' }) + @($k.GetValueNames() | Where-Object { $_ -match '(?i)rackcad' })
    Say ("5 HKCU {0} subkeys={1} values={2} RackCad={3} lastWrite={4} claves={5}" -f $rel, $k.SubKeyCount, $k.ValueCount, $rc.Count, [KeyLw3]::Get($k), ($k.GetSubKeyNames() -join ','))
    $k.Close()
}
$hklm = [Microsoft.Win32.Registry]::LocalMachine.OpenSubKey("SOFTWARE\Autodesk\AutoCAD\R25.0\ACAD-8101\Applications", $false)
if ($hklm) { Say ("5 HKLM Applications RackCad = {0}" -f @($hklm.GetSubKeyNames() | Where-Object { $_ -match '(?i)rackcad' }).Count); $hklm.Close() } else { Say "5 HKLM Applications AUSENTE o ilegible" }
$am = [Microsoft.Win32.Registry]::CurrentUser.OpenSubKey("$root\AssemblyMap", $false)
if ($am) { Say ("5 AssemblyMap values = {0}; RackCad.Plugin presente = {1}; lastWrite={2}" -f ($am.GetValueNames() -join ','), (@($am.GetValueNames()) -contains 'RackCad.Plugin'), [KeyLw3]::Get($am)); $am.Close() } else { Say "5 AssemblyMap AUSENTE" }

foreach ($d in @((Join-Path $env:APPDATA 'Autodesk\ApplicationPlugins'), (Join-Path $env:ProgramData 'Autodesk\ApplicationPlugins'), '<ApplicationPlugins-de-Program-Files>')) {
    $label = $d.Replace($env:APPDATA, '<APPDATA>').Replace($env:ProgramData, '<ProgramData>').Replace('<ProgramFiles-local>', '<ProgramFiles>')
    if (Test-Path -LiteralPath $d) { Say ("6 ApplicationPlugins {0}: RackCad* = {1}" -f $label, @(Get-ChildItem -LiteralPath $d -Filter '*RackCad*' -ErrorAction SilentlyContinue).Count) }
    else { Say ("6 ApplicationPlugins {0}: AUSENTE" -f $label) }
}
$logs = Join-Path $env:APPDATA 'RackCad\logs'
$last = Get-ChildItem -LiteralPath $logs -File -ErrorAction SilentlyContinue | Sort-Object LastWriteTimeUtc -Descending | Select-Object -First 1
if ($last) { Say ("7 RackLog mas reciente = {0} lastWriteUtc {1} bytes {2}" -f $last.Name, $last.LastWriteTimeUtc.ToString('o'), $last.Length) } else { Say '7 RackLog: sin archivos' }
$sv = @(Get-ChildItem -LiteralPath $env:TEMP -File -Filter '*.sv$' -ErrorAction SilentlyContinue)
Say "7 autoguardados .sv`$ en TEMP = $($sv.Count) (sin nombres)"

$awk = [Microsoft.Win32.Registry]::CurrentUser.OpenSubKey("$root\FixedProfile\Application Window", $false)
if ($awk) { Say ("8 posicion guardada de AutoCAD: Show={0} Position={1} lastWrite={2}" -f $awk.GetValue('Show'), $awk.GetValue('Position'), [KeyLw3]::Get($awk)); $awk.Close() } else { Say '8 posicion guardada: clave no encontrada' }
Say "fin UTC $([DateTime]::UtcNow.ToString('o'))"
if (-not (Test-Path -LiteralPath $OutFile)) { [System.IO.File]::WriteAllLines($OutFile, $lines, $Utf8) } else { Write-Output 'OutFile ya existe: no se sobrescribe' }
