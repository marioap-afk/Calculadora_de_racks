"""Genera pre-run-tonight-v2.3.md desde la v2.2: reemplazos exactos y secciones entre marcas.

Cada texto o marca tiene que aparecer exactamente una vez; si no, para sin escribir nada.
No sobrescribe: si el destino existe, para.
"""
import os
import sys

here = os.path.dirname(os.path.abspath(__file__))
base = os.path.dirname(here)
SRC = "pre-run-tonight-v2.2.md"
DST = "pre-run-tonight-v2.3.md"


def once(text, s, what):
    n = text.count(s)
    if n != 1:
        sys.exit(f"STOP: {what}: aparece {n} veces: {s[:70]!r}")


def rep(text, old, new):
    once(text, old, "reemplazo")
    return text.replace(old, new)


def section(text, start, end, new, keep_start=True):
    once(text, start, "marca de inicio")
    once(text, end, "marca de fin")
    i = text.index(start)
    j = text.index(end)
    if j <= i:
        sys.exit(f"STOP: el fin va antes que el inicio: {start[:50]!r}")
    head = text[:i] + (start if keep_start else "")
    return head + new + text[j:]


CHANGES = r"""## Cambios respecto de la v2.2

Vienen de la «Recomprobación 3 (v2.2, la corrida)» de [IR1].

- **I-09: la hora de inicio de AutoCAD se guarda y se compara como `Ticks`,** un entero, en los bloques 5 a 9. PowerShell 7 lee del JSON el texto ISO como fecha, y la comparación de texto con fecha salía siempre «distinta».
  - El registro guarda `StartTicksUtc`, y cada bloque compara `[long]$rec.StartTicksUtc` con `$q.StartTime.ToUniversalTime().Ticks`.
  - Un bloque nuevo, el 4b, prueba el viaje por JSON con el propio proceso de la sesión antes de lanzar AutoCAD.
- **M-31: el build va en tres bloques.**
  - B1, la suite de Core, y B2, los 19 filtros de UI, se lanzan en segundo plano, con su salida en el archivo de la tarea.
  - B3, los builds, la versión y `catalogs`, va en primer plano.
  - Cada uno termina en «OK: …» o en «STOP: …».
- **M-32:** el bloque 5 escribe el registro de la instancia (el PID, la ruta y los `Ticks`) justo al lanzar AutoCAD, antes de `WaitForInputIdle`. Esa espera va dentro de un `try`.
- **M-33:**
  - el bloque de §4.3 guarda el código de salida de cada `git rev-parse` que se espera fallido, y termina con `exit 0` después de «OK»;
  - el bloque 1 exige que el Candidato y el producto sean SHA completos de 40 caracteres hexadecimales.
- **Lo no verificado de la recomprobación:** si `Get-FileHash` no puede leer una copia que AutoCAD tiene abierta, el bloque 10 lo registra como «no legible (abierta)» y sigue con las demás.

"""

