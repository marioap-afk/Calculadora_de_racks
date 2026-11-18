> **Redaccion para el repo publico:** las rutas locales se sustituyen por marcadores (`<worktree-de-I-64>`, `<datos-locales-del-usuario>`, `<datos-de-aplicacion-del-usuario>`). El resto no cambia.

# I-64 F4B — Paquete del contrato de PB-TC2 (las correcciones declaradas que quedaron fuera de PB-TC), v1

**Estado:** v1, para la revisión adversarial independiente. No declara GATE PASS.

**Autorización de emisión:** condicional, del Owner (F4B-E09, pregunta 4). PB-TC2 se emite sin volver al Owner si se cumplen tres condiciones:
- su revisión independiente acaba con 0 bloqueantes y 0 importantes sin resolver;
- no aparece ninguna decisión material nueva (§8);
- se cumplen las demás reglas de F4B: [DP], D-PB-10 y las paradas que vuelven al Owner.

**Lo que corrige**, por decisión del Owner (F4B-E09, pregunta 1: «Corregir en PB-TC2 (Recomendado)»):
- **(1) H-31:** `GapBox`, `ModuleLengthBox` y las cuatro cajas avanzadas anulables, con la regla `Keep` de PB-TC;
- **(2) OWNER-1:** en «Ambos lados», una caja mixta que recibe un valor lo aplica a los dos lados.

Es un cambio declarado de RACKEDITAR por D-PB-06 («rounding behavior/state»), con presupuesto propio de 1/3 (D-PB-10). c-02 y `pb-21-cola` no entran: van a PB-T1 (F4B-E09, preguntas 2 y 3).

**Base:** rama `architecture/workspace-persistente-rackcad` en `5597c05b`, con `main` en `812baf87`.
- `git diff 4440f67d 5597c05b -- src tests` está vacío: el producto es el GREEN de PB-TC.
- PB-TC está CLOSED / PASS WITH LIMITATIONS (F4B-E08).
- Todo el §2 está medido ahí, en solo lectura y sin compilar. El detalle está en `pbtc2-measurements.md`, que se custodia con este paquete.

**Patrón:** el paquete de PB-TC, `docs/initiatives/I-64-f4b-pbtc-gate-contract-package.md` v2.2 (**[PKC]**); su contrato emitido, `docs/automation/evidence/I-64-pilot/F4B-PBTC-DECLARED-CORRECTIONS/coordinator/gate-contract.json` (**[PKC-C]**); y su generador. El diseño de H-31 es el anexo A de la v2 de PB-TC (`docs/automation/evidence/I-64-pilot/F4B-DRAFTS/pbtc/package-v2.md:1217-1302`), revisado en ICT2 (I-03 RESUELTO, N-07 y N-08), con las líneas de W y WC medidas de nuevo.

**Precondiciones de emisión:**
1. **Las decisiones del Owner,** ya tomadas: `F4B-DRAFTS/coordinator/owner-pbt1-pre-decisions.md` (**[OPT1]**, F4B-E09), custodiado en el commit de emisión y anotado en `f4b-dispositions.md`.
2. **La revisión independiente de esta v1** (o de su v1.x), cerrada con 0 bloqueantes, 0 importantes sin resolver y ninguna decisión material nueva (F4B-E09, pregunta 4).
3. **Custodia en el commit de emisión,** que es la `AuthorityRevision`:
   - este paquete, en `docs/initiatives/I-64-f4b-pbtc2-gate-contract-package.md`, y el registro de mediciones, en `docs/initiatives/I-64-f4b-pbtc2-measurements.md`;
   - [OPT1] y el expediente del analista, en `F4B-DRAFTS/pbt1/ob065-dossier.md`;
   - la sección nueva de PB-TC2 en el registro declarado [SC] (`docs/automation/evidence/I-64-pilot/F4B-DRAFTS/superseding-characterization.md`), en estado DECLARADA (§5.3), y la nota «decidido en F4B-E09» bajo el «Pendiente» de H-31 y bajo la limitación OWNER-1;
   - el contrato y su generador `make_pbtc2_contract.py`, en `docs/automation/evidence/I-64-pilot/F4B-PBTC2-DECLARED-CORRECTIONS/coordinator/`, con la ruta local redactada como en PB-T0;
   - en `f4b-dispositions.md`, la obligación de PB-T2 de §5.3.
4. `git diff 5597c05b <BaseSha> -- src tests` vacío. Si no lo está, se vuelve a medir el §2 (PBTC2-C07).
5. Las celdas del §6.8 no están `STALE` (vencen el 2026-12-29); el `config.toml` coincide con la Snapshot I; el binario de Codex es el acreditado.

## 1. Autoridades

| Autoridad | Qué fija |
|---|---|
| [OPT1] (`F4B-DRAFTS/coordinator/owner-pbt1-pre-decisions.md`, F4B-E09) | **Pregunta 1:** PB-TC2 antes de PB-T1, con H-31 (la regla `Keep` en las seis cajas) y OWNER-1 (la caja mixta aplica el valor escrito a los dos lados). El Owner fue informado de que con tres decimales o menos no cambia nada; con más, menos de 0,0005" y el BOM en diezmilésimas; y de que pasar por la longitud de un módulo con más decimales lo vuelve manual. **Preguntas 2 y 3:** c-02 y `pb-21-cola` van a PB-T1. **Pregunta 4:** la emisión condicional |
| [OD] (`F4B-DRAFTS/coordinator/owner-f4b-disposition.md`) | D-PB-06: «rounding behavior/state», sus seis pasos (`:130-150`), «Do not hide these corrections inside PB-T1 state extraction» y la vuelta al Owner si una corrección altera la geometría «materially» (`:153-155`). D-PB-10 (`:233-236`) |
| [P] (`docs/initiatives/I-64-f4b-push-back-completo-package.md`, v3.1) | OB-06.2 a OB-06.6 (`:369-398`); OB-T1B.1 a OB-T1B.3 (`:700-710`); la fila de PB-T2 (`:504`); §9 (`:740-750`) |
| [DP] (`F4B-DRAFTS/coordinator/owner-f4b-procedure-decisions.md`, F4B-E03) | Una sola replanificación, y solo por la comparación literal (pregunta 2). Los sustitutos de nc1 y nc2, solo con la firma EXACTA de PD-F4-04 y PD-F4-01; cualquier otra firma vuelve al Owner (pregunta 1) |
| El expediente (`F4B-DRAFTS/pbt1/ob065-dossier.md`) | §2.1 (H-31), §2.2 (OWNER-1) y §5 (la PB-TC2 propuesta), medidos en `5597c05b`. Aquí se vuelven a medir todos (§2) |
| [PKC] y [PKC-C] | El patrón del paquete, de la regla `Keep` (D-PBTC-01) y del contrato; D-PBTC-10 y PBTC-C14, como patrón de la medición en la cadena; las firmas literales de PBTC-C11 |
| El anexo A de la v2 de PB-TC (`F4B-DRAFTS/pbtc/package-v2.md:1217-1302`) e ICT2 (`F4B-DRAFTS/pbtc/independent-review-v2-check.md`, I-03, N-07 y N-08) | El diseño de H-31 ya revisado: `Keep` en el hueco y en la longitud del módulo, y `KeepOptional` en las cuatro anulables. N-08: la ventana editada usa M2 de `pb-31`, de 48 a 56, probado en CT-PB0-03 |
| El cierre de PB-TC (`F4B-PBTC-DECLARED-CORRECTIONS/task-close-package.md`, §4) | Las lecciones M-02 a M-05; `Assert.Fail` en las corridas temporales; el nombre `SUS2-0n` de las sustituciones |
| `F4B-DRAFTS/coordinator/owner-pbtc-mixed-decision.md` (F4B-E08) | OWNER-1 queda como limitación conocida, y su arreglo vuelve al Owner antes de PB-T1. Lo resuelve [OPT1] |
| [SC] (`F4B-DRAFTS/superseding-characterization.md`) | EJECUTADA para PB-TC; su «Pendiente» de H-31 (`:175-182`) y la limitación OWNER-1 (`:209-211`), que PB-TC2 cierra |
| `F4B-DRAFTS/f4b-dispositions.md` | F4B-E01 a F4B-E09 |
| ADR-0029 D13 | «la caracterización no se edita para que pase» |
| `docs/AUTOMATION_PLAN.md` §16.5, §16.8, §16.9 y §16.11; `docs/automation/agent-execution/README.md` §4 y §10; `routing.md`; `model-catalog.md` | A1-A8, RED de la cadena, las 14 comprobaciones, las paradas, nc1 a nc4 y las celdas |
| Las fuentes de las firmas (`F4-DRAFTS/f4-dispositions.md:520`, `:562`, `:1103-1106`, `:1540`, `:1657-1660`; `F4-DRAFTS/coordinator/owner-f4t3-authorization.md:255-265`) | PD-F4-01 y PD-F4-04, transcritas en PBTC2-C11 |

