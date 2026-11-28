ESTADO = r'''
**Estado:** v3. Aplica la comprobación acotada de la v2 (`independent-review-v1.md` §13: **NECESITA v3, B/I/M = 2 / 7 / 8**) con las decisiones de la sesión (`v3-instructions.md`). El registro V3 va primero; el de la v1 → v2 se conserva debajo. Las secciones que la v3 no cambia son las de la v2, tal cual. **Mediciones nuevas:** `measure_pbt4_v3.py` y `measure_pbt4_v3-out.txt` (R-21 a R-27 de `pbt4-measurements.md`).
'''

V3_TABLE = r'''
## Registro de cambios v2 → v3

| Id | Hallazgo | Qué cambia | Dónde |
|---|---|---|---|
| **V3-01** | **B-05** («Editar todo» no llega al anfitrión) | Hay **dos reenvíos** entre el panel y el anfitrión: `PilotEditAdapter` (WCo:283-327) y `WorkspacePilotEditGate` (`WorkspacePilotSupport.cs:153-`). Con `TryEditAll` con cuerpo por defecto, los dos lo devolverían `NotAvailable` sin reenviar.<br>En PB-T4c **los dos reenvían `TryEditAll`**: los dos entran en el alcance y en la lista cerrada. Además:<br>• CT-PBT4C-12 (Core) prueba la compuerta con el anfitrión real, sin AutoCAD;<br>• CT-PBT4C-13 (guarda de texto) exige el reenvío del adaptador del Plugin, con las 18 operaciones de `ControllerViolations` (PEG:683-695), y falla en el RED;<br>• CT-PBT4C-10 recorre la cadena de punta a punta (FVM → puerto → compuerta → anfitrión → doble del editor) | D-PBT4-13 y 16; §5.3; §6.3 |
| **V3-02** | **B-04 e I-18** (la lista) | **La causa C-0,** «la lectura viene del kit del piloto», con su firma mecánica y los archivos de kit por nombre. **El procedimiento va al Owner como Q4,** con tres opciones; la recomendada es «Editar una línea del kit compartido». **C-5 se mide sobre el texto literal del GREEN** que el contrato entrega. **Medido:** 9 clases y 428 métodos (R-21) | D-PBT4-11; §8.2 Q4; §11 |
| **V3-03** | **I-12** (PB-T4a rompe más) | La elección del origen va **en línea** en cada cuerpo, sin ayudante. Siguen los seis `_editIntentReader!.ReadIntent(`, un `_editIntentReader` seguido de `.` o `!` por cuerpo, y el orden de EIG:563-575, EIG:758-770 y `WorkspaceLifecycleGuardTests:642-656`. **Solo rompe CT-PBT3A-12** | D-PBT4-03; §4.1; §5.1 |
| **V3-04** | **I-13** (la clave tras POST) | **No hay clave que guardar.** La relectura usa el **texto** del origen del borrador, `draft.CurrentDraftState.Origin`. El Plugin elige el único miembro mutable cuyo texto es, byte a byte, ese texto, que es la regla de `ChooseOrigin` de UR:640-650. Sin coincidencia, o con varias, cae en el `Primary` de hoy.<br>Tras un POST que borra el origen, el borrador queda limpio con el origen nuevo (el comparador completo compara `Origin`). No se toca ningún tipo de Application salvo la interfaz nueva, y se quitan el diccionario y sus ayudantes (M-16 y M-18) | D-PBT4-03; CT-PBT4A-03 y CT-PBT4B-17 |
| **V3-05** | **I-14** (roturas por letra) | • **Evitadas por diseño:**<br>  – EIG:670 y :720: la clasificación va en una función pura de `Draft/`, y WHC no nombra la base;<br>  – la paridad del XAML: el XAML no cambia (sin `x:Static`), y CT-PBT4B-04 compara la regla con las propiedades de la caja real;<br>  – EIG:1011-1022 y `WorkspaceNavigationHostTests:714-723`: el abridor es una clase anidada de WCo, sin archivo nuevo en `Plugin/Workspace/`.<br>• **En la tabla:** `WorkspacePilotPrepareTests:1351-1353` (11 estados de `EditIntentStatus`, con `Kept` el último), por `Presented` (C-5, PB-T4b) | D-PBT4-06, 08 y 15; §4.2 |
| **V3-06** | **I-15** | La fila de `RequestUpdate`: `Busy` por la marca (WHC:1388), antes de `PassGate` | D-PBT4-14 |
| **V3-07** | **I-16** | La condición 7 de D2, citada literal, con una línea que discrimina: un rack resuelto de **contenido distinto** en `Current` deja `IsDirty` en `False` | D-PBT4-18; CT-PBT4B-10 |
| **V3-08** | **I-17** (Q2) | Q2 con sus costes medidos: unas 136 a 165 rutas de hoja por diseño, las hojas derivadas y los 320 px. La recomendada pasa a ser **«Por grupos»**. Se dice que la letra de D-PB-05 dice «fields» | §8.2 Q2 |
| V3-09 | M-16 | Desaparece: no hay clave que convertir, porque se pasa el texto | D-PBT4-03 |
| V3-10 | M-17 | El censo de PB-T4a admite solo RIA (más `Draft/`, PP y UR). En PB-T4b, por letra, el de WHC (C-5) | CT-PBT4A-06; §4.2 |
| V3-11 | M-18 | Desaparece: sin diccionario no hay claves que borrar | D-PBT4-03 |
| V3-12 | M-19 | CT-PBT4B-11 con una secuencia fija: el comparador da `False` entre un estado con una longitud de módulo `NaN` y su copia | §5.2 |
| V3-13 | M-20 | Se declara el límite: el testigo cubre solo el sobre del origen | D-PBT4-08 |
| V3-14 | M-21 | Q1: «retira lo que se puede retirar»; la opción (C), sin número, porque no se puede medir sin diseñarla | §8.2 Q1 |
| V3-15 | M-22 | Se cierran con su cita los `<a medir>` que la §13 da por medidos. Quedan solo K, D y E, las líneas de CT-PBT4B-06 y el coste de `IsDirty` | §9 |
| V3-16 | M-23 | PB-T4a se declara **con un cambio observable:** las relecturas pasan del `Primary` al origen del borrador | D-PBT4-01 y 03; §6.1 |
'''

S0 = r'''
## 0. En una página

**Lo que pide el Owner** ([OD], fila de PB-T4): «Add "Editar todo" … Do not make PB-T4 invent state translation logic. It should consume the already proven full Push Back draft/state API.»

**Tres subtareas secuenciales,** cada una con su 1/3:

| Subtarea | TaskId | Qué | Sustituciones |
|---|---|---|---|
| **PB-T4a** | `F4B-PBT4-FULL-READ-PATH` (24) | **Aditiva en las pruebas, con un cambio observable declarado:**<br>• el Plugin lee también el modelo completo sobre el mismo escaneo, y el resultado lleva lo completo y el catálogo;<br>• las relecturas de un borrador abierto van al origen del borrador por su texto (A-T3-8), y no al `Primary`;<br>• el tipo del borrador no cambia | **Una por letra: CT-PBT3A-12** (§4.1) |
| **PB-T4b** | `F4B-PBT4-FULL-DRAFT-PANEL` (25) | El borrador del panel pasa al completo; OB-03.2; la regla única de la escala; «Actualizar» por `ExecuteFull`; «Conservar mi borrador» con los pasos 1 a 3, OB-05.4 y `KeepOn` | Según la respuesta a Q4 (D-PBT4-11) |
| **PB-T4c** | `F4B-PBT4-EDIT-ALL-WINDOW` (24) | «Editar todo»: el gesto con `CommitField`; `TryEditAll` en el anfitrión y en los dos reenvíos; el puerto que conserva `Origin`; el abridor, anidado en WCo; la guarda de OB-01.3; «Cerrar» con una sola pregunta; el resumen y «Modificado» | PEG:156-163 por letra (§4.3) |

**Cuatro preguntas al Owner,** después de medir la lista sobre el cierre de PB-T3c (§8.2):
- Q1: el piloto;
- Q2: la presentación;
- Q3: la emisión;
- Q4: el procedimiento de la lista.
'''

