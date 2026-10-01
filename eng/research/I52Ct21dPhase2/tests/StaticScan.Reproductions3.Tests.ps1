# Negative controls that reproduce the review findings of fixer round 3 (review 1 majors M1, M2 and minors m1, m2, m3): member writes
# through ++/-- and multi-assignment, writes to automatic variables that redirect the pinned dot-source, the public write wrapper, function
# redefinitions of library guards, and FileStream overloads with extra arguments. Dot-sourced by StaticScan.Tests.ps1 after the round 2
# file (needs $fx, $allowFile, Scan-Snippet and Assert-Equal / Assert-True). Fixtures here are scanned STRICTLY (no -AllowFixtureFunctions)
# through Scan-SnippetNamed, which also lets a fixture carry the file name of a production script.

Start-TestSection 'static scan round 3: reproductions (each must be caught)'

$script:SnippetDirCounter = 0
function Scan-SnippetNamed {
    param([string]$Leaf, [string]$Text, [string]$SubDir = '')
    $script:SnippetDirCounter++
    $d = Join-Path $fx ('r3-' + $script:SnippetDirCounter.ToString('D4'))
    if ($SubDir) { $d = Join-Path $d $SubDir }
    [void][System.IO.Directory]::CreateDirectory($d)
    $p = Join-Path $d $Leaf
    [System.IO.File]::WriteAllText($p, $Text, (New-Object System.Text.UTF8Encoding($false)))
    return , (Invoke-Phase2StaticScan -Path $p -AllowedCommands $allowFile)
}

