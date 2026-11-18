> **Redaccion para el repo publico:** las rutas locales se sustituyen por marcadores (`<worktree-de-I-64>`, `<datos-locales-del-usuario>`, `<datos-de-aplicacion-del-usuario>`). El resto no cambia.

# I-64 F4B — Paquete del contrato de PB-TC2 (las correcciones declaradas que quedaron fuera de PB-TC), v2

**Estado:** v2, el texto que se emite. No declara GATE PASS.
- **Revisión independiente:** IRT2-1, EMITIBLE TRAS CORRECCIONES 0/3/7; comprobación acotada de la v2: EMITIBLE 0/0/3, con N-01 a N-03 aplicados en el texto de emision y ninguna decision nueva.
- **Decisiones del Owner:** F4B-E09, con autorización condicional de emisión (pregunta 4).
- **Emitido el 2026-11-18** con attempts 1/3 por esa autorización (F4B-E10). **El contrato manda:** `I-64-pilot/F4B-PBTC2-DECLARED-CORRECTIONS/coordinator/gate-contract.json`.

No declara GATE PASS. La autorización condicional del Owner exige tres cosas:
- la revisión, con 0 bloqueantes y 0 importantes sin resolver;
- ninguna decisión material nueva (§8);
- las demás reglas de F4B: [DP], D-PB-10 y las paradas que vuelven al Owner.

**Lo que corrige**, por decisión del Owner (F4B-E09, pregunta 1: «Corregir en PB-TC2 (Recomendado)»):
- **(1) H-31:** `GapBox`, `ModuleLengthBox` y las cuatro cajas avanzadas anulables, con la regla `Keep` de PB-TC;
- **(2) OWNER-1:** en «Ambos lados», con el lado B presente, una caja mixta que recibe un valor lo aplica a los dos lados.

Es un cambio declarado de RACKEDITAR por D-PB-06 («rounding behavior/state»), con presupuesto propio de 1/3 (D-PB-10). c-02 y `pb-21-cola` van a PB-T1 (F4B-E09, preguntas 2 y 3).

**Base:** rama `architecture/workspace-persistente-rackcad` en `5597c05b`, con `main` en `812baf87`.
- `git diff 4440f67d 5597c05b -- src tests` está vacío: el producto es el GREEN de PB-TC, que está CLOSED / PASS WITH LIMITATIONS (F4B-E08).
- Todo el §2 está medido ahí, en solo lectura. El detalle está en `pbtc2-measurements.md` (apartados A a H de la v1 e I de la v2).

**Patrón:** el paquete de PB-TC, `docs/initiatives/I-64-f4b-pbtc-gate-contract-package.md` v2.2 (**[PKC]**); su contrato, `F4B-PBTC-DECLARED-CORRECTIONS/coordinator/gate-contract.json` (**[PKC-C]**); y su generador. El diseño de H-31 es el del anexo A de la v2 de PB-TC (`F4B-DRAFTS/pbtc/package-v2.md:1217-1302`), revisado en ICT2 (I-03, N-07 y N-08).

**Precondiciones de emisión:**
1. **Las decisiones del Owner,** ya tomadas: `F4B-DRAFTS/coordinator/owner-pbt1-pre-decisions.md` (**[OPT1]**, F4B-E09), custodiado en el commit de emisión y anotado en `f4b-dispositions.md`.
2. **La comprobación acotada de esta v2,** con 0 bloqueantes, 0 importantes sin resolver y ninguna decisión nueva.
3. **Custodia en el commit de emisión,** que es la `AuthorityRevision`:
   - este paquete, en `docs/initiatives/I-64-f4b-pbtc2-gate-contract-package.md`, y el registro de mediciones, en `docs/initiatives/I-64-f4b-pbtc2-measurements.md`;
   - [OPT1] y el expediente, en `F4B-DRAFTS/pbt1/ob065-dossier.md`;
   - en [SC] (`docs/automation/evidence/I-64-pilot/F4B-DRAFTS/superseding-characterization.md`), la sección de PB-TC2 de `superseding-section.md`, en estado DECLARADA, con sus dos notas de cierre (§5.3);
   - el contrato y `make_pbtc2_contract.py`, en `docs/automation/evidence/I-64-pilot/F4B-PBTC2-DECLARED-CORRECTIONS/coordinator/`, con la ruta local del worktree redactada como en PB-T0;
   - en `f4b-dispositions.md`, la obligación de PB-T2 (§5.3) y el arrastre M-07 al paquete de PB-T1 (§10).
4. `git diff 5597c05b <BaseSha> -- src tests` vacío. Si no lo está, se vuelve a medir el §2 (PBTC2-C07).
5. Las celdas del §6.8 no están `STALE` (vencen el 2026-12-29); el `config.toml` coincide con la Snapshot I; el binario de Codex es el acreditado.

## Registro de cambios v1 → v2

| Hallazgo de IRT2-1 | Qué cambió | Dónde |
|---|---|---|
| **I-01** (un indicador mixto obsoleto en un rack de un sentido haría que `KeepCell` y el `OptionalVal` nuevo escribieran el valor redondeado de una caja sin editar) | La regla mixta se aplica solo con «Ambos lados» activo **y** el lado B presente: es la condición con la que hoy se pinta el estado mixto (WC:452). Va en un miembro nuevo, `MixedBoth`. La rama de la caja vacía de `OptionalVal` queda literalmente como hoy (W:3668). CT-PBTC2-04 gana la ventana negativa «sin lado B», con el camino del hallazgo. Se reescribe la frase de la v1 «fuera de "Ambos lados" nada cambia» | H2-06, H2-11; D-PBTC2-04; D-PBTC2-05; CT-PBTC2-04; INV-PBTC2-03 y 04 |
| **I-02** (INV-PBTC2-04 no se podía cumplir: `Val(` es subcadena de `OptionalVal(` e `IntVal(`) | INV-PBTC2-04 se escribe con expresiones regulares de palabra y la orden exacta: `git grep -nP '\bVal\(' -- <W>` vacío, `git grep -nF 'cref="Val"' -- <W>` vacío, y los recuentos de los miembros nuevos | INV-PBTC2-04; D-PBTC2-05 |
| **I-03** (`descartes=0` no estaba en CT-04, y en las ventanas que no cierra «Actualizar» se leía antes de cerrar) | La última línea de **cada** ventana lleva ` descartes=<n>`, leído **después** de cerrar la ventana. Tras «Actualizar», que la cierra; si no, el orden exacto es: la línea sin el sufijo, `Cerrar(w)` y después `log.Add(<línea> + " descartes=" + descartes)`, como últimas sentencias del `try`. Se aplica a CT-01 (ventana 2) y a las cuatro ventanas de CT-04 | D-PBTC2-08; CT-PBTC2-01 y 04; INV-PBTC2-07 |
| **M-01** (anclas e inventario) | `SetMixed` WC:565-590; `ApplyMixedSideState` WC:445-503; `SideSelector_Changed` WC:147-180, con la llamada en WC:172; `LoadSelectedModule` W:1803-1866; el refuerzo de `ApplyTo`, `PushBackAdvancedRackParameters.cs:120-122`. Se añade `ApplyTopology_Click` (WC:215, llamada en WC:237) a los llamadores de `LoadSelectedFront`, y la salida de `LoadCompositeCellPanel` sin lado B (WC:604-607) | H2-02, H2-06, H2-11; `pbtc2-measurements.md`, I-1 a I-3 |
| **M-02** («ningún ayudante nuevo» contradecía el ayudante privado que lee el JSON) | Se admite y se fija su forma: tres miembros privados de cada clase nueva, con la navegación de SC1 y `GetValue<double>()`, `GetValue<bool>()` y `GetValue<string>()` | §4, definiciones |
| **M-03** (las extensiones declaradas de OWNER-1, sin CT; la caja mixta vacía, sin fijar) | CT-PBTC2-04 gana la ventana «cambio de celda», que confirma la caja mixta escrita al cambiar de celda. Su línea 3 imprime también `A.largo` y `B.largo`. El alcance y los datos del frente se declaran en `KnownLimitations` y van a la red de PB-T1, como M-03 de PB-TC | CT-PBTC2-04; §4.1; §6.6; §10 |
| **M-04** (D-PBTC2-07 y PBTC2-C14, sin precisar) | La «base» de cada fila es la salida real de la ventana de la base tras el gesto (`LastComputation.Design`), comparada con la fila a mano. El catálogo es `w.Session.Catalog`. Un número distinto de líneas del BOM es STOP. Se transcriben `MaxDelta` y K4'. El umbral es el de «material» de OB-06.4 (más de 0,001); el «menos de 0,0005"» que se dio al Owner era una **estimación, no un umbral** | D-PBTC2-07; PBTC2-C14; §6.6; §8 |
| **M-05** (riesgo de nc1 no declarado) | Riesgo R2-08: si nc1 repite la variación de PB-TC (Remote, Scope, Tests y Trailer en fail, además de Handoff, Identity y Ci), vuelve al Owner por PBTC2-C11, sin sustituto. El Owner la aceptó una sola vez (F4B-E08) | §7; PBTC2-C11 |
| **M-06** (una ruta local en [OPT1]) | Ya la corrigió la sesión. Este paquete no la toca | — |
| **M-07** (defecto anterior: sin lado B y con el indicador obsoleto, vaciar «Largo manual» no surte efecto y las cajas obligatorias quedan opcionales) | Arrastre al paquete de PB-T1, no a PB-TC2. La corrección de I-01 lo conserva tal cual | §10; precondición 3 |