SEC_42 = r"""
La herramienta corta una orden en primer plano a los 10 minutos (M-31).
- **B1 y B2** se lanzan **en segundo plano** (`run_in_background`): su salida queda en el archivo de la tarea, y la sesión espera el aviso de que han terminado, sin sondear.
- **B3** va en primer plano. Si se acerca a los 10 minutos, también va en segundo plano.
- **Ninguno escribe en `<raiz-de-trabajo>`.**
- **El criterio de cada uno es su última línea:** «OK: …», o un «STOP: …».

**Bloque B1. La suite de Core** (en segundo plano):

```powershell
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$wt = '<worktree>'; $dotnet = '<dotnet-del-usuario>'
foreach ($v in $wt, $dotnet) { if ([string]::IsNullOrWhiteSpace($v) -or $v -match '[<>]') { throw "STOP: un valor sin rellenar" } }
foreach ($p in $wt, $dotnet) { if (-not (Test-Path -LiteralPath $p)) { throw "STOP: no existe $p" } }
if (Get-Process acad, accoreconsole -ErrorAction SilentlyContinue) { throw "STOP PBT5-C08: hay un AutoCAD abierto" }
Push-Location -LiteralPath $wt
try {
  & $dotnet --version; if ($LASTEXITCODE -ne 0) { throw "STOP: dotnet" }
  & $dotnet test tests/RackCad.Tests/RackCad.Tests.csproj -c Debug
  if ($LASTEXITCODE -ne 0) { throw "STOP PBT5-C15: la suite de Core" }
} finally { Pop-Location }
"OK: la suite de Core"
```

**Bloque B2. Los 19 filtros de UI y el archivo de disposición** (en segundo plano, cuando B1 haya terminado):

```powershell
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$wt = '<worktree>'; $dotnet = '<dotnet-del-usuario>'
# Los 19 filtros de UI de RequiredTests del contrato de PB-T4e, literales [T4e]:
$filters = @('<filtro 1>', '<filtro 2>' <# … hasta el 19 #>)
foreach ($v in @($wt, $dotnet) + $filters) { if ([string]::IsNullOrWhiteSpace($v) -or $v -match '[<>]') { throw "STOP: un valor sin rellenar" } }
if ($filters.Count -ne 19) { throw "STOP: no son 19 filtros" }
foreach ($p in $wt, $dotnet) { if (-not (Test-Path -LiteralPath $p)) { throw "STOP: no existe $p" } }
if (Get-Process acad, accoreconsole -ErrorAction SilentlyContinue) { throw "STOP PBT5-C08: hay un AutoCAD abierto" }
$lay = Join-Path $env:LOCALAPPDATA 'RackCad\RackFrameConfigurator.layout'
$layBefore = if (Test-Path -LiteralPath $lay) { (Get-FileHash -LiteralPath $lay -Algorithm SHA256).Hash } else { 'NO-EXISTE' }
"Disposicion antes $layBefore"
Push-Location -LiteralPath $wt
try {
  $i = 0
  foreach ($f in $filters) {
    $i++
    "Filtro $i de 19"
    & $dotnet test tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj -c Debug --filter $f
    if ($LASTEXITCODE -ne 0) { throw "STOP PBT5-C15: el filtro $i ($f)" }
  }
} finally { Pop-Location }
$layAfter = if (Test-Path -LiteralPath $lay) { (Get-FileHash -LiteralPath $lay -Algorithm SHA256).Hash } else { 'NO-EXISTE' }
"Disposicion despues $layAfter"
"OK: los 19 filtros de UI"
```

**Bloque B3. Los builds, la versión y `catalogs`** (en primer plano, cuando B2 haya terminado):

```powershell
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$wt = '<worktree>'; $dotnet = '<dotnet-del-usuario>'; $cand = '<Candidato>'
foreach ($v in $wt, $dotnet, $cand) { if ([string]::IsNullOrWhiteSpace($v) -or $v -match '[<>]') { throw "STOP: un valor sin rellenar" } }
if ($cand -notmatch '^[0-9a-f]{40}$') { throw "STOP: el Candidato no es un SHA completo" }
foreach ($p in $wt, $dotnet) { if (-not (Test-Path -LiteralPath $p)) { throw "STOP: no existe $p" } }
if (Get-Process acad, accoreconsole -ErrorAction SilentlyContinue) { throw "STOP PBT5-C08: hay un AutoCAD abierto" }
Push-Location -LiteralPath $wt
try {
  & $dotnet build src/RackCad.UI/RackCad.UI.csproj -c Debug; if ($LASTEXITCODE -ne 0) { throw "STOP: build UI" }
  & $dotnet build src/RackCad.Plugin/RackCad.Plugin.csproj -c Debug; if ($LASTEXITCODE -ne 0) { throw "STOP: build Plugin" }
} finally { Pop-Location }
$bin = Join-Path $wt 'src\RackCad.Plugin\bin\Debug\net8.0-windows'
$ver = [System.Diagnostics.FileVersionInfo]::GetVersionInfo((Join-Path $bin 'RackCad.Plugin.dll')).ProductVersion
"ProductVersion $ver"
if (-not $ver.EndsWith('+' + $cand)) { throw "STOP PBT5-C01: la ProductVersion no identifica el Candidato" }
foreach ($n in 'RackCad.Plugin', 'RackCad.Application', 'RackCad.Domain', 'RackCad.UI') {
  $f = Join-Path $bin "$n.dll"; "{0} {1} {2}" -f $n, (Get-FileHash -LiteralPath $f -Algorithm SHA256).Hash, (Get-Item -LiteralPath $f).LastWriteTimeUtc.ToString('o')
}
$cb = Join-Path $bin 'catalogs'; $ca = Join-Path $wt 'assets\catalogs'
foreach ($p in $cb, $ca) { if (-not (Test-Path -LiteralPath $p -PathType Container)) { throw "STOP: no existe $p" } }
$a = @(Get-ChildItem -LiteralPath $cb -Recurse -File | ForEach-Object { "{0} {1}" -f $_.FullName.Substring($cb.Length), (Get-FileHash -LiteralPath $_.FullName -Algorithm SHA256).Hash })
$b = @(Get-ChildItem -LiteralPath $ca -Recurse -File | ForEach-Object { "{0} {1}" -f $_.FullName.Substring($ca.Length), (Get-FileHash -LiteralPath $_.FullName -Algorithm SHA256).Hash })
if ($a.Count -eq 0 -or (Compare-Object $a $b)) { throw "STOP: catalogs no es igual a assets/catalogs" }
"OK: build, version y catalogs"
```

- **Si la `ProductVersion` no identifica el Candidato,** se vuelve a compilar el Plugin con `--no-incremental` en un bloque como B3, y se mide otra vez.
- **Los `warning CS`** se leen en la salida de B3.

"""

