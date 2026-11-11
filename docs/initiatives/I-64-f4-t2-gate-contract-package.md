# I-64 — Paquete del contrato de F4-T2 (lectura de borradores y admisión en el host)

**Estado:** v4, lista para emitir. Incorpora la revisión adversarial independiente de la v2 (§9) y la re-revisión de la v3 (§10).
**Base:** rama `architecture/workspace-persistente-rackcad`, en `92b1527c` (F4-T1 CLOSED / PASS sobre `ce73ceb7`).

## 1. Autoridades vinculantes

- **Freeze** (`docs/initiatives/I-64-proposal-v5.md`):
  - decisiones D-04, D-10..D-17, en particular D-11 (admisión), D-12 (StaleBase y «Conservar mi borrador») y D-13, que incluye O-04;
  - invariantes INV-04, INV-06..08, INV-16..22 e INV-25..31.
- **Órdenes del Owner** (`docs/automation/evidence/I-64-pilot/F4-DRAFTS/coordinator/`):
  - `owner-f3-ratification-f4-authorization.md`. Incluye la lista de T2 (`:214-228`) y la regla de ajuste de límites (`:337-338`): «You may adjust task boundaries if the repository evidence shows a cleaner decomposition, but do not collapse mutation into the early pure/core tasks»;
  - `owner-f4-disposition-2.md` (F4-E09): presupuesto por tarea, I-1..I-3 cerrados, N-2, F4-PILOT-PUSHBACK-01, C-F4-03, C-F4-07 y C-F4-06;
  - `owner-f4t1-nc3-substitute.md` (F4-E17): arranque de F4-T2 y nueve requisitos heredados.
- **Aclaraciones:** IC-03, IC-04 (en la redacción aceptada) e IC-06.
- **Contrato cerrado de F4-T1:** `F4-T1-DRAFT-CORE/coordinator/gate-contract-attempt3.json`, en particular INV-F4T1-19, que fija la edición en sitio del estado actual.
- **Paquete de F4 v2** (`docs/initiatives/I-64-f4-gate-contract-package.md`): descomposición, régimen de cierre (`:39`), obligaciones 1-18 y C-F4-01 (b).
- **Disposiciones:** F4-E05, F4-E09, F4-E16 y F4-E17.

## 2. Hallazgos del repositorio (medidos en `92b1527c`)

