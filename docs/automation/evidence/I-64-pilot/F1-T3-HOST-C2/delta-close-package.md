# I-64 — Paquete de cierre delta de F1-T3-HOST-C2 (estado de orden en la paleta y activación sin documento)

Preparado por la sesión responsable el 2026-11-04 por orden del Coordinator («I-64 — COORDINATOR SMOKE-1 FAILURE ANALYSIS
DISPOSITION», literal en `coordinator-authorization.md`). Para **COORDINATOR_T3_C2_CLOSE_REQUIRED**.

Este paquete:
- **no** declara el cierre de C2 ni un Candidato nuevo de Smoke-1;
- no ejecutó AutoCAD, no pide otra ocurrencia de Smoke-1, no hace merge ni rebase de `main` y no inicia F2.

La tarea la ejecutó directamente la sesión responsable, sin invocaciones de modelos: la orden no nombró Worker, Controller ni celdas,
y no exige revisión del Architect.

## 1. Identidad

| Hecho | Valor |
|---|---|
| Tarea | `F1-T3-HOST-C2`, dos correcciones estrechas tras el NOT PASS de Smoke-1 `R20261103T175351Z-0a77` (SM-13, SM-34, SM-39) |
| Base | `88475713473cd56694975fa537b0c9a4a786aa66` (custodia del reintento); producto previo = `src/` y `tests/` de `b874705e` |
| **RED** | `57fea37780abf134663f02ae827dda2a70552b50`; CI 41866515249 (push): `Tests (Domain + Application)` = failure con **exactamente los 28 fallos esperados** (Failed 28, Passed 13144, Total 13172); `Build Plugin without AutoCAD` = skipped (depende de Tests); `Build UI` y `UI Tests` = success |
| **GREEN / producto** | `0e0d536b10350cc85e4a932a0d576b22fae22fa9`; CI 41867521852 (push) **4/4 success** |
| DLL del GREEN | `RackCad.Plugin.dll` `2C2433ABB82569D3E48271467186D8D5D1AA7C636E3937252328054897097B4E` (Debug, `0e0d536b`, 282112 bytes) |
| Candidato anterior | `09f2f1c3c4e64491018ef1489ce4dd3030d1e4a8` = **HISTORICAL_SMOKE1_NOT_PASS_CANDIDATE**; no se reutiliza (DLL `2923E7F2…AE26`) |

## 2. Cambio (archivos exactos)

`git diff --stat 88475713 0e0d536b`: **9 archivos, +814 / −23**, todos en las tres rutas de producto preferidas por la orden y en
pruebas del Workspace. Nada más cambia en `src/` ni en `tests/`.

| Commit | Archivo | Cambio |
|---|---|---|
| RED `57fea377` | `src/RackCad.Application/Workspace/EventBridgeCore.cs` | +17 / −7: esqueleto compilable sin cambio de comportamiento (`OnSourceEvent` devuelve `bool`, siempre `false`; `IsCommandActiveInActiveSession` = `false`; `OnNoActiveDocument` vacío) |
| RED | `tests/RackCad.Tests/Workspace/WorkspaceBridgeCommandStateTests.cs` | +213, nuevo (C2-A) |
| RED | `tests/RackCad.Tests/Workspace/WorkspaceBridgeNoActiveDocumentTests.cs` | +228, nuevo (C2-B) |
| RED | `tests/RackCad.Tests/Workspace/WorkspaceHostC2SourceGuardTests.cs` | +262, nuevo (guardas de fuente de C2) |
| RED | `tests/RackCad.Tests/Workspace/WorkspaceBridgeFacadeSourceGuardTests.cs` | +5 / −1: guarda existente ampliada (sección 4.3) |
| RED | `tests/RackCad.Tests/Workspace/WorkspaceHostSourceGuardTests.cs` | +4 / −2: guarda existente ampliada (sección 4.3) |
| RED | `tests/RackCad.Tests/Workspace/WorkspacePaletteHostSourceGuardTests.cs` | +6 / −3: dos guardas existentes ampliadas (sección 4.3) |
| GREEN `0e0d536b` | `src/RackCad.Application/Workspace/EventBridgeCore.cs` | +33 / −7 |
| GREEN | `src/RackCad.Plugin/Workspace/AutoCadEventBridge.cs` | +32 / −3 |
| GREEN | `src/RackCad.Plugin/Workspace/WorkspaceController.cs` | +19 / −5 |

