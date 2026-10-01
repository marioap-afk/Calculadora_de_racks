# Static scan of the phase 2 production scripts (library for tests/Run-AllTests.ps1; also usable by hand:
#   pwsh -NoProfile -File .\tests\StaticScan.ps1 -Files <script>...   exit 0 = clean, 2 = findings).
#
# WHAT THIS SCAN IS. A deny-by-default guard with TWO closed allowlists and a regex layer on top:
#   A. COMMANDS (allowed-commands.txt): every command the production scripts invoke must be listed, with constrained parameters for the
#      commands that matter (Get-CimInstance, Invoke-CimMethod, Get-ChildItem, New-Object, ForEach-Object, ...). Parameters are read from
#      the AST, never from the command text, so a comment cannot hide or fake them.
#   B. .NET SURFACE (allowed-dotnet.txt): every type literal, static member, instance member (method or property name) and assigned
#      member that appears in the production scripts must be listed. A dynamic member name (`::$m`, `.('a'+'b')`), a static call on
#      something that is not a type literal (`$t::X`), a type given as a string to -as/-is, a New-Object type that is not a literal, a
#      `using`/class definition, a write to a provider drive variable (`${env:X} = 1`) and the variables $ExecutionContext/$Host/$PSCmdlet/
#      $MyInvocation are findings. The list was derived from the production scripts and reviewed line by line; ANY new .NET member used
#      by a production script is a finding until a reviewer adds it to the list (the list is in HASHES-PHASE2.txt).
#   C. Regular expressions over the comment-blanked source (strings are NOT blanked: a forbidden token inside a string is a finding on
#      purpose). This layer is a DRIFT GUARD against accidental additions and a second net; it is not the proof.
# THE PROOF is the line-by-line human review of the three production scripts plus layers A and B being closed: with A and B closed, a
# member or command that is not on the lists cannot be reached by naming it. Residual (documented in the README): property READS and
# method names are allowlisted by NAME only, not per type; a name on the list is allowed on every object.
# Exemptions are positional: the write primitives (CreateNew / FileAccess.Write / the instance members Write and Flush) are allowed only
# inside the extent of the single function Write-Phase2NewFile, and load-command words only inside Get-Phase2LoadLines.
# Round 2: (1) the comment channel `#Requires` is closed (a `#Requires -Modules <path>` makes pwsh import and run that module BEFORE the
# script body starts; only the exact text `#Requires -Version 7.0` is allowed, in the raw text AND in ScriptRequirements); (2) the CALLS of
# the single writer are pinned: only Write-Phase2RecordAndSidecar may call Write-Phase2NewFile, with exactly two call texts; (3) the
# load-command rule uses "not a letter or digit" boundaries (the "_NETLOAD" echo of AutoCAD), not the regex word-boundary escape.

param(
    [string[]]$Files,
    [string]$AllowedCommandsFile = (Join-Path -Path (Split-Path -Parent $PSScriptRoot) -ChildPath 'allowed-commands.txt')
)

Set-StrictMode -Version Latest

$script:WriterFunction = 'Write-Phase2NewFile'
$script:NetloadReaderFunction = 'Get-Phase2LoadLines'
$script:ScanLanguage = 'System.Management.Automation.Language'

