# I-65 F4-T5-PLUGIN-COMMAND — disposiciones del Coordinator para la verificacion de la correccion (intento 5, 2026-10-13)

Redactadas por el Coordinator (AUTOMATION_PLAN 16.6) ANTES de la verificacion, con los hechos de la entrega `R20261013T232041Z-8f39`
(RedSha `a2a8b868`, CurrentSha `f1d9a51d`). No amplian el contrato.

## D1. La cadena y su RED

La cadena F4-T5-PLUGIN-COMMAND tiene `ChainBaseSha` = `283ed613` (primera delegacion, 2026-10-11) y nunca acredito un RED. Entre
`283ed613` y el `BaseSha` de esta entrega (`af04ddb5`) estan, ademas de los commits de sesion y de F4-T6 (ya verificada), el commit RED del
intento 4 (`3e1ef603`, sin verificacion) con la guarda nueva, sus pruebas y las cuatro ediciones exactas de pruebas existentes. La
correccion (CD-I65-F4-39) parte de ese estado: su RED (`a2a8b868`) solo corrige la muestra de `StructuralAnalysisPluginCommandGuardTests.cs`
y su GREEN (`f1d9a51d`) solo anade el archivo del comando y el `Content` del `.csproj`. Lee el RED de la cadena asi: el material RED es
`3e1ef603` + `a2a8b868` (pruebas) y la corrida RED que cuenta es la del `RedSha` `a2a8b868`; `RT` (= `git diff --name-only 283ed613 a2a8b868
-- tests/`) incluye tambien las pruebas de F4-T6, que son de otra cadena ya verificada y no las toca esta entrega. El GREEN no toca `tests/`.

## D2. Alcance de la cadena completa

`Scope` evalua `BaseSha..CurrentSha` (tres archivos). Para cubrir tambien el commit RED del intento 4, la sesion ejecuto el alcance mecanico
de `2151ef55..3e1ef603` contra esta misma delegacion: `inputs/scope-mechanical-attempt4.json`. Usalo como evidencia adicional de que las
ediciones de pruebas existentes son exactamente las cuatro nombradas.

## D3. GREEN desde las copias retenidas

El Worker uso sin cambios las copias del GREEN retenido del intento 4, custodiadas en `af04ddb5`
(`docs/automation/evidence/I-65-pilot/F4-T5-PLUGIN-COMMAND/R20261013T225238Z-960c/hold-*.txt`), tras revisarlas contra los invariantes
(CD-I65-F4-39 lo autoriza). El SHA-256 del archivo del comando en `f1d9a51d` es igual al de la copia retenida. Admisible.

## D4. CI del commit de sesion

La CI de `af04ddb5` (commit de sesion del STOP) termino en `failure` porque ese arbol contiene el RED de `3e1ef603`: es lo esperado y no es
senal de esta entrega.
