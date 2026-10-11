# I-64 — Paquete del contrato de gate de F1-T3-HOST

```text
Estado: PAQUETE PARA EMISIÓN — NO EMITIDO
Emisor: Coordinator (AUTOMATION_PLAN 16.5; agent-execution/README §1)
Preparado por: sesión responsable de I-64 (Claude), por la revisión del Coordinator del cierre de T2 (evidencia §47)
Rama al preparar: architecture/workspace-persistente-rackcad en 29410bfa, sobre origin/main bb0d5522
Sonda del Controller: R20261011T035128Z-515d = PASS (gpt-6-luna/high, Codex CLI 0.162.0-alpha.17.2, read-only)
STOP: COORDINATOR_T3_GATE_CONTRACT_REQUIRED (decisiones C-T3-01..C-T3-09)
```

Este documento no es un `gate-contract.json`, no emite delegaciones y no autoriza trabajo. **No se ha invocado ninguna planificación de T3.**
Deriva el contrato de las autoridades sin añadir alcance: el Freeze V5, A-4 y A-6 solo para los controles de protocolo, el estado y la
evidencia de I-64, el código en `29410bfa` y las autoridades de `main` en `bb0d5522`. Si una pieza del Freeze choca con el código actual,
el choque se eleva como decisión y no se resuelve aquí.

## 1. Fuentes

| Fuente | Qué aporta |
|---|---|
| Freeze V5 (`I-64-proposal-v5.md` en `9b43dafb`, blob `dc1924ff`) D-01, D-02, D-03, D-06, D-16, D-17, D-18 y D-22 | host `PaletteSet` y foco; controlador, capas y puerto; identidad de sesión; selección fina; coexistencia; contención; instrumentación; persistencia |
| Freeze V5 §10, §11 y §13 | INV-01, 03, 13, 14, 16, 17, 21, 22, 24 (solo host), 25 y 33; cierre de F1; clase «Implementación transversal a capas», Deep, «puede ser Long-horizon», «el Worker no usa AutoCAD» |
| `I-64-f1-gate-contract-proposal.md` §§2-7 | T3 = host, raíz WPF, `RACKPANEL`, selección fina, foco, contención e instrumentación; alcance propuesto Plugin/UI/UI.Tests |
| `I-64-f1-t2-gate-contract-package.md` y la decisión del contrato de T2 | forma del presupuesto, `attempts`, C-T2-04 (la guarda del puerto, INV-21, va con T3) |
| A-4 (I64-SCOPE-BRIDGE-01) y A-6 (I64-FREETEXT-BRIDGE-01) | sustitutos de nc2 y nc3, aplicables a cualquier tarea de I-64 con sus condiciones |
| Estado `I-64.yml` y evidencia §§46-47 | T2 COMPLETE_UNDER_I64_SUBSTITUTE_PROTOCOL_EVIDENCE; `attempts` 3 de 3 |
| Código en `29410bfa` | API de T1/T2; guardas de T1/T2; censos de comandos; referencias entre proyectos; estructura de UI y UI.Tests |
| Autoridades de `main` `bb0d5522` | `routing.md` §§1-5, `model-catalog.md`, `gate-contract.schema.json`, AUTOMATION_PLAN 16.3/16.5/16.8, README §10 |

## 2. Objetivo (T3 tal como lo describen el Freeze y la propuesta F1)

Host de F1 del panel, sin escrituras en el DWG y sin AutoCAD en ejecución:
- **Un único `PaletteSet` por proceso** (D-01): sin `toolID` y sin manejadores `Load`/`Save`; acoplable y flotante; aloja con `AddVisual` un
  solo control raíz WPF de `RackCad.UI`; cerrar = ocultar (las sesiones siguen en memoria); fallo al crearlo → mensaje y los comandos
  clásicos siguen funcionando (D-17).
- **Comando nuevo `RACKPANEL`** (nombre propuesto, D-01), que crea de forma perezosa el **controlador** único por proceso del Plugin (D-02) y
  arranca el puente de T2 (`AutoCadEventBridge.Start`). Cero trabajo y cero suscripciones antes del primer `RACKPANEL` (P-01).
