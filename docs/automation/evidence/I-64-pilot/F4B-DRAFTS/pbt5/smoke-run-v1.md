# I-64 F4B PB-T5 — Guía operativa del Smoke-PB contratado (smoke-run v1)

> **Redacción para el repo público:** las rutas locales van como marcadores, salvo la carpeta de trabajo, `<raiz-de-trabajo>\`, que fijó la sesión principal. Al custodiar, `<carpeta-del-Smoke-PB>` pasa a `<carpeta-del-Smoke-PB>` y `<carpeta-de-respaldo>` a `<carpeta-de-respaldo>`, como las copias exportadas `r71…` pasaron a `<copia>` (F4B-E60). Ningún nombre personal ni de cliente. La hora del Owner, en UTC.

**Qué es:** la guía con las órdenes exactas del Smoke-PB **contratado** de PB-T5, el intento 1 de 3, después de la emisión (F4B-E62).
- Los pasos, sus orígenes y sus criterios están en `I-64-f4b-pbt5-package-v2.3.md` ([P]).
- Las decisiones del Owner, en `owner-pbt5-decisions.md` ([OD5]).

**De dónde sale:**
- de la guía de la corrida previa, `pre-run-tonight-v2.4.md`, con todas sus correcciones: I-07 a I-09 y M-18 a M-35 de [IR1] y sus recomprobaciones;
- más lo que añaden las decisiones del Owner:
  - las inyecciones (§4.11);
  - los contextos construidos (§4.12);
  - el recuento del estilo de cotas (§4.13);
  - la dispensa de la UI completa (P-02);
  - la frontera de esta noche (§2);
  - lo que se deja al Owner (§6).
- La corrida previa no llegó a hacerse.

**Los valores:**

| Marcador | Valor |
|---|---|
| `<carpeta-del-Smoke-PB>` | `<carpeta-del-Smoke-PB>` (en LISP, `<carpeta-del-Smoke-PB-con-/>`) |
| `<carpeta-de-respaldo>` | `<carpeta-de-respaldo>` |
| `<worktree>`, `<checkout-principal>`, `<dotnet-del-usuario>`, `<python-del-usuario>`, `<acad.exe>` (la ruta larga y completa), `<dir-del-acceso-directo>`, `<original-D1>`, `<carpeta-de-registros-de-RackCad>`, `<lista-local-de-la-seleccion>` (un archivo local, nunca custodiado, con una ruta de original por línea) | Los pone la sesión en cada bloque. Cada bloque para si alguno se queda sin rellenar |
| `<Candidato>` | `ebd304a3773495fa9071de6611936c4c01a7b06c`, la punta del cierre de PB-T4e (F4B-E61, §291) |
| `<producto>` | `0eac7911a025a59628b446db8b768035175f96de`, el GREEN de PB-T4e; `src/` y `tests/` del Candidato son iguales a los suyos |
| `<run>` | `45147867500`, la CI de push del Candidato (en curso al escribir esto: tiene que terminar 4/4) |
| Los 19 filtros de UI | Literales en el bloque B2 |

**La regla de los bloques de PowerShell** (I-08, M-33 y la medida de la v2.3):
- cada bloque es una orden entera, que funciona sola en un proceso nuevo;
- empieza con `Set-StrictMode -Version Latest` y `$ErrorActionPreference = 'Stop'`;
- para si un valor está vacío o sin rellenar, o si una ruta no existe;
- comprueba `$LASTEXITCODE` tras cada orden externa;
- nunca escribe en la raíz de una unidad;
- y termina con `exit 0` después de su última línea.
- Los bloques largos (B1 y B2) corren en segundo plano.

---

## 1. Qué es, y qué no es

- **Es el Smoke-PB de PB-T5,** emitido por la autorización condicional del Owner (Q-PBT5-7), con el contrato de la forma B (Q-PBT5-8).
  - Un intento es esta corrida sobre este Candidato (Q-PBT5-9).
  - No cuentan las repeticiones por error del operador (dos como máximo), una parada del entorno antes de abrir AutoCAD ni una adenda que pida el Owner.
- **Los resultados que registra la sesión son propuestas.**
  - Los juicios S+O, los OBSERVADO, SPB-07 si ningún dibujo guarda un estilo, la disposición y la firma son del Owner (§6).
  - Antes de su firma, una revisión independiente de solo lectura comprueba que el registro, las capturas y los inventarios coinciden (Q-PBT5-8).
- **Las tres reglas de Smoke-2,** adoptadas (Q-PBT5-9):
  - las etiquetas: NOT AVAILABLE FOR PHYSICAL SMOKE; NO VERIFICABLE FÍSICAMENTE / CUBIERTO POR PRUEBA AUTOMATIZADA; OBSERVADO;
  - traer la evidencia antes de decidir un OBSERVADO;
  - una copia de trabajo por escenario.

## 2. La frontera

**El mensaje del Owner de esta noche**, literal, con la zona horaria redactada:

> «En caso de que se complete antes de las 6 am y quieras ejecutar el smoke, ejecutalo tú ya que estaré dormido, a menos que sean después de las 7 am de `<zona-horaria-del-Owner>`»

**La lectura de la sesión principal** (Q-PBT5-5 no se le preguntó):
- **La sesión opera solo si puede empezar antes de las 13:00 UTC del 2026-11-23,** es decir, si a esa hora tiene cumplidas P-01 a P-08 y hecha la emisión.
- **A las 13:00 UTC,** se para en el siguiente límite de paso y se espera al Owner:
  - no se empieza otro paso con AutoCAD;
  - AutoCAD y la copia abierta se quedan como estén, sin guardar;
  - se registra el punto exacto;
  - y solo se hacen SPB-51 y SPB-52 (§4.9), que leen, con la nota «con AutoCAD abierto».
- **Después, con el Owner:** él opera los gestos que quiera, y la sesión mide lo de máquina, avisa antes de teclear y devuelve el control.
- **Antes de cada paso,** la hora UTC va al registro.

## 3. La preparación (P-01 a P-09, sin la UI completa en local)

| Id | Qué se hace | Criterio (si no se cumple, no se opera) |
|---|---|---|
| P-01 | **El Candidato = `ebd304a3`, la punta del cierre de PB-T4e (F4B-E61).** El bloque 1 y el build van antes del commit de la emisión (F4B-E62). La CI exacta (§4.1, bloque 1) | Árbol limpio; HEAD = Candidato; diff `<producto>..<Candidato> -- src tests assets` vacío; la corrida con `event = push`, la `ref` de la rama y el `head_sha` = Candidato, los cuatro trabajos en `success` y los totales del cierre de PB-T4e. **Si falla una prueba:** PBT5-C15, no se opera |
| P-02 | **Solo** Core Full local y los 19 filtros de UI de los `RequiredTests` del contrato de PB-T4e, cada uno en su proceso (§4.2). **Ninguna suite completa de UI en local** (F4-E59). El archivo de disposición, medido antes y después | Totales iguales a los de la CI. Sin la UI completa en local: la dispensa de Smoke-2, extendida por el Owner (Q-PBT5-1) |
| P-03 | Los builds Debug de UI y del Plugin, con AutoCAD cerrado (§4.2, bloque B3) | 0 errores y ningún `warning CS`; la `ProductVersion` termina en `+<Candidato>`; los SHA-256. **Si hay un AutoCAD abierto, no se toca y no se compila** (PBT5-C08) |
| P-04 | `catalogs` frente a `assets/catalogs` (§4.2) | Iguales byte a byte |
| P-05 | Variables, bundles, el perfil, `PICKFIRST`, la biblioteca de bloques, la carpeta de diseños, el registro de RackCad, el archivo de disposición y keep-awake (§4.1, bloques 2 a 4) | Ninguna `RACKCAD_DEBUG_*`; ningún bundle; nada de RackCad en `Applications`, `Loaded` ni `AssemblyMap`. Las `Dialogs\RackCad.*`, solo contadas. `PICKFIRST` = 1; si vale 0, los pasos con selección por expresión no se hacen, y la variable no se cambia |
| P-06 | `<raiz-de-trabajo>\` nueva y fuera de todo repositorio; el SHA-256 de D1 frente al registrado; el respaldo; las 44 copias (§4.3, un solo bloque) | El bloque termina en «OK». Cualquier STOP para la corrida |
| P-07 | Ningún Worker ni otra sesión en el worktree; PB-T4e cerrada | Sin delegaciones abiertas |
| P-08 | La guía del operador: los pasos de §5, con los literales copiados del `src/` del Candidato por objetos git, los A/B rotulados y las reglas de las capturas del §8 | Hecha antes de abrir AutoCAD, y su sha256 en el registro |
| P-09 | El recuento del estilo de cotas en los demás originales de la selección de Smoke-2 (Q-PBT5-2, «Contar antes en otros»): el bloque 13, sus copias abiertas solo para `RCDUMP estilo`, y el bloque 14 (§4.13). Va antes de SPB-07 | Respaldos y copias iguales a su original; los originales, iguales antes y después; el recuento por dibujo, sin nombres |

**Si no se cumple una de las tres condiciones de la emisión (PBT5-C17),** no se emite y no se opera.

## 4. Las órdenes exactas de la sesión, en orden

**Cada bloque es una orden de PowerShell entera:** se copia y se ejecuta tal cual, después de poner sus valores en las líneas de arriba. No depende de nada que haya definido otro bloque. Lo tecleado en AutoCAD va en §4.5 a §4.7.

### 4.1 Las comprobaciones (P-01, P-05 y P-07)

**Bloque 1. El Candidato y la CI (P-01):**

```powershell
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$wt = '<worktree>'; $cand = 'ebd304a3773495fa9071de6611936c4c01a7b06c'; $prod = '0eac7911a025a59628b446db8b768035175f96de'; $branch = 'architecture/workspace-persistente-rackcad'
foreach ($v in $wt, $cand, $prod) { if ([string]::IsNullOrWhiteSpace($v) -or $v -match '[<>]') { throw "STOP: un valor sin rellenar" } }
foreach ($v in $cand, $prod) { if ($v -notmatch '^[0-9a-f]{40}$') { throw "STOP: $v no es un SHA completo de 40 caracteres hexadecimales" } }
if (-not (Test-Path -LiteralPath $wt -PathType Container)) { throw "STOP: no existe el worktree" }
$st = & git -C $wt status --porcelain; if ($LASTEXITCODE -ne 0) { throw "STOP: git status" }
if ($st) { throw "STOP: el arbol no esta limpio" }
$head = & git -C $wt rev-parse HEAD; if ($LASTEXITCODE -ne 0 -or $head -ne $cand) { throw "STOP: HEAD no es el Candidato" }
$diff = & git -C $wt diff --name-only $prod $cand -- src tests assets; if ($LASTEXITCODE -ne 0) { throw "STOP: git diff" }
if ($diff) { throw "STOP: el diff producto..Candidato en src, tests o assets no esta vacio" }
& gh --version; if ($LASTEXITCODE -ne 0) { throw "STOP: gh no responde" }
Push-Location -LiteralPath $wt
try {
  $runs = & gh run list --branch $branch --commit $cand --event push --json databaseId,headSha,event,headBranch,status,conclusion | ConvertFrom-Json
  if ($LASTEXITCODE -ne 0 -or -not $runs) { throw "STOP: no hay corrida de push del Candidato" }
  $runs | Format-Table databaseId, headSha, event, headBranch, status, conclusion
} finally { Pop-Location }
exit 0
```

Después, con el `<run>` elegido de esa lista, un bloque aparte:

```powershell
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$wt = '<worktree>'; $run = '45147867500'
foreach ($v in $wt, $run) { if ([string]::IsNullOrWhiteSpace($v) -or $v -match '[<>]') { throw "STOP: un valor sin rellenar" } }
Push-Location -LiteralPath $wt
try {
  & gh run view $run --json headSha,event,headBranch,jobs --jq '{headSha,event,headBranch,jobs:[.jobs[]|{name,conclusion}]}'
  if ($LASTEXITCODE -ne 0) { throw "STOP: gh run view" }
  & gh run view $run --log | Select-String -Pattern 'Passed!|Failed!'
} finally { Pop-Location }
exit 0
```

**Bloque 2. Los procesos, las variables y los bundles (P-05 y P-07):**

```powershell
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
Get-Process acad, accoreconsole -ErrorAction SilentlyContinue | Select-Object Id, ProcessName, StartTime, Path
$dbg = @()
foreach ($s in 'User', 'Machine') { $dbg += @([Environment]::GetEnvironmentVariables($s).Keys | Where-Object { $_ -like 'RACKCAD_DEBUG_*' } | ForEach-Object { "$s $_" }) }
$dbg += @(Get-ChildItem Env: | Where-Object Name -like 'RACKCAD_DEBUG_*' | ForEach-Object { "Shell $($_.Name)" })
if ($dbg.Count) { throw ("STOP PBT5-C04: " + ($dbg -join ', ')) }
$plug = @()
foreach ($p in (Join-Path $env:APPDATA 'Autodesk\ApplicationPlugins'), (Join-Path $env:ProgramData 'Autodesk\ApplicationPlugins')) {
  if (Test-Path -LiteralPath $p) { $plug += @(Get-ChildItem -LiteralPath $p -Filter '*RackCad*' | ForEach-Object Name) }
}
if ($plug.Count) { throw ("STOP: bundles de RackCad: " + ($plug -join ', ')) }
"OK: sin RACKCAD_DEBUG_* y sin bundles"
exit 0
```

**Bloque 3. El perfil de AutoCAD, en lectura (P-05):**

```powershell
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$root = 'HKCU:\Software\Autodesk\AutoCAD'
if (-not (Test-Path -LiteralPath $root)) { throw "STOP: no existe el perfil de AutoCAD" }
$keys = @(Get-ChildItem -LiteralPath $root -Recurse -ErrorAction SilentlyContinue)
$hits = @()
foreach ($k in ($keys | Where-Object { $_.PSChildName -in 'Applications', 'Loaded', 'AssemblyMap' })) {
  $hits += @(Get-ChildItem -LiteralPath $k.PSPath -ErrorAction SilentlyContinue | Where-Object PSChildName -match 'RackCad' | ForEach-Object { "SUBKEY $($_.Name)" })
  $props = Get-ItemProperty -LiteralPath $k.PSPath -ErrorAction SilentlyContinue
  if ($props) { $hits += @($props.PSObject.Properties | Where-Object { $_.Name -notlike 'PS*' -and ($_.Name -match 'RackCad' -or "$($_.Value)" -match 'RackCad') } | ForEach-Object { "VALUE $($k.Name)\$($_.Name)" }) }
}
"Entradas de RackCad en Applications, Loaded o AssemblyMap: $($hits.Count)"; $hits
"Dialogs\RackCad.* (solo se cuentan): " + @($keys | Where-Object { $_.Name -match '\\Dialogs\\RackCad' }).Count
foreach ($k in ($keys | Where-Object { $_.PSChildName -eq 'General' })) {
  $props = Get-ItemProperty -LiteralPath $k.PSPath -ErrorAction SilentlyContinue
  if ($props -and ($props.PSObject.Properties.Name -contains 'PICKFIRST')) { "PICKFIRST $($props.PICKFIRST) en $($k.Name)" }
}
if ($hits.Count) { throw ("STOP: hay entradas de RackCad en Applications, Loaded o AssemblyMap: " + ($hits -join '; ')) }
exit 0
```

**Bloque 4. Los datos del usuario (P-05), en lectura:**

```powershell
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$logs = '<carpeta-de-registros-de-RackCad>'
if ([string]::IsNullOrWhiteSpace($logs) -or $logs -match '[<>]') { throw "STOP: un valor sin rellenar" }
$s = Join-Path $env:APPDATA 'RackCad\settings.json'
$d = $null
if (Test-Path -LiteralPath $s) {
  $cfg = Get-Content -LiteralPath $s -Raw | ConvertFrom-Json
  if ($cfg -and ($cfg.PSObject.Properties.Name -contains 'DesignLibraryPath')) { $d = $cfg.DesignLibraryPath }
}
if ([string]::IsNullOrWhiteSpace($d)) { $d = Join-Path $env:APPDATA 'RackCad\Designs' }
"Carpeta de disenos existe: " + (Test-Path -LiteralPath $d)
$lay = Join-Path $env:LOCALAPPDATA 'RackCad\RackFrameConfigurator.layout'
if (Test-Path -LiteralPath $lay) { "Disposicion " + (Get-FileHash -LiteralPath $lay -Algorithm SHA256).Hash + " " + (Get-Item -LiteralPath $lay).LastWriteTimeUtc.ToString('o') } else { "Disposicion: no existe" }
if (Test-Path -LiteralPath $logs) { Get-ChildItem -LiteralPath $logs -Filter ("rackcad-{0:yyyyMMdd}.log" -f (Get-Date).ToUniversalTime()) | Select-Object Name, Length } else { "Carpeta de registros: no existe" }
exit 0
```

**La biblioteca de bloques:** el fragmento de [G] §7.1, tal cual, como bloque propio. Va con `Set-StrictMode` y `$ErrorActionPreference` al principio, y `<catalogs>` = `<worktree>\src\RackCad.Plugin\bin\Debug\net8.0-windows\catalogs`, después de P-03.

### 4.2 El build (P-02 a P-04), con AutoCAD cerrado: tres bloques (M-31)

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
exit 0
```

