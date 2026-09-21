# ADR-0042: Preparar la vista antes de materializarla: primera vista libre, varias vistas de un rack en un flujo y colocación de grupo que conserva la identidad

- **Estado:** aceptado
- **Fecha:** 2026-09-14 (propuesto); 2026-09-21 (aceptado)
- **Decisores:** Owner del repositorio: **ACCEPTED** el 2026-09-21. Coordinador de I-55: **AGREED** sobre Proposal V5;
  Arquitecto formal independiente: **AGREED** sobre Proposal V5 + reconciliación G2I. Claude (borrador inicial);
  Codex (reconciliación del takeover)
- **Iniciativa relacionada:** I-55 — `feature/creacion-de-vistas`
  ([contrato](../initiatives/I-55-creacion-de-vistas.md), [Discovery](../initiatives/I-55-discovery.md),
  [Proposal V5](../initiatives/I-55-proposal-v5.md), [mapa de implementación V5](../initiatives/I-55-implementation-map-v5.md),
  Proposals [V1](../initiatives/I-55-proposal-v1.md), [V2](../initiatives/I-55-proposal-v2.md), [V3](../initiatives/I-55-proposal-v3.md)
  y [V4](../initiatives/I-55-proposal-v4.md) como registro, [registro de I-55](../automation/decisions/I-55.md))
- **No reemplaza a ninguna ADR.** **Complementa** a [ADR-0010](0010-actualizar-redibuja-insertar-liga-vistas.md), que sigue `aceptado`
  (ver «Relación con ADR-0010»)
- **Es un ADR de producto.** Los hechos y contratos neutrales que consume (tipo y dirección de vista, codec, disponibilidad, marco y
  tramo, resolución sin editor, plan, nombre base, requisito de bloques, selección y valor de colocación) los fija la
  [ADR-0044](0044-hechos-neutrales-de-vistas-compartidas.md), aceptado e integrado mediante `integration/I-57`; este registro no los
  redefine.

> **Estado de este registro.** `aceptado`. Coordinator = AGREED, Architect formal = AGREED, Owner = ACCEPTED y
> Technical Consensus = REACHED sobre Proposal V5 + reconciliación G2I. La implementación se rige por sus gates; la aceptación abre
> únicamente G3, caracterización de producto sin cambios de producción.
>
> **Precondiciones cumplidas.**
> 1. Consenso técnico sobre Proposal V5 y el receipt G2I.
> 2. **M-01 resuelta** como Relative Frame Window, incluidas OD-6.b/c/d, OD-7.a/b/c/d/e y OD-2.b.
> 3. Decisiones del Owner OD-1..OD-8 aceptadas.
> 4. ADR-0044 aceptado e I-57 integrado y consumible desde `main`.
>
> **Numeración.** Se redactó como 0041, número que I-49 publicó en su rama (`8cefd59`) antes del primer commit de este registro; por eso
> es 0042. La publicación previa de un número en una rama también cuenta al censar; no se espera al merge. Este número ya publicado por I-55 se conserva; el ADR neutral sigue sin número.

## Contexto

RackCad modela un rack lógico como el conjunto de definiciones de bloque cuyos sobres comparten `RackEmbedDocument.Id` (ADR-0009); cada
definición es una vista y cada referencia una colocación. ADR-0010 fija que, desde `RACKEDITAR`, **Actualizar** redibuja las
representaciones existentes e **Insertar** agrega una representación ligada con el mismo GUID. También dice que «Una vista adicional solo
se inserta desde un rack ya existente, para que disponga de diseño e identidad fuente.»

La Discovery de I-55 auditó `ba497f1` y lo volvió a medir sobre `dad4e77`:

- la primera vista está restringida por la interfaz (Selectivo solo frontal; Dinámico y cabecera solo lateral), no por la persistencia;
- cada gesto inserta una vista; la variante la decide el Plugin o la ventana, y en Push Back la lateral usa la posición de una lista;
- la definición se confirma antes del jig, que solo fija la posición;
- no existe forma de generar una clase de vista para varios racks existentes conservando su identidad; las celdas enlazadas de
  `RACKLAYOUT` son referencias de una definición con el mismo `Id`;
- una vista nueva copia las propiedades personalizadas de la vista elegida sin compararlas con sus hermanas;
- el redibujo previo a Insertar confirma vista por vista e ignora los fallos, pide la variante después de redibujar, recorre también las
  definiciones que dependen de una referencia externa y conserva las vistas huérfanas si son las únicas del rack;
