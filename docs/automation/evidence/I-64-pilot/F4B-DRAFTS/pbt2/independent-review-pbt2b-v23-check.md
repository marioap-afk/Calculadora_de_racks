# I-64 F4B PB-T2b: comprobación acotada sobre la v2.3, antes de emitir

**Veredicto: EMITIBLE TRAS CORRECCIONES. B 0 / I 3 / M 7.**

- **Decisiones nuevas para el Owner:** ninguna (§7).
- **La condición de F4B-E28, pregunta 4:** se cumple cuando se cumplan las tres cosas siguientes:
  - I-01, I-02 e I-03 están corregidas en el texto del paquete (una v2.4, o la v2.3 enmendada y custodiada);
  - la remedición del §6, sobre el cierre de PB-T2a, no trae ningún cambio material;
  - PB-T2a no llega a su intento 3.
- **Las tres importantes:** son recetas de prueba sin precisar y no cambian el diseño. Pero cada una deja una línea esperada que, con una lectura natural del texto, no se cumple en el GREEN. Es la clase de defecto que costó el intento 1 de PB-T1c y el de PB-T2a (F4B-E30, F4B-E33).
- **`BOUNDED_RESULT`:** el generador exige ASCII y que empiece por `EMITIBLE`. El literal propuesto, una vez aplicadas las correcciones, es este:
  `EMITIBLE TRAS CORRECCIONES (B 0 / I 3 / M 7; I-01, I-02 e I-03 corregidas en el paquete)`

## 1. Método y límites

- **Qué se leyó:**
  - el repositorio, solo por objetos git desde el checkout principal: `17872be8` (HEAD), `c33ac6ac`, `0dc623ca`, `5a8366c1` y `161a0dad`;
  - en el borrador: el paquete v2.3 y `v23-delta.md`, `make_pbt2b_contract.py` con `generator-notes-pbt2b.md`, `dry-pbt2b.txt`, `contract-dry-pbt2b.json`, `pbt2b-anchors-out.txt` y `measure_pbt2b_t1c.py`.
- **Qué no se hizo:** ni compilar, ni ejecutar pruebas, ni tocar el worktree.
- **Una incidencia de procedimiento, declarada:**
  - una orden de Bash llevó por error `python -I -` con un heredoc vacío, el mismo error que declara el generador en su §6;
  - el proceso quedó en espera y se paró con TaskStop; no leyó ni escribió nada;
  - incumple la letra de «nunca con un heredoc» y no se repitió.
- **Límites:**
  - todo es por lectura: la ventana de WORKSPACE no existe en la base;
  - las premisas de UI se contrastan con el código de la base y con las clases cerradas que el paquete copia.

## 2. Fidelidad del contrato en seco (punto 1)

| Lección o regla | En el contrato en seco | Estado |
|---|---|---|
| **C13 por nombre** (R-PBT2-12) | PBT2B-C13 lista los 26 nombres y el constructor privado como único constructor nuevo. El seco dice «nombres de PBT2B-C13 libres: 26»: ninguno existe en la base | ✔ |
| **Las notas fuera del contrato** | «INV propios literales sin su nota» (seco); INV-PBT2B-08b aparte (D-7) | ✔ |
| **El `Trailer` real** (R-PBT2-15) | En la `ExpectedEvidence` común | ✔ |
| **`build-plugin` omitido en el RED** (16.9 n.º 9; R-PBT2-13) | `needs = ['tests', 'build-ui']` medido. La `ExpectedEvidence` del RED lo dice | ✔ |
| **La regla del configurador** (V23-01) | INV-PBT2B-10 con la atribución; C09 con su nota | ✔ (D-4) |
| **Comparar tras el recálculo neutro** (F4B-E30, V23-02) | El momento de cada comparación con el diseño: CT-02 compara tras `Recalcular`; CT-04 y CT-PBT2C-03, tras «Aplicar»; CT-05 `bloqueante`, con un `LostFocus` en `AnnotationScaleBox` antes de `UpdateButton` | ✔ |
| **W es `UNIT_CHANGE`, con F4B-E25** | Autoridad de W con la regla de corrección; F4B-E25 citada | ✔ |
| **C02 incluye PB-T2a** | «… PB-T1c or PB-T2a test that does not stay green returns to the Owner» | ✔ |
| **C11 y C17** | Las firmas exactas de nc1 y nc2; el texto literal del Owner de D2 | ✔ |
| **Los marcadores «por rellenar»** | Hay tres: en `IssuedBy`, en la autoridad del cierre de PB-T2a y en la de esta comprobación. Al escribir, `soft()` aborta con cualquier AVISO y la línea 1486 exige `PT2A_CLOSE`, `CI_BASE_T2A` y `BOUNDED_RESULT` | ✔ (no pueden quedar) |
| **La lección de F4B-E33** (leer antes de que un gesto posterior sustituya el objeto) | **No está en la v2.3:** el §7 no tiene ninguna entrada para ella, y el §5.0 no fija el momento de lectura. Es lo que dejan abierto I-02 y M-03 | ✘ (M-03) |

**Los textos propios del generador (D-6):**
- C01, C02, C03, C09, C13 y C17 son fieles al §6.2 y al §6.0;
- `IssuedBy` nombra F4B-E28, pregunta 4, con sus dos condiciones;
- las secciones de autoridad de W, de F4B-E25 y de AUTOMATION_PLAN 16.3 dicen lo que el paquete pide.

## 3. Riesgos de momento de lectura, CT por CT

**Lo que fija la base:**
- `ToleranceBox` solo recalcula en `LostFocus="Input_Changed"` (WX:103; W:1135).
- `Close_Click` es `Close()` (W:3306), y `OnClosing` no recalcula (W:3320-3329).
- `Bom_Click` y `SaveLibrary_Click` recalculan antes de nada (W:3350, W:3368).
- Cada recálculo válido asigna una computación nueva (`lastComputation = computation;`, W:607).
- El comparador **no compara** la selección, la línea base como objeto, el informe de reconciliación ni la marca de consumo de la cola (`PushBackEditableStateComparer.cs:18-21`).
- `S0.Ventana()` abre con `canInsertInAutoCad: true` (`PushBackFullEditorCharacterizationSupport.cs:63-68`).
- `S0.Cerrar` llama a `Close()` sobre la ventana sin mostrar (`:71-77`).
- `SetNumberR` eleva `LostFocus` (`:336-341`).

| CT | Riesgo | Valoración |
|---|---|---|
| 01 | La sonda va en la primera ventana de WORKSPACE, no en la primera línea de la prueba, porque (i) es CLASSIC | M-06 (claridad) |
| 02 | `iguales` y `diseno-igual` se leen tras `Recalcular`. El segundo recálculo cambia la línea base y el informe (`AcceptComputation`), pero el comparador no los mira, así que la premisa «`Recalcular` no cambia el estado» se sostiene por el alcance del comparador. **`seleccion=Both/…` y `B/…` no son literales** | **I-03** |
| 03 | `borrador-igual` es por gesto, contra un `antes` fijo. El `descartes=0` final se lee tras «Actualizar», que deja `appliedState` igual a `editable`. En la pasada 2, `ConfirmModule_Click` confirma y recalcula (W:2379-2389), y los subdiálogos aplican en el acto (doc. de `PendingWork`, W:3331-3341). La etiqueta `ToleranceBox=4` de la pasada 1 no importa aquí, porque «Actualizar» recalcula antes | OK (en M-05, por coherencia con I-01) |
| 04 | Cada línea se arma tras su «Aplicar», que recalcula antes. `mismo-objeto` da `False` en cualquier momento | OK |
| 05 | Los mensajes CLASSIC esperados coinciden con la base: `RequestDraw` (W:3270-3287) y `PushBackCompositeDiagnostics.cs:105-106` (el hueco). `campo-en-error` no eleva `LostFocus`, a propósito. Las huellas siguen el `Vigia` cerrado de CT-PBT1C-02, que captura cada objeto al adoptarlo y vuelve a capturar tras cada paso (`PushBackWindowOnStateTests.cs:390-409, 570-600`) | OK (M-03 pide decirlo) |
| 06 | **`ToleranceBox=4` y «Cerrar» con «No» → `avisos=1`** solo se cumple si el gesto eleva `LostFocus`. Con `Text = "4"` o `SetNumber` sin `LostFocus`, el estado no cambia, `HasUnappliedDraftChanges` da `false` y se obtiene `avisos=0`. D-PBT2-07 dice que «con un clic en «Cerrar», la caja pierde el foco y recalcula antes», pero eso vale con foco real. Aquí no hay foco: el `Click` de S0 es un `RaiseEvent`. Además, el `finally` «como `CloseQuietly`» no cierra una ventana sin mostrar (`if (window.IsVisible)`, T-CL `:124-131`) | **I-01**; M-01 |
| 07 | **`bom-es-el-calculado`:** si se lee después de «Guardar en biblioteca», compara con una computación nueva y da `False`. «Guardar en biblioteca» recalcula (W:3368) y sustituye `lastComputation` (W:607), así que hay que leerlo dentro de la costura o justo tras el clic en «BOM». Además, no se nombra el fixture, y `nombre=<NameBox.Text>` no es literal | **I-02** |
| 08 | Una guarda de texto: no tiene momento de lectura. Un miembro ausente da `False` sin lanzar, así que en el RED falla por aserción (C04) | OK |
| 09 | Texto exacto del archivo del modo y G-L1c con 15 autopruebas. Pasa en el RED (`ExpectRed` false) | OK |
| 10 | Las dos validaciones son puras, en la cultura invariante | OK |