D03 = r'''
#### D-PBT4-03. La relectura con el origen (A-T3-8; I-12, I-13, M-04, M-16, M-18, M-23)

**El problema.** Las cinco relecturas de un borrador abierto (WHC:409, :503, :590, :675 y :759) pasan `request.ChosenReference`, que el panel deja en null. El Plugin elige entonces el `Primary` (WEI:686), no el origen del borrador. `Begin` (:328) sí debe usar `ChosenReference`.

**La fuente del origen: el texto, no una clave (I-13).**
- Cada estado de borrador ya lleva el texto del sobre de su origen: `PushBackPilotDraftState.Origin` (`Pilot/PushBackPilotDraftState.cs:15-32`) y `PushBackFullDraftState.Origin`.
- El Plugin lo resuelve con la regla de `ChooseOrigin` de la corrida (UR:640-650): **el único miembro mutable cuyo texto es, byte a byte, el del origen.**
- **Sin coincidencia, o con varias** (por ejemplo, el origen se editó fuera), cae en lo de hoy: `ChooseOriginDefinition(set, null)`, el `Primary`. Lo releído sigue siendo el rack actual, y la detección de conflicto de F4 hace su trabajo.
- **Tras un POST `Applied` que borra el origen** (PB-T3b reancla a la primera vista redibujada, PP:322 y :633), el borrador queda en Clean con la relectura de POST como base. El comparador completo compara `Origin` (`PushBackFullDraftStateComparer.cs:19`), así que `CurrentDraftState.Origin` es el texto del origen nuevo, y la relectura siguiente lo encuentra. Lo fija CT-PBT4B-17.

**Así no hace falta guardar ninguna clave:** desaparecen el diccionario, sus ayudantes y la conversión de la clave (M-16 y M-18). Ningún tipo de Application cambia, salvo la interfaz nueva.

**La interfaz nueva,** en `Application/Workspace/IWorkspaceEditIntentOriginReader.cs`: `EditIntentReadResult ReadIntentAtOrigin(SessionId session, string rackId, string originText, EditIntentWitness? witness);`.
- **La implementa el lector anidado de NH** (`PilotIntentReader`, que sigue siendo la única otra clase del archivo, EIG:442), por el mismo método privado `ReadOnActiveDocument`, que gana `string? originText`.
- **RIA:** `RackPilotIntentAdapter.Read` gana el argumento `string? originText`. Si viene, tras `CaptureDrawing` elige en `scan.RawEnvelopes` el único miembro mutable con ese texto; si no lo encuentra, usa el `originDefinitionHandle` de reserva, el del `Primary`, que calcula NH como hoy.
- Siguen una sola `LoadCatalog()` y una sola `RackPilotIntentAdapter.Read(` en NH (EIG:414-430).

**En línea en cada cuerpo, sin ayudante (I-12).** En las cinco relecturas, la sentencia de la lectura pasa a:

```csharp
var read = _editIntentReader is IWorkspaceEditIntentOriginReader originReader
    ? originReader.ReadIntentAtOrigin(request.Session, request.RackId, draft.CurrentDraftState.Origin, request.Witness)
    : _editIntentReader!.ReadIntent(request.Session, request.RackId, request.ChosenReference, request.Witness);
```

Lo que mantiene en verde:
- **los recuentos:** siguen los seis `.ReadIntent(` (EIG:736), un `_editIntentReader` seguido de `.` o `!` por cuerpo (EIG:573-574, :767, `WorkspaceLifecycleGuardTests:656`) y el orden (EIG:568-569, :761-763, `WorkspaceLifecycleGuardTests:646-654`). `ReadIntentAtOrigin(` no casa con `\.ReadIntent\s*\(`;
- **la base:** `draft.CurrentDraftState.Origin` no es una lectura de la base (`BaseOrReadUse`, EIG:648), y WHC ya nombra `draft.CurrentDraftState` (WHC:1772);
- **los dobles existentes del lector** (`EditIntentFakeReader`) no implementan la interfaz nueva: siguen por la rama de hoy.

**El cambio observable (M-23).** En AutoCAD, las relecturas pasan del `Primary` al origen del borrador. Es lo que pide A-T3-8, y se declara en el `Objective` de PB-T4a y en el Smoke-PB.
'''

D06 = r'''
#### D-PBT4-06. La regla única de la escala (B-02; Q-PBT2-3; I-14)

**Dónde vive:** en la UI, en `src/RackCad.UI/Controls/AnnotationScaleRule.cs`. La referencian la UI, sus pruebas y el Plugin. Contiene:
- `public const double Minimum = 0.0;`
- `public const bool MinimumInclusive = false;`
- `public static DraftFieldValidation Validate(string text)`: `NumericFieldValidation.Validate(text, false, false, Minimum, null, MinimumInclusive, true)`, traducido a `DraftFieldValidation`.

Una sola implementación: `NumericFieldValidation`, la de la ventana.

**El XAML no cambia (I-14).** W.xaml:68-69 se queda con sus literales, porque `WorkspaceDraftPilotValidationParityTests.cs:80-97` (F4, UI) los fija en el texto. CT-PBT4B-04 garantiza que regla y caja son una sola fuente: compara `Minimum` y `MinimumInclusive` de la regla con las propiedades de `AnnotationScaleBox` en una ventana real, y las dos validaciones en los diez textos de CT-PBT2B-10.

**Cómo llega al anfitrión:** el Plugin la conecta en WCo, junto a `SetPilotUpdateHost`, con `_host.SetFieldRule(AnnotationScaleRule.Validate)`. El campo del anfitrión es un `Func<string, DraftFieldValidation>?`, que no es de `Systems`, así que EIH:1088 sigue. Sin regla asignada, el campo dice «No editable aquí». EIG:1150-1190 exige el `SetPilotUpdateHost` tras el lector y antes del `return`: las asignaciones nuevas van después, y esa guarda sigue en verde (§13.6).

**El adaptador nuevo,** `Draft/PushBackFullFieldAdapter`, como en la v2.

**La copia del piloto,** según Q1.
'''

