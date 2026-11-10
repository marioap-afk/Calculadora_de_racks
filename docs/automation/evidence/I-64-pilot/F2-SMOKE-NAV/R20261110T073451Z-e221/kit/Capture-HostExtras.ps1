#Requires -Version 7.0
<#
  I-64 Smoke-NAV, RunId R20261110T073451Z-e221: read-only capture of the host state that T3C does not cover, by order of the Coordinator
  (OVERNIGHT_OWNER_COORDINATOR_DISPOSITION): AssemblyMap of the active product key and the RackCad entries of the low-integrity
  profile copies (appdatalow\...\CoreUser), plus every RackCad mention and every NETLOAD dialog history under the product key.
  Phase BEFORE runs before the T3C BASELINE; phase AFTER runs after AutoCAD is fully closed and before the T3C POST.
  Read-only: keys are opened with read access only (RegOpenKeyEx KEY_READ); nothing is written to the registry. Writes only OutFile and
  OutFile.sha256. Requires acad.exe = 0.
#>
param(
    [Parameter(Mandatory = $true)][ValidateSet('BEFORE', 'AFTER')][string]$Phase,
    [Parameter(Mandatory = $true)][string]$OutFile
)
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$RunId = 'R20261110T073451Z-e221'

$n = @(Get-Process -Name acad -ErrorAction SilentlyContinue).Count
if ($n -ne 0) { Write-Output "HOST-EXTRAS $Phase NOT TAKEN: acad.exe count = $n (debe ser 0); no se escribio nada."; exit 3 }
if (Test-Path -LiteralPath $OutFile) { Write-Output "HOST-EXTRAS $Phase NOT TAKEN: ya existe (no se repite)."; exit 1 }

Add-Type -TypeDefinition @'
using System;
using System.Runtime.InteropServices;
public static class I64RegLastWrite {
    [DllImport("advapi32.dll", CharSet = CharSet.Unicode)]
    static extern int RegOpenKeyEx(IntPtr hKey, string subKey, int options, int samDesired, out IntPtr result);
    [DllImport("advapi32.dll")]
    static extern int RegQueryInfoKey(IntPtr hKey, IntPtr lpClass, IntPtr lpcClass, IntPtr lpReserved, IntPtr lpcSubKeys,
        IntPtr lpcMaxSubKeyLen, IntPtr lpcMaxClassLen, IntPtr lpcValues, IntPtr lpcMaxValueNameLen, IntPtr lpcMaxValueLen,
        IntPtr lpcbSecurityDescriptor, out long lpftLastWriteTime);
    [DllImport("advapi32.dll")]
    static extern int RegCloseKey(IntPtr hKey);
    public static string Get(string subKey) {
        IntPtr h;
        int r = RegOpenKeyEx(new IntPtr(unchecked((int)0x80000001)), subKey, 0, 0x20019, out h);
        if (r != 0) return "ERR" + r;
        try {
            long ft;
            r = RegQueryInfoKey(h, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero,
                IntPtr.Zero, IntPtr.Zero, out ft);
            return r == 0 ? DateTime.FromFileTimeUtc(ft).ToString("yyyy-MM-ddTHH:mm:ss.fffZ") : "ERR" + r;
        } finally { RegCloseKey(h); }
    }
}
'@

$Hkcu = [Microsoft.Win32.Registry]::CurrentUser
$Product = 'Software\Autodesk\AutoCAD\R25.0\ACAD-8101:409'
$CoreUserRoots = @(
    'Software\appdatalow\software\Autodesk\AutoCAD\R25.0\CoreUser\15FCE767F24F74D1',
    'Software\appdatalow\software\Autodesk\AutoCAD\R25.0\CoreUser\BlockPreviewUser'
)

function ValueText($key, [string]$name) {
    $kind = $key.GetValueKind($name)
    $v = $key.GetValue($name, $null, [Microsoft.Win32.RegistryValueOptions]::DoNotExpandEnvironmentNames)
    $text = switch ($kind) {
        'Binary' { [System.Convert]::ToHexString([byte[]]$v) }
        'MultiString' { ([string[]]$v) -join '|' }
        default { "$v" }
    }
    [ordered]@{ Name = $name; Kind = "$kind"; Data = $text }
}

function KeyRecord([string]$path, $key, [string]$why, [bool]$allValues) {
    $vals = foreach ($vn in ($key.GetValueNames() | Sort-Object { $_ } -Culture ([System.Globalization.CultureInfo]::InvariantCulture))) {
        $vt = ValueText $key $vn
        if ($allValues -or $vn -match '(?i)rackcad' -or $vt.Data -match '(?i)rackcad') { $vt }
    }
    [ordered]@{ Path = $path; Why = $why; LastWriteUtc = [I64RegLastWrite]::Get($path); Values = @($vals) }
}

