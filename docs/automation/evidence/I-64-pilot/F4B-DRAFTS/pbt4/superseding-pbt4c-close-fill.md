# La tabla de ejecución de PB-T4c en el registro declarado, que se completa al cerrar

**Qué es:** la sección «PB-T4c» de `docs/automation/evidence/I-64-pilot/F4B-DRAFTS/superseding-characterization.md`, la sustitución de las 4 pruebas de la lista por sus equivalentes, dice que «la ejecución se completa al cerrar». Este cierre (F4B-E55) la completa: sustituye exactamente cinco líneas, leídas de este archivo, y no cambia nada más.

**La fuente de cada dato:**
- el relevo del Worker `R20261121T205731Z-41db` y la verificación `R20261121T234430Z-dd13`, custodiados en este cierre;
- las TRX de las dos CI, con los recuentos por clase del RED y del GREEN.

## Las cinco sustituciones (línea vieja → línea nueva, exactas)

1. La línea del estado.

Vieja:

```text
**Estado: EMITIDA** (2026-11-21, emisión de PB-T4c, F4B-E54, sobre el cierre de PB-T4b `6adff90d`). Lo escribe la sesión principal; el Worker no escribe en este archivo. La ejecución se completa al cerrar.
```

Nueva:

```text
**Estado: EJECUTADA** (2026-11-21, cierre de PB-T4c, F4B-E55; emitida en F4B-E54): RED `280c79beac61d31b03f86488c03686eef4f694bc`, GREEN `7cc1076eae8159294a1498ab556851977223b64c`, sin reparación del RED. Lo escribe la sesión principal; el Worker no escribe en este archivo.
```

2. La fila del RED.

Vieja:

```text
| **RED** | (al cerrar) |
```

Nueva:

```text
| **RED** | `280c79beac61d31b03f86488c03686eef4f694bc` (CI `44956801926`: tests y ui-tests en failure por las clases con `ExpectRed`; build-ui en success; build-plugin omitido. Core 19 619 = 19 149 Passed + 9 Failed + 461 Skipped; UI 2 486 = 2 432 + 5 + 49). Los 4 `Skip` van en los 4 archivos de la lista, solo en la línea del atributo y con su motivo literal (`check_skips_pbt4c_v50.py`: PASA); los 4 equivalentes son, byte a byte, su `EquivalentSha`; el esqueleto, 8 archivos de `src/`, es byte a byte el del registro de la precondición 6. Sin reparación del RED |
```

3. La fila del GREEN.

Vieja:

```text
| **GREEN** | (al cerrar) |
```

Nueva:

```text
| **GREEN** | `7cc1076eae8159294a1498ab556851977223b64c` (CI `44959653142` 4/4; Core 19 619 = 19 158 Passed + 0 Failed + 461 Skipped; UI 2 486 = 2 437 + 0 + 49). Toca solo 10 archivos de `src/`, y los 13 del GREEN literal del paquete (`pbt4c-green-v51.txt`) son byte a byte su texto; ninguna prueba de `tests/` cambia tras el RED, y ningún archivo de `Pilot/` |
```

4. La fila de los equivalentes.

Vieja:

```text
| **Los equivalentes, en el RED y en el GREEN** | (al cerrar) |
```

Nueva:

```text
| **Los equivalentes, en el RED y en el GREEN** | Los 4, nuevos y byte a byte su `EquivalentSha`, sin cambio después del RED. En el RED pasan los tres de Core, con `ExpectRed` false (`PushBackEditAllPortEquivalenceGuardTests`, `PushBackEditAllNameWritersGuardTests` y `PushBackEditAllFullCopyGuardTests`, 1 Passed cada uno), y cae por aserción el de UI, con `ExpectRed` true (`PushBackEditAllApplyCloseTests`, 1 Failed); en el GREEN pasan los cuatro. Las 4 de la lista quedan en Skipped con su motivo en las dos ramas, 3 en Core y 1 en UI; los Skipped de las CI son esas más los de la línea base (458 en Core y 48 en UI). Las CT: en el RED caen por aserción `PushBackEditAllTests` 6, `PushBackEditAllGuardTests` 3 y `PushBackEditAllWindowTests` 4, y pasan `PushBackOpenerClassicPathGuardTests` 1 y `PushBackDraftLaunchGuardTests` 1; en el GREEN pasan las 15 |
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
