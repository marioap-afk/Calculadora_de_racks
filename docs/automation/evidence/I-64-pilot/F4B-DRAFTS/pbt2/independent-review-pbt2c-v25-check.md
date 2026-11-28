# I-64 F4B PB-T2c: comprobación acotada sobre la v2.5 (CA-T2c), antes de emitir

**Veredicto: EMITIBLE TRAS CORRECCIONES. B 0 / I 1 / M 5.**

- **Decisiones nuevas para el Owner:** ninguna (§7).
- **La condición de F4B-E28, pregunta 4,** se cumple cuando se cumplan tres cosas:
  - I-01 está corregida en el texto del paquete, una v2.6 (ver M-05 sobre el orden respecto de la emisión de PB-T2b);
  - la remedición sobre el cierre de PB-T2b (§6) no trae ningún cambio material;
  - PB-T2b no llega a su intento 3.
- **La importante:** las filas de estilo y de `pb-08-redondeo` de CT-PBT2C-01 no dicen de dónde se leen `sistema`, `cotas>0` y `estilos`.
  - Los originales los leen de la petición clásica, que en WORKSPACE es nula.
  - Copiados tal cual, lanzan una excepción sobre `BaseSha`, y eso es la parada PBT2C-C04.
  - Leídos de la ventana, no prueban lo que D-PBT2-13 pide: que el estilo llegue **al borrador**.
- **`BOUNDED_RESULT` propuesto (ASCII):** pasa el `bounded_check` de `make_pbt2c_contract.py:1260-1299` una vez registrada I-01 en una fila V26 con `**CA-T2c I-01**`:
  `EMITIBLE TRAS CORRECCIONES (B 0 / I 1 / M 5; I-01 y M-01 a M-05 corregidas en el paquete)`
  Si hay una recomprobación, el literal tiene que nombrarla (`recomprobacion vX.Y: …`).

## 1. Método y límites

- **Qué se leyó:**
  - el repositorio, solo por objetos git desde el checkout principal, en `5ddeb3b4` (el GREEN de PB-T2a) y en `17872be8`;
  - en el borrador: la v2.5 y `v25-delta.md`, `make_pbt2c_contract.py` con `generator-notes-pbt2c.md`, `dry-pbt2c.txt`, `contract-dry-pbt2c.json`, `neg-pbt2c.txt` y `run_pbt2c_checks.py`.
- **La v2.5 frente a la v2.4:** el §5.4 y el §6.3 son idénticos; el §5.0 solo cambia en V25-02 y V25-03.
- **Qué no se hizo:** ni compilar, ni ejecutar pruebas, ni tocar el worktree. No hizo falta ningún guion `cat2c_`.
- **Una incidencia de procedimiento, declarada:**
  - una orden de Bash llevó por error `python -I -c "print(1)"`, pegada delante de un `grep` del contrato en seco;
  - no leyó ni escribió nada: imprimió un 1 a `/dev/null`;
  - incumple la regla «Python escrito con Write, nunca con `-c`» y no se repitió.
- **Límites:**
  - todo es por lectura;
  - PB-T2b no existe todavía, así que lo que depende de su GREEN (la línea de MU-T2C-03, la tabla de CT-PBT2C-03 y la CI base) solo se juzga como diseño.

## 2. Fidelidad del contrato en seco (punto 1)

| Campo | En el seco | Estado |
|---|---|---|
| **Alcance** | `allowed` 3 (W, WC y la clase nueva); `forbidden` 173, que se reparten así: `src` 60 (59 de hoy más el archivo del modo, aunque aún no exista), `tests` 96 (Core 29 y UI 67, con las 2 clases de PB-T2a y las 6 de PB-T2b) y raíz 17 | ✔ (§6.0 y §6.3) |
| **Invariantes** | INV-PBT2C-01 a 04, literales y sin la nota; los comunes 07 a 12 y 14. C09 remite a INV-PBT2C-10, que existe | ✔ |
| **Paradas** | 17 estándar, con P-01 en la Snapshot J. C08, C11 y C12 con el prefijo PBT2C. **C11, literal:** la referencia de V22-01 («section 6.0 of the PB-T1 contract package ([PT1], …)») y la cláusula final «neither one-off acceptance extends to PB-T2c». C04, C13 y C15 son literales del §6.3. C17 lleva el texto del Owner. C02 está ampliada (D-5) | ✔ (M-04 para C02) |
| **RequiredTests (39)** | 8 del §6.3: la clase nueva con `ExpectRed` y MinSelected 3, las 5 de PB-T2b (3, 4, 1, 1, 1) y las 2 de PB-T2a (3, 1). A eso se suman los 31 fijos del §6.2: Core 11 y UI 20, con DialogWindow 20, RichEditor 25 y FullEditor UI 16, que son sus ejecutables | ✔ (D-1) |
| **Que los filtros no se solapen** | Las clases nuevas no caen en otro filtro: `RackCad.Tests.Workspace` y `RackCad.UI.Tests.Workspace` no contienen `PushBackWorkspace…`, y `PushBackWorkspaceModeTests` (UI) no alcanza a `…ModeSourceGuardTests`, que está en Core | ✔ |
| **ExpectedEvidence** | **Lo común**, con el `Trailer` real.<br>**La CI del RED de PB-T2c:** `tests` en `failure` por CT-PBT2B-08 R4 con MU-T2C-03, `ui-tests` en `failure`, `build-ui` en `success` y `build-plugin` omitido.<br>**El GREEN:** Core 19 094, UI 2 450, 21 omitidas y 2 429 pasadas.<br>**Lo propio:** la tabla de atribución, los tres mutantes aislados y la CI del RED | ✔ |
| **Los marcadores que abortan** | Los 17 AVISOS del seco son todos del cierre de PB-T2a o PB-T2b o de la emisión de PB-T2c. `soft()` aborta al escribir, y la escritura exige `PT2B_CLOSE`, `CI_BASE_T2B` y `BOUNDED_RESULT` | ✔ |
| **El generador** | **El control positivo:** sobre `5ddeb3b4` sale el mismo sha256 (`88511B19…`).<br>**Las pruebas en negativo:** 39 de 39 fallan.<br>**Los casos de prueba de las funciones:** `ci_base_check`, `bounded_check` (con V25-01) y `ct3_table_check` | ✔ |

## 3. Momento de lectura y datos, CT por CT (punto 2)

| CT | Lectura frente al paso | Gesto o dato | ¿Expuesta? |
|---|---|---|---|
| **01**, filas F y `pb-08` | `aplicado` usa el `actual` tomado justo antes de «Aplicar»; `diseno` se lee de `DisenoDe(draft.CurrentDraftState, A)` tras «Aplicar». El gesto neutro, `DrawRackNameCheck`, escucha `Click` (WX:58-59). Por eso no recalcula al marcarlo; lo lee el `ReadInputs` de «Aplicar» (W:776). | La casilla se invierte, así que el gesto siempre cambia el estado. Las filas F son las de SC1 (base + 0,00044), y el generador las comprueba | **Sí, la fila `pb-08`:** `sistema.AnnotationScale` no tiene fuente en WORKSPACE (I-01) |
| **01**, estilo (`pb-06`, `pb-07`) | `diseno=`, `sistema=`, `cotas>0=` y `estilos=` no tienen fuente definida. En CT-PBTC-05 salen de `w.DesignToInsert` y `w.SystemToInsert` (PushBackDeclaredCorrectionDimensionStyleTests.cs:114-115) y de `Cotas(w)`, sobre `w.LastComputation` (:83-92). En WORKSPACE la petición es nula (W:264-265) | — | **Sí (I-01)** |
| **02** | Copia CT-PBTC-07. `Paso` lee el aviso y `w.LastComputation.Design.IsComposite` justo tras cada gesto, como en CLASSIC (PushBackDeclaredCorrectionSideBNoticeTests.cs:33-37). La tercera ventana se lee tras «Aplicar», por la regla del §5.0 | Declarar B sin frentes no bloquea: los únicos bloqueantes son el hueco no válido, el separador sin hueco, la estructura no válida y la celda que no cabe (`PushBackCompositeDiagnostics`). Así, `aplicar-habilitado=True llamadas=1` se sostiene. `diseno-del-borrador-compuesto` no dice su fuente; se entiende la de I-01 | No (lo cubre I-01) |
| **03** | `aplicado`, `igual-a-la-ventana`, `diseno-igual` y `descartes` se leen tras «Aplicar», que recalcula. Así, el desfase de una pasada de `GapBox`, el separador central y `StructureOverrideBox` queda cubierto: es la segunda pasada, como el `Recalcular` de C-1 de PB-T1c | Los 49 gestos (sobre `pb-01-base` 33 y sobre `pb-20-ab` 6; ninguno sobre `pb-21-cola`) son los de la clase cerrada. Los cuatro de D2 quedan fuera. `ToleranceBox=4`, la selección y el lado frontal caen en «nada que aplicar» | Por diseño, no; queda el procedimiento si la remedición da un cuarto caso (M-01) |

**Los mutantes en el RED conjunto y aislados** (INV-PBT2C-02 y 03). Lo comprobé por lectura:
- **MU-T2C-01 aislado** redondea ya en la carga: el `Recompute` de `LoadFromModel` llama a `ReadInputs` con `Keep` mutado, sobre cajas sembradas con `0.###`. Por eso W0 y el borrador quedan con el valor redondeado, y CT-01 falla.
- **MU-T2C-02** hace fallar CT-02.
- **MU-T2C-03** hace fallar CT-03 en las filas que aplican: el borrador recibe W0, así que `igual-a-la-ventana=False`.
- **En el RED conjunto,** CT-01 sigue fallando, porque W0 ya está redondeado por MU-T2C-01.
- **Core falla por R4 de CT-PBT2B-08,** que exige `Copy(editable)`.

