"""PB-T4d (paquete v5.3): aplica a las copias de los fragmentos de la v5.2 (v53/frag/, copiadas de v52/frag/ y v52/frag_56.md) los cambios de
la v5.3 por anclaje exacto y contado, para que make_v53.py los lleve al paquete. Lo que se mide (las cifras) va con marcadores @@...@@ que llena
make_v53.py. Parte siempre de las copias de la v5.2 (las vuelve a leer de v52/), asi que se puede repetir. Escribe UTF-8 con LF.

Uso: python -I edit_frags_v53.py
"""
import os

HERE = os.path.dirname(os.path.abspath(__file__))
V52 = os.path.join(os.path.dirname(HERE), 'v52')
SRC = {'d10.md': os.path.join(V52, 'frag', 'd10.md'), 's44.md': os.path.join(V52, 'frag', 's44.md'), 's56.md': os.path.join(V52, 'frag_56.md'),
       's64.md': os.path.join(V52, 'frag', 's64.md')}
EDITS = {k: [] for k in SRC}


def E(name, old, new, times=1):
    EDITS[name].append((old, new, times))


# ---------------------------------------------------------------- D-PBT4-10
E('d10.md', '**Lo que retira PB-T4d** (v5.2; el alcance B,', '**Lo que retira PB-T4d** (v5.3; el alcance B,')
E('d10.md', 'la condición `allowErase` de sus tres ramas,',
  'la condición `allowErase` de sus cuatro condiciones (tres en el bucle, `NoAddress`, `ObsoleteLateral` y `FrontalBOfOneWay`, y la de `AllObsolete`; CA-T4d M-05 b),')
E('d10.md', '- y los cuatro `cref` y el comentario que nombran lo retirado (D-S15; solo comentarios).',
  '- y los cuatro `cref` y el comentario que nombran lo retirado (D-S15; solo comentarios), y los comentarios que describían como presentes la corrida, el PREPARE y la composición para escribir del piloto (CA-T4d M-05 a; solo comentarios).')
E('d10.md', '`pbt4d-green-v52.txt`; el anexo V52-A', '`pbt4d-green-v53.txt`; el anexo V53-A')
E('d10.md', '`pbt4d-retire-list-229.tsv`), medida', '`@@LIST_FILE@@`), medida')
E('d10.md', '- **se sustituyen con registro** las 27 de la corrida genérica que cubren ramas que nada más cubre (sobre `ExecuteFull`, en `PushBackRetirePilotRunTests`), y las tres que caen al ejecutar (`Skip` y equivalente, D-S12);',
  '- **se sustituyen con registro** las 29 de WPUR que cubren líneas o ramas de código que se queda que nada más cubre (sobre el modelo completo, en `PushBackRetirePilotRunTests`): 27 de la corrida genérica, sobre `ExecuteFull`, y las n.º 48 y 49, de `PushBackPilotSystemRebuild` (CA-T4d I-01); y las tres que caen al ejecutar (`Skip` y equivalente, D-S12);')
E('d10.md', '- **se reescriben con registro** las pruebas que también prueban código que se queda (CT-PBT3B-05, 06 y 09,',
  '- **se reescriben con registro** las pruebas que también prueban código que se queda (CT-PBT3B-05, 06, con su corpus (iii) sobre el modelo completo (CA-T4d M-01), y 09,')
E('d10.md', '- **se borran con registro** las demás: lo que solo fija lo retirado o pasa en vacío, las 55 originales saltadas desde PB-T4b y sus ayudantes, la `Theory` de UI de SUS-01 y lo que deja de compilar con la retirada B;',
  '- **se borran con registro** las demás: lo que solo fija lo retirado o pasa en vacío, las 55 originales saltadas desde PB-T4b y sus ayudantes, la `Theory` de UI de SUS-01, lo que deja de compilar con la retirada B y, para que no quede código muerto en `tests/`, los ayudantes que la retirada deja sin uso y las tres clases que deja sin pruebas (CA-T4d M-03);')

# ---------------------------------------------------------------- §4.4
E('s44.md', '### 4.4 PB-T4d (V52-01 a V52-04)', '### 4.4 PB-T4d (V52-01 a V52-04; V53-01 a V53-04)')
E('s44.md', 'dos ensayos (`/d/r6l` y `/d/r6n`) y la precondición 6 de la v5.2 (§6.4.1).',
  'dos ensayos (`/d/r6l` y `/d/r6n`), y la precondición 6 de la v5.2 y, tras la comprobación acotada CA-T4d, la de la v5.3 (§6.4.1), con sus ensayos (`/d/r6v` y `/d/r6x`).')
