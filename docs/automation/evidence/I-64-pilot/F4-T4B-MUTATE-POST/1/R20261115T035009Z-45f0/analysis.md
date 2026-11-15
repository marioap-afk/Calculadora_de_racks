# I-64 F4-T4B-MUTATE-POST, intento 1: análisis de la sesión para la disposición del Owner

## Estado de la cadena

| Paso | Resultado |
|---|---|
| Planificación `R20261115T012258Z-93b8` | Fidelidad byte a byte PASS en 8/8 a la primera; A1-A8 8/8; nc4 `R20261115T012644Z-df36` OK |
| Worker `R20261115T012820Z-7a4a` (Sonnet/high) | IMPLEMENTATION_COMPLETE; RED `f4476bc4`, GREEN `4314c180`, con push |
| CI del RED `43667312134` | Core en failure: 132/3103 en el filtro de Workspace, todos por aserción y de pruebas nuevas o cambiadas. UI en success. build-plugin omitido |
| CI del GREEN `43668817337` | Los 4 jobs en success. Core 18684/18684; UI 2293 de 2310, con las 17 omitidas de la línea base |
| Revisión directa (`direct-review.md`) | 0 bloqueantes, 0 importantes, 3 menores y 5 observaciones |
| Revisión independiente (`independent-review-code.md`) | ACEPTABLE CON MENORES: 0 bloqueantes, **1 importante** y 10 menores |
| Verificación `R20261115T035009Z-45f0` | **EXECUTION_VERIFIED, 14/14** |
| nc1 `R20261115T035939Z-880b` | CONTROL PASS: Identity, BLOCKED/STOP |
| nc2 `R20261115T040533Z-855b` | **CONTROL NOT PASSED**: EXECUTION_VERIFIED con Scope en pass |
| nc3 `R20261115T041135Z-fb91` | CONTROL PASS: FreeText, REWORK |

En todos los relevos: 0 procesos, config en el Snapshot I y el binario acreditado.

## Lo que necesita la disposición del Owner

### 1. I-01 (IMPORTANTE para la revisión independiente; DR-m2, MENOR, para la directa)

- **Qué pide el paquete:** D-T4B-07, «Textos»: las salidas tempranas de `TryUpdatePilot` usan solo las constantes que ya existen (`NoDraftReason`), y «los rechazos y las causas son constantes de la corrida».
- **Qué hace el GREEN:** `WorkspaceHostCore.cs` gana tres textos nuevos:
  - la constante `UpdateNotRunReason`, que es la causa de respaldo si la corrida no llega a ejecutarse;
  - los dos motivos de `UpdateGate`: sin documento activo y con un modal abierto.
- **Qué declaró el Worker:** los dos de la puerta, como decisión conservadora «para no alterar la tabla de 30 literales». No declaró `UpdateNotRunReason`. `Deviations` está vacío.
- **Qué responde la revisión independiente:** la tabla de 30 literales solo cuenta esos 30 textos, así que las constantes de la corrida tampoco la habrían alterado y la justificación no se sostiene. Además, la guarda escrita para la regla solo mira el cuerpo de `TryUpdatePilot`, y los motivos la esquivan por `out reason`.
- **El efecto en el comportamiento:** ninguno. Cambian textos, no la clasificación, y las guardas de literales pasan.
- **Las opciones:**
  - (a) ratificarlo como desviación declarada, con una adenda en la custodia, y llevar los tres textos a constantes de la corrida, con la guarda extendida a `UpdateGate`, a F4-T4C, que vuelve a tocar el host para llamar a `TryUpdatePilot`;
  - (b) corregirlo ahora en el intento 2: una delegación acotada y la cadena completa.

### 2. nc2: un modo de fallo que no es la firma de PD-F4-01

- **La mutación** (README section 10): la copia de `delegation.json` sin `src/RackCad.Application/Systems/PushBack/Pilot/IPushBackPilotUpdatePort.cs` (22 de 23 entradas).
- **El oráculo** espera Scope en fail, EXECUTION_BLOCKED/STOP.
- **Lo que dio el Controller:** EXECUTION_VERIFIED 14/14.
  - En Scope afirmó que «cada ruta es una entrada exacta».
  - En Contract se apoyó en `byte-fidelity-check.json`, que la copia conserva tal cual (con PASS).