## 4. Las dudas del generador (punto 3)

- **D-1 (los 31 filtros fijos del §6.2):** **de acuerdo.**
  - Es la red del §5.1, que INV-PBT2C-07 manda mantener en verde, y el generador comprueba que coincide con el §6.2: Core 11 y UI 20.
  - Quedarse solo con `PushBack*` dejaría fuera Workspace, Cantilever, RichEditor, BlankFront, DialogWindow, I55 y NamespaceFolderGuard, que también protege el invariante.
  - En el RED, MU-T2C-01 romperá pruebas de PB-TC, PB-TC2, PB-T1a y quizá de StateNet o DeclaredCorrection. Eso es atribución (INV-PBT2C-04), no parada.
- **D-2 (los nombres):** de acuerdo.
- **D-3 (el formato de la tabla):** **de acuerdo.** La línea es la misma de la CT, y `  porque:` va debajo de cada fila que «se niega». Falta decir dos cosas:
  - que el Worker copia las 49 líneas sin las `  porque:`, que son de la remedición y no van al `Assert.Equal`;
  - de qué autoridad las toma (M-03).
- **D-4 (comprobar por contenido):** **de acuerdo.**
  - El Worker de PB-T2c solo escribe la clase nueva y las tres sustituciones de una línea, localizadas por su texto literal, así que las citas de línea de W (W:760-782, W:3570) son informativas.
  - `pbt2c-anchors-out.txt` debe imprimir dónde están en la `BaseSha` real: las tres líneas, `ReadInputs` y `Keep`. El seco ya lo hace, así que al escribir saldrá sobre el cierre de PB-T2b.
  - La única que depende del texto que deje PB-T2b es la línea de MU-T2C-03 (M-02).
- **D-5 (C02 ampliada):** **necesaria.** Sin el paréntesis, C02 («a … PB-T2b test that does not stay green returns to the Owner») contradiría el RED previsto, en el que CT-PBT2B-03, 04 y 08 fallan por MU-T2C-03. Conviene extenderlo también a las corridas aisladas de INV-PBT2C-03 (M-04).
- **D-6 (W y WC como `UNIT_CHANGE`):** de acuerdo. Cumple F4B-E25, y el §6.0 lo dice.
- **D-7 (solo el `AllowedWriteScope` y C11 del contrato de PB-T2b):** de acuerdo.
- **D-8 (`PKG_VERSION` v2.5):** I-01 obliga a una v2.6. Los dos generadores fijan la v2.5 (`make_pbt2b_contract.py:56`, `make_pbt2c_contract.py:48`), y el de PB-T2b exige la v2.5 en HEAD al escribir (M-05).
- **D-9 (el cierre de PB-T2a, dentro del de PB-T2b):** **de acuerdo.** La cadena es transitiva:
  - el generador de PB-T2b ya ata su `BaseSha` a `PT2A_CLOSE`;
  - el de PB-T2c exige que el `f4b-dispositions.md` del cierre de PB-T2b traiga antes el cierre de PB-T2a;
  - el `task-close-package.md` de PB-T2a es una autoridad, y su AVISO aborta al escribir.

  Una constante propia solo duplicaría la comprobación.
- **D-10 (los AVISOS agrupados):** de acuerdo.

## 5. Lo que el generador debe volver a medir al escribir, tras el cierre de PB-T2b (punto 4)

1. **El cierre de PB-T2b:**
   - `PT2B_CLOSE` y su entrada «Cierre de F4B-PBT2B-WORKSPACE-MODE-WINDOW», posterior a F4B-E33 y con el cierre de PB-T2a delante;
   - los `task-close-package.md` de PB-T2a y de PB-T2b;
   - el contrato emitido de PB-T2b, contrastado en su `AllowedWriteScope` y su C11.
2. **El producto:**
   - `git diff PT2B_CLOSE BaseSha -- src` está vacío;
   - WC y WX siguen siendo `1a3300b4` y `fb094bef`;
   - W solo cambió dentro de INV-PBT2B-03;
   - el archivo del modo existe.
3. **Los tres mutantes:** cada línea una vez, en su miembro, y sin su sustitución. Para MU-T2C-03, el literal con `var result =` (M-02).
4. **Los datos de CT-PBT2C-01 y 02:**
   - `DrawRackNameCheck` con `Click`, y `ReadInputs` que lo lee;
   - las filas F, `pb-08`, STANDARD-ISO y SIMPLEX;
   - las doce líneas de CT-PBTC-07;
   - las fuentes que fije I-01 (`Pasada` sobre el borrador y las cotas de esa computación).
5. **La tabla de CT-PBT2C-03:**
   - la corrida temporal de los 49 gestos en WORKSPACE, sobre `PT2B_CLOSE`, fuera de todo commit y con su TRX (M-01);
   - la tabla, en `pbt2c-anchors-out.txt`, que pase `ct3_table_check`.
6. **La CI:**
   - `CI_BASE_T2B` es la corrida `push` 4/4 de la punta del cierre, con (19 094, 2 447, 21); si no, `CI_T2B_EXPLAINED`;
   - el GREEN previsto, Core +0 y UI +3.
7. **Los RequiredTests:**
   - las clases de PB-T2b existen, con al menos 3, 4, 1, 1 y 1 ejecutables;
   - las 31 familias tienen los recuentos de `17872be8`, o la diferencia explicada.
8. **La custodia:**
   - el paquete vigente en HEAD (M-05);
   - CA-T2c y su encargo literal;
   - `pbt2c-anchors-out.txt` y el seco;
   - la entrada de emisión, la última, con F4B-E28, pregunta 4;
   - el `BOUNDED_RESULT` que pasa el `bounded_check`.
9. **El entorno:**
   - `MainSha` = `origin/main`;
   - las celdas vigentes hasta el 2026-12-29;
   - la Snapshot J;
   - una sola delegación abierta;
   - PB-T2b sin intento 3.

## 6. Lo que se comprobó en el código para este informe

- **La petición clásica:** `SystemToInsert` y `DesignToInsert` salen de `session.InsertionRequest` (W:264-265), que en WORKSPACE nunca se anota (R1 a R4 de CT-PBT2B-08).
- **CT-PBTC-02** lee `update.DesignToInsert` y `update.SystemToInsert.Structure.AnnotationScale` (PushBackDeclaredCorrectionRoundingTests.cs:109, :122-126).
- **CT-PBTC-05** lee `w.DesignToInsert` y `w.SystemToInsert` (PushBackDeclaredCorrectionDimensionStyleTests.cs:114-115), y las cotas de `w.LastComputation` (:83-92).
- **`Pasada`** devuelve un `PushBackEditorComputation` (por ejemplo, PushBackDraftStatePortTests.cs:68), que tiene `Design`, `System` y los planos. Con ella, el borrador puede ser la fuente.
- **Los gestos de CT-PBT1C-02** se reparten así (PushBackWindowOnStateTests.cs:425-532):
  - 49 con `G(…)`;
  - 4 de D2 con `SinComparar`;
  - sobre `pb-01-base` y `pb-20-ab` solamente.
- **El gesto «Restaurar»** escribe `DepthBox` 54 y pulsa «Restaurar» (:532).

## 7. Decisiones del Owner (punto 5)

**Ninguna nueva:**
- I-01 y las menores precisan fuentes de lectura, procedimiento y orden de custodia;
- ningún comportamiento de CLASSIC ni de WORKSPACE cambia;
- D-PBT2-13 ya dice qué se prueba: «El estado aplicado y su diseño llevan el exacto o el estilo».

**Un cuarto caso en la tabla de CT-PBT2C-03 sería un hecho nuevo (M-01).** Si mostrara una diferencia entre CLASSIC y WORKSPACE, iría al Owner. Hoy no hay ninguno medido.

## 8. Hallazgos