- **Puerto** `IWorkspaceHostPort` (hipótesis del Freeze), implementado en el Plugin y consumido por la UI: claves y valores, nunca tipos de
  AutoCAD (INV-14); toda petición con su `SessionId` y rechazo sin efectos si el documento que ejecuta es otro (INV-22, `Authorize` de T1).
  En F1 el puerto **no expone escrituras** (INV-21).
- **Documento activo:** el panel muestra la sesión del documento activo; sin documento abierto, estado vacío con acciones deshabilitadas
  (D-02, O-17).
- **Selección fina hacia el contexto** (D-06): en el drenaje de T2 (evento `Drained`), lectura de la selección implícita del documento activo,
  prefiltro por clase, lectura del sobre de la definición de cada referencia seleccionada (transacción corta, documento bloqueado, contexto
  de aplicación, en reposo, sin modal RackCad) con caché por definición que invalidan las pistas, y `SelectionContext.Classify` →
  `WorkspaceSession.ApplyContext`. Contextos: Ninguno, Uno, Selección (N racks, M otros), Sin identidad, Diagnóstico. Sin enumeración global.
- **Panel visible/oculto** (`SetPanelVisible`): oculto = cero lecturas del DWG (INV-33); visible y ocioso = cero trabajo sin pistas (INV-01).
- **Política de foco** (D-01): con el foco en un campo del panel ninguna tecla ni atajo llega a AutoCAD; el foco no vuelve al dibujo tras una
  edición; un comando de AutoCAD activo deshabilita las acciones del panel que leen o escriben el DWG. La mecánica no está congelada; su
  comprobación es solo de host (INV-24, Smoke-1).
- **Contención** (D-17, INV-25 panel): ninguna excepción originada en el panel llega al bucle de mensajes de AutoCAD; el panel pasa a Error.
- **Instrumentación de host** (D-18): tiempos y memoria del proceso en el registro existente (I-03) con un interruptor del panel en memoria,
  sin tocar la configuración del host.

**Fuera de T3:** navegación (F2), pestañas (F3), borradores, Actualizar y veto de cierre (F4), editores (F5), inventario (F6), Smoke-1 y
cualquier uso de AutoCAD, perfil, `TRUSTEDPATHS`, `SECURELOAD`, ACL o registro.

## 3. Hallazgos del código que condicionan el contrato

Comprobados en `29410bfa` (lectura de solo lectura):

| # | Hecho | Efecto en T3 |
|---|---|---|
| H-1 | `WorkspaceBridgeSourceGuardTests.NothingIsHookedWhenThePluginLoadsAndNoCommandOrInitializerIsAdded` (T2) prohíbe `[CommandMethod` en **todo** `src/RackCad.Plugin/Workspace/` | `RACKPANEL` en esa carpeta hace fallar la guarda |
| H-2 | `NoExistingPluginCommandFileReferencesTheWorkspaceBridge` (T2) prohíbe que un archivo del Plugin **fuera** de `Workspace/` referencie `RackCad.Plugin.Workspace` o `RackCad.Application.Workspace` | `RACKPANEL` fuera de `Workspace/` no puede llegar al controlador |
| H-3 | `TheBridgeNeverWritesSelectionOrViewNorReadsStoredDataNorAsksNorOpensDialogs` (T2) prohíbe `SelectImplied`, `GetObject`, `OpenCloseTransaction`, `MessageBox`, etc. en **todo** `src/RackCad.Plugin/Workspace/` | la lectura del drenaje que exige D-06 (selección implícita y sobres) hace fallar la guarda; un aviso con `MessageBox` también |
| H-4 | Censos de comandos: `CustomPropertiesCommandGuardTests.TGrd08_ElCensoDeComandosEsElDeAperturaMasRackPropiedadesRprRackProyectarYRpy` (lista exacta) y `SelectiveEditorOpenTests.GUARDA_EL_CENSO_DE_COMANDOS_NO_CAMBIA` (`Assert.Equal(37, total)`) | añadir `RACKPANEL` en cualquier archivo del Plugin hace fallar dos pruebas existentes. Precedente: I-55 G15 los actualizó al añadir `RACKPROYECTAR`/`RPY` (35 → 37) |
| H-5 | `RackLog` (`src/RackCad.Application/Diagnostics/RackLog.cs`, I-03) solo tiene `Exception` y `Warning`; su redirección de pruebas es `internal` y solo visible para `RackCad.Tests` | la instrumentación de D-18 no tiene un nivel de tiempos; el código de UI que registre durante las pruebas de UI escribiría en el registro real |
| H-6 | `RackCad.Tests` referencia Domain y Application; `RackCad.UI.Tests` referencia Domain, Application y UI; **ninguna** referencia el Plugin | el host (`PaletteSet`, foco) solo se comprueba con guardas de fuente y en Smoke-1 |
| H-7 | `UiSystemBoundaryGuardTests` cubre todo `src/RackCad.UI/**`: `x:Class` = `RackCad.UI.Workspace.<Archivo>`, URIs `/RackCad.UI;component/...`; `NamespaceFolderGuardTests`: espacio de nombres = carpeta en producción y `RackCad.UI.Tests` en todas las pruebas de UI | el filtro `FullyQualifiedName~RackCad.UI.Tests.Workspace` solo selecciona clases `Workspace*`; hoy selecciona **0** |
| H-8 | No hay `App.xaml`; un control alojado fuera de una ventana debe fusionar sus recursos (patrón `ShellResources`) | el control raíz no puede dar por supuestos recursos de aplicación |
| H-9 | La API de T1/T2 basta: `Start`, `SetPanelVisible`, `SetModalRackCadActive`, `Drained`, `TryGetSession`, `ActiveSession`, `ApplyContext`, `SelectionContext.Classify`, `WorkspaceSessionRegistry.Authorize` | el producto de T3 puede ser **solo archivos nuevos**; los 10 archivos de producto de T1/T2 quedan prohibidos |
| H-10 | No existe el «procedimiento de diff de perfil» que el Freeze §11 pone en el resultado de F1 | no es producto de T3 (`docs/` prohibido al Worker): C-T3-06 |

