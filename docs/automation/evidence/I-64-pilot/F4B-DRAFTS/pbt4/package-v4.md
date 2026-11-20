> **Redacción para el repo público:** las rutas locales se sustituyen por marcadores (`<checkout-principal>`, `<carpeta-del-paquete>`). El resto no cambia.

# I-64 F4B — Paquete del contrato de PB-T4 («Editar todo» en el panel del Workspace), v4

**Estado:** v4. Aplica la recomprobación de la v3 (`independent-review-v1.md` §14: **NECESITA v4, B/I/M = 2 / 3 / 4**) con las decisiones de la sesión (`v4-instructions.md`). **La base es ahora el cierre de PB-T3c,** `7f0d9aa3` (F4B-E48; GREEN `938a35f0`): PB-T3a, PB-T3b y PB-T3c están CERRADAS. Lo medido en `7dfc7c21` sigue valiendo, porque entre las dos `src/` y `tests/` solo cambian en el escritor, la fachada y dos guardas de PB-T3c (R-28). La base de la CI: Core 19 129 = 19 123 + 6 omitidas; UI 2 451 = 2 430 + 21. **Mediciones nuevas:** `measure_pbt4_v4.py` y `measure_pbt4_v4-out.txt` (R-28 a R-33). Las preguntas finales, listas para el diálogo, están en `owner-questions-pbt4.md`.

**Cómo se midió:** por objetos git en `7dfc7c21` (v1 a v3) y en `7f0d9aa3` (v4), el cierre de PB-T3c (F4B-E48). PB-T3a, PB-T3b y PB-T3c están CERRADAS.

**No se emite.** El orden es: v4 → comprobación acotada → preguntas al Owner con los números de este paquete → medición de la precondición 6 → emisión (§8.2).

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

## Registro de cambios v3 → v4

| Id | Hallazgo | Qué cambia | Dónde |
|---|---|---|---|
| **V4-01** | **B-07** (WCo no puede nombrar `RackCad.UI`) | **La regla de la escala pasa a Application,** sin cableado:<br>• la lógica pura de `NumericFieldValidation` (que solo usa `System.Globalization` y `LocalizedNumberParser` de `RackCad.Application.Formatting`) pasa a una clase de Application, `NumericTextRule`;<br>• `NumericFieldValidation` de la UI delega en ella, con la misma API pública y los mismos mensajes;<br>• `PushBackAnnotationScaleRule` (Application) da los parámetros de la escala; WHC la usa por defecto.<br>Hay una sola implementación y Core la tiene, así que no queda «el campo sin regla». **Medido:** ninguna guarda lee el texto de `NumericFieldValidation.cs` ni de `NumericField.cs`; las pruebas que la nombran la llaman por su API (R-30), y ninguna es de T1B por letra.<br>**El abridor, en dos piezas:**<br>• la clase anidada de WCo solo abre el modal, sin nombrar `RackCad.UI` ni `PushBack`;<br>• la ventana la abre `src/RackCad.Plugin/Views/RackWorkspaceEditorWindow.cs`, que no nombra `RackCad.Application.Workspace`.<br>**Rompe una sola guarda: CT-PBT1B-15,** el censo de archivos del Plugin con `PushBack`, que pasa de 14 a 15. Ninguna carpeta lo evita: todo archivo nuevo que abra la ventana nombra `PushBack` (R-31) | D-PBT4-06, 13 a 16; §4.3 |
| **V4-02** | **B-06** (las firmas y los números) | **Las firmas,** por método más los ayudantes de su mismo archivo, a un nivel y por nombre; ya no «por archivo». **C-8 desaparece** con la regla en Application. **C-9 es nueva:** las lecturas `FromPilot(` y `PrepareKit.Reread(` fuera del kit. **Medido por opción de Q4** (R-29):<br>• opción 1: 242 de 428;<br>• opción 2: 428;<br>• opción 3: no se mide por texto.<br>`<n4>`: 149 métodos por texto, 99 fuera de las 9 clases | D-PBT4-11; §8.2 |
| **V4-03** | **I-19** | EIG:664-690 y EIG:692-721 cuentan `pilot.Read` por cuerpo y no se pueden evitar sin quedarse con el borrador del piloto. Van a la lista de PB-T4b (C-5) | §4.2 |
| **V4-04** | **I-20** | CT-PBT4C-10 se parte en dos:<br>• Core: puerto → compuerta → WHC → doble del editor → «Actualizar»;<br>• **CT-PBT4C-14 (UI):** la FVM real sobre un doble del puerto que registra la llamada | §5.3 |
| **V4-05** | **I-21** (Q4) | Q4 neutral, con los números. **Las dos opciones cumplen las tres condiciones,** así que se recomienda «Saltar y duplicar», que no exige una excepción a C02 ni se aparta del «como D-1» | §8.2 Q4 |
| V4-06 | Q3 | La lista como **cota superior**: se emite la que vio el Owner o una parte; si el GREEN hace fallar otra prueba, vuelve al Owner | §8.2 Q3 |
| V4-07 | M-24 | El texto desfasado, corregido: D-PBT4-01 (una sustitución), D-PBT4-17 (la costura medida), D-PBT4-19 (sin `x:Static`), §7 («la v4»), la precondición 4 (Q1 a Q4) y la cabecera (`7f0d9aa3`) | Varios |
| V4-08 | M-25 | La condición 7 sustituye la línea base resuelta con `PushBackEditorState.SetWorkingBaseline(PushBackSystem)` (`PushBackEditorState.cs:74-79`), sobre una copia | D-PBT4-18 |
| V4-09 | M-26 | Q2: «del orden de un centenar de campos del editor» | §8.2 Q2 |
| V4-10 | M-27 | OWNER-1 a OWNER-3 de F4B-E48 (espacio papel o bloques del usuario, purga de un nivel, bloques dinámicos), en el Smoke-PB y en §7. La medición de la duración de la suite de Core (límite de 20 min, `ci.yml:42`) | §5.5; §7 |

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

