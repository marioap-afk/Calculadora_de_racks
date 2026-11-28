# Revisión directa de la sesión: F4-T3C2-COMMAND-ADVISOR, intento 1

- **Rango:** `3b1ee950..4e34bb56`.
- **Commits:** RED `730dd7da` y GREEN `4e34bb56`.
- **Worker:** `R20261113T203722Z-54b1`, con 264 turnos de `claude-sonnet-5-5`, esfuerzo high y 0 denegaciones.

**Veredicto: producto conforme.** Sin hallazgos propios. La revisión independiente es aparte.

## Comprobado

### Alcance
- Son exactamente las 20 rutas del contrato. Cuatro son nuevas: `WorkspaceDocumentDiscardCommand.cs`, `WorkspaceCloseAdvisorRelay.cs`, `WorkspaceCommandAdvisorTests.cs` y `WorkspaceCommandAdvisorGuardTests.cs`.
- El GREEN no toca `tests/` (`git diff 730dd7da 4e34bb56` solo cambia 10 archivos de `src/`).
- No cambian `EventBridgeCore`, `AutoCadEventBridge`, `WorkspaceHostCore`, la compuerta, `BrowserSession`, el XAML ni los VM del campo y del panel.
- `IWorkspaceLifecycleAdvisor.cs` e `IWorkspacePilotEditPort.cs` solo cambian su documentación.
- No hay ningún archivo nuevo del Plugin.

### RED (CI `43508172794`)
- Core y UI terminan en failure. El build de la UI, en success; el del Plugin no corre (`needs: tests`), como declara el paquete.
- Fallan 76 pruebas del filtro de Core y 3 del de UI, y ninguna fuera del filtro.
- Todas son métodos nuevos o cambiados del RED: 103 en total.
- Los censos de fuera del filtro pasan: `CustomPropertiesCommandGuardTests` y `SelectiveEditorOpenTests` (el Worker midió 81/81).

### GREEN (CI `43510803096`)
- Los cuatro jobs terminan en success.
- Filtros: 2668/2668 en Core y 458/458 en UI.
- Suites completas:
  - Core: 18206/18206;
  - UI: 2095 pasan, 0 fallan y 17 no se ejecutan, que son las condicionales ya existentes.

### D-T3C2-01
- `WorkspaceLazyInstance.Existing` devuelve la instancia bajo el mismo `lock`, sin la fábrica y sin tocar `CreateCount`.
- `WorkspaceController.Existing => Holder.Existing;`.

### D-T3C2-02. El núcleo del comando
- `Run(port, activeSession, isTarget, confirm, write)` sigue los pasos 1 a 11 y el 2b.
- `isTarget` solo recibe la sesión de `activeSession()`.
- Los 15 textos de T-1 son exactos, con C-02 neutral (v3) y «sin motivo» = `IsNullOrWhiteSpace`.
- La TOCTOU compara las dos listas en orden y por ordinal.
- Una lectura que lanza cuenta como el desconocido. Solo llama a `GetRacksAtRisk` y a `DiscardDocument`.

### D-T3C2-03. El adaptador
- `[CommandMethod("RACKPANELDESCARTAR")] RackPanelDiscard` no lleva flags. Usa `WorkspaceController.Existing`, `controller?.PilotEdit`, `controller?.Port.GetSnapshot().ActiveSession` y `Port.Authorize(...) == RequestDecision.Accepted`.
- Ni `GetOrCreate`, ni `OpenPanel`, ni `Show`.
- `ConfirmOnCommandLine` usa `Si`/`No`, `Default = "No"` y `AllowNone`, y da verdadero solo con `PromptStatus.OK` y `"Si"` (ordinal). Lee `.Status` y `.StringResult`.
- `WriteToCommandLine` es `internal static` y es el único escritor.
- Sin anotaciones `?` de tipo referencia y sin subdiálogo.

