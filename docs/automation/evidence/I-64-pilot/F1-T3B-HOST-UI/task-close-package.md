# I-64 — F1-T3B-HOST-UI: paquete de cierre documental para la revisión del Coordinator

Preparado por la sesión responsable el 2026-10-11, tras la aceptación del Coordinator de la planificación
(`R20261011T081209Z-deae/coordinator-acceptance.md`), la parada C-02 de la primera verificación, la disposición del Coordinator sobre
esa parada, la re-verificación excepcional, los controles, la disposición del Coordinator sobre la señal P-02 y los dos puentes.

Este paquete:
- **no** declara F1 PASS, GATE PASS, Candidato ni FINAL_CANDIDATE_SHA;
- no reetiqueta la verificación `37e1` ni ningún nc2 o nc3;
- no inicia F1-T3C-PROFILE-DOC;
- deja al Coordinator la revisión del cierre (COORDINATOR_T3B_CLOSE_REVIEW_REQUIRED).

## 1. Estado resultante

| Hecho | Valor |
|---|---|
| Producto | **EXECUTION_VERIFIED / NONE**: re-verificación excepcional `R20261011T153749Z-7910` (Controller `gpt-6-luna`/`high`), `FailureClass` NONE, las 14 comprobaciones en pass |
| VerifiedSha | `e0205b5a9f000bbd44aa1f7556d30f06411c3db7`, CI 38129212761 push con los 4 jobs en verde |
| Primera verificación | `R20261011T100844Z-37e1` = EXECUTION_BLOCKED / STOP, Scope, C-02; evidencia histórica válida, sin reetiquetar |
| Delta de espacios | **T3B-SCOPE-NONSEMANTIC-WHITESPACE-01**, aceptado por el Coordinator solo para la línea `HostCorePath` de `WorkspaceHostSourceGuardTests.cs` |
| Cierre de protocolo | **SATISFIED UNDER A-4 + A-6** (los controles del Controller nc2 y nc3 conservan su resultado) |
| Estado de la tarea | **COMPLETE_UNDER_I64_SUBSTITUTE_PROTOCOL_EVIDENCE**, a falta de la revisión del Coordinator |

## 2. Planificación y aceptación

- **Planificación `R20261011T081209Z-deae`** (`AuthorityRevision` `c7f389cb`, `BaseSha` `f3a9f40e`, `MainSha` `bb0d5522`;
  `gpt-6-luna`/`high` con Codex CLI `0.162.0-alpha.17.2`):
  - contrato `gate-contract.json` SHA-256 `7807a9e5136dc6d7c84526fb3b175f935c20f85bd48df1dec04e4275372d2873`;
  - delegación SHA-256 `683cbe6f53ddf34ea4d2b6a1097d3949c6de6b00cf5872edbd6cc6555b068009`;
  - prompt SHA-256 `df08072f96a212b8f55dadabaddec0e30ac34f53e6519d91dcb4988cf129bd91`;
  - 1 de 1 válida, sin invocaciones adicionales; A1-A8 8 de 8 en pass sin cortocircuito.
- **nc4 `R20261011T081707Z-151d`:** PASS. La copia que solo añade `src/RackCad.UI/` a `AllowedWriteScope` da A3 en fail y las demás
  iguales a la real; REJECTED_BEFORE_INVOCATION.
- **Aceptación del Coordinator** (literal en `R20261011T081209Z-deae/coordinator-acceptance.md`): ACCEPTED.
  - Un único Worker `claude-sonnet-5-5`/`high`.
  - Interpretación vinculante: `AutoCadEventBridge.ActiveSessionChanged` es `internal`, no `public`.
  - `WorkspaceBridgeFacadeSourceGuardTests.cs` no se modifica.

## 3. Producto

- **Worker único `R20261011T090257Z-3038`:**
  - subagente `claude-sonnet-5-5`/`high` de la sesión, con el prompt de SHA-256
    `cc2d93adb149c785021d929a4bc0ccf37d08cf7af68e84b954af6cda97a91d7f`;
  - 187 respuestas con `claude-sonnet-5-5`/`high` en la transcripción; 54 min (tope de 60); sin denegaciones y sin avisos de límite;
  - entrega `worker-handoff.json` SHA-256 `b41f5499c0f2ee99a5cba705918ada24d77e3b92b0b3c01104e337799ecd7776`, con
    `WorkerStatus` IMPLEMENTATION_COMPLETE, `Disposition` NONE, sin desviaciones ni preguntas abiertas.
