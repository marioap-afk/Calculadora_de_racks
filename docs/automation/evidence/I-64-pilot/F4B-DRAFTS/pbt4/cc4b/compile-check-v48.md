# PB-T4b: la precondición 6 del paquete de PB-T4, v4.8 — hecha; el coste, 2 de 3 corridas, y la decisión, del Owner

**Qué es:** la precondición 6 de PB-T4b (§6.2.1, sus ocho pasos), corrida en nombre de la sesión sobre copias exportadas del cierre de PB-T4a (`78a7cef3e9260f0955addd48344fc5a44b9c9957`, F4B-E51), con la v4.8 (`I-64-f4b-pbt4-gate-contract-package-v48.md`, sha256 `6ddc239d4db7509d50b2a2497a7adc91b6808ea4ee3511098a66446c9b25e0a2`).

**Resultado:**
- **pasos 1 a 6, cumplen:**
  - la compilación, sin errores ni avisos CS;
  - la partición, exactamente la prevista;
  - el RED y el GREEN exactos, con la suite completa de Core y los 15 filtros de UI;
  - ninguna prueba falla fuera de las 478; las dos que declaró el paseo pasan;
- **en el paso 7:**
  - CT-06 cumple;
  - **el coste de la lectura cumple todos los criterios en dos de tres corridas** (§3). La primera incumple un solo criterio en un solo caso: una mediana de 98,2 ms donde el límite es 84,8 ms;
  - la segunda y la tercera cumplen todo. La tercera se corrió con la regla que la sesión fijó antes de medirla (`hs65/pbt4/v48/session-cost-rerun-rule.md`, sha256 `85f32a422cd42b42bc8947567e5d8a48f5b25a1bb4bb9364fd42b8459a957262`);
- **el paso 8 está hecho:** `pbt4b-precondition6.json` (sha256 `4bdde2e746cf9db23e3021fe0937725eb1a3dacf37f3806da157f445a8c4afad`), con el coste registrado como «2 de 3; la decisión, del Owner» y las tres TRX por sha256.

**El coste no lo da por bueno la sesión:** el paquete dice que un coste que no cumple vuelve a la sesión, que lo lleva al Owner, con las tres corridas.

**Las corridas, en orden** (las anteriores a la v4.8, en `compile-check-v47.md`, `591233661ba29e5537a89e9da02eba96e14a6229810ef94eb9faa53e764e5f95`, y `compile-check-v46.md`, `7375b18c7ce16a7547380a79807b770920b2b46df686027bcb06f9764992574f`):

| Copia | Versión | Qué hizo | Cómo acabó |
|---|---|---|---|
| `/d/r4g`, `/d/r4h`, `/d/r4i` | v4.5, diagnóstico y v4.6 | pasos 1 a 4 | la compilación, los dos defectos de la copia y la partición (`compile-check-v46.md`) |
| `/d/r4j` a `/d/r4m` | v4.7 | pasos 1 a 6 | dos pruebas fuera de la lista y dos copias (`compile-check-v47.md`) |
| `/d/r4n` a `/d/r4p` | diagnósticos para el análisis (`v47/analysis-outside-477.md`) | el espacio raíz y la sonda del PREPARE | `/d/r4n` paró antes de compilar; `/d/r4o` y `/d/r4p`, medidos |
| **`/d/r4q`** | **v4.8** | pasos 1 a 4 | **la partición prevista** (§1) |
| **`/d/r4r`** | **v4.8** | pasos 5 y 6 | **el RED y el GREEN exactos** (§2) |
| **`/d/r4s`** | **v4.8** | paso 7 | **CT-06 cumple; el coste, no en la primera corrida** (§3) |
| **`/d/r4t`** | **v4.8** | paso 7, la tercera corrida del coste, con la regla de la sesión | **cumple todo** (§3) |

## Cómo

