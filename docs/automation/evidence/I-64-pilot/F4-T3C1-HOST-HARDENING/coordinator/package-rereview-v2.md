# I-64 F4-T3C: re-revisión adversarial independiente del paquete de contrato (v2)

- **Objeto:** `I-64-f4-t3c-gate-contract-package.md` v2, contrastado con la v1 (`_package-v1.md`), con la revisión de la v1 (`independent-review.md`) y con la disposición del §8.
- **Código:** el worktree de I-64, rama `architecture/workspace-persistente-rackcad`, HEAD `7895f397`. Comprobé que entre `23f67f56` y `7895f397` solo cambia `docs/` (`git diff --name-only` sin rutas fuera de `docs/`).
- **Método:** solo lectura (`git show`, `git diff`, `git log`, `git grep` y lectura de archivos). No ejecuté builds ni pruebas y no abrí AutoCAD. La única evaluación fue la de dos expresiones regulares con el motor de .NET, sobre cadenas sintéticas y fuera del worktree, para N-1.

## Veredicto

**EMITIBLE TRAS CORRECCIONES.** Hay 0 hallazgos nuevos BLOQUEANTES, 1 IMPORTANTE y 6 MENORES.

- **Resueltos de verdad:** 13 de los 15 hallazgos de la v1. Entre ellos el bloqueante B-1:
  - la contención solo del constructor conserva la prueba de F3, G-12 y G-13;
  - ninguna guarda fija el cuerpo del constructor del panel.
- **Tabla H-07:**
  - es completa y exacta para los dos archivos;
  - sus traducciones no tocan ninguna trampa de G-04;
  - ninguna prueba ni guarda fija esos literales ni exige ASCII.
- **Sin resolver: m-4.** La regla nueva de `IsDeclaration` contradice su propia autoprueba con `string[] Name(`. Ese es el importante N-1: la autoprueba que manda el paquete falla en el GREEN, y la regla RED no lo detecta.

## Resolución de los 15 hallazgos de la v1