# Recursive walk: records keys whose path names RackCad (all values), keys holding a value that names RackCad (those values only), and
# every NETLOAD dialog history key (all values). Keys that cannot be opened are recorded, never skipped silently.
function Walk([string]$path, [int]$depth, [System.Collections.Generic.List[object]]$acc, [ref]$count) {
    if ($depth -gt 40) { $acc.Add([ordered]@{ Path = $path; Why = 'DEPTH_LIMIT' }); return }
    $key = $null
    try { $key = $Hkcu.OpenSubKey($path, $false) } catch { $acc.Add([ordered]@{ Path = $path; Why = "OPEN_ERROR $($_.Exception.GetType().Name)" }); return }
    if ($null -eq $key) { return }
    try {
        $count.Value++
        if ($path -match '(?i)\\Dialogs\\NetLoadDialog$') { $acc.Add((KeyRecord $path $key 'NETLOAD_DIALOG_HISTORY' $true)) }
        elseif ($path -match '(?i)rackcad') { $acc.Add((KeyRecord $path $key 'PATH_NAMES_RACKCAD' $true)) }
        else {
            $rec = KeyRecord $path $key 'VALUE_NAMES_RACKCAD' $false
            if (@($rec.Values).Count -gt 0) { $acc.Add($rec) }
        }
        foreach ($sub in ($key.GetSubKeyNames() | Sort-Object { $_ } -Culture ([System.Globalization.CultureInfo]::InvariantCulture))) {
            Walk "$path\$sub" ($depth + 1) $acc $count
        }
    } finally { $key.Dispose() }
}

# 1. AssemblyMap (all values)
$amPath = "$Product\AssemblyMap"
$am = $Hkcu.OpenSubKey($amPath, $false)
$assemblyMap = if ($null -eq $am) { [ordered]@{ Path = $amPath; Exists = $false } } else {
    try {
        $rec = KeyRecord $amPath $am 'ASSEMBLYMAP' $true
        $rp = @($rec.Values | Where-Object { $_.Name -eq 'RackCad.Plugin' })
        [ordered]@{ Path = $amPath; Exists = $true; LastWriteUtc = $rec.LastWriteUtc; RackCadPluginPresent = ($rp.Count -gt 0);
                    RackCadPluginData = $(if ($rp.Count -gt 0) { $rp[0].Data } else { $null }); Values = $rec.Values }
    } finally { $am.Dispose() }
}

# 2. CoreUser copies
$coreUser = foreach ($root in $CoreUserRoots) {
    $acc = [System.Collections.Generic.List[object]]::new(); $c = 0
    $exists = $null -ne ($k = $Hkcu.OpenSubKey($root, $false)); if ($exists) { $k.Dispose(); Walk $root 0 $acc ([ref]$c) }
    [ordered]@{ Root = $root; Exists = $exists; LastWriteUtc = $(if ($exists) { [I64RegLastWrite]::Get($root) } else { $null }); KeysScanned = $c; Entries = @($acc) }
}

# 3. Active product key: every RackCad mention and every NETLOAD dialog history
$acc = [System.Collections.Generic.List[object]]::new(); $c = 0
Walk $Product 0 $acc ([ref]$c)
$product = [ordered]@{ Root = $Product; LastWriteUtc = [I64RegLastWrite]::Get($Product); KeysScanned = $c; Entries = @($acc) }

$doc = [ordered]@{
    Schema = 'rackcad-i64-host-extras/v1'; RunId = $RunId; Phase = $Phase; CapturedUtc = [DateTime]::UtcNow.ToString('o'); AcadCount = $n
    AssemblyMap = $assemblyMap; CoreUser = @($coreUser); ProductKey = $product
}
$json = $doc | ConvertTo-Json -Depth 12
[System.IO.File]::WriteAllText($OutFile, $json + "`n", [System.Text.UTF8Encoding]::new($false))
$sha = (Get-FileHash -LiteralPath $OutFile -Algorithm SHA256).Hash
Set-Content -LiteralPath "$OutFile.sha256" -Value $sha -Encoding utf8NoBOM

Write-Output ("HOST-EXTRAS {0} OK sha256={1}" -f $Phase, $sha)
Write-Output ("  AssemblyMap exists={0} lastWrite={1} RackCad.Plugin={2}" -f $assemblyMap.Exists, $(if ($assemblyMap.Exists) { $assemblyMap.LastWriteUtc } else { '-' }), $(if ($assemblyMap.Exists) { $assemblyMap.RackCadPluginPresent } else { 'False' }))
foreach ($cu in $coreUser) { Write-Output ("  CoreUser {0}: exists={1} keys={2} entries={3}" -f (Split-Path $cu.Root -Leaf), $cu.Exists, $cu.KeysScanned, @($cu.Entries).Count) }
Write-Output ("  ProductKey: keys={0} entries={1}" -f $product.KeysScanned, @($product.Entries).Count)
exit 0
