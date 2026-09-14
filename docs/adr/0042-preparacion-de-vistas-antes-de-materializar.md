# ADR-0042: Preparar la vista antes de materializarla: primera vista libre, varias vistas de un rack en un flujo y colocación de grupo que conserva la identidad

- **Estado:** propuesto
- **Fecha:** 2026-09-14 (propuesto; revisado con la Proposal V2 de I-55)
- **Decisores:** pendiente. Solo el Owner del repositorio acepta o rechaza. Coordinador de I-55: **CHANGES REQUIRED** sobre la
  Proposal V1 y **REVIEW REQUIRED** sobre la V2; Arquitecto de I-55: **REVIEW REQUIRED**; sin consenso técnico. Claude (redacción)
- **Iniciativa relacionada:** I-55 — `feature/creacion-de-vistas`
  ([contrato](../initiatives/I-55-creacion-de-vistas.md), [Discovery](../initiatives/I-55-discovery.md),
  [Proposal V2](../initiatives/I-55-proposal-v2.md), [mapa de implementación V2](../initiatives/I-55-implementation-map-v2.md),
  [Proposal V1](../initiatives/I-55-proposal-v1.md) como registro, [registro de I-55](../automation/decisions/I-55.md))
- **No reemplaza por completo a ninguna ADR.** **Complementa y enmienda reglas acotadas de**
  [ADR-0010](0010-actualizar-redibuja-insertar-liga-vistas.md): cuándo puede existir una vista adicional y dos precondiciones nuevas de
  Insertar. El resto de ADR-0010 sigue vigente; la forma de registrar la relación queda para revisión del Arquitecto (ver «Relación con
  otros ADR»)

> **Estado de este registro.** `propuesto`. **No** hay consenso técnico (Coordinator = REVIEW REQUIRED, Architect = REVIEW REQUIRED,
> Consensus = NOT REACHED), **no** autoriza implementación y puede editarse hasta que el Owner lo acepte o lo rechace.
>
> **Precondiciones de aceptación.** (1) Consenso técnico sobre la misma versión del plan. (2) **M-01 resuelta**: la semántica de
> colocación de grupo expuesta, el marco fuente, el marco destino, el modo por par de vistas, los tipos y familias mezclados y la
> variante en misma clase (OD-6.b/c/d, OD-7.a/b/c/d y OD-2.b de la Proposal V2) son material abierto. (3) Decisiones del Owner
> OD-1..OD-8. (4) Reconciliación registrada con I-52 de las autoridades compartidas del punto 8 (Proposal V11 de I-52, [V11-D10]).
> El texto final incorporará lo que se decida.
>
> **Numeración.** Se redactó como 0041, número que I-49 publicó en su rama (`8cefd59`) antes del primer commit de este registro; por eso
> es 0042. Si otra rama integrara antes un 0042, este registro tomaría el siguiente número libre antes de integrarse.

## Contexto

RackCad modela un rack lógico como el conjunto de definiciones de bloque cuyos sobres comparten `RackEmbedDocument.Id` (ADR-0009); cada
definición es una vista y cada referencia una colocación. ADR-0010 fija que, desde `RACKEDITAR`, **Actualizar** redibuja las
representaciones existentes e **Insertar** agrega una representación ligada con el mismo GUID, y que «Una vista adicional solo se
inserta desde un rack ya existente, para que disponga de diseño e identidad fuente.»

La Discovery de I-55 auditó `ba497f1` y lo volvió a medir sobre `dad4e77`:

- la primera vista está restringida por la interfaz (Selectivo solo frontal; Dinámico y cabecera solo lateral), no por la persistencia;
- cada gesto inserta una vista; la variante la decide el Plugin o la ventana, y en Push Back la lateral usa la posición de una lista;
- el payload se compone por comando en el Plugin y la definición se confirma antes del jig, que solo fija la posición;
- no existe forma de generar una clase de vista para varios racks existentes conservando su identidad; las celdas enlazadas de
  `RACKLAYOUT` son referencias de una definición con el mismo `Id`;
- una vista nueva copia las propiedades personalizadas de la vista elegida sin compararlas con sus hermanas, y el redibujo previo a
  Insertar ignora los fallos;
- la planta y las elevaciones usan ejes locales distintos, así que las posiciones de un layout en planta no son posiciones de elevación.

