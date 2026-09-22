# I-52 — Context Isolation Authority — investigación separada

**Starting I-52 SHA:** `89ae56137adac138acc522fae0717d4ef75abbe3`

**Base main:** `7097057cf8685bf5ecc09083cba37379d4a4aae8`

**Consensus Freeze V18:** `faaf709bf6401dcda91b07f41afe44f490fb916a` / blob
`c12fa65f5b6203061e8d717d1ceed595c4fed1f4`

**Proposal V18:** `1e9d7e39a50e5c6322e1d32823aa9f3130a2d0ae` / blob
`827559b504ec6bc8b5f780267a40766c3fa6db8c`

**Gate nature:** `RESEARCH ONLY / NOT CT-49R / NOT G3`

**Research result:** `PARTIAL_ONLY`

## 1. Alcance y pregunta

Esta investigación busca autoridades candidatas para el término congelado `ContextIsolationAuthority`. No modifica
`SafeOperationalState`, no implementa guardas, no ejecuta CT-49R y no convierte una observación en garantía. Se
examinaron las APIs managed y ObjectARX, garantías documentadas, inventarios de extensiones y módulos, entornos
controlados, contextos de ejecución, locks documentales y eventos/callbacks.

La pregunta de cobertura es si todo canal capaz de alterar la base fuente, representación observada, resolución de
símbolos o semántica de API consumida queda excluido, detectado con semántica completa o incorporado al
read-set/footprint. No se volvió a intentar cerrar el problema enumerando modos conocidos.

## 2. Jerarquía y clasificación de fuentes

Se usó esta precedencia: ObjectARX Reference, Managed .NET Reference, Developer Guide, SDK local, material oficial de
Autodesk/APS, observación runtime y fuentes suplementarias. Las etiquetas significan:

- `OFFICIAL CONTRACT`: una referencia de API declara una garantía o semántica concreta.
- `OFFICIAL DOCUMENTATION`: una guía oficial describe el modelo o uso, sin probar cobertura universal.
- `RUNTIME OBSERVATION`: resultado de un host/build concreto.
- `INFERENCE`: conclusión derivada que la fuente no promete literalmente.
- `COMMUNITY / NON-AUTHORITATIVE`: solo apoyo; no se usó para ninguna conclusión material.

No se encontró una declaración oficial de Autodesk que presente una API como registro completo de todos los canales
contextuales nativos, verticales y de terceros que pueden afectar las observaciones relevantes.

## 3. Fuentes externas revisadas

