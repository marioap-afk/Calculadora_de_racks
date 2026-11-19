# I-64 F4B — PB-T2: de la v1 a la v2 del paquete (hallazgo por hallazgo)

> **Redacción para el repo público:** sin rutas locales ni nombres de usuario, de máquina o de clientes.

**Entrada:**
- la revisión adversarial independiente de la v1 (`independent-review-v1.md`, **IR1**), con el veredicto **NECESITA v2, 0 BLOQUEANTES / 8 IMPORTANTES / 16 MENORES**;
- el mensaje de la sesión: la base nueva `95373af6`, con PB-T1b CERRADA; F4B-E23 y F4B-E25; las lecciones de PB-T1b; y la forma de AskUserQuestion para Q-PBT2-1 a 4.

**Salida:**
- `I-64-f4b-pbt2-gate-contract-package-v2.md`, con el registro de cambios V2-01 a V2-32 al principio;
- `pbt2-measurements.md`, al día, con el apartado Q nuevo;
- este archivo.

**Resultado:** los **24** hallazgos de IR1 están aplicados: **0 B, 8/8 I y 16/16 M**. Ninguno se deja sin aplicar. En siete se eligió una de las correcciones que IR1 ofrecía, o una variante, y aquí se dice cuál y por qué: I-04, I-05, I-07, M-11, M-12, M-13 y M-15. Las dos notas de IR1 fuera de la tabla (§2 y §3) también se aplican.

**Lo que no cambia** (IR1 §9, «Lo que no hay que rehacer»):
- el diseño de fondo: los dos modos, el puerto, la carga por copia, la guarda, las costuras y la ocultación;
- las anclas de producto, salvo las de M-10;
- las guardas de T1B, en verde y sin sustituir ninguna.

La división pasa de dos subtareas a tres (M-13).

## 1. Los IMPORTANTES