# id, regex (case-insensitive), scope: ALWAYS | NOT_IN_WRITER | NOT_IN_NETLOAD_READER
$script:ScanRules = @(
    @('START_PROCESS', '\bStart-Process\b|\bsaps\b|\bStart-Job\b|\bStart-ThreadJob\b', 'ALWAYS'),
    @('INVOKE_EXPRESSION', '\bInvoke-Expression\b|\biex\b', 'ALWAYS'),
    @('CALL_ACAD', '&\s*[''"]?[^;|)''"\r\n]*acad', 'ALWAYS'),
    @('NETLOAD_OUTSIDE_TRANSCRIPT_READER', '(?<![A-Za-z0-9])(NETLOAD|APPLOAD|ARXLOAD|ARXUNLOAD|DBXLOAD|VLLOAD|FASLOAD|VLRUN|VBALOAD|VBARUN|STARTAPP)(?![A-Za-z0-9])|(?<=[_.])(RUNSCRIPT|RSCRIPT|SCRIPT)(?![A-Za-z0-9])|\(\s*(load|arxload|autoload|vl-load-all|vl-vbaload|vl-vbarun|startapp)(?![A-Za-z0-9])', 'NOT_IN_NETLOAD_READER'),
    @('AUTOCAD_AUTOMATION', 'SendCommand|-ComObject|GetActiveObject|AutoCAD\.Application|\bacad\.exe\b', 'ALWAYS'),
    @('PROCESS_START_API', '\bProcessStartInfo\b|\.Start\s*\(|\[\s*(System\.)?Diagnostics\.Process\s*\]\s*::\s*Start', 'ALWAYS'),
    @('PROCESS_EFFECT_API', '\b(CloseMainWindow|PriorityClass|ProcessorAffinity|MinWorkingSet|MaxWorkingSet|EnableRaisingEvents)\b', 'ALWAYS'),
    @('REGISTRY_WRITE_CMDLET', '\b(Set-ItemProperty|New-ItemProperty|Remove-ItemProperty|Rename-ItemProperty|Clear-ItemProperty|Copy-ItemProperty|Move-ItemProperty)\b', 'ALWAYS'),
    @('REGISTRY_WRITE_API', '(\.|::)\s*(CreateSubKey|SetValue|DeleteValue|DeleteSubKey|DeleteSubKeyTree)\s*\(', 'ALWAYS'),
    @('REMOVE_ITEM', '\bRemove-Item\b', 'ALWAYS'),
    @('WRITE_CMDLET_OUTSIDE_WRITER', '\b(Set-Content|Add-Content|Out-File|Clear-Content|New-Item|Copy-Item|Move-Item|Rename-Item|Tee-Object|Export-Csv|Export-Clixml)\b', 'NOT_IN_WRITER'),
    @('REG_EXE', '\breg(\.exe)?\s+(add|delete|import|copy|save|restore|load|unload)\b', 'ALWAYS'),
    @('ICACLS', '\bicacls(\.exe)?\b|\bcacls\b|\bxcacls\b', 'ALWAYS'),
    @('TAKEOWN', '\btakeown(\.exe)?\b', 'ALWAYS'),
    @('ATTRIB', '\battrib(\.exe)?\b', 'ALWAYS'),
    @('ACL_API', '\bSet-Acl\b|\bSetAccessControl\b|\bSetOwner\b|\bSetSecurityDescriptor', 'ALWAYS'),
    @('ADD_TYPE', '\bAdd-Type\b', 'ALWAYS'),
    @('ASSEMBLY_LOAD', 'Reflection\.Assembly|\bLoadLibrary\b|\bDllImport\b|InteropServices|\bAssembly\s*\]\s*::\s*Load', 'ALWAYS'),
    @('CODE_EXECUTION_API', '\bScriptBlock\s*\]|\bInvokeScript\b|\bInvokeCommand\b|\bExecutionContext\b|\bCreateNestedPipeline\b|\bAddScript\b|\bCreateRunspace\b|\bGetMethod\b|\bInvokeMember\b|\bGetType\s*\(', 'ALWAYS'),
    @('WEB_ACCESS', '\b(Invoke-WebRequest|Invoke-RestMethod|iwr|irm|Start-BitsTransfer|WebClient|HttpClient|TcpClient|UdpClient|TcpListener|HttpWebRequest|WebRequest)\b|\bSystem\.Net\.|\bNet\.Sockets\b', 'ALWAYS'),
    @('SERVICE_CONTROL', '\b(New-Service|Set-Service|Start-Service|Stop-Service|Restart-Service|Remove-Service)\b|\bsc(\.exe)?\s+(create|config|delete|start|stop)\b', 'ALWAYS'),
    @('SCHEDULED_TASK', '\bschtasks(\.exe)?\b|\bRegister-ScheduledTask\b|\bNew-ScheduledTask\w*\b|\bUnregister-ScheduledTask\b', 'ALWAYS'),
    @('STOP_OR_KILL_PROCESS', '\bStop-Process\b|\bkill\b|\.Kill\s*\(|\btaskkill\b|\bspps\b', 'ALWAYS'),
    @('REMOTE_OR_SHELL_EXEC', '\bInvoke-Command\b|\bicm\b|\bInvoke-Item\b|\bwscript\b|\bcscript\b|\bmshta\b|\brundll32\b|\bEnter-PSSession\b|\bNew-PSSession\b', 'ALWAYS'),
    @('FILE_MUTATING_API', '\[\s*(System\.)?IO\.File\s*\]\s*::\s*(Create|CreateText|WriteAllText|WriteAllBytes|WriteAllLines|AppendAllText|AppendAllLines|AppendText|Delete|Move|Copy|Replace|SetAttributes|Encrypt|Decrypt|Open|OpenWrite|OpenHandle|CreateSymbolicLink|SetCreationTime|SetLastWriteTime|SetLastAccessTime)\b|\[\s*(System\.)?IO\.Directory\s*\]\s*::\s*(CreateDirectory|Delete|Move|SetCurrentDirectory|SetCreationTime|SetLastWriteTime)\b|\[\s*(System\.)?IO\.Path\s*\]\s*::\s*GetTempFileName\b|\.(Delete|MoveTo|CopyTo|CreateSubdirectory|Create|CreateText|AppendText|OpenWrite|CreateAsSymbolicLink)\s*\(|\b(StreamWriter|BinaryWriter|FileInfo|DirectoryInfo|CreateSymbolicLink|CreateHardLink)\b', 'ALWAYS'),
    @('FILE_MODE_NOT_READ_OR_CREATENEW', 'FileMode\s*\]\s*::\s*(Create|Truncate|Append|OpenOrCreate)\b', 'ALWAYS'),
    @('WRITE_PRIMITIVE_OUTSIDE_WRITER', 'FileMode\s*\]\s*::\s*CreateNew\b|FileAccess\s*\]\s*::\s*(Write|ReadWrite)\b', 'NOT_IN_WRITER'),
    @('ENVIRONMENT_WRITE', '\bSetEnvironmentVariable\b|\$env:\w+\s*=|\$\{env:[^}]*\}\s*=|\[\s*(System\.)?Environment\s*\]\s*::\s*(CurrentDirectory|Exit|FailFast)\b|\bSet-Location\b|\bPush-Location\b|\bSet-Variable\b', 'ALWAYS')
)

# Per-command parameter rules. Allowed = the only named parameters; Switches = parameters that take no value; Const = a named
# parameter whose value must be a constant string; Required = named parameters that must be present; MaxPositional = -1 means any.
# Commands not listed here are only checked against allowed-commands.txt.
$script:CommandParamRules = @{
    'get-ciminstance'           = @{ Allowed = @('ClassName', 'Filter'); Switches = @(); Const = @{ ClassName = 'Win32_Process' }; Required = @('ClassName'); MaxPositional = 0; RuleId = 'CIM_CLASS_NOT_WIN32_PROCESS' }
    'invoke-cimmethod'          = @{ Allowed = @('InputObject', 'MethodName'); Switches = @(); Const = @{ MethodName = 'GetOwner' }; Required = @('MethodName'); MaxPositional = 0; RuleId = 'CIM_METHOD_NOT_GETOWNER' }
    'get-childitem'             = @{ Allowed = @('LiteralPath', 'Force'); Switches = @('Force'); Const = @{}; Required = @('LiteralPath'); MaxPositional = 0; RuleId = 'GET_CHILDITEM_WITHOUT_LITERALPATH' }
    'get-authenticodesignature' = @{ Allowed = @('LiteralPath'); Switches = @(); Const = @{}; Required = @('LiteralPath'); MaxPositional = 0; RuleId = 'COMMAND_PARAMETER_NOT_ALLOWED' }
    'new-object'                = @{ Allowed = @('TypeName', 'ArgumentList'); Switches = @(); Const = @{}; Required = @(); MaxPositional = 2; RuleId = 'COMMAND_PARAMETER_NOT_ALLOWED' }
    'foreach-object'            = @{ Allowed = @(); Switches = @(); Const = @{}; Required = @(); MaxPositional = 1; ScriptBlockPositional = $true; RuleId = 'FOREACH_OBJECT_NOT_A_SCRIPTBLOCK' }
    'where-object'              = @{ Allowed = @(); Switches = @(); Const = @{}; Required = @(); MaxPositional = 1; ScriptBlockPositional = $true; RuleId = 'COMMAND_PARAMETER_NOT_ALLOWED' }
    'select-object'             = @{ Allowed = @('First'); Switches = @(); Const = @{}; Required = @(); MaxPositional = 0; RuleId = 'COMMAND_PARAMETER_NOT_ALLOWED' }
    'join-path'                 = @{ Allowed = @('Path', 'ChildPath'); Switches = @(); Const = @{}; Required = @(); MaxPositional = 0; RuleId = 'COMMAND_PARAMETER_NOT_ALLOWED' }
    'out-null'                  = @{ Allowed = @(); Switches = @(); Const = @{}; Required = @(); MaxPositional = 0; RuleId = 'COMMAND_PARAMETER_NOT_ALLOWED' }
    'write-output'              = @{ Allowed = @(); Switches = @(); Const = @{}; Required = @(); MaxPositional = -1; RuleId = 'COMMAND_PARAMETER_NOT_ALLOWED' }
    'set-strictmode'            = @{ Allowed = @('Version'); Switches = @(); Const = @{}; Required = @('Version'); MaxPositional = 0; RuleId = 'COMMAND_PARAMETER_NOT_ALLOWED' }
}