# @(label, text, rule id, leaf file name)
$dsRoot = 'Join-Path -Path (Join-Path -Path $PSScriptRoot -ChildPath "evil") -ChildPath ""'
$repro3 = @(
    # --- review 1 M1: member writes that bypassed the assigned-member rule (the seven reproductions of the reviewer) ---
    @('M1(a) foreach { $i.Attributes++ } over a Get-ChildItem result', '$f = Get-ChildItem -LiteralPath "D:\x" -Force; foreach ($i in $f) { $i.Attributes++ }', 'INCREMENT_OF_MEMBER_FORBIDDEN', 'a.ps1'),
    @('M1(b) Get-ChildItem | ForEach-Object { $_.Attributes++ }', 'Get-ChildItem -LiteralPath "D:\x" -Force | ForEach-Object { $_.Attributes++ }', 'INCREMENT_OF_MEMBER_FORBIDDEN', 'a.ps1'),
    @('M1(c) --$f[0].Attributes', '$f = Get-ChildItem -LiteralPath "D:\x" -Force; --$f[0].Attributes', 'INCREMENT_OF_MEMBER_FORBIDDEN', 'a.ps1'),
    @('M1(d) for (...; $f[0].Attributes++) {...}', '$f = Get-ChildItem -LiteralPath "D:\x" -Force; for ($i=0;$i -lt 1;$f[0].Attributes++){$i++}', 'INCREMENT_OF_MEMBER_FORBIDDEN', 'a.ps1'),
    @('M1(e) multi-assignment $null, $f[0].Attributes = 0, 1', '$f = Get-ChildItem -LiteralPath "D:\x" -Force; $null, $f[0].Attributes = 0, 1', 'MEMBER_ASSIGNMENT_NOT_ALLOWED', 'a.ps1'),
    @('M1(f) (Get-ChildItem ...)[0].Attributes--', '(Get-ChildItem -LiteralPath "D:\x" -Force)[0].Attributes--', 'INCREMENT_OF_MEMBER_FORBIDDEN', 'a.ps1'),
    @('M1(g) multi-assignment to a static member $a, [Console]::Error = 1, 2', '$a, [Console]::Error = 1, 2', 'STATIC_ASSIGNMENT_FORBIDDEN', 'a.ps1'),
    # --- more forms of the same family (found while closing the class) ---
    @('M1 prefix ++ on a member', '$f = Get-ChildItem -LiteralPath "D:\x" -Force; ++$f[0].Attributes', 'INCREMENT_OF_MEMBER_FORBIDDEN', 'a.ps1'),
    @('M1 postfix -- on a parenthesized member', '$f = Get-ChildItem -LiteralPath "D:\x" -Force; ($f[0].Attributes)--', 'INCREMENT_OF_MEMBER_FORBIDDEN', 'a.ps1'),
    @('M1 ++ on a property of an allowed name (MaxDepth is assignable, not incrementable)', '$o = New-Object System.Text.Json.JsonDocumentOptions; $o.MaxDepth++', 'INCREMENT_OF_MEMBER_FORBIDDEN', 'a.ps1'),
    @('M1 += on a member', '$f = Get-ChildItem -LiteralPath "D:\x" -Force; $f[0].Attributes += 1', 'MEMBER_ASSIGNMENT_NOT_ALLOWED', 'a.ps1'),
    @('M1 assignment through parentheses', '$f = Get-ChildItem -LiteralPath "D:\x" -Force; ($f[0]).Attributes = 1', 'MEMBER_ASSIGNMENT_NOT_ALLOWED', 'a.ps1'),
    @('M1 assignment through a cast and parentheses', '$f = Get-ChildItem -LiteralPath "D:\x" -Force; [int]($f[0].Attributes) = 1', 'MEMBER_ASSIGNMENT_NOT_ALLOWED', 'a.ps1'),
    @('M1 parenthesized multi-assignment', '$f = Get-ChildItem -LiteralPath "D:\x" -Force; ($null, $f[0].Attributes) = 0, 1', 'MEMBER_ASSIGNMENT_NOT_ALLOWED', 'a.ps1'),
    @('M1 nested multi-assignment', '$f = Get-ChildItem -LiteralPath "D:\x" -Force; $x, ($y, $f[0].Attributes) = 0, 1, 2', 'MEMBER_ASSIGNMENT_NOT_ALLOWED', 'a.ps1'),
    @('M1 multi-assignment to a static member in the first position', '[Console]::Error, $a = 1, 2', 'STATIC_ASSIGNMENT_FORBIDDEN', 'a.ps1'),
    @('M1 write into the collection held by a member', '$f = Get-ChildItem -LiteralPath "D:\x" -Force; $f[0].Attributes[0] = 1', 'MEMBER_ASSIGNMENT_NOT_ALLOWED', 'a.ps1'),
    @('M1 assignment to a dynamic member name inside a multi-assignment', '$m = "Attributes"; $null, $f[0].$m = 0, 1', 'MEMBER_ASSIGNMENT_NOT_ALLOWED', 'a.ps1'),
    # --- review 1 M2: the code-load channel through an assignable automatic variable ---
    @('M2 $PSScriptRoot = <other dir> then the pinned dot-source text (the reviewer t4.ps1)', ('$PSScriptRoot = ' + $dsRoot + "`n" + '. (Join-Path -Path $PSScriptRoot -ChildPath "lib/Phase2Common.ps1")'), 'FORBIDDEN_VARIABLE_ASSIGNMENT', 't4.ps1'),
    @('M2 ... and the dot-source text is the exact pinned one (single-quoted child path)', ('$PSScriptRoot = ' + $dsRoot + "`n" + ". (Join-Path -Path `$PSScriptRoot -ChildPath 'lib/Phase2Common.ps1')"), 'FORBIDDEN_VARIABLE_ASSIGNMENT', 't4.ps1'),
    @('M2 $script:PSScriptRoot =', '$script:PSScriptRoot = "C:\evil"', 'FORBIDDEN_VARIABLE_ASSIGNMENT', 'a.ps1'),
    @('M2 $global:PSScriptRoot =', '$global:PSScriptRoot = "C:\evil"', 'FORBIDDEN_VARIABLE_ASSIGNMENT', 'a.ps1'),
    @('M2 ${PSScriptRoot} =', '${PSScriptRoot} = "C:\evil"', 'FORBIDDEN_VARIABLE_ASSIGNMENT', 'a.ps1'),
    @('M2 lower case $psscriptroot =', '$psscriptroot = "C:\evil"', 'FORBIDDEN_VARIABLE_ASSIGNMENT', 'a.ps1'),
    @('M2 $PSScriptRoot += text', '$PSScriptRoot += "\evil"', 'FORBIDDEN_VARIABLE_ASSIGNMENT', 'a.ps1'),
    @('M2 $PSScriptRoot++', '$PSScriptRoot++', 'FORBIDDEN_VARIABLE_ASSIGNMENT', 'a.ps1'),
    @('M2 typed assignment [string]$PSScriptRoot =', '[string]$PSScriptRoot = "C:\evil"', 'FORBIDDEN_VARIABLE_ASSIGNMENT', 'a.ps1'),
    @('M2 multi-assignment to $PSScriptRoot', '$a, $PSScriptRoot = 1, "C:\evil"', 'FORBIDDEN_VARIABLE_ASSIGNMENT', 'a.ps1'),
    @('M2 foreach variable named PSScriptRoot', 'foreach ($PSScriptRoot in @("C:\evil")) { 1 }', 'FORBIDDEN_VARIABLE_ASSIGNMENT', 'a.ps1'),
    @('M2 script parameter named PSScriptRoot (the caller sets it)', 'param($PSScriptRoot)', 'FORBIDDEN_VARIABLE_ASSIGNMENT', 'a.ps1'),
    @('M2 $PSCommandPath =', '$PSCommandPath = "C:\evil\x.ps1"', 'FORBIDDEN_VARIABLE_ASSIGNMENT', 'a.ps1'),
    @('M2 $pwd =', '$pwd = "C:\evil"', 'FORBIDDEN_VARIABLE_ASSIGNMENT', 'a.ps1'),
    @('M2 $args =', '$args = @("x")', 'FORBIDDEN_VARIABLE_ASSIGNMENT', 'a.ps1'),
    @('M2 $PSBoundParameters =', '$PSBoundParameters = @{}', 'FORBIDDEN_VARIABLE_ASSIGNMENT', 'a.ps1'),
    # --- review 1 m1: the public write wrapper is pinned to one call in Capture-Phase2.ps1 ---
    @('m1 wrapper called from a validator-named script (the reviewer append)', 'Write-Phase2RecordAndSidecar -RootFullPath $root -RunId $RunId -RecordBytes $bytes', 'WRITER_WRAPPER_CALL_NOT_PINNED', 'Validate-Phase2.ps1'),
    @('m1 wrapper called from any other file', 'Write-Phase2RecordAndSidecar -RootFullPath $root -RunId $RunId -RecordBytes $bytes', 'WRITER_WRAPPER_CALL_NOT_PINNED', 'other.ps1'),
    @('m1 wrapper called from the library file name', 'Write-Phase2RecordAndSidecar -RootFullPath $root -RunId $RunId -RecordBytes $bytes', 'WRITER_WRAPPER_CALL_NOT_PINNED', 'Phase2Common.ps1'),
    @('m1 wrapper in Capture-Phase2.ps1 with another root', 'Write-Phase2RecordAndSidecar -RootFullPath "C:\Windows\Temp" -RunId $RunId -RecordBytes $bytes', 'WRITER_WRAPPER_CALL_NOT_PINNED', 'Capture-Phase2.ps1'),
    @('m1 wrapper in Capture-Phase2.ps1 with an extra parameter', 'Write-Phase2RecordAndSidecar -RootFullPath $root -RunId $RunId -RecordBytes $bytes -Extra 1', 'WRITER_WRAPPER_CALL_NOT_PINNED', 'Capture-Phase2.ps1'),
    @('m1 wrapper in Capture-Phase2.ps1 inside a function', 'function Exit-Phase2Usage { Write-Phase2RecordAndSidecar -RootFullPath $root -RunId $RunId -RecordBytes $bytes }', 'WRITER_WRAPPER_CALL_NOT_PINNED', 'Capture-Phase2.ps1'),
    # --- review 1 m2: a scanned script cannot redefine a library guard ---
    @('m2 Capture defines Assert-Phase2LocalPath with a no-op body (the reviewer redefinition)', 'function Assert-Phase2LocalPath { param($p) }', 'FUNCTION_REDEFINES_LIBRARY_FUNCTION', 'Capture-Phase2.ps1'),
    @('m2 Validate defines Assert-Phase2NoReparseChain', 'function Assert-Phase2NoReparseChain { param($p) }', 'FUNCTION_REDEFINES_LIBRARY_FUNCTION', 'Validate-Phase2.ps1'),
    @('m2 Capture defines Write-Phase2NewFile', 'function Write-Phase2NewFile { param($r,$n,$b) }', 'FUNCTION_REDEFINES_LIBRARY_FUNCTION', 'Capture-Phase2.ps1'),
    @('m2 Capture defines the validator exit helper', 'function Exit-Phase2ValidateUsage { exit 0 }', 'FUNCTION_DEFINITION_NOT_ALLOWED', 'Capture-Phase2.ps1'),
    @('m2 Validate defines a function that shadows Get-ChildItem', 'function Get-ChildItem { param($LiteralPath, [switch]$Force) @() }', 'FUNCTION_DEFINITION_NOT_ALLOWED', 'Validate-Phase2.ps1'),
    @('m2 any other function in an unknown script', 'function Helper { 1 }', 'FUNCTION_DEFINITION_NOT_ALLOWED', 'other.ps1'),
    @('m2 a nested function inside the allowed exit helper', 'function Exit-Phase2Usage { function Inner { 1 } exit 1 }', 'FUNCTION_DEFINITION_NOT_ALLOWED', 'Capture-Phase2.ps1'),
    @('m2 the library redefines a listed command (Join-Path)', 'function Join-Path { param($Path,$ChildPath) "C:\evil" }', 'FUNCTION_SHADOWS_LISTED_COMMAND', 'Phase2Common.ps1|lib'),
    @('m2 the library defines the same function twice (the second wins)', "function Assert-Twice { 1 }`nfunction Assert-Twice { 2 }", 'FUNCTION_DEFINED_TWICE', 'Phase2Common.ps1|lib'),
    # --- review 1 m3: FileStream constructor overloads with extra arguments ---
    @('m3 FileStream Open/Read with a buffer size (5 arguments)', "New-Object System.IO.FileStream(`"C:\x\a`", [System.IO.FileMode]::Open, [System.IO.FileAccess]::Read, [System.IO.FileShare]::Read, 4096)", 'FILESTREAM_ARGS_NOT_ALLOWED', 'a.ps1'),
    @('m3 FileStream Open/Read with a FileOptions overload (6 arguments)', "New-Object System.IO.FileStream(`"C:\x\a`", [System.IO.FileMode]::Open, [System.IO.FileAccess]::Read, [System.IO.FileShare]::Read, 4096, [System.IO.FileOptions]::DeleteOnClose)", 'FILESTREAM_ARGS_NOT_ALLOWED', 'a.ps1'),
    @('m3 FileStream whose 4th argument is not a FileShare (integer)', "New-Object System.IO.FileStream(`"C:\x\a`", [System.IO.FileMode]::Open, [System.IO.FileAccess]::Read, 4096)", 'FILESTREAM_ARGS_NOT_ALLOWED', 'a.ps1'),
    @('m3 FileStream whose 4th argument is a variable', "New-Object System.IO.FileStream(`"C:\x\a`", [System.IO.FileMode]::Open, [System.IO.FileAccess]::Read, `$share)", 'FILESTREAM_ARGS_NOT_ALLOWED', 'a.ps1'),
    @('m3 FileStream whose 4th argument is an unlisted FileShare member', "New-Object System.IO.FileStream(`"C:\x\a`", [System.IO.FileMode]::Open, [System.IO.FileAccess]::Read, [System.IO.FileShare]::Inheritable)", 'FILESTREAM_ARGS_NOT_ALLOWED', 'a.ps1'),
    @('m3 FileStream whose 4th argument mixes a literal and a variable', "New-Object System.IO.FileStream(`"C:\x\a`", [System.IO.FileMode]::Open, [System.IO.FileAccess]::Read, ([System.IO.FileShare]::Read -bor `$extra))", 'FILESTREAM_ARGS_NOT_ALLOWED', 'a.ps1'),
    @('m3 FileStream through -ArgumentList with 5 arguments', "New-Object -TypeName System.IO.FileStream -ArgumentList `"C:\x\a`", [System.IO.FileMode]::Open, [System.IO.FileAccess]::Read, [System.IO.FileShare]::Read, 4096", 'FILESTREAM_ARGS_NOT_ALLOWED', 'a.ps1'),
    @('m3 the writer with a 5th argument', "function Write-Phase2NewFile { param(`$t) `$s = New-Object System.IO.FileStream(`$t, [System.IO.FileMode]::CreateNew, [System.IO.FileAccess]::Write, [System.IO.FileShare]::None, 4096) }", 'FILESTREAM_ARGS_NOT_ALLOWED', 'a.ps1')
)
$k3 = 0
foreach ($c in $repro3) {
    $k3++
    $sub = ''; $leaf = $c[3]
    if ($leaf.Contains('|')) { $leaf, $sub = $leaf.Split('|') }
    $f = Scan-SnippetNamed -Leaf $leaf -Text $c[1] -SubDir $sub
    $hit = @($f | Where-Object { $_.StartsWith($c[2] + '|') }).Count
    Assert-True ('reproduction: ' + $c[0] + ' -> ' + $c[2]) ($hit -gt 0) (($f -join ' ; '))
}
Write-Host ('  ' + $repro3.Count + ' round 3 reproduction fixtures')

