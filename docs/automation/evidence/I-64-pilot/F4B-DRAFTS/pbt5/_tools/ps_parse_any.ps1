# Analiza SIN EJECUTAR los bloques ```powershell de un documento de pbt5 con el analizador de PowerShell.
# Para cada bloque: errores de sintaxis, si empieza con StrictMode y Stop, si termina con exit 0,
# y las variables que usa sin asignarlas en el propio bloque.
param([string]$Name = "smoke-run-v1.md")
$ErrorActionPreference = 'Stop'
$doc = Join-Path (Split-Path -Parent $PSScriptRoot) $Name
$lines = Get-Content -LiteralPath $doc -Encoding utf8
$blocks = @(); $cur = $null; $start = 0
for ($i = 0; $i -lt $lines.Count; $i++) {
  $s = $lines[$i].Trim()
  if ($null -eq $cur -and $s -eq '```powershell') { $cur = New-Object System.Collections.Generic.List[string]; $start = $i + 2; continue }
  if ($null -ne $cur -and $s -eq '```') { $blocks += [pscustomobject]@{ Line = $start; Text = ($cur -join "`n") }; $cur = $null; continue }
  if ($null -ne $cur) { $cur.Add(($lines[$i] -replace '^   ', '')) }
}
$auto = @('_', 'PID', 'env', 'LASTEXITCODE', 'true', 'false', 'null', 'PSScriptRoot', 'ErrorActionPreference')
$bad = 0
foreach ($b in $blocks) {
  $tokens = $null; $errs = $null
  $ast = [System.Management.Automation.Language.Parser]::ParseInput($b.Text, [ref]$tokens, [ref]$errs)
  $assigned = @($ast.FindAll({ param($n) $n -is [System.Management.Automation.Language.AssignmentStatementAst] }, $true) | ForEach-Object { $_.Left.Extent.Text -replace '^\[[^\]]+\]', '' -replace '^\$', '' -replace '^global:', '' })
  $assigned += @($ast.FindAll({ param($n) $n -is [System.Management.Automation.Language.ForEachStatementAst] }, $true) | ForEach-Object { $_.Variable.VariablePath.UserPath })
  $used = @($ast.FindAll({ param($n) $n -is [System.Management.Automation.Language.VariableExpressionAst] }, $true) | ForEach-Object { $_.VariablePath.UserPath } | Sort-Object -Unique)
  $free = @($used | Where-Object { $_ -notin $assigned -and $_ -notin $auto -and $_ -notlike 'env:*' })
  $strict = $b.Text -match '^Set-StrictMode -Version Latest' -and $b.Text -match "\`$ErrorActionPreference = 'Stop'"
  $exit0 = $b.Text.TrimEnd() -match 'exit 0$'
  "bloque en la linea {0}: errores={1} strict={2} exit0={3} libres=[{4}]" -f $b.Line, $errs.Count, $strict, $exit0, ($free -join ',')
  foreach ($e in $errs) { "   ERROR: $($e.Message) (linea $($e.Extent.StartLineNumber))" }
  if ($errs.Count -or -not $strict -or -not $exit0 -or $free.Count) { $bad++ }
}
"bloques: $($blocks.Count); con problemas: $bad"
