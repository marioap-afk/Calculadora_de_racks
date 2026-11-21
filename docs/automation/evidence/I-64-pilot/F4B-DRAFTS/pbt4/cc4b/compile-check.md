# PB-T4b: la precondición 6 del paquete de PB-T4, v4.9 — CUMPLE (pasos 1 a 6; el 7, el de la v4.8, dado por cumplido por el Owner)

**Qué es:** la precondición 6 de PB-T4b (§6.2.1), corrida en nombre de la sesión sobre copias exportadas del cierre de PB-T4a (`78a7cef3e9260f0955addd48344fc5a44b9c9957`, F4B-E51), con la v4.9 (`I-64-f4b-pbt4-gate-contract-package-v49.md`, sha256 `043f196d8452588530784eb04fd3cd3b63c3594d3e961df5e204f0109329c886`). La v4.9 corrige la CA-T4b (`independent-review-v1.md`, §20).

**El cambio que la hace repetir:** CA-T4b I-01. El equivalente de WHHG `NoneOfThe30…` trae la autoprueba de la guarda, así que cambia el `CopySha` de `WorkspaceHostHardeningGuardFull.cs`.

**Resultado: CUMPLE.**
- **Pasos 1 a 6, otra vez,** en copias nuevas (`/d/r4u` y `/d/r4v`):
  - la compilación, sin errores ni avisos CS;
  - la partición, la misma de la v4.8 byte a byte;
  - el RED y el GREEN exactos, con la suite completa de Core y los 15 filtros de UI;
  - ninguna prueba falla fuera de las 478;
  - **el equivalente nuevo cae como Red en el RED (en su `Assert.Empty`) y pasa en el GREEN, con sus 13 `Assert.NotEmpty`.**
- **El paso 7 se queda** con las corridas de la v4.8: CT-06 en `/d/r4s` y el coste en tres corridas (`/d/r4s` y `/d/r4t`), que el Owner dio por cumplido (`owner-pbt4b-cost-decision.md`, la séptima tanda). **Comprobado por sha256 que no cambia ninguna de sus entradas** (§3).
- **El paso 8:** `pbt4b-precondition6.json`, nuevo, con el campo `Cost` que remite a esas corridas y a la decisión (§4).

**El registro de antes**, como historia:
- la v4.8: `compile-check-v48.md` (`b1a4eeba785ebfa330e6ddeb26367d3bae600d57b47deacf278d8fa8dacf0d20`) y `pbt4b-precondition6-v48.json` (`4bdde2e746cf9db23e3021fe0937725eb1a3dacf37f3806da157f445a8c4afad`);
- la v4.7: `compile-check-v47.md`;
- la v4.6: `compile-check-v46.md`.

| Copia | Versión | Qué hizo | Cómo acabó |
|---|---|---|---|
| `/d/r4g` a `/d/r4p` | v4.5 a v4.7 y diagnósticos | — | `compile-check-v46.md` y `-v47.md` |
| `/d/r4q`, `/d/r4r`, `/d/r4s` y `/d/r4t` | v4.8 | pasos 1 a 7 y la tercera corrida del coste | `compile-check-v48.md` |
| **`/d/r4u`** | **v4.9** | pasos 1 a 4 | **la partición, la misma** (§1) |
| **`/d/r4v`** | **v4.9** | pasos 5 y 6 | **el RED y el GREEN exactos** (§2) |

## Cómo

- **Las copias, el compilador y el checkout,** como en la v4.8:
  - `git archive` de `78a7cef3` a una carpeta nueva;
  - el SDK de usuario con `--no-incremental -nodeReuse:false -p:UseSharedCompilation=false`;
  - las pruebas en Debug y el Plugin en Release con `-o`;
  - el `build-server shutdown` deja 0 procesos;
  - en el checkout principal, solo `git archive` y lecturas, con `git status` vacío;
  - ningún commit y nada en el worktree.