| Fuente | Año/versión | Clasificación | Afirmación usada |
|---|---|---|---|
| [Explicit Document Locking](https://help.autodesk.com/cloudhelp/2025/JPN/OARX-DevGuide/files/GUID-9FCD49BC-ADD0-44D2-A599-F625BB63BE22.htm) | AutoCAD 2025 | OFFICIAL CONTRACT | `kRead` exclusivo impide que otros contextos obtengan write lock y garantiza que el documento no se modifica durante el lock; `kXWrite` da acceso exclusivo del contexto a recursos documentales. |
| [Document Locking](https://help.autodesk.com/cloudhelp/2025/ESP/OARXMAC-DevGuide/files/GUID-57178B24-8CD5-4BBD-85A6-7F54BB07112F.htm) | AutoCAD 2025 | OFFICIAL DOCUMENTATION | Todo documento debe bloquearse para modificarlo; código modeless y trabajo fuera del documento activo deben bloquear explícitamente. |
| [DocumentLockMode](https://help.autodesk.com/cloudhelp/2018/ENU/OARX-ManagedRefGuide/files/OREFNET-Autodesk_AutoCAD_ApplicationServices_DocumentLockMode.html) | Managed API | OFFICIAL CONTRACT | `Read` excluye locks de escritura de otros contextos; `ExclusiveWrite` excluye todos los locks de otros contextos. |
| [AcApDocManager::lockDocument](https://help.autodesk.com/cloudhelp/2027/ENU/OARX-RefGuide/files/OARX-RefGuide-AcApDocManager__lockDocument_AcApDocument__AcAp__DocLockMode_ACHAR__ACHAR__bool.html) | API compatible con AutoCAD 2025 | OFFICIAL CONTRACT | Define recursos cubiertos, conflictos, veto y semántica de error; `kForRead` es exclusive read. |
| [Document Execution Contexts](https://help.autodesk.com/cloudhelp/2025/ESP/OARXMAC-DevGuide/files/GUID-C004A7D5-A867-412B-9B4C-CE12E4159D3A.htm) | AutoCAD 2025 | OFFICIAL DOCUMENTATION | Cada documento tiene contexto y stack propios, pero todos comparten heap y datos estáticos. |
| [AcApDocManager methods](https://help.autodesk.com/cloudhelp/2027/ENU/OARX-RefGuide/files/OARX-RefGuide-__MEMBERTYPE_Methods_AcApDocManager.html) | API compatible con AutoCAD 2025 | OFFICIAL CONTRACT | Distingue documento current/active, application context, input pendiente, activación y programación de callbacks. |
| [beginExecuteInApplicationContext](https://help.autodesk.com/cloudhelp/2027/ENU/OARX-RefGuide/files/OARX-RefGuide-AcApDocManager__beginExecuteInApplicationContext_void_procAddrvoid___void__.html) | API compatible con AutoCAD 2025 | OFFICIAL CONTRACT | Encola un callback para contexto de comando y cancela comandos pendientes antes de invocarlo; no promete aislamiento de plugins o callbacks. |
| [Transparent versus Modal Commands](https://help.autodesk.com/cloudhelp/2022/ENU/OARX-DevGuide/files/GUID-342764A2-CDF0-4B62-939B-2ED263678147.htm) | ObjectARX | OFFICIAL DOCUMENTATION | Un comando transparente puede ejecutarse durante otro; existe anidación de un nivel. |
| [Understand AutoCAD Events](https://help.autodesk.com/cloudhelp/2025/PLK/OARX-DevGuide-Managed/files/GUID-CC1C9D4A-9517-40BF-8D3D-CBC15C9646B9.htm) | AutoCAD 2025 | OFFICIAL DOCUMENTATION | Hay eventos de Application, Database, Document, DocumentCollection, Editor, Runtime y otros ámbitos. |
| [Guidelines for Event Handlers](https://help.autodesk.com/cloudhelp/2018/ENU/OARX-DevGuide-Managed/files/GUID-FE7D58D5-28A0-4C98-A876-D4D48F06D0B2.htm) | Managed API | OFFICIAL DOCUMENTATION | Un handler puede escribir otros objetos; callbacks dentro del flujo actual no quedan excluidos meramente por poseer el lock. |
| [DynamicLinker.GetLoadedModules](https://help.autodesk.com/cloudhelp/2022/ENU/OARX-ManagedRefGuide/files/OARX-ManagedRefGuide-Autodesk_AutoCAD_Runtime_DynamicLinker_GetLoadedModules.html) | Managed API | OFFICIAL CONTRACT | Devuelve módulos actualmente cargados; no promete inventario de todo código capaz de afectar el proceso ni inmovilidad posterior. |
| [acedArxLoaded](https://help.autodesk.com/cloudhelp/2019/ENU/OARX-RefGuide/files/OREF-AcEd_Global_Functions.html) | ObjectARX | OFFICIAL CONTRACT | Enumera aplicaciones ARX externas actualmente cargadas. |
| [Demand Loading](https://help.autodesk.com/cloudhelp/2022/ENU/OARX-DevGuide/files/GUID-DC378416-7BFD-46A9-9C84-5E4EABACBB92.htm) | ObjectARX | OFFICIAL DOCUMENTATION | Aplicaciones ausentes pueden cargarse al abrir objetos, invocar comandos o iniciar AutoCAD. |
| [SECURELOAD](https://help.autodesk.com/cloudhelp/2025/ENU/AutoCAD-LT/files/GUID-541566C6-2738-49DD-87C3-C1490E924A02.htm) | AutoCAD 2025 | OFFICIAL CONTRACT | Restringe archivos ejecutables por ubicación confiable; no es una allowlist inmutable de identidad o comportamiento. |
| [Automation activity](https://get-started.aps.autodesk.com/tutorials/design-automation/define-activity/) | APS | OFFICIAL DOCUMENTATION | Una Activity fija engine, AppBundles, comandos y entradas para un WorkItem headless. |
| [EnumProcessModulesEx](https://learn.microsoft.com/en-us/windows/win32/api/psapi/nf-psapi-enumprocessmodulesex) | Windows | OFFICIAL CONTRACT | Es snapshot falible, puede ser incorrecto si cambia la lista y omite módulos cargados como datafile. |
| [AppDomain.GetAssemblies](https://learn.microsoft.com/en-us/dotnet/api/system.appdomain.getassemblies) | .NET | OFFICIAL CONTRACT | Enumera assemblies del execution context del AppDomain actual, no todo código nativo o futuros loads. |

## 4. Hallazgos de Managed API

`Document.LockDocument(DocumentLockMode, ...)` es la superficie managed más fuerte encontrada. Su autoridad es
positiva pero limitada: controla locks de **otros execution contexts** sobre recursos documentales. Las variantes
`Read` y `ExclusiveWrite` tienen semántica de exclusión documentada y errores observables.

No cubre por sí sola:

1. handlers/reactores invocados dentro del mismo contexto de ejecución;
2. heap y estado estático compartido por contextos documentales;
3. estado por usuario/perfil/proceso no incluido entre recursos del documento;
4. overrules o código ya cargado que altere consultas sin escribir la base;
5. carga de extensiones después de una inspección; ni
6. toda semántica de verticales o terceros.

`DocumentCollection.ExecuteInApplicationContext` y `ExecuteInCommandContextAsync` cambian el lugar o momento de
ejecución. No contienen una garantía de aislamiento. `Editor.IsQuiescent`, `Application.IsQuiescent`, command state,
known detectors y long transactions siguen siendo componentes parciales según V18.

## 5. Hallazgos ObjectARX/native

`AcApDocManager::lockDocument` es la misma autoridad de lock bajo la capa managed. Su documentación define alcance y
fallos con más precisión: recursos incluyen bases asociadas al documento, objetos, variables residentes en DB,
variables documentales y Transaction Manager. Esa lista permite afirmar qué cubre; también muestra que no es una
autoridad universal sobre el proceso.

`AcApDocManager` expone current/active document, application context, activación, input pendiente y callbacks en
contexto. `AcEdCommandStack` y command flags describen comandos registrados, modales y transparentes. Reactores de
editor/documento/dynamic linker notifican eventos. Ninguna referencia hallada afirma que estas superficies enumeren
todos los editores contextuales, callbacks modeless, canales COM, extensiones o trabajos diferidos.

El SDK ObjectARX local no estaba instalado: no se encontró `accmd.h` bajo `C:\Autodesk` ni
`C:\Program Files\Autodesk`. Por ello no se eleva una inferencia de nombres de headers a contrato. Un shim C++ futuro
podría acceder a `AcApDocManager`, `acedArxLoaded` y linker reactors, pero sería un mecanismo de observación, no una
prueba de cobertura por sí mismo.

## 6. Garantías oficiales: resultado

La única garantía fuerte encontrada es la exclusión documental entre execution contexts mediante lock. No se halló
una garantía oficial de que:

- todo código capaz de cambiar observaciones de RACKMIRROR deba usar un contexto distinto;
- todos los callbacks de terceros queden suspendidos durante el lock;
- todo cambio de semántica observada sea una escritura de recurso documental;
- todo módulo/assembly/extensión cargable aparezca en un único inventario estable; o
- no pueda cargarse código después de comprobar un inventario.

Por tanto no hay argumento oficial que cierre `ContextIsolationAuthority` completo.

## 7. Inventario de extensiones y plugins

`DynamicLinker.GetLoadedModules`, `acedArxLoaded` y `acrxLoadedApps` enumeran subconjuntos definidos de módulos o apps
actualmente cargados. Linker reactors pueden notificar loads/unloads ObjectARX. El registro de demand-loading permite
descubrir aplicaciones registradas, pero AutoCAD admite carga manual, demand load, .NET, AutoLISP, VBA y DLLs. La
documentación no promete que un solo inventario reúna todos esos canales.

`SECURELOAD=2` restringe ubicación a `TRUSTEDPATHS`; no fija hashes, no impide que otro archivo confiable se cargue y
no convierte los directorios de Autodesk en una allowlist mínima. La carga bajo demanda puede ocurrir después de una
comprobación inicial. Resultado: inventario útil para diagnóstico y posible scope de un entorno administrado, pero
`PARTIAL_ONLY`.

## 8. Proceso y módulos

`Process.Modules`/`EnumProcessModulesEx` y `AppDomain.GetAssemblies` producen snapshots. La documentación de Windows
advierte carreras durante carga/descarga y omisión de módulos `LOAD_LIBRARY_AS_DATAFILE`; el inventario managed solo
cubre el execution context consultado. Ninguno impide cargas posteriores. Un doble snapshot puede detectar algunos
cambios, pero no prueba que ningún código intermedio ejecutó o que todo canal relevante esté representado.

No había proceso `acad.exe` activo durante esta investigación, por lo que no se capturó un snapshot que pudiera
confundirse con autoridad. Se verificaron estáticamente las identidades del build instalado; están en el artefacto de
evidencia y coinciden con G3A.

## 9. Entornos controlados

### 9.1 Core Console local

`accoreconsole` reduce UI interactiva y paletas, pero conserva engine, scripts, módulos y AppLoad. No se encontró un
contrato oficial que garantice una allowlist cerrada e inmutable de código durante toda la ejecución. Además, el
contrato vigente de RACKMIRROR es interactivo y requiere adquisición de eje; Core Console no es un sustituto automático
del producto.

### 9.2 Perfil limpio / `SECURELOAD` / paths de solo lectura

Estas medidas reducen superficie y pueden formar parte de un laboratorio reproducible. No excluyen módulos Autodesk,
verticales, código firmado/confiable, demand loading permitido o inyección a nivel de proceso. Son defensa de entorno,
no autoridad completa.

### 9.3 APS Automation

Una Activity fija engine, AppBundle y command line para un WorkItem. Es el candidato de aislamiento operacional más
controlado encontrado, pero la documentación revisada no promete que el AppBundle sea el único código relevante dentro
del engine. Es headless, remoto y no corresponde al flujo interactivo de AutoCAD 2025 congelado. Adoptarlo requeriría
otra decisión de producto y no se importa en I-52.

## 10. Contexto de aplicación y ejecución

`ExecuteInApplicationContext` solo ejecuta una función en application context. La variante `beginExecute...` encola
trabajo y, para command context, espera una oportunidad del host. `sendStringToExecute` y mecanismos relacionados
pueden dejar trabajo pendiente o ejecutarlo más tarde. Estas APIs describen despacho y ownership, no exclusión de todo
trabajo modeless o diferido.

`CurrentDocumentOwned` y `CommandBoundaryCompatible` pueden consumir estas superficies bajo sus propios términos del
predicado congelado. No satisfacen el último término por composición implícita.

## 11. Eventos, callbacks y quiescence

La API ofrece eventos en Application, Runtime, DocumentCollection, Document, Database y Editor, y reactors nativos.
Eso permite observar eventos conocidos. La guía managed permite que un handler escriba otros objetos, de modo que un
callback disparado por el propio comando puede ejecutar trabajo dentro del contexto que posee el lock.

No se encontró una autoridad para enumerar todas las suscripciones de terceros, demostrar que no queda ningún callback
idle/synchronization/COM pendiente o vetar todo callback relevante. `IsQuiescent`, input pendiente y command events no
cubren trabajo que todavía no se ha encolado o que vive fuera del canal observado.

## 12. Matriz de autoridades candidatas

| CandidateId | Authority/API | Layer | Source / símbolo | Scope | Qué prueba | Qué no prueba | Fallo | Coverage | Test adversarial | Coste / privilegio / portabilidad | Veredicto |
|---|---|---|---|---|---|---|---|---|---|---|---|
| CIA-01 | Exclusive document lock | managed/native | `Document.LockDocument(Read/ExclusiveWrite)`; `AcApDocManager::lockDocument(kRead/kXWrite)` | AutoCAD document execution contexts; exact semántica documentada | Excluye locks incompatibles de otros contexts; protege recursos documentales declarados | Mismo-contexto, estado global, overrules, módulos, semántica de API no documental | lock conflict/veto/error observable | Solo recursos y contexts definidos | Sí | Medio; sin admin; Windows/AutoCAD | `PARTIAL_ONLY` |
| CIA-02 | Application/command context dispatch | managed/native | `ExecuteInApplicationContext`, `beginExecuteInCommandContext`, `isApplicationContext` | Host/MDI activo | Contexto y momento de callback | Aislamiento, callbacks de terceros, deferred work futuro | estados de error definidos | No | Sí | Medio; sin admin; AutoCAD | `INSUFFICIENT` |
| CIA-03 | Dynamic linker + ARX inventory/reactors | managed/native | `GetLoadedModules`, `acedArxLoaded`, `acrxLoadedApps`, linker reactor | Módulos ObjectARX observables en instante | Apps/módulos actuales y algunos loads/unloads | Managed/LISP/VBA/COM/proceso completo; carga previa/intermedia no observada; conducta | API failure / race / late load | No inventario universal documentado | Sí | Alto si shim; sin admin; Windows | `PARTIAL_ONLY` |
| CIA-04 | Process/AppDomain inventory | host/process | `EnumProcessModulesEx`, `Process.Modules`, `AppDomain.GetAssemblies` | Proceso exacto, snapshot | Módulos o assemblies visibles al mecanismo | Datafile modules, carrera, loads posteriores, conducta/semántica | API failure o snapshot inconsistente | No | Sí | Medio; permisos de proceso; Windows/.NET | `PARTIAL_ONLY` |
| CIA-05 | Secure local profile | host/config | `SECURELOAD`, `TRUSTEDPATHS`, readonly paths, clean profile | Instalación administrada exacta | Restringe orígenes de carga | Identidad/behavior de todo trusted code; módulos Autodesk; inyección; loads futuros | configuración mutable/lectura fallida | No | Sí | Alto; puede requerir admin/CAD manager; Windows | `PARTIAL_ONLY` |
| CIA-06 | APS Automation Activity | controlled host | engine + AppBundle + Activity + WorkItem | Engine cloud exacto | Inputs, bundle y command line declarados | Exclusividad interna contractual; paridad interactiva; eje/UX local | workitem failure | Parcial; no garantía de único código hallada | Sí | Alto; cuenta/coste/red; no interactivo | `NOT_APPLICABLE` al producto actual |
| CIA-07 | Event/reactor fence + pre/post fingerprints | managed/native/model | eventos Database/Document/Runtime/Editor + read-set | Canales suscritos y observaciones modeladas | Detecta cambios conocidos e incorpora huellas | Suscripciones no enumerables, cambios sin evento cubierto, callbacks futuros | evento/gap/read failure => reject | No sin registro completo | Sí | Alto; sin admin; AutoCAD | `PARTIAL_ONLY` |

### 12.1 Cobertura contextual por candidato

| Candidate | Native contexts | Modeless | Nested | Transparent | Between-command | Vertical | Third-party | Deferred work |
|---|---|---|---|---|---|---|---|---|
| CIA-01 | Solo locks de otros contexts | Parcial si pide lock | Parcial | Parcial | Durante el lock | No semántica propia | Solo si respeta locks; mismo-contexto no | No prueba ausencia |
| CIA-02 | Identifica context | No excluye | Describe comando | Describe comando | Encola trabajo | No | No | Puede crear deferred work |
| CIA-03 | ObjectARX visible | No | No | No | Snapshot/reactor | Parcial | Parcial | Solo load/unload observado |
| CIA-04 | Módulos visibles | No | No | No | Snapshot | Parcial | Parcial | No |
| CIA-05 | Reduce carga permitida | Parcial | No | No | Config persistente | Parcial | Parcial | No |
| CIA-06 | Headless controlado | No UI | N/A | N/A | WorkItem | Engine seleccionado | AppBundles declarados | Job lifecycle, no contrato interno completo |
| CIA-07 | Eventos registrados | Parcial | Parcial | Parcial | Solo canales observados | Sin completitud | Sin completitud | Parcial |

Ninguna fila tiene argumento de cobertura disponible para todos los canales. Todos los candidatos son
adversarialmente testeables en partes, pero una batería verde seguiría siendo observación y no contrato universal.

## 13. Matriz adversarial

| Caso | CIA-01 lock | CIA-03/04 inventario | CIA-05 entorno | CIA-07 eventos | Gap resultante |
|---|---|---|---|---|---|
| Editor persistente no conocido | Impide otra escritura con lock; no identifica semántica | Puede no crear módulo nuevo | Puede venir del host permitido | Sin evento universal | No cerrado |
| Paleta modeless que modifica DB | Su write lock debería entrar en conflicto | Módulo puede aparecer | Puede estar permitida | Puede observar escritura | Mismo-contexto o API no conforme sin cobertura |
| Comando nested/transparente | Lock ayuda entre contexts | No modela stack | No lo excluye | Command events parciales | No cerrado |
| Cambio de documento | Lock pertenece al documento | Snapshot no vincula ownership | No lo impide | DocManager events ayudan | Detectable, no aislamiento completo |
| Editor vertical | Solo recursos documentales | Módulo quizá visible | Vertical puede ser parte del host | Eventos sin semántica | No cerrado |
| ARX tercero | Lock limita writes de otros contexts | ARX visible en algunos inventarios | Puede bloquear paths no confiables | Linker reactor observa algunos loads | Same-context behavior sigue abierto |
| Plugin managed con `DocumentLock` | Conflicto observable si otro context | Assembly visible en AppDomain | Puede estar en trusted path | Lock events ayudan | Load/behavior future abierto |
| Idle callback | No prueba que no exista; si escribe deberá lockear según contrato | Inventario no enumera callback | No | Puede no existir registro global | No cerrado |
| Queued command | No prueba cola vacía | No | No | input/command events parciales | Trabajo futuro abierto |
| Modificación retardada | Protege solo duración del lock | Snapshot no ayuda | No | Detecta solo si suscrito | Después del lock queda abierta |
| ObjectARX reactor | Puede correr en mismo contexto | Módulo quizá visible | Puede ser permitido | No hay enumeración completa | Gap crítico |
| COM automation | Debe bloquear para escribir según guía, pero canal externo persiste | No necesariamente en AppDomain/ARX | No excluido | Lock events parciales | Gap de cobertura |
| Código cargado después del inventario | Lock no impide load | Rompe snapshot | `SECURELOAD` solo restringe origen | Linker cubre subconjunto | Gap crítico |

## 14. Probes y evidencia exacta

No se inició `acad.exe` ni `accoreconsole` para fabricar una ausencia observada. AutoCAD no estaba ejecutándose. Un
run limpio solo demostraría un estado concreto y no respondería la pregunta de cobertura.

Se ejecutó una inspección estática reproducible del host instalado:

- `acad.exe`: `R25.0.171.0.0`;
- `AcCoreMgd.dll`, `AcDbMgd.dll`, `AcMgd.dll`: `25.0.171.0.0`;
- hashes SHA-256 y tamaños registrados;
- búsqueda de `accmd.h`: no encontrado en las raíces Autodesk instaladas;
- proceso `acad`: no activo.

Artefacto: [`I-52-context-isolation-research-evidence.json`](../automation/evidence/I-52-context-isolation-research-evidence.json).
Su SHA-256 al producirse fue `88B49A8105A2AA49F89DB4299A16A992285C60F2E01E4CC423528E71426B6345`.

La evidencia G3A (`I-52-g3a-autocad-runtime-probe.json`) se cita como histórica para el SHA
`560ec72a1cef16e18da5dd92a92eb5fb2af26469`. No se transfiere a este gate ni a un futuro CT-49R.

## 15. Gaps exactos

1. No existe fuente oficial hallada que obligue a todo callback relevante a ejecutar en otro context sujeto al lock.
2. No hay inventario único y contractual de ObjectARX, managed, LISP, VBA, COM, verticales y módulos nativos.
3. No hay freeze/load fence oficial que impida carga posterior durante la operación interactiva.
4. No hay autoridad pública para enumerar todo trabajo idle, synchronization, queued o diferido.
5. Un inventario dice qué está cargado, no qué semántica altera ni qué callbacks registró.
6. Locks documentales no cubren todo estado estático/de proceso ni toda representación sin escritura de DB.
7. Un entorno headless controlado no conserva automáticamente el contrato interactivo de RACKMIRROR.
8. No se obtuvo contrato Autodesk sobre composición de locks + reactors + inventario como aislamiento total.

Estos gaps describen el alcance investigado; no prueban imposibilidad universal.

## 16. Mejores mecanismos parciales

**Primero:** CIA-01, lock documental exclusivo. Es la única garantía normativa encontrada que excluye una clase amplia
y definida de interferencias. Debe conservarse como componente futuro de una autoridad, sujeto a compatibilidad con
snapshot/mutate y a prueba de callbacks dentro del mismo contexto.

**Segundo:** CIA-03 + CIA-07, inventario/linker reactors y fingerprints/eventos. Pueden detectar cambios y hacer
fail-closed ante loads o escrituras observadas, pero carecen de argumento de completitud.

**Tercero:** CIA-05, perfil administrado con `SECURELOAD=2`, paths mínimos y de solo lectura. Reduce superficie, pero
no la cierra contractualmente.

Ninguno, solo o combinado con la evidencia disponible, alcanza `PROMISING_FOR_CT49R` porque el gate futuro empezaría
sin una fuente que pueda demostrar cobertura de same-context callbacks, todo código cargable y deferred work.

## 17. Readiness de CT-49R

`CT-49R = NOT READY`. Antes de abrirlo hace falta al menos una de estas entradas nuevas:

1. contrato oficial Autodesk que delimite todos los callbacks/contexts sujetos a `AcApDocManager` locks y el tratamiento
   de same-context reactors;
2. autoridad oficial de load freeze/inventario completo para el proceso interactivo;
3. host controlado con una garantía verificable de allowlist inmutable y compatibilidad explícita con el producto; o
4. decisión arquitectónica nueva que cambie el alcance a un entorno batch controlado, seguida de Proposal/freeze
   propios; esta investigación no toma esa decisión.

Con cualquiera de esas entradas, CT-49R deberá probar fuente, predicado, build scope, fallos, canales, cobertura y todos
los casos adversariales. Esta investigación no es ese gate.

## 18. Implicaciones y contradicciones materiales

No apareció contradicción material con Proposal V18, Freeze V18 o ADR-0036. Los hallazgos confirman la separación de
sus términos: document ownership/locks, command boundary, detectors, long transactions, overrules y context isolation
no son equivalentes.

No se debilita fail-closed ni se redefine `SafeOperationalState`. El valor vigente permanece:

```text
ContextIsolationAuthority = UNKNOWN
SafeOperationalState = FALSE_FOR_ADMISSION
```

I-55 se observó en `15ad90aae749ce8bce78b5f59bca28e27097e86a`. Su avance no cambia Foundation,
session/context authority, AUTH-15 ni supuestos de I-52: impacto contractual `NON-MATERIAL`, coordinación
`MATERIAL / OBSERVED`. No se importa política I-55.

## 19. Recomendación

Cerrar esta investigación con `RESULT B / PARTIAL_ONLY`. No abrir CT-49R todavía. El siguiente gate recomendado es una
**adquisición de autoridad externa**: solicitar a Autodesk una respuesta contractual sobre exclusión mediante locks,
same-context reactors, carga de extensiones y trabajo diferido, o evaluar formalmente una garantía de host controlado.
Solo si esa entrada aporta un argumento de cobertura se prepara CT-49R para caracterizarla.

```text
CONTEXT ISOLATION RESEARCH = COMPLETE
CANDIDATE AUTHORITY = PARTIAL ONLY
CT-49R = NOT READY
ContextIsolationAuthority = UNKNOWN
SafeOperationalState = FALSE_FOR_ADMISSION
G3 = STOPPED
G3B = NOT OPEN
CT-50 = NOT EXECUTED
SUBSTANTIVE IMPLEMENTATION = BLOCKED
```