| Hallazgo | Qué decía IR1 | Cambio en la v2 | V2 |
|---|---|---|---|
| **I-01** | La guarda G-L1 de PB-T2 cerraba la lista en **trece** autopruebas, sin (k1) ni (k2), «alias + molde». La corrección de PB-T1b las añadió para discriminar su I01. El §9 y el registro P decían que la versión cerrada «hoy no existe» | CT-PBT2A-04 y CT-PBT2B-09 nombran las **quince**: (a), (b), (c), (e), (f1), (f2), (f3), (g1), (g2), (h1), (h2), (i), (j), (k1) y (k2), con `autoprueba-k1=falla` y `autoprueba-k2=falla`. Sus textos vienen de G-L1c (`95373af6`, igual a `a18ba345`), y las reglas, `Rebind`, `Members`, `Aliases`, `Violations` e `Inject`, de esa versión. El §9 y el registro P ya no dicen que «hoy no existe». `AllObjects` (:278, con `skip`) y `Fingerprint` (:1080) quedan anclados. **Nota:** IR1 situaba las autopruebas en `a18ba345:…:238-253`; la medición, sobre el mismo archivo, da `:223-235` y `:238-239` (registro Q-3) | V2-01 |
| **I-02** | El §6.0 proponía una `AuthorityRevision` con los commits de documentación de la sesión, lo que choca con A6 y está superado por F4B-E25. Los contratos no declaraban sus `Authorities` por clase. Faltaban F4B-E23 a E26 en el §1 | **(a) Fuera la «lección»;** **F4B-E25** se cita como autoridad en el §1, en el §6.0 («Las correcciones», con sus tres condiciones) y en cada contrato. **(b) Cada contrato declara sus `Authorities`** por ruta, sección y clase: `UNIT_DOC`, `UNIT_CHANGE` y `EXTERNAL`. W es `UNIT_CHANGE` en PB-T2b; W y WC lo son en PB-T2c, solo para los mutantes; PB-T2a no tiene ninguna, porque sus dos archivos nuevos no existen en la `AuthorityRevision`. **(c)** D-PB-07 se cita como **F4B-E23**, y la `ExtensionData` anidada como **F4B-E24**, con su archivo custodiado (`coordinator/owner-f4b-dpb07-extensiondata-decisions.md`) y sus vueltas. F4B-E26 entra en el §1 como el cierre de PB-T1b. **La ruta de F4B-E25** es la que dio la sesión (`coordinator/owner-f4b-correction-unit-change-decision.md`), no la de IR1 (`pbt1b2/…`) | V2-02 |
| **I-03** | La UI-STA (CT-03) no pulsaba `FrontalSideBox` ni los gestos de los subdiálogos o de módulos, y R3 solo miraba W | **R3** mira todos los `.cs` de `src/RackCad.UI/Systems/PushBack`, con su autoprueba en **WC** (`session.RequestInsert(null, -1, ctx => null);` en un miembro nuevo). **CT-PBT2B-03** gana dos pasadas: la pasada 2 trae los gestos de CT-PB0-12, 13, 15 y 16, copiados, por sus costuras (el configurador confirmado y cancelado, la seguridad, la defensa y los topes); la pasada 3, sobre `pb-20-ab`, trae `FrontalSideBox`, `SideSelectorBox` y la confirmación y la cancelación de módulos. Todas con `borrador-igual=True llamadas=0`. **Las botas por poste** (CT-PB0-14, `AbrirFueraDePantalla`, registro Q-4) necesitan la ventana mostrada: se declaran no ejercidas y cubiertas por R3, en D-PBT2-08, CT-03 y el §5.5 | V2-03 |
| **I-04** | Nada discriminaba una segunda validación equivalente en `ApplyToDraft` | **R11** en CT-PBT2B-08: `ApplyEditsToEditingState(` se llama solo desde `RequestDraw` y `ApplyToDraft`, y ni `ApplyToDraft` ni `HasUnappliedDraftChanges` contienen `AllFieldsValid(`, `OutputBlockedAfterRecompute(`, `currentInputsAreValid` ni `HasError`. Lleva dos autopruebas: R11a, con una validación propia en `ApplyToDraft`, y R11b, con la llamada quitada de `RequestDraw`. **Variante:** se añade `HasError` a la lista de IR1, para cubrir una validación que lea las cajas directamente. D-PBT2-11 y la trazabilidad citan R11 | V2-04 |
| **I-05** | El plan B de R-PBT2-09 no era viable: se decidía tarde, tocaba un archivo de PB-T1b y chocaba con el copiador | **Se aplica la segunda opción de IR1: fuera el plan B, y queda solo la parada.** La comprobación de que ninguna guarda de PB-T1c impide reasignar `editable` pasa a los **anclajes de PB-T2a**, cuando PB-T1c ya está cerrada. Si falla: STOP y una **v3** del paquete con una subtarea propia y su censo. IR1 decía «una v2» porque revisaba la v1. PBT2B-C18 sale. R-PBT2-09 y HT2-25 se reescriben | V2-05 |
| **I-06** | A-7 (M09) y A-9 (M12) del §10 de [PT1] no aparecían | **A-7 (M09):** se arrastra a **PB-T3** (A2-12), con una prueba de que toda propiedad declarada de los tres documentos tiene fila en el mapa, o la equivalencia declarada, porque la composición de D-PB-05 usa el mapa. Ancla: `Ownership.Of` en `:82`, con la decisión en `:109-112`. **A-9 (M12):** limitación declarada en PB-T2: con un `NaN`, «Aplicar» llamaría al puerto, del lado prudente, y ninguna caja da `NaN`. Se arrastra a **PB-T4** (A2-13) por `IsDirty`. Anclas: RMES:102 y :136, PESC:228. Va en el §8.1 y el §10, con las anclas en `95373af6`, cuyo `src/` es igual al de `a18ba345` | V2-06 |
| **I-07** | `SetName` y `SyncName` en `ApplyToDraft` ensuciaban el borrador con un nombre nulo o con espacios | **Se aplica la primera opción de IR1:** `ApplyToDraft` ya **no** llama a `SetName` ni a `SyncName`, porque `Name_Changed` (en `TextChanged`, WX:47) ya lo hace en cada cambio. R4 prohíbe `SetName(` y `SyncName(` en `ApplyToDraft`, con su autoprueba. CT-PBT2B-02 gana las filas `nombre-nulo` y `nombre-con-espacios`, con `iguales=True` y el nombre tal cual. CT-PBT2B-04 gana las mismas, con `llamadas=0 sucio=False` tras «Aplicar» sin editar. D-PBT2-05 dice que el nombre no se normaliza: la siembra va bajo `suppressSync` | V2-07 |
| **I-08** | Q-PBT2-1 estaba mal planteada: negaba la tensión con D8, omitía la excepción de la caja sin confirmar y no daba el literal | Q-PBT2-1 se reescribe. **El texto** dice a las claras que (A) sería «el primer aviso de cambios sin guardar de todo el editor, algo que hoy el código evita (ADR-0029 D8)». **(A)** incluye el literal de `WorkspaceUnappliedQuestion` y la excepción de Esc, la X y Alt+F4. **(B) y (C)** dicen que Esc pierde lo no aplicado sin aviso. **«Por qué (A)»** nombra el coste, W:3451-3456 y `RichEditorCloseContractTests.cs:77`. D-PBT2-07 recoge la misma tensión | V2-08 |