**La base:** el cierre de PB-T3c, `7f0d9aa3`. **Tres subtareas secuenciales,** cada una con su 1/3:

| Subtarea | TaskId | Qué | Sustituciones (por texto, en `7f0d9aa3`) |
|---|---|---|---|
| **PB-T4a** | `F4B-PBT4-FULL-READ-PATH` (24) | El Plugin lee también el modelo completo sobre el mismo escaneo; el resultado lleva lo completo y el catálogo. Las relecturas de un borrador abierto van al origen del borrador por su texto (A-T3-8): es un cambio observable declarado | **1** (CT-PBT3A-12) |
| **PB-T4b** | `F4B-PBT4-FULL-DRAFT-PANEL` (25) | El borrador del panel pasa al completo; OB-03.2; la regla única de la escala, en Application; «Actualizar» por `ExecuteFull`; «Conservar mi borrador» con los pasos 1 a 3, OB-05.4 y `KeepOn` | **Opción 1 de Q4:** 242 + unas 12 de la letra de las guardas + hasta 21 de la UI. **Opción 2:** 428 + lo mismo (§8.2) |
| **PB-T4c** | `F4B-PBT4-EDIT-ALL-WINDOW` (24) | «Editar todo», con el abridor en dos piezas | **2** (PEG:156-163 y CT-PBT1B-15) |

**Cuatro preguntas al Owner** en una sola vuelta (§8.2 y `owner-questions-pbt4.md`):
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

**El orden:** PB-T4a sobre el cierre de PB-T3c (`7f0d9aa3`); PB-T4b sobre el de PB-T4a; PB-T4c sobre el de PB-T4b. Cada una con su 1/3 (OB-10.1), sin rebase.

**Por qué tres:**
- PB-T4a deja el camino de la lectura completa y la relectura con el origen, con **una sola** sustitución por letra;
- PB-T4b concentra el cambio de tipo y su lista;
- PB-T4c es casi aditiva, con dos sustituciones por letra.

**Los TaskId** empiezan por `F4B-PBT4-`, de 24 o 25 caracteres, para los sustitutos de nc1 y nc2 (§8.3).

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



#### D-PBT4-06. La regla única de la escala, en Application (B-02; B-07; Q-PBT2-3)

**Una sola implementación, movida y no copiada:**
- **La lógica pura** de `NumericFieldValidation.Validate` (`src/RackCad.UI/Controls/NumericFieldValidation.cs:61-`; solo usa `System.Globalization` y `RackCad.Application.Formatting.LocalizedNumberParser`) pasa a `src/RackCad.Application/Formatting/NumericTextRule.cs`, al lado del analizador que ya usa:
  - `public static NumericTextCheck Check(string text, bool isOptional, bool integerOnly, double? minimum, double? maximum, bool minimumInclusive, bool maximumInclusive)`;
  - los dos mensajes como constantes, «Escribe un valor.» y «Valor no numérico.», y los de rango, tal cual.
- **`NumericFieldValidation` de la UI** conserva su API pública (`Validate`, `EmptyRequiredMessage`, `NotANumberMessage`, `NumericFieldValidationResult` y `NumericFieldStatus`) y **delega** en `NumericTextRule.Check`. Sus constantes pasan a ser `= NumericTextRule.EmptyRequiredMessage` y `= NumericTextRule.NotANumberMessage`.
- **`PushBackAnnotationScaleRule`,** en `src/RackCad.Application/Systems/PushBack/Draft/`:
  - `Minimum = 0.0` y `MinimumInclusive = false`;
  - `public static DraftFieldValidation Validate(string text)`, que es `NumericTextRule.Check(text, false, false, Minimum, null, MinimumInclusive, true)` traducido a `DraftFieldValidation`.

**Sin cableado:** WHC crea el adaptador del campo con esa regla por defecto. No hay campo nuevo en WHC ni asignación en WCo, y WCo no nombra `RackCad.UI` (`WorkspaceHostSourceGuardTests.cs:398-420`). **Desaparece la causa C-8:** Core tiene la regla.

**Las guardas, medidas (R-30):**
- ninguna prueba lee el texto de `NumericFieldValidation.cs` ni de `NumericField.cs`;
- las 27 que nombran `NumericField` o `NumericFieldValidation` los llaman por su API pública, que no cambia;
- `NumericFieldValidationTests` (UI) prueba la regla directamente: sigue en verde porque el comportamiento es el mismo, y es la prueba de que la delegación no cambia nada;
- **ninguna guarda de T1B** fija el texto de esos archivos: `PushBackClassicCharacterizationSourceGuardTests` lee `src/RackCad.UI/Systems/PushBack` (:171), no `Controls/`.

**El XAML no cambia:** `WorkspaceDraftPilotValidationParityTests.cs:80-97` sigue en verde. CT-PBT4B-04 compara `PushBackAnnotationScaleRule` (`Minimum` y `MinimumInclusive`, y la validación en los diez textos de CT-PBT2B-10) con las propiedades de `AnnotationScaleBox` en una ventana real.

**El adaptador nuevo,** `Draft/PushBackFullFieldAdapter`, llama a `PushBackAnnotationScaleRule.Validate`.

**La copia del piloto,** según Q1.

#### D-PBT4-07. «Actualizar» con la escritura completa

- `TryUpdatePilot` conserva su nombre y su firma (G-10 y los llamadores de EIG).
- Su cuerpo pasa a `PushBackPilotUpdateRun.Precheck(draft)` y `.ExecuteFull(draft, request, port)`.
- **Desde el cierre de PB-T3c** (`7f0d9aa3`), el escritor borra vistas: `RackPilotMutationScope` implementa `IPushBackPilotViewEraser` con `RackViewBlockEraser`.
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



#### D-PBT4-11. La lista de sustituciones: las causas y el procedimiento (B-03, B-04, B-06, I-03, I-04, I-18, I-21)