# Round 2: the only text allowed after a `#requires` marker anywhere in a production script, and the only two call texts of the writer.
$script:AllowedRequiresText = '#Requires -Version 7.0'
$script:WriterCallerFunction = 'Write-Phase2RecordAndSidecar'
$script:WriterCallTexts = @(
    'Write-Phase2NewFile -RootFullPath $RootFullPath -Name $recordName -Bytes $RecordBytes',
    'Write-Phase2NewFile -RootFullPath $RootFullPath -Name $sidecarName -Bytes $sidecarBytes'
)
# New-Object types that are on the .NET list for their static members but must never be instantiated.
$script:NewObjectForbiddenTypes = @('diagnostics.process', 'diagnostics.processstartinfo')

$script:ForbiddenAutomaticVariables = @('executioncontext', 'host', 'pscmdlet', 'myinvocation', 'psdefaultparametervalues', 'pshome', 'shellid')
# Round 3 (review 1 M2): automatic variables whose WRITE is a code-load or environment channel. The dot-source text is pinned, but what
# `$PSScriptRoot` points to is not: `$PSScriptRoot = <other dir>` made the pinned dot-source text load an unscanned library. A write to any
# of these (assignment, multi-assignment, ++/--, foreach variable, parameter of that name; scope prefix stripped) is a finding.
$script:ForbiddenAssignedVariables = @('psscriptroot', 'pscommandpath', 'pwd', 'args', 'input', 'home', 'pid', 'profile', 'psversiontable', 'psboundparameters', 'psedition', 'psdefaultparametervalues')
# Round 4 (review 1 m1): the common parameters that capture or buffer output into a NAMED VARIABLE (a write channel to any variable, automatic ones
# included: `-ErrorVariable PSScriptRoot`). Full names and documented aliases; any prefix of two or more letters is a finding on every command,
# a one-letter prefix on every command except the library functions (which are simple functions: the common parameters do not exist there).
$script:CommonVariableParameters = @('errorvariable', 'ev', 'warningvariable', 'wv', 'informationvariable', 'iv', 'outvariable', 'ov', 'pipelinevariable', 'pv', 'outbuffer', 'ob')
# Round 3 (review 1 m1): the public write wrapper is called once, at the top level of Capture-Phase2.ps1, with exactly this text.
$script:WriterWrapperFunction = 'Write-Phase2RecordAndSidecar'
$script:WriterWrapperCallerFile = 'Capture-Phase2.ps1'
$script:WriterWrapperCallText = 'Write-Phase2RecordAndSidecar -RootFullPath $root -RunId $RunId -RecordBytes $bytes'
# Round 3 (review 1 m2): outside lib/Phase2Common.ps1 the only function definitions are the two tiny exit helpers (one per script).
$script:AllowedScriptFunctions = @{ 'capture-phase2.ps1' = @('Exit-Phase2Usage'); 'validate-phase2.ps1' = @('Exit-Phase2ValidateUsage') }
$script:AllowedRefVariable = 'l'
$script:AllowedFileShareMembers = @('None', 'Read', 'Write', 'ReadWrite', 'Delete')

function Get-Phase2ScanCommandAllowlist {
    param([string]$Path)
    $set = New-Object 'System.Collections.Generic.HashSet[string]' ([System.StringComparer]::OrdinalIgnoreCase)
    foreach ($l in [System.IO.File]::ReadAllLines($Path)) {
        $t = ($l -split '#')[0].Trim()
        if ($t.Length -eq 0) { continue }
        [void]$set.Add($t)
    }
    return , $set
}

# Normalized type key: lower case, no spaces, one leading "system." removed (so [IO.File] and [System.IO.File] are the same key).
function ConvertTo-ScanTypeKey {
    param([string]$Name)
    $k = $Name.Trim().Replace(' ', '').ToLowerInvariant()
    if ($k.StartsWith('system.')) { $k = $k.Substring(7) }
    return $k
}

# allowed-dotnet.txt: [types] / [static] / [members] / [assignable]; '#' starts a comment. A [members] line may end in @FunctionName
# (the member is allowed only inside that function).
function Read-Phase2ScanDotnetList {
    param([string]$Path)
    $res = @{
        types      = (New-Object 'System.Collections.Generic.HashSet[string]' ([System.StringComparer]::Ordinal))
        statics    = (New-Object 'System.Collections.Generic.HashSet[string]' ([System.StringComparer]::Ordinal))
        members    = (New-Object System.Collections.Hashtable ([System.StringComparer]::Ordinal))
        assignable = (New-Object 'System.Collections.Generic.HashSet[string]' ([System.StringComparer]::Ordinal))
    }
    $section = ''
    foreach ($l in [System.IO.File]::ReadAllLines($Path)) {
        $t = ($l -split '#')[0].Trim()
        if ($t.Length -eq 0) { continue }
        if ($t -match '^\[(types|static|members|assignable)\]$') { $section = $Matches[1]; continue }
        switch ($section) {
            'types' { [void]$res.types.Add((ConvertTo-ScanTypeKey $t)) }
            'static' { [void]$res.statics.Add($t.ToLowerInvariant()) }
            'assignable' { [void]$res.assignable.Add($t.ToLowerInvariant()) }
            'members' {
                $name = $t; $scope = '*'
                if ($t.Contains('@')) { $name = $t.Substring(0, $t.IndexOf('@')); $scope = $t.Substring($t.IndexOf('@') + 1) }
                $key = $name.ToLowerInvariant()
                if (-not $res.members.ContainsKey($key)) { $res.members[$key] = New-Object 'System.Collections.Generic.List[string]' }
                $res.members[$key].Add($scope)
            }
            default { }
        }
    }
    return $res
}

function Get-ScanEnclosingFunctionName {
    param($Node)
    $p = $Node.Parent
    while ($null -ne $p) {
        if ($p -is [System.Management.Automation.Language.FunctionDefinitionAst]) { return $p.Name }
        $p = $p.Parent
    }
    return $null
}

# Splits a command's elements (after the name) into named parameters and positional elements, reading the AST only.
function Get-ScanCommandArguments {
    param($Command, [string[]]$Switches)
    $named = New-Object System.Collections.Hashtable ([System.StringComparer]::OrdinalIgnoreCase)
    $positional = New-Object 'System.Collections.Generic.List[object]'
    $els = @($Command.CommandElements)
    $i = 1
    while ($i -lt $els.Count) {
        $el = $els[$i]
        if ($el -is [System.Management.Automation.Language.CommandParameterAst]) {
            $pn = $el.ParameterName
            if ($null -ne $el.Argument) { $named[$pn] = $el.Argument }
            elseif ($Switches -contains $pn) { $named[$pn] = $true }
            elseif ($i + 1 -lt $els.Count -and $els[$i + 1] -isnot [System.Management.Automation.Language.CommandParameterAst]) { $named[$pn] = $els[$i + 1]; $i++ }
            else { $named[$pn] = $true }
        } else { $positional.Add($el) }
        $i++
    }
    return [pscustomobject]@{ Named = $named; Positional = $positional.ToArray() }
}

