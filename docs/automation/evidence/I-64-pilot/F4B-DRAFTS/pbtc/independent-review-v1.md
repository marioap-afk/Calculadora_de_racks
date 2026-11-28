# I-64 F4B PB-TC — Revisión adversarial independiente del paquete del contrato, v1

> **Redacción para el repo público:** sin rutas locales ni nombres de usuario o de máquina. Las rutas son relativas a la raíz del repositorio. «TC:n» es la línea *n* del paquete revisado (`I-64-f4b-pbtc-gate-contract-package.md`, v1); «ME» es su registro `pbtc-measurements.md`. Se usan los alias del paquete: W, WC, WX, NF, WLC y S0; además, **CS** = `src/RackCad.Application/Systems/PushBack/PushBackCompositeEditorState.cs`, **DA** = `src/RackCad.Application/Systems/PushBack/PushBackEditorDesignAssembler.cs` y **DEA** = `src/RackCad.Application/Systems/Dynamic/DynamicEditorDesignAssembler.cs`.

## 1. Objeto, método y lo que no pude verificar

**Objeto.** El paquete v1 del contrato de PB-TC y su registro de mediciones, contra la orden del Owner ([OD], `F4B-DRAFTS/coordinator/owner-f4b-disposition.md`), el paquete de la fase v3.1 ([P]), las decisiones de procedimiento ([DP] y F4B-E01 a F4B-E05), la plantilla de PB-T0 (paquete, contrato, entrega del Worker, cierre y revisión independiente), el patrón de I-39, AUTOMATION_PLAN §16 y el código de la rama.

**Método.** Solo lectura sobre la rama en `f9f2afbb` (comprobado con `git rev-parse HEAD`; árbol limpio):
- lectura de W, WC, WX, NF, CS, DA, DEA, `DynamicLoadBeamGeometry.cs`, `DynamicRackSystemResolver.cs`, `DynamicViewDecorations.cs`, los constructores de planos de Push Back, `LateralHeaderDrawer.cs`, el catálogo entregado (`assets/catalogs/secciones.csv`) y las pruebas citadas;
- `git grep` de todos los lectores de las once cajas, de `Val(`, de `SetNumber(`, de `DimensionStyle`, de números con cuatro o más decimales y de las guardas que leen el XAML o el código de la ventana;
- un recorrido de Python en solo lectura (`-I`) de los 17 sobres de `tests/RackCad.Tests/Fixtures/I64` (datum, primer nivel, decimales y estilo);
- `git cat-file -e` de las 45 rutas de `ForbiddenWriteScope`;
- `gh run view 44188678597` (estado, trabajos y totales del log);
- lectura de la verificación de F4-T3B1 (`F4-T3B1-EDIT-SURFACE/1/R20261113T061247Z-2130/output.json`) para el precedente del RED con Core en `success`.

**Lo que no pude verificar** (sin compilar ni ejecutar):
- ninguna línea esperada se ha ejecutado; las que doy por buenas o por falsas son, como las del paquete, deducciones del código;
- que un `Theory` con `Skip` y `MemberData` cuente como un resultado lo sostengo por la versión de xUnit (2.9.2 en los dos `.csproj`) y por la evidencia de la CI de HEAD (17 Skipped con un `Theory` de 4 filas), no por una corrida;
- el efecto real en la salida de suprimir la reconstrucción de módulos de I-02 no se puede medir sin ejecutar;
- el aspecto en AutoCAD de las cotas con estilo nombrado.

## 2. Análisis por punto

### 2.1 Cobertura de la orden y de la fase

**Los seis pasos de D-PB-06** ([OD]:139-150):
1. Caracterizar la pérdida actual: lo hizo PB-T0 (CT-PB0-18 a CT-PB0-21). Cumplido.
2. Un RED por pérdida: CT-PBTC-01 a 06 fallan en la base. Cumplido en el RED, pero dos de sus líneas no se cumplirán en el GREEN (B-01).
3. «implement the corrected shared state/load behavior»: la corrección vive en la ventana con portadores privados (D-PBTC-01 y 02, TC:193-241); el estado de Application ya conservaba los dos valores (H-10, TC:93). Es una lectura defendible del paso 3 antes de PB-T1, pero la trazabilidad del §4.3 (TC:438-447) no tiene fila para el paso 3 ni para el 6 (M-07).
4. RACKEDITAR: CT-PBTC-02 y 05. El Workspace queda para PB-T2 (D-PBTC-06, TC:280-285).
5. Sustitución con registro: S-01 a S-03 (§5.2).
6. Anotación de la sesión (INV-PBTC-16, TC:644-646). Cumplido.

**El paso 4 en WORKSPACE y el aviso en WORKSPACE.**
- La letra de [OD] (:144-147) pide probar los dos («prove both: Workspace; RACKEDITAR»), sin exigir que sea en la misma tarea. [P] lo asigna a PB-T2 de forma expresa (OB-06.3, [P]:383; fila PB-T2, [P]:504; [P]:511-513), y la revisión de la fase lo aceptó (IR2 R-M-06). Es **legítimo**. Pero el Owner nunca lo vio como decisión, y la vuelta del §8 es el sitio natural para informarle (M-07).
- El aviso: [P] OB-08.2 (:438) dice «Va en los dos modos, porque es la misma ventana». En WORKSPACE estará por construcción cuando exista el modo, así que dejar su prueba a PB-T2 es legítimo. Pero la fila de PB-T2 del §5 de [P] (:504) no lo recoge, y el registro declarado (TC:509-511) no obliga por sí solo al contrato de PB-T2 (M-07).

