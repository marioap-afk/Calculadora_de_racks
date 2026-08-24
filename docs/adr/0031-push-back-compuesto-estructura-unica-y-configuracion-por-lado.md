# ADR-0031: El Push Back compuesto tiene UNA estructura física y DOS configuraciones funcionales

- **Estado:** propuesto
- **Fecha:** 2026-08-23
- **Decisores:** dueño del repo (pendiente de aceptación); Claude (redacción e implementación)
- **Iniciativa relacionada:** I-42 — `feature/push-back-compuesto`

## Contexto

Hasta I-41 un Push Back tiene **un solo sentido de flujo**: el extremo bajo en el arranque del frente, el
extremo alto hacia el fondo, y una sola cama por celda. El producto real necesita, dentro de **un mismo
sistema físico**, dos lados enfrentados: cantidades de frentes distintas por lado, niveles y elevaciones
independientes, fondos y tarimas por celda en cada lado, camas encontradas o corridas —y estas últimas en los
dos sentidos—, un hueco entre las dos mitades con separador central opcional, y topes donde físicamente
proceda.

La tentación evidente es construir *rack A + rack B espejado* y deduplicar después el BOM. Eso produce dos
estructuras que pueden describir cosas distintas del mismo rack, y un BOM que se corrige a posteriori en vez de
ser correcto por construcción.

Además, tres autoridades ya establecidas no pueden reabrirse: la regla de fondo por celda de
[ADR-0030](0030-fondo-por-celda-push-back-y-envolvente-derivada.md) (`override de la celda ?? fondo por defecto
del frente`), la identidad por línea física de I-40 (`HeaderLineOverrides` por `(PostIndex, ModuleId)`) y la
física de elevaciones de I-32 (el larguero posterior es el ancla y el bajo se deriva minimizando el error de
pendiente sobre la retícula de troqueles).

## Decisión

**Un Push Back compuesto es UNA estructura física con DOS configuraciones funcionales de almacenamiento.**

1. **Separación de autoridades.** La *estructura física* —postes, perfil y peralte, cabeceras, separadores,
   postes derivados, placas, alturas, seguridad, anotaciones y los overrides por línea de I-40— es propiedad
   **única del rack** y vive en una sola intención. La *configuración funcional* —frentes, niveles,
   elevaciones, fondos, tarimas, celdas y topes— pertenece a **cada lado** y no puede describir estructura.

2. **El lado A es el de referencia y es el legacy.** Su configuración funcional ES la del diseño anterior a
   I-42. Un rack de un solo sentido no tiene lado B ni intención de interfaz, y por tanto se comporta y se
   persiste exactamente igual que antes: su JSON se re-escribe byte-idéntico y su camino de resolución es el
   anterior a la iniciativa, sin pasar por la composición.

3. **Una sola secuencia de profundidad.** El orden físico es `módulos de A → línea terminal de A → HUECO →
   línea inicial de B → módulos de B`, con los módulos de B **invertidos** porque su pasillo es el otro
   extremo. En la interfaz siguen existiendo **dos líneas de postes distintas**, también con hueco 0. El hueco
   es una **longitud física real** (nunca un desplazamiento visual, un fondo ficticio ni una posición de
   tarima) y ocupa su propia posición en la secuencia. Cuando lleva separador central se materializa como el
   **mismo separador** que el rack ya usa —una pieza, contada una vez—; sin él no dibuja nada.

4. **Una sola retícula transversal.** Cada ranura toma la **mayor demanda aplicable** de los dos lados
   (calles, ancho de larguero y niveles), de modo que las líneas de postes y el BFR son únicos. La altura de
   poste es del rack: las dos sub-estructuras se resuelven con la altura de la estructura compuesta.

5. **La estructura efectiva es editable por lado.** La cadena es `demanda de celdas → envolvente por ranura →
   estructura PROPUESTA del lado → override manual → estructura EFECTIVA`. La propuesta se **deriva siempre** y
   nunca es autoridad inmutable; el override la **sustituye**, no la acota; **restaurar** es eliminar el
   override y volver a la propuesta *actual*. Una estructura insuficiente **no se corrige en silencio**: se
   respeta, y las celdas que no caben se declaran imposibles con su motivo.

