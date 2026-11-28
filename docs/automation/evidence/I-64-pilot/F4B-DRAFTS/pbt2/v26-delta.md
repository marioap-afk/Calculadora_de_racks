# I-64 F4B — PB-T2: de la v2.5 a la v2.6 del paquete (para emitir PB-T2c)

> **Redacción para el repo público:** sin rutas locales ni nombres de usuario, de máquina o de clientes.

**Entrada:**
- **La comprobación acotada de PB-T2c,** `independent-review-pbt2c-v25-check.md` (**CA-T2c**): **EMITIBLE TRAS CORRECCIONES, B 0 / I 1 / M 5**, sin decisiones nuevas para el Owner.
- **Su recomprobación sobre la v2.6,** la §9 del mismo informe: **EMITIBLE, nuevos B 0 / I 0 / M 3**, sin decisiones nuevas para el Owner. La sesión la aplica así:
  - **M-06:** la fila V26-08;
  - **M-07:** solo en `make_pbt2c_contract.py`. El de PB-T2b no cambia (M-05); su caso va a `ideas-futuras-pbt2c-generator.md`;
  - **M-08:** la sesión regenera el encargo con el de esta recomprobación;
  - **`BOUNDED_RESULT`:** el literal de la sesión, que nombra la §9 (abajo).
- **Las decisiones de la sesión:**
  - **M-05:** PB-T2b se emite con la v2.5. Su generador y su script de emisión no cambian. La v2.6 solo la usa el generador de PB-T2c y se custodia con la emisión de PB-T2c.
  - **M-02:** MU-T2C-03 se define por contenido en PB-T2c, sin tocar R4 de PB-T2b. Si eso no fuera posible sin tocar PB-T2b, había que parar e informar. Es posible: ver §2.
  - Custodiar con PB-T2c el encargo literal de CA-T2c (`independent-review-pbt2c-v25-check-prompt.md`), escrito por la sesión.
  - **Sobre las dudas del generador:**
    - **D-11:** extender INV-PBT2C-04 a las corridas aisladas, con la redacción de C02, en una fila V26 (V26-07);
    - **D-15:** corregir la cuenta de `<checkout-principal>` en `custody_prompt_pbt2b.py` y en `v25-delta.md` §2, y poner `<worktrees-de-Codex>` en la nota de redacción de los dos ayudantes. Antes se comprobó que ni los generadores ni `issue_pbt2b.py` comparan esos bytes (§4).

**Salida:**
- `I-64-f4b-pbt2-gate-contract-package-v26.md`: la v2.5 con el registro v2.5 → v2.6 (V26-01 a V26-08) delante de los anteriores;
- este archivo;
- `make_pbt2c_contract.py`, con la v2.6, el `BOUNDED_RESULT` de CA-T2c y su §9, el aborto de V26-08 (`mu_check`) y el `bounded_check` de M-07;
- `custody_prompt_pbt2c.py`: la custodia del encargo de CA-T2c y de su recomprobación, y su comprobación;
- `ideas-futuras-pbt2c-generator.md`: la nota para `docs/ideas-futuras.md` sobre el `bounded_check` de PB-T2b (M-07);
- `recheck_pbt2b_v25.py`: la prueba de que PB-T2b sigue con la v2.5 y no toma la v2.6;
- `custody_prompt_pbt2b.py` y `v25-delta.md` §2, con la corrección de D-15;
- el seco y las pruebas en negativo de PB-T2c repetidos con la v2.6.

**Resultado:**
- Nada cambia el diseño ni lo que se informa al Owner.
- El contrato, el generador y el script de emisión de PB-T2b no cambian. De su custodia cambian dos textos por D-15: la nota del encargo de CA-T2b y `v25-delta.md` §2 (§4).
- No hay decisiones nuevas.

## 1. Los cambios

