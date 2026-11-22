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

**Estado: EJECUTADA** (2026-11-20, cierre de PB-T3b, F4B-E46, custodia `fe1c67f4`; emitida en F4B-E44 y reemitida en F4B-E45): RED `9fda277e5609e62ff715450eed03220c93df8585`, GREEN `c758278548bcdd66e7c55ca15ca1faeb1a431e53`. Lo escribe la sesión principal; el Worker no escribe en este archivo. La tabla de ejecución la completó la emisión de PB-T3c (F4B-E47), porque el cierre la dejó sin rellenar.

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
| **RED** | `9fda277e5609e62ff715450eed03220c93df8585` (CI `44704503614`: tests en failure por los 13 métodos de `PushBackFullWrite` y los 6 de la equivalencia, que fallan por aserción; ui-tests y build-ui en success; build-plugin omitido; los seis métodos de D-1 en `Skipped`) |
| **GREEN** | `c758278548bcdd66e7c55ca15ca1faeb1a431e53` (CI `44707482956` 4/4; Core 19 124: 19 118 Passed, 0 Failed y 6 Skipped, los de D-1, con la Theory S-2 como una fila; UI 2 451: 2 430 Passed y 21 Skipped) |
| **La medición sobre el GREEN** | Conforme con la variante (A): S-1, S-3, S-4 y S-5 fallan; S-2 falla en las filas 1 y 3 y pasa en las 2, 4 y 5; S-6 pasa en vacío. Los `Skip` se restauraron antes del commit (TRX `tests/m1/m1.trx` de la entrega del Worker `R20261120T075251Z-a9e0`, por SHA-256 en su manifiesto) |
| **Estado** | EJECUTADA |

## PB-T3c — La premisa «el escritor nunca borra» del Plugin (D-2), sin `Skip`

**Estado: EJECUTADA** (2026-11-20, cierre de PB-T3c, F4B-E48; emitida en F4B-E47): RED `e0940a0efa314ea26777c59ab4e55c09d019bff6`, GREEN `938a35f0f1636b4306d844eb9e34000e8967c05e`. Lo escribe la sesión principal; el Worker no escribe en este archivo.

**Qué es:** un registro declarado de un cambio autorizado, **sin `Skip`**. No se sustituye ni se debilita ninguna guarda: la premisa del escritor del piloto de F4, «nunca borra una vista», pasa a «borra solo las vistas del plan, por la fachada caller-owned». D-2 no es una decisión (§8.2 del paquete): este registro es lo que pide [OD] para todo cambio autorizado.

**Autoridad:**
- **[OD], D-PB-04** (`docs/automation/evidence/I-64-pilot/F4B-DRAFTS/coordinator/owner-f4b-disposition.md`): borrar sí; y el bloque sobre T1B, «create a declared superseding characterization/change record. Do not silently weaken the old guard».
- **[P], OB-04.1:** «una primitiva caller-owned y su guarda de fachada».
- **El contrato:** `docs/initiatives/I-64-f4b-pbt3-gate-contract-package.md` (v4.7), D-PBT3-11, §5.4, §6.3 y §8.2, y `docs/automation/evidence/I-64-pilot/F4B-PBT3C-FULL-WRITE-ERASE/coordinator/gate-contract.json`.

**La guarda que sigue en verde, y por qué:**
- **`WorkspacePilotUpdateGuardTests`, `WriterProhibitionViolations`** (`tests/RackCad.Tests/Workspace/WorkspacePilotUpdateGuardTests.cs:182-212`). Su comentario de la línea 188 dice «Never: the lock, an erasure, …», y la regla prohíbe a las tres clases del escritor `\bErase\s*\(`, `EraseViewBlocks`, `GetBlockReferenceIds`, `ForWrite` y `UpgradeOpen`.
- **Sigue siendo cierta y no cambia:** el escritor no borra por sí mismo. Llama a `RackViewBlockEraser.EraseInTransaction`, en un archivo propio (`src/RackCad.Plugin/Views/RackViewBlockEraser.cs`), que es el que borra. La llamada no casa con ninguna de esas expresiones (`.EraseInTransaction(` no es `\bErase\s*\(`).
- Siguen igual también la estructura del escritor, con un `StartTransaction`, un `Commit` y una purga, y G-1, G-2, G-7 y G-9 con sus cuentas (R-6 y R-7 del registro de mediciones).

**Las guardas nuevas** (§5.4 del paquete; un `[Fact]` cada una). CT-PBT3C-01 a 04 fijan el borrado, y son las que pide D-2; CT-PBT3C-05 extiende D2 a WR y a la fachada:

| CT | Clase | Qué fija |
|---|---|---|
| CT-PBT3C-01 | `PushBackViewEraseGuardTests` | **La fachada:** su firma exacta y sus operaciones en orden, sin bloqueo, transacción propia, confirmación, purga, regeneración, `SyncName` ni la cadena `PushBack`, y sin nombrar el piloto |
| CT-PBT3C-02 | `PushBackViewEraseGuardTests` | **`EraseUnit` en el escritor:** `RackPilotMutationScope` implementa `IPushBackPilotViewEraser`, y el cuerpo de `EraseUnit` sigue el orden de D-PBT3-11 |
| CT-PBT3C-03 | `PushBackViewEraseGuardTests` | **PREPARE y la relectura:** `PrepareUnits` resuelve cada clave de `plan.Erasures`, sin ningún `PrepareRedraw` para ellas, y `Reread` calcula `matches` sobre `_prepared` y `_erasures` |
| CT-PBT3C-04 | `PushBackViewEraseGuardTests` | **El censo de la primitiva:** en todo `src/`, una sola llamada a la fachada, dentro de `EraseUnit` de WR |
| CT-PBT3C-05 | `PushBackWritePathPluginGuardTests` | **G-L1 ampliada a WR y a la fachada:** las fichas de D2 no aparecen, y las reglas de CT-PBT1B-14 dan 0 coincidencias |

**Los dos comentarios de WR que cambian** (D-PBT3-11; V2-11 y V2-28). Son comentarios, y ninguna guarda los lee (`Src.Code` los quita):
- **El de cabecera, `RackPilotUpdateWriter.cs:26`** («It never erases a view.»): pasa a describir el borrado caller-owned.
- **El del ámbito de MUTATE, `:225`** («It never purges, renames, regenerates or erases»): pasa a «It never purges, renames or regenerates; it erases only the planned views, through the caller-owned facade».

**Si la comprobación acotada de PB-T3c lo leyera como un debilitamiento,** entraría en D-1 con sus métodos (§8.2). CA-T3c no lo lee así: su resultado va en F4B-E47.

### Ejecución de PB-T3c (se completa al cerrar)

