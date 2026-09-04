# Project Handoff

> Estado vivo de RackCad para continuidad entre sesiones. Actualizado: **2026-09-03**.
> La arquitectura se consulta en [ARCHITECTURE.md](ARCHITECTURE.md), el proceso en
> [WORKFLOW.md](WORKFLOW.md), el plan en [ROADMAP.md](ROADMAP.md), los procedimientos en
> [guias/](guias/) y la historia anterior en
> [archivo/transicion-2026-07/handoff-historial-2026-07.md](archivo/transicion-2026-07/handoff-historial-2026-07.md).

## 1. Resumen y estado actual

RackCad es un plugin de AutoCAD 2025 (.NET 8, C#/WPF) para diseñar y dibujar racks industriales
con BOM. El trunk único es `main`; Domain y Application son puros, UI usa WPF sin AutoCAD y Plugin
es el único adaptador de la API de AutoCAD.

El producto mantiene cuatro familias operativas en `main`: cabecera, selectivo, dinámico modular y cama
de rodamiento. Comparten identidad por GUID embebida en DWG, edición round-trip y vistas ligadas. El
dinámico modular de I-02 y la instalación segura de I-04 están integrados.

**I-43 — Selectivo: edición por alcance y fondos — queda INTEGRADA y CERRADA** el **2026-09-04**
(`feature/selectivo-scopes-fondos`). El editor Selectivo gana **dos ejes independientes**: *dónde* se escribe
—`TargetFondos`: el fondo actual, uno, varios o todos— y *qué alcance* tiene dentro de un fondo —`Scope`: celda,
seleccionadas, nivel, frente, todas—. Toda escritura explícita es el producto **`Scope` × `TargetFondos`**, y una
celda que un fondo destino no tiene se **omite**, nunca se recorta ni se crea. La selección son **posiciones 2D**
`(frente, nivel)` que se proyectan sobre cada destino. Cada propiedad recibió una autoridad **por fondo**: la
profundidad de tarima y la de cabecera viven en el slot de su fondo, la elevación del larguero a piso es directa por
`(FondoIndex, FrontIndex)` —el valor global histórico queda solo como compatibilidad de lectura— y la cabecera
personalizada es autoridad de `(FondoIndex, PostIndex)`, de modo que **el frontal y la preview de cada fondo dibujan
la suya** (se retira la asimetría «custom solo en el fondo 0»). Las cuatro cajas de texto pasan a ser **editores de un
valor pendiente**: los slots y las matrices son la única autoridad comprometida, `BuildDesign` no lee texto, y el
commit es **atómico, en dos fases y ordenado** (`FondosBox` → `BayCountBox` → `FondoBox` → `CabeceraFondoBox`), con
`TargetFondos` re-resuelto tras cada cambio estructural y **un solo recompute por operación**. El contrato quedó
escrito ANTES de tocar código en
[ADR-0032](adr/0032-selectivo-pendiente-comprometido-y-autoridades-por-fondo.md), **aceptado por el dueño**.
**Compatibilidad legacy intacta**: sin cambio de DTO, de `SchemaVersion` (sigue `1.0`) ni de stores, y los documentos
anteriores dibujan igual. **Validación manual del Owner: PASS TOTAL** en AutoCAD 2025 sobre el DLL construido desde
`d582deed5bbd93083261399e45b2ecc3e16088d7`.

**I-44 — Hotfix Push Back: peraltes incorrectos de largueros intermedios en BOM — queda INTEGRADA y
CERRADA** el **2026-09-03** (`fix/push-back-peraltes-intermedios-bom`). Un larguero intermedio pertenece a
una **cama**, no a la estructura (ADR-0031 §8-bis), pero `PushBackIntermediateBeamLateralBuilder.BuildFor(…)`
—que es quien materializa esa cama, y a quien el BOM le pregunta— resolvía `ProfileId` y `Peralte` con la
**envolvente de proyección** del rack o del poste. Resultado: con varios frentes en un mismo nivel, el BOM
cobraba a **todas** las camas el mayor peralte del rack (F1 = 3.5", F2 = 4.5", F3 = 6" → las tres a 6").
Ahora la autoridad la fija el **llamador**: `Build(…)` es **proyección** —un corte lateral muestra el
larguero que se ve, el que tapa a los de detrás— y `BuildFor(…)` es **cama física**, donde el par
`ProfileId + Peralte` sale **junto** de `DynamicRackLevelGeometry.At(structure, front, level)`. `postIndex`
sigue colocando pero ya **no decide** propiedades físicas. **No hubo bug legacy** y **no se creó ADR**: el
contrato ya estaba enunciado en ADR-0031 §8-bis y sólo faltaba aplicarlo. **Validación manual del Owner
APROBADA** en AutoCAD 2025 sobre el DLL construido desde `4947a1b`, en el DWG real que presentaba el
defecto. Queda **abierta y separada** una ambigüedad de producto sobre la cama **CORRIDA** (§3 y
[ideas-futuras.md](ideas-futuras.md)). Detalle en §4.

**I-42 — Push Back compuesto, bidireccional y camas compartidas — queda INTEGRADA y CERRADA** el
**2026-09-02** (`feature/push-back-compuesto`). Push Back deja de estar limitado a un solo sentido: dentro
de **un mismo sistema físico** conviven **lado A y lado B enfrentados**, con **UNA estructura física** y
**DOS configuraciones funcionales de almacenamiento**. La topología es **por celda** —`Solo A`, `Solo B`,
`Encontradas` (dos camas) y `Corrida` (una cama que atraviesa el rack, en sus dos sentidos)—, el **hueco**
central es longitud física real con separador opcional, y los **dos pasillos** de carga reciben su
seguridad. Un rack de un solo sentido **no cambia**: se persiste y se resuelve exactamente como antes de
I-42. **Validación manual del Owner APROBADA** en AutoCAD 2025 sobre el DLL construido desde `077d35a`:
**8/8 escenarios**, con el 2 (corrida sobre el hueco) **aprobado con observación** no bloqueante.
**ADR-0031 queda `aceptado`** con el modelo implementado y sus **seis limitaciones declaradas**. Detalle
en §4.

**I-41 — Configuración por celda de Push Back — queda INTEGRADA y CERRADA** el **2026-08-23**
(`feature/push-back-cell-configuration`). El **fondo** deja de ser una propiedad del FRENTE y se resuelve
por **celda** (`FrontIndex + LevelIndex`) con una regla de precedencia única, de modo que el
`PalletsDeep` del frente pasa a ser la **envolvente estructural derivada** (**PB-015**); y cada celda
decide si dibuja su **tarima** (**PB-016**), que es referencia visual y **nunca entra al BOM**.
**Validación manual del Owner APROBADA** en AutoCAD 2025 sobre el DLL construido desde `c41aee1`.
**ADR-0030 queda `aceptado`** con el modelo implementado, incluida su limitación declarada. Detalle en §4.

**I-40 — Edición de cabeceras de Push Back — queda INTEGRADA y CERRADA** el **2026-08-23**
(`feature/cabeceras-push-back`). Push Back gana la cabecera personalizada como **autoridad efectiva**, su
**reutilización** como copia independiente y la **selección masiva** por producto cartesiano
`Cabeceras destino × Líneas destino`. Funda la **línea física** como segunda dimensión del modelo y
unifica las vistas sobre `HeaderConfigurationAtPost`. Detalle en §4.

**I-39 — Contrato funcional común de ventanas WPF — queda CERRADA** el **2026-08-07** con la
integración de **I-39D** (`architecture/dialogos-y-utilitarias`), cuarta y última subiniciativa, con la
validación manual del Owner **APROBADA** en sus **24 puntos** (`OWNER_APPROVED_I39D_MANUAL_VALIDATION`).
**ADR-0029 permanece `aceptado` e inmutable.**

I-39D cierra los arquetipos **C** y **D**. Empieza donde tenía que empezar: **nueve de sus dieciséis
ventanas no se habían construido jamás en una prueba**, y toda la cobertura que existía era funcional.
Construirlas corrigió además **cinco afirmaciones** de su propia auditoría de apertura, que se había
hecho leyendo —son dos y no tres las que comparten tamaño, dos y no tres las de `SizeToContent`, seis y
no siete las que pintan con `Firebrick`, completar el chrome de Defensa cambia solo el fondo, y
`Colocar` **sí** produce resultado con los valores iniciales—.

**`RackDialogWindow` tiene por fin papel final, y es retirarse.** Nació en I-14 con patrón strangler y
cinco iniciativas después seguía con **cero subclases productivas**. El motivo está medido: encaja en
cuatro de los diez diálogos y no en seis —su `CreateActionBar` ni siquiera admite la invocación natural
de las cuatro rejillas—, no entrega tamaño, no coloca la barra que fabrica y no tiene `OnClosing`. Y
peor: asignaba fondo y tipografía como **valor local**, que en precedencia WPF gana al setter de un
`Style`, de modo que **la base bloqueaba el contrato que venía a habilitar**. Sus dos mitades se separan:
el chrome se generaliza a `DialogWindowChrome` + `DialogWindowStyle`, tercer estilo hermano de los de A y
B, adoptado **por composición** por los diez diálogos en una línea; y la barra de acciones se retira por
ser un **modelo paralelo** de `EditorActions.Button`, que la decisión 28 del Owner prohíbe. El censo baja
de **29 a 28** clases y su guarda se **reapunta, no se debilita**.

El **contrato de tamaño** se declara solo donde la evidencia ya converge —dos ventanas completas y tres
mínimos— y el arquetipo C se queda deliberadamente **sin** él: dos de sus diez se dimensionan por su
contenido y cuatro calculan el suyo de la matriz, así que unos mínimos comunes reproducirían la anomalía
de letra muerta que I-39C acababa de cerrar en B. `EditorActions.Button` gana sus **primeros consumidores
fuera del piloto** en las dos ventanas de almacén, conservando etiqueta —siguen diciendo «Colocar» y
«Calcular»—, métrica, teclado y resultado. Y quedan corregidos **tres defectos observables**: el fondo de
`SafetyDefensaGridWindow`, el motivo de bloqueo de la barra de selección masiva —que se calculaba y era
ilegible justo cuando importaba— y el diagnóstico que no se borraba nunca.

**Lo que NO se unifica, con su razón medida**: los mapeos `SafetySide`, porque dos no ofrecen «Ninguno» a
propósito; las etiquetas Todos/Ninguno frente a Todas/Ninguna, que difieren por concordancia de género; y
`EditorActionBar`, cuyo orden fijo invertiría primaria y secundaria en tres utilitarias.

**Cuatro** caracterizaciones cambian a propósito y se conservan con `Skip`. Otras **dos** no pueden
conservarse así, y se dice por qué: una prueba omitida sigue teniendo que **compilar**, y el tipo que
caracterizaban ya no existe, así que su cuerpo se transcribe palabra por palabra en la evidencia.

**La línea completa no tocó un solo archivo de producto.** El diff de I-39 entera contra su base
(`fdde6a7`) tiene **cero** archivos en `RackCad.Plugin`, `RackCad.Application`, `RackCad.Domain`,
`RackCad.Catalogs`, `assets/`, `deploy/` y `.github/`: geometría, BOM, persistencia, wire format, GUID,
catálogos, materialización en AutoCAD y comandos quedan intactos por construcción, no por comprobación.

**I-39C** (`architecture/adopcion-editores-acotados`) queda **integrada** el **2026-08-07** y es la
tercera subiniciativa de **I-39**, que **sigue abierta**: falta **I-39D**. Cierra el **arquetipo B**
completo, con la validación manual del Owner **APROBADA** en sus **37 puntos**
(`OWNER_APPROVED_I39C_MANUAL_VALIDATION`).

**Retira la fachada** `CantileverComponentEditorShell`. I-39A la había dejado en la ruta vieja para que
cuatro XAML ya validados quedaran con **diff vacío**: un andamio con su fecha de retiro escrita en su
propio comentario. Los cuatro nombran ya el tipo neutral y sus dos guardas se **reapuntan, no se
debilitan** —verificadas en rojo por partida doble: con la fachada restaurada fallan, y con un XAML
volviendo a nombrarla **no compila siquiera**, porque el tipo ya no existe—.

El **shell acotado** respalda sus tokens en `OnApplyTemplate` y **solo cuando no resuelven**, con la
simetría exacta que I-39B pagó en el rico: mergear siempre no es respaldo sino **sombreado**, y el
contenido de las ranuras acabaría resolviendo otra instancia del mismo estilo.

El **contrato de tamaño del arquetipo B** deja de ser letra muerta. `BoundedEditorWindowStyle` es
**hermano** de `EditorShellWindowStyle` y no una variante suya, porque D9 dice que un arquetipo **no**
hereda implícitamente restricciones de tamaño de otro. Las cuatro Cantilever abrían en `1120×700` y
`1120×672` sin declararlo; ahora abren en `1040×680` con mínimo `820×520`, y que ahí no se pierda nada
**no se afirma: se mide**, llevándolas al cliente más apretado.

**`EditorAction` aprende `IsDefault` e `IsCancel`** y gana su **primer consumidor productivo**. La prueba
de que la evolución es correcta es que las **treinta** caracterizaciones de I-39A siguen verdes **sin
tocarse**, incluida la que fijaba esta deuda como de I-39C.

El **Larguero** adopta el shell conservando instancias, manejadores y orden: su code-behind **no aparece
en el diff**. Su preview se queda con superficie **clara a propósito**, porque sus rótulos son grises y el
fondo oscuro del editor rico los volvería ilegibles.

**`Insertar` se apaga con motivo visible** en cinco ventanas y **`Aceptar` no**: una pieza bloqueada sigue
siendo una intención que el usuario puede conservar. La otra mitad de la medición de I-39A —que una
entrada inválida **no** bloquee— se revisó y **no** se cambia, porque D5 la hace correcta. **Foco inicial**
declarado en las seis; **preview y dirty medidos**: ninguna declara ámbito transaccional y por eso ninguna
intercepta el cierre, que es «no aplicable» con razón de **producto**, no por omisión.

**Diez** caracterizaciones cambiaron **a propósito** y **ninguna se reescribió**: se conservan con `Skip`
como evidencia versionada, y **dos de ellas son de I-39A**, cuyo propio texto anticipaba este cambio.

**Desviación explícita y medida**: `EditorActionBar` **no** se adopta en el arquetipo B —sus dos
aportaciones ya las resuelve el `DockPanel` que las cuatro Cantilever tienen, y la prueba de mínimo
demuestra que no recorta—; su papel en **C** y **D** lo decide I-39D. **No** cambia geometría, BOM,
persistencia, wire format, GUID, catálogos ni reglas de producto, y el diff **no toca** Plugin,
Application ni Domain.

**I-39B** (`architecture/interaccion-editores-ricos`) queda **integrada** el **2026-08-07** y es la
segunda subiniciativa de **I-39**, que **sigue abierta**. Lleva el contrato funcional a los **seis
editores ricos**, con la validación manual del Owner **APROBADA** en sus **31 puntos**
(`OWNER_APPROVED_I39B_MANUAL_VALIDATION`).

Lo central es la **política común de cierre** de ADR-0029 D7: las cuatro rutas —botón, Escape, la X y
`Alt+F4`— convergen en `OnClosing`, así que la política vive ahí y no en el handler del botón. La
consultan **exactamente las dos** ventanas que **declaran** un ámbito transaccional: la **Cabecera**,
que reutiliza `HasUnsavedManualEdits` y `ConfirmDiscard` —la protección ya existía y el cierre la
evitaba, de modo que Escape descartaba en silencio lo que la misma ventana protegía al restaurar— y
**Push Back**, sobre `ModuleSession`. Las otras cuatro no declaran ámbito y cierran directo: D8 admite
«no aplicable» como valor legítimo, y **no** se inventa un dirty global que el producto no tiene.
Push Back gana `IsCancel` **después** de la política, nunca antes: al revés habría convertido Escape en
un descarte instantáneo justo en la única ventana con ámbito declarado.

La hace verificable una **costura de confirmación** mínima (`EditorDiscardPrompt`), que en producción
muestra el **mismo** `MessageBox` de siempre y en pruebas se sustituye: `MessageBox.Show` no tenía
ninguna, y una política de cierre inverificable es exactamente la que pierde trabajo en silencio.

Entrega también la **caracterización de las seis** —Enter, Escape, cierre, dirty, acciones, preview,
tamaño, ownership y foco—, donde no existía ninguna cobertura; el **respaldo de tokens** de
`RackEditorVisualShell`, resuelto en `OnApplyTemplate` y **solo cuando el token no resuelve**, porque
mergear siempre sombreaba el diccionario del consumidor y rompía una prueba de identidad de I-37D;
`Editor/` **sin nombres de sistema**, con `EditorModules.cs` como excepción declarada; el **preview
obsoleto declarado** del Dinámico, que **conserva** la imagen anterior pero deja de ser mudo y apaga sus
cinco acciones de dibujo con motivo; **Insertar bloqueado con motivo** en la Cama; las **severidades**
de Cantilever, cuyos avisos no bloqueantes se pintaban con el rojo de error; y el **foco inicial** de la
Cabecera.

**Cuatro** de las doce caracterizaciones cambiaron **a propósito** y **ninguna se reescribió**: se
conservan intactas con `Skip` como evidencia versionada, y el contrato nuevo vive en clases separadas,
de modo que la transición base → ADR → contrato se lee entera.

**Desviaciones explícitas y medidas**: la **Cama** y la **Cabecera** **no** adoptan el shell —el mínimo
del arquetipo A es **mayor** que el tamaño inicial completo de la Cama, y la Cabecera perdería su layout
de paneles persistido, que es una capacidad de producto—; y **`EditorAction` con `EditorActionBar` no se
adoptan**, porque no saben declarar acción por defecto ni cancelación y sustituir los botones rompería
el contrato de teclado que I-39B fija. `EditorStatusPalette` **sí** queda adoptada y ya tiene consumidor
productivo. **No** cambia geometría, BOM, persistencia, wire format, GUID, catálogos ni reglas de
producto. Quedan **I-39C** e **I-39D**.

**I-39A** (`architecture/contrato-funcional-ventanas-wpf`) queda **integrada** el **2026-08-07** y abre la
línea **I-39**, el **contrato funcional común de ventanas WPF**. ADR-0019 había unificado la **composición
visual** de los editores ricos; lo **funcional** seguía sin contrato. **ADR-0029**, que **complementa** a
ADR-0019 sin reabrir ninguna de sus seis reglas, queda **aceptado**: fija el inventario **por tipo** y nunca
por `x:Name`, los cuatro arquetipos **A/B/C/D** con asignación obligatoria, estados **ortogonales** en vez de
un enum lineal, el preview con **dos ejes** —autoridad y frescura—, cinco grados en un valor capturado donde
una entrada inválida **no** sobrescribe en silencio un valor aplicado válido, acciones que declaran semántica
y **motivo visible al bloquearse**, un **único camino de cierre** para botón, Escape, `Alt+F4` y botón de
sistema, dirty como propiedad de un **ámbito** y no de la ventana, y el contrato de tamaño **por arquetipo**,
de modo que el **B no hereda los mínimos del editor rico A**.

El censo, obtenido por reflexión, encuentra **29 clases `Window` concretas**: 28 productivas y
`RackDialogWindow` como infraestructura. Los dos métodos alternativos se midieron y mienten: por nombre de
archivo se pierde `SafetyPerPostWindow`, declarada **dentro** de `SelectiveSafetyWindow.cs`; por `x:Name` se
contarían once consumidores del **tipo** `PreviewCanvas` donde hay **uno**. Una guarda mantiene vivo el censo
aseverando contención en los dos sentidos.

El shell del arquetipo B nace como **`RackBoundedEditorShell`** en `RackCad.UI/Shell`. Ya era neutral **como
tipo** —siete ranuras `object`, cero ramas—: lo que lo ataba a un sistema era su **ubicación**, bajo
`Systems/Cantilever/Components`, con `Generic.xaml` declarando un `xmlns` hacia ese namespace.
`CantileverComponentEditorShell` queda como **fachada que no declara nada**: al no sobrescribir
`DefaultStyleKeyProperty` hereda la clave de estilo del tipo base y con ella la misma plantilla, así que los
**cuatro XAML de componente Cantilever quedan con diff vacío** y `Generic.xaml` deja de nombrar ningún
sistema. La guarda de I-37D de las siete ranuras se **reapunta, no se debilita**, y gana una hermana que
vigila que la fachada no re-declare ninguna ranura.

El **piloto** es `StructuralSectionInspectorWindow` —code-only, y el **único consumidor del tipo**
`PreviewCanvas`—, **caracterizada antes de migrar** con 30 pruebas que cubren por primera vez en el
repositorio Enter, Escape, foco inicial, tabulación y caminos de cierre, y que pasaron **sin editarse** tras
la migración. Se caracterizó el comportamiento **actual**, no el deseable: `Insertar` nunca se deshabilita y
sin selección es un no-op silencioso, y una longitud o rotación inválidas **no** bloquean la inserción.
Convertir eso en el contrato de ADR-0029 habría sido un cambio funcional disfrazado de caracterización, así
que queda como deuda de la subiniciativa del arquetipo.

La **ronda 1** de validación manual encontró **un solo defecto**: los botones de acción quedaban pegados a
los bordes derecho e inferior. La causa no era del piloto sino del contrato visual común: un
`DynamicResource` con clave de cadena **no cae al diccionario de tema del ensamblado**, así que un consumidor
construido en código —que no mergea `AppStyles`— dejaba `ShellZoneSpacing` sin resolver y el margen caía a
**cero**. La corrección vive en el shell, que ahora mergea el diccionario compartido en sus **propios**
recursos. **No** cambia geometría, BOM, persistencia, identidad, wire format, catálogos ni Plugin.

**I-39 NO queda cerrada.** Los dos hallazgos que I-39A midió y dejó deliberadamente sin corregir —la misma
dependencia latente de recursos en `RackEditorVisualShell` y el defecto de Escape/`IsCancel` de Push Back—
los resolvió **I-39B**, ya integrada. Siguen pendientes **I-39C** (editores acotados restantes, retirada de
la fachada Cantilever y Larguero) e **I-39D** (diálogos y ventanas utilitarias).

**Push Back** es la quinta familia operativa y está **integrada en `main`** desde el **2026-07-25**
(merge `--no-ff` `77031be`, CI verde en los cuatro jobs, run 30139506411). I-18 entregó el primer sistema
construido sobre el patrón de módulos, con el gate manual del Owner **aprobado** en AutoCAD 2025
(PB-VAL-01…06). El **preview visual** se **difiere** a una iniciativa transversal futura y **no** fue
aprobado visualmente.

**I-32 corrige ese Push Back a partir del reporte del Owner y está INTEGRADA en `main` desde el
2026-07-27** (merge `--no-ff` `236619d`, CI verde en los cuatro jobs, run 30228331452).

**I-33** (`feature/frente-en-blanco`) queda **integrada** el **2026-07-27**. Implementa **PB-014**, el
**frente en blanco** para el **Dinámico** y **Push Back**; la decisión de alcance que `ideas-futuras.md`
exigía la dio el Owner al abrir la iniciativa (aplica a esos dos sistemas, **no** al Selectivo). Un frente
pasa a tener estado **Activo / En blanco**: en blanco **conserva su claro y su estructura**, **sigue
desplazando** a los frentes posteriores y **no lleva ningún nivel ni componente de carga** —ni larguero
IN/OUT, ni intermedio, ni cama, ni larguero posterior o tope de Push Back, ni seguridad indexada por
nivel— en ninguna de las cuatro vistas ni en los dos BOM, mientras su configuración queda **dormida** para
reactivarlo intacto, **sin celda falsa**. La regla vive en **una** autoridad pura de Application,
`DynamicFrontActivation`, sobre la **estructura dinámica que Push Back compone**, así que los dos sistemas
no pueden divergir; para un frente activo devuelve el histórico `Math.Max(1, LoadLevels)`, de modo que un
rack sin frentes en blanco **no cambia en nada** y **serializa igual que antes** (la bandera se omite del
wire, no se escribe `null`). Los documentos legacy cargan **todos los frentes activos**; un payload
explícitamente todo-en-blanco se **rechaza con error visible** (resolver y `RackDesignValidation`, mensaje
único) y **nada se normaliza en silencio** —el editor lo previene de forma **no destructiva**, negándose a
blanquear el último frente activo—. Al **seleccionar** un frente en blanco la selección sigue siendo válida
pero se **deshabilitan** los controles de nivel/celda y los alcances ligados a celda, con el motivo
visible; en los **diálogos de seguridad** sus celdas de nivel son **inexistentes** (no seleccionables ni
aplicables) y su configuración guardada se preserva **dormida** (`SafetyDormantCells`), volviendo intacta
al reactivar. La forma de la rejilla del desviador y la visibilidad del **selector de lado** quedan
**desacopladas** (el Dinámico conserva el selector y recibe además su lista por poste; Push Back lo apaga
explícitamente). Por **decisión del Owner**, la **frontera compartida por dos frentes en blanco NO
existe**: los dos bordes exteriores existen siempre y una interior existe salvo que sus **dos** frentes
adyacentes estén en blanco, así que una corrida de N blancos conserva solo sus dos fronteras exteriores y
pierde sus **N−1** interiores; desaparece el **ensamble físico** (poste, placa, cabecera/separador, postes
derivados y refuerzos, el corte lateral entero, su parte del BOM y su seguridad por poste) y **nunca el
frente lógico** —índices, claros, ancho, **largo total y todas las coordenadas X** se conservan—. **No**
toca Selectivo, catálogos, bloques DWG, el shell visual, I-23, I-25 ni la decisión pendiente sobre
`DesviadorCellsAreByPost`. El Owner **aprobó la validación manual en AutoCAD 2025**, incluida la **ronda
focalizada de fronteras físicas**, sobre el candidato `b840cfe`. `origin/main` **no avanzó** desde la base
`0e505d8`: **sin rebase**. La rama se integra por `git merge --no-ff` en esta sesión.

**I-35** (`feature/editor-avanzado-push-back`) queda **integrada** el **2026-07-27**. Implementa
**PB-011**, la prioridad alta del Owner que I-32 dejó diferida: el Dinámico permitía seleccionar un módulo
—cabecera o separador— y personalizarlo; **Push Back no**. Entrega la **edición longitudinal de Cabeceras y
Separadores** por módulo de **RACK** con **selección única** —`DynamicRackSystem.Modules` es **una sola
secuencia longitudinal** compartida por todos los frentes y todos los postes, así que personalizar un módulo
personaliza el rack entero y **no existe** módulo por frente ni por poste—; la **configuración transaccional**
de cabecera (escenificar, **confirmar** o **cancelar** sobre una **copia** canónica, sin modificar
`RackFrameConfiguratorWindow`, que es compartida); la **altura manual de cabecera**; el **refuerzo total o
parcial del poste derivado**; la **cantidad y separación globales de separadores**; y la **restauración
individual** por módulo y **global** del rack. Los cuatro parámetros avanzados son **globales del rack**,
viven en su **propia sección** —separada de «Módulo seleccionado»— y reutilizan **exclusivamente** las
autoridades que la estructura dinámica compuesta ya poseía (`ManualHeaderHeightOverride`,
`DerivedPostReinforced`, `DerivedPostReinforcementHeight`, `SeparatorCountOverride`,
`SeparatorSpacingOverride`): **no se creó ninguna autoridad nueva ni campo equivalente**, solo el transporte
`PushBackAdvancedRackParameters`, que **valida y asigna**. La reconciliación empareja por **`ModuleId + Kind`
exacto** —retirando el emparejamiento por ordinal, que además aterrizaba la edición de un módulo en **otro**
al desplazarse la secuencia—, **adapta** `Depth` y el peralte de rack de una cabecera conservada, y **reporta
por nombre** preservados, adaptados, eliminados, incompatibles y restaurados: **no existe descarte ordinario**
y **nada se pierde en silencio**. El refuerzo del poste derivado no es un booleano: desactivado deja el poste
sin su refuerzo, vacío refuerza toda la altura, un valor refuerza parcialmente desde la base, y una
recomputación que vuelve inválida una altura antes válida **bloquea con error visible** en vez de recortar.
**Preserva I-33** (frentes en blanco y fronteras suprimidas: un módulo que solo aparecía en postes suprimidos
se deshabilita **con su motivo**, y ninguna edición reactiva un frente ni recrea una frontera) y **PB-013**.
**No** toca el Selectivo, los catálogos, los bloques DWG, `SelectionMatrix*`, los `Safety*GridWindow`, topes,
desviadores, guías, defensas ni el comportamiento del Dinámico. La **primera ronda** del Owner quedó
**parcialmente rechazada** por cuatro residuos —altura manual, refuerzo del poste derivado, cantidad y
separación de separadores—, corregidos en la segunda **sin rediseñar** lo ya aprobado. El Owner **aprobó la
validación manual en AutoCAD 2025** sobre el candidato `f2be30c`. `origin/main` **no avanzó** desde la base
`52ce27f`: **sin rebase**. La rama se integra por `git merge --no-ff` en esta sesión.

**I-34** (`feature/edicion-masiva-seguridad`) queda **integrada** el **2026-07-27**. Implementa
**PB-007**, la **edición masiva de matrices de seguridad**: hasta ahora las rejillas eran celda a celda y
solo ofrecían «Todos»/«Ninguno», así que quitar el desviador del segundo nivel en 100 frentes costaba 100
clics. Las **cuatro** matrices booleanas —**desviador** (eje **poste**, el único del producto), **tope**
(eje **frente**, y un solo diálogo cubre a la vez el tope del Selectivo y el **tope posterior de Push
Back**), **guía** y **parrilla**— ganan el estado **Activar/Desactivar** y los alcances **Celda / Nivel /
Frente-o-Poste / Todo**, la misma gramática de «Aplicar a:» que ya usaban los editores de diseño. La
fundación es **pura** (`SelectionMatrixBulkEditor` sobre `SelectionMatrixModel`) y **agnóstica a
`RackSystemKind`**: ningún diálogo ramifica por sistema, porque cada uno **declara** sus etiquetas y sus
capacidades en vez de derivarlas. La **celda primaria** es transitoria —la última celda válida que el
usuario pulsó— y **no se persiste**: lo que se guarda sigue siendo el mismo conjunto de `OffCells`, aguas
arriba de `SafetyDormantCells`, así que las **celdas ausentes** y la configuración **dormida** de los
frentes en blanco de I-33 no se alteran ni con el alcance «Todo». Cada operación masiva emite **una**
notificación agregada con exactamente las celdas cambiadas, y el control repinta esas casillas **sin
reconstruir** la rejilla: la nota viva del desviador y el total de la parrilla se recalculan **una** vez,
no N. La **parrilla** entró por **addendum normativo del Owner** durante la validación, con la condición
de **conservar su contador vivo por celda**; se resolvió con un **adorno opt-in y neutral** del control
compartido (`CellAdornment` + `RefreshAdornments`), de modo que los otros tres diálogos no cambian ni una
línea. Corrige además un defecto propio detectado en revisión: un valor **no definido** de
`SelectionMatrixScope` caía en el `default:` y se interpretaba como «Todo», reescribiendo la rejilla
entera —la peor falla posible en una matriz de seguridad—; ahora los dos sitios **fallan cerrado**. **No**
toca Domain, Application ni Plugin (vive entera en `RackCad.UI`), ni DTO, wire format, geometría, dibujo,
BOM, GUID, catálogos, bloques DWG, el shell visual, `DesviadorCellsAreByPost`, I-23 ni I-25. La
**defensa** —el único elemento que no es una matriz booleana— **no entró y no bloqueó**: queda como
candidato futuro **independiente** en `ideas-futuras.md`. El Owner **aprobó la validación manual en
AutoCAD 2025** sobre el candidato `dbdda74`. `origin/main` **no avanzó** desde la base `7e48b5c`: **sin
rebase**, así que el árbol validado es el integrado. La rama se integra por `git merge --no-ff` en esta
sesión.

**I-23** (`refactor/namespaces-sistemas`, Fase 5) queda **integrada** el **2026-07-27** y **cierra la
Fase 5**. Salda el hallazgo **E8** de la auditoría 2026-07 —el namespace `Systems` plano y multi-sistema,
con nombres fósiles— con un refactor **mecánico** bajo congelación funcional total: **176 archivos movidos
con `git mv`**, todos registrados como renombre, más la reescritura de `namespace`/`using` en sus
consumidores. **Ninguna línea de lógica cambia.**

Los **cuatro** proyectos de producto quedan repartidos por sistema en `Systems.{Selective, Dynamic,
PushBack, FlowBed, Larguero, Shared}`: las tres raíces planas de `Systems` quedan **vacías**. Se disuelven
**cinco** namespaces: `Domain.Systems`, `Application.Systems` y `Plugin.Systems` (los planos),
`Application.Headers` y `Plugin.Headers`. La cabecera **física** conserva `RackFrames` en Domain,
Application y UI; lo que **materializa** pasa a `Drawing` en Application y Plugin, que quedan simétricos.
El único renombre autorizado es **`DynamicSystemPlan` a `Application.Drawing.HeaderRunPlan`**: no era el
plan del sistema dinámico sino el **plan de corridas de cabecera**, y lo consumen los cuatro sistemas —
exactamente lo que E8 denunciaba. **No** se aplicó el `SystemPlan` que anotaba el ROADMAP, por ambiguo en
este árbol (colisiona con `SystemBomBuilder`, `SystemDescriptor`, `SystemRegistry` y `SystemBlockWriter`,
que sí son por sistema). `HeaderGroup` y `HeaderPlacement` viajan con él y **conservan** su nombre.

La regla aplicada es objetiva: un archivo pertenece al sistema que su tipo de primer nivel **nombra y
modela**; **consumir** un contrato ajeno no lo mueve, porque componer entre sistemas es legal. Por eso
`SelectiveDesviadorPlan` sigue en `Selective` aunque lo consuman el Dinámico y Push Back, y los **diálogos
compartidos de seguridad** (`SelectiveSafetyWindow` y los cinco `Safety*GridWindow`) se quedan en la raíz
de `RackCad.UI`: **un diálogo compartido no se asigna a un sistema por número de consumidores**. La
infraestructura transversal de UI (`Controls`, `Editor`, `Preview`, `Shell`, `Themes`) tampoco se reparte.

Los **dos proyectos de prueba conservan un único namespace de ensamblado** como **excepción explícita y
comprobable**, no como exención: **92 de 220 archivos de prueba (42 %) ejercitan más de un sistema** y 48
tocan tres o más, así que asignarles propietario sería arbitrario justo donde la regla exige que sea
inequívoco; además `FullyQualifiedName~` es la interfaz operativa de verificación del repo. La vigila
`NamespaceFolderGuardTests.TestProjects_KeepExactlyOneAssemblyRootNamespace`.

La regla nace con quien la comprueba: `NamespaceFolderGuardTests` (7 aserciones) y
`UiSystemBoundaryGuardTests` (3, que **construyen de verdad** las seis ventanas WPF migradas y validan
`x:Class` y pack URIs), más `.editorconfig`. **`EnforceCodeStyleInBuild` NO se activa**: el proyecto WPF
compila vía un proyecto temporal (`RackCad.UI_<hash>_wpftmp`) e `IDE0130` deriva de ese nombre el namespace
esperado, produciendo 68 advertencias falsas. Las guardas se verificaron **en rojo** bajo cinco
infracciones inyectadas.

Equivalencia **demostrada, no afirmada**: los **7 goldens byte-idénticos**, la **superficie de API
idéntica** a la base tras normalizar namespace y el renombre (ninguna firma, accesibilidad ni miembro
cambió), el inventario de los **28 comandos y alias byte-idéntico**, bundle 105 comprobaciones y harness
10/10. **No** cambia dibujo, BOM, GUID, persistencia, wire format, catálogos, DWG, comandos, alias ni
textos. `assets/`, `deploy/`, `.github/`, `RackCad.sln` y `Directory.Build.*` quedan con **cero** archivos
cambiados. El Owner **aprobó el smoke mínimo en AutoCAD 2025** sobre el DLL Debug del SHA exacto. La
**congelación funcional termina al integrar**; lo que queda vigente es la regla. **Push Back v1 sigue
estable** e **I-25 continúa en backlog diferido**, ni completada ni descartada.

I-06 (`docs/reestructura`) está cerrada e integrada desde el **2026-07-17**. Entregó
`ARCHITECTURE.md`, nueve Context Packs, guías vigentes, archivo histórico y este HANDOFF reducido.
La iniciativa reorganizó documentación y no cambió comportamiento de producto.

I-26 (`refactor/test-catalog-ids`) está integrada desde el **2026-07-19**. Centraliza las
expectativas canónicas de tests, añade un guardián de IDs y relaciones esenciales y publica
cobertura Cobertura como artifact; no cambia producto ni catálogos distribuidos.

I-13 quedó integrada el **2026-07-20** mediante `architecture/referencias-autocad-ci`. CI compila
ahora `RackCad.Plugin` sin AutoCAD instalado con
referencias condicionales compile-only, versiones y origen fijados y guardas que impiden copiar o
publicar material Autodesk. ADR-0003 registra la única excepción autorizada a la política cero
NuGet. I-29 concluyó con decisión B, aprobada con catorce restricciones para uso interno como
aceptación interna de riesgo; no constituye conclusión jurídica ni autorización expresa de Autodesk.

I-09 (`refactor/plugin-commands`) quedó integrada el **2026-07-20**. Partió la clase única
`RackFrameCommands` (12 archivos parciales) en clases de comando públicas por área (`RackMenuCommands`,
`RackCabeceraCommands`, `RackSelectivoCommands`, `RackDinamicoCommands`, `RackCamaCommands`,
`RackDuplicarCommands`, `RackInventarioCommands`, `RackLayoutCommands`, `RackAyudaCommands`) y promovió
los helpers cruzados a tipos `internal static` (`RackBlockFinder` con el escaneo de envelopes
unificado, `RackCloner`, `LayerHelper`, `InDocumentTransaction`, `RackCommandSupport`,
`RackEnvelopeRestamp`). Es un refactor mecánico **sin cambio de comportamiento**: los 26 comandos y
alias, prompts, mensajes, switches por `Kind`, DrawServices, GUID/restamp, layers y flujos se
conservan; solo cambia la clase contenedora. AutoCAD descubre las clases públicas sin
`[assembly: CommandClass]`.

I-08 (`architecture/system-registry`) quedó integrada el **2026-07-21**. Introdujo un `SystemDescriptor`
y un `SystemRegistry` puro en `RackCad.Application` (fuente única de los cinco `RackSystemKind`, sin
reflexión ni escaneo) y migró `RackProjectStore`, la validación genérica y `RackDesignLibrary` para
despachar por el registro: mueren el `if/else` de `Serialize` y los `switch` de
`BuildProject`/`ValidateProject`, y **el enum paralelo `RackDesignKind` y su `MapKind` quedaron
eliminados por completo** (`RackDesignLibraryEntry.Kind` pasa a `RackSystemKind`; la etiqueta visible
proviene del descriptor). Es un cambio **sin comportamiento observable nuevo**: formato JSON PascalCase,
schema `2.0`, nombres de enum, fallback legacy sin `kind`, `Kind: 999` a cabecera, reconstrucción física,
reglas laxas de cama/larguero y etiquetas de biblioteca se conservan idénticos. La adaptación de UI se
limitó a las comparaciones de `RackMainMenuWindow`. I-10 e I-16 quedaron fuera de alcance.

I-16 (`refactor/draw-services`) quedó integrada el **2026-07-21** (merge `--no-ff`; rebaseada sobre el `main`
con I-08 antes de integrar; validada en AutoCAD, ver §2 y §5). Colapsa la duplicación de los `*DrawService`
del Plugin **sin cambio de comportamiento**: extrae la infraestructura compartida (`RackCatalogLoader`,
`BlockPlacement`), uniforma la regeneración condicional (`SystemBlockWriter.ApplyRegen`) y colapsa la
orquestación de los siete servicios de vista en `ViewBlockDraw`, conservando las siete fachadas públicas,
`LateralHeaderDrawService` como servicio especializado, y los invariantes observables (nombres de bloque y
sufijos, mensajes, `postIndex`, `DynamicRackEnd`, caso all-loose, payload/GUID, BOM, geometría, persistencia
y el único `Regen` final multivista).

I-10 (`architecture/kind-handlers`) quedó integrada el **2026-07-21** (merge `--no-ff`; `origin/main` no
avanzó desde la base `c5a4082`, sin rebase final). Introduce `IRackKindHandler` y un registro explícito
`KindHandlerRegistry` en `RackCad.Plugin` (los cuatro Kinds embebidos —`selective`, `dynamic`, `cabecera`,
`cama`— en orden canónico, sin reflexión; `Larguero` no tiene sobre ni handler) y migra a él los tres
despachos por el string del sobre: RACKEDITAR, RACKBOMTOTAL (`BuildRackBom` + `KindLabel`) y el restamp de
copias independientes (consumido por RACKDUPLICAR y RACKLAYOUT). Es un refactor **sin cambio de
comportamiento observable** salvo una excepción autorizada: un `Kind` sin handler produce un error visible
(el mismo mensaje de "tipo de rack no reconocido", inalcanzable con los cuatro Kinds reales). El
`SystemRegistry` de I-08 permanece en Application (persistencia/validación/biblioteca, por `RackSystemKind`);
`KindHandlerRegistry` es el registro del Plugin para las operaciones AutoCAD y **no** se unifica con él ni
con `RackListBuilder` (Application, RACKLISTA), que queda intacto por la dirección de dependencias. Cierra la
pista B del Plugin (I-09→I-16→I-10).

Una **corrección posterior a I-10** (`fix/kind-handler-missing-errors`) quedó integrada el **2026-07-21**
(merge `--no-ff`; base `c9f2d61`, sin rebase). **No reimplementa I-10** —que permanece históricamente
integrada en `c9f2d61`— sino que completa el tratamiento de handlers ausentes: un `RackEmbedDocument.Kind`
sin handler registrado ahora produce **siempre** el error visible histórico y ninguna operación continúa en
silencio. Hallazgos corregidos: (1) **RACKBOMTOTAL** ya no muestra un BOM parcial — un preflight de todos los
racks colocados **aborta** el comando ante cualquier Kind sin handler (el skip best-effort queda solo para el
payload ilegible de un handler conocido); (2) **RACKLAYOUT** valida el handler **antes** de abrir la ventana,
para copias **enlazadas e independientes** (antes solo independientes); (3) el **restamp** lanza ante handler
ausente en vez de devolver el diseño intacto (evita identidades inconsistentes); (4) **inmutabilidad**
completa de `KindHandlerRegistry.Handlers` (extraída a la `KindDispatch<T>` pura de Application, expuesta como
`ReadOnlyCollection`); (5) **cobertura de rutas negativas** verificable sin AutoCAD (`KindDispatch.TryResolveAll`
+ tests puros y source-guards, ADR-0003). El dueño **aprobó la validación manual en AutoCAD** sobre el DLL
Debug de la punta técnica. Sin cambios de geometría, BOM funcional, GUID, persistencia, comandos ni aliases.

**I-11** (`architecture/persistencia-uniforme`) uniforma la **persistencia** de Application: versiona
`FlowBedDocument` y `LargueroDocument` (planos, `SchemaVersion`, `FromDomain`/`ToDomain`, fallback legacy) y
**preserva los campos JSON desconocidos y una versión de esquema no degradada** al cargar, editar, duplicar,
guardar y re-serializar, en los **cuatro límites**: `RackEmbedDocument` (el sobre), `RackProjectDocument` (el
wrapper —incluido el diseño **interior** de los embeds dinámico y de cabecera, y los wrappers de biblioteca—),
`FlowBedDocument` y `LargueroDocument`. Una `SchemaVersionPolicy` central decide legibilidad por MAJOR y una
versión de escritura que **nunca degrada** un minor superior del mismo major; `RackEmbedComposer` (fábrica pura)
hereda `ExtensionData` y versión del `source`; un preflight discriminado
(`ResolveInnerSource`/`PreflightInnerSources`) hace que un MAJOR interior incompatible o un `Kind` incorrecto
**aborten la edición completa** sin actualización parcial. La preservación cruza biblioteca↔DWG por sidecars de
salida que `RackMenuCommands` transporta (transporte mínimo, **sin** cambiar los handlers de I-10). **No** cambia
geometría, BOM, GUID ni el **formato físico del Xrecord** (clave/chunk/`DxfCode` intactos): el sobre se preserva
desde el tipo `RackEmbedDocument`, así I-11 **no** toca `RackEnvelopeRestamp` ni el despacho por `Kind` de I-10.
El **quinto** DTO potencial —`RackFrameProjectDocument` (biblioteca de cabecera desnuda por
`RackFrameProjectStore`)— queda **excluido por decisión aprobada del dueño** (deuda registrada, no cancelada). El
dueño **aprobó la matriz manual en AutoCAD 2025** (incluidos los escenarios B5/B6/S7) y la **owner-validation**;
la rama se integra por `git merge --no-ff` en esta sesión.

**I-14** (`architecture/ui-controls`) abre la **pista C de UI** con cinco controles WPF reutilizables en
`src/RackCad.UI/Controls/`, cada uno con su lógica pura separada de la vista: `SelectionMatrix`
(+`SelectionMatrixModel`, rejilla de casillas con celdas apagadas y actualización por celda **sin rebuild** por
clic), `NumericField` (+`NumericFieldValidation`, entrada localizada sobre `LocalizedNumberParser` con rango y
opcional→auto, que **preserva la procedencia del `BorderBrush` del consumidor** —valor local o binding— en la
transición válido→error→válido), `CatalogCombo` (+`CatalogComboSelection`, sobre `CatalogOption`/
`UiSupport.ToOptions`, con sentinela "(auto)"), `PreviewCanvas` (+`PreviewProjection` + `PreviewPalette`:
proyección mundo→lienzo y paleta congelada **compartidas**, cerrando el hueco que dejaba `PreviewCanvasPainter`) y
la base `RackDialogWindow` (chrome compartido, barra Aceptar/Cancelar, estado). Crea además `tests/RackCad.UI.Tests`
(`net8.0-windows`, runner STA propio **sin dependencias nuevas**) con **85 pruebas** y un **job de CI dedicado**
(`ui-tests`). Es un cambio **confinado a la capa UI**: **no** migra ninguna ventana existente (patrón strangler),
**no** referencia AutoCAD ni el Plugin, y **no** cambia geometría, BOM, persistencia ni el dibujo. Por eso **no**
requiere validación en AutoCAD (`requires_autocad: false`) ni owner-validation; la adopción de los controles la
harán I-15/I-20/I-21/I-22. La rama se integra por `git merge --no-ff` en esta sesión.

**I-12** (`refactor/versionado`) entrega **versionado real** y empaquetado reproducible, **sin cambio de
comportamiento de producto**. Centraliza en `Directory.Build.props` una **versión única** (`RackCadVersion`),
`LangVersion`, `Nullable` y determinismo, más las series de AutoCAD (`RackCadAutoCADSeriesMin`/`Max`); estampa el
**SHA de git** reproducible en `InformationalVersion` (con fallback definido cuando no hay git). El manifiesto del
Autoloader `PackageContents.xml` se **genera** desde una plantilla con la versión y las series (nada duplicado a
mano) y el bundle se arma por **`dotnet publish`** (target `AfterTargets="Publish"`), con `deploy/build-bundle.ps1`
(publish + verificación) y `deploy/verify-bundle.ps1` **fail-closed**: allowlist recursiva, comparación por SHA-256
de los cuatro DLL contra el publish y de los catálogos contra `assets/catalogs`, versión/series del manifiesto y
**cero DLL Autodesk** (ADR-0003 intacto), con su harness `deploy/test-verify-bundle.ps1`. `install-bundle.ps1 -Build`
usa el flujo canónico verificado y **rechaza** `-Build`+`-SourceBundlePath`; la guarda de CI publica y ejecuta
`verify-bundle.ps1` + el harness (**sin tocar** `ci.yml` ni `RackCad.sln`). Documenta **ADR-0004** (una sola serie de
AutoCAD a la vez, hoy `SeriesMin = SeriesMax = R25.0` —solo AutoCAD 2025—, recompilación anual), **aceptado por el
dueño**. Como I-14 ya estaba integrada, el rebase eliminó de `tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj` el
`LangVersion`/`Nullable` duplicados (ahora heredados). El dueño **aprobó la validación manual de autocarga en
AutoCAD 2025** (bundle autoloaded sin `NETLOAD`, `RACKCAD` PASS; ver §5 y `docs/initiatives/I-12-autocad-validation.md`).
La rama se integra por `git merge --no-ff` en esta sesión.

**I-19** (`feature/validador-catalogos`) entrega un **validador de catálogos puro** en
`RackCad.Application.Catalogs.Validation`, **sin cambio de comportamiento de producto**. Reúne en **un
diagnóstico único** con severidades cinco categorías —ids duplicados, referencias/relaciones inválidas,
bloques/vistas faltantes, filas descartadas por rol (antes silenciosas) y el **manifiesto esperado** de
`blocks-library.dwg` (lista de bloques + parámetros dinámicos reales + huella SHA-256, con comparación de
versión/huella)— más un **modo estricto** para despliegues. Los nombres de parámetro viven en una **fuente única
de dominio** (`SeccionRoles`, `CatalogBlockParameters` sobre `SelectiveRackDefaults`/`SelectiveSafetyDefaults`) que
consumen el proveedor, los productores y el validador; una **guardia por igualdad exacta** cruza, por
`PieceId+View+BlockName`, lo que escriben los builders reales de las **13 familias** contra `ExpectedParameters` y
el manifiesto (ni de menos ni de más), con matriz de cobertura bidireccional. Sobre el catálogo distribuido
reporta el **baseline aprobado por el dueño**: 1 error `DUPLICATE_ID` (`TROQUEL_TOPE`, hallazgo pre-existente que
**no** corrige) y 2 advertencias `UNRESOLVED_BLOCK_PIECE` (`TARIMA_GENERICA`); huella esperada `1a31c1a9…`.
**No** toca catálogos, DWG, geometría, BOM, persistencia ni reglas de producto (el código nunca abre el DWG).
AutoCAD **no requerido** (`requires_autocad: false`); **owner-validation aprobada**. Rebasada sobre `main` vigente
(`e2057d7`, tras I-14 e I-12), reconciliando sólo la entrada de `docs/initiatives/README.md`. La rama se integra
por `git merge --no-ff` en esta sesión.

**I-15** (`architecture/editor-shell`) cierra el **Editor Shell** de la pista C de UI y **adopta** su
infraestructura en las ventanas reales, **sin cambio de comportamiento observable**. Crea en
`src/RackCad.UI/Editor/` una `RackEditorSession<TDesign,TSystem>` (catálogo, identidad GUID+nombre
`RackEditorIdentity`, recomputación coalescida `RecomputeGate`/`RecomputeDebouncer` y el contrato de
inserción/actualización), la jerarquía `RackInsertionRequest` por `Kind`, y un registro **explícito sin
reflexión** `IRackEditorModule`+`EditorModuleRegistry` (mismo patrón que el `SystemRegistry` de I-08). El
**menú principal** y la **biblioteca** consumen ese registro en lugar de las ~19 propiedades de payload y
los cinco handlers por sistema (mata el crecimiento O(N) de `RackMainMenuWindow`, hallazgos E3/E5/U1); el
único consumidor del payload en el Plugin (`RackMenuCommands.RackCad`) lee un `RackInsertionRequest` y
despacha por `Kind` a los **mismos** `Draw*`. Las **cuatro ventanas ricas** (selectivo, dinámico, cama,
cabecera) **adoptan** el shell para esas cuatro capacidades —sus props públicas pasan a getters sobre la
sesión— eliminando la duplicación real vigente (idiom de GUID ×3, la clase `RecomputeDeferral`, el
debounce de la cabecera y el bloque de flags de `RequestDraw` ×3); el larguero, sin identidad ni
inserción, no adopta. El **estado propio** de cada editor (matriz por fondo y `BuildSystem` del selectivo,
`Recompose`/módulos del dinámico) **queda reservado para I-20/I-21**. **No** cambia geometría, BOM, GUID,
edición multivista, persistencia (I-11 intacto: los `*SourceProjectToInsert`/`*SourceDocumentToInsert` se
transportan tal cual), formatos ni la UI/etiquetas/orden del menú (`RackMainMenuWindow.xaml`
byte-idéntico a `main`). El dueño **aprobó la validación manual en AutoCAD** (menú, biblioteca, comandos
directos `RACKSELECTIVO`/`RACKDINAMICO`/`RACKCAMA` y `RACKEDITAR` round-trip con el mismo GUID) y la
**owner-validation** de comportamiento y apariencia. Rebasada sobre `main` vigente (`646614d`, Merge I-19
sobre I-12), reconciliando `docs/initiatives/README.md` (conflicto manual único, preservando I-14/I-19),
los dos `.csproj` (el `LangVersion`/`Nullable` centralizados por I-12 heredados **+** `InternalsVisibleTo`
y la copia de catálogos de I-15) y `docs/ideas-futuras.md`, **sin** incorporar código funcional de I-19.
La rama se integra por `git merge --no-ff` en esta sesión.

**I-21** (`refactor/dynamic-editor-state`) cierra la **extracción del estado del editor dinámico** a
`RackCad.Application`, **sin cambio de comportamiento observable**. Mueve a
`src/RackCad.Application/Systems/` lo que era code-behind privado de `RackDynamicSystemWindow`:
`DynamicEditorCell`/`DynamicEditorFront`/`DynamicEditorValues` (filas/celdas y buffer de edición),
`DynamicFrontMatrix` (la matriz frente×nivel **y la selección** con todas sus mutaciones —alta/baja de
frentes, ajuste, toggle, commit, aplicar por alcance vía `DynamicRackCellScopeResolver`, snapshot/rollback,
refresco/restauración desde el sistema resuelto y la proyección a `DynamicRackFrontDesign`—),
`DynamicEditorSafety` (la regla «dibuja» y la copia de selecciones) y
`DynamicAnnotationOptions`+`DynamicEditorDesignAssembler` (la **recomputación y construcción del diseño**:
`MustRebuild`, `Snapshot`/`RestoreHeaderFondos`, `UpdateHeaderHeightInPlace`, `BuildDesign`, componiendo el
builder y el resolver existentes sin duplicar geometría). La ventana queda **coordinando controles, eventos,
render y diálogo** sobre el Editor Shell (I-15); `Recompose` conserva su orquestación y el code-behind baja
de ~3,339 a ~2,838 líneas. **No** cambia geometría, planes, BOM, GUID, nombre, `Section`, edición
multivista, persistencia I-11, metadatos desconocidos, fallbacks legacy, cabeceras legacy ni la cama
integrada; el XAML es idéntico. El dueño **aprobó la validación manual en AutoCAD** (matriz, selecciones y
aplicación por alcance; cabeceras calculadas y personalizadas; seguridad/IN-OUT/intermedios; previews y
vistas vinculadas; geometría/BOM; biblioteca/legacy/round-trip; actualización en sitio con el mismo GUID) y
la **owner-validation**. `origin/main` **no avanzó** desde `bfda406` (Merge I-15): **sin rebase**; la rama se
integra por `git merge --no-ff` en esta sesión.

**I-20** (`refactor/selective-editor-state`) entrega el **primer eslabón de la extracción de estado por
editor** de la pista C de UI (hallazgos U1/U3), **sin cambio de comportamiento observable**. Extrae el
**estado propio** del editor selectivo —hoy en campos privados de `RackSelectiveWindow` (~2,452 líneas,
archivo caliente)— a clases **puras y testeables** de `RackCad.Application.Systems`: `SelectiveEditorState`
(dueño de la matriz de trabajo, las matrices por fondo, la selección y las cabeceras/peraltes por poste, con
las operaciones `InitMatrix`, snapshot/restore, save/load fondo, `CloneAligned`, `ResizeBays`, add/remove
nivel, `ClampSelection`, `ApplyScope`, `MaxFrenteCount`, `SyncPostCabeceras`, `BuildBayDesigns`,
`FondoMatrixFromDesignBays` y `BuildDesign`), más `SelectiveEditorCell`/`SelectiveEditorFondoMatrix`
(equivalentes verbatim de los anidados `Cell`/`FondoMatrix`), `SelectiveApplyScope` y `SelectiveDesignInputs`
(el contrato de entradas escalares que la ventana lee de sus controles). La ventana **observa** ese estado
(propiedades de acceso) y **delega** las operaciones; conserva el pintado (matriz + previews), el editor de
celda, los eventos, la recomputación coalescida (shell I-15), la orquestación de carga y el resolve/preview
ligado al catálogo (`BuildSystem`). La **superficie pública que consume el Plugin** (`InsertRequested`,
`SystemToInsert`, `DesignToInsert`, `RackId`, `RackName`, `InsertView`, `UpdateOnly`, `SetDimensionStyles`,
`LoadExisting`, `LoadForNew`, `Session`) **no cambia**. **No** cambia geometría, BOM, GUID, inserción/
actualización, persistencia ni metadatos **I-11**, catálogos, compatibilidad legacy ni round-trip; la
equivalencia queda fijada por una **caracterización STA** que verifica que `load→build` produce el **mismo
dibujo resuelto** (frontal por fondo + planta + cortes laterales + altura) antes y después del refactor.
**No** implementa I-22 (colocación de seguridad; orden fijo, I-20 primero), **no** toca el editor **dinámico**
(I-21) ni la asimetría vigente de estilos de cota. Alineado con `ARCHITECTURE.md` §7.1/§7.3 ("estado de editor
puro + vista WPF"; "el estado del editor se extrae a Application"). El dueño **aprobó la validación manual en
AutoCAD** y la **owner-validation** sin observaciones sobre la punta de implementación `0f43087`. **Rebasada
sobre `main` vigente (`2a30fef`, Merge I-21)** reconciliando sólo la documentación compartida (HANDOFF,
ROADMAP e índice de iniciativas); I-21 sólo toca el editor **dinámico**, no el selectivo, por lo que la
validación se conserva. La rama se integra por `git merge --no-ff` en esta sesión.

**I-05** (`feature/guardrail-unidades`) añade una **guardia de unidades** visible y **NO bloqueante** en el
límite de AutoCAD (hallazgo D4 de la auditoría; **ADR-0005 aceptado**), **sin cambio de comportamiento de
producto** salvo el aviso nuevo. `RackUnitsGuard` (en `RackCad.Plugin`, **único que lee `INSUNITS`**) mapea el
`UnitsValue` del `Database` activo a la categoría neutral `DrawingUnits` y delega la decisión en la política
**pura** `DrawingUnitsAdvisory` de `RackCad.Application` (sin dependencia de AutoCAD): si el dibujo **no** está
en pulgadas —incluido `unitless`— escribe **una** advertencia en la línea de comandos **antes de la primera
modificación del DWG**. **No** convierte, reescala ni reinterpreta geometría: RackCad sigue dibujando en
pulgadas (la conversión real queda **diferida** a una iniciativa futura, ADR-0005). Cableada **una vez por
operación** (sin repetir por alias ni por vista/bloque) en las rutas de inserción: menú `RACKCAD`,
`RACKSELECTIVO`, `RACKSISTEMADINAMICO`, `QUICKCAMA`, `RACKCABECERA`, `QUICKCABECERA`; en `RACKEDITAR` avisa
**solo al insertar una vista nueva** (`!UpdateOnly`, antes del primer `RedrawInPlace`), **no** en una
actualización pura; y `RACKLAYOUT`/`RACKRELLENAR` (con sus alias) avisan antes de sus prompts. `RACKDUPLICAR`
queda fuera (clona geometría ya dibujada a la misma escala). El cableado se fija con **source-guards** (leen el
`.cs` del Plugin como texto, sin cargar AutoCAD) y la decisión pura con pruebas. El dueño **aceptó ADR-0005** y
**aprobó la validación en AutoCAD 2025** y la owner-validation. La rama se integra por `git merge --no-ff` en
esta sesión.

**I-24** (`refactor/ui-tests-editores`) cierra la **pista de UI** con **pruebas de editores** en
`tests/RackCad.UI.Tests` (hallazgo U3), **sin cambio de comportamiento**: es una iniciativa de pruebas más un
**único seam interno** de prueba. Añade **29 pruebas** (139→168 UI): el `RackFrameConfiguratorViewModel` —antes
**sin ninguna prueba**— (mutaciones estructurales altas/bajas/división/combinación de horizontales, recomputación
síncrona del modelo físico, arreglos de bracing, selección múltiple, BOM, persistencia round-trip, rutas negativas
deterministas); la **adopción del estado dinámico** (I-21) por `RackDynamicSystemWindow`, caracterizada por
**punto fijo del doble build** con una **firma COMPLETA del dibujo** —todos los cortes laterales (con su índice) +
frontal de salida + frontal de entrada + planta, por instancia, **incluidas anotaciones y cotas** (`Text`/
`DimensionOffset`/`DimensionStyleName`), normalizando el `Name` del sistema resuelto antes de comparar— sobre un
diseño **no default** con valores por celda/larguero y opciones de anotación verificados en round-trip; y la
**identidad/inserción/actualización round-trip** de las ventanas **selectiva** y de **cama**. Las pruebas de
inserción/actualización recorren los **handlers WPF reales** (Click real vía `RaiseEvent(ButtonBase.ClickEvent)`
sobre los botones de la ventana, **no** `session.RequestInsert/RequestUpdate` directo), verificando
identidad/nombre/vista/sección/`UpdateOnly`, el **tipo concreto** de `InsertionRequest`, la **correspondencia
estricta** del payload (la firma del dibujo construida desde `request.Design` —resolviéndolo y normalizando el
nombre— iguala la construida desde `request.System`) y la metadata de origen **I-11**. El **único cambio de
producción** es el seam interno `RackDynamicSystemWindow.BuildDesignForTest` (reenvía al `Recompose` privado
existente, **sin reglas nuevas**, no usado en producción; +10 líneas). **No** cambia XAML, geometría, BOM,
persistencia, handlers, Draw Services, catálogos, bloques ni reglas de producto; por eso **no** requiere
validación en AutoCAD (`requires_autocad: false`) ni owner-validation (`requires_owner_validation: false`).
**Rebasada sobre `main` vigente (`a50c4ec`, Merge I-05)** reconciliando **sólo** el índice de iniciativas
(`docs/initiatives/README.md`: se conservan íntegras las entradas de I-05 e I-24). La rama se integra por
`git merge --no-ff` en esta sesión.

**I-22** (`refactor/safety-placement`) salda los hallazgos **E6** y **E7** de la auditoría 2026-07 sobre la
**seguridad del selectivo**, **sin cambio de comportamiento observable** (fijado por caracterización **golden**:
multiset de instancias Safety/Tope/Separator/Pallet en frontal/lateral/planta + el BOM de seguridad, en 7
escenarios que incluyen medio frente y cuádruple profundidad). Cuatro entregas: (1) **servicios/planes puros de
colocación por familia parametrizados por vista** —`SelectiveTopePlan` (topes físicos por spot + su resultado
**frontal** propio `BuildFrontal`), `SelectiveTarimaPlacement`, `SelectiveSeparadorPlan`, y la unificación del
consumo de `SelectiveParrillaPlan` (`Cells`/`DeckCells`)— con los builders frontal/lateral/planta y el BOM como
**orquestadores** (mueren las travesías duplicadas por vista `TallyByTramo`/`ParrillaExistsAt` y las fórmulas
copiadas de subida-y-snap); la regla de cada familia vive en **un solo sitio**. (2) **Descomposición por subtipo**:
`SelectiveSafetySelection` compone `SelectiveTopeConfig`/`SelectiveDesviadorConfig`/`SelectiveParrillaConfig`/
`SelectiveDefensaConfig`/`SelectiveGuiaConfig` (cada una con `DeepCopy` propio; las propiedades planas se conservan
como accesos delegados), y la persistencia mapea con **DTO reales por familia** (`SafetySelectionDocuments.cs`,
`From`/`ToDomain`/`WriteInto`/`ReadFrom`) que **aplanan/desaplanan** contra el `SafetySelectionDocument` **plano**
—el formato de alambre queda **byte-idéntico** (compartido con la ruta dinámica), sin JSON anidado ni convertidores,
con fallback legacy y round-trip por subtipo. (3) **Paso de troquel único**: los 5 sitios que hardcodeaban `2.0`
como snap referencian `SelectiveRackDefaults.TroquelPaso` (mismo valor, mismo resultado). (4) **Adopción de
`SelectionMatrix`** (I-14) con soporte de **celda ausente** (rejillas dentadas; `CellCount` cuenta solo presentes,
`Toggle` sobre ausente no reporta cambio) por las rejillas **tope/desviador/guía-entrada**, conservando idénticos
contenido, cabeceras, orden, off-cells y controles auxiliares. El **frontal de tope** conserva su naturaleza
esquemática por frente: `BuildFrontal` resuelve su intención pura (celdas activas, niveles, tramos cargados,
offsets, longitud+allowance, Y fuente del snap) como un resultado **distinto** de los spots físicos —no los
proyecta, para no duplicar en pares por fondo—, y `AddTopes` solo proyecta la vista. **No** cambia geometría,
planes, BOM, GUID, identidad, inserción/actualización, persistencia ni metadatos **I-11**, catálogos, nombres de
bloque, mensajes, selección, defaults, interacción visible ni comportamiento multivista; **fuera de alcance** I-25
(guardas traseras), Push Back/I-18, el editor **dinámico**, el rediseño visual y las reglas de producto (parrilla
con contador por celda y defensa por poste **no** se fuerzan a matriz plana). Alineada con `ARCHITECTURE.md`
§7.3-7.4 (servicios de colocación por familia/vista; configuraciones de seguridad por subtipo con DTO propio).
**Rebasada dos veces sobre el trunk vigente** (`9a895e4`→`a50c4ec` Merge I-05→`27ffdf3` Merge I-24) reconciliando
**sólo** documentación compartida (índice de iniciativas; `ideas-futuras` auto-fusionado); I-05 e I-24 tocan código
**disjunto** de I-22. El dueño **aprobó la validación en AutoCAD 2025 y la owner-validation sin observaciones**
(§2 y §5). La rama se integra por `git merge --no-ff` en esta sesión.

**I-17** (`refactor/clon-unico-cabecera`) unifica las **tres** implementaciones de deep-clone de
`RackFrameConfiguration` (hallazgo **U4** de la auditoría: una manual + dos por serialización, la manual
desincronizada con cada campo nuevo) en **un solo** `RackFrameProjectStore.DeepCopy` —el round-trip del store de
serialización— que el **dinámico** (`RackDynamicSystemWindow.Clone`), el **selectivo**
(`RackSelectiveWindow.CloneCabecera`) y el **configurador** (`RackFrameConfiguratorViewModel`) consumen; se
elimina el clon manual campo-por-campo del configurador (`CopyConfiguration` + 7 ayudantes) y sus dos rutas
reasignan `Configuration`. El clon es **completo**: el modelo **persistido** por el documento, el **derivado**
(`Members`, elevaciones, miembros por panel) reconstruido en la carga por `RefreshPhysicalModel`, y las
**excepciones runtime** (`FrameExceptionOverride`) —que el documento **no** persiste ni `RefreshPhysicalModel`
reconstruye— **reanexadas dentro del propio `DeepCopy`** (con `CloneException`), **sin** tocar el DTO, el formato
de alambre ni `Save`/`Load`. **No** cambia dibujo, geometría, BOM, GUID, persistencia física, DTO, catálogos, los
stores de **I-03** ni la UI: el diseño clonado es **idéntico**, fijado por comparación **profunda** del grafo
(modelo persistido, derivado **miembro-a-miembro** y excepciones), una **guarda por reflexión** que obliga a
clasificar toda propiedad futura de `RackFrameConfiguration` como persistida/derivada/runtime-preservada, y una
**regresión de I-11** que prueba la preservación de `ExtensionData` al clonar la cabecera vía
`WithSourceMetadataFrom`. Por eso **no** requiere validación en AutoCAD (`requires_autocad: false`) ni
owner-validation (`requires_owner_validation: false`; AUTOMATION_PLAN I-17 = no|no|no). `origin/main` **no
avanzó** desde la base `f674bd4`: **sin rebase**; I-03 (`refactor/fallos-silenciosos`) sigue activa en su worktree
pero **no** integrada (sin conflicto en `RackFrameProjectStore.cs`, cuyo cambio de I-17 es aditivo). La rama se
integra por `git merge --no-ff` en esta sesión.

**I-03** (`refactor/fallos-silenciosos`) salda los hallazgos **P1** y **D2** de la auditoría 2026-07 (fallos
silenciosos), **sin cambio de comportamiento funcional** (cambio **aditivo**: añade un rastro, no altera el
flujo). Un **logger mínimo** best-effort en `RackCad.Application.Diagnostics` (`RackLog` fachada +
`RackDiagnosticsLog` escritor + `RackLogFormatter` puro) escribe a `%AppData%\RackCad\logs` (nunca lanza,
thread-safe); `Report()` del Plugin registra la excepción completa **con stack** conservando idéntico su mensaje
de línea de comandos (cubriendo de paso todos los `catch (ex) => Report(ex)`); los **14 `catch`** antes
silenciosos del Plugin y `RackCatalogLoader` (fallo de carga + aviso de catálogo vacío) registran y siguen
tragando igual. Las escrituras de los **4 stores** (`RackProjectStore`, `RackFrameProjectStore`,
`UserTemplateStore`, `UserSettingsStore`) pasan por un helper de **escritura atómica** (`AtomicFile`: temp +
`File.Replace`/`Move`, sin crear el directorio destino, conservando la precondición de cada store). La carga de
los stores best-effort (`UserSettingsStore`/`UserTemplateStore`) **distingue por la excepción** un archivo
ausente (`FileNotFoundException`/`DirectoryNotFoundException` → default silencioso) de uno ilegible
(`JsonException` → cuarentena `.bad` + log; cualquier otro fallo de lectura → log sin cuarentena), y `CorruptFile`
registra también el **fallo secundario** al mover el `.bad`. **Preserva I-11** (versiones, metadata, geometría,
BOM, GUID, formatos, fallback legacy y la clave del Xrecord) y deja idénticos comandos, alias y mensajes visibles;
**no** toca catálogos, `deploy/`, `.sln` ni el `.csproj` del Plugin (solo añade `InternalsVisibleTo(RackCad.Tests)`
en el de Application). **No requiere validación en AutoCAD** (`requires_autocad: false`; ROADMAP no la marca con
✋) ni owner-validation. La rama se integra por `git merge --no-ff` en esta sesión.

I-07 (`docs/adr-retroactivos`) queda **integrada** el **2026-07-22**. Retro-documenta las trece
decisiones que la antigua §7 conservaba temporalmente como **ADR-0006 a ADR-0018** (una por decisión):
renumeró a 0006-0012 los siete ADRs ya redactados —tras el rebase, porque `main` ocupó 0003-0005— y
añadió 0013-0018. El dueño los **aceptó** el 2026-07-22 («Sí, apruebo»; registro en
`docs/automation/decisions/I-07.md`), sin modificarlos y conservando las limitaciones sobre fecha,
decisores y evidencia originales. Es **solo documentación**: no cambia producto, catálogos, pruebas ni
build; su cierre retira esas decisiones de HANDOFF §7 (ahora viven en `docs/adr/`).

**I-30** (`architecture/editor-visual-shell`, Fase 5) queda **integrada** el **2026-07-24**. Funda el
**shell visual común de editores** en `src/RackCad.UI/Shell/` y **migra realmente `RackDynamicSystemWindow`**
al shell, **sin cambio de dibujo, BOM, GUID ni persistencia**. El shell `RackEditorVisualShell` es un
**control lookless con plantilla** (`Themes/Generic.xaml` + `[ThemeInfo]`), **no** un `UserControl` ni una
clase base de `Window`: esto permite que un editor inyecte contenido con `x:Name` en los slots sin el error
`MC3093` de ámbito de nombres (un `UserControl` lo prohíbe). Expone nueve slots de contenido como
Dependency Properties (`SidebarHeader` neutral/opcional, `SidePanelContent` con scroll, `MatrixContent`
opcional que colapsa dejando al preview llenar, `PreviewContent`, `StatusContent` fuera del scroll, y las
cuatro categorías neutrales de acción `Leading/Secondary/Primary/Trailing`), con `EditorStatusPresenter`
(severidades info/success/warning/error, **coloreadas por los tokens `ShellStatus*Brush`** vía
`ShellResources.Require` que **falla ruidosamente** ante un token ausente/mal tipado), `EditorActionBar`
(WrapPanel que nunca recorta) y `EditorActions.Button` (estilos habilitado/deshabilitado + motivo por
tooltip). Los **tokens con nombre** de `Themes/AppStyles.xaml` (tamaño/color/tipografía/espaciado) son la
**única fuente** del contrato visual; la ventana consume el tamaño común vía el **estilo compartido
`EditorShellWindowStyle`** (`Width/Height/MinWidth/MinHeight/Background/FontFamily/FontSize` por
`DynamicResource`), eliminando los tamaños hardcoded y el bypass `MinWidth/MinHeight=0`. `ShellMinHeight`
es **672** (no 640): la ventana MOSTRADA al mínimo pierde el marco no-cliente (~39 DIP) y a 640 el cliente
(~601) dejaba solo ~4 px sobre el status; 672 da ~633 de cliente y ~36 px de margen, así el mínimo acomoda
sidebar/matriz/preview/status/action bar **sin solape ni recorte** y el `ClipToBounds` del work-area queda
como pura defensa. La ventana **conserva exactamente** sus 63 `x:Name`, handlers, parsing, `LostFocus`,
selección/multiselección, recomputación, preview (el `Canvas` se aloja tal cual; **no** se adopta el control
`PreviewCanvas`, sin prueba de equivalencia), vistas, inserción, actualización, BOM, GUID y persistencia; el
`.cs` de la ventana **no cambia** en la migración de tamaño. El shell es **agnóstico a `RackSystemKind`** y
no admite ramas por sistema. **Fuera de alcance y sin tocar**: `RackSelectiveWindow` (**es I-31**),
`feature/push-back` (**solo lectura**, intacta `b2d9e9d`), cama/configurador/larguero, geometría, resolvers,
BOM, persistencia, catálogos, handlers y Plugin. **ADR-0019** (shell por composición y slots) **aceptado por
el Owner**. Rebase final **no** necesario (`origin/main` no avanzó desde `8a1bce5`). El Owner **validó en
AutoCAD 2025 los 12 puntos** (§2). La rama se integra por `git merge --no-ff` en esta sesión. **Handoff
obligatorio: I-31 (Selectivo al shell) → reanudación de I-18 (Push Back).**

**I-31** (`refactor/selective-visual-shell`, Fase 5) queda **integrada** el **2026-07-24** (merge
`--no-ff` `ad0ea1f`; base `origin/main` = `40a2c8e`, **sin rebase**). Migra **`RackSelectiveWindow`** al
**shell visual común** (`RackEditorVisualShell`, I-30) por composición y slots —`SidePanelContent`/
`MatrixContent` (con el selector de fondo)/`PreviewContent`/`StatusContent` + categorías neutrales
`Leading` (Actualizar) / `Secondary` (Lista de materiales + Guardar en biblioteca) / `Primary`
(Insertar frontal/lateral/planta) / `Trailing` (Cerrar)— y consume el contrato de tamaño común
`EditorShellWindowStyle`, **sin cambio de dibujo, BOM, GUID, persistencia, handlers ni comportamiento**.
Es una **adaptación exclusivamente XAML**: `RackSelectiveWindow.xaml.cs` es **byte-idéntico a
`origin/main`**; se conservan los **45 `x:Name`**, los 31 handlers, todos los Content/ToolTip/Text (diff
vacío), la **selección de una sola celda + alcance** Celda/Nivel/Frente/Todas (`main` **no** implementa
multiselección en la matriz principal del selectivo y I-31 **no** la agrega —discrepancia con el
Dinámico registrada como decisión consciente—), el selector y matrices por fondo, el editor de celda,
cabeceras/peraltes por poste, seguridad, previews frontal/lateral, inserción frontal/lateral/planta,
actualización en sitio, BOM, biblioteca, metadata I-11, round-trip y los estados habilitado/
deshabilitado con motivo + `ToolTipService.ShowOnDisabled`. Elimina la segunda composición exterior
propia (grid 2×3, panel de 342 px con scroll propio, disposición independiente de matriz/preview/status,
barra inferior). Añade `SelectiveShellMigrationTests` (19 pruebas) y amplía `EditorWindowTestSupport`
para localizar por contenido los botones **sin `x:Name`** en los slots del shell (aditivo). Cubierta por
**ADR-0019** (ya aceptado); no requirió decisión nueva. El Owner **validó en AutoCAD 2025 los 12
puntos** sobre el DLL Debug del SHA `b638653` (`1.0.0+b638653…`) **sin observaciones** (§2). Segundo
eslabón de la secuencia **I-30 → I-31 → reanudación de I-18**; `feature/push-back` (`b2d9e9d`) permanece
**intacta**. **Handoff obligatorio: reanudación de I-18** (rebasar `feature/push-back` sobre el nuevo
`origin/main` y migrar `RackPushBackSystemWindow` al shell, en su propio chat/worktree).

**I-36D** (`feature/perfiles-aisc-s`, Fase 6) queda **integrada** el **2026-07-28** y es la iniciativa
separada que I-36B dejó escrita como **requisito futuro obligatorio**. Incorpora las **28 filas**
`Type = S` de la AISC Shapes Database v16.0 —las que I-36A dejó fuera, **contadas y declaradas**, nunca
perdidas— como **familia propia**: token estable `S`, id `AISC-S-S10X25_4` (el punto de la designación
normaliza a `_` por ADR-0021, ya aceptado; el EDI conserva su punto en su propio campo),
`SSectionDimensions` como **tipo propio y no alias de `WSectionDimensions`**, `structural-sections-s.csv`
generado y catálogo total **1 011**. Los **cuatro CSV anteriores quedan byte-idénticos**, `secciones.csv`
intacto y `mapperVersion` sube a `I-36D.1` para que un catálogo del mapper anterior **falle ruidosamente**
en vez de cargar con una familia ausente.

Lo que la obliga está **medido contra el libro, no citado**: la fuente **no publica la pendiente del
patín ni ningún radio explícito**, ni para S ni para ninguna familia —el único encabezado con `tan` es
`tan(α)`, que es de ángulos simples y está vacío en S; `kdes`, `kdet`, `k1` y `T` son **distancias al pie
del filete** y el Readme nunca las llama radios—. Y una S sin pendiente **se lee como una W**: a
diferencia de los canales C, donde la aproximación pierde detalle, aquí perdería la **familia**. Por eso
**ADR-0023** separa dos autoridades: la **tabulada** es AISC y conserva identidad, dimensiones, `A`, peso,
propiedades y centroide —copiados, **jamás recalculados desde el contorno**—; la **visual derivada** es
RackCad y cubre sólo la pendiente `1:6`, la lectura de `tf` como espesor medio del vuelo libre *dentro de
la representación*, el radio visual del filete y la punta aguda. La autoridad viaja en un eje
**ortogonal** a `SectionFidelity`, que **no cambia**: `TabulatedConstrained` para W, HSS, C y L;
`VisualDerived` para S en los dos niveles de detalle.

La regla es **constante para las 28** —sin ajuste por designación y sin ajuste para igualar `A`— y
**degenera exactamente** en la de ADR-0022 cuando la pendiente es cero (`r = kdes − tf`), así que S **no
bifurca** el modelo geométrico. El residuo de área (**+0,25 % a +2,59 %**, media +1,12 %, siempre
positivo) queda **diagnóstico**, con una prueba de banda que falla si alguien lo cierra ajustando la
regla. La **advertencia** —geometría visual derivada, aproximada, no garantizada por fabricante y **no
apta para CNC ni fabricación**— vive en el **tipo**: `StructuralSectionGeometry.Create` se niega a
construir una geometría `VisualDerived` sin su diagnóstico, el inspector la destaca y `RACKSECCION` la
imprime **sin condición**. **No nace ninguna segunda tubería**:
`StructuralSectionRepresentationPlan` sigue siendo la autoridad única que consumen igual el preview y
AutoCAD, y dos guardas de fuente prohíben que el adaptador reimplemente la pendiente o el filete.

**No toca** W, HSS, C ni L, `secciones.csv`, catálogos de sistemas, `blocks.csv`, `blocks-library.dwg`,
`deploy/`, `.github/`, `RackCad.sln` ni `RackCad.Domain`. `origin/main` **no avanzó** desde la base
`202e456`: **sin rebase**. El Owner **aprobó la validación manual en AutoCAD 2025 sin observaciones**
(veredicto `OWNER_APPROVED_ADR_0023`) sobre el SHA técnico `3ffe4df`, y **ADR-0023 pasó de `propuesto` a
`aceptado`**. La rama se integra por `git merge --no-ff` en esta sesión. **Siguiente iniciativa
habilitada: I-37 — Cantilever MVP**, que **no** se abrió aquí.

**I-37A es la primera subiniciativa de I-37 y está INTEGRADA en `main` desde el 2026-07-29.** Funda el
primer **miembro** de RackCad sobre el catálogo neutral de secciones: el subensamble **base–columna**,
puro en Domain y Application. **No dibuja nada** —sin vistas, preview, editor, persistencia de proyecto,
`RackSystemKind`, registros ni AutoCAD—, así que **no requirió NETLOAD ni validación manual**: lo que
entrega son contratos, y lo verificable de un contrato son sus invariantes y sus guardas.

Lo que decide, y que condiciona todo lo que Cantilever construya encima
([ADR-0024](adr/0024-fundacion-cantilever-base-columna.md), **aceptado**): el **diseño vive en Domain con
los ids de sección como texto** —`RackCad.Domain.csproj` no declara ninguna `ProjectReference`, así que no
puede ver `StructuralSectionId`, y el DTO guardaría texto igualmente porque el id tiene constructor privado
y ningún `JsonConverter`—; una **frontera única** de Application parsea, busca en el catálogo y aplica una
política de elegibilidad **inyectable por ids exactos**, sin lista blanca familia→rol (eso lo prohíbe
ADR-0020, y el catálogo sigue sin saber que Cantilever existe); el resultado se tipa por **naturaleza
física** —perfil del catálogo, placa, cartabón, troquel— con el rol como enum, de modo que añadir el brazo
sea un valor y no un `switch` por consumidor; **`PrismaticSectionInstance` es la única autoridad de
colocación** y sección, longitud, extremos y dirección son derivados sin campo de respaldo; y el patrón de
agujeros de la conexión tiene **una sola autoridad**, que consumen igual la placa posterior de la base y la
cara de la columna.

Dos detalles que cuesta caro equivocar. La **coincidencia de troqueles se prueba sobre un datum lógico**
—eje, las dos coordenadas que el eje no consume, y el diámetro— y **nunca** sobre centros 3D: los dos
agujeros de un tornillo están en superficies separadas por el espesor de una placa, así que sus centros
*deben* diferir, y compararlos solo funcionaría mientras alguien siguiera restando espesores en la prueba
(el precedente es PB-004). Y **toda cota exterior sale de `StructuralSectionGeometry.Bounds`**, nunca de
`d`, `bf`, `tw` ni `tf`: `Bounds` es la envolvente del contorno que se va a dibujar, mientras una dimensión
tabulada es un número nominal, y componer contra uno y dibujar el otro es cómo se producen placas que no
casan con su perfil. **49 guardas de fuente** lo comprueban, leyendo el fuente sin comentarios para que los
XML-doc puedan explicar *por qué* no se toca `bf`.

`NominalCutLength == Length` por definición y con prueba, y **no** está liberada para fabricación; el
**peso queda diferido** para toda la línea I-37. **No toca** I-36, los cinco sistemas vigentes, UI, Plugin,
`assets/`, `deploy/`, `.github/` ni `RackCad.sln`. **Siguen sin default aprobado** los dos offsets
obligatorios de troquel, que el resolver rechaza con `CANT_REQUIRED_PARAMETER_MISSING` si faltan.

**I-37B es la segunda subiniciativa de I-37 y está INTEGRADA en `main` desde el 2026-07-29.** Es la primera
que **consume** lo que I-37A dejó resuelto: funda el **brazo** como subensamble puro en Domain y
Application, y tampoco dibuja nada, así que **no requirió AutoCAD ni validación visual**.

Entrega el cuerpo del brazo como una **colección** de miembros con el arreglo como enum —no una subclase por
arreglo—, con los **tres arreglos desde el principio**: **perfil sencillo**, **canal doble encontrado** y
**canal doble espalda con espalda**. Los dos dobles son de la misma sección y quedan **en contacto**, con
separación cero que **no es un campo**: la posición sale de la orientación canónica que I-36 **documenta**
—dorso del alma a −X, patines abriendo a +X— leída de `ChannelSectionGeometryBuilder` y aplicada vía
`Bounds`, sin tocar una sola dimensión tabulada. La **pendiente** es `SlopeRisePer12`, única autoridad con
los grados derivados, y el **extremo libre sube en ambos lados** —simetría especular, no rotación de 180°—,
en `+Y` o en `−Y`.

La **conexión no crea retícula**: el brazo **selecciona** un conjunto contiguo de los troqueles regulares
que la columna ya resolvió, conserva sus **datums exactos** y **observa** su pitch, así que ninguna
constante de espaciado vive en su resolver. La **placa de conexión** abarca el ancho de la columna y crece
**hacia arriba** al pedir más filas; un perfil demasiado aperaltado para las suyas **se rechaza** en vez de
estirarla en silencio. La **placa final** es un solo tipo con dos modos —**tapa** y **tope**—, perpendicular
al eje inclinado, y el tope crece hacia arriba sin tocar el corte del perfil.

**El datum aprobado, y su aproximación.** La cara exterior de la placa de conexión es el **origen del plano
de corte** del cuerpo, y el eje centroidal arranca ahí. Con pendiente y corte a escuadra eso **no** es
quedar a ras: una zona del perfil **penetra visualmente** la placa y la opuesta deja **holgura**, y las dos
magnitudes **difieren** cuando la sección no es simétrica respecto a su origen. El resolver **reporta las
dos por separado** en un diagnóstico informativo; el modelo **declara** la imprecisión en vez de resolverla,
porque el **corte inclinado** y la **preparación de extremo** siguen fuera de alcance.
[ADR-0025](adr/0025-brazo-cantilever-cuerpo-compuesto-y-conexion.md) quedó **aceptado** con el veredicto
`OWNER_APPROVED_ADR_0025_WITH_CURRENT_DATUM`, que conserva ese datum **expresamente**.

Extiende los contratos de I-37A de forma **estrictamente aditiva** —tres valores de enum al final y unos
tokens— y su lógica queda **intacta**. **No toca** I-36, los cinco sistemas vigentes, UI, Plugin, `assets/`,
`deploy/`, `.github/` ni `RackCad.sln`, y **no registra ningún id de producción** ni familia nueva de
catálogo. **Sigue sin default aprobado** `MountingPlateVerticalEndOffset`, además de los dos offsets
heredados de I-37A. Siguen **diferidos** el perfil de brazo visible por omisión —será HSS, pero se fija
cuando exista editor— y el **PTR**, que **no** se equipara a HSS.

**I-37C es la tercera subiniciativa de I-37 y está INTEGRADA en `main` desde el 2026-07-29.** Es la primera
que **compone** en vez de fundar: toma la columna con su base (I-37A) y el brazo con su conexión (I-37B) y
produce una **estación** completa, pura en Domain y Application. Tampoco dibuja, así que **no requirió
AutoCAD ni validación visual**.

Una **góndola sencilla** tiene una columna, una base, una lista de niveles y un brazo por nivel, todos en el
mismo lado activo. Una **góndola doble** tiene **una sola columna física** y **una sola placa inferior**, con
**dos bases espejadas** y **dos brazos por nivel**; la base negativa se **deriva** reflejando la positiva
respecto al plano central, y resolver un segundo subensamble completo está prohibido porque produciría dos
columnas que luego habría que descartar a mano. Los **niveles se comparten** entre lados: un índice y una
elevación por nivel, y en doble **gobierna el lado más restrictivo**.

El **claro libre** se mide **cuerpo a cuerpo en el plano de conexión** —ni ejes, ni centros de troquel, ni
bordes de placa— y el ajuste es **obligatorio hacia arriba** a un **índice** de la retícula, nunca a una
elevación redondeada. Además, las placas de dos niveles no se traslapan y dos niveles no comparten agujero.
La **altura** es automática o manual, con `TopClearFactor` de default `1/3` y nunca menor; una manual
insuficiente **bloquea** con el faltante medido, sin recortar niveles, mover brazos ni reducir el claro.

**Y resolvió una circularidad real**: la altura decide cuántos troqueles existen, los troqueles deciden dónde
caen los niveles, y los niveles deciden la altura mínima. La rompió extrayendo
`CantileverColumnRegularPunchGrid` —que sólo necesita el patrón de conexión y **no** la altura— más una
costura sin altura en `CantileverColumnBaseResolver`. Sin altura provisional, sin columna enorme, sin bucle de
convergencia; y el **pase final se verifica** contra el layout previo y **falla cerrado** si difiere. La
retícula **acumula** en vez de multiplicar, por compatibilidad exacta con lo que I-37A enviaba —con un pitch
no diádico las dos formas difieren—, y su **dominio** es numérico y derivado de la precisión del `double`, no
un límite comercial.

Entrega también la **matriz pura** de brazos —un default más overrides de celda, con alcances celda/nivel/
estación y sus restauraciones, sin celda falsa en el lado inactivo, y persistiendo **sólo** lo que difiere por
comparación **estructural**— y el primer **BOM por componentes** de Cantilever: **un** componente
columna–base por estación (con una o dos bases) y cada **brazo** como componente atornillable agrupado por
**receta física**, con el lado fuera de su identidad. Los **troqueles no son piezas**: son parte de la
identidad de la placa que los lleva.

[ADR-0026](adr/0026-estacion-cantilever-niveles-altura-y-bom.md) quedó **aceptado**. **No toca** I-36, el
contrato compartido de BOM, UI, Plugin, `assets/`, `deploy/`, `.github/` ni `RackCad.sln`. **No añadió ningún
parámetro sin default**: los tres heredados siguen siendo entradas obligatorias. La continuación es
**I-37D**, la última subiniciativa del MVP.

## 2. Última validación real

**I-44 (2026-09-03) — APROBADA.** El dueño cargó por NETLOAD el DLL Debug del worktree de
`fix/push-back-peraltes-intermedios-bom`, construido **exactamente** desde
`4947a1b5e43a291b01e8e43b5a8ff36d74c99186`, **abrió el DWG real que presentaba el defecto** y confirmó que
el **BOM volvió a coincidir**: los largueros intermedios recuperan su `ProfileId`, `Length`, `Peralte` y
`Quantity` por cama, y desaparece la inflación al mayor peralte del rack. Veredicto: **APROBADA**, **sin
rondas rechazadas**. `origin/main` **no avanzó** desde la base
`085ca2f5b33541cfb93c8cdec8cbc8f0368c899f`, así que **no hubo rebase final** y la validación corresponde
exactamente al contenido integrado. El SHA final de rama difiere del validado **sólo en documentación de
cierre**; el binario funcional es idéntico.

**I-42 (2026-09-02) — APROBADA.** El dueño cargó por NETLOAD el DLL Debug del worktree de
`feature/push-back-compuesto`, construido **exactamente** desde
`077d35ad418615bed4c1d8375ea9cfc0de9fca24`, y recorrió la matriz manual de ocho escenarios con veredicto
**APROBADA — 8/8**: (1) retícula transversal compartida; (2) cama **corrida** y hueco; (3) restauración
con el lado B dormido; (4) ciclo completo de un despertar fallido; (5) edición confirmada y todavía no
recalculada; (6) `RACKEDITAR` sobre un rack compuesto; (7) restauración, rangos no anidados y seguridad
de los dos pasillos; (8) bloqueo de salidas —Insertar, Actualizar, BOM y biblioteca— cuando el recálculo
no es válido. El escenario 2 quedó **APROBADO CON OBSERVACIÓN** no bloqueante —**CORRIDA GAP STORAGE**—,
que el dueño decidió **no implementar ahora**: es la limitación 6 de la iniciativa y ya vive en
[`docs/ideas-futuras.md`](ideas-futuras.md) con su coste real. `origin/main` **no avanzó** desde la base
`088c7b9`, así que **no hubo rebase final** y la validación corresponde exactamente al contenido
integrado. La aprobación llegó tras **varias rondas rechazadas** —candidatos `6c9f778`, `e90442a`,
`3b55ca7`, `36fe5d3`, `67a24d0`, `d6e6372`, `5a73b92` y `82e918b`—, todas corregidas en la misma rama.

**I-41 (2026-08-23) — APROBADA.** El dueño cargó por NETLOAD el DLL Debug del worktree de
`feature/push-back-cell-configuration`, construido **exactamente** desde
`c41aee1b8bcbfc0d6fed7a38b8c4767538648cd2`, y aprobó la configuración por celda de Push Back en AutoCAD
2025: fondos distintos por celda dentro de un mismo frente, restauración al fondo del frente, los cinco
alcances de «Aplicar fondo» y «Aplicar tarima», tarimas por celda en el lateral seccionado y en los dos
cortes frontales, su **ausencia del BOM**, la regresión de I-40 con cabeceras personalizadas por línea y
el caso legacy. La aprobación llegó tras **dos correcciones de representación** de la tarima —tangencia
al rodillo e inclinación en el lateral, alineación por calle en frontal y posterior— que no tocaron
persistencia, BOM ni el contrato de I-41. `origin/main` **no avanzó** desde la base `43181a3`, así que
**no hubo rebase final** y la validación corresponde exactamente al contenido integrado.


La última validación manual de comportamiento sigue siendo I-02 sobre `b0de31d`, después del rebase
sobre `main`: el dueño cargó el DLL Debug del worktree en AutoCAD 2025 y confirmó el checklist
completo del dinámico modular, incluidos vistas, seguridad, BOM, persistencia, round-trip, escenario
legacy y rendimiento. No se realizó ni se requiere una validación nueva en AutoCAD para I-06 porque
su alcance es documental.

La guía vigente para futuras validaciones está en
[guias/validacion-manual-autocad.md](guias/validacion-manual-autocad.md).

I-26 no requirió validación en AutoCAD. El dueño confirmó el CI de rama, incluidos tests y build UI,
y descargó el artifact de cobertura con el XML esperado antes de autorizar su integración.

I-13 tampoco cambia dibujo ni comportamiento de runtime. El dueño autorizó la integración después
de verificar el build limpio del Plugin y la aplicación documental de I-29; no se requiere una
validación adicional mediante NETLOAD.

I-09 no cambia dibujo ni comportamiento de runtime (refactor de la superficie de comandos del
Plugin). No requirió validación en AutoCAD: la equivalencia se sostiene con el inventario de los 26
`[CommandMethod]`, la revisión mecánica del refactor y el CI verde de la rama. AutoCAD: no ejecutado;
no requerido por contrato al conservar comportamiento mediante equivalencia mecánica, builds y CI.

I-08 no cambia dibujo ni BOM (refactor de persistencia/biblioteca de Application con adaptación mínima
de UI). No requiere validación en AutoCAD (`requires_autocad: false`; ROADMAP no la marca con ✋). La
**owner-validation quedó aprobada** por el dueño: recorrió el checklist de la biblioteca de diseños —las
cinco etiquetas, abrir un diseño de cada tipo con su editor correcto, una cabecera legacy sin `Kind`— y
confirmó que no cambió dibujo, BOM ni edición posterior. AutoCAD: no ejecutado como validación formal de
geometría; no requerido por contrato. La equivalencia se sostiene con la caracterización golden/round-trip
y el CI verde de la rama.

I-16 (`refactor/draw-services`) SÍ cambia la superficie de dibujo del Plugin y por eso se validó en AutoCAD:
el dueño cargó por `NETLOAD` el DLL Debug del worktree I-16 (build sobre `2d276a6`, SHA-256 `6AEF0F4D…906B`)
en AutoCAD 2025 y **aprobó** la matriz por familia (selectivo, dinámico con `postIndex` y entrada/salida,
cama, cabecera, cancelación del jig, persistencia y edición posterior) **sin observaciones**; registro en
[initiatives/I-16-autocad-validation.md](initiatives/I-16-autocad-validation.md). El avance del trunk por
I-08 cambia solo Application/UI y **no toca la superficie de dibujo del Plugin**, por lo que esa validación
se conserva tras el rebase (WORKFLOW §6).

I-10 (`architecture/kind-handlers`) **no** cambia dibujo, geometría, BOM funcional, GUID ni persistencia: es
un refactor del despacho por el `Kind` del sobre en el Plugin cuyos handlers son fachadas delgadas hacia el
código de edición/BOM/restamp **sin cambiar** (no toca la superficie de dibujo, a diferencia de I-16). No
requiere validación en AutoCAD (`requires_autocad: false`; ROADMAP no la marca con ✋) ni owner-validation
(`requires_owner_validation: false`), por analogía directa con I-09: la equivalencia se sostiene con el
inventario mecánico (26 `[CommandMethod]` idénticos a `origin/main`, 0 `switch`/cadena por el `Kind` del
sobre restante, 5 `Guid.NewGuid`, 7 `Regen`, mensajes y etiquetas verbatim), la suite completa y el CI verde
de la rama. AutoCAD: no ejecutado; no requerido por contrato.

I-11 (`architecture/persistencia-uniforme`) **sí** requiere validación en AutoCAD (`requires_autocad: true`)
porque toca el round-trip de persistencia. El dueño ejecutó la matriz manual (§10 del contrato) por `NETLOAD`
del DLL Debug (código `eea1c11`) en AutoCAD 2025 y **aprobó todos los escenarios sin observaciones**, incluidos
**B5**, **B6** y **S7**; registro en
[automation/evidence/I-11-autocad-validation.md](automation/evidence/I-11-autocad-validation.md). La
**owner-validation** (biblioteca legacy más preservación de un campo desconocido, con DWG/envelope opcional)
quedó **aprobada** por el dueño (`requires_owner_validation: true`).

I-14 (`architecture/ui-controls`) **no** cambia dibujo, geometría, BOM ni persistencia: crea controles WPF
reutilizables **sin wirearlos** a ninguna ventana existente (patrón strangler), así que no hay comportamiento ni
apariencia nuevos que validar. No requiere validación en AutoCAD (`requires_autocad: false`; ROADMAP no la marca
con ✋) ni owner-validation (`requires_owner_validation: false`). La cobertura se sostiene con las **85 pruebas**
de `tests/RackCad.UI.Tests` (lógica pura + instanciación STA de los controles) y el CI verde de la rama, incluido
el nuevo job `ui-tests`. AutoCAD: no ejecutado; no requerido por contrato.

I-15 (`architecture/editor-shell`) **sí** requiere validación en AutoCAD (`requires_autocad: true`) porque el
menú, la biblioteca, los comandos directos y `RACKEDITAR` toman ahora la identidad y la inserción de la sesión
compartida. El dueño ejecutó el checklist completo por `NETLOAD` del DLL Debug del worktree I-15 (código
`2bd5703`;
`…-I-15-editor-shell\src\RackCad.Plugin\bin\Debug\net8.0-windows\RackCad.Plugin.dll`) en AutoCAD 2025 y
**aprobó todos los escenarios sin observaciones**: menú `RACKCAD` (etiquetas, orden y flujos), apertura e
inserción desde biblioteca para todos los tipos, comandos directos `RACKSELECTIVO`/`RACKDINAMICO`/`RACKCAMA`,
`RACKEDITAR` (actualización en sitio y vistas enlazadas con el **mismo GUID**), geometría y BOM **sin
diferencias**, metadatos y persistencia **I-11 preservados**, recomputación/previews de selectivo y cabecera
**fluidos**, y larguero que **abre y guarda pero no inserta**. La **owner-validation** de comportamiento y
apariencia quedó **aprobada** (`requires_owner_validation: true`). La confirmación normativa del dueño consta en
esta sesión; el gate manual queda cerrado.

I-21 (`refactor/dynamic-editor-state`) **sí** requiere validación en AutoCAD (`requires_autocad: true`) porque
el editor dinámico produce el diseño que se dibuja. El dueño probó a profundidad el módulo dinámico por
`NETLOAD` del DLL Debug del worktree I-21 (código `779ee0c`;
`…-I-21-dynamic-editor-state\src\RackCad.Plugin\bin\Debug\net8.0-windows\RackCad.Plugin.dll`) en AutoCAD 2025
y **aprobó sin observaciones**: comportamiento y apariencia de la ventana; matriz, selecciones y aplicación
por alcance; cabeceras calculadas y personalizadas; seguridad, IN/OUT e intermedios; previews y vistas
vinculadas; geometría y BOM; biblioteca, persistencia legacy y round-trip; actualización en sitio y
conservación del GUID; registro en
[automation/evidence/I-21-autocad-validation.md](automation/evidence/I-21-autocad-validation.md). La
**owner-validation** de comportamiento y apariencia quedó **aprobada** (`requires_owner_validation: true`).

I-20 (`refactor/selective-editor-state`) **sí** requiere validación en AutoCAD (`requires_autocad: true`)
porque la ventana selectiva —cuyo estado se reescribió para observar `SelectiveEditorState` y construir el
diseño vía él— dibuja el rack real. El dueño ejecutó el checklist completo por `NETLOAD` del DLL Debug del
worktree I-20 (punta aprobada de implementación `0f43087`;
`…-I-20-selective-editor-state\src\RackCad.Plugin\bin\Debug\net8.0-windows\RackCad.Plugin.dll`) en AutoCAD 2025
y **aprobó todos los escenarios sin observaciones**: la matriz (clic en celda, ±niveles, altura por frente,
`Piso`, medio frente), «Aplicar a:» celda/nivel/frente/todas, «Editando fondo» (doble/triple profundidad con
separadores), previews frontal y lateral, «Insertar frontal», y con `RACKEDITAR` «Actualizar» en sitio e
«Insertar lateral/planta» ligadas con el **mismo GUID**; geometría y BOM **sin diferencias**, metadatos y
persistencia **I-11 preservados** (incluida la reapertura desde biblioteca, `LoadForNew`), round-trip íntegro.
La **owner-validation** de comportamiento y apariencia (idénticos a lo vigente) quedó **aprobada**
(`requires_owner_validation: true`). Los únicos cambios posteriores a esa aprobación son la **corrección de un
comentario obsoleto** (doc-comment, sin efecto en el comportamiento), el **rebase sobre `main` con I-21** (sólo
documentación compartida; I-21 no toca el selectivo) y este cierre documental; por eso la validación **se
conserva** (WORKFLOW §6). El gate manual queda cerrado.

I-05 (`feature/guardrail-unidades`) **sí** requiere validación en AutoCAD (`requires_autocad: true`) porque el
aviso aparece en la línea de comandos al insertar. El dueño cargó por `NETLOAD` el DLL Debug del worktree I-05
(implementación validada `f78baaf`;
`…-I-05-guardrail-unidades\src\RackCad.Plugin\bin\Debug\net8.0-windows\RackCad.Plugin.dll`) en AutoCAD 2025 y
**aprobó sin observaciones** («Ok, todo funciona»): dibujo en **pulgadas** ⇒ **sin** aviso; **no-pulgadas** y
**unitless** ⇒ **una** advertencia por operación (confirmó que apareció la advertencia completa de RackCad); el
aviso **no bloquea** ni convierte/reescala; `RACKEDITAR` diferencia **actualización** (sin aviso) e **inserción
de vista nueva** (con aviso); `RACKLAYOUT`, `RACKRELLENAR` y los alias se comportan correctamente sin doble
aviso; geometría, BOM, GUID, capas, persistencia y round-trip **idénticos**. La **owner-validation** quedó
**aprobada** (`requires_owner_validation: true`) y **ADR-0005** fue **aceptado** por el dueño; evidencia en
[`automation/evidence/I-05-autocad-validation.md`](automation/evidence/I-05-autocad-validation.md) y decisión en
[`automation/decisions/I-05.md`](automation/decisions/I-05.md). `origin/main` no avanzó desde `9a895e4`, así que
la validación se conserva (WORKFLOW §6): sin rebase.

I-22 (`refactor/safety-placement`) **sí** requiere validación en AutoCAD (`requires_autocad: true`) porque el
refactor toca el código que produce las piezas de seguridad dibujadas (aunque el diseño resuelto es idéntico por
construcción y queda fijado por la equivalencia golden). El dueño cargó por `NETLOAD` el DLL Debug del worktree
I-22 (código validado `3ce7139`, SHA-256 `969580AE…038C`;
`…-I-22-safety-placement\src\RackCad.Plugin\bin\Debug\net8.0-windows\RackCad.Plugin.dll`) en AutoCAD 2025 y
**aprobó sin observaciones** («Listo, probé todo, parece estar correcto»): geometría y colocación de topes,
parrillas, tarimas, separadores y elementos relacionados; BOM; vistas frontal/lateral/planta; medio frente y
múltiples fondos; actualización y vistas ligadas con conservación del **mismo GUID**; persistencia, biblioteca y
round-trip; y la **apariencia e interacción** de las rejillas `SelectionMatrix` (tope/desviador/guía). La
**owner-validation** quedó **aprobada** (`requires_owner_validation: true`); registro en
[`automation/evidence/I-22-autocad-validation.md`](automation/evidence/I-22-autocad-validation.md). `origin/main`
está en `27ffdf3` (Merge I-24) y no avanzó desde que la rama quedó rebasada sobre esa punta, así que la validación
se conserva (WORKFLOW §6): sin rebase adicional.

I-17 (`refactor/clon-unico-cabecera`) **no** cambia dibujo, geometría, BOM, GUID ni la persistencia física: es un
refactor que unifica el mecanismo de clonado de `RackFrameConfiguration`, cuyo resultado es **idéntico** por
construcción (fijado por la comparación **profunda** del grafo —persistido + derivado miembro-a-miembro +
excepciones—, la guarda de clasificación por reflexión y la regresión de I-11). No requiere validación en AutoCAD
(`requires_autocad: false`; ROADMAP no la marca con ✋) ni owner-validation (`requires_owner_validation: false`),
por analogía directa con I-09/I-10/I-24: la equivalencia se sostiene con la suite completa (**993** `RackCad.Tests`
+ **184** `RackCad.UI.Tests`) y el CI verde de la rama (run `29952433309` sobre `28e5cfe`, cuatro jobs). AutoCAD:
no ejecutado; no requerido por contrato.

I-03 (`refactor/fallos-silenciosos`) **no** requiere validación en AutoCAD (`requires_autocad: false`; el ROADMAP
no la marca con ✋) ni owner-validation: es un cambio **aditivo** de diagnóstico (logging + escritura atómica) que
**no** altera geometría, BOM, GUID, comandos ni mensajes visibles. La cobertura se sostiene con las pruebas puras
(formatter, escritor real a directorio temporal, `AtomicFile`, distinción de carga por excepción y **negativos
deterministas rojo→verde**) y los builds; el logger se prueba a través de un seam mínimo (`RackLog.RedirectForTests`)
que evita escribir en el `%AppData%` real. `origin/main` **avanzó** de `f674bd4` a `b60f142` (Merge I-17) durante
la integración, así que la rama se **rebasó** sobre `b60f142` (reconciliación **exclusivamente documental**;
el código de I-03 e I-17 es disjunto salvo `RackFrameProjectStore.cs`, aditivo por ambos lados y auto-fusionado).
AutoCAD: no ejecutado; no requerido por contrato.

I-30 (`architecture/editor-visual-shell`) **sí** requiere validación en AutoCAD (`requires_autocad: true`)
porque migra el editor **dinámico** —que produce el diseño dibujado— al shell y cambia el contrato visual de
la ventana. El Owner **validó satisfactoriamente en AutoCAD 2025 los 12 puntos** funcionales y visuales
sobre el DLL Debug del worktree I-30
(`…-architecture-editor-visual-shell\src\RackCad.Plugin\bin\Debug\net8.0-windows\RackCad.Plugin.dll`),
construido con `--no-incremental` desde el SHA **`d443ee226651c7a80840c8a97e0383163c48d60c`**
(`AssemblyInformationalVersion = 1.0.0+d443ee226651c7a80840c8a97e0383163c48d60c`, verificada). Aprobó los
**12 puntos** sin observaciones: dibujo, BOM, identidad/GUID y round-trip del dinámico migrado idénticos, y
la interfaz del shell (sidebar con scroll, matriz, preview, status y barra de acciones en sus zonas, sin
solape ni recorte al tamaño mínimo). La **owner-validation** de comportamiento y apariencia quedó
**aprobada**. `origin/main` **no avanzó** desde `8a1bce5`, así que **no hubo rebase final** y la validación
vale sobre el árbol integrado (WORKFLOW §6); el commit documental de cierre es solo documentación y no
cambia el binario. Los gates `autocad` y `owner_validation` quedan **resueltos**; registro del SHA validado
y de la versión informativa en §5. La rama se integra por `git merge --no-ff` en esta sesión.

I-31 (`refactor/selective-visual-shell`) **sí** requiere validación en AutoCAD (`requires_autocad: true`)
porque la ventana selectiva migrada dibuja el rack real. El Owner cargó por `NETLOAD` el DLL Debug del
worktree I-31 (SHA validado **`b638653b10bdba5cd5c1d9f814f196c177f18c3e`**,
`AssemblyInformationalVersion = 1.0.0+b638653b10bdba5cd5c1d9f814f196c177f18c3e`;
`…-refactor-selective-visual-shell\src\RackCad.Plugin\bin\Debug\net8.0-windows\RackCad.Plugin.dll`) en
AutoCAD 2025 y **aprobó los 12 puntos sin observaciones**: apariencia alineada con el Dinámico; matriz
(celda, ±niveles, altura por frente, `Piso`, medio frente); selección de una sola celda + «Aplicar a:»
Celda/Nivel/Frente/Todas; «Editando fondo» (doble/triple/cuádruple profundidad con separadores);
cabecera y peralte por poste; seguridad/BOM; previews frontal y lateral; «Insertar frontal» (GUID
nuevo); `RACKEDITAR` «Actualizar» en sitio, «Insertar lateral» e «Insertar planta» ligadas con el mismo
GUID; geometría y BOM sin diferencias, metadatos y persistencia **I-11** preservados, biblioteca/legacy/
round-trip; y los estados habilitado/deshabilitado con motivo por tooltip. La **owner-validation** quedó
**aprobada** (`requires_owner_validation: true`); registro en
[`automation/evidence/I-31-autocad-validation.md`](automation/evidence/I-31-autocad-validation.md). CI
del candidato: run `30108459424` **success** sobre `b638653`. `origin/main` **no avanzó** desde
`40a2c8e`, así que la validación vale sobre el árbol integrado (WORKFLOW §6): sin rebase final. La rama
se integra por `git merge --no-ff` en esta sesión.

I-33 (`feature/frente-en-blanco`) **sí** requiere validación en AutoCAD (`requires_autocad: true`): cambia
lo que se dibuja y lo que se cotiza en los dos sistemas. El Owner **aprobó la validación manual en AutoCAD
2025** sobre el candidato **`b840cfe24578bc9faa3b13dad8b11d90d47aad84`** (DLL Debug del worktree, CI del
candidato run `30240730244` **success** 4/4), **incluida la ronda focalizada de fronteras físicas** que él
mismo dirigió tras revisar el primer resultado: un frente en blanco aislado conserva sus dos postes; dos
frentes en blanco contiguos pierden **el poste que comparten** —con su placa, cabecera, separador, postes
derivados y refuerzos— en frontal, planta y lateral (ese corte deja de dibujarse) y el BOM baja exactamente
esas piezas; tres seguidos pierden dos postes; los blancos **alternados** no pierden ninguno; el rack **no
se encoge** (ancho, largo total y posiciones idénticos); reactivar cualquiera de los dos frentes devuelve el
poste, sus piezas y sus celdas de seguridad. La **owner-validation** quedó **aprobada**
(`requires_owner_validation: true`). `origin/main` **no avanzó** desde la base `0e505d8`, así que la
validación vale sobre el árbol integrado (WORKFLOW §6): **sin rebase final**. La rama se integra por
`git merge --no-ff` en esta sesión.

I-37A, I-37B e I-37C (`architecture/cantilever-base-columna`, `architecture/cantilever-brazo`,
`architecture/cantilever-estacion-bom`) **no requirieron
validación en AutoCAD ni validación visual del Owner** (`requires_autocad: false`,
`requires_owner_validation: false`): ninguna de las dos cambia dibujo, interfaz ni BOM —no hay vistas,
preview, editor, persistencia de proyecto, registros ni una línea de Plugin—, así que **no se ejecutó
NETLOAD** y no era exigible por contrato. Son las primeras iniciativas de la Fase 6 cuyo gate se resolvió
**sobre el código**: lo entregado son contratos, y lo verificable de un contrato son sus invariantes y sus
guardas, no una captura. `origin/main` **no avanzó** ni desde `e0f319f` mientras I-37B estuvo en revisión, ni
desde `0610adb` mientras lo estuvo I-37C, así que sus verificaciones valen sobre el árbol integrado
(WORKFLOW §6): **sin rebase final** en ninguna de las dos.

**I-37D será distinta.** Es la primera de la línea que cambia UI y AutoCAD, así que **sí** requiere DLL,
bundle, `NETLOAD` y la **validación manual del Owner en AutoCAD 2025**, y no puede integrarse sin ese
veredicto.

## 3. Problemas y riesgos activos

- **AMBIGÜEDAD DE PRODUCTO abierta (I-44, no decidida): quién gobierna el larguero intermedio de una cama
  CORRIDA cuando A y B discrepan.** Una corrida es **una sola cama** que cruza la interfaz, así que su
  `IntermediateBeamCatalogId`/`IntermediateBeamDepth` sólo pueden salir de un lado. Hoy salen del **lado
  BAJO**, y de forma **no decidida**: `PushBackRuns.BuildCorrida` copia al frente sintético los
  `DynamicRackLevel` del lado bajo porque las **elevaciones** son de ese lado (decisión del dueño en I-42,
  commit `82e918b`), y ese objeto transporta además el par del intermedio. Antes de `82e918b` mandaba el
  lado ALTO: la autoridad **ya cambió de lado como efecto colateral**, sin que nadie lo decidiera.
  Consecuencia observable: con A (bajo) = 3.5" y B (alto) = 4.5", el BOM publica 3.5" y el 4.5" authored
  desaparece. **I-44 NO cambió esta semántica** y la congeló con una prueba de caracterización
  (`Characterization_ACorridaTakesTheLowSideCellValues_ContractPending`). Requiere decisión del dueño antes
  de tocarla; registrada también en [ideas-futuras.md](ideas-futuras.md).
- **I-44 cambia el corte lateral de un rack COMPUESTO.** `PushBackCompositeContent.Lateral` dibuja con
  `BuildFor`, así que ahora cada cama muestra su peralte propio: dos frentes contiguos con peraltes
  distintos **dejan de fundirse** en una sola pieza dibujada. Es el efecto buscado —plano y BOM ya no
  pueden divergir— y el Owner validó el candidato, pero conviene tenerlo presente al revisar cortes
  compuestos antiguos.
- `ParrillaFrente` y `ParrillaCantidad` siguen siendo globales al rack; una configuración
  heterogénea puede requerir overrides por frente o nivel en una iniciativa futura.
- En medio frente, la cantidad de parrilla es por tramo; el comportamiento es intencional, pero
  debe comprobarse contra el uso real.
- El build del Plugin puede emitir los `MSB3277` conocidos de las referencias de AutoCAD y falla al
  copiar DLL si AutoCAD los mantiene cargados.
- Tras **I-20 e I-21**, el **estado interno propio** de los editores **selectivo** (matriz por fondo, celdas,
  `ApplyScope`, `BuildDesign` → `SelectiveEditorState`) y **dinámico** (matriz frente×nivel, selección,
  recomputación/construcción → `DynamicFrontMatrix`/`DynamicEditorDesignAssembler`) ya vive en
  `RackCad.Application`; la ventana observa el estado y pinta. El resolve/preview ligado al catálogo
  (`BuildSystem` del selectivo, la orquestación `Recompose` del dinámico) permanece en cada ventana por
  diseño, consumiendo el diseño puro que producen esos estados — ya no es deuda pendiente.
- El menú `RACKCAD` abre el selectivo **nuevo** sin `SetDimensionStyles` (asimetría vigente que I-15 preservó
  verbatim; `RACKSELECTIVO` y el abrir-desde-biblioteca sí los fijan) — registrado en `docs/ideas-futuras.md`
  como hallazgo diferido, no corregido en I-15.
- El fallback legacy del dinámico conserva cabeceras sin procedencia como personalizadas para evitar
  pérdida de datos.
- Los catálogos de producto y los overrides del usuario aún comparten ubicación; I-04 preserva el
  DWG de bloques, pero la separación de capas de datos sigue diferida.
- La compilación del Plugin en GitHub-hosted runners depende de la excepción limitada de ADR-0003.
  Debe revisarse a más tardar el 2027-07-20 o antes ante cambios de proyecto, versiones, source,
  runner, caching, artifacts, audiencia, finalidad o documentación incompatible de Autodesk.
- GitHub Actions advierte que las acciones actuales basadas en Node.js 20 se ejecutan forzadamente
  sobre Node.js 24; es una deuda de infraestructura separada de I-13.
- `RackFrameProjectDocument` (biblioteca de cabecera desnuda por `RackFrameProjectStore`, un quinto DTO de
  persistencia) quedó **fuera del alcance de I-11 por decisión aprobada del dueño**; no preserva campos JSON
  desconocidos ni versión no degradada. Es deuda para una iniciativa posterior, no cancelación
  (`automation/decisions/I-11.md`).
- RackCad sigue siendo **mono-unidad en pulgadas**: I-05 añadió la guardia que **avisa** cuando `INSUNITS` no
  es pulgadas, pero **no** convierte ni reescala. La conversión real (frontera explícita DWG↔interno) queda
  **diferida** a una iniciativa futura gobernada por **ADR-0005** (aceptado); la columna `units` de los
  catálogos sigue decorativa. `RACKDUPLICAR` no avisa por diseño (clona geometría ya dibujada a la misma escala).

## 4. Siguiente acción

### I-43 e I-44 están INTEGRADAS y CERRADAS. No hay iniciativa en curso.

**I-43 — Selectivo: edición por alcance y fondos quedó integrada el 2026-09-04** desde
`feature/selectivo-scopes-fondos` con merge `--no-ff`, sobre el candidato
**`d582deed5bbd93083261399e45b2ecc3e16088d7`** —que ya incorporaba `main` (`aff66a8`, I-44) por merge, no por
rebase— y con **validación manual del Owner PASS TOTAL** en AutoCAD 2025 sobre el DLL
`f70d89bffad38cf77fd8b5b51e2951512e34f2af5b7050392c590d8ff4a06d87`.

**Trazabilidad.** BASE `085ca2f5b33541cfb93c8cdec8cbc8f0368c899f` · CLAIM
`788febe52fd7be1dc381e798d462efd2dcc84ba3` · Claim-Id `5ef90a1b-acf8-4f18-a3be-37b75614e1d9` · código final
pre-main `d7b5e5513e86e9e44ebed23f6128bb8c6b70c247` · candidato Owner `d582dee` · CI pre-Owner **33916118566**
success · `RackCad.Tests` **4643 PASS**, `RackCad.UI.Tests` **1216 PASS / 17 skip**, **P0 61/61**, focal I-44
**22/22**.

**Cómo se llegó aquí.** El Gate 8 tuvo un PASS funcional del Owner, pero una **primera revisión arquitectónica**
(8.5) encontró que el contrato no estaba escrito en ninguna parte y que las cajas de texto eran a la vez editor y
autoridad; de ahí salió el plan por gates 8.6A–8.6G, que fijó el contrato en ADR-0032 antes de tocar código. Una
**segunda revisión independiente** (8.9) dictaminó *C — CONDITIONAL* y destapó tres regresiones introducidas por
esos mismos arreglos: encoger el número de fondos desde uno que desaparecía **pisaba la matriz del superviviente**
(bloqueante), el índice destino del combo se validaba **antes** del commit y podía quedar fuera de rango, y un gesto
estructural podía comprometer una celda **sin recalcular**, dejando la preview describiendo un estado que ya no
existía. El Gate 8.6H las cerró, junto con la acumulación de avisos, la etiqueta del poste, los textos contradictorios
y un hallazgo del dueño: la nomenclatura de BOTA de Push Back se había filtrado al Selectivo. El Gate 8.10 incorporó
`main` por merge y produjo el candidato que el Owner validó.

**Lo que NO se hizo, a propósito.** Los follow-ups quedan registrados en
[ideas-futuras.md](ideas-futuras.md): R2-04, R2-09, ARQ-43-10 (purificar `EffectiveCustomAt`, que sigue imponiendo
el Depth in-place), ARQ-43-11, ARQ-43-12, ARQ-43-14, ARQ-43-15, ARQ-43-16, ARQ-43-08B, ID12 (tope de tarima por
lado, SPLIT) e ID13 (frentes en blanco). Dos piden **decisión del dueño**: el **vocabulario de la BOTA en el
DINÁMICO** —que se conservó exactamente como estaba, `Ninguno / Entrada-Salida / Posterior / Ambas`, porque no hay
ninguna decisión registrada para ese sistema— y una **confusión de display** de «Fondo de tarima»/«Fondo de cabecera»
que el dueño reportó una vez y **no se pudo reproducir** con ninguno de los gestos disponibles.

### I-44 está INTEGRADA y CERRADA.

**I-44 — Hotfix Push Back: peraltes incorrectos de largueros intermedios en BOM quedó integrada el
2026-09-03** desde `fix/push-back-peraltes-intermedios-bom`, sobre el candidato funcional
**`4947a1b5e43a291b01e8e43b5a8ff36d74c99186`**, con **validación manual del Owner APROBADA** en AutoCAD
2025 sobre el DWG real que presentaba el defecto y CI verde. `origin/main` **no avanzó** desde la base
`085ca2f`: **sin rebase final**, así que la validación corresponde exactamente al contenido integrado.

**Qué estaba mal.** `PushBackIntermediateBeamLateralBuilder` tenía **dos clientes con significados
distintos y una sola regla**:

- `Build(…)` **proyecta** un corte lateral. Varios frentes se superponen en la proyección y el que se ve
  es el de **mayor peralte**, que tapa a los de detrás. Su autoridad es la envolvente —`PeralteAtPost` con
  poste, `PeralteAt(system, …)` sin él—, y es correcta.
- `BuildFor(…)` **materializa la pieza física de una cama concreta** (I-42,
  [ADR-0031](adr/0031-push-back-compuesto-estructura-unica-y-configuracion-por-lado.md) §8-bis: «un
  larguero intermedio pertenece a una CAMA, no a la estructura», y el BOM lo cuenta con el mismo builder
  que lo dibuja).

`BuildFor` resolvía `ProfileId` y `Peralte` con la **envolvente**, así que el BOM materializaba camas y
cobraba la proyección. Con F1 = 3.5", F2 = 4.5" y F3 = 6" en el mismo nivel, el BOM publicaba
`L=50 P=6 x3 | L=94 P=6 x3 | L=138 P=6 x3`: la **longitud** sí era la del frente, el **peralte** no.

**El contrato, ahora explícito.** Lo fija el **llamador**, mediante un enum `BeamAuthority`
(`Projection` / `PhysicalBed`), **no la forma de los argumentos**. No podía deducirse de ellos: `Build`
pasa `front` y `postIndex` cuando proyecta por poste, y `BuildFor` recibe un `postIndex` **real** desde el
dibujo de un rack compuesto (`PushBackCompositeContent.Lateral`). Deducirlo de los argumentos era
justamente el defecto.

- En una **cama física** el par sale **junto** de `DynamicRackLevelGeometry.At(structure, front, level)`,
  el único accesor que devuelve perfil y peralte del **mismo** nivel resuelto. Antes venían de dos
  consultas independientes —el id del frente de mayor peralte, el peralte de la envolvente del rack— que
  coincidían **sólo** porque ambas usaban un máximo: con dos frentes empatados en peralte y perfiles
  distintos, el id lo decidía el orden del `OrderByDescending`.
- En una **proyección** el par sigue saliendo de la envolvente, **sin cambio**.
- `postIndex` en `BuildFor` sigue participando en colocación y filtro geométrico, pero **no decide**
  `ProfileId` ni `Peralte`.

**Qué NO se tocó.** `PushBackBomBuilder.EmitIntermediates`, el conteo de `Supports`, los filtros
`rearX`/`front.StartX`, la deduplicación, `SystemBomBuilder`, la semántica compartida de
`PeralteAt(system, …)` y `PeralteAtPost(…)`, la persistencia, el editor y el conteo por cama/apoyo de
I-42. **Un solo archivo de producción**:
`src/RackCad.Application/Systems/PushBack/PushBackIntermediateBeamLateralBuilder.cs`.

**Legacy: descartado con historial, no con hipótesis.** No existe ni existió un writer capaz de persistir
`front.Levels[n].IntermediateBeamDepth != front.IntermediateBeamDepths[n]`. Las dos listas se escriben
desde **la misma celda en un solo bucle** desde el commit que introdujo `Levels` (`c91874b`, tanto en el
writer de entonces —`RackDynamicSystemWindow.xaml.cs`— como después en
`DynamicFrontMatrix.BuildFrontDesigns`, verificado en las **seis** versiones del archivo), y el re-sync de
lectura existe desde ese mismo commit en los dos límites (`DynamicRackLevelGeometry.Resolve` y
`DynamicRackFrontDocument.ToDomain`). Por eso **no se construyó ningún documento híbrido**: no habría
writer que lo produjera. Además, en un rack de **un solo sentido** el defecto sólo podía **subir** el
peralte —un máximo no baja, y un frente que materializa intermedios en un nivel está siempre en el
conjunto del máximo—, comprobado con un barrido de **81 configuraciones**. Y **editar y restaurar no
normaliza nada**: dos ciclos completos (`load → RACKEDITAR sin cambios → rebuild → save → reload → BOM`, y
el mismo cambiando el valor y devolviéndolo) dejan el BOM idéntico.

**Clasificación final: A** — bug de autoridad `system`/`post` frente a `front + level`. No hay bug legacy
demostrado.

**Sin ADR nuevo.** No hubo decisión arquitectónica nueva: I-44 **aplica** el contrato que ADR-0031 §8-bis
ya enunciaba. La única decisión pendiente —la cama **CORRIDA**— **no** se tomó y queda registrada en §3 y
en [ideas-futuras.md](ideas-futuras.md).

### I-42 está INTEGRADA y CERRADA (histórico).

**I-42 — Push Back compuesto, bidireccional y camas compartidas quedó integrada el 2026-09-02** con merge
`--no-ff` **`e6bb6d7ce9790e1e9c495cb30e580a9304bccd44`** (padres `088c7b9` y `9ea11d6`), sobre el candidato
funcional
**`077d35ad418615bed4c1d8375ea9cfc0de9fca24`**, con **validación manual del Owner APROBADA** en AutoCAD
2025 —**8/8 escenarios**, el 2 con observación no bloqueante— y CI verde. `origin/main` **no avanzó**
desde la base `088c7b9`: **sin rebase final**, así que la validación corresponde exactamente al contenido
integrado.

**Lo que Push Back gana, en su comportamiento final:**

- **A. Una estructura física, dos configuraciones funcionales.** La *estructura* —postes, perfil y
  peralte, cabeceras, separadores, postes derivados, placas, alturas, seguridad, anotaciones y los
  overrides por línea de I-40— es propiedad **única del rack**. La *configuración funcional* —frentes,
  niveles, elevaciones, fondos, tarimas, celdas y topes— pertenece a **cada lado** y no puede describir
  estructura. No hay «rack A + rack B espejado» ni un BOM deduplicado a posteriori.
- **B. El lado A es el de referencia y es el legacy.** Un rack de un solo sentido no tiene lado B ni
  intención de interfaz: su JSON se re-escribe **byte-idéntico** y su camino de resolución es el anterior
  a I-42, sin pasar por la composición.
- **C. Una sola secuencia de profundidad.** `módulos de A → línea terminal de A → HUECO → línea inicial de
  B → módulos de B`, con los de B **invertidos** porque su pasillo es el otro extremo. El hueco es
  **longitud física real** —nunca un desplazamiento visual, un fondo ficticio ni una posición de tarima—
  y ocupa su propia posición; con separador central se materializa como el **mismo** separador del rack,
  contado una vez.
- **D. Una sola retícula transversal, y la asimetría se expresa con PRESENCIA.** Cada ranura toma la
  **mayor demanda aplicable** de los dos lados (calles, ancho de larguero y niveles) —incluidos los
  overrides de celda— de modo que líneas de postes y BFR son únicos. El número de frentes es **del rack**:
  que una ranura exista sólo en un lado es **presencia** por ranura y por lado, y la retícula se iguala
  siempre **creciendo**.
- **E. La estructura efectiva es editable por lado.** `demanda de celdas → envolvente por ranura →
  estructura PROPUESTA → override manual → estructura EFECTIVA`. La propuesta se deriva siempre; el
  override la **sustituye**, no la acota; restaurar es eliminar el override. Una estructura insuficiente
  **no se corrige en silencio**: las celdas que no caben se declaran imposibles **con su motivo**.
- **F. Tres magnitudes de cama, tres autoridades.** `RequiredBedLength` es lo que exige la **demanda**
  (sólo módulos que alojan tarima; no depende del hueco); `AvailableBedSpan` es lo que ofrece la
  **estructura** (el hueco **sí** suma); `ResolvedBedLength` es la longitud **física** del primer apoyo
  válido. Se cumple `Required <= Resolved <= Available`, y por eso **un hueco positivo puede volver válida
  una cama que sin él no cabe** sin inflar la demanda.
- **G. La topología es por CELDA (ranura × nivel).** Cuatro modos físicos: `Solo A` y `Solo B` (una cama),
  `Encontradas` (**dos** camas independientes con topes independientes) y `Corrida` (**una** cama con una
  longitud, una pendiente continua, un eje y como mucho **un** tope). Un nivel que sólo existe en un lado
  degrada de forma explícita **sin tocar la intención almacenada**, que queda **dormante** y vuelve
  intacta. La topología por defecto depende de cuántos sentidos tiene el rack.
- **H. El fondo de una cama CORRIDA es una autoridad PROPIA por celda**, nunca `fondo(A) + fondo(B)`. Sin
  valor propio hereda un default derivado —la capacidad en fondos de la estructura—; los fondos de A y de
  B **no se borran** al volver corrida una celda, así que cambiar de topología es **reversible**. Y
  `CorridaDepth` son **fondos**, no módulos: el módulo que la cama atraviesa sin almacenar no vuelve a la
  demanda.
- **I. El lado B es una imagen especular FÍSICA.** Cada cama se resuelve con el código ya validado de un
  Push Back de un sentido **en su propio marco** y se lleva al rack con **una sola reflexión rígida**, que
  no toca las elevaciones. Un larguero intermedio pertenece a una **cama**, no a la estructura, y el BOM
  lo cuenta con el **mismo** builder que lo dibuja. Anotaciones y cotas se trasladan, no se reflejan.
- **J. Se comparte la retícula TRANSVERSAL, no la ALTURA.** Las cabeceras son piezas **longitudinales** de
  un lado: su altura, su celosía y sus personalizaciones de I-40 salen de la sub-estructura de **ese**
  lado. Una autoridad global del tipo `max(alturaA, alturaB)` está **prohibida**; subir un nivel en A no
  mueve ni una pieza de B.
- **K. Dos pasillos de carga, y los dos llevan su seguridad.** La autoridad no cambia —sigue siendo la
  única `PushBackSafetyAuthority`—; lo que el rack declara es en **cuántos extremos** se materializa
  (`BothEndsAreLoadFaces`, derivado y no persistido). **Pertenencia, orientación y extremo son tres ejes
  distintos.** El tope posterior vive en el extremo ALTO y su autoridad es **por lado**.
- **L. El ancla de una cama es su extremo BAJO, en las dos direcciones** (decisión del dueño).
  Longitudinalmente, el extremo por el que se carga queda anclado al poste exterior de su lado.
  Verticalmente, «Alto 1er nivel» fija el larguero de entrada y el ALTO se **deriva** contra los
  troqueles: **supersede** la redacción de I-32/PB-004 sin cambiar el criterio de selección. La autoridad
  vertical es UNA, `PushBackElevations`, y la leen las cuatro vistas, la cama, los apoyos intermedios, el
  desviador y el tope.
- **M. El ciclo de vida de la intención dormida es transaccional.** Dormir el lado B **no** borra el lado
  B: su cola compuesta se aparca, se consume **por computación** y sólo se limpia cuando esa computación
  se **acepta**; un despertar fallido no degrada nada, y «Restaurar valores» actúa sobre el sistema
  **efectivo** sin llevarse por delante una intención que no forma parte de él. Cargar otro rack sí la
  borra, y es la única acción que lo hace.
- **N. Las salidas se bloquean después del recálculo.** Insertar, Actualizar, BOM y guardar en biblioteca
  revalidan el resultado **ya recalculado**, de modo que un sistema inválido no llega al dibujo, al BOM ni
  a la biblioteca.

**Dinámico, Selectivo y Cama no se tocan.** Las **seis limitaciones declaradas** viven en la iniciativa
([`docs/initiatives/I-42-push-back-compuesto.md`](initiatives/I-42-push-back-compuesto.md) §5) y las dos
que siguen siendo deuda futura, en [`docs/ideas-futuras.md`](ideas-futuras.md). Entre ellas la
**observación del Owner** del escenario 2 —**CORRIDA GAP STORAGE**: una entrada de topología creada sólo
para guardar el fondo de una corrida fija el default vigente al escribirla; hoy no es observable porque
ningún camino de producción cambia el default—, que queda **registrada y no implementada** por decisión
del dueño.

**[ADR-0031](adr/0031-push-back-compuesto-estructura-unica-y-configuracion-por-lado.md) queda
`aceptado`** por el dueño con el modelo implementado y sus seis limitaciones. A partir de aquí su
contenido es **inmutable**.

---

#### I-41 (integrada el 2026-08-23, previa a I-42)

**I-41 — Configuración por celda de Push Back quedó integrada el 2026-08-23** con merge `--no-ff`
**`a28c9b73965f528ffbf3c2cd893e52da36995063`** (padres `43181a3` y `638c009`), sobre el candidato
funcional **`c41aee1b8bcbfc0d6fed7a38b8c4767538648cd2`**, con **validación manual del Owner APROBADA**
en AutoCAD 2025 y CI verde. `origin/main` **no avanzó** desde la base `43181a3`: **sin rebase final**,
así que la validación corresponde exactamente al contenido integrado.

**Lo que Push Back gana, en su comportamiento final:**

- **A. PB-015 — fondo efectivo por celda.** La celda se identifica por `FrontIndex + LevelIndex`. Su
  fondo se resuelve con **una sola regla de precedencia**, que vive en una sola función
  (`PushBackCellDepth.Effective`): `override de la celda ?? fondo por defecto del frente`, acotado
  después a `[2, envolvente]`. No hay una tercera fuente.
- **B. Default por frente + override por celda.** El **fondo por defecto del frente** es lo que el
  usuario escribe en «Fondos frente» y lo que heredan las celdas sin fondo propio; se persiste **aparte**
  en `PushBackFrontConfig.DefaultPalletsDeep`. Sin ese campo el round trip no sería reversible: una
  envolvente no sabe qué heredaba cada nivel, así que cada guardar/abrir subiría el default hasta la
  envolvente y el override más profundo desaparecería al segundo ciclo.
- **C. Envolvente estructural derivada.** `DynamicRackFrontDesign.PalletsDeep` deja de ser la autoridad
  final de producto y pasa a ser el **mayor fondo efectivo de los niveles activos** del frente. Es lo que
  dimensiona la estructura compartida —módulos, cabeceras, separadores y postes derivados—, y es la razón
  por la que I-40 sobrevive sin ninguna medida especial.
- **D. Todo consume el fondo efectivo.** Cama y su longitud, pendiente, elevaciones, larguero posterior,
  tope posterior, intermedios, las cuatro vistas y el **BOM** preguntan por celda, nunca por frente. El
  BOM cotiza **una cama por celda**, no una por frente.
- **E. PB-016 — `DrawPallet` por celda.** Autoridad por celda, con **default legacy `false`**: Push Back
  no dibujaba ninguna tarima antes de I-41, así que un rack anterior queda **idéntico**. La tarima se
  apoya sobre los **RODILLOS** —`Y de apoyo = origen del rodillo + radio del rodillo`—, **sigue la
  pendiente** de su cama en el lateral y va **centrada en su calle BFR** en los dos cortes frontales.
- **F. `PalletHeight` por celda.** Sigue siendo la autoridad **que ya existía** (`DynamicRackLevelDesign`
  / `DynamicEditorCell`); I-41 **no crea una segunda**. Aplicar por «Nivel» es solo un **alcance** de
  edición masiva, no una propiedad del nivel.
- **G. Tarimas fuera del BOM.** Son `HeaderBlockRole.Pallet`, referencia **visual**. El BOM de Push Back
  se construye desde el **sistema resuelto**, no desde los planes, así que no existe vía por la que
  puedan llegar a él; está fijado por prueba que el BOM es idéntico con y sin tarimas.
- **H. Compatibilidad legacy.** Los tres campos de I-41 son nullable y **ausentes** en todo documento
  anterior. Su ausencia ES el fallback: fondo por defecto = el fondo estructural (que en un rack anterior
  coincide con el de todos sus niveles), sin overrides y sin ninguna tarima. Un documento sin overrides
  ni tarimas **no escribe ningún campo nuevo**.
- **I. Preservación de I-40.** Mientras la **envolvente** no se mueva, el layout de fondos es idéntico,
  `DynamicEditorDesignAssembler.MustRebuild` responde `false`, el recálculo copia el baseline y con él
  sobreviven intactos `ModuleId`, las **cabeceras por línea** y las **alturas de poste derivado por
  línea**. Si la envolvente cambia legítimamente, la reconciliación descarta lo que apunte a un módulo
  que ya no existe, sin dejar overrides huérfanos.
- **J. Corrección final de la tarima** (tras el rechazo de la primera validación manual): tangencia al
  rodillo e **inclinación** en el lateral —se construye en el sistema local de la cama y se lleva a mundo
  con la **misma transformación rígida** del montaje de riel y rodillos, llevando su rotación—, y
  **alineación por calle** en frontal y posterior, con la altura de apoyo evaluada en el extremo que cada
  corte muestra. Ninguna de las dos tocó persistencia, BOM ni contrato.
- **K. Sin modelos paralelos.** Reutiliza `DynamicFrontMatrix`, la multiselección y los cinco alcances
  `Cell/Selected/Level/Front/All`. Las operaciones masivas de I-41 escriben **una sola propiedad** —no
  viajan en `PushBackEditorValues`, porque `Apply` arrastraría el resto de la celda origen— y provocan
  **una sola recomputación**.
- **L. Limitación declarada y ACEPTADA.** El corte lateral **NO seccionado** no dibuja tarimas: ya era
  una envolvente antes de I-41 y no hay celda a la que preguntar. Las tarimas viven en los **cortes**
  laterales y en los dos cortes frontales.

**Dinámico, Selectivo y Cama no se tocan**, y está fijado por prueba que no mencionan nada de I-41.

**[ADR-0030](adr/0030-fondo-por-celda-push-back-y-envolvente-derivada.md) queda `aceptado`** por el
dueño con el modelo implementado, incluida la limitación del punto L. A partir de aquí su contenido es
**inmutable**.

---

#### I-40 (integrada el 2026-08-23, previa a I-41)

**I-40 — Edición de cabeceras de Push Back quedó integrada el 2026-08-23** con merge `--no-ff`
**`bf327b353fc181d3ca5192641c54b9abf96ea39d`** (padres `8a54a4d` y `2673aab`), sobre el candidato
funcional **`b43fcb433eb717ad5484b67f400fb5b77bc03826`**, con **validación manual del Owner APROBADA**
en AutoCAD 2025 y CI verde en los cuatro jobs. `origin/main`
**no avanzó** desde la base `8a54a4d`: **sin rebase final**, así que la validación corresponde
exactamente al contenido integrado.

**Lo que Push Back gana, en su comportamiento final** (no las hipótesis de las rondas rechazadas):

- **A. Cabecera personalizada autoritativa.** Sobrevive Actualizar, RACKEDITAR y save/load con su
  configuración **completa** —altura, PanelClear, horizontales, paneles, placas—, no solo su efecto
  geométrico. Ya no es representable el estado híbrido «Personalizada + configuración predeterminada»:
  se repara en los **tres** límites canónicos (resolver, persistencia y sesión).
- **B. Reutilización.** La configuración de otra cabecera se copia como instancia **independiente**;
  cada destino recibe su propio `RackFrameProjectStore.DeepCopy`. **Sin biblioteca persistente y sin
  referencias entre cabeceras.**
- **C. Selección masiva.** La configuración **origen** es independiente de los destinos: se obtiene una
  vez y se aplica cuantas veces haga falta, **sin reabrir el configurador**. Los destinos son el
  **producto cartesiano `Cabeceras destino × Líneas destino`**, ambos con multiselección y atajos
  «Esta»/«Todas»; la aplicación es **explícita**, **atómica** (valida los dos ejes y resuelve el
  producto entero antes de tocar nada) y **Cancelar** revierte todo lo escenificado.
- **D. Líneas físicas.** Segunda dimensión del modelo: `DynamicHeaderLineOverride` direccionado por
  **`(PostIndex, ModuleId)`**. Una cabecera longitudinal ya no es forzosamente igual en todos los cortes.
- **E. Poste derivado.** Altura global nullable como **fallback compatible** más
  `DynamicDerivedPostLineOverride` **por línea**; vacío = hereda la altura de la cabecera, que es el
  comportamiento histórico.
- **F. Vistas.** El lateral usa la configuración **física del corte**; la **frontal es el corte de la
  PRIMERA cabecera longitudinal** y la **posterior el de la ÚLTIMA** —nunca una envolvente ni un
  `Max()`—; y el **protector lateral del último corte** deja de dibujarse invertido. Las tres vistas y
  el BOM leen la **misma** autoridad, `DynamicFrontGeometry.HeaderConfigurationAtPost`.

**Compatibilidad.** El formato de alambre **solo crece** con dos arreglos **opcionales**
(`HeaderLineOverrides`, `DerivedPostLineOverrides`) y un `double?` (`DerivedPostHeight`): ausentes en
todo documento anterior, y ausente significa el comportamiento de siempre. **GUID intacto.** Sin
paquetes NuGet nuevos, sin shell nuevo y sin tocar `RackEditorVisualShell` ni `RackFrames`.

**Consecuencia declarada sobre infraestructura compartida.** El Dinámico comparte
`DynamicFrontGeometry`, `DynamicRackSystemResolver`, los builders frontal y lateral, `SystemBomBuilder`
y `RackModuleEditSession`. Nunca escribe overrides por línea, así que con ninguno presente su
comportamiento es idéntico; lo que sí cambia para él es que su **frontal** pasa a seguir la
configuración de la cabecera del corte en vez de una altura derivada — la misma corrección que I-40
necesitaba, cubierta por sus suites (Dynamic 205 + 41, Selective 324 + 33, Cantilever 869 + 75).

**No hay siguiente acción autorizada.** Ninguna iniciativa se abre sin instrucción del Owner. I-38
—cálculo resistente, cargas y capacidad— tampoco se abre, y no reabre
[ADR-0017](adr/0017-validacion-cargas-diferida-ram-elements.md).

**Deuda registrada por I-40, fuera de su alcance y sin bloquear el cierre:**

- `DynamicSystemFrontalBuilder.ResolvePlatePeralte` sigue leyendo el peralte de placa de la **primera**
  cabecera con configuración del rack, sin mirar línea ni corte. No es una altura y el Owner no lo
  reportó; queda anotado por simetría con la corrección de las vistas.
- La regla adaptativa del protector lateral vive en `DynamicLateralGuardPlan` y su copia espejada se
  reinterpreta en el lateral. Funciona y está cubierta, pero **el eje del espejo no está modelado**: se
  deduce en cada vista.

### I-39 está CERRADA.

**I-39 — Contrato funcional común de ventanas WPF quedó cerrada el 2026-08-07**, con sus cuatro
subiniciativas integradas en `main`: **I-39A** (`44f84bd`, ronda 2 aprobada, **ADR-0029 aceptado**),
**I-39B** (`2239eac`, 31/31), **I-39C** (`fc9a287`, 37/37) e **I-39D** (`fea745c`, 24/24). Las cuatro con
validación manual del Owner en AutoCAD 2025 y CI verde. **ADR-0029 permanece `aceptado` e inmutable.**

Las **28** ventanas concretas del producto tienen arquetipo declarado y contrato cumplido, o desviación
explícita documentada. La línea **no tocó un solo archivo** de Plugin, Application, Domain, Catalogs,
`assets/`, `deploy/` ni `.github/`.

**I-37 quedó cerrada el 2026-08-03** con la integración de I-37D (merge `fa7f8c5`). Sus cuatro
subiniciativas —I-37A fundación columna–base, I-37B brazo, I-37C estación y BOM, I-37D línea,
arriostramiento, vistas, editor y AutoCAD— están **integradas en `main`**, y ADR-0024 a ADR-0028 están
**aceptados**.

**No hay siguiente acción autorizada.** Ninguna iniciativa se abre sin instrucción del Owner.

**Deuda registrada por I-39, fuera de su alcance y sin bloquear el cierre.** Ninguna de las tres corrige
un defecto observado: mueven producto validado por preferencia, no por contrato. Lo único que el Owner ha
decidido sobre ellas es **no incorporarlas a este cierre**; su destino final sigue abierto.

- **Ubicación explícita** de las cinco ventanas que declaran `CenterOwner` sin que pueda existir `Owner`
  —`RackWarehouseLayoutWindow`, `RackWarehouseFillWindow`, `RackListWindow`, `RackConsolidatedBomWindow`
  y `RackCommandHelpWindow`—, porque las abre un comando de AutoCAD sin ventana padre WPF.
- **Paleta del diagnóstico**: siete archivos pintan su aviso con `Firebrick` en vez del `#B00020` de
  `EditorStatusPalette`.
- **Foco inicial de las cuatro rejillas**, hoy emergente; hacerlo determinista exige que
  `SelectionMatrix` acepte foco, que es un cambio de **control** y no de arquetipo.

**Desviación explícita vigente y medida**: `EditorActionBar` no se adopta en ningún arquetipo. En A y B
el `DockPanel` que las ventanas ya tienen resuelve lo mismo, y en C y D su orden fijo invertiría primaria
y secundaria en tres utilitarias.

I-38 —cálculo resistente, cargas y capacidad— **tampoco se abre** sin instrucción del Owner, y no reabre
[ADR-0017](adr/0017-validacion-cargas-diferida-ram-elements.md).

Lo que quedó **declarado y no resuelto**, para quien retome:

- La ambigüedad de `CantileverPlatePlan.NearOffset`, que se documenta como coordenada del mundo pero que las
  placas de brazo usan como distancia a lo largo del normal. Unificarla toca **cuatro familias de placa**: es
  un cambio de contrato propio, no una corrección suelta.
- La **interferencia física en el cruce de tensores**, que el MVP declara y no calcula.
- El **peso**, diferido desde I-37A.

### I-37D — la última subiniciativa del MVP (histórico del cierre)

I-37A, I-37B e I-37C están integradas: el producto sabe qué es una columna con su base, qué es un brazo y
cómo se compone una estación con su BOM. Lo que todavía **no** existe es lo único que lo haría visible al
usuario, y es exactamente el alcance de I-37D:

la **línea** de estaciones con sus intervalos longitudinales; las **placas de columna** para separadores; los
**separadores**; los **paneles arriostrados** con su distribución vertical; los **tensores** estructurales y
**cold rolled** con sus adaptadores y cartabones; el **BOM completo** por componentes; la **persistencia** y
los **registros del sistema**; las vistas **frontal, lateral y planta**; el **editor** sobre el shell visual
común con la matriz estación × nivel × lado; y la **materialización en AutoCAD** con su comando y su flujo.

**Es la primera de la línea que cambia UI y AutoCAD**, así que su gate NO se resuelve sobre el código: exige
DLL, bundle, `NETLOAD` y la **validación manual del Owner en AutoCAD 2025**. Sin ese veredicto no se integra
y I-37 no se cierra.

**Sigue fuera de alcance incluso al cerrar I-37**: cálculo resistente, cargas, capacidad (son I-38), peso,
costo, optimización, soldaduras, tornillería, anclas, roscas, tolerancias, preparación de extremos, CNC, shop
drawings, la interferencia física en el cruce de tensores, y cualquier catálogo nuevo sin procedencia.

#### I-39D — **INTEGRADA en `main`** (2026-08-07) · **y con ella se CIERRA I-39**

> **Validación manual APROBADA** (`OWNER_APPROVED_I39D_MANUAL_VALIDATION`): **24 de 24 puntos aprobados**,
> agrupados por familia, sin observaciones y sin rondas rechazadas. **ADR-0029 permanece `aceptado` e
> inmutable**: I-39D lo **aplica** a los arquetipos C y D.
> **I-39 queda CERRADA**: sus cuatro subiniciativas están integradas.

| Campo | Valor |
|---|---|
| Rama | `architecture/dialogos-y-utilitarias` — **integrada y eliminada** (local y remota) |
| Merge en `main` | **`fea745c`** · padres `7eb96cb` y `1f24766` · `--no-ff`, sin squash y sin force |
| `CODE_SHA` funcional | `f513e12751a1d5a03a32bd1d50ae345852ff2298` |
| `VALIDATED_BUILD_SHA` | `f513e12751a1d5a03a32bd1d50ae345852ff2298` · DLL Debug `E93653E1…F150D54ED8` |
| Suites | `RackCad.Tests` **2979** · `RackCad.UI.Tests` **765** + **17 omitidas** de evidencia |
| CI del candidato | [`32154543819`](https://github.com/marioap-afk/Calculadora_de_racks/actions/runs/32154543819) ✅ 4/4 sobre `f513e12` · candidato final [`32171970739`](https://github.com/marioap-afk/Calculadora_de_racks/actions/runs/32171970739) ✅ sobre `1f24766` |
| CI de `main` tras integrar | [`32172287044`](https://github.com/marioap-afk/Calculadora_de_racks/actions/runs/32172287044) ✅ 4/4 jobs |
| Base | `origin/main` `7eb96cb` — **no avanzó** desde el reclamo: **sin rebase** |
| Censo final | **28** clases `Window` concretas, **todas producto**: 6 A + 6 B + 10 C + 6 D, cero infraestructura |
| **Validación manual en AutoCAD** | ✅ **APROBADA 2026-08-07** — [checklist](automation/evidence/I-39D-checklist-validacion-manual.md) |
| Auditoría de apertura | [11 lecturas en paralelo](automation/evidence/I-39D-auditoria-dialogos-y-utilitarias.md) — 8 contradicciones resueltas contra el código, 5 afirmaciones corregidas por la caracterización |
| Caracterización | [base vs contrato](automation/evidence/I-39D-caracterizacion-base-vs-contrato.md) — **4** con `Skip` y **2** transcritas |

**Trazabilidad del binario.** El commit de cierre documental `1f24766`, posterior a `f513e12`, **no toca
`src/` ni `tests/`** —verificado con `git diff`—, así que no cambia el binario: la aprobación de
`f513e12` es vigente para el árbol integrado, con el mismo criterio que I-31, I-35 e I-39A/B/C.

#### I-39 — **CERRADA** (2026-08-07) · auditoría transversal sobre el `main` integrado

| Comprobación | Resultado |
|---|---|
| Censo por tipo | **28** clases concretas, cada una con **un solo** arquetipo; `RackDialogWindow` ya no existe ni se nombra |
| Infraestructura `Shell/`, `Controls/`, `Preview/`, `Themes/` | **cero** `using` hacia namespaces de sistema |
| `Editor/` | 7 `using` en `EditorModules.cs`, **excepción declarada** y cubierta por su guarda: es el registro de módulos de I-15 y su trabajo es conocerlos |
| Modelos paralelos | ninguno: una sola fábrica de acciones (`EditorActions`), una sola paleta de estado (`EditorStatusPalette`), un solo chrome por arquetipo |
| `PreviewCanvas` | identificado por **tipo**, con un consumidor productivo; nunca por `x:Name` |
| Cobertura del contrato | Enter, Escape, X, `Alt+F4`, dirty, confirmación de descarte, autoridad y frescura del preview, acción bloqueada con motivo, severidad, foco inicial, tabulación, ownership, tamaño y scroll: **todas** con suites que las cubren |
| Regresión de producto | el diff de I-39 entera contra `fdde6a7` toca **cero** archivos de Plugin, Application, Domain, Catalogs, `assets/`, `deploy/` y `.github/` |
| Incumplimiento residual corregido | el estado versionado de **I-39A** seguía en `integration-ready` pese a estar integrada desde el 7 de agosto; lo corrige esta auditoría |

#### I-39C — **INTEGRADA en `main`** (2026-08-07) · **I-39 sigue abierta**

> **Validación manual APROBADA** (`OWNER_APPROVED_I39C_MANUAL_VALIDATION`): **37 de 37 puntos aprobados**,
> sin observaciones y sin rondas rechazadas. **ADR-0029 ya estaba aceptado** desde I-39A y **no se reabre**:
> I-39C lo **aplica** al arquetipo B y lo cierra entero.
> **I-39 NO se cierra**: queda I-39D.

| Campo | Valor |
|---|---|
| Rama | `architecture/adopcion-editores-acotados` — **integrada y eliminada** (local y remota) |
| Merge en `main` | **`fc9a287`** · padres `da3cd4a` y `b38d653` · `--no-ff`, sin squash y sin force |
| `CODE_SHA` funcional | `2401698a5801a01f3497b3bb27027f801b91960e` |
| `VALIDATED_BUILD_SHA` | `2401698a5801a01f3497b3bb27027f801b91960e` · DLL Debug `29194997…C994749DB` |
| Suites | `RackCad.Tests` **2979** · `RackCad.UI.Tests` **738** + **14 omitidas** de evidencia |
| CI del candidato | [`31223039489`](https://github.com/marioap-afk/Calculadora_de_racks/actions/runs/31223039489) ✅ 4/4 jobs sobre `2401698` |
| CI de `main` tras integrar | [`32145288441`](https://github.com/marioap-afk/Calculadora_de_racks/actions/runs/32145288441) ✅ 4/4 jobs |
| Base | `origin/main` `da3cd4a` — **no avanzó** desde el reclamo: **sin rebase**, y el árbol validado es el integrado |
| Regresiones verificadas en rojo | fachada restaurada (3 guardas), un XAML volviendo a nombrarla (no compila), y el merge incondicional de recursos |
| **Validación manual en AutoCAD** | ✅ **APROBADA 2026-08-07** — [checklist](automation/evidence/I-39C-checklist-validacion-manual.md) |
| Caracterización | [base vs contrato](automation/evidence/I-39C-caracterizacion-base-vs-contrato.md) — **10** cambiaron a propósito y se conservan con `Skip`; **2** son de I-39A |
| Alcance interno resuelto | [decisiones técnicas](automation/evidence/I-39C-decisiones-tecnicas.md) — 4 deudas heredadas + 5 dimensiones del contrato |

**Trazabilidad del binario.** El commit de cierre documental `b38d653`, posterior a `2401698`, **no toca
`src/` ni `tests/`** —verificado con `git diff`—, así que no cambia el binario: la aprobación de `2401698`
es vigente para el árbol integrado, con el mismo criterio que I-31, I-35, I-39A e I-39B.

#### I-39B — **INTEGRADA en `main`** (2026-08-07) · **I-39 sigue abierta**

> **Validación manual APROBADA** (`OWNER_APPROVED_I39B_MANUAL_VALIDATION`): **31 de 31 puntos aprobados**,
> sin observaciones y sin rondas rechazadas. **ADR-0029 ya estaba aceptado** desde I-39A y **no se reabre**:
> I-39B lo **aplica** a los seis editores ricos.
> **I-39 NO se cierra**: quedan I-39C e I-39D.

| Campo | Valor |
|---|---|
| Rama | `architecture/interaccion-editores-ricos` — **integrada y eliminada** (local y remota) |
| Merge en `main` | **`2239eac`** · padres `3853cd4` y `2675e72` · `--no-ff`, sin squash y sin force |
| `CODE_SHA` funcional | `5755845051a5f10bd06367f1f97aed42e180dc9a` |
| `VALIDATED_BUILD_SHA` | `5755845051a5f10bd06367f1f97aed42e180dc9a` · DLL Debug `CF718FE6…27A4CECC` |
| Suites | `RackCad.Tests` **2979** · `RackCad.UI.Tests` **708** + **4 omitidas** de evidencia |
| CI del candidato | [`31214414417`](https://github.com/marioap-afk/Calculadora_de_racks/actions/runs/31214414417) ✅ 4/4 jobs sobre `5755845` |
| CI de `main` tras integrar | [`31218089176`](https://github.com/marioap-afk/Calculadora_de_racks/actions/runs/31218089176) ✅ 4/4 jobs |
| Base | `origin/main` `3853cd4` — la rama estaba **8 detrás y 0 adelante** con intersección vacía: **sin rebase** |
| Regresiones verificadas en rojo | política de cierre, `IsCancel` de Push Back, preview obsoleto del Dinámico, `Insertar` de la Cama, severidades de Cantilever y respaldo de tokens del shell rico |
| **Validación manual en AutoCAD** | ✅ **APROBADA 2026-08-07** — [checklist](automation/evidence/I-39B-checklist-validacion-manual.md) |
| Caracterización | [base vs contrato](automation/evidence/I-39B-caracterizacion-base-vs-contrato.md) — **4 de 12** cambiaron a propósito y se conservan con `Skip` como evidencia versionada |
| Alcance interno resuelto | [decisiones técnicas](automation/evidence/I-39B-decisiones-tecnicas.md) y [auditoría de editores ricos](automation/evidence/I-39B-auditoria-editores-ricos.md) |

**Trazabilidad del binario.** El commit de cierre documental `2675e72`, posterior a `5755845`, **no toca
`src/` ni `tests/`** —verificado con `git diff --stat`—, así que no cambia el binario: la aprobación de
`5755845` es vigente para el árbol integrado, con el mismo criterio que I-31, I-35 e I-39A.

#### I-39A — **INTEGRADA en `main`** (2026-08-07) · **I-39 sigue abierta**

> **Validación manual APROBADA** (`OWNER_APPROVED_I39A_MANUAL_VALIDATION`): ronda 1 parcialmente rechazada
> por **un único defecto** de espaciado, ronda 2 **aprobada** sobre el candidato corregido, sin
> observaciones. **ADR-0029 queda `aceptado`** y es inmutable desde ahora.
> **I-39 NO se cierra**: quedan I-39B, I-39C e I-39D, sin fila ni contrato todavía.

| Campo | Valor |
|---|---|
| Rama | `architecture/contrato-funcional-ventanas-wpf` — **integrada y eliminada** (local y remota) |
| Merge en `main` | **`44f84bd`** · padres `fdde6a7` y `e39ccfb` · `--no-ff`, sin squash y sin force |
| `CODE_SHA` funcional | `16178dfb9c5871a4321d69594a26f67200f28c2f` |
| `VALIDATED_BUILD_SHA` | `16178dfb9c5871a4321d69594a26f67200f28c2f` · DLL Debug `AB6CC4BE…5087CB93` |
| Suites | `RackCad.Tests` **2979** · `RackCad.UI.Tests` **669** |
| CI del candidato | [`31201790868`](https://github.com/marioap-afk/Calculadora_de_racks/actions/runs/31201790868) ✅ 4/4 jobs sobre `e39ccfb` (el candidato validado `16178df` corrió en [`31197679362`](https://github.com/marioap-afk/Calculadora_de_racks/actions/runs/31197679362), también 4/4) |
| Base | `origin/main` `fdde6a7` — **no avanzó** desde el reclamo: **sin rebase**, y el árbol validado es el integrado |
| Regresiones verificadas en rojo | 4 guardas + 2 pruebas del defecto de la ronda 1 |
| **Validación manual en AutoCAD** | ✅ **APROBADA 2026-08-07** — [checklist](automation/evidence/I-39A-checklist-validacion-manual.md) |
| Censo | [evidencia](automation/evidence/I-39A-censo-ventanas.md) — 29 clases `Window`, 28 productivas + 1 de infraestructura |

**Trazabilidad del binario.** El commit de cierre documental posterior a `16178df` **no toca `src/` ni
`tests/`**, así que no cambia el binario: la aprobación de `16178df` es vigente para el árbol integrado,
con el mismo criterio que I-31 e I-35.

#### I-37D — **INTEGRADA en `main`** (2026-08-03) · **y con ella se cierra I-37**

> **Validación manual APROBADA** (`OWNER_APPROVED_I37D_MANUAL_VALIDATION`): todo funciona correctamente,
> **sin defectos bloqueantes observados**. **ADR-0027 y ADR-0028 quedan `aceptados`.**
> Integrada con merge `--no-ff` **`fa7f8c5`**, CI **`30830468566`** verde en sus cuatro jobs.
> **I-37 queda CERRADA**: sus cuatro subiniciativas están integradas.

| Campo | Valor |
|---|---|
| Rama | `feature/cantilever-mvp-final` — **integrada y eliminada** (local y remota) |
| Merge en `main` | **`fa7f8c5`** · padres `250d469` y `a973c7b` · `--no-ff`, sin force |
| CI de `main` | [`30830468566`](https://github.com/marioap-afk/Calculadora_de_racks/actions/runs/30830468566) ✅ 4/4 jobs |
| `CODE_SHA` funcional | `dd9e4a5` |
| `VALIDATED_BUILD_SHA` | **ninguno para esta ronda** — ver la nota de trazabilidad abajo |
| Suites | `RackCad.Tests` **2978** · `RackCad.UI.Tests` **621** |
| Regresiones | **14/14 en rojo** ([evidencia](automation/evidence/I-37D-round-4-regressions.md)) |
| CI | [`30674507385`](https://github.com/marioap-afk/Calculadora_de_racks/actions/runs/30674507385) ✅ sobre `4e4e6d9` |
| Bundle | Release verificado, 153 comprobaciones, cero DLL de Autodesk |
| **Validación manual en AutoCAD** | ✅ **APROBADA 2026-08-03** — [paquete de la ronda 4](automation/evidence/I-37D-autocad-validation-advanced-panels-and-adapter.md) |
| `VALIDATED_BUILD_SHA` | `a594eb5` · DLL Debug `F237CC79…8985B6E1` |

**Qué cambió, en corto.** El **adaptador** de tensor cold rolled dejó de dibujarse como una L construida a
mano y es un **prisma estructural real** de `AISC-L-L2X2X3_16`, proyectado por la misma tubería que columnas
y separadores. Sus **dos agujeros** están centrados **cada uno en su propia ala**, en el plano medio real de
esa ala: la separación mide `(0.820358, −0.906250, 0.385099)`, módulo `1.281631 in`, y **ΔY ya no es cero**.
Queda revocada `RodHoleAxialOffset = CutLength / 2`.

Como el **centro del agujero de varilla es el datum físico** del extremo del tensor, la longitud nominal pasa
de `92.131526` a `92.319026 in` — **crece 0.1875 in, exactamente el espesor del ángulo**, porque la
aproximación medía hasta la cara del ala y la física mide hasta su plano medio. **El BOM cambia con ella**, y
es legítimo (decisión 14.5): las cantidades no se mueven, sólo la longitud que se ordena.

Y la **secuencia vertical de paneles** admite modo **avanzado**: `PanelLayoutMode`, segmentos declarados
tramo a tramo, y una **lista efectiva única** que es la sola entrada del resolver en los dos modos. De ella
salen los **separadores por fronteras únicas** y los **tensores por segmentos `CrossBraced`**. El modo
automático **no cambió el producto**: línea, BOM y las seis vistas siguen en su pin.

**La vista «Sección del adaptador»** (decisión 14.7) ya está implementada. El adaptador no se lee como una L
en ninguna de las tres vistas de la línea —su eje de corte corre dentro del plano del panel— y el dueño
decidió **no deformar ninguna vista** para disimularlo, sino darle al configurador de tensor una vista propia
que mire por ese eje. Su cámara sale del **marco de la pieza**, y consume la **misma**
`StructuralSectionGeometry` que el prisma: una prueba compara el **área** del contorno proyectado contra la
del catálogo, que es independiente de la cámara, así que una geometría paralela no pasaría desapercibida.
Las tres vistas de la línea **no se tocaron**, y hay prueba sobre sus cámaras.

**El trabajo se movió a `D:`.** `C:` llegó al 100 % dos veces en esta iniciativa y una de ellas truncó un
archivo fuente. El worktree vive ahora en `D:/Documentos/Codex/worktrees/feature-cantilever-mvp-final`, y
`TEMP`, `TMP`, `NUGET_PACKAGES` y `DOTNET_CLI_HOME` apuntan a `D:`. `global.json` **no se tocó**. La puerta de
espacio que se fijó en la ronda anterior queda **sin efecto**: era un parche a un problema que ya no existe.

### I-37C — INTEGRADA en `main` (2026-07-29) — **la estación, y el primer BOM de Cantilever**

I-37C tampoco dibuja: el usuario **no ve nada nuevo**. Lo que cambia es que el producto sabe componer una
estación completa y **cotizarla**.

| Campo | Valor |
|---|---|
| Rama (eliminada tras integrar) | `architecture/cantilever-estacion-bom` |
| `Claim-Id` | `ef8cf6ce-326d-4562-b277-ed7a3404e148` |
| **SHA técnico aprobado** por el Owner y por CI | `e1c3cab24d16ea0a6565fc43a81dcc0f2e31c694` (CI **30510202275**, success) |
| SHA final de rama | vive en `git log`; su delta contra el aprobado es **solo documentación de cierre** |
| **Validación en AutoCAD** | **NO APLICA** (`requires_autocad: false`, `requires_owner_validation: false`) |
| Veredictos normativos | `OWNER_APPROVED_ADR_0026` y `OWNER_AUTHORIZED_INTEGRATION_I_37C` |
| Bundle | **no ejecutado y no exigible**: el diff no toca `assets/`, catálogos, `deploy/` ni `.github/` |
| Diff contra la base `0610adb` | **31** archivos: docs 8, Domain 3, Application 13, tests 7 |

**Qué entregó.** El diseño editable en Domain —modo de cara, altura, nivel y los dos **templates** sin
autoridades duplicadas— y en Application: la **retícula regular** extraída como autoridad única,
las **métricas de conexión** compartidas con I-37B, la autoridad de **espejo** de base lateral, el
**layout de niveles** sin tope de candidatos, el **resolver de estación** con su secuencia explícita de once
pasos y su pase final verificado, la **matriz pura** de brazos y el **BOM por componentes**.

**Seis defectos propios, encontrados en revisión y corregidos en la misma rama** antes de integrar: un tope
funcional de 250 candidatos que bloqueaba una configuración válida; **dos** autoridades midiendo la conexión,
con `Math.Max(2, count)` y `offset ?? 0` que normalizaban entradas inválidas; un pase final que no comparaba
los bordes del **cuerpo**, que es lo que el claro mide; overrides iguales al default que se persistían y
hacían que la celda dejara de seguirlo; un `ProfileId` genérico que fusionaba placas físicamente distintas; y
placas medidas con una caja del mundo, que encogía una tapa inclinada. **Veintitrés** regresiones verificadas
en rojo entre las dos rondas, y **dos caracterizaciones previas** para las extracciones.

### I-37B — INTEGRADA en `main` (2026-07-29) — **el brazo, y el primer consumidor de I-37A**

### I-37B — INTEGRADA en `main` (2026-07-29) — **el brazo, y el primer consumidor de I-37A**

I-37B **no dibuja**, igual que I-37A: el usuario **no ve nada nuevo**. Lo que cambia es que el producto sabe
qué es un brazo de cantilever, cómo se compone con uno o dos perfiles y cómo se atornilla a la columna
usando los agujeros que la columna ya tenía.

| Campo | Valor |
|---|---|
| Rama (eliminada tras integrar) | `architecture/cantilever-brazo` |
| `Claim-Id` | `57727fe5-46b6-4b51-83a9-85afa0d4ebf9` |
| **SHA técnico aprobado** por el Owner y por CI | `00d8126eb687a46bafc156480ea6f080f295a771` (CI **30499888210**, success) |
| SHA final de rama | vive en `git log`; su delta contra el aprobado es **solo documentación de cierre** |
| **Validación en AutoCAD** | **NO APLICA** (`requires_autocad: false`, `requires_owner_validation: false`): no cambia dibujo ni interfaz |
| Veredictos normativos | `OWNER_APPROVED_ADR_0025_WITH_CURRENT_DATUM` y `OWNER_AUTHORIZED_INTEGRATION_I_37B` |
| Bundle | **no ejecutado y no exigible**: el diff no toca `assets/`, catálogos, `deploy/` ni `.github/` |
| Diff contra la base `e0f319f` | **23** archivos del candidato: docs 8, Domain 2, Application 11, tests 2 |

**Qué entregó.** El diseño editable en `RackCad.Domain.Systems.Cantilever` —cuerpo, placa de conexión y
placa final, con el id de sección como **texto**— y la resolución en
`RackCad.Application.Systems.Cantilever`: `CantileverArmSectionPolicy` (elegibilidad por
`StructuralSectionId` **+** `Arrangement`, inyectable y sin ids de producción),
`CantileverArmBodyArrangementResolver` como autoridad de arreglos, `CantileverArmFrameResolver` como
**única autoridad de marcos** del brazo, `CantileverArmColumnConnectionPattern` —que **selecciona** y
**observa**, nunca crea—, `CantileverArmBodyPlan` y `CantileverArmAssembly` con firma determinista. Los
**tres arreglos** entraron desde el principio; la **pendiente** vale en los dos lados con el extremo libre
subiendo en ambos; **tapa y tope** son modos de una misma placa; y el **datum aprobado** deja la
aproximación de **intrusión y holgura** declarada, medida y reportada por separado.

**Cuatro defectos propios, encontrados por el Owner y corregidos en la misma rama** antes de integrar: un
`switch` de modo de placa final **no exhaustivo** que materializaba como tapa un valor no declarado; un
**desborde de `int`** en el rango del índice de troquel, que envolvía a negativo y reventaba al leer el
pitch; una **pendiente que colapsa el marco** —`atan` converge en 90° — que salía como excepción en vez de
diagnóstico; y una afirmación **falsa** de que placa y cuerpo «no se traslapan». Las cuatro tienen
regresión verificada **en rojo** y revertida.

### I-37A — INTEGRADA en `main` (2026-07-29) — **el primer miembro sobre el catálogo neutral**

**Lo primero, porque delimita todo lo demás:** I-37A **no dibuja**. No hay vistas, preview, editor,
persistencia de proyecto, `RackSystemKind`, registros ni una línea de Plugin. El usuario **no ve nada
nuevo** al integrarla; lo que cambia es que el producto sabe qué es una columna, qué es una base y cómo se
conectan.

| Campo | Valor |
|---|---|
| Rama (eliminada tras integrar) | `architecture/cantilever-base-columna` |
| `Claim-Id` | `af0b650d-8a88-48c3-a328-b0b05c3bb61f` |
| **SHA técnico aprobado** por el Owner y por CI | `15523679e655364c146917ece338c7cecbe24023` (CI **30488839172**, success) |
| SHA final de rama | vive en `git log`; su delta contra el aprobado es **solo documentación de cierre** |
| **Validación en AutoCAD** | **NO APLICA** (`requires_autocad: false`, `requires_owner_validation: false`): no cambia dibujo ni interfaz |
| Veredictos normativos | `OWNER_APPROVED_ADR_0024` y `OWNER_AUTHORIZED_INTEGRATION_I_37A` |
| Bundle | **no ejecutado y no exigible**: el diff no toca `assets/`, catálogos, `deploy/` ni `.github/` |
| Diff contra la base `3c6ccf5` | **34** archivos: docs 10, Domain 7, Application 14, tests 3 |

**Qué entregó.** Contratos editables en `RackCad.Domain.Systems.Cantilever` (columna, base, conexión,
troqueles, tres placas independientes y el cartabón) y la resolución en
`RackCad.Application.Systems.Cantilever`: `CantileverSectionResolver` como **único** límite de parseo y
lookup, `CantileverColumnBaseSectionPolicy` inyectable por ids exactos,
`CantileverColumnBaseFrameResolver` como **única autoridad de marcos** —lee la orientación registrada en la
variante y falla cerrado ante un enum no declarado—, `CantileverColumnBaseConnectionPattern` como
**autoridad compartida** del patrón de agujeros, y `CantileverColumnBaseAssembly` como subensamble
inmutable con firma determinista.

**El datum, que el encargo no fijaba y hubo que declarar.** `y = 0` es el plano de contacto entre la cara
de conexión de la columna y la placa posterior; `z = 0` es el **fondo común** de la sección de la columna y
de la base; `x = 0` es el centro transversal de la columna. El de `z` es el consecuente: toda elevación de
conexión se mide desde el fondo de la base y la consume la columna, así que con dos orígenes verticales el
patrón compartido no querría decir nada. Queda escrito en
[`automation/state/I-37A.yml`](automation/state/I-37A.yml) para que se pueda corregir con una línea.

**Un hecho del preflight que conviene no perder:** en el repositorio **no hay imágenes de referencia**
—solo iconos de paquetes NuGet—. El patrón de pares simétricos de la placa inferior (`±p/2`, `±3p/2`, …,
sin troquel en el centro) está implementado tal como lo describe el encargo y cubierto por prueba, pero
**no se pudo contrastar contra una pieza real**. Si una lo contradice, el cambio es una función y su prueba.

**Ronda de correcciones del Owner sobre el primer candidato.** Cuatro defectos, corregidos en la misma
rama: la igualdad de `CantileverPunchDatum` no era válida para un value type (`Equals` delegaba en la
comparación tolerante mientras el hash redondeaba, y la tolerancia no es transitiva) ⇒ `Equals` exacto y
consistente con el hash, con `ApproxEquals` separado como el método geométrico; la orientación registrada
era dato que nadie leía ⇒ nace la autoridad de marcos, con la geometría vigente **numéricamente
idéntica**; faltaba validar que los troqueles **caben** ⇒ ocho validaciones bloqueantes con **tres códigos
nuevos** y uno retirado de un uso engañoso; y el conteo de archivos de la evidencia estaba mal medido ⇒
corregido con su causa escrita.

**Siguiente paso real.** La **definición física y el contrato de la siguiente subiniciativa de I-37**
—brazos y la estación completa—, que **no está reclamada** y que necesita del Owner la anatomía real de la
pieza antes de abrir rama. I-37 permanece como **iniciativa paraguas partida**; ninguna otra subiniciativa
está registrada ni reclamada.

### I-36D — INTEGRADA en `main` (2026-07-28) — **la primera geometría que RackCad firma**

**Lo primero, porque es lo que puede malinterpretarse:** las dimensiones, el área, el peso y las
propiedades de las 28 secciones S son **de AISC y se copian tal cual**. Lo que RackCad aporta —y
**declara**— es únicamente cómo se ven: la inclinación del patín, el filete y la punta. Es la primera
vez que una geometría del producto no es trazable punto por punto a un dato publicado, y por eso lleva
advertencia obligatoria.

| Campo | Valor |
|---|---|
| Rama (eliminada tras integrar) | `feature/perfiles-aisc-s` |
| `Claim-Id` | `964effe9-9e1a-4861-ac34-594b04da48c7` |
| **SHA técnico validado** por el Owner | `3ffe4dff3ac623dcb53fc715ebc5b81ed6bcde68` (CI **30410876362**, 4/4) |
| SHA final de rama | vive en `git log`; su delta contra el validado es **solo documentación** |
| **Validación en AutoCAD** | **APLICA y está APROBADA**, sin observaciones (`requires_autocad: satisfied`, `requires_owner_validation: satisfied`); veredicto `OWNER_APPROVED_ADR_0023` |
| DLL Debug validado | `6A88D9FEB097B5052429D2DF2660EC28992598F2616CCFD587840A44289DC3B7` (121 856 bytes) |
| Catálogo | **1 011** secciones; `S` = **28**, retirada de `excludedTypeCounts`; los cuatro CSV previos **byte-idénticos** |
| ADR | **ADR-0023 ACEPTADO** el 2026-07-28. No reemplaza a ADR-0020, 0021 ni 0022: los **extiende** |
| Suites al integrar | **2094** `RackCad.Tests` + **544** `RackCad.UI.Tests`, cero fallos, cero omitidas |
| Builds Debug | Application, UI y Plugin sin errores propios (2 `MSB3277` conocidas) |
| Bundle | **153 comprobaciones**; harness 10/10 |
| Rebase | **no-op**: `origin/main` no avanzó desde la base `202e456` |
| **MERGE_SHA** | vive en `git log --first-parent main`; este documento **no lo inventa** |
| Limpieza | rama local, rama remota y worktree **eliminados** tras confirmar el merge |

**Por qué S no se colapsó en W.** Los datos encajan —AISC tabula ambas con las mismas columnas— y
precisamente por eso era peligroso: el id se forma `namespace-token-designación`, así que `S24X121`
habría recibido **`AISC-W-S24X121`**, y el constructor de W la habría dibujado con patines paralelos
rotulada `TabulatedComplete`, la degradación silenciosa que ADR-0022 prohíbe.

**Dos gotchas que conviene no volver a descubrir.** El primero: el id real es `AISC-S-S10X25_4`, con
guion bajo — `StructuralSectionDesignationNormalizer` convierte el punto por ADR-0021 y cambiarlo habría
roto los 525 ids de HSS ya guardados. El segundo: **todo CSV generado necesita su línea `-text` en
`.gitattributes`**, o un clon con `core.autocrlf=true` le cambia los saltos de línea y su SHA-256 deja de
coincidir con el manifiesto; hay una guarda nueva que lo comprueba para todos los archivos generados.

**Pendientes preservados, sin implementar y sin rama abierta:** la mejora visual de **C y los demás
laminados** (I-36D cubrió sólo S), radios y chaflanes comerciales —que siguen **sin regla acreditada**,
porque la fuente no publica ninguno—, `bf/2tf` y `h/tw`, I-37 Cantilever, miembros estructurales,
materiales, conexiones y fabricación, cálculo estructural, sólidos 3D, round-trip de perfiles
independientes y familias adicionales. **Ninguno invalida lo implementado.**

**Siguiente iniciativa habilitada: I-37 — Cantilever MVP**, que **no** se implementó ni se abrió en esta
sesión.

### I-36C — INTEGRADA en `main` (2026-07-28) — **acceso, no funcionalidad nueva**

**Lo primero, porque es lo que se malinterpreta:** el catálogo y el generador paramétrico de perfiles
estructurales **ya estaban implementados** por I-36A e I-36B. I-36C **únicamente** añadió el acceso
visible **«Generar perfil estructural»** en el menú principal `RACKCAD`, reutilizando **exactamente el
mismo flujo** de `RACKSECCION`. **No hay un segundo generador.**

El defecto era de **descubribilidad**: la capacidad existía, estaba validada, y la única forma de
invocarla era escribir el comando.

| Campo | Valor |
|---|---|
| Rama (eliminada tras integrar) | `fix/acceso-menu-secciones-estructurales` |
| `Claim-Id` | `3afd368a-0eb4-44aa-8c8f-ebde72ed256f` |
| **SHA técnico validado** por el Owner | `86867e62bba9c52bd0855719b1f51ba99c3edcaa` (CI **30386035953**, 4/4) |
| SHA final de rama | vive en `git log`; su delta contra el validado es **solo documentación** |
| **Validación en AutoCAD** | **APLICA y está APROBADA**, sin observaciones (`requires_autocad: satisfied`, `requires_owner_validation: satisfied`) |
| Botón | «Generar perfil estructural», entre «Diseñar larguero» y «Abrir de la biblioteca de diseños», con el estilo `MenuButton` vigente |
| Acción | `MainMenuAction.GenerateStructuralSection` — **no** un `RackInsertionRequest` |
| Autoridad compartida | `StructuralSectionCommandFlow.Run(document)`, que consumen el botón y `RACKSECCION` |
| Suites al integrar | **2071** `RackCad.Tests` + **534** `RackCad.UI.Tests`, cero fallos, cero omitidas |
| Builds Debug | Application, UI y Plugin sin errores propios |
| Bundle | **147 comprobaciones** |
| Rebase | **no-op**: `origin/main` no avanzó desde la base `14317a5` |
| **MERGE_SHA** | vive en `git log --first-parent main`; este documento **no lo inventa** |
| Limpieza | rama local, rama remota y worktree **eliminados** tras confirmar el merge |

**Los siete puntos que el Owner aprobó:** botón visible; posición y estilo; cancelación del inspector;
cancelación del punto de inserción; inserción de `W12X26`; **equivalencia con `RACKSECCION`**; y
ausencia de regresiones en los sistemas existentes. Cero observaciones, cero bloqueos.

**Por qué la acción no es una inserción de rack.** El menú ya lleva un `RackInsertionRequest` tipado
para los seis sistemas que sabe diseñar (I-15). Una sección **no es un rack**: no tiene
`RackSystemKind` sobre el que despachar, no tiene payload de diseño que embeber y no tiene round-trip.
Un request con un `Kind` inventado empujaría esa mentira hasta el `switch` del host. El Plugin lee la
acción **después** de que la ventana modal se cierre, porque el flujo pide un punto y el editor de
AutoCAD tiene que estar libre.

**Cero duplicación, comprobada.** Cada pieza del caso de uso —carga fail-closed del catálogo, inspector,
inserción, aviso de unidades, peso, fidelidad— la menciona **exactamente un** archivo del Plugin, y 25
guardas de fuente lo fijan. Siete guardas de I-36B se reapuntaron al archivo del flujo: lo que fijan no
cambió, cambió dónde vive.

**Lo que NO cambió, verificado con `git diff`:** `assets/**` sin una línea —`secciones.csv`,
`blocks.csv` y el manifiesto de I-36A intactos—; `blocks-library.dwg` sin tocar; `src/RackCad.Domain`,
`deploy/` y `.github/` con **cero** archivos; y **cero archivos de geometría**. Los seis sistemas y la
biblioteca conservan título, orden y handler.

**Pendientes preservados, sin implementar y sin rama abierta:** perfiles IPS/S y su verificación frente a
la familia AISC `S` o al catálogo comercial; geometría visual mejorada de laminados; conicidad de
patines; radios y chaflanes acreditados; separación entre geometría tabulada y visual aproximada; I-37
Cantilever; miembros estructurales; materiales, conexiones y fabricación; cálculo estructural; sólidos
3D; round-trip de perfiles independientes; y familias adicionales. **Ninguno invalida lo implementado.**

**Siguiente iniciativa habilitada: I-37 — Cantilever MVP**, que **no** se implementó en esta sesión.

### I-36B — INTEGRADA en `main` (2026-07-28) — **segunda de la Fase 6**

El Owner **aprobó el gate `owner-validation`** tras ejecutar en AutoCAD el **smoke focalizado de cinco
puntos** y el **checklist completo de doce**, sin bloqueos. La iniciativa queda **integrada**; no queda
acción pendiente de I-36B.

Convierte las **983** secciones de I-36A en **geometría dibujable**, generada **en código** desde sus
dimensiones: nada de un bloque por designación. Aporta primitivas 2D/3D aditivas, constructores por
familia en dos niveles de detalle con **fidelidad declarada**, la **instancia prismática** donde vive la
longitud —la sección no la tiene—, cuatro vistas más una personalizada, **un único plan neutral** que
consumen igual el preview WPF y AutoCAD, un inspector mínimo y el comando **`RACKSECCION`**.

| Campo | Valor |
|---|---|
| Rama (eliminada tras integrar) | `architecture/geometria-secciones-estructurales` |
| **SHA técnico aprobado** por el Owner | `30ef95c56c9ce6d3120e13c29f971c40dd65fbec` (CI **30378134540**, 4/4) |
| SHA final de rama | vive en `git log`; su delta contra el aprobado es **solo documentación** |
| **Validación en AutoCAD** | **APLICA y está APROBADA** (`requires_autocad: satisfied`, `requires_owner_validation: satisfied`) |
| Geometría | **983 / 983** en los dos niveles de detalle, sin excepción; **289** `TabulatedComplete` (W), **694** `TabulatedDerived` (HSS, C, L), **cero** degradadas |
| Error de área medido | W 0.732 % máx · HSS 10.927 % · C 5.545 % · L 3.012 %. El del HSS es **diferencia de definición** (AISC usa `tdes`, la geometría dibuja `tnom`; con `tdes` cae a 4.581 % máx y cero filas sobre el 5 %) |
| Suites al integrar | **2043** `RackCad.Tests` + **523** `RackCad.UI.Tests`, cero fallos, cero omitidas |
| Builds Debug | Application, UI y Plugin sin errores propios (2 `MSB3277` conocidas en Plugin) |
| Bundle | **147 comprobaciones**, DLL y catálogos idénticos, cero DLL de Autodesk |
| ADR | **ADR-0022 ACEPTADO** el 2026-07-28 por Mario Pérez, Owner. No reabre ADR-0020 ni ADR-0021 |
| Rebase | **no necesario**: `origin/main` no avanzó desde la base `eafb785`. Rebase final **no-op** |
| **MERGE_SHA** | vive en `git log --first-parent main`; este documento **no lo inventa** |
| Limpieza | rama local, rama remota y worktree **eliminados** tras confirmar el merge |

**Dos rondas de corrección antes de aprobar.** La primera validación rechazó parcialmente el gate: las
generatrices salían solo del contorno exterior, así que un **HSS visto de lado se dibujaba macizo**; y un
perfil proyectado exactamente a lo largo de X o Y **colapsa a una recta** y se seguía emitiendo como
polilínea **cerrada** —en AutoCAD, área cero recorrida dos veces e imposible de seleccionar—. Ambos están
corregidos, con la invariante viviendo en el **tipo**: `SectionPlanCurve` rechaza una curva cerrada
unidimensional, y el adaptador copia `IsClosed` tal cual, así que no queda camino al dibujo.

**Los canales C no se ven como los de una librería CAD comercial, y está aceptado.** Al compararlos, el
Owner constató que falta la **conicidad de los patines**, los **redondeos de punta** y las **transiciones
del laminado**, y que esa diferencia es la que explica su error de área. Aceptó sin bloquear: los canales
son **`TabulatedDerived`**, la geometría es honesta sobre lo que puede afirmar y **no se inventan** esas
dimensiones aquí. **No son geométricamente idénticos a un perfil comercial**, y la documentación no lo
afirma en ningún punto.

**Requisito futuro OBLIGATORIO registrado: perfiles IPS/S y geometría visual mejorada de perfiles
laminados.** Una iniciativa futura y separada deberá incorporar IPS —verificando su correspondencia con
la familia AISC `S` o con el catálogo comercial de la empresa—, importar su fuente, modelar la
inclinación de los patines, representar radios y chaflanes **cuando exista una regla acreditada**, y
mejorar visualmente C y los demás laminados; manteniendo **separadas** la geometría tabulada y la visual,
**declarando** cuándo una representación visual es aproximada, y **sin sustituir ni alterar** la
geometría tabulada de I-36B. No se mezcla con Cantilever salvo que su contrato lo exija. **No se abrió**
rama, contrato ni worktree para ella. Registrado en `docs/ideas-futuras.md`, en la decisión versionada de
I-36B, en su evidencia, en su estado, en la guía y en ADR-0022.

**Lo que NO cambió, verificado con `git diff`:** `assets/catalogs/**` sin una línea —`secciones.csv`,
`blocks.csv` y el manifiesto de I-36A intactos—; `blocks-library.dwg` sin tocar; `src/RackCad.Domain`,
`deploy/` y `.github/` con **cero** archivos. En UI y Plugin **todo lo aportado son archivos nuevos**: no
se modificó ni un archivo de los sistemas vigentes. El único archivo preexistente tocado fuera de `docs/`
es `README.md`, porque WORKFLOW §8 obliga a registrar allí un comando de AutoCAD nuevo en la misma rama.

**Siguiente iniciativa habilitada: I-37 — Cantilever MVP**, que **no** se implementó ni se abrió en esta
sesión.

### I-36A — INTEGRADA en `main` (2026-07-28) — **abre la Fase 6**

El Owner **aprobó el gate `owner-validation` y sus siete puntos**. La iniciativa queda **integrada**; no
queda acción pendiente de I-36A.

Funda el **catálogo neutral de secciones estructurales**: la sección transversal deja de ser lo mismo
que el miembro y que la pieza comercial. Importa **983** secciones de la AISC Shapes Database v16.0 —W
289, HSS rectangular/cuadrado 525, C 32, L 137— con un importador reproducible fuera del producto y un
lector CSV **estricto** propio. **No dibuja, no migra y no toca ningún sistema vigente.**

| Campo | Valor |
|---|---|
| Rama (eliminada tras integrar) | `architecture/catalogo-secciones-estructurales` |
| **CODE_SHA aprobado** por el Owner | `5cd526cca252ffcd30dc0e598c8e3049632ea4ec` |
| CI del SHA aprobado | **30354958938**, 4/4 |
| SHA final de rama | `c899a367fa76cd16218f364d438bc3491908cd2e` (CI **30363426285**, 4/4) — su delta contra el aprobado es **solo documentación** |
| **Validación en AutoCAD** | **NO APLICA** por decisión expresa del Owner: no cambia dibujo, bloques, comandos ni comportamiento visible (`requires_autocad: false`) |
| Conteos | W **289**, HSS-RECT **525** (126 cuadrados), C **32**, L **137**, total **983**; excluidos 1 316; **filas seleccionadas rechazadas 0** |
| Fuente | AISC Shapes Database v16.0, SHA-256 `82D0CEB96A0D938AE1A6BD9637CB10A1E269225B5D668DCE5B0BDC8D86013496`; el libro **no se versiona** |
| Suites al integrar | **1851** `RackCad.Tests` + **494** `RackCad.UI.Tests`, cero fallos, cero omitidas |
| Builds Debug | UI 0 errores / 0 advertencias; Plugin 0 errores propios y 2 `MSB3277` conocidas |
| Bundle | 147 comprobaciones; los **siete** archivos nuevos dentro, con hashes idénticos a `assets/catalogs` |
| ADRs | **ADR-0020** aceptado (reemplaza a ADR-0008 **solo en autoridad conceptual**); **ADR-0021** aceptado el 2026-07-28 (**no** reemplaza ADR-0005) |
| Rebase | **no necesario**: `origin/main` no avanzó desde la base `a35374f`; verificado con `git rebase origin/main` → *up to date* |
| **MERGE_SHA** | vive en `git log --first-parent main`; este documento **no lo inventa** (se escribe antes del merge) |
| Limpieza | rama local, rama remota y worktree **eliminados** tras confirmar el merge |

**Lo que NO cambió, verificado con `git diff`:** `assets/catalogs/secciones.csv` byte-idéntico y los
diez catálogos vigentes sin una línea; `blocks.csv` y `blocks-library.dwg` intactos; `src/RackCad.Domain`,
`src/RackCad.UI`, `src/RackCad.Plugin`, `deploy/` y `.github/` con **cero archivos cambiados**. El único
cambio en Application fuera del namespace nuevo es que `CsvCatalogReader` **delega** su parser léxico en
`CsvLexer`, con el comportamiento tolerante histórico intacto y fijado por regresiones.

**Siguiente iniciativa habilitada: I-36B** — geometría y representación prismática, rama
`architecture/geometria-secciones-estructurales`, **reservada y todavía sin crear**. Se reclama en una
iniciativa separada; I-36A no la abrió.

### I-23 — INTEGRADA en `main` (2026-07-27) — **cierra la Fase 5**

El Owner **aprobó el smoke mínimo en AutoCAD 2025** sobre el DLL Debug del SHA exacto del candidato. La
iniciativa queda **integrada**; no queda acción pendiente de I-23.

| Campo | Valor |
|---|---|
| Rama (eliminada tras integrar) | `refactor/namespaces-sistemas` |
| **CODE_SHA / BUILD_SHA aprobado** por el Owner | `5d49a6cc990c5fc72e321aea37dd5bc2d3d4a128` (11 ahead / 0 behind) |
| CI del candidato | **30304742946**, 4/4 sobre `5d49a6c` |
| **DLL Debug** validado | `AssemblyInformationalVersion = 1.0.0+5d49a6cc990c5fc72e321aea37dd5bc2d3d4a128`, SHA-256 `D2944E25C20098CD57AA15DA143EB2C7412710ED61A78BE548B8CD87146D43EE` |
| Smoke aprobado | `NETLOAD`, `RACKCAD`, un editor de sistema y `RACKCABECERA`, sin errores de carga, comandos, XAML ni recursos ([registro](initiatives/I-23-autocad-smoke.md)) |
| Suites al integrar | **1619** `RackCad.Tests` + **494** `RackCad.UI.Tests`, cero fallos, cero omitidas |
| Builds Debug | UI 0 errores / 0 advertencias; Plugin 0 errores propios y 2 `MSB3277` conocidas, con AutoCAD cerrado |
| Rebase | **no necesario**: `origin/main` no avanzó desde la base `b43b5d1` |
| **MERGE_SHA** | vive en `git log --first-parent main`; este documento **no lo inventa** (se escribe antes del merge) |
| Limpieza | rama local, rama remota y worktree **eliminados** tras la CI post-merge verde |

Qué dejó integrado: los **namespaces finales por sistema** en los cuatro proyectos de producto, con
`Drawing` para la materialización, `RackFrames` para la cabecera física, el renombre fósil
`DynamicSystemPlan` a `HeaderRunPlan` y las guardas que impiden que la separación vuelva a aplanarse. El
detalle está en §1 y el contrato en
[`initiatives/I-23-namespaces-sistemas.md`](initiatives/I-23-namespaces-sistemas.md).

**Con I-23 se cierra la Fase 5.** Lo siguiente del plan es **I-25 — `feature/guardas-traseras`**, que
sigue en **backlog diferido** (ni completada ni descartada) y que ya no está bloqueada por el estorbo de
I-23. **Push Back v1 queda estable.** La **congelación funcional de I-23 termina al integrar**: lo que
queda vigente es la regla namespace-carpeta, que las guardas comprueban en cada CI.

### I-35 — INTEGRADA en `main` (2026-07-27)

El Owner **aprobó explícitamente** la validación manual en AutoCAD 2025 del candidato técnico. La
iniciativa queda **integrada**; no queda acción pendiente de I-35.

| Campo | Valor |
|---|---|
| Rama (eliminada tras integrar) | `feature/editor-avanzado-push-back` |
| **CODE_SHA / BUILD_SHA aprobado** por el Owner | `f2be30c20a7ff8958a24ddf078a5310dab5dbfe0` |
| CI del candidato | **30293536290**, 4/4 sobre `f2be30c` |
| **DLL Debug** validado | `AssemblyInformationalVersion = 1.0.0+f2be30c20a7ff8958a24ddf078a5310dab5dbfe0`, SHA-256 `4FE530EFA0FFAEF005B20253A1C0F68BF99D321A82766D4FF559A3367E99C101` |
| Punta de la rama antes del cierre documental | `ec52b678e7058f556e49b46cab4b0f38967e50d4` (CI **30293863850**, 4/4) — delta contra el candidato: **solo** `docs/automation/state/I-35.yml`, sin cambio de binario |
| Suites al integrar | **1612** `RackCad.Tests` + **491** `RackCad.UI.Tests`, cero fallos, cero omitidas |
| Builds Debug | UI 0 errores / 0 advertencias; Plugin 0 errores propios y 2 `MSB3277` conocidas, con AutoCAD cerrado |
| Rebase | **no necesario**: `origin/main` no avanzó desde la base `52ce27f` |
| **MERGE_SHA** | vive en `git log --first-parent main`; este documento **no lo inventa** (se escribe antes del merge) |
| Limpieza | rama local, rama remota y worktree **eliminados** tras la CI post-merge verde |

Qué dejó integrado: el **editor avanzado de módulos de Push Back** (PB-011) — edición longitudinal de
cabeceras y separadores, configuración transaccional, altura manual de cabecera, refuerzo total o parcial
del poste derivado, cantidad y separación globales de separadores, y restauración individual y global. El
detalle funcional completo está en §1 y el contrato en
[`initiatives/I-35-editor-avanzado-push-back.md`](initiatives/I-35-editor-avanzado-push-back.md).

**Deuda que hereda quien siga:** ninguna nueva. Las que ya existían —la decisión del Owner sobre
`DesviadorCellsAreByPost`, la **defensa** que I-34 dejó fuera y el **preview visual** diferido por I-18—
siguen abiertas y **no** las toca I-35.

### I-34 — INTEGRADA en `main` (2026-07-27)

El Owner **aprobó toda la validación manual en AutoCAD 2025**. La iniciativa queda **integrada**; no
queda acción pendiente de I-34.

| Campo | Valor |
|---|---|
| Rama (eliminada tras integrar) | `feature/edicion-masiva-seguridad` |
| **CODE_SHA / BUILD_SHA aprobado** por el Owner | `dbdda74860052c481998da8b63383cf68ec499cc` |
| CI del candidato | **30283957763**, 4/4 sobre `dbdda74` |
| Base | `origin/main` `7e48b5c06afb790621d4997aa93e44e0b53e8af7` — **no avanzó** desde el reclamo, así que **no hubo rebase** y el árbol que el Owner validó es exactamente el integrado (WORKFLOW §6) |
| **DLL Debug** verificado en el cierre | `AssemblyInformationalVersion = 1.0.0+dbdda74860052c481998da8b63383cf68ec499cc`, SHA-256 `5353C298B5B099BA9DEDAA42C2252DD6891952C7FE83EFD4C0261E4B82796E39` — **bit a bit el aprobado** |
| Suites en el cierre | `RackCad.Tests` 1522, `RackCad.UI.Tests` 469 (+71 sobre la base) |

**Qué aprobó el Owner**: desviador, tope y **parrilla** del Selectivo; desviador y guía del Dinámico;
desviador y **tope posterior** de Push Back; los alcances Celda / Nivel / Frente-o-Poste / Todo; matrices
dentadas y celdas ausentes; los **contadores vivos de parrilla**; dibujo, BOM, persistencia, reapertura,
actualización y GUID; el incremento de altura de los diálogos; `Desactivar` como estado inicial; y la
regresión compartida de los demás diálogos.

**Qué entregó**: una fundación pura sobre `SelectionMatrixModel` (celda primaria transitoria, estado
Activar/Desactivar, cuatro alcances, celdas ausentes ignoradas, **una** notificación agregada por
operación y **sin rebuild**), la fila WPF compartida `SelectionMatrixBulkBar`, y la adopción por los
**cuatro** diálogos. La **parrilla** entró por addendum del Owner conservando su **contador vivo**,
resuelto con un **adorno opt-in y neutral** del control (`CellAdornment` + `RefreshAdornments`) que deja
los otros tres diálogos sin una sola línea de cambio. Corrigió además un defecto propio: un valor **no
definido** de `SelectionMatrixScope` se interpretaba como «Todo» y reescribía la rejilla entera; ahora
falla cerrado.

**Alcance vivo en `RackCad.UI` únicamente**: Domain, Application y Plugin no tienen un solo cambio, y
catálogos, bloques DWG, `deploy/` y los workflows de CI quedaron intactos.

**La defensa NO entró y NO bloqueó**: pasa a `ideas-futuras.md` como **candidato futuro independiente**,
desligado de I-34, con la decisión que habría que tomar antes de abordarla.

**Siguiente**: **I-25** (`feature/guardas-traseras`) e **I-23** (namespaces, cierra la Fase 5).
**I-35** (`feature/editor-avanzado-push-back`, PB-011) está **en curso y detenida esperando decisión del
Owner**; su rama no fue modificada por esta integración. Al rebasar sobre el nuevo `main` encontrará un
conflicto **mecánico y documental** en `docs/initiatives/README.md` (ambas iniciativas añaden su entrada
al índice) y probablemente otro adyacente en `docs/ideas-futuras.md` (I-34 reescribió el bullet PB-007,
que queda justo antes del PB-011 que I-35 amplió). Ninguno es funcional.

### I-33 — INTEGRADA en `main` (2026-07-27)

El Owner **aprobó toda la validación manual**, incluida la **ronda focalizada de fronteras físicas**. La
iniciativa queda **integrada**; no queda acción pendiente de I-33.

| Campo | Valor |
|---|---|
| Rama (eliminada tras integrar) | `feature/frente-en-blanco` |
| **CODE_SHA / BUILD_SHA aprobado** por el Owner | `b840cfe24578bc9faa3b13dad8b11d90d47aad84` |
| CI del candidato | **30240730244**, 4/4 sobre `b840cfe` |
| Punta de la rama antes del commit documental | `caaad8851780fb0ff33fc3de1fe5866850db4515` (CI **30240912689**, 4/4) — delta contra `b840cfe`: **solo** `docs/automation/state/I-33.yml`, sin cambio de binario |
| **DLL Debug** verificado en el cierre | `AssemblyInformationalVersion = 1.0.0+caaad8851780fb0ff33fc3de1fe5866850db4515`, SHA-256 `51F3FA7F6A9957EFF70689C782790A2C22644F882334FF7092569D73C21A7509` |
| Suites al integrar | **1522** `RackCad.Tests` + **398** `RackCad.UI.Tests`, cero fallos, cero omitidas |
| Builds Debug | UI 0 errores / 0 advertencias; Plugin 0 errores y 2 `MSB3277` conocidos, con AutoCAD cerrado |
| Rebase | **no necesario**: `origin/main` no avanzó desde la base `0e505d8` |
| **MERGE_SHA** | vive en `git log --first-parent main`; este documento **no lo inventa** (se escribe antes del merge) |
| Limpieza | rama local, rama remota y worktree **eliminados** tras la CI post-merge verde |

Qué dejó integrado: el **frente en blanco** (PB-014) para el **Dinámico** y **Push Back**, con la autoridad
única `DynamicFrontActivation` y —decisión del Owner— la **frontera compartida por dos frentes en blanco
que no existe**. El detalle funcional completo está en §1 y el contrato en
[`initiatives/I-33-frente-en-blanco.md`](initiatives/I-33-frente-en-blanco.md).

**Deuda que hereda quien siga:** la decisión sobre `DesviadorCellsAreByPost` del **Dinámico** sigue
**pendiente del Owner** — I-33 corrigió la **forma** de la rejilla del desviador (ya recibe la lista por
poste), pero la **lectura de la celda** en el dibujo continúa siendo por frente. Registrada en
`ideas-futuras.md`.

### I-32 — INTEGRADA en `main` (2026-07-27)

El Owner ejecutó la revalidación manual dirigida y confirmó **«Listo, todo correcto»**. La iniciativa
quedó **integrada**; no queda acción pendiente de I-32.

| Campo | Valor |
|---|---|
| Rama (eliminada tras integrar) | `fix/correcciones-push-back`, punta final `98cfbe3a5e84fac84d6127c1753292feb66dfc1e` |
| **MERGE_SHA** | `236619d9281041f9aead884f11a1f93d6f4c8599` (merge `--no-ff`, dos padres: `91eb53c` + `98cfbe3`) |
| CI del merge | **30228331452**, 4/4 sobre `236619d` |
| **CODE_SHA** funcional | `f911d75350702fb176e123a59a105d40f63690ec` |
| **BUILD_SHA validado** (el que el Owner cargó) | `a0c3f27c2447a4e1f85707ef9f3ad311765e3a43` |
| **DLL SHA-256** aprobado | `B7B15802D19C90BBE40B19546423F9CC1850645051C1DA971DA2552778B2E931` |
| CI del candidato | **30226757221**, 4/4 sobre `a0c3f27` |
| Limpieza | rama local, rama remota y worktree **eliminados** |

**Qué corrigió.** Diez de los catorce hallazgos del reporte del Owner sobre el Push Back ya integrado
(PB-002…006, 008…010, 012, 013), cada uno con su regresión observada fallando sin el fix. Además:

- **PB-004 — elevaciones.** El larguero **posterior** es el ancla y conserva su troquel; el de
  entrada/salida se deriva y se ajusta al suyo. Una sola autoridad, `PushBackElevations`, y un
  **override opt-in** que llega a los builders compartidos como último parámetro opcional en sus
  **cuatro ámbitos** —frente, poste, proyección y envolvente— sin que el Selectivo ni el Dinámico
  cambien una línea de comportamiento.
- **Seguridad.** `SafetySide` mezclaba **tres ejes ortogonales** —pertenencia por poste, orientación y
  extremo longitudinal—; ahora se resuelven por separado. Un `Right` en Push Back se dibuja **delante**,
  espejado, nunca atrás. Y el **default** del protector lateral vuelve a poner uno en **cada poste
  extremo**: el primero sin espejo, el último espejado, ninguno interior.

**La regla geométrica asimétrica final de la cama** —lo que costó cuatro validaciones rechazadas:

1. **Entrada/Salida** — mate `LARGUERO_IN_OUT.TROQUEL_CAMA` ↔ `RIEL_DE_CINTA_CALIBRE_12.TROQUEL_IN`.
2. **Posterior e intermedios** — tangentes a la **línea del ORIGEN** del bloque, que es una recta
   **paralela** a la anterior y **distinta** de ella.
3. **Una sola `RotationRadians`** para todo el bloque, resuelta por `PushBackBedRotation`.
4. El larguero **posterior** es el **ancla** y queda fijo en su troquel.
5. El larguero **bajo** se selecciona **globalmente** por menor error contra 7/192, sobre la retícula de 2".
6. **`LONGITUD` = fondo estructural completo**; el riel puede sobresalir por detrás, y eso es esperado.

> **Trampa que costó cuatro rechazos:** derivar la rotación como `atan2(HighMate − ExitMate)` trata los
> dos contactos como si compartieran recta. **No la comparten.** Su firma inconfundible es que el contacto
> posterior queda a **exactamente 1.25"** de la línea del origen — la separación entre las dos paralelas.
> Si vuelve a aparecer ese 1.25", es esto.

**No queda acción pendiente de I-32.** La integración se hizo con `git merge --no-ff` sobre la punta
remota, sin squash, sin force y sin PR; la limpieza de rama y worktree quedó completada. Los **cuatro
DLL** de las validaciones rechazadas —`2210e67`, `557858d`, `2641830` y `9a87c7c`— siguen **obsoletos**;
el único artefacto válido es el de `a0c3f27` con SHA-256 `B7B15802…`, que **no se recompiló** en ningún
momento del cierre.

Evidencia completa en
[`automation/evidence/I-32-autocad-validation.md`](automation/evidence/I-32-autocad-validation.md).

### I-18 — cerrada

**I-18 (Push Back) quedó integrada en `main` el 2026-07-25** — merge `--no-ff` `77031be` desde
`feature/push-back` (`4f93abe`), CI verde en los cuatro jobs (run 30139506411), rama y worktree
eliminados. No queda acción pendiente de I-18.

Qué dejó integrado: Push Back como **primer sistema construido sobre el patrón de módulos** —descriptor,
documento versionado, resolver y builders por vista, BOM, editor sobre el shell común y draw adapter—
**componiendo** la cama `FlowBedType.Pushback` y la geometría del Dinámico **sin editar** el código de los
otros sistemas, que era el criterio de salida de la Fase 4. El registro es aditivo: `RackSystemKind`,
`SystemRegistry`, `EditorModuleRegistry`, `KindHandlerRegistry`, menú, comandos `RACKPUSHBACK`/`RPB`,
inserción y persistencia con metadata I-11 y carga legacy; los consumidores (`RACKEDITAR`,
`RACKBOMTOTAL`, `RACKDUPLICAR`, `RACKLAYOUT`) lo adoptan por el registro, sin una sola rama nueva.

Cerró además dos deudas transversales: nace [`docs/guias/agregar-un-sistema.md`](guias/agregar-un-sistema.md)
desde la experiencia real (retira el apéndice temporal de `ARCHITECTURE.md`, DOC-02 de I-06), y se extrae la
**infraestructura de preview compartida** del renderer Dinámico, ya consumida por los dos editores, con la
equivalencia del Dinámico **medida** (misma firma de escena sobre 736 primitivas antes y después).

**Deuda abierta que hereda quien siga:** el **preview visual** sigue siendo insatisfactorio para el Owner y
su estandarización completa se **difiere a una iniciativa transversal futura** que abarque a los tres
editores. Parte de una sola tubería compartida, no de dos painters divergentes. **No está aprobado
visualmente** y no debe presentarse como tal.

**Siguiente:** quedan **I-25** (`feature/guardas-traseras`, sobre I-22) e **I-23** (namespaces, cierra
la Fase 5, depende de todas).

## 5. Última verificación vigente

**Baseline integrada de I-44 — 2026-09-03** (la vigente):

- candidato **funcional** aprobado por el Owner: `4947a1b5e43a291b01e8e43b5a8ff36d74c99186`
  (CI run `33797723636`, **success**). El SHA final de rama difiere del aprobado **sólo en documentación
  de cierre**: el binario funcional es idéntico;
- **merge `--no-ff`**: `1165240ad780f32851ebccf18c7da89525d32167`, padres `085ca2f` y `0e13410`,
  con CI verde post-merge sobre el merge ya en `main`;
- `origin/main` **no avanzó** desde la base `085ca2f5b33541cfb93c8cdec8cbc8f0368c899f`: **sin rebase
  final**, de modo que la validación manual corresponde exactamente al contenido integrado;
- **validación manual del Owner en AutoCAD 2025: APROBADA** sobre el **DWG real que presentaba el
  defecto**: el BOM volvió a coincidir, con `ProfileId`, `Length`, `Peralte` y `Quantity` correctos por
  cama. **Sin rondas rechazadas**;
- suites locales sobre el HEAD de rama: **RackCad.Tests 4425/4425** y **RackCad.UI.Tests 1070 correctas /
  17 omitidas / 1087 totales**; Debug de UI (0 advertencias, 0 errores) y del Plugin (0 errores, sólo los
  MSB3277 conocidos);
- filtros dirigidos, todos con descubrimiento no vacío: I-44 **22/22**, I-41 fondo y tarima por celda
  **86/86**, compuesto I-42 **304/304**, corrida · runs · solo-A · solo-B · ambos sentidos **234/234**;
- **6 reproducciones** verificadas en rojo antes del fix y verdes después, sin modificarlas; de las **4**
  pruebas nuevas de contrato, **3** verificadas en rojo sin el fix (la cuarta es la guarda de preservación
  de `Build`, verde a ambos lados por diseño);
- **sin ADR nuevo**: I-44 aplica el contrato ya enunciado en ADR-0031 §8-bis.

**Baseline integrada de I-42 — 2026-09-02** (previa):

- candidato **funcional** aprobado por el Owner: `077d35ad418615bed4c1d8375ea9cfc0de9fca24`
  (CI run `33578565581`, **success** en los cuatro jobs). El SHA final de rama es `9ea11d6` y difiere del
  aprobado **sólo en documentación de cierre** (CI run `33648079222`, **success** en los cuatro jobs);
- **merge `--no-ff`**: `e6bb6d7ce9790e1e9c495cb30e580a9304bccd44`, padres `088c7b9` y `9ea11d6`, con CI
  run `33650124713` **success** en los cuatro jobs sobre el merge ya en `main`;
- `origin/main` **no avanzó** desde la base `088c7b9abac4bb024369238cac6abce8c871b104`: **sin rebase
  final**, de modo que la validación manual corresponde exactamente al contenido integrado;
- **validación manual del Owner en AutoCAD 2025: APROBADA — 8/8 escenarios** (retícula compartida;
  corrida y hueco; restauración con B dormido; despertar fallido; edición confirmada sin recalcular;
  `RACKEDITAR`; restauración, rangos no anidados y seguridad de los dos pasillos; bloqueo de salidas),
  con el escenario 2 **APROBADO CON OBSERVACIÓN** no bloqueante (**CORRIDA GAP STORAGE**, registrada y
  **no** implementada por decisión del dueño);
- **ocho rondas rechazadas** antes de la aprobación (`6c9f778`, `e90442a`, `3b55ca7`, `36fe5d3`,
  `67a24d0`, `d6e6372`, `5a73b92`, `82e918b`), todas corregidas en la misma rama;
- suites locales sobre el HEAD de rama: **RackCad.Tests 4403/4403** y **RackCad.UI.Tests 1070 correctas /
  17 omitidas / 1087 totales**; Debug de UI y del Plugin con **0 errores** (sólo los MSB3277 conocidos);
- **[ADR-0031](adr/0031-push-back-compuesto-estructura-unica-y-configuracion-por-lado.md) `aceptado`**
  por el dueño.

**Baseline integrada de I-41 — 2026-08-23** (previa):

- candidato **funcional** aprobado por el Owner: `c41aee1b8bcbfc0d6fed7a38b8c4767538648cd2`
  (CI run `32627802845`, **success**). El SHA final de rama difiere del aprobado **solo en documentación
  de cierre**;
- **merge `--no-ff`**: `a28c9b73965f528ffbf3c2cd893e52da36995063`, padres `43181a3` y `638c009`;
- `origin/main` **no avanzó** desde la base `43181a33c391eb56c81dcf10b755a603743dc276`: **sin rebase
  final**, de modo que la validación manual corresponde exactamente al contenido integrado;
- **validación manual del Owner en AutoCAD 2025: APROBADA**, sobre fondos distintos por celda,
  restauración al fondo del frente, los cinco alcances de fondo y de tarima, tarimas por celda en el
  lateral seccionado y en los dos cortes frontales, su ausencia del BOM, la regresión de I-40 con
  cabeceras personalizadas por línea y el caso legacy;
- **una ronda de representación rechazada** antes de la aprobación: las tarimas se veían escalonadas en
  el lateral —dibujadas horizontales sobre la línea del ORIGEN del riel en vez de tangentes a los
  rodillos— y desalineadas respecto de su calle en frontal y posterior. Corregido en `c41aee1`;
- suites locales sobre el HEAD de rama: **RackCad.Tests 3133/3133** y **RackCad.UI.Tests 834 correctas /
  17 omitidas / 851 totales**; Debug de UI (0 advertencias, 0 errores) y del Plugin (0 errores, solo los
  MSB3277 conocidos);
- **22 filtros dirigidos** auditados sin que ninguno descubra cero pruebas;
- **[ADR-0030](adr/0030-fondo-por-celda-push-back-y-envolvente-derivada.md) `aceptado`** por el dueño.

**Baseline integrada de I-40 — 2026-08-23** (previa):

- candidato **funcional** aprobado por el Owner: `b43fcb433eb717ad5484b67f400fb5b77bc03826`
  (CI run `32621965348`, **success** en los cuatro jobs: Tests, Build UI, UI Tests y Build Plugin). El
  SHA final de rama, `2673aab`, difiere del aprobado **solo en documentación de cierre**, y su propio CI
  (run `32622721354`) también quedó **success** en los cuatro jobs;
- **merge `--no-ff`**: `bf327b353fc181d3ca5192641c54b9abf96ea39d`, padres `8a54a4d` y `2673aab`;
- `origin/main` **no avanzó** desde la base `8a54a4d`: **sin rebase final**, de modo que la validación
  manual corresponde exactamente al contenido integrado;
- **validación manual del Owner en AutoCAD 2025: APROBADA**, sobre personalización de cabeceras,
  persistencia tras Actualizar, reapertura con RACKEDITAR, copia de configuraciones, selección
  cartesiana en sus cinco combinaciones (una×una, una×todas, varias×varias, todas×una, todas×todas),
  poste derivado por línea, frontal y posterior sincronizados con los cortes físicos y **no**
  envolventes, líneas intermedias independientes, protector lateral del último corte, Cancelar y
  geometría general;
- suites locales sobre el HEAD de rama: **RackCad.Tests 3037/3037** y **RackCad.UI.Tests 818 correctas /
  17 omitidas / 835 totales**; Debug de UI (0 advertencias, 0 errores) y del Plugin (0 errores, solo los
  MSB3277 conocidos);
- **cinco rondas rechazadas** antes de la aprobación (`e31902b`, `3669adc`, `b515dc5`, `73325cf`,
  `04f76cf`). Cada una dejó su causa raíz escrita en
  [`initiatives/I-40-cabeceras-push-back.md`](initiatives/I-40-cabeceras-push-back.md): la frontera del
  configurador que devolvía una instancia obsoleta, la regeneración desde plantilla del modo rápido,
  dibujar sin confirmar la sesión, la sesión que sobrevivía a cargar otro rack, el alcance acoplado al
  configurador y la envolvente de altura en las dos vistas.

**Baseline integrada de I-37C — 2026-07-29** (histórica):

- candidato de **código** aprobado por el Owner y por CI:
  `e1c3cab24d16ea0a6565fc43a81dcc0f2e31c694` (CI run `30510202275`, **success** en los cuatro jobs). El SHA
  final de rama difiere del aprobado **solo en documentación de cierre**. `origin/main` **no avanzó** desde
  la base `0610adb`: **sin rebase final**;
- **validación en AutoCAD: NO APLICA.** `requires_autocad: false`, `requires_owner_validation: false` —
  I-37C todavía no dibuja. Es la tercera y última de la línea cuyo gate se resuelve **sobre el código**:
  I-37D sí exigirá `NETLOAD` y el veredicto manual del Owner;
- **ADR-0026 ACEPTADO**, veredicto `OWNER_APPROVED_ADR_0026`, con sus **veintidós** puntos enumerados:
  estación sencilla o doble, una columna compartida en doble, una o dos bases, niveles compartidos, templates
  sin autoridades duplicadas, default más overrides de celda con comparación **estructural**, claro cuerpo a
  cuerpo, ajuste obligatorio hacia arriba a troqueles, retícula regular única con **acumulación preservada**
  y **dominio numérico derivado**, métricas de conexión compartidas con I-37B, pase final completo, altura
  automática o manual con `TopClearFactor >= 1/3`, estación inmutable, BOM derivado de la geometría resuelta,
  columna–base y brazo como componentes, piezas planas identificadas por receta, placas medidas en su plano y
  lado excluido de la identidad de un brazo. **No se reabren en I-37D**;
- suites **2565** + **544**, cero fallos, cero omitidas (base 2355 + **210** nuevas: 81 de estación, 78 de la
  ronda de corrección, 16 y 9 de las dos caracterizaciones, y el archivo de guardas de 63 a **89**); builds
  Debug de Domain, Application y UI con **0 errores y 0 advertencias**, y Plugin con 0 errores y las 2
  `MSB3277` conocidas;
- **dos caracterizaciones previas**, escritas ANTES de cada extracción: la de la retícula regular atrapó una
  desviación numérica real —con pitch no diádico, `27.599999999999998` acumulado contra `27.6` multiplicado—,
  y la de las métricas de conexión demostró que la autoridad compartida no movió nada de I-37B;
- **veintitrés regresiones verificadas en rojo** y revertidas: catorce en la primera ronda —entre ellas la
  doble como dos subensambles, la columna duplicada, el claro redondeado, el ajuste hacia abajo, los troqueles
  compartidos, la placa ignorada, `TopClearFactor` ignorado, la altura manual normalizada, dos componentes
  columna–base y el BOM desde el diseño— y **nueve** en la de corrección: el tope de 250, la fórmula de
  `BodyTopZ` del prelayout, el `DeepCopy` incondicional, la comparación por referencia, el `ProfileId`
  genérico, los spans mundiales, la firma sin brazos, los ternarios de enum y `Math.Max(2, count)`;
- **bundle no ejecutado y no exigible**: el diff no toca `assets/`, catálogos, `deploy/` ni `.github/`;
- **sin default aprobado**, y por tanto entradas obligatorias que el resolver rechaza si faltan: los dos
  offsets de troquel de I-37A y el margen vertical de la placa de conexión del brazo de I-37B. **I-37C no
  añadió ninguno.**

**Baseline anterior — I-37B, 2026-07-29:**

- candidato de **código** aprobado por el Owner y por CI:
  `00d8126eb687a46bafc156480ea6f080f295a771` (CI run `30499888210`, **success**). El SHA final de rama
  difiere del aprobado **solo en documentación de cierre**, así que el árbol técnico aprobado y el integrado
  son el mismo. `origin/main` **no avanzó** desde la base `e0f319f`: **sin rebase final**;
- **validación en AutoCAD: NO APLICA.** `requires_autocad: false`, `requires_owner_validation: false` —
  I-37B no cambia dibujo ni interfaz, igual que I-37A;
- **ADR-0025 ACEPTADO**, veredicto `OWNER_APPROVED_ADR_0025_WITH_CURRENT_DATUM`: cuerpo como colección de
  miembros, los tres arreglos, canales dobles en contacto sin campo de separación, colocación derivada de
  `Bounds`, longitud capturada como corte del perfil con
  `NominalCutLength == GeometricLength == Body.CutLength`, `SlopeRisePer12` como única autoridad, extremo
  libre ascendiendo en ambos lados, selección contigua de troqueles existentes, pitch observado de la
  columna, placa creciendo hacia arriba, bloqueo cuando el cuerpo no cabe, tapa y tope como modos de una
  misma placa, placa final perpendicular al eje inclinado, elegibilidad por
  `StructuralSectionId + Arrangement`, validación exhaustiva de modos y enums, rango de índices sin
  overflow, rechazo diagnóstico de la pendiente que colapsa el marco, **datum actual** del plano de corte
  sobre la cara exterior de la placa, **intrusión y holgura** como aproximación visual declarada con
  magnitudes independientes, coincidencia por `CantileverPunchDatum`, y el **PTR no tratado como alias de
  HSS**. **No** autoriza estación, niveles, doble cara, separadores, arriostres, línea, BOM, peso,
  persistencia, `RackSystemKind`, registros, editor, preview, vistas, AutoCAD, bloques, preparación de
  extremos, fabricación, cálculo resistente ni cambios funcionales en I-36 o I-37A;
- suites **2355** + **544**, cero fallos, cero omitidas (base 2224 + **131** nuevas: 117 de invariantes del
  brazo y el archivo de guardas de fuente de 49 a **63**); builds Debug de Domain, Application y UI con
  **0 errores y 0 advertencias**, y Plugin con 0 errores y las 2 `MSB3277` conocidas;
- **regresiones verificadas en rojo** y revertidas: las ocho de la primera pasada —doble canal reducido a un
  miembro, separación distinta de cero, pendiente invertida en el lado negativo, el brazo recalculando el
  pitch, filas creciendo hacia abajo, placa aceptando un cuerpo más alto, tapa modificando el corte y un
  campo de separación— más las **cuatro** de la ronda de corrección: la suma vulnerable del índice, el gate
  de pendiente desactivado, el `default` retirado de la validación de modo, e intrusión y holgura
  colapsadas en un solo número;
- **bundle no ejecutado y no exigible**: el diff no toca `assets/`, catálogos, `deploy/` ni `.github/`;
- **sin default aprobado**, y por tanto entradas obligatorias que el resolver rechaza si faltan: el margen
  vertical de la placa de conexión del brazo (`MountingPlateVerticalEndOffset`), más los dos offsets de
  troquel que I-37A dejó sin default.

**Baseline anterior — I-37A, 2026-07-29:**

- candidato de **código** aprobado por el Owner y por CI:
  `15523679e655364c146917ece338c7cecbe24023` (CI run `30488839172`, **success**). El SHA final de rama
  difiere del aprobado **solo en documentación de cierre**, así que el árbol técnico aprobado y el
  integrado son el mismo;
- **validación en AutoCAD: NO APLICA.** `requires_autocad: false`, `requires_owner_validation: false` —
  I-37A no cambia dibujo ni interfaz. Es la primera iniciativa de la Fase 6 cuyo gate se resolvió **sobre
  el código** y no sobre el dibujo;
- **ADR-0024 ACEPTADO**, veredicto `OWNER_APPROVED_ADR_0024`: diseño en Domain con ids textuales, frontera
  única de resolución, modelo híbrido por naturaleza física, `PrismaticSectionInstance` como autoridad de
  colocación, patrón compartido base–columna, igualdad exacta del datum con comparación geométrica
  separada, autoridad única de marcos, longitud nominal no liberada para fabricación, geometría exterior
  derivada de I-36, elegibilidad por combinaciones exactas, datum declarado y validaciones de ajuste de
  troqueles. **No** autoriza vistas, UI, AutoCAD, persistencia, brazos, estaciones, separadores,
  arriostres, BOM, peso, cálculo, fabricación ni cambios a I-36;
- suites **2224** + **544**, cero fallos, cero omitidas (base 2094 + **130** nuevas: 81 de invariantes y 49
  de guardas de fuente); builds Debug de Domain, Application y UI con **0 errores y 0 advertencias**, y
  Plugin con 0 errores y las 2 `MSB3277` conocidas;
- **once regresiones verificadas en rojo** y revertidas, entre ellas los tres troqueles sobre la base, la
  transición de 2 in a 4 in, la autoridad compartida, el patrón simétrico de la placa inferior, la igualdad
  del datum, el marco cableado fuera de su autoridad, y una validación de offset y una de pitch;
- **bundle no ejecutado y no exigible**: el diff no toca `assets/`, catálogos, `deploy/` ni `.github/`, que
  es lo que el bundle inventariaría;
- **sin default aprobado**, y por tanto entradas obligatorias que el resolver rechaza si faltan: el offset
  desde los extremos de la placa inferior de la columna a sus troqueles, y el offset superior de la columna
  al último troquel regular.

**Baseline anterior, de I-36D — 2026-07-28**:

- candidato de **código** aprobado por el Owner y por CI:
  `3ffe4dff3ac623dcb53fc715ebc5b81ed6bcde68` (CI run `30410876362`, **success** 4/4). El SHA final de
  rama difiere del validado **solo en documentación**, así que el árbol técnico validado y el integrado
  son el mismo;
- **validación en AutoCAD APROBADA**, **sin observaciones**, veredicto `OWNER_APPROVED_ADR_0023`, sobre
  el DLL Debug `6A88D9FE…` (121 856 bytes);
- **ADR-0023 ACEPTADO**: separación de autoridades, pendiente `1:6` como convención declarada de
  RackCad, radio visual deducido, punta aguda, autoridad ortogonal a la fidelidad, residuo de área
  diagnóstico, advertencia obligatoria, y W/HSS/C/L sin cambios;
- catálogo neutral: **1 011** secciones, `S` = **28**, cuatro CSV previos **byte-idénticos**,
  `secciones.csv` intacto, `mapperVersion` `I-36D.1`;
- suites **2094** + **544**, cero fallos, cero omitidas; builds Debug de Application, UI y Plugin sin
  errores propios; bundle **153** comprobaciones y harness **10/10**.

**Baseline anterior, de I-36C — 2026-07-28**:

- candidato de **código** aprobado por el Owner y por CI:
  `86867e62bba9c52bd0855719b1f51ba99c3edcaa` (CI run `30386035953`, **success** 4/4). El SHA final de
  rama difiere del validado **solo en documentación**, así que el árbol técnico validado y el integrado
  son el mismo;
- **validación en AutoCAD APROBADA**, siete puntos, **sin observaciones**: botón visible, posición y
  estilo, cancelación del inspector, cancelación del punto, inserción de `W12X26`, **equivalencia con
  `RACKSECCION`** y sistemas existentes sin regresiones;
- suites **2071** + **534**, cero fallos, cero omitidas; builds Debug de Application, UI y Plugin sin
  errores propios; bundle **147 comprobaciones**;
- **cero duplicación del generador**: cada pieza del caso de uso la menciona exactamente un archivo del
  Plugin; **cero cambios geométricos**; **cero cambios en catálogos**; **cero cambios en sistemas**;
- **sin rebase**: `origin/main` no avanzó desde la base `14317a5`; rebase final **no-op**.

Anterior: **baseline integrada de I-36B — 2026-07-28**:

- candidato de **código** aprobado por el Owner y por CI:
  `30ef95c56c9ce6d3120e13c29f971c40dd65fbec` (CI run `30378134540`, **success** 4/4). El SHA final de
  rama difiere del aprobado **solo en documentación** —cero archivos de `src`, `tests`, `tools`,
  `assets`, `deploy` o `.github`—, así que el árbol técnico validado y el integrado son el mismo;
- **validación en AutoCAD APROBADA**: smoke focalizado de cinco puntos **y** checklist completo de doce,
  sobre el DLL Debug del worktree de la rama, sin bloqueos;
- suites **2043** + **523**, cero fallos, cero omitidas; builds Debug de Application, UI y Plugin sin
  errores propios; bundle **147 comprobaciones**;
- **983 secciones** generan geometría en los **dos** niveles de detalle sin una sola excepción: **289**
  `TabulatedComplete`, **694** `TabulatedDerived`, **cero** degradadas. Ningún diagnóstico silencioso;
- el error de área por familia está **medido y documentado**, no ajustado: el del HSS es una diferencia
  de **definición** —AISC calcula `A` con `tdes` y la geometría dibuja `tnom`, y una prueba lo acredita
  reconstruyendo el mismo contorno con `tdes`—, y el de C es la conicidad y el radio de punta que la
  fuente no publica, **aceptado por decisión del Owner**;
- **ADR-0022 aceptado**; el requisito futuro de **IPS/S y geometría visual mejorada** queda registrado en
  cinco documentos y **sin rama abierta**;
- **sin rebase**: `origin/main` no avanzó desde la base `eafb785`; rebase final **no-op**.

Anterior: **baseline integrada de I-36A — 2026-07-28**:

- candidato de **código** aprobado por el Owner y por CI:
  `5cd526cca252ffcd30dc0e598c8e3049632ea4ec` (CI run `30354958938`, **success** 4/4). El SHA final de
  rama, `c899a367fa76cd16218f364d438bc3491908cd2e` (CI `30363426285`, 4/4), difiere del aprobado
  **solo en documentación** —cero archivos de `src`, `tests`, `tools`, `assets`, `deploy` o `.github`—,
  así que el árbol técnico validado y el integrado son el mismo;
- **sin validación en AutoCAD**, por decisión expresa del Owner: I-36A no cambia dibujo, bloques,
  comandos ni el comportamiento visible de ningún sistema;
- suites **1851** + **494**, cero fallos, cero omitidas; builds Debug de UI y Plugin sin errores propios;
  bundle **147 comprobaciones** con los siete archivos nuevos dentro;
- **983 secciones** importadas —289 / 525 / 32 / 137— con **cero** filas seleccionadas rechazadas, y los
  cuatro CSV de familia **byte-idénticos** entre reimportaciones; `--check` contra el libro oficial da OK
  tanto en los datos reproducibles como en el overlay local;
- que la CI sea verde **en Linux** importa aquí de forma específica: el job de pruebas corre en ubuntu
  sobre un checkout limpio y recalcula el SHA-256 de cada CSV distribuido contra el manifiesto, lo que
  demuestra que la regla `-text` de `.gitattributes` hace su trabajo con `core.autocrlf=true`;
- **sin rebase**: `origin/main` no avanzó desde la base `a35374f`.

Anterior: **baseline integrada de I-23 — 2026-07-27**:

- candidato de **código** aprobado por el Owner en AutoCAD 2025 y por CI:
  `5d49a6cc990c5fc72e321aea37dd5bc2d3d4a128` (CI run `30304742946`, **success** 4/4), 11 ahead / 0 behind
  sobre la base. La punta de la rama antes del cierre documental fue 12 ahead; su delta contra el candidato
  es **solo** documentación (0 archivos de `src`, `tests`, `assets`, `deploy` o `.github`), así que el
  binario validado y el integrado son el mismo árbol de código;
- **DLL Debug** validado por el Owner:
  `AssemblyInformationalVersion = 1.0.0+5d49a6cc990c5fc72e321aea37dd5bc2d3d4a128`, SHA-256
  `D2944E25C20098CD57AA15DA143EB2C7412710ED61A78BE548B8CD87146D43EE`, en
  `…-refactor-namespaces-sistemas/src/RackCad.Plugin/bin/Debug/net8.0-windows/RackCad.Plugin.dll`;
- suites **1619** + **494**, cero fallos, cero omitidas; builds Debug de UI y Plugin sin errores propios;
- **7 goldens byte-idénticos** y **superficie de API idéntica** a la base tras normalizar namespace y el
  único renombre autorizado; bundle 105 comprobaciones + harness 10/10;
- **sin rebase**: `origin/main` no avanzó desde la base `b43b5d1`.

Anterior: **baseline integrada de I-35 — 2026-07-27**:

- candidato de **código** aprobado por el Owner en AutoCAD 2025 y por CI:
  `f2be30c20a7ff8958a24ddf078a5310dab5dbfe0` (CI run `30293536290`, **success** 4/4). La punta de la rama
  antes del cierre documental fue `ec52b678e7058f556e49b46cab4b0f38967e50d4` (CI run `30293863850`,
  **success** 4/4); su delta contra el candidato es **solo** `docs/automation/state/I-35.yml`, así que el
  binario validado y el integrado son el mismo árbol de código;
- **DLL Debug** validado por el Owner:
  `AssemblyInformationalVersion = 1.0.0+f2be30c20a7ff8958a24ddf078a5310dab5dbfe0`, SHA-256
  `4FE530EFA0FFAEF005B20253A1C0F68BF99D321A82766D4FF559A3367E99C101`, ruta
  `…-feature-editor-avanzado-push-back\src\RackCad.Plugin\bin\Debug\net8.0-windows\RackCad.Plugin.dll`;
- suites **1612** + **491**, cero fallos, cero omitidas; builds Debug de UI y Plugin sin errores propios;
- **sin rebase**: `origin/main` no avanzó desde la base `52ce27f`.

Anterior: **baseline integrada de I-34 — 2026-07-27**:

- candidato de **código** aprobado por el Owner en AutoCAD y por CI:
  `dbdda74860052c481998da8b63383cf68ec499cc` (CI run `30283957763`, **success** 4/4). Es también la punta
  de la rama **antes del commit documental de cierre**, así que el binario validado y el árbol de código
  integrado son el mismo. `origin/main` **no avanzó** desde la base `7e48b5c…`, de modo que **no hubo
  rebase** y la validación del Owner vale sobre el árbol integrado (WORKFLOW §6). Este documento **no
  inventa** el SHA del merge: vive en `git log --first-parent main`;
- **DLL Debug** verificado en el cierre:
  `AssemblyInformationalVersion = 1.0.0+dbdda74860052c481998da8b63383cf68ec499cc` (el sufijo `+<sha>`
  **coincide con el SHA exacto de la rama**), SHA-256
  `5353C298B5B099BA9DEDAA42C2252DD6891952C7FE83EFD4C0261E4B82796E39`, ruta
  `…-feature-edicion-masiva-seguridad\src\RackCad.Plugin\bin\Debug\net8.0-windows\RackCad.Plugin.dll`.
  Recompilado en la sesión de cierre y **bit a bit idéntico** al que el Owner aprobó;
- suites completas **verdes**: **1522** `RackCad.Tests` + **469** `RackCad.UI.Tests`, cero fallos y cero
  omitidas. Las nuevas de I-34 son, todas en UI: `SelectionMatrixBulkEditTests` (13),
  `SelectionMatrixBulkEditorStaTests` (12), `SelectionMatrixScopeGuardTests` (11),
  `SafetyGridBulkAdoptionTests` (22) y `SafetyParrillaBulkAdoptionTests` (13). El core **no cambia**:
  la iniciativa vive entera en `RackCad.UI`;
- **filtros dirigidos**, todos con conteo no cero: UI `SelectionMatrix` 63, `SelectionMatrixBulkEdit` 25,
  `SelectionMatrixScopeGuard` 11, `SafetyGridBulkAdoption` 22, `SafetyParrillaBulkAdoption` 13,
  `SafetyGrid` 38, `Parrilla` 14, `Desviador` 28, `Tope` 35, `BlankFront` 36, `Selective` 32,
  `PushBack` 132; core `Parrilla` 28, `Safety` 129, `Selective` 324, `Persistence` 119, `Bom` 106,
  `Frontal` 75, `Lateral` 161, `Equivalence` 11, `Catalog` 118;
- **builds Debug**: UI 0 errores / 0 advertencias; Plugin 0 errores con los 2 `MSB3277` conocidos;
- **intactos respecto de `origin/main`**: `assets/` (catálogos), bloques DWG, `deploy/`, `.github/`
  (workflows de CI) y los tres proyectos `RackCad.Domain`, `RackCad.Application` y `RackCad.Plugin`.

**Baseline anterior — I-33, 2026-07-27:**

- candidato de **código** aprobado por el Owner en AutoCAD y por CI:
  `b840cfe24578bc9faa3b13dad8b11d90d47aad84` (CI run `30240730244`, **success** 4/4). La punta de la rama
  antes del commit documental fue `caaad8851780fb0ff33fc3de1fe5866850db4515` (CI run `30240912689`,
  **success** 4/4); su delta contra el candidato es **solo** `docs/automation/state/I-33.yml`, así que el
  binario validado y el integrado son el mismo árbol de código. Este documento **no inventa** el SHA del
  merge: vive en `git log --first-parent main`;
- **DLL Debug** verificado en el cierre:
  `AssemblyInformationalVersion = 1.0.0+caaad8851780fb0ff33fc3de1fe5866850db4515` (el sufijo `+<sha>`
  **coincide con el SHA exacto de la rama**), SHA-256
  `51F3FA7F6A9957EFF70689C782790A2C22644F882334FF7092569D73C21A7509`, ruta
  `…-feature-frente-en-blanco\src\RackCad.Plugin\bin\Debug\net8.0-windows\RackCad.Plugin.dll`;
- suites completas **verdes**: **1522** `RackCad.Tests` + **398** `RackCad.UI.Tests`, cero fallos y cero
  omitidas. Las nuevas de I-33 son `BlankFrontTests` (28), `BlankFrontSafetyTests` (9) y
  `BlankFrontBoundaryTests` (15) en el core, más `BlankFrontEditorTests` (8), `BlankFrontSafetyGridTests`
  (11) y `BlankFrontDesviadorHandoffTests` (14) en UI;
- **filtros dirigidos**, todos con conteo no cero: core `BlankFront` 52, `BlankFrontBoundaryTests` 15,
  validador de catálogos 25, `PushBack` 444, `Dynamic` 197, `Selective` 324, `Persistence` 119; UI
  `BlankFront` 33, `Safety` 46, `Dynamic` 38, `PushBack` 129, `Selective` 32;
- **builds Debug**: UI 0 errores / 0 advertencias; Plugin 0 errores con los 2 `MSB3277` conocidos, con
  AutoCAD cerrado;
- **catálogos intactos**: `git diff origin/main..HEAD -- assets/` vacío y el validador conserva su baseline
  aprobado.

**Baseline anterior de I-31 — 2026-07-24:**

- candidato de **código** validado por el Owner en AutoCAD y por CI:
  `b638653b10bdba5cd5c1d9f814f196c177f18c3e` (CI run `30108459424`, **success**); el commit documental
  de **registro de aprobación** recibe su propio CI verde (`dc9b974`, run `30110856533`) y el **merge de
  `main`** su CI verde (run `30111201050`); este documento **no inventa** el SHA del merge (vive en
  `git log --first-parent main`: merge **`ad0ea1f`**);
- **DLL Debug validado**: `AssemblyInformationalVersion = 1.0.0+b638653b10bdba5cd5c1d9f814f196c177f18c3e`
  (el sufijo `+<sha>` **termina en el SHA completo**), ruta
  `…-refactor-selective-visual-shell\src\RackCad.Plugin\bin\Debug\net8.0-windows\RackCad.Plugin.dll`; el
  Owner **aprobó los 12 puntos** funcionales y visuales del Selectivo migrado (§2);
- suite completa **verde**: **1016** `RackCad.Tests` + **237** `RackCad.UI.Tests` (218 de la baseline de
  I-30 + **19** `SelectiveShellMigrationTests`: raíz = shell, 45 `x:Name` en slot, acciones por
  categoría con motivos, selección/alcance, matrices jagged, selector de fondo, previews frontal/lateral,
  insertar frontal/lateral/planta y actualizar por handlers reales, mínimo mostrado sin recorte);
  **ningún filtro devuelve cero pruebas**; builds Debug de UI, Plugin y solución con **0 errores** (los
  `MSB3277` de las referencias de AutoCAD no cuentan);
- `origin/main` **no avanzó** desde `40a2c8e` durante todo el ciclo de I-31 (merge-base = `origin/main`;
  I-31 **6 commits delante, 0 detrás** antes del merge): **sin rebase final**, la validación en AutoCAD
  vale sobre el árbol integrado (WORKFLOW §6);
- el diff final se limita a `src/RackCad.UI/RackSelectiveWindow.xaml` (su `.cs` **no** cambia),
  `tests/RackCad.UI.Tests/` (`SelectiveShellMigrationTests.cs` + `EditorWindowTestSupport.cs`) y
  documentación de I-31; **no** toca `Shell/`, `Themes/`, Plugin, Application, Domain, persistencia,
  catálogos ni `feature/push-back` (`b2d9e9d`, intacta);
- **ADR-0019** ya aceptado cubre la migración; **handoff obligatorio**: **reanudación de I-18** (rebasar
  `feature/push-back` sobre el nuevo `origin/main`).

**Baseline integrada de I-30 — 2026-07-24:**

- candidato de **código** validado por el Owner en AutoCAD y por CI:
  `d443ee226651c7a80840c8a97e0383163c48d60c` (CI run `30099517253`, **cuatro jobs verdes** —Tests
  (Domain+Application), Build UI, UI Tests (WPF, net8.0-windows) y Build Plugin without AutoCAD); el commit
  documental de cierre recibe su propio CI verde antes del merge; este documento **no inventa** el SHA del
  merge de `main` (vive en `git log --first-parent main`);
- **DLL Debug validado**: `AssemblyInformationalVersion = 1.0.0+d443ee226651c7a80840c8a97e0383163c48d60c`
  (reconstruido con `--no-incremental` desde ese HEAD; el sufijo `+<sha>` **termina en el SHA completo**),
  ruta `…-architecture-editor-visual-shell\src\RackCad.Plugin\bin\Debug\net8.0-windows\RackCad.Plugin.dll`;
  el Owner **aprobó los 12 puntos** funcionales y visuales (§2);
- suite completa **verde**: **1016** `RackCad.Tests` + **218** `RackCad.UI.Tests` (204 fundación + 8
  migración + 6 de tamaño/estado: contrato de tamaño, mínimo mostrado, holgura, paleta por tokens y `Require`
  ruidoso); **ningún filtro devuelve cero pruebas**; builds Debug de UI, Plugin y solución con **0 errores**
  (los `MSB3277` de las referencias de AutoCAD no cuentan);
- `origin/main` **no avanzó** desde `8a1bce5` durante todo el ciclo de I-30 (merge-base = `origin/main`;
  I-30 **19 commits delante, 0 detrás**): **sin rebase final**, la validación en AutoCAD vale sobre el árbol
  integrado (WORKFLOW §6);
- el diff final **no** toca `RackSelectiveWindow` (I-31) ni `feature/push-back` (`b2d9e9d`, intacta): la
  producción se limita a `src/RackCad.UI/Shell/`, `src/RackCad.UI/Themes/`, la composición de
  `RackDynamicSystemWindow.xaml` (su `.cs` no cambia) y `tests/RackCad.UI.Tests/`;
- **ADR-0019** (shell por composición y slots) **aceptado por el Owner**; **handoff obligatorio**:
  **I-31 → reanudación de I-18**.

**Baseline integrada de I-07 — 2026-07-22 (solo documentación):**

- iniciativa **solo documental**: **no cambia código, pruebas ni build**; el baseline de pruebas se
  mantiene en el de I-03 (**1004** `RackCad.Tests` + **184** `RackCad.UI.Tests`), sin cambio; diff **vacío**
  bajo `src/`, `assets/` y `deploy/` (solo `docs/adr/`, `docs/initiatives/` y `docs/automation/`);
- `origin/main` **no avanzó** desde `6d080eb` durante el ciclo de I-07: **sin rebase** (merge-base =
  `origin/main`; I-07 **6 commits delante, 0 detrás**); el candidato aprobado por el dueño es `600b22e`;
- el dueño **aceptó** los **ADR-0006 a 0018** el 2026-07-22 («Sí, apruebo»; registro en
  `docs/automation/decisions/I-07.md`): estados `propuesto` → `aceptado`, secciones normativas inmutables
  con nota posterior; la aceptación **conserva** las limitaciones sobre fecha, decisores y evidencia
  originales y **no** amplía I-07 ni absorbe I-18/I-23/I-25;
- cierre documental: se retiran las **trece decisiones** de HANDOFF §7 con su aviso temporal (cubiertas por
  los ADRs; §7 queda como puntero a `docs/adr/`) y se marca I-07 `integrada (2026-07-22)` en ROADMAP;
- CI de rama **verde** sobre el candidato `600b22e` (run `29965115445`, **cuatro jobs verdes** —Tests
  (Domain+Application), Build UI, UI Tests (WPF controls, net8.0-windows) y Build Plugin without AutoCAD—) y
  re-verde sobre el **commit documental final** antes del merge; este documento **no inventa** el SHA del
  merge de `main` (vive en `git log --first-parent main`).

**Baseline integrada de I-17 — 2026-07-22:**

- candidato de **código** validado por CI: `28e5cfeeccfbfe60ab844f1555d3580405ebfbb8` (CI run `29952433309`,
  **cuatro jobs verdes** —Tests (Domain+Application), Build UI, UI Tests (WPF controls, net8.0-windows) y Build
  Plugin without AutoCAD); el commit documental de cierre recibe su propio CI verde antes del merge; este documento
  **no inventa** el SHA del merge de `main` (vive en `git log --first-parent main`);
- `origin/main` **no avanzó** desde la base `f674bd4` durante el ciclo de I-17: **sin rebase** (merge-base =
  `origin/main`, 0 commits detrás); I-03 (`refactor/fallos-silenciosos`) e I-07 (`docs/adr-retroactivos`) siguen
  activas en remoto pero **no** integradas, así que no hubo conflicto con `RackFrameProjectStore.cs` (el cambio de
  I-17 es aditivo); la rama se integra por `git merge --no-ff`;
- suite `RackCad.Tests`: **993/993 verdes** (981 base + **12** de I-17: 11 en `RackFrameConfigurationDeepCopyTests`
  —modelo persistido por forma de alambre, **excepciones sin compartir referencias**, **modelo derivado
  miembro-a-miembro** superiores y por panel, independencia, idempotencia, `null`, equivalencia con las dos rutas
  previas y la **guarda por reflexión** de clasificación de propiedades— + **1** regresión de **I-11** en
  `PersistenceReopenPreservationTests`); suite `RackCad.UI.Tests`: **184/184 verdes** (183 base + 1 de restore del
  configurador); filtro persistencia/I-11/DeepCopy **143/143**; la prueba de excepciones se verificó **fallando**
  con el reanexado desactivado (1 fallo/11 ok) y se restauró el fix;
- build UI Debug: **0 errores y 0 advertencias propias**; build solución completa Debug (Plugin incl., AutoCAD
  cerrado): **0 errores**, únicamente las dos familias `MSB3277` conocidas del Plugin;
- diff vs `origin/main`: **10 archivos** (`RackFrameProjectStore.cs` **aditivo**: `DeepCopy` + `CloneException`,
  sin tocar `Serialize`/`Deserialize`/`Save`/`Load`; 3 `.cs` de UI —dinámico/selectivo/configurador: comentarios +
  delegación + reasignación—; 2 archivos de prueba nuevos/modificados con la regresión I-11; 3 docs), **sin** tocar
  XAML, DTO (`RackFrameProjectDocument`), formato físico, catálogos, geometría, BOM, GUID ni los stores de **I-03**;
- objetivo entregado (hallazgo **U4**): **un solo** deep-clone de `RackFrameConfiguration` vía el store de
  serialización, con preservación **completa** del estado (persistido + derivado + excepciones runtime).

**Baseline integrada de I-03 — 2026-07-22:**

- punta de **código** de la rama: `ff6f460` (últimos cambios de código de la revisión de defectos 1-4:
  producción `52da117` + tests `ff6f460`); el commit de estado `c3a9c47` (candidato revisado) y este cierre
  documental **no cambian código**; el commit documental final recibe su propio CI verde antes del merge; este
  documento **no inventa** el SHA del merge de `main` (vive en `git log --first-parent main`);
- `origin/main` **avanzó** de `f674bd4` a `b60f142` (Merge I-17) durante la integración: la rama se **rebasó** sobre
  `b60f142` reconciliando **sólo** documentación compartida (HANDOFF/ROADMAP/README/`ideas-futuras`) y **preservando
  el contenido integrado de I-17**; el código de I-03 e I-17 es disjunto salvo `RackFrameProjectStore.cs` (aditivo
  por ambos lados: I-17 añade `DeepCopy`, I-03 cambia `Save`, auto-fusionado); `ff6f460` es la punta de código y
  `c3a9c47` la previa al commit documental (ambos anteriores al rebase); la rama se integra por `git merge --no-ff`;
- suite `RackCad.Tests`: **1004/1004 verdes** (981 baseline + **19** iniciales de I-03 —`RackLogFormatter`,
  `RackDiagnosticsLog`, `AtomicFile`, distinción de carga en settings/templates— + **4** de la revisión —fachada
  redirigida y **3 negativos deterministas**—; **rojo→verde** demostrado en los 2 negativos de lógica: un error de
  lectura ≠ ausente no se registraba bajo la guarda `File.Exists`, y el fallo de cuarentena era silencioso, ambos
  verdes tras los fixes); suite `RackCad.UI.Tests`: **183/183 verdes** (sin cambio; I-03 no toca UI);
- build UI Debug y solución completa Debug: **0 errores**, únicamente las dos familias `MSB3277` conocidas del
  Plugin; el build del Plugin se ejecutó con **AutoCAD 2025 cerrado** (DLL no bloqueado);
- CI de rama verde sobre `c3a9c47` (run `29952811337`, **cuatro jobs verdes** —Tests (Domain+Application), Build UI,
  UI Tests (WPF controls, net8.0-windows) y Build Plugin without AutoCAD—) y re-verde sobre el **commit documental
  final** antes del merge;
- objetivo entregado (P1/D2): logger mínimo best-effort a `%AppData%\RackCad\logs`
  (`RackLog`/`RackDiagnosticsLog`/`RackLogFormatter`, nunca lanza, thread-safe); `Report()` **con stack**
  conservando su mensaje; los **14 `catch`** antes silenciosos del Plugin y `RackCatalogLoader` (fallo de carga +
  aviso de catálogo vacío) registran; **escritura atómica** (`AtomicFile`, temp + `File.Replace`/`Move`, sin crear
  el directorio destino) en los 4 stores; carga que **distingue por excepción** archivo ausente de ilegible
  (`.bad` + log) en `UserSettingsStore`/`UserTemplateStore`, con `CorruptFile` registrando el fallo secundario de
  cuarentena; seam de prueba mínimo `RackLog.RedirectForTests` (**ninguna prueba escribe en el `%AppData%` real**);
- validación manual: **no requerida** (`requires_autocad: false`, `requires_owner_validation: false`; ROADMAP no la
  marca con ✋); **sin validaciones pendientes**;
- invariantes preservados (**compatibilidad I-11**): **sin** cambios de versiones, metadata, geometría, BOM, GUID,
  formatos/DTO persistidos, fallback legacy ni la clave del Xrecord; comandos, alias y mensajes de línea de comandos
  **idénticos**; catálogos, `deploy/`, workflows, `.csproj` del Plugin, `.sln` y DWG **intactos**; **sin**
  dependencias NuGet nuevas (solo `InternalsVisibleTo(RackCad.Tests)` en el `.csproj` de Application); dirección de
  dependencias intacta;
- alcance: `src/RackCad.Application/Diagnostics/{RackLog,RackDiagnosticsLog,RackLogFormatter,CorruptFile}.cs`,
  `Persistence/AtomicFile.cs` + los `Save` de `{RackProjectStore,RackFrameProjectStore}.cs`,
  `RackFrames/UserTemplateStore.cs`, `Settings/UserSettings.cs` y el `.csproj` de Application;
  `src/RackCad.Plugin/{RackCommandSupport,RackCatalogLoader,RackEnvelopeRestamp,RackInventarioCommands.BomTotal,
  RackLayoutCommands,RackLayoutCommands.Fill}.cs` + `Headers/{BlockLibraryImporter,BlockPlacement,
  LateralHeaderDrawer,LateralHeaderDrawService,RackBlockRenamer}.cs`; pruebas (`RackLogTests`, `RackLogTestSupport`,
  `AtomicFileTests`, `UserSettingsStoreTests`, `UserTemplateStoreTests`, `DiagnosticsNegativeTests`); y
  contrato/estado/índice/`ideas-futuras` de I-03.

**Baseline integrada de I-22 — 2026-07-22:**

- punta de **código** validada por CI y por el dueño (AutoCAD): `3ce71394f8858cf600b1e28d042ecebc5ba6a7c2`
  (ancestro de la punta publicada `1e78b2c`; CI run `29944500977` sobre `1e78b2c`, **cuatro jobs verdes**); los
  commits posteriores a `3ce7139` son **solo documentales** (registro de la validación + este cierre de
  integración) y **no cambian código**; el commit documental final recibe su propio CI verde antes del merge; este
  documento **no inventa** el SHA del merge de `main` (vive en `git log --first-parent main`);
- `origin/main` **avanzó dos veces** durante el ciclo de I-22 (`9a895e4` Merge I-20 → **`a50c4ec`** Merge I-05 →
  **`27ffdf3`** Merge I-24): la rama quedó **rebasada** sobre la punta vigente `27ffdf3` (merge-base = `origin/main`,
  **0 commits detrás**); la reconciliación fue **exclusivamente documental** (índice de iniciativas;
  `ideas-futuras` auto-fusionado); I-05 (guardia de unidades en el Plugin) e I-24 (pruebas de UI + seam dinámico)
  tocan código **disjunto** de I-22, **cero solapamiento**, por lo que la validación en AutoCAD **se conserva**
  (WORKFLOW §6); la rama se integra por `git merge --no-ff`;
- suite `RackCad.Tests`: **981/981 verdes** (incluye la caracterización **golden** de 7 baselines —multiset
  frontal/lateral/planta + BOM, con medio frente y cuádruple profundidad—, los planes/DTO por familia, el
  round-trip por subtipo y **5 nuevas** de `SelectiveTopePlan.BuildFrontal`; sin regresión); suite
  `RackCad.UI.Tests`: **183/183 verdes** (154 de I-22 —adopción de rejillas + celdas ausentes— coexistiendo con las
  **29** de I-24 tras el rebase; sin regresión);
- build UI Debug: **0 errores y 0 advertencias propias**; builds Plugin y solución completa Debug: **0 errores**,
  únicamente las dos familias `MSB3277` conocidas del Plugin;
- CI de rama verde sobre `1e78b2c` (run `29944500977`) y re-verde sobre el **commit documental final** antes del
  merge (los **cuatro** jobs —Tests (Domain+Application), Build UI, UI Tests (WPF controls, net8.0-windows) y Build
  Plugin without AutoCAD— en `success`);
- objetivo entregado (E6/E7 de la seguridad del selectivo): **planes/servicios puros de colocación por familia**
  parametrizados por vista (`SelectiveTopePlan` con su resultado **frontal** propio `BuildFrontal`,
  `SelectiveTarimaPlacement`, `SelectiveSeparadorPlan`, unificación de `SelectiveParrillaPlan` `Cells`/`DeckCells`)
  con los builders y el BOM como orquestadores (mueren `TallyByTramo`/`ParrillaExistsAt` y las fórmulas copiadas de
  subida-y-snap); **configuraciones por subtipo** (`SelectiveSafetyConfig`) con `DeepCopy` propio y **DTO reales por
  familia** (`SafetySelectionDocuments.cs`) que aplanan/desaplanan contra el `SafetySelectionDocument` **plano**
  (wire format byte-idéntico, fallback legacy, round-trip por subtipo); **paso de troquel único**
  (`SelectiveRackDefaults.TroquelPaso` en los 5 sitios); y **adopción de `SelectionMatrix`** con **celda ausente**
  (`CellCount`/`Toggle`) por las rejillas tope/desviador/guía;
- validación manual: AutoCAD **requerido** (`requires_autocad: true`) y **aprobado** por el dueño sin observaciones
  («Listo, probé todo, parece estar correcto»): topes/parrillas/tarimas/separadores y elementos relacionados; BOM;
  frontal/lateral/planta; medio frente y múltiples fondos; actualización y vistas ligadas con el **mismo GUID**;
  persistencia/biblioteca/round-trip; y **apariencia e interacción** de las rejillas `SelectionMatrix`;
  **owner-validation aprobada**; DLL Debug del worktree I-22 (código `3ce7139`, **SHA-256**
  `969580AE67EAC69C8018304F3A9DD963C7DDD77307D5A26E913C32CC1A31038C`); evidencia en
  [`automation/evidence/I-22-autocad-validation.md`](automation/evidence/I-22-autocad-validation.md);
- invariantes preservados: **sin** cambios de geometría, coordenadas, planes, BOM, GUID, identidad, inserción/
  actualización, persistencia ni metadatos **I-11**, catálogos, nombres de bloque, mensajes, selección, defaults,
  interacción visible ni comportamiento multivista (fijado por los **7 golden idénticos**); el **formato serializado**
  permanece byte-idéntico; catálogos, `deploy/`, workflows, `.csproj`, `.sln` y DWG **intactos**; **sin** dependencias
  NuGet nuevas; dirección de dependencias intacta (Application no referencia UI/AutoCAD);
- alcance: `src/RackCad.Application/Systems/Selective{TopePlan,TarimaPlacement,SeparadorPlan,SeparadorPlacement,
  ParrillaPlan,ParrillaPlacement,FrontalBuilder,LateralBuilder,PlantaBuilder,BomBuilder,GeometryResolver,TopePlacement}.cs`,
  `src/RackCad.Application/Persistence/{SafetySelectionDocuments,SelectivePalletDesignDocument}.cs`,
  `src/RackCad.Domain/Systems/{SelectivePalletDesign,SelectiveSafetyConfig}.cs`,
  `src/RackCad.UI/Controls/SelectionMatrix{,Model}.cs` + `Safety{Tope,Desviador,GuiaEntrada}GridWindow.cs`, más las
  pruebas (`SelectiveSafetyEquivalenceTests` +2 golden, `SafetySelectionDocumentsTests`, `SelectiveSafetyConfigTests`,
  `Selective{Tope,Tarima,Separador,Parrilla}PlacementTests`, `SelectiveTopePlanFrontalTests`,
  `SelectionMatrixAbsentCellTests`, `SafetyGridAdoptionTests`) y contrato/estado/evidencia/índice de I-22.

**Baseline integrada de I-05 — 2026-07-22:**

- punta de **código** validada por CI y por el dueño (AutoCAD): `f78baaf209c118d168c68620e236341996f9d93e`
  (run `29932135203`, **cuatro jobs verdes**); los commits posteriores de la rama son **solo documentales**
  (registro de aprobaciones + este cierre de integración) y **no cambian código**; el commit documental final
  recibe su propio CI verde antes del merge; este documento **no inventa** el SHA del merge de `main` (vive en
  `git log --first-parent main`);
- `origin/main` **no avanzó** desde `9a895e4` (Merge I-20) durante esta integración: **sin rebase**; `f78baaf`
  (implementación validada) es **ancestro** de la punta final de la rama; la rama se integra por
  `git merge --no-ff`;
- suite `RackCad.Tests`: **936/936 verdes** (base 913 de I-20 + **23 nuevas**: `DrawingUnitsAdvisoryTests` (6,
  decisión pura) y `RackUnitsGuardSourceTests` (17, source-guards del cableado, con demostración **rojo→verde**
  contra la baseline sin cablear); sin regresión); suite `RackCad.UI.Tests`: **139/139 verdes** (sin cambio;
  I-05 no toca UI);
- build UI Debug: **0 errores y 0 advertencias propias**; builds Plugin y solución completa Debug: **0 errores**,
  únicamente las dos familias `MSB3277` conocidas del Plugin;
- CI de rama verde sobre `f78baaf` (run `29932135203`) y re-verde sobre el commit documental de cierre antes del
  merge (los **cuatro** jobs —Tests (Domain+Application), Build UI, UI Tests (WPF controls, net8.0-windows) y
  Build Plugin without AutoCAD— en `success`);
- objetivo entregado: `RackUnitsGuard` en `RackCad.Plugin` (**único lector de `INSUNITS`**, mapeo
  `UnitsValue`→`DrawingUnits`, **una** advertencia no bloqueante antes de la primera modificación) + política
  **pura** `DrawingUnitsAdvisory` en `RackCad.Application.Drawing` (sin AutoCAD); cableada una vez por operación
  en las rutas de inserción (menú, `RACKSELECTIVO`/`RACKSISTEMADINAMICO`/`QUICKCAMA`/`RACKCABECERA`/
  `QUICKCABECERA`), en `RACKEDITAR` solo al insertar vista nueva (`!UpdateOnly`, antes del primer
  `RedrawInPlace`) y en `RACKLAYOUT`/`RACKRELLENAR` antes de sus prompts; `RACKDUPLICAR` fuera por diseño;
  **ADR-0005 aceptado** (`docs/adr/0005-estrategia-de-unidades.md`);
- validación manual: AutoCAD **requerido** (`requires_autocad: true`) y **aprobado** por el dueño sin
  observaciones (pulgadas sin aviso; no-pulgadas y unitless con una advertencia; aviso no bloqueante y sin
  conversión; `RACKEDITAR` actualiza vs inserta; layout/relleno/alias correctos; «Ok, todo funciona»);
  **owner-validation aprobada**; **owner-decision** (ADR-0005) **aprobada**;
- invariantes preservados: **sin** cambios de geometría, coordenadas, BOM, GUID, capas, persistencia/DTO,
  payload/Xrecord, comandos, alias ni mensajes ajenos; catálogos, `deploy/`, workflows, `.csproj` y `.sln`
  **intactos**; **sin** dependencias NuGet nuevas; dirección de dependencias intacta (Application no referencia
  AutoCAD); diff exclusivamente **aditivo**;
- alcance: `src/RackCad.Application/Drawing/DrawingUnitsAdvisory.cs` (nuevo),
  `src/RackCad.Plugin/RackUnitsGuard.cs` (nuevo) + 7 comandos del Plugin (cableado),
  `tests/RackCad.Tests/DrawingUnitsAdvisoryTests.cs` (+6) y `RackUnitsGuardSourceTests.cs` (+17), más ADR-0005,
  contrato/estado/decisión/evidencia e índices de I-05.

**Baseline integrada de I-24 — 2026-07-22:**

- punta de **código** validada por CI: `59dbf0bf5844aa5c228ac3de2d3e16fdcb95763f` (run `29941597964`, **cuatro
  jobs verdes**) antes del rebase final; tras rebasar sobre `origin/main` (`a50c4ec`) el código es **idéntico**
  (el rebase sólo reconcilió el índice de iniciativas), y tanto el **SHA rebasado** como el **commit documental
  de cierre** reciben su propio CI verde antes del merge; este documento **no inventa** el SHA del merge de `main`
  (vive en `git log --first-parent main`);
- `origin/main` **avanzó** de `9a895e4` (Merge I-20) a **`a50c4ec`** (Merge I-05) durante la sesión de I-24: la
  rama se **rebasó** sobre esa punta; la reconciliación fue **exclusivamente documental** (`docs/initiatives/README.md`,
  conservando **íntegras** las entradas de I-05 e I-24); I-05 e I-24 tocan código **disjunto** (I-05 = guardia de
  unidades en el Plugin; I-24 = pruebas de UI + un seam en la ventana dinámica), **cero solapamiento** de código;
- suite `RackCad.Tests`: **936/936 verdes** (sin regresión: I-24 no toca esa suite; las 936 vienen de la base
  rebasada con I-05); suite `RackCad.UI.Tests`: **168/168 verdes** (139 de la base + **29 nuevas**: 13 del
  `RackFrameConfiguratorViewModel`, 8 del dinámico, 4 del selectivo, 4 de la cama);
- build UI Debug: **0 errores y 0 advertencias propias**; builds Plugin y solución completa Debug: **0 errores**,
  únicamente las dos familias `MSB3277` conocidas del Plugin;
- CI de rama verde sobre `59dbf0b` (run `29941597964`, pre-rebase) y re-verde sobre el **SHA rebasado** y el
  **commit documental final** antes del merge (los **cuatro** jobs —Tests (Domain+Application), Build UI, UI Tests
  (WPF controls, net8.0-windows) y Build Plugin without AutoCAD— en `success`);
- objetivo entregado: **29 pruebas** en `tests/RackCad.UI.Tests` que cubren el cableado WPF inalcanzable por la
  suite pura: el `RackFrameConfiguratorViewModel` (antes sin pruebas), la adopción del estado dinámico por su
  ventana caracterizada por **firma COMPLETA del dibujo** (cortes laterales + frontal salida/entrada + planta, por
  instancia, **incluidas anotaciones y cotas**, con el `Name` normalizado antes de comparar) por **punto fijo del
  doble build**, y la identidad/inserción/actualización round-trip de las ventanas selectiva y de cama; las pruebas
  de inserción/actualización recorren los **handlers WPF reales** (`RaiseEvent(ButtonBase.ClickEvent)`) verificando
  identidad/nombre/vista/sección/`UpdateOnly`, el tipo concreto de `InsertionRequest`, la **correspondencia estricta**
  del payload (`request.Design` resuelto == `request.System`) y la metadata de origen **I-11**;
- validación manual: AutoCAD **no ejecutado ni requerido** (`requires_autocad: false`) y **owner-validation no
  requerida** (`requires_owner_validation: false`): el único cambio de producción es un **seam interno sin
  comportamiento** (`RackDynamicSystemWindow.BuildDesignForTest`, reenvía a `Recompose`, +10 líneas, no usado en
  producción); la cobertura se sostiene con las suites automatizadas y el CI verde de la rama;
- invariantes preservados: **sin** cambios de XAML, geometría, BOM, GUID, inserción/actualización, persistencia,
  handlers, Draw Services, catálogos, bloques ni reglas de producto; el diff de producción vs `a50c4ec` es
  **exclusivamente** el seam de 10 líneas en `src/RackCad.UI/RackDynamicSystemWindow.xaml.cs`; **sin** dependencias
  NuGet nuevas; dirección de dependencias intacta (las pruebas de UI no referencian AutoCAD);
- alcance: `src/RackCad.UI/RackDynamicSystemWindow.xaml.cs` (seam +10), `tests/RackCad.UI.Tests/` (5 archivos:
  `EditorWindowTestSupport`, `RackFrameConfiguratorViewModelTests`, `DynamicEditorWindowTests`,
  `SelectiveEditorWindowTests`, `FlowBedEditorWindowTests`), más contrato/estado/índice de I-24 y un hallazgo en
  `docs/ideas-futuras.md` (laguna de cobertura pura `ApplyScope` Level/Front en `DynamicFrontMatrixTests`).

**Baseline integrada de I-20 — 2026-07-21:**

- punta de **código** validada por CI y por el dueño (AutoCAD): `0f430879cdc8f2a369406836db9d8661b8103e3b`
  (run `29888005513`, **cuatro jobs verdes**); tras la aprobación se añadieron una **corrección de comentario
  obsoleto** (doc-comment **sin cambio de comportamiento**) y el **cierre documental**, y la rama se
  **rebasó sobre `main` con I-21** (siguiente viñeta); el **SHA final rebasado** recibe su propio CI verde antes
  del merge; como el comportamiento del selectivo no cambia, la validación en AutoCAD y la owner-validation **se
  conservan** (WORKFLOW §6); este documento **no inventa** el SHA del merge de `main` (vive en
  `git log --first-parent main`);
- `origin/main` **avanzó** de `bfda406` (Merge I-15) a **`2a30fef`** (Merge I-21) durante esta tanda de
  integración serializada: I-20 quedó **rebasada** sobre esa punta (merge-base = `origin/main`); la
  reconciliación fue **sólo de documentación compartida** (HANDOFF, ROADMAP e índice de iniciativas), pues I-21
  sólo toca el editor **dinámico** y su código es **disjunto** del selectivo (cero solapamiento de archivos de
  código/pruebas); la rama se integra por `git merge --no-ff`;
- suite `RackCad.Tests`: **913/913 verdes** (base con I-21 = 889; + **24 nuevas** de `SelectiveEditorStateTests`;
  sin regresión); suite `RackCad.UI.Tests`: **139/139 verdes** (135 de la base + **4** de
  `SelectiveEditorStateAdoptionTests`, caracterización **STA** que fija el dibujo resuelto `load→build`);
- build UI Debug: **0 errores y 0 advertencias propias**; builds Plugin y solución completa Debug: **0 errores**,
  únicamente las dos familias `MSB3277` conocidas del Plugin;
- CI de rama verde sobre la punta pre-rebase `0f43087` (run `29888005513`, cuatro jobs) y re-verde sobre el
  **SHA final rebasado** antes del merge (los **cuatro** jobs —Tests (Domain+Application), Build UI, UI Tests
  (WPF controls, net8.0-windows) y Build Plugin without AutoCAD— en `success`);
- objetivo entregado: `SelectiveEditorState` + `SelectiveEditorCell`/`SelectiveEditorFondoMatrix`/
  `SelectiveApplyScope`/`SelectiveDesignInputs` en `RackCad.Application.Systems` (estado + operaciones puras del
  editor selectivo, hallazgos U1/U3); `RackSelectiveWindow` **observa** ese estado (propiedades de acceso) y
  **delega** las operaciones, conservando el pintado, el editor de celda, los eventos, la recomputación
  coalescida (shell I-15) y el resolve/preview ligado al catálogo; la **superficie pública que consume el
  Plugin no cambia**;
- validación manual: AutoCAD **requerido** (`requires_autocad: true`) y **aprobado** por el dueño sin
  observaciones (matriz, «Aplicar a:» por alcance, cambios de fondo doble/triple, previews frontal/lateral,
  «Insertar frontal», `RACKEDITAR` «Actualizar»/«Insertar lateral-planta» con **mismo GUID**, geometría/BOM sin
  diferencias, **I-11 preservado**, round-trip y reapertura desde biblioteca); **owner-validation aprobada**;
- invariantes preservados: **sin** cambios de geometría, BOM, GUID, inserción/actualización, persistencia
  (Xrecord/**I-11** intactos), catálogos, formatos ni **XAML visible** (`RackSelectiveWindow.xaml` **byte-idéntico**
  a `main`); el diff de I-20 **no toca** el editor **dinámico** (I-21), I-22, DrawServices, DTO/persistencia ni
  generados; **sin** dependencias NuGet nuevas; equivalencia `load→build` fijada por la caracterización STA
  (antes y después del refactor); asimetría de estilos de cota **no** tocada (fuera de alcance);
- alcance: `src/RackCad.Application/Systems/` (5 archivos nuevos: `SelectiveEditorState`, `SelectiveEditorCell`,
  `SelectiveEditorFondoMatrix`, `SelectiveApplyScope`, `SelectiveDesignInputs`),
  `src/RackCad.UI/RackSelectiveWindow.xaml.cs` (observa/delega; −256 líneas netas),
  `tests/RackCad.Tests/SelectiveEditorStateTests.cs` (+24) y
  `tests/RackCad.UI.Tests/SelectiveEditorStateAdoptionTests.cs` (+4 STA), más contrato/estado/índice de I-20.

**Baseline integrada de I-21 — 2026-07-21:**

- punta de **código** validada por CI y por el dueño: `779ee0c4ea06f2a84bc2c5738979449ed25c269f` (run
  `29887985687`, **cuatro jobs verdes** sobre la punta publicada `2470de2`); los commits posteriores de la
  rama son **solo documentales** (registro de validación + este cierre) y **no cambian código**; este
  documento **no inventa** el SHA del merge de `main` (vive en `git log --first-parent main`);
- `origin/main` **no avanzó** desde la base de I-21 (`bfda406`, Merge I-15): **sin rebase**; la rama se integra
  por `git merge --no-ff` en esta sesión;
- suite `RackCad.Tests`: **889/889 verdes** (842 de la base + **47 nuevos** de caracterización/equivalencia:
  `DynamicEditorCell`, `DynamicEditorSafety`, `DynamicFrontMatrix`, `DynamicEditorDesignAssembler`, incluida la
  resolución del diseño armado por el pipeline real); suite `RackCad.UI.Tests`: **135/135 verdes** (la adopción
  STA construye la ventana real y confirma que sigue tomando identidad/inserción de la sesión del shell);
- build UI Debug: **0 errores y 0 advertencias**; builds Plugin y solución Debug: **0 errores**, únicamente las
  dos familias `MSB3277` conocidas del Plugin;
- CI de rama verde sobre la punta de código `2470de2` (run `29887985687`): los **cuatro** jobs —Tests
  (Domain+Application), Build UI, UI Tests (WPF controls, net8.0-windows) y Build Plugin without AutoCAD— en
  `success`;
- objetivo entregado: estado puro del editor dinámico en `src/RackCad.Application/Systems/`
  (`DynamicEditorCell`/`DynamicEditorFront`/`DynamicEditorValues`; `DynamicFrontMatrix` con la matriz
  frente×nivel, la selección y todas las mutaciones —alta/baja, ajuste, toggle, commit, `ApplyScope` vía
  `DynamicRackCellScopeResolver`, snapshot/rollback, refresco/restauración desde el sistema resuelto,
  `BuildFrontDesigns`—; `DynamicEditorSafety`; `DynamicAnnotationOptions`+`DynamicEditorDesignAssembler` con
  `MustRebuild`/`Snapshot`-`RestoreHeaderFondos`/`UpdateHeaderHeightInPlace`/`BuildDesign`); la ventana
  `RackDynamicSystemWindow` lo **consume** (mueren los tipos privados `DynamicFrontRow`/`DynamicCellRow`/
  `DynamicEditorValues` y los helpers movidos) y solo coordina controles/eventos/render/diálogo sobre el
  Editor Shell; code-behind de ~3,339 a ~2,838 líneas;
- validación manual: AutoCAD **requerido** (`requires_autocad: true`) y **aprobado** por el dueño (módulo
  dinámico a profundidad: matriz/selecciones/alcance, cabeceras calculadas y personalizadas,
  seguridad/IN-OUT/intermedios, previews y vistas vinculadas, geometría/BOM, biblioteca/legacy/round-trip,
  actualización en sitio con el mismo GUID); **owner-validation aprobada**;
- invariantes preservados: **sin** cambios de geometría, planes, recetas BOM, GUID, nombre, `Section`, edición
  multivista, persistencia (Xrecord/I-11 intactos), metadatos desconocidos, fallbacks legacy, cabeceras legacy
  ni cama integrada; XAML byte-idéntico; **cero** cambios en Domain, catálogos, `deploy/`, Plugin o `.csproj`;
  **sin** dependencias NuGet nuevas; dirección de dependencias intacta (el estado nuevo vive en Application);
  única remoción incidental: el método privado muerto `EnsureIntermediateBeamDepthCount`;
- alcance: producto en `src/RackCad.Application/Systems/` (7 archivos nuevos) y
  `src/RackCad.UI/RackDynamicSystemWindow.xaml.cs` (adopción, −~500 líneas netas); 4 archivos de pruebas nuevos
  en `tests/RackCad.Tests/`; contrato `docs/initiatives/I-21-dynamic-editor-state.md`, estado
  `docs/automation/state/I-21.yml`, evidencia `docs/automation/evidence/I-21-autocad-validation.md` e índice.

**Baseline integrada de I-15 — 2026-07-21:**

- punta de **código** validada por CI y por el dueño: `2bd5703ee2635019dc15caf3358c6fbdf4d83fa7` (run
  `29879550816`, **cuatro jobs verdes**); el commit posterior de la rama es **solo documental** (este cierre +
  estado versionado) y **no cambia código**, recibiendo su propio CI antes del merge; este documento **no
  inventa** el SHA del merge de `main` (vive en `git log --first-parent main`);
- `origin/main` en `646614d` (Merge I-19 sobre I-12) al momento del gate: I-15 quedó **linealmente rebasada**
  sobre él (merge-base = `origin/main`); **rebase único** ya aplicado (base previa `abc1a53`, Merge I-14), **sin**
  otro rebase; la rama se integra por `git merge --no-ff`;
- suite `RackCad.Tests`: **842/842 verdes** (sin regresión: I-15 no toca Domain/Application; las 842 vienen de la
  base rebasada con I-19); suite `RackCad.UI.Tests`: **135/135 verdes** (85 de I-14 + 45 unitarias del shell + 5
  de **adopción STA** que construyen las ventanas reales);
- build UI Debug: **0 errores y 0 advertencias**; builds Plugin y solución completa Debug: **0 errores**,
  únicamente las dos familias `MSB3277` conocidas del Plugin;
- CI de rama verde sobre la punta de código `2bd5703` (run `29879550816`): los **cuatro** jobs —Tests
  (Domain+Application), Build UI, UI Tests (WPF controls, net8.0-windows) y Build Plugin without AutoCAD— en
  `success`;
- objetivo entregado: `RackEditorSession<TDesign,TSystem>` (catálogo + `RackEditorIdentity` + `RecomputeGate`/
  `RecomputeDebouncer` + contrato de inserción/actualización), `RackInsertionRequest` por `Kind`, e
  `IRackEditorModule`+`EditorModuleRegistry` **explícito sin reflexión**; el **menú** y la **biblioteca** consumen
  el registro (mata ~19 props O(N) + 5 handlers de `RackMainMenuWindow`); el Plugin (`RackMenuCommands.RackCad`)
  despacha el `RackInsertionRequest` por `Kind` a los mismos `Draw*`; las **cuatro ventanas ricas** (selectivo,
  dinámico, cama, cabecera) **adoptan** el shell (props públicas = getters sobre la sesión), verificado por
  `EditorShellAdoptionTests`; larguero no adopta;
- validación manual: AutoCAD **requerido** (`requires_autocad: true`) y **aprobado** por el dueño (menú,
  biblioteca, `RACKSELECTIVO`/`RACKDINAMICO`/`RACKCAMA`, `RACKEDITAR` round-trip con mismo GUID, geometría/BOM sin
  diferencias, I-11 preservado, previews fluidos, larguero sin inserción); **owner-validation aprobada**;
- invariantes preservados: **sin** cambios de geometría, BOM, GUID, edición multivista, persistencia (Xrecord/
  I-11 intactos), formatos ni UI; `RackMainMenuWindow.xaml` **byte-idéntico** a `main`; **cero** archivos de
  Domain/Application/catálogos/DrawServices; estado interno de selectivo/dinámico **reservado a I-20/I-21**; **sin**
  dependencias NuGet nuevas; reconciliación del rebase: conflicto **manual único** en `docs/initiatives/README.md`
  (preservando I-14/I-19), auto-merge verificado en los dos `.csproj` (LangVersion/Nullable de I-12 heredados +
  `InternalsVisibleTo`/copia de catálogos de I-15) y `docs/ideas-futuras.md`; **sin** código funcional de I-19 en UI;
- alcance: `src/RackCad.UI/Editor/` (11 archivos nuevos), las cuatro ventanas adoptadas +
  `RackMainMenuWindow.xaml.cs`, `src/RackCad.Plugin/RackMenuCommands.cs`, `RackCad.UI.csproj` (`InternalsVisibleTo`)
  y `RackCad.UI.Tests.csproj` (copia de catálogos), seis clases de pruebas del shell + `EditorShellAdoptionTests`, y
  contrato/estado/índice/ideas-futuras de I-15.

**Baseline integrada de I-19 — 2026-07-21:**

- punta validada por CI: `fcdc287` (run `29876393665`, **cuatro jobs verdes**); el commit documental de cierre
  posterior **no cambia código**; este documento **no inventa** el SHA del merge de `main` (vive en `git log --first-parent main`);
- `origin/main` en `e2057d7` (Merge I-12) al momento del gate: I-19 quedó **linealmente rebasada** sobre él
  (merge-base = `origin/main`); **rebase único** ya aplicado (base previa `de72287`), **sin** otro rebase;
- suite `RackCad.Tests`: **842/842 verdes** (51 nuevas de I-19; sin regresión sobre las 791 de la base); suite
  `RackCad.UI.Tests`: **85/85 verdes** (I-19 no toca UI);
- build `RackCad.Application` Debug: **0 errores y 0 advertencias**; build UI: **0 advertencias**; build Plugin sin
  AutoCAD: **0 errores**, únicamente las `MSB3277` conocidas;
- CI de rama verde sobre la punta rebasada `fcdc287` (run `29876393665`): los **cuatro** jobs —Tests (Domain+Application),
  Build UI, UI Tests (WPF controls, net8.0-windows) y Build Plugin without AutoCAD— en `success`;
- objetivo entregado: validador puro con severidades (5 categorías) + manifiesto esperado de `blocks-library.dwg`
  (bloques + parámetros dinámicos reales + huella + comparación versión/huella) + modo estricto; guardia por
  **igualdad exacta** builder→manifiesto por `PieceId+View+BlockName` (13 familias) con matriz de cobertura bidireccional;
- diagnóstico del catálogo distribuido (**baseline aprobado por el dueño**): 1 error `DUPLICATE_ID` (`TROQUEL_TOPE`,
  pre-existente, **no** corregido) + 2 advertencias `UNRESOLVED_BLOCK_PIECE` (`TARIMA_GENERICA`); huella esperada
  `1a31c1a91f00a27130b5d8778eacc174adec1e818e78722e814174685e30df40` (90 bloques), fijada por `ShippedCatalogIntegrityTests`;
- validación manual: AutoCAD **no ejecutado ni requerido** (`requires_autocad: false`); **owner-validation aprobada**
  (baseline aceptado + catálogos/DWG intactos confirmados);
- invariantes preservados: **sin** cambios de catálogos (`git diff` vacío en `assets/catalogs/*`), **ningún** `.dwg`
  (el código nunca abre el DWG), **sin** cambios de geometría, BOM, persistencia ni reglas de producto; I-12, I-14 y su
  proyecto/gate `RackCad.UI.Tests` preservados; reconciliación: **único** conflicto en `docs/initiatives/README.md`
  (I-14 vs I-19), resuelto preservando ambas entradas;
- alcance: `src/RackCad.Application/Catalogs/` (`SeccionRoles`, `CatalogBlockParameters`, `Validation/*`, costura de
  `JsonRackCatalogProvider`), consolidación de nombres de parámetro en el dominio (`SelectiveRackDefaults`/
  `SelectiveSafetyDefaults`, `SelectiveSafetyPlacement`, `LateralHeaderParameters`, `DynamicSystemLateralBuilder`,
  `FlowBedLateralBuilder`), cinco clases de pruebas nuevas y contrato/estado/evidencia de I-19.

**Baseline integrada de I-12 — 2026-07-21:**

- punta de **código** validada por CI y por el dueño: `5d5f0dc650bad5aa9ef24b5a49d1d47a58acebd7`; el commit posterior de
  la rama (`5e62a42`) es **solo documental** (registro de la validación AutoCAD); este documento **no inventa** el SHA
  del merge de `main` (vive en `git log --first-parent main`);
- `origin/main` **no avanzó** desde la base rebaseada de I-12 (`abc1a53`, Merge I-14): **sin rebase final** en esta
  sesión; la rama se integra por `git merge --no-ff`;
- suite `RackCad.Tests`: **791/791 verdes** (sin regresión: I-12 no toca Domain/Application); suite `RackCad.UI.Tests`:
  **85/85 verdes** (I-12 solo elimina el `LangVersion`/`Nullable` duplicado del `.csproj`, ahora heredado);
- build UI Debug: **0 errores y 0 advertencias**; builds Plugin y solución completa Debug: **0 errores**, únicamente las
  dos familias `MSB3277` conocidas del Plugin;
- CI de rama verde sobre la punta `5e62a42` (run `29874100238`): los **cuatro** jobs —Tests (Domain+Application), Build
  UI, UI Tests (WPF controls, net8.0-windows) y Build Plugin without AutoCAD— en `success`; la guarda de ADR-0003 publica
  el Plugin y ejecuta `verify-bundle.ps1` (fail-closed) + el harness del verificador;
- objetivo entregado: **versión única** (`RackCadVersion`) en `Directory.Build.props` que alimenta ensamblados y
  manifiesto; **SHA estampado** reproducible en `InformationalVersion` (fallback definido sin git); `PackageContents.xml`
  **generado** desde plantilla; bundle por **`dotnet publish`** con `deploy/build-bundle.ps1` + `deploy/verify-bundle.ps1`
  fail-closed (DLL≡publish, catálogos≡`assets/catalogs`, versión/series, **cero DLL Autodesk**) y su harness
  `deploy/test-verify-bundle.ps1`; `install-bundle.ps1` usa el flujo verificado y **rechaza** `-Build`+`-SourceBundlePath`;
  **ADR-0004** (una serie a la vez, `SeriesMin = SeriesMax = R25.0`, solo AutoCAD 2025) **aceptado por el dueño**;
- reproducibilidad: dos `dotnet publish` del mismo commit → **inventario y hashes idénticos** (bundle determinista);
- validación manual: el dueño **aprobó la autocarga del bundle en AutoCAD 2025** (instalación en
  `%APPDATA%\Autodesk\ApplicationPlugins\RackCad.bundle`, autocarga sin `NETLOAD` **PASS**, `RACKCAD` **PASS**);
  evidencia en `docs/initiatives/I-12-autocad-validation.md`;
- invariantes preservados: **sin** cambios de producto, UI, catálogos, persistencia, handlers, geometría, BOM ni dibujo;
  ADR-0003 intacto (referencias Autodesk compile-only, cero DLL Autodesk en output/bundle/artifacts); **sin** dependencias
  NuGet nuevas; **sin tocar** `RackCad.sln` ni `.github/workflows/ci.yml`;
- alcance: `Directory.Build.props`/`Directory.Build.targets`, los cinco `.csproj` + `RackCad.UI.Tests.csproj` (rebase),
  `src/RackCad.Plugin/RackCad.Plugin.csproj` (target), `deploy/` (`build-bundle`, `verify-bundle`, `test-verify-bundle`,
  `install-bundle`, `test-install-bundle`, plantilla `PackageContents`, borrado el `.xml` estático),
  `eng/ci/verify-autocad-references.ps1`, `docs/guias/despliegue.md`, ADR-0004 + índice, y contrato/estado/evidencia de I-12.

**Baseline integrada de I-14 — 2026-07-21:**

- punta de **código** validada por CI: `cf8ee1faf7cc71849699a39024e4f709ee5b1cd3` (commit único de corrección de la
  ronda 2 de revisión); el commit posterior de la rama es **solo documental** (estado versionado + este cierre); este
  documento **no inventa** el SHA del merge de `main` (vive en `git log --first-parent main`);
- `origin/main` **no avanzó** desde la base de I-14 (`de72287`, Merge I-11): **sin rebase final**; la rama se integra
  por `git merge --no-ff` en esta sesión;
- suite `RackCad.UI.Tests`: **85/85 verdes** (lógica pura de los controles + instanciación STA de las vistas), sin
  fallos ni omitidas; suite `RackCad.Tests`: **791/791 verdes** (sin regresión: I-14 no toca Domain/Application);
- build UI Debug: **0 errores y 0 advertencias**; builds Plugin y solución completa Debug: **0 errores**, únicamente
  las dos familias `MSB3277` conocidas del Plugin;
- CI de rama verde sobre la punta de código `d8ed898` (run `29867946030`): los **cuatro** jobs —Tests
  (Domain+Application), Build UI, **UI Tests (WPF controls, net8.0-windows)** (nuevo) y Build Plugin without AutoCAD—
  en `success`; el commit de cierre documental no altera código y recibe su propio CI antes del merge;
- objetivo entregado: cinco controles WPF reutilizables en `src/RackCad.UI/Controls/` con lógica pura separada de la
  vista (`SelectionMatrix`+`SelectionMatrixModel`; `NumericField`+`NumericFieldValidation`; `CatalogCombo`+
  `CatalogComboSelection`; `PreviewCanvas`+`PreviewProjection`+`PreviewPalette`; base `RackDialogWindow`); el proyecto
  `tests/RackCad.UI.Tests` (`net8.0-windows`, runner STA propio **sin dependencias nuevas**) y su gate de CI `ui-tests`
  en `windows-latest`;
- **sin migración de ventanas** (patrón strangler): ninguna ventana existente cambia de comportamiento ni de
  apariencia; la adopción de los controles la harán I-15/I-20/I-21/I-22;
- invariantes preservados: **sin** cambios de geometría, recetas BOM, GUID, persistencia ni dibujo; **sin** tocar
  Domain, Application ni el Plugin (que sigue compilando: UI lo referencia transitivamente); **sin** dependencias
  NuGet nuevas; dirección de dependencias intacta (UI no referencia AutoCAD; los controles no dependen del Plugin);
- validación manual: AutoCAD **no ejecutado ni requerido** (`requires_autocad: false`; ROADMAP no marca I-14 con ✋);
  owner-validation **no requerida** (`requires_owner_validation: false`);
- alcance: producto en `src/RackCad.UI/Controls/` (10 archivos nuevos), pruebas en `tests/RackCad.UI.Tests/` (proyecto
  nuevo); modificados `RackCad.sln` (alta del proyecto), `.github/workflows/ci.yml` (job `ui-tests`, **sin tocar** el
  del Plugin) y `docs/initiatives/README.md`; contrato `docs/initiatives/I-14-ui-controls.md` y estado
  `docs/automation/state/I-14.yml`; sin cambios en Domain, Application, Plugin, catálogos ni deploy.

**Baseline integrada de I-11 — 2026-07-21:**

- punta de **código** validada por CI y por el dueño: `eea1c1113dd8a33e33fa31dd61720c24c844ad4f`; los commits
  posteriores de la rama son **solo documentales** (no alteran código); este documento **no inventa** el SHA
  del merge de `main` (vive en `git log --first-parent main`);
- `origin/main` no avanzó desde la base rebaseada de I-11 (`6e18874`, que ya incluye el fix posterior a I-10):
  sin rebase final adicional; la rama se integra por `git merge --no-ff` en esta sesión;
- suite `RackCad.Tests`: **791/791 verdes**, sin fallos ni omitidas (sobre `eea1c11`; incluye los 7 archivos
  de pruebas nuevos de persistencia y la caracterización existente);
- build UI Debug: **0 errores y 0 advertencias**; build solución completa Debug: **0 errores**, únicamente las
  dos familias `MSB3277` conocidas del Plugin;
- CI de rama verde sobre la punta de código `eea1c11` (los tres jobs: Tests, Build UI, **Build Plugin without
  AutoCAD**, en `success`); el commit de cierre documental no altera código;
- objetivo entregado: `FlowBedDocument`/`LargueroDocument` versionados + preservación de campos JSON
  desconocidos y de una versión de esquema **no degradada** en los cuatro límites (`RackEmbedDocument`,
  `RackProjectDocument` —incluido el diseño interior de los embeds dinámico/cabecera y los wrappers de
  biblioteca—, `FlowBedDocument`, `LargueroDocument`); `SchemaVersionPolicy` central; `RackEmbedComposer`
  puro; preflight discriminado (`ResolveInnerSource`/`PreflightInnerSources`:
  Success/BenignFallback/IncompatibleMajor/WrongKind) que **aborta la edición completa** ante un MAJOR interior
  incompatible o un `Kind` incorrecto (sin actualización parcial); transporte biblioteca↔DWG por sidecars de
  salida vía `RackMenuCommands` (transporte mínimo);
- invariantes preservados: geometría, recetas BOM, GUID y el **formato físico del Xrecord** (clave
  `RACKCAD_SELECTIVE`, chunk 255, `DxfCode.Text`) **intactos**; sin tocar `RackEnvelopeRestamp` ni el despacho
  por `Kind` de I-10; sin cambios en Draw Services, `RackBlockData` ni Domain; sin dependencias nuevas;
- validación en **AutoCAD 2025 aprobada** por el dueño (matriz §10 del contrato por `NETLOAD`, código
  `eea1c11`, incluidos B5, B6 y S7; `automation/evidence/I-11-autocad-validation.md`); **owner-validation
  aprobada**; gate `owner-decision` resuelto (`automation/decisions/I-11.md`);
- exclusión aprobada: `RackFrameProjectDocument` (biblioteca de cabecera desnuda por `RackFrameProjectStore`,
  quinto DTO) queda fuera de alcance por decisión del dueño; su preservación de desconocidos es deuda posterior;
- alcance: producto en `src/RackCad.Application/Persistence` (12 archivos), `src/RackCad.Plugin` (6, con
  `RackMenuCommands` solo como transporte) y `src/RackCad.UI` (5); 7 archivos de pruebas nuevos y el contrato
  de I-11; sin cambios en Domain, catálogos, deploy ni `.csproj`; sin dependencias nuevas.

**Corrección posterior a I-10 (`fix/kind-handler-missing-errors`) — 2026-07-21:**

- punta técnica revisada de la rama: `5fc631a9830024ce1535fe93a5322820d7e96dab`; este documento **no inventa**
  el SHA del merge de `main` (vive en `git log --first-parent main`);
- **corrección posterior**, no una reimplementación: I-10 permanece históricamente integrada en
  `c9f2d61ee14a1afe85d3d941080405371187670e`;
- `origin/main` no avanzó desde la base de la rama (`c9f2d61`): sin rebase final; se integra por `git merge --no-ff`;
- hallazgos corregidos: BOM parcial ante handler ausente (ahora preflight + abort de todo el comando), layout
  enlazado sin gate (ahora gate incondicional antes de abrir la ventana), restamp silencioso (ahora lanza),
  inmutabilidad del registro (ahora `ReadOnlyCollection` vía la `KindDispatch<T>` pura de Application), y
  cobertura de rutas negativas (`KindDispatch.TryResolveAll` puro + source-guards, sin cargar Autodesk — ADR-0003);
- suite `RackCad.Tests`: **718/718 verdes**, sin fallos ni omitidas; build UI Debug **0 errores y 0 advertencias**;
  builds Plugin y solución Debug **0 errores** (solo las dos familias `MSB3277` conocidas);
- CI de rama verde sobre `5fc631a` (run `29849342527`): los tres jobs (Tests, Build UI, **Build Plugin without
  AutoCAD**) en `success`;
- **validación manual en AutoCAD aprobada por el dueño** sobre el DLL Debug del worktree correspondiente a
  `5fc631a` (SHA-256 `0B39BDE316B9D861C19286C0911A2226433F4ED94CD0EFAD65607CBC9975FFE3`): confirmó que funcionó
  bien; la validación se conserva porque `origin/main` no avanzó (WORKFLOW §6);
- equivalencia mecánica: **26 `[CommandMethod]`** idénticos a `origin/main`, cero duplicados; **5** `Guid.NewGuid`;
  **7** ubicaciones de `Regen`; sin cambios en geometría, recetas BOM, formatos persistidos, catálogos, Draw
  Services ni referencias AutoCAD fuera del Plugin; sin dependencias nuevas;
- alcance: producto en `src/RackCad.Plugin` (`KindHandlers/` + los consumidores migrados) y
  `src/RackCad.Application/Persistence/KindDispatch.cs`; tests en `tests/RackCad.Tests`; sin cambios en
  Domain/UI/catálogos/deploy/csproj.

**Baseline integrada de I-10 — 2026-07-21:**

- punta de implementación revisada de `architecture/kind-handlers`:
  `532eb038306a3de68277496ce47457f270200944`; este documento **no inventa** el SHA del merge de `main` (vive
  en `git log --first-parent main`);
- `origin/main` no avanzó desde la base de I-10 (`c5a4082`): sin rebase final; la rama se integra por
  `git merge --no-ff` en esta sesión;
- suite `RackCad.Tests`: **694/694 verdes**, sin fallos ni omitidas (línea base y post-refactor idénticas;
  Domain+Application no se tocan);
- build UI Debug: **0 errores y 0 advertencias**; build solución completa Debug: **0 errores**, únicamente
  las dos familias `MSB3277` conocidas del Plugin;
- CI de rama verde sobre `532eb03` (run `29836270208`, `headSha 532eb038…0944`): los tres jobs (Tests,
  Build UI, **Build Plugin without AutoCAD**) en `success`;
- objetivo entregado: `IRackKindHandler` + `KindHandlerRegistry` en `src/RackCad.Plugin/KindHandlers/`
  (registro explícito, inmutable, sin reflexión, que rechaza handlers nulos, claves vacías y duplicadas;
  `TryGet` Ordinal + `TryGetIgnoreCase`) con los cuatro handlers embebidos (`selective`, `dynamic`,
  `cabecera`, `cama`) y `Default`; RACKEDITAR, RACKBOMTOTAL (`BuildRackBom` + `KindLabel`) y el restamp
  despachan por el registro; **0** `switch`/cadena por el `Kind` del sobre restante en alcance; `Larguero` no
  registrado;
- equivalencia mecánica: **26 `[CommandMethod]`** idénticos a `origin/main`, cero duplicados; **5**
  `Guid.NewGuid` (una en el restamp); **7** ubicaciones de `Regen`; mensaje "tipo de rack no reconocido" y
  etiquetas de BOM (Selectivo/Dinámico/Cabecera/Cama) **verbatim**; constantes de Kind/View intactas;
- frontera de registros: `SystemRegistry` (Application, I-08, `RackSystemKind`) y `KindHandlerRegistry`
  (Plugin, string del sobre) **no** se unifican; `RackListBuilder` (Application, RACKLISTA) queda intacto por
  la dirección de dependencias (Application no depende del Plugin);
- alcance: producto solo en `src/RackCad.Plugin` (6 archivos nuevos en `KindHandlers/` + 3 consumidores:
  `RackMenuCommands`, `RackInventarioCommands.BomTotal`, `RackEnvelopeRestamp`); documentación el contrato de
  I-10 y su índice; sin cambios en Domain/Application/UI/catálogos/deploy/csproj; sin dependencias nuevas;
- validación manual: AutoCAD **no ejecutado ni requerido** (`requires_autocad: false`; ROADMAP no marca I-10
  con ✋); owner-validation no requerida (`requires_owner_validation: false`), por analogía directa con I-09;
  no se declara AutoCAD validado.

**Baseline integrada de I-08 — 2026-07-21:**

- punta de implementación revisada de `architecture/system-registry`:
  `997fb8e459af11f0d42ac0eb13029cb8c4b287d3`; este documento no inventa el SHA futuro del merge de `main`;
- `origin/main` no avanzó desde la base de I-08 (`0849152`): sin rebase final; la rama se integra por
  `git merge --no-ff` en esta sesión;
- suite `RackCad.Tests`: **686/686 verdes**, sin fallos ni omitidas; incluye la caracterización golden
  F1–F1.1 (wire format PascalCase, schema `2.0`, cinco nombres de enum, nulos, legacy sin `Kind`,
  reconstrucción física, `kind` sin payload, degenerado, versión futura, string desconocido, `Kind: 999`,
  reglas laxas de cama/larguero, etiquetas/precedencia/orden/tolerancia) verde tras el refactor;
- build UI Debug: **0 errores y 0 advertencias**;
- build Plugin Debug: **0 errores**; únicamente las dos familias `MSB3277` conocidas;
- CI de rama verde sobre `997fb8e`: los tres jobs (Tests, Build UI, Build Plugin without AutoCAD) en
  success;
- objetivo entregado: `SystemDescriptor` + `SystemRegistry` en Application como fuente única de los cinco
  `RackSystemKind`; `RackProjectStore`, la validación genérica y `RackDesignLibrary` despachan por el
  registro; **`RackDesignKind` y `MapKind` eliminados por completo** (búsqueda global de `RackDesignKind`
  en `.cs`/`.xaml` = cero); sin `switch`/cadena por `RackSystemKind` en el store ni en la biblioteca; sin
  un segundo registro manual de los cinco sistemas;
- compatibilidad preservada: formato JSON, schema, nombres persistidos del enum, fallback legacy y
  etiquetas visibles idénticos; `RackSystemKind` intacto (sin renombrar ni reordenar);
- owner-validation **aprobada** por el dueño (checklist de biblioteca, cinco etiquetas, editores y
  cabecera legacy); AutoCAD no ejecutado ni requerido (`requires_autocad: false`); no se declara
  validación formal de geometría;
- alcance: producto en `RackProjectStore.cs`, `RackDesignLibrary.cs`, `RackMainMenuWindow.xaml.cs` y los
  tipos nuevos del registro (`SystemDescriptor`, `SystemRegistry`, `SystemRegistry.Default`); tests; el
  contrato de I-08 y su índice; sin cambios en `src/RackCad.Plugin`, `RackEmbedDocument`,
  `RackListBuilder`, DrawServices, DTOs, geometría, BOM ni catálogos; sin dependencias nuevas. **I-10 e
  I-16 fuera de alcance.**

**Baseline integrada de I-16 — 2026-07-21:**

- punta de rama integrada `f3a84bc44faf498c94dcd26b0d469f33e49a697a` (rebaseada sobre `origin/main` `549870b`
  tras integrarse I-08); integrada en `main` por **merge `--no-ff` `2c3bee734511740ab8636894c29a74687ab1cafd`**
  (primer padre `549870b`, segundo padre `f3a84bc`);
- suite `RackCad.Tests`: **694/694 verdes** sobre el árbol combinado (686 de `main`/I-08 + 8 golden de I-16),
  sin fallos ni omitidas (build local con el SDK de usuario);
- build UI Debug: **0 errores y 0 advertencias**; build Plugin Debug: **0 errores**, únicamente las dos
  familias `MSB3277` conocidas;
- CI de rama verde en los tres jobs (Tests, Build UI, Build Plugin without AutoCAD) sobre la punta `f3a84bc`;
- validación manual en **AutoCAD 2025 aprobada** por el dueño sobre el DLL Debug del worktree I-16 (build de
  la punta F4, SHA-256 `6AEF0F4D5A49B89F6F5AAA35D4E287715473641E81D379B4BC671B55CC52906B`), matriz por
  familia (selectivo, dinámico con `postIndex`/entrada-salida, cama, cabecera, cancelación del jig,
  persistencia) sin observaciones (registro en `initiatives/I-16-autocad-validation.md`); el avance por I-08
  no toca la superficie de dibujo, así que la validación se conserva (WORKFLOW §6);
- equivalencia mecánica: las siete fachadas `*DrawService` conservan firmas públicas; infraestructura
  compartida extraída (`RackCatalogLoader`, `BlockPlacement`, `ViewBlockDraw`, `SystemBlockWriter.ApplyRegen`);
  invariantes preservados (nombres de bloque y sufijos, mensajes, `postIndex`, `DynamicRackEnd`, all-loose,
  payload/GUID, BOM, geometría, persistencia y **7 ubicaciones efectivas de `Regen`**);
- alcance: producto solo en `src/RackCad.Plugin`; golden solo en `tests/RackCad.Tests`; documentación el
  contrato, la línea base y el registro de validación de I-16; sin cambios en Domain/UI/catálogos/deploy;
- diff del merge contra su primer padre (`549870b`): únicamente el alcance acumulado de I-16 (19 archivos);
  **I-08 permanece intacta** (`RackDesignKind` eliminado, `SystemRegistry` presente).

**Baseline integrada de I-09 — 2026-07-20:**

- punta de implementación revisada de `refactor/plugin-commands`:
  `09de768cc7dfabdd29e313b4d8798abd783ec4a9`; este documento no inventa el SHA futuro del merge de
  `main`;
- `origin/main` no avanzó desde la base de I-09 (`6136fcb`): sin rebase final; la rama se integra por
  `git merge --no-ff` en esta sesión;
- suite `RackCad.Tests`: **636/636 verdes**, sin fallos ni omitidas;
- build UI Debug: **0 errores y 0 advertencias**;
- build Plugin Debug: **0 errores**; únicamente las dos familias `MSB3277` conocidas;
- CI de rama verde sobre `09de768`: los tres jobs (Tests, Build UI, Build Plugin without AutoCAD) en
  success;
- equivalencia mecánica verificada: **26 `[CommandMethod]`** con nombres idénticos a `origin/main`,
  cero duplicados, **13 principales + 13 aliases** con los mismos destinos; conjuntos idénticos de
  literales de código (prompts, keywords, mensajes, `SetRejectMessage`, nombres de bloque), `case`
  por `Kind`, DrawServices, stores, colores ACI, `directOnly`/`forceValidity`; conteos iguales de
  `Regen` (7), `Guid.NewGuid` (5), purgas (6) y `catch`/`Report` (18); el tipo y los archivos
  `RackFrameCommands` quedaron eliminados;
- alcance: producto solo en `src/RackCad.Plugin`; documentación solo el contrato de I-09 y su índice;
  sin cambios en Domain/Application/UI/tests/catálogos/deploy; sin dependencias nuevas; sin
  `[assembly: CommandClass]`;
- AutoCAD: no ejecutado; no requerido por contrato al conservar comportamiento mediante equivalencia
  mecánica, builds y CI (ROADMAP no marca I-09 con ✋).

**Baseline integrada de I-13 — 2026-07-20:**

- punta técnica final `849dff931ac5055c955ea2371c2388ec279b74b4`, contenida en `main` por
  `773feea3732497e04746c45451eb1b4e775d8961`;
- suite `RackCad.Tests`: **636/636 verdes**, sin fallos ni omitidas;
- build UI Debug: **0 errores y 0 advertencias**;
- build Plugin Debug: **0 errores**; únicamente las dos familias `MSB3277` conocidas;
- CI de rama #64: tests, build UI y build Plugin without AutoCAD verdes; único artifact
  `rackcad-coverage-cobertura`, sin artifacts del Plugin ni material Autodesk;
- la validación post-merge #63 detectó que el job conservaba la condición temporal de la rama de
  promoción; I-13 la retiró antes de la limpieza para que el Plugin se compile en cada push;
- CI post-merge #65 verde sobre `773feea3732497e04746c45451eb1b4e775d8961`: ejecutó los tres
  jobs, incluido Build Plugin without AutoCAD, y publicó solo la cobertura Cobertura;
- las tres anotaciones de CI son la deprecación heredada de Node.js 20 en las acciones usadas;
- ADR-0003 aceptado con decisión I-29 B, matriz 14/14, rollback y nueva revisión obligatoria;
- AutoCAD: no ejecutado ni requerido porque la iniciativa cambia infraestructura de compilación y
  documentación, no dibujo ni runtime;
- evidencia experimental y respaldo pre-rebase conservados en las etiquetas
  `archive/i-13-experiment-final-4e084d2` y `archive/i-13-pre-rebase-a6febd2` antes de retirar las
  ramas y worktrees de I-13.

**Baseline integrada de I-26 — 2026-07-19:**

- punta de implementación validada de `refactor/test-catalog-ids`:
  `2cf3f12684dbe495403f0a16eeaa882e4873e3c6`;
- suite `RackCad.Tests`: **636/636 verdes**, sin fallos ni omitidas;
- guardián de catálogos canónicos: verde contra IDs, bloques/vistas, conexiones, relaciones
  esenciales, defaults, plantillas y constantes equivalentes de producto;
- build UI Debug: **0 errores y 0 advertencias**;
- cobertura local observada: **91.77 % de líneas** y **75.26 % de ramas** en `RackCad.Domain` y
  `RackCad.Application`; es evidencia, no un umbral contractual;
- CI de rama #40: verde sobre la punta validada, según confirmación del dueño; el artifact
  `rackcad-coverage-cobertura` fue descargado y contiene `coverage.cobertura.xml`;
- diff bajo `src/`, `assets/catalogs/` y `deploy/`: vacío; no hubo cambios de producto ni datos;
- AutoCAD: no ejecutado ni requerido para esta iniciativa de infraestructura de pruebas;
- el commit documental final de integración requiere su propio CI antes del merge y no se declara
  verde anticipadamente.

**Baseline documental de I-06 que lleva este merge — 2026-07-17:**

- punta validada de `docs/reestructura`: `39cd54189457e8737f08cf95dbf948bc2e564dd3`;
- suite `RackCad.Tests`: **635/635 verdes**, sin fallos ni omitidas;
- build UI Debug: **0 errores y 0 advertencias**;
- build Plugin Debug: **0 errores**; únicamente los `MSB3277` conocidos;
- `git diff origin/main --check`: limpio;
- documentación Markdown: **52 documentos**, **123 enlaces locales** y **0 enlaces rotos**;
- Context Packs: nueve IDs únicos, con rutas, globs, gates y exclusiones válidos;
- diff bajo `src/`: solo el comentario XML autorizado en `RackCommandReference.cs`;
- CI de rama: verde para `39cd54189457e8737f08cf95dbf948bc2e564dd3`, según la confirmación del
  dueño; la corrección administrativa posterior requiere repetir CI antes del nuevo merge;
- AutoCAD: no ejecutado ni requerido para esta iniciativa documental.

La baseline integrada anterior correspondía a I-04 (`8e52828` como punta de integración):

- suite `RackCad.Tests`: **635/635 verdes**, sin fallos ni omitidas;
- build UI Debug: **0 errores y 0 advertencias**;
- build Plugin Debug y Release: **0 errores**, únicamente las familias `MSB3277` conocidas;
- harness del instalador: **25/25 verificaciones** en rutas temporales;
- CI de I-04: Success sobre `f82a49f`.

La evidencia técnica de la rama I-06 se conserva bajo [automation/runs/](automation/runs/). Este
documento no inventa el SHA futuro del merge de `main`.

## 6. Preguntas abiertas

1. ¿La cantidad de parrilla debe poder variar por frente/nivel, o basta el valor global según el
   uso real?

## 7. Decisiones vigentes (registradas como ADR)

Las trece decisiones que esta sección conservaba temporalmente quedaron retro-documentadas y
**aceptadas** por el dueño el **2026-07-22** («Sí, apruebo») como **ADR-0006 a ADR-0018** (iniciativa
I-07). Ya no se conservan aquí: viven en [`docs/adr/`](adr/README.md), una decisión por ADR. La
correspondencia decisión → ADR está en el
[contrato de I-07](initiatives/I-07-adr-retroactivos.md) y el registro de aceptación en
[`docs/automation/decisions/I-07.md`](automation/decisions/I-07.md).

Decisiones posteriores, cada una en su ADR: **ADR-0019** a **ADR-0023**, y **ADR-0024** —fundación
Cantilever base–columna— **aceptada el 2026-07-29** con veredicto `OWNER_APPROVED_ADR_0024`. Es la primera
ADR del lado **consumidor** del catálogo neutral, y la primera de la Fase 6 aceptada **sobre el código** en
vez de sobre el dibujo, porque la iniciativa que la trae no dibuja nada. Las decisiones vinculantes del
Owner para **toda** la línea I-37 —troqueles y placas visuales dentro; cálculo y fabricación fuera; peso
diferido; pendiente del brazo parametrizable; frontal, lateral y planta obligatorias en el MVP— viven en
[`docs/automation/decisions/I-37.md`](automation/decisions/I-37.md).

**ADR-0030 — el fondo de Push Back es de la CELDA; el del frente es una envolvente derivada —
`aceptado` el 2026-08-23** (iniciativa I-41). El dueño la acepta **con el modelo tal como quedó
implementado**, tras validar manualmente en AutoCAD 2025 el DLL construido exactamente desde
`c41aee1b8bcbfc0d6fed7a38b8c4767538648cd2`, e **incluye expresamente la limitación declarada**: el corte
lateral **NO seccionado** no dibuja tarimas, por ser una envolvente y no una celda. Su contenido es
**inmutable** desde ahora; solo pueden cambiar su Estado y sus enlaces.

**ADR-0031 — el Push Back compuesto tiene UNA estructura física y DOS configuraciones funcionales —
`aceptado` el 2026-09-02** (iniciativa I-42). El dueño la acepta **con el modelo tal como quedó
implementado**, tras validar manualmente en AutoCAD 2025 el DLL construido exactamente desde
`077d35ad418615bed4c1d8375ea9cfc0de9fca24` con veredicto **8/8 escenarios**, e **incluye expresamente sus
seis limitaciones declaradas**, entre ellas la observación no bloqueante del escenario 2 (**CORRIDA GAP
STORAGE**), que queda registrada y **no** implementada. Su contenido es **inmutable** desde ahora; sólo
pueden cambiar su Estado y sus enlaces.