**Bloque B2. Los 19 filtros de UI y el archivo de disposición** (en segundo plano, cuando B1 haya terminado):

```powershell
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$wt = '<worktree>'; $dotnet = '<dotnet-del-usuario>'
# Los 19 filtros de UI de RequiredTests del contrato de PB-T4e (gate-contract.json, en ebd304a3), literales:
$filters = @(
  'FullyQualifiedName~RackCad.UI.Tests.PushBackFullDraftWindowParityTests',
  'FullyQualifiedName~RackCad.UI.Tests.Workspace',
  'FullyQualifiedName~RackCad.UI.Tests.PushBackWindowOnStateTests',
  'FullyQualifiedName~RackCad.UI.Tests.PushBackClassicCharacterization',
  'FullyQualifiedName~RackCad.UI.Tests.PushBackFullEditorCharacterization',
  'FullyQualifiedName~RackCad.UI.Tests.PushBackStateNet',
  'FullyQualifiedName~RackCad.UI.Tests.PushBackDeclaredCorrection',
  'FullyQualifiedName~RackCad.UI.Tests.Cantilever',
  'FullyQualifiedName~RackCad.UI.Tests.PushBackWorkspaceModeTests',
  'FullyQualifiedName~RackCad.UI.Tests.PushBackWorkspaceApplyCloseTests',
  'FullyQualifiedName~RackCad.UI.Tests.PushBackWorkspaceCarryoverTests',
  'FullyQualifiedName~RackCad.UI.Tests.PushBackFullPanelFieldTests',
  'FullyQualifiedName~RackCad.UI.Tests.NumericFieldValidationTests',
  'FullyQualifiedName~RackCad.UI.Tests.NumericFieldTests',
  'FullyQualifiedName~RackCad.UI.Tests.FullPanelCopyWorkspacePilotReconciliationGreenTests',
  'FullyQualifiedName~RackCad.UI.Tests.FullPanelCopyWorkspacePilotReconciliationViewGreenTests',
  'FullyQualifiedName~RackCad.UI.Tests.PushBackEditAllWindowTests',
  'FullyQualifiedName~RackCad.UI.Tests.PushBackEditAllApplyCloseTests',
  'FullyQualifiedName~RackCad.UI.Tests.PushBackEditAllReloadFieldTests'
)
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
exit 0
```