### D-T3C2-04. El envoltorio
- **`AdviseClose`:** parte de Veto, da Allow solo con el Allow de `inner`, y una excepción de `inner` es Veto. El aviso V-01/V-02 va contenido, con el nombre acortado por cadena y «el documento» como reserva.
- **`SessionDestroyed`:**
  - `RememberLoss`, con todo su trabajo contenido, va antes de `_inner.SessionDestroyed`, que se llama siempre y sin contener;
  - lee el riesgo con `GetRacksAtRisk` y no consulta el puente.
- P-01/P-02/P-03 son exactos.
- `TakeMessages(null)` conserva; con una sesión, vacía bajo `lock`.
- Los contextos de `report` son exactos, y un `report` que lanza se ignora.

### D-T3C2-05. El controlador
- La instalación sigue el orden `_host`, `_pilotEdit`, `_closeAdvisor = new WorkspaceCloseAdvisorRelay(_host, _pilotEdit, DocumentNameOf, id => _bridge.ActiveSession == id, WorkspacePanelCommand.WriteToCommandLine, LogFault)` y `_bridge.SetLifecycleAdvisor(_closeAdvisor);`.
- Siguen 3 `+=`.
- `WriteCloseMessages()` va después del `try`/`catch` del refresco y antes de `NotifyStateChanged()`. Está contenido y usa `TakeMessages(_bridge.ActiveSession)` fuera del cuerpo del relevo.
- `DocumentNameOf` recorre `DocumentManager` en memoria con una variable local, sin campos de tipo `Document`.
- Ningún `WriteMessage` en el controlador.

### D-T3C2-06. El aviso del panel
- Los dos textos de las filas de veto son exactos y no nombran ninguna vía de aplicar.
- La prueba de evidencia de la desviación declarada de D-T3B2-04 está.

### D-T3C2-07. Los motivos del Plugin
- Los 10 literales de T-3 son exactos y están en su sitio (3 en `WorkspaceNavigationHost.cs` y 7 en `RackPilotIntentAdapter.cs`).
- Quedan intactos `ex.Message`, los de F3, `ReadPreviewWitness` y el `Debug.Assert`.

### D-T3C2-08/09. Las guardas cambiadas
Cada hunk de las pruebas existentes corresponde a un cambio declarado:
- **La instalación:** `AssertMatchesInOrder` con las cuatro expresiones tolerantes a espacios, y un `new WorkspaceHostCore(` y un `new WorkspaceCloseAdvisorRelay(`.
- **El reenvío del puente:** exactamente una `_core.SetLifecycleAdvisor(advisor)`, una sentencia y una sola llamada.
- **Los censos por nombre:** `{RACKPANEL, RACKPANELDESCARTAR}` y 2 atributos, en `WorkspaceHostSourceGuardTests.cs` (sin cambiar el número de pruebas, 45) y en `WorkspacePaletteHostSourceGuardTests.cs`.
- **`GetKeywords`:** una rama por ruta exacta con `ForbiddenCallsWithout("GetKeywords")`.
- **`DeclaresEntryPoint`:** exactamente un RACKPANEL y a lo sumo un RACKPANELDESCARTAR, con sus autopruebas (la pareja da falso; un duplicado, con flags o un nombre parecido dan verdadero).
- **Las comparaciones por ruta exacta:** 3.
- **Los censos globales:** `F4PanelDiscardCommands`, y de 39 a 40 con su párrafo.
- **La UI:** solo 780-782, 785 (`Contains`) y 805.
- **Ninguna prohibición se debilita.**

### D-T3C2-11. Los menores de F4-T3C1
- R-1 y R-2 tienen pruebas de texto exacto.
- R-4 cambia solo la documentación del puerto y del VM.

## Notas (sin hallazgo)
- El controlador gana `using Autodesk.AutoCAD.ApplicationServices;` para el `foreach (Document …)`. La CI del GREEN compila el Plugin en Release sin avisos, y ninguna guarda lo prohíbe en el controlador.
- El Worker declara 1 error de herramienta y ninguna desviación.
