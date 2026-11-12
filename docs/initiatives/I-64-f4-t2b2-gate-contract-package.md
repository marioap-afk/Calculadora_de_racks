# I-64 — Paquete del contrato de F4-T2B2 (cableado de la intención y ciclo de vida en el host)

**Estado:** v4, EMITIBLE en cuanto se cumplan sus dos precondiciones (abajo). Lo prepara la sesión principal bajo la delegación nocturna del Owner. La v1 y la v2 dieron NO EMITIBLE y la v3, EMITIBLE TRAS CORRECCIONES; el §7 y el §9 disponen cada hallazgo.
**Base:** rama `architecture/workspace-persistente-rackcad`, en `51163211`. Todos los hechos se midieron ahí.
**Precondiciones de emisión:** (1) el cierre de F4-T2B1 tras la disposición del Owner sobre nc2 y nc3 (F4-E28); (2) la ratificación del Owner del cambio de D-T2B2-10 sobre el núcleo cerrado de F4-T1. Hasta entonces, este paquete solo se prepara y se revisa.

## 1. Autoridades

- **Freeze** (`docs/initiatives/I-64-proposal-v5.md`):
  - D-02 y O3-01: lecturas iniciadas por el usuario en contexto de aplicación, con bloqueo, en reposo y sin modal; `TopTransaction == null` en Debug;
  - D-03 e INV-22: peticiones ligadas a su sesión;
  - D-04 e IC-04: el drenaje solo encola; ninguna lectura dentro de un callback;
  - D-10, D-11, D-12 y D-13: intención, origen, «Comprobar ahora» y recarga;
  - D-17: veto de cierre, aviso de pérdida y contención;
  - INV-14, INV-20, INV-28 e INV-31.
- **Paquete de F4 v2:** obligaciones 3, 4, 10, 11, 13, 14, 15 y 18; C-F4-04.
- **Paquete de F4-T2B** (`docs/initiatives/I-64-f4-t2b-gate-contract-package.md`): §6 (esbozo) y §7.
- **Disposiciones:** F4-E09, F4-E16 (M-6), F4-E26, F4-E28 (arrastres N-1 a N-4, si el Owner cierra T2B1 así).
- **Aclaraciones:** IC-04 (redacción aceptada, `I-64-implementation-clarifications.md:73-85`).
- **Producto cerrado:** núcleo de borradores, piloto y lectura pura de la intención (`PushBackPilotIntent{Scan,Reader,Result}`).

## 2. Hallazgos del repositorio (medidos en `51163211`)

| Id | Hallazgo | Consecuencia |
|---|---|---|
| H-01 | `Plugin/Workspace` tiene 9 archivos y ninguna subcarpeta, fijados en `WorkspaceBrowserHostGuardTests.cs:38` y `WorkspaceNavigationHostTests.cs:682-691` | Ningún archivo nuevo en `Plugin/Workspace` |
| H-02 | Fuera de `Plugin/Workspace`, ningún archivo del Plugin nombra `RackCad.Application.Workspace` (`WorkspaceBridgeSourceGuardTests.cs:353-358`), ni tipos de navegación (`WorkspaceNavigationPrivacyGuardTests.cs:74-83`), ni los tipos del host (`WorkspaceHostSourceGuardTests.cs:149-160`) | El adaptador de `Plugin/Views` recibe un handle `long` y devuelve el resultado del piloto |
| H-03 | El puerto solo admite nombres de lectura (`WorkspaceHostPortGuardTests.cs:71-78`), métodos con un único `WorkspaceRequest` (`:91-104`), sin genéricos ni delegados (`:81-89`), con tipos de `System`, `System.Collections*` o de `RackCad.Application.Workspace` exacto (`:116-130`); la implementación solo delega con 4 `_core.` (`:233`) | La intención no se expone por el puerto en T2B2; T3 decide su entrada desde la UI |
| H-04 | La superficie pública de `EventBridgeCore` está fijada por reflexión (`WorkspaceBridgeFacadeSourceGuardTests.cs:204-224`), con un solo constructor de 2 parámetros (`WorkspaceBridgeHintAndDrainTests.cs:288-295`). La del puente del Plugin, también (`:91-104`) | Cualquier miembro público nuevo es una actualización de guarda por nombre exacto, declarada |
| H-05 | `OnBeginDocumentClose` siempre devuelve Allow (`EventBridgeCore.cs:321-333`); el puente lo pasa como `Action` y descarta el valor (`AutoCadEventBridge.cs:159`, `:317`). La guarda exige el literal `_core.OnBeginDocumentClose(a.Session)` (Facade `:162`) y `Contain(` o `ContainSession(` en cada `handler = (` (`WorkspaceBridgeSourceGuardTests.cs:177-184`) | El veto conserva el literal y la contención |
| H-06 | `OnDocumentToBeDestroyed` resuelve la sesión, la destruye en el registro y no emite ninguna señal con su id (`EventBridgeCore.cs:242-262`; `WorkspaceSessionRegistry.cs:30-36`). El drenaje solo corre para la sesión activa, no degradada y con el panel visible (`EventBridgeCore.cs:346-352`), así que una purga en el drenaje no ve un panel oculto, el último documento ni QUIT | La retirada es síncrona en la destrucción (D-T2B2-09), no en el drenaje |
| H-07 | `MarkDraftsPossiblyStale` se produce en `WorkspaceSession.cs:52-56` y nadie la consume (`WorkspaceHostCore.cs:174` es un comentario). Las pistas no llevan rack | Marcado por sesión, grueso, hasta el ámbito de mutación propia de T4B (C-F4-04) |
| H-08 | `WorkspaceHostCore` no posee sesiones (`:31-41`); la autorización va por `_bridge.Authorize` (`:205-206`; `WorkspaceSessionRegistry.cs:38-46`). Las acciones de navegación siguen el orden documento activo → reposo y sin comando → `using LockDocument` → autorización → ejecución (`WorkspaceNavigationHost.cs:140-157`, fijado en `WorkspaceNavigationHostTests.cs:455-475`) | La lectura de la intención reutiliza ese orden |
| H-09 | `NavigationTargets.Revalidate` relee y reemplaza la entrada (`NavigationTargets.cs:108-129`); `new NavigationTargets(` solo en `WorkspaceNavigationHost.cs` (`WorkspaceNavigationHostTests.cs:414-424`); `.ReadDefinitionFacts(` solo en `NavigationTargets.cs` (`WorkspaceNavigationAuthorityTests.cs:96-106`) | El origen sale de objetivos revalidados en la misma operación |
| H-10 | `WorkspaceSelectionReader` captura solo `LockViolation` (`:109-113`) y comprueba `TopTransaction` solo después y solo en éxito (`:115`) | El adaptador mejora el precedente: antes y después, con fallo tipado |
| H-11 | `DraftClosePolicy.EvaluateClose` no tiene `try` (`DraftClosePolicy.cs:44-50`); `DraftRegistry.RetireSession` calcula la pérdida antes de marcar (`DraftRegistry.cs:186-201`) | M-6: el host contiene la excepción y veta |
| H-12 | `WorkspaceHostSourceGuardTests.cs` debe tener exactamente 45 pruebas (`WorkspacePaletteHostSourceGuardTests.cs:657`) | Las guardas nuevas van en archivos nuevos |
| H-13 | El drenaje actual también lee (selección y resumen) dentro de la pila del Idle (`WorkspaceHostCore.cs:155-171`; `WorkspaceController.cs:139-163`) | T2C; T2B2 no añade ninguna lectura al drenaje |
| H-14 | La CI compila el Plugin en Release (`ci.yml:254-317`) y no ejecuta pruebas del Plugin; las guardas de texto corren en Linux | Lo del Plugin se verifica por texto; la lógica, en Application con dobles |