# --- the reviewer appended lines to the REAL production scripts: the scan must now flag the appended copies ---
$prodAppend = @(
    @('Validate-Phase2.ps1 + multi-assignment to .Attributes (review 1 M1)', 'Validate-Phase2.ps1', '', "`n`$null, `$k[0].Attributes = 0, 1`n", 'MEMBER_ASSIGNMENT_NOT_ALLOWED'),
    @('Validate-Phase2.ps1 + $f[0].Attributes++ (review 1 M1)', 'Validate-Phase2.ps1', '', "`n`$k[0].Attributes++`n", 'INCREMENT_OF_MEMBER_FORBIDDEN'),
    @('Validate-Phase2.ps1 + a call of the write wrapper (review 1 m1)', 'Validate-Phase2.ps1', '', "`nWrite-Phase2RecordAndSidecar -RootFullPath `$root -RunId `$RunId -RecordBytes `$bytes`n", 'WRITER_WRAPPER_CALL_NOT_PINNED'),
    @('Capture-Phase2.ps1 + a no-op redefinition of a library guard (review 1 m2)', 'Capture-Phase2.ps1', '', "`nfunction Assert-Phase2LocalPath { param(`$p) }`n", 'FUNCTION_REDEFINES_LIBRARY_FUNCTION'),
    @('Capture-Phase2.ps1 + $PSScriptRoot reassignment (review 1 M2)', 'Capture-Phase2.ps1', '', "`n`$PSScriptRoot = 'C:\evil'`n", 'FORBIDDEN_VARIABLE_ASSIGNMENT'),
    @('lib/Phase2Common.ps1 + $PSScriptRoot reassignment (review 1 M2)', 'Phase2Common.ps1', 'lib', "`n`$PSScriptRoot = 'C:\evil'`n", 'FORBIDDEN_VARIABLE_ASSIGNMENT'),
    @('lib/Phase2Common.ps1 + a second definition of a library guard (review 1 m2)', 'Phase2Common.ps1', 'lib', "`nfunction Assert-Phase2LocalPath { param(`$p) }`n", 'FUNCTION_DEFINED_TWICE')
)
foreach ($c in $prodAppend) {
    $src = if ($c[2]) { Join-Path (Join-Path $script:ToolRoot $c[2]) $c[1] } else { Join-Path $script:ToolRoot $c[1] }
    $text = [System.IO.File]::ReadAllText($src) + $c[3]
    $f = Scan-SnippetNamed -Leaf $c[1] -Text $text -SubDir $c[2]
    $hit = @($f | Where-Object { $_.StartsWith($c[4] + '|') }).Count
    Assert-True ('production copy with an appended line is flagged: ' + $c[0]) ($hit -gt 0) (($f -join ' ; '))
    $base = Scan-SnippetNamed -Leaf $c[1] -Text ([System.IO.File]::ReadAllText($src)) -SubDir $c[2]
    Assert-Equal ('... and the unmodified copy of the same script is clean: ' + $c[1]) '' ($base -join ' ; ')
}
Write-Host ('  ' + $prodAppend.Count + ' production-copy append fixtures')

