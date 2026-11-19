> **Redaccion para el repo publico:** las rutas locales y las variables de entorno de carpetas del usuario se sustituyen por marcadores (`<scratchpad>`, `<worktree-de-I-64>`, `<worktrees-de-Codex>`, `<checkout-principal>`, `<datos-locales-del-usuario>`, `<datos-de-aplicacion-del-usuario>`). El resto no cambia.

# Encargo de la comprobación acotada de PB-T2c (CA-T2c) (enviado por la sesión al revisor de PB-T2 como mensaje, 2026-11-19; texto exacto)

## 1. CA-T2c, sobre la v2.5 (2026-11-19T09:04:08.178Z)

Mensaje de la sesión. Haz la **COMPROBACIÓN ACOTADA DE PB-T2c (CA-T2c)** antes de emitirla, con el mismo patrón que CA-T2b. La autorización de emisión es F4B-E28, pregunta 4: 0 bloqueantes, 0 importantes sin resolver y una remedición sobre el cierre de PB-T2b que no cambie nada material. PB-T2b aún no está emitida y PB-T2a no ha cerrado, así que las anclas que dependen del GREEN de PB-T2b solo se pueden juzgar como diseño. El generador las vuelve a medir al escribir.

**Entradas** (en `hs65\pbt2\`):
- `I-64-f4b-pbt2-gate-contract-package-v25.md`, con `v25-delta.md`: el paquete vigente. Tu §10 ya está aplicada como V25-01 a V25-04;
- `make_pbt2c_contract.py`, con `generator-notes-pbt2c.md`, que trae las DUDAS numeradas;
- el seco: `dry-pbt2c.txt` y `contract-dry-pbt2c.json`;
- `neg-pbt2c.txt` y `run_pbt2c_checks.py`.

**Qué comprobar:**
1. **La fidelidad del contrato en seco frente a la v2.5:**
   - el alcance (3 allowed y 173 forbidden, con el archivo del modo de PB-T2b ya en src);
   - las invariantes, las paradas (con C11 literal para PBT2C), los RequiredTests (39) y las evidencias;
   - los marcadores que abortan.
2. **Los riesgos de momento de lectura, CT por CT, en las CT de PB-T2c:** las lecciones de F4B-E30 y F4B-E33 y tu barrido de la §10, aplicados al contrato y a la tabla de CT-PBT2C-03 (49 gestos, con el «porque:» de las filas que «Aplicar» niega).
3. **Las DUDAS del generador,** sobre todo:
   - D-1: los 31 filtros del §6.2;
   - D-3: el formato de la tabla;
   - D-4: la comprobación por contenido;
   - D-5: la parada C02 ampliada;
   - D-9: el cierre de PB-T2a dentro del de PB-T2b.
4. **Lo que el generador debe volver a medir al escribir,** tras el cierre de PB-T2b.
5. **Decisiones nuevas para el Owner,** si las hay.

**Reglas:** las de siempre.
- El código, solo desde objetos git del checkout principal, `5ddeb3b4` o `17872be8`.
- Nunca leas ni ejecutes nada en `<worktrees-de-Codex>\...`.
- Python con `-I`, escrito con Write.
- Escribe solo `independent-review-pbt2c-v25-check.md` y, si hacen falta, guiones con prefijo `cat2c_`.

**Veredicto:** EMITIBLE o EMITIBLE TRAS CORRECCIONES, con B / I / M, y el literal ASCII propuesto de `BOUNDED_RESULT`, que empiece por EMITIBLE.

**Responde con:**
- el veredicto;
- los hallazgos, uno por línea;
- tu respuesta a cada duda.

## 2. Recomprobación 1 (2026-11-19T09:58:42.302Z)

Recomprobación de CA-T2c sobre la v2.6. Tu CA-T2c dio EMITIBLE TRAS CORRECCIONES, B 0 / I 1 / M 5. Añade al final de `independent-review-pbt2c-v25-check.md` una sección «§R. Recomprobación sobre la v2.6», sin reescribir lo anterior.

**Entradas** (en `hs65\pbt2\`):
- `I-64-f4b-pbt2-gate-contract-package-v26.md`, con el registro V26-01 a V26-07, y `v26-delta.md`;
- `make_pbt2c_contract.py`, `dry-pbt2c.txt`, `contract-dry-pbt2c.json` (sha E0C0737D…) y `neg-pbt2c.txt`;
- `generator-notes-pbt2c.md`, con D-11 a D-15.

**Decisiones de la sesión que ya están aplicadas:**
- **M-05:** PB-T2b se emite con la v2.5, sin cambios; la v2.6 solo vale para PB-T2c y se custodia con su emisión.
- **M-02:** MU-T2C-03 se define por contenido, sin tocar R4 de PB-T2b.
- **D-11:** INV-PBT2C-04 se extiende a las corridas aisladas (V26-07).

**Qué comprobar:**
1. Por cada hallazgo tuyo (I-01 y M-01 a M-05), si está corregido, con la línea.
2. **I-01:** que las fuentes nuevas (`c = Pasada(Copy(draft.CurrentDraftState), A)` justo tras «Aplicar») den de verdad las líneas esperadas de CT-PBT2C-01 y 02 sin depender de la petición clásica. Revisa también D-13: el `Cotas` de CT-PBTC-05 recibe la ventana y el Worker escribirá una variante sobre `c`. ¿Deja latitud o se puede leer mal?
3. **M-02/D-14:** que MU-T2C-03 por contenido, y único en todo W, sea seguro con lo que R4 de PB-T2b obliga a dejar.
4. **Lo nuevo:** que la v2.6 no introduzca riesgos de momento de lectura ni cambios de cifras.
5. **El `BOUNDED_RESULT`:** si sigue valiendo `EMITIBLE TRAS CORRECCIONES (B 0 / I 1 / M 5; I-01 y M-01 a M-05 corregidas en el paquete)` o hay que añadir la recomprobación, como en PB-T2b. Si hay que añadirla, propón el literal ASCII. Recuerda que `bounded_check` (V25-01) lee también la última recomprobación.
6. **Decisiones nuevas para el Owner,** si las hay.

**Las reglas de siempre:**
- objetos git del checkout principal (`5ddeb3b4` o `17872be8`);
- nada en `<worktrees-de-Codex>\...`;
- Python con `-I`, escrito con Write, sin `-c`.

**Veredicto:** EMITIBLE o EMITIBLE TRAS CORRECCIONES, con B / I / M nuevos.

**Responde con:**
- el veredicto;
- los hallazgos, uno por línea;
- el literal de `BOUNDED_RESULT`.

## 3. Recomprobación 2 (2026-11-19T14:14:06.513Z)

Recomprobación de CA-T2c sobre la v2.7. Hazla después de la §11 de PB-T2b.

**Dónde:** añade al final de `independent-review-pbt2c-v25-check.md` una sección «§10. Recomprobación sobre la v2.7». Lleva número; no reescribas lo anterior.

**Entradas:**
- la v2.7 y `v27-delta.md`;
- `make_pbt2c_contract.py`, con `dry-pbt2c.txt` y `contract-dry-pbt2c.json`, el seco sobre `17872be8` (sha 45D5C9B9…);
- `neg-pbt2c.txt`;
- `generator-notes-pbt2c.md`, con sus dudas de la v2.7.

**Qué comprobar:** lo mismo que en la §11 de PB-T2b, aplicado a PB-T2c, y además:
- **D-2:** en PB-T2c, el RED repetido sobre `RedRepairSha` incluye las corridas aisladas de los mutantes con la clase reparada. ¿Es correcto y suficiente?
- **RR frente a los mutantes:** RR-2 no deja tocar la costura ni los mutantes. ¿Lo hace cumplir el contrato?
- **El rango de autoridades:** que pasar de «F4B-E01 a F4B-E35» a lo que corresponda tras la emisión de PB-T2b (F4B-E36) no rompa nada.

**`BOUNDED_RESULT` de PB-T2c:** propón el literal ASCII que nombre esta recomprobación y pase el `bounded_check`.

**Reglas:** las de siempre.

**Veredicto:** EMITIBLE o EMITIBLE TRAS CORRECCIONES.

**Responde con:**
- el veredicto;
- los hallazgos;
- el literal.

## 4. Recomprobación 3 (2026-11-19T15:11:37.450Z)

Recomprobación de CA-T2c sobre la v2.8. Hazla después de la §12 de PB-T2b.

Añade al final de `independent-review-pbt2c-v25-check.md` una sección «§11. Recomprobación sobre la v2.8», con número, sin reescribir lo anterior.

**Entradas:**
- la v2.8 y `v28-delta.md`;
- `make_pbt2c_contract.py`, con `dry-pbt2c.txt` y `contract-dry-pbt2c.json`: el seco sobre `17872be8`, sha D8A544C1…;
- `neg-pbt2c.txt`;
- `generator-notes-pbt2c.md`.

**Qué comprobar:** lo mismo que en la §12, aplicado a PB-T2c, y además:
- tu I-01 de la §10, en especial que el tramo `RedSha..RedRepairSha` se compruebe solo con la clase nueva, sin W ni WC;
- tus M-01 a M-03: las 49 líneas y las fijas de CT-01 y CT-02, byte a byte, entre `RedSha` y `RedRepairSha`.

**`BOUNDED_RESULT` de PB-T2c:** confirma el literal final en ASCII y que pasa el `bounded_check` con el informe completo.

Las reglas son las de siempre.

**Responde con:**
- el veredicto;
- los hallazgos;
- el literal.

## 5. Recomprobación 4 (2026-11-19T16:02:12.715Z)

Recomprobación de CA-T2c sobre la v2.9.

PB-T2b ya está emitida: X `a12ea09a` y BASE `59839c71`, F4B-E36, con la v2.8 y tu `BOUNDED_RESULT` de la §12. Su Worker está trabajando AHORA en el worktree, así que por P-08 lees solo por objetos git desde el checkout principal, en `59839c71`.

**Añade** al final de `independent-review-pbt2c-v25-check.md` una sección «§12. Recomprobación sobre la v2.9», con número y sin reescribir lo anterior.

**Entradas** (en `hs65\pbt2\`):
- `I-64-f4b-pbt2-gate-contract-package-v29.md` y `v29-delta.md` (V29-01 a V29-03);
- `make_pbt2c_contract.py`, con `dry-pbt2c.txt` y `contract-dry-pbt2c.json` (el seco sobre `59839c71`, sha 09A064FD…);
- `neg-pbt2c.txt`;
- `generator-notes-pbt2c.md`.

**Qué comprobar:**
1. **Tu M-09 de la §11,** corregido en V29-01: la lista de líneas fijas de la ayuda de RR-3 de PB-T2c. ¿Está completa y cerrada? Revisa en especial si «las doce de CT-PBTC-07 y la de Aplicar» son literales del paquete.
2. **V29-02:** las ventanas del generador. Que no relajen ninguna comprobación.
3. **V29-03:** la custodia y la línea de emisión de PB-T2b conservada.
4. **Que la v2.9 no cambie nada de PB-T2b,** que ya está emitida con la v2.8.
5. **Decisiones nuevas para el Owner,** si las hay.

**`BOUNDED_RESULT` de PB-T2c:** confirma el literal final en ASCII y que pasa el `bounded_check` con el informe completo.

Las reglas son las de siempre.

**Responde con:**
- el veredicto;
- los hallazgos;
- el literal.

## 6. Recomprobación 5 (2026-11-19T19:44:49.896Z)

Recomprobación de CA-T2c, la final antes de emitir, sobre el cierre de PB-T2b.

PB-T2b está CERRADA (F4B-E38), con la custodia `56cb026f8a6fd87467a6400e2f3bca015c4abda3`. F4B-E37, la Snapshot K, va en el mismo commit.
- El GREEN es `03b16c88`, con la reparación del RED `6bc3497f`.
- En el cierre ya están custodiadas la K y las decisiones del Owner.
- Ahora no hay delegaciones activas. Lees por objetos git desde el checkout principal, en `56cb026f`.

**Añade** al final de `independent-review-pbt2c-v25-check.md` una sección «§13. Recomprobación final sobre el cierre de PB-T2b», con número y sin reescribir lo anterior.

**Entradas** (en `hs65\pbt2\`):
- **la v2.9 final** (`I-64-f4b-pbt2-gate-contract-package-v29.md`), con `v29-delta.md`:
  - V29-04 y V29-05 son tus M-11 y M-12;
  - V29-06 pasa el P-01 de PB-T2c a la Snapshot K, con las rutas de custodia del cierre;
- **el generador** `make_pbt2c_contract.py`, ya con `PT2B_CLOSE` = `56cb026f`, `GREEN_T2B` = `03b16c88` y `CI_BASE_T2B` = (`44586045707`, 19 094, 2 447, 21);
- **el seco real** `dry-pbt2c.txt` / `contract-dry-pbt2c.json` sobre `56cb026f`: sha 8F6F88C5…, 7 AVISOS, todos de la emisión;
- **`neg-pbt2c.txt`:** 124 de 124;
- **`pbt2c-anchors-out.txt`:** la remedición sobre el cierre y la tabla de CT-PBT2C-03, medida por la sesión. La corrida temporal de UI de la clase `PushBackPbt2cCt03ProbeTests.cs.txt` se hizo fuera de todo commit y con TRX; el árbol quedó limpio. Da 49 filas: aplica 42, nada 6, se niega 1;
- **`measure_pbt2c.py`**, con dos cambios: la sesión corrigió `TRX_NS` (el espacio de nombres de vstest), y la hora del TRX va en UTC.

**Qué comprobar:**
1. **V29-04 a V29-06:** que M-11 y M-12 están bien aplicadas, y que V29-06 es fiel a F4B-E37 y a lo custodiado en `56cb026f`.
2. **La remedición sobre el cierre:**
   - las anclas;
   - los mutantes MU-T2C-01 a 03: W:3713, WC:95 y W:3273 por contenido, sin la parada de V26-08;
   - la CI base;
   - los recuentos;
   - que nada material haya cambiado frente a lo que preveía la v2.9.
3. **La tabla de CT-PBT2C-03:**
   - **las seis «nada»:** `ToleranceBox=4`, `SideSelectorBox=1`, `FrontalSideBox=1`, `PostBox=otro`, `CellInOutBeamBox=otro` y `CellInOutPeralteBox=otro`. ¿Es correcto que no apliquen nada con los datos de prueba, o alguna esconde un gesto inerte que la tabla debería tratar de otro modo?
   - **la única que se niega,** `StructureOverrideBox=6`, por un bloqueante.
   - **la clase temporal:** que mida lo que dice el paquete, con los 49 gestos tomados por reflexión, la sonda del modo, `aplicado` y `descartes`.
4. **El `BOUNDED_RESULT` final de PB-T2c:** propón el literal ASCII que nombre esta §13 y pase el `bounded_check`, sobre el de la §12 más «; M-11 y M-12 aplicadas en la v2.9».
5. **Decisiones nuevas para el Owner,** si las hay.

**Reglas:** las de siempre.

**Veredicto:** EMITIBLE o EMITIBLE TRAS CORRECCIONES.

**Responde con:**
- el veredicto;
- los hallazgos;
- el literal.
