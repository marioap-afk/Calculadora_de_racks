# Decisiones del Owner sobre PB-T5 (I-64 F4B): la emisión, la forma del contrato, el intento y el Smoke-PB

**Fecha:** 2026-11-23. **Canal:** el chat de la sesión principal, con el diálogo de preguntas de la aplicación, en dos vueltas (la primera, preguntada a las 2026-11-23T07:25:41.287Z y respondida a las 2026-11-23T09:19:40.506Z, junto con la de nc1 de PB-T4e; la segunda, preguntada a las 2026-11-23T09:20:24.006Z y respondida a las 2026-11-23T09:21:13.942Z). Las preguntas y las respuestas son literales. La numeración F4B-E se asigna al custodiarlo.

**El origen:** el paquete v2.2 de PB-T5 y sus preguntas (`owner-questions-v2.2.md`), con su revisión independiente y sus recomprobaciones. La sesión condensó cada pregunta para que se entienda sola en el diálogo. Q-PBT5-5 (la lectura de la orden de esa noche) no se preguntó: la corrida previa no llegó a hacerse, porque PB-T4e no cerró a tiempo (nc1 volvió al Owner). Q-PBT5-9 dice ese hecho.

## 1. Pregunta (encabezado «Emitir T5») (Q-PBT5-7)

**Pregunta, literal:**

> ¿Se emite PB-T5? Es la última tarea de F4B (CI exacta, Smoke-PB en AutoCAD real y cierre de la fase) y no cambia código. Está en espera desde que autorizaste F4B. Cada tarea se emitió con una autorización tuya o con una emisión condicional para un grupo concreto (PB-T4a a PB-T4e), y ninguna cubre PB-T5; tu mensaje de anoche hablaba de ejecutar el smoke, no de emitir. Su paquete (v2.2) ya pasó una revisión independiente y sus recomprobaciones.

**Opciones:**
1. **«Sí, con condiciones (Recomendado)»:** Se emite sin volver a ti si la comprobación acotada del paquete da 0 bloqueantes y 0 importantes sin resolver, no aparece ninguna decisión material nueva y el Candidato tiene la CI exacta en verde. Vuelve por una decisión nueva, una prueba que falle o un intento 3.
2. **«Después de leerlo yo»:** No se emite hasta que leas la v2 del paquete y lo digas.
3. **«Todavía no»:** PB-T5 sigue en espera.

**Respuesta del Owner, literal:** «Sí, con condiciones (Recomendado)».

## 2. Pregunta (encabezado «Forma T5») (Q-PBT5-8)

**Pregunta, literal:**

> ¿Qué forma tiene el contrato de PB-T5 y cómo se cierra? PB-T5 no tiene Worker de código, y la cadena de F4 (planificación del Controller, verificación, controles nc1 a nc3, cierre D2) comprueba la entrega de un Worker. En F4, el Smoke-2 no tuvo esa cadena: lo rigieron tus decisiones y la guía, y cerró tu firma. Con cualquiera de las tres confirmas además la regla de la CI: si sale en rojo solo por el detector de cuelgue en una prueba lenta conocida, se reejecutan una vez los trabajos fallidos sobre el mismo SHA; otra causa, o un segundo rojo, te vuelve.

**Opciones:**
1. **«Adaptación con revisión (Recomendado)»:** Contrato propio (invariantes, paradas, evidencia, presupuesto), sin Controller, sin nc1 a nc4 y sin D2. Antes de tu firma, un revisor aparte comprueba en solo lectura que registro, capturas e inventarios coinciden. Añade 1–2 h. En Smoke-2 un falso FAIL solo se aclaró revisando las capturas.
2. **«Adaptación, firmo directo»:** El mismo contrato propio, sin la revisión aparte; cierras con tu firma sobre la evidencia. Es lo que se hizo en Smoke-2.
3. **«La cadena de F4 completa»:** Contrato habitual: el Controller verifica la evidencia del smoke y corren nc1 a nc3 (nc2 con la v3). Es la letra del paquete, pero los controles están pensados para la entrega de un Worker.

