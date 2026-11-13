# I-64 F4-T3C2-COMMAND-ADVISOR: revisión adversarial independiente del paquete v1

- **Objeto:** `I-64-f4-t3c2-gate-contract-package.md` (v1), contra la autorización del Owner (`owner-f4t3c2-authorization.md`) y las autoridades que cita.
- **Código:** el worktree en HEAD `e89948b9` (código de producto de `7bd275e4`). Entre los dos no hay cambios en `src/` ni en `tests/`.
- **Método:** solo lectura. Leí el código real, los archivos versionados de `tests/` y las autoridades. Los mapas se usaron como pistas y se verificó cada uno contra el código. No ejecuté dotnet, ni builds ni AutoCAD.

## Veredicto

**EMITIBLE TRAS CORRECCIONES.** Hay 0 hallazgos bloqueantes, 2 importantes y 11 menores.

El diseño es sólido. No encontré ninguna guarda versionada que el diseño rompa sin declararla. Los hechos H-01..H-23 y las trampas G-01..G-20 coinciden con el código, en líneas y en aserción literal. El flujo del comando no tiene ningún camino que descarte sin «Sí». Los dos importantes son de cobertura de requisitos del Owner. Ninguno es un defecto del flujo que produzca efecto indebido.

## Hallazgos

### I-1 (IMPORTANTE). El «destino ambiguo» no tiene decisión ni prueba, y el comando lista y pregunta sobre otro documento antes de rechazar

- **Referencias:**
  - paquete D-T3C2-02 (pasos 2, 8 y 11), INV-F4T3C2-03 y §1;
  - autorización: «targets the intended draft unambiguously» y «ambiguous target -> clear message + zero effect»;
  - `f4-dispositions.md:100` (C-F4-03, aceptada por el Owner): «Ante un destino ambiguo falla con un mensaje claro y sin efecto»;
  - código: `WorkspaceHostCore.cs:223-241` (`GetSnapshot`), `:244-245` (`Authorize`), `:746-762` (`DiscardDocument`); `WorkspaceSessionRegistry.cs:39-46`; `AutoCadEventBridge.cs:151-163`.
- **Qué pasa hoy:**
  - `activeSession()` devuelve el `_active` del puente. `DiscardDocument` autoriza contra `MdiActiveDocument`, a través de `HostProbe`.
  - Si un manejador de `DocumentActivated` falla, la falla se contiene y `_active` puede quedar en la sesión anterior, que sigue viva. Las dos fuentes divergen.
  - En ese caso el comando hace esto:
    - lista los borradores de **otro** documento como «este documento» (C-05/C-06);
    - pregunta Q;
    - solo tras «Sí» recibe `Rejected` y escribe C-13 («se rechazó. La petición no es del documento activo.»).
  - Hay cero efecto, pero el mensaje claro llega después de una lista y una pregunta engañosas.
  - El paquete no define qué es un destino ambiguo para el comando. Tampoco cita C-F4-03, la decisión del Owner que lo exige.
- **Corrección:**
  - Definir el destino ambiguo como «la sesión activa del puente no autoriza contra el documento que ejecuta». Comprobarlo antes de leer el riesgo (paso 2) y otra vez en el paso 8.
  - Por ejemplo, el lambda del adaptador devuelve null cuando `controller.Port.Authorize(new WorkspaceRequest(s)) != RequestDecision.Accepted`. `IWorkspaceHostPort.Authorize` ya existe y no toca ninguna guarda.
  - La otra opción es un parámetro `Func<SessionId, bool>` en `Run`, con un texto propio (C-02b) o con C-02.
  - Añadir la fila a T-1 y su prueba con el doble: cero llamadas a `GetRacksAtRisk` y a `DiscardDocument`.
  - Citar C-F4-03 y D-17 en el §1. Justificar que el alcance es el documento entero, como el botón del panel: C-F4-03 habla de «la sesión y el borrador activos», y D-17 y R-C1 fijan la lista de racks del documento.

### I-2 (IMPORTANTE). Las reglas de controles y sustitutos de la autorización no están recogidas