## 4. Anclas y rangos (punto 2)

- **Las tres ventanas son el mismo blob en las tres revisiones** (`161a0dad`, `0dc623ca` y `17872be8`):
  - W = `f8756a88`;
  - WC = `1a3300b4`;
  - WX = `fb094bef`.
- **El resto del producto:**
  - `git diff 161a0dad 17872be8 -- src/RackCad.UI src/RackCad.Domain src/RackCad.Plugin` está vacío;
  - en `src/`, solo se añaden `PushBackDraftEditorLaunch.cs` y `PushBackDraftStatePort.cs`, el esqueleto de `c33ac6ac`.
- **Las anclas W y WC son exactas en `17872be8`:**
  - las 92 líneas de las 66 citas del cuerpo;
  - las costuras S-7, S-9, CT-22 y CT-23;
  - R3 en la base, con `.Request*(` solo en `InsertBatch_Click` y `RequestDraw`.
- **Las comprobadas a mano en esta revisión:**
  - W:56-57 (`editable` `readonly`; `composite => editable.Editor`);
  - W:82 (`moduleOwner => composite.SideA`): se deriva de `editable`, así que la reasignación del constructor privado se propaga;
  - W:1126-1135 (`Name_Changed`, `SyncName` e `Input_Changed`);
  - W:3247-3304 (`RequestDraw`, con la extracción en W:3270-3287 y `SyncName()` en W:3290);
  - W:3342-3344 (`PendingWork`);
  - W:3348-3364;
  - W:3366-3401 (`return` en W:3384, `EnsureId` en W:3388, `SetName` en W:3389 y `SyncName()` en W:3390);
  - W:3438-3447 y W:3449-3496;
  - WC:95 y WC:790-794;
  - W:3569-3570 (`Keep`, MU-T2C-01).
- **Los diez rangos de INV-PBT2B-03 son exactos:** LoadNew 269-287, LoadDesignForNew 289-304, LoadExisting 306-320, Update_Click 3215, InsertBatch_Click 3217-3245, RequestDraw 3247-3304, PendingWork 3331-3344 (miembro 3342-3344), Bom_Click 3348-3364, SaveLibrary_Click 3366-3401 y UpdateButtons 3449-3496. Además, sus 38 autopruebas desplazadas una línea fallan (seco).
- **La API del esqueleto coincide con lo que usan D-PBT2-05 y las recetas** (`VentanaBorrador`, `PuertoContado`):
  - `PushBackDraftEditorLaunch(state, rackId, sourceProject, port)`, con `State`, `RackId`, `SourceProject` y `Port`;
  - `PushBackDraftApplyResult(Status, IsDirty)` y `PushBackDraftApplyStatus`;
  - `PushBackDraftSessionPort(draft)` y `LaunchFor(draft, sourceProject)`.

## 5. Las dudas D-1 a D-8 del generador (punto 3)

- **D-1. Por qué 23 es un riesgo.** `DialogWindowCharacterizationTests` tiene 23 métodos, de los que 3 están en `Skip` (`:345`, `:373` y `:661`), y el seco le da `MinSelected` 23. Es el **único** filtro del contrato cuyo mínimo depende de las filas omitidas:
  - `PushBackFullEditorCharacterization` (UI) tiene 16 ejecutables y 2 omitidas, con mínimo 16; PB-T1a, PB-T1b y PB-TC2 usaron también 16;
  - `RichEditor` tiene 25 ejecutables y 4 omitidas, con mínimo 25.
- **D-1. Lo que dice el puente.** I-64-A-8 §2 cuenta en `Selected` todas las filas `UnitTestResult` de primer nivel, `NotExecuted` incluidas (`I-64-A-8.md:92`). Con esa lectura, 23 pasa.
- **D-1. Qué hacer:** poner **20**, los ejecutables, que vale con las dos lecturas y no pierde nada material. Si se quiere conservar 23, citar A-8 §2 junto al filtro (M-02).
- **D-2.** Se acepta como está. Los testigos son literales de la tabla de D-PBT2-09, que es autoridad, y R10 de CT-PBT2B-08 exige cada uno una vez y con su literal. Listarlos en INV-04 sería una copia más que mantener.
- **D-3. Los nombres se aceptan.** Al custodiar hacen falta dos cosas:
  - **El encargo de esta comprobación:** hay que escribir `independent-review-pbt2b-v23-check-prompt.md` con el texto literal del encargo, como en PB-T2a, porque hoy no existe en el borrador (M-04).
  - **Las anclas:** hay que regenerar `pbt2b-anchors-out.txt` sobre el paquete final. Hoy mide la v2.2, con 64 citas distintas, y el seco cuenta 66 citas en la v2.3 (M-04).
- **D-4.** Se acepta tal cual. Por código, `WindowCensusGuardTests` solo nombra `RackFrameConfiguratorWindow` en una lista (`:32`), sin crearlo. La atribución es más amplia de lo necesario, pero no tapa ninguna escritura nueva: C09 gobierna las pruebas nuevas, y ninguna crea el configurador real.
- **D-5.** Se acepta, con el precedente de D-9 de PB-T2a.
- **D-6.** Se acepta: los textos propios son fieles (§2).
- **D-7.** Se acepta la renumeración a 08b. Quitar el invariante propio también valdría, pero 08b conserva la trazabilidad de V23-07.
- **D-8.** Se acepta, con la condición del §6, punto 4: si el cierre de PB-T2a cambia otra clase que no sea de PB-T2a, `COUNTS_BASE` se actualiza y la sesión lo explica. Es un cambio material solo si toca un filtro de este contrato.

## 6. Lo que el generador debe remedir al escribir (punto 4)

1. **El cierre de PB-T2a:**
   - `PT2A_CLOSE` es el commit del cierre, y `PT2A_ENTRY` su entrada de `f4b-dispositions.md`, posterior a F4B-E33;
   - `F4B-PBT2A-DRAFT-STATE-PORT/task-close-package.md` está en la revisión, porque es autoridad.
2. **Que la ventana no se haya movido:**
   - `git diff PT2A_CLOSE BaseSha -- src/RackCad.UI` está vacío;
   - los blobs de W, WC y WX siguen siendo `f8756a88`, `1a3300b4` y `fb094bef`. Si es así, todas las anclas y rangos del §4 valen sin medirlos otra vez; aun así, hay que repetir las 38 autopruebas en negativo.
3. **La API real del puerto en `PT2A_CLOSE`** es la del §4: los mismos nombres, firmas y valores de `PushBackDraftApplyStatus` (`Applied` y `NotEditable`). Un cambio de nombre o de firma es material: PB-T2b se ajusta antes de emitir y vuelve a pasar por esta comprobación.
4. **El producto y los recuentos:**
   - Application = `161a0dad` más los dos archivos de PB-T2a, sin nada más;
   - Domain, Plugin y UI son iguales a `161a0dad`;
   - en `tests/`, solo las dos clases de PB-T2a, con al menos 3 y 1 métodos;
   - `COUNTS_BASE` sigue igual en los 38 filtros (D-8).