## 1. Autoridades

| Autoridad | Qué fija |
|---|---|
| [OPT1] (`F4B-DRAFTS/coordinator/owner-pbt1-pre-decisions.md`, F4B-E09) | **Pregunta 1:** PB-TC2 antes de PB-T1, con H-31 (la regla `Keep` en las seis cajas) y OWNER-1 (la caja mixta aplica el valor escrito a los dos lados). Se informó al Owner, como **estimación**, de que con tres decimales o menos no cambia nada y con más el efecto es menor que 0,0005" y el BOM cambia en diezmilésimas, y de que pasar por la longitud de un módulo con más decimales lo vuelve manual. **Preguntas 2 y 3:** c-02 y `pb-21-cola` van a PB-T1. **Pregunta 4:** la emisión condicional |
| [OD] (`F4B-DRAFTS/coordinator/owner-f4b-disposition.md`) | D-PB-06: «rounding behavior/state», sus seis pasos (`:130-150`), «Do not hide these corrections inside PB-T1 state extraction» (`:152`) y la vuelta al Owner si una corrección altera la geometría «materially» (`:153-155`). D-PB-10 (`:233-236`) |
| [P] (`docs/initiatives/I-64-f4b-push-back-completo-package.md`, v3.1) | OB-06.2 a OB-06.6 (`:369-398`), con la definición de «material» de OB-06.4 (`:389`); OB-T1B.1 a OB-T1B.3 (`:700-710`); la fila de PB-T2 (`:504`); §9 |
| [DP] (`F4B-DRAFTS/coordinator/owner-f4b-procedure-decisions.md`, F4B-E03) | Una sola replanificación, y solo por la comparación literal (pregunta 2). Los sustitutos de nc1 y nc2, solo con la firma EXACTA de PD-F4-04 y PD-F4-01 (pregunta 1) |
| El expediente (`F4B-DRAFTS/pbt1/ob065-dossier.md`) | §2.1, §2.2 y §5, medidos en `5597c05b`. Aquí se vuelven a medir todos |
| [PKC], [PKC-C] y el cierre de PB-TC (`F4B-PBTC-DECLARED-CORRECTIONS/task-close-package.md`) | El patrón; D-PBTC-10 y PBTC-C14; las firmas literales de PBTC-C11; las lecciones M-02 a M-05 del §4 del cierre |
| El anexo A de la v2 de PB-TC (`F4B-DRAFTS/pbtc/package-v2.md:1217-1302`) e ICT2 (`F4B-DRAFTS/pbtc/independent-review-v2-check.md`) | El diseño de H-31; N-08 (la ventana editada usa M2 de `pb-31`, de 48 a 56) |
| `F4B-DRAFTS/coordinator/owner-pbtc-mixed-decision.md` y `owner-pbtc-nc1-decision.md` (F4B-E08) | OWNER-1, registrado y arreglado después (lo resuelve [OPT1]); la disposición única de nc1, que no se extiende |
| [SC] (`F4B-DRAFTS/superseding-characterization.md`) | EJECUTADA para PB-TC. Tiene el «Pendiente» de H-31 (`:175-182`) y la limitación OWNER-1 (`:209-211`), que PB-TC2 cierra |
| `F4B-DRAFTS/f4b-dispositions.md` | F4B-E01 a F4B-E09 |
| ADR-0029 D13; `docs/AUTOMATION_PLAN.md` §16.5, §16.8, §16.9 y §16.11; `docs/automation/agent-execution/README.md` §4 y §10; `routing.md`; `model-catalog.md` | «la caracterización no se edita para que pase»; A1-A8, el RED de la cadena, las 14 comprobaciones, las paradas, nc1 a nc4 y las celdas |
| Las fuentes de las firmas (`F4-DRAFTS/f4-dispositions.md:520`, `:562`, `:1103-1106`, `:1540`, `:1657-1660`; `F4-DRAFTS/coordinator/owner-f4t3-authorization.md:255-265`) | PD-F4-01 y PD-F4-04, transcritas en PBTC2-C11 |

## 2. Hechos medidos en `5597c05b`

**Alias:**
- **W** = `src/RackCad.UI/Systems/PushBack/RackPushBackSystemWindow.xaml.cs` (3690 líneas);
- **WC** = `….Composite.cs` (936);
- **WX** = `….xaml` (793);
- **CS** = `src/RackCad.Application/Systems/PushBack/PushBackCompositeEditorState.cs`;
- **S0** = `tests/RackCad.UI.Tests/PushBackFullEditorCharacterizationSupport.cs`;
- **SC1** = `tests/RackCad.UI.Tests/PushBackDeclaredCorrectionSupport.cs`.

Todo `ruta:línea` es de HEAD. Los ficheros usan final de línea LF.

### 2.1 H-31: las seis cajas

- **H2-01. El hueco.**
  - **Siembra:** `GapBox.SetNumber(composite.Gap)` con «0.###», en cada pasada válida (`RefreshCompositePanel`, WC:738-775, con la siembra en WC:746, bajo `suppressSync`).
  - **Lectura:** `Gap_Changed` (WC:184-200), en `LostFocus` (WX:148). Sale si `suppressSync` (WC:186) o si la caja tiene error o está vacía (WC:193), y después hace `composite.SetGap(GapBox.Value.Value)` **sin comparar** (WC:198).
  - **El hueco gobierna el módulo `GAP`:** el estado carga `composite.Gap` (CS:991), y el resolver arma el módulo `GAP` desde el hueco (IRT2-1 §2.3).
  - No está en `AllNumericFields` (W:749-755).
- **H2-02. La longitud del módulo.**
  - **Siembra:** `ModuleLengthBox.SetNumber(module.Length)` (W:1833), dentro de `LoadSelectedModule` (W:1803-1866), bajo `suppressSync`.
  - **Lectura:** `ModuleLength_Changed` (W:1892-1913), en `LostFocus` (WX:554):
    - `var length = Val(ModuleLengthBox, module.Length);` (W:1905);
    - sale si `length <= 0.0 || Math.Abs(length - module.Length) < 0.0001` (W:1906);
    - si no sale, `moduleOwner.ModuleSession.SetLength(module.ModuleId, length);` (W:1911).
  - **`SetLength` vuelve manual el módulo:** `IsManualOverride = true`, `IsCalculated = false` (`RackModuleEditSession.cs:185-203`), y deja un cambio pendiente (`:162-167`). «Actualizar» lo confirma (W:3386-3389).
  - **Las longitudes guardadas se conservan:** el resolver copia los módulos guardados (`DynamicRackSystemResolver.cs:157-172`), y no hay reconstrucción. Por eso un módulo calculado de 47,99213 (el caso del fondo de 1219 mm, expediente §2.1) llega a la caja como «47.992». Como |47,992 − 47,99213| = 0,00013 ≥ 0,0001, pasar por la caja lo deja manual de 47,992.
- **H2-03. Las cuatro avanzadas anulables.**
  - **Siembra:** `LoadAdvancedRackParameters` (W:1595-1619), bajo `suppressSync`. Las numéricas van con `SetOptional` = `SetNumber(valor)` (W:1622-1629).
  - **Lectura:** `ReadAdvancedRackParameters` (W:1632-1647) relee las seis en cualquier `LostFocus` de las cinco cajas o en el `Click` de la casilla (WX:492, :499, :505-507, :510, :524 y :531; `AdvancedRackParameter_Changed`, W:1656-1666).
  - **Las anulables con decimales** son W:1639, :1641, :1642 y :1646. `SeparatorCountOverride` es entero (W:1643-1645) y `DerivedPostReinforced`, una casilla (W:1640).
  - **El portador es `advanced`:** se siembra en la carga (W:406-411), y los dos «Restaurar» reescriben a la vez el portador y las cajas (W:700-701, W:1670-1671).
  - **Una caja vacía** da `Value` nulo, que es «el cálculo». No son opcionales y no están en `AllNumericFields`: marcan `EmptyRequired`, pero la pasada sigue. Es lo de hoy, y no cambia.
  - **El refuerzo** se escribe solo con `DerivedPostReinforced` (`PushBackAdvancedRackParameters.cs:120-122`), y no puede superar la altura resuelta del poste (`:85-105`).
