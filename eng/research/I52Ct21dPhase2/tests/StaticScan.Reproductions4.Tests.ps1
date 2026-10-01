# Negative controls that reproduce the review findings of fixer round 4 (review of the round 3 static scan: M1 scope-prefixed automatic
# variables and $PSDefaultParameterValues, M2 `[ref]` on an automatic variable, m1 common parameters on every command and advanced
# functions). Dot-sourced by StaticScan.Tests.ps1 after the round 3 file (needs $fx, $allowFile, Scan-SnippetNamed, Assert-Equal and
# Assert-True). Fixtures are scanned STRICTLY (no -AllowFixtureFunctions).

Start-TestSection 'static scan round 4: reproductions (each must be caught)'

$cvp = 'COMMON_VARIABLE_PARAMETER_FORBIDDEN'
$fav = 'FORBIDDEN_AUTOMATIC_VARIABLE'
$fva = 'FORBIDDEN_VARIABLE_ASSIGNMENT'
$ref = 'REF_NOT_ALLOWED'
$adv = 'ADVANCED_ATTRIBUTE_OUTSIDE_SCRIPT_PARAM'
$tg = '$d.RootElement.TryGetInt64'
$gs = 'Get-Phase2FileSha256 -Path x'

# @(label, text, rule id, leaf file name)
$repro4 = @(
    # --- review M1: the scope prefix hid the variable name ---
    @('M1(a) $global:PSDefaultParameterValues[''*:OutVariable''] = ''PSScriptRoot'' (the reviewer form)', '$global:PSDefaultParameterValues[''*:OutVariable''] = ''PSScriptRoot''', $fav, 'a.ps1'),
    @('M1(a) ... and it is also a forbidden assigned variable', '$global:PSDefaultParameterValues[''*:OutVariable''] = ''PSScriptRoot''', $fva, 'a.ps1'),
    @('M1(b) $global:PSDefaultParameterValues = @{...} (replacing the table)', '$global:PSDefaultParameterValues = @{ ''*:OutVariable'' = ''PSScriptRoot'' }', $fav, 'a.ps1'),
    @('M1(b) ... assigned variable', '$global:PSDefaultParameterValues = @{ ''*:OutVariable'' = ''PSScriptRoot'' }', $fva, 'a.ps1'),
    @('M1(c) $script:Host', '$script:Host = 1', $fav, 'a.ps1'),
    @('M1(d) $script:PSCmdlet', '$x = $script:PSCmdlet', $fav, 'a.ps1'),
    @('M1 unprefixed $PSDefaultParameterValues[...] =', '$PSDefaultParameterValues[''*:OutVariable''] = ''PSScriptRoot''', $fva, 'a.ps1'),
    @('M1 $PSDefaultParameterValues member call (Add)', '$PSDefaultParameterValues.Add(''*:OutVariable'', ''x'')', $fav, 'a.ps1'),
    @('M1 reading $PSDefaultParameterValues (pass-through)', 'Write-Output $PSDefaultParameterValues', $fav, 'a.ps1'),
    @('M1 reading $global:PSDefaultParameterValues into a variable', '$t = $global:PSDefaultParameterValues', $fav, 'a.ps1'),
    @('M1 braced scoped ${global:PSDefaultParameterValues}', '${global:PSDefaultParameterValues} = @{}', $fav, 'a.ps1'),
    @('M1 $local:Host', '$x = $local:Host', $fav, 'a.ps1'),
    @('M1 $private:ExecutionContext', '$x = $private:ExecutionContext', $fav, 'a.ps1'),
    @('M1 $global:MyInvocation', '$x = $global:MyInvocation', $fav, 'a.ps1'),
    @('M1 $script:PSHOME', '$x = $script:PSHOME', $fav, 'a.ps1'),
    @('M1 $global:ShellId', '$x = $global:ShellId', $fav, 'a.ps1'),
    @('M1 $PSDefaultParameterValues multi-assignment', '$a, $PSDefaultParameterValues = 1, @{}', $fva, 'a.ps1'),
    # --- review M2: [ref] makes an automatic variable an out-parameter target ---
    @('M2 TryGetInt64([ref]$PSScriptRoot) (the reviewer form)', ($tg + '([ref]$PSScriptRoot)'), $ref, 'a.ps1'),
    @('M2 [ref]$script:PSScriptRoot', ($tg + '([ref]$script:PSScriptRoot)'), $ref, 'a.ps1'),
    @('M2 [ref]$global:psscriptroot', ($tg + '([ref]$global:psscriptroot)'), $ref, 'a.ps1'),
    @('M2 [ref]${PSScriptRoot}', ($tg + '([ref]${PSScriptRoot})'), $ref, 'a.ps1'),
    @('M2 [ref]$PSCommandPath', ($tg + '([ref]$PSCommandPath)'), $ref, 'a.ps1'),
    @('M2 [ref]$PSDefaultParameterValues', ($tg + '([ref]$PSDefaultParameterValues)'), $ref, 'a.ps1'),
    @('M2 [ref]$args', ($tg + '([ref]$args)'), $ref, 'a.ps1'),
    @('M2 [ref] of a parenthesized variable', ($tg + '([ref]($PSScriptRoot))'), $ref, 'a.ps1'),
    @('M2 [ref] of an ordinary variable other than $l (deny by default)', ('$o = 0; ' + $tg + '([ref]$o)'), $ref, 'a.ps1'),
    @('M2 [ref] of a scoped $l', ('$l = 0; ' + $tg + '([ref]$script:l)'), $ref, 'a.ps1'),
    @('M2 [ref] of a member', ($tg + '([ref]$o.Value)'), $ref, 'a.ps1'),
    @('M2 [ref] of an index', ($tg + '([ref]$o[0])'), $ref, 'a.ps1'),
    @('M2 [ref] parameter type', 'param([ref]$p)', $ref, 'a.ps1'),
    @('M2 [ref] parameter type in a function', 'function Get-Q { param([ref]$p) }', $ref, 'a.ps1'),
    @('M2 the long type name of [ref]', ($tg + '([System.Management.Automation.PSReference]$PSScriptRoot)'), $ref, 'a.ps1'),
    @('M2 [ref] in a foreach body', ('foreach ($i in 1) { ' + $tg + '([ref]$PSScriptRoot) }'), $ref, 'a.ps1'),
    # --- review m1: the common parameters that write a NAMED variable, on every command ---
    @('m1 -ErrorVariable (the reviewer form)', ($gs + ' -ErrorVariable PSScriptRoot'), $cvp, 'a.ps1'),
    @('m1 -OutVariable', ($gs + ' -OutVariable PSScriptRoot'), $cvp, 'a.ps1'),
    @('m1 -WarningVariable', ($gs + ' -WarningVariable PSScriptRoot'), $cvp, 'a.ps1'),
    @('m1 -InformationVariable', ($gs + ' -InformationVariable PSScriptRoot'), $cvp, 'a.ps1'),
    @('m1 -PipelineVariable', ($gs + ' -PipelineVariable PSScriptRoot'), $cvp, 'a.ps1'),
    @('m1 -OutBuffer', ($gs + ' -OutBuffer 1'), $cvp, 'a.ps1'),
    @('m1 alias -ov', ($gs + ' -ov PSScriptRoot'), $cvp, 'a.ps1'),
    @('m1 alias -ev', ($gs + ' -ev PSScriptRoot'), $cvp, 'a.ps1'),
    @('m1 alias -wv', ($gs + ' -wv PSScriptRoot'), $cvp, 'a.ps1'),
    @('m1 alias -iv', ($gs + ' -iv PSScriptRoot'), $cvp, 'a.ps1'),
    @('m1 alias -pv', ($gs + ' -pv PSScriptRoot'), $cvp, 'a.ps1'),
    @('m1 alias -ob', ($gs + ' -ob 1'), $cvp, 'a.ps1'),
    @('m1 prefix -OutVar', ($gs + ' -OutVar PSScriptRoot'), $cvp, 'a.ps1'),
    @('m1 prefix -ErrorVar', ($gs + ' -ErrorVar PSScriptRoot'), $cvp, 'a.ps1'),
    @('m1 prefix -WarningV', ($gs + ' -WarningV PSScriptRoot'), $cvp, 'a.ps1'),
    @('m1 prefix -InformationV', ($gs + ' -InformationV PSScriptRoot'), $cvp, 'a.ps1'),
    @('m1 prefix -PipelineV', ($gs + ' -PipelineV PSScriptRoot'), $cvp, 'a.ps1'),
    @('m1 prefix -OutB', ($gs + ' -OutB 1'), $cvp, 'a.ps1'),
    @('m1 colon form -ErrorVariable:PSScriptRoot', ($gs + ' -ErrorVariable:PSScriptRoot'), $cvp, 'a.ps1'),
    @('m1 upper case -OUTVARIABLE', ($gs + ' -OUTVARIABLE PSScriptRoot'), $cvp, 'a.ps1'),
    @('m1 on a cmdlet: Get-ChildItem -ev', 'Get-ChildItem -LiteralPath "D:\x" -Force -ev PSScriptRoot', $cvp, 'a.ps1'),
    @('m1 on a cmdlet: Join-Path -OutVariable', 'Join-Path -Path a -ChildPath b -OutVariable PSScriptRoot', $cvp, 'a.ps1'),
    @('m1 on Out-Null: one letter -p (cmdlets only)', 'Out-Null -p x', $cvp, 'a.ps1'),
    @('m1 on a pipeline stage', ($gs + ' | Out-Null -ov PSScriptRoot'), $cvp, 'a.ps1'),
    @('m1 in a file named like the capture script', ($gs + ' -ErrorVariable PSScriptRoot'), $cvp, 'Capture-Phase2.ps1'),
    # --- review m1: advanced functions (the common parameters cannot become live) ---
    @('m1 [CmdletBinding()] on a function', 'function Get-Q { [CmdletBinding()] param($a) 1 }', $adv, 'a.ps1'),
    @('m1 [Parameter()] on a function parameter', 'function Get-Q { param([Parameter()]$a) 1 }', $adv, 'a.ps1'),
    @('m1 [Parameter(Mandatory)] on an inline function parameter', 'function Get-Q([Parameter(Mandatory)]$a) { 1 }', $adv, 'a.ps1'),
    @('m1 [CmdletBinding()] on a nested function in the library', 'function Get-Outer { function Get-Inner { [CmdletBinding()] param() 1 } }', $adv, 'Phase2Common.ps1|lib'),
    @('m1 [CmdletBinding()] in a script block literal', '1 | ForEach-Object { [CmdletBinding()] param() 1 }', $adv, 'a.ps1'),
    @('m1 [CmdletBinding()] in a library function', 'function Get-Q { [CmdletBinding()] param() 1 }', $adv, 'Phase2Common.ps1|lib')
)
foreach ($c in $repro4) {
    $sub = ''; $leaf = $c[3]
    if ($leaf.Contains('|')) { $leaf, $sub = $leaf.Split('|') }
    $f = Scan-SnippetNamed -Leaf $leaf -Text $c[1] -SubDir $sub
    $hit = @($f | Where-Object { $_.StartsWith($c[2] + '|') }).Count
    Assert-True ('reproduction: ' + $c[0] + ' -> ' + $c[2]) ($hit -gt 0) (($f -join ' ; '))
    Assert-True ('... and the snippet is never a clean scan: ' + $c[0]) ($f.Count -gt 0) ''
}
Write-Host ('  ' + $repro4.Count + ' round 4 reproduction fixtures')

