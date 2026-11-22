# PB-T4d: traspaso del generador y del guion de emisión, desde los de PB-T4c

**Para quién:** el agente que escriba `make_pbt4d_contract.py` e `issue_pbt4d.py` para PB-T4d, la retirada del piloto, sin leer el historial de PB-T4c. Todo está en `hs65\pbt4\`, salvo lo que se indica.

**Lo que sabes de PB-T4d:**
- **La base:** el cierre de PB-T4c, `6c2820ba` (F4B-E55, la evidencia 285).
- **Las decisiones del Owner:** el alcance B, con sustitutas donde hace falta y emisión condicional (`owner-pbt4d-list-decisions.md` y `owner-pbt4d-substitutes-decision.md`).
- **El paquete:** lo escribe otro autor, en la v5.2.
- **Este traspaso no decide nada de PB-T4d:** dice cómo están hechos los de PB-T4c y qué hay que mirar al portarlos. Lo que no fije el paquete, el Owner o la sesión, **se pregunta a la sesión**.

## 1. Las reglas de trabajo

- **Solo por objetos git,** desde el checkout principal: `git -C <checkout principal> show <sha>:<ruta>`.
  - El worktree de la rama comparte el almacén de objetos, así que los commits de la sesión (X y BASE) se leen sin `git fetch`.
  - **Nada se lee ni se escribe en el worktree.** El `--dry` y el `--commit` de la emisión los corre la sesión.
  - **Ninguna orden de git que escriba.**
- **Python:**
  - siempre escrito con Write y ejecutado con `python -I <archivo>`. Nunca `-c`, nunca `python -`, nunca un heredoc;
  - lo que importa `yaml` (los modos `--dry`/`--commit` del guion) va sin `-I`.
- **Las salidas:**
  - la salida estándar redirigida sale en cp1252, así que cada guion escribe sus salidas él mismo, en UTF-8 con LF;
  - **se escribe solo en `hs65\pbt4\`;** las carpetas `pbt4acc` a `pbt4dcc` son de la sesión y solo se leen;
  - **tus registros de trabajo,** en una carpeta de fragmentos que la custodia salte (en PB-T4c, `_t4c\`).
- **ASCII y repositorio público:**
  - el generador y los guiones que se custodian van en ASCII puro;
  - los textos custodiados no llevan rutas locales, ni rutas con unidad, ni nombres de clientes.
- **Las clasificaciones y las decisiones:** ninguna se decide por analogía sin que la sesión la confirme.

## 2. Lo que hay de PB-T4c (para copiar y adaptar)

| Pieza | sha256 | Qué es |
|---|---|---|
| `make_pbt4c_contract.py` | `1e933233c54769a96bb646d0ff823309910016fbede63486de0f6ebfe7cc5f3a` | el generador, armado desde `_t4c\`; el redactado va en BASE `d5026014`, en `F4B-PBT4-EDIT-ALL-WINDOW/coordinator/` |
| `_t4c\build_t4c.py` | `951e70268e6ae4266ad9b142d5639721165630f041b5155895bf666b5daf030a` | el armado |
| `issue_pbt4c.py` | `e6327945a2911c0a6cbe63b5b4625ded545d2ea24f34e9d1410bac4770e4e6b2` | el guion de la emisión: fase 1, fase 2 y finish |
| `issue_pbt4c_finish.py` | `22903fc644d3e2cf94ed48efdb6539a23a46f07d70919851c71b3039fa503189` | solo la BASE, si se para |
| `_neg\run_neg_pbt4c.py` | `aee1bbe5cd7a52fecb5d654c1ff61e95d5d4c2613a3d454f7dce8ca368195a0a` | las negativas, 8/8 |
| `f4b-e54.md`, `evidence-283.md`, `evidence-284.md` y `superseding-pbt4c.md` | — | las cuatro plantillas, con marcadores `{NOMBRE}` |

La emisión de PB-T4c salió limpia a la primera: X `a8e9cbe6`, BASE `d5026014`, contrato `8C7265E1…CEE7F`. El contrato emitido es el del seco salvo `AuthorityRevision`, `MainSha` e `IssuedUtc`.

## 3. El generador

### 3.1 El armado

`build_t4c.py` concatena, en este orden:
1. **`c1_head`:** el docstring, las constantes y los argumentos;
2. **el bloque común `git…prefix`** de `hs65\pbt3\make_pbt3a_contract.py`;
3. **`c2_base`:** `soft()` y `pending()`, el anclaje, la numeración, la CI de la base y el contrato anterior;
4. **el bloque común `snapk_check…q3_check`;**
5. **`c3_nc`:** la Snapshot K, los sustitutos de nc1 y nc2 con `--task PBT4`, y C11 desde la del contrato anterior;
6. **los bloques comunes `cover` y el de `count_tests_git.py`;**
7. **`c4_list`:** `tests_of` y la lista, por su sha256;
8. **`c6_req`:** los filtros heredados, con los `MinSelected` que dejan los `Skip`;
9. **`c7_pkg`:** el paquete por su sha256, sus secciones y las abreviaturas;
10. **`c8_v50`:** el bloque de §6.3.1, con `TaskId`, `Objective`, `Invariants`, `StopConditions`, `ExpectedEvidence`, `AllowedWriteScope`, `ForbiddenWriteScope`, `RequiredTests`, la CI calculada y las piezas;
11. **`c9_build`:** la precondición 6, las autoridades y el contrato;
12. **los bloques comunes `bounded_check` y `local_paths`;**
13. **`c99_tail`:** CA, el registro declarado, la salida, y la emisión sin `--dry`.

**Cómo se comprueba:**
- cada bloque común, por su primera y su última línea;
- `make_pbt3a_contract.py`, por el prefijo de su sha256 (`30595efd`);
- el resultado, ASCII y compilado.

**El uso:**

```text
python -I make_pbt4c_contract.py <IssuedUtc> <AuthorityRevision> <MainSha> --dry --objects-rev <rev> --repo <checkout principal> \
    --package-file <paquete> --p6-file <precondicion6.json> --bounded-file independent-review-v1.md --json-out <contrato.json> [--as-write]