**Bloque B3. Los builds, la versión y `catalogs`** (en primer plano, cuando B2 haya terminado):

```powershell
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$wt = '<worktree>'; $dotnet = '<dotnet-del-usuario>'; $cand = 'ebd304a3773495fa9071de6611936c4c01a7b06c'
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
exit 0
```

- **Si la `ProductVersion` no identifica el Candidato,** se vuelve a compilar el Plugin con `--no-incremental` en un bloque como B3, y se mide otra vez.
- **Los `warning CS`** se leen en la salida de B3.

### 4.3 La carpeta, el respaldo y las copias (P-06): un solo bloque

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
# Las 44 copias, cada una desde el original
$m = @(foreach ($i in 1..44) {
  $c = 'c{0:d2}.dwg' -f $i
  $dst = Join-Path $r1 $c
  if (Test-Path -LiteralPath $dst) { throw "STOP: $c ya existe" }
  Copy-Item -LiteralPath $o -Destination $dst
  $h = (Get-FileHash -LiteralPath $dst -Algorithm SHA256).Hash
  if ($h -ne $h0) { throw "STOP: $c no es igual al original" }
  [pscustomobject]@{ copia = $c; sha256 = $h }
})
if ($m.Count -ne 44) { throw "STOP: no hay 44 copias" }
$manifest = Join-Path $work 'copias.json'
[pscustomobject]@{ original_sha256 = $h0; respaldo = 'D1-respaldo.dwg'; copias = $m } | ConvertTo-Json -Depth 3 | Out-File -LiteralPath $manifest -Encoding utf8 -NoClobber
if ((Get-FileHash -LiteralPath $o -Algorithm SHA256).Hash -ne $h0) { throw "STOP PBT5-C02: el original cambio" }
"OK: $rootFull nueva; respaldo y 44 copias iguales a D1 ($h0)"
exit 0
```

**El reparto de las copias,** una por escenario contaminable:

| Copia | Pasos |
|---|---|
| c01 | SPB-01 a 05, 07 (a) y 08; el `RCDUMP pre`, que sirve a SPB-47, 48 y 56; los controles de `rcblk`, `rcnest` y `rcdyn` |
| c02 | SPB-06 |
| c03 | SPB-09 |
| c04 | SPB-10 |
| c05 | SPB-15 primero; después SPB-11 a 14, 16 y 49 |
| c06 | SPB-17 |
| c07 y c08 | SPB-18 (el panel y la gemela) y SPB-23 |
| c09 | SPB-19 |
| c10 | SPB-20 |
| c11 | SPB-21 |
| c12 | SPB-24 a 26 |
| c13 | SPB-27 |
| c14 | SPB-28 |
| c15 | SPB-29 a 32 |
| c16 | SPB-33 |
| c17 y c18 | SPB-34 |
| c19 | SPB-35 y 36 |
| c20 y c21 | SPB-37 a 39, **CONSTRUCTED REPRESENTATIVE TEST CASES** |
| c22 y c23 | SPB-37 con el frontal B y SPB-40, **CONSTRUCTED** |
| c24 y c25 | SPB-37 y 41, **CONSTRUCTED** |
| c26 | SPB-37 y 42, **CONSTRUCTED** |
| c27 | SPB-44 |
| c28 | SPB-53 |
| c29 | SPB-54 |
| c30 | SPB-55, **CONSTRUCTED** |
| c31 a c33 | El bloque J: SPB-62, 63 y 64 |
| c34 y c35 | SPB-45 (el panel y la gemela), **CONSTRUCTED** |
| c36 y c37 | SPB-46, **CONSTRUCTED** |
| c38 y c39 | SPB-57, **CONSTRUCTED** |
| c40 y c41 | SPB-58, **CONSTRUCTED** |
| c42, c43 y c44 | SPB-59, 60 y 61, cada una en su AutoCAD (§4.11) |

**Los handles** salen del `RCDUMP pre` de c01, y valen en todas las copias. Para SPB-63 hace falta el rack de D1 con dos laterales enlazadas, una con el sufijo `_1`.

### 4.4 (Unido al §4.3)

§4.3 y §4.4 de la v2.1 son ahora un solo bloque (I-08).

### 4.5 La primera copia: el lanzamiento, la carga y el panel (SPB-01 a SPB-04)

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
exit 0
```

**Bloque 5. El lanzamiento.** Con `$n = 1` y `c01.dwg`; al relanzar por §4.8, con el número siguiente y la copia que toque. El registro se escribe justo al lanzar (M-32), con la hora en `Ticks` (I-09):

```powershell
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$work = '<carpeta-del-Smoke-PB>'; $acad = '<acad.exe>'; $dir = '<dir-del-acceso-directo>'; $copyName = 'c01.dwg'; $n = 1
foreach ($v in $work, $acad, $dir, $copyName) { if ([string]::IsNullOrWhiteSpace($v) -or $v -match '[<>]') { throw "STOP: un valor sin rellenar" } }
if ($copyName -notmatch '^c\d\d\.dwg$' -or $n -lt 1) { throw "STOP: la copia o el numero no son validos" }
if ($acad -match '~') { throw "STOP: la ruta de acad.exe es una ruta corta 8.3" }
if (-not [IO.Path]::IsPathFullyQualified($acad) -or [IO.Path]::GetFullPath($acad) -cne $acad) { throw "STOP: la ruta de acad.exe no es la ruta completa canonica" }
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
exit 0
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
exit 0
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
exit 0
```

Tienen que salir los SHA-256 de B3.

**La línea de base** (SPB-04, M-20):
1. seleccionar el rack del paso con la selección por expresión (§4.6) y esperar a la vista previa del panel;
2. **después,** `(getvar "DBMOD")` y la captura de la lista de Deshacer.

En SPB-15 y SPB-35, lo mismo.

### 4.6 El inventario, tecleado en cada documento al abrirlo y antes de cualquier orden medida

Va en trozos de 16 caracteres como máximo, con 3 s entre `defun` y líneas de 1024 como máximo.

- **`rcj`, `rcn`, `rcref` y `rcdef`** son las de [BM], byte a byte.
- **`c:RCDUMP`** cambia la carpeta, añade `rcblk` y `rcnest`, y **no sobrescribe:** si el archivo de esa etiqueta ya existe, dice «STOP» y no escribe nada (M-26).
- **`rcdyn` y `rcdynchk`** son nuevas, de solo lectura.

