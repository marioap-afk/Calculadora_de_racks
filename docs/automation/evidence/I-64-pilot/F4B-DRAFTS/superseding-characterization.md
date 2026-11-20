# I-64 F4B PB-TC — Las cuatro pruebas de caracterización que se sustituyen, y por qué

> La caracterización previa es **inmutable** (ADR-0029 D13: «la caracterización no se edita para que pase»). Cuatro pruebas
> fijan hoy comportamientos que PB-TC corrige como **cambios declarados** del editor clásico de Push Back. **No se reescriben**:
> se conservan intactas, con su texto original, marcadas `Skip` con un motivo que nombra este registro. El comportamiento nuevo
> vive en clases separadas.

**Estado: EJECUTADA** (2026-11-18, cierre de PB-TC, F4B-E08): RED `f665edac046014533f031adfa797291a3d027df2`, GREEN `4440f67d4c76fb2ec019a566231353acdcca61f6`. Lo escribe la sesión principal; el Worker no escribe en este archivo.

**Autoridad:**
- **D-PB-06**, en la disposición del Owner y del Coordinator de F4B, [OD] (`docs/automation/evidence/I-64-pilot/F4B-DRAFTS/coordinator/owner-f4b-disposition.md:130-155`): las correcciones (a), el redondeo, y (b), el estilo de cotas. Su paso 6 (`:149-150`) dice que es un «cambio intencionado del editor clásico, no "no observable change"».
- **D-PB-08**, en [OD] (`:62-74`): el aviso no bloqueante del lado B sin frentes activos.
- **D-PB-11**, por la decisión del Owner F4B-E06 (`docs/automation/evidence/I-64-pilot/F4B-DRAFTS/coordinator/owner-pbtc-decisions.md`, pregunta 4: OD-PBTC-4 = (A)): «Restaurar estándar» llega al Actualizar (H-PB-28).
- **OB-T1B.2**, en el paquete de la fase (`docs/initiatives/I-64-f4b-push-back-completo-package.md:700-710`): una caracterización vigente de la ventana clásica solo cambia por una sustitución declarada.
- **El contrato:** `docs/initiatives/I-64-f4b-pbtc-gate-contract-package.md` (v2.2), §5.2, y `docs/automation/evidence/I-64-pilot/F4B-PBTC-DECLARED-CORRECTIONS/coordinator/gate-contract.json` (INV-PBTC-06).

La transición se lee entera en tres sitios:
1. **La base, `f9f2afbb`.** Ahí las cuatro pruebas viejas corren **en verde** (CI `44188678597`: Core 19 070 de 19 070; UI 2 404 Passed y 17 Skipped).
2. **Las decisiones:** [OD] D-PB-06, para SUS-01 a SUS-03, y F4B-E06 D-PB-11, para SUS-04.
3. **Las pruebas nuevas,** en `tests/RackCad.UI.Tests/PushBackDeclaredCorrection*Tests.cs`, que prueban el comportamiento nuevo.

Ninguna prueba que describía el comportamiento antiguo se convierte para fingir que siempre esperó el nuevo. En cada archivo viejo, el único cambio es la línea del atributo.

## SUS-01 — La ventana y el piloto escriben lo mismo

| | |
|---|---|
| **Base** | `WorkspaceDraftPilotWindowLoadCharacterizationTests.TheWindowAndThePilotDifferOnlyInTheKnownClasses`: `[Theory]` en `tests/RackCad.UI.Tests/Workspace/WorkspaceDraftPilotWindowLoadCharacterizationTests.cs:231`, 14 filas |
| **Contrato** | `PushBackDeclaredCorrectionPilotParityTests.CT_PBTC_06_TheWindowAndThePilotWriteTheSameDesignForTheFourteenDesigns` |
| **Autoriza** | [OD] **D-PB-06**, (a) y (b) |

**El `Skip`, literal:**

```text
SUSTITUIDA por PB-TC (D-PB-06), registro docs/automation/evidence/I-64-pilot/F4B-DRAFTS/superseding-characterization.md fila SUS-01: la ejecuta PushBackDeclaredCorrectionPilotParityTests.CT_PBTC_06_TheWindowAndThePilotWriteTheSameDesignForTheFourteenDesigns
```

**Aserción base,** literal y conservada (`:184-204`): el delta fijado de cada diseño. Once filas vacías, y estas tres con línea:

