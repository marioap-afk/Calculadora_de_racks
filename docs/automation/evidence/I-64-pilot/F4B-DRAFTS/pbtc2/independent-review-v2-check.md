# I-64 F4B — Comprobación acotada de la v2 del paquete del contrato de PB-TC2 y de su generador

**Veredicto: EMITIBLE.**
- **Recuento abierto:** 0 bloqueantes, 0 importantes y 3 menores (N-01 a N-03).
- **Hallazgos de la v1 (IRT2-1):** los tres importantes y los siete menores, RESUELTOS.
- **Decisiones nuevas para el Owner:** ninguna.
- **Los tres menores** se corrigen con una línea cada uno en la emisión, o se pueden dejar anotados. No bloquean.

**Citas:**
- **TC2v2:n** es una línea de `I-64-f4b-pbtc2-gate-contract-package-v2.md`.
- **GEN:n** es una línea de `make_pbtc2_contract.py`.
- **SEC:n** es una línea de `superseding-section.md`.
- **W**, **WC**, **WX**, **CS**, **S0** y **SC1** son los alias del paquete.
- Todo `ruta:línea` es de HEAD `5597c05b`.

---

## 1. Método y límites

**Lo que se revisó:**
- la v2 del paquete, con su registro de cambios;
- el generador;
- `superseding-section.md`;
- el apartado I de `pbtc2-measurements.md`;
- `v2-instructions.md`;
- mi informe de la v1.

**Contra qué se comprobó.** El código de HEAD, leído con `git -C`, `git grep` y `sed`, en solo lectura y con el árbol limpio.

**Las órdenes que se ejecutaron:**
- **El generador,** solo con la orden prescrita, `python make_pbtc2_contract.py 2026-11-18T00:00:00Z <40 ceros> <40 ceros> --dry --print`. Su salida se guardó fuera del worktree.
  - Después, `git status --porcelain` del worktree dio 0 líneas.
  - No apareció ninguna carpeta de PB-TC2 en `artifacts/orchestration/I-64/`.
- **Un script de Python de solo lectura,** escrito con la herramienta de archivos y ejecutado con `python -I`. Lee de [PKC-C] (`git show HEAD:`) las paradas PBTC-C11 y PBTC-C14, el `IssuedBy` y las autoridades, para compararlas con el generador.
- **Ninguna orden `python -c`.**
- **Las órdenes de INV-PBTC2-04,** ejecutadas sobre HEAD con `git grep -nP` y `git grep -nF`, para comprobar que funcionan y dan lo que dice la medición I-4.

**Límites:**
- no se compiló ni se ejecutó ninguna prueba;
- no se usó AutoCAD;
- las líneas esperadas y los fallos en la base salen de leer el código, como en el paquete;
- no se volvió a revisar a fondo el contenido de la v1 que la v2 no toca. Solo se hicieron comprobaciones puntuales.

---

## 2. Estado de los hallazgos de la v1