E('s44.md', '**La lista** (`pbt4d-retire-list-229.tsv`, sha256 `@@LIST_SHA@@`), por grupo y acción:', '**La lista** (`@@LIST_FILE@@`, sha256 `@@LIST_SHA@@`), por grupo y acción:')
E('s44.md', '`ejecucion`, las tres que caen al ejecutar (D-S12); M-04 (D-S13); `lenta` (D-S14); SUS-01.',
  '`ejecucion`, las tres que caen al ejecutar (D-S12); M-04 (D-S13); `lenta` (D-S14); SUS-01; `M-03`, los ayudantes que la retirada deja sin uso y las tres clases que deja sin pruebas (CA-T4d M-03); `ayudante-editado`, los ayudantes que el RED edita sin fila propia (CA-T4d M-04 b).')
E('s44.md', '«acotar con registro» y «se queda» (sin tocar; no se emite).',
  '«acotar con registro» y «se queda» (sin tocar; no se emite). **Desde la v5.3** (CA-T4d M-04), dos columnas más: `declaraciones-6c2820ba`, todas las declaraciones del nombre en su archivo (las sobrecargas), y `tramo-editado`, las líneas de la base que el RED cambia dentro de ellas, medidas miembro a miembro contra el árbol final del RED; `make_list_v53.py` comprueba que toda línea de código que el RED cambia cae en alguna fila. Los porqué de C-7 dicen lo que se hace (borrar), no «sustituir».')
E('s44.md', '- **las 48 de WPUR** recorrían 436 líneas de la corrida genérica que se queda; el resto de la suite cubría 401. **27 sustitutas** cubren lo demás, línea a línea y rama por rama (`owner-pbt4d-deadcode-27-decisions.md`, pregunta 2); las otras 21 se borran;',
  '- **las 48 de WPUR** recorrían 436 líneas de la corrida genérica que se queda; el resto de la suite cubría 401. **27 sustitutas** cubren lo demás, línea a línea y rama por rama (`owner-pbt4d-deadcode-27-decisions.md`, pregunta 2); **y la 48 y la 49** no prueban la corrida, sino `PushBackPilotSystemRebuild`, que se queda (lo usan `DesiredNamesOf` y el escritor real de «Actualizar»), y son las únicas que cubren su línea 32 y 6 ramas (CA-T4d I-01): **29 sustitutas** (`owner-pbt4d-cat4d-decisions.md`, pregunta 1); las otras 19 se borran, y una se queda en WPUR;')
E('s44.md', '**Lo que la medida no ve,** declarado: la cobertura se compara en las líneas que WPUR ejercitaba en la base (las 436) y en sus ramas; otras líneas de `Pilot/` que solo cubría lo retirado no son de la corrida que se queda.',
  '**La cobertura, ampliada** (CA-T4d I-02; `owner-pbt4d-cat4d-decisions.md`, pregunta 2): la precondición 6 compara la cobertura de **toda la suite de Core** y de la **unión de los filtros de UI** de PB-T4c y de PB-T4d, en la base sin cambios y en el GREEN, sobre **todo el código de `src/` que se queda** (Domain, Application y UI, instrumentados con coverlet en JSON), línea a línea y rama por rama (`cov_p6_v53.py`). Lo que solo cubre la UI se mide con la cobertura de sus filtros antes de darlo por perdido. **La regla del Owner:** todo camino que se queda y pierde su única prueba recibe una sustituta sobre el modelo completo, sin volver a él; vuelve solo si una sustituta no se puede escribir o si salen más de 10 sustitutas nuevas sobre las 29. Las 436 líneas de la v5.2 y sus 9 ramas se quedan como conjunto vigilado. La UI de cada rama es la unión de tres corridas de la unión de los filtros y de cada filtro en su proceso: un binding de WPF da golpes que dependen de lo que corrió antes (`noise-batch/`; R-T4-38). **Lo medido en la v5.3:** @@COV_SUMMARY@@. **Bajan** (informativo; D-S16: lo que cubren más corridas en la base que en el GREEN, por componente emparejado; no cambia el PASA): @@BAJAN@@.\n\n'
  '**Lo que la medida no ve,** declarado: la cobertura de datos (que un corpus pase entero por un camino que ya está cubierto línea a línea; por eso M-01 se resolvió por lectura, no por la medida); el Plugin, que no carga ninguna prueba; y la UI fuera de los filtros, que no cambia (el RED solo quita pruebas de UI dentro de ellos y el GREEN no toca la UI); la CI completa de UI lo cubre (C06/C16).')