```csharp
["pb-06-seguridad"] = new[] { "$.PushBack.Structure.DimensionStyle: null -> \"STANDARD-ISO\"" },
["pb-07-combinado"] = new[] { "$.PushBack.Structure.DimensionStyle: null -> \"SIMPLEX\"" },
["pb-08-redondeo"] = new[]
{
    "$.PushBack.Structure.AnnotationScale: 1.235 -> 1.23456",
    "$.PushBack.Structure.Fronts[0].Levels[0].PalletHeight: 61.235 -> 61.23456",
},
```

La Theory exige además que ninguna línea quede fuera de las clases (a) a (d) (`:248`).

**Aserción nueva:** los 14 diseños, admitidos (`admitida=True`) y con `delta=[]`.

**Las once filas que no cambian:** `pb-01-base`, `pb-02-modulos`, `pb-03-fondos`, `pb-04-blanco`, `pb-05-topes`, `pb-10-sin-datum`, `pb-11-sin-i41`, `pb-12-poste-nulo`, `pb-13-sideb-no-presente`, `pb-14-peralte-heredado` y `pb-15-sin-composite` siguen con delta vacío.

**Qué lo hace legítimo:** las tres líneas fijadas eran exactamente las pérdidas (b) y (a) que D-PB-06 manda corregir. La prueba nueva cubre **las mismas 14 filas** y es **más fuerte**: no admite ninguna línea, y la rama «WINDOW-NOT-ADMITTED» (`:237-241`) pasa a ser la aserción `admitida=True` en las 14.

## SUS-02 — El estilo de cotas cargado

| | |
|---|---|
| **Base** | `PushBackDimensionViewsWindowTests.T19_H1_TheDimensionStyleStillComesOutNull`: `[Fact]` en `tests/RackCad.UI.Tests/PushBackDimensionViewsWindowTests.cs:308` |
| **Contrato** | `PushBackDeclaredCorrectionDimensionStyleTests.CT_PBTC_04_TheLoadedDimensionStyleSurvivesTheLoadsAndTheGestures` |
| **Autoriza** | [OD] **D-PB-06**, (b) |

**El `Skip`, literal:**

```text
SUSTITUIDA por PB-TC (D-PB-06), registro docs/automation/evidence/I-64-pilot/F4B-DRAFTS/superseding-characterization.md fila SUS-02: la ejecuta PushBackDeclaredCorrectionDimensionStyleTests.CT_PBTC_04_TheLoadedDimensionStyleSurvivesTheLoadsAndTheGestures
```

**Aserción base,** literal y conservada (`:320-321`), sobre un diseño de un sentido con `DimensionStyle = "ESTILO_PB"`, tras la carga y tras desmarcar la casilla lateral de cotas:

```csharp
Assert.Null(afterLoad);
Assert.Null(afterGesture);
```

**Aserción nueva:** el estilo cargado sale tras la carga y tras el mismo gesto (`ESTILO_PB`). Además, `pb-06-seguridad` da `STANDARD-ISO`, `pb-07-combinado` da `SIMPLEX` (también cargado de la biblioteca), y un rack nuevo sigue sin estilo.

**Qué lo hace legítimo:** la prueba vieja se describía a sí misma como «caracterizado y NO corregido» (`:306-307`). D-PB-06 (b) lo corrige. La prueba nueva repite **la misma construcción y el mismo gesto**, y añade dos diseños del corpus, la biblioteca y un rack nuevo.

## SUS-03 — Los campos que pasan por una caja formateada

| | |
|---|---|
| **Base** | `PushBackFullEditorCharacterizationRoundingLossTests.CT_PB0_19_TheFieldsThatPassThroughAFormattedControlComeBackRoundedToThreeDecimals`: `[Fact]` en `tests/RackCad.UI.Tests/PushBackFullEditorCharacterizationRoundingLossTests.cs:129` |
| **Contrato** | `PushBackDeclaredCorrectionRoundingTests.CT_PBTC_01_TheLoadedFieldsKeepTheirExactValueAfterLoading` |
| **Autoriza** | [OD] **D-PB-06**, (a) |

**El `Skip`, literal:**