| Hallazgo v1 | Estado | Comprobación |
|---|---|---|
| B-1 (M-3 rompía la prueba de F3) | **Resuelto** | El `try` va solo alrededor de `Present(InitialSnapshot(), null)` (`WorkspacePanelViewModel.cs:73`). `Refresh` → `Query` → `EnterError` (`:143`, `:183-195`) no cambia, así que `WorkspaceBrowserViewModelTests.cs:1006-1019` sigue pasando. Detalle en «Lo que comprobé», punto 2 |
| I-1 (`catch` inalcanzable) | **Resuelto** | Queda declarado como estructural. La cobertura es una guarda por cuerpo y dos pruebas: `default` da falso y el constructor da verdadero. Quedan prohibidas las costuras y la reflexión. Lo comprobé con `DraftRegistry.cs:145`, `:169-175` y `:186-189` |
| I-2 (textos sin cerrar) | **Resuelto** | La tabla de 28 literales es completa y sus líneas son exactas (punto 3). Hay una imprecisión sobre qué guardas fijan los identificadores: N-7 a |
| I-3 (motivos del Plugin a F7) | **Resuelto en la asignación** | Pasan a T3C2 con R-C14, pero la lista de R-C14 está incompleta: N-6 |
| I-4 (M-2 sin texto ni mecanismo) | **Resuelto en textos y mecanismo** | Los textos de `:659`, `:660` y `:1000` están fijados, y ninguno choca con los textos ya fijados (`WorkspacePilotCloseSyncTests.cs:1116`, `:1134` y `:1148`). Pero el nombre propuesto `unchecked` no compila: N-2 |
| m-1 (G-02) | **Resuelto** | `Src.Code` quita los comentarios (`WorkspaceHostSourceSupport.cs:29-30`), y `\bDraft\w*` distingue mayúsculas (`WorkspacePilotEditGuardTests.cs:73`) |
| m-2 (G-04) | **Resuelto** | La lista está completa. Es más estricta de lo necesario: `CodeTraps` (`:756`) y la primera expresión de `:786` distinguen mayúsculas. Eso es conservador y no hace daño (N-7 d) |
| m-3 (M-6) | **Resuelto** | Queda sin cambio en T3C, declarado como «no necesitado», según la condición del cierre de F4-T3B2 |
| m-4 (M-4 y arrays) | **No resuelto** | Con `([\w>\]\?]+)$`, el token capturado en `string[]` sigue siendo `]` (la clase no admite `[`), y no contiene un carácter de palabra. La regla nueva cuenta `string[] Name(` como llamada. Ver N-1 |
| m-5 (forma pública) | **Resuelto en el número; el método conserva ambigüedad** | Hay 33 firmas en `23f67f56`: 41 líneas `public`, de las que se restan 2 records (`:13` y `:19`), la clase (`:44`) y 5 de `StoredNotice` (`:69-83`). El método de extracción no está fijado: N-5 |
| m-6 (prueba de M-7) | **Resuelto** | Los cuatro gestos tienen una excepción del puerto que inyectar, y la guarda cubre los seis. `CancelDiscard` (`:510-514`) y `DismissNotice` (`:517-525`) no llaman a `ContainFailure` |
| m-7 (colas) | **Resuelto** | La conversión va aviso por aviso, cada uno con su `try`, y la pérdida queda declarada. La referencia a G-09 (`WorkspacePilotEditGuardTests.cs:247-251`) es correcta. Su inalcanzabilidad no está declarada: N-4 |
| m-8 (`ErrorReason` y la regla del puerto) | **Resuelto** | Queda declarado como latente y sin traducir, se ajusta la regla 21 y se justifica `IsKnown` falso. El tipo debe ser `string?`: N-3 |
| m-9 (§6) | **Resuelto** | Comprobé por muestreo `WorkspaceLifecycleGuardTests.cs:793-797` (9 archivos y 45 pruebas), `WorkspacePaletteHostSourceGuardTests.cs:659` y `:767-777`, y `SelectiveEditorOpenTests.cs:566` (39) |
| m-10 (precisiones) | **Resuelto** | `ConfirmDiscard` ocupa `:396-507`. La RED va sin `_known` y con `Record.Exception`. `MinSelected` está medido: `docs/automation/evidence/I-64-pilot/F4-T3B2-EDIT-CLOSE-SYNC/1/R20261113T124709Z-b033/ci-filter-counts.json` da GreenCore 2331 y GreenUi 431 |

## Hallazgos nuevos

### N-1 (IMPORTANTE). La regla de M-4 contradice su propia autoprueba `string[] Name(`, y la regla RED lo oculta

**Evidencia:**
- **El paquete** (D-T3C1-07, M-4):
  - exige que el token que captura `([\w>\]\?]+)$` «contenga al menos un carácter de palabra»;
  - afirma que así `string[]` es declaración;
  - manda la autoprueba «cuentan como declaración: `string[] Name(`».
- **El código:** `WorkspacePilotCloseSyncGuardTests.cs:46` usa la clase `[\w>\]\?]`, que no admite `[`. En `…private string[]` la racha final que casa con `$` es solo `]`.
- **Lo que medí con el motor de .NET:**

  | Cadena | Token capturado | ¿Declaración con la regla v2? |
  |---|---|---|
  | `string[]` | `]` | **no** |
  | `List<int>` | `int>` | sí |
  | `int?` | `int?` | sí |
  | `void` | `void` | sí |

  Las tres llamadas dan `>`, `?` y `>`, y salen como llamadas.
- **Hoy:** `]` no está en `NotATypeBeforeAName` (`:27-30`), así que hoy `string[] Name(` sí cuenta como declaración. La v2 lo empeora.

**Problema:**
- La autoprueba que manda el paquete falla.
- En el RED pasa inadvertida, porque es una prueba nueva y la regla admite que fallen las nuevas. Vuelve a fallar en la CI del GREEN.
- El Worker tendría que cambiar la expresión fijada por el contrato o parar.
- Es exactamente lo que señalaba m-4 de la v1, y la disposición del §8 lo da por resuelto sin que lo esté.