## 3. Descomposición, orden y presupuesto

| Tarea | Alcance | Cambios de guarda o de pruebas existentes |
|---|---|---|
| **F4-T2B2A-INTENT-WIRING** (primero) | Proyección aditiva; adaptador de lectura y lector del testigo en `Plugin/Views`; costura Core–Plugin por asignación; elección del origen (función pura); registro de borradores del host; entrada de la intención y «Comprobar ahora», por acción del usuario; arrastres T2B1/N-1 a T2B1/N-4 | Ninguna guarda existente cambia (las nuevas van en archivos nuevos). Solo por los arrastres de D-T2B2-07, cambian pruebas existentes de `WorkspaceDraftPilotIntentReadTests.cs`, `WorkspaceDraftPilotIntentStaleBaseTests.cs` y `WorkspaceDraftPilotIntentGuardTests.cs` |
| **F4-T2B2B-LIFECYCLE** (después) | Asesor de ciclo de vida (veto y retirada síncrona); descarte y retirada contenidos en `DraftRegistry` (M-6); marcado de obsolescencia (IC-08); recarga perezosa por acción del usuario; registro técnico del aviso de pérdida | (1) un nombre exacto más en la lista de miembros públicos de `EventBridgeCore` (`WorkspaceBridgeFacadeSourceGuardTests.cs:204-224`); (2) la prueba de atomicidad de `RetireSession` (`WorkspaceDraftRegistryTests.cs:363-389`) pasa a la semántica nueva de D-T2B2-10 |

- **Por qué se divide:** cada parte toca superficies congeladas distintas, y así cada revisión aísla sus cambios.
- **Presupuesto:** cada subtarea empieza en 1 de 3 (F4-E09). La capacidad de T2B queda en 9 intentos (T2B1, T2B2A y T2B2B); se avisa al Owner. El agotamiento de cualquiera de las tres es S-11 y STOP de F4-T2.
- **Registro:** la división y las reasignaciones se registran en F4-E29 al emitir.

## 4. Decisiones de T2B2A

### D-T2B2-01. Proyección aditiva
`RackSiblingDrawingScan` gana `RawEnvelopes` (`internal`, diccionario ordinal de solo lectura) con el mismo `json` que `Traverse` lee con `RackBlockData.Read` para cada definición con texto. Ninguna firma ni consumidor cambia; las guardas que cuentan llamadas son por archivo y no miran `RackSiblingScan.cs`.

### D-T2B2-02. Adaptador `src/RackCad.Plugin/Views/RackPilotIntentAdapter.cs` (nuevo, `internal static`)
- `LoadCatalog()`: envuelve `LateralHeaderDrawService.LoadCatalog()` con fallo tipado; se llama **fuera** del bloqueo.
- `Read(Document, string rackId, long originDefinitionHandle, string? witnessKey, string? witnessText, RackCatalog)`, dentro del bloqueo de la operación:
  - Debug `TopTransaction == null` antes y después;
  - una sola llamada a `RackSiblingScan.CaptureDrawing(` en el nivel superior;
  - clave de origen con el helper único del archivo, `private static Handle ToHandle(long value) => new Handle(value);`, como `ToHandle(originDefinitionHandle).ToString()`; handle 0 → clave vacía (regla 2 del lector);
  - un testigo con la clave vacía da un fallo tipado, sin construir la entrada (el constructor de T2B1 lanzaría);
  - arma `PushBackPilotIntentScan` con los hechos, `RawEnvelopes` y `"RackSiblingScan/CaptureDrawing"`, y llama a `PushBackPilotIntentReader.Read`;
  - captura `Autodesk.AutoCAD.Runtime.Exception` y `System.Exception` y devuelve un fallo tipado.