## 2. Hechos medidos en `5597c05b`

**Alias:**
- **W** = `src/RackCad.UI/Systems/PushBack/RackPushBackSystemWindow.xaml.cs` (3690 líneas);
- **WC** = `….Composite.cs` (936);
- **WX** = `….xaml` (793);
- **S0** = `tests/RackCad.UI.Tests/PushBackFullEditorCharacterizationSupport.cs`;
- **SC1** = `tests/RackCad.UI.Tests/PushBackDeclaredCorrectionSupport.cs`.

Todo `ruta:línea` es de HEAD.

### 2.1 H-31: las seis cajas

- **H2-01. El hueco.**
  - **Siembra:** `GapBox.SetNumber(composite.Gap)`, con «0.###», en cada pasada válida (`RefreshCompositePanel`, WC:746, bajo `suppressSync`).
  - **Lectura:** `Gap_Changed` (WC:184-200), en `LostFocus` (WX:148). Sale si `suppressSync` (WC:186) o si la caja tiene error o está vacía (WC:193), y después hace `composite.SetGap(GapBox.Value.Value)` **sin comparar** (WC:198). Basta con pasar por la caja para que el hueco quede redondeado.
  - **Fuera de la validación global:** no está en `AllNumericFields` (W:749-755).
- **H2-02. La longitud del módulo.**
  - **Siembra:** `ModuleLengthBox.SetNumber(module.Length)` (W:1833), dentro de `LoadSelectedModule` (W:1803-1863), bajo `suppressSync`.
  - **Lectura:** `ModuleLength_Changed` (W:1892-1913), en `LostFocus` (WX:554). Calcula `var length = Val(ModuleLengthBox, module.Length);` (W:1905) y sale si `length <= 0.0 || Math.Abs(length - module.Length) < 0.0001` (W:1906). Si no sale, escenifica `moduleOwner.ModuleSession.SetLength(module.ModuleId, length);` (W:1911).
  - **`SetLength` vuelve manual el módulo:** asigna la longitud, `IsManualOverride = true` e `IsCalculated = false` (`src/RackCad.Application/Systems/Shared/RackModuleEditSession.cs:185-203`), y deja un cambio pendiente (`HasPendingChanges`, `:162-167`). «Actualizar» lo confirma (W:3386-3389).
  - **El resolver conserva las longitudes guardadas** de los módulos calculados (`src/RackCad.Application/Systems/Dynamic/DynamicRackSystemResolver.cs:157-172`). Por eso un módulo calculado de 47,99213 (el caso del fondo de 1219 mm del expediente, §2.1) llega a la caja como «47.992». Entonces |47,992 − 47,99213| = 0,00013 ≥ 0,0001, y pasar por la caja lo deja **manual** de 47,992.
- **H2-03. Las cuatro avanzadas anulables.**
  - **Siembra:** `LoadAdvancedRackParameters` (W:1595-1619), bajo `suppressSync`. Las numéricas pasan por `SetOptional`, que es `SetNumber(valor)` con «0.###», y el nulo vacía la caja (W:1622-1629).
  - **Lectura:** `ReadAdvancedRackParameters` (W:1632-1647) relee **las seis** en cualquier `LostFocus` de las cinco cajas o en el `Click` de la casilla (WX:492, :499, :505-507, :510, :524 y :531, por `AdvancedRackParameter_Changed`, W:1656-1666).
  - **Las anulables con decimales** (W:1639, :1641, :1642 y :1646): `advanced.ManualHeaderHeightOverride = RackHeaderHeightBox.Value;`, `advanced.DerivedPostReinforcementHeight = DerivedPostReinforcementHeightBox.Value;`, `advanced.DerivedPostHeight = DerivedPostHeightBox.Value;` y `advanced.SeparatorSpacingOverride = RackSeparatorSpacingBox.Value;`.
  - `SeparatorCountOverride` es entero (W:1643-1645) y `DerivedPostReinforced`, una casilla (W:1640): no tienen decimales.
  - **Una caja vacía es «el cálculo»:** su `Value` es nulo. Las seis cajas no son opcionales y no están en `AllNumericFields`, así que una vacía marca `EmptyRequired`, pero la pasada sigue. Es el comportamiento de hoy, y PB-TC2 no lo cambia.
- **H2-04. `Val`.** Su único uso es W:1905; está declarado en W:3634. Dos comentarios lo citan con `cref` (W:3641 y W:3683). El proyecto no genera documentación XML (medición B-8).
- **H2-05. Los comparadores de PB-TC:** `OptionalVal` (W:3665-3668), `ShownFormat` (W:3671), `SameShown` (W:3674-3678), `Keep` (W:3685-3686) e `IntVal` (W:3688). `Keep` está en W y es accesible desde WC, que es la misma clase parcial.

### 2.2 OWNER-1: «Ambos lados» y el indicador mixto

- **H2-06. Quién pone y quita el indicador.**
  - `mixedNow` solo cambia en `SetMixed` (WC:571-590). `SetMixed` solo lo llama `ApplyMixedSideState` (WC:445-504), y esta solo `LoadCompositeCellPanel` (WC:634), que solo se llama desde `LoadSelectedFront` (W:489).
  - `LoadSelectedFront` la llaman: la carga, la selección de celda o de frente, `MutateStructure`, el alcance, los datos del frente y el selector de lado (WC:147-175).
  - **No la llama el camino de edición:** `Input_Changed` (W:1240) es `RequestRecompute()`.
- **H2-07. El indicador sigue en pie cuando corre `ReadCellValues`.** El camino es: salir de la caja → `Input_Changed` → `Recompute` → `CommitCurrentCell` (W:758-762) → `ForEachEditedSide` (W:774-788) → `ReadCellValues(lado)` (W:802-831), una vez por lado.
  - Nada de ese camino toca `mixedNow`, y escribir en la caja no tiene ningún manejador que lo haga.
  - Una caja que estaba mixta y recibe un valor es, en esa lectura, `IsMixed(caja) && caja.Value.HasValue`, en los dos lados.
- **H2-08. La lectura de hoy.**
  - Las cinco cajas con decimales de la celda: `Keep(caja, valor del lado)` (W:818-822). Con A = 61,23456 y B = 60, y la caja mixta, escribir «61.235» deja A en 61,23456 y B en 61,235.
  - El largo manual: `OptionalVal(CellBeamLengthOverrideBox, cell.BeamLengthOverride)` (W:825). Con A = 100,00044 y B = 120, escribir «100» deja A en 100,00044 y B en 100.
  - Las cajas enteras ya aplican el valor escrito (`IntVal`).
  - **La ayuda de la caja promete otra cosa:** «Escribe uno para aplicarlo a A y a B» (WC:583-584).

### 2.3 Las guardas y las pruebas existentes

- **H2-09. Las guardas de fuente** (medición E-6):
  - ninguna cita `Val(`, `SetGap(GapBox`, `ModuleLength_Changed`, `Gap_Changed`, `AdvancedRackParameter_Changed` ni `Keep(`;
  - Fact3 (`tests/RackCad.Tests/PushBackModuleEditorCharacterizationTests.cs:381-401`) prohíbe `forceRebuild:` y `forceRebuild)` en W;
  - T-SG prohíbe sus nueve tokens en `Systems/PushBack`;
  - G-10 exige una sola `.RequestUpdate(` en W;
  - I-55 y el desviador miran W:3135, :3224, :3044 y :3048, que PB-TC2 no toca.

  PB-TC2 no toca WX, así que las guardas de WX (T-KB, la adopción del shell y Fact1) no lo ven.
- **H2-10. Las pruebas que tocan las seis cajas o «Ambos lados»** (mediciones E-2 a E-5) hacen solo cinco cosas:
  - salen de la caja sin editar;
  - escriben el valor que ya tiene la caja, que es 0 o un hueco entero;
  - escriben valores que se ven distintos (+8, +5, enteros, 21,5 o 100000);
  - escriben con el portador nulo;
  - en una caja mixta, escriben un entero que no se ve como ninguno de los lados.

  Con valores de tres decimales o menos, que una caja se vea igual a su valor implica que es igual. Por eso las reglas nuevas dan el mismo número en todas ellas. **Sustituciones: 0** (E-7).

## 3. Decisiones del paquete

### D-PBTC2-01. El hueco: `Keep(GapBox, composite.Gap)`