**Corrección:**
- Fijar la expresión `([\w>]+(?:\[\])*\??)$`, con la exigencia de un carácter de palabra y la lista `NotATypeBeforeAName` sin cambio.
- Medido con .NET:
  - **declaraciones:** `string[]` da `string[]`, `List<int>` da `int>`, `int?` da `int?` e `int[]?` da `int[]?`;
  - **llamadas:** `=>` da `>`, `a >` da `>`, y `c ?` y `list[` no casan.
- Esto evita también el falso positivo que daría añadir `[` sin más: con esa clase, `list[Name(` contaría como declaración.
- **Medido:** los resultados actuales de las guardas no cambian. En la UI, las únicas apariciones sin calificar de los nombres que se cuentan son:
  - `TakeNotices`: `WorkspaceBrowserViewModel.cs:221` (precedido de `)`) y `:680` (`void`);
  - `ContainFailure`: `:596`, `:614` y `:1219`, más `WorkspaceNavigationViewModel.cs:244` y `:348`;
  - `ApplySelectionContext`: `BrowserSession.cs:80` (`BrowserChange`).

### N-2 (MENOR). `out bool unchecked` no compila

**Evidencia:**
- D-T3C1-07, M-2: «`KeepsActive` gana un `out bool unchecked`».
- `unchecked` es una palabra reservada de C#.
- Además, `KeepsActive` se llama al final de una cadena `&&` (`WorkspaceBrowserViewModel.cs:654`). Una variable `out` declarada ahí no queda asignada de forma definida si la cadena corta (CS0165).

**Corrección:**
- Nombrarlo, por ejemplo, `couldNotCheck`, o quedarse con el resultado privado de tres valores.
- Decir que la llamada se saca de la cadena `&&` sin cambiar `keepActive`. La guarda `WorkspacePilotCloseSyncGuardTests.cs:201` exige `.ApplySelectionContext(x, keepActive)`.

### N-3 (MENOR). `string errorReason = null` genera un aviso que la CI rechaza

**Evidencia:**
- `IWorkspacePilotEditPort.cs:1` es `#nullable enable`, así que `string errorReason = null` da CS8625. Pasar `draft.ErrorReason` (`string?`, `DraftSession.cs:308`) a un `string` daría CS8604.
- La CI ejecuta en cada push la compilación Release de la solución con `-NoWarnings`:
  - `.github/workflows/ci.yml:308-317`;
  - `eng/ci/verify-autocad-references.ps1:110-114` y `:600-601`.
- Ese paso falla con cualquier `warning CS…`.

**Corrección:**
- Escribir `string? errorReason = null` y `public string? ErrorReason { get; }`.
- En el RED, `public string? ErrorReason => null;`.

### N-4 (MENOR). Los `catch` nuevos de las colas son tan inalcanzables como el de `GetRacksAtRisk`, y el paquete no lo declara ni cierra la vía para alcanzarlos

**Evidencia:**
- **`TakeLossNotices`:**
  - `DraftLossNotice` solo tiene constructores `internal`, y todos sus productores le pasan listas no nulas (`DraftRegistry.cs:86-103`, `:239`, `:248`, `:297`, `:304`, `:318` y `:347`);
  - así que `new PilotLossView(...)` (`WorkspaceHostCore.cs:815`) no lanza.
- **`TakeCloseNotices`:** `List.ToArray` y `List.Clear` (`:823-824`) tampoco lanzan.
- **Una vía para alcanzarlos:** `internal IReadOnlyCollection<DraftLossNotice> LossNotices => _lossNotices;` (`:974`) devuelve la cola misma, y las pruebas ya usan los miembros `internal` (`WorkspaceLifecycleHostTests.cs:694-695`). Un Worker podría hacer `((Queue<DraftLossNotice>)core.LossNotices).Enqueue(null)`.
- **El paquete:**
  - D-T3C1-01 prohíbe costuras y reflexión solo «para alcanzarlo» (el `catch` de `GetRacksAtRisk`);
  - D-T3C1-03 no dice nada;
  - el §4 solo pide guardas para las colas, lo que es coherente pero no exclusivo.