- `ReadPreviewWitness(Document, long definitionHandle)`: lectura ligera con su propio `using` de bloqueo y transacción, sin `Commit`, que resuelve la definición con el mismo helper `ToHandle`; para T3.
- El resultado es un tipo propio `internal` del archivo: el resultado del piloto o el motivo del fallo.

### D-T2B2-03. Costura Core–Plugin (I-1, N-1, N-8)
- **Interfaz** `src/RackCad.Application/Workspace/IWorkspaceEditIntentReader.cs` (nueva). Un método con claves y valores:
  - entrada: sesión, RackId, handle de la referencia elegida (o null) y testigo (clave y texto, o null);
  - salida: `EditIntentReadResult`, un tipo de valor de `WorkspaceEditIntent.cs` con un estado (Read, NoActiveDocument, Busy, Rejected, Failed), el motivo y, si es Read, el `PushBackPilotIntentResult`.
- **Implementación:** una clase anidada de `src/RackCad.Plugin/Workspace/WorkspaceNavigationHost.cs`, expuesta por una propiedad `internal`. Hace, en este orden:
  1. documento activo, o NoActiveDocument;
  2. sin comando y en reposo, o Busy;
  3. `LoadCatalog()`, fuera del bloqueo; un fallo da Failed;
  4. `using LockDocument`;
  5. autorización con el id nativo del activo (INV-22), o Rejected;
  6. `Revalidate` de los objetivos y elección del origen con la función pura de D-T2B2-04;
  7. `RackPilotIntentAdapter.Read`.
  
  Una guarda de orden lo fija como `WorkspaceNavigationHostTests.cs:455-475`. Los objetivos pasan a ser un campo del host de navegación.
- **Lado Core:** `WorkspaceHostCore` comprueba antes de llamar al lector, sin leer nada:
  - modal RackCad o comando activo → `Waiting`;
  - preautorización en memoria → `Rejected`.
- **Cableado por asignación, no por constructor:** `WorkspaceHostCore.SetEditIntentReader(IWorkspaceEditIntentReader)`. El controlador lo llama en el getter `Navigation`, justo después de crear el host de navegación. Así no cambian el constructor del controlador (`WorkspaceNavigationHostTests.cs:673`) ni los argumentos del host (`WorkspaceNavigationEchoTests.cs:311`). Sin lector asignado, la intención da `Unavailable`, sin leer.
- `WorkspaceHostCore` no nombra `NavigationTargets` (`WorkspaceNavigationAuthorityTests.cs:169-172`).

### D-T2B2-04. Elección del origen (D-11, N-7)
- Una función pura `static long ChooseOriginDefinition(NavigationTargetSet set, long? chosenReference)` en `WorkspaceEditIntent.cs`, probada en Core:
  - con referencia elegida que sigue en los objetivos → su `DefinitionHandle`;
  - con referencia elegida que ya no está → 0 (el lector da `OriginMissingDiagnostic`). Una referencia que ahora es de otro rack también da 0, y se declara que el diagnóstico dice «ya no está», no «pertenece a otro rack» (N-13);
  - sin referencia → `Primary`, o 0 si no hay objetivos.
- La conversión a la clave del barrido usa el helper `ToHandle` del adaptador, que contiene la única `new Handle(` del archivo (guarda).

### D-T2B2-05. Registro de borradores del host y acoplamiento declarado (I-2, N-2, N-9)
- `WorkspaceHostCore` posee **un** `DraftRegistry` y su `DraftClosePolicy` para todas las sesiones (un único hueco de Aplicando, global). El «por sesión» del paquete de T2B (`:255`) se lee como claves (sesión, RackId).
- **Acoplamiento declarado:** en `Application/Workspace`, **como máximo** `WorkspaceHostCore.cs`, `IWorkspaceEditIntentReader.cs` y `WorkspaceEditIntent.cs` nombran el piloto, entendido como el texto `RackCad.Application.Systems.PushBack.Pilot` o cualquier identificador `PushBackPilot\w*`. Lo fija una guarda con esa lista (máximo, no conjunto exacto). En el Plugin, `WorkspaceNavigationHost.cs` maneja el resultado del piloto que devuelve el adaptador, y también se declara. Generalizarlo es residual de F5.
- `WorkspaceEditIntent.cs` define su propio estado del testigo, con `NotEvaluated` para los resultados sin lectura (nunca `Unchanged`), y lo traduce desde el del piloto.
- **Marcar y retirar** usan `RackIdsOf(session)` y `Find<PushBackPilotDraftState>`; no hay estado duplicado.
- **R2:** ningún campo del host es de los tipos del piloto ni del resultado de la lectura.
- El campo del lector nuevo no se llama `_reader` (`WorkspaceHostSourceGuardTests.cs:337`).

### D-T2B2-06. Entrada de la intención y «Comprobar ahora» (reasignación de la entrada de T3, I-9, N-6)
- **`WorkspaceHostCore`** expone dos operaciones de valor, con firmas fijadas aquí. No llevan tipos de borrador ni del piloto en la firma:
  - **intención:** `EditIntentOutcome TryBeginPilotEdit(EditIntentRequest)`;
  - **«Comprobar ahora»:** `EditCheckOutcome TryCheckPilotEdit(EditIntentRequest)`.
