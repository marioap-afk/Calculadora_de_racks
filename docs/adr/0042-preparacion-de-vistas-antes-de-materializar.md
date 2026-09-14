# ADR-0042: Preparar la vista antes de materializarla: primera vista libre, varias vistas de un rack en un flujo y proyección multi-rack que conserva la identidad

- **Estado:** propuesto
- **Fecha:** 2026-09-14 (propuesto)
- **Decisores:** pendiente. Solo el Owner del repositorio acepta o rechaza. Coordinador de I-55 y Arquitecto de I-55:
  **REVIEW REQUIRED** sobre la Proposal V1; sin consenso técnico. Claude (redacción)
- **Iniciativa relacionada:** I-55 — `feature/creacion-de-vistas`
  ([contrato](../initiatives/I-55-creacion-de-vistas.md), [Discovery](../initiatives/I-55-discovery.md),
  [Proposal V1](../initiatives/I-55-proposal-v1.md), [mapa de implementación V1](../initiatives/I-55-implementation-map-v1.md),
  [registro de I-55](../automation/decisions/I-55.md))
- **Reemplazaría a:** [ADR-0010](0010-actualizar-redibuja-insertar-liga-vistas.md), si el Owner lo acepta (ver «Relación con
  otros ADR»)

> **Estado de este registro.** Nace `propuesto` junto con la Proposal V1 de I-55. **No** hay consenso técnico
> (Coordinator = REVIEW REQUIRED, Architect = REVIEW REQUIRED, Consensus = NOT REACHED) y **no** autoriza implementación.
> Puede editarse libremente hasta que el Owner lo acepte o lo rechace.
>
> **Precondición de aceptación.** Este registro fija decisiones de arquitectura. Las elecciones de producto que las
> parametrizan —nombre del comando (OD-1), variante de la proyección (OD-2), miembro no soportado (OD-3), Esc en un lote (OD-4),
> orden del lote (OD-5), orientación de las vistas proyectadas (OD-6), significado del layout relativo (OD-7) y presentación de
> las vistas proyectadas (OD-8)— son **decisiones del Owner** registradas en la Proposal V1 §22.2. Deben estar decididas antes
> de aceptar este ADR, y su texto final las incorporará tal como se decidan.
>
> **Numeración.** Se redactó como 0041, número que I-49 publicó en su rama (`8cefd59`) antes del primer commit de este
> registro; por eso es 0042, que no figuraba en ninguna rama, etiqueta ni índice en el re-fetch previo al commit. Si otra rama
> integrara antes un 0042, este registro tomaría el siguiente número libre antes de integrarse.

## Contexto

RackCad modela un rack lógico como el conjunto de definiciones de bloque cuyos sobres comparten `RackEmbedDocument.Id`
(ADR-0009); cada definición contiene una vista (`View`) y, cuando aplica, una sección (`Section`), y cada referencia es una
colocación. ADR-0010 fija que, desde `RACKEDITAR`, **Actualizar** redibuja las representaciones existentes e **Insertar**
agrega una representación ligada con el mismo GUID, y que «una vista adicional solo se inserta desde un rack ya existente».

La Discovery de I-55 auditó el código de `ba497f1` y lo volvió a medir sobre `dad4e77` tras integrarse I-53D:

- la primera vista está restringida por la interfaz y no por la persistencia: el Selectivo solo empieza por la frontal, el
  Dinámico y la cabecera por la lateral, mientras Push Back y Cantilever ya empiezan por cualquiera;
- cada gesto inserta **una** vista; la variante la decide el Plugin por prompt o la ventana, y en Push Back la lateral usa la
  posición de la lista como número de poste;
- el payload de cada vista se compone por comando en el Plugin, y la definición se confirma antes del jig;
- no existe forma de generar una clase de vista para varios racks existentes: las copias crean identidad nueva, y las celdas
  enlazadas de `RACKLAYOUT` son referencias de una definición con el mismo `Id`;
