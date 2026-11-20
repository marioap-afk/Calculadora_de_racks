> **Redacción para el repo público:** las rutas locales se sustituyen por marcadores (`<checkout-principal>`, `<carpeta-del-paquete>`). El resto no cambia.

# I-64 F4B — Paquete del contrato de PB-T4 («Editar todo» en el panel del Workspace), v2

**Estado:** v2. Aplica la revisión independiente de la v1 (`independent-review-v1.md`: **NECESITA v2, B/I/M = 3 / 11 / 15**) con las decisiones de la sesión (`v2-instructions.md`). El registro de cambios sigue.

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
| **PB-T4a** | `F4B-PBT4-FULL-READ-PATH` (24) | **Solo aditiva.**<br>• El Plugin lee también el modelo completo sobre el mismo escaneo, y el resultado de la lectura lleva lo completo y el catálogo junto al piloto.<br>• La relectura de un borrador abierto admite el origen (A-T3-8).<br>• El tipo del borrador del panel no cambia | Solo las que rompe por letra, medidas: 2 (§4.1) |
| **PB-T4b** | `F4B-PBT4-FULL-DRAFT-PANEL` (25) | El borrador del panel pasa al completo; OB-03.2; la regla única de la escala; «Actualizar» por `ExecuteFull`; «Conservar mi borrador» con los pasos 1 a 3, OB-05.4 y `KeepOn` | La lista de Q1, medida (D-PBT4-11) |
| **PB-T4c** | `F4B-PBT4-EDIT-ALL-WINDOW` (24) | «Editar todo»: el gesto con `CommitField` como frontera; el anfitrión; el puerto que conserva `Origin`; el abridor y la guarda de OB-01.3; «Cerrar» con una sola pregunta; el resumen y «Modificado» | PEG:156-163 por letra (§4.3) |

**Las tres preguntas al Owner** (§8.2) se hacen tras medir la lista sobre el cierre de PB-T3c:
- Q1: el piloto, con la lista;
- Q2: la presentación;
- Q3: la emisión.

---

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

#### D-PBT4-03. La relectura con el origen (A-T3-8; M-04)

**El problema:** las relecturas de un borrador abierto pasan `request.ChosenReference`, que es la referencia que el usuario pulsa ahora (WHC:409, :503, :590, :675 y :759). `Begin` (:328) sí debe usarla.

**La solución, aditiva:**
- **Una interfaz nueva en Application/Workspace,** `IWorkspaceEditIntentOriginReader`, con `EditIntentReadResult ReadIntentAtOrigin(SessionId session, string rackId, string originKey, EditIntentWitness? witness)`. La implementa el lector del Plugin (NH), por el mismo método privado de lectura: `ReadOnActiveDocument` gana el argumento `string? originKey`; si lo hay, el origen es ese manejador y no sale de `ChooseOriginDefinition`.
- **El anfitrión guarda la clave del origen** de cada borrador en un diccionario privado por (sesión, rack):
  - la de la lectura de `Begin` (`PilotRead.OriginKey`);
  - tras un POST `Applied`, la del origen de POST;
  - se borra con el borrador.
- **Las cinco relecturas pasan por un ayudante privado,** `ReadForOpenDraft(request, originKey)`:
  - si el lector implementa `IWorkspaceEditIntentOriginReader` y hay clave: `ReadIntentAtOrigin`;
  - si no: `ReadIntent` con `ChosenReference`, como hoy. Así los dobles existentes del lector siguen igual.
- **`Precheck` no lee** (UR:130-158). **POST** ya relee por `Origin` (UR:214-219).

**Lo que rompe por letra:** EIG:736 (seis `.ReadIntent(`): quedan dos, el de `Begin` y el del ayudante. Va a §4.1.

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

#### D-PBT4-06. La regla única de la escala (B-02; Q-PBT2-3)

**Dónde vive:** en la UI, en `src/RackCad.UI/Controls/AnnotationScaleRule.cs`. La referencian la UI, sus pruebas y el Plugin. Contiene:
- `public const double Minimum = 0.0;`
- `public const bool MinimumInclusive = false;`
- `public static DraftFieldValidation Validate(string text)`, que es `NumericFieldValidation.Validate(text, false, false, Minimum, null, MinimumInclusive, true)` traducido a `DraftFieldValidation`.

**Una sola implementación:** `NumericFieldValidation`, la de la ventana.

**Una sola declaración de los parámetros:** W.xaml:68-69 pasa a `Minimum="{x:Static controls:AnnotationScaleRule.Minimum}" MinimumInclusive="{x:Static controls:AnnotationScaleRule.MinimumInclusive}"`. El comportamiento en CLASSIC no cambia, porque los valores son los mismos. `<a medir en la comprobación del §6.0>`: si una guarda de T1B fija el texto de esos atributos, se queda el XAML y CT-PBT4B-04 compara los parámetros con las propiedades de la caja real.

**Cómo llega al anfitrión:**
- el Plugin la conecta en WCo, junto a `SetPilotUpdateHost`: `_host.SetFieldRule(AnnotationScaleRule.Validate)`;
- el campo del anfitrión es `Func<string, DraftFieldValidation>?`, y `DraftFieldValidation` es de `Workspace.Drafts`, así que no es un campo de `Systems` (EIH:1088 sigue);
- sin regla asignada, el campo responde «No editable aquí».

**El adaptador nuevo,** `PushBackFullFieldAdapter : IDraftPendingTextAdapter<PushBackFullDraftState>`, en `Draft/`:
- usa la clave de hoy (`"AnnotationScale"`, el valor de `PushBackPilotFieldAdapter.AnnotationScaleKey`, copiado como constante propia);
- `Validate` llama a la regla que recibe en su constructor, `PushBackFullFieldAdapter(Func<string, DraftFieldValidation> rule)`, y con otra clave da el mensaje de hoy;
- `Apply` devuelve `new PushBackFullDraftState(copia del editable con la escala nueva, state.Origin)` y no toca nada más.

**La copia del piloto,** `PushBackPilotFieldAdapter.Validate`:
- con Q1 «Retirar», desaparece;
- con «Conservar», queda sin uso, con su coste declarado en Q1.

CT-PBT2B-10 sigue en verde mientras exista.

#### D-PBT4-07. «Actualizar» con la escritura completa

- `TryUpdatePilot` conserva su nombre y su firma (G-10 y los llamadores de EIG).
- Su cuerpo pasa a `PushBackPilotUpdateRun.Precheck(draft)` y `.ExecuteFull(draft, request, port)`.
- **`<tras el cierre de PB-T3c>`:** el escritor borra vistas.
- **G-4** (IG:497-498) espera que WHC sea el único archivo que nombra `PushBackPilotUpdateRun.Execute(`. Tras el cambio, ninguno lo nombra: va a la lista por letra (§4.2).

#### D-PBT4-08. «Conservar mi borrador» en dos tiempos, sin miembro nuevo del puerto (OB-05.3 a 5; A-T3-3; I-02)

**Por `TryKeepMine`, que ya es una de las seis lecturas** (PEG:68). La petición sin testigo presenta; con testigo conserva:

1. **«Conservar mi borrador»** (`RequestKeepMine`) llama a `_port.TryKeepMine(new EditIntentRequest(session, rack, null, null))`. El anfitrión:
   - **paso 1:** relee con el origen (D-PBT4-03), con la detección de conflicto de F4;
   - **paso 2:** clasifica con `PushBackConflictClassifier.Classify(draft.InitialDraftState, estado releído, draft.CurrentDraftState, sobres de FullRead.StaleBase, read.Catalog)`;
   - devuelve `EditIntentOutcome` con el estado nuevo **`Presented`** (añadido al final del enumerado, como prefijo ordenado), las líneas de la presentación (`Presentation`, de solo lectura) y el testigo de lo releído, `EditIntentWitness(clave del origen, texto del sobre)`.
   - **No cambia el borrador.**