- **H2-04. `Val`.** Su único uso es W:1905; se declara en W:3634. Dos comentarios lo citan con `cref`, en W:3641 y W:3683. `git grep -nP '\bVal\(' -- <W>` da hoy exactamente W:1905 y W:3634 (medición I-4).
- **H2-05. Los comparadores de PB-TC:** `OptionalVal` (W:3665-3668), `ShownFormat` (W:3671), `SameShown` (W:3674-3678), `Keep` (W:3685-3686) e `IntVal` (W:3688). `Keep` es accesible desde WC, que es la misma clase parcial.

### 2.2 OWNER-1: «Ambos lados» y el indicador mixto

- **H2-06. Quién pone y quita el indicador.**
  - `mixedNow` solo cambia en `SetMixed` (WC:565-590): lo pone en WC:579 y lo quita en WC:588.
  - `SetMixed` solo la llama `ApplyMixedSideState` (WC:445-503), y solo con `mixed = composite.ActiveSelection == PushBackSideSelection.Both && composite.SideBPresent` (WC:452).
  - `ApplyMixedSideState` solo la llama `LoadCompositeCellPanel` (WC:602-640, llamada en WC:634).
  - **`LoadCompositeCellPanel` sale al principio sin lado B:** `if (SlotPresentText == null || !composite.SideBPresent) return;` (WC:604-607).
  - `LoadCompositeCellPanel` solo la llama `LoadSelectedFront` (W:489). Los llamadores de `LoadSelectedFront` son:
    - la carga (W:431);
    - `SelectMatrixCell` (W:1096);
    - `SelectSingleCell` (W:1201);
    - `MutateStructure` (W:1221);
    - `ApplyScope` (W:1351);
    - el desplegable de frente (W:1464);
    - `ApplyFrontData` (W:1515);
    - `ApplyTopology_Click` (WC:215, llamada en WC:237);
    - `SideSelector_Changed` (WC:147-180, llamada en WC:172).

  `Input_Changed` (W:1240) es `RequestRecompute()`, y no la llama.
- **H2-07. Con B presente y «Ambos», el indicador sigue en pie cuando corre `ReadCellValues`.**
  - El camino es: salir de la caja → `Input_Changed` → `RecomputeGate.Request`, que es síncrono → `Recompute` → `CommitCurrentCell` (W:758-762) → `ForEachEditedSide` (W:774-788) → `ReadCellValues(lado)` (W:802-831).
  - Nada de ese camino llama a `SetMixed`. Con B presente y «Ambos», una caja marcada solo puede estar vacía o llevar lo escrito, porque toda siembra (W:467-476) va seguida de `ApplyMixedSideState`.
  - Cambiar de celda (`SelectMatrixCell`, W:1080-1100) **confirma primero** la celda actual (W:1094) y después siembra la nueva (W:1096).
- **H2-08. La lectura de hoy.**
  - Las cinco cajas con decimales de la celda: `Keep(caja, valor del lado)` (W:818-822). El largo manual: `OptionalVal` (W:825).
  - Con A = 61,23456 y B = 60, la caja mixta y «61.235» escrito, A conserva 61,23456 y B pasa a 61,235. La ayuda de la caja promete otra cosa: «Escribe uno para aplicarlo a A y a B» (WC:583-584).
  - Las cajas enteras ya aplican lo escrito (`IntVal`).
- **H2-11. El indicador puede quedar obsoleto sin lado B (I-01 de IRT2-1).**
  - El camino:
    1. en «Ambos», una celda tiene A ≠ B, y su caja queda marcada y vacía;
    2. se desmarca `SideBPresentCheck` (WX:127-129, eventos `Checked` y `Unchecked`). `SideBPresent_Changed` (WC:126-145) llama a `composite.SetSideBPresent(false)`, que pone `ActiveSelection = A` (CS:294-302). `SideSelectorBox.SelectedIndex = 0` sale en WC:157-160, porque la selección ya es A. Nada llama a `ApplyMixedSideState`;
    3. un clic en una celda (`SelectMatrixCell`) siembra la caja con «0.###» (W:470), y `LoadCompositeCellPanel` vuelve a salir (WC:604-607). **El indicador sigue puesto, con un valor sembrado y redondeado.** La edición alcanza solo a A (`EditTargets`, CS:182-190).
  - **Hoy no pasa nada,** porque `Keep` conserva el exacto. Pero una regla que mirara solo `IsMixed` escribiría el redondeo.

### 2.3 Las guardas y las pruebas existentes

- **H2-09. Las guardas:**
  - ninguna cita `Val(`, `Keep(`, `OptionalVal`, `ReadCellValues`, `IsMixed`, `mixedNow`, `KeepOptional`, `KeepCell` ni `MixedBoth`;
  - Fact3 (`tests/RackCad.Tests/PushBackModuleEditorCharacterizationTests.cs:381-401`) prohíbe `forceRebuild:` y `forceRebuild)` en W;
  - T-SG prohíbe sus nueve tokens en `Systems/PushBack`;
  - G-10 exige una sola `.RequestUpdate(` en W;
  - I-55 y el desviador miran W:3135, :3224, :3044 y :3048, que PB-TC2 no toca.

  Como WX no cambia, T-KB, la adopción del shell y Fact1 no lo ven.
- **H2-10. Las pruebas que tocan las seis cajas o «Ambos lados»** (mediciones E-2 a E-5) hacen solo cinco cosas:
  - salir de la caja sin editar;
  - escribir el valor que ya tiene la caja (0 o un hueco entero);
  - escribir valores que se ven distintos;
  - escribir con el portador nulo;
  - en una caja mixta, escribir enteros.

  La única que pisa el camino de H2-11 (`PushBackCompositeLifecycleWiringTests.cs:126`, que retira el lado B) lo hace con valores enteros. **Sustituciones: 0.**

## 3. Decisiones del paquete

### D-PBTC2-01. El hueco

- En WC:198, `            composite.SetGap(GapBox.Value.Value);` pasa a `            composite.SetGap(Keep(GapBox, composite.Gap));`.
- La guarda (WC:193) y la siembra (WC:746) no cambian.

### D-PBTC2-02. La longitud del módulo

- En W:1905, `            var length = Val(ModuleLengthBox, module.Length);` pasa a `            var length = Keep(ModuleLengthBox, module.Length);`.
- W:1906 y W:1911 no cambian.
- **El resultado:**
  - pasar por la caja sin editar no escenifica nada, así que un módulo calculado sigue calculado;
  - escribir un número que se ve distinto escenifica la longitud y el módulo pasa a manual, como hoy;
  - **efecto declarado:** escribir un número que se ve igual al actual tampoco escenifica nada. Es R-03 (i) (F4B-E06).

### D-PBTC2-03. Las cuatro anulables

- W:1639 pasa a `            advanced.ManualHeaderHeightOverride = KeepOptional(RackHeaderHeightBox, advanced.ManualHeaderHeightOverride);`
- W:1641 pasa a `            advanced.DerivedPostReinforcementHeight = KeepOptional(DerivedPostReinforcementHeightBox, advanced.DerivedPostReinforcementHeight);`
- W:1642 pasa a `            advanced.DerivedPostHeight = KeepOptional(DerivedPostHeightBox, advanced.DerivedPostHeight);`
- W:1646 pasa a `            advanced.SeparatorSpacingOverride = KeepOptional(RackSeparatorSpacingBox, advanced.SeparatorSpacingOverride);`
- W:1640 y W:1643-1645 no cambian.

### D-PBTC2-04. La caja mixta de «Ambos lados», con el lado B presente (OWNER-1, I-01)

- **La condición** es la misma con la que hoy se pinta el estado mixto (WC:452). Va en un miembro nuevo, `MixedBoth`: el campo está marcado **y** «Ambos lados» está activo **y** el lado B está presente.
- **En `ReadCellValues`** (W:818-822), las cinco llamadas `Keep(<caja>, …)` pasan a `KeepCell(<caja>, …)`, con el mismo segundo argumento. Por ejemplo, W:818 pasa a `                    FirstLevelHeight = KeepCell(FirstLevelHeightBox, front.FirstLevelHeight),`, y lo mismo en W:819 a W:822.
- **En `OptionalVal`** solo cambia W:3667, que pasa a `                ? (MixedBoth(field) ? field.Value : KeepOptional(field, sideValue))`. Las líneas W:3665, W:3666 y W:3668 quedan **literalmente** como hoy. Así, la rama de la caja vacía, `: (IsMixed(field) ? sideValue : null);`, no cambia, y el defecto anterior M-07 se conserva (§10).
- **El resultado:**
  - con B presente y «Ambos», una caja mixta que recibe un valor lo aplica a cada lado que se escribe;
  - una caja mixta vacía deja a cada lado el suyo, como hoy;
  - **en cualquier otro caso, `KeepCell` es `Keep` y `OptionalVal` con valor es la regla de hoy.** Eso incluye un indicador obsoleto sin lado B (H2-11): `MixedBoth` es falso y la caja sembrada conserva el exacto.
- **El alcance:** todo lo que lee la celda con `ReadCellValues` con B presente y «Ambos». Es decir, la edición, el cambio de celda (CT-PBTC2-04), el alcance (W:1346) y los datos del frente (W:1510). Los dos últimos no tienen CT (§10).
- **No cambian:** las cajas enteras ni los desplegables.