## 4. Alcance de escritura propuesto (T3 monolítica)

`AllowedWriteScope`:
- `src/RackCad.Application/Workspace/` — solo archivos nuevos (puerto y políticas puras);
- `src/RackCad.Plugin/Workspace/` — solo archivos nuevos (controlador, comando, host de la paleta, lector del drenaje, instrumentación);
- `src/RackCad.UI/Workspace/` — carpeta nueva (control raíz y view-model);
- `tests/RackCad.Tests/Workspace/` — archivos nuevos y, por C-T3-04, `WorkspaceBridgeSourceGuardTests.cs`;
- `tests/RackCad.Tests/CustomPropertiesCommandGuardTests.cs` y `tests/RackCad.Tests/SelectiveEditorOpenTests.cs` — por C-T3-04, solo el
  censo (+`RACKPANEL`, 37 → 38);
- `tests/RackCad.UI.Tests/Workspace/` — carpeta nueva.

`ForbiddenWriteScope`:
- `docs/`, `.github/`, `assets/`, `eng/`, `deploy/`, `tools/`, `global.json`, `Directory.Build.props`, `Directory.Build.targets`, `RackCad.sln`;
- `src/RackCad.Domain/`;
- `src/RackCad.Application/`: `RackCad.Application.csproj`, `BlockNaming.cs` y las carpetas `Bom/`, `Catalogs/`, `ComputedParameters/`,
  `CustomProperties/`, `Diagnostics/`, `Drawing/`, `Expressions/`, `Formatting/`, `Geometry/`, `Layout/`, `Persistence/`,
  `ProjectVariables/`, `RackFrames/`, `Settings/`, `StructuralSections/`, `Systems/`, `Units/` y `Views/`;
- los 9 archivos existentes de `src/RackCad.Application/Workspace/`: `BridgeOutcomes.cs`, `BridgeSource.cs`, `EventBridgeCore.cs`, `Hints.cs`,
  `IHostEventSource.cs`, `SelectionContext.cs`, `SessionId.cs`, `WorkspaceSession.cs` y `WorkspaceSessionRegistry.cs`;
- `src/RackCad.Plugin/`: `RackCad.Plugin.csproj`, `PluginInitializer.cs`, `RackBlockFinder.cs`, `RackCommandSupport.cs`, las carpetas
  `Systems/`, `Views/`, `Drawing/` y `KindHandlers/`, los 17 `*Commands*.cs` existentes y `Workspace/AutoCadEventBridge.cs`;
- `src/RackCad.UI/`: `RackCad.UI.csproj`, `RackCommandReference.cs` y las carpetas `Controls/`, `Editor/`, `Preview/`, `RackFrames/`, `Shell/`,
  `StructuralSections/`, `Systems/`, `Themes/` y `Views/`;