SEC_43 = r"""
**El criterio es la línea «OK» y el código de salida 0** (M-33). Los `git rev-parse` que se esperan fallidos guardan su código en `$rc`, y el bloque termina con `exit 0`.

```powershell
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
# Los valores de esta corrida
$root  = '<raiz-de-trabajo>'
$o     = '<original-D1>'
$wt    = '<worktree>'
$main  = '<checkout-principal>'
$known = '7A868624226EE956CC411CAAA54EA2DE6B95849EEE946995917CFF70FB9F47A5'
foreach ($v in $root, $o, $wt, $main) {
  if ([string]::IsNullOrWhiteSpace($v) -or $v -match '[<>]') { throw "STOP: un valor sin rellenar" }
}
# La carpeta: nueva, no en la raiz de una unidad, fuera del perfil, del worktree, del checkout principal y de todo repositorio
$rootFull = [IO.Path]::GetFullPath($root).TrimEnd('\')
if ([IO.Path]::GetPathRoot($rootFull).TrimEnd('\') -eq $rootFull) { throw "STOP: la carpeta de trabajo es la raiz de una unidad" }
if (Test-Path -LiteralPath $rootFull) { throw "STOP: $rootFull ya existe" }
foreach ($r in $wt, $main, $env:USERPROFILE) {
  $rf = [IO.Path]::GetFullPath($r).TrimEnd('\') + '\'
  if (($rootFull + '\').StartsWith($rf, [StringComparison]::OrdinalIgnoreCase)) { throw "STOP: $rootFull esta dentro de $r" }
}
$parent = Split-Path -Parent $rootFull
if (-not (Test-Path -LiteralPath $parent -PathType Container)) { throw "STOP: no existe $parent" }
$top = & git -C $parent rev-parse --show-toplevel 2>$null; $rc = $LASTEXITCODE
if ($rc -eq 0 -and $top) { throw "STOP: $parent esta dentro de un repositorio git" }
# El original, antes de escribir nada
if (-not (Test-Path -LiteralPath $o -PathType Leaf)) { throw "STOP: no existe el original" }
$h0 = (Get-FileHash -LiteralPath $o -Algorithm SHA256).Hash
if ($h0 -ne $known) { throw "STOP: D1 no tiene el SHA-256 registrado en F4-SMOKE-2/run/integrity.md" }
# Las carpetas
$work = Join-Path $rootFull 'smoke'
$bak  = Join-Path $rootFull 'respaldo'
$r1   = Join-Path $work 'r1'
New-Item -ItemType Directory -Path $rootFull | Out-Null
foreach ($dir in $work, $bak, $r1, (Join-Path $work 'inv'), (Join-Path $work 'salidas'), (Join-Path $work 'screens'), (Join-Path $work 'screens\raw'), (Join-Path $work 'screens\crop')) {
  New-Item -ItemType Directory -Path $dir | Out-Null
}
$top = & git -C $rootFull rev-parse --show-toplevel 2>$null; $rc = $LASTEXITCODE
if ($rc -eq 0 -and $top) { throw "STOP: $rootFull esta dentro de un repositorio git" }
# El respaldo, primero
$dst = Join-Path $bak 'D1-respaldo.dwg'
if (Test-Path -LiteralPath $dst) { throw "STOP: el respaldo ya existe" }
Copy-Item -LiteralPath $o -Destination $dst
if ((Get-FileHash -LiteralPath $dst -Algorithm SHA256).Hash -ne $h0) { throw "STOP: el respaldo no es igual al original" }
# Las 33 copias, cada una desde el original
$m = @(foreach ($i in 1..33) {
  $c = 'c{0:d2}.dwg' -f $i
  $dst = Join-Path $r1 $c
  if (Test-Path -LiteralPath $dst) { throw "STOP: $c ya existe" }
  Copy-Item -LiteralPath $o -Destination $dst
  $h = (Get-FileHash -LiteralPath $dst -Algorithm SHA256).Hash
  if ($h -ne $h0) { throw "STOP: $c no es igual al original" }
  [pscustomobject]@{ copia = $c; sha256 = $h }
})
if ($m.Count -ne 33) { throw "STOP: no hay 33 copias" }
$manifest = Join-Path $work 'copias.json'
[pscustomobject]@{ original_sha256 = $h0; respaldo = 'D1-respaldo.dwg'; copias = $m } | ConvertTo-Json -Depth 3 | Out-File -LiteralPath $manifest -Encoding utf8 -NoClobber
if ((Get-FileHash -LiteralPath $o -Algorithm SHA256).Hash -ne $h0) { throw "STOP PBT5-C02: el original cambio" }
"OK: $rootFull nueva; respaldo y 33 copias iguales a D1 ($h0)"
exit 0
```

"""