| | |
|---|---|
| **RED** | `e0940a0efa314ea26777c59ab4e55c09d019bff6` (CI `44754080713`: tests en failure por CT-PBT3C-01 a 04, que fallan por aserción, y CT-PBT3C-05 pasa; ui-tests y build-ui en success; build-plugin omitido. Core 19 129 = 19 119 Passed + 4 Failed + 6 Skipped, los de D-1; UI 2 451 = 2 430 + 21). La fachada del RED es, byte a byte, la del parche `pbt3c-skeleton.patch` |
| **GREEN** | `938a35f0f1636b4306d844eb9e34000e8967c05e` (CI `44757675821` 4/4; Core 19 129 = 19 123 Passed + 0 Failed + 6 Skipped, los de D-1; UI 2 451 = 2 430 + 21). Toca solo WR y la fachada; `WriterProhibitionViolations` (UpdateGuard:182-212) y G-9 siguen en verde |
| **Los comentarios de WR, en el GREEN** | Cambiados como dice D-PBT3-11: el de cabecera (`RackPilotUpdateWriter.cs:26-27`) dice que el escritor borra una vista solo por la fachada caller-owned, dentro del único ámbito de MUTATE y solo las que pide el plan; el del ámbito de MUTATE (`:238-240`) lleva el texto literal del paquete («It never purges, renames or regenerates; it erases only the planned views, through the caller-owned facade») más una frase sobre POST. Ninguna guarda los lee |
| **Estado** | EJECUTADA |

## PB-T4a — CT-PBT3A-12, el censo de los tipos del modelo completo, sustituida por CT-PBT4A-06

**Estado: EJECUTADA** (2026-11-20, cierre de PB-T4a, F4B-E51; emitida en F4B-E50): RED `214f8c75f2ca5a248a5592075c3bac8e667bd362`, GREEN `faef702055990d1350a73bf4d254eb9e898d5670`. Lo escribe la sesión principal; el Worker no escribe en este archivo.

**Autoridad:**
- **[OD], el bloque sobre T1B:** «create a declared superseding characterization/change record. Do not silently weaken the old guard».
- **Las decisiones del Owner sobre PB-T4** (F4B-E49, `docs/automation/evidence/I-64-pilot/F4B-DRAFTS/coordinator/owner-pbt4-decisions.md`), pregunta 4: la lista de PB-T4a es una sola, CT-PBT3A-12.
- **El contrato:** `docs/initiatives/I-64-f4b-pbt4-gate-contract-package.md` (v4.3), §4.1, §5.1 y §6.1, y `docs/automation/evidence/I-64-pilot/F4B-PBT4-FULL-READ-PATH/coordinator/gate-contract.json`.

**El motivo:** RIA, el adaptador de la lectura del Plugin, pasa a nombrar `PushBackFullDraftReader` para leer el modelo completo sobre el mismo escaneo (D-PBT4-02). CT-PBT3A-12 fija por su letra que fuera de `Draft/`, PP y UR nadie nombra los cinco tipos del modelo, y fallaría. No se edita: queda con `Skip` y su sustituta, CT-PBT4A-06, que hace el mismo censo admitiendo RIA y nada más.

**El `Skip`, literal** (§6.1 de la v4.3; `PushBackDraftWriteGuardTests.cs:60`, la única línea que cambia en el archivo):

```text
[Fact(Skip = "I-64 F4B PB-T4a: sustituida por CT-PBT4A-06 de PushBackFullReadGuardTests (registro declarado superseding-characterization.md; decision del Owner owner-pbt4-decisions.md, pregunta 4).")]
```

| La prueba vieja (con `Skip`) | La sustituta | Qué fija la sustituta |
|---|---|---|
| `PushBackDraftWriteGuardTests.CT_PBT3A_12_NoSourceFileOutsideTheFolderOfTheModelNamesItsTypes` (`[Fact]`, `tests/RackCad.Tests/PushBackDraftWriteGuardTests.cs:60-61`) | CT-PBT4A-06 (`PushBackFullReadGuardTests`) | Solo `Draft/`, PP, UR y RIA nombran los cinco tipos (`PushBackFullDraftState`, `PushBackFullDraftReader`, `PushBackFullComposition`, `PushBackConflictClassifier` y `PushBackViewDispositions`), con las líneas de CT-PBT3A-12 iguales |

### Ejecución de PB-T4a (se completa al cerrar)

| | |
|---|---|
| **RED** | `214f8c75f2ca5a248a5592075c3bac8e667bd362` (CI `44810371492`: tests en failure por las cinco CT que fallan por aserción; ui-tests y build-ui en success; build-plugin omitido. Core 19 137 = 19 125 Passed + 5 Failed + 7 Skipped: las seis de D-1 y CT-PBT3A-12, ya con su `Skip`; UI 2 451 = 2 430 + 21). El esqueleto del RED es, byte a byte, el del parche `pbt4a-skeleton.patch` |
| **GREEN** | `faef702055990d1350a73bf4d254eb9e898d5670` (CI `44814418683` 4/4; Core 19 137 = 19 130 Passed + 0 Failed + 7 Skipped; UI 2 451 = 2 430 + 21). Toca solo las seis rutas de `src/` del alcance; ninguna prueba de `tests/` cambia tras el RED |
| **CT-PBT4A-06, en el GREEN** | `PushBackFullReadGuardTests.CT_PBT4A_06_OnlyTheFolderOfTheModelThePlanTheRunAndTheAdapterOfThePluginNameItsTypes` (`tests/RackCad.Tests/PushBackFullReadGuardTests.cs:98`): pasa en el RED y en el GREEN, con el mismo censo que CT-PBT3A-12 admitiendo RIA. CT-PBT3A-12 sale en Skipped con su motivo literal en las dos CI |
| **Estado** | EJECUTADA |

## PB-T4b — las 478 pruebas que el modelo completo del panel sustituye, cada una por su copia o su equivalente

**Estado: EJECUTADA** (2026-11-21, cierre de PB-T4b, F4B-E53; emitida en F4B-E52): RED `c1e4f4585217abc8c85d90b499a07bb93c84be2c`, reparación del RED `9b2d18937d3aa03b4cbeaf0546cd0e6b44a4f6fe`, GREEN `c065080d3fd1dd8bb1937b98b81e9a0c1714777b`. Lo escribe la sesión principal; el Worker no escribe en este archivo.

**Autoridad:**
- **[OD], el bloque sobre T1B:** «create a declared superseding characterization/change record. Do not silently weaken the old guard».
- **Las tandas del Owner sobre PB-T4,** en `docs/automation/evidence/I-64-pilot/F4B-DRAFTS/coordinator/`:
  - la primera, pregunta 3, «Saltar y duplicar» (`owner-pbt4-decisions.md`): cada prueba que el modelo completo rompe se salta con registro y se duplica sobre el modelo completo;
  - la segunda, «Sí, esa lista» (`owner-pbt4b-retire-list-decisions.md`): la lista de 470;
  - la tercera (`owner-pbt4b-breaks-decision.md`), la quinta (`owner-pbt4b-list-477-decision.md`) y la sexta (`owner-pbt4b-list-478-decision.md`): las 8 que la llevan a 478.