## 2. Los MENORES

| Hallazgo | Qué decía IR1 | Cambio en la v2 | V2 |
|---|---|---|---|
| M-01 | CT-02 solo cargaba estados con la selección por defecto | CT-PBT2B-02 gana tres filas de reapertura hechas con `EstadoDeVentana`: `ambos-mixto` (`pb-20-ab`, «Ambos lados» con una celda mixta), `lado-B` y `celda-1-0` (`pb-08-redondeo`). En cada una, `iguales=True diseno-igual=True seleccion=…` | V2-09 |
| M-02 | CT-02 incluía `pb-21-cola`, fuera de la garantía | `pb-21-cola` sale de CT-PBT2A-03 y de CT-PBT2B-02. El corpus «C18» son los 17 de `Fixtures/I64` sin él, más X-01 y F-01, así que PBT2B-C15 ya no puede dispararse por ese diseño | V2-10 |
| M-03 | L-PBT2-7 extendía el permiso del rótulo; N-PBT2-01 tiene precedente (F4B-E20) | El §8.4 nuevo es información para el Owner, sin pregunta: N-PBT2-01 en lenguaje llano y los once literales enteros (la tabla de D-PBT2-09). L-PBT2-2 y L-PBT2-7 remiten a él | V2-11 |
| M-04 | D-PBT2-12 contradice la letra de [P] §4 y §5 | A2-10 añade la enmienda de [P] §4 y §5: «la ventana, en los dos modos; el host decide el dueño de la ventana». D-PBT2-12 lo cita | V2-12 |
| M-05 | La disposición del configurador en el perfil era una tercera salida a disco sin declarar | **L-PBT2-11:** es una preferencia de pantalla, no un dato del rack, y F4B-E23 no la trató. Está en D-PBT2-10, INV-08 (las pruebas solo usan `HeaderConfiguratorDialog`), R-PBT2-08 y el §8.4 | V2-13 |
| M-06 | El tramo de OB-06.3 hasta el DWG no se arrastraba | **A2-11:** el paso 4 de D-PB-06 hasta el DWG, por PREPARE → MUTATE → POST, va a PB-T3 (la composición), a PB-T4 (de punta a punta) y a Smoke-PB. D-PBT2-13 y la trazabilidad lo citan | V2-14 |
| M-07 | Ni el tipo ni el mensaje de las excepciones estaban fijados; la lectura de `LaunchFor` en el RED no iba dentro de «EXCEPCION» | Las sentencias quedan literales en D-PBT2-02 a 04: `ArgumentNullException(nameof(draft\|edited\|state\|port\|launch))` y `new ArgumentException("A rack id is required.", nameof(rackId))`, el texto de DR:145. Las líneas esperadas de CT-PBT2A-01 y 02 y de CT-PBT2B-01 (iv) dan el mensaje de .NET 8, «Value cannot be null. (Parameter '…')» y «A rack id is required. (Parameter 'rackId')», con `rackId` nulo, `""` y `"   "`. La lectura de `LaunchFor(...)` va dentro de «EXCEPCION», y en el RED da `EXCEPCION NullReferenceException: …`. El mensaje exacto lo confirma el Worker en el RED (§9) | V2-15 |
| M-08 | En el RED, un miembro ausente tenía que dar `False` sin lanzar; R3 necesitaba un partidor con nombre; el `finally` de CT-06 tenía que contestar «Sí» | **CT-PBT2B-08:** «un miembro que no existe da `regla-R<n>=False` y no lanza», y los miembros se parten con `Clean`, `Collapse` y `Members` de G-L1c, cada uno con el identificador anterior a su primer `(`. **CT-PBT2B-06:** el `finally` pone la sonda en «Sí» antes de cerrar, como `CloseQuietly` (T-CL:124-131, con `probe.Answer = true` en :126) | V2-16 |
| M-09 | CT-B-12 era formal; la condición 3 de D2 no se probaba en el camino del puerto | **CT-PBT2A-03:** las huellas de los no nulos de las cuatro referencias de D2, de `s` y del borrador, antes y después de `ApplyToDraft`, con `huellas-iguales=True`. **CT-PBT2B-09:** comprueba además que el archivo del modo, sin comentarios y colapsado, es solo el `enum` (`solo-el-enum=True`). En D-PBT2-14, la condición 3 queda «en el camino del puerto», y el resto sigue en PB-T4 | V2-17 |
| M-10 | Hechos imprecisos: G-10, WC:738-746, la fila de G9, y las anclas de `Fingerprint` y `AllObjects` | **G-10** mira todo el producto (`Product() => Src.AllProduct()`, :30), con su lista cerrada: §2.7, §4 y registro K. **`RefreshCompositePanel`** es WC:738-777, y siembra seis controles: registros C y Q-4. **G9** solo mira el producto, y la clase de concordancia es una prueba: §4 y registro K. **`AllObjects`** está en :278, con `skip`, y **`Fingerprint`** en :1080: §5.0 y registros K y Q-3 | V2-18 |
| M-11 | Tres filtros de PB-T2b quedaban «medidos en la emisión»; `ExpectedEvidence` no pedía el `Trailer` | **`MinSelected` fijados:** `PushBackRichPreviewTests` 9, `PushBackLibrarySaveGateTests` 5 y `DialogWindowCharacterizationTests` 23. **Variante:** se fijan ya, con el recuento estático, porque las tres clases no tienen ninguna `Theory` (registro Q-4). La remedición de la emisión los vuelve a comprobar. **El `Trailer`:** véase V2-28 | V2-19 y V2-28 |
| M-12 | El tooltip de `FrontalSideBox` queda falso en WORKSPACE | **Se aplica la segunda opción de IR1:** el tooltip se queda como está, dicho en L-PBT2-4 y R-PBT2-21, e informado al Owner en el §8.4. Así no hace falta un literal duodécimo. La caja no escribe: solo elige el lado de la vista previa | V2-20 |
| M-13 | El tamaño de PB-T2b frente al tope de 60 minutos del Worker no se valoraba | **Se valora y se divide.** El Worker de PB-T1b tardó **70 min** con 17 CT y el producto, y la corrección, **3 h 15 min** con una interrupción (registro Q-5). **Se crea PB-T2c** (`F4B-PBT2C-WORKSPACE-CARRYOVER`), solo de pruebas, con RED por tres mutantes de una línea (MU-T2C-01 a 03), como PB-T1a. Los CT-PBT2B-08 a 10 de la v1 pasan a ser **CT-PBT2C-01 a 03**, y los 11 a 13, **CT-PBT2B-08 a 10**. Llevan su contrato (§6.3), su `ForbiddenWriteScope` (59, o 60 tras PB-T2b; registro N), su tabla de atribución y su línea base prevista (Core 19 094, UI 2 450) | V2-21 |
| M-14 | Con un W0 inválido, «Restaurar» volvía al rack por defecto, y «Aplicar» lo llevaría al borrador | **Se aplica la primera opción de IR1.** El constructor privado anula `lastComputation`, `hasValidModel` y el modelo de la sesión antes de `LoadFromModel`. `RenderPreview` (W:3144) y `PlanFor` (W:3208) ya admiten un cálculo nulo (registro B). **CT-PBT2B-02** gana la fila `estado-invalido`: `valido=False restaurar-habilitado=False`, el texto de W:3311 y `llamadas-tras-aplicar=0`. Lo recogen L-PBT2-9 y R-PBT2-20 | V2-22 |
| M-15 | A2-1 mandaba el puerto a PB-T4 tal cual, sin pasar por el host | **Se aplica la primera opción de IR1:** PB-T4 envuelve el puerto en el host, para que cada «Aplicar» acabe la marca *known clean* (`NotePilotEdit`), y declara la excepción de portador, como `TryUpdatePilot`. No se elige la devolución de llamada en el puerto, porque cambiaría la API de PB-T2a sin necesidad | V2-23 |
| M-16 | La opción (B) de Q-PBT2-3 no decía que toca `Pilot/` y sus guardas | La descripción de (B) dice: «Toca el control numérico de todos los editores y el piloto del panel (Pilot/ y sus guardas), que el plan dejaba para PB-T4». «Lo que cambia con (B)» detalla el alcance, el RED y la red | V2-24 |

