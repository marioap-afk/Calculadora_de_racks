# PB-T4b: la precondición 6 del paquete de PB-T4 — PARADA en los pasos 5 y 6, con la v4.7

**Qué es:** la precondición 6 de PB-T4b (§6.2.1, sus ocho pasos), corrida en nombre de la sesión sobre copias exportadas del cierre de PB-T4a (`78a7cef3e9260f0955addd48344fc5a44b9c9957`, F4B-E51).

**Resultado: NO CUMPLE.** Con la v4.7 (`I-64-f4b-pbt4-gate-contract-package-v47.md`, sha256 `b86ef9cbd29eea5fa1629af37db105dff8ef2feaa3a6999b2504ec49751d40c9`):
- **compila sin errores y sin ningún `warning CS`**, y la partición de D-S5 sale como estaba prevista;
- **el RED es exacto salvo una prueba;**
- **pero caen dos pruebas que NO están en las 477** (§5): `NamespaceFolderGuardTests.TestProjects_KeepExactlyOneAssemblyRootNamespace`, en el RED y en el GREEN, y `WorkspaceHostHardeningGuardTests.NoneOfThe30LiteralsInEnglishStaysInTheHostOrTheSupportAndTheFourIdentifiersAreKept`, en el GREEN;
- **y dos copias mecánicas de la lista no pasan en el GREEN** (D-S1).

Es una de las causas de parada del encargo, así que **no se escribe `pbt4b-precondition6.json`** y **no se corre el paso 7** (las sondas).

**Las corridas, en orden:**

| Copia | Paquete y guiones | Qué hizo | Cómo acabó |
|---|---|---|---|
| `/d/r4g` | v4.5 | pasos 1 a 3 | paró en la compilación (`compile-check-v46.md`, §1) |
| `/d/r4h` | diagnóstico | la compilación con la directiva a mano | 14 errores: de ahí salió la v4.6 (`compile-check-v46.md`, §2) |
| `/d/r4i` | v4.6 | pasos 1 a 4 | paró en la partición: cuatro `Theory` mezclan (`compile-check-v46.md`, §3) |
| `/d/r4j` | v4.7, primera redacción de los miembros nuevos | pasos 1 a 4 | la partición de D-S5 sale (§1) |
| `/d/r4k` | la misma | pasos 5 y 6 | el RED y el GREEN no compilan: un defecto de la v4.7, corregido antes de emitirla (§2) |
| `/d/r4l` | **v4.7** | pasos 1 a 4 | la partición, igual byte a byte a la de `/d/r4j` (§3) |
| `/d/r4m` | **v4.7** | pasos 5 y 6 | **NO CUMPLE:** dos pruebas fuera de la lista y dos copias (§4 y §5) |

`compile-check-v46.md` es el registro de la v4.6, sin cambios (`7375b18c7ce16a7547380a79807b770920b2b46df686027bcb06f9764992574f`).

## Cómo, en todas

- **Las copias:** `git -C <checkout-principal> archive --format=tar 78a7cef3 | tar -x -C <copia>`, cada una en una carpeta nueva de ruta corta fuera de todo worktree; ninguna se reutiliza. **Siguen en disco y se pueden borrar a mano** (§7).
- **El compilador:** el SDK de usuario (`<datos-locales-del-usuario>\Microsoft\dotnet\dotnet.exe`) con `--no-incremental -nodeReuse:false -p:UseSharedCompilation=false -v:minimal`. Core-tests y UI-tests van en Debug y el Plugin en Release con `-o`. Se cuentan los errores y los `warning CS`. Al acabar cada fase, `build-server shutdown` (rc 0) deja 0 procesos `dotnet`, `VBCSCompiler` o `testhost`.
- **El checkout principal:** solo `git archive` y lecturas (`rev-parse`, `show`). `git status --porcelain` está vacío al acabar cada fase. Ningún commit y nada en el worktree.
- **La UI:** solo los 15 filtros de §6.2.1 (`ui_filters_p6.py`; los de la v4.7 son los mismos que los de la v4.6), nunca la suite completa. **La de Core, entera.**
- **El conductor:** `p6_pbt4b_v47.sh`, sha256 `4d94468a7583fc0153b3ab1c733e41fcb8fa8d4bbd3c969b1898a75e6770aab9` (desde `/d/r4l`; antes, `78e596b5…06c2`, sin la parada por compilación). Es el de la v4.6 con estos cambios:
  - `partition_pbt4b_v47.py`, `p6_check_v47.py` (sha256 `ea5dead9…4c32`: la partición por archivo, método y etiqueta), `apply_red_pbt4b_v47.py` y `check_skips_copy_v47.py` (`59b7ce7a…7753`);
  - el sha256 de cada guion al empezar;
  - una salida nueva por copia (`out-<fase>-v47-<copia>`);
  - desde `/d/r4l`, **para si la compilación da errores o avisos CS**.

  Python siempre con `-I`, desde guiones escritos con Write.
