> **Redacción para el repo público:** las rutas locales se sustituyen por marcadores (`<checkout-principal>`, `<carpeta-del-paquete>`, `<datos-locales-del-usuario>`). El resto no cambia.

# I-64 F4B — Paquete del contrato de PB-T2 (aplicar, cerrar y escribir separados; los dos modos), v2.3

**Estado:** v2.3, el texto de emisión de PB-T2b y PB-T2c. PB-T2a ya está emitida con la v2.2 (abajo). Antes de emitir cada una se vuelve a medir sobre el cierre de la anterior (§6.0, «Anclajes»). No declara GATE PASS.
- **PB-T2a, emitida el 2026-11-19** con attempts 1 de 3 (F4B-E32), por la autorización condicional de F4B-E28, pregunta 4, tras su comprobación acotada (EMITIBLE TRAS CORRECCIONES 0/0/6, aplicada) y la remedición sobre el cierre de PB-T1c (`5a8366c1`). **El contrato manda:** `I-64-pilot/F4B-PBT2A-DRAFT-STATE-PORT/coordinator/gate-contract.json`.
- **v1:** revisión adversarial independiente (`F4B-DRAFTS/pbt2/independent-review-v1.md`, citada **IR1**): **NECESITA v2, 0/8/16**. Las cuatro preguntas al Owner son reales y ninguna estaba decidida; ninguna decisión material nueva.
- **v2:** aplica los 24 hallazgos de IR1 y las lecciones de PB-T1b que pidió la sesión (registro v1 → v2, abajo). El detalle de cada hallazgo y su cambio está en `v2-delta.md`.
- **v2.1:** la recomprobación de la v2 (IR1 §10, citada **IR1-R**) da **LISTO PARA EMITIR TRAS REMEDICIÓN, 0/0/7**. La v2.1 aplica sus siete menores, N-01 a N-07 (registro v2 → v2.1, abajo; el detalle está en `v21-delta.md`), y el matiz de N-04 en Q-PBT2-1 con el texto que dio la sesión.
- **v2.2:** la comprobación acotada de PB-T2a (`F4B-DRAFTS/pbt2/independent-review-pbt2a-v21-check.md`, citada **CA-T2a**) da **EMITIBLE TRAS CORRECCIONES, 0/0/6**, sin decisiones nuevas para el Owner. La v2.2 cambia solo la regla de PBT2x-C11 (M-4) y la custodia de `v22-delta.md` (registro v2.1 → v2.2, abajo; el detalle está en `v22-delta.md`). M-1 a M-3 y M-5 se aplican en el generador de PB-T2a, sin cambiar este texto; M-6 va al paquete antes de emitir PB-T2b.
- **v2.3:** prepara la emisión de PB-T2b sobre la ventana real de PB-T1c, cerrada (F4B-E31; W y WC, el GREEN `161a0dad`), con PB-T2a emitida (F4B-E32; la punta `0dc623ca`). Aplica M-6 de CA-T2a (la disposición del configurador en el perfil y el desfase de una pasada del diseño del compuesto), remide las anclas [T1c] de W y WC y fija los rangos de INV-PBT2B-03 (registro v2.2 → v2.3, abajo; el detalle está en `v23-delta.md`). El diseño no cambia.

**Lo que pide el Owner** ([OD], fila de PB-T2 del §5 de [P], literal):

> ACCEPT IN PRINCIPLE. Separate: "apply edits to current editing state" from: "close editor" and from: "write authored DWG state". Workspace mode must return edited state to DraftSession. Classic RACKEDITAR may continue its compatible existing outer workflow. Avoid two independent implementations of field validation.

**El contenido que [P] asigna a PB-T2** (§5, `:504`):
- separar «aplicar al estado», «cerrar» y «escribir»;
- el modo explícito (§4 de [P]), con la guarda de «sin camino clásico»;
- el dueño de los subdiálogos lo decide el host;
- una sola validación de campos;
- la prueba UI-STA de OB-01.3;
- el paso 4 de D-PB-06 en WORKSPACE, que PB-TC dejó pendiente (OB-06.3).

**Lo que añaden las decisiones posteriores:**
- **F4B-E23** (D-PB-07, «BOM y biblioteca (Recomendado)»): dos costuras de prueba nuevas y cuatro comprobaciones en la UI-STA de OB-01.3;
- **F4B-E06 y F4B-E09:** el aviso de D-PB-08 en WORKSPACE;
- **D2** (F4B-E11): las condiciones 4 y 10 y el límite, arrastrados por el §10 de [PT1];
- **F4B-E22 y F4B-E26:** los menores de producto M09, M11, M12 y M15 de PB-T1b;
- **F4B-E25:** la regla de F4B sobre las rutas `UNIT_CHANGE` en una corrección.

**Lo que propone este paquete:**
- **Tres subtareas secuenciales**, cada una con su cadena y 1/3 (OB-10.1), sin rebase entre ellas y con delegaciones sucesivas (R-PB-09):
  - **PB-T2a — el puerto al borrador** (`F4B-PBT2A-DRAFT-STATE-PORT`): Application, sin la ventana;
  - **PB-T2b — la ventana en dos modos** (`F4B-PBT2B-WORKSPACE-MODE-WINDOW`): W y un archivo nuevo de UI;
  - **PB-T2c — las obligaciones heredadas en WORKSPACE** (`F4B-PBT2C-WORKSPACE-CARRYOVER`): solo pruebas, con RED por mutantes (v2, V2-21).
- **CLASSIC no cambia:** los constructores, los cuatro sitios, WX, WC, el Plugin y los literales de T1B quedan igual.
- **WORKSPACE** se obtiene solo con la factoría tipada `RackPushBackSystemWindow.ForDraft(PushBackDraftEditorLaunch)`:
  - carga una **copia** del estado del borrador, sin pasar por un diseño;
  - «Aplicar al borrador» aplica al estado de edición con **el mismo paso** que «Actualizar» (extraído de `RequestDraw` sin cambiar su comportamiento) y devuelve una copia por el puerto;
  - no cierra la ventana ni escribe en el DWG.
- **Cuatro decisiones nuevas para el Owner** (§8.2): Q-PBT2-1 a Q-PBT2-4, listas para AskUserQuestion. Se le **informa sin preguntar** de N-PBT2-01 y de los once literales (§8.4).

**Condiciones para emitir** (§6.0):
1. PB-T1c cerrada (F4B-E31, sobre `161a0dad`; v2.3, V23-05);
2. las respuestas del Owner a Q-PBT2-1 a Q-PBT2-4;
3. la remedición sobre el cierre de PB-T1c, sin cambio de una premisa material (L-6 de [PT1]), **con la comprobación de que ninguna guarda de PB-T1c impide reasignar `editable` en un constructor** (v2, V2-05);
4. la comprobación acotada de cada subtarea (la de la v2 ya está hecha: IR1-R);
5. las celdas vigentes, el `config.toml` igual a la Snapshot J (F4B-E19) y el binario acreditado;
6. la custodia.

**Base de la medición:**
- **La v1** midió `2f5d3dec`.
- **La v2** mide la punta de la rama `95373af6`, el cierre de PB-T1b (F4B-E26): `git diff --name-only 2f5d3dec 95373af6 -- src` y `git diff --name-only 2f5d3dec a18ba345 -- src` vacíos; solo cambian cinco archivos de prueba de PB-T1b (registro Q-1).
- **La v2.1** (IR1-R N-06) mide la punta nueva `833ea6bf`: PB-T1c **emitida** (F4B-E27), sin cerrar. `git diff --name-only 95373af6 833ea6bf -- src tests` sale vacío; entre las dos solo cambian documentos de PB-T1c, entre ellos el paquete de PB-T1 v2.8 y su contrato emitido (registro Q-7).
- **La v2.3** (V23-03) remide sobre `0dc623ca`, con PB-T1c cerrada (F4B-E31) y PB-T2a emitida (F4B-E32): W y WC son el GREEN de PB-T1c `161a0dad`, y `git diff --name-only 161a0dad 0dc623ca -- src tests` sale vacío. Las anclas de W y WC del cuerpo son ya las de `0dc623ca`.
- `main` sigue en `812baf87`. Todo es en solo lectura y por objetos git desde el checkout principal. El detalle está en `pbt2-measurements.md`: apartados A a P (la v1, con las correcciones de IR1) y **Q** (la v2 y la v2.1).
- **Marca [T1c]:** las anclas de W, WC o WX que dependían de la ventana de PB-T1c. Hasta la v2.2 eran las de `b5c09f0b`, `2f5d3dec`, `95373af6` y `833ea6bf`; **desde la v2.3 (V23-03) son las de `0dc623ca`**, el cierre de PB-T1c, con D-PBT1-13 aplicada. Los registros de cambios conservan las de entonces.

## Registro de cambios v2.2 → v2.3

**Origen:** M-6 de CA-T2a, que su informe dejó «para el paquete antes de emitir PB-T2b», y el mensaje de la sesión: la ventana real de PB-T1c (cerrada, F4B-E31) y PB-T2a emitida (F4B-E32). **Nada cambia el diseño**: las anclas se remiden, M-6 fija una regla de atribución y el momento de dos comparaciones.

| Id | Origen | Qué cambia | Dónde |
|---|---|---|---|
| V23-01 | **CA-T2a M-6 (a)**; F4B-E30 y F4B-E31 | **La disposición del configurador en el perfil.** Pruebas que ya existían crean un `RackFrameConfiguratorWindow` real, y su `OnClosed` escribe `RackCad/RackFrameConfigurator.layout` en los datos locales del usuario. En los filtros de PB-T2b, son las del filtro `RichEditor` (`RichEditorCharacterizationTests`, `RichEditorCloseContractTests` y `RichEditorContractTests`, medidas en `0dc623ca`) y, según el análisis de la corrección de PB-T1c, las del filtro `WindowCensusGuardTests`. El invariante común 10 gana la regla de atribución de esa corrección: la prueba de limpieza del perfil se registra; si solo cambia ese archivo, se atribuye a esos filtros, sin parar; cualquier otro cambio del perfil es parada. C09 rige las pruebas nuevas, que no abren el configurador real (INV-08) | §6.0 (invariante 10 y las propias comunes); R-PBT2-08 |
| V23-02 | **CA-T2a M-6 (b)**; F4B-E30, F4B-E31 (OWNER-01) | **El desfase de una pasada.** Tras `GapBox`, el separador central o la estructura propia, el diseño de `LastComputation` del compuesto va una pasada por detrás del estado (ya en la base; C-1 de la corrección de PB-T1c). CT-PBT2B-02 toma sus líneas con `diseno-igual` tras un recálculo neutro (`Recalcular(w)`, copiado de CT-PBT1C-02, receta nueva del §5.0); CT-PBT2C-03 las toma tras «Aplicar», que recalcula antes (`ApplyEditsToEditingState`); CT-PBT2B-04 ya lo hacía. «Restaurar» no recalcula antes de leer el diseño, en los dos modos: es L-PBT2-13, sin corrección (CLASSIC no cambia) | §5.0; CT-PBT2B-02; CT-PBT2C-03; §8.3 |
| V23-03 | La sesión: la ventana real de PB-T1c | **Las anclas [T1c] de W y WC** del cuerpo del paquete pasan a `0dc623ca`, donde W y WC son el GREEN de PB-T1c `161a0dad`: de 64 citas distintas, 58 se movieron, 5 siguen igual y una cambió de sitio, `UpdateCompositeEnabled()` (de WC:933, en el `LoadCompositeFromDesign` que PB-T1c retiró, a W:314, en `LoadExisting`). WX no cambia. Los registros de cambios conservan las líneas de entonces. La tabla, cita por cita, está en `v23-delta.md` y en la salida de `measure_pbt2b_t1c.py` | Base de la medición; §2 a §10 |
| V23-04 | La sesión; la nota de INV-PBT2B-03 (la lección M10) | **Los rangos de INV-PBT2B-03,** medidos sobre `0dc623ca` con el método de INV-PBT1C-03 (desde la documentación del miembro hasta su final), van en el invariante. El generador los vuelve a comprobar en la emisión | §6.2 |
| V23-05 | La sesión | **El estado de PB-T1c y de PB-T2a:** PB-T1c cerrada (F4B-E31) y la reasignación de `editable` comprobada en los anclajes de PB-T2a (F4B-E32), en la cabecera, HT2-24, HT2-25, el §4, R-PBT2-09 y el §9 | Cabecera; §2.6; §4; §7; §9 |
| V23-06 | La sesión | La custodia de PB-T2b gana esta v2.3, `v23-delta.md` y `measure_pbt2b_t1c.py` | §6.0, «Custodia» |
| V23-07 | La redacción del generador de PB-T2b | **Dos invariantes con el mismo número:** el propio «INV-PBT2B-08» (la única escritura en disco es `ArchivoTemporal`) chocaba con el común 08 (las pruebas de UI), que el contrato numera también INV-PBT2B-08 y que ya lo dice. El propio pasa a **INV-PBT2B-08b**, como 07b y 07c en PB-T1c. «INV-08» del §5.0 y del §7 sigue siendo el común | §6.2 |

## Registro de cambios v2.1 → v2.2

**Origen:** CA-T2a (EMITIBLE TRAS CORRECCIONES, 0/0/6) y el mensaje de la sesión. **Nada cambia el diseño**, ni el contrato de PB-T2a fuera de PBT2A-C11.

| Id | Origen | Qué cambia | Dónde |
|---|---|---|---|
| V22-01 | **CA-T2a M-4** | PBT2x-C11 se copia de [PT1] §6.0 cambiando el prefijo y, además, su referencia «section 6.0 of the package»: en un contrato de PB-T2, «the package» sería este, y su §6.0 no lista las diferencias admitidas de cada firma, que están en [PT1] §6.0. Pasa a «section 6.0 of the PB-T1 contract package ([PT1], docs/initiatives/I-64-f4b-pbt1-gate-contract-package.md)». La regla no cambia | §6.0 |
| V22-02 | La sesión | `v22-delta.md` se custodia con la emisión de PB-T2a, junto a `v2-delta.md` y `v21-delta.md` | §6.0, «Custodia» |
| V22-03 | CA-T2a M-1 a M-3, M-5 y D-1 a D-14 | Sin cambio en este texto: se aplican en el generador de PB-T2a (la línea base de la CI atada al cierre de PB-T1c, la regla de D-13 para la declaración de `editable`, las capturas del estado y los nombres de PBT2B-C13 en V2-05, y la sección de F4B-E25) | — |
| V22-04 | CA-T2a M-6 | Sin cambio en la v2.2: los dos hechos de F4B-E30 (la escritura de `RackFrameConfigurator.layout` en el perfil por filtros de UI de PB-T2b, y el diseño del compuesto una pasada por detrás) van al paquete antes de emitir PB-T2b | — |

## Registro de cambios v2 → v2.1

**Origen:** IR1-R (LISTO PARA EMITIR TRAS REMEDICIÓN, 0/0/7) y el mensaje de la sesión (el texto de N-04 y la HEAD `833ea6bf`). **Nada cambia el diseño**: las siete son precisiones del texto y de las líneas esperadas.

| Id | Origen | Qué cambia | Dónde |
|---|---|---|---|
| V21-01 | **IR1-R N-01** | **El resto de I-07 por «Guardar en biblioteca»:** `SaveLibrary_Click` conserva `SetName(NameBox.Text?.Trim())` (W:3504, un literal de CT-22; PB-T1c le añade `SyncName()`), así que guardar en la biblioteca normaliza un nombre nulo o con espacios del estado de la ventana. Se declara como **L-PBT2-12** y se informa al Owner (§8.4). CT-PBT2B-04 gana las filas `nombre-nulo-biblioteca` y `nombre-con-espacios-biblioteca`: biblioteca, «Cerrar» con «No» (`avisos=1`) y «Aplicar» (`llamadas=1`). **Variante:** la costura devuelve `ArchivoTemporal` y no `null`, porque `if (path == null) return;` (W:3499) va antes de `SetName`, y con `null` no se normaliza nada. D-PBT2-05, D-PBT2-06 (2) y el §8.4 dicen la excepción | D-PBT2-05; D-PBT2-06; CT-PBT2B-04; §8.3; §8.4 |
| V21-02 | **IR1-R N-02** | **Las líneas esperadas, enteras:** las tres pasadas de CT-PBT2B-03 pasan a una lista literal, sin «…». **El sufijo ` descartes=<n>`** queda fijado por una regla normativa en el §5.0: la última línea de cada ventana se forma tras el `Cerrar` explícito del final del `try`, como en `PushBackDeclaredCorrection2MixedBoxTests.cs:94-96`. `n` es el número de avisos de descarte de esa ventana, y con Q-PBT2-1 (A) vale 1 si al cerrar queda algo sin aplicar o un módulo escenificado. Cada CT dice el valor de cada ventana | §5.0; CT-PBT2B-01 a 05 y 07; CT-PBT2C-01 a 03 |
| V21-03 | **IR1-R N-03** | **La pasada 2 de CT-PBT2B-03 usa el fixture de cada original:** `pb-31-v2-modulos` con el módulo `M3` (CT-PB0-12, confirmado y nulo), `pb-06-seguridad` (CT-PB0-13 (i) y CT-PB0-15) y `pb-05-topes` (CT-PB0-16). Va una ventana por gesto, con los ayudantes privados de los originales copiados | CT-PBT2B-03 |
| V21-04 | **IR1-R N-04** y la sesión | Q-PBT2-1: «el primer aviso sobre todo lo editado; hoy solo avisa de un módulo sin confirmar». La segunda frase pierde «y «Cerrar» no pregunta salvo por un módulo sin confirmar», que la nueva ya dice. El texto sigue por debajo de 400 caracteres (registro Q-6) | §8.2 |
| V21-05 | **IR1-R N-05** | `F4B-PBT2C-WORKSPACE-CARRYOVER` tiene **29** caracteres: los `TaskId` tienen 26, 31 y 29 | §6.0; R-PBT2-05 |
| V21-06 | **IR1-R N-06** | La base pasa a `833ea6bf`. **F4B-E27** (la emisión de PB-T1c, con el paquete de PB-T1 v2.8) entra en el §1. [PT1] es la v2.8 de `833ea6bf`. Donde la v2 decía «PB-T1c no emitida», ahora dice «emitida y sin cerrar». D-PBT2-14 dice que CT-PBT1C-04 lleva dieciséis autopruebas: las quince comunes más la (d), propia de la ventana. Las guardas de PB-T2 miran archivos que no son la ventana y llevan las quince | Estado; §0; §1; HT2-24; D-PBT2-14; R-PBT2-09; §9 |
| V21-07 | **IR1-R N-07** | La atribución prevista de MU-T2C-03 pasa a CT-PBT2B-03 y 04, y CT-PBT2B-08 en Core. CT-PBT2B-06 sale: `cerrar-tras-aplicar` no depende del estado que recibe el borrador. La tabla de atribución recoge lo que falle de verdad | §5.4 |- **Marca [T2b]:** las anclas de PB-T2c son del texto que deja PB-T2b, y se miden sobre su cierre.

## Registro de cambios v1 → v2

**Origen:** IR1 (NECESITA v2, 0/8/16) y el mensaje de la sesión (las lecciones de PB-T1b, F4B-E23 y F4B-E25). **Ninguno de estos cambios toca el diseño de fondo**: los dos modos, el puerto, la carga por copia, la guarda, las costuras y la ocultación. La división pasa de dos a tres subtareas (V2-21).

| Id | Origen | Qué cambia | Dónde |
|---|---|---|---|
| V2-01 | **IR1 I-01** | G-L1 de PB-T2: las **quince** autopruebas de la CT-PBT1B-14 cerrada, (a), (b), (c), (e), (f1), (f2), (f3), (g1), (g2), (h1), (h2), (i), (j), (k1) y (k2), con sus textos de `95373af6:tests/RackCad.Tests/PushBackSharedReferenceGuardTests.cs:223-239`. Las reglas, `Rebind`, `Aliases`, `Members`, `Violations` e `Inject`, copiadas de esa versión. La versión cerrada ya existe (F4B-E26): se corrigen el §9 y el registro P. `AllObjects` (`:278`, con el parámetro `skip`) y `Fingerprint` (`:1080`), anclados en `95373af6` | D-PBT2-14; §5.0; CT-PBT2A-04; CT-PBT2B-09; §9 |
| V2-02 | **IR1 I-02** | Se quita la «lección» de la `AuthorityRevision` de una corrección; manda **F4B-E25** (en el §1, en el §6.0 y en cada contrato). Cada contrato declara sus `Authorities` por ruta, sección y clase (§16.5). W es `UNIT_CHANGE` en PB-T2b; W y WC, en PB-T2c; PB-T2a no tiene ninguna. D-PB-07 se cita como **F4B-E23**, y la `ExtensionData` anidada, como **F4B-E24** | §1; §6.0; §6.1 a §6.3 |
| V2-03 | **IR1 I-03** | **R3** mira toda la clausura `src/RackCad.UI/Systems/PushBack/*.cs`, con una autoprueba en WC. **CT-PBT2B-03** gana dos pasadas: los subdiálogos con costura (los gestos de CT-PB0-12, 13, 15 y 16, copiados) y, sobre `pb-20-ab`, `FrontalSideBox`, `SideSelectorBox` y la confirmación y la cancelación de módulos. Las botas por poste (CT-PB0-14) exigen la ventana mostrada: se declaran no ejercidas, cubiertas por R3 | D-PBT2-08; CT-PBT2B-03; CT-PBT2B-08 |
| V2-04 | **IR1 I-04** | **R11** en la guarda de texto: `ApplyEditsToEditingState(` se llama exactamente desde `RequestDraw` y `ApplyToDraft`, y ni `ApplyToDraft` ni `HasUnappliedDraftChanges` contienen `AllFieldsValid(`, `OutputBlockedAfterRecompute(`, `currentInputsAreValid` ni `HasError`, con autopruebas | D-PBT2-11; CT-PBT2B-08 |
| V2-05 | **IR1 I-05** | Fuera el plan B. Que ninguna guarda de PB-T1c impida reasignar `editable` se comprueba **en los anclajes de PB-T2a**, cuando PB-T1c ya está cerrada. Si no se cumple: STOP y una v3 con una subtarea propia y su censo. PBT2B-C18 sale | §6.0; R-PBT2-09 |
| V2-06 | **IR1 I-06** | **A-7 (M09)** → arrastre a PB-T3, porque la composición de D-PB-05 usa el mapa. **A-9 (M12)** → limitación declarada y nota para el `IsDirty` de PB-T4. Las anclas, en `a18ba345` | §8.1; §10 |
| V2-07 | **IR1 I-07** | `ApplyToDraft` ya **no** llama a `SetName` ni a `SyncName`: `Name_Changed`, en `TextChanged` (WX:47), ya recorta y sincroniza cada cambio de la caja. CT-PBT2B-02 y 04 ganan filas con un nombre nulo y con espacios, con `llamadas=0` tras «Aplicar» sin editar. R4 lo exige | D-PBT2-06; CT-PBT2B-02 y 04; CT-PBT2B-08 |
| V2-08 | **IR1 I-08** | Q-PBT2-1 reescrita: la tensión con la lectura de ADR-0029 D8 que está en el código, dicha a las claras; la excepción de la caja sin confirmar en (A); Escape en (B) y (C); y el literal de la pregunta nueva | §8.2 |
| V2-09 | IR1 M-01 | CT-PBT2B-02 gana tres filas de reapertura con una selección no por defecto: «Ambos lados» con una caja mixta, el lado B activo y una celda distinta de (0,0) | CT-PBT2B-02 |
| V2-10 | IR1 M-02 | `pb-21-cola` sale de CT-PBT2A-03 y CT-PBT2B-02 (fuera de la garantía, D-PBT1-10): 18 diseños | CT-PBT2A-03; CT-PBT2B-02 |
| V2-11 | IR1 M-03 | N-PBT2-01 y los once literales se dan al Owner **como información, sin pregunta** (§8.4); L-PBT2-7 se ajusta | §8.3; §8.4 |
| V2-12 | IR1 M-04 | A2-10 añade la enmienda de [P] §4 y §5: el dueño de los subdiálogos | §10 |
| V2-13 | IR1 M-05 | **L-PBT2-11:** la disposición del configurador, escrita en el perfil al cerrarse, es una tercera salida a disco, una preferencia de pantalla y no un dato del rack. Las pruebas solo usan `HeaderConfiguratorDialog`, que no crea la ventana | §8.3; INV-08 |
| V2-14 | IR1 M-06 | **A2-11:** el tramo del paso 4 de D-PB-06 hasta el DWG va a PB-T3, PB-T4 y Smoke-PB | §10 |
| V2-15 | IR1 M-07 | Las excepciones, con sentencia y mensaje fijados. La lectura de `LaunchFor(...)` en el RED, dentro de «EXCEPCION» | D-PBT2-02 a 04; CT-PBT2A-01 y 02; CT-PBT2B-01 |
| V2-16 | IR1 M-08 | En la guarda de texto, un miembro ausente da `regla-R<n>=False` sin lanzar. El partidor de miembros, copiado de la G-L1 cerrada, con el nombre de cada miembro. El `finally` de CT-PBT2B-06 contesta «Sí» antes de cerrar | CT-PBT2B-06; CT-PBT2B-08 |
| V2-17 | IR1 M-09 | CT-PBT2A-03 gana las huellas de D2 antes y después de `ApplyToDraft` (condición 3, en el camino del puerto). CT-PBT2B-09 comprueba además que el archivo nuevo solo declara el `enum` | D-PBT2-14; CT-PBT2A-03; CT-PBT2B-09 |
| V2-18 | IR1 M-10 | Se corrigen los hechos: G-10 mira **todo el producto** (`Src.AllProduct()`); `RefreshCompositePanel` es WC:738-777; la fila de G9; las anclas de `Fingerprint` y `AllObjects` | §2.7; §4; registro C, K y Q |
| V2-19 | IR1 M-11 | `MinSelected` fijados: `PushBackRichPreviewTests` 9, `PushBackLibrarySaveGateTests` 5, `DialogWindowCharacterizationTests` 23 (recuento estático, sin `Theory`) | §6.2 y §6.3 |
| V2-20 | IR1 M-12 | El tooltip de `FrontalSideBox` se queda como está: no hay literal nuevo, y se informa al Owner | L-PBT2-4; §8.4 |
| V2-21 | **IR1 M-13**, valorado | Los Worker de PB-T1b tardaron **70 min** (intento 1, de 17 CT) y **3 h 15 min** (la corrección, con una interrupción), registro Q-5. PB-T2b tendría 13 CT y el producto. **Se separa PB-T2c:** CT-PBT2B-08 a 10 de la v1 pasan a CT-PBT2C-01 a 03, con RED por tres mutantes, como PB-T1a. Los demás CT de PB-T2b se renumeran (tabla de abajo) | D-PBT2-01; §5.3; §5.4; §6.3 |
| V2-22 | IR1 M-14 | El constructor privado anula `lastComputation`, `hasValidModel` y el modelo de la sesión antes de `LoadFromModel`. Con un W0 inválido, «Restaurar» queda deshabilitado y nada llega al borrador. CT-PBT2B-02 gana la fila `estado-invalido` | D-PBT2-05; CT-PBT2B-02 |
| V2-23 | IR1 M-15 | A2-1 reescrito: PB-T4 envuelve el puerto en el host, con el fin de la marca *known clean* y la excepción de portador declarada | §10 |
| V2-24 | IR1 M-16 | La opción (B) de Q-PBT2-3 dice que toca `Pilot/` y sus guardas | §8.2 |
| V2-25 | IR1 §2 (nota) | Sale `guid-en-archivo=` de CT-PBT2B-03, que no discrimina: la comprobación del GUID es `id=` | CT-PBT2B-03 |
| V2-26 | Sesión: PBT1C-C13 | Las paradas por declaración nueva, **por nombre**, en inglés y acotadas a los archivos tocados: PBT2A-C13, PBT2B-C13 y PBT2C-C13 | §6.1 a §6.3 |
| V2-27 | Sesión | Los invariantes separan lo normativo de una «Nota (no va al contrato)» | §6.0 a §6.3 |
| V2-28 | Sesión; la mitad (b) de PBT1B-IR-M14 | La `ExpectedEvidence` pide que el `Trailer` de `worker-handoff.json` sea el trailer real de los commits RED y GREEN | §6.0 a §6.3 |
| V2-29 | Sesión; PBT1CV28R-I01 | El contrato es ASCII: todo texto que un invariante o una parada busque en el código es un **testigo ASCII** (tabla de D-PBT2-09) | D-PBT2-09; §6.2 |
| V2-30 | Sesión | La base pasa a `95373af6` (PB-T1b CERRADA, F4B-E26). La línea base de la CI de PB-T1b ya está medida (`44438060317`) | Estado; §6 |
| V2-31 | Sesión | Q-PBT2-1 a 4 listas para AskUserQuestion: texto autocontenido de menos de 400 caracteres, `header` de 12 como máximo, de 2 a 4 opciones con la recomendada primero | §8.2 |
| V2-32 | Sesión de redacción | Una segunda incidencia de procedimiento, declarada en el registro | §9 |