### D-PBTC2-05. Los miembros nuevos, y `Val`

- **Los miembros nuevos** se insertan inmediatamente después de `Keep` (W:3686) y antes de `IntVal` (W:3688), en este orden y con este texto, cada uno precedido de una línea en blanco:

  ```csharp
          private static double? KeepOptional(NumericField field, double? current)
              => !field.Value.HasValue ? null : (current.HasValue && SameShown(field.Value.Value, current.Value) ? current : field.Value);

          private bool MixedBoth(NumericField field)
              => IsMixed(field) && composite.ActiveSelection == PushBackSideSelection.Both && composite.SideBPresent;

          private double KeepCell(NumericField field, double current)
              => MixedBoth(field) && field.Value.HasValue ? field.Value.Value : Keep(field, current);
  ```

  - Cada uno puede llevar delante un `/// <summary>` de una línea. Ese comentario no puede contener `Val(`, `Keep(`, `KeepOptional(`, `KeepCell(`, `MixedBoth(` ni `IsMixed(`.
  - `KeepOptional` hace lo mismo que la rama con valor de hoy de `OptionalVal`: vacía = nulo; si se ve igual al portador, el portador; si no, la caja.
- **`Val` se retira:** se quitan W:3634 y la línea en blanco que la sigue (W:3635).
- **Sus dos `cref` se corrigen:**
  - en W:3641, `(<see cref="Val"/>)` pasa a `(hoy <see cref="Keep"/>)`;
  - en W:3683, `, como el respaldo de <see cref="Val"/>.` pasa a `.`.

### D-PBTC2-06. Lo que no cambia

- `NumericField`, Cantilever, Application, el Plugin y WX;
- las siembras, las validaciones y los manejadores del XAML;
- la casilla y el entero avanzados;
- D-PB-11 (W:696-703) y las demás cajas de PB-TC.

Ninguna línea con `SetNumber(` se añade ni se quita.

### D-PBTC2-07. La medición en la cadena del efecto no medido (OB-06.4; M-04)

El efecto de la corrección en la salida no está medido: el hueco exacto, el módulo que sigue calculado y las cuatro avanzadas exactas. **El umbral es la definición de «material» de OB-06.4** ([P]:389): una coordenada que se mueve más de 0,001, o una diferencia en piezas, niveles o BOM. El «menos de 0,0005" y el BOM en diezmilésimas» que se dio al Owner ([OPT1], pregunta 1) era una **estimación, no un umbral**.

- **Paso 1 del Worker, sobre `BaseSha` y antes del RED.** Es una corrida temporal, que no entra en ningún commit. Sus cambios temporales son dos:
  1. un método temporal en `PushBackDeclaredCorrection2RoundingTests.cs`. Por fila, abre dos ventanas sobre la base con el diseño exacto (`EditInner`):
     - **la salida de la base tras el gesto** (`R`): se hace el gesto de la fila y se lee `LastComputation.Design`. Primero se compara con la fila hecha a mano de la tabla, y si difiere, es PBTC2-C07;
     - **lo que escribirá el GREEN** (`E`): `LastComputation.Design` tras la carga, sin gesto, que en la base ya conserva el exacto (PB-TC).

     Resuelve los dos con `BuildFrom` y el catálogo `w.Session.Catalog`, y calcula K1, K1o, K2, K3, K4 y K4', y `MaxDelta`, con la fórmula de CT-PB0-21 (`tests/RackCad.Tests/PushBackFullEditorCharacterizationRoundingEffectTests.cs:107-245`, «material» en `:164`), copiada. Si K3 es cierto, compara el BOM línea a línea en el orden producido. **Un número distinto de líneas es STOP;** con el mismo número, anota cada propiedad que difiere;
  2. termina en `Assert.Fail(string.Join("\n", lineas))`.

  **Las filas:**

  | Fila | Diseño | Gesto en la base | Fila a mano de la base | GREEN |
  |---|---|---|---|---|
  | T2-1 hueco | `pb-20-ab`, `Composite.Gap = 54.00044` | `LostFocus` en `GapBox` | 54 | 54,00044 |
  | T2-2 módulo | `pb-31-v2-modulos`, `Structure.Modules[1].Length = 47.99213` | `SelectModule(w, "M2")`, `LostFocus` en `ModuleLengthBox` y `Click(w, "ConfirmModuleButton")` | M2 47,992, `IsManualOverride` true, `IsCalculated` false | M2 47,99213, calculado |
  | T2-3 cabecera | `pb-06-seguridad`, `ManualHeaderHeightOverride = 150.00044` | `LostFocus` en `RackSeparatorCountBox` | 150 | 150,00044 |
  | T2-4 separación | `pb-06-seguridad`, `SeparatorSpacingOverride = 18.00044` | igual | 18 | 18,00044 |
  | T2-5 poste derivado | `pb-07-combinado`, `DerivedPostHeight = 140.00044` | `LostFocus` en `DerivedPostHeightBox` | 140 | 140,00044 |
  | T2-6 refuerzo | `pb-07-combinado`, `DerivedPostReinforcementHeight = 24.00044` | igual | 24 | 24,00044 |

- **La transcripción:** K1 a K4, K4', `MaxDelta` y el detalle del BOM, fila por fila, en `worker-handoff.json`. El TRX y el log van como artefactos, con rutas relativas comprobadas.
- **Cuándo se para (PBTC2-C14):** si alguna fila cumple una de estas condiciones:
  - K1 o K1o no vacías;
  - K2 cierto;
  - K4 no vacía (más de 0,001);
  - un número distinto de líneas del BOM;
  - una línea del BOM que difiere en algo que no es un número con |Δ| < 0,001.

  Si ninguna para, pero `MaxDelta` ≥ 0,0005 en alguna fila, **no es una parada**: el Worker lo declara y la sesión informa al Owner con el dato, porque la estimación que se le dio queda superada.
- **OWNER-1 no entra en esta medición:** es la igualación que el Owner pidió.

### D-PBTC2-08. Las pruebas

- **Las ventanas:** `var w = Ventana(); try { Cargar(w, t); … } finally { Cerrar(w); }`, con la carga dentro del `try`. Ninguna se muestra.
- **M-05 e I-03:**
  - cada método corre entero dentro de `using (EditorDiscardPrompt.Substitute(_ => { descartes++; return true; }))`, con un contador `int descartes = 0;` por método;
  - **la última línea de cada ventana** lleva el sufijo ` descartes=<n>`, leído **después** de que la ventana esté cerrada:
    - en las ventanas que cierra «Actualizar», se lee tras `Click(w, "UpdateButton")`;
    - en las demás, las tres últimas sentencias del `try` son: el texto de la línea sin el sufijo, en una variable; `Cerrar(w);`; y `log.Add(<texto> + " descartes=" + descartes);`.

    El `Cerrar` del `finally` es idempotente (S0 `:71-77`).
- **Los gestos:** sin teclado ni foco real. Se usan `SetNumberR` (S0); `EditorWindowTestSupport.SetText` seguido de `LostFocus` elevado, o sin él cuando lo dice el CT; `IsChecked` y `SelectedIndex` asignados; `SelectModule` (S0); `Click`; y `SelectMatrixCell`.
- **Los diseños:** con `EditInner` y `Poner` (SC1) sobre el corpus. Ningún fixture nuevo en `Fixtures/I64`.

## 4. Las pruebas nuevas (lista cerrada: CT-PBTC2-01 a CT-PBTC2-04)

**Definiciones:**
- **Las clases:** `PushBackDeclaredCorrection2RoundingTests` (CT-01 a 03) y `PushBackDeclaredCorrection2MixedBoxTests` (CT-04), en `tests/RackCad.UI.Tests/`.
  - Cada una es `public sealed class`, con `namespace` `RackCad.UI.Tests` y `[Collection(EditorDiscardPromptCollection.Name)]`.
  - Los métodos son `[Fact]`, sin `Theory`.
  - Cada método arma una lista de líneas y termina en **un** `Assert.Equal(<lista esperada>, <lista>)`.
- **Los ayudantes (M-02):** los de S0 y SC1 y `EditorWindowTestSupport.SetText`, solo de uso. Además, en **cada** clase nueva, tres miembros privados estáticos que leen el JSON persistido bajo `PushBack`, con la misma navegación de puntos e índices que SC1 (`:91-108`), copiada:
  - `NumR(json, ruta)`: `GetValue<double>()` con «R», o `<null>`;
  - `BoolR(json, ruta)`: `GetValue<bool>()` con `bool.ToString()`, o `<null>`;
  - `StrR(json, ruta)`: `GetValue<string>()`, o `<null>`.

  No hay ningún otro ayudante.
