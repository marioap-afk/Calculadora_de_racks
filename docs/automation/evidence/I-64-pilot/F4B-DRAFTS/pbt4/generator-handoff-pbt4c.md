> **Redaccion para el repo publico:** las rutas locales y las variables de entorno de carpetas del usuario se sustituyen por marcadores (`<scratchpad>`, `<worktree-de-I-64>`, `<checkout-principal>`, `<datos-locales-del-usuario>`, `<datos-de-aplicacion-del-usuario>`). El resto no cambia.

# PB-T4c: traspaso del generador y de la emisión (para terminar la fase 2)

**Para quién:** el agente que termine la emisión de PB-T4c (`F4B-PBT4-EDIT-ALL-WINDOW`) sin leer el historial. Todo está en `hs65\pbt4\`, salvo lo que se indica. La base es el cierre de PB-T4b, `6adff90d856026a7a51067c1e9d7248f3d81641c` (F4B-E53, la evidencia 282). La emisión será F4B-E54, con la 283 (X) y la 284 (BASE).

## Reglas de trabajo (siguen en vigor)

- **Solo por objetos git,** desde el checkout principal: `git -C "<checkout-principal>" show <sha>:<ruta>`.
- **Nada se lee ni se escribe en el worktree** de la rama (P-08); el `--dry` y el `--commit` los corre la sesión.
- **Sin dotnet y sin pruebas.**
- **Python siempre con `-I`,** en guiones escritos con Write. Nunca `-c`, nunca `python -`, nunca un heredoc. Hubo varios `python -I -c ""` vacíos por costumbre: no los añadas al final de las órdenes.
- **Ninguna orden de git que escriba.**
- **Se escribe solo en `hs65\pbt4\`.** `pbt4acc`, `pbt4bcc` y `pbt4ccc` son de la sesión: solo se leen.
- **El generador es ASCII puro.** Los textos custodiados no llevan rutas locales, porque el repositorio es público.
- **Las respuestas, en español.** No se decide una clasificación por analogía sin que la sesión la confirme.

## El estado

**El generador,** `make_pbt4c_contract.py`, sha256 `9ce285f1166ab1172791d7685ee7cddbeceb2249da55916755e0654e60d9e541`:
- se arma con `python -I _t4c/build_t4c.py`, desde los fragmentos `_t4c/`: `c1_head`, `c2_base`, `c3_nc`, `c4_list`, `c6_req`, `c7_pkg`, `c8_v50`, `c9_build` y `c99_tail`;
- usa los bloques comunes de `hs65/pbt3/make_pbt3a_contract.py`, comprobados por su primera y su última línea: git, snapk/rr/q3, `cover`, `count_static`, `bounded_check` y `local_paths`.
- **Lee la v5.0** (`I-64-f4b-pbt4-gate-contract-package-v50.md`, `d19dd830…cfe7`), con sus §6.3, §6.3.1 y §6.3.2.

**El seco final de la v5.0** (`dry-pbt4c.txt`, `e672515d…5b81`):
- la orden:

  ```text
  python -I make_pbt4c_contract.py 2026-11-21T00:00:00Z 6adff90d… 812baf87… --dry --objects-rev 6adff90d… --repo <checkout principal> --package-file I-64-f4b-pbt4-gate-contract-package-v50.md --p6-file ../pbt4ccc/compile/pbt4c-precondition6.json --json-out contract-dry-pbt4c.json
  ```
- **rc 0, 0 PENDIENTES y 14 AVISOS.** Todos se cumplen con X: F4B-E54, la lista, las 10 piezas, la §22 de CA-T4c y la entrada «## PB-T4c».
- **El contrato** (`contract-dry-pbt4c.json`): 97 538 bytes, sha256 `D56CB3F5…F429`.

  | Campo | Recuento |
  |---|---|
  | AllowedWriteScope | 26 |
  | ForbiddenWriteScope | 510 (`src/` 255, `tests/` 238, raíz 17) |
  | Invariants | 14 |
  | StopConditions | 34 (17 estándar y 17 propias) |
  | RequiredTests | 85 (76 heredados, sin el n.º 43, y 9 nuevos; 4 con `ExpectRed`, 14 resultados) |
  | ExpectedEvidence | 16 |
  | Authorities | 70 (la lista y sus textos de `Section`, en `_review_t4c/authority-sections.txt`) |
- **La CI calculada, que tiene que casar con `CI_T4C_EXPECTED` y con el texto de [14]:**

  | Rama | Core | UI |
  |---|---|---|
  | RED | 19 619 = 19 149 + 9 + 461 | 2 486 = 2 432 + 5 + 49 (las 4 CT y el equivalente de CT-PBT2B-06, con `ExpectRed`) |
  | GREEN | 19 619 = 19 158 + 0 + 461 | 2 486 = 2 437 + 0 + 49 |

**Las negativas,** `_neg/run_neg_pbt4c.py` → `neg-pbt4c.txt`, **5/5:**
- C01, el seco final, comparado byte a byte;
- N01, `--as-write`;
- E01, E02 y E03, un equivalente con `ExpectRed` cuyo cálculo no casa.

**El guion de emisión,** `issue_pbt4c.py`, sha256 `bdace7c79dbdf523e977e6bdb0175f826daf342e3daac8ea0272d8cb3ea395c4`. Es la **fase 1:** solo `--selftest` y `--dry-objects`, por objetos de `6adff90d`, y aborta con `--dry` y `--commit`. Comprueba:
- la numeración frente al generador;
- las 8 líneas del estado del cierre de PB-T4b (`pbt4bcc/_close_state_lines.py.txt`);
- la clasificación de `hs65\pbt4\` frente a lo ya custodiado: el último resultado, 369 iguales, 0 cambiados, 71 nuevos y 8 sin clasificar (los de `v51/`, que el autor sigue escribiendo);
- la custodia de `cc4c/`, de `v50/out-probe-skips-r5a/` y de `r5e`;
- el barrido de rutas con unidad.

**El encargo de CA-T4c:** `_write_cat4c_prompt.py` → `independent-review-pbt4c-check-prompt.md` (`0b35776c…31a4ff`), con 1 ronda. Hay que rehacerlo cuando llegue la recomprobación, con el prefijo «Recomprobación de CA-T4c».

## Lo que falta

1. **La v5.1** (`v51/`, en curso por CA-T4c I-01 de la §22, que dio EMITIBLE TRAS CORRECCIONES 0/1/3: los dos archivos nuevos de `Draft/` llevan `#nullable enable`, y la pureza de CT-15 lo prohíbe). Cuando el autor la termine:
   - en `c1_head.py`: `PKG_VERSION` y `PKG_TARGET` a `v5.1`, y su sha256 en `CUSTODY_SHA256`; revisa `PIECE_NAMES`, porque cambian el esqueleto, el GREEN y sus sha256;
   - `CAT4C_PKG` sigue en v5.0 (la §22 corrió sobre ella), y `BOUNDED_SECTIONS` sigue en ('22', '23');
   - en `issue_pbt4c.py`: añade `('v5.1', 'I-64-f4b-pbt4-gate-contract-package-v51.md')` a `PKG_ORDER`, extiende las reglas `*_v50*` a `v51` y trata `v51/` como `v50/`;
   - la v5.0 pasa a la historia;
   - la precondición 6 se repite: un JSON nuevo, cuyos `Pieces` tienen que casar con los sha256 de la v5.1.