- **Referencias:**
  - autorización, líneas 138-148: «Existing task-local protocol rules remain available only when their exact signatures reproduce… PD-F4-01… PD-F4-04 / PD-F4-05… PD-F4-06… Do not force a substitute onto a new signature. Preserve every ordinary control output.»;
  - paquete §1: «Sus requisitos se recogen sin cambio de sentido en el §3 y en los invariantes del §5»;
  - paquete §5: no tiene `ExpectedEvidence` y en `StopConditions` solo está C08, la replanificación;
  - modelo: `F4-T3C1-HOST-HARDENING/coordinator/gate-contract.json`, cuya `ExpectedEvidence` nombra PD-F4-04 y PD-F4-01.
- **Por qué importa:** PD-F4-01 lleva cuatro apariciones seguidas en nc2 (`task-close-package.md` de T3C1, §5), así que es muy probable que vuelva en esta subtarea. Lo que dice el §1 es falso para esta regla.
- **Corrección:**
  - Añadir al §5:
    - una `ExpectedEvidence` que fije los sustitutos disponibles solo con su firma exacta: PD-F4-01 (lista blanca exacta, Contract antes que Scope), PD-F4-04/05 (Handoff/Identity) y PD-F4-06 (variación de Ci/Trailer sin relación);
    - que se conserva cada salida ordinaria de control;
    - una parada: una firma nueva no se cubre con ningún sustituto.
  - En `ForbiddenWriteScope`, nombrar de forma explícita `docs/` y `.github/`, como en el contrato de T3C1, y fijar los prefijos para que no se solapen con las rutas exactas de `src/RackCad.Application/Workspace/`.

### M-1 (MENOR). La guarda que sustituye a M-1 tiene tres formulaciones y ninguna da la forma textual exacta

- **Referencias:** paquete D-T3C2-08 («el argumento es `_host` o un envoltorio cuyo primer argumento es `_host`»), D-T3C2-05 (el argumento real es el campo `_closeAdvisor`), INV-F4T3C2-07 y D-T3C2-10. Código: `tests/RackCad.Tests/Workspace/WorkspaceLifecycleGuardTests.cs:277-288` (284).
- **El problema:** con el diseño, el argumento de `SetLifecycleAdvisor` es un identificador de campo, no una construcción. «Un envoltorio cuyo primer argumento es `_host`» no se puede comprobar sobre el argumento sin seguir la asignación del campo.
- **Corrección:** fijar el patrón exacto, en el cuerpo del constructor y en este orden:
  1. `_host = new WorkspaceHostCore(`
  2. `_closeAdvisor = new WorkspaceCloseAdvisorRelay(_host,`
  3. `_bridge.SetLifecycleAdvisor(_closeAdvisor);`

  Con exactamente un `new WorkspaceCloseAdvisorRelay(` en el archivo y un `.SetLifecycleAdvisor(`. Decir también si se conserva la alternativa «o `_host`». Lo coherente es quitarla, o explicar que la guarda nueva de D-T3C2-10 la cierra. Usar el mismo texto en INV-07, D-08 y D-10.

### M-2 (MENOR). R-C1 (M-12) no pasa entero a la guarda nueva

- **Referencias:** T3 §8 R-C1 («`GetOrCreate` sigue apareciendo una sola vez (M-12)») y paquete D-T3C2-10.
- **El hueco:** la guarda nueva mira solo el cuerpo de `RackPanelDiscard`. Un ayudante del mismo archivo, `ConfirmOnCommandLine` o `WriteToCommandLine`, podría llamar a `WorkspaceController.GetOrCreate()` sin que nada lo vea. `WorkspaceHostSourceGuardTests.cs:70` solo hace un `Contains`.
- **Corrección:** añadir que el archivo del comando tiene exactamente un `WorkspaceController.GetOrCreate(`, dentro de `RackPanel`, con su autoprueba.

### M-3 (MENOR). Faltan tres declaraciones que piden R-C2 y D-T3-08

- **Referencias:**
  - T3 §8 R-C2: «Ambas ampliaciones acotadas de D-04 se declaran» y «El envoltorio no vacía texto vivo (D-T3-08)»;
  - `IWorkspaceLifecycleAdvisor.cs:6-7`: «Both calls … write nothing and open no dialog». El archivo está en `ForbiddenWriteScope`.
