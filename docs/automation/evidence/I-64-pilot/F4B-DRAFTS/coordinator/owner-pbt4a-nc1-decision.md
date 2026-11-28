# Decisión del Owner sobre nc1 de PB-T4a (I-64 F4B)

**Fecha:** 2026-11-20.

**Qué pasó:**
- **nc1** `R20261120T214408Z-bdf3` (la mutación pone un SHA falso, `deadbeef…`, en la entrega) dio CONTROL NOT PASSED: EXECUTION_BLOCKED/STOP, FailureClass Handoff y S-04.
- **Difieren de la verificación real** `R20261120T213256Z-8d11` las comprobaciones Handoff, Identity, Remote, Scope, Ci y Tests.
- **El sustituto revisado de PD-F4B-01** (`apply_nc1_cascade_pbt4a.sh`, `--task PBT4`, ScriptSha256 `41440edc…`; corrida `R20261120T220533Z-749e`) dio FAIL en una sola propiedad, `e2_OtherFailCitesFakeSha`. La evidencia de Tests no cita el SHA falso literalmente. Lo que dice es: «The GREEN TRX belongs to measured HEAD, not the handoff CurrentSha, so exact-SHA test evidence is absent». Es la misma causa dicha con otras palabras.
- **Es la quinta variación de nc1 en F4B,** tras PB-TC, PB-TC2, PB-T2a y PB-T2b.
- **Por qué vuelve al Owner:** un sustituto solo se aplica con la firma exacta.
- **Los otros dos controles:** nc2 `R20261120T214941Z-4cda` queda SATISFIED_UNDER_PBT4_EXACT_WHITELIST_SUBSTITUTE_F4B por su sustituto v2 (corrida `R20261120T220537Z-60ab`, PASS), y nc3 `R20261120T215653Z-e244` da CONTROL PASS ordinario.

La sesión preguntó con AskUserQuestion. Este registro copia el texto exacto.

## Pregunta (encabezado «nc1 T4a»)

**Pregunta, literal:**

> En PB-T4a, nc2 y nc3 quedan satisfechos (nc2 por su sustituto, nc3 en PASS ordinario). nc1 (la mutación que pone un SHA falso, deadbeef…, en la entrega) dio CONTROL NOT PASSED. Su sustituto revisado (PD-F4B-01) dio FAIL por una sola propiedad: entre los fallos en cascada, el de Tests no cita el SHA falso literalmente; dice «la TRX del GREEN es del HEAD medido, no del CurrentSha de la entrega», que es la misma causa dicha con otras palabras. Es la quinta variación de nc1 en F4B (antes: PB-TC, PB-TC2, PB-T2a y PB-T2b). Un sustituto solo se aplica con la firma exacta, así que te vuelve. ¿Qué hago?

**Opciones:**
1. **Aceptar solo esta vez (Recomendado):** nc1 de PB-T4a queda SATISFIED_BY_OWNER_DISPOSITION, solo para esta corrida, con la tabla de su detección custodiada. El sustituto no cambia y PB-T4a cierra. Si en PB-T4b vuelve a pasar, te vuelve otra vez.
2. **Ampliar el sustituto:** Se prepara una v2 del sustituto de nc1 que también acepte como cita del SHA falso un fallo que señala que la evidencia no corresponde al CurrentSha de la entrega, con su revisión independiente antes de usarlo. PB-T4a espera a que esté revisada; sirve para las subtareas siguientes.
3. **Repetir nc1:** Se lanza otra corrida de nc1 y se aplica el sustituto tal cual. Si vuelve a fallar, te vuelve.

**Respuesta del Owner, literal:** «Aceptar solo esta vez (Recomendado)».

## Lo que queda decidido

- **nc1 de PB-T4a = SATISFIED_BY_OWNER_DISPOSITION,** solo para la corrida `bdf3`. La salida ordinaria y la del sustituto (`749e`, FAIL) se custodian tal cual.
- **El sustituto de nc1 no cambia,** y no se amplía ninguna firma.
- **Si la variación vuelve a aparecer en PB-T4b,** vuelve al Owner.