- la planta y las elevaciones de un rack usan ejes locales distintos (en los sistemas de rack la planta dibuja la profundidad
  en X y la corrida en Y; el Cantilever proyecta con los ejes rotados y reflejados respecto de ellos), así que las posiciones de
  un layout en planta no son posiciones de elevación.

El Owner fijó tres capacidades (ID17, ID18, ID19) que comparten una necesidad: **preparar la representación de una vista antes
de materializarla**. En paralelo, la Proposal V10 de I-52 (`RACKMIRROR`, sin consenso) propone autoridades de plan por vista,
lectura de `View`/`Section`, comparación authored por kind y materialización, y registra el riesgo de duplicarlas con I-55.

## Decisión

1. **Preparación y colocación son capas distintas.** La preparación vive en Application, es pura y produce, a partir de la
   identidad, el authored o diseño, el **sistema ya resuelto**, el tipo de vista y la variante, todo lo que la colocación
   necesita. La colocación vive en el Plugin: importa bloques, crea la definición, escribe el sobre, coloca y limpia la
   definición si no llega a colocarse, **también ante una excepción**. Los builders siguen siendo la única autoridad geométrica.
2. **Una sola taxonomía de tipo de vista.** `Frontal`, `Lateral` y `Planta` son `RackViewKind`, renombre de `DimensionViewKind`
   sin cambio de miembros ni de la política de ADR-0035. La representación dentro del tipo es una **variante tipada y
   semántica**, nunca un índice de interfaz. **Un solo codec** traduce entre la dirección y `(View, Section)`, reproduce las
   lecturas vigentes y devuelve su disposición para que cada consumidor aplique su política. **La persistencia no cambia.**
3. **Vista soportada.** Un sistema soporta una dirección si tiene builder, el codec la codifica, la edición la reconoce y la
   redibuja entre sus hermanas, y existe en el sistema resuelto. Una matriz normativa por sistema fija con qué vistas empieza un
   rack, cuáles recibe después, cuáles admiten lote y cuáles proyección. La cama de rodamiento no admite hermanas.
4. **Identidad.** Un rack nuevo recibe su `RackId` **una vez**, al aceptarse la intención de insertar una o varias vistas, y
   todas las vistas de esa aceptación lo comparten; una vista hermana hereda el del rack, y la edición conserva la curación
   vigente de un `Id` en blanco; la proyección conserva el `RackId` de cada rack. Los defectos que impiden este contrato
   —poste por posición de lista en Push Back, visibilidad de la planta Cantilever e `Id` interior del Cantilever— se corrigen en
   prerrequisitos aislados **antes** de la foundation.
5. **Varias vistas de un rack en un flujo.** Todas las vistas pedidas se preparan antes de la primera escritura; cada colocación
   se confirma por separado y ninguna transacción de escritura queda abierta entre dos jigs. En un rack existente se prepara,
   después se redibuja y, si algún redibujo falla, no se inserta ninguna vista nueva. La política ante Esc y el orden del lote
   son los que decida el Owner.
6. **Autoridad entre hermanas.** Ninguna operación de I-55 crea authored divergente: en la edición, el flujo vigente unifica el
   authored de todas las hermanas y un redibujo fallido impide insertar; fuera del editor, solo se proyecta un rack cuya
   autoridad authored sobre **todas** sus hermanas es única. Las propiedades personalizadas de una vista nueva son las del sobre
   fuente, así que no aparece un valor nuevo; I-55 no introduce un bloqueo por propiedades personalizadas.
