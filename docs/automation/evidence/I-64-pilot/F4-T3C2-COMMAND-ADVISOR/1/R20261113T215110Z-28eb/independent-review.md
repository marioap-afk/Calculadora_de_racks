# Revisión adversarial independiente del código — I-64 F4-T3C2-COMMAND-ADVISOR

- **Repositorio:** el worktree de I-64, rama `architecture/workspace-persistente-rackcad`.
- **Rango:** `3b1ee950fa49e6a1545b9a59cd31424a9f1afb60..4e34bb56740ea7bd4a53f9a11a143d0af588f2b0`: RED `730dd7da` (esqueleto y pruebas) y GREEN `4e34bb56` (implementación), 20 archivos.
- **Método:** solo lectura con `git show`, `git diff`, `git log` y `git grep`. Los textos de T-1, T-2, T-3 y D-T3C2-06 se compararon byte a byte contra el paquete con un script sobre la salida de `git show`. No se compiló, no se ejecutaron pruebas, no se consultó la CI y no se abrió AutoCAD.
- **Autoridades leídas:** `gate-contract.json` de F4-T3C2, el paquete v3 (§2, §3 con T-1, T-2 y T-3, §4, §5, §8 y §9) y `owner-f4t3c2-authorization.md`. Además, las guardas y el código de producto que tocan los cambios.

## Veredicto

**ACEPTABLE CON MENORES**: 0 BLOQUEANTE, 0 IMPORTANTE y 8 MENOR.

El código de producto cumple el oráculo crítico y los invariantes INV-F4T3C2-01 a 14. No encontré ningún camino que escriba el DWG, que descarte sin «Sí», que actúe sobre otra sesión o sobre un destino ambiguo, que elija un rack solo o que trate el desconocido como «nada».

Los menores son de robustez de guardas, de nombres y comentarios, de documentación y de una prueba redundante. Ninguno exige cambiar el comportamiento.

## Hallazgos

### M-1 (MENOR). La guarda de `GetKeywords` no fija que la respuesta verdadera exija `PromptStatus.OK` **y** `"Si"`

- **Dónde:** `tests/RackCad.Tests/Workspace/WorkspaceCommandAdvisorGuardTests.cs:133-147` (agujas 138-143) y sus autopruebas 155-164.
- **Qué comprueba:** las agujas exigen por separado:
  - `Keywords.Add("Si")` y `Keywords.Add("No")`;
  - `Keywords.Default = "No"` y `AllowNone = true`;
  - `.Status`, `.StringResult`, `StringComparison.Ordinal` y `PromptStatus.OK`.

  Ninguna fija el `return` ni el literal con el que se compara `StringResult`.
- **Escenario que falla:** cualquiera de estas dos variantes de `ConfirmOnCommandLine` pasa todas las guardas nuevas y existentes, y todas las pruebas de Core:
  - `return answer.Status == PromptStatus.OK && string.Equals(answer.StringResult, "No", System.StringComparison.Ordinal);`
  - `return answer.Status != PromptStatus.OK || string.Equals(answer.StringResult, "Si", System.StringComparison.Ordinal);`

  Con la primera, Intro (la opción por defecto «No») confirmaría el descarte. El mapeo Sí/No del Plugin no se puede probar en Core, así que esta guarda es su única protección automática antes de Smoke-2.
- **Invariante:** INV-F4T3C2-04 y D-T3C2-03 («verdadero solo con `PromptStatus.OK` y `StringResult` igual a `"Si"`»).
  - El código actual es correcto (`src/RackCad.Plugin/Workspace/WorkspacePanelCommand.cs:69`).
  - La guarda cumple la letra de D-T3C2-10, pero no protege la regla del Owner «explicit discard only».
- **Sugerencia:**
  - exigir el `return` con `PromptStatus\.OK\s*&&\s*string\.Equals\s*\(\s*\w+\.StringResult\s*,\s*"Si"\s*,`;
  - añadir autopruebas con `"No"` y con `||`.

### M-2 (MENOR). La guarda del cuerpo de `RackPanelDiscard` solo exige que aparezca `.Authorize(`

