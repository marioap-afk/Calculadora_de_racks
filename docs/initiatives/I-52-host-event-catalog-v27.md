# I-52 — Host Event Catalog V27

> **HEC-V27-1 / MANAGED CLOSED / NATIVE OPEN / NOT EXECUTED.** Este catalogo define nombres, firmas, triggers y
> oraculos para la futura caracterizacion. No autoriza harness, helper ObjectARX ni CT-DA.

## 1. Autoridad y tuple inspeccionado

El catalogo managed se obtuvo por reflection directamente de las DLL instaladas, sin ejecutar AutoCAD:

| Assembly | Assembly version | File/product version | SHA-256 |
|---|---|---|---|
| `AcCoreMgd.dll` | `25.0.0.0` | `25.0.171.0.0` | `31025BC01ABC2CB398040AC82A5018E4FCD7A80FE8A2012DE0329E7F8ED746AA` |
| `AcDbMgd.dll` | `25.0.0.0` | `25.0.171.0.0` | `C360186CC0702E210635895B7608D1D8BD5FEEC6A00612B7581EF160FE09CEDE` |
| `AcMgd.dll` | `25.0.0.0` | `25.0.171.0.0` | `7AA8BF5F79F3F980DCC9F4CF9483F8438E773F6BF7C05EC3B263449B2846BC8C` |

La busqueda local no encontro `dbtrans.h`, `dbmain.h`, `dbreactor.h` ni `aced.h` del SDK ObjectARX 2025. Los nombres
nativos de este documento proceden de contratos publicos Autodesk, pero su presencia/firma contra el SDK exacto 2025
queda `UNKNOWN` hasta inspeccionar headers del toolset exacto. No se infieren nombres nativos desde wrappers managed.

## 2. Schema normativo de evento

Cada evento usa:

```text
EventId; Authority; API type; exact event/callback; publisher/registration;
trigger; lifecycle phase; transaction state; document-lock state;
allowed target; probes; DA-P/H-P; threats; PASS; FAIL; UNKNOWN.
```

`PASS` exige que el callback observado corresponda al EventId exacto. Un evento vecino, wrapper o emulacion no cuenta.

## 3. Catalogo managed exacto