2. **La UI** muestra las líneas encima del Sí/No, en un `TextBlock` nuevo, `BrowserFieldKeepMinePresentationText`, dentro de `BrowserFieldChoicePanel`.
3. **«Sí»** (`ConfirmChoice`) llama a `_port.TryKeepMine(new EditIntentRequest(session, rack, null, testigo))`. El anfitrión relee con ese testigo; el lector lo compara con lo que hay (`PilotRead.PreviewWitness`):
   - **`Changed`:** no conserva; devuelve `Presented` con las líneas nuevas y el testigo nuevo («La versión cambió; revisa otra vez.»);
   - **`Unchanged`:** `draft.KeepMineOnCurrentVersion(full.Read, PushBackFullComposition.KeepOn, confirmed: true)`.
4. **«No»** no llama al puerto. **Sin «Sí» no se escribe.**

**Cuándo se sirve:** como hoy, en Conflict o en Dirty `IsPossiblyStale`; si no, `NotNeeded(NotKeepableReason)`.

**Lo declarado (V2-28):** pedir «Conservar» pasa a leer el dibujo. Contradice la regla de F4 «Asking for an option calls no operation of the port and only reads its memory» (UI :780-793) para la opción de conservar; la de descartar no cambia. Lleva su registro en `superseding-characterization.md`, y la prueba de la UI va a la lista por la causa C-3.

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

#### D-PBT4-11. La lista de sustituciones: el procedimiento (B-03, I-03, I-04)

**(a) Las causas cerradas, con su firma.** Una prueba existente se sustituye solo si falla en el GREEN por una de estas causas, y la firma se comprueba de forma mecánica:

| Causa | Qué la provoca | Firma (cómo se comprueba) |
|---|---|---|
| **C-1. El tipo del borrador** | D-PBT4-04 | El texto de la prueba o de su kit nombra `Find<PushBackPilotDraftState>`, `PushBackPilotDraftState` o `PushBackPilotStateComparer` sobre el anfitrión, y el fallo es `Assert.NotNull`, una `NullReferenceException` o una igualdad sobre ese estado |
| **C-2. El rechazo A/B** | D-PBT4-05 | La prueba espera `OutOfPilotDiagnostic` o `CompositeOutOfPilotReason` por el anfitrión, con un fixture compuesto, y el mensaje del fallo contiene «fuera del piloto» |
| **C-3. «Conservar» en dos tiempos y `KeepOn`** | D-PBT4-08 | La prueba llama a `TryKeepMine`, `RequestKeepMine` o `ConfirmChoice`, y el fallo es un estado `Presented` donde se esperaba otro, una lectura del doble donde no se esperaba ninguna, o el estado tras conservar |
| **C-4. «Actualizar» por `ExecuteFull`** | D-PBT4-07 | El fallo nombra `Execute` o `Precheck` del piloto, o un texto de la corrida que solo cambia por el modelo |
| **C-5. La letra de una guarda** | Lo que cambia el texto del producto | La regla y la línea de la guarda, en la tabla de §4 |
| **C-6. La regla de la escala** | D-PBT4-06 | La prueba usa el adaptador del piloto por el anfitrión, y el fallo es su mensaje o su campo estático |
| **C-7. La retirada del piloto** (solo con Q1 «Retirar») | D-PBT4-10 | La prueba no compila, o nombra un miembro retirado |

**Las reglas de la sustitución:**
- **Una `[Theory]`** no se salta entera si alguna fila sigue pasando: el equivalente reproduce **todas** las filas (las que pasan, tal cual; las que fallan, con su expectativa nueva). La original se salta entera.
- **Cada equivalente cubre todo lo que cubría la retirada** (OB-T1B.2): conserva cada aserción que no depende de la causa, con su texto. En una guarda de muchas reglas (PEG:265-350), el equivalente copia todas las reglas y cambia solo la afectada.
- **Las clases de los equivalentes:**
  - los que fallan en el RED (comportamiento del GREEN) van a una clase con `ExpectRed` true;
  - los que son guardas o autopruebas que pasan ya en el RED van a otra con `ExpectRed` false, fuera del filtro de las CT. Una autoprueba que no pase en el RED se declara como excepción, con su motivo.
- **La tabla de la lista,** una fila por método: la clase, el método, la causa, la firma, el equivalente y las aserciones que conserva.

**(b) Cómo se mide sin fijarla contra una implementación que el Worker no recibe.** Cuatro opciones. La sesión elige, o lo lleva al Owner si es material.

| Opción | Cómo | A favor | En contra |
|---|---|---|---|
| **1. Lista por causas, en dos tiempos** (recomendada) | **Antes de emitir:**<br>• un guion mecánico recorre `tests/` y selecciona los métodos cuyo texto, o el de su kit, cumple la firma de C-1 a C-7: la «lista por texto»;<br>• el esqueleto exacto se compila sobre una copia exportada y se ejecuta la suite: las roturas por letra (C-5);<br>• las dos van al Owner en Q1.<br>**En el GREEN:**<br>• la verificación ejecuta la suite;<br>• un fallo fuera de la lista **con** firma de causa cerrada es la parada PBT4B-C15: vuelve al Owner con el añadido, y la corrección rehace el RED con los `Skip` nuevos;<br>• un fallo **sin** firma es C16 | Ninguna implementación oculta; determinista y revisable; el Owner ve todo lo que se salta | La lista por texto puede incluir pruebas que no fallarían, y saltarlas sin necesidad debilita la red; cada una lleva su equivalente. Puede haber una segunda vuelta al Owner con el GREEN (Q3 lo prevé) |
| **2. Esqueleto tipado ejecutable** | El esqueleto ya tiene el tipo nuevo y cuerpos inertes; se compila y se ejecuta, y todo lo que falla y cumple una firma entra en la lista | Determinista; sin implementación | Con cuerpos inertes falla casi todo el anfitrión de Push Back: la lista sale muy grande y no distingue lo que fallaría de verdad; muchos equivalentes |
| **3. Referencia de medición, custodiada y sin entregar** | La sesión escribe un GREEN de referencia solo para medir; se custodia y lo lee la revisión; la lista se fija por nombre y firma | Lista exacta y pequeña | Es un procedimiento nuevo (I-04). Una diferencia del Worker en un detalle produce un C16 sin culpa de nadie. La sesión escribe producto |
| **4. Referencia entregada** | Como la 3, y el Worker la recibe como objetivo del GREEN | Sin divergencias | Cambia los papeles (la sesión escribe el producto y el Worker lo verifica); necesita el sí del Owner |

La recomendación es la 1: cumple (a) y (b) sin procedimientos nuevos, salvo la vuelta prevista al Owner. La 3 y la 4 son material de Owner.

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

#### D-PBT4-13. El anfitrión y el puerto generalizado (A-T3-11, A2-1; M-03)

**`TryEditAll`,** el único miembro nuevo del puerto. Es una operación de lectura (puede empezar el borrador).
- **Va con cuerpo por defecto en la interfaz** (`=> EditAllOutcome.NotAvailable`). Así compila sin tocarse el doble existente `WorkspacePilotEditPortDouble` (UI), que no se edita (C02).
- WHC la implementa de forma explícita, y la compuerta de avisos (`WorkspacePilotEditGate`, en `WorkspacePilotSupport.cs`) responde `Waiting` dentro de un aviso, como las demás.
- Rompe PEG:156-163 por letra, porque la reflexión ve el miembro: va a la lista (§4.3), con un equivalente de PEG que la añade como operación de lectura.