| Id | Estado | Evidencia en la v2 y en el código |
|---|---|---|
| **I-01** | **RESUELTO** | **Qué se corrige:**<br>• `MixedBoth` (TC2v2:205-206) es `IsMixed(field) && composite.ActiveSelection == PushBackSideSelection.Both && composite.SideBPresent`, exactamente la condición `mixed` de WC:452;<br>• `KeepCell` la usa (TC2v2:208-209);<br>• en `OptionalVal` solo cambia W:3667 (TC2v2:189), y W:3665, W:3666 y W:3668 quedan literales. La rama vacía conserva `IsMixed`, que es el defecto anterior M-07, sin cambios;<br>• la frase falsa de la v1 está reescrita (TC2v2:193-194);<br>• INV-PBTC2-03 y 04 están actualizados (TC2v2:487-490, :503-504), y el generador también (GEN:127-139).<br>**Por qué la condición basta.** Con B presente y «Ambos», el indicador está siempre fresco:<br>• `ActiveSelection` solo pasa a `Both` por `SideSelector_Changed`, y con B presente (CS:203-222; WC:163). Ese manejador llama a `LoadSelectedFront` → `LoadCompositeCellPanel`, que no sale porque B está presente (WC:604-607) → `ApplyMixedSideState`;<br>• retirar B fuerza `ActiveSelection = A` (CS:294-302);<br>• `Restore_Click` también: `SetSideBPresent(false)` y después la carga (W:3321-3326);<br>• `composite.Restore(snapshot)` solo lo usa el piloto (`PushBackPilotStateComparer.cs:70`);<br>• así que, mientras `MixedBoth` puede ser cierto, toda siembra va seguida de `ApplyMixedSideState`.<br>**La ventana negativa nueva** está rehecha en §3 |
| **I-02** | **RESUELTO** | Las órdenes de INV-PBTC2-04 (TC2v2:495-504; GEN:134-139) usan límite de palabra. Sobre HEAD, `git grep -nP '\bVal\(' -- <W>` da exactamente W:1905 y W:3634. `OptionalVal(` e `IntVal(` no coinciden, y PCRE funciona. `cref="Val"` da W:3641 y W:3683. `\bIsMixed\(` da W:3668. Las cuentas que se esperan tras el GREEN se rehicieron línea a línea; `git grep -c` cuenta líneas:<br>• `KeepOptional(`, 6: la línea de la declaración, las cuatro llamadas y W:3667;<br>• `KeepCell(`, 6;<br>• `MixedBoth(`, 3: la declaración, el cuerpo de `KeepCell` y W:3667;<br>• `IsMixed(`, 2: el cuerpo de `MixedBoth` y W:3668.<br>La regla de TC2v2:212, que prohíbe esos tokens en los `/// <summary>` nuevos, protege las cuentas, y ningún comentario de W los contiene hoy |
| **I-03** | **RESUELTO** | **El orden está fijado y es coherente en los tres textos:** D-PBTC2-08 (TC2v2:265-271), INV-PBTC2-07 (TC2v2:518) y GEN:144-150. Las tres últimas sentencias del `try` son: el texto en una variable, `Cerrar(w)` y `log.Add(texto + " descartes=" + descartes)`.<br>**Las ventanas que no cierra «Actualizar»,** que son la ventana 2 de CT-01 y las cuatro de CT-04, llevan el sufijo en su última línea (TC2v2:303, :345, :349, :353, :357).<br>**`Salida` se lee antes del cierre,** en la variable.<br>**El `Cerrar` del `finally` es idempotente** (S0:70-77) |
| **M-01** | **RESUELTO** | `SetMixed` WC:565-590; `ApplyMixedSideState` WC:445-503; `SideSelector_Changed` WC:147-180, con la llamada en WC:172; `ApplyTopology_Click` WC:215/237; `LoadSelectedModule` W:1803-1866; el refuerzo de `ApplyTo` en `:120-122`; y la salida de WC:604-607, que ya figura (H2-06, TC2v2:110-124). Todo comprobado |
| **M-02** | **RESUELTO** | `NumR`, `BoolR` y `StrR`, privados de cada clase, con la navegación de SC1:91-108 copiada (TC2v2:282-287; INV-PBTC2-07, TC2v2:523). Los miembros sin usar de la clase de CT-04 no dan avisos: no hay `EnforceCodeStyleInBuild` ni `TreatWarningsAsErrors` en `Directory.Build.*` ni en el csproj de UI |
| **M-03** | **RESUELTO** | **El cambio de celda tiene su ventana** (TC2v2:354-357), rehecha en §3.<br>**La línea 3 de la ventana mixta** imprime `A.largo` y `B.largo` (TC2v2:344).<br>**El alcance y los datos del frente** van a `KnownLimitations` (TC2v2:571; GEN:179) y a la red de PB-T1 (TC2v2:716). Es una de las dos salidas que proponía la v1.<br>**Queda un detalle nuevo:** N-01 |
| **M-04** | **RESUELTO** | **La base** es la salida real de la ventana tras el gesto, comparada con la fila a mano. Una diferencia es PBTC2-C07 (TC2v2:234; GEN:207).<br>**El catálogo** es `w.Session.Catalog`.<br>**Un número distinto de líneas del BOM** es STOP.<br>**Se transcriben** K4' y `MaxDelta`.<br>**El umbral** es el de «material» de OB-06.4 (más de 0,001, [P]:389), y lo dicen igual cuatro textos: TC2v2:230, :255, :643, :691-693; GEN:214; y EVID (GEN:171).<br>**El «menos de 0,0005"»** se declara como estimación. Con `MaxDelta` ≥ 0,0005 no se para: la sesión informa al Owner con el dato (TC2v2:259).<br>**La fila T2-2** confirma con `ConfirmModuleButton`, cuyo manejador hace commit y recalcula (W:2494-2503). Así, `R` lleva M2 manual de 47,992 |
| **M-05** | **RESUELTO** | **El riesgo está en la tabla de riesgos:** R2-08 (TC2v2:678).<br>**La variación de PB-TC está en las paradas:** se nombra en PBTC2-C11, tanto en el paquete (TC2v2:627) como en el contrato (GEN:196-198).<br>**Vuelve al Owner sin sustituto.** No se extiende la disposición de F4B-E08 (TC2v2:630) |
| **M-06** | **RESUELTO** | [OPT1]:7 cita solo `F4B-DRAFTS/pbt1/ob065-dossier.md`. No queda ninguna ruta local en los textos que se custodian (`grep`) |
| **M-07** | **RESUELTO** (como arrastre) | El arrastre a PB-T1 está anotado en:<br>• TC2v2:715;<br>• la precondición 3 (TC2v2:35);<br>• la parada PBTC2-C10 (TC2v2:592; GEN:210);<br>• la sección del registro (SEC:36).<br>La corrección de I-01 conserva el defecto literalmente |

