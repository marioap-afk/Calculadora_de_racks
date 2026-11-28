> **Redacción para el repo público:** las rutas locales se sustituyen por marcadores (`<checkout-principal>`, `<carpeta-del-paquete>`). El resto no cambia.

# I-64 F4B — Paquete del contrato de PB-T4 («Editar todo» en el panel del Workspace), v3

**Estado:** v3. Aplica la comprobación acotada de la v2 (`independent-review-v1.md` §13: **NECESITA v3, B/I/M = 2 / 7 / 8**) con las decisiones de la sesión (`v3-instructions.md`). El registro V3 va primero; el de la v1 → v2 se conserva debajo. Las secciones que la v3 no cambia son las de la v2, tal cual. **Mediciones nuevas:** `measure_pbt4_v3.py` y `measure_pbt4_v3-out.txt` (R-21 a R-27 de `pbt4-measurements.md`).

**Cómo se midió:** por objetos git en `7dfc7c21`, la BASE de PB-T3c (F4B-E47). PB-T3a y PB-T3b están CERRADAS; PB-T3c, EMITIDA y en curso. Lo que depende de su GREEN lleva **`<tras el cierre de PB-T3c>`**.

**No se emite.** El orden es: v2 → comprobación acotada → cierre de PB-T3c → medición de la lista de sustituciones → preguntas al Owner → emisión (§8.2).

**Los hechos,** en `pbt4-measurements.md` (R-0 a R-20). **Los guiones:** `measure_pbt4.py` y `measure_pbt4_v2.py`, con sus salidas `measure_pbt4-out.txt` y `measure_pbt4_v2-out.txt`. La revisión midió además con `ir4_measure.py` (`ir4_measure-out.txt`).

**Abreviaturas:**
- [P], [OD], [P2] y [P3]: el paquete de la fase v3.1, la disposición del Owner y los paquetes de PB-T2 v2.9 y PB-T3 v4.7;
- [PT1]: el paquete de PB-T1, `docs/initiatives/I-64-f4b-pbt1-gate-contract-package.md`;
- [PKC2]: el paquete del contrato de F4-T4C2, del que F4B copia las paradas estándar (como [P3] §6.0);
- DISP: `F4B-DRAFTS/f4b-dispositions.md`;
- WHC: `src/RackCad.Application/Workspace/WorkspaceHostCore.cs`;
- WEI: `src/RackCad.Application/Workspace/WorkspaceEditIntent.cs`;
- NH: `src/RackCad.Plugin/Workspace/WorkspaceNavigationHost.cs`;
- RIA: `src/RackCad.Plugin/Views/RackPilotIntentAdapter.cs`;
- FVM: `src/RackCad.UI/Workspace/WorkspacePilotFieldViewModel.cs`;
- PV: `src/RackCad.UI/Workspace/WorkspacePanelView.xaml(.cs)`;
- W: `src/RackCad.UI/Systems/PushBack/RackPushBackSystemWindow.xaml(.cs)`;
- WCo: `src/RackCad.Plugin/Workspace/WorkspaceController.cs`;
- PP y UR: el plan y la corrida de `Systems/PushBack/Pilot/`;
- EIH: `tests/RackCad.Tests/Workspace/WorkspaceEditIntentHostTests.cs`;
- EIG: `…/WorkspaceEditIntentGuardTests.cs`;
- PEG: `…/WorkspacePilotEditGuardTests.cs`;
- RG: `…/WorkspacePilotReconciliationGuardTests.cs`;
- URT: `…/WorkspacePilotUpdateRequestTests.cs`;
- IG: `…/WorkspacePilotUpdateIntegrationGuardTests.cs`;
- F5-A: la fase de I-64 que sigue a F4B (la del optimizador del layout), donde el roadmap aplaza lo que F4B no hace.

---

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

## Registro de cambios v1 → v2

