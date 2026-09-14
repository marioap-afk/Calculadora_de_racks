# ADR-0042: Preparar la vista antes de materializarla: primera vista libre, varias vistas de un rack en un flujo y colocación de grupo que conserva la identidad

- **Estado:** propuesto
- **Fecha:** 2026-09-14 (propuesto; revisado con las Proposals V2, V3 y V4 de I-55)
- **Decisores:** pendiente. Solo el Owner del repositorio acepta o rechaza. Coordinador de I-55: **CHANGES REQUIRED** sobre las
  Proposals V1 y V2, decisión CQ-01 sobre la V3 (redibujo atómico de hermanas) y **REVIEW REQUIRED** sobre la V4; Arquitecto formal:
  **PENDING**; sin consenso técnico. Claude (redacción)
- **Iniciativa relacionada:** I-55 — `feature/creacion-de-vistas`
  ([contrato](../initiatives/I-55-creacion-de-vistas.md), [Discovery](../initiatives/I-55-discovery.md),
  [Proposal V4](../initiatives/I-55-proposal-v4.md), [mapa de implementación V4](../initiatives/I-55-implementation-map-v4.md),
  Proposals [V1](../initiatives/I-55-proposal-v1.md), [V2](../initiatives/I-55-proposal-v2.md) y [V3](../initiatives/I-55-proposal-v3.md)
  como registro,
  [registro de I-55](../automation/decisions/I-55.md))
- **No reemplaza a ninguna ADR.** **Complementa** a [ADR-0010](0010-actualizar-redibuja-insertar-liga-vistas.md), que sigue `aceptado`
  (ver «Relación con ADR-0010»)

> **Estado de este registro.** `propuesto`. **No** hay consenso técnico (Coordinator = REVIEW REQUIRED, Architect formal = PENDING,
> Consensus = NOT REACHED), **no** autoriza implementación y puede editarse hasta que el Owner lo acepte o lo rechace.
>
> **Precondiciones de aceptación.**
> 1. Consenso técnico sobre la misma versión del plan.
> 2. **M-01 resuelta**: la semántica de colocación de grupo expuesta, el marco fuente, el marco destino, el modo por par de vistas, los
>    tipos y familias mezclados y la variante en misma clase (OD-6.b/c/d, OD-7.a/b/c/d y OD-2.b) son material abierto.
> 3. Decisiones del Owner OD-1..OD-8.
> 4. Reconciliación con I-52 de las autoridades compartidas del punto 9, registrada por ambas iniciativas ([V11-D10], [V12-D11], [V13-D08], [V14-D08], [V14-D14]).
>
> El texto final incorporará lo que se decida.
>
> **Numeración.** Se redactó como 0041, número que I-49 publicó en su rama (`8cefd59`) antes del primer commit de este registro; por eso
> es 0042. Si otra rama integrara antes un 0042, este registro tomaría el siguiente número libre antes de integrarse.

## Contexto

RackCad modela un rack lógico como el conjunto de definiciones de bloque cuyos sobres comparten `RackEmbedDocument.Id` (ADR-0009); cada
definición es una vista y cada referencia una colocación. ADR-0010 fija que, desde `RACKEDITAR`, **Actualizar** redibuja las
representaciones existentes e **Insertar** agrega una representación ligada con el mismo GUID. También dice que «Una vista adicional solo
se inserta desde un rack ya existente, para que disponga de diseño e identidad fuente.»

La Discovery de I-55 auditó `ba497f1` y lo volvió a medir sobre `dad4e77`:

- la primera vista está restringida por la interfaz (Selectivo solo frontal; Dinámico y cabecera solo lateral), no por la persistencia;
- cada gesto inserta una vista; la variante la decide el Plugin o la ventana, y en Push Back la lateral usa la posición de una lista;
- el payload se compone por comando en el Plugin y la definición se confirma antes del jig, que solo fija la posición;
- la resolución de un rack sin editor es privada de cada handler del BOM, que solo expone el BOM y un motivo de bloqueo;
- no existe forma de generar una clase de vista para varios racks existentes conservando su identidad; las celdas enlazadas de
  `RACKLAYOUT` son referencias de una definición con el mismo `Id`;
- una vista nueva copia las propiedades personalizadas de la vista elegida sin compararlas con sus hermanas;
- el redibujo previo a Insertar confirma vista por vista e ignora los fallos, pide la variante después de redibujar y conserva las vistas
  huérfanas si son las únicas del rack;