- `tests/RackCad.Tests/`: `RackCad.Tests.csproj`, `NamespaceFolderGuardTests.cs`, `CustomPropertiesEdgeGuardTests.cs` y los 8 archivos de
  T1/T2 de `Workspace/` distintos de `WorkspaceBridgeSourceGuardTests.cs`;
- `tests/RackCad.UI.Tests/`: `RackCad.UI.Tests.csproj`, `UiSystemBoundaryGuardTests.cs`, `WindowCensusGuardTests.cs`,
  `CustomPropertiesHelpCensusTests.cs`, `StaTestRunner.cs` y `StaTestRunnerTests.cs`.

El proyecto es SDK y compila archivos nuevos sin tocar ningún `csproj`. El control raíz es un `UserControl`, no una ventana, así que el censo
de ventanas no cambia. Si la planificación necesita otra ruta, STOP (C-04).

## 5. Invariantes (borrador, INV-F1-T3-01..18)

| ID | Contenido | Origen |
|---|---|---|
| 01 | Un único `PaletteSet` por proceso, creado por el primer `RACKPANEL`; sin `toolID` ni `Load`/`Save`; `AddVisual` de exactamente un control raíz de `RackCad.UI`; cerrar = ocultar; fallo al crearlo → mensaje y comandos clásicos intactos | D-01, D-17 |
| 02 | Un controlador por proceso en el Plugin, creado de forma perezosa por el primer `RACKPANEL`; nada al cargar el plugin (`PluginInitializer.cs` intacto); `RACKPANEL` repetido no duplica controlador, paleta ni suscripciones | D-02, P-01, INV-02 |
| 03 | `IWorkspaceHostPort` implementado en el Plugin y consumido por la UI: solo claves y valores (sin `Document`, `Database`, `Transaction`, `DBObject`, `ObjectId` ni `Autodesk.*`); toda petición lleva su `SessionId` y se rechaza sin efectos si el documento que ejecuta es otro | D-02, INV-14, INV-22 |
| 04 | En F1 el puerto no expone ninguna escritura (guarda de fuente del puerto) | INV-21, C-T2-04 |
| 05 | Selección fina solo en el drenaje: selección implícita del documento activo y sobre de la definición de cada referencia seleccionada, en lectura, transacción corta dentro de `using`, documento bloqueado, contexto de aplicación, en reposo y sin modal RackCad; caché por definición invalidada por pistas; sin enumeración global | D-04 P-04, D-06, P-08 |
| 06 | Contextos por `SelectionContext.Classify` de T1: Uno(R) solo desde un sobre interpretable con Id no vacío = R; sin Id → Sin identidad; ilegible o versión futura → Diagnóstico; referencias anidadas, xref y espacio papel = «no rack»; nunca se estampa ni se inventa RackId | D-06, INV-16, INV-17 |
| 07 | Ningún evento ni cambio de contexto escribe selección o vista (sin `SetImpliedSelection`, `Zoom`, `Regen`, etc. en ningún archivo del panel); los manejadores de T2 siguen solo encolando | INV-03, D-04 |
| 08 | Panel oculto: cero lecturas del DWG; al mostrarse, una reconciliación en el siguiente reposo. Panel visible y ocioso: cero lecturas, resolver o `Regen` sin pistas | INV-33, INV-01, P-01b, P-02 |
| 09 | Sin documento abierto: estado vacío, acciones deshabilitadas, solo las suscripciones de la colección; ningún hilo de fondo toca AutoCAD | D-02, O-17 |
| 10 | Foco: con el foco en un campo del panel, ninguna tecla ni atajo llega a AutoCAD; el foco no vuelve al dibujo tras una edición; un comando de AutoCAD activo deshabilita las acciones del panel que leen o escriben el DWG. Comprobación de host en Smoke-1 (S1-06a..h); sin `Autodesk.AutoCAD.Internal` salvo decisión C-T3-07 | D-01, INV-24, O-10 |
| 11 | Toda entrada originada en el panel (eventos y comandos de la UI, callbacks del dispatcher, drenaje, llamadas al puerto) queda contenida en su frontera; ninguna excepción llega al bucle de mensajes de AutoCAD; el panel pasa a Error. Prueba de UI (STA) | D-17, INV-25 |
| 12 | Instrumentación: tiempos y memoria del proceso en el registro existente (I-03), con interruptor del panel en memoria; sin configuración del host ni archivos aparte; el código de UI no escribe en el registro real durante las pruebas | D-18, D-22, H-5 |
| 13 | La UI no referencia AutoCAD; `UiSystemBoundaryGuardTests` sigue en verde con `src/RackCad.UI/Workspace/`; recursos fusionados localmente | INV-13, ADR-0006, H-7, H-8 |
| 14 | `StartTransaction`, `StartOpenCloseTransaction` y `LockDocument` solo dentro de `using`; sin campos de tipo `Transaction`, `DBObject`, `Document`, `Entity` ni `BlockReference`; en Debug, `TopTransaction == null` al terminar cada operación y cada drenaje | INV-14 |
| 15 | Nada se persiste: ni authored, ni NOD, ni XData, ni registro de Windows, ni perfil, ni archivos aparte del registro | INV-21, D-22 |
| 16 | Sin cambio observable en los comandos existentes ni en RACKEDITAR; el censo de comandos solo crece en `RACKPANEL` | D-16, M-03, INV-19 |
| 17 | Sin tipos ni conceptos de I-63 (`ComputedParameter`, `Metric`, `Provider`, `ProjectSummary`, `TotalRacks`, `Aggregat`, `Bom`) | MASTER-I63-I64-02 |
| 18 | Los 10 archivos de producto de T1/T2 no cambian. La guarda de T2 solo se refina según C-T3-04: ninguna prueba se borra y los patrones prohibidos para los manejadores siguen vigentes | H-9, C-T3-04 |