- **Lo que falta:**
  - El paquete no declara como ampliaciones acotadas de D-04:
    - la escritura en la línea de comandos y la lectura de `DocumentManager` dentro de `AdviseClose`;
    - la lectura del riesgo y el mensaje diferido en `SessionDestroyed`.
  - No dice que el envoltorio no vacía texto vivo.
  - La documentación de la interfaz queda desfasada y no se puede corregir.
  - Por último, el contexto de registro de un fallo de `inner` cambia: hoy es el `Log("OnBeginDocumentClose")` del núcleo, y con el envoltorio pasa a ser el `report` del envoltorio. No hay ninguna prueba que lo fije, pero el contexto debe quedar escrito.
- **Corrección:**
  - declarar las dos ampliaciones y la no-vaciado en D-T3C2-04 y en INV-06;
  - declarar el desfase de la documentación de la interfaz, o admitir en el alcance un cambio solo de documentación;
  - fijar los textos de contexto de `report` (por ejemplo `WorkspaceCloseAdvisorRelay.AdviseClose`).

### M-4 (MENOR). Nada asegura que un fallo del propio envoltorio en `SessionDestroyed` no impida delegar

- **Referencia:** paquete D-T3C2-04: solo la lectura del riesgo va en un `try`.
- **El riesgo:** si la composición o el añadido del mensaje lanzaran, `inner.SessionDestroyed` no se llamaría. No habría retirada de borradores ni aviso de pérdida del host. Hoy es prácticamente imposible, porque las listas de `PilotRiskView` nunca son null, pero es un invariante del envoltorio.
- **Corrección:** «todo el trabajo propio previo a la delegación va contenido (report); la delegación se hace siempre, una vez», con una prueba que use una escritura o una composición que lanza.

### M-5 (MENOR). La mitad de M-1 que toca al puente desaparece sin disposición

- **Referencias:** `f4-dispositions.md:866` (F4-E35): «M-1: fijar solo el orden … y su conteo, **y que el reenvío llame a `_core.SetLifecycleAdvisor(`**». La guarda exacta está en `WorkspaceLifecycleGuardTests.cs:233-244`, con `Equal("internalvoidSetLifecycleAdvisor(…)=>_core.SetLifecycleAdvisor(advisor);", …)`.
- **El problema:** R-C10 y el paquete solo tratan la mitad del controlador. Que esa guarda congele una expresión no estorba a T3C2, que no toca el puente, pero la orden del Owner pide convertir en invariante la guarda congelada.
- **Corrección:** decidirlo de forma explícita: o convertirla en invariante (el reenvío interno llama a `_core.SetLifecycleAdvisor(advisor)`), sin coste porque el archivo ya está en el alcance, o declararla como no necesitada y dejarla así, con la razón.

### M-6 (MENOR). INV-F4T3C2-01 no tiene un método de verificación para las dos clases de Application

- **Referencias:** paquete INV-01 y D-T3C2-10.
- **El hueco:** el comando y el envoltorio reciben el `IWorkspacePilotEditPort` completo. Solo está prevista una prueba de «ningún `DiscardRack`» para el comando, y ninguna para el envoltorio.
- **Corrección:** con el doble que registra las llamadas, afirmar el conjunto exacto de operaciones:
  - el comando llama solo a `GetRacksAtRisk` y a `DiscardDocument`;
  - el envoltorio llama solo a `GetRacksAtRisk`;
  - ninguno llama a `TryBeginEdit`, `TryReload`, `TryCheckNow`, `SetPendingText`, `CommitField`, `CrossBoundary` ni `Take*`.

  Así queda cubierto el oráculo crítico de cero escrituras authored y sin lecturas de la intención.

### M-7 (MENOR). La fila de veto obsoleta es una desviación de una regla cerrada de T3B2

- **Referencias:**
  - paquete D-T3C2-06, último punto, y §7.6;
  - `docs/initiatives/I-64-f4-t3b2-gate-contract-package.md:107` (D-T3B2-04, «Se quitan: … el de veto, con un `DiscardDocument` con `Discarded` de esa sesión»);
  - `WorkspaceBrowserViewModel.cs:478-481`, donde solo el gesto del panel llama a `ForgetVeto`.
