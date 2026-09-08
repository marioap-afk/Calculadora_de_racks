# ADR-0033: La validación se organiza por clase de evidencia y por SHA exacto, no por equivalencia de contenido

- **Estado:** **propuesto**
- **Fecha:** 2026-09-07 (propuesto)
- **Decisores:** dueño del repo (**pendiente**: solo el dueño acepta o rechaza, `adr/README.md`);
  Coordinador de I-45 y Arquitecto de I-45 (consenso técnico sobre `PLAN VERSION V4`, cuatro rondas de
  revisión); Claude (redacción)
- **Iniciativa relacionada:** I-45 — `architecture/test-validation-workflow`
  ([contrato](../initiatives/I-45-test-validation-workflow.md), [plan](../initiatives/I-45-plan.md))

> **Este ADR está `propuesto`, no aceptado.** El consenso Coordinador ↔ Arquitecto establece que la
> decisión está lista para implementarse; **no** equivale a aceptación. Son dos actos de dos autoridades
> distintas y el repositorio lo fija así: «Estados: `propuesto` → `aceptado` | `rechazado`; **Solo el
> dueño del repo acepta o rechaza**. Los agentes pueden redactar ADRs en estado `propuesto`»
> ([`adr/README.md`](README.md)).

## Contexto

La fase de Discovery de I-45 midió el coste de validar un cambio en RackCad y encontró que **el problema
no está donde se suponía**. Tres hechos lo gobiernan y ninguno era conocido antes:

**El reloj del CI no es atacable.** Su mediana es de pocos minutos y su camino crítico es un `max()`
sobre dos ramas casi iguales —el job de UI y la cadena `tests → build-plugin`—, de modo que eliminar
entero el job más largo tiene una cota de ahorro de alrededor del diez por ciento. Debajo, el suelo de
ruido de la instrumentación es de un factor dos sobre poblaciones de prueba idénticas. Cualquier ahorro
por debajo de ese suelo es indistinguible del ruido. Y los objetivos de tiempo hasta la primera
evidencia relevante que la iniciativa se fijó **ya se cumplen** con la arquitectura actual.

**La repetición sí es atacable, pero no donde parecía.** Sobre tres iniciativas recientes se
contabilizaron del orden de tres ejecuciones de suite completa por commit, de las cuales una parte
sustancial es reconfirmación. Pero una fracción importante de ese conteo es **autodeclaración en
cuerpos de commit**, el canal menos fiable del corpus: el Discovery refuta una de esas
autodeclaraciones con un caso medido contra el log del propio CI.

**La selección automática de pruebas no es fundable con la evidencia que este repositorio puede
producir.** No existe un solo caso histórico en que una prueba preexistente detectara un defecto
introducido por un cambio de otro sistema: solo la **exposición** es medible, nunca la
**materialización**. El oráculo obvio —comparar la suite completa contra lo que un selector habría
elegido— es **ciego por construcción**, porque la suite completa no detectó ninguno de los defectos
catalogados y sus únicos rojos fueron reproducciones anunciadas por su propio autor. Y el repositorio
no sostiene metadata: `eng/` contiene un solo archivo, los directorios de estado y evidencia de
automatización se detuvieron hace cinco iniciativas, y un campo ya obligatorio del formato de
validación manual se cumple en cero de cuarenta y seis archivos.

A esto se añade una restricción técnica que se descubrió tarde y que descarta la solución que parecía
más elegante: **`Directory.Build.props` declara compilación determinista acotada al commit** —«dos
compilaciones del **mismo commit** producen ensamblados byte-idénticos»— y `Directory.Build.targets`
estampa el SHA del commit en `InformationalVersion`. Dos commits con el mismo árbol de git producen
ensamblados distintos **por diseño**.

Quiénes encontraron los defectos históricos también importa, porque delimita lo que se puede retirar:
de dieciocho defectos catalogados, la validación manual del dueño encontró ocho y la revisión
arquitectónica seis; la suite completa, el CI y las pruebas preexistentes encontraron cero. **Eso no
prueba que la suite completa carezca de valor** —su oráculo es ciego: un defecto atrapado antes del
push no deja rastro— pero sí prohíbe financiar una reducción con el argumento de que «nunca ha
detectado nada».

## Decisión

