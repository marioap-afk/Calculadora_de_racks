# I-1 OS-LEVEL OBSERVATION SCRIPT  --  PROTOCOL TEXT FOR THE CAD MANAGER.  NOT LOADED, NOT EXECUTED ON ANY HOST BY ANYONE YET.
#
# Authority: I-52 design section 5.2 row I-1 and section 3.2 (decisions sections 237, 238, 239: R-H(b) answered "yes, as typed commands
# and a documented protocol"). It is NOT an instrument: it is text that the CAD manager reviews and runs by hand on the designated
# machine, in 64-bit PowerShell, at a host gate that the Coordinator has authorized. Nothing in this repository runs it.
#
# What it does:
#   * READS: registry values (Get-ItemPropertyValue), operating-system version (Get-CimInstance), file hashes (Get-FileHash), the
#     process module list (Get-Process).
#   * WRITES only NEW files directly under -EvidenceFolder, create-new (FileMode.CreateNew: an existing file is never replaced or
#     appended to), UTF-8 without BOM, LF line endings. This is the disclosed Q-O-0(b) element (a typed command / script, not the
#     EvidenceWriter class of the instruments).
#   * It never starts AutoCAD, never modifies a registry value, a variable, a profile or a product / catalog / library file.
#
# Modes (one per invocation; each writes new files and refuses if a target exists):
#   -Mode MachineStrings   A1-R1 and A1-R2 raw files (MachineGuid, OsVersionBuild)
#   -Mode Session          hashes of acad.exe, the API assemblies, the library and its private copy; process module list
#
# Usage example (reviewed and typed by the CAD manager):
#   .\i1-os-observation.ps1 -Mode MachineStrings -EvidenceFolder 'D:\evidence\HGP-H1-...'
#   .\i1-os-observation.ps1 -Mode Session -EvidenceFolder 'D:\evidence\HGP-H1-...' -Phase SESSION_START -AcadExe 'C:\Program Files\Autodesk\AutoCAD 2025\acad.exe' -AcadPid 1234 -LibraryPath '...' -PrivateCopyPath '...'

[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)][ValidateSet('MachineStrings', 'Session')][string]$Mode,
    [Parameter(Mandatory = $true)][string]$EvidenceFolder,
    [ValidateSet('SESSION_START', 'AFTER_NETLOAD', 'END')][string]$Phase = 'SESSION_START',
    [string]$AcadExe,
    [int]$AcadPid = 0,
    [string]$LibraryPath,
    [string]$PrivateCopyPath
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

if (-not [System.IO.Path]::IsPathRooted($EvidenceFolder) -or -not (Test-Path -LiteralPath $EvidenceFolder -PathType Container)) {
    throw 'EvidenceFolder must be an existing absolute folder (the script creates no folder).'
}

# The ONLY write primitive of this script: a NEW flat file, create-new, UTF-8 without BOM.
function Write-NewEvidenceText {
    param([string]$Name, [string]$Text)
    if ($Name -notmatch '^[A-Za-z0-9][A-Za-z0-9._-]{0,119}$' -or $Name.Contains('..')) { throw "bad evidence file name: $Name" }
    if ($Text.Contains("`r")) { throw "text must be LF only: $Name" }
    $path = Join-Path -Path $EvidenceFolder -ChildPath $Name
    $bytes = (New-Object System.Text.UTF8Encoding($false)).GetBytes($Text)
    $stream = [System.IO.File]::Open($path, [System.IO.FileMode]::CreateNew, [System.IO.FileAccess]::Write, [System.IO.FileShare]::None)
    try { $stream.Write($bytes, 0, $bytes.Length); $stream.Flush($true) } finally { $stream.Dispose() }
}

if ($Mode -eq 'MachineStrings') {
    # A1-R1  MachineGuid: the REG_SZ data exactly as stored, 64-bit registry view.
    $baseKey = [Microsoft.Win32.RegistryKey]::OpenBaseKey([Microsoft.Win32.RegistryHive]::LocalMachine, [Microsoft.Win32.RegistryView]::Registry64)
    $key = $baseKey.OpenSubKey('SOFTWARE\Microsoft\Cryptography')                     # read-only open
    $guid = [string]$key.GetValue('MachineGuid')
    Write-NewEvidenceText -Name 'raw-MachineGuid.txt' -Text ($guid + "`n")

    # A1-R2  OsVersionBuild, proposal (i) accepted by decisions section 239 R-H(a): ONE read of Win32_OperatingSystem.Version,
    # exactly as returned. No join, no UBR.  [HOST-TO-CONFIRM: the form of the returned string, for example 10.0.<build>]
    $version = [string](Get-CimInstance -ClassName Win32_OperatingSystem).Version
    Write-NewEvidenceText -Name 'raw-OsVersionBuild.txt' -Text ($version + "`n")

    # Supplementary build facts (NOT in the label): UBR, DisplayVersion, ProductName.
    $cv = Get-ItemProperty -LiteralPath 'HKLM:\SOFTWARE\Microsoft\Windows NT\CurrentVersion'
    Write-NewEvidenceText -Name 'supplementary-os.txt' -Text ("UBR=$($cv.UBR)`nDisplayVersion=$($cv.DisplayVersion)`nProductName=$($cv.ProductName)`n")
    return
}

# ---- Mode Session: hashes and module list (HF-M8, HF-M9, HF-M10, HF-M11 inputs). Get-* and Get-FileHash only. ----------------------
$suffix = $Phase
function Save-FileHash {
    param([string]$Label, [string]$Path)
    if (-not $Path) { return }
    $h = Get-FileHash -LiteralPath $Path -Algorithm SHA256
    Write-NewEvidenceText -Name "hash-$Label-$suffix.txt" -Text ($h.Hash.ToLowerInvariant() + '  ' + $Path + "`n")
}

if ($AcadExe) {
    Save-FileHash -Label 'acad-exe' -Path $AcadExe
    $dir = Split-Path -Parent $AcadExe
    foreach ($api in 'AcDbMgd.dll', 'AcMgd.dll', 'AcCoreMgd.dll') {
        $p = Join-Path -Path $dir -ChildPath $api
        if (Test-Path -LiteralPath $p) { Save-FileHash -Label ($api -replace '\.dll$', '') -Path $p }
    }
    $info = (Get-Item -LiteralPath $AcadExe).VersionInfo
    Write-NewEvidenceText -Name "version-acad-$suffix.txt" -Text ("FileVersion=$($info.FileVersion)`nProductVersion=$($info.ProductVersion)`n")
}
Save-FileHash -Label 'library' -Path $LibraryPath
Save-FileHash -Label 'private-copy' -Path $PrivateCopyPath

if ($AcadPid -gt 0) {
    $proc = Get-Process -Id $AcadPid
    $lines = New-Object System.Collections.Generic.List[string]
    $lines.Add('pid=' + $proc.Id)
    $lines.Add('startUtc=' + $proc.StartTime.ToUniversalTime().ToString('yyyy-MM-ddTHH:mm:ss.fffZ'))
    foreach ($m in ($proc.Modules | Sort-Object -Property FileName)) {
        $lines.Add('module=' + $m.FileName)
    }
    Write-NewEvidenceText -Name "modules-$suffix.txt" -Text (($lines -join "`n") + "`n")
    # other acad.exe processes (stop condition of design 2.4 S0)
    $others = @(Get-Process -Name 'acad' -ErrorAction SilentlyContinue | Where-Object { $_.Id -ne $AcadPid })
    Write-NewEvidenceText -Name "other-acad-$suffix.txt" -Text ("count=$($others.Count)`n")
}