# --- the real production scripts with the reviewers' appended lines: each must now be flagged, and each unmodified copy stays clean ---
$nl = "`n"
$prodAppend4 = @(
    @('Validate-Phase2.ps1 + $global:PSDefaultParameterValues[...] (M1a)', 'Validate-Phase2.ps1', '', ($nl + '$global:PSDefaultParameterValues[''*:OutVariable''] = ''PSScriptRoot''' + $nl), $fav),
    @('Validate-Phase2.ps1 + $global:PSDefaultParameterValues = @{...} (M1b)', 'Validate-Phase2.ps1', '', ($nl + '$global:PSDefaultParameterValues = @{ ''*:OutVariable'' = ''PSScriptRoot'' }' + $nl), $fva),
    @('Validate-Phase2.ps1 + $script:Host (M1c)', 'Validate-Phase2.ps1', '', ($nl + '$script:Host = 1' + $nl), $fav),
    @('Capture-Phase2.ps1 + $script:PSCmdlet (M1d)', 'Capture-Phase2.ps1', '', ($nl + '$x = $script:PSCmdlet' + $nl), $fav),
    @('lib/Phase2Common.ps1 + TryGetInt64([ref]$PSScriptRoot) (M2)', 'Phase2Common.ps1', 'lib', ($nl + $tg + '([ref]$PSScriptRoot)' + $nl), $ref),
    @('Validate-Phase2.ps1 + TryGetInt64([ref]$PSScriptRoot) (M2)', 'Validate-Phase2.ps1', '', ($nl + $tg + '([ref]$PSScriptRoot)' + $nl), $ref),
    @('lib/Phase2Common.ps1 + -ErrorVariable PSScriptRoot (m1)', 'Phase2Common.ps1', 'lib', ($nl + $gs + ' -ErrorVariable PSScriptRoot' + $nl), $cvp),
    @('lib/Phase2Common.ps1 + -OutVariable (m1)', 'Phase2Common.ps1', 'lib', ($nl + $gs + ' -OutVariable PSScriptRoot' + $nl), $cvp),
    @('lib/Phase2Common.ps1 + -ov (m1)', 'Phase2Common.ps1', 'lib', ($nl + $gs + ' -ov PSScriptRoot' + $nl), $cvp),
    @('Capture-Phase2.ps1 + -OutVar prefix (m1)', 'Capture-Phase2.ps1', '', ($nl + $gs + ' -OutVar PSScriptRoot' + $nl), $cvp),
    @('Validate-Phase2.ps1 + -ev on a cmdlet (m1)', 'Validate-Phase2.ps1', '', ($nl + 'Join-Path -Path a -ChildPath b -ev PSScriptRoot' + $nl), $cvp),
    @('lib/Phase2Common.ps1 + an advanced function (m1)', 'Phase2Common.ps1', 'lib', ($nl + 'function Get-Zed { [CmdletBinding()] param() 1 }' + $nl), $adv),
    @('Capture-Phase2.ps1 + an advanced exit helper (m1)', 'Capture-Phase2.ps1', '', ($nl + 'function Exit-Phase2Usage { [CmdletBinding()] param([string]$Message) exit 1 }' + $nl), $adv)
)
foreach ($c in $prodAppend4) {
    $src = if ($c[2]) { Join-Path (Join-Path $script:ToolRoot $c[2]) $c[1] } else { Join-Path $script:ToolRoot $c[1] }
    $orig = [System.IO.File]::ReadAllText($src)
    $f = Scan-SnippetNamed -Leaf $c[1] -Text ($orig + $c[3]) -SubDir $c[2]
    $hit = @($f | Where-Object { $_.StartsWith($c[4] + '|') }).Count
    Assert-True ('production copy with an appended line is flagged: ' + $c[0]) ($hit -gt 0) (($f -join ' ; '))
    $base = Scan-SnippetNamed -Leaf $c[1] -Text $orig -SubDir $c[2]
    Assert-Equal ('... and the unmodified copy of the same script is clean: ' + $c[1]) '' ($base -join ' ; ')
}
Write-Host ('  ' + $prodAppend4.Count + ' round 4 production-copy append fixtures')

