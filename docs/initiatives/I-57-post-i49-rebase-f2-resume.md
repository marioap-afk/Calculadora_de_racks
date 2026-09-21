# I-57 — relevo posterior a I-49 y reanudacion de F2

Fecha = 2026-09-19

Este registro acredita el relevo durable exigido antes de iniciar produccion F2. No cambia Proposal V5,
R3 ni ADR-0044 y no abre F3.

## Identidad del relevo

```text
CURRENT_MAIN        = a61850a6095cf8ebc7d9d92eab6dbbcc2343a6d1
I49_CANDIDATE_SHA   = 589e3db5536ae9cc9af9c051e694b4c1803f0196
I49_CLOSURE_SHA     = 469e2a03d565ada0ec7e150b406b096578843641
I49_RELAY_SHA       = a61850a6095cf8ebc7d9d92eab6dbbcc2343a6d1
I57_PRE_REBASE_TIP  = 0012c5818ac30003c0ef4a9cf89a4a59d028ca82
I57_POST_REBASE_TIP = b0f36cdbf404cca82e29d1e3ca933b4ed4b4671d
I52_TIP             = ebdb358b
I55_TIP             = c20173cb
```

I-49 esta integrada en `main`; su Candidato es ancestro de su cierre y el cierre es segundo padre del
merge. La rama y el worktree de I-49 ya no existen. Los worktrees de `main`, I-57, I-52 e I-55 estaban
limpios al censarlos y no habia una operacion Git incompleta en I-57. El CI de `main` sobre
`a61850a6095cf8ebc7d9d92eab6dbbcc2343a6d1` fue `35483512657`, `success`.

## Auditoria de la autoridad compartida

I-49 agrego `PlanReadSet`, `RecoveryAssessment` y contratos G9, y cambio la preparacion del commit de
`ProjectVariableMutationExecutor`: reescanea los envelopes dentro de la transaccion caller-owned,
acredita el read-set contra el estado vigente y solo escribe el registry cuando existe una mutacion real.
Conserva rollback, purge y regen de la autoridad vigente.

La clasificacion persistida de vistas dentro de `ProjectVariableMutationExecutor` no cambio: lateral
explicita, planta por `IsPlantaView`, resto frontal y `Section` frontal negativa coercionada a fondo cero.
El cambio de `RackSelectivoCommands` de I-49 solo transporta las opciones formula-aware hacia la UI; no
cambia el contrato de `View`/`Section`. `PlanReadSet`, `RecoveryAssessment` y los DTO/resultados nuevos no
redefinen syntax, address ni availability de AUTH-01..04.

```text
shared authority status = STABLE AFTER I-49 MERGE
classification          = MATERIAL BUT COMPATIBLE
V5 still valid          = YES
Proposal V6 required    = NO
```

## Equivalencia normativa posterior al rebase

El rebase sobre `origin/main` conservo los objetos aceptados byte por byte:

```text
Proposal V5 blob       = c50141f425d191a07c608693b099e5a018fd15e0
CT-04 V2 matrix blob   = cbdca0eb7f16daa0d44319899c4d143452d530af
CT-04 V2 evidence blob = c5f5791796b31d382e9fb6ca85611ac1008cb97a
R3 blob                = cd42db03becff42f98b047e61c46689c17a69670
ADR-0044 blob          = 30eacca0dc43263100ee03b0cd3972b175a879b9
I-57 contract blob     = a58428017b889cfcdf4805b660ae43d6590b55d0
Claim-Id               = 3e77d0e4-67de-4c3a-b3ed-ee65f0ac7cd3
```

## Evidencia del SHA de inicio

Sobre `b0f36cdbf404cca82e29d1e3ca933b4ed4b4671d`, con arbol limpio:

```text
CT-04 V2 focal              = 7/7 PASS
I-49/PVME concurrency focal = 123/123 PASS
Shared View Foundation      = 33/33 PASS
CI push run                 = 35485101533
CI head_sha                 = b0f36cdbf404cca82e29d1e3ca933b4ed4b4671d
CI required jobs            = 4/4 SUCCESS
```

Los cuatro jobs fueron Core, UI, build UI y build Plugin without AutoCAD. Los avisos xUnit existentes
siguen siendo deuda registrada y no son fallos de esta evidencia.

## Resultado

```text
F2 execution can resume = YES
F2_START_SHA            = b0f36cdbf404cca82e29d1e3ca933b4ed4b4671d
F2 scope                = AUTH-01..04 ONLY
F3                      = NOT OPEN
```

La implementacion migra `ProjectVariableMutationExecutor` al final, conserva intacta la semantica de
concurrencia de I-49 y no usa `0012c5818ac30003c0ef4a9cf89a4a59d028ca82` como inicio productivo.