Sin tocar: `WorkspacePaletteHost` (colocación de C1), lector de selección, `WorkspaceHostCore`, política, UI, comando `RACKPANEL`,
comandos clásicos, `RACKEDITAR`, persistencia del DWG, perfil y seguridad, T3C y T3D.

### 2.1 C2-A — aviso de presentación del estado de orden (SM-13, SM-34)

- **`EventBridgeCore`** (puro):
  - `OnSourceEvent` devuelve `true` **solo** cuando `CommandWillStart` encuentra la sesión en reposo o `EnteringQuiescentState` la
    encuentra con una orden en curso, y la sesión está viva y sana. Lo hace el método privado `SetQuiescent`, que solo cambia el
    indicador en memoria y dice si cambió. Las fuentes de pista devuelven `false` y se comportan como en T2.
  - `IsCommandActiveInActiveSession`: verdadero solo con la sesión activa sana entre `CommandWillStart` y `EnteringQuiescentState`, o
    activada sin estar en reposo. Falso antes de `Start`, sin documento activo y con la sesión degradada (sus fuentes de sincronización
    están desenganchadas y su último estado puede estar viejo).
- **`AutoCadEventBridge`**:
  - Aviso nuevo `internal event Action CommandStateChanged;`, sin argumentos y sin tipos de AutoCAD.
  - Lo levantan **solo** los dos manejadores existentes de `CommandWillStart` y `EnteringQuiescentState`, dentro de su
    `ContainSession`, después del núcleo y solo si este devuelve `true`:
    `if (_core.OnSourceEvent(a.Session, a.Source)) RaiseCommandStateChanged();`.
  - `RaiseCommandStateChanged` está contenido: si falla un suscriptor, se registra en RackLog; nunca degrada la sesión ni llega a
    AutoCAD.
  - Fachada interna `IsCommandActiveInActiveSession => _core.IsCommandActiveInActiveSession`.
- **`WorkspaceController`**:
  - Tercera suscripción, solo al aviso del puente: `_bridge.CommandStateChanged += OnCommandStateChanged;`.
  - `OnCommandStateChanged() => NotifyStateChanged();`: la paleta visible extrae un snapshot nuevo. No pide reconciliación, no encola
    pistas, no engancha `Idle`, no drena y no lee.
  - La sonda `HostProbe.IsActiveDocumentQuiescent` = `Editor.IsQuiescent && !_bridge.IsCommandActiveInActiveSession`. Motivo: la
    paleta extrae su snapshot de forma síncrona dentro de `CommandWillStart`, y en ese instante el editor puede seguir informando
    reposo; con la condición conjunta, el snapshot ya muestra `CommandActive`. No cambia el drenaje: el puente ya lo condicionaba al
    mismo indicador.
- **Resultado esperado en el host:** con `LINE` o `RACKEDITAR` en curso, «Leer el dibujo» y «Escribir en el dibujo» desmarcados y
  «Hay un comando en curso.»; al volver al reposo, el estado disponible. Sin verificar en el host (sección 7).

### 2.2 C2-B — `DocumentActivated` sin documento = sin documento activo (SM-39)

- **`EventBridgeCore.OnNoActiveDocument()`** (puro): la transición explícita. Antes de `Start` no hace nada; después, solo
  `_active = null`. No retira, desengancha, encola, drena ni lee nada: cada sesión conserva sus enganches, pistas y contexto.
- **`AutoCadEventBridge`**, manejador de `DocumentActivated`:
  `if (e.Document == null) _core.OnNoActiveDocument(); else _core.OnDocumentActivated(NativeId(e.Document), e.Document.Editor.IsQuiescent);`
  y después, en los dos casos, `ActiveSessionChanged?.Invoke();`.
  - No hay `return` silencioso: el aviso siempre sigue. La guarda `AnActivationWithoutDocumentIsTheExplicitTransitionAndStillAnnouncesTheActiveSession`
    prohíbe cualquier `return` en el cuerpo del manejador.
- Con eso desaparece la `NullReferenceException` contenida (ya no se llama a `NativeId(null)`) y la paleta, al extraer el snapshot,
  ve `ActiveSession` = ninguna: estado «Sin documento» (`NoDocument`), sin contexto de selección del documento anterior.
- Al activar después un documento, su sesión existente vuelve a ser la activa (sin enganches nuevos) o se crea la suya si es nuevo; el
  snapshot muestra solo su contexto.

## 3. Confirmaciones pedidas