# The elements of an argument list given to New-Object: ParenExpression, array literal or a single expression.
function Get-ScanArgumentListElements {
    param($Node)
    if ($Node -is [System.Management.Automation.Language.ParenExpressionAst]) {
        $pipe = $Node.Pipeline
        if ($pipe -is [System.Management.Automation.Language.PipelineAst] -and @($pipe.PipelineElements).Count -eq 1 -and $pipe.PipelineElements[0] -is [System.Management.Automation.Language.CommandExpressionAst]) {
            return (Get-ScanArgumentListElements -Node $pipe.PipelineElements[0].Expression)
        }
        return [pscustomobject]@{ Items = @($Node) }
    }
    if ($Node -is [System.Management.Automation.Language.ArrayLiteralAst]) { return [pscustomobject]@{ Items = @($Node.Elements) } }
    return [pscustomobject]@{ Items = @($Node) }
}

function Test-ScanStaticMemberOf {
    param($Node, [string]$TypeKey, [string[]]$Members)
    if ($Node -isnot [System.Management.Automation.Language.MemberExpressionAst] -or -not $Node.Static) { return $false }
    if ($Node.Expression -isnot [System.Management.Automation.Language.TypeExpressionAst]) { return $false }
    if ($Node.Member -isnot [System.Management.Automation.Language.StringConstantExpressionAst]) { return $false }
    if ((ConvertTo-ScanTypeKey $Node.Expression.TypeName.FullName) -cne $TypeKey) { return $false }
    return ($Members -contains $Node.Member.Value)
}

# Round 3 (review 1 M1): the nodes a statement WRITES to. Unwraps casts, attributes, parentheses, array literals (multi-assignment:
# `$a, $f[0].Attributes = 0, 1`) and index targets (`$o.Items[0] = 1` writes a member's collection). Returns member and variable nodes.
function Get-ScanAssignTargets {
    param($Node)
    $out = New-Object 'System.Collections.Generic.List[object]'
    $stack = New-Object 'System.Collections.Generic.Stack[object]'
    $stack.Push($Node)
    while ($stack.Count -gt 0) {
        $n = $stack.Pop()
        if ($null -eq $n) { continue }
        if ($n -is [System.Management.Automation.Language.ConvertExpressionAst]) { $stack.Push($n.Child) }
        elseif ($n -is [System.Management.Automation.Language.AttributedExpressionAst]) { $stack.Push($n.Child) }
        elseif ($n -is [System.Management.Automation.Language.ParenExpressionAst]) {
            $pipe = $n.Pipeline
            if ($pipe -is [System.Management.Automation.Language.PipelineAst] -and @($pipe.PipelineElements).Count -eq 1 -and $pipe.PipelineElements[0] -is [System.Management.Automation.Language.CommandExpressionAst]) { $stack.Push($pipe.PipelineElements[0].Expression) }
        }
        elseif ($n -is [System.Management.Automation.Language.ArrayLiteralAst]) { foreach ($e in @($n.Elements)) { $stack.Push($e) } }
        elseif ($n -is [System.Management.Automation.Language.IndexExpressionAst]) { $stack.Push($n.Target) }
        elseif ($n -is [System.Management.Automation.Language.MemberExpressionAst]) { $out.Add($n) }
        elseif ($n -is [System.Management.Automation.Language.VariableExpressionAst]) { $out.Add($n) }
    }
    return , $out.ToArray()
}

function Add-ScanWriteTargetFindings {
    param($Findings, [string]$Path, [int]$Line, $Targets, $Dn, [string]$Kind)
    foreach ($t in $Targets) {
        if ($t -is [System.Management.Automation.Language.VariableExpressionAst]) {
            $vn = ($t.VariablePath.UserPath.ToLowerInvariant() -replace '^(script|global|local|private|using):', '')
            if ($script:ForbiddenAssignedVariables -contains $vn) { $Findings.Add('FORBIDDEN_VARIABLE_ASSIGNMENT|' + $Path + ':' + $Line + '|' + $t.Extent.Text) }
        }
        elseif ($t -is [System.Management.Automation.Language.MemberExpressionAst]) {
            if ($Kind -eq 'INCREMENT') { $Findings.Add('INCREMENT_OF_MEMBER_FORBIDDEN|' + $Path + ':' + $Line + '|' + $t.Extent.Text) }
            elseif ($t.Static) { $Findings.Add('STATIC_ASSIGNMENT_FORBIDDEN|' + $Path + ':' + $Line + '|' + $t.Extent.Text) }
            elseif ($t.Member -isnot [System.Management.Automation.Language.StringConstantExpressionAst] -or -not $Dn.assignable.Contains($t.Member.Value.ToLowerInvariant())) { $Findings.Add('MEMBER_ASSIGNMENT_NOT_ALLOWED|' + $Path + ':' + $Line + '|' + $t.Extent.Text) }
        }
    }
}

# Round 3 (review 1 m3): the fourth FileStream argument must be a FileShare literal (or a -bor of literals).
function Test-ScanFileShareExpression {
    param($Node)
    if ($Node -is [System.Management.Automation.Language.ParenExpressionAst]) {
        $pipe = $Node.Pipeline
        if ($pipe -is [System.Management.Automation.Language.PipelineAst] -and @($pipe.PipelineElements).Count -eq 1 -and $pipe.PipelineElements[0] -is [System.Management.Automation.Language.CommandExpressionAst]) { return (Test-ScanFileShareExpression -Node $pipe.PipelineElements[0].Expression) }
        return $false
    }
    if ($Node -is [System.Management.Automation.Language.BinaryExpressionAst]) {
        if ($Node.Operator.ToString() -ne 'Bor') { return $false }
        return ((Test-ScanFileShareExpression -Node $Node.Left) -and (Test-ScanFileShareExpression -Node $Node.Right))
    }
    return (Test-ScanStaticMemberOf $Node 'io.fileshare' $script:AllowedFileShareMembers)
}