## 6. Pruebas requeridas y RED esperado

| Proyecto | Filtro | `MinSelected` | `ExpectRed` | Hoy |
|---|---|---|---|---|
| `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.Workspace` | 1 | true | 159 seleccionadas, todas en verde |
| `tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj` | `FullyQualifiedName~RackCad.UI.Tests.Workspace` | 1 | true | 0 seleccionadas |

- **Cadena:** T3 es un `TaskId` nuevo, así que `ChainRedSha` es `null` y la primera entrega **exige RED** (16.8). El RED debe consistir en
  fallos reales por aserción contra un esqueleto que compila, sin pruebas artificiales.
  - **Core:** la política del controlador y del puerto con un host simulado: `RACKPANEL` repetido no duplica; panel oculto con cero lecturas;
    petición de otra sesión rechazada; el puerto sin escrituras; el mapeo de la selección con un lector simulado.
  - **UI:** el view-model presenta los cinco contextos, el estado vacío y el degradado; la contención de una excepción en un manejador
    alojado.
- **Clases:** las nuevas se llaman `Workspace*` en los espacios de nombres `RackCad.Tests` y `RackCad.UI.Tests` (H-7).
- **Fuera de los filtros:** los censos de comandos y la guarda de T2 los ejecuta la suite completa de la CI.

## 7. Enrutamiento (routing.md §§1-5 en `bb0d5522`)

**Clase:** «Implementación transversal a capas». Esta clase tiene dos perfiles: corta, que es Deep, y larga, que es
LONG_HORIZON_IMPLEMENTATION (§1).

| Dimensión (§2) | Valor | Razón |
|---|---|---|
| Ambigüedad | Medium | semántica congelada; mecánica de `PaletteSet` y foco no congelada; colisiones H-1..H-5 por resolver |
| Sensibilidad arquitectónica | High | nace el puerto, el controlador y el host; frontera UI/Plugin/Application; guardas de otras iniciativas |
| Amplitud | High | 5 proyectos y 6 carpetas de escritura, más dos archivos de prueba de otras iniciativas |
| Uso de herramientas | Medium | compilación WPF, dos proyectos de prueba, compilación del Plugin sin AutoCAD |
| Coste de fallo | High | `AttemptsRemaining` = 0: cualquier REWORK es S-11; el host no se comprueba hasta Smoke-1 |
| Repetición mecánica | Low | — |
| Horizonte | **High** | T2 (perfil corto) fueron 10 archivos y 2154 líneas en 3 carpetas con 86 turnos del Worker; T3 añade UI, host, puerto, lector, instrumentación y refinamiento de guardas en 6 carpetas de 5 proyectos, sin poder probar el host |

