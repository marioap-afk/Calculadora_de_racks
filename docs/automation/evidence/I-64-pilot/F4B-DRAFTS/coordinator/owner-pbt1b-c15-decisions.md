# I-64 F4B PB-T1b — Decisiones del Owner sobre la parada PBT1B-C15

**Fecha:** 2026-11-18. **Canal:** el chat de la sesión principal, con preguntas de opciones; las respuestas son literales. **La vuelta:** `F4B-DRAFTS/pbt1b/owner-return-c15.md`.

**Los hechos.** El Worker `R20261118T193423Z-fb01` (la reanudación de la delegación `c45c`, intento 1) paró con PBT1B-C15 antes del RED, sin commits:
- la mitad estado → diseño → estado de CT-PBT1B-02 da «distintos» en `pb-15-sin-composite`;
- solo cambia el frente 0 del lado B, que pasa de activo a inactivo, y el lado B no está presente;
- es la normalización que `LegacyDeltas` ya registra para pb-15: «`$.PushBack.Composite.AbsentSlotsB[0]: <absent> -> 0`».

## Pregunta 1

**Texto, literal:** «PB-T1b se detuvo (parada C15, sin commits). En el rack antiguo pb-15, que no guarda la parte compuesta, el estado cambia en una sola cosa tras la primera ida y vuelta: el frente 0 del lado B pasa de activo a inactivo. El lado B no está presente, así que no se dibuja ni se usa, y es la normalización que la ventana ya hace hoy al escribir (el piloto la tiene registrada para pb-15). ¿Cómo lo trato?»

**Respuesta del Owner:** «Normalización conocida (Recomendado)».

**Lo que implica:**
- No es una pérdida.
- Se corrige CT-PBT1B-02: la mitad estado → diseño → estado se mide desde el estado ya normalizado por una primera ida y vuelta, y en los seis racks antiguos la diferencia de esa primera vuelta es exactamente la de `LegacyDeltas` vista desde el estado.
- El paquete pasa a la v2.7, que aclara también CT-06, y lleva su comprobación acotada. Después se emite el contrato corregido y una delegación nueva.
- RACKEDITAR no cambia.

## Pregunta 2

**Texto, literal:** «PB-T1b: la parada vino de un hueco del contrato (CT-02 no preveía los racks antiguos) y el Worker la respetó sin escribir producto. ¿La delegación nueva gasta intento?»

**Respuesta del Owner:** «Sin gastar intento (Recomendado)».

**Lo que implica:** la delegación nueva sigue en el intento 1 de 3, como la parada C14 de PB-TC2.