# ---------------------------------------------------------------- §5.6 (las CT y la clase de las sustitutas)
E('s56.md', '### 5.6 PB-T4d: la lista cerrada CT-PBT4D-01 y 02 (entera; V52-04)', '### 5.6 PB-T4d: la lista cerrada CT-PBT4D-01 y 02 (entera; V52-04; V53-01)')
E('s56.md', '**De dónde sale cada línea:** de su clase, literal del paquete (anexo V52-A).', '**De dónde sale cada línea:** de su clase, literal del paquete (anexo V53-A; sin cambio desde la v5.2).')
E('s56.md', '(`make_fullcopy_eq_v52.py`); sus líneas esperadas son las del original, las mismas 53;',
  '(`make_fullcopy_eq_v52.py`, sobre los textos finales del RED de la v5.3); sus líneas esperadas son las del original, las mismas 53;')
E('s56.md', '- **`PushBackRetirePilotRunTests`** (`owner-pbt4d-run-coverage-decision.md`; `owner-pbt4d-deadcode-27-decisions.md`, pregunta 2): las 27 sustitutas de `WorkspacePilotUpdateRunTests` sobre `ExecuteFull`, con el ayudante `FullUpdateSetup` (`make_run_eq_v52.py`): 25 por el mapa de PB-T4b; a mano, la n.º 10 (el `Invalid` de PREPARE dentro de la corrida es la frontal B de un rack sin lado B) y la n.º 43 (la lectura y la comparación de POST del modelo completo).',
  '- **`PushBackRetirePilotRunTests`** (`owner-pbt4d-run-coverage-decision.md`; `owner-pbt4d-deadcode-27-decisions.md`, pregunta 2; `owner-pbt4d-cat4d-decisions.md`, pregunta 1): las 29 sustitutas de `WorkspacePilotUpdateRunTests` sobre el modelo completo, con el ayudante `FullUpdateSetup` (`v53/make_run_eq_v53.py`): 27 de la corrida genérica, sobre `ExecuteFull`, y la n.º 48 y la n.º 49, de `PushBackPilotSystemRebuild` sobre el plan del PREPARE completo (CA-T4d I-01). 26 por el mapa de PB-T4b (la n.º 48, con una nota); a mano, la n.º 10 (el `Invalid` de PREPARE dentro de la corrida es la frontal B de un rack sin lado B), la n.º 43 (la lectura y la comparación de POST del modelo completo) y la n.º 49 (solo el comentario del lector). @@RUN_RESULTS@@ resultados (la n.º 42 es una `Theory` de 3).')
E('s56.md', 'Las tres pasan en las dos ramas: el RED ya lleva los textos de `tests/` que miden, y `PrepareFull`, `PushBackFullPrepareInput`, su fábrica y `ExecuteFull` no cambian en el GREEN.',
  'Las tres pasan en las dos ramas: el RED ya lleva los textos de `tests/` que miden, y `PrepareFull`, `PushBackFullPrepareInput`, su fábrica, `ExecuteFull` y `PushBackPilotSystemRebuild` no cambian en el GREEN.')