5. **La CI:**
   - `CI_BASE_T2A` es la corrida `push` 4/4 de la punta del cierre, con el mismo `src` y los mismos `tests` (`ci_base_check`);
   - sus cifras son (19 092, 2 439, 21); si no, hace falta `CI_T2A_EXPLAINED`;
   - el GREEN previsto se recalcula a partir de ella: Core +2 y UI +8, con 21 omitidas.
6. **El paquete corregido:**
   - el control de versión del generador, que hoy exige la v2.3, pasa a la versión que lleve I-01 a I-03;
   - se repiten el seco y las 34 pruebas en negativo, que deben fallar todas;
   - el texto literal que compara el generador (§6.2, §6.0, la costura y las clases de §5.3, INV y paradas) no debería cambiar con estas correcciones: si cambia, se explica.
7. **La custodia** en la `AuthorityRevision`:
   - el paquete corregido en `docs/initiatives/I-64-f4b-pbt2-gate-contract-package.md`;
   - este informe;
   - las cinco rutas de `CUSTODY_EXTRA`, con el encargo y las anclas del punto D-3;
   - la entrada de emisión, la última, que nombra F4B-E28, pregunta 4, y el `BOUNDED_RESULT`.
8. **El entorno:**
   - `MainSha` = `origin/main`;
   - las celdas vigentes hasta el 2026-12-29 (A7);
   - `Test-Json` sobre el contrato escrito;
   - `config.toml` = Snapshot J, el binario y una sola delegación abierta;
   - ningún «por rellenar» en el JSON (el generador ya lo garantiza).
9. **La autorización:** se comprueba que se cumplen las tres condiciones de F4B-E28, pregunta 4, dichas arriba del todo: 0 B y 0 I sin resolver, la remedición sin cambio material y PB-T2a sin intento 3.

## 7. Decisiones del Owner (punto 5)

**No hay ninguna nueva.** Las tres importantes y las siete menores precisan recetas de prueba o cifras de procedimiento:
- no cambian ningún comportamiento, ni de CLASSIC ni de WORKSPACE;
- no tocan Q-PBT2-1, Q-PBT2-2 ni Q-PBT2-3;
- no cambian lo que se informa en §8.4.

**Dos premisas que no piden decisión:**
- **Que el comparador no mire la selección** (§3, CT-02 y CT-03) es diseño de PB-T1b ya cerrado. La selección no basta para que «Cerrar» pregunte; es coherente con Q-PBT2-1 (A).
- **L-PBT2-13** («Restaurar» no recalcula) ya está declarada en §8.3, y no alcanza al borrador, porque «Aplicar» recalcula antes de aplicar.

## 8. Hallazgos

| Id | Sev. | Dónde | Hallazgo | Qué hacer |
|---|---|---|---|---|
| I-01 | I | §5.3 CT-06 (y D-PBT2-07) | **El problema:** `ToleranceBox=4` no dice cómo se edita. Sin `LostFocus` no hay recálculo (WX:103, W:1135), y `OnClosing` no recalcula (W:3320-3329). Con «No» sale `avisos=0`, no 1. La frase de D-PBT2-07 sobre el foco vale con foco real, y aquí no lo hay | Escribir `SetNumberR(ToleranceBox, 4)` en `cerrar-con-cambios-no` y también en el «editar» de `cerrar-tras-aplicar`. En D-PBT2-07, añadir: «en la red, sin foco real, el gesto es `SetNumberR`, que eleva `LostFocus`» |
| I-02 | I | §5.3 CT-07 | **El problema:** `bom-es-el-calculado` no dice ni cuándo ni contra qué se lee. «Guardar en biblioteca» recalcula (W:3368) y sustituye la computación (W:607), así que leerlo al final da `False`, como CT-PBT2A-02 en F4B-E33. Tampoco se nombra el fixture, y `nombre=<NameBox.Text>` no es literal | Leer `ReferenceEquals(bom recibido, w.LastComputation.Bom)` dentro de la costura o justo tras el clic en «BOM», antes de la biblioteca. Fijar el fixture (`pb-01-base`) y escribir `nombre=` con su literal |
| I-03 | I | §5.3 CT-02 | **El problema:** `seleccion=Both/…` y `B/…` no son literales. Eso contradice V21-02 («sin «…»») y deja la línea a cargo del Worker (C05, R-PBT2-01) | Tomar lo esperado del **estado de entrada** (la `Copy` de `EstadoDeVentana`: `ActiveSelection` y los índices del lado activo), no de la ventana; o medirlo en el paso 1 y transcribirlo, como `<a>` y `<b>` |
| M-01 | M | §5.3 CT-06 | «Como `CloseQuietly`»: ese ayudante no cierra una ventana sin mostrar (`if (window.IsVisible)`) | Decir: «el `finally` pone «Sí» y llama a `S0.Cerrar(w)`» |
| M-02 | M | §5.1, §6.2 RequiredTests | `DialogWindowCharacterizationTests` = 23 cuenta 3 `Skip`. Es el único mínimo que depende de filas `NotExecuted` (D-1) | Poner `MinSelected` 20, o conservar 23 citando A-8 §2 |
| M-03 | M | §5.0 y §7 | **El problema:** falta la lección de F4B-E33, el momento de lectura | Añadir R-PBT2-22 y una regla en §5.0: cada línea se arma justo tras su gesto; un campo que compara con un objeto «actual» (`draft.CurrentDraftState`, `w.LastComputation`) dice cuándo se lee; las huellas de CT-05 usan el `Vigia` de CT-PBT1C-02 y capturan también las referencias del borrador tras cada paso |
| M-04 | M | Custodia (D-3) | **El problema:** las anclas se midieron sobre la v2.2, y el encargo de esta comprobación no existe como archivo | Regenerar `pbt2b-anchors-out.txt` sobre el paquete final; escribir `independent-review-pbt2b-v23-check-prompt.md` con el encargo literal |
| M-05 | M | §5.3 CT-03, pasada 1 | `ToleranceBox=4` como gesto. Aquí no cambia el resultado, porque «Actualizar» recalcula | Escribir `SetNumberR(ToleranceBox, 4)`, por coherencia con I-01 |
| M-06 | M | §5.0 `Sondeo` | **El problema:** la regla dice «la primera línea de cada prueba de WORKSPACE», pero CT-01, 05 y 06 empiezan con CLASSIC | Decir: «la primera línea de cada ventana de WORKSPACE». Si es `Classic`, la prueba no hace más gestos |
| M-07 | M | §5.2 CT-PBT2A-02 | **El problema:** la descripción de `launchfor` no recoge C-1 de F4B-E33 (`estado-es-el-actual` se lee antes de aplicar). El paquete es autoridad de PB-T2b | Añadirlo en la versión corregida. La línea esperada no cambia |

## §10. Recomprobación sobre la v2.4

**Veredicto de §10: EMITIBLE. Nuevos: B 0 / I 0 / M 4.**

- **Lo pedido en CA-T2b:** las tres importantes y las siete menores están corregidas en la v2.4. M-04 tiene una parte pendiente que es de la sesión: escribir el encargo literal. El generador la exige antes de escribir.
- **El defecto nuevo que encontró el redactor (V24-11) es real, y CA-T2b no lo vio.**
  - Los 17 fixtures tienen `PalletTolerance` = 4. Con 4, `Keep` conserva el valor, así que la corrección que CA-T2b proponía para I-01 (`SetNumberR` con 4) **no bastaba**.
  - En el §3 de CA-T2b dije que la etiqueta `ToleranceBox=4` de la pasada 1 de CT-03 «no importa, porque «Actualizar» recalcula antes». **Era falso:** con 4 no hay nada que aplicar, y `UpdateButton … llamadas=1` no se habría cumplido.
  - Lo corrige la v2.4, con 4.5.
- **Los 4.5 y 5:** por lectura del código, dan un estado válido, distinto y sin bloqueo en `pb-01-base` y en `pb-20-ab`.
- **El barrido:** no deja ninguna CT de PB-T2b ni de PB-T2c expuesta a una lectura fuera de momento ni a un gesto inerte.
- **Las cuatro menores nuevas** son de robustez y de texto. No impiden emitir.
- **Decisiones nuevas para el Owner:** ninguna.

**`BOUNDED_RESULT` propuesto (ASCII):**