| Id | Hallazgo | Consecuencia |
|---|---|---|
| T2H-01 | La ventana clásica edita un `PushBackCompositeEditorState`, con dos `PushBackEditorState`, más un `PushBackEditorInputs` que reconstruye desde los controles (`RackPushBackSystemWindow.xaml.cs:56-63`, `:812-857`). Guarda siempre con `PushBackCompositeEditorAssembler.BuildDesign(composite, inputs)` (`:646`) | El estado editable subyacente es el compuesto más los inputs, también para un rack de un solo lado |
| T2H-02 | El ensamblador compuesto escribe siempre `design.Composite = state.BuildComposite()`, que nunca es nulo (`PushBackCompositeEditorAssembler.cs:52-66`) | Un solo lado significa `!design.IsComposite` (`PushBackDesign.cs:70`), no `Composite == null` |
| T2H-03 | `LoadExisting` (`:300-317`) hace en orden: el lado A con `LoadFromDesign(SideAOnly(design))`, luego `LoadCompositeFromDesign` (`Composite.cs:788-907`) y luego `LoadFromModel`. `LoadFromModel` pasa por los controles, que muestran los valores con el formato «0.###» (`NumericField.cs:131`) y sustituyen valores por defecto (`:389-392`), y termina en `Recompute()` (`:426`). `Recompute` vuelve a leer los inputs desde los controles con `ReadInputs()`, que reconstruye `Annotations` **sin `DimensionStyle`** (`:840-848`); `CommitCurrentCell` hace lo mismo con la celda (`:737-741`) | La carga clásica pasa por la UI y pierde información. La materialización del piloto sigue la parte de Application y la diferencia se mide (D-T2-04) |
| T2H-04 | `Snapshot()`/`Restore()` de los dos tipos de estado son copias profundas del contenido editable (`PushBackEditorState.cs:477-505`; `PushBackCompositeEditorState.cs:1117-1160`). `ActiveSelection` no está en el snapshot. No capturan `WorkingBaseline`, que es la **autoridad de módulos**: de ella salen los módulos y las configuraciones por línea (`PushBackEditorState.cs:67-71`; `PushBackEditorDesignAssembler.cs:195-198`, `:315-363`), y es un `PushBackSystem` mutable sin copia profunda. Tampoco capturan `moduleCommit` (intención aceptada), la cola dormida ni campos privados como `topologies` y `sideBEverDeclared` (`PushBackCompositeEditorState.cs:38`, `:44`). Ningún tipo de estado tiene `Equals`, y los inputs no tienen copia | Comparador y copiador nuevos, con un censo de **todos** los campos de instancia. La autoridad de módulos viaja con el borrador mediante el origen inmutable (D-T2-03) |
| T2H-05 | Los dos ensambladores mutan el estado que reciben: el de un lado en `PushBackEditorDesignAssembler.cs:152-159` y `:325`, y el compuesto en `PushBackCompositeEditorAssembler.cs:48`, `:56` y `:172` | Nunca se arma un diseño sobre un estado del borrador (D-T2-03) |
| T2H-06 | `AnnotationScale` vive en `PushBackEditorInputs.Annotations.AnnotationScale`, con JSON `"Structure"."AnnotationScale"` (`DynamicRackSystemDocument.cs:61`). Un valor nulo o ≤ 0 se lee como 1.0 (`:226`, `:321`; `Load.cs:206`). El `NumericField` clásico es obligatorio, con `Minimum="0" MinimumInclusive="False"` (`xaml:68-69`). `NumericFieldValidation` (`RackCad.UI/Controls`, `:76-128`) usa `LocalizedNumberParser` (Application), que rechaza los no finitos. En un rack de un solo lado, la escala solo afecta a cotas, numeración y nombre (`DynamicViewDecorations.cs:52-91`, `:328-329`) | Regla idéntica, con paridad. Smoke-2 necesita un rack con anotaciones o la relectura |
| T2H-07 | `MigrateFirstLevelDatum` (`Load.cs:71`, `:91-124`) y el ensamblador canonicalizan los documentos antiguos: datum (`:383`), `LegacyHighEndBeamPeralte` (`:388`), `DefaultPalletsDeep` por frente (`:411`) y poste por defecto (`:169-171`). Un `SideB` con `IsPresent` en false no se carga (`Composite.cs:845`) ni se vuelve a escribir | Un Actualizar del piloto sobre un dibujo antiguo escribe también esa canonicalización, igual que la ruta de Application de RACKEDITAR |
| T2H-08 | `RackSiblingScan` abre su propio bloqueo y su propia transacción con `Commit` (`RackSiblingScan.cs:102-103`, `:152`), no conserva el texto crudo y no lleva `Section`. `RackEmbedStore.Deserialize(string)` (`RackEmbedDocument.cs`) lee un sobre en Application | Proyección aditiva en T2B. El origen del borrador se deserializa por la ruta existente |
| T2H-09 | `OnBeginDocumentClose` nunca veta. `MarkDraftsPossiblyStale` no tiene consumidor. La destrucción solo se avisa si la sesión era la activa. `DraftClosePolicy` y `RetireSession` no tienen llamador en el producto | Cableado en T2B |
| T2H-10 | La lectura del resumen ocurre dentro de la llamada a `Application.Idle` (F4-E05) | T2C |
| T2H-11 | Guardas vigentes (detalle abajo) | Excepciones por archivo exacto, sin debilitar ninguna regla |
| T2H-12 | El núcleo guarda el `InitialState` de la lectura sin copiarlo (`DraftSession.cs:252`, `:434`) y conserva el argumento de `SetCurrentDraftState` (`:324`). INV-F4T1-19 y sus pruebas (`WorkspaceDraftSessionTests.cs:1000-1054`, `:1128-1141`) fijan que **editar en sitio** `CurrentDraftState` deja el borrador Dirty y veta el cierre | N-2 copia en las entradas y protege el estado inicial. El estado actual **sigue saliendo por referencia** (D-T2-05) |
| T2H-13 | `RackCad.UI.Tests` ya construye la ventana y llama a `LoadExisting` en STA (`BlankFrontSafetyGridTests.cs:294-295`). `LastComputation` es interno y visible para ese proyecto (`xaml.cs:182`; `RackCad.UI.csproj:13-15`) | La diferencia entre la ventana y el piloto se puede caracterizar sin tocar producto |

Guardas vigentes de T2H-11:
- `src/RackCad.Application/Workspace/**` prohíbe `Autodesk`, `System.Windows`, `RackCad.Plugin`, `RackCad.UI`, `Transaction`, `Database` y `Commit(`.
- Los tipos del espacio exacto `RackCad.Application.Workspace` no pueden llamarse `*Editor*` ni `*Draft*`.
- `Drafts/` no puede nombrar sistemas.
- `Plugin/Workspace` tiene exactamente 9 archivos.
- `RackBlockData` solo puede aparecer en `WorkspaceSelectionReader.cs`.
- Fuera de `Plugin/Workspace`, ningún archivo del Plugin puede nombrar `RackCad.Application.Workspace`.
- El censo de comandos es exactamente 39.
- `Dispatcher`, `Task`, `async` y `await` están prohibidos en las carpetas Workspace.

## 3. Descomposición (ajuste de límites, `:337-338`)

