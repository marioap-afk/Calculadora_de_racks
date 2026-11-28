## 5. La red de pruebas

### 5.0 Reglas comunes (enteras; V41-12)

**Las reglas de PB-T1 a PB-T3,** que se copian en cada contrato y no se heredan:
- `[Fact]` que acaban en **un** `Assert.Equal` de las líneas unidas por `\n`, con «R», cultura invariante, GUID fijo y orden ordinal;
- **un `[Fact]` por CT. Ninguna `[Theory]` nueva:** la TRX cuenta distinto una `Theory` con `Skip` (F4B-E46);
- la regla «EXCEPCION» para toda llamada al API nuevo: una excepción se escribe como la línea `EXCEPCION <Tipo>: <mensaje>` y no escapa de la CT;
- **las pruebas de UI:**
  - en `EditorDiscardPromptCollection`, con la carga dentro del `try` y `Cerrar` en `finally`;
  - sin teclado ni foco real, sin `MessageBox` real y sin ventanas que no sean la de Push Back;
  - **ninguna crea un `RackFrameConfiguratorWindow`** (F4B-E30), ninguna cierra una ventana con trabajo pendiente y ninguna escribe en el perfil;
- sin AutoCAD, sin BOM, sin «Guardar en biblioteca» y sin «Insertar»;
- ningún fixture nuevo en `Fixtures/I64`;
- **los ayudantes existentes se usan sin editarlos** (`IntentKit`, `PilotKit`, `DraftKit`, `PilotUpdateScopeDouble`, `PilotEraseScopeDouble` de `PushBackFullWriteSupport.cs`, `WorkspaceHostTestSupport`, `WorkspacePilotEditPortDouble`, `EditIntentKit`). **Lo que falte va en un archivo nuevo de la subtarea:**
  - PB-T4a: `tests/RackCad.Tests/PushBackFullReadSupport.cs`, con `PushBackFullReadKit` (las lecturas `FromReads` sobre `IntentKit`) y `PushBackOriginReaderDouble` (el lector con origen, que registra lo que recibe);
  - PB-T4b: el kit nuevo de «Saltar y duplicar» (`WorkspaceFullReadKit.cs`, D-PBT4-11) y el doble del puerto del panel;
  - PB-T4c: el doble del anfitrión del editor.

**El momento de cada lectura** (F4B-E30 y F4B-E33): cada línea se arma justo después de su último gesto. El borrador guarda copias, y el rebase de POST cambia `InitialDraftState` y `CurrentDraftState`. La huella se toma tras cada operación.

**Los gestos que no cambian nada** (la lección M-13): una edición escribe un valor distinto del que tiene el diseño, en un campo que el ensamblador conserva. Se usan los valores medidos de PB-T3: `PalletTolerance` 4.5 y `PalletDepth` 48.5. `BeamDepth` no se edita.

**Los archivos nuevos se comprueban contra las guardas existentes antes de fijar el esqueleto** (la lección de F4B-E42 y F4B-E45). Antes de emitir, la sesión:
- compila el esqueleto exacto del RED sobre una copia exportada (fuera del worktree);
- ejecuta la suite completa de Core y los filtros de UI de la subtarea;
- barre los nombres nuevos y los textos del esqueleto contra los literales de todas las guardas que leen `src/` (el barrido de nombres y el de sobrecargas);
- **mide las guardas sobre el texto literal del GREEN que prescribe el paquete,** aplicado en memoria (para PB-T4a, `measure_pbt4_v41-out.txt`, §4.1).

**Un solo «Actualizar» lógico:** las recetas de PB-T4b no llaman a «Actualizar» desde la ventana; solo desde el panel.

**Las CT de I-24** (CT-PBT4A-08, CT-PBT4B-19 y CT-PBT4C-15): copian, sin referenciarlos, las reglas de G-L1 de CT-PBT2A-04 (`PushBackDraftPortSharedReferenceGuardTests.cs`: `Tokens`, `Rebind`, `Rule1`, `Rule1b`, `Rule1c`, `Clean`, `Collapse`, `Members`, `Aliases`, `Violations` e `Inject`, con sus 15 autopruebas) y la pureza de CT-PBT3A-14 (`ForbiddenTerms` y `Purity`). Cada autoprueba se inyecta en memoria en el texto de **cada** archivo de su lista, y falla si las reglas la ven en todos.