`EMITIBLE TRAS CORRECCIONES (B 0 / I 3 / M 7; I-01, I-02 e I-03 corregidas en el paquete; recomprobacion v2.4: EMITIBLE, B 0 / I 0 / M 4)`

Es compatible con el `bounded_check` de hoy (`make_pbt2b_contract.py:1566-1589`):
- empieza por el veredicto de la línea de arriba;
- lleva `B 0 / I 3 / M 7` y `corregidas en el paquete`;
- y deja constancia de esta recomprobación (M-08).

### 10.1 Método y límites

- **El código, solo por objetos git** desde el checkout principal, en `5ddeb3b4` (el GREEN de PB-T2a) y en `17872be8`.
- **Entre las dos revisiones:**
  - `git diff 17872be8 5ddeb3b4` solo toca los dos archivos de PB-T2a y su clase de prueba;
  - UI, Domain y Plugin no cambian, así que W, WC y WX siguen siendo `f8756a88`, `1a3300b4` y `fb094bef`;
  - la API pública del puerto es la misma: solo cambian los cuerpos.
- **Los fixtures:** se leyeron con dos guiones propios de solo lectura, escritos con la herramienta de archivos y lanzados con `python -I`: `recheck_fixture_tolerance.py` y `recheck_fixture_fields.py`, en `hs65\pbt2\`. Leen `git show <rev>:<fixture>` y desanidan el `Design`.
- **Lo que no se hizo:** ni compilar, ni ejecutar pruebas, ni tocar el worktree, ni usar heredoc con Python.
- **La validez de 4.5 y 5** sale de leer el código, no de ejecutarlo.

### 10.2 Las correcciones de CA-T2b (punto 1)

| CA-T2b | Estado | Dónde, en la v2.4 (líneas del archivo `…-v24.md`) o en el generador |
|---|---|---|
| I-01 | Corregida | **Registro:** V24-01 (l.68).<br>**§5.0, «Los gestos»** (l.775-778): `SetNumberR`, «un valor que se ve igual no cambia el estado» y la tolerancia de los fixtures.<br>**D-PBT2-07** (l.543): «En la red».<br>**CT-06** (l.854): `SetNumberR(ToleranceBox, 4.5)` en las tres ediciones, cinco ventanas y su orden.<br>**L-PBT2-10** (l.1341) |
| I-02 | Corregida | **Registro:** V24-02 (l.69).<br>**CT-07** (l.850): la costura compara en el acto, y la línea se arma tras «BOM» y antes de la biblioteca. Fixture `pb-01-base`, `nombre=PB-01` literal y `archivo` del §5.0 |
| I-03 | Corregida | **Registro:** V24-03 (l.70).<br>**`Sel(e)`** (l.795): sus miembros existen en `5ddeb3b4`:<br>- `PushBackEditableState.Editor` (:55);<br>- `ActiveSelection` (PushBackCompositeEditorState :179);<br>- `Active` (:266);<br>- `Structure` (PushBackEditorState :41);<br>- `SelectedFrontIndex` y `SelectedLevelIndex` (DynamicFrontMatrix :34-35).<br>**CT-02** (l.849): lo esperado sale de la entrada. La carga no toca la selección: `LoadFromModel` va entera bajo `suppressSync`, no llama a `SetActiveSelection` y solo normaliza los índices (W:332-367, W:373), y `RefreshCompositePanel` refleja `ActiveSelection` bajo `suppressSync` (WC:738-745) |
| M-01 | Corregida | V24-04 (l.71); CT-06 (l.854) |
| M-02 | Corregida | **Registro:** V24-05 (l.72); §5.1 (l.806).<br>**Contrato en seco:** `MinSelected` 20. El seco dice «cada mínimo frente a los ejecutables» (16, 20 y 25) |
| M-03 | Corregida | **Registro:** V24-06 (l.73); §5.0, «El momento de cada lectura» (l.768-773).<br>**CT-05** (l.853): un solo `Vigia`, que captura la ventana, `CurrentDraftState` e `InitialDraftState` tras cada paso.<br>**`InitialDraftState`** devuelve una copia (DraftSession.cs:284). Esa copia comparte las dos referencias de D2, así que el `Vigia` recoge los mismos objetos.<br>**§7:** R-PBT2-22 (l.1246) |
| M-04 | Corregida, con una parte para la sesión | **Registro:** V24-07 (l.74); custodia en el §6.0 (l.1015).<br>**Las anclas:** `pbt2b-anchors-out.txt` se regeneró. Su parte 2 nombra el paquete final por su sha256 `008de1d5…`, que es el del archivo de la v2.4.<br>**El encargo:** no existe todavía. Está en `CUSTODY_EXTRA` (`make_pbt2b_contract.py:1453`), y el AVISO impide escribir sin él |
| M-05 | Corregida | V24-08 (l.75); pasada 1 de CT-03 (l.861 y l.874), con 4.5 |
| M-06 | Corregida | V24-09 (l.76); `Sondeo`, por ventana (l.788); CT-01 (l.848) |
| M-07 | Corregida | V24-10 (l.77); CT-PBT2A-02 (l.823), con el orden de C-1. El GREEN de PB-T2a (`5ddeb3b4`) mantiene 3 y 1 métodos en sus dos clases |

### 10.3 El defecto nuevo: la tolerancia de los fixtures (punto 2)

**El hecho, medido en `5ddeb3b4`** (`recheck_fixture_tolerance.py`):
- los 17 sobres de `tests/RackCad.Tests/Fixtures/I64` tienen **una sola** clave de tolerancia, `Design.PushBack.Structure.PalletTolerance`, y vale **4** en todos, `pb-20-ab` incluido;
- el lado B no tiene tolerancia propia: `PushBackCompositeStructure.cs:837` le copia la compartida;
- `pb-01-base` se llama `PB-01`. (`pb-31-v2-modulos` también se llama `PB-01`, pero ninguna línea con su nombre depende de ello.)

**Por qué 4 era inerte:**
- `ReadInputs` hace `rackTolerance = Keep(ToleranceBox, rackTolerance)` (W:768);
- `Keep` conserva el valor actual cuando la caja se ve igual con el formato `0.###` (W:3555-3570);
- `SetNumberR(…, 4)` muestra «4» sobre un 4, así que el estado no cambia, y `HasUnappliedDraftChanges` no lo ve.

**El recorrido de 4.5 y de 5:**
- **La caja:**
  - `ToleranceBox` es `Minimum="0" MinimumInclusive="False"`, sin `IntegerOnly` (WX:103-104);
  - `SetNumberR` escribe «4.5» y «5» en la cultura invariante;
  - `OnTextChanged` vuelve a validar y fija `Value` (NumericField.cs:131-163);
  - los dos valores pasan la validación.
- **El estado:**
  - `Keep` da 4.5, porque «4.5» ≠ «4», y después 5, porque «5» ≠ «4.5»;
  - el comparador compara `PalletTolerance` (PushBackEditableStateComparer.cs:409), así que «Aplicar» llega al puerto y el borrador queda sucio frente al inicial (DraftSession.cs:298).
- **El modelo:**
  - el ensamblador pasa la tolerancia al diseño (PushBackEditorDesignAssembler.cs:172-174 y :377);
  - el resolutor solo exige que sea positiva (DynamicRackSystemResolver.cs:473) y la guarda en el sistema (:247);
  - **no entra en la geometría de Push Back:** `DynamicFrontGeometry.AutoBeamLength` la recibe y no la usa (DynamicFrontGeometry.cs:34-38), y `DynamicRackLevelGeometry` solo se la pasa a esa función (:97, :122 y :269);
  - `PushBackCompositeDiagnostics` no la menciona, así que no hay ningún bloqueo posible por ella;
  - persiste con el diseño (DynamicRackSystemDocument.cs:156), así que `diseno-igual` compara 4.5 con 4.5.
- **El resultado:** con 4.5 y con 5, el estado es válido, distinto de W0 y no bloquea «Aplicar», en `pb-01-base` y en `pb-20-ab`. Ni se redondea a 4, ni se rechaza.

La afirmación de la v2.4 de que la tolerancia «solo entra en la geometría transversal» (l.777, V24-11) es imprecisa: en Push Back no entra en ninguna geometría. La conclusión es la misma, y aún más firme (M-09).

### 10.4 El barrido de PB-T2b y PB-T2c (punto 3)

