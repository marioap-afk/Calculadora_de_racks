# La custodia del sustituto de nc2 v3 (F4B-E58): lo que entra y lo que no

**Qué es esta carpeta.** El sustituto de nc2 `PBTx-NC2-EXACT-WHITELIST-F4B-01` v3 (`nc2_exact_whitelist_substitute_f4b_v3.py`, SHA-256 `E68B6E5993B522DF83E7CD4CD2B136DDF503E237FD560B1A97067F24664328C6`), con su trabajo, su revisión independiente y el diagnóstico de la sesión que llevó a ella. Lo custodia el commit del cierre de PB-T4d (F4B-E59), por la decisión del Owner «Sustituto v3 y revisión (Recomendado)» (`F4B-DRAFTS/coordinator/owner-pbt4d-nc2-decision.md`). La v2 sigue en `F4B-DRAFTS/nc2-substitute-f4b-v2/`, sin tocar. El manifiesto de esta carpeta, `custody-manifest.json`, da el SHA-256 de cada archivo y lo que se hizo con él.

**Lo que entra,** con la redacción común para el repo público:
- **el script, byte a byte,** con su SHA-256: no lleva rutas locales;
- **sus generadores:**
  - `_make_v3.py`, que escribe la v3 desde la v2 revisada (`C033E96C…`) con reemplazos exactos: un solo cambio de lógica, la propiedad 10 en su forma cerrada, más el docstring y el campo informativo `AlternativeCandidatesExcludingPath`;
  - `_make_apply_v3.py`, que escribe el lanzador de la cadena para la v3 desde el de la v2;
- **la revisión,** en `review/`: el informe, `independent-review-substitute-v3.md`, y los guiones y las salidas de texto que deje la revisión;
- **el diagnóstico de la sesión,** en `diag-sesion/`: `diag_nc2_p10.py` y su salida, `diag-nc2-p10-result.json`.
  - **No es evidencia ni un sustituto:** corre la v2 revisada con la propiedad 10 en su forma cerrada, cambiada solo en memoria, sobre el nc2 real de PB-T4d. Da PASS en las diez propiedades y 63 de 63 autopruebas.
  - Fue el apoyo de la pregunta al Owner, y no lo que satisface nc2: eso lo da la corrida de la v3, ya revisada.

**La redacción:**
- Las rutas locales pasan a marcadores (`<perfil-de-usuario>`, `<worktree-de-I-64>`…), como en las carpetas de los sustitutos anteriores.
- Los JSON siguen siendo JSON válido y ya no son byte a byte la salida del script; ningún SHA-256 exigido depende de ellos.
- Los finales de línea CRLF pasan a LF.

**Lo que no entra como texto, solo por su SHA-256 en el manifiesto:**
- lo que está bajo `__pycache__/`, `data/`, `mut/` o `mut2/` (copias de trabajo y salidas de ejecución);
- los `.trx` y los `.log`;
- lo que no es UTF-8;
- lo que, tras la redacción, aún lleva una ruta o un nombre local;
- lo que ya está custodiado con el mismo SHA-256.

**Las corridas** no están en esta carpeta, sino en sus unidades de `F4B-PBT4-RETIRE-PILOT-nc2-substitute/1/`, con su manifiesto:
- la de la v2, que acabó sin veredicto (`MemoryError`), se conserva tal cual;
- la de la v3 también está allí. Su copia del script es un duplicado de la de esta carpeta, que es la canónica.