D08 = r'''
#### D-PBT4-08. «Conservar mi borrador» en dos tiempos (OB-05.3 a 5; A-T3-3; I-02, I-14, M-20)

**Por `TryKeepMine`, sin miembro nuevo del puerto.** La petición sin testigo presenta, y con testigo conserva:

1. **«Conservar mi borrador»** (`RequestKeepMine`) llama a `_port.TryKeepMine(new EditIntentRequest(session, rack, null, null))`. El anfitrión:
   - **paso 1:** relee con el origen (D-PBT4-03), con la detección de conflicto de F4;
   - **paso 2:** clasifica con una **función pura de `Draft/`**:
     ```csharp
     PushBackKeepMinePresentation.Read(DraftSession<PushBackFullDraftState> draft, PushBackFullIntentResult read, RackCatalog catalog, PushBackPresentationForm form)
     ```
     Dentro, llama a `PushBackConflictClassifier.Classify(draft.InitialDraftState, read.Read.State, draft.CurrentDraftState, los sobres de read.StaleBase, catalog)`. **WHC no nombra `.InitialDraftState`, `.StaleBase` ni `.Entries`,** así que EIG:670 y :720 siguen en verde;
   - devuelve `EditIntentOutcome.Presented(lines, testigo)`: el estado nuevo **`Presented`**, al final del enumerado, con el testigo `EditIntentWitness(clave del origen, texto del sobre releído)`.
   - **No cambia el borrador.**
2. **La UI** muestra las líneas encima del Sí/No, en un `TextBlock` nuevo, `BrowserFieldKeepMinePresentationText`.
3. **«Sí»** llama a `TryKeepMine` con el testigo. El lector lo compara con lo que hay:
   - **`Changed`:** no conserva, y devuelve `Presented` con lo nuevo;
   - **`Unchanged`:** `KeepMineOnCurrentVersion(full.Read, PushBackFullComposition.KeepOn, confirmed: true)`.
4. **«No»** no llama al puerto.

**Lo que rompe por letra (en la tabla de §4.2):**
- `WorkspacePilotPrepareTests:1351-1353` fija 11 estados, con `Kept` el último. `Presented` lo rompe (C-5, PB-T4b).
- **Se consideró evitarlo** presentando desde memoria, con la clasificación guardada en la última comprobación. Se descartó: en Dirty con `IsPossiblyStale` no hay comprobación previa, y la presentación necesitaría igualmente una lectura en dos tiempos, con un estado de «revisa antes».

**Lo declarado:**
- pedir «Conservar» pasa a leer el dibujo, frente a la regla de F4 de la UI :780-793. Lleva su registro (V2-28);
- **M-20:** el testigo cubre solo el sobre del origen. Si cambia otro miembro entre la presentación y el «Sí», la presentación queda vieja en ese miembro. Aun así, lo que se conserva parte de la relectura del «Sí», y POST vuelve a comprobar. Se declara como límite.
'''

D11 = r'''
#### D-PBT4-11. La lista de sustituciones: las causas y el procedimiento (B-03, B-04, I-03, I-04, I-18)

**Las causas cerradas, con su firma mecánica:**

| Causa | Firma (texto de la prueba, de su archivo o de un archivo de kit con nombre; nada transitivo) |
|---|---|
| **C-0. La lectura viene del kit del piloto** | El archivo de la prueba construye `EditIntentFakeReader` o usa `EditIntentKit` o `FromPilot(`, o el método usa un `Rig` de `WorkspaceHostTestSupport.cs` que monta ese lector. El método empieza un borrador: nombra `TryBeginEdit`, `.Begin(` o un ayudante de su archivo que los llama. El fallo es el estado de la intención o la ausencia del borrador |
| C-1. El tipo del borrador | Nombra `Find<PushBackPilotDraftState>`, `PushBackPilotDraftState` o `PushBackPilotStateComparer`, y el fallo es `NotNull`, una `NullReferenceException` o una igualdad sobre ese estado |
| C-2. El rechazo A/B | Espera `OutOfPilotDiagnostic` o `CompositeOutOfPilotReason` por el anfitrión; el fallo contiene «fuera del piloto» |
| C-3. «Conservar» en dos tiempos | Nombra `TryKeepMine`, `RequestKeepMine` o `ConfirmChoice`; el fallo es `Presented`, una lectura inesperada o el estado tras conservar |
| C-4. «Actualizar» por `ExecuteFull` | Nombra `Execute` o `Precheck` del piloto, o un texto de la corrida que cambia con el modelo |
| C-5. La letra de una guarda | Las tablas de §4, medidas sobre el texto literal del GREEN (abajo) |
| C-6. La regla de la escala | Usa el adaptador del piloto por el anfitrión |
| C-7. La retirada del piloto (con Q1 «Retirar») | No compila, o nombra un miembro retirado |

**Los archivos de kit, por nombre** (la definición mecánica de «su kit», R-21):
- `WorkspaceEditIntentHostTests.cs`, con `EditIntentFakeReader` y `EditIntentKit` (EIH:13-52);
- `WorkspaceHostTestSupport.cs`, con `WorkspaceHostRig`;
- `WorkspacePilotPrepareTests.cs` (`PrepareKit`);
- `WorkspaceDraftPilotIntentStaleBaseTests.cs` (`IntentKit`);
- `WorkspaceDraftPilotStateTests.cs` (`PilotKit`);
- `WorkspaceDraftSessionTests.cs` (`DraftKit`).

**C-5, sobre el texto entregado.**
1. El contrato de PB-T4b fija literalmente las líneas del GREEN que leen las guardas de texto. Entre ellas: la sentencia de `Begin`, las seis `var full = read.FullRead;`, la de `KeepOn`, la de `ExecuteFull` y la de `Presented`.
2. Antes de emitir, la sesión aplica esas líneas a una copia exportada **solo para ejecutar las guardas de texto**, y lista lo que rompen.
3. El Worker recibe esas mismas líneas, así que no hay implementación oculta.

Va en la precondición 6.

**El procedimiento lo decide el Owner (Q4).** La sesión recomienda la opción 1.

| Opción | Qué hace | La lista que ve el Owner | Lo que exige al contrato de PB-T4b |
|---|---|---|---|
| **1. «Editar una línea del kit compartido» (Recomendada)** | En PB-T4b, `EditIntentKit.ReadOf` (EIH:51-52) pasa a `EditIntentReadResult.FromReads(PushBackPilotIntentReader.Read(scan, PilotKit.Catalog), PushBackFullDraftReader.Read(scan, PilotKit.Catalog), PilotKit.Catalog)`. Las 9 clases (428 métodos) conservan sus aserciones, ahora sobre el camino real del producto. Solo se sustituyen con registro las que fallan por C-1 a C-7. C-0 desaparece | Por texto, **unos 44 métodos** de las 9 clases tienen alguna firma de C-1 a C-6 (R-21). Además, las roturas de C-5 (§4.2) y las de la UI de la elección (C-3; hasta 21 pruebas con `RequestKeepMine`) | **Alcance:** EIH, para una sola línea (`ReadOf`). **Una excepción declarada a C02:** esa línea, y nada más del kit. **Paradas:** C15 si una prueba falla fuera de la lista con una firma, y C16 si falla sin ella |
| **2. «Saltar y duplicar, como D-1»** | Cada método de C-0 se salta y se copia con el kit nuevo, con un mapa cerrado de sustituciones (`EditIntentKit` → `PushBackFullReadKit`, `Find<PushBackPilotDraftState>` → `Find<PushBackFullDraftState>`, el comparador…) y un guion que compara cada original con su copia módulo el mapa. C-2, C-3 y C-5 llevan equivalentes a mano | **428 métodos** de C-0, más C-2, C-3 y C-5, más los de la UI | **Alcance:** los `Skip` de las 9 clases y las copias. **Paradas:** C15 y C16, y la del guion de comparación |
| **3. «Una implementación de referencia de la sesión»** | La sesión escribe un GREEN de referencia solo para medir, lo custodia, y la revisión lo lee. El Worker lo recibe (3b) o no lo recibe (3a) | Exacta y pequeña | **3a:** C16 para cualquier diferencia del Worker. **3b:** cambia los papeles de la sesión y del Worker |

**Por qué la 1** (lo que rompe y por qué se recomienda igual):
- **Lo que rompe:** C02 (se edita un ayudante existente) y el «como D-1» de A-T3-1 (no se salta y duplica todo).
- **Lo que gana:** cumple a la vez:
  - (a) un criterio cerrado: C-1 a C-7, sin C-0;
  - (b) ninguna implementación oculta: la línea del kit y las líneas del GREEN van en el contrato;
  - (c) ninguna guarda debilitada en silencio: las 428 siguen ejecutándose con todas sus aserciones.

**Lo que no cambia con ninguna opción:**
- la tabla de la lista, una fila por método, con la causa, la firma, el equivalente y las aserciones que conserva;
- las reglas de las `[Theory]` y de los equivalentes de la v2;
- **`<N>` es una selección por texto.** Es probable una vuelta al Owner a mitad de PB-T4b con la opción 1 o la 2, y Q3 la prevé.
'''

