# I-66 — Evidencia

Unit: `I-66`. Rama `fix/placedclone-copia-anotaciones`. Decisiones: [decisions/I-66.md](../decisions/I-66.md). Los registros en sombra del piloto R1a
de I-62 están aparte, en [`I-66-R1/`](I-66-R1/README.md).

## 1. Comprobación por mutación de las pruebas del Freeze, antes de las re-revisiones del Architect

**No es evidencia de gate.** Se hizo antes del Freeze, sobre una copia desechable de `2ba93704` fuera del worktree, para comprobar que las obligaciones de
prueba detectan las implementaciones incorrectas que señalan los hallazgos REQUIRED. El RED de gate es el commit RED de la rama (§2), y el GREEN y Core
Full de gate van sobre sus propios SHA. SDK 8.0.423 de usuario; prueba `tests/RackCad.Tests/I66PlacedCloneTests.cs` de la versión indicada.

**Pruebas de la versión R2 del Freeze** (16 casos):

| Variante de `PlacedClone` | Fallan / pasan | Pruebas que fallan |
|---|---|---|
| código de la base | 12 / 4 | N-1, N-2, N-3, N-4, N-7 y N-8 (dos casos cada una), por campos que quedan por defecto |
| GREEN previsto | 0 / 16 | — |
| M1: empate contado como vertical (`<=`) | 2 / 14 | N-7 |
| M2: sin la condición de rol | 2 / 14 | N-8 |
| M3: `Text` solo para anotaciones | 1 / 15 | N-8 |
| M4: estilo solo para cotas | 2 / 14 | N-8 |
| M5: desplazamiento nunca invertido | 2 / 14 | N-3 y N-4 |

**Pruebas de la versión R3 del Freeze** (18 casos: `Text` en las cotas de N-2 y N-3, y N-10):

| Variante de `PlacedClone` | Fallan / pasan | Pruebas que fallan |
|---|---|---|
| código de la base | 12 / 6 | N-1, N-2, N-3, N-4, N-7 y N-8 (dos casos cada una); pasan N-5 (3), N-9 y N-10 (2) |
| GREEN previsto | 0 / 18 | — |
| M1: empate contado como vertical (`<=`) | 2 / 16 | N-7 |
| M2: sin la condición de rol | 2 / 16 | N-8 |
| M3: `Text` solo para anotaciones | 7 / 11 | N-2, N-3, N-4 y N-8 |
| M4: estilo solo para cotas | 2 / 16 | N-8 |
| M5: desplazamiento nunca invertido | 2 / 16 | N-3 y N-4 |
| M6: los decorados no invierten el espejo (rama de texto de `PushBackMirror`) | 2 / 16 | N-10 |
| M7: rotación negada con el espejo | 3 / 15 | N-5 y N-10 |

Core completo sobre la misma copia con el GREEN y la versión R1 de la prueba (11 casos nuevos): 12 606 correctas, 0 con error. Las pruebas existentes
no cambian (F-4).

## 2. RED de G1 (commit `3581f65eecdc2c8bbd296dc2987ce0c00b2f45d6`)

- **Commit RED:** solo `tests/RackCad.Tests/I66PlacedCloneTests.cs`, sobre el Freeze `69d015ec` (blob congelado `15f41ab3`); el código de producto es el de
  la base `bb0d5522` (`git diff bb0d5522 3581f65e -- src` vacío).
- **Corrida local** en el worktree, árbol limpio antes y después, SDK 8.0.423 de usuario, filtro `FullyQualifiedName~I66PlacedCloneTests`:
  **12 con error, 6 correctas, 18 en total**. Fallan, dos casos cada una, N-1, N-2, N-3, N-4, N-7 y N-8, todas porque el clon deja los cuatro campos por
  defecto (`null`, `0`, `0`, `null`). Pasan las de conservación: N-5 (tres casos), N-9 y N-10 (dos casos). Coincide con el RED esperado del Freeze §6.
- **CI del commit RED:** se espera el job de pruebas Core en rojo por esas mismas pruebas; su run id se registra con el QH.

