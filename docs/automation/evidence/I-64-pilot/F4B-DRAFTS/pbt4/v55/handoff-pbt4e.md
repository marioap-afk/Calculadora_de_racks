# Traspaso de PB-T4e (paquete v5.5): la recarga de «Editar todo»

**Fecha:** 2026-11-22. **Base:** el cierre de PB-T4d, `160e019d773ba9095e2d5858cd593a954b8eec67` (F4B-E59). **No se emite nada y no hay commits:** la sesión emite tras la comprobación acotada.

Las carpetas: `S = <scratchpad>\hs65`. Lo escrito, en `S\pbt4\` (las piezas y sus herramientas, archivos nuevos de PB-T4e), `S\pbt4\v55\` (el generador, las fuentes, las sondas, el armado de la v5.5 y este traspaso) y `S\pbt4ecc\compile\` (la precondición 6). Las reglas de siempre: nada en el worktree de la rama ni en los archivos en disco del checkout principal (solo `git show`, `archive`, `ls-tree`, `grep`); Python con `python -I <archivo>`; guiones en ASCII; copias exportadas nuevas (`/d/r71` en adelante), nunca reutilizadas; la UI, solo los filtros del contrato, cada uno en su proceso; UTF-8 con LF.

## 1. Estado

**HECHO, todo lo previsto** (`pbt4e-precondition6.json`, `Outcome.AllAsExpected` = true; `compile-check.md`: SI):
- **La v5.5** (`pbt4/I-64-f4b-pbt4-gate-contract-package-v55.md`, 1 003 976 bytes): la v5.4 entera más PB-T4e. «Lo que cambia» sale del diff de las secciones (`v55/make_v55-out.txt`): §0, §1, D-PBT4-01, D-PBT4-19, D-PBT4-20, §5.4, §5.5, §6.0, §7, §8.3, §8.5, §9, §10 y §11; nuevas: la sección «PB-T4e» de §3, D-PBT4-22, §4.5, §5.7, §6.5, §6.5.1, §6.5.2 y el anexo V55-A; 99 secciones sin cambio (PB-T4a a PB-T4d enteras, comprobado).
- **El diseño** (D-PBT4-22): `EditAllOfPilot` llama a `TryReloadPilotEdit(request)` en una rama `else if (draft.NeedsLazyReload)` tras la de «sin borrador», antes de las comprobaciones del estado y de `ShowModal`; si no da `Reloaded`, devuelve `EditAllOutcome.NotBegun(<la recarga>)` y no abre. FVM: con `NotBegun` + `NotAdmitted` y `view.Exists`, el aviso «No se recargó: …» (constante `NotReloadedText`, que usa también `BeginEdit`). Comentarios de WEI y del puerto. Nada más visible.
- **Las dos propuestas para la sesión:** D-S17 (la recarga en el anfitrión) y D-S18 (`NotBegun` reutilizado), en D-PBT4-22 y §8.5, con evidencia y efecto en las CT.
- **Las CT:** CT-PBT4E-01 (6 líneas), 02 (4) y 04 (5) en `PushBackEditAllReloadTests` (Core) y CT-PBT4E-03 (9) en `PushBackEditAllReloadFieldTests` (UI): 24 líneas, todas de la sonda sobre el GREEN, tal cual. La CT-05 opcional no se escribe.
- **La lista: vacía,** medida: K = 0, D = 0, E = 0.
- **La precondición 6:** `/d/r71` (ensayo y sonda), `/d/r72` (principal, sola: 0 procesos al empezar y al acabar), `/d/r73` (base con cobertura, sola), `/d/r74` y `/d/r75` (control). 17 compilaciones, 0 errores y 0 `warning CS`. Core RED 19421 = 19015 + 3 + 403 (solo las 3 CT, por aserción en su `Finish`); GREEN 19421 = 19018 + 0 + 403; base 19418 = 19015 + 0 + 403. UI, 19 filtros en su proceso: RED solo cae CT-03; GREEN 0 `Failed`; totales de la CI: RED 2465 = 2416 + 1 + 48, GREEN 2465 = 2417 + 0 + 48. Cobertura: PASA, 0 sin ejecutar, 0 ramas perdidas, 1 línea de texto cambiado (FVM:410, bien), 6 nuevas con código (bien), bajan 0. Control: solo el campo → caen las 3 de Core, pasa CT-03; solo el anfitrión → al revés. `RequiredTests`: 90 (88 + 2), sin solapamientos.
- **Medido distinto de la propuesta** (R-T4-39): sin la recarga, la receta de punta a punta da `NotAppliedPreflight` («Ningún sobre editable de la relectura coincide con el origen del borrador.»), no el conflicto.

**Falta (de la sesión, no de este relevo):** confirmar D-S17, D-S18 y el `TaskId`, y registrarlas; la comprobación acotada; la custodia (los números F4B-E, la entrada «## PB-T4e» de `superseding-characterization.md`, la redacción de las rutas de las TRX y los JSON de coverlet); el generador del contrato (`ForbiddenWriteScope`, `<medido por el generador>`).

## 2. Dónde está cada pieza

| Pieza | Ruta (relativa a `S`) | Cómo se rehace |
|---|---|---|
| La pieza del GREEN | `pbt4/pbt4e-green-v55.txt` | `python -I make_pieces_pbt4e_v55.py "<repo>" 160e019d make_pieces_v55-out.txt` (desde `pbt4/v55/`), desde `v55/frag/green-src.txt` |
| La pieza del RED | `pbt4/pbt4e-red-v55.txt` | El mismo guion, desde `v55/ct/PushBackEditAllReloadTests.cs` y `v55/ct/PushBackEditAllReloadFieldTests.cs` |
| Los módulos y aplicadores | `pbt4/green_pbt4e_v55.py`, `pbt4/apply_green_pbt4e_v55.py`, `pbt4/red_pbt4e_v55.py`, `pbt4/apply_red_pbt4e_v55.py` | Escritos a mano (copias de los de PB-T4d con la pieza de PB-T4e) |
| La sonda de las CT | `pbt4/v55/probe/write_probe_v55.py`; la de «Actualizar», `pbt4/v55/probe/SondaPushBackEditAllReloadUpdate.cs` | — |
| La precondición 6 | `pbt4ecc/compile/` | Las órdenes, abajo |
| La v5.5 | `pbt4/I-64-f4b-pbt4-gate-contract-package-v55.md` | `python -I make_v55.py "<repo>" make_v55-out.txt` (desde `pbt4/v55/`), con `v55/frag/head.md` y `ops-a.md` a `ops-d.md` y `pbt4ecc/compile/values-v55.json` |

## 3. Las órdenes de la precondición 6, en orden (desde `pbt4ecc/compile/`)

1. `bash t71_trial_v55.sh /d/r71 2>&1 | tr -d '\r' > driver-r71-tr.txt` (el ensayo y la sonda);
2. `bash p6_pbt4e_v55.sh /d/r72 2>&1 | tr -d '\r' > driver-r72-p6.txt` (la principal, sola) y `bash p6_basecov_v55.sh /d/r73 2>&1 | tr -d '\r' > driver-r73-bc.txt` (la base, sola);
3. `bash p6_control_v55.sh /d/r74 campo …` y `bash p6_control_v55.sh /d/r75 anfitrion …` (el control);
4. `python -I req_pbt4d_v55.py "<repo>" 160e019d required-pbt4d-emitted.tsv`;
5. `python -I cov_p6_pbt4e_v55.py "<repo>" ../../pbt4/pbt4e-green-v55.txt cov-p6-v55.txt BASE_CORE=r73-bc/base-core-cov.coverage.json BASE_UI=… GREEN_CORE=r72-p6/core-green-cov.coverage.json GREEN_UI=…`;
6. `python -I required_pbt4e_v55.py required-pbt4d-emitted.tsv required-pbt4e.tsv required-pbt4e.txt BASE_CORE=r73-bc/base-core-cov.trx BASE_UI=r73-bc/base-uifNN.trx… RED_CORE=r72-p6/core-red.trx RED_UI=r72-p6/red-uiNN.trx… GREEN_CORE=r72-p6/core-green.trx GREEN_UI=r72-p6/green-uiNN.trx…`;
7. `python -I p6_json_pbt4e_v55.py "<repo>"` (escribe `pbt4e-precondition6.json`, `compile-check.md` y `values-v55.json`);
8. `python -I make_v55.py "<repo>" make_v55-out.txt` (desde `pbt4/v55/`).

## 4. Los SHA-256

Del texto con `\n` (las piezas, los guiones y las clases) o de los bytes (los registros y el JSON):

| Archivo | sha256 |
|---|---|
| `pbt4/I-64-f4b-pbt4-gate-contract-package-v55.md` | `e597790ff700a2909b77c21960b5b013b2b108cf8073f2b866a0face25f8ae83` |
| `pbt4/pbt4e-green-v55.txt` | `5d26dc483f83cfca1d005adfd60c25f6d2e5eac7cc545b9342182f96bdc95407` |
| `pbt4/pbt4e-red-v55.txt` | `647e185329ce18450eccda093d3071554759043bfc1c61e27e3f52f621da033b` |
| `tests/RackCad.Tests/PushBackEditAllReloadTests.cs` (NewFileSha) | `47594d48273cf04746f7d2b0c6456f7614c12b7124168553e156f31d50c1c81a` |
| `tests/RackCad.UI.Tests/PushBackEditAllReloadFieldTests.cs` (NewFileSha) | `a29d7a362cd5de49bd787eb76e7c847dc30b46ef1314e94ae5f67041b6a4595d` |
| GREEN `src/RackCad.Application/Workspace/WorkspaceHostCore.cs` | `fa19d28ca897125d13af64bd58c06bf5c513427f780ca9ef83aa1ae9cbc0e202` |
| GREEN `src/RackCad.Application/Workspace/WorkspaceEditIntent.cs` | `bbc6882283833b02bab79c9bb70c082f3ac14b2a88650722bf8d72be0e13b5e2` |
| GREEN `src/RackCad.Application/Workspace/IWorkspacePilotEditPort.cs` | `49b5245c112e3d4f0b3b95114f9ff0444469b316a3ef912947cf3b696e0c6ebd` |
| GREEN `src/RackCad.UI/Workspace/WorkspacePilotFieldViewModel.cs` | `c4b5c6990c5623e53be607fdbb461db95103b957c96627e1d63300dabe3d643b` |
| `pbt4/green_pbt4e_v55.py` | `888c9fb21832965a40a7e4c306bb21635d8c912a5c4f02f576568631dc38aee1` |
| `pbt4/apply_green_pbt4e_v55.py` | `832afa9e4eafb298b92a47794b37b38d99dd1b1e23a9eb2679d79ee509be65c0` |
| `pbt4/red_pbt4e_v55.py` | `6727c6f20839ef28ae5895f938b2263abc4e5538a5f30baeee6b041d477fac2f` |
| `pbt4/apply_red_pbt4e_v55.py` | `50b1919438ed52a4da5ee6ce60bebacb104087bd24c74b9ff7ccea0c900e9b96` |
| `pbt4/v55/make_pieces_pbt4e_v55.py` | `9ffcb6b8ffdd02ccd5a3773a26cc1a08870eb666448f3792015b62f90a59784c` |
| `pbt4/v55/make_v55.py` | `47fa152b9fd66a8e99f043de318c9c09777b6ab0f9f68bee9bc01e9b50e178ae` |
| `pbt4ecc/compile/pbt4e-precondition6.json` | `ef33dca0f718bdc09a7e52369ea2534612a0ac8039ae75dc784f811262f26d21` |
| `pbt4ecc/compile/compile-check.md` | `1fd30380e7e0fa43c3e9df3a5e1af081690fb131404ed126edf6159dcafb505e` |
| `pbt4ecc/compile/cov-p6-v55.txt` | `70f0e41b0963929be5b4d91907e01008a7018e075a4565bf871994e9b817b2ad` |
| `pbt4ecc/compile/required-pbt4e.tsv` | `a66fd8d40972f7447f79a9a702079b465649dd38c1797c231c361e5ee75b03e6` |
| `pbt4ecc/compile/driver-r72-p6.txt` | `f6d02b559337a9a39fe1b70d9c0b58ecaf5c7f27797179bb18753cac1481cea4` |
| `pbt4ecc/compile/driver-r73-bc.txt` | `1461a8b11f7aefa3f6cfbc3ff3bb3e03f98f13c7fa14c9fb6b611a182b154427` |

## 5. Deslices y lo pendiente

**Deslices, declarados:**
1. Un `python -I -c "print(1)"` (solo imprimió 1), frente a la regla «nunca `-c`».
2. Un `python -I -` con un heredoc vacío (no ejecutó nada; la orden se quedó esperando y se paró con TaskStop), frente a la regla «nunca `python -` ni heredoc». Es el mismo desliz que declaró el redactor de PB-T4d.
3. Un ensayo de `make_v55.py` con valores de prueba (`<scratchpad>\dry55\`, fuera de `hs65`) escribió una v5.5 provisional en `pbt4/`; la versión final la sobrescribió con los valores medidos.
4. Se leyeron (sin escribir ni correr nada allí) guiones de `pbt4dcc/compile/` y `pbt4ccc/compile/`; tres se copiaron sin cambio a `pbt4ecc/compile/` (`check_trx_v55.py`, `classes_v55.py`, `cov_v55.runsettings`; mismos sha256).

**Desviaciones de la propuesta de lista, declaradas en la v5.5:** la clase de UI se llama `PushBackEditAllReloadFieldTests` (no `…WindowTests`: no abre ninguna ventana); el GREEN toca también `IWorkspacePilotEditPort.cs`, solo el comentario de `TryEditAll`; no hay CT-05; no hay paseo de guardas por marcas aparte (vale la suite completa de Core y los 19 filtros, R-T4-27); y, en el RED, la receta de punta a punta da `NotAppliedPreflight`, no `NotAppliedConflict` (R-T4-39).

**Pendiente para la sesión:** ver §1. Si cambia D-S17 o D-S18, cambian el GREEN y CT-02 y CT-03, y la precondición 6 se repite entera en copias nuevas (`/d/r76` en adelante).
