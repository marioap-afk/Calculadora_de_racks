# ADR-0036: RACKMIRROR es un espejo semántico por copia: reflexión canónica por kind, vistas admisibles y colocación sin escala negativa

- **Estado:** **propuesto**
- **Fecha:** 2026-09-12 (propuesto; borrador corregido con Proposal V2 el mismo día)
- **Decisores:** Mario Pérez, Owner del repositorio (**acepta o rechaza**; pendiente, no en el gate de Proposal V2);
  Coordinador de I-52 y Arquitecto de I-52 (consenso técnico **pendiente** sobre la Proposal); Claude (redacción)
- **Iniciativa relacionada:** I-52 — `feature/rackmirror-espejo-semantico`
  ([contrato](../initiatives/I-52-rackmirror-espejo-semantico.md),
  [Discovery](../initiatives/I-52-discovery.md), [Proposal V1](../initiatives/I-52-proposal-v1.md) (historial),
  [Proposal V2](../initiatives/I-52-proposal-v2.md), [decisiones](../automation/decisions/I-52.md))

> **Numeración.** Un número de ADR queda reclamado por su primera publicación observable en un ref remoto. Este ADR se
> publicó por primera vez con el número 0036 en `origin/feature/rackmirror-espejo-semantico`, commit
> `0fc7032bf15d03e7d478bbd9350f156708621c9d` (corrida de CI del push `34731908035`, creada el `2026-09-13T01:59:44Z`),
> sin publicación anterior de otro 0036 en ningún ref. Antes de pedir la aceptación del Owner se vuelve a buscar 0036 en
> todos los refs; si apareciera una publicación anterior, este ADR se renumera antes de la aceptación. Una vez
> `aceptado` no se renumera, y dos ADR aceptados con el mismo número detienen el trabajo hasta que decida el Owner
> (Proposal V2 §16).

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
costura de I-47 G9.1 que prepara planes e importa bloques antes de una única transacción del llamador. Tres hechos del
código condicionan el diseño: solo el Selectivo guarda en el sobre un documento authored propio, y los demás kinds se
leen y escriben a través del dominio del proyecto; la definición de bloque tiene un `Origin` que forma parte de la
transformación efectiva; y la importación de la biblioteca de bloques es de mejor esfuerzo y ocurre fuera de la
transacción del dibujo.

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
   `Section` del sobre se decodifican como las lee producción: el legado inequívoco que producción ya soporta se escribe
   en su **forma canónica** (por ejemplo, la frontal Selectiva con `−1` pasa a fondo `0`), y lo que no es inequívoco o
   producción rechaza falla cerrado. La igualdad de sección cruda entre origen y copia **no** es un invariante.
5. **Fail-closed.** Si una referencia seleccionada no es admisible, si un rack no es representable
   (`REQUIRES_MODEL_CHANGE`) o su representabilidad es desconocida (`UNKNOWN` material), la operación entera termina
   **antes** de mutar. No hay prompt de eje semántico. Lo que el modelo declara representable pero depende de anclajes
   de los builders es provisional hasta caracterizarse; una contradicción reabre la Proposal, no se parchea.
6. **Reflectores puros en Application sobre el sustrato real de cada kind**, despachados por kind fuera del comando.
   El Selectivo se refleja sobre su documento authored (`SelectivePalletDesignDocument`), sin `WithDesign` y sin
   materializar vínculos. Dinámico, Push Back, Cantilever y Cabecera se leen con la autoridad existente, se reflejan en
   su dominio, se reconstruyen con las fábricas de `RackProject` y se escriben con el mismo store, llevando los
   metadatos con `WithSourceMetadataFrom`. No se crean DTO para homogeneizar, no cambia el schema y no hay manipulación
   de JSON. **Regla de fidelidad:** el espejo no promete conservar lo que el round-trip productivo de `RACKEDITAR` →
   Actualizar ya pierde para ese kind, pero toda pérdida adicional falla cerrado. Una normalización solo es válida si
   conserva la física, el BOM, las variables y los portadores, con round-trip e involución demostrados, y declara el
   cambio de representación authored cuando lo hay.