**La firma de una causa se mide sobre el texto del método más el de los ayudantes de su mismo archivo que llama por su nombre, a un nivel.** Nada transitivo, y ya no «por archivo».

| Causa | Firma (en el método o en un ayudante de un nivel) |
|---|---|
| **C-0. La lectura viene del kit del piloto** | El archivo es una de las 9 clases de R-21 (las que construyen `EditIntentFakeReader` o usan `EditIntentKit`), y el método empieza un borrador |
| C-1. El tipo del borrador | `Find<PushBackPilotDraftState>`, `DraftSession<PushBackPilotDraftState>`, `PushBackPilotDraftState` o `PushBackPilotStateComparer` |
| C-2. El rechazo A/B | `OutOfPilotDiagnostic` o `CompositeOutOfPilotReason` |
| C-3. «Conservar» en dos tiempos | `TryKeepMine` o `KeepPilotEditsOn` |
| C-4. «Actualizar» por `ExecuteFull` | `PushBackPilotUpdateRun.Execute` o `Precheck`, `NonPilotContentReason`, `TryUpdatePilot` |
| C-5. La letra de una guarda | Las tablas de §4, medidas sobre el texto literal del GREEN (abajo) |
| C-6. El adaptador del piloto | `PushBackPilotFieldAdapter` |
| ~~C-8. El campo sin regla~~ | **Desaparece** (D-PBT4-06) |
| **C-9. Lecturas del piloto fuera del kit** | `FromPilot(` o `PrepareKit.Reread(` en el método o en un ayudante (por ejemplo, las nueve de `WorkspacePilotReconciliationHostTests` y EIH:770) |
| C-7. La retirada (Q1 «Retirar») | No compila, o nombra un miembro retirado |

**C-5, sobre el texto entregado:** el contrato fija las líneas del GREEN que leen las guardas, y la sesión las aplica a una copia exportada **solo para ejecutar las guardas de texto** (precondición 6).

**Los números por opción de Q4** (R-29; por texto, en `7f0d9aa3`; las 9 clases tienen 428 métodos):

| Clase | Métodos | Con alguna firma de C-1 a C-6 o C-9 (opción 1) |
|---|---|---|
| `WorkspaceLifecycleHostTests` | 69 | 17 |
| `WorkspacePilotPrepareTests` | 67 | 56 |
| `WorkspaceEditIntentHostTests` | 62 | 30 |
| `WorkspacePilotEditHostTests` | 59 | 43 |
| `WorkspaceCommandAdvisorTests` | 54 | 6 |
| `WorkspacePilotUpdateRequestTests` | 44 | 43 |
| `WorkspacePilotUpdateHostTests` | 35 | 23 |
| `WorkspacePilotReconciliationHostTests` | 28 | 23 |
| `WorkspaceAssociativeBookkeepingTests` | 10 | 1 |
| **Total** | **428** | **242** |

De esos 242, solo **88** se ven en el cuerpo del método; los demás, a través de sus ayudantes, como `Open(…)`, `Pilot(…)` o `Draft(…)`, tipados sobre el piloto. Con C-8, que la v4 elimina, serían 251.

**Las opciones:**

| Opción | La lista de PB-T4b | Lo que cumple | Lo que exige al contrato |
|---|---|---|---|
| **2. «Saltar y duplicar, como D-1» (Recomendada)** | Las 428 de C-0, copiadas con un kit nuevo (`PushBackFullReadKit`) y un mapa cerrado de sustituciones. Un guion compara cada original con su copia módulo el mapa. Las de C-2 a C-4 llevan además su equivalente a mano. Más unas 12 de C-5 y hasta 21 de la UI (C-3): **unas 461** | (a) lista cerrada: sí, por construcción. (b) sin implementación oculta: sí. (c) sin debilitar: sí, porque las copias conservan todas las aserciones. No exige excepción a C02 y sigue a D-1 | **Alcance:** los `Skip` de las 9 clases y las copias. **Paradas:** C15, C16 y la del guion de comparación |
| **1. «Editar una línea del kit»** | `EditIntentKit.ReadOf` (EIH:51-52) pasa a `FromReads`. Se sustituyen solo las **242** con firma, más unas 12 de C-5 y hasta 21 de la UI: **unas 275**. Las otras 186 siguen sin cambio, sobre el camino real | (a), (b) y (c): sí. **Rompe C02** (se edita un ayudante existente) y el «como D-1» de A-T3-1 | **Alcance:** EIH, solo la línea de `ReadOf`, con su excepción declarada a C02. **Paradas:** C15, C16 y PBT4B-C19 (otra edición de EIH) |
| **3. «Implementación de referencia»** | No se mide por texto. A lo sumo, no más que la opción 1 | (a) y (c): sí. (b): solo si el Worker la recibe, y entonces cambian los papeles | 3a: C16 para cualquier diferencia del Worker. 3b: el cambio de papeles |

**La recomendación:** las opciones 1 y 2 cumplen las mismas tres condiciones, así que se recomienda la 2. No exige ninguna excepción a la regla de no editar ayudantes ni se aparta de D-1, a cambio de unas 186 copias más, todas mecánicas y comprobadas por guion.

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



#### D-PBT4-13. El anfitrión, los dos reenvíos y el puerto generalizado (A-T3-11, A2-1; B-05)

Como en la v3: `TryEditAll`, con cuerpo por defecto, con las reglas de `TryKeepPilotEdit`, y sus resultados.
- **Los dos reenvíos explícitos:** `WorkspacePilotEditGate.TryEditAll` y `PilotEditAdapter.TryEditAll`. Lo prueban CT-PBT4C-12 y 13.
- **El lanzamiento y el adaptador del puerto,** en `Draft/`.

#### D-PBT4-14. Los estados de cada operación durante el editor (I-01; I-15)

Como en la v3. `RequestUpdate` da `Busy` por la marca (WHC:1388).

#### D-PBT4-15. El modal y el abridor, en dos piezas (I-01; B-07; M-14)