# ---------------------------------------------------------------- §6.4 (entera)
# los nombres de las piezas y de los guiones que cambian de version (los demas se quedan con su nombre de la v5.2)
for old, new in (('pbt4d-red-v52.txt', 'pbt4d-red-v53.txt'), ('pbt4d-green-v52.txt', 'pbt4d-green-v53.txt'),
                 ('apply_red_pbt4d_v52.py', 'apply_red_pbt4d_v53.py'), ('apply_green_pbt4d_v52.py', 'apply_green_pbt4d_v53.py'),
                 ('SHA_red_pbt4d_v52.py', 'SHA_red_pbt4d_v53.py'), ('SHA_green_pbt4d_v52.py', 'SHA_green_pbt4d_v53.py'),
                 ('`red_pbt4d_v52.py`', '`red_pbt4d_v53.py`'), ('`green_pbt4d_v52.py`', '`green_pbt4d_v53.py`'),
                 ('red_pbt4d_v52.red', 'red_pbt4d_v53.red'), ('green_pbt4d_v52.green', 'green_pbt4d_v53.green'),
                 ('pbt4d_test_edits_v52.py', 'pbt4d_test_edits_v53.py'), ('v52/make_red_v52.py', 'v53/make_red_v53.py'),
                 ('v52/make_run_eq_v52.py', 'v53/make_run_eq_v53.py'), ('v52/make_list_v52.py', 'v53/make_list_v53.py'),
                 ('v52/make_green_v52.py', 'v53/make_green_v53.py'), ('`make_red_v52.py`', '`v53/make_red_v53.py`'),
                 ('`make_run_eq_v52.py`', '`v53/make_run_eq_v53.py`'), ('p6_control_v52.sh', 'p6_control_v53.sh'),
                 ('`required_pbt4d_v52.py`', '`required_pbt4d_v53.py`'), ('anexo V52-A', 'anexo V53-A')):
    E('s64.md', old, new, times=None)
E('s64.md', '### 6.4 PB-T4d — `F4B-PBT4-RETIRE-PILOT` (entero; V52-01 a V52-08)', '### 6.4 PB-T4d — `F4B-PBT4-RETIRE-PILOT` (entero; V52-01 a V52-08; V53-01 a V53-11)')
E('s64.md', 'las 27 de la corrida (`owner-pbt4d-run-coverage-decision.md` y `owner-pbt4d-deadcode-27-decisions.md`, pregunta 2) y la rama muerta de `BuildCore` (`owner-pbt4d-deadcode-27-decisions.md`, pregunta 1): **`pbt4d-retire-list-229.tsv`**, sha256 `@@LIST_SHA@@` (242 filas, 229 con acción y 13 que se quedan sin tocar).',
  'las 29 de WPUR (`owner-pbt4d-run-coverage-decision.md`, `owner-pbt4d-deadcode-27-decisions.md`, pregunta 2, y `owner-pbt4d-cat4d-decisions.md`, pregunta 1), la rama muerta de `BuildCore` (`owner-pbt4d-deadcode-27-decisions.md`, pregunta 1) y lo que la regla del Owner de `owner-pbt4d-cat4d-decisions.md` (pregunta 2) y la sesión añaden tras la comprobación acotada CA-T4d (M-01 y M-03): **`@@LIST_FILE@@`**, sha256 `@@LIST_SHA@@` (@@LIST_ROWS@@ filas, @@N_ACTION@@ con acción y 13 que se quedan sin tocar).')
E('s64.md', '  - **tests/, existentes que el RED edita (26):** @@RED_EDITED@@;', '  - **tests/, existentes que el RED edita (23) o borra (3, bloques DELETE; CA-T4d M-03):** @@RED_EDITED@@;')
E('s64.md', 'las bajas con registro, las ediciones con registro, los 3 `Skip` (D-S12), la prueba lenta acotada (D-S14), los vacíos de M-04 (D-S13), las 27 sustitutas de la corrida y los 5 archivos nuevos.',
  'las bajas con registro, las ediciones con registro, los 3 `Skip` (D-S12), la prueba lenta acotada (D-S14), los vacíos de M-04 (D-S13), las 29 sustitutas de WPUR, el corpus de CT-PBT3B-06 sobre el modelo completo (CA-T4d M-01), los ayudantes sin uso y los 3 archivos sin pruebas que se borran (CA-T4d M-03) y los 5 archivos nuevos.')
E('s64.md', 'escribe exactamente eso, tras comprobar que la copia es de la revisión;', 'escribe exactamente eso (y borra los 3 archivos de los bloques DELETE), tras comprobar que la copia es de la revisión;')
# el Objective (ASCII)
E('s64.md', 'la prueba lenta acotada (D-S14), las 27 sustitutas de la corrida generica sobre ExecuteFull (owner-pbt4d-deadcode-27-decisions.md, pregunta 2) y las dos CT, CT-PBT4D-01 y 02; el GREEN solo toca src/, con el texto literal del paquete, segun el paquete del contrato de PB-T4 v5.2.',
  'la prueba lenta acotada (D-S14), las 29 sustitutas de WorkspacePilotUpdateRunTests sobre el modelo completo (27 de la corrida generica sobre ExecuteFull, owner-pbt4d-deadcode-27-decisions.md, pregunta 2, y 2 de PushBackPilotSystemRebuild, owner-pbt4d-cat4d-decisions.md, pregunta 1), el corpus de CT-PBT3B-06 sobre PrepareFull (CA-T4d M-01), los ayudantes que la retirada deja sin uso y las tres clases que deja sin pruebas, borrados con registro (CA-T4d M-03), y las dos CT, CT-PBT4D-01 y 02; el GREEN solo toca src/, con el texto literal del paquete, segun el paquete del contrato de PB-T4 v5.3.')