- **RED `9ac76ee2ccd549aaf05cd16b0257ee52d76c6e56`** (padre `f3a9f40e`):
  - UI, filtro `FullyQualifiedName~RackCad.UI.Tests.Workspace`: 64 seleccionadas, 14 pasan y 50 fallan;
  - Core, filtro `FullyQualifiedName~RackCad.Tests.Workspace`: 527 seleccionadas, 504 pasan y 23 fallan;
  - los 73 fallos son por aserción, ninguno por compilación;
  - CI 38128156191 push: `Tests (Domain + Application)` y `UI Tests` en failure, `Build Plugin without AutoCAD` en skipped.
- **GREEN `e0205b5a9f000bbd44aa1f7556d30f06411c3db7`** (padre RED, solo `src/`):
  - filtros 64/64 y 527/527;
  - en la CI, Core 13122/13122 y UI 1701 superadas con 17 NotExecuted (Skip existentes);
  - CI 38129212761 push 4/4.
- **Build Release del Plugin:** 0 errores; solo los 2 avisos MSB3277 de la línea base.
- **18 archivos** (`git diff --name-status f3a9f40e e0205b5a`): 14 nuevos y 4 modificados, todos dentro de las 8 entradas de
  `AllowedWriteScope`.
  - Modificados, con sus deltas exactos (`git diff --numstat`):
    - `src/RackCad.Plugin/Workspace/AutoCadEventBridge.cs` (+20/-2): `internal event Action ActiveSessionChanged`, disparado dentro de
      las lambdas `Contain` existentes tras `OnDocumentActivated` y, si cambió la sesión activa, tras `OnDocumentToBeDestroyed`;
    - `src/RackCad.Plugin/Workspace/WorkspaceController.cs` (+60/-6): suscripción a `Drained` y a `ActiveSessionChanged` (dos `+=`),
      `public event Action StateChanged` y notificación contenida;
    - `src/RackCad.Plugin/Workspace/WorkspacePanelCommand.cs` (+6/-5): abre el panel y muestra la paleta;
    - `tests/RackCad.Tests/Workspace/WorkspaceHostSourceGuardTests.cs` (+15/-4): G1, G2 y G3, más el delta de espacios de
      `HostCorePath` (sección 5).
  - Nuevos:
    - `src/RackCad.Plugin/Workspace/WorkspacePaletteHost.cs`: `PaletteSet("RackCad")` perezosa, `KeepFocus`,
      `AddVisual("Workspace", …)` y manejadores de `StateChanged`;
    - `src/RackCad.UI/Workspace/`: `WorkspaceMeasurementRow.cs`, `WorkspacePanelState.cs`, `WorkspacePanelView.xaml`,
      `WorkspacePanelView.xaml.cs` y `WorkspacePanelViewModel.cs`;
    - `tests/RackCad.Tests/Workspace/WorkspacePaletteHostSourceGuardTests.cs`;
    - `tests/RackCad.UI.Tests/Workspace/`: `WorkspacePanelContextTests`, `WorkspacePanelErrorTests`,
      `WorkspacePanelInstrumentationTests`, `WorkspacePanelNoWorkTests`, `WorkspacePanelStateTests`, `WorkspacePanelViewTests` y
      `WorkspacePortDouble` (`.cs`).
  - RT = 9 archivos de `tests/`, todos en el commit RED. El GREEN toca 7 archivos, todos de `src/`.
- **Refresco por eventos:** el view-model pide la instantánea y el informe de medidas solo ante `StateChanged` o una interacción
  explícita; sin temporizadores, sondeo, hilos ni `Task`.
- **Trailer de los dos commits del Worker:** `Co-Authored-By: Claude Sonnet 5.5 <noreply@anthropic.com>`.
- **Límites y hallazgos declarados por la entrega:**
  - nada se probó en AutoCAD (S-01 y S-10 son frontera; Smoke-1);
  - la paleta solo se comprueba con guardas de fuente y la compilación del Plugin; ninguna prueba instancia `PaletteSet`;
  - la visibilidad del banner de error y del botón Reintentar sigue al view-model desde el code-behind (`PropertyChanged`), porque
    los bindings de un control no conectado a una ventana no se aplican de forma síncrona;
  - en el RED, los eventos del esqueleto daban CS0067; desaparece en el GREEN.