- En WC:198, `composite.SetGap(GapBox.Value.Value);` pasa a `composite.SetGap(Keep(GapBox, composite.Gap));`.
- La guarda de WC:193 sigue asegurando un valor. La siembra (WC:746) no cambia.
- **El resultado:** pasar por la caja sin editar conserva el hueco exacto; escribir un número que se ve distinto manda.

### D-PBTC2-02. La longitud del módulo: `Keep(ModuleLengthBox, module.Length)`

- En W:1905, `var length = Val(ModuleLengthBox, module.Length);` pasa a `var length = Keep(ModuleLengthBox, module.Length);`.
- La comparación de W:1906 y la llamada de W:1911 no cambian.
- **El resultado:**
  - pasar por la caja sin editar no escenifica nada, así que **un módulo calculado sigue calculado** (H2-02);
  - escribir un número que se ve distinto escenifica la longitud, y el módulo pasa a manual, como hoy;
  - **efecto declarado:** escribir un número que se ve igual al actual (por ejemplo «47.992» sobre 47,99213) tampoco escenifica nada. Es la regla R-03 (i) que el Owner eligió en PB-TC (F4B-E06).

### D-PBTC2-03. Las cuatro anulables: `KeepOptional`

- **Un comparador privado nuevo, en W:** `private static double? KeepOptional(NumericField field, double? current) => !field.Value.HasValue ? null : (current.HasValue && SameShown(field.Value.Value, current.Value) ? current : field.Value);`
  - vacía = nulo, es decir, el cálculo, como hoy;
  - si se ve igual al portador, el portador;
  - si no, la caja.
- **En `ReadAdvancedRackParameters`:**
  - W:1639 pasa a `advanced.ManualHeaderHeightOverride = KeepOptional(RackHeaderHeightBox, advanced.ManualHeaderHeightOverride);`;
  - W:1641 pasa a `advanced.DerivedPostReinforcementHeight = KeepOptional(DerivedPostReinforcementHeightBox, advanced.DerivedPostReinforcementHeight);`;
  - W:1642 pasa a `advanced.DerivedPostHeight = KeepOptional(DerivedPostHeightBox, advanced.DerivedPostHeight);`;
  - W:1646 pasa a `advanced.SeparatorSpacingOverride = KeepOptional(RackSeparatorSpacingBox, advanced.SeparatorSpacingOverride);`.
- **No cambian:** W:1640 (la casilla), W:1643-1645 (el entero), las siembras y los manejadores.

### D-PBTC2-04. La caja mixta de «Ambos lados» (OWNER-1)

- **Un comparador privado nuevo, en W:** `private double KeepCell(NumericField field, double current) => IsMixed(field) && field.Value.HasValue ? field.Value.Value : Keep(field, current);`
  - una caja que estaba mixta y recibió un valor lo aplica tal cual, a cada lado que se escribe;
  - en cualquier otro caso es `Keep`. Una caja mixta vacía también lo es, y deja a cada lado su valor (H2-07).
- **En `ReadCellValues`** (W:818-822), las cinco llamadas `Keep(<caja>, …)` pasan a `KeepCell(<caja>, …)`, con el mismo segundo argumento.
- **`OptionalVal`** (W:3665-3668) pasa a: `private double? OptionalVal(NumericField field, double? sideValue) => IsMixed(field) ? (field.Value ?? sideValue) : KeepOptional(field, sideValue);`
  - mixta con valor: el valor, a cada lado;
  - mixta vacía: el valor de cada lado, como hoy;
  - no mixta: `KeepOptional`, que es exactamente lo que hace hoy esa rama (vacía = nulo; se ve igual al lado = el lado; si no, la caja).
- **No cambian:** las cajas enteras (ya aplican el valor) ni los desplegables.
- **El alcance de la regla:** todo lo que lee la celda con `ReadCellValues`. Es decir, la edición, el cambio de celda, el alcance (W:1346) y los datos del frente (W:1510) con «Ambos lados». Fuera de «Ambos lados», `IsMixed` es falso y nada cambia.

### D-PBTC2-05. Dónde se declaran los comparadores, y qué pasa con `Val`

- **`KeepOptional` y `KeepCell`** se declaran inmediatamente después de `Keep` (W:3686) y antes de `IntVal` (W:3688). Así son miembros nuevos entre miembros (INV-PBTC2-03).
- **`Val` se retira:** con D-PBTC2-02 se queda sin usos. Se quitan W:3634 y la línea en blanco que la sigue (W:3635).
- **Sus dos `cref` se corrigen**, para no dejar un comentario roto:
  - en W:3641, `(<see cref="Val"/>)` pasa a `(hoy <see cref="Keep"/>)`;
  - en W:3683, `, como el respaldo de <see cref="Val"/>.` pasa a `.`.
- Nada más cambia en esos comentarios.

### D-PBTC2-06. Lo que no cambia

- `NumericField`, Cantilever, Application, el Plugin y WX;
- las siembras de las seis cajas;
- las validaciones;
- los manejadores del XAML;
- la casilla y el entero avanzados;
- D-PB-11 (W:696-703);
- las demás cajas de PB-TC.

Ninguna línea con `SetNumber(` se añade ni se quita (INV-PBTC2-02).

### D-PBTC2-07. La medición en la cadena del efecto no medido (OB-06.4)

Es como D-PBTC-10 y PBTC-C14 de PB-TC. Lo que no está medido es el efecto de la corrección en la salida: el hueco exacto, el módulo que sigue calculado con su longitud exacta y las cuatro avanzadas exactas. Por lectura, son menos de 0,0005" y el BOM en diezmilésimas ([OPT1], pregunta 1).

- **Paso 1 del Worker, sobre `BaseSha` y antes del RED:** una corrida temporal, que no entra en ningún commit. Sus cambios temporales:
  1. un método temporal en `PushBackDeclaredCorrection2RoundingTests.cs` que, por fila, resuelve dos diseños con `BuildFrom` y el catálogo entregado:
     - **lo que escribe hoy la base** tras el gesto (el valor redondeado y, en la fila del módulo, también manual);
     - **lo que escribirá el GREEN** (el valor exacto).

     Calcula entre los dos K1, K1o, K2, K3 y K4 con la fórmula de CT-PB0-21 (`tests/RackCad.Tests/PushBackFullEditorCharacterizationRoundingEffectTests.cs:107-245`, «material» en `:164`), copiada en el código temporal. Si K3 es cierto, añade el detalle de cada línea del BOM que difiere, propiedad por propiedad;
  2. termina en `Assert.Fail(string.Join("\n", lineas))`, porque `Assert.Equal` trunca.

  **Las filas:**

  | Fila | Diseño (`EditInner`) | Base tras el gesto | GREEN |
  |---|---|---|---|
  | T2-1 hueco | `pb-20-ab`, `Composite.Gap` | 54 | 54,00044 |
  | T2-2 módulo | `pb-31-v2-modulos`, `Structure.Modules[1]` (M2) | `Length` 47,992, `IsManualOverride` true, `IsCalculated` false | `Length` 47,99213, calculado |
  | T2-3 cabecera | `pb-06-seguridad`, `Structure.ManualHeaderHeightOverride` | 150 | 150,00044 |
  | T2-4 separación | `pb-06-seguridad`, `Structure.SeparatorSpacingOverride` | 18 | 18,00044 |
  | T2-5 poste derivado | `pb-07-combinado`, `Structure.DerivedPostHeight` | 140 | 140,00044 |
  | T2-6 refuerzo | `pb-07-combinado`, `Structure.DerivedPostReinforcementHeight` | 24 | 24,00044 |

- **Su TRX y su log** se guardan como artefactos, con rutas relativas comprobadas (M-04). K1 a K4, y el detalle del BOM, se transcriben fila por fila en `worker-handoff.json`.
- **Qué es material y cuándo se para (PBTC2-C14).** El Owner aceptó «menos de 0,0005" y el BOM en diezmilésimas». Se para si alguna fila cumple una de estas condiciones:
  - K1 o K1o no vacías;
  - K2 cierto;
  - K4 no vacía (coordenadas desplazadas más de 0,001);
  - K3 cierto con una línea del BOM que difiere en algo que no es un número con |Δ| < 0,001.
- **OWNER-1 no entra en esta medición:** es una diferencia menor que 0,0005 entre dos valores que el Owner pidió igualar, de la misma clase que T-3 R8 (el alto de la tarima: nada).

### D-PBTC2-08. Las pruebas

