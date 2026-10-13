# I-65 F4-T5-PLUGIN-COMMAND — analysis.md: REWORK de la verificación del intento 5

Redactado por el Coordinator según AUTOMATION_PLAN §16.8 y §16.11 (2026-10-13). La `FailureClass` (`Tests`) difiere de la del fallo anterior de
la cadena (`StopCondition`, intento 4): análisis obligatorio antes de la corrección.

## Hechos

- Entrega `R20261013T232041Z-8f39` (intento 5): RED `a2a8b868` (corrección de la muestra), GREEN `f1d9a51d` (comando `RACKRAMPAQUETE` y `Content`
  del `.csproj`); CI del GREEN `38484644494` 4/4 `success` (Core 14 368/14 368; filtro 98/98); CI del RED `38484201964` Core `failure`.
- Verificación `R20261013T233523Z-b1dd` (Codex 36): **`EXECUTION_REWORK_REQUIRED`/`REWORK`**, `FailureClass` `Tests`; las demás 13 comprobaciones
  en `pass`; `Ci.RedPart` y `Tests.RedPart` en `pass`: **el RED queda acreditado** (`ChainRedSha` = `a2a8b868`; `ChainRedFiles` = `RT` de
  `283ed613..a2a8b868 -- tests/`, 21 archivos, registrado por el Controller).
- Motivo del Controller: el invariante «Fallo cerrado y visible» exige que, con la tabla del producto (filas `proposed`), **el comando** responda
  `Unsupported` **sin escribir nada**. `StructuralAnalysisPluginCommandProductTableTests` observa el flujo neutral (`Unsupported`, paquete nulo) pero
  no el comando; la guarda solo exige la forma de la llamada de publicación (C07) y un mínimo de comparaciones con `Valid` (C09). Falta una prueba
  que observe que cada salida no `Valid` del comando informa en la línea de órdenes y vuelve **antes** de la única publicación.

## Causa raíz

La suite Core no compila el Plugin y el contrato prohíbe tocar Application en T5: no existe un seam de pruebas permitido que ejecute el comando. La
observación a nivel del comando solo puede ser **estructural** (la guarda sobre el texto del archivo), y la guarda entregada no la hacía completa.
El código del comando cumple el comportamiento (revisión del Coordinator de `f1d9a51d`: cada rama no `Valid` llama a `Show` con el resultado y los
diagnósticos y hace `return` antes de `new StructuralPackagePublisher().Write(...)`); el defecto es de **observación por prueba**.

## Decisión del Coordinator (CD-I65-F4-41)

Corrección (`attempts` 5 → **6**, `AttemptsRemaining` 0: la última posible; cualquier REWORK posterior es S-11) con el **mismo contrato**,
`CorrectionOf` = `R20261013T233523Z-b1dd` (`Tests`), `ChainRedSha` = `a2a8b868`, `ChainRedFiles` = los 21 archivos registrados:

1. **Reglas nuevas de la guarda** (`StructuralAnalysisPluginCommandGuard.cs`), con autopruebas por variante sintética en
   `StructuralAnalysisPluginCommandGuardTests.cs`:
   - exactamente una llamada de publicación, y aparece después de las tres comprobaciones `!= Valid` de la tabla, del modelo y del paquete;
   - cada bloque `if (<x>.Outcome != StructuralExportOutcome.Valid ...)` informa en la línea de órdenes con `<x>.Outcome` y `<x>.Diagnostics` (a
     través del ayudante visible que termina en `editor.WriteMessage`) y termina con `return;` antes de cerrar el bloque;
   - ninguna otra API de escritura de archivos o carpetas en el comando (`File.Write*`, `File.Create*`, `File.Copy`, `File.Move`, `Directory.Create*`,
     `FileStream`, `StreamWriter`): solo el publicador escribe;
   - variantes que la guarda debe detectar: publicación antes de la comprobación del paquete; bloque no `Valid` sin `return`; bloque no `Valid` sin
     mensaje visible; una escritura de archivo añadida.
2. **RED por corrección desactivada** (§16.8, precedente CD-I65-F4-18): el commit RED contiene las reglas y pruebas nuevas y devuelve
   `src/RackCad.Plugin/RackRamElementsCommands.cs` y `src/RackCad.Plugin/RackCad.Plugin.csproj` al estado de `af04ddb5` (sin el comando ni el
   `Content`), de modo que las pruebas que leen el archivo real fallan por aserción.
3. **GREEN**: restaura los dos archivos byte a byte desde `f1d9a51d`; si una regla nueva revela un defecto real del comando, se corrige en ese archivo
   (dentro del alcance) y se declara. El GREEN no toca `tests/`.
