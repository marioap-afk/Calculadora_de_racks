# ADR-0036: RACKMIRROR es un espejo semántico por copia: reflexión canónica por kind, vistas admisibles y colocación sin escala negativa

- **Estado:** **propuesto**
- **Fecha:** 2026-09-12 (propuesto; borrador corregido con Proposal V2 y con Proposal V3 el mismo día, y con Proposal V4,
  Proposal V5, Proposal V6 y Proposal V7 el 2026-09-13)
- **Decisores:** Mario Pérez, Owner del repositorio (**acepta o rechaza**; pendiente). La aceptación **no** es
  precondición de la caracterización (G3): se pide **después de G3**, si G3 no contradice materialmente el contrato (si
  lo contradice, se abre una Proposal V8), y **es precondición de G4**. Coordinador de I-52 y Arquitecto de I-52
  (consenso técnico **pendiente** sobre la Proposal); Claude (redacción)
- **Iniciativa relacionada:** I-52 — `feature/rackmirror-espejo-semantico`
  ([contrato](../initiatives/I-52-rackmirror-espejo-semantico.md),
  [Discovery](../initiatives/I-52-discovery.md), [Proposal V1](../initiatives/I-52-proposal-v1.md),
  [Proposal V2](../initiatives/I-52-proposal-v2.md), [Proposal V3](../initiatives/I-52-proposal-v3.md),
  [Proposal V4](../initiatives/I-52-proposal-v4.md), [Proposal V5](../initiatives/I-52-proposal-v5.md) y
  [Proposal V6](../initiatives/I-52-proposal-v6.md) (historial), [Proposal V7](../initiatives/I-52-proposal-v7.md),
  [decisiones](../automation/decisions/I-52.md))

> **Numeración.** Un número de ADR queda reclamado por su primera publicación observable en un ref remoto. Este ADR se
> publicó por primera vez con el número 0036 en `origin/feature/rackmirror-espejo-semantico`, commit
> `0fc7032bf15d03e7d478bbd9350f156708621c9d` (corrida de CI del push `34731908035`, creada el `2026-09-13T01:59:44Z`),
> sin publicación anterior de otro 0036 en ningún ref. ADR-0035 (I-50) y ADR-0037 (I-53, ya integrada) están aceptados
> en `main`; ADR-0038 (I-49) y ADR-0039 (I-54) están aceptados en sus ramas. Los tres posteriores a 0036 se publicaron
> después y con otro número: no hay colisión. Antes de pedir la aceptación del Owner se vuelve a buscar 0036 en todos
> los refs; si apareciera una publicación anterior, este ADR se renumera antes de la aceptación. Una vez `aceptado` no se
> renumera, y dos ADR aceptados con el mismo número detienen el trabajo hasta que decida el Owner (Proposal V7 §16).

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
transacción del dibujo; algunos valores geométricos (el ancho de un claro) dependen de propiedades vinculadas a variables
de proyecto; varios builders anclan holguras gráficas a un solo lado (el tope del Selectivo y el tope posterior de
Push Back); y casi todas las piezas de frontal y planta son bloques dinámicos de una biblioteca sin versionar ni metadato
de simetría, cuyo estado (longitud, altura) puede cambiar de forma continua, a veces con una variable de proyecto, y
cuya apariencia depende de capas, colores, tipos de línea, grosores y escalas por entidad, a menudo heredados por capa o
por bloque. La ruta de esa biblioteca la configura el usuario, así que otra estación puede usar otra biblioteca.

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
   regla de reflexión o la arquitectura abre una Proposal V8. Una holgura gráfica anclada a un lado ya demostrada en
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
   definición dentro de la transacción del llamador, escribe el payload preparado y crea la referencia con la
   colocación ya calculada. La autoridad recibe el nombre lógico de la copia, el estado efectivo y la vista y sección
   canónicas. Su convergencia con los productores de planes existentes (`RACKEDITAR` → Actualizar, `RACKEDITAR` →
   **Insertar**, el ejecutor de variables y el paso del estado del editor al sistema) se demuestra **antes** de que el
   comando la consuma; si Actualizar e Insertar comparten costura, una guarda demuestra que ambos la siguen usando. El comando
   no conoce Left/Right, A/B, estaciones ni reglas de cabecera. Una pieza sin bloque en el dibujo es **fallo duro** para
   el espejo: nunca se crea una copia incompleta, y los drawers existentes no cambian.
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
    cualquier otra clave desconocida no vacía del sobre o del envoltorio falla cerrado. El espejo no crea, modifica,
    desvincula ni materializa variables y no escribe el registro. Para dibujar consume en **solo lectura** la autoridad
    efectiva existente, con su mismo criterio de acreditación del registro: un registro no acreditable o un vínculo roto
    fallan cerrado solo para los kinds cuya autoridad consume el registro. Un cambio de valor de una variable no puede
    deshacer la equivalencia reflejada **ni invalidar ninguna decisión de continuar o canonizar** tomada antes de mutar,
    incluida la aceptación de un cambio de mano cuya evidencia dependa de un parámetro dinámico vinculado.
    Una cota inferior del valor de una propiedad vinculada solo se usa para demostrarlo si la define una autoridad real
    de resolución o de dominio, nunca un control de la interfaz.