D13_16 = r'''
#### D-PBT4-13. El anfitrión, los dos reenvíos y el puerto generalizado (A-T3-11, A2-1; B-05; M-03)

**`TryEditAll` es el único miembro nuevo del puerto,** con cuerpo por defecto (`=> EditAllOutcome.NotAvailable`), para que no se edite el doble `WorkspacePilotEditPortDouble` (C02). Sus reglas son las de `TryKeepPilotEdit` (I-09):
1. `PassGate` primero;
2. la marca `_pilotReadInProgress`, como envoltorio, mantenida durante `ShowModal`;
3. `UpdateInFlightForRequest`;
4. la contención de excepciones con `Report`;
5. `NotePilotEdit(session)` por cada «Aplicar».

**Los resultados:**
- sin borrador, lo empieza;
- la cola aparcada no abre;
- Clean, Dirty o Conflict abren;
- Applying da `Waiting`;
- Unknown, Error o Undecidable dan `Refused`;
- sin anfitrión del editor, «No editable aquí»;
- con texto pendiente, `EditAllPendingTextReason`.

**Los dos reenvíos entre el panel y el anfitrión (B-05; R-22).** El panel recibe `new PilotEditAdapter(new WorkspacePilotEditGate(_host, _noticeDepth))` (WCo:52). Con cuerpo por defecto, ninguno de los dos reenviaría `TryEditAll`. En PB-T4c, los dos lo reenvían, de forma explícita:
- **la compuerta** (`WorkspacePilotSupport.cs:153-`): `public EditAllOutcome TryEditAll(EditIntentRequest request) => _depth.IsInside ? EditAllOutcome.Waiting(InsideNoticeReason) : _inner.TryEditAll(request);`. Es Application, y la prueba CT-PBT4C-12 la ejecuta con el anfitrión real;
- **el adaptador del Plugin** (`PilotEditAdapter`, WCo:283-327): `public EditAllOutcome TryEditAll(EditIntentRequest request) => _gate.TryEditAll(request);`. El Plugin no se ejecuta en las pruebas, así que la prueba es la guarda de texto CT-PBT4C-13, que copia `ControllerViolations` (PEG:683-695) con 18 operaciones y falla en el RED, donde falta el reenvío.

**No hay otros reenvíos** entre el panel y el anfitrión (R-22). El anfitrión de la paleta pasa `PilotEdit` una vez, sin envolverlo, y `WorkspaceCloseAdvisorRelay` usa `_pilotEdit` solo para los avisos de cierre.

**El lanzamiento y el adaptador del puerto** van en `Draft/`, como en la v2: `PushBackFullDraftLaunch.For` y `PushBackFullDraftStatePort`. `PushBackDraftSessionPort` y `LaunchFor` se conservan, con su registro en DISP.

#### D-PBT4-14. Los estados de cada operación durante el editor (I-01; I-15)

Con la ventana abierta (dentro de `ShowModal`):

| Operación | Responde | Por qué |
|---|---|---|
| `SetPendingText`, `CommitField`, `CrossBoundary`, `DiscardRack` y `DiscardDocument` | `Busy`, con `PilotReadInProgressReason` | Miran la marca (WHC:1127, :1155, :1208, :1243 y :1270) |
| `TryBeginEdit`, `TryCheckNow`, `TryReload`, `TryConfirmBase`, `TryKeepMine`, `TryDiscardAndReload` y `TryEditAll` | `Waiting` | La marca (WHC:1022 y siguientes) y `PassGate` (con el modal) |
| **`RequestUpdate`** | **`Busy`, con `PilotReadInProgressReason`** | **Mira la marca antes que nada** (WHC:1388), antes de `PassGate` (:1392) |
| `GetField`, `GetRacksAtRisk`, `TakeLossNotices`, `TakeCloseNotices` y `TakeUpdateProgress` | De memoria, como hoy | |

Se declara que el texto de `Busy` dice «lectura en curso» mientras el editor está abierto.

#### D-PBT4-15. Quién pone el modal, y dónde vive el abridor (I-01; I-14; M-14)

**El modal.**
- En el producto, lo pone el abridor del Plugin con `using (EnterPanelModal())`, el único conductor admitido de la bandera (PEG:778-781).
- En Core, lo pone el doble del anfitrión del editor, con `SetModalRackCadActive`.

**El abridor es una clase anidada de WCo,** `private sealed class FullEditorHost : IPushBackFullEditorHost`, como `PilotEditAdapter` y `BridgeLink`. **No es un archivo nuevo.** Así:
- **siguen en verde** EIG:1011-1022 y `WorkspaceNavigationHostTests:714-723`, que fijan 9 archivos en `Plugin/Workspace/`;
- **no se usa `Plugin/Views/`,** donde un archivo que implementa una interfaz de `RackCad.Application.Workspace` rompería `WorkspaceBridgeSourceGuardTests:364-367`. Esa guarda prohíbe nombrar `RackCad.Application.Workspace` en el Plugin fuera de `Plugin/Workspace/`;
- ninguna guarda medida fija las clases anidadas de WCo (R-22).

**`IPushBackFullEditorHost`,** con `void ShowModal(PushBackDraftEditorLaunch launch)`, se declara en `Application/Workspace`. Se asigna en WCo con `_host.SetFullEditorHost(new FullEditorHost(this))`, después de `SetPilotUpdateHost`, así que EIG:1150-1190 sigue en verde. El campo `_fullEditorHost` es de un tipo de `Application.Workspace`, así que EIH:1088 sigue en verde. Se declara como excepción a R-PB-05 y a ADR-0029 D12.

#### D-PBT4-16. El abridor y la guarda de OB-01.3 (M-05, M-09)

**`FullEditorHost.ShowModal(launch)`:**
1. `using (_controller.EnterPanelModal())`;
2. `var window = RackPushBackSystemWindow.ForDraft(launch);`;
3. `AcApplication.ShowModalWindow(window);`, sin asignar `Owner`, como el abridor clásico.

**Lo que no lleva:**
- ni `LockDocument`, ni transacción, ni nada tras cerrar;
- ni los nombres que `WorkspaceBridgeSourceGuardTests:139-142` prohíbe en `Plugin/Workspace/` (`ShowModalDialog`, `ShowDialog`, `MessageBox`, `GetObject`…).

**La guarda de OB-01.3** (CT-PBT4C-07) mira cinco sujetos:
- el cuerpo de `FullEditorHost` en WCo;
- `TryEditAll` y sus ayudantes en WHC;
- el gesto `EditAll` en FVM;
- `Draft/PushBackFullDraftLaunch.cs`;
- `Draft/PushBackFullDraftStatePort.cs`.

Ninguno nombra `RackPushBackCommands`, `RackMenuCommands`, `EditorModules`, `RackViewBatchExecution`, `DrawAndPlace`, `RedrawInPlace`, `InsertRequested` ni `InsertionRequest`. La autoprueba en negativo inyecta cada nombre en cada sujeto.
'''