```text
SUSTITUIDA por PB-TC (D-PB-06), registro docs/automation/evidence/I-64-pilot/F4B-DRAFTS/superseding-characterization.md fila SUS-03: la ejecuta PushBackDeclaredCorrectionRoundingTests.CT_PBTC_01_TheLoadedFieldsKeepTheirExactValueAfterLoading
```

**Aserción base,** literal y conservada (`:138-148`):

```text
F1 pb-01-base Structure.PalletDepth exacto=48.00044 salida=48 REDONDEA
F2 pb-01-base Structure.PostPeralte exacto=3.00044 salida=3 REDONDEA
F3 pb-01-base Structure.PalletTolerance exacto=4.00044 salida=4 REDONDEA
F4 pb-01-base Structure.BeamDepth exacto=6.00044 salida=6 REDONDEA
F5 pb-01-base Structure.AnnotationScale exacto=1.50044 salida=1.5 REDONDEA
F6 pb-01-base Structure.Fronts[0].FirstLevelHeight exacto=4.00044 salida=4 REDONDEA
F7 pb-01-base Structure.Fronts[0].Levels[0].PalletFront exacto=42.00044 salida=42 REDONDEA
F8 pb-01-base Structure.Fronts[0].Levels[0].PalletHeight exacto=60.00044 salida=60 REDONDEA
F9 pb-01-base Structure.Fronts[0].Levels[0].PalletWeight exacto=1000.00044 salida=1000 REDONDEA
F10 pb-01-base Structure.Fronts[0].Levels[0].ClearHeight exacto=6.00044 salida=6 REDONDEA
F11 pb-01-base Structure.Fronts[0].Levels[0].BeamLengthOverride exacto=100.00044 salida=100 REDONDEA
```

**Aserción nueva:** diez filas con `salida=` igual al exacto y la clase `CONSERVA`, y F4 con `F4 pb-01-base Structure.BeamDepth exacto=6.00044 salida=6 CATALOGO`.

**F4 no era una pérdida de la ventana.** El ensamblador normaliza el peralte IN/OUT base al catálogo (`LARGUERO_IN_OUT_C6`, con un solo peralte permitido, `6`), con la caja o sin ella. La base lo clasificaba `REDONDEA` solo porque el redondeo a tres decimales de 6,00044 coincide con el catálogo. **Su salida sigue siendo 6**; solo cambia su clase.

**Qué lo hace legítimo:** las diez filas restantes eran exactamente la pérdida (a) que D-PB-06 manda corregir. La prueba nueva cubre **las mismas once filas, con la misma construcción**. Otras pruebas nuevas añaden la petición de RACKEDITAR, la biblioteca, la edición, la selección de celda y la paridad con el piloto.

## SUS-04 — «Restaurar estándar» y los seis parámetros avanzados

| | |
|---|---|
| **Base** | `PushBackFullEditorCharacterizationModuleTests.CT_PB0_05_TheSixAdvancedParametersAfterRestoreStandardAreResetInTheOutputAndComeBackOnUpdate`: `[Fact]` en `tests/RackCad.UI.Tests/PushBackFullEditorCharacterizationModuleTests.cs:170` |
| **Contrato** | `PushBackDeclaredCorrectionStandardRestoreTests.CT_PBTC_14_AStandardRestoreResetsTheAdvancedControlsAndReachesTheUpdate` |
| **Autoriza** | **D-PB-11**, por F4B-E06 (OD-PBTC-4 = (A)) |

**El `Skip`, literal:**

```text
SUSTITUIDA por PB-TC (D-PB-11), registro docs/automation/evidence/I-64-pilot/F4B-DRAFTS/superseding-characterization.md fila SUS-04: la ejecuta PushBackDeclaredCorrectionStandardRestoreTests.CT_PBTC_14_AStandardRestoreResetsTheAdvancedControlsAndReachesTheUpdate
```

**Aserción base,** literal y conservada. Son diez líneas (`:199-208`), en dos diseños (`pb-06-seguridad` y `pb-07-combinado`) y tres momentos:
- **A0:** la carga;
- **A1:** tras «Restaurar estándar» y «Confirmar»;
- **A2:** la petición de «Actualizar».

Seis no cambian: A0 salida, A0 controles y A1 salida, en `:199-201` y `:204-206`. Cambian estas cuatro (`:202`, `:203`, `:207` y `:208`):