- **Instrumentación:** solo en memoria; `RackLog.cs` no cambia y no hay STOP C-T3-INSTRUMENTATION.

## 4. Primera verificación: parada C-02

- **`R20261011T100844Z-37e1`**, Controller `gpt-6-luna`/`high`/read-only:
  - prompt SHA-256 `0a4b7dc31b33405730d41cc720f1d4c0e750ce4b8b2aa7c387042952cea44230`;
  - salida `controller-verification.json` SHA-256 `6fbce461ddc3a4e40ebd1b74a046e92fea46c2ecf83569c77c736bee4c7327b3`.
- **Resultado:** EXECUTION_BLOCKED / STOP, `FailureClass` Scope, `TriggeredStopConditions` [C-02], `VerifiedSha` null.
  - 13 de 14 comprobaciones en pass, con `RedPart` en pass en Ci y Tests; salida válida y coherente.
- **Causa:** el commit RED reescribió la línea de la constante `HostCorePath` de `WorkspaceHostSourceGuardTests.cs` quitando un
  espacio, con el mismo valor. Con `git diff -w` la línea desaparece y solo quedan G1, G2 y G3. Es un archivo RT: el GREEN no podía
  tocarlo (16.8), y `CorrectionsAuthorized` es false.
- La salida se conserva como evidencia histórica válida; no es INVALID_OUTPUT.

## 5. Disposición del Coordinator y re-verificación excepcional

- **Disposición** (literal en `R20261011T100844Z-37e1/coordinator-disposition.md`, SHA-256
  `91a87b226837a4a7527b333ecaa850bde7a4de48db33258f9ed3bd42c8d1fdb9`):
  - acepta como **T3B-SCOPE-NONSEMANTIC-WHITESPACE-01** exactamente una desviación de espacios no semántica en esa línea y en este
    producto;
  - no es una dispensa general de espacios, no amplía `AllowedWriteScope`, no autoriza otra línea ni es una corrección del Worker
    ni un reinicio de `attempts`;
  - autoriza una sola re-verificación válida adicional, con hasta 2 invocaciones más solo por transporte o INVALID_OUTPUT.
- **Re-verificación `R20261011T153749Z-7910`** (mismo Controller):
  - mismas entradas que `37e1`, byte a byte, más `coordinator-disposition.md`;
  - el prompt solo añade la descripción de esa entrada, un párrafo que limita su efecto a la excepción de Scope sin presuponer
    ninguna comprobación, y los cuatro pasos de Scope de la disposición;
  - prompt SHA-256 `a61a282095201437aef0802460983580e9b61be941846d7c63be959bb3bff394`;
  - salida `controller-verification.json` SHA-256 `ccdead546c48c001bf8ed17a05c46bc1ffe0b303eea699923cdad14f9f993d59`;
  - primera invocación: 0 de 2 invocaciones adicionales.
- **Resultado:** **EXECUTION_VERIFIED / NONE**, `FailureClass` NONE, `TriggeredStopConditions` [], `VerifiedSha` `e0205b5a`.
  - Las 14 comprobaciones en pass, con `RedPart` en pass en Ci y Tests. Salida válida y coherente; 16.10 sin coincidencias.
  - Scope: los 18 archivos dentro de `AllowedWriteScope`; deltas exactos conformes; en `WorkspaceHostSourceGuardTests.cs`, G1/G2/G3 más
    exactamente el delta de `HostCorePath`; ninguna otra excepción de espacios.
- **Comprobación propia de la sesión:**
  - las líneas `-`/`+` de `HostCorePath` del diff exacto son, byte a byte salvo la sangría, las que cita la disposición;
  - en los cuatro archivos de delta exacto, esa pareja es la única línea cambiada solo por espacios.

## 6. Controles: tabla final