- una pieza sin bloque se anota como faltante, pero varios caminos de inserción y de redibujo no lo informan, y algunos builders omiten
  la pieza antes del plan;
- la planta y las elevaciones usan ejes locales distintos, así que las posiciones de un layout en planta no son posiciones de elevación.

El Owner fijó ID17, ID18 e ID19, que comparten una necesidad: **preparar la representación de una vista antes de materializarla**. La
Proposal V14 de I-52 (`RACKMIRROR`, sin consenso) necesita autoridades de la misma familia y las declara provisionales hasta
reconciliarlas con I-55.

## Decisión

1. **Preparación y colocación son capas distintas.**
   - La preparación vive en Application, es pura y parte del **sistema ya resuelto**.
   - La colocación vive en el Plugin: importa, verifica, crea con nombre único, escribe, coloca y limpia la definición sin referencias si
     no llega a colocarse, **también ante una excepción**.
   - Los builders siguen siendo la única autoridad geométrica.
   - La preparación usa una **semilla de nombre** calculada antes del plan, que reproduce los nombres base vigentes, así que el plan nunca
     necesita el nombre único.
2. **Una sola autoridad de resolución sin editor.** Convierte un diseño authored, persistido o en memoria, en su sistema efectivo con un
   resultado tipado: resuelto, legado no soportado, bloqueado, referencia rota, dependencia no disponible o ilegible.
   - Cada consumidor aplica su política: el BOM conserva la suya, por tipo de sistema y sin cambio observable, y la colocación de grupo
     solo continúa con un sistema resuelto cuyas hermanas acepta `RACKEDITAR`.
   - Un legado cuyo sistema resuelto no reproduce lo dibujado nunca se aproxima; la paridad se caracteriza antes de implementar.
3. **Una sola taxonomía de tipo de vista, una variante tipada y tres responsabilidades separadas.**
   - `RackViewKind` renombra `DimensionViewKind`, sin cambio de la política de ADR-0035. `CantileverViewKind` es un enum de cámara del
     builder, no una segunda taxonomía.
   - La representación dentro del tipo es una variante semántica, nunca un índice de interfaz.
   - El **codec sintáctico** traduce `View` y `Section` a una dirección y su disposición (`Canonical`, `Canonicalizable`, `Coerced`,
     `Invalid`) sin consultar el sistema resuelto.
   - La **disponibilidad** dice si el sistema resuelto tiene esa dirección (`Available`, `Orphaned`, `Unsupported`).
   - Cada **consumidor** aplica su política: `RACKEDITAR` conserva su lectura, y la colocación de grupo acepta solo `Canonical` o
     `Canonicalizable` disponibles y falla cerrado ante el resto.
   - **La persistencia no cambia.**
4. **Vista soportada** = builder + codec y edición que la reconocen + existencia en el sistema resuelto; una matriz normativa por sistema.
   La cama de rodamiento no admite hermanas.
5. **Identidad.**
   - Un rack nuevo recibe su `RackId` una vez, al aceptarse la intención de insertar una o varias vistas.
   - Una hermana hereda el del rack, y la colocación de grupo conserva el `RackId` de cada rack.
   - La edición conserva la curación vigente de un `Id` en blanco (`IsNullOrWhiteSpace`).
   - Los defectos que impiden el contrato (poste por posición de lista en Push Back; visibilidad de la planta Cantilever, corregida solo
     en el Plugin) se corrigen en prerrequisitos aislados **antes** de la foundation.
6. **Varias vistas de un rack en un flujo.** Redibujar las hermanas existentes y colocar las vistas nuevas son dos operaciones distintas.
   - En un rack existente, la variante de cada vista nueva se elige antes de comprobar las hermanas y de redibujarlas.
   - **Redibujo atómico de hermanas antes de Insertar.** Las hermanas propias del dibujo se clasifican antes de escribir: las que se
     redibujan, las huérfanas y, aparte, las dependientes de una referencia externa, que gobierna su dibujo de origen y no se tocan. Todo
     lo que el redibujo necesita se prepara sin escribir estado del rack (solo se importan definiciones de biblioteca). Después, **una**
     transacción redibuja todas las hermanas y borra las huérfanas, y se confirma una sola vez; la purga, los renombres cosméticos y la
     regeneración van después del commit. Si algo falla al preparar, no se escribe nada del rack; si falla dentro de la transacción, no se
     aplica ninguna actualización a las vistas existentes y no se inserta ninguna vista.
   - Si no queda ninguna hermana propia que redibujar, las huérfanas se borran dentro de la transacción que coloca la primera vista nueva,
     para no destruir la identidad del rack si esa colocación se cancela.
   - Los controles previos vigentes de cada editor siguen antes de todo, y la vista elegida que el flujo vigente añade cuando su `Id` está
     en blanco se redibuja con el `Id` curado, como hoy.
   - **Colocación de vistas nuevas**, solo tras el commit del redibujo: cada colocación se confirma por separado, ninguna transacción de
     escritura queda abierta entre jigs, y Esc o Enter detienen la cola conservando el redibujo y las vistas ya colocadas.
   - Actualizar conserva su comportamiento vigente.
