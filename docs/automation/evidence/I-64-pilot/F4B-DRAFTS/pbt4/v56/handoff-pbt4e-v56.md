# Traspaso de PB-T4e, la v5.6 (tras CA-T4e)

**Fecha:** 2026-11-22. **Base:** `160e019d`. **Encargo:** la v5.6 con D-S19 (M-01), los límites y la corrida opcional de la cobertura (M-02), un solo armado tras `post-p6` (M-04), M-03 y M-05 solo con la cita del registro de la sesión, y la precondición 6 entera en copias nuevas desde `/d/r76`; y lo que pidió el generador sobre la v5.5 (PBT4E-C11 literal con la v3 de nc2, P-01 con la Snapshot L, las autoridades de §6.5.2 y la hora de `p6_json`). Sin commits; las piezas de la v5.5 no se tocan.

## Estado: HECHA, sin emitir

| Qué | Archivo | sha256 |
|---|---|---|
| **La v5.6** | `pbt4/I-64-f4b-pbt4-gate-contract-package-v56.md` (1 029 232 bytes) | `6b8939ae26d39392824108fe9b73bc3741f30a45657810a2ecf534e9a40cf66a` |
| La pieza del GREEN | `pbt4/pbt4e-green-v56.txt` | `a745c2737a7e0fe908489a1bf17ed86569577ed3dbf54ecd3661c488faf88158` |
| La pieza del RED | `pbt4/pbt4e-red-v56.txt` | `d052f73d726817f4240f3b5edfd89a0ec8aeb7b9cc17dbce684aa8e4f433c9e1` |
| La CT de Core (`NewFileSha`) | `tests/RackCad.Tests/PushBackEditAllReloadTests.cs` | `9e849864709a964a864902c6f44e5d92014d962cc56c374cebfc7dfe42c6b017` |
| La CT de UI (`NewFileSha`, sin cambio) | `tests/RackCad.UI.Tests/PushBackEditAllReloadFieldTests.cs` | `a29d7a362cd5de49bd787eb76e7c847dc30b46ef1314e94ae5f67041b6a4595d` |
| WHC del GREEN (los otros tres, como en la v5.5) | `src/RackCad.Application/Workspace/WorkspaceHostCore.cs` | `6eb9bf7ab81666b33c1280c832257624937c95a042e46f53dcc2c371baac4d7d` |
| El resumen de la precondición 6 | `pbt4ecc/compile/pbt4e-precondition6.json` (todo lo previsto: SI, 24 comprobaciones) | `daea59847a3629111073f0b7565d9144be90c880cf2ba61c98aae91373c9c169` |
| Su registro | `pbt4ecc/compile/compile-check.md` | `549766ebfb0ab0e2d75d33e334faab1e5f66454902f31b87e7b8f97818bbd426` |
| Los valores | `pbt4ecc/compile/values-v56.json` | `d466455ac0f437172791a9f556580e2eecda04c6df739296d9b89091b7963f9c` |
| La cobertura | `pbt4ecc/compile/cov-p6-v56.txt` | `057591b797c948e0098f5ec7ea3679b5e87867a56553efa7530e7108f4f536b7` |
| La cobertura opcional | `pbt4ecc/compile/cov-p6-existentes-v56.txt` | `3887c2efe2dd6e5ee3f15f1e1fb77e91e34dc176b3e60d1f8dc88a89a86032f8` |
| Los RequiredTests | `pbt4ecc/compile/required-pbt4e-v56.tsv` | `a66fd8d40972f7447f79a9a702079b465649dd38c1797c231c361e5ee75b03e6` |
| La principal | `pbt4ecc/compile/driver-r83-p6.txt` | `fa0ede42e4a45655b7632b0c824f9bceeb537c3e35e7c694b714063990d65df6` |
| El registro de `post-p6` (nombra la v5.6 `6b8939ae`) | `pbt4ecc/compile/post-p6-v56-out.txt` | `36f88c523c894cd956290bc3856ffc2028d535b9eb1708a62b6c0595fae1b485` |
| El registro del armado | `pbt4/v56/make_v56-out.txt` | `53e884a8d0e499cbba2a921db0fa744ac69bcb89dbfb91fbdde86ab0dbb5cb51` |
| La v5.5, guardada sin cambio | `pbt4ecc/compile/pbt4e-precondition6-v55.json` y `compile-check-v55.md` | `ef33dca0…6d21` y `1fd30380…505e` |

