# La custodia del sustituto de nc2 v2 (F4B-E43): lo que entra y lo que no

**Qué es esta carpeta.** El sustituto de nc2 ampliado, `PBTx-NC2-EXACT-WHITELIST-F4B-01` v2 (`nc2_exact_whitelist_substitute_f4b_v2.py`, SHA-256 `C033E96CE147963FEB9C04733E9BFBEC7493FEA33D8FF79B1C2E8AB860798E45`), con su trabajo y su revisión independiente (APTO, B 0 / I 0 / M 4). La custodia la escribe el commit X de la emisión de PB-T3b (F4B-E44). Esta nota es nueva; `notes.md` no se edita, porque tiene su propio texto de trabajo.

**Lo que entra,** con la redacción común para el repo público:
- **el script, byte a byte,** con su SHA-256: no lleva rutas locales;
- **el trabajo de la v2:** `notes.md`, `adapt-diff.txt`, `limits-declared.md`, `checks-summary.txt`, `real-cases-summary.txt`, `result-pbt2a.json`, `result-pbt3a.json`, `run-pbt2a.txt` y `run-pbt3a.txt`;
- **sus ayudantes,** que cita `notes.md`: `verify_literals.py`, `summarize.py`, `summarize_checks.py` y `checks/` (las corridas de contraste);
- **la revisión,** en `review/`:
  - el informe entero, `independent-review-substitute-f4b-v2.md` (el del v1 con su recomprobación y su sección «Recomprobación v2»);
  - los guiones y las salidas de texto de la ronda de la v2: `probes_v2.py`, `probes_v2_mr.py`, `selftest_matrix_v2.py`, `summarize_runs.py` y `verify_literals_own.py`, con sus salidas;
  - los guiones y las salidas de texto de las rondas del v1 que cita el informe: `dupkeys_check.py`, `firma_check.py`, `inspect_data.py`, `mutations.py`, `mutations2.py`, `selftest_matrix.py` y `selftest_matrix2.py`, con `run-selftest-matrix.txt`, `recheck/*.txt` y `runs/*.txt`.

**La redacción:** las rutas locales pasan a marcadores (`<scratchpad>`, `<checkout-principal>`…), como en el dorado del sustituto de nc1. Los JSON (los resultados de las corridas reales y de los contrastes) siguen siendo JSON válido, sin nota, y ya no son byte a byte la salida del script; ningún SHA exigido depende de ellos. `limits-declared.md` cita la revisión por su ruta en esta carpeta, con su nota. Los finales de línea CRLF pasan a LF.

**Lo que no entra, y por qué:**
- **`data/`** (57 archivos, 242 067 471 bytes): las copias de trabajo de las corridas reales de PB-T2a y PB-T3a que leyó el sustituto (la verificación, nc2 y la planificación, con las TRX de la CI de hasta 26 MB). Los originales ya están custodiados en sus carpetas de corrida, en `docs/automation/evidence/I-64-pilot/`.
- **`base-6ccaf12d-original.py`:** la copia de trabajo del v1 para el diff. Es, byte a byte, el v1 custodiado en `F4B-PBT2A-DRAFT-STATE-PORT-nc2-substitute/2/R20261119T125505Z-c2d3/nc2_exact_whitelist_substitute_f4b.py` (SHA-256 `6ccaf12d…`).
- **Los datos crudos de las rondas del v1** (`review/mut/`, 1 329 487 737 bytes, y `review/mut2/`, 3 263 304 010 bytes): las copias mutadas de las corridas, con las TRX de la CI. Se rehacen con `mutations.py` y `mutations2.py` sobre las corridas custodiadas.
- **Los resultados JSON de las rondas del v1** (`review/recheck/*.json`, 29 archivos, 345 765 bytes, y `review/runs/*.json`, 13 archivos): las salidas crudas del sustituto sobre las copias mutadas. Sus resúmenes de texto (`recheck/*.txt` y `runs/*.txt`) sí entran.

**La copia del informe del v1 ya custodiada** (`F4B-DRAFTS/pbt2a-nc2-substitute/independent-review-substitute-f4b.md`) es evidencia histórica: no se toca.