**Los ids de los CT, de la v1 a la v2:**

| v1 | v2 |
|---|---|
| CT-PBT2B-01 a 07 | CT-PBT2B-01 a 07 (sin cambio) |
| CT-PBT2B-08, 09 y 10 | **CT-PBT2C-01, 02 y 03** |
| CT-PBT2B-11 | CT-PBT2B-08 |
| CT-PBT2B-12 | CT-PBT2B-09 |
| CT-PBT2B-13 | CT-PBT2B-10 |

## 0. Alias

- **W** = `src/RackCad.UI/Systems/PushBack/RackPushBackSystemWindow.xaml.cs`; **WC** = `….Composite.cs`; **WX** = `….xaml`.
- **SES** = `src/RackCad.UI/Editor/RackEditorSession.cs`; **RID** = `src/RackCad.UI/Editor/RackEditorIdentity.cs`; **EM** = `src/RackCad.UI/Editor/EditorModules.cs`.
- **RPC** = `src/RackCad.Plugin/RackPushBackCommands.cs`; **RMC** = `src/RackCad.Plugin/RackMenuCommands.cs`.
- **PES**, **PESC**, **POG** = `src/RackCad.Application/Systems/PushBack/PushBackEditableState.cs`, `…Comparer.cs`, `PushBackEditorOutputGate.cs`.
- **DS**, **DR** = `src/RackCad.Application/Workspace/Drafts/DraftSession.cs`, `DraftRegistry.cs`.
- **T-SG** = `tests/RackCad.UI.Tests/PushBackClassicCharacterizationSourceGuardTests.cs`; **T-PG** = `tests/RackCad.Tests/PushBackClassicCharacterizationPluginGuardTests.cs`; **T-CL** = `tests/RackCad.UI.Tests/PushBackClassicCharacterizationCloseTests.cs`.
- **G-L1c** = `tests/RackCad.Tests/PushBackSharedReferenceGuardTests.cs` en `95373af6` (la CT-PBT1B-14 cerrada).
- **S0** = `tests/RackCad.UI.Tests/PushBackFullEditorCharacterizationSupport.cs`; **SC1** = `tests/RackCad.UI.Tests/PushBackDeclaredCorrectionSupport.cs`.
- **[P]** = el paquete de la fase v3.1; **[PT1]** = el paquete de PB-T1, **v2.8 en `833ea6bf`** (`docs/initiatives/I-64-f4b-pbt1-gate-contract-package.md`, con el que se emitió PB-T1c; v2.1, V21-06); **[OD]**, **[DP]** y **[OD2]** como en [PT1] §1.
- **S** = el estado del borrador (`DraftSession.CurrentDraftState`); **W0** = el estado de la ventana justo después de cargar S en WORKSPACE.

## 1. Autoridades

| Autoridad | Qué fija |
|---|---|
| [OD] (`F4B-DRAFTS/coordinator/owner-f4b-disposition.md`) | La fila de PB-T2 (literal arriba). D-PB-01 («ZERO authored DWG writes», «The mode must be explicit», «Do not let a Workspace invocation accidentally execute the classic drawing-write path»), D-PB-06, D-PB-08, D-PB-10, §7.3 (T1B) y §7.4 (RACKEDITAR) |
| [P] (`docs/initiatives/I-64-f4b-push-back-completo-package.md`, v3.1) | §4 (los dos modos y su tabla, `:477-493`); OB-01.1 a OB-01.6; OB-06.3; OB-07.1; OB-08.2; OB-10.1; OB-T1B.1 a OB-T1B.3; OB-RE.1; H-PB-30 a H-PB-33; R-PB-09 y R-PB-10; la fila de PB-T2 del §5 (`:504`) |
| **F4B-E23** (`F4B-DRAFTS/coordinator/owner-f4b-dpb07-extensiondata-decisions.md`, pregunta 1; la vuelta, `F4B-DRAFTS/pbfull/owner-return-dpb07.md`) | D-PB-07: «BOM y biblioteca (Recomendado)», con sus obligaciones para PB-T2 |
| **F4B-E24** (el mismo archivo, pregunta 2; la vuelta, `F4B-DRAFTS/pbfull/owner-return-extensiondata.md`) | «Basta así (Recomendado)»: el panel rechaza todo campo desconocido (AUTH-13); la pérdida de RACKEDITAR es una limitación anterior a F4B |
| **F4B-E25** (`F4B-DRAFTS/coordinator/owner-f4b-correction-unit-change-decision.md`; la vuelta, `F4B-DRAFTS/pbt1b/owner-return-verify-stop.md`) | «Regla F4B y reverificar (Recomendado)»: una ruta `UNIT_CHANGE` de `git diff --name-only AuthorityRevision BaseSha` de una corrección no incumple 16.3 si (1) todo commit del rango que la toca es el RedSha o el CurrentSha de una entrega de un Worker de la misma cadena, (2) la ruta está en el AllowedWriteScope de la delegación de esa entrega y (3) ningún commit de la sesión la toca. Las `EXTERNAL`, sin excepción |
| **F4B-E26** (`F4B-PBT1B-LOSSLESS-STATE-TYPE/task-close-package.md`) | PB-T1b CLOSED / PASS WITH LIMITATIONS sobre `a18ba345`; sus arrastres M09, M11, M12 y M15 a PB-T2 y PB-T4; la CI del cierre `44438060317` |
| **F4B-E27** (`f4b-dispositions.md:761`; el contrato emitido, `F4B-PBT1C-LOSSLESS-STATE-WINDOW/coordinator/gate-contract.json`) (v2.1, V21-06) | La emisión de PB-T1c con [PT1] v2.8: el alcance (W, WC y tres clases), PBT1C-C13, INV-PBT1C-03 con rangos, los testigos ASCII de INV-PBT1C-05 y las dieciséis autopruebas de CT-PBT1C-04. Es el texto de PB-T1c sobre el que se vuelven a medir las anclas [T1c] |
| Proposal V5 D-15 (`docs/initiatives/I-64-proposal-v5.md:557-582`) | Punto 2: «la superficie emite la intención de Actualizar hacia su host. El host clásico la interpreta como hoy». Punto 3: ida y vuelta del estado sin la ventana. Punto 4: «el host decide el dueño» |
| D-16 e INV-19 (`:586-593`) | Sin cambio observable en RACKEDITAR |
| ADR-0029 D7, D8, D12 y D13 | Una política de cierre; dirty por ámbito; sin modelo paralelo; la caracterización no se edita para que pase |
| [PT1] | D-PBT1-02 a D-PBT1-13 (el estado y la ventana sobre él), D-PBT1-15 (D2), §4 (guardas), §5.5 (censo), §10 (A-1, A-2, A-7 a A-10) |
| [OD2] (`F4B-DRAFTS/coordinator/owner-d1-d2-decisions.md`, F4B-E11) | D2: las dos referencias compartidas, las diez condiciones y el «IMPORTANT LIMIT» |
| [DP] (`F4B-DRAFTS/coordinator/owner-f4b-procedure-decisions.md`, F4B-E03) | C08 y C11 |
| F4B-E19, F4B-E21 y F4B-E22 (`owner-config-snapshot-j-decision.md`, `owner-f4b-opaque-collections-decision.md`, `owner-pbt1b-rework-decisions.md`) | La Snapshot J (P-01); la sesión pone las ocho colecciones opacas; las lecciones M10 y M14 |
| F4B-E06 y F4B-E09 (`f4b-dispositions.md`) | Las obligaciones de PB-T2: el paso 4 de D-PB-06 y el aviso de D-PB-08 en WORKSPACE |
| Los cierres de PB-TC, PB-TC2, PB-T1a, PB-T1b y PB-T1c | Las redes que siguen en verde; CT-PBTC-02, 05 y 07 como oráculos |
| T1B (`docs/initiatives/I-64-f4-t1b-gate-contract-package.md`) | 55 casos de UI y 10 de Core, y sus guardas de texto |
| `docs/AUTOMATION_PLAN.md` §16.3 a §16.11; `docs/automation/agent-execution/README.md` §4 y §10; `routing.md`; `model-catalog.md` | El protocolo, A1-A8, el RED de la cadena, las 14 comprobaciones, los términos, las paradas y las celdas |

## 2. Hechos medidos en `2f5d3dec`, `95373af6` y `833ea6bf`

El `src/` es el mismo en las tres (registros Q-1 y Q-7). Las líneas de W, WC y WX valían en las tres [T1c]. Desde la v2.3 (V23-03), las de W y WC son las de `0dc623ca`, el cierre de PB-T1c; WX no cambia.

### 2.1 Hoy, aplicar, cerrar y escribir van juntos [T1c]

- **HT2-01. «Aplicar al estado de edición» no es un paso propio.** Cada gesto escribe en el estado y recalcula (`Recompute`, W:543-650; `ReadInputs`, W:760-782; desde PB-T1c, `ReadInputs` escribe en `editable`, D-PBT1-13 (5)). Lo único que no llega al estado sin un clic es la **edición de módulo escenificada**. `RequestDraw` la confirma (W:3270-3273), y cerrar sin petición la pierde después de preguntar (`PendingWork`, W:3342-3344).
- **HT2-02. `RequestDraw` (W:3247-3304) junta en un manejador:**
  - las guardas (W:3249, W:3255);
  - la confirmación de módulos (W:3270-3273);
  - el recálculo (W:3275);
  - la validez (W:3276-3280);
  - la compuerta de salida (W:3284-3287);
  - el nombre y el modelo (W:3289-3291);
  - la anotación de la intención (`RequestUpdate`, W:3294, o `RequestInsert`, W:3299);
  - el cierre (W:3303).

  **Escribir no lo hace la ventana:** lo hace quien la abrió, después de `ShowModalWindow` (HT2-09).
- **HT2-03. «Insertar varias…»** (W:3217-3245) hace lo mismo y además abre el diálogo de lote (W:3234). No mira `canInsertInAutoCad` (T-SG CT-23, `:357-379`).
- **HT2-04. La intención se anota en la sesión** (SES:138-195): `EnsureId`, `UpdateOnly`, `InsertView` e `InsertSection`, `InsertionRequest` e `InsertRequested = true`, y se lanza `InsertRequestedRaised`, que nadie escucha (T-SG `:251-264`).
- **HT2-05. Cerrar** (`Close_Click`, W:3306 → `OnClosing`, W:3320-3329) solo pregunta si no hay petición y hay un módulo escenificado (W:3322; W:3342-3344). Escape, la X y Alt+F4 pasan por el mismo sitio (W:3308-3319; ADR-0029 D7). **La lectura de ADR-0029 D8 que está en el código** (W:3336-3341): convertir lo demás en dirty «exigiria comparar la sesion entera contra su apertura, que es el dirty global artificial que ADR-0029 D8 no quiere». `RichEditorCloseContractTests.cs:77` lo fija: «no se inventa un dirty global que el producto no tiene».
- **HT2-06.** «Restaurar» (W:3192-3213) recarga el último diseño válido; con `lastComputation` nulo, se niega (W:3194-3198) y su botón se deshabilita (W:3492-3495). No escribe.
- **HT2-07.** «BOM» (W:3348-3364) y «Guardar en biblioteca» (W:3366-3401) pasan la misma compuerta de salida (W:3358, W:3378) y no anotan la petición («Saving never flags an insert», W:3391-3392).
- **HT2-08. `UpdateButtons`** (W:3449-3496):
  - «Actualizar» = `canInsertInAutoCad && canOutput && isEditingExisting`;
  - «Insertar» y los cuatro por vista = `canInsertInAutoCad && canOutput`;
  - «BOM» = `canOutput`;
  - «Guardar en biblioteca» = `currentInputsAreValid`;
  - `InsertBatchButton` no aparece (registro B).
- **HT2-08b. El nombre:** `Name_Changed` está conectado a `TextChanged` de `NameBox` (WX:47) y hace `SetName(NameBox.Text?.Trim())` en cada cambio que no sea una siembra (W:1126-1131). PB-T1c añade `SyncName()` justo después (D-PBT1-13 (8)). El comparador compara `Name` (PESC:429).

### 2.2 Cómo vuelve el estado al llamador

- **HT2-09. RACKEDITAR** (`EditPushBack`):
  - abre con `LoadExisting` (RPC:169) y `ShowModalWindow` (RPC:170);
  - lee `InsertRequested` (RPC:172), `DesignToInsert` y `SystemToInsert` (RPC:178-179);
  - toma el GUID del sobre por delante del de la ventana (RPC:187) y hace `system.Name = name;` (RPC:190);
  - lee `UpdateOnly` (RPC:193 y siguientes) e `InsertionRequest.Views` (RPC:254).
- **HT2-10.** RPB lee `InsertionRequest` (RPC:50) y dibuja (RPC:57; `system.Name` en RPC:98). El menú devuelve la petición solo si `InsertRequested` (EM:131) y la despacha (RMC:131-143; `pushBack.System.Name` en RMC:135).
- **HT2-11. El estado editable nunca vuelve al llamador:** solo el diseño, el sistema, el id, el nombre y las vistas de la petición (registro F-5). Por eso WORKSPACE necesita un canal nuevo: el puerto de PB-T2a.

### 2.3 Dónde se valida cada campo

- **HT2-12. La regla de cada caja** es `NumericFieldValidation.Validate`, en UI (`src/RackCad.UI/Controls/NumericFieldValidation.cs:61-137`), compartida por todos los editores (registro J-1).
- **HT2-13. La compuerta de cada pasada** es `AllFieldsValid` sobre 21 cajas (W:654-682). La usan `Recompute` (W:552) y siete manejadores (registro J-2), y cinco cajas se validan en su propio manejador (registro J-3).
- **HT2-14. La validación de Application** (`PushBackAdvancedRackParameters.Validate`, que llama el ensamblador) lanza, y la ventana lo convierte en un cálculo inválido (W:589-597; registro J-4).
- **HT2-15. La compuerta de salida** es `outputIsBlocked` (W:638; WC:790-794), que consultan «Actualizar», «Insertar», el lote, el BOM y la biblioteca (registro J-5). PB-T1c la delega en `PushBackEditorOutputGate` (D-PBT1-13 (7)).
- **HT2-16. La segunda implementación de una regla de caja que ya hay:** el campo de escala del panel valida con `PushBackPilotFieldAdapter.Validate` (`PushBackPilotFieldAdapter.cs:35-53`), con los mismos textos y el mismo analizador que `AnnotationScaleBox` (WX:68-69: `Minimum="0" MinimumInclusive="False"`), escrito otra vez en Application (registro J-6). Es anterior a F4B.

### 2.4 Los subdiálogos y su dueño

- **HT2-17. Todos los subdiálogos son de la ventana:** el configurador (W:2351), las botas por poste (W:2646), la defensa (W:2790), la seguridad (W:2941), los topes (W:2972), el BOM (W:3363), el lote (`TryShow(this, …)`, W:3234) y el diálogo de guardado (`PromptSaveToLibrary(this, …)`, W:3383, que llega a `dialog.ShowDialog(owner)`, `UiSupport.cs:81`). El aviso de descarte no tiene dueño WPF (H-108 de MB).
- **HT2-18. El dueño de la ventana lo pone quien la abre:**
  - RPB y RACKEDITAR la abren sin dueño, con `ShowModalWindow` (RPC:47-48, :168-170); T-PG CT-24 prohíbe `Owner =` en RACKPUSHBACK;
  - el menú y la biblioteca, con `{ Owner = context.Owner }` (EM:114, :121; T-SG CT-21).
- **HT2-19. Las costuras:**
  - Hay costura para el configurador (`HeaderConfiguratorDialog`, que **no crea la ventana**, W:1450), la seguridad (`SafetyDialog`, W:1418), la defensa (W:1430), los topes (W:1424) y las botas por poste (W:2625).
  - **No hay costura para el BOM ni para el diálogo de guardado.** En una ventana sin mostrar, el diálogo de guardado lanza al pedir su dueño, y `PushBackLibrarySaveGateTests.cs:95` usa esa excepción como señal. `PromptSaveToLibrary` crea antes la carpeta de biblioteca del perfil (`UiSupport.cs:66-68`).
  - El configurador real escribe su disposición en el perfil al cerrarse (T-SG CT-22b, `:344-352`).
  - Las botas por poste solo se ejercen con la ventana mostrada (`S0.AbrirFueraDePantalla`, CT-PB0-14; registro Q-4).

### 2.5 El piloto Workspace hoy, y lo que le falta para recibir el estado editado

- **HT2-20. Lo que hace hoy:**
  - el panel edita un solo campo, la escala (`PushBackPilotFieldAdapter`);
  - el borrador es un `DraftSession<PushBackPilotDraftState>` con el comparador y el copiador del piloto (`WorkspaceHostCore.cs:347-352`);
  - el materializador rechaza el compuesto y la cola (PM:106, PM:108) y no compone nada que no sea la escala (PM:112);
  - escribe con PREPARE → MUTATE → POST de T4;
  - cuando un rack no entra, el panel muestra el texto de `ClassicEditorHint` y no abre nada (registro I).

  El host acaba la marca *known clean* en cada cambio de un borrador (`WorkspaceHostCore.cs:66`, `:832`; `WorkspaceLifecycleGuardTests.cs:509-520`), y los valores de su puerto no exponen tipos de `Drafts` ni de `Systems`, salvo la excepción declarada de `TryUpdatePilot` (`WorkspaceEditIntentHostTests.cs:1030-1046`).
- **HT2-21. Lo que le falta:**

| Falta | Tarea |
|---|---|
| Un canal por el que el editor devuelva su estado a un `DraftSession` | **PB-T2a** |
| Abrir la ventana sobre un estado, y no sobre un diseño, con un botón principal que aplique al borrador | **PB-T2b** |
| Un `DraftSession<PushBackEditableState>` con el comparador y el copiador de PB-T1, la materialización del estado desde la lectura (A-5), el envoltorio del puerto en el host y su excepción de portador | PB-T4 |
| «Editar todo», la modalidad, el texto pendiente del panel y K = 1 (R-PB-10) | PB-T4 |
| Componer y escribir el diseño completo | PB-T3 |
| Quitar los rechazos A/B (OB-03.2) | PB-T3 o PB-T4 |

- **HT2-22. El núcleo ya admite el estado de PB-T1:** `DraftRegistry.Begin<TState>` es público (DR:143-167), `SetCurrentDraftState` guarda una copia hecha con el copiador inyectado (DS:345-350) y `DraftEditStatus` solo tiene `Updated` y `NotEditable` (DS:150-156). Una prueba puede nacer un `DraftSession<PushBackEditableState>` sin tocar el host (registro H-1).

### 2.6 El estado de PB-T1 sobre el que se diseña

- **HT2-23. PB-T1b, cerrada** (F4B-E26, sobre `a18ba345`; `src/` igual que en `2f5d3dec`):
  - `PushBackEditableState`;
  - su comparador y su copiador (`Copy` sobre un estado **nuevo**, PESC:63-136; «The target is a fresh state», PESC:140-141);
  - el mapa y `PushBackEditorOutputGate` (registro G).

  **No hay ninguna carga en sitio de un estado desde otro.**
- **HT2-24. PB-T1c** (cerrada, F4B-E31, sobre `161a0dad`; D-PBT1-13 de la v2.8; v2.3, V23-05):
  - `private readonly PushBackEditableState editable = new PushBackEditableState();`, con `composite => editable.Editor`;
  - los portadores, como reenvíos calculados;
  - las cargas, delegadas, con `UpdateCompositeEnabled()` en la línea siguiente;
  - `editable.AdoptCarriers(inputs, catalog);` como primera sentencia del `try` de `LoadFromModel`;
  - `ReadInputs`, que escribe en el estado;
  - `SyncName()` tras cada `Adopt` y cada `SetName`;
  - la costura `EditableStateForTest`.

  CT-PBT1C-03, por la letra de la v2.8, solo exige `EditableStateForTest => editable;` y los reenvíos sin almacenamiento (IR1 §3). IR1-R lo confirma sobre el texto emitido de CT-PBT1C-01, 03 y 04 (`833ea6bf`): nada impide reasignar `editable` (IR1-R §10.2, I-05).
- **HT2-25. La reasignación de `editable` en el constructor de WORKSPACE** es C# válido aunque el campo sea `readonly`, porque es otro constructor de la misma clase. Es segura porque todo lo que cuelga de `editable` es calculado: `composite`, `state` y `moduleOwner` son propiedades (W:63, :82), ninguna sección de seguridad, botas o topes guarda el estado (se construyen al abrir su diálogo, W:2460-2536), y no hay suscripciones a eventos del estado (IR1 §3 y §4). **Se comprobó sobre el texto real de PB-T1c en los anclajes de PB-T2a** (V2-05; F4B-E32; v2.3, V23-05).
- **HT2-26. M11 de PB-T1b** (A-8 de [PT1]): con un diseño nulo, `LoadDesignEditor` y `RestoreEditor` mutan antes de lanzar. **PB-T2 no lo ejerce:** la carga de WORKSPACE no llama a `LoadDesignEditor`, y «Restaurar» conserva su guarda (W:3194-3198).

