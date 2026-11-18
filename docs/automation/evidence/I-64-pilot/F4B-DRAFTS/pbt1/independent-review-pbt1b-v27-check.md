> **Redacción para el repo público:** sin rutas locales ni nombres de usuario o de máquina. El worktree se cita como `<worktree-de-I-64>` y las rutas del repo son relativas.

# I-64 F4B — Comprobación acotada de la v2.7 de PB-T1 (CT-PBT1B-02 y CT-06 tras la parada PBT1B-C15)

**Veredicto:** **EMITIBLE — B/I/M = 0 / 0 / 4. Decisiones materiales nuevas para el Owner: ninguna.**

- **CT-PBT1B-02 aplica F4B-E20 sin ir más allá:**
  - la mitad estado → diseño → estado se mide desde el estado ya normalizado (paso iii);
  - en los seis racks antiguos, la primera vuelta vista desde el estado es exactamente `LegacyDeltas` vista desde el estado (paso ii);
  - las líneas de (ii) salen de la medición del Worker;
  - no esconde ninguna pérdida: el frente 0 del lado B de `pb-15` no es alcanzable ni siquiera declarando B después.
- **La aclaración de CT-PBT1B-06 tiene sus anclas en el código y no debilita D2.** Ninguno de los fallos de `ct06-failures.txt` es un defecto de la copia.
- **La excepción de PBT1B-C15 cubre solo la normalización conocida.** Cambiarla era necesario: sin ella, la línea esperada de CT-02 dispararía la parada.
- **El generador coincide con el §6.2 de la v2.7.** Además del `Objective` y de `IssuedBy`, cambian solo `Authorities` (+2) y el texto de PBT1B-C15. Las dos cosas son coherentes.
- **Los cuatro menores son de texto:**
  - la custodia del §6.0 no está al día;
  - CT-08 no excluye `SafetyFacePiece` como hace el piloto;
  - F4B-E20 no está en el §1 ni en el §8.2;
  - falta precisar el formato de la línea de `pb-15`.

  Se pueden aplicar en el mismo commit de emisión. Ninguno pide al Owner.

## 1. Método y límites

**Las entradas:**
- la v2.7 (`T27:n`), con su registro de cambios v2.6 → v2.7, comparada línea a línea con la v2.6 (el diff tiene 7 bloques);
- la v2.6 custodiada en HEAD, que es la v2.6 local más la línea «emitida»;
- F4B-E20 y la vuelta (`pbt1b/owner-pbt1b-c15-decisions.md` y `owner-return-c15.md`);
- `make_pbt1b_contract.py`, comparado con el generador custodiado en HEAD, y `make_pbt1b_contract-dry.txt`.

**La entrega del Worker,** leída en `<worktree-de-I-64>`, en `artifacts/orchestration/I-64/F4B-PBT1B-LOSSLESS-STATE-TYPE/1/R20261118T193423Z-fb01/`:
- `tests/dev/ct02-lines-expected-vs-actual.txt`, `pb15-state-diff.txt`, `ct06-failures.txt` y los logs `d1` a `d3`;
- del borrador `uncommitted-work-at-stop.patch`, solo lo que hacía falta: el bucle de CT-02, el de CT-06, la batería de CT-08, y `Copy` y el comparador para `Advanced`.

**Contrastado en `<worktree-de-I-64>`,** HEAD `7905b3a9`, con `git -C` y en solo lectura. `git status --short`, vacío antes y después. Leí:
- CS:269-470, CS:970-1015 y CS:1090-1105;
- ES:235-247 y ES:495-584;
- `DynamicFrontMatrix.cs` (`SetActive` :194-215, `Snapshot` y `Restore` :369-379);
- `SafetyFacePiece.cs:20-46` y `SelectivePalletDesign.cs:596-612`;
- `WorkspaceDraftPilotStateTests.cs`: :106, :437-466, :525-537, :985-1110 y :1153-1245;
- `WorkspaceDraftPilotMaterializationTests.cs:740-750`.

