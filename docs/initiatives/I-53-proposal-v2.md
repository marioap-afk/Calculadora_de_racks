# I-53 — Proposal V2 (congelada en G2-F): ID6 REUSE + ID7 BATCH DISTRIBUTION de la configuracion de cabecera

> **Contrato congelado (G2-F).** Esta Proposal V2 **supera** a la [Proposal V1](I-53-proposal-v1.md), que se conserva
> intacta como historial de la primera ronda. Es el **contrato tecnico vinculante** de la linea I-53: la re-revision
> del Arquitecto la dejo en `ARCHITECT_V2 = AGREED`, el Owner aprobo OD-2.b, OD-6 y OD-8 y acepto ADR-0037, y este
> freeze incorpora PA-1B, N-01 y RR-01. **El freeze no implementa nada y no abre G3**: la implementacion empieza en G3,
> en una sesion posterior. Todo cambio material del contrato durante la implementacion vuelve al Coordinador y, si es
> arquitectonico, al Arquitecto.
>
> Entradas: [Discovery G1](I-53-discovery.md), [contrato](I-53-cabeceras-configurables-multidestino.md),
> [registro de decisiones](../automation/decisions/I-53.md) y
> [ADR-0037](../adr/0037-reutilizacion-de-cabecera-por-copia-y-distribucion-por-lotes.md) (aceptado).

```text
Initiative     = I-53
Owner IDs      = ID6 (REUSE) + ID7 (BATCH DISTRIBUTION)          OD-1 = RESUELTA (autorizacion original)
Branch         = feature/cabeceras-configurables-multidestino
BASE_SHA       = 46fcac2b071929d2bd5b07aa28373941417f74a8   (base del reclamo)
REBASE_BASE    = f8deb675c6d1ef0e64693b157d69c4cc170d7b24   (Merge I-50; rebase de G2-F)
CLAIM_SHA      = e1d5996e70cb75589beb96a256714982fe693be3 → f317ea947daa1c0fdff1cc8fcc13ef4b5ebc1258 tras el rebase
Claim-Id       = d7144fe8-6921-4a46-9d66-2d723620bea4
G1_SHA         = c8476cccc98809fdb7fd0aeccef288521126f442 → df3ac1c0bb52696ff7bf97e36190d567c1881d15
PROPOSAL_V1    = f38362d32a737f62d69a229854dc5c1812adf063 → 686df3eafd0ba1397c3612e4f36f3b671a1a90cc   (superada)
PROPOSAL_V2    = d7f17addb47ea943b61ff50b4b4a5b23010d6fab → d0db698facf2297d6bd5aaa512f3b3b3f2358a59   (revisada en d7f17ad)
Proposal       = V2 CONGELADA (G2-F)
Architect V1   = AGREED WITH CHANGES
Architect V2   = AGREED (G2C)
Owner          = OD-2.b APPROVED · OD-6 APPROVED (B′) · OD-8 APPROVED · ADR-0037 ACCEPTED («Acepto el ADR»)
ADR            = ADR-0037 aceptado
PA-1           = PA-1B: unidades Git I-53 (E1) / I-53S (E2) / I-53D (E3)
N-01           = A (G5 de I-53S; prueba S-31)
RR-01          = incorporado (§3.3, §3.8, §3.9, §3.11, §6.9; prueba S-32)
requires_owner_decision = false
G2             = FROZEN
G3             = NOT OPENED   (proximo gate: nucleo compartido de la fundacion)
```

## 0. Estado del repositorio y citas

### 0.1 Preflight de G2-F (2026-09-12, 22:24 -06:00)

Tras `git fetch --all --prune`:

| Punto | Observado |
|---|---|
| Rama I-53 antes del rebase | HEAD = upstream = `d7f17ad`, `0/0`, arbol limpio; sin stash ni `MERGE_HEAD`, `REBASE_HEAD`, `CHERRY_PICK_HEAD`, `REVERT_HEAD`, `BISECT_LOG`, `rebase-*` o `sequencer` |
| `origin/main` | `f8deb67` — Merge I-50 (21:37) |
| I-50 | **integrada**; rama remota retirada; CI posterior al merge run `34736306222` (`push`) **success**, 4/4 jobs, artifact `rackcad-coverage-cobertura` presente; cobertura diferida run `34736321850` success |
| Ramas paralelas | I-49 @ `048a508` (Proposal V6), I-52 @ `0445718` (Proposal V2 + ADR-0036 `propuesto`), I-54 @ `5d25da8` (Proposal V3); las tres solo docs |
| Worktrees | cinco: principal (`main` @ `f8deb67`), I-49, I-52, I-53, I-54 |
| Rebase | I-53 iba 21 detras y 5 delante; rebase sobre `f8deb67` con un unico conflicto, documental, en `docs/adr/README.md` (filas 0035 y 0037: se conservan ambas). `git range-diff`: reclamo, G1 y V1 identicos; bootstrap y V2 solo cambian de contexto. El diff frente a `main` sigue solo en `docs/` y el blob de V1 (`7f5b99a`) esta intacto |

| Commit | Publicado | Tras el rebase |
|---|---|---|
| Reclamo (vacio) | `e1d5996` | `f317ea9` |
| Bootstrap | `405cfc0` | `667f1b7` |
| G1 Discovery | `c8476cc` | `df3ac1c` |
| G2A Proposal V1 | `f38362d` | `686df3e` |
| G2B Proposal V2 | `d7f17ad` | `d0db698` |

La evidencia de CI de los SHAs publicados **no** se transfiere a los rebasados (AGENTS, «Reutilizacion de evidencia»);
el commit de freeze tiene su propia corrida.

### 0.2 Citas de codigo

Las citas `archivo:linea` de esta Proposal se **re-verificaron en G2-F contra `f8deb67`**. El Discovery y la Proposal
V1 citan `46fcac2`. I-50 desplazo lineas en `RackSelectiveWindow.xaml.cs`, `RackDynamicSystemWindow.xaml.cs`,
`RackPushBackSystemWindow.xaml.cs`, `DynamicRackSystemResolver.cs` y `SelectiveGeometryResolver.cs`; en
`SelectiveEditorState.cs` y `DynamicEditorDesignAssembler.cs` solo anadio una linea despues de las regiones citadas.

Clases de evidencia: **HECHO** (codigo leido y linea verificada), **DOC**, **INFERENCIA**. Rutas abreviadas: `A/` =
`src/RackCad.Application/`, `D/` = `src/RackCad.Domain/`, `U/` = `src/RackCad.UI/`, `TC/` = `tests/RackCad.Tests/`,
`TU/` = `tests/RackCad.UI.Tests/`.

### 0.3 Historial de G2

- **G2A** publico la Proposal V1 con `main` en `46fcac2`.
- **G2B** publico esta V2 en `d7f17ad`, con `main` en `46fcac2` e I-50 sin integrar: la veda sobre las ventanas estaba
  activa y el esquema de tres integraciones quedo elevado como PA-1.
- **G2C**: re-revision acotada del Arquitecto sobre `d7f17ad`, con I-50 en su cierre documental (`c9a5f0a`) y aun sin
  merge → `ARCHITECT_V2 = AGREED`.
- **G2-F**: rebase sobre el merge de I-50 y este freeze.

## 1. Entradas vinculantes (no se reabren)

### 1.1 Clasificacion de G1

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

### 1.3 Decisiones del Owner

| Id | Decision | Donde aterriza |
|---|---|---|
| **OD-2.b** | APROBADA. En el Dinamico, al reconstruir por cambios de tarima o fondos: conservar cabeceras personalizadas por identidad de modulo cuando sea posible y longitudes manuales cuando corresponda, **usando la reconciliacion**, adaptando lo compatible e informando Preserved / Adapted / Removed / Incompatible / Restored. Nada se pierde en silencio | §7.9 (G6 de I-53 sin cablear; G7 de I-53D cableado e informe) |
| **OD-6** | APROBADA — **B′**. Una iniciativa conceptual de ID6 + ID7 con un contrato comun, entregada en **tres integraciones**: E1 fundacion sin cambio visible (G3 + G4 + G6), E2 UI del Selectivo (G5), E3 UI del Dinamico (G7). Cada una con su Candidato, suites, CI, integracion y validacion proporcional. Sin mecanismos conceptuales distintos | §9, §10, §12 |
| **OD-8** | APROBADA. Retirar los presets «Personalizada N» del Dinamico; la reutilizacion queda unificada bajo ID6 + ID7. La retirada ocurre en el gate de UI del Dinamico | §7.11 (G7 de I-53D) |
| **ADR-0037** | **ACEPTADO** por el Owner: «Acepto el ADR», referido a ADR-0037 de I-53 | §15 |

### 1.4 Veredictos del Arquitecto

| Revision | Veredicto | Contenido |
|---|---|---|
| Proposal V1 (`f38362d`) | **AGREED WITH CHANGES** | AM-1..AM-4 AGREED WITH CHANGES; H-01..H-07, M-01..M-11, L-01..L-06; `ADR_REQUIRED = YES`; L-1 dentro con su companera; L-2 fuera; ningun BLOCKER |
| Proposal V2 (`d7f17ad`, G2C) | **AGREED** | H-01..H-07 y M-01..M-05, M-09 CLOSED; AM-1..AM-4 AGREED; **PA-1 = PA-1B** (`OWNER_REQUIRED_FOR_PA1 = NO`); **N-01 = A**; hallazgo nuevo **RR-01** (MEDIUM); ADR-0037 AGREED WITH REQUIRED CHANGES (tres correcciones, aplicadas en G2-F); E1 integrable con E1-V = veredicto del Owner + smoke corto en AutoCAD; separacion E2/E3 correcta |

> **Nota de transparencia.** Las dos revisiones de Arquitecto las ejecuto **el mismo agente** que redacto el
> Discovery y las Proposals, en el rol asignado en cada sesion; se declara como hicieron I-47, I-51 e I-52. En G2C busco
> defectos en su propio texto: de ahi salen RR-01 y las correcciones del ADR.

### 1.5 Reglas vigentes tras el freeze

- G2-F es solo documental: **no** se abre G3 en la misma ejecucion.
- Push Back: cero diff de produccion; ninguna prueba canoniza L-2.
- Esta rama no toca ventanas, XAML, configurador, Plugin ni pruebas existentes: G5 y G7 son I-53S e I-53D (§9).
- Todo cambio material del contrato compartido durante G3, G4 o G6 vuelve al Coordinador y, si es arquitectonico, al
  Arquitecto.

