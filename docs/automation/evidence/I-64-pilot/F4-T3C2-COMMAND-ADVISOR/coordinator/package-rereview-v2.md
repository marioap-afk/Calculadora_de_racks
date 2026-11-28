# I-64 F4-T3C2-COMMAND-ADVISOR: re-revisión adversarial independiente del paquete v2

- **Objeto:** `I-64-f4-t3c2-gate-contract-package.md` (v2). Lo comparo con la v1 (`_package-v1.md`), con la revisión de la v1 (`independent-review.md`, 0/2/11), con la autorización literal del Owner (`owner-f4t3c2-authorization.md`) y con las autoridades que cita.
- **Código:** worktree en HEAD `e89948b9`, con el código de producto de `7bd275e4`.
- **Método:** solo lectura. Usé `git grep` y `git ls-files`, y leí el código y los archivos versionados de `tests/`. No ejecuté dotnet, builds, pruebas ni AutoCAD, ni lancé subagentes. Los mapas sirvieron solo de pistas.

## Veredicto

**EMITIBLE TRAS CORRECCIONES.** Hay 0 hallazgos bloqueantes, 0 importantes y 11 menores.

- **Los dos importantes de la v1 quedan resueltos.**
  - I-1: el delegado `isTarget` y la fila C-15.
  - I-2: la `ExpectedEvidence` de los controles.
- **El adaptador `isTarget` es correcto:**
  - compila;
  - no rompe ninguna guarda;
  - en el camino normal, una sesión legítima siempre autoriza.
- **La documentación de `IWorkspaceLifecycleAdvisor.cs` no la lee ninguna guarda que prohíba palabras.**
- **Los 11 menores son de dos tipos:**
  - Precisión del contrato. Los más útiles son estos:
    - N-1: el caso espejo de I-1;
    - N-2: el orden de `_pilotEdit`, que no fija ninguna guarda;
    - N-6: la cláusula de los intentos de corrección en C02.
  - Coherencia interna entre el §4 y D-T3C2-10.
- **Ninguno cambia el flujo ni provoca un efecto indebido.** Pueden corregirse sin otra re-revisión.

## Estado de los 13 hallazgos de la v1

| Hallazgo v1 | Estado en v2 | Referencia v2 / observación |
|---|---|---|
| I-1 (destino ambiguo) | **Resuelto** | D-T3C2-02 (destino, paso 2b y paso 8), fila C-15, adaptador de D-T3C2-03, INV-03, D-T3C2-10 y §1 (C-F4-03 y D-17). Queda un caso espejo, que no cubre: N-1 |
| I-2 (controles y sustitutos) | **Resuelto** | §5, `ExpectedEvidence` de los controles: firma exacta, se conserva cada salida ordinaria y una firma nueva vuelve al Owner. `ForbiddenWriteScope` con `docs/` y `.github/`. Faltan detalles: N-9 |
| M-1 (forma de la guarda) | **Resuelto, con reservas** | D-T3C2-08, INV-07 y D-T3C2-10 usan el mismo patrón de tres líneas. El patrón es sensible al formato y más estricto que F4-E35 (N-3), y no fija `_pilotEdit` antes del relevo (N-2) |
| M-2 (M-12 en el archivo) | **Resuelto** | D-T3C2-03, D-T3C2-10 e INV-02 |
| M-3 (ampliaciones de D-04, texto vivo, documentación del asesor, contextos) | **Resuelto** | D-T3C2-04, «Declarado» y contextos de `report`; INV-06. No hay un oráculo mecánico de «solo documentación»: N-8 |
| M-4 (fallo propio antes de delegar) | **Resuelto** | D-T3C2-04, `SessionDestroyed`; INV-06; prueba en D-T3C2-10 |
| M-5 (mitad del puente) | **Dispuesto, con reserva** | D-T3C2-08 la declara «no necesitada» porque «esa forma es el invariante mismo». Eso contradice la letra de F4-E35, que fija qué es el invariante: N-4 |
| M-6 (operaciones exactas) | **Resuelto** | D-T3C2-02, D-T3C2-04, D-T3C2-10 e INV-01 |
| M-7 (desviación de D-T3B2-04) | **Resuelto** | D-T3C2-06, con prueba de evidencia; el doble de UI ya tiene `Set`/`Remove` y `DiscardAsTheHostDoes`. El §4 no la lista: N-5 |
| M-8 (trampas de compilación y de guardas) | **Resuelto en parte** | G-21 a G-24. G-21 no cubre los archivos de prueba nuevos ni el Plugin: N-7 |
| M-9 (RED ambiguo) | **Resuelto** | §4 RED: cuerpo de `RackPanelDiscard`, `WorkspaceLazyInstance.Existing`, `Record.Exception` y `build-plugin` declarado |
| M-10 (conteos de F3) | **Resuelto** | D-T3C2-07: hecho medido, no guarda; D-T3C2-10: guarda solo por cuerpos |
| M-11 (precisiones) | **Resuelto** | D-T3C2-02 y D-T3C2-13: los cuatro puntos están declarados |

