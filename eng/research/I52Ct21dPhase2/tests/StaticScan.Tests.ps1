# Static scan tests: the production scripts must be clean, and every forbidden token must make a fixture script fail the scan
# (negative controls). Dot-sourced by Run-AllTests.ps1 after TestSupport.ps1.

. (Join-Path -Path $PSScriptRoot -ChildPath 'StaticScan.ps1')

$allowFile = Join-Path -Path $script:ToolRoot -ChildPath 'allowed-commands.txt'

# ======================================================================================================================
Start-TestSection 'static scan: the production scripts'
$prod = @(
    (Join-Path $script:ToolRoot 'lib/Phase2Common.ps1'),
    (Join-Path $script:ToolRoot 'Capture-Phase2.ps1'),
    (Join-Path $script:ToolRoot 'Validate-Phase2.ps1')
)
$onDisk = @(Get-ChildItem -LiteralPath $script:ToolRoot -Recurse -Force -File -Filter '*.ps1' | Where-Object { $_.FullName -notlike (Join-Path $script:ToolRoot 'tests\*') } | ForEach-Object { $_.FullName } | Sort-Object)
Assert-Equal 'every .ps1 outside tests/ is in the scanned production list' (($prod | Sort-Object) -join '|') ($onDisk -join '|')
$findings = Invoke-Phase2StaticScanSet -Paths $prod -AllowedCommands $allowFile -RequireSingleWriter
Assert-Equal 'production scripts: zero findings' 0 $findings.Count
if ($findings.Count -gt 0) { $findings | ForEach-Object { Write-Host ('  finding: ' + $_) } }
$asts = foreach ($f in $prod) { $t = $null; $e = $null; [System.Management.Automation.Language.Parser]::ParseFile($f, [ref]$t, [ref]$e) }
$writerDefs = 0
foreach ($a in $asts) { $writerDefs += @($a.FindAll({ param($n) $n -is [System.Management.Automation.Language.FunctionDefinitionAst] -and $n.Name -eq 'Write-Phase2NewFile' }, $true)).Count }
Assert-Equal 'exactly one writer function in the production scripts' 1 $writerDefs
# every command used by the production scripts is documented in the allowed command list (and every listed command is used)
$used = New-Object 'System.Collections.Generic.HashSet[string]' ([System.StringComparer]::OrdinalIgnoreCase)
foreach ($a in $asts) { foreach ($c in $a.FindAll({ param($n) $n -is [System.Management.Automation.Language.CommandAst] }, $true)) { $n = $c.GetCommandName(); if ($n) { [void]$used.Add($n) } } }
$listed = Get-Phase2ScanCommandAllowlist -Path $allowFile
$unused = @($listed | Where-Object { -not $used.Contains($_) })
Assert-Equal 'no stale entry in allowed-commands.txt' '' ($unused -join ',')
# the .NET allowlist is EXACTLY the surface the production scripts use (closed in both directions: nothing unlisted is used by the scan's
# own rules, and nothing listed is unused, so the list cannot grow silently into a back door)
$dnFile = Join-Path -Path $script:ToolRoot -ChildPath 'allowed-dotnet.txt'
$dnList = Read-Phase2ScanDotnetList -Path $dnFile
$usedTypes = New-Object 'System.Collections.Generic.HashSet[string]'
$usedStatics = New-Object 'System.Collections.Generic.HashSet[string]'
$usedMembers = New-Object 'System.Collections.Generic.HashSet[string]'
$usedAssign = New-Object 'System.Collections.Generic.HashSet[string]'
foreach ($a in $asts) {
    foreach ($n in $a.FindAll({ $true }, $true)) {
        if ($n -is [System.Management.Automation.Language.TypeExpressionAst] -or $n -is [System.Management.Automation.Language.TypeConstraintAst] -or $n -is [System.Management.Automation.Language.AttributeAst]) { [void]$usedTypes.Add((ConvertTo-ScanTypeKey $n.TypeName.FullName)) }
        if ($n -is [System.Management.Automation.Language.CommandAst] -and $n.GetCommandName() -eq 'New-Object' -and $n.CommandElements[1] -is [System.Management.Automation.Language.StringConstantExpressionAst]) { [void]$usedTypes.Add((ConvertTo-ScanTypeKey $n.CommandElements[1].Value)) }
        if ($n -is [System.Management.Automation.Language.MemberExpressionAst]) {
            if ($n.Static) { [void]$usedStatics.Add((ConvertTo-ScanTypeKey $n.Expression.TypeName.FullName) + '::' + $n.Member.Value.ToLowerInvariant()) }
            else { [void]$usedMembers.Add($n.Member.Value.ToLowerInvariant()) }
        }
        if ($n -is [System.Management.Automation.Language.AssignmentStatementAst] -and $n.Left -is [System.Management.Automation.Language.MemberExpressionAst]) { [void]$usedAssign.Add($n.Left.Member.Value.ToLowerInvariant()) }
    }
}
Assert-Equal 'allowed-dotnet.txt: no stale type' '' (@($dnList.types | Where-Object { -not $usedTypes.Contains($_) }) -join ',')
Assert-Equal 'allowed-dotnet.txt: no stale static member' '' (@($dnList.statics | Where-Object { -not $usedStatics.Contains($_) }) -join ',')
Assert-Equal 'allowed-dotnet.txt: no stale instance member' '' (@($dnList.members.psbase.Keys | Where-Object { -not $usedMembers.Contains($_) }) -join ',')
Assert-Equal 'allowed-dotnet.txt: no stale assignable member' '' (@($dnList.assignable | Where-Object { -not $usedAssign.Contains($_) }) -join ',')
Assert-Equal 'allowed-dotnet.txt: every used type is listed' '' (@($usedTypes | Where-Object { -not $dnList.types.Contains($_) }) -join ',')
Assert-Equal 'allowed-dotnet.txt: every used static member is listed' '' (@($usedStatics | Where-Object { -not $dnList.statics.Contains($_) }) -join ',')
Assert-Equal 'allowed-dotnet.txt: every used instance member is listed' '' (@($usedMembers | Where-Object { -not $dnList.members.ContainsKey($_) }) -join ',')
Assert-Equal 'allowed-dotnet.txt: every assigned member is listed as assignable' '' (@($usedAssign | Where-Object { -not $dnList.assignable.Contains($_) }) -join ',')
Assert-True 'allowed-dotnet.txt: the stream members Write and Flush are scoped to the single writer' (($dnList.members['write'] -join ',') -ceq 'Write-Phase2NewFile' -and ($dnList.members['flush'] -join ',') -ceq 'Write-Phase2NewFile')
# none of the dangerous names is on the lists
foreach ($bad in 'start', 'kill', 'create', 'createtext', 'delete', 'moveto', 'copyto', 'setvalue', 'createsubkey', 'closemainwindow', 'invokescript', 'getmethod', 'gettype', 'load', 'loadfrom', 'invokemember', 'writealltext', 'openhandle', 'createsymboliclink', 'streamwriter') {
    Assert-True ('allowed-dotnet.txt: dangerous member name not listed: ' + $bad) (-not $dnList.members.ContainsKey($bad))
}
foreach ($bad in 'type', 'scriptblock', 'activator', 'io.fileinfo', 'io.directoryinfo', 'io.streamwriter', 'reflection.assembly', 'convert', 'runtime.interopservices.marshal', 'net.sockets.tcpclient', 'management.automation.scriptblock') {
    Assert-True ('allowed-dotnet.txt: dangerous type not listed: ' + $bad) (-not $dnList.types.Contains($bad))
}
# no write cmdlet, registry or process API appears anywhere (belt and braces independent of the scanner implementation)
$allText = ($prod | ForEach-Object { [System.IO.File]::ReadAllText($_) }) -join "`n"
foreach ($tok in 'Start-Process', 'Invoke-Expression', 'Set-ItemProperty', 'New-ItemProperty', 'Remove-Item', 'Set-Content', 'Add-Type', 'Invoke-WebRequest', 'Invoke-RestMethod', 'New-Service', 'schtasks', 'Stop-Process', 'icacls', 'takeown', 'Set-Acl') {
    Assert-True ('production text does not contain ' + $tok + ' (even in comments)') (-not $allText.Contains($tok))
}
Assert-True 'production text has no CR' (-not $allText.Contains("`r"))