```

- **Sin `--dry`,** emite en el worktree sobre X: hace `os.chdir(WT)`, escribe `artifacts/orchestration/I-64/<TASK>/coordinator/gate-contract.json` y lo valida con `Test-Json`. En ese modo, todo lo que en seco es AVISO o PENDIENTE aborta.
- **Para el redactado** hay dos líneas que se sustituyen al custodiarlo: `WT = os.path.expanduser(...)` y `MAIN_CHECKOUT = r'...'` (`gen_redacted()` del guion).

### 3.2 Cómo se decide en seco y al emitir

- **`soft(cond, msg)`:** lo que solo existe con X (F4B-Enn, la lista, las piezas, CA, la entrada del registro declarado). En seco, un AVISO; con `--as-write`, aborta.
- **`pending(msg)`:** lo que espera al paquete, a la precondición 6 o a CA. En seco, un PENDIENTE; **con `--as-write` aborta en el acto,** al llamarla.

### 3.3 Las constantes que se portan

- **El anclaje de la tarea anterior:** sus X y BASE, su RED, su GREEN y su cierre, y su CI base (`CI_BASE_T4B`, que pasa a ser la de PB-T4c).
- **La numeración:** una asignación por constante, para leerlas con `ast`. En PB-T4c eran `EMIT_ENTRY`, `X_EV` y `BASE_EV`. **Compruébala contra la revisión,** como hace el guion: previsiblemente F4B-E56, la 286 y la 287.
- **La CA:** `BOUNDED_RESULT`, `BOUNDED_SECTIONS` y `CAT4C_PKG`.
- **El paquete:** `PKG_VERSION`, `PKG_TARGET` y `CUSTODY_SHA256`.
- **La lista:** `LIST_NAME` y `LIST_SHA`, con la historia de las listas anteriores.
- **Las piezas:** `PIECE_NAMES`.
- **Las cifras:** `N_REQ_ROWS`, `N_CT` y la tabla de totales (`CI_T4C_EXPECTED`).
- **La precondición 6:** `P6_REL_NAME`, `P6_SCHEMA` y los campos que no lee (§6).

**Lo que lleva escrito por posición** en el contrato anterior (`c9_build`), y hay que rehacer:
- **El número de autoridades:** se comprueba con `len(_A) == 72`; el contrato de PB-T4c tiene 70.
- **Los índices:** el 0 es el paquete, el 6 el registro declarado, el 28 `f4b-dispositions.md`, el 29 el contrato anterior, el 36 la base, el 37 «este contrato» y el 65 la revisión independiente. En el contrato de PB-T4c, la revisión es la n.º 43.
- **Las que se quitan** son rangos por número.

**Toma los índices de la sección de autoridades del paquete v5.2, no de PB-T4c.**

## 4. El guion de emisión

### 4.1 Los ayudantes

**Los ayudantes de `hs65\pbt1cc\issue_pbt1c.py`** se ejecutan hasta `# --- precondiciones ---`, quitando su `import yaml`:
- `git`, `lf`, `wr` y `sub`;
- `clean`, `src`, `redact`, `red_text` y `red`;
- `NL`, `BS`, `WT`, `A`, `BRANCH` y `PILOT`.

