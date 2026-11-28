# F2-T2B-NAVIGATION-UI: análisis de la corrección del intento 3

- **Verificación de origen:** `R20261110T053114Z-ffd2`, `EXECUTION_VERIFIED` sobre `6c01f644` (entrega `R20261110T045058Z-7d3d`; 14 comprobaciones en pass; CI 42761455131 4/4). Se conserva tal cual: acredita proceso, alcance, CI y pruebas.
- **Origen del hallazgo:** revisión del código hecha por la sesión principal sobre los objetos de Git antes de lanzar nc1..nc3, y una revisión independiente de un subagente de solo lectura al que no se dio esa conclusión. Las dos trazaron T2B-F1 por separado. T2B-F2 y T2B-F3 los trazó el subagente y los comprobó la sesión contra `NavigationActions.cs` y `SelectionContext.cs`.
- **`FailureClass` de la corrección:** `StopCondition`. El Controller no falló ninguna comprobación; la parada es de la sesión (revisión de producto antes de los controles) y su resolución cambia el trabajo, que es el caso de AUTOMATION_PLAN 16.8 que incrementa `attempts`.

## Por qué no lo detectó la verificación

Las 14 comprobaciones contrastan la entrega con Git, el contrato, la CI y los TRX. Ninguna ejecuta secuencias de uso. Las pruebas de la copia ejercen Ver y Centrar con el cursor, y Localizar solo una vez seguida de un cambio de vista, así que ningún caso repite Localizar sobre la misma vista.

## Hallazgos

### T2B-F1 (alto; INV-F2T2B-04, SN-03): Localizar vista nunca pasa de la primera copia

`EnterScope` asigna `_scopeView = view` y después llama a `ResetCopy`, que deja `_scopeView = null`. Tras cualquier cambio de ámbito el campo queda en null.

Secuencia con 3 referencias de la vista Frontal:

1. Localizar: null distinto de Frontal, se reinicia. El puerto recibe k=0 y responde M=3.
2. Siguiente copia: k=1, el panel muestra «Copia 2 de 3».
3. Localizar: null otra vez distinto de Frontal, se reinicia. El puerto recibe k=0 y el panel vuelve a la copia 1.

La segunda copia y las siguientes son inalcanzables desde Localizar, y la vuelta a la primera no puede ocurrir.

En sentido contrario, tras Localizar con k=2, un Ver compara null con null, no reinicia y envía k=2 contra el conjunto completo, con la M de la vista anterior a la vista hasta que el puerto responde. INV-F2T2B-04 exige el reinicio al cambiar la vista pedida.

Ninguna prueba lo cubre. `LocateAfterViewStartsItsOwnCopyBecauseItsCountBelongsToTheRequestedView` pasa precisamente por ese reinicio.

### T2B-F2 (medio; INV-F2T2B-06): una copia fuera de rango se presenta como rack sin objetivos

`NavigationActionPolicy.Plan` no elige ninguna referencia cuando k no cabe en el conjunto, y `Decide` responde `RejectedNoTargets`. El puerto devuelve `Unavailable` con ese veredicto y `TargetCount` positivo. El panel dice «El rack no tiene objetivos para esa acción» junto a «Copia 1 de 2».

Ocurre cuando el conjunto encoge sin que cambie el rack del contexto, por ejemplo con el panel oculto mientras se borra una copia. `ACopyBeyondTheTargetsThatShrankGoesBackToTheFirstOne` responde `Executed(2)` para k=2, una respuesta que el ejecutor real no puede dar, y no comprueba el texto.

### T2B-F3 (medio; INV-F2T2B-02): el motivo de bloqueo es falso para un único objeto que no es rack

`SelectionContext.Classify` devuelve `Selection(0, 1)` al seleccionar una línea. El panel dice «Hay varios objetos seleccionados», mientras el texto de contexto de encima dice 0 racks y 1 objeto. `Present` solo recibe el tipo y no distingue los casos, así que el texto debe valer para todos.

## Considerado y no corregido

- `Failed` muestra el detalle del host, sin traducir, tras un prefijo en español. `EveryStatusHasItsOwnSpanishText` lo exige a propósito y el detalle sirve para diagnosticar un fallo contenido. INV-F2T2B-06 se cumple con el texto propio del estado. Queda como limitación conocida.

## Corrección

Dos rutas exactas: `src/RackCad.UI/Workspace/WorkspaceNavigationViewModel.cs` y `tests/RackCad.UI.Tests/Workspace/WorkspaceNavigationViewModelTests.cs`.

1. `EnterScope` reinicia primero y asigna el ámbito después.
2. `TextOf`, en `Unavailable` con veredicto y `TargetCount` mayor que 0, da un texto propio: la copia pedida ya no existe.
3. `ReasonOfBlock`, en `Selection`, da un texto neutro válido para varios objetos y para uno que no es rack.

Pruebas nuevas en un commit RED nuevo (el archivo pertenece a `ChainRedFiles`), que fallan por aserción contra el producto actual. El GREEN solo toca el view-model. Ningún otro archivo cambia: ni guardas, ni puerto, ni XAML, ni cableado.

## Presupuesto

`attempts` pasa de 2 a 3 antes de emitir la delegación (`max_attempts` = 3). Tras esta corrección no queda otra: un nuevo REWORK de la unidad sería S-11.