# ======================================================================================================================
Start-TestSection 'static scan: positive controls (what must NOT be flagged)'
$fx = New-CaseDir
function Scan-Snippet {
    param([string]$Name, [string]$Text)
    $p = Join-Path $fx ($Name + '.ps1')
    [System.IO.File]::WriteAllText($p, $Text, (New-Object System.Text.UTF8Encoding($false)))
    return , (Invoke-Phase2StaticScan -Path $p -AllowedCommands $allowFile -AllowFixtureFunctions)
}
$f = Scan-Snippet 'p-comments' "# Start-Process icacls NETLOAD Set-Content reg add`n<# Invoke-Expression Remove-Item #>`n`$x = 1 # takeown`n"
Assert-Equal 'tokens inside comments are not flagged' 0 $f.Count
$f = Scan-Snippet 'p-writer' "function Write-Phase2NewFile {`n param(`$t)`n `$s = New-Object System.IO.FileStream(`$t, [System.IO.FileMode]::CreateNew, [System.IO.FileAccess]::Write, [System.IO.FileShare]::None)`n `$s.Dispose()`n}`n"
Assert-Equal 'create-new FileStream inside Write-Phase2NewFile is the allowed write primitive' 0 $f.Count
$f = Scan-Snippet 'p-netload' "function Get-Phase2LoadLines {`n param(`$l)`n [regex]::IsMatch(`$l, '\bNETLOAD\b')`n}`n"
Assert-Equal 'the word NETLOAD is allowed inside the transcript reader' 0 $f.Count
$f = Scan-Snippet 'p-read' "`$s = New-Object System.IO.FileStream('C:\x', [System.IO.FileMode]::Open, [System.IO.FileAccess]::Read)`n`$k = [Microsoft.Win32.Registry]::CurrentUser.OpenSubKey('Software\x', `$false)`n[System.IO.File]::ReadAllBytes('C:\x') | Out-Null`n"
Assert-Equal 'read-only file open and read-only registry open are fine' 0 $f.Count
$f = Scan-Snippet 'p-cim' "Get-CimInstance -ClassName Win32_Process -Filter 'ProcessId=1' | Out-Null`nInvoke-CimMethod -InputObject `$o -MethodName GetOwner | Out-Null`n"
Assert-Equal 'the two constrained CIM queries are fine' 0 $f.Count
$f = Scan-Snippet 'p-gci' "Get-ChildItem -LiteralPath 'C:\x' -Force | Out-Null`n"
Assert-Equal 'Get-ChildItem with -LiteralPath is fine' 0 $f.Count