| Tarea | Capa | Alcance | Mutación del DWG |
|---|---|---|---|
| **F4-T2A-PILOT-STATE** | Application (pura) y pruebas | Estado del piloto con origen inmutable; comparador y copiador con censo completo; adaptador de texto pendiente de un campo; materialización de un solo lado con resultado tipado; composición para escribir; N-2 en el núcleo; caracterización de la diferencia con la ventana; paridad de validación | Ninguna |
| **F4-T2B-DRAFT-READ-HOST** | Plugin, Application/Workspace y pruebas | Proyección aditiva del barrido (texto crudo y `Section`); una lectura lógica que da StaleBase (una entrada por clave), admisión (D-11, AUTH-13, `RackSiblingMembership`, INV-16 e INV-17, «fuera del piloto») e `InitialDraftState`; cambio de sobre entre la preview y la intención (INV-27); validación exacta de sesión y documento (INV-22); «Comprobar ahora»; recarga perezosa solo por acción del usuario (fuera de la pila del Idle mientras no exista T2C); consumo del marcado de obsolescencia; veto real de `BeginDocumentClose` (M-6); señal de sesión destruida, `RetireSession` y aviso de pérdida; M-9; el host no conserva lo materializado | Ninguna |
| **F4-T2C-IC04-READ-SCHEDULING** | UI, Plugin y pruebas | El drenaje solo marca; la lectura del resumen pasa a una operación separada posterior. **Prohíbe modificar la clausura de dependencias de la ventana** | Ninguna |
| F4-T3-EDIT-UI | UI y Plugin | Como en F4 v2, más `RACKPANELDESCARTAR`, el texto del veto y la regla entre texto y valor (D-T2-01). **Prohíbe modificar la clausura de dependencias de la ventana**; usarla sí está permitido | Ninguna |
| **F4-T1B-PILOT-CHARACTERIZATION** | Pruebas | Como en F4 v2. Se reubica antes de la **primera tarea que modifique cualquier archivo de la clausura de dependencias de la ventana**, y **como muy tarde antes de T4A** | Ninguna |
| F4-T4A / F4-T4B | Como en F4 v2 | La escritura usa `ComposeForWrite` (D-T2-03). La composición no necesita lectura y puede hacerse en el paso 1 de D-13 tal como está declarado (Proposal `:509`) | Solo en T4B |

**Clausura de dependencias de la ventana:**
- `src/RackCad.UI/Systems/PushBack/**`;
- `src/RackCad.UI/Controls/**`, `src/RackCad.UI/Shell/**`, `src/RackCad.UI/Themes/**`, `src/RackCad.UI/Editor/**` y `src/RackCad.UI/Preview/**`;
- los subdiálogos que abre la ventana, como `SafetyTopeGridWindow`;
- los tipos de estado y ensambladores de `src/RackCad.Application/Systems/PushBack/**`;
- las fachadas de escritura de Push Back del Plugin.

Si una ruta de esta lista no existe con ese nombre, el contrato de la tarea la fija por su ruta real antes de emitirse.

**Presupuesto:**
- cada tarea contratada por separado empieza en 1 de 3 (F4-E09), y T2A recibe el arranque de F4-E17;
- la división triplica la capacidad de intentos de T2: se registra (F4-E18) y se avisa al Owner;
- el agotamiento de cualquier subtarea es S-11 y STOP de F4-T2.

**Reubicación de T1B:**
- B1, en la revisión del paquete de F4 v1, pedía la caracterización «antes de tocar el estado del piloto».
- Con IC-06 (b), ninguna tarea anterior a T4A modifica la ventana, los tipos de estado ni su clausura de dependencias. Lo prohíben sus contratos.
- INV-19 pide la caracterización «antes de extraer».
- Se registra como reinterpretación del cumplimiento de B1 en F4-E18.

## 4. Decisiones

### D-T2-01. Campo exacto del piloto (F4-PILOT-PUSHBACK-01)

**Un único campo: `AnnotationScale`.**

| Aspecto | Valor |
|---|---|
| Ubicación en el estado | `PushBackEditorInputs.Annotations.AnnotationScale` (dato del rack) |
| JSON | `"Structure"."AnnotationScale"` del documento de Push Back |
| Clave del campo | `"AnnotationScale"` (ordinal); cualquier otra clave es inválida |
| Validación | La de `AnnotationScaleBox`: vacío o en blanco es inválido; no numérico según `LocalizedNumberParser.TryDouble` es inválido; ≤ 0 es inválido; en otro caso es válido. Paridad exacta con `NumericFieldValidation.Validate(text, false, false, 0, null, minimumInclusive:false)` en varias culturas, al menos `es-MX`, `en-US`, `de-DE` e invariante |
| Regla entre texto y valor | Solo una edición del usuario crea texto pendiente. Mostrar un valor nunca crea texto pendiente. T3 muestra el valor con precisión de ida y vuelta, de modo que comprometer el texto mostrado sin editarlo no cambia el valor y no crea un dirty falso |
| Efecto | Escala de cotas, numeración y nombre; no estructural; no dispara `MustRebuild` |