### 5.1 PB-T4a: la lista cerrada CT-PBT4A-01 a 08 (Core; entera)

| CT | Qué fija | Las líneas (literales del paquete) | Clase | `ExpectRed` |
|---|---|---|---|---|
| 01 | `FromReads`: `PilotRead`, `FullRead` y `Catalog`; un nulo da `ArgumentNullException`; `FromPilot` y los demás estados los dejan en null | `fromreads piloto=True completo=True catalogo=True estado=Read`; `fromreads-nulo piloto=ArgumentNullException completo=ArgumentNullException catalogo=ArgumentNullException`; `frompilot completo=null catalogo=null`; `otros completo=null catalogo=null` (`NoActiveDocument`, `Busy`, `Rejected` y `Failed`) | `PushBackFullReadPathTests` | true |
| 02 | El lector completo y el del piloto sobre el mismo escaneo de `IntentKit`, por `PushBackFullReadKit` (`FromReads`): `pb-01-base`, `pb-20-ab` (rechazado por el piloto y admitido por el completo), `pb-21-cola` y el caso de AUTH-13 de PB-T3a | Una por diseño, `<diseño> piloto=<Admitted|NotAdmitted> completo=<Admitted|NotAdmitted> mismo-escaneo=True`; entre ellas, literal, `pb-20-ab piloto=NotAdmitted completo=Admitted mismo-escaneo=True` | `PushBackFullReadPathTests` | true |
| 03 | **Las cinco relecturas** usan `ReadIntentAtOrigin` con `draft.CurrentDraftState.Origin` cuando el lector la ofrece (`PushBackOriginReaderDouble`); `Begin` usa `ReadIntent` con `ChosenReference` | `Begin lector=ReadIntent`; y, por cada una de `CheckNow`, `Reload`, `ConfirmBase`, `KeepMine` y `DiscardAndReload`, `<operación> lector=ReadIntentAtOrigin origen-del-borrador=True` | `PushBackFullReadPathTests` | true |
| 04 | **La elección por texto, pura:** `PushBackOriginByText.Choose(DraftStaleBase, string)` sobre `PushBackPilotIntentReader.BuildStaleBase` del escaneo, como UR:645-649, y `PushBackOriginByText.AtKey` | `choose uno-mutable=<clave>`; `choose ninguno=null`; `choose varios=null`; `choose no-mutable=null`; `atkey clave=<clave> resto-igual=True` | `PushBackFullReadPathTests` | true |
| 05 | Con un lector sin la interfaz nueva, las cinco relecturas usan `ReadIntent` como hoy | Por cada operación, `<operación> lector=ReadIntent` | `PushBackFullReadGuardTests` | **false** |
| 06 | El censo: solo `Draft/`, PP, UR y RIA nombran los cinco tipos de CT-PBT3A-12. **Sustituye a CT-PBT3A-12** | Las de CT-PBT3A-12, iguales: `sin-conectar archivos-revisados>0=True coincidencias=[]` y las diez `autoprueba-<nombre>-en-<destino>=falla` (los cinco nombres en `WorkspaceHostCore.cs` y en `WorkspacePilotSupport.cs`) | `PushBackFullReadGuardTests` | **false** |
| 07 | **La guarda del Plugin, en texto,** con autoprueba en negativo sobre texto inyectado en memoria | `ria-firma-de-Read-sin-cambio=True`; `ria-Read completo=1 tras-el-piloto=True`; `ria-AtOrigin choose=1 atkey=1 base=1 piloto=1 completo=1 lecturas-del-dibujo=0 literales=0`; `nh-lector interfaces=IWorkspaceEditIntentReader,IWorkspaceEditIntentOriginReader`; `nh-ReadIntentAtOrigin ReadOnActiveDocument=1`; `nh-AtOrigin=1 dentro-del-bloqueo=True tras-Read=True`; `nh-resultado FromReads=1 FromPilot=0`; `autoprueba-sin-lector-completo=falla`; `autoprueba-AtOrigin-con-CaptureDrawing=falla`; `autoprueba-segundo-AtOrigin=falla`; `autoprueba-FromPilot-en-NH=falla` | `PushBackFullReadPathTests` | true |
| **08** | **I-24:** G-L1 y la pureza sobre el archivo nuevo de `Draft/` de PB-T4a, `PushBackOriginByText.cs` (§5.0) | **32:** `g-l1 archivos=1 reglas-sin-coincidencias=True`; las 15 `autoprueba-<a, b, c, e, f1, f2, f3, g1, g2, h1, h2, i, j, k1, k2>=falla`; `pureza archivos=1 violaciones=[]`; `autoprueba-#nullable=falla`; `autoprueba-namespace RackCad.Application.Systems.PushBack.Draft=falla`; y las 13 `autoprueba-<término>=falla` de `ForbiddenTerms` | `PushBackFullReadGuardTests` | **false** |