## 2. Cierre de las revisiones del Arquitecto

### 2.1 HIGH — hallazgo → correccion → seccion → prueba (todos CLOSED en G2C)

| Hallazgo | Correccion | Seccion | Prueba (§13) |
|---|---|---|---|
| **H-01** — arreglar L-1 sin reabrir las personalizadas en editor avanzado destruye la receta en silencio (el configurador arranca en «Configuracion rapida» y su «Aplicar» regenera desde plantilla) | L-1 en I-53D: configurador sobre una **copia**, lectura de `window.Configuration` y una cabecera **ya personalizada** abierta con `IsAdvancedEditor = true` desde un seam de presentacion, sin tocar `RackFrameConfiguratorWindow` (I-35, decision 5); limite residual declarado. La misma regla se aplica al Selectivo (N-01 = A, §6.11) | §7.12, §6.11 | D-28..D-31, S-31 |
| **H-02** — V1 §4.2 rechazaba fondos o postes inexistentes, contra I-43 | Fondo inexistente y poste ausente en un fondo → **`Omitted(AbsentInScope)`**, nunca `Rejected`, como `SelectiveTargetResolver.cs:50-57`, `SelectiveTargetPlan.cs:62-63` y ADR-0032 (`:55`, `:149`) | §3.5, §6.4 | S-09, S-10 |
| **H-03** — el Report mezclaba estado de la operacion y resultado por destino | **Plan** (`Rejected` / `Prepared`) separado del **Outcome** (`Rejected` / `Cancelled` / `Committed`); `Applied` solo existe en `Committed` | §3.4 | C-05, C-06 |
| **H-04** — `IsManualOverride` sobrecargado; el preset ya lo fija al copiar | `IsManualOverride` = **solo longitud manual**; la personalizacion se expresa con `UseCalculatedHeaderConfiguration = false`; DISTRIBUTE no toca `Length` ni `IsManualOverride`; EDIT solo por la regla existente de fondo editado; la ruta del preset desaparece en G7; «Calculada» se caracteriza sin cambio | §7.3 | D-12, D-14, D-15, D-26 |
| **H-05** — presets «Personalizada N» = segundo mecanismo de reutilizacion | OD-8: se retiran en G7; el unico mecanismo es ID6 + ID7 y el origen es siempre una cabecera existente | §7.11 | D-27 |
| **H-06** — capturar al «Tomar como origen» creaba un portapapeles invisible | El origen es una **direccion**, capturada **al aplicar** con su valor ACTUAL; si la direccion ya no designa una cabecera → `SourceNotFound` | §3.3, §6.2, §7.4 | S-06, S-07, D-06, D-07 |
| **H-07** — `ModuleId` posicionales: un destino podia sobrevivir a una reconstruccion y reorientarse | Firma = secuencia `ModuleId:Kind` + **generacion de reconstruccion**; toda reconstruccion invalida origen y destinos explicitos (informado); peticion con firma distinta → `Rejected(StaleTargets)`; plan con firma distinta en MUTATE → fail closed | §7.6 | D-16, D-17, D-18 |

### 2.2 MEDIUM de la re-revision — M-01..M-05 y M-09 (todos CLOSED en G2C)

| Hallazgo | Cierre | Seccion |
|---|---|---|
| **M-01** — la normalizacion de peralte citaba la regla en linea de la ventana | Autoridad unica **`SelectivePostGeometry.PostPeralteAt`** (`A/Systems/Selective/SelectivePostGeometry.cs:45-49`): la copia lleva el peralte efectivo del poste destino **tras** la operacion. La fuga es **visible** en planta solo con peralte efectivo `<= 0` (`PlantaHeaderLayoutBuilder.cs:68-73`); la incoherencia en escritura ocurre siempre | §6.7 |
| **M-02** — normalizar en PREPARE del Dinamico duplicaria el constructor | PREPARE solo verifica precondiciones (`IsHeader`, `Length > 0`, presencia fisica); `Depth` y peralte los impone el RECOMPUTE canonico (`DynamicRackSystemBuilder.ApplyPostPeralte` `:179-194` + `Refresh` `:137-152`) | §7.8 |
| **M-03** — la frontera C4 aparecia como `Rejected` del lote | La frontera previa (`CommitPendingEditors` → `SaveWorkingToSelected`) es **anterior y externa** al lote y ocurre **fuera** de su ambito diferido (RR-01); si falla, el gesto termina sin plan ni Outcome | §3.11, §6.8 |
| **M-04** — pureza de PREPARE subespecificada | Estado observable definido; lecturas permitidas y ayudantes prohibidos sobre instancias guardadas; vida del plan = un gesto, con firma. RR-01 anade la frescura de la resolucion leida | §3.8, §3.9 |
| **M-05** — `TryCapture` sin fallos definidos | `Failed(NullSource)` / `Failed(UnusableHeader)`, validacion **antes** de copiar con el mismo predicado con que `Deserialize` lanzaria (`RackDesignValidation.cs:20-22`, `RackFrameProjectStore.cs:62-65`) y **sin catch-all** | §4 |
| **M-09** — brechas materiales de la matriz | Matriz con las 18 exigencias del Coordinador mapeadas; S-31 (N-01) y S-32 (RR-01) anadidas en G2-F | §13 |

### 2.3 Resto de MEDIUM y LOW

| Hallazgo | Tratamiento | Seccion |
|---|---|---|
| M-06 `HeaderCoveragePolicy`/`StageInert` en Shared | **Eliminado**: Shared no representa Push Back | §3.13, §5 |
| M-07 la tabla de OD pedia al Owner decisiones tecnicas | Tabla final cerrada | §16 |
| M-08 la particion ignoraba el acoplamiento de revalidacion por SHA exacto | Motivo de las tres unidades: una ronda del Dinamico no revalida el Selectivo | §9.6 |
| M-10 ADR pendiente | ADR-0037 aceptado | §15 |
| M-11 foto de paralelas desactualizada | Foto de G2-F | §0, §11 |
| L-01 RED de compilacion; C-06 y P-03 mal clasificadas | RED por asercion con andamiaje compilable; C-06 → guarda (C-08); P-03 → comprobacion de gate | §13.1 |
| L-02 redundancias S-10/S-12, S-18/S-20, D-20/D-22, C-02/C-03 | Fusionadas en S-10, S-29, D-36 y C-02 | §13 |
| L-03 redaccion de D-17 | D-23: una reconstruccion **hoy** pierde los overrides por linea; se caracteriza sin cambiar | §7.10, §13 |
| L-04 S-22 frente a L-7 | S-25 excluye el escenario de L-7 | §13 |
| L-05 retirada del par ordinal sin especificar | Se retira en G7 cuando la ventana deja de llamarlo; sus pruebas se reapuntan sin relajarse | §7.9 |
| L-06 proyeccion de intenciones sin fijar | `DynamicRackSystemResolver.Snapshot(...).Modules` (`:292`, `:408-421`), sin mapeador paralelo | §7.9 |
| Exclusion de postes de medio frente sin prueba | Cubierta en S-18 | §6.3, §13 |

### 2.4 Re-revision G2C incorporada en G2-F

| Decision de G2C | Incorporacion | Seccion |
|---|---|---|
| **PA-1 = PA-1B** | tres unidades Git (I-53, I-53S, I-53D), cada una con ciclo WORKFLOW completo; P-A descartada | §9, §10, §12, §14 |
| **N-01 = A** | reapertura avanzada en el Selectivo, en G5 de I-53S, con seam y S-31 obligatoria | §6.11, §13.3 |
| **RR-01** | precondicion de frescura de PREPARE, C4 fuera del ambito diferido del lote, firma con la generacion del sistema resuelto | §3.3, §3.8, §3.9, §3.11, §6.9, S-32 |
| **ADR-0037** | tres correcciones y aceptacion del Owner | §15 |
| **E1-V** | veredicto del Owner + smoke corto en AutoCAD 2025 (metadata monotonica) | §9.4, §13.8 |
| **Follow-ups** | L-2..L-6, L-8..L-14, N-02 y N-03 en `ideas-futuras.md`; XML-doc de `IsManualOverride` en G6 | §17 |

### 2.5 Que cambio respecto de V1

- DISTRIBUTE ya no admite «resultado del configurador tomado como origen»: el origen es siempre una cabecera
  existente, capturada al aplicar (H-05, H-06).
- El Report desaparece: Plan + Outcome, con codigos cerrados (H-03, H-02).
- `Rejected` por inexistencia pasa a `Omitted(AbsentInScope)`; la frontera C4 sale del lote (H-02, M-03).
- Firma (peticion y plan), generacion de reconstruccion en el Dinamico y frescura de la resolucion leida (H-07, M-04,
  RR-01).
- Semantica de `IsManualOverride` fijada; presets retirados en G7 (H-04, H-05, OD-8).
- Normalizacion del Dinamico delegada al recompute canonico (M-02); peralte del Selectivo por `PostPeralteAt` (M-01).
- L-1 exige reapertura avanzada (H-01), y el Selectivo aplica la misma regla (N-01).
- Una iniciativa conceptual en tres unidades Git (OD-6 B′, PA-1B).
- ADR-0037 aceptado; `HeaderCoveragePolicy`/`StageInert` fuera de Shared (M-06).

## 3. Contrato comun (AM-4)

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
| **Firma** | valor inmutable, comparado por igualdad, que resume la topologia, la resolucion y las entradas de normalizacion que PREPARE leyo (§3.9) |
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
frontera previa de edicion                (externa al lote y fuera de su ambito diferido; si falla, el gesto termina: §3.11)
precondiciones de PREPARE (RR-01)         (resolucion vigente: sistema resuelto no nulo, sin recompute pendiente ni
                                           diferido; si no se cumplen, el gesto termina antes de PREPARE, sin Outcome)
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
RECOMPUTE  uno, posterior a MUTATE, dentro del unico ambito diferido del lote
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

- **`Applied` solo existe en `Committed`**, y es exactamente la lista `Targets` del plan, en el mismo orden.
- `Cancelled` no es `Rejected`: `Rejected` = peticion invalida o insegura; `Cancelled` = el usuario decidio no
  aplicar una peticion valida.
- `Warnings[]` = avisos no bloqueantes con severidad `Informativo` o `Severo`; CONFIRM se exige si y solo si hay
  alguno `Severo`. Hoy solo los produce la revision de altura del Selectivo.