## Hallazgos nuevos

### N-1 (MENOR). C-02 afirma lo que no puede saber: es el caso espejo de I-1

- **Referencias:**
  - paquete: D-T3C2-02 (paso 2), T-1 C-02 y INV-03;
  - código:
    - `AutoCadEventBridge.cs`, `Start()` y el manejador de `DocumentActivated` (líneas 150-160);
    - `EventBridgeCore.OnDocumentActivated` y `OnDocumentToBeDestroyed`;
    - `WorkspaceHostCore.GetSnapshot` (223-241).
- **Qué pasa:**
  - Un comando sin flags se ejecuta siempre en un documento activo, y `Start()` activa ese documento.
  - Por eso `activeSession() == null` solo se da en estados anómalos. Ejemplo:
    1. se destruye el documento activo y `_active` pasa a null;
    2. falla, dentro de su contención, el `DocumentActivated` del documento A, que tiene borradores (por ejemplo, al leer `e.Document.Editor.IsQuiescent`);
    3. `_active` sigue en null mientras A es el documento activo.
  - Entonces el comando escribe C-02: «No hay un documento activo con borradores del panel Workspace». Es falso.
  - Es la misma clase de divergencia que I-1. El efecto es cero, pero el mensaje no es «claro» en el sentido de C-F4-03.
- **Corrección:**
  - Redactar C-02 de forma neutral, sin afirmar que no hay borradores. Por ejemplo: `RackCad: El panel Workspace no sigue ningún documento activo en este momento; no se descartó nada.`
  - O bien tratar ese caso como destino ambiguo (C-15) y declararlo en INV-03.

### N-2 (MENOR). La guarda de la instalación no fija que `_pilotEdit` exista antes de construir el relevo

- **Referencias:**
  - paquete: D-T3C2-05 («tras `_pilotEdit`»), D-T3C2-04 (los argumentos no anulables lanzan `ArgumentNullException`), y el patrón de D-T3C2-08 e INV-07;
  - código: `WorkspaceController.cs:39-56`.
- **El riesgo:**
  - C# no impide leer un campo `readonly` antes de asignarlo. Si `_closeAdvisor = new WorkspaceCloseAdvisorRelay(_host, _pilotEdit, …)` quedara antes de `_pilotEdit = new PilotEditAdapter(…)`:
    - el relevo recibiría null y lanzaría en el constructor del controlador;
    - RACKPANEL quedaría roto;
    - ninguna prueba lo vería, porque Core no carga el Plugin y la guarda es el único seguro. Solo lo detectaría Smoke-2.
  - El patrón de tres líneas no menciona `_pilotEdit`.
- **Corrección:** añadir `_pilotEdit = new PilotEditAdapter(` al patrón ordenado, entre la línea 1 y la 2, en D-T3C2-08, INV-07 y D-T3C2-10.
  - El literal que fija `WorkspacePilotEditGuardTests.cs:487` y la mutación de 523 siguen intactos, porque esa línea no cambia.

### N-3 (MENOR). El patrón exacto depende del formato y fija más de lo que manda F4-E35

- **Referencias:**
  - paquete: D-T3C2-08 («patrón exacto», «Esto protege el comportamiento, no una expresión»);
  - `WorkspaceLifecycleGuardTests.cs:43-53` (`AssertInOrder`) y 277-288;
  - `f4-dispositions.md` F4-E35: «fijar **solo** el orden `_host = new WorkspaceHostCore(` → `_bridge.SetLifecycleAdvisor(` y su conteo»;
  - autorización: «The guard must protect behavior, not one exact implementation expression».