La validación de RackCad se organiza sobre dos ejes: **la clase de evidencia** y **el SHA exacto**. No
sobre equivalencia de contenido, no sobre selección por impacto y no sobre niveles de riesgo.

### 1. Objetivo

Mantener o aumentar la seguridad **reduciendo la repetición de validación que no aporta evidencia
nueva**. El objetivo declarado **no** es acelerar una corrida de CI, y esta decisión **no promete
ninguna aceleración**: el cambio esperado del reloj del CI es aproximadamente cero, y es intencional.

### 2. Roles local y CI

**Iteración ordinaria.** La suite completa de UI **deja de ser obligatoria en local en cada
iteración**. La evidencia intermedia de UI se obtiene del CI: un SHA tiene evidencia de UI cuando el
job de UI del CI está en verde **sobre ese SHA**. Un SHA sin corrida local de UI y sin corrida propia
de CI de UI es un **SHA sin evidencia de UI** y **no puede ser Candidato**.

**Núcleo.** Esta regla **no se extiende por analogía al núcleo**. La evidencia local del núcleo y la
del CI son clases distintas: el CI corre el núcleo en ubuntu y el bucle local en Windows, y existen
pruebas cuyo resultado depende de bytes en disco y de la cultura del hilo.

### 3. Candidato

Un Candidato es un **SHA exacto** con evidencia asociada. El mecanismo debe poder responder, sin
inventar un manifiesto nuevo y usando el registro documental vivo de la iniciativa:

- qué SHA exacto se validó;
- si el árbol de trabajo estaba limpio al producir la evidencia local;
- qué SDK de .NET quedó resuelto;
- si pasó la suite completa local del núcleo;
- si pasó la suite completa local de UI;
- si pasó el build Debug de UI;
- si pasó el build Debug del Plugin;
- si el CI pasó **sobre ese SHA exacto**;
- si la validación del dueño en AutoCAD era requerida y, en tal caso, cuál fue su veredicto.

### 4. Las clases de evidencia no son intercambiables

No se infiere que la evidencia local del núcleo equivalga a la del CI, ni la local de UI a la del CI,
ni un build Debug a un build Release del CI, ni las pruebas a la validación del dueño en AutoCAD. **La
única excepción es la regla de UI de §2 durante la iteración ordinaria**, y es una excepción normativa
explícita, no una inferencia.

### 5. Reutilización de evidencia por SHA exacto

Quedan **retiradas**: la clave de estado de validación, la equivalencia por contenido parcial, la
equivalencia entre SHA distintos y la igualdad de árbol como frontera universal.

La regla es: **mismo commit, mismo SHA**. Si una evidencia requerida ya existe sobre ese SHA exacto y
sigue siendo válida para el mismo propósito, cruzar otro gate administrativo **no obliga a repetirla
por ceremonia**. El objetivo de proceso es **una ejecución de suite completa por Candidato**, salvo
invalidación explícita.

Debe leerse con su alcance real: **esa métrica ya está esencialmente satisfecha en el CI** —cada
Candidato histórico tiene exactamente una corrida— y por tanto el valor inicial de esta regla se
concentra en **evitar la repetición ceremonial local sobre el mismo SHA**. No promete ningún porcentaje
de ahorro.

### 6. Invalidación

Además de cambiar el SHA, y **solo** estos:

- **evidencia local** — el árbol de trabajo estaba sucio cuando se produjo;
- **evidencia dependiente del SDK** — el SDK resuelto cambió;
- **evidencia del dueño en AutoCAD** — cambió la versión de AutoCAD o la biblioteca externa de bloques.

Esta lista no se amplía especulativamente.

### 7. Merge

**`tree(merge) == tree(segundo padre)` NO implica evidencia reutilizable.** RackCad estampa el SHA del
commit en los ensamblados y un merge `--no-ff` produce un SHA nuevo, de modo que el DLL construible
desde el trunk no es el que se construyó desde la punta de la rama. Por eso **el CI posterior al merge
sigue siendo obligatorio** aunque el árbol de git sea idéntico. La alternativa de reutilizar la
evidencia del segundo padre queda **rechazada**.

### 8. Suite completa

Se conservan la suite completa del Candidato, la suite completa final y el CI posterior al merge.

### 9. Cobertura