7. **Proyección multi-rack.** Las referencias seleccionadas se agrupan por `RackId`; dentro de un grupo deben ser colocaciones de
   una misma definición, y cada grupo recibe **una** definición nueva de la vista pedida y **una** referencia nueva por
   referencia seleccionada. El layout relativo se preserva con **una transformación común aplicada en el marco físico de los
   racks** (corrida, profundidad, altura): cada vista declara qué eje físico representa cada eje local y dónde cae su origen
   físico —descriptores caracterizados contra la salida de los builders—, y la transformación conserva el eje compartido entre la
   vista fuente y la pedida, alinea las vistas sobre el eje común y colapsa el descartado con aviso. Este significado del layout
   relativo, la orientación de las vistas proyectadas y su presentación dependen de OD-6, OD-7 y OD-8. Todo se valida antes de
   pedir puntos, se prepara y verifica —incluidos los bloques de biblioteca— antes de escribir, y se materializa en **una**
   transacción, sin identidad nueva, sin re-estampado y sin regeneración.
8. **Una autoridad por responsabilidad entre iniciativas.** Selección y agrupación, paso «sistema resuelto → plan por vista»,
   lectura de `View`/`Section`, comparación authored por kind y el primitivo de materialización en la transacción del llamador
   tienen **una** autoridad cada una, compartida con I-52 y extraída una sola vez; las políticas de cada comando quedan en el
   llamador.

### Semántica de edición que se conserva de ADR-0010

Desde `RACKEDITAR`, **Actualizar** reconstruye el diseño y redefine en sitio las representaciones existentes, conserva el GUID,
no crea otro rack lógico ni inserta vistas; las referencias que comparten una definición reflejan la redefinición. **Insertar**
crea representaciones adicionales ligadas al rack existente con su mismo GUID, después de sincronizar las existentes según el
flujo de cada editor. `View` y `Section` distinguen la representación sin convertirla en otro rack; la definición contiene la
geometría y el sobre; la referencia solo la coloca. La cama de rodamiento solo redibuja su vista.

**Lo que cambia respecto de ADR-0010:** una vista adicional puede insertarse **también en el flujo que crea el rack**, siempre
que comparta el diseño y la identidad acuñada una sola vez en ese flujo; Insertar puede agregar **varias** vistas en un gesto; la
inserción inicial deja de estar restringida a una vista de entrada por sistema; y un redibujo fallido impide insertar.

## Alternativas consideradas

- **Seguir por sistema con `View` como texto y `Section` como entero.** Repite el defecto de índice de interfaz, duplica la
  preparación de lotes y proyecciones por comando y deja lógica nueva en el Plugin, que no se puede probar sin AutoCAD.
- **Un framework genérico de proveedores y materializadores.** Extensibilidad sin segundo cliente real.
- **Un registro lógico persistente de racks fuera de los bloques.** Formato nuevo en el dibujo, sincronización ante COPY,
  WBLOCK, xref y UNDO, y migración de todos los DWG; contradice la identidad embebida por definición.
- **Un segundo enum de vista con mapeo a `DimensionViewKind`.** Dos taxonomías para el mismo concepto.
- **Acuñar el `RackId` al abrir el editor o en la primera colocación.** GUIDs sin uso, o identidad inventada en el Plugin.
- **Una transacción para todo el lote, o preparar cada vista justo antes de colocarla.** Transacción abierta durante varios
  jigs, o fallos de preparación después de colocar.
- **Proyectar con una traslación literal de las posiciones de las referencias.** Trivial, pero apila y superpone las
  elevaciones de un layout en planta porque los ejes de planta y elevación no coinciden; queda como opción del Owner (OD-7).
- **Separar filas distintas en la proyección con un espaciado.** Dejaría de ser una transformación común.
- **Bloquear las vistas nuevas cuando las propiedades personalizadas de las hermanas no son únicas.** No evita ningún valor
  nuevo y contradice que un rack existente pueda recibir cualquier hermana soportada (un payload ilegible en cualquier parte
  del dibujo bloquearía todos los racks).
- **Elegir una hermana ganadora o reconciliar automáticamente** cuando el authored diverge: deciden por el usuario o escriben
  hermanas no tocadas.
