# I-64 F4B — Revisión adversarial independiente del paquete del contrato de PB-TC2, v1

**Revisor:** el de los paquetes de PB-TC, independiente y adversarial, en solo lectura.
**Base:** rama `architecture/workspace-persistente-rackcad`, HEAD `5597c05b5b2f4a24d0baa05555334ff2a66e6c9c`, árbol limpio.
**Veredicto:** **EMITIBLE TRAS CORRECCIONES**: 0 bloqueantes, 3 importantes y 7 menores.
**Decisiones nuevas para el Owner:** ninguna para emitir PB-TC2. El menor M-07 es para el paquete de PB-T1.

**Citas:**
- `TC2:n` es una línea del paquete v1 (`I-64-f4b-pbtc2-gate-contract-package.md`).
- «Medición X-n» es una fila de `pbtc2-measurements.md`.
- **W**, **WC**, **WX**, **S0** y **SC1** son los alias del paquete (TC2:58-63).
- **CS** es `src/RackCad.Application/Systems/PushBack/PushBackCompositeEditorState.cs`.
- Todo `ruta:línea` es de HEAD.

---

## 1. Objeto, método y límites

**Objeto.** Se revisaron:
- el paquete v1 de PB-TC2;
- sus mediciones;
- el encargo del redactor (`drafting-brief.md`);
- las decisiones literales del Owner, [OPT1] (`../pbt1/owner-pbt1-pre-decisions.md`, F4B-E09);
- el expediente del analista, §2.1, §2.2 y §5 (`../pbt1/ob065-dossier.md`).

Se contrastaron con el código y las autoridades del worktree:
- [OD], [DP] y [P];
- `f4b-dispositions.md`;
- [PKC] v2.2 y su contrato [PKC-C];
- el cierre de PB-TC y su revisión independiente;
- [SC];
- el anexo A de PB-TC v2;
- `model-catalog.md`.

**Método.**
- Lectura del código de HEAD con `git -C`, `git grep` y `sed`.
- Cada CT se rehizo por el código, línea a línea.
- **Dos scripts de Python de solo lectura,** escritos con la herramienta de archivos y ejecutados con `python -I`. No se custodian.
  - El primero comprueba la existencia y la unicidad de los 51 archivos de prueba del `ForbiddenWriteScope` y hace el recuento estático de las 18 filas de `RequiredTests`.
  - El segundo lee los cuatro sobres del corpus que usan los CT.
  - Se escribieron primero junto al paquete y se movieron a una carpeta aparte para no mezclarlos con él.
- **Desviación declarada:** se usaron dos líneas `python -I -c`, de solo lectura, para listar los campos del contrato de PB-TC. La regla pide escribir el Python con la herramienta de archivos.
- `gh run view 44232324035`, en solo lectura.

**Límites.**
- No se compiló ni se ejecutó nada, y no se usó AutoCAD.
- Las líneas esperadas y los fallos en la base son lecturas del código, como las del paquete.
- No se revisaron los scripts de la cadena que aparecieron en la carpeta del paquete durante la revisión (`launch_*`, `make_*`, `relay_*`…). No forman parte del encargo.

---

## 2. Los nueve puntos

### 2.1 Punto 1 — Fidelidad a F4B-E09 y a [OD]

**El alcance es exactamente H-31 y OWNER-1.**
- La declaración del alcance está en TC2:12-16, con D-PBTC2-01 a D-PBTC2-04.
- c-02 y `pb-21-cola` quedan fuera de forma explícita:
  - en TC2:16;
  - en el `Objective` (TC2:361);
  - y como parada, PBTC2-C10 (TC2:534).
- No hay nada de PB-T1: ninguna extracción de estado y ningún archivo de Application.
- Cumple el «Do not hide these corrections inside PB-T1» de [OD] (`owner-f4b-disposition.md:152`): la tarea es propia y tiene su presupuesto de 1/3 (TC2:16, TC2:360).

**Las extensiones declaradas** (TC2:635-639):
- **(a) «Ambos lados» al cambiar de celda, en el alcance y en los datos del frente.**
  - La regla vive en `ReadCellValues`, y esas rutas la leen en W:1094, W:1199, W:1214, W:1346 y W:1510.
  - Lo literal del Owner es «la caja mixta de "Ambos lados", que aplica el valor escrito a los dos lados» ([OPT1]:67). No lo limita a la ruta de edición.
  - En PB-TC, el Owner aceptó el mismo alcance para `Keep` (OD-PBTC-1).
  - **No es una decisión nueva.** Pero no tiene CT (M-03).