D18 = r'''
#### D-PBT4-18. D2 (A2-5, A-T3-4; I-06, I-16)

**La letra del Owner** (`owner-d1-d2-decisions.md:113-131`, «L-1 SAFETY CONDITIONS»):
- 3: «DraftSession editing does not mutate either shared object»;
- 5: «pending-text/edit operations do not mutate either shared object»;
- 6: «discard/reload does not mutate either shared object»;
- **7:** «comparing InitialDraftState vs CurrentDraftState does not depend on mutable changes inside those shared objects».

**CT-PBT4B-10,** la huella profunda de CT-PBT1B-16 (copiada), tomada **después de cada operación**:
- condición 3: `Begin`, `SetCurrentDraftState` y el rebase de POST;
- condición 5: `SetPendingText` y `CommitField`;
- condición 6: `DiscardAndReload` y `KeepMineOnCurrentVersion` con `KeepOn`;
- **condición 7, con una línea que discrimina:**
  - se arma `Current` como una copia del borrador cuyo rack resuelto se sustituye por **otro objeto de contenido distinto** (el rack resuelto de otro fixture), con todo lo editable igual;
  - el comparador tiene que dar `Equals` = `True`, y el borrador, `IsDirty` = `False`;
  - un comparador que dependiera del contenido de esa referencia daría `True` y fallaría.

  La línea es `rack-resuelto-de-otro-contenido isdirty=False`. Lo mismo con el informe de módulos: `informe-de-otro-contenido isdirty=False`.

**CT-PBT4C-11,** sobre `ApplyToDraft`.

**El límite literal,** en PBT4x-C17.

**M11 (A2-8)** no aplica.
'''

S4 = r'''
## 4. Las guardas, por subtarea

### 4.1 PB-T4a

| Guarda | Ancla | Qué hace PB-T4a | Resultado |
|---|---|---|---|
| CT-PBT3A-12 (`PushBackDraftWriteGuardTests:44-94`) | Fuera de `Draft/`, PP y UR nadie nombra los cinco tipos | RIA nombra `PushBackFullDraftReader` | **Por letra (C-5): se sustituye** por CT-PBT4A-06, que admite RIA y nada más (M-17) |
| EIG:733-736 (seis asignaciones y seis `.ReadIntent(`) | | Elección en línea (D-PBT4-03) | **Verde** |
| EIG:563-575, EIG:758-770 y `WorkspaceLifecycleGuardTests:642-656` (el orden y un lector por cuerpo) | | Elección en línea | **Verde** (I-12) |
| EIG:414-430 (NH: una `LoadCatalog()` y una `Read(`) | | El mismo método privado | Verde |
| EIG:441-470 y :442 (el lector anidado; dos clases en NH) | | `PilotIntentReader` gana la segunda interfaz, sin clase nueva | Verde (§13.3) |
| EIH:1088 y :1102-1121 | | Sin campos nuevos en WHC | Verde |
| EIH:1175-1194 (la forma del resultado) | | Aditivo | Verde |
| `WorkspaceCommandAdvisorGuardTests:406-450` (los motivos en español en los tres cuerpos) | | La rama del origen no añade texto en inglés | Verde (§13.6) |
| G-9 (IG:677-699) | | Sin términos nuevos | Verde |

**Total de PB-T4a: una sustitución,** CT-PBT3A-12.

### 4.2 PB-T4b

Las roturas por letra (C-5) se miden sobre el texto literal del GREEN (D-PBT4-11). Las ya medidas por objetos git:

| Guarda o prueba | Causa | Resultado |
|---|---|---|
| CT-PBT4A-06 (el censo de PB-T4a) | C-5: WHC nombra los tipos completos | Se sustituye por CT-PBT4B-13, que admite también WHC |
| CT-PBT3B-13 (`PushBackWritePathGuardTests:235-296`) | C-5 | Se sustituye |
| G-4 (IG:497-498) | C-5 | Se sustituye: `ExecuteFull(` solo en WHC |
| EIG:711, :776 y :805 (`KeepPilotEditsOn`) | C-5 y C-3 | Se sustituyen |
| EIG:733-735 (`var pilot = read.PilotRead;`) | C-5 | Se sustituye: seis `var full = read.FullRead;` y tres lecturas de `read.PilotRead.PreviewWitness` |
| EIG:670 y :720 (la base en WHC) | — | **Verde por diseño** (D-PBT4-08) |
| PEG:280 (el adaptador del piloto, campo estático) | C-6 | Se sustituye, con todas las demás reglas de PEG:265-350 |
| RG:116 (`Mutate` en `RequestKeepMine`) | C-5 | Se sustituye |
| **`WorkspacePilotPrepareTests:1351-1353`** (11 estados de `EditIntentStatus`, `Kept` el último) | C-5: `Presented` | **Se sustituye:** 12 estados, `Kept` en el 11.º y `Presented` en el 12.º |
| `WorkspaceDraftPilotValidationParityTests:80-97` (el texto del XAML) | — | **Verde por diseño** (D-PBT4-06) |
| UI `WorkspacePilotReconciliationTests:780-793` y las que piden «Conservar» | C-3 | Se sustituyen, con registro declarado |
| Las de comportamiento del anfitrión | C-0 a C-4 | Según Q4 (D-PBT4-11) |

### 4.3 PB-T4c

| Guarda | Causa | Resultado |
|---|---|---|
| **PEG:156-163** (17 nombres por reflexión) | C-5 | Se sustituye: 18 nombres, con `TryEditAll` entre las de lectura |
| PEG:683-695 (`ControllerViolations`, el adaptador) | — | Sigue en verde: recorre las 17 como cota inferior. **Su equivalente con 18 es CT-PBT4C-13** |
| EIG:1011-1022 y `WorkspaceNavigationHostTests:714-723` (9 archivos en `Plugin/Workspace/`) | — | **Verde por diseño** (D-PBT4-15) |
| EIG:1150-1190 (el cableado) | — | Verde: las asignaciones van después de `SetPilotUpdateHost` |
| CT-PBT2B-08 (`PushBackWorkspaceModeSourceGuardTests`) | — | Verde: R10 cuenta los once literales uno a uno (:271-274), y ninguna regla mira `PendingWork` (§13.6) |
| `WorkspaceBridgeSourceGuardTests:139-142` y :364-367 | — | Verde: el abridor está en WCo, sin los nombres prohibidos |
| URT:1107-1109, EIH:1066 y PEG:380-385 | — | Verdes |

**Si cualquier otra prueba falla,** fuera de las tablas y de la lista, es C02 o C16 y vuelve al Owner (OB-T1B.3).
'''