- una pieza sin bloque se anota como faltante, pero varios caminos de inserción y de redibujo no lo informan;
- la planta y las elevaciones usan ejes locales distintos, así que las posiciones de un layout en planta no son posiciones de elevación.

El Owner fijó ID17, ID18 e ID19, que comparten una necesidad: **preparar la representación de una vista antes de materializarla**, sobre los
hechos neutrales de ADR-0044 y Shared View Foundation.

## Decisión

1. **Preparación y colocación son capas distintas.**
   - La preparación vive en Application, es pura, parte del **sistema ya resuelto** y usa el plan y el nombre base de la fundación.
   - La colocación vive en el Plugin: importa, verifica, crea con nombre único, escribe, coloca y limpia la definición sin referencias si
     no llega a colocarse, **también ante una excepción**.
   - Los builders siguen siendo la única autoridad geométrica.
2. **Política de producto sobre los hechos de la fundación.**
   - La creación de vistas nuevas escribe solo direcciones canónicas.
   - `RACKEDITAR` conserva su lectura legada por tipo de sistema.
   - La colocación de grupo acepta solo direcciones canónicas o canonizables, disponibles en el sistema resuelto, de racks resueltos sin
     diagnósticos que bloqueen la salida y con una forma persistida cuya paridad esté demostrada; ante cualquier otra cosa falla cerrado
     antes de pedir puntos, con un remedio que depende del tipo de sistema, de la disposición, de la disponibilidad y del estado de la
     autoridad del rack.
   - Una matriz de exposición por sistema dice qué vistas puede empezar, añadir, agrupar o proyectar cada flujo; la cama de rodamiento no
     admite hermanas.
3. **Identidad.**
   - Un rack nuevo recibe su `RackId` una vez, al aceptarse la intención de insertar una o varias vistas.
   - Una hermana hereda el del rack, y la colocación de grupo conserva el `RackId` de cada rack.
   - Insertar cura una fuente elegida con `Id` en blanco (solo espacios incluidos) y la redibuja con el `Id` curado en los sistemas que ya
     curan; Actualizar conserva su criterio vigente.
   - Los defectos que impiden el contrato (poste por posición de lista en Push Back; visibilidad de la planta Cantilever, corregida solo
     en el Plugin) se corrigen en prerrequisitos de producto aislados después de integrar y consumir la fundación neutral.
4. **Varias vistas de un rack en un flujo.** Redibujar las hermanas existentes y colocar las vistas nuevas son dos operaciones distintas.
   - En un rack existente, la variante de cada vista nueva se elige antes de comprobar las hermanas y de redibujarlas.
   - **Una sola función de pertenencia**, calculada sobre un único barrido, separa las hermanas propias del dibujo (que se redibujan o,
     si son huérfanas, se borran) de las que dependen de una referencia externa, que gobierna su dibujo de origen y no se tocan. Esa misma
     pertenencia es la de la comprobación de propiedades.
   - **Redibujo atómico de hermanas antes de Insertar.** Todo lo que el redibujo necesita se prepara sin escribir estado del rack (solo se
     importan definiciones de biblioteca) y comprobando que ninguna vista afectada esté en una capa bloqueada. Después, **una**
     transacción redibuja todas las hermanas y borra las huérfanas, y se confirma una sola vez; la purga, los renombres cosméticos y la
     regeneración van después del commit. Si algo falla al preparar, no se escribe nada del rack; si falla dentro de la transacción, no se
     aplica ninguna actualización a las vistas existentes y no se inserta ninguna vista.
   - Si no queda ninguna hermana propia con referencias de layout que redibujar, hay dos modos. Con el primitivo compartido de creación
     caller-owned ya integrado, redibujos sin referencias de layout, borrados, definición nueva, sobre y referencia se hacen en la
     transacción de la primera colocación; cancelar aborta todo. Sin ese primitivo integrado, se conserva el modo legado: definición y
     sobre antes del jig, limpieza best effort y riesgo residual de definición sin referencias. El producto no crea otra autoridad.
   - **Colocación de vistas nuevas**, solo tras el commit del redibujo y sin transacciones abiertas: cada colocación se confirma por
     separado, y Esc o Enter detienen la cola conservando el redibujo y las vistas ya colocadas.
   - Actualizar conserva su comportamiento vigente.