### 2.7 Las guardas que tocan estas zonas

El detalle está en los registros K y Q, y su tratamiento, guarda por guarda, en el §4. Estas son las que condicionan el diseño:
- **T-SG lee toda la clausura `src/RackCad.UI/Systems/PushBack`** (`:169-176`): un archivo nuevo ahí entra en CT-19.
- **S-7, S-9 y CT-22** fijan los literales y el orden del BOM y de la biblioteca. **CT-23** fija `InsertBatch_Click`.
- **G-10 mira todo el producto** (`Src.AllProduct()`, `WorkspacePilotUpdateIntegrationGuardTests.cs:30`), con una lista cerrada de los que pueden llamar a `.RequestUpdate(`: el controlador, la compuerta, cuatro ventanas con una llamada cada una (W entre ellas) y el modelo de vista del campo (`:725-767`). Ningún archivo nuevo de PB-T2, ni de Application ni de UI, puede contenerla.
- **El núcleo de borradores** no admite nombres de sistema ni tipos llamados `…Editor…` (`WorkspaceDraftBoundaryTests.cs:193`, `:204`). Por eso el puerto no va en `Drafts/`.
- **La reflexión de la vista previa** (`PushBackRichPreviewTests.cs:349-367`) y **el censo de ventanas**.

## 3. El diseño

### D-PBT2-01. Tres subtareas secuenciales (OB-10.1, R-PB-09)

**Por qué dividir:**
- **PB-T2a frente a PB-T2b:** son dos capas con dos regímenes de RED, el esqueleto inerte en Application y la costura inerte en la ventana. El canal al borrador se prueba entero sin la ventana **antes** de que la ventana dependa de él (D-15, punto 3), como PB-T1b frente a PB-T1c.
- **PB-T2b frente a PB-T2c** (v2, V2-21; IR1 M-13): el tamaño. El Worker de PB-T1b tardó 70 min con 17 CT y su producto, y el de la corrección, 3 h 15 min con una interrupción (registro Q-5). PB-T2b llevaría el producto de W, siete clases y 13 CT. Las tres pruebas que solo caracterizan el modo ya hecho (el paso 4 de D-PB-06, el aviso de D-PB-08 y el censo) pasan a PB-T2c, con RED por mutantes, como PB-T1a.

**La secuencia:** PB-T2a → PB-T2b → PB-T2c, cada una desde la punta que deja cerrada la anterior. PB-T2a solo cambia Application; PB-T2b, W y el archivo del modo; PB-T2c solo pruebas (sus mutantes se revierten).

**No es material** (como L-3 de [PT1]): el contenido, el orden y las dependencias de la fase no cambian. Se informa al Owner en Q-PBT2-4.

### D-PBT2-02. PB-T2a: el puerto al borrador (API exacta)

**Dónde:** `src/RackCad.Application/Systems/PushBack/PushBackDraftStatePort.cs`, con un solo `namespace RackCad.Application.Systems.PushBack` de bloque. **No va en `Drafts/`**: el núcleo no admite nombres de sistema ni tipos llamados `…Editor…` (§2.7).

```csharp
public enum PushBackDraftApplyStatus { NotEditable = 0, Applied = 1 }

public readonly struct PushBackDraftApplyResult
{
    public PushBackDraftApplyResult(PushBackDraftApplyStatus status, bool isDirty);
    public PushBackDraftApplyStatus Status { get; }
    public bool IsDirty { get; }
}

/// El único canal por el que el editor completo de Push Back, en modo WORKSPACE, devuelve su estado editado.
public interface IPushBackDraftStatePort
{
    PushBackDraftApplyResult ApplyToDraft(PushBackEditableState edited);
}

public sealed class PushBackDraftSessionPort : IPushBackDraftStatePort
{
    public PushBackDraftSessionPort(DraftSession<PushBackEditableState> draft);
    public PushBackDraftApplyResult ApplyToDraft(PushBackEditableState edited);
    public static PushBackDraftEditorLaunch LaunchFor(DraftSession<PushBackEditableState> draft, RackProject sourceProject);
}
```

**La semántica, con las sentencias de las excepciones fijadas** (v2, V2-15):
- **el constructor:** con `draft` nulo, `throw new ArgumentNullException(nameof(draft));`. Si no, guarda la referencia en el campo privado `private readonly DraftSession<PushBackEditableState> draft;`;
- **`ApplyToDraft`:**
  - con `edited` nulo, `throw new ArgumentNullException(nameof(edited));`;
  - si no, llama **solo** a `draft.SetCurrentDraftState(edited)`, que guarda una copia (DS:345-350), y traduce su resultado: `Updated` → `Applied`; `NotEditable` → `NotEditable`. `IsDirty` es el del núcleo;
- **`LaunchFor`:**
  - con `draft` nulo, `throw new ArgumentNullException(nameof(draft));`;
  - si no, `return new PushBackDraftEditorLaunch(draft.CurrentDraftState, draft.RackId, sourceProject, new PushBackDraftSessionPort(draft));`.

**Por qué un resultado propio y no `DraftEditResult`:** el puerto del panel prohíbe que tipos de `Drafts` crucen sus valores (registro H-3). El de la ventana sigue el mismo criterio.

**Lo que el adaptador no hace:**
- no copia: copia el borrador;
- no valida: la validación es la de la ventana (D-PBT2-11);
- no llama a ninguna otra operación del borrador.

### D-PBT2-03. PB-T2a: el lanzamiento tipado

**Dónde:** `src/RackCad.Application/Systems/PushBack/PushBackDraftEditorLaunch.cs`.

```csharp
public sealed class PushBackDraftEditorLaunch
{
    public PushBackDraftEditorLaunch(PushBackEditableState state, string rackId, RackProject sourceProject, IPushBackDraftStatePort port);
    public PushBackEditableState State { get; }
    public string RackId { get; }
    public RackProject SourceProject { get; }
    public IPushBackDraftStatePort Port { get; }
}
```

- **Las comprobaciones, en este orden** (v2, V2-15):
  - `if (state == null) throw new ArgumentNullException(nameof(state));`;
  - `if (string.IsNullOrWhiteSpace(rackId)) throw new ArgumentException("A rack id is required.", nameof(rackId));`, que es el texto de `DraftRegistry.Begin`, DR:145;
  - `if (port == null) throw new ArgumentNullException(nameof(port));`.

  `sourceProject` puede ser nulo.
- Guarda las referencias tal cual: **no copia**. Copia la ventana (D-PBT2-05).
- `SourceProject` es el proyecto interior del sobre, con sus metadatos I-11. Lo usa «Guardar en biblioteca» (`WithSourceMetadataFrom`). El host de PB-T4 lo saca de la lectura.
- Es el **argumento tipado** de OB-01.1b.

### D-PBT2-04. PB-T2b: el modo explícito

- **El tipo:** `src/RackCad.UI/Systems/PushBack/PushBackEditorMode.cs`, que solo contiene `public enum PushBackEditorMode { Classic = 0, Workspace = 1 }` en un `namespace RackCad.UI.Systems.PushBack` de bloque. Entra en la clausura de T-SG y no lleva ningún token de CT-19.
- **El campo:** `private readonly PushBackEditorMode mode;` en W.
  - Vale `Classic`, el valor por defecto, en los constructores de hoy, que no cambian ni un carácter (OB-01.1b).
  - Solo lo asigna, una vez, el constructor privado de WORKSPACE. **Es inmutable.**
- **La factoría:** `public static RackPushBackSystemWindow ForDraft(PushBackDraftEditorLaunch launch)`. Con `launch` nulo, `throw new ArgumentNullException(nameof(launch));`; si no, `return new RackPushBackSystemWindow(launch);`.
- **El constructor privado:** `private RackPushBackSystemWindow(PushBackDraftEditorLaunch launch) : this(canInsertInAutoCad: false)`, con la carga de D-PBT2-05. La ventana que devuelve **no se muestra** y no tiene dueño: los pone el host (PB-T4, OB-01.4).
- **Quién elige el modo:** quien construye la ventana. Los cuatro sitios de hoy eligen CLASSIC. En F4B, solo el «Editar todo» del panel (PB-T4) llamará a `ForDraft`.
- **La costura:** `internal PushBackEditorMode ModeForTest => mode;`.

### D-PBT2-05. PB-T2b: WORKSPACE carga una copia del estado, sin pasar por un diseño

**Por qué no por un diseño:** la cola aparcada no se persiste («dormir no persiste», R-PB-02). Cargar S por su diseño perdería la cola, la sesión de módulos y la selección (D-PB-03).

**El cuerpo del constructor privado, en este orden** (el de `LoadExisting` en D-PBT1-13 (3)):

```csharp
mode = PushBackEditorMode.Workspace;
draftPort = launch.Port;
editable = PushBackEditableStateComparer.Copy(launch.State);   // el editor, la cola, la autoridad de módulos y los portadores
var inputs = editable.BuildInputs(assembler);                  // lo que la carga clásica habría adoptado
UpdateCompositeEnabled();                                      // el sitio de W:314, como en LoadExisting
sourceProject = launch.SourceProject;
isEditingExisting = true;
session.Identity.Adopt(launch.RackId, editable.Name);
SyncName();
lastComputation = null;                                        // v2, V2-22 (IR1 M-14): nada del LoadNew encadenado
hasValidModel = false;
session.SetModel(null, null);
LoadFromModel(inputs, editable.Name);
appliedState = PushBackEditableStateComparer.Copy(editable);   // W0: la referencia de «sin aplicar»
ApplyModeChrome();                                             // D-PBT2-09
```

- **Los campos nuevos:** `private readonly IPushBackDraftStatePort draftPort;` y `private PushBackEditableState appliedState;`.
- **La reasignación de `editable`** ocurre en un constructor, así que el campo sigue siendo `readonly` (HT2-25).
- **Las tres anulaciones** (v2, V2-22): con un S cuyo cálculo es inválido, W0 se queda sin último cálculo válido. «Restaurar» se deshabilita (W:3492-3495), y el BOM, la biblioteca y «Aplicar» se niegan (HT2-07; D-PBT2-06). Así nunca llega al borrador el rack por defecto del `LoadNew` encadenado. `RenderPreview` y `PlanFor` ya admiten un cálculo nulo (W:3029, W:3093).
- **D2:** la copia comparte con S solo las dos referencias declaradas de cada lado (D-PBT1-05). La ventana no las muta (CT-PBT1C-04). Su primer recálculo vuelve a apuntar su línea base, sin tocar la de S.
- **M11 no aplica** (HT2-26).
- **La normalización declarada N-PBT2-01** (se informa al Owner, §8.4). `LoadFromModel` empieza por `editable.AdoptCarriers(inputs, catalog)`, que vuelve a adoptar los portadores desde las entradas que el propio estado produce. Es idempotente salvo en dos casos, y los dos dejan **la misma salida**:
  - (a) si S trae `DimensionViewsTouched = true`, W0 queda con `false` y con `LoadedDimensionViews` = la política efectiva (`DimensionViewPolicy.FromEditor(…)`; PES:157);
  - (b) si S trae una selección de seguridad no autorizada, W0 no la trae (`BuildInputs` usa `AuthorizedSafety`, PES:399).

  Es lo que pasa hoy al reabrir con RACKEDITAR el diseño que ese estado produce. Lo fija CT-PBT2B-02. Cualquier otra diferencia, o una salida distinta, es PBT2B-C15. **La carga no normaliza el nombre:** la siembra de `NameBox` va bajo `suppressSync`, así que `Name_Changed` no corre, y un nombre nulo o con espacios queda tal cual (CT-PBT2B-02, v2, V2-07). **Lo normaliza un guardado en la biblioteca** (v2.1, V21-01; IR1-R N-01): `SaveLibrary_Click` hace `SetName(NameBox.Text?.Trim())` (W:3389, un literal de CT-22 que no se puede quitar), con el `SyncName()` de PB-T1c, cuando el diálogo devuelve una ruta (W:3384). Desde ese momento el estado de la ventana difiere de W0 en el nombre: «Cerrar» pregunta y «Aplicar» lo lleva al borrador. Es L-PBT2-12, y CT-PBT2B-04 lo fija.
- **Las cargas públicas en WORKSPACE:** `LoadNew`, `LoadDesignForNew` y `LoadExisting` empiezan por `if (mode == PushBackEditorMode.Workspace) throw new InvalidOperationException(WorkspaceLoadRefusalMessage);`. El constructor encadenado llama a `LoadNew` cuando `mode` todavía vale `Classic`.

### D-PBT2-06. «Aplicar al estado de edición», «Aplicar al borrador» y «escribir», separados

**(1) Aplicar al estado de edición: un solo paso, compartido.** Se extrae de `RequestDraw`, literal y en el mismo orden (W:3270-3287):

```csharp
private bool ApplyEditsToEditingState(string invalidMessage, string blockedFallback)
{
    if (moduleOwner.ModuleSession.HasPendingChanges)
    {
        moduleOwner.CommitModuleEdits();
    }

    RequestRecompute(); // synchronous validate + build
    if (!currentInputsAreValid || session.System == null)
    {
        SetStatus(invalidMessage, true);
        return false;
    }

    return !OutputBlockedAfterRecompute(blockedFallback);
}
```

`RequestDraw` (CLASSIC) queda así:
- `if (RefuseInWorkspace()) return;`;
- sus dos guardas de hoy (W:3249-3259), sin cambio;
- `if (!ApplyEditsToEditingState("Corrige los datos: no se puede insertar un modelo inválido.", "Corrige los datos: no se puede dibujar un modelo bloqueado.")) return;`;
- y lo de hoy, de W:3289 al `Close()` de W:3303, con el `SyncName()` de PB-T1c.

**Los textos, el orden y el efecto son los de hoy.** `InsertBatch_Click` no se refactoriza: CT-23 fija sus literales.

**(2) Aplicar al borrador (solo en WORKSPACE).** v2, V2-07: sin `SetName` ni `SyncName`, que ya hace `Name_Changed` en cada cambio de la caja.

```csharp
private void ApplyToDraft()
{
    if (!ApplyEditsToEditingState(WorkspaceInvalidMessage, WorkspaceBlockedMessage)) return;
    if (!HasUnappliedDraftChanges())
    {
        SetStatus(WorkspaceNothingToApplyMessage, false);
        return;
    }

    var result = draftPort.ApplyToDraft(PushBackEditableStateComparer.Copy(editable));
    if (result.Status != PushBackDraftApplyStatus.Applied)
    {
        SetStatus(WorkspaceNotEditableMessage, true);
        return;
    }

    appliedState = PushBackEditableStateComparer.Copy(editable);
    SetStatus(WorkspaceAppliedMessage, false);
}

private bool HasUnappliedDraftChanges()
    => mode == PushBackEditorMode.Workspace && appliedState != null
       && !PushBackEditableStateComparer.Instance.Equals(editable, appliedState);
```

- `Update_Click` pasa a `{ if (mode == PushBackEditorMode.Workspace) { ApplyToDraft(); return; } RequestDraw(null, -1, updateOnly: true); }`.
- **No cierra la ventana** (según Q-PBT2-1, opción A). No anota ninguna petición, no llama a `SetModel` y no lee `InsertRequested`.
- **«Sin cambios» no llama al puerto:** si el estado es igual al último aplicado, o a W0, el borrador no se toca. Así, abrir y aplicar sin editar deja S exacto, también con N-PBT2-01 y con un nombre nulo o con espacios (CT-PBT2B-04), **salvo que antes se haya guardado en la biblioteca**, que normaliza el nombre (L-PBT2-12; v2.1, V21-01).
- **Lo que recibe el borrador es una copia**, y el borrador la vuelve a copiar. La ventana no guarda ninguna referencia a S, salvo las dos de D2 que trae su copia.

**(3) Escribir sigue siendo de otro:**
- en CLASSIC, del llamador tras `ShowModalWindow` (HT2-09 y HT2-10);
- en WORKSPACE, del «Actualizar» del panel (PB-T3).

La ventana en WORKSPACE no tiene ningún camino a la escritura (D-PBT2-08).

### D-PBT2-07. «Cerrar»

- **El camino es el de hoy:** `Close_Click`, Escape, la X y Alt+F4 llegan a `OnClosing` (W:3320-3329), que no cambia. Una sola política de cierre (ADR-0029 D7).
- **`PendingWork` declara un ámbito más en WORKSPACE** (según Q-PBT2-1, opción A):

```csharp
internal EditorPendingWork PendingWork()
{
    var modules = EditorPendingWork.When(
        moduleOwner.ModuleSession != null && moduleOwner.ModuleSession.HasPendingChanges,
        "Hay cambios de módulo sin confirmar que se perderán al cerrar. ¿Deseas continuar?");
    return modules.HasPendingWork ? modules : EditorPendingWork.When(HasUnappliedDraftChanges(), WorkspaceUnappliedQuestion);
}
```

  - **En CLASSIC,** `HasUnappliedDraftChanges()` es `false` y el resultado es el de hoy, con la expresión del módulo literal.
  - **En WORKSPACE,** primero va el módulo escenificado, con el texto de hoy, y después «sin aplicar al borrador».
- **La tensión con D8, dicha a las claras** (v2, V2-08; IR1 I-08): este ámbito compara **todo** el estado editable con el último aplicado. Es el primer dirty de todo el estado de este editor, el que la lectura de D8 del código evita en CLASSIC (HT2-05). Lo acota una frontera explícita, la del borrador, y solo existe en WORKSPACE. Por eso es Q-PBT2-1, y el Owner lo ve en su texto.
- **El borrador queda como estaba** si se cierra sin aplicar ([P] §4).
- **Lo que no cuenta como «sin aplicar»:** un texto escrito en una caja sin salir de ella todavía no está en el estado.
  - Con Escape, la X o Alt+F4 se pierde sin preguntar, como en CLASSIC (H-095 de MB).
  - Con un clic en «Cerrar», la caja pierde el foco y recalcula antes, así que sí se pregunta.

  Lo dice la opción (A) de Q-PBT2-1 (L-PBT2-10).

### D-PBT2-08. La guarda «sin camino clásico» (OB-01.3)

- **El ayudante:**
  ```csharp
  private bool RefuseInWorkspace()
  {
      if (mode != PushBackEditorMode.Workspace) return false;
      SetStatus(WorkspaceRefusalMessage, true);
      return true;
  }
  ```
- **Dónde va, como primera sentencia:**
  - en `RequestDraw` («Actualizar» en CLASSIC, «Insertar» y los cuatro botones por vista);
  - en `InsertBatch_Click`, delante de la confirmación de módulos, que CT-23 solo exige antes del diálogo.
- **En toda la clausura `src/RackCad.UI/Systems/PushBack/*.cs`** (W, WC y los demás), los únicos miembros que anotan una petición clásica (`.RequestUpdate(`, `.RequestInsert(` y `.RequestInsertViews(`) son esos dos, y los dos empiezan por la guarda. Lo fija R3 (CT-PBT2B-08), que mira toda la clausura y tiene su autoprueba en WC (v2, V2-03).
- **No depende de `canInsertInAutoCad`:** OB-01.1b manda que el modo no sea un booleano.
- **Lo que prueba la UI-STA** (CT-PBT2B-03, con tres pasadas, v2, V2-03):
  - pulsa en WORKSPACE **todos** los botones de la barra, también los ocultos;
  - una edición;
  - los subdiálogos con costura: el configurador, la seguridad, la defensa y los topes;
  - la confirmación y la cancelación de módulos;
  - `FrontalSideBox` y `SideSelectorBox`.

  Después de cada gesto comprueba que `InsertRequested` sigue en `false` e `InsertionRequest` en `null`. Las botas por poste necesitan la ventana mostrada (HT2-19): no se ejercen, y las cubre R3.
- **Lo que queda para PB-T4** (OB-01.3): la guarda de texto del abridor de «Editar todo».

### D-PBT2-09. Rótulos, ocultación y los once literales (OB-01.6)

**`ApplyModeChrome()`** (solo en WORKSPACE, una vez, al final del constructor privado):
- `UpdateButton.Content = WorkspaceApplyLabel;`;
- `Visibility.Collapsed` para `InsertButton`, `InsertLateralButton`, `InsertFrontalEntradaButton`, `InsertFrontalPosteriorButton`, `InsertPlantaButton` e `InsertBatchButton`.

**`UpdateButtons()`** no cambia el código de hoy. Solo añade al final:
```csharp
if (mode == PushBackEditorMode.Workspace)
{
    UpdateButton.IsEnabled = canOutput;
    UpdateButton.ToolTip = outputIsBlocked ? WorkspaceApplyBlockedToolTip
        : currentInputsAreValid ? WorkspaceApplyToolTip : "Corrige los campos numéricos marcados.";
}
```
Según Q-PBT2-2, opción A: «Aplicar» exige lo mismo que «Actualizar».

**WX no cambia.** En CLASSIC no cambia ningún rótulo. `FrontalSideBox` sigue visible en un compuesto, con su tooltip de hoy (L-PBT2-4, v2, V2-20).

**Los once literales** (constantes `private const string` en W, cada una una sola vez). Se informan al Owner, sin pregunta (§8.4). La columna «Testigo ASCII» es lo que busca el contrato, que es ASCII (v2, V2-29); los tests comparan el literal entero en C#.

| Constante | Texto | Testigo ASCII del contrato |
|---|---|---|
| `WorkspaceApplyLabel` | `Aplicar al borrador` | `Aplicar al borrador` |
| `WorkspaceApplyToolTip` | `Guarda los cambios en el borrador. El dibujo cambia con «Actualizar» del panel.` | `Guarda los cambios en el borrador. El dibujo cambia con ` |
| `WorkspaceApplyBlockedToolTip` | `Corrige el diagnóstico bloqueante: el rack no puede pasar al borrador así.` | ` bloqueante: el rack no puede pasar al borrador as` |
| `WorkspaceRefusalMessage` | `En el editor del borrador no se dibuja: aplica al borrador y actualiza desde el panel.` | el texto entero |
| `WorkspaceLoadRefusalMessage` | `La ventana del borrador solo carga el estado del borrador.` | el texto entero |
| `WorkspaceInvalidMessage` | `Corrige los datos: no se puede aplicar al borrador un modelo inválido.` | `Corrige los datos: no se puede aplicar al borrador un modelo inv` |
| `WorkspaceBlockedMessage` | `Corrige los datos: no se puede aplicar al borrador un modelo bloqueado.` | el texto entero |
| `WorkspaceNothingToApplyMessage` | `No hay cambios que aplicar al borrador.` | el texto entero |
| `WorkspaceAppliedMessage` | `Cambios aplicados al borrador. El dibujo cambia con «Actualizar» del panel.` | `Cambios aplicados al borrador. El dibujo cambia con ` |
| `WorkspaceNotEditableMessage` | `El borrador no admite cambios ahora: cierra el editor y revisa el panel.` | el texto entero |
| `WorkspaceUnappliedQuestion` | `Hay cambios sin aplicar al borrador que se perderán al cerrar. ¿Deseas continuar?` | `Hay cambios sin aplicar al borrador que se perder` |

**Los testigos ASCII de los textos de hoy** que fijan los invariantes:
- la pregunta del módulo: `sin confirmar que se perder`;
- los mensajes de `RequestDraw`: `Corrige los datos: no se puede insertar un modelo inv` y `Corrige los datos: no se puede dibujar un modelo bloqueado.`;
- S-7, S-9 y las sentencias de CT-22 y CT-23, que ya son ASCII.

### D-PBT2-10. BOM y biblioteca en WORKSPACE (F4B-E23): las dos costuras

**La decisión** (F4B-E23): los dos botones se ofrecen en WORKSPACE, con la misma compuerta que en CLASSIC (HT2-07), sin código propio de WORKSPACE.

**Costura 1, el BOM:** `internal Action<RackCad.Application.Bom.BillOfMaterials, Window> BomDialog;`, nula por defecto. En `Bom_Click`, justo delante de la línea S-7, que no cambia:
```csharp
if (BomDialog != null)
{
    BomDialog(lastComputation.Bom, this);
    return;
}

new RackBomWindow(lastComputation.Bom) { Owner = this }.ShowDialog();
```
La costura recibe el BOM calculado y el dueño que usaría la ventana real, sin construirla. WPF no admite como dueña una ventana que no se ha mostrado: con las ventanas de prueba, `Owner = this` lanzaría.

**Costura 2, el diálogo de guardado:** `internal Func<Window, string, string> LibraryPathDialog;`, que recibe el dueño y el nombre propuesto y devuelve la ruta, o nulo si se cancela. En `SaveLibrary_Click`, la sentencia de W:3383 pasa a:
```csharp
var path = LibraryPathDialog != null
    ? LibraryPathDialog(this, NameBox.Text)
    : UiSupport.PromptSaveToLibrary(this, NameBox.Text, "sistema");
```
- El literal S-9 sigue, y el orden de CT-22 se conserva.
- Con la costura, la prueba no crea la carpeta de biblioteca.

**S-7, S-9 y CT-22 quedan en verde** (§4). La condición de F4B-E23 («si no pueden quedar en verde, se vuelve al Owner antes del contrato») no se dispara.