- **Los guiones del paquete en `/d/r4l` y `/d/r4m`** (sha256):
  - `make_copies_pbt4b_v47.py` `2a96b2af…d247`;
  - `pbt4b_hand_edits_v47.py` `d2e4429d…c000`;
  - `partition_pbt4b_v47.py` `96f75552…9e63`;
  - `pbt4b_skips_v47.py` `b2d62116…022d`;
  - `apply_red_pbt4b_v47.py` `fed6eb54…8863`;
  - el mapa v4 `0c5abc6d…9b1c`;
  - y, sin cambios, los de la v4.5 y la v4.6 que cargan (la lista entera, en la cabecera de `out-run-*-v47-*.txt`).

## 1. `/d/r4j`: los pasos 1 a 4 con la primera redacción de la v4.7

| Paso | Resultado |
|---|---|
| 1. La copia | 12 886 archivos de `78a7cef3` |
| 2. `apply_red_pbt4b_v47.py`, modo de medida | 47 archivos, conjunto `6e05d66d…a5e` (dos motivos de `Skip` pasan a «a mano»: los de D-S5) |
| 3. La compilación | Core-tests, UI-tests y Plugin Release: **0 errores, 0 avisos CS** |
| 3. TRX de las copias | Core: 613 filas, 292 Failed y 321 Passed, 425 s (`2d98f64e…1849`). UI: 36 de 36 Passed (`47d6d325…08cb`) |
| 4. `partition_pbt4b_v47.py red` | **480 filas: 476 pruebas, 4 partidas por filas (D-S5)**, sha256 `2e67b4c4…c2f0`; **R_C = 292, R_UI = 0**. Las cuatro, con sus filas: WLHT 3/7, PEG 1/3 y las dos de WPP 7/1 |

## 2. `/d/r4k`: los pasos 5 y 6 con la primera redacción. No compila

`apply_red_pbt4b_v47.py … --partition` escribió 47 archivos (conjunto `ebfd231f…0ecf`), y `check_skips_copy_v47.py` dio 20 archivos, 0 mal y 477 `Skip`. **Core-tests no compiló** en el RED ni en el GREEN: 12 errores, todos CS0030, en `WorkspacePilotPrepareFull.cs` :128, :141 y :1016, y 0 avisos CS (`out-redgreen-v47-r4k/red-core-tests.log`, `d4821ba7…`).

**La causa es un defecto de la v4.7, no del diseño:**
- los dos miembros nuevos (`StatusNamesRed` y `StatusNamesGreen`) leían cada fila como `(string)row[0]`, pero con xunit 2.9.2 `TheoryData<string>` se enumera como `string`;
- además, un comentario de `StatusNamesRed` nombraba `StatusNamesGreen`, y la regla de «los ayudantes que alcanzan sus pruebas», que va por nombre, metía los dos en la clase Red.

**Corregido antes de la corrida de la v4.7:**
- `foreach (string name in StatusNames())`, y el comentario ya no nombra el otro miembro;
- `selftest_pbt4b_v47.py` comprueba ahora que cada clase lleva solo su miembro y que no hay `row[0]`.

La revisión, el anexo V47-A y la v4.7 se regeneraron. La v4.7 que vale es la de `b86ef9cb…40c9`. Las dos anteriores no se entregaron: `bf044fad…`, la primera, y `2a851e33…`, la de `/d/r4k`. Por eso la precondición se corrió otra vez entera, desde el paso 1.

## 3. `/d/r4l`: los pasos 1 a 4 con la v4.7