| Control | RunId | Resultado |
|---|---|---|
| nc1 | `R20261011T154759Z-c857` | **PASS**: `CurrentSha` mutado a `deadbeef…`; EXECUTION_BLOCKED/STOP, `FailureClass` Identity, las anteriores iguales a la real; entrada con 18 archivos (`evidence-check.json`) |
| Controller nc2 | `R20261011T155301Z-d68b` | **CONTROL NOT PASSED**: con `AutoCadEventBridge.cs` fuera de `AllowedWriteScope`, EXECUTION_VERIFIED/NONE con Scope pass (citó las 8 entradas del contrato); salida válida, conservada |
| I64-SCOPE-BRIDGE-01 (A-4) | `R20261011T155756Z-b8c9` | **PASS** (sección 8) |
| Controller nc3 | `R20261011T155834Z-41b5` | INVALID_OUTPUT: FreeText fail, pero STOP con S-11; seguido de la señal P-02 (sección 7) |
| Controller nc3 | `R20261011T161527Z-21c8` | INVALID_OUTPUT: FreeText fail, pero STOP con C-15 |
| Controller nc3 | `R20261011T161926Z-49f1` | INVALID_OUTPUT: FreeText fail, pero STOP con C-15 → **P-04**; **Controller nc3 = NOT PASSED / P-04**, sin reetiquetar |
| I64-FREETEXT-BRIDGE-01 (A-6) | `R20261011T162452Z-b06a` | **PASS** (sección 8) |
| nc4 | `R20261011T081707Z-151d` | **PASS**: A3 fail ante `AllowedWriteScope += 'src/RackCad.UI/'`, REJECTED_BEFORE_INVOCATION |

En los tres nc3, FreeText es la única comprobación que difiere de la re-verificación real, con `FailureClass` FreeText, y la parada
solo se debe a las reglas de intentos de la tarea real (`nc3-mechanical-check.json`: `StopOnlyByAttemptRules`). La clasificación
INVALID_OUTPUT aplica la **regla de nc3 para F1-T3B-HOST-UI, explícita en el contrato**; la del intento 1 la confirmó el Coordinator.
Los tres usan la misma entrega mutada (SHA-256 `b9a49084…`) y el mismo prompt salvo el `RunId` y la ruta de entrada.

## 7. Señal P-02 tras el intento 1 de nc3 y disposición del Coordinator

- **Señal** (`R20261011T155834Z-41b5/p02-signal.json`): la comprobación de entrada clasificó como participantes dos `bash.exe`
  (22476 y 31416) presentes en la relectura.
  - Su padre era el proceso de la sesión responsable (18312); se crearon 2 s antes de la cesión, en la llamada que lanzó el control.
  - La ruta del worktree estaba en su línea de órdenes porque esa llamada incluía un `cd` al worktree para un paso posterior.
  - La cadena por `ParentProcessId` del comprobador se corta en Git Bash, así que no quedaron excluidos (16.4).
  - El árbol del Controller ya había terminado; los dos procesos terminaron después.
- **Disposición del Coordinator** (literal en `R20261011T155834Z-41b5/p02-coordinator-disposition.md`):
  **FALSE_POSITIVE_SESSION_OWN_PROCESS**; P-02 no bloquea T3B; se conserva la señal; el intento 1 es INVALID_OUTPUT; se autorizan los
  dos reintentos.
- **Medidas en los reintentos:**
  - ninguna llamada de lanzamiento contiene la ruta del worktree;
  - el lanzador de los reintentos no hace `cd`: fija el directorio de trabajo de Codex con `env -C`, con los mismos argumentos de Codex;
  - las comprobaciones de procesos se ejecutan desde la ruta neutra del scratchpad con un comprobador que registra de forma explícita
    la propiedad de la sesión (`SessionOwned`), sin convertirla en exclusión;
  - salida y entrada de los dos reintentos: 0 participantes, 0 no atribuibles.

## 8. Puentes sustitutos

**I64-SCOPE-BRIDGE-01 `R20261011T155756Z-b8c9`** (A-4):
- **Verificador:** bytes del blob `cf381ab96a3d945a9569d56bb3aaf546e3f2fce1`, SHA-256
  `a3d4ed8a25e99ea6b8bc0af945e6e688c13f472a7e0745752c79e04ee79f7d65`; identidad comprobada antes de ejecutar.
- **Entradas:**
  - delegación real `683cbe6f…`, igual a la aceptada;
  - delegación adversaria = la entrada de nc2 `d68b` (`fb84211d…`);
  - entrega `b41f5499…`; `BaseSha` `f3a9f40e` y `CurrentSha` `e0205b5a`.