---

## 3. Lo nuevo de la v2, atacado, y los hallazgos nuevos

### 3.1 `MixedBoth` frente a WC:452 y WC:604-607

**Correcto.**
- Su condición es literalmente la de WC:452.
- WC:604-607 exige además `SlotPresentText != null`, que solo es nulo antes de `InitializeComponent`.
- Todo lo de §2, fila I-01, se sostiene.

**`KeepOptional` y el `OptionalVal` nuevo, con valor y fuera de «Ambos»** devuelven lo mismo que la rama de hoy de W:3667: `sideValue` si se ve igual, y si no, `field.Value`.

### 3.2 Las dos ventanas nuevas de CT-04

**Ventana «sin lado B»** (TC2v2:350-353).
1. `IsChecked = false` dispara `Unchecked` → `SideBPresent_Changed` (WX:129; WC:126-145).
2. `SetSideBPresent(false)` pone `ActiveSelection = A`. El selector sale en WC:157-160.
3. Recalcula: la caja mixta vacía da `Keep` → 61,23456.
4. `w.SelectMatrixCell(0, 0, false)`:
   - `AllFieldsValid` pasa, porque la caja sigue opcional, el estado obsoleto de WC:580;
   - `CommitCurrentCell` da `Keep` → el exacto;
   - `ToggleCell` sin extender: limpia la selección y elige la celda (`DynamicFrontMatrix.ToggleCell`);
   - `LoadSelectedFront` siembra «61.235»; `LoadCompositeCellPanel` sale y el indicador sigue puesto;
   - `RequestRecompute` (W:1099).
5. En el GREEN v2, `MixedBoth` es falso → `Keep` → **61,23456**.
   - **En la base:** también 61,23456.
   - **Con el diseño de la v1:** 61,235.
6. La línea `sin lado B CellPalletHeightBox texto=[61.235] A.alto=61.23456 descartes=0` se cumple en la base y en el GREEN, y falla con el diseño de la v1.
7. **Es un negativo correcto.** `SelectMatrixCell` es `internal` y ya lo usan las pruebas (`PushBackCellConfigurationWindowTests.cs:104`).

