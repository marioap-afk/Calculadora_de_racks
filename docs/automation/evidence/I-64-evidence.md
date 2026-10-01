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