- **Los formatos:**
  - los números van con «R», y `<null>` es el nulo;
  - `texto=<t>` es el texto actual de la caja, y en CT-04 va entre corchetes, `texto=[<t>]`;
  - `<caja>=<v>` es lo escrito;
  - los booleanos se escriben `True` y `False`;
  - el sufijo ` descartes=<n>` sigue la regla de D-PBTC2-08.
- **La cultura:** la invariante, por `Sta`.

**CT-PBTC2-01. El hueco sin editar conserva el exacto hasta RACKEDITAR.** `CT_PBTC2_01_AnUneditedGapBoxKeepsTheExactGapUpToTheUpdate`.
- **Diseño:** `pb-20-ab` con `Composite.Gap = 54.00044`.
- **Ventana 1:**
  1. `carga Gap=54.00044`;
  2. `GapBox texto=54`;
  3. tras `LostFocus` en `GapBox`, sin editar: `GapBox sin editar Gap=54.00044`;
  4. tras `Click(w, "UpdateButton")`, leyendo `Persist(w.DesignToInsert)`: `peticion Gap=54.00044 descartes=0`.
- **Ventana 2:** `SetNumberR(w, "GapBox", 60)`; después la línea sin el sufijo, `Cerrar(w)` y el sufijo: `GapBox=60 Gap=60 descartes=0`.
- **En la base:** la línea 3 da `Gap=54`. Falla por aserción.

**CT-PBTC2-02. Pasar por la longitud de un módulo calculado no lo vuelve manual; escribir otra sí.** `CT_PBTC2_02_PassingThroughTheModuleLengthKeepsACalculatedModuleAndAnEditMakesItManual`.
- **Diseño:** `pb-31-v2-modulos` con `Structure.Modules[1].Length = 47.99213`. Es M2 `Separator`, calculado y no manual.
- **`M2`** = `StrR(… "Structure.Modules[1].ModuleId")`, `NumR(… ".Length")`, `BoolR(… ".IsCalculated")` y `BoolR(… ".IsManualOverride")`.
- **Ventana 1:**
  1. `Cargar` y `SelectModule(w, "M2")`: `M2 texto=47.992`;
  2. tras `LostFocus` en `ModuleLengthBox`, sin editar: `sin editar pendiente=False`, con `pendiente` = `w.PendingWork().HasPendingWork`;
  3. tras «Actualizar»: `sin editar peticion M2 Length=47.99213 IsCalculated=True IsManualOverride=False descartes=0`.
- **Ventana 2:**
  1. `SelectModule(w, "M2")` y `SetNumberR(w, "ModuleLengthBox", 56)`: `editado pendiente=True`;
  2. tras «Actualizar»: `editado peticion M2 Length=56 IsCalculated=False IsManualOverride=True descartes=0`.
- **En la base:** la línea 2 de la ventana 1 da `pendiente=True`. Falla por aserción.

**CT-PBTC2-03. Las cuatro anulables sin editar conservan el exacto; vaciar una sigue siendo el cálculo.** `CT_PBTC2_03_UneditedNullableAdvancedBoxesKeepTheExactValueAndAnEmptyBoxIsTheCalculation`.
- En cada ventana, antes de los gestos: `Check(w, "AdvancedModulesToggle").IsChecked = true`.
- **Ventana `pb-06-seguridad`,** con `ManualHeaderHeightOverride = 150.00044` y `SeparatorSpacingOverride = 18.00044`:
  1. `pb-06-seguridad carga ManualHeaderHeightOverride=150.00044 SeparatorSpacingOverride=18.00044`;
  2. `pb-06-seguridad textos RackHeaderHeightBox=150 RackSeparatorSpacingBox=18`;
  3. tras `LostFocus` en `RackSeparatorCountBox`: `pb-06-seguridad sin editar ManualHeaderHeightOverride=150.00044 SeparatorSpacingOverride=18.00044`;
  4. tras `SetNumberR(w, "RackSeparatorSpacingBox", 21.5)`: `pb-06-seguridad RackSeparatorSpacingBox=21.5 ManualHeaderHeightOverride=150.00044 SeparatorSpacingOverride=21.5`;
  5. tras `SetText(w, "RackHeaderHeightBox", "")` y `LostFocus`: `pb-06-seguridad RackHeaderHeightBox vacia ManualHeaderHeightOverride=<null> SeparatorSpacingOverride=21.5`;
  6. tras «Actualizar»: `pb-06-seguridad peticion ManualHeaderHeightOverride=<null> SeparatorSpacingOverride=21.5 SeparatorCountOverride=3 descartes=0`.
- **Ventana `pb-07-combinado`,** con `DerivedPostHeight = 140.00044` y `DerivedPostReinforcementHeight = 24.00044`:
  1. `pb-07-combinado carga DerivedPostHeight=140.00044 DerivedPostReinforcementHeight=24.00044`;
  2. `pb-07-combinado textos DerivedPostHeightBox=140 DerivedPostReinforcementHeightBox=24`;
  3. tras `LostFocus` en `DerivedPostHeightBox`: `pb-07-combinado sin editar DerivedPostHeight=140.00044 DerivedPostReinforcementHeight=24.00044`;
  4. tras «Actualizar»: `pb-07-combinado peticion DerivedPostHeight=140.00044 DerivedPostReinforcementHeight=24.00044 descartes=0`.
- **En la base:** la línea 3 de `pb-06` da 150 y 18. Falla por aserción.

**CT-PBTC2-04. En «Ambos lados» con B presente, una caja mixta que recibe un valor lo aplica a los dos lados; sin «Ambos» o sin B, conserva el exacto.** `PushBackDeclaredCorrection2MixedBoxTests.CT_PBTC2_04_AMixedBoxThatReceivesAValueAppliesItToBothSidesAndANonMixedBoxKeepsTheExactValue`.
- **Notación:**
  - `A.alto` = `Structure.Fronts[0].Levels[0].PalletHeight` y `B.alto` = `SideB.Fronts[0].Levels[0].PalletHeight`;
  - `A.largo` y `B.largo` son lo mismo con `BeamLengthOverride`;
  - todos se leen de `Salida` con `NumR`;
  - «Ambos» = `Combo(w, "SideSelectorBox").SelectedIndex = 2`;
  - `seleccion(<f>,<l>)` = `w.State.Structure.SelectedFrontIndex` y `w.State.Structure.SelectedLevelIndex`, leídos en línea en ese punto (N-01 de la comprobación acotada; `State` ya lo usan las pruebas, `BlankFrontEditorTests.cs:333`). No es un ayudante nuevo.
- **Diseño D-mixto:** `pb-20-ab` con `A.alto = 61.23456`, `A.largo = 100.00044` y `B.largo = 120`. `B.alto` queda en 60.
- **Ventana mixta** (D-mixto):
  1. `mixta carga A.alto=61.23456 B.alto=60 A.largo=100.00044 B.largo=120`;
  2. tras «Ambos»: `mixta Ambos CellPalletHeightBox texto=[] CellBeamLengthOverrideBox texto=[]`;
  3. tras `SetText(w, "CellPalletHeightBox", "61.235")` y `LostFocus`: `mixta CellPalletHeightBox=61.235 A.alto=61.235 B.alto=61.235 A.largo=100.00044 B.largo=120`;
  4. tras `SetText(w, "CellBeamLengthOverrideBox", "100")` y `LostFocus`; después la línea sin el sufijo, `Cerrar(w)` y el sufijo: `mixta CellBeamLengthOverrideBox=100 A.largo=100 B.largo=100 descartes=0`.
- **Ventana no mixta** (`pb-20-ab` con `A.alto = 61.23456` y `B.alto = 61.23456`):
  1. `no mixta carga A.alto=61.23456 B.alto=61.23456`;
  2. tras «Ambos»: `no mixta Ambos CellPalletHeightBox texto=[61.235]`;
  3. tras `SetText(w, "CellPalletHeightBox", "61.2350")` y `LostFocus`; después la línea, `Cerrar(w)` y el sufijo: `no mixta CellPalletHeightBox texto=[61.2350] A.alto=61.23456 B.alto=61.23456 descartes=0`.
- **Ventana sin lado B** (D-mixto; el camino de I-01, H2-11):
  1. `sin lado B carga A.alto=61.23456 B.alto=60`;
  2. tras «Ambos»: `sin lado B Ambos CellPalletHeightBox texto=[]`;
  3. tras `Check(w, "SideBPresentCheck").IsChecked = false` y `w.SelectMatrixCell(0, 0, false)`; después la línea, `Cerrar(w)` y el sufijo: `sin lado B CellPalletHeightBox texto=[61.235] A.alto=61.23456 descartes=0`.
- **Ventana cambio de celda** (D-mixto; la extensión de OWNER-1 al cambio de celda, M-03):
  1. `cambio de celda carga A.alto=61.23456 B.alto=60`;
  2. tras «Ambos»: `cambio de celda Ambos CellPalletHeightBox texto=[]`;
  3. tras `SetText(w, "CellPalletHeightBox", "61.235")`, **sin** `LostFocus`, y `w.SelectMatrixCell(0, 1, false)`, que confirma primero la celda actual (H2-07); después la línea, `Cerrar(w)` y el sufijo: `cambio de celda CellPalletHeightBox=61.235 seleccion(0,1) A.alto=61.235 B.alto=61.235 descartes=0`.