**Sus reglas,** copiadas de `TryKeepPilotEdit` (I-09):
1. `PassGate` primero;
2. la marca `_pilotReadInProgress`, como envoltorio, igual que `TryBeginEdit` (WHC:1020-1031), y mantenida **durante `ShowModal`** (D-PBT4-15);
3. `UpdateInFlightForRequest`;
4. la contención de excepciones con `Report`;
5. `NotePilotEdit(session)` en cada «Aplicar».

**Los resultados:**
- **sin borrador:** lo empieza por el camino de `TryBeginEdit` (D-PBT4-04) y sigue; si no se admite, devuelve ese resultado (por ejemplo, la cola);
- **Clean, Dirty o Conflict:** abre;
- **Applying:** `Waiting`;
- **Unknown, Error o Undecidable:** `Refused`, con el texto de hoy de cada estado, y no abre;
- **sin anfitrión del editor asignado:** «No editable aquí».

**El lanzamiento** (`Draft/PushBackFullDraftLaunch.For(DraftSession<PushBackFullDraftState> draft, IPushBackDraftStatePort port)`) lleva:
- `State` = `draft.CurrentDraftState.Editable` (la ventana copia al cargar);
- `RackId`;
- `SourceProject`, del sobre de `Origin` con `RackEmbedStore` y `RackProjectStore`;
- el puerto.

**El adaptador** (`Draft/PushBackFullDraftStatePort : IPushBackDraftStatePort`, `ctor(DraftSession<PushBackFullDraftState> draft, Action applied)`):
- `ApplyToDraft(edited)` llama a `SetCurrentDraftState(new PushBackFullDraftState(edited, draft.CurrentDraftState.Origin))`;
- traduce `Updated` a `Applied`; si no, `NotEditable`;
- si aplicó, llama a `applied()` = `() => NotePilotEdit(request.Session)`.

**La excepción de portador** se declara como la de `TryUpdatePilot` (EIH:1039-1046).

**`PushBackDraftSessionPort` y `LaunchFor`** se conservan sin uso, **con su registro en DISP** (M-07): son el puerto de PB-T2a sobre `DraftSession<PushBackEditableState>` y no estorban.

#### D-PBT4-14. Los estados de cada operación durante el editor (I-01; M-12)

Con la ventana abierta (dentro de `ShowModal`):

| Operación | Responde | Por qué |
|---|---|---|
| `SetPendingText`, `CommitField`, `CrossBoundary`, `DiscardRack` y `DiscardDocument` | `Busy`, con `PilotReadInProgressReason` | Miran la marca (WHC:1127, :1155, :1208, :1243, :1270) |
| `TryBeginEdit`, `TryCheckNow`, `TryReload`, `TryConfirmBase`, `TryKeepMine`, `TryDiscardAndReload` y `TryEditAll` | `Waiting` | La marca y `PassGate` (modal) |
| `RequestUpdate` | `Waiting` | `UpdateGate` (modal) |
| `GetField`, `GetRacksAtRisk`, `TakeLossNotices`, `TakeCloseNotices` y `TakeUpdateProgress` | Responden de memoria, como hoy | |

El texto de `Busy` dice «lectura en curso» mientras el editor está abierto. Se declara: en AutoCAD la paleta está deshabilitada por el modal WPF, y eso solo se ve en el Smoke-PB.

#### D-PBT4-15. Quién pone el modal (I-01; M-14)

- **En el producto,** el abridor del Plugin, con `using (_controller.EnterPanelModal())`. Es el único conductor admitido de la bandera: la regla de F4 «TheModalFlagIsDrivenOnlyByTheLiteralForwardAndByEnterPanelModalOverTheScope».
- **En Core,** el doble del anfitrión del editor llama a `SetModalRackCadActive(true)` y `false`, como hacen ya las pruebas de F4.
- **La marca de lectura** la pone el anfitrión (D-PBT4-13).
- **`IPushBackFullEditorHost`,** con `void ShowModal(PushBackDraftEditorLaunch launch)`, se declara en `Application/Workspace` y no en `Systems/PushBack` por EIH:1088 (un solo campo de tipo de `Systems` en WHC). Es una excepción declarada a R-PB-05 y a ADR-0029 D12 (P:732).
- **Se asigna en WCo** con `_host.SetFullEditorHost(new WorkspaceFullEditorHost(this))`, tras `SetPilotUpdateHost`. EIG:1150-1190 fija el cableado de F4: `<a medir en la comprobación del §6.0>`. Si lo rompe, va a la lista por C-5.

#### D-PBT4-16. El abridor y la guarda de OB-01.3 (M-05, M-09)

**`WorkspaceFullEditorHost.ShowModal(launch)`,** en `src/RackCad.Plugin/Workspace/`:
1. `using (_controller.EnterPanelModal())`;
2. `var window = RackPushBackSystemWindow.ForDraft(launch);`;
3. `AcApplication.ShowModalWindow(window);` **sin asignar `Owner`**, como el abridor clásico (M-09).

**Lo que no lleva:**
- ni `LockDocument`, ni transacción, ni lectura del dibujo, ni nada tras cerrar;
- ni `ShowModalDialog`, `ShowModelessDialog`, `ShowDialog`, `MessageBox`, `GetObject` ni `OpenCloseTransaction`, que prohíbe a `Plugin/Workspace/` `WorkspaceBridgeSourceGuardTests:139-142`.

**La guarda de OB-01.3** (CT-PBT4C-07), sobre **cinco archivos**:
- `WorkspaceFullEditorHost.cs`;
- WHC, con `TryEditAll` y sus ayudantes;
- FVM, con `EditAll`;
- `Draft/PushBackFullDraftLaunch.cs`;
- `Draft/PushBackFullDraftStatePort.cs`.

Ninguno nombra `RackPushBackCommands`, `RackMenuCommands`, `EditorModules`, `RackViewBatchExecution`, `DrawAndPlace`, `RedrawInPlace`, `InsertRequested` ni `InsertionRequest`. La autoprueba en negativo inyecta cada nombre en cada archivo.

#### D-PBT4-17. «Cerrar» con las dos cosas (A-T2b-OWNER-01; M-12)

`PendingWork()` de W (:3427), **solo en WORKSPACE**:
- **un módulo escenificado y cambios sin aplicar:** una pregunta, `WorkspaceStagedAndUnappliedQuestion` = «Hay un cambio de módulo sin confirmar y cambios sin aplicar al borrador; se perderán los dos al cerrar. ¿Deseas continuar?»;
- **uno solo:** la de hoy;
- **en CLASSIC:** nada cambia.

**La prueba** (CT-PBT4C-09) **lee `PendingWork()` y no cierra la ventana:** la suite de UI tiene un `MessageBox` real en su STA (`ci.yml:213-222`).

**La costura para escenificar un módulo sin el configurador,** `<a medir en la comprobación del §6.0>`. Las candidatas son las de `PushBackWorkspaceSupport.cs` y las de las pruebas de PB-T2b. Si ninguna sirve, es C02 y vuelve a la sesión antes de emitir.

### Común

#### D-PBT4-18. D2 (A2-5, A-T3-4; I-06)

**CT-PBT4B-10,** la huella profunda de CT-PBT1B-16 (copiada) de las dos referencias compartidas, **tomada tras cada operación**:
- condición 3: `Begin`, `SetCurrentDraftState` y el rebase de POST;
- condición 5: `SetPendingText` y `CommitField`;
- condición 6: `DiscardAndReload` y `KeepMineOnCurrentVersion` con `KeepOn`;
- condición 7: sustituir en `Current` el rack resuelto, o el informe de módulos, por otro objeto del mismo contenido no cambia `IsDirty`, porque el comparador no depende de esas dos referencias.

`<a medir>`: la letra de las condiciones 5 a 7 en [PT1] (las condiciones de D2). Si alguna no aplica, se declara con su motivo.