| Confirmación | Evidencia |
|---|---|
| **La regla de T2 (sin pista, sin `Idle`) sigue intacta** | El oráculo `WorkspaceBridgeHintAndDrainTests.SourcesThatAreNotHintsQueueNothingAndNeverAttachIdle` **no se tocó** y pasa en RED, en GREEN y en la CI. Además, `TheCommandStateSourcesQueueNoHintAttachNoIdleAndDrainNothingEvenWhenTheyReportAChange` comprueba, incluso cuando el núcleo informa un cambio, que no hay pistas en ninguna sesión, ni llamadas de enganche o desenganche nuevas, ni `Idle`, y que `OnIdle` no drena. La guarda `TheCommandStateCasesOfTheCoreOnlyFlipTheInMemoryFlag` fija que `SetQuiescent` no nombra `EnqueueHint`, `SyncIdle`, `_source.` ni `TryDrain`. |
| **Ninguna lectura ni escritura nueva del DWG ni de la selección** | `ACommandStateChangeAsksTheHostForNoReconciliationAndReadsNothing`: con el host puro, `CommandWillStart` y `EnteringQuiescentState` no producen reconciliación, llamada al lector ni drenaje. `ThePanelShowsTheNoDocumentStateAndNoContextOfThePreviousDocument`: la transición sin documento no lee. Guardas: `TheCommandStateHandlersQueueNoHintNoReconciliationNoIdleAndReadNothing` (sin `SelectImplied`, `GetObject`, transacción ni bloqueo en los dos manejadores); `ARelayedCommandStateChangeOnlyAnnouncesTheState` (el relevo es exactamente `NotifyStateChanged()`); la sonda no usa `GetSystemVariable`, `SetSystemVariable` ni `Database`; y siguen en verde las guardas de T2/T3A/T3B que prohíben lecturas, escrituras y persistencia en `Plugin/Workspace`. |
| **La activación sin documento conserva las sesiones y vacía la sesión activa** | `TheActiveSessionBecomesNone`; `EverySessionStaysAliveWithItsHookupsItsHintsAndItsContext` (2 sesiones vivas, sin degradar, 12 enganches cada una, mismas llamadas de enganche y desenganche, pistas y contexto intactos); `ActivatingAnExistingDocumentAfterwardsRestoresItsOwnSessionWithoutANewHookup`; `ComingBackFromTheStartTabShowsTheContextOfTheActivatedDocumentOnly`; `ClosingTheLastDocumentAfterTheStartTabWasActiveRetiresOnlyThatSession`. Guarda `TheNoActiveDocumentTransitionOfTheCoreOnlyClearsTheActiveSession`. |
| **Sin suscripción nueva a AutoCAD** | El puente sigue con 16 `+=`, 16 `-=` y 16 manejadores (`TheCorrectionsAddNoAutoCadSubscriptionNorHandler` y las guardas de T2/T3A/T3B). La única suscripción nueva es la del controlador al aviso interno en memoria. |
| **Sin escritura de registro, perfil ni archivos** | `TheThreeFilesOfTheCorrectionsWriteNoRegistryProfileFileOrDrawing` (expresión de persistencia de T2 más `SetSystemVariable`, `CPROFILE`, `TRUSTEDPATHS` y `SECURELOAD`) sobre los tres archivos de producto; guardas de persistencia de T2 sobre `Plugin/Workspace`. |

## 4. Pruebas

### 4.1 Pruebas nuevas por exigencia de la orden

**C2-A** (`WorkspaceBridgeCommandStateTests`, 11 métodos, 20 casos; y guardas de `WorkspaceHostC2SourceGuardTests`):

