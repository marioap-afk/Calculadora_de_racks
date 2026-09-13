# I-53 — Proposal V1 (G2): ID6 REUSE + ID7 BATCH DISTRIBUTION de la configuracion de cabecera

> **Esto es G2 en curso, no G2 cerrado.** Esta Proposal fija el contrato que se propone como vinculante y
> prepara la revision de Arquitecto (Anexo A). **Solo sera vinculante** cuando el Arquitecto la acuerde
> (AM-1..AM-4) y el Owner cierre OD-2..OD-7. **No implementa nada, no abre G3, no toca UI, pruebas ni
> produccion, no toca Push Back y no crea ADR.**
>
> Entradas: [Discovery G1](I-53-discovery.md) y [contrato](I-53-cabeceras-configurables-multidestino.md).

```text
Initiative     = I-53
Owner IDs      = ID6 (REUSE) + ID7 (BATCH DISTRIBUTION)          OD-1 = RESUELTA (autorizacion original)
Branch         = feature/cabeceras-configurables-multidestino
BASE_SHA       = 46fcac2b071929d2bd5b07aa28373941417f74a8
CLAIM_SHA      = e1d5996e70cb75589beb96a256714982fe693be3   (Claim-Id d7144fe8-6921-4a46-9d66-2d723620bea4)
G1_SHA         = c8476cccc98809fdb7fd0aeccef288521126f442   (HEAD al abrir G2)
Proposal       = V1
Architect      = PENDING REVIEW (AM-1..AM-4)
Owner          = PENDING (OD-2..OD-7)
G2             = OPEN
G3             = NOT OPENED; implementacion BLOQUEADA
ADR_REQUIRED   = PENDING_ARCHITECT
```

## 0. Preflight de G2 y alcance de este documento

Preflight del 2026-09-12 a las 19:06 -06:00, tras `git fetch --all --prune`:

| Punto | Observado |
|---|---|
| Rama I-53 | `feature/cabeceras-configurables-multidestino`, HEAD = upstream = `c8476cc`, `0/0`, arbol limpio |
| `main` | `main` = `origin/main` = `46fcac2` (no avanzo desde la base: **sin rebase**); I-53 va 3 commits delante, 0 detras |
| Stash / operaciones | ninguno; sin `MERGE_HEAD`, `CHERRY_PICK_HEAD`, `REVERT_HEAD`, `BISECT_LOG`, `rebase-*` ni `sequencer` en ninguno de los seis worktrees |
| I-49 | `architecture/motor-expresiones-parametricas` @ `4df9480` (solo docs) |
| I-50 | `feature/cotas-independientes-por-vista` @ `8ceb3a7` (G6 cerrado a las 18:37; su siguiente gate es **G3**) |
| I-52 | `feature/rackmirror-espejo-semantico` @ `339b3ab` (G1 Discovery publicado; solo docs) |
| I-54 | `architecture/propiedades-personalizadas` @ `7c197af` (G1 + Proposal V1 + paquete de Arquitecto; solo docs) |

**Re-verificacion antes de publicar (19:26 -06:00, `git fetch --all --prune`):** `main` sigue en `46fcac2`;
**I-49 → `ccf21c6`** (G2D, Proposal V4: solo docs) e **I-50 → `ed50cbd`**, que a las 19:13 abrio su **G3 (A)**:
`RackSelectiveWindow.xaml` (+12), `RackSelectiveWindow.xaml.cs` (+64, C-01), `SelectiveShellMigrationTests.cs`
(censo T-20), `SelectiveEditorWindowTests.cs` (firmas T-21) y una prueba de UI nueva. I-52 e I-54 sin cambios.

**Conflicto material nuevo respecto del Discovery: NINGUNO.** El G3 de I-50 sobre las ventanas es el cruce que
G1 §16.2 ya clasifico como material en implementacion, y la orden de G2 lo anticipa: ahora esta **activo** para
el Selectivo y **pendiente** para el Dinamico (C-07) y Push Back (C-12). I-52 e I-54 solo publicaron
documentacion y no preven archivos productivos de I-53 (§10).

Este documento **no** es un Discovery nuevo: parte de G1 y solo re-lee codigo cuando una decision lo exige.
Clases de evidencia como en G1: **HECHO** (codigo leido y linea verificada en esta sesion), **TRAZA**, **DOC**,
**INFERENCIA**, **HIPOTESIS**. Rutas abreviadas: `A/` = `src/RackCad.Application/`, `D/` = `src/RackCad.Domain/`,
`U/` = `src/RackCad.UI/`, `TC/` = `tests/RackCad.Tests/`, `TU/` = `tests/RackCad.UI.Tests/`.

## 1. Entradas vinculantes (no se reabren)

### 1.1 Clasificacion de G1 (congelada)

| Sistema | Estado para I-53 |
|---|---|
| Selectivo | **EN ALCANCE** — aplicable y pendiente |
| Dinamico | **EN ALCANCE** — aplicable y pendiente |
| Push Back | **ALREADY DONE** por I-40: solo precedente y regresion |
| Cabecera independiente | N/A (no hay multidestino interno) |
| Cantilever | N/A |
| Cama | N/A |
| Larguero | N/A |
| Drive-In | N/A (no existe como sistema funcional) |

### 1.2 OD-1 — RESUELTA por la autorizacion original

```text
ID6 — REUSE
  El usuario puede seleccionar una configuracion de cabecera EXISTENTE como SOURCE.
  Reutilizar = COPIAR sus valores authored.
  NO live link, NO instancia mutable compartida, NO referencia viva source ↔ destination.
  Despues de aplicar, source y destination son independientes.

ID7 — BATCH DISTRIBUTION
  Una source configuration se aplica a un CONJUNTO de destinos compatibles.
  Source y Destinations son conceptos SEPARADOS: nunca una lista ambigua que los mezcle.
  La taxonomia de destinos NO es universal: cada sistema traduce sus destinos al contrato comun.
```

La HIPOTESIS H-ID del Discovery (§6) queda **superada** por este texto.

### 1.3 Otras entradas del Coordinador

- **Cadena conceptual obligatoria**: `SOURCE CONFIGURATION → SNAPSHOT/COPY CONTRACT → DESTINATION SET → PREPARE →
  MUTATE → RECOMPUTE`, con las ocho garantias de §2.6.
- **Invariantes I1..I6** de copia (§3.4), con la redaccion del Coordinador.
- **I-50 sigue abierta**: ningun gate de UI de I-53 edita `RackSelectiveWindow.xaml/.cs`,
  `RackDynamicSystemWindow.xaml/.cs`, los censos de `x:Name` ni sus pruebas de UI mientras el G3 de I-50 este
  abierto o sin integrar. Esa dependencia **no** bloquea G2 ni los gates de Application.
- **L-2 = OUT OF SCOPE / FOLLOW-UP FIX** salvo decision explicita del Owner.
- **No se crea un identificador universal de destino** que invente ejes.
- **No se divide la iniciativa automaticamente** (OD-6 se evalua, §9).
- **Push Back no se toca** salvo lectura y regresion contractual.

## 2. Contrato comun

### 2.1 Vocabulario

| Termino | Definicion |
|---|---|
| **Source** | La configuracion de cabecera que se va a copiar. Tiene un **SourceKind** |
| **SourceKind** | `ConfiguratorResult` (lo que el configurador deja al cerrar) o `ExistingHeader(direccion)` (una cabecera personalizada que ya existe en el rack) |
| **Snapshot** | Copia inmutable y transitoria de la Source, capturada y validada **antes** de resolver destinos (§3) |
| **Direccion de destino** | Identidad de una cabecera en la taxonomia **propia** del sistema: Selectivo `(FondoIndex, PostIndex)`; Dinamico `ModuleId` (§6.1). **No hay tipo universal** |
| **Targets** | La **intencion** del usuario sobre los ejes del sistema (p. ej. `TargetFondos × PostTargets`). No es un conjunto de direcciones resuelto |
| **Destination set** | El resultado de resolver los Targets contra la topologia vigente: direcciones aplicables + omitidas con motivo |
| **Report** | `Applied` / `Omitted` / `Rejected`, deterministico (§4) |

### 2.2 Dos operaciones, un protocolo

| Operacion | Source | Destinos | Donde |
|---|---|---|---|
| **EDIT** | `ConfiguratorResult` | el conjunto que el editor ya editaba: Selectivo = poste visible × `TargetFondos`; Dinamico = modulo seleccionado | «Personalizar» / «Editar cabecera» (existe hoy) |
| **DISTRIBUTE** (ID6 + ID7) | `ExistingHeader` o `ConfiguratorResult` tomada como origen | los Targets elegidos por el usuario | «Tomar como origen» + «Aplicar configuracion a la seleccion» (nuevo) |

EDIT es el caso degenerado de DISTRIBUTE: **el mismo protocolo, la misma copia y el mismo informe**. Eso impide
que la edicion puntual y la distribucion diverjan en semantica de copia o de atomicidad.

### 2.3 Protocolo exacto

```text
PREPARE  (puro; puede fallar; NINGUNA mutacion observable)
  1. capturar la Source → Snapshot            (validacion de uso AL CAPTURAR; §3.2)
  2. validar la Source                          (elegibilidad por SourceKind; §5.2, §6.2)
  3. resolver TODOS los destinos                 (Targets × topologia vigente → aplicables + omitidos)
  4. validar TODOS los aplicables                (reglas de Rejected; §4.2)
  5. materializar TODAS las copias               (una por destino aplicable; §3.3)
  6. normalizar cada copia segun su destino      (profundidad, peralte; §5.6, §6.5)
  7. validar cada copia normalizada              (cabecera usable tras normalizar)
  8. revisiones propias del sistema              (Selectivo: altura por (fondo, poste); §5.5)
  9. construir el Report                         (Rejected, o Ready con Applied + Omitted)

CONFIRM  (opcional segun editor)
  - un solo aviso consolidado (p. ej. altura severa en varios destinos)
  - Cancel = mutacion CERO: el plan preparado se descarta; nada se asigna

MUTATE   (no puede fallar razonablemente)
  - solo asignaciones de lo ya preparado: copia[i] → direccion[i]
  - escalares asociados SOLO para destinos Applied
  - ninguna copia, resolucion, validacion ni lectura de catalogo aqui

RECOMPUTE
  - uno por operacion, dentro del mismo alcance diferido que MUTATE
```

