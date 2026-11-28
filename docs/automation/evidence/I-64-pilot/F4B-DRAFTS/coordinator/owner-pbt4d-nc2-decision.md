# Decisión del Owner sobre nc2 de PB-T4d (I-64 F4B) y la v3 del sustituto de nc2

**Fecha:** 2026-11-22. **Canal:** el chat de la sesión principal, con el diálogo de preguntas de la aplicación. La pregunta y la respuesta son literales. La numeración F4B-E se asigna al custodiarlo.

**Contexto:**
- **La verificación real** `R20261122T194536Z-4c59` dio EXECUTION_VERIFIED / NONE, 14/14.
- **nc1** `R20261122T195309Z-fc6e` dio CONTROL NOT PASSED con la firma PD-F4B-01, y su sustituto revisado `R20261122T201052Z-07ea` dio PASS. **nc3** `R20261122T200420Z-4be7` dio CONTROL PASS.
- **nc2** `R20261122T195517Z-096b` dio CONTROL NOT PASSED: EXECUTION_BLOCKED / STOP, FailureClass Contract, Contract y Scope en fail, con S-04, P-03 y PBT4D-C01. La evidencia de Scope nombra la ruta quitada, `src/RackCad.Application/Systems/PushBack/Pilot/PushBackPilotDraftState.cs`.
- **El sustituto v2 revisado** (`C033E96C…`), corrida `R20261122T201101Z-bb30`, no dio veredicto: `FAIL (entrada ausente o mal formada: MemoryError: )`. Su propiedad 10 enumera los 2^n subconjuntos del AllowedWriteScope del contrato, y PB-T4d tiene 37 rutas; PB-T4c tenía 21.
- **El diagnóstico de la sesión,** que no es evidencia (`_session_t4d/diag_nc2_p10.py`): el código de la v2 tal cual, con la propiedad 10 en su forma cerrada, da PASS 10/10, y la autoprueba sigue pasando.

## Pregunta (encabezado «nc2»)

> Control nc2 de PB-T4d: el Controller detectó la mutación y paró (Contract y Scope en fail, con S-04, P-03 y C01). Es la misma firma que en PB-T3a, donde el sustituto v2 revisado dio PASS. Esta vez el sustituto se cayó sin dar veredicto, con MemoryError: su propiedad 10 recorre todos los subconjuntos del alcance del contrato, y con 37 rutas son 2³⁷ (en PB-T4c eran 21). Hice un diagnóstico que no cuenta como evidencia: con el código revisado tal cual y solo la propiedad 10 en su forma cerrada equivalente («la ruta está en el alcance del contrato y no en el prohibido»), da PASS 10/10, y las 63 autopruebas siguen pasando. Lo demás de PB-T4d está en verde: verificación 14/14, nc1 PASS por su sustituto y nc3 PASS. ¿Qué hago?

**Opciones:**
1. **«Sustituto v3 y revisión (Recomendado)»:** Escribo una v3 del sustituto igual a la v2 salvo la propiedad 10 en forma cerrada. Pasa una revisión independiente (equivalencia, autopruebas y los casos reales de PB-T2a, PB-T3a y PB-T4c), y la aplico a este nc2 y a PB-T5. Si la revisión no da APTO o el resultado no es PASS, vuelvo a ti. Cuesta como una hora.
2. **«Aceptar nc2 solo esta vez»:** Lo registro como satisfecho por tu disposición, solo en PB-T4d, con el diagnóstico como apoyo, y cierro ya. PB-T5 volvería a toparse con el mismo límite si su alcance es grande.
3. **«Parar y revisarlo yo»:** No cierro PB-T4d. Te dejo el diagnóstico y la causa del MemoryError para que lo mires tú.

**Respuesta del Owner, literal:** «Sustituto v3 y revisión (Recomendado)».

## Lo que implica

1. **La v3,** `nc2_exact_whitelist_substitute_f4b_v3.py`, es la v2 revisada con un solo cambio de lógica: la propiedad 10 en su forma cerrada (`p is not None and covered(p, ca) and not covered(p, ForbiddenWriteScope)`). Cambian además el docstring y el campo informativo `AlternativeCandidatesExcludingPath`. El id, la autoprueba, `--task` y su ligadura, y las propiedades 1 a 9 no cambian.
2. **La revisión independiente** comprueba la equivalencia de la propiedad 10, la autoprueba (determinista) y los casos reales en los que la v2 sí termina: la v3 tiene que dar el mismo resultado y las mismas propiedades. Después, la v3 se aplica a este nc2 con `--task PBT4`.
3. **Si la revisión da APTO y la v3 da PASS sobre este nc2:** `nc2 = SATISFIED_UNDER_PBT4_EXACT_WHITELIST_SUBSTITUTE_F4B` para F4B-PBT4-RETIRE-PILOT, intento 1, y la v3 sustituye a la v2 de aquí en adelante, PB-T5 incluida. La corrida ordinaria de nc2 y la corrida fallida de la v2 se conservan tal cual.
4. **Si la revisión no da APTO, o la v3 no da PASS:** se vuelve al Owner.
5. **La firma** no cambia: el sustituto solo se usa con la firma autorizada (F4B-E43 y PD-F4-01 en modo F4B-E21), y cualquier otra variación vuelve al Owner.
