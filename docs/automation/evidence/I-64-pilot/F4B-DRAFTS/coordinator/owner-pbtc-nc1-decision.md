# I-64 F4B — Decisión del Owner sobre nc1 de PB-TC (intento 1)

**Fecha:** 2026-11-18. **Canal:** el chat de la sesión principal, con el diálogo de preguntas de la aplicación.

**Contexto:**
- **El resultado:** nc1 `R20261118T032437Z-2812`, sobre las entradas de la verificación `R20261118T031948Z-a6f6`, dio **CONTROL NOT PASSED**:
  - `EXECUTION_BLOCKED` / STOP, con S-04;
  - primera en fail: Handoff;
  - en fail: Handoff, Identity, Remote, Scope, Ci, Tests y Trailer;
  - en pass, igual que en la verificación real: Termination, Authority, Contract, CleanTree, Routing, FreeText y Denials.
- **La evidencia** de cada una de las siete comprobaciones en fail nombra el CurrentSha falso (`deadbeef…`) o dice que no se puede resolver.
- **Lo que admite la firma:** la de PD-F4-04, que PBTC-C11 transcribe literal, solo admite como diferencias Handoff, Identity y Ci, con Remote en pass. Por PBTC-C11 y [DP] (pregunta 1: «Solo con la firma EXACTA»), la variación vuelve al Owner antes de cualquier sustituto.
- **El resto de la cadena:** nc2 `R20261118T032822Z-2db5` y nc3 `R20261118T033422Z-412a` dieron CONTROL PASS ordinario.

La pregunta y la respuesta se transcriben **literales**.

## La pregunta

> nc1 es un control que le entrega al Controller una entrega con un SHA falso, para comprobar que lo detecta. Lo detectó y paró con S-04, que es su objetivo. La firma autorizada (PD-F4-04) solo admite que fallen Handoff, Identity y Ci, con Remote en pass. Esta vez fallaron además Remote, Scope, Tests y Trailer, y la evidencia de cada uno dice que falla solo porque el SHA declarado no existe. No es la firma exacta, así que por tu regla no aplico el sustituto por mi cuenta. nc2, nc3, la verificación (14/14) y todo lo demás de PB-TC pasaron. ¿Qué hago?

## Las opciones

- **«Aceptarlo solo esta vez (Recomendado)»:** Dejo constancia de que nc1 cumplió su objetivo: todos los fallos dependen del SHA falso y paró con S-04. Lo registro como decisión tuya para este caso concreto, sin ampliar las firmas autorizadas, y cierro PB-TC.
- **«Repetir nc1 una vez»:** Conservo este resultado tal cual y lanzo un nc1 nuevo (unos 5 minutos). Si da un CONTROL PASS normal o la firma exacta, cierro PB-TC. Si no, vuelvo a ti.
- **«Ampliar la firma para F4B»:** Admitir desde ahora en F4B que fallen todas las comprobaciones que dependan del SHA falso, siempre que su evidencia lo nombre. Evita esta pregunta en el futuro, pero relaja tu regla de «firma exacta».

## La respuesta del Owner

«Aceptarlo solo esta vez (Recomendado)».

## Alcance

- **El registro:** `nc1 = SATISFIED_BY_OWNER_DISPOSITION (F4B-E08)`, solo para esta ocurrencia (nc1 `R20261118T032437Z-2812` de PB-TC, intento 1).
- **La salida ordinaria del control** se conserva tal cual; no se ejecuta ningún sustituto.
- **Las firmas autorizadas no cambian:** PD-F4-01 y PD-F4-04 siguen siendo EXACTAS. Una variación futura vuelve al Owner.
- **PB-TC se cierra** sobre `4440f67d`, con attempts 1 de 3.