# ======================================================================================================================
Start-TestSection 'static scan: negative controls (each forbidden token must fail)'
$neg = @(
    @('Start-Process', 'Start-Process notepad', 'START_PROCESS'),
    @('Start-Process alias saps', 'saps notepad', 'START_PROCESS'),
    @('Start-Job', 'Start-Job { 1 }', 'START_PROCESS'),
    @('Invoke-Expression', "Invoke-Expression 'dir'", 'INVOKE_EXPRESSION'),
    @('iex alias', "iex 'dir'", 'INVOKE_EXPRESSION'),
    @('call operator on acad.exe path', "& 'C:\Program Files\Autodesk\AutoCAD 2025\acad.exe' /b x.scr", 'CALL_ACAD'),
    @('call operator on bare acad', '& acad', 'CALL_ACAD'),
    @('call operator generic (CALL_OPERATOR)', "& 'C:\Windows\notepad.exe'", 'CALL_OPERATOR'),
    @('call operator on a variable', '$sb = { 1 }; & $sb', 'CALL_OPERATOR'),
    @('dot-source of another file', '. C:\other.ps1', 'DOT_SOURCE_NOT_THE_LIBRARY'),
    @('NETLOAD execution string', "`$cmd = '(command ""NETLOAD"" ""x.dll"")'", 'NETLOAD_OUTSIDE_TRANSCRIPT_READER'),
    @('NETLOAD in a bare string', "'netload x.dll'", 'NETLOAD_OUTSIDE_TRANSCRIPT_READER'),
    @('NETLOAD in another function', "function Other { 'NETLOAD' }", 'NETLOAD_OUTSIDE_TRANSCRIPT_READER'),
    @('SendCommand', '$doc.SendCommand("NETLOAD ")', 'AUTOCAD_AUTOMATION'),
    @('COM object', 'New-Object -ComObject AutoCAD.Application', 'AUTOCAD_AUTOMATION'),
    @('GetActiveObject', '[Runtime.InteropServices.Marshal]::GetActiveObject("AutoCAD.Application")', 'AUTOCAD_AUTOMATION'),
    @('acad.exe literal', "`$p = 'C:\x\acad.exe'", 'AUTOCAD_AUTOMATION'),
    @('Process.Start', "[System.Diagnostics.Process]::Start('x.exe')", 'PROCESS_START_API'),
    @('ProcessStartInfo', '$i = New-Object System.Diagnostics.ProcessStartInfo', 'PROCESS_START_API'),
    @('instance .Start()', '$p.Start()', 'PROCESS_START_API'),
    @('Set-ItemProperty', "Set-ItemProperty -Path HKCU:\x -Name y -Value 1", 'REGISTRY_WRITE_CMDLET'),
    @('New-ItemProperty', "New-ItemProperty -Path HKCU:\x -Name y -Value 1", 'REGISTRY_WRITE_CMDLET'),
    @('Remove-ItemProperty', "Remove-ItemProperty -Path HKCU:\x -Name y", 'REGISTRY_WRITE_CMDLET'),
    @('Remove-Item', 'Remove-Item C:\x -Recurse', 'REMOVE_ITEM'),
    @('Set-Content outside the writer', "Set-Content -Path C:\x -Value 1", 'WRITE_CMDLET_OUTSIDE_WRITER'),
    @('Add-Content', "Add-Content -Path C:\x -Value 1", 'WRITE_CMDLET_OUTSIDE_WRITER'),
    @('Out-File', "'x' | Out-File C:\x", 'WRITE_CMDLET_OUTSIDE_WRITER'),
    @('New-Item', 'New-Item -ItemType Directory C:\x', 'WRITE_CMDLET_OUTSIDE_WRITER'),
    @('Copy-Item', 'Copy-Item C:\a C:\b', 'WRITE_CMDLET_OUTSIDE_WRITER'),
    @('Move-Item', 'Move-Item C:\a C:\b', 'WRITE_CMDLET_OUTSIDE_WRITER'),
    @('Set-Content inside a function that is not THE writer', "function Write-Other { Set-Content C:\x 1 }", 'WRITE_CMDLET_OUTSIDE_WRITER'),
    @('Set-Content not allowed even inside the writer (not on the allowed command list)', "function Write-Phase2NewFile { Set-Content C:\x 1 }", 'COMMAND_NOT_ALLOWED'),
    @('reg add', 'reg add HKCU\Software\x /v y /d 1', 'REG_EXE'),
    @('reg delete', 'reg delete HKCU\Software\x /f', 'REG_EXE'),
    @('reg import', 'reg import x.reg', 'REG_EXE'),
    @('reg.exe add', 'reg.exe add HKCU\Software\x', 'REG_EXE'),
    @('icacls', 'icacls C:\x /grant Everyone:F', 'ICACLS'),
    @('takeown', 'takeown /f C:\x', 'TAKEOWN'),
    @('attrib', 'attrib +r C:\x', 'ATTRIB'),
    @('Set-Acl', 'Set-Acl -Path C:\x -AclObject $a', 'ACL_API'),
    @('SetAccessControl', '$d.SetAccessControl($acl)', 'ACL_API'),
    @('Add-Type', "Add-Type -TypeDefinition 'public class X{}'", 'ADD_TYPE'),
    @('Reflection.Assembly Load', "[Reflection.Assembly]::Load([byte[]]@())", 'ASSEMBLY_LOAD'),
    @('Reflection.Assembly LoadFrom', "[System.Reflection.Assembly]::LoadFrom('C:\x.dll')", 'ASSEMBLY_LOAD'),
    @('DllImport', "[DllImport('kernel32.dll')] param()", 'ASSEMBLY_LOAD'),
    @('Invoke-WebRequest', 'Invoke-WebRequest http://x', 'WEB_ACCESS'),
    @('Invoke-RestMethod', 'Invoke-RestMethod http://x', 'WEB_ACCESS'),
    @('iwr alias', 'iwr http://x', 'WEB_ACCESS'),
    @('WebClient', 'New-Object System.Net.WebClient', 'WEB_ACCESS'),
    @('New-Service', 'New-Service -Name x -BinaryPathName y', 'SERVICE_CONTROL'),
    @('sc.exe create', 'sc.exe create x binPath= y', 'SERVICE_CONTROL'),
    @('schtasks', 'schtasks /create /tn x /tr y', 'SCHEDULED_TASK'),
    @('Register-ScheduledTask', 'Register-ScheduledTask -TaskName x', 'SCHEDULED_TASK'),
    @('Stop-Process', 'Stop-Process -Id 1 -Force', 'STOP_OR_KILL_PROCESS'),
    @('kill alias', 'kill 1', 'STOP_OR_KILL_PROCESS'),
    @('.Kill()', '$p.Kill()', 'STOP_OR_KILL_PROCESS'),
    @('taskkill', 'taskkill /im acad.exe', 'STOP_OR_KILL_PROCESS'),
    @('Invoke-Command', 'Invoke-Command -ScriptBlock { 1 }', 'REMOTE_OR_SHELL_EXEC'),
    @('registry CreateSubKey', '$k.CreateSubKey("x")', 'REGISTRY_WRITE_API'),
    @('registry SetValue', '$k.SetValue("x", 1)', 'REGISTRY_WRITE_API'),
    @('registry DeleteValue', '$k.DeleteValue("x")', 'REGISTRY_WRITE_API'),
    @('registry DeleteSubKeyTree', '$k.DeleteSubKeyTree("x")', 'REGISTRY_WRITE_API'),
    @('OpenSubKey writable', "[Microsoft.Win32.Registry]::CurrentUser.OpenSubKey('Software\x', `$true)", 'REGISTRY_OPEN_NOT_EXPLICIT_READONLY'),
    @('OpenSubKey without the explicit read-only flag', "[Microsoft.Win32.Registry]::CurrentUser.OpenSubKey('Software\x')", 'REGISTRY_OPEN_NOT_EXPLICIT_READONLY'),
    @('File.WriteAllText', "[System.IO.File]::WriteAllText('C:\x', 'y')", 'FILE_MUTATING_API'),
    @('File.WriteAllBytes', "[IO.File]::WriteAllBytes('C:\x', [byte[]]@())", 'FILE_MUTATING_API'),
    @('File.Delete', "[System.IO.File]::Delete('C:\x')", 'FILE_MUTATING_API'),
    @('File.Open', "[System.IO.File]::Open('C:\x', 'Open')", 'FILE_MUTATING_API'),
    @('File.Move', "[System.IO.File]::Move('C:\x','C:\y')", 'FILE_MUTATING_API'),
    @('File.SetAttributes', "[System.IO.File]::SetAttributes('C:\x', 'Hidden')", 'FILE_MUTATING_API'),
    @('Directory.CreateDirectory', "[System.IO.Directory]::CreateDirectory('C:\x')", 'FILE_MUTATING_API'),
    @('Directory.Delete', "[System.IO.Directory]::Delete('C:\x')", 'FILE_MUTATING_API'),
    @('instance .Delete()', '$f.Delete()', 'FILE_MUTATING_API'),
    @('FileMode.Create', "New-Object System.IO.FileStream('C:\x', [System.IO.FileMode]::Create)", 'FILE_MODE_NOT_READ_OR_CREATENEW'),
    @('FileMode.Append', "New-Object System.IO.FileStream('C:\x', [System.IO.FileMode]::Append)", 'FILE_MODE_NOT_READ_OR_CREATENEW'),
    @('FileMode.Truncate', "New-Object System.IO.FileStream('C:\x', [System.IO.FileMode]::Truncate)", 'FILE_MODE_NOT_READ_OR_CREATENEW'),
    @('FileMode.OpenOrCreate', "New-Object System.IO.FileStream('C:\x', [System.IO.FileMode]::OpenOrCreate)", 'FILE_MODE_NOT_READ_OR_CREATENEW'),
    @('FileMode.CreateNew outside the writer', "New-Object System.IO.FileStream('C:\x', [System.IO.FileMode]::CreateNew)", 'WRITE_PRIMITIVE_OUTSIDE_WRITER'),
    @('FileAccess.Write outside the writer', "New-Object System.IO.FileStream('C:\x', [System.IO.FileMode]::Open, [System.IO.FileAccess]::Write)", 'WRITE_PRIMITIVE_OUTSIDE_WRITER'),
    @('FileAccess.ReadWrite outside the writer', "New-Object System.IO.FileStream('C:\x', [System.IO.FileMode]::Open, [System.IO.FileAccess]::ReadWrite)", 'WRITE_PRIMITIVE_OUTSIDE_WRITER'),
    @('write primitive in a function named like the writer but suffixed', "function Write-Phase2NewFile2 { New-Object System.IO.FileStream('C:\x', [System.IO.FileMode]::CreateNew) }", 'WRITE_PRIMITIVE_OUTSIDE_WRITER'),
    @('file redirection', 'Get-Date > C:\x.txt', 'FILE_REDIRECTION'),
    @('file redirection append', 'Get-Date >> C:\x.txt', 'FILE_REDIRECTION'),
    @('Invoke-CimMethod other than GetOwner', 'Invoke-CimMethod -InputObject $o -MethodName Terminate', 'CIM_METHOD_NOT_GETOWNER'),
    @('Get-CimInstance of another class', 'Get-CimInstance -ClassName Win32_Service', 'CIM_CLASS_NOT_WIN32_PROCESS'),
    @('Get-ChildItem without -LiteralPath', 'Get-ChildItem C:\*', 'GET_CHILDITEM_WITHOUT_LITERALPATH'),
    @('a command that is not on the allowed list (deny by default)', 'Get-Process -Id 1', 'COMMAND_NOT_ALLOWED'),
    @('an unlisted alias', 'gci C:\x', 'COMMAND_NOT_ALLOWED'),
    @('a dynamically named command', '$n = "x"; & $n', 'CALL_OPERATOR'),
    @('environment variable set', '$env:X = 1', 'ENVIRONMENT_WRITE'),
    @('SetEnvironmentVariable', "[Environment]::SetEnvironmentVariable('X','1')", 'ENVIRONMENT_WRITE'),
    @('Set-Location', 'Set-Location C:\', 'ENVIRONMENT_WRITE'),
    @('forbidden token inside a string literal (conservative)', "`$s = 'please run Start-Process later'", 'START_PROCESS'),
    @('unparsable script', 'function {', 'PARSE_ERROR')
)
$i = 0
foreach ($c in $neg) {
    $i++
    $f = Scan-Snippet ('n' + $i.ToString('D3')) $c[1]
    $hit = @($f | Where-Object { $_.StartsWith($c[2] + '|') }).Count
    Assert-True ('negative control: ' + $c[0] + ' -> ' + $c[2]) ($hit -gt 0) (($f -join ' ; '))
}
Write-Host ('  ' + $neg.Count + ' negative-control fixtures')
. (Join-Path -Path $PSScriptRoot -ChildPath 'StaticScan.Reproductions.Tests.ps1')
. (Join-Path -Path $PSScriptRoot -ChildPath 'StaticScan.Reproductions2.Tests.ps1')
. (Join-Path -Path $PSScriptRoot -ChildPath 'StaticScan.Reproductions3.Tests.ps1')
. (Join-Path -Path $PSScriptRoot -ChildPath 'StaticScan.Reproductions4.Tests.ps1')

# the writer-count rule
$w1 = Join-Path $fx 'w1.ps1'; $w2 = Join-Path $fx 'w2.ps1'
[System.IO.File]::WriteAllText($w1, "function Write-Phase2NewFile { 1 }`n")
[System.IO.File]::WriteAllText($w2, "function Write-Phase2NewFile { 2 }`n")
Assert-True 'two writer definitions in the set are a finding' (@(Invoke-Phase2StaticScanSet -Paths @($w1, $w2) -AllowedCommands $allowFile -RequireSingleWriter | Where-Object { $_.StartsWith('WRITER_COUNT_NOT_ONE|') }).Count -eq 1)
$w3 = Join-Path $fx 'w3.ps1'; [System.IO.File]::WriteAllText($w3, "`$x = 1`n")
Assert-True 'no writer definition in the set is a finding' (@(Invoke-Phase2StaticScanSet -Paths @($w3) -AllowedCommands $allowFile -RequireSingleWriter | Where-Object { $_.StartsWith('WRITER_COUNT_NOT_ONE|') }).Count -eq 1)
Assert-True 'a duplicate writer in one file is a finding' (@(Scan-Snippet 'w4' "function Write-Phase2NewFile { 1 }`nfunction Write-Phase2NewFile { 2 }`n" | Where-Object { $_.StartsWith('MULTIPLE_WRITER_DEFINITIONS|') }).Count -eq 1)

# the scanner as a command line tool: exit codes
$cli = Join-Path $PSScriptRoot 'StaticScan.ps1'
& (Get-Process -Id $PID).Path -NoProfile -File $cli -Files ($prod -join ',') | Out-Null
Assert-Equal 'scanner CLI: production scripts exit 0' 0 $LASTEXITCODE
& (Get-Process -Id $PID).Path -NoProfile -File $cli -Files (Join-Path $fx 'n001.ps1') | Out-Null
Assert-Equal 'scanner CLI: a negative fixture exits 2' 2 $LASTEXITCODE

# ======================================================================================================================
Start-TestSection 'hash ledger HASHES-PHASE2.txt'
$ledgerPath = Join-Path $script:ToolRoot 'HASHES-PHASE2.txt'
if (-not [System.IO.File]::Exists($ledgerPath)) {
    Write-Host 'NOTE HASHES-PHASE2.txt not present yet: ledger tests skipped (it is generated last)'
} else {
    $lbytes = [System.IO.File]::ReadAllBytes($ledgerPath)
    $ltext = (New-Object System.Text.UTF8Encoding($false, $true)).GetString($lbytes)
    Assert-True 'ledger: LF only, no BOM, ends with LF' (-not $ltext.Contains("`r") -and $lbytes[0] -ne 0xEF -and $ltext.EndsWith("`n"))
    $listedHashes = @{}
    $order = New-Object 'System.Collections.Generic.List[string]'
    foreach ($l in ($ltext.TrimEnd("`n") -split "`n")) {
        if ($l -cmatch '^([0-9a-f]{64})  (\S.*)$') { $listedHashes[$Matches[2]] = $Matches[1]; $order.Add($Matches[2]) } else { Assert-True ('ledger line is well formed: ' + $l) $false }
    }
    $sortedOrder = $order.ToArray(); [System.Array]::Sort($sortedOrder, [System.StringComparer]::Ordinal)
    Assert-Equal 'ledger: sorted by ordinal path' ($sortedOrder -join '|') ($order.ToArray() -join '|')
    $rootLen = $script:ToolRoot.TrimEnd('\').Length + 1
    $actual = @{}
    foreach ($f in (Get-ChildItem -LiteralPath $script:ToolRoot -Recurse -Force -File)) {
        $rel = $f.FullName.Substring($rootLen).Replace('\', '/')
        if ($rel -ceq 'HASHES-PHASE2.txt') { continue }
        $actual[$rel] = Get-Phase2FileSha256 -Path $f.FullName
    }
    Assert-Equal 'ledger: lists exactly the files of the folder' ((@($actual.Keys) | Sort-Object) -join '|') ((@($listedHashes.Keys) | Sort-Object) -join '|')
    $bad = @($actual.Keys | Where-Object { $listedHashes[$_] -cne $actual[$_] })
    Assert-Equal 'ledger: every hash equals the file on disk' '' ($bad -join ',')
    Assert-True 'ledger: no file of the folder has a CR' (@($actual.Keys | Where-Object { ([System.IO.File]::ReadAllText((Join-Path $script:ToolRoot $_))).Contains("`r") }).Count -eq 0)
}