```lisp
(vl-load-com)
(defun rcj (bn / be x r) (setq be (cdr (assoc 330 (entget (tblobjname "BLOCK" bn)))) x (cdr (assoc 360 (entget be)))) (if x (if (setq r (dictsearch x "RACKCAD_SELECTIVE")) (apply 'strcat (mapcar 'cdr (vl-remove-if-not '(lambda (p) (= (car p) 1)) r))))))
(defun rcn (bn / e n) (setq n 0 e (entnext (tblobjname "BLOCK" bn))) (while e (setq n (1+ n) e (entnext e))) n)
(defun rcref (f / s i e d o en mn mx r) (setq s (ssget "_X" '((0 . "INSERT"))) i 0) (if s (repeat (sslength s) (setq e (ssname s i) i (1+ i) d (entget e) o (vlax-ename->vla-object e) en (vla-get-EffectiveName o)) (if (rcj en) (progn (setq r (vl-catch-all-apply 'vla-getboundingbox (list o 'mn 'mx))) (write-line (strcat "REF\t" (cdr (assoc 5 d)) "\t" en "\t" (vl-princ-to-string (cdr (assoc 10 d))) "\t" (rtos (cdr (assoc 50 d)) 2 6) "\t" (rtos (cdr (assoc 41 d)) 2 6) "\t" (cdr (assoc 8 d)) "\t" (if (vl-catch-all-error-p r) "NA" (strcat (vl-princ-to-string (vlax-safearray->list mn)) " " (vl-princ-to-string (vlax-safearray->list mx))))) f))))))
(defun rcdef (f / b bn j) (setq b (tblnext "BLOCK" T)) (while b (setq bn (cdr (assoc 2 b))) (if (and (= (logand (cdr (assoc 70 b)) 4) 0) (setq j (rcj bn))) (progn (write-line (strcat "DEF\t" bn "\t" (itoa (rcn bn)) "\t" (itoa (strlen j))) f) (write-line j f))) (setq b (tblnext "BLOCK"))))
(defun rcblk (f / b) (setq b (tblnext "BLOCK" T)) (while b (write-line (strcat "BLK\t" (cdr (assoc 2 b)) "\t" (itoa (cdr (assoc 70 b)))) f) (setq b (tblnext "BLOCK"))))
(defun rcdyn (bn / x) (setq x (cdr (assoc 360 (entget (cdr (assoc 330 (entget (tblobjname "BLOCK" bn)))))))) (if (and x (dictsearch x "ACAD_ENHANCEDBLOCK")) "1" "0"))
(defun rcnest (f / b bn e d n) (setq b (tblnext "BLOCK" T)) (while b (setq bn (cdr (assoc 2 b))) (if (= (logand (cdr (assoc 70 b)) 4) 0) (progn (setq e (entnext (tblobjname "BLOCK" bn))) (while e (setq d (entget e)) (if (and (= (cdr (assoc 0 d)) "INSERT") (rcj (setq n (cdr (assoc 2 d))))) (write-line (strcat "NEST\t" bn "\t" (itoa (cdr (assoc 70 b))) "\t" (rcdyn bn) "\t" n "\t" (cdr (assoc 5 d))) f)) (setq e (entnext e))))) (setq b (tblnext "BLOCK"))))
(defun c:RCDUMP (/ tg p f) (setq tg (getstring "\nEtiqueta: ") p (strcat "<carpeta-del-Smoke-PB-con-/>/inv/" (vl-filename-base (getvar "DWGNAME")) "-" tg ".txt")) (if (findfile p) (princ (strcat "\nSTOP: ya existe la etiqueta " tg)) (progn (setq f (open p "w")) (write-line (strcat "CLAYER\t" (getvar "CLAYER")) f) (rcref f) (rcdef f) (rcblk f) (rcnest f) (close f) (princ "\nRCDUMP ok"))) (princ))
(defun rcdynchk (/ bl b bn o v d n m k f p) (setq p (strcat "<carpeta-del-Smoke-PB-con-/>/inv/" (vl-filename-base (getvar "DWGNAME")) "-rcdyn.txt") bl (vla-get-blocks (vla-get-ActiveDocument (vlax-get-acad-object))) n 0 m 0 k 0) (if (findfile p) (princ "\nSTOP: ya existe el control de rcdyn") (progn (setq f (open p "w") b (tblnext "BLOCK" T)) (while b (setq bn (cdr (assoc 2 b)) o (vl-catch-all-apply 'vla-item (list bl bn)) v (if (vl-catch-all-error-p o) o (vl-catch-all-apply 'vla-get-IsDynamicBlock (list o))) d (cond ((vl-catch-all-error-p v) "ERR") ((= v :vlax-true) "1") (T "0"))) (if (= d "1") (setq n (1+ n))) (if (/= d (rcdyn bn)) (setq m (1+ m))) (if (and (rcj bn) (/= (rcdyn bn) "0")) (setq k (1+ k))) (write-line (strcat "DYN\t" bn "\t" (rcdyn bn) "\t" d (if (rcj bn) "\tRACK" "")) f) (setq b (tblnext "BLOCK"))) (close f) (princ (strcat "\nrcdynchk dinamicas=" (itoa n) " discrepancias=" (itoa m) " rack-no-cero=" (itoa k))))) (princ))
```

**La comprobación de que se cargaron:**

```lisp
(mapcar 'type (list rcj rcn rcref rcdef rcblk rcdyn rcnest c:RCDUMP rcdynchk))
```

Ningún `nil`.

**Los controles, en c01, antes de usar las funciones nuevas:**
- **`rcblk`** (M-21): el `RCDUMP pre` tiene más de 0 líneas `BLK`, y entre ellas todas las definiciones de rack de las líneas `DEF`.
- **`rcnest`** (M-21): da la referencia anidada de la planta dentro del bloque pegado «A$C…» que encontró [BM]. Ese contenedor no es anónimo ni dinámico, así que no cuenta como caso de SPB-47.
- **`rcdyn`** (M-28): `(rcdynchk)` compara `rcdyn` con la propiedad `IsDynamicBlock` de ActiveX, un oráculo independiente, en cada definición de c01, y escribe `c01-rcdyn.txt`.
  - **Control negativo:** `rack-no-cero=0`, es decir, toda definición de vista de rack da `"0"`.
  - **Control positivo:** `dinamicas` > 0, y esas definiciones dan `"1"` en `rcdyn`, porque las piezas de la biblioteca de bloques que dibujan los racks son bloques dinámicos.
  - **Y en las dos:** `discrepancias=0`.
  - Si D1 no tuviera ninguna definición dinámica, el control positivo no se puede hacer. En ese caso SPB-47 queda NOT AVAILABLE con el oráculo como evidencia: no hay contenedores dinámicos.
  - **Si `discrepancias` > 0, o el control negativo falla:** `rcdyn` no se usa, y SPB-47 queda LIMITATION.
- **Si cualquier control falla,** esa función no se usa, y su paso queda LIMITATION.

**Otras órdenes de lectura:**
- `RCDUMP` y la etiqueta. **Cada etiqueta, una sola vez por copia.**
- `(getvar "DBMOD")`.
- La selección por expresión, `(sssetfirst nil (ssadd (handent "<handle>")))`.
- En RACKEDITAR, `(handent "<handle>")`.

**N-I1:** nada de esto entre una orden medida y sus `U`.

### 4.7 El paso de una copia a la siguiente (I-07)

**Con la copia `cNN` activa y `cMM` como la siguiente:**
1. **El panel:** «Descartar borradores de este documento» → «Descartar». Si no hay borradores en riesgo, se registra el mensaje.
2. **Tecleado:**

   ```lisp
   (vla-activate (vla-open (vla-get-documents (vlax-get-acad-object)) "<carpeta-del-Smoke-PB-con-/>/r1/cMM.dwg"))
   ```

3. **En `cMM`,** antes de cualquier captura que se vaya a custodiar:

   ```text
   EXTERNALREFERENCESCLOSE
   ```

4. **`(getvar "DWGNAME")`:** tiene que dar `"cMM.dwg"`.
5. **El inventario** (§4.6), con la comprobación de los `type`.
6. **El cierre de la anterior,** sin guardar, desde `cMM`:

   ```lisp
   (vla-close (vla-item (vla-get-documents (vlax-get-acad-object)) "cNN.dwg") :vlax-false)
   ```