**Corrección:**
- Declarar los dos `catch` como endurecimiento estructural, cubierto solo por las guardas del §4.
- Extender la prohibición a la conversión de tipo y a la reflexión sobre `LossNotices` y `_closeNotices`. Si se prefiere una prueba de comportamiento, admitirla de forma expresa.

### N-5 (MENOR). El método de la forma pública de M-5 no es del todo inequívoco

**Evidencia:**
- D-T3C1-07, M-5: «se quita antes el cuerpo de `private sealed class StoredNotice`, se comparan los conjuntos». Quitado ese cuerpo, siguen quedando en el archivo tres líneas `public` que no son de la clase exterior: los 2 records (`:13` y `:19`) y la declaración de la clase (`:44`).
- **No se define qué es una «firma»:**
  - si se corta en `=>` o `{`;
  - cómo se normalizan los espacios;
  - si `public WorkspacePilotFieldViewModel Field { get; }` incluye `{ get; }`.
- Pasar de un cuerpo de expresión a un autoproperty, o al revés, cambiaría o no el conjunto según se interprete.
- Las 33 cadenas no están escritas en el paquete: las mide el Worker.

**Corrección:**
- Fijar la extracción: las líneas `^\s*public\b` del cuerpo de `public sealed class WorkspaceBrowserViewModel`, sin el cuerpo de `StoredNotice`, cortadas en el primer `=>` o `{`, con los espacios colapsados y recortadas.
- Copiar en el contrato las 33 cadenas que da esa regla en `23f67f56`. Las medí así:
  - el constructor con sus tres parámetros;
  - `Field`, `IsAvailable`, `Tabs`, `HasTabs`, `ActiveRackId`, `LiveSurfaceCount`, `HasSurface`, `EmptyText`, `SurfaceTitle`, `SurfaceKindText`, `SurfaceViewsText`, `SurfaceStatusText`, `CanDiscardDocument`, `HasConfirmation`, `ConfirmationText`, `NoticeText`, `Notices` y `HasNotices`;
  - `Present(…5 parámetros…)`, `ActivateTab`, `PinTab`, `CloseTab`, `DiscardField`, `RequestDocumentDiscard`, `ConfirmDiscard`, `CancelDiscard`, `DismissNotice`, `BeginFieldEdit`, `BeginFieldEditFromBox`, `BeginFieldEditFromTab`, `ReloadField` y `ReloadSummary`.

### N-6 (MENOR). La lista de R-C14 omite tres motivos del Plugin que llegan al campo, e incluye dos que hoy no llegan

**Evidencia:**
- **Omitidos.** Llegan al campo por `catalog.FailureReason` (`WorkspaceNavigationHost.cs:230`) y por `read.FailureReason` (`:251`), y después por `read.Reason ?? …` (`WorkspaceHostCore.cs:322`, `:403` y `:481`). Son tres literales de `RackPilotIntentAdapter.cs`, cada uno con `+ ex.Message`:
  - `:85`, «The catalog could not be loaded: »;
  - `:131`, «The drawing could not be read: »;
  - `:135`, «The edit intention could not be read: ».
- **Incluidos y que hoy no llegan:** `:157` y `:174` son de `ReadPreviewWitness`, que no tiene llamadores en `src/` (`git grep ReadPreviewWitness`: solo su declaración y dos guardas).
- **Lo que no se rompe:** ninguna prueba fija esos literales.

**Corrección:**
- Completar R-C14 con `:85`, `:131` y `:135`, y decidir cómo se traducen los prefijos que llevan `ex.Message`.
- Marcar `:157` y `:174` como «no llegan hoy», o quitarlos.
- Como el paquete de T3C2 se fija después, basta con corregir la regla.

### N-7 (MENOR). Precisiones

- **a. Identificadores.** El §2.2 dice que `WorkspacePilotEditGuardTests.cs:385`, `:428` y `:448-450` fijan los cuatro identificadores. Solo fijan dos:
  - `PilotReadInProgressReason` (`:385`);
  - `InsideNoticeReason` (`:428` y `:448-450`).

  `NotThisDocumentReason` y `NoDraftReason` solo se usan en `WorkspaceHostCore.cs`. Se conservan por decisión, que es correcto, pero no porque los fije una guarda.