- **Las copias:** `git -C <checkout-principal> archive --format=tar 78a7cef3 | tar -x -C <copia>`, cada una en una carpeta nueva. Ninguna se reutiliza.
- **El compilador:** el SDK de usuario, con `--no-incremental -nodeReuse:false -p:UseSharedCompilation=false`. Las pruebas, en Debug; el Plugin, en Release con `-o`; las sondas, en Release. Se cuentan los `warning CS`. Al acabar cada fase hay `build-server shutdown` (rc 0) y no queda ningún proceso.
- **El checkout principal:** solo `git archive` y lecturas; `git status --porcelain`, vacío. Ningún commit y nada en el worktree.
- **La UI:** solo los 15 filtros de §6.2.1 (`ui_filters_p6_v48.py`): los de la v4.7 con el de las copias, `FullyQualifiedName~RackCad.UI.Tests.FullPanelCopy`. Nunca la suite completa. **La de Core, entera.**
- **El conductor:** `p6_pbt4b_v48.sh` (`d0e1e9a12fc7063a3c49c59321cf2ef9b43559d2f89e50793dfa1d572fe4d41c`), con `p6_check_v48.py`, `check_skips_copy_v48.py` y `ui_filters_p6_v48.py`. **Los guiones del paquete tienen el mismo sha256 en las tres fases**: lo imprime la cabecera de cada `out-run-*-v48-*.txt`, y no cambió ninguno entre `/d/r4q` y `/d/r4s`.
- **Las fases y el paquete:** la fase del modo de medida (`/d/r4q`) empezó antes de escribir el paquete v4.8, porque no lo lee; las otras dos lo leen, con el sha256 de arriba.

## 1. `/d/r4q`: los pasos 1 a 4

| Paso | Resultado |
|---|---|
| 1. La copia | 12 886 archivos |
| 2. `apply_red_pbt4b_v48.py`, modo de medida | **49 archivos** (21 copias, 21 archivos con `Skip`, el kit y el esqueleto), conjunto `cccd98f2…0779` (`apply-red-probe.txt`, `3340f084…8529`) |
| 3. La compilación | Core-tests, UI-tests y Plugin: **0 errores, 0 avisos CS**. Binarios: `011b6117…4e31`, `ff156813…3b63`, `d9549d1c…ad33` y `b75f41aa…dfe6` |
| 3. TRX de las copias (`…Tests.FullPanelCopy`) | Core: 614 filas, **291 Failed y 323 Passed**, 455 s (`6318dcb7…59de`). UI: 36 de 36 Passed (`29525244…82b2da`) |
| 4. `partition_pbt4b_v48.py red` | **481 filas: 477 pruebas, 4 partidas por filas (D-S5)**, sha256 **`e87e024c168dd81d05c1ba1ae9dfa0027da8724ee0d9fd219cce74fe1f613dec`**; **R_C = 291, R_UI = 0** (`partition-red.txt`, `f06f3462…a07e`) |

**La partición es la prevista**, igual, como conjunto, a la sintética de `selftest_pbt4b_v48.py` (`v48/partition-synthetic-v48.tsv`), que se calculó antes de medir:
- la de la v4.7;
- más la guarda de la sexta tanda, **Red** (1);
- más las dos de WPP de D-S1, que pasan a **Green**.

R_C = 292 + 1 − 2 = **291**. **El reparto de C-0 es 356/72.**

## 2. `/d/r4r`: los pasos 5 y 6

