# I-65 — Entradas de FOUNDATIONS redactadas (F7)

- **Estado:** REDACTADAS, **no publicadas**. Se publican en `docs/FOUNDATIONS.md` en el commit de cierre de la integración, ya revisadas
  por la conformidad de READY-06 ([INITIATIVE_LIFECYCLE](../INITIATIVE_LIFECYCLE.md) §4.1; [WORKFLOW](../WORKFLOW.md) §11.4).
- **Fuente:** Freeze [Proposal V3.1](I-65-proposal-v3.1.md) §7 («Introduces» y «Extends») y
  [ADR-0050](../adr/0050-interoperabilidad-analisis-estructural.md) (aceptado). Verificadas contra el código del Candidato de hito F5
  `bd1fcf8a` (producto = árbol de `51fc82f1`).
- **Condición de `STABLE`:** el registro exige ADR aceptado o Freeze **integrado**, código existente, pruebas identificables, autoridad y
  mutación coincidentes con el código, sin EXP-01 A abierta y con un punto de extensión declarado. Hoy faltan la integración en `main` y
  la conformidad de READY-06; hasta entonces, el estado es la propuesta para la publicación.
- **Consume:** Rack Identity (entrada de `docs/FOUNDATIONS.md`, ADR-0009; se verifica y no se edita). Sin entrada en el registro, por
  sus ADR: catálogo neutral de secciones (ADR-0020..0023), colocación Cantilever (`PrismaticSectionInstance`, ADR-0022/0024/0025),
  `LengthUnits` (ADR-0005, ADR-0040 D3) y ADR-0017. La verificación de DC-08 en la base de READY-04 corresponde a la conformidad final.
- **Nomenclatura:** «D-nn» sin más es una decisión del Freeze (Proposal V3.1 §2). La firma de ingeniería de las filas de la tabla es la
  decisión del Owner **D-09 de F4-D** (paquete de aprobación de ingeniería), que no es la D-09 del Freeze (transporte).

## Entradas propuestas