**La identidad:** la ventana de WORKSPACE adopta el `RackId` del borrador, así que `EnsureId` (W:3388) no acuña nada. El archivo de biblioteca nunca lleva el GUID (`RackProject.ForPushBack(design)`). La comprobación que discrimina es `id=` en CT-PBT2B-03 (v2, V2-25).

**Los campos desconocidos:** `WithSourceMetadataFrom(sourceProject)` conserva los del nivel del proyecto (`RackProjectStore.cs:71-95`, llamado en `:60`). Los de la carga útil se pierden también en RACKEDITAR (F4B-E24). La prueba usa un campo del nivel del proyecto (L-PBT2-6).

**Las salidas a disco:** las dos de F4B-E23, aceptadas (la exportación del BOM y el archivo de biblioteca), y una tercera, declarada: la disposición del configurador en el perfil al cerrarse (L-PBT2-11, v2, V2-13).

### D-PBT2-11. Una sola validación de campos

- **Dentro de la ventana, por construcción y con una guarda:**
  - WORKSPACE no tiene ninguna validación propia;
  - «Aplicar al borrador» pasa por **el mismo** `ApplyEditsToEditingState` que «Actualizar»;
  - el puerto no valida.
- **Lo fijan:**
  - **R11** de CT-PBT2B-08 (v2, V2-04): `ApplyEditsToEditingState(` se llama exactamente desde `RequestDraw` y `ApplyToDraft`, y ni `ApplyToDraft` ni `HasUnappliedDraftChanges` contienen `AllFieldsValid(`, `OutputBlockedAfterRecompute(`, `currentInputsAreValid` ni `HasError`. Una segunda validación equivalente salta;
  - **CT-PBT2B-05:** para el mismo gesto, las dos ventanas dan la misma validez y el mismo bloqueo.
- **El campo de escala del panel** (HT2-16): según Q-PBT2-3, opción A, PB-T2b lo fija con una prueba de concordancia (CT-PBT2B-10) y lo deja para PB-T4.
- **D2, condición 4:** CT-PBT2B-05, con huellas profundas.

### D-PBT2-12. El dueño de los subdiálogos (D-15, punto 4)

- **La lectura:** D-15 (4) se escribió para una superficie alojada en el panel. D-PB-01 eligió otra forma: el editor completo **es una ventana**, modal y a tamaño completo, la misma en los dos modos.
- **El diseño:**
  - en los dos modos, el dueño de cada subdiálogo es la propia ventana (`Owner = this`, HT2-17), con el código de hoy;
  - el host decide el dueño **de la ventana**: en CLASSIC, los cuatro sitios de hoy; en WORKSPACE, PB-T4 (`ForDraft` no pone ninguno).
- **Por qué no un dueño inyectado:**
  - con el editor modal abierto, su dueño está deshabilitado;
  - rompería los literales `Owner = this` de CT-16, CT-20, S-7 y S-9 (OB-T1B.3).
- **Las pruebas:** CT-PBT2B-07, para las dos ventanas hijas de F4B-E23; las guardas de T1B, que no dependen del modo, para las demás.
- **Lo que hay que enmendar:** la letra de [P] §4 y §5 («el que decide el host») queda así; la enmienda es el arrastre A2-10 (v2, V2-12).

### D-PBT2-13. El paso 4 de D-PB-06 y el aviso de D-PB-08 en WORKSPACE (PB-T2c)

- **El paso 4** (OB-06.3): **CT-PBT2C-01**. Con los oráculos de CT-PBTC-02 y CT-PBTC-05, un borrador con los valores exactos o con el estilo nombrado pasa por la ventana en WORKSPACE, recibe un gesto neutro y se aplica. El estado aplicado y su diseño llevan el exacto o el estilo. **El tramo hasta el DWG** (PREPARE → MUTATE → POST) es el arrastre A2-11 (v2, V2-14).
- **El aviso** (OB-08.2): **CT-PBT2C-02**, con las líneas de CT-PBTC-07 y una línea de aplicación.
- **El censo en WORKSPACE:** **CT-PBT2C-03**.

### D-PBT2-14. D2 en PB-T2

| Condición | Qué hace PB-T2 |
|---|---|
| 2. sin alias fuera de D2 | CT-PBT2A-03 y CT-PBT2B-04: el estado del borrador no comparte con S ni con la ventana nada mutable fuera de las dos referencias |
| 3. «DraftSession editing does not mutate either shared object», en el camino del puerto (v2, V2-17) | CT-PBT2A-03: huellas profundas de las cuatro referencias de S y del borrador antes y después de `ApplyToDraft`. **El resto** (editar en el `DraftSession` del panel) sigue arrastrado a PB-T4 |
| 4. «validation does not mutate either shared object» | CT-PBT2B-05: huellas de las referencias de la ventana y del borrador durante los gestos de validación, el BOM, la biblioteca y una aplicación |
| 10. guardas | CT-PBT2A-04 (G-L1 sobre los dos archivos de PB-T2a) y CT-PBT2B-09 (sobre el archivo del modo), con las quince autopruebas de G-L1c (v2, V2-01). W y WC siguen bajo CT-PBT1C-04, sin tocarla. Esa guarda lleva dieciséis: las quince comunes más la (d), propia de la ventana ([PT1] v2.8, D-PBT1-15; F4B-E27). Las de PB-T2 miran archivos que no son la ventana y llevan las quince (v2.1, V21-06) |
| 5, 6 y 7 | Siguen arrastradas a PB-T4 (A-2) |
| 8. el ajuste de `Name` | WORKSPACE no tiene ningún escritor del Plugin. CT-PBT1B-15 sigue en verde sin tocarla |
| El límite | PBT2A-C17, PBT2B-C17 y PBT2C-C17 |

### D-PBT2-15. Lo que no hace PB-T2

- «Editar todo», el host, la modalidad, el texto pendiente y K = 1: PB-T4 (OB-01.4, R-PB-10).
- La escritura completa, la barrera de PREPARE y «Conservar mi borrador»: PB-T3 y PB-T4.
- El piloto (`Pilot/`), `WorkspaceHostCore`, el tipo del borrador del panel y el campo de escala del panel: PB-T4.
- No se tocan el núcleo de borradores, WX, WC (salvo los mutantes de PB-T2c, que se revierten), el Plugin, `EditorModules`, los constructores actuales ni los archivos de PB-T1b.
- Ni los rechazos A/B o de cola (OB-03.2), ni AUTH-13, ni ninguna corrección de RACKEDITAR (OB-RE.1).

### La tabla del §4 de [P], con el diseño de PB-T2

| | CLASSIC | WORKSPACE |
|---|---|---|
| Quién la abre | RACKEDITAR, RPB y el menú, con los constructores de hoy | Solo `RackPushBackSystemWindow.ForDraft(launch)`. En F4B, el «Editar todo» del panel (PB-T4) |
| De dónde carga | `LoadExisting`, `LoadDesignForNew` o `LoadNew` | Una copia del estado del borrador, sin pasar por un diseño. Las cargas públicas lanzan |
| Botón principal | «Actualizar» e «Insertar», sin cambios | «Aplicar al borrador». Insertar, los cuatro por vista e «Insertar varias…» no se muestran |
| Qué hace | Aplica al estado de edición (el paso extraído), anota la intención y cierra. Escribe quien llamó | Aplica al estado de edición (el mismo paso) y devuelve una copia al borrador por el puerto. No cierra. Ninguna escritura en el DWG |
| Cerrar sin aplicar | Como hoy: solo pregunta por el módulo escenificado | Pregunta si hay cambios sin aplicar (Q-PBT2-1). El borrador queda como estaba |
| BOM y biblioteca | Como hoy | Los mismos botones, con la misma compuerta y el mismo dueño. Dos salidas a disco aceptadas (F4B-E23) |
| Dueño de los subdiálogos | La ventana | La ventana. El dueño de la ventana lo decide su host (D-PBT2-12) |
| Guardas | T1B en verde | CT-PBT2B-03 (UI-STA de OB-01.3) y CT-PBT2B-08 (texto). PB-T4 añade la guarda del abridor |

## 4. Las guardas de T1B y de texto (OB-T1B.3), una por una

**Conclusión:** el diseño deja en verde **todas** las guardas sin sustituir ninguna (lo confirma IR1 §3). OB-T1B.3 no obliga a volver al Owner. Las marcadas [T1c] se vuelven a comprobar sobre el cierre de PB-T1c.

| Guarda | Qué fija | ¿La toca PB-T2? | Cómo queda en verde |
|---|---|---|---|
| T-SG CT-16 (`:217-231`) | El configurador con `{ Owner = this }` y `window.Configuration ?? copy` | No | `ShowHeaderConfigurator` no cambia |
| T-SG CT-19 (`:236-248`) | Ningún token de teclado, foco ni `DialogResult` en `Systems/PushBack` | Sí (código nuevo y un archivo nuevo en la clausura) | Ningún token en el código nuevo ni en `PushBackEditorMode.cs` (INV-PBT2B-04) |
| T-SG `InsertRequestedRaised` (`:251-264`) | Nadie se suscribe | No | Ningún `+=` nuevo |
| T-SG S-5 y S-6 (`:269-278`) | Los dueños de la defensa y los topes | No | No se tocan |
| **T-SG S-7** (`:281-284`) | La línea del BOM | **Sí** | La línea sigue, byte a byte, tras el ramal de la costura |
| T-SG S-8 (`:287-291`) | El lote con `TryShow(this, …)` | No | `InsertBatch_Click` solo gana la guarda como primera sentencia |
| **T-SG S-9** (`:294-298`) | `UiSupport.PromptSaveToLibrary(this, NameBox.Text, "sistema")` | **Sí** | El literal sigue, en el segundo término de la condicional |
| T-SG CT-21 (`:303-319`) | El host del menú | No | `EditorModules` no se toca |
| **T-SG CT-22** (`:324-341`) | El orden de las cinco sentencias de la biblioteca | **Sí** | El `IndexOf` del prompt sigue antes de `EnsureId`, que no se mueve |
| T-SG CT-22b (`:344-352`) | El configurador guarda su disposición al cerrarse | No | No se toca (L-PBT2-11) |
| **T-SG CT-23** (`:357-379`) | `InsertBatch_Click` | **Sí** | La guarda va primero y no contiene `canInsertInAutoCad` ni `Close();`. La confirmación sigue antes del diálogo |
| T-PG CT-24 a CT-27 | RPB, RACKEDITAR, el menú y las fachadas | No | El Plugin no se toca |
| T-KB, T-OW, T-RS, foco y `AnnotationScaleBox` | WX y los dueños en CLASSIC | No | WX no se toca. La ocultación es solo de WORKSPACE y va en código |
| T-CL | Los caminos de cierre en CLASSIC | Sí (`PendingWork`) | En CLASSIC da lo mismo: la expresión del módulo es literal y el segundo ámbito es `false` |
| T-IN | La intención de «Actualizar» e «Insertar» en CLASSIC | Sí (`RequestDraw`) | La extracción conserva el orden, los textos y el efecto |
| `RichEditorCloseContractTests.cs:69-86` | Solo Push Back y la cabecera declaran `OnClosing`; sin dirty global en el producto | Sí (`PendingWork`) | `OnClosing` no cambia. El ámbito nuevo solo existe en WORKSPACE (Q-PBT2-1) |
| **G-10** (`WorkspacePilotUpdateIntegrationGuardTests.cs:725-767`, sobre `Src.AllProduct()`) | Una lista cerrada de los que llaman a `.RequestUpdate(`, con una llamada en W | Sí | W conserva la suya en `RequestDraw`. Ningún archivo nuevo, de Application ni de UI, la contiene. El método nuevo se llama `ApplyToDraft` |
| G9 (`:703-719`) | Ningún archivo **de producto** que nombre el piloto usa una API de escritura | No | Los archivos nuevos de `src/` no nombran el piloto. La clase de CT-PBT2B-10, que lo nombra, es una prueba, y G9 no mira las pruebas |
| I-55 (`I55DeferredProductFixCharacterizationTests.cs:8-15`), Fact3, desviador | Literales de W | No | No se tocan. Ninguna línea nueva lleva `forceRebuild` |
| Reflexión de la vista previa (`PushBackRichPreviewTests.cs:349-367`) | Ni `DrawModel` ni `RoleBrush`; un `EditorPreviewSurface`; ningún `PreviewCanvasPainter` | Sí | Los nombres nuevos de métodos y los tipos de los campos nuevos no son ninguno de esos |
| Censo de ventanas | Ningún `Window` nuevo | No | Ninguno |
| Corpus de WLC; RAM Elements | Las restricciones | Restricción | Los diseños nuevos se hacen en código; ninguna palabra partida |
| Núcleo de borradores (`WorkspaceDraftBoundaryTests.cs`; `WorkspaceDraftPilotIntentGuardTests.cs:233-244`) | `Drafts/` sin sistemas, con sus archivos esperados y sin nombres prohibidos | Restricción | `Drafts/` no se toca |
| Las del puerto del panel (`WorkspaceEditIntentHostTests.cs:1030-1046`, `WorkspaceLifecycleHostTests.cs:1477-1480`, `WorkspacePilotEditGuardTests.cs:385`) | Los valores del puerto del panel y las interfaces del host core | No | No se tocan (A2-1 lo deja a PB-T4) |
| `NamespaceFolderGuardTests` | Un `namespace` de bloque por archivo de `src/` | Restricción | INV-PBT2A-04 e INV-PBT2B-04 |
| El censo del piloto (HT1-21 de [PT1]) | Todo campo de instancia de su grafo | Restricción | Ningún campo nuevo en los tipos del censo |
| `PushBackLibrarySaveGateTests` (`:95`) | La compuerta, que llega al diálogo real | Sí (`SaveLibrary_Click`) | La costura es nula en esas pruebas |
| `DialogWindowCharacterizationTests` | La ventana del BOM compartida | No | `RackBomWindow` no se toca |
| Las costuras de la ventana [T1c] | 40 + `EditableStateForTest` | Sí (añade tres) | Se conservan las 41; se añaden `ModeForTest`, `BomDialog` y `LibraryPathDialog` |
| CT-PBT1C-01 a 04 [T1c] | La ventana sobre el estado, el censo, la forma de W y G-L1 sobre W y WC | Sí (W) | `editable` sigue declarado como en D-PBT1-13 (1). `LoadFromModel` y `ReadInputs` no cambian. Ninguna línea nueva muta las dos referencias. **Se comprobó en los anclajes de PB-T2a** (F4B-E32) |
| CT-PBT1B-14 y 15 (G-L1c) | G-L1 sobre Application y los escritores de `Name` del Plugin | No | Sus archivos no se tocan. Los nuevos tienen su propia guarda |

## 5. La red de pruebas

### 5.0 Recetas y ayudantes

**El régimen común,** copiado en cada contrato:
- clases `public sealed`;
- en UI, `[Collection(EditorDiscardPromptCollection.Name)]`, en el hilo STA y con la cultura invariante (`S0.Sta`);
- `[Fact]` que acaban en **un** `Assert.Equal(lo esperado, las líneas)`;
- «R», GUID fijo y orden ordinal;
- la regla «EXCEPCION» de [PT1] §5.0;
- ventanas **sin mostrar**, con la carga dentro del `try` y `Cerrar` en el `finally`;
- el aviso de descarte siempre sustituido, con el sufijo ` descartes=<n>` en la última línea de cada ventana (la regla, abajo);
- sin teclado ni foco real; sin `MessageBox` real, ni el configurador, «Insertar», la ventana del BOM o el diálogo de guardado reales.

**El sufijo ` descartes=<n>`** (v2.1, V21-02; IR1-R N-02). Es normativo en PB-T2b y PB-T2c.
- **Cómo se forma:** la última línea de cada ventana se arma antes de cerrarla; después se llama a `Cerrar(w)` dentro del `try` y se añade ` descartes=` + el contador del aviso sustituido. Es el patrón de `PushBackDeclaredCorrection2MixedBoxTests.cs:94-96`. El `Cerrar` del `finally` queda como red de seguridad.
- **Qué cuenta:** `n` es el número de veces que se mostró el aviso de descarte en esa ventana, contando el `Cerrar` explícito. El sustituto contesta «Sí», salvo en las ventanas con un `PromptProbe` (CT-PBT2B-04 y 06), que dicen su respuesta en cada paso.
- **Cuánto vale:** con Q-PBT2-1 (A), cada `Cerrar` sin «No» muestra el aviso a lo sumo una vez, y lo muestra si y solo si en ese momento hay un módulo escenificado o, en WORKSPACE, el estado difiere del último aplicado (`PendingWork`, D-PBT2-07). En CLASSIC, solo cuenta el módulo escenificado.
- **El valor de cada ventana** lo da cada CT. Sale de la regla y de lo que fijan los oráculos de cada gesto: un gesto cuyo original fija un cambio en la salida deja el estado distinto de W0. Si una ventana da otro valor, el Worker no ajusta la línea: para y lo entrega (C07).
- **En CT-PBT2B-06,** el campo `avisos=<n>` de la última línea de cada ventana es ese mismo contador, y no lleva el sufijo aparte.

**Las recetas** (en `PushBackWorkspaceSupport`, `internal static`, sin pruebas, para PB-T2b y PB-T2c; copiadas como privadas en las clases de PB-T2a):
- **`Catalogo`** = `JsonRackCatalogProvider.FromBaseDirectory().Load()`.
- **`Pasada(s, frontal)`:** el ayudante de [PT1] §5.0, copiado.
- **`Estado(sobre)`:** `var s = new PushBackEditableState(); s.LoadNew(cat); Pasada; s.SeedDefaultSafety(cat); Pasada; s.LoadDesign(<diseño del sobre>, <nombre del sobre>, resolver); Pasada`. **X-01 (Core)** y **F-01 (Core)** son las recetas de [PT1] §5.0.
- **`EstadoDeVentana(gestos)`** (v2, V2-09): una ventana CLASSIC `S0.Ventana()`, `Cargar`, los gestos indicados y `Copy(w.EditableStateForTest)` [T1c].
- **`Borrador(s)`:** con `registry = new DraftRegistry()`, `registry.Begin(new SessionId(64), "GUID-PBT2-0001", DraftIntentRead<PushBackEditableState>.Admitted(DraftStaleBase.Create(new[] { new DraftStaleBaseEntry("BLK-PBT2", DraftMembership.Mutable, "planta", null, "{}") }), s), PushBackEditableStateComparer.Instance, PushBackEditableStateComparer.Copy)`. Devuelve el registro y su `Draft`.
- **`PuertoContado`:** un `IPushBackDraftStatePort` privado de la prueba que cuenta las llamadas y delega en `new PushBackDraftSessionPort(draft)`.
- **`VentanaBorrador(draft, proyecto, puerto)`:** `RackPushBackSystemWindow.ForDraft(new PushBackDraftEditorLaunch(draft.CurrentDraftState, draft.RackId, proyecto, puerto))`, con su propio registro de `Closed`, como `S0.Ventana`.
- **`Sondeo(w)`, la sonda del modo** (obligatoria): la primera línea de cada prueba de WORKSPACE es `modo=<w.ModeForTest>`. Si no es `Workspace`, la prueba **no hace ningún gesto más** y termina con su `Assert.Equal`.
- **`ProyectoConDesconocido(sobre)`:** el JSON del proyecto interior con `"CampoFuturoPbt2": { "v": 1 }` en la raíz, deserializado con `new RackProjectStore().Deserialize(...)`.
- **`ArchivoTemporal`:** `Path.Combine(Path.GetTempPath(), "rackcad-pbt2-0001", "biblioteca.rackcad.json")`. La carpeta se crea antes del gesto y se borra en el `finally`. **Es la única escritura en disco de la red**, y ninguna línea lleva la ruta.
- **`Huella(objeto)`:** `Fingerprint` (`:1080`) y `AllObjects` (`:278`, con `skip`) de `95373af6:tests/RackCad.Tests/PushBackEditableStateCopyTests.cs`, copiados (v2, V2-01).
- **`Recalcular(w)`** (v2.3, V23-02): un recálculo sin editar ningún campo, `Field(w, "AnnotationScaleBox").RaiseEvent(new RoutedEventArgs(UIElement.LostFocusEvent, Field(w, "AnnotationScaleBox")))`, copiado de `PushBackWindowOnStateTests.cs:449-450` (CT-PBT1C-02, la corrección C-1). No cambia el estado, así que no deja nada sin aplicar.
- **`G-L1(archivos)`:** `Tokens`, `Rebind`, los patrones, `Rule1`, `Rule1b`, `Rule1c`, `Clean`, `Collapse`, `Members`, `Aliases`, `Violations` e `Inject` de G-L1c (`:33-210`), copiados. Las quince autopruebas son sus líneas `:223-239`, inyectadas en el texto de cada archivo nuevo con `Inject` (v2, V2-01).
- **`Igual(a, b)`** = `PushBackEditableStateComparer.Instance.Equals(a, b)`. **`DisenoDe(estado, frontal)`** = `Persist(Pasada(Copy(estado), frontal).Design)`.
- **De S0 y SC1, sin tocarlos:** `Fixture`, `CorpusNames`, `EditInner`, `Persist`, `Field`, `Check`, `Combo`, `Btn`, `StatusText`, `Click`, `SetNumberR`, `FilasF`, `VarianteDe`, `Num`, `Texto`, `Cargar` y `Aviso`.
- **El corpus «C18»** (v2, V2-10): los 17 de `Fixtures/I64` **sin `pb-21-cola`** (fuera de la garantía, D-PBT1-10), más X-01 y F-01. Son 18.

### 5.1 Lo que sigue en verde sin tocarlo

- T1B (55 de UI y 10 de Core).
- PB-T0 (16 y 3, con SUS-03 y SUS-04 en `Skip`), PB-TC (10) y PB-TC2 (4).
- PB-T1a (14), PB-T1b (Core 13 + 1 + 2 y UI 1, en su versión cerrada) y PB-T1c (UI 2 y Core 2) [T1c].
- Las familias `PushBack*` de UI.
- `RackCad.Tests.Workspace` (3 271) y `RackCad.UI.Tests.Workspace` (547).
- Cantilever (67), `BlankFront` (33), `WindowCensusGuardTests` (8), `RichEditor` (25), `RichEditorCloseContractTests`, `PushBackRichPreviewTests` (9), `PushBackLibrarySaveGateTests` (5), `DialogWindowCharacterizationTests` (23), `I55DeferredProductFixCharacterizationTests` (3) y `NamespaceFolderGuardTests` (7).
- Cada subtarea añade las anteriores de PB-T2.

### 5.2 PB-T2a: el puerto (lista cerrada CT-PBT2A-01 a 04)

**Régimen:** el RED es el **esqueleto inerte** y las dos clases nuevas, que fallan por aserción salvo la guarda CT-PBT2A-04, que pasa en el RED y discrimina por sus autopruebas (INV-F4T3A-13). El GREEN solo toca los dos archivos de `src/`.

**El esqueleto exacto:**
- `PushBackDraftApplyStatus`, `PushBackDraftApplyResult` e `IPushBackDraftStatePort`, completos;
- `PushBackDraftSessionPort`: el constructor no guarda ni comprueba nada; `ApplyToDraft` devuelve `new PushBackDraftApplyResult(PushBackDraftApplyStatus.NotEditable, false)`; `LaunchFor` devuelve `null`;
- `PushBackDraftEditorLaunch`: el constructor no asigna ni comprueba nada.

**Las clases** (`tests/RackCad.Tests/`, `namespace RackCad.Tests`): `PushBackDraftStatePortTests` (01, 02 y 03) y `PushBackDraftPortSharedReferenceGuardTests` (04).