- **Las ventanas:** se abren como `var w = Ventana(); try { Cargar(w, t); … } finally { Cerrar(w); }`, con la carga dentro del `try` (D-PBTC-08 de PB-TC). Ninguna se muestra.
- **M-05:** cada método de PB-TC2 corre entero dentro de `using (EditorDiscardPrompt.Substitute(_ => { descartes++; return true; }))`. Si una ventana se cerrara con cambios de módulos pendientes, no saldría el `MessageBox` real. La última línea de cada ventana fija `descartes=0`, porque «Actualizar» confirma lo pendiente antes de cerrar (W:3386-3389).
- **Los gestos:** sin teclado ni foco real. Son `SetNumberR` de S0; `EditorWindowTestSupport.SetText` seguido de `LostFocus` elevado; `IsChecked` y `SelectedIndex` asignados; `SelectModule` de S0 y `Click`.
- **Los diseños:** con `EditInner` sobre el corpus. Ningún fixture nuevo en `Fixtures/I64`.
- **Las salidas:** `Salida(w)` (S0) y `Persist(w.DesignToInsert)` tras «Actualizar», con `Num` (SC1). Para los booleanos y el `ModuleId`, un ayudante privado de la propia clase lee el JSON.

## 4. Las pruebas nuevas (lista cerrada: CT-PBTC2-01 a CT-PBTC2-04)

**Definiciones:**
- **Las clases:** `PushBackDeclaredCorrection2RoundingTests` (CT-PBTC2-01 a 03) y `PushBackDeclaredCorrection2MixedBoxTests` (CT-PBTC2-04), en `tests/RackCad.UI.Tests/`.
  - Cada una es `public sealed class`, con `namespace` `RackCad.UI.Tests` y `[Collection(EditorDiscardPromptCollection.Name)]`.
  - Los métodos son `[Fact]` y no hay ningún `Theory`.
  - Cada método arma una lista de líneas y termina en **un** `Assert.Equal(<lista esperada>, <lista>)`.
- **Las ayudas:** solo de uso, las de S0, de SC1 y `EditorWindowTestSupport.SetText`. No hay ayudante nuevo.
- **Los formatos:** los de [PKC] §4.1.
  - Los números van con «R» (`Num`), y `<null>` es el nulo.
  - `texto=<t>` es el texto actual de la caja. En CT-PBTC2-04 va entre corchetes, `texto=[<t>]`, porque puede estar vacío.
  - `<caja>=<v>` es el número o el texto escrito.
  - Los booleanos se escriben como `True` y `False`.
- **La cultura:** la invariante, por `Sta`.

**CT-PBTC2-01. El hueco sin editar conserva el exacto, y llega a RACKEDITAR.** `CT_PBTC2_01_AnUneditedGapBoxKeepsTheExactGapUpToTheUpdate`.
- **Diseño:** `pb-20-ab` con `Composite.Gap = 54.00044`.
- **Ventana 1:**
  1. `Cargar` y `Salida`: `carga Gap=54.00044`;
  2. `GapBox texto=54`;
  3. `LostFocus` elevado en `GapBox`, sin editar: `GapBox sin editar Gap=54.00044`;
  4. `Click(w, "UpdateButton")`, leyendo `Persist(w.DesignToInsert)`: `peticion Gap=54.00044 descartes=0`.
- **Ventana 2:**
  1. `Cargar` y `SetNumberR(w, "GapBox", 60)`: `GapBox=60 Gap=60 descartes=0`.
- **En la base:** la línea 3 da `Gap=54`, por `SetGap(54)` (WC:198). Falla por aserción.
- **No es tautológica (M-02):** un producto que no aplique nunca la caja falla en la ventana 2, y uno que redondee, en la línea 3.

**CT-PBTC2-02. Pasar por la longitud de un módulo calculado no lo vuelve manual; escribir otra sí.** `CT_PBTC2_02_PassingThroughTheModuleLengthKeepsACalculatedModuleAndAnEditMakesItManual`.
- **Diseño:** `pb-31-v2-modulos` con `Structure.Modules[1].Length = 47.99213`. Es M2 `Separator`, calculado y no manual (medición D-1): el caso del expediente.
- **`M2`** = `ModuleId`, `Length`, `IsCalculated` e `IsManualOverride` de `Structure.Modules[1]` del JSON leído.
- **Ventana 1:**
  1. `Cargar` y `SelectModule(w, "M2")`: `M2 texto=47.992`;
  2. `LostFocus` elevado en `ModuleLengthBox`, sin editar: `sin editar pendiente=False`, con `pendiente` = `w.PendingWork().HasPendingWork`;
  3. `Click(w, "UpdateButton")`: `sin editar peticion M2 Length=47.99213 IsCalculated=True IsManualOverride=False descartes=0`.
- **Ventana 2** (N-08):
  1. `Cargar`, `SelectModule(w, "M2")` y `SetNumberR(w, "ModuleLengthBox", 56)`: `editado pendiente=True`;
  2. `Click(w, "UpdateButton")`: `editado peticion M2 Length=56 IsCalculated=False IsManualOverride=True descartes=0`.
- **En la base:** en la ventana 1, `Val` da 47,992 y la diferencia es 0,00013 ≥ 0,0001. Así que se escenifica `SetLength(47.992)`: `pendiente=True`, y la petición lleva `Length=47.992 IsCalculated=False IsManualOverride=True`. Falla por aserción en la línea 2.
- **El caso negativo** (la ventana 2, M-03) pasa también en la base. Un producto que no escenificara nunca fallaría en ella.

**CT-PBTC2-03. Las cuatro anulables sin editar conservan el exacto; vaciar una sigue siendo el cálculo.** `CT_PBTC2_03_UneditedNullableAdvancedBoxesKeepTheExactValueAndAnEmptyBoxIsTheCalculation`.
- Antes de los gestos de cada ventana: `Check(w, "AdvancedModulesToggle").IsChecked = true`, como CT-PB0-05.
- **Ventana `pb-06-seguridad`,** con `ManualHeaderHeightOverride = 150.00044` y `SeparatorSpacingOverride = 18.00044`:
  1. `pb-06-seguridad carga ManualHeaderHeightOverride=150.00044 SeparatorSpacingOverride=18.00044`;
  2. `pb-06-seguridad textos RackHeaderHeightBox=150 RackSeparatorSpacingBox=18`;
  3. tras `LostFocus` en `RackSeparatorCountBox`, que no cambia y hace releer las seis: `pb-06-seguridad sin editar ManualHeaderHeightOverride=150.00044 SeparatorSpacingOverride=18.00044`;
  4. tras `SetNumberR(w, "RackSeparatorSpacingBox", 21.5)`: `pb-06-seguridad RackSeparatorSpacingBox=21.5 ManualHeaderHeightOverride=150.00044 SeparatorSpacingOverride=21.5`;
  5. tras `SetText(w, "RackHeaderHeightBox", "")` y `LostFocus`: `pb-06-seguridad RackHeaderHeightBox vacia ManualHeaderHeightOverride=<null> SeparatorSpacingOverride=21.5`;
  6. tras `Click(w, "UpdateButton")`: `pb-06-seguridad peticion ManualHeaderHeightOverride=<null> SeparatorSpacingOverride=21.5 SeparatorCountOverride=3 descartes=0`.
- **Ventana `pb-07-combinado`,** con `DerivedPostHeight = 140.00044` y `DerivedPostReinforcementHeight = 24.00044` (pb-07 trae `DerivedPostReinforced = true`):
  1. `pb-07-combinado carga DerivedPostHeight=140.00044 DerivedPostReinforcementHeight=24.00044`;
  2. `pb-07-combinado textos DerivedPostHeightBox=140 DerivedPostReinforcementHeightBox=24`;
  3. tras `LostFocus` en `DerivedPostHeightBox`, sin editar: `pb-07-combinado sin editar DerivedPostHeight=140.00044 DerivedPostReinforcementHeight=24.00044`;
  4. tras «Actualizar»: `pb-07-combinado peticion DerivedPostHeight=140.00044 DerivedPostReinforcementHeight=24.00044 descartes=0`.
- **En la base:** la línea 3 de `pb-06` da 150 y 18. Falla por aserción.
- **No es tautológica:** la línea 4 falla con un producto que no aplique la caja, y la línea 5 con uno que trate la caja vacía como «conservar».

**CT-PBTC2-04. En «Ambos lados», una caja mixta que recibe un valor lo aplica a los dos lados; una no mixta conserva el exacto.** `PushBackDeclaredCorrection2MixedBoxTests.CT_PBTC2_04_AMixedBoxThatReceivesAValueAppliesItToBothSidesAndANonMixedBoxKeepsTheExactValue`.
- **Notación:**
  - `A.alto` = `Structure.Fronts[0].Levels[0].PalletHeight` y `B.alto` = `SideB.Fronts[0].Levels[0].PalletHeight`;
  - `A.largo` y `B.largo` son lo mismo con `BeamLengthOverride`;
  - todos se leen de `Salida`;
  - «Ambos» = `Combo(w, "SideSelectorBox").SelectedIndex = 2` (medición C-7).