```text
Name: Structural Analysis Model
Status: STABLE (propuesto; se publica al integrar I-65 si READY-06 lo confirma)
Authority: `StructuralModelBuilder.BuildStructuralModel(source, scope, loads, profile, catalog)` es el unico productor del `StructuralModel` neutral en el codigo de producto y solo expone el modelo cuando el resultado es `Valid` (`Valid | Unsupported | Invalid | Unavailable`, `StructuralExportOutcome`); el tipo tiene constructor publico y `StructuralModelResult.Create` es publico (los usan pruebas y fixtures), y esa exclusividad no esta protegida por una guarda.
Persistence: ninguna; el modelo es derivado y regenerable desde el sobre del rack, el archivo de cargas, el perfil de materiales y el catalogo; nunca se guarda en el DWG ni es autoridad de producto.
Mutation contract: entradas de solo lectura; cualquier diagnostico bloqueante impide el modelo y el paquete; supuestos (`AnalysisAssumption`) con identificador y cargas solo desde el canal explicito `rackcad-structural-loads/v1` (`StructuralLoadFileReader`); huellas de fuente y de entrada (`StructuralFingerprints.Source`, `StructuralFingerprints.AnalysisInput`) deterministas.
Extension point: un sistema nuevo implementa `IStructuralModelSource` (precedente: `CantileverStructuralModelSource`, entrada `CantileverStructuralExport.Build`); `StructuralReadbackComparison.CompareReadbacks` compara lecturas de vuelta y volcados con su contrato; consumidor previsto fuera de I-65: la capacidad de ID30 (Freeze D-13).
Decision source: [ADR-0050](adr/0050-interoperabilidad-analisis-estructural.md) Decisiones 1, 2, 5, 6 y 8; Freeze integrado de I-65 (Proposal V3.1 D-02, D-05, D-10, D-10b, D-11, D-12, D-13).
Protecting tests: `StructuralAnalysisModelBuilderTests`, `StructuralAnalysisModelTests`, `StructuralAnalysisCantileverGoldenTests`, `StructuralAnalysisCantileverIdealisationTests`, `StructuralAnalysisCantileverBoundaryTests`, `StructuralAnalysisLoadFileReaderTests`, `StructuralAnalysisFingerprintTests`, `StructuralAnalysisCanonicalizerTests`, `StructuralAnalysisOutcomeTests`, `StructuralAnalysisSourceEntryTests`, `StructuralAnalysisComparisonVerdictTests`, `StructuralAnalysisComparisonRowTests`.
Known limitations: V1 cubre una estacion Cantilever de una cara (sin doble cara, `NegativeY`, inclinacion > 10 grados, C/L ni canales pareados); sin peso propio, combinaciones, momentos ni cargas distribuidas; equivalencia de transferencia, no validacion estructural (ADR-0050 D5, Q-O3); sin analisis ni resultados (D-14); la exclusividad de `BuildStructuralModel` como productor no esta guardada (una guarda seria seguimiento).
Last changed by: I-65

Name: Structural Analysis Units Boundary
Status: STABLE (propuesto; se publica al integrar I-65 si READY-06 lo confirma)
Authority: el modelo de analisis guarda longitudes en pulgadas y fuerzas en libras-fuerza como cantidades tipadas (`StructuralLength`, `StructuralArea`, `StructuralForce`, `StructuralMomentOfInertia`); `ForceUnits` es el unico propietario de las unidades de fuerza y de la gravedad estandar; `MassUnits` es el unico propietario de la masa de libra.
Persistence: ninguna propia; la pulgada sigue canonica en producto, DWG, catalogos y persistencia (ADR-0005).
Mutation contract: cada proveedor convierte en un solo punto; una unidad desconocida falla cerrado; `StructuralSectionUnits` consume `MassUnits` con API y valores identicos bit a bit (Extends de ADR-0021 §10).
Extension point: una unidad de fuerza nueva se agrega solo en `ForceUnits` (simbolo, conversion y prueba); las longitudes siguen por `LengthUnits` (ADR-0040 D3).
Decision source: [ADR-0050](adr/0050-interoperabilidad-analisis-estructural.md) Decision 3; Freeze integrado de I-65 (D-03, A65-V2-07).
Protecting tests: `StructuralAnalysisUnitTests`, `StructuralAnalysisSourceGuardTests` (G4 sin literales de conversion; INV-16 propietario unico de lb y gn), `LengthUnitsAuthorityGuardTests`, `StructuralAnalysisLoadFileReaderTests`, `StructuralAnalysisRamElementsTranslationTests`.
Known limitations: el canal de cargas admite N, kN, lbf, kip y kgf; el modelo no representa masas ni momentos.
Last changed by: I-65

Name: Structural Analysis Provider Boundary
Status: STABLE (propuesto; se publica al integrar I-65 si READY-06 lo confirma)
Authority: cada proveedor vive en `RackCad.Application/StructuralAnalysis/Providers/<Proveedor>/`; ningun tipo, identificador ni convencion de proveedor aparece fuera de esa carpeta en el producto, salvo el unico comando del Plugin que lo invoca (`RackRamElementsCommands`, lista de permiso de la guarda); el primer proveedor es RAM Elements (`RamBuildPackageFactory.CreateRamBuildPackage`, `RamDumpReader`, `RamTransferReaders`, `RamSectionMapReader`).
Persistence: el paquete de construccion (manifiesto, script de plantilla fija y archivo de cargas) se publica de forma atomica en una carpeta nueva (`StructuralPackagePublisher`); la tabla `assets/structural-analysis/ram-elements/section-material-map.json` es la unica fuente de nombres y perfiles del proveedor y una fila no verificada bloquea la exportacion; la tabla viaja solo en la salida de compilacion del Plugin (`Content` de `RackCad.Plugin.csproj`) y no en el paquete del Autoloader (CD-I65-F4-10).
Mutation contract: sin exportacion parcial; nada se sobrescribe; el script de RAM no escribe antes de sus puertas de capacidad, modelo vacio y carga util, construye en `LB_IN`, lee de vuelta y escribe una sola lectura de vuelta por carpeta; no analiza ni guarda el modelo.
Extension point: un proveedor nuevo agrega su carpeta con traductor, fabrica de paquete y lectores, su comando en la lista de permiso y su propia guarda de identificadores (la guarda G1 vigente es especifica de RAM); un ensamblado propio solo si trae una dependencia binaria (ADR-0050 D4).
Decision source: [ADR-0050](adr/0050-interoperabilidad-analisis-estructural.md) Decisiones 4, 6 y 7; Freeze integrado de I-65 (D-01, D-06, D-07, D-08, D-09, D-13).
Protecting tests: `StructuralAnalysisSourceGuardTests`, `StructuralAnalysisPluginCommandGuardTests`, `StructuralAnalysisPluginCommandProductTableTests`, `StructuralAnalysisRamElementsGuardTests`, `StructuralAnalysisRamElementsHardeningGuardTests`, `StructuralAnalysisRamElementsPackageGoldenTests`, `StructuralAnalysisRamElementsPackageManifestTests`, `StructuralAnalysisRamElementsPackagePayloadTests`, `StructuralAnalysisRamElementsPackageTemplateTests`, `StructuralAnalysisRamElementsPackageValidationTests`, `StructuralAnalysisRamElementsPackageWriterTests`, `StructuralAnalysisRamElementsDumpReaderTests`, `StructuralAnalysisRamElementsDumpTemplateTests`, `StructuralAnalysisRamElementsMapReaderTests`, `StructuralAnalysisSectionMapVerifiedTests`, `StructuralAnalysisRamElementsAxesTests`.
Known limitations: con la instalacion por Autoloader falta la tabla y el comando responde `Unavailable` (CD-I65-F4-10, diferido); API `ramelements` 0.1.0 (puerta de capacidad); propiedades de seccion y material no legibles por la API (NOT_EVALUATED; dependen de la firma de ingenieria D-09 de F4-D); edicion formal del catalogo de RAM desconocida; el paso dentro de RAM es manual; la relacion volcado-sesion es operativa, no criptografica (A65-F4F-04).
Last changed by: I-65
```