- **Resultado para T3 monolítica:** «Horizonte `High` fija `Long-horizon`» (§2.3), así que el nivel es Frontera (§3).
- **Celda Worker elegible para Long-horizon: NINGUNA.**
  - El catálogo (`model-catalog.md` en `bb0d5522`) registra `claude-opus-5-5` con «subagente × `write-commit-push` — no medido».
  - `claude-fable-5-1` aparece con «`write-commit-push` — no medido» y consumo `UNKNOWN`.
  - `gpt-6-astra` figura como «no es elegible».
- **Consecuencia:** STOP según §4, «Sin celda elegible»: decide el Coordinator (C-T3-01).
- **No se propone `claude-sonnet-5-5`/`high` para la T3 monolítica**, por orden del Coordinator.

| Rol | Celda | Elegibilidad (§5) |
|---|---|---|
| Worker Deep (solo si T3 se divide en tareas cortas, C-T3-01) | subagente × `claude-sonnet-5-5` × `high`, `write-commit-push` | Equilibrado. Medida en U-04 (`medium`/`high`) y usada con `high` en G3 y en T2 (`3df4`). Consumo cubierto. Verificada el 2026-09-30, pasa a STALE el 2026-12-29 |
| Worker Long-horizon | — | ninguna celda Frontera con escritura medida |
| Controller (planificación y verificación) | Codex CLI `0.162.0-alpha.17.2` × `gpt-6-luna` × `high` | Eficiente con más effort para Balanced (§3). Catálogo: medida en PR-1, consumo cubierto, verificada el 2026-09-30. **Vuelta a medir con el CLI vigente** en la sonda `R20261011T035128Z-515d` (PASS) |

`RoutingEnforcement` propuesto: `required`.

## 8. Presupuesto, `attempts` y controles

- **Unidad:** `attempts` = 3 y `max_attempts` = 3, así que la primera delegación lleva `Attempt` 3, `AttemptsRemaining` 0 y
  `MaxReworkLoops` 3. No se reinicia ni se aumenta nada.
  - Esto no impide la primera delegación de un `TaskId` nuevo.
  - Pero cualquier resultado válido que exija corrección da **STOP S-11**. `CorrectionsAuthorized` = **false**.
- **Presupuesto de invocaciones propuesto por `TaskId`** (misma forma que la decisión de T2; P-07 se evalúa contra él):

  | Fase | Tope |
  |---|---|
  | Planificación | 1 salida válida + hasta 2 invocaciones adicionales solo por transporte o INVALID_OUTPUT |
  | Trabajo | 1 salida válida + hasta 2 adicionales solo por transporte o INVALID_OUTPUT |
  | Verificación | 1 salida válida + hasta 2 adicionales solo por transporte o INVALID_OUTPUT |
  | nc4 | sin invocación (REJECTED_BEFORE_INVOCATION) |
  | nc1, nc2, nc3 | 1 salida válida cada uno + hasta 2 adicionales solo por transporte o INVALID_OUTPUT |
  | I64-SCOPE-BRIDGE-01 (A-4) | 1 ejecución si el Controller nc2 no discrimina |
  | I64-FREETEXT-BRIDGE-01 (A-6) | 1 ejecución si el Controller nc3 se agota (P-04), con sus condiciones |

  Una segunda salida válida de planificación, trabajo o verificación no se autoriza para mejorar la primera.
- **Riesgo conocido:** con `CorrectionsAuthorized` = false, en T2 el Controller convirtió el REWORK de nc3 en STOP (C-13/C-15/S-11) en los
  tres intentos. Es previsible que T3 vuelva a necesitar A-6, y el presupuesto lo deja explícito.

## 9. STOP

S-01..S-14 y P-01..P-08 sin cambios, más estos:

