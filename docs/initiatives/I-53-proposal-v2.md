# I-53 — Proposal V2 (G2B): ID6 REUSE + ID7 BATCH DISTRIBUTION de la configuracion de cabecera

> **Esto es G2 en curso, no G2 cerrado.** V2 reconcilia la [Proposal V1](I-53-proposal-v1.md) —que se conserva
> intacta como historial— con la revision de Arquitecto (`AGREED WITH CHANGES`) y con las tres decisiones del Owner
> ya aprobadas (OD-2.b, OD-6 B′, OD-8). Fija el **contrato recomendado** y prepara una **re-revision acotada** del
> Arquitecto (Anexo A). **No implementa nada, no abre G3, no toca UI, pruebas, produccion ni Push Back, y no
> integra.** Solo sera vinculante con `ARCHITECT_V2 = AGREED` y el freeze posterior de G2.
>
> Entradas: [Discovery G1](I-53-discovery.md), [Proposal V1](I-53-proposal-v1.md),
> [contrato](I-53-cabeceras-configurables-multidestino.md), [registro de decisiones](../automation/decisions/I-53.md)
> y [ADR-0037 propuesto](../adr/0037-reutilizacion-de-cabecera-por-copia-y-distribucion-por-lotes.md).

```text
Initiative     = I-53
Owner IDs      = ID6 (REUSE) + ID7 (BATCH DISTRIBUTION)          OD-1 = RESUELTA (autorizacion original)
Branch         = feature/cabeceras-configurables-multidestino
BASE_SHA       = 46fcac2b071929d2bd5b07aa28373941417f74a8
CLAIM_SHA      = e1d5996e70cb75589beb96a256714982fe693be3   (Claim-Id d7144fe8-6921-4a46-9d66-2d723620bea4)
G1_SHA         = c8476cccc98809fdb7fd0aeccef288521126f442
PROPOSAL_V1    = f38362d32a737f62d69a229854dc5c1812adf063   (HEAD al abrir G2B; historial, no se modifica)
Proposal       = V2 (este documento)
Architect V1   = AGREED WITH CHANGES   (AM-1..AM-4 = AGREED WITH CHANGES)
Owner          = OD-2.b APPROVED · OD-6 APPROVED (B′, tres integraciones) · OD-8 APPROVED
Architect V2   = PENDING (re-revision acotada, Anexo A)
G2             = OPEN (no congelado)
G3             = NOT OPENED; implementacion BLOQUEADA
ADR_REQUIRED   = YES → ADR-0037 `propuesto` (no aceptado; solo el Owner acepta)
PROCESO        = PA-1 ELEVADA al Coordinador/Arquitecto (tres integraciones frente a WORKFLOW literal; §10)
NUEVO          = N-01 elevado (reapertura avanzada en el Selectivo; §17)
```

## 0. Preflight de G2B y alcance de este documento

Preflight del 2026-09-12 a las 20:38 -06:00, tras `git fetch --all --prune`:

| Punto | Observado |
|---|---|
| Rama I-53 | HEAD = upstream = `f38362d`, `0/0` frente a su upstream; 4 commits delante y 0 detras de `origin/main`; arbol limpio |
| `main` | `origin/main` = `46fcac2` (no avanzo desde la base: **sin rebase**) |
| Stash / operaciones | ninguno; sin `MERGE_HEAD`, `REBASE_HEAD`, `CHERRY_PICK_HEAD`, `REVERT_HEAD`, `BISECT_LOG`, `rebase-*` ni `sequencer` |
| Worktrees | seis: principal (`main` @ `46fcac2`), I-49, I-50, I-52, I-53 e I-54, cada uno en su rama |
| CI de V1 | run `34730730052` (`push`, `head_sha` = `f38362d`) = **success** |

Ramas paralelas (las puntas de V1 son las de su re-verificacion de las 19:26):

| Iniciativa | Rama | Punta en V1 | Punta en G2B | Que cambio | Archivos / fuera de `docs/` | Detras / delante de `main` |
|---|---|---|---|---|---|---|
| I-49 | `architecture/motor-expresiones-parametricas` | `ccf21c6` | `2783acc` | G2E: Proposal V5 (solo docs) | 9 / 0 | 9 / 8 |
| I-50 | `feature/cotas-independientes-por-vista` | `ed50cbd` | **`6cd2970`** | **G3 (B)** 19:32: ventana del Dinamico (C-07); **G3 (C)** 19:55: ventana de Push Back (C-12) | 62 / 52 | 9 / 19 |
| I-52 | `feature/rackmirror-espejo-semantico` | `339b3ab` | `0fc7032` | G2: Proposal V1, **ADR-0036** `propuesto` y `decisions/I-52.md` (solo docs) | 7 / 0 | 0 / 5 |
| I-54 | `architecture/propiedades-personalizadas` | `7c197af` | `36c337b` | G2C: Proposal V2 y paquete de Arquitecto (solo docs) | 6 / 0 | 0 / 5 |

**Cambios materiales respecto de V1 (auditados en §11):**

1. I-50 completo su G3 sobre **las tres** ventanas ricas y sus pruebas: la veda del Coordinador sobre las dos ventanas
   de I-53 esta **activa** para ambas (§11.1).
2. El solape de I-50 con los archivos de Application que la Entrega 1 podria tocar **no cambio**: +1 linea en
   `SelectiveEditorState.cs` (`:1318`), +1 en `DynamicEditorDesignAssembler.cs` (`:175`) y +2 en
   `DynamicRackSystemResolver.cs` (`:245`, `:359`). Es **textual**, en regiones que I-53 no toca (§11.2).
3. **Numeracion de ADR**: 0035 lo usa I-50 y 0036 I-52, ambos en sus ramas; I-49 e I-54 declaran ADR sin numero. El
   primer libre es **0037** (§15).
4. **Cruce futuro, no activo**: I-49 V5 preve modificar un miembro de `RackSelectiveWindow.xaml.cs` en su G10
   (`I-49-proposal-v5.md:2756`); con el G5 de I-53 sera un archivo caliente compartido que exige serializacion (§11.3).

**Re-verificacion antes de publicar (21:22 -06:00, `git fetch --all --prune`):** `main` sigue en `46fcac2` y **ninguna**
rama remota cambio de punta desde las 20:38 (I-49 `2783acc`, I-50 `6cd2970`, I-52 `0fc7032`, I-54 `36c337b`); I-50
sigue **sin integrar**; ningun ref cita ADR-0037.

Este documento **no** es un Discovery nuevo. Solo re-lee codigo cuando una correccion lo exige, y lo cita contra el
arbol de `46fcac2`. Clases de evidencia: **HECHO** (codigo leido y linea verificada en esta sesion), **DOC**,
**INFERENCIA**. Rutas abreviadas: `A/` = `src/RackCad.Application/`, `D/` = `src/RackCad.Domain/`, `U/` =
`src/RackCad.UI/`, `TC/` = `tests/RackCad.Tests/`, `TU/` = `tests/RackCad.UI.Tests/`.

## 1. Entradas vinculantes (no se reabren)

### 1.1 Clasificacion de G1 (sin cambio)

Selectivo y Dinamico **EN ALCANCE**; Push Back **ALREADY DONE** (I-40: precedente y regresion); Cabecera
independiente, Cantilever, Cama, Larguero y Drive-In **N/A** (V1 §1.1).

### 1.2 OD-1 — RESUELTA por la autorizacion original

```text
ID6 — REUSE
  Una configuracion de cabecera EXISTENTE como SOURCE. Reutilizar = COPIAR sus valores authored.
  NO live link, NO instancia mutable compartida, NO referencia viva source ↔ destination.
ID7 — BATCH DISTRIBUTION
  Una source configuration se aplica a un CONJUNTO de destinos compatibles.
  Source y Destinations son conceptos SEPARADOS. La taxonomia de destinos NO es universal.
```

### 1.3 Decisiones del Owner APROBADAS (orden G2B del Coordinador, 2026-09-12)

| Id | Decision aprobada | Donde aterriza |
|---|---|---|
| **OD-2.b** | En el Dinamico, al reconstruir por cambios de tarima o fondos: conservar cabeceras personalizadas por identidad de modulo cuando sea posible y longitudes manuales cuando corresponda, **usando la reconciliacion**, adaptando lo compatible e informando Preserved / Adapted / Removed / Incompatible / Restored (taxonomia real vigente). Nada se pierde en silencio | §7.9 (E1 = G6, sin cablear; E3 = G7, cableado e informe) |
| **OD-6** | **B′**: I-53 sigue siendo **una** iniciativa conceptual de ID6 + ID7 con **un** contrato comun, entregada en **tres integraciones**: E1 fundacion sin cambio visible (G3 + G4 + G6), E2 UI del Selectivo (G5), E3 UI del Dinamico (G7). Cada entrega con su propio Candidato, suites, CI, integracion y validacion proporcional. Sin I-53A/I-53B | §9, §10, §12 |
| **OD-8** | Retirar los presets «Personalizada N» del Dinamico. La reutilizacion queda **unificada** bajo ID6 + ID7 (cabecera existente como SOURCE → destinos → copias independientes → apply atomico). La retirada ocurre en el gate del Dinamico, **no en G2** | §7.11 (E3 = G7) |

### 1.4 Veredicto del Arquitecto sobre V1

| Punto | Veredicto |
|---|---|
| AM-1 Snapshot | **AGREED WITH CHANGES** (§4) |
| AM-2 Sesion del Dinamico | **AGREED WITH CHANGES** (§7.2) |
| AM-3 `Members`/`Exceptions` sin tocar Push Back | **AGREED WITH CHANGES** (§5) |
| AM-4 Resultado comun | **AGREED WITH CHANGES** (§3) |
| ADR | **ADR_REQUIRED = YES** (§15) |
| L-1 / L-2 | L-1 **dentro** de I-53 con su companera de reapertura avanzada; L-2 **fuera** |
| Hallazgos | H-01..H-07 (HIGH), M-01..M-11 (MEDIUM), L-01..L-06 (LOW); **ningun BLOCKER** |

> **Nota de transparencia.** La revision de Arquitecto de V1 la ejecuto **el mismo agente** que redacto el Discovery,
> la Proposal V1 y esta V2, en el rol asignado en cada sesion. Se declara como hicieron I-47, I-51 e I-52; una
> revision por un tercero independiente sigue siendo posible si el Coordinador la pide.

### 1.5 Restricciones del Coordinador para G2B

- Solo docs: V2, ajustes minimos al contrato, registro de decisiones y ADR `propuesto` si el proceso lo permite. Ni
  HANDOFF ni ROADMAP.
- **Veda I-50** (§11.1), sin `OWNER_OVERRIDE` como camino normal.
- **Push Back**: cero diff de produccion; sin prueba que canonice L-2.
- No crear ramas; no abrir G3; no cerrar G2.

## 2. Cierre de la revision del Arquitecto

### 2.1 HIGH — hallazgo → correccion V2 → seccion → prueba futura

| Hallazgo | Correccion en V2 | Seccion | Prueba futura (§13) |
|---|---|---|---|
| **H-01** — arreglar L-1 sin reabrir las personalizadas en editor avanzado destruye la receta en silencio (el configurador arranca en «Configuracion rapida» y su «Aplicar» regenera desde plantilla) | L-1 dentro de E3: el configurador se abre sobre una **copia**, se lee `window.Configuration` y una cabecera **ya personalizada** se abre con `IsAdvancedEditor = true` desde un seam de presentacion, sin tocar `RackFrameConfiguratorWindow` (I-35, decision 5); limite residual declarado | §7.12 | D-28..D-31 (RED vistos fallar en G7) |
| **H-02** — V1 §4.2 rechazaba fondos o postes inexistentes, contra I-43 | Fondo inexistente y poste ausente en un fondo → **`Omitted(AbsentInScope)`**, nunca `Rejected`, como `SelectiveTargetResolver.cs:50-57`, `SelectiveTargetPlan.cs:62-63` y ADR-0032 (`:55`, `:149`) | §3.5, §6.4 | S-09, S-10 |
| **H-03** — el Report mezclaba estado de la operacion y resultado por destino (`Ready(Applied[])` antes de escribir) | **Plan** (`Rejected` / `Prepared`) separado del **Outcome** (`Rejected` / `Cancelled` / `Committed`); `Applied` solo existe en `Committed` | §3.4 | C-05, C-06 |
| **H-04** — `IsManualOverride` sobrecargado; el preset ya lo fija al copiar | Para ID6/ID7 **`IsManualOverride` = solo longitud manual**; la personalizacion se expresa con `UseCalculatedHeaderConfiguration = false`; DISTRIBUTE no toca `Length` ni `IsManualOverride`; EDIT solo por la regla existente de fondo editado; la ruta del preset que lo fija se elimina en G7; «Calculada» se caracteriza sin cambio | §7.3 | D-12, D-14, D-15, D-26 |
| **H-05** — presets «Personalizada N» = segundo mecanismo de reutilizacion | OD-8: se retiran en G7; el unico mecanismo es ID6 + ID7 y el origen es siempre una cabecera existente | §7.11 | D-27 |
| **H-06** — capturar al «Tomar como origen» creaba un portapapeles invisible | El origen es una **direccion**; se captura **al aplicar**, con el valor ACTUAL; editar el origen despues cambia lo que se aplica; si la direccion ya no designa una cabecera → `SourceNotFound` | §3.3, §6.2, §7.4 | S-06, S-07, D-06, D-07 |
| **H-07** — `ModuleId` posicionales: podar destinos tras una reconstruccion puede reorientar en silencio | Firma de topologia = secuencia `ModuleId:Kind` + **generacion de reconstruccion**; toda reconstruccion invalida origen y destinos explicitos (informado); peticion con firma distinta → `Rejected(StaleTargets)`; plan con firma distinta en MUTATE → fail closed | §7.6 | D-16, D-17, D-18 |

### 2.2 MEDIUM de la re-revision — M-01..M-05 y M-09