E('s64.md', 'y limpiar los cuatro cref y el comentario que nombran lo retirado (D-S15).',
  'y limpiar los cuatro cref y el comentario que nombran lo retirado (D-S15) y los comentarios que describian como presentes la corrida, el PREPARE y la composicion para escribir del piloto (CA-T4d M-05).')
E('s64.md', 'El RED lleva todo lo de tests/ con el texto literal del paquete (D-S11): la lista exacta pbt4d-retire-list-229.tsv,', 'El RED lleva todo lo de tests/ con el texto literal del paquete (D-S11): la lista exacta @@LIST_FILE@@,')
# los invariantes
E('s64.md', 'en tests/ solo los 26 archivos existentes y los 5 nuevos del alcance, con el texto literal de la pieza del RED.',
  'en tests/ solo los 26 archivos existentes (23 que el RED edita y 3 que borra) y los 5 nuevos del alcance, con el texto literal de la pieza del RED.')
E('s64.md', 'INV-PBT4D-03. Lo retirado y lo que se queda: exactamente D-PBT4-10 (v5.2)', 'INV-PBT4D-03. Lo retirado y lo que se queda: exactamente D-PBT4-10 (v5.3)')
E('s64.md', '(la red de section 5, las 27 sustitutas y las ediciones con registro en verde).', '(la red de section 5, las 29 sustitutas y las ediciones con registro en verde).')
E('s64.md', 'INV-PBT4D-05. Las sustitutas y los equivalentes, literales: PushBackRetirePilotRunTests (las 27 de la corrida, sobre ExecuteFull),',
  'INV-PBT4D-05. Las sustitutas y los equivalentes, literales: PushBackRetirePilotRunTests (las 29 de WPUR sobre el modelo completo: 27 de la corrida, sobre ExecuteFull, y 2 de PushBackPilotSystemRebuild),')
E('s64.md', 'INV-PBT4D-06. La cobertura: ninguna de las lineas de la corrida generica y del plan que se quedan, y que la base cubria, queda sin ejecutar en CurrentSha, y ninguna de sus ramas cubiertas en la base queda sin cubrir (medido en la precondicion 6, rama por rama, con coverlet en JSON; owner-pbt4d-deadcode-27-decisions.md, pregunta 2).',
  'INV-PBT4D-06. La cobertura, de todo el codigo de src/ que se queda (CA-T4d I-02; owner-pbt4d-cat4d-decisions.md, pregunta 2): cada linea que la suite completa de Core o los filtros de UI de PB-T4c y de PB-T4d ejecutan en BaseSha, fuera de lo que retira el GREEN literal, se ejecuta en CurrentSha, y cada rama que cubren en BaseSha sigue cubierta en CurrentSha (medido en la precondicion 6, linea a linea y rama por rama, con coverlet en JSON sobre RackCad.Domain, RackCad.Application y RackCad.UI). Todo camino que se queda y pierda su unica prueba tiene una sustituta sobre el modelo completo; las 436 lineas y las 9 ramas de la v5.2 son un conjunto vigilado dentro de este.')
E('s64.md', 'salvo las 229 filas con accion de la lista cerrada (pbt4d-retire-list-229.tsv, sha256 @@LIST_SHA@@),', 'salvo las @@N_ACTION@@ filas con accion de la lista cerrada (@@LIST_FILE@@, sha256 @@LIST_SHA@@),')
# las paradas
E('s64.md', 'No exception outside the closed list of PB-T4d: the 229 rows with an action of pbt4d-retire-list-229.tsv (sha256 @@LIST_SHA@@),',
  'No exception outside the closed list of PB-T4d: the @@N_ACTION@@ rows with an action of @@LIST_FILE@@ (sha256 @@LIST_SHA@@),')