- **(b) Una longitud de módulo que se ve igual deja de quedar pendiente.**
  - Es R-03 (i) de F4B-E06 («si escribes en la caja algo que se ve igual … se conserva el exacto»).
  - Además, la opción de OD-PBTC-5 lo decía literalmente: «escribir una longitud de módulo que se ve igual a la actual deja de marcarse como cambio pendiente» (`owner-pbtc-decisions.md:81`).
  - Con tres decimales o menos ya era así: la tolerancia de 0,0001 de W:1906 lo impedía.
  - **No es una decisión nueva.**
- **(c) La caja avanzada vacía.**
  - Sigue igual que hoy:
    - `KeepOptional` de una caja vacía da nulo;
    - las seis cajas no son opcionales (WX:492-535) y no están en `AllNumericFields` (W:749-755);
    - `RequestDraw` no mira esas cajas (W:3363-3419).
  - **No es una decisión nueva.**

**Lo que no se sostiene.** TC2:162 dice: «Fuera de "Ambos lados", `IsMixed` es falso y nada cambia».
- Es falso: el indicador puede quedar puesto en un rack de un sentido (I-01, en §2.2).
- El efecto es que el diseño cambia **más de lo declarado**: redondea una caja sin editar.
- Su corrección no exige ninguna decisión: aplica la regla `Keep` que el Owner ya eligió.

### 2.2 Punto 2 — El diseño por línea frente a HEAD

| Cambio | Código en HEAD | Juicio |
|---|---|---|
| D-PBTC2-01: `Keep(GapBox, composite.Gap)` | `Gap_Changed` WC:184-200. La guarda está en WC:193 y la escritura sin comparar en WC:198. La siembra, `GapBox.SetNumber(composite.Gap)`, está en WC:746, dentro de `RefreshCompositePanel` (WC:738-775) y bajo `suppressSync` | Correcto. La guarda asegura el valor y `Keep` solo cambia la escritura |
| D-PBTC2-02: `Keep(ModuleLengthBox, module.Length)` | `ModuleLength_Changed` W:1892-1913: `Val` en W:1905, la tolerancia en W:1906 y `SetLength` en W:1911. `SetLength` pasa el módulo a manual (`RackModuleEditSession.cs:185-203`). Con la caja vacía, `Keep` y `Val` dan lo mismo (`current`) | Correcto. El manejador no recalcula, así que «pendiente» es `HasPendingChanges` (`:162-167`), visible en `PendingWork()` (W:3457-3459) |
| D-PBTC2-03: `KeepOptional` en W:1639, :1641, :1642 y :1646 | El portador es `advanced`. Se siembra en la carga (W:406-411) y las cajas desde él (W:1606-1611). Los dos «Restaurar» reescriben portador y cajas juntos (W:700-701, W:1670-1671) | Correcto. `KeepOptional` es exactamente la rama no mixta de hoy de `OptionalVal` (W:3666-3668) |
| D-PBTC2-04: `KeepCell` en W:818-822 y el `OptionalVal` nuevo | El indicador **sí** sigue en pie al leer tras escribir, por este camino: `Input_Changed` (W:1240) → `RecomputeGate.Request` síncrono (`src/RackCad.UI/Editor/RecomputeGate.cs:35-43`) → `Recompute` (W:616-635) → `CommitCurrentCell` → `ReadCellValues`. Nada de ese camino llama a `SetMixed` (WC:565-590). Con B presente y «Ambos», una caja marcada solo puede estar vacía (WC:581) o llevar lo escrito. La única siembra de esas cajas (W:467-476) va seguida de `ApplyMixedSideState` (W:489 → WC:634) | **Correcto con B presente. Incorrecto sin B: I-01** |
| D-PBTC2-05: retirar `Val` | Un solo uso, en W:1905. Se declara en W:3634 y se cita en dos `cref`, W:3641 y W:3683. Ningún otro parcial lo usa (`git grep` en `Systems/PushBack`). `KeepOptional`, `KeepCell` y `MixedBoth` no existen en `src/` | Correcto |
| Otros lectores y escritores | Los de la medición B-1 son todos. Fuera de `Systems/PushBack`, el único `ModuleLengthBox` es el de otra ventana, la del Dinámico (`RackDynamicSystemWindow.xaml.cs:721`) | Correcto |

**I-01. Un indicador mixto obsoleto hace que `KeepCell` y el `OptionalVal` nuevo redondeen una caja sin editar.**

*Por qué el indicador queda puesto:*
- `mixedNow` solo se limpia en `SetMixed(field, false)` (WC:588).
- `SetMixed` solo lo llama `ApplyMixedSideState`, y esta solo `LoadCompositeCellPanel`.
- `LoadCompositeCellPanel` **sale al principio si no hay lado B**: `if (SlotPresentText == null || !composite.SideBPresent) return;` (WC:604-607).