- **Dónde:** `WorkspaceCommandAdvisorGuardTests.cs:101-111` (línea 106) y su autoprueba 124.
- **Escenario que falla:** pasan la guarda
  - `session => { controller.Port.Authorize(new WorkspaceRequest(session)); return true; }`,
  - o `... != RequestDecision.Accepted`.

  El destino ambiguo (C-15, pasos 2b y 8) dejaría de detectarse, y el núcleo de Core no lo vería, porque recibe el delegado ya hecho. La guarda tampoco fija:
  - que el puerto sea `controller?.PilotEdit`;
  - que la sesión salga de `controller?.Port.GetSnapshot().ActiveSession`.
- **Invariante:** INV-F4T3C2-03 y C-F4-03. El código actual es correcto (`WorkspacePanelCommand.cs:45-47`).
- **Sugerencia:** fijar `Authorize\s*\(\s*new\s+WorkspaceRequest\s*\(\s*\w+\s*\)\s*\)\s*==\s*RequestDecision\.Accepted` y los dos primeros argumentos de `Run`, con su autoprueba en negativo.

### M-3 (MENOR). Varias guardas nuevas no tienen autoprueba en negativo, y una mira un cuerpo que no es el que hace el trabajo

- **Invariante:** INV-F4T3C2-12 dice «las guardas nuevas tienen su autoprueba en negativo».
- **Sin autoprueba en negativo,** en `WorkspaceCommandAdvisorGuardTests.cs`:
  - `NothingOutsideTheCommandFileOfThePluginNamesGetKeywords` (198-203);
  - `TheControllerAddsNoAsyncNoTimerNoDispatcherAndNoMessageBox` (233-239);
  - `TheReasonsThatCarryTheMessageOfTheExceptionStillCarryIt` (347-358);
  - `TheCatalogBodyDoesNotNameTheDocumentTypeAndNoSpanishReasonUsesAWordThatTheNeighbouringGuardsForbid` (360-367);
  - `TheTwoNewFilesOfApplicationAreNullableEnabledPublicAndOfTheWorkspaceNamespace` (443-458);
  - `TheRelayStartsFromVetoNeverFromTheDefaultAndNeverCallsTheBridgeNorHoldsAnyStateOfDrafts` (460-475);
  - `ExistingOfTheLazyInstanceIsAReadUnderTheSameLockThatNeverCallsTheFactory` (479-488).
- **El cuerpo equivocado:** `TheRelayStartsFromVeto…` (473) busca `_documentNameOf|_isActive|_writeNow` solo en el cuerpo de `SessionDestroyed`, pero el trabajo propio está en `RememberLoss` (`WorkspaceCloseAdvisorRelay.cs:134-160`). Un `_documentNameOf(session)` añadido allí pasaría la guarda.
- **Mitigación:** las pruebas de comportamiento cubren las tres reglas de Application:
  - `NameCalls`/`ActiveCalls` en 0 y `Written` vacío (`WorkspaceCommandAdvisorTests.cs:888-890`, `910-911` y `1298-1299`);
  - el conjunto exacto de operaciones del puerto;
  - el «Veto» de partida.

  Por eso es menor.

### M-4 (MENOR). La guarda nueva de la interfaz del asesor congela para siempre «exactamente dos sentencias», algo que F4-T3A (M-2) quitó a propósito

- **Dónde:**
  - `WorkspaceCommandAdvisorGuardTests.cs:384-408` (391: `Src.Count(body, ";") != 2`);
  - frente a `tests/RackCad.Tests/Workspace/WorkspaceLifecycleGuardTests.cs:84-85`: «the count of statements ends with the file, so a later member does not trip a guard that is about these two».
- **Escenario:** si F4-T4 o una tarea posterior añade un miembro al asesor, la guarda falla y exige un cambio declarado. Esto es justo lo que F4-T3A había evitado.
- **Encaje con el paquete:** D-T3C2-10 pide esta guarda («con dos sentencias en el cuerpo de la interfaz»), así que no es una desviación. Pero el invariante es de esta subtarea (INV-F4T3C2-14: «IWorkspaceLifecycleAdvisor cambia solo su documentación»), y la guarda queda como permanente. Responde a la pregunta 6 del encargo: es la única guarda nueva que prohíbe para siempre algo plausible después.
- **Sugerencia:** disponerlo en el cierre, ya sea acotando el mensaje y el nombre a F4-T3C2 o anotándolo como cambio declarado previsible de F4-T4.
- Las demás prohibiciones nuevas del controlador (`Task`, `async`, `Dispatcher`, `Timer`) ya existían en guardas anteriores (`WorkspacePilotEditGuardTests.cs:510`, `WorkspaceHostSourceGuardTests.cs:426-453` y `WorkspaceNavigationHostTests.cs:678`). No añaden restricción.

