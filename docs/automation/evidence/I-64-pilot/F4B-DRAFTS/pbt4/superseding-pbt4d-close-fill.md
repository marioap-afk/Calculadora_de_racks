# La tabla de ejecución de PB-T4d en el registro declarado, que se completa al cerrar

**Qué es:** la sección «PB-T4d» de `docs/automation/evidence/I-64-pilot/F4B-DRAFTS/superseding-characterization.md`, la retirada del piloto con las 271 filas con acción de su lista, dice que «la ejecución se completa al cerrar». Este cierre (F4B-E59) la completa: sustituye exactamente cinco líneas, leídas de este archivo, y no cambia nada más.

**La fuente de cada dato:**
- el relevo del Worker `R20261122T173325Z-c9e1` y la verificación `R20261122T194536Z-4c59`, custodiados en este cierre;
- las TRX de las dos CI, con los recuentos por clase del RED y del GREEN;
- la revisión independiente del código y las decisiones del Owner sobre el código (`F4B-DRAFTS/coordinator/owner-pbt4d-code-decisions.md`).

## Las cinco sustituciones (línea vieja → línea nueva, exactas)

1. La línea del estado.

Vieja:

```text
**Estado: EMITIDA** (2026-11-22, emisión de PB-T4d, F4B-E56, sobre el cierre de PB-T4c `6c2820ba`). Lo escribe la sesión principal; el Worker no escribe en este archivo. La ejecución se completa al cerrar.
```

Nueva:

```text
**Estado: EJECUTADA** (2026-11-22, cierre de PB-T4d, F4B-E59; emitida en F4B-E56): RED `aa65ecfde87bfe274e9ba518f63dfe94c9e9f61a`, GREEN `7db6fd0c4e4a27ec05d56cce0daaf0883940a4e2`, sin reparación del RED. Lo escribe la sesión principal; el Worker no escribe en este archivo.
```

2. La fila del RED.

Vieja:

```text
| **RED** | (al cerrar) |
```

Nueva:

```text
| **RED** | `aa65ecfde87bfe274e9ba518f63dfe94c9e9f61a` (CI `45054587789`: tests en failure por CT-PBT4D-01, la única clase con `ExpectRed`; ui-tests y build-ui en success; build-plugin omitido. Core 19 418 = 19 014 Passed + 1 Failed + 403 Skipped; UI 2 464 = 2 416 + 0 + 48). Solo `tests/`: 31 rutas (23 M, 5 A y 3 D). Cada archivo es el texto literal de la pieza `pbt4d-red-v54.txt` (`red_full` de `red_pbt4d_v53.py` sobre BaseSha), y los 3 que borra no están; en `WorkspacePilotUpdateRunTests.cs` es el de la pieza v5.4 (`7ea6170422f48334f18ff2cd3b1cb1b32c79264861dd52a6e6b56570daf7ba13`), no el `RedFiles` de la v5.3 de la precondición 6 (M-16 de la emisión). C02 de la retirada: 138 pruebas borradas y 45 cambiadas, cada una con su fila de la lista, y ninguna otra (las 29 de WPUR cuentan como cambiadas porque su sustituta lleva el mismo nombre; en WPUR se borran); las 13 filas que se quedan, sin tocar. Los 3 `Skip`, en la línea de atributo de su prueba y con su motivo literal. La prueba lenta, acotada (D-S14): 7,6 s en esta CI. Sin reparación del RED |
```

3. La fila del GREEN.

Vieja:

```text
| **GREEN** | (al cerrar) |
```

Nueva:

```text
| **GREEN** | `7db6fd0c4e4a27ec05d56cce0daaf0883940a4e2` (CI `45057868915` 4/4; Core 19 418 = 19 015 Passed + 0 Failed + 403 Skipped; UI 2 464 = 2 416 + 0 + 48). Solo `src/RackCad.Application/Systems/PushBack/Pilot/`: 4 archivos cambian y 2 se borran (`PushBackPilotStateComparer.cs` y `PushBackPilotFieldAdapter.cs`), con el texto literal de la pieza `pbt4d-green-v54.txt` (`green` de `green_pbt4d_v53.py`), iguales a `GreenFiles` y `GreenDeleted` de la precondición 6. Salen exactamente las 57 declaraciones de C13, y `allowErase` ya no aparece en `src/`; ninguna prueba de `tests/` cambia tras el RED. La prueba lenta acotada: 9,2 s en esta CI |
```

4. La fila de las sustitutas y los equivalentes.

Vieja:

```text
| **Las sustitutas y los equivalentes, en el RED y en el GREEN** | (al cerrar) |
```

Nueva:

```text
| **Las sustitutas y los equivalentes, en el RED y en el GREEN** | Los 5 archivos nuevos son el texto de la pieza, sin cambio después del RED; las tres clases que no son CT, byte a byte su `NewFileSha`. En el RED pasan las tres, con `ExpectRed` false: `PushBackRetirePilotRunTests` 31 (las 29 sustitutas de WPUR; la n.º 42 es una `Theory` de 3), `PushBackRetirePilotFullCopyGuardTests` 1 y `PushBackRetirePilotPlanGuardTests` 2. De las CT, cae por aserción `PushBackRetirePilotGuardTests` (CT-PBT4D-01, 1 Failed, `ExpectRed` true) y pasa `PushBackRetirePilotCensusGuardTests` (CT-PBT4D-02, 1). En el GREEN pasan las 36. En el RED, 12 de las 14 autopruebas de CT-PBT4D-01 discriminan: las dos de archivo no, porque el archivo que inyectan aún existe (M-02; límite del diseño, ratificado por el Owner en F4B-E59); en el GREEN discriminan las 14. Las 3 de la lista con `Skip` quedan NotExecuted con su motivo en las dos ramas, todas en Core (`PushBackEditAllFullCopyGuardTests` 1 y `WorkspacePilotPrepareGuardTests` 2); los Skipped de Core, 403, son esas 3 y los de la línea base que se quedan, y en UI los 48 de la línea base. Ninguna prueba falla fuera de CT-PBT4D-01 (C02, C07 y C16) |
```

5. La fila del estado.

Vieja (la única `| **Estado** | EMITIDA |` del archivo):

```text
| **Estado** | EMITIDA |
```

Nueva:

```text
| **Estado** | EJECUTADA |
```
