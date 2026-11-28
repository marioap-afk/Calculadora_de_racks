# La tabla de ejecución de PB-T3b en el registro declarado: lo que el cierre dejó sin rellenar

**Qué pasó:** el cierre de PB-T3b (F4B-E46, custodia `fe1c67f4`) no completó la sección «PB-T3b» de `docs/automation/evidence/I-64-pilot/F4B-DRAFTS/superseding-characterization.md`. El estado sigue en «EMITIDA» y las tres celdas de la tabla de ejecución, en «(al cerrar)». La tabla está pensada para eso («se completa al cerrar»), y los cierres de PB-TC y PB-TC2 la completaron. Es una omisión de la sesión, no un cambio de lo custodiado.

**Qué se hace:** el commit X de la emisión de PB-T3c (F4B-E47) sustituye exactamente cinco líneas de esa sección, y F4B-E47 lo declara. Ninguna otra línea del archivo cambia, salvo la entrada nueva de PB-T3c al final.

**La fuente de cada dato** (todos custodiados en `fe1c67f4`):
- los SHA y las CI: F4B-E46 en `f4b-dispositions.md` y el paquete de cierre de PB-T3b;
- la medición de INV-PBT3B-15: `tests/m1/m1.trx` de la entrega del Worker `R20261120T075251Z-a9e0`, por SHA-256 en su manifiesto, y la revisión independiente del código de PB-T3b.

## Las cinco sustituciones (línea vieja → línea nueva, exactas)

1. La línea del estado.

Vieja:

```text
**Estado: EMITIDA** (2026-11-20, emisión de PB-T3b, F4B-E44, sobre el cierre de PB-T3a `c40eea4a`). Lo escribe la sesión principal; el Worker no escribe en este archivo. La ejecución se completa al cerrar.
```

Nueva:

```text
**Estado: EJECUTADA** (2026-11-20, cierre de PB-T3b, F4B-E46, custodia `fe1c67f4`; emitida en F4B-E44 y reemitida en F4B-E45): RED `9fda277e5609e62ff715450eed03220c93df8585`, GREEN `c758278548bcdd66e7c55ca15ca1faeb1a431e53`. Lo escribe la sesión principal; el Worker no escribe en este archivo. La tabla de ejecución la completó la emisión de PB-T3c (F4B-E47), porque el cierre la dejó sin rellenar.
```

2. La fila del RED.

Vieja:

```text
| **RED** | (al cerrar) |
```

Nueva:

```text
| **RED** | `9fda277e5609e62ff715450eed03220c93df8585` (CI `44704503614`: tests en failure por los 13 métodos de `PushBackFullWrite` y los 6 de la equivalencia, que fallan por aserción; ui-tests y build-ui en success; build-plugin omitido; los seis métodos de D-1 en `Skipped`) |
```

3. La fila del GREEN.

Vieja:

```text
| **GREEN** | (al cerrar) |
```

Nueva:

```text
| **GREEN** | `c758278548bcdd66e7c55ca15ca1faeb1a431e53` (CI `44707482956` 4/4; Core 19 124: 19 118 Passed, 0 Failed y 6 Skipped, los de D-1, con la Theory S-2 como una fila; UI 2 451: 2 430 Passed y 21 Skipped) |
```

4. La fila de la medición.

Vieja:

```text
| **La medición sobre el GREEN** | (al cerrar) |
```

Nueva:

```text
| **La medición sobre el GREEN** | Conforme con la variante (A): S-1, S-3, S-4 y S-5 fallan; S-2 falla en las filas 1 y 3 y pasa en las 2, 4 y 5; S-6 pasa en vacío. Los `Skip` se restauraron antes del commit (TRX `tests/m1/m1.trx` de la entrega del Worker `R20261120T075251Z-a9e0`, por SHA-256 en su manifiesto) |
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

## Comprobaciones que hace `issue_pbt3c.py`

- Cada línea vieja está **una sola vez** en el archivo de `fe1c67f4`, y dentro de la sección «## PB-T3b — Las seis guardas de F4 que se sustituyen por D-1 (F4B-E29)». Si no, aborta.
- Tras sustituir, el archivo solo difiere del de `fe1c67f4` en esas cinco líneas y en la entrada nueva de PB-T3c, que va al final.
- `tests/m1/m1.trx` está en el manifiesto de custodia de la unidad `F4B-PBT3B-FULL-WRITE-RUN/2/R20261120T075251Z-a9e0` en `fe1c67f4`. Si no, aborta.
