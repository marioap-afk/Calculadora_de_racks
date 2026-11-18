# I-64 F4B PB-TC2 — Revisión adversarial independiente del código (intento 1)

**Objeto:** `git diff eedb069c..b5c09f0b` en la rama `architecture/workspace-persistente-rackcad`.
- RED `3677847c`: solo los dos archivos de prueba nuevos.
- GREEN `b5c09f0b`: solo W y WC.

**Modo:** solo lectura, con git, lectura de archivos y tres guiones de Python de solo lectura sobre los TRX, los logs y el contrato. No compilé, no ejecuté pruebas ni dotnet, no escribí en el worktree y no usé la red. No leí las revisiones previas de la sesión.

**Alias:**
- W = `src/RackCad.UI/Systems/PushBack/RackPushBackSystemWindow.xaml.cs`;
- WC = `…/RackPushBackSystemWindow.Composite.cs`;
- CS = `src/RackCad.Application/Systems/PushBack/PushBackCompositeEditorState.cs`;
- ART = `artifacts/orchestration/I-64/F4B-PBTC2-DECLARED-CORRECTIONS/1`.

Si no digo otra cosa, las líneas de W y WC son las del GREEN.

## Veredicto

**ACEPTABLE CON MENORES.**

| Bloqueantes | Importantes | Menores | OWNER |
|---|---|---|---|
| 0 | 0 | 4 | 1 (informativo, no bloquea) |

**El código del GREEN es, línea a línea, el diseño D-PBTC2-01 a 05.**
- No encontré ningún camino por el que `MixedBoth` escriba en los dos lados un valor que el usuario no escribió.
- No hay ningún lector de las seis cajas fuera de los sitios corregidos.
- No hay ningún cambio de comportamiento sin declarar.

**Los cuatro CT** tienen la clase, el método y las líneas del §4 sin ajustar, y discriminan.

**El RED cumple el criterio mecánico.**

**La medición `m8b` es idéntica byte a byte a `m8`,** así que continuar por D1 es correcto.

**Los menores son de registro y de método,** no de producto.

## Hallazgos