| Exigencia | Pruebas | RED |
|---|---|---|
| 1. `CommandWillStart` produce el aviso | `CommandWillStartReportsAPresentationChangeAndTheActiveSessionRunsACommand`; guarda `TheNoticeIsRaisedOnlyFromTheTwoCommandStateHandlersAndOnlyWhenTheCoreReportsAChange` | FAIL |
| 2. `EnteringQuiescentState` produce el aviso complementario | `EnteringQuiescentStateReportsTheComplementaryChange`; la misma guarda | FAIL |
| 3, 4 y 5. ninguno encola pista, engancha `Idle` ni drena | `TheCommandStateSourcesQueueNoHintAttachNoIdleAndDrainNothingEvenWhenTheyReportAChange` (2 casos); `ACommandStateChangeKeepsTheHintsAlreadyQueuedForTheRestAndDrainsThemOnlyAtRest`; guardas `TheCommandStateHandlersQueueNoHintNoReconciliationNoIdleAndReadNothing` y `TheCommandStateCasesOfTheCoreOnlyFlipTheInMemoryFlag` | FAIL, salvo `TheCommandStateHandlersQueueNoHintNoReconciliationNoIdleAndReadNothing` (PASS: los manejadores ya no los nombraban) |
| 6. ninguna lectura del DWG ni de la selección | `ACommandStateChangeAsksTheHostForNoReconciliationAndReadsNothing`; guardas de la sección 3 | FAIL |
| 7. el oráculo de T2 sigue verde | `SourcesThatAreNotHintsQueueNothingAndNeverAttachIdle` sin cambios; `TheHintSourcesKeepTheirT2BehaviourAndReportNoPresentationChange` (9 casos) | PASS (sin cambios) |
| 8. el controlador releva el aviso a `StateChanged` | guardas `TheControllerRelaysTheCommandStateNoticeAsItsOnlyNewSubscription` y `ARelayedCommandStateChangeOnlyAnnouncesTheState` | FAIL |
| estado de orden de la sesión activa | `ANotificationThatChangesNothingReportsNoChange`; `OnlyTheActiveSessionCountsAsRunningACommand`; `ADocumentActivatedWhileNotAtRestRunsACommandUntilItComesBackToRest`; `ADegradedActiveSessionIsNeverReportedAsRunningACommandAndReportsNoChange`; `BeforeStartAndForARetiredSessionNothingIsReportedAndNothingRuns`; guardas `TheCoreTellsTheCommandStateOnlyForAHealthyActiveSession`, `TheBridgeTellsTheCommandStateOfTheActiveSessionOnlyByDelegatingToTheCore` y `TheProbeCountsACommandThatTheBridgeSawStartingAsACommandInCourse` | FAIL |
| forma del aviso | guardas `TheBridgeDeclaresOneInternalCommandStateNoticeWithoutArguments` y `RaisingTheNoticeIsContainedAndNeverDegradesTheSession` | FAIL |

**C2-B** (`WorkspaceBridgeNoActiveDocumentTests`, 11 métodos; y guardas):

| Exigencia | Pruebas | RED |
|---|---|---|
| 1. `DocumentActivated(null)` sin excepción | `NoActiveDocumentProducesNoExceptionAndNoFault`; guarda `AnActivationWithoutDocumentIsTheExplicitTransitionAndStillAnnouncesTheActiveSession` (ya no se llama a `NativeId(e.Document)` con documento nulo) | PASS la prueba (el esqueleto no lanza); FAIL la guarda |
| 2. la sesión activa pasa a ninguna | `TheActiveSessionBecomesNone`; `WithoutAnActiveDocumentNothingDrainsAndNoReconciliationIsQueued` | FAIL |
| 3. se conservan las sesiones | `EverySessionStaysAliveWithItsHookupsItsHintsAndItsContext`; `ClosingTheLastDocumentAfterTheStartTabWasActiveRetiresOnlyThatSession` | PASS (caracterización) |
| 4. aviso `ActiveSessionChanged` | guarda `AnActivationWithoutDocumentIsTheExplicitTransitionAndStillAnnouncesTheActiveSession` (aviso tras las dos ramas, sin `return`); `ThePanelShowsTheNoDocumentStateAndNoContextOfThePreviousDocument` (estado «Sin documento», sin contexto anterior) | FAIL |
| 5. activar después un documento existente restaura su sesión | `ActivatingAnExistingDocumentAfterwardsRestoresItsOwnSessionWithoutANewHookup`; `TheHintsOfADocumentWaitWhileNoDocumentIsActiveAndDrainWhenItIsActivatedAgain`; `ComingBackFromTheStartTabShowsTheContextOfTheActivatedDocumentOnly`; `ANewDocumentActivatedAfterwardsGetsItsOwnNewSession` | FAIL la de contexto; PASS el resto (caracterización) |
| 6. sin suscripción nueva | `TheCorrectionsAddNoAutoCadSubscriptionNorHandler`; mismas llamadas de enganche en las pruebas | PASS (guarda) |
| 7. sin escritura de registro, perfil ni archivos | `TheThreeFilesOfTheCorrectionsWriteNoRegistryProfileFileOrDrawing` | PASS (guarda) |
| transición del núcleo | `TheNoActiveDocumentTransitionOfTheCoreOnlyClearsTheActiveSession`; `BeforeStartNoActiveDocumentChangesNothingAndAfterStopNothingIsLeft` | FAIL la guarda; PASS la prueba |

