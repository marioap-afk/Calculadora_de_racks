# I-64 — F1-T3A-HOST-CORE: paquete de cierre documental para la revisión del Coordinator

Preparado por la sesión responsable el 2026-10-11, tras la aceptación del Coordinator del contrato de reemplazo
(`R20261011T045535Z-0140/coordinator-acceptance.md`) y la cadena completa de Worker, verificación, controles y puentes.

Este paquete:
- **no** declara F1 PASS, GATE PASS, Candidato ni FINAL_CANDIDATE_SHA;
- no reescribe la historia de nc2 ni de nc3, y no reetiqueta la planificación `ed7b`;
- deja al Coordinator la revisión del cierre (COORDINATOR_T3A_CLOSE_REVIEW_REQUIRED).

## 1. Estado resultante

| Hecho | Valor |
|---|---|
| Producto | **EXECUTION_VERIFIED / NONE**: verificación `R20261011T061322Z-5734` (Controller `gpt-6-luna`/`high`), `FailureClass` NONE, las 14 comprobaciones en pass |
| VerifiedSha | `ba52b3b983fb7d2a02393df643e8323b125b3d03`, CI 38116594290 push con los 4 jobs en verde |
| Cierre de protocolo | **SATISFIED UNDER A-4 + A-6** (evidencia sustitutiva para Scope y FreeText; los controles del Controller nc2 y nc3 conservan su resultado) |
| Estado de la tarea | **COMPLETE_UNDER_I64_SUBSTITUTE_PROTOCOL_EVIDENCE**, a falta de la revisión del Coordinator |

## 2. Cadena de planificación

1. **Planificación original `R20261011T043044Z-ed7b`** (`AuthorityRevision` `42d45776`, `MainSha` `bb0d5522`):
   - contrato `gate-contract.json` SHA-256 `d17227c5943d11a08da22f0b4e94c273ae39ed0ee7531439be4380a3a0620e09`;
   - delegación SHA-256 `07683da70b1e10de7c48f0eb11bd6312d7d93ce01038d75d930f086a5d4f8fff`;
   - A1-A8 8 de 8 en pass; nc4 `R20261011T043456Z-2bac` PASS;
   - aceptación del Coordinator (literal en `R20261011T043044Z-ed7b/coordinator-acceptance.md`).
2. **STOP previo al Worker** (`pre-worker-stop.json`): el adaptador del Plugin no exponía la sesión activa, la autorización ni la
   reconciliación del núcleo. Sin Worker y sin escrituras Git.
3. **Corrección de alcance del Coordinator** (`coordinator-scope-correction.md`, commit `ee676668`, CI 38112864850 4/4):
   - `ed7b` = VALID / HISTORICAL / **SUPERSEDED_BY_SCOPE_CORRECTION** (`planning-disposition.json`); no es INVALID_OUTPUT;
   - `EventBridgeCore.cs` y `AutoCadEventBridge.cs` quedan liberados solo para sus deltas exactos.
4. **Planificación de reemplazo `R20261011T045535Z-0140`** (`AuthorityRevision` `ee676668`, `MainSha` `bb0d5522`):
   - contrato SHA-256 `2616ac79e4de1a4872bfcc9796deb375dc17abb60afb3c5cc9f84cab77df3d74`, ASCII y válido contra
     `rackcad-gate-contract/v1`;
   - delegación SHA-256 `1d61b5ffcfd6ef3508c23e51d84100f6baadccf5636a63790c55903bc70a45d1`, de `gpt-6-luna`/`high` con Codex CLI
     `0.162.0-alpha.17.2`, sin fallos de transporte (0 de 2 invocaciones adicionales);
   - A1-A8 8 de 8 en pass; nc4 `R20261011T045742Z-5e57` PASS;
   - I-65 releído (`ff0c57c7`), sin solape;
   - aceptación del Coordinator (literal en `R20261011T045535Z-0140/coordinator-acceptance.md`): ACCEPTED; `0140` es la autoridad
     operativa de planificación.

## 3. Producto

