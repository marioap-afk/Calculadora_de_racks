# F4B-SMOKE-PB/run: la evidencia del Smoke-PB (PB-T5)

**Qué es:** la corrida física del Smoke-PB contratado de PB-T5 (F4B-E62), intento 1 de 3, sobre el Candidato `ebd304a3`. La operó la sesión entre 2026-11-23 13:15 UTC y 2026-11-24 05:45 UTC.

**Qué hay en esta carpeta:**

- **`run-log.md`:** el registro, paso a paso.
  - Va redactado con los marcadores de la guía: la carpeta del smoke pasa a `<carpeta-del-Smoke-PB>`, la de respaldo a `<carpeta-de-respaldo>`, y la raíz de las dos a `<raiz-de-trabajo>`.
  - Las órdenes con rutas se escriben con sus marcadores (`<worktree>`, `<carpeta-del-Smoke-PB>`), y nunca con su eco (M-09).
- **`owner-summary.md`:** el resumen para el Owner, con los veredictos propuestos y las preguntas abiertas.
- **`integrity.md`:** SPB-51, la salida del bloque 10.
- **`environment-pre.md` y `environment-post.md`:** P-05 y SPB-52.
- **`inventories/`:**
  - un resumen por inventario de `RCDUMP` (las líneas REF, DEF y NEST, y el número de BLK, sin los JSON de los sobres), con el SHA-256 del inventario crudo;
  - `INDICE.txt`, con el SHA-256 y el tamaño de los 136 archivos.
  - Los inventarios crudos (560 MB) se quedan en local.
- **`inv-texts/`:** las 11 lecturas de evidencia que no son de `RCDUMP` (UI Automation, cronómetros, textos del panel y recuentos), enteras.
  - Seis se pasaron a UTF-8 para custodiarlas: cinco lecturas de UI Automation desde la página OEM (cp850) y `c27-SPB-44-panel-textos.txt` desde Windows-1252. El SHA-256 de cada original está en `inventories/INDICE.txt`.
- **`screens/`:**
  - las capturas recortadas y tapadas (§8 de la guía), con `derivation-record.txt` (por cada recorte: la cruda, el rectángulo, lo tapado y su motivo, el paso y los SHA-256);
  - `no-custodiadas.txt`, con lo que no se custodia y por qué.
  - Las crudas nunca se custodian.

- **`review/`:** el encargo de la revisión independiente (`brief.md`) y su informe, con la recomprobación y la confirmación final: **CUSTODIABLE, B 0 / I 0 / M 0**. Las rutas locales van con marcadores.

**La revisión independiente** (Q-PBT5-8) se hizo antes del commit. Sus correcciones están aplicadas y las propuestas revisadas están al final de `run-log.md`.

**Antes de custodiar:**
- se pasaron por todo los patrones locales de privacidad (`scan_custody.py`), sin ninguna coincidencia;
- la revisión independiente de la evidencia (Q-PBT5-8) se hizo antes del commit, con una recomprobación de sus correcciones.
