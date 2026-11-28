# I-64 F4B PB-T5 — Preguntas al Owner (v2.2)

> **Redacción para el repo público:** sin rutas locales ni nombres personales. La hora del Owner va en UTC; su zona horaria, como `<zona-horaria-del-Owner>`.

**Qué cambia en la v2.2** (la recomprobación 2): dos frases que decían de más, en Q-PBT5-8 y en Q-PBT5-1 (M-30). Nada más.

**Qué cambió en la v2.1** (la recomprobación de la v2): Q-PBT5-8 lleva la regla de reejecutar la CI y el porqué de la recomendada (M-23), y Q-PBT5-1 nombra los filtros exactos (M-24). Nada más.

**Qué cambió en la v2 respecto de la v1** (la revisión independiente de la v1, NO APTO, B 2 / I 6 / M 17):
- **Ninguna pregunta tiene un valor que se aplique sin tu respuesta.** Mientras no respondas, lo que dependa de la pregunta **no se hace**. Cada una dice qué queda parado.
- **Hay preguntas nuevas:** la emisión de PB-T5, la forma de su contrato, qué es un intento y las inyecciones de Debug.
- **La antigua Q-PBT5-3 pasa a información:**
  - «Restaurar estandar» (D-PB-11) es obligatorio en el Smoke-PB, porque el cierre de PB-TC se lo dejó;
  - lo demás de esa pregunta lo puede decidir la sesión.
- **La antigua Q-PBT5-6** (una revisión antes de firmar) es ahora una opción de la pregunta sobre la forma del contrato.
- **Son ocho preguntas en dos vueltas de cuatro.** La emisión y la forma van en la primera.
- **Cada pregunta se entiende sola,** sin haber leído el paquete.

**El contexto común, por si lees una sola:**
- PB-T5 es la última tarea de F4B, «Push Back completo». Tiene la CI exacta del producto, el Smoke-PB (la prueba en AutoCAD real) y el cierre de la fase.
- PB-T5 no cambia código.
- **Esta noche, con tu orden directa**, la sesión puede operar en AutoCAD una **corrida previa** del Smoke-PB, sobre copias de tus dibujos. Esa corrida **no** es el Smoke-PB contratado: tú decides después si la aceptas como parte de él.

---

## Vuelta 1

### Q-PBT5-7. ¿Se emite PB-T5?

**Lo que hay:**
- PB-T5 está en espera desde que autorizaste F4B.
- Cada tarea de F4B se emitió con una autorización tuya, o con una emisión condicional que diste para un grupo concreto de tareas: PB-T4a a PB-T4c, PB-T4d y PB-T4e. Ninguna cubre PB-T5.
- Tu mensaje de esta noche solo habla de ejecutar el smoke, no de emitir la tarea.

**Las opciones:**
- **A. Sí, con condiciones (Recomendado).** Se emite sin volver a ti si se cumplen tres condiciones:
  - la comprobación acotada de la v2 del paquete da 0 bloqueantes y 0 importantes sin resolver;
  - no aparece ninguna decisión material nueva;
  - el Candidato tiene la CI exacta en verde.

  Vuelve a ti por una decisión nueva, por una prueba que falle o por un intento 3.
- **B. Sí, pero después de leer yo la v2 y lo de esta noche.** No se emite hasta que lo digas.
- **C. Todavía no.** PB-T5 sigue en espera. La corrida previa, si se hizo, queda solo como registro.

**Mientras no respondas:** PB-T5 no se emite. Esta noche solo puede hacerse la corrida previa.

### Q-PBT5-8. La forma del contrato de PB-T5 y cómo se cierra

**Lo que hay:**
- El paquete de F4B pide a cada tarea la cadena de F4:
  - paquete con revisión independiente;
  - contrato;
  - planificación del Controller;
  - Worker;
  - CI exacta;
  - verificación;
  - revisiones directa e independiente;
  - los controles nc1 a nc3;
  - cierre por la regla D2.
- Solo deja adaptarla a PB-T0, para su medición en AutoCAD.
- **PB-T5 no tiene Worker de código.** La planificación, la verificación y los controles comprueban la entrega de un Worker, y aquí no hay entrega.
- **Hay un precedente de F4:** Smoke-2 no tuvo esa cadena. Lo rigieron tus decisiones y la guía, y el cierre de F4 lo autorizó tu firma.
- **Una contradicción:** cuando elegiste la v3 del sustituto de nc2 (F4B-E58), su descripción decía que se aplicaría «a este nc2 y a PB-T5».
- El cierre de F4B exige tu firma en cualquier caso.
- **La regla de reejecutar la CI va con esta respuesta.** Si la CI del Candidato sale en rojo **solo** por el detector de cuelgue (`blame-hang`), en una prueba ya conocida como lenta, se reejecutan una vez los trabajos fallidos sobre el mismo SHA. Otra causa, o un segundo rojo, vuelve a ti. Con A, B o C la confirmas; con D, la escribes tú.