| ID | Condición |
|---|---|
| C-01 | `origin/main` cambia antes de escribir (S-13, 16.7) |
| C-02 | Hay que tocar un archivo de producto de T1/T2, `PluginInitializer.cs`, un `csproj`, el `sln`, un comando existente, `RackBlockFinder.cs` o `RackLog.cs` |
| C-03 | Hace falta AutoCAD, cargar el DLL, perfil, `TRUSTEDPATHS`, `SECURELOAD`, ACL o el registro; Smoke-1 no es una tarea delegada |
| C-04 | Hay que ampliar el alcance |
| C-05 | Aparece una ambigüedad material del Freeze |
| C-06 | `config.toml` fuera de A/B/C o con otro inventario |
| C-07 | Conflicto de propiedad o archivo caliente (I-52, I-65, I-62) |
| C-08 | Deriva hacia F2-F7 (navegación, pestañas, borradores, inventario) |
| C-09 | Tipos de I-63 |
| C-10 | Cambio observable en comandos existentes o RACKEDITAR |
| C-11 | Escritura authored, NOD o XData |
| C-12 | `Autodesk.AutoCAD.Internal` sin decisión C-T3-07 |
| C-13 | Presupuesto agotado |
| C-14 | Una modificación de la guarda de T2 o de los censos fuera de lo que autorice C-T3-04 |
| C-15 | Tokens reservados por las guardas de I-65 en `src/` (`RamElements`, `Utilization`, `DesignRatio`, `0.45359237`, `9.80665`, etc.) |

## 10. Autoridades

- **`UNIT_DOC`, en `AuthorityRevision`:**
  - Freeze V5, discovery y contrato de la iniciativa;
  - A-2, A-4, A-5 y A-6;
  - evidencia §§18, 39 y 45-48;
  - estado;
  - propuesta F1, paquete de T2 y este paquete;
  - ADR 0047 (propuesto, de la unidad).
- **`EXTERNAL`, en `MainSha`:** `AGENTS.md`, `docs/WORKFLOW.md`, `docs/INITIATIVE_LIFECYCLE.md`, `docs/AUTOMATION_PLAN.md` §16,
  `docs/automation/agent-execution/` (incluidos `routing.md` y `model-catalog.md`), ADR-0006, ADR-0019, ADR-0029, ADR-0044 y I-55 V5
  §4.4-§4.10.
  - Ninguna ruta `EXTERNAL` cambió en la rama desde `bb0d5522`: el único archivo bajo `docs/adr/` es el ADR 0047 de la propia unidad.
- **Sin `UNIT_CHANGE`.**

## 11. Coordinación (lectura de solo lectura, 2026-10-11, tras `git fetch`)

| Rama | Punta | Rutas de T3 y archivos calientes | Nota |
|---|---|---|---|
| I-52 `feature/rackmirror-espejo-semantico` | `fb6b5648` (2026-10-02; 90 detrás de `main`) | ninguna ruta de `src/` ni `tests/`; toca `docs/HANDOFF.md` y `docs/ROADMAP.md` | **no hay RELEASE del host**. Su última decisión (§258): P2 NOT_AUTHORIZED; perfil `<<Unnamed Profile>>`; exige `acad.exe` = 0 antes de sus corridas y una línea base exacta de `TRUSTEDPATHS`. T3 no usa el host; Smoke-1 sigue bloqueado hasta su RELEASE |
| I-65 `architecture/interop-analisis-estructural` | `1692804a` (2026-10-10) | ninguna: `src/RackCad.Application/StructuralAnalysis/`, unidades y pruebas propias; fila propia en ROADMAP | su contrato reconoce que Workspace, `PaletteSet`, UX WPF, selección y eventos son de I-64. Sus guardas futuras escanean todo `src/` (C-15) |
| I-62 `architecture/portabilidad-coordinador-principal` | `1c42300f` (2026-10-10) | ninguna ruta de `src/`; pruebas `tests/RackCad.Tests/I62*` | §68: prioridad de I-64 en CPU, CI, archivos calientes y tiempo del Owner |

T3 no escribe ROADMAP ni HANDOFF. Las filas de ROADMAP de I-64 e I-65 son casi adyacentes: es un riesgo para la integración, no para T3.

## 12. Decisiones del Coordinator (STOP COORDINATOR_T3_GATE_CONTRACT_REQUIRED)