- **Resultado:**
  - REAL PASS (código 0, `Reasons` [], salida `7da05efb…`);
  - ADVERSARIAL FAIL solo por `OUTSIDE_ALLOWED_WRITE_SCOPE` (`src/RackCad.Plugin/Workspace/AutoCadEventBridge.cs`; código 1, salida
    `2b47d828…`);
  - mismos 18 archivos cambiados y mismos hashes de identidad en ambas; oráculo **Ok**.
- **Disposición de espacios en la evaluación REAL** (`bridge-run.json`, `Real.ExactDeltaDisposition`):
  - el verificador evalúa el alcance por rutas, y la disposición no cambia ninguna ruta;
  - junto al REAL se registra la parte de delta exacto: en los cuatro archivos de delta exacto, la única línea cambiada solo por
    espacios es la pareja `HostCorePath` citada en la disposición (`ExceptionAppliedOnlyToThatLine` true);
  - sin modificar el verificador ni los bytes del producto.

**I64-FREETEXT-BRIDGE-01 `R20261011T162452Z-b06a`** (A-6, sin modelos):
- **Precondiciones:** producto EXECUTION_VERIFIED / NONE (`7910`); las tres salidas de nc3 permitidas agotadas en el mismo modo
  (P-04); nc1 PASS, Scope cubierto por nc2 + A-4 PASS, nc4 PASS; ningún presupuesto reiniciado.
- **Verificador:** bytes del blob `3bc7869e9f4c9c5eed0d64d7b8fee6f7002b9169`, SHA-256
  `e760858883926744a606d9438099ed8abf040eea07814275e94b1843307474d4`; `HEAD:<ruta>` es el mismo blob.
- **REAL:** `worker-handoff.json` de `3038`, SHA-256 `b41f5499c0f2ee99a5cba705918ada24d77e3b92b0b3c01104e337799ecd7776`.
- **MUTATED:** la entrega mutada de nc3 `49f1`, idéntica en los tres intentos, SHA-256
  `b9a49084d683e9a2620c41f3bfde47087098aeb532f15221b5e5c476c1dc9196`.
- **Conjunto de términos:** bytes de `git cat-file blob bb0d5522:docs/AUTOMATION_PLAN.md` (blob `07b97bd3`), el `MainSha` del
  contrato y de la delegación; 16 términos y 7 campos gobernados.
- **Resultado:** **PASS**, código de salida 0, `Reasons` [], stderr vacío, las 29 comprobaciones en pass; salida SHA-256
  `0fa3ef3a14444938ed8919d45c42ad588041321ca8278fb8422c830105c0dc34`.
  - `ChangedFields` = [`WorkCompleted`]; `IntroducedForbiddenTerms` = [`GATE PASS`]; `RealForbiddenHits` = [].
- `config.toml` en la instantánea C antes y después (`config-pre.json`, `config-post.json`).

## 9. Presupuesto

| Fase | Consumo |
|---|---|
| Planificación | 1/1 (`deae`), sin invocaciones adicionales |
| Trabajo | 1/1 (`3038`) |
| Verificación | 1 válida (`37e1`, STOP C-02) + 1 válida excepcional autorizada por el Coordinator (`7910`), sin invocaciones adicionales |
| nc1 | 1 válida, sin reintentos |
| nc2 | 1 |
| nc3 | 0 válidas + 3 INVALID_OUTPUT (P-04) |
| nc4 | 1 (`151d`) |
| Puentes | I64-SCOPE-BRIDGE-01: 1 ejecución; I64-FREETEXT-BRIDGE-01: 1 ejecución |

`attempts` sigue en 3 de 3 (AttemptsRemaining 0) y `CorrectionsAuthorized` en false. No se reinició ningún presupuesto ni hubo Worker
de corrección. El producto sigue siendo exactamente `e0205b5a`.

## 10. Desviaciones y eventos que se conservan

- **Delta de espacios de `HostCorePath`** (secciones 4 y 5): introducido por el Worker en el RED; detectado por `37e1`; aceptado solo
  para esa línea por T3B-SCOPE-NONSEMANTIC-WHITESPACE-01.