## 3. Las notas de IR1 fuera de la tabla

| Origen | Nota | Cambio | V2 |
|---|---|---|---|
| IR1 §2 (fila de F4B-E23) | `guid-en-archivo=False` no discrimina, porque el archivo de biblioteca nunca lleva el GUID | La comprobación sale de CT-PBT2B-03. La que discrimina es `id=GUID-PBT2-0001`, en D-PBT2-10 | V2-25 |
| IR1 §3 (G-10, WC:738-746, G9, las huellas) | Las cuatro diferencias de M-10 | Véase M-10 | V2-18 |
| IR1 §3 (CT-PBT1B-14) | La lista de quince y su ancla | Véase I-01, con el ancla medida (`:223-239`) | V2-01 |
| IR1 §4 («Por lectura, el plan B no hará falta») | La reasignación es segura por lectura | Va a HT2-25, y se comprueba en los anclajes de PB-T2a | V2-05 |
| IR1 §6 («¿Alguna estaba ya decidida? No») | Las cuatro preguntas son reales | Se conservan las cuatro. Ninguna decisión material nueva | V2-31 |

## 4. Lo que pidió la sesión

| Petición | Cambio | V2 |
|---|---|---|
| La base nueva: la punta `95373af6`, con PB-T1b CERRADA y `src` igual a `2f5d3dec` | El estado, el §2 y el §6 miden sobre `95373af6`. La línea base de la CI de PB-T1b queda medida (`44438060317`). El apartado Q del registro, nuevo | V2-30 |
| Leer IR1 entera y aplicar cada hallazgo con un registro de cambios con ids V2-xx | El registro de cambios al principio del paquete (V2-01 a V2-32, con la tabla de los ids de los CT) y este archivo | todos |
| Reformular Q-PBT2-1 | Véase I-08 | V2-08 |
| Completar la opción (B) de Q-PBT2-3 | Véase M-16 | V2-24 |
| Q-PBT2-1 a 4 listas para AskUserQuestion: texto autocontenido de menos de 400 caracteres, `header` de 12 como máximo, de 2 a 4 opciones, la recomendada primero y marcada «(Recomendado)» | En el §8.2, cada pregunta con `header`, `question` y una tabla `label` / `description`. Medido con `check_questions_v2.py` (registro Q-6): `PBT2 cierre` (11 caracteres de `header`, 376 de texto, 3 opciones), `PBT2 bloqueo` (12, 198, 2), `PBT2 valid.` (11, 243, 3) y `PBT2 emisión` (12, 307, 2). En las cuatro, la recomendada va primero y es la única marcada. Q-PBT2-4 menciona ya las tres subtareas | V2-31 |
| N-PBT2-01 y los once literales, como información y sin pregunta | El §8.4 | V2-11 |
| F4B-E25 | Véase I-02 | V2-02 |
| Las paradas por declaración nueva, por nombre y acotadas a los archivos tocados (PBT1C-C13) | **PBT2A-C13** solo admite, en los dos archivos de Application, los 16 nombres de la API, más los constructores. **PBT2B-C13** solo admite, en W y en el archivo del modo, 26 nombres: los 23 nuevos de INV-PBT2B-03 (cinco campos, `ModeForTest`, `ForDraft`, cinco métodos y las once constantes) más el `enum` y sus dos miembros. El constructor privado es el único constructor nuevo. **PBT2C-C13** no admite ninguna declaración: solo las tres sustituciones de la tabla cerrada. Las tres, en inglés, como en PB-T1c, y con «The Worker stops before adding it» | V2-26 |
| Separar en los invariantes lo normativo de las notas | Los invariantes de cada contrato son normativos. Lo explicativo va en líneas «**Nota (no va al contrato).**»: en INV-PBT2A, en INV-PBT2B-03 (los rangos y M10) y en INV-PBT2C | V2-27 |
| El `Trailer` real de los commits en `ExpectedEvidence` | La `ExpectedEvidence` común pide el `Trailer` de `worker-handoff.json` igual al trailer real de los commits RED y GREEN. Es la mitad (b) de PBT1B-IR-M14, y cierra también la mitad de M-11 | V2-28 |
| Los testigos en ASCII | D-PBT2-09 tiene una columna «Testigo ASCII del contrato» para los once literales; los testigos de los textos de hoy, con tilde, se cortan antes de ella («sin confirmar que se perder», «…un modelo inv»). Los `Objective` de los tres contratos son ASCII. INV-PBT2B-04 busca solo testigos ASCII. Las pruebas comparan el literal entero en C# | V2-29 |
| Citar D-PB-07 como F4B-E23, custodiada en `coordinator/owner-f4b-dpb07-extensiondata-decisions.md` | Véase I-02 (c). En el texto ya no queda «[OPB7]» | V2-02 |
| Escribir solo en `hs65\pbt2\`, en solo lectura y por objetos git | Hecho. Incidencia declarada: `python -I -c "import sys"` al final de una tubería de `git show`, sin efecto (§9 y registro) | V2-32 |

## 5. Efectos en cadena de la v2

- **Los ids de los CT** cambian por la división; la tabla está al principio del paquete. Las referencias de IR1 a «CT-11», «CT-12» y «CT-13» son hoy CT-PBT2B-08, 09 y 10, y las de «CT-08», «CT-09» y «CT-10», CT-PBT2C-01, 02 y 03.
- **Las líneas base previstas de la CI** (§6):

  | Punto | Core | UI |
  |---|---|---|
  | El cierre de PB-T1c | 19 088 | 2 439 |
  | El GREEN de PB-T2a | 19 092 | 2 439 |
  | El GREEN de PB-T2b | 19 094 | 2 447 (2 446 si Q-PBT2-3 no es A) |
  | El GREEN de PB-T2c | 19 094 | 2 450 |

- **El RED de las tres subtareas tiene Core en `failure`**, y por eso `build-plugin` sale omitido (registro O):
  - PB-T2a, por CT-PBT2A-01 a 03;
  - PB-T2b, por CT-PBT2B-08;
  - PB-T2c, por CT-PBT2B-08 bajo MU-T2C-03, con la tabla de atribución.
- **Las condiciones para emitir** ganan la comprobación de la reasignación de `editable` en los anclajes de PB-T2a (V2-05).
- **Ninguna decisión material nueva.** Las cuatro del §8.2 siguen siendo las únicas.
