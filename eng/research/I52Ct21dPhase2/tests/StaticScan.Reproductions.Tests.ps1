# Negative controls that reproduce the review findings of the phase 2 static scan (fixer round 1): the 38 injected forms of the first
# reviewer (23 of them passed the old scan with ZERO findings) plus the forms the closed .NET allowlist now has to stop. Each snippet
# must produce AT LEAST one finding with the named rule id. Dot-sourced by StaticScan.Tests.ps1 (needs Scan-Snippet, $fx, $allowFile).

Start-TestSection 'static scan: reproductions of the review evasions (each must be caught)'

$repro = @(
    # (a) registry write through the static call form
    @('(a) Registry::SetValue static form', "[Microsoft.Win32.Registry]::SetValue('HKEY_CURRENT_USER\Software\x','n','v')", 'REGISTRY_WRITE_API'),
    @('(a) Registry::SetValue static form (AST layer)', "[Microsoft.Win32.Registry]::SetValue('HKEY_CURRENT_USER\Software\x','n','v')", 'STATIC_MEMBER_NOT_ALLOWED'),
    @('(a) Registry LocalMachine hive', "[Microsoft.Win32.Registry]::LocalMachine.OpenSubKey('Software', `$false)", 'STATIC_MEMBER_NOT_ALLOWED'),
    # (b) process creation through CIM, with the allowed argument hidden in an inline comment
    @('(b) Invoke-CimMethod Create with GetOwner in a comment', "Invoke-CimMethod <# -MethodName GetOwner #> -ClassName Win32_Process -MethodName Create -Arguments @{CommandLine='x'}", 'CIM_METHOD_NOT_GETOWNER'),
    @('(b) ... and its extra parameters', "Invoke-CimMethod <# -MethodName GetOwner #> -ClassName Win32_Process -MethodName Create -Arguments @{CommandLine='x'}", 'COMMAND_PARAMETER_NOT_ALLOWED'),
    @('(b) Get-CimInstance Win32_Service with the class hidden in a comment', "Get-CimInstance <# -ClassName Win32_Process #> -ClassName Win32_Service", 'CIM_CLASS_NOT_WIN32_PROCESS'),
    @('(b) Get-ChildItem * with -LiteralPath only in a comment', "Get-ChildItem <# -LiteralPath #> C:\*", 'GET_CHILDITEM_WITHOUT_LITERALPATH'),
    @('(b) Get-CimInstance on a remote computer', "Get-CimInstance -ClassName Win32_Process -ComputerName other", 'COMMAND_PARAMETER_NOT_ALLOWED'),
    # (c) file writes
    @('(c) StreamWriter', "New-Object System.IO.StreamWriter('C:\x\a.txt')", 'NEW_OBJECT_TYPE_NOT_ALLOWED'),
    @('(c) FileStream with a string mode', "New-Object System.IO.FileStream('C:\x\a.txt','Create')", 'FILESTREAM_ARGS_NOT_ALLOWED'),
    @('(c) FileStream with an integer mode', "New-Object System.IO.FileStream('C:\x\a.txt',2)", 'FILESTREAM_ARGS_NOT_ALLOWED'),
    @('(c) FileStream Open without an explicit read access (defaults to read-write)', "New-Object System.IO.FileStream('C:\x\a.txt', [System.IO.FileMode]::Open)", 'FILESTREAM_ARGS_NOT_ALLOWED'),
    @('(c) FileStream Open with a variable access', "New-Object System.IO.FileStream('C:\x\a.txt', [System.IO.FileMode]::Open, `$acc)", 'FILESTREAM_ARGS_NOT_ALLOWED'),
    @('(c) FileStream through -ArgumentList with integers', "New-Object -TypeName System.IO.FileStream -ArgumentList 'C:\x\a.txt', 2, 2", 'FILESTREAM_ARGS_NOT_ALLOWED'),
    @('(c) File::OpenHandle', "[System.IO.File]::OpenHandle('C:\x\a.txt', 'Create')", 'STATIC_MEMBER_NOT_ALLOWED'),
    @('(c) File::CreateSymbolicLink', "[System.IO.File]::CreateSymbolicLink('C:\x\l', 'C:\x\t')", 'STATIC_MEMBER_NOT_ALLOWED'),
    @('(c) Path::GetTempFileName creates a file', "[System.IO.Path]::GetTempFileName()", 'STATIC_MEMBER_NOT_ALLOWED'),
    @('(c) FileInfo cast and Create()', "([System.IO.FileInfo]'C:\x\a').Create()", 'TYPE_NOT_ALLOWED'),
    @('(c) FileInfo cast and Create() (member)', "([System.IO.FileInfo]'C:\x\a').Create()", 'MEMBER_NOT_ALLOWED'),
    @('(c) FileInfo CreateText()', "([System.IO.FileInfo]'C:\x\a').CreateText()", 'MEMBER_NOT_ALLOWED'),
    @('(c) DirectoryInfo Create()', "([System.IO.DirectoryInfo]'C:\x\d').Create()", 'TYPE_NOT_ALLOWED'),
    @('(c) a Get-ChildItem result deleted through the method group', "(Get-ChildItem -LiteralPath 'C:\x').Delete.Invoke()", 'MEMBER_NOT_ALLOWED'),
    @('(c) a Get-ChildItem result deleted', "(Get-ChildItem -LiteralPath 'C:\x').Delete()", 'MEMBER_NOT_ALLOWED'),
    # (d) dynamic or indirect members
    @('(d) dynamic static member name', "`$m='Write'+'AllText'; [System.IO.File]::`$m('C:\x','y')", 'MEMBER_NAME_DYNAMIC'),
    @('(d) static member name built from an expression', "[System.IO.File]::('Write'+'AllText')('C:\x','y')", 'MEMBER_NAME_DYNAMIC'),
    @('(d) type held in a variable', "`$t=[type]'System.IO.File'; `$t::WriteAllText('C:\x','y')", 'STATIC_TARGET_NOT_TYPE_LITERAL'),
    @('(d) the [type] accelerator', "`$t=[type]'System.IO.File'; `$t::WriteAllText('C:\x','y')", 'TYPE_NOT_ALLOWED'),
    @('(d) instance member name built from an expression', "`$p.('St'+'art')()", 'MEMBER_NAME_DYNAMIC'),
    @('(d) property name from a variable', "`$p.`$name", 'MEMBER_NAME_DYNAMIC'),
    @('(d) reflection GetType().GetMethod(...).Invoke', "[type]::GetType('System.Diagnostics.Pro'+'cess').GetMethod('Start').Invoke(`$null, @('x'))", 'TYPE_NOT_ALLOWED'),
    @('(d) reflection through an instance', "`$x.GetType().GetMethod('Start').Invoke(`$null, @('x'))", 'MEMBER_NOT_ALLOWED'),
    # (e) process effects
    @('(e) CloseMainWindow', "[System.Diagnostics.Process]::GetProcessById(1).CloseMainWindow()", 'MEMBER_NOT_ALLOWED'),
    @('(e) CloseMainWindow (regex layer)', "[System.Diagnostics.Process]::GetProcessById(1).CloseMainWindow()", 'PROCESS_EFFECT_API'),
    @('(e) PriorityClass assignment', "`$p.PriorityClass='Idle'", 'MEMBER_ASSIGNMENT_NOT_ALLOWED'),
    @('(e) Process::Start overload', "[System.Diagnostics.Process]::Start('x.exe')", 'STATIC_MEMBER_NOT_ALLOWED'),
    # (f) code execution and load
    @('(f) ScriptBlock::Create().Invoke()', "[scriptblock]::Create('1').Invoke()", 'TYPE_NOT_ALLOWED'),
    @('(f) ScriptBlock::Create (regex layer)', "[scriptblock]::Create('1').Invoke()", 'CODE_EXECUTION_API'),
    @('(f) ExecutionContext.InvokeCommand.InvokeScript', "`$ExecutionContext.InvokeCommand.InvokeScript('1')", 'FORBIDDEN_AUTOMATIC_VARIABLE'),
    @('(f) ExecutionContext (regex layer)', "`$ExecutionContext.InvokeCommand.InvokeScript('1')", 'CODE_EXECUTION_API'),
    @('(f) $Host', "`$Host.UI.RawUI", 'FORBIDDEN_AUTOMATIC_VARIABLE'),
    @('(f) $PSCmdlet', "`$PSCmdlet.InvokeCommand", 'FORBIDDEN_AUTOMATIC_VARIABLE'),
    # (g) environment, working directory, network
    @('(g) braced environment variable assignment', "`${env:X} = 1", 'PROVIDER_DRIVE_VARIABLE'),
    @('(g) braced environment variable assignment (regex layer)', "`${env:X} = 1", 'ENVIRONMENT_WRITE'),
    @('(g) Environment::CurrentDirectory assignment', "[Environment]::CurrentDirectory='C:\'", 'STATIC_ASSIGNMENT_FORBIDDEN'),
    @('(g) Environment::CurrentDirectory (regex layer)', "[Environment]::CurrentDirectory='C:\'", 'ENVIRONMENT_WRITE'),
    @('(g) a function: provider drive variable', "`${function:Foo} = { 1 }", 'PROVIDER_DRIVE_VARIABLE'),
    @('(g) TcpClient', "New-Object Net.Sockets.TcpClient('1.2.3.4',80)", 'NEW_OBJECT_TYPE_NOT_ALLOWED'),
    @('(g) TcpClient (regex layer)', "New-Object Net.Sockets.TcpClient('1.2.3.4',80)", 'WEB_ACCESS'),
    # indirections the closed lists must also stop
    @('string-typed -as', "'C:\x' -as 'System.IO.FileInfo'", 'TYPE_OPERATOR_RHS_NOT_LITERAL'),
    @('string-typed -is', "`$x -is 'System.IO.FileInfo'", 'TYPE_OPERATOR_RHS_NOT_LITERAL'),
    @('New-Object with a variable type', "New-Object -TypeName `$n", 'NEW_OBJECT_TYPE_NOT_LITERAL'),
    @('New-Object with a type that is not on the list', "New-Object System.Net.Sockets.Socket", 'NEW_OBJECT_TYPE_NOT_ALLOWED'),
    @('ForEach-Object with a member name (calls a method by name)', "`$a | ForEach-Object Kill", 'FOREACH_OBJECT_NOT_A_SCRIPTBLOCK'),
    @('ForEach-Object -MemberName', "`$a | ForEach-Object -MemberName Kill", 'COMMAND_PARAMETER_NOT_ALLOWED'),
    @('splatted parameters', "`$h=@{LiteralPath='C:\x'}; Get-ChildItem @h", 'SPLAT_NOT_ALLOWED'),
    @('using namespace', "using namespace System.IO", 'USING_OR_TYPE_DEFINITION'),
    @('class definition', "class A { [void] M() { } }", 'USING_OR_TYPE_DEFINITION'),
    @('static member that is not on the list', "[string]::Join(',', @('a'))", 'STATIC_MEMBER_NOT_ALLOWED'),
    @('type literal that is not on the list', "`$e = [System.Text.Encoding]::UTF8; [System.Convert]::ToBase64String(`$e.GetBytes('x'))", 'TYPE_NOT_ALLOWED'),
    @('Marshal delegate', "[System.Runtime.InteropServices.Marshal]::GetDelegateForFunctionPointer(`$p, [type]'x')", 'TYPE_NOT_ALLOWED'),
    @('Activator', "[Activator]::CreateInstance([type]'System.Diagnostics.Process')", 'TYPE_NOT_ALLOWED')
)