- **La UI:** solo los 15 filtros de §6.2.1 (`ui_filters_p6_v48.py`); los de la v4.9 son los mismos que los de la v4.8. **La de Core, entera.**
- **El conductor:** `p6_pbt4b_v49.sh` (`654abcaf…251a`), el de la v4.8 con `apply_red_pbt4b_v49.py` y `make_copies_pbt4b_v49.py`. Lo demás son los guiones de la v4.8: `pbt4b_skips_v48.py`, `partition_pbt4b_v48.py`, `p6_check_v48.py` y `check_skips_copy_v48.py`.
- **Los guiones:** su sha256 es el mismo en `/d/r4u` y en `/d/r4v`, y lo imprime la cabecera de cada `out-run-*-v49-*.txt`.
- **El paquete:** la fase del modo de medida empezó antes de escribir el paquete v4.9, porque no lo lee; `/d/r4v` lo lee, con el sha256 de arriba.

## 1. `/d/r4u`: los pasos 1 a 4

| Paso | Resultado |
|---|---|
| 2. `apply_red_pbt4b_v49.py`, modo de medida | 49 archivos, conjunto `07b129b4…84e8` (`apply-red-probe.txt`, `10727ad8…bfcd`): los de la v4.8 salvo `WorkspaceHostHardeningGuardFull.cs` (`selftest_pbt4b_v49.py`) |
| 3. La compilación | Core-tests, UI-tests y Plugin: **0 errores, 0 avisos CS** (`48d913a4…7005`, `5649ef67…2e04`, `a06cee72…7686`, `5d8f3de6…19e6`) |
| 3. TRX de las copias | Core: 614 filas, **291 Failed y 323 Passed**, 617 s (`ef54d1b6…9cce`). UI: 36 de 36 (`1b015718…c6cac4`) |
| 4. La partición | **481 filas, 477 pruebas y 4 partidas por filas: `e87e024c168dd81d05c1ba1ae9dfa0027da8724ee0d9fd219cce74fe1f613dec`, la misma de la v4.8**; R_C = 291, R_UI = 0. La guarda de WHHG, **Red** con 1 resultado |

## 2. `/d/r4v`: los pasos 5 y 6

| Paso | Resultado |
|---|---|
| 5. El RED con la partición | 49 archivos, conjunto `b03d14a3…974b` (`apply-red.txt`, `9303de39…04c7`: el sha256 de las 21 copias). `check_skips_copy_v48.py`: 21 archivos, 0 mal, 478 `Skip` |
| 5. La compilación | **0 errores, 0 avisos CS** (`5ddd1915…5207`, `fafad98f…0d39`, `1bd9921a…ac77`, `8e6ac54f…603f`) |
| 5. **Core, la suite completa** | **19 587 = 18 838 Passed + 291 Failed + 458 omitidas**, 778 s (`core-red.trx`, `366c4f43…6f51`). Las 291 son exactamente las Red de la partición. **El equivalente de WHHG cae en su `Assert.Empty`** (`WorkspaceHostHardeningGuardFull.cs`, :105), con las tres cuentas de la base: «La relectura…», «El borrador se está aplicando…» y «El rack no tiene escala…». **Ninguna falla fuera de las copias** |
| 5. UI, los 15 filtros | 913 filas, 0 Failed |
| 5. `p6_check_v48.py red` | **CUMPLE** (`check-red.txt`, `f29b6985…6388`) |
| 6. El GREEN literal | 10 archivos, conjunto `f93c7262…4c45`, **el mismo de la v4.8**. `check_skips_copy_v48.py`: 21, 0 mal, 478 |
| 6. La compilación | **0 errores, 0 avisos CS** (`ca7ae815…5e57`, `510f7e3a…0ab1`, `5b141abb…87a0`, `afe1ff3a…2758`) |
| 6. **Core, la suite completa** | **19 587 = 19 129 Passed + 0 Failed + 458 omitidas**, 749 s (`core-green.trx`, `9da003b7…7e0e`). **El equivalente de WHHG pasa:** su `Assert.Empty` con la tabla nueva y sus 13 `Assert.NotEmpty` de la autoprueba. Pasan también las dos que declaró el paseo |
| 6. UI, los 15 filtros | 913 filas, 0 Failed |
| 6. `p6_check_v48.py green` y `partition_pbt4b_v48.py green` | **CUMPLE** (`check-green.txt`, `2443ecd6…0999`); las 477 pruebas de copia pasan (`partition-green.txt`, `d7d58d55…6c8a`) |