| CT | Lectura frente al paso | Gesto o dato | ¿Expuesta? |
|---|---|---|---|
| T2B-01 | (ii) se lee antes de (iii). Las tres cargas empiezan por la guarda (R8). Si la sonda da `Classic` en el RED, (iii) y (iv) no se ejecutan | Ninguno | No |
| T2B-02 | `diseno-igual` y `iguales`, tras `Recalcular`; `tras-restaurar`, antes de «Aplicar»; `nombre` y `seleccion`, de la entrada | `ambos-mixto` escribe 55 sobre un 60 (todas las celdas de `pb-20-ab`). `celda-1-0`: `pb-08-redondeo` tiene 3 frentes. `lado-B` y `ambos-mixto`: WC:154-156 | No |
| T2B-03 | Una línea por gesto, contra un `antes` fijo; `archivo` por clic | 4.5 sobre 4. La pasada 2 usa los gestos de sus originales. Pasada 3: `StageModuleEdit` suma 8 a la longitud (T-CL :90-98) | No |
| T2B-04 | Cada línea, tras su «Aplicar»; `actual` y `c`, justo antes | 4.5 ≠ 4 y 5 ≠ 4.5. `PalletsDeepBox` pasa de 8 a 3: entero y ≥ 2 (WX:111-112); cambia el portador general, que el comparador mira (:406), y no la geometría, porque los frentes traen 8 (DynamicDepthGeometry.cs:199-202) | No |
| T2B-05 | Mensajes CLASSIC iguales a la base: `Cargar` = `LoadExisting` (PushBackDeclaredCorrectionSupport.cs:71-76). `Vigia` tras cada paso | El hueco pasa de 54 a −1 (el comparador mira `Gap`, :289). La cuarta ventana usa 4.5 | No |
| T2B-06 | `Click(CloseButton)`, sin recálculo en `OnClosing`; cada línea, tras su gesto | 4.5 en `pb-01-base`; `StageModuleEdit` +8 | No |
| T2B-07 | El BOM se compara dentro de la costura; la línea, antes de la biblioteca | `archivo=False` depende del disco (M-10) | Robustez, M-10 |
| T2B-08 a 10 | Texto y validación pura | — | No |
| T2C-01 | `aplicado` con `actual` tomado justo antes; `diseno`, tras «Aplicar» | `DrawRackNameCheck` escucha `Click` (WX:58-59). Se invierte, así que siempre cambia, y lo lee el `ReadInputs` de «Aplicar» | No |
| T2C-02 | Copia CT-PBTC-07 | Declarar B cambia `SideBPresent` | No |
| T2C-03 | Regla de tres casos. Lo esperado se fija sobre el cierre de PB-T2b, con la ventana real | `ToleranceBox=4` de `Gestos()` cae en «nada que aplicar» | No |

### 10.5 Las cuatro dudas del redactor (punto 4)

1. **El encargo de CA-T2b:** sí, lo escribe la sesión con su texto literal, porque el revisor no conserva el mensaje palabra por palabra.
   - El generador ya no escribe sin él (AVISO de `CUSTODY_EXTRA`).
   - Como esta §10 forma parte del informe custodiado y decide el `BOUNDED_RESULT`, su encargo también se custodia literal: en el mismo archivo, a continuación, o en uno nuevo de `CUSTODY_EXTRA` (M-11).
2. **El `BOUNDED_RESULT`:**
   - El literal de hoy pasa el `bounded_check`, pero no dice nada de la recomprobación.
   - Se propone el de arriba del todo, que sigue empezando por `EMITIBLE TRAS CORRECCIONES`.
   - El `bounded_check` lee solo el **primer** veredicto (`re.search`, :1569), así que no ve esta §10 (M-08).
3. **4.5 y 5 en `pb-20-ab`:** válidos, distintos y sin bloqueo (§10.3). La tolerancia es una sola para todo el rack, y el lado B la copia; en Push Back no entra en la geometría, y ningún diagnóstico la mira.
4. **CT-01 (iii) en la misma ventana que (ii):** vale, por cuatro razones:
   - la línea de (ii) se lee antes;
   - R8 exige que las tres cargas empiecen por la guarda, así que lanzan sin tocar nada;
   - el `modo=Workspace` posterior y el `descartes=0` final lo confirman;
   - en el RED, la sonda da `Classic` en (ii) y la prueba no hace (iii), así que nunca se ejecuta una carga CLASSIC sobre esa ventana.

### 10.6 Decisiones del Owner (punto 5)

**Ninguna nueva:**
- se cambian datos de prueba (4 → 4.5), el momento de las lecturas, un mínimo y la custodia;
- ningún comportamiento de CLASSIC ni de WORKSPACE cambia;
- Q-PBT2-1 a Q-PBT2-4 y lo que informa el §8.4 siguen como estaban.

### 10.7 Hallazgos nuevos

| Id | Sev. | Dónde | Hallazgo | Qué hacer |
|---|---|---|---|---|
| M-08 | M | `make_pbt2b_contract.py`, `bounded_check` (:1566-1589) | **El problema:** lee el primer `**Veredicto: …**` del informe, el de la v2.3, y no ve el de esta §10. Si una recomprobación trajera bloqueantes o importantes nuevos, el generador emitiría igual | Leer también la línea `**Veredicto de §10: … Nuevos: B n / I n / M n.**` y exigir B 0 e I 0; pedir en `BOUNDED_RESULT` la mención de la recomprobación, como el literal propuesto. Añadir una prueba en negativo con una §10 que tenga I 1 |
| M-09 | M | v2.4 l.777 y V24-11 (l.78) | «La tolerancia solo entra en la geometría transversal»: en Push Back no entra en ninguna. `AutoBeamLength` la ignora (DynamicFrontGeometry.cs:34-38); solo hay la comprobación de que es positiva (DynamicRackSystemResolver.cs:473), el sistema (:247) y la persistencia | Corregir la frase. La conclusión no cambia |
| M-10 | M | CT-07 (l.850) y §5.0 `ArchivoTemporal` (l.790) | **El problema:** `archivo=False` depende del disco. El borrado previo solo cubre los clics con la costura devolviendo `ArchivoTemporal`. Un archivo que quede de una corrida local abortada (p. ej. por `--blame-hang`) daría `archivo=True` | Borrar `ArchivoTemporal` antes de **cada** clic en «Guardar en biblioteca», también cuando la costura devuelve `null` |
| M-11 | M | §6.0 «Custodia» (l.1015) y `CUSTODY_EXTRA` | El encargo de esta §10 no está en la custodia, y la §10 decide el `BOUNDED_RESULT` | Custodiarlo literal, como el de CA-T2b |

## §11. Recomprobación sobre la v2.7

**Veredicto de §11: EMITIBLE TRAS CORRECCIONES. Nuevos: B 0 / I 1 / M 4.**

**El veredicto en breve:**
- **La v2.7 refleja bien RR-1 a RR-6,** con dos lecturas declaradas para un RED que ya toca `src/`. Son fieles a la intención de F4B-E35.
- **Le falta la mitad de RR-7.** RR-7 dice: «La revisión independiente y la verificación del Controller leen la reparación como parte de la entrega».
  - La comprobación n.º 10 de AUTOMATION_PLAN 16.9 (`Tests`) exige, en su `RedPart`, `git diff --name-only RedSha CurrentSha` sin tocar `RT ∪ ChainRedFiles`.
  - Cualquier reparación toca un archivo de `RT` entre `RedSha` y el GREEN, así que, leída al pie de la letra, esa comprobación da `fail`, y eso es REWORK.
  - Ni el paquete ni el contrato dicen cómo se lee esa comprobación con una reparación. La primera reparación del RED gastaría así el intento que RR-7 promete no gastar (I-01).
- **No veo una decisión nueva para el Owner.** Hacer explícita esa lectura aplica RR-7, como F4B-E25 explicó cómo comprobar su excepción a 16.3.
- **El seco real sobre `b29551ae`** es correcto: `PT2A_CLOSE` y `CI_BASE_T2A` están bien, y los 12 AVISOS son todos de la emisión.