- Si la frontera previa o las precondiciones de PREPARE fallan, **no hay Plan ni Outcome**: el gesto termina.

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
| `SourceUnusable` | la direccion resuelve, pero la Source no es elegible o `TryCapture` falla (`NullSource`, `UnusableHeader`) | cabecera estandar o `Height <= 0` segun `UsableCustomAt` | cabecera calculada, no presente fisicamente, o no usable |
| `NoTargets` | la intencion no tiene destinos | peticion sin fondo o sin poste | `Actual` sin modulo de cabecera seleccionado; conjunto vacio |
| `MalformedTarget` | una direccion no pertenece al universo de la taxonomia | indice de poste negativo | `ModuleId` vacio o desconocido con firma vigente; un separador |
| `StaleTargets` | la peticion o el plan se capturaron contra otra topologia o resolucion | plan con firma distinta en MUTATE (incluida la generacion del sistema resuelto) | peticion con firma o generacion anterior; plan con firma distinta en MUTATE |
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

| Permitido (lecturas comprometidas y vigentes) | Prohibido sobre instancias guardadas o sobre el sistema vivo |
|---|---|
| Selectivo: `CabeceraAt`, `SelectiveCabeceraAuthority.UsableCustomAt` (pura, `:51-55`), `PostExistsIn`, `MaxFrenteCount`, `CabeceraDepthOfFondo`, `TargetFondos`, `SelectivePostGeometry.PostPeralteAt`, la revision de altura | `EffectiveCustomAt` (normaliza en sitio, `:78-88`), `SyncPostCabeceras` (poda, `SelectiveEditorState.cs:739-762`), `ApplyCabeceraToTargets`, `ImposeFondoDepth` sobre una configuracion guardada, `SaveWorkingToSelected` |
| Dinamico: `system.Modules` (lectura), `RackModuleDescriptor.Describe` (`:163`), escalares de modulo | `DynamicRackSystemBuilder.Refresh`/`ApplyPostPeralte`, `RackModuleReconciliation.Reconcile`, cualquier asignacion a un modulo |
| Ambos: `HeaderConfigurationSnapshot.TryCapture` y `Materialize` | cualquier recompute; abrir dialogos; escribir en la UI |

`ImposeFondoDepth` **si** se usa en PREPARE, pero solo sobre la **copia** materializada (§6.6).

**Frescura de la resolucion leida (RR-01).** PREPARE lee el estado comprometido **y** la resolucion vigente de ese
estado, nunca una anterior:

- En el Selectivo, `RecomputeGate.Request` dentro de un ambito `Defer` solo deja el recompute pendiente, y el recompute
  corre al cerrar el ambito **mas externo** (HECHO `U/Editor/RecomputeGate.cs:35-44`, `:63-72`; `IsDeferred` `:26`,
  `IsPending` `:29`).
- `CommitPendingEditors` recomputa dentro de su propio ambito diferido (HECHO `RackSelectiveWindow.xaml.cs:591-611`).
  Si la frontera se ejecutara dentro del ambito diferido del lote, PREPARE leeria un `lastSystem` anterior al commit.
- Si el build falla, `lastSystem` queda nulo (HECHO `:2312-2314`).

Por eso PREPARE exige **sistema resuelto no nulo, ningun recompute pendiente y ningun ambito diferido abierto**. Si no
se cumple, el gesto termina antes de PREPARE y no hay Outcome. En el Dinamico, cuya recomposicion es sincrona, basta
exigir sistema no nulo.

### 3.9 Vida y firma del plan

- El plan **vive un solo gesto**: no se guarda, no cruza recomputes y no se reutiliza.
- Lleva una **firma** suficiente de la topologia, de la resolucion y de las entradas de normalizacion que PREPARE leyo:
  - Selectivo, como minimo: fondos y frentes por fondo (`SelectiveTopology`), `MaxFrenteCount`, la profundidad de
    cabecera de cada fondo destino, el peralte efectivo de cada poste destino y **la generacion o identidad del sistema
    resuelto** que PREPARE uso (RR-01).
  - Dinamico, como minimo: la secuencia ordenada `ModuleId:Kind` y la generacion de reconstruccion (§7.6).
- MUTATE recalcula la firma **antes** de la primera asignacion. Si no coincide —incluido un sistema reconstruido entre
  PREPARE y MUTATE—: `Rejected(StaleTargets)`, mutacion cero, sin recompute. **Nunca** se re-resuelve ni se reorienta un
  destino en MUTATE.

### 3.10 MUTATE y RECOMPUTE

- **MUTATE** solo asigna lo preparado y los escalares precomputados: ninguna copia, resolucion, validacion, lectura de
  catalogo ni dialogo. La unica verificacion es la firma, previa a toda asignacion.
- **RECOMPUTE**: exactamente **uno** por `Committed`; **cero** en `Rejected` (incluido `StaleTargets` en MUTATE) y
  en `Cancelled`. El recompute de la frontera previa, si lo hubo, se cuenta aparte (§3.11).

### 3.11 Frontera previa de edicion y precondiciones

```text
Selectivo:  CommitPendingEditors → SaveWorkingToSelected        FUERA de cualquier DeferRecompute del lote
            → precondiciones RR-01: lastSystem no nulo, sin recompute pendiente, sin ambito diferido abierto
            → si falla cualquiera: el gesto termina (sin plan, sin Outcome)
            → PREPARE → CONFIRM → ambito diferido del lote { MUTATE; Recompute }   (un solo recompute)
Dinamico:   la recomposicion sincrona existente; si reconstruye, invalida origen y destinos (§7.6)
            → sistema no nulo → PREPARE → MUTATE → RECOMPUTE
```

- En el Selectivo la frontera es la C4 de ADR-0032 D6 (HECHO `U/Systems/Selective/RackSelectiveWindow.xaml.cs:560-617`;
  ya la usa «Personalizar» en `:1173` y `:1182`). **No es un codigo `Rejected`**: es una precondicion externa y su
  fallo deja cero mutacion ID6/ID7. Si habia campos pendientes validos, su recompute es propio y no cuenta como el de la
  operacion.
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
| Frontera previa, precondiciones y recompute | no | Selectivo: C4 + RR-01 + `DeferRecompute`; Dinamico: recomposicion sincrona |

## 4. AM-1 — `HeaderConfigurationSnapshot`

`sealed`, transitorio, **no persistido**, en `A/Systems/Shared`; no expone la copia mutable interna; no contiene
procedencia; no contiene UI ni AutoCAD.

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

## 5. AM-3 — `Members`, `Exceptions` y Push Back

- **Separacion verdadera, verificada.** L-2 afecta solo a overrides **por linea** fuera del editor de Push Back. El
  Dinamico cotiza resolviendo el diseno (`src/RackCad.Plugin/KindHandlers/DynamicKindHandler.cs:37-43`) y el resolver
  refresca las cabeceras de modulo (`DynamicRackSystemResolver.cs:201`, `:204`); I-53 no escribe overrides por linea.
- **`Members`** nunca es fuente authored ni se comparte. `Materialize` produce un grafo nuevo con derivado
  reconstruido.
  - Selectivo: tras cambiar la profundidad de la copia, `ImposeFondoDepth` refresca el derivado (HECHO
    `SelectiveCabeceraAuthority.cs:101-107`). `PostPeralte` y `Height` no alimentan el derivado.
  - Dinamico: el refresco lo hace el recompute existente (`Refresh`, `ApplyPostPeralte`).
- **`Exceptions`**: politica de `DeepCopy`, sin cambio de persistencia. Hecho que no cambia: en el Dinamico toda
  recomposicion pasa por `CloneHeader` (`DynamicRackSystemResolver.cs:194`, `:514-518`), que no transporta las
  `Exceptions` de runtime; lo caracteriza `Fact6` (`TC/PushBackModuleEditorCharacterizationTests.cs:251`, `:275`).
- **Promesa**: coherencia **en escritura** de lo que I-53 escribe. La coherencia historica en carga no se promete: la
  cubren los consumidores actuales y las guardas S-22 y D-24.
- **Push Back**: cero diff de produccion; **no** consume el contrato comun. Ninguna prueba de I-53 fija L-2 ni crea una
  golden de ese defecto.
- **Shared no contiene** `HeaderCoveragePolicy`, `StageInert` ni abstraccion equivalente.

## 6. Selectivo

### 6.1 Entradas de I-43 que no cambian

ADR-0032 (aceptado, inmutable): D1 (seleccion 2D proyectada y omision de posiciones inexistentes, `:55`), D4, D6
(commit atomico de pendientes), **D9** (cabecera por `(fondo, poste)`: `Height` de la receta, `Depth` del fondo;
omitir e informar, `:149`), **D10** (autoridades) y D12 (preferencia de destinos del editor). I-53 anade solo:
direccion de origen, eje de postes, Plan/Outcome, Snapshot, frescura de la resolucion y atomicidad estricta.

### 6.2 Source

| Operacion | Source | Elegibilidad | Captura |
|---|---|---|---|
| DISTRIBUTE | direccion `(fondo, poste)` recordada por la UI | `SelectiveCabeceraAuthority.UsableCustomAt(sistemaResuelto, fondo, poste)` no nula, sobre la resolucion vigente tras la frontera C4 y las precondiciones de §3.11 | en PREPARE, con `TryCapture` sobre ese valor ACTUAL |
| EDIT | `window.Configuration` (HECHO `RackSelectiveWindow.xaml.cs:1207`) | resultado no nulo y cambiado (la ventana ya compara, `:1208`) | en PREPARE, sobre el resultado |

- Direccion que no designa una posicion vigente → `SourceNotFound`; posicion vigente con cabecera estandar o no usable
  → `SourceUnusable`.
