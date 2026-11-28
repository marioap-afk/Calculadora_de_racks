# I-64 F4B PB-T5 — Guía operativa del Smoke-PB contratado (smoke-run v2)

> **Redacción para el repo público:** las rutas locales van como marcadores, salvo la carpeta de trabajo, `<raiz-de-trabajo>\`, que fijó la sesión principal. Al custodiar, `<carpeta-del-Smoke-PB>` pasa a `<carpeta-del-Smoke-PB>` y `<carpeta-de-respaldo>` a `<carpeta-de-respaldo>`, como las copias exportadas `r71…` pasaron a `<copia>` (F4B-E60). Ningún nombre personal ni de cliente. La hora del Owner, en UTC.

**Qué es:** la guía con las órdenes exactas del Smoke-PB **contratado** de PB-T5, el intento 1 de 3, después de la emisión (F4B-E62).
- Los pasos, sus orígenes y sus criterios están en `I-64-f4b-pbt5-package-v2.4.md` ([P]).
- Las decisiones del Owner, en `owner-pbt5-decisions.md` ([OD5]).
- La comprobación acotada de la v1 de esta guía y de la v2.3 del paquete, en `review-v23/bounded-check-pbt5-v23.md` ([CA5]).

**De dónde sale:**
- de `smoke-run-v1.md`, con las correcciones de [CA5] (abajo, «Cambios v1 → v2»);
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
| `<worktree>`, `<checkout-principal>`, `<dotnet-del-usuario>`, `<python-del-usuario>`, `<acad.exe>` (la ruta larga y completa), `<dir-del-acceso-directo>`, `<original-D1>`, `<carpeta-de-registros-de-RackCad>` | Los pone la sesión en cada bloque. Cada bloque para si alguno se queda sin rellenar |
| `<lista-local-de-la-seleccion>` | La ruta de `s2-selection.local.json`, el archivo de la sesión con la selección de Smoke-2 (I-01, M-10). **Vive fuera de la carpeta del paquete** (la que barre la emisión) y **nunca se custodia:** lleva rutas y nombres de clientes. Es un JSON con `D1` (`Source`, `Sha256`, `Exists`) y `Selection`: siete entradas con `Source`, `Sha256`, `Roles` y `Exists`. Los bloques 10, 13 y 13b la leen por su ruta y comprueban antes su SHA-256, `DD3613CB94CD4E19712D0FE2687CBDBDED2500C2AA8C4AD450D8556A26D3F2C1`. Nada de su contenido pasa a un archivo que se custodie: los manifiestos y el registro nombran cada dibujo por su `eNN` y sus papeles de Smoke-2 |
| `<Candidato>` | `ebd304a3773495fa9071de6611936c4c01a7b06c`, la punta del cierre de PB-T4e (F4B-E61, §291) |
| `<producto>` | `0eac7911a025a59628b446db8b768035175f96de`, el GREEN de PB-T4e; `src/` y `tests/` del Candidato son iguales a los suyos |
| `<run>` | `45147867500`, la CI de push del Candidato: 4/4 en `success`, con los totales del cierre de PB-T4e ([CA5]) |
| Los 19 filtros de UI | Literales en el bloque B2 |

**La regla de los bloques de PowerShell** (I-08, M-33 y la medida de la v2.3):
- cada bloque es una orden entera, que funciona sola en un proceso nuevo;
- empieza con `Set-StrictMode -Version Latest` y `$ErrorActionPreference = 'Stop'`;
- para si un valor está vacío o sin rellenar, o si una ruta no existe;
- comprueba `$LASTEXITCODE` tras cada orden externa;
- nunca escribe en la raíz de una unidad;
- y termina con `exit 0` después de su última línea.
- Los bloques largos (B1 y B2) corren en segundo plano.
- **Sus mensajes de STOP no nombran la ruta de un original** (M-09): dicen de qué dibujo se trata (`D1`, `eNN`).

**Cambios v1 → v2** (los hallazgos de [CA5]; I-03 e I-04, por el lado estrecho):

| Hallazgo | Dónde |
|---|---|
| **I-01** | El bloque 13 lee la lista local en JSON, por su ruta y con su SHA-256 comprobado, y compara cada original con su SHA-256 de Smoke-2 sin parar. El bloque 13b, nuevo, hace `s01` y `s02`. El bloque 8 vale para cualquier paso entre `cNN`, `eNN` y `sNN`. El bloque 10 mide al final `estilo.json`, `s-copias.json`, los respaldos `eNN-respaldo.dwg` y los originales de la lista. Los bloques 5 y 9 aceptan las tres clases de copia |
| **I-02** | El contrato es el §6 de [P], con las 17 paradas enteras. El bloque 10 ya aplicaba la regla de HEAD de después de la emisión, que ahora está en PBT5-C01 y en SPB-51 |
| **I-03** | §2, §6 y §7: la sesión opera sola solo si la emisión y P-01 a P-08 están hechas antes de las 12:00 UTC |
| **I-04** | §1 y §6: solo la adenda que pida el Owner no cuenta. Una discrepancia de la revisión de la evidencia solo deja corregir transcripciones; si cambia un resultado o exige repetir un paso en AutoCAD, al Owner |
| M-04 | Bloque 14: `hash-object --no-filters`, y `dimkeys.py` desde el Candidato. Medido: los bytes y el id salen iguales |
| M-05 | c01 lleva «P-09 (D1)», no «SPB-07 (a)» (§4.3 y §5) |
| M-06 | §4.11: «Comprobar ahora» en SPB-60 y SPB-61, sin «Actualizar»; en SPB-61, «Descartar y recargar» → «Sí», como el paso 84 de [S2] |
| M-07 | Bloque 15, nuevo: las líneas `ERROR` del registro de RackCad, los eventos de error de acad y el SHA-256 de `settings.json` frente a P-05 (bloque 4) |
| M-08 | Bloque B4, nuevo: la biblioteca de bloques, entero, con su comprobación y `exit 0` |
| M-09 | §7: el registro no copia órdenes con rutas, y antes de custodiar pasa `_tools/scan_custody.py` con los patrones locales |
| M-10 | La lista local, fuera de la carpeta del paquete (arriba) |
| M-11 | §4.12: la regla de las respuestas nativas, el estado esperado de cada contexto y la aclaración de «variable de sistema» |
| M-14 | §4.10: el mismo orden que [P] §6.7 (`--dry`, el build, `--commit`) |

---

## 1. Qué es, y qué no es

- **Es el Smoke-PB de PB-T5,** emitido por la autorización condicional del Owner (Q-PBT5-7), con el contrato de la forma B (Q-PBT5-8).
  - Un intento es esta corrida sobre este Candidato (Q-PBT5-9).
  - No cuentan las repeticiones por error del operador (dos como máximo), una parada del entorno antes de abrir AutoCAD ni una adenda que pida el Owner.
  - **Solo la adenda que pida el Owner no cuenta** (I-04). Si la revisión de la evidencia encuentra una discrepancia, la sesión solo corrige transcripciones (el registro frente a la captura o al inventario) y vuelve a la revisión. Si la discrepancia cambia un resultado o exige repetir un paso en AutoCAD, vuelve al Owner, que puede pedir una adenda (PBT5-C16). Un paso repetido por otra causa cuenta según la definición de intento.
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

**La lectura estrecha** (I-03 de [CA5]; vale en las dos interpretaciones de «se complete» y no necesita una decisión nueva; Q-PBT5-5 no se preguntó):
- **La sesión opera sola solo si, antes de las 12:00 UTC del 2026-11-23** (las 6 am del Owner), **están hechas la emisión de PB-T5 y P-01 a P-08.** La hora a la que quedan hechas va al registro.
- **Si no lo están a las 12:00 UTC,** la sesión termina lo que no necesita AutoCAD (P-01 a P-08 y la emisión, si se cumplen sus condiciones) y espera al Owner. No abre AutoCAD sola.
- **La franja de 12:00 a 13:00 UTC** solo la usa una corrida que ya empezó, lista antes de las 12:00 UTC. Empezar en ella sin el Owner sería la lectura amplia, que exige preguntar Q-PBT5-5.
- **A las 13:00 UTC** (las 7 am del Owner), se para en el siguiente límite de paso y se espera al Owner:
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
| P-05 | Variables, bundles, el perfil, `PICKFIRST`, la carpeta de diseños, el registro de RackCad, el archivo de disposición, el SHA-256 de `settings.json` y keep-awake (§4.1, bloques 2 a 4); la biblioteca de bloques, después de B3 (§4.2, bloque B4) | Ninguna `RACKCAD_DEBUG_*`; ningún bundle; nada de RackCad en `Applications`, `Loaded` ni `AssemblyMap`. Las `Dialogs\RackCad.*`, solo contadas. `PICKFIRST` = 1; si vale 0, los pasos con selección por expresión no se hacen, y la variable no se cambia |
| P-06 | `<raiz-de-trabajo>\` nueva y fuera de todo repositorio; el SHA-256 de D1 frente al registrado; el respaldo; las 44 copias (§4.3, un solo bloque) | El bloque termina en «OK». Cualquier STOP para la corrida |
| P-07 | Ningún Worker ni otra sesión en el worktree; PB-T4e cerrada | Sin delegaciones abiertas |
| P-08 | La guía del operador: los pasos de §5, con los literales copiados del `src/` del Candidato por objetos git, los A/B rotulados y las reglas de las capturas del §8 | Hecha antes de abrir AutoCAD, y su sha256 en el registro |
| P-09 | El recuento del estilo de cotas en los demás originales de la selección de Smoke-2 (Q-PBT5-2, «Contar antes en otros»): el bloque 13, que lee la lista local con su SHA-256 comprobado; sus copias `eNN`, abiertas solo para `RCDUMP estilo` y cerradas con el bloque 8; y el bloque 14 (§4.13). P-09 (D1) es el `RCDUMP pre` de c01. Va antes de SPB-07 | La lista, con su SHA-256; respaldos y copias iguales a su original; los originales, iguales antes y después (y otra vez en el bloque 10); el recuento por dibujo, sin nombres. Si un original no coincide con su SHA-256 de Smoke-2, se registra y se sigue |

**Si no se cumple una de las tres condiciones de la emisión (PBT5-C17),** no se emite y no se opera.

**La frontera (§2):** la sesión abre AutoCAD sola solo si la emisión y P-01 a P-08 están hechas antes de las 12:00 UTC.

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
if (Test-Path -LiteralPath $s) { "settings.json " + (Get-FileHash -LiteralPath $s -Algorithm SHA256).Hash } else { "settings.json NO-EXISTE" }
if (Test-Path -LiteralPath $logs) { Get-ChildItem -LiteralPath $logs -Filter 'rackcad-*.log' | Sort-Object LastWriteTimeUtc | Select-Object -Last 3 Name, Length, LastWriteTimeUtc } else { "Carpeta de registros: no existe" }
exit 0
```