11. **Fuente canónica.** Solo `BlockReference` de Model Space, no MINSERT, con `Normal = +Z`, rotación finita y escala
    uniforme positiva (`(−s,−s)` se canoniza a `(s,s)` + π), definición con **`Origin = 0`** y `View`/`Section`
    decodificables. Escala no uniforme, una sola componente negativa, `sz < 0`, una definición con BASE movida o una
    referencia **dinámica, anónima o anotativa** que lleve datos de RackCad fallan cerrado con mensaje propio; nunca se
    ignoran como si no fueran racks. Primero se reconoce si un objeto lleva datos de RackCad: los objetos que no los
    llevan —aunque tengan escala, rotación, origen o tipo no canónicos, o contengan racks anidados— se ignoran con aviso
    y nunca abortan la operación; las restricciones estrictas solo se aplican a los candidatos RackCad. Un candidato
    RackCad fuera de Model Space no entra como fuente: se ignora con aviso y no aborta (decisión deliberada, en
    continuidad con `RACKDUPLICAR`). La línea se captura en el UCS con Z paralela a la de WCS y se convierte a WCS. Las
    tolerancias son las absolutas del repositorio; no se introduce tolerancia relativa.
12. **Atomicidad semántica, no de infraestructura.**
    `ACQUIRE → SNAPSHOT → PREFLIGHT (un solo orden normativo) → LINE → PREPARE → MUTATE → COMMIT`, con una sola
    transacción de escritura para todas las definiciones, payloads y referencias: cualquier fallo revierte todas las
    copias. Una huella de cada referencia fuente (identidad, transformación, capa, `Origin`, banderas de bloque y payload)
    y las observaciones del registro que la resolución efectiva realmente usó (acreditación, variables leídas y, si la
    autoridad de expresiones está integrada, sus dependencias transitivas; nunca una «versión» o una huella del registro
    completo) se re-verifican antes de mutar. La importación de la
    biblioteca de bloques en PREPARE es **de mejor esfuerzo**: ocurre fuera de esa transacción, puede arrastrar
    dependencias (bloques anidados, capas, estilos), puede quedar parcial y puede **permanecer** aunque la operación
    falle. PREPARE re-verifica los bloques requeridos y la definición real que quedó en el dibujo para las piezas cuya
    equivalencia visual-geométrica se aceptó (decisión 13). El comportamiento del UNDO sobre lo importado es desconocido
    hasta la validación del Owner y no se promete.