El Owner fijó ID17, ID18 e ID19, que comparten una necesidad: **preparar la representación de una vista antes de materializarla**. La
Proposal V11 de I-52 (`RACKMIRROR`, sin consenso) propone autoridades compartidas de taxonomía de tipo de vista, lectura de
`View`/`Section`, plan por vista, comparación authored, materialización y transformaciones de colocación, y exige reconciliar su
propiedad con I-55 antes de congelar cualquiera de los dos contratos.

## Decisión

1. **Preparación y colocación son capas distintas.** La preparación vive en Application, es pura y parte del sistema ya resuelto; la
   colocación vive en el Plugin, importa, crea, escribe, coloca y limpia la definición si no llega a colocarse, **también ante una
   excepción**. Los builders siguen siendo la única autoridad geométrica.
2. **Una sola taxonomía de tipo de vista y una variante tipada.** `RackViewKind` (renombre de `DimensionViewKind`, sin cambio de la
   política de ADR-0035); la representación dentro del tipo es una variante semántica, nunca un índice de interfaz. **Un solo codec**
   devuelve la dirección y su disposición (`Canonical`, `Canonicalizable`, `Coerced`, `Invalid`); cada consumidor aplica su política:
   `RACKEDITAR` conserva su lectura, y **la colocación de grupo acepta solo `Canonical` y `Canonicalizable`** (interpretada sin reescribir
   la fuente) y falla cerrado ante `Coerced`, `Invalid` y variantes que ya no existen en el sistema resuelto. **La persistencia no
   cambia.**
3. **Vista soportada** = builder + codec y edición que la reconocen + existencia en el sistema resuelto; una matriz normativa por sistema.
   La cama de rodamiento no admite hermanas.
4. **Identidad.** Un rack nuevo recibe su `RackId` una vez, al aceptarse la intención de insertar una o varias vistas; una hermana hereda
   el del rack; la edición conserva la curación vigente de un `Id` en blanco; la colocación de grupo conserva el `RackId` de cada rack.
   Los defectos que impiden el contrato (poste por posición de lista en Push Back; visibilidad de la planta Cantilever, corregida solo en
   el Plugin) se corrigen en prerrequisitos aislados **antes** de la foundation.
5. **Varias vistas de un rack en un flujo.** Todo se prepara antes de la primera escritura; cada colocación se confirma por separado y
   ninguna transacción de escritura queda abierta entre jigs; en un rack existente, un redibujo fallido de una hermana impide insertar.
6. **Ninguna hermana nace distinta de las demás.** Antes de crear una vista en un rack existente se comprueban, **acotadas a ese
   `RackId`**, las propiedades personalizadas del sobre elegido o fuente y de las hermanas conocidas (sobres interpretables, no
   dependientes de xref, con ese `Id`), con la igualdad canónica de I-54; la vista nueva hereda la colección común. Tipos distintos,
   colecciones no escribibles, propiedades divergentes o un payload no interpretable **cuyo `Id` de nivel superior sea legible y coincida**
   fallan cerrado con remedio. Un payload no interpretable con el `Id` de otro rack, o sin `Id` legible, no bloquea; un barrido que la
   lectura hace fallar sigue fallando. Fuera del editor, la autoridad authored de todas las hermanas debe ser única. No se reabre ADR-0039 ni cambia
   `RACKPROPIEDADES`.
7. **Colocación de grupo.** Una ejecución proyecta **una selección**: sus referencias se agrupan por `RackId` y cada grupo recibe una
   definición nueva, con una referencia por referencia seleccionada (varias definiciones fuente de un mismo `RackId`: según M-01). La **foundation** es **una sola
   transformación rígida común por ejecución** entre un **marco fuente** y un **marco destino** (traslación y rotación comunes, escala 1,
   sin cizalla, nunca una transformación por rack ni por grupo), aplicada al ancla física de cada referencia, que cada vista declara
   mediante descriptores de marco caracterizados contra los builders; la diferencia entre las anclas destino de cualquier par de
   referencias es la rotación común de la diferencia de sus anclas fuente. Sobre ella hay **políticas**: **rígida** (el layout de anclas
   se reproduce rotado y trasladado y cada vista gira con la rotación común) y **ortográfica** (se conserva el eje físico compartido entre
   la vista fuente y la pedida, conservando su sentido respecto de cada vista, se alinea el común y se colapsa el descartado con aviso).
   Qué pares, modos y variantes se exponen, y cómo se orientan los marcos, **lo decide M-01**. Todo se resuelve, valida y planifica
   antes de pedir puntos, se importa y verifica antes de escribir, y se materializa en una transacción, sin identidad nueva,
   re-estampado ni regeneración.