7. **Autoridad pura de planes por vista en Application** y un **materializador genérico en el Plugin** que crea la
   definición dentro de la transacción del llamador, escribe el payload preparado y crea la referencia con la
   colocación ya calculada. La autoridad recibe el nombre lógico de la copia para las anotaciones. El comando no conoce
   Left/Right, A/B, estaciones ni reglas de cabecera. Una pieza sin bloque en el dibujo es **fallo duro** para el
   espejo: nunca se crea una copia incompleta, y los drawers existentes no cambian.
8. **Colocación canónica.** Con la reflexión de la hoja `G`, la colocación efectiva de la fuente
   `P = T(p)·R(θ)·S(s,s)·T(−o)` —donde `o` es el `Origin` de su definición— y la reflexión local de la vista `F`, la
   referencia nueva es `P' = G·P·F`, con determinante positivo y la escala uniforme de la fuente. Para una vista
   admitida, `P'·Π(μ_k D) = G·P·Π(D)`: la geometría física coincide con el espejo geométrico y los textos y cotas se
   regeneran legibles. Las definiciones nuevas se crean con `Origin = 0`.
9. **Identidad nueva.** Un `NewRackId` por grupo lógico, compartido por sus vistas seleccionadas; el mapa
   `OldRackId → NewRackId` vive solo en memoria; el diseño se refleja **antes** de componer el sobre con el sobre fuente
   real y de re-estampar la identidad con la entrada única de I-51.
10. **Portadores intactos.** `SchemaVersion`, `ExtensionData` según la fidelidad del store de cada kind,
    `PropertyValues` (incluidos tipos desconocidos), literales congelados, `VariableId` y todo portador de iniciativas
    integradas (`DimensionViews`, propiedades de rack) viajan desde el origen. El espejo no crea, modifica, desvincula
    ni materializa variables y no escribe el registro. Para dibujar consume en **solo lectura** la autoridad efectiva
    existente, con su mismo criterio de acreditación del registro: un registro no acreditable o un vínculo roto fallan
    cerrado solo para los kinds cuya autoridad consume el registro.
11. **Fuente canónica.** Solo `BlockReference` de Model Space, no MINSERT, con `Normal = +Z`, rotación finita, escala
    uniforme positiva (`(−s,−s)` se canoniza a `(s,s)` + π), definición con **`Origin = 0`** y `View`/`Section`
    decodificables. Escala no uniforme, una sola componente negativa, `sz < 0` o una definición con BASE movida fallan
    cerrado. La línea se captura en el UCS con Z paralela a la de WCS y se convierte a WCS. Las tolerancias son las
    absolutas del repositorio; no se introduce tolerancia relativa.
12. **Atomicidad semántica, no de infraestructura.**
    `ACQUIRE → SNAPSHOT ALL → PREFLIGHT ALL → LINE → PREPARE ALL → MUTATE ALL → COMMIT`, con una sola transacción de
    escritura para todas las definiciones, payloads y referencias: cualquier fallo revierte todas las copias. Una
    huella de cada referencia fuente (identidad, transformación, capa, `Origin` y payload) se re-verifica antes de
    mutar. La importación de la biblioteca de bloques en PREPARE es **de mejor esfuerzo**: ocurre fuera de esa
    transacción, puede arrastrar dependencias (bloques anidados, capas, estilos), puede quedar parcial y puede
    **permanecer** aunque la operación falle. PREPARE re-verifica los bloques requeridos. El comportamiento del UNDO sobre
    lo importado es desconocido hasta la validación del Owner y no se promete.
13. **Mano y simetría de bloques.** Ningún bloque DWG se asume simétrico. Una diferencia de mano entre el plan
    reflejado y el plan original transformado solo es equivalente si el bloque figura en una **declaración tipada
    única** en Application, con eje local y centro de simetría, respaldada por caracterización y evidencia del Owner.
    Ninguna holgura, offset gráfico o parámetro con semántica de lado se asume simétrico: sin regla, falla cerrado.
14. **Verificación dinámica por rack.** Antes de pedir la línea, cada grupo verifica sobre el rack concreto la igualdad
    del BOM con la clave con que producción lo consolida, la conmutación de cada vista seleccionada y el round-trip del
    store. Es la garantía ejecutable del fail-closed: una regla estática que resulte falsa para un rack concreto no llega
    a mutar.

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
- **Soportar definiciones con BASE movida** — diferida: exige componer el origen en la colocación y en los planes; el
  primer corte falla cerrado.
