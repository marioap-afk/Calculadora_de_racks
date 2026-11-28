# Análisis de la verificación `R20261110T154803Z-fc18` (F3-T2-BROWSER-UI, intento 1)

La verificación dio `EXECUTION_VERIFIED`, 14/14, sobre `2c36b36b`, y se conserva. Acredita proceso, alcance, CI y pruebas. La revisión del código de la sesión y una revisión independiente hallaron seis defectos de comportamiento que ninguna prueba de la entrega ejerce. La corrección es delegada (F3-E09) y consume el intento 2 de 3. `FailureClass` para `CorrectionOf`: `StopCondition`.

## T2-F1. La pestaña activa no se distingue

`WorkspacePanelView.xaml` fija `BorderThickness` y `BorderBrush` como atributos del `Border` de la plantilla. En WPF esos valores tienen más precedencia que un `Style.Trigger`, así que el `DataTrigger` de `IsActive` nunca se aplica. Todas las pestañas se ven iguales. Corrección: mover ambos valores a setters del estilo.

## T2-F2. Se bloquea y se lee el dibujo dentro de notificaciones de AutoCAD

`ActiveSessionChanged` se dispara dentro del manejador de `DocumentActivated`, y `CommandStateChanged` dentro de `EnteringQuiescentState`. Cada uno llega por `StateChanged` a `Refresh`, `Browser.Present` y, si la superficie cambia o estaba pendiente, a `ReadSummary`, que hace `LockDocument`, abre una transacción y recorre la tabla de bloques. Secuencia: se trabaja en A, un comando invalida la caché, se cambia a B y se vuelve a A. Incumple P-03 y P-04: los manejadores de notificación solo encolan y no leen el DWG. Corrección: una presentación solo lee durante el aviso del drenaje; el clic del usuario no depende de esa ventana. Las guardas de F1-C2 fijan el cuerpo de `OnCommandStateChanged`, así que el cambio va en `OnDrained`.

## T2-F3. Un rack borrado sigue constando como presente

`ReadBrowserFacts` usa `includeReferences: false`, y `ScanEnvelopes` devuelve las definiciones hasta un PURGE. Tras un ERASE del rack, la pestaña muestra «Resumen cargado» con sus vistas, mientras Navegación responde que no hay objetivos. Corrección: una definición sin referencias directas en Model Space no cuenta, sin sacar handles del adaptador.

## T2-F4. Los estados de fallo son terminales

`SyncSurface` solo reintenta `Pending` y `Busy`. Con una sola pestaña en `NotFound`, `Rejected` o `Failed`, o con un resumen ya desactualizado, no hay forma de volver a leer sin cerrar la pestaña y provocar una transición de selección. Corrección: acción explícita «Volver a leer» en la superficie. Una relectura automática en cada drenaje no se adopta: contradice «una consulta por materialización» y añadiría un barrido tras cada comando con el panel visible.

## T2-F5. Las vistas de Cama salen como «ninguna»

Cama persiste `View = null` de forma canónica y `BrowserRackSummaries` descarta las vistas en blanco. Localizar vista funciona, porque `NavigationTargets` usa `RackViewCodec.Decode`. Corrección: decodificar con el mismo codec, con la vista canónica cuando hay dirección.

## T2-F6. Cada presentación regenera la tira

`RebuildTabs` crea siempre una lista nueva y `Tabs` notifica cambio, así que `ItemsControl` destruye y recrea los botones en cada drenaje y en cada inicio o fin de comando. Se pierde el foco de teclado y un clic puede perderse. Corrección: conservar la instancia si la lista es igual por valor.

## Observación que no se corrige aquí

Con `PICKADD=0`, partiendo de Uno(B), activar a mano la pestaña A y pulsar Seleccionar: el eco no avanza el contexto, y al picar B llega un contexto igual al último y la pestaña no sigue a B. Es una consecuencia del eco por contenido en un caso raro y exige tocar el núcleo o el eco. Se registra como limitación conocida.