### 2.4 Resultado

```text
Rejected(motivo, omitidos-informativos)  → mutacion cero
Ready(Applied[], Omitted[])              → CONFIRM → Committed | Cancelled (mutacion cero)
```

`Cancelled` **no** es `Rejected`: `Rejected` significa que la peticion es invalida o insegura; `Cancelled`, que el
usuario decidio no aplicar una peticion valida. Las reglas completas estan en §4.

### 2.5 Politica de cobertura

| Politica | Significado | Quien |
|---|---|---|
| `OmitAndReport` | un destino direccionable sin instancia fisica donde la operacion aterriza se **omite** y se informa | Selectivo (hoy), **Dinamico (propuesto, OD-4)** |
| `StageInert` | el par se escribe aunque no tenga cobertura y queda inerte | **Push Back historico** (I-40): documentado, **no migrado** y **no** implementado por el nucleo comun |

El contrato comun debe poder **expresar** ambas para describir Push Back sin cambiarlo; el nucleo nuevo solo
implementa `OmitAndReport`.

### 2.6 Las ocho garantias y su mecanismo

| # | Garantia del Coordinador | Mecanismo |
|---|---|---|
| 1 | source y destinations separados | la Source es un **Snapshot**; los destinos son **direcciones**. Ningun parametro mezcla ambos. Con `ExistingHeader`, su direccion se **omite** si aparece en el destination set (`IsSource`) |
| 2 | copia independiente por destino | `Materialize()` por destino, nunca una instancia compartida (§3.3) |
| 3 | zero partial mutation | todo lo que puede fallar ocurre en PREPARE; MUTATE solo asigna |
| 4 | destinos resueltos antes de escribir | paso 3 de PREPARE |
| 5 | validacion completa antes de escribir | pasos 1, 2, 4, 7 y 8 de PREPARE |
| 6 | reporte deterministico | orden fijado por el resolver de cada sistema; motivos enumerados (§4) |
| 7 | un recompute coherente | alcance diferido unico por operacion (§5.7, §6.7) |
| 8 | ningun dato persistido nuevo para el batch | Snapshot, Targets, destination set y Report son transitorios; las copias caen en los campos que el diseno **ya** persiste (§5.9, §6.10) |

### 2.7 Que es comun y que queda especifico

| Pieza | Comun (Application/Systems/Shared) | Especifico por sistema |
|---|---|---|
| Snapshot | **si** (§3) | — |
| Protocolo PREPARE/CONFIRM/MUTATE/RECOMPUTE | **si**, como contrato y como forma del Report | la implementacion de cada paso sobre su estado |
| Report y motivos | **si** (generico sobre la direccion) | la direccion y el orden |
| Politica de cobertura | **si** (enumeracion) | cual usa cada sistema |
| Targets y resolver | **no** | Selectivo: `TargetFondos × PostTargets`; Dinamico: `ModuleTargets` |
| Normalizacion | **no** | Selectivo: profundidad del fondo, peralte del poste; Dinamico: `Length` del modulo, peralte del rack |
| Revisiones propias | **no** | Selectivo: altura por destino (ADR-0032 D9) |
| Recompute y frontera transaccional | **no** | Selectivo: C4 + `DeferRecompute`; Dinamico: recomposicion de la ventana |

## 3. AM-1 — Snapshot: disciplina (A) frente a tipo inmutable (B)

### 3.1 Evaluacion

| Criterio | A — `RackFrameConfiguration` + `RackFrameProjectStore.DeepCopy` por disciplina | B — `HeaderConfigurationSnapshot` inmutable |
|---|---|---|
| Aliasing | lo impide solo la convencion. G1 registro cuatro alias vivos: dos verificados de nuevo en G2 —el respaldo de Push Back `HeaderConfigurationFromLastComputation` (HECHO `U/Systems/PushBack/RackPushBackSystemWindow.xaml.cs:1837`, `:2360-2363`) y `EditHeader_Click` del Dinamico (HECHO `U/Systems/Dynamic/RackDynamicSystemWindow.xaml.cs:684`)— y dos de la auditoria de G1 (G1 §13.2): el estado, diseno y sistema del Selectivo, y `PushBackMirror` | imposible por construccion: el Snapshot no expone instancia mutable |
| Copia opcional | el delegado de copia del Selectivo es **opcional** y varias pruebas de UI pasan la identidad (HECHO `A/Systems/Selective/SelectiveEditorState.cs:1214-1216`) | no hay camino sin copia: la unica salida es `Materialize()` |
| Separacion Source/Destination | implicita (dos `RackFrameConfiguration`) | explicita en la firma: Snapshot frente a direcciones (garantia 1) |
| Validacion | ocurre dentro de `DeepCopy` y **lanza** (HECHO `A/Persistence/RackFrameProjectStore.cs:62-65`) | se hace en `Capture` y devuelve un fallo tipado, antes de tocar destinos |
| Serializacion nueva | ninguna | **ninguna**: reutiliza `DeepCopy` |
| Coste | cero tipos | un tipo pequeno en Application |
| Persistencia | ninguna | ninguna (transitorio) |

**Recomendacion del ejecutor: B.** La disciplina ya fallo cuatro veces; el tipo convierte la garantia 1 y las
invariantes I1..I6 en propiedades de la API y cuesta un solo tipo sin formato nuevo.

### 3.2 Definicion de B (conceptual; los nombres los fija G3)

```text
HeaderConfigurationSnapshot                         Application/Systems/Shared, transitorio, inmutable, NO persistido
  TryCapture(RackFrameConfiguration source) → Snapshot | CaptureFailure(motivo)
      = una RackFrameProjectStore.DeepCopy(source) PRIVADA; el fallo de uso se convierte en CaptureFailure
  Materialize() → RackFrameConfiguration NUEVA en cada llamada
      = RackFrameProjectStore.DeepCopy(copia privada)
  (sin mas miembros publicos)
```

- **Por que envolver `DeepCopy`** y no guardar el JSON: `DeepCopy` ya es exactamente «capturar + materializar»
  (HECHO `RackFrameProjectStore.cs:87-104`: `Deserialize(Serialize(c))` + re-anexar `Exceptions`). Guardar una
  copia canonica privada y volver a copiarla en cada `Materialize` reproduce su politica **al pie de la letra**,
  incluidas las `Exceptions`, sin segunda implementacion.
- **Neutral**: depende solo de `D/RackFrames` y `A/Persistence/RackFrameProjectStore`. **No puede contener**
  `Window`, `Control`, `ObjectId`, `Database`, `Transaction` ni `BlockReference`, y una guarda de G3 lo comprueba.
- **Procedencia**: **no** vive en el Snapshot. El tipo de origen y su direccion viajan en la **peticion** de cada
  sistema, que es quien la necesita (exclusion `IsSource` y texto del informe). Asi el Snapshot no aprende
  direcciones de ningun sistema.
- **Momento de captura** de `ExistingHeader`: **al tomarla como origen** («Tomar como origen»), igual que Push
  Back (`SourceConfigurationCopy` devuelve copia; HECHO `A/Systems/Shared/RackModuleEditSession.cs:543-567`). Lo
  que se aplica es lo que el rotulo «Origen» muestra, aunque despues se edite la cabecera de origen; por eso su
  direccion se omite siempre del destination set (§4.3).

### 3.3 Copia profunda en el modelo real

HECHO `D/RackFrames/RackFrameConfiguration.cs:5-65`. Grafo de una cabecera y como lo reproduce `DeepCopy`:

| Parte | Contenido | Como nace en la copia |
|---|---|---|
| Escalares persistidos | `Name`, `Units`, `Height`, `Depth`, `PostPeralte`, cinco enteros de troquel, `PasoTroquel`, `PanelClear`, `StandardBaseline*` | JSON del `RackFrameProjectDocument` |
| Objetos persistidos | `LeftPost`/`RightPost` (`PostAssembly`), `LeftBasePlate`/`RightBasePlate` (`BasePlatePlacement`) | instancias nuevas desde JSON |
| Listas persistidas | `Horizontals` (`FrameHorizontal`), `BracingPanels` (`BracingPanel`) | listas **nuevas** con elementos nuevos |
| Derivado | `Members` (`FrameMember` + `FrameMemberEnd`), elevaciones y miembros por panel | **reconstruido** por `RefreshPhysicalModel` en `Deserialize` (HECHO `:67`) |
| Runtime | `Exceptions` (`FrameExceptionOverride`) | lista nueva con cada elemento clonado campo a campo (HECHO `:98-116`) |

**Copia profunda**, en este modelo, significa: **ningun objeto ni lista alcanzable desde la copia es alcanzable
desde el origen ni desde otra copia**, el derivado se reconstruye y no se copia, y las `Exceptions` siguen la
politica de `DeepCopy`. Al guardar y reabrir, las `Exceptions` se pierden exactamente como hoy (no se
persisten): I-53 **no** cambia eso.

### 3.4 Invariantes (congeladas)

| # | Invariante | Como la garantiza B |
|---|---|---|
| I1 | cada destino obtiene una instancia semanticamente independiente | un `Materialize()` por destino |
| I2 | ningun destino queda aliado al source | el Snapshot guarda su propia copia; el source nunca se asigna |
| I3 | ningun destino queda aliado a otro destino | cada `Materialize()` devuelve un grafo nuevo |
| I4 | aplicar source → destino y editar el source no cambia el destino | I2 |
| I5 | aplicar a A y B y editar A no cambia source ni B | I1 + I3 |
| I6 | listas, arrays y todo estado mutable authored tambien son independientes | §3.3: listas y objetos nuevos; derivado reconstruido |

