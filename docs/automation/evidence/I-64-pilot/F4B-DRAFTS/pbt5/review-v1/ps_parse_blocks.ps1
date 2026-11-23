# Recomprobacion 3: analiza, SIN ejecutar, cada bloque ```powershell del pre-run v2.2 con el
# analizador de PowerShell, y cuenta en cada uno las variables que usa sin asignarlas en el mismo
# bloque (las de entorno y las automaticas se excluyen).
param([Parameter(Mandatory)] [string] $PreRun)
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$text = Get-Content -LiteralPath $PreRun -Raw
$ms = [regex]::Matches($text, '(?ms)^[ \t]*```powershell\r?\n(.*?)^[ \t]*```')
$auto = 'true','false','null','_','env','PSItem','LASTEXITCODE','ErrorActionPreference','PID','args','Matches','PSVersionTable','global'
$i = 0
foreach ($m in $ms) {
  $i++
  $code = $m.Groups[1].Value
  $tokens = $null; $errors = $null
  $ast = [System.Management.Automation.Language.Parser]::ParseInput($code, [ref]$tokens, [ref]$errors)
  $assigned = @($ast.FindAll({ param($n) $n -is [System.Management.Automation.Language.AssignmentStatementAst] }, $true) |
    ForEach-Object { $_.Left } | ForEach-Object { if ($_ -is [System.Management.Automation.Language.VariableExpressionAst]) { $_.VariablePath.UserPath } elseif ($_ -is [System.Management.Automation.Language.ConvertExpressionAst]) { $_.Child.VariablePath.UserPath } })
  $foreachVars = @($ast.FindAll({ param($n) $n -is [System.Management.Automation.Language.ForEachStatementAst] }, $true) | ForEach-Object { $_.Variable.VariablePath.UserPath })
  $used = @($ast.FindAll({ param($n) $n -is [System.Management.Automation.Language.VariableExpressionAst] }, $true) | ForEach-Object { $_.VariablePath.UserPath } | Sort-Object -Unique)
  $free = @($used | Where-Object { $_ -notin $assigned -and $_ -notin $foreachVars -and $_ -notin $auto -and $_ -notlike 'env:*' })
  $strict = $code -match 'Set-StrictMode -Version Latest'
  $eap = $code -match "\`$ErrorActionPreference = 'Stop'"
  $firstLine = ($code -split "`n")[0..2] -join ' | '
  "Bloque {0}: errores_de_sintaxis={1} strict={2} stop={3} sin_asignar=[{4}] :: {5}" -f $i, $errors.Count, $strict, $eap, ($free -join ','), $firstLine.Substring(0, [Math]::Min(110, $firstLine.Length))
  foreach ($e in $errors) { "   ERROR: " + $e.Message }
}
"Bloques: $i"
