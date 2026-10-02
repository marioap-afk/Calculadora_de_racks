# I-64 — Evidencia de la unidad (ID30, Persistent RackCad Workspace)

Unit / Initiative / Workflow: `I-64` / I-64 / V2. Claim-Id `614371d5-441f-4f14-bac9-f97017105610`.
Contrato: [I-64-workspace-persistente-rackcad.md](../../initiatives/I-64-workspace-persistente-rackcad.md).
Fuentes: [brief del Owner](../../initiatives/I-64-owner-brief.txt) y [confirmación D0-R3](../../initiatives/I-64-coordinator-confirmation-d0-r3.txt).

Estado de esta evidencia: F0 / tarea D0 (reclamo y bootstrap). Las secciones son registros de su momento y no se reescriben
([WORKFLOW](../../WORKFLOW.md) §11.4). Los hechos posteriores al commit que publica este archivo (su CI exacta, el RELEASE de la
ventana) se informan al Coordinator y se registran en un commit posterior: un archivo no puede citar el SHA del commit que lo contiene.
Etiquetas: MEASURED (medido en este host), RECONSTRUCTED (leído de documentos o registros), INFERENCE, UNKNOWN.

## 1. Base de reclamo y clasificación de transición

| Hecho | Valor | Fuente |
|---|---|---|
| Base (`origin/main`) | `819955d61a6da4c811a11fbd11b5dca13f634b7c` (recién obtenida antes del reclamo) | MEASURED: `git fetch` + `git rev-parse origin/main` |
| Commit de reclamo | `6ea9b7e8002eaa69e17fbcf677e7855f40315ec4` (vacío; 2026-10-01T10:01:36-06:00) | MEASURED: `git log` |
| Primer push aceptado (reclamo) | `architecture/workspace-persistente-rackcad` nueva en `origin`, sin force | MEASURED: salida de `git push -u` |
| Rama / worktree | `architecture/workspace-persistente-rackcad` / `%USERPROFILE%\.codex\worktrees\architecture-workspace-persistente-rackcad` (fuera del árbol versionado; distinto del principal del Owner) | MEASURED: `git worktree list` |
| `WORKFLOW_V2_EFFECTIVE_SHA` | `8a021fb67c16dfccd6afc18448ea7e6a71a32364`: único commit con el trailer `Workflow-V2-Normative: I-56` en la historia de `origin/main` (`afd1077c`); primer merge first-parent cuyo segundo padre lo alcanza y el primero no (§11.2); coincide con el tag `integration/I-56` (objeto `41b1835b`); ancestro de la base | MEASURED: `git log --grep`, `git rev-list --first-parent --merges`, `git merge-base --is-ancestor` |
| Fin de la pausa de activación | `integration/I-56`: `Claim pause … end=2026-09-17T22:30:00Z` | MEASURED: `git cat-file -p integration/I-56` |
| Clasificación | **V2, T4** (posterior probado, base con el SHA efectivo, sin pausa). T7/T8 negativos: no consume diseño V1 de I-49/I-52/I-55 ni pertenece conceptualmente a ellas | [WORKFLOW](../../WORKFLOW.md) §11.3 |

Los encabezados de WORKFLOW §11, INITIATIVE_LIFECYCLE, PROMPT_TEMPLATES, AUTOMATION_PLAN y FOUNDATIONS siguen diciendo «not
effective»; la clasificación se apoya en §11.2 y en el tag, no en esos encabezados (orden D0, punto 2).

## 2. Fuentes recibidas

Cadena de custodia: Owner → Coordinator → sesión responsable, como archivos locales en `D:\IDs\I-64\` (fuera del repositorio).

| Fuente | Bytes | SHA-256 medido en el host | Nota |
|---|---:|---|---|
| `00-owner-brief.txt` (brief del Owner) | 18 668 | `f0be18e7a7f408be9f4bc4a66dd7c9a1e47e920124311e87b3ae77dac31df660` | UTF-8 sin BOM, solo LF, sin LF final. Coincide con el valor de la orden D0 y de D0-R3. Copia única byte-idéntica: [`I-64-owner-brief.txt`](../../initiatives/I-64-owner-brief.txt). `<NEXT_FREE_INITIATIVE_ID>` se conserva sin reemplazar (D0-R3) |
| `01-confirmacion-I64-D0-R3.txt` (confirmación del Coordinator) | 3 268 | `8af5ba57a7b533f8ce9d148dfa92f945d189644503cb53d33de3e9140eec0b85` | UTF-8 sin BOM, solo LF, con LF final. Copia única byte-idéntica: [`I-64-coordinator-confirmation-d0-r3.txt`](../../initiatives/I-64-coordinator-confirmation-d0-r3.txt) |
| `01-orden-D0.txt` (orden D0) | 4 443 | `e91bee1239a5cbd9bc3a15714b3a014d3e368c3d34ae7450e9a6ac782f266666` | no se versiona; D0-R3 la actualiza sin reescribirla |
| `02-intake-y-matriz-30-puntos.md` (intake) | 12 279 | `1849239d3821ed07c6f42084b0d4f09ed1aa634201bfc2676742265cabf036b7` | no se versiona; documento de planificación, no Discovery |
| D0-R1 y D0-R2 | — | — | **No recibidas por esta sesión**: D0-R3 dice complementarlas, pero no están en el paquete local. Contenido UNKNOWN; se aplican D0 y D0-R3 |

**Decisión registrada (D0-R3, CD-ID30-NUM-01):** I-64 es el número técnico de ID30, confirmado directamente por el Coordinator;
I-63 queda retirado como candidato de ID30. Esta resolución actualiza las referencias de la orden D0 y del intake a «I-63
candidato»; esos textos históricos no se reescriben.

**Blob de Git:** la identidad durable es el blob versionado. Con `core.autocrlf=true` (sin `.gitattributes` en la base), un
checkout en Windows puede mostrar CRLF en disco; el SHA-256 del blob es el que se compara con el de transporte. La comprobación
sobre el blob del commit de bootstrap se informa con su CI.

## 3. Preflight

**Primer preflight (D0, 2026-10-01 ≈14:57Z–15:07Z): STOP antes del reclamo.** MEASURED:
- `origin/main` = `819955d61a6da4c811a11fbd11b5dca13f634b7c`; CI de push 36866499374 (`ref` main, ese SHA) con los cuatro jobs en
  `success` (evidencia de CI, no local). Heads remotos: `main` y `feature/rackmirror-espejo-semantico`.
- Bloqueos devueltos: B-01 brief ausente en el host; B-02 número no acreditado (intakes paralelos de I-62 e ID20; carpeta del
  paquete `I-64` frente a «I-63 candidato»); B-03 ventana de ROADMAP no pedida a propósito.
- Sin rama, worktree, commit ni push. Única escritura Git: `git fetch origin --prune --tags` (sin cambios de referencias).

**Resolución:** D0-R3 confirmó I-64 y entregó el brief, verificado arriba (§2). Entretanto, ID20 tomó **I-63** y lo reclamó
(`architecture/parametros-calculados-resumen-proyecto`).

**Preflight diferencial (antes del reclamo, 2026-10-01T16:01:03Z):** MEASURED:
- `origin/main` sin cambio (`819955d6…`); heads remotos: `main`, I-52 (`c1982b2a`) e I-63 (`cba24838`). La rama
  `architecture/workspace-persistente-rackcad` estaba libre en el remoto y en local, y la ruta del worktree no existía.
- Ningún tag ni archivo `docs/**/I-64*` en las referencias remotas. Las únicas menciones de «I-64» en el historial están en el
  bootstrap de I-63 (decisiones y evidencia sobre la numeración); no son reserva ni reclamo.
- Worktree principal del Owner sin tocar (`main` en `95690c28`, limpio); AutoCAD no en ejecución.
- Push del reclamo: aceptado como `* [new branch]` a las 2026-10-01T16:01:44Z; `git ls-remote` devolvió el commit de reclamo.

## 4. Ventanas de `docs/ROADMAP.md`

Canal: mensajería entre sesiones locales de Claude. El canal no registra hora; las horas son aproximadas, del reloj de esta sesión.

**Acuses que esta sesión dio a otros escritores** (todos cerrados con RELEASE antes de pedir la ventana propia):

| Escritor | Acuse (msg_id) | RELEASE recibido | Nota |
|---|---|---|---|
| I-62 (1.ª ventana) | `618376f1-…` | sí (sin escrituras; I-62 se detuvo) | no se reutiliza |
| ID20 / I-63 | `117d185e-…` y `0209a42a-…` (confirmación para I-63) | sí, tras publicar su bootstrap | verificado en Git |
| I-62 (2.ª ventana) | `fa373730-…` | sí (sin escrituras; I-62 sigue sin reclamo) | |

**Ventana propia de I-64:**

| Escritor | Solicitud (msg_id) | Respuesta | Alcance |
|---|---|---|---|
| I-52 (sesión «I - 52») | `f10d0917-…` | **ACUSE** (≈16:00Z) | solo `docs/ROADMAP.md`, una fila nueva; sin HANDOFF, índice de ADR ni host de I-52 |
| I-63 (sesión «I - 63», ID20) | `9df58d93-…` | **ACUSE** (≈16:00Z) | solo `docs/ROADMAP.md` |
| I-62 (sesión «I - 62») | `6a5e1bd6-…` | **ACUSE** (≈16:00Z) | solo `docs/ROADMAP.md`; HANDOFF fuera |

INICIO = cada acuse (los tres, recibidos antes del reclamo). FIN = «RELEASE» de I-64 por el mismo canal tras publicar el bootstrap:
hecho posterior a este commit, que se informa al Coordinator.

Textos recibidos (literales):

```text
[I-52]
ACUSE (I-52) para I-64 (ID30): desde este mensaje hasta tu RELEASE, I-52 no escribe, rebasa ni publica cambios que toquen
docs/ROADMAP.md. Motivo: sin escritura de ROADMAP en curso ni planeada; mi rebase está diferido hasta después de las corridas de
host; sin reclamo ni rama incompatible. Host y demás superficies de I-52 siguen.

[I-63]
ACUSE — I-63 (ID20) no escribirá, rebasará ni publicará cambios que toquen docs/ROADMAP.md desde este mensaje hasta tu «RELEASE».
Motivo: el bootstrap de I-63 (cba24838) ya está publicado, no tengo escrituras de ROADMAP pendientes y espero la revisión de mi
Coordinator.