## 4. AM-4 — Report comun y politicas de cobertura

### 4.1 Forma

```text
HeaderApplyReport<TDireccion>                      (nombre tentativo)
  Status          = Rejected | Ready | Committed | Cancelled
  Applied         = IReadOnlyList<TDireccion>                  orden deterministico del sistema
  Omitted         = IReadOnlyList<(TDireccion, HeaderOmissionReason)>
  RejectionReason = texto + codigo                             solo si Rejected
HeaderOmissionReason = AbsentInScope | NotPhysicallyPresent | IsSource
HeaderCoveragePolicy = OmitAndReport | StageInert(solo descriptivo; Push Back historico)
```

Es generico sobre la direccion propia de cada sistema: **no inventa ejes**.

### 4.2 Rejected frente a Omitted

**Omitted** — el destino es una direccion **valida** en la taxonomia del sistema, pero no hay donde aterrizar, o
es el propio origen. No invalida a los demas:

| Motivo | Cuando |
|---|---|
| `AbsentInScope` | Selectivo: ese poste no existe en ese fondo (`PostExistsIn` falso) |
| `NotPhysicallyPresent` | Dinamico: el modulo no se dibuja en ninguna frontera existente (I-33) |
| `IsSource` | con `ExistingHeader`, la direccion del origen aparece entre los destinos |

**Rejected** — la operacion entera es invalida o insegura; **mutacion cero**, incluidos los escalares:

| Motivo | Ejemplo |
|---|---|
| Source no capturable | cabecera no usable; `ExistingHeader` que no es personalizada o ya no existe al tomarla |
| Peticion mal formada | direccion fuera del universo del sistema (id desconocido, un separador en el Dinamico, fondo o poste fuera de rango) |
| Targets vacios | ningun poste o ningun fondo o ningun modulo elegido |
| **Applied vacio tras resolver** | todos los destinos omitidos |
| Frontera previa invalida | Selectivo: `CommitPendingEditors` falla (ADR-0032 D6) |
| Normalizacion imposible | Selectivo: el fondo destino no resuelve profundidad > 0 (hoy `ImposeFondoDepth` lo ignoraria en silencio; HECHO `A/Systems/Selective/SelectiveCabeceraAuthority.cs:103`) |
| Copia normalizada no usable | la validacion del paso 7 falla para cualquier destino |

- **Nunca se clampea** un destino inexistente a un vecino y **nunca se crea** un destino (fila, poste o modulo).
- «Applied vacio ⇒ Rejected» **corrige** el caso actual del Selectivo, que informa «no se cambio nada» pero ya
  escribio el peralte (L-7; HECHO `U/Systems/Selective/RackSelectiveWindow.xaml.cs:1252-1266` y
  `A/Systems/Selective/SelectiveCabeceraApplyResult.cs:41-45`).
- Una **advertencia** (altura severa del Selectivo) no es ni Omitted ni Rejected: pide CONFIRM; si el usuario
  cancela, el resultado es `Cancelled`.

### 4.3 Orden deterministico

- Selectivo: fondo ascendente, luego poste ascendente.
- Dinamico: orden longitudinal de los modulos (`Index`).
- Omitted en el mismo orden, con su motivo.

## 5. Selectivo

### 5.1 Contratos de I-43 que se reutilizan tal cual

DOC ADR-0032 (aceptado, inmutable): D1 (seleccion 2D proyectada), D4 (`TargetFondos` independiente de `Scope`),
D5/D6 (pendiente frente a comprometido; commit atomico en dos fases; un recompute), **D9** (cabecera por
`(fondo, poste)`: **`Height` es de la receta**, **`Depth` del fondo**, copia profunda por destino, omitir e
informar), D10 (tabla de autoridades) y D12 (la preferencia de destinos es del editor, no del documento).

I-53 anade **solo**: Source explicita, eje de postes, destination set, copia por Snapshot, PREPARE antes de
MUTATE y atomicidad estricta. **No** cambia `Scope` ni la edicion de celdas.

### 5.2 Seleccion de la Source

| SourceKind | Elegible | Captura |
|---|---|---|
| `ExistingHeader(fondo, poste)` | solo una cabecera **personalizada y usable** en esa direccion (`SelectiveCabeceraAuthority.UsableCustomAt`); una estandar **no** es origen (precedente I-40 opcion A) | al pulsar «Tomar como origen», desde el estado comprometido |
| `ConfiguratorResult` | lo que devuelve `window.Configuration` (HECHO `:1205`; la frontera ya es correcta en el Selectivo) | al cerrar el configurador con cambios |

### 5.3 `PostTargets`

Misma gramatica que `TargetFondos` (HECHO `A/Systems/Selective/SelectiveFondoTargets.cs:23-56`,
`SelectiveTargetMode.cs:14-28`):

| Modo | Significado |
|---|---|
| `Actual` | el poste seleccionado en el editor; sigue a la seleccion |
| `Explicito` | un conjunto elegido (casillas), distinto y ascendente; nunca vacio |
| `Todos` | todo poste de la reticula maestra: `0..MaxFrenteCount()` (HECHO `SelectiveEditorState.cs:741`) |

- **«Todos» respecto de la autoridad real**: la fila de cada fondo solo tiene los postes que ese fondo alcanza
  (`PostExistsIn`: `0..FrontCount(k)`, HECHO `SelectiveEditorState.cs:1166-1170`). «Todos» es la **intencion**
  sobre la reticula maestra; el resolver proyecta cada poste sobre cada fondo y **omite** (`AbsentInScope`) donde
  no existe.
- **Postes de medio frente**: no son direccionables (siempre estandar); quedan fuera del universo.
- **Tras un cambio estructural** se re-resuelven como `TargetFondos` (ADR-0032 D6): «Todos» se re-expande, un
  conjunto explicito se poda y «Actual» sigue.
- **No se persiste** ni se recuerda como preferencia: al abrir, vale `Actual` (OD-3).

### 5.4 Destination set

```text
para cada fondo en TargetFondos (asc)
  para cada poste en PostTargets (asc)
    si no PostExistsIn(fondo, poste)                    → Omitted(AbsentInScope)
    si SourceKind = ExistingHeader y (fondo, poste) = origen → Omitted(IsSource)
    si no                                               → aplicable
```

**Reduccion destructiva existente**: se conserva `SyncPostCabeceras` (HECHO `:739-762`: poda sin resurreccion).
El destination set se resuelve **despues** de la frontera C4 y de la sincronizacion, asi que nunca apunta a una
fila podada, y `EnsureCabeceraRow` solo rellena **dentro** de un poste que el fondo tiene.

### 5.5 Regla de HEIGHT — recomendacion explicita

| Fuente | Que dice |
|---|---|
| Precedente I-40 (PBH-01) | la altura de una cabecera personalizada es autoridad efectiva y viaja con la copia |
| **ADR-0032 D9 y D10** (aceptado) | **`Height` pertenece a la receta**, es identica en todos los destinos y **no** se ajusta por fondo; si difiere de la altura resuelta de un destino, **se avisa, no se corrige** |
| Selectivo hoy | `ApplyCabeceraToTargets` no toca `Height` (HECHO `:1193-1228`); la revision avisa por fondo (HECHO `SelectiveCabeceraHeightReview.cs:89-129`); pero «Personalizar» **siembra** el configurador con la altura resuelta incluso sobre una personalizada (HECHO `RackSelectiveWindow.xaml.cs:1191`) |
| Autoridad por `(fondo, poste)` | la receta es de la direccion; la profundidad, del fondo |

**Recomendacion: la altura authored VIAJA con el Snapshot; el destino NO impone su altura resuelta.** Es la
aplicacion literal de ADR-0032 D9, no una decision nueva. PREPARE ejecuta la revision de altura para **cada**
`(fondo, poste)` aplicable y la consolida en **un** aviso: severa → CONFIRM; solo informativa → aviso sin
confirmacion.

La **semilla** de «Personalizar» (`:1191`) es comportamiento de EDIT anterior a I-53 y **no se cambia** aqui;
queda registrada como asimetria para el Owner (OD-3.c): distribuir conserva la altura del origen, y re-editar
una personalizada arranca el configurador en la altura resuelta.

### 5.6 Normalizacion por destino

| Propiedad | Autoridad | Regla |
|---|---|---|
| `Depth` | el fondo destino (D9) | `CabeceraDepthOfFondo(fondo)` > 0 obligatorio; si no, Rejected (§4.2) |
| `Height` | la receta | se conserva |
| `PostPeralte` de la copia | el **poste** destino | se normaliza al peralte efectivo del poste destino (`PostPeraltes[i]` si > 0; si no, el global), la misma regla que la semilla (HECHO `RackSelectiveWindow.xaml.cs:1196-1197`) |
| `PostPeraltes[i]` (escalar global por poste) | el poste (I-43: global, sin eje de fondo) | **DISTRIBUTE no lo escribe** (OD-3.b). **EDIT** conserva su escritura del poste visible, pero **solo si** ese poste queda en Applied (cierra L-7) |

### 5.7 Aterrizaje en el estado y recompute

- PREPARE: una operacion pura nueva de Application sobre el estado comprometido (resolver + copias +
  normalizacion + revision + Report). **No** muta `SelectiveEditorState`.
- MUTATE: una asignacion `row[poste] = copia` por destino Applied y el escalar de EDIT, sin copias ni
  resoluciones. La operacion actual `ApplyCabeceraToTargets` pasa a ser el MUTATE de EDIT, o se reimplementa
  sobre el plan (lo decide G4 sin cambiar su resultado observable para un destino valido).
- Recompute: **uno** por operacion, en el `DeferRecompute` que ya envuelve la aplicacion (HECHO
  `:1261-1270`). La frontera C4 previa produce **su propio** recompute solo si habia campos pendientes (ADR-0032
  D6): la revision de altura necesita el sistema ya recomputado sobre la topologia comprometida.

### 5.8 Lo que no cambia