Sin cobertura en el push ordinario; con cobertura en el Candidato y en el trunk. La cobertura es una
**señal de salud**: no es un selector por impacto y **no demuestra sensibilidad de las aserciones** —se
midió un defecto cuyas líneas exactas se ejecutaban decenas de miles de veces con la suite en verde—.

### 10. Validación del dueño

Se conserva donde la automatización no cubre adecuadamente AutoCAD real, la interacción, la experiencia
de uso, el resultado visual, el DWG y la integración real. **No se reduce por política general.**

### 11. Duración activa del dueño

Se instrumenta como **experimento**, y como experimento no gobierna nada. El agente pregunta en la
misma ronda de validación: «Duración activa aproximada de esta validación: ___ min». Solo se registra
un valor **dado explícitamente por el dueño**; **nunca se infiere de marcas de tiempo**.

**Que el dato falte —o que nadie lo preguntara— no es un fallo.** No invalida la validación, no cambia
su veredicto, no impide que un cambio esté terminado y **no bloquea ninguna integración**. Es un dato
experimental ausente, y nada más.

Se evalúa cuando existan al menos diez rondas elegibles —aquellas en que el dueño ejecutó realmente
una validación manual— repartidas en al menos tres iniciativas: se conserva si la **tasa de captura**
alcanza el ochenta por ciento y **se retira del proceso** si no lo alcanza. Se dice *tasa de captura*
y no *cumplimiento* a propósito: aquí «cumplimiento» nombra la violación de un campo obligatorio, y
este campo no lo es.

La sede operativa —qué ronda cuenta, la pregunta exacta, qué incluye y excluye, y dónde se
registra— es [`docs/guias/validacion-manual-autocad.md`](../guias/validacion-manual-autocad.md) §8,
que se sostiene sola. Esta sección enuncia la decisión; la guía la ejecuta.

### 12. Lo que NO se introduce ahora

No se introducen niveles operativos de validación, ni modelo operativo de riesgo, ni selector por
impacto, ni el nombre completamente calificado como frontera de seguridad, ni selección por rutas, ni
selección por grafo de proyectos, ni etiquetas masivas, ni mapa de pruebas mantenido a mano, ni mapa de
riesgo mantenido a mano, ni un CI rápido por riesgo, ni múltiples hilos STA, ni el Golden DWG.

**No porque estén prohibidos para siempre**, sino porque hoy su seguridad, su retorno y su coste de
mantenimiento no están suficientemente demostrados.

### 13. Criterios de reapertura

- **Selección por impacto** — cuando exista un instrumento defendible para medir capacidad de
  detección: controles positivos, defectos sembrados o un corpus de mutación representativo. La
  observación pasiva no sirve: produciría un registro vacío, y un registro vacío no es evidencia.
- **Taxonomía** — cuando exista un consumidor operacional real que justifique mantenerla y que pueda
  verificarse suficientemente.
- **Múltiples hilos STA** — tras controlar el estado global mutable relevante, disponer de un
  *benchmark* reproducible y demostrar retorno.
- **CI rápido separado del completo** — cuando el tiempo hasta la primera evidencia relevante deje de
  cumplir sus objetivos, o cuando exista una reducción demostrablemente segura y material.
- **Golden DWG** — cuando exista infraestructura suficiente para comparar salidas reales.
  **Invariante: el baseline de un Golden NUNCA se actualiza solo; el dueño aprueba cada cambio de
  baseline.**

## Alternativas consideradas

- **Niveles operativos de validación (T0–T4) y modelo de riesgo (R0–R4)** — descartados: el objetivo de
  tiempo hasta la primera evidencia relevante **ya se cumple**, el techo de cualquier intervención
  sobre el CI queda por debajo del suelo de ruido, y un riesgo autodeclarado sin mecanismo que lo
  fuerce es la clase de dato que este repositorio degrada en silencio.
- **Selección por impacto como compuerta** — descartada: su recall no es acotable desde este
  repositorio y el oráculo pasivo es ciego por construcción. Un contradiseño agresivo se midió y
  resultó **más amplio** que la heurística por nombre que ya se había rechazado.