# --- every round 3 snippet, taken ALONE, yields at least one finding (none slips through with zero findings) ---
$zero3 = @()
foreach ($c in $repro3) {
    $sub = ''; $leaf = $c[3]
    if ($leaf.Contains('|')) { $leaf, $sub = $leaf.Split('|') }
    $f = Scan-SnippetNamed -Leaf $leaf -Text $c[1] -SubDir $sub
    if ($f.Count -eq 0) { $zero3 += $c[0] }
}
Assert-Equal 'no round 3 reproduction passes the scan with zero findings' '' ($zero3 -join ' | ')

# --- the set-level pin: exactly one call of the wrapper in the production set ---
$wd = Join-Path $fx 'r3-set'; [void][System.IO.Directory]::CreateDirectory($wd)
$wc = Join-Path $wd 'Capture-Phase2.ps1'
[System.IO.File]::WriteAllText($wc, "function Write-Phase2NewFile { 1 }`n")
Assert-True 'a set with no call of the write wrapper is a finding' (@(Invoke-Phase2StaticScanSet -Paths @($wc) -AllowedCommands $allowFile -RequireSingleWriter | Where-Object { $_.StartsWith('WRITER_WRAPPER_CALL_COUNT_NOT_ONE|') }).Count -eq 1)
$wc2 = Join-Path $wd 'two.ps1'
[System.IO.File]::WriteAllText($wc2, "Write-Phase2RecordAndSidecar -RootFullPath `$root -RunId `$RunId -RecordBytes `$bytes`nWrite-Phase2RecordAndSidecar -RootFullPath `$root -RunId `$RunId -RecordBytes `$bytes`n")
Assert-True 'a set with two calls of the write wrapper is a finding' (@(Invoke-Phase2StaticScanSet -Paths @($wc2) -AllowedCommands $allowFile -RequireSingleWriter | Where-Object { $_.StartsWith('WRITER_WRAPPER_CALL_COUNT_NOT_ONE|') }).Count -eq 1)

