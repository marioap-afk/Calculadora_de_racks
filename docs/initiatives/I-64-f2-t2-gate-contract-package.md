# I-64 — Paquete del contrato de gate de F2-T2-NAVIGATION-HOST-UI

```text
Estado: PAQUETE PARA REVISIÓN — NO EMITIDO
Emisor del contrato: Coordinator (AUTOMATION_PLAN 16.5; agent-execution/README §1)
Preparado por: sesión responsable de I-64 (Claude), por el cierre final de F2-T1 (coordinator-f2t1-final-close.md, evidencia §88)
Rama al preparar: architecture/workspace-persistente-rackcad en bbc3a654 (= remoto, limpia), sobre origin/main a140eb3a (sin movimiento)
config.toml: SNAPSHOT D aceptada (104 nombres)
STOP: COORDINATOR_F2_T2_CONTRACT_REVIEW_REQUIRED (decisiones C-F2T2-01..13)
```

Este documento no es un `gate-contract.json`, no emite delegaciones y no autoriza trabajo. Deriva el contrato de las autoridades sin
añadir alcance: el Freeze V5, el cierre de F2-T1, el estado, el código en `bbc3a654` (producto `c726410f`) y las autoridades de `main` en
`a140eb3a`. Si una pieza del Freeze o de la orden choca con el código actual, el choque se eleva como decisión y no se resuelve aquí.

**No se ha invocado ninguna planificación, y por eso no se han ejecutado A1-A8 ni nc4.** La orden pide «contrato + delegación + paquete de
aceptación», con A1-A8 y nc4 antes del Worker. Pero:
- A1-A8 evalúan una `delegation.json` cuyo `RunId` es el de una planificación del Controller `COMPLETED` (README §4, A2), y nc4 es una
  copia de esa delegación (README §10). Sin planificación no hay nada que aceptar.
- La planificación exige un contrato utilizable, y este paquete encuentra dos bloqueos previos:
  - **Enrutamiento:** F2-T2 monolítica da Horizonte `High`, es decir **Long-horizon**, y no hay ninguna celda Frontera con escritura
    medida. Según `routing.md` §4, decide el Coordinator (§ 5).
  - **Choques materiales:** las guardas congeladas de F1 y F2-T1 prohíben justo lo que F2-T2 necesita; el diseño exige decisiones
    reservadas (S-03).
- Una planificación ahora daría una delegación fuera de contrato o un bloqueo de planificación, y consumiría presupuesto.
- **Propuesta (C-F2T2-13):** cuando el Coordinator decida, emitir el contrato y ejecutar relevo → planificación → A1-A8 → nc4, con la
  parada que el Coordinator fije.

## 1. Fuentes

| Fuente | Qué aporta |
|---|---|
| Cierre final de F2-T1 (`coordinator-f2t1-final-close.md`) | F2-T2: Workspace UI, acciones de host SELECT/VIEW/CENTER/LOCATE VIEW, foco, seguridad de documento y sesión, supresión de eco; D-05/D-07 puntos 1-12; Workspace no modal, acoplable, flotante, redimensionable o en otro monitor; exclusiones de F3/F4 y posteriores |
| Freeze V5 (`I-64-proposal-v5.md` en `9b43dafb`, blob `dc1924ff`) D-01, D-02, D-03, D-04, D-05, D-07 | host y foco (incluida la API interna aislada con degradación, O-10); capas, puerto y contexto de comando; sesiones; puente y drenaje; `NavigationTargets`; acciones, eco, `UNKNOWN` de la selección, revalidación |
| Freeze V5 §10, §11, §12.3 y §13 | INV-03 completo, INV-14, INV-22, INV-36 (guarda), INV-38 (contexto), INV-40; «pruebas de UI de las acciones»; Smoke-NAV SN-01..10; clase de F2 |
| Producto de F2-T1 (`f2761979` RED, `c726410f` GREEN) | `NavigationTargets`, `NavigationActionPolicy`, `INavigationFactsPort`, `INavigationEffects` |
| Código en `bbc3a654` | guardas de texto de F1/F2-T1, host de la paleta, controlador, puente, lector, UI del panel, proyectos de prueba, CI |
| Autoridades de `main` `a140eb3a` | `routing.md` §§1-5, `model-catalog.md`, `gate-contract.schema.json`, AUTOMATION_PLAN 16.3-16.8, README §4 y §10 |
| Paquetes de F1-T2, F1-T3 y F1-T3B | forma del paquete; precedente de división de una tarea Long-horizon de host + UI (F1-T3 → T3A + T3B) |

