> **Redaccion para el repo publico:** las rutas locales se sustituyen por marcadores (`<worktree-de-I-64>`, `<carpeta-del-paquete>`, `<datos-locales-del-usuario>`). El resto no cambia.

# I-64 F4B — Paquete del contrato de PB-T1 (el estado editable de Push Back sin pérdidas fuera de la ventana), v1

**Estado:** v1, para la revisión independiente adversarial. No se emite. No declara GATE PASS.

**Lo que pide el Owner** ([OD], fila de PB-T1 del §5 de [P]):

> ACCEPT IN PRINCIPLE. PB-T1 extracts a lossless Push Back editable state outside the WPF window. Hard requirements: module state preserved; parked queue preserved; A/B preserved; no aliasing; no WPF control becomes source of truth; state round-trip lossless for supported classic racks; classic RACKEDITAR still works through the extracted state. Do not rewrite the entire editor merely to obtain an MVVM architecture. The goal is state separation, not aesthetic refactoring.

**Lo que propone este paquete:**
- **Tres subtareas secuenciales**, cada una con su cadena y su presupuesto de 1/3 (OB-10.1):
  - **PB-T1a — la red** (`F4B-PBT1A-LOSSLESS-STATE-NET`): solo pruebas de caracterización, con RED por mutantes. Cubre los huecos de D-5, M-03 de PB-TC y de PB-TC2, M-04, M-07, la observación de CT-16 y M-02 de PB-TC, y fija las huellas de salida de la ventana que PB-T1c no puede mover. El producto no cambia.
  - **PB-T1b — el tipo** (`F4B-PBT1B-LOSSLESS-STATE-TYPE`): el estado editable en Application, con sus cargas, su salida, su copiador, su comparador, el mapa de propiedad, la regla de c-02, la exclusión de `pb-21-cola` y el criterio de bloqueo extraído. La ventana no cambia.
  - **PB-T1c — la ventana sobre el estado** (`F4B-PBT1C-LOSSLESS-STATE-WINDOW`): la ventana guarda su estado en el tipo de PB-T1b, en modo CLASSIC y sin cambio observable.
- **Ninguna corrección de producto.** RACKEDITAR no cambia (OB-RE.1). c-02 se reproduce como regla explícita (F4B-E09, pregunta 2).
- **Decisiones materiales nuevas:** ninguna que el paquete tome por su cuenta. El §8 lista tres lecturas que la revisión debe confirmar; si la revisión considera material alguna, PB-T1 vuelve al Owner antes de emitirse.

**Base de la medición:** rama `architecture/workspace-persistente-rackcad` en HEAD `eedb069c`, con `main` en `812baf87`.
- El producto de HEAD es el GREEN de PB-TC, `4440f67d`: `git diff 4440f67d eedb069c -- src tests` vacío (medición A-4).
- **PB-T1 depende del cierre de PB-TC2** (F4B-E09), que está parada en una vuelta al Owner. La `BaseSha` de PB-T1a es el commit de cierre de PB-TC2.
- Las zonas que PB-TC2 toca se marcan **[TC2]**: «a volver a medir sobre el GREEN de PB-TC2». Son `Gap_Changed`, `ModuleLength_Changed`, `ReadAdvancedRackParameters`, `ReadCellValues`, `OptionalVal` y `Val`, más el desplazamiento de las líneas de W posteriores a W:3634 (medición L).
- Todo hecho va anclado por **nombre de miembro y línea**. El detalle y las órdenes están en `pbt1-measurements.md` (apartados A a L).

**Precondiciones de emisión** (de cada subtarea, salvo que se diga otra cosa):
1. **PB-TC2 cerrada** (CLOSED con su cadena y su anotación en `f4b-dispositions.md`). Si PB-TC2 no cierra, o cierra con un diseño distinto de su paquete v2, este paquete se vuelve a redactar.
2. **La revisión independiente de esta v1**, y sus comprobaciones acotadas, con 0 bloqueantes, 0 importantes sin resolver y ninguna decisión material nueva (F4B-E09, pregunta 4).
3. **La medición de los anclajes** sobre la `BaseSha` de la subtarea (§6.0, «Anclajes»). Un anclaje que no se sostiene de un modo que cambia un CT o el diseño es STOP antes de emitir.
4. Las celdas del §6.0 no están `STALE` (vencen el 2026-12-29); el `config.toml` coincide con la Snapshot I; el binario de Codex es el acreditado.
5. **Custodia en el commit de emisión de PB-T1a,** que es su `AuthorityRevision`: este paquete, en `docs/initiatives/I-64-f4b-pbt1-gate-contract-package.md`; el registro, en `docs/initiatives/I-64-f4b-pbt1-measurements.md`; los tres scripts de medición; el contrato y su generador en `docs/automation/evidence/I-64-pilot/F4B-PBT1A-LOSSLESS-STATE-NET/coordinator/`; y en `f4b-dispositions.md`, la división (§3, D-PBT1-01) y los arrastres del §10.

## Registro de cambios

v1: primera redacción.

## 1. Autoridades

| Autoridad | Qué fija |
|---|---|
| [OD] (`F4B-DRAFTS/coordinator/owner-f4b-disposition.md`) | La fila de PB-T1 (literal arriba). D-PB-03 («Do not admit A/B racks before that state is lossless»), D-PB-05 (el editor completo gana en sus campos), D-PB-06 («Do not hide these corrections inside PB-T1 state extraction»), D-PB-10 (1/3; el intento 3 vuelve al Owner), §7.3 (T1B) y §7.4 (RACKEDITAR) |
| [P] (`docs/initiatives/I-64-f4b-push-back-completo-package.md`, v3.1) | OB-01.1b (constructores y llamadas de los cuatro sitios), OB-01.5, OB-03.1 (el rechazo del piloto sigue), OB-05.1 (el mapa de propiedad), OB-06.5, OB-10.1 (las subtareas, cada una 1/3), OB-T1B.1 a OB-T1B.3, OB-RE.1, H-PB-27, R-PB-01, R-PB-02, R-PB-05 y R-PB-09; la fila de PB-T1 del §5 (`:503`) |
| [OPT1] (`F4B-DRAFTS/coordinator/owner-pbt1-pre-decisions.md`, F4B-E09) | Pregunta 2: c-02 «Como hoy, como regla explícita». Pregunta 3: `pb-21-cola` «Dejarlo fuera de la garantía». Los demás casos de T-2, tal cual. H-31 y OWNER-1 en PB-TC2, antes de PB-T1. Pregunta 4: la autorización condicional de emisión |
| [DP] (`F4B-DRAFTS/coordinator/owner-f4b-procedure-decisions.md`, F4B-E03) | Pregunta 1: los sustitutos de nc1 y nc2 solo con la firma EXACTA de PD-F4-04 y PD-F4-01. Pregunta 2: una replanificación por la comparación literal |
| El expediente (`F4B-DRAFTS/pbt1/ob065-dossier.md`) | §3: los huecos de red (D-5, los arrastres de PB-T0 y PB-TC). §4: las guardas de T1B y de texto, con sus condiciones |
| PB-T0: su paquete, su cierre (`F4B-PBT0-CHARACTERIZATION/task-close-package.md`) y su entrega (`…/1/R20261117T185616Z-3c1b/worker-handoff.json`) | Los 21 CT-PB0; T-1 (el censo control → prueba), T-2 (los casos alcanzables) y T-5; M-01, M-04, D-3, D-5 y la observación de CT-16; el patrón del RED por mutantes |
| PB-TC: su paquete ([PKC], v2.2), su cierre (`F4B-PBTC-DECLARED-CORRECTIONS/task-close-package.md`) y sus pruebas | El patrón del contrato; M-02 a M-05 |
| PB-TC2: su paquete ([PKC2], v2) | El patrón más reciente; M-03 y M-07 (§10, arrastres a PB-T1); las firmas literales de PBTC2-C11; R2-08 |
| T1B (`docs/initiatives/I-64-f4-t1b-gate-contract-package.md`) | La caracterización vigente de la ventana clásica: 55 casos en UI y 10 en Core |
| Los contratos de F4 con una API nueva (`I-64-f4-t2-gate-contract-package.md` §6, `I-64-f4-t3-gate-contract-package.md` INV-F4T3A-13, `I-64-f4-t3c-gate-contract-package.md`) | El RED con **esqueleto inerte** que compila y no lanza, y pruebas que fallan **por aserción** |
| ADR-0029 D12 y D13; Proposal V5 D-10, D-15 (punto 3) y D-16 | Sin modelo paralelo; la caracterización no se edita para que pase; ida y vuelta del estado sin la ventana; sin cambio observable |
| `docs/AUTOMATION_PLAN.md` §16.5, §16.8, §16.9, §16.10 y §16.11; `docs/automation/agent-execution/README.md` §4 y §10; `routing.md`; `model-catalog.md` | A1-A8, el RED de la cadena, las 14 comprobaciones, los términos, las paradas, nc1 a nc4 y las celdas |

## 2. Hechos medidos en `eedb069c`

### 2.0 Alias

- **W** = `src/RackCad.UI/Systems/PushBack/RackPushBackSystemWindow.xaml.cs` (3690 líneas); **WC** = `….Composite.cs` (936); **WX** = `….xaml` (793).
- **CS** = `src/RackCad.Application/Systems/PushBack/PushBackCompositeEditorState.cs`; **ES** = `…/PushBackEditorState.cs`; **EL** = `…/PushBackEditorState.Load.cs`; **DA** = `…/PushBackEditorDesignAssembler.cs`; **CA** = `…/PushBackCompositeEditorAssembler.cs`.
- **PC** = `…/Pilot/PushBackPilotStateComparer.cs`; **PM** = `…/Pilot/PushBackPilotMaterializer.cs`.
- **RMES** = `src/RackCad.Application/Systems/Shared/RackModuleEditSession.cs`.
- **S0** = `tests/RackCad.UI.Tests/PushBackFullEditorCharacterizationSupport.cs`; **SC1** = `tests/RackCad.UI.Tests/PushBackDeclaredCorrectionSupport.cs`.

### 2.1 Hoy: qué estado editable vive en la ventana y qué ya vive en Application

- **HT1-01. Ya en Application:** `composite` (W:56-57), un `PushBackCompositeEditorState` con los dos `PushBackEditorState`.
  - Contiene los dos lados (matriz, configuración Push Back por celda, saque, tope y defensa), la intención compuesta (hueco, separador, overrides, topologías y su default), la presencia de B y `sideBEverDeclared`, la cola aparcada (CS:58, CS:61) y su marca de consumo (CS:147), y la selección (`ActiveSide` CS:167, `ActiveSelection` CS:179 y la selección de cada matriz).
  - Cada lado lleva además la **autoridad de módulos**: `workingBaseline` (ES:65), `moduleSession`, `moduleCommit` y `moduleSignature` (ES:110-112) y `LastModuleReconciliation` (ES:143). Los módulos son del lado A (`moduleOwner`, W:82).
- **HT1-02. En campos de la ventana** (medición B.1): `generalPallet` (W:101), los cuatro portadores de PB-TC `rackTolerance`, `rackPostPeralte`, `rackBeamDepth` y `rackAnnotationScale` (W:106-109), `advanced` (W:114, los seis avanzados), `firstLevelDatum` (W:380), `loadedDimensionViews` y `dimensionViewsTouched` (W:1249, W:1260), `loadedDimensionStyle` (W:1256) y `safetySelections` (W:84).
- **HT1-03. Solo en un control, releído en cada pasada por `ReadInputs`** (medición B.3): `WeightUnitBox` (W:852), `PalletsDeepBox` (W:853), `PostBox` (W:868), las tres casillas de anotación (W:874-876), `DimensionsBox` (W:878) y las tres casillas de cotas por vista (`SelectedDimensionViews`, W:1291-1297, desde W:880).
- **HT1-04. Contexto de la sesión clásica, no editable:** `sourceProject` (W:91), `isEditingExisting` (W:87), `frontalSide` (W:3261, no se persiste) y el origen de cabecera pendiente (`pendingHeaderConfiguration`, W:2038-2039).
- **HT1-05. La identidad y el nombre** viven en `session.Identity` (`RackEditorIdentity`, UI): `Adopt` en las tres cargas (W:279, :305, :322); `SetName` en `Name_Changed` (W:1234-1238) y antes de cada salida (W:3352, :3405, :3504); `EnsureId` en la biblioteca (W:3503). `RackEditorIdentity` guarda el nombre tal cual (`RackEditorIdentity.cs:36-39`).
- **HT1-06. Derivados o de interfaz** (medición B.2): `lastComputation`, las banderas de validez, `outputIsBlocked`, `lastCompositeSystem`, el estado mixto (`mixedOptional`, `mixedNow`, WC:416, WC:429) y las cachés de la interfaz.

### 2.2 El flujo: quién siembra, quién lee y quién escribe

- **HT1-07. La carga mezcla Application y controles.** Las tres cargas y «Restaurar» (W:270-289, W:294-307, W:311-324, W:3307-3330) hacen primero la parte de Application y después `LoadFromModel` (W:385-440). **`LoadFromModel` siembra a la vez los controles y los portadores** (W:391, :393, :398, :401, :403, :406-411, :414, :419, :421 y :423-427) y recalcula (W:439). En `LoadNew` hay dos pasadas, con la seguridad por defecto entre ellas (W:280, W:286-288).
- **HT1-08. La parte de Application de la carga vive en la ventana:**
  - `SideAOnly` (W:342-373), estático y puro;
  - `LoadCompositeFromDesign` (WC:813-814, WC:827-934), toda de Application salvo `UpdateCompositeEnabled()` (WC:933). Hace el aparcado de la cola (WC:860-868), la reposición de lo dormido en «Restaurar» (WC:829-858) y la carga del lado B (WC:870-927).

  El piloto tiene su propia copia de esa parte (PM:150-172).
- **HT1-09. Cada pasada** (`Recompute`, W:616-723) escribe la celda primaria desde los controles (`CommitCurrentCell` W:635 → `ReadCellValues` W:802-831 **[TC2]**), actualiza los portadores con `Keep` (`ReadInputs` W:838-844), construye unos inputs **nuevos** (W:846-889), arma y resuelve (W:659-660), y adopta la computación: la línea base pasa a ser `computation.System` (W:675 → DA:101-107).
- **HT1-10. El ensamblador muta lo que recibe:** el estado del lado A (los peraltes y el saque, DA:155-159) y los inputs de la pasada (`PushBackAdvancedRackParameters.Reset`, DA:302). Que los inputs sean nuevos en cada pasada es lo que protege hoy al portador `advanced` (H-PB-28, D-PB-11 en W:696-702).

### 2.3 Los alias con el modelo cargado (medición D)

- **HT1-11.** `SideAOnly` y el diseño del lado B de `LoadCompositeFromDesign` comparten objetos con el diseño del llamador, pero solo de forma transitoria: los lee la carga, que resuelve, y el resolver copia (D-1, D-2).
- **HT1-12. La cola aparcada guarda los mismos objetos del diseño cargado** (WC:863-867 → CS:85-95). Nadie los muta: el armado los copia al despertar (DA:535-571). Es el único alias que dura toda la sesión (D-3).
- **HT1-13.** `sourceProject` es el proyecto del llamador, por diseño: viaja por referencia en cada petición (D-4).
- **HT1-14. La línea base resuelta es el mismo objeto que `lastComputation.System` y `session.System`** (W:674-680). Por lectura, nadie la muta: se sustituye en cada computación válida (D-5).

