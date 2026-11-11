# I-64 — F4-T1-DRAFT-CORE: paquete de cierre

Preparado por la sesión responsable, bajo las órdenes del Owner de F4 recogidas en `I-64-pilot/F4-DRAFTS/coordinator/`. Las decisiones están en `I-64-pilot/F4-DRAFTS/f4-dispositions.md`, de F4-E07 a F4-E17.

Este paquete cierra una tarea, no un gate. No declara GATE PASS.

## 1. Estado resultante

| Hecho | Valor |
|---|---|
| Producto | `ce73ceb719cbc08ee4b1ee0f989ac2e5b4258649`, CI `43029959059` push 4/4 |
| Verificación | `R20261111T181015Z-1f4f`: `EXECUTION_VERIFIED` / NONE, 14/14 |
| Cadena | ChainBaseSha `13775d8f`; intento 1: RED `a362744c`, GREEN `aefc34a5`; intento 2: RED `500a336d`, GREEN `09955667`; intento 3: BaseSha `d737b485`, RED `5a27ac8b`, GREEN `ce73ceb7` |
| Intentos | 3 de 3; no existe intento 4 |
| Punta de custodia antes de este cierre | `61d83788` (solo `docs/` encima de `ce73ceb7`) |
| Config | Snapshot G (`7C1516A9…`), aceptada tras la sonda `32f7` |
| nc1 | `bc22` CONTROL PASS (`c4f7` CONTROL NOT PASSED, conservado) |
| nc2 | SATISFIED_UNDER_F4T1_SINGLE_PATH_SCOPE_SUBSTITUTE (`5ed5`; `50da` CONTROL NOT PASSED, conservado) |
| nc3 | SATISFIED_UNDER_F4T1_FREETEXT_SUBSTITUTE (`b8f7`; `2f8e` CONTROL NOT PASSED, conservado) |

## 2. Corridas

| Corrida | Resultado | Notas |
|---|---|---|
| `e455` | EXECUTION_VERIFIED, 14/14 | intento 1, `aefc34a5`; las revisiones hallaron I-1 a I-4 (F4-E07) |
| `0d3b` | EXECUTION_VERIFIED, 14/14 | intento 2, `09955667`; la revisión halló N-1 y N-2 (F4-E08) |
| `5556` | EXECUTION_BLOCKED / STOP | intento 3; error de entrada de la sesión (AuthorityRevision) |
| `ce94` | EXECUTION_REWORK_REQUIRED | intento 3; término de 16.10 dentro de un nombre de archivo citado; se reemitió la entrega (F4-E12) |
| `4bb6` | EXECUTION_BLOCKED / STOP | reverificación; el prompt pedía `git ls-remote` a un Controller sin red |
| `1f4f` | EXECUTION_VERIFIED, 14/14 | reverificación final con la sustitución de Identity tras la custodia |

## 3. Pruebas sobre `ce73ceb7` (TRX de la CI)

| Conjunto | Resultado |
|---|---|
| Filtro Core Workspace | 1300/1300 |
| Core completa | 16838/16838 |
| RED `5a27ac8b` | 13 fallidas, todas del bloque de regresión de N-1 y por aserción |
| `n1_hashes.py` | Pass (H1, H2 y H3) |

## 4. Producto entregado

El núcleo puro de los borradores en `RackCad.Application.Workspace.Drafts`:

- `DraftSession<TState>`, con comparador y copiador inyectados. El inicial y el actual nunca comparten objeto.
- Texto pendiente con compromiso en dos fases y orden ordinal.
- StaleBase de cuatro valores. Una clave ambigua nunca coincide, y Blocking no conserva el texto del sobre.
- Máquina de estados de Actualizar con la clasificación de D-13. La ranura única pertenece a la operación, y un error durante Aplicando queda diferido.
- Registro por (sesión, rack), con retirada y aviso de pérdida.
- Veto de cierre decidido desde memoria.
- Guardas de pureza, de nombres (incluida la regla BOM sin distinguir mayúsculas) y de preview.

## 5. Abierto

Lo que pasa a tareas posteriores está en F4-E16 y F4-E17. Destaca N-2 (anti-alias del estado de la lectura), obligatoria en F4-T2. Sin AutoCAD: el comportamiento en host lo cubren F4-T2 a F4-T4 y Smoke-2.