## 2. Comprobaciones previas pedidas por la orden

| Comprobación | Resultado |
|---|---|
| `main` actual | `git fetch`: `origin/main` = `a140eb3a` (merge de I-65), sin movimiento desde la apertura de F2 |
| rama actual de I-64 | `architecture/workspace-persistente-rackcad` en `bbc3a654` = `ls-remote`; árbol limpio; sin operación Git en curso |
| `config.toml` | SNAPSHOT D aceptada (`cfg_check_d`, 104 nombres, sin valores) |
| CLI de Codex | un único `codex.exe` (`bin/f71a721e0e6193a9`), SHA-256 `4B3286E3…`, el acreditado |
| coordinación | `git fetch` y diff contra `main` de las ramas remotas (I-62 portabilidad, I-52 RACKMIRROR, arreglo de `placedclone`): ninguna toca `*/Workspace/`, `RackBlockFinder.cs` ni `RackInventarioCommands.cs` |
| línea base de pruebas en `bbc3a654` (Release, local) | Core `FullyQualifiedName~RackCad.Tests.Workspace` = **656**; Core `~Workspace` = 724; UI `FullyQualifiedName~RackCad.UI.Tests.Workspace` = **64**; UI `~Workspace` = 66; todas en verde |

## 3. Objetivo de F2-T2 (orden y Freeze)

Conectar el núcleo de navegación verificado de F2-T1 con el host AutoCAD y con la UI del Workspace, sin crear una segunda autoridad:
- **acciones** SELECT, VIEW, CENTER y LOCATE VIEW sobre el rack del contexto `Uno(R)`, con objetivos solo de `NavigationTargets`
  (`NavigationActionPolicy.Plan/Decide/Execute`);
- **contexto de comando** para ejecutar, con revalidación en ese contexto (INV-40) y rechazo por sesión (D-03, INV-22); fallo = cero acción,
  invalidación de la caché y refresco;
- **SELECT:** selección implícita de todas las referencias de Model Space, copias incluidas. Fuera de Model Space no está disponible, se
  explica y no se cambia `TILEMODE`;
- **VIEW:** extensión de la referencia principal con margen del 10 %, como `ZoomToRack`, con «copia k/M»;
- **CENTER:** cambia el centro sin cambiar la escala;
- **LOCATE VIEW:** la primera referencia de una vista concreta;
- **sin escritura:** ni del authored ni de layouts o `TILEMODE`, y sin otras mutaciones de vista;
- **foco** al dibujo al terminar cada acción;
- **eco por contenido**, sin atajo del «siguiente evento» (INV-03 completo);
- **dirección de producto:** Workspace no modal; nada en la UI de navegación puede suponer una barra lateral estrecha anclada.

## 4. Hallazgos del código que condicionan el contrato (H-01..H-14)

