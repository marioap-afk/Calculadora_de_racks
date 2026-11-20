param(
    [Parameter(Mandatory = $true)][string]$BaseSha,
    [Parameter(Mandatory = $true)][string]$RedSha,
    [string]$Now = 'worktree'
)
# Read-only checks of INV-PBT3C-01, 03 and 04, of CT-PBT3C-04 and of the census of declarations (PBT3C-C13). Paths are relative to the
# worktree root (run it from there). The output goes to the console and is saved as UTF-8 by the caller.
$ErrorActionPreference = 'Stop'
$writer = 'src/RackCad.Plugin/Views/RackPilotUpdateWriter.cs'
$facade = 'src/RackCad.Plugin/Views/RackViewBlockEraser.cs'
$tests = @('tests/RackCad.Tests/PushBackViewEraseGuardTests.cs', 'tests/RackCad.Tests/PushBackWritePathPluginGuardTests.cs')

function Get-Text([string]$ref, [string]$path) {
    if ($ref -eq 'worktree') {
        if (Test-Path $path) { return [IO.File]::ReadAllText((Resolve-Path $path).Path) } else { return $null }
    }
    $null = git cat-file -e "${ref}:${path}" 2>$null
    if ($LASTEXITCODE -ne 0) { return $null }
    return (git show "${ref}:${path}") -join "`n"
}

function Normalize([string]$text) { if ($null -eq $text) { return $null } return $text.Replace("`r`n", "`n") }

function Count-Of([string]$text, [string]$literal) {
    if ($null -eq $text) { return 0 }
    $count = 0; $at = 0
    while (($at = $text.IndexOf($literal, $at, [StringComparison]::Ordinal)) -ge 0) { $count++; $at += $literal.Length }
    return $count
}

function Clean-Code([string]$text) {
    # The comments out and the string and character literals emptied (as the source guards do).
    $text = [regex]::Replace($text, '@"(?:[^"]|"")*"', '""')
    $text = [regex]::Replace($text, '\$?"(?:\\.|[^"\\\n])*"', '""')
    $text = [regex]::Replace($text, "'(?:\\.|[^'\\\n])'", "''")
    $text = [regex]::Replace($text, '/\*.*?\*/', ' ', 'Singleline')
    $text = [regex]::Replace($text, '//[^\n]*', '')
    return $text
}

# The declarations of a file: every type and every member of a type, by a scan of the cleaned text (a member is the text at the depth of
# the body of its type, up to its first ';', '{' or '='). An overload is one more entry with the same name.
function Get-Declarations([string]$text) {
    $result = New-Object System.Collections.Generic.List[string]
    if ($null -eq $text) { return $result }
    $code = Clean-Code (Normalize $text)
    $code = [regex]::Replace($code, '(?m)^\s*#.*$', '')
    $stack = New-Object System.Collections.Generic.List[string]   # names of the types, innermost last
    $depthOfBody = New-Object System.Collections.Generic.List[int]
    $depth = 0
    $buffer = New-Object System.Text.StringBuilder
    $parenDepth = 0
    $namespaceDepth = 0
    for ($i = 0; $i -lt $code.Length; $i++) {
        $c = $code[$i]
        if ($c -eq '(' ) { $parenDepth++ }
        if ($c -eq ')' ) { $parenDepth-- }
        $atMemberLevel = ($depthOfBody.Count -gt 0 -and $depth -eq $depthOfBody[$depthOfBody.Count - 1] -and $parenDepth -eq 0)
        $atTopLevel = ($depthOfBody.Count -eq 0 -and $parenDepth -eq 0)
        if ($c -eq '{') {
            if ($atTopLevel -or $atMemberLevel) {
                $head = [regex]::Replace($buffer.ToString(), '\[[^\]]*\]', ' ').Trim()
                $buffer.Clear() | Out-Null
                if ($head -match '\bnamespace\s+[\w\.]+$') { $depth++; continue }
                if ($head -match '\b(class|struct|interface|enum|record)\s+(\w+)') {
                    $name = $matches[2]
                    $full = (($stack + $name) -join '.')
                    $result.Add('type ' + $full)
                    $stack.Add($name)
                    $depth++
                    $depthOfBody.Add($depth)
                    continue
                }
                # A member with a body: take its name, then skip the body.
                if ($head.Length -gt 0) {
                    $result.Add('member ' + ($stack -join '.') + '.' + (Get-MemberName $head))
                }
                $depth++
                continue
            }
            $depth++
            continue
        }
        if ($c -eq '}') {
            $depth--
            if ($depthOfBody.Count -gt 0 -and $depth -lt $depthOfBody[$depthOfBody.Count - 1]) {
                $depthOfBody.RemoveAt($depthOfBody.Count - 1)
                $stack.RemoveAt($stack.Count - 1)
            }
            $buffer.Clear() | Out-Null
            continue
        }
        if ($atMemberLevel -and ($c -eq ';' -or $c -eq '=')) {
            if ($c -eq '=' -and $i + 1 -lt $code.Length -and $code[$i + 1] -eq '>') {
                # An expression-bodied member: the name is what is before '=>'; it ends at the next ';'.
                $head = [regex]::Replace($buffer.ToString(), '\[[^\]]*\]', ' ').Trim()
                $result.Add('member ' + ($stack -join '.') + '.' + (Get-MemberName $head))
                $end = $code.IndexOf(';', $i)
                $i = $end
                $buffer.Clear() | Out-Null
                continue
            }
            $head = [regex]::Replace($buffer.ToString(), '\[[^\]]*\]', ' ').Trim()
            if ($head.Length -gt 0) { $result.Add('member ' + ($stack -join '.') + '.' + (Get-MemberName $head)) }
            if ($c -eq '=') {
                # A field with an initializer: skip to its ';' at the same depth.
                $d = 0
                for ($j = $i + 1; $j -lt $code.Length; $j++) {
                    $x = $code[$j]
                    if ($x -eq '(' -or $x -eq '{' -or $x -eq '[') { $d++ }
                    elseif ($x -eq ')' -or $x -eq '}' -or $x -eq ']') { $d-- }
                    elseif ($x -eq ';' -and $d -le 0) { $i = $j; break }
                }
            }
            $buffer.Clear() | Out-Null
            continue
        }
        if ($atMemberLevel -or $atTopLevel) { $buffer.Append($c) | Out-Null }
    }
    return $result
}