- `EditIntentRequest` lleva la sesión, el RackId, la referencia elegida (o null) y el testigo (o null).
- **Estados de la intención:**
  - `Unavailable`: no hay lector asignado;
  - `Waiting`: un modal, un comando, sin reposo o sin documento activo (la espera de D-10);
  - `Rejected`: INV-22, sin efectos y sin lectura del DWG;
  - `AlreadyOpen`: ya hay un borrador; no se lee;
  - `NotAdmitted`: con el diagnóstico, el detalle y la indicación de RACKEDITAR;
  - `Admitted`: `Begin` con el comparador y el copiador del piloto;
  - `Failed`: un fallo del host en la lectura (D-17).
  
  Cada estado lleva también el estado del testigo.
- **Orden y estados en el core**, sin lecturas antes del paso 6:

  | Paso | Comprobación | Resultado si falla |
  |---|---|---|
  | 1 | RackId en blanco | `NotAdmitted` con `NoIdentityDiagnostic`, sin tocar el registro (que lanzaría) |
  | 2 | lector asignado | `Unavailable` |
  | 3 | sonda: documento activo (id nativo presente) | `Waiting` |
  | 4 | modal RackCad, comando activo o sin reposo | `Waiting` |
  | 5 | preautorización en memoria con el id nativo del activo | `Rejected` |
  | 6 | intención: borrador existente → `AlreadyOpen`; «Comprobar ahora»: borrador ausente → `NotFound` o no Desconocido → `NotApplicable`; recarga: borrador ausente → `NotFound` o sin `NeedsLazyReload` → `NotNeeded` | (sin lectura) |
  | 7 | lectura por la interfaz: NoActiveDocument o Busy → `Waiting`; Rejected → `Rejected`; Failed → `Failed` | — |
  | 8 | intención: `Begin` → `Admitted`, `NotAdmitted` o `SessionRetired` → `Rejected`; «Comprobar ahora»: `CheckNow` → `Conflict` o `Recovered`; recarga: `DiscardAndReload` → `Reloaded` o `NotAdmitted` | — |

  Toda excepción dentro de una operación se contiene y da `Failed` (D-17); la sesión **no** se degrada por ello (no pasa a `_faulted`), porque es una acción del usuario y no el drenaje. `EditCheckOutcome` tiene los estados `Unavailable`, `Waiting`, `Rejected`, `NotFound`, `NotApplicable`, `Conflict`, `Recovered` y `Failed`.
- **«Comprobar ahora»** relee y llama a `CheckNow(result.StaleBase)`.
  - Solo actúa en Desconocido por `CommitThrew`, que no se alcanza en el producto hasta T4B.
  - Se prueba con un accesor `internal` del registro **en `WorkspaceHostCore`** (hay `InternalsVisibleTo` hacia las pruebas, `RackCad.Application.csproj:11`; `DraftRegistry.cs` no se toca en T2B2A). La prueba lleva el borrador a Desconocido con la API pública del núcleo (`TryBeginApply` y `CompleteApply(DraftApplyOutcome<…>.CommitThrew(…))`), y así se ven Conflict y Recovered a través del host. Sin Desconocido, la prueba espera NotApplicable.
- **Reasignación (F4-E29):** el paquete de T2B daba a T2B2 «el punto de entrada para T3». T2B2A entrega estas operaciones con firma fija, y **T3** las expone a la UI con su propio puerto y sus propios cambios de guarda (H-03). Hasta T3 son código oscuro, declarado.
- **Arrastre a T3:** su entrada asegura que el lector está asignado (acceder a `Navigation` antes de la intención; hoy lo garantiza la paleta, `WorkspaceBrowserHostGuardTests.cs:211-215`), para no quedar en `Unavailable`.

### D-T2B2-07. Arrastres de T2B1 (condicionados a la disposición del Owner; N-17). Se citan como T2B1/N-1 a T2B1/N-4 para no confundirlos con los hallazgos N-1..N-17 de la re-revisión de la v2
| Arrastre | Archivo |
|---|---|
| N-1 (comentario o aserción de la prueba de los estados del dibujo) | `tests/RackCad.Tests/Workspace/WorkspaceDraftPilotIntentStaleBaseTests.cs` |
| N-2 (prueba del par 9,10 con un Kind `PushBack` con otras mayúsculas) | `tests/RackCad.Tests/Workspace/WorkspaceDraftPilotIntentReadTests.cs` |
| N-3 (`Detail` null con un Kind vacío o en blanco) | `src/RackCad.Application/Systems/PushBack/Pilot/PushBackPilotIntentReader.cs` y su prueba en `WorkspaceDraftPilotIntentReadTests.cs` |
| N-4 (nombres y comentarios) | `WorkspaceDraftPilotIntentReadTests.cs` y `tests/RackCad.Tests/Workspace/WorkspaceDraftPilotIntentGuardTests.cs` (comentario de la guarda de hilos) |

Si el Owner dispone otra cosa para T2B1, se retiran del alcance antes de emitir.

### D-T2B2-08. Pruebas y guardas de T2B2A
- **Core, con un doble de la interfaz y el rig existente** (sin cambiar `WorkspaceHostTestSupport.cs`):
  - obligación 4: una intención = una operación de lectura; `Revalidate` y `CaptureDrawing`, dentro del mismo bloqueo, cuentan como una lectura lógica y su coste se mide en Smoke-2;
  - INV-22: A con B activo → `Rejected` y cero lecturas; id nativo reciclado → `Rejected`;
  - `Waiting`, `Unavailable` y `AlreadyOpen`, sin lectura;
  - una intención admitida crea el borrador y una no admitida no;
  - INV-27: testigo cambiado → `Changed` y sin pulsación aplicada;
  - «Comprobar ahora»: Conflict y Recovered a través del host (D-T2B2-06);
  - ninguna operación se llama desde el drenaje;
  - la función de elección del origen (D-T2B2-04), con todos sus casos;
  - se nombran como evidencia: la **obligación 11** (la comprobación relee datos authored: «Comprobar ahora» con el doble), la **obligación 13** (la sesión A nunca opera sobre B: las pruebas de INV-22) y **R6** (sin escritura en el DWG: las guardas del adaptador y del host).