| Id | Hallazgo | Qué cambia | Dónde |
|---|---|---|---|
| **V26-01** | I-01 | **Las fuentes de CT-PBT2C-01 y 02:** todo se lee del borrador, justo tras «Aplicar», con `c = Pasada(Copy(draft.CurrentDraftState), A)` (`Pasada` es el ayudante de [PT1] §5.0 que ya usa `DisenoDe`).<br>- **Filas F y `pb-08-redondeo`:** `Persist(c.Design)`, que es `DisenoDe(draft.CurrentDraftState, A)`. `sistema.AnnotationScale` = `c.System.Structure.AnnotationScale`.<br>- **Filas de estilo:** `diseno` = `c.Design.Structure.DimensionStyle` y `sistema` = `c.System.Structure.DimensionStyle`. `cotas>0` y `estilos` salen de las instancias `Dimension` de los planos de `c`, con el `Cotas` de CT-PBTC-05 (`PushBackDeclaredCorrectionDimensionStyleTests.cs:82-92`) aplicado a `c` y no a `w.LastComputation`.<br>- **Lo que nunca se lee:** la petición clásica (`DesignToInsert` y `SystemToInsert`, `:114-115`), que en WORKSPACE es nula.<br>- **CT-PBT2C-02:** `diseno-del-borrador-compuesto` = `c.Design.IsComposite`, con la misma `c`.<br>- **D-PBT2-13** gana una frase con la misma regla | D-PBT2-13; §5.4, CT-PBT2C-01 y 02 |
| **V26-02** | M-01 | **Cómo se mide la tabla de CT-PBT2C-03:** una corrida temporal de UI de los 49 gestos en WORKSPACE sobre el cierre de PB-T2b, fuera de todo commit y con TRX (INV-10). La tabla va en `pbt2c-anchors-out.txt`.<br>**Un cuarto caso** (aplica con `diseno-igual=False`, o se niega con `diseno-igual=True`) es un hecho nuevo. Lleva a otra versión del paquete y, si muestra una diferencia entre CLASSIC y WORKSPACE, al Owner | §5.4, CT-PBT2C-03; §6.0, anclajes de PB-T2c |
| **V26-03** | M-02 | **MU-T2C-03, por contenido:** la única línea de `ApplyToDraft` que contiene `draftPort.ApplyToDraft(PushBackEditableStateComparer.Copy(editable))`. En esa línea, `Copy(editable)` pasa a `Copy(appliedState)`. El título de la tabla lo dice | §5.4, la tabla de mutantes |
| **V26-04** | M-03 | **Cómo copia el Worker la tabla:** las 49 líneas esperadas de `pbt2c-anchors-out.txt`, que es autoridad, en su orden y sin las líneas `  porque:`, que son de la remedición. `<fila>` es el nombre del gesto en `Gestos()` | §5.4, CT-PBT2C-03 |
| **V26-05** | M-04 | **Las corridas aisladas:** una prueba existente que falle en la corrida aislada de un mutante (INV-PBT2C-03) también se atribuye a él, como en el RED | §5.4, el régimen; PBT2C-C02 del generador |
| **V26-06** | M-05; la sesión | **El orden de custodia:**<br>- PB-T2b se emite con la v2.5;<br>- la v2.6, este archivo y el encargo literal de CA-T2c se custodian con la emisión de PB-T2c.<br>También cambian el **Estado** y la cabecera | §6.0, «Custodia»; cabecera |
| **V26-07** | M-04; la sesión (D-11) | **INV-PBT2C-04 también cubre las corridas aisladas,** con la misma redacción que C02: «cada prueba existente que falle en el RED de PB-T2c o en las corridas aisladas de los mutantes de INV-PBT2C-03 se atribuye a un mutante de la tabla cerrada MU-T2C-01..03». V26-05 lo decía en el régimen y en C02, pero el invariante, que va literal al contrato, se quedaba en el RED | §6.3, INV-PBT2C-04 |
| **V26-08** | M-06 (§9) | **Si MU-T2C-03 no tiene la forma esperada en el cierre de PB-T2b,** el generador de PB-T2c aborta y la emisión de PB-T2c se para. Pasa si:<br>- el fragmento no está en una sola línea física de W;<br>- o la firma de `ApplyToDraft` no es `private void ApplyToDraft()`.<br>Entonces la sesión redefine la línea del mutante por contenido en una versión nueva del paquete, con su comprobación acotada. No es decisión del Owner, salvo que el cambio altere el comportamiento. El mensaje de aborto del generador lo dice | §5.4, la tabla de mutantes; el generador de PB-T2c |

**La §9 en la cabecera:** la viñeta de la v2.6 y el origen del registro dicen su veredicto y dónde va cada menor.

**`BOUNDED_RESULT`** (`make_pbt2c_contract.py`) es el literal de la sesión, que nombra la §9:
`EMITIBLE TRAS CORRECCIONES (B 0 / I 1 / M 5; I-01 y M-01 a M-05 corregidas en el paquete; recomprobacion v2.6: EMITIBLE, B 0 / I 0 / M 3; M-06 a M-08 aplicadas)`.

**`bounded_check` lo acepta** con el informe real (con su §9) y las líneas de la v2.6: I-01 tiene su fila V26-01 con `**CA-T2c I-01**`, y la §9 se empareja con su veredicto. Da error:
- con las líneas de la v2.5, que no tienen fila V26;
- con el literal sin la §9;
- con otra cuenta de la §9.

## 2. M-02: por qué basta el contenido sin tocar PB-T2b

**R4 de CT-PBT2B-08 ya exige el fragmento.** Pide `draftPort.ApplyToDraft(PushBackEditableStateComparer.Copy(editable))` una sola vez dentro de `ApplyToDraft`. El mutante se define sobre ese fragmento, así que el Worker de PB-T2b puede llamar a la variable del resultado como quiera.