- **Es aceptable como limitación,** porque no se puede cerrar sin romper otras guardas o el alcance:
  - `GetRacksAtRisk` solo se admite en los dos gestos (`WorkspacePilotCloseSyncGuardTests.cs:126-134`);
  - el host no cambia.
  - La fila se purga cuando muere la sesión (`PurgeDeadSessions`).
- **Pero** debe llamarse «desviación declarada de D-T3B2-04 e INV-F4T3B2-08», no solo «limitación».
- **Además,** el campo y las marcas del panel solo se re-presentan en el siguiente `EnteringQuiescentState`, por `CommandStateChanged` y el `Refresh` de la paleta. Una prueba de UI barata en `WorkspacePilotCloseSyncTests.cs` lo dejaría como evidencia: un descarte externo con el doble, después `Present`, y comprobar que el campo queda sin pendiente y la fila sigue.

### M-8 (MENOR). Trampas de compilación y de guardas que el paquete no avisa

- **El aviso de nulabilidad tumba la build:** la build Release de la solución usa `-NoWarnings` (`eng/ci/verify-autocad-references.ps1:110-113` y `:601`).
  - Los dos archivos nuevos de Application usan anotaciones `?` (`IWorkspacePilotEditPort? port`, `Func<SessionId, string?>`, `Action<string, Exception>? report`). Sin `#nullable enable` dan CS8632, porque `Directory.Build.props` pone `Nullable` en disable.
  - Las pruebas «sin motivo» de C-12..C-14 y de R-1 llaman a `EditDiscardOutcome.Busy(string)`, que no admite null (`WorkspaceEditIntent.cs:331`). Con null dan CS8625; el precedente usa `reason!` (`WorkspacePilotFieldViewModelTests.cs:1400`), o `" "`.
- **`\.Result\b` está prohibido en todo el Plugin** (`WorkspaceHostSourceGuardTests.cs:435-436`). Hay que tenerlo en cuenta al nombrar variables en `ConfirmOnCommandLine`.
- **R-4 en `IWorkspacePilotEditPort.cs`:** la guarda lee el texto con comentarios (`WorkspacePilotEditGuardTests.cs:782-788`) y prohíbe `Transaction`, `\bDatabase\b`, `\bCommit\s*\(` y las palabras de I-63 sin distinguir mayúsculas, también en la documentación nueva.
- **Corrección:** añadirlas a la lista de trampas del §2.5.

### M-9 (MENOR). El esqueleto del RED queda ambiguo

- **Referencias:** paquete §4 RED e INV-13.
- **El problema:**
  - «El controlador … sin cambio» y «`Existing` da null» chocan: con el controlador sin cambio no existe `WorkspaceController.Existing`, así que el cuerpo RED de `RackPanelDiscard` no puede usarlo.
  - En el RED, `build-plugin` no corre, porque `needs: tests` y `tests` falla por `ExpectRed`. La compilación del Plugin en el RED no la comprueba la CI.
  - En el RED «el envoltorio solo delega». Una prueba aislada con un `inner` que lanza fallaría por excepción y no por aserción.
- **Corrección:**
  - fijar el cuerpo RED de `RackPanelDiscard`, por ejemplo vacío dentro de su `try/catch`;
  - decir que «`Existing` da null» se refiere a `WorkspaceLazyInstance`;
  - exigir `Record.Exception` en las pruebas del envoltorio y del núcleo, como en T3C1.

### M-10 (MENOR). La guarda de los conteos de inglés residual congela un detalle de F3

- **Referencias:** paquete D-T3C2-07 «Comprobado» y D-T3C2-10, último punto.
- **El problema:** fijar «"There is no active document." 2 veces…» fuera de los tres cuerpos congela literales de F3, que F7 traducirá. Va contra «replace it with an invariant-oriented guard».
- **Corrección:** limitar la guarda de T-3 a los tres cuerpos, que es el invariante. Si se quiere probar que F3 no se tocó, hacerlo sobre los cuerpos de F3 (`ReadSummaryOnActiveDocument`, `RunOnActiveDocument`, `ReadPreviewWitness`) y documentar que F7 la cambia.

### M-11 (MENOR). Precisiones y limitaciones que faltan por declarar