| Hallazgo | Cierre en V2 | Seccion |
|---|---|---|
| **M-01** — la normalizacion de peralte citaba la regla en linea de la ventana | Autoridad unica **`SelectivePostGeometry.PostPeralteAt`** (`A/Systems/Selective/SelectivePostGeometry.cs:45-49`): la copia lleva el peralte efectivo del poste destino **tras** la operacion. V2 **precisa** la fuga verificada: visible en planta solo con peralte efectivo `<= 0` (`PlantaHeaderLayoutBuilder.cs:68-73`), incoherencia en escritura siempre; prueba S-16 | §6.7 |
| **M-02** — normalizar en PREPARE del Dinamico duplicaria el Builder | PREPARE solo verifica precondiciones (`IsHeader`, `Length > 0`, presencia fisica); `Depth` y peralte los impone el RECOMPUTE canonico (`DynamicRackSystemBuilder.ApplyPostPeralte` `:179-194` + `Refresh` `:137-152`) | §7.8 |
| **M-03** — la frontera C4 aparecia como `Rejected` del lote | La frontera previa (`CommitPendingEditors` → `SaveWorkingToSelected`) es **anterior y externa** al lote: si falla, el gesto aborta sin plan ni Outcome; no es un codigo `Rejected`; prueba de aislamiento S-27 | §3.11, §6.8 |
| **M-04** — pureza de PREPARE subespecificada | Definicion de estado observable; lista de lecturas permitidas y de ayudantes prohibidos sobre instancias guardadas (`EffectiveCustomAt`, `SyncPostCabeceras`, `ImposeFondoDepth`, `Refresh`, `ApplyPostPeralte`, `Reconcile`); vida del plan = un gesto, con firma; pruebas S-13, D-13 | §3.8, §3.9 |
| **M-05** — `TryCapture` sin fallos definidos | `Failed(NullSource)` / `Failed(UnusableHeader)` con validacion **antes** de copiar y **sin catch-all** alrededor de `DeepCopy` | §4 |
| **M-09** — brechas materiales de la matriz | Matriz reescrita con las 18 exigencias del Coordinador mapeadas a pruebas; redundancias fusionadas; clasificaciones RED corregidas | §13 |

### 2.3 Resto de MEDIUM y LOW

| Hallazgo | Tratamiento | Seccion |
|---|---|---|
| M-06 `HeaderCoveragePolicy`/`StageInert` en Shared | **Eliminado**: Shared no representa Push Back; Selectivo y Dinamico = omitir e informar | §3.13, §5 |
| M-07 la tabla de OD pedia al Owner decisiones tecnicas | Tabla final: resueltas por arquitectura, tecnicas y **solo** tres del Owner (ya aprobadas) | §16 |
| M-08 la particion ignoraba el acoplamiento de revalidacion por SHA exacto | Motivo decisivo de B′: E2 se integra antes de empezar G7, asi que una ronda del Dinamico no revalida el Selectivo | §9 |
| M-10 ADR pendiente | ADR-0037 `propuesto` con alcance cerrado | §15 |
| M-11 foto de paralelas desactualizada | Foto de las 20:38 | §0, §11 |
| L-01 RED de compilacion; C-06 y P-03 mal clasificadas | RED siempre por asercion con andamiaje compilable; C-06 → guarda (C-08); P-03 → comprobacion de gate | §13.1 |
| L-02 redundancias S-10/S-12, S-18/S-20, D-20/D-22, C-02/C-03 | Fusionadas en S-10, S-29, D-36 y C-02 | §13 |
| L-03 redaccion de D-17 | Reescrita como D-23: una reconstruccion **hoy** pierde los overrides por linea; se caracteriza sin cambiar | §7.10, §13 |
| L-04 S-22 frente a L-7 | S-25 excluye el escenario de L-7, que cambia por diseno y cubre S-11 | §13 |
| L-05 retirada del par ordinal sin especificar | Se retira en G7 solo cuando la ventana deja de llamarlo; sus pruebas se reapuntan sin relajarse | §7.9 |
| L-06 proyeccion de intenciones sin fijar | `DynamicRackSystemResolver.Snapshot(...).Modules` (`:291`, `:406-419`), sin mapeador paralelo | §7.9 |
| Exclusion de postes de medio frente (revision, punto 9) sin prueba | Cubierta en S-18 | §6.3, §13 |

### 2.4 Que cambia respecto de V1 (resumen)

- DISTRIBUTE ya no admite «resultado del configurador tomado como origen»: el origen es siempre una cabecera
  existente, capturada al aplicar (H-05, H-06).
- El Report desaparece: Plan + Outcome, con codigos cerrados (H-03, H-02).
- `Rejected` por inexistencia pasa a `Omitted(AbsentInScope)`; la frontera C4 sale del lote (H-02, M-03).
- Nuevo concepto de **firma** (peticion y plan) y generacion de reconstruccion en el Dinamico (H-07, M-04).
- Semantica de `IsManualOverride` fijada; presets retirados en G7 (H-04, H-05, OD-8).
- La normalizacion del Dinamico se delega al recompute canonico (M-02); la del peralte del Selectivo usa
  `PostPeralteAt` (M-01).
- L-1 exige reapertura avanzada (H-01); se detecta el mismo riesgo en el Selectivo y se eleva como N-01 (§17).
- Una iniciativa, **tres integraciones** (OD-6 B′), con la adaptacion procesal PA-1 elevada (§10).
- `ADR_REQUIRED = YES`: ADR-0037 `propuesto` (§15).
- `HeaderCoveragePolicy`/`StageInert` fuera de Shared (M-06).

## 3. Contrato comun (AM-4 final)

### 3.1 Vocabulario

| Termino | Definicion |
|---|---|
| **Direccion de origen** | posicion de una cabecera EXISTENTE en la taxonomia de su sistema: Selectivo `(FondoIndex, PostIndex)`; Dinamico `ModuleId` con la firma de §7.6. La UI puede recordar una **direccion**, nunca una configuracion |
| **Source** | la configuracion que se copia: en DISTRIBUTE, la que la direccion designa **al aplicar**; en EDIT, el resultado del configurador |
| **Snapshot** | copia canonica privada y transitoria de la Source, creada **dentro** de PREPARE (§4) |
| **Targets** | la intencion del usuario sobre los ejes del sistema: Selectivo `TargetFondos × PostTargets`; Dinamico `ModuleTargets`. No es un conjunto de direcciones resuelto |
| **Destination set** | Targets resueltos contra la topologia vigente: aplicables + omitidos con motivo |
| **Plan** | resultado de PREPARE: `Rejected(code)` o `Prepared(...)`. **Nada aplicado** |
| **Outcome** | resultado final de la operacion: `Rejected`, `Cancelled` o `Committed` |
| **Firma** | valor inmutable, comparado por igualdad, que resume la topologia y las entradas de normalizacion que PREPARE leyo (§3.9) |
| **Frontera previa de edicion** | lo que el editor compromete antes del gesto (Selectivo: C4, ADR-0032 D6); **no** forma parte del lote (§3.11) |

**No existe un tipo universal de destino.** Push Back conserva su `(PostIndex, ModuleId)` de I-40 y no consume este
contrato.

### 3.2 Dos operaciones, un protocolo

| Operacion | Source | Destinos | Donde |
|---|---|---|---|
| **EDIT** | resultado del configurador (`window.Configuration`), abierto sobre una **copia** | Selectivo: poste visible × `TargetFondos`; Dinamico: modulo seleccionado | «Personalizar» / «Editar cabecera» (existen hoy) |
| **DISTRIBUTE** (ID6 + ID7) | cabecera existente designada por su direccion | Targets elegidos por el usuario | «Tomar como origen» (recuerda la direccion) + «Aplicar a la seleccion» (nuevo) |

EDIT es el caso degenerado de DISTRIBUTE: la misma secuencia, la misma copia y el mismo Outcome. En EDIT no hay
exclusion `IsSource`, porque su Source no tiene direccion.

### 3.3 Secuencia exacta

```text
frontera previa de edicion                (externa al lote; si falla, el gesto aborta: §3.11)
PREPARE  (pura; §3.8)
  0. verificar la firma de la peticion     (solo Dinamico: origen y destinos explicitos recordados; §7.6)
  1. resolver la direccion de origen       (DISTRIBUTE)
  2. capturar la Source ACTUAL             (TryCapture sobre el valor vigente; §4)
  3. resolver los Targets                  (contra la topologia vigente)
  4. clasificar omitidos y malformados
  5. validar los destinos aplicables
  6. materializar TODAS las copias         (una por destino aplicable)
  7. normalizar las copias segun el sistema (Selectivo: aqui; Dinamico: delegado al RECOMPUTE, §7.8)
  8. validar las copias preparadas
  9. revisiones propias                    (Selectivo: altura por (fondo, poste) → Warnings)
 10. producir el Plan                      (Rejected(code) | Prepared(Targets, Omitted, Warnings, firma))
CONFIRM  (opcional: solo si algun Warning lo exige) → continua | Cancelled
MUTATE   (Prepared) → verificar firma → asignaciones → Committed(Applied, Omitted)
RECOMPUTE  uno, posterior a MUTATE
```

**No existe portapapeles oculto.** Ninguna configuracion se captura al pulsar «Tomar como origen»: solo se recuerda la
direccion. Si el usuario edita despues esa cabecera, Apply usa su estado actual; si la direccion ya no designa una
cabecera, `SourceNotFound`.

### 3.4 Plan y Outcome

```text
PREPARE → Plan
  Rejected(code)                                        nada preparado, nada aplicado
  Prepared(Targets[], Omitted[], Warnings[], firma)      nada aplicado

CONFIRM (opcional)
  sin Warning que exija confirmacion → continua
  el usuario no confirma             → Outcome = Cancelled        (mutacion CERO, recomputes CERO)

MUTATE(Prepared)
  firma vigente ≠ firma del plan     → Outcome = Rejected(StaleTargets)   (mutacion CERO; §3.9)
  si no                              → Outcome = Committed(Applied[], Omitted[])

Outcome final ∈ { Rejected, Cancelled, Committed }
```

- **`Applied` solo existe en `Committed`**, y es exactamente la lista `Targets` del plan, en el mismo orden. Nunca
  se informa «aplicado» antes de escribir.
- `Cancelled` no es `Rejected`: `Rejected` = peticion invalida o insegura; `Cancelled` = el usuario decidio no
  aplicar una peticion valida.
- `Warnings[]` = avisos no bloqueantes con severidad `Informativo` o `Severo`; CONFIRM se exige si y solo si hay
  alguno `Severo`. Hoy solo los produce la revision de altura del Selectivo.

### 3.5 Omitted — la direccion es valida en la taxonomia, pero no hay instancia aplicable

| Motivo | Definicion | Selectivo | Dinamico |
|---|---|---|---|
| `AbsentInScope` | la taxonomia expresa la direccion, pero la topologia vigente no tiene esa posicion | fondo inexistente; poste ausente en ese fondo (`PostExistsIn` falso, `SelectiveEditorState.cs:1166-1170`) | — |
| `NotPhysicallyPresent` | el destino existe y esta bien dirigido, pero no se dibuja | — | modulo de cabecera sin presencia fisica (`RackModuleDescriptor.IsPhysicallyPresent`, `:101`) |
| `IsSource` | el destino es la direccion del origen | la direccion origen aparece en el destination set | el `ModuleId` origen aparece en el destination set |

Un Omitted **no** invalida a los demas destinos. Nunca se clampea a un vecino, nunca se crea un destino y nunca se
resucita una configuracion podada.

### 3.6 Rejected — la operacion entera es invalida; mutacion CERO, incluidos los escalares

| Codigo | Cuando | Selectivo | Dinamico |
|---|---|---|---|
| `SourceNotFound` | la direccion de origen no designa una cabecera en la topologia vigente | el poste origen no existe en su fondo, o el fondo no existe | el `ModuleId` no existe o ya no es cabecera en la firma vigente |
| `SourceUnusable` | la direccion resuelve, pero la Source no es elegible o `TryCapture` falla (`NullSource`, `UnusableHeader`) | cabecera estandar (sin personalizada) o `Height <= 0` segun `UsableCustomAt` | cabecera calculada, no presente fisicamente, o no usable |
| `NoTargets` | la intencion no tiene destinos | peticion sin fondo o sin poste | `Actual` sin modulo de cabecera seleccionado; conjunto vacio |
| `MalformedTarget` | una direccion no pertenece al universo de la taxonomia | indice de poste negativo | `ModuleId` vacio o desconocido con firma vigente; un separador |
| `StaleTargets` | la peticion o el plan se capturaron contra otra topologia | plan con firma distinta en MUTATE | peticion con firma o generacion anterior; plan con firma distinta en MUTATE |
| `DestinationInvalid` | un destino **aplicable** es invalido, o su copia normalizada no es usable | `CabeceraDepthOfFondo(fondo) <= 0` | `Length <= 0` |
| `NoApplicableTargets` | todos los destinos resueltos quedaron omitidos | todo `AbsentInScope`/`IsSource` | todo `NotPhysicallyPresent`/`IsSource` |

**Todo o nada.** Si un solo destino aplicable es invalido, se rechaza **todo el lote**: con destinos 1..N−1 validos y
el N invalido, la mutacion es cero en todos (S-12, D-11).

### 3.7 Precedencia determinista

```text
1. StaleTargets          (paso 0: la peticion; §7.6)
2. SourceNotFound
3. SourceUnusable
4. NoTargets
5. MalformedTarget       (primer destino malformado en orden de informe)
6. NoApplicableTargets   ┐ mutuamente excluyentes: 0 aplicables frente a >= 1 aplicable invalido
   DestinationInvalid    ┘ primer aplicable invalido en orden de informe; antes la validacion previa a la copia
                           (paso 5) que la de la copia normalizada (paso 8)
```

`StaleTargets` va primero porque una peticion capturada contra otra topologia no se evalua por partes: sus ids pueden
coincidir por azar con modulos distintos. En MUTATE, la verificacion de la firma del plan es previa a la primera
asignacion.

### 3.8 Pureza de PREPARE

**Estado observable** = estado del editor, diseno, sistema resuelto, UI y contadores de recompute. PREPARE no cambia
nada de eso; las **copias privadas** que materializa si pueden mutarse (normalizacion).