SEC_45 = r"""
**Bloque 4b. La prueba del viaje de la hora por JSON** (I-09). Va antes del bloque 5, con el propio proceso de la sesión, y no escribe nada:

```powershell
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$me = Get-Process -Id $PID
$t = $me.StartTime.ToUniversalTime().Ticks
$back = ([pscustomobject]@{ StartTicksUtc = $t } | ConvertTo-Json | ConvertFrom-Json).StartTicksUtc
"Tipo leido: $($back.GetType().FullName)"
if ([long]$back -ne $t) { throw "STOP I-09: los Ticks no vuelven iguales del JSON" }
"OK: los Ticks vuelven iguales del JSON"
```

**Bloque 5. El lanzamiento.** Con `$n = 1` y `c01.dwg`; al relanzar por §4.8, con el número siguiente y la copia que toque. El registro se escribe justo al lanzar (M-32), con la hora en `Ticks` (I-09):

```powershell
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$work = '<carpeta-del-Smoke-PB>'; $acad = '<acad.exe>'; $dir = '<dir-del-acceso-directo>'; $copyName = 'c01.dwg'; $n = 1
foreach ($v in $work, $acad, $dir, $copyName) { if ([string]::IsNullOrWhiteSpace($v) -or $v -match '[<>]') { throw "STOP: un valor sin rellenar" } }
if ($copyName -notmatch '^c\d\d\.dwg$' -or $n -lt 1) { throw "STOP: la copia o el numero no son validos" }
$copy = Join-Path $work "r1\$copyName"
foreach ($p in $work, $acad, $dir, $copy) { if (-not (Test-Path -LiteralPath $p)) { throw "STOP: no existe $p" } }
$rec = Join-Path $work ("acad-session-{0}.json" -f $n)
if (Test-Path -LiteralPath $rec) { throw "STOP: $rec ya existe" }
$others = @(Get-Process acad -ErrorAction SilentlyContinue | ForEach-Object { $_.Id })
$psi = New-Object System.Diagnostics.ProcessStartInfo
$psi.FileName = [IO.Path]::GetFullPath($acad)
$psi.Arguments = '/product ACAD /language "en-US" "' + $copy + '"'
$psi.WorkingDirectory = $dir
$psi.UseShellExecute = $false
if (@($psi.Environment.Keys | Where-Object { $_ -like 'RACKCAD_DEBUG_*' }).Count) { throw "STOP PBT5-C04" }
$p = [System.Diagnostics.Process]::Start($psi)
[pscustomobject]@{ Id = $p.Id; Path = $psi.FileName; StartTicksUtc = $p.StartTime.ToUniversalTime().Ticks; Copy = $copyName; Others = $others } |
  ConvertTo-Json | Out-File -LiteralPath $rec -Encoding utf8 -NoClobber
try { $idle = $p.WaitForInputIdle(180000); "WaitForInputIdle: $idle" } catch { "WaitForInputIdle fallo: $($_.Exception.Message). El registro ya esta escrito: seguir con el bloque 6" }
Get-Content -LiteralPath $rec -Raw
```

**Bloque 6. La comprobación de la instancia,** en una orden aparte y siempre antes de teclear nada:

```powershell
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$work = '<carpeta-del-Smoke-PB>'
if (-not (Test-Path -LiteralPath $work -PathType Container)) { throw "STOP: no existe $work" }
$recFile = Get-ChildItem -LiteralPath $work -Filter 'acad-session-*.json' | Sort-Object { [int]($_.BaseName -replace '\D', '') } | Select-Object -Last 1
if (-not $recFile) { throw "STOP: no hay registro de la instancia" }
$rec = Get-Content -LiteralPath $recFile.FullName -Raw | ConvertFrom-Json
$q = Get-Process -Id ([int]$rec.Id) -ErrorAction SilentlyContinue
if (-not $q) { throw "STOP: la instancia $($rec.Id) no sigue viva" }
if ($q.ProcessName -ne 'acad') { throw "STOP: el PID no es acad" }
if ($q.Path -ne $rec.Path) { throw "STOP: la ruta no es la registrada" }
if ([long]$rec.StartTicksUtc -ne $q.StartTime.ToUniversalTime().Ticks) { throw "STOP: la hora de inicio no es la registrada (PID reutilizado)" }
"OK: PID $($q.Id), titulo: $($q.MainWindowTitle)"
```

- **Si la instancia no sigue viva,** se relanza una vez con el bloque 5 y el número siguiente. Si tampoco sigue viva: STOP.
- **Si el bloque 5 se quedó a medias** con AutoCAD vivo, su registro ya existe, y el bloque 6 lo encuentra.
- **Los AutoCAD de `Others`, y cualquiera que aparezca después, no se tocan.**

**Los diálogos de arranque:**
- «Proxy Information» → «OK», sin cambiar su opción;
- «References - Not Found Files» → «Ignore unresolved reference files», sin marcar «Always ignore».

**Tecleado en la línea de órdenes,** en trozos de 16 caracteres como máximo, con una captura antes de cada clic:
1. `EXTERNALREFERENCESCLOSE`, antes de cualquier captura que se vaya a custodiar (§8).
2. `(vl-load-com)`
3. `(command "NETLOAD" "<worktree-con-/>/src/RackCad.Plugin/bin/Debug/net8.0-windows/RackCad.Plugin.dll")`. En el aviso de seguridad, «Load Once»; **nunca «Always Load»**. Su captura no se custodia (§8).
4. `RACKCAD`: el menú sin excepciones y la ruta de la biblioteca. Se cierra el menú.
5. `RACKPANEL`: «RackCad: panel Workspace mostrado.».

**Bloque 7. Los módulos cargados (SPB-02):**

```powershell
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$work = '<carpeta-del-Smoke-PB>'
if (-not (Test-Path -LiteralPath $work -PathType Container)) { throw "STOP: no existe $work" }
$recFile = Get-ChildItem -LiteralPath $work -Filter 'acad-session-*.json' | Sort-Object { [int]($_.BaseName -replace '\D', '') } | Select-Object -Last 1
if (-not $recFile) { throw "STOP: no hay registro de la instancia" }
$rec = Get-Content -LiteralPath $recFile.FullName -Raw | ConvertFrom-Json
$q = Get-Process -Id ([int]$rec.Id)
if ($q.ProcessName -ne 'acad' -or $q.Path -ne $rec.Path -or [long]$rec.StartTicksUtc -ne $q.StartTime.ToUniversalTime().Ticks) { throw "STOP: el PID ya no es la instancia de la sesion" }
$q.Modules | Where-Object ModuleName -like 'RackCad.*' | ForEach-Object { "{0} {1}" -f $_.ModuleName, (Get-FileHash -LiteralPath $_.FileName -Algorithm SHA256).Hash }
```

Tienen que salir los SHA-256 de B3.

**La línea de base** (SPB-04, M-20):
1. seleccionar el rack del paso con la selección por expresión (§4.6) y esperar a la vista previa del panel;
2. **después,** `(getvar "DBMOD")` y la captura de la lista de Deshacer.

En SPB-15 y SPB-35, lo mismo.

"""