[I-62]
ACUSE — I-62 no escribe, rebasa ni publica cambios en docs/ROADMAP.md desde este mensaje hasta tu «RELEASE» por este canal.
Motivo: I-62 no tiene reclamo y sigue esperando el acuse de «I - 52», así que no tiene escritura de ROADMAP pendiente.
```

Nota de integración (de I-63): las dos ramas insertan su fila justo después de I-60; quien integre en segundo lugar resolverá un
conflicto textual de filas adyacentes en su rebase. La compatibilidad textual no acredita exclusividad (C61-G0-03).

## 5. Bootstrap

Archivos nuevos: el contrato `docs/initiatives/I-64-workspace-persistente-rackcad.md`; `docs/initiatives/I-64-owner-brief.txt`;
`docs/initiatives/I-64-coordinator-confirmation-d0-r3.txt`; este archivo. Archivo modificado: `docs/ROADMAP.md` (una fila en la
tabla de la Fase 6, tras I-60). **Nada más**: sin cambios en normas globales, HANDOFF, índice de ADR, FOUNDATIONS, `src/`, `tests/`,
`assets/`, `eng/`, `deploy/`, `.github/`, soluciones, `global.json` ni `Directory.Build.*`. `automation.enabled: false`. Sin archivo
de estado ni registro de decisiones en D0 (fuera del alcance documental de la orden; el estado se crea antes de la primera delegación).

## 6. Validación de D0

- Comprobación documental: `git diff --name-only` del bootstrap contra la base, todo bajo `docs/`; se informa con el commit.
- CI exigible: la corrida `push` del SHA exacto del bootstrap ([WORKFLOW](../../WORKFLOW.md) §4.5.2, commit documental); es un hecho
  posterior al commit y se informa al Coordinator.
- Suites Core/UI locales, builds del Plugin y Owner Validation: **no aplican a D0** (sin cambios de producto); no se ejecutaron.

## 7. Intersecciones activas observadas (para DC-07)

- **I-52** (`feature/rackmirror-espejo-semantico`, tip `c1982b2a`, §248). MEASURED: 21 detrás / 151 delante de `819955d6`
  (merge-base `95690c28`); 0 archivos bajo `src/`, `tests/` y `assets/`; toca `docs/ROADMAP.md` (3 líneas), `docs/HANDOFF.md` (+393)
  y `docs/adr/README.md` (+1); el resto, `docs/automation`, `docs/initiatives` y `eng/`. RECONSTRUCTED (§§247-248): host no
  iniciado (`S1-A..S4 = NOT_ELIGIBLE`, `FIRST_NON_GOVERNING_HOST_RUN = NOT_AUTHORIZED`, `REBASE_BEFORE_HOST = NO`, `BOUND_PRODUCT_SHA
  = 95690c28`); su producto planificado recorre `RACKEDITAR` → Actualizar → redibujo. UNKNOWN: archivos de su implementación futura.
- **I-63** (ID20). MEASURED: reclamo `cdae6421` y bootstrap `cba24838` (fila en Fase 6 tras I-60). RECONSTRUCTED: frontera de su
  contrato (ID20 = semántica, providers y agregación; ID30 = inventario runtime de navegación y UI); cruces previstos en enumeración
  de racks por RackId, RACKLISTA y comandos de inventario.
- **I-62** (portabilidad del Principal): reserva confirmada; sin reclamo al abrir I-64; espera el acuse de I-52.

## 8. Capacidades observadas para la ejecución delegada (sin instalar ni configurar nada)

MEASURED (2026-10-01):
- `gh` 2.96.0 autenticado (permisos `repo`, `workflow`). SDK .NET: el `dotnet` del `PATH` es 10.0.401 y no resuelve `global.json`;
  el de usuario resuelve **8.0.423**.
- AutoCAD 2025 instalado (`R25.0.171.0.0`), no en ejecución; no se ejecutó.
- Codex CLI 0.159.2 en `%LOCALAPPDATA%\OpenAI\Codex\bin\de8a38d2100ae498\codex.exe`, «Logged in using ChatGPT», fuera del `PATH`. El
  directorio de `bin` más reciente (`5cb96978…`) no contiene `codex.exe`: una receta que tome «la versión más nueva» fallaría.
- CLI `claude` 2.1.270 con `loggedIn: false`.
- `~/.codex/config.toml`: SHA-256 `42E15A039EFD7E1A9197423AFB14B4358C7D60C89FEDDD734583891DB6E732A5`, modificado
  2026-10-01T05:30:20Z; distinto de `89F375C6…` registrado por I-61. Sin delegación abierta no es P-01; antes de la primera hace falta
  una línea base nueva. Motivo del cambio: UNKNOWN.
- Sesión responsable: `claude-opus-5-5` / `xhigh` (observado con `get_session`); PRINCIPAL_COORDINATION → Frontera → MATCH con el
  catálogo (verificación 2026-09-30).

RECONSTRUCTED (catálogo en la base, sin sondas nuevas): Controller `gpt-6-luna`/`high`; Worker `claude-sonnet-5-5` `medium`/`high`;
`claude-opus-5-5` subagente solo lectura/herramientas; Haiku `STALE` desde 2026-10-01; Fable sin medir. Ningún Controller, Worker,
Architect ni subagente se invocó en D0. `AuthorityRevision`, `BaseSha`, `RunId` y contratos de gate: no existen todavía.

## 9. Métricas, conformidad, Owner Validation y tag

Métricas: UNKNOWN (sin gates funcionales). Arquetipo inicial: NEW ARCHITECTURE (brief). Conformidad: no aplica todavía. Owner
Validation: requerida por el brief; no aplica a D0. Tag de integración: no existe.

## 10. Resolución de D0 y hechos posteriores al bootstrap (CD-I64-D0-01/02)

Sección nueva: las secciones 1-9 son el registro histórico del bootstrap y no se reescriben. Fuente: orden del Coordinator
`I64_F0_D1_orden_discovery.txt` (8 644 bytes, SHA-256 `5b46811eacae664926c2cc86b0a38c1a6f0c08a72fc81086d5a3284e033a120b`, medido en
el host; no se versiona). **CD-I64-D0-01:** D0 aceptada (reclamo `6ea9b7e8…`, bootstrap `d8a02163…`, Claim-Id `614371d5-…`, V2/T4);
no declara F0 PASS, Freeze, Candidato ni integración. **CD-I64-D0-02:** los hechos posteriores al bootstrap se registran en este primer
commit útil de D1, sin commit ceremonial ni amend/rebase.

**CI exigible de D0** (MEASURED con `gh run view`; evidencia de CI, no local):

| Commit | Corrida | `event` | `ref` | `head_sha` | Jobs | Creada / actualizada (UTC) |
|---|---|---|---|---|---|---|
| reclamo | 36888872244 | `push` | `refs/heads/architecture/workspace-persistente-rackcad` | `6ea9b7e8002eaa69e17fbcf677e7855f40315ec4` | Tests (Domain + Application), UI Tests (WPF controls, net8.0-windows), Build UI (WPF, valida API de Application), Build Plugin without AutoCAD: `success` | 16:01:50 / 16:06:22 |
| bootstrap | 36889041587 | `push` | `refs/heads/architecture/workspace-persistente-rackcad` | `d8a0216306f40e4d502b77310c1632cdbb84d70d` | los mismos cuatro: `success` | 16:03:09 / 16:07:23 |

**RELEASE de la ventana propia** (canal: mensajería entre sesiones locales de Claude). El canal no devuelve hora ni acuse de lectura;
el resultado de cada envío fue «en cola» en la sesión destino. La hora es una **estimación de esta sesión**: inmediatamente después del
push del bootstrap (16:03:01Z, medido) y antes de 16:04Z.

| Interlocutor | msg_id | Alcance |
|---|---|---|
| sesión «I - 52» | `4e98f665-4979-45dc-a896-840bd56742fe` | fin de la ventana sobre `docs/ROADMAP.md`; sin HANDOFF, índice de ADR ni host |
| sesión «I - 63» (ID20) | `f341dded-27ab-48e6-9e9f-5f224c936f1b` | fin de la ventana sobre `docs/ROADMAP.md` |
| sesión «I - 62» | `9be7a49c-89c2-4f88-8403-0a918399fe6d` | fin de la ventana sobre `docs/ROADMAP.md` |

**Acuse posterior dado a I-62** (msg `ff2d1346-06a0-44d5-9034-78a05ed7d779`, hora estimada por esta sesión: poco después de 16:03Z):
ventana sobre `docs/ROADMAP.md` para su bootstrap. Su RELEASE llegó por el mismo canal, sin hora de canal, antes del preflight de D1
(16:19:53Z). Su bootstrap publicado: `85ae4324` sobre el reclamo `a851841a`.

**Identidad de las copias versionadas** (MEASURED sobre el commit de bootstrap `d8a02163`). Se distinguen tres valores: el
identificador Git del blob, el SHA-256 de los bytes del blob y el SHA-256 de transporte medido en el host.

| Archivo | Identificador Git del blob | SHA-256 de los bytes del blob | SHA-256 de transporte | Bytes |
|---|---|---|---|---:|
| `docs/initiatives/I-64-owner-brief.txt` | `8970c15e228adf94c96a98fd3f577437ab66dab0` | `f0be18e7a7f408be9f4bc4a66dd7c9a1e47e920124311e87b3ae77dac31df660` | igual | 18 668 |
| `docs/initiatives/I-64-coordinator-confirmation-d0-r3.txt` | `cfc9e721e42c2c3a63cee171d02e5ee5cd960939` | `8af5ba57a7b533f8ce9d148dfa92f945d189644503cb53d33de3e9140eec0b85` | igual | 3 268 |

Ningún blob del bootstrap contiene CR.

## 11. F0-D1 — Discovery (hechos de esta tarea)

Resultado: [I-64-discovery.md](../../initiatives/I-64-discovery.md). **No** es Proposal, Freeze, revisión del Architect ni GATE PASS.

| Hecho | Valor |
|---|---|
| Autorización | orden F0-D1 (CD-I64-F0-D1-01/02): Discovery de lectura y documentación; EXP-07 y EXP-09 condicionales |
| Preflight diferencial (16:19:53Z) | `HEAD` = `origin/architecture/workspace-persistente-rackcad` = `d8a02163…`; árbol limpio; sin operaciones Git en curso; ningún proceso con la ruta del worktree; AutoCAD no en ejecución; `origin/main` = `819955d6…` (sin cambio) |
| Ramas paralelas observadas (por referencia remota) | I-52 `c1982b2a`; I-62 `85ae4324`; I-63 `cba24838` al inicio y `b557ea3b` durante D1. Lectura solo como coordinación |
| Escrituras versionadas | solo los tres archivos de la allowlist: el Discovery, esta evidencia y el contrato (estado y referencias) |
| Ejecución | sin AutoCAD, sin builds, sin pruebas, sin prototipos, sin Controller, Worker, Architect ni subagentes |
| Expansiones | EXP-07 ejecutada (§10 del Discovery); EXP-09 ejecutada solo para M-08 (activado); EXP-04, EXP-05 y EXP-08 positivas sin investigación adicional |

**Superficie de API leída del host** (MEASURED; metadatos con `System.Reflection.Metadata`, sin cargar los DLL; AutoCAD 2025
`R25.0.171.0.0`):

| DLL | SHA-256 |
|---|---|
| `C:\Program Files\Autodesk\AutoCAD 2025\acmgd.dll` | `7AA8BF5F79F3F980DCC9F4CF9483F8438E773F6BF7C05EC3B263449B2846BC8C` |
| `C:\Program Files\Autodesk\AutoCAD 2025\accoremgd.dll` | `31025BC01ABC2CB398040AC82A5018E4FCD7A80FE8A2012DE0329E7F8ED746AA` |
| `C:\Program Files\Autodesk\AutoCAD 2025\acdbmgd.dll` | `C360186CC0702E210635895B7608D1D8BD5FEEC6A00612B7581EF160FE09CEDE` |
| `C:\Program Files\Autodesk\AutoCAD 2025\AcWindows.dll` | `236DEEACD835B9BDD33B51061F8A9EBE9489FC6EF2A2F93148DFCF32F47588C3` (no contiene `PaletteSet`; el tipo está en `acmgd.dll`) |

**Documentación oficial consultada el 2026-10-01** (RECONSTRUCTED):

| Fuente | URL | Versión o fecha |
|---|---|---|
| Autodesk, `PaletteSet` (clase, métodos, propiedades, eventos, `AddVisual`) | `https://help.autodesk.com/cloudhelp/2025/ENU/OARX-ManagedRefGuide/files/OARX-ManagedRefGuide-Autodesk_AutoCAD_Windows_PaletteSet.html` y páginas de miembros hermanas | ruta 2025; sin marcador de versión en la página |
| Autodesk, `Application.ShowModelessWindow` (sobrecargas con URI) | `https://help.autodesk.com/cloudhelp/2025/ENU/OARX-ManagedRefGuide/files/OARX-ManagedRefGuide-__OVERLOADED_ShowModelessWindow_Autodesk_AutoCAD_ApplicationServices_Application.html` | ruta 2025 |
| Autodesk, «Lock and Unlock a Document (.NET)» | `https://help.autodesk.com/cloudhelp/2025/ENU/OARX-DevGuide-Managed/files/GUID-A2CD7540-69C5-4085-BCE8-2A8ACE16BFDD.htm` | ruta 2025; sin marcador de versión |
| Autodesk, «Code Differences under the Application Execution Context» (ObjectARX) | `https://help.autodesk.com/cloudhelp/2022/ENU/OARX-DevGuide/files/GUID-634ADE0B-35FD-4146-A2D0-3621D2FB5B0C.htm` | **2022**; no verificada para 2025 |
| Microsoft Learn, «WPF and Win32 interop» | `https://learn.microsoft.com/en-us/dotnet/desktop/wpf/advanced/wpf-and-win32-interoperation` | `updated_at` 2025-08-27 |

**Hallazgos laterales** (no se corrigen en D1; candidatos a `docs/ideas-futuras.md` en el cierre):

- [WORKFLOW](../../WORKFLOW.md) §7 cita tamaños antiguos de los editores grandes; hoy: Selectivo 3 713, Push Back 3 630 y Dinámico 3 508
  líneas (MEASURED).
- [ARCHITECTURE](../../ARCHITECTURE.md) §7.3 cita `RackDialogWindow`, retirada del código, y §8 cita `RackFrameCommands`, que ya no
  existe (MEASURED).
- [FOUNDATIONS](../../FOUNDATIONS.md) no tiene entradas para Auto Rack Naming, el motor de expresiones ni la base AUTH-01..07 de
  Shared View (MEASURED).
- `RackListBuilder.KindLabel` no etiqueta Push Back (hueco declarado en el propio código; MEASURED).
- El directorio más reciente de `%LOCALAPPDATA%\OpenAI\Codex\bin` no contiene `codex.exe` (MEASURED en D0).

**CI de esta tarea:** la corrida `push` del SHA que publica este archivo es un hecho posterior al commit; se informa al Coordinator en
la entrega y no se encadena otro commit para citarla (CD-I64-D0-02).

## 12. F0-D1-R1 — Resolución del Coordinator y correcciones (CD-I64-D1-01/02)

Sección nueva; las anteriores no se reescriben.