- **Ventana mixta:** `pb-20-ab` con `A.alto = 61.23456`, `A.largo = 100.00044` y `B.largo = 120`. `B.alto` queda en 60.
  1. `mixta carga A.alto=61.23456 B.alto=60 A.largo=100.00044 B.largo=120`;
  2. tras «Ambos»: `mixta Ambos CellPalletHeightBox texto=[] CellBeamLengthOverrideBox texto=[]`;
  3. tras `SetText(w, "CellPalletHeightBox", "61.235")` y `LostFocus`: `mixta CellPalletHeightBox=61.235 A.alto=61.235 B.alto=61.235`;
  4. tras `SetText(w, "CellBeamLengthOverrideBox", "100")` y `LostFocus`: `mixta CellBeamLengthOverrideBox=100 A.largo=100 B.largo=100`.
- **Ventana no mixta (el caso negativo, M-03):** `pb-20-ab` con `A.alto = 61.23456` y `B.alto = 61.23456`.
  1. `no mixta carga A.alto=61.23456 B.alto=61.23456`;
  2. tras «Ambos»: `no mixta Ambos CellPalletHeightBox texto=[61.235]`;
  3. tras `SetText(w, "CellPalletHeightBox", "61.2350")` y `LostFocus`: `no mixta CellPalletHeightBox texto=[61.2350] A.alto=61.23456 B.alto=61.23456`.
- **En la base:** la línea 3 de la ventana mixta da `A.alto=61.23456 B.alto=61.235`. Falla por aserción.
- **No es tautológica:** un producto que aplique siempre lo escrito en «Ambos» falla en la ventana no mixta, y uno que mantenga `Keep` en la caja mixta, en la mixta.

**Trazabilidad:**

| Obligación | CT |
|---|---|
| D-PB-06, paso 2 (un RED por corrección) | 01 (el hueco), 02 (el módulo), 03 (las avanzadas) y 04 (OWNER-1) |
| D-PB-06, paso 3 (el comportamiento corregido) | El GREEN de W y WC; INV-PBTC2-02 a 04 sobre el diff |
| D-PB-06, paso 4, en RACKEDITAR | 01, 02 y 03, con «Actualizar». El 04 lee `Salida` |
| D-PB-06, paso 4, en el Workspace | Pendiente de PB-T2 (§5.3) |
| D-PB-06, paso 5 | Sin sustituciones (§5.2) |
| D-PB-06, paso 6 | La sección de PB-TC2 de [SC], DECLARADA y después EJECUTADA (§5.3) |
| OB-06.4 | D-PBTC2-07 y PBTC2-C14 |
| M-03 (una extensión, un caso negativo) | La ventana 2 de 02, las líneas 4 y 5 de 03, y la ventana no mixta de 04 |
| M-05 | `descartes=0` en cada ventana, y la costura en cada método |

**Mapa CT → método:**

| CT | Clase | Método |
|---|---|---|
| 01 | `PushBackDeclaredCorrection2RoundingTests` | `CT_PBTC2_01_AnUneditedGapBoxKeepsTheExactGapUpToTheUpdate` |
| 02 | `PushBackDeclaredCorrection2RoundingTests` | `CT_PBTC2_02_PassingThroughTheModuleLengthKeepsACalculatedModuleAndAnEditMakesItManual` |
| 03 | `PushBackDeclaredCorrection2RoundingTests` | `CT_PBTC2_03_UneditedNullableAdvancedBoxesKeepTheExactValueAndAnEmptyBoxIsTheCalculation` |
| 04 | `PushBackDeclaredCorrection2MixedBoxTests` | `CT_PBTC2_04_AMixedBoxThatReceivesAValueAppliesItToBothSidesAndANonMixedBoxKeepsTheExactValue` |

## 5. El RED, las sustituciones y el registro

### 5.1 El RED

- **El RED:** solo los dos archivos nuevos, ningún archivo de `src/`. Los cuatro métodos fallan **por aserción** en la base, en la línea que dice cada CT. Ninguno lanza antes: las cargas, `SelectModule`, `SetNumberR`, `SetText`, el selector de lado y «Actualizar» son mecánicas que ya pasan en la CI de HEAD (CT-PB0-03, CT-PB0-05, CT-PB0-20, CT-PBTC-03, CT-PBTC-14 y `PushBackBothModeCellBeamOverrideTests`).
- **El GREEN:** solo W y WC.
- **Core no tiene RED:** su filtro va con `ExpectRed` false, como en PB-TC.

### 5.2 Las sustituciones

**Ninguna** (H2-10; medición E-7). Si el Worker encontrara una prueba existente que choca, sería PBTC2-C02. No hay patrón I-39 ni nombre `SUS2-0n` que usar en esta v1.

### 5.3 El registro declarado: la sección de PB-TC2

La sesión la crea en [SC] en el commit de emisión, en estado DECLARADA, y la pasa a EJECUTADA al cerrar, con el RED, el GREEN y sus CI. Texto propuesto:

> ## PB-TC2 — Las cajas que aún redondeaban y la caja mixta de «Ambos lados» (F4B-E09)
>
> **Estado: DECLARADA.** Autoridad: [OD] D-PB-06 («rounding behavior/state») y la decisión del Owner F4B-E09, pregunta 1 (`F4B-DRAFTS/coordinator/owner-pbt1-pre-decisions.md`). Contrato: `F4B-PBTC2-DECLARED-CORRECTIONS/coordinator/gate-contract.json`.
>
> **Sustituciones: ninguna.** Ninguna prueba existente fija el redondeo de estas cajas ni el «Ambos lados» corregido.
>
> **El cambio de comportamiento declarado de RACKEDITAR:**
> - el hueco entre lados, la longitud de un módulo y las cuatro cajas avanzadas anulables (`ManualHeaderHeightOverride`, `DerivedPostReinforcementHeight`, `DerivedPostHeight` y `SeparatorSpacingOverride`) dejan de guardarse redondeados a tres decimales con solo pasar por la caja. Escribir algo que se ve igual conserva el exacto; escribir algo distinto manda; vaciar una avanzada sigue siendo «el cálculo»;
> - pasar por la longitud de un módulo calculado ya no lo convierte en manual;
> - en «Ambos lados», una caja mixta que recibe un valor lo aplica a los dos lados, como dice su ayuda;
> - con tres decimales o menos, nada cambia.
>
> **Pendiente de PB-T2:** probar estas correcciones también en el modo WORKSPACE de la ventana (OB-06.3).

**Además, en la emisión,** bajo el «Pendiente: decisión del Owner antes de PB-T1» (`:175-182`) y bajo la «Limitación conocida… OWNER-1» (`:209-211`), una línea: «Decidido por el Owner en F4B-E09: se corrige en PB-TC2 (sección PB-TC2)». **Al cerrar,** las dos pasan a «Cerrado por PB-TC2 (GREEN `<sha>`)».

## 6. Contrato propuesto (a fijar en la emisión)

### 6.1 Identidad

- `Schema` `rackcad-gate-contract/v1`; `Unit` I-64; `Gate` F4B; `TaskId` **`F4B-PBTC2-DECLARED-CORRECTIONS`**.
- Evidencia en `docs/automation/evidence/I-64-pilot/F4B-PBTC2-DECLARED-CORRECTIONS/`.
- Attempts 1 de 3; `RoutingEnforcement` `required`; `CorrectionsAuthorized` true.
- **Objective** (ASCII): «Corregir en la ventana clasica de Push Back, como cambios declarados de RACKEDITAR por D-PB-06 y la decision del Owner F4B-E09, las cajas que aun redondean (H-31: GapBox, ModuleLengthBox y las cuatro cajas avanzadas anulables, con la regla Keep de PB-TC y KeepOptional) y la caja mixta de 'Ambos lados' (OWNER-1: una caja mixta que recibe un valor lo aplica a los dos lados), segun el paquete del contrato de PB-TC2 v1 y la lista cerrada CT-PBTC2-01..04. El RED son pruebas nuevas que fallan por asercion en la base, sin sustituciones; el GREEN corrige solo W y WC. NumericField, Cantilever, Application, el Plugin y el XAML no cambian; c-02 y pb-21-cola quedan para PB-T1; el modo Workspace queda para PB-T2; no se ejecuta AutoCAD.»

### 6.2 `AllowedWriteScope` (4 rutas exactas)

1. `src/RackCad.UI/Systems/PushBack/RackPushBackSystemWindow.xaml.cs`
2. `src/RackCad.UI/Systems/PushBack/RackPushBackSystemWindow.Composite.cs`
3. `tests/RackCad.UI.Tests/PushBackDeclaredCorrection2RoundingTests.cs`
4. `tests/RackCad.UI.Tests/PushBackDeclaredCorrection2MixedBoxTests.cs`