- **Fallo de compilación del lanzador del puente de Scope:** el primer arranque del script de la sesión falló al compilar porque un
  heredoc convirtió `\n` en saltos de línea reales. No se ejecutó el verificador ni se creó el directorio de la ejecución. Se corrigió
  el script y se usó el mismo `RunId` para la única ejecución.
- **Frase fija «sin participantes» del generador de relevos de control:** habría sido falsa para el intento 1 de nc3. Se corrigió
  para leer los recuentos de los archivos de procesos; los registros de nc1 y nc2 se regeneraron y solo cambió esa frase.
- **Redacción ambigua de la orden anterior del Coordinator:** «Do not continue to controls» contradecía la continuación a nc1-nc3
  tras EXECUTION_VERIFIED / NONE. La sesión la leyó como aplicable solo si la re-verificación no daba EXECUTION_VERIFIED / NONE; el
  Coordinator aceptó esa interpretación.
- **Señal P-02 falsa** (sección 7) y la disposición del Coordinator.
- **Pruebas Release locales del Worker:** 2 fallos en UI (`PendingTextFieldTests.ShowOnADirtyField_*`) por una comprobación
  `#if DEBUG` de I-43 (`07ebcd33`), ajena a T3B; la CI corre en Debug y no tiene fallos.
- **Suites completas Debug del Worker:** se ejecutaron antes del commit GREEN sobre un árbol idéntico; la CI del SHA exacto las
  confirmó.
- **Proceso huérfano en la entrada del Worker:** un `bash.exe` de `selftest-gates.sh` de otra sesión (I-62), creado después del fin de
  la cesión y sin la ruta del worktree en su línea de órdenes: no es participante.
- **Temporizador de tope del Worker:** su `sleep.exe` sobrevivió a la parada de la tarea y la sesión lo detuvo antes de la relectura.
- **Proceso de la sesión:** las comprobaciones de huérfanos y de propiedad usan el proceso actual de la sesión (18312); el de T3A
  (7356) ya no existe.
- **Resumen TRX heredado de T3A:** contaba NotExecuted como fallidas; se corrigió para contar solo `outcome == 'Failed'`.
- **Patrón «limit reached»:** sus coincidencias en los eventos son el texto de S-11 leído de las autoridades; sin P-06.
- **Ramas vecinas:** I-65 avanzó hasta `a6fa5a35` (F4-T1 de RAM Elements) e I-62 hasta `460665ab` (documentos de F6/F7); sin solape con
  T3B y sin consumo. `main` sigue en `bb0d5522`.

## 11. Custodia

- **Manifiesto:** `custody-manifest.json` lista los 395 archivos transitorios de `artifacts/orchestration/I-64/F1-T3B-HOST-UI*/`,
  cada uno con su SHA-256 y su disposición:
  - 96 custodiados;
  - 9 custodiados con redacción;
  - 46 idénticos a un archivo custodiado;
  - 244 excluidos con motivo: 182 salidas crudas de compilación o pruebas, 42 datos operativos, 9 stderr de 39 bytes o vacíos, 8
    `events.jsonl` y 3 copias de blobs versionados.
- **Redacción:** la ruta local del worktree, la del checkout principal y la del dotnet de usuario se sustituyen por marcadores. Los
  nombres `[projects.*]` ya constaban como `[projects.<redactado>]` en los registros. Se conserva el SHA-256 de cada original.
- **Fin de línea:** los 25 archivos que escribió PowerShell con CRLF se custodian en LF, con los dos SHA-256 en el manifiesto.
- **Decisiones custodiadas:**
  - aceptación de la planificación (`R20261011T081209Z-deae/coordinator-acceptance.md`);
  - disposición sobre la parada C-02 (`R20261011T100844Z-37e1/coordinator-disposition.md`);
  - disposición sobre la señal P-02 (`F1-T3B-HOST-UI-nc3/R20261011T155834Z-41b5/p02-coordinator-disposition.md`);
  - ya versionada: decisión sobre el contrato (`coordinator-decision-t3b-contract.md`).

## 12. Siguiente

- **F1-T3C-PROFILE-DOC:** RESERVED; no se inicia.
- **F1:** sigue IN PROGRESS; F1 PASS no se declara.
- **Smoke-1:** va tras F1 PASS y el RELEASE de la ventana de host de I-52.
- Siguiente parada: **COORDINATOR_T3B_CLOSE_REVIEW_REQUIRED**.