**Ventana «cambio de celda»** (TC2v2:354-357).
1. `SetText` sin `LostFocus` deja `Value` = 61,235. `NumericField` valida en `OnTextChanged`.
2. `SelectMatrixCell(0, 1, false)` hace primero `CommitCurrentCell` (W:1094) con «Ambos» → `MirrorSelection` → `ReadCellValues` en A y en B, sobre (0,0).
   - **GREEN:** `MixedBoth` es cierto → 61,235 a los dos lados.
   - **Base:** `Keep` → A 61,23456 y B 61,235.
3. Después siembra (0,1). En `pb-20-ab`, A y B son iguales en el nivel 1, así que no queda nada mixto.
4. `A.alto` y `B.alto` leen `Levels[0]`. **GREEN:** `A.alto=61.235 B.alto=61.235`.
5. En la base esta ventana también difiere. El método ya falla antes, en la línea 3 de la ventana mixta, que es la primera diferencia (TC2v2:358).
6. **No es tautológica.**
7. **Queda un hueco:** N-01.

**Las otras dos ventanas,** rehechas con las líneas de la v2:
- **la mixta** (TC2v2:341-345): la línea 3 imprime el largo mixto vacío conservado, 100,00044 y 120;
- **la no mixta.**

Las dos se cumplen.

### 3.3 El orden exacto de `descartes=<n>`

**Correcto y sin latitud** (§2, fila I-03).
- Ninguna ventana de CT-04 ni la ventana 2 de CT-01 escenifica módulos.
- `OnClosing` no pregunta (W:3435-3459), así que n = 0.

### 3.4 Las órdenes de INV-PBTC2-04

**Correctas** (§2, fila I-02).
- Se ejecutan sin revisión, sobre el árbol en `CurrentSha`, que es lo que dice el texto.
- Las cuentas por línea casan con el código literal de D-PBTC2-05.

### 3.5 El umbral de M-04, y R2-08

**Coherentes en el paquete, en el contrato y en la evidencia** (§2, filas M-04 y M-05).
- El umbral de parada sigue la letra de OB-06.4.
- Por encima de la estimación, solo se informa.
- **Ninguno de los dos es una decisión nueva:**
  - las paradas que vuelven al Owner son las de F4B-E09, pregunta 4, «un efecto material sin medir», y lo «material» lo define OB-06.4;
  - R2-08 aplica [DP] tal cual.

### 3.6 El generador frente al §6 de la v2

**La salida en seco:** `DRY allowed 4 forbidden 129 (src 59 tests 53 root 17 ) inv 14 stops 31 required 18 evidence 8 authorities 30 bytes 37420`.

**Lo que está bien:**
- **Las entradas que cambian respecto a [PKC-C]:** WX es la 54, `PushBackAdvancedRackParametersWindowTests.cs` la 95 y `tests/RackCad.UI.Tests/Workspace/` la 112.
- **Los `assert` pasan.** Comprueban:
  - la forma del §6.3;
  - la existencia en HEAD;
  - la cobertura de `src/`;
  - que las rutas nuevas no existen;
  - que es ASCII;
  - que no hay términos de cierre;
  - que no hay rutas locales.
- **Autoridades, 30:** las 4 nuevas de la unidad:
  - el paquete;
  - las mediciones;
  - [OPT1];
  - el expediente.

  Además, 18 que existen, entre ellas el cierre de PB-TC, el registro, `owner-pbtc-nc1-decision.md` y `owner-pbtc-mixed-decision.md`. Y este contrato, W y WC como `UNIT_CHANGE`, y 5 externas. Las que todavía no están en HEAD son las que se custodian en la emisión, y no se comprueban.
- **`AllowedWriteScope` y `ForbiddenWriteScope`:** coinciden con §6.2 y §6.3.
- **Los 14 invariantes:** coinciden en sustancia con §6.5. GEN:127-133 añade, con razón, que W:3665, W:3666 y W:3668 quedan literales.
- **Las 18 filas de `RequiredTests`:** idénticas al §6.4. El `assert` de GEN:242-243 impide que el prefijo de PB-TC colisione.
- **Las 31 paradas:**
  - las 17 estándar se comprueban iguales a las de PB-TC (GEN:34);
  - las 14 propias coinciden con §6.7.