**Por qué un campo:**
- la orden pide «the smallest coherent group», y un campo ejerce las seis capacidades;
- Smoke-2 pide «edit one pilot field»;
- el compromiso todo o nada lo prueba ya el núcleo de F4-T1.

**Descartados:**
- `RearTopeSaque`: el diálogo compartido convierte en silencio un valor ≤ 0 en el de por defecto (`PushBackRearTopeDialogAdapter.cs:68`);
- `PalletTolerance`: su efecto no es fácil de observar.

**Smoke-2:** usar un rack con anotaciones activas o verificar el resultado por relectura.

### D-T2-02. Alcance del piloto: Push Back de un solo lado (IC-07, punto 1)

- **Se admite** un diseño con `!design.IsComposite` cuya cola dormida quede vacía al materializarse.
- **Fuera del piloto**, con «No editable aquí: fuera del piloto (F5)» y la indicación de usar RACKEDITAR sin lanzarlo (C-F4-07):
  - un A/B;
  - un diseño que aparque cola.

**IC-07, punto 1** registra esto como aclaración escalonada de D-11, bajo IC-06 (requisito 5: lo no soportado es residual de F5), y **se avisa al Owner**. La sesión escribe IC-07 en `I-64-implementation-clarifications.md` antes de emitir T2A.

**Preparación de C-F4-06:** los dibujos de Smoke-2 deben incluir al menos un rack Push Back de un solo lado, admisible y con anotaciones.

### D-T2-03. Estado del piloto, comparador, copiador y composición para escribir

**`PushBackPilotDraftState`** (nombre provisional, `RackCad.Application.Systems.PushBack.Pilot`) contiene:
- **una** instancia de `PushBackCompositeEditorState`;
- **una** de `PushBackEditorInputs`;
- **`Origin`**: el texto authored crudo, una `string` inmutable, del sobre del que se materializó. En el estado inicial es siempre la **base vigente**, porque `Begin`, `DiscardAndReload`, `KeepMineOnCurrentVersion` y el rebase de Applied lo reemplazan con la lectura nueva.

No hay modelo paralelo ni persistencia propia, y el tipo no expone ninguna operación de armado de diseños.

**Clasificación de cada campo de instancia del grafo** (públicos, privados y de respaldo, de forma recursiva), cerrada por un censo por reflexión:

| Clase | Campos | Comparador | Copiador |
|---|---|---|---|
| Editable | Contenido de `Snapshot()` de los dos lados y del compuesto, `topologies`, `sideBEverDeclared` y todos los inputs (`Pallet`, `Annotations`, `SafetySelections`, …) | Lo cubre | Copia profunda |
| Selección | `SelectedFrontIndex`, `SelectedLevelIndex`, `SelectedCells`, `ActiveSide` y `ActiveSelection` | Lo ignora | Lo copia |
| Autoridad de módulos y cola | `WorkingBaseline`, `moduleSession`, `moduleCommit`, `moduleSignature`, `LastModuleReconciliation`, la cola dormida (`DormantCompositeTail`, `DormantTailLineOverrides`) y `dormantTailConsumed` | Lo ignora: no es editable en el piloto y siempre sale de la base (ver abajo); lo registra el residual de F5 | No lo copia. Se rederiva de la base al componer |
| Origen | `Origin` | Lo ignora | Lo comparte (inmutable) |

**Censo:**
- un campo nuevo sin clasificar hace fallar la prueba;
- en el copiador, demuestra que no se comparte ningún objeto mutable.

**`ComposeForWrite(initial, current, catalog)`, pura:**
- materializa de nuevo desde `initial.Origin` (la base vigente), lo que da una autoridad de módulos fresca;
- le transfiere el contenido editable de `current`;
- devuelve el par que consume `PushBackCompositeEditorAssembler.BuildDesign`;
- no muta ninguno de los dos estados;
- es total: una base que ya no admite la materialización da un resultado tipado de rechazo;
- ignora `current.Origin`.

**Semántica (IC-07, punto 2, aclaración de D-12 para el piloto):**
- Lo que el piloto edita sale del borrador.
- Todo lo authored que el piloto **no** edita sale de la **base vigente**. Es análogo a la reconciliación de I-48 que cita D-12 («sobre la versión actual», Proposal `:489-491`).
- Después de «Conservar mi borrador», el estado inicial pasa a la versión nueva, y por tanto la capa no editable también. No se revierte en silencio nada que la superficie no muestra.
- **Consecuencia:** un borrador Limpio compone exactamente la base, así que nunca escribe un cambio authored que la UI no declare.
- La metadata de origen del proyecto (`WithSourceMetadataFrom`, `RackPushBackCommands.cs:456`) también sale de la base. T4A la fija (INV-10).
- La relectura de MUTATE se usa solo para comparar con la StaleBase (D-13). Si no hubo cambios, la base y la relectura coinciden.
- T4 nunca arma un diseño directamente sobre un estado del borrador.