| CT | Qué fija (las líneas) | Falla en el RED porque |
|---|---|---|
| **01** `CT_PBT2A_01_ThePortHandsACopyOfTheEditedStateToTheDraftAndNothingElse` | Sobre `Borrador(Estado(Fixture("pb-01-base")))`, con `antes = Copy(draft.CurrentDraftState)`:<br>- `inicio sucio=False`;<br>- con `e = Copy(draft.CurrentDraftState)` y `e.PalletTolerance = 4.5`, `ApplyToDraft(e)` → `aplicar estado=Applied sucio=True igual-al-editado=True mismo-objeto=False inicial-intacto=True`;<br>- tras `e.PalletTolerance = 9.0` → `mutar-tras-aplicar sin-cambio=True`;<br>- `ApplyToDraft(Copy(antes))` → `volver estado=Applied sucio=False`;<br>- con `draft.EnterError("prueba")` → `error estado=NotEditable sin-cambio=True`;<br>- en otro borrador, descartado con `registry.Discard` → `descartado estado=NotEditable`;<br>- `new PushBackDraftSessionPort(null)` → `EXCEPCION ArgumentNullException: Value cannot be null. (Parameter 'draft')`;<br>- `ApplyToDraft(null)` → `EXCEPCION ArgumentNullException: Value cannot be null. (Parameter 'edited')` | el esqueleto devuelve `NotEditable` y no lanza |
| **02** `CT_PBT2A_02_TheLaunchCarriesTheStateTheIdentityTheSourceAndThePortAndRefusesWhatIsMissing` | `new PushBackDraftEditorLaunch(s, "GUID-PBT2-0001", p, puerto)` → `lanzamiento estado-mismo=True id=GUID-PBT2-0001 proyecto-mismo=True puerto-mismo=True`. Con el proyecto nulo → `proyecto=<null>`. Las excepciones (v2, V2-15):<br>- `state` nulo → `EXCEPCION ArgumentNullException: Value cannot be null. (Parameter 'state')`;<br>- `rackId` nulo, `""` y `"   "` → `EXCEPCION ArgumentException: A rack id is required. (Parameter 'rackId')`, tres veces;<br>- `port` nulo → `EXCEPCION ArgumentNullException: Value cannot be null. (Parameter 'port')`.<br>`LaunchFor(draft, p)`, leída dentro de «EXCEPCION» → `launchfor estado-es-el-actual=True id=GUID-PBT2-0001 puerto-es-del-borrador=True`; la última comprueba que aplicar por ese puerto cambia ese borrador. `LaunchFor(null, p)` → `EXCEPCION ArgumentNullException: Value cannot be null. (Parameter 'draft')` | el esqueleto no guarda ni comprueba nada (en el RED, `LaunchFor` da nulo y su lectura da `EXCEPCION NullReferenceException: …`) |
| **03** `CT_PBT2A_03_ThePortRoundTripIsLosslessAndSharesNothingOutsideTheTwoDeclaredReferences` | Para el corpus C18, en el orden de [PT1] CT-PBT1B-02:<br>1. `s = Estado(…)` y `d = Borrador(s)`;<br>2. `otro = Copy(s)` con `otro.PalletTolerance = s.PalletTolerance + 1.0`, puesto con `d.SetCurrentDraftState(otro)`, en una llamada directa al núcleo;<br>3. **las huellas** (v2, V2-17) de los no nulos de las cuatro referencias de D2 de `s` y de `d.CurrentDraftState`, capturadas antes del paso 4;<br>4. `ApplyToDraft(Copy(s))` por el puerto;<br>5. la línea `<diseño> iguales=<Igual(d.CurrentDraftState, s)> diseno-igual=<DisenoDe(d.CurrentDraftState, A) == DisenoDe(s, A)> compartidos-fuera-de-D2=<n> huellas-iguales=<b>`.<br>`n` cuenta los objetos mutables comunes a `d.CurrentDraftState` y a `s`, quitados los no nulos de las cuatro referencias y lo que alcanzan (como CT-PBT1B-07, parte 1). Lo esperado, en los 18: `iguales=True diseno-igual=True compartidos-fuera-de-D2=0 huellas-iguales=True` | el esqueleto no aplica: el borrador sigue en `otro` |
| **04** `CT_PBT2A_04_NoFileOfThePortMutatesTheResolvedRackOrTheModuleReport` | **D2, condición 10.** `G-L1` sobre los dos archivos nuevos, con las **quince** autopruebas de G-L1c inyectadas en el texto de cada uno (v2, V2-01). Las líneas: `reglas-sin-coincidencias=True`, y `autoprueba-<x>=falla` para (a), (b), (c), (e), (f1), (f2), (f3), (g1), (g2), (h1), (h2), (i), (j), (k1) y (k2) | **no falla en el RED** (`ExpectRed` false) |

### 5.3 PB-T2b: la ventana en dos modos (lista cerrada CT-PBT2B-01 a 10)

**Régimen:** el RED es la **costura inerte** y las clases nuevas:
- las de UI fallan por aserción gracias a la sonda del modo;
- CT-PBT2B-08 falla porque sus reglas no se cumplen;
- CT-PBT2B-09 y 10 pasan en el RED y discriminan por sus autopruebas.

El GREEN toca solo W y el archivo del modo.

**La costura inerte exacta** (en W, y nada más de W):
- `internal PushBackEditorMode ModeForTest => PushBackEditorMode.Classic;`;
- `public static RackPushBackSystemWindow ForDraft(PushBackDraftEditorLaunch launch) => new RackPushBackSystemWindow(canInsertInAutoCad: false);`;
- `internal Action<RackCad.Application.Bom.BillOfMaterials, Window> BomDialog;` e `internal Func<Window, string, string> LibraryPathDialog;`, declaradas y sin usar;
- `PushBackEditorMode.cs`, completo.

**Las clases:**
- en `tests/RackCad.UI.Tests/`: `PushBackWorkspaceSupport` (el ayudante), `PushBackWorkspaceModeTests` (01, 02 y 07), `PushBackWorkspaceApplyCloseTests` (03 a 06) y `PushBackFieldValidationConcordanceTests` (10);
- en `tests/RackCad.Tests/`: `PushBackWorkspaceModeSourceGuardTests` (08) y `PushBackWorkspaceSharedReferenceGuardTests` (09).

| CT | Qué fija | Falla en el RED porque |
|---|---|---|
| **01** `CT_PBT2B_01_TheModeIsExplicitAndFixedAndClassicDoesNotChange` | **(i) CLASSIC:** con `S0.Ventana()` y `Cargar(pb-01-base)`, `new RackPushBackSystemWindow()` y `new RackPushBackSystemWindow(false, null)`, la línea `clasico modo=Classic boton=Actualizar ocultos=[]`, con los seis botones de inserción en `Visible`.<br>**(ii) WORKSPACE:** `VentanaBorrador` sobre `Estado(pb-01-base)` → `modo=Workspace boton=Aplicar al borrador tooltip=<WorkspaceApplyToolTip> ocultos=[InsertButton,InsertLateralButton,InsertFrontalEntradaButton,InsertFrontalPosteriorButton,InsertPlantaButton,InsertBatchButton] visibles=[BomButton,SaveLibraryButton,RestoreButton,CloseButton] dueno-de-la-ventana=<null>`.<br>**(iii)** En WORKSPACE, `LoadExisting(…)`, `LoadDesignForNew(…)` y `LoadNew()` → `EXCEPCION InvalidOperationException: La ventana del borrador solo carga el estado del borrador.` cada una, y después `modo=Workspace`.<br>**(iv)** `ForDraft(null)` → `EXCEPCION ArgumentNullException: Value cannot be null. (Parameter 'launch')`.<br>**`descartes`** (v2.1, V21-02): ` descartes=0` en la última línea de cada ventana, CLASSIC y WORKSPACE, porque ninguna edita | la sonda da `Classic` |
| **02** `CT_PBT2B_02_WorkspaceLoadsACopyOfTheDraftStateWithoutGoingThroughADesign` | **El corpus C18:** `<diseño> iguales=<Igual(w.EditableStateForTest, draft.CurrentDraftState)> mismo-objeto=<ReferenceEquals(…)> diseno-igual=<DisenoDe(draft.CurrentDraftState, A) == Persist(w.LastComputation.Design)> id=GUID-PBT2-0001 nombre=<RackName> peticion=False`. Toda línea con `diseno-igual` de este CT se toma tras un recálculo neutro (`Recalcular(w)`, §5.0; v2.3, V23-02), porque el diseño de `LastComputation` del compuesto puede ir una pasada por detrás del estado (F4B-E30). Lo esperado es `iguales=True mismo-objeto=False diseno-igual=True`, **salvo** los diseños que el paso 1 identifique con seguridad no autorizada, que dan `iguales=False normalizacion=seguridad diseno-igual=True` (N-PBT2-01 (b)).<br>**N-PBT2-01 (a):** `Estado(pb-01-base)` con `DimensionsLateral = false` y `DimensionViewsTouched = true` → `N-PBT2-01a iguales=False tocado=True->False vistas=<a>-><b> diseno-igual=True`, con `<a>` y `<b>` del paso 1.<br>**La reapertura con selección** (v2, V2-09; `EstadoDeVentana`):<br>- `ambos-mixto` (`pb-20-ab`: `SideSelectorBox` a 1, `SetNumberR(CellPalletHeightBox, 55)`, `Click(ApplyCellButton)`, `SideSelectorBox` a 2);<br>- `lado-B` (`pb-20-ab`: `SideSelectorBox` a 1);<br>- `celda-1-0` (`pb-08-redondeo`: `w.SelectMatrixCell(1, 0, false)`).<br>Por cada una, `<fila> iguales=True diseno-igual=True seleccion=<ActiveSelection>/<SelectedFrontIndex>,<SelectedLevelIndex del lado activo>`, con `Both/…`, `B/…` y `A/1,0`.<br>**El nombre** (v2, V2-07), sobre `Estado(pb-01-base)` con `Name = null`, y con `Name = "  PB  Uno  "` → `nombre-nulo iguales=True nombre=<null>` y `nombre-con-espacios iguales=True nombre=[  PB  Uno  ]`.<br>**Un estado inválido** (v2, V2-22): `Estado(pb-01-base)` con `Advanced.DerivedPostReinforced = true` y `Advanced.DerivedPostReinforcementHeight = 100000` → `estado-invalido valido=False restaurar-habilitado=False tras-restaurar=<StatusText tras Click(RestoreButton)> llamadas-tras-aplicar=0`, con `Todavía no hay un sistema válido que restaurar.`<br>**`descartes`** (v2.1, V21-02): cada fila es una ventana de WORKSPACE, y su línea acaba en ` descartes=0`. La carga fija W0 como lo aplicado, y nada se edita después: en `estado-invalido`, «Restaurar» y «Aplicar» se niegan sin tocar el estado. Las ventanas CLASSIC de `EstadoDeVentana` son de la receta: se cierran dentro de ella, sin línea propia y sin ningún módulo escenificado | la sonda da `Classic` |
| **07** `CT_PBT2B_07_InWorkspaceTheWindowOwnsTheBomAndTheSaveDialog` | En WORKSPACE, con las dos costuras puestas:<br>- «BOM» → `BomDialog llamadas=1 dueno-es-la-ventana=True bom-es-el-calculado=True`;<br>- «Guardar en biblioteca», con la costura devolviendo `null` → `LibraryPathDialog llamadas=1 dueno-es-la-ventana=True nombre=<NameBox.Text> archivo=False peticion=False descartes=0`. Con la ruta nula, `SaveLibrary_Click` vuelve antes de `SetName` (W:3384), y el estado no cambia.<br>Los demás subdiálogos los fijan las guardas de T1B, que no dependen del modo | la sonda da `Classic` |
| **03** `CT_PBT2B_03_NoWorkspaceGestureFlagsAClassicRequestAndBomAndLibraryLeaveTheDraftAndItsIdentityAlone` | **La UI-STA de OB-01.3, con las cuatro comprobaciones de F4B-E23.** Tres pasadas, en seis ventanas de WORKSPACE, cada una con su propio `Borrador(Estado(<fixture>))`, su `PuertoContado` y su `antes = Copy(draft.CurrentDraftState)`. **Las líneas esperadas, literales y en orden, van en la lista de debajo de la tabla** (v2.1, V21-02 y V21-03).<br>Los campos comunes:<br>- `peticion` = `w.InsertRequested`;<br>- `solicitud` = `w.InsertionRequest`, `<null>` si es nula;<br>- `borrador-igual` = `Igual(draft.CurrentDraftState, antes)`;<br>- `llamadas` = las del puerto contado;<br>- `dialogos` = las de cada costura;<br>- `campo-desconocido` lee `"CampoFuturoPbt2"` en el texto del archivo.<br>Las botas por poste (CT-PB0-14) necesitan la ventana mostrada: no se ejercen (D-PBT2-08) | la sonda da `Classic` |
| **04** `CT_PBT2B_04_ApplyToDraftHandsTheEditedStateToTheDraftOnceAndKeepsTheWindowOpen` | Sobre `Estado(pb-20-ab)`:<br>- `carga llamadas=0 sucio=False`;<br>- `SetNumberR(ToleranceBox, 4)` → `editar aplicar-habilitado=True tooltip=<WorkspaceApplyToolTip>`;<br>- «Aplicar» → `aplicar estado=<WorkspaceAppliedMessage> llamadas=1 sucio=True igual-a-la-ventana=True mismo-objeto=False compartidos-fuera-de-D2=0 diseno-igual=True abierta=True peticion=False`;<br>- «Aplicar» sin cambios → `otra-vez estado=<WorkspaceNothingToApplyMessage> llamadas=1`;<br>- `SetNumberR(PalletsDeepBox, 3)` y «Aplicar» → `segunda llamadas=2 igual-a-la-ventana=True diseno-igual=True`;<br>- `draft.EnterError("prueba")`, `SetNumberR(ToleranceBox, 5)` y «Aplicar» → `error estado=<WorkspaceNotEditableMessage> llamadas=3 sin-cambio=True abierta=True descartes=1`. La tolerancia 5 queda sin aplicar al cerrar (v2.1, V21-02).<br>**El nombre** (v2, V2-07): en ventanas nuevas sobre los estados `nombre-nulo` y `nombre-con-espacios` de CT-02, «Aplicar» sin editar → `nombre-nulo aplicar-sin-editar llamadas=0 sucio=False estado=<WorkspaceNothingToApplyMessage> descartes=0`, y lo mismo con `nombre-con-espacios`.<br>**El nombre tras la biblioteca** (v2.1, V21-01; IR1-R N-01; L-PBT2-12) [T1c]. En dos ventanas nuevas, sobre los mismos dos estados, con `ArchivoTemporal` y un `PromptProbe` (T-CL) que contesta «No» y después «Sí»:<br>- la costura de la biblioteca devuelve `ArchivoTemporal` (con `null` no se llega a `SetName`, W:3384) y se pulsa `SaveLibraryButton` → `nombre-nulo-biblioteca archivo=True nombre-ventana=[] llamadas=0 sucio=False`;<br>- «Cerrar» con «No» → `cerrar avisos=1 pregunta=Hay cambios sin aplicar al borrador que se perderán al cerrar. ¿Deseas continuar? cerrada=False`;<br>- «Aplicar» → `aplicar estado=<WorkspaceAppliedMessage> llamadas=1 sucio=True nombre-borrador=[]`;<br>- `Cerrar` → `cerrada=True descartes=1`.<br>Con `nombre-con-espacios`, lo mismo, con `nombre-con-espacios-biblioteca`, `nombre-ventana=[PB  Uno]` y `nombre-borrador=[PB  Uno]`. Los nombres se escriben entre corchetes, y el nulo como `<null>` | la sonda da `Classic` |
| **05** `CT_PBT2B_05_WorkspaceAndClassicShareOneValidationAndNeitherMutatesTheSharedReferences` | **Una sola validación, y D2 (4).** Tres gestos, cada uno en una ventana CLASSIC nueva (`S0.Ventana()` + `Cargar`) y en una de WORKSPACE nueva, seguidos del clic en `UpdateButton`:<br>- **campo-en-error:** `pb-01-base`, `Field(ToleranceBox).Text = "abc"`;<br>- **calculo-invalido:** `pb-01-base`, el refuerzo de F-03;<br>- **bloqueante:** `pb-20-ab`, `w.CompositeState.SetGap(-1)` y un `LostFocus` en `AnnotationScaleBox`.<br>Por gesto, `<g> clasico valido=<…> bloqueado=<…> estado=<StatusText> peticion=<…>` y `<g> borrador valido=<…> bloqueado=<…> estado=<StatusText> llamadas=<n>`. Lo esperado (según Q-PBT2-2, A):<br>- `campo-en-error` y `calculo-invalido`: `clasico valido=False bloqueado=False estado=Corrige los datos: no se puede insertar un modelo inválido. peticion=False` y `borrador valido=False bloqueado=False estado=<WorkspaceInvalidMessage> llamadas=0`;<br>- `bloqueante`: `clasico valido=True bloqueado=True estado=El hueco (-1") no es una separacion fisica valida: debe ser cero o positivo. Corrigelo; no se interpreta como otro valor. peticion=False`, y `borrador` con el mismo `estado` y `llamadas=0`.<br>**Las huellas,** como CT-PBT1C-02, parte 2: de las referencias de D2 del borrador (actual e inicial) y de las que adopta la ventana, a lo largo de los tres gestos de WORKSPACE y de una cuarta ventana de WORKSPACE (`pb-01-base`: el BOM, la biblioteca con la costura devolviendo `null`, `SetNumberR(ToleranceBox, 4)` y «Aplicar») → `huellas-iguales=True descartes=0`.<br>**`descartes`** (v2.1, V21-02), en la línea de cada ventana:<br>- las tres CLASSIC: `descartes=0`, sin módulo escenificado;<br>- en WORKSPACE, `campo-en-error`: `descartes=0`. Una caja en error no toca el estado (`Recompute`, W:550-560: «do NOT touch state»);<br>- `calculo-invalido` y `bloqueante`: `descartes=1`. El gesto cambia el estado, y «Aplicar» se niega;<br>- la cuarta ventana: `descartes=0`, porque acaba aplicada | la sonda da `Classic` |
| **06** `CT_PBT2B_06_ClosingAsksOnlyForWhatWasNotApplied` | **Según Q-PBT2-1, A.** Con `EditorDiscardPrompt.Substitute(probe)` (copia de `PromptProbe` de T-CL; el `finally` pone la respuesta en «Sí» antes de cerrar, como `CloseQuietly`, T-CL `:124-131`, v2, V2-16):<br>- `cerrar-sin-cambios avisos=0 cerrada=True borrador-igual=True`;<br>- `ToleranceBox=4` y «Cerrar» con «No» → `cerrar-con-cambios-no avisos=1 pregunta=<WorkspaceUnappliedQuestion> cerrada=False`;<br>- después, «Sí» → `cerrar-con-cambios-si avisos=2 cerrada=True borrador-igual=True llamadas=0`;<br>- editar, «Aplicar» y «Cerrar» → `cerrar-tras-aplicar avisos=0 cerrada=True llamadas=1`;<br>- `StageModuleEdit` y «Cerrar» con «Sí» → `cerrar-con-modulo avisos=1 pregunta=Hay cambios de módulo sin confirmar que se perderán al cerrar. ¿Deseas continuar? cerrada=True`;<br>- en CLASSIC, `clasico-con-cambios avisos=0 cerrada=True peticion=False` | la sonda da `Classic` |
| **08** (Core) `CT_PBT2B_08_NoWorkspacePathReachesTheClassicWritePath` | **La guarda de texto,** sobre el texto sin comentarios y con los espacios colapsados, partido en miembros con `Clean`, `Collapse` y `Members` de G-L1c. El nombre de un miembro es el identificador justo antes del primer `(` de su cabecera. **Un miembro que no existe da `regla-R<n>=False` y no lanza** (v2, V2-16).<br>- **R1 y R2:** los cuerpos de `RequestDraw` y de `InsertBatch_Click` empiezan por `{ if (RefuseInWorkspace()) return;`.<br>- **R3 (v2, V2-03):** en **todos** los `.cs` de `src/RackCad.UI/Systems/PushBack`, los miembros cuyo cuerpo contiene `.RequestUpdate(`, `.RequestInsert(` o `.RequestInsertViews(` son exactamente `RequestDraw` e `InsertBatch_Click`, de W.<br>- **R4:** el cuerpo de `ApplyToDraft` no contiene `RequestUpdate(`, `RequestInsert`, `RequestDraw(`, `Close();`, `SetModel(`, `InsertionRequest`, `InsertRequested`, `ShowDialog`, `SetName(` ni `SyncName(` (v2, V2-07), y contiene una vez `draftPort.ApplyToDraft(PushBackEditableStateComparer.Copy(editable))`.<br>- **R5:** `private readonly PushBackEditorMode mode;` una vez, y `mode = PushBackEditorMode.Workspace;` una vez, dentro de `private RackPushBackSystemWindow(PushBackDraftEditorLaunch launch)`, sin otra asignación.<br>- **R6:** las dos firmas públicas de hoy, literales; `ForDraft` presente; ningún parámetro de tipo `PushBackEditorMode` ni `bool` cuyo nombre contenga `workspace`, `draft` o `borrador`.<br>- **R7:** `if (BomDialog != null)` antes de S-7, y la sentencia de S-9 empieza por `var path = LibraryPathDialog != null ? LibraryPathDialog(this, NameBox.Text) :`.<br>- **R8:** `LoadNew`, `LoadDesignForNew` y `LoadExisting` empiezan por la guarda de WORKSPACE.<br>- **R9:** el cuerpo literal de `Update_Click` de D-PBT2-06.<br>- **R10:** las once constantes, cada una una vez, con su literal entero.<br>- **R11 (v2, V2-04):** `ApplyEditsToEditingState(` aparece como llamada solo en `RequestDraw` y en `ApplyToDraft`, y ni `ApplyToDraft` ni `HasUnappliedDraftChanges` contienen `AllFieldsValid(`, `OutputBlockedAfterRecompute(`, `currentInputsAreValid` ni `HasError`.<br>**Las autopruebas en negativo,** una por regla:<br>- R1: quitar la guarda de `RequestDraw`;<br>- R2: quitar la de `InsertBatch_Click`;<br>- R3: un miembro nuevo de **WC** con `session.RequestInsert(null, -1, ctx => null);`;<br>- R4: `SyncName();` dentro de `ApplyToDraft`;<br>- R5: `mode = PushBackEditorMode.Classic;` en `UpdateButtons`;<br>- R6: `bool workspace = false` en un constructor;<br>- R7: el ramal del BOM movido detrás de S-7;<br>- R8: la guarda de `LoadExisting` quitada;<br>- R9: `Update_Click` que llama a `RequestDraw` en WORKSPACE;<br>- R10: un literal cambiado;<br>- R11a: `if (!currentInputsAreValid) return;` dentro de `ApplyToDraft`;<br>- R11b: la llamada a `ApplyEditsToEditingState` quitada de `RequestDraw`.<br>Las líneas: `regla-R<n>=True` y `autoprueba-R<n>=falla` | las reglas no se cumplen en la base (`ExpectRed` true) |
| **09** (Core) `CT_PBT2B_09_TheNewFileOfTheModeMutatesNoSharedReferenceAndOnlyDeclaresTheEnum` | **D2, condición 10:** `G-L1` sobre `PushBackEditorMode.cs`, con las quince autopruebas inyectadas en su texto: `reglas-sin-coincidencias=True` y `autoprueba-<x>=falla`. **Y** (v2, V2-17) el texto del archivo, sin comentarios y colapsado, es exactamente `namespace RackCad.UI.Systems.PushBack { public enum PushBackEditorMode { Classic = 0, Workspace = 1 } }`: `solo-el-enum=True` | **no falla en el RED** (`ExpectRed` false) |
| **10** (UI) `CT_PBT2B_10_ThePanelScaleFieldAndTheWindowScaleBoxAcceptAndRejectTheSameTexts` | **Según Q-PBT2-3, A.** Para `""`, `"   "`, `"abc"`, `"0"`, `"-1"`, `"0,5"`, `"0.5"`, `"2.5"`, `"1e3"` y `"1.234,5"`, la línea `[<texto>] iguales=True`. Se comparan `new PushBackPilotFieldAdapter().Validate(PushBackPilotFieldAdapter.AnnotationScaleKey, texto)` (`IsValid` y `Message`) y `NumericFieldValidation.Validate(texto, false, false, 0.0, null, false, true)` (`!HasError` y `Message`), en la cultura invariante. La autoprueba: `autoprueba [0] inclusivo iguales=False` | **no falla en el RED** (`ExpectRed` false) |

**Las líneas esperadas de CT-PBT2B-03** (v2.1, V21-02 y V21-03), literales y en este orden. `<R>` es el texto de `WorkspaceRefusalMessage` («En el editor del borrador no se dibuja: aplica al borrador y actualiza desde el panel.»), y `<A>`, el de `WorkspaceAppliedMessage` («Cambios aplicados al borrador. El dibujo cambia con «Actualizar» del panel.»). La prueba los escribe enteros, y `<C>` abrevia aquí `peticion=False solicitud=<null>`.

- **Pasada 1:** una ventana sobre `Estado(pb-01-base)`, con `ProyectoConDesconocido(pb-01-base)`. Las costuras del BOM y de la biblioteca están puestas, y la de la biblioteca devuelve `ArchivoTemporal`. Los seis botones de inserción se pulsan con `Click` aunque estén ocultos.
  ```
  modo=Workspace
  carga id=GUID-PBT2-0001 <C> borrador-igual=True llamadas=0
  InsertButton visible=Collapsed estado=<R> <C> borrador-igual=True llamadas=0
  InsertLateralButton visible=Collapsed estado=<R> <C> borrador-igual=True llamadas=0
  InsertFrontalEntradaButton visible=Collapsed estado=<R> <C> borrador-igual=True llamadas=0
  InsertFrontalPosteriorButton visible=Collapsed estado=<R> <C> borrador-igual=True llamadas=0
  InsertPlantaButton visible=Collapsed estado=<R> <C> borrador-igual=True llamadas=0
  InsertBatchButton visible=Collapsed estado=<R> <C> borrador-igual=True llamadas=0
  BomButton dialogos=1 dueno-es-la-ventana=True <C> borrador-igual=True llamadas=0
  SaveLibraryButton dialogos=1 archivo=True campo-desconocido=True id=GUID-PBT2-0001 <C> borrador-igual=True llamadas=0
  RestoreButton <C> borrador-igual=True llamadas=0
  ToleranceBox=4 <C> borrador-igual=True llamadas=0
  BomButton dialogos=2 dueno-es-la-ventana=True <C> borrador-igual=True llamadas=0
  SaveLibraryButton dialogos=2 archivo=True campo-desconocido=True id=GUID-PBT2-0001 <C> borrador-igual=True llamadas=0
  UpdateButton estado=<A> id=GUID-PBT2-0001 abierta=True sucio=True <C> borrador-igual=False llamadas=1
  CloseButton cerrada=True <C> borrador-igual=False llamadas=1 descartes=0
  ```
