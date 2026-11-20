# Instrucciones de la sesión para el paquete v2 de PB-T4

La revisión independiente de la v1 (`independent-review-v1.md`, en esta carpeta) da **NECESITA v2, B/I/M = 3 / 11 / 15**. Escribe la v2 en `I-64-f4b-pbt4-gate-contract-package-v2.md`. La v1 no se toca.

## Reglas

Son las del encargo original (`drafting-brief.md`):
- solo lectura por objetos git en `7dfc7c21`;
- NADA en el worktree;
- no compilar;
- Python con `-I` y en guiones escritos con Write;
- escribe solo en `hs65\pbt4\`.

PB-T3c sigue en ejecución. Lo que dependa de su GREEN lleva el marcador `<tras el cierre de PB-T3c>`.

## Lo que decide la sesión (aplícalo)

1. **La subtarea previa, solo aditiva** (§6 de la revisión). Divide PB-T4 en tres subtareas secuenciales, cada una con su 1/3:
   - **PB-T4a (aditiva):** el lector del Plugin lee también el modelo completo, con el escaneo, el catálogo, los sobres de los miembros y el testigo. La relectura admite el origen (A-T3-8: `ReadIntent(…, long? chosenReference, …)`). El resultado de la lectura lleva lo completo junto al piloto. El tipo del borrador del panel no cambia, y no se sustituye ninguna prueba salvo las que el cambio aditivo rompa por letra (mídelas).
   - **PB-T4b:** el cambio de tipo del borrador del panel, OB-03.2, la validación única y el nuevo «Conservar mi borrador». Las sustituciones van con registro.
   - **PB-T4c:** «Editar todo».

   Si al medir ves que la división no se sostiene, dilo con los hechos. No la cambies por tu cuenta.
2. **B-02, la regla única de la escala:** tiene que vivir donde la puedan probar las suites. Application o UI, no el Plugin. El Plugin solo la conecta. Con cualquier respuesta a Q1, la segunda copia del adaptador del piloto o desaparece o queda declarada con su coste en Q1.
3. **I-02:** en PB-T4c, reutiliza `CommitField` como frontera en lugar de `PilotBoundaryKind.EditAll`, si así se evita la sustitución de `WorkspacePilotUpdateRequestTests:1107-1109`. Si no se puede, explica por qué. Mide y cita todas las guardas que rompen los miembros nuevos del puerto (`WorkspacePilotEditGuardTests:156-163`, `:280-281` y `:380-385`). Para cada una: o la evitas por diseño, o va a la lista de sustituciones.
4. **Lo que el paquete declara, sin preguntarlo** (§8 de la revisión):
   - que pedir «Conservar» pasará a leer el dibujo, frente a la regla de F4 «calls nothing of the port but the memory» (UI :780-793). Lleva su registro declarado;
   - el procedimiento para medir la lista de sustituciones (punto 5).
5. **B-03, I-03 e I-04: la lista de sustituciones.** La lista que verá el Owner tiene que ser exacta, por clase y por causa. Diseña el procedimiento con dos condiciones:
   - **(a) cerrado y verificable antes de emitir.** Una prueba se sustituye solo si falla por una causa de una lista cerrada de causas del diseño. Cada causa lleva su firma comprobable: el mensaje, la línea o la regla. Una `[Theory]` no se salta entera si alguna fila sigue pasando: hay que decir qué se hace con esas filas. Cada equivalente cubre todo lo que cubría lo retirado (OB-T1B.2). Los equivalentes que sean autopruebas no pueden pasar en el RED sin una excepción declarada, porque si no, salta C04;
   - **(b) que no fije la lista contra una implementación que el Worker no recibe.** La revisión señala que un «GREEN de referencia» de la sesión, sin entregar, haría que cualquier diferencia del Worker diera C15 o C16. Propón al menos dos opciones con sus pros y contras. Por ejemplo:
     - un esqueleto de la API pública ya con el tipo nuevo, que se compila y se ejecuta para medir la lista (lo que falla con cuerpos inertes frente a lo que solo falla con el comportamiento);
     - o una medición en dos tiempos, sobre el esqueleto antes de emitir y sobre el GREEN del Worker, con una parada solo por pruebas fuera de las causas cerradas.

     La sesión elegirá, o lo llevará al Owner si es material.
6. **Las preguntas al Owner:** usa los textos de la §8 de la revisión como base.
   - **Q1:** con huecos `<N>`, `<n1>`, … para los números medidos, y con el coste de conservar el piloto (A-5, R-T1-17, D-1 con su premisa y la regla duplicada). La opción (B) tiene que ser realista: se retira lo que solo usaba el panel y se conserva el lector del piloto.
   - **Q2:** con «Por grupos», «Por campos» y «Por campos, con sus valores». Sin «Solo recuentos» y sin «no abre diálogos». La recomendada va primero: elige con criterio y justifícalo.
   - **Q3:** con la condición «la lista medida al emitir es la que viste, sin ninguna prueba añadida», para cada una de las tres subtareas.

   Las preguntas se hacen después de la medición de la lista, que hará la sesión sobre el cierre de PB-T3c. Escribe en el §8 el orden: v2 → comprobación acotada → cierre de PB-T3c → medición → pregunta al Owner → emisión.
7. **Todos los demás hallazgos,** del B-01 al M-15: resuélvelos en la v2 o explica por qué no, uno a uno. Incluye I-01 (K = 1: quién pone el modal y qué hacen `SetPendingText`, `CommitField`, `CrossBoundary` y `DiscardRack`), I-06 (D2, condiciones 6 y 7), I-07 (el `NaN` en `RackModuleCommit.SameAs`), I-08 (los literales de la presentación, que dependen de Q2: déjalos parametrizados por la respuesta), I-09 (la lista de ayudantes cerrada y la disciplina de lectura) e I-10 (el Smoke-PB con todo lo que solo existe en el Plugin).

## Lo que entregas

- **`I-64-f4b-pbt4-gate-contract-package-v2.md`.** Al principio lleva una tabla V2-xx que asocia cada hallazgo de la revisión con su cambio y la sección donde está.
- **`pbt4-measurements.md` actualizado,** con las mediciones nuevas y su fuente.
- **Los guiones nuevos y sus salidas.**

**Responde con:**
- la división final;
- las CT por subtarea;
- el procedimiento propuesto para la lista, con sus opciones;
- las preguntas al Owner (texto con huecos);
- los hallazgos que no resuelvas, con su motivo.