- **Cómo funciona `AssertInOrder`:**
  - toma la primera aparición de cada aguja con `IndexOf` ordinal;
  - exige posiciones estrictamente crecientes;
  - no comprueba la unicidad, que la cubren los conteos aparte;
  - es sensible a los espacios y a los saltos de línea.
- **El problema:**
  - Las tres agujas son subcadenas literales: `_closeAdvisor = new WorkspaceCloseAdvisorRelay(_host,`.
  - El mismo constructor ya parte los argumentos tras `(` en `new WorkspaceHostCore(`. Si se da ese formato a la línea del relevo, de unos 170 caracteres, la guarda falla sin ningún cambio de comportamiento.
  - Además fija el nombre del campo y el argumento `(_closeAdvisor)`. Es otra «exact implementation expression», más estricta que el «solo el orden y su conteo» del Owner.
- **Corrección:**
  - Escribir las agujas como expresiones regulares tolerantes a espacios, por ejemplo:
    - `_closeAdvisor\s*=\s*new\s+WorkspaceCloseAdvisorRelay\s*\(\s*_host\s*,`;
    - `_bridge\.SetLifecycleAdvisor\s*\(\s*_closeAdvisor\s*\)\s*;`.
  - Justificar en D-T3C2-08 que cada elemento es el mínimo para probar «no second independent lifecycle authority»: el host primero, el relevo sobre el host y el asesor instalado igual a ese relevo. Así queda dicho por qué se va más allá del «solo» de F4-E35.
  - O bien declararlo como desviación de F4-E35.

### N-4 (MENOR). La disposición de la mitad del puente de M-1 contradice la letra de F4-E35

- **Referencias:**
  - paquete: D-T3C2-08 («La otra mitad de M-1 … no se necesita cambiar … Esa forma es el invariante mismo») y §8 M-5;
  - `f4-dispositions.md` F4-E35: «y que el reenvío llame a `_core.SetLifecycleAdvisor(`»;
  - autorización: «Carry and close where owned: … M-1 from T2B2B where explicitly assigned»;
  - `WorkspaceLifecycleGuardTests.cs:233-244`.
- **El problema:**
  - La mitad del puente se asignó de forma explícita, y el Owner ya dijo qué es el invariante: que el reenvío llame a `_core.SetLifecycleAdvisor(`.
  - La guarda actual exige igualdad exacta del cuerpo aplanado (`Assert.Equal("internalvoidSetLifecycleAdvisor(…)=>_core.SetLifecycleAdvisor(advisor);", …)`). Es justo la «exact implementation expression» que el Owner manda sustituir.
  - Decir que «la forma es el invariante» no se corresponde con F4-E35.
- **Corrección, a elegir:**
  - **Ejecutarla.** Tiene coste mínimo, porque el archivo ya está en el alcance: el cuerpo contiene exactamente una llamada a `_core.SetLifecycleAdvisor(advisor)`, el método sigue `internal` y sin `public`, y se conservan los conteos de `IWorkspaceLifecycleAdvisor`, `+=`, `-=` y manejadores. Se añade como cambio declarado en D-T3C2-08, §4 e INV-12.
  - **Registrarla como desviación declarada de F4-E35.** En ese caso, con el motivo verdadero: «no se toca porque T3C2 no toca el puente y el Owner pidió actualizar solo las guardas necesarias», no «es el invariante».

### N-5 (MENOR). El §4 no coincide con D-T3C2-10, y una frase de D-T3C2-10 se lee mal

- **Referencias:** paquete §4 («las filas C-01 a C-14…», «UI: los cambios declarados de 780-785 y 805; las pruebas nuevas de R-1 y R-2») y D-T3C2-10 («cada fila de T-1, C-15 incluida, con cero lecturas del riesgo»).
- **El problema:**
  - El §4 no incluye nada de esto:
    - C-15;
    - el conjunto exacto de operaciones;
    - las pruebas de contención del relevo (riesgo que lanza, `report` que lanza);
    - los contextos de `report`;
    - la prueba de evidencia de la desviación de D-T3C2-06, que en la UI tampoco aparece.
  - Quien planifique a partir del §4 se dejará pruebas.
  - Además, «cada fila … con cero lecturas del riesgo» parece decir que toda fila tiene cero lecturas. Eso es falso para C-05 a C-14 y para el C-15 del paso 8.
  - Tampoco se nombra la prueba del C-15 del paso 8: deja de autorizar después de «Sí», con cero `DiscardDocument`.