- **b. G-13 tiene una trampa más.** `WorkspacePilotCloseSyncGuardTests.cs:298` exige que el texto del archivo (`Src.Text`, con comentarios) no contenga «Building the panel never calls it». La documentación nueva de M-3 debe evitar esa frase. Conviene añadirlo a G-13.
- **c. Terminología.** La traducción #26 es «Hay una orden en curso.», pero el mismo panel dice «Hay un comando en curso.» (`WorkspacePanelViewModel.cs:346`) y «hay un comando en curso» (`WorkspaceBrowserViewModel.cs:1310`). Se propone «Hay un comando en curso.», que tampoco toca G-04.
- **d. G-04.** «Sin distinguir mayúsculas» es más estricto que `CodeTraps` (`WorkspacePilotEditGuardTests.cs:756`) y que la primera expresión de `:786`, que sí distinguen. Es conservador y se puede dejar.
- **e. Trampas de la UI.** La UI tiene su propia lista, sobre todos los archivos de `src/RackCad.UI` (`WorkspacePaletteHostSourceGuardTests.cs:592-593`):
  - `Registry`, `RackLog`, `XData` y otras, que distinguen mayúsculas;
  - `Metric`, `Provider`, `\bBom` y otras, sin distinguirlas.

  Los textos nuevos de D-T3C1-02 y D-T3C1-07 la cumplen. Conviene citarla junto a G-04.

## Lo que comprobé

1. **Hechos del §2.**
   - H-01 a H-08, M-1 a M-7 y G-01 a G-13 coinciden con el código, con las líneas citadas.
   - `AFieldThatThrowsRetainsTheActiveTab` está en `WorkspacePilotCloseSyncTests.cs:1179-1189` y sale por `:659`.
   - `EnterError` no tiene llamadores en `src/`.
   - `DraftSession.ErrorReason` es público (`DraftSession.cs:308`).
2. **M-3, contención en el constructor.**
   - **`EnterError` en el constructor no tiene efectos indebidos:**
     - cuando corre, `Navigation` ya existe (`:71`);
     - `Navigation.Present` no llama a su puerto (`WorkspaceNavigationViewModel.cs:156-182`);
     - `SetProperty` no tiene suscriptores durante la construcción.
   - **No se pierde ningún aviso:** los vetos ya tomados por `TakeNotices` siguen en `_stored`, porque la toma (`WorkspaceBrowserViewModel.cs:221`) va antes de la purga (`:222`). Se publican en la siguiente presentación.
   - **Se conservan las tres pruebas:**
     - `BuildingThePanelAsksNeitherTheSummaryPortNorTheNavigationPort` (`WorkspaceBrowserViewModelTests.cs:913-920`) usa `Panel()` sin puerto del piloto (`:37-44`): sin colas, `_stored` queda vacío y `AliveCalls` en 0;
     - `:1006-1019` va por `Refresh`, que no cambia;
     - G-13 mira la firma, la construcción del navegador, que no haya colas ni descartes, y la frase de N-7 b.
   - **Ninguna guarda fija el cuerpo del constructor del panel.** Las que leen el archivo miran:
     - la firma: `WorkspaceBrowserHostGuardTests.cs:227-230` y `WorkspacePaletteHostSourceGuardTests.cs:540`;
     - las construcciones: `new WorkspaceBrowserViewModel(...)` una vez y `new WorkspaceNavigationViewModel(navigation, Refresh)` una vez (`WorkspaceNavigationHostTests.cs:894-896`);
     - las llamadas: `.GetSnapshot(` una vez y ningún `.IsSessionAlive(`, `.ReadSummary(`, `.Authorize(` ni `.GetSelectionContext(`;
     - lo que falta: ningún `new WorkspacePilotFieldViewModel` (`WorkspacePilotSurfaceGuardTests.cs:348`) y ningún estado estático (`WorkspacePaletteHostSourceGuardTests.cs:543`).
   - **La prueba de M-3 se puede escribir** dentro del alcance: `WorkspaceBrowserSummaryPortDouble.ThrowOnAlive` y `WorkspacePilotEditPortDouble.CloseQueue` ya existen.