- **Guardas nuevas** en `tests/RackCad.Tests/Workspace/WorkspaceEditIntentGuardTests.cs`:
  - **adaptador:**
    - INV-14 completo: sin campos de `Document`, `Transaction` ni `DBObject`; bloqueo y transacciones solo dentro de `using`; `TopTransaction` comprobada antes y después; `LockViolation` capturada;
    - ninguna escritura: `ForWrite`, `UpgradeOpen`, `AppendEntity`, `Erase(`, `.Commit(`, `RackBlockData.Write`, `SetImpliedSelection`, vista;
    - una sola llamada a `CaptureDrawing(` y una sola `new Handle(`, dentro del helper `ToHandle`;
    - no lee el registro de variables (`ProjectVariablesData`);
    - no nombra `RackCad.Application.Workspace` (prefijo que veta también `.Drafts`) ni tipos de navegación;
    - no construye `RackEmbedDocument`;
  - **orden del método nuevo** del host de navegación: `LoadCatalog` antes del bloqueo; autorización antes de `Revalidate`; `Revalidate` antes del adaptador;
  - **host:** sin campos de los tipos del piloto ni del resultado (R2); la lista exacta de D-T2B2-05;
  - **obligación 10:** la base de un resultado solo llega a `Begin`, `CheckNow` o `DiscardAndReload` (guarda de texto, como M-9);
  - **obligación 3, extendida:** los cuerpos de los métodos de resumen del host de navegación y los cuatro archivos de preview no nombran la entrada nueva.
- **Trampas conocidas:**
  - `\bRegistry\b` (`WorkspaceHostSourceGuardTests.cs:446`);
  - `Provider`;
  - un solo `RackLog.Exception` en el host de navegación (`WorkspaceNavigationHostTests.cs:527`);
  - sin `async`, `Task`, `Dispatcher` ni `yield` en métodos con «Draft» en el nombre (`WorkspaceModelBoundaryTests.cs:101`);
  - el constructor del controlador no puede contener `.Start(` (`WorkspaceHostSourceGuardTests.cs:143-146`);
  - `\.Result\b` está prohibido en todo `Plugin/Workspace` (`WorkspaceHostSourceGuardTests.cs:427-428`), y `GetResult`, `StartNew` y `ContinueWith` en el host de navegación y en el controlador (`:417`; `WorkspaceNavigationHostTests.cs:443-449`): ninguna propiedad de los tipos nuevos que se lea en esos archivos se llama `Result` (por ejemplo, `PilotRead`);
  - ningún `+=` ni `-=` en el archivo del host de navegación, ni siquiera aritmético (`WorkspaceNavigationHostTests.cs:529`; `WorkspaceBrowserHostGuardTests.cs:105`);
  - un solo `new WorkspaceNavigationFacts(` (`WorkspaceBrowserHostGuardTests.cs:99`): la clase anidada reutiliza el campo de objetivos;
  - ningún tipo anidado explícito con `Draft`, `Editor`, `Subscription` o `Tab` (incluido `Stable`) en el nombre (`WorkspaceModelBoundaryTests.cs:101-102`; `WorkspaceBrowserBoundaryTests.cs:285-294`);
  - el texto `Transaction`, `\bDatabase\b` y `\bCommit\(` está prohibido en todo `Application/Workspace` (`WorkspaceModelBoundaryTests.cs:41-49`).

## 5. Decisiones de T2B2B

### D-T2B2-09. Asesor de ciclo de vida (B-1, I-3, N-5, N-11)
- **Interfaz** `src/RackCad.Application/Workspace/IWorkspaceLifecycleAdvisor.cs` (nueva), con dos métodos:
  - `CloseDecision AdviseClose(SessionId)`;
  - `void SessionDestroyed(SessionId)`.
- **`EventBridgeCore` gana UN miembro público:** `SetLifecycleAdvisor(IWorkspaceLifecycleAdvisor)`. Es el cambio (1) del §3; el constructor no cambia.
- **`OnBeginDocumentClose`:**
  - sin asesor, Allow (las pruebas actuales siguen valiendo);
  - con asesor, su decisión;
  - cualquier excepción del asesor da Veto (M-6), contenida en el core.
- **`OnDocumentToBeDestroyed`:** llama a `SessionDestroyed(id)` de forma síncrona y solo en memoria, para toda sesión destruida (activa o no, T2H-09). Reglas:
  - va **después** de `_registry.Destroy(id)` y de limpiar `_active`, y en su **propio** `try/catch`, que registra con `logFault`;
  - una excepción del asesor no impide la destrucción, ni la limpieza de la sesión activa, ni la sincronización siguiente;
  - el asesor no vuelve a llamar al puente.
- **Excepción acotada a D-04:** retirar en memoria dentro de `DocumentToBeDestroyed` va más allá de «solo encolan», igual que la decisión síncrona del veto. Se declara como tal.
- **Residual declarado:** `Stop()` destruye sesiones sin llamar al asesor. Hoy `Dispose` no tiene llamador; queda para T3 o F7.
- **`WorkspaceHostCore` implementa el asesor:**
  - `AdviseClose` = `DraftClosePolicy.EvaluateClose`;
  - `SessionDestroyed` = `RetireSession` y encolar el `DraftLossNotice`.