S51_53 = r'''
### 5.1 PB-T4a: la lista cerrada CT-PBT4A-01 a 07 (Core)

| CT | Qué fija | `ExpectRed` |
|---|---|---|
| 01 | `FromReads`: `PilotRead`, `FullRead` y `Catalog`; un nulo da `ArgumentNullException`; `FromPilot` y los demás estados los dejan en null | true |
| 02 | El lector completo y el del piloto sobre el mismo escaneo de `IntentKit`: `pb-01-base`, `pb-20-ab` (rechazado por el piloto y admitido por el completo), `pb-21-cola` y AUTH-13 | true |
| 03 | **Las cinco relecturas** usan `ReadIntentAtOrigin` con `draft.CurrentDraftState.Origin` cuando el lector la ofrece (un doble nuevo que registra el texto); `Begin` usa `ReadIntent` con `ChosenReference` | true |
| 04 | **La elección por texto, pura:** una función de `Draft/`, `PushBackOriginByText.Choose(DraftStaleBase staleBase, string originText)`, que es la que llama RIA sobre `PushBackPilotIntentReader.BuildStaleBase` del escaneo, como hace UR:645-649. Da la clave del único miembro **mutable** con ese texto; null con cero o con varios | true |
| 05 | Con un lector sin la interfaz nueva, las relecturas usan `ReadIntent` como hoy | **false** |
| 06 | El censo: solo `Draft/`, PP, UR y RIA nombran los cinco tipos (sustituye a CT-PBT3A-12) | **false** |
| 07 | La guarda del Plugin, en texto, con autoprueba en negativo:<br>• RIA llama una vez a `PushBackFullDraftReader.Read(` sobre el mismo `input`, tras el lector del piloto, y una vez a `PushBackOriginByText.Choose(`;<br>• NH implementa la interfaz nueva por el mismo método privado, con una `LockDocument` y una `RackPilotIntentAdapter.Read(`;<br>• NH devuelve `FromReads` | true |

**Las clases:** `PushBackFullReadPathTests` (01 a 04 y 07), con `ExpectRed` true; `PushBackFullReadGuardTests` (05 y 06), con false.

**Los totales.** Base prevista, `<tras el cierre de PB-T3c>`: Core 19 129 = 19 123 + 6. Con 7 CT y un `Skip` (CT-PBT3A-12, un `[Fact]`):

| Rama | Core | UI |
|---|---|---|
| **RED** | 19 136 = (19 123 − 1 + 2) `Passed` + 5 `Failed` + 7 `Skipped` | 2 451, sin cambio |
| **GREEN** | 19 136 = 19 129 `Passed` + 7 `Skipped` | 2 451 |

D = 0. Se confirma en la precondición 6.

### 5.2 PB-T4b: la lista cerrada CT-PBT4B-01 a 17

Las CT-01 a 16 de la v2, con estos cambios:
- **CT-04:** sin `x:Static`. Compara `AnnotationScaleRule.Minimum` y `MinimumInclusive` con las propiedades de `AnnotationScaleBox` de una ventana real, y las dos validaciones en los diez textos.
- **CT-06:** la presentación sale de `PushBackKeepMinePresentation.Read`, con las líneas medidas antes de emitir.
- **CT-10:** las filas de D-PBT4-18, con la condición 7 que discrimina.
- **CT-11 (M-19):** un estado del borrador con una longitud de módulo `NaN` en su commit, construido en código (no hace falta fixture), comparado con su propia copia. La línea es `nan-longitud-modulo igual-a-su-copia=False`. Es lo que A-9 y A2-13 declaran: con `NaN`, `IsDirty` no vuelve a `false`.
- **CT-13:** el censo pasa a admitir WHC, y sustituye a CT-PBT4A-06.
- **CT-17, nueva (I-13):** tras un «Actualizar» `Applied` que borra la vista de origen (`PilotEraseScopeDouble`, POST reanclado), el borrador queda en Clean, su `CurrentDraftState.Origin` es el texto de la primera vista redibujada, y la relectura siguiente lo pasa a `ReadIntentAtOrigin`. Core, `ExpectRed` true.

**Los totales** (B = la base de PB-T4b):

| Rama | Core | UI |
|---|---|---|
| **RED** | (B + 15 + E^R + E^G − D) = (B_P − K − D + 2 + E^G) `Passed` + (13 + E^R) `Failed` + (B_S + K) `Skipped` | Como en la v2 |
| **GREEN** | El mismo total, con 0 `Failed` | |

K, D y E dependen de la opción de Q4.

### 5.3 PB-T4c: la lista cerrada CT-PBT4C-01 a 13

Las CT-01 a 11 de la v2, con estos cambios:
- **CT-05:** la tabla de D-PBT4-14, con `RequestUpdate` en `Busy`.
- **CT-07:** los cinco sujetos de D-PBT4-16.
- **CT-08:** el cuerpo de `FullEditorHost` en WCo y su cableado.
- **CT-10:** recorre la cadena entera de producto que corre sin AutoCAD:
  1. una FVM real sobre el puerto real (`WorkspacePilotEditGate` sobre WHC);
  2. el doble del anfitrión del editor aplica por el puerto;
  3. «Actualizar» con `PilotEraseScopeDouble`.

  Solo el adaptador del Plugin queda fuera, y CT-13 lo cubre.

**Y dos nuevas:**

| CT | Qué fija | Proyecto | `ExpectRed` |
|---|---|---|---|
| **12** | **La compuerta reenvía `TryEditAll`** (B-05): `new WorkspacePilotEditGate(host, depth).TryEditAll(request)` llega al doble del anfitrión del editor. Dentro de un aviso da `Waiting` con `InsideNoticeReason` | Core | true |
| **13** | **El adaptador del Plugin reenvía `TryEditAll`** (B-05): `ControllerViolations` copiada con 18 operaciones (PEG:683-695), con la autoprueba de un adaptador sin el reenvío | Core | true |

**Las clases:** las de la v2. CT-12 va en `PushBackEditAllTests` y CT-13 en `PushBackEditAllGuardTests`.

**Los totales:**

| Rama | Core | UI |
|---|---|---|
| **RED** | (C + 10 + 1 − D_C) = (C_P − K_C − D_C + 2) `Passed` + 9 `Failed` + (C_S + K_C) `Skipped` | + 3, con 3 `Failed` |
| **GREEN** | El mismo total, con 0 `Failed` | + 3, con 0 `Failed` |

K_C = 1 (PEG:156-163, un `[Fact]`).
'''

