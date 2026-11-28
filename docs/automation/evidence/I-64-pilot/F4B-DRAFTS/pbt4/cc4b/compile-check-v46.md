# PB-T4b: la precondición 6 del paquete de PB-T4 — PARADA en el paso 4 (la partición), con la v4.6

**Qué es:** la precondición 6 de PB-T4b (§6.2.1, sus ocho pasos), corrida en nombre de la sesión sobre copias exportadas del cierre de PB-T4a (`78a7cef3e9260f0955addd48344fc5a44b9c9957`, F4B-E51).

**Resultado: NO CUMPLE TODAVÍA.** Tres corridas:
1. **`/d/r4g`, con la v4.5:** paró en la compilación (§1);
2. **`/d/r4h`, un diagnóstico**, no la precondición (§2);
3. **`/d/r4i`, con los guiones de la v4.6** (`I-64-f4b-pbt4-gate-contract-package-v46.md`, `e58df774f417ebdb2a0d5cf0e10adab4ead84b2045fea8504eb886fdef54f08f`): compila sin errores ni avisos y paró en el paso 4, porque **cuatro `Theory` mezclan resultados en el RED** y la partición no parte una `Theory` (§3). Es una decisión de diseño para la sesión: no se corrige aquí.

No hay partición, R_C, R_UI, `CopySha`, RED exacto, GREEN ni sondas: **no se escribe `pbt4b-precondition6.json`**.

## Cómo, en las tres