- **Worker único `R20261011T050726Z-0829`:**
  - subagente `claude-sonnet-5-5`/`high` de la sesión, con el prompt de SHA-256
    `33c50b3f7b66ed3341160ed4144a932a6fdd976c6b24403080a37013b22fa7de`;
  - 175 respuestas con `claude-sonnet-5-5`/`high` en la transcripción; sin denegaciones y sin avisos de límite;
  - entrega `worker-handoff.json` SHA-256 `7e1117919fe4f454ba9f484b0f85dd53c060a68e936cb13dd1eeada82823607b`, con
    `WorkerStatus` IMPLEMENTATION_COMPLETE, `Disposition` NONE, sin desviaciones ni preguntas abiertas.
- **RED `de8e767372c55a5bfb5aecc6541259af778bba24`** (padre `ee676668`):
  - filtro `FullyQualifiedName~RackCad.Tests.Workspace`: 486 seleccionadas, 314 pasan y 172 fallan;
  - los 172 fallos son por aserción, ninguno por compilación, y todos caen en las clases nuevas de T3A;
  - CI 38116482614 push: `Tests (Domain + Application)` en failure y `Build Plugin without AutoCAD` en skipped.
- **GREEN `ba52b3b983fb7d2a02393df643e8323b125b3d03`** (padre RED, solo `src/`):
  - filtro 486/486 y suite Core 13081/13081 (censos y guardas de T1/T2 incluidos);
  - en la CI, UI 1637 superadas y 17 NotExecuted (Skip existentes);
  - CI 38116594290 push 4/4.
- **Build Release del Plugin** a un directorio temporal: 0 errores; solo los 2 avisos MSB3277 de la línea base de la estación, medida
  por la sesión en `BaseSha`. RackCad.Tests y RackCad.UI.Tests con 0 errores y 0 avisos MSB/CS/NU/NETSDK.
- **30 archivos** (`git diff --name-status ee676668 ba52b3b9`): 23 nuevos y 7 modificados, todos dentro de `AllowedWriteScope`.
  - `src/RackCad.Application/Workspace/`:
    - modificado: `EventBridgeCore.cs` (+32/-0, solo líneas añadidas);
    - nuevos: `IWorkspaceBridgeLink.cs`, `IWorkspaceHostPort.cs`, `WorkspaceHostCore.cs`, `WorkspaceHostPolicy.cs`,
      `WorkspaceHostSnapshot.cs`, `WorkspaceInstrumentation.cs`, `WorkspaceLazyInstance.cs` y `WorkspaceSelectionRead.cs`.
  - `src/RackCad.Plugin/Workspace/`:
    - modificado: `AutoCadEventBridge.cs` (+18/-0, solo las cuatro fachadas finas);
    - nuevos: `WorkspaceController.cs`, `WorkspaceHostPort.cs`, `WorkspacePanelCommand.cs` y `WorkspaceSelectionReader.cs`.
  - Pruebas existentes modificadas:
    - `tests/RackCad.Tests/CustomPropertiesCommandGuardTests.cs` (+5/-2) y `tests/RackCad.Tests/SelectiveEditorOpenTests.cs`
      (+6/-1): censo 37 → 38 con `RACKPANEL`, sin alias;
    - `tests/RackCad.Tests/Workspace/WorkspaceBridgeSourceGuardTests.cs` (+43/-6): solo las excepciones exactas autorizadas.
  - Pruebas nuevas en `tests/RackCad.Tests/Workspace/`: `WorkspaceBridgeFacadeSourceGuardTests`,
    `WorkspaceBridgeReconciliationTests`, `WorkspaceHostAuthorizationTests`, `WorkspaceHostDrainTests`, `WorkspaceHostLifecycleTests`,
    `WorkspaceHostPolicyTests`, `WorkspaceHostPortGuardTests`, `WorkspaceHostSourceGuardTests`, `WorkspaceHostSourceSupport`,
    `WorkspaceHostTestSupport`, `WorkspaceInstrumentationTests`, `WorkspaceSelectionMappingTests` y
    `WorkspaceSourceGuardExceptionTests` (`.cs`).
  - RT = 16 archivos de `tests/`, todos en el commit RED. El GREEN toca 11 archivos, todos de `src/`.