| ID | Hallazgo (en `bbc3a654`) | Consecuencia |
|---|---|---|
| H-01 | F2-T1 ya define los puertos que F2-T2 implementa: `INavigationFactsPort.ReadDefinitionFacts(SessionId)` (`NavigationFacts.cs`) e `INavigationEffects` {`SelectReferences`, `ViewReference`, `CenterOnReference`} (`NavigationActions.cs`); LOCATE VIEW se resuelve en `Plan` como VIEW o CENTER (`locateWith`) | F2-T2 no cambia esos tipos; los implementa en el Plugin |
| H-02 | Nada en `src/` construye `NavigationTargets` ni `NavigationActionPolicy`, ni llama a `ApplyDrain` o `EndSession` | F2-T2 los instancia y conecta el drenaje |
| H-03 | `WorkspaceBridgeSourceGuardTests.TheBridgeNeverWritesSelectionOrViewNorReadsStoredDataNorAsksNorOpensDialogs` prohíbe en **todo** `Plugin/Workspace/` `SetImpliedSelection`, `SetCurrentView`, `Zoom*`, `GetObject`, `SelectImplied`, `OpenCloseTransaction`, entre otras; solo el lector es excepción, por archivo exacto | el ejecutor de efectos y el lector de hechos necesitan excepciones por archivo exacto |
| H-04 | `WorkspaceHostSourceGuardTests.NoHostFileWritesTheSelectionTheViewOrAnySystemVariable` repite la prohibición en `Plugin/Workspace/` y `Application/Workspace/`; `TheReaderApisAreUsedInNoOtherFileOfThePluginWorkspace` prohíbe `GetObject`, `RackBlockData`, `RackEmbedStore` y `SelectImplied` fuera del lector | ídem |
| H-05 | `NoHostFileStartsABackgroundThreadOrAsyncWork` prohíbe `async`, `await`, `Task.Run`, `Task.Factory`, `Timer`; `[CommandMethod]` solo existe para `RACKPANEL`; `SendStringToExecute` está prohibido | **no existe vía para el contexto de comando** que exige D-07 (C-F2T2-02) |
| H-06 | `WorkspacePaletteHostSourceGuardTests.TheFirstPlacementAddsNoPersistenceIdentityAutoloadProfileOrSecurityMechanism` congela `Plugin/Workspace/` en seis archivos; `TheRefinementsG1G2AndG3AreExact…` fija 43 `[Fact]/[Theory]` en `WorkspaceHostSourceGuardTests.cs` | cada archivo nuevo del Plugin y cada guarda nueva exige un delta exacto de estas metaguardas |
| H-07 | `WorkspaceNavigationAuthorityTests.NoEventSideFileCanReachTheNavigationEffectsOrThePolicy`: ningún archivo nombra a la vez tipos del lado de efectos (`INavigationEffects`, `NavigationActionPolicy`, `NavigationActionPlan`) y del lado de eventos (`IWorkspaceBridgeLink`, `IHostEventSource`, `EventBridgeCore`, `HintKind`, `EnqueueHint`) | la política no puede vivir en `WorkspaceController.cs` ni en `AutoCadEventBridge.cs`: necesita un archivo propio |
| H-08 | `OnlyTheAuthorityTraversesTheReferenceHandlesOfTheFacts`: solo `NavigationFacts.cs` y `NavigationTargets.cs` nombran `ModelSpaceReferenceHandles` | el adaptador de hechos necesita una excepción exacta (o construir el registro por posición) |
| H-09 | Ningún escaneo integrado da los handles de las referencias de Model Space: `RackBlockFinder.ScanEnvelopes` (Plugin, `internal`) solo da el recuento; `RackSiblingScan.CaptureDrawing` tampoco | F2-T2 necesita una lectura nueva de handles; D-05 pide consumir las autoridades integradas sin cambios (C-F2T2-04) |
| H-10 | `IWorkspaceHostPort` está fijado por `WorkspaceHostPortGuardTests` como solo lectura (nombres `Get`/`Authorize`/`Is`/`Can`/`Try`, un único `WorkspaceRequest`); `WorkspaceHostPort.cs` tiene exactamente 4 llamadas `_core.` | las acciones de navegación necesitan un puerto propio; el de F1 no cambia (C-F2T2-05) |
| H-11 | El puente no tiene ningún aviso que nombre una `SessionId` destruida; su superficie pública está fijada (`TheBridgePublicSurfaceIsTheT2OneAndTheFourFacadesAndNothingElse`) | `EndSession` necesita un mecanismo (C-F2T2-07) |
| H-12 | No existe supresión de eco: solo `WorkspaceSession.ApplyContext` ignora un contexto igual | INV-03 completo exige el eco por contenido (C-F2T2-06) |
| H-13 | Foco: solo `KeepFocus = true` en el host de la paleta; `SetFocusToDwgView`, `SetFocus`, `Keyboard.`, `DllImport` y `HwndSource` están prohibidos; F1-T3B (INV-F1-T3B-12) prohibió además `Autodesk.AutoCAD.Internal` | el foco al dibujo exige decidir el mecanismo (C-F2T2-03) |
| H-14 | UI: el constructor del view-model está fijado por regex a `(IWorkspaceHostPort, Action<bool>)`; la vista no usa anchos fijos salvo columnas de la tabla de medidas; los supuestos de tamaño están en el host (`InitialWidth 320`, `MinimumWidth 240`, `DockEnabled Left|Right`, acoplada a la derecha) | la UI de navegación necesita un delta de esa guarda y no debe añadir supuestos de barra estrecha (C-F2T2-10) |

