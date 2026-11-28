# Comprobacion acotada de PB-T5 (CA-T5): analiza SIN EJECUTAR los bloques ```powershell de un documento.
# Uso: pwsh -NoProfile -File ps_audit.ps1 -Doc <documento.md>
# Para cada bloque: errores de sintaxis; StrictMode y Stop al principio; exit 0 como ultima linea;
# variables usadas sin asignar en el propio bloque; literales '<...>' asignados y si el bloque los comprueba;
# y las operaciones con efecto (escrituras, borrados, entorno, registro, procesos).
param([Parameter(Mandatory = $true)][string]$Doc)
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$lines = Get-Content -LiteralPath $Doc -Encoding utf8
$blocks = @(); $cur = $null; $start = 0
for ($i = 0; $i -lt $lines.Count; $i++) {
  $s = $lines[$i].Trim()
  if ($null -eq $cur -and $s -eq '```powershell') { $cur = New-Object System.Collections.Generic.List[string]; $start = $i + 2; continue }
  if ($null -ne $cur -and $s -eq '```') { $blocks += [pscustomobject]@{ Line = $start; Text = ($cur -join "`n") }; $cur = $null; continue }
  if ($null -ne $cur) { $cur.Add(($lines[$i] -replace '^   ', '')) }
}
$auto = @('_', 'PID', 'LASTEXITCODE', 'true', 'false', 'null', 'ErrorActionPreference', 'PSItem')
$effects = 'Remove-Item|Stop-Process|Set-ItemProperty|New-ItemProperty|Remove-ItemProperty|SetEnvironmentVariable|\$env:[A-Za-z_]+\s*=|reg(\.exe)?\s+(add|delete)|Set-Content|Add-Content|Move-Item|Rename-Item|Copy-Item|Out-File|New-Item|\s>\s|Start-Process|Process\]::Start|CloseMainWindow|Kill\(|git\s.*(commit|push|checkout|reset|clean|add)\b'
$n = 0
foreach ($b in $blocks) {
  $n++
  $tokens = $null; $errs = $null
  $ast = [System.Management.Automation.Language.Parser]::ParseInput($b.Text, [ref]$tokens, [ref]$errs)
  $assigned = @($ast.FindAll({ param($x) $x -is [System.Management.Automation.Language.AssignmentStatementAst] }, $true) | ForEach-Object {
      $l = $_.Left
      if ($l -is [System.Management.Automation.Language.ConvertExpressionAst]) { $l = $l.Child }
      if ($l -is [System.Management.Automation.Language.VariableExpressionAst]) { $l.VariablePath.UserPath -replace '^global:', '' }
    })
  $assigned += @($ast.FindAll({ param($x) $x -is [System.Management.Automation.Language.ForEachStatementAst] }, $true) | ForEach-Object { $_.Variable.VariablePath.UserPath })
  $assigned += @($ast.FindAll({ param($x) $x -is [System.Management.Automation.Language.ParameterAst] }, $true) | ForEach-Object { $_.Name.VariablePath.UserPath })
  $used = @($ast.FindAll({ param($x) $x -is [System.Management.Automation.Language.VariableExpressionAst] }, $true) | ForEach-Object { $_.VariablePath.UserPath } | Sort-Object -Unique)
  $free = @($used | Where-Object { $_ -notin $assigned -and $_ -notin $auto -and $_ -notlike 'env:*' })
  $first2 = @($b.Text -split "`n" | Select-Object -First 2)
  $strict = ($first2.Count -eq 2) -and ($first2[0].Trim() -eq 'Set-StrictMode -Version Latest') -and ($first2[1].Trim() -eq "`$ErrorActionPreference = 'Stop'")
  $lastLine = @($b.Text.TrimEnd() -split "`n")[-1].Trim()
  $exit0 = $lastLine -eq 'exit 0'
  $exitCount = ([regex]::Matches($b.Text, '(?m)^\s*exit\s+0\s*$')).Count
  $ph = @([regex]::Matches($b.Text, "=\s*'(<[^']*>)'") | ForEach-Object { $_.Groups[1].Value } | Sort-Object -Unique)
  $phCheck = $b.Text -match "-match '\[<>\]'"
  $eff = @(($b.Text -split "`n") | Where-Object { $_ -match $effects } | ForEach-Object { $_.Trim() })
  "== bloque {0} (linea {1}): errores={2} strict={3} exit0_ultima={4} exit0_veces={5} libres=[{6}] marcadores=[{7}] comprueba_marcadores={8}" -f $n, $b.Line, $errs.Count, $strict, $exit0, $exitCount, ($free -join ','), ($ph -join ','), $phCheck
  foreach ($e in $errs) { "   ERROR: $($e.Message) (linea $($e.Extent.StartLineNumber))" }
  foreach ($e in $eff) { "   efecto: $e" }
}
"bloques: $($blocks.Count)"
exit 0
