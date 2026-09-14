# ADR-0036: RACKMIRROR es un espejo semántico por copia: reflexión canónica por kind, vistas admisibles y colocación sin escala negativa

- **Estado:** **propuesto**
- **Fecha:** 2026-09-12 (propuesto; borrador corregido con Proposal V2 y con Proposal V3 el mismo día, y con
  Proposal V4, Proposal V5, Proposal V6, Proposal V7, Proposal V8, Proposal V9 y Proposal V10 el 2026-09-13, y con
  Proposal V11 el 2026-09-14)
- **Decisores:** Mario Pérez, Owner del repositorio (**acepta o rechaza**; pendiente). La aceptación **no** es
  precondición de la caracterización (G3): se pide **después de G3**, si G3 no contradice materialmente el contrato (si
  lo contradice, se abre una Proposal V12), y **es precondición de G4**. Coordinador de I-52 y Arquitecto de I-52
  (consenso técnico **pendiente** sobre la Proposal); Claude (redacción)
- **Iniciativa relacionada:** I-52 — `feature/rackmirror-espejo-semantico`
  ([contrato](../initiatives/I-52-rackmirror-espejo-semantico.md), [Discovery](../initiatives/I-52-discovery.md),
  [Proposal V1](../initiatives/I-52-proposal-v1.md), [Proposal V2](../initiatives/I-52-proposal-v2.md),
  [Proposal V3](../initiatives/I-52-proposal-v3.md), [Proposal V4](../initiatives/I-52-proposal-v4.md),
  [Proposal V5](../initiatives/I-52-proposal-v5.md), [Proposal V6](../initiatives/I-52-proposal-v6.md),
  [Proposal V7](../initiatives/I-52-proposal-v7.md), [Proposal V8](../initiatives/I-52-proposal-v8.md),
  [Proposal V9](../initiatives/I-52-proposal-v9.md) y [Proposal V10](../initiatives/I-52-proposal-v10.md) (historial),
  [Proposal V11](../initiatives/I-52-proposal-v11.md), [decisiones](../automation/decisions/I-52.md))

> **Numeración.** Un número de ADR queda reclamado por su primera publicación observable en un ref remoto. Este ADR se
> publicó por primera vez con el número 0036 en `origin/feature/rackmirror-espejo-semantico`, commit
> `0fc7032bf15d03e7d478bbd9350f156708621c9d` (corrida de CI del push `34731908035`, creada el `2026-09-13T01:59:44Z`),
> sin publicación anterior de otro 0036 en ningún ref. ADR-0035 (I-50), ADR-0037 (I-53) y ADR-0039 (I-54), ya
> integradas, están aceptados en `main`; ADR-0038 (I-49) se aceptó en su rama y lo reemplazó ADR-0040, aceptado en la
> misma y reemplazado a su vez por ADR-0041, también aceptado allí; ADR-0042 (I-55) está propuesto en su rama. Los seis
> posteriores a 0036 se publicaron después y con otro número: no hay colisión. Antes de pedir la aceptación del Owner se
> vuelve a buscar 0036 en todos los refs; si apareciera una publicación anterior, este ADR se renumera antes de la
> aceptación. Una vez `aceptado` no se renumera, y dos ADR aceptados con el mismo número detienen el trabajo hasta que
> decida el Owner (Proposal V11 §16).

## Contexto

RackCad no tiene un comando para reflejar racks. El `MIRROR` nativo de AutoCAD solo refleja la **referencia**: la
deja con escala negativa y no toca el diseño embebido en la definición (ADR-0009). Como toda autoridad lee ese diseño
—`RACKEDITAR` y Actualizar (ADR-0010), el BOM consolidado y las variables de proyecto (ADR-0034)— el resultado se
«desespeja»: los textos y cotas del bloque se ven al revés, Insertar agrega vistas sin espejo, los lados
Izquierda/Derecha del editor contradicen el dibujo y `RACKDUPLICAR`/`RACKLAYOUT` propagan la escala negativa. No hay
reactores que lo corrijan.

Un espejo que conserve la coherencia entre diseño y geometría tiene que reflejar el **diseño authored** y regenerar
la geometría desde él. Pero cada vista de RackCad es una **proyección con punto de vista fijo** de un rack descrito
en su marco local, y una misma línea de espejo en la hoja no significa lo mismo en todas: en una frontal refleja el
eje de frentes, en una lateral la profundidad y en una planta ambos, módulo un giro de 180°. Existe un
contraejemplo mínimo verificable —un Selectivo con frentes de claro distinto y una bota solo en la cara cercana, con
su frontal y su lateral seleccionadas— en el que ningún diseño reflejado único reproduce las dos vistas con una
colocación de determinante positivo. El diseño authored es uno por `RackId` (ADR-0034 §9), así que la reflexión
tiene que ser **una por rack lógico**.

El código ya ofrece las piezas para hacerlo sin infraestructura paralela: la agrupación multi-rack y la identidad por
`Guid` de I-51, `Transform2D` en Application (refleja sin `Matrix3d`), constructores puros de planes por vista, y la
costura de I-47 G9.1 que prepara planes e importa bloques antes de una única transacción del llamador. Varios hechos del
código condicionan el diseño: solo el Selectivo guarda en el sobre un documento authored propio, y los demás kinds se
leen y escriben a través del dominio del proyecto; la definición de bloque tiene un `Origin` que forma parte de la
transformación efectiva; la importación de la biblioteca de bloques es de mejor esfuerzo y ocurre fuera de la
transacción del dibujo; algunos valores geométricos (el ancho de un claro) dependen de propiedades vinculadas a
variables de proyecto; varios builders anclan holguras gráficas a un solo lado (el tope del Selectivo y el tope
posterior de Push Back); y casi todas las piezas de frontal y planta son bloques dinámicos de una biblioteca sin
versionar ni metadato de simetría, cuyo estado (longitud, altura) puede cambiar de forma continua, a veces con una
variable de proyecto, y cuya apariencia depende de capas, colores, tipos de línea, grosores y escalas por entidad, a
menudo heredados por capa o por bloque. La ruta de esa biblioteca la configura el usuario, así que otra estación puede
usar otra biblioteca. La referencia del rack en el espacio modelo tiene además su propia presentación (capa, color, tipo
de línea, escala de tipo de línea, grosor, transparencia, estilo de trazado y visibilidad) y un lugar en el orden de
dibujo, y la definición de un bloque se interpreta con el contexto del dibujo que la contiene (modo de estilos de
trazado, unidades y escala de anotación). Los drawers no asignan explícitamente determinadas propiedades de las
referencias internas, anotaciones y cotas que crean (capa, color, tipo de línea y su escala, grosor, transparencia,
estilo de trazado y estilo de texto; una cota sin estilo con nombre usa el estilo de cota actual del dibujo): la
caracterización debe establecer qué mecanismo y qué valores efectivos aplica AutoCAD y qué observaciones consume cada
productor, igual que al redibujar con Actualizar. Lo que un objeto ocupa en el modelo puede salirse de su extensión
geométrica (máscaras de texto, marcos, anchos de polilínea), mientras que el grosor de línea que se ve en pantalla o en
papel no es una distancia del modelo: depende de la escala de visualización o de trazado y de las tablas de estilos de
trazado. Además, importar una definición de la biblioteca conserva los bloques anidados, las capas y los estilos
homónimos que ya existen en el dibujo, así que el tipo de línea que resulta para una entidad —explícito, por capa o
heredado por la capa `0` o por bloque— es el del registro del dibujo destino, que puede ser complejo (con formas o
texto) aunque el de la biblioteca no lo sea. Los objetos del dibujo que el espejo no crea pueden ser de cualquier clase,
con representaciones que dependen del estado actual de sus capas o del entorno (tamaño de los puntos, escalas de tipo de
línea, representaciones anotativas, métricas de las fuentes), y su lugar en el orden de dibujo persiste aunque hoy no se
vean. Los estilos de cota y de texto arrastran a su vez estilos de texto, bloques de flecha, tipos de línea y fuentes, y
la escala de anotación de un rack es un dato de su diseño, distinto de la escala de anotación actual del dibujo.

## Decisión

1. **Espejo semántico, no de entidades.** `RACKMIRROR` refleja el documento authored y **regenera** la geometría con
   las mismas autoridades de plan que usa el redibujo. Nunca refleja entidades ni persiste una escala negativa. La copia
   es fiel al plan regenerado del documento fuente, no a las entidades actuales de su definición.
2. **Solo copia.** Crea un rack nuevo y conserva los originales; no borra, no refleja en sitio y no conserva el
   `RackId` en la copia.
3. **Una reflexión semántica canónica por kind (`μ_k`)**, independiente de la línea y de las vistas seleccionadas.
   Primer corte: Selectivo = frentes (RUN), Dinámico y Push Back = frentes (RT), Cantilever = línea de estaciones
   (R_X), Cabecera = profundidad (R_D). La cama no tiene reflexión representable.
4. **Admisibilidad por vista con sección canónica.** Cada kind declara qué vistas **exponen** `μ_k`: su plan reflejado
   coincide con el plan original transformado por una reflexión local `F` de la vista. Primer corte: frontal y planta
   para Selectivo, Dinámico, Push Back y Cantilever; lateral y planta para Cabecera; ninguna para Cama. La `View` y la
   `Section` del sobre se decodifican como las lee producción: el legado inequívoco que alguna versión pudo escribir y
   que producción ya soporta se escribe en su **forma canónica** (la frontal Selectiva con `−1` pasa a fondo `0`); lo
   que no es inequívoco, lo que ninguna versión pudo escribir (una `View` vacía del Selectivo con `Section ≥ 0`) o lo que
   producción rechaza falla cerrado.