### 2.4 Los controles fuente de verdad que quedan tras PB-TC y PB-TC2 (medición E)

- **HT1-15.** Con la definición de H-PB-44, y con el diseño de PB-TC2 v2, **solo `PostBox` decide el estado con el catálogo entregado**: c-02 (un id fuera del catálogo → nulo → `Defaults.Post`, W:399, W:868, DA:169-171) y c-01 (en blanco → `Defaults.Post`, W:399).
- **HT1-16.** Los demás controles de HT1-03 **no pierden nada** (E-5, E-6): la siembra y la relectura coinciden. Pero hoy su valor **no tiene portador fuera del control**. PB-T1 se lo da.
- **HT1-17.** `RearPeralteBox` (lista fija W:44) y los combos de peralte de la celda (`SetPeralteOptions` W:3620-3630) solo sustituyen con un catálogo distinto del entregado o con un documento que la carga de Application ya normaliza (E-3, E-4). Sin caso alcanzable (OB-06.5).
- **HT1-18. M-07 de PB-TC2:** el indicador mixto obsoleto (H2-11 de PB-TC2) hace inerte una caja vacía. No cambia el estado (E-7).

### 2.5 El criterio de bloqueo de salida (medición F)

- **HT1-19.** La línea que cita la fase (`RackPushBackSystemWindow.Composite.cs:778-782` en `5ccaa770`) es `CompositeHasBlocking`. Hoy es **WC:803-807**.
  - El criterio son `CompositeDiagnostics` (WC:783-786) y `CompositeStatusOr` (WC:789-801).
  - Es «los diagnósticos de la intención (`composite.IntentDiagnostics()`, CS:629-631) más los del sistema resuelto (`PushBackCompositeDiagnostics.Evaluate`)», con bloqueante si alguno `IsBlocking`.
  - **No es `RackBomOutputGate.For`,** que solo mira el sistema (`RackBomOutputGate.cs:49-59`).

### 2.6 El copiador y el comparador del piloto (medición G)

- **HT1-20.** `PushBackPilotStateComparer.Copy` (PC:58-111) **no copia la autoridad de módulos ni la cola**, y el comparador no las lee (PC:53-56, PC:134-177). `KeepPilotEditsOn` (PM:242-251) devuelve un estado sin ellas, y `ComposeForWrite` (PM:196-234) repone desde la base (H-PB-27).

### 2.7 Guardas que atan el diseño y que el expediente no lista (medición H)

- **HT1-21. El censo del piloto** (`tests/RackCad.Tests/Workspace/WorkspaceDraftPilotStateTests.cs:204-537` y `:955-964`) exige que **todo campo de instancia** del grafo de `PushBackPilotDraftState` esté clasificado. Ese grafo incluye `PushBackCompositeEditorState`, `PushBackEditorState`, `PushBackEditorInputs`, `PalletSpecification`, `DynamicAnnotationOptions` y los tipos de seguridad. **Un campo nuevo en cualquiera de ellos la rompe.** Los métodos nuevos no la tocan.
- **HT1-22.** Ninguna guarda de ensamblado mira `RackCad.Application.Systems.PushBack` (H-8). Ningún prefijo de clase propuesto choca con uno existente (medición I).
- **HT1-23.** Ninguna prueba nombra `SideAOnly`, `LoadCompositeFromDesign`, `generalPallet`, los portadores, `loadedDimension*`, `firstLevelDatum`, `isEditingExisting`, `CompositeHasBlocking`, `frontalSide` ni `ReadCellValues`. Solo los literales de T-SG nombran `lastComputation`, `sourceProject`, `moduleOwner`, `session.Identity` y `NameBox.Text` (medición H).

### 2.8 Lo que PB-TC2 cambia y obliga a volver a medir [TC2]

- W:198 de WC (`Gap_Changed`), W:1905 (`ModuleLength_Changed`), W:1639, :1641, :1642 y :1646 (`ReadAdvancedRackParameters`), W:818-822 (`ReadCellValues`), W:3667 (`OptionalVal`), la retirada de `Val` (W:3634-3635) y tres miembros nuevos tras `Keep` (W:3686).
- **Desplazamiento:** las líneas de W anteriores a W:3634 no se mueven. Las posteriores se mueven −2 hasta `Keep`; `IntVal` y lo que le sigue, además, por los miembros nuevos.
- **Lo que PB-T1 usa de esas zonas:** los mutantes MU-T1A-03, 04, 07a y 10 (§5.2) y la rama vacía de `OptionalVal`, que PB-TC2 deja literal (D-PBTC2-04).

## 3. El diseño

### D-PBT1-01. Tres subtareas secuenciales (R-PB-01, R-PB-09, OB-10.1)

**Por qué dividir:**
- La red de caracterización necesita RED por mutantes y un producto intacto (el régimen de PB-T0). La extracción necesita RED con esqueleto. La ventana sobre el estado es una refactorización sin cambio observable, cuyo RED solo pueden ser pruebas estructurales. Son tres regímenes de RED distintos.
- La red tiene que estar **en la CI, en verde y con su discriminación probada** antes de que la ventana cambie («antes de mover el estado», [P] §6.2 y el expediente §3).
- El tipo de Application se puede probar entero contra la ventana de hoy **antes** de que la ventana dependa de él (D-15 punto 3: ida y vuelta sin la ventana).
- Tamaño. Como referencia, PB-T0 hizo 21 CT y 20 mutantes en 437 turnos, y PB-TC 10 métodos en 220. PB-T1 entera serían unos 30 CT, 15 mutantes, cuatro archivos nuevos de Application y la refactorización de W y WC: demasiado para un Worker sin perder trazabilidad.

**La secuencia:** PB-T1a → PB-T1b → PB-T1c. Cada una parte de la punta que deja cerrada la anterior, sin rebase entre tareas. Sus alcances de escritura no se solapan salvo W y WC: PB-T1a los toca solo con mutantes, que revierte. Delegaciones sucesivas, una abierta por unidad (`AUTOMATION_PLAN.md:557-558`).

**Lo que no cambia en la fase:** PB-T2 y PB-T3 dependen de PB-T1, que se cierra cuando cierra PB-T1c. El contenido de PB-T1 es el del §5 de [P], sin añadir ni quitar.

**Propiedad que da la división:** PB-T1a solo cambia `tests/` en su commit final y PB-T1b solo cambia Application. Por eso los anclajes de W y WC que PB-T1c usa son los del cierre de PB-TC2, y basta comprobar `git diff <cierre de PB-T1b> -- src/RackCad.UI` vacío antes de emitir PB-T1c.

### D-PBT1-02. El tipo de estado: `PushBackEditableState` (R-PB-05)

**Dónde:** `src/RackCad.Application/Systems/PushBack/PushBackEditableState.cs`, espacio de nombres `RackCad.Application.Systems.PushBack`. Ni en `Pilot/` ni en `Workspace/`.

**Qué es:** un **agrupador** del estado que ya existe, sin modelo paralelo (D-10, ADR-0029 D12). Contiene:

| Miembro | Tipo | Valor inicial (el de los campos de hoy) | Es hoy |
|---|---|---|---|
| `Editor` | `PushBackCompositeEditorState` (de solo lectura) | `new PushBackCompositeEditorState(new PushBackEditorState(), new PushBackEditorState())` | `composite` (W:56-57) |
| `Pallet` | `PalletSpecification` | `PushBackEditorInputs.NewDesign().Pallet` | `generalPallet` (W:101); **también lleva la unidad** |
| `PalletsDeep` | `int` | `DynamicRackDefaults.DefaultPalletsDeep` | `PalletsDeepBox` |
| `PostCatalogId` | `string` | `null` | `PostBox` |
| `PostPeralte`, `PalletTolerance`, `BeamDepth`, `AnnotationScale` | `double` | `NewDesign().PostPeralte`, `DefaultPalletTolerance`, `DefaultBeamDepth`, `1.0` | W:106-109 |
| `FirstLevelDatum` | `int?` | `(int)RackFirstLevelDatumMode.LowestUsablePunch` | W:380 |
| `NumberFronts`, `NumberLevels`, `DrawRackName` | `bool` | `false` | las tres casillas |
| `Dimensions` | `DimensionDetail` | `DimensionDetail.None` | `DimensionsBox` |
| `DimensionStyle` | `string` | `null` | `loadedDimensionStyle` (W:1256) |
| `LoadedDimensionViews` | `DimensionViewVisibility?` | `null` | W:1249 |
| `DimensionViewsTouched` | `bool` | `false` | W:1260 |
| `DimensionsFrontal`, `DimensionsLateral`, `DimensionsPlanta` | `bool` | `false` | las tres casillas de cotas |
| `Advanced` | `PushBackEditorInputs` (de solo lectura; **solo sus seis avanzados**) | `PushBackEditorInputs.NewDesign()` | `advanced` (W:114) |
| `SafetySelections` | `List<SelectiveSafetySelection>` (de solo lectura) | vacía | `safetySelections` (W:84) |
| `Name` | `string` | `null` | el nombre de la identidad (HT1-05) |

**Lo que no contiene** (§3, D-PBT1-08): el GUID, `sourceProject`, `isEditingExisting`, `frontalSide`, el origen de cabecera pendiente, el estado mixto, `lastComputation` y lo demás de HT1-06.

**Ningún campo nuevo en los tipos del censo del piloto** (HT1-21). El tipo nuevo no es alcanzable desde `PushBackPilotDraftState`.

### D-PBT1-03. Las cargas, con la semántica de hoy

Son **métodos de instancia que mutan en sitio** el mismo `Editor`, como hoy la ventana muta su `composite` de solo lectura. Una prueba que guarda `w.CompositeState` entre dos cargas sigue viendo el mismo objeto.

| Método | Reproduce | Pasos, en este orden |
|---|---|---|
| `PushBackEditorInputs LoadNew(RackCatalog catalog)` | W:273-276 y los portadores de `LoadFromModel` | `Editor.SetActiveSide(A)`; `Editor.SetSideBPresent(false)`; `Editor.LoadComposite(null)`; `var inputs = Editor.Active.LoadNew()`; `AdoptCarriers(inputs, catalog)`; `Name = null`; devuelve `inputs` |
| `void SeedDefaultSafety(RackCatalog catalog)` | W:286-287 | `SafetySelections.Clear()`; `AddRange(new PushBackSafetyAuthority(catalog).Defaults())` |
| `PushBackEditorInputs LoadDesign(PushBackDesign design, string rackName, PushBackResolver resolver)` | W:316-319 (y W:299-302, que son iguales), WC:827-932 con `preserveDormantIntent: false`, y los portadores | `SetActiveSide(A)`; `SetSideBPresent(false)`; `var inputs = Editor.Active.LoadFromDesign(SideAOnly(design), resolver)`; `LoadComposite(design, false, resolver)`; `AdoptCarriers(inputs, resolver.Catalog)`; `Name = rackName`; devuelve `inputs` |
| `PushBackEditorInputs Restore(PushBackDesign lastValidDesign, PushBackResolver resolver)` | W:3320-3327 y los portadores | igual que `LoadDesign`, con `preserveDormantIntent: true` y **sin tocar `Name`** (hoy `Restore_Click` no toca la identidad) |
| `static PushBackDesign SideAOnly(PushBackDesign design)` | W:342-373, movido literal | — |

- **`LoadComposite(design, preserveDormantIntent, resolver)`** es privado y es WC:829-931 movido literal, con dos cambios sin efecto observable:
  1. no llama a `UpdateCompositeEnabled()`, que es de la interfaz (la llama la ventana, D-PBT1-13);
  2. **la cola que aparca se copia** (`DynamicRackModuleDesign` con la copia de `CopyIntent` de RMES, y `DynamicHeaderLineOverride` campo a campo con `RackFrameProjectStore.DeepCopy` para su configuración). Así el estado no comparte objetos con el diseño cargado (HT1-12). Como el armado ya copiaba al despertar (DA:535-571), el diseño de salida es el mismo.
- **`AdoptCarriers(inputs, catalog)`** es privado y es la parte de portadores de `LoadFromModel`, con sus normalizaciones literales:
  - `Pallet = inputs.Pallet ?? new PalletSpecification(42.0, 48.0, 60.0, 1000.0, "kg")`, y **la unidad, en blanco → «kg»** (W:392, W:395);
  - `PalletsDeep = Math.Max(2, inputs.PalletsDeep)` (W:396);
  - `PalletTolerance`, `BeamDepth` y `AnnotationScale` con su «> 0 ? : defecto» (W:397-398, W:402-403, W:418-419);
  - `PostPeralte = inputs.PostPeralte` (W:401);
  - **`PostCatalogId = ResolvePostCatalogId(catalog, inputs.PostCatalogId)`** (D-PBT1-09);
  - los seis avanzados copiados (W:406-411);
  - de `inputs.Annotations ?? new DynamicAnnotationOptions()`: `DimensionStyle`, las tres casillas, `Dimensions` recortado a `[0, Detailed]` (W:413-420);
  - `LoadedDimensionViews = options.DimensionViews`, `DimensionViewsTouched = false` y cada casilla de cotas = `DimensionViewPolicy.EffectiveDetail(DimensionDetail.Minimal, views, kind) != DimensionDetail.None` (W:1268-1287);
  - `FirstLevelDatum = inputs.FirstLevelDatum` (W:391);
  - `SafetySelections` vaciada y llenada con `DeepCopy` de las no nulas (W:423-427).
- **Una carga que lanza** (c-13, D-3) lanza el mismo tipo y el mismo mensaje que hoy, desde el mismo resolver, antes de tocar los portadores.

### D-PBT1-04. La salida, con la semántica de hoy

- **`PushBackEditorInputs BuildInputs(PushBackEditorDesignAssembler assembler)`** devuelve **un objeto nuevo en cada llamada**, igual campo a campo al que hoy construye `ReadInputs` (W:846-889) cuando los controles no se editaron:
  - `Pallet = new PalletSpecification(Pallet.Front, Pallet.Depth, Pallet.Height, Pallet.Weight, Pallet.WeightUnit)`;
  - los escalares y los seis avanzados, copiados;
  - `Annotations` nuevas, con `DimensionViews = DimensionViewPolicy.FromEditor(LoadedDimensionViews, DimensionViewsTouched, DimensionsFrontal, DimensionsLateral, DimensionsPlanta)`;
  - `SafetySelections` = `assembler.AuthorizedSafety(SafetySelections)`, que ya da copias.
- **Por qué nuevo cada vez:** el ensamblador muta los inputs (DA:302). Así ningún armado toca el estado más allá de lo que ya toca hoy (DA:155-159, sobre `Editor.SideA`).
- **El armado no cambia:** quien calcula sigue llamando a `assembler.BuildFrom(compositeAssembler.BuildDesign(state.Editor, state.BuildInputs(assembler)), frontalSide)` y a `AcceptComputation`, como W:659-679. PB-T1 no crea un camino de cálculo nuevo.

### D-PBT1-05. El copiador: `PushBackEditableStateComparer.Copy` (H-PB-27)