7. **Bloque 8**, con `cNN` y `cMM` rellenados:

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
   $chk = [pscustomobject]@{
     dwlAnteriorExiste  = Test-Path -LiteralPath (Join-Path $r1 "$prev.dwl")
     anteriorIgualD1    = (Get-FileHash -LiteralPath (Join-Path $r1 "$prev.dwg") -Algorithm SHA256).Hash -eq $known
     dwlSiguienteExiste = Test-Path -LiteralPath (Join-Path $r1 "$next.dwl")
     titulo             = $q.MainWindowTitle
   }
   $chk
   $fail = @()
   if ($chk.dwlAnteriorExiste) { $fail += "la copia anterior $prev sigue abierta (.dwl)" }
   if (-not $chk.anteriorIgualD1) { $fail += "la copia anterior $prev ya no es igual a D1" }
   if (-not $chk.dwlSiguienteExiste) { $fail += "la copia siguiente $next no esta abierta (.dwl)" }
   if ($chk.titulo -notmatch [regex]::Escape("$next.dwg")) { $fail += "el titulo no nombra $next.dwg" }
   if ($fail.Count) { throw ("STOP: " + ($fail -join '; ')) }
   exit 0
   ```

   Lo esperado: `False`, `True`, `True`, y el título con `cMM.dwg`.

**Si el paso 6 falla:**
- **un veto de RackCad:** `(vla-activate (vla-item (vla-get-documents (vlax-get-acad-object)) "cNN.dwg"))` → «Descartar» → volver a `cMM` con la misma orden y su nombre → repetir el paso 6;
- **cualquier otro error:** se registra literal, y `cNN` se deja abierta hasta SPB-50.

**`NETLOAD` no se repite** en la misma instancia.

### 4.8 Si la instancia se queda sin documentos: cerrarla y relanzarla, sin COM (M-27)

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
exit 0
```

**Lo que se hace con lo que salga:**
- **una pregunta de guardar:** «No». Sin documentos no debería salir;
- **el veto de RackCad:** STOP;
- **al relanzar:** el bloque 5 con `$n` siguiente y la copia que toque. Queda un `acad-session-<n>.json` nuevo con el PID nuevo, y otra vez `NETLOAD`, `RACKPANEL` y el inventario;
- **si `vla-open` falla:** se registra, y no se cierra la copia activa;
- **si aparece otro `acad.exe`:** no se toca, y se registra;
- **si AutoCAD se cuelga o se cierra solo:** PBT5-C12.

### 4.9 El cierre (SPB-50 a SPB-52)

**SPB-50,** en AutoCAD:
1. en cada copia que siga abierta, «Descartar borradores de este documento» → «Descartar»;
2. las demás, cerradas desde la última con `vla-close … :vlax-false`, como en §4.7;
3. la última, con `CLOSE` → «No»;
4. la instancia, con el bloque 9.

**Bloque 10. SPB-51, la integridad.** Solo lee, y se hace siempre, también en la parada de la frontera. Si una copia está abierta en AutoCAD y no se puede leer, lo registra como «no legible (abierta)» y sigue con las demás:

```powershell
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$root = '<raiz-de-trabajo>'; $o = '<original-D1>'; $wt = '<worktree>'; $cand = 'ebd304a3773495fa9071de6611936c4c01a7b06c'
foreach ($v in $o, $wt, $cand) { if ([string]::IsNullOrWhiteSpace($v) -or $v -match '[<>]') { throw "STOP: un valor sin rellenar" } }
if ($cand -notmatch '^[0-9a-f]{40}$') { throw "STOP: el Candidato no es un SHA completo" }
$work = Join-Path $root 'smoke'; $manifest = Join-Path $work 'copias.json'; $bakFile = Join-Path $root 'respaldo\D1-respaldo.dwg'
foreach ($p in $o, $manifest, $bakFile) { if (-not (Test-Path -LiteralPath $p -PathType Leaf)) { throw "STOP: no existe $p" } }
$man = Get-Content -LiteralPath $manifest -Raw | ConvertFrom-Json
$h0 = [string]$man.original_sha256
if ($h0 -notmatch '^[0-9A-F]{64}$') { throw "STOP: el manifiesto no trae el SHA-256 del original" }
$fail = @()
$origEq = (Get-FileHash -LiteralPath $o -Algorithm SHA256).Hash -eq $h0
"Original igual: $origEq"; if (-not $origEq) { $fail += "el original cambio (PBT5-C02)" }
$bakEq = (Get-FileHash -LiteralPath $bakFile -Algorithm SHA256).Hash -eq $h0
"Respaldo igual: $bakEq"; if (-not $bakEq) { $fail += "el respaldo cambio" }
foreach ($c in $man.copias) {
  $f = Join-Path $work ("r1\" + $c.copia); $dwl = [IO.Path]::ChangeExtension($f, '.dwl')
  if (-not (Test-Path -LiteralPath $f -PathType Leaf)) { $fail += "falta $($c.copia)"; continue }
  try { $eq = (Get-FileHash -LiteralPath $f -Algorithm SHA256).Hash -eq $h0 } catch { $eq = 'no legible (abierta)' }
  "{0} igual={1} dwl={2}" -f $c.copia, $eq, (Test-Path -LiteralPath $dwl)
  if ($eq -is [bool] -and -not $eq) { $fail += "$($c.copia) ya no es igual a D1" }
}
Get-ChildItem -LiteralPath (Join-Path $work 'r1') -File | Where-Object { $_.Extension -notin '.dwg', '.dwl', '.dwl2' } | Select-Object Name, Length
Get-ChildItem -LiteralPath $env:TEMP -Filter 'c*.sv$' -ErrorAction SilentlyContinue | Where-Object { $_.Name -match '^c\d\d.*\.sv\$$' } | Select-Object Name, LastWriteTimeUtc
$st = & git -C $wt status --porcelain; if ($LASTEXITCODE -ne 0) { throw "STOP: git status" }
$clean = -not $st
"Worktree limpio: $clean"; if (-not $clean) { $fail += "el worktree no esta limpio" }
& git -C $wt merge-base --is-ancestor $cand HEAD; $anc = ($LASTEXITCODE -eq 0)
$dd = & git -C $wt diff --name-only $cand HEAD -- src tests assets
$headOk = $anc -and -not $dd
"El Candidato es antepasado de HEAD, sin cambios en src, tests ni assets: $headOk"; if (-not $headOk) { $fail += "HEAD no desciende del Candidato sin cambios de codigo" }
if ($fail.Count) { throw ("STOP: " + ($fail -join '; ')) }
exit 0
```

**SPB-52, el entorno:** los bloques 2 a 4 del §4.1, otra vez, cada uno en su orden. Solo leen, y se hacen siempre.

### 4.10 El orden de la corrida

1. **La preparación, sin AutoCAD:**
   - el bloque 1 (P-01), con HEAD = Candidato;
   - B1 y B2, en segundo plano, y después B3 (P-02 a P-04);
   - los bloques 2, 3 y 4 (P-05) y el 4b;
   - `emit_pbt5.py --dry` y después `--commit` (la emisión, F4B-E62), que solo añade un commit documental: el DLL de B3 no se vuelve a compilar;
   - el bloque de §4.3 (P-06);
   - el bloque 13 (P-09, las copias para el estilo);
   - P-07 y P-08.
2. **La instancia principal de AutoCAD,** con el bloque 5 y `c01`:
   - c01, con SPB-01 a 05, 07 (a) y 08, el `RCDUMP pre` y los controles;
   - **las copias `eNN` del estilo,** abiertas una a una con el paso de §4.7, solo para el `RCDUMP estilo`;
   - el bloque 14, el recuento;
   - si algún dibujo guarda un estilo, `s01` y `s02` para SPB-07 (a) y (b) (§4.13);
   - c02 a c41, por la tabla de §4.3. Los contextos construidos son c34 a c41 (§4.12);
   - el cierre de la instancia (SPB-50 y el bloque 9).
3. **Las tres inyecciones,** cada una en su AutoCAD propio (§4.11): c42, c43 y c44.
4. **El cierre:** el bloque 10 (SPB-51) y los bloques 2, 3 y 4 (SPB-52).

### 4.11 Las inyecciones (SPB-59 a SPB-61; Q-PBT5-10, «Hacerlas como en Smoke-2»)

Cada inyección va en **un AutoCAD propio**, lanzado con la variable **solo en el entorno de ese proceso**. Ninguna otra instancia de la sesión puede estar viva.

| Paso | Copia | `$n` | Variable | Valor |
|---|---|---|---|---|
| SPB-59 | c42 | el siguiente libre | `RACKCAD_DEBUG_FAIL_PILOT_UPDATE_UNIT` | `2` |
| SPB-60 | c43 | el siguiente | `RACKCAD_DEBUG_FAIL_PILOT_UPDATE_SEAL` | `before` |
| SPB-61 | c44 | el siguiente | `RACKCAD_DEBUG_FAIL_PILOT_UPDATE_SEAL` | `after` |

**Bloque 11. El lanzamiento con la inyección:**