- **PBTC2-C11:** su texto es idéntico al PBTC-C11 emitido, salvo los identificadores y la mención de la variación de PB-TC. Lo comprobé con el script contra [PKC-C].
  - **Lo literal de PD-F4-04** es el inglés original.
  - **PD-F4-01** va en inglés por la restricción ASCII, como en PB-TC. Lo literal en castellano está en §6.7 del paquete, al que el contrato remite.
- **Las celdas:** se comprueban iguales a las de PB-TC (GEN:246).
- **`IssuedBy`:** es el texto de la instrucción (GEN:303-306).

**Ningún hallazgo.**
- **La línea `WT`** (GEN:9) lleva la ruta del perfil y se redacta al custodiar, como dice la precondición 3.

### 3.7 `superseding-section.md`

**Es fiel y está lista para un repositorio público:**
- no tiene rutas locales ni nombres;
- sus autoridades son rutas del repositorio;
- tiene el estado DECLARADA y la tabla de ejecución pendiente;
- tiene las dos notas de cierre (SEC:47-53);
- tiene los pendientes de PB-T2 y de PB-T1.

**Dos detalles:** N-02 y N-03.

### 3.8 Hallazgos nuevos

| Id | Gravedad | Contenido | Evidencia | Corrección |
|---|---|---|---|---|
| N-01 | MENOR | **`seleccion(0,1)` no tiene fuente.** Esa parte de la línea de la ventana «cambio de celda» no está definida en la notación de CT-04. Si el Worker la escribe como literal, esa parte es tautológica (M-02). Por ser un ayudante, tampoco puede ser un cuarto ayudante nuevo (INV-PBTC2-07) | TC2v2:335-339 y TC2v2:357 | Definirla en la notación de CT-04: `seleccion(<f>,<l>)` = `w.State.Structure.SelectedFrontIndex` y `SelectedLevelIndex`, leídos en línea. `State` ya lo usan las pruebas (`BlankFrontEditorTests.cs:333`) |
| N-02 | MENOR | **La sección del registro se queda corta al declarar el cambio.** Dice «Vale al salir de la caja y al cambiar de celda». Pero la regla también vale en el alcance y en los datos del frente (TC2v2:194; W:1346, W:1510), que solo no tienen prueba. Un registro de cambios declarados debe nombrar todos los sitios donde cambia el comportamiento | SEC:28 frente a SEC:35 y TC2v2:194 | En SEC:28: «Vale al salir de la caja, al cambiar de celda, al aplicar a un alcance y con los datos del frente (estos dos, sin prueba propia; ver "Lo que sigue pendiente")» |
| N-03 | MENOR | **La lista general de pendientes de PB-T2 no se actualiza.** La obligación de PB-T2 de PB-TC2 está en la sección nueva (SEC:33) y en `f4b-dispositions.md`, pero no en «## Pendientes de PB-T2» de [SC] (`:169-173`). Quien lea solo esa lista no la verá | [SC]:169-173; SEC:33 | Añadir una viñeta a esa lista en la emisión, o decir en SEC:33 que la lista general sigue siendo la de PB-TC |

---

## 4. Veredicto

**EMITIBLE.**
- **Recuento abierto:** 0 bloqueantes, 0 importantes y 3 menores (N-01 a N-03). No bloquean.
- **Lo que está bien:**
  - los diez hallazgos de IRT2-1 están resueltos;
  - las correcciones son correctas frente al código de HEAD;
  - las ventanas nuevas de CT-04 se cumplen en el GREEN por lectura, y la negativa pasa en la base;
  - el generador produce, en seco, el contrato del §6, sin escribir nada.
- **Lo recomendable antes de emitir:**
  - definir el origen de `seleccion(<f>,<l>)` (N-01);
  - completar SEC:28 (N-02).

**Decisiones materiales nuevas para el Owner: ninguna.**
- **`MixedBoth`** solo concreta que «Ambos lados» existe con el lado B presente.
- **El umbral** sigue la letra de OB-06.4, y la estimación dada al Owner solo da lugar a informarle.
- **R2-08** aplica [DP] tal cual.
- **M-07** va al paquete de PB-T1.