**Dónde:** `src/RackCad.Application/Systems/PushBack/PushBackEditableStateComparer.cs`, que es también el comparador (D-PBT1-06). **El copiador del piloto no se toca** (D-PBT1-14).

**Qué hace,** en este orden:
1. **El contenido editable y la selección:** el `Snapshot()`/`Restore()` de CS sobre un `Editor` nuevo, con la reposición de `ActiveSelection`, de la selección de cada lado y de `ActiveSide` que ya hace el piloto (PC:64-83), copiada.
2. **La cola:** `copy.Editor.ParkDormantTail(<copias de DormantCompositeTail>, <copias de DormantTailLineOverrides>)`, con la copia de D-PBT1-03; después, si `source.Editor.DormantTailConsumptionPending`, `copy.Editor.MarkDormantTailConsumed()`. Así se reproduce `dormantTailConsumed` (CS:116-117, :147) sin tocar CS.
3. **La autoridad de módulos de cada lado:** `copy.Editor.SideX.CopyAuthorityFrom(source.Editor.SideX)`, un método nuevo de ES (D-PBT1-07) que, sobre el lado ya restaurado:
   - copia la sesión con `RackModuleEditSession.Copy()` y el commit con `RackModuleCommit.Copy()`, o deja null si eran null;
   - copia `moduleSignature`;
   - **comparte por referencia** `workingBaseline` y `LastModuleReconciliation` (L-1 del §8).
4. **Los portadores:** campo a campo; `Pallet` nueva; `Advanced` con sus seis avanzados copiados; `SafetySelections` con `DeepCopy`; `Name`.

**El resultado:**
- es igual para el comparador;
- no comparte con el original ningún objeto mutable salvo los dos derivados del punto 3, que el producto ya comparte hoy entre la ventana, la sesión y el estado (HT1-14) y que nadie muta (se sustituyen);
- **conserva los módulos** (sesión con lo escenificado, commit y línea base), **la cola aparcada** (también la dormida en sesión) y **el lado B**, presente o dormido, con `sideBEverDeclared`.

### D-PBT1-06. El comparador: `PushBackEditableStateComparer : IEqualityComparer<PushBackEditableState>`

Compara, con igualdad exacta (un `double` por valor, una cadena ordinal):
- **el contenido editable del `Editor`**, como el piloto (PC:134-249, copiado): la intención compuesta, `sideBEverDeclared`, las topologías y los dos lados sin su selección;
- **la cola:** las dos listas, elemento a elemento, con la forma persistida de cada configuración de cabecera (la de `RackFrameProjectStore`, como RMES `:818-846`);
- **la autoridad de módulos de cada lado:**
  - la intención de módulos que el próximo armado llevaría: la del commit, o si no la de la línea base (`ModuleIntents`, `LineOverridesOf` y `DerivedPostLinesOf`, DA:57-81), con igualdad estructural;
  - la sesión, con `RackModuleEditSession.SameEditAs` (D-PBT1-07), o las dos null;
  - el commit, con `RackModuleCommit.SameAs`, o los dos null;
- **los portadores** (todos los de D-PBT1-02), con la igualdad estructural de la seguridad del piloto (PC:309-350, copiada).

**No compara:** la selección (`ActiveSide`, `ActiveSelection` y la selección de cada matriz), la línea base como objeto, el informe de reconciliación ni la marca de consumo.

`GetHashCode` mira solo campos baratos (como PC:37-49). Un `NaN` es igual a sí mismo (`double.Equals`).

### D-PBT1-07. Los métodos nuevos en ES y en RMES (sin campos nuevos)

- **ES:** `public void CopyAuthorityFrom(PushBackEditorState source)`, con lo del paso 3 de D-PBT1-05. Es el único cambio de ES. **CS no cambia.**
- **RMES:**
  - `public RackModuleEditSession Copy()`: una sesión nueva con `restored`, `committed`, `working`, `standardRestoreRequested`, las dos tablas de líneas (con `CopyLines`, RMES:732-742) y las dos de postes derivados, copiados con `CopyIntent` y `clone.DeepCopy` (RMES:799-811);
  - `public bool SameEditAs(RackModuleEditSession other)`: `committed` y `working` con `AreEquivalent` (RMES:818-846), las líneas con `AreEquivalentLines` (RMES:750-769), los postes derivados con `AreEquivalentDerivedPosts` (RMES:744-746), `restored` en orden y `standardRestoreRequested`;
  - en `RackModuleCommit` (RMES:16-50): `public RackModuleCommit Copy()` y `public bool SameAs(RackModuleCommit other)`, con las mismas copias e igualdades.
- **Son neutrales** (RMES no conoce sistemas, `RackModuleEditSessionTests.cs:509-…`): no nombran `RackSystemKind` ni Push Back.

### D-PBT1-08. El mapa de propiedad (OB-05.1)

**Dónde:** `src/RackCad.Application/Systems/PushBack/PushBackEditableStateOwnership.cs`. Es una **tabla de datos**, sin lógica de composición (la composición es de PB-T3, OB-05.2).

**Las clases:** `Editor` (lo edita el editor completo, directamente o recalculado desde lo que edita) y `Envelope` (sobre o infraestructura).

| Nivel | `Envelope` | `Editor` | Fuente |
|---|---|---|---|
| Sobre (`RackEmbedDocument`) | `SchemaVersion`, `Kind`, `View`, `Section`, `Id` (la identidad, el GUID), `CustomProperties`, `ExtensionData` | **`Name`** (lo edita `NameBox`; OB-05.1) | `RackEmbedDocument.cs:35-75`; W:1234-1238 |
| Proyecto (`RackProjectDocument`) | `SchemaVersion`, `Kind`, `Header`, los documentos de los otros sistemas, `ExtensionData` | — (`PushBack` es el contenedor) | `RackProjectDocument.cs:18-52`; W:3507 (`WithSourceMetadataFrom`) |
| Carga útil (`PushBackDesignDocument`) | `SchemaVersion`, `ExtensionData` | todo lo demás: `Structure.*`, `Fronts`, `LegacyHighEndBeamPeralte`, `RearTopeSaque`, `RearTopePieceId`, `DefensePieceId`, `RearTopeOffCells`, `SideB.*` y `Composite.*` | `PushBackDesignDocument.cs:24-76`; lo escriben DA:383-436, CA:52-89 y DDA:90-124 desde el estado |
| Sesión del borrador | `RackId` y los metadatos del borrador | — | `DraftSession.cs:277-321` |

- **`Name` es del editor; la identidad es el GUID** (OB-05.1). Con «Conservar mi borrador» gana el nombre del borrador; eso es de PB-T3 y PB-T4.
- **Límite declarado** (OB-05.2, IR2 R-I-10): los campos desconocidos de los documentos **anidados** se pierden en cada reescritura, clásica o del panel. El mapa los cuenta como `Editor` por su contenedor. La pregunta al Owner sigue siendo de antes de PB-T3.
- **Cada fila lleva su ancla como dato** (la columna «Fuente»), así que el mapa se deriva del código y no se inventa.

### D-PBT1-09. La regla de c-02 (F4B-E09, pregunta 2)

- **`public static string ResolvePostCatalogId(RackCatalog catalog, string requested)`:**
  - en blanco o solo espacios → `catalog?.Defaults?.Post` (c-01, W:399);
  - un id que **no** está, con comparación **ordinal**, entre los `Id` de `catalog.PostProfiles` → `catalog?.Defaults?.Post` (c-02);
  - si no, `requested`.
- **Por qué ordinal y no como `ResolveBeamId`,** que compara sin mayúsculas (`DynamicRackLevelGeometry.cs:200-204`): el Owner pidió «como hoy» y «RACKEDITAR no cambia», y hoy decide `PostBox.SelectedValue`, que por lectura compara de forma ordinal. **No está medido** (medición B.3). El paso 1 de PB-T1b lo mide en la base (§6.2) y, si la ventana conserva un id con otras mayúsculas, la regla se para (PBT1B-C14). Es la lectura L-2 del §8.
- **Dónde vive:** en `AdoptCarriers` (D-PBT1-03). La ventana sigue sembrando `PostBox` con la expresión de hoy (W:399), así que el desplegable se ve igual que hoy, vacío con un poste desconocido. Lo que cambia es que el estado guarda el poste por defecto en vez de dejar que lo decida el control.
- **CT-PB0-17 sigue en verde** (fija la salida, que no cambia).

### D-PBT1-10. La exclusión de `pb-21-cola` (F4B-E09, pregunta 3)

- **Declaración:** «Un rack de un solo sentido (`!design.IsComposite`) cuya `Structure` guarda módulos o líneas con ids de cola (`PushBackCompositeStructure.IsCompositeTailId`) **no es un supported classic rack** para la garantía sin pérdidas de PB-T1.»
- **`public static PushBackEditableSupport SupportOf(PushBackDesign design)`**, con el enum `PushBackEditableSupport { Supported, DormantCompositeTail }`, en el mismo archivo. Es la misma condición con la que el piloto rechaza (PM:160-170).
- **Lo que no cambia:** RACKEDITAR sigue como hoy (pierde las 667 hojas al Actualizar, CT-PB0-01); el Workspace conserva `DormantTailOutOfPilotReason` en todo F4B (OB-03.1); guardar el lado dormido sigue siendo la deuda H14.
- **La prueba** es CT-PBT1B-04 (§5.3).

### D-PBT1-11. El criterio de bloqueo de salida, extraído

- **Dónde:** `src/RackCad.Application/Systems/PushBack/PushBackEditorOutputGate.cs`, estático:
  - `IReadOnlyList<PushBackCompositeDiagnostic> Diagnostics(PushBackCompositeEditorState composite, PushBackSystem system)` = `composite.IntentDiagnostics().Concat(PushBackCompositeDiagnostics.Evaluate(system)).ToList()` (WC:783-786);
  - `bool IsBlocked(…)` = algún `IsBlocking` (WC:806);
  - `string StatusOr(…, string fallback)` = el primer bloqueante, si no el primer aviso, si no `fallback` (WC:789-801).
- **Lo que no cambia:** `RackBomOutputGate` y `PushBackCompositeDiagnostics`.
- **Quién lo usa:** la ventana desde PB-T1c (D-PBT1-13) y la barrera de diagnósticos del PREPARE de PB-T3, que lo consume sin duplicarlo ([P] §5).

### D-PBT1-12. M-07 de PB-TC2 y los demás arrastres

- **M-07 se reproduce tal cual.** La caja vacía que no surte efecto con el indicador obsoleto no pierde estado (HT1-18): el estado sigue siendo la autoridad y la caja no consigue editarlo. Por OB-RE.1, RACKEDITAR no cambia; corregirlo sería una decisión nueva del Owner, que este paquete no propone. CT-PBT1A-10 lo fija como «INCUMPLIMIENTO CARACTERIZADO» y PB-T1c no toca `OptionalVal` ni el estado mixto.
- **M-03 de PB-TC y de PB-TC2, D-5, M-04, la observación de CT-16 y M-02 de PB-TC** entran en la red de PB-T1a (§5.2).
- **M-01 y M-05 de PB-TC:** las pruebas nuevas usan `var w = Ventana(); try { carga } finally { Cerrar(w); }` y confirman o cancelan los módulos en un `finally` interno. S0 no cambia.
- **D-3:** CT-PBT1A-13 en la ventana y CT-PBT1B-05 en el estado.

### D-PBT1-13. La ventana sobre el estado (PB-T1c): un cambio mínimo, sin MVVM ni estética

**Regla general:** cada campo que pasa al estado se queda con su **mismo nombre** como reenvío privado. Así el código que lo usa, los literales de T-SG y las costuras no cambian.

1. **El estado:** en lugar de `composite` (W:56-57), `private readonly PushBackEditableState editable = new PushBackEditableState();` y `private PushBackCompositeEditorState composite => editable.Editor;`.
2. **Los reenvíos**, en el sitio de cada campo y con su comentario:
   - `private List<SelectiveSafetySelection> safetySelections => editable.SafetySelections;` (W:84);
   - `private PalletSpecification generalPallet { get => editable.Pallet; set => editable.Pallet = value; }` (W:101);
   - `rackTolerance`, `rackPostPeralte`, `rackBeamDepth` y `rackAnnotationScale`, con `get`/`set` sobre `editable.PalletTolerance`, `PostPeralte`, `BeamDepth` y `AnnotationScale` (W:106-109);
   - `private PushBackEditorInputs advanced => editable.Advanced;` (W:114);
   - `firstLevelDatum`, `loadedDimensionViews`, `loadedDimensionStyle` y `dimensionViewsTouched`, con `get`/`set` sobre `editable.FirstLevelDatum`, `LoadedDimensionViews`, `DimensionStyle` y `DimensionViewsTouched` (W:380, W:1249, W:1256, W:1260).
3. **Las cargas** (`LoadNew`, `LoadDesignForNew`, `LoadExisting` y `Restore_Click`): las líneas de Application se sustituyen por la llamada al estado, y lo de la ventana queda donde está:
   - `LoadNew`: W:273-276 → `var inputs = editable.LoadNew(catalog);`; W:286-287 → `editable.SeedDefaultSafety(catalog);`;
   - `LoadDesignForNew` y `LoadExisting`: W:299-302 y W:316-319 → `var inputs = editable.LoadDesign(design, rackName, assembler.Resolver);` seguido de `UpdateCompositeEnabled();`;
   - `Restore_Click`: W:3320-3327 → `var inputs = editable.Restore(design, assembler.Resolver);` seguido de `UpdateCompositeEnabled();`.

   El proyecto, el modo, la identidad y `LoadFromModel` siguen en el mismo orden (W:277-280, W:303-306, W:320-323, W:3328).
4. **`LoadFromModel`** (W:385-440) **solo pinta y recalcula:** se quitan las asignaciones a portadores (W:391, :393, :398, :401, :403, :406-411, :414, :419 y :423-427). **Las siembras de los controles no cambian ni un carácter** (W:390, :392, :394-397, :399-400, :402, :413, :415-418, :420-421), y `ShowDimensionViews` (W:1268-1284) sigue igual: vuelve a escribir en el estado los mismos dos valores que puso la carga.
5. **`ReadInputs`** (W:833-890) escribe en el estado lo que lee y devuelve `editable.BuildInputs(assembler)`. Cada lectura conserva **la expresión de hoy**:
   - la caja de fondo y la unidad van a `generalPallet` (la unidad, que hoy no tenía portador, se añade como quinto argumento de la `PalletSpecification` de W:838-840, con la expresión de W:852);
   - las cuatro de `Keep`, como W:841-844;
   - `editable.PalletsDeep = IntVal(PalletsDeepBox, DynamicRackDefaults.DefaultPalletsDeep);` (W:853);
   - `editable.PostCatalogId = PostBox.SelectedId ?? editable.PostCatalogId;` (W:868, con el respaldo de las vigas de W:823 y W:826, porque la regla de D-PBT1-09 ya dejó en el estado el poste que hoy pone DA:169-171);
   - las tres casillas, `Dimensions` y las tres casillas de cotas, con las expresiones de W:874-876, W:878 y W:1295-1297.

   `SelectedDimensionViews` (W:1291-1297), cuyo único uso es W:880, se retira: su regla vive en `BuildInputs`.