**Lo que ejecuté:**
- **El generador,** solo con la orden literal del encargo (`… --dry --print`). La salida es idéntica a `make_pbt1b_contract-dry.txt`: `allowed 12 forbidden 213 (src 117 tests 79 root 17) inv 15 stops 33 required 23 evidence 8 authorities 51 anchors-ok 39 skeleton-in-HEAD vigente bytes 56536 sha256 1772E664…EF22`.
- **Mis scripts de solo lectura,** escritos con la herramienta de archivos y ejecutados con `python -I` y la entrada cerrada (sin `-c` y sin heredoc):
  - `rv-b/dump_pbt1b_json.py`, que vuelca el JSON del generador en seco;
  - `rv-b/skel_v25_vs_gen.py`: esqueleto, título y `Objective` de la v2.7 frente al generador;
  - `rv-c/cmp_v26_v27_contract.py`: el contrato custodiado de la v2.6 frente al JSON de la v2.7, campo a campo.

**Límites:**
- Nada se compiló ni se ejecutó.
- Las líneas de (ii) se apoyan en una medición hecha con el borrador del Worker (su comparador y su copia), que no está revisado. El GREEN las vuelve a medir, con C07 y C15.
- `normalizada-iguales=True` es lectura (§2).

## 2. CT-PBT1B-02

**Aplica F4B-E20 tal cual** (T27:891):
- **(iii)** es «se mide desde el estado ya normalizado por una primera ida y vuelta»: S2 frente a S3.
- **(ii)** es «la diferencia de esa primera vuelta es exactamente la de `LegacyDeltas` vista desde el estado»: S1 frente a S2.
- **(i)** no cambia y sigue con el oráculo de `LegacyDeltas` (WDPM:746).

**Que (ii) cubra los 18 diseños no va más allá** (punto declarado por el redactor):
- Para los 12 que no son antiguos, su `LegacyDeltas` es vacío, así que «vista desde el estado» es «ninguna».
- Es además lo que la v2.6 ya exigía a esos 12 («estado → diseño → estado» iguales). Quitarlo debilitaría la red.
- Para los cinco antiguos que normalizan al cargar, «ninguna» está medido (`iguales=True`).

**Las líneas, frente a lo medido:**
- **El borrador del Worker medía exactamente S1 frente a S2:** `first.State` frente a `second.State` cargado de `first.Computation.Design`, en el bucle de CT-02 del parche. Su salida: `iguales=True` en 17 y `False` solo en `pb-15-sin-composite`.
- **El detalle de `pb-15`** (`B0 active True False`, `SideBPresent False False`, `ever False False`) es el de `pb15-state-diff.txt`.
- **(i)** coincide con `LegacyDeltas` en los 18.
- **No se inventa nada medible.** (iii) no está medido, pero es la obligación misma del Owner, con C15 si no se cumple. Por lectura, S2 → D2 → S3 repite un proceso que ya parte de un diseño canónico.

**`normalizada-iguales=True` (punto declarado: «no está medido»). Es legítimo:**
- es la condición que hace comprobable la premisa del Owner («solo cambia el frente 0 del lado B»), y si no se cumple es C15 y vuelve al Owner;
- **la lectura dice que da `True` si esa premisa es cierta:**
  - `SetSlotPresent(B, 0, false)` es la misma llamada que hace la carga (CS:1006-1009);
  - llega a `ES.SetActive` (ES:238-246), que solo cambia `IsActive` y la selección de la matriz (DFM:209-213);
  - la selección no la compara el comparador (D-PBT1-06);
  - `SyncPushConfig` no cambia ninguna forma, porque el número de niveles no se mueve;
  - y `OtherSideHasAnyFront(B)` es cierto, porque A tiene tres frentes.
- **Lo único que no se ha medido** es que no haya otra diferencia. `pb15-state-diff.txt` no lista todos los campos, y esa es justo la comprobación que hace la línea. Pido escribir estas anclas (PBT1BC27-M04).

**Discrimina:**
- (i) da el diseño por defecto sobre el esqueleto.
- (ii) y (iii) usan el comparador, que sobre el esqueleto es `ReferenceEquals` y da `False`.
- Una carga o una copia que pierda algo sale en (i) o en (iii).
- La columna del RED (T27:891) es correcta.

**Ninguna pérdida escondida** (lo comprobé, porque es la pregunta del Owner con otra cara):
- **El frente 0 del lado B de `pb-15` no lo lee nadie con B no presente:**
  - `IsSlotPresent` devuelve `false` para B (CS:430-433);
  - `AbsentSlotsOfB` escribe `[0]` desde S1 y desde S2, así que D1 y D2 son iguales;
  - `OtherSideHasAnyFront(A)` ignora a B (CS:460-463).