- **El contrato:** `docs/initiatives/I-64-f4b-pbt4-gate-contract-package.md` (v4.9), D-PBT4-11, §4.2 y §6.2, y `docs/automation/evidence/I-64-pilot/F4B-PBT4-FULL-DRAFT-PANEL/coordinator/gate-contract.json`.

**La lista:** `F4B-DRAFTS/pbt4/pbt4b-substitution-list-478.tsv`, sha256 `8b0fcd0ca775ce3a5e46ef2059f4b06e7ec7319883daf3cb37b1fe91290e9e8b`. Ninguna prueba original se edita: cada una queda con un `Skip` de motivo literal, y su sustituta va en una clase `<Clase>Full` nueva, en el espacio de nombres raíz de su ensamblado (D-S6).
- **C-0** (428, en 9 clases): 356 copias mecánicas, que el guion de la copia genera desde el original, y 72 equivalentes escritos a mano (D-S1), con su texto literal en el paquete. Cuatro `Theory` se parten por filas en las dos clases de la partición, sin cambiar ninguna fila (D-S5).
- **C-5** (23): las guardas que fijan por su letra lo que PB-T4b cambia, cada una con su equivalente. Entre ellas, las dos de PB-T4a (CT-PBT4A-03 y CT-PBT4A-05) y la de la sexta tanda (WHHG `NoneOfThe30…`). CT-PBT4A-06 la sustituye CT-PBT4B-13.
- **UI** (27): copias.

**Las 28 copias sin cambio:** las 27 de la UI y RG:109-123 se saltan y se sustituyen por una copia idéntica, porque con D-S2 su texto sigue valiendo. Se quedan en la lista, declaradas aquí como copias sin cambio.

**Los motivos de los `Skip`, literales** (`<Clase>` es la clase original sin `Tests`):

| N.º | Causa | El motivo |
|---|---|---|
| 1 | C-0, copia mecánica | `I-64 F4B PB-T4b: sustituida por su copia en <Clase>Full (registro declarado superseding-characterization.md; decision del Owner owner-pbt4-decisions.md, pregunta 3).` |
| 2 | C-0, a mano | `I-64 F4B PB-T4b: sustituida por su equivalente escrito a mano en <Clase>Full (registro declarado superseding-characterization.md; decision del Owner owner-pbt4-decisions.md, pregunta 3).` |
| 3 | C-5 de la v4.3 | `I-64 F4B PB-T4b: sustituida por su equivalente en <Clase>Full (registro declarado superseding-characterization.md; decision del Owner owner-pbt4b-retire-list-decisions.md, pregunta 2).` |
| 4 | CT-PBT4A-06 | `I-64 F4B PB-T4b: sustituida por CT-PBT4B-13 de PushBackFullPanelCensusGuardTests (registro declarado superseding-characterization.md; decision del Owner owner-pbt4b-retire-list-decisions.md, pregunta 2).` |
| 5 | C-5, las cinco de la tercera tanda | `I-64 F4B PB-T4b: sustituida por su equivalente en <Clase>Full (registro declarado superseding-characterization.md; decision del Owner owner-pbt4b-breaks-decision.md).` |
| 6 | C-5, las dos de PB-T4a (CT-PBT4A-03 y 05) | `I-64 F4B PB-T4b: sustituida por su equivalente en <Clase>Full (registro declarado superseding-characterization.md; decision del Owner owner-pbt4b-list-477-decision.md).` |
| 7 | C-5, la de la sexta tanda (WHHG) | `I-64 F4B PB-T4b: sustituida por su equivalente en <Clase>Full (registro declarado superseding-characterization.md; decision del Owner owner-pbt4b-list-478-decision.md).` |
| 8 | UI | `I-64 F4B PB-T4b: sustituida por su copia en <Clase>Full (registro declarado superseding-characterization.md; decision del Owner owner-pbt4b-retire-list-decisions.md, pregunta 2).` |

**Por clase y por causa:**

| La clase original | Cuántas | La sustituta | El motivo (n.º) |
|---|---|---|---|
| `tests/RackCad.Tests/PushBackFullReadGuardTests.cs` | 1 | CT-PBT4B-13 (`PushBackFullPanelCensusGuardTests`) | 4 |
| `tests/RackCad.Tests/PushBackFullReadGuardTests.cs` | 1 | `PushBackFullReadGuardFull` | 6 |
| `tests/RackCad.Tests/PushBackFullReadPathTests.cs` | 1 | `PushBackFullReadPathFull` | 6 |
| `tests/RackCad.Tests/PushBackWritePathGuardTests.cs` | 1 | `PushBackWritePathGuardFull` | 3 |
| `tests/RackCad.Tests/Workspace/WorkspaceAssociativeBookkeepingTests.cs` | 10 | `WorkspaceAssociativeBookkeepingFull` | 1 |
| `tests/RackCad.Tests/Workspace/WorkspaceCommandAdvisorTests.cs` | 54 | `WorkspaceCommandAdvisorFull` | 1 |
| `tests/RackCad.Tests/Workspace/WorkspaceEditIntentGuardTests.cs` | 5 | `WorkspaceEditIntentGuardFull` | 3 |
| `tests/RackCad.Tests/Workspace/WorkspaceEditIntentGuardTests.cs` | 2 | `WorkspaceEditIntentGuardFull` | 5 |
| `tests/RackCad.Tests/Workspace/WorkspaceEditIntentHostTests.cs` | 57 | `WorkspaceEditIntentHostFull` | 1 |
| `tests/RackCad.Tests/Workspace/WorkspaceEditIntentHostTests.cs` | 5 | `WorkspaceEditIntentHostFull` | 2 |
| `tests/RackCad.Tests/Workspace/WorkspaceHostHardeningGuardTests.cs` | 1 | `WorkspaceHostHardeningGuardFull` | 7 |
| `tests/RackCad.Tests/Workspace/WorkspaceLifecycleGuardTests.cs` | 2 | `WorkspaceLifecycleGuardFull` | 5 |
| `tests/RackCad.Tests/Workspace/WorkspaceLifecycleHostTests.cs` | 68 | `WorkspaceLifecycleHostFull` | 1 |
| `tests/RackCad.Tests/Workspace/WorkspaceLifecycleHostTests.cs` | 1 | `WorkspaceLifecycleHostFull` | 2 |
| `tests/RackCad.Tests/Workspace/WorkspacePilotEditGuardTests.cs` | 5 | `WorkspacePilotEditGuardFull` | 3 |
| `tests/RackCad.Tests/Workspace/WorkspacePilotEditHostTests.cs` | 53 | `WorkspacePilotEditHostFull` | 1 |
| `tests/RackCad.Tests/Workspace/WorkspacePilotEditHostTests.cs` | 6 | `WorkspacePilotEditHostFull` | 2 |
| `tests/RackCad.Tests/Workspace/WorkspacePilotPrepareTests.cs` | 46 | `WorkspacePilotPrepareFull` | 1 |
| `tests/RackCad.Tests/Workspace/WorkspacePilotPrepareTests.cs` | 21 | `WorkspacePilotPrepareFull` | 2 |
| `tests/RackCad.Tests/Workspace/WorkspacePilotReconciliationGuardTests.cs` | 1 | `WorkspacePilotReconciliationGuardFull` | 3 |
| `tests/RackCad.Tests/Workspace/WorkspacePilotReconciliationHostTests.cs` | 17 | `WorkspacePilotReconciliationHostFull` | 1 |
| `tests/RackCad.Tests/Workspace/WorkspacePilotReconciliationHostTests.cs` | 11 | `WorkspacePilotReconciliationHostFull` | 2 |
| `tests/RackCad.Tests/Workspace/WorkspacePilotUpdateGuardTests.cs` | 1 | `WorkspacePilotUpdateGuardFull` | 5 |
| `tests/RackCad.Tests/Workspace/WorkspacePilotUpdateHostTests.cs` | 12 | `WorkspacePilotUpdateHostFull` | 1 |
| `tests/RackCad.Tests/Workspace/WorkspacePilotUpdateHostTests.cs` | 23 | `WorkspacePilotUpdateHostFull` | 2 |
| `tests/RackCad.Tests/Workspace/WorkspacePilotUpdateIntegrationGuardTests.cs` | 2 | `WorkspacePilotUpdateIntegrationGuardFull` | 3 |
| `tests/RackCad.Tests/Workspace/WorkspacePilotUpdateRequestTests.cs` | 39 | `WorkspacePilotUpdateRequestFull` | 1 |
| `tests/RackCad.Tests/Workspace/WorkspacePilotUpdateRequestTests.cs` | 5 | `WorkspacePilotUpdateRequestFull` | 2 |
| `tests/RackCad.UI.Tests/Workspace/WorkspacePilotReconciliationTests.cs` | 21 | `WorkspacePilotReconciliationFull` | 8 |
| `tests/RackCad.UI.Tests/Workspace/WorkspacePilotReconciliationViewTests.cs` | 6 | `WorkspacePilotReconciliationViewFull` | 8 |
| **Total** | **478** | | |