6. **`SideAOnly`** (W:342-373) y **`LoadCompositeFromDesign`** (WC:813-814, WC:827-934) se retiran: viven en el estado.
7. **El criterio de bloqueo:** `CompositeDiagnostics` y `CompositeStatusOr` delegan en `PushBackEditorOutputGate` con `composite` y `lastCompositeSystem`; `CompositeHasBlocking` guarda `lastCompositeSystem = system` y devuelve `PushBackEditorOutputGate.IsBlocked(composite, system)`.
8. **El nombre:** un método privado `SyncName()` = `editable.Name = session.Identity.Name;`, llamado inmediatamente después de cada `Adopt` y de cada `SetName` de W (W:279, :305, :322, :1237, :3352, :3405 y :3504). `session.Identity` sigue siendo el contrato de inserción (el sumidero, como `session.SetModel`), y los literales de CT-22 no cambian.
9. **La costura:** `internal PushBackEditableState EditableStateForTest => editable;`, junto a las demás (W:165-193).

**Fuera, por la letra:** `ReadCellValues`, `CommitCurrentCell`, `ForEachEditedSide`, `Keep`, `KeepCell`, `KeepOptional`, `MixedBoth`, `OptionalVal`, `IntVal`, el estado mixto, los manejadores de módulos, seguridad, topologías y estructura, los subdiálogos, `RequestDraw` (salvo `SyncName`), `SaveLibrary_Click` e `InsertBatch_Click` (salvo `SyncName`), WX y el resto de los miembros.

### D-PBT1-14. Lo que no cambia en todo PB-T1

- **Los constructores y las llamadas de los cuatro sitios** (OB-01.1b): `RackPushBackSystemWindow()` y `RackPushBackSystemWindow(bool canInsertInAutoCad, Func<string> newIdFactory = null)` (W:131-161), el Plugin y `EditorModules`.
- **`LoadExisting`, `LoadDesignForNew` y `LoadNew` conservan su firma pública** (W:270, :294, :311): el Plugin sigue cargando con `window.LoadExisting(project.PushBackDesign, embed.Id, embed.Name, project)` (T-PG CT-25), y la ventana construye el estado dentro.
- **El modo WORKSPACE no se añade:** ningún constructor, factoría ni tipo de modo nuevo. Es de PB-T2.
- **El piloto no se toca:** `Pilot/` entero, su copiador, su comparador, su censo, sus rechazos (OB-03.1) y su copia de la carga (PM:150-172). PB-T3 y PB-T4 pasan al estado de PB-T1 cuando generalizan `ComposeForWrite` y `KeepPilotEditsOn` (OB-05.2) y conectan «Editar todo».
- **`NumericField`, Cantilever, Domain, el Plugin, WX, `Workspace/` y `Drafts/`.**
- **Ninguna corrección de producto:** RACKEDITAR se comporta igual en cada gesto (OB-RE.1, D-16).

## 4. Las guardas de T1B y de texto (OB-T1B.3)

**Conclusión:** el diseño mantiene en verde todas las guardas, sin sustituir ninguna. **OB-T1B.3 no obliga a volver al Owner.**

| Guarda | Qué fija | ¿Depende del diseño? | Cómo queda en verde (invariante) |
|---|---|---|---|
| **T-SG S-9** (`…SourceGuardTests.cs:293-298`) | `UiSupport.PromptSaveToLibrary(this, NameBox.Text, "sistema")` | Sí | `SaveLibrary_Click` solo gana una línea `SyncName();` tras `SetName`; la llamada no cambia (INV-PBT1C-04) |
| **T-SG CT-22** (`:323-341`) | El orden `PromptSaveToLibrary` → `session.Identity.EnsureId();` → `session.Identity.SetName(NameBox.Text?.Trim());` → `var project = RackProject.ForPushBack(lastComputation.Design).WithSourceMetadataFrom(sourceProject);` → `Save` | Sí | `session`, `lastComputation` y `sourceProject` siguen siendo campos de W; las cinco sentencias no cambian y `SyncName();` va después de `SetName` (INV-PBT1C-04) |
| **T-PG CT-25** (`tests/RackCad.Tests/PushBackClassicCharacterizationPluginGuardTests.cs:235-272`) | La carga del Plugin con `window.LoadExisting(...)` y la lectura de la petición | Sí | El Plugin no se toca y la firma de `LoadExisting` no cambia (D-PBT1-14) |
| T-SG S-7 (`:280-284`) | `new RackBomWindow(lastComputation.Bom) { Owner = this }.ShowDialog();` | No | `Bom_Click` no cambia |
| T-SG CT-23 (`:356-379`) | El orden de `InsertBatch_Click` | Débil | Solo gana `SyncName();` tras su `SetName` (W:3352), que está antes de `RequestInsertViews`; el orden de las cuatro sentencias fijadas no cambia |
| T-SG CT-21 (`:302-319`) | El host del menú | No | `EditorModules` no se toca |
| T-SG CT-16, S-5, S-6, S-8 y el configurador | Los subdiálogos | No | No se tocan |
| T-SG CT-19 e `InsertRequestedRaised` (`:235-264`) | Ningún token de teclado, foco ni `DialogResult` bajo `src/RackCad.UI/Systems/PushBack` | Débil | El diff de PB-T1c no añade ninguno de los nueve tokens, ni en comentarios; el tipo nuevo vive en Application (INV-PBT1C-05) |
| **G-10** (`tests/RackCad.Tests/Workspace/WorkspacePilotUpdateIntegrationGuardTests.cs:721-797`) | Exactamente una `.RequestUpdate(` en W y ninguna en archivos de `src` fuera de la lista | Sí | Ningún archivo nuevo de PB-T1b contiene `.RequestUpdate(`; W conserva la suya (W:3409) (INV-PBT1B-05, INV-PBT1C-05) |
| I-55 (`I55DeferredProductFixCharacterizationTests.cs:8-15`) | `PhysicalPostIndex(LateralSectionBox.SelectedItem)` y `corte.PostIndex == section` en W | No | `SelectedView` y `PlanFor` no se tocan |
| Fact3 (`PushBackModuleEditorCharacterizationTests.cs:381-401`) | W con `RequestStandardRestore()` y sin `forceRebuild:` ni `forceRebuild)`, en crudo; DA con `StandardRestoreRequested` | Débil | Ninguna línea nueva de W contiene `forceRebuild`; DA no se toca |
| Desviador (`BlankFrontDesviadorHandoffTests.cs:311-318`) | `desviadorLevelsPerPost: DesviadorLevelsPerPost()` y `showDesviadorSide: false` en W | No | La vista previa no se toca |
| T-KB, T-OW, T-RS, foco y `AnnotationScaleBox` | WX | No | WX no se toca |
| Censo de ventanas (`WindowCensusGuardTests.cs:86-120`) | Ningún `Window` nuevo | No | Ninguno |
| Corpus de WLC (`WorkspaceDraftPilotWindowLoadCharacterizationTests.cs:323-333`) | Ningún `pb-*.json` nuevo en `Fixtures/I64` | Restricción | Los diseños nuevos se fabrican en código |
| RAM Elements (`StructuralAnalysisRamElementsHardeningGuardTests.cs:84-92`) | Los literales de `tests/RackCad.Tests` | Restricción | Las pruebas de Core nuevas no parten ninguna palabra de ese dominio |
| Núcleo de borradores (`WorkspaceDraftPilotIntentGuardTests.cs:232-247`) | `Drafts/` sin `PushBack` | Restricción | `Drafts/` no se toca |
| Espacio de nombres = carpeta (`tests/RackCad.Tests/NamespaceFolderGuardTests.cs`) | Un solo `namespace` de bloque por archivo de `src/`, igual a su carpeta; un espacio por proyecto de prueba | Restricción | INV-PBT1B-04b |
| **El censo del piloto** (HT1-21; `WorkspaceDraftPilotStateTests.cs:204-537`, `:955-982`) | Todo campo de instancia de su grafo, clasificado | **Sí** (no la lista el expediente) | Ningún campo nuevo en CS, ES, `PushBackEditorInputs`, `PalletSpecification`, `DynamicAnnotationOptions` ni en los tipos de seguridad; solo métodos nuevos en ES y RMES (INV-PBT1B-04) |
| Las costuras internas (W:165-193, WC:43 y las de T-7 de PB-T0) | Las usan T1B, PB-T0, PB-TC y PB-TC2 | Sí | Se conservan todas, como reenvíos; se añade `EditableStateForTest` (INV-PBT1C-06) |

**Para PB-T2, informativo** (expediente §4): G-10, T-SG CT-23 y T-PG CT-25 son las que probablemente salten al separar aplicar, cerrar y escribir. Esa vuelta es de PB-T2.

## 5. La red de pruebas

### 5.1 Lo que tiene que seguir en verde sin tocarlo (las tres subtareas)

- **T1B:** 55 en UI y 10 en Core, sin sustitución (OB-T1B.3).
- **PB-T0:** 16 en UI (y CT-PB0-05 y CT-PB0-19 con su `Skip` de SUS-03 y SUS-04) y 3 en Core.
- **PB-TC:** sus 10 métodos; **PB-TC2:** sus 4.
- **Las demás de la ventana:** todas las clases `PushBack*` de UI (538 casos estáticos) y las de Core que leen la ventana (Fact3, I-55, desviador).
- **El piloto:** `RackCad.Tests.Workspace` entero, censo incluido (HT1-21).
- **Cantilever:** 67 (OB-06.6), porque `NumericField` sigue fuera.
- **PB-T1a**, una vez cerrada, es red de PB-T1b y PB-T1c; **PB-T1b**, de PB-T1c.

### 5.2 PB-T1a: la red de caracterización (lista cerrada CT-PBT1A-01 a 14)

**Régimen** (el de PB-T0 y T1B, copiado en el contrato y no heredado):
- se fija lo que hay. Las líneas esperadas **las mide el Worker en la `BaseSha`** con código temporal que no entra en ningún commit, y las transcribe; si algo contradice el diseño, la línea lleva el comentario «INCUMPLIMIENTO CARACTERIZADO»;
- el RED son las pruebas nuevas **y los mutantes de la tabla cerrada**, todos juntos, en W y WC; el GREEN revierte los mutantes con `git restore --source=<BaseSha>`. Cada mutante hace fallar, solo y por aserción, el CT de su fila;
- ninguna prueba existente se edita, se reactiva, se salta ni se borra;
- toda prueba existente que falle en el RED se atribuye a un mutante (tabla T-8, como PB-T0);
- ventanas con `Ventana()`, carga dentro del `try` y `Cerrar` en `finally`; ninguna mostrada (salvo la de botas de CT-11, fuera de pantalla con `AbrirFueraDePantalla` de S0, como CT-PB0-14);
- **el aviso de descarte, sustituido:** cada método corre dentro de `using (EditorDiscardPrompt.Substitute(_ => { descartes++; return true; }))` y la última línea de cada ventana termina en ` descartes=<n>`, leído tras cerrarla (la regla de D-PBTC2-08);
- sin teclado ni foco real; prohibidos un `MessageBox` real, el BOM, «Guardar en biblioteca», el configurador real e «Insertar»;
- números con «R», cultura invariante, GUID fijo, orden ordinal; los JSON se leen con la navegación de SC1.

**Las clases** (en `tests/RackCad.UI.Tests/`, `public sealed`, `[Collection(EditorDiscardPromptCollection.Name)]`, métodos `[Fact]` que acaban en **un** `Assert.Equal(<lista esperada>, <lista>)`): `PushBackStateNetRackControlsTests` (01, 02), `PushBackStateNetCellAndFrontTests` (03, 04, 07, 08), `PushBackStateNetCompositeTests` (05, 09, 10), `PushBackStateNetModuleAndSafetyTests` (06, 11), `PushBackStateNetActionsTests` (12, 13) y `PushBackStateNetFingerprintTests` (14). El ayudante `PushBackStateNetSupport` (`internal static`, sin pruebas) solo fabrica diseños en código y arma las huellas.

| CT | Qué fija (sobre `Salida` o `LastComputation.Design` persistido, nunca sobre el estado privado) | Origen | Mutante |
|---|---|---|---|
| **01** | Las tres casillas de anotación, `DimensionsBox` y las tres casillas de cotas: cada una cambiada sola → `Structure.NumberFronts`, `NumberLevels`, `DrawRackName`, `Dimensions` y `DimensionViews` de la salida; un diseño con `DimensionViews` nulo sigue nulo tras una pasada sin tocarlas, y tocar una escribe el valor de I-50 | D-5 (H-029); expediente §3.1 | MU-T1A-01 |
| **02** | `WeightUnitBox` a «lb» → `Pallet.WeightUnit`; la carga con «ton» (c-09) la conserva y Actualizar la escribe; `PalletsDeepBox` editado → `PalletsDeep` | D-5 (c-09); T-1 «hueco de escritura» | MU-T1A-02 |
| **03** | `PositionsBox`, `LevelsBox`, `FondosBox` y `DepthStartBox` editados en un frente → `Structure.Fronts[f].*`; lo mismo en un compuesto con «Ambos lados», en `Structure.Fronts` y `SideB.Fronts` | D-5 (H-030) | MU-T1A-03 [TC2] |
| **04** | `CellIntermediatePeralteBox`, `CellFondoOverrideBox` (`LostFocus`) y `CellDrawPalletCheck` → la celda de la salida (`IntermediateBeamDepth`, `Fronts[f].PalletsDeepOverrides`, `Fronts[f].DrawPallets`) | D-5 (H-030, H-031) | MU-T1A-04 [TC2] |
| **05** | `CentralSeparatorCheck`, `StructureOverrideBox` con «Aplicar» y «Restaurar estructura», la topología, el sentido y el alcance con «Aplicar topología», y `FrontCountBox` → `Composite.*` y `Structure.Fronts.Count` | D-5 (H-031) | MU-T1A-05 |
| **06** | `DerivedPostLineHeightBox` aplicado a una línea: nada en la salida hasta «Confirmar»; tras confirmar, `Structure.DerivedPostLineOverrides`; cancelar no deja nada | D-5 (H-031) | MU-T1A-06 |
| **07** | **M-03 de PB-TC:** un valor exacto sin editar (61,23456) llega exacto a las demás celdas por el alcance «Todas», a los demás frentes por «Aplicar a todos los frentes» y tras «Restaurar»; y el **caso negativo de D-PB-11:** «Restaurar módulo» individual con «Confirmar» no reinicia los avanzados | M-03 de PB-TC | MU-T1A-07a [TC2], MU-T1A-07b |
| **08** | **M-03 de PB-TC2:** con B presente y «Ambos lados», una caja mixta escrita llega a los dos lados por el alcance (W:1346) y por los datos del frente (W:1510); sin escribirla, cada lado conserva el suyo | M-03 de PB-TC2 | MU-T1A-08 |
| **09** | **M-04:** «+ frente», «- frente» y «En blanco», con y sin B, fijados en la salida: `Structure.Fronts.Count`, `SideB.Fronts.Count`, `Composite.AbsentSlotsA` y `Composite.AbsentSlotsB` | M-04 de PB-T0 | MU-T1A-09 |
| **10** | **M-07, INCUMPLIMIENTO CARACTERIZADO:** el camino de H2-11 (Ambos con A ≠ B, retirar B, clic en la celda) y después vaciar «Largo manual» y salir: la salida conserva el override; una caja obligatoria vacía no marca error | M-07 de PB-TC2 | MU-T1A-10 [TC2] |
| **11** | **La observación de CT-16:** aceptar Seguridad sin cambios sobre `pb-05-topes` añade el portador de botas: se fijan `Structure.SafetySelections` de la salida y las líneas del BOM de `LastComputation.Bom` (descripción y cantidad, en el orden producido), antes y después | CT-16 de PB-T0 | MU-T1A-11 |
| **12** | **M-02 de PB-TC:** `IsEnabled` de los once botones de acción (Insertar, Actualizar, BOM, Guardar en biblioteca, Restaurar, los cuatro por vista, Insertar varias y Cerrar) con el aviso de D-PB-08 visible y sin él, sobre el mismo rack | M-02 de PB-TC | MU-T1A-12 |
| **13** | **D-3 y M-01:** `LoadExisting` con `PalletTolerance = 0` lanza; se fija el tipo y el mensaje, y que la ventana creada se cierra sin dejar viva ninguna (con `Ventana()` y `finally`) | D-3, M-01 de PB-T0 | MU-T1A-13 |
| **14** | **Las huellas de la ventana:** para cada diseño del corpus extendido (los 17 de `Fixtures/I64`, X-01 de S0, **F-01**: `pb-20-ab` con un módulo de A y uno de `B:` personalizados y confirmados, reabierto, y **F-02**: `pb-20-ab` con «Frontal de B» elegido) y cada paso de los guiones G-1 a G-6, la línea `<diseño> <paso> valido=<b> bloqueado=<b> sha256=<huella de Persist(LastComputation.Design)>`. Guiones: G-1 cargar; G-2 Actualizar (segunda pasada); G-3 «Restaurar»; G-4 retirar B y volver a declararlo (solo compuestos); G-5 «Restaurar estándar» y «Confirmar»; G-6 escribir 61.5 en `CellPalletHeightBox` y aplicar a «Todas» | La equivalencia de PB-T1c (§5.4) | MU-T1A-14 |