**Prueba obligatoria del escenario de «Conservar»:**
1. se abre V1 y se edita la escala;
2. un V2 externo cambia **solo la capa de módulos** (una cabecera personalizada), con la misma escala que V1;
3. el usuario elige «Conservar mi borrador»: el estado inicial pasa a V2;
4. la composición lleva la cabecera de V2 y la escala del borrador;
5. al devolver la escala a su valor original, el borrador queda Limpio y la composición arma un documento idéntico al de V2.

**Residual de F5:** cuando los módulos sean editables, el comparador y el copiador incorporarán la autoridad de módulos.

### D-T2-04. Materialización de un solo lado

`Materialize(originText, catalog)` devuelve un resultado tipado: Admitido (estado), Fuera del piloto (motivo) o Fallido (error de lectura o del resolver, sin excepción hacia fuera).

**Pasos:**
1. Deserializa por la ruta existente de RACKEDITAR (`RackPushBackCommands.cs:154`, `:162-169`), sin parser propio:
   - `RackEmbedStore.Deserialize(originText)`; si devuelve null → Fallido;
   - comprobar que el kind es Push Back; si no → Fallido;
   - `RackProjectStore.TryDeserialize(embed.Design, …)` → `PushBackDesign` (`RackProjectStore.cs:128-148`). Una versión mayor incompatible → Fallido con el diagnóstico de INV-17; cualquier otro fallo → Fallido.
2. Reproduce **la parte de Application** de `LoadExisting`, en el mismo orden:
   - `SetActiveSide(A)` y `SetSideBPresent(false)`;
   - `inputs = SideA.LoadFromDesign(design, resolver)`;
   - `SetSideBPresent(false)` y `LoadComposite(design.Composite)`;
   - `ClearDormantTail()` y `ParkDormantTail` con los ids de cola (si queda cola, el resultado es Fuera del piloto);
   - `AlignSlotGrid()`.
3. **No** reproduce `LoadFromModel` ni el primer `Recompute`.

**Divergencias conocidas con la ventana**, que caracteriza una prueba:
- (a) el redondeo de valores al formato «0.###» de los controles;
- (b) la pérdida de `DimensionStyle` en `ReadInputs`;
- (c) toda sustitución o ajuste que hacen los controles en `LoadFromModel` (`:382-419`), por ejemplo los valores por defecto, la unidad de peso, el ajuste de cotas o las vistas de cota;
- (d) la celda vuelta a comprometer desde los controles.

Las cuatro son pérdidas de la ruta de UI clásica y se registran en `docs/ideas-futuras.md`. El piloto conserva el valor authored.

**Pruebas obligatorias:**
- **Ida y vuelta canónica (identidad):** sobre diseños canónicos, armados con el ensamblador compuesto del producto, materializar, componer y armar da un documento persistido idéntico. Con una edición de `AnnotationScale`, la única diferencia es ese campo.
  - El corpus tiene al menos 6 diseños: módulos personalizados, fondos manuales, frentes en blanco, `OffCells` del tope trasero, selecciones de seguridad, parámetros avanzados, `AnnotationScale` no predeterminada y `DimensionStyle` no nulo.
- **Ida y vuelta de dibujos antiguos (canonicalización enumerada):** fixtures JSON sin datum, sin los campos de I-41, con poste nulo, con `SideB` no presente y con un peralte heredado distinto de 3.5.
  - La prueba afirma el delta exacto de canonicalización (T2H-07) más el campo editado.
  - Smoke-2 lo recoge entre los resultados esperados: el Actualizar del panel canonicaliza como la ruta de Application.
- **Caracterización ventana frente a piloto** (`RackCad.UI.Tests`, STA con `StaTestRunner`, sin tocar producto): para cada diseño del corpus, canónico y antiguo, `LoadExisting` más `LastComputation.Design` frente a `ComposeForWrite` más `BuildDesign`. Fija el delta por diseño.
  - Los dos lados parten del **mismo texto de sobre** y del **mismo catálogo** (`window.Session.Catalog`).
  - La prueba exige `CurrentInputsAreValid` antes de leer `LastComputation`. Si la ventana rechaza los valores, el caso se registra como «la ventana no admite», sin comparar.
  - El corpus es **uno solo**, en fixtures JSON de sobre en `tests/RackCad.Tests/Fixtures/I64/`. Los dos proyectos los leen por ruta desde la raíz del repositorio, como hace `BoundedEditorCharacterizationTests.cs:96-100`, porque `RackCad.UI.Tests` no referencia `RackCad.Tests`.
  - Un delta fuera de las clases (a)-(d) es una **parada**: el Worker lo informa y no lo fija.

### D-T2-05. N-2: frontera de propiedad del núcleo (respeta INV-F4T1-19)