- **C-T3-01 — Enrutamiento.** La T3 monolítica da Horizonte High y por tanto Long-horizon, y no hay celda Frontera con escritura elegible
  (§7). Opciones:
  - (i) **dividir T3** en dos tareas cortas Deep con `claude-sonnet-5-5`/`high`. Es la recomendación de la sesión, sin crear alcance:
    - **F1-T3A-HOST-CORE:** puerto, controlador, `RACKPANEL`, lector del drenaje, instrumentación, refinamiento de la guarda de T2 y censos.
      Rutas `Application/Workspace/`, `Plugin/Workspace/`, `tests/RackCad.Tests/...`; filtro Core.
    - **F1-T3B-HOST-UI:** control raíz y view-model, host `PaletteSet`, foco y contención. Rutas `UI/Workspace/`, `Plugin/Workspace/` (archivo
      nuevo del host), `UI.Tests/Workspace/`; filtro UI. Entra con T3A verificada.
    - Cada una es un `TaskId` nuevo, con su propio presupuesto, sus controles nc1-nc4 y `AttemptsRemaining` 0.
  - (ii) routing §4 (a): el siguiente nivel de transporte, con la limitación documentada;
  - (iii) routing §4 (b): una A-n de Coordinator + Architect con `RoutingEnforcement: advisory`;
  - (iv) routing §4 (c): BLOCKED — OWNER DECISION.
- **C-T3-02 — Presupuesto** de §8 por `TaskId`, que fija el Coordinator al emitir (P-07).
- **C-T3-03 — `attempts`.** `AttemptsRemaining` 0 y `CorrectionsAuthorized` false.
  - Por tamaño, el riesgo de S-11 en T3 es alto.
  - La sesión no lo cambia: un cambio de `max_attempts` exige la autoridad del contrato de la iniciativa.
- **C-T3-04 — Archivos de prueba existentes (H-1..H-4).** Sin modificarlos, T3 no puede pasar en verde.
  - **Guarda de T2** (`WorkspaceBridgeSourceGuardTests.cs`, de I-64). Se propone refinarla solo para:
    - permitir `[CommandMethod("RACKPANEL")]` en **un** archivo nombrado de `Plugin/Workspace/`;
    - permitir lecturas (`SelectImplied`, `GetObject` en lectura) en **un** archivo lector nombrado que solo se invoca desde el drenaje.

    Todo lo demás sigue igual: ningún `+=` nuevo sobre eventos gobernados, los manejadores solo encolan, ninguna escritura de selección o
    vista, ninguna `Internal` y nada en `PluginInitializer.cs`.
  - **Censos** (`CustomPropertiesCommandGuardTests.cs` y `SelectiveEditorOpenTests.cs`, de iniciativas integradas): solo +`RACKPANEL` y
    37 → 38, como hizo I-55 G15.
  - Sacar el comando o el lector de `Workspace/` choca con H-2 y no se propone.
- **C-T3-05 — Instrumentación (H-5).** Elegir una opción:
  - (a) T3 solo deja el interruptor en memoria y los puntos de medida, y escribe con los niveles existentes de `RackLog`;
  - (b) se autoriza un método nuevo en `RackLog.cs` (archivo de I-03, hoy prohibido);
  - (c) la instrumentación pasa a F7, que según D-18 la consolida.

  En todos los casos, el código de UI informa por el puerto y no escribe directamente en el registro.
- **C-T3-06 — Procedimiento de diff de perfil (H-10).** Está en el resultado de F1 y no es producto del Worker. Se propone como trabajo
  documental de la sesión o como tarea Documentación aparte, siempre antes de Smoke-1.
- **C-T3-07 — Foco.** Con la guarda de T2, `Autodesk.AutoCAD.Internal` (por ejemplo, `SetFocusToDwgView`) está prohibido en
  `Plugin/Workspace/`. Se propone mantener la prohibición en F1: el foco no vuelve al dibujo tras una edición y en F1 no hay acciones de
  navegación (llegan en F2). Si hace falta, se decide en F2 con evidencia (O-10).
- **C-T3-08 — `AuthorityRevision` y `MainSha`.**
  - `AuthorityRevision` = el commit que versione la decisión del Coordinator sobre T3, igual que en T2. Su ancestro contiene este paquete,
    sin rutas `EXTERNAL` modificadas desde `bb0d5522` (16.3).
  - `MainSha` = `origin/main` recién obtenido al emitir; hoy `bb0d5522e8411f66a51fdfb3f1f0d0514b737453`. Si `main` avanza: S-13 y 16.7.
  - Primer `BaseSha` = `HEAD` al delegar.
- **C-T3-09 — Host e I-52.** No hay RELEASE de I-52.
  - T3 no lo necesita (C-03).
  - F1 PASS no exige Smoke-1, que va después; pero F4 exige Smoke-1 PASS.
  - Antes de emitir hay que volver a comprobar la propiedad de I-52.
