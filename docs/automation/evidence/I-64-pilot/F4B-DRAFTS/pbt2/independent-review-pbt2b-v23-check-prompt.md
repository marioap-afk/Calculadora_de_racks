> **Redaccion para el repo publico:** las rutas locales y las variables de entorno de carpetas del usuario se sustituyen por marcadores (`<scratchpad>`, `<worktree-de-I-64>`, `<worktrees-de-Codex>`, `<checkout-principal>`, `<datos-locales-del-usuario>`, `<datos-de-aplicacion-del-usuario>`). El resto no cambia.

# Encargo de la comprobación acotada de PB-T2b (CA-T2b) y de sus recomprobaciones (enviados por la sesión al revisor de PB-T2 como mensajes, 2026-11-19; texto exacto)

## 1. CA-T2b, sobre la v2.3 (2026-11-19T06:47:13.816Z)

Mensaje de la sesion: haz la COMPROBACION ACOTADA DE PB-T2b, antes de emitirla. Usa el mismo patron que la de PB-T2a. La autorizacion de emision es F4B-E28, pregunta 4: emitir si hay 0 bloqueantes y 0 importantes sin resolver y la remedicion no cambia nada material.

**Reglas:** las de siempre. HEAD 17872be8: PB-T2a esta en su correccion, en el intento 2, todavia sin GREEN. Su src de Application es el esqueleto de c33ac6ac. Lee por objetos git. Puede haber una delegacion abierta en el worktree, asi que el worktree esta prohibido.

**Entradas** (en hs65\pbt2\):
- el paquete v2.3: I-64-f4b-pbt2-gate-contract-package-v23.md, con v23-delta.md (V23-01 a V23-07);
- el generador: make_pbt2b_contract.py, con generator-notes-pbt2b.md (las DUDAS, en su §5);
- el seco: dry-pbt2b.txt y contract-dry-pbt2b.json;
- la remedicion de las anclas [T1c]: pbt2b-anchors-out.txt y measure_pbt2b_t1c.py.

**Que verificas:**
1. Que el contrato en seco es fiel al contrato propuesto de PB-T2b en la v2.3, y que aplica F4B-E23, F4B-E28 y las lecciones de PB-T1b, PB-T1c y PB-T2a:
   - C13 por nombre;
   - las notas fuera;
   - el Trailer real;
   - build-plugin omitido si Core falla en el RED;
   - la regla del configurador;
   - las comparaciones tras el recalculo neutro.

   Ojo a la leccion de PB-T1c y PB-T2a: el intento 1 de ambas se gasto por una prueba nueva mal escrita (una comparacion antes del recalculo; una lectura despues de aplicar). ¿Las CT de PB-T2b tienen riesgos parecidos de orden o de momento de la lectura que el paquete deberia fijar? Si los ves, son hallazgos.
2. Que las anclas remedidas de W y WC son exactas en 17872be8 (la ventana es la del GREEN 161a0dad) y que los rangos de INV-PBT2B-03 son exactos.
3. Que las DUDAS D-1 a D-8 quedan resueltas, con lo que hay que hacer en cada una.
4. Que comprobaciones de remedicion tiene que hacer el generador al escribir, tras el cierre de PB-T2a.
5. Si hay decisiones materiales nuevas para el Owner. Se estricto.

**Donde escribes:** hs65\pbt2\independent-review-pbt2b-v23-check.md, con el veredicto (EMITIBLE, EMITIBLE TRAS CORRECCIONES o NO EMITIBLE) y su B/I/M arriba.

**Respuesta final:** una linea.

## 2. La recomprobación §10, sobre la v2.4 (2026-11-19T08:18:20.642Z)

Recomprobación acotada de la v2.4 (tu revisión CA-T2b dio EMITIBLE TRAS CORRECCIONES, B 0 / I 3 / M 7). Añade al final de tu `independent-review-pbt2b-v23-check.md` una sección «§10. Recomprobación sobre la v2.4». No reescribas las secciones anteriores.

