# I-64 F4B — Decisión del Owner: las colecciones opacas del contrato las pone la sesión (F4B-E21)

**Fecha:** 2026-11-18. **Canal:** chat de la sesión principal, con una pregunta de opciones; la respuesta es literal. **La vuelta:** `F4B-DRAFTS/pbt1b/owner-return-planning2.md`.

**Los hechos.** El contrato de PB-T1b reemitido con la v2.7 mide unos 56 KB, y su delegación se rechazó dos veces solo por la copia:
- **`R20261118T203710Z-4000`:** colecciones enteras vacías y 17 de las 52 autoridades;
- **la replanificación única `R20261118T204045Z-a60d`:** todas las colecciones vacías. El Controller lo declaró así: «la respuesta generada desde el contrato excede el limite de salida disponible y se trunca, por lo que no puedo garantizar la copia literal de las colecciones opacas».

Lo propio del Controller (la clase, el enrutamiento y los criterios de aceptación) sale bien en todas las corridas. Por PBT1B-C08, el segundo rechazo vuelve al Owner.

**Texto de la pregunta, literal:** «PB-T1b: el contrato (56 KB: 52 autoridades, 213 archivos prohibidos, 33 paradas) ya no cabe en la salida del Controller. En la replanificación lo dijo él mismo («excede el limite de salida disponible y se trunca») y dejó vacías todas las colecciones que solo tiene que copiar. Su trabajo propio (clase, enrutamiento, criterios de aceptación) siempre sale bien, y los contratos siguientes serán más grandes. Es el segundo rechazo, así que vuelve a ti. ¿Qué hago?»

**Respuesta del Owner:** «La sesión copia; resto de F4B (Recomendado)».

**Lo que implica**, desde PB-T1b y en el resto de F4B:
- **Lo que deja de copiar el Controller:** las ocho colecciones opacas del contrato (Authorities, Objective, AllowedWriteScope, ForbiddenWriteScope, Invariants, RequiredTests, ExpectedEvidence y StopConditions). Las deja vacías.
- **Lo que pone la sesión:** esas ocho colecciones, byte a byte desde el contrato, de forma determinista. La salida del Controller se conserva tal cual (`output.json`), y la delegación completa es `delegation.json`.
- **Lo que sigue haciendo el Controller:** la clase de la tarea, las dimensiones, la celda y el enrutamiento, y los criterios de aceptación, que redacta contrastándolos con el contrato.
- **Lo que no cambia:** la comparación byte a byte se hace igual sobre la delegación final y tiene que dar PASS. A1-A8 y nc4 se mantienen. Cada relevo lo declara.
- **El presupuesto:** para PB-T1b hace falta una planificación nueva con ese prompt, que no gasta intento.