1. D-T3C2-02 dice que una excepción de `write` sale «antes de cualquier descarte». No es verdad para C-11..C-14, que se escriben después de `DiscardDocument`: el descarte ya ocurrió y el texto se pierde, y queda el «RackCad error: …» de `RackCommandSupport.Report`.
2. El desconocido es hoy inalcanzable. Si llegara a ocurrir, el documento quedaría vetado, porque `AdviseClose` da Veto cuando la evaluación lanza (`WorkspaceHostCore.cs:947-951`), y sin descarte posible, porque C-03 lo rechaza. Es coherente con R-C7, pero debe declararse como residual.
3. Si se destruye un documento que no es el activo, el relevo no se dispara en la destrucción (`AutoCadEventBridge.cs:172`). El mensaje P-0x espera al siguiente `DocumentActivated`, que puede tardar. Es compatible con R-C12, pero debe quedar declarado.
4. El comando no se añade a la ayuda (`RackCommandReference`, `CustomPropertiesHelpCensusTests`), igual que RACKPANEL. Se descubre por V-01/V-02 y por las filas del panel. Conviene decirlo para cerrar «do not leave the command undiscoverable».

## Lo comprobado y correcto

- **Hechos y trampas:** H-01..H-23 y G-01..G-20 coinciden con el código, en líneas y en aserción literal. Las correcciones de R-C14 son exactas:
  - la 251 es un conducto;
  - `ReadPreviewWitness` tiene 4 literales (157, 174, 178 y 182) y no tiene llamadores.
- **H-04 y «comparator failure must not make explicit discard impossible»:**
  - si el comparador falla, la vía de reserva (`WorkspaceHostCore.cs:779-800`) da inciertos **conocidos**;
  - el `catch` externo es inalcanzable con `RackIdsOf` y `Find` (`DraftRegistry.cs:186-195`);
  - `DiscardPilotEdits` no llama al comparador (`:514-535`).

  La distinción es correcta y no contradice R-C7.
- **El flujo del comando:**
  - ningún camino descarta sin `OK` + `"Si"`;
  - `GetSnapshot().ActiveSession` no depende del reposo;
  - `DiscardDocument` autoriza contra `MdiActiveDocument` sin exigir reposo ni ausencia de comando («needs no rest, no absence of command», `:514-519`);
  - las listas vienen ordenadas por ordinal (`DraftRegistry.cs:186-195`), y la TOCTOU en orden y por ordinal es la de `SameList` del panel;
  - las excepciones de las lecturas cuentan como el desconocido. La de `DiscardDocument` no puede convertir el resultado.
- **El envoltorio:**
  - partir de Veto y dar Allow solo si `inner` da Allow es idéntico a `EventBridgeCore.OnBeginDocumentClose` (`:357-375`);
  - `default(CloseDecision)` es Allow (`BridgeOutcomes.cs:8-12`);
  - las consultas del puente en `AdviseClose` no tienen efectos y no olvidan la secuencia (`WorkspaceLifecycleGuardTests.cs:178-190`);
  - ninguna prueba ni guarda depende del `Log("OnBeginDocumentClose")` con el cableado del controlador;
  - el relevo se dispara después de `TellAdvisorTheSessionEnded`, con `ActiveSession` null en la destrucción y también con la pestaña de inicio.