**La tabla cerrada de mutantes** (W y WC; cada uno es la sustitución exacta de una línea, en la `BaseSha`; las marcadas [TC2] se vuelven a medir sobre el GREEN de PB-TC2 y, si su texto no es el de abajo, es PBT1A-C07):

| Id | Miembro | Línea de hoy | Sustitución |
|---|---|---|---|
| MU-T1A-01 | `ReadInputs` | W:874 `NumberFronts = NumberFrontsCheck.IsChecked == true,` | `NumberFronts = NumberLevelsCheck.IsChecked == true,` |
| MU-T1A-02 | `ReadInputs` | W:852 `generalPallet.Weight, WeightUnitBox.SelectedItem as string ?? "kg"),` | `generalPallet.Weight, "kg"),` |
| MU-T1A-03 [TC2] | `ReadCellValues` | W:817 `DepthStartPosition = IntVal(DepthStartBox, front.DepthStartPosition),` | `DepthStartPosition = front.DepthStartPosition,` |
| MU-T1A-04 [TC2] | `ReadCellValues` | W:827 `IntermediateBeamDepth = SelectedPeralte(CellIntermediatePeralteBox, cell.IntermediateBeamDepth)` | `IntermediateBeamDepth = cell.IntermediateBeamDepth` |
| MU-T1A-05 | `CentralSeparator_Changed` | WC:209 `composite.SetCentralSeparator(CentralSeparatorCheck.IsChecked == true);` | `composite.SetCentralSeparator(false);` |
| MU-T1A-06 | `ApplyDerivedPostLines_Click` | W:2133 `var height = DerivedPostLineHeightBox.Value;` | `var height = (double?)null;` |
| MU-T1A-07a [TC2] | `Keep` | W:3686 `=> !field.Value.HasValue \|\| SameShown(field.Value.Value, current) ? current : field.Value.Value;` | `=> !field.Value.HasValue ? current : field.Value.Value;` |
| MU-T1A-07b | `Recompute` | W:696 `var standardRestoreLanded = moduleOwner.ModuleCommit?.StandardRestoreRequested == true; // I-64 F4B PB-TC (D-PB-11)` | `var standardRestoreLanded = moduleOwner.ModuleCommit != null;` |
| MU-T1A-08 | `ApplyFrontData` | W:1510 `ForEachEditedSide(side => side.Structure.ApplyFrontValuesTo(ReadCellValues(side).Dynamic, frontTargets));` | `ForEachEditedSide(side => side.Structure.ApplyFrontValuesTo(ReadCellValues().Dynamic, frontTargets));` |
| MU-T1A-09 | `AddFront_Click` | W:1171 `=> MutateStructure(() => composite.SetSlotCount(state.Structure.Count + 1));` | `=> MutateStructure(() => composite.SetSlotCount(state.Structure.Count + 2));` |
| MU-T1A-10 [TC2] | `OptionalVal` | W:3668 `: (IsMixed(field) ? sideValue : null);` | `: null;` |
| MU-T1A-11 | `Safety_Click` | W:2682 `ApplyBootSections(bootSectionA, bootSectionB);` | `;` |
| MU-T1A-12 | `UpdateButtons` | W:3568 `BomButton.IsEnabled = canOutput;` | `BomButton.IsEnabled = canOutput && SideBEmptyNoticeText.Visibility != Visibility.Visible;` |
| MU-T1A-13 | `LoadExisting` | W:313 `if (design == null) return;` | `if (design == null \|\| design.Structure?.PalletTolerance <= 0.0) return;` |
| MU-T1A-14 | `LoadFromModel` | W:419 `rackAnnotationScale = options.AnnotationScale > 0.0 ? options.AnnotationScale : 1.0;` | `rackAnnotationScale = 1.0;` |

Las sangrías son las de la línea original. Ningún mutante toca un literal de I-55, Fact3, el desviador, G-10 ni T-SG; por eso Core sigue en `success` en el RED.

**Lo que PB-T1a deja:** ninguna línea de `src/` cambia entre su `BaseSha` y su GREEN.

### 5.3 PB-T1b: el tipo (lista cerrada CT-PBT1B-01 a 13)

**Régimen** (el de F4-T2A, `I-64-f4-t2-gate-contract-package.md:282`):
- **El RED** es el **esqueleto inerte** de los tipos y métodos nuevos, que compila, no lanza y no cambia ningún comportamiento, más las pruebas nuevas. Todas fallan **por aserción**.
- **El esqueleto exacto:**
  - `PushBackEditableState`: todos los miembros de D-PBT1-02 con sus valores iniciales; las cargas y `Restore` devuelven `PushBackEditorInputs.NewDesign()` sin hacer nada más; `SeedDefaultSafety` no hace nada; `BuildInputs` devuelve `PushBackEditorInputs.NewDesign()`; `ResolvePostCatalogId` devuelve `requested`; `SupportOf` devuelve `Supported`; `SideAOnly` devuelve `design`;
  - `PushBackEditableStateComparer`: `Instance`; `Equals` = `ReferenceEquals(x, y)`; `GetHashCode` = 0; `Copy` = `new PushBackEditableState()`;
  - `PushBackEditableStateOwnership`: la tabla vacía; `Of` devuelve `Unclassified`;
  - `PushBackEditorOutputGate`: `Diagnostics` vacía; `IsBlocked` = false; `StatusOr` = `fallback`;
  - ES `CopyAuthorityFrom` no hace nada; RMES `Copy()` = `RackModuleEditSession.Begin(Array.Empty<DynamicRackModuleDesign>())`; `SameEditAs` = false; `RackModuleCommit.Copy()` = `this`; `SameAs` = false.
- **Sin excepciones en el RED:** cada paso que llama al API nuevo pasa por un ayudante privado de la clase que convierte una excepción en la línea `EXCEPCION <Tipo>: <Mensaje>`. Las líneas esperadas no contienen ninguna.
- **El GREEN** solo toca `src/` (los cuatro archivos nuevos, ES y RMES).
- **La ventana no cambia.**

**Las clases:** en `tests/RackCad.Tests/`, `PushBackEditableStateLoadTests` (02 a 05), `PushBackEditableStateCopyTests` (06 a 10 y 13), `PushBackEditableStateOwnershipTests` (11) y `PushBackEditorOutputGateTests` (12); en `tests/RackCad.UI.Tests/`, `PushBackEditableStateParityTests` (01), con `[Collection(EditorDiscardPromptCollection.Name)]`. Los diseños F-01 y F-02 se fabrican en código, como en PB-T1a.

| CT | Qué fija | Oráculo | Falla en el RED porque |
|---|---|---|---|
| **01** (UI) | **Paridad de las cargas** con la ventana de la base, sobre el corpus extendido de CT-PBT1A-14: `LoadNew` con sus dos pasadas y la seguridad entre ellas; `LoadDesign` como `LoadExisting` y como `LoadDesignForNew`; `Restore` tras cargar, tras retirar B (cola dormida) y tras `SetGap(60)` en el estado y en la ventana. Cada línea: `<diseño> <camino> iguales=<b> bloqueado-ventana=<b> bloqueado-estado=<b>`, comparando `Persist` del diseño de la ventana con el del estado calculado con el mismo catálogo (`w.Session.Catalog`) y `PushBackEditorOutputGate.IsBlocked` con `OutputIsBlockedForTest`. Incluye los casos c-01 a c-16 de CT-PB0-17 | la ventana de hoy | el esqueleto no carga |
| **02** | **La ida y vuelta sin pérdidas** de los racks soportados: diseño → estado → diseño, y estado → diseño → estado (igual para el comparador) | los 8 canónicos, `pb-20-ab`, `pb-31-v2-modulos`, X-01 y F-01: delta vacío (CT-PB0-01, `WorkspaceDraftPilotCanonicalLineEndingTests`); los 6 heredados: exactamente las líneas de `LegacyDeltas` (`WorkspaceDraftPilotMaterializationTests.cs:706-735`), copiadas | el esqueleto da el diseño por defecto |
| **03** | **c-02 como regla explícita:** `ResolvePostCatalogId` con un id del catálogo, el mismo con otras mayúsculas, uno inexistente, en blanco, solo espacios y nulo; y el `HeaderPostCatalogId` de la salida del estado para c-01 y c-02 | el paso 1 de PB-T1b (§6.2) para el id con otras mayúsculas; CT-PB0-17 para la salida | el esqueleto devuelve `requested` |
| **04** | **La exclusión de `pb-21-cola`:** `SupportOf` da `DormantCompositeTail` para `pb-21-cola` y `Supported` para los demás diseños del corpus extendido; la ida y vuelta de `pb-21-cola` pierde exactamente las 667 hojas de CT-PB0-01 (`delta-filtrado=667 sha256=0b54e8ff…`, copiado); el piloto sigue dando `OutOfPilot` con `DormantTailOutOfPilotReason` | CT-PB0-01; PM:108 | el esqueleto da `Supported` |
| **05** | **Los demás casos de T-2, tal cual:** c-01, c-03, c-04/c-06, c-05/c-07, c-08/c-09, c-10/c-11/c-16, c-14 y c-15 dan las mismas líneas que CT-PB0-17; c-12 lo rechaza el almacén; **c-13 lanza al cargar el estado el mismo tipo y el mismo mensaje** (D-3) | CT-PB0-17 (`PushBackFullEditorCharacterizationCatalogTests.cs`) | el esqueleto no carga ni lanza |
| **06** | **El censo:** todo campo de instancia del grafo de `PushBackEditableState` está clasificado en una de cinco clases (editable, selección, autoridad de módulos copiada, derivado compartido, transitorio del armado) y toda entrada existe; con un estado rico (F-01 con una edición escenificada, un commit y la cola dormida), cada campo editable sobrevive a `Copy` | el censo del piloto como plantilla (`WorkspaceDraftPilotStateTests.cs:204-537`), con tabla propia | `Copy` da un estado vacío |
| **07** | **Sin alias:** la copia es igual para el comparador y la intersección de `AllObjects` de la copia y del original, quitadas las cadenas, es exactamente el conjunto de los derivados compartidos y lo que ellos alcanzan; y el estado recién cargado no comparte ningún objeto con el diseño cargado (HT1-11, HT1-12) | — | `Equals` es `ReferenceEquals` |
| **08** | **Independencia:** mutar cada ranura editable de la copia deja el original igual (comparador y salida persistida), y al revés; ningún gesto del API (cargas, armado, `AcceptComputation`, sesión de módulos, en el original y en la copia) muta una línea base ya adoptada: su huella, `Persist(resolver.Snapshot(<línea base>))`, es la misma antes y después | — | `Copy` da un estado vacío |
| **09** | **Módulos, cola y A/B se conservan:** sobre F-01, F-02 y `pb-20-ab` con B retirado: la copia calcula el mismo diseño; volver a declarar B en la copia devuelve la cola intacta, igual que en el original; confirmar en la copia la edición escenificada da lo mismo que en el original | el original | `Copy` da un estado vacío |
| **10** | **El comparador:** dos cargas del mismo diseño son iguales; cada cambio del contenido editable, de la cola, de la sesión, del commit o de un portador las hace distintas; cambiar solo la selección no; `NaN` es reflexivo; `null` se trata como en el piloto | — | `Equals` es `ReferenceEquals` |
| **11** | **El mapa de propiedad:** toda ruta de hoja del sobre, del proyecto y de la carga útil de los 17 diseños persistidos está clasificada; `Envelope` = exactamente las de D-PBT1-08; `Name` del sobre es `Editor`; toda fila tiene fuente | D-PBT1-08 | la tabla está vacía |
| **12** | **El criterio de bloqueo:** sobre los sistemas de `PushBackOutputGateTests` (válido, bloqueante), un hueco negativo, un separador sin hueco y un override no construible, `IsBlocked` y `StatusOr` dan lo que daban `CompositeHasBlocking` y `CompositeStatusOr`; `RackBomOutputGate.For` no cambia | WC:783-807, copiado | `IsBlocked` es false |
| **13** | **`BuildInputs` es nuevo en cada llamada:** dos llamadas no comparten objetos; un «Restaurar estándar» confirmado (el `Reset` de DA:302) no toca `Advanced` del estado; los seis avanzados, la unidad y las casillas llegan a los inputs | D-PBT1-04 | `BuildInputs` da `NewDesign()` |

### 5.4 PB-T1c: la ventana sobre el estado (lista cerrada CT-PBT1C-01 a 03)

**Régimen:**
- **El RED** es la costura `internal PushBackEditableState EditableStateForTest => new PushBackEditableState();` en W (un estado ajeno a la ventana) y las pruebas nuevas, que fallan por aserción.
- **El GREEN** es D-PBT1-13 en W y WC.
- **La equivalencia la prueban las redes existentes sin tocarlas:** T1B, PB-T0, PB-TC, PB-TC2, las clases `PushBack*` y, sobre todo, CT-PBT1A-01 a 14 (sus huellas de CT-14 no pueden moverse).