5. **Ninguna hermana nace distinta de las demás.**
   - Antes de crear una vista en un rack existente se comprueban, **acotadas a ese `RackId`**, las propiedades personalizadas de las
     hermanas con la igualdad canónica de I-54; la vista nueva hereda la colección común.
   - Tipos distintos, colecciones no escribibles, propiedades divergentes o un payload no interpretable **cuyo `Id` de nivel superior sea
     legible y coincida** fallan cerrado con remedio.
   - Un payload no interpretable con el `Id` de otro rack, o sin `Id` legible, no bloquea.
   - Fuera del editor, la autoridad authored de las hermanas debe ser única; se comprueba antes de resolver el rack y la resolución usa su
     representante.
   - No se reabre ADR-0039 ni cambia `RACKPROPIEDADES`.
6. **Colocación de grupo.**
   - **Grupos.** Una ejecución proyecta **una selección**; sus referencias se agrupan por `RackId`, y cada definición fuente del grupo
     recibe una definición nueva con una referencia por cada referencia seleccionada.
   - **Transformación.** **Una sola transformación rígida común por ejecución** entre un marco fuente y un marco destino, aplicada al
     ancla física de cada referencia, que se verifica sobre la colocación real (incluido el `Origin` de la definición fuente).
   - **Política rígida.** El layout de anclas se reproduce rotado y trasladado.
   - **Política ortográfica.** Conserva el eje físico compartido, con la dirección destino derivada de la familia, el tipo y la variante
     destino de cada referencia; alinea el eje común y colapsa el descartado; la superposición se detecta por intersección de tramos. Lo
     que conserva es la coordenada de cada tramo sobre la recta orientada elegida, no la coordenada descartada ni las orientaciones.
   - **Exposición y sentido.** Qué pares, modos y variantes se exponen, cómo se orientan los marcos y cómo se elige el sentido del eje común
     (ventana relativa al marco fuente o mayoría de referencias) **lo decide M-01**.
   - **Secuencia.** Todo se resuelve, se calcula sobre la selección entera, se valida y se planifica antes de pedir puntos. Toda pieza del
     plan que requiere un bloque de biblioteca debe tener una clave no vacía (comprobada antes de los puntos) presente tras importar, o **no
     se escribe ninguna definición ni referencia de rack**. Se materializa en una transacción, sin identidad nueva, re-estampado ni
     regeneración.

## Cambios sobre Insertar

| ID | Cambio | Conducta propuesta |
|---|---|---|
| IC-01 | Origen de hermana B (intencion de creacion) | Tambien desde una intencion de creacion aceptada con `RackId`, authored, sistema y vistas preparadas (flujo de creacion, fuera del Insertar de edicion) |
| IC-02 | Momento del prompt de variante | Antes del gate y del redibujo; Esc → nada modificado |
| IC-03 | Gate de propiedades acotado al `RackId` | Divergentes, ilegibles atribuibles o tipos distintos fallan con remedio |
| IC-04 | Autoridad authored | Unificacion vigente + redibujo atomico: todas las MUTABLE quedan con el authored nuevo o ninguna; sin gate authored nuevo en el editor |
| IC-05 | Redibujo atomico de hermanas | PREPARE → una MUTATE → un commit → POST |
| IC-06 | Huerfanas | En la MUTATE si hay supervivientes con referencias de layout; si no, junto con los redibujos sin referencias en la transaccion de la primera colocacion (Proposal V5 §4.6) |
| IC-07 | Hermanas dependientes de xref | Solo lectura: no se tocan; el informe las nombra |
| IC-08 | Colocacion tras el redibujo | Solo tras `REDRAW_APPLIED` o `REDRAW_NOT_REQUIRED`, con `TopTransaction == null` (si no, `PLACEMENT_BLOCKED`) |
| IC-09 | Cancelacion | Esc en el prompt: nada; Esc en un jig: redibujo y vistas previas conservados; primer jig sin supervivientes: nada en el modo 1 de Proposal V5 §4.6, limpieza best effort en el modo 2 |
| IC-10 | Fuente elegida con `Id` en blanco (Selectivo, Dinamico, Cabecera) | Siempre curada y redibujada, cualquiera que sea la vista; en Selectivo y Cabecera, las hermanas con el mismo `Id` de espacios tambien se curan |
| IC-11 | Predicado de blanco en Selectivo y Cabecera | `IsNullOrWhiteSpace` solo en Insertar; Dinamico, Push Back y Cantilever sin cambio |
| IC-12 | Capas bloqueadas | `PREPARE_FAILED` con la vista y la capa |
| IC-13 | Bloques faltantes | Todo camino informa con el requisito estructural |
| IC-14 | Excepcion tras crear la definicion | Limpieza best effort de la definicion sin referencias (D-10) |
| IC-15 | `Regen` | Uno en POST; Cantilever uno al final de la cola |
| IC-16 | Varias vistas en un gesto (ID18) | Lote con cola; mismo redibujo atomico |
| IC-17 | Definicion de superviviente | REDRAW con al menos una referencia de layout |
| IC-18 | Purga tras el redibujo | Consolidada en POST, excluyendo las anidadas que piden las vistas nuevas preparadas |
| IC-19 | Transaccion abierta inesperada | `PLACEMENT_BLOCKED` |