- **En la base:** la línea 3 de la ventana mixta da `A.alto=61.23456 B.alto=61.235`. Falla por aserción.
- **Las ventanas no mixta y sin lado B pasan también en la base.**
- **No es tautológica:**
  - un producto que aplique siempre lo escrito en «Ambos» falla en la ventana no mixta;
  - uno que mire solo `IsMixed` (el diseño de la v1) falla en la ventana sin lado B;
  - uno que mantenga `Keep` en la caja mixta falla en la ventana mixta y en la de cambio de celda.

### 4.1 Trazabilidad

| Obligación | CT |
|---|---|
| D-PB-06, paso 2 | 01 (el hueco), 02 (el módulo), 03 (las avanzadas) y 04 (OWNER-1) |
| D-PB-06, paso 3 | El GREEN de W y WC; INV-PBTC2-02 a 04 sobre el diff |
| D-PB-06, paso 4, en RACKEDITAR | 01, 02 y 03, con «Actualizar». El 04 lee `Salida` |
| D-PB-06, paso 4, en el Workspace | Pendiente de PB-T2 (§5.3) |
| D-PB-06, pasos 5 y 6 | Sin sustituciones; la sección de PB-TC2 en [SC] (§5.3) |
| OB-06.4 | D-PBTC2-07 y PBTC2-C14 |
| I-01 (la condición de B presente) | 04, ventana sin lado B |
| M-03: los casos negativos y las extensiones | 02, ventana 2; 03, líneas 4 y 5; 04, ventanas no mixta, sin lado B y cambio de celda. **El alcance y los datos del frente** con «Ambos»: sin CT, en `KnownLimitations` y en la red de PB-T1 (§10) |
| M-05 e I-03 | El sufijo ` descartes=<n>`, leído tras cerrar, en la última línea de cada ventana |

**Mapa CT → método:**

| CT | Clase | Método |
|---|---|---|
| 01 | `PushBackDeclaredCorrection2RoundingTests` | `CT_PBTC2_01_AnUneditedGapBoxKeepsTheExactGapUpToTheUpdate` |
| 02 | `PushBackDeclaredCorrection2RoundingTests` | `CT_PBTC2_02_PassingThroughTheModuleLengthKeepsACalculatedModuleAndAnEditMakesItManual` |
| 03 | `PushBackDeclaredCorrection2RoundingTests` | `CT_PBTC2_03_UneditedNullableAdvancedBoxesKeepTheExactValueAndAnEmptyBoxIsTheCalculation` |
| 04 | `PushBackDeclaredCorrection2MixedBoxTests` | `CT_PBTC2_04_AMixedBoxThatReceivesAValueAppliesItToBothSidesAndANonMixedBoxKeepsTheExactValue` |

## 5. El RED, las sustituciones y el registro

### 5.1 El RED

- **El RED:** solo los dos archivos nuevos, sin ningún archivo de `src/`. Los cuatro métodos fallan **por aserción** en la base, en la línea que dice cada CT.
  - Ninguno lanza antes: las cargas, `SelectModule`, `SetNumberR`, `SetText`, el selector de lado, la casilla del lado B, `SelectMatrixCell` y «Actualizar» ya pasan en la CI de HEAD (CT-PB0-03, CT-PB0-05, CT-PB0-07, CT-PB0-20, CT-PBTC-03, CT-PBTC-14 y `PushBackBothModeCellBeamOverrideTests`).
- **El GREEN:** solo W y WC.
- **Core no tiene RED.**

### 5.2 Las sustituciones

**Ninguna** (H2-10). Una sustitución que resultara necesaria sería PBTC2-C02, con el nombre `SUS2-0n` y el patrón de I-39.

### 5.3 El registro declarado

- **El texto de la sección de PB-TC2,** en estado DECLARADA, es `superseding-section.md`, que acompaña a este paquete. La sesión la añade a [SC] en el commit de emisión y la pasa a EJECUTADA al cerrar, con el RED, el GREEN y sus CI.
- **Sus dos notas de cierre:** el «Pendiente» de H-31 (`:175-182`) y la limitación OWNER-1 (`:209-211`), con F4B-E09 y PB-TC2.
- **La obligación de PB-T2:** probar las correcciones de PB-TC2 en el modo WORKSPACE (OB-06.3), anotada también en `f4b-dispositions.md`.

## 6. Contrato propuesto

### 6.1 Identidad

- `Schema` `rackcad-gate-contract/v1`; `Unit` I-64; `Gate` F4B; `TaskId` **`F4B-PBTC2-DECLARED-CORRECTIONS`**.
- Evidencia en `docs/automation/evidence/I-64-pilot/F4B-PBTC2-DECLARED-CORRECTIONS/`.
- Attempts 1 de 3; `RoutingEnforcement` `required`; `CorrectionsAuthorized` true.
- **Objective** (ASCII): «Corregir en la ventana clasica de Push Back, como cambios declarados de RACKEDITAR por D-PB-06 y la decision del Owner F4B-E09, las cajas que aun redondean (H-31: GapBox, ModuleLengthBox y las cuatro cajas avanzadas anulables, con la regla Keep de PB-TC y KeepOptional) y la caja mixta de 'Ambos lados' con el lado B presente (OWNER-1: una caja mixta que recibe un valor lo aplica a los dos lados), segun el paquete del contrato de PB-TC2 v2 y la lista cerrada CT-PBTC2-01..04. El RED son pruebas nuevas que fallan por asercion en la base, sin sustituciones; el GREEN corrige solo W y WC. NumericField, Cantilever, Application, el Plugin y el XAML no cambian; c-02 y pb-21-cola quedan para PB-T1; el modo Workspace queda para PB-T2; no se ejecuta AutoCAD.»

### 6.2 `AllowedWriteScope` (4 rutas exactas)

1. `src/RackCad.UI/Systems/PushBack/RackPushBackSystemWindow.xaml.cs`
2. `src/RackCad.UI/Systems/PushBack/RackPushBackSystemWindow.Composite.cs`
3. `tests/RackCad.UI.Tests/PushBackDeclaredCorrection2RoundingTests.cs`
4. `tests/RackCad.UI.Tests/PushBackDeclaredCorrection2MixedBoxTests.cs`

### 6.3 `ForbiddenWriteScope` (129 entradas)

Con el método de [PKC-C], hermano por hermano: un prefijo solo si ninguna ruta permitida queda debajo. Lo calcula `make_pbtc2_contract.py` con `assert` sobre los recuentos, la forma, la existencia en HEAD, la cobertura de `src/` y la no existencia de las rutas nuevas.

- **`src/`, 59:**
  - los 3 proyectos;
  - en `src/RackCad.UI/`, las 9 carpetas y los 34 archivos de la raíz;
  - 5 sistemas;
  - en `Systems/PushBack/`, los 7 hermanos de la ventana **y WX**.
- **`tests/`, 53:**
  - `tests/RackCad.Tests/`;
  - 51 archivos de `tests/RackCad.UI.Tests/`:
    - 6 ayudantes, incluido SC1;
    - las 5 clases de PB-TC y las 6 de PB-T0;
    - las 6 de T1B;
    - las 11 guardas de [PKC] §6.3;
    - las 12 que tocan las seis cajas o «Ambos lados»;
    - las 5 de OB-06.6;
  - `tests/RackCad.UI.Tests/Workspace/`, como un solo prefijo.
- **Raíz, 17.**

El orden exacto es la salida de `make_pbtc2_contract.py … --dry --print` (medición I-7).

### 6.4 `RequiredTests` (18 filas)

| Proyecto | Filtro (`FullyQualifiedName~RackCad.<proyecto>.`…) | `MinSelected` | `ExpectRed` |
|---|---|---|---|
| UI | `PushBackDeclaredCorrection2` | **4** | true |
| UI | `PushBackDeclaredCorrectionRoundingTests` | 3 | false |
| UI | `PushBackDeclaredCorrectionDimensionStyleTests` | 2 | false |
| UI | `PushBackDeclaredCorrectionPilotParityTests` | 2 | false |
| UI | `PushBackDeclaredCorrectionSideBNoticeTests` | 2 | false |
| UI | `PushBackDeclaredCorrectionStandardRestoreTests` | 1 | false |
| UI | `PushBackFullEditorCharacterization` | 16 | false |
| UI | `PushBackClassicCharacterization` | 55 | false |
| UI | `Cantilever` | 67 | false |
| UI | `PushBackModuleEditorWindowTests` | 13 | false |
| UI | `PushBackAdvancedRackParametersWindowTests` | 9 | false |
| UI | `PushBackCompositeWindowTests` | 52 | false |
| UI | `PushBackCompositeLifecycleWiringTests` | 7 | false |
| UI | `PushBackCompositeLoadExistingModulesTests` | 3 | false |
| UI | `PushBackBothModeCellBeamOverrideTests` | 10 | false |
| Core | `PushBackClassicCharacterization` | 10 | false |
| Core | `PushBackFullEditorCharacterization` | 3 | false |
| Core | `PushBackModuleEditorCharacterizationTests` | 15 | false |