**OB-06.1 a OB-06.6:**
- OB-06.4 (vuelta al Owner antes de PB-TC): se hace (§8), pero el material no es fiel a lo medido (B-01, I-02, I-03 y M-01).
- OB-06.5: el paquete no registra los lectores que siguen redondeando fuera de F1-F11 (I-03), que son casos alcanzables de la misma clase.
- OB-06.6 («se limita a la ruta de carga de Push Back», [P]:395): la corrección va más allá de la carga (selección, «Ambos lados», alcance y datos del frente; TC:214-219). Es coherente con el propósito de OB-06.6 (no tocar `NumericField` ni Cantilever) y está declarado (TC:221, TC:832). Conviene que el Owner lo vea como extensión explícita y no solo como «consecuencia» (M-01).

**D-PB-08 y OB-08.1 a OB-08.4:** se cumplen. La presentación va al Owner (OD-PBTC-3), como pide OB-08.4.

**Lo que nadie asignó:**
- CT-PBTC-09: es un medio para OB-06.6, pero decorativo (M-04);
- los cambios de «Insertar» y de «Guardar en biblioteca» desde la biblioteca: consecuencia declarada de la misma corrección (TC:241).

No sobra nada material.

### 2.2 El diseño de (a), el redondeo (D-PBTC-01)

**Lectores de las once cajas.** `git grep` de cada nombre en `src/`:
- confirma que solo `ReadInputs` (W:819, :837-839, :845) y `ReadCellValues` (W:797-801, :804) leen su `Value`;
- y que solo `LoadFromModel` (W:386-406), `LoadSelectedFront` (W:454-463) y `SetMixed` (WC:556) escriben su `Text`.

Las cajas espejo (W:558-564) solo se validan (W:728-734). El adaptador del piloto solo nombra `AnnotationScaleBox` en un comentario.

**`Val` no queda sin usos (I-01).** `Val` se usa también en W:1865 (`Val(ModuleLengthBox, module.Length)`, en `ModuleLength_Changed`). El paso 5 de D-PBTC-01 (TC:207: «se queda sin usos y se retira») es falso: aplicarlo rompe la compilación.

**F4 no es una pérdida de la ventana (B-01).** `BeamDepthBox` redondea, pero el ensamblador normaliza el valor al catálogo antes de escribir el diseño:
- DA:165-168 llama a `DynamicLoadBeamGeometry.ResolveBeamDepth(catalog, "LARGUERO_IN_OUT_C6", editorInputs.BeamDepth)`;
- `ResolveBeamDepth` (`DynamicLoadBeamGeometry.cs:52-71`) devuelve el pedido solo si está a menos de 1e-6 de un peralte permitido, y si no, `allowed[0]`;
- el perfil `LARGUERO_IN_OUT_C6` declara un solo peralte, `6` (`assets/catalogs/secciones.csv:8`);
- el diseño toma `BeamDepth = system.InOutBeamDepth > 0 ? system.InOutBeamDepth : beamDepth` (`DynamicRackSystemResolver.cs:311`, por `DEA` `BuildDesign` → `Snapshot`), y `InOutBeamDepth` sale del mismo `ResolveBeamDepth` (`DynamicRackSystemResolver.cs:57`, `:252`).

Así, con 6,00044 exacto en la entrada, la salida sigue siendo 6. CT-PB0-19 la clasificó `REDONDEA` solo porque R3(6,00044) = 6. Y T-3 R4 («nada») es coherente: los dos diseños resuelven a 6.

Las otras diez filas sí pasan tal cual al diseño:
- F1: `Snapshot` copia `system.Pallet`;
- F2: DEA:105;
- F3: DEA:106;
- F5: DEA:118;
- F6: `BuildFrontDesigns`, `DynamicFrontMatrix.cs:449-462`;
- F7 a F11: `DynamicEditorCell.ToDesign`, `DynamicEditorCell.cs:41-53`.

**El orden con la selección de celda.**
- `SelectMatrixCell` hace `CommitCurrentCell()` (W:1061) con las cajas todavía sembradas desde la celda anterior, después `ToggleCell` (W:1062), luego `LoadSelectedFront` siembra la nueva (W:1063) y la pasada siguiente la compromete (W:1066 → W:621).
- `SelectSingleCell` (W:1165-1170), `MutateStructure` (W:1181-1188) y el cambio de lado (WC:138-154) siguen el mismo patrón.
- Con «Ambos», `ForEachEditedSide` replica la selección antes de escribir (W:753-767), y `ApplyMixedSideState` decide el estado mixto con la selección activa para los dos lados (WC:486-537). Una caja no mixta implica que A y B difieren en 1e-6 o menos (WC:437, :481-483).

`Keep` nunca compara la caja con otra celda ni con el otro lado de un modo que escriba un valor equivocado. **Correcto.**

**`OptionalVal` y la cultura.**
- La semántica de nulo de W:3615-3625 se conserva: con caja vacía, la regla no cambia.
- La cultura es correcta. `SetNumber` pinta con la cultura actual (NF:131-135) y `Value` sale de reinterpretar ese texto. Formatear con «0.###» en la invariante es idempotente sobre `Value`, y el redondeo a tres decimales no depende de la cultura: `SameShown` equivale a comparar lo que la caja muestra.