**Lo que cambia frente a la v5.5** (el diff de las secciones): §0, §1, «PB-T4e» de §3, D-PBT4-22, §4.5, §5.7, §6.0, §6.5, §6.5.1, §6.5.2, §7, §8.5, §9 y §11; nuevo, el anexo V56-A (sale el V55-A). **Sin cambio:** 109 secciones: las 99 de PB-T4a a PB-T4d (sin cambio desde la v5.4) y 10 más (D-PBT4-01, D-PBT4-19, D-PBT4-20, §5.4, §5.5, §8.3, §10, el registro v5.4 → v5.5, la cabecera de la v5.4 y el archivo de CT-PBT4E-03 del anexo).

## Lo medido

- **La línea nueva** (`marcado-sin-decidir`, CT-01): con el GREEN de la v5.5 (`/d/r76`), `estado=Failed motivo=[the comparer threw] … fallos=[WorkspaceHostCore.TryEditAll]`, y CT-01 cae; con el de la v5.6 (`/d/r82` y `/d/r83`), `estado=Refused motivo=[El borrador no admite el editor completo en su estado actual.] … lecturas=0 fallos=[]`, y pasa. En el RED ya da lo de hoy (la línea no hace caer CT-01 por sí sola). Las otras 24 líneas, iguales con los dos GREEN.
- **La principal** (`/d/r83`, sola: 0 procesos a las 20:11:52 y a las 21:09:18): Core RED 19421 = 19015 + 3 + 403 (solo las tres CT de Core); GREEN 19421 = 19018 + 0 + 403; los 19 filtros de UI, cada uno en su proceso: RED, solo CT-PBT4E-03; GREEN, 0 Failed; las uniones del GREEN, 900 = 870 + 0 + 30 tres veces. UI de la CI: RED 2465 = 2416 + 1 + 48, GREEN 2465 = 2417 + 0 + 48. **La base** (`/d/r79`): Core 19418 = 19015 + 0 + 403 (la de la CI del cierre de PB-T4d), la unión 899 = 869 + 0 + 30. 21 compilaciones, 0 errores, 0 `warning CS` (el Plugin incluido).
- **La cobertura:** PASA (773 archivos; 69 844 líneas y 28 820 ramas de la base; 0 sin ejecutar ni ramas perdidas; 8 líneas nuevas, todas ejecutadas); bajan, informativo, 2 VARIA (`RackViewBatchDialogPresenter.cs:37-38`, en las uniones).
- **La corrida opcional** (M-02): Core sin las dos clases, 19418 = 19015 + 0 + 403 (el total de la base); la unión de los 18 heredados, 899 = 869 + 0 + 30 tres veces; **PASA**, con el mismo resumen. Informativo: sin las clases nuevas, 6 de las 8 líneas nuevas no se ejecutan (la rama de la recarga, WHC:1216-1219, y FVM:482; WHC:1215 con 3 de 4 ramas): solo las CT nuevas llegan a la recarga, como dice §4.5.
- **El control:** solo el campo (`/d/r80`): las 3 CT de Core caen y CT-03 pasa; solo el anfitrión (`/d/r81`): las 3 de Core pasan y CT-03 cae. **RequiredTests:** 90 (88 + 2), sin solapamientos (el heredado 84 con `ExpectRed` false, como en la v5.5).

## Desviaciones e historia

