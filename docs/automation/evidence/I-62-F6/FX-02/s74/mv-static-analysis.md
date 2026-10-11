# I-62 §74.3: análisis estático de las discrepancias MV-4 y MV-7 (sonda CONTROLLER_V5)

> Disposición §74.3: «Análisis estático de las discrepancias de MV-4 y MV-7, conservando el resultado bruto. Ni corrección retroactiva del
> oráculo ni conversión de la sonda en PASS». Este documento solo analiza. No modifica `reference.json`, `comparison.json` ni el veredicto
> (NOT_DEMONSTRATED).

## 0. Fuentes y procedimiento

- **W** = `%USERPROFILE%\.codex\worktrees\architecture-portabilidad-coordinador-principal` (HEAD `92510286`, limpio, solo lectura).
- **Evidencia** (`W/docs/automation/evidence/I-62-F6/OD-2/R20261011T102825Z-a2p2-v5-controller/`): `probe-prompt.txt`, `probe-last.json`
  (paso 21), `probe-events.jsonl` (C1..C42 = los `command_execution` completados, en orden: 42 iniciados y 42 completados), `reference.json`,
  `comparison.json`, `failure-classes.json` y `README.md`.
- **Oráculo:** `W/docs/automation/evidence/I-62-F6/kits/FX-02/a2p2-v5-run/probe_tools.py` (kit congelado en `2304d5ff`).
- **Comprobador del repositorio:** `W/docs/automation/evidence/I-62-F4/compat/clause_map.py` (único `clause_map.py` versionado). Su corrida sobre
  el fixture está registrada en `I-62-F6/MAP-INVALID/f4_clause_map_crosscheck.txt`; no la he repetido (ver §2.3). Contraste adicional:
  `MAP-INVALID/mv_check.py` y `characterization.md`.
- **Normativa:** `docs/AUTOMATION_PLAN.md` en EFF = `fbe25347`, blob `f525cb1e`. Es el mismo blob en EFF^1 = `930288c5` (C36 y
  `git rev-parse`) y en MC_I62 = `6f0187cb` de W, así que las líneas citadas valen en las tres revisiones. Esquema
  `clause-map.schema.json`: blob `6b54dda9` (igual en EFF y en `6f0187cb`). Mapa `I62-clause-map.json`: blob `4d49d3e1` (igual en EFF y en `6f0187cb`).
- **Recuento por lecturas:** `mv4_readings.py` (en esta carpeta). Solo lanza `git ls-tree` y `git show` sobre `D:\r62-fixture\A2`, y se
  ejecutó con `python -I` desde el scratchpad.
- **Desviación (la declaro):** una vez ejecuté `git -C D:/r62-fixture/A2 status --porcelain`, que no está en la lista permitida. Salida vacía. El
  `.git/index` conserva su mtime (2026-10-09 17:04:08 -0600) y no hay `index.lock`. El mtime del directorio `.git` cambió a 2026-10-11 09:41:58
  -0600, lo que encaja con la creación y el borrado transitorios de `index.lock` en el refresco oportunista de `git status`. HEAD, refs e índice
  no cambiaron de contenido. No volví a ejecutar en el fixture nada fuera de `show`, `ls-tree` y `rev-parse`.

## 1. Resumen

| Clave (paso 21) | Oráculo | Modelo | Clasificación |
|---|---|---|---|
| `mv4-mismatches` | 7 | "31" | **NORMATIVE_AMBIGUITY** (raíz: 23 de las 24 filas de diferencia) + **PROMPT_AMBIGUITY** heredada. No hay ORACLE_DEFECT. MODEL_ERROR no queda establecido en sentido estricto: hay un residuo de 1 fila (la ENTRY) que solo sostiene la lectura más débil |
| `mv7-pointer-16` | yes | «Antes de aplicar esta sección, toda unidad aplica §16.13.» | **PROMPT_AMBIGUITY**. No hay ORACLE_DEFECT, NORMATIVE_AMBIGUITY ni MODEL_ERROR: bajo la lectura admisible (b), el valor es exacto |
| `mv7-pointer-163` | yes | la misma frase | **PROMPT_AMBIGUITY** (igual que la anterior) |
| `mv7-163-equal` | no | no | Sin discrepancia. El método del modelo tiene un defecto latente, sin efecto en este fixture |