## 3. El paso 7: el de la v4.8, con sus entradas sin cambio

`step7_inputs_v49.py` (`82987dd1…a54e`) da **EL PASO 7 DE LA v4.8 SIGUE VALIENDO: SI** (`out-redgreen-v49-r4v-extra/step7-inputs.txt`, `9097ce55…5c87`):

1. **El GREEN de `src/`:** los 10 archivos que escribe `apply_green_pbt4b_v45.py`, iguales por sha256 en `/d/r4s` (v4.8) y en `/d/r4v` (v4.9).
2. **El RED que va debajo:** de los 49 archivos (el esqueleto, los `Skip`, el kit y las 21 copias) cambia solo `WorkspaceHostHardeningGuardFull.cs`. **El kit es el mismo** (`b4ed29f9…`). `/d/r4t` tenía el mismo RED que `/d/r4s`.
3. **Las tres sondas** tienen el sha256 que imprimieron `/d/r4s` y, la del coste, `/d/r4t`:
   - `pbt4b-read-cost-probe.cs`, `91acfca3…ba0e`;
   - `pbt4b-ct06-presentar-probe.cs`, `32d9fbc4…8f65`;
   - `TmpIsDirtyCostProbe.cs`, `fb3c8ae1…1195`.
4. **Lo que usan las sondas:** ninguna nombra un tipo declarado en las 21 copias. Usan el kit (`FullEditIntentFakeReader`, `FullEditIntentKit`, `FullPrepareKit` y `FullReadRig`) y los kits de las pruebas originales.

**Así que CT-06 (`/d/r4s`: CUMPLE) y el coste (`/d/r4s` y `/d/r4t`) valen para la v4.9.** El coste lo dio por cumplido el Owner, con dos de tres corridas: `owner-pbt4b-cost-decision.md`, `d0f4ca259f26fbf91676538d0895eca7acf921b76d5c56eb47885965213f5576`. Las corridas y sus TRX están en `compile-check-v48.md`, §3.

## 4. El paso 8: `pbt4b-precondition6.json`

Escrito por `p6_json_v49.py` (`1cac2f77…01a6`). Su sha256 es **`8df3e566d87d55b063989de6efd6b857e72aaf46770529d1d13e25904dd4631b`**:
- `Schema` `rackcad-pbt4b-precondition6/v1` y `Revision` `78a7cef3e9260f0955addd48344fc5a44b9c9957`;
- `PartitionSha256` `e87e024c…3dec`;
- `CopySha`: las 21 copias. **Frente a la v4.8 cambia solo la de `WorkspaceHostHardeningGuardFull.cs`;**
- **R_C = 291, R_UI = 0**, y el `MinSelected` de las 42 clases (31 con pruebas). Son los de la v4.8: el guion comprueba que son iguales;
- **`Cost`:**
  - `Result`: «Dado por cumplido por el Owner (owner-pbt4b-cost-decision.md, la septima tanda): 2 de 3 corridas de la v4.8»;
  - `DecisionSha256` (la decisión) y `RuleSha256` (`v48/session-cost-rerun-rule.md`);
  - `FromPackage` «v4.8» y `FromPrecondition6JsonSha256`, el del JSON de la v4.8;
  - `Runs`: las tres corridas con su TRX por sha256 y su resultado:
    - `6487a1c3…59f1`, NO CUMPLE;
    - `ca1bea9c…8420`, CUMPLE;
    - `67a2bc8e…50f1`, CUMPLE;
  - `Step7InputsUnchanged` true, con `Step7InputsCheckSha256`.

**Lo que queda fuera:** la suite completa de UI, por la regla vigente; la vería la CI (C16).

## 5. Las copias en disco

`/d/r4g` a `/d/r4v` siguen en disco con sus `bin/obj`.
- **`/d/r4g` a `/d/r4r` y `/d/r4u` se pueden borrar a mano ya.**
- **`/d/r4s`, `/d/r4t` y `/d/r4v` también**, cuando la sesión no necesite sus TRX: los del paso 7 que se conserva y los de los pasos 5 y 6 de la v4.9, que aquí van por sha256.
