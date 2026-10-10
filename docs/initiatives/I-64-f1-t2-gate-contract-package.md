# I-64 — Paquete del contrato de gate de F1-T2-BRIDGE

```text
Estado: PAQUETE PARA EMISIÓN — NO EMITIDO
Emisor: Coordinator (AUTOMATION_PLAN 16.5; agent-execution/README §1)
Preparado por: sesión responsable de I-64 (Claude), orden NIGHT RUN, fase 9
Borrador ASCII: docs/automation/evidence/I-64-pilot/F1-T2-BRIDGE/contract-package/gate-contract.DRAFT-NOT-ISSUED.json
Rama al preparar: architecture/workspace-persistente-rackcad, rebasada sobre origin/main bb0d5522 (evidencia §39)
STOP: COORDINATOR_T2_GATE_CONTRACT_REQUIRED (sin presupuesto autoritativo de T2; decisiones C-T2-01..C-T2-05)
```

Este documento no es un `gate-contract.json`, no emite delegaciones y no autoriza trabajo. Deriva el contrato de F1-T2-BRIDGE de las
fuentes de autoridad, sin añadir alcance, para que el Coordinator lo emita, lo corrija o lo rechace. **No se ha lanzado ninguna
planificación del Controller para T2:** la orden NIGHT RUN la condiciona a que el presupuesto de la tarea nueva esté definido, y no lo está
(C-T2-01).

## 1. Fuentes

| Fuente | Qué aporta |
|---|---|
| Freeze V5 (`I-64-proposal-v5.md`, blob `dc1924ff`) D-02, D-03 y D-04 | capas, puerto, sesiones e identidad, puente único, fuentes, ciclo de vida, manejadores, `BeginDocumentClose`, drenaje y modo degradado |
| Freeze V5 §10 | INV-02, INV-03 (lado de eventos), INV-14, INV-21, con su clase de prueba y su RED esperado |
| Freeze V5 §11 y §13 | F1 y su cierre; clase «Implementación transversal a capas (corta)» / Deep |
| `I-64-f1-gate-contract-proposal.md` §§2-7 | descomposición T0..T3, alcance propuesto, autoridades, celdas, topes propuestos y STOP adicionales |
| `I-64-A-2.md` y `I-64-A-4.md` | sustituto de nc2 (I64-SCOPE-BRIDGE-01) aplicable a cualquier tarea de I-64 con las condiciones 1-10 |
| Estado `I-64.yml` y evidencia §§38-39 | T1 COMPLETE_UNDER_I64_SCOPE_BRIDGE; `attempts` 3/3; rebase y revalidación |
| Código tras el rebase (`8e6a1b4e`) | API del modelo de T1; referencias de `RackCad.Tests`; ausencia de suscripciones a eventos en el Plugin |
| Autoridades de `main` (`bb0d5522`) | AGENTS, WORKFLOW, LIFECYCLE, AUTOMATION_PLAN §16, `agent-execution/`, ADR-0006/0019/0029/0044; ninguna cambió entre `819955d6` y `bb0d5522` |

## 2. Objetivo

Puente central de eventos de F1 (D-04) con fuente de eventos simulable, sin host ni panel:
- un único suscriptor de eventos de AutoCAD, en `src/RackCad.Plugin/Workspace/`, con exactamente una suscripción por fuente:
  - colección de documentos: activación, creación y destrucción;
  - por documento con sesión: `ImpliedSelectionChanged`, `CommandWillStart`, `CommandEnded`, `CommandCancelled`, `CommandFailed`,
    `BeginDocumentClose` y `EnteringQuiescentState` de su `Editor`;
  - por base de datos de documento con sesión: `ObjectAppended`, `ObjectErased`, `ObjectModified`, `ObjectUnappended` y
    `ObjectReappended`;
  - `Application.Idle` solo mientras haya pistas pendientes.
- Ciclo de vida de las suscripciones como función exacta de las sesiones del modelo de T1 (`WorkspaceSessionRegistry`).
- Manejadores que solo encolan (`WorkspaceSession.EnqueueHint`).
- Drenaje diferido y coalescido (`WorkspaceSession.TryDrain`, `DrainConditions`).
- `BeginDocumentClose` de vida de sesión, también en modo degradado; el modo degradado es por sesión.
- Guardas de fuente.