- **Tampoco aparece si el usuario declara B después.** Con `sideBEverDeclared = false`, que es el caso de `pb-15` («ever False False»), `DeclareSideBCapability` llama a `SideB.LoadNew()` y deja todas las ranuras de B inactivas (CS:356-361 y CS:381-385). La diferencia se borra.
- **La ventana de hoy hace lo mismo al escribir:** la línea `AbsentSlotsB[0]` de `LegacyDeltas` lo muestra.
- **El marco de la pregunta al Owner** («no se dibuja ni se usa») **es correcto y completo.**

## 3. CT-PBT1B-06

**Las exclusiones tienen sus anclas, y las comprobé:**
- **(5a), las ranuras de forma:**
  - `ES.Restore` llama a `SyncPushConfig` (ES:503);
  - `SyncPushConfig` (ES:552-584) recorta `pushFronts` al número de frentes de la matriz (ES:564) y ajusta las celdas a `LoadLevels` (ES:581-582);
  - la matriz vuelve con clones (DFM:369-379, ES:497).
- **(5b), `SafetyFacePiece`:**
  - constructor privado y propiedades de solo lectura (`SafetyFacePiece.cs:26-44`);
  - `None` es una instancia única (`:35`), y `Of` lleva un id en blanco a `None` (`:38-39`);
  - `DeepCopy` comparte `NearFace` y `FarFace` (`SelectivePalletDesign.cs:605-606`);
  - el piloto la excluye por lo mismo (`WorkspaceDraftPilotStateTests.cs:1090-1091` y `:1103-1104`).

**(2), (3) y (4) son lo que el Worker ya aplicó en su borrador:**
- los contenedores `Editor`, `Pallet` y `Advanced`;
- `AdvancedSix` en `EditableSlots`;
- la `Copy` y el comparador de `Advanced`, solo con los seis avanzados.

La v2.7 los fija en el texto. Son coherentes con D-PBT1-02, D-PBT1-05 (paso 4) y D-PBT1-06.

**No debilita el censo ni la condición 9 de D2:**
- **El censo** (todo campo clasificado y toda fila existente) no cambia.
- **La condición 9 es CT-08,** que no toca esta aclaración.
- **La forma la siguen ejerciendo:**
  - (1), con `Copy(R) == R` sobre un estado rico con formas distintas de las de por defecto (F-01; `pb-20-ab` sin B);
  - las operaciones por API de CT-08 (`SetSlotCount`, `ApplyTopology`, `SetSlotPresent`…);
  - y CT-09.
- **La sustitución de la pieza de cara** la ejercen las ranuras `NearFace` y `FarFace`: el `Mutator` las sustituye con `SafetyFacePiece.Of(…)`, `:1209`.

**Los fallos de `ct06-failures.txt`: ninguno es un defecto de la copia.**

El `Mutator` cambia un entero sumando 1 y una colección añadiéndole un elemento sin inicializar (`:1199-1243`). Así deja la matriz y la configuración paralela desalineadas, un estado que el API no produce. `Copy` lo vuelve a alinear. Fallo a fallo:

| Fallo | Explicación | ¿Defecto real? |
|---|---|---|
| `LoadLevels`, «no es igual» (×4), sin «perdió» | La copia lleva el nivel nuevo, pero `SyncPushConfig` alarga las celdas de `pushFronts`, y R no | No: la copia lleva la mutación y además la versión alineada |
| `fronts`, «no es igual» (×2), sin «perdió» | La copia lleva el frente añadido, y `SyncPushConfig` añade su `pushFronts` | No, por lo mismo |
| `Cells`, «no es igual» y «perdió» (×4 cada uno) | `TrimToLevelCount` (ES:582) quita la celda que sobra | No: esa celda solo existe fuera del invariante del API |
| `pushFronts`, «no es igual» y «perdió» (×2 cada uno) | `RemoveRange` (ES:564) quita el frente que sobra | No, por lo mismo |
| `SafetyFacePiece.ElementId` y `.IsNone`, «perdió» (×4 cada uno) | B, R y la copia comparten la pieza inmutable, así que la reflexión los cambia a la vez | No: es compartida a propósito |