E('s64.md', 'BuildCore loses its parameter allowErase, the condition allowErase of its three branches,',
  'BuildCore loses its parameter allowErase, the condition allowErase of its four conditions (three in the loop and the one of AllObsolete),')
E('s64.md', 'and the four cref and the comment of D-S15 change.', 'and the four cref and the comment of D-S15 and the comments of CA-T4d M-05 change (comments only).')
E('s64.md', 'PBT4D-C16: an existing test outside the closed list of PB-T4d (the 229 rows with an action of pbt4d-retire-list-229.tsv) fails, including one that reads the text or the types in a way only seen when it runs; or a line or a branch of the generic run that stays, covered in BaseSha, is no longer covered. It returns to the Owner (owner-pbt4d-list-decisions.md, question 4; owner-pbt4d-deadcode-27-decisions.md).',
  'PBT4D-C16: an existing test outside the closed list of PB-T4d (the @@N_ACTION@@ rows with an action of @@LIST_FILE@@) fails, including one that reads the text or the types in a way only seen when it runs; or a line or a branch of src/ that stays (RackCad.Domain, RackCad.Application or RackCad.UI), executed or covered in BaseSha by the full Core suite or by the UI filters of PB-T4c and PB-T4d, is no longer executed or covered in CurrentSha. It returns to the Owner (owner-pbt4d-list-decisions.md, question 4; owner-pbt4d-deadcode-27-decisions.md; owner-pbt4d-cat4d-decisions.md, question 2: before the emission, every such path gets a substitute over the full model, and only a substitute that cannot be written, or more than 10 new substitutes over the 29, return to the Owner).')
# la evidencia
E('s64.md', 'owner-pbt4d-run-coverage-decision.md y owner-pbt4d-deadcode-27-decisions.md) y de la sesion (D-S11 a D-S15);',
  'owner-pbt4d-run-coverage-decision.md, owner-pbt4d-deadcode-27-decisions.md y owner-pbt4d-cat4d-decisions.md) y de la sesion (D-S11 a D-S15 y lo que la sesion fijo tras CA-T4d);')
E('s64.md', '[13] la entrada "## PB-T4d" de superseding-characterization.md, escrita por la sesion en el commit de emision: la lista (pbt4d-retire-list-229.tsv, con su sha256)',
  '[13] la entrada "## PB-T4d" de superseding-characterization.md, escrita por la sesion en el commit de emision: la lista (@@LIST_FILE@@, con su sha256)')
E('s64.md', 'anade 3 clases nuevas que no son CT (las 27 sustitutas, el equivalente de CT-PBT4B-20 y las dos guardas de WPPG) y 2 CT de Core, sin ninguna CT de UI.',
  'anade 3 clases nuevas que no son CT (las 29 sustitutas, el equivalente de CT-PBT4B-20 y las dos guardas de WPPG) y 2 CT de Core, sin ninguna CT de UI, y borra 3 archivos de UI sin pruebas.')
E('s64.md', 'el sha256 de cada uno de los cinco archivos nuevos (iguales a NewFileSha) y de cada uno de los 26 archivos existentes de tests/ en RedSha (iguales a los de la pieza del RED),',
  'el sha256 de cada uno de los cinco archivos nuevos (iguales a NewFileSha) y de cada uno de los 23 archivos existentes de tests/ que el RED edita en RedSha (iguales a los de la pieza del RED), y la ausencia en RedSha de los 3 que borra,')
E('s64.md', 'sobre RedSha, el texto de cada archivo de tests/ del alcance es el que da red_pbt4d_v53.red sobre BaseSha (apply_red_pbt4d_v53.py),',
  'sobre RedSha, el texto de cada archivo de tests/ del alcance es el que da red_pbt4d_v53.red sobre BaseSha, y los 3 de los bloques DELETE no estan (red_pbt4d_v53.red_full; apply_red_pbt4d_v53.py),')