| CT | Qué fija | Falla en el RED porque |
|---|---|---|
| **01** (UI) | La ventana edita el estado extraído: `ReferenceEquals(w.EditableStateForTest.Editor, w.CompositeState)`; y, tras `LoadNew`, `LoadExisting`, `LoadDesignForNew` y «Restaurar» sobre el corpus extendido, el estado de la ventana es igual (comparador de PB-T1b) al que da el estado solo con la misma carga | la costura da otro estado |
| **02** (UI) | **Ningún control es la fuente de verdad:** para cada control del censo (§5.5) y su gesto, tras el gesto, el diseño calculado desde una **copia** del estado de la ventana (`Copy`, `BuildInputs`, `BuildDesign`, `BuildFrom` con `FrontalSideForTest`) es igual, persistido, a `LastComputation.Design`; y `EditableStateForTest.Name` es igual a `RackName` | la costura da otro estado |
| **03** (Core) | **La ventana ya no guarda estado editable ni duplica Application:** en W y WC, sin comentarios, no hay declaraciones de campo con almacenamiento para los nombres de D-PBT1-13 (2), ni `SideAOnly(` como declaración, ni `LoadCompositeFromDesign`, ni `ParkDormantTail(`; `CompositeHasBlocking` contiene `PushBackEditorOutputGate.IsBlocked(`; W contiene `editable.BuildInputs(assembler)` y `EditableStateForTest => editable;` | la base declara los campos y no delega |

### 5.5 El censo control → prueba actualizado (T-1 de PB-T0 más lo de PB-T1)

| Control | Prueba que fija su efecto en la salida | Prueba de que el estado lo lleva (PB-T1c) |
|---|---|---|
| `DepthBox`, `ToleranceBox`, `PostPeralteBox`, `BeamDepthBox`, `AnnotationScaleBox` | CT-PB0-19 (SUS-03) y CT-PBTC-01 a 03 | CT-PBT1C-02 |
| `WeightUnitBox` | CT-PB0-17 (carga) y **CT-PBT1A-02** (escritura) | CT-PBT1C-02 |
| `PalletsDeepBox` | CT-PB0-04 y **CT-PBT1A-02** | CT-PBT1C-02 |
| `PostBox` | CT-PB0-17 (c-01, c-02) y **CT-PBT1B-03** | CT-PBT1C-02 |
| `NumberFrontsCheck`, `NumberLevelsCheck`, `DrawRackNameCheck` | **CT-PBT1A-01** (antes, hueco) | CT-PBT1C-02 |
| `DimensionsBox` | CT-PB0-17 (c-10, c-11, c-16) y CT-PBT1A-01 | CT-PBT1C-02 |
| Las tres casillas de cotas | `PushBackDimensionViewsWindowTests` y **CT-PBT1A-01** | CT-PBT1C-02 |
| `PositionsBox`, `LevelsBox`, `FondosBox`, `DepthStartBox` | **CT-PBT1A-03** (antes, hueco) | CT-PBT1C-02 |
| `FirstLevelHeightBox` y las cuatro cajas de la celda | CT-PB0-19 (SUS-03), CT-PBTC-01 a 03 y CT-PBT1A-07 | CT-PBT1C-02 |
| `CellInOutBeamBox`, `CellInOutPeralteBox`, `CellIntermediateBeamBox` | CT-PB0-17 (c-04 a c-07) | CT-PBT1C-02 |
| `CellIntermediatePeralteBox`, `CellFondoOverrideBox`, `CellDrawPalletCheck` | **CT-PBT1A-04** (antes, hueco) | CT-PBT1C-02 |
| `CellBeamLengthOverrideBox` | CT-PB0-19 (SUS-03), CT-PBTC2-04 y **CT-PBT1A-10** (M-07) | CT-PBT1C-02 |
| `RearPeralteBox` | CT-PB0-17 (c-03) | CT-PBT1C-02 |
| `GapBox` | CT-PB0-20 (carga) y CT-PBTC2-01 (manejador) | CT-PBT1C-02 |
| `CentralSeparatorCheck`, `StructureOverrideBox`, `CellTopologyBox`, `RunDirectionBox`, `TopologyScopeBox`, `FrontCountBox` | **CT-PBT1A-05** (antes, hueco) | CT-PBT1C-02 |
| `NameBox` | CT-PB0-11 | CT-PBT1C-02 (`Name` = `RackName`) |
| Las cajas avanzadas | CT-PB0-05 (SUS-04), CT-PBTC-14 y CT-PBTC2-03 | CT-PBT1C-02 |
| `ModuleLengthBox` | CT-PB0-03, CT-PB0-04 y CT-PBTC2-02 | CT-PBT1C-02 |
| `DerivedPostLineHeightBox` | **CT-PBT1A-06** (antes, hueco) | CT-PBT1C-02 |
| `SideBPresentCheck`, `SideSelectorBox` | CT-PB0-06 a CT-PB0-09 y **CT-PBT1A-09** (en la salida) | CT-PBT1C-02 |

**Huecos que quedan:** ninguno de los 20 de D-5.

### 5.6 Trazabilidad (obligación → CT)

| Obligación | CT |
|---|---|
| «module state preserved» | CT-PBT1B-06, 08 y 09; CT-PBT1A-06; CT-PBT1C-02 |
| «parked queue preserved» | CT-PBT1B-09 (cola dormida en sesión) y 02 (X-01); CT-PBT1A-14 (G-4) |
| «A/B preserved» | CT-PBT1B-02 (`pb-20-ab`, X-01, F-01) y 09; CT-PBT1A-03, 08 y 09 |
| «no aliasing» | CT-PBT1B-07, 08 y 13; L-1 del §8 |
| «no WPF control becomes source of truth» | CT-PBT1C-02; CT-PBT1B-03 (c-02); §5.5 |
| «state round-trip lossless for supported classic racks» | CT-PBT1B-02 y 04 |
| «classic RACKEDITAR still works through the extracted state» | CT-PBT1C-01; las redes de §5.1 y CT-PBT1A-14 en verde sobre PB-T1c |
| OB-05.1 | CT-PBT1B-11 |
| El criterio de bloqueo para PB-T3 | CT-PBT1B-12 y CT-PBT1C-03 |
| c-02 (F4B-E09 P2) | CT-PBT1B-03 y 01 |
| `pb-21-cola` (F4B-E09 P3) | CT-PBT1B-04 |
| T-2 tal cual; D-3 | CT-PBT1B-05; CT-PBT1A-13 |
| D-5; M-03 de PB-TC y de PB-TC2; M-04; M-07; CT-16; M-02 de PB-TC | CT-PBT1A-01 a 12 |
| OB-03.1 (el rechazo del piloto sigue) | CT-PBT1B-04 y el filtro `Workspace` en verde |
| OB-T1B.3 | §4; INV-PBT1x-07 |

## 6. Los contratos propuestos (patrón de PB-TC y PB-TC2)

### 6.0 Común a las tres subtareas

**Identidad:** `Schema` `rackcad-gate-contract/v1`; `Unit` I-64; `Gate` F4B; attempts 1 de 3 (OB-10.1); `RoutingEnforcement` `required`; `CorrectionsAuthorized` true. Evidencia en `docs/automation/evidence/I-64-pilot/<TaskId>/`.

**Anclajes:** antes de emitir cada subtarea, la sesión vuelve a medir sobre su `BaseSha` los anclajes que la subtarea usa, con las órdenes del registro, y lo deja escrito en el commit de emisión:
- PB-T1a: los quince textos de la tabla de mutantes y las zonas [TC2];
- PB-T1b: `git diff <cierre de PB-T1a> -- src` vacío;
- PB-T1c: `git diff <cierre de PB-T1b> -- src/RackCad.UI` vacío y los anclajes de D-PBT1-13.

Si un anclaje no se sostiene de un modo que cambia un CT o el diseño, STOP y vuelta a la sesión: se rehace la parte afectada del paquete y pasa por una comprobación acotada.

**`ForbiddenWriteScope`:** con el método de [PKC-C] (hermano por hermano; un prefijo solo si ninguna ruta permitida queda debajo), calculado por el generador de cada contrato con `assert` sobre los recuentos, la existencia en HEAD, la cobertura de `src/` y la no existencia de las rutas nuevas (medición J):
- `src/`: 59 en PB-T1a y PB-T1c (la forma de PB-TC2, con WX), 117 en PB-T1b;
- `tests/`: en PB-T1a, `tests/RackCad.Tests/` entero; en PB-T1b y PB-T1c, que escriben pruebas nuevas en `tests/RackCad.Tests/`, la lista curada de PB-T0 (18 entradas, con `Fixtures/`, `Workspace/`, `StructuralAnalysis/`, `RackCad.Tests.csproj`, `RackModuleEditSessionTests.cs`, `NamespaceFolderGuardTests.cs` y las guardas de la ventana y del Plugin) más las clases de Core de PB-T0 y, en PB-T1c, las de PB-T1b. En `tests/RackCad.UI.Tests/`, la lista curada de [PKC2] (51 archivos y `Workspace/`) **más** las clases de PB-TC2 y, en PB-T1b y PB-T1c, las de las subtareas anteriores;
- la raíz, las 17 entradas de [PKC2].

El orden exacto es la salida del generador con `--dry --print`.

**`StopConditions` estándar** (17), copiadas de [PKC2] §6.7: S-02, S-03, S-04, S-05, S-07, S-08, S-12, S-13 y S-14, y P-01 a P-08, con P-01 = «config.toml no longer matches the accepted Snapshot in force (today Snapshot I, docs/automation/evidence/I-64-pilot/F4-ENVIRONMENT/snapshot-I/snapshot-i.json)».

**Las de [DP], literales, en cada contrato** (`PBT1x-C08`, `PBT1x-C11` y `PBT1x-C12`, con x = A, B o C):
- **PBT1x-C08** ([DP], pregunta 2): una sola replanificación, y solo si el rechazo de la planificación es **solo** por la comparación literal de las colecciones, con el prompt reforzado y sin gastar intento; un segundo rechazo de fidelidad, o cualquier otro rechazo (A1-A8 u otra causa), es STOP y vuelve al Owner.
- **PBT1x-C11** ([DP], pregunta 1). Un control que no da CONTROL PASS se conserva tal cual, y solo hay dos salidas sin volver al Owner:
  - **nc2 con la firma exacta de PD-F4-01.** El texto literal de su fuente (`f4-dispositions.md:1103-1106`):

    > «- Scope en fail sobre la ruta quitada;
    > - Contract en fail solo por la cláusula de exactitud;
    > - `EXECUTION_BLOCKED`/STOP.»

    Y su aplicación literal (`:1540`): «ninguna otra comprobación distinta de la verificación real».

    **Qué cuenta como la cláusula de exactitud:** forma parte de la firma que la evidencia de Contract cite el contraste del `AllowedWriteScope` estrechado con el contrato o con `byte-fidelity-check.json` (`f4-dispositions.md:520`, `:562`). No forma parte que Contract cite cualquier otra discrepancia.

    → El sustituto **`PBT1x-NC2-EXACT-WHITELIST-01`**: copia de `F4T1B-NC2-EXACT-WHITELIST-01` (`docs/automation/evidence/I-64-pilot/F4-T1B-PILOT-CHARACTERIZATION-nc2-substitute/1/R20261114T235423Z-083e/nc2_exact_whitelist_substitute.py`) con **solo** el identificador y el ámbito cambiados. Registro `nc2 = SATISFIED_UNDER_PBT1x_EXACT_WHITELIST_SUBSTITUTE`.
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

    → El sustituto **`PBT1x-NC1-IDENTITY-SIGNAL-01`**: copia de `F4T3C2-NC1-IDENTITY-SIGNAL-01` (`docs/automation/evidence/I-64-pilot/F4-T3C2-COMMAND-ADVISOR-nc1-substitute/1/R20261113T215949Z-935f/nc1_identity_signal_substitute.py`) con **solo** el identificador y el ámbito cambiados. Registro `nc1 = SATISFIED_UNDER_PBT1x_NC1_IDENTITY_SIGNAL_SUBSTITUTE`.
  - **Cualquier otra firma o variación vuelve al Owner antes de cualquier sustituto.** Por ejemplo: PD-F4-05 y PD-F4-06; nc2 con Ci o Trailer en fail además; cualquier nc1 que no reproduzca la firma literal, **como el de PB-TC** (Remote, Scope, Tests y Trailer también en fail; R-T1-08); cualquier fallo de nc3. La disposición «Aceptarlo solo esta vez» de nc1 en PB-TC (F4B-E08) **no** se extiende a PB-T1.
  - **Cómo se decide la coincidencia.** Nunca por el nombre de la comprobación: se compara la salida del control con la de la verificación real, comprobación por comprobación (el conjunto en fail, la primera en fail, el `Outcome`, la `StopClass` y el texto de la evidencia). Las únicas diferencias admitidas: en PD-F4-01, Scope (en fail sobre la ruta quitada) y Contract (solo por el alcance); en PD-F4-04, Identity, Handoff (la primera en fail) y Ci (solo como consumidor del `CurrentSha` falso); en las dos, `EXECUTION_BLOCKED` / STOP. La sesión registra la tabla en `f4b-dispositions.md`.
- **PBT1x-C12:** una corrección sería el intento 3. Vuelve al Owner antes de emitirse (D-PB-10).

**`EligibleCells`:** las de [PKC2]: Worker `anthropic/subagent/claude-sonnet-5-5`, effort `high` (VerifiedOn 2026-09-30); Controller `openai/process/gpt-6-luna`, effort `high` (VerifiedOn 2026-11-11). Las dos son `STALE` desde el **2026-12-29** (`model-catalog.md:14`). nc4 en la primera delegación de cada cadena; nc1 a nc3 tras la verificación.

**Invariantes comunes** (cada contrato los numera como INV-PBT1x-07 a 12):
- **07. Las redes:** T1B (55 y 10), PB-T0 (16 y 3), PB-TC (10), PB-TC2 (4), Cantilever (67), las guardas de la tabla del §4, el censo del piloto y las subtareas anteriores, en verde; ninguna prueba existente se edita, se salta, se reactiva ni se borra (salvo lo que diga cada contrato).
- **08. Las pruebas de UI:** las clases nuevas en `EditorDiscardPromptCollection`; `Ventana()`, la carga dentro del `try` y `Cerrar` en `finally`; ninguna ventana mostrada, salvo CT-PBT1A-11; cada método dentro de `EditorDiscardPrompt.Substitute` con su contador y el sufijo ` descartes=<n>` en la última línea de cada ventana, leído tras cerrarla; sin teclado ni foco real; prohibidos un `MessageBox` real, el BOM, «Guardar en biblioteca», el configurador real e «Insertar»; ninguna aserción dentro de una costura; ningún fixture nuevo en `Fixtures/I64`.
- **09. Determinismo:** el GUID fijo, la cultura invariante, «R», el orden ordinal; ni hora ni aleatoriedad.
- **10. Las corridas locales:** solo los filtros del contrato en UI, **nunca la suite completa de UI en local** (F4-E59); la de Core, sí; sin copias de árboles (MAX_PATH); las corridas temporales terminan en `Assert.Fail(string.Join("\n", lineas))`, no entran en ningún commit y guardan su TRX y su log como artefactos; el perfil del usuario no cambia (INV-PBT0-06).
- **11. La evidencia:** toda ruta de `worker-handoff.json` es relativa y se comprueba que existe; `TreeSha` es el árbol real (`git write-tree`) o null; el Worker no escribe en `docs/`; las sustituciones, si las hubiera, se llamarían `SUS3A-0n`, `SUS3B-0n` o `SUS3C-0n` y son STOP (`PBT1x-C02`).
- **12. Las reglas de F4B y sin AutoCAD:** la planificación según C08, los controles según C11, el intento 3 según C12; ninguna corrida de AutoCAD.