**Efectos que el paquete no ve:**
- **La reconstrucción de módulos (I-02).**
  - Hoy, un `PalletDepth` con más de tres decimales hace que la primera pasada tras la carga reconstruya la secuencia de módulos: `MustRebuild` compara el palé redondeado de la caja con el del baseline con tolerancia 1e-6 (DEA:44-60; DA:211-214), y `BuildDefault` más la reconciliación sustituyen a la copia del baseline.
  - Con la corrección, el baseline se copia.
  - Es un cambio en lo que RACKEDITAR escribe, más allá del número, y nadie lo midió. T-3 resuelve cada diseño por separado con `BuildFrom` (`tests/RackCad.Tests/PushBackFullEditorCharacterizationRoundingEffectTests.cs`, `Compute`), sin la decisión de reconstruir de la ventana. INV-PBTC-09 (TC:616) solo lo compara en los 14 diseños de WLC, ninguno con un fondo de más de tres decimales.
- **Otros lectores que siguen redondeando (I-03).** Fuera de F1-F11, tres grupos de cajas sembradas con «0.###» se releen en su propio gesto, aunque el usuario no cambie su valor:
  - `GapBox`: `Gap_Changed`, en `LostFocus`, hace `SetGap(GapBox.Value)` sin comparar (WC:168-174). Basta con pasar por el campo;
  - `ModuleLengthBox`: W:1865-1871, con tolerancia 0,0001. El cambio se escenifica y `RequestDraw` lo confirma (W:3346-3349);
  - las seis cajas avanzadas: se siembran con `SetOptional` (W:1566-1571, `SetNumber` por defecto), y cualquier `LostFocus` o clic del panel las relee **todas** (W:1592-1607, W:1616-1626).

  PB-T0 solo midió G4 a G8 en la carga. Esas cajas siguen siendo fuente de verdad, que es el problema que [P]:509 da como razón de PB-TC.

**Decisiones del paquete.** «Escribir el mismo número que se ve conserva el exacto», las consecuencias en «Ambos lados», alcance y datos del frente, y la extensión fuera de la carga son decisiones de comportamiento de RACKEDITAR. El paquete las declara (TC:217-219, TC:831-832), pero solo da al Owner la opción binaria (i)/(ii) (TC:834-836), sin la alternativa de que el texto escrito mande siempre (M-01). La regla de Cantilever es precedente técnico, no autoridad del Owner para Push Back.

### 2.3 El diseño de (b), el estilo de cotas (D-PBTC-02)

- **Cargas:** las cuatro pasan por `LoadFromModel`: W:272, :298, :315 y :3288 (`Restore_Click`, desde `lastComputation.Design`, que tras la corrección ya lleva el estilo).
- **Construcción de `Annotations`:** solo la construye `ReadInputs` (W:840-848), y a esta solo la llama W:646.
- **Reinicios:** ningún otro camino reinicia `Annotations`. «Restaurar estándar» solo reinicia los avanzados (DA:296-305).

**La cadena hasta cada cota es la de Push Back.**
- Push Back compone los constructores del Dinámico como caja negra: `PushBackSystemFrontalBuilder.cs:28-37`, `PushBackSystemPlantaBuilder.cs:15-24` y `PushBackSystemLateralBuilder.cs:213` (`DynamicViewDecorations.AppendLateral`).
- `DynamicViewDecorations` lee `system.DimensionStyle` (:117, :206, :285) y lo asigna en `DimensionStyleName = style` (:402). `PushBackMirror.cs:69` lo copia en las instancias espejadas.
- CT-PB0-18 lo confirma en Push Back: `{STANDARD-ISO}` y `{SIMPLEX}` en todos los planos y cortes.
- El Plugin construye los planos con los mismos tres constructores que la computación de la ventana (DA:38-40 y :474-489; `PushBackFrontalDrawService.cs:19`, `PushBackPlantaDrawService.cs:18`, `PushBackSystemDrawService.cs:20`).
- Las anclas de H-12 son algo imprecisas: :117, :206 y :285 son lecturas, no asignaciones (M-06).

**El efecto en el dibujo (H-13).** Es exacto frente a `LateralHeaderDrawer.cs:340-394`:
- con un estilo nombrado presente en el dibujo, ese estilo sin sobrescrituras;
- si no, el estilo actual con las sobrescrituras escaladas.

OD-PBTC-2 lo describe bien. Omite dos cosas que también cambian (M-01): RACKEDITAR deja de borrar el estilo guardado en el sobre, y la biblioteca e «Insertar» lo llevan al rack nuevo.

### 2.4 D-PB-08, el aviso (D-PBTC-03)

**La regla de visibilidad coincide con la de capacidad sin presencia.** `BuildSideB` devuelve nulo sin `SideBPresent` o sin ninguna ranura presente (CS:1024-1047). Por eso `effectivelyComposite` es falso exactamente en el caso del aviso (`PushBackCompositeEditorAssembler.cs:51-56`).
- Un lado B dormido da colapsado.
- Al volver con su presencia, colapsado si hay alguna ranura de B activa (CS:364-374).

**Evaluación.** En toda pasada efectiva, después de W:605-607. Todo gesto que mueve la presencia termina en una pasada:
- la casilla (WC:101-120);
- «En blanco» (W:1089-1133 → `MutateStructure`);
- `+`/`-` frente y `FrontCountBox` (W:1071-1081, W:1137-1141);
- las cargas;
- «Restaurar» (W:3288).