**Fuera de T2:**
- F1-T3-HOST: `PaletteSet`, raíz WPF, `RACKPANEL`, selección fina hacia el contexto, foco, contención de la UI e instrumentación de host.
- F4: el veto de cierre por borradores (INV-20).
- F2: la navegación.
- F6: el inventario.
- La escritura authored y AutoCAD en ejecución.

## 3. Alcance de escritura

| | Rutas | Origen |
|---|---|---|
| `AllowedWriteScope` | `src/RackCad.Application/Workspace/`, `src/RackCad.Plugin/Workspace/`, `tests/RackCad.Tests/Workspace/` | propuesta F1 §3 más la primera ruta; ver C-T2-03 |
| `ForbiddenWriteScope` | `docs/`, `.github/`, `assets/`, `eng/`, `deploy/`, `global.json`, `Directory.Build.*`, `RackCad.sln`, `src/RackCad.UI/`, `src/RackCad.Domain/`, `tests/RackCad.UI.Tests/`, `src/RackCad.Plugin/{Systems,Views,Drawing}/`, `src/RackCad.Plugin/PluginInitializer.cs`, los `*.csproj` del Plugin, Application, pruebas e importador, los 17 `src/RackCad.Plugin/*Commands*.cs` existentes y los 5 archivos de T1 en `src/RackCad.Application/Workspace/` | propuesta F1 §3 («rutas nuevas; ninguna existente de producto»); precedente del contrato de T1; archivos calientes |

Los proyectos son SDK y compilan los `.cs` nuevos sin tocar ningún `csproj`. El Plugin actual no tiene ninguna suscripción a eventos de
AutoCAD, así que la guarda de INV-02 («`+=` solo en la carpeta del puente») no necesita lista de excepciones.

## 4. Invariantes (borrador, INV-F1-T2-01..14)

| ID | Contenido | Origen |
|---|---|---|
| 01 | Suscripciones = función exacta de los documentos con sesión. Cero para un documento destruido. Se cumple en estos ciclos: `RACKPANEL` repetido (inicialización repetida del controlador), A→B→A, ocultar y mostrar, alta y baja de `Idle`, destrucción. Ninguna suscripción por pestaña ni editor | INV-02, D-04 |
| 02 | `+=` sobre eventos de AutoCAD solo en `src/RackCad.Plugin/Workspace/` | INV-02 (guarda) |
| 03 | Ningún evento escribe selección o vista. Los manejadores solo encolan: no leen ni escriben la base de datos, no piden datos, no abren diálogos y no llaman a resolvers | INV-03 (eventos), D-04 P-03 |
| 04 | Toda excepción del manejador se captura y no se propaga. Una excepción del puente degrada la sesión afectada y retira solo sus suscripciones de sincronización | D-04, O-07 |
| 05 | `BeginDocumentClose` vive toda la sesión, también degradada. Su manejador solo usa estado en memoria | D-04, A64-PV3-04 |
| 06 | Las pistas llegan en cualquier momento. El drenaje se hace solo en reposo y sin modal RackCad activo, y coalesce. `Idle` solo está suscrito con pistas pendientes | D-04, INV-23, P-04 |
| 07 | `StartTransaction`, `StartOpenCloseTransaction` y `LockDocument` solo dentro de `using`. Sin campos de tipo `Transaction`, `DBObject` ni `Document`. La API del puente no expone tipos de AutoCAD | INV-14 |
| 08 | Nada se persiste. Nada se escribe en authored, NOD, XData, el registro de Windows ni el perfil | INV-21, D-22 |
| 09 | Cero suscripciones antes del primer `RACKPANEL`. Nada al cargar el plugin | D-02, P-01 |
| 10 | Pista ligada a su sesión. Retiro en `DocumentToBeDestroyed`. Sin reemparejamiento por identificador reciclado | D-03, INV-22 |
| 11 | AutoCAD solo en el Plugin. Las guardas de T1 siguen en verde | ADR-0006 |
| 12 | Sin tipos de I-63 (`ComputedParameters`, métricas, providers, `ProjectSummary`, agregación) | MASTER-I63-I64-02 |
| 13 | RACKEDITAR, los comandos clásicos y los editores no cambian | D-16, M-03 |
| 14 | Sin `Autodesk.AutoCAD.Internal` | D-01, O-10 |