13. **Mano y simetría de bloques, con equivalencia visual-geométrica evaluada por estado.** Ningún bloque DWG se asume
    simétrico. Una diferencia de mano entre el plan reflejado y el plan original transformado solo es equivalente si
    pasan, en este orden, dos evidencias:
    - **Evidencia visual-geométrica evaluada por estado (`GeometricEvidence`).** Para cada pieza de cualquier vista
      admisible que necesite aceptar un cambio de mano se evalúa la pieza real en el **estado concreto** que usa: la
      definición real del bloque, clonada en una base de datos auxiliar privada y descartable; su vector de parámetros
      dinámicos, aplicado con **exactamente la misma semántica que el materializador** (nombres sin distinguir
      mayúsculas y la última clave duplicada ganando, propiedades de solo lectura sin escribir, nombres ausentes
      ignorados y recomputación gráfica solo si se aplicó alguna propiedad); y, leídos después del **punto de
      observación** que fije la caracterización, los valores efectivos, la definición evaluada, su `Origin` y la
      **transformación efectiva** de la referencia (posición, rotación, escalas, normal y transformación de bloque). Si
      una acción dinámica altera la referencia de un modo no representado, la pieza falla cerrado. La pieza se aplana
      recursivamente —bloques anidados con su transformación completa, su propio estado y su bloque evaluado; un ciclo
      falla cerrado.
    - **Apariencia por fuentes simbólicas.** Cada propiedad visual (color, tipo de línea, escala de tipo de línea, grosor,
      transparencia y estilo de trazado) se compara por su **fuente simbólica tipada** según las reglas de AutoCAD —valor
      explícito, por capa con nombre, heredada por la capa `0` (`InheritLayer`) o heredada por bloque (`InheritBlock`),
      con rutas canónicas relativas a la raíz de la pieza—, **nunca** por el valor que resuelve hoy: una mitad `ByLayer` y
      otra con el mismo color explícito no son equivalentes, ni lo son una herencia por capa y una por bloque. La escala
      de tipo de línea forma siempre parte de la firma. La **ruta de visibilidad** de cada primitiva (visibilidad y fuente
      de capa de cada referencia ancestro y de la propia primitiva) debe coincidir en las homólogas, de modo que ningún
      estado de capas o de visibilidad pueda mostrar una mitad y ocultar la otra; un mecanismo de visibilidad no modelado
      falla cerrado. Donde dos primitivas se superponen de forma visualmente material (rellenos, trazos coincidentes con
      firmas distintas, entidades transparentes), el orden visual relativo de las homólogas se conserva o la pieza falla
      cerrado.
    - **Correspondencia conservadora.** La simetría se prueba respecto del eje que derivan `F` y la rotación de la
      instancia (una rotación oblicua falla cerrado) y de un centro candidato, con una correspondencia uno a uno entre
      **primitivas canónicas** de la misma firma. La canonización es conservadora: solo se fusionan trazos con continuidad
      demostrable por su fuente; sin ella se conservan segmentación, orientación, escala y multiplicidad; los sólidos y
      trazos se comparan por el polígono que AutoCAD representa y los sombreados sólidos por sus bucles y su estilo de
      islas; nunca se fusionan familias distintas. La caja envolvente nunca es prueba: una pieza asimétrica con caja
      simétrica falla, y una pieza geométricamente simétrica con fuentes visuales o rutas de visibilidad distintas también.
    - **Política de clases cerrada.** Se evalúan líneas, arcos, círculos, elipses, splines, polilíneas con bulges sin
      ancho, sin grosor y con normal +Z, sólidos 2D, trazos, sombreados sólidos uniformes canonizables y referencias
      anidadas. **Toda otra clase o variante falla cerrado**: entre ellas polilíneas con ancho, grosor o normal distinta,
      regiones, sólidos 3D, entidades proxy o personalizadas, imágenes, OLE, puntos, polilíneas 2D y 3D de estilo antiguo,
      multilíneas, caras, wipeouts, directrices, tablas, formas, textos, atributos, cotas y sombreados de **patrón** o de
      **degradado**; igual falla una correspondencia ambigua.
    - **Evidencia de colocación (`PlanPlacementEvidence`).** Después, la conmutación de la vista demuestra que los
      builders colocan la pieza de forma coherente con el centro verificado; nunca ajusta, infiere ni corrige el centro.

    La definición evaluada es la del dibujo si el bloque ya existe y, si no, la de la biblioteca candidata; tras importar,
    PREPARE verifica de nuevo la definición real que quedó en el dibujo, porque la importación conserva una definición
    local con el mismo nombre. La evaluación no muta el dibujo del usuario (el dibujo queda intacto y la base de datos de
    trabajo restaurada, también ante excepción) ni la base de datos cacheada de la biblioteca, que solo se lee y se clona;
    entrega datos planos a Application, que no depende de AutoCAD, y su resultado no se persiste.

    Una huella de la pieza evaluada —valores efectivos, clases, geometría aplanada, fuentes simbólicas, rutas, escala,
    visibilidad, orden visual material y transformación efectiva, en serialización canónica y nunca en el orden de
    iteración de la definición— sirve solo para trazabilidad, detección de obsolescencia y enlace entre la verificación
    previa y PREPARE; **no** es evidencia de simetría. La evidencia registrada de una biblioteca concreta se identifica por
    el **SHA-256** de su contenido y no se reutiliza si cambia; para una definición ya presente en el dibujo manda su
    huella efectiva. No se usa una forma afín del centro sobre un parámetro dinámico ni se interpretan los grafos de
    acciones de los bloques dinámicos. Si un parámetro dinámico que la evidencia necesita depende directa o
    transitivamente de una propiedad vinculable, la evidencia de un solo estado **no** basta: se exige una prueba sobre
    todo el dominio autoritativo del parámetro, una autoridad integrada que garantice la equivalencia para todos sus
    estados o la demostración de que el parámetro no depende del vínculo; si no, falla cerrado, y nunca se infiere
    universalidad a partir de estados de muestra. Nunca se infiere un centro de la diferencia entre planes, de la caja
    envolvente, de la diferencia de inserciones ni de un mínimo de error; por eso ninguna evidencia puede hacer
    equivalente una pieza con holgura u offset anclado a un lado, cualquiera sea su rol, y el rechazo de las familias con
    holgura `UNKNOWN` (hoy los topes) es una defensa adicional. Ninguna holgura, offset gráfico o parámetro con semántica
    de lado se asume simétrico: sin regla, falla cerrado. La caracterización demuestra, fuera del código de producción y en
    AutoCAD 2025 (`acad.exe`), la viabilidad de la evaluación, su paridad con el materializador, el punto de observación,
    las reglas de fuentes, visibilidad y orden visual, y la postcondición del dibujo y de la caché; la evaluación
    productiva nace en el Plugin durante la implementación. La confirmación visual del Owner es confirmación, nunca la
    única prueba.