### M-5 (MENOR). Nombres de prueba y comentarios de guardas existentes que ya no describen lo que comprueban

- **Nombres de prueba:**
  - `WorkspaceLifecycleGuardTests.cs:295`, `TheControllerWiresTheHostAsTheAdvisorOnceInItsConstructorAfterCreatingTheHostWithNoNewSubscription`: ahora instala el envoltorio sobre el host. D-T3C2-08 permitía conservar el nombre solo «si sigue siendo verdadero», o bien poner uno nuevo.
  - `WorkspacePaletteHostSourceGuardTests.cs:321`, `TheCommandStaysTheOnlyOneWithOneAttributeNoAliasAndTheSharedReporter`, y `WorkspaceHostSourceGuardTests.cs:50`, `TheCommandAdapterHoldsNoOtherCommandAndNoAlias`: el archivo tiene ahora dos comandos. Conservar estos nombres es la opción prudente por F4T3C2-C02 (no se declararon renombres), pero confunde.
- **Comentarios desfasados:**
  - `WorkspaceBridgeSourceGuardTests.cs:50-52`: «the ONLY two refinements… RACKPANEL may be declared»;
  - `WorkspaceSourceGuardExceptionTests.cs:10-11`: «TWO exceptions»;
  - `WorkspaceSourceGuardExceptionTests.cs:279`: «The two exceptions are compared…», justo antes de la cuenta 3.
- Es cosmético.

### M-6 (MENOR). Documentación: «the one writer of the command line of the panel»

- **Dónde:** `WorkspacePanelCommand.cs:17-18` y `72-75`.
- **Qué dice:** que `WriteToCommandLine` es el único escritor de la línea de comandos del panel.
- **Qué hace el archivo:** `RackPanel` (29-30) sigue escribiendo directamente con `Editor.WriteMessage`, y `RackCommandSupport.Report` escribe los errores de los dos comandos.
- **Qué se cumple:**
  - G-12 / INV-F4T3C2-07: `WriteMessage` solo aparece en este archivo.
  - Era correcto no tocar RACKPANEL (H-01, F1 cerrado).
- **Lo impreciso:** solo la frase, frente a D-T3C2-03 («Es el único escritor… lo usan el comando y el controlador»). Basta matizarla: «the one writer of the notices of the command and the controller».

### M-7 (MENOR). La documentación R-4 del VM es imprecisa cuando la lectura lanza

- **Dónde:** `src/RackCad.UI/Workspace/WorkspaceBrowserViewModel.cs:356`.
- **Qué dice:** «When the risk is unknown (or its read throws)… the notice says that it could not be decided».
- **Qué hace el código cuando la lectura lanza:**
  - el aviso es el de `FailGesture`, «No se pudieron leer los borradores en riesgo: <motivo>» (371-375 y 910-914), y no `UnknownRiskText` («No se pudo decidir…», 87);
  - además quita cualquier confirmación abierta, y no solo la del documento.
- **Lo que sí es correcto:** lo esencial; ni pregunta ni descarta.
- **Invariante:** INV-F4T3C2-11 (R-4 es solo documentación, y debe ser exacta).

### M-8 (MENOR). Una teoría con tres casos idénticos

- **Dónde:** `tests/RackCad.Tests/Workspace/WorkspaceCommandAdvisorTests.cs:372-392`, `AnythingButAYesDiscardsNothing("no" | "enter" | "cancel")`.
- **Qué pasa:** los tres casos ejecutan lo mismo (`Answer = _ => false`), y el parámetro solo se comprueba con `Assert.NotNull`. Parece cubrir Intro y Esc, pero ese mapeo vive en el Plugin y queda para Smoke-2 (D-T3C2-12).
- No es un defecto de producto: la teoría sobredeclara lo que prueba.
- **Sugerencia:** un solo `[Fact]`, y Intro/Esc/cancelar anotados explícitamente como obligación de Smoke-2. El refuerzo de M-1 haría además de red estática.

## Lo comprobado y correcto