**Las propias comunes** (cada contrato las numera con su prefijo):
- **C01:** hacerlo bien exigiría escribir fuera del `AllowedWriteScope`, en particular `NumericField`, WX, el Plugin, `EditorModules`, `Pilot/`, `Drafts/`, Domain o un `.csproj`.
- **C02:** habría que editar, saltar, reactivar o borrar una prueba o un ayudante existente, o tocar `InternalsVisibleTo`, los paquetes o `Fixtures/I64`. Una guarda de T1B o de texto, el censo del piloto o una prueba de PB-T0, PB-TC o PB-TC2 que no queda en verde **vuelve al Owner** (OB-T1B.3).
- **C03:** aparece un cambio observable de RACKEDITAR: una diferencia en la salida, la petición, un rótulo, un control o un estado de la ventana frente a la base, o un fallo de una prueba existente en el GREEN. STOP y vuelta al Owner (D-16, OB-RE.1).
- **C05:** un CT necesitaría AutoCAD, un `MessageBox` real, el configurador real, el BOM, «Guardar en biblioteca», «Insertar», el teclado o el foco real, o escribir en el perfil o en el disco.
- **C06:** la CI del GREEN falla fuera de las pruebas nuevas, se reproduce y no es atribuible a ellas.
- **C07:** los hechos del §2 no se sostienen de un modo que cambia un CT o el diseño; o una línea esperada no se cumple en el GREEN. El Worker no ajusta lo esperado.
- **C09:** una prueba colgaría, dejaría viva una ventana, un modal o un dispatcher, escribiría en el perfil o dependería de rutas de una máquina.
- **C10:** hacerlo bien exigiría corregir algo fuera del alcance: una pérdida de T-2, M-07, `pb-21-cola`, el modo WORKSPACE, el piloto, D-PB-07 o una corrección de PB-TC o PB-TC2.

### 6.1 Contrato de PB-T1a — `F4B-PBT1A-LOSSLESS-STATE-NET`

- **Objective** (ASCII): «Fijar con pruebas de caracterizacion nuevas, sin cambiar el producto, la red de la ventana clasica de Push Back que PB-T1 necesita antes de mover su estado: los huecos de D-5, M-03 de PB-TC y de PB-TC2, M-04, M-07, la observacion de CT-16, M-02 de PB-TC y D-3, y las huellas de salida del corpus extendido por los guiones G-1 a G-6, segun el paquete del contrato de PB-T1 v1 y la lista cerrada CT-PBT1A-01..14. El RED son las pruebas nuevas y los mutantes de la tabla cerrada MU-T1A-01..14 en W y WC; el GREEN revierte los mutantes. Ninguna linea de src cambia entre BaseSha y CurrentSha; no se ejecuta AutoCAD.»
- **`AllowedWriteScope`** (9): W y WC (solo para los mutantes del RED); los siete archivos nuevos `tests/RackCad.UI.Tests/PushBackStateNet{Support,RackControlsTests,CellAndFrontTests,CompositeTests,ModuleAndSafetyTests,ActionsTests,FingerprintTests}.cs`.
- **`ForbiddenWriteScope`:** 59 de `src/` y lo común.
- **`RequiredTests`:**

  | Proyecto | Filtro (`FullyQualifiedName~RackCad.<proyecto>.`…) | `MinSelected` | `ExpectRed` |
  |---|---|---|---|
  | UI | `PushBackStateNet` | **14** | true |
  | UI | `PushBackClassicCharacterization` | 55 | false |
  | UI | `PushBackFullEditorCharacterization` | 16 | false |
  | UI | `PushBackDeclaredCorrectionRoundingTests`, `…DimensionStyleTests`, `…PilotParityTests`, `…SideBNoticeTests`, `…StandardRestoreTests` (uno por clase) | 3, 2, 2, 2, 1 | false |
  | UI | `PushBackDeclaredCorrection2` | 4 | false |
  | UI | `Cantilever` | 67 | false |
  | UI | `PushBackCompositeWindowTests`, `PushBackCompositeLifecycleWiringTests`, `PushBackCompositeLoadExistingModulesTests`, `PushBackBothModeCellBeamOverrideTests`, `PushBackModuleEditorWindowTests`, `PushBackAdvancedRackParametersWindowTests` | 52, 7, 3, 10, 13, 9 | false |
  | Core | `PushBackClassicCharacterization` | 10 | false |
  | Core | `PushBackFullEditorCharacterization` | 3 | false |
  | Core | `PushBackModuleEditorCharacterizationTests` | 15 | false |

  En el RED, los filtros sin `ExpectRed` pueden fallar **solo** por los mutantes, con la atribución T-8 en la entrega; en el GREEN, todos en verde.
- **Invariantes propios:**
  - **INV-PBT1A-01. Alcance:** `git diff --name-only BaseSha..CurrentSha` = los siete archivos nuevos; `git diff BaseSha..CurrentSha -- src` vacío.
  - **INV-PBT1A-02. RED y GREEN:** el RED son los siete archivos y los quince mutantes, juntos; el GREEN es `git restore --source=BaseSha` de W y WC y no toca `tests/`. Criterio del RED sobre el filtro con `ExpectRed`: todos `Failed`; `Message` sin el prefijo de tipo de [PKC-C]; la primera línea del `StackTrace` bajo `tests/`; ninguno `NotExecuted` ni `Skipped`; `Failed == Selected >= 14`.
  - **INV-PBT1A-03. Cada mutante solo:** aplicado aislado sobre la `BaseSha` (parche y `git restore`), hace fallar por aserción el CT de su fila. La entrega lo transcribe, mutante por mutante, con el TRX de cada corrida temporal.
  - **INV-PBT1A-04. Las líneas esperadas:** salen de la medición en la `BaseSha` (paso 1), transcrita en la entrega antes del RED.
  - **INV-PBT1A-05. CT-14:** el corpus extendido y los guiones son los de §5.2, en ese orden; un diseño o un paso que no carga o no es válido se fija como tal.
  - **INV-PBT1A-06. Las mediciones guardadas** (M-06 de PB-T0): la salida de cada medición del paso 1, como artefacto con ruta relativa.
- **`ExpectedEvidence`:** antes del Worker, el contrato, la delegación, la comparación byte a byte, A1-A8 y nc4; el paso 1 con su TRX y su log; el RED y su CI (ui-tests en `failure`; tests, build-ui y build-plugin en `success`; sin cuelgue); la tabla T-8; los quince mutantes aislados; el GREEN y su CI 4/4; en local, el build Release con el Plugin sin avisos nuevos y los filtros sobre el RED y el GREEN, y tres corridas en verde del filtro con `ExpectRed` sobre el GREEN; la entrega con el mapa CT → método y las rutas relativas comprobadas; la verificación y nc1 a nc3.
- **`StopConditions`** (17 estándar, C08, C11, C12 y las comunes C01 a C10): ninguna propia más.
- **Línea base de la CI:** la del cierre de PB-TC2. Previsión desde su §6.9: **GREEN de PB-TC2** UI 2 422 resultados, 21 `Skipped`, 2 401 `Passed`; Core 19 070. **GREEN de PB-T1a:** UI 2 436, 21 `Skipped`, 2 415 `Passed`; Core 19 070. El RED: 14 nuevas `Failed` más las existentes que rompan los mutantes, atribuidas.

### 6.2 Contrato de PB-T1b — `F4B-PBT1B-LOSSLESS-STATE-TYPE`

- **Objective** (ASCII): «Crear en Application el estado editable de Push Back sin perdidas fuera de la ventana (PushBackEditableState), con las cargas de LoadNew, LoadExisting, LoadDesignForNew y Restaurar, la salida BuildInputs, el copiador y el comparador completos con modulos, cola aparcada y A/B, el mapa de propiedad, la regla explicita de c-02, la exclusion de pb-21-cola y el criterio de bloqueo de salida extraido, segun el paquete del contrato de PB-T1 v1 y la lista cerrada CT-PBT1B-01..13. El RED es el esqueleto inerte y las pruebas nuevas, que fallan por asercion; el GREEN implementa solo Application. La ventana, el piloto, el Plugin y el XAML no cambian; no se ejecuta AutoCAD.»
- **`AllowedWriteScope`** (11): ES y RMES; los cuatro archivos nuevos de `src/RackCad.Application/Systems/PushBack/` (`PushBackEditableState.cs`, `PushBackEditableStateComparer.cs`, `PushBackEditableStateOwnership.cs`, `PushBackEditorOutputGate.cs`); los cuatro nuevos de `tests/RackCad.Tests/` (`PushBackEditableStateLoadTests.cs`, `PushBackEditableStateCopyTests.cs`, `PushBackEditableStateOwnershipTests.cs`, `PushBackEditorOutputGateTests.cs`) y el nuevo `tests/RackCad.UI.Tests/PushBackEditableStateParityTests.cs`.
- **`ForbiddenWriteScope`:** 117 de `src/` (CS incluido) y lo común.
- **`RequiredTests`:**

  | Proyecto | Filtro | `MinSelected` | `ExpectRed` |
  |---|---|---|---|
  | Core | `PushBackEditableState` | **11** | true |
  | Core | `PushBackEditorOutputGate` | **1** | true |
  | UI | `PushBackEditableStateParityTests` | **1** | true |
  | Core | `Workspace` | 3 271 | false |
  | Core | `PushBackOutputGateTests`, `PushBackBlankFrontAuthorityTests`, `PushBackModuleEditorCharacterizationTests` | 8, 9, 15 | false |
  | Core | `RackModuleEditSessionTests` | el medido en la emisión | false |
  | Core y UI | Las de PB-T1a (`PushBackStateNet`, 14), T1B, PB-T0, PB-TC, PB-TC2 y Cantilever | como en §6.1 | false |

- **Invariantes propios:**
  - **INV-PBT1B-01. Alcance:** el diff dentro de las 11 rutas; en `src/`, solo las seis de producto. `git diff BaseSha..CurrentSha -- src/RackCad.UI src/RackCad.Plugin src/RackCad.Domain src/RackCad.Application/Systems/PushBack/Pilot src/RackCad.Application/Workspace src/RackCad.Application/Systems/PushBack/PushBackCompositeEditorState.cs` vacío.
  - **INV-PBT1B-02. RED y GREEN:** el RED es el esqueleto exacto del §5.3 y las cinco pruebas; el GREEN solo toca `src/`. Criterio del RED sobre cada filtro con `ExpectRed`: todos `Failed` por aserción (como INV-PBT1A-02), con `Failed == Selected`.
  - **INV-PBT1B-03. La API:** los miembros, las firmas y los valores iniciales son exactamente los de D-PBT1-02 a D-PBT1-11; ningún otro miembro público nuevo.
  - **INV-PBT1B-04. Ningún campo nuevo** en CS, ES, `PushBackEditorInputs`, `PalletSpecification`, `DynamicAnnotationOptions`, los tipos de seguridad ni RMES. Se comprueba con el censo del piloto en verde y con `git diff -U0` (ninguna línea añadida que declare un campo en ES ni en RMES).
  - **INV-PBT1B-04b. Los espacios de nombres** (`tests/RackCad.Tests/NamespaceFolderGuardTests.cs`): cada archivo nuevo de `src/` declara un solo `namespace RackCad.Application.Systems.PushBack` de bloque, no de archivo; las pruebas nuevas declaran `namespace RackCad.Tests` o `namespace RackCad.UI.Tests`, como las de PB-T0.
  - **INV-PBT1B-05. Las cuentas, sobre `CurrentSha`:** `git grep -c -F ".RequestUpdate(" -- src/RackCad.Application/Systems/PushBack/PushBackEditable*.cs src/RackCad.Application/Systems/PushBack/PushBackEditorOutputGate.cs` sin coincidencias; ningún archivo nuevo nombra `RackCad.UI`, `System.Windows`, `Autodesk`, `Workspace`, `Pilot` ni `RackSystemKind`.
  - **INV-PBT1B-06. Paso 1, sobre `BaseSha` y antes del RED** (una corrida temporal de UI que no entra en ningún commit, con `Assert.Fail`): con `pb-01-base` y su `HeaderPostCatalogId` cambiado a (a) el id del catálogo, (b) el mismo con otras mayúsculas, (c) uno inexistente, (d) en blanco y (e) solo espacios, la línea `<caso> PostBox.SelectedId=<v> salida=<HeaderPostCatalogId>`. Se transcribe en la entrega. Si (b) conserva el id, es **PBT1B-C14**.
- **`StopConditions` propias:**
  - **PBT1B-C13:** la implementación necesitaría un campo nuevo en un tipo del censo del piloto, tocar CS, o cambiar el comportamiento de un método existente de ES, CS, DA, CA o RMES.
  - **PBT1B-C14:** el paso 1 muestra que hoy un id de poste con otras mayúsculas se conserva, o cualquier otra fila de (a) a (e) distinta de la regla de D-PBT1-09. STOP antes del RED: la regla vuelve a la sesión, que decide si es una corrección del paquete o una vuelta al Owner (L-2).
  - **PBT1B-C15:** CT-PBT1B-01 o 02 da una diferencia entre el estado y la ventana de hoy, o una pérdida en un rack soportado, que no sale de un error del esqueleto. STOP: es un hecho nuevo (una pérdida no declarada) y vuelve al Owner por OB-06.5.
- **`ExpectedEvidence`:** como §6.1, con el paso 1 de INV-PBT1B-06 y, en vez de los mutantes, el esqueleto (el diff del RED en `src/`) y la tabla «CT → la aserción que falla en el RED».
- **Línea base de la CI:** la del cierre de PB-T1a. **Previsión del GREEN:** Core 19 070 + 12; UI la de PB-T1a + 1.

### 6.3 Contrato de PB-T1c — `F4B-PBT1C-LOSSLESS-STATE-WINDOW`

- **Objective** (ASCII): «Hacer que la ventana clasica de Push Back guarde su estado editable en el PushBackEditableState de PB-T1b, en modo CLASSIC y sin cambio observable, con el cambio minimo de D-PBT1-13 (reenvios con el mismo nombre, las cargas delegadas, ReadInputs que escribe en el estado, el criterio de bloqueo extraido y la costura EditableStateForTest), segun el paquete del contrato de PB-T1 v1 y la lista cerrada CT-PBT1C-01..03. El RED es la costura inerte y las pruebas nuevas; el GREEN toca solo W y WC. Los constructores, los cuatro sitios, el XAML, el Plugin, el piloto y Application no cambian; el modo Workspace queda para PB-T2; no se ejecuta AutoCAD.»
- **`AllowedWriteScope`** (4): W, WC, `tests/RackCad.UI.Tests/PushBackWindowOnStateTests.cs` y `tests/RackCad.Tests/PushBackWindowOnStateSourceGuardTests.cs`.
- **`ForbiddenWriteScope`:** 59 de `src/` y lo común.
- **`RequiredTests`:**

  | Proyecto | Filtro | `MinSelected` | `ExpectRed` |
  |---|---|---|---|
  | UI | `PushBackWindowOnStateTests` | **2** | true |
  | Core | `PushBackWindowOnStateSourceGuardTests` | **1** | true |
  | UI | `PushBack` (todas las clases `PushBack*`) | el medido en la emisión (538 + 14 + 1 + 4 de las nuevas de PB-TC2) | false |
  | UI | `BlankFront`, `WindowCensusGuardTests`, `RichEditor`, `Workspace` | 33, 8, 25, 547 | false |
  | Core | `PushBackEditableState`, `PushBackEditorOutputGate`, `PushBackClassicCharacterization`, `PushBackFullEditorCharacterization`, `PushBackModuleEditorCharacterizationTests`, `I55DeferredProductFixCharacterizationTests`, `Workspace` | 11, 1, 10, 3, 15, 3, 3 271 | false |
  | UI | `Cantilever` | 67 | false |