- **Pasada 2** (v2.1, V21-03; IR1-R N-03): cuatro ventanas, cada una con **el fixture de su original** (`PushBackFullEditorCharacterizationSubdialogTests.cs`). Los gestos son los de los originales, copiados con su costura, y `SelectModule` viene de S0.
  - **ventana 2a,** `Estado(pb-31-v2-modulos)`, el CT-PB0-12 (`:38`): `SelectModule(w, "M3")` y la costura `HeaderConfiguratorDialog` devolviendo `null`, con un clic en `ConfigureModuleHeaderButton`; después, la costura devolviendo la copia con `PanelClear = 40.0` y otro clic, más `ConfirmModuleButton`;
  - **ventana 2b,** `Estado(pb-06-seguridad)`, el gesto (i) de CT-PB0-13 (`:104`): `SafetyDialog` quita `DEFENSA_MONTACARGAS`, con un clic en `SafetyButton`;
  - **ventana 2c,** `Estado(pb-06-seguridad)`, el CT-PB0-15 (`:200`): `DefenseDialog` con su lista de un poste;
  - **ventana 2d,** `Estado(pb-05-topes)`, el CT-PB0-16 (`:237`): `RearTopeDialog` con su `TopeResult`.

  Los originales fijan un cambio en la salida para el configurador confirmado, la seguridad, la defensa y los topes. Por eso, al cerrar, las cuatro ventanas tienen algo sin aplicar y dan `descartes=1`.
  ```
  configurador-nulo <C> borrador-igual=True llamadas=0
  configurador-confirmado <C> borrador-igual=True llamadas=0 descartes=1
  seguridad-aceptada <C> borrador-igual=True llamadas=0 descartes=1
  defensa-por-poste <C> borrador-igual=True llamadas=0 descartes=1
  topes-traseros <C> borrador-igual=True llamadas=0 descartes=1
  ```
- **Pasada 3:** una ventana sobre `Estado(pb-20-ab)`. Los gestos, en orden:
  - `Combo(w, "FrontalSideBox").SelectedIndex = 1`;
  - `SideSelectorBox` a 1 y después a 2;
  - la receta `StageModuleEdit` de T-CL con `ConfirmModuleButton`, y otra vez con `CancelModuleButton`.

  La confirmación cambia un módulo, así que la ventana da `descartes=1`.
  ```
  FrontalSideBox=1 <C> borrador-igual=True llamadas=0
  SideSelectorBox=1 <C> borrador-igual=True llamadas=0
  SideSelectorBox=2 <C> borrador-igual=True llamadas=0
  modulo-confirmado <C> borrador-igual=True llamadas=0
  modulo-cancelado <C> borrador-igual=True llamadas=0 descartes=1
  ```

**El paso 1 de PB-T2b, sobre `BaseSha` y antes del RED** (una corrida temporal de Core, fuera de todo commit, que termina en `Assert.Fail`):
- por diseño del corpus C18, `<diseño> seguridad-autorizada=<SequenceEqual estructural de s.SafetySelections y assembler.AuthorizedSafety(s.SafetySelections)>`, que decide la rama de cada diseño en CT-02;
- `N-PBT2-01a vistas=<s.LoadedDimensionViews>-><DimensionViewPolicy.FromEditor(s.LoadedDimensionViews, true, s.DimensionsFrontal, false, s.DimensionsPlanta)>`, que da `<a>` y `<b>`.

Se transcribe en la entrega. Si un diseño no puede llevar ninguna de las dos ramas, es PBT2B-C15.

### 5.4 PB-T2c: las obligaciones heredadas en WORKSPACE (lista cerrada CT-PBT2C-01 a 03)

**Régimen:** el de PB-T1a, copiado en el contrato:
- solo pruebas nuevas, en `tests/RackCad.UI.Tests/PushBackWorkspaceCarryoverTests.cs`, con los ayudantes de `PushBackWorkspaceSupport` (que no se edita) y privados propios;
- **el RED** es esa clase y los tres mutantes de la tabla cerrada, juntos; **el GREEN** los revierte con `git restore --source=<BaseSha>` de W y WC;
- cada mutante, aislado sobre `BaseSha`, hace fallar por aserción el CT de su fila. Las pruebas existentes que fallen en el RED se atribuyen a un mutante en una tabla de atribución, como T-8 de PB-T1a.

| CT | Qué fija | Mutante |
|---|---|---|
| **01** `CT_PBT2C_01_InWorkspaceTheDraftKeepsTheExactValuesAndTheNamedStyle` | **El paso 4 de D-PB-06 en WORKSPACE.** Por fila de `SC1.FilasF()`: `Estado(VarianteDe(fila))`, una ventana de WORKSPACE, un gesto neutro (`Check(w, "DrawRackNameCheck").IsChecked = !…`) y «Aplicar» → `<Fn> WORKSPACE aplicado=True peticion=False diseno=<Num(DisenoDe(draft.CurrentDraftState, A), fila.Path)>`, con los valores de CT-PBTC-02 (i): el exacto en diez filas (F1 48.00044, F2 3.00044, F3 4.00044, F5 1.50044, F6 4.00044, F7 42.00044, F8 60.00044, F9 1000.00044, F10 6.00044 y F11 100.00044) y `6` en F4. `pb-08-redondeo` → `pb-08-redondeo WORKSPACE aplicado=True peticion=False AnnotationScale=1.23456 PalletHeight=61.23456 sistema.AnnotationScale=1.23456`. `pb-06-seguridad` y `pb-07-combinado` → `<diseño> WORKSPACE aplicado=True diseno=<estilo> sistema=<estilo> cotas>0=True estilos=[<estilo>]`, con STANDARD-ISO y SIMPLEX, como CT-PBTC-05. Cada fila es una ventana que acaba aplicada: su línea termina en ` descartes=0` (v2.1, V21-02) | MU-T2C-01 |
| **02** `CT_PBT2C_02_InWorkspaceTheSideBNoticeShowsExactlyAsInRackeditar` | **El aviso de D-PB-08 en WORKSPACE.** Los gestos de CT-PBTC-07, copiados, en ventanas de WORKSPACE sobre `Estado(pb-01-base)` y `Estado(pb-20-ab)`: sus doce líneas literales (`PushBackDeclaredCorrectionSideBNoticeTests.cs:89-100`). La última de cada ventana gana ` descartes=1` (v2.1, V21-02): las dos secuencias dejan el estado distinto de W0 sin aplicarlo, con las ranuras 1 y 2 de A en blanco en `pb-01-base` y todas las de B en blanco en `pb-20-ab`. Y, en una tercera ventana sobre `pb-01-base`, declarar B y «Aplicar» → `pb-01-base declarar B y aplicar aviso=Visible aplicar-habilitado=True llamadas=1 borrador-B-presente=True diseno-del-borrador-compuesto=False descartes=0` | MU-T2C-02 |
| **03** `CT_PBT2C_03_InWorkspaceNoControlIsTheSourceOfTruthOnTheWayToTheDraft` | **El censo en WORKSPACE.** Por cada fila del censo (§5.5), el gesto de esa fila en CT-PBT1C-02, copiado de su clase cerrada [T1c], sobre el mismo diseño, en una ventana de WORKSPACE nueva, y «Aplicar» → `<fila> aplicado=<b> igual-a-la-ventana=True diseno-igual=True descartes=0`. `diseno-igual` se toma tras «Aplicar», que recalcula antes de aplicar (`ApplyEditsToEditingState`; v2.3, V23-02). Lo esperado de `aplicado` se fija fila por fila en la remedición sobre el cierre de PB-T2b. `descartes=0` en todas las filas (v2.1, V21-02): con `aplicado=True` la ventana acaba aplicada, y con `aplicado=False` el gesto no cambió nada | MU-T2C-03 |

**La tabla cerrada de mutantes** [T2b] (una sola línea cada uno, en el texto que deja PB-T2b; el generador lo comprueba sobre `BaseSha`):

| Id | Miembro | Línea | Sustitución |
|---|---|---|---|
| MU-T2C-01 | `Keep` (W) | `=> !field.Value.HasValue \|\| SameShown(field.Value.Value, current) ? current : field.Value.Value;` | `=> !field.Value.HasValue ? current : field.Value.Value;` |
| MU-T2C-02 | `UpdateSideBEmptyNotice` (WC) | `SideBEmptyNoticeText.Visibility = empty ? Visibility.Visible : Visibility.Collapsed;` | `SideBEmptyNoticeText.Visibility = Visibility.Collapsed;` |
| MU-T2C-03 | `ApplyToDraft` (W) | `var result = draftPort.ApplyToDraft(PushBackEditableStateComparer.Copy(editable));` | `var result = draftPort.ApplyToDraft(PushBackEditableStateComparer.Copy(appliedState));` |

- **MU-T2C-01** es MU-T1A-07a de PB-T1a; también hace fallar pruebas de PB-TC, PB-TC2 y PB-T1a (atribución).
- **MU-T2C-02** hace fallar también CT-PBTC-07 y 08.
- **MU-T2C-03** hace fallar también, como previstos, CT-PBT2B-03 y 04, y **CT-PBT2B-08 por su R4, en Core** (v2.1, V21-07; IR1-R N-07). CT-PBT2B-06 sale de la previsión: su `cerrar-tras-aplicar` no depende de lo que recibe el borrador, porque `appliedState` se sigue copiando de `editable`. La tabla de atribución recoge lo que falle de verdad, sea lo previsto o no.

Por eso, **en el RED, Core falla** (§16.9, n.º 9) y `build-plugin` sale omitido.

### 5.5 El censo control → prueba, actualizado

La columna nueva es «WORKSPACE»: el camino del control al borrador. Las dos primeras son las de [PT1] §5.5.

| Control | Su efecto en la salida | El estado lo lleva (PB-T1c) | WORKSPACE |
|---|---|---|---|
| `DepthBox`, `ToleranceBox`, `PostPeralteBox`, `BeamDepthBox`, `AnnotationScaleBox` | CT-PB0-19 (SUS-03), CT-PBTC-01 a 03, CT-PBT1A-07 | CT-PBT1C-02 | CT-PBT2C-03; CT-PBT2C-01 (exactos) |
| `WeightUnitBox`, `PalletsDeepBox` | CT-PB0-17, CT-PB0-04, CT-PBT1A-02, CT-PBT1B-17 | CT-PBT1C-02 | CT-PBT2C-03; CT-PBT2B-04 |
| `PostBox` | CT-PB0-17, CT-PBT1B-03 | CT-PBT1C-02 | CT-PBT2C-03 |
| `NumberFrontsCheck`, `NumberLevelsCheck`, `DrawRackNameCheck`, `DimensionsBox`, las tres de cotas | CT-PB0-17, `PushBackDimensionViewsWindowTests`, CT-PBT1A-01 | CT-PBT1C-02 | CT-PBT2C-03; CT-PBT2B-02 (N-PBT2-01a) |
| `PositionsBox`, `LevelsBox`, `FondosBox`, `DepthStartBox` | CT-PBT1A-03 | CT-PBT1C-02 | CT-PBT2C-03 |
| `FirstLevelHeightBox` y las cuatro de la celda | CT-PB0-19, CT-PBTC-01 a 03, CT-PBT1A-07 | CT-PBT1C-02 | CT-PBT2C-03; CT-PBT2C-01; CT-PBT2B-02 (`ambos-mixto`) |
| `CellInOutBeamBox`, `CellInOutPeralteBox`, `CellIntermediateBeamBox` | CT-PB0-17 | CT-PBT1C-02 | CT-PBT2C-03 |
| `CellIntermediatePeralteBox`, `CellFondoOverrideBox`, `CellDrawPalletCheck` | CT-PBT1A-04 | CT-PBT1C-02 | CT-PBT2C-03 |
| `CellBeamLengthOverrideBox` | CT-PB0-19, CT-PBTC2-04, CT-PBT1A-10 | CT-PBT1C-02 | CT-PBT2C-03; CT-PBT2C-01 (F11) |
| `RearPeralteBox` | CT-PB0-17 | CT-PBT1C-02 | CT-PBT2C-03 |
| `GapBox` | CT-PB0-20, CT-PBTC2-01 | CT-PBT1C-02 | CT-PBT2C-03; CT-PBT2B-05 |
| `CentralSeparatorCheck`, `StructureOverrideBox`, `CellTopologyBox`, `RunDirectionBox`, `TopologyScopeBox`, `FrontCountBox` | CT-PBT1A-05 | CT-PBT1C-02 | CT-PBT2C-03 |
| `NameBox` | CT-PB0-11 | CT-PBT1C-02 | CT-PBT2C-03; CT-PBT2B-02 y 04 (nombre) |
| Las cajas avanzadas | CT-PB0-05 (SUS-04), CT-PBTC-14, CT-PBTC2-03 | CT-PBT1C-02 | CT-PBT2C-03; CT-PBT2B-05 |
| `ModuleLengthBox`, `DerivedPostLineHeightBox` | CT-PB0-03, CT-PB0-04, CT-PBTC2-02, CT-PBT1A-06 | CT-PBT1C-02 | CT-PBT2C-03; CT-PBT2B-03 y 06 |
| `SideBPresentCheck`, `SideSelectorBox`, `FrontalSideBox` | CT-PB0-06 a 09, CT-PBT1A-09, CT-PBT1A-14 | CT-PBT1C-02 | CT-PBT2C-03; CT-PBT2C-02; CT-PBT2B-02 y 03 |
| **Los botones de la barra y los subdiálogos con costura** | T1B (T-IN, T-CL), S-7, S-9, CT-22, CT-23, CT-PB0-12 a 16 | — | CT-PBT2B-01, 03, 04, 06 y 07; CT-PBT2B-08 |

**Huecos que quedan:** ninguno en la ventana. Las botas por poste se cubren por R3 (D-PBT2-08), y el campo de escala del panel queda para PB-T4 (Q-PBT2-3).

### 5.6 Trazabilidad

| Obligación | CT |
|---|---|
| [OD] separar aplicar, cerrar y escribir | D-PBT2-06 y D-PBT2-07; CT-PBT2B-04, 06 y 08 (R3, R4, R9, R11) |
| [OD] «Workspace mode must return edited state to DraftSession» | CT-PBT2A-01 y 03; CT-PBT2B-04; CT-PBT2C-03 |
| [OD] «Classic RACKEDITAR may continue its compatible existing outer workflow» | INV-PBT2B-05; §5.1; CT-PBT2B-01 (i) y 06 |
| [OD] «Avoid two independent implementations of field validation» | D-PBT2-11; CT-PBT2B-08 (R11), 05 y 10; Q-PBT2-3 |
| OB-01.1b | CT-PBT2B-01; CT-PBT2B-08 (R5, R6, R8) |
| OB-01.2 y OB-01.3 (la parte de PB-T2) | CT-PBT2B-03; CT-PBT2B-08 (R1 a R4) |
| OB-01.5 | §5.1; INV-PBT2B-05 |
| OB-01.6 | CT-PBT2B-01; CT-PBT2B-08 (R10) |
| F4B-E23: las dos costuras y las cuatro comprobaciones | D-PBT2-10; CT-PBT2B-03 y 07; CT-PBT2B-08 (R7); S-7, S-9 y CT-22 en verde |
| OB-06.3 | CT-PBT2C-01; A2-11 (hasta el DWG) |
| OB-08.2 | CT-PBT2C-02 |
| D-15, punto 3 | CT-PBT2A-03; CT-PBT2B-02 |
| D-15, punto 4 | D-PBT2-12; CT-PBT2B-01 y 07 |
| D-PB-03 por la ventana | CT-PBT2B-02 (X-01, F-01, `pb-20-ab` y la reapertura con selección) |
| D2, condiciones 2, 3 (el puerto), 4 y 10, y el límite | CT-PBT2A-03 y 04; CT-PBT2B-04, 05 y 09; C17 |
| A-1 y A-7 a A-10 de [PT1] §10 | todo lo anterior; §8.1 y §10 |

## 6. Los contratos propuestos

### 6.0 Común a las tres subtareas

**Identidad:** `Schema` `rackcad-gate-contract/v1`; `Unit` I-64; `Gate` F4B; attempts 1 de 3 (OB-10.1); `RoutingEnforcement` `required`; `CorrectionsAuthorized` true. Evidencia en `docs/automation/evidence/I-64-pilot/<TaskId>/`. Los `TaskId` tienen 26, 31 y 29 caracteres (MAX_PATH; v2.1, V21-05).

**Anclajes:** antes de emitir cada subtarea, la sesión vuelve a medir sobre su `BaseSha` y lo escribe en el commit de emisión.
- **PB-T2a:**
  - `git diff <cierre de PB-T1c> <BaseSha> -- src` vacío;
  - las anclas de los registros G, H y Q;
  - **sobre el texto real de PB-T1c** (v2, V2-05): que `editable` está declarado como en D-PBT1-13 (1), que todo lo que cuelga de él es calculado, y que ni CT-PBT1C-01 a 04 ni ninguna otra guarda impiden reasignarlo en un constructor ni añadir los miembros de INV-PBT2B-03. **Si falla, STOP:** una v3 del paquete con una subtarea propia y su censo, antes de emitir nada;
  - las reglas y las autopruebas de G-L1c;
  - la línea base de la CI.
- **PB-T2b:**
  - `git diff <cierre de PB-T2a> <BaseSha> -- src/RackCad.UI` vacío;
  - todas las anclas [T1c] del §2 y del §4 sobre el texto real de PB-T1c, con los rangos de INV-PBT2B-03;
  - la línea base de la CI.
- **PB-T2c:**
  - `git diff <cierre de PB-T2b> <BaseSha> -- src` vacío;
  - los textos de los tres mutantes [T2b];
  - la tabla de gestos de CT-PBT1C-02, con lo esperado de cada fila de CT-PBT2C-03;
  - la línea base de la CI.

**La comprobación acotada:** un revisor independiente, en solo lectura, sobre este paquete, la sección de la subtarea y la salida de sus anclajes. Su resultado, EMITIBLE o NO EMITIBLE, se custodia en `F4B-DRAFTS/pbt2/`.

**Custodia:**
- en el commit de emisión de PB-T2a: este paquete (en `docs/initiatives/I-64-f4b-pbt2-gate-contract-package.md`), el registro, `v2-delta.md`, `v21-delta.md`, `v22-delta.md` (v2.2, V22-02), los guiones de medición (`measure_pbt2.py`, `measure_pbt2_v2.py`, `forbidden_pbt2.py` y `check_questions_v2.py`), IR1 con su recomprobación (IR1-R), las decisiones de Q-PBT2-1 a 4, y el contrato y su generador en su `coordinator/`;
- en los de PB-T2b y PB-T2c: su contrato y su generador, la salida de sus anclajes y su comprobación acotada; en el de PB-T2b, además, esta v2.3, `v23-delta.md` y `measure_pbt2b_t1c.py` (v2.3, V23-06);
- en `f4b-dispositions.md`: la división, Q-PBT2-1 a 4, cada emisión y los arrastres del §10.

**La delegación (F4B-E21):** el Controller deja vacías las ocho colecciones opacas, y la sesión las pone byte a byte. La comparación byte a byte, A1-A8 y nc4 se hacen sobre la delegación final.

**Las `Authorities`** (§16.5; v2, V2-02), por ruta, sección y clase. Las enumera el generador.
- **`UNIT_DOC`:**
  - [P];
  - [PT1];
  - este paquete, el registro y `v2-delta.md`;
  - IR1 y las comprobaciones acotadas;
  - las decisiones del Owner F4B-E03, E09, E11, E19, E21, E22, E23, E24 y E25 (sus archivos de `coordinator/`) y sus vueltas;
  - `f4b-dispositions.md` y los cierres de PB-T1b y PB-T1c;
  - la Proposal V5 (`docs/initiatives/I-64-proposal-v5.md`);
  - el propio contrato.
- **`UNIT_CHANGE`:**
  - **PB-T2a: ninguna.** Sus dos archivos no existen en la `AuthorityRevision` y no son autoridades;
  - **PB-T2b:** W, con la sección «los miembros de INV-PBT2B-03 (el modo WORKSPACE, D-PBT2-04 a D-PBT2-10)»;
  - **PB-T2c:** W y WC, con la sección «solo los tres mutantes del RED (MU-T2C-01 a 03), revertidos por el GREEN», como en PB-T1a.
- **`EXTERNAL`:** `docs/adr/0029-contrato-funcional-comun-de-ventanas-wpf.md` (D7, D8, D12 y D13), `docs/AUTOMATION_PLAN.md` (§16), `docs/automation/agent-execution/README.md`, `routing.md` y `model-catalog.md`.
- **Las correcciones:** se leen con **F4B-E25**. Una ruta `UNIT_CHANGE` de `git diff --name-only AuthorityRevision BaseSha` no incumple 16.3 si se cumplen sus tres condiciones, que comprueba el Controller. Las `EXTERNAL`, nunca. No hace falta reemitir el contrato para corregir.

**`ForbiddenWriteScope`:** el método de [PKC-C], calculado por el generador con `assert` (registros N y Q):
- **`src/`:** 87 en PB-T2a, 60 en PB-T2b y 59 en PB-T2c (medidos en `95373af6`). En PB-T2c sube a 60 tras PB-T2b, por el archivo del modo;
- **`tests/`:** la lista curada de PB-T0 (18 entradas); en `tests/RackCad.UI.Tests/`, la de [PKC2] (51 archivos y `Workspace/`), las clases de PB-TC2, PB-T1a, PB-T1b y PB-T1c y las de las subtareas anteriores de PB-T2;
- **la raíz:** las 17 entradas de [PKC2].

**`StopConditions` estándar** (17): S-02, S-03, S-04, S-05, S-07, S-08, S-12, S-13 y S-14, y P-01 a P-08, con P-01 = «config.toml no longer matches the accepted Snapshot in force (today Snapshot J, docs/automation/evidence/I-64-pilot/F4-ENVIRONMENT/snapshot-J/snapshot-j.json)».

**Las de [DP], literales en cada contrato** (x = A, B o C): PBT2x-C08, PBT2x-C11 y PBT2x-C12, con el texto de [PT1] §6.0, cambiando solo el prefijo y, en PBT2x-C11, la referencia «section 6.0 of the package» por «section 6.0 of the PB-T1 contract package ([PT1], docs/initiatives/I-64-f4b-pbt1-gate-contract-package.md)» (v2.2, V22-01). Ninguna aceptación puntual anterior de un control se extiende.

**La parada del límite de D2:** PBT2x-C17, con el texto de [PT1] §6.0 y el literal del Owner. El tiempo de vida incluye el de la ventana de WORKSPACE.

**El RED de la cadena en una corrección (§16.8):** una corrección que toque `RT` ∪ `ChainRedFiles` exige RED otra vez:
- en PB-T2a, vuelve a poner el esqueleto;
- en PB-T2b, la costura inerte;
- en PB-T2c, los tres mutantes.

**La CI del RED (lección M14 de PB-T1b):** en los tres RED, Core falla, así que `build-plugin` sale **omitido** por su `needs` (registro O). `RedPart` es `pass` con:
- **PB-T2a:** `tests` en `failure`, `ui-tests` y `build-ui` en `success`;
- **PB-T2b:** `tests` y `ui-tests` en `failure`, `build-ui` en `success`;
- **PB-T2c:** `tests` en `failure` (CT-PBT2B-08 por MU-T2C-03, atribuido) y `ui-tests` en `failure`, `build-ui` en `success`.

**`EligibleCells`:** Worker `anthropic/subagent/claude-sonnet-5-5` con effort `high`; Controller `openai/process/gpt-6-luna` con effort `high`. Vencen el **2026-12-29** (`model-catalog.md:14`, `:51`, `:97`). Se comprueban en cada emisión y delegación (A7).

**Invariantes comunes** (cada contrato los numera INV-PBT2x-07 a 12 y 14). Lo normativo va en el contrato; las notas no (v2, V2-27).
- **07. Las redes:** las del §5.1, las guardas del §4, el censo del piloto y las subtareas anteriores, en verde. Ninguna prueba existente se edita, se salta, se reactiva ni se borra. En PB-T2c, en el RED, solo pueden fallar por los mutantes, con su atribución.
- **08. Las pruebas de UI:**
  - el régimen del §5.0 y la sonda del modo antes de cualquier gesto de WORKSPACE;
  - el BOM y «Guardar en biblioteca» solo por las costuras nuevas; el configurador solo por `HeaderConfiguratorDialog`;
  - la única escritura en disco es `ArchivoTemporal`, borrado en el `finally`;
  - nunca `PromptSaveToLibrary`, un `RackBomWindow` mostrado, un `MessageBox` real, el configurador real ni «Insertar» real;
  - ninguna aserción dentro de una costura;
  - ningún fixture nuevo en `Fixtures/I64`.