## Verificación contra el código (DC-08, en `bd1fcf8a`)

| Símbolo citado | Archivo |
|---|---|
| `StructuralModelBuilder.BuildStructuralModel`, `IStructuralModelSource` | `src/RackCad.Application/StructuralAnalysis/StructuralModelBuilder.cs` |
| `StructuralModel` | `src/RackCad.Application/StructuralAnalysis/StructuralModel.cs` |
| `StructuralExportOutcome` | `src/RackCad.Application/StructuralAnalysis/StructuralExportOutcome.cs` |
| `StructuralLoadFileReader` (`rackcad-structural-loads/v1`) | `src/RackCad.Application/StructuralAnalysis/StructuralLoadFileReader.cs` |
| `StructuralFingerprints.Source`, `.AnalysisInput` | `src/RackCad.Application/StructuralAnalysis/StructuralFingerprints.cs` |
| `StructuralReadbackComparison.CompareReadbacks` | `src/RackCad.Application/StructuralAnalysis/StructuralReadbackComparison.cs` |
| `CantileverStructuralModelSource`, `CantileverStructuralExport.Build` | `src/RackCad.Application/Systems/Cantilever/` |
| `StructuralLength`, `StructuralArea`, `StructuralForce`, `StructuralMomentOfInertia` | `src/RackCad.Application/StructuralAnalysis/` |
| constructor público de `StructuralModel` y `StructuralModelResult.Create` | `StructuralModel.cs`, `StructuralExportOutcome.cs` |
| `ForceUnits` (gravedad estándar), `MassUnits` (masa de libra) | `src/RackCad.Application/Units/` |
| `StructuralSectionUnits` | `src/RackCad.Application/StructuralSections/StructuralSectionUnits.cs` |
| `StructuralPackagePublisher` | `src/RackCad.Application/StructuralAnalysis/StructuralPackageWriter.cs` |
| `RamBuildPackageFactory`, `RamDumpReader`, `RamTransferReaders`, `RamSectionMapReader` | `src/RackCad.Application/StructuralAnalysis/Providers/RamElements/` |
| `RackRamElementsCommands` (`RACKRAMPAQUETE`) | `src/RackCad.Plugin/RackRamElementsCommands.cs` |
| Tabla del proveedor | `assets/structural-analysis/ram-elements/section-material-map.json` |