| Id | Hallazgo | Qué cambia | Dónde |
|---|---|---|---|
| V2-01 | **B-01** (el anfitrión no recibe la lectura completa) | **PB-T4a nueva, solo aditiva:**<br>• el adaptador del Plugin (RIA) lee también el modelo completo sobre el mismo escaneo;<br>• `EditIntentReadResult` lleva la lectura completa y el catálogo junto a la del piloto (`FromReads`); `FromPilot` no cambia;<br>• la relectura de un borrador abierto admite el origen.<br>Los archivos del Plugin entran en el alcance; el censo de PB-T3 se amplía; `EditIntentKit` no se toca y hay un kit nuevo | D-PBT4-02 y 03; §6.1 |
| V2-02 | **B-02** (la regla en el Plugin, sin prueba posible) | **La regla única vive en la UI** (`AnnotationScaleRule`, en `src/RackCad.UI/Controls/`), sobre `NumericFieldValidation`. La caja `AnnotationScaleBox` toma de ella sus parámetros (`x:Static`), y la prueba la llama desde la UI. El Plugin solo la conecta al anfitrión (`Func<string, DraftFieldValidation>`, un tipo de `Workspace.Drafts`). La copia del adaptador del piloto se trata en Q1 | D-PBT4-06 |
| V2-03 | **B-03, I-03 e I-04** (la lista) | **El procedimiento de la lista,** con causas cerradas y firmas, y cuatro opciones que la sesión elige. Las preguntas se hacen después de medir | D-PBT4-11; §8.2 |
| V2-04 | **I-01** (K = 1) | **Un solo mecanismo,** con los estados por operación:<br>• el abridor del Plugin pone el modal (`EnterPanelModal()`, el único conductor admitido de la bandera);<br>• `TryEditAll` mantiene la marca de lectura durante `ShowModal`: las operaciones del campo y `DiscardRack` dan `Busy`, y las de intención, `Waiting`, sin comprobaciones nuevas | D-PBT4-15 |
| V2-05 | **I-02** (los miembros nuevos del puerto) | **Sin `PilotBoundaryKind.EditAll`:** el gesto usa `CommitField` como frontera, y URT:1107-1109 sigue en verde.<br>**«Conservar» sin miembro nuevo:** usa `TryKeepMine` en dos tiempos, con el testigo.<br>**El único miembro nuevo es `TryEditAll`** (PB-T4c), que rompe PEG:156-163: va a la lista.<br>Medidas y citadas PEG:156-163, :280-281 y :380-385 | D-PBT4-08, 13 y 14; §4 |
| V2-06 | **I-05** (Q1) | Q1 con huecos, el coste de conservar el piloto (A-5, R-T1-17, la premisa de D-1 y la regla duplicada) y una opción de retirada realista: se retira lo que solo usaba el panel y se conserva el lector | §8.2 |
| V2-07 | **I-06** (D2) | CT-PBT4B-10 con las filas de `DiscardAndReload`, `SetPendingText` y `CommitField`, y la condición 7 (sustituir una de las dos referencias en `Current` no cambia `IsDirty`); la huella, tras cada operación | D-PBT4-18; §5.3 |
| V2-08 | **I-07** (`NaN`) | CT-PBT4B-11 pone el `NaN` en una longitud de módulo del commit (A-9, `RackModuleCommit.SameAs`) | §5.3 |
| V2-09 | **I-08** (los literales) | La presentación, parametrizada por Q2, con la unión, el plural y las vistas rechazadas fijados. Las líneas de CT-PBT4B-06 se miden antes de emitir con el clasificador real | D-PBT4-09 |
| V2-10 | **I-09** (los ayudantes y la disciplina de lectura) | La lista nominal cerrada por contrato. Las reglas de lectura de `TryEditAll`, copiadas de `TryKeepPilotEdit`. El equivalente de PEG amplía sus listas | §6.0; D-PBT4-14 |
| V2-11 | **I-10** (el Smoke-PB) | §5.5 y §10 ampliados con todo lo que solo existe en el Plugin | §5.5; §10 |
| V2-12 | **I-11** (Q2) | Q2 con «Por grupos», «Por campos» y «Por campos, con sus valores», sin «Solo recuentos» ni «no abre diálogos». Recomendada: «Por campos» | §8.2 |
| V2-13 | M-01 | 17 operaciones (PEG:44-65) | §2; R-2 |
| V2-14 | M-02 | G-4 exige que el conjunto de archivos que nombran `PushBackPilotUpdateRun.Execute(` sea solo WHC (IG:497-498) | §4 |
| V2-15 | M-03 | `NotePilotEdit(SessionId)` | D-PBT4-13 |
| V2-16 | M-04 | Las relecturas de A-T3-8 son las de un borrador abierto (WHC:409, :503, :590, :675 y :759), no `Begin` (:328); `Precheck` no lee; POST ya relee por `Origin` | D-PBT4-03 |
| V2-17 | M-05 | Los cinco archivos del camino, nombrados en CT-PBT4C-07 | D-PBT4-16 |
| V2-18 | M-06 | Las cuentas de cada rama, rehechas | §5.4 |
| V2-19 | M-07 | `PushBackDraftSessionPort` y `LaunchFor`: se conservan **con registro** en DISP. A-T3-9: registro en DISP y en `ideas-futuras.md` | D-PBT4-13; D-PBT4-20 |
| V2-20 | M-08 | [PKC2] y F5-A, definidos | Abreviaturas |
| V2-21 | M-09 | El abridor muestra la ventana con `ShowModalWindow`, **sin dueño WPF**, como el clásico (`PushBackClassicCharacterizationPluginGuardTests.cs:196-209`) | D-PBT4-16 |
| V2-22 | M-10 | Las citas corregidas: la forma del resultado (EIH:1175-1194) y el cableado (EIG:1150-1190) | §4 |
| V2-23 | M-11 | La guarda de «sin traducción» mira WHC, WEI, `WorkspacePilotSupport.cs`, RIA y NH | CT-PBT4B-12 |
| V2-24 | M-12 | CT-PBT4C-09 lee `PendingWork()` sin cerrar la ventana; la costura de escenificar se mide; la petición de «Editar todo» y los estados Unknown, Error y Undecidable, fijados | D-PBT4-14 y 17 |
| V2-25 | M-13 | Una medición del coste de `IsDirty` en la corrida de medición (precondición 6) | §7 |
| V2-26 | M-14 | `UNIT_CHANGE` completa. `IPushBackFullEditorHost` se declara en `Application/Workspace` por EIH:1088 (un solo campo de tipo de `Systems`), frente a R-PB-05 y ADR-0029 D12 | §6.0; D-PBT4-15 |
| V2-27 | M-15 | CT-PBT4C-10 lleva una línea tras un gesto compuesto (F4B-E30) | §5.4 |
| V2-28 | §8 de la revisión, lo declarado | Que pedir «Conservar» lee el dibujo, frente a la regla de F4 de la UI :780-793, con su registro. Y el procedimiento de la lista | D-PBT4-08; D-PBT4-11 |
| V2-29 | La sesión | **Tres subtareas** (PB-T4a, PB-T4b y PB-T4c) y sus TaskId | D-PBT4-01 |