| Id | Sev. | Dónde (v2.5) | Hallazgo | Qué hacer |
|---|---|---|---|---|
| I-01 | I | §5.4, CT-PBT2C-01 (l.935); D-PBT2-13 (l.682) | **Las fuentes:** las filas de estilo (`diseno=`, `sistema=`, `cotas>0=`, `estilos=`) y `sistema.AnnotationScale` de `pb-08` no dicen de dónde se leen. Los originales leen la petición clásica (`w.DesignToInsert` y `w.SystemToInsert`), que en WORKSPACE es nula (W:264-265).<br>**Las consecuencias:** copiados tal cual, lanzan sobre `BaseSha`, y eso es la parada PBT2C-C04. Leídos de la ventana, no prueban lo que pide D-PBT2-13, que el estilo llega al borrador. `diseno-del-borrador-compuesto` de CT-02 tampoco dice su fuente | **Fijar las fuentes en el borrador,** leídas justo tras «Aplicar», con `c = Pasada(Copy(draft.CurrentDraftState), A)`:<br>- `diseno` = `c.Design.Structure.DimensionStyle`;<br>- `sistema` = `c.System.Structure.DimensionStyle`, y `.AnnotationScale` en `pb-08`;<br>- `cotas>0` y `estilos`, de las instancias `Dimension` de los planos de `c`, con un `Cotas` sobre una computación;<br>- `diseno-del-borrador-compuesto` = `c.Design.IsComposite`.<br>**Registrarlo** en una fila V26 con `**CA-T2c I-01**` |
| M-01 | M | §5.4, CT-PBT2C-03 (l.937); §6.0, «Anclajes» de PB-T2c | **El procedimiento de la remedición:** no se dice cómo se mide la tabla. Si una fila da un cuarto caso (aplica con `diseno-igual=False`, o se niega con `diseno-igual=True`), `ct3_table_check` aborta, pero nada dice qué sigue | Decir que la remedición es una corrida temporal de UI de los 49 gestos en WORKSPACE, sobre el cierre de PB-T2b, fuera de todo commit, con TRX (INV-10). Y que una fila fuera de los tres casos es un hecho nuevo: una v2.6 y, si es una diferencia entre los dos modos, el Owner |
| M-02 | M | §5.4, la tabla de mutantes (l.945); CT-PBT2B-08 R4 (l.867) | **El problema:** la línea de MU-T2C-03 es un literal (`var result = …`) que PB-T2b no está obligado a dejar así. R4 solo exige el fragmento de la llamada. Si el Worker de PB-T2b usa otro nombre de variable, la emisión de PB-T2c aborta y hace falta otra versión del paquete | **Antes de emitir PB-T2b:** que R4, o INV-PBT2B-04, exija la sentencia literal de D-PBT2-06.<br>**O, si no:** definir MU-T2C-03 por contenido, en la única línea de `ApplyToDraft` que contiene `draftPort.ApplyToDraft(PushBackEditableStateComparer.Copy(editable))`, cambiando `editable` por `appliedState` |
| M-03 | M | §5.4, CT-PBT2C-03; §6.3 | **El problema:** no se dice que el Worker copia las 49 líneas esperadas de la tabla custodiada (`pbt2c-anchors-out.txt`, que es autoridad) **sin** las líneas `  porque:`, ni que `<fila>` es el nombre del gesto de `Gestos()` | Decirlo en CT-PBT2C-03 |
| M-04 | M | PBT2C-C02 (seco; D-5) | **El problema:** el paréntesis admite fallos por un mutante «in the RED», pero las corridas aisladas de INV-PBT2C-03 también rompen pruebas existentes | Añadir «and in the isolated mutant runs of INV-PBT2C-03» |
| M-05 | M | D-8; `PKG_VERSION` en los dos generadores | **El problema:** I-01 lleva a una v2.6. Si esa v2.6 entra en HEAD antes de emitir PB-T2b, el generador de PB-T2b, que exige la v2.5, no escribe | **Una de dos:**<br>- emitir PB-T2b con la v2.5 y custodiar la v2.6 con la emisión de PB-T2c;<br>- o poner los dos generadores en la v2.6 y repetir sus secos y sus pruebas en negativo |

## §9. Recomprobación sobre la v2.6

**Veredicto de §9: EMITIBLE. Nuevos: B 0 / I 0 / M 3.**

**Por qué la sección se llama §9 y no «§R».** El encargo pedía «§R». Pero el `bounded_check` de `make_pbt2c_contract.py:1287-1298` (V25-01) solo reconoce recomprobaciones con el número de sección: `## §<n>. Recomprobación sobre la vX.Y` y `**Veredicto de §<n>: …**`. Con «§R» pasaría esto:
- la línea `**Veredicto de §R: …**` activa la rama de recomprobaciones, pero ninguna expresión la captura, y `rechecks[-1]` lanza `IndexError`;
- si esa línea faltara, la recomprobación se ignoraría.

Por eso uso el siguiente número, §9, y el hallazgo es M-07.

**Resumen:**
- I-01 y M-01 a M-05 están corregidas en la v2.6 o resueltas por decisión de la sesión.
- Las fuentes nuevas de I-01 dan las líneas esperadas sin tocar la petición clásica.
- MU-T2C-03 por contenido nunca muta una línea equivocada. Si la línea no tiene la forma esperada, el generador aborta (M-06).
- No hay riesgos nuevos de momento de lectura, y las cifras no cambian.
- Las tres menores nuevas no impiden emitir.
- **Decisiones nuevas para el Owner:** ninguna.

**`BOUNDED_RESULT`:** hay que añadirle la recomprobación. Con esta §9, el `bounded_check` exige que la nombre, y el literal de hoy (`make_pbt2c_contract.py:47`) daría el error «BOUNDED_RESULT no nombra la ultima recomprobacion». El literal ASCII propuesto:

`EMITIBLE TRAS CORRECCIONES (B 0 / I 1 / M 5; I-01 y M-01 a M-05 corregidas en el paquete; recomprobacion v2.6: EMITIBLE, B 0 / I 0 / M 3)`

Lo comprobé contra el código de `bounded_check`:
- empieza por el primer veredicto;
- lleva `B 0 / I 1 / M 5` y `corregidas en el paquete`;
- V26-01 tiene `**CA-T2c I-01**`;
- el encabezado da `('9', 'v2.6')`, y la línea del veredicto da `('9', 'EMITIBLE', '0', '0', '3')`;
- y el literal contiene `recomprobacion v2.6: EMITIBLE, B 0 / I 0 / M 3`.

### 9.1 Método y límites

- **Las entradas:**
  - del repositorio, solo los objetos git de `5ddeb3b4` y `17872be8`, desde el checkout principal;
  - del borrador: la v2.6 con `v26-delta.md`, `make_pbt2c_contract.py`, `dry-pbt2c.txt`, `contract-dry-pbt2c.json` y `neg-pbt2c.txt`.
- **El contrato en seco:** su sha256 es `E0C0737D…`, el que da el encargo.
- **Lo que no se hizo:** no se ejecutó Python ni nada de compilación, y no se tocó el worktree.

### 9.2 Las correcciones (punto 1)

| CA-T2c | Estado | Dónde |
|---|---|---|
| **I-01** | Corregida | **Registro:** V26-01 (l.70).<br>**D-PBT2-13** (l.699): «Se leen del borrador, con `Pasada(Copy(draft.CurrentDraftState), A)` justo tras «Aplicar», y nunca de la petición clásica».<br>**CT-PBT2C-01** (l.950): fija las fuentes de las filas F, de `pb-08` y de las filas de estilo.<br>**CT-PBT2C-02** (l.951): `diseno-del-borrador-compuesto` = `c.Design.IsComposite` |
| **M-01** | Corregida | **Registro:** V26-02 (l.71).<br>**CT-PBT2C-03** (l.952), «Cómo se mide»: una corrida temporal de UI con TRX, y el cuarto caso lleva a otra versión y, si toca, al Owner.<br>**§6.0, los anclajes de PB-T2c** (l.1035) |
| **M-02** | Corregida, por decisión de la sesión | **Registro:** V26-03 (l.72).<br>**La tabla de mutantes** (l.954 y l.960).<br>**El generador** (`make_pbt2c_contract.py:308-335`): busca por fragmento, exige una sola línea en todo W, en `private void ApplyToDraft()`, sin la sustitución y con `Copy(editable)` una sola vez en la línea. Ver §9.4 |
| **M-03** | Corregida | **Registro:** V26-04 (l.73).<br>**CT-PBT2C-03** (l.952), «Cómo la copia el Worker»: las 49 líneas en su orden, sin `  porque:`, y `<fila>` = el nombre del gesto en `Gestos()` |
| **M-04** | Corregida | **Registro:** V26-05 (l.74) y V26-07 (l.76).<br>**El régimen** (l.946).<br>**PBT2C-C02 en seco:** «in the RED of PB-T2c and in the isolated mutant runs of INV-PBT2C-03…».<br>**INV-PBT2C-04 en seco:** literal, con las corridas aisladas |
| **M-05** | Resuelta por decisión de la sesión | **Registro:** V26-06 (l.75).<br>**La custodia** del §6.0 (l.1040-1043): PB-T2b se emite con la v2.5, y la v2.6 se custodia con PB-T2c.<br>**Los generadores:** `make_pbt2b_contract.py` sigue con `PKG_VERSION = 'v2.5'` (140 205 bytes, sha `677a8664…`), y `make_pbt2c_contract.py` pasa a `'v2.6'` |

### 9.3 I-01: las fuentes nuevas dan las líneas esperadas (punto 2)

- **`Pasada`** es el ayudante de [PT1] §5.0. Hace `a.BuildFrom(ca.BuildDesign(s.Editor, s.BuildInputs(a)), frontalSide)`, el mismo camino que el `Recompute` de la ventana (`I-64-f4b-pbt1-gate-contract-package.md:890-904`; copia en PushBackDraftStatePortTests.cs:66-80).
  - Devuelve un `PushBackEditorComputation`, con `Design` (`PushBackDesign`), `System` (`PushBackSystem`) y los planos que lee `Cotas`: `PlantaPlan`, `FrontalEntradaSalida`, `FrontalPosterior`, `LateralPlan` y `LateralCortes`.
  - `Pasada` muta la copia, porque avanza la línea base y la cola. Por eso va sobre `Copy(...)`, como `DisenoDe`, sin tocar el borrador ni las referencias de D2.
- **Los tipos son los que leía la petición clásica.** `DesignToInsert` es un `PushBackDesign` y `SystemToInsert` un `PushBackSystem` (W:264-265). Así:
  - `c.Design.Structure.DimensionStyle`, `c.System.Structure.DimensionStyle` y `c.System.Structure.AnnotationScale` existen;
  - son los mismos miembros que CT-PBTC-05 (PushBackDeclaredCorrectionDimensionStyleTests.cs:114-115) y CT-PBTC-02 (PushBackDeclaredCorrectionRoundingTests.cs:126) leen hoy.
- **Los valores:**
  - el estado del borrador lleva el exacto (filas F, 1.23456) y el estilo nombrado de `pb-06` y `pb-07`;
  - los planos los construye `BuildFrom` desde el diseño, que lleva `DimensionStyle`, y ese es el mismo camino que mide CT-PBTC-05 en la ventana;
  - así, `diseno` y `sistema` dan STANDARD-ISO y SIMPLEX, `cotas>0=True` con `Dimensions` = 2 en los sobres, y `estilos=[<estilo>]`;
  - el frontal A es el de CT-PBTC-05.