### Ejecución de PB-T4b (se completa al cerrar)

| | |
|---|---|
| **RED** | `c1e4f4585217abc8c85d90b499a07bb93c84be2c` (CI `44893322578`: tests y ui-tests en failure por las clases con `ExpectRed`; build-ui en success; build-plugin omitido. Core 19 605 = 18 841 Passed + 306 Failed + 458 Skipped; UI 2 481 = 2 430 + 3 + 48). Los 478 `Skip` van en los 21 archivos de la lista, solo en la línea del atributo y con el motivo literal de su causa; las 21 copias y el kit son, byte a byte, los de la precondición 6 (`CopySha`). Reparación del RED `9b2d18937d3aa03b4cbeaf0546cd0e6b44a4f6fe` (RR, F4B-E35): solo `tests/RackCad.Tests/PushBackFullPanelTests.cs`, CT-PBT4B-08; CI `44899232904` con los mismos 306 + 3 fallos; `rr_check` PASS |
| **GREEN** | `c065080d3fd1dd8bb1937b98b81e9a0c1714777b` (CI `44903818806` 4/4; Core 19 605 = 19 147 Passed + 0 Failed + 458 Skipped; UI 2 481 = 2 433 + 0 + 48). Toca solo nueve archivos de código de `src/`, iguales byte a byte al GREEN literal del paquete; ninguna prueba de `tests/` cambia tras la reparación del RED, y ningún archivo de `Pilot/` |
| **Los recuentos por clase, en el GREEN** | Las 478 de la lista, en Skipped con su motivo: 451 en Core y 27 en UI, las mismas por clase que la tabla de arriba. Los Skipped de las CI son esas más los de la base: 7 en Core (D-1 y CT-PBT3A-12) y 21 en UI. Las copias pasan enteras: en Core, 614 filas en 29 clases `FullPanelCopy<Clase>RedTests` y `FullPanelCopy<Clase>GreenTests` de 19 archivos; en UI, 36 en 2 clases. Las 21 CT pasan: `PushBackFullPanelTests` 11, `PushBackFullPanelGuardTests` 4, `PushBackFullPanelCensusGuardTests` 1, `PushBackPanelDraftGuardTests` 1, `WorkspaceFullCopyGuardTests` 1 (su comprobación (5), acotada a las nueve clases de C-0 por la errata que ratificó el Owner) y `PushBackFullPanelFieldTests` 3 |
| **Estado** | EJECUTADA |

## PB-T4c — las 4 pruebas que «Editar todo» sustituye, cada una por su equivalente

**Estado: EJECUTADA** (2026-11-21, cierre de PB-T4c, F4B-E55; emitida en F4B-E54): RED `280c79beac61d31b03f86488c03686eef4f694bc`, GREEN `7cc1076eae8159294a1498ab556851977223b64c`, sin reparación del RED. Lo escribe la sesión principal; el Worker no escribe en este archivo.

**Autoridad:**
- **[OD], el bloque sobre T1B:** «create a declared superseding characterization/change record. Do not silently weaken the old guard».
- **Las decisiones del Owner sobre PB-T4,** en `docs/automation/evidence/I-64-pilot/F4B-DRAFTS/coordinator/`:
  - la primera (`owner-pbt4-decisions.md`): la pregunta 3, «Saltar y duplicar», por la que cada prueba que se rompe se salta con registro y se duplica, y la pregunta 4, la emisión condicional con la lista que vio el Owner, que para PB-T4c eran 2 (PEG:155-163 y CT-PBT1B-15);
  - la octava (`owner-pbt4c-list-3-decision.md`): CT-PBT4B-20, y la lista pasa a 3;
  - la novena (`owner-pbt4c-list-4-decision.md`): CT-PBT2B-06, y la lista pasa a 4.
- **El contrato:** `docs/initiatives/I-64-f4b-pbt4-gate-contract-package.md` (v5.1), D-PBT4-12 a D-PBT4-17, §4.3, §5.3 y §6.3, y `docs/automation/evidence/I-64-pilot/F4B-PBT4-EDIT-ALL-WINDOW/coordinator/gate-contract.json`.

**La lista:** `F4B-DRAFTS/pbt4/pbt4c-substitution-list-4.tsv`, sha256 `9bd4c17a0eda837c92a7a0b7b3562d4ab17f1608baf3ffa8efaf560d8b1f82de`. Ninguna prueba original se edita: cada una queda con un `Skip` de motivo literal en su línea de atributo `[Fact]`, y su equivalente va en una clase nueva, que genera `make_equivalents_pbt4c_v50.py` desde el original de `6adff90d`, con un solo cambio declarado.