- PB-TC entra **por clase**, porque su prefijo seleccionaría también las clases nuevas (medición F-4).
- Los recuentos son estáticos (F-3), y los `MinSelected` con `Skip` cuentan solo los que pasan.

### 6.5 Invariantes (14)

- **INV-PBTC2-01. Alcance.**
  - `git diff --name-only BaseSha..CurrentSha` cae dentro de las 4 rutas, y en `src/` solo W y WC.
  - `git diff BaseSha..CurrentSha -- src/RackCad.UI/Controls src/RackCad.UI/Systems/Cantilever src/RackCad.Application src/RackCad.Domain src/RackCad.Plugin src/RackCad.UI/Systems/PushBack/RackPushBackSystemWindow.xaml` está vacío.
- **INV-PBTC2-02. RED y GREEN.**
  - El RED es solo los dos archivos nuevos y se publica antes del GREEN. El GREEN es solo W y WC.
  - **Criterio del RED,** solo sobre el filtro con `ExpectRed`: todos `Failed`; `Message` sin el prefijo de tipo de [PKC-C]; la primera línea del `StackTrace` bajo `tests/`; ninguno `NotExecuted` ni `Skipped`; `Failed == Selected >= 4`.
  - En el RED, los filtros sin `ExpectRed` pasan, con los `Skipped` de la base.
  - Ninguna línea añadida ni quitada de `git diff -U0 BaseSha..CurrentSha -- <W> <WC>` contiene `SetNumber(`.
- **INV-PBTC2-03. Dónde cambia, sobre el diff.** Cada hunk de `git diff -U0 BaseSha..CurrentSha -- src` cae en uno de estos sitios:
  - WC:198 (`Gap_Changed`, WC:184-200);
  - W:1905 (`ModuleLength_Changed`, W:1892-1913);
  - W:1639, :1641, :1642 y :1646 (`ReadAdvancedRackParameters`, W:1632-1647);
  - W:818-822 (`ReadCellValues`, W:802-831);
  - W:3667 (`OptionalVal`, W:3665-3668), y W:3641, en su documentación;
  - W:3683, en la documentación de `Keep`;
  - la retirada de `Val` (W:3634-3635);
  - la inserción de `KeepOptional`, `MixedBoth` y `KeepCell` entre `Keep` (W:3686) e `IntVal` (W:3688).

  **Tocar un miembro** es cambiar o quitar una línea de su rango, o insertar dentro de él (en un hunk `@@ -a,0 …`, cuando s ≤ a < e). Una inserción entre dos miembros es una declaración nueva, y solo se admite si está en esta lista.
- **INV-PBTC2-04. Las cuentas, sobre `CurrentSha`** (I-02), con `<W>` = `src/RackCad.UI/Systems/PushBack/RackPushBackSystemWindow.xaml.cs` y `<WC>` = `src/RackCad.UI/Systems/PushBack/RackPushBackSystemWindow.Composite.cs`:

  | Orden | Resultado |
  |---|---|
  | `git grep -nP '\bVal\(' -- <W>` | **Vacío** |
  | `git grep -nF 'cref="Val"' -- <W>` | **Vacío** |
  | `git grep -cF 'Keep(GapBox, composite.Gap)' -- <WC>` | 1 |
  | `git grep -cF 'Keep(ModuleLengthBox, module.Length)' -- <W>` | 1 |
  | `git grep -cP '\bKeepOptional\(' -- <W>` | 6: la declaración, las cuatro de `ReadAdvancedRackParameters` y la de `OptionalVal` |
  | `git grep -cP '\bKeepCell\(' -- <W>` | 6: la declaración y las cinco de `ReadCellValues` |
  | `git grep -cP '\bMixedBoth\(' -- <W>` | 3: la declaración, `KeepCell` y `OptionalVal` |
  | `git grep -cP '\bIsMixed\(' -- <W>` | 2: `MixedBoth` y la rama vacía de `OptionalVal` |

  El comportamiento lo fijan CT-PBTC2-01 a 04 en verde.
- **INV-PBTC2-05. Lo que sigue en verde:**
  - PB-TC (10 métodos), PB-T0 (16 en UI y 3 en Core), T1B (55 y 10) y Cantilever (67);
  - Fact1 y Fact3, G-10, I-55, el desviador, T-SG, T-KB, el censo de ventanas y el corpus de WLC;
  - las clases del §6.4.

  W sigue sin `forceRebuild:` ni `forceRebuild)`, y con una sola `.RequestUpdate(`.
- **INV-PBTC2-06. Sin sustituciones.** Ninguna prueba existente se edita, se salta, se reactiva ni se borra.
- **INV-PBTC2-07. Las pruebas de UI** (I-03):
  - las dos clases van en `EditorDiscardPromptCollection`;
  - las ventanas, con `Ventana()`, la carga dentro del `try` y `Cerrar` en `finally`. Ninguna se muestra;
  - cada método corre dentro de `EditorDiscardPrompt.Substitute`, con un contador por método;
  - **la última línea de cada ventana termina en ` descartes=<n>`,** leído después de cerrarla: tras «Actualizar», o tras el `Cerrar(w)` explícito que es la penúltima sentencia del `try`. En todas, n = 0;
  - sin teclado ni foco real;
  - prohibidos un `MessageBox` real, el BOM, «Guardar en biblioteca», el configurador real e «Insertar»;
  - ninguna aserción dentro de una costura;
  - ningún fixture nuevo en `Fixtures/I64`;
  - los únicos ayudantes nuevos son los tres privados de cada clase (§4).
- **INV-PBTC2-08. Determinismo:** el GUID fijo, la cultura invariante, «R», el orden ordinal, y ni hora ni aleatoriedad.
- **INV-PBTC2-09. Las corridas locales.**
  - Solo los filtros del §6.4 en UI; la suite de Core, sí.
  - Sin copias de árboles.
  - Las corridas temporales no entran en ningún commit, terminan en `Assert.Fail` y guardan su TRX y su log como artefactos.
  - El perfil del usuario no cambia (INV-PBT0-06).
- **INV-PBTC2-10. CI y compilación.**
  - GREEN 4/4.
  - RED con `ui-tests` en `failure`, y `tests`, `build-ui` y `build-plugin` en `success`, sin cuelgue ni timeout.
  - Los avisos, iguales a la línea base.
- **INV-PBTC2-11. La evidencia (M-04 de PB-TC).** Toda ruta de `worker-handoff.json` es relativa y se comprueba que existe. `TreeSha` es el árbol real (`git write-tree`) o null.
- **INV-PBTC2-12. El registro.** El Worker no escribe en `docs/`. La sección de [SC] y sus notas son de la sesión.
- **INV-PBTC2-13. Las reglas de F4B:**
  - la planificación, según PBTC2-C08;
  - los controles, según PBTC2-C11;
  - antes de un intento 3, el Owner (PBTC2-C12).
- **INV-PBTC2-14. Sin AutoCAD.**

### 6.6 `ExpectedEvidence`

- **Antes del Worker:** el contrato y la delegación válidos; la comparación byte a byte, A1-A8 y nc4; el `AllowedWriteScope` igual al del contrato; el registro de una replanificación por PBTC2-C08, si la hubo.
- **Paso 1, sobre `BaseSha`** (D-PBTC2-07):
  - la corrida temporal, con `Assert.Fail`;
  - por fila de T2-1 a T2-6:
    - la salida real de la base tras el gesto, comparada con la fila a mano;
    - K1, K1o, K2, K3, K4, K4' y `MaxDelta`;
    - el detalle del BOM;
  - el TRX y el log, como artefactos.

  Si PBTC2-C14 se activa, STOP antes del RED. Si `MaxDelta` ≥ 0,0005 en alguna fila, se declara.
- **El RED y su push:** la CI del RED de INV-PBTC2-10; los TRX del filtro con `ExpectRed` cumplen INV-PBTC2-02.
- **El GREEN y su push:**
  - `git diff --name-only RedSha..CurrentSha` = W y WC;
  - la salida de las órdenes de INV-PBTC2-02 a 04;
  - la CI 4/4 con los conteos del §6.9.
- **En local:**
  - el build Release con el Plugin y sin avisos nuevos, sobre el RED y sobre el GREEN;
  - los filtros del §6.4 sobre los dos;
  - tres corridas en verde del filtro con `ExpectRed` sobre el GREEN;
  - la limpieza del perfil.
- **`worker-handoff.json`:**
  - el RED, el GREEN, el alcance, el trailer y los conteos por filtro;
  - el mapa CT → método;
  - por CT, la primera diferencia en la base;
  - la transcripción del paso 1;
  - las rutas relativas comprobadas;
  - `TreeSha` real o null;
  - `KnownLimitations`, con el alcance y los datos del frente en «Ambos lados» sin CT (§10).