Ningún proyecto de prueba referencia el Plugin: su comportamiento real solo se comprueba en Smoke-NAV; en CI, el Plugin solo se compila
(`Build Plugin without AutoCAD`, AutoCAD.NET 25.0.1). Las pruebas del Worker son de Core (puro y guardas de texto) y de UI.

## 5. Enrutamiento (`routing.md` §§1-5 en `a140eb3a`)

- **Clase:** «Implementación transversal a capas» (Freeze §13, fila F2).
- **Dimensiones de F2-T2 monolítica:**

| Dimensión | Valor |
|---|---|
| ambigüedad | High (H-05, H-09, H-11, H-12, H-13) |
| sensibilidad arquitectónica | High |
| amplitud | High (Application, Plugin, UI, Core, UI.Tests y unas seis guardas existentes) |
| uso de herramientas | High |
| coste de fallo | High |
| repetición mecánica | Low |
| horizonte | **High** |

- **Resultado:** Horizonte `High` fija **Long-horizon** (§2.3), con nivel Frontera (§3).
- **Celdas:** `model-catalog.md` no tiene ninguna celda Frontera con `write-commit-push` medida (`claude-opus-5-5` y `claude-fable-5-1`:
  «no medido»). Sin celda elegible, decide el Coordinator (§4).
- **Propuesta (C-F2T2-01), con el precedente de F1-T3:** dos tareas cortas y secuenciales dentro del mismo gate F2.
  - **F2-T2A-NAVIGATION-HOST:** puertos y adaptadores del Plugin, contexto de comando, eco, `EndSession`, foco y deltas de guardas; pruebas
    de Core.
  - **F2-T2B-NAVIGATION-UI:** controles de navegación del panel; pruebas de UI. Su contrato se emite tras T2A EXECUTION_VERIFIED, como
    F2-T2 tras F2-T1.
  - Cada una: «Implementación transversal a capas (corta)», Deep, con las celdas de F2-T1: Worker `claude-sonnet-5-5`/subagente/`high`
    (medida en U-04; Worker real en F1-T2, F1-T3A, F1-T3B y F2-T1) y Controller `gpt-6-luna`/Codex CLI read-only/`high` (acreditada en este
    CLI por la sonda `3b8b`). `RoutingEnforcement = required`.

## 6. Alcance de escritura propuesto

Toda excepción de guarda es **por archivo exacto, nunca por carpeta**, y queda fijada por una metaprueba en
`WorkspaceSourceGuardExceptionTests.cs` (como las dos existentes).

**F2-T2A-NAVIGATION-HOST** (nombres de archivo propuestos; el Coordinator puede cambiarlos):