| N.º | La prueba, con su `Skip` | Proyecto | La causa | La decisión del Owner | El equivalente | `EquivalentSha` | El cambio declarado |
|---|---|---|---|---|---|---|---|
| 1 | `WorkspacePilotEditGuardTests.ThePortDeclaresTheSeventeenOperationsWithTheEnumsAsOrderedPrefixesAndNamesNoTypeOfTheDraftsNorOfThePilot` (`tests/RackCad.Tests/Workspace/WorkspacePilotEditGuardTests.cs:155`) | Core | C-5: PEG:155-163, las 17 operaciones del puerto pasan a 18 (TryEditAll) | `owner-pbt4-decisions.md`, pregunta 4 | `PushBackEditAllPortEquivalenceGuardTests` (`tests/RackCad.Tests/PushBackEditAllPortEquivalenceGuardTests.cs`) | `1a2607d3a35ae8f9737fffb0eb32a4acd8100318fe83e5b0098d30428f57d250` | PEG:155-163, con la fila de `TryEditAll` al final de `Operations` (18) |
| 2 | `PushBackSharedReferenceGuardTests.CT_PBT1B_15_TheWritersOfNameInThePluginRunAfterTheWindowCloses` (`tests/RackCad.Tests/PushBackSharedReferenceGuardTests.cs:306`) | Core | C-5: CT-PBT1B-15, archivos=14 pasa a archivos=15 con RackWorkspaceEditorWindow.cs | `owner-pbt4-decisions.md`, pregunta 4 | `PushBackEditAllNameWritersGuardTests` (`tests/RackCad.Tests/PushBackEditAllNameWritersGuardTests.cs`) | `3744992127be8b7e1d8d822a769b8eb2a6886789c73285c8436e5baec6bf7b6b` | CT-PBT1B-15, con «archivos=15» |
| 3 | `WorkspaceFullCopyGuardTests.CT_PBT4B_20_TheScriptOfSkipAndDuplicateGivesTheCopiesTheKitAndTheSkipsOfTheClosedList` (`tests/RackCad.Tests/WorkspaceFullCopyGuardTests.cs:568`) | Core | C-5: CT-PBT4B-20 (1), el Skip de PB-T4c en PEG:155 deja PEG distinto de su BaseSha | `owner-pbt4c-list-3-decision.md` | `PushBackEditAllFullCopyGuardTests` (`tests/RackCad.Tests/PushBackEditAllFullCopyGuardTests.cs`) | `4619b6d310340f9c97a7d9f31bda1db852d2fcda905b91bb857510f7e6b7e6ce` | CT-PBT4B-20, con la clase renombrada y `Restore`, que además devuelve la línea exacta del `Skip` de PB-T4c de PEG:155 |
| 4 | `PushBackWorkspaceApplyCloseTests.CT_PBT2B_06_ClosingAsksOnlyForWhatWasNotApplied` (`tests/RackCad.UI.Tests/PushBackWorkspaceApplyCloseTests.cs:686`) | UI | C-3: CT-PBT2B-06, la ventana 4 (un modulo escenificado y cambios sin aplicar) pide la pregunta del modulo; D-PBT4-17 (A-T2b-OWNER-01) la cambia por una que nombra las dos | `owner-pbt4c-list-4-decision.md` | `PushBackEditAllApplyCloseTests` (`tests/RackCad.UI.Tests/PushBackEditAllApplyCloseTests.cs`) | `7c2458260ef301d7f05bfaff8e1e8c4a4a618f25fd7022705d55dee32dbe78d2` | CT-PBT2B-06, con la línea de la ventana 4 a la pregunta combinada; cae en el RED y pasa en el GREEN (`ExpectRed` true) |

**Los motivos de los `Skip`, literales** (`pbt4c_skips_v50.py`; ASCII):

- `tests/RackCad.Tests/Workspace/WorkspacePilotEditGuardTests.cs:155` (`ThePortDeclaresTheSeventeenOperationsWithTheEnumsAsOrderedPrefixesAndNamesNoTypeOfTheDraftsNorOfThePilot`):

  ```text
  [Fact(Skip = "I-64 F4B PB-T4c: sustituida por su equivalente en PushBackEditAllPortEquivalenceGuardTests (registro declarado superseding-characterization.md; decision del Owner owner-pbt4-decisions.md, pregunta 4).")]
  ```

- `tests/RackCad.Tests/PushBackSharedReferenceGuardTests.cs:306` (`CT_PBT1B_15_TheWritersOfNameInThePluginRunAfterTheWindowCloses`):

  ```text
  [Fact(Skip = "I-64 F4B PB-T4c: sustituida por su equivalente en PushBackEditAllNameWritersGuardTests (registro declarado superseding-characterization.md; decision del Owner owner-pbt4-decisions.md, pregunta 4).")]
  ```

- `tests/RackCad.Tests/WorkspaceFullCopyGuardTests.cs:568` (`CT_PBT4B_20_TheScriptOfSkipAndDuplicateGivesTheCopiesTheKitAndTheSkipsOfTheClosedList`):

  ```text
  [Fact(Skip = "I-64 F4B PB-T4c: sustituida por su equivalente en PushBackEditAllFullCopyGuardTests (registro declarado superseding-characterization.md; decision del Owner owner-pbt4c-list-3-decision.md).")]
  ```

- `tests/RackCad.UI.Tests/PushBackWorkspaceApplyCloseTests.cs:686` (`CT_PBT2B_06_ClosingAsksOnlyForWhatWasNotApplied`):

  ```text
  [Fact(Skip = "I-64 F4B PB-T4c: sustituida por su equivalente en PushBackEditAllApplyCloseTests (registro declarado superseding-characterization.md; decision del Owner owner-pbt4c-list-4-decision.md).")]
  ```

### Ejecución de PB-T4c (se completa al cerrar)