6. **`RequiredBedLength`, `AvailableBedSpan` y `PhysicalBedLength` pertenecen a autoridades distintas.**

   - `RequiredBedLength` es lo que exige la **DEMANDA** de almacenamiento: cuánta longitud necesitan esos fondos
     con la receta normal. Se mide **sólo** sobre los módulos que alojan tarima, y por tanto **no** depende del
     hueco, ni de la longitud total del rack, ni de la estructura sobrante, ni del ajuste manual vigente.
   - `AvailableBedSpan` es lo que ofrece la **ESTRUCTURA** física efectiva: el tramo realmente utilizable por esa
     cama. **El hueco pertenece a la estructura**, así que suma longitud disponible.
   - `PhysicalBedLength = RequiredBedLength`, **nunca** `AvailableBedSpan` ni la longitud total del rack: una cama
     no se estira hasta la capacidad disponible, y tampoco se recorta contra ella cuando no cabe.

   La única regla de validez es `RequiredBedLength <= AvailableBedSpan`. De la separación se sigue lo que el
   contrato exige: **un hueco positivo puede volver válida una cama que sin él no cabe**, porque aumenta lo
   disponible sin tocar lo exigido ni alargar la cama. El hueco **no** es una posición de tarima, **no** suma un
   fondo ficticio y **no** aumenta `DemandPositions`: sólo aporta la longitud física que faltaba. Mezclar las dos
   magnitudes —sumar el hueco también a la demanda— las reacopla y hace que el hueco no pueda rescatar nada por
   construcción; ésa es exactamente la regresión que la autoridad única existe para impedir.

   No se trunca la cama, no se aumentan los fondos y no se inventa estructura. Ninguna suma de fondos se codifica.

   **La capacidad se mide POR CAMA FÍSICA, no por celda.** Dos camas encontradas son dos piezas independientes,
   cada una medida contra su propia estructura. Medir la demanda de una contra el espacio de la otra producía
   errores de capacidad inexistentes.

7. **La topología es POR CELDA (ranura × nivel), no global ni por frente.** Cuatro modos físicos: `Solo A` y
   `Solo B` (una cama), `Encontradas` (**dos** camas independientes, con sus extremos altos enfrentados y topes
   independientes) y `Corrida` (**una** cama que atraviesa A + hueco + B: una longitud, una pendiente continua,
   un eje y como mucho **un** tope, en su extremo alto). Un nivel que solo existe en un lado degrada de forma
   explícita a ese lado **sin tocar la intención almacenada**, que queda dormante.

8. **El lado B es una imagen especular física, no una decoración espejada.** Cada cama se resuelve con el
   código ya validado de un Push Back de un sentido **en su propio marco** —donde el flujo avanza hacia +X— y
   el conjunto completo (riel, rodillos, tope de cama, tarima, largueros e intermedios) se lleva al rack con
   **una sola reflexión rígida**. La reflexión niega la rotación y conmuta el espejo en X, y **no toca las
   elevaciones**: el eje es vertical, así que el extremo alto sigue siendo el alto. Cambiar el sentido de una
   corrida cambia **físicamente** qué extremo es alto; no es un espejo gráfico. Una **anotación o una cota no son
   piezas**: sólo se traslada su posición, porque reflejarlas las dejaría escritas del revés.

8-bis. **Un larguero intermedio pertenece a una CAMA, no a la estructura.** Sostiene su riel, sigue su pendiente y
   vive en su marco, así que se construye por cama y viaja con su misma transformación. Resolverlos una sola vez
   sobre la estructura compuesta daba ejes que no son los de ninguna cama real. El BOM los cuenta con el **mismo**
   builder que los dibuja, de modo que la cantidad cotizada y la dibujada no pueden divergir.

9. **En una corrida gobierna el lado ALTO, y es también su ancla FÍSICA.** Su larguero posterior es el ancla de
   elevación, exactamente como en I-32, y además el extremo desde el que la cama se desarrolla: desde el ALTO hacia
   el BAJO, exactamente `RequiredBedLength`. Una corrida **puede** atravesar parte del lado alto, el hueco y parte
   del bajo **sin llegar al extremo exterior del lado bajo**; y cuando cruza el hueco lo **atraviesa** sin gastar en
   él longitud de demanda. La estructura sobrante no se destruye ni se reduce: puede existir porque otros niveles o
   frentes la necesitan, y ése es justamente el caso de los frentes largos que gobiernan la estructura mientras
   otros reutilizan sólo una parte. La elevación propia del lado bajo **no se borra**: queda dormante mientras esa
   topología la sustituye y vuelve a gobernar en cuanto la celda deja de ser corrida.