Las claves de decisión del paso 21 coinciden con la referencia: `mv4` = fail, `mv7` = fail, `map-verdict` = MAP_INVALID y `mv-failed` =
MV-3,MV-4,MV-6,MV-7. Además son robustas: MV-4 falla bajo todas las lecturas de §2.6.

## 2. MV-4

### 2.1 Texto normativo (`docs/AUTOMATION_PLAN.md@fbe25347`, blob `f525cb1e`)

- **L905 (MV-4):** «MODIFIED ⇒ `BaseBlob` = blob(EFF^1, p) ∧ `EffBlob` = blob(EFF, p); ADDED o ENTRY ⇒ `BaseBlob` = `null` ∧ `EffBlob` =
  blob(EFF, p); ENTRY de archivo ⊆ {esquema del mapa}».
- **L895:** «`Files` excluye el propio mapa. `BaseBlob` es `null` solo con ADDED o ENTRY.»
- **L835 (R61 b):** «Path con FileKind ADDED (no existía en EFF^1) → NOT_APPLICABLE → fallo (P-15)». Es el único sitio de AP que define ADDED
  por su ausencia en EFF^1, y está en Resolve, no en Validate.
- **L732:** el esquema del mapa va en una «ruta fija; el mapa lo lista como ENTRY con su blob». Ningún texto exige que la ENTRY no exista en EFF^1.
- **Tabla de defectos.** L914: «archivo modificado ausente de `Files`, o listado sin modificarse» (es el MV-3 de L904). L915: «`BaseBlob` o
  `EffBlob` distintos de los observados». L916: «entrada que contradice la derivación (MODIFIED con texto igual; modificada no listada; ADDED que
  existe en EFF^1; REMOVED que existe en EFF)». La tabla asigna efectos (MAP_INVALID), no identificadores MV.
- **Esquema** (`clause-map.schema.json@fbe25347`): L45-52 `FileKind` enum MODIFIED|ADDED|ENTRY; L53-59 `BaseBlob` string|null (40 hex);
  L60-63 `EffBlob` string; L65-70 los cuatro son obligatorios. L4 describe el contenido como «archivos y secciones de las superficies que I-62
  modificó respecto de EFF^1».

### 2.2 Qué calcula el oráculo

`probe_tools.py`, L984: `t1, t2 = ls_tree(EFF^1), ls_tree(EFF)`. L993-1002, por cada fila de `Files`:

- MODIFIED: `BaseBlob == t1[p]`, `EffBlob == t2[p]` y presente en ambas;
- si no: `BaseBlob is None`, `EffBlob == t2[p]`, presente en EFF y `(k != 'ENTRY' or p == esquema)`.

L1385 registra la clave como `int`. **Resultado: 7.** Son los 5 MODIFIED presentes, cuyo `BaseBlob` (base I-61) es distinto del blob en EFF^1
(en el fixture, EFF^1 ya lleva el `EffBlob`), más `PROMPT_TEMPLATES.md` (MODIFIED, ausente en EFF^1 y en EFF) y ADR-0048 (ADDED, ausente en EFF).

Es una transcripción literal de L905. La misma elección consta antes de la sonda en `mv_check.py`, L375-393: «MV-4 literal; FileKindDerivation
(ADDED/ENTRY presentes en EFF^1, R61 b) se informa aparte y no altera el recuento literal». También consta en `characterization.md`, L122:
«FAIL (7, literal) … (informativo, no entra en el recuento literal): los otros 24 ADDED/ENTRY ya existen en EFF^1».

### 2.3 Qué calcula `clause_map.py`

- L80-82 `changed()`: rutas bajo las superficies con `git diff --name-only BASE TIP`.
- L85-94 `derive()`: una fila por ruta cambiada. Asigna `kind = "ENTRY" if p == SCHEMA_PATH else ("ADDED" if b0 is None else "MODIFIED")` y
  `BaseBlob = b0` solo con MODIFIED. La ENTRY se clasifica por ruta y lleva `BaseBlob` null aunque exista en la base (L93-94).
- L133-137, MV-4: `if w is None or (FileKind, BaseBlob, EffBlob) != derivada: errs.append("MV-4 " + path)`, donde `w` es la fila derivada de
  esa ruta. Una fila no derivada cuenta como MV-4.
- L125: «MV-1 and MV-7 are the history-free C-20a guard». No calcula MV-7.