8. **Una autoridad por responsabilidad entre iniciativas.** Selección, paso «sistema resuelto → plan», lectura de `View`/`Section`,
   comparación authored, primitivo de materialización, valor de colocación sobre `Transform2D` y origen y tramo del eje de una vista
   tienen una autoridad cada uno, compartida con I-52 y extraída una sola vez; las políticas quedan en cada llamador.

### Reglas de ADR-0010 que se enmiendan

ADR-0010 dice, literal: «Una vista adicional solo se inserta desde un rack ya existente, para que disponga de diseño e identidad
fuente.» Con este registro, **una hermana adicional requiere**:

- **A.** un rack lógico **ya materializado**; **o**
- **B.** una **única intención de creación aceptada** que ya tenga `RackId` único, autoridad authored, sistema resuelto y el conjunto de
  vistas preparado.

Así, las vistas 2..N del lote de un rack nuevo no necesitan fingir que provienen de un rack físicamente colocado.

ADR-0010 dice también: «Insertar una vista crea una representación adicional ligada al rack existente y conserva su identidad. Cada
editor puede sincronizar previamente las representaciones existentes según su flujo implementado.» Con este registro, **Insertar sobre
un rack existente gana dos precondiciones**: (1) la comprobación de propiedades personalizadas acotada al `RackId` del punto 6; y (2) si
el redibujo previo de una hermana falla, no se inserta ninguna vista nueva.

**Todo lo demás de ADR-0010 sigue vigente**: Actualizar reconstruye y redefine en sitio sin crear otro rack ni insertar vistas; Insertar
crea representaciones ligadas con el mismo GUID mediante el flujo de colocación; `View` y `Section` distinguen la representación sin
convertirla en otro rack; la definición contiene la geometría y el sobre, y la referencia solo la coloca; la cama solo redibuja su vista
lateral. ADR-0010 declara que no cambia la inserción inicial de un rack nuevo; este registro la gobierna (puntos 3 a 5) sin contradecirlo.

## Alternativas consideradas

- **Seguir por sistema con `View` como texto y `Section` como entero**: repite el defecto de índice de interfaz y deja lógica no probable.
- **Framework genérico de proveedores**: sin segundo cliente real.
- **Registro lógico persistente de racks**: formato nuevo en el dibujo y migración de todos los DWG.
- **Segundo enum de vista con mapeo**: dos taxonomías para un concepto.
- **Acuñar el `RackId` al abrir el editor o en la primera colocación**: GUIDs sin uso, o identidad inventada en el Plugin.
- **Una transacción para todo el lote, o preparar cada vista justo antes**: transacción abierta durante jigs, o fallos tras colocar.
- **Traslación literal como única semántica de grupo**: apila elevaciones de un layout en planta; queda representable como caso rígido.
- **Proyección ortográfica como única semántica de grupo** (versión anterior de este registro): no representa misma clase, frontal ↔
  lateral, orientaciones distintas ni un marco destino orientable.
- **Una transformación por grupo `RackId`**: no conserva el layout entre racks.
- **Transformación afín general**: escala y cizalla carecen de sentido para vistas 1:1.
- **Usar la autoridad global de propiedades de I-54**: un payload ilegible no relacionado bloquearía cualquier rack.
- **Copiar la colección de la vista elegida sin comparar** (versión anterior): permite que una hermana nazca distinta.
- **Reemplazar ADR-0010 por completo** (versión anterior): reescribe decisiones que no cambian.

## Consecuencias

- Positivas: un rack empieza por cualquier vista soportada; varias vistas se colocan en un flujo con un `RackId`; varios racks reciben
  una clase de vista con una transformación común y conservando su identidad; ninguna hermana nueva nace distinta; las reglas son puras y
  probables sin AutoCAD; conteos intactos; sin migración de dibujos.
- Negativas / costos aceptados: se re-enrutan todas las inserciones (goldens y validación del Owner); cambian a propósito censos y
  guardas; Insertar en un rack con propiedades divergentes o ilegibles, o con un redibujo fallido, falla con remedio, y ese remedio puede
  exigir reparar antes lo que deja `RACKPROPIEDADES` en solo lectura; la colocación de grupo depende de descriptores que deben seguir a los
  builders; un payload ilegible sin `Id` atribuible que sí pertenezca al rack no se detecta; la extracción de autoridades compartidas exige
  secuenciar con I-52.