**Fuentes** (archivos locales en `D:\IDs\I-64\`, medidos en el host; no se versionan):

| Archivo | Bytes | SHA-256 |
|---|---:|---|
| `01-orden-F0-D1-R1.txt` (orden) | 4 230 | `9683cdbf538b6b7ddd4df4b368417b42e834cde03b2781f02ad7128d77d45168` |
| `02-revision-Coordinator.md` (revisión, adjunto normativo de la orden) | 14 201 | `fe47ea6ce270397eb1edc447829d91fbd33fdf461692f5b129fd92792e075f80` |

**Resolución, en resumen saneado:**
- **CD-I64-D1-01:** D0 sigue ACCEPTED. D1 se recibe como entrega publicada con su CI exacta. La revisión queda en CHANGES REQUIRED para
  cerrar el Discovery. Se autoriza F0-D1-R1 (correcciones y ampliaciones documentales), no Proposal, ADR, Architect formal, Freeze ni
  producto. NEW ARCHITECTURE y M-04..M-07 confirmados; M-08 se mantiene activado de forma conservadora.
- **CD-I64-D1-02:** expansiones autorizadas EXP-04/05/06/08, delta de EXP-07, EXP-09 solo para M concretos UNKNOWN y EXP-02 condicional,
  con sus preguntas, áreas y salidas.
- **Hallazgos REQUIRED:** C64-D1-01..07. Disposición por ID abajo y en el Discovery §17.

**CI de la entrega D1** (MEASURED con `gh run view`; evidencia de CI, no local): corrida **36894342807**, `event=push`,
`ref=refs/heads/architecture/workspace-persistente-rackcad`, `head_sha=cf034e594daf6b222013808c9795fd92a66940cb`. Jobs Tests (Domain +
Application), UI Tests (WPF controls, net8.0-windows), Build UI (WPF, valida API de Application) y Build Plugin without AutoCAD, los
cuatro `success`. Creada 16:45:13Z, actualizada 16:49:35Z. El blob del Discovery revisado era `7748152ec62984d0c9dedddc94d629f5ab0a61d7`.

**Disposiciones C64-D1-01..07:**

| ID | Disposición | Dónde |
|---|---|---|
| C64-D1-01 | resuelto: matriz de fallos con fallo sin commit, commit parcial, fallo posterior al commit y desconocido; EXP-06 positiva | Discovery §9.12, §3.4 |
| C64-D1-02 | resuelto: el sobre no es diseño tipado ni sistema resuelto; etapas separadas; coste `UNKNOWN` | Discovery §9.4, §9.8 |
| C64-D1-03 | resuelto: lecturas y escrituras por sistema; hipótesis H-LOCK acotada con verificación; riesgos de normalización | Discovery §9.7, §9.13, §9.14 |
| C64-D1-04 | resuelto: M-01 activado; M-02 y M-03 `UNKNOWN` tratados como activados (EXP-09); M-08 con delta exacto; EXP-02 reevaluada | Discovery §12, §13 |
| C64-D1-05 | resuelto: intención explícita interpretada por el host; `BindingIntent`; EXP-04; EXP-05; trazas DC-08 con aserción; censo por tipo | Discovery §3.3, §3.4, §7, §8, §11 |
| C64-D1-06 | resuelto con consulta pendiente: delta DC-07, hechos de host acotados, consultas registradas | Discovery §10 |
| C64-D1-07 | resuelto: enrutamiento por tarea, sin elegir ni sondear modelos | Discovery §14, §15 |

**Preflight diferencial (17:13:37Z, MEASURED):**
- `HEAD` = remoto = `cf034e59…`; árbol limpio; sin operaciones Git en curso; ningún proceso con la ruta del worktree.
- `origin/main` = `819955d6…`, sin cambio, así que no hubo rebase.
- Había **un proceso de AutoCAD en ejecución**, no iniciado ni tocado por esta sesión.

**Ramas paralelas** (por referencia remota, solo como coordinación):

| Rama | Tip | Avance |
|---|---|---|
| I-52 | `c1982b2a` | sin cambio |
| I-62 | `2b7976b0` | antes `85ae4324` → `f25dd1d5`; solo documentos |
| I-63 | `85293273` | antes `cba24838` → `b557ea3b` → `dbfe1150`; solo documentos |

**Consultas de coordinación** (canal: mensajería entre sesiones locales; el canal no da hora ni acuse de lectura):
- **I-63, consulta de frontera.** msg `474704b7-b532-4157-a9dc-38d32f6304e7`, enviada ≈17:20Z (estimación de esta sesión), en cola.
  **Respuesta recibida** antes de 17:23Z:
  - la consulta al Master la preparó su Coordinator y **no está enviada** (no tiene canal; irá por el Owner);
  - no hay Proposal ni decisión;
  - su lectura es que un índice de navegación sin semántica de métricas cae del lado de ID30, sin conflicto con la frontera escrita;
  - el mínimo común sigue abierto;
  - corrige su Discovery: `RackListBuilder` ya agrupa por Id en Application, y lo que falta es un contrato común.

  Acuse de I-64: msg `c6c38df8-adef-45c3-af1c-599a5345c79b`, en cola.
- **I-52, consulta de convivencia** (host, eventos y redibujo). msg `7b90958e-ac63-4f30-bfc4-9f5920098b4a`, enviada ≈17:20Z, en cola.
  **Sin acuse ni respuesta** al preparar este commit.

**Documentación oficial adicional consultada el 2026-10-01** (RECONSTRUCTED):

| Fuente | URL | Versión |
|---|---|---|
| Autodesk, «Guidelines for Event Handlers (.NET)» | `https://help.autodesk.com/cloudhelp/2025/ENU/OARX-DevGuide-Managed/files/GUID-FE7D58D5-28A0-4C98-A876-D4D48F06D0B2.htm` | ruta 2025 |
| Autodesk, `BlockTableRecord.GetBlockReferenceIds` | `https://help.autodesk.com/cloudhelp/2022/ENU/OARX-ManagedRefGuide/files/OARX-ManagedRefGuide-Autodesk_AutoCAD_DatabaseServices_BlockTableRecord_GetBlockReferenceIds__MarshalAsUnmanagedType_U1__bool__MarshalAsUnmanagedType_U1__bool.html` | **2022**; la ruta 2025 dio 404 |

**Ejecución:** lectura estática y documentación. Sin AutoCAD, builds, pruebas, prototipos, sondas, subagentes, Controller ni Worker.
Escrituras versionadas: solo los tres archivos de la allowlist.

**Elevación:** el candidato a fundación común con I-63 en la enumeración lógica de racks se eleva al Coordinator para el Master. No se
diseña ni se consume.

## 13. F0 / Proposal V1 — Aceptación de D1-R1, decisión del Master y hechos posteriores

Sección nueva; las anteriores no se reescriben.

**Fuente:** orden del Coordinator «I-64 — F0 / PROPOSAL V1», recibida en el chat de la sesión el 2026-10-01, antes de las 19:09Z. No llegó
como archivo a `D:\IDs\I-64\` (listado MEASURED a las 19:09Z), así que no tiene hash. Resumen saneado:

- **Aceptación:** el Coordinator acepta D1-R1 (`c6c44828`) y cierra el Discovery. No se implementa producto.
- **`MASTER-I63-I64-01`:**
  - I-63 e I-64 comparten solo una fundación mínima de snapshot lógico neutral por RackId;
  - I-63 posee métricas, providers y agregación, y es el autor inicial del contrato puro común;
  - I-64 posee el índice runtime por documento, la invalidación, ObjectIds y handles, la navegación, la selección, las pestañas y los
    borradores;
  - el snapshot de solo lectura no decide la población de métricas ni sustituye a las autoridades de pertenencia de hermanas para mutar;
  - no se inventa RackId; no se resuelven diseños, geometría ni BOM para formar el snapshot; no se persiste;
  - F1 de I-64 puede avanzar sin esa fundación; F2 consumirá el contrato de I-63 solo cuando esté integrado en `origin/main`.
- **Smoke:**
  - en la estación principal del Owner, con AutoCAD 2025;
  - solo después de F1 y de un RELEASE explícito de la ventana de host de I-52;
  - una sola sesión de AutoCAD;
  - I-64 no cambia perfil, `TRUSTEDPATHS`, `SECURELOAD`, ACL ni la configuración de I-52;
  - si cargar el DLL exige modificar la seguridad, STOP al Owner o al CAD manager.
- **Registro:** la respuesta tardía de I-52 va en el próximo commit útil, no en uno ceremonial (este commit).
- **Encargo:** redactar y publicar la Proposal V1 completa y entregar el paquete para la revisión adversarial del Architect, sin declarar
  consenso, Freeze ni GATE PASS. `IMPLEMENTATION AUTHORIZATION = NO`.

**CI de la entrega D1-R1** (MEASURED con `gh run view`; evidencia de CI, no local): corrida **36900174150**, `event=push`,
`ref=refs/heads/architecture/workspace-persistente-rackcad`, `head_sha=c6c44828de103c293fac7b4edd6385e285243b9f`. Jobs Tests (Domain +
Application), UI Tests (WPF controls, net8.0-windows), Build UI (WPF, valida API de Application) y Build Plugin without AutoCAD, los
cuatro `success`. Creada 17:32:51Z, actualizada 17:37:08Z. El blob del Discovery aceptado es `3565015c21a44b08df564b3277b5546a8a6ce013`.

**Respuesta tardía de I-52** (RECONSTRUCTED). Llegó después de publicar `c6c44828`: el registro de esta sesión la recibió a las
17:33:28Z, del remitente «I - 52». I-52 la declara informativa y sin compromiso; RACKMIRROR no está implementado y su G3 está detenido.
Resumen saneado:
1. **Host:**
   - su campaña CT-21D (no gobernante) está ligada a esta máquina física y a su perfil «<<Unnamed Profile>>»;
   - durante la campaña rigen: sin actualizaciones; sin cambios de perfil, plugins, `TRUSTEDPATHS` ni `SECURELOAD`; una sola `acad.exe` a
     la vez, porque otra concurrente invalida su sesión;
   - un smoke de I-64 en esta máquina debe no solaparse con sus sesiones (al responder no había ninguna iniciada ni autorizada), no tocar
     esos ajustes ni instalar plugins, y avisarle antes por el canal entre sesiones;
   - un perfil o una sesión distintos no bastan: crear un perfil o cambiar el registro de AutoCAD altera la máquina ligada;
   - lo más limpio es otra máquina o esperar a que cierren sus sesiones S1..S3, que aún no tienen fecha (dependen del CAD manager).
2. **Eventos:** lo previsto para RACKMIRROR es un comando que escribe en una transacción del llamador (AUTH-15), sin suscripción a
   eventos; la observación de eventos es instrumentación de I-52 bajo `eng/research`. No es un compromiso de diseño.
3. **Redibujo:** la intención es crear un rack **nuevo** espejado («<base> - espejo») sin mutar el origen, así que no prevé redibujar
   ni redefinir el origen. Está sin implementar y su ADR está propuesto: es un supuesto, no un contrato.

Acuse de I-64: msg `6ace751e-5062-42ae-87f9-9e91a1155313`. Tratamiento: Proposal V1 §12.1 y §15.

**Preflight (MEASURED):**
- 19:09:19Z: `HEAD` = remoto = `c6c44828…`; árbol limpio; `origin/main` = `819955d6…`, sin cambio, así que no hubo rebase.
- 19:24:18Z: ningún proceso `acad` en ejecución.

**Ramas paralelas** (por referencia remota, solo como coordinación):

| Rama | Tip | Avance |
|---|---|---|
| I-52 | `88138f01` | antes `c1982b2a`; un commit (§249, 18:01:28Z) en `docs/automation/decisions/I-52.md` y `eng/research`; 0 archivos de `src/`, `tests/` y `assets/` |
| I-62 | `ea055591` | antes `2b7976b0`; su Proposal V1, el paquete del Architect y el borrador de su ADR; solo documentos |
| I-63 | `85293273` | sin cambio |

**Metadatos de API leídos sin cargar los ensamblados** (MEASURED; mismos archivos y hashes que §11):
- `accoremgd.dll`: `DocumentBeginCloseEventArgs` con `Veto` e `IsVetoed`; `DocumentLockModeChangedEventArgs` con `Veto`; en `Application`,
  los eventos `Idle`, `BeginQuit`, `QuitWillStart` y `QuitAborted`.
- `acmgd.dll`: `PaletteSet` con los eventos `Load` y `Save`, y `KeepFocus`, `Visible` y `Name`.

Se usan en las decisiones D-01 y D-17 de la Proposal.

**Ejecución:** redacción directa de la sesión responsable, con lectura estática de código y documentos, ramas paralelas leídas por
referencia remota y los metadatos anteriores. Sin AutoCAD, builds, pruebas, prototipos, subagentes, Controller ni Worker. Archivos
versionados: [Proposal V1](../../initiatives/I-64-proposal-v1.md) (nueva), [paquete del Architect](../../initiatives/I-64-architect-package-v1.md)
(nuevo), el contrato y esta evidencia.

**Estado:** Proposal V1 publicada para revisión, sin consenso, Freeze ni GATE PASS. `IMPLEMENTATION AUTHORIZATION = NO`.

## 14. F0 / Proposal V2 — Veredicto del Architect sobre V1, orden del Coordinator y hechos posteriores

Sección nueva; las anteriores no se reescriben.

**CI de la Proposal V1** (MEASURED con `gh run view`; evidencia de CI, no local): corrida **36915005173**, `event=push`,
`ref=refs/heads/architecture/workspace-persistente-rackcad`, `head_sha=dcc16bed371207c8c68dd6bc6a7dd7e5a7acee6d`. Jobs Tests (Domain +
Application), UI Tests (WPF controls, net8.0-windows), Build UI (WPF, valida API de Application) y Build Plugin without AutoCAD, los
cuatro `success`. Creada 19:32:12Z, actualizada 19:35:55Z. Blobs de V1: Proposal `962fc1958f3b6885d43eff3a94190325edaf850f`, paquete
`d54f5bd231fb5a5fc0ef3bc6319322163a98c780`.

**Veredicto del Architect sobre V1** (RECONSTRUCTED):
- **Procedencia.** La orden del Coordinator lo acepta, pero no lo transcribe. La sesión autora lo leyó en la transcripción local de la
  sesión separada «I-64 Architect Review Proposal V1» (archivo de sesión `caf7ae64-ba5b-4648-ae0b-f7aeb9e7103a.jsonl` del proyecto, mensaje
  del Architect de 2026-10-01T20:07:09Z), localizada con la búsqueda de transcripciones. El texto extraído mide 30 092 bytes UTF-8, con
  SHA-256 `2cf82c1e96b83859bbdba517425d9ed251b7c542f48c116a469e11675474a4a3`. No se versiona aquí; se trata como dato.
- **Contenido**, en resumen saneado:
  - objeto: commit `dcc16bed`, Proposal blob `962fc195`, paquete blob `d54f5bd2`, Discovery blob `3565015c`, CI 36915005173;
  - modo SEPARATE SESSION; revisor ≠ autor; declara como límite de independencia que es el mismo modelo que la sesión autora;
  - puntos acordados: autoridad del DWG, `PaletteSet` con contingencia, capas, estructura del puente, D-05, dirección de la selección,
    pestañas, D-10 como evolución, D-11 (sin elevar a I-55), núcleo de D-12, resultado de D-13, D-16, D-18, D-20, D-21 y el plan de gates
    con piloto Push Back; M-02 y M-03 NOT ACTIVATED con condiciones;
  - **CHANGES REQUIRED**: A64-PV1-01..22 REQUIRED, O-01..O-17 OPTIONAL, y la disposición de C64-PV1-01..04 (revisión del Coordinator sobre
    V1, que esta sesión no recibió directamente);
  - riesgos R-01..R-09.

**Orden F0 / Proposal V2 del Coordinator**, recibida en el chat de la sesión el 2026-10-01 tras el veredicto. No llegó como archivo a
`D:\IDs\I-64\` (listado MEASURED a las 20:11Z), así que no tiene hash. Resumen saneado:
- acepta el veredicto (CHANGES REQUIRED sobre `dcc16bed` / `962fc195`) y los REQUIRED A64-PV1-01..22 para corregir en V2;
- no reabrir el Discovery, no implementar, no ejecutar AutoCAD, no declarar consenso, Freeze ni GATE PASS;
- redactar una Proposal V2 **autocontenida** que disponga cada A64-PV1-01..22;
- 22 precisiones vinculantes, citadas en la Proposal como PV2-01..PV2-22:
  - cierre de F4 con Smoke-2 y las comprobaciones de host dentro de él; hito Smoke-NAV tras F2;
  - atomicidad por resultado sobre la costura de I-55, con importación fuera de la transacción authored, `Commit()` que lanza =
    Desconocido y renombrado fuera del núcleo;
  - separación de StaleBase, InitialDraftState, CurrentDraftState y Dirty; base y estado inicial de una sola lectura;
  - registro completo de Selectivo sin entradas de escaneo; comparación por atribución;
  - veto continuo de cierre con descarte explícito; identidad de instancia y peticiones ligadas;
  - eventos en cualquier momento con manejadores que solo encolan; eco por contenido; foco; contención de excepciones; preview y texto
    pendiente;
  - panel nunca abierto frente a oculto y escenario masivo; ciclo de vida de suscripciones; guardas de transacciones;
  - INV-19 con ADR-0029 D13 completo; acuse de I-52 y diff del perfil;
  - diferimiento `OWNER-RESERVED`; STOP al Master si el snapshot no llega; oráculos de UI y OV para avisos, admisión y Desconocido;
- materialidad aceptada para V2: M-01, M-04..M-08 ACTIVATED; M-02 NOT ACTIVATED con las restricciones de V1 y V2; M-03 NOT ACTIVATED
  mientras los comandos clásicos permanezcan observacionalmente invariantes;
- actualizar la coordinación con las puntas de I-52, I-62 e I-63; registrar la CI de V1 en este commit; publicar V2 y su paquete; esperar
  la CI exacta; la re-revisión vuelve al **mismo** Architect. `IMPLEMENTATION AUTHORIZATION = NO`.

**Aviso de I-63 sobre P-14** (RECONSTRUCTED; canal entre sesiones, remitente «I - 63», recibido por esta sesión a las 19:59:00Z):
- el Owner decidió en el chat de I-63 la frontera P-14: **sin contrato común** de inventario o enumeración entre I-63 e I-64; cada
  iniciativa implementa su propio mecanismo; la duplicación se acepta; una iniciativa posterior podrá revisarla; para I-63, la consulta al
  Master queda superada;
- I-63 nunca recibió MASTER-I63-I64-01; preguntado, el Owner eligió que su decisión **prevalece para I-63** y que I-63 avisara a I-64;
- I-63 lo registró en su rama (`f0b063e9`, decisiones de I-63 §6) y no compromete diseño de I-64.

Acuse de I-64: msg `83fa245d-78ff-4639-99a0-242a9b20bbcd`, sin cambios en la Proposal y con elevación al Coordinator. La orden V2 no
resuelve expresamente el conflicto; la Proposal V2 aplica MASTER-I63-I64-01 y PV2-21 y lo declara abierto (§15).

**Preflight (MEASURED):**
- 20:11:20Z: `HEAD` = remoto = `dcc16bed…`; árbol limpio; `origin/main` = `819955d6…`, sin cambio, así que no hubo rebase.
- 20:16:25Z: ningún proceso `acad` en ejecución.

**Ramas paralelas** (por referencia remota, solo como coordinación):

| Rama | Punta | Avance desde §13 |
|---|---|---|
| I-52 | `d8078ef3` | un commit (19:32:09Z): dictamen de `TRUSTEDPATHS` y revisión de scripts; 3 archivos; 0 de `src/`, `tests/` y `assets/` |
| I-62 | `486e45e7` | dos commits: Proposal V2 y V3 con sus paquetes; solo documentos |
| I-63 | `f6da2768` | tres commits: Discovery R2; aceptación de R2 con la decisión P-14 (`f0b063e9`); decisiones del Owner P-01..P-05 sobre sus métricas (20:21:18Z, leída antes de publicar); solo documentos |

**Hechos de código verificados para la V2** (MEASURED en la base `819955d6`):
- `ISiblingRedrawPort` (`Prepare`, `Mutate`, `Post`) y `RackSiblingRedrawRun.Execute`, que convierte una excepción de `Mutate` en
  `Discarded`;
- `SiblingRedrawTransaction.Mutate`, que abre una transacción con `using`, aplica las unidades sin confirmar y llama a `Commit()` una vez;
  `Post` ejecuta el postproceso de las unidades y un `Regen`;
- `SiblingRedrawDebugFaultInjection`, solo Debug, con la variable de entorno `RACKCAD_DEBUG_FAIL_SIBLING_REDRAW_UNIT`; OV-RED-06 de I-55 la
  usa al lanzar AutoCAD;
- `CallerOwnedFacadeGuardTests`: la primitiva no confirma, no abre transacción, no bloquea y no regenera;
- `BlockLibraryImporter` traga y registra las excepciones de importación;
- `RackVariablesCommands` abre rondas modales dentro de un bucle del mismo comando;
- `RackCantileverCommands` confirma su propia transacción al crear la definición;
- I-55 V5 §4.4 (transacción de `Mutate`, INV-TX-1), §4.6 (OM-5) y §4.10 (renombrado en POST).

**Ejecución:** redacción directa de la sesión responsable, con lectura estática de código y documentos, ramas paralelas por referencia
remota y la transcripción local citada. Sin AutoCAD, builds, pruebas, prototipos, subagentes, Controller ni Worker. Archivos versionados:
[Proposal V2](../../initiatives/I-64-proposal-v2.md) (nueva), [paquete de re-revisión](../../initiatives/I-64-architect-package-v2.md)
(nuevo), el contrato y esta evidencia. La Proposal V1 y su paquete no cambian.

**Estado:** Proposal V2 publicada para la re-revisión del mismo Architect y del Coordinator, sin consenso, Freeze ni GATE PASS.
`IMPLEMENTATION AUTHORIZATION = NO`.

## 15. F0 / Proposal V3 — MASTER-I63-I64-02 y hechos posteriores

Sección nueva; las anteriores no se reescriben.

**CI de la Proposal V2** (MEASURED con `gh run view`; evidencia de CI, no local): corrida **36921626892**, `event=push`,
`ref=refs/heads/architecture/workspace-persistente-rackcad`, `head_sha=ab4efe865638cbe121c0ecd232a689563ba7ee6a`. Jobs Build UI (WPF, valida
API de Application), UI Tests (WPF controls, net8.0-windows), Tests (Domain + Application) y Build Plugin without AutoCAD, los cuatro
`success`. Creada 20:26:54Z, actualizada 20:31:10Z. Blobs de V2: Proposal `7bf98cca583c4b5c0ea2c090f2b239cdfd7f8239`, paquete
`0a66d7c23fa782880ea99d8a51fda14d00f6b054`. V2 no llegó a re-revisarse y queda como registro histórico.

**Orden F0 / Proposal V3 del Coordinator**, recibida en el chat de la sesión el 2026-10-01. No llegó como archivo a `D:\IDs\I-64\` (listado
MEASURED a las 22:18Z), así que no tiene hash. Resumen saneado:
- V2 (`ab4efe86`) queda como registro histórico y no se edita;
- el Coordinator acepta la disposición de A64-PV1-01..22 de V2 y resuelve antes de la re-revisión el conflicto MASTER-I63-I64-01 / P-14;
- **MASTER-I63-I64-02**, que sustituye parcialmente a MASTER-I63-I64-01:
  1. se retira la obligación de crear una fundación o snapshot compartido entre I-63 e I-64;
  2. se retira la autoría inicial asignada a I-63;
  3. se conserva la frontera: I-63 = métricas, providers, población y agregación; I-64 = captura e índice runtime para navegación,
     selección, UI y sesión; RackId e identidad de vista = autoridades integradas vigentes; pertenencia para mutar = autoridades
     integradas correspondientes; DWG authored = única autoridad persistida;
  4. I-64 puede usar una representación pura interna de hechos neutrales para su índice, sin declararla fundación reutilizable ni contrato
     consumido por I-63;
  5. convertirla después en fundación común requerirá una decisión explícita nueva del Master;
  6. I-64 no depende de la integración de I-63 para cerrar ningún gate;
  7. I-63 no consume código no integrado de I-64;
- **plan de gates confirmado:** F1 → Smoke-1; F2 Context Navigation (AutoCAD ↔ RackCad para el rack en contexto; Seleccionar, Ver, Centrar y
  Localizar; sin depender de I-63) → Smoke-NAV; F3 Browser Session; F4 Draft + Update (Candidato F4 → evidencia automatizada → Smoke-2 →
  revisión del Coordinator → F4 PASS); F5 integraciones de editores; F6 Document-wide Inventory + Selection (N) + evaluación de ID3, con el
  inventario global a cargo de I-64 sobre autoridades integradas y sin esperar a I-63; F7 rendimiento y regresión; READY.
  `ProjectSummary`/ID20 puede alojarse después como consumidor o superficie futura, sin bloquear a I-64;
- redactar una V3 **autocontenida** que conserve las correcciones de V2, incorpore MASTER-I63-I64-02, elimine toda dependencia obligatoria
  de I-63 y el STOP por ausencia del snapshot, actualice D-05, gates, coordinación, riesgos, UNKNOWN y el borrador de ADR, mantenga la
  separación entre inventario runtime y métricas, los tres smokes y M-02/M-03 NOT ACTIVATED, y añada la tabla cláusula de -01 →
  disposición bajo -02;
- actualizar contrato y evidencia solo de forma histórica; registrar aquí la CI de V2; actualizar las puntas paralelas; publicar y esperar
  la CI exacta;
- la V3 completa vuelve al **mismo** Architect, con ambos deltas, la disposición de A64-PV1-01..22, MASTER-I63-I64-02 y las mismas
  identidades del revisor. No implementar, sin Freeze ni GATE PASS. `IMPLEMENTATION AUTHORIZATION = NO`.

**Preflight (MEASURED):**
- 22:18:37Z: `HEAD` = remoto = `ab4efe86…`; árbol limpio; `origin/main` = `819955d6…`, sin cambio, así que no hubo rebase.
- Ningún proceso `acad` en ejecución al preparar este commit.

**Ramas paralelas** (por referencia remota, solo como coordinación):

| Rama | Punta | Avance desde §14 |
|---|---|---|
| I-52 | `d8078ef3` | sin cambio |
| I-62 | `acd88eaf` | dos commits: Proposal V4 y V5 con sus paquetes; solo documentos |
| I-63 | `a0654ae2` | un commit (20:32:56Z): cierre de P-05 por el Owner, sobre sus métricas; solo documentos |

Ninguna toca `src/`, `tests/` ni `assets/`.

**Hechos de código verificados para la V3** (MEASURED en la base `819955d6`):
- `RackBlockFinder.ScanEnvelopes` recorre las definiciones del `BlockTable`, omite layouts, anónimas y xref, devuelve también las
  definiciones cuyo sobre no se interpreta (sobre nulo) y, si se pide, su recuento de referencias directas tomado del registro, no del
  sobre. La política de validez queda en cada consumidor;
- `RackEnvelopeIdProbe` existe en Application para sondear el Id de un sobre no interpretable.

**Ejecución:** redacción directa de la sesión responsable, con lectura estática de código y documentos y ramas paralelas por referencia
remota. Sin AutoCAD, builds, pruebas, prototipos, subagentes, Controller ni Worker. Archivos versionados:
[Proposal V3](../../initiatives/I-64-proposal-v3.md) (nueva), [paquete de re-revisión V3](../../initiatives/I-64-architect-package-v3.md)
(nuevo), el contrato y esta evidencia. V1, V2 y sus paquetes no cambian.

**Estado:** Proposal V3 publicada para la re-revisión del mismo Architect y del Coordinator, sin consenso, Freeze ni GATE PASS.
`IMPLEMENTATION AUTHORIZATION = NO`.

## 16. F0 / Proposal V4 — Dictamen del Architect sobre V3 y hechos posteriores

Sección nueva; las anteriores no se reescriben.

**CI de la Proposal V3** (MEASURED con `gh run view`; evidencia de CI, no local): corrida **36934958555**, `event=push`,
`ref=refs/heads/architecture/workspace-persistente-rackcad`, `head_sha=1f1530be15a3bc95e260c03df07d51b9625345ae`. Jobs Tests (Domain +
Application), UI Tests (WPF controls, net8.0-windows), Build UI (WPF, valida API de Application) y Build Plugin without AutoCAD, los cuatro
`success`. Creada 22:25:22Z, actualizada 22:29:22Z. Blobs de V3: Proposal `d08c2cee75ee486bf5ed8f2ebebe6d2482aebbbe`, paquete
`45133653e5eeea6e6f7a7dd1f1493d4a48e3b90f`.

**Dictamen del mismo Architect sobre V3** (RECONSTRUCTED):
- **Procedencia.** La orden del Coordinator lo acepta y resume. El texto completo se leyó en la transcripción local de la misma sesión
  separada «I-64 Architect Review Proposal V1» (archivo de sesión `caf7ae64-ba5b-4648-ae0b-f7aeb9e7103a.jsonl`, mensaje del Architect de
  2026-10-01T22:43:29Z). El texto extraído mide 14 072 bytes UTF-8, con SHA-256
  `692fdd6c9b1884285f326e58ab172cd37455f8d03624a0768e1253c16336d214`. No se versiona aquí; se trata como dato.
- **Contenido**, en resumen saneado:
  - objeto: commit `1f1530be`, proposal blob `d08c2cee`, package blob `45133653`, CI 36934958555; V1 y V2 intactas;
  - el mismo revisor; SEPARATE SESSION; revisor ≠ autor; límite declarado: el mismo modelo que la sesión autora;
  - **A64-PV1-01..22 = CLOSED**, con evidencia por ID en V3;
  - **A64-PV3-01..06 = REQUIRED**: privacidad del índice frente a las métricas; F6 consumía estado de borradores y la evaluación de ID3;
    criterios de cierre de F3, F6 y F7 con evidencia de host; `BeginDocumentClose` frente a «solo encolan» y al modo degradado; sondeo
    usado como identidad, dos derivaciones de objetivos y «RackId curado»; Smoke-2 frente a §12.1;
  - O3-01..O3-07 OPTIONAL; riesgos R-01, R-02, R-08, R-09 y nuevos R-10 y R-11;
  - estado: CHANGES REQUIRED; ninguno exige autoridad reservada del Owner.

**Orden F0 / Proposal V4 del Coordinator**, recibida en el chat de la sesión el 2026-10-01. No llegó como archivo a `D:\IDs\I-64\` (listado
MEASURED a las 22:46Z), así que no tiene hash. Resumen saneado:
- acepta el dictamen sobre `1f1530be` / `d08c2cee` / `45133653`: A64-PV1-01..22 CLOSED, A64-PV3-01..06 REQUIRED, CHANGES REQUIRED;
- no reabrir el Discovery, no implementar, sin Freeze, sin AutoCAD ni GATE PASS; Proposal V4 autocontenida; V1, V2 y V3 intactas;
- disposiciones vinculantes:
  - **A64-PV3-01:** invariante verificable de privacidad del índice y de los hechos neutrales frente a métricas, `ProjectSummary`, BOM,
    `TotalRacks`, agregación y comandos ajenos; sin API pública; recuentos rotulados por navegación o selección; guarda de fuente o
    visibilidad y oráculo de UI o view-model;
  - **A64-PV3-02:** F6 entra con F2 + F3 y entrega inventario, Selección (N), navegación agregada y puntos de extensión de ID3, sin clasificar
    ID3; InventoryStatus (Normal, Diagnóstico, Sin identidad, No editable por captura) separado del SessionOverlay (Dirty, Posiblemente
    obsoleto, Conflicto, Error, Aplicando), que el índice no posee; clasificación de ID3 en READY tras F4/F5;
  - **A64-PV3-03:** sin evidencia de host como criterio de cierre del Worker; arneses automatizados (cambio de rack, de pestaña, 10
    pestañas, inventario grande sintético, selección grande sintética); hito del Owner PERF tras F6 y antes de READY; F7 consolida sin
    afirmar AutoCAD;
  - **A64-PV3-04:** manejadores ordinarios solo encolan; `BeginDocumentClose` decide el veto de forma síncrona con estado en memoria; el modo
    degradado conserva `BeginDocumentClose` mientras haya dirty o texto pendiente; INV-20 ampliado;
  - **A64-PV3-05:** el sondeo solo atribuye un diagnóstico; una única autoridad `NavigationTargets` para contexto, pestañas y navegador;
    «Id no vacío tal como figura en el sobre»;
  - **A64-PV3-06:** Smoke-1 tras F1 PASS; Smoke-NAV tras F2 PASS; Smoke-2 dentro de la secuencia de cierre de F4;
- adoptar O3-01, O3-02, O3-03 y O3-06; O3-04, O3-05 y O3-07 pueden seguir opcionales;
- conservar A64-PV1-01..22 = CLOSED sin reescribir el dictamen; tabla A64-PV3-01..06 → disposición → cláusula u oráculo;
- contrato y evidencia solo de forma histórica; registrar aquí la CI de V3; refrescar puntas y `main` antes de publicar; sin rebase si
  `main` no cambió; publicar y esperar la CI exacta;
- la V4 vuelve al **mismo** Architect; si todos sus REQUIRED cierran sin nuevos, Architect = AGREED, pero sin crear Freeze; el Coordinator
  revisa la misma V4 y ordena después el Consensus Freeze. `IMPLEMENTATION AUTHORIZATION = NO`.

**Preflight (MEASURED):**
- 22:46:27Z: `HEAD` = remoto = `1f1530be…`; árbol limpio; `origin/main` = `819955d6…`, sin cambio, así que no hubo rebase ni ventana de
  ROADMAP que coordinar.
- Ningún proceso `acad` en ejecución al preparar este commit.

**Ramas paralelas** (por referencia remota, solo como coordinación):

| Rama | Punta | Avance desde §15 |
|---|---|---|
| I-52 | `352cc3de` | un commit (22:25:15Z): ACL parte 1 aplicada (EXACT_MATCH), relecturas de pre-vuelo de S1-A y plan de ACL del hijo; 0 archivos de `src/`, `tests/` y `assets/` |
| I-62 | `3888c5c8` | dos commits: Proposal V5 y V6 con sus paquetes; solo documentos |
| I-63 | `772ac242` | un commit (22:32:25Z): su Proposal V1 y su paquete; solo documentos; su §16 se alinea con MASTER-I63-I64-02 (I-64 podrá presentar `ProjectSummary` como consumidor cuando esté integrado) |

**Hechos de código verificados para la V4** (MEASURED en la base `819955d6`):
- `RackSiblingScan.Traverse` abre su propia transacción (`RackSiblingScan.cs:103`): no sirve tal cual para releer dentro de la transacción
  de MUTATE (O3-02);
- `RackEnvelopeIdProbe` declara su ámbito como sondeo de Id para diagnósticos de pertenencia, sin tratar un Id anidado como identidad de
  rack (`RackEnvelopeIdProbe.cs:6`).

**Ejecución:** redacción directa de la sesión responsable, con lectura estática de código y documentos, ramas paralelas por referencia
remota y la transcripción local citada. Sin AutoCAD, builds, pruebas, prototipos, subagentes, Controller ni Worker. Archivos versionados:
[Proposal V4](../../initiatives/I-64-proposal-v4.md) (nueva), [paquete de re-revisión V4](../../initiatives/I-64-architect-package-v4.md)
(nuevo), el contrato y esta evidencia. V1, V2, V3 y sus paquetes no cambian.

**Estado:** Proposal V4 publicada para la re-revisión del mismo Architect y la revisión posterior del Coordinator, sin consenso, Freeze ni
GATE PASS. `IMPLEMENTATION AUTHORIZATION = NO`.

## 17. F0 / Proposal V5 — Dictamen del Architect sobre V4 y hechos posteriores

Sección nueva; las anteriores no se reescriben.

**CI de la Proposal V4** (MEASURED con `gh run view`; evidencia de CI, no local): corrida **36937865502**, `event=push`,
`ref=refs/heads/architecture/workspace-persistente-rackcad`, `head_sha=e87ba32cf4132cd29677c49cf208d4ca6fb9ea49`. Jobs Tests (Domain +
Application), UI Tests (WPF controls, net8.0-windows), Build UI (WPF, valida API de Application) y Build Plugin without AutoCAD, los cuatro
`success`. Creada 22:54:55Z, actualizada 22:59:00Z. Blobs de V4: Proposal `ef3aa9994bf0028aa1966761ccdfb4dcf42b3c56`, paquete
`580beba108911a97bcc6a33db5bd2d025d5a2388`.

**Dictamen del mismo Architect sobre V4** (RECONSTRUCTED):
- **Procedencia.** La orden del Coordinator lo acepta y resume. El texto completo se leyó en la transcripción local de la misma sesión
  separada «I-64 Architect Review Proposal V1» (archivo de sesión `caf7ae64-ba5b-4648-ae0b-f7aeb9e7103a.jsonl`, mensaje del Architect de
  2026-10-01T23:08:45Z). El texto extraído mide 9 065 bytes UTF-8, con SHA-256
  `776ea95195ff6a5a3e1a79d8c29e487383d0ea86cfc796f8146a4231f39883ee`. No se versiona aquí; se trata como dato.
- **Contenido**, en resumen saneado:
  - objeto: commit `e87ba32c`, proposal blob `ef3aa999`, package blob `580beba1`, CI 36937865502; V1, V2 y V3 intactas;
  - el mismo revisor; SEPARATE SESSION; revisor ≠ autor; límite declarado: el mismo modelo que la sesión autora;
  - A64-PV3-01, 02, 03, 05 y 06 = **CLOSED**; V4 no reabre A64-PV1-01..22 ni reintroduce dependencias de I-63;
  - **A64-PV3-04 = STILL OPEN:** en modo degradado, un borrador que se ensucia después de degradar quedaba sin veto, porque
    `BeginDocumentClose` se conservaba solo «mientras» hubiera dirty; el oráculo no probaba el orden degradar → editar → cerrar;
  - **A64-PV4-01 = REQUIRED:** el PASS de Smoke-PERF, condición de entrada de READY, no era verificable; faltaba qué hacer si el
    rendimiento no es aceptable con F7 cerrado; el momento era ambiguo;
  - O4-01..O4-06 OPTIONAL; siguen O3-04, O3-05 y O3-07;
  - estado: CHANGES REQUIRED; ninguno exige autoridad reservada del Owner.

**Orden F0 / Proposal V5 del Coordinator**, recibida en el chat de la sesión el 2026-10-01. No llegó como archivo a `D:\IDs\I-64\`, así
que no tiene hash. Resumen saneado:
- acepta el dictamen sobre `e87ba32c` / `ef3aa999` / `580beba1`;
- no reabrir el Discovery, no implementar, sin Freeze, sin AutoCAD ni GATE PASS; Proposal V5 autocontenida; V1..V4 intactas;
- **A64-PV3-04 (regla vinculante):** una vez creada una sesión de documento, `BeginDocumentClose` permanece suscrito toda su vida, también
  en modo degradado, sin depender del dirty al degradar; el manejador solo lee estado en memoria, decide el veto y emite un aviso no modal
  permitido; no lee la base de datos, no resuelve, no escribe ni abre diálogos; con dirty o texto pendiente en el instante del cierre:
  VETO; INV-20 ampliado con sesión limpia → modo degradado → edición posterior → dirty → cierre → veto;
- **A64-PV4-01:** Smoke-PERF PASS = SP-01..SP-05 completos y válidos + SP-06 PASS + aceptación explícita del Owner; sin umbrales antes de la
  línea base; sin aceptación: NOT PASS y STOP antes de READY, con A-n del Coordinator (umbral cuantitativo o gate correctivo) y repetición
  de la evidencia afectada; momento exacto: F7 PASS → Smoke-PERF → si PASS → READY; eliminar toda frase ambigua «tras F6»;
- adoptar O4-01, O4-02, O4-05 y O4-06;
- conservar A64-PV1-01..22 y A64-PV3-01, 02, 03, 05, 06 como CLOSED; tabla finding → disposición → cláusula V5 → oráculo;
- contrato y evidencia solo de forma histórica; registrar aquí la CI de V4; refrescar puntas y `main` antes de publicar; sin rebase si
  `main` no cambió; publicar y esperar la CI exacta;
- la V5 vuelve al **mismo** Architect, con el formato «A64-PV3-04 / A64-PV4-01 = CLOSED | STILL OPEN», hallazgos nuevos y CONSENSUS STATUS;
  si ambos cierran sin REQUIRED nuevos, Architect = AGREED, pero sin Freeze, implementación ni GATE PASS; el Coordinator revisa después la
  misma V5 y ordena el Consensus Freeze. `IMPLEMENTATION AUTHORIZATION = NO`.

**Preflight (MEASURED):**
- 23:16:25Z: `HEAD` = remoto = `e87ba32c…`; árbol limpio; `origin/main` = `819955d6…`, sin cambio, así que no hubo rebase ni ventana de
  ROADMAP que coordinar.
- Ningún proceso `acad` en ejecución al preparar este commit.

**Ramas paralelas** (por referencia remota, solo como coordinación):

| Rama | Punta | Avance desde §16 |
|---|---|---|
| I-52 | `51a66245` | dos commits: lectura de pre-vuelo exacta de sesión para S1-A (§252) y autorización de S1-A detenida antes de AutoCAD (§253); 0 archivos de `src/`, `tests/` y `assets/` |
| I-62 | `3d7ec77f` | un commit: su Proposal V7 y su paquete; solo documentos |
| I-63 | `dddc215f` | un commit: su Proposal V2 y su paquete de re-revisión; solo documentos; su §16 sigue alineado con MASTER-I63-I64-02 |

**Ejecución:** redacción directa de la sesión responsable, con lectura estática de documentos, ramas paralelas por referencia remota y la
transcripción local citada. Sin AutoCAD, builds, pruebas, prototipos, subagentes, Controller ni Worker. Archivos versionados:
[Proposal V5](../../initiatives/I-64-proposal-v5.md) (nueva), [paquete de re-revisión V5](../../initiatives/I-64-architect-package-v5.md)
(nuevo), el contrato y esta evidencia. V1..V4 y sus paquetes no cambian.

**Estado:** Proposal V5 publicada para la re-revisión del mismo Architect y la revisión posterior del Coordinator, sin consenso, Freeze ni
GATE PASS. `IMPLEMENTATION AUTHORIZATION = NO`.

## 18. Consensus Freeze (I64-CONSENSUS-V5-01) y cierre de F0

Sección nueva; las anteriores no se reescriben.

**Orden del Coordinator**, recibida en el chat de la sesión el 2026-10-01 (sin archivo en `D:\IDs\I-64\`, así que sin hash). Resumen
saneado:
- Proposal V5 acordada: commit `9e3d289a9b61339c162b2f791f6995a584207800`, blob `bd4e96264e538ffc679bfd9f088400b22b553931`; paquete blob
  `faf516c807f0a712a694a3cfc36ad51390fb9115`; CI 36940271314 (`push`, `head_sha` exacto, 4/4 `success`);
- **Architect sobre V5 = AGREED**; **Coordinator sobre V5 = AGREED**; REQUIRED abiertos: 0; Consensus **REACHED**, ID
  **`I64-CONSENSUS-V5-01`**; Freeze autorizado, pendiente de publicar y verificar;
- preflight real; STOP al Coordinator si `origin/main` cambió respecto de `819955d6`, sin rebase automático antes del Freeze;
- commit de Freeze que toque solo `docs/initiatives/I-64-proposal-v5.md` y solo con tres reemplazos literales de cabecera; verificación
  mecánica; push fast-forward; CI exacta del Freeze antes de declararlo verificado;
- después, un commit separado, sin tocar la Proposal congelada, que registre Freeze, CI, Consensus, F0 COMPLETE, `freeze_ref`, estado y
  la siguiente acción persistente; regla operativa del Owner: relay automático entre agentes cuando esté autorizado, escalado de autoridad
  al Owner, y registro de `AUTONOMY_GAP` cuando un rol no pueda invocarse; preparar F1 sin implementarlo sin contrato de gate autorizado.

**Preflight** (MEASURED, 23:36:55Z):
- `origin/main` = `819955d61a6da4c811a11fbd11b5dca13f634b7c`, sin cambio: no hubo STOP ni rebase;
- `HEAD` = `origin/architecture/workspace-persistente-rackcad` = `9e3d289a9b61339c162b2f791f6995a584207800`;
- árbol limpio; ninguna operación Git en curso (sin `MERGE_HEAD`, `REBASE_HEAD`, `CHERRY_PICK_HEAD`, `REVERT_HEAD`, `BISECT_LOG`,
  `rebase-merge` ni `rebase-apply`);
- las tres cadenas de origen aparecían una sola vez, en las líneas 4, 16 y 17 de la cabecera.

**Commit de Freeze** (MEASURED):

| Hecho | Valor |
|---|---|
| `FREEZE_SHA` | `9b43dafbe3b8874aa0ba5a85d62cbf953dbdd311` |
| Padre único | `9e3d289a9b61339c162b2f791f6995a584207800` |
| Rutas cambiadas | solo `docs/initiatives/I-64-proposal-v5.md` (3 líneas añadidas y 3 eliminadas) |
| Blob anterior | `bd4e96264e538ffc679bfd9f088400b22b553931` |
| Blob congelado | `dc1924ff5a73a4e529fbc5f995621d29351a091b` (sin CR) |
| Comprobación | aplicar los tres reemplazos literales al blob anterior reproduce exactamente el blob congelado |
| Push | fast-forward `9e3d289a..9b43dafb`, sin force |

**CI del Freeze** (MEASURED con `gh run view`): corrida **36941847270**, `event=push`, `headBranch=architecture/workspace-persistente-rackcad`,
`head_sha=9b43dafbe3b8874aa0ba5a85d62cbf953dbdd311`. Jobs Tests (Domain + Application), UI Tests (WPF controls, net8.0-windows), Build UI
(WPF, valida API de Application) y Build Plugin without AutoCAD, los cuatro `success`. Creada 23:38:13Z, actualizada 23:41:29Z.

**Resultado:** Freeze **VERIFICADO**. **F0 = COMPLETE.** F1 = NOT STARTED. Desde aquí el Freeze es inmutable y todo cambio va por A-n.

**Estado posterior** (este commit):
- [estado canónico](../state/I-64.yml), con la siguiente acción persistente (`NEXT_ROLE` … `OWNER_REQUIRED_IF`) y tres `AUTONOMY_GAP`;
- [propuesta de contratos de gate de F1](../../initiatives/I-64-f1-gate-contract-proposal.md), no emitida;
- contrato: estado `f1-pending-gate-contract`, `freeze_ref`, `ov_assignment_ref` y `automation_state_path`.

**Participantes y autonomía** (MEASURED el 2026-10-01):
- No se invocó a ningún participante: el Freeze lo hizo la sesión responsable por orden directa, y la siguiente acción delegable (la
  planificación del Controller) necesita antes un contrato de gate que emite el Coordinator (AUTOMATION_PLAN 16.5; agent-execution/README §1).
- Capacidad presente: binario de Codex en `%LOCALAPPDATA%\OpenAI\Codex\bin\` y celda del Controller `gpt-6-luna`/`high` medida (catálogo);
  Worker subagente `claude-sonnet-5-5`/`medium`-`high` con escritura medida. No se tomó todavía la línea base de `config.toml` (se toma en la
  salida del primer relevo; el archivo se modificó por última vez a las 23:09Z, fuera de esta sesión).
- `ListAgents` no muestra ninguna sesión Coordinator; sí la del Architect de I-64, que no hace falta en este paso.
- `AUTONOMY_GAP` registrados en el estado: Coordinator (emisión de contratos, sin sesión direccionable), Worker Long-horizon (sin celda
  Frontera con escritura medida) y Worker Routine (sin celda Eficiente elegible).
- El Freeze no fija el tope de invocaciones de 16.8; la propuesta de F1 pide que lo fije el contrato de gate.

**Ramas paralelas** (por referencia remota, 23:42:24Z):

| Rama | Punta | Avance desde §17 |
|---|---|---|
| I-52 | `51a66245` | sin cambio |
| I-62 | `d66463a5` | un commit: su Proposal V8 y su paquete; solo documentos |
| I-63 | `fb3c8788` | un commit: su Proposal V3 y su paquete de re-revisión; solo documentos |

A las 23:42:24Z había **un proceso `acad` en ejecución**, no iniciado ni tocado por esta sesión. I-64 no usa AutoCAD y respeta la
restricción de host de I-52 (§12.1 del Freeze).

**Ejecución:** sesión responsable, sin AutoCAD, builds, pruebas, subagentes, Controller ni Worker. Este commit no toca la Proposal
congelada.

## 19. F1-T0-ADR — Contrato recibido, preflight, relevo de salida y STOP antes de la planificación

Sección nueva; las anteriores no se reescriben.

**Contrato de gate recibido** (orden del Coordinator en el chat de la sesión, 2026-10-02; sin archivo, así que sin hash). Resumen saneado:
- Gate F1 — Workspace Foundation; `TaskId` **F1-T0-ADR**; `AuthorityRevision` `5570b9249e6398efdea826bb03fba43c6ad484d2`; `MainSha`
  `819955d61a6da4c811a11fbd11b5dca13f634b7c`;
- objetivo: crear solo `docs/adr/0047-workspace-persistente-modeless-rackcad.md`, en estado `propuesto`, como proyección fiel del
  anexo A del Freeze;
- `AllowedWriteScope`: ese archivo; prohibido todo lo demás;
- invariantes F1-T0-01..F1-T0-08 (un solo archivo; ADR `propuesto` y subordinado al Freeze; nada que no venga de la Proposal V5;
  MASTER-I63-I64-02; Actualizar como única frontera authored; RACKEDITAR fuera; sin AutoCAD, DLL, perfil, `TRUSTEDPATHS`, `SECURELOAD`, ACL
  ni registro; sin opcionales no incorporados);
- clase Documentación / Routine; Worker `claude-sonnet-5-5` × subagente × `medium`, con escalado de nivel autorizado y
  `ModelEscalationReason` literal; Controller Codex CLI `gpt-6-luna` × `high`; `RoutingEnforcement` `required`; `CorrectionsAuthorized` `true`;
- tope: 1 planificación, 4 trabajos, 4 verificaciones, `MaxReworkLoops` 3; los controles negativos fuera del tope;
- STOP adicionales: avance de `main`, ADR 0047 ocupado, necesidad de cambiar el Freeze, AutoCAD, producto, ampliar el alcance, cambio de
  `config.toml`, conflicto de propiedad, autoridad ausente y presupuesto agotado;
- la aceptación A1-A8 la hace el Coordinator; si I-61 la reserva, la sesión se detiene en `COORDINATOR_ACCEPTANCE_REQUIRED`.

**Preflight** (MEASURED, 2026-10-02T00:01:58Z):
- `origin/main` = `819955d6…`; `HEAD` = `origin/…` = `git ls-remote` = `5570b9249e6398efdea826bb03fba43c6ad484d2`;
- árbol limpio; ninguna operación Git en curso;
- ADR 0047 libre: ninguna referencia remota contiene `docs/adr/0047*` (el último ADR en la base es 0046);
- Proposal congelada intacta: blob `dc1924ff5a73a4e529fbc5f995621d29351a091b`.

**Relevo de salida** (MEASURED desde PowerShell, 2026-10-02T00:03:20Z; AUTOMATION_PLAN 16.4 paso 1, README §3.1-§3.2):
- `~/.codex/config.toml`: SHA-256 `40C27B570B0056BC6D2B5AAF460628922C5AD39A405751E68B9001FFDF15F74F`, última escritura 2026-10-01T23:09:21Z, 103
  nombres de secciones y claves registrados sin valores en el área transitoria ignorada
  (`artifacts/orchestration/I-64/F1-T0-ADR/0/preflight-20261002T0003Z/config-baseline.json`);
- procesos: ningún `participant` con la ruta del worktree; ningún `unattributable`; tres procesos `codex*` del Owner sin la ruta del worktree
  (`owner-app`); un `acad.exe` (PID 26000, desde 23:39:52Z) no iniciado ni tocado por esta sesión.

**STOP antes de la planificación del Controller** (no se invocó a ningún participante):
- **C-F0-RED aplica a F1-T0-ADR.** AUTOMATION_PLAN 16.8 exige RED en toda entrega cuyo `ChainRedSha` sea `null`, incluida la primera
  delegación de cualquier tarea, y 16.9 lo comprueba en `Ci` y `Tests` (`RedPart` = `fail` → REWORK). Una tarea solo documental, cuyo
  alcance prohíbe `tests/` y `src/`, no puede producir un RED legítimo, y fabricarlo está excluido. La cadena no puede llegar a
  `EXECUTION_VERIFIED`: el protocolo forzaría correcciones hasta S-11. Es la misma limitación que I-63 registró y su Coordinator aceptó como no
  corregida (decisiones de I-63, CD-I63-F0-01, leídas por referencia remota).
- **Contrato incompleto respecto del esquema** `rackcad-gate-contract/v1`: la orden no enumera `Authorities` (ruta, sección y clase para
  16.3 y la comprobación `Authority`), `RequiredTests`, `ExpectedEvidence` ni `IssuedUtc`. La sesión no los inventa.
- Invocar ahora la única planificación del presupuesto la gastaría en una cadena que no puede cerrarse. Resolver un STOP corresponde al
  Coordinator (16.11; README §1): **AUTONOMY_GAP-COORDINATOR**.

**Opciones para el Coordinator** (la sesión no elige):
- (a) Autorizar F1-T0-ADR como **trabajo directo** de la sesión responsable, sin delegación §16 ni `EXECUTION_VERIFIED` (el precedente de
  I-63 para su Discovery), con revisión del Coordinator sobre el diff y la CI exacta. No modifica AUTOMATION_PLAN.
- (b) Reemitir: incluir el ADR en el alcance de la primera tarea de F1 que sí tiene pruebas (F1-T1-MODEL), de modo que el RED de la cadena
  venga de sus pruebas y el ADR se publique en el primer commit de esa cadena, antes de cualquier código.
- (c) Mantener la delegación documental: no es viable sin cambiar AUTOMATION_PLAN 16.8, que es autoridad de I-61, fuera del alcance de I-64.

Con (a) o (b), el Coordinator completa también `Authorities`, `RequiredTests`, `ExpectedEvidence` e `IssuedUtc` del contrato que emita.

**Presupuesto:** intacto (0 planificaciones, 0 trabajos, 0 verificaciones; `attempts` 0).

## 20. F1-T1-MODEL — Resolución de C-F0-RED y contrato de la primera cadena de F1

Sección nueva; las anteriores no se reescriben.

**Decisión del Coordinator** (orden en el chat de la sesión, 2026-10-02, sin archivo y por tanto sin hash; resumen saneado):
- adopta la opción (b) de §19: **F1-T0-ADR = SUPERSEDED_BY_F1-T1-MODEL** y **C-F0-RED = RESOLVED_BY_TASK_MERGE**. No se modifica I-61 ni
  AUTOMATION_PLAN. El RED de la cadena sale de pruebas reales del modelo de F1-T1 y no se fabrica para una tarea documental;
- F1-T0-ADR no consumió presupuesto (0 planificaciones, 0 trabajos, 0 verificaciones).

**Contrato de gate emitido para `F1-T1-MODEL`** (`rackcad-gate-contract/v1`; unidad I-64; gate F1):
- `AuthorityRevision` `8a3fb8e26a260317b28d5573654b0221f1341029`; `MainSha` `819955d61a6da4c811a11fbd11b5dca13f634b7c`;
- objetivo: el ADR 0047 en estado `propuesto`, fiel al Freeze y publicado antes del GREEN; el modelo puro de Workspace en
  `RackCad.Application` (sesión por documento, identidad de instancia, `SelectionContext`, estado mínimo de pistas y cola, reglas puras del
  drenaje y políticas puras de F1 sin AutoCAD); RED→GREEN real. Fuera: `PaletteSet`, host WPF, puente real de AutoCAD, selección real,
  navegación, escritura authored y AutoCAD;
- autoridades `UNIT_DOC`: Proposal V5 congelada, Discovery D1-R1, contrato, evidencia §§18-19, estado y la propuesta de contratos de F1
  (subordinada al contrato); `EXTERNAL`: AGENTS, WORKFLOW, LIFECYCLE, AUTOMATION_PLAN §16, README y `routing.md` de agent-execution,
  ADR-0006, ADR-0019, ADR-0029, ADR-0044 e I-55 V5 §4.4-§4.10 cuando aplique; sin `UNIT_CHANGE`;
- `AllowedWriteScope`: `docs/adr/0047-workspace-persistente-modeless-rackcad.md`, `src/RackCad.Application/Workspace/`,
  `tests/RackCad.Tests/Workspace/`; prohibido todo lo demás;
- invariantes INV-F1-T1-01..12; prueba requerida `tests/RackCad.Tests/RackCad.Tests.csproj`, filtro `FullyQualifiedName~RackCad.Tests.Workspace`,
  `MinSelected` 1, `ExpectRed` true (RED real por aserción, sin fallo de compilación ni prueba artificial);
- Controller Codex CLI `gpt-6-luna` × `high`; Worker `claude-sonnet-5-5` × subagente × `medium` (Balanced); `RoutingEnforcement` `required`;
  `CorrectionsAuthorized` `true`; `MaxReworkLoops` 3;
- tope: 1 planificación, 4 trabajos, 4 verificaciones; los controles negativos fuera del tope solo donde I-61 lo establece;
- S-01..S-14, P-01..P-08 y los STOP adicionales del contrato;
- la aceptación A1-A8 es formal del Coordinator: si la delegación real pasa A1-A8 de forma mecánica, la sesión se detiene en
  `COORDINATOR_ACCEPTANCE_REQUIRED` sin invocar al Worker.

**Transcripción operativa:** la sesión transcribe el contrato a `gate-contract.json` en el área transitoria del `RunId` de planificación.
Dos campos que el esquema exige y que la orden no da con forma literal se fijan así, sin añadir alcance: `IssuedUtc` = hora de recepción
en la sesión, e `IssuedBy` = el Coordinator de I-64. Las rutas `docs/adr/00NN-*.md` del contrato se resuelven a su único archivo en la base.

**Presupuesto de `F1-T1-MODEL`:** planificación 0 de 1; trabajo 0 de 4; verificación 0 de 4; `attempts` 0 de 3.

## 21. F1-T1-MODEL — Planificación del Controller, nc4, A1-A8 mecánicas y STOP P-03

Sección nueva; las anteriores no se reescriben. Hechos de la sesión responsable. No hay aceptación, GATE PASS, F1 PASS ni ADR aceptado.
No se invocó al Worker.

**Relevo de salida (16.4, paso 1).** Commit de estado `0a3fec301a18ef38646729e20b8a276f80feb36e`.
- HEAD = `ls-remote` = `0a3fec30`; `origin/main` = `819955d61a6da4c811a11fbd11b5dca13f634b7c`; árbol limpio; sin operación de Git en curso.
- `config.toml`: SHA-256 `40C27B57…F15F74F`, igual que `relay_baseline`.
- Procesos (PowerShell): sin participantes y sin procesos no atribuibles.
- ADR 0047 libre en HEAD y en `origin/main`.
- Rama y worktree: los de la unidad.

**Planificación** (`RunId` `R20261002T001349Z-aad5`; intento 0; fase PLANNING):
- Invocación: Codex CLI 0.159.2, `-s read-only`, `gpt-6-luna` × `high`, con `--output-schema delegation.schema.json`.
  - Modelo y effort efectivos: `gpt-6-luna` / `high` (`turn_context` del registro de sesión; `thread_id` `01a0f9f8-348e-74f0-8646-edcdf8af1a22`;
    SHA-256 del registro `6DDC0638…26E49150FC3EF`).
- Cesión 00:16:27Z → 00:18:51Z, sin operación de la sesión.
- Fin: `turn.completed`, código 0, muerte del lanzador (PID Windows 26504) confirmada.
- Salida: SHA-256 `B3B081BC…CD8D9430788F`, escrita a las 00:18:51.0454129Z; `delegation.json` es su copia byte a byte.
- Uso: 438001 tokens de entrada (367360 en caché) y 9407 de salida (2970 de razonamiento).
- Ejecución: 9 órdenes, una con código distinto de 0 (pwsh en modo de lenguaje restringido; el Controller la rodeó).
- Coincidencias de patrones de límite: 4, todas contenido de archivos leídos. P-06 no aplica.
- La delegación propone:
  - Worker `claude-sonnet-5-5|subagent`, `Balanced` / `medium`, sin escalado, `ROUTINE_IMPLEMENTATION`;
  - `BaseSha` = `ChainBaseSha` = `0a3fec30`; `ChainRedSha` null; `ChainRedFiles` vacío;
  - `AllowedWriteScope` de archivos exactos: el ADR 0047, 8 archivos en `src/RackCad.Application/Workspace/` y 6 de pruebas en
    `tests/RackCad.Tests/Workspace/`;
  - 7 criterios de aceptación.

**Relevo de entrada.**
- HEAD = `ls-remote` = `0a3fec30`; árbol limpio; `config.toml` sin cambios; sin participantes vivos.
- Dos procesos no atribuibles: `bash.exe` 31724 y `git.exe` 38412, con la línea de órdenes ilegible.
  - En la lectura de detalle y en la relectura ya no existían, así que no se pudieron medir el padre ni la hora de creación.
  - Por la letra de 16.4 esto es **P-02**.
  - El criterio de relectura de DEV-G3-02 lo decidió el Coordinator de I-61 para su G3; aquí no se aplica sin una decisión del Coordinator
    de I-64.
- Desviación declarada (DEV-F1T1-01): el registro de procesos de la salida solo midió las clases `participant` y `unattributable`, y no
  listó `own-tree`, `owner-app` ni `nominal-exclusion`.

**A1-A8, evaluación mecánica sin cortocircuito.** La aceptación formal es del Coordinator. Resultado:

| | A1 | A2 | A3 | A4 | A5 | A6 | A7 | A8 |
|---|---|---|---|---|---|---|---|---|
| delegación real | pass | pass | pass | **fail** | **fail** | pass | pass | pass |
| nc4 | pass | pass | **fail** | fail | fail | pass | pass | pass |

- **A4.** Faltan 497 de las 593 entradas prohibidas.
  - El `gate-contract.json` transcrito por la sesión expandió «fuera de Workspace/» y «cualquier otro docs/adr/» en 593 rutas, con
    `git ls-tree` sobre la base.
  - El Controller copió 97 entradas:
    - `docs/initiatives/`, `docs/automation/`, Plugin, UI, Domain y UI.Tests;
    - las 19 de `src/RackCad.Application/`;
    - 49 de `docs/adr/`;
    - 23 de las 512 de `tests/RackCad.Tests/`.
  - Omitió 489 entradas de `tests/RackCad.Tests/` y 8 de raíz: `.github/`, `assets/`, `eng/`, `deploy/`, `global.json`,
    `Directory.Build.props`, `Directory.Build.targets` y `RackCad.sln`.
  - El alcance permitido de la delegación (A3) sí está dentro del contrato. El hueco es de copia, no de ampliación.
- **A5, invariantes.** Faltan 5 de 12 (INV-F1-T1-02, 08, 09, 10 y 11).
  - Causa raíz propia: la transcripción de la sesión pasó los invariantes del Coordinator a ASCII.
  - El Controller les devolvió las tildes y la eñe («sesión», «título», «pestaña», «destrucción», «métrica», «añade», «semántica»).
  - El contenido es idéntico salvo los diacríticos, pero A5 compara texto exacto.
- **A5, autoridades.** Faltan 3 de 24, con el encabezado de sección alterado:
  - evidencia §19, con «-» en lugar de «—»;
  - I-55 V5 §4.4 y §4.6, con «transacción» y «colocación», mientras que los encabezados reales no llevan tilde.
  - Añade, como estaba permitido, `docs/adr/README.md` y `docs/adr/plantilla.md`.
- **A5, resto.** Las 34 paradas y la prueba requerida están completas.
- **Defecto de contenido no cubierto por A1-A8.** `TaskClass` = «Controller: planificación / verificación», que es la clase del Controller y
  no la del trabajo del Worker.
- **nc4** (`RunId` `R20261002T002047Z-f0b0`). Copia de la delegación real en la que solo cambia `AllowedWriteScope` (+ `src/RackCad.Plugin/`).
  - A3 queda en fail; las demás coinciden con la real.
  - Se cumple el oráculo relativo.
  - Resultado: `REJECTED_BEFORE_INVOCATION`, STOP (P-03) confinado al control y fuera de todo tope.

**Disposición (tabla de 16.5): STOP P-03**, «delegación fuera del contrato». En este punto no aplica `COORDINATOR_ACCEPTANCE_REQUIRED`,
porque la delegación real no pasa A1-A8.

**Presupuesto.**
- Planificación: **1 de 1, consumida**. Trabajo: 0 de 4. Verificación: 0 de 4.
- `attempts`: 0 de 3. BLOCKED: 0.
- Replanificar excede el tope (P-07) mientras el Coordinator no lo decida.

**Opciones para el Coordinator.** Son de la sesión y no vinculan.
1. **Reemitir el contrato con los invariantes literales del Coordinator**, con tildes y tal como los dio en el chat.
2. **Prohibiciones compactas.** `ForbiddenWriteScope` como prefijos de primer nivel (`docs/initiatives/`, `docs/automation/`,
   `src/RackCad.Plugin/`, `src/RackCad.UI/`, `src/RackCad.Domain/`, `tests/RackCad.UI.Tests/`, `.github/`, `assets/`, `eng/`, `deploy/`,
   `global.json`, `Directory.Build.*`, solución).
   - «Fuera de Workspace/» y «otros docs/adr/» quedan cubiertos por A3 y por la comprobación de alcance de 16.9, que exige
     `diff ⊆ AllowedWriteScope`.
   - Así A4 no depende de que el Controller copie cientos de rutas.
3. **Prompt de planificación con copia byte a byte.** Invariantes, `Section` de las autoridades, paradas y prohibidas se copian sin
   normalizar acentos, guiones ni tildes; `TaskClass` es la clase del trabajo del Worker.
4. **Autorizar una segunda planificación** (P-07 resuelto por el Coordinator), con un `RunId` nuevo, sobre la base vigente.
5. **Decidir el P-02 efímero de la entrada.** Las opciones son adoptar para I-64 el criterio de relectura de DEV-G3-02 o tratarlo como STOP
   propio.

**Custodia (16.12)** en `docs/automation/evidence/I-64-pilot/`:
- `F1-T1-MODEL/R20261002T001349Z-aad5/`: `gate-contract.json`, `prompt.md`, `delegation.json`, `relay-record.json`, `acceptance-precheck.json`
  y `acceptance-precheck-reasons.json`;
- `F1-T1-MODEL-nc4/R20261002T002047Z-f0b0/`: `delegation.json`, `nc4-result.json` y `relay-record.json`.

`events.jsonl` (SHA-256 `C31DF251…E90D5CAD07`), `stderr.txt` y el registro de sesión de Codex no se versionan; quedan como procedencia.
`acceptance-precheck-reasons.json` y `nc4-result.json` se escribieron con CRLF en el área transitoria, así que su SHA-256 transitorio difiere
del blob, normalizado a LF por `core.autocrlf`. La identidad duradera es el blob del commit de custodia.

## 22. F1-T1-MODEL — Segunda planificación, aceptación, Worker, verificación REWORK y STOP P-07

Sección nueva; las anteriores no se reescriben. Hechos de la sesión responsable. Sin GATE PASS, F1 PASS ni ADR aceptado.

**Decisiones del Coordinator** (órdenes en el chat de la sesión, 2026-10-02; sin archivo y por tanto sin hash; resumen saneado):
1. Sobre el STOP de §21:
   - **P-03 = CONFIRMED.** La delegación `R20261002T001349Z-aad5` queda rechazada y nunca se entrega al Worker.
   - **P-07.** Autoriza una segunda planificación sustitutiva. El tope de planificación pasa a **2 en total**; trabajo 4, verificación 4 y
     `MaxReworkLoops` 3 no cambian.
   - **P-02.** `bash.exe` 31724 y `git.exe` 38412 quedan **CLEARED_BY_REREAD**. Regla para I-64: un proceso no atribuible es STOP P-02 solo
     si reaparece de forma persistente o toca Git, `config.toml` o el worktree.
   - **Empaquetado.** Prohibidas sin expandir; siete colecciones opacas copiadas byte a byte; `TaskClass` del trabajo del Worker;
     BaseSha `e0587355`; MainSha `819955d6`; nc4 repetido sobre la segunda planificación.
2. **Aceptación formal.**
   - A1-A8 = PASS sobre `R20261002T003827Z-3a76` (`delegation.json` SHA-256 `14058EA3…3FDE1A`): **ACCEPTED**.
   - nc4 `R20261002T004303Z-8d3c` aceptado como control negativo válido.
   - **AuthorityRevision `e0587355` ACCEPTED.**
   - La colección completa de `Invariants[]` sigue siendo vinculante aunque `AcceptanceCriteria[]` sea menos exhaustiva.
   - Worker autorizado. Cadena autónoma: Worker → RED → GREEN → push/CI → verificación → REWORK autorizados.

**Contrato reemitido** (SHA-256 `A4D36818…991999D2`; transcripción literal por la sesión; mismo contrato material):
- Invariantes, paradas C-01..C-12, objetivo y evidencia esperada: texto de la orden original, con sus tildes.
- S-01..S-14 y P-01..P-08: de la tabla de AUTOMATION_PLAN §16 en MainSha.
- `ForbiddenWriteScope`: 17 prefijos y rutas canónicos, sin expandir. «Fuera de Workspace/» y «otros docs/adr/» quedan cubiertos por
  `AllowedWriteScope` y por `Scope` de 16.9 (precedente de I-61).
- 24 autoridades con los encabezados comprobados byte a byte en Git.
- **Supuesto aceptado:** AuthorityRevision = BaseSha = `e0587355`. 16.4 paso 1 no exigía commit, porque `e0587355` lleva el resumen de
  estado, y el paso 3 prohíbe commit entre planificación y aceptación.

**Segunda planificación** (`R20261002T003827Z-3a76`):
- Ejecución: Codex `gpt-6-luna` / `high`, efectivo comprobado (`thread_id` `01a0fa0d-…`); código 0; `turn.completed`.
- Salida: `delegation.json` = salida `-o`, SHA-256 `14058EA3…`.
- Las siete colecciones y `Objective` son iguales byte a byte al contrato.
- `TaskClass`: «Implementación de pruebas» (ROUTINE_IMPLEMENTATION, Balanced). Worker `claude-sonnet-5-5|subagent` / `medium`, sin escalado.
- A1-A8: las ocho en pass.
- **nc4** `R20261002T004303Z-8d3c`: A3 en fail y las demás iguales; `REJECTED_BEFORE_INVOCATION`.
- Relevos de salida y de entrada limpios.

**Worker** (`R20261002T005244Z-f656`; agent() de workflow `wf_b6d3c36c-5e2`, alias `sonnet`, effort `medium`):
- Prompt: §G.1 + ROUTINE_IMPLEMENTATION + delta; 182 líneas; SHA-256 `BBB6C336…25EAE8`. El texto entregado, quitada la línea de marco y la
  sangría, es igual al archivo: sin P-05. El harness añadió un turno previo con la orden del Coordinator, como contexto.
- Cesión 00:55:00Z → 01:08:00Z; 705238 ms; 27 llamadas de herramienta.
- Efectivos: `claude-sonnet-5-5` / `medium` en las 47 entradas de la transcripción (SHA-256 `3903E0B5…1FAA1A`).
- Commits, ambos con `Co-Authored-By: Claude Sonnet 5.5`:
  - RED `36c344dc283d4a9855110a31ca6b20a2da7fd972`: ADR 0047 `propuesto`, esqueleto y 37 pruebas;
  - GREEN `66d34af31524c49214e38479d5892cd9ec985eb9`: los 5 archivos de producción, sin tocar pruebas.
- Diff: 10 archivos dentro de `AllowedWriteScope`.
- Entrega: `WorkerStatus` BLOCKED, `TriggeredStopConditions` C-05 y C-06, con dos hallazgos (abajo).
- Relevo de entrada:
  - HEAD = remoto = `66d34af3`, árbol limpio, `config.toml` sin cambio;
  - cuatro procesos ilegibles efímeros (`bash.exe` 34280, 29336 y 34004; `git.exe` 7848) ya no existían en la lectura de detalle: CLEARED_BY_REREAD
    por la regla del Coordinator;
  - un `dotnet.exe` huérfano es servidor de compilación.
  - 0 denegaciones. 4 órdenes fallidas, todas superadas. Sin P-06.

**Hechos remotos:**

| Corrida | SHA | Core | Build UI | UI Tests | Plugin | Filtro `FullyQualifiedName~RackCad.Tests.Workspace` (TRX Core) |
|---|---|---|---|---|---|---|
| 36948737576 (RedRun) | `36c344dc` | failure (32 fallidas) | success | success | skipped | 37 seleccionadas, 7 pass, 30 fail por aserción |
| 36949370189 (CurrentRun) | `66d34af3` | **failure** (2 fallidas) | success | success | skipped | 37 seleccionadas, 36 pass, 1 fail |

Las dos fallidas de CurrentRun:
- `RackCad.Tests.Workspace.WorkspaceModelBoundaryTests.ModelDoesNotPersistNorWriteAuthoredData`
- `RackCad.Tests.NamespaceFolderGuardTests.TestProjects_KeepExactlyOneAssemblyRootNamespace`

**Verificación 1 de 4** (`R20261002T011239Z-647e`; Codex `gpt-6-luna` / `high`, efectivo comprobado):
- Resultado: **EXECUTION_REWORK_REQUIRED / REWORK**, `FailureClass` **Ci**, `VerifiedSha` null.
- Ci y Tests en fail, ambas con `RedPart` pass. Las otras 12 en pass.
- Coherencia del README §8 comprobada.
- **RED acreditado:** `ChainRedSha` `36c344dc`; `ChainRedFiles` = `tests/RackCad.Tests/Workspace/` {SelectionContextTests,
  WorkspaceHintDrainTests, WorkspaceModelBoundaryTests, WorkspaceSessionTests}.cs.
- Hallazgo del Controller: C-05 y C-06 de la entrega no se confirman. Git no muestra salida del alcance ni ambigüedad material del Freeze.

**Causa raíz** (hechos de la sesión; el `analysis.md` formal es del Coordinator):
1. **Defecto de la prueba RT.** La guarda `ModelDoesNotPersistNorWriteAuthoredData` prohíbe la expresión `Registry`, pensada para
   `Microsoft.Win32.Registry`. Coincide con el tipo `WorkspaceSessionRegistry` del propio modelo, así que la guarda no puede pasar con ese
   nombre.
2. **Convención del repositorio.** `NamespaceFolderGuardTests.TestProjects_KeepExactlyOneAssemblyRootNamespace` exige que todo archivo de
   `tests/RackCad.Tests` declare exactamente `namespace RackCad.Tests`.
   - El criterio de aceptación de la delegación pide «el espacio de nombres RackCad.Tests.Workspace». Lo introdujo el delta de la
     planificación que redactó la sesión: **error propio**, por no comprobar las guardas de convención antes de fijar un espacio de nombres.
   - El filtro del contrato se cumple sin conflicto con `namespace RackCad.Tests` y clases con prefijo `Workspace`, porque
     `FullyQualifiedName~` compara por subcadena: `RackCad.Tests.WorkspaceSelectionContextTests…` contiene `RackCad.Tests.Workspace`.
   - Ambos arreglos quedan dentro de `tests/RackCad.Tests/Workspace/`.
3. **Forma de la corrección.** Toca `ChainRedFiles`, así que exige un RED nuevo:
   - commit RED con la corrección desactivada y los cambios de las pruebas de la cadena;
   - después un GREEN que no toque las pruebas.

   El criterio de espacio de nombres de la delegación debe cambiar, y eso exige una delegación de corrección emitida por una planificación
   del Controller, con `CorrectionOf`.

**STOP P-07.**
- El REWORK está autorizado por el contrato (`CorrectionsAuthorized` true).
- La corrección exige commit de `attempts` y volver a la planificación (I-61 Proposal V9 §12.1 paso 8; AUTOMATION_PLAN 16.8). El tope de
  planificación está agotado (2 de 2), así que la invocación de planificación de la corrección **no se lanza**.
- `attempts` sigue en 0 y el contador de la clase Ci en 0, porque la corrección no se ha lanzado. Decide el Coordinator: tope y `analysis.md`.

**Presupuesto:**

| Tipo | Uso |
|---|---|
| Planificación | 2 de 2 |
| Trabajo | 1 de 4 |
| Verificación | 1 de 4 |
| `attempts` | 0 de 3 |
| BLOCKED | 0 |

nc1-nc3 no se ejecutan: solo se ejecutan tras un VERIFIED.

**Custodia (16.12)** en `docs/automation/evidence/I-64-pilot/`:
- `F1-T1-MODEL/R20261002T003827Z-3a76/` (planificación y aceptación);
- `F1-T1-MODEL-nc4/R20261002T004303Z-8d3c/`;
- `F1-T1-MODEL/R20261002T005244Z-f656/` (prompt, entrega y registro del trabajo);
- `F1-T1-MODEL/R20261002T011239Z-647e/` (prompt, verificación y registro).

Los JSON de procesos solo llevan PID, padre, nombre, fecha y clase. Los TRX, eventos y transcripciones no se versionan: sus SHA-256 están en
los registros. Algunos archivos se escribieron con CRLF en el área transitoria: `worker-handoff.json`, los `processes-*.json`,
`nc4-result.json` y `acceptance-precheck-reasons.json`. Su SHA-256 transitorio difiere del blob, normalizado a LF, y la identidad duradera es
el blob del commit de custodia.

## 23. F1-T1-MODEL — REWORK aceptado, análisis del Coordinator y `attempts` = 1

Sección nueva; las anteriores no se reescriben. Hechos de la sesión responsable. Sin GATE PASS, F1 PASS ni ADR aceptado.

**Decisión del Coordinator** (orden en el chat de la sesión, 2026-10-02; resumen saneado):
- Acepta la verificación `R20261002T011239Z-647e` sobre el GREEN fallido `66d34af3`: **Disposition REWORK**, **FailureClass Ci**,
  ChainRedSha `36c344dc`.
- **STOP P-07 resuelto.**
- Nuevo presupuesto:

  | Tipo | Máximo | Consumidas | Restantes |
  |---|---|---|---|
  | Planificación | 3 | 2 | 1 |
  | Trabajo | 4 | 1 | 3 |
  | Verificación | 4 | 1 | 3 |

  `MaxReworkLoops` sigue en 3.
- La corrección entra como **attempt = 1**, sin cambio del Freeze, del alcance ni de la semántica de I-61.
- No se repite nc4: README §10 lo limita a la primera delegación de la cadena.

**Análisis de la corrección** (texto literal del Coordinator):
- Ubicación: `artifacts/orchestration/I-64/F1-T1-MODEL/0/R20261002T011239Z-647e/analysis.md`, en el directorio de la verificación que lo
  origina; custodiado en `docs/automation/evidence/I-64-pilot/F1-T1-MODEL/R20261002T011239Z-647e/analysis.md`.
- 59 líneas, UTF-8 con LF. SHA-256 `b0ddc8de37199ba703efa180b6bf8caab1a2230dc1469d9a53c6159315b98664`, igual al del blob por estar en LF.
- Contenido:
  - ROOT CAUSE 1: espacio de nombres de las pruebas frente a `NamespaceFolderGuardTests`;
  - ROOT CAUSE 2: la guarda de datos authored es demasiado amplia;
  - alcance de la corrección;
  - cadena: ChainBaseSha `e0587355`, ChainRedSha `36c344dc` y los 4 ChainRedFiles acreditados;
  - la instrucción de no fabricar un RED artificial.
- La delegación de corrección lo cita en `CorrectionOf`: `RunId` `R20261002T011239Z-647e`, `FailureClass` `Ci`, `AnalysisSha256` el de
  arriba.

**Lectura de la sesión para la planificación** (no vincula; la aceptación formal la decide el Coordinator):
- La corrección cambia las 4 pruebas de `ChainRedFiles`: espacio de nombres `RackCad.Tests`, clases `Workspace…Tests` y guarda acotada.
- Con la regla «exige RED» de AUTOMATION_PLAN 16.8, esa entrega exige un RED: el diff `BaseSha..CurrentSha` toca `ChainRedFiles`.
- 16.8 e I-61 Proposal V9 §12.1 paso 6.1 lo definen como la **corrección desactivada** con todos los cambios de las pruebas de la cadena en el
  mismo commit, y después un GREEN que no toque RT ∪ ChainRedFiles.
- No es un RED artificial. Las pruebas corregidas tienen que fallar por aserción del comportamiento congelado con la implementación
  desactivada, y el RED acreditado `36c344dc` sigue como `ChainRedSha` de la delegación.

**Presupuesto:**

| Tipo | Estado |
|---|---|
| Planificación | 2 de 3 |
| Trabajo | 1 de 4 |
| Verificación | 1 de 4 |
| `attempts` | **1** de 3 |
| Contador de la clase Ci | 1 al lanzar la corrección |

`main` `819955d6`, Freeze `9b43dafb` (blob `dc1924ff`) intacto, `config.toml` sin cambio.

## 24. F1-T1-MODEL — Corrección attempt 1: planificación, Worker, verificación REWORK (Ci) y STOP P-07

Sección nueva; las anteriores no se reescriben. Hechos de la sesión responsable. Sin GATE PASS, F1 PASS ni ADR aceptado.

**Planificación de corrección** (`R20261002T061355Z-c5cd`, 3 de 3; Codex `gpt-6-luna` / `high`, efectivo comprobado):
- Delegación SHA-256 `EC4EACA2…D11027`. Contrato sin cambios (`A4D36818…`).
- Attempt 1 y AttemptsRemaining 2. `CorrectionOf` = {`R20261002T011239Z-647e`, `Ci`, `b0ddc8de…98664`}.
- Cadena: ChainBaseSha `e0587355`, ChainRedSha `36c344dc` y los 4 ChainRedFiles.
- `TaskClass` «Depuración» (DEBUGGING, Balanced). Las siete colecciones y `Objective`, iguales byte a byte al contrato.
- A1-A8: las ocho en pass. nc4 no aplica (README §10).

**Aceptación formal del Coordinator** (chat, 2026-10-02): **ACCEPTED**.
- La corrección exige un **RED nuevo**, porque modifica ChainRedFiles.
- El RED anterior sigue siendo el de la emisión de la delegación.
- Si Ci.RedPart y Tests.RedPart quedan en pass, el RED nuevo pasa a ser el vigente.

**Worker de corrección** (`R20261002T062258Z-de0f`; agent() de workflow `wf_6903972a-b8d`, `sonnet` / `medium`):
- Prompt: §G.1 + DEBUGGING + delta; 181 líneas; SHA-256 `15009B20…236115`. El texto entregado es igual al archivo: sin P-05.
- Cesión 06:23:21Z → 06:32:11Z; 469750 ms; 23 llamadas de herramienta, 0 con error.
- Efectivos `claude-sonnet-5-5` / `medium` en las 43 entradas de la transcripción (SHA-256 `61A10533…B22CCB3DA4E298`).
- Commits:
  - **RED nuevo** `189353f8df954b05cbbcdc0f15dca0cb3bf21002`:
    - pruebas en `namespace RackCad.Tests` con clases `Workspace…Tests` (`SelectionContextTests.cs` → `WorkspaceSelectionContextTests.cs`);
    - guarda de datos authored acotada, con mutaciones en memoria;
    - comportamiento desactivado, con espacios de nombres de bloque.
  - **GREEN** `b38e54f9f434fee7aa41ce6dcfd1804978afd619`: el comportamiento restaurado, sin tocar pruebas.
- Diff `BaseSha..CurrentSha` = las 4 pruebas; `src` vuelve al estado de BaseSha. El ADR 0047 no cambia.
- Entrega: `IMPLEMENTATION_COMPLETE`.
- Relevo de entrada limpio: HEAD = remoto = `b38e54f9`; árbol limpio; `config.toml` sin cambio; sin participantes ni procesos no atribuibles. Un
  `dotnet.exe` huérfano es servidor de compilación.

**Hechos remotos:**

| Corrida | SHA | Core | Build UI | UI Tests | Plugin | Filtro (TRX Core) |
|---|---|---|---|---|---|---|
| 36973700069 (RedRun) | `189353f8` | failure (29 fallidas, todas del filtro) | success | success | skipped | 51 seleccionadas, 22 pass, 29 fail por aserción |
| 36973802008 (CurrentRun) | `b38e54f9` | **success** (12447/12447) | success | success | **failure** | 51/51 pass |

Causa del fallo del Plugin:
- El paso «Verify AutoCAD compile references» (`eng/ci/verify-autocad-references.ps1:113`, `-NoWarnings` sobre avisos MSB, CS, NU y NETSDK)
  rechaza la build Release de la solución por **CS8632** en `tests/RackCad.Tests/Workspace/WorkspaceSelectionContextTests.cs(62,75)`.
- Ahí, `string? id` está en un archivo sin `#nullable enable`. Hubo 0 errores de compilación.
- El defecto viene del RED original `36c344dc` (misma línea). El job del Plugin quedó `skipped` mientras Core fallaba.

**Verificación 2 de 4** (`R20261002T063546Z-0cd7`; Codex `gpt-6-luna` / `high`, efectivo comprobado):
- Resultado: **EXECUTION_REWORK_REQUIRED / REWORK**, `FailureClass` **Ci**. Solo Ci en fail, con `RedPart` pass.
- Tests en pass con `RedPart` pass; las otras 12 en pass. Coherencia del README §8 comprobada.
- **RED nuevo acreditado:** `ChainRedSha` `189353f8`.
- `ChainRedFiles` según 16.8, cálculo de la sesión: las 4 anteriores, incluida `SelectionContextTests.cs`, ∪ RT
  (`WorkspaceHintDrainTests.cs`, `WorkspaceModelBoundaryTests.cs`, `WorkspaceSelectionContextTests.cs`, `WorkspaceSessionTests.cs`) = **5 rutas**.
- Hallazgos de forma en la Evidence del Controller, sin efecto en el resultado:
  - Tests dice «cuatro rutas» sin enumerar el conjunto;
  - Scope menciona tres archivos de `src` que el diff no contiene;
  - Identity cita el HEAD con dos caracteres omitidos.

**STOP P-07.**
- El REWORK está autorizado y es de la misma clase (Ci).
- La siguiente corrección lleva `attempts` a 2 y el contador de Ci a 2; ambos están dentro de los topes (3).
- La corrección exige volver a la planificación, y el tope de planificación está agotado (3 de 3). **No se lanza.**
- **Análisis de la sesión** (no vincula):
  - El arreglo es una línea `#nullable enable` en `WorkspaceSelectionContextTests.cs`, o quitar la anotación `?`.
  - Toca ChainRedFiles, así que según 16.8 exige otra vez un RED (corrección desactivada + cambio de la prueba) y un GREEN.
  - Como la `FailureClass` es la misma, 16.8 no exige un `analysis.md` nuevo (`AnalysisSha256` admite null).
  - Para el siguiente paquete conviene un criterio verificable en local: la build Release del proyecto de pruebas sin avisos CS nuevos. Así se
    detectaría lo que hoy solo ve el job del Plugin cuando Core está verde.
  - **Lección propia:** el delta del Worker no exigía comprobar avisos en Release.

**Presupuesto:**

| Tipo | Uso |
|---|---|
| Planificación | 3 de 3 |
| Trabajo | 2 de 4 |
| Verificación | 2 de 4 |
| `attempts` | 1 de 3 |
| Contador de la clase Ci | 1 |

**Custodia (16.12)** en `docs/automation/evidence/I-64-pilot/F1-T1-MODEL/`:
- `R20261002T061355Z-c5cd/` (planificación y aceptación);
- `R20261002T062258Z-de0f/` (prompt, entrega y registro);
- `R20261002T063546Z-0cd7/` (prompt, verificación y registro).

Los archivos escritos con CRLF en el área transitoria (`worker-handoff.json` y los `processes-*.json`) se normalizan a LF en el blob, que es la
identidad duradera.

## 25. F1-T1-MODEL — Segundo REWORK aceptado, cuarta planificación autorizada y `attempts` = 2

Sección nueva; las anteriores no se reescriben. Hechos de la sesión responsable. Sin GATE PASS, F1 PASS ni ADR aceptado.

**Decisión del Coordinator** (chat, 2026-10-02; resumen saneado):
- Acepta la verificación `R20261002T063546Z-0cd7` sobre el GREEN fallido `b38e54f9`: **REWORK**, **FailureClass Ci**. RED vigente `189353f8`.
- **STOP P-07 resuelto.** Autoriza una **cuarta y última** planificación:

  | Tipo | Máximo | Consumidas | Restantes |
  |---|---|---|---|
  | Planificación | 4 | 3 | 1 |
  | Trabajo | 4 | 2 | 2 |
  | Verificación | 4 | 2 | 2 |

  `MaxReworkLoops` sigue en 3. La corrección entra como **attempt = 2**, sin cambio del Freeze ni de I-61.
- **Causa aceptada:** CS8632 en `tests/RackCad.Tests/Workspace/WorkspaceSelectionContextTests.cs`, por una anotación `string?` sin contexto de
  anotaciones nullable. La detecta la build Release del job «Build Plugin without AutoCAD». No es un defecto del Plugin.
- **Corrección preferida:**
  - directiva local `#nullable enable annotations`, o la forma mínima equivalente que admita el compilador del repositorio;
  - sin tocar `NamespaceFolderGuardTests`, los scripts de CI, el nullable global, los `.csproj` ni el Plugin;
  - sin debilitar avisos ni quitar cobertura.
- **RED nuevo obligatorio**, porque el archivo pertenece a ChainRedFiles:
  - el RED lleva la corrección nullable y el comportamiento desactivado, compila y falla por aserción real, sin fallo ni aviso artificial;
  - el GREEN restaura el comportamiento sin tocar RT ∪ ChainRedFiles.
- **Comprobación local obligatoria:** build Release sin CS8632 nuevo y la comprobación equivalente a «Build Plugin without AutoCAD» en verde.
  Si el script de CI puede ejecutarse en local sin AutoCAD, se usa antes del push final.
- **CorrectionOf:** `RunId` `R20261002T063546Z-0cd7`, `FailureClass` Ci. **`AnalysisSha256` = null.** La clase es la misma que la del REWORK
  anterior, así que AUTOMATION_PLAN 16.8 no exige un `analysis.md` nuevo, y el Coordinator ordenó no inventarlo.
- **Cadena:** ChainBaseSha `e0587355`, ChainRedSha `189353f8df954b05cbbcdc0f15dca0cb3bf21002`, ChainRedFiles las 5 rutas acreditadas (§24).

**Hecho de la sesión.** `eng/ci/verify-autocad-references.ps1` compila contra los paquetes NuGet de Autodesk de nuget.org. Usa rutas aisladas
y un directorio vacío como instalación de AutoCAD (`EmptyAutoCADDir`): no requiere AutoCAD instalado ni abierto. Necesita pwsh 7,
`DOTNET_ROOT` y red hacia nuget.org.

**Presupuesto:**

| Tipo | Estado |
|---|---|
| Planificación | 3 de 4 |
| Trabajo | 2 de 4 |
| Verificación | 2 de 4 |
| `attempts` | **2** de 3 |
| Contador de la clase Ci | 2 al lanzar la corrección |

`main` `819955d6` y Freeze `9b43dafb` intactos; `config.toml` sin cambio.

## 26. F1-T1-MODEL — Corrección attempt 2: Worker, CI verde y verificación EXECUTION_VERIFIED

Sección nueva; las anteriores no se reescriben. Hechos de la sesión responsable. Sin GATE PASS, F1 PASS ni ADR aceptado.

**Planificación 4 de 4** (`R20261002T140200Z-3886`; Codex `gpt-6-luna` / `high`, efectivo comprobado):
- Delegación SHA-256 `C3384842…BD9DF62`. Attempt 2 y AttemptsRemaining 1.
- `CorrectionOf` = {`R20261002T063546Z-0cd7`, `Ci`, `null`}.
- Cadena: ChainBaseSha `e0587355`, ChainRedSha `189353f8`, 5 ChainRedFiles. `TaskClass` «Depuración».
- A1-A8: las ocho en pass. Copia exacta.

**Aceptación formal del Coordinator** (chat, 2026-10-02): **ACCEPTED**, con la regla de RED nuevo y la comprobación local de la build Release.

**Worker** (`R20261002T141605Z-dc2b`; workflow `wf_6e99223f-343`, `sonnet` / `medium`):
- Prompt: 193 líneas; SHA-256 `16EC3AA1…29C040A`. El texto entregado es igual al archivo: sin P-05.
- Cesión 14:16:44Z → 14:29:19Z; 684879 ms; 70 llamadas de herramienta.
- Efectivos `claude-sonnet-5-5` / `medium` en las 102 entradas de la transcripción (SHA-256 `EC6A40AE…C6DE7C`).
- 1 error de herramienta: el harness bloqueó un `sleep`. Se superó y no es una denegación.
- Commits:
  - **RED** `a9778068af566bc2b84ba6a0005711b6452a2666`: `#nullable enable annotations` como primera línea de
    `WorkspaceSelectionContextTests.cs`, más el comportamiento desactivado.
  - **GREEN** `8d9a0c6e6ea148caf3e8f20d26a95a864a2943ce`: el comportamiento restaurado, sin tocar pruebas.
- Diff neto respecto de la base: solo esa directiva.
- El Worker declaró un HEAD desacoplado temporal que produjo el commit local `a2db0a91`. No se publicó: no está en ninguna rama remota.
- `verify-autocad-references.ps1` en local se detuvo en `Assert-CleanRunner` (AutoCAD 2025 instalado; causa de entorno). Las builds Release de
  RackCad.Tests y RackCad.UI.Tests dieron 0 avisos MSB, CS, NU o NETSDK.
- Relevos:
  - salida: un `bash.exe` ilegible desapareció en la relectura (CLEARED_BY_REREAD);
  - entrada: sin participantes, procesos no atribuibles ni descendientes nuevos; dos `dotnet.exe` servidores de compilación.

**Hechos remotos:**

| Corrida | SHA | Core | Build UI | UI Tests | Plugin | Filtro (TRX Core) |
|---|---|---|---|---|---|---|
| 37019198364 (RedRun) | `a9778068` | failure (29, todas del filtro) | success | success | skipped | 51 seleccionadas, 22 pass, 29 fail |
| 37020159700 (CurrentRun) | `8d9a0c6e` | **success** 12447/12447 | **success** | **success** | **success** | 51/51 pass |

El log del Plugin dice «Plugin Release publish and solution Release build succeeded without warnings» y «AutoCAD compile-reference
verification passed».

**Verificación 3 de 4** (`R20261002T143410Z-ae0b`; Codex `gpt-6-luna` / `high`, efectivo comprobado):
- Resultado: **EXECUTION_VERIFIED / NONE**. `VerifiedSha` `8d9a0c6e`. Las 14 comprobaciones en pass.
- Coherencia del README §8 comprobada.
- Control manual de 16.10 de la sesión sobre el texto libre de la verificación: 0 coincidencias.
- **RED acreditado:** `ChainRedSha` `a9778068`; `ChainRedFiles` = las 5 rutas. La delegación queda cerrada.

**ADR 0047.** La sesión lo comparó mecánicamente con el Anexo A de la Proposal V5:
- las 11 decisiones y el contexto son literalmente iguales;
- el resto solo difiere en formato (mayúscula inicial, viñetas);
- sigue propuesto y no cambió después de `36c344dc`.

**Controles negativos nc1..nc3:** no se ejecutaron.
- En I-61 cuentan dentro del tope de invocaciones de Codex. Queda 1 verificación de 4 y el contrato no los lista. Decide el Coordinator.

**Paquete de cierre de tarea:** `docs/automation/evidence/I-64-pilot/F1-T1-MODEL/task-close-package.md`.

**Custodia (16.12):** `F1-T1-MODEL/R20261002T140200Z-3886/`, `R20261002T141605Z-dc2b/` y `R20261002T143410Z-ae0b/`. El CRLF de
`worker-handoff.json` y de los `processes-*.json` se normaliza a LF en el blob.

**Presupuesto final:**

| Tipo | Uso |
|---|---|
| Planificación | 4 de 4 |
| Trabajo | 3 de 4 |
| Verificación | 3 de 4 |
| `attempts` | 2 de 3 |
| Contador de la clase Ci | 2 |

`main` `819955d6` y Freeze intactos; `config.toml` sin cambio.

## 27. F1-T1-MODEL — Controles negativos nc1..nc3: nc1 conforme, nc2 no superado y STOP

Sección nueva; las anteriores no se reescriben. Hechos de la sesión responsable. No hay cierre de la tarea ni F1 PASS.

**Decisión del Coordinator** (chat, 2026-10-02; resumen saneado):
- **PRODUCT EXECUTION = ACCEPTED.** **TASK CLOSE = PENDING nc1..nc3.**
- README §10 es general y los exige.
- Presupuesto independiente de fase CONTROL: 3 controles, 1 salida válida cada uno y hasta 2 reintentos por control, solo por transporte o
  `INVALID_OUTPUT`.
- El presupuesto productivo queda congelado (planificación 4/4, trabajo 3/4, verificación 3/4, `attempts` 2/3).
- Entradas: las de la verificación VERIFIED `R20261002T143410Z-ae0b`.

**Método de la sesión:**
- Cada control copia todas las entradas de `ae0b` y muta solo el campo indicado.
- Usa el mismo prompt salvo la ruta de entrada y el `RunId`.
- Controller Codex `gpt-6-luna` / `high`, solo lectura, efectivo comprobado.
- **Decisión declarada:** en la copia se recalculó solo el hash derivado del archivo mutado: `Outcome.OutputSha256` del registro del trabajo en
  nc1 y del de planificación en nc2.
  - Motivo: la verificación real compara ese hash, y sin recalcularlo la copia sería incoherente y Handoff o Contract fallarían antes que la
    comprobación objetivo.
  - En I-61 no se recalculó porque aquel Controller no comparaba hashes.

**nc1** (`R20261002T150553Z-09df`): `worker-handoff.CurrentSha` `8d9a0c6e…` → `deadbeef…deadbeef`, inexistente.
- Salida válida y coherente: `EXECUTION_BLOCKED/STOP`, `FailureClass` Identity.
- Termination, Handoff, Authority y Contract en pass, como en la real. **Oráculo relativo cumplido.**
- **Salvedad:** la Evidence de Identity cita también que HEAD = `origin/<rama>` = `747ead04` ≠ `CurrentSha`. Identity habría fallado igualmente
  sin la mutación, así que el control **no es discriminante** en el estado actual de la rama.

**nc2** (`R20261002T151201Z-abb1`): `delegation.AllowedWriteScope`.
- El primer archivo del diff `1504c938..8d9a0c6e` es `tests/RackCad.Tests/Workspace/WorkspaceSelectionContextTests.cs`. Lo cubre el prefijo
  `tests/RackCad.Tests/Workspace/`, que se sustituyó por los otros 3 archivos de pruebas bajo él en `CurrentSha`.
- Salida válida y coherente, pero `EXECUTION_BLOCKED/STOP` con **`FailureClass` Identity**: Identity en fail y **Scope en pass**.
- **Oráculo no cumplido. Control NO superado.** Al ser una salida válida, no es un fallo de transporte y no admite reintento.

**Causas:**
1. **Sesión: desviación DEV-F1T1-02.** El commit (7) de custodia `747ead04` se hizo tras la verificación y antes de los controles negativos.
   - AUTOMATION_PLAN 16.4, paso 5, prohíbe escrituras Git de la sesión hasta terminar también los controles negativos. En ese momento la
     sesión los consideraba fuera de presupuesto y no ordenados.
   - Desde entonces HEAD = `origin/<rama>` = `747ead04` ≠ `CurrentSha` `8d9a0c6e`. Identity (16.9) exige su igualdad.
   - Los commits posteriores a `8d9a0c6e` (`747ead04`) solo tocan `docs/automation/`.
2. **Controller (nc2).** Scope quedó en pass con la afirmación «ruta cubierta por AllowedWriteScope», falsa para la delegación mutada. Su orden
   pwsh para leer `delegation.json` falló con `InvalidArgument`.

**nc3 no se ejecuta.** Su oráculo exige las 12 comprobaciones anteriores iguales a la real, incluida Identity en pass. Es imposible mientras HEAD
≠ `CurrentSha`, y ejecutarlo consumiría una invocación con un no-superado previsible.

**Opciones para el Coordinator** (de la sesión; no vinculan):
- **(a)** Una A-n (Coordinator con el Architect) que, para los controles de esta tarea, defina Identity sobre el estado verificado cuando
  `CurrentSha..HEAD` solo contenga commits documentales de la sesión en `docs/automation/`. Después, repetir nc1..nc3 con ese criterio en el prompt.
- **(b)** Ejecutar nc1..nc3 sobre una réplica desechable fuera del worktree con la rama en `8d9a0c6e`, como las réplicas de la sonda U-04 de I-61.
  Desvía la receta («`-C` solo con el worktree de la unidad»), así que necesita autorización expresa.
- **(c)** Registrar DEV-F1T1-02 y decidir el cierre de la tarea sin nc2/nc3 válidos.

**Para tareas futuras:** ningún commit de la sesión entre la verificación VERIFIED y el final de nc1..nc3 (16.4 paso 5).

**Presupuesto CONTROL:**

| Control | Estado |
|---|---|
| nc1 | 1 salida válida (oráculo cumplido, con salvedad) |
| nc2 | 1 salida válida (no superado) |
| nc3 | 0 |

Sin reintentos de transporte. El presupuesto productivo sigue congelado y sin cambios.

**Relevos:** sin participantes ni procesos no atribuibles persistentes; `config.toml` sin cambio; `main` `819955d6`; HEAD = remoto = `747ead04`.

**Custodia (16.12):** `docs/automation/evidence/I-64-pilot/F1-T1-MODEL-nc1/R20261002T150553Z-09df/` y
`F1-T1-MODEL-nc2/R20261002T151201Z-abb1/`. Incluye prompt, verificación, registro, `mutation.json`, `oracle-result.json` y las entradas mutadas
con su registro recalculado.

## 28. F1-T1-MODEL — STOP de controles confirmado; recuperación (d) y `attempts` = 3

Sección nueva; las anteriores no se reescriben. Hechos de la sesión responsable. Sin cierre de la tarea ni F1 PASS.

**Decisión del Coordinator** (chat, 2026-10-02; resumen saneado):
- **STOP confirmado.** Causa: la sesión publicó `747ead04` antes de nc1..nc3, en contra de AUTOMATION_PLAN 16.4 paso 5 (**DEV-F1T1-02**). I-61 no cambia.
- **Rechazadas:** (a) una A-n que debilite Identity; (b) los controles en una copia o desvío del worktree; (c) cerrar T1 sin nc1..nc3.
- **Autorizada la recuperación (d):**
  - una entrega final semánticamente neutra sobre la punta actual;
  - su verificación;
  - nc1..nc3 **inmediatamente después, sin escrituras Git de la sesión entre medias**.
- **nc1 `R20261002T150553Z-09df` y nc2 `R20261002T151201Z-abb1` = SUPERSEDED_BY_CONTROL_RECOVERY.** No son evidencia final de §10: nc1 no
  discriminaba y nc2 no cumplió su oráculo. nc3 no se ejecutó.
- La resolución cambia el trabajo: **`attempts` 2 → 3**, que es el máximo. Un nuevo REWORK o STOP que exija trabajo será STOP S-11 definitivo.
- Presupuesto excepcional:

  | Tipo | Máximo | Consumidas | Restantes |
  |---|---|---|---|
  | Planificación | 5 | 4 | 1 |
  | Trabajo | 4 | 3 | 1 |
  | Verificación productiva | 4 | 3 | 1 |

  El presupuesto CONTROL independiente sigue vigente. Después de esta recuperación no se autoriza nada más.
- **Cambio permitido:** solo el comentario de documentación XML existente de `WorkspaceSessionRegistry`, en
  `src/RackCad.Application/Workspace/WorkspaceSessionRegistry.cs`. Debe aclarar que es un registro puro y transitorio en memoria de sesiones
  vivas, sin autoridad de persistencia.
  - Sin cambio ejecutable, de API, de pruebas, del ADR, de Plugin, UI, Domain ni de configuración. Sin «marcador de control».
  - No exige RED si el diff no toca ChainRedFiles.
- **Delegación:** Attempt 3, AttemptsRemaining 0. Cadena: ChainBaseSha `e0587355`, ChainRedSha `a9778068`, ChainRedFiles las 5 rutas.
  `CorrectionOf` = {`R20261002T151201Z-abb1` (el control que detuvo la cadena), `StopCondition`, SHA-256 del `analysis.md`}.
- **Oráculos finales:**
  - nc1: Identity en fail → BLOCKED/STOP;
  - nc2: Identity y Remote en pass, Scope en fail → BLOCKED/STOP;
  - nc3: FreeText en fail → REWORK, con las posteriores iguales a la real.

**Análisis del Coordinator** (texto literal):
- Ubicación: `artifacts/orchestration/I-64/F1-T1-MODEL-nc2/2/R20261002T151201Z-abb1/analysis.md`; custodiado en
  `docs/automation/evidence/I-64-pilot/F1-T1-MODEL-nc2/R20261002T151201Z-abb1/analysis.md`.
- 21 líneas, UTF-8 con LF. SHA-256 `6fb6b914dd47cb43dff341868edf701ffe1fd89c8096bbcdad2b304e98b68b66`.

**Compromiso de la sesión.** Este commit es el `BaseSha` de la última delegación. Desde él, ningún commit, push ni otra escritura Git de la sesión
hasta terminar Worker → verificación → nc1 → nc2 → nc3 (16.4 paso 5, literal). La custodia, el estado y la evidencia irán después, en un único
commit documental.

`main` `819955d6` y Freeze intactos; `config.toml` sin cambio.