- **Corrección:**
  - En el §4, remitir a D-T3C2-10 como lista exhaustiva, o copiarla.
  - Reescribir así: «C-15 del paso 2b con cero llamadas a `GetRacksAtRisk` y a `DiscardDocument`; C-15 del paso 8 sin `DiscardDocument`».
  - Añadir que `Run` solo llama a `isTarget` con la sesión que acaba de dar `activeSession()` (un doble que lo registra). Así se fija la premisa del adaptador: «el tercer delegado solo se invoca con una sesión».

### N-6 (MENOR). C02 no tiene la cláusula de los intentos de corrección que sí tenía F4T3C1-C02

- **Referencias:**
  - paquete §5, C02: «un cambio de una guarda o prueba existente no declarado en el §4»;
  - `F4-T3C1-HOST-HARDENING/coordinator/gate-contract.json`, F4T3C1-C02: «in a correction attempt, files created by this task in an earlier attempt are not existing guards or tests».
- **El riesgo:** con `attempts` 1 de 3, un intento de corrección que tenga que tocar `WorkspaceCommandAdvisorTests.cs` o `WorkspaceCommandAdvisorGuardTests.cs` caería, por la letra, en un STOP falso.
- **Corrección:** copiar la cláusula en C02.

### N-7 (MENOR). G-21 no cubre los archivos de prueba nuevos ni las anotaciones del Plugin

- **Referencias:**
  - paquete G-21;
  - `eng/ci/verify-autocad-references.ps1:110-113 y 601-602`: la build Release de la **solución**, con las pruebas incluidas, falla ante cualquier `warning CS`;
  - `Directory.Build.props` (`Nullable` disable);
  - `WorkspaceController.cs` y `WorkspacePanelCommand.cs` no tienen `#nullable`.
- **El problema:**
  - Una anotación `?` de tipo referencia, sin `#nullable enable`, da CS8632:
    - en los dos archivos de prueba nuevos, por ejemplo `string?` o `Exception?`, que serán casi seguros en los dobles;
    - en el Plugin, por ejemplo `private string? DocumentNameOf(…)` o `WorkspaceController? controller`.
  - Ese aviso tumba la CI del GREEN.
  - El paquete escribe bien `private string DocumentNameOf`, pero no lo avisa.
- **Corrección:** ampliar G-21:
  - los dos archivos de prueba nuevos empiezan por `#nullable enable`, como sus vecinos;
  - en el controlador y en el comando no hay anotaciones `?` de tipo referencia (`?.` y `SessionId?` sí caben).

### N-8 (MENOR). Ningún oráculo mecánico comprueba «solo documentación» en `IWorkspaceLifecycleAdvisor.cs`

- **Referencias:**
  - paquete: D-T3C2-04 («Declarado»), §5 (`AllowedWriteScope`), C04 e INV-14;
  - `WorkspaceLifecycleGuardTests.cs:60-73`, cuyo comentario de F4-T3A M-2 dice que un miembro posterior no la dispara a propósito.
- **El problema:**
  - El archivo pasa de prohibido (contrato de T3C1) a permitido.
  - `Scope` solo mira que la ruta esté en la lista. G-24 fija las dos firmas, pero admite otro miembro, por ejemplo un método por defecto de la interfaz, que no obligaría a tocar el host.
  - C04 solo lo detectaría una revisión.
- **Corrección:** añadir una guarda en `WorkspaceCommandAdvisorGuardTests.cs` de que el código sin comentarios (`Src.Code`) de la interfaz declara exactamente esos dos miembros (dos sentencias en su cuerpo), con su autoprueba.

### N-9 (MENOR). Al `ForbiddenWriteScope` le falta lo que tenía el de T3C1

- **Referencias:** paquete §5 `ForbiddenWriteScope` y `F4-T3C1-HOST-HARDENING/coordinator/gate-contract.json`, que incluye `assets/`, `deploy/` y el prefijo `src/RackCad.Plugin/Views/`.
- **El problema:**
  - El resumen de la v2 no nombra `assets/` ni `deploy/`. `deploy/` contiene el manifiesto del paquete, relevante para el despliegue conjunto de Application y Plugin.
  - No dice qué pasa con `src/RackCad.Plugin/Views/`. Ese prefijo ya no puede ser prohibido, porque contiene `RackPilotIntentAdapter.cs`, que está permitido.
