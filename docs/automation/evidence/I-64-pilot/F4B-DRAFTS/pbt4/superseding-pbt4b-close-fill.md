# La tabla de ejecución de PB-T4b en el registro declarado, que se completa al cerrar

**Qué es:** la sección «PB-T4b» de `docs/automation/evidence/I-64-pilot/F4B-DRAFTS/superseding-characterization.md`, la sustitución de las 478 pruebas de la lista por sus copias y sus equivalentes, dice que «la ejecución se completa al cerrar». Este cierre (F4B-E53) la completa: sustituye exactamente cinco líneas, leídas de este archivo, y no cambia nada más.

**La fuente de cada dato:**
- el relevo del Worker `R20261121T085458Z-5ef5` y la verificación `R20261121T133638Z-1b82`, custodiados en este cierre;
- `rr_check` sobre la reparación del RED;
- las TRX de las tres CI, con los recuentos por clase del GREEN.

## Las cinco sustituciones (línea vieja → línea nueva, exactas)

1. La línea del estado.

Vieja:

```text
**Estado: EMITIDA** (2026-11-21, emisión de PB-T4b, F4B-E52, sobre el cierre de PB-T4a `78a7cef3`). Lo escribe la sesión principal; el Worker no escribe en este archivo. La ejecución se completa al cerrar.
```

Nueva:

```text
**Estado: EJECUTADA** (2026-11-21, cierre de PB-T4b, F4B-E53; emitida en F4B-E52): RED `c1e4f4585217abc8c85d90b499a07bb93c84be2c`, reparación del RED `9b2d18937d3aa03b4cbeaf0546cd0e6b44a4f6fe`, GREEN `c065080d3fd1dd8bb1937b98b81e9a0c1714777b`. Lo escribe la sesión principal; el Worker no escribe en este archivo.
```

2. La fila del RED.

Vieja:

```text
| **RED** | (al cerrar) |
```

Nueva:

```text
| **RED** | `c1e4f4585217abc8c85d90b499a07bb93c84be2c` (CI `44893322578`: tests y ui-tests en failure por las clases con `ExpectRed`; build-ui en success; build-plugin omitido. Core 19 605 = 18 841 Passed + 306 Failed + 458 Skipped; UI 2 481 = 2 430 + 3 + 48). Los 478 `Skip` van en los 21 archivos de la lista, solo en la línea del atributo y con el motivo literal de su causa; las 21 copias y el kit son, byte a byte, los de la precondición 6 (`CopySha`). Reparación del RED `9b2d18937d3aa03b4cbeaf0546cd0e6b44a4f6fe` (RR, F4B-E35): solo `tests/RackCad.Tests/PushBackFullPanelTests.cs`, CT-PBT4B-08; CI `44899232904` con los mismos 306 + 3 fallos; `rr_check` PASS |
```

3. La fila del GREEN.

Vieja:

```text
| **GREEN** | (al cerrar) |
```

Nueva:

```text
| **GREEN** | `c065080d3fd1dd8bb1937b98b81e9a0c1714777b` (CI `44903818806` 4/4; Core 19 605 = 19 147 Passed + 0 Failed + 458 Skipped; UI 2 481 = 2 433 + 0 + 48). Toca solo nueve archivos de código de `src/`, iguales byte a byte al GREEN literal del paquete; ninguna prueba de `tests/` cambia tras la reparación del RED, y ningún archivo de `Pilot/` |
```

4. La fila de los recuentos.

Vieja:

```text
| **Los recuentos por clase, en el GREEN** | (al cerrar) |
```

Nueva:

```text
| **Los recuentos por clase, en el GREEN** | Las 478 de la lista, en Skipped con su motivo: 451 en Core y 27 en UI, las mismas por clase que la tabla de arriba. Los Skipped de las CI son esas más los de la base: 7 en Core (D-1 y CT-PBT3A-12) y 21 en UI. Las copias pasan enteras: en Core, 614 filas en 29 clases `FullPanelCopy<Clase>RedTests` y `FullPanelCopy<Clase>GreenTests` de 19 archivos; en UI, 36 en 2 clases. Las 21 CT pasan: `PushBackFullPanelTests` 11, `PushBackFullPanelGuardTests` 4, `PushBackFullPanelCensusGuardTests` 1, `PushBackPanelDraftGuardTests` 1, `WorkspaceFullCopyGuardTests` 1 (su comprobación (5), acotada a las nueve clases de C-0 por la errata que ratificó el Owner) y `PushBackFullPanelFieldTests` 3 |
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