## Relación con ADR-0010

**ADR-0042 complementa a ADR-0010 y no lo reemplaza; ADR-0010 sigue `aceptado`.**

ADR-0010 acota su propio alcance: «Esta decisión gobierna el flujo de edición. No cambia la inserción inicial que crea un rack nuevo ni
promete capacidades multivista para todos los editores.» Este registro no revierte ninguna de sus decisiones y añade dos cosas:

1. **Generaliza el origen válido de una hermana.**
   - **A.** Un rack lógico materializado: es la regla de ADR-0010, que conserva su propósito de disponer de diseño e identidad fuente.
   - **B.** Una intención de creación aceptada con `RackId` único, autoridad authored, sistema resuelto y vistas preparadas: rige el
     flujo de creación que ADR-0010 deja fuera de su alcance.
2. **Añade precondiciones y garantías al flujo de Insertar**, que ADR-0010 delega en «su flujo implementado»: los cambios IC-02 a IC-19 de la
   sección anterior.

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
- **Acuñar el `RackId` al abrir el editor o en la primera colocación**: GUIDs sin uso, o identidad inventada en el Plugin.
- **Una transacción para todo el lote, incluidos los jigs, o preparar cada vista justo antes**: dejaría sin confirmar el redibujo y las
  vistas colocadas a lo largo de varias interacciones del usuario y convertiría Esc en rollback de vistas ya colocadas, o fallaría tras
  colocar.
- **Confirmar el redibujo de cada hermana por separado y detenerse ante un fallo** (versión anterior de este registro): dejaba un
  subconjunto de hermanas confirmado con el diseño nuevo.
- **Renombrar dentro de la transacción del redibujo**: el nombre es cosmético por contrato vigente; después del commit no corre si hay
  rollback.
- **Borrar huérfanas antes de confirmar la primera colocación cuando no hay supervivientes:** rechazado porque puede destruir la única identidad del rack.
- **Confirmar la definición antes del primer jig sin supervivientes:** modo legado conservado solo mientras falta el primitivo compartido integrado; puede dejar
  una definición sin referencias si falla la limpieza best effort. La alternativa atómica elimina ese riesgo cuando su requisito existe.
- **Traslación literal como única semántica de grupo**: apila elevaciones de un layout en planta; queda representable como caso rígido.
- **Proyección ortográfica como única semántica de grupo** (primera versión de este registro): no representa misma clase, frontal ↔
  lateral, orientaciones distintas ni un marco destino orientable.
- **Dirección ortográfica tomada de una familia fija**: rechaza pares Cantilever soportados.
- **Una transformación por grupo `RackId`**: no conserva el layout entre racks.
- **Transformación afín general**: escala y cizalla carecen de sentido para vistas 1:1.
- **Usar la autoridad global de propiedades de I-54**: un payload ilegible no relacionado bloquearía cualquier rack.
- **Copiar la colección de la vista elegida sin comparar** (primera versión): permite que una hermana nazca distinta.
- **Resolver el rack antes de comprobar su autoridad authored**: elegiría un representante arbitrario.
- **Reemplazar ADR-0010**, por completo (primera versión) o de forma acotada: reescribe o marca como reemplazadas decisiones que no
  cambian.

## Consecuencias

**Positivas.**
- Un rack empieza por cualquier vista soportada.
- Varias vistas se colocan en un flujo con un `RackId`.
- Varios racks reciben una clase de vista con una transformación común y conservando su identidad.
- Ninguna hermana nueva nace distinta de las demás; el modo con creación caller-owned integrada revierte la primera colocación cancelada.
- El modo legado de huérfanas conserva riesgo residual de definición sin referencias y exige informar cualquier fallo de limpieza.
- Las reglas son puras y probables sin AutoCAD.
- Conteos intactos y sin migración de dibujos.