No encontré ningún gesto que la cambie sin pasar por `Recompute`: el `RecomputeGate` es síncrono fuera de `Defer` (`src/RackCad.UI/Editor/RecomputeGate.cs:35-44`).

**Los rótulos y el texto.**
- «Lado B» existe en `SideSelectorBox` (WC:50) y «En blanco» en la casilla de la columna (W:936).
- «así no se guardará» es exacto en el modo clásico: el diseño sale sin `SideB` (CS:1044-1047).
- En WORKSPACE no está demostrado, porque el borrador de PB-T1 podría conservar la capacidad. La recomendación «vale en los dos modos» (TC:861) es una afirmación sin medir (M-02).

**Guardas.** Ninguna salta:
- T-KB, incluida la comprobación (3) de valores locales (`PushBackClassicCharacterizationKeyboardTests.cs:257-277`);
- los tokens de T-SG;
- I-55, módulos, desviador, `AnnotationScaleBox`, el censo de ventanas y G-10;
- además, `PushBackSidebarCompositionTests` (como mucho 9 `WrapPanel`, y medianiles de 10), `PushBackShellAdoptionTests` (solo comprueba `PreviewSummary`, `PreviewHint` y `StatusText` en el `StatusHost`), `PushBackClassicCharacterizationResourceTests` y `UiSystemBoundaryGuardTests`.

Pero:
- **la regla del shell.** WX:678-679 y el resumen de `PushBackShellAdoptionTests` (líneas 15-20) declaran «no second status inside the sidebar». El aviso es un mensaje de estado dentro de la barra lateral, fuera de `CompositeSection`, y con `FontSize="9.5"` fijo en vez de un token. Tiene precedente en `SlotPresentText` (WX:166). No rompe ninguna prueba, pero es una decisión de presentación que el Owner debe ver con la regla delante (OB-08.4; M-02);
- **una ancla inactiva.** La guarda «foco» que cita H-18 en `RichEditorCharacterizationTests.cs:288-317` es una prueba **saltada** (`[Fact(Skip = BaseEvidence)]` en :288), no una guarda activa (M-06);
- **el censo control → prueba de PB-T0** no es una prueba. El `TextBlock` no es un control.

### 2.5 Cada CT

| CT | ¿Falla en la base por aserción, con la diferencia del §5.1? | ¿Se cumplen sus líneas en el GREEN? |
|---|---|---|
| 01 | Sí, en F1 (`salida=48`) | **No en F4**: `salida=6 REDONDEA`, no `6.00044 CONSERVA` (B-01). Las otras diez, sí |
| 02 | Sí, en (i) F1 | **(i) F4 no** (B-01). (ii) y (iii), sí. `vistas` no está definido; debe ser `InsertionRequest.Views.Count`, como en CT-PB0-02 (`StateTests.cs:191`) (M-08) |
| 03 | Sí, en la línea 1 | Los seis pasos, rehechos por el código, dan las líneas previstas: carga 1,23456/61,23456/60,00044; `Keep` en cada recomprometido; `caja=60`; vuelta a 70,5. Pero el **paso 3 no prueba R-03**: asignar el mismo `Text` no dispara `TextChanged`, y equivale a no editar (I-04) |
| 04 | Sí, en la línea 1 | Sí |
| 05 | Sí | Sí. Mismos constructores que el Plugin, y CT-PB0-18 ya mide los conjuntos y que las cotas son más de cero |
| 06 | Sí, en `pb-06-seguridad` | Sí en las 14: los deltas de la base son exactamente las 4 líneas de WLC:191-197; el recorrido de los sobres confirma que solo pb-08 tiene más de tres decimales y solo pb-06 y pb-07 traen estilo; `pb-10-sin-datum` da delta vacío en la base, así que su reexpresión de datum no tiene más de tres decimales |
| 07 | Sí, `Assert.IsType<TextBlock>` en `Aviso(w)` | Sí. Línea a línea frente a CT-PB0-09 (`CompositeTests.cs:196-201`) y CT-PB0-07 (`:121-124`): los `compuesto` coinciden, y `aviso` sale de B=3:000, 4:0000, 4:0100, 4:0000 y `absentB=[0,1]` |
| 08 | Sí | Sí. Ningún diagnóstico bloquea un B vacío (`PushBackCompositeDiagnostics.cs:94-150`), así que `UpdateButton` está habilitado (W:3525-3527) |
| 09 | — (no tiene RED) | Pasa siempre. Es **decorativo**: con INV-PBTC-01, `NumericField` no puede cambiar, y Cantilever no usa el formato por defecto (H-PB-34). La prueba real de OB-06.6 es INV-PBTC-01 más el filtro de 67 (M-04) |

**Determinismo:** GUID fijo, cultura invariante por `Sta` y el catálogo entregado. Correcto.

**Costuras:** solo las existentes (`SelectMatrixCell`, `Session`, `CompositeState`, `LastComputation`, `CurrentInputsAreValid` y el contrato público). Ventanas cerradas en `finally`, sin `MessageBox` real ni BOM. Correcto.

**Latitudes de forma** (M-08):
- `AnnotationScaleBox=2.5` no dice si es el texto o el valor;
- `estilos=[…]` no fija el separador;
- el literal del §3 está entre comillas angulares exteriores (TC:252) y el de CT-PBTC-07 no (TC:411).