| Ruta | Tipo | Contenido |
|---|---|---|
| `src/RackCad.Application/Workspace/IWorkspaceNavigationPort.cs` | nuevo | puerto de peticiones de navegación (C-F2T2-05): solo valores y `WorkspaceRequest`; ningún tipo de AutoCAD (INV-14) |
| `src/RackCad.Application/Workspace/NavigationEcho.cs` | nuevo | memoria pura de eco por contenido, por sesión (C-F2T2-06) |
| `src/RackCad.Plugin/Workspace/WorkspaceNavigationHost.cs` | nuevo | dueño de `NavigationTargets` y `NavigationActionPolicy`; implementa el puerto; contexto de comando (C-F2T2-02); foco (C-F2T2-03); purga de sesiones (C-F2T2-07) |
| `src/RackCad.Plugin/Workspace/WorkspaceNavigationFacts.cs` | nuevo | `INavigationFactsPort` sobre `RackBlockFinder.ScanEnvelopes` + handles de Model Space (C-F2T2-04) |
| `src/RackCad.Plugin/Workspace/WorkspaceNavigationEffects.cs` | nuevo | `INavigationEffects`: `SetImpliedSelection`, vista con margen, centrado sin escala |
| `src/RackCad.Plugin/Workspace/WorkspaceController.cs` | delta exacto | crea el host de navegación de forma perezosa, le pasa el resultado del drenaje y lo expone; sin suscripción nueva a eventos de AutoCAD |
| `src/RackCad.Plugin/Workspace/WorkspaceSelectionReader.cs` y `src/RackCad.Application/Workspace/WorkspaceHostCore.cs` | delta exacto (C-F2T2-06) | conjunto de handles de la selección implícita para el eco y su aplicación en el drenaje |
| `tests/RackCad.Tests/Workspace/` (archivos nuevos `WorkspaceNavigationHost*Tests.cs`, `WorkspaceNavigationEcho*Tests.cs`) | nuevos | eco, puerto, cableado, purga, guardas de los archivos nuevos |
| guardas existentes en `tests/RackCad.Tests/Workspace/` | delta exacto | `WorkspaceBridgeSourceGuardTests.cs`, `WorkspaceHostSourceGuardTests.cs`, `WorkspacePaletteHostSourceGuardTests.cs` (seis → nueve archivos; 43 → nuevo recuento), `WorkspaceNavigationAuthorityTests.cs` (H-08), `WorkspaceSourceGuardExceptionTests.cs`; si el controlador cambia, la guarda de recuento de `+=` de `WorkspaceHostC2SourceGuardTests.cs` |

**F2-T2B-NAVIGATION-UI** (contrato posterior): `src/RackCad.UI/Workspace/` (vista y view-model de navigación),
`src/RackCad.Plugin/Workspace/WorkspacePaletteHost.cs` (delta exacto: construir el view-model con el puerto de navigación),
`tests/RackCad.UI.Tests/Workspace/` (nuevos) y la guarda del constructor del view-model (H-14).

**Prohibido en las dos:** `src/RackCad.Domain/`, `src/RackCad.Application/StructuralAnalysis/` y el resto de `src/RackCad.Application/`
fuera de `Workspace/`. También:
- los tipos de F2-T1 (`NavigationFacts.cs`, `NavigationTargetSet.cs`, `NavigationTargets.cs`, `NavigationActions.cs`);
- `RackBlockFinder.cs`, `RackInventarioCommands.cs` y todo `src/RackCad.Plugin/` fuera de `Workspace/`;
- `EventBridgeCore.cs`, `AutoCadEventBridge.cs`, `WorkspaceHostPort.cs`, `IWorkspaceHostPort.cs` y `WorkspacePanelCommand.cs`;
- `docs/`, `.github/`, `eng/`, `tools/`, `assets/`, `deploy/` y los archivos de solución y de build.

## 7. Invariantes (borrador)

1. **D-05:** `NavigationTargets` es la única autoridad de objetivos; el Plugin y la UI no reconstruyen el descubrimiento de navegación.
2. **D-07, INV-40:** antes de actuar, ya en contexto de comando, se revalidan documento, sesión, existencia y atribución
   (`NavigationActionPolicy.Decide`). Si algo falla: cero llamadas de efectos, invalidación de la caché del rack y refresco.
