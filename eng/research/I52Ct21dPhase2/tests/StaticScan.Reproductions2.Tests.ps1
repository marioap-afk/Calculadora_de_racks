# Negative controls that reproduce the review findings of the phase 2 static scan, fixer round 2: the `#requires` comment channel, the
# load-command words with the "_" echo, the pinned calls of the single writer, the constructs found passing in the second review, and the
# evasion forms the reviewers confirmed as caught (kept as regression controls). Dot-sourced by StaticScan.Tests.ps1 (needs Scan-Snippet,
# $fx, $allowFile, Assert-ReproductionSet from the first reproductions file).

Start-TestSection 'static scan round 2: reproductions (each must be caught)'

$mod = 'C:\scratch\m.psm1'
$repro2 = @(
    # --- review 1 M1: the #requires comment channel (pwsh imports and runs the module before the script body) ---
    @('M1 one-line #requires -Modules Foo (the reviewer file)', '#requires -Modules Foo', 'REQUIRES_NOT_EXACT'),
    @('M1 ... and through ScriptRequirements', '#requires -Modules Foo', 'REQUIRES_NOT_ALLOWED'),
    @('M1 #Requires -Modules <path to a .psm1> then code', ("#Requires -Modules $mod`n`$x = 1"), 'REQUIRES_NOT_EXACT'),
    @('M1 ... and through ScriptRequirements (path form)', ("#Requires -Modules $mod`n`$x = 1"), 'REQUIRES_NOT_ALLOWED'),
    @('M1 #requires after code on the same line (pwsh acts on it)', ("`$x = 1 #requires -Modules $mod"), 'REQUIRES_NOT_EXACT'),
    @('M1 indented #requires', ("   #requires -Modules $mod`n`$x = 1"), 'REQUIRES_NOT_EXACT'),
    @('M1 -Version 7.0 followed by -Modules on the same line', ("#requires -Version 7.0 -Modules $mod"), 'REQUIRES_NOT_EXACT'),
    @('M1 -Version 7.0 followed by -Modules (ScriptRequirements)', ("#requires -Version 7.0 -Modules $mod"), 'REQUIRES_NOT_ALLOWED'),
    @('M1 a different version', '#requires -Version 5.1', 'REQUIRES_NOT_EXACT'),
    @('M1 a different version (ScriptRequirements)', '#requires -Version 5.1', 'REQUIRES_NOT_ALLOWED'),
    @('M1 lower-case exact text is not the allowed text', '#requires -Version 7.0', 'REQUIRES_NOT_EXACT'),
    @('M1 -RunAsAdministrator', '#requires -RunAsAdministrator', 'REQUIRES_NOT_ALLOWED'),
    @('M1 -PSEdition', '#Requires -PSEdition Core', 'REQUIRES_NOT_ALLOWED'),
    @('M1 -Assembly', "#Requires -Assembly $mod", 'REQUIRES_NOT_ALLOWED'),
    @('M1 inside a block comment (conservative)', ("<#`n#requires -Modules $mod`n#>`n`$x = 1"), 'REQUIRES_NOT_EXACT'),
    @('M1 with a space after the hash (conservative)', '# requires -Modules Foo', 'REQUIRES_NOT_EXACT'),
    @('M1 upper case', '#REQUIRES -MODULES Foo', 'REQUIRES_NOT_EXACT'),
    @('M1 inside a string (conservative)', "`$s = '#requires -Modules Foo'", 'REQUIRES_NOT_EXACT'),
    # --- review 1 M2: the load-command words in the static rule (a "_netload" string in a production script) ---
    @('M2 "_netload" in a string outside the reader', "`$c = '(command ""_netload"" ""x.dll"")'", 'NETLOAD_OUTSIDE_TRANSCRIPT_READER'),
    @('M2 "^C^C_netload" macro text', "`$c = '^C^C_netload'", 'NETLOAD_OUTSIDE_TRANSCRIPT_READER'),
    @('M2 _appload', "'_appload'", 'NETLOAD_OUTSIDE_TRANSCRIPT_READER'),
    @('M2 vl-load-all form', "'(vl-load-all ""x.lsp"")'", 'NETLOAD_OUTSIDE_TRANSCRIPT_READER'),
    @('M2 startapp form', "'(startapp ""calc.exe"")'", 'NETLOAD_OUTSIDE_TRANSCRIPT_READER'),
    @('M2 _SCRIPT', "'_SCRIPT'", 'NETLOAD_OUTSIDE_TRANSCRIPT_READER'),
    @('M2 _-VBARUN', "'_-VBARUN'", 'NETLOAD_OUTSIDE_TRANSCRIPT_READER'),
    # --- review 1 m1: the calls of the single writer are pinned ---
    @('m1 Write-Phase2NewFile with another root (the reviewer call)', "Write-Phase2NewFile -RootFullPath 'C:\Windows\Temp' -Name 'evil.dll' -Bytes ([byte[]]@(1))", 'WRITER_CALL_NOT_PINNED'),
    @('m1 the pinned text but in another function', "function Other { param(`$RootFullPath,`$recordName,`$RecordBytes) Write-Phase2NewFile -RootFullPath `$RootFullPath -Name `$recordName -Bytes `$RecordBytes }", 'WRITER_CALL_NOT_PINNED'),
    @('m1 the pinned function with a different call', "function Write-Phase2RecordAndSidecar { param(`$r) Write-Phase2NewFile -RootFullPath 'C:\x' -Name 'a.json' -Bytes `$r }", 'WRITER_CALL_NOT_PINNED'),
    @('m1 the pinned function with an extra parameter', "function Write-Phase2RecordAndSidecar { param(`$RootFullPath,`$recordName,`$RecordBytes) Write-Phase2NewFile -RootFullPath `$RootFullPath -Name `$recordName -Bytes `$RecordBytes -Extra 1 }", 'WRITER_CALL_NOT_PINNED'),
    @('m1 a call at the top level of a script', "Write-Phase2NewFile -RootFullPath `$root -Name `$n -Bytes `$b", 'WRITER_CALL_NOT_PINNED'),
    # --- review 1 m3: constructs found passing with zero findings ---
    @('m3 switch -File reads a file', "switch -File C:\x\a.txt { default { 1 } }", 'SWITCH_FILE_NOT_ALLOWED'),
    @('m3 data block', "`$d = data { 'a' }", 'DATA_OR_CONFIGURATION_BLOCK_NOT_ALLOWED'),
    @('m3 configuration block', "configuration C { }", 'DATA_OR_CONFIGURATION_BLOCK_NOT_ALLOWED'),
    @('m3 New-Object System.Diagnostics.Process (type is on the list for GetProcessById)', "`$p = New-Object System.Diagnostics.Process", 'NEW_OBJECT_TYPE_NEVER_INSTANTIATED'),
    @('m3 New-Object ProcessStartInfo', "`$p = New-Object System.Diagnostics.ProcessStartInfo", 'NEW_OBJECT_TYPE_NOT_ALLOWED')
)
Assert-ReproductionSet -Set $repro2 -Prefix 'q'
Write-Host ('  ' + $repro2.Count + ' round 2 reproduction fixtures')