| Paso | Resultado |
|---|---|
| 5. El RED con la partición | 49 archivos, conjunto `abcfbfe9…c041` (`apply-red.txt`, `0c7f6fc0…982e`: el sha256 de las 21 copias). `check_skips_copy_v48.py`: 21 archivos, 0 mal, **478 `Skip`** |
| 5. La compilación | **0 errores, 0 avisos CS** (`73240db6…28d2`, `a236c715…0613`, `50017f84…19d2`, `50b1b8ec…7ee9`) |
| 5. **Core, la suite completa** | **19 587 = 18 838 Passed + 291 Failed + 458 omitidas**, 709 s (`core-red.trx`, `d57ca6ff…0bde`). **Lo previsto: (19 129 − 291) + 291 + 458.** Las 291 son exactamente las Red de la partición, en su clase y con sus resultados. Las omitidas fuera de la lista son las 7 de la base. **Ninguna falla fuera de las copias** |
| 5. UI, los 15 filtros | 913 filas, 0 Failed, R_UI = 0 |
| 5. `p6_check_v48.py red` | **CUMPLE** (`check-red.txt`, `e4a99a0b…b742`) |
| 6. El GREEN literal (`apply_green_pbt4b_v45.py`) | 10 archivos, conjunto `f93c7262…4c45`. `check_skips_copy_v48.py`: 21, 0 mal, 478 |
| 6. La compilación | **0 errores, 0 avisos CS** (`55434550…2d2f`, `d785d265…73ca`, `42c8b11e…f7c4`, `631e9d12…19eb`) |
| 6. **Core, la suite completa** | **19 587 = 19 129 Passed + 0 Failed + 458 omitidas**, 752 s (`core-green.trx`, `3f4baec6…86b2b`). **Ninguna falla.** Pasan, entre otras: `NamespaceFolderGuardTests` (D-S6), las dos de WPP con D-S1, el equivalente de la guarda de la sexta tanda, y **las dos que declaró el paseo, `WorkspaceHostHardeningGuardTests.TheGuardOfTheLiteralsTrips…` y `WorkspaceCommandAdvisorGuardTests.TheCatalogBody…`** |
| 6. UI, los 15 filtros | 913 filas, 0 Failed |
| 6. `p6_check_v48.py green` y `partition_pbt4b_v48.py green` | **CUMPLE** (`check-green.txt`, `f7b4b0fc…7e3e`); **las 477 pruebas de copia pasan** («no pasan: 0»; `partition-green.txt`, `d7d58d55…6c8a`) |

## 3. `/d/r4s`: el paso 7 (las sondas, sobre el GREEN, en Release)

**La compilación** (Debug y Release, con las tres sondas): 0 errores, 0 avisos CS. Las sondas:
- `TmpReadCostProbe.cs`, `91acfca3…ba0e`;
- `TmpCt06PresentarProbe.cs`, `32d9fbc4…8f65`;
- `TmpIsDirtyCostProbe.cs`, `fb3c8ae1…1195`.

**Las líneas «presentar» de CT-06: CUMPLE** (TRX `71116b91…57aa9`; `ct06-probe.txt`, `50e86859…6abc`; `check-ct06.txt`, `8b5f6dba…200a`). Son las tres de §5.2, literales:
- `presentar estado=Presented testigo-clave=A1 testigo-texto-igual-al-origen=True borrador-igual=True origen-igual=True lecturas=1 lineas=2`;
- `presentar Cambiado fuera: el nombre.`;
- `presentar Cambiado por ti: las anotaciones.`

**El coste de la lectura: NO CUMPLE** (`check-cost.txt`, `a01cd61a…8403`), con dos corridas:
- primera: TRX `6487a1c3…59f1`, 695 s; `cost-run1.txt`, `c1afaeba…e7f8`;
- segunda: TRX `ca1bea9c…8420`, 567 s; `cost-run2.txt`, `dbbf39d5…030c`.