# las bajas y la precondicion 6
E('s64.md', '**Las bajas con registro:** las filas «borrar con registro» y «sustituir con registro» de la lista, que la pieza del RED quita (`v53/make_red_v53.py` con `retire_pbt4d_v52.py` en seco: 193 bajas de las 201 necesarias de `/d/r6h`, menos las 17 que se quedan editadas, más las 9 que compilan y se quitan).',
  '**Las bajas con registro:** las filas «borrar con registro» y «sustituir con registro» de la lista, que la pieza del RED quita (`v53/make_red_v53.py` con `retire_pbt4d_v52.py` en seco: 193 bajas de las 201 necesarias de `/d/r6h`, menos las 17 que se quedan editadas, más las 9 que compilan y se quitan; y, desde la v5.3, los @@N_UNUSED@@ ayudantes sin uso de M-03, que quita `v53/unused_v53.py` tras las ediciones, y los 3 archivos sin pruebas, en bloques DELETE).')
E('s64.md', '4. **el RED exacto:** la suite completa de Core, donde **solo cae CT-PBT4D-01, por aserción** (las 2 CT, los 3 `Skip` con su motivo y las tres clases nuevas en verde), y los 18 filtros de UI heredados, **0 `Failed`**;',
  '4. **el RED exacto:** la suite completa de Core, donde **solo cae CT-PBT4D-01, por aserción** (las 2 CT, los 3 `Skip` con su motivo y las tres clases nuevas en verde), y **los 18 filtros de UI del contrato** (los de `RequiredTests`, `ui_filters_pbt4d_v53.txt`; CA-T4d M-07 b), **0 `Failed`**. **La copia principal corre sola** (CA-T4d M-07 a): el conductor comprueba al empezar que no hay otro `dotnet` ni `testhost`;')
E('s64.md', '7. **la cobertura de la corrida genérica que se queda, rama por rama** (`owner-pbt4d-deadcode-27-decisions.md`, pregunta 2): la suite completa de Core sobre el GREEN con coverlet en JSON (`cov_v52b.runsettings`), frente a la misma sobre la base sin cambios (la referencia), con `cov_p6_v52.py`: cada línea de las 436 que WPUR ejercitaba en la base (`/d/r6j`), llevada al GREEN, se ejecuta, y cada rama cubierta en la base sigue cubierta; las de texto cambiado (la rama muerta) y las retiradas, listadas. **Una línea o una rama sin cubrir vuelve a la sesión;**',
  '7. **la cobertura ampliada de todo `src/` que se queda, línea a línea y rama por rama** (CA-T4d I-02; `owner-pbt4d-cat4d-decisions.md`, pregunta 2): la suite completa de Core y la unión de los filtros de UI de PB-T4c y de PB-T4d (`ui_union_v53.txt`), con coverlet en JSON sobre Domain, Application y UI (`cov_v53.runsettings`), en el GREEN de la copia principal y en la base sin cambios, sola, en otra copia (`p6d_basecov_v53.sh` y `p6d_uirep_v53.sh`; la referencia); la UI de cada rama, la unión de tres corridas de la unión de los filtros y de cada filtro por separado, en su proceso, como los corre el contrato (`p6d_uirep_v53.sh` y `p6d_uifilt_v53.sh`; los golpes de un binding de WPF dependen de lo que corrió antes en el mismo proceso: `noise-batch/`, R-T4-38); `cov_p6_v53.py` lleva cada línea de la base al GREEN (por texto en los archivos que el GREEN cambia) y exige que cada línea ejecutada en la base (Core o UI) siga ejecutada y cada rama cubierta siga cubierta; las de texto cambiado, ejecutadas y con todas sus ramas; las retiradas, listadas; lo que Core sola pierde y cubre la UI, listado aparte; y el conjunto vigilado de la v5.2 (las 436 líneas y las 9 ramas). **Un camino que se queda sin su única prueba** recibe una sustituta sobre el modelo completo (la regla del Owner); **vuelve al Owner** si una no se puede escribir o si salen más de 10 nuevas sobre las 29;')
E('s64.md', '9. los `RequiredTests` y los totales (`required_pbt4d_v53.py` y `ui_totals_v52.py`),',
  '9. los `RequiredTests` y los totales (`required_pbt4d_v53.py`, con la base de UI en la corrida de la unión, y `ui_totals_v52.py`),')
E('s64.md', '**Lo medido en la v5.2** (V52-06): @@MEASURED@@', '**Lo medido en la v5.3** (V53-06; la v5.2, historia): @@MEASURED@@')
E('s64.md', '| **La pieza del RED** | `pbt4/pbt4d-red-v53.txt` | sha256 `@@RED_SHA@@`. Bloques EDIT (un OLD que está exactamente una vez en el texto que se va editando) y NEWFILE, con `\\n`;',
  '| **La pieza del RED** | `pbt4/pbt4d-red-v53.txt` | sha256 `@@RED_SHA@@`. Bloques EDIT (un OLD que está exactamente una vez en el texto que se va editando), DELETE (desde la v5.3) y NEWFILE, con `\\n`;')
