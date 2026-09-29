# I-52 AUTH-15 — Registro de decisiones de la unidad de integracion

Unit: `I-52-AUTH15` (iniciativa I-52). Rama: `feature/i52-auth15-definition-creator`. Workflow: V2.

Contrato de la unidad: [I-52-auth15-definition-creator.md](../../initiatives/I-52-auth15-definition-creator.md).

## 1. Autorizacion del Coordinador

```text
Coordinator = AUTH-15 IMPLEMENTATION AUTHORIZED
Integration unit = feature/i52-auth15-definition-creator
Base = 016bf46715cec45e644f88a22ef091b311a2bef1
§148/§156 production exclusion = LIFTED FOR AUTH-15 ONLY
AUTH15-DEV-01 = BlockNameUnavailable omitted from implementation because the existing
                family naming loops expose no reachable/distinguishable exhaustion result.
                ARCHITECT REVIEW REQUIRED.
```

- **Segunda unidad de I-52.** El Coordinador autoriza expresamente esta rama como segunda unidad de integracion
  de I-52, de proposito unico. No contradice la regla «1 iniciativa = 1 rama»:
  - su funcion es aislar AUTH-15 de la rama de investigacion CT-DA de I-52, que esta bloqueada;
  - nace de `origin/main`;
  - solo implementa e integra AUTH-15 y no absorbe otro trabajo de I-52.
- **Exclusion levantada solo para AUTH-15.** La exclusion de codigo de produccion de §148 y §156 del registro
  de I-52 queda levantada unicamente para AUTH-15. El comando RACKMIRROR y el resto del producto de I-52 siguen
  BLOQUEADOS en su propia rama.

## 2. Reclamo y clasificacion Workflow

- **Reclamo.** Commit vacio `4b20bda3`, aceptado en el primer push sin force, con
  `Claim-Id: 1f035b6f-6891-48fd-8965-f7ff1f522767`.
- **Clasificacion: V2, caso T4.** La base contiene el `WORKFLOW_V2_EFFECTIVE_SHA` y no hay pausa activa.
  - La pausa de activacion termino segun el registro durable del tag `integration/I-56`
    (`Claim pause: end=2026-09-17T22:30:00Z`), que es el registro que exige WORKFLOW §11.7.
  - La linea de `docs/HANDOFF.md` en `main` que aun dice «pausa ACTIVE» esta desfasada respecto de ese tag. No
    se corrige aqui (ver §3).
- **Orden.** El codigo de la unidad se redacto en local antes del reclamo, por orden previa del Coordinador. En
  el historial versionado el orden es el de WORKFLOW §2: reclamo, bootstrap (contrato, este registro y la fila
  de ROADMAP) y despues la implementacion.

## 3. HANDOFF

La orden pedia una entrada en decisiones y HANDOFF. **`docs/HANDOFF.md` no se toca en esta sesion**: WORKFLOW
lo reserva a la sesion de integracion, como ultimo commit de la rama (tabla de archivos calientes y de
registro). El registro durable de la unidad es este archivo, junto con el contrato y la fila de ROADMAP del
bootstrap (momento 2 de WORKFLOW §2). HANDOFF se actualizara en el commit de cierre de la integracion.

## 4. Contrato implementado y desviacion AUTH15-DEV-01

- **Contrato.** Se implemento el de la revision de Arquitecto «AUTH-15 DESIGN / FREEZE REVIEW», resumido en el
  contrato de la unidad.
- **Superficie.** Dos altas en el Plugin (`RackDefinitionCreator`, `RackDefinitionCreationResult`) y guardas de
  fuente. No se modifica ningun archivo de produccion existente.
- **AUTH15-DEV-01.** La revision listaba el fallo tipado `BlockNameUnavailable`, y la implementacion lo omite.
  - **Por que.** Ninguno de los dos creadores de familia expone un agotamiento de nombres alcanzable y
    distinguible:
    - el bucle de `LateralHeaderDrawer.UniqueBlockName` no tiene cota;
    - el de `CantileverViewMaterializer.UniqueBlockName` solo lanza `InvalidOperationException` tras
      `int.MaxValue` candidatos, indistinguible de cualquier otra `InvalidOperationException`.
  - **Que pasaria si ocurriera.** Saldria como `WriteFailed` con su diagnostico.
  - **Estado.** Por orden del Coordinador, no se restaura ni se cambia el comportamiento de produccion para
    resolverla. Resuelta en §5.

## 5. Revision exacta del Arquitecto sobre `fed44e56` y correccion C-1..C-5

```text
ARCHITECT_VERDICT = CHANGES REQUIRED (1 blocker, 2 major, 4 minor)
EXACT_SHA_REVIEWED = fed44e564a3efc0544fb07c1a1529ca6167300aa
AUTH15_DEV_01 = ACCEPTED / CONTRACT NORMALIZATION
```