- **En CT-07,** `literales=0` y `lecturas-del-dibujo=0` son la restricción de CAG (D-PBT4-02): el cuerpo de `AtOrigin` no tiene ningún literal de cadena, ni `CaptureDrawing`, `LockDocument`, `Transaction`, `AssertNoOpenTransaction` o `ex.Message`. `ria-firma-de-Read-sin-cambio` compara la firma con la expresión de EIG:139-141.
- **CT-08 pasa en el RED:** el esqueleto ya crea `PushBackOriginByText.cs` (con `Choose` que devuelve null y `AtKey` que devuelve el escaneo), puro y sin ninguna ficha de G-L1. Sus autopruebas fallan sobre el texto inyectado.

**Las clases:** `PushBackFullReadPathTests` (01 a 04 y 07), con `ExpectRed` true; `PushBackFullReadGuardTests` (05, 06 y 08), con false. Las dos y `PushBackFullReadSupport.cs`, en `tests/RackCad.Tests/`, con `namespace RackCad.Tests`; ningún filtro heredado las selecciona por prefijo.

**Los totales** (F4B-E46: una sola rama, porque PB-T4a no quita ningún `Skip` ni añade ninguna `Theory`). La base: el cierre de PB-T3c (CI `44757675821` sobre `938a35f0`), **Core 19 129 = 19 123 `Passed` + 6 `Skipped`**, y UI 2 451 = 2 430 + 21. PB-T4a añade **8 CT, ocho `[Fact]`,** y pone **un `Skip`** (CT-PBT3A-12, un `[Fact]`):

| Rama | Core | UI |
|---|---|---|
| **RED** | **19 137 = 19 125 `Passed` + 5 `Failed` (CT-01 a 04 y 07) + 7 `Skipped`** | 2 451 = 2 430 + 21, sin cambio |
| **GREEN** | **19 137 = 19 130 `Passed` + 0 `Failed` + 7 `Skipped`** | 2 451 = 2 430 + 21 |

- 19 125 = 19 123 − 1 (el `Skip`) + 3 (CT-05, 06 y 08, que pasan en el RED).
- **La suite de Core sobre la copia con el esqueleto** (el parche de `src/` y el `Skip`, sin las clases nuevas; precondición 6): **19 129 = 19 122 + 7**. Sin el `Skip` daría 19 129 = 19 123 + 6, y CT-PBT3A-12 pasaría, porque RIA no cambia en el esqueleto.
- D = 0: ninguna otra prueba falla en el RED ni en el GREEN (§4.1). Se confirma en la precondición 6.

### 5.2 PB-T4b: la lista cerrada CT-PBT4B-01 a 20

Las CT-01 a 17 de la v3 y la CT-18 de la v4, con lo que fijan las decisiones del Owner:
- **CT-03:** el campo valida con `PushBackAnnotationScaleRule` sin asignar nada.
- **CT-04 (UI):** `PushBackAnnotationScaleRule` frente a `AnnotationScaleBox` de una ventana real (parámetros y validación en los diez textos de CT-PBT2B-10).
- **CT-06:** la presentación «Por grupos» (D-PBT4-09), con las líneas medidas antes de emitir sobre el clasificador real.
- **CT-13:** el censo pasa a admitir WHC, y sustituye a CT-PBT4A-06.
- **CT-16:** WHC crea el adaptador con `PushBackAnnotationScaleRule.Validate` y WCo no asigna ninguna regla.
- **CT-18 (UI):** `NumericFieldValidation.Validate` y `NumericTextRule.Check` dan el mismo resultado (estado, valor y mensaje) en una tabla fija de textos y parámetros. `ExpectRed` true: en el RED, la regla de Application es inerte.

