# I-64 F4-T4A1-PREPARE-CORE: análisis del intento 1 (parada del Worker antes del RED)

**Unidades del intento 1:**
- la planificación `R20261114T023014Z-4765`: fidelidad byte a byte PASS a la primera en las ocho colecciones y A1-A8 8/8;
- nc4 `R20261114T023355Z-3201`: solo A3 en fail;
- el Worker `R20261114T023526Z-9d4c`.

**Base:** `7f8f9aa7`, el commit de emisión del contrato, hijo de la AuthorityRevision `c76f01da`.

## 1. Hechos

**La entrega del Worker** (`worker-handoff.json`):
- WorkerStatus BLOCKED, Disposition STOP, `TriggeredStopConditions` F4T4A1-C01 y F4T4A1-C02.
- No hay commit RED ni GREEN, ni push, ni CI. En la entrada, `HEAD = remoto = 7f8f9aa7` y el árbol queda limpio en el worktree y en el checkout principal.
- La implementación de las 19 rutas del alcance quedó en un solo parche, `work-in-progress.patch` (4777 líneas, 19 archivos), con el esqueleto y la implementación juntos.

**Corridas locales del Worker con el parche aplicado:**
- la suite completa de Core en Debug: 18373 pruebas, 18371 en verde, y 2 fallos, los de §2;
- el filtro `RackCad.Tests.Workspace`: 2835 seleccionadas, 1 fallo;
- la suite completa de UI: 2112 pruebas y 0 fallos;
- el Plugin en Release: 0 errores y los 2 avisos MSB3277 de la línea base.

**Lo que el Worker no hizo:** el RED con el esqueleto aislado, el build del Plugin en Debug y los builds Release de los proyectos de prueba.

**Relevo** (`relay-record.json` del trabajo):
- 297 turnos `claude-sonnet-5-5` y effort high;
- 0 denegaciones;
- config.toml en la snapshot H a la salida y a la entrada.

**Desviación:** el Worker lanzó suites en segundo plano pese al prompt. Seis procesos no atribuibles desaparecieron en la relectura de la entrada.

## 2. Las dos pruebas fuera del alcance (comprobadas por la sesión en el código de `7f8f9aa7`)

1. **`WorkspaceDraftRegistryTests.cs:130-136`**, en `NoOperationOfTheCoreButTheReadFactoryAndCheckNowTakesAStaleBaseAlone`.
   - **Lo que fija:** recorre por reflexión los métodos públicos del núcleo y prohíbe que ninguno, salvo `CheckNow` y la fábrica `Admitted`, reciba una `DraftStaleBase` sola. El comentario dice: «The base alone is never accepted by an operation that creates, replaces or reloads a draft».
   - **El choque:** el `ConfirmBase(DraftStaleBase reread)` público de D-T4A1-01 la rompe.
   - **Por qué eximirlo:** `ConfirmBase` compara sin crear, sustituir ni recargar un borrador, igual que `CheckNow`, así que eximirlo por nombre conserva la intención de la guarda.
2. **`CustomPropertiesEnvelopeTests.cs:708`**, en `TEnv12_ComposeNull_SoloLlegaPorElParametroDeLosConstructoresDePayload`.
   - **Lo que fija:** `Assert.Equal(10, calls.Count)` sobre las llamadas a `Compose` de `src/`.
   - **El choque:** con la entrada declarada en el censo de `CustomPropertiesEnvelopeGuardTests.cs` (INV-F4T4A1-10), pasan a ser 11.
   - **Lo que no cambia:** sus otros asertos siguen siendo ciertos. La llamada nueva pasa el parámetro `source`, no es un `Build*` y no está en los archivos que nombra.

En los dos casos, cualquier implementación del contrato del intento 1 deja la prueba en rojo en el RED y en el GREEN. **La parada del Worker es correcta.**

## 3. Causa

**Es un defecto de mapeo del paquete, no del Worker.**
- §2 y D-T4A1-07 del paquete v3 enumeraron las guardas del puerto, del host y del censo de I-54 que se encontraron buscando dentro de `tests/RackCad.Tests/Workspace/` y en el archivo del censo.
- No encontraron otras dos:
  - la guarda por reflexión del núcleo, en `WorkspaceDraftRegistryTests.cs`;
  - la cuenta hermana del censo, en `CustomPropertiesEnvelopeTests.cs`.
- Tampoco las encontraron las dos revisiones independientes del paquete.

**Lección:** al añadir un miembro público al núcleo, o una llamada a una API censada, hay que buscar en **todo** `tests/`:
- el nombre del tipo o de la API, como `DraftStaleBase` y `ComposeCallsOutsideComposer`;
- las aserciones de cuentas exactas sobre ellos;
- y no solo la carpeta de la tarea ni el archivo del censo.

## 4. Un rodeo de guarda en el parche (declarado por el Worker)

- **El rodeo:** en `TryConfirmPilotBase`, el parche lee el diagnóstico de admisión de M-5 con un patrón cuya variable se llama `reread`, `read.PilotRead is { } reread && !reread.Read.IsAdmitted ? reread.Read.Admission.Diagnostic : null`, para que no lo capture la expresión `\b(?:pilot|result)\.Read\b` de la guarda del uso de la base (`WorkspaceEditIntentGuardTests.cs:635-663`).
- **El Worker lo declaró:** dijo que cumple la letra de la guarda, no su espíritu.
- **Disposición:** el contrato del intento 2 declara ese uso de forma explícita. Solo `pilot.Read.IsAdmitted` y `pilot.Read.Admission.Diagnostic`, en `TryConfirmPilotBase`, con la guarda extendida y un recuento por operación. Además prohíbe eludir una guarda con un alias o un nombre.

## 5. Lo que no se ejecutó

**La verificación del Controller no se ejecuta sobre esta entrega.**
- No hay producto que verificar: `CurrentSha = BaseSha`, sin RED, sin GREEN y sin CI.
- La parada se dispone por la decisión del Owner (§6), y la entrega se conserva como historia.
- Los controles nc1..nc3 son de una verificación EXECUTION_VERIFIED y no proceden.

## 6. Decisión del Owner y corrección

**La decisión** (`F4-DRAFTS/coordinator/owner-f4t4a1-attempt2-decision.md`, pregunta y respuesta literales): «Corrección, intento 2 (Recommended)».
- `attempts` pasa a 2 de 3.
- El contrato del intento 2, `gate-contract-attempt2.json`, parte del contrato del intento 1.

**Lo que cambia en el contrato:**
- `AllowedWriteScope` gana las dos pruebas de §2 (21 rutas).
- Se añade texto a INV-F4T4A1-04 (el uso declarado de §4), a INV-F4T4A1-10 (TEnv12 de 10 a 11) y a INV-F4T4A1-12 (la exención de `ConfirmBase` en la guarda del núcleo).
- Se añade INV-F4T4A1-15, la regla de la corrección.

**Lo que no cambia:**
- `ForbiddenWriteScope` (106);
- `RequiredTests` (2668 y 458);
- las paradas.

**La cadena de la corrección:**
- planificación y Worker nuevos;
- el Worker puede partir del parche del intento 1, revisándolo contra el contrato del intento 2;
- como no hay RED acreditado, la cadena tiene `ChainRedSha` nulo y su primer RED es el del intento 2.