**CT-PBT4C-11,** sobre `ApplyToDraft`.

**El límite literal del Owner,** en PBT4x-C17.

**M11 (A2-8):** no aplica, porque el anfitrión no llama a `LoadDesignEditor` ni a `RestoreEditor`.

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

### 4.1 PB-T4a (aditiva)

| Guarda | Ancla | Qué hace PB-T4a | Resultado |
|---|---|---|---|
| CT-PBT3A-12 (`PushBackDraftWriteGuardTests:44-94`) | Fuera de `Draft/`, PP y UR nadie nombra los cinco tipos | RIA nombra `PushBackFullDraftReader` | **Por letra (C-5): se sustituye** por CT-PBT4A-06, el censo ampliado (admite RIA, NH, WEI y, desde ya, WHC) |
| EIG:736 | Seis `.ReadIntent(` en WHC | Quedan dos (D-PBT4-03) | **Por letra: se sustituye.** El equivalente exige seis asignaciones `var pilot = read.PilotRead;` (sin cambio), un `ReadIntent` en `Begin`, el ayudante con `ReadIntent` y `ReadIntentAtOrigin`, y que las cinco relecturas pasen por el ayudante |
| EIG:414-430 | NH: una `LoadCatalog()` y una `Read(` | Sin cambio | Verde |
| EIH:1175-1194 (la forma del resultado) | `PilotRead` solo en `Read` | Aditivo: `PilotRead` igual | Verde |
| EIH:1088 | Un campo de `Systems` en WHC | El diccionario de orígenes es `Dictionary<(SessionId, string), string>` | Verde |
| G-9 (IG:677-699) | RIA y NH con sus cuentas | Sin términos nuevos | Verde |
| `WorkspaceCommandAdvisorGuardTests` (:21-22) | Lee NH y RIA | `<a medir en la comprobación del §6.0>` | |

### 4.2 PB-T4b

| Guarda o prueba | Causa | Resultado |
|---|---|---|
| CT-PBT3B-13 (`PushBackWritePathGuardTests:235-296`) | C-5 | Se sustituye: su equivalente admite WHC con `Find<PushBackFullDraftState>`, `ExecuteFull(` y `Precheck(` |
| G-4 (IG:497-498: el conjunto de los que nombran `PushBackPilotUpdateRun.Execute(` es solo WHC) | C-5 | Se sustituye: el conjunto de los que nombran `PushBackPilotUpdateRun.ExecuteFull(` es solo WHC |
| EIG:711 (la línea de `KeepPilotEditsOn`), :776, :805 | C-5 y C-3 | Se sustituyen: `KeepMineOnCurrentVersion(full.Read, PushBackFullComposition.KeepOn, confirmed: true)`, solo tras un testigo `Unchanged` |
| EIG:733-735 (seis `var pilot = read.PilotRead;`) | C-5 | Se sustituye: seis `var full = read.FullRead;` y tres lecturas de `read.PilotRead.PreviewWitness` |
| PEG:280 (el adaptador del piloto, único campo estático) | C-6 | Se sustituye, con todas las demás reglas de PEG:265-350 copiadas |
| RG:116 (`Mutate` anclado en `RequestKeepMine`) | C-5 | Se sustituye con el ancla nueva |
| UI `WorkspacePilotReconciliationTests:780-793` y las que piden «Conservar» (hasta 21) | C-3 | Se sustituyen; registro declarado (D-PBT4-08) |
| Las de comportamiento del anfitrión | C-1 a C-4 | La lista de Q1 |
| CT-PBT2B-10 | — | Verde mientras exista el adaptador del piloto |
| Las de D-1 (E-1 a E-6) | C-7 solo con «Retirar» | Con «Conservar», verdes |

### 4.3 PB-T4c

| Guarda | Causa | Resultado |
|---|---|---|
| **PEG:156-163** (los 17 nombres del puerto por reflexión) | C-5: `TryEditAll` | Se sustituye. El equivalente: 18 nombres, `TryEditAll` entre las de lectura (:68) y con las reglas de lectura (PassGate primero, la marca y la contención) |
| PEG:380-385 (dos interfaces de WHC) | — | Verde: WHC no gana interfaces |
| EIH:1066 (los dos métodos con «Update») | — | Verde: `TryEditAll` no lo contiene |
| URT:1107-1109 | — | Verde: sin valor nuevo del enumerado |
| `WorkspacePilotSurfaceGuardTests:341` | — | Verde: el abridor va en el Plugin |
| `WorkspaceBridgeSourceGuardTests:139-142` | — | Verde por diseño (D-PBT4-16) |
| EIG:1150-1190 (el cableado) | `<a medir>` | Si rompe, C-5 |
| CT-PBT2B-08 (`PushBackWorkspaceModeSourceGuardTests`) | `<a medir>` (el literal nuevo y `PendingWork()`) | Si rompe, C-5 |
| T1B | — | Verde |

**Si cualquier otra prueba falla,** fuera de la tabla y de la lista, es C02 o C16 y vuelve al Owner (OB-T1B.3).

---

## 5. La red de pruebas

### 5.0 Reglas comunes

Las de la v1 (§5.0), con estos cambios:
- **Un `[Fact]` por CT, sin `[Theory]` nueva** (F4B-E46).
- **Los kits nuevos van en archivos nuevos:** `PushBackFullReadKit` (las lecturas `FromReads` sobre `IntentKit`) y los dobles del lector con origen, del puerto del panel y del anfitrión del editor. `EditIntentKit`, `WorkspacePilotEditPortDouble` y los demás no se editan.
- **Ninguna prueba de UI** crea un configurador, cierra una ventana con trabajo pendiente ni toca el perfil.
- **Las líneas se toman en el momento justo:** la huella, tras cada operación.

### 5.1 PB-T4a: la lista cerrada CT-PBT4A-01 a 07 (Core)

| CT | Qué fija | ¿Falla en el RED? |
|---|---|---|
| 01 | `FromReads`: `Read`, con `PilotRead`, `FullRead` y `Catalog`; un nulo da `ArgumentNullException`; `FromPilot` y los demás estados dejan `FullRead` y `Catalog` en null | Sí |
| 02 | El lector completo y el del piloto sobre el mismo escaneo de `IntentKit`: `pb-01-base` admitido en los dos; `pb-20-ab`, rechazado por el piloto (`OutOfPilotDiagnostic`) y admitido por el completo; `pb-21-cola`, rechazado en los dos; un miembro desconocido, AUTH-13 en los dos | Sí |
| 03 | El anfitrión guarda la clave del origen de `Begin` y la cambia tras un POST `Applied` (con el doble de la corrida de PB-T3b) | Sí |
| 04 | Las cinco relecturas usan `ReadIntentAtOrigin` con la clave guardada cuando el lector la ofrece; `Begin` usa `ReadIntent` con `ChosenReference` | Sí |
| 05 | Con un lector sin `IWorkspaceEditIntentOriginReader`, las cinco relecturas usan `ReadIntent` como hoy | **No** (`ExpectRed` false) |
| 06 | El censo ampliado (sustituye a CT-PBT3A-12): solo `Draft/`, PP, UR, RIA, NH, WEI y WHC nombran los cinco tipos; ningún archivo de la UI | **No** |
| 07 | La guarda del Plugin, en texto, con autoprueba en negativo:<br>• RIA llama una vez a `PushBackFullDraftReader.Read(` sobre el mismo `input`, después del lector del piloto;<br>• NH implementa `IWorkspaceEditIntentOriginReader` por el mismo método privado, con una `LockDocument` y una `RackPilotIntentAdapter.Read(`;<br>• NH devuelve `FromReads` | Sí |