| | |
|---|---|
| **RED** | `280c79beac61d31b03f86488c03686eef4f694bc` (CI `44956801926`: tests y ui-tests en failure por las clases con `ExpectRed`; build-ui en success; build-plugin omitido. Core 19 619 = 19 149 Passed + 9 Failed + 461 Skipped; UI 2 486 = 2 432 + 5 + 49). Los 4 `Skip` van en los 4 archivos de la lista, solo en la línea del atributo y con su motivo literal (`check_skips_pbt4c_v50.py`: PASA); los 4 equivalentes son, byte a byte, su `EquivalentSha`; el esqueleto, 8 archivos de `src/`, es byte a byte el del registro de la precondición 6. Sin reparación del RED |
| **GREEN** | `7cc1076eae8159294a1498ab556851977223b64c` (CI `44959653142` 4/4; Core 19 619 = 19 158 Passed + 0 Failed + 461 Skipped; UI 2 486 = 2 437 + 0 + 49). Toca solo 10 archivos de `src/`, y los 13 del GREEN literal del paquete (`pbt4c-green-v51.txt`) son byte a byte su texto; ninguna prueba de `tests/` cambia tras el RED, y ningún archivo de `Pilot/` |
| **Los equivalentes, en el RED y en el GREEN** | Los 4, nuevos y byte a byte su `EquivalentSha`, sin cambio después del RED. En el RED pasan los tres de Core, con `ExpectRed` false (`PushBackEditAllPortEquivalenceGuardTests`, `PushBackEditAllNameWritersGuardTests` y `PushBackEditAllFullCopyGuardTests`, 1 Passed cada uno), y cae por aserción el de UI, con `ExpectRed` true (`PushBackEditAllApplyCloseTests`, 1 Failed); en el GREEN pasan los cuatro. Las 4 de la lista quedan en Skipped con su motivo en las dos ramas, 3 en Core y 1 en UI; los Skipped de las CI son esas más los de la línea base (458 en Core y 48 en UI). Las CT: en el RED caen por aserción `PushBackEditAllTests` 6, `PushBackEditAllGuardTests` 3 y `PushBackEditAllWindowTests` 4, y pasan `PushBackOpenerClassicPathGuardTests` 1 y `PushBackDraftLaunchGuardTests` 1; en el GREEN pasan las 15 |
| **Estado** | EJECUTADA |

## PB-T4d — la retirada del piloto: las 271 filas con acción de su lista

**Estado: EMITIDA** (2026-11-22, emisión de PB-T4d, F4B-E56, sobre el cierre de PB-T4c `6c2820ba`). Lo escribe la sesión principal; el Worker no escribe en este archivo. La ejecución se completa al cerrar.

**Autoridad:**
- **[OD], el bloque sobre T1B:** «create a declared superseding characterization/change record. Do not silently weaken the old guard».
- **Las decisiones del Owner sobre PB-T4d,** en `docs/automation/evidence/I-64-pilot/F4B-DRAFTS/coordinator/`: `owner-pbt4d-list-decisions.md` (el alcance B y la emisión condicional), `owner-pbt4d-substitutes-decision.md` («Sustitutas donde hace falta»), `owner-pbt4d-run-coverage-decision.md` («Sustituir las 21»), `owner-pbt4d-deadcode-27-decisions.md` («Quitarla en PB-T4d» y «Sí, 27») y `owner-pbt4d-cat4d-decisions.md` («Sustituirlas» y «Sustituirlos siempre»).
- **Las decisiones de la sesión:** D-S11 a D-S16 (`F4B-DRAFTS/pbt4/session-pbt4d-design-decisions.md`).
- **El contrato:** `docs/initiatives/I-64-f4b-pbt4-gate-contract-package.md` (v5.4), D-PBT4-10, §4.4, §5.6 y §6.4, y `docs/automation/evidence/I-64-pilot/F4B-PBT4-RETIRE-PILOT/coordinator/gate-contract.json`.

**La lista:** `F4B-DRAFTS/pbt4/pbt4d-retire-list-271.tsv`, sha256 `05a4d3d1905529c5bc4d53e0684d27d97eaa96502098eebeb9d16bfd931c75ac`: 284 filas, 271 con acción y 13 que se quedan sin tocar. Cada fila lleva su grupo, su archivo, su miembro, sus líneas en `6c2820ba`, su `Skip`, su acción y su porqué. Todo lo de `tests/` va en el RED, con el texto literal de la pieza `pbt4d-red-v54.txt` (D-S11).

**Por grupo y acción:**

| Grupo | Acción | Filas |
|---|---|---|
| C-7 | borrar con registro | 75 |
| C-7 | reescribir con registro | 27 |
| C-7 | se queda | 7 |
| C-7 | sustituir con registro | 29 |
| M-03 | borrar con registro | 35 |
| M-04 | reescribir con registro | 3 |
| SUS-01 | borrar con registro | 1 |
| apoyo | borrar con registro | 21 |
| apoyo | reescribir con registro | 2 |
| apoyo | se queda | 6 |
| ayudante-editado | reescribir con registro | 6 |
| compilacion | borrar con registro | 11 |
| compilacion | sustituir con registro | 1 |
| directiva | reescribir con registro | 1 |
| ejecucion | sustituir con registro | 3 |
| lenta | acotar con registro | 1 |
| original | borrar con registro | 55 |
| **Total** | | **284** (271 con acción) |

**Los motivos de los `Skip`, literales** (`pbt4d_test_edits_v54.py`, `SKIPS`; ASCII), cada uno en la línea de atributo `[Fact]` de su prueba en `6c2820ba`:

- `tests/RackCad.Tests/PushBackEditAllFullCopyGuardTests.cs:572` (`CT_PBT4B_20_TheScriptOfSkipAndDuplicateGivesTheCopiesTheKitAndTheSkipsOfTheClosedList`):

  ```text
  [Fact(Skip = "I-64 F4B PB-T4d: sustituida por su equivalente en PushBackRetirePilotFullCopyGuardTests (registro declarado superseding-characterization.md; decision del Owner owner-pbt4d-list-decisions.md, pregunta 1).")]
  ```

- `tests/RackCad.Tests/Workspace/WorkspacePilotPrepareGuardTests.cs:81` (`TheFileOfThePlanIsNullableEnabledInTheNamespaceOfThePilotWithBracesAndDeclaresTheTypesOfThePackage`):

  ```text
  [Fact(Skip = "I-64 F4B PB-T4d: sustituida por su equivalente en PushBackRetirePilotPlanGuardTests (registro declarado superseding-characterization.md; decision del Owner owner-pbt4d-list-decisions.md, pregunta 1).")]
  ```

- `tests/RackCad.Tests/Workspace/WorkspacePilotPrepareGuardTests.cs:194` (`TheOrderOfPrepareIsTheOneOfThePackageAndNoOutcomeBeforeConflictLooksAtTheReread`):

  ```text
  [Fact(Skip = "I-64 F4B PB-T4d: sustituida por su equivalente en PushBackRetirePilotPlanGuardTests (registro declarado superseding-characterization.md; decision del Owner owner-pbt4d-list-decisions.md, pregunta 1).")]
  ```

**Los archivos nuevos, literales** (en la pieza del RED; `NewFileSha` del texto con `\n`):