14. **Verificación dinámica por rack sobre todas las vistas admisibles.** Antes de pedir la línea, cada rack lógico
    verifica sobre su diseño reflejado: la ausencia de metadata semántica desconocida en el payload y en el exterior; la
    autoridad de dependencias y la estabilidad dinámica de sus decisiones; la evidencia visual-geométrica de las piezas con
    cambio de mano; la conmutación de **todas** las vistas admisibles que el rack podría materializar después con
    Insertar —estén o no seleccionadas—; el BOM, con el multiconjunto de líneas de su builder y con la clave con que
    producción lo consolida; y la fidelidad del store. La enumeración de vistas es pura: no busca vistas hermanas en el
    dibujo, no las añade a la selección y no las crea. Es la garantía ejecutable del fail-closed: una regla estática que
    resulte falsa para un rack concreto, en cualquier vista que la copia pueda generar, no llega a mutar.

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
    todas sus piezas necesitan aceptar un cambio de mano; si la evaluación no es viable en AutoCAD 2025, no tiene paridad
    con el materializador ni un punto de observación soportado, o la biblioteca usa clases, variantes o apariencias fuera
    de la política (textos, atributos, sombreados con patrón o degradado, sólidos 3D, polilíneas con ancho, mitades con
    fuentes visuales o rutas de visibilidad distintas), esas piezas —y con ellas kinds enteros— fallan cerrado;
  - la **representabilidad depende de la biblioteca efectiva** y puede reducirse cuando una pieza no satisface el contrato
    visual-geométrico. La biblioteca no está versionada en el repositorio y su ruta la configura el usuario: otra
    biblioteca puede dar otro resultado, siempre con fallo cerrado. La evidencia de una biblioteca concreta es indicativa,
    se identifica por su SHA-256 y está en la Proposal (§1.4); la caracterización es la autoridad;
  - la canonización conservadora puede dejar fuera piezas visualmente simétricas cuyos trazos sin continuidad
    demostrable difieren en segmentación u orientación, hasta que la caracterización habilite más con evidencia;
  - la **selección es todo-o-nada**: un solo rack que no pase hace fallar el comando entero y no se refleja ninguno;
  - `RACKMIRROR` no corrige racks espejados antes con el `MIRROR` nativo (escala negativa): esas fuentes fallan cerrado;
  - los racks legados con **miembros retirados** fallan cerrado: si el store actual los descarta, hay que abrirlos con
    `RACKEDITAR` y Actualizar antes de reflejarlos; si el store los conserva (el peralte retirado del larguero alto de
    Push Back), no hay remedio en I-52;
  - la evaluación visual-geométrica (fuentes simbólicas, rutas de visibilidad, orden visual, canonización y clonado
    aislado desde la caché incluidos) tiene un coste por pieza y estado que se mide en AutoCAD 2025 antes de exponer el
    comando;
  - lo importado de la biblioteca puede sobrevivir a un fallo;
  - todo tipo, miembro o valor de enum nuevo alcanzado desde los tipos raíz exige clasificar su regla de espejo antes de
    integrarse;
  - la aceptación de este ADR espera a la caracterización (G3), lo que añade una ronda del Owner antes de implementar; si
    G3 contradice materialmente el contrato o reduce materialmente el alcance que el Owner aceptó, se abre una Proposal
    V8 antes de pedirla.