| Paso | Resultado |
|---|---|
| 1. La copia | 12 886 archivos |
| 2. Modo de medida | 47 archivos, conjunto `6e05d66d…a5e`, el mismo que en `/d/r4j`: los miembros nuevos no se alcanzan en el modo de medida (`apply-red-probe.txt`, `b1066315…a94a`) |
| 3. La compilación | **0 errores, 0 avisos CS** en los tres. Binarios: `RackCad.Application.dll` `155fce7c…f57e`, `RackCad.Tests.dll` `1f512b5d…78d5`, `RackCad.UI.Tests.dll` `68662b8c…70f2` y `RackCad.Plugin.dll` `d8be8570…c454` |
| 3. TRX | Core: 613 filas, 292 Failed y 321 Passed, 389 s (`dc9e6102…ad5e`). UI: 36 de 36 Passed (`bed02a34…a51c2a`) |
| 4. La partición | **la misma, byte a byte: `2e67b4c4a6c4a531c8fd6848f882ba43179f26599c39b0933b16bd9f0e5dc2f0`**, 480 filas y 4 partidas, R_C = 292 y R_UI = 0 (`out-probe-v47-r4l/pbt4b-copy-partition.tsv`; `partition-red.txt`, `7cf35eed…81f4`) |

**El reparto final** (el mapa v4): C-0 con **358 copias mecánicas y 70 a mano**, que es lo previsto.

## 4. `/d/r4m`: los pasos 5 y 6 con la v4.7

| Paso | Resultado |
|---|---|
| 5. El RED, `apply_red_pbt4b_v47.py … --partition` | 47 archivos, conjunto `edc90426…b1d5` (`apply-red.txt`, `3c14cc17…5366`: el sha256 de las 20 copias) |
| 5. `check_skips_copy_v47.py` | 20 archivos, 0 mal, 477 `Skip` |
| 5. La compilación | **0 errores, 0 avisos CS** en los tres. Binarios: `2e0b916a…c286`, `9ba82dad…8643`, `0cce8948…84a4` y `b1a4ad5d…72b2` |
| 5. **Core, la suite completa** | **19 586 filas: 18 836 Passed, 293 Failed y 457 omitidas**, 552 s (`core-red.trx`, `6be31172…b4f4`). **Lo previsto era 19 586 = (19 129 − 292) + 292 + 457.** Las 292 Failed de las clases Red son exactamente las de la partición, en su clase y con su número de resultados, y las cuatro partidas por filas se portan como dice D-S5. Hay **una más: `NamespaceFolderGuardTests.TestProjects_KeepExactlyOneAssemblyRootNamespace`**. Las omitidas fuera de la lista son las 7 de la base |
| 5. UI, los 15 filtros | 913 filas, 0 Failed, R_UI = 0, como prevé la partición |
| 5. `p6_check_v47.py red` | **NO CUMPLE**, solo por esa prueba (`check-red.txt`, `ac4c9abf…8cb6`; la de Core sola, `out-redgreen-v47-r4m-extra/check-red-core-only.txt`) |
| 6. El GREEN, `apply_green_pbt4b_v45.py` | 10 archivos, conjunto `f93c7262…4c45`. `check_skips_copy_v47.py`: 20 archivos, 0 mal, 477 |
| 6. La compilación | **0 errores, 0 avisos CS.** Binarios: `65b4810d…f70b`, `0107d2e1…88b8`, `1b12b632…44dc5` y `e4c01aa3…c786` |
| 6. **Core, la suite completa** | **19 586 filas: 19 125 Passed, 4 Failed y 457 omitidas**, 570 s (`core-green.trx`, `153c3b06…52bb1`). **Lo previsto era 19 586 = 19 129 + 0 + 457.** Las cuatro Failed, en §5 |
| 6. UI, los 15 filtros | 913 filas, 0 Failed |
| 6. `p6_check_v47.py green` y `partition_pbt4b_v47.py green` | **NO CUMPLE** (`check-green.txt`, `b09899b8…61b0f1`). De las 476 copias, **no pasan 2** (`partition-green.txt`, `6dbbd090…50ba`). **Las cuatro partidas por filas pasan en el GREEN** |
| 7 y 8 | **No se corren:** sin las sondas, sin el coste de `IsDirty` y sin el JSON |

## 5. Lo que cae

**Fuera de la lista de 477. Cada una es una parada que vuelve al Owner (PBT4B-C16):**

