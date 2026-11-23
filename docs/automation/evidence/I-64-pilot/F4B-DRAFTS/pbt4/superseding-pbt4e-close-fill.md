# La tabla de ejecución de PB-T4e en el registro declarado, que se completa al cerrar

**Qué es:** la sección «PB-T4e» de `docs/automation/evidence/I-64-pilot/F4B-DRAFTS/superseding-characterization.md`, la recarga de «Editar todo» con la lista vacía, dice que «la ejecución se completa al cerrar». Este cierre (F4B-E61) la completa: sustituye exactamente cinco líneas, leídas de este archivo, y no cambia nada más.

**La fuente de cada dato:**
- el relevo del Worker `R20261123T045827Z-6c9e` y la verificación `R20261123T070111Z-b46b`, custodiados en este cierre;
- las TRX de las dos CI, con los recuentos por clase del RED y del GREEN;
- la revisión independiente del código y la revisión directa de la sesión.

## Las cinco sustituciones (línea vieja → línea nueva, exactas)

1. La línea del estado.

Vieja:

```text
**Estado: EMITIDA** (2026-11-23, emisión de PB-T4e, F4B-E60, sobre el cierre de PB-T4d `160e019d`). Lo escribe la sesión principal; el Worker no escribe en este archivo. La ejecución se completa al cerrar.
```

Nueva:

```text
**Estado: EJECUTADA** (2026-11-23, cierre de PB-T4e, F4B-E61; emitida en F4B-E60): RED `265f9e31cb33ce312c04d2d7959c999aee763b3c`, GREEN `0eac7911a025a59628b446db8b768035175f96de`, sin reparación del RED. Lo escribe la sesión principal; el Worker no escribe en este archivo.
```

2. La fila del RED.

Vieja:

```text
| **RED** | (al cerrar) |
```

Nueva:

```text
| **RED** | `265f9e31cb33ce312c04d2d7959c999aee763b3c` (CI `45119881969`: tests y ui-tests en failure, por las CT, las dos clases con `ExpectRed`; build-ui en success; build-plugin omitido. Core 19 421 = 19 015 Passed + 3 Failed + 403 Skipped; UI 2 465 = 2 416 + 1 + 48). Solo `tests/`: las 2 rutas nuevas (2 A, 662 líneas), cada una el texto literal de la pieza `pbt4e-red-v56.txt` (`red` de `red_pbt4e_v56.py` sobre BaseSha, con `apply_red_pbt4e_v56.py`), byte a byte su `NewFileSha` de la tabla de arriba; ningún archivo existente de `tests/` cambia, y `src/` es el de BaseSha (sin esqueleto). Caen por aserción, en el `Assert.Equal` de su `Finish`, las cuatro CT: CT-PBT4E-01, 02 y 04 en `PushBackEditAllReloadTests` (3 Failed) y CT-PBT4E-03 en `PushBackEditAllReloadFieldTests` (1 Failed), cada una con la primera diferencia que da la tabla del paquete. Sin reparación del RED |
```

3. La fila del GREEN.

Vieja:

```text
| **GREEN** | (al cerrar) |
```

Nueva:

```text
| **GREEN** | `0eac7911a025a59628b446db8b768035175f96de` (CI `45123886125` 4/4, con el Plugin; Core 19 421 = 19 018 Passed + 0 Failed + 403 Skipped; UI 2 465 = 2 417 + 0 + 48). Solo los 4 archivos de `src/` del alcance (`UNIT_CHANGE`), con 34 líneas añadidas y 7 quitadas: el texto literal de la pieza `pbt4e-green-v56.txt` (`green` de `green_pbt4e_v56.py`, con `apply_green_pbt4e_v56.py`), igual a `GreenFiles` de la precondición 6. En `WorkspaceHostCore.cs`, la recarga de `EditAllOfPilot` con `TryReloadPilotEdit`, tras las tres comprobaciones de la vista y antes de `ShowModal` (D-S17 y D-S19); en `WorkspacePilotFieldViewModel.cs`, el aviso de `EditAll` y la constante `NotReloadedText`, que también usa `BeginEdit`, con el mismo texto (D-S18); en `WorkspaceEditIntent.cs` e `IWorkspacePilotEditPort.cs`, solo comentarios. Una sola declaración nueva (C13), la constante; ninguna prueba de `tests/` cambia tras el RED |
```

4. La fila de las pruebas existentes.

Vieja:

```text
| **Las pruebas existentes, en el RED y en el GREEN** | (al cerrar) |
```

Nueva:

```text
| **Las pruebas existentes, en el RED y en el GREEN** | Ninguna se edita, se salta, se reactiva ni se borra: `git diff 7da361eb 0eac7911 -- tests/` solo añade los 2 archivos nuevos (la lista vacía, INV-PBT4E-07). Ninguna prueba existente falla, ni en la CI del RED ni en la del GREEN (C16). Los Skipped son los de la línea base, los mismos en las dos ramas: 403 en Core, con las 7 filas de D-1 y CT-PBT3A-12, y 48 en UI, sin ninguno de la lista. En el GREEN pasan las cuatro CT (`PushBackEditAllReloadTests` 3 y `PushBackEditAllReloadFieldTests` 1), sin ningún Failed en las dos suites, y los dos filtros con `ExpectRed` pasan tres veces seguidas en la corrida local del Worker. Cero `warning CS` |
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