- **B-1 (bloqueante).** `Precheck` comparaba `ReferenceEquals(database.TransactionManager.TopTransaction,
  transaction)`.
  - En AutoCAD 2025, el getter de `TopTransaction` construye un wrapper gestionado **nuevo** en cada lectura
    (`newobj Transaction(IntPtr, false)` en `AcDbMgd.dll`). La comparacion por referencia era siempre falsa y toda
    llamada devolvia `TransactionMismatch`.
  - Las 16 guardas y el CI pasaron, porque la guarda solo buscaba el token `TopTransaction`: las guardas de fuente
    (ADR-0003) prueban la forma, no el comportamiento. Por eso la validacion en host es obligatoria antes de
    Candidate.
  - **Correccion C-1.** Si la base de datos o la transaccion son nulas o estan dispuestas: `TransactionMismatch`.
    Despues, `top = database.TransactionManager.TopTransaction`; si `top` es nulo o `top.UnmanagedObject !=
    transaction.UnmanagedObject`: `TransactionMismatch`. Mismo diagnostico. La identidad es la nativa (la que
    comparan `DisposableWrapper.Equals` y `==`). La `OpenCloseTransaction` sigue sin admitirse. Ningun otro cambio
    de comportamiento de produccion.
- **M-1 → C-3.** Nueva guarda sobre los cuerpos exactos de los escritores delegados:
  - `LateralHeaderDrawer`: `CreateSystemBlock`, `NewBlock`, `AppendInstance`, `AppendDimension`,
    `ResolveDimStyle`, `EnsureAnnotationLayer`, `ApplyDynamicParameters` y `UniqueBlockName`;
  - `CantileverViewMaterializer`: `CreateBlockDefinitionNamed`, `AppendCurves`, `EnsureRoleLayers`,
    `UniqueBlockName` y `Sanitize`;
  - `LayerHelper.EnsureLayer`;
  - `RackBlockData.Write` y `Read`.

  La guarda localiza cada cuerpo por su firma exacta (bloque o expresion) y falla si no la encuentra o si esta
  repetida. Nunca escanea el archivo entero: `LateralHeaderDrawer.PurgeUnreferenced` abre y commitea legitimamente.
- **M-2 → C-4.** La fuente de AUTH-15 no puede contener `Dispose(`, `using (` ni `using var`: disponer la
  transaccion sin commit la aborta, y su vida es del llamador.
- **C-2.** La guarda de la transaccion exige `database.IsDisposed`, `UnmanagedObject` y `TopTransaction`, y prohibe
  `ReferenceEquals(`.
- **AUTH15-DEV-01 = ACCEPTED / CONTRACT NORMALIZATION.** Ningun creador de familia expone un agotamiento de nombres
  alcanzable y distinguible. `BlockNameUnavailable` sale de la lista normativa de fallos, sin codigo muerto; si
  alguna vez ocurriera, saldria como `WriteFailed`.
- **Menores m-1..m-4 → C-5 (contrato).** Clases de fallo PRE-WRITE / POST-WRITE, alcance de `InvalidBlockName` y de
  `InvalidPlan`, y semantica representativa de `MissingInstances`. m-4 (`database.IsDisposed`) queda cubierta por
  C-1.
- **Diseno de la validacion en host.** Queda registrado en la revision del Arquitecto. El arnes **no** se crea en esta
  compuerta: requiere orden propia del Coordinador.

## 6. Re-revision del delta y arnes de validacion en host

```text
ARCHITECT_DELTA_VERDICT = APPROVED
EXACT_SHA_REVIEWED = 2d10de705fffee3dc03473c2d4c76bff489fc13e
B1 = CLOSED; C-1..C-5 = SATISFIED
HOST_VALIDATION_AUTHORIZATION_READY = YES
```

- **Observaciones menores del Arquitecto.**
  - Los helpers de entidad de Cantilever (`Build`, `BuildCircle`, `BuildPolyline`, `ApplyRole`, `ApplyByBlock`) no estan en la
    lista de C-3: no reciben `Transaction` ni `Database`, asi que no pueden commitear ni colocar. Ampliar la guarda es opcional.
  - La fila de ROADMAP y la linea de estado del contrato estaban desfasadas: se actualizan en el commit del arnes.
- **Decision del Coordinador.** El arnes de validacion en host vive TEMPORALMENTE en esta misma rama, en
  `eng/research/I52Auth15Host/`, y se revierte antes de Candidate. No modifica `src/` ni `tests/`.