`clean` se envuelve con `FALSE_POS` (§6).

**Del generador:** `gen_const(name)` lee sus constantes por `ast`, y `gen_fn(name)` extrae un bloque marcado, como `local_paths`.

### 4.2 La fase 1 (comprobaciones, por objetos de la base)

1. **La numeración,** frente al generador y frente a la revisión.
2. **Las 8 líneas del estado del cierre anterior:** para PB-T4d, `hs65\pbt4ccc\_close_state_lines.py.txt`. Cada una tiene que estar una sola vez en el estado de la base, en ASCII, y `attempts` tiene que ser 1.
3. **La custodia de `hs65\pbt4\`** (§5).
4. **Las carpetas que van como `cc4b/`,** con su manifiesto (§5).
5. **La redacción de las copias exportadas,** con su autoprueba.

### 4.3 La fase 2

- **`TEXTS_FOR`:** los textos se escribieron para una versión del paquete. Si el generador fija otra, PENDIENTE.
- **B-08:** las cifras que las plantillas llevan, buscadas literales en el paquete.
- **Los registros que el paquete cita por sha256:** el de la sesión y los del Owner, comprobados en su cabecera. En PB-T4b, la v4.8 citaba uno viejo.
- **La fila del registro de cambios** que corrige lo de la CA.
- **`COMPILE_RESULT`:** redactado por el autor del guion, con `COMPILE_RESULT_FOR`, el sha256 del `compile-check.md` sobre el que se escribió. Si cambia, PENDIENTE. Lo confirma la sesión.
- **Las plantillas y su relleno** (`fill`): un valor que falta va como `<pendiente: …>` y abre un PENDIENTE, nunca un hueco silencioso.
- **`verdicts()`:** los dos veredictos que la F4B-Enn declara, sacados de `BOUNDED_RESULT`.
- **La tabla del registro declarado** (`sup_tables`): desde la lista, el módulo de los `Skip`, los equivalentes literales del paquete y `EquivalentSha` de la precondición 6, cruzados.
- **`set_sha_lines`:** lo que queda fuera de la custodia, por el sha256 de su conjunto.
- **El estado:** `SWAPS`, subcadenas del cierre, cada una una sola vez; `WHOLE`, líneas enteras; `next_action_base(X)`.
- **`gen_redacted()`.**
- **`x_texts()`:**
  - la custodia nueva y las carpetas;
  - los cuatro registros que crecen: `f4b-dispositions.md`, `I-64-evidence.md`, `superseding-characterization.md` y el estado, con 7 líneas cambiadas;
  - `replaced`: lo que se vuelve a custodiar en su ruta, el paquete e `independent-review-v1.md`;
  - `x_paths_check`.
- **`ev284()`, `MSG_X` y `msg_base()`,** con el trailer `Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>`.
- **`commit_files()`:** usa `git add --pathspec-from-file`, porque la línea de órdenes de Windows no admite miles de rutas. Escribe `_pathspec.txt` y `_msg.txt` en `hs65\pbt4\`.
- **`dry_counts()`:** lee el seco custodiado.
  - Saca los recuentos, los PENDIENTES (tienen que ser 0), la versión, el sha256 de la precondición 6 y la línea `BOUNDED_RESULT … sin errores … secciones …`.
  - Comprueba que el sha256 del contrato en seco es el del JSON.
- **`contract_on_x()`:** el generador con `--dry --as-write --objects-rev X`. Su contrato tiene que ser el del seco salvo las tres claves.

### 4.4 Los modos

- **`--selftest` y `--dry-objects`:** por objetos de la base. Solo escriben su salida y `_review_t4c/`, los textos rellenos para la sesión.
  - **No borres la carpeta:** allí vive `authority-sections.txt`. Quita solo tus archivos.
  - Si existe `X_T4C`, comprueban X por objetos (`x_check`) y la BASE.
- **`--dry` y `--commit`:** en el worktree, los corre la sesión.
  - `--commit` hace y sube X, ejecuta `contract_on_x` sobre X **antes** de emitir, emite, compara byte a byte, hace y sube la BASE.
  - Escribe `authority-rev.txt` y `base-sha.txt` en la carpeta de la sesión.
- **El modo finish** (`--finish-dry` y `--finish-commit`, por `issue_pbt4c_finish.py`): solo la BASE sobre un X ya subido.
  - Exige `X_T4C`, `DRY_LISTING` (la salida del `--dry` de la sesión, `hs65\_session_t4c\issue-dry.txt`) y `GEN_NOTE_FINISH`, la línea de la evidencia que declara por qué se paró. La confirma la sesión.
  - En PB-T4c no hizo falta; en PB-T4b, sí.

## 5. La custodia de `hs65\pbt4\`

**El recorrido.** Cada archivo, frente a lo ya custodiado en la base, es **igual** (byte a byte o tras la redacción), **cambiado**, **nuevo** (va a `HIST_C`) o **sin clasificar**. Lo cambiado y lo sin clasificar son PENDIENTE.
- **Lo que no se custodia** (`CUSTODY_SKIP`) es el literal de `issue_pbt4b.py`, leído por `ast`, más lo de cada tarea:
  - las plantillas;
  - el generador y sus fragmentos;
  - el contrato en seco;
  - los guiones de emisión y sus salidas;
  - `_review_t4c/`, `_neg/`, `__pycache__/` y `_ref_*`;
  - las carpetas `copies_probe_vNN/`.
- **Las reglas de lo nuevo** (`HIST_C_RX`):
  - `vNN/` y los guiones `*_vNN*.py`, `make_vNN.py` y `*-vNN*.txt/md/tsv`;
  - las dudas y el traspaso del generador;
  - el encargo de la CA y su guion;
  - las decisiones del Owner, que van a `coordinator/`, y la de la sesión, a `pbt4/`;
  - las piezas `pbt4c-*`;
  - los guiones del revisor, `ir4_*` salvo `ir4_measure*`.

  **Las reglas de PB-T4d (`pbt4d-*`, `v52/`…) las confirma la sesión.**
- **Byte a byte** (`BYTE_EXACT_C`): las listas, las piezas de texto y las sondas `.cs`, porque el paquete las cita por su sha256. Tienen que estar sin nada que redactar.
- **El paquete:** la versión del generador va a `docs/initiatives/I-64-f4b-pbt4-gate-contract-package.md`, y las anteriores que aún no están, a `F4B-DRAFTS/pbt4/package-vNN.md` (`PKG_ORDER` y `DST`).
- **`independent-review-v1.md`** se vuelve a custodiar si lo de la base es su principio, con su sustitución local (`LOCAL_FIX`, los worktrees de Codex como marcador) y su nota.
- **La versión en curso:** mientras el autor escribe, sus archivos quedan «en curso» y son PENDIENTE. En PB-T4c, `V51_READY` y `V51_RX`; se cambia cuando la sesión avisa.
- **Las carpetas que van como `cc4b/`** (`custody_dir_like_cc4b`): la precondición 6 de la sesión (en PB-T4c, `pbt4ccc\compile\` a `F4B-DRAFTS/pbt4/cc4c/`), la medida de D-S9 y la sonda de la octava tanda.
  - **Los textos:** en UTF-8 con LF, con los CR quitados y su nota, y la copia como `<copia>`.
  - **Solo por sha256:** las TRX, los binarios, el bytecode y, según la carpeta, los `.log`. Primero se mira el tipo y después la ruta.
  - **Un manifiesto por carpeta,** también con los archivos de fuera (`extra`, con su `Source`).
  - **El resumen JSON** va byte a byte a `F4B-DRAFTS/pbt4/<precondition6>.json`, la ruta que lee el generador.
  - **El nombre de la carpeta de PB-T4d** lo decide la sesión.

## 6. Las lecciones de PB-T4b y PB-T4c

1. **El pendiente que se cerraba tarde.**
   - **Lo que pasó en PB-T4b:** la BASE de PB-T4b se paró. El generador abría el pendiente de las `Theory` de `MemberData` antes de leer la precondición 6 de la revisión, y al emitir `pending()` aborta en el acto.
   - **Lo que estaba por pasar en PB-T4c:** el fragmento `c6_req` abría el pendiente del n.º 43 y `c8` lo quitaba después de la lista. Al emitir habría abortado.
   - **La regla:** no se abre ningún pendiente que una sección posterior decida. Todo lo que depende del paquete, de la precondición 6 o de la CA se decide después de leerlos.
   - **La prueba es W00** (§7), y corre antes de que exista X.
2. **El cálculo que decía «iguales» sin comparar.** El RED de UI de PB-T4c no contaba el equivalente con `ExpectRed`.
   - Toda cifra calculada se compara con la tabla del generador y con el texto literal de [14] del paquete, por expresión.
   - Si no casa, es un error, nunca «iguales». Lo prueban E01 y E02.
3. **Los equivalentes con `ExpectRed`.** El equivalente de UI de CT-PBT2B-06 cae en el RED, porque el esqueleto no cambia la ventana.
   - Cuenta en los `Failed` del RED: el generador los suma desde los filtros con `ExpectRed` de las clases equivalentes.
   - Se comprueba el reparto: 4 filtros con `ExpectRed`, 2 de Core, con 14 resultados. Lo prueba E03.
4. **La n.º 37.**
   - **El fallo de PB-T4b:** en su contrato, la n.º 37, con la sección «este contrato», apuntaba al contrato de PB-T4a.
   - **La corrección de PB-T4c:** su propio contrato.
   - **Lo que eso rompía:** ese contrato no está en X, porque lo escribe la BASE, y al emitir contaba como autoridad que falta. Se excluye de `AUTH_MISSING`, comprobando que es «este contrato» y que está una sola vez.
   - **En PB-T4d:** compruébalo por la sección «este contrato», no solo por el índice.
5. **MAX_PATH a 156** (decisión de la sesión): ninguna ruta custodiada en X pasa de 156 caracteres relativos.
   - **Lo que pasa de 156:** va solo por sha256, con el motivo «ruta > 156 (MAX_PATH): solo por SHA-256 (decision de la sesion)» en su manifiesto.
   - **El resultado de su clase:** `compile-check.md` tiene que darlo en una fila de sus tablas; si no, PENDIENTE.
   - **En X:** `x_paths_check` aborta con cualquier ruta de más de 156. Lo prueba X01.
   - **Pide al autor nombres cortos de salida:** en la v5.1, ninguno pasó de 150 con el prefijo de `cc4c/`.
6. **`FALSE_POS`.** La redacción común rechaza un nombre de cliente de cuatro letras que aparece dentro de dos identificadores. `FALSE_POS` es una lista cerrada y nombrada:
   - la de DU-28, una opción de la línea de órdenes;
   - la de PB-T4c, la clase `RackCad.Tests.Persistence…ValidationTests` de las tablas por clase.

   **Cada falso positivo nuevo lo decide la sesión,** como un nombre más de la lista, sin ampliar el patrón. Un rechazo de la redacción es PENDIENTE: no se arregla en silencio.
7. **Los campos que no lee.** El JSON de la precondición 6 crece entre versiones.
   - **Lo que el generador declara, por listas** (`P6_UNREAD_V50`, `P6_V51_KEYS` y `P6_UNREAD_V51`):
     - lo que no lee;
     - lo que lee, con sus comprobaciones: el ensayo PASA en las dos ramas, el control «VE I-01», el censo PASA y `ClassResults`, los sha256 de las tablas por clase;
     - lo que decide la sesión: `Checks`, como `Cost` en PB-T4b.
   - **Una clave que el paquete no declara** es PENDIENTE, y la sesión decide.
   - **El guion de emisión** comprueba cada sha256 de `ClassResults` frente a su archivo. Una clave sin su carpeta vale si casa con un solo archivo, y lo dice.
8. **Las rutas con unidad.**
   - **En lo que escribe la emisión, 0:** las plantillas, `COMPILE_RESULT`, los mensajes, el estado y el generador redactado. Se usa la etiqueta de la copia en texto (`r5g`), nunca con la barra de unidad delante.
   - **En lo custodiado,** las etiquetas `/d/r4<letra>` y `/d/r5<letra>` van tal cual, y las rutas con perfil o usuario se redactan.
   - **La redacción de las copias** (`unit_redact`) pasa `<unidad>:\r5<letra>\` a `<copia>\`, con su autoprueba; cualquier otra ruta con unidad aborta.
   - **`RX_BASH_DRIVE`** no cuenta el marcador literal `<letra>`.
   - **Las copias de PB-T4d** se llaman `r6<letra>`: amplía las dos expresiones y su autoprueba, y que lo confirme la sesión.
9. **`BOUNDED_RESULT` se copia del archivo** con un guion, no a mano.
   - Es la línea de la §N.10 más la cola de la recomprobación.
   - Se compara con la línea entera que da el revisor, y se comprueban ASCII y su longitud.
   - Al emitir, el generador exige las dos secciones de la CA.
10. **El encargo de la CA:** `_write_cat4c_prompt.py` lo saca de la transcripción de la sesión, con una sección por ronda. Se rehace cuando llega la recomprobación; ajusta sus marcas a CA-T4d.
11. **El autor sigue escribiendo mientras corres:** los recuentos de `--selftest` y `--dry-objects` pueden variar entre corridas. `pbt4ccc\compile\` creció en cada vuelta.
12. **`--selftest` tarda unos 3 o 4 minutos,** porque hace muchos `git cat-file`. Córrelos uno tras otro, nunca a la vez: escriben la misma carpeta.

## 7. Las negativas (`_neg\run_neg_pbt4c.py`) y cómo se adaptan

Leen la versión, el sha256 del paquete, las piezas y la lista del generador, y trabajan en copias, en `_neg\tmp4c\`, que borran al final.

| Caso | Qué prueba | En PB-T4d |
|---|---|---|
| **C01** | el seco final, con `--p6-file` y `--bounded-file`: su salida igual byte a byte a `dry-pbt4c.txt` y su contrato al JSON | igual |
| **N01** | `--as-write` sobre la base: aborta en el primer AVISO (F4B-Enn) | igual |
| **W00** | como al emitir, con `soft()` sin abortar: ningún `pendiente:`, y se para solo en las autoridades que faltan, todas de `F4B-DRAFTS/` y sin la n.º 37 | igual; es la que caza la lección 1 antes de X |
| **E01** | el generador espera otros totales (mutando `CI_T4C_EXPECTED`): aborta | los totales de PB-T4d, con los borrados |
| **E02** | [14] del paquete con otros totales: aborta | igual |
| **E03** | el `ExpectRed` cambia de filtro: aborta | según el reparto que fije la v5.2 |
| **W01** | cuando exista X (`X_T4C`): `--dry --as-write --objects-rev X`, sin archivos locales; PENDIENTES 0, AVISOS 0 y el contrato del seco salvo las tres claves | igual |
| **X01** | la regla de las rutas de X, sacada de `issue_pbt4c.py` entre sus marcas: 156 pasa, 157 aborta | igual |

## 8. Lo que cambia en una retirada

**La autoridad** es la que fija el paquete v5.2 con las dos decisiones del Owner. Estas son las piezas del generador de PB-T4c que no valen tal cual:

- **El alcance, con `Pilot/` y con borrados en `src/` y en `tests/`:**
  - **Lo que suponía PB-T4c:** comprobaba que ninguna ruta era de `Pilot/` y que cada ruta del alcance existía en la base o era nueva. Lo primero, solo los `UNIT_CHANGE` y los archivos de los `Skip`.
  - **En una retirada** hay rutas que existen en la base y no deben existir después.
  - **Hay que reescribir:**
    - la comprobación `in_head(a) == (…)`;
    - la clase de esas rutas en las autoridades y en el contrato;
    - el `ForbiddenWriteScope`: `cover('src/')` menos el alcance. Si se borra una carpeta entera, el prefijo prohibido que la cubre se parte, como hace `SPLIT2` en `tests/`.
  - **Cómo se declara un borrado en el contrato** lo fija el paquete. Si no lo fija, se pregunta.
- **C02 con su lista de borrados.** En PB-T4c, C02 no admitía ninguna excepción fuera de los `Skip` de la lista de 4, y el generador lo comprobaba por texto. En PB-T4d, C02 nombra también los borrados:
  - las pruebas y los ayudantes borrados con registro;
  - las sustitutas;
  - las que se acotan.

  El generador comprueba que la excepción de C02 es exactamente la de la lista, con su sha256, fila a fila, y que no hay otra.
- **Los totales, menos lo borrado.**
  - **De dónde salen:** los totales de cada rama salen de la base de PB-T4c, menos los resultados de lo borrado, más lo que se añade y menos lo que se salta.
  - **Las `Theory` de `MemberData`** no se cuentan por texto (`count_tests_git.py`). Su número lo mide la precondición 6. No se abre su pendiente antes de leerla: lección 1.
  - **La comprobación:** la cifra se compara con [14] del paquete; nunca «iguales» por defecto.
  - **Los filtros heredados:** los que se quedan sin pruebas (`REQ_ZERO`) los decide el paquete en su tabla, nunca un pendiente abierto antes.
- **Sin esqueleto.**
  - `PIECE_NAMES`, la fila «El esqueleto y el GREEN» de la §6.3.2 que lee `c8` y lo que el RED es en INV-02 cambian: el RED de una retirada no lleva esqueleto.
  - Qué lleva el RED y qué el GREEN lo fija el paquete; con eso cambian también E03 y las plantillas.
- **El registro declarado, «## PB-T4d»:** su tabla la fija la [13] de la v5.2, con lo sustituido, lo borrado y lo acotado.
- **La base:** el cierre de PB-T4c, `6c2820ba`. Su CI y su `task-close-package.md` sustituyen a los de PB-T4b en `c2_base` y en las autoridades.