### 2.6 Las sustituciones (S-01 a S-03)

**Son exactamente las que chocan.**
- `DimensionStyle` en las pruebas de UI: solo `T19_H1` y WLC fijan la pérdida en la ventana (66 líneas en `tests/`, como dice ME E-1).
- Números con cuatro o más decimales en UI: solo WLC y PB-T0.
- Ninguna prueba fija la ausencia de un elemento con nombre ni un recuento que el cambio mueva. `RecomputePassesForTest` no cambia: el aviso se evalúa dentro de la pasada.
- CT-PB0-01 (con X-01), CT-PB0-02, CT-PB0-13 a 15, CT-PB0-17 y CT-PB0-20 no chocan.

**Cobertura.**
- S-01 → CT-PBTC-06: más fuerte, y cubre la rama `WINDOW-NOT-ADMITTED`.
- S-02 → CT-PBTC-04: la misma construcción y el mismo gesto.
- S-03 → CT-PBTC-01: las mismas filas. Pero la comprobación temporal del §5.2 (TC:483, «S-03: en las once filas») es falsa: sin el `Skip`, CT-PB0-19 seguirá pasando en F4 (B-01).

**Forma.** I-39 usa una constante (`BaseEvidence`) con un comentario de transición (`RichEditorCharacterizationTests.cs:34-47`). El paquete usa un literal en línea y solo toca la línea del atributo. Es más estricto, y el registro lleva la explicación. Aceptable.

**Recuentos.**
- Con xUnit 2.9.2, un `Theory` saltado produce un solo resultado, aunque use `MemberData` (coherente con H-26).
- §6.9: 2 421 + 9 − 13 = 2 417 en total; 20 Skipped; 2 389 Passed en el RED y 2 397 en el GREEN. Cuadra.
- §6.4: los mínimos sobre los que pasan valen con las dos lecturas del Controller.

### 2.7 El contrato

**`AllowedWriteScope`:** basta y no sobra.

**`ForbiddenWriteScope`:** las 45 rutas existen (comprobado). Pero **no sigue el método de PB-T0** (M-03). El contrato de PB-T0 prohibía además:
- los 7 archivos hermanos de `src/RackCad.UI/Systems/PushBack/`;
- `Systems/Dynamic/`, `FlowBed/`, `Larguero/` y `Selective/`;
- `Preview/`, `RackFrames/`, `StructuralSections/`, `Views/` y los archivos raíz de `src/RackCad.UI/`;
- las guardas de UI que leen la ventana: `BlankFrontDesviadorHandoffTests`, `EditorClosePolicyTests`, `RichEditorCharacterizationTests`, `RichEditorCloseContractTests`, `RichEditorContractTests`, `PushBackShellAdoptionTests`, `PushBackModuleEditorWindowTests` y `PushBackSafetyPerPostFlowTests`;
- y `tests/RackCad.UI.Tests/Workspace/`.

PB-TC no puede usar ese prefijo, pero puede listar sus hermanos, entre ellos `WorkspaceDraftPilotValidationParityTests.cs`, la guarda de `AnnotationScaleBox` de H-18. No abre un hueco, porque la lista blanca exacta es la barrera primaria, pero debilita la secundaria frente a la plantilla.

**Invariantes.** Casi todos son mecánicos. INV-PBTC-03 («el mismo texto que en la base») e INV-PBTC-15 («medido sobre el diff propio») no lo son tal como están escritos (M-10). INV-PBTC-09 se queda corto (I-02).

**RED.**
- El criterio de INV-PBTC-02 es aplicable a los ocho métodos: aserciones de xUnit, con la primera línea en `tests/`.
- AUTOMATION_PLAN 16.9, fila 9 (`docs/AUTOMATION_PLAN.md:619`), pide literalmente «el job Core en `failure`». El precedente que cita R-10 es real: la verificación de F4-T3B1 dio `RedPart = pass` con el trabajo de UI en `failure` y Core en `success`, «as required by the contract's Core ExpectRed=false». INV-PBTC-14 lleva la misma base contractual.
- `build-plugin` depende de `tests` y de `build-ui`, no de `ui-tests` (`.github/workflows/ci.yml:254-258`). Así que en el RED debe dar `success`, como dice INV-PBTC-14.

**`RequiredTests`.** Los namespaces son `RackCad.UI.Tests` y `RackCad.Tests`, y WLC también está en `RackCad.UI.Tests`. Los recuentos estáticos cuadran:
- 8 nuevos;
- 1 (CT-PBTC-09);
- Cantilever: 10 + (28 + 5) + 24 = 67, en sus tres clases;
- T1B en UI: 21 + 12 + 4 + 4 + 2 + 12 = 55;
- PB-T0 en UI: 18, menos el saltado;
- WLC: 3;
- `PushBackDimensionViewsWindowTests`: 12, más `T19_H1` saltado;
- T1B en Core: 10;
- PB-T0 en Core: 3;
- `PushBackBlankFrontAuthorityTests`: 4 + 5 = 9.

No hay colisiones.

**Paradas.** Cubren [DP] (PBTC-C08 y C11), D-PB-10 (C12), OB-06.4 (C03), OB-06.6 (C01), OB-T1B.3 (C02) y OB-08.4 (C13). Las paradas estándar coinciden con [PK0-C].