9-bis. **El sistema sintético de una corrida es una RECETA, no un rack.** No materializa postes, cabeceras, placas
   ni separadores, no aparece en ninguna vista como estructura y no aporta una sola línea al BOM estructural. Sirve
   únicamente para que la física ya validada de un sentido resuelva el contenido de esa cama.

10. **El BOM cuenta piezas físicas del plan, no celdas de una rejilla.** La estructura sale **una vez** del BOM
    compartido; el contenido de almacenamiento se cuenta por **ejecución física de cama**. Dos encontradas son
    dos camas, dos largueros bajos, dos altos y hasta dos topes; una corrida es una cama —**a SU longitud
    requerida**, no a la del rack—, un larguero bajo, uno alto y como mucho un tope. **No se genera A + B para
    deduplicar después**: el plan ya es correcto.

11. **La UI es un selector de lado sobre la matriz que ya existe.** No hay matriz tridimensional, no hay un
    segundo modelo de selección y los cinco alcances (`Cell/Selected/Level/Front/All`) se reutilizan **dentro
    del lado activo**. Cambiar de lado, retirar el lado B o cambiar la topología **no destruye configuración**:
    la del lado que deja de dibujar queda dormante y reaparece intacta. Con el rack de un solo sentido la sección
    compuesta se **colapsa**, no se queda deshabilitada ocupando la barra lateral.

11-bis. **Ninguna entrada inválida se corrige en silencio.** Un hueco negativo se conserva tal cual y bloquea con su
    motivo; un ajuste manual de estructura por debajo del mínimo físico **no** se convierte en «sin ajuste», porque
    eso significa RESTAURAR y restaurar sólo ocurre por acción explícita del usuario. Una intención inválida no se
    resuelve: se declara.

11-ter. **Los módulos supervivientes conservan su identidad.** Cuando la estructura de un lado crece o encoge, cada
    pieza que sigue existiendo en la misma posición contada desde el extremo exterior del lado —y con el mismo
    carácter físico— conserva su `ModuleId`, su configuración personalizada y su longitud manual, y con ellos los
    `HeaderLineOverrides` de I-40 que la apuntan. Una pieza nueva nace calculada; una que desapareció no deja
    rastro y su override **no** se traslada a otra.

12. **Las etiquetas A/B son información gráfica del plano.** Se emiten por el pipeline de anotaciones que ya
    existe (mismo rol, misma escala, misma capa) en planta y en los cortes laterales, y **nunca** entran al BOM.

## Alternativas consideradas

- **Rack A + rack B espejado, con el BOM deduplicado al final** — descartada explícitamente por el contrato de
  la iniciativa y por su consecuencia técnica: dos estructuras pueden describir cosas distintas del mismo rack,
  y un BOM correcto por corrección posterior deja de serlo en cuanto aparece un caso nuevo.
- **Un `PushBackDesign` nuevo, simétrico, con los dos lados al mismo nivel** — más elegante sobre el papel,
  pero obliga a migrar todo documento existente y a mantener dos formas del mismo rack durante la migración.
  Se prefirió el modelo aditivo, donde el lado A *es* el legacy y `SideB`/`Composite` son nulos en todo
  documento anterior.
- **Declarar como limitación que no coexistan una ranura solo-A y otra solo-B** — se descartó: contradice el
  contrato de I-42, donde `F1` compartida, `F2` solo A, `F3` solo B y `F4` compartida deben convivir en una sola
  estructura. Lo adoptado es un **modo explícito** de la autoridad de profundidad (`DynamicDepthNesting.NotRequired`)
  que sólo enciende el compositor del Push Back compuesto, que es intención **derivada** y **nunca se persiste**, y
  que deja intacto el contrato del sistema Dinámico: allí el anidamiento se sigue exigiendo igual.