**Las clases:**
- `PushBackFullReadPathTests` (01 a 04 y 07), `ExpectRed` true;
- `PushBackFullReadFallbackGuardTests` (05 y 06), `ExpectRed` false;
- más el equivalente de EIG:736, en `PushBackFullReadEquivalenceGuardTests`, `ExpectRed` false.

**Los totales.** Base: el GREEN de PB-T3c, `<tras el cierre de PB-T3c>`. Prevista: Core 19 129 = 19 123 + 6 omitidas; UI 2 451 = 2 430 + 21. Con 7 CT, el equivalente nuevo y 2 `Skip`:
- el método de CT-PBT3A-12, uno de los tres de `PushBackDraftWriteGuardTests`, que tiene uno por CT (12, 13 y 14);
- el de EIG:727-737, un `[Fact]`.

| Rama | Core | UI |
|---|---|---|
| **RED** | 19 129 + 7 + 1 = 19 137 resultados = (19 123 − 2 + 3) `Passed` + 5 `Failed` + (6 + 2) `Skipped` = 19 124 + 5 + 8 | 2 451 sin cambio |
| **GREEN** | 19 137 = (19 123 − 2 + 8) `Passed` + 8 `Skipped` = 19 129 + 8 | 2 451 |

- Los 3 `Passed` del RED son CT-05, CT-06 y el equivalente.
- **Los dos `Skip` y D = 0** se confirman en la comprobación de la precondición 6: que ningún otro método de esas clases rompe y que ninguno es una `[Theory]`. El contrato da el número exacto (F4B-E46).

### 5.2 PB-T4b: la lista cerrada CT-PBT4B-01 a 16

| CT | Qué fija | Proyecto |
|---|---|---|
| 01 | El borrador es el completo: `Find<PushBackFullDraftState>` no nulo y `Find<PushBackPilotDraftState>` nulo; el editable, igual al del lector completo; `Origin`, el texto del sobre | Core |
| 02 | OB-03.2 por el anfitrión: `pb-20-ab` editable; `pb-21-cola` rechazado con su pista; AUTH-13 | Core |
| 03 | El campo de escala: solo cambia `Editable.AnnotationScale`; `IsDirty`; sin regla asignada, «No editable aquí» | Core |
| 04 | La regla única: `AnnotationScaleRule.Validate` y la caja `AnnotationScaleBox` de una ventana real (sus propiedades) dan lo mismo en los diez textos de CT-PBT2B-10. La caja toma sus parámetros de la regla | **UI** |
| 05 | «Actualizar» por `ExecuteFull`, con `PilotEraseScopeDouble`: `Applied`, un `BeginMutation`, un `Seal`, el borrador en Clean y una línea `EraseUnit:<clave>` para un lateral obsoleto | Core |
| 06 | Los pasos 1 y 2: `TryKeepMine` sin testigo da `Presented`, con las líneas medidas antes de emitir (D-PBT4-09) y el testigo; el borrador, sin cambios (comparador y huella) | Core |
| 07 | El paso 3 y OB-05.4: con un testigo viejo, `Presented` de nuevo, sin conservar; con el bueno, `KeepOn` (`Initial` = la relectura, `Current.Editable` = el del borrador, `Current.Origin` = el nuevo) y Dirty | Core |
| 08 | Los pasos 4 a 7 por el panel: tras conservar y «Actualizar», el sobre del miembro lleva el diseño del borrador y el resto del rack actual | Core |
| 09 | OWNER-01 de PB-T2c: un lado B declarado sin frentes escribe el diseño sin lado B (D-PB-08), y el borrador conserva la declaración hasta el rebase | Core |
| 10 | D2, condiciones 3, 5, 6 y 7, con una fila por operación: `<operacion> huellas-iguales=True` y `sustituir-referencia isdirty=False` | Core |
| 11 | `NaN` (A-9 y A2-13), declarado: una longitud de módulo `NaN` en el commit del borrador deja `IsDirty` en `True` frente a una copia de sí mismo. Línea `nan-longitud-modulo isdirty=True` | Core |
| 12 | Sin traducción (M-11): WHC, WEI, `WorkspacePilotSupport.cs`, RIA y NH no construyen un `PushBackFullDraftState` a partir de `PilotRead` ni de un `PushBackPilotDraftState`. WHC no nombra `pilot.Read`, `KeepPilotEditsOn`, `PushBackPilotFieldAdapter`, `PushBackPilotStateComparer`, `Find<PushBackPilotDraftState>` ni `PushBackPilotUpdateRun.Execute(`. Con autoprueba | Core |
| 13 | La presentación pura, en la forma de Q2: las líneas exactas sobre una clasificación fabricada; la unión, el plural y las vistas rechazadas | Core |
| 14 | La UI de la elección: `RequestKeepMine` llama a `TryKeepMine` sin testigo y muestra las líneas antes del Sí; «Sí» pasa el testigo; «No» no llama al puerto | **UI** |
| 15 | La guarda de la elección: FVM llama a `TryKeepMine` solo en `RequestKeepMine` (sin testigo) y en `ConfirmChoice` (con él); la UI no compone texto | Core |
| 16 | El cableado de la regla: WCo asigna `SetFieldRule(AnnotationScaleRule.Validate)` una vez, tras `SetPilotUpdateHost`; WHC guarda la regla en un campo `Func<string, DraftFieldValidation>` | Core |

**Lo que falla en el RED:** todas, salvo CT-12, 15 y 16, que son guardas y van con `ExpectRed` false en su propia clase.
- Una autoprueba que pase en el RED sin su sujeto se declara como excepción.
- CT-12, en el RED, encuentra a WHC todavía con el piloto, así que **sí falla**: va con `ExpectRed` true. Las otras dos (15 y 16) no.

**Las clases:**
- `PushBackFullPanelTests` (01 a 03 y 05 a 11), `PushBackFullPanelPresentationTests` (13) y `PushBackFullPanelGuardTests` (12): `ExpectRed` true;
- `PushBackFullPanelWiringGuardTests` (15 y 16): `ExpectRed` false;
- `PushBackFullPanelFieldTests` (04 y 14): UI, `ExpectRed` true;
- los equivalentes de la lista, en `PushBackFullPanelEquivalenceTests` (`ExpectRed` true) y `PushBackFullPanelEquivalenceGuardTests` (`ExpectRed` false).

**Los totales** (F4B-E46), con la base del GREEN de PB-T4a (B_P `Passed`, B_S `Skipped`) y lo que se mide antes de emitir:
- K_B: los métodos que se saltan;
- D_B: las filas que pierden las `[Theory]` saltadas;
- E_B^R y E_B^G: los equivalentes que fallan en el RED y los que pasan en él;
- lo mismo para la UI, con el superíndice UI.

| Rama | Core | UI |
|---|---|---|
| **RED** | (B + 14 + E_B^R + E_B^G − D_B) resultados = (B_P − K_B − D_B + 2 + E_B^G) `Passed` + (12 + E_B^R) `Failed` + (B_S + K_B) `Skipped` | (2 451 + 2 + E^UI − D^UI) = (2 430 − K^UI − D^UI + E^UI,G) `Passed` + (2 + E^UI,R) `Failed` + (21 + K^UI) `Skipped` |
| **GREEN** | igual total, con 0 `Failed` | igual total, con 0 `Failed` |

### 5.3 PB-T4c: la lista cerrada CT-PBT4C-01 a 11