7. **Ninguna hermana nace distinta de las demás.**
   - Antes de crear una vista en un rack existente se comprueban, **acotadas a ese `RackId`**, las propiedades personalizadas del sobre
     elegido y de las hermanas conocidas, con la igualdad canónica de I-54; la vista nueva hereda la colección común.
   - Tipos distintos, colecciones no escribibles, propiedades divergentes o un payload no interpretable **cuyo `Id` de nivel superior sea
     legible y coincida** fallan cerrado con remedio.
   - Un payload no interpretable con el `Id` de otro rack, o sin `Id` legible, no bloquea.
   - Fuera del editor, la autoridad authored de todas las hermanas debe ser única.
   - No se reabre ADR-0039 ni cambia `RACKPROPIEDADES`.
8. **Colocación de grupo.**
   - **Grupos.** Una ejecución proyecta **una selección**; sus referencias se agrupan por `RackId`, y cada definición fuente del grupo
     recibe una definición nueva con una referencia por cada referencia seleccionada (con una sola definición fuente por rack, una por
     grupo).
   - **Foundation.** **Una sola transformación rígida común por ejecución** entre un **marco fuente** y un **marco destino**, aplicada al
     ancla física de cada referencia. Cada vista declara su ancla con descriptores de marco caracterizados contra los builders, con
     **tramos por variante**.
   - **Política rígida.** El layout de anclas se reproduce rotado y trasladado.
   - **Política ortográfica.** Conserva el eje físico compartido, con la **dirección destino derivada de la familia, el tipo y la
     variante destino de cada referencia**. Alinea el eje común y colapsa el descartado; la superposición se detecta por intersección de
     intervalos. Lo que conserva es el intervalo de cada referencia sobre el eje común, no la coordenada descartada ni las orientaciones.
   - **Exposición.** Qué pares, modos y variantes se exponen, y cómo se orientan los marcos, **lo decide M-01**.
   - **Secuencia.** Todo se resuelve, se calcula sobre la selección entera, se valida y se planifica antes de pedir puntos. Toda pieza
     del plan que requiere un bloque de biblioteca debe tener una clave válida (comprobada antes de los puntos) presente tras importar,
     o **no se escribe ninguna definición ni referencia de rack**. Se materializa en una transacción, sin identidad nueva, re-estampado
     ni regeneración.
9. **Una autoridad por responsabilidad entre iniciativas.** Cada una de estas responsabilidades tiene una sola autoridad, compartida con
   I-52 y extraída una sola vez; las políticas quedan en cada llamador:
   - selección;
   - taxonomía, codec sintáctico y disponibilidad (extraídas solo por I-55, porque I-52 excluye los archivos que tocan);
   - resolución sin editor (extraída solo por I-55, con la delegación de los handlers del BOM);
   - plan desde el sistema resuelto (extraído solo por I-55);
   - comparación authored;
   - primitivo de materialización y requisito estructural de bloques;
   - valor de colocación sobre `Transform2D`, descomposición de la transformación fuente y tolerancia de escala;
   - origen y tramo del eje de una vista (extraídos solo por I-55).

## Relación con ADR-0010

**ADR-0042 complementa a ADR-0010 y no lo reemplaza; ADR-0010 sigue `aceptado`.**

ADR-0010 acota su propio alcance: «Esta decisión gobierna el flujo de edición. No cambia la inserción inicial que crea un rack nuevo ni
promete capacidades multivista para todos los editores.» Este registro no revierte ninguna de sus decisiones y añade dos cosas:

1. **Generaliza el origen válido de una hermana.**
   - **A.** Un rack lógico materializado: es la regla de ADR-0010, que conserva su propósito de disponer de diseño e identidad fuente.
   - **B.** Una intención de creación aceptada con `RackId` único, autoridad authored, sistema resuelto y vistas preparadas: rige el
     flujo de creación que ADR-0010 deja fuera de su alcance.

   Así, las vistas 2..N del lote de un rack nuevo no necesitan fingir que provienen de un rack físicamente colocado.