- **Selección por nombre completamente calificado, por rutas o por grafo de proyectos** — descartadas:
  la primera selecciona una fracción enorme de la suite para un cambio de un archivo; la segunda es hoy
  la misma heurística de subcadena; la tercera es indistinguible de correrlo todo en la gran mayoría de
  los cambios con código, y donde recorta **recorta mal**, porque pierde los archivos del núcleo que
  verifican el Plugin y la UI **leyendo su texto** sin que ningún proyecto de pruebas los referencie.
- **Taxonomía de etiquetas** — descartada: sin selector no hay consumidor de selección, y el
  diagnóstico se cubre mucho más barato con el informe de resultados de prueba, que nadie mantiene.
- **Clave de estado de validación por contenido** — descartada tras medirla: su versión corregida
  seguía comprando su valor incremental **exclusivamente** excluyendo la documentación, que es
  justamente la parte cuya seguridad exigía definir un conjunto de entradas, demostrar la clausura de
  lecturas del repositorio, llevar una tupla de canal y exigir árbol limpio. Un sistema entero para un
  margen pequeño, y precisamente la clase de metadata que esta misma decisión rechaza en todo lo demás.
- **Igualdad de árbol como frontera** (`tree(merge) == tree(segundo padre)`) — descartada por la
  estampación del SHA en los ensamblados. Se midió que los árboles de merge coinciden con los de su
  segundo padre, pero la conclusión operativa era falsa: identidad de árbol no es identidad de build.
- **Retirar la cobertura por completo** — defendible con la misma evidencia y **no adoptada**: su
  pregunta legítima —dónde escribir pruebas nuevas— sigue teniendo valor y una muestra periódica la
  responde igual.
- **Reducir la validación manual del dueño** — descartada: es el detector más eficaz medido y está
  fuera del alcance permanente de la iniciativa.
- **Múltiples hilos STA inmediatamente** — descartada: hay contraindicaciones de estado global de
  proceso y una condición de carrera **ya viva**, y el trabajo serializado real sobre ese hilo sigue
  sin medirse.

## Consecuencias

**Positivas.** La única frontera de reutilización que queda **no puede producir una falsa equivalencia
por construcción**: compara identificadores, no infiere nada. No se añade metadata que mantener, ni
clave, ni clausura, ni tupla de canal. La evidencia intermedia de UI se **traslada**, no se elimina, y
el hueco del push agrupado queda cerrado por la regla del SHA sin evidencia. Las decisiones aplazadas
quedan con criterio de reapertura escrito, de modo que aplazar no es enterrar.

**Negativas y costos aceptados.** Esta decisión **no mejora el recall de detección** y no debe
insinuarse que lo haga: ninguna de sus piezas habría encontrado ninguno de los defectos catalogados. El
ahorro de la reutilización por SHA exacto **no está cuantificado** en el canal local y su valor en el
canal de CI es esencialmente nulo, porque la métrica ya se cumple ahí. La suite completa se conserva en
tres puntos, con su coste íntegro. Y quedan **sin cerrar**, declaradamente: el determinismo de la suite
de UI —que tiene una condición de carrera viva sobre un delegado estático de proceso—, el trabajo
serializado real del hilo STA, y la duración del ciclo local obligatorio, que es el denominador de
cualquier ahorro que se reclame en el futuro.

**Qué vigilar.** Que la excepción de UI no se colapse por analogía sobre el núcleo. Que la reutilización
por SHA exacto no se lea como intercambio entre clases de evidencia. Que la métrica de duración activa
del dueño se retire si no se cumple, en vez de sobrevivir como obligación fantasma.

## Referencias

- [Contrato de I-45](../initiatives/I-45-test-validation-workflow.md) y
  [evidencia de Discovery](../initiatives/I-45-discovery.md), con marcadores epistemológicos por
  afirmación.
- [Plan consensuado V4](../initiatives/I-45-plan.md), con la secuencia de gates.
- [`AGENTS.md`](../../AGENTS.md), sección «Pruebas — definición de terminado».
- [`docs/WORKFLOW.md`](../WORKFLOW.md), secciones 4, 5 y 8.
- [`docs/guias/validacion-manual-autocad.md`](../guias/validacion-manual-autocad.md).
- [ADR-0017](0017-validacion-cargas-diferida-ram-elements.md), precedente de decisión de aplazamiento
  con criterio de reapertura.