| CT | Qué fija | Proyecto | `ExpectRed` |
|---|---|---|---|
| 01 | El botón, en texto: XAML y code-behind | Core | true |
| 02 | El gesto: con texto pendiente inválido, `CommitField` da `Invalid` y `TryEditAll` no se llama; con texto válido, `CommitField` y después `TryEditAll`; sin texto, solo `TryEditAll`. La petición lleva `ChosenReference` nulo y sin testigo | UI | true |
| 03 | `TryEditAll`: sin borrador lo empieza; la cola no abre; Clean, Dirty y Conflict abren; Applying da `Waiting`; Unknown, Error y Undecidable dan `Refused`; sin anfitrión, «No editable aquí»; con texto pendiente, «Hay texto sin confirmar en el campo.» | Core | true |
| 04 | El adaptador del puerto: `Origin` conservado, `Applied` e `IsDirty`, `NotEditable`, y `NotePilotEdit(session)` por cada aplicar | Core | true |
| 05 | K = 1: dentro de `ShowModal` (el doble del anfitrión pone el modal), la tabla de D-PBT4-14 operación por operación; después, todas responden | Core | true |
| 06 | El resumen y «Modificado» al cerrar: «Modificado» y « *» tras un aplicar; igual que antes sin aplicar | UI | true |
| 07 | La guarda de OB-01.3, sobre los cinco archivos, con autoprueba en negativo | Core | **false** |
| 08 | El abridor, en texto: un `EnterPanelModal()` con `using`, un `ForDraft(`, un `ShowModalWindow(`, sin `Owner =`, sin `LockDocument`, `StartTransaction`, `GetObject` ni `Commit(`; y el cableado de `SetFullEditorHost` en WCo, tras `SetPilotUpdateHost` | Core | true |
| 09 | «Cerrar» con las dos cosas: `PendingWork()` (sin cerrar la ventana) da `WorkspaceStagedAndUnappliedQuestion` con las dos; la de hoy con una sola; en CLASSIC, la de hoy | UI | true |
| 10 | De punta a punta sin AutoCAD: empezar, «Editar todo» (el doble aplica «+ frente» y un gesto compuesto), «Actualizar» con `PilotEraseScopeDouble`: `Applied`, Clean. Una línea tras el gesto compuesto fija que lo aplicado es `editable` y no `LastComputation.Design` (F4B-E30, M-15) | Core | true |
| 11 | D2 en `ApplyToDraft`, con la huella tras cada aplicar | Core | true |

**Las clases:**
- `PushBackEditAllTests` (03 a 05, 10 y 11) y `PushBackEditAllGuardTests` (01 y 08): Core, `ExpectRed` true;
- `PushBackOpenerClassicPathGuardTests` (07): Core, `ExpectRed` false;
- `PushBackEditAllWindowTests` (02, 06 y 09): UI;
- el equivalente de PEG:156-163, en `PushBackEditAllPortEquivalenceGuardTests`: `ExpectRed` false. Pasa en el RED, porque el esqueleto ya declara `TryEditAll`.

**Los totales.** Con la base del GREEN de PB-T4b (C_P, C_S) y K_C: los métodos de PEG:156-163 y, si las hay, de EIG:1150-1190 y de CT-PBT2B-08.

| Rama | Core | UI |
|---|---|---|
| **RED** | (C + 8 + 1 − D_C) = (C_P − K_C − D_C + 2) `Passed` + 7 `Failed` + (C_S + K_C) `Skipped` | +3, con 3 `Failed` |
| **GREEN** | igual total, con 0 `Failed` | +3, con 0 `Failed` |

Los 2 `Passed` del RED son CT-07 y el equivalente de PEG.

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

**Identidad:**
- `Schema` `rackcad-gate-contract/v1`; `Unit` I-64; `Gate` F4B;
- `attempts` 1 de 3; `RoutingEnforcement` `required`; `CorrectionsAuthorized` true.

**TaskId** de §0, con el prefijo `F4B-PBT4-`.

**Las colecciones opacas** (F4B-E21) las pone la sesión.

**`ExpectedEvidence[0]`,** byte a byte, el literal de PB-T3 del sustituto de nc2:

> antes del Worker: el contrato, la salida del Controller (output.json, con las ocho colecciones opacas vacias), la delegacion final con esas colecciones puestas por la sesion (delegation.json; F4B-E21), la comparacion byte a byte sobre la delegacion final, A1-A8 y nc4;

**El tamaño** (F4B-E21): PB-T3c mide 54 162 bytes. La lista de PB-T4b va como tabla de nombres completos, con un número de líneas a medir. Si el contrato pasa de unos 60 KB, la lista va en un archivo custodiado que el contrato cita por su sha256, `<a decidir con la medición>`.

**`UNIT_CHANGE`** (M-14; F4B-E25):
- **PB-T4a:** WEI, WHC, RIA y NH;
- **PB-T4b:** WHC, WEI, `WorkspacePilotSupport.cs`, FVM, PV, WCo y W.xaml; con «Retirar», los archivos de `Pilot/` que retira;
- **PB-T4c:** WHC, `IWorkspacePilotEditPort.cs`, `WorkspacePilotSupport.cs`, WEI, FVM, PV, WCo y W.

**La comprobación antes de emitir** (precondición 6): el esqueleto exacto compilado sobre una copia exportada, la suite completa de Core y los filtros de UI. Mide:
- las roturas por letra;
- la lista según la opción elegida en D-PBT4-11;
- K, D y E de cada rama;
- las líneas de CT-PBT4B-06;
- las `<a medir>` de este paquete;
- el coste de `IsDirty` (M-13).

**Las paradas estándar** de [PKC2] §6.7 (17), las de [DP] (C08, C11 y C12) con los sustitutos vigentes, C17 (D2) y la regla RR completa (RR-1 a RR-7, C07 con su excepción y C18), como en [P3] §6.0.

**La lista cerrada de declaraciones nuevas** (INV-13; I-09), **por nombre**. Cualquier otra, pública o privada, es C13:
- **PB-T4a:**
  - `EditIntentReadResult.FullRead`, `.Catalog` y `.FromReads`;
  - `IWorkspaceEditIntentOriginReader` y `.ReadIntentAtOrigin`;
  - en WHC: el campo `_originKeys` y los privados `ReadForOpenDraft`, `RememberOrigin` y `ForgetOrigin`;
  - en RIA: `RackPilotIntentReadResult.Full`;
  - en NH: `ReadIntentAtOrigin` y el argumento nuevo de `ReadOnActiveDocument`.
- **PB-T4b:**
  - `PushBackFullFieldAdapter` (el constructor, `AnnotationScaleKey`, `Validate` y `Apply`);
  - `AnnotationScaleRule` (`Minimum`, `MinimumInclusive` y `Validate`);
  - `PushBackKeepMinePresentation` (`Of`, `Lines`, `IsEmpty` y los literales);
  - `PushBackPresentationForm` (los valores de Q2);
  - con «Por campos», `PushBackFieldLabels` (`Of` y la tabla);
  - `EditIntentStatus.Presented`, `EditIntentOutcome.Presentation` y su fábrica `Presented(lines, witness)`;
  - en WHC: el campo `_fieldRule`, `SetFieldRule`, `FullReadMissingReason`, `KeepStaleWitnessReason`, y los privados `PresentKeep`, `MembersOf` y `FindFullDraft`;
  - en FVM: `KeepMinePresentationText`.
- **PB-T4c:**
  - `IWorkspacePilotEditPort.TryEditAll` (con su cuerpo por defecto) y su implementación en `WorkspacePilotEditGate`;
  - `EditAllOutcome` y sus fábricas;
  - `IPushBackFullEditorHost.ShowModal`;
  - en WHC: el campo `_fullEditorHost`, `SetFullEditorHost`, `EditAllPendingTextReason` y el privado `OpenFullEditor`;
  - `PushBackFullDraftLaunch.For`;
  - `PushBackFullDraftStatePort` (el constructor y `ApplyToDraft`);
  - `WorkspaceFullEditorHost` (el constructor y `ShowModal`);
  - en FVM: `EditAll` y `ShowEditAllButton`;
  - en PV: `OnBrowserFieldEditAllClick`;
  - en W: `WorkspaceStagedAndUnappliedQuestion`.

