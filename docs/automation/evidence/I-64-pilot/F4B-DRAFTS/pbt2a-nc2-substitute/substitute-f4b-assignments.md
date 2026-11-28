# Encargos del sustituto adaptado PBT2A-NC2-EXACT-WHITELIST-F4B-01 (autor y revisor independiente; enviados por la sesion, 2026-11-19; texto exacto, en orden)

## 1. encargo inicial al autor (2026-11-19T08:17:58.121Z)

Prepara, SIN aplicarlo, un sustituto de control nc2 adaptado a F4B. Es una preparación para una decisión que el Owner tomará después; nada de lo que hagas se custodia ni cuenta como aplicado.

## Contexto
Un "sustituto de nc2" es un script Python determinista que demuestra mecánicamente, con 10 propiedades, que un control negativo nc2 (la delegación del Worker con UNA ruta quitada del AllowedWriteScope) tiene la firma exacta PD-F4-01: el Controller dio Scope en fail sobre la ruta quitada y Contract en fail solo por la cláusula de exactitud de la lista blanca, con EXECUTION_BLOCKED/STOP. Antes de evaluar, ejecuta dos veces una autoprueba sintética (casos A-N) y exige que sea determinista.

El sustituto de F4 (`data/pbt2a-substitute-as-run.py`, copia exacta del custodiado F4T1B con solo id y ámbito cambiados) se ejecutó contra PB-T2a y dio **FAIL** (`data/pbt2a-substitute-result-FAIL.json`), con 8 de 10 propiedades en PASS. Fallan dos, por diferencias de formato de F4B, no del Controller:
- **Propiedad 5 (los hashes derivados):** exige que `relay-record-planning.json` → `Outcome.OutputSha256` sea igual a SHA-256(delegation.json real). En F4B rige la regla F4B-E21:
  - el Controller deja vacías las 8 colecciones opacas;
  - la sesión las inyecta desde el contrato;
  - el relevo de planificación registra el SHA de la salida CRUDA del Controller (`output.json` de la planificación, 100A7695…), no el de `delegation.json` (BF3F0F62…);
  - `data/planning/session-inject-note.json` registra los dos SHA: OutputSha256 (crudo) y DelegationSha256;
  - la copia mutada de nc2 recalculó el hash de su relevo con la regla de F4: rr1.Outcome.OutputSha256 = SHA(delegation mutada), es decir 0420BB…
- **Propiedad 8:** exige que `ExpectedEvidence` del contrato contenga literalmente `EXACT_CLAUSE` ('AllowedWriteScope de la delegacion = exactamente el AllowedWriteScope del contrato.'). Los contratos de F4B no llevan esa frase. Su exactitud viene de la comparación byte a byte:
  - `ExpectedEvidence[0]` del contrato la exige: lee el texto real en `data/real/inputs/gate-contract.json`;
  - `byte-fidelity-check.json` la atestigua con `Result = PASS`, `AllowedWriteScopeElementwiseInOrder = true`, `Fields` que incluye AllowedWriteScope y las 7 opacas, `OutputSha256` = SHA(delegation real) y `ContractSha256` = SHA(contrato).
  - El resto de la propiedad 8 se conserva: A3/A4/A5 llanos en true sobre la mutada, `ma != ca`, y las 7 SEMANTIC_OPAQUE iguales al contrato.