*El camino, por la interfaz:*
1. En «Ambos lados», una celda tiene A ≠ B (por ejemplo, el alto: A = 61,23456 y B = 60). Su caja queda marcada y vacía.
2. Se desmarca «Rack de dos sentidos (lado B)» (WX:127-129, fuera de `CompositeSection`). `SideBPresent_Changed` (WC:126-145) llama a `composite.SetSideBPresent(false)`, que pone `ActiveSelection = A` (CS:294-302). Después, `SideSelectorBox.SelectedIndex = 0` sale en WC:157-160, porque la selección ya es A. Nada llama a `ApplyMixedSideState`.
3. Se hace clic en cualquier celda. `SelectMatrixCell` (W:1080-1100) llama a `LoadSelectedFront`, que siembra la caja con el exacto con «0.###» (W:470: «61.235»). `LoadCompositeCellPanel` vuelve a salir en WC:604-607, así que el indicador sigue puesto.
4. `RequestRecompute` (W:1099) → `ReadCellValues(A)`. El único destino es A (CS:182-190).

*El resultado:*
- **Paquete:** `KeepCell` da `IsMixed && HasValue` → 61,235. **A queda redondeado sin que nadie edite.**
- **HEAD:** `Keep` conserva 61,23456.
- **El largo manual:** igual con el `OptionalVal` nuevo. Un override sembrado de 100,00044 pasa a 100, porque `IsMixed ? (field.Value ?? sideValue)` da `field.Value`. Hoy, con valor, el lado se conserva si se ve igual.
- **Otras rutas que siembran** y dejan el indicador obsoleto:
  - `SelectSingleCell`, `MutateStructure` y el selector de frente;
  - `ApplyScope` y `ApplyFrontData`, que además propagan el valor redondeado al alcance;
  - `ApplyTopology_Click` (WC:237).

*Por qué ninguna prueba lo ve:*
- Con tres decimales o menos, el valor sembrado es el exacto y no cambia nada.
- Por eso ninguna prueba existente lo detectaría, y la CI del GREEN saldría verde.

*Qué contradice:*
- es una pérdida de la clase H-31: una caja que el usuario no editó cambia el diseño (D-PBTC-01; la regla «ninguna caja manda» de [OPT1]:17);
- TC2:162, que dice que fuera de «Ambos lados» nada cambia;
- el registro propuesto: «con tres decimales o menos, nada cambia» se mantiene, pero la promesa de «pasar por la caja sin editar conserva el exacto» se rompe.

### 2.3 Punto 3 — Cada CT

Cada CT se rehizo por el código. Ninguno lanza en la base. Las mecánicas que usan ya pasan en la CI:
- `Update` sobre `pb-06`, `pb-07`, `pb-20-ab` y `pb-31` (CT-PB0-02, `…StateTests.cs:227-238`);
- M2 de `pb-31` de 48 a 56 (CT-PB0-03, `…ModuleTests.cs:89-91`).

**CT-PBTC2-01** (TC2:239-249):
- **Línea 1** se cumple. `Composite.Gap` gobierna el hueco:
  - el estado lo carga de `composite.Gap` (CS:991);
  - el resolver arma el módulo `GAP` desde `layout.Gap` (`PushBackCompositeStructure.cs:123-133` y `:509-521`; `PushBackCompositeResolver.cs:56-61`);
  - por eso el `GAP` de 54 guardado en `pb-20-ab` no lo pisa.
- **Línea 2:** «54».
- **Línea 3:** en la base da `Gap=54` (WC:198). Falla por aserción.
- **GREEN:** `SameShown("54", "54")` → 54,00044.
- **Ventana 2:** 60.
- **No es tautológica.**
- **Su `descartes=0`** de la ventana 2 se lee antes del `Cerrar` del `finally` (I-03).

**CT-PBTC2-02** (TC2:251-262):
- **M2 de 47,99213 sigue calculado al cargar:**
  - no hay reconstrucción: `MustRebuild` solo mira la tarima y el número de posiciones (`DynamicEditorDesignAssembler.cs:44-61`; `DynamicDepthGeometry.Matches`);
  - el resolver copia los módulos (`DynamicRackSystemResolver.cs:157-172`);
  - `Refresh` no toca longitudes (`DynamicRackSystemBuilder.cs:137-151`);
  - la reconciliación no toca los calculados (`RackModuleReconciliation.cs:164-170`).
- **Línea 1:** «47.992».
- **Línea 2:**
  - en la base, `pendiente=True`, porque 0,00013 ≥ 0,0001 y se escenifica `SetLength(47.992)`. Falla por aserción;
  - en el GREEN, `False`.