| EventId | API type / exact event | Publisher / firma | Trigger y fase | T/lock | Target permitido | Probes | DA-P / threats | Oraculo |
|---|---|---|---|---|---|---|---|---|
| M-DB-OPEN | `Database.ObjectOpenedForModify` | scratch `Database`; `ObjectEventHandler(object,ObjectEventArgs)` | DBObject abierto para write; antes de modificar | current/top T se mide; lock se mide | objeto fixture distinto del notifier, salvo probe de rechazo | 01,02,03,06,08,10*,12,14,15 | P1,3,5,6,9,11; T1,3,7,8,10–12 | PASS si fires una vez sobre trigger exacto y log conserva orden; FAIL si orden contradice contrato; UNKNOWN si T/lock no observable |
| M-DB-MOD | `Database.ObjectModified` | scratch `Database`; `ObjectEventHandler(object,ObjectEventArgs)` | despues de modificar DBObject | current/top T y lock registrados | target fixture controlado | 01–06,08,09C,10*,11–15 | P1–6,8–11; T1,3–12 | mismo criterio; no se llama commit-phase |
| M-DB-ERASE | `Database.ObjectErased` | scratch `Database`; `ObjectErasedEventHandler(object,ObjectErasedEventArgs)` | erase/unerase completado para notifier | T/lock registrados | sibling fixture distinto o target explicitado | 08,10SM,12,15 | P5,6,7,9,11; T7,8,11,12 | verifica `Erased` y DB final |
| M-DB-APPEND | `Database.ObjectAppended` | scratch `Database`; `ObjectEventHandler(object,ObjectEventArgs)` | objeto agregado al DB | T/lock registrados | nueva referencia/payload fixture | 08,10SM,12,15 | P5,6,7,9,11 | verifica ObjectId, owner y estado final |
| M-DOC-WILL | `Document.CommandWillStart` | scratch `Document`; `CommandEventHandler(object,CommandEventArgs)` | comando identificado a punto de iniciar | T normalmente no asumida; lock medido | logging/arming only | lifecycle,16* | P6,11,12; T8,16 | nombre de comando y sequence exactos |
| M-DOC-END | `Document.CommandEnded` | scratch `Document`; misma firma | comando identificado completado | no se asume T ni writability | logging; mutation solo en probe que abra su propia T y registre resultado | 09E,10*,16* | P2,6,11,12; T4,6,8,16 | no se preclasifica antes/despues de success; CT-DA lo decide |
| M-DOC-CANCEL | `Document.CommandCancelled` | scratch `Document`; misma firma | comando cancelado | medido | logging/cleanup | 04,13,lifecycle | P3,12 | outcome CANCEL exacto |
| M-DOC-FAIL | `Document.CommandFailed` | scratch `Document`; misma firma | comando fallo | medido | logging/cleanup | 04,13,lifecycle | P3,12 | outcome FAIL exacto |
| M-LOCK-WILL | `DocumentCollection.DocumentLockModeWillChange` | `DocumentCollection`; `DocumentLockModeWillChangeEventHandler` | antes de cambiar lock mode | old/new context en args/log | logging; probe de veto separado | 13,16* | P3,6,11,12; T13,16 | orden frente a acquire/release |
| M-LOCK-CHANGED | `DocumentCollection.DocumentLockModeChanged` | `DocumentCollection`; `DocumentLockModeChangedEventHandler` | despues de cambiar lock mode | lock result registrado | logging | 13,16* | P3,6,11,12 | lock state coincide con marker |
| M-LOCK-VETO | `DocumentCollection.DocumentLockModeChangeVetoed` | `DocumentCollection`; `DocumentLockModeChangeVetoedEventHandler` | cambio de lock vetado | lock no concedido | logging | 13 | P3,12 | no mutation survives |
| M-IDLE | `Application.Idle` | `Application`; `EventHandler(object,EventArgs)` | mensaje idle entregado por host | T/lock no asumidos | callback abre context/T solo si API lo permite | 16S/M/SM-IDLE | P6,11,12; T16 | scheduling/execution markers y DB final |
| M-MODELESS-WILL | `Document.ModelessOperationWillStart` | scratch `Document`; `ModelessOperationEventHandler` | operacion modeless identificada inicia | medido | logging only | lifecycle-observation | P12 | context string + order; no scheduling claim |
| M-MODELESS-END | `Document.ModelessOperationEnded` | scratch `Document`; `ModelessOperationEventHandler` | operacion modeless identificada termina | medido | logging only | lifecycle-observation | P12 | context string + order; no scheduling claim |

No se reclama `TransactionManager` event managed: la reflection exacta no expone eventos publicos en ese type.
`SystemVariableChanged`, document activation y drawing open/close existen, pero quedan OUT del fixture porque ninguna
surface ni lifecycle claim de HF-V27-1 depende de ellos. Una futura dependencia los vuelve IN y exige revision.

Registration points son normativos: M-DB-* se registran en la scratch `Database` antes de crear triggers; M-DOC-* en
el scratch `Document` antes de invocar el command de research; M-LOCK-* en `DocumentCollection` antes de adquirir el
lock principal; M-IDLE se registra one-shot exactamente en el schedule marker y se retira en su primera invocacion o
cleanup; M-MODELESS-* se registran antes del command sólo para observar. Registro tardio o global distinto=UNKNOWN.

## 4. Schedulers managed exactos