### 4.2 RED

En local (Workspace, 577 casos) y en la CI (13172), exactamente **28 FAIL**:
- 11 casos de C2-A;
- 4 de C2-B;
- 11 guardas nuevas de C2;
- 2 guardas existentes ampliadas (la tercera suscripción del controlador, en T3A y en T3B).

Los 20 casos nuevos que pasan en RED son caracterización de lo que no debe cambiar: 9 fuentes de pista, conservación de sesiones,
ausencia de suscripciones y persistencia, y transición antes de `Start`.

### 4.3 Guardas existentes ampliadas (solo por C2)

La orden permite tocar «narrowly related Workspace tests/guards». Cuatro aserciones de forma exacta se amplían solo con los nombres o
la cuenta que C2 añade. No se elimina ninguna exigencia y no cambia ninguna otra línea:

| Archivo (unidad) | Prueba | Antes → después |
|---|---|---|
| `WorkspaceBridgeFacadeSourceGuardTests` (T3A) | `TheCoreAddsExactlyTwoPublicMembersToWhatT2Left` | lista exacta T2 + `Authorize`, `RequestReconciliation` → la misma lista + `IsCommandActiveInActiveSession`, `OnNoActiveDocument` (comentario C2; nombre de la prueba sin cambiar) |
| `WorkspaceHostSourceGuardTests` (T3A) | `TheControllerSubscribesOnlyToTheDrainedEventOfTheBridge` | 2 `+=` (`Drained`, `ActiveSessionChanged`) → 3, con `_bridge.CommandStateChanged +=` exigido; sigue sin `-=`. El archivo conserva sus 43 pruebas |
| `WorkspacePaletteHostSourceGuardTests` (T3B) | `TheControllerSubscribesToExactlyTheDrainedAndTheActiveSessionNoticesOfTheBridge` | 2 `+=` → 3, con `_bridge.CommandStateChanged += OnCommandStateChanged;` en el constructor |
| `WorkspacePaletteHostSourceGuardTests` (T3B, G3) | `TheRefinementsG1G2AndG3AreExactAndNothingWasRemovedFromTheT3AGuardFile` | exige en la guarda de T3A el texto `Assert.Equal(2, …"\+=")` → `Assert.Equal(3, …)` y además `_bridge\.CommandStateChanged\s*\+=`; 43 pruebas sin cambio |

Siguen exactamente igual, entre otras:
- las guardas del aviso `ActiveSessionChanged`: interno, sin argumentos, 3 menciones, 2 invocaciones y solo en `DocumentActivated` y
  `DocumentToBeDestroyed`, después del núcleo;
- el núcleo sin `public event`;
- la superficie pública del puente: el miembro nuevo es `internal`;
- los 16 enganches;
- una sola declaración `event` en el controlador;
- las 4 guardas de C1.

### 4.4 Verificación local sobre `0e0d536b`

Después del commit, con el árbol limpio antes y después y el SDK 8.0.423. Pasos en `local-verification-steps.txt`; los registros de
dotnet llevan rutas locales y no se versionan.

| Suite o build | Resultado |
|---|---|
| Core enfocada (`FullyQualifiedName~RackCad.Tests.Workspace`) | PASS 577/577 (531 + 46 nuevas) |
| UI enfocada (`FullyQualifiedName~Workspace`) | PASS 66/66 |
| Todas las guardas de T2, T3A, T3B y C1 | PASS (dentro de la Core enfocada y la Full) |
| Core Full | PASS 13172/13172 (13126 + 46) |
| UI Full | PASS 1701/1718 (17 Skip existentes, 0 con error) |
| Debug UI build | PASS (0 advertencias) |
| Debug Plugin build | PASS (0 errores; solo MSB3277, conocidas de las referencias de AutoCAD) |
| CI del SHA exacto | 41867521852 push 4/4 success |

AutoCAD: durante toda la verificación había un `acad.exe` abierto, iniciado por el usuario desde el Explorador el
2026-11-03T21:31:43Z, con **0 módulos RackCad** antes y después. La sesión no lo lanzó ni lo usó, y la compilación del Plugin no chocó
con él.

DLL de `0e0d536b` (Debug, en `src/RackCad.Plugin/bin/Debug/net8.0-windows`):

