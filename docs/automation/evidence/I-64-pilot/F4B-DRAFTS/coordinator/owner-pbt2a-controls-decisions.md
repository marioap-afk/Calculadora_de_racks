# I-64 F4B — Decisiones del Owner sobre nc1 y nc2 de PB-T2a (intento 2), dentro de F4B-E34

**Fecha:** 2026-11-19. **Canal:** el chat de la sesión principal, con el diálogo de preguntas de la aplicación, en una sola ronda de cuatro preguntas. Las preguntas 3 y 4 de esa ronda están en `owner-f4b-pbt3-emission-red-repair-decisions.md` (F4B-E35).

**Contexto:** sobre las entradas de la verificación `R20261119T074453Z-10a9` (EXECUTION_VERIFIED / NONE, 14/14):
- nc1 `R20261119T074938Z-0302` dio **CONTROL NOT PASSED**:
  - EXECUTION_BLOCKED / STOP, con S-04;
  - Handoff primero;
  - en fail: Handoff, Identity, Remote, Scope, Ci y Tests;
  - Trailer en pass.
- nc2 `R20261119T075153Z-a799` dio **CONTROL NOT PASSED** con la firma exacta de PD-F4-01:
  - Contract primero, y en fail solo Contract y Scope;
  - S-04 y PBT2A-C01.
- El sustituto `PBT2A-NC2-EXACT-WHITELIST-01` (`R20261119T081504Z-9d60`, copia de `F4T1B-NC2-EXACT-WHITELIST-01` con solo el identificador y el ámbito cambiados) dio **FAIL**: las propiedades 5 y 8, por el formato de F4B-E21.
- nc3 `R20261119T075831Z-6b19` dio CONTROL PASS.

Las preguntas y las respuestas se transcriben **literales**.

## Pregunta 1

> Control nc1 de PB-T2a. Le pasamos al Controller una entrega con un SHA falso, y lo detectó y paró (STOP, S-04). Pero fallaron Handoff, Identity, Remote, Scope, Ci y Tests, todas por el SHA falso, con Trailer en pass. La firma autorizada PD-F4-04 solo admite Handoff, Identity y Ci. Es la misma variación que aceptaste una vez en PB-TC (F4B-E08), y es la segunda vez en F4B. El resto de PB-T2a está en verde: CI 4/4, revisión 0/0/5, verificación 14/14 y nc3 PASS. ¿Qué hago?

**Opciones:**
- **«Aceptarlo solo esta vez (Recomendado)»:** Lo registro como decisión tuya solo para PB-T2a (nc1 = SATISFIED_BY_OWNER_DISPOSITION), sin cambiar las firmas. La próxima variación en otra subtarea volverá a ti.
- **«Repetir nc1 una vez»:** Conservo este resultado y lanzo otro nc1 (unos 5 minutos). Si da CONTROL PASS o la firma exacta, sigo. Si vuelve a salir la variación, vuelvo a ti.
- **«Ampliar la firma para F4B»:** Para el resto de F4B, admito también Remote, Scope y Tests en fail cuando su evidencia nombra el SHA falso. Es una firma nueva, así que lleva un sustituto nuevo con su autoprueba y su revisión.

**Respuesta del Owner:** «Aceptarlo solo esta vez (Recomendado)».

**Registro:** `nc1 = SATISFIED_BY_OWNER_DISPOSITION`, solo para F4B-PBT2A-DRAFT-STATE-PORT, intento 2. Las firmas no cambian. La salida ordinaria de nc1 se conserva tal cual.

## Pregunta 2

> Control nc2 de PB-T2a: tiene la firma exacta de PD-F4-01 (solo Contract y Scope en fail, por la ruta quitada; STOP). Pero el sustituto de F4, copiado con solo el id y el ámbito cambiados, dio FAIL en 2 de sus 10 propiedades. Es por el formato de F4B-E21: el relevo guarda el SHA crudo del Controller, y la exactitud va por la comparación byte a byte en vez de la frase de F4. No es un fallo del Controller, y fallará igual en toda F4B. Preparé una versión adaptada solo en esas 2 propiedades: vista previa PASS, autoprueba 50/50 y revisión independiente APTO tras dos rondas. ¿Qué hago?

**Opciones:**
- **«Adaptar el sustituto (Recomendado)»:** Aplico el sustituto adaptado y revisado a PB-T2a, y lo custodio con su revisión y con el FAIL del de F4. En el resto de F4B se usa la misma copia con solo el id y el ámbito cambiados, y solo con la firma exacta de PD-F4-01.
- **«Aceptarlo solo esta vez»:** Lo registro como decisión tuya solo para PB-T2a (nc2 = SATISFIED_BY_OWNER_DISPOSITION). La próxima vez que salga PD-F4-01 en F4B volverá a ti.
- **«Repetir nc2 una vez»:** Lanzo otro nc2. En PB-T1b y PB-T1c dio CONTROL PASS por Scope; puede repetirse eso o la misma firma. Si sale la misma firma, vuelvo a ti.

**Respuesta del Owner:** «Adaptar el sustituto (Recomendado)».

**Lo que pasó después:**
- El sustituto `PBT2A-NC2-EXACT-WHITELIST-F4B-01` (SHA-256 `6ccaf12dbf9122c0bddc5b724bcff573705c95698af38bf750bf69459ee2ffd3`) se ejecutó en `R20261119T125505Z-c2d3`. Usó los siete argumentos: la verificación real, nc2, BaseSha, CurrentSha, la salida, y la nota de inyección y la salida cruda de la planificación `R20261119T065628Z-7de7`.
- **Resultado: PASS** en modo F4B-E21:
  - las 10 propiedades en true;
  - la autoprueba, 50 de 50 y determinista.
- **Registro:** `nc2 = SATISFIED_UNDER_PBT2A_EXACT_WHITELIST_SUBSTITUTE_F4B`. La salida ordinaria de nc2 y el FAIL del sustituto de F4 se conservan tal cual.
- **Origen del sustituto:**
  - es una copia de `F4T1B-NC2-EXACT-WHITELIST-01`, vía `PBT2A-NC2-EXACT-WHITELIST-01`;
  - cambian solo el identificador, el ámbito, el registro, las propiedades 5 y 8 en el modo F4B-E21, la autoprueba y los argumentos 6 y 7;
  - sin la nota y la salida cruda, se comporta como el de F4.
- **La revisión independiente:**
  - primera ronda, APTO TRAS CORRECCIONES 0/4/7, con tres falsos PASS encontrados por sondas;
  - tras corregir, la recomprobación dio APTO 0/0/3, y esas menores también se aplicaron.
- **Lo que hay que saber:**
  - el kit de nc2 recalcula el hash del relevo con la regla de F4, y el sustituto exige exactamente eso;
  - el sustituto queda acoplado a la serialización de `inject_opaque.py`.

## Alcance

- **nc1:** la aceptación vale solo para PB-T2a, intento 2. No se extiende a ninguna otra subtarea ni cambia PD-F4-04.
- **nc2:** en el resto de F4B, el sustituto adaptado se reutiliza como `PBT<x>-NC2-EXACT-WHITELIST-F4B-01`, con solo el identificador y el ámbito cambiados, y **solo** con la firma exacta de PD-F4-01 (F4B-E03). Cualquier otra firma o variación vuelve al Owner. Si cambia `inject_opaque.py`, el sustituto se revisa antes de reutilizarlo.