function Get-MemberName([string]$head) {
    # The name of a member from the text before its body, its ';' or its '=': before the first '(' for a method or constructor, or the
    # last identifier for a field or a property.
    $paren = $head.IndexOf('(')
    $text = if ($paren -ge 0) { $head.Substring(0, $paren) } else { $head }
    $text = [regex]::Replace($text, '<[^<>]*>', '')
    $m = [regex]::Matches($text, '[A-Za-z_]\w*')
    if ($m.Count -eq 0) { return '?' }
    return $m[$m.Count - 1].Value
}

$lines = New-Object System.Collections.Generic.List[string]
function Say([string]$line) { $script:lines.Add($line); Write-Output $line }

$range = if ($Now -eq 'worktree') { $BaseSha } else { "$BaseSha..$Now" }
$nowRef = $Now
Say "# INV-PBT3C-01 (alcance); Now=$Now"
Say "git diff --name-status $range -- src tests:"
foreach ($l in (git diff --name-status $range -- src tests)) { Say ("  " + $l) }
Say "git diff --name-only $RedSha$(if ($Now -ne 'worktree') { "..$Now" }) -- src tests:"
$redRange = if ($Now -eq 'worktree') { $RedSha } else { "$RedSha..$Now" }
foreach ($l in (git diff --name-only $redRange -- src tests)) { Say ("  " + $l) }
$forbidden = @(git diff --name-only $range -- src/RackCad.Application src/RackCad.UI src/RackCad.Domain src/RackCad.Plugin/RackPushBackCommands.cs src/RackCad.Plugin/RackCommandSupport.cs)
Say ("diff de Application, UI, Domain, RPC y RCS vacio=" + ($forbidden.Count -eq 0))

Say "# INV-PBT3C-03 (el escritor, texto entero con comentarios): Base y ahora"
$baseWriter = Normalize (Get-Text $BaseSha $writer)
$nowWriter = Normalize (Get-Text $nowRef $writer)
$present = @('class RackPilot', 'StartTransaction', '.Commit(', 'Purge', '[Conditional("DEBUG")]', '.Name =')
$absent = @('GetBlockReferenceIds', 'Erase(', 'EraseViewBlocks', 'ForWrite', 'UpgradeOpen')
foreach ($term in ($present + $absent)) {
    Say ("  '" + $term + "' base=" + (Count-Of $baseWriter $term) + " ahora=" + (Count-Of $nowWriter $term))
}