- **Las copias:** `git -C <checkout-principal> archive --format=tar 78a7cef3 | tar -x -C <copia>`, cada una en una carpeta nueva de ruta corta fuera de todo worktree (`/d/r4g`, `/d/r4h` y `/d/r4i`); ninguna se reutiliza. Las tres siguen en disco, con sus `bin/obj`: **no hacen falta para lo que sigue y se pueden borrar a mano.**
- **El compilador:** el SDK de usuario (`<datos-locales-del-usuario>\Microsoft\dotnet\dotnet.exe`), con `--no-incremental -nodeReuse:false -p:UseSharedCompilation=false -v:minimal`; Core-tests y UI-tests en Debug y el Plugin en Release con `-o`; se cuentan los errores y los `warning CS`. Al acabar cada fase, `build-server shutdown` (rc 0), y no queda ningún `dotnet`, `VBCSCompiler` ni `testhost` en marcha.
- **El checkout principal:** solo `git archive` y lecturas (`rev-parse`, `show`, `ls-tree`); `git status --porcelain` vacío al acabar cada fase. Ningún commit. Nada en el worktree.
- **La UI:** solo el filtro de las copias de la interfaz (`FullyQualifiedName~RackCad.UI.Tests.FullPanelCopies`); nunca la suite completa.
- **Los guiones** (en esta carpeta, sha256): `p6_pbt4b.sh` `c2065e60…f7c8` (la v4.5), `p6_pbt4b_v46.sh` (la v4.6; `P6_VER`), `p6_check.py` `5598f97e…37bd3`, `check_skips_copy_v45.py` `28bf20f4…97b2` y `check_skips_copy_v46.py`, `ui_filters_p6.py` `db73d01a…53d5`, `TmpIsDirtyCostProbe.cs` `fb3c8ae1…1195` (sin usar todavía), los del diagnóstico (`p6_diag_using.sh` `217e0345…c1dca5e` y `diag_add_using.py` `011b4b5f…cf164`) y `mixed_rows_p6.py` (§3). Usan los guiones del paquete de `hs65\pbt4\` sin tocarlos. Python siempre con `-I`, desde guiones escritos con Write.

## 1. `/d/r4g`, con la v4.5: parada en la compilación

| Paso | Resultado |
|---|---|
| 1. La copia | 12 886 archivos de `78a7cef3` |
| 2. `apply_red_pbt4b_v45.py`, modo de medida | 47 archivos, conjunto `25412d84…bd2f` |
| 3. La compilación | **Core-tests: rc 1, 41 errores distintos (82 líneas), 0 avisos CS** (`out-probe/probe-core-tests.log`, `cfa0e9b0…2ac1a`); UI-tests y Plugin Release: 0 errores, 0 avisos CS |
| 3. TRX | UI `FullPanelCopies`: 36 de 36 Passed (`79200b3f…7289`); Core: no compiló |

**La causa:** faltaba la directiva `using RackCad.Application.Systems.PushBack.Draft;` en las copias de C-0 (M-04 y M-06 cambiaban el nombre del tipo, no su espacio de nombres): CS0246 ×35, CS0103 ×5 y CS0535 ×1, en ocho copias.

## 2. `/d/r4h`: diagnóstico, no la precondición

El mismo RED en modo de medida, con la directiva añadida a mano (`diag_add_using.py`). Core-tests siguió en **rc 1, 14 errores distintos (28 líneas), 0 avisos CS** (`out-diag-using/core-tests.log`, `024658d1…a511d`): ocho copias «mecánicas» que abren un borrador con una lectura del piloto (WPEH: el ayudante `Env.OpenUndecidable` y dos pruebas; WPP: dos pruebas). De ahí salió la v4.6: MU (la directiva, V46-01) y D-S1 para las ocho (V46-02, 360/68).

## 3. `/d/r4i`, con la v4.6: compila; parada en la partición

| Paso | Resultado |
|---|---|
| 1. La copia | 12 886 archivos de `78a7cef3` |
| 2. `apply_red_pbt4b_v46.py`, modo de medida | 47 archivos, conjunto `25394db7…2e8d` |
| 3. La compilación | **Core-tests, UI-tests y Plugin Release: rc 0, 0 errores y 0 avisos CS** (`out-probe-v46/`). Binarios: `RackCad.Application.dll` `dc17d5ad…f773`, `RackCad.Tests.dll` `a5511639…759b`, `RackCad.UI.Tests.dll` `3906c80c…3267`, `RackCad.Plugin.dll` `a41cad97…f315`. **El kit v4.5 y las dos copias nuevas de PB-T4a compilan** |
| 3. TRX de las copias | **Core** `FullyQualifiedName~RackCad.Tests.FullPanelCopies`: 613 filas, 292 Failed y 321 Passed, 418 s (`4f3d6586…e0d2`). **UI**: 36 filas, 36 Passed (`5065bf29…2b8e`) |
| 4. `partition_pbt4b_v45.py red` | **PARA:** cuatro pruebas mezclan resultados |
| 5 a 8 | No se corren |

**Lo que sí se mide** (`mixed_rows_p6.py`, `out-probe-v46/mixed-rows.txt`):
- **476 pruebas de copia** (449 de Core y 27 de la UI):
  - **249 pasan enteras** en el RED (345 filas: 309 de Core y las 36 de la UI);
  - **223 fallan enteras** (274 filas), **todas por aserción**: la primera línea del StackTrace está bajo `tests/`, en las 274;
  - **4 mezclan** (30 filas: 18 Failed y 12 Passed).
- **Las cuatro que mezclan:**

| Copia y prueba | Filas | Fallan en el RED | Pasan en el RED | Por qué fallan |
|---|---|---|---|---|
| `WorkspaceLifecycleHostFull.EachOperationThatCanChangeADraftEndsTheMarkSoTheSessionIsRetiredConservatively` (mecánica; `InlineData`) | 10 | `a-check-that-finds-a-conflict`, `a-check-that-recovers` y `a-reload` | las otras 7 | `Env.Pilot` (la copia, :96): `Assert.NotNull`, porque el anfitrión del RED abre un borrador del piloto |
| `WorkspacePilotEditGuardFull.TheHostGuardTripsOnAPublicOperationAnOrderOfBasesOrAnAdapterThatIsAFieldOfInstance` (guarda de C-5; `InlineData`) | 4 | la del adaptador estático (`private static readonly PushBackFullFieldAdapter PilotFieldAdapter = new();`) | las otras 3 | `Assert.Contains`: WHC del RED todavía tiene el adaptador del piloto |
| `WorkspacePilotPrepareFull.ConfirmAppliesOnlyToCleanDirtyAndConflictAndTheOthersReadNothing` (mecánica; `MemberData(StatusNames)`) | 8 | las 7 que editan (`Applying`, `Conflict`, `Dirty`, `DirtyStale`, `Error`, `UnknownCommit` y `UnknownPost`) | `Clean` | `Rig.Edit` (la copia, :114): `NullReferenceException` en el código de la prueba |
| `WorkspacePilotPrepareFull.KeepAppliesToConflictAndToDirtyMarkedStaleApplyingWaitsAndTheOthersAreNotNeededAllWithoutReading` (a mano, D-S1; `MemberData(StatusNames)`) | 8 | las mismas 7 | `Clean` | lo mismo |

**Lo que pide la decisión:** el paquete dice que una prueba que mezcla resultados «para la emisión (no se parte una `Theory`; C04)». Ninguna clase puede llevarla tal cual:
- `…FullRedTests` exige que todas sus filas fallen en el RED;
- `…FullGreenTests` exige que todas pasen;
- una tercera clase fuera de los `RequiredTests` fallaría en la CI del RED fuera de los filtros con `ExpectRed`.

Las opciones, para la sesión:
- **(a) partirlas por filas,** como equivalentes escritos a mano (D-S1): el mismo método, con las filas que fallan en `…FullRedTests` y las que pasan en `…FullGreenTests`; para las dos de `MemberData`, dos miembros de datos nuevos en la copia. Los totales no cambian (las 30 filas siguen siendo 30), CT-PBT4B-20 tiene que admitirlo, y el reparto de C-0 pasaría a 358/70 (dos de las cuatro son mecánicas; `KeepApplies…` ya es a mano y la de PEG es una guarda);
- **(b) otra regla de la partición** que el paquete y el generador lleven escrita.

## Lo que no cubre todavía

- Los pasos 4 a 8 con la v4.6: la partición y su sha256, R_C, R_UI, los `MinSelected` de las 40 clases, el `CopySha`, el RED exacto, el GREEN literal, `check_skips` sobre la copia, las dos sondas (CT-06 y el coste), el coste de `IsDirty` y la duración de la suite de Core. **Medido por ahora:** las copias de Core tardan 418 s en el modo de medida.
- La suite completa de UI, por la regla vigente.
- **Una nota de forma:** los comentarios de `make_copies_pbt4b_v46.py` y `selftest_pbt4b_v46.py` se cambiaron después de la corrida de `/d/r4i`, solo en el nombre de la regla (M-09 → MU). La salida es la misma: el conjunto de las copias sigue siendo `f72b1f2f…fb58` y la revisión es idéntica byte a byte. El sha256 que cita la v4.6 es el de después del cambio. El de `selftest_pbt4b_v46.py` se hizo con `sed`, no con Write.