- **Regla vinculante.** El commit del arnes deja `src/` y `tests/` byte-identicos a `2d10de70`.
  `build-hostval.ps1` lo comprueba (`git diff --quiet` y arboles iguales) y ademas exige que el commit del arnes solo toque
  `eng/research/I52Auth15Host/` y `docs/`. La evidencia registra `IMPLEMENTATION_SHA`, los arboles `src` y `tests` de la
  implementacion y del commit del arnes, y `treesEqual`.
- **Alcance del arnes.** Un unico comando `I52AUTH15_HOSTVAL`; el Plugin de producto no se NETLOADea, se carga como dependencia
  desde la carpeta versionada; las superficies internas de AUTH-15 se invocan por reflexion, sin `InternalsVisibleTo`. Casos
  HV-00..HV-14 segun el dictamen del Arquitecto; `EnvelopeWriteFailed` y el `InvalidEnvelope` por fallo de serializacion se
  declaran `NOT_EXERCISABLE_WITHOUT_FAULT_INJECTION` y no se les da PASS.
- **Precision de diseno.** Cada caso opera sobre una base de datos lateral propia del arnes, con `WorkingDatabase` apuntada a
  ella durante el caso y restaurada despues; un `.dwg` de trabajo en blanco (aportado por el Owner) solo ancla `WorkingDatabase`
  y nunca se escribe. El llamador de AUTH-15 en cada caso es el arnes, que abre, aborta o commitea la transaccion.