| SchedulerId | API exacta verificada | Enqueue | Ejecucion/limite | Probes |
|---|---|---|---|---|
| MS-SEND | `Document.SendStringToExecute(string,bool,bool,bool)` | durante comando de research | comando en command queue; fase exacta se caracteriza, no se presupone | 16S/M/SM-SEND |
| MS-CONTEXT | `DocumentCollection.ExecuteInCommandContextAsync(Func<object,Task>,object)` | durante comando | callback en command context; completion/order se mide | 13,16S/M/SM-CONTEXT |
| MS-IDLE | handler one-shot de `Application.Idle` | handler armado durante comando | siguiente Idle observado; causalidad se registra | 16S/M/SM-IDLE |

`SynchronizationContext`, COM/modeless externo y otra queue no se consideran equivalentes. Los eventos M-MODELESS son
marcadores, no un mecanismo de enqueue. Si el harness o un kind usa una de esas rutas,
`SchedulerCoverageCompatible=false/UNKNOWN` hasta catalogo y probe propios.

## 5. Catalogo nativo candidato — autoridad exacta abierta

| NativeEventId | Clase/callback publico Autodesk | Trigger/phase contractual publico | Target/probes | Exact 2025 header |
|---|---|---|---|---|
| N-DB-OPEN | `AcDbDatabaseReactor::objectOpenedForModify` | antes de modificar el objeto | 02N,10N-* | UNKNOWN |
| N-DB-MOD | `AcDbDatabaseReactor::objectModified` | despues de modificar el objeto | 04N,10N-* | UNKNOWN |
| N-DB-ERASE | `AcDbDatabaseReactor::objectErased` | despues de erase/unerase | 10N-SM | UNKNOWN |
| N-TX-ABOUT-END | `AcTransactionReactor::transactionAboutToEnd` | antes de terminar la T | 09N-A | UNKNOWN |
| N-TX-COMMIT-BEGIN | `AcTransactionReactor::endCalledOnOutermostTransaction` | inicio documentado del commit exterior | 09N-B | UNKNOWN |
| N-TX-ENDED | `AcTransactionReactor::transactionEnded` | T terminada; count identifica outermost | 09N-C | UNKNOWN |
| N-TX-ABOUT-ABORT | `AcTransactionReactor::transactionAboutToAbort` | antes del aborto | 04N-A | UNKNOWN |
| N-TX-ABORTED | `AcTransactionReactor::transactionAborted` | T abortada | 04N-B | UNKNOWN |
| N-ED-WILL | `AcEditorReactor::commandWillStart` | comando a punto de iniciar | lifecycle,16N-* | UNKNOWN |
| N-ED-END | `AcEditorReactor::commandEnded` | comando completado | 09N-D,16N-* | UNKNOWN |
| N-ED-CANCEL | `AcEditorReactor::commandCancelled` | comando cancelado | 04N-C | UNKNOWN |
| N-ED-FAIL | `AcEditorReactor::commandFailed` | comando fallo | 04N-C | UNKNOWN |
| N-ED-MODELESS-WILL | `AcEditorReactor::modelessOperationWillStart` | operacion modeless inicia | 16N-* candidate | UNKNOWN |
| N-ED-MODELESS-END | `AcEditorReactor::modelessOperationEnded` | operacion modeless termina | 16N-* candidate | UNKNOWN |

Autoridades publicas consultadas: Autodesk ObjectARX Developer/Reference Guide para `AcDbDatabaseReactor`,
`AcTransactionReactor` y `AcEditorReactor`. Hasta contar con los headers 2025 exactos, todos los NativeEventId son
`CANDIDATE / EXACT_BUILD_UNKNOWN`; no pueden cerrar el catalogo ni producir PASS.

## 6. Schema normativo de probe

```text
ProbeId; Threats; EventClassId; Managed/Native; Setup; Trigger;
PrimaryTransactionId; TopTransactionId; DocumentLock; Thread/Context;
Mutation; S/M/SM; SchedulePoint; ExecutionPoint; Before; After;
Verifier; LifecycleMarkers; PASS; FAIL; UNKNOWN; Cleanup.
```

Todo probe debe registrar cada campo. Campo no observable => UNKNOWN, nunca valor inferido.