5. **Fail-closed con tres dimensiones separadas.** Cada propiedad de un rack concreto tiene una **clase de
   representabilidad** (`REPRESENTABLE`, `REPRESENTABLE_BY_NORMALIZATION`, `REQUIRES_MODEL_CHANGE`, `UNKNOWN`), un
   **estado de evidencia** (demostrada en código, pendiente de caracterización, verificada) y una **disposición
   operativa** (continuar, canonizar, continuar con fidelidad limitada a la línea base declarada del store, fallar
   cerrado). Si una referencia seleccionada no es admisible, o alguna propiedad es `REQUIRES_MODEL_CHANGE` o `UNKNOWN`
   material, la operación entera termina **antes** de mutar. No hay prompt de eje semántico. Lo que depende de anclajes
   de los builders queda pendiente de caracterización; una contradicción reabre la Proposal, no se parchea. Al cerrar la
   caracterización **ninguna** propiedad puede quedar pendiente: cada una queda verificada o reclasificada `UNKNOWN` o
   `REQUIRES_MODEL_CHANGE` con fallo cerrado, y una reclasificación que cambie materialmente el alcance, este ADR, una
   regla de reflexión o la arquitectura abre una Proposal V12. Una holgura gráfica anclada a un lado ya demostrada en
   código (tope del Selectivo, tope posterior activo de Push Back) es `UNKNOWN` y falla cerrado hasta que se apruebe una
   regla explícita.
6. **Reflectores puros en Application sobre el sustrato real de cada kind**, despachados por kind fuera del comando.
   El Selectivo se refleja sobre su documento authored (`SelectivePalletDesignDocument`), sin `WithDesign` y sin
   materializar vínculos. Dinámico, Push Back, Cantilever y Cabecera se leen con la autoridad existente, se reflejan en
   su dominio, se reconstruyen con las fábricas de `RackProject` y se escriben con el mismo store, llevando los
   metadatos con `WithSourceMetadataFrom`. No se crean DTO para homogeneizar, no cambia el schema y no hay manipulación
   de JSON para escribir.
   - **Fidelidad del store.** El espejo no pierde nada que el store del kind conserva; la comparación es contra el store
     y **no** contra el camino de `RACKEDITAR`, cuyos defectos históricos no autorizan pérdidas del espejo.
   - **Normalizaciones.** Una normalización solo es válida si conserva la física, el BOM, las variables y los portadores,
     con round-trip e involución demostrados, declara el cambio de representación authored y **conmuta con la resolución
     efectiva para todo estado futuro de las propiedades vinculadas**. Queda **prohibida** toda normalización o regla que
     escriba en el documento un valor calculado a partir de una propiedad vinculada: el caso es `REQUIRES_MODEL_CHANGE`.
   - **Estabilidad dinámica.** Toda decisión de continuar o canonizar cuyo predicado dependa directa o transitivamente
     de una propiedad vinculable (ajuste, desborde, ancho gobernante, espacio de índices, cantidad o posición) conserva su
     verdad para **todo** estado acreditable futuro del registro, incluido el **estado dinámico** de las piezas cuya
     simetría se acepta (decisión 13); si no puede demostrarse, falla cerrado. Una autoridad explícita de dependencias
     declara, por regla, sus entradas, los descriptores vinculables de los que dependen, su predicado y la prueba o el
     motivo del fallo cerrado.
   - **Metadata desconocida.** Se distingue la metadata **portadora**, nombrada en una **lista cerrada** cuyo contrato es
     conservarse exactamente (versiones del sobre y del envoltorio, valores de propiedades vinculables, `DimensionViews`,
     propiedades de rack de I-54), de la metadata **semántica** del payload que se somete a `μ_k` y de la metadata
     **exterior** del sobre y del envoltorio. Un miembro desconocido no vacío en el payload, o una clave desconocida no
     vacía del sobre o del envoltorio que no esté en la lista, es `UNKNOWN` y falla cerrado, salvo declaración tipada de
     invariancia o de transformador. Ningún contenedor es portador por ser contenedor. Los valores de propiedades
     vinculables son portador de almacenamiento, pero cada propiedad conocida tiene un descriptor con efecto semántico
     que se clasifica en la cobertura y en la autoridad de dependencias.
   - **Contrato de serialización real.** «Conocido», «desconocido», «vacío» y «colisión» se deciden con el contrato que
     producen las mismas opciones de serialización de cada store (modificadores, conversores, coincidencia sin
     distinguir mayúsculas, miembros ignorados y de extensión), no con la reflexión de tipos. Fallan cerrado: dos claves
     que esa coincidencia asigna al mismo miembro, un entero de enum no definido (salvo invariante compatible declarado y
     probado) y un **miembro retirado** de una versión anterior. El espejo no limpia datos automáticamente: si el store
     actual descarta el miembro retirado, el mensaje pide abrir el rack con `RACKEDITAR` y Actualizar antes de reflejarlo;
     si el store lo conserva, el mensaje no promete remedio.
   - **Cobertura.** Un **cierre transitivo** calculado sobre ese contrato desde los tipos raíz del sobre, del envoltorio y
     de cada kind clasifica exactamente una vez cada miembro alcanzado, cada propiedad vinculable y cada valor de los
     enums alcanzados; un tipo, miembro, valor o descriptor nuevo sin clasificar detiene el trabajo. La guarda, no una
     lista manual, es la autoridad.
7. **Autoridad pura de planes por vista en Application** y un **materializador genérico en el Plugin** que crea la
   definición dentro de la transacción del llamador, escribe el payload preparado y crea la referencia con la colocación
   ya calculada. La autoridad recibe el nombre lógico de la copia, el estado efectivo y la vista y sección canónicas. Su
   convergencia con los productores de planes existentes (`RACKEDITAR` → Actualizar, `RACKEDITAR` → **Insertar**, el
   ejecutor de variables y el paso del estado del editor al sistema) debe demostrarse **antes** de que el comando la
   consuma; si Actualizar e Insertar comparten costura, una guarda debe demostrar que ambos la siguen usando. El comando
   no conoce Left/Right, A/B, estaciones ni reglas de cabecera. Una pieza sin bloque en el dibujo es **fallo duro** para
   el espejo: nunca se crea una copia incompleta. Las referencias internas, anotaciones y cotas de la definición nueva
   se crean con las mismas observaciones del **contexto de materialización** con que se evalúan las dos generaciones del
   plan, como al redibujar con Actualizar: los drawers no asignan explícitamente determinadas propiedades, y la
   caracterización establece qué mecanismo aplica AutoCAD y qué observaciones consume cada productor (entre otras, el
   estilo de cota actual con el estilo de texto, los bloques de flecha y los tipos de línea que use, el estilo de texto
   con sus fuentes o las capas de anotación; la escala de anotación del rack es intención del plan, y la del dibujo solo
   cuenta si la caracterización demuestra que se consume). Solo invalida la operación un estado que el productor o la
   huella visual consumen y que cambia antes de mutar; un estado no consumido no invalida, y un consumo que no puede
   determinarse falla cerrado. La copia no clona la presentación histórica de las referencias internas del bloque
   fuente, un productor que aplique ese contexto de forma asimétrica falla cerrado y los drawers existentes no cambian.
   Una guarda estructural vigila a esos productores: si alguno empieza o deja de asignar una propiedad visual, cambia el
   uso de capas, estilos o valores por defecto, crea una clase de entidad relevante nueva o cambia la resolución de
   estilos, falla hasta que se actualicen la caracterización, las observaciones consumidas y las pruebas.
8. **Colocación canónica.** Con la reflexión de la hoja `G`, la colocación efectiva de la fuente
   `P = T(p)·R(θ)·S(s,s)·T(−o)` —donde `o` es el `Origin` de su definición— y la reflexión local de la vista `F`, la
   referencia nueva es `P' = G·P·F`, con determinante positivo y la escala uniforme de la fuente. Para una vista
   admitida, `P'·Π(μ_k D) = G·P·Π(D)` para todo estado del registro: la geometría física coincide con el espejo
   geométrico, también tras cambiar una variable, y los textos y cotas se regeneran legibles. Esa igualdad solo se
   afirma si cada decisión que la autoriza —incluidos los predicados de ajuste, desborde, ancho e índices y el estado
   dinámico de las piezas cuya simetría se acepta— es estable para todo estado acreditable del registro (decisión 6,
   estabilidad dinámica). Las definiciones nuevas se crean con
   `Origin = 0`.
9. **Identidad nueva.** Un `NewRackId` por grupo lógico, compartido por sus vistas seleccionadas; el mapa
   `OldRackId → NewRackId` vive solo en memoria; el diseño se refleja **antes** de componer el sobre con el sobre fuente
   real y de re-estampar la identidad con la entrada única de I-51.
10. **Portadores intactos, por nombre.** Los portadores de la lista cerrada —`SchemaVersion` del sobre y del envoltorio,
    `PropertyValues` (incluidos tipos desconocidos), literales congelados, `VariableId`, **`DimensionViews`** (I-50,
    integrada: requisito normativo, con el entero exacto) y las propiedades de rack de I-54— viajan desde el origen;
    cualquier otra clave desconocida no vacía del sobre o del envoltorio falla cerrado. Las propiedades personalizadas
    de alcance Proyecto, guardadas a nivel de dibujo, no son un portador del rack: son un alcance separado del de Rack,
    y el espejo no las lee para reflejar el rack, no las copia, no las refleja, no las transforma ni las escribe. El
    espejo no crea, modifica, desvincula ni materializa variables y no escribe el registro. Para dibujar consume en
    **solo lectura** la autoridad efectiva existente, con su mismo criterio de acreditación del registro: un registro no
    acreditable o un vínculo roto fallan cerrado solo para los kinds cuya autoridad consume el registro. Un cambio de
    valor de una variable no puede deshacer la equivalencia reflejada **ni invalidar ninguna decisión de continuar o
    canonizar** tomada antes de mutar, incluida la aceptación de un cambio de mano cuya evidencia dependa de un
    parámetro dinámico vinculado. Una cota inferior del valor de una propiedad vinculada solo se usa para demostrarlo si
    la define una autoridad real de resolución o de dominio, nunca un control de la interfaz.