**Por qué dos piezas.** Una sola no cabe en ningún sitio sin romper una guarda medida:
- **en WCo** (anidada) tiene que nombrar la ventana, de `RackCad.UI.Systems.PushBack`, y `WorkspaceHostSourceGuardTests.cs:398-420` prohíbe `RackCad.UI` en todo archivo de `Plugin/Workspace/` salvo la paleta y el puente;
- **en un archivo nuevo de `Plugin/Workspace/`** rompe las dos guardas de 9 archivos (EIG:1011-1022; `WorkspaceNavigationHostTests:714-723`);
- **fuera de `Plugin/Workspace/`,** si implementa una interfaz de `RackCad.Application.Workspace`, rompe `WorkspaceBridgeSourceGuardTests.cs:364-367`.

**Las dos piezas:**
1. **El modal, anidado en WCo:** `private sealed class FullEditorModal : IWorkspaceFullEditorHost`. Su único método es `public void ShowModal<TLaunch>(TLaunch launch) where TLaunch : class { using (_controller.EnterPanelModal()) _open(launch); }`, y `_open` es un `Action<object>`.
   - **No nombra `RackCad.UI`** (398-420 sigue en verde) **ni `PushBack`** (WCo no está en el censo de CT-PBT1B-15, R-31).
   - Se asigna en WCo con `_host.SetFullEditorHost(new FullEditorModal(this, RackWorkspaceEditorWindow.Show))`, después de `SetPilotUpdateHost`, así que EIG:1150-1190 sigue en verde.
2. **La ventana, en `src/RackCad.Plugin/Views/RackWorkspaceEditorWindow.cs`:** `internal static class RackWorkspaceEditorWindow`, con un solo método, `internal static void Show(object launch)`:
   - `var window = RackPushBackSystemWindow.ForDraft((PushBackDraftEditorLaunch)launch);`
   - `AcApplication.ShowModalWindow(window);`, sin `Owner`, como el abridor clásico.
   - **No nombra `RackCad.Application.Workspace`** (364-367 sigue en verde) ni ningún término de G-9 (no nombra el piloto).

**`IWorkspaceFullEditorHost`,** en `Application/Workspace/`, con el método genérico `void ShowModal<TLaunch>(TLaunch launch) where TLaunch : class;`. WHC lo llama con el `PushBackDraftEditorLaunch`. El campo `_fullEditorHost` es de un tipo de `Application.Workspace`, así que EIH:1088 sigue en verde. Se declara como excepción a R-PB-05 y a ADR-0029 D12.

**La guarda que rompe: CT-PBT1B-15** (`PushBackSharedReferenceGuardTests:306-` y :381: «archivos=14»). `RackWorkspaceEditorWindow.cs` nombra `PushBack` (la ventana y el lanzamiento), así que el censo pasa a 15. **Ninguna carpeta lo evita:** todo archivo nuevo que abra la ventana nombra `PushBack`, y ponerlo en uno de los 14 existentes mezclaría el abridor con el camino clásico o con el escritor (R-31). Se sustituye con registro en PB-T4c (C-5), con un equivalente: «archivos=15 (los 14 y el abridor); asignaciones=7, sin cambio»; las demás líneas, iguales.

#### D-PBT4-16. La guarda de OB-01.3 (M-05, M-09)

CT-PBT4C-07 mira **seis sujetos**:
- `RackWorkspaceEditorWindow.cs`;
- el cuerpo de `FullEditorModal` en WCo;
- `TryEditAll` y sus ayudantes en WHC;
- el gesto `EditAll` en FVM;
- `Draft/PushBackFullDraftLaunch.cs`;
- `Draft/PushBackFullDraftStatePort.cs`.

Ninguno nombra `RackPushBackCommands`, `RackMenuCommands`, `EditorModules`, `RackViewBatchExecution`, `DrawAndPlace`, `RedrawInPlace`, `InsertRequested` ni `InsertionRequest`. La autoprueba en negativo inyecta cada nombre en cada sujeto.

**`RackWorkspaceEditorWindow.cs` no lleva** `LockDocument`, transacción ni nada tras cerrar.


#### D-PBT4-17. «Cerrar» con las dos cosas (A-T2b-OWNER-01; M-12)

`PendingWork()` de W, **solo en WORKSPACE**:
- un módulo escenificado y cambios sin aplicar: una sola pregunta, `WorkspaceStagedAndUnappliedQuestion`, con el texto de la v2;
- uno solo: la de hoy;
- en CLASSIC, nada cambia.

**La prueba** (CT-PBT4C-09) lee `PendingWork()` sin cerrar la ventana. **Escenifica el módulo con la costura existente,** `HeaderConfiguratorPresenter` y `ConfigureModuleHeaderButton` (`PushBackHeaderAuthorityWindowTests.cs:99-106`), sin el configurador real.

### Común



#### D-PBT4-18. D2 (A2-5, A-T3-4; I-06, I-16, M-25)

**La letra del Owner** (`owner-d1-d2-decisions.md:113-131`), con la 7: «comparing InitialDraftState vs CurrentDraftState does not depend on mutable changes inside those shared objects».

**CT-PBT4B-10,** la huella tras cada operación:
- condición 3: `Begin`, `SetCurrentDraftState` y el rebase;
- condición 5: `SetPendingText` y `CommitField`;
- condición 6: `DiscardAndReload` y `KeepMineOnCurrentVersion` con `KeepOn`;
- **condición 7:**
  - sobre una copia del borrador, se sustituye la línea base resuelta del lado A por la de otro fixture, de contenido distinto, con `PushBackEditorState.SetWorkingBaseline(PushBackSystem)` (`PushBackEditorState.cs:74-79`);
  - el comparador da `True`, porque no compara la línea base ni el informe (`PushBackEditableStateComparer.cs:20-27`), e `IsDirty` da `False`;
  - la línea es `linea-base-de-otro-contenido isdirty=False`;
  - el informe de reconciliación se sustituye por el miembro con el que CT-PBT1B-16 lo lee para su huella, con la línea `informe-de-otro-contenido isdirty=False`.