Say "# INV-PBT3C-04 (la fachada)"
$nowFacade = Normalize (Get-Text $nowRef $facade)
Say ("  la fachada existe=" + ($null -ne $nowFacade))
if ($null -ne $nowFacade) {
    Say ("  PushBack en el texto entero=" + (Count-Of $nowFacade 'PushBack'))
    Say ("  NamesThePilot=" + ([regex]::Matches($nowFacade, 'PushBackPilot\w*|RackPilot\w*|RackCad\.Application\.Systems\.PushBack\.Pilot').Count))
    foreach ($term in @('LockDocument', 'StartTransaction', 'StartOpenCloseTransaction', '.Commit(', 'Purge', 'Regen', 'SyncName')) {
        Say ("  '" + $term + "'=" + (Count-Of $nowFacade $term))
    }
}
# The census of CT-PBT1B-15: the files of the Plugin whose code carries PushBack.
$pluginFiles = @()
if ($Now -eq 'worktree') {
    $pluginFiles = Get-ChildItem 'src/RackCad.Plugin' -Recurse -Filter *.cs | Where-Object { $_.FullName -notmatch '[\\/](bin|obj)[\\/]' } | ForEach-Object { $_.FullName }
    $withPushBack = @($pluginFiles | Where-Object { (Clean-Code (Normalize ([IO.File]::ReadAllText($_)))).Contains('PushBack') })
} else {
    $names = @(git ls-tree -r --name-only $Now -- src/RackCad.Plugin | Where-Object { $_ -like '*.cs' })
    $withPushBack = @($names | Where-Object { (Clean-Code (Normalize (Get-Text $Now $_))).Contains('PushBack') })
}
Say ("  censo de CT-PBT1B-15: archivos del Plugin con PushBack=" + $withPushBack.Count)

Say "# CT-PBT3C-04 (el censo de la primitiva en todo src/)"
$call = 'RackViewBlockEraser.EraseInTransaction('
if ($Now -eq 'worktree') {
    $srcFiles = Get-ChildItem 'src' -Recurse -Filter *.cs | Where-Object { $_.FullName -notmatch '[\\/](bin|obj)[\\/]' }
    $hits = @($srcFiles | ForEach-Object { $n = Count-Of ([IO.File]::ReadAllText($_.FullName)) $call; if ($n -gt 0) { (Resolve-Path -Relative $_.FullName).Replace('\', '/') + ' x' + $n } })
} else {
    $names = @(git ls-tree -r --name-only $Now -- src | Where-Object { $_ -like '*.cs' })
    $hits = @($names | ForEach-Object { $n = Count-Of (Get-Text $Now $_) $call; if ($n -gt 0) { $_ + ' x' + $n } })
}
foreach ($h in $hits) { Say ("  " + $h) }
Say ("  llamadas totales=" + (($hits | ForEach-Object { [int]($_ -replace '^.* x', '') } | Measure-Object -Sum).Sum))
if ($null -ne $nowWriter) {
    $eraseUnit = [regex]::Match($nowWriter, 'public\s+PushBackPilotUnitResult\s+EraseUnit\s*\(')
    Say ("  EraseUnit en el escritor=" + $eraseUnit.Success)
    if ($eraseUnit.Success) {
        $start = $eraseUnit.Index; $open = $nowWriter.IndexOf('{', $start); $d = 0; $end = $open
        for ($k = $open; $k -lt $nowWriter.Length; $k++) { if ($nowWriter[$k] -eq '{') { $d++ } elseif ($nowWriter[$k] -eq '}') { $d--; if ($d -eq 0) { $end = $k; break } } }
        Say ("  llamadas dentro de EraseUnit=" + (Count-Of $nowWriter.Substring($start, $end - $start + 1) $call))
    }
}

Say "# CENSO de declaraciones (PBT3C-C13): Base y ahora, una a una, con su nombre"
$diffAll = @()
foreach ($file in @($writer, $facade)) {
    $b = Get-Declarations (Get-Text $BaseSha $file)
    $n = Get-Declarations (Get-Text $nowRef $file)
    Say ("## " + $file)
    Say ("  Base (" + $b.Count + "):")
    foreach ($d in $b) { Say ("    " + $d) }
    Say ("  Ahora (" + $n.Count + "):")
    foreach ($d in $n) { Say ("    " + $d) }
    $baseBag = @{}; foreach ($d in $b) { $baseBag[$d] = 1 + [int]$baseBag[$d] }
    $nowBag = @{}; foreach ($d in $n) { $nowBag[$d] = 1 + [int]$nowBag[$d] }
    $keys = @($baseBag.Keys) + @($nowBag.Keys) | Sort-Object -Unique -CaseSensitive
    foreach ($key in $keys) {
        $delta = [int]$nowBag[$key] - [int]$baseBag[$key]
        if ($delta -ne 0) { $diffAll += ($file + ' | ' + $key + ' | delta=' + $delta) }
    }
}
Say "## DIFERENCIA (ahora - base)"
foreach ($d in $diffAll) { Say ("  " + $d) }
Say ("  total de diferencias=" + $diffAll.Count)

Say "# INV-PBT3C-07 (las redes): ningun archivo de tests existente cambia"
$changedTests = @(git diff --name-status $range -- tests)
foreach ($l in $changedTests) { Say ("  " + $l) }