2. **`BOUNDED_RESULT`:** la línea BOUNDED literal de la §22 (y de la §23, con la cola de la recomprobación), comparada con el archivo y no copiada del mensaje. `bounded_check` corre con `--bounded-file independent-review-v1.md`; el reviewer de la fila de registro es «CA-T4c».
3. **La fase 2 de `issue_pbt4c.py`.** Pórtala desde `issue_pbt4b.py`, que hace exactamente esto para PB-T4b:
   - **las plantillas:** `f4b-e54.md`, `evidence-283.md`, `evidence-284.md` y `superseding-pbt4c.md`. Tómalas de `f4b-e52.md`, `evidence-280.md`, `evidence-281.md` y `superseding-pbt4b.md`; mételas en `CUSTODY_SKIP`, como plantillas;
   - **`x_texts()`:**
     - la custodia de HIST_C;
     - el paquete, byte a byte, con su sha256; la v4.9 y las anteriores que falten, a `package-vNN.md`;
     - `independent-review-v1.md`, re-custodiado comprobando que lo de `6adff90d` es su principio;
     - `cc4c/`, `cc4c/r5e/`, `v50/out-probe-skips-r5a/` y el JSON a `F4B-DRAFTS/pbt4/pbt4c-precondition6.json`;
     - los cuatro registros que crecen: `f4b-dispositions.md` con E54, la evidencia con la 283, el estado con 7 líneas y el registro declarado con «## PB-T4c»;
   - **los SWAPS y WHOLE del estado,** desde las 8 líneas del cierre de PB-T4b;
   - **`dry_counts()`,** sobre `dry-pbt4c.txt`, que custodia el seco final;
   - **`gen_redacted()`;**
   - **`commit_files()`** (`git add --pathspec-from-file`) y `with_trailer()`. El trailer de los dos commits es `Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>`, y el `--dry` imprime los dos mensajes;
   - **los modos** `--dry`/`--commit`, con el generador en escritura en el worktree y el contrato emitido igual al del seco salvo `AuthorityRevision`, `MainSha` e `IssuedUtc`;
   - **el barrido `drive_hits`:** 0 en lo que escribe la emisión;
   - **`_review_t4c/`** con los textos rellenos, para la sesión;
   - **el modo finish:** ten preparado `--finish-dry`/`--finish-commit` (`issue_pbt4b_finish.py`), por si la BASE vuelve a pararse.