# --- review 1 m9: forms the reviewer tried and confirmed as caught, kept as regression controls (any finding is enough) ---
$caught = @(
    @('alias ni', 'ni C:\x\a.txt'),
    @('alias sc', 'sc C:\x\a.txt 1'),
    @('alias iex', "iex 'dir'"),
    @('alias saps', 'saps notepad'),
    @('alias % with a call', '1 | % { $_.Kill() }'),
    @('alias ? ', '1 | ? { $_ }'),
    @('alias select', '1 | select -First 1'),
    @('native cmd', 'cmd /c dir'),
    @('native ping', 'ping 127.0.0.1'),
    @('native path call', 'C:\Windows\System32\whoami.exe'),
    @('call operator on a variable', '$c = "x"; & $c'),
    @('ForEach-Object { & $_ }', '@("a") | ForEach-Object { & $_ }'),
    @('ForEach-Object Kill', '$a | ForEach-Object Kill'),
    @('[IO.File]::WriteAllText', "[IO.File]::WriteAllText('C:\x', 'y')"),
    @('[Diagnostics.Process]::Start', "[Diagnostics.Process]::Start('x.exe')"),
    @('splatting', '$h = @{ LiteralPath = "C:\x" }; Get-ChildItem @h'),
    @('here-string with a forbidden word', "`$s = @'`nStart-Process x`n'@"),
    @('base64 [Convert]', "[Convert]::FromBase64String('AAAA')"),
    @('module-qualified Set-Content', "Microsoft.PowerShell.Management\Set-Content C:\x 1"),
    @('backtick-split command name', "Set-Con`tent C:\x 1"),
    @('homoglyph command name (Cyrillic o)', ("Set-C" + [char]0x043E + "ntent C:\x 1")),
    @('$env: read and +=', '$env:PATH += ";C:\x"'),
    @('redirection', 'Get-Date > C:\x.txt'),
    @('$k.setvalue lower case', '$k.setvalue("x", 1)'),
    @('Invoke-CimMethod Terminate', 'Invoke-CimMethod -InputObject $o -MethodName Terminate'),
    @('Get-CimInstance -Namespace', 'Get-CimInstance -Namespace root\cimv2 -ClassName Win32_Process'),
    @('abbreviated parameter', 'Get-ChildItem -Lit C:\x'),
    @('[Console]::Error.Write', "[Console]::Error.Write('x')")
)
$ci = 0
foreach ($c in $caught) {
    $ci++
    $f = Scan-Snippet ('c' + $ci.ToString('D3')) $c[1]
    Assert-True ('reviewer-confirmed form is caught (at least one finding): ' + $c[0]) ($f.Count -gt 0) ($c[1])
}
Write-Host ('  ' + $caught.Count + ' regression fixtures of forms already caught')