**Transcripciones de PD-F4-01 y PD-F4-04:** literales frente a `F4-DRAFTS/f4-dispositions.md:520`, `:562`, `:1104-1106`, `:1540` y `:1657-1660`, y frente a `F4-DRAFTS/coordinator/owner-f4t3-authorization.md:257-262`. Los dos sustitutos de origen existen.

**Celdas:** vigentes hasta el 2026-12-29, porque las dos tienen `VerifiedOn` 2026-09-30 en el catálogo.

**Líneas base:**
- CI: `gh run view 44188678597` confirma `push` sobre `f9f2afbb`, los cuatro trabajos en `success`, Core 19 070/19 070 y UI 2 404 Passed, 17 Skipped y 2 421 en total;
- avisos: los de PB-T0, Evidence 26, que el GREEN de PB-T0 dejó iguales.

### 2.8 Guardas existentes

Con el RED, que solo añade pruebas, no salta ninguna guarda. Con el GREEN, revisé:
- las del H-18;
- `PushBackSidebarCompositionTests`, `PushBackShellAdoptionTests`, `PushBackClassicCharacterizationResourceTests` y `UiSystemBoundaryGuardTests`;
- las guardas de texto de G-10 e I-55, y las de los módulos, el desviador y el foco;
- toda prueba que lee `RackPushBackSystemWindow.xaml(.cs)`: `git grep` en `tests/` da 9 archivos.

Ninguna salta por el diseño. El H-18 no lista cuatro de ellas, aunque tampoco saltan (M-06).

El único fallo previsible del GREEN es de las pruebas nuevas: CT-PBTC-01 y 02 en F4 (B-01).

### 2.9 Lecciones de PB-T0

- **M-01:** aplicada (D-PBTC-08 e INV-PBTC-10).
- **M-05:** aplicada (INV-PBTC-08). Los diseños de CT-PB0-01 (también X-01, que sale de pb-21-cola) y de CT-PB0-17 no tienen ni estilo ni más de tres decimales.
- **«El prompt del Worker no puede dar latitudes que el contrato no tiene»:** el contrato las crea él mismo en dos sitios:
  - el paso 5 de D-PBTC-01 es falso (I-01): el Worker tiene que elegir entre conservar `Val`, contra el paquete, o parar por PBTC-C07;
  - los formatos sin definir de M-08.
- **M-06 de PB-T0** («guardar la salida de cada medición»): no se aplica a las corridas temporales del §5.2 (M-09).
- **F4-E59, MAX_PATH, teclado y foco:** aplicadas (R-07, R-13 y D-PBTC-09).

### 2.10 Las anclas

Comprobé unas 80, de W, WC, WX, NF, CS, DA, WLC, S0, PB-T0, [OD], [P], AUTOMATION_PLAN, el catálogo de modelos, el bloque M y el Plugin. Casi todas son exactas. Fallan:
- **H-08** (TC:88): «la altura del primer nivel no la reescribe nadie más». `MigrateFirstLevelDatum` (`PushBackEditorState.Load.cs:91-124`) la reescribe al cargar un documento sin marcador de datum. No afecta a CT-PBTC-01, porque `pb-01-base` trae datum 1, pero sí a la afirmación del Owner de TC:806 sobre los racks antiguos (M-05);
- **H-12** (TC:98): las líneas :117, :206 y :285 son lecturas, no asignaciones (M-06);
- **H-18** (TC:126): la guarda de foco citada está saltada (M-06);
- **D-PBTC-01, paso 5** (TC:207), y **ME B-5**: el `grep` de `Val(` debía mostrar W:1865 (I-01);
- **MB H-077 y H-02** (TC:62-66): incluyen F4 como campo que redondea (B-01).

### 2.11 La vuelta al Owner (§8)

**OD-PBTC-1.**
- **Los valores.** La tabla reproduce los de T-3 (entrega de PB-T0, Evidence 31), y la lectura OWNER-4 está bien transmitida.
- **No es fiel a lo medido en cinco puntos:**
  - (1) «once números se guardan redondeados» y «con la corrección se guardan exactos» (TC:801-806) son falsos para el peralte IN/OUT base, que siempre se guarda como el peralte del catálogo (B-01);
  - (2) T-3 mide la diferencia entre dos diseños, no lo que hace RACKEDITAR. No ve la reconstrucción de módulos de F1 (I-02) ni las cajas que siguen redondeando (I-03);
  - (3) R11, «la línea lleva 100,00044 en vez de 100» (TC:823), no se midió; el propio §9 (TC:897) y ME J lo dicen;
  - (4) la recomendación «el efecto es menor que 0,02 pulgadas» (TC:835) ignora que el desplazamiento real de R7 en la planta no se midió (TC:828);
  - (5) solo hay dos opciones; falta la alternativa sobre la regla de «escribir el mismo número» (M-01).

**H-PB-37.** Es cierto por lectura y por WLC: el piloto, con `ComposeForWrite` y `BuildDesign`, conserva los dos valores, y la escritura del panel redibuja desde ese diseño (H-PB-05). No está medido en el DWG. Como está fuente, es aceptable.

**OD-PBTC-2:** correcto, con las omisiones de §2.3 (M-01).

**OD-PBTC-3:** correcto. Le falta la regla del shell «sin un segundo estado en la barra lateral» (M-02).