```text
pb-06-seguridad A1 controles: RackHeaderHeightBox=[150] DerivedPostReinforcedCheck=False DerivedPostReinforcementHeightBox=[] DerivedPostHeightBox=[] RackSeparatorCountBox=[3] RackSeparatorSpacingBox=[18]
pb-06-seguridad A2 peticion: ManualHeaderHeightOverride=150,DerivedPostReinforced=false,DerivedPostReinforcementHeight=null,DerivedPostHeight=null,SeparatorCountOverride=3,SeparatorSpacingOverride=18
pb-07-combinado A1 controles: RackHeaderHeightBox=[] DerivedPostReinforcedCheck=True DerivedPostReinforcementHeightBox=[] DerivedPostHeightBox=[140] RackSeparatorCountBox=[] RackSeparatorSpacingBox=[]
pb-07-combinado A2 peticion: ManualHeaderHeightOverride=null,DerivedPostReinforced=true,DerivedPostReinforcementHeight=null,DerivedPostHeight=140,SeparatorCountOverride=null,SeparatorSpacingOverride=null
```

La prueba vieja lo comenta como «INCUMPLIMIENTO CARACTERIZADO… NO se corrige» (`:160-168`): la salida de A1 ya está reiniciada, pero los controles y la petición vuelven a lo cargado.

**Aserción nueva:** las mismas seis líneas sin cambio y, en lugar de las cuatro de arriba, estas:

```text
pb-06-seguridad A1 controles: RackHeaderHeightBox=[] DerivedPostReinforcedCheck=True DerivedPostReinforcementHeightBox=[] DerivedPostHeightBox=[] RackSeparatorCountBox=[] RackSeparatorSpacingBox=[]
pb-06-seguridad A2 peticion: ManualHeaderHeightOverride=null,DerivedPostReinforced=true,DerivedPostReinforcementHeight=null,DerivedPostHeight=null,SeparatorCountOverride=null,SeparatorSpacingOverride=null
pb-07-combinado A1 controles: RackHeaderHeightBox=[] DerivedPostReinforcedCheck=True DerivedPostReinforcementHeightBox=[] DerivedPostHeightBox=[] RackSeparatorCountBox=[] RackSeparatorSpacingBox=[]
pb-07-combinado A2 peticion: ManualHeaderHeightOverride=null,DerivedPostReinforced=true,DerivedPostReinforcementHeight=null,DerivedPostHeight=null,SeparatorCountOverride=null,SeparatorSpacingOverride=null
```

**Qué lo hace legítimo:** el ensamblador ya declara que un «Restaurar estándar» reinicia todo el rack, avanzados incluidos, y la salida de A1 lo cumple. La pérdida estaba en que la ventana no reiniciaba su portador ni sus controles. El Owner decidió corregirlo en PB-TC como D-PB-11 (F4B-E06). La prueba nueva cubre **los mismos dos diseños, los mismos pasos y las mismas diez líneas**: seis no cambian y cuatro se corrigen.

## Lo que no se sustituye

| Prueba | Motivo |
|---|---|
| `WorkspaceDraftPilotWindowLoadCharacterizationTests.ClassAIsEvidenced…` (`:254-264`) | Solo asevera sobre el diccionario estático de la base. Sigue en verde sin cambio, y documenta lo que era la base |
| Las demás de WLC, el corpus y `…KeepMine…` (`:270-332`) | Sus valores tienen tres decimales o menos y no llevan estilo. No chocan |
| CT-PB0-01 a CT-PB0-04, CT-PB0-06 a CT-PB0-18, CT-PB0-20 y CT-PB0-21 (PB-T0) | No fijan ni la pérdida (a), ni la (b), ni H-PB-28, o comparan dos salidas de la misma ventana. Siguen en verde |
| T1B (55 en UI y 10 en Core) y sus guardas | El cambio no toca nada de lo que miran. Siguen en verde, sin sustitución (OB-T1B.3) |

## Pendientes de PB-T2

Anotados también en `docs/automation/evidence/I-64-pilot/F4B-DRAFTS/f4b-dispositions.md`:
- **el paso 4 de D-PB-06 en modo WORKSPACE** (OB-06.3). PB-TC lo prueba solo en RACKEDITAR, la ventana en modo clásico;
- **el aviso de D-PB-08 en modo WORKSPACE** (OB-08.2). En el modo clásico, «así no se guardará» es exacto; en WORKSPACE no está demostrado.
- **las correcciones de PB-TC2 en modo WORKSPACE** (OB-06.3): ver la sección de PB-TC2 (N-03 de su comprobación acotada).