**Nada en el texto del esqueleto lo leen las guardas.** Ni `ForWrite` como token, ni `Transaction` en `Pilot/`, ni `ShowDialog` ni `MessageBox` en `Plugin/Workspace/`, ni `Autodesk.` en Application, ni un `RequestKeepMine` que deje de ser el ancla de RG:116 antes del GREEN de PB-T4b.

**Las propias comunes:** C01 a C03, C05 a C07, C09, C10 (el configurador), C13, C14, C16, C17 y C18, como en la v1 (§6.0). **C15:** un fallo con firma de causa cerrada fuera de la lista (opción 1 de D-PBT4-11).

### 6.1 PB-T4a — `F4B-PBT4-FULL-READ-PATH`

- **Objective** (ASCII):
  > Hacer que la lectura de la intencion de edicion del Workspace de Push Back traiga tambien el modelo completo de PB-T3 sobre el mismo escaneo: el adaptador del Plugin llama al lector completo tras el del piloto y el resultado de la lectura lleva la lectura completa y el catalogo junto a la del piloto (FromReads; FromPilot no cambia); y que las relecturas de un borrador abierto usen la clave del origen guardada por el anfitrion cuando el lector la ofrece, y la referencia elegida si no (A-T3-8). El tipo del borrador del panel no cambia, nada traduce estados y no se lee el dibujo mas de una vez por lectura, segun el paquete del contrato de PB-T4 v2 y la lista cerrada CT-PBT4A-01..07. Solo se sustituyen con registro CT-PBT3A-12 y la cuenta de ReadIntent de EIG:736. Si hiciera falta mutar el rack resuelto o el informe de modulos dentro de la vida del borrador: STOP y se reabre L-1. No se ejecuta AutoCAD.

- **`AllowedWriteScope`:**
  - WEI, `IWorkspaceEditIntentOriginReader.cs` (nuevo), WHC, RIA y NH;
  - las tres clases nuevas y el kit nuevo;
  - **solo para los `Skip`:** `PushBackDraftWriteGuardTests.cs` y EIG.
- **El esqueleto del RED:**
  - `FromReads` deja `FullRead` y `Catalog` en null;
  - RIA y NH, sin cambio;
  - `ReadIntentAtOrigin` delega en `ReadIntent(…, null, …)`;
  - WHC, sin el diccionario;
  - los `Skip`, puestos.
- **`RequiredTests`:**
  - `PushBackFullReadPath`: 5, `ExpectRed` true;
  - `PushBackFullReadFallbackGuardTests` y `PushBackFullReadEquivalenceGuardTests`: 3, false;
  - `Workspace` (a medir) y las clases de PB-T1 a PB-T3: false.
- **Invariantes:**
  - INV-PBT4A-01: el alcance;
  - INV-PBT4A-02: el RED y el GREEN;
  - **INV-PBT4A-03:** una sola `CaptureDrawing` y una sola `LockDocument` por lectura;
  - **INV-PBT4A-04:** el tipo del borrador no cambia (`Find<PushBackPilotDraftState>` sigue en las nueve líneas).
- **`ExpectedEvidence`:** como en [P3] §6.1, con el literal de nc2.
- **La línea base:** el GREEN de PB-T3c.

### 6.2 PB-T4b — `F4B-PBT4-FULL-DRAFT-PANEL`

- **Objective** (ASCII):
  > Pasar el borrador del panel del Workspace de Push Back al modelo completo de PB-T3 con la lectura completa de PB-T4a, sin traducir estados: OB-03.2 (los racks A/B se editan en el panel; la cola y AUTH-13 siguen rechazados), la regla unica de la escala de la UI conectada por el Plugin, Actualizar con PrepareFull y ExecuteFull, y Conservar mi borrador en dos tiempos por TryKeepMine (presentacion clasificada con un testigo de lo releido, y KeepOn solo tras confirmar con el testigo vigente), segun el paquete del contrato de PB-T4 v2, la decision del Owner sobre el piloto y la presentacion, la lista cerrada CT-PBT4B-01..16 y la lista de sustituciones que vio el Owner. Si hiciera falta mutar el rack resuelto o el informe de modulos dentro de la vida del borrador: STOP y se reabre L-1. Ni Editar todo, ni RACKEDITAR; no se ejecuta AutoCAD.

- **`AllowedWriteScope`:**
  - WHC, WEI, `WorkspacePilotSupport.cs`, FVM, PV, WCo y W.xaml;
  - los nuevos `Draft/PushBackFullFieldAdapter.cs`, `Draft/PushBackKeepMinePresentation.cs` (y `Draft/PushBackFieldLabels.cs` con «Por campos») y `src/RackCad.UI/Controls/AnnotationScaleRule.cs`;
  - con «Retirar», los archivos de `Pilot/` de D-PBT4-10;
  - las clases nuevas;
  - **solo para los `Skip`:** los archivos de la lista.
- **El esqueleto:**
  - los tipos y miembros nuevos, con cuerpos inertes;
  - WHC sin cambio;
  - la regla devuelve inválido;
  - los `Skip` de la lista, puestos.
- **`RequiredTests`:**
  - Core `PushBackFullPanel`: 12 + E_B^R, true;
  - Core `PushBackFullPanelWiringGuardTests` y `PushBackFullPanelEquivalenceGuardTests`: 2 + E_B^G, false;
  - UI `PushBackFullPanelFieldTests`: 2, true;
  - y el resto sin `ExpectRed`.
- **Invariantes:**
  - INV-PBT4B-03: sin traducción;
  - INV-PBT4B-04: sin testigo vigente no se conserva;
  - INV-PBT4B-05: la presentación se compone en Application;
  - INV-PBT4B-06: una sola implementación de la regla de la escala en el producto, salvo el adaptador del piloto con «Conservar».
- **`StopConditions`:** C15 de la opción 1, C13 y C17.
- **La línea base:** el GREEN de PB-T4a.

### 6.3 PB-T4c — `F4B-PBT4-EDIT-ALL-WINDOW`

- **Objective** (ASCII):
  > Anadir Editar todo al panel del Workspace de Push Back sobre el borrador completo de PB-T4b: un boton y su gesto, que confirma el texto pendiente con CommitField antes de abrir (R-PB-10); TryEditAll en el anfitrion, que empieza el borrador si hace falta, mantiene la marca de lectura mientras la ventana esta abierta (K = 1) y abre la ventana de Push Back en modo WORKSPACE por un anfitrion del editor del Plugin, que entra en el modal del panel y la muestra sin dueno; el adaptador del puerto que conserva Origin y acaba la marca known clean; la guarda de texto de OB-01.3 sobre los cinco archivos del camino; Cerrar con una sola pregunta si hay a la vez un modulo escenificado y cambios sin aplicar; y el resumen y Modificado al cerrar, segun el paquete del contrato de PB-T4 v2 y la lista cerrada CT-PBT4C-01..11. Solo se sustituye con registro la guarda de las operaciones del puerto (PEG:156-163). Ni el camino clasico, ni RACKEDITAR, ni la ventana en CLASSIC; cero escrituras en el DWG desde la ventana; no se ejecuta AutoCAD.

- **`AllowedWriteScope`:**
  - WHC, `IWorkspacePilotEditPort.cs`, `WorkspacePilotSupport.cs`, WEI, FVM, PV, WCo y W;
  - los nuevos `IPushBackFullEditorHost.cs`, `Draft/PushBackFullDraftLaunch.cs`, `Draft/PushBackFullDraftStatePort.cs` y `Plugin/Workspace/WorkspaceFullEditorHost.cs`;
  - las clases nuevas;
  - **solo para el `Skip`:** PEG.
