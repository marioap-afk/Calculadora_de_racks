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