## 5. RED esperado y pruebas requeridas

- `RequiredTests`: `tests/RackCad.Tests/RackCad.Tests.csproj`, filtro `FullyQualifiedName~RackCad.Tests.Workspace`, `MinSelected` 1,
  `ExpectRed` true. Esta es la misma forma que en T1. Las clases nuevas deben empezar por `Workspace` para que el filtro las seleccione.
  Hoy el filtro selecciona 51 pruebas, todas en verde (revalidación §39).
- RED de la cadena: T2 es un `TaskId` nuevo. `ChainBaseSha` es el `BaseSha` de su primera delegación, `ChainRedSha` es `null` y
  `ChainRedFiles` está vacío. **La primera entrega exige RED** (16.8):
  - un RED real por aserción del comportamiento congelado (INV-02: el recuento crece en algún ciclo; INV-03: un contexto que reescribe;
    INV-14: la guarda falla ante un campo o un `StartTransaction` sin `using`);
  - sin fallos de compilación ni pruebas artificiales;
  - después, el GREEN.

## 6. STOP

S-01..S-14 y P-01..P-08 sin cambios, más estos:

| ID | Condición |
|---|---|
| C-01 | `origin/main` cambia antes de escribir |
| C-02 | Hay que tocar UI, Domain, comandos, `PluginInitializer.cs`, `csproj` o `sln` |
| C-03 | Hace falta AutoCAD, el DLL, el perfil, `TRUSTEDPATHS`, `SECURELOAD`, ACL o el registro |
| C-04 | Hay que ampliar el alcance |
| C-05 | Aparece una ambigüedad material del Freeze |
| C-06 | Cambia `config.toml` |
| C-07 | Conflicto de propiedad o archivo caliente |
| C-08 | Deriva hacia T3 |
| C-09 | Tipos de I-63 |
| C-10 | Comandos o RACKEDITAR |
| C-11 | Escritura authored, NOD o XData |
| C-12 | `Autodesk.AutoCAD.Internal` |
| C-13 | Presupuesto agotado |

## 7. Autoridades

- **`UNIT_DOC`, en `AuthorityRevision`:**
  - Freeze V5, discovery y contrato de la iniciativa;
  - A-2 y A-4;
  - evidencia §18 y §39;
  - estado;
  - propuesta F1;
  - este paquete.
- **`EXTERNAL`, en `MainSha`:** las de T1, más `docs/automation/agent-execution/model-catalog.md`.
- Sin `UNIT_CHANGE`.
- Los nombres de sección del borrador están transliterados a ASCII con la regla de REISSUE_GATE_CONTRACT_ASCII.

## 8. Enrutamiento

| Rol | Celda | Elegibilidad (routing.md §5) |
|---|---|---|
| Worker (Deep) | subagente × `claude-sonnet-5-5` × `high`, `write-commit-push` | Equilibrado, Deep → `high` (§3). Medida en U-04 (`medium` y `high`) y usada con `high` en G3. Consumo cubierto. Verificada el 2026-09-30; pasa a STALE el 2026-12-29 |
| Controller | Codex CLI × `gpt-6-luna` × `high` | Eficiente con más effort para Balanced (§3). Medida en PR-1 y vuelta a medir con codex-cli 0.162.0-alpha.2 en la sonda `R20261010T014704Z-97f8` (PASS). Consumo cubierto |

- `gpt-6.1-sol`/`high` quedó medida esta noche, en la sonda `R20261010T024641Z-ed2c`, para la revisión del Architect. El catálogo, que es
  `EXTERNAL`, todavía la registra como no medida, y no se propone como Controller.
- `RoutingEnforcement`: `required`.

## 9. Controles negativos

- nc1..nc4 según README §10, con una ejecución válida cada uno.
- nc2 se ejecuta con el Controller y su evidencia se conserva sea cual sea el resultado (A-2, condición 2).
- Si el Controller vuelve a dar Scope en pass sobre la delegación mutada (deuda de protocolo registrada para I-62), el nc2 se satisface con
  I64-SCOPE-BRIDGE-01 bajo A-4:
  - verificador `cf381ab9`;
  - REAL = PASS;
  - ADVERSARIAL = FAIL solo por `OUTSIDE_ALLOWED_WRITE_SCOPE`;
  - condiciones 1-10.