| Archivo | `NewFileSha` | Qué es |
|---|---|---|
| `tests/RackCad.Tests/PushBackRetirePilotGuardTests.cs` | `0d64806a53b36ea0719bc7647dcfd3c15e8169a66a4494517097a82ce12fadb3` | CT-PBT4D-01; **`ExpectRed` true** |
| `tests/RackCad.Tests/PushBackRetirePilotCensusGuardTests.cs` | `1874c47811ddda428745a1f5566f1e7290b31fa2a2e65b07c2ff52059f863f20` | CT-PBT4D-02; `ExpectRed` false |
| `tests/RackCad.Tests/PushBackRetirePilotRunTests.cs` | `3554df0d960f18ef6bb2b930f4ac7757e8ca7438ebf6a9eda8934ef9acf8fe69` | las 29 sustitutas de WPUR sobre el modelo completo, con `FullUpdateSetup` (`v53/make_run_eq_v53.py`): 27 de la corrida genérica sobre `ExecuteFull` y la n.º 48 y la n.º 49 de `PushBackPilotSystemRebuild` (CA-T4d I-01); 26 por el mapa de PB-T4b; a mano la n.º 10, la n.º 43 y la n.º 49 (solo su comentario); `ExpectRed` false |
| `tests/RackCad.Tests/PushBackRetirePilotFullCopyGuardTests.cs` | `ee9b6f1513f549065da391c10d6da947cc8e197644efbc7d72cf41e46443e6cf` | el equivalente de CT-PBT4B-20 de PB-T4c, copia literal con los `BaseSha`, los `Skip`, las `CopySha` y los recuentos que PB-T4d cambia (`make_fullcopy_eq_v52.py`, sobre los textos finales del RED de la v5.3); `ExpectRed` false |
| `tests/RackCad.Tests/PushBackRetirePilotPlanGuardTests.cs` | `48c2f4a9619e2f3e7e5de4f1ad98bf68024635c268866fa1242f65f329d8ca43` | las dos guardas de `WorkspacePilotPrepareGuardTests` (D-S12), cada una con su cambio declarado; `ExpectRed` false |

**Las ediciones con registro** («reescribir» y «acotar con registro»), fila a fila:

| N.º | Grupo | Archivo y líneas | Miembro | Acción |
|---|---|---|---|---|
| 1 | C-7 | `tests/RackCad.Tests/PushBackFullWritePrepareTests.cs:474-544` | `CT_PBT3B_05_TheNameOfTheDraftReachesEveryViewAndABlankNameTakesTheOneOfTheOrigin` | reescribir con registro |
| 2 | C-7 | `tests/RackCad.Tests/PushBackFullWritePrepareTests.cs:558-629` | `CT_PBT3B_06_TheBarrierOfTheDiagnosticsStopsABlockedDesignOfTheFullModelAndOfThePilot` | reescribir con registro |
| 4 | C-7 | `tests/RackCad.Tests/Workspace/WorkspaceDraftPilotIntentStaleBaseTests.cs:551-568` | `ARereadThatIsNotAdmittedBecauseAMemberBecameBlockingTakesTheUnknownDraftToConflictWithTheDifferences` | reescribir con registro |
| 5 | C-7 | `tests/RackCad.Tests/Workspace/WorkspaceDraftPilotIntentStaleBaseTests.cs:570-586` | `ARereadThatIsNotAdmittedBecauseAuth13FindsDivergenceTakesTheUnknownDraftToConflictWithTheDifferences` | reescribir con registro |
| 6 | C-7 | `tests/RackCad.Tests/Workspace/WorkspaceDraftPilotIntentStaleBaseTests.cs:588-602` | `ARereadThatIsEqualRecoversTheUnknownDraft` | reescribir con registro |
| 7 | C-7 | `tests/RackCad.Tests/Workspace/WorkspaceDraftPilotIntentStaleBaseTests.cs:604-615` | `ARereadThatIsNotAdmittedOnlyForTheCatalogStillRecoversBecauseTheBaseIsTheSame` | reescribir con registro |
| 46 | C-7 | `tests/RackCad.Tests/Workspace/WorkspacePilotEditGuardTests.cs:401-416` | `TheHostGuardTripsOnAReadAnAuthorizationOrAMarkWhereTheOperationMustNotHaveThem` | reescribir con registro |
| 47 | C-7 | `tests/RackCad.Tests/Workspace/WorkspacePilotEditGuardTests.cs:418-431` | `TheHostGuardTripsWhenAnOperationThatChangesADraftLeavesOutNotePilotEdit` | reescribir con registro |
| 48 | C-7 | `tests/RackCad.Tests/Workspace/WorkspacePilotEditGuardTests.cs:434-451` | `TheUpdateRequestGuardTripsWithoutTheGateWithASecondRequestWithTheUpdateItselfOrWhereTheProgressAuthorizesOrMutates` | reescribir con registro |
| 49 | C-7 | `tests/RackCad.Tests/Workspace/WorkspacePilotEditGuardTests.cs:453-468` | `TheBoundGuardTripsWhenTheLazyExpiryReachesTheOwnMutationTheProbeTheGateTheReaderOrAnotherMemberOfTheBridge` | reescribir con registro |
| 50 | C-7 | `tests/RackCad.Tests/Workspace/WorkspacePilotEditGuardTests.cs:470-492` | `TheFlightGuardTripsWhenTheCheckOfTheFlightIsMissingOrComesBeforeTheAuthorizationAndTheMarkOfTheReads` | reescribir con registro |
| 51 | C-7 | `tests/RackCad.Tests/Workspace/WorkspacePilotEditGuardTests.cs:516-526` | `TheFlightGuardTripsOnABodyThatChecksTheFlightWithNoAuthorizationAtAll` | reescribir con registro |
| 52 | C-7 | `tests/RackCad.Tests/Workspace/WorkspacePilotPrepareGuardTests.cs:172-176` | `ComposeForWriteIsCalledOnlyByThePlannerAndOnlyAfterTheComparisonOfTheBase` | reescribir con registro |
| 53 | C-7 | `tests/RackCad.Tests/Workspace/WorkspacePilotPrepareGuardTests.cs:178-190` | `TheAfterMatchGuardTripsOnAnotherCallerAndOnACallBeforeTheComparison` | reescribir con registro |
| 75 | C-7 | `tests/RackCad.Tests/PushBackFullWriteRunTests.cs:245-304` | `CT_PBT3B_09_ComparePostFullChecksTheErasedKeysTheKeysTheEnvelopesAndTheScaleAndThePilotKeepsItsComparison` | reescribir con registro |
| 76 | C-7 | `tests/RackCad.Tests/Workspace/WorkspaceDraftPilotMaterializationTests.cs:317-332` | `MaterializeReproducesTheApplicationPartOfTheWindowLoadInTheSameOrder` | reescribir con registro |
| 77 | C-7 | `tests/RackCad.Tests/Workspace/WorkspaceDraftPilotOwnershipTests.cs:283-286` | `EditingTheCurrentStateInPlaceLeavesTheDraftDirtyAndTheCloseVetoed` | reescribir con registro |
| 78 | C-7 | `tests/RackCad.Tests/Workspace/WorkspaceDraftPilotOwnershipTests.cs:258-264` | `MutatingTheObjectOfThePostReadAfterTheRebaseOfAnAppliedUpdateChangesNothing` | reescribir con registro |
| 79 | C-7 | `tests/RackCad.Tests/Workspace/WorkspaceDraftPilotOwnershipTests.cs:243-246` | `MutatingTheObjectOfTheReadAfterBeginChangesNeitherTheInitialStateNorDirty` | reescribir con registro |
| 80 | C-7 | `tests/RackCad.Tests/Workspace/WorkspaceDraftPilotOwnershipTests.cs:248-251` | `MutatingTheObjectOfTheReadAfterDiscardAndReloadChangesNothing` | reescribir con registro |
| 81 | C-7 | `tests/RackCad.Tests/Workspace/WorkspaceDraftPilotOwnershipTests.cs:253-256` | `MutatingTheObjectOfTheReadAfterKeepMineChangesNothing` | reescribir con registro |
| 82 | C-7 | `tests/RackCad.Tests/Workspace/WorkspaceDraftPilotOwnershipTests.cs:266-269` | `MutatingTheObjectPassedToSetCurrentDraftStateAfterTheCallDoesNotChangeTheCurrentState` | reescribir con registro |
| 83 | C-7 | `tests/RackCad.Tests/Workspace/WorkspaceDraftPilotOwnershipTests.cs:271-274` | `MutatingTheObjectReturnedByInitialDraftStateDoesNotChangeTheInternalInitialState` | reescribir con registro |
| 84 | C-7 | `tests/RackCad.Tests/Workspace/WorkspaceDraftPilotOwnershipTests.cs:276-279` | `ReusingOneObjectOfOriginForTwoDraftsOrTwoSessionsLeavesThemIndependent` | reescribir con registro |
| 136 | C-7 | `tests/RackCad.Tests/PushBackPilotUpdateEquivalenceGuardTests.cs:136-145` | `E_3_TheMemberPlansAreBuiltOnlyFromTheMutableMembersOfTheRereadInOrdinalOrderAndErasuresOnlyComeFromAnObsoleteLateralOfAModelThatErases` | reescribir con registro |
| 137 | C-7 | `tests/RackCad.Tests/PushBackPilotUpdateEquivalenceGuardTests.cs:220-228` | `E_5_TheEscapeOfEachRunIsClassifiedByTheStateThatTheRunAlreadyHadAndNeverAsAPlainFailureAfterAWriteOrASeal` | reescribir con registro |
| 138 | C-7 | `tests/RackCad.Tests/PushBackPilotUpdateEquivalenceGuardTests.cs:230-249` | `E_6_TheEscapeGuardTripsOnACatchThatDoesNotConsultTheStateAndOnAnEscapeThatAnswersAPlainFailureFirst` | reescribir con registro |
| 204 | apoyo | `tests/RackCad.Tests/Workspace/WorkspaceDraftPilotIntentStaleBaseTests.cs:538-549` | `UnknownAfterACommitThatThrew` | reescribir con registro |
| 209 | apoyo | `tests/RackCad.Tests/Workspace/WorkspaceDraftPilotOwnershipTests.cs:76-80` | `Both` | reescribir con registro |
| 239 | M-04 | `tests/RackCad.Tests/Workspace/WorkspacePilotEditGuardTests.cs:591-598` | `TheReadGuardAllowsAnotherReadThatALaterTaskWouldAddWithItsOwnMark` | reescribir con registro |
| 240 | M-04 | `tests/RackCad.Tests/Workspace/WorkspacePilotEditGuardTests.cs:916-920` | `TheOperationsOfThePortAndTheDecoratorNameNoApiOfWritingTheDrawing` | reescribir con registro |
| 241 | M-04 | `tests/RackCad.Tests/Workspace/WorkspacePilotEditGuardFull.cs:255-258` | `WriteViolations` | reescribir con registro |
| 242 | lenta | `tests/RackCad.Tests/Workspace/WorkspacePilotPrepareFull.cs:1911-1944` | `InvalidD_ABuilderThatCannotBuildTheOutgoingSystemIsInvalidWithTheTypedErrorAndNothingIsWritten` | acotar con registro |
| 278 | ayudante-editado | `tests/RackCad.Tests/PushBackPilotUpdateEquivalenceGuardTests.cs:105-134` | `E3Violations` | reescribir con registro |
| 279 | ayudante-editado | `tests/RackCad.Tests/PushBackPilotUpdateEquivalenceGuardTests.cs:186-218` | `E5Violations` | reescribir con registro |
| 280 | ayudante-editado | `tests/RackCad.Tests/Workspace/WorkspaceDraftPilotOwnershipTests.cs:74-74` | `Kinds` | reescribir con registro |
| 281 | ayudante-editado | `tests/RackCad.Tests/Workspace/WorkspacePilotEditGuardTests.cs:265-377` | `HostViolations` | reescribir con registro |
| 282 | ayudante-editado | `tests/RackCad.Tests/Workspace/WorkspacePilotEditGuardTests.cs:874-914` | `WriteViolations` | reescribir con registro |
| 283 | ayudante-editado | `tests/RackCad.Tests/Workspace/WorkspacePilotPrepareGuardTests.cs:156-170` | `AfterMatchViolations` | reescribir con registro |
| 284 | directiva | `tests/RackCad.Tests/Workspace/WorkspaceDraftPilotIntentStaleBaseTests.cs:-` | `using RackCad.Application.Systems.PushBack.Draft;` | reescribir con registro |

