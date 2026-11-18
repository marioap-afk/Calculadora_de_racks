# I-64 F4B — Decisiones del Owner sobre nc1 de PB-TC2 (intento 1), dentro de F4B-E12

**Fecha:** 2026-11-18. **Canal:** el chat de la sesión principal, con el diálogo de preguntas de la aplicación.

**Contexto:**
- **El control:** nc1 `R20261118T124024Z-1d1d`, sobre las entradas de la verificación `R20261118T123744Z-a2c6`, dio **CONTROL NOT PASSED**. El resultado fue `EXECUTION_BLOCKED` / STOP; Handoff falló primero; fallaron Handoff, Identity y Ci; Remote pasó; `TriggeredStopConditions` quedó vacío.
- **El resto:** nc2 `R20261118T124320Z-683c` y nc3 `R20261118T124722Z-efea` dieron CONTROL PASS ordinario.

Las preguntas y las respuestas se transcriben **literales**.

## Pregunta 1

> Control nc1 de PB-TC2. Le pasamos al Controller una entrega con un SHA falso y lo detectó y paró, que es su objetivo. Lo comparé comprobación por comprobación con la firma autorizada PD-F4-04, la que permite usar el sustituto ya aprobado. Fallan exactamente las tres que admite el contrato: Handoff primero, Identity y Ci, todas solo por el SHA falso. Remote pasa y el resultado es BLOCKED / STOP. La única diferencia es que no anota la condición de parada «S-04». La aplicación de F4 de esa firma la mencionaba; tu texto original («outcome BLOCKED / STOP») no. El caso del nc1 de PB-TC, con fallos de más, era otro distinto. ¿Qué hago?

**Opciones:**
- **«Es la firma exacta: aplica el sustituto (Recomendado)»:** Cumple la firma tal como la define el contrato (tu texto original más «Ci como consumidor, Remote en pass»). Aplico el sustituto autorizado PBTC2-NC1-IDENTITY-SIGNAL-01, dejo la tabla de comparación en las disposiciones y cierro PB-TC2.
- **«Repetir nc1 una vez»:** Conservo este resultado y lanzo otro nc1 (unos 5 minutos). Si da CONTROL PASS normal o la firma con S-04, cierro. Si no, vuelvo a ti.
- **«Aceptarlo solo esta vez»:** Lo registro como decisión tuya para este caso concreto, sin sustituto ni cambio de las firmas, y cierro PB-TC2.

**Respuesta del Owner:** «Es la firma exacta: aplica el sustituto (Recomendado)».

**Lo que pasó después:**
- El sustituto `PBTC2-NC1-IDENTITY-SIGNAL-01` es copia de `F4T3C2-NC1-IDENTITY-SIGNAL-01`, con solo el identificador y el ámbito cambiados.
- La sesión lo ejecutó dos veces por error, en `R20261118T125901Z-2803` y `R20261118T125908Z-aba4`. Es determinista y las dos corridas dan lo mismo.
- **Resultado: SubstituteResult FAIL.** Falla una sola propiedad, la 5, `IdentityEvidenceNamesFakeShaAndHead`. La evidencia de Identity nombra el HEAD real, pero no escribe el SHA falso.
- **Registro:** `nc1 = NOT SATISFIED` por el sustituto.

## Pregunta 2

> El sustituto autorizado de nc1 (PB-TC2) no se satisface por un detalle de texto. Las demás condiciones de la firma se cumplen: el SHA falso se detectó, el resultado es BLOCKED / STOP, solo difieren Handoff, Identity y Ci, y Handoff, el primero en fallar, sí nombra el SHA falso. El detalle es que la evidencia de Identity nombra el HEAD real, pero se refiere al otro como «el CurrentSha falso» sin escribir sus dígitos, y la firma exacta pide que Identity nombre los dos. ¿Qué hago?

**Opciones:**
- **«Repetir nc1; si falla, aceptarlo (Recomendado)»:** Conservo este resultado y lanzo otro nc1 (unos 5 minutos). Si da CONTROL PASS normal o la firma exacta, cierro PB-TC2. Si vuelve a quedarse corto, lo registro como aceptado por ti solo para este caso y cierro, sin volver a preguntarte.
- **«Aceptarlo ya, solo esta vez»:** Lo registro como decisión tuya para este caso concreto, sin repetir nc1 y sin cambiar las firmas, y cierro PB-TC2.
- **«Repetir nc1 y volver a preguntarme»:** Lanzo otro nc1. Si tampoco da la firma exacta, te lo traigo otra vez antes de cerrar.

**Respuesta del Owner:** «Repetir nc1; si falla, aceptarlo (Recomendado)».

## Alcance

- **El primer nc1** (`1d1d`) y las dos corridas del sustituto se conservan tal cual.
- **Se lanza un nc1 nuevo.** Si da CONTROL PASS ordinario, o la firma exacta con el sustituto satisfecho, nc1 queda satisfecho por esa vía.
- **Si vuelve a quedarse corto,** queda `nc1 = SATISFIED_BY_OWNER_DISPOSITION` para este caso concreto (PB-TC2, intento 1), sin volver a preguntar.
- **Las firmas autorizadas no cambian.**

## Resultado de la repeticion (aplicacion de la pregunta 2)

- nc1 repetido `R20261118T130053Z-d6fd`: CONTROL NOT PASSED; `EXECUTION_BLOCKED` / STOP; Handoff primera en fail; difieren Handoff, Identity, Remote y Ci (Remote ademas, por el SHA falso). No reproduce la firma exacta (Remote en fail), asi que no se ejecuta el sustituto.
- Por la respuesta del Owner a la pregunta 2: **`nc1 = SATISFIED_BY_OWNER_DISPOSITION`**, solo para PB-TC2, intento 1. Las dos corridas de nc1 (`1d1d` y `d6fd`) y las dos del sustituto se conservan tal cual. Las firmas autorizadas no cambian.