**Negativas / costos aceptados.**
- Se re-enrutan todas las inserciones, con goldens y validación del Owner.
- Cambian a propósito censos y guardas.
- Insertar en un rack con propiedades divergentes o ilegibles falla con remedio; un redibujo fallido no aplica ninguna actualización ni
  inserta ninguna vista; una vista en una capa bloqueada impide Insertar hasta desbloquearla.
- En Insertar, el prompt de variante pasa a antes del redibujo, y las hermanas dependientes de una referencia externa dejan de
  redibujarse.
- Una transacción de escritura para todas las hermanas de un rack; las definiciones de biblioteca importadas al preparar no se revierten.
- Actualizar sigue redibujando vista por vista y puede quedar redibujado en parte, como hoy.
- Según la regla que decida M-01 para el sentido del eje común: con la ventana relativa, una recta girada a mano justo en el límite
  invierte el orden con un giro mínimo; con la mayoría, girar racks puede invertir el orden de toda la elevación.
- El remedio puede exigir reparar antes lo que deja `RACKPROPIEDADES` en solo lectura.
- La colocación de grupo depende de marcos y tramos que deben seguir a los builders.
- Un payload ilegible sin `Id` atribuible que sí pertenezca al rack no se detecta.
- Este producto solo puede implementarse sobre la fundación de vistas ya integrada.

## Relación con otros ADR

- [ADR-0009](0009-identidad-guid-embebida-en-dwg.md): se conserva y se precisa.
- [ADR-0010](0010-actualizar-redibuja-insertar-liga-vistas.md): complementado (sección anterior); sigue `aceptado`.
- [ADR-0011](0011-parametros-dinamicos-con-patron-array.md): sin cambio.
- [ADR-0029](0029-contrato-funcional-comun-de-ventanas-wpf.md): el diálogo de varias vistas es del arquetipo C.
- [ADR-0034](0034-project-variables-autoridad-drawing-level.md): sin cambio; la resolución efectiva del selectivo sigue una vez por rack
  dentro de su manejador y la colocación de grupo llega a ella por el contrato de la fundación.
- [ADR-0035](0035-visibilidad-de-cotas-por-tipo-de-vista.md): sin cambio de política.
- [ADR-0037](0037-reutilizacion-de-cabecera-por-copia-y-distribucion-por-lotes.md): precedente de preparación completa antes de escribir.
- [ADR-0039](0039-custom-properties-persistencia-autoridad.md): sin cambio; I-55 solo consume su lectura y su igualdad canónica.
- [ADR-0044](0044-hechos-neutrales-de-vistas-compartidas.md): autoridad aceptada de los hechos que este registro consume, integrada
  mediante el recibo `integration/I-57`.
- ADR-0036 (propuesto, espejo semántico): consumidor de la misma fundación.

## Referencias

- [Proposal V5 de I-55](../initiatives/I-55-proposal-v5.md): §1..§6, §8, §10 (D-01..D-27, OD-1..OD-8, M-01 con OD-7.e).
- [Mapa de implementación V5](../initiatives/I-55-implementation-map-v5.md).
- [Especificación de la fundación compartida de vistas](../architecture/shared-view-foundation/specification.md).
- Proposals [V1](../initiatives/I-55-proposal-v1.md), [V2](../initiatives/I-55-proposal-v2.md), [V3](../initiatives/I-55-proposal-v3.md)
  y [V4](../initiatives/I-55-proposal-v4.md) de I-55: registro histórico.
- [`ProjectVariableMutationExecutor`](../../src/RackCad.Plugin/ProjectVariableMutationExecutor.cs): precedente de PREPARE, una MUTATE y POST.
- [`Transform2D`](../../src/RackCad.Application/Geometry/Transform2D.cs),
  [`CustomPropertiesCanonicalForm`](../../src/RackCad.Application/CustomProperties/CustomPropertiesCanonicalForm.cs),
  [`RackEmbedDocument`](../../src/RackCad.Application/Persistence/RackEmbedDocument.cs),
  [`BlockPlacement`](../../src/RackCad.Plugin/Drawing/BlockPlacement.cs),
  [`LateralHeaderDrawer`](../../src/RackCad.Plugin/Drawing/LateralHeaderDrawer.cs),
  [`RackLayoutCommands`](../../src/RackCad.Plugin/RackLayoutCommands.cs).
