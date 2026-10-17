## Recuperación del transporte (decisiones §87)

El binario fijado (claude-cli 2.1.293, `8693c4a0…`) ya no existe (preflight de §86: STOP; evidencia §172). Decisiones §87 caracterizó 2.1.299
(registro R20261016T185242Z-char, ELIGIBLE, commit `1c960c0a`; evidencia §174) y ordenó recuperar el kit con cambios mínimos; el Coordinator aprobó
la opción A y §88 la validación incremental. **Lo vigente es esta sección y `kit/kit-manifest.json`**; la cabecera de arriba (Autorización,
Transporte, Compuerta D-01, Estado) y la tabla de archivos describen la congelación original.

- **Sin cambio:** el clon `D:\r62-arch-a6r4` (088b3227) y el run; `prompt.md` (re-renderizado: byte a byte `a8dc5773…`), `order.txt`, la
  plantilla, el esquema, `kit-params.json` y el alcance del cierre (CanonicalInputs idénticos: decisiones `4e090ed4` y evidencia `fafd4392`, con
  sus rangos). `transport_gate.py` sin cambio (su texto de rutas «vía a» sigue nombrando R20261008T183600Z-char).
- **Transporte:** claude-cli 2.1.299, `%APPDATA%\Claude\claude-code\2.1.299\f0584be0fbaf\claude.exe`, SHA-256 `295b49ec092f31a7de9c6ff286d9d8f120a4f029b9ad9a5ac0b12c35cbf4df2b`,
  Authenticode válida de «Anthropic, PBC»; flags, ArgsTemplate, modelo, effort, tope y orden congelada sin cambio.
- **Compuerta D-01:** MEASURED sobre `kit/claude-cli-characterization.json`, copia (`git cat-file blob`) del registro de `1c960c0a`, blob `19b8a27e`,
  SHA-256 `4ba6ba58…`, fuera del clon; `verify_clone.py` la comprueba (en kit/, SHA-256 fijado, blob = el del commit, leído del worktree).
- **Autorización:** §87 fijada con `pin_authorization.py` sobre las decisiones de la HEAD `1c960c0a` (Ready, línea 1717): solo cambian
  `AUTH_SECTION` (86 → 87), `AUTH_HEADING` y `AUTH_TEXT_SHA` (`f85aa811…` → `0eb44fac…`).
- **Archivos cambiados:** `launch.py`, `make_closure.py` (y `closure.json`: solo Transport y la versión de DeclaredRuntimeContext), `verify_clone.py`,
  `post-review.py` (una línea de docstring), `selftest-post-review.py` (versiones de los fixtures), `owner_gate.py`, `transport-gate.json`,
  `clone-verification.json`, `selftest-result.json` y el registro nuevo. Adaptador `prep/s87/adapt_transport_s87.py` (sustituciones exactas desde
  el kit fijado a §86, manifiesto `cf71191e`, copia en `prep/s87/pinned-kit/`); diffs en `prep/diffs/s87/`.
- **Validación (§88):** matriz en `prep/s87/validation-matrix.json` y `.md`. verify_clone positiva AllChecks (guardas 815/815, iguales a las
  custodiadas) y negativa (copia alterada → falla); autoprueba completa 297/297 (terminada antes de §88); mutación reutilizada (191/191 de
  `mutation-result.json`: el auditor solo cambia en una línea de docstring, `transport_gate.py` es idéntico y `owner_gate.py` solo en las tres
  constantes); la mutación repetida se detuvo por §88 en 152/191, todos muertos.
- **Congelación:** `kit/kit-manifest.json` SHA-256 `70d68119e8e1f0a54cad030ba46fd120bf5e609d9b798a08aa3fcf63d42be79f` (26 archivos; autorización FIJADA §87).
- **Sonda y prueba en seco** (orden congelada, desde esta carpeta, 2026-10-17T00:10Z): `--record-probe` código 0; `--dry-run` código 2 con
  `BlockingChecks` = ["11-OwnerAndAuthorizationGate"] por el único problema «sección posterior a §87 en decisiones (§88 en la línea 1733): una
  disposición posterior tiene que revisarse antes de lanzar» (esperado: §88 se registró después; la compuerta 11 no se alteró ni se fijó §88);
  comprobaciones 1-3 y 6-10 en Ok. NO lanzado.