**Alcance**
- El diff tiene exactamente las 20 rutas del `AllowedWriteScope`. Las 4 nuevas son las declaradas.
- El GREEN solo toca 10 archivos de `src/` y ninguno de `tests/`.
- Nada toca el `ForbiddenWriteScope`. No cambian `EventBridgeCore`, `AutoCadEventBridge`, `WorkspaceHostCore`, la compuerta, `BrowserSession`, el XAML ni la forma pública del VM.
- `IWorkspaceLifecycleAdvisor.cs` e `IWorkspacePilotEditPort.cs` solo cambian comentarios. El vocabulario de G-23 está limpio.
- No hay archivo nuevo del Plugin.

**Oráculo crítico (INV-01 y 14)**
- El núcleo llama a `GetRacksAtRisk` dos veces y a `DiscardDocument` solo después del «Sí» y de una segunda lectura igual. El envoltorio solo llama a `GetRacksAtRisk`.
- No hay `DiscardRack`, `Take*`, operaciones del campo ni lectura de la intención. No hay escritura del DWG, Actualizar ni nada de F4-T4.
- No hay ventana, `MessageBox`, `EnterPanelModal`, `Dispatcher`, `Timer`, `Task`, `async`, API de foco ni suscripción nueva.
- El controlador sigue con `+=` = 3, `-=` = 0 y un `event`.

**Comando (D-T3C2-02 y 03)**
- **Flujo:** coincide paso a paso con 1-11 y 2b.
  - 2b responde C-15 sin leer el riesgo y sin preguntar.
  - El paso 8 responde C-08 si la sesión es otra o null, y C-15 si deja de autorizar.
  - El paso 10 compara las dos listas en orden y por ordinal.
  - `isTarget` solo recibe la sesión que `activeSession()` acaba de dar.
- **Desconocido:** el desconocido o una excepción dan C-03 o C-09.
- **Textos y motivos:**
  - Los 15 textos de T-1 son exactos (comprobados byte a byte; C-13, C-14 y C-11 por composición). C-02 es el neutral de la v3.
  - «Sin motivo» es `IsNullOrWhiteSpace`, también para `ex.Message`.
  - C-11 cuenta N como `RemovedRackIds` fuera de las dos listas, en cultura invariante.
  - `default(EditDiscardOutcome)` cae en Rejected, que es el valor por defecto del enum.
- **Adaptador del Plugin:**
  - Usa `WorkspaceController.Existing` (que lee `Holder.Existing` bajo el mismo `lock` y nunca llama a la fábrica). No llama a `GetOrCreate`, `OpenPanel` ni `Show`.
  - Hay un solo `WorkspaceController.GetOrCreate(` en el archivo, dentro de `RackPanel` (M-12).
  - `GetKeywords` usa Si/No, Default No, `AllowNone` y solo `PromptStatus.OK` con `"Si"` en ordinal. La respuesta se lee con `.Status` y `.StringResult`, sin `\.Result\b`.
  - No hay anotaciones `?` de tipo referencia en el comando ni en el controlador (sin CS8632).
  - El tercer delegado solo se invoca con una sesión, lo que implica controlador.