- Vigilar: cada kind o vista nueva debe declarar su reflector, su decodificación de sección, su conjunto de vistas
  admisibles y su exposición; todo miembro nuevo y toda propiedad vinculable nueva necesitan clasificación antes de
  integrarse; todo offset gráfico nuevo de un builder de vista admitida debe caracterizarse.

## Referencias

- ADR-0009 (identidad GUID), ADR-0010 (Actualizar/Insertar), ADR-0031 §8 (reflexión rígida del lado B; texto y cota
  solo se trasladan), ADR-0034 §9, §12 y §14, ADR-0035 (cotas por tipo de vista).
- I-51: `RackDuplicationPlan`, `RackEnvelopeRestamp.RestampEnvelope(payload, name, Guid)` (Plugin).
- I-47 G9.1: `ViewBlockDraw.PrepareRedraw`, `SystemBlockWriter.RedefineInTransaction`, `PreparedViewRedraw`,
  `ProjectVariableMutationExecutor`.
- I-50: `DimensionViewVisibility`, `DimensionViewPolicy`.
- I-54 Proposal V5 D-21 (invariante de preservación del sobre, que el espejo cumple) y sus residuales F-14a y F-14b;
  ADR-0039 (aceptado en su rama).
- I-49 ADR-0038 (aceptado en su rama; G5 solo sintáctico, sin `PlanReadSet` ni dominio integrados): `PlanReadSet` y
  dominio del consumidor declarado por el descriptor; guardas de texto de `ProjectVariablesConformanceTests`.
- I-53 E1 (integrada en `main`): guardas C-08 y C-10 sobre `RackCad.Application.Systems.Shared`; ADR-0037 (aceptado).
- I-19: `CatalogBlockParameters`, `CatalogBlockManifest`.
- `src/RackCad.Application/Geometry/Transform2D.cs`; `src/RackCad.Application/Geometry/Vector2D.cs` (`GeometryTolerance`);
  `src/RackCad.Application/Persistence/RackProjectStore.cs`; `src/RackCad.Application/Persistence/RackProject.cs`;
  `src/RackCad.Application/Systems/Selective/SelectiveEffectiveDesignResolver.cs`;
  `src/RackCad.Application/Systems/Selective/SelectiveGeometryResolver.cs`;
  `src/RackCad.Application/Systems/Selective/SelectivePostGeometry.cs`;
  `src/RackCad.Application/Systems/Selective/SelectiveFrontalBuilder.cs`;
  `src/RackCad.Application/Systems/PushBack/PushBackRearTopeBuilder.cs`;
  `src/RackCad.Application/Persistence/RackEmbedDocument.cs`;
  `src/RackCad.Application/Catalogs/BlockLibrary.cs` (ruta configurable de la biblioteca);
  `src/RackCad.Plugin/Drawing/BlockLibraryImporter.cs` (caché de sesión de la biblioteca);
  `src/RackCad.Plugin/Drawing/LateralHeaderDrawer.cs` (`ApplyDynamicParameters`).

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
- **Borrador V7** — este texto, con Proposal V7. Cambios respecto del V6, por la revisión de Arquitecto de Proposal V6
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

  Sigue **propuesto**: su aceptación se pide después de G3 y antes de G4.