## Pendiente: decisión del Owner antes de PB-T1

Son casos alcanzables de la misma pérdida (a) que PB-TC **no** corrige, por la decisión del Owner OD-PBTC-5 = (B) (F4B-E06, pregunta 4). Vuelven al Owner antes del contrato de PB-T1, por OB-06.5:
- `GapBox`, el hueco entre lados;
- `ModuleLengthBox`, la longitud del módulo elegido;
- las cuatro cajas avanzadas anulables: `ManualHeaderHeightOverride`, `DerivedPostReinforcementHeight`, `DerivedPostHeight` y `SeparatorSpacingOverride`.

Hasta esa decisión, siguen redondeando como en la base, y PB-TC no las toca (INV-PBTC-15 y PBTC-C10).

## El cambio de comportamiento declarado de RACKEDITAR

- **(a) Los decimales** (D-PB-06; OD-PBTC-1 (i), con R-03 (i)):
  - diez campos dejan de guardarse redondeados a tres decimales: el fondo y la tolerancia de la tarima, el peralte del poste, la escala de anotación, la altura del primer nivel, y en la celda el frente, el alto, el peso, el claro y el largo manual. El peralte IN/OUT base sigue siendo el del catálogo;
  - vale al abrir, al cambiar de celda, con «Ambos lados», al aplicar a otras celdas, en los datos del frente y al cargar de la biblioteca;
  - escribir en una caja algo que se ve igual a lo que muestra conserva el valor exacto, y escribir algo distinto manda;
  - un fondo exacto ya no hace que RACKEDITAR reconstruya la secuencia de módulos en la primera pasada: la copia, como el panel del Workspace.
- **(b) El estilo de cotas** (D-PB-06; OD-PBTC-2 (i)): RACKEDITAR conserva el estilo guardado y deja de borrarlo del rack. Si el dibujo tiene un estilo con ese nombre, las cotas salen con él; si no, como hoy. Lo mismo al insertar una vista o al guardar en la biblioteca.
- **El aviso** (D-PB-08; OD-PBTC-3): con el lado B declarado y sin ningún frente de B activo, la franja de estado muestra este texto, que no bloquea nada:

  ```text
  Lado B declarado sin ningún frente activo: así no se guardará. Para conservarlo, elige «Lado B» y desmarca «En blanco» en al menos un frente.
  ```

- **«Restaurar estándar»** (D-PB-11; OD-PBTC-4 (A)): tras «Restaurar estándar» y «Confirmar», los seis controles avanzados muestran el estándar, y «Actualizar» escribe lo restaurado. Cambia lo que se dibuja en ese flujo: la altura de cabecera, el poste derivado, su refuerzo y los separadores.

## Ejecución (se completa al cerrar)

| | |
|---|---|
| **RED** | `f665edac046014533f031adfa797291a3d027df2` (CI `44223960356`: ui-tests en failure por los 10 métodos nuevos; tests, build-ui y build-plugin en success) |
| **GREEN** | `4440f67d4c76fb2ec019a566231353acdcca61f6` (CI `44225456328` 4/4; UI 2 397 Passed y 21 Skipped de 2 418; Core 19 070/19 070) |
| **Comprobación temporal** (las cuatro viejas sin `Skip` sobre el GREEN; §5.2 del paquete) | Conforme con el §5.2: SUS-01 falla en pb-06, pb-07 y pb-08 y pasan 11; SUS-02 falla en el estilo; SUS-03 difiere en 10 líneas y F4 coincide; SUS-04 difiere en 4 y coinciden 6 (TRX y logs `tests/s1` a `tests/s4` de la entrega del Worker `R20261118T021306Z-069b`, por SHA-256 en su manifiesto) |
| **Estado** | EJECUTADA |

## Limitación conocida declarada al cerrar (OWNER-1, F4B-E08)

