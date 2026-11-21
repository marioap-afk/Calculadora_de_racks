# PB-T4c: la precondición 6 del paquete de PB-T4 v5.0

**Base:** el cierre de PB-T4b, `6adff90d856026a7a51067c1e9d7248f3d81641c` (F4B-E53). Cada corrida, en una copia exportada nueva (`git archive` del checkout principal; nada más en él), fuera de todo worktree. SDK de usuario (`<datos-locales-del-usuario>\Microsoft\dotnet\dotnet.exe`), con `--no-incremental -nodeReuse:false -p:UseSharedCompilation=false`; pruebas en Debug y el Plugin en Release con `-o`; cero `warning CS`; la suite **completa** de Core y, en UI, **solo los filtros** del contrato; `build-server shutdown` al final. Ninguna corrida toca el worktree, y el checkout principal queda limpio en todas (`MAINCO []`).

**El resumen:** `pbt4c-precondition6.json` (Schema `rackcad-pbt4c-precondition6/v1`), sha256 `0cb67aee2b2d925d68235bb6444566b4222205a6cf97f472b3851fd0795b2f51`, que escribe `p6_json_pbt4c_v50.py` (`963e80ff…0a91`) desde los registros de abajo.

## Las corridas que valen

**Las piezas** (sha256 del texto con `\n`): `pbt4c-green-v50.txt` `6b304a009d05ff586603ed4bbb9fa69b9973977bea09222c688ef80faa983e1b`, `pbt4c-skeleton-v50.txt` `48e097a7a579bd311fdeb2f6b87ff57ec0ada157573b174ad3a02935e5ab816c`, la lista `pbt4c-substitution-list-4.tsv` `9bd4c17a0eda837c92a7a0b7b3562d4ab17f1608baf3ffa8efaf560d8b1f82de`; los demás guiones, en la cabecera de cada registro (`pieza <sha256> <nombre>`). El conductor: `p6_pbt4c_v50b.sh` (`6dab6c0f…a397`), con `p6_check_pbt4c_v50b.py` (`bb2c983b…8486`) y `ui_filters_pbt4c_v50b.txt` (los 17 filtros de UI heredados de PB-T4b y el del equivalente de CT-PBT2B-06).

### `/d/r5g`: los pasos 2 a 6, el RED exacto y el GREEN literal, sin las CT

Registro: `driver-redgreen-v50b-r5g.txt` (`f2f5d7ef…ae1c`) y `out-redgreen-v50b-r5g/`.

- **El RED** (`apply_red_pbt4c_v50.py`): el esqueleto (4 existentes y 4 nuevos), los 4 `Skip` y los 4 equivalentes, con el sha256 de cada archivo en `apply-red.txt`. La compilación: Core, UI y el Plugin, 0 errores y 0 `warning CS`.
- **Core, RED:** **19 608 = 19 147 `Passed` + 0 `Failed` + 461 `Skipped`** (TRX `46aeb97b…36e4`, 785 s). Las 461 omitidas son las 458 de la base y las 3 de Core de la lista, con su motivo literal; los 3 equivalentes de Core pasan.
- **UI, RED:** los 18 filtros; **solo cae el equivalente de CT-PBT2B-06** (`PushBackEditAllApplyCloseTests`), por aserción (la primera línea del StackTrace, bajo `tests/`), como se espera; CT-PBT2B-06 omitida con su motivo literal; ninguna otra cae (`check-ui-red.txt`).
- **El GREEN** (`apply_green_pbt4c_v50.py`): 9 existentes y 4 nuevos, con el sha256 de cada archivo en `apply-green.txt`. La compilación: 0 errores y 0 `warning CS`.
- **Core, GREEN:** **19 608 = 19 147 `Passed` + 0 `Failed` + 461 `Skipped`** (TRX `c6df1250…bb09`, 591 s).
- **UI, GREEN:** los 18 filtros, **0 `Failed`**; el equivalente de CT-PBT2B-06 pasa (`check-ui-green.txt`).
- **D = 0, K = 4 y E = 4.**
- **Los `Skip`** (`check_skips_pbt4c_v50.py` sobre la copia): PASA, 4 archivos con su `Skip` literal y nada más (`check-skips.txt`, `4d74c67e…7885`).
- **Los filtros** (`filter_overlap_pbt4c_v50b.py`, `d5ebc82f…6265`, sobre las TRX del GREEN; D-S8): PASA. Cada filtro nuevo selecciona solo su clase y ningún heredado selecciona una clase nueva. Los heredados que seleccionan una prueba de la lista: n.º 7 (`PushBackSharedReferenceGuardTests`), **n.º 12 (`RackCad.Tests.Workspace`: PEG y `WorkspaceFullCopyGuardTests`)**, n.º 36 (`PushBackWorkspaceApplyCloseTests`) y n.º 43 (`WorkspaceFullCopyGuardTests`, que queda en 0). `filter-overlap-v50b.txt`, `b1bacf8c…2083`.
- **La duración de la suite de Core:** 785 s en el RED y 591 s en el GREEN (la del RED, con el paseo de guardas corriendo a la vez).