**Respuesta del Owner, literal:** «Adaptación con revisión (Recomendado)».

## 3. Pregunta (encabezado «Intento T5») (Q-PBT5-9)

**Pregunta, literal:**

> En PB-T5, ¿qué es un intento y qué reglas de Smoke-2 se adoptan? Tu regla es 1 de 3 por tarea, y el intento 3 te vuelve siempre, pero no dice qué es un intento sin Worker. En Smoke-2 fijaste tres reglas que F4B no hereda sin tu orden: las etiquetas (NOT AVAILABLE FOR PHYSICAL SMOKE; NO VERIFICABLE FÍSICAMENTE / CUBIERTO POR PRUEBA AUTOMATIZADA; OBSERVADO), traer la evidencia antes de decidir un OBSERVADO y una copia de trabajo por escenario. Anoche no se hizo ninguna corrida previa.

**Opciones:**
1. **«Definición y reglas (Recomendado)»:** Un intento es una corrida del Smoke-PB sobre un Candidato. No cuentan repetir un paso por error del operador (dos veces como máximo), una parada del entorno antes de abrir AutoCAD ni una adenda que pidas al revisar. Se adoptan las tres reglas de Smoke-2.
2. **«Lo decido con el paquete»:** No lo fijo ahora: lo decido cuando lea la v2 del paquete.

**Respuesta del Owner, literal:** «Definición y reglas (Recomendado)».

## 4. Pregunta (encabezado «UI local») (Q-PBT5-1)

**Pregunta, literal:**

> PB-T5: ¿se corre la suite completa de UI en local para el Candidato del Smoke-PB? La guía y AGENTS piden la suite completa de Core y de UI en local sobre el SHA exacto. Tu regla de F4-E59 dice: «en tareas con contención de perfil, nunca suites completas de UI en local; solo en la CI», porque algunas pruebas de UI reescriben la disposición guardada del configurador de cabeceras en tu perfil, y eso sigue abierto. En F4, para Smoke-2, aceptaste una dispensa: la CI exacta y los filtros locales, sin la UI completa. En cualquier caso correrán la suite completa de Core en local, los 19 filtros de UI del contrato de PB-T4e (cada uno en su proceso) y la CI exacta.

**Opciones:**
1. **«Extender la dispensa (Recomendado)»:** Como en Smoke-2: CI exacta y filtros locales, sin la UI completa en local. No se levanta ninguna regla. La validación final de la iniciativa sí la exigirá.
2. **«Levantar la regla esta vez»:** La UI completa corre en local para este Candidato. Antes se copia el archivo de tu perfil y después se restaura byte a byte (su fecha cambia).
3. **«Aislar antes el archivo»:** Una tarea de código propia que aísle la disposición del configurador, y después la UI completa en local. Es lo más limpio, pero añade una tarea antes del cierre de F4B.

**Respuesta del Owner, literal:** «Extender la dispensa (Recomendado)».

## 5. Pregunta (encabezado «Inyecciones») (Q-PBT5-10)

**Pregunta, literal:**

> PB-T5: ¿se hacen en el Smoke-PB las inyecciones de fallo de Debug en el borrado y en el sellado? El producto tiene variables de depuración que provocan a propósito un fallo en la escritura: en una unidad, o antes o después de sellar. La tarea del borrado de vistas (PB-T3c) hizo que la unidad que falla cuente también los borrados; ese camino es nuevo, nadie lo ha probado en AutoCAD, y el paquete de PB-T3 lo dejó para el Smoke-PB. En Smoke-2 se hicieron tres, con un AutoCAD por inyección y la variable solo en el entorno de ese proceso, y las ratificaste PASS.

**Opciones:**
1. **«Hacerlas como en Smoke-2 (Recomendado)»:** Tres corridas sobre copias, un AutoCAD por inyección: el fallo en la segunda unidad (que ahora puede ser un borrado), antes del sellado y después del sellado. La variable solo en ese proceso. Nada cambia el producto.
2. **«No hacerlas»:** Quedan como NO VERIFICABLE FÍSICAMENTE, cubiertas por las pruebas automáticas.