- **Metadato de simetría en los catálogos (`assets/`)** — diferida: cambio de catálogo compartido; el primer corte usa
  la declaración tipada en Application.

## Consecuencias

- Positivas: la copia reflejada es un rack RackCad normal que sobrevive a guardar, reabrir, `RACKEDITAR` →
  Actualizar, Insertar, BOM y cambios de variable; el comando no dispersa casos por sistema; la agrupación de I-51 se
  reutiliza; Application no depende de AutoCAD; el espejo no empeora la fidelidad de ningún store.
- Negativas / costos aceptados: en el primer corte no se reflejan laterales de Selectivo, Dinámico, Push Back ni
  Cantilever, ni la cama, ni diseños no representables (esquinas, brazos C/L sencillos, ciertas ausencias de Push
  Back); un Dinámico dibujado solo en lateral no puede reflejarse; los Selectivos con topes, las cabeceras de plantilla
  con diagonales automáticas y las definiciones con BASE movida fallan cerrado hasta nueva decisión; la equivalencia
  exige declarar o caracterizar la mano de los bloques DWG, que no están versionados; lo importado de la biblioteca
  puede sobrevivir a un fallo; los miembros desconocidos que el round-trip productivo ya pierde tampoco los conserva el
  espejo.
- Vigilar: cada kind o vista nueva debe declarar su reflector, su decodificación de sección y su exposición; todo campo
  nuevo direccionado por poste, fondo, módulo, lado o estación necesita regla de reflexión antes de integrarse; todo
  offset gráfico nuevo de un builder de vista admitida debe caracterizarse.

## Referencias

- ADR-0009 (identidad GUID), ADR-0010 (Actualizar/Insertar), ADR-0031 §8 (reflexión rígida del lado B; texto y cota
  solo se trasladan), ADR-0034 §9, §12 y §14.
- I-51: `RackDuplicationPlan`, `RackEnvelopeRestamp.RestampEnvelope(payload, name, Guid)` (Plugin).
- I-47 G9.1: `ViewBlockDraw.PrepareRedraw`, `SystemBlockWriter.RedefineInTransaction`, `PreparedViewRedraw`,
  `ProjectVariableMutationExecutor`.
- I-54 Proposal V2 D-21 (invariante de preservación del sobre, que el espejo cumple).
- `src/RackCad.Application/Geometry/Transform2D.cs`; `src/RackCad.Application/Geometry/Vector2D.cs` (`GeometryTolerance`);
  `src/RackCad.Application/Persistence/RackProjectStore.cs`; `src/RackCad.Application/Persistence/RackProject.cs`;
  `src/RackCad.Application/Systems/Selective/SelectiveEffectiveDesignResolver.cs`;
  `src/RackCad.Plugin/Drawing/BlockLibraryImporter.cs`; `src/RackCad.Plugin/Drawing/LateralHeaderDrawer.cs`.

## Historial del borrador

Mientras este ADR sea `propuesto` puede editarse (adr/README.md). Para no perder la historia:

- **Borrador V1** — publicado con Proposal V1 en `0fc7032` (texto íntegro recuperable con
  `git show 0fc7032:docs/adr/0036-rackmirror-espejo-semantico-por-copia.md`).
- **Borrador V2** — este texto, con Proposal V2. Cambios respecto del V1, por la revisión de Arquitecto de Proposal V1
  (`Architect: CHANGES REQUIRED — PROPOSAL V2`, hallazgo AR-13):
  - decisión 6: sustrato real por kind y regla de fidelidad, en lugar de «documento del mismo kind en la entrada y la
    salida»;
  - decisión 4: sección canónica y fail-closed para lo no inequívoco, en lugar de sección idéntica;
  - decisiones 8 y 11: `Origin` en la colocación y `Origin = 0` como restricción de la fuente;
  - decisión 12: importación de mejor esfuerzo con dependencias, sin atomicidad de infraestructura y con UNDO
    desconocido, en lugar de «única mutación permitida»;
  - decisiones 7 y 10: nombre lógico en la autoridad de planes, pieza faltante como fallo duro y criterio de
    acreditación del registro por kind;
  - decisiones 13 y 14 nuevas: política de mano y simetría y verificación dinámica por rack;
  - nota de numeración: precedencia por primera publicación observable, en lugar de «gana quien integre primero».