## 7. Probes managed 01..16

| Probe | Event binding y operacion exacta | Surface/oraculo |
|---|---|---|
| 01 | M-DB-MOD sobre trigger; handler escribe destination Xrecord en primary T si es accesible | S; semantic verifier; T mismatch=FAIL/UNKNOWN |
| 02 | M-DB-OPEN; handler usa el primary `Transaction` registrado para abrir target/write | S; bytes staged + T identity |
| 03 | M-DB-MOD; handler intenta `StartTransaction` nested y luego nueva T segun API | S/M; survival/containment y ids |
| 04 | 02/03 seguidos por abort de primary T | S/M; before bytes restaurados o FAIL |
| 05 | 02/03 seguidos por commit de primary T | S/M; postcommit exacto |
| 06 | M-DB-OPEN y M-DB-MOD durante reemplazo `Xrecord.Data` | S; orden real, sin etiqueta commit |
| 07 | sin event dependency: crear definition/Xrecord y reader fresh en primary T | S/M; RYOW |
| 08 | despues de leer A, modificar trigger para M-DB-MOD; handler muta B antes de leer B | S/SM; mixed snapshot detectado/rechazado |
| 09C | M-DB-MOD/object-close-associated; no se llama commit-phase | S/M; orden observado solamente |
| 09E | M-DOC-END; intenta T nueva y mutacion despues de command completion reportado | S/M; lifecycle/postcommit, no commit interno |
| 10S | M-DB-MOD armado despues de R_post; trigger antes de return muta Xrecord | S; P6/P12 |
| 10M | igual, muta transform/layer | M; P11/P12 |
| 10SM | igual, erase/add sibling + linkage | SM; P6/P11/P12 |
| 11 | M-DB-MOD sobre trigger; muta NOD Xrecord | S; NOD verifier |
| 12 | despues de leer physical A, M-DB-MOD muta B antes de leer B | M/SM; mixed snapshot rechazado |
| 13 | MS-CONTEXT intenta lock/T/write desde otro context durante lock principal | S/M/SM; rechazo/bloqueo/survival |
| 14 | M-DB-MOD despues de S muta F-SRC | S; source ordering |
| 15 | un M-DB-MOD handler muta F-SRC y destination/linkage | SM; ambos verifiers |
| 16S/M/SM-SEND | MS-SEND agenda comando mutador antes del boundary | clase correspondiente; schedule/execute/C points |
| 16S/M/SM-CONTEXT | MS-CONTEXT agenda callback antes del boundary | idem |
| 16S/M/SM-IDLE | MS-IDLE arma handler one-shot antes del boundary | idem |

## 8. Probes nativos normativos y estado

### 02N

`EventClassId=N-DB-OPEN`; primary T modifica trigger; callback registra manager/count y trata de abrir el destination
Xrecord para write mediante el manager documentado. Mutacion S=`semantic value`. PASS requiere exact header verificado,
callback observado, T identity/affiliation demostrada y bytes staged/postcommit coherentes. Error de apertura, T distinta
o persistencia inesperada=FAIL. Header/firma/manager no verificable=UNKNOWN.

### 04N

Leg A usa N-DB-MOD para mutar destination S; el harness aborta primary T. Legs N-TX-ABOUT-ABORT/N-TX-ABORTED registran
counts y orden sin mutar el notifier. PASS requiere rollback completo y callbacks exactos; supervivencia=FAIL;
header/fase/identidad no verificable=UNKNOWN.

### 09N

- `09N-A`: N-TX-ABOUT-END, logging y write-attempt separado.
- `09N-B`: N-TX-COMMIT-BEGIN, write-attempt S/M/SM contra target controlado.
- `09N-C`: N-TX-ENDED, abre una nueva T sólo si la API exacta lo permite y registra resultado.
- `09N-D`: N-ED-END, mutacion en nueva T para distinguir command-end de transaction-end.