---


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

## 1. Autoridades

Las de la v1 (§1), sin cambio. Además, la revisión de la v1 y las instrucciones de la sesión. Las principales:

| Autoridad | Lo que manda aquí |
|---|---|
| **[OD]**, la fila de PB-T4, D-PB-01 y D-PB-05 | Un editor completo, modal, con borrador dentro y fuera y cero escrituras en el DWG; los pasos 1 a 7; «an informed destructive choice»; sin traducción de estado |
| **[P]**: OB-01.1 a 6, OB-03.2, OB-05.1 a 5, R-PB-10, §7.3 (OB-T1B.1 a 3) y §7.4 | |
| **[P2] §10** (A2-1 a A2-13) y **[P3] §10** (A-T3-1 a A-T3-11) | |
| **[PT1]**: A-5 («La copia de la carga del piloto … PB-T4 la retira al consumir el API», :1549), A-9 (:1553), A-10 (:1554) y R-T1-17 (:1367) | |
| **F4B-E28 Q3; F4B-E38; F4B-E29 (D-1); F4B-E11 (D2) con L-5; F4B-E24; la nota del Owner sobre la interfaz; F4B-E03; E15, E21, E25, E35 Q4, E37, E42, E45 y E46; D-PB-10** | |

---

## 2. Hechos medidos en `7dfc7c21`

Los de la v1 (R-1 a R-12), corregidos, y los nuevos (R-13 a R-20). Lo esencial:

| Hecho | Fuente |
|---|---|
| **El resultado de la lectura solo lleva el piloto:** `EditIntentReadResult(status, reason, PushBackPilotIntentResult? pilotRead)`, con las fábricas `FromPilot`, `NoActiveDocument`, `Busy`, `Rejected` y `Failed` | WEI:35-64 |
| **El Plugin lee:** NH `ReadIntent` → `ReadOnActiveDocument` → `RackPilotIntentAdapter.LoadCatalog()` fuera del bloqueo → `LockDocument` → `ChooseOriginDefinition(revalidation.Fresh, chosenReference)` → `RackPilotIntentAdapter.Read(document, rackId, origin, witnessKey, witnessText, catalog)` → `EditIntentReadResult.FromPilot(read.Pilot)` | NH:207-251; WEI:682-692 |
| **El escaneo** es `new PushBackPilotIntentScan(rackId, originKey, scan.Facts, scan.RawEnvelopes, CompletenessEvidence, witness)`. `PushBackFullDraftReader.Read` recibe ese mismo tipo y llama primero al lector del piloto | RIA:116-121; `Draft/PushBackFullDraftReader.cs:51-59` |
| **`PushBackFullIntentResult` no lleva el testigo;** el resultado del piloto sí (`PreviewWitness`) | `Pilot/PushBackPilotIntentResult.cs:25-59`; `Draft/PushBackFullDraftReader.cs:12-41` |
| **Las seis lecturas del anfitrión:** `ReadIntent(request.Session, request.RackId, request.ChosenReference, request.Witness)` en WHC:328 (`Begin`), :409, :503, :590, :675 y :759; seis `var pilot = read.PilotRead;` | R-13 |
| **EIG:733-736:** ningún `.PilotRead` fuera de esas asignaciones; exactamente 6 asignaciones y 6 `.ReadIntent(` | EIG:725-737 |
| **EIG:414-430:** NH llama una vez a `RackPilotIntentAdapter.LoadCatalog()` y una vez a `RackPilotIntentAdapter.Read(` | R-14 |
| **EIH:1088:** WHC tiene exactamente un campo cuyo tipo, o algún argumento genérico, es de `RackCad.Application.Systems` (`_updateHost`) | EIH:1075-1099 |
| **EIH:52:** el kit de lecturas de todo el anfitrión construye con `FromPilot`; 12 usos de `FromPilot` en las pruebas | R-13 |
| **Las 17 operaciones del puerto** (PEG:44-65), fijadas por reflexión (PEG:159-161); las seis de lectura (:68); `WorkspaceHostCore : IWorkspaceLifecycleAdvisor, IWorkspacePilotEditPort` y nada más (:380-385); el adaptador del piloto es el único campo estático del anfitrión (:280) | R-15 |
| **URT:1107-1109** fija los 7 nombres de `PilotBoundaryKind` | R-15 |
| **K = 1:**<br>• `SetPendingText`, `CommitField`, `CrossBoundary`, `DiscardRack` y `DiscardDocument` miran la marca `_pilotReadInProgress` y dan `Busy`; no miran el modal;<br>• `PassGate` y `UpdateGate` miran el modal y dan `Waiting`;<br>• `PilotFieldStatus` = { Rejected, Applied, NotEditable, Busy, Invalid } | WHC:1127, :1155, :1208, :1243, :1270, :1940, :996; `IWorkspacePilotEditPort.cs:143-157` |
| **La regla de la UI de F4:** «Asking for an option calls no operation of the port and only reads its memory» | `tests/RackCad.UI.Tests/Workspace/WorkspacePilotReconciliationTests.cs:780-793` |
| **RG:116** muta `public void RequestKeepMine() => AskChoice(ChoiceKind.KeepMine);` | R-15 |
| **La UI de la reconciliación:** 40 llamadas a `RequestKeepMine` o `ConfirmChoice` en `tests/RackCad.UI.Tests/Workspace/` | R-15 |
| **Las pruebas no referencian el Plugin:** `RackCad.Tests.csproj:23-31`, `RackCad.UI.Tests.csproj:25-27` | R-16 |
| **La caja de la escala:** `<controls:NumericField x:Name="AnnotationScaleBox" … Minimum="0" MinimumInclusive="False" />`; `NumericField` valida con `NumericFieldValidation.Validate` | W.xaml:68-69; `src/RackCad.UI/Controls/NumericField.cs:149` |
| **`IDraftPendingTextAdapter<TState>`:** `DraftFieldValidation Validate(string fieldKey, string text)` y `TState Apply(TState current, IReadOnlyDictionary<string, string> pendingText)`; `DraftFieldValidation(bool IsValid, string? Message)` | `Drafts/DraftSession.cs:95-110` |
| **El modelo completo usa el piloto:** las constantes de `PushBackPilotMaterializer` (en `Draft/` 15 veces) y `PushBackPilotIntentReader.Read` (el lector completo); el lector del piloto usa `PushBackPilotMaterializer.Materialize` | R-17 |
| **`NaN`:** `PalletTolerance` se compara con `double.Equals` (`NaN` es igual a `NaN`); `RackModuleCommit.SameAs` compara longitudes con `!=` | `PushBackEditableStateComparer.cs:409`; `Systems/Shared/RackModuleEditSession.cs:76-` |
| **El abridor clásico** muestra la ventana con `AcApplication.ShowModalWindow(window)` y sin asignar `Owner` | `PushBackClassicCharacterizationPluginGuardTests.cs:196-209` |
| **La suite de UI** corre en un STA compartido con un `MessageBox` modal real | `.github/workflows/ci.yml:213-222` |

---

## 3. El diseño

### D-PBT4-01. Tres subtareas secuenciales

**Por qué tres** (§6 de la revisión):
- PB-T4a cierra el camino de la lectura completa sin cambiar el comportamiento del panel. Es aditiva y deja medido lo que necesitan PB-T4b y PB-T4c.
- PB-T4b concentra el cambio de tipo y su lista.
- PB-T4c es casi solo aditiva.

**La secuencia:**
1. PB-T4a sobre el cierre de PB-T3c.
2. PB-T4b sobre el de PB-T4a.
3. PB-T4c sobre el de PB-T4b.

Cada una con su 1/3 (OB-10.1), en delegaciones sucesivas, sin rebase.

**La división se sostiene con lo medido.** Las sustituciones por letra de PB-T4a son dos (§4.1). El único bloque grande de sustituciones es el de PB-T4b, y es inevitable mientras el tipo del borrador cambie.

**Cada TaskId empieza por `F4B-PBT4-`,** con 24 o 25 caracteres. Así los sustitutos de nc1 y nc2 se usan con `--task PBT4` sin editarlos (§8.3).