function Assert-ReproductionSet {
    param([object[]]$Set, [string]$Prefix)
    $k = 0
    foreach ($c in $Set) {
        $k++
        $f = Scan-Snippet ($Prefix + $k.ToString('D3')) $c[1]
        $hit = @($f | Where-Object { $_.StartsWith($c[2] + '|') }).Count
        Assert-True ('reproduction: ' + $c[0] + ' -> ' + $c[2]) ($hit -gt 0) (($f -join ' ; '))
    }
}
Assert-ReproductionSet -Set $repro -Prefix 'r'
Write-Host ('  ' + $repro.Count + ' reproduction fixtures')

# every reproduction snippet, taken ALONE, yields at least one finding (none slips through with zero findings)
$zero = @()
$seen = @{}
foreach ($c in $repro) {
    if ($seen.ContainsKey($c[1])) { continue }
    $seen[$c[1]] = 1
    $f = Scan-Snippet ('z' + $seen.Count.ToString('D3')) $c[1]
    if ($f.Count -eq 0) { $zero += $c[0] }
}
Assert-Equal 'no reproduction passes the scan with zero findings' '' ($zero -join ' | ')

# positive controls of the new layer: the forms the production scripts rely on stay clean
$pos = @(
    @('constant string member names', "`$x = 'abc'; `$x.Substring(1); `$x.Length"),
    @('hashtable dot notation', "`$h = @{ Name = 'a' }; `$h.Name"),
    @('script-scoped variable', "`$script:Foo = 1; `$script:Foo"),
    @('read-only registry open', "[Microsoft.Win32.Registry]::CurrentUser.OpenSubKey('Software\x', `$false)"),
    @('read-only FileStream', "New-Object System.IO.FileStream('C:\x', [System.IO.FileMode]::Open, [System.IO.FileAccess]::Read, [System.IO.FileShare]::ReadWrite)"),
    @('typed cast to an allowed type', "[long]'5'; [string]5; [byte[]]@(1)"),
    @('ForEach-Object with a script block', "@(1) | ForEach-Object { `$_ }"),
    @('assignment to an allowed member', "`$o = New-Object System.Text.Json.JsonDocumentOptions; `$o.MaxDepth = 64")
)
foreach ($c in $pos) {
    $f = Scan-Snippet ('pp' + [guid]::NewGuid().ToString('N').Substring(0, 6)) $c[1]
    Assert-Equal ('positive: ' + $c[0]) '' ($f -join ' ; ')
}