- **Corrección:**
  - añadir `assets/` y `deploy/`;
  - para `Views/`, enumerar sus demás archivos por ruta exacta, o dejarlos de forma explícita al `Scope`.

### N-10 (MENOR). La excepción de `GetKeywords` no tiene autopruebas, al contrario que sus precedentes

- **Referencias:**
  - paquete: D-T3C2-09 (fila de `WorkspaceBridgeSourceGuardTests.cs:159-163`) e INV-12 («las guardas nuevas tienen su autoprueba»);
  - precedentes: `WorkspaceSourceGuardExceptionTests.cs:63-104` y 140-213, una autoprueba por cada excepción por archivo exacto.
- **El problema:**
  - La rama nueva de `HandlerForbiddenCallsFor` cambia una guarda, no la crea, y el paquete no le exige autoprueba.
  - Sin ella, un error al construir la rama no lo detecta nada, por ejemplo un `ForbiddenCallsWithout` con más nombres, o comparar por `EndsWith`.
- **Corrección:** en `WorkspaceCommandAdvisorGuardTests.cs`, con `HandlerForbiddenCallsFor`, que es `internal static`:
  - para el archivo del comando, `ed.GetKeywords(o)` no coincide, pero `GetString`, `GetSelection`, `MessageBox`, `ShowDialog`, `GetObject` y `SelectImplied` sí coinciden;
  - para el controlador, el lector, los hechos, los efectos, el host y `…/WorkspacePanelCommand2.cs`, `GetKeywords` coincide.

### N-11 (MENOR). «Sin motivo» no está definido en C-12 a C-14

- **Referencias:**
  - paquete: T-1 C-12, C-13 y C-14 («sin motivo, …»), «C-11 a C-14 reproducen … `WhyOf`», y G-21 (las pruebas pasan `reason!` o `" "`);
  - código: `WorkspaceBrowserViewModel.cs:937-948` (`WhyOf` usa `IsNullOrWhiteSpace`).
- **El problema:**
  - Si el núcleo trata «sin motivo» como solo null, una prueba con `" "` produce `…documento:  ` en lugar del texto de reserva.
  - El Worker escribe las dos partes, pero el contrato debe decir cuál es la correcta.
- **Corrección:**
  - «sin motivo = null o en blanco (`string.IsNullOrWhiteSpace`), como `WhyOf`»;
  - lo mismo para `ex.Message` en C-14.

## Lo comprobado y correcto

**El adaptador `isTarget` (D-T3C2-03):**

- **Compila.** Los tipos encajan:
  - `WorkspaceRequest(SessionId Origin)` es un `record struct` y `RequestDecision.Accepted` existe (`SessionId.cs`);
  - `IWorkspaceHostPort.Authorize(WorkspaceRequest)` es público;
  - el lambda se convierte a `Func<SessionId, bool>`;
  - `() => controller?.Port.GetSnapshot().ActiveSession` da `SessionId?`.
- **No genera avisos de nulabilidad,** porque el Plugin la tiene desactivada.
- **Hacen falta dos `using`:** `RackCad.Application.Workspace` y `Autodesk.AutoCAD.EditorInput`. Ninguna guarda los prohíbe en el archivo del comando:
  - `NoExistingPluginCommandFileReferencesTheWorkspaceBridge` excluye la carpeta Workspace;
  - el alias `AcApplication` evita la ambigüedad con el espacio `RackCad.Application`.
- **El lambda nunca ve `controller` a null:**
  - `controller` es una variable local que no se reasigna;
  - `activeSession` solo da una sesión si `controller` existe, y el flujo llama a `isTarget` solo después.
  - Falta fijar esa premisa en una prueba del núcleo: N-5.
- **Ninguna guarda del archivo del comando se rompe:**
  - `WorkspaceNavigationHostTests.cs:609-622` prohíbe `WorkspaceNavigation|NavigationEcho|IWorkspaceNavigationPort` en ese archivo. `PilotEdit`, `Port`, `Authorize` y `GetSnapshot` no coinciden.
  - `WorkspaceHostSourceGuardTests.cs:65-73` (`GetObject`, `LockDocument`, `AutoCadEventBridge`…), 84-89 y 431-436 (`\.Result\b`): `.StringResult` y `result.Status` no coinciden.
  - `WorkspacePaletteHostSourceGuardTests.cs:311-318`: «no muestra», «aun no»…
  - `PublicSurfaceLeaks` y `ForbiddenFieldLines` (`WorkspaceBridgeSourceGuardTests.cs:205-252`): los métodos `private static` y las variables locales de tipo `Editor` o `Document` no coinciden.
  - Las guardas de la paleta que prohíben `Authorize` y `GetSnapshot` (194 y 219) solo miran `WorkspacePaletteHost.cs`.