**Envoltorio (D-T3C2-04)**
- **`AdviseClose`:**
  - parte de Veto y da Allow solo con el Allow de `inner`;
  - una excepción de `inner` es Veto, con `report("…AdviseClose")`;
  - el nombre y la actividad se consultan, y el aviso se escribe, en `try` (`…VetoMessage`); un `isActive` que lanza cuenta como activo;
  - V-01 y V-02 son exactos por composición, y el último segmento del nombre se toma tras `\` o `/` sin `System.IO`.
- **`SessionDestroyed`:**
  - lee el riesgo **antes** de delegar, y todo ese trabajo propio va contenido;
  - la delegación ocurre siempre, una vez y sin contener;
  - no llama a `documentNameOf` ni a `isActive`, ni consume las colas;
  - P-01, P-02 y P-03 son exactos.
- **El resto:**
  - `TakeMessages(null)` conserva los mensajes y `TakeMessages(sesión)` los vacía en orden;
  - un `report` que lanza se ignora;
  - el envoltorio no guarda estado de borradores.

**Controlador (D-T3C2-05)**
- **Instalación:** el orden es `_host` → `_pilotEdit` → `_closeAdvisor = new WorkspaceCloseAdvisorRelay(_host, …)` → `_bridge.SetLifecycleAdvisor(_closeAdvisor)`. Hay uno solo de cada `new WorkspaceHostCore(`, `new WorkspaceCloseAdvisorRelay(` y `.SetLifecycleAdvisor(`.
- **Relevo:** `WriteCloseMessages()` va después del `try`/`catch` del refresco y antes de `NotifyStateChanged()`, y el cuerpo del relevo no nombra `_bridge.`.
- **`DocumentNameOf`:** solo memoria (`TryGetSession`, `IsDestroyed`, recorrido de `DocumentManager` por `UnmanagedObject`), con variable local.
- **El nuevo `using Autodesk.AutoCAD.ApplicationServices;`:** no introduce ambigüedad.
  - El nombre simple `Application` sigue resolviendo al espacio `RackCad.Application`, por el espacio contenedor `RackCad`; además, el archivo usa el alias `AcApplication`.
  - `Exception` se escribe `System.Exception`.
  - Ningún tipo de `RackCad.Application.*` que use el controlador se llama como un tipo de ese espacio de AutoCAD.
  - Ninguna guarda lo prohíbe: G-13, `ForbiddenFieldLines`, `PublicSurfaceLeaks`, `WorkspaceHostSourceGuardTests.cs:474` y la frontera de `WorkspaceHostSourceGuardTests.cs:401`.

**Aviso del panel y motivos (D-T3C2-06 y 07)**
- Los dos textos de veto del panel son exactos y no nombran ninguna vía de aplicar.
- La prueba de evidencia de la desviación de D-T3B2-04 sigue los 4 pasos: veto presentado, descarte externo, `Present`, campo sin pendiente con la fila presente.
- Los 10 literales de T-3 son exactos y están en sus cuerpos.
  - El inglés que queda fuera de ellos es el medido: 2, 2, 1 y 1.
  - `ex.Message`, F3, `ReadPreviewWitness` y el `Debug.Assert` siguen intactos.

**Guardas existentes (D-T3C2-08 y 09 y §4)**
- Cada hunk corresponde a un cambio declarado:
  - la instalación del asesor, con agujas regex en orden y los conteos 1/1/1, 3 y 0, sin `.Start(`;
  - el reenvío del puente, que pasa a invariante (una llamada, una sentencia, `internal` y ningún `public`), con los conteos 1/16/16/16 conservados;
  - `WorkspaceHostSourceGuardTests` 49-62, que sigue con 45 pruebas;
  - `WorkspacePaletteHostSourceGuardTests` 325-331;
  - la rama nueva por igualdad exacta de ruta en `HandlerForbiddenCallsFor`;
  - `DeclaresEntryPoint`, con exactamente un RACKPANEL, a lo sumo un RACKPANELDESCARTAR y ningún otro;
  - en `WorkspaceSourceGuardExceptionTests`, las autopruebas y la cuenta 2 → 3 por igualdad de ruta;
  - el censo por nombre y el bruto 39 → 40. Medí 40 `[CommandMethod(` en `src/RackCad.Plugin`.
  - en `WorkspacePilotCloseSyncTests`, 780-785 y 805.
- No encontré ninguna prohibición perdida ni excepción por prefijo.
- Las guardas vecinas que podrían romperse siguen compatibles por inspección: G-11, G-14, G-15, G-16 y G-17, la privacidad de la navegación, los nombres de tipo sin `Tab`/`Draft`/`Editor` y la unicidad de comandos.

**Pruebas de comportamiento**
- Discriminan las implementaciones incorrectas plausibles:
  - Allow por defecto;
  - una delegación omitida cuando el riesgo lanza;
  - el riesgo leído después de delegar, con el host real: la pérdida desaparecería;
  - una TOCTOU que no compara orden, mayúsculas ni inciertos;
  - `isTarget` no consultado tras el «Sí»;
  - prefijos duplicados en los textos;
  - pérdidas escritas al instante;
  - un `TakeMessages` que no vacía o que vacía con null;
  - operaciones prohibidas del puerto, que el doble registra y hace lanzar.
- R-1 discrimina el `WhyOf` de Busy con y sin motivo.
- R-2 discrimina la memoria `_confirmUnread` sin `CancelDiscard`.

**RED**
- El esqueleto coincide con el §4:
  - los dos atributos y los censos;
  - `RackPanelDiscard` con un `try` vacío y su `catch`;
  - `Run` vacío y el envoltorio que solo delega, con `TakeMessages` vacío;
  - `Existing` en null;
  - el controlador, los textos, la UI y la documentación sin cambio.
- Por inspección, las pruebas nuevas o cambiadas fallarían por aserción:
  - las excepciones se recogen con `Record.Exception`;
  - `LastText` hace `Assert.NotEmpty`;
  - `BodyOf` hace `Assert.True`.
- Los censos de fuera del filtro pasan. No lo comprobé ejecutándolo.

## Observaciones sin hallazgo (comportamiento declarado o del paquete)

- **`WriteCloseMessages`:** vacía la lista antes de escribir. Si una escritura lanza a mitad, los mensajes restantes se pierden y quedan como un solo `LogFault`. Es conforme a D-T3C2-05.
- **La pérdida que escribe el envoltorio** es informativa y puede no coincidir con la del host en el residual conocido (H-13). Por ejemplo, tras un Allow con un comparador que falla después, puede salir P-02.
- **Leer `PilotEdit` crea el host de navegación** (D-T3C2-03). El adaptador evalúa `controller?.PilotEdit` antes de `Run`, también cuando la respuesta va a ser C-02.

## Riesgos para el Smoke-2 con AutoCAD (no son defectos)

1. **La pregunta con la paleta visible.** La paleta tiene `KeepFocus = true`, así que hay que comprobar:
   - que el teclado llega a la línea de comandos;
   - que «S»/«Si» elige Sí, que «Sí» con tilde se rechaza, y que Intro, Esc o cancelar dan «No se descartó nada.»;
   - que un clic en la paleta durante la pregunta no la rompe.

   Recordatorio: tras dar foco a la paleta, el primer clic en el dibujo solo activa; el comando se escribe con el teclado.
2. **`WriteMessage` dentro de `BeginDocumentClose`.** Hay que comprobar V-01 y V-02 con CLOSE, con la «X» de la pestaña (activa y no activa: si AutoCAD activa primero, sale V-01), con CLOSEALL y con QUIT. Que el texto sea visible y que recorrer `DocumentManager` durante el cierre sea seguro. Y el orden respecto del diálogo «¿Guardar cambios?».
3. **Pérdidas escritas en el relevo de `DocumentActivated`.** Hay que comprobar:
   - que salen en la línea de comandos del documento recién activado y sin romper su prompt;
   - que con la pestaña de inicio esperan;
   - que con QUIT solo queda el registro (declarado).
4. **El panel tras el comando.**
   - Su refresco depende del `CommandEnded` (relevo `CommandStateChanged`), porque el comando no llama a `NotifyStateChanged`.
   - Hay que comprobar el campo y las pestañas de los borradores descartados, y la fila de veto obsoleta (desviación declarada).
5. **Texto vivo del campo que aún no llegó al host.** Si el `TextBox` no empuja el pendiente en cada pulsación, el borrador se tomaría por limpio y se descartaría. Hay que comprobar el estado del campo después.
6. **Uso del panel durante la pregunta.** Si la paleta sigue interactiva, la TOCTOU por listas no detecta más texto pendiente en un rack que ya estaba en riesgo. Es una limitación de diseño heredada del botón del panel (R-B8).
7. **RACKPANELDESCARTAR antes de RACKPANEL.** Debe dar C-01, el controlador no debe crearse, y RACKPANEL debe funcionar después.
8. **Historial y repetición.** Intro repite el comando, que pregunta otra vez con «No» por defecto. UNDO tras el comando no restaura borradores (es memoria). También el comportamiento con AutoCAD no inglés (palabras clave globales «Si»/«No»).
9. **Nombres de documento.** Sin guardar («Drawing1.dwg»), rutas largas, caracteres especiales y guillemets en la línea de comandos.
10. **Los 10 riesgos de la revisión de la v1 del paquete** que pasaron a la lista de Smoke-2 (§8).

## Límites de esta revisión

- No compilé ni ejecuté pruebas, ni consulté la CI del RED o del GREEN.
- La compilación del Plugin (por ejemplo, la ambigüedad de tipos del nuevo `using`) y el resultado RED/GREEN se juzgaron por inspección del texto.