**`BOUNDED_RESULT`:**
- **Mientras esta §11 tenga una importante, ningún literal pasa el `bounded_check`.** Es lo previsto en V25-01: la última recomprobación tiene que dar EMITIBLE con B 0 e I 0 (`make_pbt2b_contract.py:1692-1732`).
- **La v2.8 y la §12:** con I-01 corregida en una v2.8 y una §12 «Recomprobación sobre la v2.8» que dé EMITIBLE con B 0 e I 0, el literal, en ASCII, será este, con `<m>` las menores de esa §12:
  `EMITIBLE TRAS CORRECCIONES (B 0 / I 3 / M 7; I-01, I-02 e I-03 corregidas en el paquete; recomprobacion v2.4: EMITIBLE, B 0 / I 0 / M 4; M-08 a M-11 aplicadas en la v2.5; recomprobacion v2.8: EMITIBLE, B 0 / I 0 / M <m>)`
- **Por qué pasa:** empieza por el primer veredicto, lleva `B 0 / I 3 / M 7` y `corregidas en el paquete`, y nombra la última recomprobación, como pide el `bounded_check`.
- **Si la sesión prefiere no hacer una v2.8** y resolver I-01 de otro modo, por ejemplo solo en el texto de la verificación, sigue haciendo falta una recomprobación que dé B 0 e I 0 antes de escribir.

### 11.1 Método y límites

- **Qué se leyó:**
  - del repositorio, solo objetos git de `b29551ae` desde el checkout principal: el archivo de F4B-E35 entero, `f4b-dispositions.md` (F4B-E33 a F4B-E35), AUTOMATION_PLAN 16.4, 16.8 y 16.9, y F4B-E15;
  - del borrador: la v2.7, `v27-delta.md`, el generador de PB-T2b, su seco (sha `5301c315…`, que coincide), `neg-pbt2b.txt` y las notas.
- **Qué no se hizo:** no ejecuté Python, no compilé y no toqué el worktree.
- **La numeración de las dudas:** las notas numeran las dudas de la v2.7 como D-18 a D-20. La «D-1» del encargo es su D-18.

### 11.2 Fidelidad a RR (punto 1)

| RR | En la v2.7 y en el seco | Estado |
|---|---|---|
| **RR-1** (cuándo; una por intento) | §6.0 (l.1098): «después del commit RED y su push, y antes del commit GREEN; como mucho una reparación por intento». C07 y C18 lo repiten («found after the RED push and before the GREEN»; «the second one in the attempt») | ✔, con la lectura de D-18 (§11.3) |
| **RR-2** (qué toca) | §6.0 (l.1099), incluido «tampoco la costura inerte ni los mutantes del RED». C18: «a file the RED did not add, an existing test, src/, a fixture or anything outside the AllowedWriteScope» | ✔ |
| **RR-3** (sin cambiar lo esperado) | §6.0 (l.1100), con la restricción positiva: «corrige solo el cuándo y el cómo de una lectura o de un gesto que el paquete ya prescribe». C07 y C18 solo llevan la negativa («changes no expected line, literal or oracle») | ✔ en el paquete; **la restricción positiva no llega al contrato** (M-03) |
| **RR-4** (el RED repetido entero) | §6.0 (l.1101), «La CI del RED reparado» (l.1111), la `ExpectedEvidence` «el RED» y «en local», C18 «the full RED criterion and its CI are not rerun» e INV-PBT2B-02 | ✔ |
| **RR-5** (declarado) | §6.0 (l.1102) y la `ExpectedEvidence` «la entrega», con `RedRepairSha`, la prueba y la línea, el error, por qué no cambia nada esperado, el diff y `RedSha` original. C18 incluye «or it is not declared as RR-5 says» | ✔, aunque el `Trailer` de la evidencia nombra solo los commits RED y GREEN (M-02) |
| **RR-6** (la cadena) | §6.0 (l.1103) e INV-PBT2B-02: `BaseSha..RedSha`, el RED del contrato; `RedSha..RedRepairSha`, solo clases que el RED añadió; `RedRepairSha..CurrentSha`, el GREEN del contrato | ✔, con la lectura de D-18 |
| **RR-7** (no gasta intento; la revisión y la verificación leen la reparación) | Solo la primera mitad: «sin gastar intento (RR-7)» y el C07 «without spending an attempt». **Nada dice cómo la verificación del Controller lee la reparación** en las comprobaciones de 16.9 que la tocan: n.º 10 `Tests`, su `RedPart`, y n.º 11 `Trailer` | **✘ (I-01)** |
| **La autoridad F4B-E35** | Fila del §1 (l.240) y autoridad del contrato. En `b29551ae` el archivo está, y `rr_check` pasa: la respuesta literal del Owner, «Empieza en PB-T2b», RR-1 a RR-7 cada una una vez y el título | ✔ |
| **«Excepciona 16.8 solo en estas condiciones; si se rompe alguna, la regla no aplica»** | §6.0 (l.1097) y C18: «The rule does not apply and the repair counts as touching tests/ after the RED (AUTOMATION_PLAN 16.8): STOP» | ✔ |

Sobre C18: el Owner dijo «vale la parada de siempre». Tocar `tests/` tras el RED ya era parada (16.8), así que STOP es fiel, no una ampliación.

### 11.3 D-18, las dos lecturas (punto 2)

**RR-1, «antes del primer commit que toque `src/` (el GREEN)», se lee «antes del commit GREEN».**
- El paréntesis del Owner identifica ese commit con el GREEN, así que la lectura conserva la intención: reparar antes de que la implementación entre en un commit.
- La lectura literal haría la regla imposible en PB-T2b y PB-T2c, porque el RED ya toca `src/`.
- **No abre ningún hueco.** RR-2 sigue prohibiendo `src/`: ni la costura inerte ni el archivo del modo, aunque los añadiera el RED, porque solo cuentan los de `tests/`. Y C18 lo hace cumplir.

**RR-6, «BaseSha..RedSha, solo `tests/`», se lee «el RED de cada contrato».**
- Es la cadena que el contrato ya define en INV-PBT2B-02: las clases nuevas y la costura inerte.
- El tramo final, «`RedRepairSha..CurrentSha`, el GREEN del contrato», es más estricto que «solo `src/`», porque solo admite W y el archivo del modo.

**Un matiz que el contrato cubre:** el archivo del modo lo añade el RED, pero es `src/`. RR-2 («solo archivos de `tests/`») e INV-PBT2B-02 («clases de prueba») lo dejan fuera de cualquier reparación.

### 11.4 Que RR no abra una vía para cambiar lo esperado (punto 3)

**Lo que la verificación puede comprobar de forma mecánica:**
- **RR-1:** que entre `RedSha` y el GREEN hay a lo sumo un commit, titulado «RED repair» y con el `TaskId`.
- **RR-2 y RR-6:** que `git diff --name-only RedSha RedRepairSha` ⊆ `git diff --name-only --diff-filter=A BaseSha RedSha -- tests/`, y que `RedRepairSha..CurrentSha` es el GREEN de INV-PBT2B-02.
- **RR-4:** que la CI de `RedRepairSha` da lo que espera el RED, y que están los TRX de los filtros con `ExpectRed` sobre ese commit.
- **RR-5:** que la entrega trae la declaración y el diff, y que `RedSha` es el original.

**Lo que no se puede comprobar del todo, RR-3:** muchas líneas esperadas no son literales del paquete.
- Unas se derivan de la entrada: `Sel(entrada)`, `Nombre(…)`, `<a>` y `<b>` del paso 1.
- Otras están en otra autoridad, como la tabla de CT-PBT2C-03.

**Hoy RR-3 queda a la revisión,** como dice RR-7, y eso es fiel a F4B-E35. **Una ayuda mecánica barata cerraría la vía más probable (M-01):**
- las líneas literales del paquete (las tres listas de CT-PBT2B-03 y las líneas fijas de CT-01 a CT-07) aparecen byte a byte en la clase reparada, igual que en `RedSha`;
- ningún hunk de `git diff -U0 RedSha RedRepairSha` cambia el primer argumento de un `Assert.Equal` ni una cadena de la forma `<campo>=…`;
- lo que no pase, va a la revisión con la declaración de RR-5.

**Lo que impide que esta vía se use sin parar:** C07 y C18 la convierten en parada, y la entrega trae el diff. Lo que falta es quién compara y cómo. Eso es parte de I-01, para la verificación, y de M-01, para la ayuda mecánica.

### 11.5 El seco real sobre `b29551ae` (punto 4)

