# La tabla de ejecución de PB-T3c en el registro declarado, que se completa al cerrar

**Qué es:** la sección «PB-T3c» de `docs/automation/evidence/I-64-pilot/F4B-DRAFTS/superseding-characterization.md` (D-2, sin `Skip`) dice «La ejecución se completa al cerrar». Este cierre la completa. Es la lección del cierre de PB-T3b, que dejó la suya sin rellenar y la completó la emisión de PB-T3c (F4B-E47).

**Qué se hace:** el commit de cierre (F4B-E48) sustituye exactamente cinco líneas de esa sección, leídas de este archivo. El guion de custodia comprueba que cada línea vieja está una sola vez en el archivo y dentro de la sección de PB-T3c, y que no cambia nada más.

**La fuente de cada dato:**
- los SHA, las CI y los totales: el relevo del Worker `R20261120T135026Z-80e5` y la verificación `R20261120T150458Z-e403`, custodiados en este cierre;
- los comentarios de WR: el GREEN `938a35f0` (`RackPilotUpdateWriter.cs:26-27` y `:238-240`) y la revisión independiente del código.

## Las cinco sustituciones (línea vieja → línea nueva, exactas)

1. La línea del estado.

Vieja:

```text
**Estado: EMITIDA** (2026-11-20, emisión de PB-T3c, F4B-E47, sobre el cierre de PB-T3b `fe1c67f4`). Lo escribe la sesión principal; el Worker no escribe en este archivo. La ejecución se completa al cerrar.
```

Nueva:

```text
**Estado: EJECUTADA** (2026-11-20, cierre de PB-T3c, F4B-E48; emitida en F4B-E47): RED `e0940a0efa314ea26777c59ab4e55c09d019bff6`, GREEN `938a35f0f1636b4306d844eb9e34000e8967c05e`. Lo escribe la sesión principal; el Worker no escribe en este archivo.
```

2. La fila del RED.

Vieja:

```text
| **RED** | (al cerrar) |
```

Nueva:

```text
| **RED** | `e0940a0efa314ea26777c59ab4e55c09d019bff6` (CI `44754080713`: tests en failure por CT-PBT3C-01 a 04, que fallan por aserción, y CT-PBT3C-05 pasa; ui-tests y build-ui en success; build-plugin omitido. Core 19 129 = 19 119 Passed + 4 Failed + 6 Skipped, los de D-1; UI 2 451 = 2 430 + 21). La fachada del RED es, byte a byte, la del parche `pbt3c-skeleton.patch` |
```

3. La fila del GREEN.

Vieja:

```text
| **GREEN** | (al cerrar) |
```

Nueva:

```text
| **GREEN** | `938a35f0f1636b4306d844eb9e34000e8967c05e` (CI `44757675821` 4/4; Core 19 129 = 19 123 Passed + 0 Failed + 6 Skipped, los de D-1; UI 2 451 = 2 430 + 21). Toca solo WR y la fachada; `WriterProhibitionViolations` (UpdateGuard:182-212) y G-9 siguen en verde |
```

4. La fila de los comentarios de WR.

Vieja:

```text
| **Los comentarios de WR, en el GREEN** | (al cerrar) |
```

Nueva:

```text
| **Los comentarios de WR, en el GREEN** | Cambiados como dice D-PBT3-11: el de cabecera (`RackPilotUpdateWriter.cs:26-27`) dice que el escritor borra una vista solo por la fachada caller-owned, dentro del único ámbito de MUTATE y solo las que pide el plan; el del ámbito de MUTATE (`:238-240`) lleva el texto literal del paquete («It never purges, renames or regenerates; it erases only the planned views, through the caller-owned facade») más una frase sobre POST. Ninguna guarda los lee |
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