1. **`/d/r77`, historia:** el primer GREEN (`8e3e8077…aafb`, sobrescrito) daba un `warning CS8604` (`begunHere = draft == null`); el final usa `if (draft == null)` con `begunHere = true` dentro.
2. **`/d/r78`, historia:** un `sed` de renombrado cambió en `p6_pbt4e_v56.sh` la lista de filtros a `ui_filters_pbt4e_v56.txt`, que no existe, y no corrió ningún filtro de UI. Corregido (lee `ui_filters_pbt4e_v55.txt` y comprueba que tiene 19 líneas) y repetido en `/d/r83`.
3. **Tres ejecuciones de `post_p6_v56.sh`:** la 1.ª (`post-p6-v56-out-intento1.txt`) corrió el control r80 y r81, que vale y no se repite, y falló por las salidas de r78 que faltaban, sin escribir ninguna v5.6. La 2.ª (`post-p6-v56-out-intento2.txt`) armó una v5.6 `38efafdb47020551b5e5c775638b29363118fd50af028929139067b3c3362a47` (1 028 892 bytes; guardada en `v56/historia/v56-38efafdb.md`, con su `make_v56-out` en `v56/historia/make_v56-out-38efafdb.txt`). Al revisarla vi que su «Lo que no cambia» atribuía las 109 secciones a PB-T4a a PB-T4d. Corregí `make_v56.py` para que nombre las 99 y las 10 más (con una aserción) y repetí `post-p6` entero. La 3.ª, `post-p6-v56-out.txt`, dio resúmenes, registros, valores, cobertura y RequiredTests iguales byte a byte y armó la `6b8939ae`. **Lo único que cambia frente a la `38efafdb` es la línea 9** (la frase «Lo que no cambia» de la cabecera).
4. **Lo añadido después de r78:** `p6_json_pbt4e_v56.py` registra y comprueba los totales de la corrida opcional (`COVX_CORE` y `COVX_U18`, citados en §4.5 y §6.5.1) y nombra bien la copia de la sonda (`driver-r82-tr.txt` y `/d/r82` en `compile-check.md`); `post_p6_v56.sh` no repite un control ya corrido.
5. **Los ensayos en seco de `make_v56.py`:** con valores de prueba (`v56/ensayo_values_v56.py` → `v56/_ensayo/`, borrada); no son versiones. `v56/same_extra_v56.py` (solo lee) nombró las 10 secciones de más.
6. **Desvíos menores:** una vez `python -I -m py_compile` (comprobación de sintaxis; su `__pycache__` está borrado); y dos archivos auxiliares escritos por error en `/tmp` (una copia de seguridad de `p6_json` y una lista de rutas), borrados en seguida.
7. **Reutilizados sin cambio:** `p6_basecov_v55.sh`, `req_pbt4d_v55.py`, `check_trx_v55.py`, `classes_v55.py`, `cov_v55.runsettings`, `control_pieces_v55.py`, `trx_lines_v55.py`, `ui_filters_pbt4e_v55.txt` y `ui_union_v55.txt`.
8. **PBT4E-C11, una lectura:** «la lista de aceptaciones puntuales con la de PB-T4d» la escribí como «the satisfaction of the nc2 of PB-T4d under the version 3 (F4B-E58, for F4B-PBT4-RETIRE-PILOT, attempt 1) do not extend».

Sin cambios en el checkout principal (`git status` vacío tras cada copia); nada en el worktree; sin AutoCAD.

## La segunda ejecución de `make_v55.py` (CA-T4e, M-04)

`post_p6_v55.sh` armó una v5.5 `a241bfbe6d2a18d5fba9d154aa0975a9c38031c1d121074bb04827d541cc103d` (1 003 244 bytes). Después cambié dos fragmentos y volví a ejecutar `make_v55.py`, que dio la `e597790ff700a2909b77c21960b5b013b2b108cf8073f2b866a0face25f8ae83` (1 003 976 bytes), la que revisó CA-T4e:
- **`v55/frag/ops-d.md`, la fila R-T4-39 de §7:** añadí la interpretación del motivo medido: que la relectura no encuentra el origen por su texto (A-T3-8; deducido del motivo), que con un cambio de fuera que no toque el sobre de origen el camino puede ser el del conflicto (no medido), y que no cambia PB-T4e;
- **`v55/frag/head.md`, «Lo que sale de la v5.5»:** añadí `post_p6_v55.sh` y su registro, `values-v55.json` y `required-pbt4d-emitted.tsv`, y los tres ayudantes copiados sin cambio (`check_trx_v55.py`, `classes_v55.py`, `cov_v55.runsettings`).

Las cifras no cambiaron: los valores son los mismos de `values-v55.json`. La `a241bfbe` no se conservó. La autoridad de la v5.5 es `make_v55-out.txt` (`e597790f`).