- **09. Determinismo:** `GUID-PBT2-0001`, `SessionId(64)`, la cultura invariante, «R» y el orden ordinal.
- **10. Las corridas locales:**
  - en UI, solo los filtros del contrato, nunca la suite completa; la de Core, sí;
  - sin copias de árboles;
  - las corridas temporales terminan en `Assert.Fail`, corren dentro de `EditorDiscardPrompt.Substitute`, no entran en ningún commit y guardan TRX y log;
  - el perfil no cambia, salvo `RackCad/RackFrameConfigurator.layout` de los datos locales del usuario, que escriben al cerrarse los `RackFrameConfiguratorWindow` reales de pruebas que ya existían, en los filtros `RichEditor` y `WindowCensusGuardTests` (F4B-E30 y F4B-E31; v2.3, V23-01): la prueba de limpieza del perfil se registra, y si solo cambia ese archivo se atribuye a esos filtros, sin parar; cualquier otro cambio del perfil es parada.
- **11. La evidencia:** rutas relativas comprobadas; `TreeSha` real o null; el Worker no escribe en `docs/`; una sustitución sería C02.
- **12. Las reglas de F4B, sin AutoCAD,** con las tres comprobaciones deterministas de F4B-E15 en la verificación.
- **14. D2:** ninguna copia nueva comparte un objeto mutable fuera de las dos referencias declaradas de cada lado, y ninguna línea añadida muta un `PushBackSystem` ni un `RackModuleReconciliationResult` adoptado.

**Las propias comunes** (con el prefijo de cada contrato): C01, C02 (una guarda del §4 que no queda en verde vuelve al Owner, OB-T1B.3), C03 (un cambio observable de RACKEDITAR, RPB o el menú: STOP y vuelta al Owner), C05 (un CT necesitaría AutoCAD, un modal real, el perfil o el disco fuera de `ArchivoTemporal`), C06, C07 (el Worker no ajusta lo esperado), C09 (rige las pruebas nuevas; la disposición del configurador que escriben las existentes se atribuye como dice el invariante 10; v2.3, V23-01) y C10 (corregir algo fuera del alcance: el piloto, el panel, el host core, «Editar todo», la escritura del panel, el campo de escala del panel, T-2, M-07, `pb-21-cola`, AUTH-13, PB-TC o PB-TC2).

**`ExpectedEvidence` común** (v2, V2-28):
- **antes del Worker:** el contrato, la salida del Controller (`output.json`, con las colecciones opacas vacías), la delegación final con esas colecciones puestas por la sesión (`delegation.json`), la comparación byte a byte, A1-A8 y nc4;
- **el RED:** el commit RED y su push antes del GREEN, con su CI tal como dice arriba (sin cuelgue, ni blame-hang ni timeout), y los TRX de cada filtro con `ExpectRed`;
- **el GREEN:** el commit GREEN y su CI 4/4;
- **en local:** el build Release con el Plugin sin avisos nuevos, los filtros sobre el RED y el GREEN, y tres corridas en verde de los filtros con `ExpectRed` sobre el GREEN;
- **la entrega:** el mapa CT → método, las rutas relativas comprobadas y **el `Trailer` de `worker-handoff.json`, que es el trailer real de los commits RED y GREEN** (`Co-Authored-By: …`; la mitad (b) de PBT1B-IR-M14);
- **después:** la verificación con las tres comprobaciones de F4B-E15, y nc1 a nc3.

### 6.1 Contrato de PB-T2a — `F4B-PBT2A-DRAFT-STATE-PORT`

- **Objective** (ASCII): «Crear en Application el puerto por el que el editor completo de Push Back en modo WORKSPACE devuelve su estado editado al borrador (IPushBackDraftStatePort, PushBackDraftApplyResult y el adaptador PushBackDraftSessionPort sobre DraftSession<PushBackEditableState>) y el lanzamiento tipado de ese modo (PushBackDraftEditorLaunch), segun el paquete del contrato de PB-T2 v<n> y la lista cerrada CT-PBT2A-01..04. El RED es el esqueleto inerte y las pruebas nuevas, que fallan por asercion salvo la guarda G-L1; el GREEN implementa solo los dos archivos nuevos de Application. La ventana, el piloto, el host core, el nucleo de borradores, el Plugin y los archivos de PB-T1b no cambian; no se ejecuta AutoCAD. Si hiciera falta mutar el rack resuelto o el informe de modulos dentro del tiempo de vida del borrador: STOP y se reabre L-1 (decision D2 del Owner, F4B-E11).»
- **`AllowedWriteScope`** (4): `src/RackCad.Application/Systems/PushBack/PushBackDraftStatePort.cs`, `src/RackCad.Application/Systems/PushBack/PushBackDraftEditorLaunch.cs`, `tests/RackCad.Tests/PushBackDraftStatePortTests.cs` y `tests/RackCad.Tests/PushBackDraftPortSharedReferenceGuardTests.cs`.
- **`ForbiddenWriteScope`:** 87 de `src/` y lo común.
- **`Authorities`:** las comunes; ninguna `UNIT_CHANGE`.
- **`RequiredTests`:**

  | Proyecto | Filtro (`FullyQualifiedName~RackCad.<proyecto>.`…) | `MinSelected` | `ExpectRed` |
  |---|---|---|---|
  | Core | `PushBackDraftStatePortTests` | **3** | true |
  | Core | `PushBackDraftPortSharedReferenceGuardTests` | **1** | false |
  | Core | `PushBackEditableState`, `PushBackEditorOutputGate`, `PushBackSharedReferenceGuardTests` | 13, 1, 2 | false |
  | Core | `PushBackWindowOnStateSourceGuardTests`, `PushBackWindowSharedReferenceGuardTests` [T1c] | 1, 1 | false |
  | Core | `Workspace`, `NamespaceFolderGuardTests`, `PushBackClassicCharacterization`, `PushBackFullEditorCharacterization` | 3 271, 7, 10, 3 | false |
  | UI | `PushBackWindowOnStateTests`, `PushBackEditableStateParityTests`, `PushBackStateNet` | 2, 1, 14 | false |
  | UI | `PushBackClassicCharacterization`, `PushBackFullEditorCharacterization`, `PushBackDeclaredCorrection` | 55, 16, 14 | false |
  | UI | `Cantilever` | 67 | false |

- **Invariantes propios:**
  - **INV-PBT2A-01. Alcance:** el diff dentro de las 4 rutas; en `src/`, solo los dos archivos nuevos. `git diff BaseSha..CurrentSha -- src/RackCad.UI src/RackCad.Plugin src/RackCad.Domain src/RackCad.Application/Workspace src/RackCad.Application/Systems/PushBack/Pilot` vacío, y ningún archivo existente de `src/RackCad.Application/Systems/PushBack/` cambia.
  - **INV-PBT2A-02. RED y GREEN:** el RED es el esqueleto exacto del §5.2 y las dos clases; el GREEN solo toca los dos archivos de `src/`. El criterio del RED: el filtro con `ExpectRed` da `Failed == Selected`, todos por aserción. La guarda pasa en el RED y en el GREEN, y sus autopruebas fallan sobre lo inyectado.
  - **INV-PBT2A-03. La API:** exactamente la de D-PBT2-02 y D-PBT2-03, con las sentencias de excepción literales. El único miembro privado de producto es el campo `draft` del adaptador.
  - **INV-PBT2A-04.** Ningún campo estático. Un solo `namespace RackCad.Application.Systems.PushBack` de bloque por archivo; las pruebas, `namespace RackCad.Tests`.
  - **INV-PBT2A-05.** Los archivos nuevos de `src/` no contienen `RackCad.UI`, `System.Windows`, `Autodesk`, `Pilot`, `RackSystemKind`, `.RequestUpdate(`, `CrossBoundary`, `TryBeginApply`, `CompleteApply`, `DiscardAndReload`, `KeepMineOnCurrentVersion`, `EnterError`, `MarkPossiblyStale` ni `SetPendingText`.
  - **INV-PBT2A-06.** `ApplyToDraft` hace una sola llamada al borrador: `SetCurrentDraftState(edited)`.
  - **Nota (no va al contrato).** El RED y su CI siguen la lección M14 (§6.0). G-L1 se copia de G-L1c (registro Q-3). La lista cerrada de miembros es la lección M10.
- **`StopConditions`:** las 17 estándar, C08, C11, C12, C17, las comunes, y:
  - **PBT2A-C04:** a method of a filter with `ExpectRed` passes over the skeleton, or fails by an exception and not by an assertion.
  - **PBT2A-C13** (v2, V2-26): doing the task would require touching `src/RackCad.Application/Workspace/Drafts/`, a file of PB-T1b, `Pilot/` or `WorkspaceHostCore`; or adding, in `PushBackDraftStatePort.cs` or `PushBackDraftEditorLaunch.cs`, a member or type declaration (of any visibility: type, method, property, field, constant, enum member or nested type) whose name is not one of `PushBackDraftApplyStatus`, `NotEditable`, `Applied`, `PushBackDraftApplyResult`, `Status`, `IsDirty`, `IPushBackDraftStatePort`, `ApplyToDraft`, `PushBackDraftSessionPort`, `draft`, `LaunchFor`, `PushBackDraftEditorLaunch`, `State`, `RackId`, `SourceProject` and `Port`, besides the constructors of those types. The Worker stops before adding it.
  - **PBT2A-C15:** CT-PBT2A-03 gives `iguales=False`, `diseno-igual=False`, `huellas-iguales=False` or something shared outside D2. It is a new fact of PB-T1b: STOP and return to the Owner (OB-06.5).
- **`ExpectedEvidence`:** la común, con la tabla «CT → la aserción que falla en el RED» y la salida de las autopruebas de la guarda.
- **Línea base de la CI:** la del cierre de PB-T1c. **Prevista:** el cierre de PB-T1b medido (`44438060317`: Core 19 086; UI 2 437, con 2 416 `Passed` y 21 `Skipped`), más PB-T1c (Core + 2, UI + 2): **Core 19 088, UI 2 439**. **GREEN de PB-T2a:** Core + 4 = **19 092**; UI **2 439**.

### 6.2 Contrato de PB-T2b — `F4B-PBT2B-WORKSPACE-MODE-WINDOW`

- **Objective** (ASCII): «Dar a la ventana de Push Back sus dos modos explicitos: CLASSIC sin cambio observable, y WORKSPACE solo por la factoria tipada ForDraft. Separar aplicar al estado de edicion, aplicar al borrador, cerrar y la escritura clasica. En WORKSPACE la ventana carga una copia del estado del borrador; Aplicar al borrador devuelve una copia por el puerto de PB-T2a y no cierra; Insertar y el lote no se muestran y ninguna ruta llega a la peticion clasica; BOM y Guardar en biblioteca siguen, con dos costuras de prueba nuevas; la validacion de campos es la misma en los dos modos. Todo segun el paquete del contrato de PB-T2 v<n>, con las decisiones del Owner F4B-E23 y Q-PBT2-1..4, y la lista cerrada CT-PBT2B-01..10. El RED es la costura inerte y las pruebas nuevas; el GREEN toca solo W y el archivo nuevo del modo. El XAML, WC, Application, el Plugin, el piloto y los constructores actuales no cambian; no se ejecuta AutoCAD. Si hiciera falta mutar el rack resuelto o el informe de modulos dentro del tiempo de vida del borrador: STOP y se reabre L-1 (decision D2 del Owner, F4B-E11).»
- **`AllowedWriteScope`** (8):
  - W;
  - `src/RackCad.UI/Systems/PushBack/PushBackEditorMode.cs`;
  - en `tests/RackCad.UI.Tests/`: `PushBackWorkspaceSupport.cs`, `PushBackWorkspaceModeTests.cs`, `PushBackWorkspaceApplyCloseTests.cs` y `PushBackFieldValidationConcordanceTests.cs`;
  - en `tests/RackCad.Tests/`: `PushBackWorkspaceModeSourceGuardTests.cs` y `PushBackWorkspaceSharedReferenceGuardTests.cs`.
- **`ForbiddenWriteScope`:** 60 de `src/` (WC y WX incluidos) y lo común.
- **`Authorities`:** las comunes, y W como `UNIT_CHANGE` («los miembros de INV-PBT2B-03»).
- **`RequiredTests`** (con filtros por clase exacta, para que ningún filtro sin `ExpectRed` contenga una clase nueva):

  | Proyecto | Filtro | `MinSelected` | `ExpectRed` |
  |---|---|---|---|
  | UI | `PushBackWorkspaceModeTests` (CT-01, 02 y 07) | **3** | true |
  | UI | `PushBackWorkspaceApplyCloseTests` (CT-03 a 06) | **4** | true |
  | UI | `PushBackFieldValidationConcordanceTests` (CT-10) | **1** | false |
  | Core | `PushBackWorkspaceModeSourceGuardTests` (CT-08) | **1** | true |
  | Core | `PushBackWorkspaceSharedReferenceGuardTests` (CT-09) | **1** | false |
  | Core | `PushBackDraftStatePortTests`, `PushBackDraftPortSharedReferenceGuardTests` (PB-T2a) | 3, 1 | false |
  | Core | `PushBackWindowOnStateSourceGuardTests`, `PushBackWindowSharedReferenceGuardTests`, `PushBackSharedReferenceGuardTests` [T1c] | 1, 1, 2 | false |
  | Core | `PushBackEditableState`, `PushBackEditorOutputGate`, `PushBackClassicCharacterization`, `PushBackFullEditorCharacterization`, `PushBackModuleEditorCharacterizationTests`, `I55DeferredProductFixCharacterizationTests`, `NamespaceFolderGuardTests`, `Workspace` | 13, 1, 10, 3, 15, 3, 7, 3 271 | false |
  | UI | `PushBackWindowOnStateTests`, `PushBackEditableStateParityTests`, `PushBackStateNet` | 2, 1, 14 | false |
  | UI | `PushBackClassicCharacterization`, `PushBackFullEditorCharacterization`, `PushBackDeclaredCorrection` | 55, 16, 14 | false |
  | UI | `PushBackCompositeWindowTests`, `PushBackCompositeLifecycleWiringTests`, `PushBackCompositeLoadExistingModulesTests`, `PushBackBothModeCellBeamOverrideTests`, `PushBackModuleEditorWindowTests`, `PushBackAdvancedRackParametersWindowTests` | 52, 7, 3, 10, 13, 9 | false |
  | UI | `PushBackRichPreviewTests`, `PushBackLibrarySaveGateTests`, `DialogWindowCharacterizationTests` (v2, V2-19) | 9, 5, 23 | false |
  | UI | `BlankFront`, `WindowCensusGuardTests`, `RichEditor`, `Workspace`, `Cantilever` | 33, 8, 25, 547, 67 | false |

- **Invariantes propios:**
  - **INV-PBT2B-01. Alcance:** dentro de las 8 rutas, y en `src/` solo W y el archivo del modo. `git diff BaseSha..CurrentSha -- src/RackCad.UI/Systems/PushBack/RackPushBackSystemWindow.xaml src/RackCad.UI/Systems/PushBack/RackPushBackSystemWindow.Composite.cs src/RackCad.Application src/RackCad.Domain src/RackCad.Plugin src/RackCad.UI/Editor src/RackCad.UI/Controls src/RackCad.UI/Workspace src/RackCad.UI/Shell` vacío.
  - **INV-PBT2B-02. RED y GREEN:** el RED son las clases nuevas y, en W, solo la costura inerte del §5.3; el GREEN solo W y el archivo del modo. El criterio, como INV-PBT1A-02, con `Failed == Selected` en cada filtro con `ExpectRed`. CT-09 y CT-10 pasan en el RED y en el GREEN.
  - **INV-PBT2B-03. Dónde cambia W.** Cada hunk de `git diff -U0 BaseSha..CurrentSha -- W` cae, por su rango en `BaseSha` y con su documentación, en un miembro de esta lista, o es una inserción entre dos miembros que solo declara uno de los nuevos:
    - **los miembros que cambian,** con sus rangos de `BaseSha`, documentación incluida (v2.3, V23-04): `LoadNew` (W:269-287), `LoadDesignForNew` (W:289-304), `LoadExisting` (W:306-320), `Update_Click` (W:3215), `InsertBatch_Click` (W:3217-3245), `RequestDraw` (W:3247-3304), `PendingWork` (W:3331-3344), `Bom_Click` (W:3348-3364), `SaveLibrary_Click` (W:3366-3401) y `UpdateButtons` (W:3449-3496);
    - **las declaraciones nuevas:** los campos `mode`, `draftPort`, `appliedState`, `BomDialog` y `LibraryPathDialog`; `ModeForTest`; `ForDraft`; el constructor privado; `ApplyModeChrome`, `RefuseInWorkspace`, `ApplyEditsToEditingState`, `ApplyToDraft` y `HasUnappliedDraftChanges`; y las once constantes de D-PBT2-09.
    - **Nota (no va al contrato).** Los rangos se midieron en la v2.3 sobre el texto de PB-T1c (`0dc623ca`), con el método de INV-PBT1C-03, y el generador los vuelve a comprobar en la emisión. Es la lección M10.
  - **INV-PBT2B-04. Los testigos y las cuentas, sobre `CurrentSha`** (testigos ASCII, D-PBT2-09):
    - siguen byte a byte S-7, S-9, las cinco sentencias de CT-22 en su orden y los literales de CT-23;
    - siguen los testigos `sin confirmar que se perder`, `Corrige los datos: no se puede insertar un modelo inv` y `Corrige los datos: no se puede dibujar un modelo bloqueado.`;
    - cada testigo de las once constantes aparece en W;
    - las dos firmas públicas de los constructores no cambian;
    - W tiene una sola `.RequestUpdate(`;
    - ninguna línea añadida de W o del archivo del modo contiene `forceRebuild`, `KeyDown`, `PreviewKeyDown`, `InputBindings`, `KeyBinding`, `TabIndex`, `IsTabStop`, `KeyboardNavigation`, `.Focus(` ni `DialogResult`;
    - ninguna línea con `SetNumber(` se añade ni se quita;
    - `LoadFromModel`, `ReadInputs`, `Recompute` y `OnClosing` no cambian;
    - el archivo del modo solo declara el `enum`.
  - **INV-PBT2B-05. CLASSIC sin cambio:** T1B, PB-T0, PB-TC, PB-TC2, PB-T1a (con las huellas de CT-PBT1A-14 y F-03), PB-T1b y PB-T1c, en verde, en local y en la CI.
  - **INV-PBT2B-06. Las costuras:** las 41 de hoy [T1c], con su nombre y su tipo; solo se añaden `ModeForTest`, `BomDialog` y `LibraryPathDialog`.
  - **INV-PBT2B-07c. D2:** CT-PBT2B-05 y 09 en verde; CT-PBT1C-02 y 04 y CT-PBT1B-14 y 15, en verde sin tocarlas; ninguna línea añadida asigna el `Name` de un sistema ni muta una línea base o un informe.
  - **INV-PBT2B-08b.** La única escritura en disco de la red es `ArchivoTemporal` (v2.3, V23-07).
- **`StopConditions`:** las 17 estándar, C08, C11, C12, C17, las comunes, y:
  - **PBT2B-C04:** a method of a filter with `ExpectRed` passes over the inert seam, or fails by an exception.
  - **PBT2B-C13** (v2, V2-26): keeping a CLASSIC gesture without observable change, or doing the WORKSPACE one, would require touching a member outside INV-PBT2B-03, WX, WC, Application or the Plugin, or changing `LoadFromModel`, `ReadInputs`, `Recompute`, `OnClosing` or the order of a load; or doing the task would require adding, in W or in `PushBackEditorMode.cs`, a member declaration with a name that does not exist in `BaseSha` (of any visibility: method, property, field, constant, enum member or nested type) other than `mode`, `draftPort`, `appliedState`, `BomDialog`, `LibraryPathDialog`, `ModeForTest`, `ForDraft`, `ApplyModeChrome`, `RefuseInWorkspace`, `ApplyEditsToEditingState`, `ApplyToDraft`, `HasUnappliedDraftChanges`, `WorkspaceApplyLabel`, `WorkspaceApplyToolTip`, `WorkspaceApplyBlockedToolTip`, `WorkspaceRefusalMessage`, `WorkspaceLoadRefusalMessage`, `WorkspaceInvalidMessage`, `WorkspaceBlockedMessage`, `WorkspaceNothingToApplyMessage`, `WorkspaceAppliedMessage`, `WorkspaceNotEditableMessage`, `WorkspaceUnappliedQuestion`, `PushBackEditorMode`, `Classic` and `Workspace`. The private constructor `RackPushBackSystemWindow(PushBackDraftEditorLaunch)` is the only new constructor. The Worker stops before adding it.
  - **PBT2B-C14:** a fingerprint of CT-PBT1A-14 changes, or a CLASSIC test fails. STOP (C03).
  - **PBT2B-C15:** CT-PBT2B-02 gives a difference outside N-PBT2-01, `diseno-igual=False`, or a design without a branch in step 1. It is a new fact: return to the Owner (OB-06.5, D-PB-03).
  - **PBT2B-C16:** a gesture would need a real modal, the profile or the disk outside `ArchivoTemporal`, or the mode probe is not enough to avoid it.
- **`ExpectedEvidence`:** la común, y:
  - el paso 1;
  - la tabla «CT → la aserción que falla en el RED»;
  - la salida de INV-PBT2B-03 y 04 sobre el diff;
  - las autopruebas de CT-08 a 10;
  - en local, la clase de huellas de PB-T1a tres veces en verde sobre el GREEN.
- **Línea base de la CI:** la del cierre de PB-T2a, **prevista**: Core 19 092, UI 2 439. **GREEN de PB-T2b:**
  - Core + 2 (CT-08 y 09) = **19 094**;
  - UI + 8 (CT-01 a 07 y 10) = **2 447**, con 21 `Skipped` y 2 426 `Passed`;
  - si Q-PBT2-3 no es la opción A, UI 2 446.

### 6.3 Contrato de PB-T2c — `F4B-PBT2C-WORKSPACE-CARRYOVER`

- **Objective** (ASCII): «Fijar con pruebas nuevas, sin cambiar el producto, las obligaciones heredadas por el modo WORKSPACE de la ventana de Push Back: el paso 4 de D-PB-06 (los valores exactos y el estilo de cotas llegan al borrador), el aviso de D-PB-08 en WORKSPACE y el censo de controles en el camino al borrador, segun el paquete del contrato de PB-T2 v<n> y la lista cerrada CT-PBT2C-01..03. El RED son las pruebas nuevas y los tres mutantes de la tabla cerrada MU-T2C-01..03 en W y WC; el GREEN revierte los mutantes. Ninguna linea de src cambia entre BaseSha y CurrentSha; no se ejecuta AutoCAD. Si hiciera falta mutar el rack resuelto o el informe de modulos dentro del tiempo de vida del borrador: STOP y se reabre L-1 (decision D2 del Owner, F4B-E11).»
- **`AllowedWriteScope`** (3): W y WC (solo los mutantes del RED), y `tests/RackCad.UI.Tests/PushBackWorkspaceCarryoverTests.cs`.
- **`ForbiddenWriteScope`:** 60 de `src/` (59 hoy, más el archivo del modo de PB-T2b) y lo común, con las clases de PB-T2a y PB-T2b.
- **`Authorities`:** las comunes, y W y WC como `UNIT_CHANGE` («solo los tres mutantes del RED, revertidos por el GREEN»).
- **`RequiredTests`:**

  | Proyecto | Filtro | `MinSelected` | `ExpectRed` |
  |---|---|---|---|
  | UI | `PushBackWorkspaceCarryoverTests` | **3** | true |
  | UI | `PushBackWorkspaceModeTests`, `PushBackWorkspaceApplyCloseTests`, `PushBackFieldValidationConcordanceTests` (PB-T2b) | 3, 4, 1 | false |
  | Core | `PushBackWorkspaceModeSourceGuardTests`, `PushBackWorkspaceSharedReferenceGuardTests` (PB-T2b), `PushBackDraftStatePortTests`, `PushBackDraftPortSharedReferenceGuardTests` (PB-T2a) | 1, 1, 3, 1 | false |
  | UI y Core | las familias de PB-T1, PB-TC, PB-TC2, PB-T0 y T1B del §6.2 | los del §6.2 | false |

  En el RED, los filtros sin `ExpectRed` solo pueden fallar por los mutantes, con la tabla de atribución. En el GREEN, todos en verde.
- **Invariantes propios:**
  - **INV-PBT2C-01. Alcance:** `git diff --name-only BaseSha..CurrentSha` = la clase nueva; `git diff BaseSha..CurrentSha -- src` vacío.
  - **INV-PBT2C-02. RED y GREEN:** el RED son la clase y los tres mutantes, juntos; el GREEN es `git restore --source=BaseSha` de W y WC, y no toca `tests/`. El criterio: `Failed == Selected >= 3` en el filtro con `ExpectRed`, por aserción.
  - **INV-PBT2C-03. Cada mutante, solo:** aplicado aislado sobre `BaseSha`, hace fallar por aserción el CT de su fila. La entrega lo transcribe con el TRX de cada corrida temporal.
  - **INV-PBT2C-04. La atribución:** cada prueba existente que falle en el RED se atribuye a un mutante.
  - **Nota (no va al contrato).** Es el régimen de PB-T1a. MU-T2C-03 hace fallar CT-PBT2B-08 en Core, y por eso Core falla en el RED (§16.9, n.º 9).