- **Trailer de los dos commits del Worker:** `Co-Authored-By: Claude Sonnet 5.5 <noreply@anthropic.com>`.
- **Límites declarados por la entrega:**
  - la degradación que decide el puente de T2 no se puede consultar con las cuatro fachadas autorizadas; el host solo ve su propia
    degradación, la de una lectura fina que falla;
  - un cambio de documento activo no pide por sí solo una reconciliación (la piden la apertura, mostrar y `RefreshPanel`);
  - `RACKPANEL` en T3A solo inicia el estado del panel y escribe un mensaje en la línea de comandos; `PaletteSet`, foco y contención
    de UI quedan para T3B;
  - nada se probó en AutoCAD (Smoke-1).
- **Instrumentación:** solo en memoria (`WorkspaceInstrumentation`). `RackLog.cs` no cambia y no hay STOP C-T3-INSTRUMENTATION.

## 4. Verificación

- **`R20261011T061322Z-5734`**, Controller `gpt-6-luna`/`high`/read-only con Codex CLI `0.162.0-alpha.17.2`:
  - prompt SHA-256 `db777b6c9eba74de7c40cea91b175edff09fb53fbda552f2a24af49ce60b4393`;
  - salida `controller-verification.json` SHA-256 `adbbedda2d557d23ece9d0e5b6ed7691ce57f443266a039783c69806fbd0866e`.
- **Resultado:** **EXECUTION_VERIFIED / NONE**, `FailureClass` NONE, `VerifiedSha` `ba52b3b9`.
  - Las 14 comprobaciones en pass, con `RedPart` en pass en Ci y Tests.
  - Salida válida y coherente según README §8. El control manual de 16.10 sobre el texto libre no encontró coincidencias.
- **Hallazgo inexacto del Controller:** dice que el encabezado de la solicitud indicaba `AuthorityRevision` …`8354`. El prompt y todas
  las entradas dicen `ee6766684f560c5128f0cc6af9c5d091de6e8353` (0 apariciones de `8354`). No tiene efecto: el Controller usó el SHA
  de la delegación aceptada.

## 5. Controles: tabla final

| Control | RunId | Resultado |
|---|---|---|
| nc1 | `R20261011T062049Z-a33e` | **PASS**: `CurrentSha` mutado a `deadbeef…`; EXECUTION_BLOCKED/STOP, `FailureClass` Identity, las otras 13 iguales a la real; evidencia de la entrada con 30 archivos (`evidence-check.json`) |
| Controller nc2 | `R20261011T062511Z-162f` | **CONTROL NOT PASSED**: con `EventBridgeCore.cs` fuera de `AllowedWriteScope`, EXECUTION_VERIFIED/NONE con Scope pass; salida válida, conservada |
| I64-SCOPE-BRIDGE-01 (A-4) | `R20261011T063223Z-324b` | **PASS** (sección 6) |
| Controller nc3 | `R20261011T063320Z-587f` | INVALID_OUTPUT: FreeText fail, pero STOP con C-15 |
| Controller nc3 | `R20261011T063955Z-af95` | INVALID_OUTPUT: FreeText fail, pero STOP con C-15 y S-11 |
| Controller nc3 | `R20261011T064522Z-302e` | INVALID_OUTPUT: FreeText fail, pero STOP con C-15 → **P-04**; **Controller nc3 = NOT PASSED / P-04**, sin reetiquetar |
| I64-FREETEXT-BRIDGE-01 (A-6) | `R20261011T064937Z-7dd5` | **PASS** (sección 6) |
| nc4 | `R20261011T045742Z-5e57` (sobre `0140`) | **PASS**: A3 fail ante `AllowedWriteScope += 'src/RackCad.UI/'`, REJECTED_BEFORE_INVOCATION |
| nc4 histórico | `R20261011T043456Z-2bac` (sobre `ed7b`) | PASS, conservado |

