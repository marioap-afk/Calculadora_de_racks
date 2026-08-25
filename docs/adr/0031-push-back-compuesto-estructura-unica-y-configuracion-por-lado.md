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

6. **`RequiredBedLength`, `ResolvedBedLength` y `AvailableBedSpan` pertenecen a autoridades distintas.**

   - `RequiredBedLength` es lo que exige la **DEMANDA** de almacenamiento: cuánta longitud necesitan esos fondos
     con la receta normal. Se mide **sólo** sobre los módulos que alojan tarima, y por tanto **no** depende del
     hueco, ni de la longitud total del rack, ni de la estructura sobrante, ni del ajuste manual vigente.
   - `AvailableBedSpan` es lo que ofrece la **ESTRUCTURA** física efectiva: el tramo realmente utilizable por esa
     cama. **El hueco pertenece a la estructura**, así que suma longitud disponible.
   - `ResolvedBedLength` es la longitud **FÍSICA** que la cama utiliza de verdad: la del **primer apoyo válido**,
     recorriendo desde su ancla ALTA hacia la baja, cuya distancia satisface la demanda. Es la que se dibuja y la
     que se cotiza.

   Se cumple siempre `RequiredBedLength <= ResolvedBedLength <= AvailableBedSpan`, y la regla de validez es
   `RequiredBedLength <= AvailableBedSpan`.

   La relación **no** es `ResolvedBedLength = RequiredBedLength`. Esa igualdad —que esta ADR sostuvo en una
   redacción anterior— obligaba a colocar el extremo bajo de la cama restando una longitud continua, y eso la
   dejaba **flotando entre dos apoyos**: era la causa del defecto por el que una corrida parecía arrancar en el
   segundo fondo. Una cama descansa sobre estructura, no sobre una coordenada calculada. Por eso `ResolvedBedLength`
   **puede** cambiar con el hueco: si para alcanzar su último fondo la cama tiene que cruzarlo, el apoyo válido
   queda más lejos, y ésa es su longitud real. Lo que el hueco no cambia nunca es la **demanda**.

   De la separación se sigue lo que el contrato exige: **un hueco positivo puede volver válida una cama que sin él
   no cabe**, porque aumenta lo disponible sin tocar lo exigido. El hueco **no** es una posición de tarima, **no**
   suma un fondo ficticio y **no** aumenta `DemandPositions`: sólo aporta longitud física. Mezclar las dos
   magnitudes —sumar el hueco también a la demanda— las reacopla y hace que el hueco no pueda rescatar nada por
   construcción; ésa es exactamente la regresión que la autoridad única existe para impedir.

5-bis. **La retícula transversal es del RACK, y la asimetría A/B se expresa con PRESENCIA.**

   Las dos mitades comparten las mismas líneas de postes: el número de frentes es del rack, no de un lado, y crece
   y decrece en los dos a la vez. Que una ranura exista sólo en un lado —el caso `A = 3` y `B = 4`— es
   **presencia**, una propiedad por ranura y por lado, no un conteo distinto en cada matriz.

   Sin esta regla, crecer el rack desde un lado dejaba el otro atrás **en silencio**: el primer frente tenía las dos
   mitades y los demás sólo una. La retícula se iguala también al volver de un archivo o al re-declarar un lado
   dormante, y siempre **creciendo**: las ranuras que un lado no tenía nacen ausentes en él, nunca se recortan las
   del otro. Retirar una ranura que quedaría sin ningún lado, o la última de un lado, se **rehúsa y se explica**.

5-ter. **La topología POR DEFECTO depende de cuántos sentidos tiene el rack.** Uno de un sentido es `SoloA`; uno de
   dos, `Encontradas`. Es la misma regla al cargar y al declarar el lado B — sin re-evaluarla al declararlo, un rack
   nuevo se quedaba en `SoloA` y el lado B aportaba estructura y **ni una sola cama**. Una elección explícita
   distinta (una corrida, `SoloB`) no la pisa nadie: no es el default de ningún modo.