```powershell
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$work = '<carpeta-del-Smoke-PB>'; $acad = '<acad.exe>'; $dir = '<dir-del-acceso-directo>'
$copyName = 'c42.dwg'; $n = '<n>'; $injName = 'RACKCAD_DEBUG_FAIL_PILOT_UPDATE_UNIT'; $injValue = '2'
foreach ($v in $work, $acad, $dir, $copyName, $n, $injName, $injValue) { if ([string]::IsNullOrWhiteSpace($v) -or $v -match '[<>]') { throw "STOP: un valor sin rellenar" } }
$n = [int]$n
$ok = @{ 'RACKCAD_DEBUG_FAIL_PILOT_UPDATE_UNIT' = @('2'); 'RACKCAD_DEBUG_FAIL_PILOT_UPDATE_SEAL' = @('before', 'after') }
if (-not $ok.ContainsKey($injName) -or $injValue -notin $ok[$injName]) { throw "STOP: inyeccion no prevista" }
if ($copyName -notin 'c42.dwg', 'c43.dwg', 'c44.dwg') { throw "STOP: la copia no es una de las inyecciones" }
if ($acad -match '~') { throw "STOP: la ruta de acad.exe es una ruta corta 8.3" }
if (-not [IO.Path]::IsPathFullyQualified($acad) -or [IO.Path]::GetFullPath($acad) -cne $acad) { throw "STOP: la ruta de acad.exe no es la ruta completa canonica" }
$copy = Join-Path $work "r1\$copyName"
foreach ($p in $work, $acad, $dir, $copy) { if (-not (Test-Path -LiteralPath $p)) { throw "STOP: no existe $p" } }
$dbg = @()
foreach ($s in 'User', 'Machine') { $dbg += @([Environment]::GetEnvironmentVariables($s).Keys | Where-Object { $_ -like 'RACKCAD_DEBUG_*' }) }
$dbg += @(Get-ChildItem Env: | Where-Object Name -like 'RACKCAD_DEBUG_*' | ForEach-Object Name)
if ($dbg.Count) { throw ("STOP PBT5-C04 antes de lanzar: " + ($dbg -join ', ')) }
foreach ($f in Get-ChildItem -LiteralPath $work -Filter 'acad-session-*.json') {
  $r = Get-Content -LiteralPath $f.FullName -Raw | ConvertFrom-Json
  $q = Get-Process -Id ([int]$r.Id) -ErrorAction SilentlyContinue
  if ($q -and $q.ProcessName -eq 'acad' -and [long]$r.StartTicksUtc -eq $q.StartTime.ToUniversalTime().Ticks) { throw "STOP: sigue viva la instancia $($r.Id) de la sesion" }
}
$rec = Join-Path $work ("acad-session-{0}.json" -f $n)
if (Test-Path -LiteralPath $rec) { throw "STOP: $rec ya existe" }
$psi = New-Object System.Diagnostics.ProcessStartInfo
$psi.FileName = $acad
$psi.Arguments = '/product ACAD /language "en-US" "' + $copy + '"'
$psi.WorkingDirectory = $dir
$psi.UseShellExecute = $false
foreach ($k in @($psi.Environment.Keys | Where-Object { $_ -like 'RACKCAD_DEBUG_*' })) { throw "STOP PBT5-C04: $k en el entorno heredado" }
$psi.Environment[$injName] = $injValue
$p = [System.Diagnostics.Process]::Start($psi)
[pscustomobject]@{ Id = $p.Id; Path = $psi.FileName; StartTicksUtc = $p.StartTime.ToUniversalTime().Ticks; Copy = $copyName; Injection = "$injName=$injValue" } |
  ConvertTo-Json | Out-File -LiteralPath $rec -Encoding utf8 -NoClobber
$after = @(Get-ChildItem Env: | Where-Object Name -like 'RACKCAD_DEBUG_*' | ForEach-Object Name)
foreach ($s in 'User', 'Machine') { $after += @([Environment]::GetEnvironmentVariables($s).Keys | Where-Object { $_ -like 'RACKCAD_DEBUG_*' }) }
if ($after.Count) { throw ("STOP PBT5-C04: la variable salio del proceso: " + ($after -join ', ')) }
try { $idle = $p.WaitForInputIdle(180000); "WaitForInputIdle: $idle" } catch { "WaitForInputIdle fallo: $($_.Exception.Message). El registro ya esta escrito: seguir con el bloque 6" }
Get-Content -LiteralPath $rec -Raw
exit 0
```

**Después, en esa instancia:**
1. el bloque 6;
2. los diálogos de arranque, sin «Always ignore»;
3. `EXTERNALREFERENCESCLOSE`, `(vl-load-com)`, el `NETLOAD` por expresión con «Load Once», `RACKPANEL` y el bloque 7;
4. el inventario (§4.6);
5. **la preparación:** RACKEDITAR → «Insertar lateral» del último poste → el punto `9000,-1642` (el de [BM] M-6a) → `RCDUMP ins`;
6. **el paso:** seleccionar el rack → «Editar» → «Editar todo» → un frente menos → «Aplicar al borrador» → «Actualizar» → `RCDUMP post`. Se registra el texto literal y la lista de Deshacer;
7. **el cierre:** «Descartar borradores de este documento» → «Descartar» → `QUIT` → «No»;
8. el bloque 12.

**Bloque 12. La instancia de la inyección salió, y la variable no está en ninguna parte:**

```powershell
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$work = '<carpeta-del-Smoke-PB>'
if (-not (Test-Path -LiteralPath $work -PathType Container)) { throw "STOP: no existe $work" }
$recFile = Get-ChildItem -LiteralPath $work -Filter 'acad-session-*.json' | Sort-Object { [int]($_.BaseName -replace '\D', '') } | Select-Object -Last 1
if (-not $recFile) { throw "STOP: no hay registro de la instancia" }
$rec = Get-Content -LiteralPath $recFile.FullName -Raw | ConvertFrom-Json
if (-not ($rec.PSObject.Properties.Name -contains 'Injection')) { throw "STOP: el ultimo registro no es el de una inyeccion" }
$q = Get-Process -Id ([int]$rec.Id) -ErrorAction SilentlyContinue
if ($q -and $q.ProcessName -eq 'acad' -and [long]$rec.StartTicksUtc -eq $q.StartTime.ToUniversalTime().Ticks) { throw "STOP: la instancia de la inyeccion sigue viva" }
$dbg = @(Get-ChildItem Env: | Where-Object Name -like 'RACKCAD_DEBUG_*' | ForEach-Object Name)
foreach ($s in 'User', 'Machine') { $dbg += @([Environment]::GetEnvironmentVariables($s).Keys | Where-Object { $_ -like 'RACKCAD_DEBUG_*' }) }
if ($dbg.Count) { throw ("STOP PBT5-C04 despues: " + ($dbg -join ', ')) }
"OK: salio la instancia $($rec.Id) ($($rec.Injection)) y no hay RACKCAD_DEBUG_* en el usuario, la maquina ni el shell"
exit 0
```

### 4.12 Los contextos construidos (SPB-45, 46, 57 y 58; Q-PBT5-4, «Construirlos y rotularlos»)

**Lo común:**
- **cada contexto** va en una copia para el panel y una **gemela** para RACKEDITAR, construidas igual;
- **todo va rotulado** **CONSTRUCTED REPRESENTATIVE TEST CASES**: las etiquetas de `RCDUMP` empiezan por `CONSTRUCTED-`, y las capturas llevan `CONSTRUCTED` en el nombre;
- **todo se teclea** en la línea de órdenes, en trozos de 16 caracteres como máximo;
- `<h…>` son handles y `<…>` nombres, tomados del `RCDUMP ins` de esa copia;
- **la preparación**, igual en las cuatro: RACKEDITAR sobre el rack → «Insertar lateral» del último poste → el punto `9000,-1642` → `RCDUMP CONSTRUCTED-ins`. Es un gesto del producto.

**SPB-45, la referencia en una presentación** (c34, el panel; c35, la gemela):

```lisp
(command "_.-LAYOUT" "_S" (car (layoutlist)))
(command "_.PSPACE")
(command "_.-INSERT" "<nombre-de-la-definicion-del-lateral>" "0,0" "1" "1" "0")
(command "_.MODEL")
```

Después, `RCDUMP CONSTRUCTED-construido`. Las líneas `REF` cuentan las referencias de modelo y de papel, porque `ssget "_X"` las lee todas.

**SPB-46, la referencia dentro de un bloque del usuario** (c36 y c37):

```lisp
(command "_.-BLOCK" "ZZ-CONSTRUCTED-SPB46" "0,0" (handent "<h-de-la-referencia-del-lateral>") "")
(command "_.-INSERT" "ZZ-CONSTRUCTED-SPB46" "0,0" "1" "1" "0")
```

Después, `RCDUMP CONSTRUCTED-construido`: una línea `NEST` con `ZZ-CONSTRUCTED-SPB46` como contenedor.

**SPB-57, la capa bloqueada** (c38 y c39):

```lisp
(command "_.-LAYER" "_LO" "<capa-de-la-referencia-del-lateral>" "")
```

La capa sale de la línea `REF` del lateral en `RCDUMP CONSTRUCTED-ins`. Después, `RCDUMP CONSTRUCTED-construido`.

**SPB-58, el último vínculo** (c40 y c41), sobre un rack sin referencias anidadas (el que [BM] llamó PB E):