El valor de `settings.json` (un SHA-256 o `NO-EXISTE`) va al registro: el bloque 15 lo compara al final (SPB-52).

**La biblioteca de bloques** va en el bloque B4, después de B3 (§4.2), porque necesita `catalogs` junto al DLL.

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

**Bloque B4. La biblioteca de bloques (P-05; [G] §7.1; M-08),** en primer plano, cuando B3 haya terminado. Resuelve la ruta como el producto: el override `BlockLibraryPath` de `settings.json` si está definido; si no, `blocks-library.dwg` junto a los catálogos. Solo lee:

```powershell
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$wt = '<worktree>'
if ([string]::IsNullOrWhiteSpace($wt) -or $wt -match '[<>]') { throw "STOP: un valor sin rellenar" }
if (-not (Test-Path -LiteralPath $wt -PathType Container)) { throw "STOP: no existe el worktree" }
$catalogs = Join-Path $wt 'src\RackCad.Plugin\bin\Debug\net8.0-windows\catalogs'
if (-not (Test-Path -LiteralPath $catalogs -PathType Container)) { throw "STOP: no existe catalogs junto al DLL (antes, B3)" }
$s = Join-Path $env:APPDATA 'RackCad\settings.json'
$over = $null
if (Test-Path -LiteralPath $s) {
  $cfg = Get-Content -LiteralPath $s -Raw | ConvertFrom-Json
  if ($cfg -and ($cfg.PSObject.Properties.Name -contains 'BlockLibraryPath')) { $over = [string]$cfg.BlockLibraryPath }
}
if ([string]::IsNullOrWhiteSpace($over)) { $lib = Join-Path $catalogs 'blocks-library.dwg'; $origen = 'blocks-library.dwg junto a los catalogos' } else { $lib = $over; $origen = 'override BlockLibraryPath de settings.json' }
if (-not (Test-Path -LiteralPath $lib -PathType Leaf)) { throw "STOP: no existe la biblioteca de bloques ($origen)" }
"Biblioteca de bloques: $origen; archivo $([IO.Path]::GetFileName($lib)); SHA-256 $((Get-FileHash -LiteralPath $lib -Algorithm SHA256).Hash)"
"Ruta resuelta (solo para el registro local, no se custodia): $lib"
exit 0
```