- **`RequiredTests`:**
  - Core `PushBackEditAll`: 7, true;
  - Core `PushBackOpenerClassicPathGuardTests` y `PushBackEditAllPortEquivalenceGuardTests`: 2, false;
  - UI `PushBackEditAllWindowTests`: 3, true;
  - y el resto.
- **Invariantes:**
  - INV-PBT4C-03: cero escrituras;
  - INV-PBT4C-04: W solo cambia `PendingWork()` y su literal, en WORKSPACE;
  - INV-PBT4C-05: `ForDraft` tiene un solo llamador en `src/`;
  - INV-PBT4C-06: `EnterPanelModal()`, un solo llamador.
- **La línea base:** el GREEN de PB-T4b.

---

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
1. la v2;
2. su comprobación acotada;
3. el cierre de PB-T3c;
4. la medición de la lista sobre ese cierre (precondición 6, con la opción de D-PBT4-11 que elija la sesión);
5. las preguntas al Owner, en una sola vuelta;
6. la emisión de PB-T4a.

**Lo que la sesión declara, sin preguntar** (V2-28):
- que pedir «Conservar» lee el dibujo, frente a la regla de F4 de la UI :780-793, con su registro;
- el procedimiento de la lista que elija.

Las opciones 3 y 4 de D-PBT4-11 cambian el procedimiento: si se eligen, se añaden a Q3 como condición.

**Q1. El piloto, con la lista** (B-03, I-05). La recomendada va primero: es «Retirar», porque cumple la letra de A-5, R-T1-17 y la premisa de D-1, y deja una sola regla de la escala.

> PB-T4 pasa el borrador del panel de Push Back al modelo completo de PB-T3, sin convertir borradores, como pediste. Al hacerlo, <N> pruebas existentes dejan de describir el panel: <n1> de comportamiento, <n2> guardas de F4 y de PB-T3, y <n3> de la interfaz del panel. La lista, por clase y por causa, va en el paquete. Se midieron sobre <cómo, según la opción elegida>. Cada una se sustituye con registro por otra que conserva todo lo que protegía, como en D-1.
>
> Queda además código del piloto que el panel ya no usará: su comparador, su adaptador de la escala (con su propia copia de la regla, que pediste dejar en una sola), «Conservar» y la composición del piloto, y su Prepare, Precheck y Execute. PB-T1 dejó la copia de la carga del piloto como un modelo paralelo interino que PB-T4 debía retirar, y tu decisión D-1 contaba con quitar el piloto. El lector del piloto y su carga se quedan en todos los casos, porque el modelo completo los usa para la admisión y el rechazo AUTH-13. ¿Qué hace PB-T4 con ese código?

- **«Retirarlo (Recomendado)»:** se retira lo que solo usaba el panel. Se sustituyen además sus <n4> pruebas propias y tres equivalentes de D-1 (E-3, E-5 y E-6). Queda una sola regla de la escala. La copia de la carga sigue solo dentro del lector.
- **«Conservarlo sin uso»:** no se toca y sus <n5> pruebas siguen. Quedan, anotados en ideas futuras, el modelo paralelo de PB-T1, la segunda copia de la regla de la escala y la premisa de D-1 sin cumplir.
- **«Dos modelos hasta F5-A»:** el campo de escala del panel sigue con el piloto; «Editar todo» descarta o exige actualizar el borrador del piloto antes de abrir uno completo, que se lee de nuevo del dibujo. El panel tiene dos códigos de borrador y dos «Conservar mi borrador», y necesita igual la lectura completa de PB-T4a. Las sustituciones bajan a <n6>.

**Q2. La presentación** (I-11). La recomendada va primero: es «Por campos», porque es la letra de D-PB-05 («fields changed externally / by the user») con el menor cambio, y la nota del Owner sobre la interfaz pide funcionalidad antes que forma.

> Antes de aceptar «Conservar mi borrador», pediste mostrar lo bastante para distinguir los campos cambiados fuera, los cambiados por el usuario, los cambiados en los dos sitios, los cambios estructurales (módulos, frentes, lado B) y lo que pasa con las vistas, sin un diff JSON. El clasificador de PB-T3 da cada valor cambiado con su grupo (nombre, rack, anotaciones, seguridad, frentes, módulos, lado B, parte compuesta, fuera del editor). ¿Qué se muestra encima del Sí/No del panel?

- **«Por campos (Recomendado)»:** una línea por clase con el nombre en español de cada campo cambiado, por ejemplo «Cambiado fuera: el nombre y el módulo 3: longitud.», más una línea de lo estructural y otra de las vistas. Necesita una tabla de nombres para los campos del editor.
- **«Por grupos»:** lo mismo, pero con los grupos afectados, por ejemplo «Cambiado fuera: el nombre y los frentes.». Dice qué zonas cambiaron, no qué campo.
- **«Por campos, con sus valores»:** como «Por campos», con el valor de antes, el de fuera y el tuyo tras cada campo. Es la más larga.

**Q3. La emisión** (B-03).

> PB-T4 va en tres subtareas: PB-T4a, que lee también el modelo completo sin cambiar el panel; PB-T4b, que pasa el panel al modelo completo, con el nuevo «Conservar mi borrador»; y PB-T4c, que añade «Editar todo». Ninguna autorización tuya cubre PB-T4. La lista de pruebas sustituidas de cada una va en Q1 (PB-T4a: <a>; PB-T4b: <b>; PB-T4c: <c>). ¿Emito cada subtarea, en ese orden y cada una tras el cierre de la anterior, sin volver a ti, si su comprobación acotada da 0 bloqueantes y 0 importantes sin resolver, la remedición sobre el cierre anterior no cambia nada material y la lista medida al emitir es la que viste, sin ninguna prueba añadida?

- **«Sí, con esas condiciones (Recomendado)»:** vuelve a ti por una decisión nueva, un control con una firma no prevista, un intento 3, un efecto material sin medir o una prueba fuera de la lista. Si en el GREEN de PB-T4b aparece una prueba con causa cerrada fuera de la lista, el Worker para y te vuelve con ella antes de la corrección.
- **«Solo PB-T4a; las demás me vuelven»**.
- **«No; cada una me vuelve con su lista»**.

### 8.3 Lo que no es decisión

- La división en tres y su orden.
- Los sustitutos de nc1 y nc2 con `--task PBT4` para las tres subtareas: el prefijo `F4B-PBT4-` las liga a las tres, y no las distingue entre sí (M-V1 de nc2). Es una lectura declarada; se vuelve al Owner por [DP] si la revisión o un control la contradicen.
- La regla única en la UI, conectada por el Plugin.
- `PushBackDraftSessionPort` y `LaunchFor`, conservados con registro.
- A-T3-9 fuera, con registro.

---

## 9. Límites

- Nada se compiló ni se ejecutó. Todas las cifras son previsiones o recuentos de texto.
- El worktree no se leyó.
- PB-T3c no está cerrada.
- **Las `<a medir>`:**
  - `WorkspaceCommandAdvisorGuardTests` frente a RIA y NH;
  - EIG:1150-1190 frente al cableado nuevo;
  - CT-PBT2B-08 frente al literal y `PendingWork()`;
  - las guardas de T1B frente a `x:Static` en W.xaml;
  - la costura para escenificar un módulo;
  - la letra de las condiciones 5 a 7 de D2 en [PT1];
  - si alguna prueba fija los nombres de `EditIntentStatus` (el valor nuevo `Presented`);
  - si alguna guarda prohíbe un miembro de interfaz con cuerpo por defecto (`TryEditAll`);
  - la lista de PB-T4b, con sus K, D y E;
  - las líneas de CT-PBT4B-06;
  - el coste de `IsDirty`.
- El texto de Q1 y Q3 lleva huecos que solo llena la medición.

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