**OD-PBTC-4:** fiel a T-4 (Evidence 32) y a OWNER-2. Las anclas W:1628-1634 y DA:296-305 son exactas. Las opciones están equilibradas.

**Decisiones materiales sin autoridad que deben ir al Owner antes de emitir:**
- **I-03:** si (a) incluye también `GapBox`, `ModuleLengthBox` y las seis cajas avanzadas, o si van a OB-06.5 antes de PB-T1;
- **la OD-PBTC-1 corregida:** el peralte IN/OUT base, que no es una pérdida, y la reconstrucción de módulos de F1.

## 3. Hallazgos

| Id | Gravedad | Contenido | Evidencia | Corrección propuesta |
|---|---|---|---|---|
| B-01 | **BLOQUEANTE** | **F4 (`BeamDepth`) no es una pérdida de la ventana.** El ensamblador normaliza el peralte IN/OUT al catálogo (`LARGUERO_IN_OUT_C6`, solo «6»), así que con el valor exacto la salida sigue siendo 6. En el GREEN no se cumplirán:<br>• la línea F4 de CT-PBTC-01 (TC:353, «salida= igual a exacto=»);<br>• la línea F4 de CT-PBTC-02 (i) (TC:359).<br>El resultado es PBTC-C07, STOP. Además son falsos:<br>• «S-03 falla en las once filas» (TC:483);<br>• «once números se guardan redondeados... se guardan exactos» (TC:801-806);<br>• el marco de «once cajas» de H-02, INV-PBTC-03 y el registro | DA:165-168; `DynamicLoadBeamGeometry.cs:52-71`; `DynamicRackSystemResolver.cs:57`, `:252` y `:311`; `assets/catalogs/secciones.csv:8`; T-3 R4 «nada»; CT-PB0-19 F4 = R3 por coincidencia | Fijar F4 en CT-PBTC-01 y 02 con su salida real (`salida=6`) y una clase propia definida en el §4.1, por ejemplo `CATALOGO` si la salida es igual a `ResolveBeamDepth(exacto)`. O retirarla de las filas `CONSERVA`, conservando `Keep`, que es inocuo. Corregir:<br>• el §2: un hecho nuevo y H-02;<br>• el §5.1 y el §5.2: S-03 falla en 10 filas;<br>• el registro;<br>• OD-PBTC-1: diez campos, y el peralte IN/OUT siempre es el del catálogo.<br>Volver a comprobar las otras diez filas: ya las comprobé, y pasan tal cual |
| I-01 | IMPORTANTE | **`Val` no queda sin usos:** W:1865 (`ModuleLengthBox`). El paso 5 de D-PBTC-01 rompe la compilación, o deja al Worker elegir entre desviarse o parar por PBTC-C07 | W:1865; TC:207; ME B-5 | Borrar el paso 5, o escribir «`Val` se conserva para W:1865, que PB-TC no toca» |
| I-02 | IMPORTANTE | **Efecto no medido de corregir F1.** Hoy, un fondo con más de tres decimales obliga a reconstruir los módulos en la primera pasada (`SamePallet` con 1e-6). Con la corrección se copia el baseline.<br>Lo que RACKEDITAR escribe puede cambiar más allá del número, en un rack con módulos personalizados.<br>T-3 no lo ve, porque resuelve los diseños sin la ventana. Tampoco INV-PBTC-09, porque su corpus no tiene ese caso | DEA:44-60; DA:197-214; `PushBackFullEditorCharacterizationRoundingEffectTests.cs` (`Compute` con `BuildFrom`); TC:616, TC:813 | Añadir a CT-PBTC-06, o a un CT nuevo, la paridad ventana–piloto en:<br>• cada variante F de `pb-01-base`;<br>• una variante de `pb-31-v2-modulos` o de `pb-02-modulos` con `PalletDepth` + 0,00044 (en el GREEN se espera `delta=[]`).<br>Presentar el efecto en OD-PBTC-1 |
| I-03 | IMPORTANTE | **Siguen redondeando otros lectores, sin que el usuario cambie su valor:**<br>• `GapBox`: basta con pasar por el campo;<br>• `ModuleLengthBox`: se escenifica y se confirma al Actualizar;<br>• las seis cajas avanzadas: se releen todas en cualquier gesto del panel.<br>Son la misma clase que (a), y PB-T0 no los midió: G4 a G8, solo en la carga. Dejan controles como fuente de verdad, que es la razón de [P]:509. El paquete ni los corrige ni los registra para OB-06.5 | WC:168-174; W:1566-1571; W:1592-1607; W:1616-1626; W:1865-1871; W:3346-3349 | Registrarlos en el §2 y en el registro como casos alcanzables de OB-06.5. Presentarlos al Owner en el §8: incluirlos en (a) con `Keep` y sus CT, o llevarlos al Owner antes del contrato de PB-T1 |
| I-04 | IMPORTANTE | **El paso 3 de CT-PBTC-03 no prueba R-03.** Asignar el mismo `Text` no dispara `TextChanged`: equivale a no editar, y no distinguiría una implementación que ignorase la regla. Sin embargo, R-03 y OD-PBTC-1 lo citan como lo que la fija | TC:369; TC:783; TC:831; NF:138-142 | Escribir un texto distinto que se vea igual (por ejemplo «61.2350») y elevar `LostFocus`. La línea esperada conserva 61,23456 |
| M-01 | MENOR | **OD-PBTC-1 y OD-PBTC-2: fidelidad y completitud.**<br>• R11, «la línea lleva 100,00044» (TC:823), no está medido;<br>• «< 0,02 pulgadas» (TC:835) no está establecido para R7 en la planta;<br>• faltan la alternativa sobre la regla de «escribir el mismo número», la extensión explícita fuera de la «ruta de carga» de OB-06.6, que RACKEDITAR deja de borrar el estilo del sobre, y el efecto en la biblioteca e «Insertar» | TC:823, :828, :834-836, :838-848; [P]:395; ME J | Reescribir las frases como no medidas; añadir la opción y las consecuencias |
| M-02 | MENOR | **El aviso en la barra lateral contradice la regla declarada del shell** («no second status inside the sidebar»), con `FontSize` fijo. Además, «vale en los dos modos» no está demostrado | WX:678-679; `PushBackShellAdoptionTests.cs:15-20`; WX:166; TC:247, :861 | Mostrárselo al Owner en OD-PBTC-3, o proponer ponerlo dentro de `CompositeSection`. Retirar o matizar «vale en los dos modos» |
| M-03 | MENOR | **`ForbiddenWriteScope` no sigue el método de PB-T0.** Faltan los hermanos de `Systems/PushBack`, los demás sistemas y carpetas de `src/RackCad.UI` y las guardas de UI que leen la ventana, incluidos los hermanos de `Workspace/` | Contrato de PB-T0, `ForbiddenWriteScope`; TC:547-556 | Listarlos uno a uno, como hizo PB-T0 |
| M-04 | MENOR | **CT-PBTC-09 es decorativo:** no puede fallar con INV-PBTC-01, y Cantilever no usa el formato por defecto | TC:430-436; H-PB-34 | Declarar que la prueba de OB-06.6 son INV-PBTC-01 y el filtro de 67, o retirar CT-PBTC-09 |
| M-05 | MENOR | **H-08 es inexacto:** `MigrateFirstLevelDatum` reescribe el primer nivel de los documentos sin datum. «Un rack con tres decimales o menos no cambia en nada» no está garantizado para ellos | `PushBackEditorState.Load.cs:91-124`; TC:88, :806 | Corregir H-08 y la frase del §8 |
| M-06 | MENOR | **Anclas imprecisas:**<br>• H-12: :117, :206 y :285 son lecturas;<br>• H-18: la guarda de foco citada está saltada;<br>• faltan en H-18 cuatro guardas revisadas | `DynamicViewDecorations.cs:117-402`; `RichEditorCharacterizationTests.cs:288`; TC:98, :126 | Corregir las citas y completar H-18 |
| M-07 | MENOR | **Trazabilidad y vínculos.**<br>• El §4.3 no tiene filas para los pasos 3 y 6.<br>• El aviso en WORKSPACE no está en la fila de PB-T2 de [P].<br>• El aplazamiento del paso 4 en el Workspace no se informa al Owner | TC:438-447, :509-511; [P]:504; [OD]:144-147 | Añadir las filas. Anotarlo en `f4b-dispositions.md` como obligación de PB-T2 e informarlo en el §8 |
| M-08 | MENOR | **Formatos sin definir que dan latitud al Worker:**<br>• `vistas`;<br>• `AnnotationScaleBox=2.5`: ¿texto o valor?;<br>• el separador de `estilos`;<br>• las comillas angulares exteriores del literal (TC:252 frente a TC:411) | TC:359, :368, :396, :252, :411 | Definir cada uno en el §4.1, con el literal en un bloque de código |
| M-09 | MENOR | **M-06 de PB-T0 no se aplica** a las corridas temporales del §5.2 | Cierre de PB-T0, `:93`; TC:480-483, :671 | Exigir que su TRX y su log se guarden como artefactos y se citen en la entrega |
| M-10 | MENOR | **INV-PBTC-03 e INV-PBTC-15 no son mecánicos** tal como están escritos | TC:591-594, :641-643 | Reformularlos como comprobaciones del diff, o como CT que los fije |