| Id | Sev. | Hallazgo | Evidencia | Qué hacer |
|---|---|---|---|---|
| IRC-M01 | M | **`PushBackDeclaredCorrection2MixedBoxTests` declara solo `NumR`.** El §4 del paquete dice «en **cada** clase nueva, tres miembros privados estáticos» (`NumR`, `BoolR` y `StrR`). INV-PBTC2-07 («los únicos ayudantes nuevos son…») se cumple, porque es una cota superior. La entrega lo explica en `Evidence`, pero no lo lista en `Deviations` | `tests/RackCad.UI.Tests/PushBackDeclaredCorrection2MixedBoxTests.cs:31-49`; paquete `docs/initiatives/I-64-f4b-pbtc2-gate-contract-package.md:282-287`; contrato `gate-contract.json:305`; `ART/R20261118T113927Z-037c/worker-handoff.json:868` | Registrarlo como desviación declarada en el cierre. Sin cambio de código |
| IRC-M02 | M | **El método temporal del paso 1 no corre dentro de `EditorDiscardPrompt.Substitute`.** Cierra 12 ventanas con `Cerrar`. Si alguna hubiera quedado con trabajo pendiente (T2-2 escenifica la longitud y pulsa «Confirmar»), habría saltado el `MessageBox` real de producción (riesgo de PBTC2-C05 y C09). **No se materializó:** `m8b` corrió en unos 7,5 s (`<Times>` del TRX) y no se colgó. El código no entró en ningún commit | `ART/R20261118T055535Z-38d3/temp-measurement-t2.patch:162-193` y `:320-396`; `ART/R20261118T113927Z-037c/tests/m8b/m8b.trx` (`start` 05:44:51, `finish` 05:44:58, hora local) | Lección para futuras corridas temporales. Nada que corregir en PB-TC2 |
| IRC-M03 | M | **La continuación tras PBTC2-C14 descansa en D1 (F4B-E11), que en `b5c09f0b` no está custodiado.**<br>• No existe `docs/automation/evidence/I-64-pilot/F4B-DRAFTS/coordinator/owner-d1-d2-decisions.md`, y `f4b-dispositions.md` termina en F4B-E10 (`:319`).<br>• La entrega declara `TriggeredStopConditions: []`, mientras su propia transcripción de `m8b` termina en `STOPS=[T2-3 PBTC2-C14: distinto numero de lineas del BOM]`.<br>• Es coherente con la orden de la sesión al Worker («repite el paso 1… si es idéntica… no es una parada»), pero la trazabilidad depende de esa custodia | `ART/R20261118T113927Z-037c/worker-handoff.json:850` y `:888`; `ART/R20261118T113927Z-037c/prompt.md:146-147`; D1: «Preserve the stop and Owner decision in evidence» (`ART/R20261118T113927Z-037c/owner-d1-d2-decisions.md:69-80`) | La sesión custodia D1 y D2 y anota F4B-E11, con la tabla m8/m8b, antes del cierre. No es del Worker (INV-PBTC2-12) |
| IRC-M04 | M | **El efecto R-03 (i) solo se declara para la longitud del módulo.** El efecto es este: escribir el número que la caja ya muestra conserva el exacto, y no hay forma de redondear tecleando el mismo texto. Con el GREEN vale igual para el hueco y para las cuatro avanzadas anulables. Para llegar exactamente a «150», hay que pasar por otro número, porque vaciar es «el cálculo», no 150.<br>Es la regla `Keep` que el Owner eligió (R-03 (i), F4B-E06; F4B-E09, «con la regla Keep»), así que **no es una decisión nueva**. Pero `KnownLimitations` y D-PBTC2-02 solo la nombran para el módulo | WC:198; W:1639, :1641, :1642, :1646 y :3687-3688 (`KeepOptional`); `worker-handoff.json:878` (037c); paquete `:175` y `:700` | Una línea en la sección de PB-TC2 de [SC] o en `KnownLimitations`: R-03 (i) vale para las seis cajas. Sin cambio de código |
| IRC-O01 | OWNER | **La línea nueva del BOM ratificada en D1 lleva además OTRA descripción.**<br>• En T2-3, los 4 postes de 150,0004 salen en su propia línea con la descripción larga del catálogo, con saltos de línea: «Poste omega atornillable con / troquel gota de agua de cinta / calibre 14». Los 32 de 150 conservan «Poste Omega 3x3 calibre 14». Es el **mismo** `ProfileId`.<br>• **D1 describe el efecto solo por cantidades y longitudes** («32 @ 150 / 4 @ 150.0004»).<br>• **La causa:** `BomBuilder.Merge` agrupa por (categoría, `ProfileId`, longitud) y toma la primera descripción no vacía. El BOM de la cabecera usa la descripción del catálogo, y el catálogo tiene las dos.<br>• **No lo introduce PB-TC2:** ya pasa hoy al cargar sin tocar el panel de avanzados (E = la base sin gesto). PB-TC2 lo hace estable. D1 admite «the existing display/formatting rules», así que probablemente cae dentro, pero el Owner no vio este detalle | `ART/R20261118T055535Z-38d3/worker-handoff.json:112` y `:118-120` (BOM-E[0] y E[6]) frente a `:129` (BOM-R[0]); `src/RackCad.Application/Bom/BomBuilder.cs:97-104` y `:116-126`; `assets/catalogs/secciones.csv:2-4`; D1 `owner-d1-d2-decisions.md:31-45` | Informar al Owner al cerrar, sin proponer cambio: la línea partida de postes también se ve con otra descripción |

## Diseño → código (GREEN `b5c09f0b`)