### PB-T4a

#### D-PBT4-02. La lectura completa, junto a la del piloto (B-01)

**En Application** (WEI), aditivo:
- `EditIntentReadResult` gana dos propiedades: `PushBackFullIntentResult? FullRead` y `RackCatalog? Catalog`.
- Gana una fábrica: `public static EditIntentReadResult FromReads(PushBackPilotIntentResult pilotRead, PushBackFullIntentResult fullRead, RackCatalog catalog)`. Si alguno es nulo, `ArgumentNullException`.
- `FromPilot` no cambia: deja `FullRead` y `Catalog` en null, así que `EditIntentKit` (EIH:51-52) y los 12 usos de `FromPilot` siguen como están.
- El constructor privado gana los dos argumentos. Los demás estados los dejan en null.

**En el Plugin** (RIA y NH):
- `RackPilotIntentReadResult` gana `PushBackFullIntentResult? Full`.
- `RackPilotIntentAdapter.Read`, justo después de `PushBackPilotIntentReader.Read(input, catalog)` (RIA:121), llama una vez a `PushBackFullDraftReader.Read(input, catalog)` sobre el **mismo** `input`, y devuelve los dos.
- NH:250 pasa a `EditIntentReadResult.FromReads(read.Pilot, read.Full, catalog.Catalog)`.

**Lo que no hace:**
- **Nada traduce:** el completo sale del lector completo sobre el mismo escaneo (P3:530, A-T3-2), y no del resultado del piloto.
- **Ninguna lectura nueva del dibujo:** el escaneo es el mismo, dentro del mismo bloqueo; no hay segundo `LockDocument` ni segundo `CaptureDrawing`.
- **El anfitrión no usa todavía `FullRead`:** lo usa PB-T4b.

**Lo que necesita `Classify`:**
- los sobres de los miembros salen de `FullRead.StaleBase` (las entradas con su texto);
- el catálogo, de `Catalog`.

**El testigo** se queda en `PilotRead.PreviewWitness`, porque el lector completo lo calcula con el mismo lector del piloto.

**Las guardas del Plugin** (R-14):
- EIG:414-430 (una `LoadCatalog()` y una `RackPilotIntentAdapter.Read(` en NH) sigue: no se añade ninguna;
- G-9 sigue con sus cuentas: RIA conserva `StartTransaction` 1 y `LockDocument` 1, y NH `LockDocument` 2;
- INV-14 (`AssertNoOpenTransaction` antes y después) sigue;
- `WorkspaceCommandAdvisorGuardTests` lee NH y RIA (:21-22): `<a medir en la comprobación del §6.0>`.


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

### PB-T4b

#### D-PBT4-04. El borrador del panel es el completo (A-T3-1)

**El anfitrión:**
- **`Begin`:** `_drafts.Begin(session, rackId, read.FullRead.Read, PushBackFullDraftStateComparer.Instance, PushBackFullDraftStateComparer.Copy)`.
- **La admisión y su diagnóstico** salen de `FullRead`, y el testigo de `PilotRead.PreviewWitness`.
- **Sin `FullRead`** (un lector que devuelve `FromPilot`): «No editable aquí: la lectura no trae el modelo completo.» (`FullReadMissingReason`), y no nace ningún borrador.
- **Los nueve `Find<PushBackPilotDraftState>`** pasan a `Find<PushBackFullDraftState>`.
- **Las seis asignaciones** pasan a `var full = read.FullRead;`. El testigo se lee con `read.PilotRead.PreviewWitness` en las tres que lo usan (WHC:344, :519 y :691), y en ningún otro sitio.

**Nada traduce:**
- un borrador del piloto no existe tras el GREEN, porque el tipo se fija en `Begin`;
- el registro se vacía al cerrar el documento, como hoy.

**Lo que pasa con el código del piloto** depende de Q1 (D-PBT4-10).

#### D-PBT4-05. OB-03.2

Explícito y con su prueba:
- un rack A/B del corpus (`pb-20-ab` y los demás compuestos) abre su borrador en el panel;
- `pb-21-cola` sigue rechazado con `OutOfPilotDiagnostic` y su `ClassicEditorHint`;
- AUTH-13 sigue: el lector completo pasa primero por el del piloto.


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

#### D-PBT4-07. «Actualizar» con la escritura completa

- `TryUpdatePilot` conserva su nombre y su firma (G-10 y los llamadores de EIG).
- Su cuerpo pasa a `PushBackPilotUpdateRun.Precheck(draft)` y `.ExecuteFull(draft, request, port)`.
- **`<tras el cierre de PB-T3c>`:** el escritor borra vistas.
- **G-4** (IG:497-498) espera que WHC sea el único archivo que nombra `PushBackPilotUpdateRun.Execute(`. Tras el cambio, ninguno lo nombra: va a la lista por letra (§4.2).


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