**CT-PBT4C-11,** sobre `ApplyToDraft`.

**El límite literal,** en C17. **M11** no aplica.


#### D-PBT4-19. Lo que PB-T4 no hace

- No toca la ventana clásica, RACKEDITAR, RPB ni el menú. **W solo cambia la pregunta de D-PBT4-17,** en WORKSPACE; el XAML no cambia.
- No crea vistas.
- No traduce estados.
- No usa el asiento en `Recompute` (A-T3-9).
- No abre AUTH-13 ni cambia `DormantTailOutOfPilotReason` ni el camino clásico.
- No añade botones ni campos fuera de «Editar todo», el texto de la presentación y la pregunta de «Cerrar».
- No cachea las pasadas.
- **No cambia el comportamiento de ninguna caja numérica:** `NumericFieldValidation` delega en la misma lógica (D-PBT4-06).

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
| OWNER-01 de «Restaurar»; PBT3B-IR-M01 y OWNER-2; el configurador | Fuera o regla, como en la v1. OWNER-2 de PB-T3b se anota en `ideas-futuras.md` con el cierre de PB-T3c (F4B-E48); los riesgos del borrado real (OWNER-1 a OWNER-3 de F4B-E48), al Smoke-PB (§5.5) | |

---



## 4. Las guardas, por subtarea

### 4.1 PB-T4a

Como en la v3: **una sustitución, CT-PBT3A-12.** Lo demás sigue en verde (EIG:563-575, EIG:733-736, EIG:758-770, `WorkspaceLifecycleGuardTests:642-656`, EIG:414-430, EIG:441-470, G-9, `WorkspaceCommandAdvisorGuardTests:406-460` y `WorkspaceBridgeSourceGuardTests:364-367`; §14.1 de la revisión).

### 4.2 PB-T4b

Las roturas por letra (C-5), medidas por objetos git. El resto sale del texto literal del GREEN (precondición 6):

| Guarda o prueba | Causa | Resultado |
|---|---|---|
| CT-PBT4A-06 (el censo de PB-T4a) | C-5 | Se sustituye por CT-PBT4B-13 (admite WHC) |
| CT-PBT3B-13 | C-5 | Se sustituye |
| G-4 (IG:497-498) | C-5 | Se sustituye |
| EIG:692-721 (con :711, `KeepPilotEditsOn`) y los métodos de :776 y :805 | C-5 y C-3 | Se sustituyen |
| **EIG:664-690** (`pilot.Read` por cuerpo, :677-688) | **C-5 (I-19)** | **Se sustituye:** `full.Read` en `Begin`, y el diagnóstico de la admisión con `full` |
| EIG:725-737 (`var pilot = read.PilotRead;`, seis) | C-5 | Se sustituye |
| PEG:380-386 (`HostViolations`, el adaptador estático, :280) | C-6 | Se sustituye con todas sus reglas |
| RG:109-123 (`Mutate` en `RequestKeepMine`) | C-5 | Se sustituye |
| `WorkspacePilotPrepareTests:1350-1355` (11 estados) | C-5 | Es una de las 9 clases: entra por la opción de Q4 |
| UI: `WorkspacePilotReconciliationTests:780-793` y las de `RequestKeepMine` | C-3 | Hasta 21, con registro declarado |
| `WorkspaceDraftPilotValidationParityTests:80-97` | — | **Verde:** el XAML no cambia |
| `NumericFieldValidationTests` y las 26 clases de UI que usan `NumericField` | — | **Verdes:** misma API y mismo comportamiento |
| EIG:670 y :720 (la base en WHC) | — | **Verdes por diseño** (D-PBT4-08) |

**Unas 12 de la letra:** las 9 filas sustituibles de arriba sin las dos de las 9 clases, más las autopruebas de esos métodos. El número exacto sale de la precondición 6.

### 4.3 PB-T4c

| Guarda | Causa | Resultado |
|---|---|---|
| PEG:156-163 (17 nombres) | C-5 | Se sustituye: 18 |
| **CT-PBT1B-15** (`PushBackSharedReferenceGuardTests`, «archivos=14») | **C-5 (B-07)** | **Se sustituye:** «archivos=15» con `RackWorkspaceEditorWindow.cs`; lo demás, igual |
| `WorkspaceHostSourceGuardTests:398-420` | — | **Verde:** WCo no nombra `RackCad.UI` |
| EIG:1011-1022 y `WorkspaceNavigationHostTests:714-723` | — | **Verdes:** ningún archivo nuevo en `Plugin/Workspace/` |
| `WorkspaceBridgeSourceGuardTests:364-367` | — | **Verde:** `RackWorkspaceEditorWindow.cs` no nombra `RackCad.Application.Workspace` |
| `WorkspaceBridgeSourceGuardTests:139-142` | — | Verde: `FullEditorModal` no nombra los prohibidos |
| G-9 | — | Verde: el abridor no nombra el piloto |
| EIG:1150-1190, CT-PBT2B-08, URT:1107-1109, EIH:1066 y PEG:380-385 | — | Verdes |

**Si cualquier otra prueba falla,** fuera de las tablas y de la lista, es C02 o C16 y vuelve al Owner (OB-T1B.3).

## 5. La red de pruebas

### 5.0 Reglas comunes

Las de la v1 (§5.0), con estos cambios:
- **Un `[Fact]` por CT, sin `[Theory]` nueva** (F4B-E46).
- **Los kits nuevos van en archivos nuevos:** `PushBackFullReadKit` (las lecturas `FromReads` sobre `IntentKit`) y los dobles del lector con origen, del puerto del panel y del anfitrión del editor. `EditIntentKit`, `WorkspacePilotEditPortDouble` y los demás no se editan.
- **Ninguna prueba de UI** crea un configurador, cierra una ventana con trabajo pendiente ni toca el perfil.
- **Las líneas se toman en el momento justo:** la huella, tras cada operación.