# --- positive controls of round 3: the forms the production scripts rely on must stay clean (strict scan) ---
$pos3 = @(
    @('plain variable increment', '$i = 0; $i++; ++$i; $i--', 'a.ps1'),
    @('plain variable compound assignment', '$n = 0; $n += 2; $n -= 1', 'a.ps1'),
    @('for loop with a plain counter', 'for ($i = 0; $i -lt 3; $i++) { $i }', 'a.ps1'),
    @('counter in a hashtable entry', '$h = @{}; $h["a"] = 0; $h["a"]++', 'a.ps1'),
    @('plain multi-assignment', '$a, $b = 1, 2; $x, $y, $z = 1, 2, 3', 'a.ps1'),
    @('typed variable assignment', '[int]$n = 5; [string]$s = "a"', 'a.ps1'),
    @('reading Attributes is fine', '$f = Get-ChildItem -LiteralPath "D:\x" -Force; $x = $f[0].Attributes', 'a.ps1'),
    @('reading $PSScriptRoot is fine', '$p = Join-Path -Path $PSScriptRoot -ChildPath "x"', 'a.ps1'),
    @('assignment to an allowed member via a multi-assignment', '$o = New-Object System.Text.Json.JsonDocumentOptions; $a, $o.MaxDepth = 1, 64', 'a.ps1'),
    @('the pinned wrapper call in Capture-Phase2.ps1 at the top level', '$written = Write-Phase2RecordAndSidecar -RootFullPath $root -RunId $RunId -RecordBytes $bytes', 'Capture-Phase2.ps1'),
    @('the exit helper of Capture-Phase2.ps1', 'function Exit-Phase2Usage { param([string]$Message) exit 1 }', 'Capture-Phase2.ps1'),
    @('the exit helper of Validate-Phase2.ps1', 'function Exit-Phase2ValidateUsage { param([string]$Message) exit 1 }', 'Validate-Phase2.ps1'),
    @('FileStream with three arguments (read)', 'New-Object System.IO.FileStream("C:\x", [System.IO.FileMode]::Open, [System.IO.FileAccess]::Read)', 'a.ps1'),
    @('FileStream with a -bor of FileShare literals (read)', 'New-Object System.IO.FileStream("C:\x", [System.IO.FileMode]::Open, [System.IO.FileAccess]::Read, ([System.IO.FileShare]::ReadWrite -bor [System.IO.FileShare]::Delete))', 'a.ps1'),
    @('library with unique function names', "function Assert-Unique1 { 1 }`nfunction Assert-Unique2 { 2 }", 'Phase2Common.ps1|lib')
)
foreach ($c in $pos3) {
    $sub = ''; $leaf = $c[2]
    if ($leaf.Contains('|')) { $leaf, $sub = $leaf.Split('|') }
    $f = Scan-SnippetNamed -Leaf $leaf -Text $c[1] -SubDir $sub
    Assert-Equal ('positive (strict): ' + $c[0]) '' ($f -join ' ; ')
}
Write-Host ('  ' + $pos3.Count + ' round 3 positive controls')