- **`PT2A_CLOSE` = `b29551ae`, hija de `5ddeb3b4`:**
  - `git diff --name-only 5ddeb3b4 b29551ae -- src tests` sale vacío;
  - `f4b-dispositions.md` trae F4B-E34, «Cierre de F4B-PBT2A-DRAFT-STATE-PORT», posterior a F4B-E33, y F4B-E35;
  - el `task-close-package.md` de PB-T2a está.
- **`CI_BASE_T2A` = (`44480898605`, `5ddeb3b4`, 19 092, 2 439, 21):**
  - F4B-E34 la registra («GREEN `44480898605`: 4/4, con Core 19 092 y UI 2 439»);
  - el `src` y los `tests` de `5ddeb3b4` son los del cierre, así que `ci_base_check` puede atarla;
  - las cifras son las previstas.
- **Los 12 AVISOS son todos de la emisión:**
  - la v2.7 en HEAD;
  - CA-T2b;
  - las nueve rutas de `CUSTODY_EXTRA`;
  - la entrada de emisión, después de F4B-E35.
- **Han desaparecido los del cierre de PB-T2a,** también F4B-E35, que ya está.
- **Las cifras:** `stops` 35, por C18, y `authorities` 45, por F4B-E35. El resto sigue igual: alcance 8/167, `inv` 15, `required` 38, `evidence` 9, y anclas y rangos con sus 38 autopruebas.

### 11.6 Decisiones del Owner (punto 5)

**Ninguna nueva.**
- I-01 hace operativa la segunda frase de RR-7, que el Owner ya aprobó, en las comprobaciones de 16.9 que implementan 16.8.
- Las menores son de texto y de procedimiento.
- **Si la sesión lee que cambiar la lectura de 16.9 n.º 10 va más allá de «excepciona 16.8»,** esa sí sería una pregunta para el Owner. Yo no la leo así: la n.º 10 es la comprobación de la regla de 16.8 («el GREEN no toca `RT ∪ ChainRedFiles`»), y RR-7 dice que la verificación lee la reparación como parte de la entrega.

### 11.7 Hallazgos nuevos

| Id | Sev. | Dónde | Hallazgo | Qué hacer |
|---|---|---|---|---|
| I-01 | I | §6.0, «La reparación del RED» (l.1097-1104); la autoridad F4B-E35 del contrato; el texto de la verificación | **Falta la segunda mitad de RR-7:** el contrato no dice cómo lee la verificación del Controller una reparación.<br>**El problema concreto:** la `RedPart` de 16.9 n.º 10 (`Tests`) exige `git diff --name-only RedSha CurrentSha` sin tocar `RT ∪ ChainRedFiles`. Toda reparación toca `RT`, así que, al pie de la letra, sale `Tests` = `fail`, que es REWORK, y el intento que RR-7 promete no gastar se gasta.<br>**Además:** n.º 11 `Trailer` revisa todos los commits `BaseSha..CurrentSha`, también el de reparación | **Decirlo en el §6.0, en la sección de la autoridad F4B-E35 y en el texto de la verificación, como F4B-E25 lo dijo de 16.3.** Con una reparación:<br>- `Ci` y `Tests` acreditan el RED con `RedSha` (su CI con Core en `failure` y su TRX, con los `ExpectRed` entre los fallidos);<br>- la cláusula de `Tests` sobre `RT ∪ ChainRedFiles` se comprueba desde `RedRepairSha` (`git diff --name-only RedRepairSha CurrentSha`);<br>- el Controller comprueba RR-1, RR-2, RR-4, RR-5 y RR-6 como en §11.4;<br>- `Trailer` exige el trailer también en el commit de reparación.<br>Si alguna falla, es C18 |
| M-01 | M | §6.0, RR-3; la verificación | **El problema:** RR-3 queda entero a la revisión, sin ninguna ayuda mecánica | Añadir las comprobaciones mínimas de §11.4: las líneas literales del paquete, byte a byte, en la clase reparada; ningún hunk en el primer argumento de un `Assert.Equal` ni en una cadena `<campo>=`; el resto, a la revisión con la declaración |
| M-02 | M | `ExpectedEvidence`, «la entrega» | **El problema:** «el trailer real de los commits RED y GREEN» no nombra el commit «RED repair», y `Trailer` (16.9 n.º 11) lo revisa | Escribir «de los commits RED, RED repair (si lo hay) y GREEN» |
| M-03 | M | PBT2B-C07 y PBT2B-C18 | **El problema:** el contrato lleva solo la parte negativa de RR-3 («changes no expected line, literal or oracle»). La positiva («solo el cuándo y el cómo de una lectura o de un gesto que el paquete ya prescribe») se queda en el paquete | Añadir a C18: «or it changes anything other than the when or the how of a reading or a gesture the package already prescribes (RR-3)» |
| M-04 | M | `make_pbt2b_contract.py`, `bounded_check` (:1692-1732) | **El problema:** PB-T2b no recibió la versión robusta de M-07 de CA-T2c que sí tiene PB-T2c (`make_pbt2c_contract.py:1479-1495`). Así, «la misma función en los dos generadores» de V25-01 ya no se cumple, y en PB-T2b una sección sin número sigue lanzando `IndexError` o se ignora | Copiar la función de PB-T2c, con su revisor, y su prueba en negativo con «§R» |

## §12. Recomprobación sobre la v2.8

**Veredicto de §12: EMITIBLE. Nuevos: B 0 / I 0 / M 2.**

**El veredicto en breve:**
- **I-01 de la §11 está corregida.** Una reparación del RED válida ya no puede acabar en REWORK por la letra de 16.9, y una inválida no pasa: cualquier incumplimiento de RR-1, RR-2, RR-4, RR-5 o RR-6 es C18.
- **M-01 a M-04 de la §11 están corregidas.** M-01 lo está con la precisión de la sesión: parada C07 solo si una línea literal no sigue byte a byte; los indicios heurísticos van a la revisión.
- **Las dos menores nuevas** son de precisión (M-05) y de mecanizar la verificación (M-06). No impiden emitir.
- **Nada de lo cerrado en las §1 a §11 se rompe.**
- **No hay decisiones nuevas para el Owner.**

**`BOUNDED_RESULT` final (ASCII):**

`EMITIBLE TRAS CORRECCIONES (B 0 / I 3 / M 7; I-01, I-02 e I-03 corregidas en el paquete; recomprobacion v2.4: EMITIBLE, B 0 / I 0 / M 4; M-08 a M-11 aplicadas en la v2.5; recomprobacion v2.8: EMITIBLE, B 0 / I 0 / M 2)`

Lo comprobé con la función real (§12.6): con el informe completo, esta §12 incluida, y las líneas de la v2.8, el `bounded_check` robusto (V28-05) no da ningún error.

### 12.1 Método y límites

- **Qué se leyó:**
  - del repositorio, solo objetos git de `b29551ae` desde el checkout principal;
  - del borrador: la v2.8 y `v28-delta.md`, el generador, su seco (sha `396a047b…`, el del encargo), `neg-pbt2b.txt` (110 de 110 pruebas en negativo y 15 casos de `bounded_check`) y las notas, con D-21 y D-22.
- **La comprobación del literal:** con un guion propio de solo lectura, `recheck_bounded_v28.py`, escrito con la herramienta de archivos y lanzado con `python -I`. Extrae la función `bounded_check` del generador, sin ejecutar el resto del generador, y la aplica al informe y al paquete.
- **Qué no se hizo:** no compilé y no toqué el worktree.
- **Una incidencia de procedimiento, declarada:**
  - una orden de Bash llevó por error `python -I -c ""` delante de un `grep` del contrato en seco;
  - no ejecutó nada, porque el programa estaba vacío, pero es Python con `-c`, contra la regla;
  - es la segunda vez en estas rondas (la primera está en el §1 de CA-T2c), y no se repitió.

### 12.2 I-01: la verificación con una reparación (punto 1)

**Dónde está:**
- V28-01 (l.72);
- el §6.0, «La verificación con una reparación del RED» (l.1119-1127);
- la autoridad F4B-E35 del contrato;
- una entrada nueva de la `ExpectedEvidence` (de 9 a 10).

**Lo que dice, frente a cada comprobación de 16.9:**