## 4. Veredicto

**NO EMITIBLE** en su v1. Recuento: **1 BLOQUEANTE, 4 IMPORTANTES y 10 MENORES (1/4/10).**

- **B-01** haría fallar el GREEN en CT-PBTC-01 y CT-PBTC-02, y acabaría en STOP por PBTC-C07. También hace falsa la descripción que se lleva al Owner.
- **I-01** rompe la compilación si se sigue el paquete al pie de la letra.
- **I-02 e I-03** son efectos y casos de la misma pérdida que ni se midieron ni se presentaron.
- **I-04** es una prueba que no fija lo que se declara al Owner.

Todas tienen una corrección acotada. La v2 debería pasar por una comprobación acotada antes de emitirse.

**Vuelve al Owner, en la vuelta ya prevista del §8:**
- OD-PBTC-1 corregida:
  - son diez los campos, y el peralte IN/OUT base no es una pérdida (B-01);
  - el efecto de F1 sobre la reconstrucción de los módulos (I-02);
  - las frases sin medir (M-01);
- **decisión nueva (I-03):** incluir en (a) `GapBox`, `ModuleLengthBox` y las seis cajas avanzadas, o dejarlos para OB-06.5 antes del contrato de PB-T1;
- OD-PBTC-3, con la regla del shell delante (M-02);
- como información, el aplazamiento a PB-T2 del paso 4 en el Workspace y del aviso en el modo WORKSPACE (M-07).