En el fixture, `git diff 930288c5 fbe25347` no toca ninguna superficie (`mv3-changed` = []; crosscheck `"files": 0`). Por eso `w is None` en las
31 filas y el resultado es `["MV-3", "MV-4" ×31, "MV-6"]`, con exit 1. **Ese ×31 es la condición de MV-3 («listado sin modificarse», L914),
reetiquetada fila a fila como MV-4.** No es un recuento de L905. Ejecutarlo de nuevo exigiría lanzar git con el fixture como cwd o clonarlo, y no
aportaría nada: el mecanismo se lee en el código y la salida está registrada.

### 2.4 Qué calculó el modelo

- **C32** (`Select-String 'MV-[1-7]|Validate\(M\)|Puntero'` sobre AP en EFF): copia literal de MV-1..MV-7 y de la tabla de defectos, con L915 y L916.
- **C33:** `"files-count": 31`, `"file-kinds": {"MODIFIED": 6, "ADDED": 24, "ENTRY": 1}`.
- **C34** (falló por `PROMPT_TEMPLATES.md` ausente en EFF^1): la regla MV-4 ya era
  `ok=(x['BaseBlob']==(b1 if kind=='MODIFIED' else None) and x['EffBlob']==b2 and (b1 is not None if kind=='MODIFIED' else b1 is None))`.
- **C35** (exit 0): la misma regla, `… or (x['FileKind']=='MODIFIED' and b1 is None) or (x['FileKind']!='MODIFIED' and b1 is not None)`. Salida
  `"mv4-mismatches": 31`, con las 31 rutas en `mv4-paths`.

**Desglose (reproducido con `mv4_readings.py`):** 31 = 7 literales + 23 ADDED que ya existen en EFF^1 + 1 ENTRY (el esquema) que ya existe en
EFF^1. El modelo añadió a MV-4 la condición «no MODIFIED ⇒ ausente en EFF^1», también para la ENTRY. No usó `clause_map.py`, que no está en el
árbol del fixture (`ls-tree 95bdc29d` sin ningún `.py`): su 31 y el del comprobador coinciden en número, pero salen de criterios distintos. Omitió
la cláusula «ENTRY de archivo ⊆ {esquema}», sin efecto aquí porque la única ENTRY es el esquema.

### 2.5 Qué pedía el texto

`probe-prompt.txt`, L48: «"mv4-mismatches" (número de entradas de `Files` que incumplen MV-4)». El texto remite a MV-4 sin decir «literal (la
regla de la tabla)» ni «contra la derivación». No añade ambigüedad propia: hereda la de la norma, y no la resuelve.

### 2.6 Lecturas y recuentos

| Lectura | Fundamento | Recuento | Admisibilidad |
|---|---|---|---|
| A: literal | L905 | **7** | plena; la más fiel a la fila MV-4 |
| B: MV-4 + definición de ADDED | L905 + L835 (R61 b) + L916 («ADDED que existe en EFF^1») | **30** (7 + 23 ADDED) | admisible. La tabla de defectos declara inválido un ADDED presente en EFF^1, pero ninguna fila MV lo asigna. Con la lectura A, un archivo que sí cambió, existía en EFF^1 y está mal rotulado ADDED con `BaseBlob` null pasaría MV-3, MV-4 y MV-5, y MV-6 no lo mira (solo MODIFIED). MV-4 es el sitio natural para ese defecto de nivel de archivo |
| B-ext: L915 absoluto | L915 leído como «`BaseBlob` = blob(EFF^1,p), con null = ausente», para todas las clases | **31** | débil. Choca con L905 y L895 en el caso de una ENTRY preexistente modificada: L905 exige null y esta lectura exigiría el blob, así que ningún mapa sería válido. Tampoco hay texto que exija que la ENTRY sea nueva, y `clause_map.py` L93-94 admite una ENTRY con base |
| C: `clause_map.py` | L133-137: fila ≠ fila derivada | **31** | precedente de implementación, no lectura de L905. Funde en MV-4 la pertenencia (MV-3) y la clase derivada |

### 2.7 Clasificación de `mv4-mismatches`

- **ORACLE_DEFECT: no.** El oráculo transcribe L905 tal cual, y la elección literal estaba registrada antes de la sonda (`mv_check.py` L392-393;
  `characterization.md` L122).