- **Las materializaciones:** `Begin=1 origen-encontrado=1 cae-al-Primary=1 origen-es-Primary=1` en los seis escenarios, **en las dos corridas**. **La materialización única se cumple.**
- **La relectura con el origen encontrado**, en los cinco escenarios admitidos, **en las dos corridas**: no pasa de `Begin` más el piloto, `BuildStaleBase` y el margen, y es menor que el 75 % de la de PB-T4a. Baja de +68…+128 ms sobre la base (t4a) a +36…+67 (t4b).
- **«Sin regresión»** (`t4b Begin`, `cae-al-Primary` y `origen-es-Primary` no pasan de su `t4a` más el mayor de 10 ms y el 30 %): **se cumple en todo salvo en un caso de la primera corrida.** En `pb-20-ab x4 vistas +200 racks ajenos`:

  | | t4a | t4b | Límite | ¿Cumple? |
  |---|---|---|---|---|
  | `origen-es-Primary`, 1.ª corrida | 65,2 ms (p95 88,1) | **98,2 ms** (p95 130,6; n = 200) | 84,8 ms | **no** |
  | `origen-es-Primary`, 2.ª corrida | 46,5 ms | 47,2 ms | — | sí |
  | `Begin`, 1.ª corrida | 61,6 ms | 65,8 ms | — | sí |
  | `cae-al-Primary`, 1.ª corrida | 67,0 ms | 67,5 ms | — | sí |

  En esa primera corrida todos los tiempos de ese escenario son más altos que en la segunda (`Begin` 61,6/65,8 frente a 45,7/47,1).

**La tercera corrida** (`/d/r4t`; `p6_cost_rerun_v48.sh`, `0704d90c…f988`; la regla de la sesión, fijada antes de medir: `v48/session-cost-rerun-rule.md`):
- **cómo se corrió:** una copia exportada nueva del mismo GREEN literal de la v4.8, con el RED y la partición de `/d/r4q` (49 archivos, `abcfbfe9…c041`) y el GREEN (10, `f93c7262…4c45`), es decir, las mismas copias y el mismo `src/` que `/d/r4s`;
- **la sonda, sin cambiar nada:** `TmpReadCostProbe.cs` `91acfca3…ba0e`, la misma de `/d/r4s`, compilada en Debug y Release (0 errores, 0 avisos CS) y corrida solo con su filtro, en Release;
- **la TRX:** `cost-run3.trx` `67a2bc8e…50f1`, 731 s; `cost-run3.txt` `f8fdbf13…7fb7`;
- **evaluada con `p6_check_v48.py cost`**, como las dos primeras: **CUMPLE** (`check-cost-run3.txt`, `8754253f…b77b`):
  - las materializaciones, 1/1/1/1 en los seis escenarios;
  - la relectura con el origen encontrado, por debajo de sus dos límites en los cinco escenarios admitidos;
  - sin regresión en todos. El más justo es `pb-01-base x2`: `t4b Begin` 42,5 ms con un límite de 42,6 ms (t4a 32,6).

**El caso de la primera corrida, en las tres** (`cost-table-3-runs.txt`, `2db3d8e5…ae97`; la mediana, con el p95 entre paréntesis, en ms; n = 200; el límite es t4a + el mayor de 10 ms y el 30 %):

| Corrida | Caso | t4a | t4b | Límite | ¿Cumple? |
|---|---|---|---|---|---|
| 1 (`/d/r4s`) | `Begin` | 61,6 (81,0) | 65,8 (82,5) | 80,0 | sí |
| 1 | `cae-al-Primary` | 67,0 (92,3) | 67,5 (90,6) | 87,1 | sí |
| 1 | `origen-es-Primary` | 65,2 (88,1) | **98,2 (130,6)** | 84,8 | **no** |
| 2 (`/d/r4s`) | `Begin` | 45,7 (52,6) | 47,1 (71,1) | 59,4 | sí |
| 2 | `cae-al-Primary` | 46,6 (63,3) | 48,8 (66,9) | 60,6 | sí |
| 2 | `origen-es-Primary` | 46,5 (62,8) | 47,2 (71,2) | 60,4 | sí |
| 3 (`/d/r4t`) | `Begin` | 61,2 (82,8) | 58,2 (71,6) | 79,5 | sí |
| 3 | `cae-al-Primary` | 61,3 (80,6) | 60,6 (83,2) | 79,6 | sí |
| 3 | `origen-es-Primary` | 65,4 (**157,2**) | 60,1 (83,3) | 85,0 | sí |