**El generador de PB-T2c:**
- busca, en `ApplyToDraft` de W, la única línea que contiene el fragmento;
- sustituye `PushBackEditableStateComparer.Copy(editable)` por `PushBackEditableStateComparer.Copy(appliedState)`;
- comprueba la fila de la tabla contra ese contenido.

Hoy es AVISO, porque la línea la deja PB-T2b. Al escribir, es error si:
- falta;
- aparece más de una vez en W (más estricto que R4, que la cuenta solo en `ApplyToDraft`);
- está fuera de `private void ApplyToDraft()`;
- `Copy(editable)` sale más de una vez en la línea.

## 3. M-05: PB-T2b sigue con la v2.5

**`make_pbt2b_contract.py` no cambia:** 140 205 bytes, sha256 `677A8664…`, `PKG_VERSION = 'v2.5'`. Tampoco cambian `issue_pbt2b.py` ni sus plantillas.

**No puede tomar la v2.6:**
- en seco lee solo `--package-file`;
- al escribir, lee el paquete de HEAD, que `issue_pbt2b.py` custodia como la v2.5 con la emisión de PB-T2b.

**`recheck_pbt2b_v25.py` lo comprueba:**
- **con la v2.5 sobre `17872be8`:** rc 0, contrato `6E7A2801…`, 13 AVISOS, salida igual byte a byte a `dry-pbt2b.txt`;
- **con la v2.6 por `--package-file`:** falla con `AssertionError: ('--package-file no es la v2.5', 'v2.6')`.

## 4. La custodia de los encargos de CA-T2c y CA-T2b

**`custody_prompt_pbt2c.py`** es una copia de `custody_prompt_pbt2b.py`, con la misma sustitución local (`<worktrees-de-Codex>\...`).

**La nota de redacción (D-15), en los dos ayudantes:**
- `prompt_note()` es la NOTE de `issue_pbt1c.py` con `<worktrees-de-Codex>` entre sus marcadores;
- `prompt_text` la pone siempre en lugar de la NOTE común, porque la sustitución local también es una redacción;
- el encargo de CA-T2c no nombra el checkout principal, así que sin esto no llevaría nota: `red_text` solo la pone si `redact` cambia el texto.

**Las comprobaciones** (`python -I custody_prompt_pbt2b.py` y `python -I custody_prompt_pbt2c.py`):
- sin la sustitución local, FORBIDDEN rechaza el texto;
- con ella, el texto sale limpio, con la nota nueva y `<worktrees-de-Codex>\...` una vez por ruta;
- **`<checkout-principal>`, contado:**
  - en el de CA-T2b sale dos veces, en la nota y en la ruta del checkout;
  - en el de CA-T2c sale una vez, en la nota;
- solo cambian las líneas de las rutas: 58 y 59 en el de CA-T2b, y 30 y 65 en el de CA-T2c.

**El encargo de CA-T2c con su recomprobación (M-08):** la sesión lo regeneró con dos mensajes, el de CA-T2c y el de la §9, y cada uno nombra la ruta de los worktrees una vez. El ayudante exigía exactamente una marca y lo rechazaba.
- **Ahora** (decisión de la sesión, 2026-11-19), en los dos ayudantes, la cuenta sale del archivo. `prompt_expected` pide:
  - una marca por cada aparición de la ruta original;
  - que una búsqueda laxa de la carpeta de worktrees dé la misma cuenta, para que una forma no reconocida aborte con un mensaje claro;
  - que la marca no esté ya en el texto.
- **No cuenta secciones:** una sección puede no nombrar la ruta. Pasa en el encargo de CA-T2c regenerado, con tres secciones y la ruta en la 1 y la 2, y en el de CA-T2b, con la ruta en la 2 y la 3. Durante un tiempo se contaron secciones (D-16); se cambió por eso.
- **Con los de hoy:**
  - CA-T2c: 2 marcas, líneas 30 y 65;
  - CA-T2b: 2 marcas, líneas 59 y 108, más la del checkout, la 58.

  Los dos salen con FORBIDDEN limpio.
- **En negativo:** abortan una ruta de worktrees sin perfil y la marca ya escrita en el texto.

**La línea de custodia del §6.0 de PB-T2c** (D-16) dice ahora, como la de PB-T2b, «el encargo literal de su comprobación acotada y de su recomprobación». Cambia en la v2.6 y en el texto que espera el generador.

**D-15 en lo de PB-T2b:**
- El mensaje de `custody_prompt_pbt2b.py` y `v25-delta.md` §2 decían «`<checkout-principal>` una vez». Ahora dan la cuenta real, dos.
- **Nada compara esos bytes:**
  - `issue_pbt2b.py` no fija ningún sha. Custodia `v25-delta.md` con la redacción común, toma el encargo de `prompt_text()` y solo exige `PROMPT_MARK` una vez;
  - en `make_pbt2b_contract.py`, `v25-delta.md` es solo una ruta de `CUSTODY_EXTRA` (`soft(in_head)`).
