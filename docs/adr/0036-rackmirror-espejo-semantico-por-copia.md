# ADR-0036: RACKMIRROR es un espejo semántico por copia: reflexión canónica por kind, vistas admisibles y colocación sin escala negativa

- **Estado:** **propuesto**
- **Fecha:** 2026-09-12 (propuesto; borrador corregido con Proposal V2 y con Proposal V3 el mismo día, y con Proposal V4
  el 2026-09-13)
- **Decisores:** Mario Pérez, Owner del repositorio (**acepta o rechaza**; pendiente). La aceptación **no** es
  precondición de la caracterización (G3): se pide **después de G3**, si G3 no contradice materialmente el contrato (si
  lo contradice, se abre una Proposal V5), y **es precondición de G4**. Coordinador de I-52 y Arquitecto de I-52
  (consenso técnico **pendiente** sobre la Proposal); Claude (redacción)
- **Iniciativa relacionada:** I-52 — `feature/rackmirror-espejo-semantico`
  ([contrato](../initiatives/I-52-rackmirror-espejo-semantico.md),
  [Discovery](../initiatives/I-52-discovery.md), [Proposal V1](../initiatives/I-52-proposal-v1.md),
  [Proposal V2](../initiatives/I-52-proposal-v2.md) y [Proposal V3](../initiatives/I-52-proposal-v3.md) (historial),
  [Proposal V4](../initiatives/I-52-proposal-v4.md), [decisiones](../automation/decisions/I-52.md))

> **Numeración.** Un número de ADR queda reclamado por su primera publicación observable en un ref remoto. Este ADR se
> publicó por primera vez con el número 0036 en `origin/feature/rackmirror-espejo-semantico`, commit
> `0fc7032bf15d03e7d478bbd9350f156708621c9d` (corrida de CI del push `34731908035`, creada el `2026-09-13T01:59:44Z`),
> sin publicación anterior de otro 0036 en ningún ref. ADR-0035 (I-50) está aceptado en `main`; ADR-0037 (I-53) y
> ADR-0038 (I-49) están aceptados en sus ramas, ambos publicados después y con otro número: no hay colisión. Antes de pedir la aceptación del Owner se vuelve a buscar 0036 en todos los refs; si apareciera una
> publicación anterior, este ADR se renumera antes de la aceptación. Una vez `aceptado` no se renumera, y dos ADR
> aceptados con el mismo número detienen el trabajo hasta que decida el Owner (Proposal V4 §16).

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
de proyecto; y varios builders anclan holguras gráficas a un solo lado (el tope del Selectivo y el tope posterior de
Push Back).

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
   regla de reflexión o la arquitectura abre una Proposal V5. Una holgura gráfica anclada a un lado ya demostrada en
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
     verdad para **todo** estado acreditable futuro del registro; si no puede demostrarse, falla cerrado. Una autoridad
     explícita de dependencias declara, por regla, sus entradas, los descriptores vinculables de los que dependen, su
     predicado y la prueba o el motivo del fallo cerrado.
   - **Metadata desconocida.** Se distingue la metadata **portadora**, nombrada en una **lista cerrada** cuyo contrato es
     conservarse exactamente (versiones del sobre y del envoltorio, valores de propiedades vinculables, `DimensionViews`,
     propiedades de rack de I-54), de la metadata **semántica** del payload que se somete a `μ_k` y de la metadata
     **exterior** del sobre y del envoltorio. Un miembro desconocido no vacío en el payload, o una clave desconocida no
     vacía del sobre o del envoltorio que no esté en la lista, es `UNKNOWN` y falla cerrado, salvo declaración tipada de
     invariancia o de transformador. Ningún contenedor es portador por ser contenedor.
   - **Cobertura.** Un **cierre transitivo** calculado desde los tipos raíz del sobre, del envoltorio y de cada kind
     clasifica exactamente una vez cada miembro alcanzado, cada propiedad vinculable y cada valor de los enums
     alcanzados; un tipo, miembro, valor o descriptor nuevo sin clasificar detiene el trabajo. La guarda, no una lista
     manual, es la autoridad.
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
   afirma si cada decisión que la autoriza —incluidos los predicados de ajuste, desborde, ancho e índices— es estable
   para todo estado acreditable del registro (decisión 6, estabilidad dinámica). Las definiciones nuevas se crean con
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
    deshacer la equivalencia reflejada **ni invalidar ninguna decisión de continuar o canonizar** tomada antes de mutar.
    Una cota inferior del valor de una propiedad vinculada solo se usa para demostrarlo si la define una autoridad real
    de resolución o de dominio, nunca un control de la interfaz.
11. **Fuente canónica.** Solo `BlockReference` de Model Space, no MINSERT, con `Normal = +Z`, rotación finita y escala
    uniforme positiva (`(−s,−s)` se canoniza a `(s,s)` + π), definición con **`Origin = 0`** y `View`/`Section`
    decodificables. Escala no uniforme, una sola componente negativa, `sz < 0`, una definición con BASE movida o una
    referencia **dinámica, anónima o anotativa** que lleve datos de RackCad fallan cerrado con mensaje propio; nunca se
    ignoran como si no fueran racks. Primero se reconoce si un objeto lleva datos de RackCad: los objetos que no los
    llevan —aunque tengan escala, rotación, origen o tipo no canónicos, o contengan racks anidados— se ignoran con aviso
    y nunca abortan la operación; las restricciones estrictas solo se aplican a los candidatos RackCad. La línea se
    captura en el UCS con Z paralela a la de WCS y se convierte a WCS. Las
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
    falle. PREPARE re-verifica los bloques requeridos. El comportamiento del UNDO sobre lo importado es desconocido hasta
    la validación del Owner y no se promete.