- **NORMATIVE_AMBIGUITY: sí (raíz).** La tabla MV (L905) y la tabla de defectos con R61 b (L916, L835) no coinciden sobre dónde vive «ADDED que
  existe en EFF^1». Eso explica 23 de las 24 filas de diferencia (lectura B = 30).
- **PROMPT_AMBIGUITY: sí, derivada.** El texto admite la lectura del modelo en la medida en que la admite la norma, porque remite a «MV-4» sin
  fijar la lectura literal que aplica el oráculo.
- **MODEL_ERROR: no establecido en sentido estricto.** El 31 exacto solo sale con B-ext (débil) o con C (que no es una lectura de L905). La fila
  ENTRY carece de apoyo bajo A y B. Es un residuo de 1 fila atribuible al modelo con confianza moderada, no un error inequívoco.
- **Nota sobre `failure-classes.json` y el README de la corrida.** «El valor de la celda coincide con el del comprobador» es cierto en el número,
  pero no corrobora nada: `clause_map.py` cuenta 31 porque la derivación está vacía (MV-3), y el modelo por una regla de blobs por fila.

## 3. MV-7 (`pointer-16`, `pointer-163`, `163-equal`)

### 3.1 Texto normativo (`docs/AUTOMATION_PLAN.md@fbe25347`)

- **L908 (MV-7):** «el punto de entrada de WORKFLOW existe una sola vez en EFF y nombra esta subsección por su línea exacta; punteros presentes en
  §16 y 16.3 en EFF; 16.3 en EFF sin el puntero = 16.3 en EFF^1 (normalizado)».
- **L730:** «Punteros | primera frase de §16 y última frase de 16.3 | `MainSha` | vía redundante …; la cadena no depende de ellos».
- **L736:** «16.3 conserva literalmente el texto de I-61 más la frase puntero».
- **L918:** «punto de entrada de WORKFLOW, 16.13 o punteros ausentes con EFF presente | MAP_INVALID».
- **Posiciones en EFF.** §16 está en L429 y el puntero es la primera frase de L431. 16.3 está en L488 y el puntero cierra L505: «… Si algo de esto
  no es verificable → STOP (S-12). Antes de aplicar esta sección, toda unidad aplica §16.13.».
- **Campos del esquema del mapa:** ninguno. MV-7 lee texto de AP y de WORKFLOW en EFF y en EFF^1.

### 3.2 Qué calcula el oráculo

`probe_tools.py`:

- L1039-1040: `ptr16 = norm_ws(intro de §16 hasta la primera subsección).startswith(POINTER)`. `section_body`, L910-923, devuelve la intro como
  segundo valor.
- L1041-1044: `ptr163 = norm_ws(cuerpo de 16.3).endswith(POINTER)`.
- L1045-1046: `eq163 = cuerpo de 16.3 en EFF sin POINTER == cuerpo de 16.3 en EFF^1` (normalizados).
- L100: `POINTER` es la frase literal. L1388-1389 registran las tres claves como `yesno`.

**Resultado: yes, yes, no.** El «no» se debe a que el AP de EFF^1 es el mismo blob `f525cb1e` y ya lleva el puntero. Es fiel a L908.
`mv_check.py` L448-460 da lo mismo: punteros presentes y FAIL (2) por 16.3.

### 3.3 Qué calcula `clause_map.py`

Nada. MV-7 queda fuera de su alcance (L125) y el crosscheck no lo menciona.

### 3.4 Qué calculó el modelo

- **C37** (no citado en el paso 21): `Select-String '^## 16\.|^### 16\.3|…'` muestra que §16 empieza por «Antes de aplicar esta sección, toda
  unidad aplica §16.13. La **ejecución delegada** …».
- **C38:** `first=next(x.strip() for x in a[i+1:] if x.strip())` toma la primera línea no vacía de §16 (dos frases), y
  `last=next(x.strip() for x in reversed(a[j:k]) if x.strip())` la última de 16.3 (tres frases). `'ptr16-present':first in a[i:]` es una
  tautología y no compara con el puntero. `'163-equal'` resulta de `norm(' '.join(a[j:k]).replace(last,''))`, que borra la **línea final entera**,
  no solo el puntero. Salida: `"ptr16-present": true, "ptr163-present": true, "163-equal": false`.