- Los presupuestos de T1 no se reutilizan.

## 10. Coordinación (lectura de solo lectura, 2026-10-10)

| Rama | Punta | Rutas de T2 | Archivos calientes que toca |
|---|---|---|---|
| I-52 `feature/rackmirror-espejo-semantico` | `fb6b5648` | ninguna; no toca `src/` ni `tests/` | `docs/HANDOFF.md`, `docs/ROADMAP.md`, `docs/adr/README.md`, 4 `*Commands.cs` de `eng/research` y `eng/validation` |
| I-62 `architecture/portabilidad-coordinador-principal` | `362bec12` | ninguna | `docs/ROADMAP.md`, `tests/RackCad.Tests/I62*` |
| I-63 | integrada en `main` (`bb0d5522`) | — | `src/RackCad.Application/ComputedParameters/`; sin referencias cruzadas con Workspace (MASTER-I63-I64-02 comprobado en `8e6a1b4e`) |

- T2 no escribe archivos calientes y no necesita Smoke-1.
- Smoke-1 sigue bloqueado hasta el RELEASE de host de I-52 y ocurre tras F1 PASS (Freeze §12.1).

## 11. Decisiones del Coordinator (STOP COORDINATOR_T2_GATE_CONTRACT_REQUIRED)

- **C-T2-01 — Tope de invocaciones de T2 (16.8, P-07).**
  - El Freeze no lo fija, y ninguna orden emitida lo fija para T2. Los topes de T0 y T1 los fijó el Coordinator tarea a tarea.
  - La propuesta F1 §5 sugiere 1 planificación, 4 trabajos y 4 verificaciones, más los controles del README §10. No es autoritativa.
  - Sin tope, P-07 no es evaluable y no se lanza ninguna planificación.
- **C-T2-02 — `attempts` de la unidad.**
  - `attempts` cuenta por unidad (16.8) y está en 3, con `max_attempts` = 3 (contrato de la iniciativa, `automation.max_attempts`).
  - T2 puede delegarse con `Attempt` 3, pero `AttemptsRemaining` = 0: el primer REWORK de T2 daría STOP S-11 y `CorrectionsAuthorized`
    no tendría efecto.
  - La sesión no lo resuelve: no reinicia, no reutiliza y no amplía. El Coordinator decide aceptar T2 sin correcciones o pedir el cambio de
    `max_attempts` a quien tenga autoridad sobre el contrato de la iniciativa.
- **C-T2-03 — Comprobabilidad y alcance.**
  - `tests/RackCad.Tests` solo referencia Domain y Application (y el importador). No puede instanciar tipos del Plugin, que necesita los
    ensamblados de AutoCAD.
  - Las pruebas Core con fuente simulada de INV-02 e INV-03 exigen un núcleo del puente sin AutoCAD. El borrador añade
    `src/RackCad.Application/Workspace/` (solo archivos nuevos; los de T1 quedan prohibidos). El único suscriptor (`+=`) sigue en
    `src/RackCad.Plugin/Workspace/`, y D-02 asigna a Application las máquinas de estado y las políticas.
  - La alternativa literal de la propuesta (solo el Plugin) deja INV-02 sin su clase de prueba Core. Cambiar los `csproj` está prohibido.
  - La sesión no lo considera material, pero clasificarlo (y, si fuera material, la revisión del Architect) corresponde al Coordinator.
- **C-T2-04 — INV-21 y el puerto.**
  - La guarda del Freeze se formula sobre el puerto (`IWorkspaceHostPort`): «no expone escrituras salvo Actualizar y el descarte». El
    puerto lo consume la UI de T3.
  - En T2, el borrador reduce INV-21 a INV-F1-T2-08, es decir, que el puente no persiste ni escribe nada. La guarda del puerto iría con
    T3.
  - El Coordinator confirma ese reparto o fija dónde nace el puerto.
- **C-T2-05 — `AuthorityRevision` y `MainSha`.**
  - `AuthorityRevision` = el commit X que contiene este paquete y la §39, sin rutas `EXTERNAL` desde `05a5ae9d` (imagen de `e0587355`)
    (16.3).
  - `MainSha` = `origin/main` al emitir (hoy `bb0d5522`).
  - El primer `BaseSha` = `HEAD` al delegar.