- **Precisiones del arnes que el Arquitecto no fijo** (registradas para su revision de la evidencia):
  - `run.scr` pone `FILEDIA` a 0 alrededor del `NETLOAD` y lo restaura a 1 antes de `QUIT` (como el driver de I52Ctda);
  - el lanzador copia el `.dwg` en blanco a `out\` y prueba que su hash no cambia;
  - el lanzador rechaza `RACKCAD_AUTOLOAD_PRESENT` y un `acad.exe` ya en ejecucion;
  - `null database` y `OpenCloseTransaction` se registran como caracterizaciones fuera del veredicto.
- **Estado.** El paquete se construye y se verifica; **no se ejecuta AutoCAD**. La ejecucion en host requiere autorizacion
  propia del Coordinador y la confirmacion del Owner de que no toca el equipo. No se toca `TRUSTEDPATHS` ni `SECURELOAD`.

## 7. Revision exacta del arnes (`8411381f`) y correccion HC-1..HC-4

```text
ARCHITECT_HARNESS_VERDICT = CHANGES REQUIRED
EXACT_HARNESS_SHA_REVIEWED = 8411381fac7800969313f81704d1ef24496be678
IMPLEMENTATION_BINDING = HOLDS (2d10de70; arboles src y tests iguales)
```

- **Ruling sobre AutoCAD durante la construccion.** ACEPTADO: construir no arranca ni carga AutoCAD; el rechazo que importa es el
  del lanzamiento. `TRANSFER-METADATA.json` registra `acadRunningDuringBuild`.
- **Desviaciones D-HV-01, 03, 04, 05:** ACEPTADAS (DWG en blanco aportado por el Owner y copiado a `out\`; rechazo de autoload
  RackCad; rechazo de ficheros no listados en `run\`; observaciones extra fuera del veredicto).
- **D-HV-02 (FILEDIA): CAMBIO REQUERIDO** (bloqueante B-1). `run.scr` restauraba una constante 1 y nada capturaba el valor original
  ni lo verificaba. Correccion HC-1: captura del original, `before/during/after` en `out\filedia.txt`, restauracion inmediata del
  valor EXACTO, el comando solo corre si `after == before`, la comprobacion estricta en el arnes antes de HV-00 (y al final) y el
  cruce en el lanzador; el lanzador nunca escribe FILEDIA e imprime el original en caso de corte.
- **M-1 -> HC-2.** La evidencia malformada ya no aborta el lanzador: `evidenceParseable=false`, todas las comprobaciones derivadas de
  la evidencia son falsas, y `launcher-record.json` se escribe siempre.
- **M-2 -> HC-3.** Codigos de salida explicitos: 0 = lanzamiento valido y PASS; 2 = INVALID (incluido un codigo de salida de AutoCAD
  distinto de cero aunque la evidencia diga PASS); 3 = lanzamiento valido con veredicto FAIL o UNKNOWN. Ultima linea:
  `RUN_RESULT = PASS | INVALID | FAIL | UNKNOWN`.
- **Menores -> HC-4.** m-1 evidencia progresiva y atomica tras cada caso; m-2 el enlace exige tipos de parametros exactos, un mismo
  tipo de resultado y exactamente 2 sobrecargas; m-3 identidad de Application contra el arnes, contra la referencia del Plugin y
  contra `SHA256SUMS` y los metadatos; m-4 el lanzador cruza los hashes de arnes, Plugin, Application y Domain, `harnessSha`,
  `implementationSha`, arboles y `SHA256SUMS`; m-5 SNAP incluye linetype, regapp, UCS, view y viewport; m-6 `matchesExpectation`
  en las observaciones extra; m-7 la construccion rechaza ficheros fuente/config IGNORADOS fuera de bin/obj; m-8 firma DWG del
  dibujo en blanco; m-9 preflight de solo lectura de `TRUSTEDPATHS`/`SECURELOAD` (`TRUSTEDPATH_REQUIRED` /
  `TRUSTEDPATH_UNDETERMINED`).
- **Regla de fugas.** Tras el abort del llamador, CUALQUIER objeto o estado que sobreviva es FAIL (incluidos bloques anonimos `*D`),
  sin normalizar ni reclasificar; el arnes enumera cada clave y handle filtrado. El dictamen sobre una fuga es del Arquitecto,
  despues de la corrida.
- **Pruebas sin AutoCAD.** `eng/research/I52Auth15Host/offline/` caracteriza el lanzador y la logica de FILEDIA con un sustituto de
  `acad.exe`. **No es evidencia de host** y no entra en ningun paquete.

## 8. RUN-1: primera corrida gobernada de host (INVALID / DEFECTO DEL ARNES)

```text
RUN-1 = INVALID / HARNESS DEFECT
Harness SHA = 9674dcc91fdcb3b371f5511acd506c18266f8bc4
Implementation SHA = 2d10de705fffee3dc03473c2d4c76bff489fc13e
PID = 29588
Cause = GetSystemVariable("PROFILENAME") -> eInvalidInput (AutoCAD 2025); the profile variable is CPROFILE
HV-00 = UNKNOWN
HV-01..HV-14 = NOT RUN
AUTH15_CALLS = 0
FILEDIA = RESTORED 1 -> 0 -> 1 (live 1 at harness start and end)
AutoCAD exit = 0 / process gone / no timeout
Scratch = UNCHANGED
```

- **No es evidencia de host de AUTH-15 y no se publica como tal.** Ninguna llamada a AUTH-15 se ejecuto; el defecto es del arnes.
  Los artefactos de la corrida quedan en `D:\I52-AUTH15-HV\9674dcc9\out` (evidencia, registro del lanzador, log, `filedia.txt`).
- **Regla de re-ejecucion.** HV-00 llego a empezar, asi que la re-ejecucion preautorizada NO aplicaba: una nueva corrida exige
  autorizacion nueva del Coordinador.
- **Lo que la corrida SI probo en AutoCAD real** (sobre el andamiaje, no sobre AUTH-15): la comprobacion de `TRUSTEDPATHS`, `NETLOAD`
  del arnes, la secuencia de FILEDIA de `run.scr` (`(command "I52AUTH15_HOSTVAL")` sobre un comando de sesion incluido), la puerta de
  FILEDIA del arnes, la evidencia progresiva y el lanzador (INVALID, codigo 2, registro escrito).
- **Paquete `9674dcc9` = SUPERADO PARA EJECUCION** una vez existe el arnes corregido. Tampoco se ejecuta `8411381f`.

### Correccion HC-5..HC-8

- **HC-5.** El perfil se lee con `CPROFILE` y se registra en la evidencia.
- **HC-6.** Auditoria de TODA lectura de variable de sistema: solo `ACADVER`, `CPROFILE` y `FILEDIA`, todas validas en AutoCAD 2025.
  Una sola funcion (`SysVar.Read`/`ReadInt`) llama a `GetSystemVariable`; nunca lanza; devuelve valor o error; jamas un valor por
  defecto. Una lectura obligatoria fallida (`ACADVER`, `CPROFILE`) deja HV-00 en UNKNOWN. `SysVarCatalog` lista los nombres auditados
  y los invalidos conocidos (`PROFILENAME`).
- **HC-7.** Antes de HV-00 y de cualquier lectura falible se persiste la identidad inmutable de la corrida (SHAs, `treesEqual`, PID e
  inicio del proceso, carpeta de ejecucion, DLL y hash del arnes, hash del DWG en blanco, bandera de no-toque del Owner declarada por el
  lanzador, registro de FILEDIA y hashes ESPERADOS del paquete). Los hechos de ensamblado CARGADO siguen siendo de HV-00 y nunca se
  fabrican antes. El lanzador ata esa evidencia temprana a la corrida (PID, inicio de proceso, hash del DWG, bandera) y recalcula PASS
  a partir de los casos.
- **HC-8.** Este registro.
- **Pruebas sin AutoCAD** (no son evidencia de host): auditoria estatica de cada literal de variable de sistema, con autoprueba
  (rechaza el codigo de RUN-1); el simulador conoce la lista exacta de nombres validos y rechaza `PROFILENAME` con `eInvalidInput`;
  reproduccion de RUN-1; PASS falsificado.
- **Fuera de alcance por decision:** `run.scr` no cambia (byte a byte el mismo que se probo en la corrida real).

## 9. RUN-2: segunda corrida gobernada de host (VALID EXECUTION / FAIL)

```text
RUN-2 = VALID EXECUTION / FAIL
Governing classification = VALID FAIL
Raw launcher label = INVALID (evidenceFinalAndCompleted=false: the run stopped on a NEW DEVIATION at HV-08)
Environment = VALID
Binding = VALID
Harness SHA = 20158fa5fb6bd2e4c4e95f63197f9c79ef3b9576
Implementation SHA = 2d10de705fffee3dc03473c2d4c76bff489fc13e
PID = 27592 ; AutoCAD R25.0.171.0.0 ; exit 0 / process gone / no timeout
FILEDIA = RESTORED 1 -> 0 -> 1 (live 1 at harness start and end) ; scratch UNCHANGED
HV-00 = PASS
HV-01 = FAIL ; HV-02 = FAIL ; HV-03 = FAIL
HV-04 = PASS
HV-05 = FAIL
HV-06 = PASS ; HV-07 = PASS
HV-08 = FAIL / NEW DEVIATION
HV-09..HV-14 = NOT RUN
AUTH15 calls = about 30 (9 creations, 21 refusals before writing)
F-1 rollback leaks = UNRESOLVED (84 leak entries)
F-2 empty effective name = CONFIRMED
```

- **Clasificacion del Arquitecto.** Los 24 controles de lanzamiento, proceso, paquete, identidad y FILEDIA se cumplieron; el unico que
  fallo (`evidenceFinalAndCompleted`) es un artefacto de clasificacion del lanzador (la corrida se detuvo a proposito). Las
  observaciones son reales. La etiqueta cruda `INVALID` se conserva como historia y NO se reescribe.
- **No se publica como evidencia PASS** y no cuenta para un PASS de host. **No se puede arrastrar** a un SHA de implementacion nuevo:
  esta correccion cambia produccion, asi que habra un SHA de implementacion nuevo y la validacion de host se repite completa.
- **F-1: fugas tras el Abort del llamador (SIN ATRIBUIR).** Todo lo creado sobrevivio al `Abort`: definiciones, anidadas, `*D`, capas
  (incluidas `Defpoints` y las de rol de Cantilever) y sobres. Causas por descartar con controles: A) escrituras que escapan de la
  transaccion del llamador (improbable: HV-01/02 confirman que la transaccion siguio viva y superior tras la llamada, y el codigo delegado
  no contiene Commit/StartTransaction); B) `Transaction.Abort` no deshace lo asumido en una base de datos lateral; C) el ayudante `End()`
  del arnes trago una excepcion de `Abort`/`Dispose`; D) efectos nativos fuera del deshacer (`Defpoints`, `*D`), que NO explican HV-02
  (sin cotas). AUTH-15 no queda ni culpado ni exonerado. Regla de fugas intacta: cualquier fuga tras el abort es FAIL.
- **F-2: nombre efectivo vacio (CONFIRMADO).** `HeaderRun` con `"<>"` devolvio `Success` con `BlockName = ""` y un sobre escrito sobre una
  definicion sin nombre. Causa raiz (defecto PREEXISTENTE, fuera de esta unidad): `BlockNaming.SanitizeBlockName` promete «empty ->
  Cabecera», pero devuelve `""` cuando la entrada no es vacia y se compone solo de caracteres invalidos; `LateralHeaderDrawer.UniqueBlockName`
  se lo pasa a AutoCAD, que acepta una definicion sin nombre. Lo consumen tambien `RackBlockRenamer`, `RackCloner` y `RackViewBaseName`,
  y una prueba de caracterizacion de I-57 fija su frontera «legacy» (con tres entradas, ninguna compuesta solo de caracteres invalidos). Corregir el ayudante compartido es una decision APARTE del
  Coordinador; **no** se hace aqui.
- **Fuera de alcance por decision:** HANDOFF sin tocar; la unidad no modifica `BlockNaming`, `SanitizeBlockName`, `UniqueBlockName`, los
  creadores de familia ni la Foundation.

## 10. Correccion post RUN-2: postcondicion de AUTH-15

- **Cambio de produccion (solo AUTH-15).** Tras la creacion de la familia y ANTES del tratamiento de bloques faltantes y de cualquier
  escritura del sobre, `RackDefinitionCreator` exige que el nombre EFECTIVO de la definicion (el que devolvio la familia: `created.BlockName`
  en HeaderRun, `out blockName` en Cantilever) no sea nulo, vacio ni espacios. Si lo es: `WriteFailed` con diagnostico. Es POST-ESCRITURA: lo
  escrito queda en la transaccion del llamador para su rollback (Option B sin cambios, sin limpieza interna).
- **Lo que NO hace.** No prevalida el nombre efectivo antes de escribir (eso duplicaria la sanitizacion de la familia, I-09), no deriva ni
  transforma nombres, no usa `BlockNaming`, no anade valores al enum de fallos.
- **Contrato.** Un exito devuelve SIEMPRE un `BlockName` real y no vacio. `InvalidBlockName` sigue cubriendo el nombre nulo, vacio o solo
  espacios TAL COMO SE RECIBE; un nombre no vacio que la familia reduce a vacio es `WriteFailed` post-escritura.
- **Guardas de fuente** (`RackDefinitionCreatorGuardTests`): ambas rutas validan el nombre efectivo, antes de `Envelope(...)` y (HeaderRun)
  antes de `HasMissingBlocks`; AUTH-15 sigue sin referenciar `BlockNaming`, `Sanitize`, `UniqueBlockName`, `Replace`, `Trim` ni el literal
  `"Cabecera"`. Cuatro mutaciones (quitar la validacion, ponerla despues del sobre, derivar el nombre con `Trim`, ponerla despues del manejo
  de faltantes) hacen fallar las guardas.

## 11. Correccion del arnes post RUN-2 (HC-9..HC-16)

Sobre la implementacion `a80a3801`; `src/` y `tests/` byte-identicos. Detalle operativo en `eng/research/I52Auth15Host/README.md`.

- **Transacciones nunca silenciosas.** `End`/`EndDisposeOnly`/`EndAfterCommit` registran `abort`/`dispose` (intentado, exito, excepcion),
  `IsDisposed`, transacciones activas antes/despues e identidad de la transaccion superior; un Abort o Dispose fallido hace FAIL el caso.
- **Autoridad DOCUMENTO.** Los casos sensibles a rollback (HV-01, 02, 03, 05, 08, 10, 12, 13, 14) corren sobre una base de datos de
  DOCUMENTO bajo `LockDocument` (segundo documento desde una copia en blanco, cerrado con descarte) y esa corrida es la autoritativa
  (`DOCUMENT-AUTHORITY`). La variante en base lateral se conserva solo como `SIDE-DB-CHARACTERIZATION`, sin efecto en el veredicto.
  La autoridad de documento no esta probada en host.
- **Controles de rollback** RB-01, RB-01V, RB-01D, RB-02a/b/c, RB-03, RB-05 tras HV-00 y antes de HV-01: resultado crudo, sin
  reinterpretar, con fugas enumeradas aparte. El arbol de decision de lectura vive en el README; nada se codifica en produccion.
- **Clasificacion (`stopKind`).** `none`/`deviation`/`hv00`/`filedia`/`exception`. Un FAIL con `completed=false` es ejecucion VALIDA solo si
  el entorno, paquete, proceso y evidencia son fiables, corrio al menos un caso mas alla de HV-00, el veredicto es FAIL con `stopKind=deviation`
  y esta respaldado por un caso FAIL o una desviacion. INVALID se conserva para entorno/vinculo/paquete/proceso/timeout/FILEDIA/evidencia
  malformada/parada en HV-00. Codigos de salida 0/2/3 sin cambio.
- **HV-08.** Una DESVIACION NUEVA se registra, el caso queda FAIL y la corrida CONTINUA con HV-09..HV-14. Solo se detiene por HV-00, FILEDIA,
  una base de trabajo no restaurada o una excepcion del ejecutor; un conteo de documentos distinto o un documento que no cierra se registra como
  problema (impide PASS) sin detener los casos independientes.
- **RUN-3** = UNA campana completa y limpia: HV-00, controles, HV-01..HV-14. Sin reejecuciones parciales.
- **Verificacion sin AutoCAD** (NO-EVIDENCIA-DE-HOST): rig offline con el interprete mini-LISP y las clases reales `Evidence`/`SysVarCatalog`,
  86/86; mutaciones del lanzador (check borrado, forma de corrida siempre verdadera, FAIL sin respaldo) detectadas.

## 12. Correccion del arnes tras la revision exacta (MAJOR-1 + EndCore)

Revision exacta del Arquitecto sobre `a80a3801` + `0a5fe046`: implementacion APROBADA, arnes con cambios requeridos. Implementacion sin cambios
(`a80a3801`; `src/` y `tests/` byte-identicos). El paquete `0a5fe046` queda SUPERADO PARA EJECUCION.

- **Los controles gobiernan el veredicto.** Conjunto exacto de 14 registros (id + `dbKind`); PASS exige todos presentes una vez y PASS, sin fugas de
  control y `documentAuthority.available = true`; cualquier control FAIL => FAIL; faltante, duplicado, `dbKind` incorrecto, inesperado o UNKNOWN => no PASS.
  Un registro SIDE-DB nunca sustituye a uno DOCUMENT-AUTHORITY. Los casos HV sensibles a rollback (01, 02, 03, 05, 08, 10, 12, 13, 14) deben ser la
  corrida DOCUMENT-AUTHORITY.
- **El lanzador lo recalcula** (`evidenceControlSet`, `evidenceRollbackCasesDocument`: 29 comprobaciones fijadas) y un FAIL puede estar respaldado por un control FAIL.
- **EndCore.** Transaccion ya desechada antes del Abort esperado => FAIL (no es rollback limpio). Deltas de transacciones activas relativos a la linea base
  del propio fin (-1, tope coherente, sin exigir cero absoluto; metrica ilegible => FAIL cerrado), solo en casos/controles sensibles a rollback.
- **stopKind.** El FAIL con `completed=false` (`stopKind=deviation`) sigue soportado por el lanzador y queda cubierto offline (T44); no se fuerza una parada
  artificial en AutoCAD.
- **Verificacion sin AutoCAD** (NO-EVIDENCIA-DE-HOST): rig offline con mutaciones (control borrado/duplicado/reetiquetado/FAIL/UNKNOWN/fuga, autoridad
  de documento no disponible, HV-10 reetiquetado SIDE-DB, comprobacion del lanzador eliminada, `Verdict()` ignorando controles) rechazadas.

## 13. RUN-3: tercera corrida gobernada de host (ejecucion VALIDA, resultado crudo FAIL)

Paquete `D:\I52-AUTH15-HV\fcca6e6c` (arnes `fcca6e6c`, implementacion `a80a3801`, `src/` y `tests/` byte-identicos). Una sola corrida completa y limpia
(HV-00, los 14 controles, HV-01..HV-14), autorizada por el Coordinador y con el compromiso de no-tocar del Owner. Evidencia cruda sin modificar en
[`docs/automation/evidence/I-52-AUTH15-run3/`](../evidence/I-52-AUTH15-run3/README.md) (hashes en su README).

```text
RUN-3 = VALID EXECUTION / raw launcher result FAIL (exit 3)
LAUNCH_VALID = true; 29/29 comprobaciones del lanzador = true; AutoCAD exit 0; sin timeout; proceso terminado
FILEDIA 1 -> 0 -> 1; vivo al inicio 1, vivo al final 1
Problems = 0; Deviations = 0; stopKind = none; completed = true
HV-00..HV-14 = 15/15 PASS; los nueve casos sensibles a rollback (01,02,03,05,08,10,12,13,14) corrieron como DOCUMENT-AUTHORITY
HV leaks = 0
Controles DOCUMENT-AUTHORITY (RB-01V, RB-01D, RB-02a, RB-02b, RB-02c, RB-03, RB-05) = 7/7 PASS, 0 fugas
Controles SIDE-DB (RB-01, RB-01V, RB-02a, RB-02b, RB-02c, RB-03, RB-05) = 7/7 FAIL, 42 fugas (3+3+4+9+16+2+5)
Caracterizaciones SIDE-DB-CHARACTERIZATION de los casos = 8/9 FAIL (HV-13 limpia), 128 fugas
Abort/Dispose = intentados y con exito en los 43 fines de transaccion de casos y caracterizaciones (y en los 14 de controles), 0 excepciones
Transacciones activas 1 -> 0, transaccion superior tras el fin = null; comprobaciones estrictas = PASS
HV-08 "<>" = WriteFailed, BlockName null, rollback del llamador limpio (SNAP identico) en DOCUMENT-AUTHORITY
```

- **Lo que muestra el instrumento.** Cada control reporta diferencias DENTRO de la transaccion (2 a 16) y las mismas diferencias siguen alli tras el
  Abort en la base lateral, y ninguna en el documento: el SNAP detecta fugas y el documento las revierte.
- **Los 17 DWG de `doc-cases\`** son byte-identicos a la plantilla en blanco: ningun documento de caso se guardo (`CloseAndDiscard`).
- **F-1 (atribucion del rollback)** queda resuelta por comparacion: RB-01 (sin codigo de producto) fuga en la base lateral y RB-01D (mismo cuerpo, documento bajo
  `LockDocument`) esta limpio; todos los controles de creadores de familia, escritor de sobre y cota nativa (`*D`, `Defpoints`) estan limpios en documento. Las fugas de
  RUN-2 fueron en bases laterales. La hipotesis de que Abort/Dispose fallidos las causaban no se sostiene (0 excepciones, transacciones cerradas). **No es AUTH-15.**
  El mecanismo por el que la `Database` lateral de este arnes (`new Database(true, true)` con `WorkingDatabase` cambiada) no revierte NO se identifico y no se afirma como
  comportamiento general de AutoCAD.
- **F-2** (nombre efectivo vacio) queda CERRADA para esta unidad: HV-08 pasa en host y el bloque de nombre vacio solo persiste en la caracterizacion lateral
  (`added BT: = 81`). El defecto raiz compartido de `BlockNaming` sigue fuera de la unidad.
- **Cobertura en host** de HV-09..HV-14: PASS los seis; no falta cobertura.

## 14. Decision del Owner: admision en host de AUTH-15 (PASS derivado bajo DOCUMENT-AUTHORITY)

Regla del Arquitecto sobre RUN-3 y ratificacion del Owner:

```text
RAW_RUN3_RESULT = FAIL (se conserva como evidencia historica; no se reescribe ni se oculta)
El FAIL crudo lo causan exclusivamente los controles SIDE-DB (caracterizacion). Rollback en SIDE-DB = no autoritativo para la admision de AUTH-15.
CANONICAL_AUTH15_HOST_RESULT = PASS UNDER DOCUMENT-AUTHORITY, derived from RUN-3 by ruling
  (no "RUN-3 = PASS": el resultado se DERIVA por decision, no lo emite el lanzador)