En «Ambos lados», con una caja **mixta**, escribir el número que se ve del valor exacto de un lado no iguala los dos lados: ese lado conserva su exacto y la caja vuelve a quedar mixta (diferencia menor que 0,0005). La encontró la revisión independiente (M-01); es un hueco del diseño del paquete, no una desviación. El Owner eligió «Registrar y arreglarlo luego (Recomendado)» (`F4B-DRAFTS/coordinator/owner-pbtc-mixed-decision.md`): la propuesta de arreglo vuelve al Owner antes del contrato de PB-T1, junto con el caso pendiente de arriba.

<!-- Texto de emisión para docs/automation/evidence/I-64-pilot/F4B-DRAFTS/superseding-characterization.md.
     Bloque 1: la sección nueva, que se añade al final del archivo.
     Bloques 2 y 3: las notas de cierre, cada una al final de la sección que cierra. -->

## PB-TC2 — Las cajas que aún redondeaban y la caja mixta de «Ambos lados» (F4B-E09)

**Estado: EJECUTADA** (2026-11-18, cierre de PB-TC2, F4B-E12): RED `3677847c59e92b34567fa42c600eb2c01e65576c`, GREEN `b5c09f0bd741f67b7dd8311dce65c26e3d4cf3c4`. Lo escribe la sesión principal; el Worker no escribe en este archivo.

**Autoridad:**
- **D-PB-06**, en [OD] (`docs/automation/evidence/I-64-pilot/F4B-DRAFTS/coordinator/owner-f4b-disposition.md:130-155`): «rounding behavior/state», y «Do not hide these corrections inside PB-T1 state extraction».
- **La decisión del Owner F4B-E09**, pregunta 1, «Corregir en PB-TC2 (Recomendado)» (`docs/automation/evidence/I-64-pilot/F4B-DRAFTS/coordinator/owner-pbt1-pre-decisions.md`), con su autorización condicional de emisión (pregunta 4).
- **El contrato:**
  - el paquete `docs/initiatives/I-64-f4b-pbtc2-gate-contract-package.md` (v2);
  - `docs/automation/evidence/I-64-pilot/F4B-PBTC2-DECLARED-CORRECTIONS/coordinator/gate-contract.json`.

**Sustituciones: ninguna.**
- Ninguna prueba existente fija el redondeo de estas cajas ni el «Ambos lados» corregido. Todas las que las tocan escriben valores que se ven distintos, valores enteros o el valor que la caja ya tiene, y con tres decimales o menos la regla nueva da el mismo número.
- Ninguna prueba cambia de atributo, y no hay filas `SUS2-0n`.

**El cambio de comportamiento declarado de RACKEDITAR:**
- **Las cajas que aún redondeaban** (H-31) dejan de guardarse redondeadas a tres decimales con solo pasar por ellas:
  - el hueco entre lados;
  - la longitud del módulo elegido;
  - las cuatro cajas avanzadas anulables: la altura manual de la cabecera, la altura del refuerzo del poste derivado, la altura del poste derivado y la separación entre separadores.

  Escribir algo que se ve igual a lo que muestra la caja conserva el valor exacto, y escribir algo distinto manda. Vaciar una caja avanzada sigue siendo «el cálculo».
- **Pasar por la longitud de un módulo calculado ya no lo convierte en manual.** Escribir una longitud distinta sí lo convierte, como hasta ahora.
- **En «Ambos lados», con el lado B presente,** una caja mixta (vacía porque los dos lados difieren) que recibe un valor lo aplica a los dos lados, como dice su ayuda. Vale al salir de la caja, al cambiar de celda, al aplicar a un alcance y con los datos del frente (estos dos, sin prueba propia; ver «Lo que sigue pendiente»). Una caja mixta que se deja vacía sigue dejando a cada lado el suyo.
- **Sin lado B, o fuera de «Ambos lados»,** una caja sin editar conserva el valor exacto, aunque el indicador mixto haya quedado puesto.
- **Con tres decimales o menos, nada cambia.**

**Lo que sigue pendiente:**
- **De PB-T2:** probar estas correcciones también en el modo WORKSPACE de la ventana (OB-06.3). Anotado también en `docs/automation/evidence/I-64-pilot/F4B-DRAFTS/f4b-dispositions.md`.
- **Del paquete de PB-T1, antes de su contrato:**
  - la regla de «Ambos lados» en el alcance y en los datos del frente, que no tiene prueba propia (red de PB-T1);
  - un defecto anterior, que PB-TC2 conserva tal cual: sin lado B y con el indicador mixto obsoleto, vaciar «Largo manual» no surte efecto, y las cajas obligatorias quedan opcionales. PB-T1 decide si lo reproduce o lo corrige.

