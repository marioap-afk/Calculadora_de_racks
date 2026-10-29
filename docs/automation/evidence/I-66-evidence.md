# I-66 — Evidencia

Unit: `I-66`. Rama `fix/placedclone-copia-anotaciones`. Decisiones: [decisions/I-66.md](../decisions/I-66.md). Los registros en sombra del piloto R1a
de I-62 están aparte, en [`I-66-R1/`](I-66-R1/README.md).

## 1. Comprobación por mutación de las pruebas del Freeze R2 (antes de la re-revisión del Architect)

Copia desechable de `2ba93704` fuera del worktree, con la prueba `tests/RackCad.Tests/I66PlacedCloneTests.cs` de R2 y SDK 8.0.423 de usuario.

| Variante de `PlacedClone` | Fallan / pasan | Pruebas que fallan |
|---|---|---|
| código de la base (`2ba93704`) | 12 / 4 | N-1, N-2, N-3, N-4, N-7 y N-8 (dos casos cada una), por campos que quedan por defecto; pasan N-5 (3) y N-9 |
| GREEN previsto | 0 / 16 | — |
| M1: empate contado como vertical (`<=`) | 2 / 14 | N-7 |
| M2: sin la condición de rol | 2 / 14 | N-8 |
| M3: `Text` solo para anotaciones | 1 / 15 | N-8 |
| M4: estilo solo para cotas | 2 / 14 | N-8 |
| M5: desplazamiento nunca invertido | 2 / 14 | N-3 y N-4 |

Core completo sobre la misma copia con el GREEN de la versión R1 de la prueba (11 casos nuevos): 12 606 correctas, 0 con error. Las pruebas existentes
no cambian (F-4).