- **Un ADR complementario que conserve ADR-0010.** Más corto, pero dejaría vigente la frase literal de ADR-0010 que el lote de
  un rack nuevo contradice.

## Consecuencias

- Positivas: un rack empieza por cualquier vista que su sistema soporte; varias vistas se colocan en un flujo con un solo
  `RackId`; varios racks reciben una clase de vista alineada con su layout físico y conservando su identidad; la variante deja
  de depender de posiciones de interfaz; las reglas nuevas son puras y se prueban sin AutoCAD; `RACKLISTA` y `RACKBOMTOTAL`
  siguen contando racks y copias; no hay migración de dibujos.
- Negativas / costos aceptados: se re-enrutan todas las inserciones, con riesgo de regresión que exige goldens y validación del
  Owner; cambian a propósito censos y guardas existentes; la proyección depende de descriptores de marco que deben seguir a los
  builders (sus caracterizaciones fallan si un builder mueve un origen); en V1 no se proyectan juntos sistemas de familias de
  marco distintas ni racks con orientaciones distintas; la extracción de autoridades compartidas exige secuenciar con I-52; los
  bloques de biblioteca importados durante una preparación que después falla siguen quedando en el dibujo, como hoy.

## Relación con otros ADR

- [ADR-0009](0009-identidad-guid-embebida-en-dwg.md): se conserva; este registro precisa cuándo nace el GUID de un rack creado
  con varias vistas y que la proyección no crea identidad.
- [ADR-0010](0010-actualizar-redibuja-insertar-liga-vistas.md): al aceptarse este registro pasaría a `reemplazado por ADR-0042`;
  su semántica de Actualizar e Insertar se conserva arriba.
- [ADR-0011](0011-parametros-dinamicos-con-patron-array.md): sin cambio.
- [ADR-0029](0029-contrato-funcional-comun-de-ventanas-wpf.md): el diálogo de varias vistas pertenece al arquetipo C.
- [ADR-0034](0034-project-variables-autoridad-drawing-level.md): el registro de variables se lee una vez por comando.
- [ADR-0035](0035-visibilidad-de-cotas-por-tipo-de-vista.md): sin cambio de política; el tipo de vista cambia de nombre.
- [ADR-0037](0037-reutilizacion-de-cabecera-por-copia-y-distribucion-por-lotes.md): precedente de preparación completa antes de
  escribir.
- [ADR-0039](0039-custom-properties-persistencia-autoridad.md): sin cambio; I-55 no consulta ni amplía su borde.
- ADR-0036 (propuesto en la rama de I-52): las autoridades compartidas del punto 8 se acuerdan con esa iniciativa.

## Referencias

- [Proposal V1 de I-55](../initiatives/I-55-proposal-v1.md): §4-§17 y §22 (D-01..D-17, OD-1..OD-8, X-1..X-6).
- [Discovery de I-55](../initiatives/I-55-discovery.md): matriz sistema × vista × variante, cancelación, hallazgos H-01..H-12.
- [Mapa de implementación V1](../initiatives/I-55-implementation-map-v1.md): gates G3..G16, pruebas y validación del Owner.
- [`RackEmbedComposer`](../../src/RackCad.Application/Persistence/RackEmbedComposer.cs),
  [`BlockPlacement`](../../src/RackCad.Plugin/Drawing/BlockPlacement.cs),
  [`RackLayoutCommands`](../../src/RackCad.Plugin/RackLayoutCommands.cs),
  [`RackDuplicationPlan`](../../src/RackCad.Application/Persistence/RackDuplicationPlan.cs),
  [`DimensionViewPolicy`](../../src/RackCad.Application/Systems/Shared/DimensionViewPolicy.cs),
  [`SelectivePlantaBuilder`](../../src/RackCad.Application/Systems/Selective/SelectivePlantaBuilder.cs),
  [`CantileverViewPlanBuilder`](../../src/RackCad.Application/Systems/Cantilever/CantileverViewPlanBuilder.cs).
