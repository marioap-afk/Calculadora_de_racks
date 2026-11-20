# Sustituto de nc2 v2 (PREPARACION; requiere revision independiente antes de usarse)

Script: `nc2_exact_whitelist_substitute_f4b_v2.py`, SHA-256 `C033E96CE147963FEB9C04733E9BFBEC7493FEA33D8FF79B1C2E8AB860798E45`.
Base (v1 custodiado, intacto, copia solo para el diff): `base-6ccaf12d-original.py`, SHA `6ccaf12dbf9122c0bddc5b724bcff573705c95698af38bf750bf69459ee2ffd3`.
Diff: `adapt-diff.txt` (19 hunks; 16 lineas quitadas, 83 anadidas).

## Cambios
1. Propiedad 8: `F4B_EXACT_EVIDENCE` -> `F4B_EXACT_EVIDENCES`, tupla cerrada de 2 literales: PB-T2 (el del v1) y PB-T3 (`ExpectedEvidence[0]` del contrato de 485b5651, byte a byte; `verify_literals.py` lo comprueba contra el contrato de PB-T2a, el de PB-T3a y el `git show` de 485b5651). La entrada de `ExpectedEvidence` debe ser igual, entera y exacta, a uno de ellos (con guarda: `ExpectedEvidence` debe ser lista). Mide `ExactEvidenceMatched` (PB-T2 / PB-T3). El resto de la propiedad 8 (bfc Result PASS, ElementwiseInOrder true, AllowedWriteScope en Fields, A3-A5 llanos, ma != ca, opacas iguales) intacto.
2. `--task` (PBT2A, PBT3A, PBT3B, PBT3C, PBT4, PBT5, VALIDATION): el id (`<TAREA>-NC2-EXACT-WHITELIST-F4B-01`), el registro (`SATISFIED_UNDER_<TAREA>_...`) y el ambito (TaskId, intento y RunId de la verificacion, leidos de los datos) salen del argumento y de los datos; el texto no se edita por uso. Ligadura: el TaskId de la delegacion real y el de la copia de nc2 empiezan por `F4B-<TAREA>-` (si no, FAIL). VALIDATION: cualquier TaskId `F4B-`, sin registro (`VALIDATION_ONLY_NO_RECORD`). Sin `--task`, FAIL escrito. Se anade `ScriptSha256` al resultado.
3. Autoprueba: los 50 casos anteriores intactos; nuevos AY (literal de PB-T3: pasa), AZ (sin "ocho"), BA (espacio de mas dentro), BB (espacio final), BC (punto en vez de punto y coma), BD (inicial en mayuscula), BE (literal de PB-T2 con "ocho"), los seis fallan solo la propiedad 8; y T1-T6 (ligadura de --task). Total 63, determinista.

## Resultados
- PB-T2a nc2 `a799` frente a `10a9`, `--task PBT2A` (planificacion `7de7`): PASS, 10/10, evidencia PB-T2 (`result-pbt2a.json`).
- PB-T3a nc2 `ad66` frente a `6d41`, `--task PBT3A` (planificacion `fc50`): PASS, 10/10, evidencia PB-T3 (`result-pbt3a.json`).
- Contrastes (`checks/`): el v1 sobre PB-T3a da FAIL solo en la 8; `--task PBT3B` o `PBT4` sobre datos de PB-T3a dan FAIL por la ligadura; sin `--task` da FAIL; VALIDATION pasa sin registro.

## Dudas
- PBT2A y PBT3A entran en `--task` (hacen falta para los dos casos reales aunque la lista del encargo no los nombra).
- VALIDATION aqui no se limita a datos dorados (como en nc1, que tiene pares de RunId dorados); solo exige TaskId `F4B-` y no registra.
- Un paquete futuro (PB-T3b en adelante) cuyo contrato redacte la entrada de otra forma dara FAIL: es lo pedido (lista cerrada); anadir un literal es otro cambio revisado.
- Los datos se copiaron del worktree (lectura, con la autorizacion del encargo): solo `inputs/`, `output.json` y `mutation.json`; el contrato de PB-T3a tambien se comprobo contra `git show 485b5651`.