| Permitido (lecturas comprometidas) | Prohibido sobre instancias guardadas o sobre el sistema vivo |
|---|---|
| Selectivo: `CabeceraAt`, `SelectiveCabeceraAuthority.UsableCustomAt` (pura, `:51-55`), `PostExistsIn`, `MaxFrenteCount`, `CabeceraDepthOfFondo`, `TargetFondos`, `SelectivePostGeometry.PostPeralteAt`, la revision de altura | `EffectiveCustomAt` (normaliza en sitio, `:78-88`), `SyncPostCabeceras` (poda, `SelectiveEditorState.cs:739-762`), `ApplyCabeceraToTargets`, `ImposeFondoDepth` sobre una configuracion guardada, `SaveWorkingToSelected` |
| Dinamico: `system.Modules` (lectura), `RackModuleDescriptor.Describe` (`:163`), escalares de modulo | `DynamicRackSystemBuilder.Refresh`/`ApplyPostPeralte`, `RackModuleReconciliation.Reconcile`, cualquier asignacion a un modulo |
| Ambos: `HeaderConfigurationSnapshot.TryCapture` y `Materialize` | cualquier recompute; abrir dialogos; escribir en la UI |

`ImposeFondoDepth` **si** se usa en PREPARE, pero solo sobre la **copia** materializada, que es donde el Selectivo
normaliza la profundidad (§6.6).

### 3.9 Vida y firma del plan

- El plan **vive un solo gesto**: no se guarda, no cruza recomputes y no se reutiliza.
- Lleva una **firma** suficiente de la topologia y de las entradas de normalizacion que PREPARE leyo:
  - Selectivo, como minimo: fondos y frentes por fondo (`SelectiveTopology`), `MaxFrenteCount`, la profundidad de
    cabecera de cada fondo destino y el peralte efectivo de cada poste destino.
  - Dinamico, como minimo: la secuencia ordenada `ModuleId:Kind` y la generacion de reconstruccion (§7.6).
- MUTATE recalcula la firma **antes** de la primera asignacion. Si no coincide: `Rejected(StaleTargets)`, mutacion
  cero, sin recompute. **Nunca** se re-resuelve ni se reorienta un destino en MUTATE.

### 3.10 MUTATE y RECOMPUTE

- **MUTATE** solo asigna lo preparado y los escalares precomputados: ninguna copia, resolucion, validacion, lectura de
  catalogo ni dialogo. No puede fallar razonablemente; la unica verificacion es la firma, previa a toda asignacion.
- **RECOMPUTE**: exactamente **uno** por `Committed`; **cero** en `Rejected` (incluido `StaleTargets` en MUTATE) y
  en `Cancelled`. El recompute de la frontera previa, si lo hubo, se cuenta aparte (§3.11).

### 3.11 Frontera previa de edicion

```text
Selectivo:  CommitPendingEditors → SaveWorkingToSelected → si falla: el gesto termina (sin plan, sin Outcome)
            → PREPARE
Dinamico:   la recomposicion sincrona existente; si reconstruye, invalida origen y destinos (§7.6) → PREPARE
```

- En el Selectivo es la C4 de ADR-0032 D6 (HECHO `U/Systems/Selective/RackSelectiveWindow.xaml.cs:558-615`; ya la
  usan «Personalizar» en `:1171` y `:1180` y los comandos de escritura). **No es un codigo `Rejected`**: es una
  precondicion externa y su fallo deja cero mutacion ID6/ID7. Si habia campos pendientes validos, su recompute es
  propio y no cuenta como el de la operacion.
- `SyncPostCabeceras` pertenece a esta frontera (o a la sincronizacion estructural), nunca a PREPARE.

### 3.12 Orden determinista

- Selectivo: `(FondoIndex, PostIndex)` ascendente para Targets, Applied, Omitted y Warnings.
- Dinamico: `Index` longitudinal ascendente.
- Un Omitted ocupa su posicion en ese orden, con su motivo.

### 3.13 Que es comun y que no

| Pieza | Comun (`A/Systems/Shared`) | Especifico por sistema |
|---|---|---|
| Snapshot | **si** (§4) | — |
| Plan, Outcome, `Warnings`, motivos Omitted y codigos Rejected | **si**, como tipos de valor genericos sobre la direccion | la direccion, la firma y el orden |
| Ejecutor PREPARE/MUTATE | **no**: no hay ejecutor generico | cada sistema implementa el protocolo sobre su estado |
| Politica de cobertura / `StageInert` | **no existe** | Selectivo y Dinamico omiten e informan; Push Back se describe solo en prosa (§8) |
| Targets, gramatica y resolver | no | Selectivo `TargetFondos × PostTargets`; Dinamico `ModuleTargets` |
| Normalizacion | no | Selectivo: `ImposeFondoDepth` + `PostPeralteAt` en PREPARE; Dinamico: recompute canonico |
| Revisiones | no | Selectivo: altura (ADR-0032 D9) |
| Frontera previa y recompute | no | Selectivo: C4 + `DeferRecompute`; Dinamico: recomposicion |

## 4. AM-1 final — `HeaderConfigurationSnapshot`

**Congelado como contrato recomendado.** `sealed`, transitorio, **no persistido**, en `A/Systems/Shared`; no expone la
copia mutable interna; no contiene procedencia; no contiene UI ni AutoCAD.

```text
TryCapture(source)
  → Captured(snapshot)
  | Failed(NullSource)
  | Failed(UnusableHeader)

Materialize()
  → una NUEVA RackFrameConfiguration por llamada
```

| # | Regla |
|---|---|
| 1 | `source == null` → `Failed(NullSource)` |
| 2 | `!RackDesignValidation.IsUsableHeader(source)` → `Failed(UnusableHeader)` |
| 3 | se valida **antes** de copiar: una Source invalida nunca llega a `DeepCopy` |
| 4 | copia privada = `RackFrameProjectStore.DeepCopy(source)` (HECHO `A/Persistence/RackFrameProjectStore.cs:87-104`) |
| 5 | **sin catch-all** alrededor de `DeepCopy`: tras la validacion explicita, una excepcion es un defecto y se propaga (I-03) |
| 6 | `Materialize()` = una `DeepCopy` nueva de la copia privada en cada llamada |
| 7 | nunca expone la copia privada (ni getters de objetos, ni colecciones, ni setters) |
| 8 | nunca se guarda entre gestos |
| 9 | nace dentro de PREPARE y muere con la operacion |
| 10 | no crea JSON, DTO ni serializacion nuevos |

**Copia profunda** significa, sobre el grafo real de `RackFrameConfiguration` (V1 §3.3): source ≠ destination;
destination A ≠ destination B; listas, colecciones y objetos authored mutables (`Horizontals`, `BracingPanels`,
`LeftPost`/`RightPost`, placas base) independientes; editar cualquiera despues no cambia a los demas.

- **`Members`**: derivado; lo reconstruye la ruta canonica (`Deserialize` → `RefreshPhysicalModel`). Nunca se copia.
- **`Exceptions`**: exactamente la politica de `DeepCopy` (se re-anexan clonadas en memoria; no se persisten).
- **No es un framework**: capturar y materializar; nada mas. El nombre exacto lo fija G3 y el ADR no depende de el.

| # | Invariante | Garantia |
|---|---|---|
| I1 | cada destino obtiene una instancia semanticamente independiente | un `Materialize()` por destino |
| I2 | ningun destino queda aliado al source | la copia privada se toma al capturar; la Source nunca se asigna |
| I3 | ningun destino queda aliado a otro | cada `Materialize()` devuelve un grafo nuevo |
| I4 | editar la Source despues no cambia los destinos | I2 |
| I5 | editar A no cambia la Source ni B | I1 + I3 |
| I6 | listas, arrays y todo estado authored mutable tambien son independientes | `DeepCopy` crea listas y objetos nuevos |

## 5. AM-3 final — `Members`, `Exceptions` y Push Back

- **Separacion verdadera, verificada.** L-2 afecta solo a overrides **por linea** fuera del editor de Push Back. El
  Dinamico cotiza resolviendo el diseno (`src/RackCad.Plugin/KindHandlers/DynamicKindHandler.cs:37-43`) y el resolver
  refresca las cabeceras de modulo (`DynamicRackSystemResolver.cs:201`, `:204`); I-53 no escribe overrides por linea.
- **`Members`** nunca es fuente authored ni se comparte. `Materialize` produce un grafo nuevo con derivado
  reconstruido.
  - Selectivo: tras cambiar la profundidad de la copia, `ImposeFondoDepth` refresca el derivado (HECHO
    `SelectiveCabeceraAuthority.cs:101-107`). `PostPeralte` y `Height` no alimentan el derivado.
  - Dinamico: el refresco lo hace el recompute existente (`Refresh`, `ApplyPostPeralte`).
- **`Exceptions`**: politica de `DeepCopy`, sin cambio de persistencia. Hecho que no cambia: en el Dinamico toda
  recomposicion pasa por `CloneHeader` (`DynamicRackSystemResolver.cs:194`, `:512-516`), que no transporta las
  `Exceptions` de runtime; lo caracteriza `Fact6` (`TC/PushBackModuleEditorCharacterizationTests.cs:251`, `:275`).
- **Promesa**: coherencia **en escritura** de lo que I-53 escribe. La coherencia historica en carga no se promete: la
  cubren los consumidores actuales y las guardas S-22 y D-24.
- **Push Back**: cero diff de produccion; **no** consume el contrato comun, que por eso puede ser mas estricto.
  Ninguna prueba de I-53 fija L-2 ni crea una golden de ese defecto.
- **Shared no contiene** `HeaderCoveragePolicy`, `StageInert` ni abstraccion equivalente.

## 6. Selectivo

### 6.1 Entradas de I-43 que no cambian

ADR-0032 (aceptado, inmutable): D1 (seleccion 2D proyectada y omision de posiciones inexistentes, `:55`), D4, D6
(commit atomico de pendientes), **D9** (cabecera por `(fondo, poste)`: `Height` de la receta, `Depth` del fondo;
omitir e informar, `:149`), **D10** (autoridades) y D12 (preferencia de destinos del editor). I-53 anade solo:
direccion de origen, eje de postes, Plan/Outcome, Snapshot y atomicidad estricta.

### 6.2 Source

| Operacion | Source | Elegibilidad | Captura |
|---|---|---|---|
| DISTRIBUTE | direccion `(fondo, poste)` recordada por la UI | `SelectiveCabeceraAuthority.UsableCustomAt(sistemaResuelto, fondo, poste)` no nula, leida tras la frontera C4 | en PREPARE, con `TryCapture` sobre ese valor ACTUAL |
| EDIT | `window.Configuration` (HECHO `RackSelectiveWindow.xaml.cs:1205`) | resultado no nulo y cambiado (la ventana ya compara `:1206`) | en PREPARE, sobre el resultado |

- Direccion que no designa una posicion vigente → `SourceNotFound`; posicion vigente con cabecera estandar o no usable
  → `SourceUnusable`.
- **Por que una direccion basta en el Selectivo**: los fondos crecen y se truncan **por el final** (HECHO
  `RackSelectiveWindow.xaml.cs:490-491`) y las filas de cabecera se podan por el final **sin resurreccion** (HECHO
  `SyncPostCabeceras`, `SelectiveEditorState.cs:739-762`). Una direccion recordada designa la misma posicion o deja
  de existir; una posicion que renace es estandar. **Condicion de parada de G4**: si aparece una operacion que
  desplace indices, la direccion del Selectivo pasa a llevar firma, como la del Dinamico.

### 6.3 Targets: `TargetFondos × PostTargets`

- **`TargetFondos`**: el contrato de I-43 **tal cual** (`SelectiveEditorState.cs:503-595`; `SelectiveTargetMode`).
- **`PostTargets`** (nuevo, estado propio, **no persistido y no recordado** entre sesiones; al abrir vale `Actual`):

| Modo | Significado |
|---|---|
| `Actual` | el poste seleccionado en el editor; sigue a la seleccion |
| `Explicito` | un conjunto explicito de postes **principales**, distinto y ascendente |
| `Todos` | todo poste principal de la reticula maestra `0..MaxFrenteCount()` (HECHO `SelectiveEditorState.cs:716-726`), re-expandido contra la reticula vigente |

- **Universo**: postes principales `0..MaxFrenteCount()`. Los postes de **medio frente** no son direccionables.
- **Tras un cambio estructural**, la misma gramatica que `TargetFondos` (`SyncTargetFondos`, `:576-581`): `Todos` se
  re-expande; un explicito se poda y, si queda vacio, cae al poste actual (como `SetTargetFondosCore`, `:551-559`,
  que nunca deja el conjunto vacio); `Actual` sigue.
- Sin clamp, sin crear, sin resucitar.

### 6.4 Destination set

```text
para cada fondo en TargetFondos (asc)
  para cada poste en PostTargets (asc)
    si fondo no existe o no PostExistsIn(fondo, poste)       → Omitted(AbsentInScope)
    si DISTRIBUTE y (fondo, poste) = direccion de origen     → Omitted(IsSource)
    si no                                                    → aplicable
indice de poste negativo                                     → Rejected(MalformedTarget)
```

### 6.5 HEIGHT — resuelta

Viaja con la receta; **vinculante por ADR-0032 D9/D10**. No se corrige en destino y **no** es decision del Owner.
PREPARE revisa la altura para cada `(fondo, poste)` aplicable (extension aditiva de `SelectiveCabeceraHeightReview`,
hoy por un poste: `:89-163`) y la consolida en `Warnings`: severa → CONFIRM; informativa → aviso sin confirmacion.
La semilla de altura de «Personalizar» (`:1191`) es EDIT anterior a I-53 y no cambia.

### 6.6 DEPTH

- La impone el fondo destino: `CabeceraDepthOfFondo(fondo)` (`SelectiveEditorState.cs:1158-1163`).
- Se aplica **sobre la copia** con la ruta canonica `SelectiveCabeceraAuthority.ImposeFondoDepth`, que tambien
  refresca el derivado.
- Si la profundidad no es valida (`<= 0`) → **`DestinationInvalid` → Rejected global**. Hoy `ImposeFondoDepth` ignora
  `<= 0` en silencio (HECHO `:103`): por eso la validacion ocurre antes, en el paso 5.
- Nunca se hereda el `Depth` de la Source.

### 6.7 PERALTE

**Autoridad unica: `SelectivePostGeometry.PostPeralteAt`** (HECHO `:45-49`: el valor por poste si `> 0`; si no, el
peralte del tramo). En PREPARE:

```text
copia.PostPeralte = peralte efectivo del poste destino TRAS la operacion, por la regla de PostPeralteAt
  DISTRIBUTE: la operacion no cambia PostPeraltes → PostPeralteAt(sistema, posteDestino)
  EDIT:       la operacion deja en PostPeraltes[visible] el escalar precomputado → la misma regla sobre ese escalar
```

- **Por que no basta la regla local de la ventana ni dejar el peralte del origen**. El sistema resuelto guarda en
  `PostPeraltes` el valor efectivo por poste (HECHO `A/Systems/Selective/SelectiveGeometryResolver.cs:90-91`) y la
  planta lo pasa como override (`PostPeralteAt`, HECHO `SelectivePlantaBuilder.cs:74`); `PlantaHeaderLayoutBuilder`
  solo cae al `PostPeralte` **propio de la configuracion** cuando ese override es `<= 0` (HECHO
  `A/RackFrames/PlantaHeaderLayoutBuilder.cs:68-73`). Por tanto:
  - la fuga es **visible** en planta cuando el peralte efectivo del poste destino es `<= 0` (peralte del tramo sin
    fijar): el destino dibujaria el peralte del **origen** en vez del de una cabecera de ese poste;
  - y es **siempre** una incoherencia en escritura: la copia persistiria en `PostCabeceras` un peralte authored que no
    es el de su poste, contra la promesa de §5 (y «Personalizar» siembra el peralte desde el poste, `:1197`, no desde la
    configuracion).
  Normalizar con `PostPeralteAt` cierra los dos casos (prueba S-16). Esta precision acota el enunciado de M-01 de la
  revision de V1, que presentaba la fuga visible como incondicional.
- La regla vive en **un solo sitio**: `PostPeralteAt` o una sobrecarga suya para evaluar el escalar de EDIT; nunca
  una re-escritura en la ventana ni en el preparador (AGENTS, convencion 2).
- **DISTRIBUTE no escribe `PostPeraltes`.**
- **EDIT** escribe `PostPeraltes[visible]` solo si ese poste queda en `Applied` en al menos un fondo. Si todo se omite,
  `Rejected(NoApplicableTargets)` y el peralte no se toca: cierra L-7 (hoy se escribe antes de aplicar, `:1252-1257`).

### 6.8 Frontera previa

`CommitPendingEditors` → `SaveWorkingToSelected` → PREPARE (§3.11). Hoy `ApplyCabeceraToTargets` llama
`SyncPostCabeceras` dentro de la escritura (HECHO `:1196`); en el contrato nuevo la sincronizacion es previa y PREPARE
no la invoca.

### 6.9 Aterrizaje en el estado y recompute

- **PREPARE**: operacion pura nueva de Application sobre el estado comprometido y el sistema resuelto.
- **MUTATE**: por cada destino de `Targets`, `fila[poste] = copia` (con el relleno de fila dispersa `EnsureCabeceraRow`
  **dentro** de un poste que el fondo tiene) y, en EDIT, el escalar de `PostPeraltes`. Metodo **aditivo** del estado.
- **RECOMPUTE**: uno, dentro del `DeferRecompute` que envuelve el gesto (G5).
- **E1 sin cambio visible**: `ApplyCabeceraToTargets` y `SelectiveCabeceraHeightReview.Of` quedan **intactos** y la
  ventana los sigue usando hasta G5. En G5 la ventana pasa a Plan/Outcome; G5 decide si `ApplyCabeceraToTargets` se
  retira o se reimplementa sobre el plan, sin cambiar el resultado observable de un destino valido salvo el cierre
  de L-7.

### 6.10 Lo que no cambia

`SelectivePalletDesign` (archivo caliente) no se modifica; `Scope` y celdas; `TargetFondos` y su preferencia;
«Restablecer poste»; frontal, lateral y planta por fondo; RACKEDITAR y Actualizar. Las copias caen en
`PostCabeceras`/`ExtraFondoPostCabeceras`, ya persistidos: **sin DTO ni campo nuevo**.

## 7. Dinamico

### 7.1 Unidad de destino — RESUELTA: `ModuleId`

`Destination = ModuleId`, **no** `(PostIndex, ModuleId)`. Razones: decision 1 del Owner en I-35 («la personalizacion es
por modulo longitudinal de rack, nunca por frente ni por poste», `I-35-editor-avanzado-push-back.md:51-53`); autoridad
authored por modulo; persistencia por modulo (`Modules[].Header`); planta y BOM por modulo; no introduce overrides por
linea y no hereda L-2. Ya no es pregunta del Owner.

### 7.2 AM-2 final — sin sesion y sin ejecutor generico

- **No** se adopta `RackModuleEditSession` ni se modifica. **No** se crea ejecutor generico en Shared.
- Confirmar/Cancelar escenificado no es contractual: lo contractual es la atomicidad por operacion y el CONFIRM
  opcional. El Dinamico sigue sin ambito sucio (ADR-0029 D8, `:127-130`; `TU/RichEditorCloseContractTests.cs:76-85`).

| Capa | Responsabilidad |
|---|---|
| `A/Systems/Shared` | Snapshot y tipos de Plan/Outcome/codigos (§3.13) |
| `A/Systems/Dynamic` (nuevo, G6) | `ModuleTargets` con firma y generacion; PREPARE puro; MUTATE; RECOMPUTE por el builder; operacion de reconstruccion con reconciliacion |
| Ventana del Dinamico (G7) | orquesta gestos; abre el configurador sobre copia por seam; recompone y redibuja una vez; muestra el informe; retira presets |
| Sin tocar | `RackModuleEditSession`, `RackModuleReconciliation`, `RackModuleDescriptor`, todo `PushBack*` |

### 7.3 `IsManualOverride` — semantica para I-53

```text
IsManualOverride                    = SOLO longitud manual
personalizacion de cabecera         = UseCalculatedHeaderConfiguration = false
DISTRIBUTE                          → no toca Length, IsManualOverride ni IsCalculated
EDIT                                → solo si el fondo editado difiere de Length (regla existente)
```

HECHO del estado actual, por ruta:

| Ruta | Hoy | En I-53 |
|---|---|---|
| `ApplyModule_Click` (`RackDynamicSystemWindow.xaml.cs:633-635`) | longitud editada → `IsManualOverride = true` | sin cambio (es longitud) |
| `EditHeader_Click` (`:703-710`) | fondo editado ≠ `Length` → `Length = fondo`, `IsManualOverride = true` | sin cambio; la regla pasa a la preparacion de EDIT en Application (G6) y la ventana la consume (G7) |
| Preset «Personalizada N» (`:1821-1824`) | copia **solo** la configuracion y fija `IsManualOverride = true` | **se elimina** con OD-8 (G7) |
| «Calculada» (`:1807-1812`) | regenera la configuracion y fija `IsManualOverride = false` | **sin cambio y caracterizada**: es un restablecimiento, no una reutilizacion; la doble semantica queda como hallazgo lateral N-02 (§17) |
| Reconciliacion (`RackModuleReconciliation.cs:164`, `:186-191`) y par ordinal (`DynamicEditorDesignAssembler.cs:71`) | leen el flag como longitud manual | coherente con la semantica fijada |

Guardas: D-12, D-14, D-15, D-26.

### 7.4 Source

- DISTRIBUTE: direccion `ModuleId` + firma de la peticion (§7.6), recordada por la UI.
- En PREPARE: la firma de la peticion debe coincidir (si no, `StaleTargets`); el `ModuleId` debe designar un modulo de
  cabecera (si no, `SourceNotFound`); debe ser personalizado (`UseCalculatedHeaderConfiguration = false` y
  configuracion no nula), fisicamente presente y capturable (si no, `SourceUnusable`).
- EDIT: `window.Configuration` sobre una copia (§7.12).

### 7.5 `ModuleTargets`

| Modo | Significado |
|---|---|
| `Actual` | el modulo de cabecera seleccionado. La seleccion se pierde en cada recomposicion (HECHO `:487`): sin seleccion → `NoTargets` |
| `Explicito` | conjunto de modulos de cabecera en orden longitudinal; se **invalida** en toda reconstruccion |
| `Todas` | todo modulo de cabecera de la secuencia vigente (`IsHeader`), re-expandido en cada resolucion |

No se persisten ni se recuerdan.

### 7.6 Firma, generacion e invalidacion (H-07)

- **Hecho**: los ids son posicionales. `BuildDefault` crea modulos sin id y `AssignIds` los numera `"M" + (i + 1)`
  (HECHO `DynamicRackSystemBuilder.cs:229-241`); una reconstruccion reutiliza los mismos ids para modulos nuevos.
- **Firma** = secuencia ordenada `ModuleId:Kind` + **generacion de reconstruccion**. La generacion la incrementa toda
  reconstruccion (tarima o fondos que exigen `MustRebuild`, y «Restaurar estandar») y la conserva el editor.
- **Reglas:**
  1. Toda reconstruccion **invalida** la direccion de origen y los destinos explicitos, y lo **informa**.
  2. PREPARE con una peticion de firma o generacion anterior → `Rejected(StaleTargets)`, **aunque los ids coincidan**.
  3. MUTATE con firma distinta de la del plan → `Rejected(StaleTargets)`, mutacion cero.
  4. **Nunca** se aplica al modulo que heredo el mismo id.
- Una recomposicion **sin** reconstruccion conserva ids (viajan por `resolver.Snapshot` → `Resolve`) y generacion: el
  origen sigue valido para aplicar otra vez. Un cambio de tipo sin reconstruccion cambia la firma y tambien invalida.

### 7.7 Destination set

```text
para cada modulo del universo de ModuleTargets, en orden de Index
  si Kind no es cabecera                         → Rejected(MalformedTarget)   (Explicito)
  si no IsPhysicallyPresent                      → Omitted(NotPhysicallyPresent)
  si DISTRIBUTE y ModuleId = origen              → Omitted(IsSource)
  si no                                          → aplicable
id vacio o desconocido con firma vigente         → Rejected(MalformedTarget)
Actual con un modulo seleccionado que no es cabecera → cuenta como sin seleccion → Rejected(NoTargets)
```

### 7.8 PREPARE, MUTATE y RECOMPUTE del Dinamico

```text
PREPARE (pura)
  verificar firma de la peticion → resolver ModuleId origen → TryCapture → resolver ModuleIds destino
  → validar IsHeader, Length > 0 (si no: DestinationInvalid), presencia fisica
  → Materialize una copia por destino aplicable
  → NO normalizar (no duplicar el Builder)
  → Plan (en EDIT, con los escalares de longitud manual precomputados por la regla existente)
MUTATE
  verificar firma → module.AssociatedFrameConfiguration = copia; module.UseCalculatedHeaderConfiguration = false
  → NO toca Length, IsManualOverride ni IsCalculated (salvo los escalares de EDIT) → sin transacciones de UI
RECOMPUTE (uno)
  DynamicRackSystemBuilder.ApplyPostPeralte(system, system.PostPeralte)  → peralte vigente del rack en cada cabecera + derivado
  DynamicRackSystemBuilder.Refresh(system)                               → Depth = Length + derivado + posiciones
```

HECHO de las rutas delegadas: `Refresh` impone `Depth = Length` y refresca el derivado de toda cabecera
(`DynamicRackSystemBuilder.cs:137-152`); `ApplyPostPeralte` impone el peralte del rack a toda cabecera, personalizada o
no, y refresca (`:179-194`). El peralte del Dinamico es **uniforme por rack** (la ventana lo fuerza en `:689-692` y
`:1827-1830`), asi que no existe la fuga por poste del Selectivo.

### 7.9 Reconstruccion y reconciliacion (OD-2.b APROBADA)

```text
reconstruir  = intenciones := DynamicRackSystemResolver.Snapshot(sistemaPrevio, ...).Modules
               sistema     := builder.BuildDefault(...)
               informe     := RackModuleReconciliation.Reconcile(intenciones, sistema, restaurados = ∅)
               generacion  := generacion + 1
«Restaurar estandar» (forceRebuild) = reconstruir SIN intenciones (reset; semantica intacta, Fact3)
sin reconstruccion                  = como hoy (UpdateHeaderHeightInPlace + ApplyPostPeralte)
```

- **Proyeccion de intenciones**: `DynamicRackSystemResolver.Snapshot` (HECHO `:291`, modulos en `:406-419`) es la
  autoridad de la intencion editable del Dinamico: la misma que `BuildDesign` persiste (`DynamicEditorDesignAssembler.cs:158`).
  No se crea un mapeador paralelo. Transporta la configuracion con `CloneHeader` (sin `Exceptions` de runtime, como
  la persistencia); `Reconcile` la copia con `DeepCopy` y cierra con `builder.Refresh` (HECHO
  `RackModuleReconciliation.cs:195`, `:209-210`).
- **Que conserva** (HECHO `RackModuleReconciliation.cs:125-213`): personalizada por `ModuleId + Kind`, **adaptada**
  en fondo y peralte; longitud manual de **cabeceras y separadores** (`:114-115`, `:164`, `:186-191`).
- **Que informa**: `RackModuleReconciliationResult` con `Preserved`, `Adapted` (subconjunto de `Preserved`),
  `Removed`, `Incompatible`, `Restored`, `Describe()` y `LostAnything` (HECHO `:16-66`). `restaurados = ∅` porque el
  Dinamico no escenifica restauraciones: «Calculada» restablece en el acto y la reconstruccion ya no ve nada que
  llevar.
- **Cambios de comportamiento aprobados por OD-2.b** (visibles en E3):
  - una personalizada sobrevive a un cambio de tarima o fondos si su `ModuleId + Kind` sigue existiendo;
  - las longitudes manuales se casan por id y tipo, y ya no por ordinal de cabecera;
  - la longitud manual de un **separador** tambien se conserva (hoy solo el fondo de las cabeceras,
    `DynamicEditorDesignAssembler.cs:61-116`);
  - lo perdido se informa (hoy se pierde en silencio).
- **E1 (G6)**: la operacion de Application con pruebas, **sin cablear**: la ventana sigue llamando al par ordinal
  (`RackDynamicSystemWindow.xaml.cs:446`, `:458`) y `Fact5` sigue verde y sin cambios.
