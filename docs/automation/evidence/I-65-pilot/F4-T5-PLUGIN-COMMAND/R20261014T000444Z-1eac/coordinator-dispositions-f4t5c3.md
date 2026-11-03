# I-65 F4-T5-PLUGIN-COMMAND — disposiciones del Coordinator para la verificacion de la correccion final (intento 6, 2026-10-14)

Redactadas por el Coordinator (AUTOMATION_PLAN 16.6) ANTES de la verificacion, con los hechos de la entrega `R20261013T234619Z-b5ef`
(RedSha `e413b9a2`, CurrentSha `e610ea49`). No amplian el contrato.

## D1. La cadena y su RED

`ChainBaseSha` = `283ed613`; `ChainRedSha` = `a2a8b868` (acreditado por la verificacion `R20261013T233523Z-b1dd`); `ChainRedFiles` = las 21
rutas de la delegacion. Esta correccion toca `StructuralAnalysisPluginCommandGuard.cs` y `StructuralAnalysisPluginCommandGuardTests.cs` (en
`ChainRedFiles`): exige RED, por correccion desactivada (§16.8; precedente CD-I65-F4-18): el arbol RED no tiene el archivo del comando ni el
`Content` del `.csproj`, y el GREEN los restaura byte a byte desde `f1d9a51d` (`git diff f1d9a51d e610ea49 -- src` vacio).

## D2. Dos commits RED (desviacion de procedimiento declarada por el Worker)

El prompt pedia un solo commit RED. El Worker hizo `28fe112e` (desactivacion: borra el comando y devuelve el `.csproj`; sin pruebas, porque un
`git add` fallo sobre la ruta ya borrada) y, sin `amend` ni `force` (prohibidos), `e413b9a2` con las reglas C15-C18 y sus autopruebas. El RED de
esta entrega es `e413b9a2` (pruebas nuevas + implementacion desactivada); las corridas push de ambos terminaron con Core en `failure`. El diff neto
`BaseSha..CurrentSha` son solo los dos archivos de la guarda. Disposicion del Coordinator: **desviacion de procedimiento sin efecto** en lo que se
verifica (el `RedSha` declarado es el arbol RED completo); no es motivo de REWORK por si sola. Evaluala en `Identity` (RedSha entre BaseSha y
CurrentSha) y en `Tests` (corrida del RedSha) y citala en Findings.

## D3. Observacion estructural (respuesta al REWORK del intento 5)

No existe un seam de pruebas permitido para ejecutar el comando (el Plugin no se compila en Core; Application prohibido en T5; CD-I65-F4-41). El
invariante «fallo cerrado y visible» a nivel del comando se observa con las reglas nuevas de la guarda sobre el texto del comando (una sola
publicacion, despues de las tres comprobaciones `!= Valid`; cada rama no `Valid` informa con su resultado y sus diagnosticos y vuelve antes de
publicar; el ayudante visible usa `editor.WriteMessage`; ninguna otra escritura de archivos), con autopruebas por variante que la guarda detecta,
junto con `StructuralAnalysisPluginCommandProductTableTests` (con la tabla del producto, `Unsupported` y sin paquete). Evalua `Tests` con esta
lectura: es la observacion posible dentro del contrato.