# -AllowFixtureFunctions is for the unit-test snippets only (they define the pinned library functions to exercise the other rules); the
# command line mode and the production set never pass it.
function Invoke-Phase2StaticScan {
    param([string]$Path, [string]$AllowedCommands, [string]$DotnetList, [switch]$AllowFixtureFunctions)
    $findings = New-Object 'System.Collections.Generic.List[string]'
    $allow = Get-Phase2ScanCommandAllowlist -Path $AllowedCommands
    $builtinCommands = Get-Phase2ScanCommandAllowlist -Path $AllowedCommands
    $libFunctionNames = New-Object 'System.Collections.Generic.HashSet[string]' ([System.StringComparer]::OrdinalIgnoreCase)
    if ([string]::IsNullOrEmpty($DotnetList)) { $DotnetList = Join-Path -Path (Split-Path -Parent $AllowedCommands) -ChildPath 'allowed-dotnet.txt' }
    $dn = Read-Phase2ScanDotnetList -Path $DotnetList
    $libFile = Join-Path -Path (Split-Path -Parent $AllowedCommands) -ChildPath 'lib/Phase2Common.ps1'
    if ([System.IO.File]::Exists($libFile)) {
        $lt = $null; $le = $null
        $lastLib = [System.Management.Automation.Language.Parser]::ParseFile($libFile, [ref]$lt, [ref]$le)
        foreach ($lf in $lastLib.FindAll({ param($n) $n -is [System.Management.Automation.Language.FunctionDefinitionAst] }, $true)) { [void]$allow.Add($lf.Name); [void]$libFunctionNames.Add($lf.Name) }
    }
    $tokens = $null; $perr = $null
    $ast = [System.Management.Automation.Language.Parser]::ParseFile($Path, [ref]$tokens, [ref]$perr)
    if ($perr.Count -gt 0) { $findings.Add('PARSE_ERROR|' + $Path + '|' + $perr[0].Message); return , $findings.ToArray() }
    $text = [System.IO.File]::ReadAllText($Path)

    # Comment-blanked copy with identical offsets.
    $chars = $text.ToCharArray()
    foreach ($t in $tokens) {
        if ($t.Kind -eq [System.Management.Automation.Language.TokenKind]::Comment) {
            for ($i = $t.Extent.StartOffset; $i -lt $t.Extent.EndOffset; $i++) { if ($chars[$i] -ne "`n") { $chars[$i] = ' ' } }
        }
    }
    $clean = New-Object string (, $chars)

    # Function extents.
    $funcs = $ast.FindAll({ param($n) $n -is [System.Management.Automation.Language.FunctionDefinitionAst] }, $true)
    foreach ($f0 in $funcs) { [void]$allow.Add($f0.Name) }
    $writerExt = @(); $netExt = @(); $writerCount = 0
    foreach ($f in $funcs) {
        if ($f.Name -eq $script:WriterFunction) { $writerExt += , @($f.Extent.StartOffset, $f.Extent.EndOffset); $writerCount++ }
        if ($f.Name -eq $script:NetloadReaderFunction) { $netExt += , @($f.Extent.StartOffset, $f.Extent.EndOffset) }
    }
    $inside = {
        param($offset, $extents)
        foreach ($e in $extents) { if ($offset -ge $e[0] -and $offset -lt $e[1]) { return $true } }
        return $false
    }

    # Layer C: token regexes (drift guard).
    foreach ($rule in $script:ScanRules) {
        $ms = [regex]::Matches($clean, $rule[1], [System.Text.RegularExpressions.RegexOptions]::IgnoreCase)
        foreach ($m in $ms) {
            $exempt = $false
            if ($rule[2] -eq 'NOT_IN_WRITER') { $exempt = $inside.InvokeReturnAsIs($m.Index, $writerExt) }
            elseif ($rule[2] -eq 'NOT_IN_NETLOAD_READER') { $exempt = $inside.InvokeReturnAsIs($m.Index, $netExt) }
            if (-not $exempt) {
                $line = 1 + ($clean.Substring(0, $m.Index) -split "`n").Count - 1
                $findings.Add($rule[0] + '|' + $Path + ':' + $line + '|' + $m.Value.Trim())
            }
        }
    }

    # Layer C2 (round 2, review 1 M1): the `#requires` comment channel. pwsh acts on a `#requires` line comment (also one that follows code
    # on the same line) BEFORE the script body runs: `#requires -Modules <path>` imports and runs that module. The RAW text is scanned
    # (strings and block comments included, conservatively): every `#requires` marker must be exactly the allowed text.
    foreach ($m in [regex]::Matches($text, '#[ \t]*requires\b[^\r\n]*', [System.Text.RegularExpressions.RegexOptions]::IgnoreCase)) {
        if ($m.Value.TrimEnd() -cne $script:AllowedRequiresText) {
            $line = 1 + ($text.Substring(0, $m.Index) -split "`n").Count - 1
            $findings.Add('REQUIRES_NOT_EXACT|' + $Path + ':' + $line + '|' + $m.Value.Trim())
        }
    }
    $sr = $ast.ScriptRequirements
    if ($null -ne $sr) {
        $srBad = (@($sr.RequiredModules).Count -gt 0) -or (@($sr.RequiredAssemblies).Count -gt 0) -or (@($sr.RequiredPSEditions).Count -gt 0) -or (-not [string]::IsNullOrEmpty([string]$sr.RequiredApplicationId)) -or ($sr.IsElevationRequired -eq $true)
        if ($srBad) { $findings.Add('REQUIRES_NOT_ALLOWED|' + $Path + '|ScriptRequirements names modules, assemblies, editions, an application id or elevation') }
        if ($null -ne $sr.RequiredPSVersion -and $sr.RequiredPSVersion.ToString() -ne '7.0') { $findings.Add('REQUIRES_NOT_ALLOWED|' + $Path + '|RequiredPSVersion is ' + $sr.RequiredPSVersion.ToString()) }
    }

    $all = $ast.FindAll({ $true }, $true)

    # Layer A: commands.
    foreach ($c in @($all | Where-Object { $_ -is [System.Management.Automation.Language.CommandAst] })) {
        $name = $c.GetCommandName()
        $line = $c.Extent.StartLineNumber
        if ($c.InvocationOperator -eq [System.Management.Automation.Language.TokenKind]::Ampersand) {
            $findings.Add('CALL_OPERATOR|' + $Path + ':' + $line + '|' + $c.Extent.Text); continue
        }
        if ($c.InvocationOperator -eq [System.Management.Automation.Language.TokenKind]::Dot) {
            if ($c.Extent.Text -ne ". (Join-Path -Path `$PSScriptRoot -ChildPath 'lib/Phase2Common.ps1')") { $findings.Add('DOT_SOURCE_NOT_THE_LIBRARY|' + $Path + ':' + $line + '|' + $c.Extent.Text) }
            continue
        }
        if ($null -eq $name) { $findings.Add('DYNAMIC_COMMAND|' + $Path + ':' + $line + '|' + $c.Extent.Text); continue }
        if (-not $allow.Contains($name)) { $findings.Add('COMMAND_NOT_ALLOWED|' + $Path + ':' + $line + '|' + $name) }
        if ($name -ieq $script:WriterFunction) {
            # round 2 (review 1 m1): the writer is called only by the one pinned function, with exactly the two pinned call texts
            if ((Get-ScanEnclosingFunctionName -Node $c) -cne $script:WriterCallerFunction -or $script:WriterCallTexts -cnotcontains $c.Extent.Text) {
                $findings.Add('WRITER_CALL_NOT_PINNED|' + $Path + ':' + $line + '|' + $c.Extent.Text)
            }
        }
        if ($name -ieq $script:WriterWrapperFunction) {
            # round 3 (review 1 m1): the public write wrapper is called once, at the top level of Capture-Phase2.ps1, with one call text
            if ([System.IO.Path]::GetFileName($Path) -cne $script:WriterWrapperCallerFile -or $c.Extent.Text -cne $script:WriterWrapperCallText -or $null -ne (Get-ScanEnclosingFunctionName -Node $c)) {
                $findings.Add('WRITER_WRAPPER_CALL_NOT_PINNED|' + $Path + ':' + $line + '|' + $c.Extent.Text)
            }
        }
        foreach ($el in @($c.CommandElements)) {
            if ($el -is [System.Management.Automation.Language.VariableExpressionAst] -and $el.Splatted) { $findings.Add('SPLAT_NOT_ALLOWED|' + $Path + ':' + $line + '|' + $el.Extent.Text) }
            if ($el -is [System.Management.Automation.Language.CommandParameterAst]) {
                # round 4 (review 1 m1): no output/error/... -Variable or -Buffer common parameter on ANY command, abbreviations included
                $pl = $el.ParameterName.ToLowerInvariant()
                $isLibCall = $libFunctionNames.Contains($name)
                $hitCommon = $false
                foreach ($cp in $script:CommonVariableParameters) { if ($pl.Length -ge 1 -and $cp.StartsWith($pl, [System.StringComparison]::Ordinal) -and ($pl.Length -ge 2 -or -not $isLibCall)) { $hitCommon = $true } }
                if ($hitCommon) { $findings.Add('COMMON_VARIABLE_PARAMETER_FORBIDDEN|' + $Path + ':' + $line + '|' + $name + ' ' + $el.Extent.Text) }
            }
        }
        $rule = $script:CommandParamRules[$name.ToLowerInvariant()]
        if ($null -ne $rule) {
            $ca = Get-ScanCommandArguments -Command $c -Switches $rule.Switches
            foreach ($pn in $ca.Named.psbase.Keys) {
                if ($rule.Allowed -notcontains $pn) { $findings.Add('COMMAND_PARAMETER_NOT_ALLOWED|' + $Path + ':' + $line + '|' + $name + ' -' + $pn) }
            }
            foreach ($req in $rule.Required) {
                if (-not $ca.Named.ContainsKey($req)) { $findings.Add($rule.RuleId + '|' + $Path + ':' + $line + '|' + $name + ' lacks -' + $req) }
            }
            foreach ($ck in $rule.Const.Keys) {
                if ($ca.Named.ContainsKey($ck)) {
                    $v = $ca.Named[$ck]
                    if ($v -isnot [System.Management.Automation.Language.StringConstantExpressionAst] -or $v.Value -ine $rule.Const[$ck]) { $findings.Add($rule.RuleId + '|' + $Path + ':' + $line + '|' + $name + ' -' + $ck + ' is not the constant ' + $rule.Const[$ck]) }
                }
            }
            if ($rule.MaxPositional -ge 0 -and $ca.Positional.Count -gt $rule.MaxPositional) { $findings.Add('COMMAND_PARAMETER_NOT_ALLOWED|' + $Path + ':' + $line + '|' + $name + ' has unexpected positional arguments') }
            if ($rule.ContainsKey('ScriptBlockPositional') -and $ca.Positional.Count -ge 1 -and $ca.Positional[0] -isnot [System.Management.Automation.Language.ScriptBlockExpressionAst]) { $findings.Add($rule.RuleId + '|' + $Path + ':' + $line + '|' + $name + ' argument is not a script block literal (member-name form is forbidden)') }
            if ($name -ieq 'New-Object') {
                $typeNode = $null; $argNode = $null
                if ($ca.Named.ContainsKey('TypeName')) { $typeNode = $ca.Named['TypeName'] } elseif ($ca.Positional.Count -ge 1) { $typeNode = $ca.Positional[0] }
                if ($ca.Named.ContainsKey('ArgumentList')) { $argNode = $ca.Named['ArgumentList'] } elseif ($ca.Positional.Count -ge 2 -and -not $ca.Named.ContainsKey('TypeName')) { $argNode = $ca.Positional[1] } elseif ($ca.Positional.Count -ge 1 -and $ca.Named.ContainsKey('TypeName')) { $argNode = $ca.Positional[0] }
                if ($typeNode -isnot [System.Management.Automation.Language.StringConstantExpressionAst]) { $findings.Add('NEW_OBJECT_TYPE_NOT_LITERAL|' + $Path + ':' + $line + '|' + $c.Extent.Text) }
                else {
                    $tk = ConvertTo-ScanTypeKey $typeNode.Value
                    if (-not $dn.types.Contains($tk)) { $findings.Add('NEW_OBJECT_TYPE_NOT_ALLOWED|' + $Path + ':' + $line + '|' + $typeNode.Value) }
                    if ($script:NewObjectForbiddenTypes -contains $tk) { $findings.Add('NEW_OBJECT_TYPE_NEVER_INSTANTIATED|' + $Path + ':' + $line + '|' + $typeNode.Value) }
                    if ($tk -ceq 'io.filestream') {
                        $fn = Get-ScanEnclosingFunctionName -Node $c
                        $items = @(); if ($null -ne $argNode) { $items = @((Get-ScanArgumentListElements -Node $argNode).Items) }
                        # round 3 (review 1 m3): exactly 3 or 4 arguments, the 4th (when present) a FileShare literal: no buffer size / FileOptions overload
                        $argShape = ($items.Count -eq 3) -or ($items.Count -eq 4 -and (Test-ScanFileShareExpression -Node $items[3]))
                        $readOk = $argShape -and (Test-ScanStaticMemberOf $items[1] 'io.filemode' @('Open')) -and (Test-ScanStaticMemberOf $items[2] 'io.fileaccess' @('Read'))
                        $createOk = $argShape -and ($fn -ceq $script:WriterFunction) -and (Test-ScanStaticMemberOf $items[1] 'io.filemode' @('CreateNew')) -and (Test-ScanStaticMemberOf $items[2] 'io.fileaccess' @('Write'))
                        if (-not ($readOk -or $createOk)) { $findings.Add('FILESTREAM_ARGS_NOT_ALLOWED|' + $Path + ':' + $line + '|' + $c.Extent.Text) }
                    }
                }
            }
        }
    }

    # Layer B: the .NET surface.
    foreach ($n in $all) {
        $line = $n.Extent.StartLineNumber
        if ($n -is [System.Management.Automation.Language.UsingStatementAst] -or $n -is [System.Management.Automation.Language.TypeDefinitionAst]) {
            $findings.Add('USING_OR_TYPE_DEFINITION|' + $Path + ':' + $line + '|' + $n.Extent.Text); continue
        }
        # round 2 (review 1 m3): constructs that read files, define DSC resources or run restricted data code outside the command list
        if ($n -is [System.Management.Automation.Language.SwitchStatementAst] -and ($n.Flags -band [System.Management.Automation.Language.SwitchFlags]::File) -ne 0) {
            $findings.Add('SWITCH_FILE_NOT_ALLOWED|' + $Path + ':' + $line + '|' + $n.Extent.Text); continue
        }
        if ($n -is [System.Management.Automation.Language.DataStatementAst] -or $n -is [System.Management.Automation.Language.ConfigurationDefinitionAst] -or $n -is [System.Management.Automation.Language.DynamicKeywordStatementAst]) {
            $findings.Add('DATA_OR_CONFIGURATION_BLOCK_NOT_ALLOWED|' + $Path + ':' + $line + '|' + $n.Extent.Text); continue
        }
        if ($n -is [System.Management.Automation.Language.TypeExpressionAst]) {
            if (-not $dn.types.Contains((ConvertTo-ScanTypeKey $n.TypeName.FullName))) { $findings.Add('TYPE_NOT_ALLOWED|' + $Path + ':' + $line + '|' + $n.TypeName.FullName) }
        }
        elseif ($n -is [System.Management.Automation.Language.TypeConstraintAst]) {
            $tck = ConvertTo-ScanTypeKey $n.TypeName.FullName
            if (-not $dn.types.Contains($tck)) { $findings.Add('TYPE_NOT_ALLOWED|' + $Path + ':' + $line + '|' + $n.TypeName.FullName) }
            if ($tck -ceq 'ref' -or $tck -ceq 'management.automation.psreference') {
                # round 4 (review 1 M2): [ref] makes the variable an out-parameter target (`TryGetInt64([ref]$PSScriptRoot)` wrote $PSScriptRoot).
                # Allowed ONLY as the cast of the plain local variable $l (the production TryGetInt64 calls); a [ref] parameter type is never allowed.
                $rp = $n.Parent
                $refOk = $rp -is [System.Management.Automation.Language.ConvertExpressionAst] -and $rp.Child -is [System.Management.Automation.Language.VariableExpressionAst] -and $rp.Child.VariablePath.UserPath -ceq $script:AllowedRefVariable
                if (-not $refOk) { $findings.Add('REF_NOT_ALLOWED|' + $Path + ':' + $line + '|' + $rp.Extent.Text) }
            }
        }
        elseif ($n -is [System.Management.Automation.Language.AttributeAst]) {
            if (-not $dn.types.Contains((ConvertTo-ScanTypeKey $n.TypeName.FullName))) { $findings.Add('TYPE_NOT_ALLOWED|' + $Path + ':' + $line + '|attribute ' + $n.TypeName.FullName) }
            # round 4 (review 1 m1): [CmdletBinding()] / [Parameter()] make a function advanced (live common parameters). Attributes are allowed
            # only in the script-level param block of the script itself, never inside a function or a nested script block.
            $ap = $n.Parent; $attrScriptLevel = $true
            while ($null -ne $ap) {
                if ($ap -is [System.Management.Automation.Language.FunctionDefinitionAst]) { $attrScriptLevel = $false; break }
                if ($ap -is [System.Management.Automation.Language.ScriptBlockAst]) { if (-not [object]::ReferenceEquals($ap, $ast)) { $attrScriptLevel = $false }; break }
                $ap = $ap.Parent
            }
            if (-not $attrScriptLevel) { $findings.Add('ADVANCED_ATTRIBUTE_OUTSIDE_SCRIPT_PARAM|' + $Path + ':' + $line + '|' + $n.Extent.Text) }
        }
        elseif ($n -is [System.Management.Automation.Language.BinaryExpressionAst]) {
            $op = $n.Operator.ToString()
            if (($op -eq 'As' -or $op -eq 'Is' -or $op -eq 'IsNot') -and $n.Right -isnot [System.Management.Automation.Language.TypeExpressionAst]) { $findings.Add('TYPE_OPERATOR_RHS_NOT_LITERAL|' + $Path + ':' + $line + '|' + $n.Extent.Text) }
        }
        elseif ($n -is [System.Management.Automation.Language.VariableExpressionAst]) {
            $vp = $n.VariablePath
            $lname = $vp.UserPath.ToLowerInvariant()
            # round 4 (review 1 M1): the scope prefix does not hide the variable (`$global:host`, `$script:PSDefaultParameterValues`)
            $lstripped = ($lname -replace '^(script|global|local|private|using):', '')
            if ($script:ForbiddenAutomaticVariables -contains $lstripped) { $findings.Add('FORBIDDEN_AUTOMATIC_VARIABLE|' + $Path + ':' + $line + '|' + $n.Extent.Text) }
            if ($vp.IsDriveQualified -and $lname -notmatch '^(script|global|local|private|using):') { $findings.Add('PROVIDER_DRIVE_VARIABLE|' + $Path + ':' + $line + '|' + $n.Extent.Text) }
            elseif ($lname.Contains(':') -and $lname -notmatch '^(script|global|local|private|using):') { $findings.Add('PROVIDER_DRIVE_VARIABLE|' + $Path + ':' + $line + '|' + $n.Extent.Text) }
        }
        elseif ($n -is [System.Management.Automation.Language.AssignmentStatementAst]) {
            # round 3 (review 1 M1/M2): every write target of the statement, through casts, parentheses, multi-assignment and index targets
            Add-ScanWriteTargetFindings -Findings $findings -Path $Path -Line $line -Targets (Get-ScanAssignTargets -Node $n.Left) -Dn $dn -Kind 'ASSIGN'
        }
        elseif ($n -is [System.Management.Automation.Language.UnaryExpressionAst]) {
            $tk2 = $n.TokenKind.ToString()
            if ($tk2 -eq 'PlusPlus' -or $tk2 -eq 'MinusMinus' -or $tk2 -eq 'PostfixPlusPlus' -or $tk2 -eq 'PostfixMinusMinus') {
                Add-ScanWriteTargetFindings -Findings $findings -Path $Path -Line $line -Targets (Get-ScanAssignTargets -Node $n.Child) -Dn $dn -Kind 'INCREMENT'
            }
        }
        elseif ($n -is [System.Management.Automation.Language.ForEachStatementAst]) {
            Add-ScanWriteTargetFindings -Findings $findings -Path $Path -Line $line -Targets @($n.Variable) -Dn $dn -Kind 'ASSIGN'
        }
        elseif ($n -is [System.Management.Automation.Language.ParameterAst]) {
            Add-ScanWriteTargetFindings -Findings $findings -Path $Path -Line $line -Targets @($n.Name) -Dn $dn -Kind 'ASSIGN'
        }
        elseif ($n -is [System.Management.Automation.Language.FunctionDefinitionAst]) {
            # round 3 (review 1 m2): a scanned script cannot redefine a library guard (or a listed command) with a different body
            $fname = $n.Name
            $leaf = [System.IO.Path]::GetFileName($Path)
            $isLib = ($leaf -ceq 'Phase2Common.ps1') -and ([System.IO.Path]::GetFileName([System.IO.Path]::GetDirectoryName([System.IO.Path]::GetFullPath($Path))) -ceq 'lib')
            if ($isLib) {
                if ($builtinCommands.Contains($fname)) { $findings.Add('FUNCTION_SHADOWS_LISTED_COMMAND|' + $Path + ':' + $line + '|' + $fname) }
                if (@($funcs | Where-Object { $_.Name -ieq $fname }).Count -gt 1) { $findings.Add('FUNCTION_DEFINED_TWICE|' + $Path + ':' + $line + '|' + $fname) }
            }
            elseif (-not $AllowFixtureFunctions) {
                $okNames = $script:AllowedScriptFunctions[$leaf.ToLowerInvariant()]
                if ($libFunctionNames.Contains($fname)) { $findings.Add('FUNCTION_REDEFINES_LIBRARY_FUNCTION|' + $Path + ':' + $line + '|' + $fname) }
                elseif ($null -eq $okNames -or $okNames -cnotcontains $fname) { $findings.Add('FUNCTION_DEFINITION_NOT_ALLOWED|' + $Path + ':' + $line + '|' + $fname) }
            }
        }
        if ($n -is [System.Management.Automation.Language.MemberExpressionAst]) {
            if ($n.Member -isnot [System.Management.Automation.Language.StringConstantExpressionAst]) {
                $findings.Add('MEMBER_NAME_DYNAMIC|' + $Path + ':' + $line + '|' + $n.Extent.Text); continue
            }
            $mname = $n.Member.Value
            if ($n.Static) {
                if ($n.Expression -isnot [System.Management.Automation.Language.TypeExpressionAst]) { $findings.Add('STATIC_TARGET_NOT_TYPE_LITERAL|' + $Path + ':' + $line + '|' + $n.Extent.Text); continue }
                $key = (ConvertTo-ScanTypeKey $n.Expression.TypeName.FullName) + '::' + $mname.ToLowerInvariant()
                if (-not $dn.statics.Contains($key)) { $findings.Add('STATIC_MEMBER_NOT_ALLOWED|' + $Path + ':' + $line + '|' + $n.Expression.TypeName.FullName + '::' + $mname) }
            } else {
                $scopes = $dn.members[$mname.ToLowerInvariant()]
                $ok = $false
                if ($null -ne $scopes) {
                    $fn = Get-ScanEnclosingFunctionName -Node $n
                    foreach ($sc in $scopes) { if ($sc -ceq '*' -or ($null -ne $fn -and $sc -ceq $fn)) { $ok = $true } }
                }
                if (-not $ok) { $findings.Add('MEMBER_NOT_ALLOWED|' + $Path + ':' + $line + '|' + $mname) }
                if ($mname -ieq 'OpenSubKey' -and $n -is [System.Management.Automation.Language.InvokeMemberExpressionAst]) {
                    $args2 = @($n.Arguments)
                    if ($args2.Count -ne 2 -or $args2[1].Extent.Text -ine '$false') { $findings.Add('REGISTRY_OPEN_NOT_EXPLICIT_READONLY|' + $Path + ':' + $line + '|' + $n.Extent.Text) }
                }
            }
        }
        elseif ($n -is [System.Management.Automation.Language.FileRedirectionAst]) {
            $findings.Add('FILE_REDIRECTION|' + $Path + ':' + $line + '|' + $n.Extent.Text)
        }
    }
    if ($writerCount -gt 1) { $findings.Add('MULTIPLE_WRITER_DEFINITIONS|' + $Path + '|' + $writerCount) }
    return , $findings.ToArray()
}

