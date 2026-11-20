> **Redaccion para el repo publico:** las rutas locales y las variables de entorno de carpetas del usuario se sustituyen por marcadores (`<scratchpad>`, `<worktree-de-I-64>`, `<worktrees-de-Codex>`, `<checkout-principal>`, `<datos-locales-del-usuario>`, `<datos-de-aplicacion-del-usuario>`). El resto no cambia.

# Encargo de la comprobación acotada de PB-T3a (CA-T3a) (enviado por la sesión al revisor de PB-T3 como mensaje, 2026-11-19; texto exacto)

## 1. CA-T3a, sobre la v4.3 (2026-11-19T21:17:21.195Z)

Mensaje de la sesión. Haz la **COMPROBACIÓN ACOTADA DE PB-T3a (CA-T3a)** antes de emitirla.

**La autorización:** F4B-E35, pregunta 3 (el Owner, «Sí, con esa condición»): 0 bloqueantes y 0 importantes sin resolver, y que la remedición sobre el cierre de PB-T2c no cambie nada material.

**El estado:**
- PB-T2b está CERRADA (`56cb026f`, con la Snapshot K, F4B-E37).
- PB-T2c está EMITIDA (BASE `05beabfd`) y su Worker trabaja AHORA en el worktree. Por P-08, el código solo por objetos git desde el checkout principal, en `05beabfd`.
- La base de PB-T3a será el cierre de PB-T2c. Lo que dependa de él solo se puede juzgar como diseño; el generador lo vuelve a medir al escribir.