- **Línea 3** se cumple, porque `ModuleIntents` toma los módulos del baseline sin commit.
- **La ventana 2** pasa en las dos.
- **Un detalle:** `Num` hace `GetValue<double>()` (SC1) y lanza sobre un booleano o sobre `ModuleId`. El ayudante privado de TC2:222 es imprescindible, y TC2:231 dice «No hay ayudante nuevo» (M-02).

**CT-PBTC2-03** (TC2:264-279):
- **`pb-06`:** base 150/18 en la línea 3, que falla por aserción.
- **Línea 4:** 21,5 manda y la cabecera conserva su exacto.
- **Línea 5:** vaciar la caja da nulo. Es el patrón de `PushBackAdvancedRackParametersWindowTests.cs:110-128`. La caja queda en `EmptyRequired`, pero no bloquea «Actualizar» (W:3363-3419).
- **`pb-07`:** 24,00044 es válido, por tres razones:
  - solo tiene que no superar la altura resuelta del poste (`PushBackAdvancedRackParameters.cs:85-105`);
  - `DerivedPostHeight` es 140,00044;
  - `pb-07` tiene el poste derivado, con M2 y M3 como separadores consecutivos (segundo script).
- **La casilla de módulos** solo vuelve a sembrar las cajas (W:1571-1587).
- **Es correcto.**

**CT-PBTC2-04** (TC2:281-297):
- **`pb-20-ab`** tiene en `Fronts[0].Levels[0]` los mismos valores en A y en B: 42, 60, 1000, 6 y un override nulo. Tiene una tarima por frente y una holgura de 4. Así que solo se marcan las dos cajas editadas.
- **Línea 2:** `[]` y `[]`.
- **Línea 3:**
  - en el GREEN, 61,235 en los dos lados;
  - en la base, 61,23456 y 61,235. Falla por aserción.
- **Línea 4** se cumple. 100 y 120 son válidos: la regla es > 0 (`DynamicEditorCell.cs:25`), y el ancho de una calle con una tarima de 42 y una holgura de 4 ronda 50. Con esto, R2-02 queda resuelto por lectura para `pb-20-ab`.
- **La ventana no mixta** se cumple. «61.2350» se conserva, porque `NumericField` no reformatea el texto al perder el foco (`NumericField.cs:131-140`).
- **No es tautológica.**
- **Sus siete líneas no llevan `descartes`** (I-03).

**M-02:**
- Ninguna aserción es tautológica.
- La línea de la ventana 2 de 01 («Gap=60») y las líneas de «Actualizar» pueden fallar con un producto plausible distinto.

**M-03:**
- **Los casos negativos de las correcciones principales** están bien.
- **Falta el negativo de I-01:** sin lado B, la regla mixta no actúa.
- **Faltan los CT de las extensiones declaradas de OWNER-1:** cambio de celda, alcance y datos del frente (M-03).
- **La línea 3 de la ventana mixta no imprime `A.largo` ni `B.largo`.** Así no fija que una caja mixta vacía deja a cada lado el suyo. Lo cubre ya `PushBackBothModeCellBeamOverrideTests.cs:112-114` y `:142-144`, en el §6.4.

**M-05:**
- La costura es estática (`src/RackCad.UI/Shell/EditorClosePolicy.cs:49-73`), así que vale dentro o fuera de `Sta`.
- Ningún CT cierra con módulos pendientes: 02 confirma con «Actualizar» (W:3386-3389).
- El problema es de texto, no de riesgo: I-03.

**Determinismo:** GUID fijo, cultura invariante, «R» y ninguna hora. Correcto.

### 2.4 Punto 4 — Sustituciones (0)

**Coincido: ninguna.** Mi `git grep`:
- **Ningún archivo de prueba que nombre las seis cajas o `SideSelectorBox` contiene un literal con cuatro decimales o más.** Recuento 0 en todos.
- **Las ediciones en «Ambos»** (`PushBackBothModeCellBeamOverrideTests.cs:87-90`, `:195`, `:214`, `:272`) usan 100, 120, 130, nulo y 150. Con la regla nueva dan lo mismo.
- **Las ediciones de la longitud de módulo** son siempre `+ 8`, `+ 5` o `ValueOf + increment`: `PushBackModuleEditorWindowTests.cs:115-117`, `:146-148`, `:259-261`; `PushBackClassicCharacterizationCloseTests.cs:95-97`; `…IntentTests.cs:71-73`; `…ModuleTests.cs:34-37`. Ninguna prueba pasa por la caja sin editar para afirmar que el módulo se vuelve manual.
- **`GapBox`:** 54, enteros o `CompositeState.Gap` (por ejemplo `PushBackCompositeLifecycleWiringTests.cs:55-57`).
- **Las avanzadas:** el portador nulo, 137 o 100000 (`PushBackEditorSideOwnershipTests.cs:370-373`).
- **PB-TC y PB-T0** no usan «Ambos» con decimales (`git grep` de `SideSelectorBox` y `SideBPresentCheck`).
- **El camino de I-01** lo pisa `PushBackCompositeLifecycleWiringTests.cs:126`, que retira el lado B, pero con valores enteros. **Tampoco con la corrección de I-01 hace falta una sustitución.**