**Además, lo que no aparece descarta una copia que pierda contenido:**
- ningún campo de valor de una celda o de un frente (peraltes, topes, fondos) sale como «perdió»;
- y (1), `Copy(R) == R` sin mutar, pasó.

## 4. PBT1B-C15

**La excepción solo cubre la normalización conocida** (T27:1157 y la C15 del generador, con el mismo contenido en inglés):
- un diseño: `pb-15-sin-composite`;
- un paso: la primera vuelta, (ii);
- un cambio: el frente 0 del lado B de activo a inactivo, con B no presente, y nada más (`normalizada-iguales=True`);
- su línea de `LegacyDeltas`.

**Sigue siendo C15:**
- cualquier otra diferencia, también en `pb-15`;
- `normalizada-iguales=False`;
- cualquier diferencia en (iii);
- y CT-PBT1B-01 entero.

**Cambiarla en el generador no va más allá** (punto declarado por el redactor). Sin la excepción, la línea esperada `iguales=False` de CT-02 cumpliría la condición de la parada («CT-PBT1B-01 o 02 da una diferencia…»), y el Worker tendría dos órdenes contradictorias. Es la consecuencia directa de «Normalización conocida: no es una pérdida».

## 5. El generador

**El JSON en seco frente al contrato custodiado de la v2.6, campo a campo:**
- **Iguales:** `Schema`, `Unit`, `Gate`, `TaskId`, `AllowedWriteScope` (12), `ForbiddenWriteScope` (213), `Invariants` (15), `RequiredTests` (23), `ExpectedEvidence` (8), `EligibleCells`, `RoutingEnforcement` y `CorrectionsAuthorized`.
- **Cambian,** además de `AuthorityRevision`, `MainSha` e `IssuedUtc`, que son argumentos:
  - **`Objective`:** «v2.6» → «v2.7». Es idéntico al de T27:1105 (comprobado por script).
  - **`IssuedBy`:** añade F4B-E20.
  - **`Authorities` (49 → 51):** entran `owner-pbt1b-c15-decisions.md` (F4B-E20) e `independent-review-pbt1b-v27-check.md`. Cambian solo los textos `Section` del paquete (v2.7) y de la comprobación anterior.
  - **`StopConditions`:** solo el texto de PBT1B-C15 (§4).

**El resto, comprobado:**
- **El esqueleto, el título y la custodia:** el esqueleto sigue siendo el vigente (comprobado por script); el título «v2.7» se reconoce. Al escribir, exige la v2.7 custodiada, las tres rutas de `CUSTODY` y las cuatro de `CUSTODY_EXTRA`. Dos de esas cuatro son la medición del Worker, en `I-64-pilot/F4B-PBT1B-LOSSLESS-STATE-TYPE/1/R20261118T193423Z-fb01/tests/dev/`, la misma forma con que se custodiaron las corridas de PB-T1a.
- **Recuentos, CI e invariantes, sin cambio.** Es lo que dice el registro de cambios («Ningún cambio toca el alcance, el esqueleto, los recuentos, la CI, D2 ni RACKEDITAR»), y es cierto: CT-02 y CT-06 siguen siendo un método cada uno.

## 6. Los hallazgos