S6 = r'''
## 6. Los contratos propuestos

### 6.0 Común

Lo de la v2 (§6.0): la identidad, los TaskId, las colecciones opacas, `ExpectedEvidence[0]` literal, el tamaño, las paradas y RR. Con estos cambios:

**`UNIT_CHANGE`:**
- **PB-T4a:** WEI, WHC, RIA y NH;
- **PB-T4b:** WHC, WEI, `WorkspacePilotSupport.cs`, FVM, PV y WCo, y con la opción 1 de Q4, EIH (una línea); W.xaml ya no;
- **PB-T4c:** WHC, `IWorkspacePilotEditPort.cs`, `WorkspacePilotSupport.cs`, WEI, FVM, PV, WCo y W.

**La lista cerrada de declaraciones nuevas, por nombre** (C13):
- **PB-T4a:**
  - `EditIntentReadResult.FullRead`, `.Catalog` y `.FromReads`;
  - `IWorkspaceEditIntentOriginReader.ReadIntentAtOrigin`;
  - `PushBackOriginByText.Choose`;
  - `RackPilotIntentReadResult.Full`;
  - el argumento `originText` de `RackPilotIntentAdapter.Read` y de `ReadOnActiveDocument`;
  - `ReadIntentAtOrigin` en el lector anidado de NH;
  - **en WHC, ninguna declaración nueva:** solo la sentencia en línea de D-PBT4-03, en cinco cuerpos.
- **PB-T4b:** las de la v2, sin `x:Static` y con `PushBackKeepMinePresentation.Read`. Sin `PresentKeep` ni `MembersOf` en WHC, porque los hace `Draft/`.
- **PB-T4c:** las de la v2, con estos cambios:
  - **más:** `WorkspacePilotEditGate.TryEditAll` y `PilotEditAdapter.TryEditAll` (los dos reenvíos);
  - **en lugar del archivo `WorkspaceFullEditorHost.cs`:** la clase anidada `FullEditorHost` de WCo, con su constructor y `ShowModal`.

**La precondición 6** mide:
- las roturas de C-5 sobre el texto literal del GREEN, aplicado a una copia solo para las guardas de texto;
- K, D y E;
- las líneas de CT-PBT4B-06;
- el coste de `IsDirty`.

### 6.1 PB-T4a — `F4B-PBT4-FULL-READ-PATH`

- **Objective** (ASCII):
  > Hacer que la lectura de la intencion de edicion del Workspace de Push Back traiga tambien el modelo completo de PB-T3 sobre el mismo escaneo: el adaptador del Plugin llama al lector completo tras el del piloto y el resultado de la lectura lleva la lectura completa y el catalogo junto a la del piloto (FromReads; FromPilot no cambia); y que las cinco relecturas de un borrador abierto vayan al origen del borrador por su texto (el unico miembro mutable con ese texto, o el Primary de hoy si no hay uno) cuando el lector lo ofrece, con la eleccion en linea en cada cuerpo (A-T3-8). Es un cambio observable declarado: las relecturas dejan de ir al Primary. El tipo del borrador del panel no cambia, nada traduce estados y no se lee el dibujo mas de una vez por lectura, segun el paquete del contrato de PB-T4 v3 y la lista cerrada CT-PBT4A-01..07. Solo se sustituye con registro CT-PBT3A-12. Si hiciera falta mutar el rack resuelto o el informe de modulos dentro de la vida del borrador: STOP y se reabre L-1. No se ejecuta AutoCAD.

- **`AllowedWriteScope`:**
  - WEI, `IWorkspaceEditIntentOriginReader.cs` (nuevo), `Draft/PushBackOriginByText.cs` (nuevo), WHC, RIA y NH;
  - las dos clases nuevas y el kit nuevo;
  - **solo para el `Skip`:** `PushBackDraftWriteGuardTests.cs`.
- **El esqueleto:**
  - `FromReads` deja los dos campos en null;
  - `Choose` devuelve null;
  - RIA y NH, sin cambio;
  - WHC, sin cambio;
  - el `Skip`, puesto.
- **`RequiredTests`:**
  - `PushBackFullReadPathTests`: 5, `ExpectRed` true;
  - `PushBackFullReadGuardTests`: 2, false;
  - `Workspace` (a medir) y las de PB-T1 a PB-T3: false.
- **Invariantes:**
  - una `CaptureDrawing` y una `LockDocument` por lectura;
  - `Find<PushBackPilotDraftState>` en las nueve líneas;
  - **WHC solo cambia en las cinco sentencias de lectura.**
- **La línea base:** el GREEN de PB-T3c.

### 6.2 PB-T4b — `F4B-PBT4-FULL-DRAFT-PANEL`

- **Objective** (ASCII):
  > Pasar el borrador del panel del Workspace de Push Back al modelo completo de PB-T3 con la lectura completa de PB-T4a, sin traducir estados: OB-03.2 (los racks A/B se editan en el panel; la cola y AUTH-13 siguen rechazados), la regla unica de la escala de la UI conectada por el Plugin, Actualizar con PrepareFull y ExecuteFull, y Conservar mi borrador en dos tiempos por TryKeepMine (presentacion clasificada por una funcion pura de Draft con un testigo de lo releido, y KeepOn solo tras confirmar con el testigo vigente), segun el paquete del contrato de PB-T4 v3, las decisiones del Owner sobre el piloto, la presentacion y el procedimiento de la lista, la lista cerrada CT-PBT4B-01..17 y la lista de sustituciones que vio el Owner. Si hiciera falta mutar el rack resuelto o el informe de modulos dentro de la vida del borrador: STOP y se reabre L-1. Ni Editar todo, ni RACKEDITAR, ni la ventana; no se ejecuta AutoCAD.
- **`AllowedWriteScope`:**
  - el de la v2, **sin W.xaml**;
  - con la opción 1 de Q4, **EIH, solo para la línea de `EditIntentKit.ReadOf`** (la excepción declarada a C02).
- **`RequiredTests`:**
  - Core `PushBackFullPanel`: 13 + E^R, `ExpectRed` true;
  - Core `PushBackFullPanelWiringGuardTests` y `PushBackFullPanelEquivalenceGuardTests`: 2 + E^G, false;
  - UI `PushBackFullPanelFieldTests`: 2, true;
  - y el resto sin `ExpectRed`.
- **`StopConditions`:**
  - C15 (un fallo con firma fuera de la lista);
  - C16;
  - C13;
  - C17;
  - con la opción 1, **PBT4B-C19:** se edita en EIH algo distinto de la línea de `ReadOf`.
- **La línea base:** el GREEN de PB-T4a.

### 6.3 PB-T4c — `F4B-PBT4-EDIT-ALL-WINDOW`

- **Objective** (ASCII):
  > Anadir Editar todo al panel del Workspace de Push Back sobre el borrador completo de PB-T4b: un boton y su gesto, que confirma el texto pendiente con CommitField antes de abrir (R-PB-10); TryEditAll en el anfitrion y en sus dos reenvios (la compuerta de avisos y el adaptador del panel del Plugin), que empieza el borrador si hace falta, mantiene la marca de lectura mientras la ventana esta abierta (K = 1) y abre la ventana de Push Back en modo WORKSPACE por un anfitrion del editor, una clase anidada del controlador del Plugin que entra en el modal del panel y la muestra sin dueno; el adaptador del puerto que conserva Origin y acaba la marca known clean; la guarda de texto de OB-01.3 sobre los cinco sujetos del camino; Cerrar con una sola pregunta si hay a la vez un modulo escenificado y cambios sin aplicar; y el resumen y Modificado al cerrar, segun el paquete del contrato de PB-T4 v3 y la lista cerrada CT-PBT4C-01..13. Solo se sustituye con registro la guarda de las operaciones del puerto (PEG:156-163). Ni el camino clasico, ni RACKEDITAR, ni la ventana en CLASSIC; cero escrituras en el DWG desde la ventana; no se ejecuta AutoCAD.
- **`AllowedWriteScope`:**
  - WHC, `IWorkspacePilotEditPort.cs`, `WorkspacePilotSupport.cs`, WEI, FVM, PV, WCo y W;
  - los nuevos `IPushBackFullEditorHost.cs`, `Draft/PushBackFullDraftLaunch.cs` y `Draft/PushBackFullDraftStatePort.cs`;
  - **ningún archivo nuevo en el Plugin;**
  - las clases nuevas;
  - **solo para el `Skip`:** PEG.
- **`RequiredTests`:**
  - Core `PushBackEditAll`: 9, true;
  - Core `PushBackOpenerClassicPathGuardTests` y `PushBackEditAllPortEquivalenceGuardTests`: 2, false;
  - UI `PushBackEditAllWindowTests`: 3, true;
  - y el resto.
- **Invariantes:**
  - cero escrituras;
  - W solo cambia `PendingWork()` y su literal;
  - `ForDraft` y `EnterPanelModal()`, un solo llamador cada uno;
  - **los dos reenvíos de `TryEditAll`, uno en cada clase y nada más.**
- **La línea base:** el GREEN de PB-T4b.
'''