### 5.1 PB-T4a: la lista cerrada CT-PBT4A-01 a 07 (Core)

Como en la v3.

**Los totales,** sobre la base de `7f0d9aa3` (Core 19 129 = 19 123 + 6), con un `Skip`:

| Rama | Core | UI |
|---|---|---|
| **RED** | 19 136 = 19 124 `Passed` + 5 `Failed` + 7 `Skipped` | 2 451, sin cambio |
| **GREEN** | 19 136 = 19 129 `Passed` + 7 `Skipped` | 2 451 |

### 5.2 PB-T4b: la lista cerrada CT-PBT4B-01 a 17

Como en la v3, con estos cambios:
- **CT-03:** el campo valida con `PushBackAnnotationScaleRule` sin asignar nada; la fila «sin regla» desaparece.
- **CT-04 (UI):** `PushBackAnnotationScaleRule` frente a `AnnotationScaleBox` de una ventana real (parámetros y validación en los diez textos).
- **CT-16:** pasa a fijar que WHC crea el adaptador con `PushBackAnnotationScaleRule.Validate` y que WCo no asigna ninguna regla.
- **CT-18, nueva:** `NumericFieldValidation.Validate` y `NumericTextRule.Check` dan el mismo resultado (estado, valor y mensaje) en una tabla fija de textos y parámetros (vacío, opcional, entero, límites inclusivos y exclusivos, no numérico). Es UI, con `ExpectRed` true: en el RED, la regla de Application es inerte.

K, D y E dependen de Q4 (D-PBT4-11).

### 5.3 PB-T4c: la lista cerrada CT-PBT4C-01 a 14

Como en la v3, con estos cambios:
- **CT-07:** los seis sujetos de D-PBT4-16.
- **CT-08:** las dos piezas del abridor, en texto:
  - `FullEditorModal` en WCo, con un `using (_controller.EnterPanelModal())` y la llamada a `_open`, sin `RackCad.UI` ni `PushBack`;
  - `RackWorkspaceEditorWindow.Show`, con un `ForDraft(`, un `ShowModalWindow(`, sin `Owner =` ni `RackCad.Application.Workspace`;
  - y el cableado en WCo.
- **CT-10 (Core):** puerto real (`WorkspacePilotEditGate` sobre WHC) → `TryEditAll` → el doble de `IWorkspaceFullEditorHost` aplica por el puerto (incluido un gesto compuesto, F4B-E30) → «Actualizar» con `PilotEraseScopeDouble` → `Applied` y Clean.
- **CT-14, nueva (UI; I-20):** la FVM real, sobre un doble nuevo del puerto que registra: `EditAll` llama a `CommitField` si hay texto y después a `TryEditAll`, con la petición de D-PBT4-12.

**Los totales:** K_C = 2 (PEG:156-163 y CT-PBT1B-15, dos `[Fact]`).

| Rama | Core | UI |
|---|---|---|
| **RED** | (C + 10 + 2 − D_C) = (C_P − 2 + 3) `Passed` + 9 `Failed` + (C_S + 2) `Skipped` | + 4, con 4 `Failed` |
| **GREEN** | El mismo total, con 0 `Failed` | + 4, con 0 `Failed` |

- En el RED pasan CT-07 y los dos equivalentes.
- D_C = 0. Se confirma en la precondición 6.

### 5.4 La CI del RED

`tests` en failure, `build-ui` en success y `build-plugin` omitido. `ui-tests`: en failure en PB-T4b y PB-T4c; en success en PB-T4a, que no añade pruebas de UI.

### 5.5 Lo que queda para el Smoke-PB (PB-T5, D-PB-09; I-10; M-27)

La lista de la v3, más:
- **los riesgos del borrado real de PB-T3c** (F4B-E48, `f4b-dispositions.md:1609`):
  - OWNER-1: las referencias de la vista en espacio papel o dentro de bloques del usuario se borran sin aviso;
  - OWNER-2: la purga de las anidadas es de un nivel;
  - OWNER-3: los bloques dinámicos.

  Se observan con «Editar todo» → «Actualizar» sobre racks reales, y junto a la línea «Vistas: se borrarán…» de la presentación;
- **las dos piezas del abridor cableadas:** la ventana se abre desde la paleta, sin dueño, con el modal del panel.


## 6. Los contratos propuestos

### 6.0 Común

Lo de la v3, con estos cambios:

**`UNIT_CHANGE`:**
- **PB-T4b:** añade `src/RackCad.UI/Controls/NumericFieldValidation.cs` y, con la opción 1 de Q4, EIH;
- **PB-T4c:** añade WCo, que ya estaba, y nada de `Plugin/Workspace/` más.

**La lista cerrada de declaraciones nuevas, por nombre** (C13):
- **PB-T4b:** las de la v3, sin `AnnotationScaleRule` de la UI ni `SetFieldRule` ni el campo `_fieldRule`. En su lugar:
  - `NumericTextRule.Check` y `NumericTextCheck`, con sus constantes;
  - `PushBackAnnotationScaleRule` (`Minimum`, `MinimumInclusive` y `Validate`);
  - y la delegación en el cuerpo de `NumericFieldValidation.Validate`.
- **PB-T4c:** las de la v3, sin `FullEditorHost` ni `IPushBackFullEditorHost`. En su lugar:
  - `IWorkspaceFullEditorHost.ShowModal<TLaunch>`;
  - la clase anidada `FullEditorModal` de WCo (el constructor y `ShowModal`);
  - `RackWorkspaceEditorWindow.Show`;
  - `SetFullEditorHost`.

### 6.1 PB-T4a — `F4B-PBT4-FULL-READ-PATH`