- **Cableado:**
  - `AutoCadEventBridge.cs` gana un método `internal` que reenvía al core (los `internal` no cuentan en `:91-104`); el controlador lo llama con el host al crearlo, sin `+=` nuevos;
  - el manejador de cierre conserva, en la misma línea, `handler = (`, `ContainSession(` y el literal `_core.OnBeginDocumentClose(a.Session)`, y llama a `e.Veto()` cuando la decisión es Veto. Se mantienen los 16 `+=`, 16 `-=` y 16 manejadores.

### D-T2B2-10. Descarte y retirada contenidos (I-4, N-3; reabre el núcleo)
- **Problema:** si el comparador lanza siempre, `EvaluateClose` → Veto. Además, `Discard`, `DiscardAll` y `RetireSession` evalúan `IsDirty` antes de quitar nada y lanzan, así que el usuario no podría cerrar AutoCAD.
- **Cambio mínimo en `DraftRegistry.cs`:** en `Discard`, `DiscardAll` y `RetireSession`, una excepción al evaluar un borrador lo cuenta como en riesgo (Dirty), y la operación continúa y nunca lanza.
- **Cambia una garantía cerrada de F4-T1:** la atomicidad de `RetireSession` ante un comparador que lanza. Viene de INV-F4T1-23 («calcula la lista de pérdida antes de marcar», `gate-contract-attempt3.json`, Invariants[22]) y de M-6 de la revisión independiente de F4-T1 (`F4-T1-DRAFT-CORE/1/R20261110T184730Z-e455/independent-review.md:93-96`); su prueba está en la sección de INV-F4T1-12 (`WorkspaceDraftRegistryTests.cs:325`, prueba `:363-389`). Esa prueba pasa en el RED a la semántica nueva, escrita con `Record.Exception` para que falle por aserción: el borrador cuenta como perdido y la sesión queda retirada. Es el cambio (2) del §3.
  - **Alcance exacto:** «nunca lanza **por el comparador**»; la validación de argumentos sigue igual (`WorkspaceDraftRegistryTests.cs:153`).
  - **Razón:** en una destrucción el documento ya no existe; conservar sus borradores no protege nada y los deja huérfanos.
- **Requiere la ratificación del Owner** al emitir, porque cambia una garantía de un producto cerrado; la ratificación cita INV-F4T1-23, INV-F4T1-12 y M-6.
- Pruebas nuevas, con `Record.Exception`, para que fallen por aserción en el RED: comparador que lanza → Veto → descarte → Allow; y retirada con un comparador que lanza.

### D-T2B2-11. Marcado de obsolescencia (IC-08 nueva; N-10)
- Cuando el drenaje de una sesión produce `MarkDraftsPossiblyStale`, el host marca todos los borradores del piloto de esa sesión.
- **Dónde:** antes de los retornos tempranos de `OnDrained` (`WorkspaceHostCore.cs:140`, `:143`, `:161`, `:168`), con su prueba.
- **Texto de la IC-08** (lo escribe la sesión al emitir):
  - el marcado es grueso por sesión, porque las pistas no llevan rack;
  - es solo una pista: sin Conflicto y sin lectura;
  - `MarkDraftsPossiblyStale` nace de cualquier cambio de base de datos y de cualquier `CommandEnded`, también de comandos que no modifican nada (`WorkspaceSession.cs:52-56`);
  - **obligación vinculante para T4B:** las pistas que produce la mutación propia de Actualizar no marcan los borradores. El mecanismo lo fija T4B; debe respetar que las pistas se agrupan por sesión y por tipo (`WorkspaceSession.cs:26`, `:46-57`) y se drenan después del ámbito, y no puede suprimir `InvalidateNavigationCaches`. El oráculo es «tras Actualizar A, el borrador B no queda marcado» (INV-28);
  - Smoke-2 y Smoke-PERF miden la frecuencia del marcado y el coste de las recargas que provoca.

### D-T2B2-12. Recarga perezosa
`EditIntentOutcome TryReloadPilotEdit(EditIntentRequest)` sigue el mismo camino que la intención. Si `NeedsLazyReload`, llama a `DiscardAndReload(result.Read)`. Solo por acción del usuario y nunca desde el drenaje.

### D-T2B2-13. Visibilidad de la pérdida (B-1, N-4, N-12)
- Hasta T3 no hay superficie de edición: ningún usuario puede crear un borrador, así que **no puede haber pérdidas reales antes de T3**.
- **Registro técnico, sin cambiar guardas:** `WorkspaceHostCore.SessionDestroyed` solo actúa si `HasLoss`: encola el `DraftLossNotice` y lo registra en el acto con su `logFault` inyectado, con el contexto `"WorkspaceHostCore.DraftLoss"` y una `InvalidOperationException` cuyo mensaje lista los RackIds perdidos (en el Plugin es el `LogFault` del controlador, que ya usa `RackLog`; `WorkspaceHostSourceGuardTests.cs:494-506`). Se declara que la pérdida queda registrada con nivel de excepción. El disparador es la propia llamada síncrona del asesor. Sin pérdida no se encola nada, para que la cola, sin consumidor hasta T3, no crezca con cada documento cerrado.
- **T3** lleva el aviso al panel y a la línea de comandos, con sus textos y su cambio de guarda para `WriteMessage` (`:84-89`).
- **QUIT** es una INFERENCE: que `DocumentToBeDestroyed` llegue en QUIT y que el registro sobreviva no está medido. Va al oráculo de Smoke-2 (S2-06).