SEC_47_B8 = r"""7. **Bloque 8**, con `cNN` y `cMM` rellenados:

   ```powershell
   Set-StrictMode -Version Latest
   $ErrorActionPreference = 'Stop'
   $work = '<carpeta-del-Smoke-PB>'; $prev = 'cNN'; $next = 'cMM'
   foreach ($v in $prev, $next) { if ($v -notmatch '^c\d\d$') { throw "STOP: nombre de copia sin rellenar" } }
   $known = '7A868624226EE956CC411CAAA54EA2DE6B95849EEE946995917CFF70FB9F47A5'
   $r1 = Join-Path $work 'r1'
   foreach ($p in $work, $r1, (Join-Path $r1 "$prev.dwg"), (Join-Path $r1 "$next.dwg")) { if (-not (Test-Path -LiteralPath $p)) { throw "STOP: no existe $p" } }
   $recFile = Get-ChildItem -LiteralPath $work -Filter 'acad-session-*.json' | Sort-Object { [int]($_.BaseName -replace '\D', '') } | Select-Object -Last 1
   if (-not $recFile) { throw "STOP: no hay registro de la instancia" }
   $rec = Get-Content -LiteralPath $recFile.FullName -Raw | ConvertFrom-Json
   $q = Get-Process -Id ([int]$rec.Id)
   if ($q.ProcessName -ne 'acad' -or $q.Path -ne $rec.Path -or [long]$rec.StartTicksUtc -ne $q.StartTime.ToUniversalTime().Ticks) { throw "STOP: el PID ya no es la instancia de la sesion" }
   [pscustomobject]@{
     dwlAnteriorExiste  = Test-Path -LiteralPath (Join-Path $r1 "$prev.dwl")
     anteriorIgualD1    = (Get-FileHash -LiteralPath (Join-Path $r1 "$prev.dwg") -Algorithm SHA256).Hash -eq $known
     dwlSiguienteExiste = Test-Path -LiteralPath (Join-Path $r1 "$next.dwl")
     titulo             = $q.MainWindowTitle
   }
   ```

   Lo esperado: `False`, `True`, `True`, y el título con `cMM.dwg`.

"""