### Ejecución de PB-TC2 (se completa al cerrar)

| | |
|---|---|
| **RED** | `3677847c59e92b34567fa42c600eb2c01e65576c` (CI `44298811020`: ui-tests en failure por los 4 métodos nuevos; tests, build-ui y build-plugin en success) |
| **GREEN** | `b5c09f0bd741f67b7dd8311dce65c26e3d4cf3c4` (CI `44300874797` 4/4; UI 2 401 Passed y 21 Skipped de 2 422; Core 19 070/19 070) |
| **Medición previa de la base** (el efecto del hueco, el módulo y las avanzadas exactas; la parada por efecto material) | Primera invocación `R20261118T055535Z-38d3`: parada prevista PBTC2-C14 en T2-3 (postes 150 x32 y 150,0004 x4 frente a 150 x36), sin commits; el Owner ratificó el efecto y ordenó reanudar sin gastar intento (D1, F4B-E11). Segunda invocación `R20261118T113927Z-037c`: la medición `m8b` es idéntica línea a línea; K1, K1o, K2 y K4 vacías en las seis filas; MaxDelta 0,00044 |
| **Estado** | EJECUTADA |

<!-- Bloque 2: al final de «## Pendiente: decisión del Owner antes de PB-T1» -->

**Decidido por el Owner en F4B-E09** (pregunta 1, «Corregir en PB-TC2 (Recomendado)»): estas cajas se corrigen en la tarea PB-TC2, con la regla `Keep` de PB-TC (sección «PB-TC2», arriba). **Cerrado por PB-TC2 (GREEN `b5c09f0b`).**

<!-- Bloque 3: al final de «## Limitación conocida declarada al cerrar (OWNER-1, F4B-E08)» -->

**Decidido por el Owner en F4B-E09** (pregunta 1): la caja mixta de «Ambos lados» que recibe un valor lo aplica a los dos lados, con el lado B presente. Se corrige en la tarea PB-TC2 (sección «PB-TC2»). **Cerrado por PB-TC2 (GREEN `b5c09f0b`).**

**Notas del cierre de PB-TC2 (F4B-E12):**
- La regla R-03 (i) (escribir algo que se ve igual conserva el exacto) vale para las seis cajas de PB-TC2, no solo para la longitud del módulo (IRC-M04 de la revisión independiente).
- La línea del BOM que el Owner ratificó en D1 (los postes de 150,0004) sale con la descripción larga del catálogo, distinta de la de los postes de 150, con el mismo `ProfileId`: `BomBuilder.Merge` agrupa por categoría, perfil y longitud y toma la primera descripción. Ya ocurría sin tocar el panel de avanzados; cae en las reglas de presentación existentes que D1 admite (IRC-O01, informativo).

## PB-T3b — Las seis guardas de F4 que se sustituyen por D-1 (F4B-E29)

**Estado: EMITIDA** (2026-11-20, emisión de PB-T3b, F4B-E44, sobre el cierre de PB-T3a `c40eea4a`). Lo escribe la sesión principal; el Worker no escribe en este archivo. La ejecución se completa al cerrar.

**Autoridad:**
- **[OD], la fila de PB-T3 y D-PB-04** (`docs/automation/evidence/I-64-pilot/F4B-DRAFTS/coordinator/owner-f4b-disposition.md`): borrar sí; y el bloque sobre T1B, «create a declared superseding characterization/change record. Do not silently weaken the old guard».
- **D-1, decidida por el Owner en F4B-E29** (`docs/automation/evidence/I-64-pilot/F4B-DRAFTS/coordinator/owner-pbt3-d1-decision.md`): «Un solo código (Recomendado)», la variante (A).
- **El contrato:** `docs/initiatives/I-64-f4b-pbt3-gate-contract-package.md` (v4.5), §5.3, §6.2 y §8.2, y `docs/automation/evidence/I-64-pilot/F4B-PBT3B-FULL-WRITE-RUN/coordinator/gate-contract.json`.