### 2.5 Punto 5 — El contrato

- **`AllowedWriteScope`, 4 rutas** (TC2:363-368):
  - están completas;
  - SC1 queda prohibido y solo se usa.
- **`ForbiddenWriteScope`, 129:**
  - los 51 archivos de UI existen y son distintos (primer script);
  - los 59 de `src/` son los 58 de [PKC-C] más WX, contados sobre su lista;
  - `tests/RackCad.UI.Tests/Workspace/` no tiene ninguna ruta permitida debajo;
  - `DeclaredCorrection2` no aparece en HEAD.
  - **Es correcto.**
- **`RequiredTests`, 18 filas:** los recuentos estáticos coinciden:
  - PB-TC 3, 2, 2, 2 y 1;
  - PB-T0, 18 con 2 `Skip`, es decir 16;
  - T1B 55 y Cantilever 67;
  - módulos 13 y avanzados 9;
  - compuesto 40 Fact más 12 `InlineData`, es decir 52;
  - 7, 3 y 10;
  - Core: 3 y 15. Los 10 de T1B no se pueden contar estáticamente por su `MemberData`; es el mismo valor que en PB-TC.
  - **La colisión de prefijos está bien resuelta:** `…DeclaredCorrectionRoundingTests` no es subcadena de `…DeclaredCorrection2RoundingTests`.
  - **Frente a [PKC-C]** salen las filas de WLC, de las vistas de cota y de `PushBackBlankFrontAuthorityTests`. Es aceptable, porque PB-TC2 no toca su zona y la CI 4/4 las corre.
- **Invariantes:**
  - INV-PBTC2-04 no se puede cumplir tal como está escrito (I-02);
  - INV-PBTC2-07 contradice a CT-04 (I-03);
  - las anclas de INV-PBTC2-03 son correctas.
- **`StopConditions`:**
  - las estándar coinciden una a una con [PKC-C] (S-02 a S-14 y P-01 a P-08).
  - **Las transcripciones de PBTC2-C11 son literales,** comprobadas byte a byte:
    - `f4-dispositions.md:1104-1106`, `:1540`, `:520`, `:562` y `:1657-1660`;
    - `owner-f4t3-authorization.md:257-263`.
  - Los dos scripts sustitutos existen en las rutas citadas.
  - La disposición «solo esta vez» de PB-TC no se extiende.
  - Es previsible que se repita la variación de nc1 de PB-TC (M-05).
- **PBTC2-C14:** es coherente con OB-06.4 y con la aceptación del Owner. Pero no impone lo que §8 dice que impone (M-04).
- **`EligibleCells`:**
  - `model-catalog.md:14` (`STALE` = verificación + 90 días);
  - `:51` y `:97` (2026-09-30) → vencen el 2026-12-29.
  - Están vigentes.
- **Línea base de la CI:** `gh run view 44232324035` da `push` sobre `5597c05b…`, `success` en los 4 trabajos, Core 19 070/19 070 y UI 2 397 Passed, 21 Skipped y 2 418 en total. La previsión es aritméticamente correcta. Si la corrección de I-01 añade una ventana a CT-04 y no un método, no cambia.

### 2.6 Punto 6 — Guardas existentes

**Ninguna salta, ni con el RED ni con el GREEN:**
- **Fact3:** W conserva `RequestStandardRestore()` (W:2539) y no gana `forceRebuild:` ni `forceRebuild)` (`PushBackModuleEditorCharacterizationTests.cs:381-401`).
- **G-10:** una sola `.RequestUpdate(` (W:3409).
- **I-55:** W:3135 y W:3224, sin tocar.
- **El desviador:** W:3044 y W:3048, sin tocar.
- **T-SG CT-19:** ningún token nuevo en `Systems/PushBack` (`…SourceGuardTests.cs:235-247`).
- **WX** no cambia, así que T-KB, la adopción del shell y Fact1 quedan fuera.
- **El corpus de WLC:** no hay fixtures nuevos.
- **El censo de ventanas:** no hay ventanas nuevas.