- **Invariantes propios:**
  - **INV-PBT1C-01. Alcance:** `git diff --name-only BaseSha..CurrentSha` dentro de las 4 rutas, y en `src/` solo W y WC; `git diff BaseSha..CurrentSha -- src/RackCad.UI/Systems/PushBack/RackPushBackSystemWindow.xaml src/RackCad.Application src/RackCad.Domain src/RackCad.Plugin src/RackCad.UI/Editor src/RackCad.UI/Controls` vacío.
  - **INV-PBT1C-02. RED y GREEN:** el RED son las dos pruebas y, en W, solo la línea de la costura inerte; el GREEN solo toca W y WC. Criterio del RED como INV-PBT1A-02, con `Failed == Selected` en cada filtro con `ExpectRed`.
  - **INV-PBT1C-03. Dónde cambia, sobre el diff:** cada hunk de `git diff -U0 BaseSha..CurrentSha -- src` cae en un miembro de esta lista, por su rango en `BaseSha` (con la definición de «tocar un miembro» de [PKC2] INV-PBTC2-03): los campos de D-PBT1-13 (1) y (2); `LoadNew`, `LoadDesignForNew`, `LoadExisting`, `SideAOnly` (retirado), `LoadFromModel`, `ReadInputs`, `Name_Changed`, `SelectedDimensionViews` (retirado), `Restore_Click`, `InsertBatch_Click`, `RequestDraw`, `SaveLibrary_Click`; en WC, `LoadCompositeFromDesign` (los dos, retirados), `CompositeDiagnostics`, `CompositeStatusOr` y `CompositeHasBlocking`; y las declaraciones nuevas `SyncName` y `EditableStateForTest`.
  - **INV-PBT1C-04. Los literales de T-SG:** las cinco sentencias de CT-22, la de S-9, la de S-7 y las cuatro de CT-23 siguen en W, byte a byte y en su orden; `SyncName();` va inmediatamente después de cada `SetName` y de cada `Adopt`.
  - **INV-PBT1C-05. Las cuentas, sobre `CurrentSha`:** W tiene exactamente una `.RequestUpdate(`; ninguna línea añadida de W o WC contiene `forceRebuild`, `KeyDown`, `PreviewKeyDown`, `InputBindings`, `KeyBinding`, `TabIndex`, `IsTabStop`, `KeyboardNavigation`, `.Focus(` ni `DialogResult`; W sigue conteniendo `PhysicalPostIndex(LateralSectionBox.SelectedItem)`, `corte.PostIndex == section`, `RequestStandardRestore()`, `desviadorLevelsPerPost: DesviadorLevelsPerPost()` y `showDesviadorSide: false`; ninguna línea con `SetNumber(` se añade ni se quita; las siembras de `LoadFromModel` listadas en D-PBT1-13 (4) no cambian.
  - **INV-PBT1C-06. Las costuras:** las 40 de la medición H siguen, con el mismo nombre y tipo; solo se añade `EditableStateForTest`.
  - **INV-PBT1C-07b. Las huellas:** CT-PBT1A-14 en verde sin tocarla, en local y en la CI.
- **`StopConditions` propias:**
  - **PBT1C-C13:** mantener sin cambio observable un gesto exigiría tocar un miembro fuera de INV-PBT1C-03, o cambiar el orden de una carga (en particular, que `UpdateCompositeEnabled()` no vaya inmediatamente después de la carga del estado).
  - **PBT1C-C14:** una huella de CT-PBT1A-14 cambia. STOP: es un cambio observable (C03), y el Worker no ajusta ni la huella ni el producto para cuadrarla.
- **`ExpectedEvidence`:** como §6.1, con la salida de INV-PBT1C-03 a 05 sobre el diff y, en local, la clase de huellas de PB-T1a tres veces en verde sobre el GREEN.
- **Línea base de la CI:** la del cierre de PB-T1b. **Previsión del GREEN:** Core + 1; UI + 2.

## 7. Riesgos y lecciones aplicadas

| Id | Riesgo o lección | Tratamiento |
|---|---|---|
| R-T1-01 | **Sin latitud para el Worker** (lección de PB-T0; funcionó en PB-TC) | Los contratos no dan latitud para corregir una prueba, una línea esperada ni un mutante; cualquier discrepancia es C07 |
| R-T1-02 | **Las líneas esperadas salen de leer el código** en PB-T1b y PB-T1c | Los oráculos son la ventana de hoy, CT-PB0-01, CT-PB0-17 y `LegacyDeltas`, no la lectura; PBT1B-C15 |
| R-T1-03 | **Evidencia con rutas relativas y `TreeSha` real** (M-04 de PB-TC) | INV-PBT1x-11 |
| R-T1-04 | **Corridas temporales con `Assert.Fail`** (el TRX trunca los mensajes, H-PB-45) | INV-PBT1x-10; los pasos 1 de PB-T1a y PB-T1b |
| R-T1-05 | **Las ventanas con el aviso de descarte sustituido** (M-05 de PB-TC, I-03 de PB-TC2) | INV-PBT1x-08 |
| R-T1-06 | **Nunca la suite completa de UI en local** (F4-E59) | INV-PBT1x-10 |
| R-T1-07 | **MAX_PATH** | Sin copias de árboles; los nombres de `TaskId` (31 caracteres como mucho) son del orden de los de PB-TC2 |
| R-T1-08 | **La variación de nc1 de PB-TC** (Remote, Scope, Tests y Trailer en fail, además de Handoff, Identity y Ci). El Owner la aceptó una sola vez | Vuelve al Owner por C11, sin sustituto, en cada una de las tres cadenas |
| R-T1-09 | **Los nombres de las sustituciones** chocarían con las paradas `S-0n` | `SUS3A-0n`, `SUS3B-0n` y `SUS3C-0n`; se esperan 0 |
| R-T1-10 | **Tres cadenas antes del 2026-12-29** | Se comprueba la vigencia de las celdas en cada emisión y en cada delegación (A7) |
| R-T1-11 | **El censo del piloto** (HT1-21) rompería con un campo nuevo | INV-PBT1B-04; PBT1B-C13 |
| R-T1-12 | **La línea base compartida** podría mutarse por un camino no leído | CT-PBT1B-08 la vigila en todo el API; L-1 del §8 |
| R-T1-13 | **`UpdateCompositeEnabled()` puede lanzar un recálculo** si cambia `FrontalSideBox` (W:3277-3289) | Se llama en el mismo punto relativo de hoy (D-PBT1-13 (3)); F-02 en CT-PBT1A-14 lo ejerce; PBT1C-C13 |
| R-T1-14 | **PB-TC2 no ha cerrado** y su GREEN puede desplazar o cambiar líneas | Las zonas [TC2]; la precondición 1 y los anclajes del §6.0 |
| R-T1-15 | **Cómo cuenta el Controller un `Skipped`** | `MinSelected` sobre los que pasan, como en PB-TC |
| R-T1-16 | **El RED de PB-T1a rompe pruebas existentes** por los mutantes | La tabla T-8, como en PB-T0; los mutantes no tocan las guardas de Core, así que `tests` sigue en `success` |
| R-T1-17 | **Dos copias de la carga** (la del estado y la del piloto) hasta PB-T4 | Declarado (D-PBT1-14); PB-T4 consume el API de PB-T1 y no inventa traducción ([OD], fila de PB-T4) |

## 8. Las decisiones del Owner

### 8.1 Ya tomadas, que este paquete aplica

- **F4B-E09, pregunta 2 (c-02):** «Como hoy, como regla explícita (Recomendado)» → D-PBT1-09 y CT-PBT1B-03.
- **F4B-E09, pregunta 3 (`pb-21-cola`):** «Dejarlo fuera de la garantía (Recomendado)» → D-PBT1-10 y CT-PBT1B-04.
- **F4B-E09, alcance:** «Los demás casos de T-2 … se quedan como hoy, y PB-T1 los reproduce» → CT-PBT1B-05.
- **F4B-E09, pregunta 1:** H-31 y OWNER-1 en PB-TC2, antes de PB-T1 → la precondición 1 y las zonas [TC2].
- **F4B-E09, pregunta 4:** la emisión condicional → precondición 2.
- **[OD], fila de PB-T1:** sin MVVM ni estética → D-PBT1-13.
- **[P] OB-10.1:** cada subtarea empieza en 1/3 → D-PBT1-01.

### 8.2 Decisiones materiales nuevas

**Ninguna que el paquete tome.** Lo siguiente no cambia ningún comportamiento observable ni ninguna regla del Owner:
- **M-07 se reproduce** (D-PBT1-12): es lo que manda OB-RE.1. Corregirlo sería una decisión nueva, que no se propone.
- **La regla de c-02 sigue a `PostBox`, no a `ResolveBeamId`,** en el único punto en que podrían diferir (las mayúsculas): es «como hoy». Si el paso 1 de PB-T1b mide otra cosa, la cadena para (PBT1B-C14).
- **`Name` pasa al estado** como campo del editor y `session.Identity` queda como sumidero del contrato: es OB-05.1.
- **El piloto no se toca:** es OB-03.1.

### 8.3 Lecturas que la revisión debe confirmar

Si la revisión considera material cualquiera de ellas, **PB-T1 vuelve al Owner antes de emitirse**, con las opciones que se indican.

- **L-1. «no aliasing» y los derivados resueltos.** El copiador comparte por referencia dos objetos: la línea base resuelta de cada lado (`workingBaseline`) y el informe de la última reconciliación (`LastModuleReconciliation`). Todo lo demás se copia en profundidad: el contenido editable, la sesión de módulos con lo escenificado, el commit, la cola y los portadores.
  - **Por qué no es un alias en el sentido del requisito:** los dos son el resultado del último recálculo aceptado, no estado editable. El producto ya comparte hoy la línea base entre la ventana, la sesión y el estado (HT1-14), y nadie la muta: se sustituye. CT-PBT1B-07 declara la excepción por clase y CT-PBT1B-08 prueba que ningún gesto del API muta una línea base adoptada.
  - **La alternativa:** una copia profunda de `PushBackSystem`. No existe en el producto, y escribirla es una tarea de Domain con decenas de tipos, entre ellos las configuraciones de cabecera con sus excepciones de tiempo de ejecución (I-17).
  - **Si fuera material, las opciones para el Owner:** (a) aceptar la lectura; (b) una tarea previa de Domain para la copia profunda del sistema resuelto.
- **L-2. La comparación de c-02.** «igual que ResolveBeamId» del Owner describe el resultado (fuera del catálogo → por defecto), no la comparación. Con «RACKEDITAR no cambia», manda la de hoy (D-PBT1-09). El paso 1 de PB-T1b la mide.
- **L-3. La división y su procedimiento.** Las tres subtareas caben en OB-10.1 («y sus subtareas si se dividen»), y el contenido, las dependencias y el orden de la fase no cambian. Pero son tres cadenas, cada una con su 1/3, su planificación y sus controles. **El procedimiento que se propone:** esta v1 se revisa una vez para las tres; PB-T1a se emite tras esa revisión; PB-T1b y PB-T1c se emiten al cerrarse la anterior, con los anclajes del §6.0 y una comprobación acotada, sin otra revisión completa, salvo que un anclaje cambie el diseño.
  - **Si fuera material, las opciones:** (a) aceptarlo; (b) una revisión completa por subtarea; (c) PB-T1 en una sola cadena, con más riesgo de una tarea grande para un Worker (R-PB-01).

### 8.4 Lo que se informa sin preguntar

- **Un control ya no decide el estado, aunque se vea igual:** con un poste fuera del catálogo, el desplegable sigue vacío como hoy, y el estado guarda el poste por defecto.
- **La cola aparcada deja de compartir objetos con el diseño cargado.** Sin efecto observable.
- **Con un catálogo distinto del entregado**, `RearPeralteBox` y los combos de peralte de la ventana pueden sustituir valores que el estado de Application conservaría (HT1-17). No hay caso alcanzable con el catálogo entregado (OB-06.5), y la ventana no admite otro catálogo (H-PB-31). Es un dato para PB-T4.

## 9. Límites

Están en `pbt1-measurements.md`, apartado L. En resumen:
- **Nada se compiló ni se ejecutó.** Las líneas esperadas de PB-T1a las mide el Worker; las de PB-T1b y PB-T1c salen de oráculos existentes, pero el diseño se escribió leyendo código.
- **PB-TC2 no ha cerrado:** las zonas [TC2] y los mutantes MU-T1A-03, 04, 07a y 10 se vuelven a medir sobre su GREEN.
- **No medido:** cómo compara WPF un `SelectedValue` con otras mayúsculas (lo mide el paso 1 de PB-T1b); que ninguna ruta mute una línea base adoptada (por lectura; lo vigila CT-PBT1B-08); que `RackModuleEditSession` se pueda copiar y comparar con sus métodos privados como se describe (por lectura de sus nueve campos).
- **Los recuentos son estáticos;** las `Theory` con `MemberData` no se cuentan.
- **Los conteos de la CI de `eedb069c`** se dan por iguales a los de `5597c05b` (mismo `src` y `tests`).
- **El tamaño de cada subtarea** en turnos de Worker se estima por analogía con PB-T0 y PB-TC.
- **Lo que solo se ve en AutoCAD** (el dibujo con los valores exactos) sigue siendo de Smoke-PB.

## 10. Arrastres que salen de PB-T1

| Id | Qué | Va a |
|---|---|---|
| A-1 | El modo WORKSPACE, la prueba UI-STA de OB-01.3, el paso 4 de D-PB-06 y el aviso de D-PB-08 en WORKSPACE, y el dueño de los subdiálogos | PB-T2 (sin cambio) |
| A-2 | El borrador sobre `PushBackEditableState`: el copiador y el comparador de PB-T1 sustituyen a los del piloto en el `DraftSession`; la composición y el clasificador de D-PB-05 con el mapa de D-PBT1-08; la barrera de PREPARE con `PushBackEditorOutputGate` | PB-T3 (composición, barrera) y PB-T4 (borrador, «Editar todo») |
| A-3 | Quitar el rechazo A/B del piloto (OB-03.2) cuando PB-T1 esté cerrada; `DormantTailOutOfPilotReason` se queda en todo F4B | PB-T3 o PB-T4 |
| A-4 | La pregunta al Owner sobre la `ExtensionData` anidada (OB-05.2, IR2 R-I-10) | Antes del contrato de PB-T3 (sin cambio) |
| A-5 | Las dos copias de la carga (estado y piloto) | PB-T4 retira la del piloto al consumir el API |
| A-6 | M-07 (reproducido) y la caja de c-02 que se ve vacía | `docs/ideas-futuras.md`, por la sesión |