- **E3 (G7)**: la ventana cambia a la reconciliacion y muestra `Describe()` en su estado. El par ordinal
  `SnapshotHeaderFondos`/`RestoreHeaderFondos` queda sin llamador de produccion (hoy su unico llamador es la ventana)
  y **se retira en G7**; sus pruebas (`TC/DynamicEditorDesignAssemblerTests.cs` y las dos aserciones del Dinamico de
  `Fact5`, `TC/PushBackModuleEditorCharacterizationTests.cs:186`, `:215`) se **reapuntan** al contrato de
  reconciliacion, nunca se relajan; las aserciones de Push Back no se tocan.

### 7.10 Overrides por linea

El Dinamico sigue sin escribirlos. Guardas: ninguna operacion de I-53 escribe `HeaderLineOverrides` ni
`DerivedPostLineOverrides`, y uno entrante sobrevive intacto a EDIT y DISTRIBUTE. **En una reconstruccion hoy se
pierden** (el sistema reconstruido nace sin ellos y `BuildDesign` los toma de el): se **caracteriza** ese comportamiento
historico y **no** se cambia (L-2 fuera; el Dinamico no se convierte a overrides por linea).

### 7.11 Retirada de presets «Personalizada N» (OD-8 APROBADA)

- **Contrato**: los presets no son un tercer mecanismo; ID6 los reemplaza; el origen es siempre una cabecera
  existente; **no existe portapapeles** persistente ni en memoria de configuraciones authored fuera de la direccion de
  origen.
- **Estado actual (HECHO)**: lista en memoria `headerPresets` (`:78`), clase `HeaderPreset` (`:1847-1857`), alta en
  `EditHeader_Click` (`:712-716`), entradas del desplegable (`:1765`) y rama de preset en
  `ConfigBox_SelectionChanged` (`:1813-1825`). No se persisten.
- **G7 elimina** esas cinco piezas. **Conserva** el restablecimiento por modulo a «Calculada» (no es un preset); la
  forma de su control es detalle de G7.
- **No se retira en G2 ni en E1.**
- Pruebas: D-27 (no aparece «Personalizada N» ni queda lista de configuraciones en la ventana) y D-26.

### 7.12 L-1 dentro de I-53 (E3 = G7)

Contrato:

1. El configurador se abre sobre una **copia** (`RackFrameProjectStore.DeepCopy`) de la configuracion del modulo,
   nunca sobre la instancia viva (hoy `:684` la pasa por referencia).
2. Se lee el resultado **real**: `window.Configuration` (HECHO `U/RackFrames/RackFrameConfiguratorWindow.xaml.cs:76`).
3. Si la cabecera **ya esta personalizada**, se abre en editor avanzado: `window.ViewModel.IsAdvancedEditor = true`
   (propiedad publica, `RackFrameConfiguratorViewModel.cs:176-191`; hoy arranca en falso, `:45`).
4. **No** se modifica `RackFrameConfiguratorWindow` (I-35, decision 5).
5. **Seam de presentacion** en la ventana del Dinamico, con la forma del precedente de Push Back:
   `Action<RackFrameConfiguratorWindow>` que sustituye solo el `ShowDialog` y deja la lectura del resultado en el
   camino de produccion (HECHO `RackPushBackSystemWindow.xaml.cs:2321-2358`).
6. El peralte del rack se aplica a la **copia resultado** antes de comparar (hoy se fuerza sobre la instancia viva,
   `:689-692`).

**Limite residual declarado**: si el usuario cambia a mano a «Configuracion rapida» y pulsa «Aplicar», esa accion
consciente reconstruye desde plantilla por comportamiento del configurador.

Pruebas RED **vistas fallar** antes del arreglo y confirmadas juntas en G7 (AGENTS, «Pruebas», punto 2): D-28 «Aplicar»
rapido, D-29 «Restaurar estandar», D-30 «Abrir proyecto» y D-31 reapertura avanzada sobre copia. `Fact7`
(`:424`, `:439-440`) se reapunta si su literal deja de existir.

### 7.13 Persistencia, BOM, dibujo, Actualizar y RACKEDITAR

Las copias caen en `Modules[].Header` + `UseCalculatedHeaderConfiguration`, ya persistidos. BOM (`SystemBomBuilder`),
lateral, frontal y planta ya leen la configuracion del modulo. **Sin DTO ni campo nuevo.**

## 8. Push Back: precedente y regresion

- Precedente: destinos cabeceras × lineas, copia canonica por par, validacion previa y el seam
  `HeaderConfiguratorPresenter`. Su comportamiento por pares sin cobertura se describe aqui en prosa: **no** se
  representa en Shared.
- Regresion: las suites de I-40 (V1 §8) verdes y **sin cambios**.
- **Cero diff de produccion** en `A/Systems/PushBack/**`, `U/Systems/PushBack/**`, `RackModuleEditSession.cs` y
  `RackModuleReconciliation.cs`, comprobado en cada gate (P-02).
- Unica excepcion declarada, y solo en E3: las aserciones **del Dinamico** de
  `TC/PushBackModuleEditorCharacterizationTests.cs` (`Fact5` × 2; `Fact7` si cambia su literal).
- L-2 no se corrige ni se fija con una prueba (P-03); se registra como follow-up al congelar G2.

## 9. OD-6 B′ — una iniciativa, tres integraciones

### 9.1 Entregas

| Entrega | Gates | Contenido | Cambio visible | Validacion |
|---|---|---|---|---|
| **E1 — Fundacion** | G3 + G4 + G6 | nucleo Shared provisional; Selectivo Application/estado; Dinamico Application/estado + reconciliacion **sin cablear** | **ninguno** | proporcional (§9.3) |
| **E2 — Selectivo UI** | G5 | ventana del Selectivo sobre Plan/Outcome; ID6/ID7; L-7 cerrado; N-01 si se acepta | si | Owner Validation Selectivo (AutoCAD 2025) |
| **E3 — Dinamico UI** | G7 | ventana del Dinamico; ID6/ID7; retirada de presets; L-1; reconciliacion cableada con informe | si | Owner Validation Dinamico (AutoCAD 2025) |

**Por que tres y no una**: con un Candidato unico, cada ronda de correccion del Dinamico produciria un SHA nuevo e
invalidaria la validacion del Selectivo (AGENTS, «Reutilizacion de evidencia»: solo SHA exacto). Con E2 integrada
**antes** de empezar G7, una correccion aislada del Dinamico no obliga a revalidar el Selectivo (M-08).

### 9.2 Invariantes de E1 — «sin cambio visible» demostrable

| Id | Invariante | Como se comprueba |
|---|---|---|
| E1-INV-1 | el diff de E1 no toca `src/RackCad.UI/**`, `src/RackCad.Plugin/**`, `assets/**`, `deploy/**`, `eng/**` ni `.github/**` | `git diff --name-only <base-E1>..<candidato-E1>` |
| E1-INV-2 | los tipos nuevos no tienen llamador de produccion: solo pruebas | busqueda de sus nombres en `src/` |
| E1-INV-3 | ninguna prueba existente se modifica; siguen verdes las suites de UI, las firmas de dibujo de I-24 y las suites de I-40 e I-43 | `git diff` sobre `tests/` + suites |
| E1-INV-4 | las modificaciones a archivos existentes de Application son **aditivas**: no cambian `ApplyCabeceraToTargets`, `SelectiveCabeceraHeightReview.Of`, `SnapshotHeaderFondos`/`RestoreHeaderFondos` ni ninguna ruta que use la ventana | diff + guardas |
| E1-INV-5 | Push Back: cero diff de produccion | P-02 |

### 9.3 Evidencia por entrega

- **Candidato de cada entrega** (AGENTS, «Pruebas», punto 1): Core Full local, **UI Full local**, build Debug de UI
  y de Plugin, CI de `push` verde sobre el SHA exacto. LC-UI no reduce ningun Candidato.
- **E1**: no cambia el comportamiento de dibujo, asi que no se activa la sesion en AutoCAD (AGENTS, punto 5;
  WORKFLOW §4.5.3). El contrato declara `requires_owner_validation: true`, que es **monotonico** (AUTOMATION_PLAN,
  «solo puede anadir»): E1 lo satisface con un **veredicto explicito proporcional** sobre la evidencia de E1
  (E1-INV + Candidato), sin sesion AutoCAD. Precedente de aceptacion sobre codigo: ADR-0024 (I-37A). Forma parte de PA-1
  (§10), que esta elevada.
- **E2 / E3**: Owner Validation en AutoCAD 2025 sobre el SHA exacto del Candidato, con los checklists de §13.8.
- Cada integracion: merge `--no-ff`, **CI posterior al merge** sobre el `MERGE_SHA` con cobertura y comprobacion
  diferida de la cobertura del Candidato (WORKFLOW §4.5 pasos 5-7). Nada se hereda entre entregas.

### 9.4 Aislamiento entre entregas

- El nucleo Shared queda **provisional** en G3 y se considera demostrado al pasar G4 **y** G6. Si G6 obliga a
  cambiar el nucleo, se repiten las suites relevantes de G3 y G4 antes del Candidato E1.
- Tras E1, el nucleo esta integrado. **Condicion de parada**: si G5 o G7 necesitan cambiar el nucleo Shared, o G7
  necesita tocar codigo del Selectivo, se detiene y vuelve al Coordinador; nunca se cambia «de paso».
- E2 se integra antes de abrir G7: con una sola rama, un commit de G7 posterior al Candidato E2 entraria en la
  integracion de E2.

### 9.5 Trazabilidad

Una iniciativa (I-53), un contrato, un registro de decisiones, un ADR, una fila de ROADMAP, una rama y una worktree.
Cada entrega deja: Candidato, CI del Candidato, veredicto de validacion, commit de cierre documental de entrega,
`MERGE_SHA` y su CI con cobertura. **Como encaja eso con WORKFLOW es PA-1** (§10).

## 10. Compatibilidad con WORKFLOW — PA-1 (ELEVADA, no aplicada)

### 10.1 Lo que WORKFLOW ya permite tal cual

- Varias sesiones secuenciales sobre la misma rama y worktree, con rebase al abrir (§3, §4.2).
- Una sesion de integracion completa por cada integracion: rebase final, Candidato, CI, validacion, commit
  documental, merge `--no-ff`, CI posterior y cobertura (§4.5 pasos 1-7).
- Tras un merge, la rama sigue siendo ancestro de `main`; continuar sobre ella es un avance rapido y no exige force.

### 10.2 Lo que WORKFLOW **no** permite literalmente

| Id | Regla literal | Conflicto con tres integraciones de una misma rama |
|---|---|---|
| G-W1 | §3 y §4.6: la rama «sobrevive hasta que el merge exista y pasen las comprobaciones posteriores al merge; **solo entonces se limpia**» | tras E1-I y E2-I la rama debe seguir viva |
| G-W2 | §4.5.4: «**Ultimo commit de la rama**: actualizar HANDOFF §8-12 y marcar la iniciativa en ROADMAP como `integrada (fecha)`» | el commit de cierre de E1 y E2 no es el ultimo de la rama y la iniciativa no esta integrada |
| G-W3 | §8, ultima fila: «en ROADMAP la marca de cierre es `integrada (fecha)`»; §2: ROADMAP solo en tres momentos | no existe una marca de entrega parcial |
| G-W4 | §5: checklist «de cierre de iniciativa» | se ejecutaria tres veces sobre una iniciativa no cerrada |
| G-W5 | AUTOMATION_PLAN: `requires_owner_validation: true` es monotonico | anade un gate de validacion del dueno tambien a E1, que no cambia el dibujo |
| G-W6 | §3: el borrado seguro usa `git branch -r --merged main` | entre entregas la rama aparece como mergeada y otra sesion podria limpiarla |

### 10.3 Opciones

| Opcion | Descripcion | Evaluacion |
|---|---|---|
| **P-A** (recomendada) | misma rama y worktree, tres sesiones de integracion, limpieza diferida a E3-I, marca de entrega en ROADMAP | conserva «1 iniciativa = 1 rama = 1 worktree = 1 fila» y las dos compuertas por integracion; solo necesita las excepciones acotadas de §10.4 |
| P-B | subiniciativas con rama propia, como I-39A..D | contradice OD-6 («sin I-53A/I-53B») y exige ramas nuevas |
| P-C | una sola integracion al final | contradice OD-6 B′ |

### 10.4 PA-1 — adaptacion procesal minima propuesta (P-A)

| Id | Excepcion acotada a I-53 | Regla que exceptua |
|---|---|---|
| PA-1.a | tras E1-I y E2-I **no** se limpian rama ni worktree, aunque pasen los pasos 5.6 y 5.7; la limpieza del §4.6 ocurre solo tras E3-I verificada | G-W1 |
| PA-1.b | el commit de cierre documental de E1 y E2 actualiza HANDOFF §8-12 con la entrega y marca la fila de ROADMAP como `entrega N/3 integrada (fecha)`; `integrada (fecha)` solo en E3-I. Es el momento 3 del §2 aplicado por entrega | G-W2, G-W3 |
| PA-1.c | «ultimo commit de la rama» se lee como «ultimo commit de la entrega»; la sesion siguiente rebasa sobre `main` (§4.2) y continua | G-W2 |
| PA-1.d | el checklist §5 se aplica por entrega sobre lo que esa entrega contiene; los puntos de cierre global (ADR, hallazgos laterales, decisiones) se cierran en E3-I | G-W4 |
| PA-1.e | el contrato y la fila declaran «rama viva por PA-1 hasta E3-I»; ninguna sesion la borra por aparecer mergeada | G-W6 |
| PA-1.f | E1 satisface `requires_owner_validation` con un veredicto explicito proporcional sobre su evidencia, sin sesion AutoCAD; E2 y E3 con validacion en AutoCAD | G-W5 |
| PA-1.g | nada mas cambia: sin ramas auxiliares, sin commits directos a `main`, evidencia por SHA exacto, dos compuertas por integracion, HANDOFF solo en sesiones de integracion, ROADMAP nunca «en curso» | — |

**Estado: ELEVADA, no aplicada.** La autoridad para autorizarla la determina el Coordinador con el Arquitecto. Si
concluyen que exceptuar WORKFLOW exige al Owner, o una enmienda de WORKFLOW («Cambia el proceso mismo», §8), se eleva.
La alternativa mayor —una enmienda general de WORKFLOW para «entregas multiples»— **no** se propone: cambiaria el
proceso de todas las iniciativas por un caso unico. Debe resolverse **antes del freeze de G2**, porque los gates de §12
dependen de ella. Registro previsto: contrato §12 y `docs/automation/decisions/I-53.md`.