11. **Fuente canónica.** Solo `BlockReference` de Model Space, no MINSERT, con `Normal = +Z`, rotación finita y escala
    uniforme positiva (`(−s,−s)` se canoniza a `(s,s)` + π), definición con **`Origin = 0`** y `View`/`Section`
    decodificables. Escala no uniforme, una sola componente negativa, `sz < 0`, una definición con BASE movida o una
    referencia **dinámica, anónima o anotativa** que lleve datos de RackCad fallan cerrado con mensaje propio; nunca se
    ignoran como si no fueran racks. Primero se reconoce si un objeto lleva datos de RackCad: los objetos que no los
    llevan —aunque tengan escala, rotación, origen o tipo no canónicos, o contengan racks anidados— se ignoran con aviso
    y nunca abortan la operación; las restricciones estrictas solo se aplican a los candidatos RackCad. Un candidato
    RackCad fuera de Model Space no entra como fuente: se ignora con aviso y no aborta (decisión deliberada, en
    continuidad con `RACKDUPLICAR`). La línea se captura en el UCS con Z paralela a la de WCS y se convierte a WCS. Las
    tolerancias son las absolutas del repositorio; no se introduce tolerancia relativa. La referencia nueva **conserva
    la presentación exterior de su referencia fuente** —capa, color, tipo de línea, escala de tipo de línea, grosor,
    transparencia y visibilidad, y el estilo de trazado cuando el dibujo usa estilos con nombre; con estilos
    dependientes del color, el estilo sigue al color y no se asigna aparte—; una fuente con un estado de presentación
    que no se puede transportar, que no está caracterizado o que queda fuera del inventario cerrado (un recorte
    espacial, atributos u otra presentación desconocida) falla cerrado antes de mutar, nunca con un simple aviso. Las
    copias se crean en el orden de dibujo relativo de sus fuentes y, si la huella visual de una copia se superpone de
    forma material, o de forma no clasificable, con la de **cualquier** objeto del espacio modelo que en ese orden va
    después de su fuente —otra fuente seleccionada, un objeto seleccionado pero ignorado o uno no seleccionado—, el
    orden no puede conservarse y la operación falla cerrado antes de mutar; la fuente propia es la única excepción, y su
    copia puede quedar encima de ella. Si dos fuentes admitidas tienen un orden relativo distinto en pantalla y en
    trazado y sus copias se superponen de forma material o no clasificable, ningún orden de creación conserva los dos y
    la operación falla cerrado. La huella visual es la **ocupación de modelo** del objeto en el sistema de coordenadas
    universal: la geometría ocupada completa, anchos reales incluidos, se transforma con la transformación compuesta
    —con escala uniforme, los anchos se multiplican por su valor absoluto, y una transformación anidada o no uniforme
    que no se soporte exactamente cuenta como no clasificable—, sin dilatarla por grosores de línea de pantalla o de
    papel, tablas de estilos de trazado ni escala de trazado; por eso el orden entre dos objetos que solo se tocan por
    el grosor de línea visible no se garantiza. Para los objetos existentes del dibujo, una **política cerrada por tipo
    exacto** decide si su huella está soportada y con qué método, sin ninguna suposición optimista: una clase, un
    componente o un estado no caracterizado cuenta como no clasificable, y la extensión geométrica solo sirve en las
    clases donde la caracterización demuestre que acota de forma conservadora. La huella de orden es la ocupación
    **latente**: no descarta geometría porque hoy su capa esté apagada, inutilizada o inutilizada en una ventana
    gráfica, el objeto sea invisible o una representación anotativa no se muestre, porque el orden persiste si eso
    cambia; la ruta de visibilidad sigue rigiendo solo la equivalencia de las piezas (decisión 13). Una representación
    que depende del entorno (tamaño de los puntos, escalas de tipo de línea, representaciones anotativas, métricas de
    las fuentes) solo está soportada si la caracterización demuestra una ocupación conservadora bajo todos los estados
    que admite el contrato. Por eso una línea de construcción infinita o un rayo sin huella analítica caracterizada, un
    objeto posterior de una clase no soportada o un objeto dependiente del entorno sin huella conservadora hacen fallar
    la operación aunque estén lejos de la copia. La cola de objetos posteriores a cada fuente se enumera de nuevo antes
    de la decisión definitiva y al empezar a mutar: si se añadió, eliminó o reordenó un objeto, cambió la identidad o la
    clase de alguno, o alguno pasó de estar antes a estar después de su fuente, la operación falla cerrado o se
    revierte; un objeto que estaba antes de su fuente y sigue antes no la invalida. La decodificación de
    `View`/`Section` y la taxonomía de tipo de vista que usa esta decisión, igual que la autoridad de planes desde el
    sistema resuelto, el comparador authored, el primitivo de materialización y las transformaciones de colocación y
    proyección (decisiones 4, 7, 8 y 14), son autoridades que también necesita I-55: su propiedad se reconcilia con I-55
    antes de congelar el contrato, sin duplicarlas, mientras que `μ_k`, las huellas visuales, la evidencia de simetría y
    el orden en el espacio modelo siguen siendo propios de este ADR.
12. **Atomicidad semántica, no de infraestructura.**
    `ACQUIRE → SNAPSHOT → PREFLIGHT (un solo orden normativo) → LINE → PREPARE → MUTATE → COMMIT`, con una sola
    transacción de escritura para todas las definiciones, payloads y referencias: cualquier fallo revierte todas las
    copias. Una huella de cada referencia fuente (identidad, transformación, presentación, estado no transportable,
    orden de dibujo, cola completa de los objetos posteriores con sus huellas de orden, observaciones consumidas del
    contexto de materialización con su cierre transitivo, `Origin`, banderas de bloque y payload) y las observaciones
    del registro que la resolución efectiva realmente usó (acreditación, variables leídas y, si la autoridad de
    expresiones está integrada, sus dependencias transitivas; nunca una «versión» o una huella del registro completo) se
    re-verifican antes de mutar. La importación de la biblioteca de bloques en PREPARE es **de mejor esfuerzo**: ocurre
    fuera de esa transacción, puede arrastrar dependencias (bloques anidados, capas, estilos), puede quedar parcial y
    puede **permanecer** aunque la operación falle. PREPARE re-verifica los bloques requeridos y la definición real que
    quedó en el dibujo para las piezas cuya equivalencia visual-geométrica se aceptó (decisión 13); después de importar
    relee el dibujo real, vuelve a calcular las huellas visuales de toda instancia cuyas entradas cambiaron —tenga o no
    cambio de mano— y vuelve a evaluar el orden entre piezas y el orden en el espacio modelo, con la cola de objetos
    posteriores enumerada de nuevo, antes de mutar, y al empezar a mutar vuelve a enumerar esa cola: la evaluación
    anterior a la importación solo sirve para rechazar de forma anticipada. El comportamiento del UNDO sobre lo
    importado es desconocido hasta la validación del Owner y no se promete.