Ninguna prueba nombra `OptionalVal`, `ReadCellValues`, `ReadAdvancedRackParameters`, `IsMixed`, `mixedNow`, `SameShown`, `KeepOptional` ni `KeepCell` (`git grep`). La única coincidencia es la palabra `OptionalValues`, en una guarda ajena (`WorkspaceHostHardeningGuardTests.cs:388`).

### 2.7 Punto 7 — El efecto declarado y la medición en la cadena

**Lo que es suficiente en principio:**
- **La fórmula** es la de CT-PB0-21 (`PushBackFullEditorCharacterizationRoundingEffectTests.cs:107-245`).
- **El BOM redondea a cuatro decimales** (`PushBackBomBuilder.cs:447`), así que lo esperable son diferencias numéricas menores que 0,001.
- **Un salto de paso en la retícula de troqueles** (`PushBackTroquelGrid.cs:68`) lo verían K1 o K4, y pararía la cadena.
- **T2-1 mide un delta real,** porque el módulo `GAP` sale de `Composite.Gap` (§2.3).

**Por qué la tolerancia del BOM es legítima.** OB-06.4 dice que cualquier diferencia en la lista de materiales es material ([P]:389). PBTC2-C14 admite diferencias numéricas menores que 0,001. Esa holgura tiene autoridad: el Owner eligió la opción que decía «el BOM en diezmilésimas» ([OPT1]:20, :23).

**Efectos que se descartan:**
- **OWNER-1** queda fuera con razón: menos de 0,0005 entre dos valores que el Owner pidió igualar.
- **La acumulación de residuos al pasar por varios módulos** se refuta, porque el GREEN escribe lo guardado y es la base la que deriva. La corrección no altera la geometría de un documento respecto a lo que tiene.

**Lo que falta precisar** (M-04):
- las filas de la «base» se construyen a mano a partir de la predicción, y no de la salida real de la ventana de la base tras el gesto;
- no se dice de dónde sale el catálogo;
- no se dice cómo se emparejan las líneas del BOM si cambia su número;
- el §8 (TC2:632) dice que PBTC2-C14 para la cadena si no se cumple «menos de 0,0005"», pero K4 usa 0,001.

### 2.8 Punto 8 — Las anclas

**Muestra de 40 anclas, casi todas exactas:**
- **WC:**
  - 184-200, 186, 193, 198, 746;
  - 432, 579, 588, 602-640, 634, 583-584;
  - 50.
- **W:**
  - 749-755, 758-762, 774-788, 802-831, 818-822, 825;
  - 1240, 1346, 1510;
  - 1595-1619, 1622-1629, 1632-1647, 1639-1646;
  - 1656-1666, 1833, 1892-1913, 1905-1911;
  - 3386-3389, 3457-3459;
  - 3634, 3641, 3665-3668, 3671, 3674-3678, 3683, 3685-3688;
  - 696-703.
- **WX:** 148, 492, 499, 505-507, 510, 524, 531, 554.
- **Application:**
  - `RackModuleEditSession.cs:162-167` y `:185-203`;
  - `DynamicRackSystemResolver.cs:157-172`;
  - CS:248 y CS:584.
- **Pruebas:**
  - `EditorWindowTestSupport.cs:89-95`;
  - `PushBackBothModeCellBeamOverrideTests.cs:60-65`;
  - Fact3 `:381-401`.
- **Registros:**
  - [SC] `:8`, `:169-173`, `:175-182` y `:209-211`;
  - el anexo A, `package-v2.md:1217`, `:1221`, `:1285` y `:1294`;
  - [OD] `:150-155` y `:233-236`;
  - `model-catalog.md:14`, `:51` y `:97`.

**Desviaciones,** todas menores (M-01):

| Ancla | Dice | Es |
|---|---|---|
| `SetMixed` | WC:571-590 (TC2:90) | WC:565-590 |
| `ApplyMixedSideState` | WC:445-504 | WC:445-503 |
| `SideSelector_Changed` | WC:147-175, con la llamada en WC:173 | WC:147-180, con la llamada en WC:172 |
| `LoadSelectedModule` | W:1803-1863 | W:1803-1866 |
| `PushBackAdvancedRackParameters.cs`, el refuerzo de `ApplyTo` | `:116-118` | `:120-122` |
| Quién llama a `LoadSelectedFront` (H2-06 y C-2) | — | Falta `ApplyTopology_Click` (WC:237) |
| El límite que hace posible I-01 | — | No se anota la salida de WC:604-607 |

### 2.9 Punto 9 — Decisiones materiales nuevas