## 11. I-50 y ramas paralelas

### 11.1 Regla exacta (OD-7 eliminada como decision del Owner: la fijan WORKFLOW §7 y el contrato)

Mientras `origin/feature/cotas-independientes-por-vista` exista sin integrarse en `main` con CI posterior verde,
**prohibido en I-53** tocar: `RackSelectiveWindow.xaml/.cs`, `RackDynamicSystemWindow.xaml/.cs`,
`SelectiveShellMigrationTests`, `DynamicShellMigrationTests`, `SelectiveEditorWindowTests`,
`DynamicEditorWindowTests`, y toda prueba nueva que instancie esas ventanas.

**Liberacion**, en este orden: (1) merge de I-50 en `main`; (2) CI posterior al merge verde; (3) verificacion de la
cobertura y de las compuertas de cierre de I-50; (4) rebase de I-53 sobre `main`; (5) solo entonces se editan
ventanas y pruebas calientes. **Sin `OWNER_OVERRIDE` como camino normal.** Si I-50 se integra mientras I-53 sigue en
G2, no se rebasa ni se abre UI: solo se actualiza el estado observado.

**Estado a las 20:38**: I-50 @ `6cd2970`, G3 (A) `ed50cbd` (Selectivo), G3 (B) `2765a45` (Dinamico) y G3 (C)
`6cd2970` (Push Back) completos; **sin integrar** → veda **ACTIVA** para E2 y E3.

### 11.2 Solape por archivo previsto

| Archivo previsto | Gate / entrega | I-50 @ `6cd2970` | Clase |
|---|---|---|---|
| `A/Systems/Shared/HeaderConfigurationSnapshot.cs` y tipos de Plan/Outcome (nuevos) | G3 / E1 | — | ninguno |
| `A/Systems/Selective/` tipos nuevos (`PostTargets`, preparacion) | G4 / E1 | — | ninguno |
| `A/Systems/Selective/SelectiveEditorState.cs` (metodo aditivo de MUTATE) | G4 / E1 | +1 linea `:1318` (`DimensionViews`) | **textual**, region distinta (`:1136-1235`) |
| `A/Systems/Selective/SelectiveCabeceraHeightReview.cs` (extension aditiva) | G4 / E1 | — | ninguno |
| `A/Systems/Selective/SelectivePostGeometry.cs` (sobrecarga, solo si hace falta) | G4 / E1 | — | ninguno |
| `A/Systems/Dynamic/` tipos nuevos (targets, preparacion, reconstruccion) | G6 / E1 | — | ninguno |
| `A/Systems/Dynamic/DynamicEditorDesignAssembler.cs` | **no se toca en E1**; G7 retira el par | +1 linea `:175` | textual, en E3 |
| `A/Systems/Dynamic/DynamicRackSystemResolver.cs` | solo lectura (`Snapshot`) | +2 lineas `:245`, `:359` | ninguno |
| `U/Systems/Selective/RackSelectiveWindow.xaml/.cs`, `TU/SelectiveShellMigrationTests.cs`, `TU/SelectiveEditorWindowTests.cs` | G5 / E2 | G3 (A): XAML +11/−1, `.cs` +64 (hunks `:17`, `:263`, `:1704-1763`, `:2401`, `:2750`), censo y firmas | **MATERIAL ACTIVO → VEDADO** |
| `U/Systems/Dynamic/RackDynamicSystemWindow.xaml/.cs`, `TU/DynamicShellMigrationTests.cs`, `TU/DynamicEditorWindowTests.cs` | G7 / E3 | G3 (B): XAML +11, `.cs` +70 (hunks `:17`, `:166`, `:300-369`, `:2844`), censo y firmas | **MATERIAL ACTIVO → VEDADO** |
| `TC/PushBackModuleEditorCharacterizationTests.cs` (aserciones del Dinamico), `TC/DynamicEditorDesignAssemblerTests.cs` | G7 / E3 | — | ninguno |

Si I-50 se integra antes del Candidato E1, E1 rebasa como cualquier sesion; si E1 se integra antes, el rebase de
I-50 resuelve lineas en regiones distintas.

### 11.3 Otras ramas

- **I-49** @ `2783acc` (docs): su V5 preve un miembro de `RackSelectiveWindow.xaml.cs` en su G10. Con G5 de I-53 es un
  archivo caliente compartido: un solo agente a la vez (WORKFLOW §7). No activo; se verifica en el preflight de G5.
- **I-52** @ `0fc7032` (docs): RACKMIRROR refleja disenos con datos de cabecera por indice; I-53 **no anade datos
  persistidos**, asi que no le cambia nada. Comparte el indice de ADR (`docs/adr/README.md`).
- **I-54** @ `36c337b` (docs): sin archivos de I-53.
- **Indice de ADR**: I-50 (0035), I-52 (0036) e I-53 (0037) anaden filas contiguas en `docs/adr/README.md`; es un
  conflicto textual trivial de integracion.

## 12. Gates V2

Ninguno se abre en esta ejecucion.

| Gate | Entrega | Contenido | RED / verde | Evidencia de cierre | Condicion |
|---|---|---|---|---|---|
| G2A | — | Proposal V1 + paquete de Arquitecto | — | `f38362d`, CI | hecho |
| **G2B** | — | Proposal V2, registro de decisiones, ADR-0037 `propuesto`, contrato | — | este commit + CI de `push` | esta ejecucion |
| G2-R | — | re-revision acotada (Anexo A) | — | `ARCHITECT_V2` | — |
| G2-F | — | freeze: contrato vinculante reescrito; decisiones (freeze); `ideas-futuras.md` (L-2, laterales, N-02, N-03); PA-1 resuelta; N-01 clasificado; ADR-0037 con el estado que decida el Owner | — | docs + CI | `ARCHITECT_V2 = AGREED`; PA-1 resuelta |
| **G3** | E1 | nucleo Shared **provisional**: Snapshot, Plan, Outcome, codigos, fixtures | RED por asercion → verde; guardas C-04, C-07..C-10 | Core local; CI | G2-F; ADR-0037 redactado |
| **G4** | E1 | Selectivo Application: `PostTargets`, PREPARE, MUTATE aditivo, revision multi-poste, peralte, firma | RED → verde; verdes I-43 | Core local; CI | G3 |
| **G6** | E1 | Dinamico Application: `ModuleTargets`, firma y generacion, PREPARE/MUTATE/RECOMPUTE, reconstruccion con reconciliacion **sin cablear** | RED → verde; `Fact3`/`Fact5`/`Fact7` intactos | Core local; CI | G3; si cambia el nucleo, repetir G3 + G4 |
| **E1-C** | E1 | Candidato E1 (SHA exacto rebasado) + E1-INV | — | Core Full + UI Full locales, Debug UI + Plugin, CI `push` 4/4 | G4 + G6 |
| **E1-V** | E1 | validacion proporcional | — | veredicto explicito sobre la evidencia de E1-C | E1-C; PA-1 |
| **E1-I** | E1 | integracion 1 (WORKFLOW §4.5 con PA-1) | — | `MERGE_SHA` CI 4/4 + cobertura; cobertura diferida del Candidato | E1-V |
| [I-50] | — | liberacion (§11.1) + rebase de I-53 | — | — | externo |
| **G5** | E2 | Selectivo UI; censo actualizado, **no relajado** | RED UI → verde | Core + UI (LC-UI); CI | E1-I; veda liberada |
| **E2-C** | E2 | Candidato E2 | — | como E1-C | G5 |
| **E2-V** | E2 | Owner Validation Selectivo (AutoCAD 2025) | — | veredicto del Owner sobre E2-C | E2-C |
| **E2-I** | E2 | integracion 2 | — | `MERGE_SHA` CI 4/4 + cobertura | E2-V |
| **G7** | E3 | Dinamico UI: ID6/ID7, retirada de presets, L-1, reconciliacion cableada e informe, retirada del par ordinal; `Fact5` reapuntados; censo actualizado | RED UI (L-1 vistos fallar) → verde | Core + UI; CI | E2-I; veda liberada |
| **E3-C** | E3 | Candidato E3 | — | como E1-C | G7 |
| **E3-V** | E3 | Owner Validation Dinamico (AutoCAD 2025) | — | veredicto del Owner sobre E3-C | E3-C |
| **E3-I** | E3 | integracion 3 + cierre global (HANDOFF, ROADMAP `integrada`, limpieza) | — | `MERGE_SHA` CI 4/4 + cobertura; limpieza tras 5.6 y 5.7 | E3-V |

## 13. Matriz de pruebas V2 (no se implementa en G2)

### 13.1 Reglas

- Toda corrida filtrada demuestra que selecciono pruebas: **0 seleccionadas = FALLO**, con su conteo registrado.
- **RED siempre por asercion de comportamiento sobre codigo compilable**: andamiaje con la firma y sin
  comportamiento, luego la prueba ve fallar la asercion (precedente I-51 G3). Un error de compilacion **nunca** es un
  RED contractual.
- Clases: **RED** (capacidad nueva vista fallar), **guarda** (propiedad que ya se cumple y debe seguir), **verde**
  (existente sin cambios), **comprobacion de gate** (no es prueba: `git diff`, busqueda).
- Todo dialogo pasa por seam; ninguna prueba abre un modal real (leccion del cuelgue headless de I-48).

### 13.2 Nucleo comun (G3)

| Id | Prueba | Clase |
|---|---|---|
| C-01 | `TryCapture(null)` → `Failed(NullSource)`; `TryCapture(no usable)` → `Failed(UnusableHeader)`; sin lanzar, sin copiar y sin mutar el origen | RED |
| C-02 | (V1 C-02 + C-03) cada `Materialize()` devuelve un grafo nuevo: ningun objeto ni lista compartido con el origen ni entre copias; `Members` reconstruido; `Exceptions` segun `DeepCopy` | RED |
| C-03 | mutar el origen tras capturar, o una copia materializada, no cambia el Snapshot ni otras copias (I4-I6) | RED |
| C-04 | el Snapshot no expone la copia privada, colecciones ni setters | guarda |
| C-05 | Plan: `Rejected` sin Targets; `Prepared` con Targets, Omitted, Warnings y firma; no hay `Applied` en el Plan | RED |
| C-06 | Outcome: `Committed(Applied = Targets, mismo orden)`; `Cancelled` ≠ `Rejected`; ninguno de los dos lleva Applied | RED |
| C-07 | conjunto cerrado: tres motivos Omitted y siete codigos Rejected | guarda |
| C-08 | (V1 C-06) neutralidad: los tipos nuevos no referencian WPF, `Autodesk.*`, `ObjectId`, `Database`, `Transaction` ni `BlockReference` | **guarda** |
| C-09 | (V1 C-07) el nucleo no usa `CloneHeader` ni `ToConfiguration` sin refresco, y no crea serializacion ni DTO | guarda |
| C-10 | Shared no contiene politica de cobertura, `StageInert` ni ejecutor generico | guarda |

### 13.3 Selectivo (G4 Application, G5 UI)

| Id | Caso | Clase | Gate |
|---|---|---|---|
| S-01 | origen elegible solo si `UsableCustomAt` no es nula; estandar → `SourceUnusable` | RED | G4 |
| S-02 | aplicar a uno (`Actual` × fondo actual) | RED | G4 |
| S-03 | aplicar a varios (postes explicitos × fondos explicitos) | RED | G4 |
| S-04 | aplicar a todos (`Todos` × `Todos`) con fondos de distinta longitud | RED | G4 |
| S-05 | la direccion origen se omite (`IsSource`) en su posicion del orden | RED | G4 |
| S-06 | **origen en vivo**: recordar la direccion, editar despues esa cabecera y aplicar → los destinos reciben el valor ACTUAL | RED | G4 |
| S-07 | **origen desaparecido** (su poste deja de existir en su fondo, o su fondo desaparece) → `SourceNotFound`, mutacion cero | RED | G4 |
| S-08 | independencia: editar el origen tras aplicar no cambia destinos (I4); editar un destino no cambia origen ni otros (I5, I6) | RED | G4 |
| S-09 | **fondo inexistente** → `Omitted(AbsentInScope)`, nunca `Rejected` | RED | G4 |
| S-10 | (V1 S-10 + S-12) **poste ausente** en un fondo → `Omitted(AbsentInScope)`; nunca creado ni clampeado; sin resurreccion tras reducir y volver a crecer | RED multi-poste; verdes existentes `ATargetWhereThePostDoesNotExist_…`, `ShrinkingAFondo_…`, `Scenario4_…` | G4 |
| S-11 | todos omitidos → `Rejected(NoApplicableTargets)`; `PostPeraltes` intacto (cierra L-7) | RED | G4 |
| S-12 | **fallo en el ultimo destino**: 1..N−1 validos y el N con profundidad `<= 0` → `Rejected(DestinationInvalid)`, mutacion CERO en todos (estado serializado identico) | RED | G4 |
| S-13 | **pureza de PREPARE**: estado, diseno y sistema serializados identicos antes y despues; ningun recompute; `EffectiveCustomAt`, `SyncPostCabeceras` e `ImposeFondoDepth` nunca actuan sobre instancias guardadas | RED | G4 |
| S-14 | altura: viaja con la receta; revision por cada `(fondo, poste)` consolidada en Warnings; severa → CONFIRM; no confirmar → `Cancelled`, mutacion cero | RED | G4 |
| S-15 | profundidad: cada copia recibe la de su fondo por `ImposeFondoDepth` sobre la copia; nunca la del origen | RED | G4 |
| S-16 | **fuga de peralte**: origen con override explicito y destino que hereda (override 0) → la copia persiste `PostPeralteAt` del destino (coherencia en escritura); con el peralte del tramo sin fijar, la planta del destino dibuja lo mismo que una cabecera de ese poste y no el peralte del origen | RED | G4 |
| S-17 | DISTRIBUTE no escribe `PostPeraltes`; EDIT escribe `PostPeraltes[visible]` solo si queda Applied | RED | G4 |
| S-18 | `PostTargets`: `Todos` se re-expande; explicito se poda y cae al actual si queda vacio; `Actual` sigue; **medio frente fuera del universo**; no se persiste ni se recuerda | RED | G4 |
| S-19 | **orden determinista** `(FondoIndex, PostIndex)` de Targets, Applied, Omitted y Warnings | RED | G4 |
| S-20 | **precedencia determinista**: una peticion con varios defectos devuelve el primer codigo de §3.7 | RED | G4 |
| S-21 | firma del plan: topologia cambiada entre PREPARE y MUTATE → `Rejected(StaleTargets)`, mutacion cero | RED | G4 |
| S-22 | BOM del editor = BOM del diseno recargado por la ruta de `RACKBOMTOTAL` (store → registro → resolver → builder), con personalizadas distribuidas | guarda | G4 |
| S-23 | RACKEDITAR: reabrir con personalizadas distribuidas en varios fondos (`LoadExisting`) | RED (hoy sin fijar) | G4 |
| S-24 | guardar/reabrir: `RoundTrip_PreservesDifferentCustomsInFondos0And1And3` + caso distribuido | verde + RED | G4 |
| S-25 | legacy: documentos sin personalizadas y «Personalizar» de un poste dan el mismo resultado, **excluido el escenario de L-7** (cambia por diseno; S-11) | verdes I-43 | G4 |
| S-26 | regresion I-43: `TargetFondos × Scope` de celdas intacto | verdes | G4 |
| S-27 | **frontera C4 anterior al lote**: con pendientes invalidos el gesto aborta sin plan ni mutacion ID6/ID7; con pendientes validos su recompute se cuenta aparte | RED (UI) | G5 |
| S-28 | un recompute por operacion confirmada (`RecomputeCount`, `RackSelectiveWindow.xaml.cs:2188`); cero en Rejected y Cancelled | RED (UI) | G5 |
| S-29 | (V1 S-18 + S-20) Actualizar y dibujo lateral, planta y frontal por fondo con copias distribuidas | verdes + E2E nueva | G5 |
| S-30 | dialogos de confirmacion e informe por seam, sin modal real | guarda (UI) | G5 |
| S-31 | [solo si N-01 se acepta] «Personalizar» de una personalizada abre el configurador en editor avanzado | RED (UI) | G5 |