En los tres nc3, FreeText es la única comprobación que difiere de la verificación real (`nc3-mechanical-check.json`). La clasificación
INVALID_OUTPUT aplica la regla de la decisión del Coordinator sobre nc3 en T2: C-15/S-11 gobiernan una corrección real, no el oráculo de
un control negativo, y README §10 exige EXECUTION_REWORK_REQUIRED/REWORK (sección 8).

## 6. Puentes sustitutos

**I64-SCOPE-BRIDGE-01 `R20261011T063223Z-324b`** (A-4):
- **Verificador:** bytes de `git cat-file blob cf381ab96a3d945a9569d56bb3aaf546e3f2fce1`, SHA-256
  `a3d4ed8a25e99ea6b8bc0af945e6e688c13f472a7e0745752c79e04ee79f7d65`; identidad comprobada antes de ejecutar.
- **Entradas:**
  - delegación real `1d61b5ff…`, igual a la aceptada;
  - delegación adversaria = la entrada de nc2 `162f` (`3ddd210d…`);
  - entrega `7e111791…`; `BaseSha` `ee676668` y `CurrentSha` `ba52b3b9`.
- **Resultado:**
  - REAL PASS (código 0, `Reasons` [], salida `fd1f809c…`);
  - ADVERSARIAL FAIL solo por `OUTSIDE_ALLOWED_WRITE_SCOPE` (`src/RackCad.Application/Workspace/EventBridgeCore.cs`; código 1, salida
    `21717652…`);
  - mismos 30 archivos cambiados y mismos hashes de identidad en ambas; oráculo **Ok**.

**I64-FREETEXT-BRIDGE-01 `R20261011T064937Z-7dd5`** (A-6, sin modelos):
- **Verificador:** bytes de `git cat-file blob 3bc7869e9f4c9c5eed0d64d7b8fee6f7002b9169`, SHA-256
  `e760858883926744a606d9438099ed8abf040eea07814275e94b1843307474d4`; `HEAD:<ruta>` es el mismo blob.
- **REAL:** `worker-handoff.json` de `0829`, SHA-256 `7e1117919fe4f454ba9f484b0f85dd53c060a68e936cb13dd1eeada82823607b`.
- **MUTATED:** la entrega mutada de nc3 `302e`, idéntica en los tres intentos, SHA-256
  `239a9ef60a32b12e70c6dcbbdc87e0fd6153c96e4e5afe5843737ee2ca5617f4`.
- **Conjunto de términos:** bytes de `git cat-file blob bb0d5522:docs/AUTOMATION_PLAN.md` (blob `07b97bd3`, SHA-256 `b5ca7294…`), el
  `MainSha` del contrato y de la delegación; sección 16.10 SHA-256 `76766227…`; 16 términos y 7 campos gobernados.
- **Producto:** verificación `5734`, EXECUTION_VERIFIED / NONE sobre `ba52b3b9`.
- **Resultado:** **PASS**, código de salida 0, `Reasons` [], stderr vacío, las 29 comprobaciones en pass; salida SHA-256
  `5d282952945f5f1ec5574683984e0665542580cf86cb5e0fb85a1a115fdb941e`.
  - `ChangedFields` = [`WorkCompleted`]; `IntroducedForbiddenTerms` = [`GATE PASS`]; `RealForbiddenHits` = [];
    `MutatedForbiddenHits` = `WorkCompleted`/`GATE PASS`/1.
- `config.toml` en la instantánea C antes y después (`config-pre.json`, `config-post.json`).

## 7. Presupuesto

| Fase | Consumo |
|---|---|
| Planificación | 1 válida histórica (`ed7b`, superada) + 1/1 de reemplazo (`0140`), sin invocaciones adicionales |
| Trabajo | 1/1 (`0829`) |
| Verificación | 1/1 (`5734`) |
| nc1 | 1 válida, sin reintentos |
| nc2 | 1 |
| nc3 | 0 válidas + 3 INVALID_OUTPUT (P-04) |
| nc4 | 1 por planificación (`2bac`, `5e57`) |
| Puentes | I64-SCOPE-BRIDGE-01: 1 ejecución; I64-FREETEXT-BRIDGE-01: 1 ejecución |