## Relación con otros ADR

- [ADR-0009](0009-identidad-guid-embebida-en-dwg.md): se conserva y se precisa.
- [ADR-0010](0010-actualizar-redibuja-insertar-liga-vistas.md): **complementado y enmendado solo en las reglas citadas**. El índice de ADR
  dice que un aceptado es inmutable en su contenido, que admite una sección final «Notas posteriores» con fecha y que, para cambiar la
  decisión, se escribe un ADR que lo reemplace ([README](README.md), «Cuándo modificar / reemplazar»). Formas de registrar la relación,
  para revisión del Arquitecto:
  - **(a) Complemento con nota posterior fechada en ADR-0010** que enlace las reglas enmendadas; ADR-0010 sigue `aceptado`. La relación
    de complemento y enmienda es la que pide CR-08 del [registro de I-55](../automation/decisions/I-55.md); la nota fechada es la forma
    propuesta de registrarla. Hay precedentes de complemento sin reemplazo
    (ADR-0028 y ADR-0029 en su cabecera) y de nota posterior fechada que enlaza un ADR posterior en un aceptado que no queda reemplazado
    ([ADR-0005](0005-estrategia-de-unidades.md), nota del 2026-07-27 sobre ADR-0021); a diferencia de esos casos, aquí sí cambia una
    regla.
  - **(b) Reemplazo acotado**: ADR-0010 pasaría a `reemplazado por ADR-0042` con una nota fechada que diga qué sigue vigente, como
    [ADR-0008](0008-secciones-unificadas-por-rol.md) frente a ADR-0020 (reemplazo de autoridad conceptual, no de comportamiento). Solo si el
    sistema de ADR exige reemplazo para cambiar una regla; no es la forma preferida.
- [ADR-0011](0011-parametros-dinamicos-con-patron-array.md): sin cambio.
- [ADR-0029](0029-contrato-funcional-comun-de-ventanas-wpf.md): el diálogo de varias vistas es del arquetipo C.
- [ADR-0034](0034-project-variables-autoridad-drawing-level.md): registro de variables leído una vez por comando.
- [ADR-0035](0035-visibilidad-de-cotas-por-tipo-de-vista.md): sin cambio de política; el tipo de vista cambia de nombre.
- [ADR-0037](0037-reutilizacion-de-cabecera-por-copia-y-distribucion-por-lotes.md): precedente de preparación completa antes de escribir.
- [ADR-0039](0039-custom-properties-persistencia-autoridad.md): sin cambio; I-55 solo consume su lectura y su igualdad canónica.
- ADR-0036 (propuesto en la rama de I-52): autoridades compartidas del punto 8, cuya propiedad se reconcilia entre I-52 e I-55, sin
  duplicarlas, antes de congelar cualquiera de los dos contratos.

## Referencias

- [Proposal V2 de I-55](../initiatives/I-55-proposal-v2.md): §0, §7.3, §9, §12, §17.2 y §22 (D-01..D-18, OD-1..OD-8, M-01, X-1..X-8).
- [Proposal V1 de I-55](../initiatives/I-55-proposal-v1.md): registro histórico.
- [Mapa de implementación V2](../initiatives/I-55-implementation-map-v2.md).
- [`Transform2D`](../../src/RackCad.Application/Geometry/Transform2D.cs),
  [`CustomPropertiesCanonicalForm`](../../src/RackCad.Application/CustomProperties/CustomPropertiesCanonicalForm.cs),
  [`RackEmbedDocument`](../../src/RackCad.Application/Persistence/RackEmbedDocument.cs),
  [`RackEmbedComposer`](../../src/RackCad.Application/Persistence/RackEmbedComposer.cs),
  [`BlockPlacement`](../../src/RackCad.Plugin/Drawing/BlockPlacement.cs),
  [`RackLayoutCommands`](../../src/RackCad.Plugin/RackLayoutCommands.cs),
  [`SelectivePlantaBuilder`](../../src/RackCad.Application/Systems/Selective/SelectivePlantaBuilder.cs),
  [`CantileverViewPlanBuilder`](../../src/RackCad.Application/Systems/Cantilever/CantileverViewPlanBuilder.cs).