### 13.4 Dinamico (G6 Application, G7 UI)

| Id | Caso | Clase | Gate |
|---|---|---|---|
| D-01 | origen elegible: personalizado, usable y fisicamente presente; calculado o no presente → `SourceUnusable` | RED | G6 |
| D-02 | aplicar a uno | RED | G6 |
| D-03 | aplicar a varios | RED | G6 |
| D-04 | aplicar a todas | RED | G6 |
| D-05 | el modulo origen se omite (`IsSource`) | RED | G6 |
| D-06 | **origen en vivo**: direccion recordada, origen editado despues, aplicar → valor ACTUAL | RED | G6 |
| D-07 | **origen desaparecido** (el id ya no designa una cabecera con firma vigente) → `SourceNotFound` | RED | G6 |
| D-08 | independencia origen/destino y destino/destino (I4-I6) | RED | G6 |
| D-09 | separador o id desconocido con firma vigente → `Rejected(MalformedTarget)` | RED | G6 |
| D-10 | modulo sin presencia fisica por frentes en blanco → `Omitted(NotPhysicallyPresent)` | RED | G6 |
| D-11 | **fallo en el ultimo destino** (`Length <= 0`) → `Rejected(DestinationInvalid)`, mutacion CERO en todos | RED | G6 |
| D-12 | DISTRIBUTE no crea modulos ni cambia tipos, y su MUTATE no toca `Length`, `IsManualOverride` ni `IsCalculated` | RED | G6 |
| D-13 | **pureza de PREPARE**: sistema serializado identico; sin `Refresh`, `ApplyPostPeralte` ni `Reconcile` sobre el sistema vivo; cero recomputes | RED | G6 |
| D-14 | **`IsManualOverride`**: DISTRIBUTE y despues cambio de tarima → la longitud calculada del destino sigue al layout y su personalizada se conserva **adaptada** e informada | RED | G6 |
| D-15 | EDIT con fondo editado ≠ `Length` → longitud manual (regla existente); sin cambio de fondo, `IsManualOverride` intacto | RED | G6 |
| D-16 | **stale**: una reconstruccion (tarima, fondos o «Restaurar estandar») invalida la direccion de origen y los destinos explicitos, y lo informa | RED | G6 |
| D-17 | **stale**: peticion con generacion anterior → `Rejected(StaleTargets)` aunque los ids coincidan; ningun apply al modulo que heredo el mismo id | RED | G6 |
| D-18 | **stale**: firma distinta entre PREPARE y MUTATE → `Rejected(StaleTargets)`, mutacion cero | RED | G6 |
| D-19 | RECOMPUTE unico tras `Committed`: `ApplyPostPeralte` + `Refresh` imponen `Depth = Length` y el peralte del rack en cada copia; PREPARE no normaliza | RED | G6 |
| D-20 | reconstruccion con reconciliacion: intenciones por `DynamicRackSystemResolver.Snapshot`; personalizadas y longitudes manuales (cabeceras y separadores) por `ModuleId + Kind`; informe Preserved/Adapted/Removed/Incompatible/Restored | RED | G6 |
| D-21 | cambio de tipo y reconstruccion → `Incompatible` informado (hoy: perdida silenciosa) | RED | G6 |
| D-22 | «Restaurar estandar» = reset sin intenciones (`Fact3` intacto) | verde | G6 |
| D-23 | (antes D-17) overrides por linea: I-53 nunca los escribe; uno entrante sobrevive a EDIT y DISTRIBUTE; en reconstruccion hoy se pierden: se caracteriza sin cambiar | guarda + caracterizacion | G6 |
| D-24 | BOM del editor = BOM por la ruta equivalente a `RACKBOMTOTAL` (store → registro → resolver → `SystemBomBuilder`), con `Members` presentes | guarda | G6 |
| D-25 | orden determinista por `Index` y precedencia determinista de Rejected | RED | G6 |
| D-26 | `IsManualOverride`: ninguna ruta de reutilizacion lo fija; desaparece la ruta del preset; «Calculada» conserva su comportamiento caracterizado | RED + guarda | G7 |
| D-27 | **retirada de presets**: tras «Editar cabecera» no aparece «Personalizada N»; la ventana no guarda configuraciones authored fuera de la direccion de origen; unico mecanismo = ID6 + ID7 | RED (UI) + guarda | G7 |
| D-28 | **L-1**: «Aplicar» de configuracion rapida sobre una calculada se lee de `window.Configuration` | RED visto fallar | G7 |
| D-29 | **L-1**: «Restaurar estandar» del configurador se lee | RED visto fallar | G7 |
| D-30 | **L-1**: «Abrir proyecto» del configurador se lee | RED visto fallar | G7 |
| D-31 | **L-1**: una personalizada se reabre en editor avanzado, sobre una copia y nunca sobre la instancia viva | RED visto fallar | G7 |
| D-32 | un recompute por operacion confirmada; cero en Rejected y Cancelled (seam de conteo) | RED (UI) | G7 |
| D-33 | **informe de reconciliacion visible y transportable** (`Describe`, `LostAnything`) cuando algo se adapta o se pierde | RED (UI) | G7 |
| D-34 | configurador e informe por seam, sin modal real | guarda (UI) | G7 |
| D-35 | RACKEDITAR con personalizadas distribuidas: procedencia y configuraciones identicas | RED | G7 |
| D-36 | (V1 D-20 + D-22) Actualizar y dibujo lateral, frontal y planta con personalizada de modulo | verdes + E2E | G7 |
| D-37 | guardar/reabrir `Modules[].Header` (`RackProjectStoreTests`) + caso distribuido | verde + RED | G7 |
| D-38 | legacy: sin personalizadas, firma de dibujo de I-24 identica | verde | G7 |
| D-39 | `Fact5` (× 2) y `DynamicEditorDesignAssemblerTests` reapuntados a la reconciliacion sin relajarse; `Fact7` si cambia su literal | guarda reapuntada | G7 |

### 13.5 Push Back e I-43

| Id | Caso | Clase |
|---|---|---|
| P-01 | suites de I-40 (V1 §8) verdes y sin cambios | verdes |
| P-02 | cero diff de produccion de Push Back | **comprobacion de gate** |
| P-03 | ninguna prueba de I-53 fija L-2 ni crea una golden de ese defecto | **comprobacion de gate** |
| I-01 | suite de I-43 verde y sin cambios, salvo el escenario de L-7 excluido en S-25 | verdes |

### 13.6 Exigencias del Coordinador → pruebas

| # | Exigencia | Pruebas |
|---|---|---|
| 1 | fallo en ultimo destino, cero mutacion parcial | S-12, D-11 |
| 2 | pureza de PREPARE | S-13, D-13 |
| 3 | Selectivo: fondo o poste inexistente → Omitted | S-09, S-10 |
| 4 | source en vivo | S-06, D-06 |
| 5 | source desaparecida → `SourceNotFound` | S-07, D-07 |
| 6 | stale del Dinamico sin reorientar | D-16, D-17, D-18 |
| 7 | `IsManualOverride` | D-12, D-14, D-15, D-26 |
| 8 | retirada de presets | D-27 |
| 9 | L-1 | D-28..D-31 |
| 10 | fuga de peralte del Selectivo | S-16 |
| 11 | orden determinista | S-19, D-25 |
| 12 | precedencia determinista | S-20, D-25 |
| 13 | frontera C4 anterior al lote | S-27 |
| 14 | informe de reconciliacion visible y transportable | D-20, D-33 |
| 15 | dialogos por seam, headless-safe | S-30, D-34 |
| 16 | BOM editor = ruta `RACKBOMTOTAL` en el Dinamico | D-24 (y S-22) |
| 17 | Push Back: I-40, cero diff, sin canonizar L-2 | P-01, P-02, P-03 |
| 18 | I-43: guardas verdes intactas | I-01, S-26 |

### 13.7 Correcciones de clasificacion aplicadas

C-06 de V1 → guarda (C-08). P-03 → comprobacion de gate. S-22 de V1 → S-25 excluye L-7. Redundancias fusionadas:
S-10/S-12 → S-10; S-18/S-20 → S-29; D-20/D-22 → D-36; C-02/C-03 → C-02.

### 13.8 Validaciones por entrega

- **E1-V (proporcional)**: E1-INV-1..5 comprobadas; Candidato completo; lista de tipos nuevos y su ausencia de
  llamadores de produccion; conteos de suites. Sin AutoCAD.
- **E2-V (Selectivo, AutoCAD 2025)**: personalizar; tomar como origen; aplicar a uno, a varios y a todos; origen editado
  despues de tomarlo (se aplica el actual); omisiones por poste ausente y por fondo inexistente; altura severa
  (confirmar y cancelar); destino con peralte heredado desde un origen con peralte propio (con y sin peralte del
  tramo); independencia; Actualizar; RACKEDITAR; guardar y reabrir; vistas
  por fondo; BOM del editor frente a `RACKBOMTOTAL`; N-01 si se acepta.
- **E3-V (Dinamico, AutoCAD 2025)**: L-1 con «Aplicar» rapido y reapertura avanzada; ausencia de «Personalizada N»;
  tomar como origen; aplicar a uno, a varias y a todas; omision por frentes en blanco; independencia; cambio de tarima
  con informe (personalizada adaptada, longitud manual de cabecera y de separador); invalidacion informada de origen y
  destinos tras reconstruir; «Restaurar estandar»; «Calculada»; Actualizar; RACKEDITAR; guardar y reabrir; vistas; BOM
  del editor frente a `RACKBOMTOTAL`.

## 14. Archivos previstos

| Gate | Crear | Modificar | Solo leer |
|---|---|---|---|
| G2B (esta) | `docs/initiatives/I-53-proposal-v2.md`, `docs/automation/decisions/I-53.md`, `docs/adr/0037-reutilizacion-de-cabecera-por-copia-y-distribucion-por-lotes.md` | contrato I-53 (minimo), `docs/adr/README.md` (fila del indice) | todo lo citado |
| G2-F | — | contrato; `decisions/I-53.md`; `docs/ideas-futuras.md` | — |
| G3 | `A/Systems/Shared/HeaderConfigurationSnapshot.cs` y tipos de Plan/Outcome/codigos (nombres en G3); pruebas nuevas en `TC/` | — | `RackFrameProjectStore.cs`, `RackFrameConfiguration.cs`, `RackDesignValidation.cs` |
| G4 | tipos nuevos del Selectivo (`PostTargets`, preparacion, firma); pruebas nuevas en `TC/` | `SelectiveEditorState.cs` (aditivo), `SelectiveCabeceraHeightReview.cs` (aditivo), `SelectivePostGeometry.cs` (sobrecarga, solo si hace falta) | `SelectiveCabeceraAuthority.cs`, `SelectiveFondoTargets.cs`, `SelectiveTargetResolver.cs`, `PlantaHeaderLayoutBuilder.cs` |
| G6 | tipos nuevos del Dinamico (targets, firma, preparacion, reconstruccion); pruebas nuevas en `TC/` | — | `RackModuleReconciliation.cs`, `RackModuleDescriptor.cs`, `DynamicRackSystemBuilder.cs`, `DynamicRackSystemResolver.cs`, `DynamicEditorDesignAssembler.cs` |
| G5 | pruebas nuevas en `TU/` | `RackSelectiveWindow.xaml/.cs`, `TU/SelectiveShellMigrationTests.cs`, `TU/SelectiveEditorWindowTests.cs` si cambian firmas | — |
| G7 | pruebas nuevas en `TU/` | `RackDynamicSystemWindow.xaml/.cs`, `TU/DynamicShellMigrationTests.cs`, `TU/DynamicEditorWindowTests.cs` si cambian firmas; `DynamicEditorDesignAssembler.cs` (retirada del par); `TC/DynamicEditorDesignAssemblerTests.cs`; aserciones del Dinamico en `TC/PushBackModuleEditorCharacterizationTests.cs` | `RackFrameConfiguratorWindow.xaml.cs`, `RackFrameConfiguratorViewModel.cs` |
| E1-I, E2-I | — | `docs/HANDOFF.md`, `docs/ROADMAP.md` (PA-1.b), contrato | — |
| E3-I | — | `docs/HANDOFF.md`, `docs/ROADMAP.md` (`integrada`), guias si aplica, contrato | — |