13. **Mano y simetría de bloques, con equivalencia visual-geométrica evaluada por estado.** Ningún bloque DWG se asume
    simétrico. Una diferencia de mano entre el plan reflejado y el plan original transformado solo es equivalente si
    pasan, en este orden, dos evidencias:
    - **Evidencia visual-geométrica evaluada por estado (`GeometricEvidence`).** Para cada pieza de cualquier vista
      admisible que necesite aceptar un cambio de mano se evalúa la pieza real en el **estado concreto** que usa: la
      definición real del bloque, clonada en una base de datos auxiliar privada y descartable que reproduce el contexto
      necesario de la autoridad origen (modo de estilos de trazado, unidades, escala de anotación y lo que descubra la
      caracterización; si no se puede reproducir, la pieza falla cerrado); su vector de parámetros
      dinámicos, aplicado con **exactamente la misma semántica que el materializador** (nombres sin distinguir
      mayúsculas y la última clave duplicada ganando, propiedades de solo lectura sin escribir, nombres ausentes
      ignorados y recomputación gráfica solo si se aplicó alguna propiedad); y, leídos después del **punto de
      observación** que fije la caracterización, los valores efectivos, la definición evaluada, su `Origin` y la
      **transformación efectiva** de la referencia (posición, rotación, escalas, normal y transformación de bloque). Si
      una acción dinámica altera la referencia de un modo no representado, la pieza falla cerrado. La pieza se aplana
      recursivamente —bloques anidados con su transformación completa, su propio estado y su bloque evaluado; un ciclo
      falla cerrado.
    - **Apariencia por fuentes simbólicas tipadas.** Cada propiedad visual (color, tipo de línea, escala de tipo de
      línea, grosor, transparencia y estilo de trazado) se compara por su **fuente simbólica tipada** según las reglas
      de AutoCAD —valor explícito con su método y su valor, por capa con nombre, heredada por la capa `0`
      (`InheritLayer`) o heredada por bloque (`InheritBlock`)—, **nunca** por el valor que resuelve hoy: una mitad
      `ByLayer` y otra con el mismo color explícito no son equivalentes, ni lo son una herencia por capa y una por
      bloque, ni un color indexado y un color verdadero del mismo aspecto, ni dos colores del mismo libro y nombre con
      distinto valor almacenado. Esa igualdad simbólica **no** decide si la representación **resuelta** está soportada:
      es una pregunta separada, que se resuelve en el **dibujo destino** —con el dibujo por encima de la biblioteca—
      siguiendo el valor explícito, la capa con nombre, las herencias por la capa `0` o por bloque, la presentación con
      que se crea la referencia de pieza y la presentación exterior copiada; si el tipo de línea efectivo es complejo
      (con formas o texto), no se puede determinar o no está caracterizado, la pieza falla cerrado aunque su fuente sea
      por capa o heredada o su nombre explícito coincida con uno complejo del dibujo, y los registros resueltos que lo
      deciden forman parte de las entradas registradas. Las herencias usan rutas canónicas **estables** relativas a la
      raíz de la pieza, que identifican cada anidado por su definición base, sus valores dinámicos efectivos y un
      ordinal por contenido, y nunca por nombres anónimos generados ni por identificadores de la base de datos. La
      escala de tipo de línea forma siempre parte de la firma y, en los trazos sin continuidad demostrable, también las
      escalas y las escalas de tipo de línea de sus ancestros. La **ruta de visibilidad** de cada primitiva (visibilidad
      y fuente de capa de cada referencia ancestro y de la propia primitiva) debe coincidir en las homólogas, de modo
      que ningún estado de capas o de visibilidad pueda mostrar una mitad y ocultar la otra; un mecanismo de visibilidad
      no modelado —incluidos los objetos anotativos— falla cerrado. Donde dos primitivas se superponen de forma
      visualmente material (rellenos, trazos coincidentes con firmas distintas, entidades transparentes), el orden
      visual relativo de las homólogas se conserva o la pieza falla cerrado; lo mismo rige entre instancias de pieza
      distintas de una vista, tengan o no cambio de mano: cuando su orden relativo cambia en el diseño reflejado y sus
      huellas visuales se superponen de forma material, o no puede demostrarse que no lo hagan, la vista falla cerrado.
    - **Correspondencia conservadora.** La simetría se prueba respecto del eje que derivan `F` y la rotación de la
      instancia (una rotación oblicua falla cerrado) y de un centro candidato, con una correspondencia uno a uno entre
      **primitivas canónicas** de la misma firma, incluida su clase de origen: una polilínea nunca se empareja con líneas o
      arcos sueltos, y su cierre y la generación de su patrón de tipo de línea forman parte de la firma. La canonización es
      conservadora: solo se fusionan trazos con continuidad
      demostrable por su fuente; sin ella se conservan segmentación, orientación, escala y multiplicidad; los sólidos y
      trazos se comparan por el polígono que AutoCAD representa y los sombreados sólidos por sus bucles, los tipos de sus
      bucles y su estilo de islas; nunca se fusionan familias distintas. La caja envolvente nunca es prueba: una pieza asimétrica con caja
      simétrica falla, y una pieza geométricamente simétrica con fuentes visuales o rutas de visibilidad distintas también.
    - **Política cerrada de clases y variantes.** Cada clase soportada lo es solo con la variante y los atributos de
      representación que la política enumera; **cualquier otro atributo, variante o subclase se trata como no soportado
      y falla cerrado**, y la clase se comprueba por su tipo exacto, nunca por herencia. En el primer corte se evalúan
      líneas, arcos y círculos sin grosor y con normal +Z; elipses y splines planas en el plano de la vista; polilíneas
      con bulges, sin ancho, sin grosor y con normal +Z; sólidos 2D y trazos sin grosor y con normal +Z; sombreados
      sólidos uniformes no degradados, con normal +Z, con la elevación que admita la caracterización y con bucles
      canonizables por sus aristas y sus tipos; y referencias anidadas de tipo exacto, sin atributos, sin inserción
      múltiple, no anotativas, sin recorte espacial y con una definición que no procede de una referencia externa. Falla
      cerrado todo lo demás: entre otros, tipos de línea complejos resueltos en el dibujo destino, polilíneas con ancho,
      grosor o normal distinta, regiones, sólidos 3D, entidades proxy o personalizadas, imágenes, OLE, puntos,
      polilíneas 2D y 3D de estilo antiguo, multilíneas, caras, wipeouts, directrices, tablas, formas, textos,
      atributos, cotas y sombreados de **patrón** o de **degradado**; igual falla una correspondencia ambigua. La
      completitud se establece con un **inventario cerrado**: la caracterización debe descubrir, por tipo exacto, las
      propiedades y el estado que expone AutoCAD 2025 y clasificar cada uno como transportado, de colocación, de
      contexto común, no visual o no soportado; lo no clasificado falla cerrado.
    - **Huellas visuales de ocupación de modelo.** Además de la evidencia de simetría, el espejo obtiene para cada
      instancia que pueda intervenir en el orden visual —tenga o no cambio de mano— una huella que sobre-aproxima lo que
      ocupa en el modelo (soporte de sus trazos, anchos geométricos, rellenos, máscaras con tamaño en el modelo, límites
      de texto y de cota, marcos y contenido anidado), construida después de transformar al sistema de coordenadas
      universal y sin dilatarla por grosores de línea de pantalla o de papel, tablas de estilos de trazado ni escala de
      trazado; el grosor de línea sigue formando parte de la firma visual, pero no añade área. Las huellas se evalúan en
      el **dibujo destino**: una definición que ya existe es la del dibujo, y una que falta se evalúa simulando su
      importación con el dibujo primero y la biblioteca después, sin sustituir lo que ya existe; una colisión de nombres
      que no pueda simularse deja la huella como no clasificable. Cada huella registra sus entradas (definiciones
      reales, registros de símbolos resueltos, incluidos los que deciden la elegibilidad de la presentación, contexto
      consumido con su cierre transitivo, estado dinámico y dependencias anidadas) y, tras importar, se recalcula si
      alguna cambió. Una huella que no está disponible, es infinita o no puede garantizarse conservadora cuenta como
      superposición no clasificable. La huella demuestra ocupación y superposición; la simetría la sigue demostrando la
      evidencia visual-geométrica.
    - **Evidencia de colocación (`PlanPlacementEvidence`).** Después, la conmutación de la vista debe demostrar que los
      builders colocan la pieza de forma coherente con el centro verificado; nunca ajusta, infiere ni corrige el centro.

    La definición evaluada es la del dibujo si el bloque ya existe y, si no, la de la biblioteca candidata; tras
    importar, PREPARE verifica de nuevo la definición real que quedó en el dibujo, porque la importación conserva una
    definición local con el mismo nombre. Cuando una base auxiliar reúne definiciones del dibujo y de la biblioteca,
    primero se clona lo que ya existe en el dibujo y después solo lo que falta en la biblioteca, sin sustituir lo
    existente; una base auxiliar por pieza solo vale si reproduce exactamente la misma resolución global. La evaluación
    no muta el dibujo del usuario (el dibujo queda intacto y la base de datos de trabajo restaurada, también ante
    excepción) ni la base de datos cacheada de la biblioteca, a la que solo se accede con la API pública que clona
    definiciones hacia la base auxiliar —nunca por reflexión—; las definiciones que ya están en el dibujo se clonan
    desde él en solo lectura, y el valor que devuelve el clonado no es evidencia: tras el intento se verifica que
    existen todos los bloques requeridos; entrega datos planos a Application, que no depende de AutoCAD, y su resultado
    no se persiste.

    Una huella de la pieza evaluada —valores efectivos, clases y variantes, geometría aplanada, fuentes simbólicas
    tipadas, rutas estables, escala y contexto de patrón, visibilidad, orden visual material, transformación efectiva,
    contexto de la base auxiliar y registros de los que depende la elegibilidad de la presentación, en serialización
    canónica y nunca en el orden de iteración de la definición— sirve solo para trazabilidad, detección de obsolescencia
    y enlace entre la verificación previa y PREPARE; **no** es evidencia de simetría ni sustituye a las entradas
    registradas de la huella visual. La evidencia registrada de una biblioteca concreta se identifica por el **SHA-256**
    de su contenido y no se reutiliza si cambia; para una definición ya presente en el dibujo manda su huella efectiva.
    No se usa una forma afín del centro sobre un parámetro dinámico ni se interpretan los grafos de acciones de los
    bloques dinámicos. Si un parámetro dinámico que la evidencia necesita depende directa o transitivamente de una
    propiedad vinculable, la evidencia de un solo estado **no** basta: se exige una prueba sobre todo el dominio
    autoritativo del parámetro, una autoridad integrada que garantice la equivalencia para todos sus estados o la
    demostración de que el parámetro no depende del vínculo; si no, falla cerrado, y nunca se infiere universalidad a
    partir de estados de muestra. Nunca se infiere un centro de la diferencia entre planes, de la caja envolvente, de la
    diferencia de inserciones ni de un mínimo de error; por eso ninguna evidencia puede hacer equivalente una pieza con
    holgura u offset anclado a un lado, cualquiera sea su rol, y el rechazo de las familias con holgura `UNKNOWN` (hoy
    los topes) es una defensa adicional. Ninguna holgura, offset gráfico o parámetro con semántica de lado se asume
    simétrico: sin regla, falla cerrado. La caracterización **debe demostrar**, fuera del código de producción y en
    AutoCAD 2025 (`acad.exe`), la viabilidad de la evaluación, su paridad con el materializador, el punto de
    observación, el contexto de la base auxiliar, la política de variantes, las reglas de fuentes tipadas, visibilidad y
    orden visual dentro de la pieza y entre piezas, la presentación y el orden de las referencias, la elegibilidad de la
    presentación resuelta en el dibujo destino, las huellas visuales de ocupación de modelo en el dibujo destino con la
    transformación de sus anchos y su recálculo tras importar, la política de huellas por tipo exacto de los objetos del
    dibujo con su independencia de la visibilidad y del entorno, la cola completa de objetos posteriores, el contexto de
    materialización que consume cada productor con su cierre transitivo y la postcondición del dibujo y de la caché; la
    evaluación productiva nace en el Plugin durante la implementación. La confirmación visual del Owner es confirmación,
    nunca la única prueba.