3. **D-03, INV-22:** cada petición lleva su `SessionId`; una petición de A ejecutada con B activo se rechaza sin efectos.
4. **Sin escritura:** ni authored, ni Xrecord, ni XData, ni diccionarios, ni archivos; sin `SetSystemVariable`; `TILEMODE` y layouts no
   cambian. Fuera de Model Space, SELECT no está disponible y se explica.
5. **VIEW:** extensión de la referencia principal con 10 % de margen, como `ZoomToRack`; «copia k/M» con el `copyIndex` de la política.
   **CENTER:** solo cambia el centro, no la escala. **LOCATE VIEW:** la vista pedida, por `Plan(... view, locateWith)`.
6. **INV-14:** transacciones y bloqueos solo dentro de `using`; ningún objeto de AutoCAD en campos; el puerto de navegación sin tipos de
   AutoCAD.
7. **INV-03 completo:** ningún evento ni cambio de contexto escribe selección o vista. El eco se reconoce por contenido: mientras la
   selección implícita sea igual, como conjunto, a la escrita por la acción, el contexto, la lista y la pestaña no cambian. La primera
   selección distinta reanuda la sincronización. No se usa el «siguiente evento».
8. **D-01:** el foco vuelve al dibujo al terminar cada acción, con el mecanismo que fije C-F2T2-03; nunca al terminar una edición.
9. **D-04:** sin suscripciones nuevas a eventos de AutoCAD fuera del puente; los manejadores siguen sin escribir.
10. **INV-36:** los tipos de navegación siguen privados del panel; la guarda de F2-T1 sigue en verde.
11. **Sin trabajo en segundo plano:** ni hilos, ni `Task.Run`, ni temporizadores; la única excepción posible es la de C-F2T2-02.
12. **Comandos:** RACKEDITAR, RACKPANEL, RACKRAMPAQUETE y los comandos clásicos sin cambio; censos de comandos y ventanas sin cambio.
13. **Fuera de alcance:** F3, F4, F6, migración de editores, rediseño general, sistemas nuevos, inventario global y UX final de doble monitor.
14. **(T2B) UI:** no modal y sin supuestos de barra estrecha anclada: sin anchos fijos ni máximos en los controles de navegación, con
   disposición que se adapta; las constantes de tamaño del host no cambian en F2.

## 8. Pruebas requeridas y RED esperado

| Tarea | Proyecto | Filtro | MinSelected | ExpectRed |
|---|---|---|---|---|
| T2A | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.Workspace` | 656 | true |
| T2A | `tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj` | `FullyQualifiedName~RackCad.UI.Tests.Workspace` | 64 | false (sin cambios de UI) |
| T2B | `tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj` | `FullyQualifiedName~RackCad.UI.Tests.Workspace` | 64 + las de T2A si las hubiera | true |
| T2B | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.Workspace` | el GREEN de T2A | true solo si cambia una guarda |

- **Filtros con prefijo de espacio de nombres (C-F2T2-09):**
  - excluyen las clases ajenas al panel (`CustomPropertiesWorkspaceTests`, `ProjectVariablesWorkspaceTests`: 724 − 656 en Core);
  - hacen **idénticas** la selección por clase y la selección por FQN, porque el prefijo `RackCad.Tests.Workspace` cae dentro de
    `className`;
  - así eliminan la clase de defecto que agotó el nc3 ordinario de F2-T1 (selección solo por clase, 723 frente a 724).
- **RED de T2A:** el commit RED compila y falla por aserción, no por compilación:
  - el eco por contenido (INV-03 completo, con el contexto previo distinto);
  - el puerto de navegación con rechazo por sesión y por revalidación (puerto simulado: cero efectos);
  - la purga de sesiones terminadas;
  - las guardas nuevas de los archivos del Plugin (excepciones exactas, sin `async`/hilos, `using`).
  - Las 656 pruebas existentes siguen en verde, salvo los deltas de guardas autorizados.
- **RED de T2B:** pruebas de UI de las acciones:
  - habilitado solo con `Uno(R)`, `CanReadDrawing` y sin bloqueo;
  - petición con la sesión y el RackId del contexto;
  - copia k/M y vista para LOCATE;
  - rótulos de navegación (INV-36);
  - ausencia de anchos fijos.