#### D-PBT4-09. La presentación, parametrizada por Q2 (I-08)

`PushBackKeepMinePresentation.Of(PushBackConflictClassification c, PushBackPresentationForm form)` es pura, en `Draft/`. Devuelve `Lines` e `IsEmpty`. Q2 fija `form`, y el contrato lleva solo la elegida.

**Las tres clases,** en este orden y solo si tienen entradas: «Cambiado fuera: …», «Cambiado por ti: …» y «Cambiado en los dos: …». Lo que va detrás de los dos puntos depende de la forma:

| Forma (Q2) | El contenido de cada línea |
|---|---|
| **Por grupos** | Los grupos, en el orden de `PushBackChangeGroup` (Name, Rack, Annotations, Safety, Fronts, Modules, SideB, Composite, OutsideEditor), con su literal: «el nombre», «el rack», «las anotaciones», «la seguridad», «los frentes», «los módulos», «el lado B», «la parte compuesta», «fuera del editor» |
| **Por campos** | Los campos, en el orden de las entradas del clasificador, con el literal de su ruta en la tabla de nombres (`PushBackFieldLabels`, una entrada por cada ruta del mapa de propiedad de PB-T1, `PushBackEditableStateOwnership.Table`). Una ruta indexada se nombra por su campo y su índice («el módulo 3: longitud») |
| **Por campos, con sus valores** | Como «Por campos», con « (antes <b>, fuera <c>, tuyo <d>)» tras cada campo, con «R» y cultura invariante |

**La unión:** «A», «A y B», «A, B y C». **El punto final,** siempre.

**Las demás líneas,** en las tres formas:
- «Cambios estructurales: <la misma enumeración, solo las estructurales>.» si `Structural` no está vacío;
- «Vistas: se borrará 1 lateral obsoleto.» o «Vistas: se borrarán <n> laterales obsoletos.» según `ViewsToErase`;
- «Vistas: <n> no se pueden conservar.» según `ViewsRejected`, que son claves y no motivos;
- «Todas las vistas quedarían obsoletas.» si `AllViewsObsolete`.

**Las líneas de CT-PBT4B-06** se miden antes de emitir con el clasificador real (PB-T3a, ya cerrado), en la corrida de medición de la precondición 6, porque cambiar los frentes cambia también hojas de otros grupos (P3:713). El contrato las lleva literales.

#### D-PBT4-10. Lo que pasa con el código del piloto (Q1)

**Lo que el panel deja de usar:**
- el comparador (`PushBackPilotStateComparer`);
- el adaptador del campo (`PushBackPilotFieldAdapter`);
- `KeepPilotEditsOn` y `ComposeForWrite`;
- `PushBackPilotUpdatePlanner.Prepare` (del piloto);
- `PushBackPilotUpdateRun.Precheck` y `Execute` (del piloto).

**Lo que se queda en los dos casos,** porque el modelo completo lo usa (R-17):
- el lector del piloto (`PushBackPilotIntentReader`) y su materialización (`PushBackPilotMaterializer.Materialize`);
- el tipo `PushBackPilotDraftState` que devuelve ese lector;
- las constantes de `PushBackPilotMaterializer`.

**Con «Retirar»:**
- se borra lo de la primera lista;
- sus pruebas propias se sustituyen con registro (causa C-7), y también E-3, E-5 y E-6 de D-1 (P3:1228-1236), que exigen que `Execute` y `ViewUnavailablePrefix` existan;
- **la copia de la carga de A-5** (PM:150-172, dentro de `Materialize`) **no se puede retirar entera**, porque el lector la usa. Se declara: queda solo como la admisión del lector;
- queda una sola regla de la escala.

**Con «Conservar»:**
- no se toca nada y sus pruebas siguen;
- quedan el modelo paralelo de R-T1-17 y A-5, y la segunda copia de la regla de la escala, anotados en `ideas-futuras.md` con su registro en DISP;
- la premisa de D-1, «al quitar el piloto», queda pendiente.


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

### PB-T4c

#### D-PBT4-12. El gesto, con `CommitField` como frontera (R-PB-10; I-02)

- **El botón:** `BrowserFieldEditAllButton`, `Content="Editar todo"`, `AutomationProperties.Name="Editar el rack completo en el editor"`. Va en el `WrapPanel` de PV:218-254, tras «Editar». Lo maneja `OnBrowserFieldEditAllClick` con `Contained(() => _viewModel.Browser.Field.EditAll())`. Es visible con `Browser.Field.ShowEditAllButton`, en las mismas condiciones que «Editar».
- **El gesto `EditAll()`** en FVM:
  1. si el campo tiene texto pendiente, `_port.CommitField(origin, rack)`, «the same two-phase step as a blocking boundary» (WHC:1149):
     - `Invalid`: no abre, y muestra «No se abrió el editor: <mensaje>.»;
     - `Busy` o `Rejected`: no abre, y muestra su motivo;
     - `Applied` o `NotEditable` por falta de borrador: sigue;
  2. `_port.TryEditAll(new EditIntentRequest(session, rack, null, null))`.
  
  La petición lleva `ChosenReference` nulo, porque el origen lo guarda el anfitrión (D-PBT4-03), y sin testigo.