### 6.3 `ForbiddenWriteScope` (129 entradas)

Con el método de [PKC-C], hermano por hermano: un prefijo solo si ninguna ruta permitida queda debajo. Lo calcula el generador con `assert` sobre los recuentos, la existencia en HEAD, la cobertura de `src/` y la no existencia de las rutas nuevas (medición F-6).

- **`src/`, 59:**
  - `src/RackCad.Application/`, `src/RackCad.Domain/` y `src/RackCad.Plugin/`;
  - en `src/RackCad.UI/`, las 9 carpetas y los 34 archivos de la raíz, como en [PKC] §6.3;
  - `Systems/Cantilever/`, `Dynamic/`, `FlowBed/`, `Larguero/` y `Selective/`;
  - en `Systems/PushBack/`, los 7 hermanos de la ventana **y `RackPushBackSystemWindow.xaml`**, que PB-TC2 no toca.
- **`tests/`, 53:**
  - `tests/RackCad.Tests/`, entero;
  - en `tests/RackCad.UI.Tests/`, 51 archivos:
    - los ayudantes: `RackCad.UI.Tests.csproj`, `StaTestRunner.cs`, `EditorWindowTestSupport.cs`, `EditorDiscardPromptCollection.cs`, `PushBackFullEditorCharacterizationSupport.cs` y `PushBackDeclaredCorrectionSupport.cs`;
    - las cinco clases de PB-TC: `PushBackDeclaredCorrection{Rounding,DimensionStyle,PilotParity,SideBNotice,StandardRestore}Tests.cs`;
    - las seis de PB-T0: `PushBackFullEditorCharacterization{State,Module,Composite,Subdialog,Catalog,RoundingLoss}Tests.cs`;
    - las seis de T1B;
    - las 11 guardas de [PKC] §6.3;
    - las que tocan las seis cajas o «Ambos lados»: `PushBackAdvancedRackParametersWindowTests.cs`, `PushBackCompositeWindowTests.cs`, `PushBackCompositeLifecycleWiringTests.cs`, `PushBackCompositeLoadExistingModulesTests.cs`, `PushBackBothModeCellBeamOverrideTests.cs`, `PushBackA1EditorTests.cs`, `PushBackDormantRestoreTests.cs`, `PushBackDormantTailTransactionTests.cs`, `PushBackEditorSideOwnershipTests.cs`, `PushBackLibrarySaveGateTests.cs`, `PushBackOutputGateAfterRecomputeTests.cs` y `PushBackHeaderLineWindowTests.cs`;
    - las de OB-06.6: `CantileverAdvancedPanelWindowTests.cs`, `CantileverComponentEditorTests.cs`, `CantileverEditorWindowTests.cs`, `NumericFieldTests.cs` y `NumericFieldValidationTests.cs`;
  - `tests/RackCad.UI.Tests/Workspace/`, como un solo prefijo, porque ninguna ruta permitida queda debajo.
- **Raíz, 17:** las de [PKC] §6.3.

El orden exacto es la salida del generador con `--dry --print`.

### 6.4 `RequiredTests`

| Proyecto | Filtro (`FullyQualifiedName~RackCad.<proyecto>.`…) | `MinSelected` | `ExpectRed` | Para qué |
|---|---|---|---|---|
| UI | `PushBackDeclaredCorrection2` | **4** | true | CT-PBTC2-01 a 04 |
| UI | `PushBackDeclaredCorrectionRoundingTests` | 3 | false | PB-TC (CT-PBTC-01 a 03). **Por clase**, porque el prefijo de PB-TC seleccionaría también las clases nuevas (medición F-4) |
| UI | `PushBackDeclaredCorrectionDimensionStyleTests` | 2 | false | PB-TC |
| UI | `PushBackDeclaredCorrectionPilotParityTests` | 2 | false | PB-TC |
| UI | `PushBackDeclaredCorrectionSideBNoticeTests` | 2 | false | PB-TC |
| UI | `PushBackDeclaredCorrectionStandardRestoreTests` | 1 | false | PB-TC (D-PB-11) |
| UI | `PushBackFullEditorCharacterization` | 16 | false | PB-T0: 16 Passed, y CT-PB0-19 y CT-PB0-05 Skipped (SUS-03 y SUS-04) |
| UI | `PushBackClassicCharacterization` | 55 | false | T1B (OB-T1B.3) |
| UI | `Cantilever` | 67 | false | OB-06.6 |
| UI | `PushBackModuleEditorWindowTests` | 13 | false | Módulos |
| UI | `PushBackAdvancedRackParametersWindowTests` | 9 | false | Avanzados |
| UI | `PushBackCompositeWindowTests` | 52 | false | Compuesto: 40 Fact y 12 filas de 4 Theory |
| UI | `PushBackCompositeLifecycleWiringTests` | 7 | false | Compuesto |
| UI | `PushBackCompositeLoadExistingModulesTests` | 3 | false | Compuesto con módulos |
| UI | `PushBackBothModeCellBeamOverrideTests` | 10 | false | «Ambos lados» |
| Core | `PushBackClassicCharacterization` | 10 | false | T1B |
| Core | `PushBackFullEditorCharacterization` | 3 | false | PB-T0 |
| Core | `PushBackModuleEditorCharacterizationTests` | 15 | false | Fact1 y Fact3 |

- Los proyectos son `tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj` y `tests/RackCad.Tests/RackCad.Tests.csproj`.
- Los recuentos son estáticos (medición F-3).
- Los `MinSelected` de las filas con `Skip` cuentan solo los que pasan.

### 6.5 Invariantes

- **INV-PBTC2-01. Alcance.**
  - `git diff --name-only BaseSha..CurrentSha` cae dentro de las 4 rutas.
  - En `src/`, solo W y WC.
  - `git diff BaseSha..CurrentSha -- src/RackCad.UI/Controls src/RackCad.UI/Systems/Cantilever src/RackCad.Application src/RackCad.Domain src/RackCad.Plugin src/RackCad.UI/Systems/PushBack/RackPushBackSystemWindow.xaml` está vacío.
- **INV-PBTC2-02. RED y GREEN.**
  - **El RED:** solo los dos archivos nuevos. Se publica antes del GREEN.
  - **El GREEN:** solo W y WC, sin `tests/`.
  - **Criterio mecánico del RED,** solo sobre el filtro con `ExpectRed`: todos `Failed`; `Message` sin el prefijo de tipo de [PKC-C]; la primera línea del `StackTrace` bajo `tests/`; ninguno `NotExecuted` ni `Skipped`; y `Failed == Selected >= 4`.
  - **En el RED,** los filtros sin `ExpectRed` pasan, con los mismos `Skipped` que en la base (SUS-01 a SUS-04 de PB-TC).
  - Ninguna línea añadida ni quitada de `git diff -U0 BaseSha..CurrentSha -- W WC` contiene `SetNumber(`.
- **INV-PBTC2-03. Lo que cambia, sobre el diff.** Cada hunk de `git diff -U0 BaseSha..CurrentSha -- src` cae en uno de estos sitios:
  - WC:198, en `Gap_Changed` (WC:184-200);
  - W:1905, en `ModuleLength_Changed` (W:1892-1913);
  - W:1639, :1641, :1642 y :1646, en `ReadAdvancedRackParameters` (W:1632-1647);
  - W:818-822, en `ReadCellValues` (W:802-831);
  - `OptionalVal` (W:3665-3668), y la línea W:3641 de su documentación;
  - la línea W:3683 de la documentación de `Keep`;
  - la retirada de `Val` (W:3634-3635);
  - la declaración de los miembros nuevos `KeepOptional` y `KeepCell` entre `Keep` (W:3686) e `IntVal` (W:3688).

  **Qué es tocar un miembro:** cambiar o quitar una línea de su rango, o insertar dentro de él (en un hunk `@@ -a,0 …`, cuando s ≤ a < e). Una inserción entre dos miembros es una declaración nueva, y solo se admite si está en esta lista. Ningún otro miembro de W ni de WC se toca.
- **INV-PBTC2-04. Las cuentas, sobre `CurrentSha`:**
  - en W no aparecen `Val(` ni `cref="Val"`;
  - aparece `Keep(GapBox, composite.Gap)` en WC y `Keep(ModuleLengthBox, module.Length)` en W;
  - hay cuatro llamadas `KeepOptional(` en `ReadAdvancedRackParameters` y una en `OptionalVal`, más su declaración;
  - hay cinco `KeepCell(` en `ReadCellValues`, más su declaración;
  - en W, `IsMixed(` aparece solo en `KeepCell` y en `OptionalVal`.

  El comportamiento lo fijan CT-PBTC2-01 a 04 en verde.