13. **Mano y simetría de bloques, con evidencia geométrica independiente.** Ningún bloque DWG se asume simétrico. Una
    diferencia de mano entre el plan reflejado y el plan original transformado solo es equivalente si el bloque figura
    en una **declaración tipada única** en Application con: nombre exacto del bloque, vista o familia, **eje local** de
    simetría y **centro** medido en el **marco local de inserción de la pieza después de aplicar `T(−Origin)` de su
    definición** (nunca en WCS, en el marco del rack ni en coordenadas crudas de la definición), y si la declaración
    **aplica**. La evidencia tiene dos partes con orden obligatorio: primero una **evidencia geométrica** que demuestra,
    sin usar builders ni la diferencia entre planes, que la **definición real** del bloque es igual a su reflexión
    respecto de ese eje y centro en cada estado de parámetros cubierto; después una **evidencia de colocación** que
    demuestra que los builders usan esa simetría. Nunca se infiere un centro de la diferencia entre planes, del
    centro de su caja envolvente, de la diferencia de inserciones ni de un mínimo de error. Un centro afín sobre un
    parámetro dinámico solo se admite si la relación se deriva de la parametrización del bloque y se verifica en los
    estados caracterizados; ajustar una recta a unos puntos no basta. Un bloque dinámico solo queda cubierto en su
    dominio caracterizado, y la declaración se liga a la definición real que el espejo dibuja: si la definición difiere
    de la caracterizada, no aplica. Por eso una declaración nunca puede hacer equivalente una pieza con holgura u
    offset anclado a un lado, cualquiera sea su rol; el rechazo de las familias con holgura `UNKNOWN` (hoy los topes) es
    una defensa adicional. Ninguna holgura, offset gráfico o parámetro con semántica de lado se asume simétrico: sin
    regla, falla cerrado. La confirmación visual del Owner es confirmación, nunca la única prueba.
14. **Verificación dinámica por rack sobre todas las vistas admisibles.** Antes de pedir la línea, cada rack lógico
    verifica sobre su diseño reflejado: la ausencia de metadata semántica desconocida en el payload y en el exterior; la
    autoridad de dependencias y la estabilidad dinámica de sus decisiones; la conmutación de **todas** las vistas
    admisibles que el rack podría materializar después con
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
  la declaración tipada en Application.
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
    envoltorios con **claves desconocidas** fuera de la lista de portadores; las piezas con diferencia de mano sin
    evidencia geométrica aplicable; y todo rack con **alguna vista admisible** que no conmute;
  - la equivalencia exige evidencia geométrica independiente de la mano de los bloques DWG, que no están versionados, y
    ligarla a la definición real; si no puede obtenerse, las piezas afectadas fallan cerrado;
  - lo importado de la biblioteca puede sobrevivir a un fallo;
  - todo tipo, miembro o valor de enum nuevo alcanzado desde los tipos raíz exige clasificar su regla de espejo antes de
    integrarse;
  - la aceptación de este ADR espera a la caracterización (G3), lo que añade una ronda del Owner antes de implementar; si
    G3 contradice materialmente el contrato, se abre una Proposal V5 antes de pedirla.
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
- I-54 Proposal V5 D-21 (invariante de preservación del sobre, que el espejo cumple) y sus residuales F-14a y F-14b.
- I-49 ADR-0038 (aceptado en su rama; implementación no integrada): `PlanReadSet` y dominio del consumidor declarado
  por el descriptor.
- I-53 G3: guardas C-08 y C-10 sobre `RackCad.Application.Systems.Shared`.
- I-19: `CatalogBlockParameters`, `CatalogBlockManifest`.
- `src/RackCad.Application/Geometry/Transform2D.cs`; `src/RackCad.Application/Geometry/Vector2D.cs` (`GeometryTolerance`);
  `src/RackCad.Application/Persistence/RackProjectStore.cs`; `src/RackCad.Application/Persistence/RackProject.cs`;
  `src/RackCad.Application/Systems/Selective/SelectiveEffectiveDesignResolver.cs`;
  `src/RackCad.Application/Systems/Selective/SelectiveGeometryResolver.cs`;
  `src/RackCad.Application/Systems/PushBack/PushBackRearTopeBuilder.cs`;
  `src/RackCad.Plugin/Drawing/BlockLibraryImporter.cs`; `src/RackCad.Plugin/Drawing/LateralHeaderDrawer.cs`.

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
- **Borrador V4** — este texto, con Proposal V4. Cambios respecto del V3, por la revisión de Arquitecto de Proposal V3
  (`Architect: CHANGES REQUIRED — PROPOSAL V4`) y la orden del Coordinador:
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