| Decisión | Qué fija el paquete | Qué hace el código | ¿Exacto? |
|---|---|---|---|
| D-PBTC2-01 | WC:198 pasa a `composite.SetGap(Keep(GapBox, composite.Gap));`. La guarda (WC:193) y la siembra (WC:746) no cambian | El mismo texto en WC:198. WC:193 y WC:746 intactas. Sin edición, `Keep` devuelve `composite.Gap` (el exacto). Con un número que se ve distinto, manda la caja | Sí |
| D-PBTC2-02 | W:1905 pasa a `Keep(ModuleLengthBox, module.Length)`. W:1906 y W:1911 no cambian | El mismo texto. Si la caja se ve igual, `length == module.Length`, así que \|Δ\| = 0 < 0,0001 y sale: **no escenifica nada y un módulo calculado sigue calculado.** Lo fija CT-02, ventana 1 | Sí |
| D-PBTC2-03 | W:1639, :1641, :1642 y :1646 pasan a `KeepOptional(<caja>, advanced.<campo>)`. W:1640 y W:1643-1645 no cambian | El mismo texto. `KeepOptional`: **vacía es `null` (el cálculo)**; si se ve igual al portador, el portador; si no, la caja. Es la rama con valor de la `OptionalVal` de la base. El portador `advanced` y las cajas se siembran juntos en W:406-411 y :1595-1619, y en los dos «Restaurar» (W:700-701 y :1670-1671). Nadie más escribe el portador | Sí |
| D-PBTC2-04 | W:818-822: `Keep` pasa a `KeepCell` con el mismo segundo argumento. W:3667 (de la base) pasa a `? (MixedBoth(field) ? field.Value : KeepOptional(field, sideValue))`. **La rama vacía queda literal.** `MixedBoth` = la condición de WC:452 | W:818-822, idénticas salvo el identificador. W:3665 es el texto pedido. **W:3663, :3664 y :3666 son byte a byte las de la base,** con la rama vacía `: (IsMixed(field) ? sideValue : null);` intacta, así que M-07 se conserva. W:3692 = `IsMixed && Both && SideBPresent`. Si `MixedBoth` es falso, `KeepCell` es `Keep` y `OptionalVal` es la regla de hoy | Sí |
| D-PBTC2-05 | Insertar `KeepOptional`, `MixedBoth` y `KeepCell` entre `Keep` e `IntVal`, con su texto exacto y un `<summary>` de una línea sin tokens prohibidos. Retirar `Val` (base 3634-3635). Corregir los dos `cref` | El hunk `@@ -3687,0 +3686,12 @@` inserta los tres miembros con el texto exacto, en ese orden. Los tres `<summary>` no contienen `Val(`, `Keep(`, `KeepOptional(`, `KeepCell(`, `MixedBoth(` ni `IsMixed(`. `@@ -3634,2 +3633,0 @@` retira `Val` y su línea en blanco. W:3639 dice `(hoy <see cref="Keep"/>)` y W:3681 termina en `"current"/>.` | Sí |
| INV-PBTC2-03 | Cada hunk en su sitio | 11 hunks: WC:198; W:818-822; W:1639; W:1641-1642; W:1646; W:1905; W:3634-3635; W:3641; W:3667; W:3683; y la inserción tras 3687. **Ninguno fuera de la lista** | Sí |
| INV-PBTC2-04 | Las cuentas | `\bVal\(` vacío; `cref="Val"` vacío; `Keep(GapBox, composite.Gap)` = 1 (WC); `Keep(ModuleLengthBox, module.Length)` = 1; `KeepOptional(` = 6; `KeepCell(` = 6; `MixedBoth(` = 3; `IsMixed(` = 2 (W). Las medí con `git grep` sobre `b5c09f0b` | Sí |
| D-PBTC2-06 e INV-02 y 05 | Nada más cambia | `SetNumber(` no aparece en ninguna línea `+` ni `-`. `forceRebuild:` y `forceRebuild)`: 0. `.RequestUpdate(`: 1. WX, `NumericField`, Cantilever, Application y el Plugin no están en el diff | Sí |

### Las preguntas del encargo sobre el código

**`MixedBoth`: ¿puede escribir en los dos lados un valor que el usuario no escribió?** No, por estos motivos:
1. **La única siembra de las seis cajas es W:467-476** (`LoadSelectedFront`). Con lado B, siempre va seguida de `LoadCompositeCellPanel` (W:489), luego `ApplyMixedSideState` (WC:634), luego `SetMixed`. `SetMixed(true)` vacía la caja (WC:581). `git grep` no encuentra ninguna otra escritura de esas cajas en `src/`.
2. **«Ambos» implica lado B presente en CS:**
   - `SetActiveSelection` cae a A sin B (CS:203-208);
   - `SetSideBPresent(false)` pone A (CS:294-302);
   - volver a marcar B deja A.

   Toda llegada a «Ambos» pasa por `SideSelector_Changed`, que recarga el panel (WC:163-177).