| Comprobación | Con una reparación (v2.8) | ¿Una válida pasa? ¿Una inválida para? |
|---|---|---|
| **n.º 10 `Tests`** | El RED se acredita con `RedSha`, su CI original y sus TRX. La cláusula sobre `RT ∪ ChainRedFiles` se comprueba desde `RedRepairSha` | **La válida pasa:** el archivo reparado queda en `RedSha..RedRepairSha`, fuera del tramo que se comprueba.<br>**La inválida para:** si el GREEN tocara `tests/` después, la cláusula lo ve desde `RedRepairSha` |
| **n.º 11 `Trailer`** | Incluye el commit «RED repair». La entrega lo declara (V28-03, l.1164) | **Pasa** si lleva el trailer; si no, el REWORK de siempre, que es lo correcto |
| **RR-1** | Un solo commit «RED repair», hijo directo de `RedSha` y anterior al GREEN | **Para (C18):** dos reparaciones, una reparación después de un commit de `src/`, o una reparación que no cuelga de `RedSha` |
| **RR-2 y RR-6** | `git diff --name-only RedSha RedRepairSha` ⊆ `git diff --name-only --diff-filter=A BaseSha RedSha -- tests/` | **Para (C18):** tocar el archivo del modo o la costura (`src/`, aunque los añadiera el RED), una prueba existente o un fixture |
| **RR-4** | La CI de `RedRepairSha`, registrada como la del RED reparado, y sus TRX | **Para (C18)** si faltan |
| **RR-5** | La declaración con el diff, y `RedSha` el original | **Para (C18)** si falta |
| **n.º 5, 7 y 9** (`Identity`, `Scope`, `Ci`) | Sin cambios | `RedSha` sigue entre `BaseSha` y `CurrentSha`; la reparación cae en el `AllowedWriteScope`; `Ci` acredita el RED con la corrida de `RedSha` |

**Un caso que conviene saber:** una reparación no rescata un RED que no cumplió su propio criterio (PBT2B-C04, un `ExpectRed` que falla por una excepción en el RED), porque n.º 10 acredita el RED con `RedSha`. Es fiel a F4B-E35: la reparación es para un error que «solo aparece al probar la implementación después del commit RED».

### 12.3 M-01 a M-04, y D-21 (punto 2)

| §11 | Estado | Dónde |
|---|---|---|
| **M-01** | Corregida, con la precisión de la sesión | V28-02 (l.73); el §6.0 (l.1124 y l.1126); C07, que dice «a literal line the package fixes for the repaired class … that does not stay byte for byte in it, is this stop»; la `ExpectedEvidence`. Los indicios (`<campo>=` añadido o quitado, el primer argumento de `Assert.Equal`) van a Findings y a la revisión, no son parada |
| **M-02** | Corregida | V28-03 (l.74): la entrega nombra los commits RED, «RED repair» (si lo hay) y GREEN (l.1164) |
| **M-03** | Corregida | V28-04 (l.75): C07 «changes only the when or the how of a reading or a gesture the package already prescribes»; C18 «or anything other than the when or the how … (RR-3)» (l.1118) |
| **M-04** | Corregida | V28-05 (l.76): el `bounded_check` de PB-T2b es la función robusta (`make_pbt2b_contract.py`), y `neg-pbt2b.txt` prueba «§R», con encabezado y veredicto, sin veredicto y desemparejado |

**D-21: «las líneas literales que el paquete fija para sus clases».**
- Las tres listas de CT-PBT2B-03 están bien acotadas: son literales y la prueba las escribe enteras, con `<R>`, `<A>` y `<C>` expandidos por su definición del §5.3.
- **«Las líneas fijas de CT-PBT2B-01 a 07» no es una lista,** y el añadido «si alguna otra línea del paquete es literal para una clase, entra en la misma regla» la deja abierta. Para una regla que es parada, un conjunto abierto puede dar una parada discutida o una que no salta.
- Además, «byte a byte, como en `RedSha`» solo funciona con las líneas que la prueba escribe como literal. Una línea que la prueba compone en código, por ejemplo con una constante de D-PBT2-09 o con `Sel(entrada)`, no tiene bytes que comparar y cae en los indicios.
- **Lo que hace falta (M-05):** que el generador escriba la lista cerrada en la salida de los anclajes, y que la regla diga que se aplica a las líneas de esa lista que la clase de `RedSha` lleva como literal.

### 12.4 D-22: dónde se le dice al Controller (punto 3)

**Basta en contenido.**
- El contrato no tiene un campo para la verificación.
- La `ExpectedEvidence` es lo que la verificación coteja, y la autoridad F4B-E35 es lo que lee según 16.3. Las dos dicen lo mismo, con el mismo detalle de la tabla de §12.2.
- F4B-E25 hizo lo mismo.

**Para que el Controller lo aplique sin depender de su lectura (M-06):**
- F4B-E15 mostró que el Controller pasa por alto cosas en las entregas grandes, y se arregló con comprobaciones deterministas por guion en el texto de la verificación.
- Lo mismo conviene aquí. Con una reparación, el texto de la verificación debería añadir por guion lo de §12.2: RR-1 (el padre de `RedRepairSha` y el título del commit), RR-2 y RR-6 (los dos conjuntos de archivos), n.º 10 desde `RedRepairSha`, y la comparación byte a byte de M-01.
- No cambia el contrato.

### 12.5 Lo nuevo no rompe lo cerrado (punto 4)

- **El seco real sobre `b29551ae`:**
  - rc 0, `evidence` 10 (antes 9), `stops` 35 y `authorities` 45;
  - el alcance 8/167, `inv` 15, `required` 38, los anclajes (100 líneas de 72 citas, 10 rangos con sus 38 autopruebas) y los datos de las recetas no cambian;
  - 13 AVISOS, todos de la emisión: los 12 de la v2.7 y `v28-delta.md`.
- **El texto que compara el generador** gana las nueve líneas de la verificación y los nuevos C07 y C18. Las 110 pruebas en negativo, entre ellas V01 a V18 de la v2.8, fallan todas.
- **Lo de las §1 a §11 sigue igual:** el momento de las lecturas, los gestos con 4.5, la sonda, las costuras, CT-07, la custodia de los encargos y la base (`PT2A_CLOSE`, `CI_BASE_T2A`).

### 12.6 El `BOUNDED_RESULT`, comprobado

`recheck_bounded_v28.py` aplica la función `bounded_check` de `make_pbt2b_contract.py` a este informe entero, con esta §12, a las líneas de la v2.8 y al literal de arriba. La salida está en `recheck_bounded_v28.txt` y da 0 errores.

Lo que hace pasar el literal:
- **El primer veredicto:** `EMITIBLE TRAS CORRECCIONES. B 0 / I 3 / M 7`.
- **Las importantes:** las filas `| I-01 | I |` a `| I-03 | I |` y V24-01 a V24-03, cada una con `**CA-T2b I-0n**`.
- **Las recomprobaciones:** §10, §11 y §12, cada encabezado emparejado con su veredicto.
- **La última, §12:** EMITIBLE con B 0 e I 0, y nombrada en el literal (`recomprobacion v2.8: EMITIBLE, B 0 / I 0 / M 2`).

### 12.7 Decisiones del Owner (punto 5)

**Ninguna nueva.**
- La v2.8 hace operativa RR-7 sin ampliarla. La ayuda de RR-3 en dos grados es más estrecha que la regla, porque solo es parada lo que la viola sin duda.
- M-05 y M-06 son de procedimiento.

### 12.8 Hallazgos nuevos

| Id | Sev. | Dónde | Hallazgo | Qué hacer |
|---|---|---|---|---|
| M-05 | M | El §6.0 (l.1124); la `ExpectedEvidence`; D-21 | **El problema:** «las líneas fijas de CT-PBT2B-01 a 07» no es una lista cerrada, y «si alguna otra línea… entra en la misma regla» la deja abierta para una regla que es parada. Además, una línea compuesta en código no tiene bytes que comparar | El generador escribe en la salida de los anclajes la lista cerrada de líneas literales de PB-T2b (las tres de CT-03, expandidas, y las de CT-01 a 07 sin marcadores `<…>`). La regla se aplica a las de esa lista que la clase de `RedSha` lleva como literal; lo demás, a los indicios |
| M-06 | M | El texto de la verificación (fuera del contrato) | **El problema:** las comprobaciones de RR quedan a la lectura del Controller | Con una reparación, añadirlas por guion, como F4B-E15: RR-1, RR-2, RR-6, n.º 10 desde `RedRepairSha` y la comparación byte a byte |