`SelectivePalletDesign` (archivo caliente) **no** se modifica; `Scope` y celdas, `TargetFondos` y su
preferencia, «Restablecer poste» (sigue siendo un poste × `TargetFondos`), la frontal, lateral y planta por fondo.

### 5.9 Persistencia, BOM, dibujo, Actualizar y RACKEDITAR

Las copias caen en `PostCabeceras` y `ExtraFondoPostCabeceras`, que ya se persisten y leen
(`SelectivePalletDesignDocument`, `SchemaVersion` 1.0). BOM, lateral, planta y frontal ya leen por
`SelectiveCabeceraAuthority` (G1 §10.2). RACKEDITAR y Actualizar no cambian. **Sin DTO nuevo ni campo nuevo.**

## 6. Dinamico

### 6.1 OD-2 — unidad de destino: `ModuleId` (A) o `(PostIndex, ModuleId)` (B)

| Criterio | A — modulo longitudinal `ModuleId` | B — instancia fisica `(PostIndex, ModuleId)` |
|---|---|---|
| Autoridad actual | paso 2 de `HeaderConfigurationAtPost`: la configuracion personalizada del modulo vale en **todas** sus lineas (HECHO `A/Systems/Dynamic/DynamicFrontGeometry.cs:296-299`) | paso 1: `LineOverride` (HECHO `:374-395`); existe, pero el Dinamico nunca lo produce |
| Dibujo | lateral, frontal por corte y planta usan la misma configuracion: **coherente** | la planta usa la del modulo (HECHO `A/Systems/Dynamic/DynamicSystemPlantaBuilder.cs:46-66`) y el lateral general no consulta overrides: **divergencia** entre vistas |
| BOM del editor | correcto | correcto (el editor copiaria con `DeepCopy`) |
| BOM de `RACKBOMTOTAL` | correcto: `BuildDynamic` refresca las cabeceras de modulo (HECHO `A/Systems/Shared/SystemRegistry.Default.cs:79-85`) y el resolver las refresca (HECHO `DynamicRackSystemResolver.cs:194`, `:201`) | **heredaria L-2**: el resolver clona overrides sin refresco (HECHO `:226-238`, `:512-516`) y la carga no los refresca → cabeceras sin celosia |
| Overrides por linea | el Dinamico sigue sin escribirlos | habria que escribirlos, transportarlos y reconciliarlos en una reconstruccion, como `PushBackEditorDesignAssembler.cs:327-365` |
| Persistencia | `Modules[].Header` + `UseCalculatedHeaderConfiguration`, ya persistidos | `HeaderLineOverrides`, existente pero sin refresco en la carga |
| Editor | el modulo ya es la unidad seleccionable del Dinamico | exige eje de lineas, cobertura y selector nuevo |
| ID7 | «un conjunto de cabeceras»: satisfecho | granularidad mayor, **sin requisito del Owner** para el Dinamico |

**Recomendacion: A.** B no esta pedida para el Dinamico, divide las vistas, y hoy **arrastraria L-2** al BOM
total del Dinamico; resolverlo exige tocar el resolver compartido con Push Back (AM-3). B queda posible en el
futuro, **despues** del follow-up de L-2 y solo con requisito del Owner.

### 6.2 Seleccion de la Source

| SourceKind | Elegible | Captura |
|---|---|---|
| `ExistingHeader(ModuleId)` | modulo de cabecera, **personalizado** (`HasCustomHeaderConfiguration`) y **fisicamente presente** (`RackModuleDescriptor.IsPhysicallyPresent`, HECHO `A/Systems/Shared/RackModuleDescriptor.cs:92`, `:101`, `:163-195`) | al «Tomar como origen» |
| `ConfiguratorResult` | lo que devuelve **`window.Configuration`**, abierto sobre una **copia** del modulo, nunca sobre la instancia viva (cierra L-1, §6.9) | al cerrar el configurador con cambios |

### 6.3 `ModuleTargets`

| Modo | Significado |
|---|---|
| `Actual` | el modulo de cabecera seleccionado |
| `Explicito` | conjunto de modulos de cabecera, en orden longitudinal; nunca vacio |
| `Todas` | todo modulo de cabecera del rack (`IsHeader`) |

Tras un recompute estructural se re-resuelven igual que en el Selectivo (los ids son posicionales y cambian en
una reconstruccion: `DynamicRackSystemBuilder.AssignIds`, G1 §11.1). No se persisten.

### 6.4 Destination set

```text
para cada modulo de cabecera del universo, en orden longitudinal, que este en ModuleTargets
  si no IsPhysicallyPresent                                → Omitted(NotPhysicallyPresent)
  si SourceKind = ExistingHeader y ModuleId = origen       → Omitted(IsSource)
  si no                                                    → aplicable
id fuera del universo (desconocido o separador)            → Rejected
```

### 6.5 Normalizacion por destino

| Propiedad | Autoridad | Regla |
|---|---|---|
| `Depth` | la `Length` del modulo destino | la impone `DynamicRackSystemBuilder.Refresh` (HECHO `:144-148`) |
| `PostPeralte` | el rack | la impone `ApplyPostPeralte` (HECHO `:179-194`) |
| `Height` | la receta | se conserva (como hoy: `UpdateHeaderHeightInPlace` no toca personalizadas; HECHO `DynamicEditorDesignAssembler.cs:120-139`) |
| `Length` e `IsManualOverride` del destino | el modulo | **DISTRIBUTE no los toca**. **EDIT** conserva su regla actual: si el fondo editado difiere de la `Length`, pasa a longitud manual (HECHO `RackDynamicSystemWindow.xaml.cs:703-710`) |
| `UseCalculatedHeaderConfiguration` | procedencia | `false` en cada destino Applied |

### 6.6 AM-2 — sesion: que se reutiliza y que no

Hechos que deciden:

1. `RackModuleEditSession.ApplyHeaderConfiguration` marca **`IsManualOverride = true`** en cada destino (HECHO
   `A/Systems/Shared/RackModuleEditSession.cs:275`), y `RackModuleReconciliation` trata eso como **longitud
   manual** que se conserva en una reconstruccion (HECHO `A/Systems/Shared/RackModuleReconciliation.cs:164`,
   `:186-191`). Adoptarla tal cual **congelaria la longitud —y por tanto el fondo— de cada destino** en el
   siguiente cambio de tarima: contradice «la profundidad es del destino».
2. Las operaciones de **nivel modulo** de la sesion (`ApplyHeaderConfiguration`, `SetHeaderConfiguration`,
   `CopyHeaderConfiguration`) **no tienen llamador de produccion**: el unico es
   `ApplyHeaderConfigurationToInstances` (HECHO `RackPushBackSystemWindow.xaml.cs:1959`); las otras solo aparecen
   en 60 llamadas de prueba.
3. Una sesion **escenificada** (Confirmar/Cancelar) crea un ambito sucio: ADR-0029 D8 obliga a declararlo y a
   que el cierre lo agregue. Hoy el Dinamico **no** declara ambito (HECHO
   `TU/RichEditorCloseContractTests.cs:76-85`).
4. La sesion no sabe cambiar el **tipo** de un modulo, cosa que el Dinamico si ofrece (G1 §11.4).
5. Lo neutral y ya compartido, sin codigo de Push Back: `RackModuleReconciliation` (declara que existe para
   sustituir el par ordinal y que no lo sustituyo porque I-35 no podia tocar el Dinamico; HECHO `:81-97`),
   `RackModuleDescriptor` y `RackFrameProjectStore.DeepCopy`.

| Opcion | Descripcion | Coste | Riesgo |
|---|---|---|---|
| **A — nucleo PREPARE/MUTATE por operacion + reconciliacion** | ID6/ID7 y EDIT se preparan con el nucleo comun y se asignan en MUTATE sobre el modelo de trabajo del editor; la reconstruccion usa `RackModuleReconciliation`; **sin** sesion escenificada | medio | cambia la reconstruccion del Dinamico (OD-2.b) |
| B — recompute dirigido por commit | un dueno de Application (baseline + sesion + commit) como `PushBackEditorState`; todos los gestos de modulo pasan por la sesion, con commit inmediato | alto: reenrutar todo gesto, incluido el cambio de tipo que la sesion no soporta | exige una operacion aditiva de sesion sin longitud manual (hecho 1) |
| C — sesion escenificada completa como Push Back | B + Confirmar/Cancelar + ambito sucio + `OnClosing` | muy alto | ADR-0029 D8, contrato de cierre, UX nueva no pedida |

**Recomendacion del ejecutor: A.** Se aparta de la preferencia de G1 por los hechos 1 a 4: ID6/ID7 necesitan
atomicidad **por operacion**, no escenificacion entre operaciones, y la sesion de nivel modulo trae un efecto
lateral que el Dinamico no puede aceptar. El riesgo que G1 senalaba —«mutar el modelo vivo»— se cierra donde
importaba: el configurador ya no muta una instancia viva, todo lo que puede fallar ocurre en PREPARE sobre
copias, y MUTATE solo asigna lo preparado.

**Reutilizacion sin «codigo de Push Back disfrazado»:**

| Pieza | Decision |
|---|---|
| `RackModuleReconciliation`, `RackModuleDescriptor`, `RackFrameProjectStore.DeepCopy`, `DynamicFrontGeometry` | **se consumen tal cual**, sin modificarlas |
| `RackModuleEditSession` | **no** se adopta en I-53 ni se modifica |
| `PushBackEditorState` (ciclo de vida de la sesion) | **no** se mueve ni se copia |
| `PushBackEditorDesignAssembler` (cola del compuesto, parametros avanzados, transporte de overrides) | **no** se duplica: el Dinamico solo necesita «reconciliar intenciones sobre el sistema reconstruido», que es una llamada al servicio compartido |

Si el Arquitecto prefiere B o C, la operacion de sesion sin longitud manual pasa a ser **obligatoria y aditiva**
(las existentes intactas), y C ademas cambia `RichEditorCloseContractTests`.