14. **Verificación dinámica por rack sobre todas las vistas admisibles.** Antes de pedir la línea, cada rack lógico
    verifica sobre su diseño reflejado: la ausencia de metadata semántica desconocida en el payload y en el exterior; la
    autoridad de dependencias y la estabilidad dinámica de sus decisiones; la evidencia visual-geométrica de las piezas
    con cambio de mano; la conmutación de **todas** las vistas admisibles que el rack podría materializar después con
    Insertar —estén o no seleccionadas—, incluido el orden visual relativo de las piezas cuyas huellas se superponen de
    forma material; el BOM, con el multiconjunto de líneas de su builder y con la clave con que producción lo consolida;
    y la fidelidad del store. La enumeración de vistas es pura: no busca vistas hermanas en el dibujo, no las añade a la
    selección y no las crea. Es la garantía ejecutable del fail-closed: una regla estática que resulte falsa para un
    rack concreto, en cualquier vista que la copia pueda generar, no llega a mutar.

## Alternativas consideradas

- **Espejo de entidades o referencia con escala negativa** — descartada: el diseño embebido no cambia y todas las
  autoridades lo contradicen; los textos quedan al revés y la escala se propaga.
- **Espejo en sitio o prompt «¿Borrar objetos originales?»** — descartada en este corte: exige cubrir todas las
  vistas y referencias del `RackId` o produce hermanas divergentes, racks parciales y BOM ambiguo.
- **Inferir el eje por vista o preguntar Frentes/Fondos** — descartada: el diseño reflejado dependería de qué vistas
  se seleccionaron y un grupo frontal + lateral no tiene reflexión única.
- **Admitir las vistas que no exponen `μ_k` con su contenido sin voltear** (fidelidad física pero no gráfica) —
  descartada en el primer corte; queda como relajación futura con decisión del Owner.
- **Propiedad de vista «observada desde el lado opuesto» en el sobre** — descartada en I-52 por ser cambio de modelo;
  es la vía futura para reflejar laterales, la vista primaria del Dinámico y la cama.
- **Segunda reflexión por kind** (intercambio A↔B en la lateral de Push Back compuesto, R_Y en la lateral de
  Cantilever de estación sencilla) — descartada: exige un prompt de eje o reglas por vista; registrada como futuro.
- **Profundidad (DEPTH) como eje canónico del Selectivo** — descartada: invierte la convención «fondo 0 = frente» y
  concentra los casos no representables.
- **Reflector genérico sobre JSON, DTO nuevos para homogeneizar kinds, o `WithDesign` en el Selectivo** — descartadas:
  el primero no conoce la semántica, el segundo cambia el schema y el tercero materializa literales vinculados.
- **Congelar como valor explícito el remanente de un medio frente cuyo ancho depende de una variable** — descartada:
  deja de ser espejo al cambiar la variable. Un medio frente que absorba en su primer tramo sería cambio de modelo.
- **Conservar sin transformar la metadata desconocida del payload** — descartada: un miembro por poste o frente de un
  build más nuevo quedaría apuntando a posiciones equivocadas.
- **Verificar solo las vistas seleccionadas** — descartada: la copia puede generar después cualquier vista admisible.
- **Soportar definiciones con BASE movida o referencias dinámicas** — diferida: exige componer el origen y las acciones
  del bloque en la colocación y en los planes; el primer corte falla cerrado.
- **Metadato de simetría en los catálogos (`assets/`)** — diferida: cambio de catálogo compartido; el primer corte usa
  la evidencia visual-geométrica evaluada por estado.
- **Comparar solo la geometría de las piezas, sin capa, color, tipo de línea ni visibilidad** — descartada: una mitad en
  otra capa o con otro trazo haría que la copia no fuera el espejo visual del original, o que un estado de capa mostrara
  solo una mitad.
- **Comparar los valores visuales que resuelven hoy las propiedades, con un único token de herencia y la misma capa
  efectiva como regla de visibilidad** — descartada: dos mitades con fuentes distintas y el mismo aspecto actual dejan de
  verse iguales al cambiar una capa o la referencia, y una referencia anidada en otra capa puede ocultar una sola mitad.
- **Fusionar o normalizar el sentido de trazos sin continuidad demostrable** — diferida: con tipos de línea discontinuos
  cambia la fase del patrón; solo la caracterización puede habilitarlo con evidencia.
- **Evaluar los estados dinámicos directamente en la base de datos cacheada de la biblioteca** — descartada: contaminaría
  un estado compartido por todas las importaciones de la sesión.
- **Comparar las primitivas tal como están dibujadas, sin canonizarlas** — descartada: dos mitades simétricas dibujadas
  con segmentaciones distintas fallarían sin motivo.
- **Aceptar sombreados de degradado cuando sus parámetros parecen simétricos** — diferida: el primer corte los trata como
  clase no soportada.
- **Caracterizar la sonda solo en la consola sin interfaz de AutoCAD** — descartada como autoridad: el runtime objetivo es
  AutoCAD 2025; la consola solo sirve como comparación.
- **Registro estático de simetrías caracterizadas** (lista finita de estados o intervalo con un centro afín sobre un
  parámetro) — descartada: no cubre los estados continuos de los bloques dinámicos y derivar la relación exigiría
  interpretar las acciones del bloque.
- **Huella de la definición como ligadura normativa de la simetría** — descartada: una huella no demuestra simetría y el
  orden de iteración de una definición cambia con ediciones irrelevantes; queda solo como trazabilidad.
- **Probar la simetría con la caja envolvente** — descartada: una pieza asimétrica puede tener una caja simétrica.
- **Inferir la simetría de todos los estados de un bloque dinámico a partir de estados de muestra** — descartada: un
  número finito de estados no demuestra el continuo que un cambio de variable puede producir.
- **Limpiar automáticamente los miembros retirados de versiones anteriores** — descartada: el espejo no reescribe datos
  que no sabe interpretar; si el store los descarta, el usuario los limpia con `RACKEDITAR` → Actualizar.
- **Inferir el centro de simetría de la diferencia entre el plan reflejado y el original** — descartada: es circular; un
  centro «conveniente» oculta un desplazamiento.
- **Tratar como portador todo el `ExtensionData` del sobre y del envoltorio** — descartada: el sobre ya lleva semántica
  de vista y un build futuro puede añadir más.
- **Verificar la estabilidad solo con el estado del registro del momento** — descartada: un cambio de variable posterior
  puede desbordar filas o mover índices; tampoco se toma como cota un control de la interfaz.
- **Censo manual de los tipos anidados cubiertos** — descartada: puede quedar incompleto sin avisar.
- **Copiar solo la capa de la referencia fuente, como `RACKDUPLICAR`** — descartada: la apariencia de la referencia forma
  parte de lo que el usuario refleja; una presentación que no se puede conservar falla cerrado en lugar de copiarse a
  medias o de avisar.
- **Emparejar polilíneas con líneas o arcos sueltos por su geometría** — descartada: la clase de origen, el cierre y la
  generación del patrón cambian la representación.
- **Tratar las piezas de una vista como un multiconjunto sin orden** — descartada donde se superponen de forma material: el
  orden relativo cambia lo que se ve.
- **Leer la caché de la biblioteca por reflexión** — descartada: la API pública ya clona definiciones hacia una base
  auxiliar sin tocar la caché.
- **Clasificar el orden en el espacio modelo solo frente a objetos no seleccionados** — descartada: una copia puede caer
  sobre otra fuente seleccionada o sobre un objeto seleccionado e ignorado que iban después de su fuente, y el orden se
  invertiría sin detectarse.
- **Usar la extensión geométrica de los objetos como huella visual sin sus ocupaciones de modelo** — descartada: las
  máscaras de texto, los marcos y los anchos geométricos ocupan fuera de ella.
- **Copiar la presentación histórica de cada referencia interna del bloque fuente** — descartada: el espejo es fiel al
  plan regenerado, como Actualizar, y los drawers existentes no cambian.
- **Tratar como prueba el valor que devuelve el clonado de bloques** — descartada: devuelve cero también cuando no
  faltaba ningún bloque; la prueba es la presencia de todos los bloques requeridos.
- **Dilatar la huella visual por el grosor de línea de pantalla o de papel** — descartada en el primer corte: no es una
  distancia del modelo, depende de la escala de visualización o de trazado y de las tablas de estilos de trazado, y
  ninguna escala de referencia la haría conservadora; lo que eso deja sin garantía se declara en el alcance.
- **Evaluar las huellas visuales con el contexto de la biblioteca** — descartada: la importación conserva capas, bloques
  anidados y estilos homónimos del dibujo, así que la huella no describiría lo que se materializa.
- **Clonar primero de la biblioteca y después del dibujo en una base auxiliar compartida** — descartada: el primer
  clonado de cada nombre gana, y una definición del dibujo se evaluaría con dependencias de la biblioteca.
- **Decidir el orden en el espacio modelo solo antes de importar** — descartada como decisión final: la importación
  puede cambiar lo que se materializa; la evaluación previa queda como rechazo anticipado.
- **Invalidar la operación por cualquier cambio del contexto global de creación** — descartada: solo invalida lo que el
  productor o la huella consumen; lo no consumido no cambia el resultado.
- **Inventar un alias para `RACKMIRROR` para pasar las guardas de la ayuda** — descartada: el alias ausente es un estado
  válido y la guarda lo verifica solo cuando existe.
- **Tratar la igualdad de fuentes simbólicas como prueba de que la representación resuelta está soportada** —
  descartada: una capa, un tipo de línea o una herencia del dibujo destino pueden resolver en un tipo de línea complejo
  aunque las dos mitades compartan la misma fuente.