- **La semántica es correcta:**
  - `Authorize` del host compara con `MdiActiveDocument` a través de `HostProbe`, y `DiscardDocument` usa la misma comprobación.
  - Un comando sin flags se ejecuta en el documento activo, y `AutoCadEventBridge.Start()` y `DocumentActivated` registran ese documento por el mismo `UnmanagedObject.ToInt64()`.
  - En el camino normal, la sesión legítima siempre autoriza. Solo falla con una divergencia contenida o si el documento cambia durante la pregunta, y esos son precisamente los casos C-15 y C-08, con cero efecto.

**El patrón de D-T3C2-08, frente a las guardas del constructor:**

- Es compatible con `WorkspacePilotEditGuardTests.cs:487`, una expresión regular sobre todo el archivo que no depende del orden.
- La mutación de 523 sigue encontrando su literal.
- Siguen en pie el conteo de 3 `+=` (cinco guardas), el único `\bevent\b`, `DoesNotContain(".Start(")`, `OpenPanel` y `RequestReconciliation` en el constructor.
- `Src.BodyOf` del constructor no confunde la flecha del lambda `id => …`, porque la `{` del cuerpo va antes.
- El orden `_pilotEdit` → `_closeAdvisor` → `SetLifecycleAdvisor` no choca con ninguna guarda. La única debilidad es la de N-2.

**`IWorkspaceLifecycleAdvisor.cs`:**

- Ninguna guarda lee su texto con comentarios para prohibir palabras. `Src.Text` solo se usa para `#nullable enable` (línea 63).
- Las demás guardas (`TheFilesOfTheLifecycleWriteNothing…` y `ModelDoesNotPersist…`) usan el código sin comentarios, del que `CodeOnly` quita `///`.

**El relevo y los nombres:**

- `OnActiveSessionChanged` sigue sin `_bridge.` ni `DrainNotice`, con `NotifyStateChanged()` después de la guarda (434-448).
- `TakeMessages` no coincide con `\bTake(?:Close|Loss)Notices\b`, que se barre en todo `src/RackCad.Plugin` y `src/RackCad.Application` (`WorkspacePilotCloseSyncGuardTests.cs:175-197`).
- Los nombres de tipo nuevos no chocan con `Tab|Draft|Editor|Subscription|Log|Metric…`.
- Los textos T-1 y T-2 que viven en Application no contienen `Transaction|LockDocument|\bDatabase\b|\bCommit\(`, ni las palabras de I-63, ni `Timer`, `File.` o `Dispatcher`.

**Los censos y las excepciones:**

- Con el refinamiento «exactamente un RACKPANEL y a lo sumo un RACKPANELDESCARTAR», las 10 autopruebas de `WorkspaceSourceGuardExceptionTests.cs:37-46` siguen dando verdadero, y la de 33 sigue dando falso.
- El conteo de `string.Equals(path, (Reader|Command)AdapterPath, …)` pasa de 2 a 3. `(Facts|Effects)` sigue en 2.
- La ayuda (`CustomPropertiesHelpCensusTests`) exige solo que lo que la ayuda nombra exista, así que el comando nuevo no la rompe.
- `PushBackRoundTripSourceGuardTests` exige unicidad, que se cumple.

**Las cuentas y el contrato:**

- **Rutas:** 20 en total y 4 nuevas. C01 dice «cuatro» y coincide.
- **Líneas base:** 2517 y 454 coinciden con `ci-filter-counts.json` de T3C1.
- **`ExpectedEvidence`:** las firmas de PD-F4-01, 04, 05 y 06 coinciden con F4-E43, las órdenes de T3B1 y la autorización de T3C2.
- **Los textos y las pruebas de UI:**
  - D-T3C2-06 conserva 783-784, que distinguen mayúsculas.
  - C-11 coincide pieza a pieza con `DocumentDiscardedText` (1069-1080).
  - Los 10 literales de T-3 y sus conteos residuales son exactos.
  - Ninguna prueba fija los literales en inglés del Plugin: las apariciones en pruebas son de dobles de F3.