- **Barrido de guardas:** revisé, sin encontrar ninguna rotura no declarada:
  - en Core: `WorkspaceHostSourceGuardTests` (las 45 pruebas), `WorkspaceBridgeSourceGuardTests`, `WorkspaceSourceGuardExceptionTests`, `WorkspaceLifecycleGuardTests`, `WorkspacePaletteHostSourceGuardTests` (también la metaguarda de 644-659), `WorkspaceHostC2SourceGuardTests`, `WorkspaceIdleSchedulingGuardTests` (G6, G9 y la metaguarda de 309-319), `WorkspacePilotEditGuardTests` (`ControllerViolations`, `DialogViolations` y modal), `WorkspaceEditIntentGuardTests` (`NamesThePilot`, `LoadCatalog` sin `Document\b`, la independencia del adaptador y el constructor), `WorkspaceNavigationHostTests` (613-621 y 669-678), `WorkspaceBrowserHostGuardTests`, `WorkspaceNavigationAuthorityTests`, `WorkspaceModelBoundaryTests` (nombres de tipo y miembros), `WorkspaceBrowserBoundaryTests` (palabras de los archivos `Browser*`), `WorkspaceDraftBoundaryTests` (tipos del espacio padre), `WorkspacePilotCloseSyncGuardTests` (colas y llamadores de la UI) y `WorkspaceHostHardeningGuardTests` (solo el host y el soporte);
  - fuera del filtro: `CustomPropertiesCommandGuardTests`, `SelectiveEditorOpenTests` (conteo bruto), `CustomPropertiesHelpCensusTests` de UI, `PushBackRoundTripSourceGuardTests` (unicidad), `RackProjectionCommandGuardTests` y `StructuralAnalysisPluginCommandGuard`.

  Los nombres `WorkspaceDocumentDiscardCommand` y `WorkspaceCloseAdvisorRelay` no chocan con `Draft|Editor|Subscription|Tab|Log` ni con las palabras de `Browser*` o `Drafts/`.
- **D-T3C2-08 y D-T3C2-09:** no debilitan ninguna prohibición.
  - «A lo sumo un RACKPANELDESCARTAR» en `DeclaresEntryPoint`, junto con la guarda exacta nueva, mantiene la unicidad.
  - La excepción de `GetKeywords` solo afecta al archivo del comando, que no tiene manejadores de eventos, y se ciñe a `ConfirmOnCommandLine`.
  - El conteo 2 → 3 de `string.Equals(path, …AdapterPath…)` sigue siendo una excepción por igualdad de ruta.
  - No hay suscripciones nuevas ni toques al puente.
- **T-3:** los 10 literales y sus líneas son exactos, y los conteos residuales también. Ninguna prueba fija el inglés. No hay ningún otro literal del Plugin que llegue al campo: los catch de `ReadIntent` y del adaptador pasan `ex.Message`, y los tipos de resultado no tienen literales.
- **Los textos** T-1, T-2 y D-T3C2-06 no contienen ninguna subcadena prohibida por las guardas que los ven, con o sin mayúsculas según cada regla. Las pruebas 783-784 (`Actualizar` y `Aplicar`, que distinguen mayúsculas) siguen pasando.
- **RED, filtros y alcance:**
  - el RED es coherente con R-C3: los censos de fuera del filtro pasan con el atributo y sus cambios ya en el RED;
  - `ExpectRed` se cumple en los dos filtros: en UI, por 780-785 y 805;
  - las líneas base 2517/454 son las de la CI de `7bd275e4`;
  - el alcance de 19 rutas está completo: el doble de UI ya tiene `DiscardRackResponder` y no hace falta tocarlo.

## Riesgos para el smoke con AutoCAD (Smoke-2)

1. `GetKeywords` con `AllowNone` y `Default "No"`: Intro debería dar `OK` con `"No"` (o `None`). No es comprobable sin AutoCAD, pero es inocuo en los dos casos.
2. La pregunta con la paleta visible y con el foco en ella: el texto tecleado podría ir al panel. También el foco después del comando.
3. V-01/V-02 escritos dentro de `BeginDocumentClose`, con CLOSE, CLOSEALL, QUIT, el «×» de la pestaña y un documento vetado que no es el activo. Comprobar qué devuelve `isActive` en cada caso.
4. P-01..P-03 escritos durante un `DocumentActivated`: al abrir o crear un documento nuevo, al volver desde la pestaña de inicio y cuando el destruido no era el activo.
5. Cambiar de documento durante la pregunta. Puede dar Cancel, la vía C-07, o C-08.
6. Gestos del panel durante la pregunta (descartar o editar en la paleta): deben acabar en C-10.
7. El estado del panel después del descarte por comando: el campo sin pendiente tras el reposo, la fila de veto obsoleta y una confirmación pendiente del panel (`NoRiskText`).
8. RACKPANELDESCARTAR antes de RACKPANEL: C-01, sin crear controlador ni paleta, y un RACKPANEL posterior normal.
9. Los motivos del campo en español: «En espera: Hay un comando en curso.».
10. Desplegar juntos Application y Plugin, porque hay tipos nuevos.