- **`c` se toma justo tras «Aplicar»,** sobre el `CurrentDraftState` nuevo, el que deja `SetCurrentDraftState`. El desfase de la ventana no le afecta: `c` no es la computación de la ventana.
- **Bajo los mutantes:**
  - con MU-T2C-01, el borrador recibe los valores redondeados y CT-01 sigue fallando;
  - con MU-T2C-03 solo, el borrador recibe W0, que lleva el exacto y el estilo, así que CT-01 pasa. Es correcto: su CT es CT-03.
- **D-13, el `Cotas` sobre `c`:** la latitud es mínima.
  - El cuerpo de `:86-91` queda fijado: la lista de planos, `Flatten`, `Role == Dimension`.
  - Solo cambia la entrada: una `PushBackEditorComputation` en lugar de la ventana.
  - El texto prohíbe expresamente `w.LastComputation`, que es la lectura errónea posible.
  - No lo cuento como hallazgo. Opcionalmente, el paquete podría dar la firma, `Cotas(PushBackEditorComputation computation)`, con el cuerpo de `:86-91`.

### 9.4 M-02 y D-14: MU-T2C-03 por contenido (punto 3)

**Es seguro en el sentido que importa: nunca muta otra cosa.**
- El generador exige:
  - una sola línea de W con el fragmento;
  - la declaración más cercana por encima, `private void ApplyToDraft()`, a menos de 40 líneas;
  - `Copy(editable)` una sola vez en esa línea;
  - la sustitución ausente.
- Si algo falla, aborta al escribir. Contar la unicidad en todo W (D-14) es más estricto que R4 y no tiene coste: C13 de PB-T2b no admite otro miembro que llame al puerto.

**Lo que R4 no garantiza (M-06).** R4 se comprueba sobre el texto colapsado, partido en miembros por su nombre. Hay dos casos en que PB-T2b cumple R4 y el generador no encuentra la línea:
- una llamada partida en dos líneas físicas, por ejemplo `ApplyToDraft(` y, en la línea siguiente, `PushBackEditableStateComparer.Copy(editable));`;
- una firma distinta de `private void ApplyToDraft()`. R9 solo exige que `Update_Click` llame a `ApplyToDraft();`, y una firma `private bool ApplyToDraft()` cumpliría.

En los dos, la emisión de PB-T2c para y hace falta otra versión del paquete. Es poco probable, porque D-PBT2-06 da la sentencia en una línea y la firma `void`. No puede producir un RED equivocado.

### 9.5 Lo nuevo de la v2.6 (punto 4)

- **Las cifras no cambian:** `allowed` 3, `forbidden` 173, `inv` 11, `stops` 32, `required` 39, `evidence` 9 y `authorities` 49. Las anclas de pruebas pasan de 12 a 17, por las líneas 82, 83, 92, 114 y 115 de CT-PBTC-05.
  - La CI base sigue en (19 094, 2 447, 21), y el GREEN en +0 y +3.
  - El control positivo sobre `5ddeb3b4` da el mismo sha.
  - Fallan 63 de 63 pruebas en negativo, y el informe real vale con la v2.6 (`ca-t2c-real-v26`) y no con la v2.5.
- **El momento de las lecturas, sin riesgos nuevos:**
  - V26-01 fija «justo tras «Aplicar»» para `c`;
  - V26-02 mide con el mismo gesto y el mismo «Aplicar» que la CT;
  - V26-04 a V26-07 no leen nada.
- **La custodia del encargo de esta recomprobación** (M-08). La v2.6 custodia solo el encargo de CA-T2c (l.1042). Esta §9 decide el `BOUNDED_RESULT`, igual que la §10 de CA-T2b, cuyo encargo se custodió junto al de CA-T2b (V25-04).

### 9.6 Decisiones del Owner (punto 6)

**Ninguna nueva.** M-02 y M-05 los decidió la sesión dentro de su procedimiento. Ningún comportamiento de producto cambia, ni lo que se informa al Owner.

### 9.7 Hallazgos nuevos

| Id | Sev. | Dónde | Hallazgo | Qué hacer |
|---|---|---|---|---|
| M-06 | M | §5.4, la tabla de mutantes (l.954 y l.960); `make_pbt2c_contract.py:308-335` | **El problema:** MU-T2C-03 por contenido exige el fragmento en una sola línea física y la firma `private void ApplyToDraft()`, y R4 de PB-T2b no garantiza ninguna de las dos. Si el Worker de PB-T2b parte la llamada o usa otra firma, la emisión de PB-T2c para. No hay riesgo de un RED equivocado | **Una de dos:**<br>- aceptar la parada sabiéndolo, y anotarlo en V26-03;<br>- o definir en el paquete el caso de la llamada partida (la sentencia que contiene el fragmento) y aceptar otra firma sin parámetros, como un ajuste no material de la remedición |
| M-07 | M | `bounded_check`, `make_pbt2c_contract.py:1287-1298` (y la misma función de `make_pbt2b_contract.py`) | **El problema:** una recomprobación con la sección sin número («§R») lanza `IndexError` si trae `**Veredicto de §R: …**`, y se ignora si no lo trae. Ninguna de las dos es un error claro | Reconocer `## §(\S+)\. Recomprobación` y `**Veredicto de §(\S+): …**`, y devolver un error si hay un encabezado de recomprobación o un «Veredicto de §» que no se pueda emparejar. Añadir una prueba en negativo con «§R» |
| M-08 | M | §6.0, «Custodia» (l.1042); `CUSTODY_EXTRA` | **El problema:** se custodia el encargo de CA-T2c, pero no el de esta recomprobación, que decide el `BOUNDED_RESULT` | Custodiar también este encargo literal en `independent-review-pbt2c-v25-check-prompt.md`, como hizo V25-04 con el de CA-T2b |

## §10. Recomprobación sobre la v2.7

**Veredicto de §10: EMITIBLE TRAS CORRECCIONES. Nuevos: B 0 / I 1 / M 3.**

Es la misma revisión que la §11 de CA-T2b (`independent-review-pbt2b-v23-check.md`), aplicada a PB-T2c. Allí está el análisis completo de RR-1 a RR-7, de la lectura de 16.9 y de las ayudas mecánicas. Aquí va lo que cambia o se añade en PB-T2c.

**El veredicto en breve:**
- La v2.7 refleja RR-1 a RR-6 en PB-T2c:
  - C07 con la excepción;
  - C18;
  - INV-PBT2C-02, con la cadena y con las corridas aisladas;
  - la CI del RED reparado;
  - lo que declara la entrega;
  - la autoridad F4B-E35.
- **Tiene la misma falta que PB-T2b:**
  - la segunda mitad de RR-7, cómo lee la verificación del Controller una reparación, no está;
  - la `RedPart` de 16.9 n.º 10 (`Tests`), al pie de la letra, da `fail` con cualquier reparación, porque la clase nueva está en `RT` (I-01).

  La corrección es la misma en el §6.0 común, así que una v2.8 la cierra para las dos subtareas.
- **No hay decisiones nuevas para el Owner.**
- Las menores de la §9 de esta comprobación están aplicadas:
  - M-06 en V26-08;
  - M-07 en el `bounded_check` robusto de este generador (`make_pbt2c_contract.py:1479-1495`);
  - M-08 en la custodia del §6.0 (l.1059: «el encargo literal de su comprobación acotada y de su recomprobación»).

**`BOUNDED_RESULT`:**
- **Mientras esta §10 tenga una importante, ningún literal pasa el `bounded_check`,** porque la última recomprobación tiene que dar B 0 e I 0.
- **La v2.8 y la §11:** con I-01 corregida en una v2.8 y una §11 «Recomprobación sobre la v2.8» que dé EMITIBLE con B 0 e I 0, el literal, en ASCII, será este, con `<m>` las menores de esa §11:
  `EMITIBLE TRAS CORRECCIONES (B 0 / I 1 / M 5; I-01 y M-01 a M-05 corregidas en el paquete; recomprobacion v2.6: EMITIBLE, B 0 / I 0 / M 3; M-06 a M-08 aplicadas; recomprobacion v2.8: EMITIBLE, B 0 / I 0 / M <m>)`
- **Por qué pasa:** empieza por el primer veredicto, lleva `B 0 / I 1 / M 5` y `corregidas en el paquete`, y nombra la última recomprobación. Lo exige el `bounded_check` robusto (`make_pbt2c_contract.py:1450-1509`), que empareja encabezados y veredictos por su identificador.

### 10.1 Método y límites

- **Qué se leyó:**
  - del repositorio, solo objetos git de `b29551ae` y `17872be8`, desde el checkout principal;
  - del borrador: la v2.7, `v27-delta.md`, `make_pbt2c_contract.py`, el seco sobre `17872be8` (sha `45d5c9b9…`, que coincide), `neg-pbt2c.txt` (88 de 88 pruebas en negativo fallan) y las notas, con D-17 a D-20.
- **Qué no se hizo:** no ejecuté Python, no compilé y no toqué el worktree.
- **La numeración de las dudas:** la «D-2» del encargo es la D-18 de las notas.

### 10.2 Lo mismo que en PB-T2b, aplicado a PB-T2c

- **La fidelidad a RR:** igual que en la §11.2 de CA-T2b, con las mismas lecturas de RR-1 y de RR-6 (D-17):
  - el RED de PB-T2c ya toca `src/` con los tres mutantes;
  - el GREEN, `git restore` de W y WC, solo toca `src/`, así que «`RedRepairSha..CurrentSha`, el GREEN del contrato» equivale a «solo `src/`».