SEC_48 = r"""
**Bloque 9.** No se ejecuta si queda algún documento abierto:

```powershell
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$work = '<carpeta-del-Smoke-PB>'
if (-not (Test-Path -LiteralPath $work -PathType Container)) { throw "STOP: no existe $work" }
$recFile = Get-ChildItem -LiteralPath $work -Filter 'acad-session-*.json' | Sort-Object { [int]($_.BaseName -replace '\D', '') } | Select-Object -Last 1
if (-not $recFile) { throw "STOP: no hay registro de la instancia" }
$rec = Get-Content -LiteralPath $recFile.FullName -Raw | ConvertFrom-Json
$q = Get-Process -Id ([int]$rec.Id) -ErrorAction SilentlyContinue
if (-not $q) { throw "STOP: la instancia ya no existe; se relanza con el bloque 5, sin cerrar nada" }
if ($q.ProcessName -ne 'acad') { throw "STOP: el PID no es acad" }
if ($q.Path -ne $rec.Path) { throw "STOP: la ruta no es la registrada" }
if ([long]$rec.StartTicksUtc -ne $q.StartTime.ToUniversalTime().Ticks) { throw "STOP: la hora de inicio no es la registrada (PID reutilizado)" }
$t = $q.MainWindowTitle
if ($t -match '\.dwg') {
  if ($t -match 'c\d\d\.dwg') { throw "STOP: sigue abierta una copia de la sesion ($t): se vuelve a 4.7" }
  throw "STOP: el titulo nombra un documento que no es de la sesion"
}
[void]$q.CloseMainWindow()
if (-not $q.WaitForExit(60000)) { throw "STOP: la instancia no salio en 60 s; no se usa Stop-Process" }
"OK: la instancia $($rec.Id) salio; relanzar con el bloque 5 y el numero siguiente"
```

"""