E('s64.md', 'con `pbt4d_test_edits_v53.py` (`@@SHA_pbt4d_test_edits_v53.py@@`), `v53/make_run_eq_v53.py` (`@@SHA_v53/make_run_eq_v53.py@@`) y `v52/make_fullcopy_eq_v52.py` (`@@SHA_v52/make_fullcopy_eq_v52.py@@`) |',
  'con `pbt4d_test_edits_v53.py` (`@@SHA_pbt4d_test_edits_v53.py@@`), `v53/make_run_eq_v53.py` (`@@SHA_v53/make_run_eq_v53.py@@`), `v53/unused_v53.py` (`@@SHA_v53/unused_v53.py@@`) y `v52/make_fullcopy_eq_v52.py` (`@@SHA_v52/make_fullcopy_eq_v52.py@@`) |')
E('s64.md', 'La genera `v53/make_green_v53.py` (`@@SHA_v53/make_green_v53.py@@`) desde la retirada B medida en `/d/r6e`, con la limpieza de D-S15 y la rama muerta de `BuildCore` |',
  'La genera `v53/make_green_v53.py` (`@@SHA_v53/make_green_v53.py@@`): la pieza de la v5.2 (de `v52/make_green_v52.py`, desde la retirada B medida en `/d/r6e`, con la limpieza de D-S15 y la rama muerta de `BuildCore`), sin cambiar ninguno de sus bloques, y 11 bloques de solo comentarios de CA-T4d M-05 (a) |')
E('s64.md', '| **La lista** | `pbt4/pbt4d-retire-list-229.tsv` | sha256 `@@LIST_SHA@@`; la genera `v53/make_list_v53.py` (`@@SHA_v53/make_list_v53.py@@`): una fila por baja, edición o sustitución, con su grupo, archivo, miembro, líneas en `6c2820ba`, `Skip`, acción y porqué |',
  '| **La lista** | `pbt4/@@LIST_FILE@@` | sha256 `@@LIST_SHA@@`; la genera `v53/make_list_v53.py` (`@@SHA_v53/make_list_v53.py@@`): una fila por baja, edición o sustitución, con su grupo, archivo, miembro, líneas en `6c2820ba`, `Skip`, acción y porqué, y, desde la v5.3, sus declaraciones y su tramo editado (CA-T4d M-04) |')
E('s64.md', '`SuiteDurationSeconds`, `SlowTestSeconds`, `BaseCoverage` y `Coverage`,', '`SuiteDurationSeconds`, `SlowTestSeconds`, `BaseCoverage` y `Coverage` (la ampliada, CA-T4d I-02), `GreenUiCoverage`, `Alone` (la copia principal, sola),')
E('s64.md', '- **Se añaden:** `owner-pbt4d-list-decisions.md`, `owner-pbt4d-substitutes-decision.md`, `owner-pbt4d-run-coverage-decision.md`, `owner-pbt4d-deadcode-27-decisions.md` y `session-pbt4d-design-decisions.md` (D-S11 a D-S15); esta versión del paquete (v5.2);',
  '- **Se añaden:** `owner-pbt4d-list-decisions.md`, `owner-pbt4d-substitutes-decision.md`, `owner-pbt4d-run-coverage-decision.md`, `owner-pbt4d-deadcode-27-decisions.md`, `owner-pbt4d-cat4d-decisions.md` y `session-pbt4d-design-decisions.md` (D-S11 a D-S16); la comprobación acotada de la v5.2 (`independent-review-pbt4d-check.md`); esta versión del paquete (v5.3);')


def apply(name, text):
    for old, new, times in EDITS[name]:
        n = text.count(old)
        assert times is None or n == times, (name, old[:100], n, times)
        text = text.replace(old, new)
    return text


for name, path in SRC.items():
    text = open(path, encoding='utf-8').read()
    text = apply(name, text)
    with open(os.path.join(HERE, 'frag', name), 'w', encoding='utf-8', newline='\n') as f:
        f.write(text)
    print(name, len(EDITS[name]), 'cambios')