**Entradas: el núcleo copia**
- el `InitialState` de la lectura en `Begin`, `DiscardAndReload`, `KeepMineOnCurrentVersion` y el rebase de Applied y AppliedWithWarnings;
- el argumento de `SetCurrentDraftState`.

**Salidas:**
- `InitialDraftState` devuelve una **copia**.
- `CurrentDraftState` **sigue devolviendo la referencia interna**: es el modelo de edición en sitio de INV-F4T1-19, que no se enmienda.
- `IsDirty`, `Status`, `RacksAtRiskOf` y la política de cierre leen los campos privados, **nunca** el getter que copia.
- **Resultado de `adapter.Apply` en `CrossBoundary`** (`DraftSession.cs:375-376`): no se copia, para no romper la identidad del estado actual. El contrato del adaptador del piloto fija, con una prueba, que devuelve el mismo `current` (mutado en la fase 2) y que no conserva ninguna referencia a estados: no tiene campos de instancia de tipo estado.

**Pruebas adversariales.** Cada una falla en el RED y pasa en el GREEN, con el `TState` mutable de prueba y con `PushBackPilotDraftState`:
- (a) mutar el origen de la lectura tras `Begin` no cambia ni el estado inicial ni dirty;
- (b) lo mismo tras `DiscardAndReload`, `KeepMineOnCurrentVersion` y el rebase de Applied;
- (c) mutar el objeto pasado a `SetCurrentDraftState` tras la llamada no cambia el estado actual;
- (d) mutar el objeto devuelto por `InitialDraftState` no cambia el estado inicial interno;
- (e) reutilizar un mismo origen para dos borradores los deja independientes.

**Prueba de conservación:** con `PushBackPilotDraftState`, editar en sitio `CurrentDraftState` deja Dirty y veta, igual que INV-F4T1-19.

**Cambio en pruebas de F4-T1:** solo las aserciones de identidad sobre el estado inicial (`Assert.Same`) pasan a no-identidad más igualdad estructural completa. No se usa el comparador de dirty, que ignora `Source` (`WorkspaceDraftSessionTests.cs:72-82`). Las pruebas de edición en sitio no cambian. Se listan en el RED.

### D-T2-06. Mecanismo de IC-04 (T2C)

- **Drenaje:** marca y no lee.
- **Lectura:** en una operación separada posterior al retorno del callback.
  - La programa una sola vez la UI, agrupada, con una excepción de guarda por archivo exacto para el planificador.
  - O la pide el usuario.
- **Condiciones normales de contexto** en cualquier caso.

### D-T2-07. `RACKPANELDESCARTAR` y el texto del veto

Van a T3. T2B entrega la decisión del veto y el aviso de pérdida, sin texto de UI.

## 5. Obligaciones

**Heredadas (F4-E17):**

| N.º | Obligación | Tarea |
|---|---|---|
| R1 | N-2 anti-alias | T2A (D-T2-05) |
| R2 | El host no puede mutar `InitialDraftState` | T2A (núcleo) y T2B (el host no conserva lo materializado) |
| R3 | Prueba adversarial de mutación del origen | T2A, (a)-(e) |
| R4 | Una lectura lógica da StaleBase, admisión e `InitialDraftState` | T2B (un mismo texto crudo alimenta la StaleBase y `Materialize`) |
| R5 | Admisión con `RackSiblingMembership` | T2B |
| R6 | Sin escritura en el DWG | T2A, T2B, T2C |
| R7 | IC-04 | T2B y T2C |
| R8 | F4-PILOT-PUSHBACK-01 | T2A y T2B |
| R9 | Campo exacto | D-T2-01 |

**Arrastres:**

| Arrastre | Tarea |
|---|---|
| M-6 | T2B |
| M-9 | T2B; `Materialize` nunca recibe texto de la StaleBase |
| Copiador profundo | T2A |
| Una entrada por clave | T2B |
| m-1 y m-2 | Siguen pendientes para la próxima tarea que **necesite** tocar la guarda; T2A no la toca |
| La UI no usa borradores desligados | T3 |
| `CompleteApply` en `finally` y avisos de POST | T4B |

**Obligaciones de F4 v2 asignadas:**

| N.º | Obligación | Tarea |
|---|---|---|
| 1 | Dirty no es JSON crudo | T2A |
| 3 | Una preview no crea `DraftSession` | T2B |
| 4 | Una lectura lógica | T2B |
| 5 | Ninguna escritura antes de Actualizar | T2A, T2B, T2C |
| 10 | La StaleBase solo detecta cambios | T2B |
| 11 | La comprobación relee datos authored | T2B |
| 13 | La sesión A nunca escribe en B | T2B (INV-22, en la lectura) |
| 14 | Destruir el documento retira sesión y borradores | T2B |
| 15 | Dirty o pendiente participan en el veto | T2B (el texto vivo, en T3) |
| 18 | Todo objeto y transacción muere dentro | T2B y T2C |

## 6. Contrato propuesto de F4-T2A-PILOT-STATE