## Lo que tienes que hacer
Todo dentro de `<scratchpad>\b491591bc47aa335\98cecaf8-117d-4d62-b418-bd3e08504b6f\scratchpad\hs65\pbt2acc2\nc2-substitute-f4b\`. No escribas fuera de esa carpeta.

1. **Escribe `nc2_exact_whitelist_substitute_f4b.py`** partiendo de `data/pbt2a-substitute-as-run.py`:
   - id `PBT2A-NC2-EXACT-WHITELIST-F4B-01`;
   - docstring: copia de F4T1B-NC2-EXACT-WHITELIST-01 adaptada a F4B-E21, solo en las propiedades 5 y 8;
   - ámbito: solo F4B-PBT2A-DRAFT-STATE-PORT, intento 2, verificación R20261119T074453Z-10a9;
   - registro: `SATISFIED_UNDER_PBT2A_EXACT_WHITELIST_SUBSTITUTE_F4B`.
2. **Cambia SOLO lo necesario:**
   - **Propiedad 5:** en vez de `rr0.Outcome.OutputSha256 == sha(real_deleg)`, exige el par de F4B-E21:
     - `rr0.Outcome.OutputSha256 == note.OutputSha256`;
     - `note.DelegationSha256 == sha(real_deleg) == bfc.OutputSha256`;
     - `note.Injected` = las 8 colecciones;
     - `note.NonEmptyInControllerOutput == []`;
     - y `rr1.Outcome.OutputSha256 == sha(mut_deleg)`, la regla de recálculo de nc2.
     El resto de la propiedad 5 queda igual: las entradas cambiadas son exactamente delegation.json y relay-record-planning.json, `diffpaths(rr0, rr1) == ['.Outcome.OutputSha256']`, bfc sin cambio y los InputSha256 de mutation.json coherentes. La nota de inyección entra como 6.º argumento opcional de main (ruta a session-inject-note.json); sin él, la propiedad 5 se evalúa como en F4.
   - **Propiedad 8:** en vez de `EXACT_CLAUSE in ExpectedEvidence`, acepta la exactitud por comparación byte a byte, con estas condiciones:
     - alguna entrada de ExpectedEvidence menciona la comparación byte a byte (define una prueba precisa y documéntala);
     - el bfc de la real tiene `Result == 'PASS'` y `AllowedWriteScopeElementwiseInOrder is True`;
     - `'AllowedWriteScope' in bfc.Fields`.
     Si el contrato SÍ lleva EXACT_CLAUSE, se acepta como antes (compatibilidad con F4).
   - Nada más del código cambia. Al final, lista las líneas cambiadas frente a `data/pbt2a-substitute-as-run.py`, con `diff`, en `adapt-diff.txt`.
3. **Autoprueba:** añade casos sintéticos nuevos, sin tocar los A-N existentes salvo lo mínimo para que el bundle sintético pueda llevar la nota y el bfc de F4B:
   - un caso F4B que pasa;
   - por cada condición nueva de 5 y 8, un caso que falla si esa condición se rompe. Por ejemplo:
     - la nota no coincide con el relevo;
     - `DelegationSha256` ≠ SHA(delegation);
     - `NonEmptyInControllerOutput` no vacío;
     - el contrato sin la comparación byte a byte y sin EXACT_CLAUSE;
     - el bfc con `AllowedWriteScopeElementwiseInOrder` false;
     - el bfc con `Result` FAIL.
   Todos los casos A-N existentes deben seguir dando su resultado esperado.
4. **Ejecuta** (Python con `-I`; escribe los scripts con la herramienta Write, nunca con heredoc ni `python -c`) contra los datos copiados en `data/`:
   - verificación real: `data/real` (con `inputs/` y `output.json`);
   - nc2: `data/nc2` (con `inputs/`, `output.json` y `mutation.json`);
   - nota: `data/planning/session-inject-note.json`;
   - BaseSha `17872be8` (completo: `git -C "<checkout-principal>" rev-parse 17872be8`);
   - CurrentSha `5ddeb3b4ed99cb17d05142c67d47a26e89e238f9`.

   El script hace `git diff --name-only BaseSha..CurrentSha` con el cwd. Ejecútalo con `cwd = <checkout-principal>` (el checkout principal, que comparte objetos), sin tocar ese checkout más allá de lecturas git. **NUNCA** leas ni ejecutes nada en `<worktrees-de-Codex>\...` (regla P-08). Las rutas de datos pásalas absolutas.

   Guarda la salida en `preview-run.txt` y el JSON en `preview-result.json`. Ejecuta también el sustituto de F4 original sobre los mismos datos, como contraste (debe dar FAIL en 5 y 8): `contrast-run.txt`.
5. **Escribe `adapt-notes.md`:**
   - qué cambió y por qué, línea por línea;
   - qué casos nuevos hay y qué rompe cada uno;
   - el resultado de la vista previa (PASS/FAIL, las 10 propiedades);
   - cualquier duda.

   Si la vista previa da FAIL, NO ajustes el sustituto para forzar PASS: informa qué falla y por qué.

Responde con:
- el resultado de la vista previa;
- las propiedades;
- el número de líneas cambiadas;
- los casos nuevos;
- las dudas.

Sé conciso.

## 2. encargo inicial al revisor (2026-11-19T08:21:10.567Z)

Haz una revisión independiente y adversarial de un script Python, un «sustituto de control nc2». Busca por qué podría dar PASS cuando no debe (un falso PASS) o FAIL cuando debe pasar. No apliques ni custodies nada.

## Qué es
En un régimen de verificación, el «control nc2» copia las entradas de una verificación real y quita UNA ruta del `AllowedWriteScope` de `delegation.json`, recalculando los hashes derivados. Después un Controller (un modelo) vuelve a verificar. Lo esperado según la firma PD-F4-01 es:
- Scope en fail sobre la ruta quitada;
- Contract en fail solo por la exactitud de la lista blanca;
- EXECUTION_BLOCKED/STOP;
- ninguna otra comprobación distinta de la verificación real.

El sustituto demuestra mecánicamente 10 propiedades sobre esos datos, tras una autoprueba sintética que corre dos veces y debe ser determinista.

**El original** es `data/pbt2a-substitute-as-run.py`, una copia de un sustituto ya aceptado en una fase anterior (F4). Contra los datos actuales da FAIL en las propiedades 5 y 8, por dos diferencias de formato de la fase nueva (F4B):
- **Regla F4B-E21:** el Controller deja vacías 8 colecciones opacas y la sesión las inyecta desde el contrato. Por eso el relevo de planificación (`relay-record-planning.json`, `Outcome.OutputSha256`) lleva el SHA de la salida CRUDA del Controller y no el de `delegation.json`. La nota `data/planning/session-inject-note.json` registra los dos.
- **Los contratos de F4B no llevan** la frase literal `EXACT_CLAUSE` en `ExpectedEvidence`. Su exactitud se exige por una «comparación byte a byte», atestiguada por `byte-fidelity-check.json`.

**El adaptado** es `nc2_exact_whitelist_substitute_f4b.py`. Solo debía cambiar el id, el ámbito y el registro, más las propiedades 5 y 8 para aceptar el formato de F4B, sin aflojar nada más.

## Dónde
Carpeta: `<scratchpad>\b491591bc47aa335\98cecaf8-117d-4d62-b418-bd3e08504b6f\scratchpad\hs65\pbt2acc2\nc2-substitute-f4b\`
- `data/pbt2a-substitute-as-run.py`, el original;
- `nc2_exact_whitelist_substitute_f4b.py`, el adaptado;
- `data/real/` (`inputs/` y `output.json` de la verificación real);
- `data/nc2/` (`inputs/`, `output.json` y `mutation.json` del control);
- `data/planning/` (la salida cruda `output.json`, `session-inject-note.json` y `byte-fidelity-check.json` de la planificación).

No leas `adapt-notes.md`, `adapt-diff.txt` ni `preview-*` hasta haber terminado tu propio análisis: quiero una lectura no anclada. Al final puedes contrastar con ellos.

## Qué revisar
1. **El diff:** ¿cambia algo más que id, ámbito, registro, propiedades 5 y 8, autoprueba y el 6.º argumento? Hazlo tú con `diff` o con difflib.
2. **La propiedad 5 adaptada:** ¿ata de verdad la cadena relevo crudo → nota → delegación real → bfc? ¿Y el relevo mutado → delegación mutada? ¿Puede pasar con una nota inventada o incoherente? ¿Qué pasa si la nota falta o está mal formada?
3. **La propiedad 8 adaptada:** ¿basta una subcadena en `ExpectedEvidence`? ¿Hay entradas de `ExpectedEvidence` en el contrato real que la contengan por casualidad (por ejemplo, para otra cosa)? Las condiciones sobre el bfc, ¿se leen del bfc real o del mutado? ¿Se puede pasar sin que el bfc atestigüe el `AllowedWriteScope` exacto? ¿Sigue exigiendo A3/A4/A5 llanos, `ma != ca` y las opacas iguales?
4. **La autoprueba:** ¿cada caso nuevo rompe exactamente lo que dice? ¿Falta algún caso negativo para una condición nueva? ¿Siguen intactos los A-N originales?
5. **Ejecútalo tú mismo** (Python con `-I`, scripts propios escritos con la herramienta Write, nunca heredoc ni `python -c`), con cwd = `<checkout-principal>`:
   - **los argumentos:** `<data/real> <data/nc2> <BaseSha> 5ddeb3b4ed99cb17d05142c67d47a26e89e238f9 <salida-propia.json> <data/planning/session-inject-note.json>`, con BaseSha = `git -C "<checkout-principal>" rev-parse 17872be8` y rutas absolutas;
   - **las mutaciones:** prueba al menos 3 mutaciones propias de los datos, en copias dentro de una subcarpeta `review/`, que deberían dar FAIL. Por ejemplo, una nota con otro SHA crudo, un bfc del real con Result FAIL, o un contrato sin la frase.
   - **Nunca** leas ni ejecutes nada en `<worktrees-de-Codex>\...`. Escribe solo dentro de `nc2-substitute-f4b\review\`.

## Salida
Escribe `review/independent-review-substitute-f4b.md` con:
- un veredicto: APTO, APTO TRAS CORRECCIONES o NO APTO;
- los conteos B / I / M (bloqueante, importante, menor);
- los hallazgos con su línea;
- tus ejecuciones.

Responde con el veredicto, los conteos y los hallazgos, uno por línea. Sé conciso.

## 3. mensaje al autor (Corregir sustituto F4B tras revisión 0/4/7) (2026-11-19T08:30:29.175Z)

La revisión independiente de tu sustituto adaptado dio APTO TRAS CORRECCIONES, B 0 / I 4 / M 7. El informe está en `nc2-substitute-f4b\review\independent-review-substitute-f4b.md`, con sus sondas en `review\runs\` y `review\mut\`. Léelo entero. Corrige `nc2_exact_whitelist_substitute_f4b.py` en el mismo archivo, sigue sin aplicar nada y escribe solo dentro de `nc2-substitute-f4b\`.

**Las importantes:**
- **I-1, atar el relevo a la salida cruda.**
  - `main` recibe como 7.º argumento la salida cruda de la planificación (`data/planning/output.json`). Deja la nota y la salida cruda obligatorias en el modo F4B.
  - Exige estas tres cosas:
    - SHA(salida cruda) == `note.OutputSha256` == `rr0.Outcome.OutputSha256`;
    - las 8 colecciones opacas, vacías en la salida cruda: se calcula, no se cree a la nota, y `note.NonEmptyInControllerOutput` debe coincidir con lo calculado;
    - la salida cruda con las 8 colecciones tomadas del contrato == `delegation.json` real, por igualdad JSON (`json.loads`). La revisión comprobó que hoy coincide. Si quieres, añade también la comparación byte a byte reproduciendo la serialización de `inject_opaque.py` (`pbt2acc2\inject_opaque.py`, solo lectura); si no la reproduces exacta, deja solo la igualdad JSON y decláralo.
  - `note.Injected` debe ser exactamente la lista de las 8, sin repetidos y sin elementos de más.
- **I-2, validar la nota entera:**
  - `note.RunId == bfc.PlanningRunId`;
  - `note.ContractSha256 == SHA(contrato)`;
  - `note.DelegationSha256 == SHA(delegación real)`;
  - `Fidelity` contiene PASS;
  - `Decision` empieza por `F4B-E21`.

  Registra en el resultado la ruta y el SHA de la nota y de la salida cruda.
- **I-3, la propiedad 8 por igualdad exacta, como el original.**
  - Define `F4B_EXACT_EVIDENCE` como el literal completo de la entrada F4B de `ExpectedEvidence` («Antes del Worker: el contrato, la salida del Controller (output.json, con las colecciones opacas vacias), la delegacion final con esas colecciones puestas por la sesion (delegation.json), la comparacion byte a byte, A1-A8 y nc4.»). Cópialo byte a byte de `data/real/inputs/gate-contract.json`, sin reescribirlo.
  - Exige que `ExpectedEvidence` lo contenga como entrada exacta. Quita la subcadena normalizada.
- **I-4, no se cambia el kit de nc2.** Su recálculo del hash del relevo con la regla de F4 es lo que hace hoy el kit. El sustituto acepta exactamente eso y solo eso: `rr1.Outcome.OutputSha256 == SHA(delegación mutada)`.
  - Decláralo en `Notes`, en una clave `Nc2KitRelayRecompute`, y en el docstring: el kit recalcula con la regla de F4; el sustituto lo exige así, y una copia que siguiera F4B-E21 sería otra mutación de kit, fuera de este sustituto.
  - No añadas lógica para aceptar las dos.

**Las menores:**
- **M-2:** en cada caso negativo nuevo, la autoprueba exige que falle la propiedad objetivo y que todas las demás sigan en true.
- **M-3:** añade los casos que faltan:
  - nota incoherente (RunId, ContractSha256 y Fidelity, cada uno por separado);
  - relevo que no cuadra con la salida cruda;
  - una colección no vacía en la salida cruda;
  - salida cruda más colecciones distinta de la delegación;
  - `Injected` con un elemento de más o repetido;
  - una entrada de `ExpectedEvidence` parecida pero no exacta, por ejemplo negada.
  
  Si alguno no se puede aislar, por solaparse con la 1, dilo y explícalo; no lo fuerces.
- **M-4:** el script borra `out_path` al empezar. Si la nota o la salida cruda faltan o están mal formadas, escribe un resultado FAIL con el motivo, sin excepción.
- **M-5:** acopla los modos.
  - **Con nota y salida cruda,** el modo F4B: la 5 y la 8 con las reglas de F4B; la 8 exige `F4B_EXACT_EVIDENCE` y el bfc.
  - **Sin ellas,** el modo F4 exacto: la 5 y la 8 como el original, con `EXACT_CLAUSE`.
  - Registra `Mode` en el resultado.
- **M-6:** corrige el docstring (fuente: copia de F4T1B-NC2-EXACT-WHITELIST-01, vía PBT2A-NC2-EXACT-WHITELIST-01), el texto de `Notes` y el error de la tilde en `adapt-notes.md`.
- **M-1 y M-7:** solo decláralos en `adapt-notes.md`.
  - **M-1:** se solapa con la propiedad 1.
  - **M-7:** heredado; la sesión comprueba Outcome y «ninguna otra comprobación distinta» con su tabla de comparación, fuera del sustituto.

**Después:**
- vuelve a ejecutar la vista previa con los 7 argumentos (`preview-run.txt`, `preview-result.json`);
- ejecuta las sondas X1, X2 y X3 de la revisión (en `review\mut\`), que ahora deben dar FAIL; guarda su salida en `probes-after-fix.txt`;
- actualiza `adapt-diff.txt` y `adapt-notes.md`.

**Reglas:** las mismas de antes. Nada en `<worktrees-de-Codex>\...`; cwd en el checkout principal solo para lecturas git; Python con `-I`, escrito con Write.

**Responde con:**
- la vista previa;
- la autoprueba (casos);
- las sondas X1, X2 y X3;
- las líneas cambiadas;
- las dudas.

## 4. mensaje al revisor (Recomprobar sustituto F4B corregido) (2026-11-19T08:34:58.828Z)

El autor corrigió `nc2_exact_whitelist_substitute_f4b.py` tras tu revisión (APTO TRAS CORRECCIONES 0/4/7). Las decisiones de la sesión:
- I-1 a I-3 corregidas;
- I-4: el kit de nc2 NO se cambia; el sustituto exige exactamente el recálculo del kit con la regla de F4, y lo declara en `Notes.Nc2KitRelayRecompute`;
- M-2 a M-6 corregidas;
- M-1 y M-7 solo declaradas.

Ahora `main` lleva 7 argumentos: los 5 de antes, más la nota y la salida cruda de la planificación (`data/planning/output.json`).

**Haz una recomprobación acotada.** Añade «§R. Recomprobación» al final de tu `review\independent-review-substitute-f4b.md`, sin reescribir lo anterior.
1. **Tus hallazgos:** por cada uno, si está corregido, con la línea.
2. **Tus sondas:** vuelve a ejecutar X1 a X4 y tus mutaciones M1 a M7 con los 7 argumentos.
   - X1 a X3 deben dar FAIL. X4 da FAIL por I-4, y eso es lo esperado.
   - M6 y M7 deben escribir un resultado FAIL sin excepción.
3. **Lo nuevo:**
   - **Sondas nuevas:** busca falsos PASS en lo que se añadió:
     - la salida cruda con las colecciones frente a la delegación, por igualdad JSON;
     - `F4B_EXACT_EVIDENCE` como entrada exacta;
     - el acoplamiento de modos;
     - `note.RunId == rr0.RunId`.
   - **El diff:** que no toque nada fuera de lo previsto.
4. **La autoprueba:** que dé 40 de 40, que sea determinista, y que cada caso exija exactamente el conjunto de propiedades que dice.

Las reglas son las de antes:
- escribe solo en `review\`;
- cwd en el checkout principal, solo con lecturas git;
- Python con `-I`, scripts escritos con Write;
- nada en `<worktrees-de-Codex>\...`.

**Veredicto:** APTO o APTO TRAS CORRECCIONES, con B / I / M nuevos. Responde con el veredicto y los hallazgos, uno por línea.

## 5. mensaje al autor (Aplicar M-R1..M-R3 de la recomprobación) (2026-11-19T08:41:48.969Z)

La recomprobación (§R, al final de `review\independent-review-substitute-f4b.md`) da **APTO**, con 3 menores nuevas. Aplícalas en el mismo archivo:
- **M-R1:**
  - la salida cruda más las 8 colecciones debe ser igual a la delegación **byte a byte**, con la serialización de `inject_opaque.py` que ya mediste; esto pasa a exigido;
  - al leer la salida cruda, la delegación y la nota, rechaza las claves duplicadas (`object_pairs_hook`);
  - exige que las 8 claves estén en la salida cruda, con `''` exacto en `Objective` y `[]` exacto en las otras 7.
- **M-R2:** `RunId` no vacío, y el mismo en la nota, el bfc (`PlanningRunId`), `rr0` y la delegación real.
- **M-R3:** `Fidelity` debe empezar por `PASS`.

**Autoprueba:** añade un caso por cada una que falle solo en la propiedad objetivo, o declara el solapamiento si no se puede aislar.

**Ejecuta:**
- la vista previa (`preview-*`, debe dar PASS);
- las sondas del revisor que dieron falso PASS (N1, N2, N3, N5, N6, N7 y N8, con su `review\mutations2.py` o en copias tuyas). Deben dar FAIL. Guárdalo en `probes-after-fix2.txt`.

Actualiza `adapt-diff.txt` y `adapt-notes.md`. Las reglas son las de siempre.

**Responde con:**
- la vista previa;
- la autoprueba;
- las sondas;
- las líneas cambiadas.