**No se tocan en ningun gate:** `A/Systems/PushBack/**`, `U/Systems/PushBack/**`, `RackModuleEditSession.cs`,
`RackModuleReconciliation.cs`, `RackFrameProjectStore.cs`, `RackFrameConfiguratorWindow.xaml(.cs)`,
`RackFrameConfiguratorViewModel.cs`, los DTO y disenos de Selectivo y Dinamico (`SelectivePalletDesign.cs` es
caliente), Plugin, `assets/`, `deploy/`, `.github/`.

## 15. ADR-0037 — `propuesto`

- **Archivo**: [`docs/adr/0037-reutilizacion-de-cabecera-por-copia-y-distribucion-por-lotes.md`](../adr/0037-reutilizacion-de-cabecera-por-copia-y-distribucion-por-lotes.md),
  estado **`propuesto`**, con fila en el indice. **No aceptado**: solo el Owner acepta o rechaza (`adr/README.md`).
- **Por que ahora**: el Arquitecto lo pidio `propuesto` antes de G3; los agentes pueden redactar ADR `propuesto`; y
  WORKFLOW §8 lo exige **antes de implementar**. Precedentes: I-50 (ADR-0035) e I-52 (ADR-0036) lo redactaron en G2.
- **Numeracion**: 0037 es el primer numero libre en `origin/main` y en todas las ramas remotas vivas a las 20:38
  (0035 = I-50, 0036 = I-52; I-49 e I-54 sin numero; ninguna rama cita ADR-0037). Protocolo de colision del
  precedente I-52: se comprueba en cada preflight; mientras sea `propuesto` puede renumerarse si otra rama integra antes
  con el mismo numero.
- **Alcance incluido**: reutilizar = copia, nunca vinculo vivo; origen y destinos separados; taxonomia de destino por
  sistema, sin destino universal; materializacion canonica; PREPARE todo-o-nada; Omitted frente a Rejected tipados;
  Outcome `Rejected`/`Cancelled`/`Committed`; un recompute por operacion confirmada; autoridad de normalizacion por
  sistema; `IsManualOverride` = longitud manual en el Dinamico; Push Back conserva I-40 y no se migra.
- **Excluido**: XAML y controles; nombre concreto del tipo Snapshot; reglas ya cubiertas por ADR-0032; «Personalizada
  N»; lista de controles.
- **Momento de aceptacion**: lo decide el Owner. Se solicita en el freeze de G2 (precedente I-50); G3 exige que exista.

## 16. Decisiones — tabla final

| Id | Tema | Estado | Autoridad |
|---|---|---|---|
| OD-1 | Texto de ID6/ID7 | **RESUELTA** | autorizacion original del Owner |
| OD-2 | Unidad de destino del Dinamico | **RESUELTA: `ModuleId`** | I-35 decision 1 + arquitectura (§7.1) |
| OD-2.b | Reconciliacion en la reconstruccion del Dinamico | **OWNER APPROVED** | Owner (§1.3) |
| OD-3.a | `PostTargets` del Selectivo | **TECNICA**: `Actual`/`Explicito`/`Todos`, sin recordar | gramatica de I-43 (§6.3) |
| OD-3.b | ¿DISTRIBUTE escribe `PostPeraltes`? | **TECNICA**: no | autoridad por poste `PostPeralteAt` (§6.7) |
| OD-3.c | Altura / semilla de «Personalizar» | **CUBIERTA** por ADR-0032 D9/D10; la semilla no cambia | ADR aceptado (§6.5) |
| OD-4 | Cobertura del Dinamico | **TECNICA**: omitir e informar | ID7 «destinos compatibles», ADR-0032 D1, I-35 decision 3 |
| OD-5 | L-1 / L-2 | **RESUELTA**: L-1 dentro (con companera); L-2 fuera | Arquitecto + Coordinador |
| OD-6 | Particion | **OWNER APPROVED: B′** | Owner (§1.3); mecanismo = PA-1 elevada |
| OD-7 | Serializacion con I-50 | **RESUELTA** por WORKFLOW §7 + contrato; eliminada como decision del Owner | Coordinador (§11.1) |
| OD-8 | Presets «Personalizada N» | **OWNER APPROVED: retirar** en G7 | Owner (§1.3) |

**`requires_owner_decision`**: todas las decisiones de producto conocidas estan resueltas o aprobadas, pero V2 eleva
dos elementos **nuevos** (PA-1 procesal y N-01; §17). Conforme a la orden («si aparece una decision material nueva, no
asumir»), el contrato **conserva `true`** hasta que el Coordinador los clasifique.

## 17. Elementos nuevos elevados al Coordinador (no se asumen)

| Id | Elemento | Por que es nuevo | Recomendacion | Quien decide |
|---|---|---|---|---|
| **PA-1** | adaptacion procesal para tres integraciones de una rama (§10.4) | WORKFLOW no la permite literalmente (G-W1..G-W6) | P-A con PA-1.a..g, antes del freeze de G2 | Coordinador + Arquitecto; Owner si exige exceptuar o enmendar WORKFLOW |
| **N-01** | el Selectivo tiene el mismo riesgo que H-01: «Personalizar» abre una personalizada en «Configuracion rapida» (`RackSelectiveWindow.xaml.cs:1202`, sin `IsAdvancedEditor`), y un «Aplicar» rapido regenera la receta desde plantilla antes de aplicarla a `TargetFondos` | detectado en G2B al verificar la companera de L-1; hoy ya afecta a EDIT del Selectivo y afectaria al origen de DISTRIBUTE | incluirlo en G5 con el mismo contrato que el Dinamico (seam, `IsAdvancedEditor = yaPersonalizada`, sin tocar el configurador; S-31) | Coordinador + Arquitecto; Owner si se considera cambio de UX de producto. Alternativa: hallazgo lateral en `ideas-futuras.md` |
| N-02 | «Calculada» del Dinamico regenera la configuracion **y** borra la longitud manual (`:1810`): la misma sobrecarga de H-04 en sentido inverso | fuera de ID6/ID7 (restablecimiento, no reutilizacion) | caracterizar sin cambio (D-26) y registrar como hallazgo lateral en el freeze | registro, sin decision |
| N-03 | el desplegable de configuracion del Dinamico muestra siempre «Calculada» al seleccionar un modulo, aunque sea personalizado (`UpdateSelectedPanel` → `SelectConfigCalculated`, `:1754`, `:1775-1791`) | lectura enganosa, no reutilizacion | registrar como hallazgo lateral; G7 decide la forma del control al retirar los presets | registro, sin decision |

## 18. Riesgos

| Id | Riesgo | Mitigacion |
|---|---|---|
| R-01 | I-50 retrasa E2 y E3 | E1 no depende de I-50; veda y liberacion de §11.1 |
| R-02 | rebase de ventanas tras integrar I-50 | no abrir G5/G7 antes; un rebase por entrega |
| R-03 | la reconciliacion cambia la reconstruccion del Dinamico | OD-2.b aprobada; D-14, D-20, D-21; E3-V |
| R-04 | con otro numero de modulos una personalizada queda `Removed` o `Incompatible` | se informa; nunca silencioso |
| R-05 | PA-1 no se autoriza | sin PA-1, B′ no tiene mecanismo: el Coordinador decide la alternativa antes del freeze |
| R-06 | la rama aparece mergeada entre entregas y alguien la limpia | PA-1.e |
| R-07 | codigo de Application durmiente entre E1 y E3 | E1-INV-2 lo declara; E3 lo cablea; los tipos se documentan como contrato de I-53 |
| R-08 | con una sola rama, G7 espera a E2-I (y a su validacion) | coste aceptado por no revalidar el Selectivo (M-08) |
| R-09 | L-2 sigue en Push Back y alguien compara BOMs | follow-up registrado en el freeze de G2 |
| R-10 | censos y guardas de I-35 tentados a relajarse | se reapuntan en el gate del cambio, con justificacion (D-39) |
| R-11 | colision de numero de ADR | protocolo de colision de §15 |

## 19. Lo que V2 no decide y queda fuera

Nombres definitivos de tipos (G3); disposicion de controles WPF (G5, G7); reset por lotes; aplicar entre racks del
dibujo; unidad por instancia en el Dinamico; cualquier cambio de Push Back; L-2; migracion de documentos;
`SchemaVersion`; catalogos; Plugin; la forma del control «Calculada» tras retirar los presets (G7).

---

## Anexo A — Paquete autonomo y acotado de re-revision (Arquitecto)

> **No es una revision completa.** Revisa solo lo listado, contra la revision de V1 ya emitida. Se puede leer sin el
> resto del repositorio. Respuesta pedida: **`ARCHITECT_V2 = AGREED`**, **`AGREED WITH CHANGES`** (con los cambios)
> o **`NOT AGREED`** (con el motivo).

### A.0 Contexto minimo

- **Iniciativa**: I-53, rama `feature/cabeceras-configurables-multidestino`; base `46fcac2`; V1 `f38362d`; V2 = el
  commit que publica este documento.
- **Objetivo**: en Selectivo y Dinamico, tomar una cabecera existente como origen (ID6, copia sin vinculo vivo) y
  aplicarla a un conjunto de destinos compatibles (ID7), con copias independientes y aplicacion atomica.
- **Revision de V1**: `AGREED WITH CHANGES`, AM-1..AM-4 `AGREED WITH CHANGES`, `ADR_REQUIRED = YES`, sin BLOCKER.
- **Owner**: OD-2.b, OD-6 (B′) y OD-8 **aprobadas**; no se reabren.
- **Restriccion externa**: I-50 @ `6cd2970` sin integrar; veda activa sobre las dos ventanas y sus pruebas.

### A.1 Revisar: H-01..H-07

Tabla completa en §2.1. Puntos concretos a verificar:

| Hallazgo | Que mirar | Seccion |
|---|---|---|
| H-01 | ¿basta copia + `window.Configuration` + `IsAdvancedEditor = yaPersonalizada` por seam, con el limite residual declarado? | §7.12 |
| H-02 | ¿`AbsentInScope` para fondo inexistente y poste ausente queda alineado con I-43 y ADR-0032? | §3.5, §6.4 |
| H-03 | ¿Plan y Outcome quedan separados, con `Applied` solo en `Committed` y `Rejected(StaleTargets)` como resultado de MUTATE? | §3.4, §3.9 |
| H-04 | ¿es correcta la semantica de `IsManualOverride` y el tratamiento de «Calculada» como caracterizacion (N-02)? | §7.3 |
| H-05 | ¿la retirada de presets conserva el restablecimiento «Calculada» sin dejar otro mecanismo de reutilizacion? | §7.11 |
| H-06 | ¿direccion + captura al aplicar cierra el portapapeles oculto en ambos sistemas? ¿basta una direccion sin firma en el Selectivo, dada la truncacion por el final? | §3.3, §6.2 |
| H-07 | ¿es suficiente firma = `ModuleId:Kind` + generacion? ¿es correcto colocar `StaleTargets` primero en la precedencia (paso 0 de PREPARE, anadido a la cadena del Coordinador)? | §3.7, §7.6 |

### A.2 Revisar: M-01..M-05 y M-09

| Hallazgo | Que mirar | Seccion |
|---|---|---|
| M-01 | peralte de la copia = efectivo del poste destino **tras** la operacion, por `PostPeralteAt`; EDIT con escalar precomputado; prueba de fuga; ¿es correcta la **precision** de V2 (fuga visible solo con peralte efectivo `<= 0`, incoherencia en escritura siempre)? | §6.7, S-16 |
| M-02 | PREPARE del Dinamico sin normalizar; RECOMPUTE = `ApplyPostPeralte` + `Refresh` | §7.8 |
| M-03 | frontera C4 externa al lote; sin codigo `Rejected`; prueba de aislamiento | §3.11, S-27 |
| M-04 | lista de lecturas permitidas y ayudantes prohibidos; firma minima por sistema; vida del plan | §3.8, §3.9 |
| M-05 | `TryCapture` con `NullSource`/`UnusableHeader`, validacion previa y sin catch-all | §4 |
| M-09 | matriz: 18 exigencias mapeadas; redundancias y clasificaciones | §13 |

### A.3 Revisar: incorporacion de las decisiones del Owner

| Decision | Que mirar | Seccion |
|---|---|---|
| OD-2.b | reconstruccion con `resolver.Snapshot` + `Reconcile`; informe visible en E3; sin cablear en E1; retirada del par ordinal en G7 con pruebas reapuntadas; conservacion de la longitud manual de separadores declarada como parte de la aprobacion | §7.9 |
| OD-6 B′ | E1 sin cambio visible demostrable (E1-INV-1..5); validacion proporcional de E1; aislamiento entre entregas; condicion de parada si G5/G7 tocan el nucleo | §9 |
| OD-8 | presets retirados en G7; sin portapapeles; «Calculada» conservado | §7.11 |

### A.4 Revisar: ADR scope

¿El alcance de ADR-0037 (§15) es el correcto, sin XAML, sin nombre de clase, sin reglas de ADR-0032 y sin presets?
¿Falta o sobra alguna decision duradera?

### A.5 Revisar: compatibilidad de las tres integraciones con WORKFLOW

¿Es correcta la lectura literal (G-W1..G-W6)? ¿Es PA-1 (P-A con PA-1.a..g) la adaptacion **minima**? ¿Quien debe
autorizarla (Coordinador/Arquitecto u Owner)? ¿Debe resolverse antes del freeze de G2?

### A.6 Revisar: gates V2

¿Orden y condiciones de §12: nucleo provisional hasta G4 + G6; E1-C/E1-V/E1-I; espera de I-50; E2 integrada antes de
G7; E3 con cierre global?

### A.7 Clasificar: N-01

¿La reapertura avanzada del Selectivo entra en G5 (S-31) o se registra como hallazgo lateral? ¿Requiere al Owner?

### A.8 Veredicto pedido

```text
ARCHITECT_V2 = AGREED | AGREED WITH CHANGES | NOT AGREED
  H-01..H-07        = cerrados | con cambios (cuales)
  M-01..M-05, M-09  = cerrados | con cambios (cuales)
  OD-2.b / OD-6 / OD-8 incorporadas = si | con cambios
  ADR-0037 alcance  = correcto | con cambios
  PA-1              = minima y correcta | con cambios; autoridad = ...
  Gates V2          = correctos | con cambios
  N-01              = en G5 | lateral | Owner
```