**Régimen** (F4 v2, `:39`):
- RED y GREEN exactos. El RED **compila** y falla **por aserción**: es un esqueleto inerte de los tipos nuevos, que devuelve valores por defecto sin lanzar, como el RED `a362744c` de F4-T1, porque el GREEN no puede tocar ningún archivo de `tests/` del RED (16.8);
- CI del SHA exacto;
- verificación del Controller;
- revisión directa de la sesión;
- revisión adversarial independiente;
- **nc4** antes de aceptar la primera delegación, porque T2A abre una cadena nueva (`agent-execution/README.md:232`; precedente `F4-T1-DRAFT-CORE-nc4/`), y nc1..nc3 tras la verificación.
- El contrato lista `ExpectRed` con las pruebas de los dos proyectos.
- El prompt del Worker prohíbe los términos de 16.10 también en nombres de archivo, rutas y artefactos.

**Escritura permitida:**
- archivos nuevos en `src/RackCad.Application/Systems/PushBack/Pilot/`, con los nombres exactos que fija el contrato;
- `src/RackCad.Application/Workspace/Drafts/DraftSession.cs` y `DraftRegistry.cs`;
- archivos nuevos `tests/RackCad.Tests/Workspace/WorkspaceDraftPilot*Tests.cs` (nombres exactos en el contrato) y fixtures JSON nuevos bajo el prefijo `tests/RackCad.Tests/Fixtures/I64/`;
- en `WorkspaceDraftSessionTests.cs`, `WorkspaceDraftRegistryTests.cs` y `WorkspaceDraftApplyTests.cs`, solo las aserciones de identidad de D-T2-05;
- archivos nuevos `tests/RackCad.UI.Tests/Workspace/WorkspaceDraftPilotValidationParityTests.cs` y `tests/RackCad.UI.Tests/Workspace/WorkspaceDraftPilotWindowLoadCharacterizationTests.cs`.

La sesión, no el Worker, registra las divergencias (a)-(d) en `docs/ideas-futuras.md` durante la custodia.

**Prohibido:**
- `src/RackCad.UI`, `src/RackCad.Plugin` y `src/RackCad.Domain`;
- los archivos existentes de `src/RackCad.Application/**`, salvo los dos del núcleo;
- `WorkspaceDraftBoundaryTests.cs` y cualquier otra guarda.

**Invariantes:**
- **I-A1:** el estado del piloto agrupa las instancias existentes y el origen inmutable, y no persiste nada.
- **I-A2:** el comparador es reflexivo y simétrico. Da igualdad entre materializaciones del mismo origen y de orígenes equivalentes (INV-26). Detecta cada campo editable (censo completo) e ignora las otras tres clases.
- **I-A3:** el copiador devuelve una copia igual según el comparador y con la misma selección. No comparte ningún objeto mutable (censo completo) y no copia la autoridad de módulos.
- **I-A4:** el adaptador cumple D-T2-01. La fase 1 no muta.
- **I-A5:** la materialización y la composición cumplen D-T2-02 a D-T2-04: las pruebas de ida y vuelta, la caracterización y la prueba del escenario de «Conservar» (IC-07, punto 2).
- **I-A6:** N-2 según D-T2-05, incluida la prueba de conservación de INV-F4T1-19.
- **I-A7:** ninguna guarda se modifica ni se debilita, y el núcleo sigue sin nombrar sistemas.
- **I-A8:** ningún archivo de producto de T2A referencia Plugin, UI, `Autodesk`, IO ni `Transaction`.

**Pruebas:**
- `FullyQualifiedName~RackCad.Tests.Workspace`: mínimo 1300 (línea base de la CI de `92b1527c`) más las nuevas;
- `FullyQualifiedName~RackCad.UI.Tests.Workspace`: mínimo 222 (misma línea base) más las nuevas;
- las suites completas de los dos proyectos, en verde en la CI del SHA exacto.

**Paradas:**
- el comparador o el copiador exigen modificar los tipos existentes;
- la ida y vuelta canónica no da identidad;
- aparece un delta de caracterización fuera de (a)-(d);
- haría falta tocar la ventana o una guarda.

## 7. Esbozo de F4-T2B y F4-T2C

Se contratan tras cerrar T2A. Sus paquetes fijarán:
- los archivos exactos;
- las excepciones de guarda por archivo exacto: número de archivos de `Plugin/Workspace`, `RackBlockData` y planificador de T2C;
- la proyección aditiva sin cambiar a los consumidores;
- la elección del miembro canónico cuyo texto es `Origin`;
- la señal de sesión destruida;
- la validación exacta de sesión y documento;
- la detección del cambio de sobre entre la preview y la intención (INV-27).

## 8. Puntos que se pidieron a la re-revisión de la v3 (respondidos en §10)