# --- positive controls of round 2 ---
$pos2 = @(
    @('the exact #Requires line', "#Requires -Version 7.0`n`$x = 1"),
    @('the exact line with trailing spaces', "#Requires -Version 7.0   `n`$x = 1"),
    @('the word requires in a normal comment', "# this function requires a path`n`$x = 1"),
    @('the pinned writer call (first)', "function Write-Phase2RecordAndSidecar { param(`$RootFullPath,`$recordName,`$RecordBytes) `$recordSha = Write-Phase2NewFile -RootFullPath `$RootFullPath -Name `$recordName -Bytes `$RecordBytes }"),
    @('the pinned writer call (second)', "function Write-Phase2RecordAndSidecar { param(`$RootFullPath,`$sidecarName,`$sidecarBytes) [void](Write-Phase2NewFile -RootFullPath `$RootFullPath -Name `$sidecarName -Bytes `$sidecarBytes) }"),
    @('the reader function may hold the load words', "function Get-Phase2LoadLines { param(`$l) [regex]::IsMatch(`$l, '(?<![A-Za-z0-9])_NETLOAD|^C^C_netload|(startapp') }"),
    @('PowerShell script-scope variables are not a SCRIPT command', "`$script:Foo = 1; `$script:Foo"),
    @('a switch without -File', "switch (1) { 1 { 'a' } default { 'b' } }")
)
foreach ($c in $pos2) {
    $f = Scan-Snippet ('pq' + [guid]::NewGuid().ToString('N').Substring(0, 6)) $c[1]
    Assert-Equal ('positive: ' + $c[0]) '' ($f -join ' ; ')
}

# --- parity between the scan rule and the transcript reader over the shared corpus (review 1 M2) ---
$pi = 0
foreach ($line in $script:LoadLinesBoth) {
    $pi++
    $f = Scan-Snippet ('par' + $pi.ToString('D3')) ("`$s = @'`n" + $line + "`n'@`n")
    $hit = @($f | Where-Object { $_.StartsWith('NETLOAD_OUTSIDE_TRANSCRIPT_READER|') }).Count
    Assert-True ('scan rule agrees with the reader (load line): [' + $line + ']') ($hit -gt 0) ($f -join ' ; ')
}
$pi = 0
foreach ($line in $script:LoadLinesClean) {
    $pi++
    $f = Scan-Snippet ('parc' + $pi.ToString('D3')) ("`$s = @'`n" + $line + "`n'@`n")
    $hit = @($f | Where-Object { $_.StartsWith('NETLOAD_OUTSIDE_TRANSCRIPT_READER|') }).Count
    Assert-Equal ('scan rule agrees with the reader (clean line): [' + $line + ']') 0 $hit
}

# --- the set-level pin of the writer calls ---
$sa = Join-Path $fx 'sa.ps1'
[System.IO.File]::WriteAllText($sa, "function Write-Phase2NewFile { 1 }`nfunction Write-Phase2RecordAndSidecar { param(`$RootFullPath,`$recordName,`$RecordBytes) `$r = Write-Phase2NewFile -RootFullPath `$RootFullPath -Name `$recordName -Bytes `$RecordBytes }`n")
Assert-True 'a set with one pinned call instead of the two is a finding' (@(Invoke-Phase2StaticScanSet -Paths @($sa) -AllowedCommands $allowFile -RequireSingleWriter | Where-Object { $_.StartsWith('WRITER_CALL_COUNT_NOT_PINNED|') }).Count -eq 1)
$findingsProd = Invoke-Phase2StaticScanSet -Paths $prod -AllowedCommands $allowFile -RequireSingleWriter
Assert-Equal 'the production set has exactly the two pinned writer calls' 0 $findingsProd.Count
