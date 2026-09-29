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

## 9. Estado

```text
IMPLEMENTATION = CORRECTED (C-1..C-5); Architect delta re-review = APPROVED
AUTH15_DEV_01 = ACCEPTED / CONTRACT NORMALIZATION
RUN-1 = INVALID / HARNESS DEFECT (0 AUTH-15 calls); package 9674dcc9 SUPERSEDED FOR EXECUTION
HARNESS = CORRECTED (HC-5..HC-8); pending Architect delta re-review
HOST_VALIDATION = NOT RUN (no valid run yet; AutoCAD not started in this gate)
NEXT_GATE = ARCHITECT DELTA RE-REVIEW OF CORRECTED HARNESS
```