### D-T2B2-14. Pruebas de T2B2B
- **Veto:**
  - con un borrador Dirty o con texto pendiente;
  - intentos sucesivos vetados (repetir no es consentir);
  - Allow sin borradores;
  - asesor que lanza → Veto;
  - tras descartar, el cierre siguiente procede;
  - el asesor no llama a ningún lector.
- **Degradación:** la secuencia «limpia → degradada → edición → dirty → cierre → veto»; el cierre sobrevive a la degradación (`BridgeSource.cs:86-87`).
- **Destrucción:**
  - retirada y aviso (en cola y en el registro) de una sesión destruida activa y de una no activa;
  - asesor que lanza en la destrucción → la sesión queda destruida, un id reciclado da una sesión nueva sin borradores y `Authorize` del id viejo da `RejectedSessionDestroyed`.
- **Marcado:** todos los borradores de la sesión y ninguno de otra; también con un eco y un cambio de base de datos en el mismo drenaje.
- **Recarga:** solo con `NeedsLazyReload`.
- **Descarte y retirada contenidos** con un comparador que lanza.
- **Obligaciones:** 5 (cero escrituras), 14, 15 y 18.

## 6. Contratos propuestos

**Régimen** (como T2B1):
- RED inerte que compila y falla por aserción;
- GREEN sin tocar `tests/`;
- CI del SHA exacto;
- verificación del Controller;
- revisión directa de la sesión y revisión independiente;
- nc4 en la primera delegación de cada cadena;
- nc1..nc3 tras la verificación, con el oráculo de nc2 y nc3 que disponga el Owner (F4-E28).

**Líneas base:**
- **T2B2A:** las de la CI del SHA con el que cierre T2B1. Si cierra sobre `fe7f136b` (corrida `43190415649`), son `RackCad.Tests.Workspace` 1592 y `RackCad.UI.Tests.Workspace` 248, medidas por la sesión en sus TRX; si no, se miden al emitir.
- **T2B2B:** las de la CI del GREEN de T2B2A, medidas al emitir.
- En las dos: Core `ExpectRed` true; UI `ExpectRed` false.

**`AllowedWriteScope` de T2B2A** (lista blanca exacta):
- `src/RackCad.Plugin/Views/RackSiblingScan.cs`
- `src/RackCad.Plugin/Views/RackPilotIntentAdapter.cs` (nuevo)
- `src/RackCad.Plugin/Workspace/WorkspaceNavigationHost.cs`
- `src/RackCad.Plugin/Workspace/WorkspaceController.cs`
- `src/RackCad.Application/Workspace/WorkspaceHostCore.cs`
- `src/RackCad.Application/Workspace/IWorkspaceEditIntentReader.cs` (nuevo)
- `src/RackCad.Application/Workspace/WorkspaceEditIntent.cs` (nuevo)
- `tests/RackCad.Tests/Workspace/WorkspaceEditIntentHostTests.cs` (nuevo)
- `tests/RackCad.Tests/Workspace/WorkspaceEditIntentGuardTests.cs` (nuevo)
- solo para los arrastres (D-T2B2-07):
  - `src/RackCad.Application/Systems/PushBack/Pilot/PushBackPilotIntentReader.cs`
  - `tests/RackCad.Tests/Workspace/WorkspaceDraftPilotIntentReadTests.cs`
  - `tests/RackCad.Tests/Workspace/WorkspaceDraftPilotIntentStaleBaseTests.cs`
  - `tests/RackCad.Tests/Workspace/WorkspaceDraftPilotIntentGuardTests.cs`

**`AllowedWriteScope` de T2B2B:**
- `src/RackCad.Application/Workspace/EventBridgeCore.cs`
- `src/RackCad.Application/Workspace/IWorkspaceLifecycleAdvisor.cs` (nuevo)
- `src/RackCad.Application/Workspace/WorkspaceHostCore.cs`
- `src/RackCad.Application/Workspace/WorkspaceEditIntent.cs`
- `src/RackCad.Application/Workspace/Drafts/DraftRegistry.cs` (solo D-T2B2-10, con ratificación)
- `src/RackCad.Plugin/Workspace/AutoCadEventBridge.cs`
- `src/RackCad.Plugin/Workspace/WorkspaceController.cs`
- `tests/RackCad.Tests/Workspace/WorkspaceBridgeFacadeSourceGuardTests.cs` (solo el nombre exacto nuevo)
- `tests/RackCad.Tests/Workspace/WorkspaceDraftRegistryTests.cs` (solo la prueba de `:363-389`, D-T2B2-10)
- `tests/RackCad.Tests/Workspace/WorkspaceLifecycleHostTests.cs` (nuevo)
- `tests/RackCad.Tests/Workspace/WorkspaceLifecycleGuardTests.cs` (nuevo)
- `tests/RackCad.Tests/Workspace/WorkspaceDraftRegistryContainmentTests.cs` (nuevo)

**`ForbiddenWriteScope`:** solo prefijos seguros (F4-E21), generados desde el árbol.

**Invariantes:** se redactan al emitir, uno por decisión (D-T2B2-01 a 08 para T2B2A; 09 a 14 para T2B2B), más el de cobertura RED/GREEN y el de «ninguna guarda ni prueba existente cambia, salvo las declaradas en el §3».

**Paradas**, además de las generales:
- hace falta un archivo nuevo en `Plugin/Workspace`;
- `WorkspaceHostCore` necesitaría nombrar `NavigationTargets`;
- hace falta tocar `Drafts/` fuera de D-T2B2-10;
- el veto exigiría cambiar el constructor de `EventBridgeCore`;
- una lectura tendría que ocurrir dentro del drenaje o del Idle;
- una guarda o prueba existente tendría que cambiar fuera de las declaradas.

