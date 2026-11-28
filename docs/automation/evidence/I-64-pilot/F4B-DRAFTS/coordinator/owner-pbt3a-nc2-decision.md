# I-64 F4B — Decisión del Owner sobre nc2 de PB-T3a (intento 2), dentro de F4B-E43

**Fecha:** 2026-11-20. **Canal:** el chat de la sesión principal, con el diálogo de preguntas de la aplicación. La pregunta y la respuesta son literales.

**Contexto:**
- **La verificación real:** `R20261120T034433Z-6d41`, EXECUTION_VERIFIED / NONE, 14/14.
- **nc1** `R20261120T035005Z-c736` y **nc3** `R20261120T040354Z-5eac`: CONTROL PASS.
- **nc2** `R20261120T035748Z-ad66`: CONTROL NOT PASSED, con la firma exacta de PD-F4-01.
  - Fallan solo Contract, la primera, y Scope.
  - Contract falla por el contraste del `AllowedWriteScope` estrechado con el contrato y con `byte-fidelity-check.json`, y Scope por la ruta quitada, `PushBackConflictClassifier.cs`.
  - El resultado es STOP, con S-04, P-03 y PBT3A-C01.
- **El sustituto adaptado a F4B** `PBT3A-NC2-EXACT-WHITELIST-F4B-01` (`R20261120T040923Z-882e`) dio **FAIL** en su propiedad 8.
  - La propiedad 8 exige el literal de la entrada de evidencia de la comparación byte a byte de los contratos de PB-T2.
  - El contrato de PB-T3a la redacta con otras palabras.
  - Las otras nueve propiedades pasan.

## Pregunta

> Control nc2 de PB-T3a: el Controller dio la firma exacta de PD-F4-01. Solo fallan Contract y Scope por la ruta quitada, Contract solo por la exactitud de la lista blanca, y hay STOP. El resto de PB-T3a está en verde: verificación 14/14, revisión 0/0/9, nc1 y nc3 PASS. Pero el sustituto de nc2 adaptado a F4B dio FAIL en su propiedad 8. Al corregirlo se fijó como cláusula de exactitud el texto literal de la entrada de evidencia de los contratos de PB-T2, y el contrato de PB-T3a la redacta con otras palabras ('ocho colecciones', 'F4B-E21', 'sobre la delegación final'). Volverá a fallar en PB-T3b y PB-T3c, que usan el mismo paquete. ¿Qué hago?

**Opciones:**
- **«Aceptar y ampliar el sustituto (Recomendado)»:** Acepto este nc2 por disposición tuya y cierro PB-T3a. Para PB-T3b en adelante, el sustituto admite una lista cerrada de los literales exactos de cada paquete (PB-T2 y PB-T3, sin variantes libres), con autoprueba y revisión independiente antes de usarlo.
- **«Aceptarlo solo esta vez»:** Lo registro solo para PB-T3a, sin cambiar el sustituto. En PB-T3b, si nc2 da PD-F4-01, volverá a fallar y volverá a ti.
- **«Repetir nc2 una vez»:** Lanzo otro nc2. En PB-T2b y PB-T2c, nc2 dio CONTROL PASS por Scope, sin sustituto. Si vuelve a salir esta firma, vuelvo a ti.

**Respuesta del Owner:** «Aceptar y ampliar el sustituto (Recomendado)».

## Lo que implica

- **PB-T3a:** `nc2 = SATISFIED_BY_OWNER_DISPOSITION`, solo para F4B-PBT3A-FULL-WRITE-MODEL, intento 2. La salida ordinaria de nc2 y el FAIL del sustituto se conservan tal cual.
- **El sustituto ampliado, de PB-T3b en adelante:**
  - la propiedad 8 admite una lista cerrada de los literales exactos de la entrada de evidencia de la comparación byte a byte, uno por paquete, PB-T2 y PB-T3, sin variantes libres;
  - lo demás no cambia, y solo se usa con la firma exacta de PD-F4-01;
  - lleva autoprueba y revisión independiente antes de usarlo;
  - un paquete nuevo con otra redacción exige añadir su literal y volver a revisarlo.