- **`StopConditions`:** las 17 estándar, C08, C11, C12, C17, las comunes, y:
  - **PBT2C-C04:** a mutant of the table, isolated, does not make the CT of its row fail by an assertion; or a new CT fails on `BaseSha` without mutants; or it fails by an exception. The Worker changes neither the CT nor the mutant.
  - **PBT2C-C13** (v2, V2-26): doing the task would require changing W or WC other than by the three single-line substitutions of the closed table, or adding there any declaration, or keeping any of them in the GREEN.
  - **PBT2C-C15:** CT-PBT2C-01 does not give the exact value or the style on `BaseSha`. It is a new fact: return to the Owner (OB-06.4, OB-06.5).
- **`ExpectedEvidence`:** la común, y:
  - la tabla de atribución;
  - los tres mutantes aislados;
  - el RED con su CI (`tests` y `ui-tests` en `failure`, `build-ui` en `success`, `build-plugin` omitido).
- **Línea base de la CI:** la del cierre de PB-T2b, **prevista**: Core 19 094, UI 2 447. **GREEN de PB-T2c:** Core **19 094**; UI + 3 = **2 450**.

## 7. Riesgos y lecciones aplicadas

| Id | Riesgo o lección | Tratamiento |
|---|---|---|
| R-PBT2-01 | **Sin latitud para el Worker** | Las APIs, las sentencias, los ayudantes, el cuerpo del constructor, los literales, las líneas esperadas, las ramas del paso 1 y los mutantes están en el paquete; las listas de miembros son cerradas y las paradas, por nombre |
| R-PBT2-02 | **Corridas temporales con `Assert.Fail`** | INV-10; el paso 1 de PB-T2b; los mutantes aislados de PB-T2c |
| R-PBT2-03 | **El aviso de descarte sustituido** | §5.0 e INV-08; `PromptProbe` con «Sí» en el `finally` |
| R-PBT2-04 | **Nunca la suite completa de UI en local** | INV-10 |
| R-PBT2-05 | **MAX_PATH** | `TaskId` de 26, 31 y 29 caracteres; `ArchivoTemporal` corto |
| R-PBT2-06 | **El tamaño del contrato y del Worker** (F4B-E21; IR1 M-13) | La sesión pone las colecciones opacas. El trabajo se reparte en tres subtareas (V2-21) |
| R-PBT2-07 | **Un modal real en el RED colgaría la CI** | Ventanas sin mostrar; costuras; la sonda del modo; `--blame-hang`; PBT2B-C16 |
| R-PBT2-08 | **Escribir en el perfil** (`PromptSaveToLibrary`; la disposición del configurador) | Ninguna prueba nueva los llama; INV-08; L-PBT2-11. La disposición que escriben las pruebas existentes se atribuye con la regla del invariante 10 (v2.3, V23-01) |
| R-PBT2-09 | **PB-T1c estaba emitida pero no cerrada** (F4B-E27; cerrada en F4B-E31) | Las anclas [T1c] se volvieron a medir sobre su cierre (v2.3, V23-03). La reasignación de `editable` se comprobó en los anclajes de PB-T2a (F4B-E32); si falla, STOP y una v3 con una subtarea propia (V2-05). Sin plan B dentro de PB-T2a |
| R-PBT2-10 | **El punto fijo de la carga** se razona por lectura | CT-PBT2B-02 lo mide, con filas de selección, de nombre y de un estado inválido; el paso 1 fija las ramas; PBT2B-C15 |
| R-PBT2-11 | **El coste de `Equals`** (M15) en «Aplicar» y «Cerrar» | Una o dos comparaciones por clic. Se mide en Smoke-PB; el `IsDirty` caliente es de PB-T4 |
| R-PBT2-12 | **Una lista cerrada de miembros sin sus ayudantes** (M10) | INV-PBT2A-03 e INV-PBT2B-03 los nombran; PBT2x-C13, por nombre |
| R-PBT2-13 | **La CI del RED con Core en `failure`** (M14) | §6.0: `build-plugin` omitido en los tres RED |
| R-PBT2-14 | **Una corrección con rutas `UNIT_CHANGE` ya cambiadas en la base** | F4B-E25, citada como autoridad (§6.0) |
| R-PBT2-15 | **El `Trailer` de la entrega** (M14, la mitad (b)) | `ExpectedEvidence` común |
| R-PBT2-16 | **Los testigos con tildes en un contrato ASCII** (PBT1CV28R-I01) | Testigos ASCII (D-PBT2-09) |
| R-PBT2-17 | **nc1 no satisface el sustituto**, como en PB-TC y PB-TC2 | C11 al Owner |
| R-PBT2-18 | **Las celdas vencen el 2026-12-29**, con PB-T1c y tres cadenas por delante | A7 en cada emisión y delegación |
| R-PBT2-19 | **La validación del campo de escala del panel sigue duplicada** hasta PB-T4 | CT-PBT2B-10; Q-PBT2-3; A2-6 |
| R-PBT2-20 | **«Restaurar» con un estado inválido** | Desde la v2 queda deshabilitado (V2-22); CT-PBT2B-02 |
| R-PBT2-21 | **`FrontalSideBox` queda sola**, con su tooltip de hoy, en la barra de un compuesto | L-PBT2-4; se informa al Owner |

## 8. Las decisiones del Owner

### 8.1 Ya tomadas, que este paquete aplica

- **[OD], fila de PB-T2** (literal, al principio) → D-PBT2-06, D-PBT2-07 y D-PBT2-11.
- **[OD] D-PB-01:** «The mode must be explicit», «Do not let a Workspace invocation accidentally execute the classic drawing-write path» y «If changing the button label in Workspace mode improves clarity, that is allowed only if the package declares it and RACKEDITAR mode remains unchanged» → D-PBT2-04, D-PBT2-08 y D-PBT2-09.
- **F4B-E23** (literal: «BOM y biblioteca (Recomendado)») → D-PBT2-10, CT-PBT2B-03 y 07, y las dos costuras, con S-7, S-9 y CT-22 en verde: sin vuelta.
- **F4B-E24** («Basta así (Recomendado)») → L-PBT2-6.
- **F4B-E25** («Regla F4B y reverificar (Recomendado)») → el §6.0, «Authorities» y «Las correcciones».
- **[OD2] D2** (F4B-E11) → D-PBT2-14, C17 y sus CT.
- **[OD] D-PB-10** («Each separately contracted PB task starts at: 1 / 3») → cada subtarea, en 1/3.
- **[OD] §7.3** → §4, sin sustituciones.
- **F4B-E06 y F4B-E09** → CT-PBT2C-01 y 02.
- **[DP]** → C08 y C11; **F4B-E19** → P-01 con la Snapshot J; **F4B-E21** → la delegación.
- **F4B-E22 y F4B-E26, los arrastres de PB-T1b** (v2, V2-06; IR1 I-06):
  - **M09 (A-7):** `PushBackEditableStateOwnership.Of` decide «declarada» por la tabla y no por el tipo (`:109-112` en `a0a5f699`, igual en `a18ba345`). **Se arrastra a PB-T3,** cuya composición de D-PB-05 usa el mapa para saber qué sale del borrador y qué del rack actual: su contrato añade una prueba de que toda propiedad declarada de los tres documentos tiene fila, o declara la equivalencia (A2-12). PB-T2 no usa el mapa;
  - **M11 (A-8):** no se ejerce (HT2-26);
  - **M12 (A-9):** `RackModuleCommit.SameAs` compara los `double` con `!=` (RMES:102 y :136; PESC:228). Un `NaN` dentro de un commit hace que el estado no sea igual a sí mismo. **Limitación declarada en PB-T2:** «Aplicar» lo vería como un cambio y llamaría al puerto, del lado prudente; ninguna caja da `NaN` (`NumericFieldValidation` lo rechaza). **Nota para el `IsDirty` de PB-T4** (A2-13);
  - **M15 (A-10):** el coste de `Equals`; R-PBT2-11 y PB-T4.

### 8.2 Las decisiones nuevas para el Owner

Las cuatro son reales y ninguna estaba decidida (IR1 §6). Cada una está lista para AskUserQuestion: el texto es autocontenido y tiene menos de 400 caracteres, el `header` tiene 12 como máximo, y la opción recomendada va primero. Los recuentos están en el registro Q-6.

**Q-PBT2-1. El ciclo «Aplicar al borrador» / «Cerrar» en WORKSPACE** (material; reescrita en la v2, V2-08; el matiz de N-04 en la v2.1, V21-04).
- **header:** `PBT2 cierre`
- **question:** «PB-T2, el editor abierto desde el panel: «Aplicar al borrador» guarda en el borrador sin dibujar. En RACKEDITAR, «Actualizar» guarda y cierra. Avisar al cerrar de lo no aplicado sería el primer aviso sobre todo lo editado; hoy solo avisa de un módulo sin confirmar, y el código evita a propósito ese aviso general (ADR-0029 D8). ¿Qué hacen «Aplicar» y «Cerrar»?»
- **options:**

  | label | description |
  |---|---|
  | Aplica sin cerrar; Cerrar avisa (Recomendado) | Tras aplicar se puede seguir, ver el BOM o guardar. Al cerrar con algo sin aplicar pregunta: «Hay cambios sin aplicar al borrador que se perderán al cerrar. ¿Deseas continuar?». Lo tecleado sin salir de la caja se pierde sin aviso con Esc, la X o Alt+F4. |
  | Aplica y cierra, como Actualizar | Un solo paso, como en RACKEDITAR; para seguir hay que reabrir «Editar todo». Cerrar descarta sin preguntar, como hoy, y Esc pierde lo no aplicado sin aviso. |
  | Aplica sin cerrar; Cerrar no avisa | Como el clásico al cerrar: lo no aplicado se pierde sin preguntar. Con un paso de aplicar aparte, un Esc por error pierde más trabajo. |

- **Lo que cambia con cada una:**
  - **(A)** es D-PBT2-06 y D-PBT2-07, con CT-PBT2B-06 tal cual;
  - **(B):** `ApplyToDraft` termina con `Close()`; `PendingWork` no gana ámbito; CT-03, 04 y 06 se rehacen;
  - **(C):** `PendingWork` no cambia y CT-06 espera `avisos=0`.
- **Por qué (A), con su coste dicho:** separar los dos pasos crea una forma nueva de perder trabajo, porque aplicar se puede confundir con guardar y cerrar. Pero (A) introduce, solo en WORKSPACE, un dirty sobre todo el estado editable. La lectura de ADR-0029 D8 que está en el código lo evita (W:3336-3341; `RichEditorCloseContractTests.cs:77`). Lo acota la frontera explícita del borrador. Por eso decide el Owner.

**Q-PBT2-2. La compuerta de «Aplicar al borrador» con un diagnóstico bloqueante** (material).
- **header:** `PBT2 bloqueo`
- **question:** «PB-T2: ¿«Aplicar al borrador» acepta un diseño con un diagnóstico bloqueante, por ejemplo una celda que no cabe? En RACKEDITAR, «Actualizar», el BOM y la biblioteca lo rechazan y muestran el motivo.»
- **options:**

  | label | description |
  |---|---|
  | Lo rechaza, como Actualizar (Recomendado) | El borrador nunca guarda un diseño que no se puede dibujar: se corrige en la ventana. Es la misma compuerta en los dos modos. |
  | Lo acepta | El borrador puede guardar un diseño a medias; el «Actualizar» del panel lo rechazará al escribir (PB-T3). Los campos inválidos se siguen rechazando. |

- **Lo que cambia con (B):** `ApplyEditsToEditingState` deja de mirar la compuerta de salida en WORKSPACE, así que deja de ser el mismo paso; R11 se ajusta; CT-05 espera `llamadas=1` en `bloqueante`; y PB-T3 hereda el bloqueo (A2-7).
- **Por qué (A):** es el mismo paso en los dos modos, que es lo que pide la frase de la validación única.

**Q-PBT2-3. El alcance de «una sola validación de campos»** (material: es una lectura de la frase del Owner).
- **header:** `PBT2 valid.`
- **question:** «PB-T2: pides una sola validación de campos. La ventana usará la misma en los dos modos, pero el campo de escala del panel valida con su propia copia de esa regla (mismos textos, mayor que 0). ¿La unifico ya o al conectar «Editar todo» (PB-T4)?»
- **options:**

  | label | description |
  |---|---|
  | En PB-T4, con prueba ya (Recomendado) | PB-T2 añade una prueba de que las dos dan lo mismo. PB-T4, que rehace ese campo sobre el estado nuevo, deja una sola. |
  | Ya en PB-T2 | La regla común de las cajas pasa a Application y el campo del panel la usa. Toca el control numérico de todos los editores y el piloto del panel (Pilot/ y sus guardas), que el plan dejaba para PB-T4. Ningún texto cambia. |
  | No hace falta | Son la misma regla; no se toca el panel. |

- **Lo que cambia con cada una:**
  - **(B)** (v2, V2-24): PB-T2a gana el traslado de `NumericFieldValidation` a Application, con un reenvío en UI, y el cambio del adaptador del piloto. Cambian su alcance (`Pilot/` y las guardas del piloto, que D-PBT2-15 y [P] dejaban para PB-T4), su RED y su contrato, y Cantilever y todos los editores entran en la red;
  - **(C):** CT-PBT2B-10 sale.
- **Por qué (A):** PB-T2 no crea ninguna validación nueva (R11 lo fija). El duplicado es anterior a F4B y está en el campo que PB-T4 rehace.

**Q-PBT2-4. La emisión condicional** (procedimiento).
- **header:** `PBT2 emisión`
- **question:** «PB-T2 se divide en tres: PB-T2a (el puerto al borrador), PB-T2b (la ventana en dos modos) y PB-T2c (las pruebas heredadas en ese modo). ¿Emito cada una sin volver a ti si su revisión da 0 bloqueantes y 0 importantes sin resolver y la remedición sobre PB-T1c no cambia nada material, como con PB-TC2 y PB-T1?»
- **options:**

  | label | description |
  |---|---|
  | Sí, con esa condición (Recomendado) | Vuelvo a ti por una decisión nueva, un control con firma no prevista, un intento 3 o un efecto material sin medir. |
  | No, vuelve antes de cada una | Te presento cada contrato antes de emitirlo. |

- **Por qué se pregunta:** [OD] dio a PB-T2 «ACCEPT IN PRINCIPLE», sin la autorización condicional que F4B-E09 (pregunta 4) dio a PB-TC2 y PB-T1.

### 8.3 Lo que no es decisión (lecturas declaradas)

- **L-PBT2-1. La división** en tres subtareas: la prevén OB-10.1 y D-PB-10. Se informa en Q-PBT2-4.
- **L-PBT2-2. N-PBT2-01:** una normalización sin cambio de salida, como reabrir con RACKEDITAR. Se informa al Owner (§8.4), como la de `pb-15` (F4B-E20).
- **L-PBT2-3. El dueño de los subdiálogos:** la ventana en los dos modos (D-PBT2-12). La enmienda de [P] va en A2-10.
- **L-PBT2-4. `FrontalSideBox`** sigue visible en WORKSPACE, con su tooltip de hoy («De qué lado son los dos cortes frontales de al lado»), aunque esos botones estén ocultos. No escribe, elige el lado de la vista previa y no se añade ningún literal. Se informa al Owner.
- **L-PBT2-5. La prueba de la biblioteca escribe** el archivo en una carpeta temporal propia: lo exige F4B-E23.
- **L-PBT2-6. El campo desconocido de la prueba** es del nivel del proyecto. Con AUTH-13, el panel no abre hoy un rack con campos desconocidos (F4B-E24): la prueba usa un lanzamiento fabricado.
- **L-PBT2-7. Los once literales** los declara el paquete. [OD] solo permite declarar el rótulo; los demás son textos de estado, y la pregunta de cierre va con Q-PBT2-1. Se dan literales al Owner (§8.4) (v2, V2-11).
- **L-PBT2-8. Las cargas públicas lanzan en WORKSPACE:** es un error de programación, y ningún gesto llega a ellas.
- **L-PBT2-9. «Restaurar»** sigue en WORKSPACE, y con un estado inválido queda deshabilitado (V2-22).
- **L-PBT2-10. El texto escrito sin salir de la caja** no cuenta como «sin aplicar» con Escape, la X o Alt+F4; sí con un clic en «Cerrar». Está en la opción (A) de Q-PBT2-1.
- **L-PBT2-11** (v2, V2-13). **La disposición del configurador** se escribe en el perfil al cerrarse, en WORKSPACE como en CLASSIC (T-SG CT-22b). Es una tercera salida a disco: una preferencia de pantalla, no un dato del rack, que F4B-E23 no trató («no se pregunta aquí», el §5 de su vuelta). Las pruebas no abren el configurador real.
- **L-PBT2-12** (v2.1, V21-01; IR1-R N-01). **«Guardar en biblioteca» normaliza el nombre.** `SaveLibrary_Click` conserva `SetName(NameBox.Text?.Trim())` (W:3389), un literal de CT-22 que no se puede quitar, con el `SyncName()` que le añade PB-T1c. Solo corre si el diálogo devuelve una ruta (W:3384). En WORKSPACE, con un nombre nulo o con espacios en los extremos, el guardado deja el estado de la ventana con el nombre recortado (o vacío). Desde ese momento, «Cerrar» pregunta por cambios sin aplicar y «Aplicar» lleva ese nombre al borrador. El borrador no cambia hasta «Aplicar», así que F4B-E23 sigue cumplida, y CLASSIC hace hoy lo mismo con su identidad. Lo fija CT-PBT2B-04, y se informa al Owner (§8.4).
- **L-PBT2-13** (v2.3, V23-02; F4B-E31, OWNER-01). **«Restaurar» no recalcula antes de leer el diseño**, en WORKSPACE como en CLASSIC (`Restore_Click`, W:3192-3213; igual en la base). Tras `GapBox`, el separador central o la estructura propia, el diseño de `LastComputation` del compuesto va una pasada por detrás, y «Restaurar» lo leería así. Ningún CT de PB-T2 ejerce «Restaurar» tras esos gestos, y CLASSIC no cambia (D-16): no se corrige aquí. El Owner ya lo conoce (OWNER-01, F4B-E31).

**Decisiones materiales nuevas: las cuatro del §8.2.** Ninguna otra.

### 8.4 Para informar al Owner, sin pregunta (v2, V2-11)

Va en la misma vuelta que Q-PBT2-1 a 4.
- **N-PBT2-01.** Al abrir un borrador en el editor del panel, la ventana vuelve a adoptar dos datos desde la salida del propio estado, sin cambiar lo que se dibuja:
  - si el usuario había tocado las casillas de cotas por vista, queda guardada la política resultante en lugar de «tocadas»;
  - una selección de seguridad que el ensamblador no admite no se conserva, y tampoco llegaba nunca al dibujo.

  Es lo mismo que pasa hoy al reabrir con RACKEDITAR. Abrir no toca el nombre del rack. CT-PBT2B-02 lo fija.
- **El nombre y la biblioteca** (v2.1, V21-01; L-PBT2-12). Si el rack tiene un nombre vacío o con espacios en los extremos y se guarda en la biblioteca desde el editor del panel, el guardado deja el nombre recortado. Desde ese momento el editor lo trata como un cambio: «Cerrar» avisa, y «Aplicar» lo lleva al borrador. El borrador no cambia hasta «Aplicar».
- **Los once literales** del modo WORKSPACE, tal como irán en el código: la tabla de D-PBT2-09, con el texto entero. CLASSIC no cambia ningún rótulo.
- **`FrontalSideBox`:** su tooltip se queda como está (L-PBT2-4).
- **La tercera salida a disco:** la disposición del configurador (L-PBT2-11).

## 9. Límites

Lo que no se pudo medir está en el registro, apartados P y Q. En resumen:
- **Nada se compiló ni se ejecutó.** Las líneas esperadas salen de oráculos existentes (CT-PBTC-02, 05 y 07, CT-PBT1B-12, CT-PB0-12 a 16, T-CL y T-SG) o son literales. Las ramas del paso 1 y los mutantes aislados los mide el Worker.
- **PB-T1c, cerrada (F4B-E31):** las anclas de W y WC se remidieron en la v2.3 sobre `0dc623ca` (V23-03), y WX no cambió. Los gestos de CT-PBT1C-02 (para CT-PBT2C-03) son los de su clase cerrada.
- **Los valores de `descartes`** (V21-02) salen de la regla del §5.0 y de los oráculos de cada gesto, por lectura. Si una ventana da otro, el Worker para (C07). **PB-T1b está cerrada** (F4B-E26), y G-L1c existe y se copia (V2-01; corrige la v1).
- **Por lectura, sin medir:**
  - que `Close()` sobre una ventana sin mostrar pase por `OnClosing` (lo usan las pruebas de PB-TC2 y PB-T1a);
  - el punto fijo de la carga en WORKSPACE, con selección y con nombre;
  - los valores de N-PBT2-01a, y si algún diseño del corpus trae seguridad no autorizada;
  - el coste de `Equals` por llamada;
  - el mensaje exacto de `ArgumentNullException` en .NET 8 («Value cannot be null. (Parameter '…')»), que el Worker confirma en el RED;
  - que los mutantes de PB-T2c, aislados, hagan fallar su CT; lo mide el Worker.
- **Los recuentos son estáticos** (registros M y Q-4). La línea base de la CI de PB-T1b está **medida** (`44438060317`); las siguientes, **previstas**.
- **No medido aquí:** el `config.toml` frente a la Snapshot J, el binario acreditado y la vigencia de las celdas.
- **El worktree de la rama no se leyó.** Toda lectura fue por objetos git desde el checkout principal.
- **Dos incidencias de procedimiento** (registro, cabecera):
  - `python3 -c "pass"`, en la v1;
  - `python -I -c "import sys"`, en la v2, al final de una tubería de `git show`.

  Ninguna tuvo efecto. Incumplen la regla «Python nunca con `-c`», y no se repitieron.

## 10. Arrastres que salen de PB-T2

| Id | Qué | Va a |
|---|---|---|
| A2-1 | **Abrir la ventana** (v2, V2-23; IR1 M-15): PB-T4 no entrega a la ventana el puerto de `LaunchFor` tal cual. Lo envuelve en el host, para que cada «Aplicar» pase por el host y acabe la marca *known clean* (`NotePilotEdit`, `WorkspaceHostCore.cs:66`, `:832`; `WorkspaceLifecycleGuardTests.cs:509-520`), y declara la excepción de portador, como `TryUpdatePilot` (`WorkspaceEditIntentHostTests.cs:1039-1046`). También pone el dueño de la ventana, la muestra modal (OB-01.4) y saca el proyecto interior de la lectura | PB-T4 |
| A2-2 | La guarda de texto de OB-01.3 sobre el abridor de «Editar todo» | PB-T4 |
| A2-3 | El texto pendiente del panel antes de abrir, y K = 1 (R-PB-10) | PB-T4 |
| A2-4 | El borrador del panel como `DraftSession<PushBackEditableState>`, y el coste de `IsDirty` (M15) | PB-T4 |
| A2-5 | D2, condiciones 3 (en el `DraftSession` del panel), 5, 6 y 7 | PB-T4 |
| A2-6 | Según Q-PBT2-3 (A): una sola regla para el campo de escala del panel; CT-PBT2B-10 sigue en verde mientras existan las dos | PB-T4 |
| A2-7 | Según Q-PBT2-2: con (B), el PREPARE de PB-T3 rechaza un borrador bloqueado | PB-T3 |
| A2-8 | M11: un host que llame a `LoadDesignEditor` o `RestoreEditor` fuera de la ventana se guarda del diseño nulo | PB-T4 |
| A2-9 | El resumen y «Modificado» del panel tras cerrar «Editar todo» | PB-T4 |
| A2-10 | **Las enmiendas de [P]** en su próxima versión: la nota de F4B-E24 (la línea ~314 y la promesa de OB-05.2) y, desde la v2 (V2-12), **el dueño de los subdiálogos en §4 y §5**: «la ventana, en los dos modos; el host decide el dueño de la ventana» (D-PBT2-12) | la sesión |
| A2-11 | **El paso 4 de D-PB-06 hasta el DWG** (v2, V2-14): que el valor exacto y el estilo lleguen al dibujo por PREPARE → MUTATE → POST desde el Workspace | PB-T3 (la composición), PB-T4 (de punta a punta desde «Editar todo») y Smoke-PB |
| A2-12 | **M09** (v2, V2-06): una prueba de que toda propiedad declarada de los tres documentos tiene fila en el mapa, o la equivalencia declarada | PB-T3 |
| A2-13 | **M12** (v2, V2-06): con un `NaN` en un commit, `IsDirty` no vuelve a `false`; se declara en su contrato | PB-T4 |