SEC_49_B10 = r"""**Bloque 10. SPB-51, la integridad.** Solo lee, y se hace siempre, también en la parada de la frontera. Si una copia está abierta en AutoCAD y no se puede leer, lo registra como «no legible (abierta)» y sigue con las demás:

```powershell
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$root = '<raiz-de-trabajo>'; $o = '<original-D1>'; $wt = '<worktree>'; $cand = '<Candidato>'
foreach ($v in $o, $wt, $cand) { if ([string]::IsNullOrWhiteSpace($v) -or $v -match '[<>]') { throw "STOP: un valor sin rellenar" } }
if ($cand -notmatch '^[0-9a-f]{40}$') { throw "STOP: el Candidato no es un SHA completo" }
$work = Join-Path $root 'smoke'; $manifest = Join-Path $work 'copias.json'; $bakFile = Join-Path $root 'respaldo\D1-respaldo.dwg'
foreach ($p in $o, $manifest, $bakFile) { if (-not (Test-Path -LiteralPath $p -PathType Leaf)) { throw "STOP: no existe $p" } }
$man = Get-Content -LiteralPath $manifest -Raw | ConvertFrom-Json
$h0 = [string]$man.original_sha256
if ($h0 -notmatch '^[0-9A-F]{64}$') { throw "STOP: el manifiesto no trae el SHA-256 del original" }
"Original igual: " + ((Get-FileHash -LiteralPath $o -Algorithm SHA256).Hash -eq $h0)
"Respaldo igual: " + ((Get-FileHash -LiteralPath $bakFile -Algorithm SHA256).Hash -eq $h0)
foreach ($c in $man.copias) {
  $f = Join-Path $work ("r1\" + $c.copia); $dwl = [IO.Path]::ChangeExtension($f, '.dwl')
  try { $eq = (Get-FileHash -LiteralPath $f -Algorithm SHA256).Hash -eq $h0 } catch { $eq = 'no legible (abierta)' }
  "{0} igual={1} dwl={2}" -f $c.copia, $eq, (Test-Path -LiteralPath $dwl)
}
Get-ChildItem -LiteralPath (Join-Path $work 'r1') -File | Where-Object { $_.Extension -notin '.dwg', '.dwl', '.dwl2' } | Select-Object Name, Length
Get-ChildItem -LiteralPath $env:TEMP -Filter 'c*.sv$' -ErrorAction SilentlyContinue | Where-Object { $_.Name -match '^c\d\d.*\.sv\$$' } | Select-Object Name, LastWriteTimeUtc
$st = & git -C $wt status --porcelain; if ($LASTEXITCODE -ne 0) { throw "STOP: git status" }
"Worktree limpio: " + (-not $st)
"HEAD = Candidato: " + ((& git -C $wt rev-parse HEAD) -eq $cand)
```

"""