- **El seco:** `stops` 33, por C18, y `authorities` 50, por F4B-E35. El resto sigue igual: alcance 3/173, `inv` 11, `required` 39, `evidence` 9.
- **Los 19 AVISOS:** el seco es sobre `17872be8`, así que se suman los del cierre de PB-T2a y de PB-T2b, los de F4B-E35, que todavía no está en esa revisión, y los de la emisión. Ninguno es un defecto. Todos abortan al escribir, y al escribir la revisión será el cierre de PB-T2b, que contiene `b29551ae`.

### 10.3 D-18: las corridas aisladas en el RED repetido

**Es correcto.** INV-PBT2C-03 pide, por mutante, una corrida aislada sobre `BaseSha` que haga fallar por aserción el CT de su fila, transcrita con su TRX. Esas corridas se hicieron con la clase original. Si la reparación cambia la clase, el TRX entregado ya no prueba nada de la clase que se entrega.

**Es suficiente, con la frase de INV-PBT2C-02** («el criterio se repite entero sobre `RedRepairSha`, también las corridas aisladas de INV-PBT2C-03 con la clase reparada»):
- **la atribución de INV-PBT2C-04:** las pruebas existentes que fallan no cambian con una reparación que solo toca la clase nueva, pero el criterio «entero» la repite igual;
- **C04 sin mutantes:** que los CT nuevos pasen sobre `BaseSha` sin mutantes lo acredita el GREEN, que revierte los mutantes;
- **los mutantes:** siguen en `RedRepairSha`, porque RR-2 no los deja tocar, así que el RED repetido es el mismo RED con la clase corregida.

### 10.4 RR frente a los mutantes

**El contrato lo hace cumplir.** El texto es completo:
- C18: «touches … src/» es parada;
- INV-PBT2C-02: «`RedSha..RedRepairSha` solo toca la clase nueva»;
- el §6.0: «tampoco la costura inerte ni los mutantes del RED».

**Que se compruebe depende de I-01.** El Controller tiene que comprobar `git diff --name-only RedSha RedRepairSha` ⊆ la clase nueva. Hoy ninguna de las 14 comprobaciones de 16.9 mira ese tramo:
- `Scope` mira `BaseSha..CurrentSha`, y ahí W y WC no aparecen, porque el GREEN los revierte;
- una reparación que tocara un mutante y un GREEN que lo revirtiera quedarían ocultos en `Scope`.

Por eso la corrección de I-01 tiene que incluir RR-6 entre lo que comprueba el Controller.

### 10.5 El rango de autoridades tras la emisión de PB-T2b

**No rompe nada.**
- La sección de `f4b-dispositions.md` es «archivo completo: F4B-E01 a `DISP_LAST`», y `DISP_LAST` se calcula de la última entrada de la revisión (`make_pbt2c_contract.py:469-471`, :1359). Hoy, sobre `17872be8`, da F4B-E33. Al escribir dará la de la emisión de PB-T2c, después de F4B-E36, la emisión de PB-T2b, y del cierre de PB-T2b.
- El generador no fija ningún número posterior a F4B-E35:
  - solo exige la numeración seguida (≥ 33) y las entradas E06, E09 y E23;
  - el cierre de PB-T2b lo busca por su título («Cierre de F4B-PBT2B-WORKSPACE-MODE-WINDOW»), posterior a F4B-E33 y con el cierre de PB-T2a delante (:203-219);
  - F4B-E35 se fija solo como la autoridad de RR, que ya existe en `b29551ae`.

### 10.6 Decisiones del Owner

**Ninguna nueva,** por las mismas razones que en la §11.6 de CA-T2b.

### 10.7 Hallazgos nuevos

| Id | Sev. | Dónde | Hallazgo | Qué hacer |
|---|---|---|---|---|
| I-01 | I | §6.0, «La reparación del RED» (común); la autoridad F4B-E35 del contrato de PB-T2c; el texto de la verificación | Es I-01 de la §11 de CA-T2b, en PB-T2c:<br>- la `RedPart` de 16.9 n.º 10 da `fail` con cualquier reparación, porque la clase nueva está en `RT`;<br>- `Trailer` (n.º 11) revisa también el commit de reparación;<br>- ninguna comprobación mira `RedSha..RedRepairSha`, así que un mutante tocado y revertido por el GREEN quedaría oculto (§10.4) | La misma corrección del §6.0 y de la verificación. En PB-T2c, el Controller comprueba además que `git diff --name-only RedSha RedRepairSha` es solo `tests/RackCad.UI.Tests/PushBackWorkspaceCarryoverTests.cs`, y que el criterio repetido incluye las corridas aisladas |
| M-01 | M | §6.0, RR-3; la verificación | **El problema:** RR-3 queda a la revisión. En PB-T2c tiene una ayuda mecánica directa | Comprobar que las 49 líneas esperadas de CT-PBT2C-03 en la clase reparada son, byte a byte, las de la tabla custodiada (`pbt2c-anchors-out.txt`, sin `  porque:`), y que las líneas fijas de CT-PBT2C-01 y 02 (los exactos, las doce de CT-PBTC-07 y la línea de aplicación) no cambian entre `RedSha` y `RedRepairSha` |
| M-02 | M | `ExpectedEvidence`, «la entrega» | Igual que M-02 de la §11 de CA-T2b: el `Trailer` nombra solo los commits RED y GREEN | «de los commits RED, RED repair (si lo hay) y GREEN» |
| M-03 | M | PBT2C-C07 y PBT2C-C18 | Igual que M-03 de la §11 de CA-T2b: la restricción positiva de RR-3 no llega al contrato | La misma frase en C18 |

## §11. Recomprobación sobre la v2.8

**Veredicto de §11: EMITIBLE. Nuevos: B 0 / I 0 / M 2.**

Es la misma revisión que la §12 de CA-T2b (`independent-review-pbt2b-v23-check.md`), aplicada a PB-T2c. Allí está el detalle de 16.9 con una reparación, de D-22 y del método. Aquí va lo propio de PB-T2c.

**El veredicto en breve:**
- I-01 de la §10 está corregida, incluida la parte propia de PB-T2c: el tramo `RedSha..RedRepairSha` se comprueba como solo la clase nueva, sin W ni WC.
- M-01 a M-03 de la §10 están corregidas, con la precisión de la sesión para M-01.
- Las dos menores nuevas no impiden emitir.
- No hay decisiones nuevas para el Owner.

**`BOUNDED_RESULT` final (ASCII):**

`EMITIBLE TRAS CORRECCIONES (B 0 / I 1 / M 5; I-01 y M-01 a M-05 corregidas en el paquete; recomprobacion v2.6: EMITIBLE, B 0 / I 0 / M 3; M-06 a M-08 aplicadas; recomprobacion v2.8: EMITIBLE, B 0 / I 0 / M 2)`

Lo comprobé con la función real de `make_pbt2c_contract.py`, este informe completo y las líneas de la v2.8 (`recheck_bounded_v28.py`, salida en `recheck_bounded_v28.txt`), y no da ningún error.

### 11.1 Método y límites

- **Qué se leyó:**
  - la v2.8 y `v28-delta.md`;
  - el generador de PB-T2c;
  - su seco sobre `17872be8`: sha `d8a544c1…`, el del encargo; `evidence` 10, `stops` 33, `authorities` 50 y 20 AVISOS;
  - `neg-pbt2c.txt`: 107 de 107 pruebas en negativo y los casos de `bounded_check`, con «§R»;
  - las notas.
- **Lo mismo que en la §12 de CA-T2b:**
  - solo lectura;
  - el mismo guion para el literal;
  - la misma incidencia declarada (`python -I -c ""`, una vez, en esta ronda).

### 11.2 I-01 de la §10, en PB-T2c

**La verificación con una reparación** (§6.0, l.1119-1127; la autoridad F4B-E35 y la `ExpectedEvidence` del contrato de PB-T2c) es la misma que en PB-T2b:
- n.º 10 desde `RedRepairSha`;
- n.º 11 con el commit «RED repair»;
- el Controller comprueba RR-1, RR-2, RR-4, RR-5 y RR-6, y si algo falla, es C18.

**Lo propio de PB-T2c** (l.1123, y en el contrato: «En PB-T2c, ademas: RedSha..RedRepairSha es solo tests/RackCad.UI.Tests/PushBackWorkspaceCarryoverTests.cs, sin tocar W ni WC (los mutantes del RED), aunque el GREEN los revierta despues»):
- es justo lo que pedía la §10.4: se comprueba sobre el tramo de la reparación, y no sobre `BaseSha..CurrentSha`, donde W y WC no aparecen porque el GREEN los revierte;
- un mutante tocado en la reparación ya no puede quedar oculto;
- **las corridas aisladas:** INV-PBT2C-02 las incluye en el criterio repetido. RR-4, en lo que comprueba el Controller, pide la CI y los TRX de los filtros con `ExpectRed` sobre `RedRepairSha`;
- **lo que no dice:** que los TRX de las corridas aisladas sobre la clase reparada estén entre lo que comprueba el Controller. Los pide INV-PBT2C-02, que el Controller también lee, y la entrega los transcribe (INV-PBT2C-03). Basta, pero entra en M-10.

### 11.3 M-01 a M-03 de la §10, en PB-T2c

| §10 | Estado | Dónde |
|---|---|---|
| **M-01** | Corregida, con la precisión de la sesión | **El §6.0** (l.1125): es parada C07 que las 49 líneas de CT-PBT2C-03 o las líneas fijas de CT-PBT2C-01 y 02 no sean, byte a byte, las mismas en `RedSha` y en `RedRepairSha`. Esas líneas fijas son los exactos, las doce de CT-PBTC-07 y la de «Aplicar».<br>**Los indicios** van a la revisión (l.1126).<br>**El contrato:** C07 y la `ExpectedEvidence` lo llevan |
| **M-02** | Corregida | La entrega nombra el commit «RED repair» (V28-03) |
| **M-03** | Corregida | C07 y C18 llevan la parte positiva de RR-3 (V28-04) |