- **Usar la extensión geométrica de cualquier objeto del dibujo como huella por defecto** — descartada: sin una
  caracterización por clase no se sabe si acota todo lo que ocupa (patrones, símbolos de punto, representaciones
  anotativas, atributos).
- **Excluir de la huella de orden la geometría que hoy no se ve** — descartada: el orden de dibujo persiste y reaparece
  al encender o reutilizar la capa.
- **Re-verificar solo los objetos posteriores ya capturados** — descartada: un objeto añadido después quedaría debajo de
  la copia sin detectarse.
- **Una guarda de productores basada solo en hashes de archivos** — descartada: no distingue un cambio de lo que el
  productor asigna, usa o crea de un cambio irrelevante, y no dice qué hay que volver a caracterizar.
- **Congelar el contrato antes de reconciliar con I-55 las autoridades compartidas** — descartada: dos iniciativas
  podrían congelar autoridades duplicadas o incompatibles; la reconciliación va antes del freeze, sin esperar al
  consenso de I-55.

## Consecuencias

- Positivas: la copia reflejada es un rack RackCad normal que sobrevive a guardar, reabrir, `RACKEDITAR` →
  Actualizar, Insertar de cualquier vista admisible, BOM y cambios de variable; el comando no dispersa casos por sistema;
  la agrupación de I-51 se reutiliza; Application no depende de AutoCAD; el espejo no empeora la fidelidad de ningún
  store y no copia metadata que no sepa transformar.
- Negativas / costos aceptados:
  - en el primer corte no se reflejan laterales de Selectivo, Dinámico, Push Back ni Cantilever, ni la cama, ni diseños
    no representables (esquinas, brazos C/L sencillos, ciertas ausencias de Push Back); un Dinámico dibujado solo en
    lateral no puede reflejarse;
  - fallan cerrado hasta nueva decisión: los Selectivos con topes; los **Push Back con tope posterior activo**, que es
    su **estado por defecto**, de modo que la mayoría de los Push Back solo pasan con el tope en «Ninguno» o sin ninguna
    celda que lo dibuje en cada lado; los **medios frentes cuyo ancho depende de una propiedad vinculada** (y los
    desviadores que dependen de ellos); las **filas de parrillas o tarimas desbordadas** y las que un cambio de variable
    podría desbordar sin prueba para todo estado del registro; las **tarimas frontales de Push Back desbordadas**, en
    rack simple o compuesto; las cabeceras de plantilla con diagonales automáticas; las definiciones con BASE movida; las
    referencias dinámicas, anónimas o anotativas; los payloads con **metadata semántica desconocida** y los sobres o
    envoltorios con **claves desconocidas** fuera de la lista de portadores, con nombres que colisionan o con enteros
    de enum no definidos; los estados latentes del tope posterior con índices fuera del rango válido; las piezas con
    diferencia de mano sin evidencia geométrica aplicable; los **Selectivos cuyos largueros o postes cambian de estado
    con una variable** sin overrides que los fijen; y todo rack con **alguna vista admisible** que no conmute;
  - las vistas frontal y planta dependen de la evidencia visual-geométrica evaluada por estado, porque prácticamente
    todas sus piezas necesitan aceptar un cambio de mano; si la evaluación no es viable en AutoCAD 2025, no tiene
    paridad con el materializador, no tiene un punto de observación soportado o no puede reproducir el contexto del
    dibujo, o la biblioteca usa clases, variantes o apariencias fuera de la política (textos, atributos, sombreados con
    patrón o degradado, sólidos 3D, polilíneas con ancho, tipos de línea complejos —también cuando los aportan capas o
    herencias del dibujo destino—, inserciones múltiples o anotativos anidados, mitades con fuentes visuales, clases de
    origen o rutas de visibilidad distintas, piezas solapadas cuyo orden se invierte), esas piezas —y con ellas kinds
    enteros— fallan cerrado;
  - la **representabilidad depende de la biblioteca efectiva** y puede reducirse cuando una pieza no satisface el contrato
    visual-geométrico. La biblioteca no está versionada en el repositorio y su ruta la configura el usuario: otra
    biblioteca puede dar otro resultado, siempre con fallo cerrado. La evidencia de una biblioteca concreta es indicativa,
    se identifica por su SHA-256 y está en la Proposal (§1.4); la caracterización es la autoridad;
  - la canonización conservadora puede dejar fuera piezas visualmente simétricas cuyos trazos sin continuidad
    demostrable difieren en segmentación u orientación, hasta que la caracterización habilite más con evidencia;
  - la **selección es todo-o-nada**: un solo rack que no pase hace fallar el comando entero y no se refleja ninguno;
  - la copia conserva la presentación exterior de la referencia fuente; una fuente con un estado de presentación que no
    se puede transportar o que no está caracterizado falla cerrado, y también una copia cuyo orden de dibujo no puede
    conservarse frente a cualquier objeto posterior a su fuente que se superponga con ella, o cuya huella visual no
    puede garantizarse conservadora; `RACKDUPLICAR` no cambia;
  - el orden en el espacio modelo no se garantiza entre objetos que solo se tocan por el grosor de línea que se ve en
    pantalla o en papel; una línea de construcción infinita o un rayo posterior a una fuente, sin huella analítica
    caracterizada, hacen fallar la operación aunque estén lejos, y también dos fuentes con órdenes distintos en pantalla
    y en trazado cuyas copias se superponen; fallan igualmente las operaciones con objetos posteriores de clases sin
    huella soportada, con representaciones que dependen del entorno sin huella conservadora o con geometría hoy oculta
    que se superpone, y aquellas cuya cola de objetos posteriores cambia antes de mutar;
  - la copia regenera sus piezas internas, anotaciones y cotas como Actualizar: los drawers no asignan explícitamente
    determinadas propiedades, la caracterización establece qué mecanismo aplica AutoCAD y qué observaciones consume cada
    productor, con su cierre transitivo (estilos de texto, bloques de flecha, tipos de línea y fuentes de los estilos de
    cota y de texto), una guarda detiene el trabajo si un productor cambia lo que consume, y la copia puede verse
    distinta de una fuente dibujada con otro contexto;
  - `RACKMIRROR` añade un único comando, sin alias, al censo de comandos vigente al integrarse (no a un número fijo), y
    una entrada de ayuda sin alias, que la ventana de ayuda muestra sin distintivo de alias; comparte con las
    propiedades personalizadas, ya integradas, la referencia de ayuda y las guardas de censo;
  - el contrato no se congela sin reconciliar antes con I-55, e I-49 donde aplique, la propiedad de las autoridades
    compartidas (taxonomía de tipo de vista, códec de vista y sección, plan desde el sistema resuelto, comparador
    authored, primitivo de materialización y transformaciones de colocación y proyección); si cualquiera intenta
    congelar una autoridad incompatible, no hay freeze para ninguna hasta resolverlo, y la segunda iniciativa en
    integrar restablece la convergencia con Actualizar e Insertar;
  - `RACKMIRROR` no corrige racks espejados antes con el `MIRROR` nativo (escala negativa): esas fuentes fallan cerrado;
  - los racks legados con **miembros retirados** fallan cerrado: si el store actual los descarta, hay que abrirlos con
    `RACKEDITAR` y Actualizar antes de reflejarlos; si el store los conserva (el peralte retirado del larguero alto de
    Push Back), no hay remedio en I-52;
  - la evaluación visual-geométrica (variantes, fuentes simbólicas tipadas, rutas de visibilidad, orden visual dentro de
    la pieza y entre piezas, huellas visuales y su recálculo tras importar, contexto de la base auxiliar y del dibujo
    destino, canonización y clonado aislado desde la caché incluidos) tiene un coste por pieza y estado que se mide en
    AutoCAD 2025 antes de exponer el comando;
  - lo importado de la biblioteca puede sobrevivir a un fallo;
  - todo tipo, miembro o valor de enum nuevo alcanzado desde los tipos raíz exige clasificar su regla de espejo antes de
    integrarse;
  - la aceptación de este ADR espera a la caracterización (G3), lo que añade una ronda del Owner antes de implementar;
    si G3 contradice materialmente el contrato o reduce materialmente el alcance que el Owner aceptó, se abre una
    Proposal V12 antes de pedirla.
- Vigilar: cada kind o vista nueva debe declarar su reflector, su decodificación de sección, su conjunto de vistas
  admisibles y su exposición; todo miembro nuevo y toda propiedad vinculable nueva necesitan clasificación antes de
  integrarse; todo offset gráfico nuevo de un builder de vista admitida debe caracterizarse; toda variante de clase o
  propiedad de presentación nueva debe clasificarse antes de admitirse; todo comando nuevo del plugin se suma al censo
  vigente al integrarse; si ADR-0042 (I-55) se acepta antes del freeze, la autoridad de Actualizar e Insertar en la que
  se apoya la convergencia se relee y se reconcilia.

## Referencias

- ADR-0009 (identidad GUID), ADR-0010 (Actualizar/Insertar), ADR-0031 §8 (reflexión rígida del lado B; texto y cota
  solo se trasladan), ADR-0034 §9, §12 y §14, ADR-0035 (cotas por tipo de vista).
- I-51: `RackDuplicationPlan`, `RackEnvelopeRestamp.RestampEnvelope(payload, name, Guid)` (Plugin).
- I-47 G9.1: `ViewBlockDraw.PrepareRedraw`, `SystemBlockWriter.RedefineInTransaction`, `PreparedViewRedraw`,
  `ProjectVariableMutationExecutor`.
- I-50: `DimensionViewVisibility`, `DimensionViewPolicy`.
- I-54 (integrada en `main`): Proposal V5 D-21 (invariante de preservación del sobre, que el espejo cumple) y sus
  residuales F-14a y F-14b; ADR-0039 (aceptado en `main`; alcances Proyecto y Rack independientes: identidad local al
  alcance, §1, y sin herencia entre ellos, §12); colección de propiedades de alcance Proyecto y ejecutores físicos,
  fuera del alcance del espejo; comando `RACKPROPIEDADES` con alias `RPR`, ayuda y censos por nombre de comandos, de
  ayuda y de ventanas (T-GRD-08) y censo de llamadas al compositor del sobre (T-GRD-02).
