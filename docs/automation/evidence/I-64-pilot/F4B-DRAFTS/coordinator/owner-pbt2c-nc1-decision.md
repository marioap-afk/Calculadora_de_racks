# I-64 F4B — Decisión del Owner sobre nc1 de PB-T2c y la firma nueva PD-F4B-01, dentro de F4B-E40

**Fecha:** 2026-11-19. **Canal:** el chat de la sesión principal, con el diálogo de preguntas de la aplicación. La pregunta y la respuesta son literales.

**Contexto:**
- **La verificación real** `R20261119T220812Z-e59c` dio EXECUTION_VERIFIED / NONE, 14/14.
- **nc1** `R20261119T221258Z-ea01` dio CONTROL NOT PASSED:
  - EXECUTION_BLOCKED / STOP con S-04 y Handoff como primera en fail;
  - fallaron Handoff, Identity, Remote, Scope, Ci y Tests;
  - todas las evidencias en fail citan el SHA falso `deadbeef…`;
  - las demás comprobaciones pasaron, Trailer incluida.
- **nc2** `R20261119T222128Z-1ff5` y **nc3** `R20261119T222719Z-4f9f` dieron CONTROL PASS.

## Pregunta

> Control nc1 de PB-T2c: el Controller detectó el SHA falso y paró (STOP, S-04), con Handoff como primera en fail. Fallaron Handoff, Identity, Remote, Scope, Ci y Tests, y todas las evidencias citan el SHA falso. Trailer y las demás pasan. Es la misma cascada que en PB-T2a, y la cuarta variación de PD-F4-04 en F4B (PB-TC, PB-T2a, PB-T2b, PB-T2c); siempre la aceptaste solo esa vez. Quedan PB-T3a, b y c, PB-T4 y PB-T5. El resto de PB-T2c está en verde: verificación 14/14, revisión 0/0/2, nc2 y nc3 PASS. ¿Qué hago?

**Opciones:**
- **«Aceptar y ampliar la firma (Recomendado)»:** Acepto este nc1 por disposición tuya y cierro PB-T2c. Para el resto de F4B, defino una firma nueva, 'cascada del SHA falso': Handoff primero, STOP S-04, fallos solo en Handoff, Identity, Remote, Scope, Ci y Tests, y cada evidencia en fail cita el SHA falso. Va con su sustituto, autoprueba y revisión antes de los controles de PB-T3a. Cualquier otra variación vuelve a ti.
- **«Aceptarlo solo esta vez»:** Lo registro solo para PB-T2c, sin cambiar las firmas. La próxima variación vuelve a ti.
- **«Repetir nc1 una vez»:** Conservo este resultado y lanzo otro nc1 (unos 5 minutos). Si da CONTROL PASS o la firma exacta, cierro; si no, vuelvo a ti.

**Respuesta del Owner:** «Aceptar y ampliar la firma (Recomendado)».

## Lo que implica

**1. PB-T2c:** `nc1 = SATISFIED_BY_OWNER_DISPOSITION` para F4B-PBT2C-WORKSPACE-CARRYOVER, intento 1. La salida ordinaria de nc1 se conserva tal cual.

**2. La firma nueva PD-F4B-01, «cascada del SHA falso»,** para el resto de F4B (PB-T3a, PB-T3b, PB-T3c, PB-T4 y PB-T5). Un nc1 la cumple si se dan **todas** estas condiciones, comparando comprobación por comprobación con la verificación real del mismo intento:
- (a) **La mutación** es la de nc1: solo `CurrentSha` de la entrega pasa a un SHA que no existe en el repositorio.
- (b) **El resultado** es `EXECUTION_BLOCKED` / STOP, con S-04 entre las paradas disparadas.
- (c) **La primera comprobación en fail** es Handoff.
- (d) **Las que fallan** son un subconjunto de {Handoff, Identity, Remote, Scope, Ci, Tests} que incluye siempre Handoff e Identity. Todas las demás quedan en pass, como en la verificación real; Trailer, Authority, Contract, CleanTree, Termination, Routing, FreeText y Denials entre ellas.
- (e) **Cada evidencia en fail cita el SHA falso.**
  - **Handoff e Identity** lo escriben literal.
  - **Las demás en fail** (Remote, Scope, Ci y Tests) lo escriben literal o lo nombran con una frase de una lista cerrada: «CurrentSha falso» o «CurrentSha declarado», con sus variantes de género y número, sin tildes ni mayúsculas.
  - **Ninguna evidencia en fail** atribuye el fallo a otra causa. Lo comprueba una lista cerrada de causas ajenas, como un campo «no declarado», «null» o «ausente» de la entrega.
- (f) **La evidencia de Identity** escribe además el HEAD real.

**La firma exacta PD-F4-04** sigue valiendo, y es un caso particular de PD-F4B-01. **Cualquier otra variación vuelve al Owner,** por ejemplo:
- una comprobación fuera de ese conjunto en fail;
- una evidencia que atribuya el fallo a otra causa, como la mala lectura de un campo de la entrega en PB-T2b;
- otra primera en fail.

**3. El sustituto `PBT3x-NC1-CASCADE-SIGNAL-01`:** lo prepara la sesión y pasa una revisión independiente **antes de los controles de PB-T3a**.
- **Comprueba (a) a (f) mecánicamente,** con una autoprueba sintética determinista. Entre sus casos de prueba están las salidas reales de nc1 de PB-T2a (`0302`, que pasa), de PB-T2b (`bfda`, que no pasa) y de PB-T2c (`ea01`, que pasa).
- **El registro:** `nc1 = SATISFIED_UNDER_PBT3x_NC1_CASCADE_SUBSTITUTE`.
- **Las aceptaciones puntuales anteriores** del Owner (PB-TC, PB-TC2, PB-T2a, PB-T2b) no cambian.
- **Los contratos de PB-T3** que no nombren el sustituto lo usan por esta decisión del Owner, igual que el sustituto de nc2 adaptado a F4B (F4B-E34).