- **Siempre:** CI exacta 4/4, incluido `Build Plugin without AutoCAD`, que compila las llamadas a la API de AutoCAD.

## 9. Presupuesto, `attempts` y controles

- **`attempts` de la unidad:** 1 de 3 (F2-T1 no consumió correcciones). Se propone el mismo esquema de F2-T1 para cada `TaskId`:
  - Attempt 1, AttemptsRemaining 2, MaxReworkLoops 3, CorrectionsAuthorized true;
  - planificación: 1 salida válida + hasta 2 reejecuciones solo por BLOCKED, transporte o INVALID_OUTPUT;
  - Worker: 1 invocación por delegación aceptada;
  - verificación: 1 salida válida por WorkRunId + 2 reejecuciones.
- **Controles:**
  - nc1..nc4 según README §10;
  - **nc4 en la primera delegación de cada cadena** (T2A y T2B son cadenas distintas);
  - A-4 (I64-SCOPE-BRIDGE-01) disponible para nc2 con sus condiciones; A-6 para el FreeText de nc3 si aplica;
  - **A-11 no aplica**: se activó para una sola aplicación de F2-T1, ya consumida. No hay sustituto activo del defecto de filtro de Tests
    del Controller; la mitigación es el filtro de C-F2T2-09. Si nc3 se agota otra vez: P-04 y decide el Coordinator.

## 10. STOP

- **S y P:** S-02, S-03, S-04, S-05, S-07, S-08, S-12, S-13 y S-14; P-01..P-08 (como F2-T1).
- **Específicos (borrador):**
  - **F2T2-C01:** una segunda autoridad de objetivos o un descubrimiento propio en Plugin o UI.
  - **F2T2-C02:** una acción que escribe authored, layouts, `TILEMODE` o variables de sistema.
  - **F2T2-C03:** una excepción de guarda por carpeta, o fuera de las autorizadas.
  - **F2T2-C04:** el mecanismo de contexto de comando o de foco requiere algo distinto de lo que fije el contrato.
  - **F2T2-C05:** inventario global o funciones de F3, F4 o F6.
  - **F2T2-C06:** cambiar los tipos de F2-T1 o los puertos de F1.
  - **F2T2-C07:** el alcance resulta Long-horizon tras la división.

## 11. Autoridades

- **UNIT_DOC:** `docs/initiatives/I-64-proposal-v5.md` (D-01..D-05, D-07; §10, §11, §12.3, §13);
  `docs/automation/evidence/I-64-pilot/F2-T1-NAVIGATION-CORE/coordinator/coordinator-f2t1-final-close.md`; `docs/automation/state/I-64.yml`;
  este paquete.
- **UNIT_CHANGE:** el producto de F2-T1 en `c726410f` (`src/RackCad.Application/Workspace/Navigation*.cs`).
- **EXTERNAL:** `docs/AUTOMATION_PLAN.md` §16; `routing.md`; `model-catalog.md`; `docs/WORKFLOW.md`.

## 12. Decisiones del Coordinator (STOP COORDINATOR_F2_T2_CONTRACT_REVIEW_REQUIRED)