- **Después:**
  - la verificación del Controller;
  - nc1 a nc3;
  - un control que no da CONTROL PASS, solo según PBTC2-C11;
  - cada uso, registrado en `f4b-dispositions.md`.

### 6.7 `StopConditions` (31)

**Las estándar,** copiadas de [PKC-C] (17): S-02, S-03, S-04, S-05, S-07, S-08, S-12, S-13 y S-14, y P-01 a P-08, con P-01 de la Snapshot I.

**Las propias (14):**
- **PBTC2-C01:** la corrección necesitaría cambiar un archivo fuera de W y WC: `NumericField`, WX, otra ventana, Application o el Plugin (OB-06.6).
- **PBTC2-C02:** habría que tocar una prueba o un ayudante existente, un `.csproj`, `InternalsVisibleTo`, los paquetes o `Fixtures/I64`. Una guarda de T1B, Fact3 o una prueba de PB-TC que no queda en verde vuelve al Owner. Una sustitución necesaria es un defecto del paquete, y vuelve a la sesión con `SUS2-0n` propuesto.
- **PBTC2-C03 (OB-06.4):** un efecto material no medido: una diferencia en el diseño, las piezas, los niveles, las posiciones o el BOM que no sale de los valores exactos de las seis cajas ni de la igualación de «Ambos lados»; o un fallo, en el GREEN, de una prueba del §6.4.
- **PBTC2-C04:** un método del filtro con `ExpectRed` pasa en la base, o falla por excepción.
- **PBTC2-C05:** un CT necesitaría AutoCAD, un `MessageBox` real, el configurador real, el BOM, «Guardar en biblioteca», «Insertar», el teclado o el foco real, o escribir en el perfil o en el disco.
- **PBTC2-C06:** la CI del GREEN falla fuera de las pruebas nuevas, se reproduce y no es atribuible a ellas.
- **PBTC2-C07:** los hechos del §2 no se sostienen de un modo que cambia un CT o el diseño; una línea esperada no se cumple en el GREEN; o, en el paso 1, la salida real de la base difiere de la fila a mano. El Worker no ajusta lo esperado.
- **PBTC2-C08:** una sola replanificación por la comparación literal ([DP], pregunta 2); cualquier otro rechazo de la planificación, al Owner.
- **PBTC2-C09:** una prueba colgaría, dejaría viva una ventana, un modal o un dispatcher, escribiría en el perfil o dependería de rutas de una máquina.
- **PBTC2-C10:** hacerlo bien exigiría tocar algo fuera del alcance: c-02, `pb-21-cola`, las demás normalizaciones de T-2, D-PB-11, M-07, las cajas de PB-TC fuera de «Ambos lados», o un miembro no listado en INV-PBTC2-03.
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
    - cualquier nc1 que no reproduzca la firma literal, **como el de PB-TC**, con Remote, Scope, Tests y Trailer también en fail (R2-08);
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
  - K4 no vacía (una coordenada que se mueve más de 0,001: la definición de «material» de OB-06.4);
  - un número distinto de líneas del BOM;
  - una línea del BOM que difiere en algo que no es un número con |Δ| < 0,001.

  El Worker se detiene **antes del RED**, entrega la transcripción y la sesión la lleva al Owner (OB-06.4; F4B-E09, «un efecto material sin medir»).

### 6.8 `EligibleCells`

Las de [PKC-C]:
- **Worker:** `anthropic/subagent/claude-sonnet-5-5`, effort `high` (VerifiedOn 2026-09-30).
- **Controller:** `openai/process/gpt-6-luna`, effort `high` (VerifiedOn 2026-11-11).
- **Vigencia:** `model-catalog.md:51` y `:97` las verifican el 2026-09-30, y con `:14` vencen el 2026-12-29. Hoy están vigentes.

### 6.9 Línea base de la CI y conteos previstos

**La línea base:** el run `44232324035`, sobre `5597c05b`, 4/4 `success`. Core: 19 070 de 19 070. UI: 2 397 Passed, 21 Skipped y 2 418 en total.

**La previsión.** Las ventanas nuevas de CT-04 no añaden métodos.

| CI | UI total | Failed | Skipped | Passed | Core |
|---|---|---|---|---|---|
| RED | 2 422 | 4 | 21 | 2 397 | 19 070 Passed |
| GREEN | 2 422 | 0 | 21 | 2 401 | 19 070 Passed |

## 7. Riesgos

| Id | Riesgo | Tratamiento |
|---|---|---|
| R2-01 | **Las líneas esperadas salen de leer el código** | PBTC2-C07 |
| R2-02 | **La validez de los valores de prueba.** IRT2-1 lo resuelve por lectura: el largo manual exige > 0, y en `pb-20-ab` una calle mide unas 50; el refuerzo de 24,00044 no supera el poste derivado de `pb-07` | PBTC2-C07 si una carga da una salida no válida |
| R2-03 | **El efecto en el dibujo y el BOM no está medido** | D-PBTC2-07 y PBTC2-C14, antes del RED |
| R2-04 | **El filtro de PB-TC por prefijo** atraparía las clases nuevas | Filtros por clase |
| R2-05 | **M-05 de PB-TC** | La costura y el sufijo `descartes`, leído tras cerrar |
| R2-06 | **Las celdas vencen el 2026-12-29** | Se comprueba en la emisión y en cada delegación |
| R2-07 | **El modo WORKSPACE no está demostrado** | Obligación de PB-T2 |
| R2-08 | **nc1 puede repetir la variación de PB-TC** (Remote, Scope, Tests y Trailer en fail, además de Handoff, Identity y Ci) con la misma celda. El Owner la aceptó una sola vez (F4B-E08, `owner-pbtc-nc1-decision.md`) | Vuelve al Owner por PBTC2-C11, sin sustituto |
| R2-09 | **Un indicador mixto obsoleto** (H2-11) | `MixedBoth` (D-PBTC2-04) y la ventana sin lado B de CT-04 |

## 8. Las decisiones del Owner

**Tomadas en F4B-E09** ([OPT1]):
- **Pregunta 1:** «Corregir en PB-TC2 (Recomendado)».
- **Preguntas 2 y 3:** c-02 y `pb-21-cola` van a PB-T1.
- **Pregunta 4:** «Sí, emite con esa condición (Recomendado)».

**Lo que se le dijo al Owner y este paquete respeta:**
- la misma regla de PB-TC, y solo en la ventana Push Back;
- «unas 5 pruebas nuevas y ninguna sustituida». Son 4 CT y 0 sustituciones, con los casos negativos dentro de cada CT;
- **«con 3 decimales o menos no cambia nada; con más, menos de 0,0005" y el BOM en diezmilésimas»** era una estimación. El paso 1 lo mide:
  - **el umbral de parada** es el de «material» de OB-06.4: más de 0,001 o una diferencia de piezas, niveles o BOM fuera de las diezmilésimas (PBTC2-C14);
  - si el efecto supera 0,0005 sin llegar al umbral, la sesión se lo dice al Owner con el dato, sin parar la cadena;
- pasar por la longitud de un módulo con más decimales lo convertía en manual, y eso se corrige.

**Decisiones materiales nuevas: ninguna.**
- **Escribir en una caja mixta** el número que se ve del exacto de un lado iguala los dos lados al número escrito. Es lo que el Owner eligió.
- **La regla de «Ambos lados» también vale** al cambiar de celda, en el alcance y en los datos del frente. Es la extensión que ya aceptó en PB-TC.
- **Una longitud de módulo que se ve igual ya no queda pendiente.** Es R-03 (i) y el texto de OD-PBTC-5.
- **Una avanzada vacía sigue siendo «el cálculo»,** como hoy.
- **La corrección de I-01** (B presente) no decide nada: un rack de un sentido sigue la regla `Keep`, que el Owner ya eligió.

## 9. Límites

Están en `pbtc2-measurements.md`, apartados H e I:
- nada se compiló ni se ejecutó;
- el efecto en el dibujo y el BOM lo mide la cadena;
- los recuentos son estáticos;
- no se sabe cómo cuenta el Controller un `Skipped`.

## 10. Arrastres (no son de PB-TC2)

| Id | Qué | Va a |
|---|---|---|
| M-07 de IRT2-1 | **Defecto anterior.** Sin lado B y con el indicador mixto obsoleto (H2-11), vaciar «Largo manual» no surte efecto, porque la rama vacía de `OptionalVal` da el lado (W:3668). Además, las cajas obligatorias quedan opcionales (WC:580). PB-TC2 lo conserva tal cual (D-PBTC2-04) | **Paquete de PB-T1, antes de su contrato:** reproducirlo o corregirlo, con una posible vuelta por OB-06.5 |
| M-03 de PB-TC2 | La regla de «Ambos lados» en el alcance (W:1346) y en los datos del frente (W:1510), sin CT | Red de PB-T1, como M-03 de PB-TC; en `KnownLimitations` de la entrega |