**Y dos nuevas:**

| CT | Qué fija | Las líneas | Clase | `ExpectRed` |
|---|---|---|---|---|
| **19** | **I-24:** G-L1 y la pureza sobre los tres archivos nuevos de `Draft/` de PB-T4b: `PushBackFullFieldAdapter.cs`, `PushBackKeepMinePresentation.cs` y `PushBackAnnotationScaleRule.cs` (§5.0). **Cubre `Apply`:** una línea dice en cuál de los tres se declara | **33:** `g-l1 archivos=3 reglas-sin-coincidencias=True`; las 15 autopruebas de G-L1; `pureza archivos=3 violaciones=[]`; las 15 de la pureza; y `cubre Apply=PushBackFullFieldAdapter.cs` | `PushBackPanelDraftGuardTests` (Core) | **false** |
| **20** | **El guion de «Saltar y duplicar»** (D-PBT4-11): para cada una de las 9 clases, el original igual a la base salvo los `Skip`, cada prueba con su copia o su fila «a mano», y la copia igual al original con el mapa M | Una por clase, `copia <Clase> original-igual-a-la-base=True pruebas=<n> copiadas=<c> a-mano=<h> copia-igual-al-mapa=True`, con los números de `pbt4-copy-map.tsv`; y `autoprueba-linea-de-mas=falla`, `autoprueba-asercion-cambiada=falla`, `autoprueba-skip-de-mas=falla` y `autoprueba-sin-copia-ni-fila=falla` | `WorkspaceFullCopyGuardTests` (Core) | **false** |

**CT-19 y CT-20 pasan en el RED:** el esqueleto de PB-T4b crea los tres archivos de `Draft/` (inertes y puros), y las copias, el kit nuevo y los `Skip` van en el commit RED. **El esqueleto del RED de PB-T4b no toca `NumericFieldValidation.cs`** (M-29): la delegación en `NumericTextRule` es del GREEN; en el RED, `NumericTextRule.Check` existe y es inerte, y la caja de la UI sigue con su lógica de hoy.

**Las copias, en su propio espacio de nombres:** `namespace RackCad.Tests.FullPanelCopies;`, para que el filtro heredado `RackCad.Tests.Workspace` (que el RED tiene que pasar, INV-PBT3A-02) no las seleccione; CT-20 sí está en `RackCad.Tests` y pasa en el RED.

**Un punto abierto de PB-T4b, para antes de emitirlo** (§8.3, punto 3): en el RED, el anfitrión todavía es del piloto, así que unas copias fallan (las que buscan el borrador completo) y otras pasan (las del ciclo de vida o de los avisos). Un filtro con `ExpectRed` true exige que fallen todas, y uno con false, que pasen todas. **Lo que propone el paquete, sin decidirlo:** que la precondición 6 mida, sobre la copia exportada con el esqueleto, cuáles fallan, y que el mapa M parta cada copia en dos clases por esa medida (`<Clase>FullRedTests` y `<Clase>FullGreenTests`), con CT-20 comprobando la unión. Necesita el kit nuevo para medir: si lo escribe la sesión, se acerca a la «implementación de referencia» que el Owner no eligió.

**Los totales de PB-T4b** salen de la precondición 6, con la lista que apruebe el Owner (§8.3), y el contrato los fija por rama (F4B-E46).

### 5.3 PB-T4c: la lista cerrada CT-PBT4C-01 a 15

Las CT-01 a 13 de la v3 y la CT-14 de la v4, con estos cambios:
- **CT-07:** los seis sujetos de D-PBT4-16.
- **CT-08 (M-30):** las dos piezas del abridor, en texto:
  - `FullEditorModal` en WCo, con un `using (_controller.EnterPanelModal())` y la llamada a `_open`, sin `RackCad.UI` ni `PushBack`;
  - `RackWorkspaceEditorWindow.Show(object launch)`, con **esta línea literal, una vez:** `var window = RackPushBackSystemWindow.ForDraft((PushBackDraftEditorLaunch)launch);`; después `AcApplication.ShowModalWindow(window);`, una vez; sin `Owner =` ni `RackCad.Application.Workspace`;
  - y el cableado en WCo, `_host.SetFullEditorHost(new FullEditorModal(this, RackWorkspaceEditorWindow.Show))`, después de `SetPilotUpdateHost`.
  - Sus líneas llevan el literal de la conversión: `abridor conversion=[var window = RackPushBackSystemWindow.ForDraft((PushBackDraftEditorLaunch)launch);] veces=1`.