## 7. Disposición de las revisiones

### Revisión de la v1 (NO EMITIBLE)

| Hallazgo | Disposición |
|---|---|
| B-1 pérdida silenciosa | D-T2B2-09 (retirada síncrona) y D-T2B2-13 (registro ahora; panel y línea de comandos en T3) |
| I-1 costura | D-T2B2-03 (asignación, clase anidada y reparto Core/Plugin) |
| I-2 acoplamiento y marcado | D-T2B2-05 |
| I-3 fachada del asesor | D-T2B2-09 |
| I-4 veto permanente | D-T2B2-10 |
| I-5 marcado grueso | D-T2B2-11 |
| I-6 origen | D-T2B2-04 |
| I-7 pruebas | D-T2B2-08 y D-T2B2-14 |
| I-8 guardas del adaptador | D-T2B2-08 |
| I-9 entrada para T3 | D-T2B2-06 |
| I-10 contratos | §6 |

### Re-revisión de la v2 (NO EMITIBLE: 6 importantes y 11 menores)

| Hallazgo | Disposición en la v3 |
|---|---|
| N-1 cableado por constructor | D-T2B2-03: asignación desde el getter `Navigation`; estado `Unavailable` |
| N-2 la interfaz nombra el piloto | D-T2B2-05: lista exacta de tres archivos y estado propio del testigo |
| N-3 invierte una prueba cerrada | D-T2B2-10 y §3: cambio declarado de INV-F4T1-12, con su prueba en el alcance; `Discard` incluido |
| N-4 registro sin guarda ni disparador | D-T2B2-13: `logFault` del host en la propia llamada síncrona |
| N-5 orden de la destrucción | D-T2B2-09: después de `Destroy`, en su propio `try`, y prueba de id reciclado |
| N-7 prueba del origen | D-T2B2-04 y D-T2B2-08: función pura y guardas de `new Handle(` y de orden |
| N-6 «Comprobar ahora» por el host | D-T2B2-06: accesor `internal` |
| N-8 reparto y mapa de fallos | D-T2B2-03 y D-T2B2-06 |
| N-9 conjunto propio | D-T2B2-05: `RackIdsOf` y `Find` |
| N-10 redacción de IC-08 | D-T2B2-11 |
| N-11 excepción a D-04 y `Stop()` | D-T2B2-09 |
| N-12 QUIT | D-T2B2-13: INFERENCE y Smoke-2 S2-06 |
| N-13 diagnóstico del origen | D-T2B2-04: declarado |
| N-14 líneas base | §6 |
| N-15 trampas | D-T2B2-05 y D-T2B2-08 |
| N-16 guardas del adaptador | D-T2B2-08 |
| N-17 archivos de los arrastres | D-T2B2-07 |

## 8. Puntos para la re-revisión

1. ¿Cierra la v3 los seis importantes de la v2 y los parciales de la v1, sin abrir otros?
2. ¿Caben el cableado por asignación, la clase anidada con su propiedad `internal`, el campo de objetivos del host de navegación y la llamada en el getter `Navigation` en las guardas reales?
3. ¿Es aceptable el cambio de INV-F4T1-12 tal como se declara?
4. ¿Son emitibles las listas de `AllowedWriteScope`?

## 9. Re-revisión de la v3 y disposición

**Veredicto:** EMITIBLE TRAS CORRECCIONES. Cierra los seis importantes de la v2 (N-1, N-2, N-3, N-4, N-5 y N-7) y casi todos los menores; no abre bloqueantes ni importantes. Comprobó contra las guardas reales que caben el cableado por asignación, la clase anidada con su propiedad `internal`, la llamada en el getter `Navigation`, `NavigationTargetSet` en `Application/Workspace`, el registro con el `logFault` del host y el asesor tras `Destroy`.

| Hallazgo | Disposición en la v4 |
|---|---|
| NV-1 los arrastres cambian pruebas existentes | §3: declarados en la fila de T2B2A |
| NV-2 dos `new Handle(` en el adaptador | D-T2B2-02, 04 y 08: helper único `ToHandle` |
| NV-3 trampas que faltaban | D-T2B2-08: `.Result`, `+=`, `new WorkspaceNavigationFacts(`, tipos anidados y textos vetados de `Application/Workspace` |
| NV-4 orden y mapa del core | D-T2B2-06: tabla de pasos y estados, contención sin degradar y estados de `EditCheckOutcome` |
| NV-5 referencias de D-T2B2-10 | D-T2B2-10: INV-F4T1-23, INV-F4T1-12 y M-6; «nunca lanza por el comparador»; RED con `Record.Exception` |
| NV-6 registro con `logFault` | D-T2B2-13: `InvalidOperationException`, contexto fijo, solo con pérdida |
| NV-7 texto de IC-08 | D-T2B2-11: origen de la pista, agrupación por tipo y mecanismo para T4B |
| NV-8 obligaciones 11, 13 y R6 | D-T2B2-08: nombradas con su evidencia |
| NV-9 asignación del lector en T3 | D-T2B2-06: arrastre a T3 |
| NV-10 lista del piloto | D-T2B2-05: máximo, expresión fijada y acoplamiento del Plugin declarado |
| N-6, N-14 y N-17 (parciales) | D-T2B2-06 (accesor en el host), §6 (línea base atada al cierre de T2B1) y D-T2B2-07 (prefijo T2B1/) |

No se pide otra ronda: los menores no cambian la semántica y la re-revisión los declaró suficientes para emitir. Lo decide la sesión bajo la delegación nocturna.