- **Sin valor nuevo en `PilotBoundaryKind`:** URT:1107-1109 sigue en verde.
- **El anfitrión no abre si el borrador aún tiene texto pendiente,** por si el gesto no lo confirmó: «Hay texto sin confirmar en el campo.».


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

#### D-PBT4-17. «Cerrar» con las dos cosas (A-T2b-OWNER-01; M-12)

`PendingWork()` de W (:3427), **solo en WORKSPACE**:
- **un módulo escenificado y cambios sin aplicar:** una pregunta, `WorkspaceStagedAndUnappliedQuestion` = «Hay un cambio de módulo sin confirmar y cambios sin aplicar al borrador; se perderán los dos al cerrar. ¿Deseas continuar?»;
- **uno solo:** la de hoy;
- **en CLASSIC:** nada cambia.

**La prueba** (CT-PBT4C-09) **lee `PendingWork()` y no cierra la ventana:** la suite de UI tiene un `MessageBox` real en su STA (`ci.yml:213-222`).

**La costura para escenificar un módulo sin el configurador,** `<a medir en la comprobación del §6.0>`. Las candidatas son las de `PushBackWorkspaceSupport.cs` y las de las pruebas de PB-T2b. Si ninguna sirve, es C02 y vuelve a la sesión antes de emitir.

### Común


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

#### D-PBT4-19. Lo que PB-T4 no hace

- No toca la ventana clásica, RACKEDITAR, RPB ni el menú. W solo cambia la pregunta de D-PBT4-17 (en WORKSPACE) y los dos atributos `x:Static` de D-PBT4-06.
- No crea vistas.
- No traduce estados.
- No usa el asiento en `Recompute` (A-T3-9).
- No abre AUTH-13.
- No cambia `DormantTailOutOfPilotReason` ni el camino clásico.
- No añade botones ni campos fuera de «Editar todo», el texto de la presentación y la pregunta de «Cerrar».
- No cachea las pasadas.

#### D-PBT4-20. Los arrastres y las limitaciones

| Arrastre o limitación | Dónde | Por qué |
|---|---|---|
| A-T3-1, A-5 y R-T1-17 | PB-T4b, con Q1 | D-PBT4-10 |
| A-T3-2 (OB-03.2) | PB-T4b | D-PBT4-05 |
| A-T3-3 (`KeepOn`; la presentación; las guardas «sin conectar»; el nombre `""`) | PB-T4b. El nombre `""` sale como «Cambiado por ti: el nombre» (o el campo) | D-PBT4-08 y 09 |
| A-T3-4 (D2, M11, M15, F4B-E30) | D2 en PB-T4b y PB-T4c; M11 no aplica; M15 se mide (§7); F4B-E30, una línea en CT-PBT4C-10 | D-PBT4-18 |
| A-T3-8 | PB-T4a | D-PBT4-03 |
| A-T3-9 | Fuera: registro en DISP y entrada en `ideas-futuras.md` (M-07) | CT-PBT3A-01 y CT-PBT2B-02 ya fijan la igualdad; tocar `Recompute` arriesga T1B |
| A-T3-10 | Con «Retirar», las constantes se quedan en `PushBackPilotMaterializer` (el lector las usa); con «Conservar», no aplica | D-PBT4-10 |
| A-T3-11 y A2-1 | PB-T4c | D-PBT4-13 |
| A2-2, A2-3 y A2-9 | PB-T4c | D-PBT4-12 a 16 |
| A2-6 (la regla) | PB-T4b | D-PBT4-06 |
| A2-11 | CT-PBT4C-10, sin AutoCAD; lo físico, el Smoke-PB | §5.5 |
| A2-13 y A-9 (`NaN`) | CT-PBT4B-11 | D-PBT4-18 |
| A-T2b-OWNER-01 | PB-T4c | D-PBT4-17 |
| OWNER-01 de PB-T2c (lado B sin frentes) | CT-PBT4B-09 | D-PB-08 |
| OWNER-01 de «Restaurar»; PBT3B-IR-M01 y OWNER-2; el configurador | Fuera o regla, como en la v1. Se vuelve a medir OWNER-2 en `ideas-futuras.md` tras el cierre de PB-T3c | |

---


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

## 5. La red de pruebas

### 5.0 Reglas comunes