### 6.7 Reconstruccion y reconciliacion

Hoy (HECHO `RackDynamicSystemWindow.xaml.cs:441-459`; `DynamicEditorDesignAssembler.cs:61-116`): una
reconstruccion conserva **solo** las longitudes manuales de cabecera, por **orden** de cabecera, y re-estampa
todo como calculado; se pierden **sin informe** las personalizadas, las longitudes de separador, los cambios de
tipo y los overrides.

Propuesto:

```text
reconstruir  = intenciones := modulos del sistema de trabajo ANTES de BuildDefault
               sistema     := BuildDefault(...)
               informe     := RackModuleReconciliation.Reconcile(intenciones, sistema, restaurados = ∅)
«Restaurar layout» (forceRebuild) = reconstruir SIN intenciones  (reset: conserva su semantica)
sin reconstruccion = como hoy (UpdateHeaderHeightInPlace + ApplyPostPeralte)
```

- Una personalizada distribuida **sobrevive** a un cambio de tarima si su `ModuleId + Kind` sigue existiendo,
  **adaptada** en fondo y peralte, e informada; si no, **Removed** o **Incompatible**, informada.
- Es un **cambio de comportamiento del Dinamico** (hoy se pierde en silencio, y las longitudes manuales pasan de
  casarse por orden de cabecera a casarse por id y tipo): **requiere confirmacion del Owner (OD-2.b)** y
  validacion en AutoCAD.
- La recomposicion vive en la ventana: G6 aporta la operacion de Application con pruebas; G7 la cablea.
- Guardas de I-35 afectadas, en `TC/PushBackModuleEditorCharacterizationTests.cs`: `Fact5_TheDynamicPair_IsUnchanged…`
  (`:186`) y `Fact5_SnapshotHeaderFondos_CapturesOnlyManualFondos…` (`:215`). **Se reapuntan** al contrato nuevo en
  el gate que cambia el comportamiento; sus aserciones de Push Back **no** se tocan.

### 6.8 Overrides por linea

El Dinamico **sigue sin escribirlos**; el resolver y el DTO los transportan como hoy. Guardas: ninguna operacion
de I-53 escribe `HeaderLineOverrides` ni `DerivedPostLineOverrides`, y un override entrante sobrevive intacto a
EDIT y DISTRIBUTE.

### 6.9 L-1 — frontera del configurador

**Recomendacion: forma parte necesaria de I-53 Dinamico, no un `fix/` previo.**

- La captura correcta (`window.Configuration` sobre una copia) **es** el paso 1 de PREPARE para EDIT: sin ella,
  ID7 distribuiria una configuracion obsoleta.
- Un `fix/` previo tocaria el mismo metodo del mismo archivo caliente, **tambien bloqueado** por el G3 de I-50, y
  G7 lo reescribiria despues: no adelanta nada.
- La prueba de regresion se escribe y se ve **fallar** sin el arreglo, y ambos entran juntos en G7 (AGENTS,
  «Pruebas», punto 2). Un rojo publicado durante la espera por I-50 dejaria la rama en rojo semanas.
- Si el Owner quiere el arreglo en `main` antes de I-53, la alternativa es un `fix/` **despues** de integrar
  I-50, con I-53 rebasando encima (OD-5).
- La guarda `Fact7` (HECHO `TC/PushBackModuleEditorCharacterizationTests.cs:424`, `:439-440`) se reapunta en G7 si
  su literal deja de existir; `Fact1` (`:366`, `:408`: sin Confirmar en el XAML del Dinamico) y `Fact3` (`:339`,
  `:349`: `forceRebuild: true`) siguen siendo ciertas con la opcion A.

### 6.10 Persistencia, BOM, dibujo, Actualizar y RACKEDITAR

Las copias caen en `Modules[].Header` + `UseCalculatedHeaderConfiguration`, ya persistidos y refrescados en la
carga. BOM (`SystemBomBuilder`), lateral, frontal y planta ya leen la configuracion del modulo. RACKEDITAR y
Actualizar no cambian. **Sin DTO nuevo ni campo nuevo.**

## 7. AM-3 — fronteras de `Members` y `Exceptions` sin tocar Push Back

| Riesgo | Como no se hereda |
|---|---|
| L-2 (overrides por linea sin `Members` fuera del editor) | el Dinamico usa `ModuleId` (§6.1): sus datos pasan por cabeceras de **modulo**, que toda frontera refresca. Guarda verde de G6: el BOM de un rack con personalizadas distribuidas es identico por la ruta del editor y por la ruta `RackProjectStore` → registro → resolver → `SystemBomBuilder` |
| Copias sin derivado | `Materialize()` pasa por `Deserialize` (`RefreshPhysicalModel`); el nucleo **nunca** usa `CloneHeader` ni `ToConfiguration` sin refresco. Guarda de G3 |
| `Exceptions` | siguen la politica de `DeepCopy` en memoria y se pierden al persistir, como hoy. Guarda de G3 que lo fija sin cambiarlo |
| Selectivo | `SelectiveBomBuilder` ya refresca una personalizada sin `Members` (G1 §10.2). Guarda de G4: BOM del editor = BOM desde el diseno recargado |
| Push Back | **cero diff de produccion** en `A/Systems/PushBack/`, `U/Systems/PushBack/`, `RackModuleEditSession.cs` y `RackModuleReconciliation.cs`, comprobado por `git diff` en cada gate; sus pruebas de I-40 sin tocar. **L-2 no se corrige ni se fija con una prueba** en I-53: el follow-up se registra al cerrar G2 |

## 8. Push Back: precedente y regresion

- Se lee como precedente: modelo de destinos cabeceras × lineas, Source recordada, copia por par, validacion
  previa, `StageInert`.
- Regresion: las suites de I-40 (`TC/PushBackHeaderApplyTests.cs`, `TC/PushBackDerivedPostAndLineTests.cs`,
  `TC/PushBackFrontalRearCutTests.cs`, `TC/RackModuleEditSessionTests.cs`, `TU/PushBackHeaderAuthorityWindowTests.cs`,
  `TU/PushBackHeaderLifecycleTests.cs`, `TU/PushBackHeaderOwnerRoundTwoTests.cs`,
  `TU/PushBackPhysicalHeaderScopeTests.cs`, `TU/PushBackHeaderCartesianTests.cs`,
  `TU/PushBackHeaderLineWindowTests.cs`) siguen verdes y **sin cambios**.
- La unica excepcion declarada son las aserciones **del Dinamico** dentro de
  `TC/PushBackModuleEditorCharacterizationTests.cs` (§6.7, §6.9), que se reapuntan y nunca se relajan.
- No se corrigen sus discrepancias de G1 §9.10 ni L-2.

## 9. OD-6 — una iniciativa o particion

| Criterio | A — una I-53 con gates por sistema | B — nucleo congelado + I-53A Selectivo + I-53B Dinamico |
|---|---|---|
| Duracion | un candidato y una validacion; el Selectivo espera al Dinamico | el Selectivo puede integrarse antes |
| Archivos calientes | los mismos | los mismos |
| Conflicto I-50 | G5 y G7 esperan al **mismo** G3 de I-50 | igual: la particion no desbloquea antes |
| Owner Validation | una sesion AutoCAD con dos sistemas | dos sesiones |
| Integracion | un merge; un rebase tras I-50 | dos merges serializados y un rebase extra |
| Divergencia del contrato comun | minima: dos consumidores en la misma revision y en el mismo candidato | mayor: el nucleo se integraria con **un** solo consumidor probado |

**Recomendacion: A.** Las dos UI esperan al mismo gate externo, asi que partir no acelera, y el nucleo solo se
prueba de verdad con **dos** consumidores. Punto de control: **al cerrar G4 y G6** se re-evalua; si el Dinamico
supera su presupuesto o el Owner quiere adelantar el Selectivo, se parte en ese momento con el nucleo ya
probado.

## 10. Conflictos con ramas activas (auditados de nuevo en G2)

`git diff --name-only origin/main...<rama>` el 2026-09-12, con las puntas de las 19:26:

| Rama | Archivos cambiados | Fuera de `docs/` |
|---|---|---|
| I-49 @ `ccf21c6` | 8 | **0** |
| I-50 @ `ed50cbd` | 54 | 44 |
| I-52 @ `339b3ab` | 3 | **0** |
| I-54 @ `7c197af` | 5 | **0** |

Solape por archivo productivo previsto por I-53:

| Archivo previsto | Gate I-53 | I-49 | I-50 hoy | I-50 despues | I-52 | I-54 | Clase |
|---|---|---|---|---|---|---|---|
| `A/Systems/Shared/HeaderConfigurationSnapshot.cs` (nuevo) | G3 | — | — | — | — | — | **ninguno** |
| `A/Systems/Shared/HeaderApplyReport.cs` (nuevo; nombre tentativo) | G3 | — | — | — | — | — | **ninguno** |
| `A/Systems/Selective/SelectiveEditorState.cs` | G4 | — | +1 linea (`:1318`, `DimensionViews` al construir el diseno) | — | — | — | **textual** (region distinta de `:1143-1235`) |
| `A/Systems/Selective/SelectiveCabeceraHeightReview.cs` | G4 | — | — | — | — | — | **ninguno** |
| `A/Systems/Selective/SelectivePostTargets.cs` y la distribucion del Selectivo (nuevos) | G4 | — | — | — | — | — | **ninguno** |
| `U/Systems/Selective/RackSelectiveWindow.xaml/.cs` | G5 | — | **G3 (A), C-01**: XAML +12 (`:60-73`), `.cs` +64 (`:17`, `:263`, `:1704-1763`, `:2401`, `:2750`) | — | — | — | **MATERIAL ACTIVO → BLOQUEADO** |
| `TU/SelectiveShellMigrationTests.cs` (censo) | G5 | — | **G3 (A), T-20** (+9) | — | — | — | **MATERIAL ACTIVO → BLOQUEADO** |
| `A/Systems/Dynamic/DynamicEditorDesignAssembler.cs` | G6 | — | +1 linea (`:175`, `BuildDesign`) | — | — | — | **textual** (region distinta de `:59-139`) |
| `A/Systems/Dynamic/DynamicHeaderModuleTargets.cs` y la distribucion del Dinamico (nuevos) | G6 | — | — | — | — | — | **ninguno** |
| `TC/PushBackModuleEditorCharacterizationTests.cs` (solo aserciones del Dinamico) | G6/G7 | — | — | — | — | — | **ninguno** |
| `U/Systems/Dynamic/RackDynamicSystemWindow.xaml/.cs` | G7 | — | — | **G3 (C-07)** | — | — | **MATERIAL → BLOQUEADO** |
| `TU/DynamicShellMigrationTests.cs` (censo) | G7 | — | — | **G3 (T-20)** | — | — | **MATERIAL → BLOQUEADO** |
| `TU/SelectiveEditorWindowTests.cs` (firmas I-24) | G5 si cambia | — | **G3 (A), T-21** (+117) | — | — | — | **MATERIAL ACTIVO** si I-53 lo toca |
| `TU/DynamicEditorWindowTests.cs` (firmas I-24) | G7 si cambia | — | — | **G3, T-21** | — | — | **MATERIAL** si I-53 lo toca |
| Solo lectura: `RackModuleEditSession.cs`, `RackModuleReconciliation.cs`, `RackModuleDescriptor.cs`, `RackFrameProjectStore.cs`, `SelectiveCabeceraAuthority.cs`, `DynamicFrontGeometry.cs` | — | — | — | — | — | — | no se modifican |
| No se tocan: `SelectivePalletDesign.cs`, `SelectivePalletDesignDocument.cs`, `DynamicRackSystemDocument.cs`, `DynamicRackDesign.cs`, `DynamicRackSystemResolver.cs` (I-50 los toca) | — | — | si | — | — | — | fuera de I-53 |

Semantica: I-52 registra un acoplamiento con los datos de cabecera direccionados por indice (su Discovery `:779`);
I-53 **no** anade datos persistidos, asi que no le cambia nada. I-54 preve `RackEmbedDocument`,
`RackEmbedComposer`, un comando y una ventana propios: sin cruce.

**OD-7 — estado actual (19:26):** el G3 de I-50 **ya empezo**. Su bloque (A) modifico la ventana del Selectivo,
su censo y sus firmas de dibujo (`ed50cbd`); el Dinamico (C-07) y Push Back (C-12) siguen pendientes dentro del
mismo G3. Mientras I-50 no se integre, **G5 y G7 de I-53 estan bloqueados**; G3, G4 y G6 no. Tras integrarse
I-50, I-53 rebasa sobre `main` antes de G5/G7. Alternativa: excepcion explicita del Owner con condicion de
detenerse antes de editar, que hoy ya no evitaria el conflicto textual en la ventana del Selectivo.

## 11. Gates refinados (propuesta)

Orden de ejecucion recomendado: **G3 → G4 → G6 → [I-50 integrada] → G5 → G7 → G8 → G9 → G10**. Justificacion
material: G4 y G6 no chocan con I-50 y prueban el nucleo con sus dos consumidores **antes** de que una UI lo
congele; G5 y G7 esperan al mismo bloqueo externo y se hacen juntos tras un solo rebase.

| Gate | Contenido | RED / verde | Evidencia de cierre | Bloqueo |
|---|---|---|---|---|
| **G2** | Esta Proposal → revision de Arquitecto → V2 si hay cambios → decisiones del Owner → freeze: contrato reescrito vinculante, `docs/automation/decisions/I-53.md`, hallazgos laterales en `ideas-futuras.md`, ADR si AM-1/AM-4 lo exigen | — | docs; CI de `push` | — |
| **G3** | Nucleo puro: Snapshot, Report y motivos, politica de cobertura; guardas de neutralidad (sin WPF ni AutoCAD) y de frontera `Members`/`Exceptions`; comprobacion de cero diff en Push Back | andamiaje sin comportamiento + RED por asercion, luego verde | Core local; CI | — |
| **G4** | Selectivo, Application: `PostTargets`, resolver, PREPARE/MUTATE, revision de altura multi-poste, EDIT atomico (L-7) | RED de las capacidades nuevas; verdes de I-43 | Core local; CI | — |
| **G5** | Selectivo, UI: Origen, Postes destino, Aplicar, informe, aviso consolidado; censo **actualizado, no relajado** | RED de UI | Core + UI (LC-UI); CI | **G3 de I-50** |
| **G6** | Dinamico, Application: `ModuleTargets`, resolver con presencia fisica, PREPARE/MUTATE sin tocar longitud manual, reconstruccion con reconciliacion; reapuntar `Fact5` | RED de las capacidades nuevas; verdes del Dinamico actual | Core local; CI | — |
| **G7** | Dinamico, UI: L-1 (RED visto fallar + arreglo), Origen, Cabeceras destino, Aplicar, informe de reconciliacion; censo actualizado; `Fact7` si cambia | RED de UI | Core + UI; CI | **G3 de I-50** |
| **G8** | Candidato por SHA exacto | — | Core Full y UI Full locales, Debug de UI y de Plugin, CI de `push` 4/4 | — |
| **G9** | Owner Validation en AutoCAD 2025 por sistema (§12.4) | — | veredicto del Owner | G8 |
| **G10** | Documentacion, integracion `--no-ff`, CI del merge, cobertura y limpieza (WORKFLOW §4.5) | — | compuertas posteriores al merge | G9 |

## 12. Matriz de pruebas futura (no se implementa en G2)

Regla del repo: toda corrida filtrada debe demostrar que selecciono pruebas (**0 seleccionadas = FALLO**) y se
registra con su conteo.

### 12.1 Nucleo comun (G3)

| Id | Prueba | Clase |
|---|---|---|
| C-01 | `TryCapture` de una cabecera no usable devuelve fallo tipado, sin lanzar y sin mutar | RED |
| C-02 | cada `Materialize()` devuelve una instancia nueva; ningun objeto ni lista compartidos con el origen ni entre copias (recorrido del grafo) | RED |
| C-03 | la copia materializada equivale a `DeepCopy`: `Members` = `RefreshPhysicalModel`; `Exceptions` clonadas | RED |
| C-04 | mutar el origen tras capturar o mutar una copia no cambia el Snapshot | RED |
| C-05 | Report: orden deterministico; `Rejected` ⇒ `Applied` vacio; motivos enumerados; `Cancelled` ≠ `Rejected` | RED |
| C-06 | guarda de neutralidad: los tipos nuevos no referencian WPF, `Autodesk.*`, `ObjectId`, `Database`, `Transaction` ni `BlockReference` | RED → verde |
| C-07 | el nucleo no usa `CloneHeader` ni `ToConfiguration` sin refresco | guarda |

### 12.2 Selectivo (G4, G5)

| Id | Caso | Clase |
|---|---|---|
| S-01 | elegir origen: solo personalizada usable; una estandar no es elegible | RED |
| S-02 | aplicar a uno (`Actual` × fondo actual) | RED |
| S-03 | aplicar a varios (postes explicitos × fondos explicitos) | RED |
| S-04 | aplicar a todos (`Todos` × `Todos`) con fondos de distinta longitud | RED |
| S-05 | la direccion origen se omite (`IsSource`) | RED |
| S-06 | editar el origen despues no cambia los destinos (I4) | RED |
| S-07 | editar un destino no cambia origen ni otros destinos (I5, I6) | RED |
| S-08 | origen invalido → `Rejected`, mutacion cero | RED |
| S-09 | fondo destino sin profundidad resoluble → `Rejected` | RED |
| S-10 | poste inexistente en un fondo → `Omitted(AbsentInScope)`, nunca creado | RED (multi-poste); verde el caso de un poste existente (`ATargetWhereThePostDoesNotExist_IsOmittedAndReported_NeverPadded`) |
| S-11 | todos omitidos → `Rejected` y `PostPeraltes` intacto (L-7) | RED |
| S-12 | sin clamping ni resurreccion tras reducir frentes | RED (multi-poste); verdes `ShrinkingAFondo_…`, `Scenario4_…` |
| S-13 | un recompute por operacion (`RecomputeCount`) | RED (UI) |
| S-14 | altura: viaja con la receta; aviso consolidado multi-poste; severa → confirmar; cancelar = mutacion cero | RED |
| S-15 | EDIT escribe `PostPeraltes[visible]` solo si queda aplicado | RED |
| S-16 | `PostTargets` tras cambio estructural: `Todos` se re-expande, explicito se poda, `Actual` sigue | RED |
| S-17 | RACKEDITAR: reabrir con personalizadas distribuidas en varios fondos por `LoadExisting` | RED (hoy sin fijar) |
| S-18 | Actualizar redibuja con las copias | verde (ruta existente) + E2E nueva |
| S-19 | guardar/reabrir | verde `RoundTrip_PreservesDifferentCustomsInFondos0And1And3` + caso distribuido |
| S-20 | dibujo lateral, planta y frontal por fondo | verdes de consumidores + escenario distribuido |
| S-21 | BOM del editor = BOM desde diseno recargado | guarda |
| S-22 | legacy: documentos sin personalizadas y «Personalizar» de un poste dan el mismo resultado | verdes: suite de I-43 sin cambios |
| S-23 | regresion I-43: `TargetFondos × Scope` de celdas intacto | verdes |

### 12.3 Dinamico (G6, G7)