`attempts` sigue en 3 de 3 (AttemptsRemaining 0) y `CorrectionsAuthorized` en false. No se reinició ningún presupuesto ni hubo Worker de
corrección.

## 8. Desviaciones y eventos que se conservan

- **Regla de nc3 aplicada por analogía:** la sesión clasificó los tres nc3 de T3A como INVALID_OUTPUT con la regla de la decisión del
  Coordinator sobre nc3 en T2, sin una decisión propia de T3A. Esa clasificación decidió los dos reintentos, el P-04 y la entrada en
  A-6. Queda para la revisión del Coordinator.
- **Hallazgo inexacto del Controller** en la verificación (`…8354`, sección 4): sin efecto, registrado en el relevo.
- **stderr de la verificación:** además del aviso de lectura de stdin, tiene dos líneas del CLI
  `failed to record rollout items: thread 01a12998-… not found`. El código de salida es 0 y la salida es válida. El relevo no las
  menciona y se custodian tal cual (`R20261011T061322Z-5734/stderr.txt`).
- **Recuento de líneas del relevo del Worker:** dice +29/-0 y +13/-0 para los dos archivos liberados; son líneas añadidas no vacías.
  `git diff --numstat` da +32/-0 y +18/-0. En los dos casos no se elimina ninguna línea.
- **Planificación `ed7b`:** superada por la corrección de alcance, que nació del error de derivación H-9 de la sesión en el paquete de
  T3 (§50). Se conservan sus A1-A8, su nc4 y la parada previa al Worker.
- **I-65:** su punta pasó de `237d03eb` (entrada del Worker) a `16fa9e76` (F3-T1 de RAM Elements, sobre `main` `bb0d5522`). Sus archivos
  están en `StructuralAnalysis/`, `Systems/Cantilever/`, `assets/structural-analysis/` y sus propios documentos: sin solape con T3A y
  sin consumo.

## 9. Custodia

- **Manifiesto:** `custody-manifest.json` lista los 373 archivos transitorios de `artifacts/orchestration/I-64/F1-T3A-HOST-CORE*/`,
  cada uno con su SHA-256 y su disposición:
  - 101 custodiados;
  - 11 custodiados con redacción;
  - 34 idénticos a un archivo custodiado;
  - 227 excluidos con motivo: 166 salidas crudas de compilación o pruebas, 42 datos operativos, 8 `events.jsonl`, 8 stderr de 39
    bytes o vacíos y 3 copias de blobs versionados.
- **Redacción:** la ruta local del worktree, la del checkout principal y la del dotnet de usuario se sustituyen por marcadores. Los
  nombres `[projects.*]` ya constaban como `[projects.<redactado>]` en los registros. Se conserva el SHA-256 de cada original. Las
  entradas REAL y MUTATED de los puentes se custodian redactadas; sus originales, con los SHA-256 de la sección 6, siguen en los
  transitorios.
- **Fin de línea:** los archivos que escribió PowerShell con CRLF se custodian en LF, con los dos SHA-256 en el manifiesto.
- **Decisiones custodiadas:**
  - aceptación original (`R20261011T043044Z-ed7b/coordinator-acceptance.md`);
  - aceptación del contrato de reemplazo (`R20261011T045535Z-0140/coordinator-acceptance.md`);
  - ya versionadas: descomposición de T3 (`coordinator-decision-t3-decomposition.md`) y corrección de alcance
    (`coordinator-scope-correction.md`).

## 10. Siguiente

- **F1-T3B-HOST-UI:** NOT YET AUTHORIZED; no se inicia. Contiene `PaletteSet`, raíz WPF, foco y contención de UI sobre el puerto de
  T3A.
- **F1-T3C-PROFILE-DOC:** RESERVED.
- **F1:** sigue IN PROGRESS; F1 PASS no se declara.
- **Smoke-1:** va tras F1 PASS y el RELEASE de la ventana de host de I-52.