**Comparar `RedSha` con `RedRepairSha`, y no la clase con la tabla, es lo correcto.** Corregir en la reparación una línea mal copiada de la tabla no es «el cuándo o el cómo de una lectura o de un gesto», sino cambiar una línea esperada de la prueba. RR-3 no lo admite, y debe ser C07. Que el RED copie bien la tabla ya lo garantiza el GREEN: una línea mal copiada no se cumple.

**El conjunto, cerrado aquí sí (no como M-05 de la §12 de CA-T2b):**
- las 49 líneas;
- los once exactos de las filas F;
- las doce líneas de CT-PBTC-07;
- la línea de «Aplicar» de CT-PBT2C-02.

**Faltan en la enumeración** la línea de `pb-08-redondeo` (`AnnotationScale=1.23456 PalletHeight=61.23456 sistema.AnnotationScale=1.23456`) y las dos de estilo (STANDARD-ISO y SIMPLEX). Son literales del §5.4 y no están entre los «exactos» ni las «doce» (M-09).

### 11.4 Lo nuevo no rompe lo cerrado

- **El seco:**
  - el alcance 3/173, `inv` 11, `stops` 33 y `required` 39 no cambian;
  - `evidence` pasa de 9 a 10, con la entrada de la verificación;
  - `authorities` sigue en 50.
- **Los 20 AVISOS:** son los 19 de la v2.7 más `v28-delta.md`, que llega con la emisión de PB-T2b. El seco es sobre `17872be8`, sin el cierre de PB-T2a ni de PB-T2b.
- **Lo cerrado en las §1 a §10 sigue igual:**
  - las fuentes de I-01 (`c = Pasada(...)`);
  - MU-T2C-03 por contenido, con V26-08;
  - la medición de la tabla;
  - la copia sin `porque:`;
  - las corridas aisladas;
  - el rango de autoridades, que el generador calcula de la última entrada de la revisión;
  - el `bounded_check` robusto.

### 11.5 Decisiones del Owner

**Ninguna nueva.**

### 11.6 Hallazgos nuevos

| Id | Sev. | Dónde | Hallazgo | Qué hacer |
|---|---|---|---|---|
| M-09 | M | El §6.0 (l.1125); la `ExpectedEvidence` de PB-T2c | **El problema:** la lista cerrada de líneas fijas no incluye la línea de `pb-08-redondeo` ni las dos de estilo de CT-PBT2C-01, que también son literales del §5.4 | Añadirlas: «los exactos, la línea de `pb-08-redondeo`, las dos de estilo, las doce de CT-PBTC-07 y la de «Aplicar»» |
| M-10 | M | El texto de la verificación (fuera del contrato) | Es M-06 de la §12 de CA-T2b: las comprobaciones de RR quedan a la lectura del Controller | Con una reparación, añadirlas por guion: RR-1; RR-2 y RR-6, con el tramo `RedSha..RedRepairSha`, que es solo la clase nueva, sin W ni WC; n.º 10 desde `RedRepairSha`; la comparación byte a byte; y los TRX de las corridas aisladas sobre la clase reparada |

## §12. Recomprobación sobre la v2.9

**Veredicto de §12: EMITIBLE. Nuevos: B 0 / I 0 / M 2.**

**El veredicto en breve:**
- **M-09 de la §11 está corregida,** y la lista de líneas fijas de la ayuda de RR-3 de PB-T2c queda completa y cerrada.
- **V29-02:** las ventanas del generador no relajan ninguna comprobación.
- **V29-03:** la custodia es coherente y conserva la línea de emisión de PB-T2b.
- **PB-T2b:** la v2.9 no cambia nada de lo que ya está emitido.
- **Las dos menores nuevas** son de precisión y de robustez del generador. No impiden emitir.
- **No hay decisiones nuevas para el Owner.**
- **M-10 de la §11 sigue fuera del paquete,** como dice `v29-delta.md`: es de la cadena, para el texto de la verificación.

**`BOUNDED_RESULT` final de PB-T2c (ASCII):**

`EMITIBLE TRAS CORRECCIONES (B 0 / I 1 / M 5; I-01 y M-01 a M-05 corregidas en el paquete; recomprobacion v2.6: EMITIBLE, B 0 / I 0 / M 3; M-06 a M-08 aplicadas; recomprobacion v2.8: EMITIBLE, B 0 / I 0 / M 2; M-09 aplicada en la v2.9; recomprobacion v2.9: EMITIBLE, B 0 / I 0 / M 2)`

Lo comprobé con la función real de `make_pbt2c_contract.py`, este informe completo, con esta §12, y las líneas de la v2.9 (`recheck_bounded_v29.py`, salida en `recheck_bounded_v29.txt`), y no da ningún error (§12.6).

### 12.1 Método y límites

- **Qué se leyó:**
  - del repositorio, solo objetos git de `59839c71` desde el checkout principal, que es la BASE de PB-T2b; su Worker está trabajando en el worktree (P-08);
  - del borrador: la v2.9 y `v29-delta.md`, el generador, su seco (sha `09a064fd…`, el del encargo), `neg-pbt2c.txt` (113 de 113 pruebas en negativo) y las notas.
- **Las comprobaciones:**
  - un `diff` del paquete custodiado en `59839c71` frente a la v2.9 local;
  - el guion de solo lectura `recheck_bounded_v29.py`, escrito con la herramienta de archivos y lanzado con `python -I`.
- **Qué no se hizo:** no compilé y no toqué el worktree. En esta ronda no hubo ninguna orden con `python -c`.

### 12.2 M-09: la lista de líneas fijas (punto 1)

**El texto** (§6.0, l.1137; el contrato en seco, en la entrada de la verificación de la `ExpectedEvidence`): «las líneas fijas de CT-PBT2C-01 y 02 (los exactos, la línea de `pb-08-redondeo`, las dos de estilo, las doce de CT-PBTC-07 y la de «Aplicar»)».

**Completa:** con esto están todas las líneas esperadas de CT-PBT2C-01 y 02:
- **CT-01:** once filas F, cada una una plantilla literal del §5.4 con su valor; la de `pb-08-redondeo`; y las dos de estilo;
- **CT-02:** las doce de cada ventana y la línea de «Aplicar» de la tercera ventana;
- **CT-03:** sus 49 líneas.

**Cerrada:** cada elemento se puede enumerar sin juicio. No queda ninguna cláusula abierta como la de D-21 en PB-T2b (M-05 de la §12 de CA-T2b).

**Si son literales del paquete:**
- **La de «Aplicar»** sí: está entera en el §5.4.
- **Las doce de CT-PBTC-07** no están escritas en el paquete. El paquete las fija **por referencia** a `PushBackDeclaredCorrectionSideBNoticeTests.cs:89-100`, una prueba existente que PB-T2c no puede tocar (C02). Comprobé en `59839c71` que esas doce líneas están en `:89-100`. A dos de ellas, la última de cada ventana, el paquete les añade ` descartes=1`.
- **La comparación de la regla** es entre `RedSha` y `RedRepairSha`, no contra el paquete. Así que fijarlas por referencia basta para la regla: su texto no puede cambiar en la clase.
- **Lo que conviene precisar:** para que el Controller enumere las cadenas exactas, conviene nombrar la referencia y el sufijo en la ayuda de RR-3. Lo es M-11.

### 12.3 V29-02: las ventanas del generador (punto 2)

**No relajan ninguna comprobación** (`make_pbt2c_contract.py:979-1022`):
- **La cabecera:** antes eran las 20 primeras líneas. Ahora son las líneas anteriores a `**Lo que pide el Owner**`, y el generador exige que esa marca aparezca **una sola vez** en las 40 primeras (`assert len(_ko) == 1`). Las aserciones de contenido de la cabecera (v2.4 a v2.9) no cambian, y el límite es semántico en vez de un número.
- **La fila de [OD]:** sigue siendo la misma línea literal entera (`'> ' + OD_ROW`), ahora buscada en las 40 primeras en vez de en las 24. Es el mismo literal, en un margen mayor.

**Lo que se quedó fuera (M-12):** la línea de IR1 se sigue buscando en las 12 primeras (`'\n'.join(PKG_LINES[:12])`, :1022).
- Hoy está en la l.8.
- La línea de emisión de PB-T2c la bajará a la 9, y una línea de cierre de PB-T2b en el Estado, a la 10.
- Cabe, pero con un margen de dos líneas y con el mismo riesgo que V29-02 quiere quitar: abortar en la emisión por la posición. Conviene buscarla en la cabecera (`_hdr`).

### 12.4 V29-03 y PB-T2b (puntos 3 y 4)

**El `diff` del paquete custodiado en `59839c71`** (la v2.8 con la línea de emisión de PB-T2b) frente a la v2.9 local da solo estos cambios:
- el título;
- el **Estado**;
- la línea nueva de la v2.9 en la cabecera;
- el registro v2.8 → v2.9 (V29-01 a V29-03);
- la línea de custodia;
- la ayuda de RR-3 de PB-T2c (l.1137).

**La línea de emisión de PB-T2b** («PB-T2b, emitida el 2026-11-19 … F4B-E36», l.6) se conserva sin cambios.

**La custodia:**
- en PB-T2b solo cambia «esta v2.8» por «la v2.8», un texto de historia;
- la de PB-T2c gana «esta v2.9» y `v29-delta.md`;
- conserva el encargo de CA-T2c y de su recomprobación.

