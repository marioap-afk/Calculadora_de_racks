# Recomprobacion 3: prueba la semantica de PowerShell en la que se apoyan los bloques del
# pre-run v2.2. No toca AutoCAD, ni el repositorio, ni <raiz-de-trabajo>. Solo escribe dentro de esta
# carpeta (review-v1\_probe), y la borra al final.
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
"PSVersion " + $PSVersionTable.PSVersion
"PSNativeCommandUseErrorActionPreference: " + $(if (Get-Variable PSNativeCommandUseErrorActionPreference -ErrorAction SilentlyContinue) { $PSNativeCommandUseErrorActionPreference } else { 'no existe' })

# 1. El viaje de la hora de inicio por JSON, como en los bloques 5 a 9
$p = Get-Process -Id $PID
$s = $p.StartTime.ToUniversalTime().ToString('o')
$here = Split-Path -Parent $MyInvocation.MyCommand.Path
$dir = Join-Path $here '_probe'
if (Test-Path -LiteralPath $dir) { throw "la carpeta de prueba ya existe" }
New-Item -ItemType Directory -Path $dir | Out-Null
try {
  $f = Join-Path $dir 'rec.json'
  [pscustomobject]@{ Id = $p.Id; StartTimeUtc = $s } | ConvertTo-Json | Out-File -LiteralPath $f -Encoding utf8 -NoClobber
  $rec = Get-Content -LiteralPath $f -Raw | ConvertFrom-Json
  "Tipo de StartTimeUtc tras ConvertFrom-Json: " + $rec.StartTimeUtc.GetType().FullName
  "Cadena original:  $s"
  "Valor leido:      $($rec.StartTimeUtc)"
  "Comparacion de los bloques (-ne): " + ($p.StartTime.ToUniversalTime().ToString('o') -ne $rec.StartTimeUtc)

  # 2. git -C <no repositorio> rev-parse 2>$null con Stop
  $nr = Join-Path $dir 'norepo'
  New-Item -ItemType Directory -Path $nr | Out-Null
  $env:GIT_CEILING_DIRECTORIES = $dir
  try {
    $top = & git -C $nr rev-parse --show-toplevel 2>$null
    "git fuera de repo: sin excepcion; LASTEXITCODE=$LASTEXITCODE; top='$top'"
  } catch { "git fuera de repo: EXCEPCION: $($_.Exception.GetType().Name)" }
  finally { Remove-Item Env:GIT_CEILING_DIRECTORIES }

  # 3. New-Item -ItemType Directory con carpetas intermedias
  $deep = Join-Path $dir 'a\b\c'
  try { New-Item -ItemType Directory -Path $deep | Out-Null; "New-Item crea intermedias: " + (Test-Path -LiteralPath $deep) }
  catch { "New-Item no crea intermedias: $($_.Exception.Message)" }
  # 4. New-Item sobre una carpeta que ya existe, sin -Force
  try { New-Item -ItemType Directory -Path $deep | Out-Null; "New-Item sobre existente: sin error" }
  catch { "New-Item sobre existente: error (para)" }
}
finally { Remove-Item -LiteralPath $dir -Recurse -Force }
"Carpeta de prueba borrada: " + (-not (Test-Path -LiteralPath $dir))