Las de la v1 (§5.0), con estos cambios:
- **Un `[Fact]` por CT, sin `[Theory]` nueva** (F4B-E46).
- **Los kits nuevos van en archivos nuevos:** `PushBackFullReadKit` (las lecturas `FromReads` sobre `IntentKit`) y los dobles del lector con origen, del puerto del panel y del anfitrión del editor. `EditIntentKit`, `WorkspacePilotEditPortDouble` y los demás no se editan.
- **Ninguna prueba de UI** crea un configurador, cierra una ventana con trabajo pendiente ni toca el perfil.
- **Las líneas se toman en el momento justo:** la huella, tras cada operación.


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

### 5.4 La CI del RED

`tests` y `ui-tests` en failure, `build-ui` en success y **`build-plugin` omitido**. Con PB-T4a, `ui-tests` sale en success: es la única que no toca la UI.

### 5.5 Lo que queda para el Smoke-PB (PB-T5, D-PB-09; I-10)

Lo de la v1:
- el botón abre la ventana modal real;
- el dibujo no se edita con ella abierta (OB-01.4);
- el borrado físico (PB-T3c);
- A2-11 hasta el DWG;
- el coste;
- «Restaurar»;
- OWNER-2.

**Y lo que solo existe en el Plugin:**
- **la lectura completa en AutoCAD** (PB-T4a): un rack A/B se abre en el panel (OB-03.2);
- **la relectura con el origen tras borrar la vista de origen** (A-T3-8 con POST reanclado);
- **la regla única cableada:** los mensajes del campo del panel y de la caja son iguales en AutoCAD;
- **el abridor cableado:** la ventana se abre sin dueño, y nada se escribe tras cerrarla;
- **el paso 3 con un cambio real del dibujo** entre la presentación y el «Sí»;
- **K = 1 con el modal real:** la paleta deshabilitada.

Los racks A/B fabricados se rotulan «CONSTRUCTED REPRESENTATIVE TEST CASES».

---


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

## 7. Riesgos y lecciones aplicadas

Los de la v1 (R-T4-01 a 15), con estos cambios:

| Riesgo | Qué hace la v2 |
|---|---|
| R-T4-01, la lista | La lista ya no se fija contra una referencia oculta: opción 1 de D-PBT4-11, causas cerradas con firma, `Theory` y equivalentes con reglas |
| R-T4-08, los miembros por defecto | Solo uno, `TryEditAll` (D-PBT4-13), con su sustitución de PEG:156-163 medida. «Conservar» va por `TryKeepMine`, sin miembro nuevo |
| R-T4-13, el modal | Un mecanismo, con los estados por operación (D-PBT4-14 y 15) |
| **R-T4-16. La lectura completa duplica el trabajo de la lectura** (el lector completo vuelve a llamar al del piloto) | El lector del piloto se llama dos veces sobre el mismo escaneo en memoria, sin otra lectura del dibujo. Se mide el coste en la corrida de medición; si pesa, una optimización posterior, sin cambiar el contrato |
| **R-T4-17. El coste de `IsDirty` en el camino caliente** (M-13; A-10: 27,7 s en CT-PBT1B-06) | Se mide en la corrida de medición (precondición 6), con `GetField` y la pestaña sobre `pb-20-ab`. Si pasa de unos 50 ms por llamada, vuelve a la sesión antes de emitir PB-T4b |
| **R-T4-18. La vuelta al Owner a mitad de PB-T4b** (opción 1) | Q3 la prevé. La corrección rehace el RED con los `Skip` nuevos |

---

## 8. Las decisiones del Owner

### 8.1 Ya tomadas

Las de la v1 (§8.1).


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

## 10. Lo que queda para PB-T5

- El Smoke-PB, con la lista de §5.5.
- La CI exacta del Candidato.
- El gate-close de F4B.

## 11. Las precondiciones de emisión

1. **PB-T3c CERRADA** (`<tras el cierre de PB-T3c>`) y su remedición.
2. **La revisión de la v2** y la comprobación acotada de cada subtarea.
3. **La medición de la precondición 6.**
4. **Las respuestas a Q1, Q2 y Q3,** dentro de la `AuthorityRevision`.
5. **Las celdas, la Snapshot y el binario** vigentes.
6. **La comprobación sobre una copia exportada:**
   - el esqueleto exacto de cada subtarea compila;
   - la suite completa de Core y los filtros de UI dan lo previsto;
   - las listas, según la opción elegida;
   - el barrido de nombres y textos nuevos contra las guardas;
   - las `<a medir>` de §9.
7. **La custodia,** en el commit de emisión de PB-T4a:
   - el paquete y el registro de mediciones;
   - las revisiones;
   - los guiones y sus salidas;
   - la lista medida;
   - los registros declarados (D-PBT4-08, D-PBT4-13 y A-T3-9).
8. **Para PB-T4b y PB-T4c:** la anterior CERRADA y su remedición.