**El motivo:** con (A), el plan y la corrida del piloto se generalizan en su sitio (`BuildCore` y `Execution<TState>`), y estas seis guardas de F4 fijan por su letra la forma de hoy: cinco fallan y S-6 queda vacía (R-6 del registro de mediciones). No se editan: cada una queda con `Skip` y su equivalente, que se ejecuta y **empieza afirmando la forma nueva**, cubre lo que cubría.

**El `Skip`, literal** (§5.3; `<E-n>` es el equivalente de cada método):

```text
I-64 F4B PB-T3b: sustituida por <E-n> de PushBackPilotUpdateEquivalenceGuardTests (registro declarado superseding-characterization.md; decisión del Owner F4B-E29, D-1).
```

| Id | La prueba vieja (con `Skip`) | El equivalente (`PushBackPilotUpdateEquivalenceGuardTests`) | Qué fija el equivalente |
|---|---|---|---|
| S-1 | `WorkspacePilotPrepareGuardTests.TheFileOfThePlanComposesExactlyOnceInAMemberOfItsOwnWithAParameterSourceAndTheNameOfTheRereadEnvelope` (`[Fact]`, `tests/RackCad.Tests/Workspace/WorkspacePilotPrepareGuardTests.cs:121`) | E-1 | El plan compone una vez, en `ComposeMemberEnvelope(RackEmbedDocument source, string name, string view, int section, string designJson)`, con el nombre de una sola expresión de `BuildCore` |
| S-2 | `WorkspacePilotPrepareGuardTests.TheComposeGuardTripsOnAnotherCallAnUnsourcedCompositionANullableSourceOrAnotherName` (`[Theory]` de cinco filas, `:127`) | E-2 | Empieza afirmando la forma nueva; después, las autopruebas sobre el texto real hacen fallar E-1 |
| S-3 | `WorkspacePilotPrepareGuardTests.TheMemberPlansAreBuiltOnlyFromTheMutableMembersOfTheRereadInOrdinalOrderAndNeverErase` (`[Fact]`, `:229`) | E-3 | Las agujas de S-3 sobre `BuildCore` y `PushBackViewDispositions.Of`; `Erasures` solo con borrado admitido y solo con `ObsoleteLateral` |
| S-4 | `WorkspacePilotUpdateGuardTests.TheRunReleasesBothScopesInFinallyBlocksOfItsOwnAndNeverWithAUsing` (`[Fact]`, `tests/RackCad.Tests/Workspace/WorkspacePilotUpdateGuardTests.cs:126`) | E-4 | Las mismas aserciones sobre `Execution<TState>`: los dos ámbitos en `finally` propios y POST tras liberar MUTATE |
| S-5 | `WorkspacePilotUpdateIntegrationGuardTests.M01TheEscapeOfTheRunIsClassifiedByTheStateThatTheRunAlreadyHadAndNeverAsAPlainFailureAfterAWriteOrASeal` (`[Fact]`, `tests/RackCad.Tests/Workspace/WorkspacePilotUpdateIntegrationGuardTests.cs:895`) | E-5 | Las reglas de `RunEscapeViolations` sobre las dos `Execute` y sobre `OutcomeOfAnEscape` de `Execution<TState>` |
| S-6 | `WorkspacePilotUpdateIntegrationGuardTests.M01TheEscapeGuardTripsOnACatchThatDoesNotConsultTheStateAndOnAnEscapeThatAnswersAPlainFailureFirst` (`[Fact]`, `:902`) | E-6 | Empieza afirmando la forma nueva; después, las tres autopruebas adaptadas al tipo genérico, que cambian el texto y dan violaciones |

**La medición sobre el GREEN** (INV-PBT3B-15): el Worker quita en local el `Skip`, corre los seis sobre el texto del GREEN y lo vuelve a poner antes del commit. Lo esperado con (A): S-1, S-3, S-4 y S-5 fallan; en S-2, las filas 1 y 3 fallan y las 2, 4 y 5 pasan en vacío; S-6 pasa en vacío. Cualquier otra diferencia es PBT3B-C16 y vuelve al Owner.

**Lo que queda para PB-T4** (§5.3): al quitar el piloto, PB-T4 sustituye con registro E-3, E-5 y E-6.

### Ejecución de PB-T3b (se completa al cerrar)

| | |
|---|---|
| **RED** | (al cerrar) |
| **GREEN** | (al cerrar) |
| **La medición sobre el GREEN** | (al cerrar) |
| **Estado** | EMITIDA |