2. **Añade precondiciones al flujo de Insertar**, que ADR-0010 delega en «su flujo implementado»: la comprobación de propiedades
   personalizadas acotada al `RackId` (punto 7) y el redibujo atómico de las hermanas antes de colocar (punto 6).

**Todo lo demás de ADR-0010 sigue vigente:**
- Actualizar reconstruye y redefine en sitio sin crear otro rack ni insertar vistas.
- Insertar crea representaciones ligadas con el mismo GUID mediante el flujo de colocación.
- `View` y `Section` distinguen la representación sin convertirla en otro rack.
- La definición contiene la geometría y el sobre, y la referencia solo la coloca.
- La cama solo redibuja su vista lateral.

**Registro de la relación.** Cuando el Owner acepte este registro, y solo entonces, se añadirá en ADR-0010 una entrada fechada en
«Notas posteriores» que lo enlace, como admite el [índice de ADR](README.md). Hasta entonces ADR-0010 no se modifica. Hay precedentes de
complemento declarado (ADR-0028, ADR-0029) y de nota posterior que enlaza un ADR posterior en un aceptado no reemplazado
([ADR-0005](0005-estrategia-de-unidades.md), sobre ADR-0021).

## Alternativas consideradas

- **Seguir por sistema con `View` como texto y `Section` como entero**: repite el defecto de índice de interfaz y deja lógica no probable.
- **Framework genérico de proveedores**: sin segundo cliente real.
- **Registro lógico persistente de racks**: formato nuevo en el dibujo y migración de todos los DWG.
- **Segundo enum de vista con mapeo**: dos taxonomías para un concepto.
- **Acuñar el `RackId` al abrir el editor o en la primera colocación**: GUIDs sin uso, o identidad inventada en el Plugin.
- **Una transacción para todo el lote, incluidos los jigs, o preparar cada vista justo antes**: dejaría sin confirmar el redibujo y todas
  las vistas colocadas a lo largo de varias interacciones del usuario y convertiría Esc en rollback de vistas ya colocadas, o fallaría
  tras colocar.
- **Confirmar el redibujo de cada hermana por separado y detenerse ante un fallo** (versión anterior de este registro): dejaba un
  subconjunto de hermanas confirmado con el diseño nuevo. Sustituida por el redibujo atómico, que reutiliza el primitivo de redefinir en
  la transacción del llamador que ya usa la mutación de variables de proyecto.
- **Renombrar dentro de la transacción del redibujo**: el nombre es cosmético por contrato vigente; después del commit no corre si hay
  rollback.
- **Resolver con el camino del BOM tal cual**: no expone el sistema ni un resultado tipado.
- **Codec que consulta el sistema resuelto**: mezcla sintaxis y disponibilidad e impide compartirlo.
- **Verificar solo la unión de nombres de bloque**: una pieza sin nombre desaparece de la unión.
- **Traslación literal como única semántica de grupo**: apila elevaciones de un layout en planta; queda representable como caso rígido.
- **Proyección ortográfica como única semántica de grupo** (primera versión de este registro): no representa misma clase, frontal ↔
  lateral, orientaciones distintas ni un marco destino orientable.
- **Dirección ortográfica tomada de una familia fija**: rechaza pares Cantilever soportados.
- **Una transformación por grupo `RackId`**: no conserva el layout entre racks.
- **Transformación afín general**: escala y cizalla carecen de sentido para vistas 1:1.
- **Usar la autoridad global de propiedades de I-54**: un payload ilegible no relacionado bloquearía cualquier rack.
- **Copiar la colección de la vista elegida sin comparar** (primera versión): permite que una hermana nazca distinta.
- **Reemplazar ADR-0010**, por completo (primera versión) o de forma acotada: reescribe o marca como reemplazadas decisiones que no
  cambian.

## Consecuencias

**Positivas.**
- Un rack empieza por cualquier vista soportada.
- Varias vistas se colocan en un flujo con un `RackId`.
- Varios racks reciben una clase de vista con una transformación común y conservando su identidad.
- Ninguna hermana nueva nace distinta de las demás.
- La resolución sin editor tiene una sola autoridad.
- Ninguna instancia del plan desaparece del diagnóstico; las que un builder omite antes del plan quedan censadas.
- Las reglas son puras y probables sin AutoCAD.
- Conteos intactos y sin migración de dibujos.