**Respuesta del Owner, literal:** «Hacerlas como en Smoke-2 (Recomendado)».

## 6. Pregunta (encabezado «Construidos») (Q-PBT5-4)

**Pregunta, literal:**

> PB-T5: para algunas pruebas de borrado de vistas, el dibujo tiene que estar en un estado que el producto no produce solo: (1) una referencia del corte lateral en una presentación (espacio papel); (2) una referencia del corte dentro de un bloque tuyo; (3) la capa del corte bloqueada, para el aviso LOCKED_LAYER; (4) el corte como única vista del rack, para el rechazo del último vínculo. Ningún dibujo real conocido trae ninguno. Tu decisión sobre el Smoke-PB solo autoriza fabricar racks A/B, y en Smoke-2 pediste no fabricar casos que no existían. Al borrar un corte sobrante, el panel borra todas sus referencias, también las de (1) y (2), sin avisar, igual que RACKEDITAR. ¿Qué hago?

**Opciones:**
1. **«Construirlos y rotularlos (Recomendado)»:** Los cuatro, en copias, con órdenes nativas tecleadas (-INSERT, -BLOCK, -LAYER, ERASE y PURGE). Cada uno se compara con RACKEDITAR en otra copia y va rotulado CONSTRUCTED REPRESENTATIVE TEST CASES, como los A/B. Amplía lo que autorizaste.
2. **«Solo lo que traigan los reales»:** Lo demás queda NOT AVAILABLE FOR PHYSICAL SMOKE.
3. **«Fuera del Smoke-PB»:** Quedan como riesgo informado, sin mirarlos en AutoCAD.

**Respuesta del Owner, literal:** «Construirlos y rotularlos (Recomendado)».

## 7. Pregunta (encabezado «Estilo cotas») (Q-PBT5-2)

**Pregunta, literal:**

> PB-T5: el cierre de F4B pide verificar en RACKEDITAR la corrección D-PB-06, y una mitad es que RACKEDITAR conserve el estilo de cotas guardado en el rack. Solo se ve con un rack que lo guarde. En tus dos dibujos medidos, 0 de 22 racks Push Back lo guardan, y ningún botón de la ventana lo escribe; fabricarlo exige escribir a mano el sobre de un rack con un guion. Hay otros dibujos reales con racks Push Back, de la selección de Smoke-2, donde nadie ha contado el estilo. Si no se ve en AutoCAD, ese criterio de cierre se cumple solo en parte, y eso lo aceptas tú.

**Opciones:**
1. **«Contar antes en otros (Recomendado)»:** La sesión cuenta, en copias y en solo lectura, los racks con estilo guardado en los demás dibujos de la selección de Smoke-2. Si alguno lo tiene, se prueba con él; si ninguno, te vuelve para aceptar el criterio cumplido en parte.
2. **«NOT AVAILABLE ya»:** Se registra el recuento (0) y aceptas el criterio cumplido en parte; la corrección queda cubierta por las pruebas automáticas.
3. **«Fabricarlo en una copia»:** La sesión escribe el estilo en el sobre de un rack de una copia con un guion y comprueba RACKEDITAR y «Actualizar», sobre un dato que el producto no produce.

**Respuesta del Owner, literal:** «Contar antes en otros (Recomendado)».

## Lo que queda decidido

- **Q-PBT5-7 (Emitir T5):** «Sí, con condiciones (Recomendado)».
- **Q-PBT5-8 (Forma T5):** «Adaptación con revisión (Recomendado)».
- **Q-PBT5-9 (Intento T5):** «Definición y reglas (Recomendado)».
- **Q-PBT5-1 (UI local):** «Extender la dispensa (Recomendado)».
- **Q-PBT5-10 (Inyecciones):** «Hacerlas como en Smoke-2 (Recomendado)».
- **Q-PBT5-4 (Construidos):** «Construirlos y rotularlos (Recomendado)».
- **Q-PBT5-2 (Estilo cotas):** «Contar antes en otros (Recomendado)».