3. **Por tanto,** con «Ambos» y B, una caja marcada solo puede estar vacía o llevar lo que el usuario tecleó.
4. **Todo cambio de celda confirma antes de sembrar:**
   - `SelectMatrixCell` (W:1094-1096);
   - `SelectSingleCell` (W:1199-1201);
   - `MutateStructure` (W:1214-1221).

   El «cambio de celda» aplica, pues, lo escrito a la celda en la que se escribió. Es la extensión declarada (M-03).
5. **Residuo sin efecto:** tras escribir V, el indicador sigue en pie hasta la siguiente recarga, y cada confirmación vuelve a aplicar V a los dos lados. Es idempotente, porque los dos ya valen V, y todo camino que cambia la celda recarga antes.
6. **Indicador obsoleto sin B (H2-11):** `MixedBoth` es falso porque la selección es A, y `Keep` conserva el exacto.

**¿Algún lector queda fuera?**
- Solo leen la celda tres sitios: la confirmación (W:761), el alcance (W:1346) y los datos del frente (W:1510). Todos pasan por `ReadCellValues`. El de W:790 no tiene llamadores.
- `GapBox`, `ModuleLengthBox` y las cuatro avanzadas no tienen más lectores en W, WC ni en el resto de `src/`. Solo W y WC son la clase parcial.
- `DerivedPostLineHeightBox` (W:2133) se lee crudo, pero nunca se siembra: es una acción explícita con botón, fuera de H-31.

## Los CT

| CT | Clase y método | ¿Líneas del §4? | ¿Fija lo que promete? | Negativos | Reglas de ventana | RED (TRX de la CI) |
|---|---|---|---|---|---|---|
| CT-PBTC2-01 | `PushBackDeclaredCorrection2RoundingTests.CT_PBTC2_01_AnUneditedGapBoxKeepsTheExactGapUpToTheUpdate` (`:95-149`) | **Sí, 5 de 5, literales** | Sí. La línea 3 y la petición de «Actualizar» separan `Keep` de la base (54). La ventana 2 descarta un producto que no escriba nunca | La ventana 2 (escribir 60 manda) | `Ventana()`; la carga dentro del `try`; `Cerrar` en `finally`; `Substitute` con contador por método; sufijo leído tras cerrar; sin `Show` | Falla en la posición 2: `GapBox sin editar Gap=54` |
| CT-PBTC2-02 | `…RoundingTests.CT_PBTC2_02_PassingThroughTheModuleLengthKeepsACalculatedModuleAndAnEditMakesItManual` (`:151-217`) | **Sí, 5 de 5** | Sí. `pendiente=False` y `IsCalculated=True` separan `Keep` de `Val` (en la base: `pendiente=True` y 47,992 manual) | La ventana 2 (56 lo vuelve manual) | Las mismas | Falla en la posición 1: `sin editar pendiente=True` |
| CT-PBTC2-03 | `…RoundingTests.CT_PBTC2_03_UneditedNullableAdvancedBoxesKeepTheExactValueAndAnEmptyBoxIsTheCalculation` (`:219-320`) | **Sí, 10 de 10** | Sí. La línea 3 de las dos ventanas separa las cuatro anulables. La línea 4 prueba que editar una caja no redondea la otra | La línea 4 (lo editado manda) y la línea 5 (vacía = `<null>`, que descarta un `KeepOptional` que devuelva el portador) | Las mismas; `AdvancedModulesToggle` antes de los gestos | Falla en la posición 2. El valor no se ve en el mensaje truncado; es coherente con `m8b` (150 y 18) |
| CT-PBTC2-04 | `PushBackDeclaredCorrection2MixedBoxTests.CT_PBTC2_04_AMixedBoxThatReceivesAValueAppliesItToBothSidesAndANonMixedBoxKeepsTheExactValue` (`:51-189`) | **Sí, 13 de 13** | **Sí:**<br>• **mixta:** A=61,235 frente a 61,23456 de la base; largo 100/100 frente a 100,00044/100;<br>• **no mixta:** descarta «aplicar siempre en Ambos»;<br>• **sin lado B:** `SelectMatrixCell` termina en `RequestRecompute` (W:1099), que es síncrono. Así se confirma la caja sembrada con el indicador obsoleto, y un producto que solo mire `IsMixed` escribiría 61,235;<br>• **cambio de celda:** confirma antes de cambiar.<br>Cubre `CellPalletHeightBox` y «Largo manual». Las otras tres cajas de celda con `KeepCell` (`CellPalletFrontBox`, `CellPalletWeightBox` y `CellClearBox`) y `FirstLevelHeightBox`, que es del frente, solo las fija la cuenta de INV-04. Su segundo argumento es idéntico al de la base | No mixta, sin lado B y cambio de celda | Las mismas; cuatro ventanas con su sufijo | Falla en la posición 2: `A.alto=61.23456` |