**Nada de PB-T2b cambia:**
- el §5.3, el §6.2 y las líneas comunes de RR que lee el contrato de PB-T2b son idénticos;
- su contrato emitido está atado a su `AuthorityRevision` `a12ea09a`, con la v2.8, por 16.3, así que una corrección de PB-T2b seguiría leyendo la v2.8;
- `make_pbt2b_contract.py` solo cambió, antes de emitir, en los 48 bytes de la mención «; recomprobacion v2.8: EMITIBLE, B 0 / I 0 / M 2» del `BOUNDED_RESULT`.

**El seco sobre `59839c71`:**
- **Los AVISOS:** 14, siete del cierre de PB-T2b y siete de la emisión de PB-T2c. Desaparecen los de la emisión de PB-T2b y los de F4B-E35, que ya pasa `rr_check`.
- **Las cifras no cambian:** alcance 3/173, `inv` 11, `stops` 33, `required` 39, `evidence` 10 y `authorities` 50.
- **El rango de `f4b-dispositions.md`** llega a F4B-E36.

### 12.5 Decisiones del Owner (punto 5)

**Ninguna nueva.** Son una lista cerrada, márgenes del generador y custodia, sin cambio de comportamiento ni de lo que se informa al Owner.

### 12.6 El `BOUNDED_RESULT`, comprobado

`recheck_bounded_v29.py` aplica la función `bounded_check` de `make_pbt2c_contract.py`, sin ejecutar el resto del generador, a este informe entero y a las líneas de la v2.9. Da 0 errores con el literal de arriba.

Lo que hace pasar el literal:
- **El primer veredicto:** `EMITIBLE TRAS CORRECCIONES. B 0 / I 1 / M 5`.
- **La importante:** la fila `| I-01 | I |` y V26-01, con `**CA-T2c I-01**`.
- **Las recomprobaciones:** §9, §10, §11 y §12, emparejadas.
- **La última, §12:** EMITIBLE con B 0 e I 0, y nombrada en el literal (`recomprobacion v2.9: EMITIBLE, B 0 / I 0 / M 2`).

**Los dos controles** dan error, como deben:
- el literal sin la mención de la v2.9;
- el literal de la §11.

### 12.7 Hallazgos nuevos

| Id | Sev. | Dónde | Hallazgo | Qué hacer |
|---|---|---|---|---|
| M-11 | M | §6.0 (l.1137); la `ExpectedEvidence` de PB-T2c | **El problema:** «las doce de CT-PBTC-07» no son literales del paquete. Están fijadas por referencia a `PushBackDeclaredCorrectionSideBNoticeTests.cs:89-100`, y dos llevan además el sufijo ` descartes=1`. La regla sigue valiendo, porque compara `RedSha` con `RedRepairSha` | Escribir «las doce de CT-PBTC-07 (`PushBackDeclaredCorrectionSideBNoticeTests.cs:89-100`, con ` descartes=1` en la última de cada ventana)», para que se puedan enumerar sin interpretar |
| M-12 | M | `make_pbt2c_contract.py:1022` | **El problema:** la línea de IR1 se sigue buscando en las 12 primeras líneas, mientras V29-02 ensancha la cabecera y la fila de [OD]. Hoy cabe, en la l.8, pero con dos líneas de margen frente a las de emisión y de cierre | Buscarla en `_hdr`, la cabecera hasta «Lo que pide el Owner» |

## §13. Recomprobación sobre la v2.9, final sobre el cierre de PB-T2b

**Veredicto de §13: EMITIBLE. Nuevos: B 0 / I 0 / M 2.**

**Por qué el título dice «sobre la v2.9».** El encargo pedía «§13. Recomprobación final sobre el cierre de PB-T2b». Pero el `bounded_check` exige que todo encabezado `## …Recomprobación…` tenga la forma `## §<n>. Recomprobación sobre la vX.Y`, con texto libre detrás (`make_pbt2c_contract.py`, `h_rx`). Con el título literal, la emisión daría el error «un encabezado de recomprobacion no tiene la forma…». El título que uso conserva lo que se pedía y pasa.

**El veredicto en breve:**
- **V29-04 y V29-05** aplican M-11 y M-12.
- **V29-06 es fiel a F4B-E37** y a lo custodiado en `56cb026f`.
- **La remedición sobre el cierre no trae ningún cambio material:**
  - las anclas son las previstas;
  - los tres mutantes están por contenido y sin la parada de V26-08;
  - la CI base es la prevista;
  - los recuentos no cambian.
- **La tabla de CT-PBT2C-03** cumple la regla de tres casos.
  - De las seis «nada», tres son comparador ciego o dato igual, y tres son gestos inertes porque el catálogo solo ofrece una opción. El resultado de la tabla es correcto, pero esos tres controles quedan sin probar en el camino al borrador (M-13).
  - La clase temporal mide lo que dice el paquete.
- **Las dos menores nuevas** no impiden emitir. **No hay decisiones nuevas para el Owner.**

**`BOUNDED_RESULT` final de PB-T2c (ASCII):**

`EMITIBLE TRAS CORRECCIONES (B 0 / I 1 / M 5; I-01 y M-01 a M-05 corregidas en el paquete; recomprobacion v2.6: EMITIBLE, B 0 / I 0 / M 3; M-06 a M-08 aplicadas; recomprobacion v2.8: EMITIBLE, B 0 / I 0 / M 2; M-09 aplicada en la v2.9; recomprobacion v2.9: EMITIBLE, B 0 / I 0 / M 2; M-11 y M-12 aplicadas en la v2.9; recomprobacion v2.9: EMITIBLE, B 0 / I 0 / M 2 (seccion 13, final sobre el cierre de PB-T2b))`

Lo comprobé con la función real, este informe completo, con esta §13, y las líneas de la v2.9 final (`recheck_bounded_final.py`, salida en `recheck_bounded_final.txt`): 0 errores (§13.7).

### 13.1 Método y límites

- **Qué se leyó:**
  - del repositorio, solo objetos git de `56cb026f`, `03b16c88` y `59839c71` desde el checkout principal. No hay delegaciones activas;
  - del borrador: la v2.9 final y `v29-delta.md`, el generador, el seco real (sha `8f6f88c5…`, el del encargo), `neg-pbt2c.txt` (124 de 124), `pbt2c-anchors-out.txt`, la clase temporal `PushBackPbt2cCt03ProbeTests.cs.txt`, `measure_pbt2c.py` y el TRX `_work/trx/pbt2c-ct03.trx`.
- **El TRX:** su sha256, `a311c661…`, es el que cita la salida de los anclajes. Da `Passed` y trae 49 líneas `aplicado=`.
- **Las comprobaciones:**
  - el catálogo (`assets/catalogs/secciones.csv` y `connection-layout.csv` en `56cb026f`), para el porqué de las filas «nada»;
  - un guion de solo lectura para el literal, escrito con la herramienta de archivos y lanzado con `python -I`.
- **Qué no se hizo:** no compilé ni ejecuté pruebas, no toqué el worktree y no usé `python -c`.

### 13.2 V29-04 a V29-06 (punto 1)

- **V29-04 (M-11)** (l.77; §6.0, l.1140). La ayuda dice «las doce de CT-PBTC-07 (`PushBackDeclaredCorrectionSideBNoticeTests.cs:89-100`, con ` descartes=1` en la última de cada ventana)». El contrato en seco lo lleva en ASCII.
  - En `56cb026f`, `:89-100` siguen siendo las doce líneas.
  - El doble espacio de «con  descartes=1» en el JSON viene del sufijo, que empieza por un espacio. Es cosmético.
- **V29-05 (M-12)** (l.78). `make_pbt2c_contract.py:1072` busca la línea de IR1 en `_hdr`, toda la cabecera, «en toda la cabecera (v2.9, V29-05)».
- **V29-06 es fiel a F4B-E37** (l.79):
  - **La respuesta del Owner:** «Sonda neutra, luego K (Recomendado)», en `owner-config-snapshot-k-decision.md`.
  - **La sonda neutra:** `probe-R20261119T154313Z-2a9a` da `Pass` (F4B-E37 en `f4b-dispositions.md`).
  - **El registro:** `snapshot-K/snapshot-k.json` está en `56cb026f`, con `ConfigSha256` `E1F69847…322FE`, el que cita V29-06.
  - **En el contrato en seco:** P-01 dice «today Snapshot K, …/snapshot-K/snapshot-k.json», y su autoridad es la decisión de la K (UNIT_DOC). La vuelta antigua de la J (`pbt1b/owner-return-config.md`) queda como historia.
  - **PB-T2a y PB-T2b** se emitieron con la J, y no cambian.

### 13.3 La remedición sobre el cierre (punto 2)

- **El cierre:**
  - `56cb026f` es hijo de `03b16c88`, el GREEN, con la reparación del RED `6bc3497f` delante;
  - `git diff --name-only 03b16c88 56cb026f -- src tests` sale vacío;
  - `f4b-dispositions.md` llega a F4B-E38, «Cierre de F4B-PBT2B-WORKSPACE-MODE-WINDOW», con F4B-E37, la K, delante.
- **Las anclas:**
  - W cambió, como debía: su blob es `da66eba0`, el mismo en el GREEN y en el cierre;
  - WC (`1a3300b4`) y WX (`fb094bef`) siguen iguales;
  - en `src`, desde `161a0dad`, solo cambian los dos archivos de PB-T2a, el archivo del modo y W.
- **Los mutantes, leídos en el W del cierre:**
  - **MU-T2C-01:** W:3713, la línea literal de `Keep`, con su declaración en W:3712;
  - **MU-T2C-02:** WC:95, sin cambios;
  - **MU-T2C-03:** W:3273, `var result = draftPort.ApplyToDraft(PushBackEditableStateComparer.Copy(editable));`, en una sola línea física, dentro de `private void ApplyToDraft()` (W:3264). El fragmento sale una sola vez en todo W, y `Copy(appliedState))` ninguna. La parada de V26-08 no salta.