**Las entradas** (en `hs65\pbt3\`):
- **el paquete v4.3:** `I-64-f4b-pbt3-gate-contract-package-v43.md` y `v43-delta.md`, con V43-01 a V43-13 sobre tu v4.2 (§13). Trae:
  - la reparación del RED (RR, F4B-E35) portada como en PB-T2 v2.7 y v2.8, con C07, C18, la cadena RR-6, la lectura de la verificación y la lista cerrada de RR-3;
  - la Snapshot K;
  - la C11 con el sustituto de nc2 adaptado a F4B (F4B-E34);
  - las lecciones de F4B-E30 y F4B-E33, que cambian ocho recetas de CT;
- **el generador:** `make_pbt3a_contract.py`, con `generator-notes-pbt3a.md` y sus DUDAS;
- **el seco:** `dry-pbt3a.txt` y `contract-dry-pbt3a.json` (sobre `05beabfd`, sha D0B3FEF1…), con `neg-pbt3a.txt`;
- **la referencia de lo que ya se aceptó en PB-T2:** el paquete de PB-T2 v2.9 en `05beabfd` (`docs/initiatives/I-64-f4b-pbt2-gate-contract-package.md`) y su informe CA-T2c (`docs/automation/evidence/I-64-pilot/F4B-DRAFTS/pbt2/independent-review-pbt2c-v25-check.md`), con sus §9 a §13.

**Qué comprobar:**
1. **La fidelidad del contrato en seco frente a la v4.3:** el alcance, las invariantes, las paradas (C07, C18 y C11 literal), los RequiredTests, la evidencia, las autoridades y los marcadores.
2. **RR en PB-T3a:**
   - ¿cubre lo mismo que en PB-T2c, sin huecos?: la verificación, las comprobaciones n.º 10 y n.º 11, RR-6 y la lista cerrada;
   - la lectura de D-1 (el RED toca `src/`);
   - qué no puede tocar la reparación;
   - la lista cerrada de RR-3, que aquí incluye también los valores de edición: ¿es correcta?
3. **Las ocho recetas de CT cambiadas y el resto de las CT de PB-T3a** frente al momento de cada lectura y a los gestos que no cambian nada, con los valores de los 17 fixtures.
4. **La Snapshot K y la C11 de F4B-E34:** fieles a lo custodiado en `56cb026f`.
5. **Lo que el generador debe volver a medir al escribir,** tras el cierre de PB-T2c.
6. **Decisiones nuevas para el Owner,** si las hay.

**Formato:** escribe el informe en `hs65\pbt3\independent-review-pbt3a-v43-check.md`, con secciones numeradas (§1, §2, …) y con el veredicto en una línea propia: «**Veredicto: EMITIBLE …**» o «**Veredicto: EMITIBLE TRAS CORRECCIONES …**», con B / I / M. Propón el literal ASCII de `BOUNDED_RESULT`, que debe empezar por EMITIBLE.

**Reglas:**
- nada en `<worktrees-de-Codex>\...`;
- Python con `-I` y solo en scripts escritos con Write, sin `-c` ni heredoc;
- escribe solo en `hs65\pbt3\`, en ese informe y en guiones con el prefijo `cat3a_`.

**Responde con:**
- el veredicto;
- los hallazgos, uno por línea;
- el literal.

## 2. Recomprobación 1 (2026-11-19T22:12:02.679Z)

Recomprobación de CA-T3a sobre la v4.4.

Tu CA-T3a dio EMITIBLE TRAS CORRECCIONES, B 0 / I 1 / M 4.

**Añade** al final de `hs65\pbt3\independent-review-pbt3a-v43-check.md` una sección «§10. Recomprobación sobre la v4.4». Lleva número, con el veredicto en línea propia: «**Veredicto: EMITIBLE …**». No reescribas lo anterior.

**Entradas** (en `hs65\pbt3\`):
- **El paquete:** `I-64-f4b-pbt3-gate-contract-package-v44.md` (SHA a96063ac…) y `v44-delta.md`, con V44-01 a V44-06.
  - **I-01:** `PalletDepth` de 48 a 48,5, en lugar de `BeamDepth`; se añade la lección M-13 al §5.0, con la tabla de gestos.
  - **M-01:** la 9.ª viñeta de la evidencia.
  - **M-02:** los valores de RR-3 cuentan como tokens.
  - **M-03:** la tabla de RR-3, enumerable.
- **El generador:** `make_pbt3a_contract.py`.
- **El seco y las pruebas en negativo** (M-04): `dry-pbt3a.txt` y `contract-dry-pbt3a.json` (sobre `05beabfd`, sha DFFF5C02…) y `neg-pbt3a.txt` (115/115).
- **Las notas:** `generator-notes-pbt3a.md`, con DU-23 a DU-25.

**Qué comprobar:**
1. **Por cada hallazgo tuyo** (I-01, M-01 a M-04): si está corregido, con la línea.
2. **I-01:** que `PalletDepth` 48,5 lo conserva de verdad el ensamblador hasta el diseño persistido, y que su ancla saldrá `presente=True`. Mira los sitios que cita V44-01, `Erasures` vacío en CT-PBT3B-01 (i) y la revisión de los demás gestos con la lente de M-13.
3. **Los `PostPeralte` 3,25 y 4,25 de V44-03:** son valores nuevos. ¿El ensamblador los admite, o vuelve a ser un gesto inerte?
4. **DU-23:** la custodia redactada de los `cat3a_*.py`. **DU-25:** la opción `--bounded-file`.
5. **Decisiones nuevas para el Owner,** si las hay.

**El `BOUNDED_RESULT`:** confirma el literal final en ASCII, alargado con «; recomprobacion v4.4: EMITIBLE, B 0 / I 0 / M n», y que pasa el `bounded_check` con tu informe completo.

**Reglas:** las de siempre.
- El código, por objetos git en `05beabfd`. PB-T2c tiene ahora su verificación en curso en el worktree.
- Nada en el worktree.
- Python con `-I`, escrito con Write.

**Responde con:**
- el veredicto;
- los hallazgos;
- el literal.

## 3. Recomprobación 2 (2026-11-20T02:06:29.596Z)

Recomprobación de CA-T3a sobre la v4.5, antes de reemitir PB-T3a.

**Lo que pasó:**
- El Worker de PB-T3a, intento 1, paró antes del RED con PBT3A-C02, sin commits. El esqueleto de la v4.4 hacía que `PushBackFullComposition.cs` nombrara el piloto y declarara `ForWrite`, y la guarda existente G9 (`WorkspacePilotUpdateIntegrationGuardTests`) prohíbe `\bForWrite\b` en los archivos de Application que nombran el piloto.
- El Owner decidió «Renombrar ForWrite» (F4B-E42; `hs65\pbt3acc\owner-pbt3a-g9-decision.md`).
- La corrección va en el intento 2, con reemisión. El análisis de la sesión está en `hs65\pbt3acc\analysis.md`.

**Añade** al final de `hs65\pbt3\independent-review-pbt3a-v43-check.md` una sección «§11. Recomprobación sobre la v4.5», con el veredicto en línea propia («**Veredicto: EMITIBLE …**»).

**Entradas:**
- `hs65\pbt3\I-64-f4b-pbt3-gate-contract-package-v45.md` (SHA 69411084…) y `v45-delta.md` (V45-01 a V45-05):
  - `ForWrite` → `ForFullWrite`;
  - el §4.1 nuevo, con el barrido de todas las guardas;
  - la lección del §5.0;
  - la precondición 11 de la reemisión;
- el análisis de la sesión.

**Qué comprobar:**
1. **El renombrado,** completo y coherente en PB-T3a y PB-T3b: no queda ningún `ForWrite` de producto y `ForFullWrite` no casa con ningún término de ninguna guarda.
2. **El barrido del §4.1:** hazlo tú de forma independiente, sobre los archivos nuevos o tocados de PB-T3a, b y c frente a TODAS las guardas de `tests/` que leen `src/`. Esta vez nos falló una guarda, y quiero una segunda mirada. ¿Hay otra colisión?
3. **La reemisión en el intento 2,** en el §6.0 y la precondición 11: ¿es coherente con el análisis y con el régimen de corrección? Revisa en especial `ChainRedSha` null, `CorrectionOf` y la reutilización de los parches.
4. **Decisiones nuevas para el Owner,** si las hay.

**El `BOUNDED_RESULT`:** propón el literal final, alargado con «; recomprobacion v4.5: EMITIBLE, B 0 / I 0 / M n».

**Reglas:**
- objetos git en `18e4b783`; no hay delegaciones activas;
- nada en el worktree;
- Python con `-I`, escrito con Write.

**Responde con:**
- el veredicto;
- los hallazgos;
- el literal.