| Id | Gravedad | Contenido | Evidencia | Corrección |
|---|---|---|---|---|
| **PBT1BC27-M01** | MENOR | **El §6.0 no está al día con la v2.7** (punto declarado por el redactor). Dice «esta v2.6», «las cuatro rutas de `CUSTODY` … el paquete en la v2.6», cuando el generador tiene tres rutas de `CUSTODY` (el paquete en la v2.7, F4B-E20 y la comprobación de la v2.7) y cuatro de `CUSTODY_EXTRA`, con las dos de la medición del Worker. Además, «La comprobación acotada — Sobre qué» sigue en «v2.5», y «Resultado» no nombra `independent-review-pbt1b-v27-check.md` | T27:985, :987, :993; `make_pbt1b_contract.py:446-456`, :527-528 | Reescribir la viñeta de PB-T1b de «Custodia» con la v2.7 y las listas reales del generador. En «Sobre qué», «v2.7 para PB-T1b (la v2.5 en su primera emisión)…». En «Resultado», añadir `independent-review-pbt1b-v27-check.md` |
| **PBT1BC27-M02** | MENOR | **CT-08 (y la condición 9 de D-PBT1-15) no excluye los campos de `SafetyFacePiece`, y el piloto sí** (`WorkspaceDraftPilotStateTests.cs:1103-1104`, en su prueba equivalente, la de mutar la copia). CT-08 pasa igual (`d3.log`: 6 de 7 en verde; solo falla CT-06), pero con esos campos pasa **sin comprobar nada**: el original, su copia previa y la copia comparten la pieza inmutable, así que la mutación por reflexión se ve en todos a la vez. Además, si la pieza es `SafetyFacePiece.None`, la reflexión muta un singleton de todo el proceso mientras dura, a la vista de otras pruebas de Core que corren en paralelo | T27:897, la condición 9 de D-PBT1-15; la batería de CT-08 del borrador (`EditableSlots` sin exclusión); `SafetyFacePiece.cs:35` | Añadir a CT-08 y a la condición 9 la exclusión de CT-06 (5)(b), con sus mismas anclas: «los campos de `SafetyFacePiece`, inmutable y compartida a propósito; su sustitución la ejercen `NearFace` y `FarFace`». No debilita la condición 9: lo que el Owner pide no mutar es el estado editable ordinario, y una pieza inmutable compartida no lo es (D2, condición 2, con `IsImmutable`) |
| **PBT1BC27-M03** | MENOR | **F4B-E20 no está en el §1 (Autoridades) ni en el §8.2 (Ya tomadas),** aunque el generador ya la tiene como autoridad. La vuelta `owner-return-c15.md` no está entre las autoridades del generador, y la de PB-T1a sí lo estaba (`owner-return-pbt1a.md`) | T27:216-218 (el §1 termina en F4B-E16); §8.2; `make_pbt1b_contract.py`, `EXISTING` | Añadir una fila de F4B-E20 al §1 y una viñeta al §8.2 («Normalización conocida» → CT-PBT1B-02 en tres pasos y la excepción de PBT1B-C15; «Sin gastar intento» → intento 1). Si se quiere el paralelo con PB-T1a, añadir `F4B-DRAFTS/pbt1b/owner-return-c15.md` a `EXISTING`; pasan a 52 autoridades y hay que ajustar el `assert` |
| **PBT1BC27-M04** | MENOR | **La línea (ii) de `pb-15` se da solo por su ejemplo.** No dice que su sufijo se escriba siempre para `pb-15`, ni de dónde salen sus valores. `normalizada-iguales=True` se declara «condición, no medición», pero no lleva la lectura que la respalda, así que un `False` no se distinguiría de un artefacto de la prueba | T27:891 | Precisar: «para `pb-15-sin-composite`, la línea lleva siempre `SideB.Fronts[0].IsActive=<de S1>-><de S2>` (`Editor.SideB.Structure.IsActive(0)`) y después `normalizada-iguales=<b>`». Añadir la lectura: `SetSlotPresent` → `ES.SetActive` (ES:238-246) → `DynamicFrontMatrix.SetActive` (:194-215), que solo cambia `IsActive` y la selección, que el comparador no mira, más un `SyncPushConfig` sin cambio de forma |

## 7. El veredicto

**EMITIBLE — B/I/M = 0 / 0 / 4.**

**Se puede emitir sin volver al Owner** (F4B-E09, pregunta 4; F4B-E11; F4B-E20):
- no hay ningún bloqueante ni ningún importante;
- los cuatro menores son de texto, y recomiendo aplicarlos en el mismo commit de emisión. PBT1BC27-M01 es el más útil, porque hoy el §6.0 contradice al generador;
- el contrato que sale del generador es correcto tal como está;
- si se aplica PBT1BC27-M03 en el generador, cambian el recuento de autoridades y su `assert`.

**Decisiones materiales nuevas para el Owner: ninguna.**
- **CT-02** aplica literalmente las dos mitades de F4B-E20, pregunta 1. Que (ii) cubra los 18 diseños mantiene una garantía que ya existía.
- **CT-06** es la aclaración que F4B-E20 encargó: sus exclusiones salen del código y del piloto.
- **La excepción de C15** es la consecuencia necesaria de «no es una pérdida», limitada a esa normalización.
- **«Sin gastar intento»** ya está decidido.
- **Nada cambia** el alcance, el esqueleto, los recuentos, la CI, D2 ni RACKEDITAR.