FAILURE_ATOMICITY_OPTION_B = AFFIRMED
CALLER_ROLLBACK_GUARANTEE = AFFIRMED solo para: base de datos de documento + LockDocument sostenido por el llamador +
  TransactionManager.StartTransaction() + Abort del llamador o Dispose sin Commit
SIDE-DB = solo caracterizacion. No se debe confiar en Abort para limpiar una base lateral: se descarta la base lateral.
OpenCloseTransaction = no cubierta / no soportada
RUN-4 = NO REQUERIDA
Cambio de produccion tras RUN-3 = ninguno (a80a3801 se mantiene). Cambio de contrato = solo documental (ver el contrato).
```

- **Base de la derivacion.** HV-00..HV-14 15/15 PASS con los nueve casos gobernantes como DOCUMENT-AUTHORITY; los siete controles de documento PASS; fugas de documento = 0;
  Problems = 0; Deviations = 0; enlace y entorno validos; FILEDIA restaurado.
- **Cambio posterior a la vista de los resultados.** Los criterios de PASS previos a la corrida pedian los 14 controles PASS; esta admision los reinterpreta despues de ver
  el resultado. Es una decision de politica del Owner, tomada sobre una rama del arbol de decision fijada ANTES de la corrida (RB-01 con fuga y RB-01D limpio => la base
  lateral es el problema) y con la regla previa de que los resultados SIDE-DB son solo caracterizacion para los casos HV. Se registra como tal.
- **Alcance de la garantia.** Verificada con una muestra por caso, en un segundo documento en blanco abierto y activo. No cubre contenido previo del dibujo, efectos de la pila de
  UNDO ni concurrencia; el SNAP no cubre propiedades de entidades ni cargas de Xrecord. AUTH-15 no rechaza bases laterales (no se anade guarda): el contrato lo documenta.
- **Identidad de la evidencia de host.** La validacion se hizo sobre el binario del paquete (implementacion `a80a3801` + arnes `fcca6e6c`), no sobre el SHA del Candidato: el arnes
  se retira y `src/` y `tests/` del Candidato deben ser byte-identicos a los validados (los arboles `224ca6a3...` / `53fb9b98...`). Igualdad de arbol NO es identidad de binario
  (AGENTS.md, «Reutilizacion de evidencia»); el Owner ratifico esta admision con esa salvedad, y el archivo de evidencia de la unidad la repite.

## 15. Estado

```text
IMPLEMENTATION = a80a3801 APPROVED (no new implementation SHA); no production change after RUN-3
AUTH15_DEV_01 = ACCEPTED / CONTRACT NORMALIZATION
RUN-1 = INVALID / HARNESS DEFECT (0 AUTH-15 calls)
RUN-2 = VALID EXECUTION / FAIL (F-1 unresolved, F-2 confirmed); cannot be carried forward
HOST_VALIDATION = RUN-3 raw FAIL preserved; CANONICAL_AUTH15_HOST_RESULT = PASS UNDER DOCUMENT-AUTHORITY (derived by ruling, Owner-ratified)
HARNESS = fcca6e6c APPROVED (delta re-review); temporary, removed from the Candidate
NEXT_GATE = CANDIDATE CI + READY-06 CONFORMANCE + INTEGRATION (no merge yet)
```