6-bis. **El fondo de una cama CORRIDA es una autoridad PROPIA por celda, no la suma de los fondos de A y de B.**

   Una corrida de 10 fondos es una cama con demanda de 10. No obliga a repartir 5 y 5 entre los lados, y sobre una
   estructura 5 + 8 **no** produce una demanda de 13. `demand = fondo(A) + fondo(B)` sería una tercera autoridad
   derivada de dos que gobiernan otra cosa —la estructura de cada lado— y por eso está prohibida.

   Sin fondo propio, la corrida hereda un **default derivado**: la capacidad en fondos de la estructura, es decir
   «la calle atraviesa el rack». Es un default, exactamente como en I-41 el fondo del frente lo es de la celda; en
   cuanto se escribe un valor propio, manda ése.

   Los fondos de A y de B **no se borran** al volver corrida una celda: quedan **dormantes** y vuelven a gobernar
   en cuanto deja de serlo. Los tres valores conviven, y cambiar de topología es por tanto **reversible**. En el
   editor es el **mismo** campo, con los **mismos** cinco alcances, el que cambia de autoridad según la topología de
   la celda seleccionada; la etiqueta lo dice («Fondo de cama corrida») para que no haya forma de escribir un número
   creyendo que significa otra cosa. Un alcance que mezcla celdas corridas con celdas que no lo son escribe **sólo**
   en las corridas y lo declara.

   La persistencia es **aditiva y anulable**: un documento que nunca usó la autoridad no escribe el campo y se lee
   exactamente igual que antes.

6-ter. **`CorridaDepth` son FONDOS; el rango del frente son MÓDULOS. No son la misma magnitud.**

   Un hueco es un módulo que la cama **atraviesa sin almacenar nada**. El rango físico de una corrida que lo cruza
   tiene por tanto **un módulo más** que su demanda, y esa diferencia no puede colarse de vuelta a la demanda:
   escribir el conteo de módulos donde I-41 espera posiciones de carga repartía **una tarima de más** a lo largo del
   riel y desplazaba todas las posiciones — el defecto que se ve como «la cama está en el fondo equivocado».

   Se separan tres cosas, cada una con su autoridad:

   - `CorridaDepth` → `DemandPositions` (fondos declarados);
   - `DemandPositions` → `RequiredBedLength` (longitud mínima);
   - estructura + `Required` → `ResolvedSpan` (apoyo bajo, apoyo alto, longitud física y módulos atravesados).

   El fondo EFECTIVO de la celda —lo que I-41 y la cama consumen— es siempre la **demanda**. El reparto de tarimas se
   hace sobre la longitud de **almacenamiento** (la cama menos sus huecos) y cada tarima se empuja por los huecos que
   queden antes de ella, de modo que ninguna cae dentro de un hueco. Una estructura **sin** huecos —todo rack de un
   sentido, y todo rack anterior a I-42— produce una lista de huecos vacía y el reparto es literalmente el de I-41.

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

8-ter. **Los dos lados comparten la retícula TRANSVERSAL, no la ALTURA.**

   Comparten dónde caen las líneas de postes, el ancho y el BFR: eso es la retícula, y es una sola. No comparten
   cuántos niveles tienen, ni a qué elevación, ni por tanto cuánto miden sus cabeceras. Una cabecera es una pieza
   **longitudinal** y pertenece a un lado: su altura, su celosía y sus personalizaciones de I-40 salen de la
   sub-estructura de **ese** lado, resuelta con **sus** niveles.

   Con 4 niveles en A y 2 en B, las cabeceras de A miden lo que A pide y las de B lo que pide B, y las **dos** líneas
   de la interfaz —la terminal de A y la inicial de B— pueden medir distinto: son dos piezas. Subir un nivel en A no
   puede mover ni una pieza de B.

   Una autoridad global de altura del tipo `max(alturaA, alturaB)` aplicada a todas las cabeceras está **prohibida**:
   estiraba los postes de B por lo que pedía A. La compuesta ADOPTA, cabecera por cabecera y por `ModuleId`, la
   configuración que su lado ya resolvió.