### `/d/r5f`: el paso 7, las sondas de las líneas de las CT

Registro: `driver-probes-v50b-r5f.txt` (`68f7250d…497b`) y `out-probes-v50b-r5f/`. RED y GREEN aplicados; las dos sondas como clases temporales (`TmpPbt4c…`), que no entran en ningún commit y acaban en `Assert.Fail`, solo con su filtro; 0 `warning CS`.
- **Core** (`pbt4c-probe-editall.cs`): las líneas de CT-03, 04, 05 y 12 (`probe-core-lines.txt`, `cbbd8457…a59f`).
- **UI** (`pbt4c-probe-close-ui.cs`): CT-09 y D-S10; en WORKSPACE, escenificar un módulo deja siempre cambios sin aplicar (`probe-ui-lines.txt`, `3ee13f60…85ce`).

### `/d/r5h`: el coste de `IsDirty`

Registro: `driver-isdirty-v50-r5h.txt` (`ba65fabb…0546`) y `out-isdirty-v50-r5h/`. La sonda de PB-T4b (`pbt4bcc/compile/TmpIsDirtyCostProbe.cs`, sin tocarla) sobre el GREEN de PB-T4c, en Release y sola en la máquina:

| Escenario | Limpio, mediana (p95) | Sucio, mediana (p95) | En PB-T4b (`pbt4bcc/compile/out-probes-v48-r4s`), limpio y sucio |
|---|---|---|---|
| `pb-01-base` | 3.32 ms (5.94) | 3.05 ms (4.00) | 2.94 y 2.89 ms |
| `pb-07-combinado` | 4.23 ms (5.50) | 4.22 ms (5.84) | 3.93 y 3.94 ms |
| `pb-20-ab` | 9.64 ms (12.08) | 9.66 ms (13.11) | 9.35 y 9.46 ms |

PB-T4c no cambia `IsDirty` ni su comparador: la diferencia es la de la máquina, dentro del ruido.

## La vuelta parada (historia)

Las corridas de la primera vuelta, con las piezas de antes de D-S9 y con la lista de 2 y de 3. **No valen para la emisión;** se custodian como historia.

| Copia | Qué midió | Lo que dio | Registro |
|---|---|---|---|
| `/d/r5a` | La sonda de la lista: solo los dos `Skip` de la lista de 2, sin GREEN | Core 19 605 = 19 144 + 1 + 460; cae CT-PBT4B-20 → `owner-pbt4c-list-3-decision.md` | `pbt4/v50/probe-skips-r5a-driver.txt` (`6a7de01d…8c66a8c`) y `pbt4/v50/out-probe-skips-r5a/` |
| `/d/r5b` | La compilación del RED y del GREEN con `IWorkspaceFullEditorHost` | 0 errores y 0 `warning CS` | `driver-compile-r5b.txt` (`6b744168…ff4a`) |
| `/d/r5c` | La precondición 6 con la lista de 3 | Core 19 608 = 19 146 + 1 + 461 en las dos ramas (`WorkspaceModelBoundaryTests`, por «Editor») y, en el GREEN, cae CT-PBT2B-06 en UI → D-S9 y `owner-pbt4c-list-4-decision.md` | `driver-redgreen-r5c.txt` (`6cbc2f2f…8314`) y `out-redgreen-v50-r5c/` |
| `/d/r5d` | La sonda de CT-03, 04, 05 y 12 con las piezas de la primera vuelta | Las líneas de la sonda (las mismas que en `/d/r5f`, salvo el nombre del host) | `driver-probes-r5d.txt` (`5db36eec…069e`) |
| `/d/r5e` | **La medida de D-S9:** el RED y el GREEN con `IWorkspaceFullEditHost`, `SetFullEditHost` y `_fullEditHost` | Core **19 608 = 19 147 + 0 + 461** en las dos ramas; en UI solo cae CT-PBT2B-06 en el GREEN | `pbt4/v50/variant-rename/driver-redgreen-r5e.txt` (`1603ee23…f2a4`) y `pbt4/v50/variant-rename/out-redgreen-v50-r5e/` |

Las piezas de `/d/r5e` (`pbt4/v50/variant-rename/`) son las de la v5.0: `pbt4c-green-v50.txt` y `pbt4c-skeleton-v50.txt` son byte a byte las mismas (comprobado con `cmp`).