1. **`NamespaceFolderGuardTests.TestProjects_KeepExactlyOneAssemblyRootNamespace`** (la excepción de I-23), **en el RED y en el GREEN.**
   - **La regla de la guarda:** cada `.cs` de `tests/RackCad.Tests` y `tests/RackCad.UI.Tests` declara exactamente un `namespace`, igual a la raíz de su ensamblado.
   - **Lo que choca:** las 20 copias declaran `RackCad.Tests.FullPanelCopies` o `RackCad.UI.Tests.FullPanelCopies`, como manda D-PBT4-11.3 desde la v4.4 (V44-05). La guarda nombra las 20 (`out-redgreen-v47-r4m-extra/core-red-messages.txt`).
   - **Por qué no se vio antes:** el modo de medida solo corre el filtro de las copias, y esta es la primera vez que la suite entera de Core corre con ellas.
   - **Dónde está en el paquete:** es un `RequiredTest` de PB-T4b (§6.2.1, fila 30, `FullyQualifiedName~RackCad.Tests.NamespaceFolderGuardTests`, 7, `ExpectRed` false), así que el contrato tal como está no puede cumplirse.
2. **`WorkspaceHostHardeningGuardTests.NoneOfThe30LiteralsInEnglishStaysInTheHostOrTheSupportAndTheFourIdentifiersAreKept`, solo en el GREEN.**
   - **La regla de la guarda:** cuenta cuántas veces está cada texto en español de su tabla en WHC y `WorkspacePilotSupport.cs`, y el número tiene que ser exacto.
   - **Lo que choca:** con el GREEN literal (`green_pbt4b_v45.py`), tres textos tienen otra cuenta (`count_literals_p6.py`, `out-redgreen-v47-r4m-extra/literal-counts.txt`, `b04218c4…bb30`):

     | Texto | La guarda exige | En la base | En el GREEN |
     |---|---|---|---|
     | «La relectura no trajo resultado.» | 2 | 2 | **0** |
     | «El borrador se está aplicando o ya no está registrado.» | 5 | 5 | **6** |
     | «El rack no tiene escala de anotación.» | 1 | 1 | **0** |

   - **Por qué no se vio antes:** el paseo de guardas de la v4.5 la recorrió (`hs65\pbt4\v45\pbt4b-guard-walk-v45-full.tsv`) y no la marcó.

**De la lista (C-0, mecánicas, M-03), candidatas a D-S1.** Fallan igual en el RED y en el GREEN, y en la partición son Red:
- **`WorkspacePilotPrepareFull.ThePilotNeverErasesAViewAPhantomThatWasAlreadyInTheDrawingMakesTheUpdateInvalidAndIsNeverInThePlan`:** `Assert.Equal` espera `Invalid` y recibe `Ready`. Con el modelo completo, una vista fantasma ya presente no invalida la preparación;
- **`WorkspacePilotPrepareFull.InvalidE_AMutableMemberThatIsNotAvailableInTheOutgoingSystemOrWhoseEnvelopeDoesNotDecodeIsInvalidAndNeverErasesAView`:** falla con el mensaje `phantom-lateral`, por la misma causa.

Las dos fijan una conducta del piloto («nunca borra una vista») que el modelo completo no comparte. **No se han escrito sus equivalentes:** la parada de arriba manda antes, y su texto necesita fijar qué afirma el modelo completo.

## 6. Lo que sí queda medido

- **La compilación de la v4.7, con la partición, en el RED y en el GREEN:** 0 errores y 0 avisos CS.
- **La partición:** `2e67b4c4…c2f0` (480 filas: 476 pruebas y 4 partidas por filas), R_C = 292, R_UI = 0. El reparto de C-0 es 358/70.
- **El RED, fuera de la guarda de I-23, es exacto:** las 292 Failed de Core son las de la partición y en la UI no falla ninguna.
- **En el GREEN, las cuatro `Theory` de D-S5 pasan** en sus dos mitades.
- **La duración de la suite de Core:** 552 s en el RED y 570 s en el GREEN.

**No medido:** las sondas de CT-06 y del coste, el coste de `IsDirty`, `MinSelected` y el JSON.

## 7. Las copias en disco

`/d/r4g` a `/d/r4m` siguen en disco con sus `bin/obj`.
- **`/d/r4g` a `/d/r4l` se pueden borrar a mano ya.**
- **`/d/r4m` también,** cuando la sesión no necesite mirar sus TRX. Están en `/d/r4m/_trx/` y aquí van por sha256.

La próxima corrida usará carpetas nuevas. `/d/r4a` a `/d/r4f` no son de esta precondición.