Ninguna leg se denomina commit-phase salvo `09N-B`, cuya autoridad publica dice que señala el inicio del commit exterior.
PASS exige header exacto y orden observado consistente; contradiccion=FAIL; falta de autoridad exacta=UNKNOWN.

### 10N-S / 10N-M / 10N-SM

Despues de R_post/M_post, el comando modifica un trigger antes de return para disparar N-DB-MOD. El callback muta,
respectivamente, destination Xrecord, transform/layer o sibling/linkage. Semantic/physical verifiers y lifecycle markers
determinan si la mutacion entra en un snapshot posterior o invalida P6/P11/P12. Header/callback no exacto=UNKNOWN.

### 16N-S / 16N-M / 16N-SM

No se identifico un scheduler ObjectARX 2025 exacto en headers locales. `AcEditorReactor` es notificacion, no mecanismo
de enqueue. Las tres variantes quedan `BLOCKED_UNDEFINED / UNKNOWN`: no tienen EventClassId de scheduling acreditado y
no pueden ejecutarse ni sustituirse con MS-SEND/MS-CONTEXT/MS-IDLE. Este estado hace
`NativeProbeCatalogClosed=false` y `HostFixtureContractClosed=false`.

## 9. Mapping exacto T1..T16

| Threat | Event/scheduler exacto | Probes | Estado V27 |
|---|---|---|---|
| T1 managed same-context | M-DB-OPEN, M-DB-MOD | 01,02,10*,15 | DEFINED |
| T2 native reactor | N-DB-OPEN/MOD/ERASE, N-TX-*, N-ED-* | 02N,04N,09N,10N*,16N* | OPEN: exact 2025 authority + 16N missing |
| T3 nested/new T | M-DB-MOD trigger + `TransactionManager.StartTransaction`; native N-DB-MOD candidate | 03,04,05,13; 04N | managed DEFINED / native OPEN |
| T4 commit-associated | N-TX-COMMIT-BEGIN candidate; M-DB-MOD=close-associated y M-DOC-END=command-ended solamente | 09N-B;09C/09E | OPEN por native authority; no falsa managed commit callback |
| T5 preread→commit | M-DB-MOD controlado dentro de primary T | 01,05 | DEFINED |
| T6 commit→postread | M-DOC-END y 09E sólo para post-command; N-TX-ENDED candidate | 09E,09N-C/D | OPEN para fase nativa exacta |
| T7 durante scan | M-DB-MOD entre A/B | 08,12 | DEFINED |
| T8 postverification→C_host | M-DB-MOD antes de return; N-DB-MOD candidate | 10S/M/SM;10N-S/M/SM | managed DEFINED / native OPEN |
| T9 NOD | M-DB-MOD muta F-NOD | 11,10S,16S-* | DEFINED para managed |
| T10 source after S | M-DB-MOD muta F-SRC | 14,16S-* | DEFINED para managed |
| T11 destination | M-DB-OPEN/MOD/ERASE/APPEND | 01,09C,10S/SM,15 | DEFINED para managed |
| T12 material-only | M-DB-MOD/ERASE; physical scan trigger | 10M,12,F1,F8,16M-* | DEFINED para managed primitives |
| T13 other context/command | MS-CONTEXT + M-LOCK-WILL/CHANGED/VETO | 13,16*-CONTEXT | DEFINED |
| T14 module load sin mutator | diagnostic load marker, sin protected write | diagnostic only | OUT con oraculo no-state-change |
| T15 module + mutator | controlled module registra un EventId exacto | registration + inherited probe | DEFINED sólo para EventId heredado |
| T16 deferred boundary | MS-SEND, MS-CONTEXT, MS-IDLE; native scheduler no identificado | 16*-SEND/CONTEXT/IDLE;16N* | managed DEFINED / native OPEN |

Un estado `DEFINED` describe el probe, no su futuro resultado. Cualquier fila `OPEN` impide
`EventCatalogClosed=true`.