function Invoke-Phase2StaticScanSet {
    param([string[]]$Paths, [string]$AllowedCommands, [switch]$RequireSingleWriter)
    $all = New-Object 'System.Collections.Generic.List[string]'
    $writers = 0
    $writerCalls = 0
    $wrapperCalls = 0
    foreach ($p in $Paths) {
        foreach ($f in (Invoke-Phase2StaticScan -Path $p -AllowedCommands $AllowedCommands)) { $all.Add($f) }
        $tokens = $null; $perr = $null
        $ast = [System.Management.Automation.Language.Parser]::ParseFile($p, [ref]$tokens, [ref]$perr)
        $writers += @($ast.FindAll({ param($n) $n -is [System.Management.Automation.Language.FunctionDefinitionAst] -and $n.Name -eq 'Write-Phase2NewFile' }, $true)).Count
        $wrapperCalls += @($ast.FindAll({ param($n) $n -is [System.Management.Automation.Language.CommandAst] -and $n.GetCommandName() -eq 'Write-Phase2RecordAndSidecar' }, $true)).Count
        $writerCalls += @($ast.FindAll({ param($n) $n -is [System.Management.Automation.Language.CommandAst] -and $n.GetCommandName() -eq 'Write-Phase2NewFile' }, $true)).Count
    }
    if ($RequireSingleWriter -and $writers -ne 1) { $all.Add('WRITER_COUNT_NOT_ONE|set|' + $writers) }
    if ($RequireSingleWriter -and $writerCalls -ne $script:WriterCallTexts.Count) { $all.Add('WRITER_CALL_COUNT_NOT_PINNED|set|' + $writerCalls) }
    if ($RequireSingleWriter -and $wrapperCalls -ne 1) { $all.Add('WRITER_WRAPPER_CALL_COUNT_NOT_ONE|set|' + $wrapperCalls) }
    return , $all.ToArray()
}

if ($Files -and $Files.Count -gt 0) {
    $Files = @($Files | ForEach-Object { $_ -split ',' } | Where-Object { $_ })
    $res = Invoke-Phase2StaticScanSet -Paths $Files -AllowedCommands $AllowedCommandsFile
    foreach ($r in $res) { Write-Output $r }
    if ($res.Count -gt 0) { exit 2 }
    exit 0
}