- **La CI base:** `44586045707`, 4/4. F4B-E38 la registra con Core 19 094 y UI 2 447 (2 426 pasadas y 21 omitidas), lo previsto. El GREEN de PB-T2c sigue en Core +0 y UI +3.
- **Los recuentos:**
  - las clases de PB-T2b tienen 3, 4, 1, 1 y 1 ejecutables;
  - las 31 familias son las de `17872be8`;
  - el seco lo dice: «recuentos de las familias == 17872be8: OK».
- **Los 7 AVISOS** son todos de la emisión: la v2.9 en HEAD, CA-T2c, la salida de los anclajes, `v29-delta.md`, el encargo, el seco y la entrada.

**Nada material cambia** frente a lo que preveía la v2.9.

### 13.4 La tabla de CT-PBT2C-03 (punto 3)

**El reparto:** aplica 42, nada 6 y se niega 1. Todas las filas tienen una de las tres colas, y la de «se niega» trae su `porque:`.

**Las seis «nada», una por una:**

| Fila | Diseño | Por qué no aplica | ¿Es lo que dice la regla? |
|---|---|---|---|
| `ToleranceBox=4` | `pb-01-base` | La tolerancia del fixture ya es 4, así que `Keep` conserva el valor (V24-11) | Sí: «un valor que se ve igual que el del fixture» |
| `SideSelectorBox=1` | `pb-20-ab` | Solo cambia la selección, que el comparador no mira | Sí: «la selección» |
| `FrontalSideBox=1` | `pb-20-ab` | El lado frontal es un campo de la ventana (`frontalSide`), no del estado | Sí: «el lado frontal» |
| `PostBox=otro` | `pb-01-base` | **Gesto inerte.** `Otro` elige el índice 1 solo si la caja tiene más de una opción; si no, vuelve a elegir el 0 (`PushBackWindowOnStateTests.cs:442`). En el catálogo hay un solo poste: `secciones.csv` solo tiene uno con rol `POSTE`, `POSTE_OMEGA_ATORNILLABLE_CON_TROQUEL_GOTA_DE_AGUA` | Solo en el resultado. El comparador sí mira `PostCatalogId` |
| `CellInOutBeamBox=otro` | `pb-01-base` | **Gesto inerte.** Las opciones son `CompatibleInOutBeams`, los largueros con punto `TROQUEL_CAMA` en `LATERAL`, y en `connection-layout.csv` solo hay uno, `LARGUERO_IN_OUT_C6` | Solo en el resultado |
| `CellInOutPeralteBox=otro` | `pb-01-base` | **Gesto inerte.** `AllowedPeraltes` de `LARGUERO_IN_OUT_C6` es solo «6» (`secciones.csv`) | Solo en el resultado |

**Lo que eso significa:**
- La cola de las seis es correcta, `aplicado=False igual-a-la-ventana=True diseno-igual=True descartes=0`, y no es un cuarto caso.
- Pero las tres últimas no prueban nada. Con el catálogo actual, el gesto no puede cambiar el valor, así que CT-PBT2C-03 no ejerce el camino al borrador de `PostBox`, `CellInOutBeamBox` ni `CellInOutPeralteBox`. En CLASSIC, CT-PBT1C-02 tampoco los ejercía.
- No es un defecto de WORKSPACE: `diseno-igual=True` dice que la ventana tampoco cambió su diseño.
- **Lo que hace falta (M-13):** declararlo para que nadie lea esas filas como cobertura.
  - Una línea `  porque: el catalogo solo ofrece una opcion` bajo cada una. `ct3_table_check` admite líneas `porque` tras cualquier fila y el Worker no las copia.
  - Un límite conocido en la entrega o en los arrastres.
  - Si el catálogo gana opciones, esas filas dejarán de ser «nada» y CT-PBT2C-03 lo detectará. Es lo que se espera de una caracterización.

**La que se niega, `StructureOverrideBox=6` sobre `pb-20-ab`:**
- **La cola:** `aplicado=False igual-a-la-ventana=False diseno-igual=False descartes=1`, el tercer caso.
- **El porqué:** `boton-habilitado=False`, el aviso del bloqueante y el estado «Lado A, frente 1, nivel 1: la cama necesita 396" y la estructura efectiva solo ofrece 300"…». Es el bloqueante `CellDoesNotFit` de `PushBackCompositeDiagnostics`, el que preveía el paquete.
- **El clic con el botón deshabilitado:** la prueba usa `S0.Click`, que hace `RaiseEvent` (EditorWindowTestSupport.cs:110-111) y llega al manejador aunque el botón esté deshabilitado. El `ApplyToDraft` niega, y el estado, ya cambiado, da `descartes=1`. La CT hará lo mismo.

**La clase temporal mide lo que dice el paquete** (`PushBackPbt2cCt03ProbeTests.cs.txt`):
- **Los gestos:** por reflexión de `PushBackWindowOnStateTests.Gestos()`. Son los mismos objetos, con `Nombre`, `Diseno`, `Compara` y `Pasos`, y en su orden. Filtra `Compara` y da 49, sin los cuatro de D2.
- **Las recetas:** las de `PushBackWorkspaceSupport` de `03b16c88`: `Estado(Fixture(...))`, `Borrador`, `VentanaBorrador` con `PuertoContado` y `CerrarVentana`.
- **La sonda del modo,** antes de cualquier gesto.
- **`aplicado`:** usa `actual` tomado justo antes del clic en `UpdateButton`, como dice el §5.0.
- **`igual-a-la-ventana` y `diseno-igual`:** se leen tras «Aplicar», con `DisenoDe(…, A)` frente a `DisenoDeVentana(w)`, como CT-PBT2B-02.
- **`porque`:** solo cuando se niega.
- **`descartes`:** el contador del sustituto para esa ventana, con la línea armada antes de `CerrarVentana`.
- **El hilo STA:** se arranca antes de sustituir el aviso, la lección de la reparación del RED de PB-T2b.
- **Los pasos que lanzan:** se vuelven líneas `EXCEPCION`, y `measure_pbt2c.py` rechaza una tabla que las tenga. Exige además el TRX `Passed`, las marcas de inicio y fin y el `ct3_table_check` del generador.
- **`measure_pbt2c.py`:** el arreglo de `TRX_NS` (`…/TeamTest/2010`) es el espacio de nombres del logger trx de vstest, y la hora sale en UTC con «Z».

**Dos observaciones, que no son hallazgos:**
- **La reflexión frente a la copia:** la clase temporal toma los gestos por reflexión, y el paquete pide que la CT los copie de la clase cerrada. Si una copia del Worker difiere, el GREEN no cumplirá la fila y será C07, así que la diferencia se detecta. Tomarlos por reflexión también en la CT quitaría ese riesgo de transcripción, pero sería otra versión del paquete.
- **El proyecto:** la clase temporal pasa `null` a `VentanaBorrador`, y el §5.4 no lo fija para CT-PBT2C-03. Ningún gesto de los 49 guarda en la biblioteca, que es lo único que lee `sourceProject`, así que el resultado no depende de eso.

### 13.5 Decisiones del Owner (punto 5)

**Ninguna nueva.**
- La Snapshot K ya es F4B-E37.
- Las filas inertes son un límite de cobertura que se declara, no un cambio de comportamiento.
- La custodia de la medición es procedimiento.

### 13.6 Hallazgos nuevos

| Id | Sev. | Dónde | Hallazgo | Qué hacer |
|---|---|---|---|---|
| M-13 | M | `pbt2c-anchors-out.txt`, las filas `PostBox=otro`, `CellInOutBeamBox=otro` y `CellInOutPeralteBox=otro`; la entrega o los arrastres | **El problema:** son «nada» porque el gesto es inerte: el catálogo solo tiene un poste, un larguero in/out y un peralte, no porque el comparador no los vea. La cola es correcta, pero esos tres controles quedan sin probar en el camino al borrador | Añadir bajo cada una `  porque: el catalogo solo ofrece una opcion`, que el Worker no copia y `ct3_table_check` admite, y anotar el límite de cobertura en la entrega de PB-T2c o en los arrastres |
| M-14 | M | `CUSTODY_EXTRA` (`make_pbt2c_contract.py:1431`) | **El problema:** el oráculo de las 49 filas sale de la clase temporal, de `measure_pbt2c.py` y del TRX `pbt2c-ct03.trx`, y no se custodia ninguno de los tres. La salida de los anclajes solo cita el sha del TRX. En PB-T2b sí se custodió `measure_pbt2b_t1c.py` | Custodiar con la emisión `PushBackPbt2cCt03ProbeTests.cs.txt`, `measure_pbt2c.py` y el TRX, o al menos su `StdOut` con su sha. El TRX lleva `computerName` y rutas, así que en el repositorio público va redactado |

### 13.7 El `BOUNDED_RESULT`, comprobado

`recheck_bounded_final.py` aplica la función `bounded_check` de `make_pbt2c_contract.py` a este informe entero, con la §13, a las líneas de la v2.9 final y al literal de arriba. La salida está en `recheck_bounded_final.txt` y da 0 errores.

**Lo que hace pasar el literal:**
- el primer veredicto, I-01 con su fila y V26-01;
- las recomprobaciones §9 a §13, emparejadas;
- la última, la §13, que es EMITIBLE con B 0 e I 0, y que el literal nombra (`recomprobacion v2.9: EMITIBLE, B 0 / I 0 / M 2`).

**El control:** con el título literal del encargo, el `bounded_check` da el error de forma del encabezado. Así queda probado por qué el título lleva «sobre la v2.9».