**Las opciones:**
- **A. Adaptación, y firmo sobre la evidencia.** Tiene contrato propio (invariantes, paradas, evidencia y presupuesto), sin la planificación del Controller, sin nc1 a nc4 y sin D2. El nc2 con la v3 no se usa, porque no hay entrega. Cierra tu firma, como el cierre de F4. Es lo que se hizo en Smoke-2.
- **B. Adaptación, con una revisión independiente de la evidencia antes de tu firma (Recomendado).** Igual que A, pero un revisor aparte comprueba, en solo lectura, que el registro, las capturas y los inventarios dicen lo mismo. De la cadena de F4 se conserva lo que sirve sin Worker: la revisión del paquete, la CI exacta y una revisión independiente. Añade una o dos horas.
  - **Por qué B y no A,** que es el precedente: en Smoke-2 viste en directo los pasos 0 a 29 de 88, y esta vez puede que no veas ninguno. Además, en Smoke-2 el paso 50 fue un falso FAIL de la sesión, por un clic fuera del botón, que solo se aclaró al revisar las capturas. Una revisión aparte busca ese tipo de error antes de que firmes.
- **C. La cadena de F4 completa.** El contrato va en el formato habitual, el Controller verifica la evidencia del smoke y corren nc1 a nc3, con la v3 en nc2, como describía tu elección de F4B-E58. Es la forma que pide la letra del paquete, pero los controles se diseñaron para comprobar la entrega de un Worker.
- **D. Otra forma:** la escribes tú.

**Mientras no respondas:** no hay contrato emitido.

### Q-PBT5-9. Qué es un intento, las reglas de Smoke-2 y la corrida previa de esta noche

**Lo que hay:**
- Tu regla de F4B es que cada tarea empieza en 1 de 3 y que el intento 3 vuelve siempre a ti. No dice qué es un intento cuando no hay Worker.
- En Smoke-2 fijaste tres reglas que F4B no hereda sin tu orden:
  - las etiquetas (NOT AVAILABLE FOR PHYSICAL SMOKE; NO VERIFICABLE FÍSICAMENTE / CUBIERTO POR PRUEBA AUTOMATIZADA; OBSERVADO);
  - el protocolo OBSERVADO: traer la evidencia antes de decidir;
  - una copia de trabajo por escenario.
- Una evidencia en AutoCAD se reutiliza solo con el mismo SHA del Candidato, la misma versión de AutoCAD y la misma biblioteca de bloques (la guía, §7, y AGENTS).

**Las opciones:**
- **A. La definición propuesta, las reglas de Smoke-2 y la corrida previa como parte del intento 1 (Recomendado).**
  - Un intento es una corrida del Smoke-PB sobre un Candidato.
  - No cuentan: repetir un paso por error del operador (dos veces como máximo), una parada del entorno antes de abrir AutoCAD, ni una adenda que pidas al revisar.
  - Se adoptan las tres reglas de Smoke-2.
  - La corrida previa de esta noche cuenta como parte del intento 1, si cumple la regla de reutilización y la aceptas paso a paso.
- **B. La misma definición y las mismas reglas, pero la corrida previa solo como exploración.** El Smoke-PB contratado repite todos sus pasos después de la emisión.
- **C. Otra definición:** la escribes tú.

**Mientras no respondas:** la corrida previa queda registrada, pero no cuenta para nada.

### Q-PBT5-5. Confirmar cómo se leyó tu orden de esta noche

**Tu mensaje**, literal, con la zona horaria redactada:

> «En caso de que se complete antes de las 6 am y quieras ejecutar el smoke, ejecutalo tú ya que estaré dormido, a menos que sean después de las 7 am de `<zona-horaria-del-Owner>`»

**Cómo lo aplicó la sesión principal esta noche** (es su lectura, no una opción elegida por ti):
- la sesión opera la corrida previa **solo si está lista para empezar antes de las 13:00 UTC** (las 7:00 de tu hora);
- **a las 13:00 UTC para** en el siguiente límite de paso y te espera, aunque no hayas escrito;
- **la franja de 12:00 a 13:00 UTC** (de 6:00 a 7:00) se trató como «antes de las 7:00». Tu mensaje pone como condición que «se complete antes de las 6 am», que son las 12:00 UTC; la sesión lo leyó así, sin preguntarte;
- la corrida previa no emite PB-T5, no hace los juicios visuales que te tocan y no firma nada.

**Las opciones:**
- **A. Confirmo esa lectura (Recomendado).**
- **B. Corrijo la franja de 6:00 a 7:00:** quería decir que la sesión solo empezara si estaba lista antes de las 6:00.
- **C. Otra corrección:** la escribes tú.