8-quater. **Un rack compuesto tiene DOS pasillos de carga, y los dos llevan su seguridad por defecto.**

   Físicamente son dos Push Back opuestos: los dos extremos longitudinales son caras de carga y ninguno es un extremo
   alto donde la seguridad estorbe. La autoridad no cambia —sigue siendo la única `PushBackSafetyAuthority`, que
   deduplica y excluye GUIA/PARRILLA/TOPE—; lo que el rack declara es en **cuántos extremos** se materializa. No hay
   que pedirla a mano para el segundo lado, y un rack de un sentido no cambia en nada.

   **«Dos pasillos» viaja en su propio eje** (`BothEndsAreLoadFaces`), derivado y no persistido como `LowEndOnly`.
   No puede expresarse escribiendo `Side`: la **pertenencia** (qué postes llevan la pieza), la **orientación** y el
   **extremo** son tres ejes distintos, y las reglas ADAPTATIVAS de cada familia —la del protector lateral, por
   ejemplo, que sólo lo pone en las dos líneas de orilla— únicamente se aplican cuando el usuario no ha elegido
   lado. Fijar el lado las apaga: el protector aparecía entonces en **todos** los postes y por duplicado.

   Consecuencia: dos pasillos significa que **cada cara de carga recibe el conjunto que tendría un Push Back normal**,
   no que toda línea transversal reciba protectores.

   El tope posterior es otra cosa: vive en el extremo ALTO y su autoridad es **por lado**. Un lado nuevo nace con el
   default del PRODUCTO, nunca con una copia de lo que el usuario ya hubiera personalizado en el otro.

9. **El ancla de una cama es su extremo BAJO, en las DOS direcciones** (decisión del dueño; retira la redacción
   anterior, que hacía del ALTO la autoridad vertical):

   - **Longitudinalmente** manda el BAJO: el extremo por donde se carga y se descarga queda **siempre** anclado al
     poste exterior de su lado, y el ALTO es el que se desplaza hacia dentro cuando la cama pide menos fondo que la
     estructura disponible. Se recorre desde el bajo hacia el alto y se toma el primer apoyo físico que satisface la
     demanda. **Implementado y probado.**
   - **Verticalmente** manda también el BAJO: «Alto 1er nivel» fija la elevación del larguero de entrada, y el ALTO
     se **deriva** de esa altura más la pendiente sobre la longitud real de la cama, resuelta contra los troqueles.
     Ni la topología ni el fondo pueden mover el larguero de entrada. **Implementado y probado.**

     Esto **supersede** la redacción de I-32/PB-004, que hacía del ALTO la autoridad vertical. El criterio de
     selección NO cambia —menor error de pendiente contra 7/192 sobre la retícula de 2"—; cambia cuál de los dos
     extremos se conserva y cuál se elige. Los desempates, en orden: (a) menor error de pendiente; (b) el más
     cercano al ALTO teórico (`contacto bajo + subida nominal sobre la longitud real de la cama`); (c) el de menor
     elevación. El tercer desempate de la regla anterior —la cercanía al resultado PRE-I-32— pertenecía a la
     selección del BAJO, no tiene equivalente para el alto y queda **retirado**.

     La autoridad es UNA: `PushBackElevations`. El corte lateral, los dos cortes frontales, la cama, los apoyos
     intermedios, el desviador y el tope posterior leen de ahí. Leer `EntranceElevation` del resolver compartido
     para dibujar el larguero alto sería una **segunda** autoridad vertical, y era el defecto real: la misma pieza
     física salía en dos troqueles distintos según la vista.

   Anclar longitudinalmente en el alto —como hizo una redacción anterior— dejaba la cama arrancando **dentro** del
   rack, con el pasillo delante inaccesible. Es un defecto físico, no una preferencia de dibujo.

   Una corrida **puede** atravesar parte del lado alto, el hueco y parte
   del bajo **sin llegar al extremo exterior del lado bajo**; y cuando cruza el hueco lo **atraviesa** sin gastar en
   él longitud de demanda. La estructura sobrante no se destruye ni se reduce: puede existir porque otros niveles o
   frentes la necesitan, y ése es justamente el caso de los frentes largos que gobiernan la estructura mientras
   otros reutilizan sólo una parte. La elevación propia del lado bajo **no se borra**: queda dormante mientras esa
   topología la sustituye y vuelve a gobernar en cuanto la celda deja de ser corrida.

