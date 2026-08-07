# I-39C — Caracterización base frente al contrato nuevo

> Evidencia de I-39C ([contrato](../../initiatives/I-39C-adopcion-editores-acotados.md),
> [ADR-0029](../../adr/0029-contrato-funcional-comun-de-ventanas-wpf.md)). Registro factual; no incluye
> conteos de pruebas ni hashes, que viven en `docs/HANDOFF.md` §12.

## 1. Por qué existe este documento

**La caracterización previa es inmutable.** Cuando una migración cambia a propósito un comportamiento
caracterizado, la prueba original **no se reescribe**: se conserva intacta con `Skip` como evidencia
versionada del estado anterior, y el comportamiento nuevo se prueba en una clase separada. Así la
transición se lee entera en el historial —*comportamiento anterior caracterizado → cambio deliberado
autorizado por ADR-0029 → comportamiento nuevo probado*— en vez de aparecer como una prueba que
siempre hubiera esperado lo nuevo.

El commit **`7cc6260`** es la versión en que toda la caracterización de I-39C corría en verde contra el
árbol **sin tocar**. Las pruebas de I-39A que aquí se marcan corrían en verde desde su integración.

## 2. Las diez que cambiaron

`BoundedEditorCharacterizationTests` (escrita por I-39C, conservada con `Skip`):

| Prueba | La base decía | El contrato dice | Autoriza | Lo prueba |
|---|---|---|---|---|
| `LosCuatroXAMLNombranUnShellConNombreDeSISTEMA` | los cuatro XAML nombran `components:CantileverComponentEditorShell` | nombran `shell:RackBoundedEditorShell` y la fachada no existe | D12 | `LosCuatroXAMLNombranElShellNEUTRAL`, `LaFachadaDeI39AYaNoExiste` |
| `LasCuatroCantileverAplicanElContratoDeTamanoDelArquetipoRICO` | aplican `EditorShellWindowStyle` y heredan `1120×672` | aplican `BoundedEditorWindowStyle` y sus propios mínimos | D9 | `LasCuatroCantileverAplicanElContratoDeTamanoDeSuPropioARQUETIPO` |
| `CadaCantileverDeclaraUnTamanoQueSuMinimoHeredadoNoRESPETA` | el ancho declarado pierde siempre contra el mínimo | lo declarado es lo que abre | D9 | `ElTamanoQueDeclaranEsElQueABREN` |
| `NingunaDeLasCincoLeeLosTokensDelArquetipoB` | los tokens `BoundedEditor*` no los lee nadie | las seis salen de ellos | D9 | `LasCuatroCantileverAplican…`, `ElInspectorLeeLosMismosTokensEnVezDeRepetirlos`, `ElLargueroComponeSobreElShell…` |
| `ElLargueroDeclaraSuTamanoAManoYNoAplicaNingunEstiloDeVentana` | `720×440`, mínimo `640×380`, sin estilo | tamaño del arquetipo, con estilo | D9 | `ElLargueroComponeSobreElShellYAplicaElContratoDelArquetipo` |
| `ElLargueroNoUsaNingunShellYArmaSuChromeAMano` | sin shell, con cuatro colores literales | sobre el shell, con tokens | D11 | `ElChromeDelLargueroSaleYaDeLosTokensCompartidos` |
| `SeparadorYTensorDejanInsertarHabilitadoSinLineaResuelta` | `Insertar` encendido sin línea resuelta | apagado, con el motivo visible | D6 | `InsertarSeApagaConMotivoCuandoLaPiezaNoPuedeMaterializARSE` |
| `NingunaDeLasCuatroCantileverDeclaraFocoInicial` | ninguna declara foco inicial | las cuatro lo declaran en su primer control de captura | D9 | `LasSeisDeclaranSuFocoInicialYNingunoEsUnaAccion` |

`StructuralSectionInspectorWindowTests` (escrita por **I-39A**, conservada con `Skip`; las demás de esa
suite siguen corriendo **sin tocarse**):

| Prueba | La base decía | El contrato dice | Autoriza | Lo prueba |
|---|---|---|---|---|
| `InsertarIsNeverDisabledAndWithoutSelectionItIsASilentNoOp` | `Insertar` nunca se deshabilita y sin selección es un no-op silencioso | se apaga con el motivo visible | D6 | `ElInspectorApagaInsertarSinSeleccionYLoDICE` |
| `TheWindowDeclaresNoInitialFocus` | no declara foco inicial | lo declara en la caja de búsqueda | D9 | `LasSeisDeclaranSuFocoInicialYNingunoEsUnaAccion` |

**Las dos de I-39A se cambian con la autorización que su propio texto anticipaba.** La primera lleva
escrito «queda como deuda de I-39C»; la segunda, «darle un orden explícito es contrato de ADR-0029 D9 y
trabajo de otra subiniciativa». I-39C **es** la subiniciativa del arquetipo.

## 3. Lo que se revisó y NO se cambió

- **Que una longitud o una rotación inválidas no bloqueen la inserción en el inspector.** I-39A lo midió
  junto al no-op silencioso, pero **es correcto**: ADR-0029 D5 exige que una entrada inválida **no**
  sobrescriba en silencio un valor aplicado válido, y eso es exactamente lo que la ventana hace —el campo
  se pinta en ámbar y el estado conserva el último valor válido—. Lo que se inserta es un valor aplicado,
  no basura, así que bloquear habría sido el error. Queda **probado**, no supuesto, en
  `UnaLongitudInvalidaNoBloqueaPorqueElValorAPLICADOSigueSiendoValido`.
- **El dirty y el cierre de las seis.** Ninguna declara ámbito transaccional pendiente y ninguna
  intercepta el cierre. Es `NotApplicable` con razón de producto, no por omisión (§4 de
  [decisiones técnicas](I-39C-decisiones-tecnicas.md)).
- **La frescura del preview.** Las seis lo rehacen en el mismo paso en que recalculan; ninguna conserva un
  último-válido obsoleto. D4 dice expresamente que una ventana no está obligada a implementar estados que
  hoy no exhibe.

## 4. Trazabilidad

- Base en verde sobre el árbol sin tocar: commit `7cc6260`.
- Contrato nuevo: `tests/RackCad.UI.Tests/BoundedEditorContractTests.cs`.
- Base conservada: `tests/RackCad.UI.Tests/BoundedEditorCharacterizationTests.cs` y
  `tests/RackCad.UI.Tests/StructuralSectionInspectorWindowTests.cs`.