3. **H-07.**
   - **Completa.** Enumeré todos los literales de `WorkspaceHostCore.cs` y `WorkspacePilotSupport.cs`. Fuera de los 28 de la tabla, solo quedan:
     - los contextos de `Report` y `Contain`;
     - `DescribeLoss` y `DescribeDifferences`;
     - `"R"`, `"."` y `"0"` del formato numérico (`WorkspacePilotSupport.cs:30`, `:31`, `:52` y `:67`).
   - Las líneas de la tabla son exactas.
   - Ningún otro archivo de Application cruza el puerto con inglés: `WorkspaceEditIntent.cs` no tiene literales, y los del adaptador y del lector del piloto ya están en español.
   - **Traducciones:**
     - ninguna contiene, sin distinguir mayúsculas, `transaction`, `database`, `commit(`, `racklog`, `provider`, `metric`, `lockdocument`, `aggregat`, `\bbom`, `totalracks`, `projectsummary` ni `computedparameter`;
     - «borrador» no casa con `\bBom`.
   - **Nada lo fija:** ninguna prueba ni guarda fija esos literales (busqué cada uno en `tests/`: solo aparecen literales propios de dobles), y no hay guardas de ASCII.
   - Los tres archivos de Application son hoy ASCII sin BOM. Escribir UTF-8 con tildes es válido, como ya hace `PushBackPilotIntentReader.cs`.
4. **Desduplicación de `RefusalOf` y `WhyOf`.**
   - **No rompe ninguna prueba.** Todas afirman con `Contains` el motivo del doble:
     - `WorkspacePilotFieldViewModelTests.cs:914-924`, `:992-997` y `:1206-1209`;
     - `WorkspacePilotFieldViewTests.cs:370-374`;
     - `WorkspacePilotCloseSyncTests.cs:323-336`.
   - **No pierde información** con los productores reales:
     - el `Busy` del host lleva siempre `PilotReadInProgressReason` (`WorkspaceHostCore.cs:616`, `:643`, `:695`, `:727` y `:751`);
     - `NotEditable` lleva `NoDraftReason` o el #20.
   - Solo cambia la mayúscula tras los dos puntos, que es cosmético.
5. **M-2 frente a las guardas de T3B2.** No hay choque, salvo N-2.
   - Ninguna guarda cuenta `.GetField(` en la UI.
   - `CallsOutside` solo vigila los descartes, los riesgos y las colas.
   - Un campo privado de instancia no casa con `\bstatic\s+…;`, y no se añade `+=`.
   - `.ApplySelectionContext(x, keepActive)` se conserva.
6. **M-4.** La regla de la v2 no es correcta para `string[]` (N-1), aunque no cambia ningún resultado actual de las guardas de T3B2. La regla corregida que propongo tampoco lo cambia.
7. **M-5.**
   - El número es correcto: 33 en `23f67f56`.
   - Ninguna tarea de T3C1 cambia la superficie pública del navegador: todo lo nuevo es privado.
8. **Regla RED y alcance.**
   - **La regla RED es satisfacible:**
     - `IsKnown => true` sin `_known` pasa G-01 (`WorkspacePilotEditGuardTests.cs:204-210`);
     - las pruebas del desconocido con `RisksResponder = _ => default`, las de los textos y la de M-3 con `Record.Exception` fallan por aserción;
     - las de M-7 pasan ya en el RED: son de caracterización, y la regla lo admite.
     - El único fallo «nuevo» que no se arregla en el GREEN es N-1.
   - **Los 12 archivos bastan.** No hace falta tocar:
     - el doble del resumen ni las pruebas de la vista;
     - `WorkspacePilotEditGuardTests.cs`;
     - el Plugin.
   - El archivo nuevo debe declarar `namespace RackCad.Tests;`, por `NamespaceFolderGuardTests.cs:211-246`. Su clase empieza por `Workspace`, así que entra en el filtro.
