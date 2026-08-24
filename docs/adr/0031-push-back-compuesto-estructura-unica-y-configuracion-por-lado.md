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

6. **`RequiredBedLength` y `AvailableBedSpan` son dos magnitudes distintas.** La primera es la longitud mínima
   que exige la demanda de fondos; la segunda, la longitud físicamente disponible entre los apoyos de la
   estructura efectiva. La única regla de validez es `RequiredBedLength <= AvailableBedSpan`. No se trunca la
   cama, no se aumentan los fondos y no se inventa estructura. Ninguna suma de fondos se codifica: un hueco
   positivo aumenta el span disponible sin aumentar la demanda, y por eso puede volver válida una cama corrida
   que sin él no cabría.

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
   corrida cambia **físicamente** qué extremo es alto; no es un espejo gráfico.

9. **En una corrida gobierna el lado ALTO.** Su larguero posterior es el ancla, exactamente como en I-32. La
   elevación propia del lado bajo **no se borra**: queda dormante mientras esa topología la sustituye y vuelve
   a gobernar en cuanto la celda deja de ser corrida.

10. **El BOM cuenta piezas físicas del plan, no celdas de una rejilla.** La estructura sale **una vez** del BOM
    compartido; el contenido de almacenamiento se cuenta por **ejecución física de cama**. Dos encontradas son
    dos camas, dos largueros bajos, dos altos y hasta dos topes; una corrida es una cama —a la longitud del
    rack entero—, un larguero bajo, uno alto y como mucho un tope. **No se genera A + B para deduplicar
    después**: el plan ya es correcto.

11. **La UI es un selector de lado sobre la matriz que ya existe.** No hay matriz tridimensional, no hay un
    segundo modelo de selección y los cinco alcances (`Cell/Selected/Level/Front/All`) se reutilizan **dentro
    del lado activo**. Cambiar de lado, retirar el lado B o cambiar la topología **no destruye configuración**:
    la del lado que deja de dibujar queda dormante y reaparece intacta.

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
- **Extender la retícula de profundidad compartida para admitir rangos no anidados** — habría permitido que
  coexistieran una ranura presente solo en A y otra presente solo en B, pero toca el contrato del sistema
  Dinámico, que I-40 e I-41 mantuvieron intacto a propósito. Se prefirió declarar la limitación (ver abajo).
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
  - **no pueden coexistir** una ranura presente solo en el lado A y otra presente solo en el lado B: la
    retícula de profundidad compartida exige que los rangos de los frentes aniden. Se reporta con un error
    explícito en vez de producir una estructura incoherente;
  - las decoraciones compartidas del corte lateral (cotas y etiquetas de nivel) siguen el contexto de
    elevaciones del **lado A**, el de referencia: una sola tabla frente/nivel/elevación no puede describir dos
    pasillos a la vez;
  - la **planta colapsa los niveles**, así que una ranura cuyos niveles fueran *todos* camas corridas seguiría
    mostrando los largueros posteriores de la interfaz;
  - el separador central exige hueco mayor que cero; con hueco 0 se declara ausente en vez de dibujar una pieza
    de longitud nula;
  - cuando la demanda mueve la envolvente de un lado, la secuencia de módulos almacenada deja de describir esa
    estructura y se reconstruye por defecto, con la consiguiente pérdida de personalizaciones de módulo — es el
    mismo comportamiento que ya tenía un rack de un sentido al cambiar su envolvente, ahora también aplicable
    al compuesto.

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