# --- positive controls (strict): what the production scripts rely on stays clean ---
$pos4 = @(
    @('[ref] of the plain local $l (the production TryGetInt64 form)', '$l = [long]0; $e.TryGetInt64([ref]$l)', 'a.ps1'),
    @('negated [ref]$l inside a condition', '$l = [long]0; if (-not $e.TryGetInt64([ref]$l)) { throw "x" }', 'a.ps1'),
    @('a script-level [CmdletBinding()] and [Parameter] param block', ('[CmdletBinding()]' + $nl + 'param([Parameter(Position = 0)][string]$A, [string]$B)'), 'Validate-Phase2.ps1'),
    @('a simple function without attributes', 'function Get-Q { param([string]$A, [switch]$S) 1 }', 'Phase2Common.ps1|lib'),
    @('the one-letter parameter -P to a library function (declared parameter)', 'Invoke-Phase2Capture -Params @{} -P 1', 'a.ps1'),
    @('-Path / -ChildPath / -LiteralPath / -Force are not common-variable prefixes', 'Join-Path -Path a -ChildPath b; Get-ChildItem -LiteralPath "D:\x" -Force', 'a.ps1'),
    @('a plain scoped variable that is not forbidden', '$script:counter = 1; $global:flag = 2', 'a.ps1'),
    @('reading $PSScriptRoot', '$p = Join-Path -Path $PSScriptRoot -ChildPath "x"', 'a.ps1')
)
foreach ($c in $pos4) {
    $sub = ''; $leaf = $c[2]
    if ($leaf.Contains('|')) { $leaf, $sub = $leaf.Split('|') }
    $f = Scan-SnippetNamed -Leaf $leaf -Text $c[1] -SubDir $sub
    Assert-Equal ('positive (strict): ' + $c[0]) '' ($f -join ' ; ')
}
Write-Host ('  ' + $pos4.Count + ' round 4 positive controls')