**Negativas / costos aceptados.**
- Se re-enrutan todas las inserciones y los handlers del BOM delegan su resolución, con goldens y validación del Owner.
- Cambian a propósito censos y guardas.
- Insertar en un rack con propiedades divergentes o ilegibles falla con remedio; un redibujo fallido no aplica ninguna actualización ni
  inserta ninguna vista.
- En Insertar, el prompt de variante pasa a antes del redibujo, y las hermanas dependientes de una referencia externa dejan de
  redibujarse.
- Una transacción de escritura para todas las hermanas de un rack; las definiciones de biblioteca importadas al preparar no se revierten.
- Actualizar sigue redibujando vista por vista y puede quedar redibujado en parte, como hoy.
- En la proyección ortográfica, el sentido del eje común lo decide la mayoría de referencias: girar racks puede invertir el orden de la
  elevación.
- El remedio puede exigir reparar antes lo que deja `RACKPROPIEDADES` en solo lectura.
- La colocación de grupo depende de descriptores y tramos que deben seguir a los builders.
- Un payload ilegible sin `Id` atribuible que sí pertenezca al rack no se detecta.
- La extracción de autoridades compartidas exige secuenciar con I-52, que consume en varios gates autoridades que solo extrae I-55.

## Relación con otros ADR

- [ADR-0009](0009-identidad-guid-embebida-en-dwg.md): se conserva y se precisa.
- [ADR-0010](0010-actualizar-redibuja-insertar-liga-vistas.md): complementado (sección anterior); sigue `aceptado`.
- [ADR-0011](0011-parametros-dinamicos-con-patron-array.md): sin cambio.
- [ADR-0029](0029-contrato-funcional-comun-de-ventanas-wpf.md): el diálogo de varias vistas es del arquetipo C.
- [ADR-0034](0034-project-variables-autoridad-drawing-level.md): registro de variables leído una vez por comando y consumido por la
  resolución sin editor.
- [ADR-0035](0035-visibilidad-de-cotas-por-tipo-de-vista.md): sin cambio de política; el tipo de vista cambia de nombre.
- [ADR-0037](0037-reutilizacion-de-cabecera-por-copia-y-distribucion-por-lotes.md): precedente de preparación completa antes de escribir.
- [ADR-0039](0039-custom-properties-persistencia-autoridad.md): sin cambio; I-55 solo consume su lectura y su igualdad canónica.
- ADR-0036 (propuesto en la rama de I-52): autoridades compartidas del punto 9, cuya propiedad se reconcilia entre I-52 e I-55, sin
  duplicarlas, antes de congelar cualquiera de los dos contratos.

## Referencias

- [Proposal V4 de I-55](../initiatives/I-55-proposal-v4.md): §0, §4.4-§4.6, §7.3, §9, §11, §12, §17.2 y §22 (D-01..D-19, OD-1..OD-8,
  M-01, X-1..X-8, CQ-01).
- Proposals [V1](../initiatives/I-55-proposal-v1.md), [V2](../initiatives/I-55-proposal-v2.md) y [V3](../initiatives/I-55-proposal-v3.md)
  de I-55: registro histórico.
- [Mapa de implementación V4](../initiatives/I-55-implementation-map-v4.md).
- [`SystemBlockWriter`](../../src/RackCad.Plugin/Systems/Shared/SystemBlockWriter.cs) (`RedefineInTransaction`) y
  [`ProjectVariableMutationExecutor`](../../src/RackCad.Plugin/ProjectVariableMutationExecutor.cs): precedente de PREPARE, una MUTATE y
  POST.
- [`Transform2D`](../../src/RackCad.Application/Geometry/Transform2D.cs),
  [`CustomPropertiesCanonicalForm`](../../src/RackCad.Application/CustomProperties/CustomPropertiesCanonicalForm.cs),
  [`RackEmbedDocument`](../../src/RackCad.Application/Persistence/RackEmbedDocument.cs),
  [`RackEmbedComposer`](../../src/RackCad.Application/Persistence/RackEmbedComposer.cs),
  [`BlockPlacement`](../../src/RackCad.Plugin/Drawing/BlockPlacement.cs),
  [`LateralHeaderDrawer`](../../src/RackCad.Plugin/Drawing/LateralHeaderDrawer.cs),
  [`RackLayoutCommands`](../../src/RackCad.Plugin/RackLayoutCommands.cs),
  [`SelectivePlantaBuilder`](../../src/RackCad.Application/Systems/Selective/SelectivePlantaBuilder.cs),
  [`CantileverViewPlanBuilder`](../../src/RackCad.Application/Systems/Cantilever/CantileverViewPlanBuilder.cs).