**El resultado global de cada corrida:**
- 1.ª: **NO CUMPLE** (`check-cost-run1-only.txt`, `1c2abefb…882e`), por ese único caso;
- 2.ª: **CUMPLE** (`check-cost-run2-only.txt`, `69bbe1d9…b8b9`);
- 3.ª: **CUMPLE**.

**La carga de la máquina en la tercera corrida** (`tasklist` filtrado, en `out-run-costrerun-v48-r4t.txt`): la misma al empezar, justo antes de la sonda y al terminar, cinco procesos de otros: `codex.exe` ×2 (unos 112 y 71 MB), `codex-computer-use-swift` ×1, `codex-windows-sandbox-ser` ×1 y `node.exe` ×1. **Ningún `dotnet`, `testhost` ni compilación de otro agente.** Durante la sonda, además, los dos `dotnet` y el `testhost` de la propia corrida, que el `build-server shutdown` deja en 0. **En las dos primeras corridas no se anotó la carga.**

**Lo que se ve:** la primera corrida es más lenta en todo su escenario, y la tercera tiene picos también en t4a (p95 de 157,2 ms en `origen-es-Primary`). Las medianas de t4b en `origen-es-Primary` son 98,2, 47,2 y 60,1 ms: el caso no se repite. Que eso baste lo decide el Owner, con la recomendación de la regla de la sesión.

**El coste de `IsDirty`** (sin criterio; se registra): `isdirty.trx` `8da7524a…3e66`, `isdirty-probe.txt` `0c44c74b…84d1`. Medianas, limpio y sucio:
- `pb-01-base`: 2,9 y 2,9 ms;
- `pb-07-combinado`: 3,9 y 3,9 ms;
- `pb-20-ab`: 9,3 y 9,5 ms.

**La duración de la suite de Core:** 709 s en el RED y 752 s en el GREEN.

## 4. El paso 8: `pbt4b-precondition6.json`

Escrito por `p6_json_v48.py` (`25f64d14…b13e`). Es `p6_check_v48.py json` (el `CopySha` sale de `out-redgreen-v48-r4r/apply-red.txt`) con un campo más, `Cost`. Su sha256 es **`4bdde2e746cf9db23e3021fe0937725eb1a3dacf37f3806da157f445a8c4afad`**:
- `Schema` `rackcad-pbt4b-precondition6/v1` y `Revision` `78a7cef3e9260f0955addd48344fc5a44b9c9957`;
- `PartitionSha256` `e87e024c168dd81d05c1ba1ae9dfa0027da8724ee0d9fd219cce74fe1f613dec`;
- `CopySha`: las 21 copias;
- **R_C = 291, R_UI = 0**;
- `MinSelected`: las 42 clases, por su nombre `FullPanelCopy<Clase>RedTests` o `…GreenTests`. 31 tienen pruebas; las 11 con 0 salen de la tabla al emitir;
- **`Cost`:**
  - `Result`: «2 de 3; la decisión, del Owner»;
  - `RuleSha256`: el de la regla de la sesión;
  - `Runs`: las tres corridas, cada una con su TRX por sha256 y su resultado:
    - `cost-run1` `6487a1c3…59f1`, NO CUMPLE;
    - `cost-run2` `ca1bea9c…8420`, CUMPLE;
    - `cost-run3` `67a2bc8e…50f1`, CUMPLE.

**Lo que queda fuera:** la suite completa de UI, por la regla vigente. La vería la CI (C16).

## 5. Las copias en disco

`/d/r4g` a `/d/r4t` siguen en disco con sus `bin/obj`.
- **`/d/r4g` a `/d/r4p` se pueden borrar a mano ya.**
- **`/d/r4q` a `/d/r4t` también**, cuando la sesión no necesite sus TRX, que aquí van por sha256.

`/d/r4a` a `/d/r4f` no son de esta precondición.