4. **Lo que F4B-E54 tiene que declarar:**
   - las dos tandas del Owner de PB-T4c (la octava, `owner-pbt4c-list-3-decision.md`, y la novena, `owner-pbt4c-list-4-decision.md`);
   - D-S7 a D-S10 (`session-pbt4c-design-decisions.md`);
   - la vuelta parada de la precondición 6 (las copias `r5a` a `r5e`, con D-S9 en `r5e`);
   - la n.º 37 del contrato de PB-T4b, que apuntaba al contrato de PB-T4a: en PB-T4c es su propio contrato;
   - CA-T4c, §22 y §23, con sus veredictos.
5. **La negativa de escritura sobre X,** cuando exista X: W01, con `--dry --as-write --objects-rev <X>` sin `--package-file`, `--p6-file` ni `--bounded-file`. Tiene que dar PENDIENTES 0, AVISOS 0 y el contrato del seco salvo las tres claves (como W01 de `_neg/run_neg_pbt4b.py`).
6. **El resultado de `FullPanelCopyWorkspacePilotReconciliationViewGreenTests`:** su salida va solo por sha256 por la ruta, y `compile-check.md` no lo da. Lo traerá el `compile-check.md` de la v5.1.

## Las decisiones de la sesión (este hilo)

- **La n.º 37** va en su propio contrato (`F4B-PBT4-EDIT-ALL-WINDOW/coordinator/gate-contract.json`), y se anota en F4B-E54.
- **La precondición 6** va en `cc4c/` (`hs65\pbt4ccc\compile\`): los textos redactados en UTF-8 con LF y su manifiesto; las TRX, los binarios y el bytecode, solo por sha256.
- **MAX_PATH:** un archivo cuya ruta en X, con el worktree de 86 caracteres, pase de 259 va **solo por sha256**, con el motivo «ruta > MAX_PATH». La carpeta sigue siendo `cc4c/`.
- **`r5e` (D-S9):** de `v50/variant-rename/` se custodian solo las salidas de texto de la corrida r5e (el driver, los outcomes y los resúmenes), redactadas, en `cc4c/r5e/`. Sus `.log` van por sha256, y la variante en sí queda fuera por el sha256 de su conjunto.
- **`v50/`:** `eq-tree/`, `eq-tree4/` y `variant-rename/` quedan fuera por el sha256 de su conjunto. `out-probe-skips-r5a/` se custodia como `cc4c/`, con los `.log` y las `.trx` solo por sha256.
- **`r5<letra>`** se redacta como `r4<letra>`: `<letra>:\r5x\` pasa a `<copia>\`, y las etiquetas `/d/r5<letra>` quedan tal cual. Una autoprueba lo comprueba en `issue_pbt4c.py`.
- **Las reglas de custodia:**
  - `ir4_t4c_*` y los demás guiones nuevos del revisor, con la regla de `IR4B_RX`;
  - `make_list_pbt4c_<n>.py`, confirmada;
  - `__pycache__/`, fuera;
  - las decisiones del Owner sobre PB-T4c, a `coordinator/`, y la de la sesión, a `pbt4/`;
  - las listas, el esqueleto, el GREEN y las sondas `pbt4c-*`, byte a byte.
- **Los textos de `Section`:** revisados por la sesión. Las n.º 44 y 45 nombran ya cada una su tanda.

## Las lecciones

- **La BASE de PB-T4b se paró la primera vez** porque un pendiente se abría antes de leer la precondición 6: al emitir, `pending()` aborta en el acto. Todo pendiente que dependa de la precondición 6 o de CA-T4c se decide **después de leerlas de la revisión**. Prueba siempre la escritura con `--as-write` por objetos sobre X.
- **Un cálculo decía «iguales» sin comparar:** el RED de UI de PB-T4c no contaba el equivalente con `ExpectRed`. Toda cifra calculada se compara con la del paquete (`CI_T4C_EXPECTED` y el texto literal de [14]), y si no casa es un error o un PENDIENTE, nunca «iguales».
- **El sha256 del registro de la sesión:** el paquete lo cita en su cabecera. Comprueba que el archivo custodiado es ese, y si cambia, que la versión del paquete se ponga al día. En PB-T4b, la v4.8 citaba uno viejo.
- **Las rutas con unidad:** en lo que escribe la emisión (las plantillas, `COMPILE_RESULT`, los mensajes y el generador redactado), 0; usa la etiqueta de la copia, `r5g`, nunca `/d/r5g`. En lo custodiado, las etiquetas `/d/r4<letra>` y `/d/r5<letra>` van tal cual, y las rutas con perfil o usuario se redactan.
- **MAX_PATH:** mide la ruta más larga de X con el worktree antes de emitir.
- **La línea de órdenes de Windows** no admite las 786 rutas de un `git add` (32 767 caracteres): `commit_files` usa `--pathspec-from-file`.
- **El autor sigue escribiendo mientras corres:** `--selftest` y `--dry-objects` pueden dar recuentos distintos.