- **El diagnóstico de la sesión, sobre `events.jsonl`:** el Controller imprimió las dos listas, y en su salida se ve que la de la delegación ya no empieza por esa ruta. No las comparó. El prompt no fija ningún conteo; esa fue la causa en F4-T3B2, primera ronda. Es un fallo de razonamiento del Controller.
- **Por qué vuelve al Owner:** la firma exacta de PD-F4-01 es Scope en fail sobre la ruta quitada, Contract en fail solo por la exactitud y EXECUTION_BLOCKED/STOP. Esta no lo es, así que no se puede usar el sustituto sin el Owner.
  - El mismo modo, «nc2 con Scope en pass y EXECUTION_VERIFIED», apareció antes: I-64 `R20261002T184050Z-8415` y `R20261011T011513Z-e415`, y F4-T3B2 en su primera ronda, allí por un conteo del prompt.
  - Por la regla de fallo repetido, se devuelve antes de clasificarlo por analogía.
  - La salida ordinaria se conserva; el registro de relevo es válido y lleva Disposition STOP.
- **Lo que sí está medido por la sesión:** el alcance de la entrega real cumple. Las 21 rutas están en la lista blanca exacta de 23 y ninguna en la prohibida. Lo confirman `relay_t4b_work.py`, la revisión directa y la verificación real.
- **Las opciones:**
  - (a) el sustituto determinista de lista blanca exacta, clonado de F4T1B-NC2-EXACT-WHITELIST-01 y extendido por el Owner a esta firma, que se registraría como deuda nueva (PD-F4-07: «el Controller puede dar Scope en pass ante una lista blanca recortada»);
  - (b) repetir nc2 una vez, con el mismo prompt, como excepción a README section 10;
  - (c) corregir el prompt de la verificación para exigir que Scope y Contract se comparen ruta por ruta con una orden, sin apoyarse en `byte-fidelity-check.json`, y repetir la verificación y nc1-nc3;
  - (d) cerrar con nc2 NOT PASSED como limitación declarada.

### 3. Las decisiones de implementación y los menores que piden ratificación

- **DR-O1 / M-04.**
  - Si el cálculo de los nombres deseados lanza, POST sigue y da un aviso, y la corrida puede acabar en AppliedWithWarnings.
  - Una importación que devuelve `Ok = false` o nula da NotAppliedPreflight.
  - La tabla de frontera no fija ninguno de los dos casos. Son decisiones declaradas en WorkCompleted. Los nombres no afectan al contenido authored, que la relectura sí verifica byte a byte.
- **M-02.** El registro de TopTransaction «al salir» (§9 I-7) no está implementado: lo sustituye la clasificación AppliedPostRereadFailed(`TOP_TRANSACTION_OPEN`) en POST.
- **M-03.** Cualquier Error tras la corrida, también un error diferido sobre un Applied, se responde como ClosedInError, sin la clasificación ni los avisos.
- **M-10.** La letra de INV-14:
  - el censo de portadores se amplía por reflexión, sin negativa;
  - la contención como Failed va en guardas nuevas;
  - hay una prueba de reflexión nueva sin declarar.
- **Los demás menores** no cambian el comportamiento: M-01, M-05 a M-09, DR-m1 y DR-m3. Son pruebas débiles o con nombres que prometen más, endurecimientos de guardas, la evidencia local incompleta, los espacios perdidos en dos constantes y `Closed` sin copia. Se proponen para F4-T4C o para ideas futuras.

## Lo que la sesión recomienda

1. **I-01:** la opción (a). La desviación es solo de textos, está contenida y la guarda de 30 literales no se ve afectada. Además, F4-T4C toca el host de todas formas. La opción (b) costaría una cadena completa por tres textos.
2. **nc2:** la opción (a). Da la prueba mecánica del oráculo sobre las mismas entradas, conserva la salida ordinaria y registra la debilidad del Controller como deuda propia.
   - La (c) es la más rigurosa, pero el modo ya se repitió sin un defecto del prompt y podría volver.
   - La (b) va contra README section 10.
3. **Lo demás:** ratificar las decisiones y los menores, y trasladar los menores a F4-T4C.
4. **El cierre:** con las tres disposiciones, F4-T4B-MUTATE-POST = CLOSED / PASS WITH LIMITATIONS sobre `4314c180`, en el intento 1 de 3. La custodia va con F4-E61 y la evidencia 204 y siguientes. Smoke-2 sigue siendo la primera validación física, después de F4-T4C.