- **Objective** (ASCII):
  > Hacer que la lectura de la intencion de edicion del Workspace de Push Back traiga tambien el modelo completo de PB-T3 sobre el mismo escaneo: el adaptador del Plugin llama al lector completo tras el del piloto y el resultado de la lectura lleva la lectura completa y el catalogo junto a la del piloto (FromReads; FromPilot no cambia); y que las cinco relecturas de un borrador abierto vayan al origen del borrador por su texto (el unico miembro mutable con ese texto, o el Primary de hoy si no hay uno) cuando el lector lo ofrece, con la eleccion en linea en cada cuerpo (A-T3-8). Es un cambio observable declarado: las relecturas dejan de ir al Primary. El tipo del borrador del panel no cambia, nada traduce estados y no se lee el dibujo mas de una vez por lectura, segun el paquete del contrato de PB-T4 v4 y la lista cerrada CT-PBT4A-01..07. Solo se sustituye con registro CT-PBT3A-12. Si hiciera falta mutar el rack resuelto o el informe de modulos dentro de la vida del borrador: STOP y se reabre L-1. No se ejecuta AutoCAD.

- **El resto,** como en la v3.
- **La línea base:** el cierre de PB-T3c (`7f0d9aa3`; la CI de `938a35f0`).

### 6.2 PB-T4b — `F4B-PBT4-FULL-DRAFT-PANEL`

- **Objective** (ASCII):
  > Pasar el borrador del panel del Workspace de Push Back al modelo completo de PB-T3 con la lectura completa de PB-T4a, sin traducir estados: OB-03.2 (los racks A/B se editan en el panel; la cola y AUTH-13 siguen rechazados), la regla unica de la escala en Application (la logica pura de la validacion numerica pasa de la UI a Application y la UI delega en ella, sin cambiar su comportamiento; el anfitrion la usa por defecto), Actualizar con PrepareFull y ExecuteFull, y Conservar mi borrador en dos tiempos por TryKeepMine (presentacion clasificada por una funcion pura de Draft con un testigo de lo releido, y KeepOn solo tras confirmar con el testigo vigente), segun el paquete del contrato de PB-T4 v4, las decisiones del Owner sobre el piloto, la presentacion y el procedimiento de la lista, la lista cerrada CT-PBT4B-01..18 y la lista de sustituciones que vio el Owner, como cota superior. Si hiciera falta mutar el rack resuelto o el informe de modulos dentro de la vida del borrador: STOP y se reabre L-1. Ni Editar todo, ni RACKEDITAR, ni la ventana; no se ejecuta AutoCAD.

- **`AllowedWriteScope`:** el de la v3, más `src/RackCad.UI/Controls/NumericFieldValidation.cs`, `src/RackCad.Application/Formatting/NumericTextRule.cs` (nuevo) y `Draft/PushBackAnnotationScaleRule.cs` (nuevo). Sin `AnnotationScaleRule.cs` de la UI. **WCo, no.**
- **`RequiredTests`:**
  - Core `PushBackFullPanel`: 13 + E^R, `ExpectRed` true;
  - Core las guardas: 2 + E^G, false;
  - UI `PushBackFullPanelFieldTests`: 3, true (CT-04, 14 y 18);
  - `NumericFieldValidationTests` y `NumericFieldTests`, en verde, false;
  - y el resto.
- **`StopConditions`:**
  - C15, C16, C13 y C17;
  - **PBT4B-C20:** una caja numérica de cualquier ventana cambia de estado, valor o mensaje (lo vería `NumericFieldValidationTests` o CT-18).

### 6.3 PB-T4c — `F4B-PBT4-EDIT-ALL-WINDOW`

- **Objective** (ASCII):
  > Anadir Editar todo al panel del Workspace de Push Back sobre el borrador completo de PB-T4b: un boton y su gesto, que confirma el texto pendiente con CommitField antes de abrir (R-PB-10); TryEditAll en el anfitrion y en sus dos reenvios (la compuerta de avisos y el adaptador del panel del Plugin), que empieza el borrador si hace falta, mantiene la marca de lectura mientras la ventana esta abierta (K = 1) y abre la ventana de Push Back en modo WORKSPACE en dos piezas: el modal del panel en una clase anidada del controlador del Plugin y la ventana, sin dueno, en una clase nueva de Plugin/Views; el adaptador del puerto que conserva Origin y acaba la marca known clean; la guarda de texto de OB-01.3 sobre los seis sujetos del camino; Cerrar con una sola pregunta si hay a la vez un modulo escenificado y cambios sin aplicar; y el resumen y Modificado al cerrar, segun el paquete del contrato de PB-T4 v4 y la lista cerrada CT-PBT4C-01..14. Solo se sustituyen con registro la guarda de las operaciones del puerto (PEG:156-163) y el censo de archivos del Plugin con PushBack (CT-PBT1B-15). Ni el camino clasico, ni RACKEDITAR, ni la ventana en CLASSIC; cero escrituras en el DWG desde la ventana; no se ejecuta AutoCAD.

- **`AllowedWriteScope`:**
  - WHC, `IWorkspacePilotEditPort.cs`, `WorkspacePilotSupport.cs`, WEI, FVM, PV, WCo y W;
  - los nuevos `IWorkspaceFullEditorHost.cs`, `Draft/PushBackFullDraftLaunch.cs`, `Draft/PushBackFullDraftStatePort.cs` y `src/RackCad.Plugin/Views/RackWorkspaceEditorWindow.cs`;
  - las clases nuevas;
  - **solo para los `Skip`:** PEG y `PushBackSharedReferenceGuardTests.cs`.
- **`RequiredTests`:**
  - Core `PushBackEditAll`: 9, true;
  - Core `PushBackOpenerClassicPathGuardTests` y las dos guardas equivalentes: 3, false;
  - UI `PushBackEditAllWindowTests`: 4, true;
  - y el resto.


## 7. Riesgos y lecciones aplicadas

Los de la v1 (R-T4-01 a 15), con lo que hace la v4:

| Riesgo | Qué hace la v4 |
|---|---|
| R-T4-01, la lista | Firmas por método y ayudantes, a un nivel. Números por opción. Q4 neutral. Q3 como cota superior |
| R-T4-08, los miembros por defecto | Solo `TryEditAll`, con sus dos reenvíos explícitos (CT-12 y CT-13) |
| R-T4-13, el modal y el abridor | Dos piezas, medidas contra las guardas de cada carpeta; una sola rotura, CT-PBT1B-15 |
| R-T4-16, la doble lectura del piloto | Sin cambio: se mide en la precondición 6 |
| R-T4-17, el coste de `IsDirty` | Sin cambio |
| **R-T4-19. La duración de la suite de Core** (M-27) | Cientos de pruebas pasan al comparador completo (A-10: 27,7 s en CT-PBT1B-06), y el trabajo `tests` tiene 20 min (`ci.yml:42`). Se mide en la precondición 6 y, si la suite pasa de 15 min, vuelve a la sesión antes de emitir PB-T4b |
| **R-T4-20. Mover la validación numérica a Application** | El comportamiento de todas las cajas numéricas depende de ella. `NumericFieldValidationTests` (UI) y CT-PBT4B-18 la fijan, y PBT4B-C20 para |
| **R-T4-21. Los riesgos del borrado real** (OWNER-1 a OWNER-3 de F4B-E48) | Al Smoke-PB (§5.5). La presentación los anuncia en la línea de las vistas |

## 8. Las decisiones del Owner

### 8.1 Ya tomadas

Las de la v1 (§8.1).



### 8.2 El orden y las preguntas

**El orden:**
1. la v4;
2. su comprobación acotada;
3. **PB-T3c ya está cerrada** (`7f0d9aa3`);
4. las cuatro preguntas, en una sola vuelta, con los números de este paquete (`owner-questions-pbt4.md`);
5. la medición de la precondición 6, que da K, D y E, la letra de C-5 sobre el texto del GREEN, las líneas de CT-PBT4B-06, el coste de `IsDirty` y la duración de la suite;
6. la emisión de PB-T4a.

**La lista que ve el Owner es una cota superior** (§14.3 de la revisión):
- con la opción 2 de Q4 es exacta por construcción;
- con la 1, las de la lista que pasen en el GREEN no se saltan.

**Lo declarado, sin preguntar:**
- que pedir «Conservar» lee el dibujo, frente a la regla de F4 de la UI :780-793, con su registro;
- el límite del testigo (M-20);
- el cambio observable de PB-T4a (M-23);
- **que la validación numérica pasa de la UI a Application, sin cambiar su comportamiento** (D-PBT4-06).

**El texto final de las cuatro preguntas, con sus números, está en `owner-questions-pbt4.md`.** Lo esencial:
- **Q1, «Retirar» (Recomendado):** añade unas 99 pruebas por texto fuera de las 9 clases (35 guardas de texto y 64 de comportamiento: 24 de la materialización, 15 del estado, 9 de la base, 6 de la corrida, 3 de PB-T3b y 7 de la UI) y las tres de D-1 (E-3, E-5 y E-6). «Conservar sin uso» las deja en verde. «Dos modelos» no se puede medir sin diseñarlo.
- **Q2, «Por grupos» (Recomendado):** del orden de un centenar de campos del editor (el diseño tiene de 136 a 165 rutas de hoja contando todo el proyecto).
- **Q3, «Sí, con esas condiciones» (Recomendado):** las listas son una cota superior. PB-T4a: 1. PB-T4b: unas 461 con la opción 2 o unas 275 con la 1. PB-T4c: 2.
- **Q4, «Saltar y duplicar, como D-1» (Recomendado):** las dos opciones cumplen lo mismo, y esta no exige excepción a C02.

### 8.3 Lo que no es decisión

Lo de la v3 (§8.3). Además:
- **el abridor en dos piezas;**
- **la validación numérica movida a Application,** que no cambia el comportamiento. Si la comprobación acotada o la precondición 6 muestran una guarda de T1B que lo impide, vuelve al Owner por OB-T1B.3.

---

## 9. Límites

- Nada se compiló ni se ejecutó.
- El worktree no se leyó.
- **Los números de Q1, Q3 y Q4 son por texto** (el método y los ayudantes de su archivo, a un nivel): pueden contar de más o de menos.
- **Quedan para la copia exportada:**
  - K, D y E;
  - la letra de C-5 sobre el texto del GREEN;
  - las líneas de CT-PBT4B-06;
  - el coste de `IsDirty`;
  - la duración de la suite de Core.
- **El miembro del informe de reconciliación** para la condición 7 se nombra como el que lee CT-PBT1B-16. La línea base va por `SetWorkingBaseline`.

## 10. Lo que queda para PB-T5

- El Smoke-PB, con la lista de §5.5.
- La CI exacta del Candidato.
- El gate-close de F4B.

## 11. Las precondiciones de emisión

1. **PB-T3c CERRADA:** cumplida (`7f0d9aa3`, F4B-E48).
2. **La comprobación acotada de la v4** y la de cada subtarea.
3. **La medición de la precondición 6.**
4. **Las respuestas del Owner a Q1, Q2, Q3 y Q4,** dentro de la `AuthorityRevision`.
5. **Las celdas, la Snapshot y el binario,** vigentes.
6. **La comprobación sobre una copia exportada:**
   - el esqueleto exacto de cada subtarea compila;
   - la suite completa de Core y los filtros de UI dan lo previsto;
   - las roturas de C-5 sobre el texto literal del GREEN;
   - K, D y E de cada rama;
   - las líneas de CT-PBT4B-06;
   - el coste de `IsDirty`;
   - la duración de la suite.
7. **La custodia,** en el commit de emisión de PB-T4a:
   - el paquete, el registro de mediciones, las revisiones, los guiones y sus salidas;
   - las preguntas y respuestas del Owner;
   - la lista medida;
   - los registros declarados.
8. **Para PB-T4b y PB-T4c:** la anterior CERRADA y su remedición.