- **Por que una direccion basta en el Selectivo**: los fondos crecen y se truncan **por el final** (HECHO
  `RackSelectiveWindow.xaml.cs:492-493`) y las filas de cabecera se podan por el final **sin resurreccion** (HECHO
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
  re-expande; un explicito se poda y, si queda vacio, cae al poste actual (como `SetTargetFondosCore`, `:551-559`);
  `Actual` sigue.
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

### 6.5 HEIGHT

Viaja con la receta; **vinculante por ADR-0032 D9/D10**. No se corrige en destino y no es decision del Owner. PREPARE
revisa la altura para cada `(fondo, poste)` aplicable (extension aditiva de `SelectiveCabeceraHeightReview`, hoy por
un poste: `:89-163`) y la consolida en `Warnings`: severa → CONFIRM; informativa → aviso sin confirmacion. La semilla de
altura de «Personalizar» (`RackSelectiveWindow.xaml.cs:1193`) es EDIT anterior a I-53 y no cambia.

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

- **Por que no basta la regla local de la ventana ni dejar el peralte del origen.** El sistema resuelto guarda en
  `PostPeraltes` el valor efectivo por poste (HECHO `A/Systems/Selective/SelectiveGeometryResolver.cs:91-92`) y la
  planta lo pasa como override (`PostPeralteAt`, HECHO `SelectivePlantaBuilder.cs:74`); `PlantaHeaderLayoutBuilder`
  solo cae al `PostPeralte` **propio de la configuracion** cuando ese override es `<= 0` (HECHO
  `A/RackFrames/PlantaHeaderLayoutBuilder.cs:68-73`). Por tanto:
  - la fuga es **visible** en planta cuando el peralte efectivo del poste destino es `<= 0` (peralte del tramo sin
    fijar): el destino dibujaria el peralte del **origen** en vez del de una cabecera de ese poste;
  - y es **siempre** una incoherencia en escritura: la copia persistiria en `PostCabeceras` un peralte authored que no
    es el de su poste, contra la promesa de §5 («Personalizar» siembra el peralte desde el poste,
    `RackSelectiveWindow.xaml.cs:1199`, no desde la configuracion).
  Normalizar con `PostPeralteAt` cierra los dos casos (prueba S-16).
- La regla vive en **un solo sitio**: `PostPeralteAt` o una sobrecarga suya para evaluar el escalar de EDIT; nunca
  una re-escritura en la ventana ni en el preparador (AGENTS, convencion 2).
- **DISTRIBUTE no escribe `PostPeraltes`.**
- **EDIT** escribe `PostPeraltes[visible]` solo si ese poste queda en `Applied` en al menos un fondo. Si todo se omite,
  `Rejected(NoApplicableTargets)` y el peralte no se toca: cierra L-7 (hoy se escribe antes de aplicar,
  `RackSelectiveWindow.xaml.cs:1254-1259`).

### 6.8 Frontera previa

`CommitPendingEditors` → `SaveWorkingToSelected` → precondiciones RR-01 → PREPARE (§3.11). Hoy `ApplyCabeceraToTargets`
llama `SyncPostCabeceras` dentro de la escritura (HECHO `SelectiveEditorState.cs:1196`); en el contrato la
sincronizacion es previa y PREPARE no la invoca.

### 6.9 Aterrizaje en el estado y recompute

- **PREPARE** (G4): operacion pura nueva de Application sobre el estado comprometido y la resolucion vigente.
- **MUTATE** (G4): por cada destino de `Targets`, `fila[poste] = copia` (con el relleno de fila dispersa
  `EnsureCabeceraRow` **dentro** de un poste que el fondo tiene) y, en EDIT, el escalar de `PostPeraltes`. Metodo
  **aditivo** del estado.
- **RECOMPUTE** (G5, I-53S): uno. El `DeferRecompute` del lote envuelve **solo** MUTATE y su recompute; la frontera C4 y
  PREPARE quedan fuera (RR-01).
- **E1 sin cambio visible**: `ApplyCabeceraToTargets` y `SelectiveCabeceraHeightReview.Of` quedan **intactos** y la
  ventana los sigue usando hasta G5. En G5 la ventana pasa a Plan/Outcome; G5 decide si `ApplyCabeceraToTargets` se
  retira o se reimplementa sobre el plan, sin cambiar el resultado observable de un destino valido salvo el cierre
  de L-7.

### 6.10 Lo que no cambia

`SelectivePalletDesign` (archivo caliente) no se modifica; `Scope` y celdas; `TargetFondos` y su preferencia;
«Restablecer poste»; frontal, lateral y planta por fondo; RACKEDITAR y Actualizar. Las copias caen en
`PostCabeceras`/`ExtraFondoPostCabeceras`, ya persistidos: **sin DTO ni campo nuevo**.

### 6.11 EDIT del Selectivo: reapertura avanzada (N-01 = A, en G5 de I-53S)

- **Hecho**: «Personalizar» abre el configurador sin elegir modo (HECHO `RackSelectiveWindow.xaml.cs:1204`); el
  configurador arranca en «Configuracion rapida» (`RackFrameConfiguratorViewModel.cs:45`) y su «Aplicar» construye una
  cabecera nueva desde la plantilla (`:1205-1217`). El Selectivo lee bien el resultado (`:1207`), asi que la receta
  perdida **se aplicaria** a todos los `TargetFondos`.
- **Contrato**: una cabecera **ya personalizada** se abre en editor avanzado (`IsAdvancedEditor = true` o mecanismo
  equivalente real); una estandar, en modo rapido. Se lee el resultado real y **no** se modifica
  `RackFrameConfiguratorWindow`.
- **Seam** de presentacion testeable con el patron de Push Back (`RackPushBackSystemWindow.xaml.cs:2381-2418`).
- **Prueba S-31**: RED de comportamiento sobre codigo compilable antes del arreglo (personalizada existente → avanzado =
  true; estandar → avanzado = false).
- **Limite residual declarado**: si el usuario cambia a mano al modo rapido y pulsa «Aplicar», acepta la
  reconstruccion desde plantilla propia del configurador.
- No es una decision funcional nueva: es la regla que el Owner fijo para Push Back (I-40, ronda 2) y la aprobada para
  el Dinamico (§7.12).

## 7. Dinamico

### 7.1 Unidad de destino: `ModuleId`

`Destination = ModuleId`, **no** `(PostIndex, ModuleId)`. Razones: decision 1 del Owner en I-35 («la personalizacion es
por modulo longitudinal de rack, nunca por frente ni por poste», `I-35-editor-avanzado-push-back.md:51-53`); autoridad
authored por modulo; persistencia por modulo (`Modules[].Header`); planta y BOM por modulo; no introduce overrides por
linea y no hereda L-2.

### 7.2 AM-2 — sin sesion y sin ejecutor generico

- **No** se adopta `RackModuleEditSession` ni se modifica. **No** se crea ejecutor generico en Shared.
- Confirmar/Cancelar escenificado no es contractual: lo contractual es la atomicidad por operacion y el CONFIRM
  opcional. El Dinamico sigue sin ambito sucio (ADR-0029 D8, `:127-130`; `TU/RichEditorCloseContractTests.cs:76-85`).

| Capa | Responsabilidad |
|---|---|
| `A/Systems/Shared` (G3) | Snapshot y tipos de Plan/Outcome/codigos (§3.13) |
| `A/Systems/Dynamic` (G6, nuevo) | `ModuleTargets` con firma y generacion; PREPARE puro; MUTATE; RECOMPUTE por el constructor; operacion de reconstruccion con reconciliacion |
| Ventana del Dinamico (G7, I-53D) | orquesta gestos; abre el configurador sobre copia por seam; recompone y redibuja una vez; muestra el informe; retira presets |
| Sin tocar | `RackModuleEditSession`, `RackModuleReconciliation`, `RackModuleDescriptor`, todo `PushBack*` |

### 7.3 `IsManualOverride` — semantica fijada por ADR-0037

```text
IsManualOverride                    = SOLO longitud manual
personalizacion de cabecera         = UseCalculatedHeaderConfiguration = false
DISTRIBUTE                          → no toca Length, IsManualOverride ni IsCalculated
EDIT                                → solo si el fondo editado difiere de Length (regla existente)
```

HECHO del estado actual, por ruta (`RackDynamicSystemWindow.xaml.cs` salvo indicacion):

| Ruta | Hoy | En I-53 |
|---|---|---|
| `ApplyModule_Click` (`:702-704`) | longitud editada → `IsManualOverride = true` | sin cambio (es longitud) |
| `EditHeader_Click` (`:772-779`) | fondo editado ≠ `Length` → `Length = fondo`, `IsManualOverride = true` | sin cambio; la regla pasa a la preparacion de EDIT en Application (G6) y la ventana la consume (G7) |
| Preset «Personalizada N» (`:1890-1893`) | copia **solo** la configuracion y fija `IsManualOverride = true` | **se elimina** con OD-8 (G7) |
| «Calculada» (`:1876-1881`) | regenera la configuracion y fija `IsManualOverride = false` | **sin cambio y caracterizada**: es un restablecimiento, no una reutilizacion; follow-up N-02 (§17) |
| Reconciliacion (`RackModuleReconciliation.cs:164`, `:186-191`) y par ordinal (`DynamicEditorDesignAssembler.cs:71`) | leen el flag como longitud manual | coherente con la semantica fijada |

El XML-doc de `DynamicRackModule.IsManualOverride` (`D/Systems/Dynamic/DynamicRackModule.cs:28-29`) dice todavia
«modificado respecto del layout»: **G6 lo alinea** con ADR-0037, sin cambio de comportamiento. Guardas: D-12, D-14,
D-15, D-26.

### 7.4 Source

- DISTRIBUTE: direccion `ModuleId` + firma de la peticion (§7.6), recordada por la UI.
- En PREPARE: la firma de la peticion debe coincidir (si no, `StaleTargets`); el `ModuleId` debe designar un modulo de
  cabecera (si no, `SourceNotFound`); debe ser personalizado (`UseCalculatedHeaderConfiguration = false` y
  configuracion no nula), fisicamente presente y capturable (si no, `SourceUnusable`).
- EDIT: `window.Configuration` sobre una copia (§7.12).

### 7.5 `ModuleTargets`

| Modo | Significado |
|---|---|
| `Actual` | el modulo de cabecera seleccionado. La seleccion se pierde en cada recomposicion (HECHO `RackDynamicSystemWindow.xaml.cs:556`): sin seleccion → `NoTargets` |
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
PREPARE (pura; exige sistema no nulo)
  verificar firma de la peticion → resolver ModuleId origen → TryCapture → resolver ModuleIds destino
  → validar IsHeader, Length > 0 (si no: DestinationInvalid), presencia fisica
  → Materialize una copia por destino aplicable
  → NO normalizar (no duplicar el constructor)
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
no, y refresca (`:179-194`). El peralte del Dinamico es **uniforme por rack** (la ventana lo fuerza en
`RackDynamicSystemWindow.xaml.cs:758-761` y `:1896-1899`), asi que no existe la fuga por poste del Selectivo.

### 7.9 Reconstruccion y reconciliacion (OD-2.b)

```text
reconstruir  = intenciones := DynamicRackSystemResolver.Snapshot(sistemaPrevio, ...).Modules
               sistema     := builder.BuildDefault(...)
               informe     := RackModuleReconciliation.Reconcile(intenciones, sistema, restaurados = ∅)
               generacion  := generacion + 1
«Restaurar estandar» (forceRebuild) = reconstruir SIN intenciones (reset; semantica intacta, Fact3)
sin reconstruccion                  = como hoy (UpdateHeaderHeightInPlace + ApplyPostPeralte)
```

- **Proyeccion de intenciones**: `DynamicRackSystemResolver.Snapshot` (HECHO `:292`, modulos en `:408-421`) es la
  autoridad de la intencion editable del Dinamico: la misma que `BuildDesign` persiste
  (`DynamicEditorDesignAssembler.cs:158`). No se crea un mapeador paralelo. Transporta la configuracion con
  `CloneHeader` (sin `Exceptions` de runtime, como la persistencia); `Reconcile` la copia con `DeepCopy` y cierra con
  `builder.Refresh` (HECHO `RackModuleReconciliation.cs:195`, `:209-210`).
- **Que conserva** (HECHO `RackModuleReconciliation.cs:125-213`): personalizada por `ModuleId + Kind`, **adaptada**
  en fondo y peralte; longitud manual de **cabeceras y separadores** (`:114-115`, `:164`, `:186-191`).
- **Que informa**: `RackModuleReconciliationResult` con `Preserved`, `Adapted` (subconjunto de `Preserved`),
  `Removed`, `Incompatible`, `Restored`, `Describe()` y `LostAnything` (HECHO `:16-66`). `restaurados = ∅` porque el
  Dinamico no escenifica restauraciones: «Calculada» restablece en el acto y la reconstruccion ya no ve nada que
  llevar.
- **Cambios de comportamiento aprobados por OD-2.b** (visibles con I-53D):
  - una personalizada sobrevive a un cambio de tarima o fondos si su `ModuleId + Kind` sigue existiendo;
  - las longitudes manuales se casan por id y tipo, y ya no por ordinal de cabecera;
  - la longitud manual de un **separador** tambien se conserva (hoy solo el fondo de las cabeceras,
    `DynamicEditorDesignAssembler.cs:61-116`);
  - lo perdido se informa (hoy se pierde en silencio).
- **G6 (E1)**: la operacion de Application con pruebas, **sin cablear**: la ventana sigue llamando al par ordinal
  (`RackDynamicSystemWindow.xaml.cs:515`, `:527`) y `Fact5` sigue verde y sin cambios.
- **G7 (I-53D)**: la ventana cambia a la reconciliacion y muestra `Describe()` en su estado. El par ordinal
  `SnapshotHeaderFondos`/`RestoreHeaderFondos` queda sin llamador de produccion (hoy su unico llamador es la ventana)
  y **se retira en G7**; sus pruebas (`TC/DynamicEditorDesignAssemblerTests.cs` y las dos aserciones del Dinamico de
  `Fact5`, `TC/PushBackModuleEditorCharacterizationTests.cs:186`, `:215`) se **reapuntan** al contrato de
  reconciliacion, nunca se relajan; las aserciones de Push Back no se tocan.

### 7.10 Overrides por linea

El Dinamico sigue sin escribirlos. Guardas: ninguna operacion de I-53 escribe `HeaderLineOverrides` ni
`DerivedPostLineOverrides`, y uno entrante sobrevive intacto a EDIT y DISTRIBUTE. **En una reconstruccion hoy se
pierden** (el sistema reconstruido nace sin ellos y `BuildDesign` los toma de el): se **caracteriza** ese comportamiento
historico y **no** se cambia (L-2 fuera; el Dinamico no se convierte a overrides por linea).

### 7.11 Retirada de presets «Personalizada N» (OD-8)

- **Contrato**: los presets no son un tercer mecanismo; ID6 los reemplaza; el origen es siempre una cabecera
  existente; **no existe portapapeles** persistente ni en memoria de configuraciones authored fuera de la direccion de
  origen.
- **Estado actual (HECHO `RackDynamicSystemWindow.xaml.cs`)**: lista en memoria `headerPresets` (`:79`), clase
  `HeaderPreset` (`:1916-1926`), alta en `EditHeader_Click` (`:781-785`), entradas del desplegable (`:1834`) y rama de
  preset en `ConfigBox_SelectionChanged` (`:1882-1894`). No se persisten.
- **G7 (I-53D) elimina** esas cinco piezas. **Conserva** el restablecimiento por modulo a «Calculada» (no es un
  preset); la forma de su control es detalle de G7.
- Pruebas: D-27 (no aparece «Personalizada N» ni queda lista de configuraciones en la ventana) y D-26.

### 7.12 L-1 (G7 de I-53D)

Contrato:

1. El configurador se abre sobre una **copia** (`RackFrameProjectStore.DeepCopy`) de la configuracion del modulo,
   nunca sobre la instancia viva (hoy `RackDynamicSystemWindow.xaml.cs:753` la pasa por referencia).
2. Se lee el resultado **real**: `window.Configuration` (HECHO `U/RackFrames/RackFrameConfiguratorWindow.xaml.cs:76`).
3. Si la cabecera **ya esta personalizada**, se abre en editor avanzado: `window.ViewModel.IsAdvancedEditor = true`
   (propiedad publica, `RackFrameConfiguratorViewModel.cs:176-191`; hoy arranca en falso, `:45`).
4. **No** se modifica `RackFrameConfiguratorWindow` (I-35, decision 5).
5. **Seam de presentacion** en la ventana del Dinamico, con la forma del precedente de Push Back:
   `Action<RackFrameConfiguratorWindow>` que sustituye solo el `ShowDialog` y deja la lectura del resultado en el
   camino de produccion (HECHO `RackPushBackSystemWindow.xaml.cs:2381-2418`).
6. El peralte del rack se aplica a la **copia resultado** antes de comparar (hoy se fuerza sobre la instancia viva,
   `RackDynamicSystemWindow.xaml.cs:758-761`).

**Limite residual declarado**: si el usuario cambia a mano a «Configuracion rapida» y pulsa «Aplicar», esa accion
consciente reconstruye desde plantilla por comportamiento del configurador.

Pruebas RED **vistas fallar** antes del arreglo y confirmadas juntas en G7 (AGENTS, «Pruebas», punto 2): D-28 «Aplicar»
rapido, D-29 «Restaurar estandar», D-30 «Abrir proyecto» y D-31 reapertura avanzada sobre copia. `Fact7`
(`TC/PushBackModuleEditorCharacterizationTests.cs:424`, `:439-440`) se reapunta si su literal deja de existir.

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
- Unica excepcion declarada, y solo en G7 de I-53D: las aserciones **del Dinamico** de
  `TC/PushBackModuleEditorCharacterizationTests.cs` (`Fact5` × 2; `Fact7` si cambia su literal).
- L-2 no se corrige ni se fija con una prueba (P-03); registrado como follow-up (§17).

## 9. Entregas y unidades Git (OD-6 B′, PA-1B)

### 9.1 Unidades

| Unidad | Rama | Entrega | Cambio visible | Validacion |
|---|---|---|---|---|
| **I-53** (esta) | `feature/cabeceras-configurables-multidestino` | contrato congelado + **E1 fundacion**: G3 nucleo compartido, G4 Selectivo Application/estado, G6 Dinamico Application/estado/reconciliacion **sin cablear** | **ninguno** | E1-V (§9.4) |
| **I-53S** (futura) | `feature/cabeceras-multidestino-selectivo` | **E2**: G5 UI del Selectivo sobre Plan/Outcome; ID6/ID7; L-7 cerrado; N-01; cableado de RR-01 | si | Owner Validation Selectivo (AutoCAD 2025) |
| **I-53D** (futura) | `feature/cabeceras-multidestino-dinamico` | **E3**: G7 UI del Dinamico; ID6/ID7; retirada de presets; L-1; reconciliacion cableada con informe; retirada del par ordinal | si | Owner Validation Dinamico (AutoCAD 2025) |

### 9.2 Reglas de la linea

- Un solo contrato conceptual (este, congelado), un solo **ADR-0037** y un solo
  [registro de decisiones](../automation/decisions/I-53.md) para las tres unidades (precedente: `decisions/I-37.md` para
  I-37A..D).
- **I-53** sigue el ciclo WORKFLOW completo: E1-C → E1-V → cierre documental → merge `--no-ff` → CI posterior al merge y
  cobertura → limpieza de rama y worktree. Su cierre deja **explicito** que la linea funcional continua en I-53S e I-53D.
- **I-53S** se reclama desde `origin/main` **despues** de integrar y verificar I-53; **I-53D**, despues de integrar
  I-53S. Ambas por el **caso (d)** de WORKFLOW §2, con **OD-6** como autorizacion explicita del Owner.
- Cada subunidad tiene reclamo, rama, worktree, bootstrap con **su propia fila de ROADMAP** (nace en ese bootstrap; no se
  precrea), contrato minimo que **remite a este contrato congelado**, Candidato, Owner Validation, integracion y
  limpieza. Sin Discovery ni Proposal propios, salvo hallazgo material.
- El cierre documental de I-53D registra la linea como completa en la fila de I-53 (precedente: el cierre de I-39D,
  `1f24766`, sobre la fila paraguas de I-39).
- Cualquier desviacion material del contrato congelado vuelve al Coordinador y, si es arquitectonica, al Arquitecto.

### 9.3 Invariantes de E1 — «sin cambio visible» demostrable

| Id | Invariante | Como se comprueba |
|---|---|---|
| E1-INV-1 | el diff de E1 no toca `src/RackCad.UI/**`, `src/RackCad.Plugin/**`, `assets/**`, `deploy/**`, `eng/**` ni `.github/**` | `git diff --name-only <base-E1>..<candidato-E1>` |
| E1-INV-2 | los tipos nuevos no tienen llamador de produccion: solo pruebas | busqueda de sus nombres en `src/` |
| E1-INV-3 | ninguna prueba existente se modifica; siguen verdes las suites de UI, las firmas de dibujo de I-24 y las suites de I-40 e I-43 | `git diff` sobre `tests/` + suites |
| E1-INV-4 | las modificaciones a archivos existentes son **aditivas** (o solo XML-doc, en `DynamicRackModule.cs`): no cambian `ApplyCabeceraToTargets`, `SelectiveCabeceraHeightReview.Of`, `SnapshotHeaderFondos`/`RestoreHeaderFondos` ni ninguna ruta que use una ventana | diff + guardas |
| E1-INV-5 | Push Back: cero diff de produccion | P-02 |

### 9.4 Evidencia y validacion por unidad

- **Candidato de cada unidad** (AGENTS, «Pruebas», punto 1): Core Full local, **UI Full local**, build Debug de UI y
  de Plugin, CI de `push` 4/4 sobre el SHA exacto. LC-UI no reduce ningun Candidato.
- **E1-C** anade las invariantes E1-INV-1..5.
- **E1-V**: `requires_autocad: true` y `requires_owner_validation: true` son **monotonicos** (AUTOMATION_PLAN, «La
  metadata de validacion del dueno es MONOTONICA»), asi que E1 **no** queda exenta. E1-V = **veredicto explicito del
  Owner** sobre la evidencia de E1-C **mas** un **smoke corto en AutoCAD 2025** sobre el SHA exacto (§13.8). No es
  validacion funcional de ID6/ID7: E1 no expone esa UI.
- **E2-V / E3-V**: Owner Validation en AutoCAD 2025 sobre el SHA exacto del Candidato de su unidad (§13.8).
- **Cada integracion**: merge `--no-ff`, **CI posterior al merge** sobre el `MERGE_SHA` con cobertura y comprobacion
  diferida de la cobertura del Candidato (WORKFLOW §4.5 pasos 5-7). Nada se hereda entre unidades.

### 9.5 Aislamiento y condiciones de parada

- El nucleo compartido queda **provisional** en G3 y se considera demostrado al pasar G4 **y** G6. Si G6 obliga a
  cambiar el nucleo, se repiten las suites relevantes de G3 y G4 antes del Candidato E1.
- Tras integrar I-53, el nucleo esta en `main`. Si G5 o G7 necesitan cambiar el nucleo compartido, o G7 necesita tocar
  codigo del Selectivo, se detiene y vuelve al Coordinador y al Arquitecto; nunca se cambia «de paso».
- Si E1 necesita tocar una ventana, el configurador, el Plugin, una prueba existente o Push Back, se detiene.

### 9.6 Por que tres unidades

- **Revalidacion por SHA exacto (M-08)**: con un Candidato unico, cada ronda de correccion del Dinamico produciria un
  SHA nuevo e invalidaria la validacion del Selectivo (AGENTS, «Reutilizacion de evidencia»). Con I-53S integrada antes
  de reclamar I-53D, una correccion del Dinamico no obliga a revalidar el Selectivo.
- **Continuar una misma rama despues de un merge (P-A) se descarto**: violaba WORKFLOW (§10.2).
- **Una rama padre solo documental seguida de tres unidades se descarto**: anadia una cuarta integracion sin valor de
  producto. Con E1 en esta rama quedan **exactamente tres** integraciones, y el precedente I-37A..C ya integro
  fundaciones que no dibujan.

## 10. Compatibilidad con WORKFLOW — PA-1 = PA-1B (resuelta)

### 10.1 El mecanismo que WORKFLOW ya preve

- §2: «1 iniciativa = 1 rama = 1 worktree = 1 entrada en ROADMAP»; «si crece mas, se parte (el ROADMAP muestra como)»,
  con los precedentes I-36A..D, I-37A..D e I-39A..D.
- §2, caso (d): una iniciativa autorizada explicitamente por el Owner sin fila previa nace con reclamo atomico y
  bootstrap inmediato (contrato + fila).
- §3, §4.5 y §4.6: cada rama cumple su ciclo completo, con limpieza tras las comprobaciones posteriores al merge.

### 10.2 Por que se descarto P-A

| Regla de WORKFLOW | Choque de P-A (continuar la misma rama tras un merge) |
|---|---|
| §3 y §4.6: la rama sobrevive solo hasta las comprobaciones posteriores al merge; «solo entonces se limpia» | la rama habria seguido viva tras E1 y E2 |
| §4.5.4: el cierre documental es el **ultimo commit de la rama** y marca la iniciativa `integrada` | E1 y E2 no habrian sido el ultimo commit ni la iniciativa entera |
| §8: la marca de cierre en ROADMAP es `integrada (fecha)`; §2: tres momentos | «entrega N/3» no existe |
| §8: «Cambia el proceso mismo → este documento + ADR, antes de aplicar» | P-A habria sido un cambio de proceso por excepcion local |

### 10.3 Como cumple PA-1B

| Regla | PA-1B |
|---|---|
| 1 iniciativa = 1 rama = 1 worktree = 1 fila | I-53, I-53S e I-53D tienen cada una su rama, su worktree y su fila |
| Reclamo atomico y bootstrap | I-53 ya reclamada; I-53S e I-53D se reclaman desde `origin/main` por el caso (d) con OD-6, y su fila nace en su bootstrap |
| Ciclo de integracion y limpieza | completo en cada unidad; ninguna rama sobrevive a su integracion verificada |
| Momentos de ROADMAP | fila de I-53 en su cierre (momento 3); filas de I-53S e I-53D en su bootstrap (momento 2) y su cierre (momento 3) |
| WORKFLOW | **no** se modifica; no hay excepciones locales |

### 10.4 Owner

`OWNER_REQUIRED_FOR_PA1 = NO`: PA-1B realiza exactamente lo que el Owner aprobo en OD-6 —una iniciativa conceptual,
un contrato, tres integraciones— con el mecanismo que el propio WORKFLOW prescribe; la diferencia con P-A es mecanica.

## 11. Ramas paralelas y coordinacion

### 11.1 I-50 — integrada

Merge `f8deb675c6d1ef0e64693b157d69c4cc170d7b24`; CI posterior al merge run `34736306222` **success** (4/4, con
`rackcad-coverage-cobertura`); rama remota retirada. I-53 se rebaso sobre ese merge en G2-F, asi que la veda sobre
`RackSelectiveWindow.xaml/.cs`, `RackDynamicSystemWindow.xaml/.cs`, `SelectiveShellMigrationTests`,
`DynamicShellMigrationTests`, `SelectiveEditorWindowTests`, `DynamicEditorWindowTests` y las pruebas nuevas que
instancien esas ventanas queda **liberada**. G5 y G7 **no** pertenecen a esta rama: son I-53S e I-53D.

### 11.2 Solape de los archivos de E1 con `main`

No hay solape activo: las lineas que I-50 anadio a `SelectiveEditorState.cs`, `DynamicEditorDesignAssembler.cs`,
`DynamicRackSystemResolver.cs` y `SelectiveGeometryResolver.cs` ya estan en la base de esta rama.

| Archivo previsto | Gate | Nota |
|---|---|---|
| `A/Systems/Shared/HeaderConfigurationSnapshot.cs` y tipos de Plan/Outcome (nuevos) | G3 | nuevo |
| `A/Systems/Selective/` tipos nuevos (`PostTargets`, preparacion, firma) | G4 | nuevo |
| `A/Systems/Selective/SelectiveEditorState.cs` (metodo aditivo de MUTATE) | G4 | fuera de la linea que anadio I-50 (`:1318`) |
| `A/Systems/Selective/SelectiveCabeceraHeightReview.cs` (extension aditiva) | G4 | — |
| `A/Systems/Selective/SelectivePostGeometry.cs` (sobrecarga, solo si hace falta) | G4 | — |
| `A/Systems/Dynamic/` tipos nuevos (targets, firma, preparacion, reconstruccion) | G6 | nuevo |
| `D/Systems/Dynamic/DynamicRackModule.cs` (solo XML-doc de `IsManualOverride`) | G6 | — |
| `A/Systems/Dynamic/DynamicEditorDesignAssembler.cs`, `DynamicRackSystemResolver.cs` | — | solo lectura en E1 |

### 11.3 Coordinacion futura

- **I-49** @ `048a508` (Proposal V6, solo docs): preve modificar un miembro de `RackSelectiveWindow.xaml.cs` en su G10
  (`I-49-proposal-v6.md:2077-2080`). Si coincide con el arranque de **I-53S**, se aplica la regla de archivo caliente
  (WORKFLOW §7) y se serializa.
- **I-52** @ `0445718` (Proposal V2 + ADR-0036 `propuesto`, solo docs): comparte el indice de ADR (fila 0036 junto a la
  0035 y la 0037).
- **I-54** @ `5d25da8` (Proposal V3, solo docs): sin archivos de I-53.

### 11.4 Numero de ADR

Hasta que ADR-0037 llegue a `main`, se comprueba el numero en cada preflight; si otra rama integra antes un ADR-0037,
este se renumera cambiando solo numero y enlaces.

## 12. Gates congelados

```text
I-53 — rama actual / E1
  G2-F  FROZEN
  G3    Shared foundation
  G4    Selectivo Application/state
  G6    Dinámico Application/state/reconciliation
  E1-C
  E1-V
  E1-I

I-53S — futura
  G0 claim/bootstrap
  G5 Selectivo UI + N-01
  E2-C
  E2-V
  E2-I

I-53D — futura
  G0 claim/bootstrap
  G7 Dinámico UI + L-1 + preset removal + reconciliation report
  E3-C
  E3-V
  E3-I
```

| Gate | Unidad | Contenido | RED / verde | Evidencia de cierre | Condicion |
|---|---|---|---|---|---|
| **G2-F** | I-53 | freeze: Proposal V2 congelada, contrato, decisiones, ADR-0037 aceptado, follow-ups | — | commit documental + CI de `push` | `ARCHITECT_V2 = AGREED`; aceptacion del Owner |
| **G3** | I-53 | nucleo compartido **provisional**: Snapshot, Plan, Outcome, codigos, fixtures | RED por asercion → verde; guardas C-04, C-07..C-10 | Core local; CI | G2-F cerrado con CI verde |
| **G4** | I-53 | Selectivo Application/estado: `PostTargets`, PREPARE, MUTATE aditivo, revision multi-poste, peralte, firma con generacion (RR-01) | RED → verde; verdes I-43 | Core local; CI | G3 |
| **G6** | I-53 | Dinamico Application/estado: `ModuleTargets`, firma y generacion, PREPARE/MUTATE/RECOMPUTE, reconstruccion con reconciliacion **sin cablear**, XML-doc de `IsManualOverride` | RED → verde; `Fact3`/`Fact5`/`Fact7` intactos | Core local; CI | G3; si cambia el nucleo, repetir G3 + G4 |
| **E1-C** | I-53 | Candidato E1 (SHA exacto) + E1-INV-1..5 | — | Core Full + UI Full locales, Debug UI + Plugin, CI `push` 4/4 | G4 + G6 |
| **E1-V** | I-53 | veredicto del Owner + smoke corto AutoCAD 2025 | — | veredicto sobre E1-C | E1-C |
| **E1-I** | I-53 | integracion (WORKFLOW §4.5 completo): cierre documental con la linea abierta en I-53S/I-53D, merge, CI posterior, cobertura, limpieza | — | `MERGE_SHA` CI 4/4 + cobertura | E1-V |
| **G0** | I-53S | reclamo desde `origin/main` (caso (d), OD-6) + bootstrap (fila + contrato minimo) | — | reclamo aceptado sin force | E1-I verificada; preflight de archivos calientes (I-49) |
| **G5** | I-53S | Selectivo UI + N-01 + cableado de RR-01; censo actualizado, **no relajado** | RED UI → verde (S-27..S-32) | Core + UI; CI | G0 de I-53S |
| **E2-C / E2-V / E2-I** | I-53S | Candidato, Owner Validation Selectivo, integracion y limpieza | — | como E1-C; veredicto AutoCAD; `MERGE_SHA` CI + cobertura | G5 |
| **G0** | I-53D | reclamo desde `origin/main` (caso (d), OD-6) + bootstrap | — | reclamo aceptado sin force | E2-I verificada |
| **G7** | I-53D | Dinamico UI: ID6/ID7, retirada de presets, L-1 (RED vistos fallar), reconciliacion cableada e informe, retirada del par ordinal, `Fact5` reapuntados; censo actualizado | RED UI → verde (D-26..D-39) | Core + UI; CI | G0 de I-53D |
| **E3-C / E3-V / E3-I** | I-53D | Candidato, Owner Validation Dinamico, integracion, cierre de la linea y limpieza | — | como E1-C; veredicto AutoCAD; `MERGE_SHA` CI + cobertura | G7 |

## 13. Matriz de pruebas (congelada; no se implementa en G2)

### 13.1 Reglas

- Gates por unidad: **G3, G4 y G6** en I-53; **G5** en I-53S; **G7** en I-53D.
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
| C-02 | cada `Materialize()` devuelve un grafo nuevo: ningun objeto ni lista compartido con el origen ni entre copias; `Members` reconstruido; `Exceptions` segun `DeepCopy` | RED |
| C-03 | mutar el origen tras capturar, o una copia materializada, no cambia el Snapshot ni otras copias (I4-I6) | RED |
| C-04 | el Snapshot no expone la copia privada, colecciones ni setters | guarda |
| C-05 | Plan: `Rejected` sin Targets; `Prepared` con Targets, Omitted, Warnings y firma; no hay `Applied` en el Plan | RED |
| C-06 | Outcome: `Committed(Applied = Targets, mismo orden)`; `Cancelled` ≠ `Rejected`; ninguno de los dos lleva Applied | RED |
| C-07 | conjunto cerrado: tres motivos Omitted y siete codigos Rejected | guarda |
| C-08 | neutralidad: los tipos nuevos no referencian WPF, `Autodesk.*`, `ObjectId`, `Database`, `Transaction` ni `BlockReference` | guarda |
| C-09 | el nucleo no usa `CloneHeader` ni `ToConfiguration` sin refresco, y no crea serializacion ni DTO | guarda |
| C-10 | Shared no contiene politica de cobertura, `StageInert` ni ejecutor generico | guarda |

### 13.3 Selectivo (G4 en I-53; G5 en I-53S)

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
| S-10 | **poste ausente** en un fondo → `Omitted(AbsentInScope)`; nunca creado ni clampeado; sin resurreccion tras reducir y volver a crecer | RED multi-poste; verdes existentes `ATargetWhereThePostDoesNotExist_…`, `ShrinkingAFondo_…`, `Scenario4_…` | G4 |
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
| S-21 | firma del plan: topologia o generacion del sistema resuelto cambiada entre PREPARE y MUTATE → `Rejected(StaleTargets)`, mutacion cero | RED | G4 |
| S-22 | BOM del editor = BOM del diseno recargado por la ruta de `RACKBOMTOTAL` (store → registro → resolver → builder), con personalizadas distribuidas | guarda | G4 |
| S-23 | RACKEDITAR: reabrir con personalizadas distribuidas en varios fondos (`LoadExisting`) | RED (hoy sin fijar) | G4 |
| S-24 | guardar/reabrir: `RoundTrip_PreservesDifferentCustomsInFondos0And1And3` + caso distribuido | verde + RED | G4 |
| S-25 | legacy: documentos sin personalizadas y «Personalizar» de un poste dan el mismo resultado, **excluido el escenario de L-7** (cambia por diseno; S-11) | verdes I-43 | G4 |
| S-26 | regresion I-43: `TargetFondos × Scope` de celdas intacto | verdes | G4 |
| S-27 | **frontera C4 anterior al lote**: con pendientes invalidos el gesto aborta sin plan ni mutacion ID6/ID7; con pendientes validos su recompute se cuenta aparte | RED (UI) | G5 |
| S-28 | un recompute por operacion confirmada (`RecomputeCount`, `RackSelectiveWindow.xaml.cs:2250`); cero en Rejected y Cancelled | RED (UI) | G5 |
| S-29 | Actualizar y dibujo lateral, planta y frontal por fondo con copias distribuidas | verdes + E2E nueva | G5 |
| S-30 | dialogos de confirmacion e informe por seam, sin modal real | guarda (UI) | G5 |
| S-31 | **N-01**: «Personalizar» de una cabecera ya personalizada abre el configurador en editor avanzado (`IsAdvancedEditor = true`); una estandar, en modo rapido (`false`) | RED (UI), obligatoria | G5 |
| S-32 | **RR-01**: con recompute pendiente, ambito diferido abierto o `lastSystem` nulo, el gesto termina antes de PREPARE (sin plan ni Outcome); la frontera C4 queda fuera del ambito diferido del lote; un recompute entre PREPARE y MUTATE → `StaleTargets` | RED (UI) | G5 |

### 13.4 Dinamico (G6 en I-53; G7 en I-53D)

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
| D-23 | overrides por linea: I-53 nunca los escribe; uno entrante sobrevive a EDIT y DISTRIBUTE; en reconstruccion hoy se pierden: se caracteriza sin cambiar | guarda + caracterizacion | G6 |
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
| D-36 | Actualizar y dibujo lateral, frontal y planta con personalizada de modulo | verdes + E2E | G7 |
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

### 13.6 Exigencias → pruebas

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
| 19 | N-01: reapertura avanzada en el Selectivo | S-31 |
| 20 | RR-01: frescura de la resolucion leida | S-32, S-21 |

### 13.7 Correcciones de clasificacion aplicadas

C-06 de V1 → guarda (C-08). P-03 → comprobacion de gate. S-22 de V1 → S-25 excluye L-7. Redundancias fusionadas:
S-10/S-12 → S-10; S-18/S-20 → S-29; D-20/D-22 → D-36; C-02/C-03 → C-02.

### 13.8 Validaciones por unidad

- **E1-V (I-53)**: veredicto explicito del Owner sobre E1-C (E1-INV-1..5 comprobadas, Candidato completo, tipos nuevos
  sin llamadores de produccion, conteos de suites) **mas** smoke corto en AutoCAD 2025 sobre el SHA exacto:
  1. abrir el editor Selectivo;
  2. abrir el editor Dinamico;
  3. Actualizar o Insertar una vista;
  4. comparar o verificar `RACKBOMTOTAL`;
  5. confirmar que no hay regresion visible.
  No es validacion funcional de ID6/ID7.
- **E2-V (I-53S, AutoCAD 2025)**: personalizar (una personalizada abre en editor avanzado, N-01); tomar como origen;
  aplicar a uno, a varios y a todos; origen editado despues de tomarlo (se aplica el actual); omisiones por poste ausente
  y por fondo inexistente; altura severa (confirmar y cancelar); destino con peralte heredado desde un origen con peralte
  propio (con y sin peralte del tramo); independencia; Actualizar; RACKEDITAR; guardar y reabrir; vistas por fondo; BOM
  del editor frente a `RACKBOMTOTAL`.
- **E3-V (I-53D, AutoCAD 2025)**: L-1 con «Aplicar» rapido y reapertura avanzada; ausencia de «Personalizada N»; tomar
  como origen; aplicar a uno, a varias y a todas; omision por frentes en blanco; independencia; cambio de tarima con
  informe (personalizada adaptada, longitud manual de cabecera y de separador); invalidacion informada de origen y
  destinos tras reconstruir; «Restaurar estandar»; «Calculada»; Actualizar; RACKEDITAR; guardar y reabrir; vistas; BOM
  del editor frente a `RACKBOMTOTAL`.

## 14. Archivos previstos por unidad

| Unidad / gate | Crear | Modificar | Solo leer |
|---|---|---|---|
| I-53 / G2-F (hecho) | — | `docs/initiatives/I-53-proposal-v2.md`, contrato, `docs/automation/decisions/I-53.md`, `docs/adr/0037-*.md`, `docs/adr/README.md`, `docs/ideas-futuras.md` | todo lo citado |
| I-53 / G3 | `A/Systems/Shared/HeaderConfigurationSnapshot.cs` y tipos de Plan/Outcome/codigos; pruebas nuevas en `TC/` | — | `RackFrameProjectStore.cs`, `RackFrameConfiguration.cs`, `RackDesignValidation.cs` |
| I-53 / G4 | tipos nuevos del Selectivo (`PostTargets`, preparacion, firma); pruebas nuevas en `TC/` | `SelectiveEditorState.cs` (aditivo), `SelectiveCabeceraHeightReview.cs` (aditivo), `SelectivePostGeometry.cs` (sobrecarga, solo si hace falta) | `SelectiveCabeceraAuthority.cs`, `SelectiveFondoTargets.cs`, `SelectiveTargetResolver.cs`, `PlantaHeaderLayoutBuilder.cs` |
| I-53 / G6 | tipos nuevos del Dinamico (targets, firma, preparacion, reconstruccion); pruebas nuevas en `TC/` | `D/Systems/Dynamic/DynamicRackModule.cs` (solo XML-doc de `IsManualOverride`) | `RackModuleReconciliation.cs`, `RackModuleDescriptor.cs`, `DynamicRackSystemBuilder.cs`, `DynamicRackSystemResolver.cs`, `DynamicEditorDesignAssembler.cs` |
| I-53 / E1-I | — | `docs/HANDOFF.md`, `docs/ROADMAP.md` (fila de I-53, momento 3), contrato | — |
| I-53S / G0 | contrato de I-53S | `docs/ROADMAP.md` (su fila, momento 2) | contrato congelado de I-53 |
| I-53S / G5 | pruebas nuevas en `TU/` | `RackSelectiveWindow.xaml/.cs`, `TU/SelectiveShellMigrationTests.cs`, `TU/SelectiveEditorWindowTests.cs` si cambian firmas | `RackFrameConfiguratorWindow.xaml.cs`, `RackFrameConfiguratorViewModel.cs` |
| I-53D / G0 | contrato de I-53D | `docs/ROADMAP.md` (su fila, momento 2) | contrato congelado de I-53 |
| I-53D / G7 | pruebas nuevas en `TU/` | `RackDynamicSystemWindow.xaml/.cs`, `TU/DynamicShellMigrationTests.cs`, `TU/DynamicEditorWindowTests.cs` si cambian firmas; `DynamicEditorDesignAssembler.cs` (retirada del par); `TC/DynamicEditorDesignAssemblerTests.cs`; aserciones del Dinamico en `TC/PushBackModuleEditorCharacterizationTests.cs` | `RackFrameConfiguratorWindow.xaml.cs`, `RackFrameConfiguratorViewModel.cs` |
| E2-I / E3-I | — | `docs/HANDOFF.md`, `docs/ROADMAP.md` (su fila; E3-I tambien la fila de I-53 como linea completa), su contrato | — |

**No se tocan en ninguna unidad:** `A/Systems/PushBack/**`, `U/Systems/PushBack/**`, `RackModuleEditSession.cs`,
`RackModuleReconciliation.cs`, `RackFrameProjectStore.cs`, `RackFrameConfiguratorWindow.xaml(.cs)`,
`RackFrameConfiguratorViewModel.cs`, los DTO y disenos de Selectivo y Dinamico (`SelectivePalletDesign.cs` es
caliente), Plugin, `assets/`, `deploy/`, `.github/`.

## 15. ADR-0037 — aceptado

- **Archivo**: [`docs/adr/0037-reutilizacion-de-cabecera-por-copia-y-distribucion-por-lotes.md`](../adr/0037-reutilizacion-de-cabecera-por-copia-y-distribucion-por-lotes.md).
- **Estado**: `aceptado` el **2026-09-12** por el Owner: «**Acepto el ADR**», referido a ADR-0037 de I-53; fila del
  indice actualizada.
- **Correcciones aplicadas antes de aceptar** (re-revision G2C):
  1. la alternativa de disciplina dice que el Discovery registro cuatro sitios de referencia compartida y confirmo al
     menos un defecto real (L-1), sin afirmar que la disciplina «fallo»;
  2. la decision 5 fija que la preparacion lee solo estado comprometido y su resolucion vigente, sin recompute
     pendiente ni diferido;
  3. la nota de numeracion sigue vigente hasta que el ADR llegue a `main`.
- **Alcance**: reutilizar = copia, nunca vinculo vivo; origen y destinos separados; taxonomia de destino por sistema,
  sin destino universal; materializacion canonica; PREPARE todo-o-nada; Omitted frente a Rejected tipados; Outcome
  separado del Plan; un recompute por operacion confirmada; autoridad de normalizacion por sistema;
  `IsManualOverride` = longitud manual en el Dinamico; Push Back conserva I-40.
- **Excluido**: XAML y controles; nombres accidentales de clases; reglas ya cubiertas por ADR-0032; presets; esquema de
  entregas de I-53.

## 16. Decisiones — tabla final (cerrada)

| Id | Tema | Estado | Autoridad |
|---|---|---|---|
| OD-1 | Texto de ID6/ID7 | **RESUELTA** | autorizacion original del Owner |
| OD-2 | Unidad de destino del Dinamico | **RESUELTA: `ModuleId`** | I-35 decision 1 + arquitectura (§7.1) |
| OD-3.a | `PostTargets` del Selectivo | **RESUELTA** (tecnica) | gramatica de I-43 (§6.3) |
| OD-3.b | ¿DISTRIBUTE escribe `PostPeraltes`? | **RESUELTA** (tecnica): no | `PostPeralteAt` (§6.7) |
| OD-3.c | Altura / semilla de «Personalizar» | **RESUELTA**: cubierta por ADR-0032 D9/D10 | ADR aceptado (§6.5) |
| OD-4 | Cobertura del Dinamico | **RESUELTA** (tecnica): omitir e informar | ID7, ADR-0032 D1, I-35 decision 3 |
| OD-5 | L-1 / L-2 | **RESUELTA**: L-1 dentro (con companera); L-2 fuera | Arquitecto + Coordinador |
| OD-7 | Serializacion con I-50 | **RESUELTA** por WORKFLOW §7 + contrato; I-50 ya integrada | Coordinador (§11.1) |
| PA-1 | Realizacion de las tres integraciones | **RESUELTA: PA-1B** | Arquitecto + Coordinador (§10) |
| N-01 | Reapertura avanzada en el Selectivo | **RESUELTA: A** | Arquitecto + Coordinador (§6.11) |
| RR-01 | Frescura de la resolucion leida | **RESUELTA: incorporada** | Arquitecto (§3.8) |
| OD-2.b | Reconciliacion en la reconstruccion del Dinamico | **OWNER APPROVED** | Owner (§1.3) |
| OD-6 | Particion | **OWNER APPROVED: B′** | Owner (§1.3) |
| OD-8 | Presets «Personalizada N» | **OWNER APPROVED: retirar** en G7 | Owner (§1.3) |
| ADR-0037 | Reutilizacion por copia y distribucion por lotes | **OWNER ACCEPTED** | Owner (§15) |

**No queda ninguna decision del Owner pendiente.** `requires_owner_decision = false`.

## 17. Follow-ups y hallazgos laterales

- **Registrados en [`docs/ideas-futuras.md`](../ideas-futuras.md) en G2-F**, con la evidencia y la clasificacion del
  Discovery: L-2..L-6, L-8..L-14, **N-02** («Calculada» regenera la configuracion y tambien borra la longitud manual:
  `RackDynamicSystemWindow.xaml.cs:1879`) y **N-03** (el desplegable de configuracion del Dinamico muestra siempre
  «Calculada» al seleccionar un modulo, aunque sea personalizado: `UpdateSelectedPanel` → `SelectConfigCalculated`,
  `:1823`, `:1844-1860`). L-1 y L-7 no son follow-ups: estan dentro de la linea I-53.
- **Para G6**: alinear el XML-doc de `DynamicRackModule.IsManualOverride` con ADR-0037, sin cambio de comportamiento.
- **Para E1-I**: la fila de I-53 en ROADMAP dice todavia que no existe registro de decisiones y conserva el estado del
  bootstrap; se actualiza en su cierre (momento 3), no en G2-F.

## 18. Riesgos

| Id | Riesgo | Mitigacion |
|---|---|---|
| R-01 | la reconciliacion cambia la reconstruccion del Dinamico | OD-2.b aprobada; D-14, D-20, D-21; E3-V |
| R-02 | con otro numero de modulos una personalizada queda `Removed` o `Incompatible` | se informa; nunca silencioso |
| R-03 | codigo de Application durmiente entre E1 y E3 | E1-INV-2 lo declara; I-53S e I-53D lo cablean |
| R-04 | alguien reclama I-53S o I-53D antes de tiempo, o las olvida tras integrar I-53 | condiciones de §9.2 y del contrato §12; el cierre de I-53 deja la linea abierta de forma explicita |
| R-05 | I-49 toca `RackSelectiveWindow.xaml.cs` cuando arranca I-53S | regla de archivo caliente en el preflight de G0 de I-53S (§11.3) |
| R-06 | un recompute diferido deja a PREPARE con un sistema viejo | RR-01 (§3.8, §3.11), S-32 |
| R-07 | L-2 sigue en Push Back y alguien compara BOMs | follow-up registrado (§17) |
| R-08 | censos y guardas de I-35 tentados a relajarse | se reapuntan en el gate del cambio, con justificacion (D-39) |
| R-09 | colision de numero de ADR antes de llegar a `main` | protocolo de §11.4 |

## 19. Lo que no decide este contrato y queda fuera

Nombres definitivos de tipos (G3); disposicion de controles WPF (G5, G7); reset por lotes; aplicar entre racks del
dibujo; unidad por instancia en el Dinamico; cualquier cambio de Push Back; L-2; migracion de documentos;
`SchemaVersion`; catalogos; Plugin; la forma del control «Calculada» tras retirar los presets (G7).

---

## Anexo A — Registro de la re-revision del Arquitecto (G2C, cerrada)

> El paquete acotado que V2 publico en G2B se consumio en G2C. Se conserva aqui el veredicto, no el cuestionario.

| Punto revisado | Veredicto |
|---|---|
| Objeto | Proposal V2 @ `d7f17addb47ea943b61ff50b4b4a5b23010d6fab`; `main` @ `46fcac2`; I-50 @ `c9a5f0a` sin merge |
| H-01..H-07 | CLOSED |
| M-01..M-05, M-09 | CLOSED |
| AM-1-V2 / AM-2-V2 / AM-3-V2 / AM-4-V2 | AGREED / AGREED / AGREED / AGREED (AM-4 con RR-01 como precondicion) |
| N-01 | **A**: real; gate G5 (I-53S); seam; S-31; sin decision del Owner |
| RR-01 | hallazgo nuevo, MEDIUM: frescura del sistema resuelto que lee PREPARE en el Selectivo |
| PA-1 | P-A viola WORKFLOW → **PA-1B**; `OWNER_REQUIRED_FOR_PA1 = NO` |
| ADR-0037 | AGREED WITH REQUIRED CHANGES (tres correcciones); puede pasar a aceptado en el freeze con aceptacion expresa del Owner |
| E1 | integrable; E1-V = veredicto del Owner + smoke corto en AutoCAD 2025 |
| E2/E3 | separacion correcta; orden E2 → E3 |
| Decisiones del Owner pendientes | ninguna nueva; un acto formal (aceptar ADR-0037), cumplido en G2-F |
| Global | **`ARCHITECT_V2 = AGREED`**; autoriza solo G2-F documental, sin abrir G3 en la misma ejecucion |