**Ninguna para emitir PB-TC2.**
- Las cuatro candidatas de TC2:635-639 están cubiertas, como se dice en §2.1.
- **La corrección de I-01** no decide nada: un rack de un sentido sigue la regla `Keep` que el Owner ya eligió.
- **M-07** es un defecto **anterior** que PB-TC2 no toca ni debe tocar:
  - qué es: sin lado B, vaciar «Largo manual» con el indicador obsoleto no surte efecto;
  - a dónde va: al paquete de PB-T1, antes de su contrato;
  - por qué allí: PB-T1 tendrá que decidir si lo reproduce o lo corrige, y eso puede pedir una vuelta por OB-06.5.
  - **No para la emisión de PB-TC2.**

---

## 3. Tabla de hallazgos

| Id | Gravedad | Contenido | Evidencia | Corrección |
|---|---|---|---|---|
| **I-01** | **IMPORTANTE** | **Un indicador mixto obsoleto en un rack de un sentido hace que `KeepCell` y el `OptionalVal` nuevo escriban el valor sembrado y redondeado de una caja sin editar.** El camino es: «Ambos» con A ≠ B → desmarcar el lado B → clic en cualquier celda. Contradice TC2:162 y la regla `Keep` (D-PBTC-01). Con tres decimales o menos no se ve, así que ni la CI ni ninguna prueba existente lo detectarían | WC:604-607 (la salida sin lado B); WC:588 (la única limpieza); WC:126-145 y WC:157-160; CS:294-302; W:467-476 y W:489; W:1080-1100; CS:182-190 | **1. La guarda.** La regla mixta se condiciona a que «Ambos» esté activo con B presente, la misma condición de WC:452. Por ejemplo, un miembro nuevo `private bool MixedBoth(NumericField field) => IsMixed(field) && composite.ActiveSelection == PushBackSideSelection.Both && composite.SideBPresent;`.<br>**2. `KeepCell`:** `=> MixedBoth(field) && field.Value.HasValue ? field.Value.Value : Keep(field, current);`.<br>**3. `OptionalVal`:** `=> field.Value.HasValue ? (MixedBoth(field) ? field.Value : KeepOptional(field, sideValue)) : (IsMixed(field) ? sideValue : null);`. La rama vacía queda **literalmente** como hoy (W:3668), para no cambiar el defecto anterior M-07.<br>**4. El texto.** Reescribir TC2:162. Añadir `MixedBoth` a D-PBTC2-05 y a INV-PBTC2-03. Ajustar INV-PBTC2-04 (TC2:449).<br>**5. Un caso negativo (M-03), como tercera ventana de CT-04.** `pb-20-ab` con A.alto = 61,23456 y B.alto = 60, «Ambos», `Check(w, "SideBPresentCheck").IsChecked = false` y `w.SelectMatrixCell(0, 0, false)`. La línea es `sin lado B CellPalletHeightBox texto=[61.235] A.alto=61.23456`. Pasa en la base, falla con el diseño v1 y pasa con el corregido |
| **I-02** | **IMPORTANTE** | **INV-PBTC2-04 no se puede cumplir tal como está escrito:** «en W no aparecen `Val(`» (TC2:445). W contiene y seguirá conteniendo `Val(` como subcadena de `OptionalVal(` (la declaración y W:825) y de `IntVal(` (W:814-817 y otras). Una comprobación literal del Worker o del Controller daría fallo sobre un GREEN correcto. Con la regla «sin latitud», eso es parar o reinterpretar | TC2:445; W:3665, W:3688, W:814 | Escribirlo como expresión regular de palabra: «ninguna coincidencia de `\bVal\(` en W, ni del texto `cref="Val"`», con la orden exacta (`git grep -nP '\bVal\(' -- <W>` vacío) |
| **I-03** | **IMPORTANTE** | **INV-PBTC2-07 y D-PBTC2-08 dicen que «cada ventana termina en `descartes=0`»** (TC2:219, TC2:462). **Las siete líneas de CT-04** (TC2:288-295) no lo llevan. El Worker no puede cumplir a la vez las líneas literales («no ajusta lo esperado», PBTC2-C07) y el invariante. Además, en las ventanas que no se cierran con «Actualizar» (la ventana 2 de 01 y las dos de 04), el contador se lee **antes** del `Cerrar` del `finally`. Así no observa el cierre que M-05 quiere vigilar | TC2:219, :247, :288-295, :462; S0 `Cerrar` | **1. CT-04:** añadir ` descartes=0` a la última línea de cada ventana.<br>**2. Las ventanas que no cierra «Actualizar»:** fijar que el contador se lee después de cerrar. Por ejemplo, con una línea final `<ventana> cerrada descartes=0` tras un `Cerrar(w)` explícito, idempotente con el del `finally`. O decir expresamente que la línea es previa al cierre y cambiar INV-PBTC2-07 en consecuencia |
| M-01 | MENOR | **Anclas e inventario.** Hay cinco anclas desplazadas. H2-06 y C-2 omiten `ApplyTopology_Click`. H2-06 no anota que `LoadCompositeCellPanel` sale sin lado B, que es el límite de I-01 | §2.8 | Corregirlas |
| M-02 | MENOR | **«Ningún ayudante nuevo» (TC2:231) contradice «un ayudante privado … lee el JSON» (TC2:222).** El privado es imprescindible: `Num` lanza sobre booleanos y cadenas (`GetValue<double>()`) | SC1 `Num`; TC2:222, :231 | Admitirlo en TC2:231 y fijar su forma: la misma navegación de SC1 y `GetValue<bool>()` o `GetValue<string>()`, sin latitud |
| M-03 | MENOR | **M-03 de PB-TC:** las extensiones declaradas de OWNER-1 (cambio de celda, alcance y datos del frente) no tienen CT. La tabla de TC2:310 las da por cubiertas con negativos que son de otras correcciones. INV-PBTC2-03 y 04 sujetan el sitio y bajan el riesgo. La línea 3 de la ventana mixta tampoco fija la caja mixta vacía | TC2:162, :310; W:1346, :1510 | Añadir una línea a la ventana mixta: tras escribir, «Aplicar» a otra celda, y los dos lados leídos. Y `A.largo`/`B.largo` en la línea 3. O declararlo en `KnownLimitations` y llevarlo a la red de PB-T1, como M-03 de PB-TC |
| M-04 | MENOR | **D-PBTC2-07 y PBTC2-C14, sin precisar:**<br>• la «base» se construye a mano;<br>• no se dice de dónde sale el catálogo;<br>• no se dice cómo se emparejan las líneas del BOM si cambia su número;<br>• TC2:632 dice que C14 para si no se cumple «menos de 0,0005"», pero K4 usa 0,001 | TC2:189-213, :582-588, :632 | **1. La base:** la salida real de la ventana de la base tras el gesto, `LastComputation.Design`, comparada con la fila a mano.<br>**2. El catálogo:** `w.Session.Catalog`.<br>**3. El BOM:** un número de líneas distinto es STOP.<br>**4. La transcripción:** anotar `MaxDelta` y K4'.<br>**5. TC2:632:** corregir el texto, o informar a la sesión si MaxDelta ≥ 0,0005 |
| M-05 | MENOR | **Riesgo no declarado.** El nc1 de PB-TC salió con una variación no exacta. Con la misma celda es previsible que se repita y que la cadena vuelva al Owner por PBTC2-C11. Es lo correcto, pero conviene anticiparlo | `task-close-package.md:57-61`; TC2:566-572 | Añadir la fila R2-08 a §7 |
| M-06 | MENOR | **Una ruta local de trabajo en un texto que se custodia.** [OPT1]:7 cita `hs65/pbt1/ob065-dossier.md` | [OPT1]:7; precondición 3 (TC2:28-33) | Al custodiarlo, sustituirla por `F4B-DRAFTS/pbt1/ob065-dossier.md`, con la nota de redacción |
| M-07 | MENOR (arrastre) | **Defecto anterior, fuera de PB-TC2.** Con el indicador obsoleto:<br>• vaciar «Largo manual» en un rack de un sentido no surte efecto (W:3668: vacía y mixta da el lado);<br>• las cajas obligatorias se quedan opcionales (WC:580).<br>PB-TC2 no debe cambiarlo, y la corrección de I-01 lo conserva | WC:580, :588, :604-607; W:3668 | Registrarlo para el paquete de PB-T1, antes de su contrato: reproducirlo o corregirlo, con posible vuelta por OB-06.5 |

---

## 4. Veredicto

**EMITIBLE TRAS CORRECCIONES. B/I/M = 0/3/7.**
- **El alcance** es fiel a F4B-E09 y a [OD].
- **Las seis cajas de H-31** están bien diseñadas.
- **Los cuatro CT** fallan por aserción en la base y se cumplen en el GREEN por lectura.
- **Las sustituciones son 0,** y ninguna guarda salta.
- **Las tres importantes deben corregirse antes de emitir:**
  - I-01 es un defecto de diseño de OWNER-1 que cambiaría más de lo declarado;
  - I-02 e I-03 son contradicciones mecánicas del contrato.
- **Las menores** se pueden corregir en la misma v1.1.

**Decisiones materiales nuevas para el Owner antes de emitir PB-TC2: ninguna.** M-07 va al paquete de PB-T1.