- **Reescribir la geometría de la cama para admitir un sentido negativo** — habría duplicado la física de
  elevaciones, rotación y tangencias de I-32 en un segundo camino que podría divergir. La reflexión rígida
  reutiliza el camino ya validado por el Owner.
- **Explotar los grupos anidados del lado B en instancias sueltas al reflejarlos** — más simple, pero pierde el
  patrón ARRAY y con él el rendimiento de inserción que motivó su existencia. Se refleja la *definición*.

## Consecuencias

- Positivas:
  - una sola propiedad física de postes, cabeceras, placas, separadores y camas, y un BOM correcto por
    construcción;
  - toda la física de Push Back ya validada por el Owner (I-32, I-40, I-41) se **reutiliza**, no se reescribe:
    un lado no tiene reglas propias, tiene un **marco** propio;
  - un rack anterior a I-42 se comporta y se persiste exactamente igual, sin pedir reconfiguración;
  - la capacidad geométrica deja de ser un conteo de fondos y pasa a ser una comparación de longitudes reales,
    de modo que el hueco y el ajuste manual de estructura tienen efecto físico verificable.
- Negativas / costos aceptados:
  - el separador central exige hueco mayor que cero; con hueco 0 se declara ausente en vez de dibujar una pieza
    de longitud nula;
  - una ranura presente en **los dos** lados tiene estructura a lo largo de toda la profundidad: su fondo por lado
    gobierna dónde acaban sus **camas**, no dónde acaban sus marcos. Es la misma regla con la que I-41 hace que un
    nivel más corto termine antes dentro de la estructura de su frente;
  - un **ajuste manual** de estructura se aplica a todas las ranuras de ese lado (el usuario ha declarado cuánto
    mide el lado). Sin ajuste manual cada ranura conserva su propia envolvente, de modo que frentes cortos y largos
    conviven igual que en I-41;
  - una ranura ausente en un lado que quede en una posición **interior** deja en el corte frontal de ese lado la
    línea de postes de su frontera, por la regla de I-33 que conserva los bordes exteriores de un frente en blanco.
    Las ausencias del final sí se retiran, que es el caso habitual (`A=3`, `B=4`);
  - una corrida que **cruza el hueco** apoya su extremo bajo tantas pulgadas dentro del lado bajo como mida el
    hueco, porque su longitud es la de su demanda y el hueco no consume demanda. Es consecuencia directa de
    `PhysicalBedLength = RequiredBedLength`, y se declara para que se vea en la validación en AutoCAD;
  - una celda bloqueada **se sigue dibujando** en la vista previa, con su cama sobresaliendo de la estructura: ver
    cuánto le falta es más útil que verla recortada a un tamaño que nadie pidió. El editor la declara y no deja
    insertar a ciegas.

## Referencias

- [ADR-0030](0030-fondo-por-celda-push-back-y-envolvente-derivada.md) — fondo por celda y envolvente derivada
  (I-41). Sigue vigente y no se reabre.
- I-40 — línea física como segunda dimensión del modelo (`HeaderLineOverrides` por `(PostIndex, ModuleId)`).
- I-32 — la autoridad de elevaciones de Push Back: el posterior es el ancla, el bajo se deriva minimizando el
  error de pendiente sobre la retícula de 2".
- [ADR-0017](0017-validacion-cargas-diferida-ram-elements.md) — la validación de cargas sigue diferida; I-42 no
  la toca.
- Implementación: `src/RackCad.Domain/Systems/PushBack/PushBackSide.cs`,
  `PushBackSideDesign.cs`, `PushBackCompositeDesign.cs`, `PushBackCompositeSystem.cs`;
  `src/RackCad.Application/Systems/PushBack/PushBackSideConfiguration.cs`, `PushBackCompositeStructure.cs`,
  `PushBackCompositeResolver.cs`, `PushBackBedSpan.cs`, `PushBackMirror.cs`, `PushBackRuns.cs`,
  `PushBackCompositeContent.cs`, `PushBackCompositeFrontal.cs`, `PushBackCompositePlanta.cs`,
  `PushBackSideAnnotations.cs`, `PushBackCompositeEditorState.cs`, `PushBackCompositeEditorAssembler.cs`,
  `PushBackCompositeDiagnostics.cs`.