| Id | Caso | Clase |
|---|---|---|
| D-01 | elegir origen: personalizado y presente; calculado o no presente no son elegibles | RED |
| D-02 | aplicar a uno | RED |
| D-03 | aplicar a varios | RED |
| D-04 | aplicar a todas | RED |
| D-05 | el modulo origen se omite (`IsSource`) | RED |
| D-06 | independencia origen/destino (I4) | RED |
| D-07 | independencia destino/destino (I5, I6) | RED |
| D-08 | origen invalido → `Rejected`, mutacion cero | RED |
| D-09 | destino desconocido o separador → `Rejected` | RED |
| D-10 | modulo no presente por frentes en blanco → `Omitted(NotPhysicallyPresent)` | RED |
| D-11 | lote rechazado = mutacion cero | RED |
| D-12 | no crea modulos ni cambia tipos | RED |
| D-13 | un recompute por operacion | RED (UI) |
| D-14 | DISTRIBUTE no toca `Length` ni `IsManualOverride` del destino | RED |
| D-15 | reconstruccion por cambio de tarima conserva personalizadas por `ModuleId + Kind`, adapta fondo y peralte, e informa `Removed`/`Incompatible` | RED |
| D-16 | «Restaurar layout» descarta todo (reset intacto) | verde |
| D-17 | overrides por linea: I-53 nunca los escribe; uno entrante sobrevive a EDIT y DISTRIBUTE | RED + guarda |
| D-18 | L-1: «Aplicar» rapido, «Restaurar estandar» y «Abrir» del configurador se capturan | RED visto fallar en G7 |
| D-19 | RACKEDITAR con personalizadas distribuidas: procedencia y configuraciones identicas | RED |
| D-20 | Actualizar | verde + E2E |
| D-21 | guardar/reabrir `Modules[].Header` | verde `RackProjectStoreTests` + caso distribuido |
| D-22 | dibujo lateral, frontal y planta con personalizada de modulo | verdes de autoridad + escenario |
| D-23 | BOM del editor = BOM por la ruta de `RACKBOMTOTAL`, con `Members` presentes (AM-3) | guarda |
| D-24 | legacy: sin personalizadas, firma de dibujo de I-24 identica | verde |
| D-25 | cambio de tipo y reconstruccion → `Incompatible` informado (antes: perdida silenciosa) | RED (OD-2.b) |

### 12.4 Push Back (regresion I-40) y Owner Validation

- P-01: las suites de §8 verdes y sin cambios; P-02: cero diff de produccion de Push Back (comprobacion de
  gate); P-03: L-2 ni se corrige ni se fija.
- Checklist de G9. Selectivo: personalizar, tomar como origen, aplicar a uno, a varios y a todos, omisiones,
  altura severa (confirmar y cancelar), independencia, Actualizar, RACKEDITAR, guardar y reabrir, vistas por
  fondo, BOM del editor frente a `RACKBOMTOTAL`. Dinamico: L-1 con «Aplicar» rapido, tomar como origen, aplicar
  a uno, a varias y a todas, omision por frentes en blanco, independencia, cambio de tarima con informe de
  reconciliacion, «Restaurar layout», Actualizar, RACKEDITAR, guardar y reabrir, vistas, BOM del editor frente a
  `RACKBOMTOTAL`.

## 13. Archivos previstos

| Gate | Crear | Modificar | Solo leer |
|---|---|---|---|
| G2 | `docs/initiatives/I-53-proposal-v1.md` | contrato I-53 (ajustes minimos) | todo lo citado |
| G2 freeze | `docs/automation/decisions/I-53.md` | contrato; `docs/ideas-futuras.md` (hallazgos); ADR solo si se decide | — |
| G3 | `A/Systems/Shared/HeaderConfigurationSnapshot.cs`; Report, motivos y politica (nombres tentativos); pruebas nuevas en `TC/` | — | `RackFrameProjectStore.cs`, `RackFrameConfiguration.cs` |
| G4 | `A/Systems/Selective/SelectivePostTargets.cs` y la distribucion del Selectivo (nombres tentativos); pruebas nuevas en `TC/` | `A/Systems/Selective/SelectiveEditorState.cs`, `A/Systems/Selective/SelectiveCabeceraHeightReview.cs` | `SelectiveCabeceraAuthority.cs`, `SelectiveFondoTargets.cs` |
| G5 | pruebas nuevas en `TU/` | `U/Systems/Selective/RackSelectiveWindow.xaml/.cs`, `TU/SelectiveShellMigrationTests.cs` | — |
| G6 | `A/Systems/Dynamic/DynamicHeaderModuleTargets.cs` y la distribucion del Dinamico (nombres tentativos); pruebas nuevas en `TC/` | `A/Systems/Dynamic/DynamicEditorDesignAssembler.cs`; aserciones del Dinamico en `TC/PushBackModuleEditorCharacterizationTests.cs` | `RackModuleReconciliation.cs`, `RackModuleDescriptor.cs`, `DynamicFrontGeometry.cs` |
| G7 | pruebas nuevas en `TU/` | `U/Systems/Dynamic/RackDynamicSystemWindow.xaml/.cs`, `TU/DynamicShellMigrationTests.cs`; `Fact7` si cambia | `RackFrameConfiguratorWindow.xaml.cs` |
| G10 | — | `docs/HANDOFF.md`, `docs/ROADMAP.md` (momento 3), guias si aplica, contrato | — |

**No se tocan en ningun gate:** `A/Systems/PushBack/**`, `U/Systems/PushBack/**`, `RackModuleEditSession.cs`,
`RackModuleReconciliation.cs`, `RackFrameProjectStore.cs`, `RackFrameConfiguratorViewModel.cs`, los DTO y
disenos de Selectivo y Dinamico, `DynamicRackSystemResolver.cs`, Plugin, `assets/`, `deploy/`, `.github/`.

## 14. ADR_REQUIRED = PENDING_ARCHITECT

Criterios de `docs/adr/README.md:17-29`:

| Criterio | ¿Se cumple? |
|---|---|
| 1. Restringe trabajo futuro en mas de un modulo o capa (contratos) | **Probablemente si**, si AM-1 = B y AM-4 se aceptan como contrato comun que consumen dos sistemas |
| 2. Cara de revertir (formatos, nombres publicos, dependencias) | **No**: todo es transitorio y no persistido |
| 3. Cierra un debate recurrente | **Parcialmente**: «¿tipo universal de destino?», «¿copiar la semantica de Push Back?», «¿reutilizar el resolver de celdas?» ya se discutieron en I-40 e I-43 |
| 4. Excepcion a una convencion de AGENTS | **No** |

La regla de altura y la profundidad del Selectivo **no** necesitan ADR: aplican ADR-0032 D9. La reconciliacion
del Dinamico aplica la regla del Owner de I-35 («perder una personalizacion nunca es silencioso») a un sistema
nuevo. Si el Arquitecto confirma el contrato comun como publico y duradero, ADR **SI** antes de G3 (criterio 1);
si lo reduce a ayudantes internos por sistema, **NO**. No se reserva numero ni se crea borrador.

## 15. Riesgos

| Id | Riesgo | Mitigacion |
|---|---|---|
| R-01 | el G3 de I-50 retrasa G5 y G7 | orden G3 → G4 → G6 primero; OD-7 |
| R-02 | rebase sobre las ventanas tras integrar I-50 | no empezar G5/G7 antes; un solo rebase |
| R-03 | la reconciliacion cambia la reconstruccion del Dinamico | OD-2.b; D-15, D-25; validacion en AutoCAD |
| R-04 | ids posicionales: con otro numero de modulos, una personalizada puede quedar `Removed` o `Incompatible` | se informa; nunca silencioso |
| R-05 | la semilla de «Personalizar» del Selectivo reinicia la altura de una personalizada (asimetria con DISTRIBUTE) | OD-3.c; no se cambia sin decision |
| R-06 | L-2 sigue en Push Back y alguien compara BOMs | follow-up `fix/` registrado al cerrar G2 |
| R-07 | censos y guardas de I-35 tentados a relajarse | se actualizan en el gate del cambio, con justificacion |
| R-08 | Push Back (`StageInert`, instancia) y Dinamico (`OmitAndReport`, modulo) se comportan distinto | textos de UI explicitos; validacion del Owner |
| R-09 | un origen tomado y luego editado aplica la version tomada | rotulo «Origen» visible; exclusion `IsSource` |
| R-10 | la reapertura de `ExtraFondoPostCabeceras` por `LoadExisting` no estaba fijada | S-17 |

## 16. Decisiones pendientes

**Arquitecto**: AM-1 (Snapshot B), AM-2 (opcion A), AM-3 (fronteras sin tocar Push Back), AM-4 (Report y
politicas) — Anexo A.

**Owner**:

| Id | Pregunta | Recomendacion |
|---|---|---|
| OD-1 | Texto de ID6/ID7 | **RESUELTA** (§1.2) |
| OD-2 | Unidad de destino del Dinamico | `ModuleId` |
| OD-2.b | Adoptar la reconciliacion por `ModuleId + Kind` en la reconstruccion del Dinamico, con informe | si |
| OD-3.a | `PostTargets` del Selectivo | `Actual` / `Explicito` / `Todos`, sin recordar preferencia |
| OD-3.b | ¿DISTRIBUTE escribe `PostPeraltes` de los postes destino? | no; el peralte es del poste destino |
| OD-3.c | ¿Cambiar la semilla de altura de «Personalizar»? | no en I-53; queda registrada |
| OD-4 | Cobertura del Dinamico | `OmitAndReport` |
| OD-5 | L-1 y L-2 | L-1 dentro de I-53 (G7); L-2 follow-up `fix/` fuera de I-53 |
| OD-6 | Particion | una sola I-53, punto de control tras G4 y G6 |
| OD-7 | Serializacion con I-50 | serializar G5/G7 tras integrar I-50 |

## 17. Lo que V1 no decide y queda fuera

Nombres definitivos de tipos (G3); disposicion de controles WPF (G5, G7); reset por lotes; aplicar entre racks
del dibujo (ID21); unidad por instancia en el Dinamico; cualquier cambio de Push Back; L-2; migracion de
documentos; `SchemaVersion`; catalogos; Plugin.

---

## Anexo A — Paquete autonomo para el Arquitecto (AM-1..AM-4)

> Se puede leer sin el resto del repositorio. **No** pide rehacer el Discovery. Cada AM termina con la decision
> que se solicita. Respuesta esperada por AM: `AGREED`, `AGREED WITH CHANGES` (con los cambios) o `NOT AGREED`
> (con el motivo).