**Los cuatro métodos:**
- son `[Fact]` y terminan en un único `Assert.Equal`;
- van en `[Collection(EditorDiscardPromptCollection.Name)]` y son `public sealed`;
- no tienen ninguna aserción dentro de la costura;
- no usan fixture nuevo, ni teclado o foco real, ni `MessageBox`, BOM, biblioteca, configurador o «Insertar»;
- tienen cultura invariante (`Sta`), GUID fijo y «R».

En las ventanas que cierra «Actualizar» hay además un `Cerrar(w)` explícito antes del sufijo. Es inocuo, porque `Cerrar` es idempotente (S0 `:71-77`).

## El RED

**La CI del RED** (`ART/R20261118T113927Z-037c/red-ci-ui/ui.trx`, el mismo SHA-256 que `tests/redci/ui/ui.trx`) da:
- 2422 resultados: 4 Failed, 21 NotExecuted y 2397 Passed;
- los 4 Failed son exactamente los métodos nuevos;
- `Message` = `Assert.Equal() Failure: Collections differ`, sin el prefijo de tipo de INV-02;
- la primera línea del `StackTrace` cae en `tests/RackCad.UI.Tests/PushBackDeclaredCorrection2*.cs` (`:139`, `:207`, `:305` y `:171`);
- ninguna prueba existente falla;
- los 21 NotExecuted son **el mismo conjunto** que en la CI del GREEN.

**Core:** 19 070 Passed.

**La corrida local `r1`** da lo mismo, 4 de 4 fallos por aserción.

**El RED no toca `src/` y el GREEN no toca `tests/`.**
- Los árboles: base `e79148f0`, RED `fc8d6ed4` y GREEN `6d5d2dfc`, iguales a los `TreeSha` de la entrega.
- Las cuatro rutas están en el `AllowedWriteScope` (4) y ninguna cae bajo el `ForbiddenWriteScope` (129).
- WX no cambia.

## La medición previa y D1

**Las corridas:**
- `m7` tiene 51 líneas, todas contenidas en `m8`. `m8` solo añade el detalle del BOM.
- **`m8` y `m8b` dan el mismo `Message` del TRX, byte a byte, con 82 líneas.** La comparación «idéntica» se sostiene sobre los TRX, no solo sobre el texto de la entrega.

**La fórmula del parche es la de CT-PB0-21, copiada:** K1, K1o, K2, K3, K4, K4', K5 y `MaxDelta` (`tests/RackCad.Tests/PushBackFullEditorCharacterizationRoundingEffectTests.cs:107-245`).

**Los resultados:**
- La única condición de PBTC2-C14 es el número de líneas del BOM de T2-3.
- `MaxDelta` máximo = 0,00044 < 0,0005.
- Las diferencias del BOM son ≤ 0,0004.

**Continuar por D1 sin otra vuelta es correcto:**
- D1 dice «CONTINUE WITH THE FIX», «resume/relaunch… WITHOUT consuming a product attempt» y «No new planning is required unless the contract itself changes».
- `m8b` no trae ningún efecto distinto del ratificado.
- `Attempt` sigue en 1.
- Queda la custodia (IRC-M03) y el detalle de la descripción (IRC-O01).