- **El `--selftest` de `issue_pbt2b.py`** sigue en rc 0. Solo cambian dos tamaños: `v25-delta.md` (5 941 → 6 111 bytes) y el encargo (5 521 → 5 545, la nota).
- **El seco de PB-T2b** sigue igual byte a byte, con el contrato `6E7A2801…`.

El script de emisión de PB-T2c no existe todavía. Al escribirlo, la custodia del encargo pasa por `prompt_text()`.

## 5. Lo que comparan los generadores y cambia

Todo en `make_pbt2c_contract.py`:
- **Versión y resultado:** `PKG_VERSION = 'v2.6'` y `BOUNDED_RESULT`.
- **EXPECT_60:** la línea de anclajes, con la medición (V26-02), y la frase de custodia, con PB-T2b en la v2.5 y PB-T2c en la v2.6 (V26-06). La de PB-T2c lleva «y de su recomprobación» (D-16).
- **EXPECT_54:** la línea del régimen (V26-05) y el título de la tabla de mutantes (V26-03).
- **La tabla de mutantes:** se compara plegada contra las filas que salen de `MUTANTS`. MU-T2C-01 y 02 son por línea; MU-T2C-03, por contenido.
- **`mu_check` (V26-08):** la búsqueda de la línea de cada mutante pasa a una función extraíble. Para MU-T2C-03 aborta con el texto de V26-08 si:
  - la llamada `draftPort.ApplyToDraft(` está pero el fragmento no cabe en una línea;
  - la línea se repite;
  - la declaración de encima no es `private void ApplyToDraft()`;
  - la sustitución ya está.

  El AVISO del seco nombra V26-08 para cuando se escriba.
- **`bounded_check` (M-07):**
  - acepta un identificador de sección de número o de letra, como §R;
  - empareja en orden cada encabezado `## …Recomprobación…` con su línea `**Veredicto de §…`;
  - si uno no tiene su forma o no se empareja, da un error claro en lugar del `IndexError` de antes.
- **Huellas nuevas:**
  - siete de I-01 en CT-PBT2C-01;
  - una de I-01 en CT-PBT2C-02;
  - cinco de M-01 y M-03 en CT-PBT2C-03;
  - la frase de D-PBT2-13.
- **Cabecera y registro:**
  - la cabecera de la v2.6, con la frase de la §9;
  - el registro V26-01 a V26-08, cada fila con su `**CA-T2c X**` (V26-07 con M-04 y V26-08 con M-06);
  - dos huellas del texto de V26-08;
  - que el mensaje de aborto (`M06`) diga lo mismo.
- **PBT2C-C02:** el paréntesis dice «in the RED of PB-T2c and in the isolated mutant runs of INV-PBT2C-03…» (V26-05).
- **INV-PBT2C-04:** el texto nuevo, literal, en los invariantes propios (V26-07).
- **Custodia y autoridades:**
  - `CUSTODY_EXTRA` gana `v26-delta.md`;
  - la sección del paquete en Authorities nombra el registro v2.5 → v2.6 (V26-01 a V26-08) y la §9;
  - la autoridad de la comprobación acotada lleva el `BOUNDED_RESULT` nuevo.
- **`T_ANCHORS` gana cinco líneas** de `PushBackDeclaredCorrectionDimensionStyleTests.cs`: 82, 83 y 92 (el `Cotas`) y 114 y 115 (la petición clásica). Todas comprobadas sobre `17872be8`.

**Lo que no cambia:** el alcance (3 allowed y 173 forbidden), los demás invariantes, las demás paradas, los RequiredTests (39), la ExpectedEvidence y la línea base. Del Objective solo cambia la versión.

## 6. Lo que no cambia, a propósito

- **Las líneas de W:** las fuentes de I-01 no citan W:264-265, porque PB-T2b cambia W y la cita quedaría vieja. Que la petición clásica es nula en WORKSPACE se dice por contenido.
- **El registro de mediciones:** no gana apartado.

## 7. Reglas

- **Lecturas:** solo por objetos git desde el checkout principal (`17872be8` y `5ddeb3b4`), sin tocar el worktree.
- **Escrituras:** solo en `hs65\pbt2\`, con guiones escritos con la herramienta de escritura: `_work/c/make_v26.py`, `_work/c/make_v26b.py` (V26-07) y `_work/c/make_v26c.py` (V26-08 y la §9), `custody_prompt_pbt2c.py`, `recheck_pbt2b_v25.py` y los ayudantes de `_work/c/`.
- **Ni dotnet ni git de escritura.**