```lisp
(command "_.ERASE" (handent "<h-de-otra-referencia-del-rack>") "")
(command "_.-PURGE" "_B" "<nombre-de-esa-definicion>" "_N")
```

- Se repiten las dos órdenes por cada referencia y cada definición del rack que no sea el lateral.
- Después, `RCDUMP CONSTRUCTED-construido`: el rack solo tiene la definición y la referencia del lateral.
- La selección del paso es el propio lateral.

**El paso, en las cuatro:**
- **en el panel:** seleccionar el rack → «Editar» → «Editar todo» → un frente menos (el del lateral) → «Aplicar al borrador» → «Actualizar» → `RCDUMP CONSTRUCTED-post`, y el texto literal;
- **en la gemela:** RACKEDITAR → «- frente» → «Actualizar» → `RCDUMP CONSTRUCTED-post`, y el mensaje de la línea de órdenes;
- **la comparación** de los dos `post`, con las herramientas de [BM], va al registro.

### 4.13 El recuento del estilo de cotas (P-09 y SPB-07; Q-PBT5-2, «Contar antes en otros»)

**Bloque 13. Respaldos y copias de los demás originales de la selección de Smoke-2.** Va después del bloque de §4.3. La lista es local y no lleva D1 ni D2: D1 se cuenta en c01, y D2 ya está contado en [BM].

```powershell
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$root = '<raiz-de-trabajo>'; $list = '<lista-local-de-la-seleccion>'
if ([string]::IsNullOrWhiteSpace($list) -or $list -match '[<>]') { throw "STOP: un valor sin rellenar" }
$work = Join-Path $root 'smoke'; $bak = Join-Path $root 'respaldo'; $est = Join-Path $work 'estilo'
foreach ($p in $work, $bak) { if (-not (Test-Path -LiteralPath $p -PathType Container)) { throw "STOP: no existe $p (primero el bloque de 4.3)" } }
if (-not (Test-Path -LiteralPath $list -PathType Leaf)) { throw "STOP: no existe la lista" }
if (Test-Path -LiteralPath $est) { throw "STOP: $est ya existe" }
$orig = @(Get-Content -LiteralPath $list | Where-Object { $_.Trim() })
if ($orig.Count -eq 0 -or $orig.Count -gt 99) { throw "STOP: la lista esta vacia o es demasiado larga" }
New-Item -ItemType Directory -Path $est | Out-Null
$rows = @(); $hs = @{}
for ($i = 0; $i -lt $orig.Count; $i++) {
  $o = $orig[$i].Trim(); $tag = 'e{0:d2}' -f ($i + 1)
  if (-not (Test-Path -LiteralPath $o -PathType Leaf)) { throw "STOP: no existe el original de $tag" }
  $h = (Get-FileHash -LiteralPath $o -Algorithm SHA256).Hash; $hs[$tag] = $h
  $b = Join-Path $bak "$tag-respaldo.dwg"; $c = Join-Path $est "$tag.dwg"
  foreach ($d in $b, $c) { if (Test-Path -LiteralPath $d) { throw "STOP: ya existe $d" } }
  Copy-Item -LiteralPath $o -Destination $b
  if ((Get-FileHash -LiteralPath $b -Algorithm SHA256).Hash -ne $h) { throw "STOP: el respaldo de $tag no es igual a su original" }
  Copy-Item -LiteralPath $o -Destination $c
  if ((Get-FileHash -LiteralPath $c -Algorithm SHA256).Hash -ne $h) { throw "STOP: la copia de $tag no es igual a su original" }
  $rows += [pscustomobject]@{ copia = "$tag.dwg"; sha256 = $h }
}
for ($i = 0; $i -lt $orig.Count; $i++) {
  $tag = 'e{0:d2}' -f ($i + 1)
  if ((Get-FileHash -LiteralPath $orig[$i].Trim() -Algorithm SHA256).Hash -ne $hs[$tag]) { throw "STOP PBT5-C02: cambio el original de $tag" }
}
$rows | ConvertTo-Json | Out-File -LiteralPath (Join-Path $est 'estilo.json') -Encoding utf8 -NoClobber
"OK: $($rows.Count) respaldos y copias para el recuento del estilo"
exit 0
```

**En la instancia principal,** después de c01: cada `eNN` se abre con el paso de §4.7, y la ruta va con `/estilo/eNN.dwg` en lugar de `/r1/cNN.dwg`. En cada una:
1. `EXTERNALREFERENCESCLOSE`;
2. el inventario;
3. `RCDUMP estilo`;
4. y al pasar a la siguiente, `vla-close … :vlax-false`.

En las `eNN` no se hace nada más. El bloque 8 no aplica, porque compara con D1; en su lugar, `Test-Path` del `.dwl` y el hash frente a `estilo.json`.

**Bloque 14. El recuento,** fuera de AutoCAD, con `dimkeys.py` de [BM], sacado byte a byte de los objetos git:

```powershell
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$work = '<carpeta-del-Smoke-PB>'; $main = '<checkout-principal>'; $py = '<python-del-usuario>'
foreach ($v in $main, $py) { if ([string]::IsNullOrWhiteSpace($v) -or $v -match '[<>]') { throw "STOP: un valor sin rellenar" } }
foreach ($p in $work, $main, $py) { if (-not (Test-Path -LiteralPath $p)) { throw "STOP: no existe $p" } }
$rev = '7da361eb:docs/automation/evidence/I-64-pilot/F4B-PBT0-CHARACTERIZATION/block-m/tools/dimkeys.py'
$tools = Join-Path $work 'tools'; $dk = Join-Path $tools 'dimkeys.py'
if (-not (Test-Path -LiteralPath $tools)) { New-Item -ItemType Directory -Path $tools | Out-Null }
if (Test-Path -LiteralPath $dk) { throw "STOP: $dk ya existe" }
& git -C $main show $rev > $dk; if ($LASTEXITCODE -ne 0) { throw "STOP: git show" }
$want = & git -C $main rev-parse $rev; $got = & git -C $main hash-object $dk
if ($LASTEXITCODE -ne 0 -or $want -ne $got) { throw "STOP: dimkeys.py no es byte a byte el del repositorio" }
$inv = @(Get-ChildItem -LiteralPath (Join-Path $work 'inv') -File | Where-Object { $_.Name -eq 'c01-pre.txt' -or $_.Name -match '^e\d\d-estilo\.txt$' } | Sort-Object Name)
if ($inv.Count -lt 2) { throw "STOP: faltan inventarios (c01-pre y los eNN-estilo)" }
foreach ($f in $inv) {
  "== $($f.Name)"
  & $py -I $dk $f.FullName; if ($LASTEXITCODE -ne 0) { throw "STOP: dimkeys.py en $($f.Name)" }
}
"OK: recuento hecho; se lee en la salida: los sobres Push Back con DimensionStyle no nulo, por dibujo"
exit 0
```