1. El origen inmutable y la semántica de escritura «el borrador entero» después de «Conservar mi borrador» (D-T2-03), frente a D-12 y D-13.
2. ¿Cubre la clasificación de campos todo el grafo? ¿Es correcta la clase «Autoridad de módulos»?
3. ¿Es implementable y verificable la caracterización ventana frente a piloto dentro de T2A?
4. ¿Hay algún residuo de B-1 en D-T2-05?

## 9. Disposición de la revisión adversarial de la v2

| Hallazgo | Disposición |
|---|---|
| B-1. Los getters con copia invierten INV-F4T1-19 | **Aceptado.** Copia en las entradas; solo `InitialDraftState` sale como copia; los campos internos se leen sin getters; prueba de conservación (D-T2-05) |
| I-1. La materialización no reproduce `LoadExisting` | **Aceptado.** D-T2-04 reescrito: parte de Application, divergencias (a)-(d) enumeradas y caracterización en `RackCad.UI.Tests` |
| I-2. Canonicalización de dibujos antiguos | **Aceptado.** Identidad solo para los canónicos; fixtures antiguos con delta enumerado; expectativa de Smoke-2 |
| I-3. `WorkingBaseline` no es una caché | **Aceptado.** Reclasificado como autoridad de módulos; residual de F5 |
| I-4. «Conservar mi borrador» produce un híbrido | En la v3, origen inmutable y escritura del borrador entero. **Sustituido en la v4 por R-2 (§10):** la capa no editable sale de la base vigente, con una prueba |
| I-5. El censo solo veía propiedades públicas | **Aceptado.** Censo de todos los campos de instancia con cuatro clases |
| I-6. El disparador de T1B era incompleto | **Aceptado.** Clausura de dependencias y prohibición en T3; F4-E18 |
| I-7. Faltaban responsabilidades de T2B | **Aceptado.** INV-27, INV-22, INV-16 e INV-17, y recarga perezosa solo por acción del usuario |
| I-8. La exclusión de A/B extiende D-11 | **Aceptado.** IC-07; aviso al Owner; requisito para los dibujos de Smoke-2 |
| M-1 | **Aceptado.** Composición en PREPARE desde el origen, sin relectura de MUTATE; total y sin mutar |
| M-2 | **Aceptado.** m-1 y m-2 salen de T2A |
| M-3 | **Aceptado.** Tabla de asignación en §5 |
| M-4 | **Aceptado.** Citas corregidas |
| M-5 | **Aceptado.** F4-E18 y aviso al Owner |
| M-6 | **Aceptado.** El RED compila con stubs |
| M-7 | **Aceptado.** Regla entre texto y valor; observabilidad en Smoke-2 |
| M-8 | **Aceptado.** Paridad en varias culturas |
| M-9 | **Aceptado.** Resultado tipado |
| M-10 | **Aceptado.** Régimen de cierre en §6 |

## 10. Disposición de la re-revisión de la v3

Veredicto de la re-revisión: EMITIBLE TRAS CORRECCIONES, sin necesidad de volver al Owner salvo los avisos de IC-07 y F4-E18.

| Hallazgo | Disposición |
|---|---|
| R-1. Un borrador Limpio podía escribir cambios de módulos | **Resuelto por R-2.** La capa no editable sale siempre de la base vigente, así que un borrador Limpio compone exactamente la base. Lo comprueba la prueba del escenario de «Conservar» |
| R-2. La semántica de «Conservar» necesita una IC de D-12 | **Aceptado, opción (b):** lo que el piloto no edita sale de la base vigente, análogo a I-48 (IC-07, punto 2). `ComposeForWrite(initial, current, catalog)` materializa desde `initial.Origin` |
| R-3. nc4 sí aplica | **Aceptado.** §6 |
| M-a. Resultado de `adapter.Apply` | **Aceptado:** no se copia, para conservar la identidad del estado actual; el adaptador del piloto devuelve `current` y no conserva nada, fijado con prueba (D-T2-05) |
| M-b. Precisiones de la caracterización | **Aceptado:** mismo origen y catálogo, `CurrentInputsAreValid`, corpus único leído desde la raíz y clase (c) ampliada a `:382-419` (D-T2-04) |
| M-c. `dormantTailConsumed` sin clasificar | **Aceptado:** va en la clase «Autoridad de módulos y cola» |
| M-d. T2C y la clausura por rutas | **Aceptado:** T2C la prohíbe; clausura listada por rutas (§3) |
| M-e. Cita de O-04 | **Aceptado:** la composición no necesita lectura y puede ir en el paso 1 de D-13 (`:509`) |
| M-f. Ruta exacta de deserialización | **Aceptado:** D-T2-04, paso 1 |
| Precisión de M-4 (efecto de la escala) | **Aceptado:** T2H-06 cita `DynamicViewDecorations.cs` |
| Corrección del revisor sobre su I-4 de la v2 | **Tomada en cuenta:** D-12 prevé que lo authored que no está en el borrador salga de la versión nueva. Es la opción (b) |
