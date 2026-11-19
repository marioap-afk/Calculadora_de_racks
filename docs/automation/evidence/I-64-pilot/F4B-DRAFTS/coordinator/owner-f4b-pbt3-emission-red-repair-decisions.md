# I-64 F4B — Decisiones del Owner: la emisión condicional de PB-T3 y la reparación del RED (F4B-E35)

**Fecha:** 2026-11-19. **Canal:** el chat de la sesión principal, con el diálogo de preguntas de la aplicación, en la misma ronda que las decisiones sobre nc1 y nc2 de PB-T2a (`owner-pbt2a-controls-decisions.md`, F4B-E34).

Las preguntas y las respuestas se transcriben **literales**.

## Pregunta 3

> PB-T3 se divide en tres: PB-T3a (el modelo de escritura completa, solo archivos nuevos), PB-T3b (la corrida generalizada, con las seis sustituciones de D-1) y PB-T3c (el borrado de vistas en el Plugin). Su paquete v4.2 está recomprobado (LISTO PARA EMITIR) y el generador de PB-T3a está listo. Su base será el cierre de PB-T2c. ¿Emito cada una sin volver a ti si su revisión da 0 bloqueantes y 0 importantes sin resolver y la remedición sobre el cierre anterior no cambia nada material, como con PB-T2?

**Opciones:**
- **«Sí, con esa condición (Recomendado)»:** Vuelvo a ti solo por una decisión nueva, un control con una firma no prevista, un intento 3 o un efecto material sin medir.
- **«No, vuelve antes de cada una»:** Te presento cada contrato de PB-T3 antes de emitirlo.

**Respuesta del Owner:** «Sí, con esa condición (Recomendado)».

**Alcance:**
- **La autorización condicional cubre** PB-T3a, PB-T3b y PB-T3c, en ese orden y cada una tras el cierre de la anterior, con la misma condición que F4B-E28 (pregunta 4):
  - su comprobación acotada da 0 bloqueantes y 0 importantes sin resolver;
  - la remedición sobre el cierre anterior no cambia nada material.
- **Vuelve al Owner antes de emitir:**
  - una decisión nueva;
  - un control con una firma no prevista;
  - un intento 3;
  - un efecto material sin medir.
- **Resuelve DU-1** del generador de PB-T3a (la precondición 10 del paquete v4.2).

## Pregunta 4

> En PB-T1c y en PB-T2a se gastó el intento 1 por un error de escritura en una prueba nueva, que solo aparece al probar la implementación después del commit RED. El régimen prohíbe tocar tests/ tras el RED, así que cada error cuesta una corrección completa y un intento. Un tercer caso en una misma tarea lleva al intento 3, que vuelve a ti. ¿Permito en el resto de F4B un commit 'reparación del RED' antes del GREEN, solo en las pruebas nuevas de la tarea, declarado, con el RED repetido entero y sin cambiar ninguna línea esperada del paquete?

**Opciones:**
- **«Sí, con esas condiciones (Recomendado)»:** El Worker corrige la prueba en un commit propio antes del GREEN, vuelve a pasar el criterio del RED sobre el esqueleto y lo declara. No gasta intento. Si la reparación cambia una línea esperada del paquete, sigue siendo parada.
- **«No, cada error es corrección»:** Como hoy: cada error de escritura en una prueba nueva gasta un intento y una corrección completa.

**Respuesta del Owner:** «Sí, con esas condiciones (Recomendado)».

## La regla operativa de la reparación del RED (RR), regla de F4B

Vale para el resto de F4B, en cada subtarea cuyo contrato la incorpore. **Empieza en PB-T2b.** Excepciona AUTOMATION_PLAN 16.8 («tests/ no se toca tras el RED») solo en estas condiciones, como F4B-E25 hizo con 16.3. Si se rompe alguna, la regla no aplica y vale la parada de siempre.

- **RR-1. Cuándo:** solo después del commit RED y su push, y antes del primer commit que toque `src/` (el GREEN). Como mucho **una** reparación por intento; un segundo error de escritura en el mismo intento es la parada de siempre.
- **RR-2. Qué toca:**
  - solo archivos de `tests/` que el commit RED de la tarea **añadió**, es decir, las clases de prueba nuevas de la tarea;
  - nunca una prueba existente, ni `src/`, ni un fixture, ni ningún archivo fuera del `AllowedWriteScope`.
- **RR-3. Sin cambiar lo esperado:**
  - ninguna línea esperada, literal ni oráculo que fije el paquete o el contrato cambia;
  - la reparación corrige solo el **cuándo** y el **cómo** de una lectura o de un gesto: el orden de las lecturas, el objeto leído o el paso de la receta que el paquete ya prescribe;
  - si para que la prueba pase hace falta cambiar una línea esperada, es la parada de siempre (C07 o su equivalente) y vuelve como hoy.
- **RR-4. El RED, repetido entero:**
  - sobre el commit de reparación (`RedRepairSha`) se vuelve a pasar el criterio completo del RED del contrato: los filtros con `ExpectRed` fallan por aserción, el resto pasa y la CI del RED es la que espera el contrato;
  - el commit se empuja y su CI se registra como la del RED reparado.
- **RR-5. Declarado:**
  - el commit se titula «RED repair» y nombra la tarea;
  - la entrega del Worker declara `RedRepairSha`, la prueba y la línea corregidas, el error y por qué no cambia ninguna línea esperada, con el diff;
  - `RedSha` sigue siendo el commit RED original.
- **RR-6. La cadena:**
  - BaseSha..RedSha, solo `tests/`;
  - RedSha..RedRepairSha, solo archivos que el RED añadió;
  - RedRepairSha..CurrentSha, solo `src/`.

  Sin reparación, la cadena es la de siempre.
- **RR-7. No gasta intento.** La revisión independiente y la verificación del Controller leen la reparación como parte de la entrega.