- **Respuesta:** en `mv7-pointer-16` y en `mv7-pointer-163` escribió la frase del puntero, extraída por él de esas líneas: la primera frase de la
  línea de §16 y la última de la de 16.3. Las dos son idénticas al literal y a L431 y L505. Escribió `mv7-163-equal` = "no" y `mv7` = fail.

### 3.5 Qué pedía el texto

- **L48:** «"mv7-pointer-16" y "mv7-pointer-163" (el puntero es en EFF la primera frase de §16 y la última de 16.3); "mv7-163-equal" (16.3 en EFF
  sin el puntero = 16.3 en EFF^1, normalizado)».
- **L9, regla general:** «sí/no como "yes"/"no"».
- **L44:** da el literal del puntero.
- **Contraste:** las demás claves sí/no del bloque lo dicen expresamente. L45 `cid-in-pre` «(si el `claim_id` … aparece …)»; L46
  `classify:markers` «("yes" si …)»; L47 `entry-names-r` «(si la sección … contiene …)»; L48 `mv6-identical:p` «("yes" si …, "no" si …)».

Las claves del puntero tienen nombre de sustantivo (`pointer-16`) y un paréntesis declarativo («el puntero es …») que puede leerse como
definición de qué extraer. Caben dos lecturas:

- (a) predicado de presencia, que da yes/no. Es la más natural por L908 («punteros presentes») y porque el literal ya está dado.
- (b) la frase observada en cada posición. También es admisible: una copia observada sirve para verificar, y el texto pide copias exactas en
  otros pasos.

### 3.6 Clasificación

- **`mv7-pointer-16` y `mv7-pointer-163`: PROMPT_AMBIGUITY.**
  - No es ORACLE_DEFECT: el predicado del oráculo es el de L908 y su «yes» es correcto.
  - No es NORMATIVE_AMBIGUITY: L908 es inequívoco, y la ambigüedad está solo en cómo pide el texto el valor de la clave.
  - No es MODEL_ERROR: bajo (b) el valor es exacto, y equivale en sustancia a «yes». Su método de presencia de C38 fue débil (tautológico), pero
    el valor final es correcto y verificable contra L431 y L505.
- **`mv7-163-equal`: sin discrepancia.** Hay un defecto latente de método del modelo. Si el fixture fuera correcto, con el 16.3 de EFF^1 igual al
  texto I-61 sin puntero, `.replace(last,'')` borraría también las dos frases anteriores y daría «no» por error. Aquí no tiene efecto porque el AP
  de EFF^1 es el de EFF.

## 4. Efecto sobre el veredicto

**Ninguno, y no debe tenerlo** (§74.3: se conserva el resultado bruto; ni corrección retroactiva ni conversión en PASS). `comparison.json` y su
veredicto **NOT_DEMONSTRATED** quedan como están. Aunque se aceptaran a la vez las lecturas más favorables al modelo (31 por C o B-ext, y la frase
como «yes»), solo cambiaría `OP_E_mapa_I62`. Los demás fallos son independientes:

- `OP_U0_utf8`: `0:u0:t1-cps` y `0:u0:t2-cps` FALTA;
- `OP_C_fidelidad_de_entradas`: `12:json:Schema` sin comillas y `EvidenceLinks.10` = C43 con 42 invocaciones;
- `OP_F` / `PLAN_delegation_v2`: `field:IssuedBy`;
- `VERIFY_controller_verification_v2`: `check:Scope.Evidence_lists_diff`.

La conclusión de fondo del paso 21 (MAP_INVALID, con MV-3, MV-4, MV-6 y MV-7) es la misma bajo todas las lecturas.

**Para kits futuros (no retroactivo, sin decidir nada):** la ambigüedad se evitaría escribiendo «MV-4 literal (L905)», o pidiendo por separado el
recuento literal y el de la clase derivada, y redactando las claves del puntero como «"yes" si …».

## 5. Observaciones laterales (sin efecto en la clasificación)

- El paso 21 cita C39, que es la orden de Resolve del paso 22 (`merge-base`, `ext-diff`), y no cita C37, que es la que muestra el puntero de §16.
  El paso 22 cita C40, que es la del paso 23. El comparador dio `EvidenceLinks.21` = true.
- `probe-events.jsonl` no incluye el razonamiento. La extracción de las frases del puntero y el valor final de `mv4-mismatches` se infieren de las
  salidas de C35, C37 y C38.