| DLL | SHA-256 |
|---|---|
| `RackCad.Plugin.dll` | `2C2433ABB82569D3E48271467186D8D5D1AA7C636E3937252328054897097B4E` (282112 bytes) |
| `RackCad.Application.dll` | `1A62440BD59279D5ED15EC6C29D23A6673C9420369D3E6A1E16FBC621409412C` |
| `RackCad.Domain.dll` | `F176F4D3BA2451B3D4578915E7A5592D5096A223127C098EFC873F4BC604FED6` |
| `RackCad.UI.dll` | `821CD0A88F0A85F7BEC8BB620D9D169CBB7202B982C0AC079E60428F8288F432` |

El SHA del commit se estampa en los ensamblados (guía §7.1): estos hashes identifican `0e0d536b` y no sirven para un Candidato de
otro SHA.

## 5. Resultados históricos conservados

- `git diff --quiet 88475713 0e0d536b -- docs/automation/evidence/I-64-pilot/F1-SMOKE-1` es vacío: la custodia de
  `R20261103T175351Z-0a77` y de `R20261019T180407Z-1be6` no se toca.
- `R20261103T175351Z-0a77` = **NOT PASS**; SM-13, SM-34 y SM-39 = **FAIL**. No se reetiquetan tras la corrección.
- `09f2f1c3` = **HISTORICAL_SMOKE1_NOT_PASS_CANDIDATE**; no se reutiliza para otro Smoke.

## 6. Comparación con lo congelado (D-03, D-04, S1-06f, S1-07)

| Requisito | Antes de C2 (`b874705e`) | Después de C2 (`0e0d536b`) |
|---|---|---|
| S1-06f: con una orden de AutoCAD en curso, las acciones que leen o escriben el DWG están deshabilitadas | el snapshot pedido lo calculaba, pero nada pedía el repintado: en el host, SM-13 FAIL | `CommandWillStart` → aviso interno → `StateChanged` → snapshot con `CommandActive`, aunque el editor aún informe reposo |
| S1-07 / OV-17: con `RACKEDITAR` el panel no interfiere y muestra «Hay un comando en curso.» | sin interferencia, pero sin el texto (SM-34 FAIL) | el mismo aviso; sigue sin interferir: no lee, no drena, no abre nada |
| D-04: los manejadores ordinarios solo encolan pistas o cambian estado en memoria; el drenaje solo en reposo | sí | sí; los dos manejadores de estado de orden solo cambian el indicador y avisan, sin pista ni `Idle` |
| D-03: sesiones por documento, perezosas, retiradas al destruir el documento | sí, pero una activación sin documento lanzaba excepción y dejaba la sesión anterior visible (SM-39 FAIL) | la pestaña Start deja sin sesión activa y conserva todas; volver a un documento restaura la suya |

## 7. Límites y residuales

- **Comportamiento real en el host, sin verificar.** No se lanzó AutoCAD. Lo comprobarán SM-13, SM-34, SM-26/SM-27 y SM-39 en el próximo
  Smoke-1, con un Candidato nuevo y su autorización de host.
- **Instante de `Editor.IsQuiescent` dentro de `CommandWillStart`, desconocido.** C2 no depende de él: la sonda usa también el indicador
  del puente.
- **Puntos de instrumentación:** el aviso no crea filas (no hay apertura, drenaje ni lectura). SM-14 contaría las mismas filas por
  orden que en `0a77`.
- **`Idle` sin documento activo:** si otra sesión tiene pistas pendientes cuando la pestaña Start está activa, el origen `Idle` sigue
  enganchado y cada pasada no drena nada hasta que se active un documento. Es el mismo patrón previo de una sesión inactiva con pistas.
  C2 no añade coste.
- **Aviso por sesión no activa:** si una sesión no activa cambia su estado de orden, la paleta extrae un snapshot igual al que tenía.
  Inocuo.
- **Candidato:** lo establece la sesión solo tras el C2 PASS del Coordinator, con SHA exacto, SHA-256 del DLL, evidencia local Full,
  CI del SHA exacto y autorización de host nueva. Este paquete añade un commit documental sobre `0e0d536b`.
- **Sincronización con `main`** (censo 39): sigue pendiente, por orden del Coordinator.

## 8. Siguiente

STOP en **COORDINATOR_T3_C2_CLOSE_REQUIRED**. Sin AutoCAD, sin otra ocurrencia de Smoke-1, sin merge ni rebase de `main` y sin F2.