- **INV-PBTC2-05. Lo que sigue en verde:**
  - PB-TC (sus 10 métodos), PB-T0 (16 en UI y 3 en Core), T1B (55 y 10) y Cantilever (67);
  - Fact1 y Fact3, G-10, I-55, el desviador, T-SG, T-KB, el censo de ventanas y el corpus de WLC;
  - las clases de módulos, compuesto, «Ambos lados» y avanzados del §6.4.

  W sigue sin `forceRebuild:` ni `forceRebuild)`, y con una sola `.RequestUpdate(`.
- **INV-PBTC2-06. Sin sustituciones.** Ninguna prueba existente se edita, se salta, se reactiva ni se borra.
- **INV-PBTC2-07. Las pruebas de UI.**
  - Las dos clases van en `EditorDiscardPromptCollection`.
  - Las ventanas se abren con `Ventana()` y su carga dentro del `try`, y se cierran en `finally`. Ninguna se muestra.
  - Cada método corre dentro de `EditorDiscardPrompt.Substitute`, y cada ventana termina en `descartes=0`.
  - Sin teclado ni foco real.
  - Prohibidos un `MessageBox` real, el BOM, «Guardar en biblioteca», el configurador real e «Insertar».
  - Ninguna aserción dentro de una costura.
  - Ningún fixture nuevo en `Fixtures/I64`.
- **INV-PBTC2-08. Determinismo:** el GUID fijo de `Ventana()`, la cultura invariante, los números con «R», el orden ordinal, y ni hora ni aleatoriedad.
- **INV-PBTC2-09. Las corridas locales.**
  - Nunca la suite completa de UI en local, solo los filtros del §6.4. La de Core, sí.
  - Sin copias de árboles (MAX_PATH).
  - Las corridas temporales (D-PBTC2-07) no entran en ningún commit, terminan en `Assert.Fail` y guardan su TRX y su log como artefactos.
  - El perfil del usuario no cambia, con la prueba de la limpieza de INV-PBT0-06.
- **INV-PBTC2-10. CI y compilación.**
  - **GREEN:** 4/4 con las suites completas.
  - **RED:** `ui-tests` en `failure`; `tests`, `build-ui` y `build-plugin` en `success`; sin `blame-hang` ni timeout.
  - **Avisos:** los conteos por código, iguales a la línea base.
- **INV-PBTC2-11. La evidencia (M-04).**
  - Toda ruta de `worker-handoff.json` es relativa y se comprueba que existe.
  - `TreeSha` es el árbol real (`git write-tree`) o null.
- **INV-PBTC2-12. El registro.** El Worker no escribe en `docs/`. La sección de [SC] y las notas del «Pendiente» las escribe la sesión.
- **INV-PBTC2-13. Las reglas de F4B:**
  - la planificación, según PBTC2-C08;
  - los controles, según PBTC2-C11;
  - antes de un intento 3, siempre el Owner (PBTC2-C12).
- **INV-PBTC2-14. Sin AutoCAD.**

### 6.6 `ExpectedEvidence`

- **Antes del Worker:** el contrato y la delegación válidos; la comparación byte a byte, A1-A8 y nc4; el `AllowedWriteScope` de la delegación igual al del contrato; el registro de una replanificación por PBTC2-C08, si la hubo.
- **Paso 1, sobre `BaseSha`** (D-PBTC2-07):
  - la corrida temporal, con `Assert.Fail`;
  - K1, K1o, K2, K3 y K4, y el detalle del BOM, de T2-1 a T2-6, transcritos en `worker-handoff.json`;
  - el TRX y el log, como artefactos.

  Si PBTC2-C14 se activa, STOP antes del RED.
- **El RED y su push:** la CI del RED de INV-PBTC2-10; los TRX del filtro con `ExpectRed` cumplen INV-PBTC2-02; los demás filtros, en verde.
- **El GREEN y su push:**
  - `git diff --name-only RedSha..CurrentSha` = W y WC;
  - las comprobaciones de INV-PBTC2-02 a 04 sobre el diff, con su salida;
  - la CI 4/4, con los conteos del §6.9.
- **En local:**
  - el build Release de la solución, con el Plugin y sin avisos nuevos, sobre el RED y sobre el GREEN;
  - los filtros del §6.4 sobre los dos;
  - tres corridas seguidas en verde del filtro con `ExpectRed` sobre el GREEN;
  - la prueba de la limpieza del perfil.
- **`worker-handoff.json`:**
  - el RED, el GREEN, el alcance, el trailer y los conteos por filtro;
  - el mapa CT → método;
  - por CT, la primera diferencia en la base, con su mensaje;
  - la transcripción del paso 1;
  - las rutas relativas comprobadas;
  - `TreeSha` real o null;
  - `KnownLimitations`.
- **Después:**
  - la verificación del Controller;
  - nc1 a nc3, según el README §10;
  - un control que no da CONTROL PASS, solo como dice PBTC2-C11;
  - cada uso, registrado en `f4b-dispositions.md`.

### 6.7 `StopConditions`

**Las estándar,** copiadas de [PKC-C]: S-02, S-03, S-04, S-05, S-07, S-08, S-12, S-13 y S-14, y P-01 a P-08, con P-01 de la Snapshot I.

**Las propias:**
- **PBTC2-C01:** la corrección necesitaría cambiar un archivo fuera de W y WC: `NumericField`, WX, otra ventana, Application o el Plugin (OB-06.6).
- **PBTC2-C02:** habría que editar, saltar, reactivar o borrar una prueba o un ayudante existente, tocar un `.csproj`, `InternalsVisibleTo` o los paquetes, o añadir un fixture a `Fixtures/I64`. Una guarda de T1B, Fact3 o una prueba de PB-TC que no queda en verde vuelve al Owner. Una sustitución necesaria es un defecto del paquete y vuelve a la sesión con `SUS2-0n` propuesto.
- **PBTC2-C03 (OB-06.4):** un efecto material no medido. Es decir, una diferencia en el diseño, las piezas, los niveles, las posiciones o el BOM que no sale de los valores exactos de las seis cajas ni de la igualación de «Ambos lados»; o un fallo, en el GREEN, de una prueba de §6.4.
- **PBTC2-C04:** un método del filtro con `ExpectRed` pasa en la base, o falla por excepción.
- **PBTC2-C05:** un CT necesitaría AutoCAD, un `MessageBox` real, el configurador real, el BOM, «Guardar en biblioteca», «Insertar», el teclado o el foco real, o escribir en el perfil o en el disco.
- **PBTC2-C06:** la CI del GREEN falla fuera de las pruebas nuevas, se reproduce y no es atribuible a ellas.
- **PBTC2-C07:** los hechos del §2 no se sostienen de un modo que cambia un CT o el diseño; o una línea esperada no se cumple en el GREEN. El Worker no ajusta lo esperado.
- **PBTC2-C08:** como PBTC-C08 ([DP], pregunta 2): una sola replanificación por la comparación literal; cualquier otro rechazo, al Owner.
- **PBTC2-C09:** una prueba colgaría, dejaría viva una ventana, un modal o un dispatcher, escribiría en el perfil o dependería de rutas de una máquina.
- **PBTC2-C10:** hacerlo bien exigiría tocar algo fuera del alcance: c-02 (el poste), `pb-21-cola`, las demás normalizaciones de T-2, D-PB-11, las cajas de PB-TC fuera de «Ambos lados», o cualquier miembro no listado en INV-PBTC2-03.
- **PBTC2-C11** ([DP], pregunta 1; la plantilla es PBTC-C11 de [PKC] §6.7). Un control que no da CONTROL PASS se conserva tal cual, y solo hay dos salidas sin volver al Owner:
  - **nc2 con la firma exacta de PD-F4-01.** El texto literal de su fuente (`f4-dispositions.md:1103-1106`):

    > «- Scope en fail sobre la ruta quitada;
    > - Contract en fail solo por la cláusula de exactitud;
    > - `EXECUTION_BLOCKED`/STOP.»

    Y su aplicación literal (`:1540`): «ninguna otra comprobación distinta de la verificación real».

    **Qué cuenta como la cláusula de exactitud:** forma parte de la firma que la evidencia de Contract cite el contraste del `AllowedWriteScope` estrechado con el contrato o con `byte-fidelity-check.json` (`f4-dispositions.md:520`, `:562`). No forma parte que Contract cite cualquier otra discrepancia.

    → El sustituto **`PBTC2-NC2-EXACT-WHITELIST-01`**: copia de `F4T1B-NC2-EXACT-WHITELIST-01` (`docs/automation/evidence/I-64-pilot/F4-T1B-PILOT-CHARACTERIZATION-nc2-substitute/1/R20261114T235423Z-083e/nc2_exact_whitelist_substitute.py`) con **solo** el identificador y el ámbito cambiados. Registro `nc2 = SATISFIED_UNDER_PBTC2_EXACT_WHITELIST_SUBSTITUTE`.
  - **nc1 con la firma exacta de PD-F4-04.** El texto literal de su fuente (`owner-f4t3-authorization.md:255-265`):

    > «* false CurrentSha correctly detected;
    > * Identity = fail;
    > * Handoff also fails first only because the handoff itself contains the false
    > SHA;
    > * outcome BLOCKED / STOP;
    > * all other intended checks match;»

    Su aplicación en F4, transcrita literal (`f4-dispositions.md:1657-1660`):

    > «- el CurrentSha falso se detecta, e Identity está en fail con una evidencia que nombra el SHA falso y el HEAD;
    > - Handoff falla primero solo porque la entrega contiene el SHA falso;
    > - `EXECUTION_BLOCKED` / STOP con S-04;
    > - solo difieren consumidores del CurrentSha: Handoff, Identity y Ci, con Remote en pass, un subconjunto de la cascada de F4-T2C.»

    Por tanto, Ci en fail solo como consumidor del `CurrentSha` falso forma parte de la firma. Ci en fail por cualquier otra causa no.

    → El sustituto **`PBTC2-NC1-IDENTITY-SIGNAL-01`**: copia de `F4T3C2-NC1-IDENTITY-SIGNAL-01` (`docs/automation/evidence/I-64-pilot/F4-T3C2-COMMAND-ADVISOR-nc1-substitute/1/R20261113T215949Z-935f/nc1_identity_signal_substitute.py`) con **solo** el identificador y el ámbito cambiados. Registro `nc1 = SATISFIED_UNDER_PBTC2_NC1_IDENTITY_SIGNAL_SUBSTITUTE`.
  - **Cualquier otra firma o variación vuelve al Owner antes de cualquier sustituto.** Por ejemplo:
    - PD-F4-05 y PD-F4-06;
    - nc2 con Ci o Trailer en fail además;
    - cualquier nc1 que no reproduzca la firma literal, como el de PB-TC, con Remote, Scope, Tests y Trailer también en fail;
    - cualquier fallo de nc3.

    La disposición «Aceptarlo solo esta vez» de nc1 en PB-TC (F4B-E08) **no** se extiende a PB-TC2.
  - **Cómo se decide la coincidencia.** Nunca por el nombre de la comprobación:
    - **qué se compara:** la salida del control frente a la de la verificación real, comprobación por comprobación, con el conjunto en fail, la primera en fail, el `Outcome`, la `StopClass` y el texto de la evidencia;
    - **las únicas diferencias admitidas:**
      - PD-F4-01: Scope, en fail sobre la ruta quitada, y Contract, en fail solo por el alcance;
      - PD-F4-04: Identity, Handoff (la primera en fail) y Ci (solo como consumidor del `CurrentSha` falso);
      - en las dos, `EXECUTION_BLOCKED` / STOP;
    - **dónde queda:** la sesión registra la tabla en `f4b-dispositions.md`.
