# Decisión del Owner sobre PB-T4d (I-64 F4B): los caminos que solo cubren las pruebas de la corrida genérica

**Fecha:** 2026-11-22. Después de `owner-pbt4d-substitutes-decision.md`, cuya nota de la sesión pedía comprobar la cobertura de las 48 pruebas de WPUR.

**Por qué se preguntó:** el autor del paquete midió la cobertura de líneas y de ramas con coverlet sobre `6c2820ba` sin cambios, en dos copias exportadas:
- `/d/r6j`: WPUR sola, frente a toda la suite de Core sin WPUR;
- `/d/r6k`: cada prueba de WPUR sola.

Los resultados están en `v52/cov-r6j.txt` y `v52/cov-tests-r6k.txt`:
- **Lo que recorren:** las 48 recorren 436 líneas de la corrida genérica (`Execution<TState>`) y del plan que se quedan con `ExecuteFull`.
- **Lo que cubre el resto de la suite:** 401 de esas líneas.
- **Lo que solo cubre WPUR:** 35 líneas y 22 líneas con ramas, en 9 miembros. Son fronteras raras de la tabla D-13:
  - sin ámbito;
  - la liberación externa que lanza;
  - los cinco resultados de PREPARE dentro de la corrida;
  - la liberación de MUTATE que lanza;
  - el destino que cambió;
  - el abort que falla;
  - el aviso de nombre en POST;
  - sin supervivientes;
  - la vista no disponible de `BuildCore`.
- **Prueba a prueba:** 21 de las 49 de WPUR ejercitan algo que nada más ejercita, y el conjunto mínimo que lo cubre todo son 17.

La afirmación de la pregunta anterior («las 48 … también prueban las CT de PB-T3b») valía solo para 401 de las 436 líneas. La sesión lo corrige en esta pregunta.

La sesión preguntó con AskUserQuestion. Este registro copia el texto exacto. La numeración F4B-E se asigna al custodiarlo.

## Pregunta (encabezado «Corrida»)

**Pregunta, literal:**

> PB-T4d: comprobé lo que te dije sin medir (que las CT de PB-T3b cubren lo que ejercitan las 48 pruebas de la corrida genérica) y no es del todo cierto. Lo medí con cobertura de líneas: el resto de la suite cubre 401 de las 436 líneas que recorren, pero 35 líneas y 22 ramas solo las ejercitan esas pruebas. Son 9 fronteras raras de la escritura: sin ámbito, la liberación que lanza, los cinco resultados de la preparación dentro de la corrida, el destino que cambió, el abort que falla, el aviso de nombre, sin supervivientes y la vista no disponible. 21 de las 48 ejercitan algo que nada más ejercita. ¿Qué hago con esos caminos?

**Opciones:**
1. **Sustituir las 21 (Recomendado):** Esas 21 pasan a equivalentes sobre la escritura completa (ExecuteFull) en una clase nueva, con un ayudante del modelo completo; las otras 27 se borran con registro. Suma 21 piezas a las que ya ibas a tener. Así no queda sin prueba ningún camino que se queda.
2. **Solo las 17 mínimas:** Igual, pero solo el conjunto mínimo de 17 que cubre todas esas líneas. Cuatro de las 21 fijan salidas distintas sobre las mismas líneas y se perderían.
3. **Borrar las 48:** Se borran con registro y se declaran esos 9 caminos como pérdida de prueba. Es lo más barato, pero va contra lo que elegiste (no dejar sin red código que se queda).

**Respuesta del Owner, literal:** «Sustituir las 21 (Recomendado)».

## Lo que queda decidido

- **Las 21 pruebas de WPUR** que ejercitan algo que nada más ejercita (la lista, en `v52/cov-tests-r6k.txt`) se sustituyen por equivalentes sobre `ExecuteFull`.
  - Van en una clase nueva, `PushBackRetirePilotRunTests`, con un ayudante del modelo completo.
  - Se copian con el mapa de PB-T4b, y a mano donde no case.
  - La precondición 6 las mide.
- **Las otras 27 de WPUR** se borran con registro.
- **Es parte de la lista de PB-T4d,** con las sustitutas de `owner-pbt4d-substitutes-decision.md`. Si aparece otra prueba que falle fuera de ella, o un camino que se quede sin prueba, vuelve al Owner.