## 10. Composiciones

```text
ComposedProbeRequired(A,B) iff
  A can schedule/trigger B or their shared T/lock/lifecycle ordering can produce
  a state not implied by independent A and B evidence.
```

| Composicion | Regla |
|---|---|
| T2+T4 | 09N-B sirve una vez si N-TX-COMMIT-BEGIN es el callback nativo commit-associated exacto; otros callbacks exigen probe compuesto |
| T2+T16 | REQUIRED, pero UNKNOWN hasta identificar scheduler nativo exacto; managed scheduling no sustituye |
| T15+T16 | modulo research controlado carga, registra M-IDLE/MS-SEND/MS-CONTEXT exacto, agenda, ejecuta, muta y verifica |
| T3+T8 | probe compuesto sólo si nested/new T puede sobrevivir o ejecutarse despues del snapshot final; 03+10* con ids |
| T13+T16 | probe compuesto para other context que agenda MS-CONTEXT/SEND/IDLE durante lock principal |

## 11. Predicados de cierre

```text
ManagedProbeCatalogClosed(M) iff
  every managed EventId/API/signature is exact-build verified
  AND every IN managed probe conforms to the schema
  AND no managed wildcard is used as evidence

NativeProbeCatalogClosed(N) iff
  every required NativeEventId exists in exact 2025 headers
  AND every callback signature/registration/phase is verified
  AND 02N,04N,09N,10N-S/M/SM,16N-S/M/SM are fully defined
  AND every required composed native probe is defined
  AND no UNKNOWN/BLOCKED_UNDEFINED remains

EventCatalogClosed(H) iff
  ManagedProbeCatalogClosed(M)
  AND NativeProbeCatalogClosed(N)
  AND every event used by a claimed host property is enumerated
  AND every required callback has a probe and complete oracle
  AND unavailable required classes produce UNKNOWN, never omission
```

Estado documental:

```text
ManagedProbeCatalogClosed(M) = TRUE
NativeProbeCatalogClosed(N) = FALSE
EventCatalogClosed(H) = FALSE
```

## 12. Invalidacion

Cambio de DLL/version/hash, SDK/header, callback signature, registration point, scheduler, event binding, schema de probe,
oraculo o threat composition invalida las filas afectadas. Un resultado futuro no se transfiere entre EventId.

## 13. Autoridades publicas Autodesk consultadas

- `Database.ObjectModified` managed y su correspondencia con `AcDbDatabaseReactor::objectModified`:
  <https://help.autodesk.com/cloudhelp/2024/ENU/OARX-ManagedRefGuide/files/OARX-ManagedRefGuide-Autodesk_AutoCAD_DatabaseServices_Database_ObjectModified.html>
- metodos `AcDbDatabaseReactor` y fases before/after de open/modify/erase:
  <https://help.autodesk.com/cloudhelp/2027/ENU/OARX-RefGuide/files/OARX-RefGuide-__MEMBERTYPE_Methods_AcDbDatabaseReactor.html>
- transaction reactors y `endCalledOnOutermostTransaction` como inicio de commit exterior:
  <https://help.autodesk.com/cloudhelp/2018/ENU/OARX-DevGuide/files/GUID-F6936D1E-6C7F-42AE-9EB2-92F4FD66D67E.htm>
- metodos `AcEditorReactor`, incluidos command lifecycle y modeless notifications:
  <https://help.autodesk.com/cloudhelp/2027/ENU/OARX-RefGuide/files/OARX-RefGuide-__MEMBERTYPE_Methods_AcEditorReactor.html>
- clases de reactor y notifiers:
  <https://help.autodesk.com/cloudhelp/2022/ENU/OARX-DevGuide/files/GUID-BB1A5826-9584-43A5-A1D5-BF64211EE7EA.htm>

Estas referencias fijan el significado publico de los nombres candidatos. No sustituyen los headers ObjectARX 2025
exigidos por `NativeProbeCatalogClosed`.