**Después del recuento:**
- **Si algún sobre Push Back guarda un `DimensionStyle` no nulo:** dos copias nuevas de ese original, `s01.dwg` y `s02.dwg`, en `<carpeta-del-Smoke-PB>\r1\`, con un bloque como el 13 (sin sobrescribir, con el hash comprobado).
  - **SPB-07 (a)** va en `s01`: RACKEDITAR → «Actualizar» → el estilo de las cotas.
  - **SPB-07 (b)** va en `s02`: «Editar todo» → «Actualizar».
- **Si ninguno:** SPB-07 queda **PENDIENTE DEL OWNER**, para que decida si acepta el criterio 3 cumplido en parte. No se fabrica nada.

## 5. Los pasos, por copia y en orden

Es el detalle de [P] §4.2: los 65 pasos, con los bloques I y J, las inyecciones y los contextos construidos.

| Orden | Copia | Pasos | Notas |
|---|---|---|---|
| 1 | c01 | SPB-01 a 05; `RCDUMP pre` y los controles de `rcblk`, `rcnest` y `rcdyn`; SPB-07 (a), el recuento en D1; SPB-08 | La línea de base de M-20 |
| 2 | e01 a eNN | P-09: el `RCDUMP estilo` de cada una (§4.13) | Solo lectura; después, el bloque 14 |
| 3 | s01 y s02 | SPB-07 (a) y (b), si algún dibujo guarda un estilo | Si ninguno: SPB-07 PENDIENTE DEL OWNER |
| 4 | c02 a c04 | SPB-06, 09 (con la geometría) y 10 | |
| 5 | c05 | SPB-15 primero; después SPB-11 a 14, 16 y 49 | SPB-12 y 13, con las coordenadas y la captura de después |
| 6 | c06 | SPB-17 | Solo si existe la carpeta de diseños |
| 7 | c07 a c11 | SPB-18 (y su gemela c08), 23, 19, 20 y 21; SPB-22 se registra | SPB-18 con la purga (f); SPB-23 con el coste de `Equals` |
| 8 | c12 a c14 | SPB-24 a 28 | SPB-26 y 28 con los campos del sobre que no son del editor |
| 9 | c15 a c19 | SPB-29 a 36 | En SPB-31 y SPB-36 se anota OWNER-01 de PB-T4e: con la ventana abierta, la paleta no se repinta y sigue mostrando «Limpio (posiblemente obsoleto)» y el valor de antes, hasta cerrar (F4B-E61). Es informativo |
| 10 | c20 a c26 | SPB-37 a 42, **CONSTRUCTED REPRESENTATIVE TEST CASES**; SPB-43 se registra | |
| 11 | c27 | SPB-44 | |
| 12 | — | SPB-47 (con `rcnest` y `rcdyn`) y SPB-48, recuentos sobre los inventarios; SPB-56, el registro | |
| 13 | c28 a c30 | SPB-53, 54 y 55 (rotulado) | |
| 14 | c31 a c33 | SPB-62, 63 y 64; SPB-65 se registra | |
| 15 | c34 a c41 | SPB-45, 46, 57 y 58, cada uno en su copia y en su gemela, **CONSTRUCTED** (§4.12) | |
| 16 | — | SPB-50, la instancia principal: «Descartar», `CLOSE` → «No» y el bloque 9 | |
| 17 | c42 a c44 | SPB-59, 60 y 61, cada una en su AutoCAD (§4.11) | Los bloques 11 y 12 |
| 18 | — | SPB-51 (el bloque 10) y SPB-52 (los bloques 2, 3 y 4) | Siempre |

**Recuento:**

| | Pasos |
|---|---|
| **Total** | **65** |
| S | 56 |
| S+O, PENDIENTE DE REVISIÓN DEL OWNER | 9: SPB-08, 09, 16, 18, 20, 23, 25, 36 y 39 |
| Solo registrados | 3: SPB-22, SPB-43 y SPB-65 |
| Precondiciones | 9: P-01 a P-09 |

**Si la frontera de las 13:00 UTC llega a mitad:** se para en el límite de paso. Lo que falte, se sigue con el Owner, en el mismo orden.

## 6. Lo que la sesión no hace, y lo que se deja al Owner

**Lo que la sesión no hace:**
- **juzgar los S+O.** Se miden y se capturan, y quedan PENDIENTE DE REVISIÓN DEL OWNER;
- **decidir un OBSERVADO.** Se trae la evidencia;
- **fabricar el estilo de cotas,** ni dar SPB-07 por NOT AVAILABLE. Si ningún dibujo lo guarda, vuelve al Owner;
- **la UI completa en local** (Q-PBT5-1);
- **la revisión independiente de la evidencia.** La hace un revisor aparte, en solo lectura (Q-PBT5-8);
- **la disposición, la firma ni el gate-close;**
- **operar AutoCAD pasadas las 13:00 UTC** sin el Owner;
- **guardar una copia,** abrir un original, sobrescribir un archivo, escribir en la raíz de una unidad o tocar un AutoCAD que no abrió la sesión;
- **usar COM** o `Stop-Process`;
- **poner `RACKCAD_DEBUG_*`** fuera del entorno de los AutoCAD de SPB-59 a SPB-61;
- **cambiar la configuración de AutoCAD,** el registro o una variable de sistema;
- **custodiar** la paleta de referencias externas, el aviso de NETLOAD, un diálogo de archivos o una captura sin redactar (§8).

**Lo que se deja al Owner,** en `owner-summary.md`:
- **«Para tu revisión»:** los 9 S+O, cada uno con su captura recortada y la medida de la sesión;
- **SPB-07,** si ningún dibujo guarda un estilo: si acepta el criterio 3 cumplido en parte;
- **los OBSERVADO,** cada uno con toda su evidencia;
- **los FAIL,** con su evidencia, para decidir la tarea de corrección;
- **la información:** R-T4-39 y OWNER-01 de PB-T4e;
- **después de la revisión independiente de la evidencia,** su disposición y su firma.

## 7. El registro

En `<carpeta-del-Smoke-PB>`, y después en `F4B-SMOKE-PB/run/`, redactado.

**La cabecera de `run-log.md`** (M-22):
- «Smoke-PB contratado de PB-T5 (F4B-E62), intento 1 de 3»;
- el sha256 de este documento, el del paquete y el de la guía redactada;
- el Candidato y el producto; la versión de AutoCAD; el SHA-256 de la biblioteca de bloques;
- los PID de la sesión, de los `acad-session-<n>.json`;
- la hora de inicio frente a las 13:00 UTC.

**Por cada paso:**
- la hora UTC;
- la copia;
- lo hecho;
- lo observado, con los literales;
- el resultado propuesto;
- la evidencia: el inventario, las capturas y las coordenadas de los clics en los resultados negativos.

Los S+O llevan **PENDIENTE DE REVISIÓN DEL OWNER**. Los errores del operador (dos repeticiones como máximo) y los incidentes, declarados. Cada cambio de copia queda con la salida del bloque 8.

**Lo demás:**
- `inventories/`: los crudos, en local; se custodian sus resúmenes, con su SHA-256, y `c01-rcdyn.txt` resumido;
- `integrity.md` (la salida del bloque 10), `environment-pre.md` y `environment-post.md`;
- `owner-summary.md`.

**La custodia:** un commit documental de la sesión, con `<raiz-de-trabajo>\…` redactado. No cambia el Candidato. Después va la revisión independiente de la evidencia (Q-PBT5-8).

## 8. Las capturas: dónde van y cómo se redactan antes de custodiarlas

**Dónde van:**
- **`<carpeta-del-Smoke-PB>\screens\raw\`:** las capturas tal como se toman, con el nombre `<hora UTC>-<paso>-<antes|despues|n>.png`. **Nunca se custodian.**
- **`<carpeta-del-Smoke-PB>\screens\crop\`:** las versiones para custodiar, hechas desde una copia de la cruda. Al custodiar, van a `F4B-SMOKE-PB/run/screens/`.

**Cómo se redactan:**
1. **Recortar** al área del paso: el panel, la ventana de RackCad, la línea de órdenes, la lista de Deshacer o la vista del rack.
2. **Tapar** con un rectángulo opaco (no un desenfoque) lo que quede dentro y no se pueda publicar. Eso incluye **todo el historial visible de la línea de órdenes** que lleve una ruta: el eco del `NETLOAD`, de `vla-open` y de los `defun` de `RCDUMP` y `rcdynchk`, que llevan `<raiz-de-trabajo-con-/>/…`.
3. **Nunca se custodian:** la paleta de referencias externas, el aviso de seguridad de NETLOAD, los diálogos de archivos y cualquier captura con otra aplicación. Para esos momentos, el registro guarda el texto del botón pulsado y el resultado.
4. **La lista de antes de custodiar,** abriendo cada captura:
   - sin letra de unidad ni ruta;
   - sin nombre de usuario;
   - sin cliente, proyecto ni lámina;
   - sin barra de tareas ni otras ventanas.

   Los nombres `cNN.dwg` y de los racks sí pueden salir. Una captura que no pase la lista no se custodia.
5. **`derivation-record.txt`:** por cada captura custodiada, la cruda (su nombre y su SHA-256, sin ruta), el rectángulo, las zonas tapadas con su motivo, el paso y el SHA-256 del resultado.
6. **Los A/B y los contextos construidos** llevan `CONSTRUCTED` en el nombre del archivo.
7. **En los S+O:** si la captura no puede bastar sin dejar ver algo prohibido, el paso queda PENDIENTE DE REVISIÓN DEL OWNER en directo, y la imagen no se custodia.

## 9. Cómo se carga el plugin, en resumen

| Regla | Fuente |
|---|---|
| El DLL Debug del worktree, compilado desde el Candidato, con AutoCAD cerrado (bloque del §4.2) | [G] §1 y §2; WORKFLOW §6 |
| Nunca se cierra ni se toca un AutoCAD que no abrió la sesión. Si hay uno abierto al compilar, no se compila. Sin COM y sin `Stop-Process`. El PID se comprueba por su ruta y su hora de inicio | PBT5-C08; I-07; M-27 |
| La `ProductVersion` y el SHA-256, antes de NETLOAD | [G] §7.2 |
| NETLOAD del DLL exacto, por expresión, y «Load Once» | [G] §3; [F] §6.3; [BM] |
| Sin tocar Trusted Locations, SECURELOAD, «Always Load», `FILEDIA`, `PICKFIRST` ni ninguna otra variable; sin «Always ignore» | [F] §6.3; [S2] |
| `RACKCAD` y la biblioteca; `RACKPANEL` | [G] §3, pasos 4 y 5 |
| Al final, nada de RackCad en `Applications`, `Loaded` ni `AssemblyMap` | SPB-52 |