**Entradas** (todo en `...\scratchpad\hs65\pbt2\`):
- `I-64-f4b-pbt2-gate-contract-package-v24.md`, con el registro V24-01 a V24-16 delante;
- `v24-delta.md`;
- `make_pbt2b_contract.py`, `dry-pbt2b.txt`, `contract-dry-pbt2b.json`, `neg-pbt2b.txt` y `generator-notes-pbt2b.md`;
- `measure_pbt2b_t1c.py` y `pbt2b-anchors-out.txt`.

**Qué comprobar:**
1. **Las correcciones:** por cada importante (I-01, I-02 e I-03) y cada menor (M-01 a M-07) tuyos, si está corregido, con la cita de la línea de la v2.4 o del generador.
2. **El defecto nuevo que encontró el redactor:** la tolerancia de los 17 fixtures de `Fixtures/I64` es 4, así que `ToleranceBox=4` no cambia nada.
   - Comprueba el hecho con la lectura del código y de los fixtures.
   - Comprueba que **4.5**, y **5** en la segunda edición de CT-04, dan un estado válido y distinto en cada fixture que usan las CT (sobre todo `pb-20-ab` y `pb-01-base`). Mira la validación y el redondeo de `ToleranceBox` y de la tolerancia del modelo: que 4.5 no se redondee a 4, ni se rechace, ni bloquee «Aplicar». El redactor dice que eso se razonó y no se midió: mídelo por lectura del código si se puede.
3. **El barrido de todas las CT de PB-T2b y PB-T2c** con el riesgo de F4B-E30 y F4B-E33: el momento de cada lectura frente al paso que cambia el estado, los gestos inertes con los datos de prueba y las líneas esperadas que dependen de datos. ¿Queda alguna CT expuesta?
4. **Las cuatro dudas del redactor:**
   - el encargo de CA-T2b se custodia con su texto literal, y lo pone la sesión;
   - el literal de `BOUNDED_RESULT`;
   - 4.5 y 5 en `pb-20-ab`;
   - CT-01 (iii) en la misma ventana que (ii).
5. **Decisiones nuevas para el Owner:** si las hay.

**Reglas:**
- El código, solo desde objetos git del checkout principal: `git -C "<checkout-principal>" show <rev>:<ruta>`, con `<rev>` = `5ddeb3b4` (el GREEN de PB-T2a, que será la base del código de PB-T2b) o `17872be8`.
- **Nunca** leas ni ejecutes nada en `<worktrees-de-Codex>\...`.
- Python con `-I`, scripts escritos con Write; nada de heredoc ni `python -c`.
- No escribas fuera de `hs65\pbt2\` (tu §10 y, si hace falta, un script de medición propio con el prefijo `recheck_`).

**Veredicto de §10:** EMITIBLE o EMITIBLE TRAS CORRECCIONES, con B / I / M nuevos. Si es EMITIBLE, propón el literal ASCII de `BOUNDED_RESULT` que debe empezar por `EMITIBLE`.

**Respuesta:**
- el veredicto;
- los hallazgos nuevos, uno por línea;
- la respuesta a cada duda.

## 3. La recomprobación §11, sobre la v2.7 (2026-11-19T14:14:00.765Z)

Recomprobación acotada de la v2.7 para PB-T2b.

PB-T2a cerró (custodia `b29551ae`, F4B-E34). El Owner aprobó una regla nueva para el resto de F4B: la reparación del RED (RR), en F4B-E35 (`F4B-DRAFTS/coordinator/owner-f4b-pbt3-emission-red-repair-decisions.md`, custodiado en `b29551ae`; léelo entero, RR-1 a RR-7). La v2.7 la incorpora en PB-T2b y PB-T2c, y PB-T2b se emite ya con la v2.7.

**Añade al final de `independent-review-pbt2b-v23-check.md` una sección «§11. Recomprobación sobre la v2.7».** Usa la numeración, porque el `bounded_check` del generador de PB-T2b solo reconoce secciones con número. No reescribas lo anterior.

**Entradas** (en `hs65\pbt2\`):
- `I-64-f4b-pbt2-gate-contract-package-v27.md` y `v27-delta.md` (V27-01 a V27-07);
- `make_pbt2b_contract.py`, con `dry-pbt2b.txt`, `contract-dry-pbt2b.json` (el seco real sobre `b29551ae`, sha 5301C315…) y `neg-pbt2b.txt`;
- `generator-notes-pbt2b.md`, con las dudas D-1 a D-5 de la v2.7.

**Qué comprobar:**
1. **Fidelidad a RR:** que la v2.7 y el contrato en seco reflejan RR-1 a RR-7 sin ampliarla ni recortarla. En particular:
   - la excepción de C07;
   - la parada nueva C18;
   - la cadena en INV-PBT2B-02;
   - la CI del RED reparado;
   - lo que declara la entrega;
   - la autoridad F4B-E35.
2. **D-1:** el RED de PB-T2b ya toca `src/` con la costura inerte, así que se lee:
   - RR-1 como «antes del commit GREEN»;
   - RR-6 como «el RED de cada contrato».

   ¿Es fiel a la intención de F4B-E35? ¿Abre un hueco?
3. **Que RR no cree una vía para cambiar lo esperado sin parar:**
   - la detección de RR-3 (líneas esperadas);
   - lo que el Controller puede comprobar mecánicamente y lo que queda para la revisión.
4. **El seco real sobre `b29551ae`:**
   - que `PT2A_CLOSE` y `CI_BASE_T2A` estén bien;
   - que los 12 AVISOS que quedan sean solo de la emisión.
5. **Decisiones nuevas para el Owner,** si las hay.

**El `BOUNDED_RESULT`:** propón el literal ASCII que nombre esta recomprobación y pase el `bounded_check`. El de hoy es: `EMITIBLE TRAS CORRECCIONES (B 0 / I 3 / M 7; I-01, I-02 e I-03 corregidas en el paquete; recomprobacion v2.4: EMITIBLE, B 0 / I 0 / M 4; M-08 a M-11 aplicadas en la v2.5)`.

**Reglas:** las de siempre.
- Objetos git del checkout principal, en `b29551ae`.
- Nada en `<worktrees-de-Codex>\...`.
- Python con `-I`, escrito con Write, sin `-c`.

**Veredicto:** EMITIBLE o EMITIBLE TRAS CORRECCIONES, con B / I / M nuevos.

**Responde con:**
- el veredicto;
- los hallazgos, uno por línea;
- el literal.

## 4. La recomprobación §12, sobre la v2.8 (2026-11-19T15:11:32.995Z)

Recomprobación acotada de la v2.8 para PB-T2b.

Añade al final de `independent-review-pbt2b-v23-check.md` una sección «§12. Recomprobación sobre la v2.8», con número y sin reescribir nada de lo anterior.

**Entradas** (en `hs65\pbt2\`):
- `I-64-f4b-pbt2-gate-contract-package-v28.md` y `v28-delta.md` (V28-01 a V28-06);
- `make_pbt2b_contract.py`, con `dry-pbt2b.txt`, `contract-dry-pbt2b.json` (el seco real sobre `b29551ae`, sha 396A047B…) y `neg-pbt2b.txt`;
- `generator-notes-pbt2b.md`, con sus dudas D-21 a D-23.

**Qué comprobar:**
1. **I-01 de tu §11:** que la v2.8 y el contrato en seco dicen cómo lee el Controller una reparación del RED en las comprobaciones n.º 10 (`Tests`) y n.º 11 (`Trailer`), y que comprueba RR-1, RR-2, RR-4, RR-5 y RR-6, con el tramo `RedSha..RedRepairSha`. Que una reparación válida ya no pueda acabar en REWORK por la letra de 16.9, ni una inválida pasar.
2. **M-01 a M-04 de tu §11,** con la precisión de la sesión sobre M-01:
   - es parada (C07) que una línea literal fijada por el paquete para la clase reparada no siga byte a byte;
   - los indicios heurísticos (`<campo>=` y el primer argumento de `Assert.Equal`) van a la revisión, no son parada.

   Comprueba también si D-21 acota bien esas líneas literales.
3. **D-22:** que lo que se dice al Controller vaya en la `ExpectedEvidence` y en la autoridad F4B-E35. ¿Basta para que lo aplique?
4. **Lo nuevo:** que no rompa nada de lo cerrado en §1 a §11.
5. **Decisiones nuevas para el Owner,** si las hay.

**`BOUNDED_RESULT`:** si da EMITIBLE con B 0 e I 0, confirma el literal final en ASCII. Tu propuesta era: `EMITIBLE TRAS CORRECCIONES (B 0 / I 3 / M 7; I-01, I-02 e I-03 corregidas en el paquete; recomprobacion v2.4: EMITIBLE, B 0 / I 0 / M 4; M-08 a M-11 aplicadas en la v2.5; recomprobacion v2.8: EMITIBLE, B 0 / I 0 / M <m>)`. Comprueba que pasa el `bounded_check` robusto (V28-05) con tu informe completo.

**Reglas:** las de siempre.

**Veredicto:** EMITIBLE o EMITIBLE TRAS CORRECCIONES.

**Responde con:**
- el veredicto;
- los hallazgos;
- el literal.