- I-49 ADR-0041 (aceptado en su rama; reemplaza a ADR-0040, que reemplazó a ADR-0038; reproduce sin cambios sus demás
  decisiones, incluidos el `PlanReadSet` (D19) y la identidad textual de `VariableId`, y solo cambia la validez de la
  clave, la sintaxis del cualificador y las formas de referencia que lo emiten; G5 sintáctico y G6 con enlace y
  evaluación, sin `PlanReadSet` ni dominio integrados): `PlanReadSet` y dominio del consumidor declarado por el
  descriptor; guardas de texto de `ProjectVariablesConformanceTests` y autoridad de unidades de longitud
  (`LengthUnitsAuthorityGuardTests`).
- I-53 E1, I-53S E2 e I-53D E3 (integradas en `main`): guardas C-08 y C-10 sobre `RackCad.Application.Systems.Shared`;
  ADR-0037 (aceptado); la ventana del Selectivo distribuye cabeceras con `ApplyHeaderBatch` y la del Dinámico conecta la
  misma fundación (`DynamicHeaderBatch`, `DynamicRackRebuild`).
- I-55 (Proposal V1, sin consenso): ADR-0042 (propuesto; se propone como sucesor de ADR-0010, conserva su semántica de
  Actualizar e Insertar y añade que un redibujo fallido impide insertar) y la reconciliación de las autoridades
  compartidas con este ADR antes del freeze.
- I-19: `CatalogBlockParameters`, `CatalogBlockManifest`.
- `src/RackCad.Application/Geometry/Transform2D.cs`; `src/RackCad.Application/Geometry/Vector2D.cs`
  (`GeometryTolerance`); `src/RackCad.Application/Persistence/RackProjectStore.cs`;
  `src/RackCad.Application/Persistence/RackProject.cs`;
  `src/RackCad.Application/Systems/Selective/SelectiveEffectiveDesignResolver.cs`;
  `src/RackCad.Application/Systems/Selective/SelectiveGeometryResolver.cs`;
  `src/RackCad.Application/Systems/Selective/SelectivePostGeometry.cs`;
  `src/RackCad.Application/Systems/Selective/SelectiveFrontalBuilder.cs`;
  `src/RackCad.Application/Systems/PushBack/PushBackRearTopeBuilder.cs`;
  `src/RackCad.Application/Persistence/RackEmbedDocument.cs`; `src/RackCad.Application/Catalogs/BlockLibrary.cs` (ruta
  configurable de la biblioteca); `src/RackCad.Plugin/Drawing/BlockLibraryImporter.cs` (caché privada de sesión de la
  biblioteca; acceso por `EnsureBlocks`); `src/RackCad.Plugin/Drawing/LateralHeaderDrawer.cs` (`ApplyDynamicParameters`;
  orden de materialización; referencias internas sin presentación asignada; estilo de cota actual cuando el plan no trae
  estilo con nombre, búsqueda del estilo con nombre y reconstrucción de la cota); `src/RackCad.Plugin/LayerHelper.cs`
  (una capa existente se usa tal cual); `src/RackCad.Application/Systems/Selective/SelectiveAnnotations.cs` (altura de
  texto por la escala de anotación del diseño); `src/RackCad.UI/RackCommandHelpWindow.cs` (distintivo de alias de la
  ayuda).

## Historial del borrador

Mientras este ADR sea `propuesto` puede editarse (adr/README.md). Para no perder la historia:

- **Borrador V1** — publicado con Proposal V1 en `0fc7032` (texto íntegro recuperable con
  `git show 0fc7032:docs/adr/0036-rackmirror-espejo-semantico-por-copia.md`).
- **Borrador V2** — publicado con Proposal V2 en `0445718` (recuperable con
  `git show 0445718:docs/adr/0036-rackmirror-espejo-semantico-por-copia.md`). Cambios respecto del V1, por la revisión de
  Arquitecto de Proposal V1 (`Architect: CHANGES REQUIRED — PROPOSAL V2`, hallazgo AR-13): sustrato real por kind y
  regla de fidelidad; sección canónica; `Origin` en la colocación y `Origin = 0` en la fuente; importación de mejor
  esfuerzo con dependencias y UNDO desconocido; nombre lógico, pieza faltante como fallo duro y criterio del registro por
  kind; decisiones 13 y 14 nuevas; precedencia de numeración por primera publicación observable.
- **Borrador V3** — publicado con Proposal V3 en `545c222` (recuperable con
  `git show 545c222:docs/adr/0036-rackmirror-espejo-semantico-por-copia.md`). Cambios respecto del V2, por la revisión
  de Arquitecto de Proposal V2 (`Architect: CHANGES REQUIRED — PROPOSAL V3`) y la orden del Coordinador:
  - decisión 6: fidelidad contra el store y no contra `RACKEDITAR`; prohibición de normalizaciones que congelen valores
    dependientes de propiedades vinculadas; política de metadata semántica desconocida frente a portadores; cobertura
    de miembros;
  - decisión 5: tres dimensiones separadas y holguras ancladas a un lado ya demostradas (tope Selectivo y tope
    posterior de Push Back);
  - decisión 13: centro en el marco local de inserción tras `T(−Origin)`, identidad de biblioteca y manifiesto, forma
    afín solo con caracterización, sin declaraciones para familias con holgura `UNKNOWN`;
  - decisión 14: verificación de **todas** las vistas admisibles, no solo de las seleccionadas;
  - decisiones 4, 7, 8, 10, 11 y 12: `View` vacía del Selectivo solo con `−1`; convergencia demostrada antes de que el
    comando consuma la autoridad; equivalencia para todo estado del registro; `DimensionViews` obligatorio con I-50
    integrada; referencias dinámicas, anónimas o anotativas; conjunto de entradas del registro leídas en la huella;
  - consecuencias ampliadas y aceptación del Owner después de G3, como precondición de G4.
- **Borrador V4** — publicado con Proposal V4 en `8e2ae4f` (recuperable con
  `git show 8e2ae4f:docs/adr/0036-rackmirror-espejo-semantico-por-copia.md`). Cambios respecto del V3, por la revisión
  de Arquitecto de Proposal V3 (`Architect: CHANGES REQUIRED — PROPOSAL V4`) y la orden del Coordinador:
  - decisión 13: evidencia geométrica independiente de la definición real antes de la evidencia de colocación; nunca un
    centro inferido de planes; forma afín solo con relación derivada de la parametrización; dominio caracterizado y
    ligadura a la definición que se dibuja; el rechazo de los topes pasa a ser defensa adicional;
  - decisión 6: estabilidad dinámica para toda decisión de continuar o canonizar y autoridad explícita de dependencias;
    lista cerrada de portadores y metadata exterior desconocida con fallo cerrado; cobertura por cierre transitivo con
    enums;
  - decisiones 8 y 10: la equivalencia para todo estado del registro exige predicados estables; un cambio de variable no
    invalida ninguna decisión; cota inferior solo de una autoridad real;
  - decisión 7: la convergencia incluye `RACKEDITAR` → Insertar;
  - decisión 5: ninguna propiedad pendiente al cerrar la caracterización y Proposal V5 ante una reclasificación material;
  - decisiones 11 y 12: precedencia de fuente (primero el candidato RackCad) y observaciones reales del registro;
  - consecuencias: tope posterior de Push Back activo por defecto, tarimas frontales de Push Back desbordadas, filas que
    un cambio de variable podría desbordar y claves exteriores desconocidas.
- **Borrador V5** — publicado con Proposal V5 en `e998a2b` (recuperable con
  `git show e998a2b:docs/adr/0036-rackmirror-espejo-semantico-por-copia.md`). Cambios respecto del V4, por la revisión
  de Arquitecto de Proposal V4 (`Architect: CHANGES REQUIRED — PROPOSAL V5`) y la orden del Coordinador:
  - decisión 13: evidencia geométrica **evaluada por estado** sobre la definición real (la del dibujo si existe;
    re-verificada en PREPARE tras importar), aplanada recursivamente y sin mutar el dibujo; política explícita de clases
    de entidad; verificación por multiconjunto y nunca por caja envolvente; eje derivado de `F` y de la rotación; huella
    solo de trazabilidad; sin forma afín ni interpretación de acciones de bloques dinámicos; regla para parámetros
    dinámicos que dependen de vínculos; viabilidad demostrada en la caracterización fuera del código de producción;
  - decisión 6: contrato de serialización real para la metadata y la cobertura; colisiones de nombres, enteros de enum
    no definidos y miembros retirados fallan cerrado, con mensaje y sin limpieza automática; descriptores de las
    propiedades vinculables en la cobertura; estado dinámico en la estabilidad;
  - decisiones 8, 10, 12 y 14: estado dinámico de las piezas en la igualdad para todo estado del registro; re-verificación
    de la definición real en PREPARE; evidencia geométrica en la verificación por rack;
  - decisión 11: Model Space como decisión deliberada (candidato fuera de Model Space ignorado con aviso);
  - decisión 5 y consecuencias: una contradicción material de G3 abre una Proposal V6; frontal y planta dependen de la
    evidencia geométrica; fuentes espejadas con `MIRROR` nativo fuera; miembros retirados; clases de entidad no
    soportadas; coste de la evaluación;
  - alternativas: registro estático de simetrías, huella como ligadura, caja envolvente, muestras de estados y limpieza
    automática de miembros retirados, descartadas.