src = os.path.join(base, SRC)
dst = os.path.join(base, DST)
if os.path.exists(dst):
    sys.exit(f"STOP: {DST} ya existe")
text = open(src, encoding="utf-8").read()

text = rep(text, "# I-64 F4B PB-T5 — La corrida previa a la emisión de esta noche, bajo la orden directa del Owner (v2.2)",
           "# I-64 F4B PB-T5 — La corrida previa a la emisión de esta noche, bajo la orden directa del Owner (v2.3)")
text = rep(text, "## Cambios respecto de la v2.1\n", CHANGES + "## Cambios respecto de la v2.1\n")
text = rep(text,
           """foreach ($v in $wt, $cand, $prod) { if ([string]::IsNullOrWhiteSpace($v) -or $v -match '[<>]') { throw "STOP: un valor sin rellenar" } }""",
           """foreach ($v in $wt, $cand, $prod) { if ([string]::IsNullOrWhiteSpace($v) -or $v -match '[<>]') { throw "STOP: un valor sin rellenar" } }
foreach ($v in $cand, $prod) { if ($v -notmatch '^[0-9a-f]{40}$') { throw "STOP: $v no es un SHA completo de 40 caracteres hexadecimales" } }""")
text = section(text, "### 4.2 El build (P-02 a P-04), con AutoCAD cerrado\n",
               "### 4.3 La carpeta, el respaldo y las copias (P-06): un solo bloque\n", SEC_42)
text = rep(text, "### 4.2 El build (P-02 a P-04), con AutoCAD cerrado\n",
           "### 4.2 El build (P-02 a P-04), con AutoCAD cerrado: tres bloques (M-31)\n")
text = section(text, "### 4.3 La carpeta, el respaldo y las copias (P-06): un solo bloque\n",
               "**El reparto de las copias,** una por escenario contaminable:\n", SEC_43)
text = section(text, "### 4.5 La primera copia: el lanzamiento, la carga y el panel (SPB-01 a SPB-04)\n",
               "### 4.6 El inventario, tecleado en cada documento al abrirlo y antes de cualquier orden medida\n", SEC_45)
text = section(text, "7. **Bloque 8**, con `cNN` y `cMM` rellenados:\n", "**Si el paso 6 falla:**\n", SEC_47_B8, keep_start=False)
text = section(text, "### 4.8 Si la instancia se queda sin documentos: cerrarla y relanzarla, sin COM (M-27)\n",
               "**Lo que se hace con lo que salga:**\n", SEC_48)
text = section(text, "**Bloque 10. SPB-51, la integridad.** Solo lee, y se hace siempre, también en la parada de la frontera:\n",
               "**SPB-52, el entorno:**", SEC_49_B10, keep_start=False)
text = rep(text, "| P-03 | Los builds Debug de UI y del Plugin, con AutoCAD cerrado (§4.2) |",
           "| P-03 | Los builds Debug de UI y del Plugin, con AutoCAD cerrado (§4.2, bloque B3) |")

with open(dst, "x", encoding="utf-8", newline="\n") as f:
    f.write(text)
print(f"{DST}: escrito")