- **PBTC2-C12:** una corrección sería el intento 3. Vuelve al Owner antes de emitirse (D-PB-10).
- **PBTC2-C13:** el diseño necesitaría cambiar el texto de la ayuda de la caja mixta (WC:583-584) o la presentación de una caja.
- **PBTC2-C14:** en el paso 1 (D-PBTC2-07), una fila de T2-1 a T2-6 cumple alguna de estas condiciones:
  - K1 o K1o no vacías;
  - K2 cierto;
  - K4 no vacía;
  - una línea del BOM que difiere en algo que no es un número con |Δ| < 0,001.

  El Worker se detiene **antes del RED**, entrega la transcripción y la sesión la lleva al Owner (OB-06.4; F4B-E09, pregunta 4: «un efecto material sin medir»).

### 6.8 `EligibleCells`

Las de [PKC-C], copiadas:
- **Worker:** `anthropic/subagent/claude-sonnet-5-5`, effort `high` (VerifiedOn 2026-09-30).
- **Controller:** `openai/process/gpt-6-luna`, effort `high` (VerifiedOn 2026-11-11).
- **Vigencia:** el catálogo las da por verificadas el 2026-09-30 (`model-catalog.md:51`, `:97`), y vencen como `STALE` el 2026-12-29 (`:14`). Hoy están vigentes.

### 6.9 Línea base de la CI y conteos previstos

**La línea base:** el run `44232324035`, sobre `5597c05b`, 4/4 `success` (medición F-1 y F-2).
- **Core:** 19 070 Passed de 19 070.
- **UI:** 2 397 Passed, 21 Skipped y 2 418 en total.

**La previsión** sale del diff, no de una ejecución: se añaden 4 métodos en UI, y Core no cambia.

| CI | UI total | Failed | Skipped | Passed | Core |
|---|---|---|---|---|---|
| RED | 2 422 | 4 | 21 | 2 397 | 19 070 Passed |
| GREEN | 2 422 | 0 | 21 | 2 401 | 19 070 Passed |

## 7. Riesgos

| Id | Riesgo | Tratamiento |
|---|---|---|
| R2-01 | **Las líneas esperadas salen de leer el código** | PBTC2-C07: STOP sin ajustar lo esperado |
| R2-02 | **`BeamLengthOverride = 100,00044` y `120` en `pb-20-ab`, y `DerivedPostReinforcementHeight = 24,00044` en `pb-07`, no están medidos como válidos** (medición H) | Si una carga da una salida no válida, PBTC2-C07 |
| R2-03 | **El efecto en el dibujo y el BOM no está medido** | D-PBTC2-07 y PBTC2-C14, antes del RED |
| R2-04 | **El filtro de PB-TC por prefijo** atraparía las clases nuevas | Filtros por clase (§6.4) |
| R2-05 | **M-05**: una ventana con cambios de módulos pendientes que se cerrara sacaría un `MessageBox` real | La costura en cada método y `descartes=0` |
| R2-06 | **Las celdas vencen el 2026-12-29** | Se comprueba en la emisión y en cada delegación |
| R2-07 | **El modo WORKSPACE no está demostrado** | Obligación de PB-T2 (§5.3) |

## 8. Las decisiones del Owner

**Tomadas en F4B-E09** ([OPT1]):
- **Pregunta 1:** «Corregir en PB-TC2 (Recomendado)»: H-31 y OWNER-1, en esta tarea.
- **Preguntas 2 y 3:** c-02 y `pb-21-cola` van a PB-T1, fuera de PB-TC2 (PBTC2-C10).
- **Pregunta 4:** «Sí, emite con esa condición (Recomendado)». La emisión es condicional.

**Lo que se le dijo al Owner y este paquete respeta:**
- la misma regla de PB-TC, y solo en la ventana Push Back (D-PBTC2-06);
- «unas 5 pruebas nuevas y ninguna sustituida». Son 4 CT y 0 sustituciones: el caso del módulo calculado va dentro de CT-PBTC2-02, y el negativo de «Ambos lados», dentro de CT-PBTC2-04;
- «con 3 decimales o menos no cambia nada; con más, menos de 0,0005" y el BOM en diezmilésimas»: es lo que mide el paso 1, y PBTC2-C14 para la cadena si no se cumple;
- «pasar por la longitud de un módulo cuyo largo trae más decimales lo convierte de calculado en manual»: eso es lo que se corrige (CT-PBTC2-02).

**Decisiones materiales nuevas: ninguna.** Se revisaron cuatro candidatas, y ninguna es nueva:
- **escribir en una caja mixta el número que se ve del exacto de un lado iguala los dos lados al número escrito,** y ese lado pierde su diferencia de menos de 0,0005. Es exactamente lo que el Owner eligió («aplica el valor escrito a los dos lados») y lo que promete la ayuda;
- **la regla de «Ambos lados» también alcanza** al cambio de celda, al alcance y a los datos del frente, porque leen la celda con `ReadCellValues`. Es la misma extensión que el Owner aceptó en PB-TC (OD-PBTC-1);
- **escribir una longitud de módulo que se ve igual a la actual ya no queda pendiente.** Es la regla R-03 (i) de PB-TC, y estaba en el texto de la opción de OD-PBTC-5 (F4B-E06);
- **una caja avanzada vacía marca `EmptyRequired` pero sigue siendo «el cálculo».** Es el comportamiento de hoy, y PB-TC2 no lo cambia (H2-03).

## 9. Límites de esta medición

Están en `pbtc2-measurements.md`, apartado H:
- nada se compiló ni se ejecutó;
- el efecto en el dibujo y el BOM lo mide la cadena;
- la validez de tres valores de prueba en sus diseños no está medida (R2-02);
- los recuentos son estáticos;
- no se sabe cómo cuenta el Controller un `Skipped`.