**Mientras no respondas:** lo de esta noche queda registrado con esa lectura, marcada como no confirmada.

---

## Vuelta 2

### Q-PBT5-1. La suite completa de UI en local, para el Candidato

**Lo que hay:**
- **La guía de validación manual y AGENTS** dicen que el DLL de una validación del dueño sale de un Candidato, y que un Candidato necesita la suite completa de Core y la de UI en local sobre su SHA exacto, además de la CI exacta.
- **Tu regla de F4-E59** dice: «en tareas con contención de perfil, nunca suites completas de UI en local; solo en la CI».
  - La razón: las pruebas de UI existentes reescriben un archivo de tu perfil, la disposición guardada de la ventana del configurador de cabeceras (`RackFrameConfigurator.layout`).
  - Ese problema sigue abierto, y todos los contratos de F4B llevan la regla.
- **En F4, para Smoke-2,** aceptaste una dispensa: la CI exacta y los filtros locales, sin la UI completa en local.
- **El paquete de F4B** dice que las reglas de F4 no pasan a F4B sin tu orden.
- **Lo que se hará esta noche, en cualquier caso:** la suite completa de Core en local, los 19 filtros de UI de los `RequiredTests` del contrato de PB-T4e (cada uno en su proceso) y la CI exacta.
  - Esos 19 ya corrieron en local en la precondición 6 de PB-T4e, y ninguno nombra las clases conocidas que reescriben el archivo de tu perfil. Esa lista no es completa, así que el archivo se mide antes y después.
  - El Candidato queda «pendiente de Q-PBT5-1». La UI completa puede correr después sobre el mismo SHA.

**Las opciones:**
- **A. Levantar la regla de F4-E59 solo para este Candidato.** La UI completa corre en local. Antes se copia el archivo de tu perfil y después se restaura byte a byte (su fecha cambia). El Candidato cumple la guía al pie de la letra.
- **B. Extender a F4B la dispensa de Smoke-2 (Recomendado).** CI exacta y filtros locales, sin la UI completa en local. No se levanta ninguna regla, y es lo que ya hiciste en F4. El Smoke-PB no es la validación final de la iniciativa, que sí la exigirá.
- **C. Aislar antes el archivo del configurador** con una tarea de código propia, y después correr la UI completa en local. Es lo más limpio, pero añade una tarea antes del cierre de F4B.

**Mientras no respondas:** el Candidato queda «pendiente de Q-PBT5-1», y PB-T5 no se cierra.

### Q-PBT5-10. Las inyecciones de Debug en el borrado y en el sellado

**Lo que hay:**
- El producto tiene variables de depuración que provocan un fallo a propósito en la escritura: en una unidad, o justo antes o después de sellar.
- **La tarea del borrado de vistas (PB-T3c)** hizo que la unidad que falla cuente también los borrados. Ese camino es nuevo y nadie lo ha probado en AutoCAD. El paquete de PB-T3 lo dejó para el Smoke-PB.
- **En Smoke-2 se hicieron tres:**
  - un AutoCAD por inyección;
  - la variable solo en el entorno de ese proceso, nunca en el usuario, la máquina ni la consola;
  - comprobación antes y después;
  - y las ratificaste PASS.
- **Choque con el paquete de F4B,** que dice «No se finge una condición interna». La variable no finge nada que AutoCAD muestre, pero provoca un fallo que no ocurriría solo.

**Las opciones:**
- **A. Hacerlas con el método de Smoke-2 (Recomendado).** Son tres corridas: el fallo en la segunda unidad, que ahora puede ser un borrado; antes del sellado; y después del sellado. Van sobre copias, con un AutoCAD por inyección. Nada cambia el producto.
- **B. No hacerlas.** Quedan como NO VERIFICABLE FÍSICAMENTE, cubiertas por las pruebas automáticas.

**Mientras no respondas:** no se hace ninguna inyección.

### Q-PBT5-4. Los casos que hay que preparar con órdenes de AutoCAD

**Lo que hay:**
- Para algunas pruebas de borrado, el dibujo tiene que estar en un estado que el producto no produce por sí solo:
  - **(1)** una referencia del corte lateral en una presentación (espacio papel);
  - **(2)** una referencia del corte dentro de un bloque tuyo;
  - **(3)** la capa del corte bloqueada, para el aviso `LOCKED_LAYER`;
  - **(4)** el corte como única vista del rack (con ERASE y PURGE de las demás), para el rechazo del último vínculo.
- **Ningún dibujo real conocido trae ninguno de los cuatro.** La única referencia anidada que medimos en tus dibujos es de una planta, que no se borra.
- **La línea entre preparar y construir:**
  - insertar una vista con RACKEDITAR («Insertar lateral», «Insertar vista actual») es un gesto del producto, y se usa como preparación;
  - poner el dibujo con órdenes nativas en un estado que el producto no crea es construir.
