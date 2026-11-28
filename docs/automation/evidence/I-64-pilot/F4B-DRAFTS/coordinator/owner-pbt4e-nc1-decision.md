# Decisión del Owner sobre nc1 de PB-T4e (I-64 F4B)

**Fecha:** 2026-11-23. **Canal:** el chat de la sesión principal, con el diálogo de preguntas de la aplicación (preguntado a las 2026-11-23T07:25:41.287Z, respondido a las 2026-11-23T09:19:40.506Z). La pregunta y la respuesta son literales. La numeración F4B-E se asigna al custodiarlo.

**Qué pasó:**
- **La verificación real** `R20261123T070111Z-b46b` dio EXECUTION_VERIFIED / NONE, 14/14.
- **nc1** `R20261123T070642Z-446d` (la mutación pone un SHA falso, `deadbeef…`, en la entrega) dio CONTROL NOT PASSED: EXECUTION_BLOCKED / STOP, FailureClass Handoff y S-04. Difieren de la verificación real Handoff, Identity, Remote, Scope, Ci y Tests.
- **El sustituto revisado de PD-F4B-01** (`apply_nc1_cascade_pbt4e.sh`, `--task PBT4`, ScriptSha256 `41440edc…`; corrida `R20261123T072321Z-9cda`) dio FAIL en una sola propiedad, `e2_OtherFailCitesFakeSha`. Las evidencias de Ci y de Tests no citan el SHA falso por su valor: dicen «its HeadSha is 0eac7911…, not the handoff CurrentSha» y «CurrentSha is not a commit», la misma causa dicha con el nombre del campo. Ninguna atribuye el fallo a otra causa (`e3_NoAlienCause` en true).
- **Es la séptima variación de nc1 en F4B,** tras PB-TC, PB-TC2, PB-T2a, PB-T2b, PB-T4a y PB-T4b. La de PB-T4a fue la misma propiedad, con Tests.
- **Los otros dos controles:** nc2 `R20261123T071150Z-beaa` queda SATISFIED_UNDER_PBT4_EXACT_WHITELIST_SUBSTITUTE_F4B por la v3 de su sustituto (corrida `R20261123T072425Z-00b5`, PASS), y nc3 `R20261123T071718Z-ba7f` da CONTROL PASS ordinario.

## 1. Pregunta (encabezado «nc1 PB-T4e»)

**Pregunta, literal:**

> PB-T4e (la recarga de «Editar todo») pasó la verificación 14/14; nc3 dio PASS y nc2 quedó satisfecho por su sustituto v3. nc1 (pone un SHA falso en la entrega) dio CONTROL NOT PASSED: la verificación del control lo paró, y todos sus fallos salen de ese SHA falso. Su sustituto revisado da FAIL por una sola propiedad, la misma que en PB-T4a: las evidencias de Ci y de Tests nombran «el CurrentSha de la entrega», pero no escriben el SHA falso. Es la séptima variación de nc1 en F4B, y un sustituto solo vale con la firma exacta. PB-T4e no puede cerrar sin tu decisión, y por eso la corrida previa del smoke no se hizo anoche. ¿Qué hago?

**Opciones:**
1. **«Aceptarlo solo esta vez (Recomendado)»:** nc1 queda satisfecho por tu decisión, solo en esta corrida, sin cambiar el sustituto ni ampliar su firma, como en PB-T4a y PB-T4b. PB-T4e cierra enseguida.
2. **«Aceptar y revisar el sustituto»:** Lo mismo para PB-T4e, y además una v2 del sustituto que admita la cita por el nombre del campo, con revisión independiente. Solo sirve si PB-T5 lleva controles.
3. **«Repetir nc1»:** Otra corrida de nc1, unos 5 minutos, con el sustituto tal cual. Si vuelve a fallar, te vuelve.

**Respuesta del Owner, literal:** «Aceptarlo solo esta vez (Recomendado)».

## Lo que queda decidido

- **nc1 de PB-T4e = SATISFIED_BY_OWNER_DISPOSITION,** solo para la corrida `446d`. La salida ordinaria y la del sustituto (`9cda`, FAIL) se custodian tal cual.
- **El sustituto de nc1 no cambia,** y no se amplía ninguna firma.
- **Si la variación vuelve a aparecer,** vuelve al Owner.
