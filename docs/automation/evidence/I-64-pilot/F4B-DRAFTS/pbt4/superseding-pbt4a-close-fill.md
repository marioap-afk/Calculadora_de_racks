# La tabla de ejecución de PB-T4a en el registro declarado, que se completa al cerrar

**Qué es:** la sección «PB-T4a» de `docs/automation/evidence/I-64-pilot/F4B-DRAFTS/superseding-characterization.md`, la sustitución de CT-PBT3A-12 por CT-PBT4A-06, dice que «la ejecución se completa al cerrar». Este cierre (F4B-E51) la completa: sustituye exactamente cinco líneas, leídas de este archivo, y no cambia nada más.

**La fuente de cada dato:** el relevo del Worker `R20261120T193653Z-de54` y la verificación `R20261120T213256Z-8d11`, custodiados en este cierre, y el GREEN `faef7020` (`tests/RackCad.Tests/PushBackFullReadGuardTests.cs:98`).

## Las cinco sustituciones (línea vieja → línea nueva, exactas)

1. La línea del estado.

Vieja:

```text
**Estado: EMITIDA** (2026-11-20, emisión de PB-T4a, F4B-E50, sobre el cierre de PB-T3c `7f0d9aa3`). Lo escribe la sesión principal; el Worker no escribe en este archivo. La ejecución se completa al cerrar.
```

Nueva:

```text
**Estado: EJECUTADA** (2026-11-20, cierre de PB-T4a, F4B-E51; emitida en F4B-E50): RED `214f8c75f2ca5a248a5592075c3bac8e667bd362`, GREEN `faef702055990d1350a73bf4d254eb9e898d5670`. Lo escribe la sesión principal; el Worker no escribe en este archivo.
```

2. La fila del RED.

Vieja:

```text
| **RED** | (al cerrar) |
```

Nueva:

```text
| **RED** | `214f8c75f2ca5a248a5592075c3bac8e667bd362` (CI `44810371492`: tests en failure por las cinco CT que fallan por aserción; ui-tests y build-ui en success; build-plugin omitido. Core 19 137 = 19 125 Passed + 5 Failed + 7 Skipped: las seis de D-1 y CT-PBT3A-12, ya con su `Skip`; UI 2 451 = 2 430 + 21). El esqueleto del RED es, byte a byte, el del parche `pbt4a-skeleton.patch` |
```

3. La fila del GREEN.

Vieja:

```text
| **GREEN** | (al cerrar) |
```

Nueva:

```text
| **GREEN** | `faef702055990d1350a73bf4d254eb9e898d5670` (CI `44814418683` 4/4; Core 19 137 = 19 130 Passed + 0 Failed + 7 Skipped; UI 2 451 = 2 430 + 21). Toca solo las seis rutas de `src/` del alcance; ninguna prueba de `tests/` cambia tras el RED |
```

4. La fila de la sustituta.

Vieja:

```text
| **CT-PBT4A-06, en el GREEN** | (al cerrar) |
```

Nueva:

```text
| **CT-PBT4A-06, en el GREEN** | `PushBackFullReadGuardTests.CT_PBT4A_06_OnlyTheFolderOfTheModelThePlanTheRunAndTheAdapterOfThePluginNameItsTypes` (`tests/RackCad.Tests/PushBackFullReadGuardTests.cs:98`): pasa en el RED y en el GREEN, con el mismo censo que CT-PBT3A-12 admitiendo RIA. CT-PBT3A-12 sale en Skipped con su motivo literal en las dos CI |
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