**El código temporal se retiró por completo:**
- `TEMP_`, `Medicion`, `Fila`, `Medir` y `BomRows` no aparecen en el RED ni en el GREEN;
- el `write-tree` final de 38d3 es el árbol de la base;
- `git status` está limpio, sin `.cs` ignorados bajo `src/` ni `tests/`;
- el parche solo creaba el archivo de pruebas.

## El GREEN y la estabilidad

**La CI del GREEN** (`ci-ui`, el mismo SHA que `tests/gci/ui`):
- UI: 2422 resultados, 0 Failed, 21 NotExecuted y 2401 Passed;
- Core: 19 070.

Son los conteos previstos en el §6.9.

**En local:**
- `g1` a `g3`: 4 de 4, tres veces, entre 0,4 s y 2,2 s cada método;
- los filtros de `RequiredTests`, en verde con el mínimo de seleccionados;
- los avisos por código, idénticos en b0, b1 y b2: UI 14 (xUnit), Core 66 y Plugin 2. **Ninguno viene de los archivos nuevos.**

**No encontré dependencias** de orden, reloj, cultura, rutas, foco ni ventanas:
- la colección se serializa;
- `Sta` fija la cultura invariante;
- los fixtures se leen por la raíz del repositorio;
- no hay `Show`;
- `LostFocus` se eleva a mano;
- `RecomputeGate` es síncrono;
- toda ventana se cierra en `finally`;
- la costura es un estático de proceso que se restaura con `using`.

## Contención

- **La prueba de limpieza** (`clean-before` = `clean-after`) es idéntica byte a byte **dentro** de cada invocación. Entre invocaciones cambia el formato del listado de la biblioteca, cosa que la entrega declara.
- **Los dos instantáneos de procesos** de cada invocación no muestran `acad`.
- **El Plugin** se compiló con `-o` bajo `ART/…/plugin-out`. El ensamblado del paquete del Autoloader solo corre tras `Publish` (`src/RackCad.Plugin/RackCad.Plugin.csproj:56`), así que el build no escribe en el perfil.
- **Las pruebas** no escriben en disco: `Persist` serializa en memoria y los fixtures se abren solo para leer.

## OWNER

- **IRC-O01** (informativo, no bloquea): la línea del BOM que D1 ratificó también aparece con otra descripción para el mismo `ProfileId`. Ya ocurre hoy si no se toca el panel de avanzados; PB-TC2 lo vuelve estable. No propongo ningún cambio.

## Lo no verificado

- **Nada se ejecutó.** Que la ventana «sin lado B» tumbe un producto que solo mire `IsMixed` lo razono por lectura de W:1094-1099 y W:635, sin un mutante. Lo mismo vale para que las ventanas «no mixta» y «sin lado B» pasen en la base.
- **El estado de los cuatro jobs** de las CI del RED (`44298811020`) y del GREEN (`44300874797`), y la ausencia de *blame-hang* o *timeout* a nivel de job: solo tengo los TRX y la entrega, sin red.
- **El valor exacto de la línea 3 de CT-03 en el RED:** el mensaje de xUnit lo trunca. Que sea 150 y 18 lo infiero de `m8b`.
- **Que el parche de `m8b` sea byte a byte el código que corrió:** lo infiero de la salida idéntica. No hay copia del árbol temporal.
- **E como sustituto del GREEN en T2-1:** el fixture edita `Composite.Gap` sin tocar el módulo `GAP` guardado (54). Por lectura de `PushBackCompositeStructure.cs:509-520`, una recomposición tras el gesto daría `GAP` = 54,00044, no 54. Sería una diferencia ≤ 0,00044 sin BOM, que es un artefacto del fixture y no está medido.
- **Escrituras en el perfil** fuera de los dos objetos de INV-PBT0-06 (por ejemplo, registros del producto o la caché de NuGet).
- **La fidelidad de la transcripción de D1 y D2** frente al chat del Owner.