- **CT-10 (Core):** puerto real (`WorkspacePilotEditGate` sobre WHC) → `TryEditAll` → el doble de `IWorkspaceFullEditorHost` aplica por el puerto (incluido un gesto compuesto, F4B-E30) → «Actualizar» con `PilotEraseScopeDouble` → `Applied` y Clean.
- **CT-14 (UI; I-20):** la FVM real, sobre un doble nuevo del puerto que registra: `EditAll` llama a `CommitField` si hay texto y después a `TryEditAll`, con la petición de D-PBT4-12.

**Y una nueva:**

| CT | Qué fija | Las líneas | Clase | `ExpectRed` |
|---|---|---|---|---|
| **15** | **I-24:** G-L1 y la pureza sobre los dos archivos nuevos de `Draft/` de PB-T4c: `PushBackFullDraftLaunch.cs` y `PushBackFullDraftStatePort.cs` (§5.0). **Cubre `ApplyToDraft`** | **33:** `g-l1 archivos=2 reglas-sin-coincidencias=True`; las 15 autopruebas de G-L1; `pureza archivos=2 violaciones=[]`; las 15 de la pureza; y `cubre ApplyToDraft=PushBackFullDraftStatePort.cs` | `PushBackDraftLaunchGuardTests` (Core) | **false** |

CT-15 pasa en el RED: el esqueleto de PB-T4c crea los dos archivos (inertes y puros).

**Los totales:** K_C = 2 (PEG:155-163 y CT-PBT1B-15, dos `[Fact]`), con la base C = el cierre de PB-T4b:

| Rama | Core | UI |
|---|---|---|
| **RED** | (C + 11 + 2 − D_C) = (C_P − 2 + 4) `Passed` + 9 `Failed` + (C_S + 2) `Skipped` | + 4, con 4 `Failed` |
| **GREEN** | El mismo total, con 0 `Failed` | + 4, con 0 `Failed` |

- En el RED pasan CT-07, CT-15 y los dos equivalentes.
- D_C = 0. Se confirma en la precondición 6.

### 5.4 La CI del RED

`tests` en failure, `build-ui` en success y `build-plugin` omitido. `ui-tests`: en failure en PB-T4b y PB-T4c; en success en PB-T4a, que no añade pruebas de UI.

### 5.5 Lo que queda para el Smoke-PB (PB-T5, D-PB-09; I-10; M-27)

La lista de la v3:
- el botón abre la ventana modal real;
- el dibujo no se edita con ella abierta (OB-01.4);
- el borrado físico (PB-T3c);
- A2-11 hasta el DWG;
- el coste;
- «Restaurar»;
- OWNER-2;
- **la lectura completa en AutoCAD** (PB-T4a): un rack A/B se abre en el panel (OB-03.2);
- **la relectura con el origen tras borrar la vista de origen** (A-T3-8 con POST reanclado), y su coste: la relectura pura de `AtOrigin` repite las dos lecturas sobre el escaneo ya tomado;
- **la regla única cableada:** los mensajes del campo del panel y de la caja son iguales en AutoCAD;
- **el abridor cableado:** la ventana se abre desde la paleta, sin dueño, con el modal del panel, y nada se escribe tras cerrarla;
- **el paso 3 con un cambio real del dibujo** entre la presentación y el «Sí»;
- **K = 1 con el modal real:** la paleta deshabilitada.

Más los riesgos del borrado real de PB-T3c (F4B-E48, `f4b-dispositions.md:1609`):
- OWNER-1: las referencias de la vista en espacio papel o dentro de bloques del usuario se borran sin aviso;
- OWNER-2: la purga de las anidadas es de un nivel;
- OWNER-3: los bloques dinámicos.

Se observan con «Editar todo» → «Actualizar» sobre racks reales, y junto a la línea «Vistas: se borrará…» de la presentación. Los racks A/B fabricados se rotulan «CONSTRUCTED REPRESENTATIVE TEST CASES».