### A.0 Contexto minimo

- **Iniciativa**: I-53, rama `feature/cabeceras-configurables-multidestino` @ `c8476cc`; base `46fcac2`.
- **Objetivo**: en el **Selectivo** y el **Dinamico**, reutilizar una configuracion de cabecera existente como
  origen (**ID6 REUSE**: copia de valores authored, sin vinculo vivo) y aplicarla a un **conjunto** de destinos
  compatibles (**ID7 BATCH DISTRIBUTION**: origen y destinos separados, taxonomia por sistema).
- **Push Back ya lo tiene** (I-40): destinos cabeceras × lineas, copia canonica por par, sesion con
  Confirmar/Cancelar. Es precedente y regresion; **no se toca**.
- **Cadena obligatoria**: `SOURCE → SNAPSHOT/COPY → DESTINATION SET → PREPARE → MUTATE → RECOMPUTE`, con
  separacion origen/destinos, copia independiente, mutacion parcial cero, destinos resueltos y validados antes de
  escribir, informe deterministico, un recompute y ningun dato persistido nuevo.
- **Restriccion externa**: el G3 de I-50 ya modifico la ventana del Selectivo (`ed50cbd`) y modificara la del
  Dinamico; los gates de UI de I-53 esperan a que I-50 se integre.

### A.1 AM-1 — Snapshot inmutable o disciplina sobre `DeepCopy`

- **Codigo**: `src/RackCad.Application/Persistence/RackFrameProjectStore.cs:87-104`
  (`DeepCopy` = `Deserialize(Serialize(c))` + re-anexar `Exceptions`; `Deserialize` valida uso `:62-65` y
  reconstruye `Members` `:67`); `src/RackCad.Domain/RackFrames/RackFrameConfiguration.cs:5-65`.
- **Alias vivos medidos**: `SelectiveEditorState.cs:1214-1216` (delegado de copia opcional);
  `RackPushBackSystemWindow.xaml.cs:1837`, `:2360-2363`; `RackDynamicSystemWindow.xaml.cs:684`.
- **Alternativas**: A disciplina (cero tipos, aliasing por convencion); B `HeaderConfigurationSnapshot`
  transitorio e inmutable que **envuelve** una copia canonica privada, con `TryCapture` (fallo tipado) y
  `Materialize()` (una `DeepCopy` nueva por llamada), sin procedencia dentro.
- **Recomendacion**: **B**.
- **Riesgos**: un tipo mas; que alguien exponga la copia privada (lo impide la API y una guarda).
- **Decision solicitada**: ¿B? ¿Procedencia fuera del Snapshot? ¿Captura de `ExistingHeader` al tomarla como
  origen?

### A.2 AM-2 — Sesion del Dinamico y reutilizacion

- **Codigo**:
  - `src/RackCad.Application/Systems/Shared/RackModuleEditSession.cs:231-280` (`ApplyHeaderConfiguration`,
    `IsManualOverride = true` en `:275`);
  - `src/RackCad.Application/Systems/Shared/RackModuleReconciliation.cs:125-213` (longitud manual `:164`,
    `:186-191`; documentado como sustituto del par ordinal, `:81-97`);
  - `src/RackCad.Application/Systems/Shared/RackModuleDescriptor.cs:163-195` (presencia fisica);
  - `src/RackCad.Application/Systems/Dynamic/DynamicEditorDesignAssembler.cs:61-139` (par ordinal y altura en
    sitio);
  - `src/RackCad.UI/Systems/Dynamic/RackDynamicSystemWindow.xaml.cs:441-495` (recomposicion),
    `:667-724` (`EditHeader_Click`);
  - `src/RackCad.Application/Systems/PushBack/PushBackEditorState.cs:77-192` y
    `PushBackEditorDesignAssembler.cs:307-365` (lo que **no** se mueve);
  - `tests/RackCad.UI.Tests/RichEditorCloseContractTests.cs:76-85` (el Dinamico no declara ambito).
- **Hechos**: las operaciones de nivel modulo de la sesion no tienen llamador de produccion (solo pruebas); la
  sesion no cambia tipos de modulo; una sesion escenificada crea un ambito sucio (ADR-0029 D8).
- **Alternativas**: A nucleo por operacion + reconciliacion, sin sesion; B recompute dirigido por commit con
  sesion; C sesion escenificada completa.
- **Recomendacion**: **A**; consumir `RackModuleReconciliation`, `RackModuleDescriptor` y `DeepCopy` sin
  modificarlos; no mover ni duplicar nada de Push Back.
- **Riesgos**: cambio de la reconstruccion del Dinamico (lo decide el Owner); reapuntar dos guardas `Fact5` y,
  si aplica, `Fact7` de `tests/RackCad.Tests/PushBackModuleEditorCharacterizationTests.cs` (solo sus aserciones
  del Dinamico).
- **Decision solicitada**: ¿A? Si B o C: ¿operacion aditiva de sesion sin longitud manual?

### A.3 AM-3 — Fronteras de `Members` y `Exceptions` sin tocar Push Back

- **Codigo**: `DynamicRackSystemResolver.cs:212-238` y `:512-516` (`CloneHeader` sin refresco para overrides);
  `SystemRegistry.Default.cs:79-85` y `:180-186` (la carga solo refresca cabeceras de modulo);
  `src/RackCad.Application/Bom/BomBuilder.cs:74-93` (itera `Members`).
- **Hecho**: L-2 (fuera de alcance) afecta a overrides por linea fuera del editor; las cabeceras de **modulo**
  se refrescan en toda frontera.
- **Propuesta**: el Dinamico usa `ModuleId`; el nucleo materializa siempre por `DeepCopy`; guardas de BOM
  editor = BOM por la ruta de `RACKBOMTOTAL` en Selectivo y Dinamico; cero diff de produccion de Push Back; L-2
  ni se corrige ni se fija con prueba.
- **Riesgos**: si el Owner pide la unidad por instancia en el Dinamico, L-2 pasa a ser prerrequisito.
- **Decision solicitada**: ¿basta con evitar la ruta de overrides y las guardas, sin tocar el resolver?

### A.4 AM-4 — Report comun y politicas de cobertura

- **Codigo**: `src/RackCad.Application/Systems/Selective/SelectiveCabeceraApplyResult.cs:16-67` (hoy: aplicados
  y omitidos por fondo; «no se cambio nada» con escritura previa del peralte);
  `RackModuleEditSession.cs:396-458` (Push Back: rechaza ids desconocidos o separadores, no valida cobertura);
  `src/RackCad.Application/Systems/Selective/SelectiveCabeceraAuthority.cs:103` (profundidad ≤ 0 ignorada).
- **Propuesta**: `Report<TDireccion>` con `Rejected | Ready | Committed | Cancelled`; `Omitted` solo con
  `AbsentInScope`, `NotPhysicallyPresent` o `IsSource`; `Rejected` si el origen no se captura, la peticion esta
  mal formada, los Targets estan vacios, **Applied queda vacio**, la frontera previa falla o una normalizacion o
  copia no es valida; `OmitAndReport` para Selectivo y Dinamico; `StageInert` solo descriptivo de Push Back.
- **Riesgos**: diferencia visible entre Push Back y el Dinamico.
- **Decision solicitada**: ¿reglas Rejected/Omitted? ¿«Applied vacio ⇒ Rejected»? ¿Report generico sobre la
  direccion en vez de un tipo universal?

### A.5 Gates propuestos

`G2 (esta revision) → G3 nucleo → G4 Selectivo Application → G6 Dinamico Application → [I-50 integrada] → G5
Selectivo UI → G7 Dinamico UI → G8 Candidato → G9 Owner AutoCAD → G10 cierre` (detalle en §11).

### A.6 Archivos previstos

**Crear**:
- `src/RackCad.Application/Systems/Shared/HeaderConfigurationSnapshot.cs`
- Report, motivos y politica en `src/RackCad.Application/Systems/Shared/` (nombres tentativos)
- `src/RackCad.Application/Systems/Selective/SelectivePostTargets.cs` y la distribucion del Selectivo
- `src/RackCad.Application/Systems/Dynamic/DynamicHeaderModuleTargets.cs` y la distribucion del Dinamico
- pruebas nuevas en `tests/RackCad.Tests/` y `tests/RackCad.UI.Tests/`
- `docs/automation/decisions/I-53.md`

**Modificar**:
- `src/RackCad.Application/Systems/Selective/SelectiveEditorState.cs`
- `src/RackCad.Application/Systems/Selective/SelectiveCabeceraHeightReview.cs`
- `src/RackCad.Application/Systems/Dynamic/DynamicEditorDesignAssembler.cs`
- `src/RackCad.UI/Systems/Selective/RackSelectiveWindow.xaml` y `.xaml.cs` (bloqueado por I-50)
- `src/RackCad.UI/Systems/Dynamic/RackDynamicSystemWindow.xaml` y `.xaml.cs` (bloqueado por I-50)
- `tests/RackCad.UI.Tests/SelectiveShellMigrationTests.cs` y `DynamicShellMigrationTests.cs` (bloqueados por I-50)
- aserciones del Dinamico en `tests/RackCad.Tests/PushBackModuleEditorCharacterizationTests.cs`

**No tocar**: `src/RackCad.Application/Systems/PushBack/**`, `src/RackCad.UI/Systems/PushBack/**`,
`RackModuleEditSession.cs`, `RackModuleReconciliation.cs`, `RackFrameProjectStore.cs`,
`RackFrameConfiguratorViewModel.cs`, DTO y disenos de Selectivo y Dinamico, `DynamicRackSystemResolver.cs`, Plugin.

### A.7 Pregunta de ADR

¿AM-1 + AM-4 son un contrato comun publico y duradero (ADR antes de G3) o ayudantes internos por sistema (sin
ADR)? Recomendacion del ejecutor: **decide el Arquitecto** (§14).