- **Tu decisión sobre el Smoke-PB** solo autoriza fabricar racks A/B. En Smoke-2 pediste no fabricar casos que no existían.
- Cuando el panel borra un corte que sobra, borra todas sus referencias, también las de (1) y (2), sin avisar, igual que RACKEDITAR. La revisión de esa tarea lo dejó para que se mirara en AutoCAD.

**Las opciones:**
- **A. Construirlos en copias, con órdenes nativas, y rotularlos (Recomendado).** Los cuatro, con `-INSERT`, `-BLOCK`, `-LAYER` y `ERASE` con `PURGE` tecleados. Cada uno se compara con RACKEDITAR en otra copia, y va rotulado CONSTRUCTED REPRESENTATIVE TEST CASES, como los A/B. Amplía lo que autorizaste en el Smoke-PB. Los bloques dinámicos solo se miran si un dibujo real los trae.
- **B. Solo lo que traigan los dibujos reales.** Lo demás queda NOT AVAILABLE FOR PHYSICAL SMOKE.
- **C. Fuera del Smoke-PB.** Quedan como riesgo informado, sin mirarlos en AutoCAD.

**Mientras no respondas:** no se construye nada.

### Q-PBT5-2. El estilo de cotas guardado, y el criterio de cierre que depende de él

**Lo que hay:**
- **El cierre de F4B** pide que el Smoke-PB verifique en RACKEDITAR las correcciones D-PB-06 y D-PB-08.
- **Una mitad de D-PB-06** es que RACKEDITAR conserve el estilo de cotas guardado en el rack. Solo se ve con un rack que lo guarde y un dibujo que tenga ese estilo.
- **En tus dos dibujos medidos,** 0 de 22 racks Push Back lo guardan, y ningún botón de la ventana lo escribe. Fabricarlo exige escribir a mano el sobre de un rack con un guion: es un arnés, no un gesto del producto.
- **Hay otros dibujos reales** con racks Push Back, de la selección de Smoke-2, donde nadie ha contado el estilo.
- **Si no se ve en AutoCAD,** ese criterio de cierre se cumple solo en parte, y eso lo tienes que aceptar tú.

**Las opciones:**
- **A. NOT AVAILABLE, y acepto el criterio cumplido en parte.** Se registra el recuento (0), y la corrección queda cubierta por las pruebas automáticas.
- **B. Fabricarlo en una copia.** La sesión escribe el estilo en el sobre de un rack de una copia, con un guion, y comprueba RACKEDITAR → «Actualizar». Prueba la corrección en AutoCAD, pero sobre un dato que el producto no puede producir.
- **C. Contar antes en los demás dibujos reales (Recomendado).** La sesión cuenta, en copias y en solo lectura, los racks con estilo guardado en los demás dibujos de la selección de Smoke-2. Si alguno lo tiene, se prueba con él. Si ninguno, se vuelve a A y lo aceptas entonces.

**Mientras no respondas:** el paso del estilo queda pendiente, y el criterio de cierre no se da por cumplido.

---

## Información, no decisión

### «Restaurar estandar» (D-PB-11) y las otras correcciones de RACKEDITAR

**D-PB-11 es obligatorio en el Smoke-PB:** el cierre de la tarea que lo corrigió dejó a esta prueba «la geometría del reinicio de D-PB-11 y el aspecto del aviso».
- **Lo que se mira:** con «Restaurar estandar» (así, sin tilde, en el botón), «Actualizar» escribe lo restaurado.
- Se comprueba en los datos del rack y en cada vista. Tu juicio visual queda pendiente de que revises las capturas.

**Las otras dos correcciones las añade la sesión como cobertura,** sobre copias y con gestos del producto:
- seis cajas que conservan el valor exacto al pasar por ellas (H-31);
- la caja mixta de «Ambos lados», que iguala los dos lados (OWNER-1 de PB-TC2).

Si no las quieres en el Smoke-PB, dilo y sus resultados quedan solo como información.

### R-T4-39 (F4B-E60)

F4B-E60 manda contártelo en PB-T5, junto al paso 6 de la recarga de «Editar todo». Ninguna de tus respuestas de PB-T4e dependía de esto.
- **El hecho:** con un borrador modificado y un cambio de fuera que reescriba el sobre de origen, «Actualizar» puede responder `NotAppliedPreflight` (no se aplica) en lugar de ofrecer «Conservar mi borrador».
- **Lo que sigue en pie:** no se escribe nada sin avisar.
- **Lo que hace el Smoke-PB:** registra cuál de las dos respuestas sale en tu dibujo real. Si ningún cambio de fuera disponible lleva a «Conservar mi borrador», sus pasos quedan NOT AVAILABLE y te lo cuento con la evidencia.
