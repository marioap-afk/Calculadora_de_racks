# Decisiones del Owner sobre PB-T4d (I-64 F4B): la rama muerta de BuildCore y las 27 sustitutas

**Fecha:** 2026-11-22. Después de `owner-pbt4d-run-coverage-decision.md`, que dejaba en 21 las sustitutas de WPUR.

**Por qué se preguntó:** al preparar las sustitutas, el autor del paquete leyó la corrida genérica y la cobertura de `/d/r6j` y `/d/r6k`. El análisis está en `v52/sub/cov-sides.txt` y `v52/sub/cov-otras.txt`, y sus guiones al lado. Encontró dos cosas:
1. **Código muerto tras la retirada.**
   - Después del GREEN de B, a `BuildCore` (`PushBackPilotUpdatePlan.cs`) solo lo llama `BuildFull`, con `allowErase: true`.
   - La rama verdadera de `if (disposition != Redraw)` y la línea `ViewUnavailable` (UpdatePlan:600-601), y las demás ramas que solo existen con `allowErase` falso, ya no se alcanzan, así que ninguna prueba las puede cubrir.
   - La sesión le había dicho al Owner que el alcance B «no deja código muerto».
2. **Siete ramas más que solo cubren pruebas de WPUR que no están entre las 21.**
   - El primer cálculo prueba a prueba contaba cuántas ramas cubría cada prueba, no cuáles. Rama por rama, 7 ramas verdaderas de la corrida que se queda solo las cubren las pruebas 06, 07, 16, 19, 29 y 42 de WPUR:
     - UpdateRun:422, 650 y 651, el origen en la corrida;
     - UpdateRun:460, el `RejectionCode` al empezar MUTATE;
     - UpdateRun:510, el documento que cambió entre PREPARE y MUTATE;
     - UpdateRun:589, un ámbito ajeno abierto en POST;
     - UpdateRun:604, la comparación de POST que da una causa.
   - El resto de la suite solo cubre la rama falsa.

La sesión preguntó con AskUserQuestion, en una sola vuelta de dos preguntas. Este registro copia el texto exacto. La numeración F4B-E se asigna al custodiarlo.

## 1. Pregunta (encabezado «Rama muerta»)

**Pregunta, literal:**

> PB-T4d: al preparar las sustitutas salieron dos cosas más. La primera: te dije que el alcance B «no deja código muerto», pero al quitar el piloto quedan ramas de la preparación de la escritura que solo usaba el piloto (el modo que no borra vistas, en BuildCore). Ya no se pueden alcanzar, así que ninguna prueba las puede cubrir. Quitarlas toca algo más de código del que se midió en B. ¿Qué hago?

**Opciones:**
1. **Quitarla en PB-T4d (Recomendado):** El GREEN quita también esas ramas (el parámetro allowErase de BuildCore y lo que solo existe con él), como pide B. Se vuelve a medir en la precondición 6; si eso hace caer algo fuera de la lista, te vuelve.
2. **Declararla y dejarla:** Se declaran inalcanzables tras la retirada y se excluyen del criterio de cobertura con esa razón. Su limpieza va a una tarea propia después. Queda ese código muerto hasta entonces.

**Respuesta del Owner, literal:** «Quitarla en PB-T4d (Recomendado)».

## 2. Pregunta (encabezado «27 en vez de 21»)

**Pregunta, literal:**

> PB-T4d: la segunda: elegiste sustituir las 21 pruebas de la corrida genérica para que no quedara sin prueba ningún camino que se queda. Al mirar rama por rama (no solo cuántas ramas cubre cada prueba), hay 7 ramas más que solo cubren otras 6 pruebas de esas 48 (el origen ninguno o varios, el rechazo al empezar a escribir, el documento que cambió en medio, un ámbito ajeno abierto y la comparación final que da una causa). Para cumplir lo que pediste hacen falta 27 sustitutas en vez de 21, y se borran 22 en vez de 27. ¿Lo hago?

**Opciones:**
1. **Sí, 27 (Recomendado):** Se sustituyen las 21 más esas 6, y se borran 22. La precondición 6 mide la cobertura rama por rama con la retirada aplicada, y si algo queda sin cubrir te vuelve.
2. **Solo las 21:** Se quedan las 21 que elegiste; esas 7 ramas quedan sin prueba y se declaran como pérdida en el registro.

**Respuesta del Owner, literal:** «Sí, 27 (Recomendado)».

## Lo que queda decidido

- **El GREEN de PB-T4d quita también las ramas muertas de `BuildCore`:** el parámetro `allowErase` y lo que solo existe con él.
  - La precondición 6 lo mide otra vez.
  - Si eso hace caer algo fuera de la lista, vuelve al Owner.
- **Las sustitutas de WPUR pasan a 27:** las 21 de `owner-pbt4d-run-coverage-decision.md` más la 06, la 07, la 16, la 19, la 29 y la 42.
  - Las otras 22 se borran con registro.
  - La precondición 6 mide la cobertura rama por rama con la retirada aplicada. Puede usar el formato JSON de coverlet, que dice qué rama se cubre, y así resuelve también la 525 y la 594.
  - Si algo queda sin cubrir, vuelve al Owner.