| ID | Decisión | Recomendación de la sesión |
|---|---|---|
| C-F2T2-01 | Enrutamiento de F2-T2 monolítica (Long-horizon, sin celda Frontera con escritura): dividir, o una de las opciones de routing §4 (a) siguiente transporte, (b) A-n advisory, (c) BLOCKED — OWNER DECISION | **Dividir** en F2-T2A-NAVIGATION-HOST y F2-T2B-NAVIGATION-UI, secuenciales, con las celdas de F2-T1 |
| C-F2T2-02 | Contexto de comando (H-05). Opciones: (a) `DocumentCollection.ExecuteInCommandContextAsync` (API pública; su callback corre en el hilo principal en contexto de comando), llamado sin `async`/`await` en un solo archivo exacto, con delta de la guarda; (b) contexto de aplicación con `LockDocument`, que contradice el «contexto de comando» de D-07 y exigiría una A-n; (c) comando oculto + `SendStringToExecute`, que choca con dos guardas y con el censo de comandos | **(a)**, solo en `WorkspaceNavigationHost.cs`. La compilación contra AutoCAD.NET 25.0.1 en la CI confirma la firma; el `UNKNOWN` de la selección tras el comando interno sigue en SN-01 |
| C-F2T2-03 | Foco al dibujo (H-13). Opciones: (a) solo API pública (candidatos a confirmar: `Application.MainWindow.Focus()` o la ventana del documento activo), aislada en el host de navegación; (b) `Autodesk.AutoCAD.Internal` (`SetFocusToDwgView`) aislada, con degradación y evidencia, como admite D-01 (O-10) pero prohibió F1-T3B | **(a)**, con SN-07 como prueba; (b) solo con una decisión nueva si SN-07 falla |
| C-F2T2-04 | Hechos de navegación (H-09): (a) adaptador nuevo que usa `RackBlockFinder.ScanEnvelopes` **sin cambiarlo** y lee los handles de las referencias directas de nivel superior en Model Space de cada definición; (b) ampliar `ScanEnvelopes` (archivo integrado fuera del panel) | **(a)**: D-05 consume las autoridades integradas sin cambios; excepción exacta de `GetObject` y de H-08 para ese archivo |
| C-F2T2-05 | Puerto de acciones (H-10) | puerto **nuevo** `IWorkspaceNavigationPort` en un archivo; `IWorkspaceHostPort` y sus guardas sin cambio. El resultado de la ejecución en contexto de comando queda como valor legible por la UI |
| C-F2T2-06 | Mecánica del eco (H-12) | memoria pura por sesión del conjunto de handles escrito por SELECT, comparada como conjunto con la selección implícita del drenaje; deltas exactos en el lector y en `WorkspaceHostCore.cs`; rechazar la comparación solo a nivel de `SelectionContext` |
| C-F2T2-07 | `EndSession` (H-11): (a) purga perezosa de las sesiones que el puente ya no conoce (`TryGetSession`), en cada drenaje y antes de cada acción, sin aviso nuevo; (b) un aviso interno nuevo del puente, con delta de su superficie fijada | **(a)**: `SessionId` no se reutiliza, así que una caché huérfana nunca se usa; sin tocar el puente |
| C-F2T2-08 | Política de deltas de guardas | excepciones **por archivo exacto**, cada una fijada por una metaprueba en `WorkspaceSourceGuardExceptionTests.cs`; los deltas existentes se hacen en el commit RED (AUTOMATION_PLAN 16.8); recuentos de las metaguardas (seis archivos, 43 pruebas) actualizados de forma exacta |
| C-F2T2-09 | Filtros de RequiredTests | `FullyQualifiedName~RackCad.Tests.Workspace` (656) y `FullyQualifiedName~RackCad.UI.Tests.Workspace` (64), por § 8 |
| C-F2T2-10 | UI no modal (T2B) | invariante 14; las constantes de tamaño y acoplamiento del host no cambian en F2; nada de rediseño responsive general |
| C-F2T2-11 | Presupuesto y controles | § 9, con nc4 por cadena y sin sustituto de A-11 |
| C-F2T2-12 | Procedimiento de Smoke-NAV | tarea documental aparte (como F1-T3D), antes de Smoke-NAV y fuera de T2A/T2B |
| C-F2T2-13 | Secuencia tras la decisión | emitir el contrato de T2A → relevo 16.4 → planificación → A1-A8 → nc4 → STOP para la aceptación explícita del contrato y del paquete antes del Worker (o seguir al Worker si el Coordinator lo autoriza) |

## 13. Lo que este paquete no hace

- No invoca la planificación, el Worker ni ningún modelo, ni ejecuta A1-A8 ni nc4 (motivo al principio).
- No toca `src/` ni `tests/`, ni AutoCAD.
- No reutiliza el contrato de F2-T1; no amplía F2-T2 a F3, F4 ni F6.