8-bis. **Los TOPES de los dos lados son visibles y editables sin salir del panel compuesto.** La autoridad no
   cambia —sigue siendo el `PushBackRearTopeConfig` de cada lado y sus celdas apagadas—; lo que se añade es la
   superficie: `Tope lado A` y `Tope lado B` con los MISMOS cinco alcances. Un tope vive en el extremo ALTO de una
   cama, así que la **topología** decide cuál puede materializarse: dos con camas encontradas, y uno solo con una
   sola cama (el del lado alto, que en una corrida es el del sentido). La casilla del lado que hoy no aplica se
   deshabilita **con su motivo** y conserva lo elegido: la intención queda dormante y vuelve intacta.

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
  - **«Alto 1er nivel» se mide desde el TROQUEL UTILIZABLE MÁS BAJO del poste**, que es el cero real del producto:
    `0"` pone el larguero exactamente en ese troquel y cualquier otro valor es un **offset** sobre él, resuelto
    después contra la retícula. La lectura anterior trataba el número como una elevación **absoluta** ajustada al
    troquel *más cercano*: `0` no significaba nada físico y, con un poste cuya retícula empezara más arriba, podía
    caer por debajo del piso.

    El datum sale de la **geometría del poste** —su mate `TROQUEL_LARGUERO` y el paso de troquel—, nunca de una
    constante: cada perfil puede tener el suyo. La autoridad es neutral y **compartida** (`RackFirstLevelDatum`)
    porque el dato de usuario es literalmente el mismo en el **Dinámico** y en el **Push Back**, y los dos lo
    resuelven en el mismo sitio (`DynamicRackSystemResolver`). No se toca ningún sistema que hable de otra cosa.

    La compatibilidad es **aditiva**: el documento declara su datum (`FirstLevelDatum`), y **ausente = lectura
    histórica**. Ningún archivo existente se reinterpreta ni se mueve, y la migración no resta ninguna constante —
    mide la elevación física ya resuelta y la re-expresa desde el nuevo datum. El marcador viaja también al sistema
    resuelto y al snapshot, para que `RACKEDITAR` no lo pierda;
  - una corrida que **cruza el hueco** lo atraviesa sin gastar demanda en él, pero su longitud FÍSICA sí lo
    incluye: para llegar a su último fondo tiene que salvarlo. Su extremo bajo está en el poste exterior y el alto
    apoya en una línea de módulo real, nunca en un punto intermedio;
  - la **selección de edición** puede ser «ambos lados». Es una operación del editor, **no** un tercer lado: no
    existe en el dominio, en el archivo ni en el dibujo, y no posee ninguna pieza. Escribe la misma intención en A y
    en B; lo que por definición es de un lado —la presencia de un frente, el ajuste manual de estructura— se
    deshabilita con su motivo en vez de aplicarse a ciegas, y un campo cuyo valor difiera entre los lados se muestra
    **vacío** en lugar de mentir eligiendo uno de los dos;
  - el **fondo de almacenamiento** y la **estructura longitudinal** son dos autoridades distintas y se presentan
    como tales: el fondo base del frente y el fondo por celda dicen cuánto se almacena; la estructura dice hasta
    dónde llega el acero, con su propuesta automática, su efectiva y su ajuste manual. Ninguna escribe en la otra;
  - la **planta proyecta todos los frentes**: los intermedios se reponen por cama y en el marco de cada una, y dos
    frentes físicamente distintos nunca se deduplican entre sí — la planta colapsa NIVELES, no FRENTES;
  - **una estructura más larga que una cama es normal y no deja piezas huérfanas**: la estructura efectiva puede
    tener 8 fondos mientras una celda usa 4, y en el tramo sobrante esa cama no pone riel, rodillo, intermedio ni
    tarima. Otro nivel del MISMO rack puede usar los 8 a la vez: la estructura es capacidad, no longitud obligatoria;
  - un rack compuesto se **reabre cargando el lado A contra su propio diseño**, no contra la estructura compartida:
    el estado del editor se reconstruye desde el sistema resuelto, y el resuelto de un rack compuesto lleva rangos no
    anidados que no son los de ningún lado;
  - una celda bloqueada **se sigue dibujando** en la vista previa, apoyada en el tramo más largo que la estructura
    ofrece —`ResolvedBedLength = AvailableBedSpan`— porque `Resolved <= Available` no admite excepciones: una cama
    volando fuera de la estructura no descansaría en nada. Lo que le falta lo dice el **diagnóstico**, con su
    dirección y su medida, y el editor no deja insertar a ciegas.

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