S82_9 = r'''
### 8.2 El orden y las preguntas

**El orden:**
1. la v3;
2. su comprobación acotada;
3. el cierre de PB-T3c;
4. la medición de la precondición 6;
5. las cuatro preguntas, en una sola vuelta;
6. la emisión de PB-T4a.

**Q4 decide el procedimiento con el que se mide la lista de Q1.** Si el Owner elige otra opción distinta de la medida, la medición se repite antes de emitir PB-T4b, y Q1 se le vuelve a mostrar con sus números.

**Lo declarado, sin preguntar** (V2-28):
- que pedir «Conservar» lee el dibujo, frente a la regla de F4 de la UI :780-793, con su registro;
- el límite del testigo (M-20);
- el cambio observable de PB-T4a (M-23).

**Q1. El piloto.**

> PB-T4 pasa el borrador del panel de Push Back al modelo completo de PB-T3, sin convertir borradores, como pediste. Al hacerlo, <N> pruebas existentes dejan de describir el panel: <n1> de comportamiento, <n2> guardas de F4 y de PB-T3 y <n3> de la interfaz del panel. Es una selección por texto, con el procedimiento que elijas en Q4: puede incluir pruebas que no fallarían, y es probable que te vuelva a mitad de PB-T4b con alguna más. Cada una se sustituye con registro por otra que conserva todo lo que protegía.
>
> Queda además código del piloto que el panel ya no usará: su comparador, su adaptador de la escala (con su propia copia de la regla, que pediste dejar en una sola), «Conservar» y la composición del piloto, y su Prepare, Precheck y Execute. PB-T1 dejó la copia de la carga del piloto como un modelo paralelo interino que PB-T4 debía retirar, y tu decisión D-1 contaba con quitar el piloto. El lector del piloto y su carga se quedan en todos los casos, porque el modelo completo los usa para la admisión y el rechazo AUTH-13. ¿Qué hace PB-T4 con ese código?

- **«Retirarlo (Recomendado)»:** retira lo que se puede retirar: lo que solo usaba el panel. Se sustituyen además sus <n4> pruebas propias y tres equivalentes de D-1 (E-3, E-5 y E-6). Queda una sola regla de la escala. La copia de la carga sigue dentro del lector.
- **«Conservarlo sin uso»:** no se toca, y sus <n5> pruebas siguen. Quedan, anotados en ideas futuras, el modelo paralelo de PB-T1, la segunda copia de la regla de la escala y la premisa de D-1 sin cumplir.
- **«Dos modelos hasta F5-A»:** el campo de escala del panel sigue con el piloto; «Editar todo» descarta o exige actualizar el borrador del piloto antes de abrir uno completo, leído de nuevo del dibujo. El panel tiene dos códigos de borrador y dos «Conservar mi borrador», y necesita igual la lectura completa de PB-T4a. Cuántas pruebas toca no se puede medir sin diseñar esa variante.

**Q2. La presentación** (I-17). La recomendada es «Por grupos»:
- **los números** (R-23): un diseño de Push Back tiene unas 136 a 165 rutas de hoja (`pb-01-base`, `pb-07-combinado` y `pb-20-ab`, con los índices normalizados). «Por campos» necesita un nombre en español para cada una, y la tabla de propiedad de PB-T1 solo tiene unas 25 de primer nivel;
- **el ruido:** el clasificador informa también de las hojas derivadas (P3:713). Añadir un frente lista posiciones y valores que nadie editó;
- **el espacio:** el panel mide unos 320 px.

Con eso, «Por campos» no es barata y mete ruido.

> Antes de aceptar «Conservar mi borrador», pediste mostrar lo bastante para distinguir los campos (fields) cambiados fuera, los cambiados por el usuario, los cambiados en los dos sitios, los cambios estructurales (módulos, frentes, lado B) y lo que pasa con las vistas, sin un diff JSON. El clasificador de PB-T3 da cada valor cambiado con su grupo (nombre, rack, anotaciones, seguridad, frentes, módulos, lado B, parte compuesta, fuera del editor), y también los valores derivados que cambian solos, como posiciones al añadir un frente. ¿Qué se muestra encima del Sí/No del panel, que mide unos 320 px?

- **«Por grupos (Recomendado)»:** una línea por clase con los grupos afectados, por ejemplo «Cambiado fuera: el nombre y los frentes.», más una línea de lo estructural y otra de las vistas. Son 9 nombres fijos y cabe en el panel. Dice qué zonas cambiaron, no qué campo: es menos que la letra de tu decisión, que dice «fields».
- **«Por campos»:** lo mismo, con el nombre en español de cada campo cambiado. Es la letra de tu decisión. Necesita una tabla de nombres para unas 150 rutas y enseña también los valores derivados que nadie editó.
- **«Por campos, con sus valores»:** como «Por campos», con el valor de antes, el de fuera y el tuyo tras cada campo. Es la más larga.

**Q3. La emisión.**

> PB-T4 va en tres subtareas: PB-T4a, que lee también el modelo completo y relee el origen del borrador, sin cambiar su tipo; PB-T4b, que pasa el panel al modelo completo, con el nuevo «Conservar mi borrador»; y PB-T4c, que añade «Editar todo». Ninguna autorización tuya cubre PB-T4. La lista de pruebas sustituidas de cada una va en Q1 (PB-T4a: <a>; PB-T4b: <b>; PB-T4c: <c>). ¿Emito cada subtarea, en ese orden y cada una tras el cierre de la anterior, sin volver a ti, si su comprobación acotada da 0 bloqueantes y 0 importantes sin resolver, la remedición sobre el cierre anterior no cambia nada material y la lista medida al emitir es la que viste, sin ninguna prueba añadida?

- **«Sí, con esas condiciones (Recomendado)»:** vuelve a ti por una decisión nueva, un control con una firma no prevista, un intento 3, un efecto material sin medir o una prueba fuera de la lista. Si en el GREEN de PB-T4b aparece una prueba con causa cerrada fuera de la lista, el Worker para y te vuelve con ella antes de la corrección.
- **«Solo PB-T4a; las demás me vuelven»**.
- **«No; cada una me vuelve con su lista»**.

**Q4. El procedimiento de la lista** (B-04, I-18).

> Para pasar el panel al modelo completo, las pruebas del panel de F4 empiezan sus borradores con una lectura del piloto que les da un kit de pruebas compartido. Cambiar el tipo del borrador las hace fallar a todas: 428 pruebas en 9 clases, aunque solo unas <m> fallarían por el cambio en sí. ¿Cómo las trato?

- **«Editar una línea del kit compartido (Recomendado)»:** la lectura del kit pasa a traer también el modelo completo; es la única línea que se edita de una prueba o un ayudante existente, con su registro. Las 428 siguen con sus mismas aserciones, ahora sobre el camino real del producto. Solo se sustituyen con registro las <m> que fallan por el cambio. Va contra la regla de no editar ayudantes y contra el «como D-1» de PB-T3, pero es la única opción que deja la lista cerrada, no esconde ninguna implementación y no debilita ninguna prueba.
- **«Saltar y duplicar, como D-1»:** cada una de las 428 se salta y se copia con el kit nuevo, con un guion que comprueba que solo cambia el kit. La lista que verías tiene más de 428 pruebas.
- **«Una implementación de referencia»:** la sesión escribe una para medir la lista exacta, la custodia, y el Worker la recibe como objetivo o no. Si no la recibe, cualquier diferencia suya para el trabajo.

**Los números que ya se pueden dar** (R-21): las 9 clases y sus métodos:

| Clase | Métodos |
|---|---|
| `WorkspaceLifecycleHostTests` | 69 |
| `WorkspacePilotPrepareTests` | 67 |
| `WorkspaceEditIntentHostTests` | 62 |
| `WorkspacePilotEditHostTests` | 59 |
| `WorkspaceCommandAdvisorTests` | 54 |
| `WorkspacePilotUpdateRequestTests` | 44 |
| `WorkspacePilotUpdateHostTests` | 35 |
| `WorkspacePilotReconciliationHostTests` | 28 |
| `WorkspaceAssociativeBookkeepingTests` | 10 |
| **Total** | **428** |

**`<m>`, estimado por texto:** 44, la suma de los métodos de esas clases con alguna firma de C-1 a C-6. Fuera de las 9, más las roturas de C-5 y las de la UI. El número exacto sale de la precondición 6.

### 8.3 Lo que no es decisión

Lo de la v2 (§8.3), salvo el procedimiento de la lista, que pasa a Q4.

---

## 9. Límites

- Nada se compiló ni se ejecutó.
- El worktree no se leyó.
- PB-T3c no está cerrada.
- **Cerrados con su cita** (los `<a medir>` de la v2 que la §13.6 da por medidos):
  - `WorkspaceCommandAdvisorGuardTests:406-450`: verde;
  - EIG:1150-1190: verde;
  - CT-PBT2B-08: verde;
  - la costura de escenificar, `HeaderConfiguratorPresenter` y `ConfigureModuleHeaderButton` (`PushBackHeaderAuthorityWindowTests.cs:99-106`): existe, y CT-PBT4C-09 la usa;
  - la letra de D2: citada en D-PBT4-18;
  - los nombres de `EditIntentStatus`: en §4.2;
  - el miembro con cuerpo por defecto: B-05, resuelto.
- **Quedan para la copia exportada:**
  - K, D y E de cada rama;
  - las roturas de C-5 sobre el texto del GREEN;
  - las líneas de CT-PBT4B-06;
  - el coste de `IsDirty`.
- **`<m>` = 44 es una estimación por texto,** no un recuento de fallos.
'''