- **Borrador V6** — publicado con Proposal V6 en `91bdd38` (recuperable con
  `git show 91bdd38:docs/adr/0036-rackmirror-espejo-semantico-por-copia.md`). Cambios respecto del V5, por la revisión
  de Arquitecto de Proposal V5
  (`Architect: CHANGES REQUIRED — PROPOSAL V6`) y la orden del Coordinador:
  - decisión 13: **equivalencia visual-geométrica** (firma visual efectiva con capa `0`, `ByLayer` y `ByBlock`, estado de
    capa, visibilidad y orden de rellenos solapados); **primitivas canónicas** dentro de la misma firma; parámetros
    aplicados con la semántica del materializador y **valores efectivos**; `Origin` del **bloque evaluado**; política de
    clases **cerrada** con el degradado como no soportado; huella con firma visual; caracterización en AutoCAD 2025 con la
    postcondición del dibujo;
  - decisiones 5, 12 y 14: una contradicción material abre una Proposal V7; re-verificación y verificación por rack
    visual-geométricas;
  - contexto y consecuencias: apariencia por entidad, alcance dependiente de la biblioteca efectiva, evidencia indicativa
    de la biblioteca inspeccionada, selección todo-o-nada y coste de la evaluación;
  - alternativas: comparación solo geométrica, primitivas sin canonizar, degradados y caracterización solo en la consola,
    descartadas o diferidas;
  - numeración: ADR-0037 ya está en `main`.
- **Borrador V7** — publicado con Proposal V7 en `b70b5bf` (recuperable con
  `git show b70b5bf:docs/adr/0036-rackmirror-espejo-semantico-por-copia.md`). Cambios respecto del V6, por la revisión de
  Arquitecto de Proposal V6
  (`Architect: CHANGES REQUIRED — PROPOSAL V7`) y la orden del Coordinador:
  - decisión 13: **fuentes visuales simbólicas** (`Explicit`, `ByLayer`, `InheritLayer`, `InheritBlock`) en lugar de
    valores resueltos; **ruta de visibilidad** de ancestros y primitiva; escala de tipo de línea siempre en la firma;
    **orden visual** de superposiciones materiales; canonización **conservadora** (sin continuidad demostrable no se
    fusiona ni se normaliza el sentido); polígono real de sólidos y trazos y sombreados por bucles y estilo de islas;
    polilíneas con ancho, grosor o normal distinta y sombreados de patrón fallan cerrado; recomputación gráfica solo si se
    aplicó alguna propiedad; **punto de observación**; **transformación efectiva**; **caché de biblioteca aislada**;
    identidad **SHA-256** de la evidencia;
  - decisión 5 y consecuencias: una contradicción material abre una Proposal V8; las consecuencias ya no enumeran piezas
    de una biblioteca concreta y remiten a la Proposal (§1.4);
  - contexto, alternativas y referencias: apariencia heredada por capa o por bloque; comparación de valores resueltos,
    fusión sin continuidad demostrable y evaluación en la caché, descartadas o diferidas.
- **Borrador V8** — publicado con Proposal V8 en `e0a779f` (recuperable con
  `git show e0a779f:docs/adr/0036-rackmirror-espejo-semantico-por-copia.md`). Cambios respecto del V7, por la revisión
  de Arquitecto de Proposal V7 (`Architect: CHANGES REQUIRED — PROPOSAL V8`) y la orden del Coordinador:
  - decisión 13: política **cerrada de clases y variantes** por tipo exacto; clase de origen, cierre y generación del
    patrón en la firma; fuentes **tipadas** (método y valor); escalas de ancestros en los trazos sin continuidad
    demostrable; rutas anidadas **estables**, sin nombres anónimos; base auxiliar con el **contexto de la autoridad
    origen**; caché accedida solo por la API pública que clona definiciones; **orden visual entre piezas**; «la
    caracterización **debe** demostrar»;
  - decisiones 7, 10, 11, 12 y 14: «debe demostrarse»; la colección de nivel dibujo de las propiedades personalizadas no
    es portador y el espejo no la toca; **presentación de la referencia fuente** conservada, con fallo cerrado ante un
    estado no transportable o un orden en el espacio modelo que no se puede conservar; huella con presentación y orden;
    orden entre piezas en la verificación por rack;
  - decisión 5 y consecuencias: una contradicción material abre una Proposal V9; consecuencias genéricas de la
    presentación, las variantes, el contexto y el orden;
  - contexto, alternativas y referencias: presentación de la referencia y contexto del dibujo; copiar solo la capa,
    emparejar por geometría entre clases, orden como multiconjunto y lectura de la caché por reflexión, descartadas;
    ADR-0040 aceptado en I-49 en lugar de ADR-0038, I-53S integrada en `main` y G6 y G7A de I-54.
- **Borrador V9** — publicado con Proposal V9 en `deb08cd` (recuperable con
  `git show deb08cd:docs/adr/0036-rackmirror-espejo-semantico-por-copia.md`). Cambios respecto del V8, por la revisión
  de Arquitecto de Proposal V8 (`Architect: CHANGES REQUIRED — PROPOSAL V9`) y la orden del Coordinador:
  - decisión 11: orden en el espacio modelo frente a **todo** objeto posterior a la fuente, con huellas visuales
    conservadoras; presentación exterior con el estilo de trazado según el modo del dibujo; inventario cerrado;
  - decisión 13: huellas visuales separadas de la evidencia de simetría; orden entre piezas con o sin cambio de mano;
    colores de libro con su valor almacenado; referencias anidadas sin atributos ni referencias externas; inventario
    cerrado; clonado desde el dibujo o por la API pública con verificación de presencia; «la caracterización **debe**
    demostrar» también las huellas y el contexto de materialización;
  - decisiones 7, 10, 12 y 14: contexto de materialización común a las dos generaciones; propiedades personalizadas de
    alcance Proyecto; huella con el contexto de materialización y los objetos posteriores; orden por huellas;
  - decisión 5 y consecuencias: una contradicción material abre una Proposal V10; regeneración interna bajo el contexto
    vigente; censo de comandos relativo al vigente al integrar;
  - contexto, alternativas y referencias: valores por defecto de creación y alcance visual fuera de la extensión;
    clasificar solo objetos no seleccionados, usar la extensión sin dilatar, copiar la presentación interna histórica y
    tratar como prueba el retorno del clonado, descartadas; G6 de I-49, G7 de I-54 y G7 de I-53D.
- **Borrador V10** — publicado con Proposal V10 en `636f7fd` (recuperable con
  `git show 636f7fd:docs/adr/0036-rackmirror-espejo-semantico-por-copia.md`). Cambios respecto del V9, por la revisión
  de Arquitecto de Proposal V9 (`Architect: CHANGES REQUIRED — PROPOSAL V10`) y la orden del Coordinador:
  - decisión 11: la huella visual es ocupación de modelo, construida después de transformar y sin grosores de pantalla o
    de papel; órdenes de pantalla y de trazado contradictorios; líneas de construcción infinitas y rayos;
  - decisión 12: observaciones consumidas del contexto de materialización; relectura del dibujo real y recálculo de
    huellas tras importar, con la evaluación previa como rechazo anticipado;
  - decisión 13: huellas en el dibujo destino, simulando la importación con el dibujo primero; entradas registradas de
    cada huella; precedencia del dibujo en una base auxiliar compartida; «la caracterización **debe** demostrar» también
    el recálculo tras importar;
  - decisión 7 y contexto: lo que los drawers no asignan y lo que la caracterización debe establecer; estilo de cota,
    estilo de texto y capas de anotación;
  - decisión 5 y consecuencias: una contradicción material abre una Proposal V11; orden no garantizado por el grosor de
    línea visible; entrada de ayuda sin alias;
  - alternativas y referencias: dilatar por grosores de presentación, evaluar con el contexto de la biblioteca, clonar
    primero de la biblioteca, decidir el orden solo antes de importar, invalidar por todo el contexto global e inventar
    un alias, descartadas; I-54 e I-53D integradas en `main`.
- **Borrador V11** — este texto, con Proposal V11. Cambios respecto del V10, por la revisión de Arquitecto de
  Proposal V10 (`Architect: CHANGES REQUIRED — PROPOSAL V11`) y la orden del Coordinador:
  - decisión 11: huella de orden de los objetos del dibujo por una política cerrada por tipo exacto, latente e
    independiente de la visibilidad, con la regla del entorno y la cola completa de objetos posteriores; anchos de
    modelo transformados con la transformación compuesta; autoridades que también necesita I-55, reconciliadas antes del
    freeze;
  - decisión 12: cola de objetos posteriores enumerada de nuevo antes de la decisión definitiva y al empezar a mutar;
    contexto consumido con su cierre transitivo;
  - decisión 13: igualdad de fuentes simbólicas separada de la elegibilidad de la representación resuelta en el dibujo
    destino, con tipos de línea complejos por capa o por herencia; base auxiliar por pieza solo si reproduce la
    resolución global; «la caracterización **debe demostrar**» también esa elegibilidad, la política por tipo exacto y
    la cola;
  - decisión 7 y contexto: cierre transitivo de los estilos de cota y de texto; escala de anotación del diseño frente a
    la del dibujo; guarda estructural de los productores;
  - decisión 5 y consecuencias: una contradicción material abre una Proposal V12; objetos posteriores no soportados,
    ocultos o dependientes del entorno; ayuda sin distintivo de alias; reconciliación con I-55 antes del freeze;
  - alternativas y referencias: igualdad simbólica como elegibilidad, extensión geométrica por defecto, excluir lo
    oculto, re-verificar solo lo capturado, guarda por hashes y congelar antes de reconciliar, descartadas; ADR-0041
    aceptado en I-49 y ADR-0042 propuesto en I-55.

  Sigue **propuesto**: su aceptación se pide después de G3 y antes de G4.