**Las erratas que declara la emisión** (la recomprobación 2 de CA-T4d, sobre el diff de la v5.3 a la v5.4: EMITIBLE, B 0 / I 0 / M 3; la sesión las corrige en este commit, sin una v5.5):
- **M-14:** «Lo que cambia» de la cabecera de la v5.4 nombra §7, que no cambia, y omite D-PBT4-10, §5.6 y la apertura de §6.4; la columna «Dónde» de V54-04 omite §8.5. Todo lo que cambia cabe en alguna fila V54.
- **M-15:** la nota de D-S16 de `session-pbt4d-design-decisions.md` (cuyo sha256 cita la v5.4, y no se toca) dice `cov_p6_v53.py`, y lo aplica `cov_p6_v54.py`; la definición de PERDIDA de `cov_p6_v54.py:248-249` y `:327` no se ajustó a la ESTABLE nueva (el caso medido da 0); y su «Uso» (`:17`) dice v53. Nada de esto cambia la medida.
- **M-16:** `RedFiles` de la precondición 6 es el RED de la copia `r6y`, el de la v5.3: en `WorkspacePilotUpdateRunTests.cs`, el RED de la v5.4 da `7ea6170422f48334f18ff2cd3b1cb1b32c79264861dd52a6e6b56570daf7ba13`, que solo difiere en comentarios (lo comprueba el generador; decisión de la sesión). `pbt4d-red-v53.txt`, `pbt4d-green-v53.txt` y `pbt4d-retire-list-270.tsv` se custodian como historia.

### Ejecución de PB-T4d (se completa al cerrar)

| | |
|---|---|
| **RED** | (al cerrar) |
| **GREEN** | (al cerrar) |
| **Las sustitutas y los equivalentes, en el RED y en el GREEN** | (al cerrar) |
| **Estado** | EMITIDA |