Al registro custodiado van el origen, el nombre del archivo y el SHA-256; la ruta resuelta, solo al registro local.

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
| c01 | SPB-01 a 05 y 08; el `RCDUMP pre`, que sirve a SPB-47, 48 y 56, y es el recuento P-09 (D1); los controles de `rcblk`, `rcnest` y `rcdyn` |
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
| e01 a eNN (en `<carpeta-del-Smoke-PB>\estilo\`) | P-09: el `RCDUMP estilo` de cada una (bloque 13; §4.13) |
| s01 y s02 (en `<carpeta-del-Smoke-PB>\spb07\`) | SPB-07 (a) y (b), solo si algún dibujo guarda un estilo (bloque 13b; §4.13) |

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

**Bloque 5. El lanzamiento.** Con `$n = 1` y `c01.dwg`; al relanzar por §4.8, con el número siguiente y la copia que toque, que puede ser una `cNN` (en `r1\`), una `eNN` (en `estilo\`) o una `sNN` (en `spb07\`). El registro se escribe justo al lanzar (M-32), con la hora en `Ticks` (I-09):

```powershell
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$work = '<carpeta-del-Smoke-PB>'; $acad = '<acad.exe>'; $dir = '<dir-del-acceso-directo>'; $copyName = 'c01.dwg'; $n = 1
foreach ($v in $work, $acad, $dir, $copyName) { if ([string]::IsNullOrWhiteSpace($v) -or $v -match '[<>]') { throw "STOP: un valor sin rellenar" } }
if ($copyName -notmatch '^[ces]\d\d\.dwg$' -or $n -lt 1) { throw "STOP: la copia o el numero no son validos" }
if ($acad -match '~') { throw "STOP: la ruta de acad.exe es una ruta corta 8.3" }
if (-not [IO.Path]::IsPathFullyQualified($acad) -or [IO.Path]::GetFullPath($acad) -cne $acad) { throw "STOP: la ruta de acad.exe no es la ruta completa canonica" }
$sub = @{ c = 'r1'; e = 'estilo'; s = 'spb07' }[$copyName.Substring(0, 1)]
$copy = Join-Path $work "$sub\$copyName"
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

**Vale para las tres clases de copia** (I-01): las `cNN` en `<carpeta-del-Smoke-PB>\r1\`, las `eNN` del recuento en `<carpeta-del-Smoke-PB>\estilo\` y las `sNN` de SPB-07 en `<carpeta-del-Smoke-PB>\spb07\`. Abajo, `<sub>` es `r1`, `estilo` o `spb07`, según la clase de la siguiente. Los pasos de la corrida entre clases son c01 → e01, eNN → eNN+1, la última `eNN` → s01 (o → c02, si no hay estilo), s01 → s02 y s02 → c02.

**Con la copia `cNN` (o `eNN`, o `sNN`) activa y `cMM` (o `eMM`, o `sMM`) como la siguiente:**
1. **El panel:** «Descartar borradores de este documento» → «Descartar». Si no hay borradores en riesgo, se registra el mensaje.
2. **Tecleado:**

   ```lisp
   (vla-activate (vla-open (vla-get-documents (vlax-get-acad-object)) "<carpeta-del-Smoke-PB-con-/>/<sub>/cMM.dwg"))
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

7. **Bloque 8**, con la anterior y la siguiente rellenadas (`cNN`, `eNN` o `sNN`). Cada copia se compara con su original: una `cNN`, con D1; una `eNN`, con su fila de `estilo.json`; una `sNN`, con `s-copias.json`:

   ```powershell
   Set-StrictMode -Version Latest
   $ErrorActionPreference = 'Stop'
   $work = '<carpeta-del-Smoke-PB>'; $prev = '<anterior>'; $next = '<siguiente>'
   foreach ($v in $prev, $next) { if ($v -notmatch '^[ces]\d\d$') { throw "STOP: nombre de copia sin rellenar o no valido" } }
   if ($prev -eq $next) { throw "STOP: la anterior y la siguiente son la misma" }
   $known = '7A868624226EE956CC411CAAA54EA2DE6B95849EEE946995917CFF70FB9F47A5'
   $subs = @{ c = 'r1'; e = 'estilo'; s = 'spb07' }
   $mans = @{ e = 'estilo\estilo.json'; s = 'spb07\s-copias.json' }
   if (-not (Test-Path -LiteralPath $work -PathType Container)) { throw "STOP: no existe $work" }
   $info = @{}
   foreach ($nm in $prev, $next) {
     $k = $nm.Substring(0, 1)
     $dwg = Join-Path $work ($subs[$k] + '\' + $nm + '.dwg')
     if (-not (Test-Path -LiteralPath $dwg -PathType Leaf)) { throw "STOP: no existe $nm.dwg" }
     if ($k -eq 'c') { $want = $known } else {
       $mf = Join-Path $work $mans[$k]
       if (-not (Test-Path -LiteralPath $mf -PathType Leaf)) { throw "STOP: no existe el manifiesto de $nm" }
       $rows = @((Get-Content -LiteralPath $mf -Raw | ConvertFrom-Json).copias | Where-Object { $_.copia -eq "$nm.dwg" })
       if ($rows.Count -ne 1) { throw "STOP: $nm no aparece una vez en su manifiesto" }
       $want = [string]$rows[0].sha256
     }
     if ($want -notmatch '^[0-9A-F]{64}$') { throw "STOP: sin SHA-256 esperado para $nm" }
     $info[$nm] = [pscustomobject]@{ Dwg = $dwg; Dwl = [IO.Path]::ChangeExtension($dwg, '.dwl'); Want = $want }
   }
   $recFile = Get-ChildItem -LiteralPath $work -Filter 'acad-session-*.json' | Sort-Object { [int]($_.BaseName -replace '\D', '') } | Select-Object -Last 1
   if (-not $recFile) { throw "STOP: no hay registro de la instancia" }
   $rec = Get-Content -LiteralPath $recFile.FullName -Raw | ConvertFrom-Json
   $q = Get-Process -Id ([int]$rec.Id)
   if ($q.ProcessName -ne 'acad' -or $q.Path -ne $rec.Path -or [long]$rec.StartTicksUtc -ne $q.StartTime.ToUniversalTime().Ticks) { throw "STOP: el PID ya no es la instancia de la sesion" }
   $chk = [pscustomobject]@{
     dwlAnteriorExiste        = Test-Path -LiteralPath $info[$prev].Dwl
     anteriorIgualASuOriginal = (Get-FileHash -LiteralPath $info[$prev].Dwg -Algorithm SHA256).Hash -eq $info[$prev].Want
     dwlSiguienteExiste       = Test-Path -LiteralPath $info[$next].Dwl
     titulo                   = $q.MainWindowTitle
   }
   $chk
   $fail = @()
   if ($chk.dwlAnteriorExiste) { $fail += "la copia anterior $prev sigue abierta (.dwl)" }
   if (-not $chk.anteriorIgualASuOriginal) { $fail += "la copia anterior $prev ya no es igual a su original" }
   if (-not $chk.dwlSiguienteExiste) { $fail += "la copia siguiente $next no esta abierta (.dwl)" }
   if ($chk.titulo -notmatch [regex]::Escape("$next.dwg")) { $fail += "el titulo no nombra $next.dwg" }
   if ($fail.Count) { throw ("STOP: " + ($fail -join '; ')) }
   exit 0
   ```

   Lo esperado: `False`, `True`, `True`, y el título con el nombre de la siguiente.

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
  if ($t -match '[ces]\d\d\.dwg') { throw "STOP: sigue abierta una copia de la sesion ($t): se vuelve a 4.7" }
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

**Bloque 10. SPB-51, la integridad.** Solo lee, y se hace siempre, también en la parada de la frontera. Si una copia está abierta en AutoCAD y no se puede leer, lo registra como «no legible (abierta)» y sigue con las demás. Mide D1, su respaldo y las `cNN`; y, si existen, `estilo.json` (los originales de P-09, leídos de la lista local con su SHA-256 comprobado, sus respaldos `eNN-respaldo.dwg` y sus copias) y `s-copias.json` (`s01` y `s02`). No imprime ninguna ruta de original:

```powershell
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$root = '<raiz-de-trabajo>'; $o = '<original-D1>'; $wt = '<worktree>'; $cand = 'ebd304a3773495fa9071de6611936c4c01a7b06c'
$list = '<lista-local-de-la-seleccion>'; $listSha = 'DD3613CB94CD4E19712D0FE2687CBDBDED2500C2AA8C4AD450D8556A26D3F2C1'
foreach ($v in $o, $wt, $cand) { if ([string]::IsNullOrWhiteSpace($v) -or $v -match '[<>]') { throw "STOP: un valor sin rellenar" } }
if ($cand -notmatch '^[0-9a-f]{40}$') { throw "STOP: el Candidato no es un SHA completo" }
$work = Join-Path $root 'smoke'; $manifest = Join-Path $work 'copias.json'; $bakFile = Join-Path $root 'respaldo\D1-respaldo.dwg'
if (-not (Test-Path -LiteralPath $o -PathType Leaf)) { throw "STOP: no existe el original de D1" }
foreach ($p in $manifest, $bakFile) { if (-not (Test-Path -LiteralPath $p -PathType Leaf)) { throw "STOP: no existe $p" } }
$man = Get-Content -LiteralPath $manifest -Raw | ConvertFrom-Json
$h0 = [string]$man.original_sha256
if ($h0 -notmatch '^[0-9A-F]{64}$') { throw "STOP: el manifiesto no trae el SHA-256 del original" }
$fail = @()
$origEq = (Get-FileHash -LiteralPath $o -Algorithm SHA256).Hash -eq $h0
"D1 original igual: $origEq"; if (-not $origEq) { $fail += "el original de D1 cambio (PBT5-C02)" }
$bakEq = (Get-FileHash -LiteralPath $bakFile -Algorithm SHA256).Hash -eq $h0
"D1 respaldo igual: $bakEq"; if (-not $bakEq) { $fail += "el respaldo de D1 cambio" }
foreach ($c in $man.copias) {
  $f = Join-Path $work ("r1\" + $c.copia); $dwl = [IO.Path]::ChangeExtension($f, '.dwl')
  if (-not (Test-Path -LiteralPath $f -PathType Leaf)) { $fail += "falta $($c.copia)"; continue }
  try { $eq = (Get-FileHash -LiteralPath $f -Algorithm SHA256).Hash -eq $h0 } catch { $eq = 'no legible (abierta)' }
  "{0} igual={1} dwl={2}" -f $c.copia, $eq, (Test-Path -LiteralPath $dwl)
  if ($eq -is [bool] -and -not $eq) { $fail += "$($c.copia) ya no es igual a D1" }
}
# P-09: los originales de la lista local, sus respaldos y sus copias, frente a estilo.json
$estJson = Join-Path $work 'estilo\estilo.json'
if (Test-Path -LiteralPath $estJson -PathType Leaf) {
  if ([string]::IsNullOrWhiteSpace($list) -or $list -match '[<>]') { throw "STOP: existe estilo.json y falta la ruta de la lista local" }
  if (-not (Test-Path -LiteralPath $list -PathType Leaf)) { throw "STOP: no existe la lista local" }
  if ((Get-FileHash -LiteralPath $list -Algorithm SHA256).Hash -ne $listSha) { throw "STOP: la lista local no es la esperada (su SHA-256 no coincide)" }
  $sel = @((Get-Content -LiteralPath $list -Raw | ConvertFrom-Json).Selection)
  $est = Get-Content -LiteralPath $estJson -Raw | ConvertFrom-Json
  if ([string]$est.lista_sha256 -ne $listSha) { $fail += "estilo.json no se hizo con esta lista" }
  foreach ($r in @($est.copias)) {
    $tag = [IO.Path]::GetFileNameWithoutExtension([string]$r.copia); $want = [string]$r.sha256; $ix = [int]$r.indice
    if ($ix -lt 0 -or $ix -ge $sel.Count) { $fail += "$tag sin entrada en la lista"; continue }
    $src = [string]$sel[$ix].Source
    $oe = if (Test-Path -LiteralPath $src -PathType Leaf) { (Get-FileHash -LiteralPath $src -Algorithm SHA256).Hash -eq $want } else { 'no existe' }
    $bf = Join-Path $root "respaldo\$tag-respaldo.dwg"
    $be = if (Test-Path -LiteralPath $bf -PathType Leaf) { (Get-FileHash -LiteralPath $bf -Algorithm SHA256).Hash -eq $want } else { 'no existe' }
    $cf = Join-Path $work "estilo\$tag.dwg"
    if (Test-Path -LiteralPath $cf -PathType Leaf) { try { $ce = (Get-FileHash -LiteralPath $cf -Algorithm SHA256).Hash -eq $want } catch { $ce = 'no legible (abierta)' } } else { $ce = 'no existe' }
    "{0} ({1}) original igual={2} respaldo igual={3} copia igual={4} dwl={5}" -f $tag, $r.papeles, $oe, $be, $ce, (Test-Path -LiteralPath ([IO.Path]::ChangeExtension($cf, '.dwl')))
    if ($oe -isnot [bool] -or -not $oe) { $fail += "el original de $tag cambio o no existe (PBT5-C02)" }
    if ($be -isnot [bool] -or -not $be) { $fail += "el respaldo de $tag cambio o no existe" }
    if ($ce -is [bool] -and -not $ce) { $fail += "$tag ya no es igual a su original" }
    if ($ce -is [string] -and $ce -eq 'no existe') { $fail += "falta $tag.dwg" }
  }
} else { "estilo.json: no existe (P-09 no llego a hacerse)" }
# SPB-07: s01 y s02, frente a s-copias.json
$sJson = Join-Path $work 'spb07\s-copias.json'
if (Test-Path -LiteralPath $sJson -PathType Leaf) {
  foreach ($r in @((Get-Content -LiteralPath $sJson -Raw | ConvertFrom-Json).copias)) {
    $sf = Join-Path $work ("spb07\" + [string]$r.copia)
    if (-not (Test-Path -LiteralPath $sf -PathType Leaf)) { $fail += "falta $($r.copia)"; continue }
    try { $se = (Get-FileHash -LiteralPath $sf -Algorithm SHA256).Hash -eq [string]$r.sha256 } catch { $se = 'no legible (abierta)' }
    "{0} (de {1}) igual={2}" -f $r.copia, $r.origen, $se
    if ($se -is [bool] -and -not $se) { $fail += "$($r.copia) ya no es igual a su original" }
  }
} else { "s-copias.json: no existe (ningun dibujo con estilo, o no se llego)" }
foreach ($sub in 'r1', 'estilo', 'spb07') {
  $d = Join-Path $work $sub
  if (Test-Path -LiteralPath $d -PathType Container) { Get-ChildItem -LiteralPath $d -File | Where-Object { $_.Extension -notin '.dwg', '.dwl', '.dwl2', '.json' } | Select-Object Name, Length }
}
Get-ChildItem -LiteralPath $env:TEMP -Filter '*.sv$' -ErrorAction SilentlyContinue | Where-Object { $_.Name -match '^[ces]\d\d.*\.sv\$$' } | Select-Object Name, LastWriteTimeUtc
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

**SPB-52, el entorno:** los bloques 2 a 4 del §4.1, otra vez, cada uno en su orden, y después el bloque 15 (M-07). Solo leen, y se hacen siempre.

**Bloque 15. Las líneas `ERROR` del registro de RackCad, los eventos de error de acad y `settings.json` (SPB-52).** El inicio de la corrida es la hora del primer `acad-session-<n>.json`; si no hay ninguno, la de creación de `<raiz-de-trabajo>`. `$settingsPre` es el valor que dio el bloque 4 en P-05 (un SHA-256 o `NO-EXISTE`):

```powershell
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$root = '<raiz-de-trabajo>'; $logs = '<carpeta-de-registros-de-RackCad>'; $settingsPre = '<settings-de-P-05>'
foreach ($v in $logs, $settingsPre) { if ([string]::IsNullOrWhiteSpace($v) -or $v -match '[<>]') { throw "STOP: un valor sin rellenar" } }
if ($settingsPre -ne 'NO-EXISTE' -and $settingsPre -notmatch '^[0-9A-F]{64}$') { throw "STOP: el valor de settings.json de P-05 no es un SHA-256 ni NO-EXISTE" }
if (-not (Test-Path -LiteralPath $root -PathType Container)) { throw "STOP: no existe $root" }
$work = Join-Path $root 'smoke'
$ticks = @(Get-ChildItem -LiteralPath $work -Filter 'acad-session-*.json' -ErrorAction SilentlyContinue | ForEach-Object { [long](Get-Content -LiteralPath $_.FullName -Raw | ConvertFrom-Json).StartTicksUtc })
$sinceUtc = if ($ticks.Count) { [DateTime]::new([long]($ticks | Sort-Object | Select-Object -First 1), [DateTimeKind]::Utc) } else { (Get-Item -LiteralPath $root).CreationTimeUtc }
"Desde (UTC): $($sinceUtc.ToString('o'))"
# Las lineas ERROR de los registros de RackCad escritos desde el inicio
if (Test-Path -LiteralPath $logs -PathType Container) {
  $files = @(Get-ChildItem -LiteralPath $logs -Filter 'rackcad-*.log' | Where-Object { $_.LastWriteTimeUtc -ge $sinceUtc })
  "Registros de RackCad desde el inicio: $($files.Count)"
  foreach ($f in $files) {
    $err = @(Select-String -LiteralPath $f.FullName -Pattern 'ERROR' -SimpleMatch)
    "{0}: {1} lineas ERROR" -f $f.Name, $err.Count
    $err | ForEach-Object { "  linea {0}: {1}" -f $_.LineNumber, $_.Line }
  }
} else { "Carpeta de registros: no existe" }
# Los eventos de error de acad desde el inicio
try {
  $ev = @(Get-WinEvent -FilterHashtable @{ LogName = 'Application'; ProviderName = 'Application Error', '.NET Runtime', 'Application Hang'; StartTime = $sinceUtc.ToLocalTime() } -ErrorAction Stop)
} catch {
  if ($_.FullyQualifiedErrorId -match 'NoMatchingEventsFound') { $ev = @() } else { throw }
}
$acadEv = @($ev | Where-Object { "$($_.Message)" -match 'acad' })
"Eventos Application Error, .NET Runtime o Application Hang desde el inicio: $($ev.Count); de acad: $($acadEv.Count)"
$acadEv | ForEach-Object { "{0} {1} {2}" -f $_.TimeCreated.ToUniversalTime().ToString('o'), $_.ProviderName, $_.Id }
# settings.json, frente a P-05
$s = Join-Path $env:APPDATA 'RackCad\settings.json'
$settingsPost = if (Test-Path -LiteralPath $s) { (Get-FileHash -LiteralPath $s -Algorithm SHA256).Hash } else { 'NO-EXISTE' }
"settings.json antes $settingsPre despues $settingsPost igual=$($settingsPre -eq $settingsPost)"
exit 0
```

- **Las líneas `ERROR`** van enteras al registro local. Al registro custodiado van su número y un resumen redactado (§7).
- **El criterio de SPB-52** está en [P]: PASS si no queda configuración de RackCad ni errores sin explicar. Un `settings.json` distinto se registra y se explica; no se toca.

### 4.10 El orden de la corrida

Es el mismo orden que [P] §6.7 (M-14): el `--dry` antes del build, y el `--commit` después.

1. **La preparación, sin AutoCAD:**
   - P-07 (ningún Worker ni otra sesión en el worktree) y el bloque 1 (P-01), con HEAD = Candidato;
   - `emit_pbt5.py --dry`, que solo lee y comprueba las condiciones de la emisión;
   - B1 y B2, en segundo plano, y después B3 y B4 (P-02 a P-04, y la biblioteca de P-05);
   - los bloques 2, 3 y 4 (P-05) y el 4b;
   - `emit_pbt5.py --commit` (la emisión, F4B-E62), que solo añade un commit documental: el DLL de B3 no se vuelve a compilar;
   - el bloque de §4.3 (P-06);
   - el bloque 13 (P-09, las copias para el estilo);
   - P-08.
   - **La hora a la que quedan hechas la emisión y P-01 a P-08** va al registro, frente a las 12:00 UTC (§2). Si es después, no se abre AutoCAD sin el Owner. El bloque 13 (P-09) no cuenta para esa hora.
2. **La instancia principal de AutoCAD,** con el bloque 5 y `c01`:
   - c01, con SPB-01 a 05 y 08, el `RCDUMP pre` (P-09 (D1)) y los controles;
   - **las copias `eNN` del estilo,** abiertas una a una con el paso de §4.7 y el bloque 8, solo para el `RCDUMP estilo`;
   - el bloque 14, el recuento;
   - si algún dibujo guarda un estilo, el bloque 13b, y `s01` y `s02` para SPB-07 (a) y (b), con el paso de §4.7 (§4.13);
   - c02 a c41, por la tabla de §4.3. Los contextos construidos son c34 a c41 (§4.12);
   - el cierre de la instancia (SPB-50 y el bloque 9).
3. **Las tres inyecciones,** cada una en su AutoCAD propio (§4.11): c42, c43 y c44.
4. **El cierre:** el bloque 10 (SPB-51), y los bloques 2, 3, 4 y 15 (SPB-52).

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
6. **el paso:** seleccionar el rack → «Editar» → «Editar todo» → un frente menos → «Aplicar al borrador» → «Actualizar» → `RCDUMP post`. Se registran el texto literal, el estado del borrador, los botones que se ofrecen y la lista de Deshacer;
   - **SPB-59** (`UNIT=2`): nada más. «Actualizar» no se vuelve a pulsar;
   - **SPB-60** (`SEAL=before`, como el paso 82 de [S2]; M-06): «Comprobar ahora» → el texto literal, el estado y lo que se ofrece → `RCDUMP check`. **«Actualizar» no se pulsa,** aunque se ofrezca;
   - **SPB-61** (`SEAL=after`, como el paso 84 de [S2]; M-06): «Comprobar ahora» → el texto literal, el estado y lo que se ofrece (en [S2]: «Comprobar ahora», «Descartar y recargar» y «Conservar mi borrador») → `RCDUMP check`. **Sin «Actualizar».** Después, «Descartar y recargar» → el texto de la pregunta → «Sí» → el texto literal y el estado;
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

**La regla de las respuestas nativas** (M-11; [P] §5.6). Las órdenes de abajo suponen unas respuestas de AutoCAD que no se han visto en estas copias: que `-INSERT` pida el punto, la escala X, la Y y el giro, sin atributos; que `-LAYOUT _S` acepte el nombre de la primera presentación; que `-PURGE` acepte el patrón y no pregunte por cada nombre con `_N`.
- **Antes de teclear la orden siguiente,** se lee la línea de órdenes.
- **Si una orden pide algo distinto de lo previsto, da un error o un aviso, o si `RCDUMP CONSTRUCTED-construido` no muestra el estado esperado** (abajo, en cada contexto): **STOP del paso y LIMITATION** (NO VERIFICABLE FÍSICAMENTE), con el literal en el registro. Se cancela con `Esc` lo que quede a medias, la copia se cierra sin guardar y su gemela no se construye.
- **No se improvisan órdenes** ni respuestas: ni otra forma de `-INSERT`, ni otra orden, ni otro patrón.
- **Estas órdenes cambian el estado del dibujo de la copia** (`CTAB` y `TILEMODE`, una capa bloqueada, un bloque nuevo). Eso no es una variable de sistema de la configuración (§9): la copia se cierra sin guardar.

**SPB-45, la referencia en una presentación** (c34, el panel; c35, la gemela):

```lisp
(command "_.-LAYOUT" "_S" (car (layoutlist)))
(command "_.PSPACE")
(command "_.-INSERT" "<nombre-de-la-definicion-del-lateral>" "0,0" "1" "1" "0")
(command "_.MODEL")
```

Después, `RCDUMP CONSTRUCTED-construido`. Las líneas `REF` cuentan las referencias de modelo y de papel, porque `ssget "_X"` las lee todas. **El estado esperado:** la definición del lateral tiene una línea `REF` más que en `CONSTRUCTED-ins`, con un handle nuevo, y `(getvar "TILEMODE")` vuelve a dar `1`.

**SPB-46, la referencia dentro de un bloque del usuario** (c36 y c37):

```lisp
(command "_.-BLOCK" "ZZ-CONSTRUCTED-SPB46" "0,0" (handent "<h-de-la-referencia-del-lateral>") "")
(command "_.-INSERT" "ZZ-CONSTRUCTED-SPB46" "0,0" "1" "1" "0")
```

Después, `RCDUMP CONSTRUCTED-construido`. **El estado esperado:** una línea `BLK` de `ZZ-CONSTRUCTED-SPB46`; una línea `NEST` con `ZZ-CONSTRUCTED-SPB46` como contenedor y la definición del lateral; y una línea `REF` menos de esa definición en el nivel superior, porque `-BLOCK` retira la referencia que mete en el bloque.

**SPB-57, la capa bloqueada** (c38 y c39):

```lisp
(command "_.-LAYER" "_LO" "<capa-de-la-referencia-del-lateral>" "")
```

La capa sale de la línea `REF` del lateral en `RCDUMP CONSTRUCTED-ins`. Después, `RCDUMP CONSTRUCTED-construido` y, de solo lectura, el bloqueo de la capa:

```lisp
(logand 4 (cdr (assoc 70 (tblsearch "LAYER" "<capa-de-la-referencia-del-lateral>"))))
```

**El estado esperado:** `4` (la capa bloqueada), y las líneas `REF` iguales que en `CONSTRUCTED-ins`.

**SPB-58, el último vínculo** (c40 y c41), sobre un rack sin referencias anidadas (el que [BM] llamó PB E):

```lisp
(command "_.ERASE" (handent "<h-de-otra-referencia-del-rack>") "")
(command "_.-PURGE" "_B" "<patron-de-esa-definicion>" "_N")
```

- **El patrón** es el nombre de la definición con cada espacio cambiado por `?`, como hizo [BM] M-2b («PB?G?-?lateral?1»).
- Se repiten las dos órdenes por cada referencia y cada definición del rack que no sea el lateral.
- Después, `RCDUMP CONSTRUCTED-construido`. **El estado esperado:** el rack solo tiene la línea `REF` y la línea `DEF` del lateral; y, frente a `CONSTRUCTED-ins`, las únicas líneas `BLK` que faltan son las de las definiciones purgadas. Si falta otra, el patrón casó de más: es el STOP de la regla de arriba.
- La selección del paso es el propio lateral.

**El paso, en las cuatro:**
- **en el panel:** seleccionar el rack → «Editar» → «Editar todo» → un frente menos (el del lateral) → «Aplicar al borrador» → «Actualizar» → `RCDUMP CONSTRUCTED-post`, y el texto literal;
- **en la gemela:** RACKEDITAR → «- frente» → «Actualizar» → `RCDUMP CONSTRUCTED-post`, y el mensaje de la línea de órdenes;
- **la comparación** de los dos `post`, con las herramientas de [BM], va al registro.

### 4.13 El recuento del estilo de cotas (P-09 y SPB-07; Q-PBT5-2, «Contar antes en otros»)

**Bloque 13. Respaldos y copias de los demás originales de la selección de Smoke-2** (I-01). Va después del bloque de §4.3.
- **Lee la lista local por su ruta,** después de comprobar su SHA-256. No copia su contenido a nada que se custodie: `estilo.json` lleva, por cada `eNN`, su posición en la lista, sus papeles de Smoke-2 y su SHA-256, pero no su ruta ni su nombre.
- **Salta D1 y D2:** D1 se cuenta en c01 (P-09 (D1)), y D2 ya está contado en [BM] M-7.
- **Compara cada original con su SHA-256 de Smoke-2** (`F4-SMOKE-2/run/integrity.md`, O1 a O7) y con el de la lista, y lo registra **sin parar** (P-09).
- **Comprueba todo antes de escribir nada:** que existan todos los originales y ningún destino.

```powershell
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$root = '<raiz-de-trabajo>'; $list = '<lista-local-de-la-seleccion>'
$listSha = 'DD3613CB94CD4E19712D0FE2687CBDBDED2500C2AA8C4AD450D8556A26D3F2C1'
# Los SHA-256 de los siete originales de la seleccion, en F4-SMOKE-2/run/integrity.md (O1 a O7)
$s2 = @{
  'C496D070F5E2331D560BC3BE07239F3CADFCA4E056A69CDED2AF9E936BD9174C' = 'O1'; '895DE2EDA1C00D7660EB572C9E62A9CCA3BF934B61A0093738B5C4939CE182CC' = 'O2'
  'F865183291263C6AC5578682566743E543E59410C875FB76110833E190B4AF10' = 'O3'; 'A411969066F85236CDAEBAB678470090A8EC3034A75D20A1DD86EE63AF25B8C3' = 'O4'
  '9F65934E7BD8014CBC477C4BC2FA9822561B2E4B988D14CEF5650DFB2081D4BE' = 'O5'; '31AEB8010076040C7971164505312F76FC02ABC5D2B848CAA20972409C238FB4' = 'O6'
  '7A868624226EE956CC411CAAA54EA2DE6B95849EEE946995917CFF70FB9F47A5' = 'O7'
}
if ([string]::IsNullOrWhiteSpace($list) -or $list -match '[<>]') { throw "STOP: un valor sin rellenar" }
$work = Join-Path $root 'smoke'; $bak = Join-Path $root 'respaldo'; $est = Join-Path $work 'estilo'
foreach ($p in $work, $bak) { if (-not (Test-Path -LiteralPath $p -PathType Container)) { throw "STOP: no existe $p (primero el bloque de 4.3)" } }
if (-not (Test-Path -LiteralPath $list -PathType Leaf)) { throw "STOP: no existe la lista local" }
if ((Get-FileHash -LiteralPath $list -Algorithm SHA256).Hash -ne $listSha) { throw "STOP: la lista local no es la esperada (su SHA-256 no coincide)" }
if (Test-Path -LiteralPath $est) { throw "STOP: $est ya existe" }
$sel = Get-Content -LiteralPath $list -Raw | ConvertFrom-Json
if (-not ($sel.PSObject.Properties.Name -contains 'Selection')) { throw "STOP: la lista no trae Selection" }
$entries = @($sel.Selection)
if ($entries.Count -ne 7) { throw "STOP: la lista no trae siete entradas" }
# Primero, todas las comprobaciones; nada se escribe todavia
$todo = @()
for ($i = 0; $i -lt $entries.Count; $i++) {
  $e = $entries[$i]
  foreach ($k in 'Source', 'Sha256', 'Roles', 'Exists') { if (-not ($e.PSObject.Properties.Name -contains $k)) { throw "STOP: la entrada $i de la lista no trae $k" } }
  $roles = @($e.Roles | ForEach-Object { [string]$_ })
  if ($roles -contains 'D1' -or $roles -contains 'D2') { continue }
  $todo += [pscustomobject]@{ Indice = $i; Papeles = ($roles -join ' '); Src = [string]$e.Source; Lista = ([string]$e.Sha256).ToUpperInvariant() }
}
if ($todo.Count -eq 0 -or $todo.Count -gt 99) { throw "STOP: no queda ningun original que contar, o son demasiados" }
for ($j = 0; $j -lt $todo.Count; $j++) {
  $tag = 'e{0:d2}' -f ($j + 1)
  if (-not (Test-Path -LiteralPath $todo[$j].Src -PathType Leaf)) { throw "STOP: no existe el original de $tag ($($todo[$j].Papeles)): P-09 no se cumple; al Owner" }
  foreach ($d in (Join-Path $bak "$tag-respaldo.dwg"), (Join-Path $est "$tag.dwg")) { if (Test-Path -LiteralPath $d) { throw "STOP: ya existe $d" } }
}
# Despues, los respaldos y las copias, cada uno desde su original y con el hash comprobado
New-Item -ItemType Directory -Path $est | Out-Null
$rows = @()
for ($j = 0; $j -lt $todo.Count; $j++) {
  $t = $todo[$j]; $tag = 'e{0:d2}' -f ($j + 1)
  $h = (Get-FileHash -LiteralPath $t.Src -Algorithm SHA256).Hash
  $b = Join-Path $bak "$tag-respaldo.dwg"; $c = Join-Path $est "$tag.dwg"
  Copy-Item -LiteralPath $t.Src -Destination $b
  if ((Get-FileHash -LiteralPath $b -Algorithm SHA256).Hash -ne $h) { throw "STOP: el respaldo de $tag no es igual a su original" }
  Copy-Item -LiteralPath $t.Src -Destination $c
  if ((Get-FileHash -LiteralPath $c -Algorithm SHA256).Hash -ne $h) { throw "STOP: la copia de $tag no es igual a su original" }
  $o2 = if ($s2.ContainsKey($h)) { $s2[$h] } else { 'ninguno' }
  $rows += [pscustomobject]@{ copia = "$tag.dwg"; indice = $t.Indice; papeles = $t.Papeles; sha256 = $h; igual_a_la_lista = ($h -eq $t.Lista); original_de_smoke2 = $o2 }
  "{0} ({1}): igual al SHA-256 de la lista={2}; original de Smoke-2={3}" -f $tag, $t.Papeles, ($h -eq $t.Lista), $o2
}
for ($j = 0; $j -lt $todo.Count; $j++) {
  if ((Get-FileHash -LiteralPath $todo[$j].Src -Algorithm SHA256).Hash -ne $rows[$j].sha256) { throw "STOP PBT5-C02: cambio el original de $($rows[$j].copia)" }
}
ConvertTo-Json -InputObject ([pscustomobject]@{ lista_sha256 = $listSha; copias = @($rows) }) -Depth 4 | Out-File -LiteralPath (Join-Path $est 'estilo.json') -Encoding utf8 -NoClobber
"OK: $($rows.Count) respaldos y copias para el recuento del estilo; los que no coinciden con Smoke-2 se registran y se cuentan igual"
exit 0
```

**En la instancia principal,** después de c01: cada `eNN` se abre con el paso de §4.7, con `<sub>` = `estilo` (la ruta `<carpeta-del-Smoke-PB-con-/>/estilo/eNN.dwg`). En cada una:
1. `EXTERNALREFERENCESCLOSE`;
2. `(getvar "DWGNAME")` y el inventario;
3. `RCDUMP estilo`;
4. el cierre de la anterior, `vla-close … :vlax-false`;
5. el bloque 8, con la anterior y la siguiente: c01 → e01, e01 → e02, y así.

En las `eNN` no se hace nada más. La última `eNN` se cierra al pasar a `s01`, o a c02 si no hay estilo, otra vez con el bloque 8.

**Bloque 14. El recuento,** fuera de AutoCAD, con `dimkeys.py` de [BM], sacado byte a byte de los objetos git:

```powershell
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$work = '<carpeta-del-Smoke-PB>'; $main = '<checkout-principal>'; $py = '<python-del-usuario>'
foreach ($v in $main, $py) { if ([string]::IsNullOrWhiteSpace($v) -or $v -match '[<>]') { throw "STOP: un valor sin rellenar" } }
foreach ($p in $work, $main, $py) { if (-not (Test-Path -LiteralPath $p)) { throw "STOP: no existe $p" } }
$rev = 'ebd304a3773495fa9071de6611936c4c01a7b06c:docs/automation/evidence/I-64-pilot/F4B-PBT0-CHARACTERIZATION/block-m/tools/dimkeys.py'
$tools = Join-Path $work 'tools'; $dk = Join-Path $tools 'dimkeys.py'
if (-not (Test-Path -LiteralPath $tools)) { New-Item -ItemType Directory -Path $tools | Out-Null }
if (Test-Path -LiteralPath $dk) { throw "STOP: $dk ya existe" }
& git -C $main show $rev > $dk; if ($LASTEXITCODE -ne 0) { throw "STOP: git show" }
$want = & git -C $main rev-parse $rev; if ($LASTEXITCODE -ne 0) { throw "STOP: git rev-parse" }
$got = & git -C $main hash-object --no-filters $dk; if ($LASTEXITCODE -ne 0) { throw "STOP: git hash-object" }
if ($want -ne $got) { throw "STOP: dimkeys.py no es byte a byte el del repositorio" }
$inv = @(Get-ChildItem -LiteralPath (Join-Path $work 'inv') -File | Where-Object { $_.Name -eq 'c01-pre.txt' -or $_.Name -match '^e\d\d-estilo\.txt$' } | Sort-Object Name)
if ($inv.Count -lt 2) { throw "STOP: faltan inventarios (c01-pre y los eNN-estilo)" }
foreach ($f in $inv) {
  "== $($f.Name)"
  & $py -I $dk $f.FullName; if ($LASTEXITCODE -ne 0) { throw "STOP: dimkeys.py en $($f.Name)" }
}
"OK: recuento hecho; se lee en la salida: los sobres Push Back con DimensionStyle no nulo, por dibujo"
exit 0
```

`dimkeys.py` sale del Candidato; su blob es el mismo en `7da361eb` (`57decb54…`). **Medido** (M-04): en PowerShell 7.7, la redirección `>` deja los bytes de `git show` tal cual (0 CR), y `hash-object --no-filters` da el id de `rev-parse`.

**Después del recuento:**
- **Si algún sobre Push Back de una `eNN` guarda un `DimensionStyle` no nulo:** el bloque 13b, con la primera `eNN` que lo guarde, por orden. Hace dos copias nuevas de su original, `s01.dwg` y `s02.dwg`, en `<carpeta-del-Smoke-PB>\spb07\`, y su manifiesto `s-copias.json`.
  - **SPB-07 (a)** va en `s01`: RACKEDITAR → «Actualizar» → el estilo de las cotas.
  - **SPB-07 (b)** va en `s02`: «Editar todo» → «Actualizar».
  - Se abren con el paso de §4.7, con `<sub>` = `spb07`, y el bloque 8.
- **Si `c01-pre` (D1) diera un sobre con estilo:** no se espera, porque [BM] M-7 midió en D1 los 14 sobres con el estilo nulo. Sería un OBSERVADO: SPB-07 vuelve al Owner con esa evidencia, y no se hacen copias sin un bloque exacto.
- **Si ninguno:** SPB-07 queda **PENDIENTE DEL OWNER**, para que decida si acepta el criterio 3 cumplido en parte. No se fabrica nada.

**Bloque 13b. `s01` y `s02` para SPB-07,** desde el original de la `eNN` que guarda el estilo. Sin sobrescribir, desde el original, con el hash comprobado y frente a `estilo.json`:

```powershell
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$root = '<raiz-de-trabajo>'; $list = '<lista-local-de-la-seleccion>'; $tag = '<eNN-con-estilo>'
$listSha = 'DD3613CB94CD4E19712D0FE2687CBDBDED2500C2AA8C4AD450D8556A26D3F2C1'
if ([string]::IsNullOrWhiteSpace($list) -or $list -match '[<>]') { throw "STOP: un valor sin rellenar" }
if ($tag -notmatch '^e\d\d$') { throw "STOP: la eNN sin rellenar o no valida" }
$work = Join-Path $root 'smoke'; $estJson = Join-Path $work 'estilo\estilo.json'; $sp = Join-Path $work 'spb07'
if (-not (Test-Path -LiteralPath $estJson -PathType Leaf)) { throw "STOP: no existe estilo.json (primero el bloque 13)" }
if (Test-Path -LiteralPath $sp) { throw "STOP: $sp ya existe" }
if (-not (Test-Path -LiteralPath $list -PathType Leaf)) { throw "STOP: no existe la lista local" }
if ((Get-FileHash -LiteralPath $list -Algorithm SHA256).Hash -ne $listSha) { throw "STOP: la lista local no es la esperada (su SHA-256 no coincide)" }
$est = Get-Content -LiteralPath $estJson -Raw | ConvertFrom-Json
if ([string]$est.lista_sha256 -ne $listSha) { throw "STOP: estilo.json no se hizo con esta lista" }
$row = @(@($est.copias) | Where-Object { $_.copia -eq "$tag.dwg" })
if ($row.Count -ne 1) { throw "STOP: $tag no aparece una vez en estilo.json" }
$h = [string]$row[0].sha256
$sel = @((Get-Content -LiteralPath $list -Raw | ConvertFrom-Json).Selection)
$ix = [int]$row[0].indice
if ($ix -lt 0 -or $ix -ge $sel.Count) { throw "STOP: $tag no tiene entrada en la lista" }
$src = [string]$sel[$ix].Source
if (-not (Test-Path -LiteralPath $src -PathType Leaf)) { throw "STOP: no existe el original de $tag" }
if ((Get-FileHash -LiteralPath $src -Algorithm SHA256).Hash -ne $h) { throw "STOP PBT5-C02: el original de $tag cambio desde el bloque 13" }
New-Item -ItemType Directory -Path $sp | Out-Null
$rows = @(foreach ($s in 's01.dwg', 's02.dwg') {
  $dst = Join-Path $sp $s
  if (Test-Path -LiteralPath $dst) { throw "STOP: $s ya existe" }
  Copy-Item -LiteralPath $src -Destination $dst
  if ((Get-FileHash -LiteralPath $dst -Algorithm SHA256).Hash -ne $h) { throw "STOP: $s no es igual al original de $tag" }
  [pscustomobject]@{ copia = $s; origen = $tag; papeles = [string]$row[0].papeles; sha256 = $h }
})
if ((Get-FileHash -LiteralPath $src -Algorithm SHA256).Hash -ne $h) { throw "STOP PBT5-C02: cambio el original de $tag" }
ConvertTo-Json -InputObject ([pscustomobject]@{ lista_sha256 = $listSha; copias = @($rows) }) -Depth 4 | Out-File -LiteralPath (Join-Path $sp 's-copias.json') -Encoding utf8 -NoClobber
"OK: s01 y s02 iguales al original de $tag ($($row[0].papeles))"
exit 0
```

## 5. Los pasos, por copia y en orden

Es el detalle de [P] §4.2: los 65 pasos, con los bloques I y J, las inyecciones y los contextos construidos.

| Orden | Copia | Pasos | Notas |
|---|---|---|---|
| 1 | c01 | SPB-01 a 05; `RCDUMP pre` y los controles de `rcblk`, `rcnest` y `rcdyn`; P-09 (D1), el recuento en D1; SPB-08 | La línea de base de M-20 |
| 2 | e01 a eNN | P-09: el `RCDUMP estilo` de cada una (§4.13), con el paso de §4.7 y el bloque 8 | Solo lectura; después, el bloque 14 |
| 3 | s01 y s02 | SPB-07 (a) y (b), si algún dibujo guarda un estilo: el bloque 13b, y el paso de §4.7 con el bloque 8 | Si ninguno: SPB-07 PENDIENTE DEL OWNER |
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
| 18 | — | SPB-51 (el bloque 10) y SPB-52 (los bloques 2, 3, 4 y 15) | Siempre |

**Recuento:**

| | Pasos |
|---|---|
| **Total** | **65** |
| S | 56 |
| S+O, PENDIENTE DE REVISIÓN DEL OWNER | 9: SPB-08, 09, 16, 18, 20, 23, 25, 36 y 39 |
| Solo registrados | 3: SPB-22, SPB-43 y SPB-65 |
| Precondiciones | 9: P-01 a P-09 |

**Si la frontera de las 13:00 UTC llega a mitad:** se para en el límite de paso. Lo que falte, se sigue con el Owner, en el mismo orden. **Si la preparación no quedó hecha antes de las 12:00 UTC,** el orden 1 en adelante espera al Owner (§2).

## 6. Lo que la sesión no hace, y lo que se deja al Owner

**Lo que la sesión no hace:**
- **juzgar los S+O.** Se miden y se capturan, y quedan PENDIENTE DE REVISIÓN DEL OWNER;
- **decidir un OBSERVADO.** Se trae la evidencia;
- **fabricar el estilo de cotas,** ni dar SPB-07 por NOT AVAILABLE. Si ningún dibujo lo guarda, vuelve al Owner;
- **la UI completa en local** (Q-PBT5-1);
- **la revisión independiente de la evidencia.** La hace un revisor aparte, en solo lectura (Q-PBT5-8);
- **repetir un paso como adenda** por una discrepancia de esa revisión: solo corrige transcripciones; si cambia un resultado o hace falta AutoCAD, al Owner (PBT5-C16);
- **la disposición, la firma ni el gate-close;**
- **abrir AutoCAD sola** si la emisión y P-01 a P-08 no estaban hechas antes de las 12:00 UTC;
- **operar AutoCAD pasadas las 13:00 UTC** sin el Owner;
- **guardar una copia,** abrir un original, sobrescribir un archivo, escribir en la raíz de una unidad o tocar un AutoCAD que no abrió la sesión;
- **usar COM** o `Stop-Process`;
- **poner `RACKCAD_DEBUG_*`** fuera del entorno de los AutoCAD de SPB-59 a SPB-61;
- **cambiar la configuración de AutoCAD,** el registro o una variable de sistema (las de la configuración; el estado del dibujo de una copia, como `CTAB`, no lo es: §4.12);
- **custodiar** la paleta de referencias externas, el aviso de NETLOAD, un diálogo de archivos o una captura sin redactar (§8);
- **custodiar la lista local de la selección,** o copiar su contenido a algo que se custodie.

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
- la hora a la que quedaron hechas la emisión y P-01 a P-08, frente a las 12:00 UTC, y la de inicio, frente a las 13:00 UTC;
- el sha256 de la lista local de la selección (no su contenido).

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

**Lo que el registro no copia** (M-09):
- **órdenes con rutas:** el eco de `NETLOAD`, de `vla-open`, de los `defun` o de un STOP que nombre una ruta. Se escribe la orden con sus marcadores (`<worktree>`, `<raiz-de-trabajo-con-/>/…`) y el resultado;
- **la ruta resuelta de la biblioteca de bloques** (bloque B4) ni las líneas `ERROR` enteras (bloque 15): al registro custodiado van el origen, el nombre y el SHA-256, y el número de líneas con un resumen redactado;
- **nada de la lista local de la selección:** cada dibujo se nombra por su `eNN` y sus papeles de Smoke-2.

**La custodia:** un commit documental de la sesión, con `<raiz-de-trabajo>\…` redactado. No cambia el Candidato. **Antes del commit,** los patrones locales de privacidad pasan por todo lo que se va a custodiar:

```text
python -I <carpeta-del-paquete>/_tools/scan_custody.py --patterns <carpeta-del-paquete>/_tools/patterns.local.txt --dir <carpeta-preparada-para-custodiar>
```

- Sale con 0 si nada casa. Si algo casa, lista el archivo, cuántas coincidencias tiene y su sha256, nunca el texto que casa.
- **Lo que case va solo por su sha256,** como en la emisión y en F4B-E60: no se copia.
- Después va la revisión independiente de la evidencia (Q-PBT5-8).

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
| Sin tocar Trusted Locations, SECURELOAD, «Always Load», `FILEDIA`, `PICKFIRST` ni ninguna otra variable de la configuración; sin «Always ignore». El estado del dibujo de una copia que se cierra sin guardar (`CTAB`, `TILEMODE`) no es configuración | [F] §6.3; [S2]; M-11 |
| `RACKCAD` y la biblioteca; `RACKPANEL` | [G] §3, pasos 4 y 5 |
| Al final, nada de RackCad en `Applications`, `Loaded` ni `AssemblyMap` | SPB-52 |
