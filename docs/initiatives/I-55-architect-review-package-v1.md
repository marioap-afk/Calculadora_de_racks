# I-55 — Paquete de revision del Arquitecto (Proposal V1)

```text
PROPOSAL V1 — NOT CONSENSUS
Coordinator    = REVIEW REQUIRED
Architect      = REVIEW REQUIRED
Consensus      = NOT REACHED
Implementation = BLOCKED

Objeto de la revision  = docs/initiatives/I-55-proposal-v1.md @ d1918ab9a44ce7ae391aeacc10ff6686c8807a61
Misma version del plan = docs/initiatives/I-55-implementation-map-v1.md · docs/adr/0042-preparacion-de-vistas-antes-de-materializar.md
Base del codigo citado = dad4e77f4f267b9fa248ecb0c8bfd8a74bbab093
Paralela cruzada       = I-52 Proposal V10 @ 636f7fd (origin/feature/rackmirror-espejo-semantico)
```

> **Que es este paquete.** El material autonomo para revisar la Proposal V1 sin reconstruir la sesion. El ejecutor **no**
> emite ni simula el veredicto del Arquitecto y **no** declara consenso.

## 1. Veredicto que se solicita

```text
Architect: AGREED | AGREED WITH CHANGES | CHANGES REQUIRED
Hallazgos: BLOCKER | HIGH | MEDIUM | LOW — seccion, evidencia archivo:linea, cambio exigido
Version:   el veredicto vale solo para el SHA exacto revisado
```

## 2. Resumen tecnico

- **Frontera (D-01).** Preparacion pura en Application (soporte → codec → plan desde el sistema resuelto → sobre → nombre) y
  colocacion en el Plugin. Builders intactos.
- **Tipo y variante (D-02, D-03).** `DimensionViewKind` → `RackViewKind`. Variante tipada semantica. Un codec con disposicion
  `Canonical | Canonicalizable | Coerced | Invalid`. Persistencia sin cambios.
- **Identidad (D-05).** `RackId` al aceptar la intencion; un acunado por intencion tambien en comandos sin sesion; curacion de
  `Id` en blanco conservada en edicion; `Id` interior de Cantilever nuevo alineado en el prerrequisito PR-3.
- **Lote (D-06, D-15).** PREPARE ALL; commit por colocacion; en edicion preparar → redibujar → si un redibujo falla, ninguna
  vista nueva.
- **Hermanas (D-07).** Sin bloqueo por propiedades personalizadas (la coleccion nueva es la de un miembro); ID19 exige authored
  unico sobre todas las hermanas del barrido.
- **Proyeccion (D-08, D-13, D-16, D-17).** Grupos por `RackId` con referencias de una definicion; clasificacion con fallos
  cerrados; **transformacion comun en el marco fisico** (corrida, profundidad, altura) mediante descriptores de marco por vista
  caracterizados en G3; salidas bloqueadas por kind; faltantes de biblioteca antes de escribir; una transaccion.
- **Entre iniciativas (D-14).** X-1..X-6 con I-52.

## 3. Preguntas para el Arquitecto

### A-1 — Variante tipada y codec con disposicion (D-03; §7.2-§7.3)

- **Propuesta.** Variante sellada y codec por kind que compone `PushBackSystemFrontalBuilder.EncodeSection/DecodeSection/IsValidSection`
  (`A/Systems/PushBack/PushBackSystemFrontalBuilder.cs:138-155`) y `CantileverViewPlanBuilder.SectionFor`
  (`A/Systems/Cantilever/CantileverViewPlanBuilder.cs:238-239`), reproduciendo las lecturas del Plugin: Selectivo (todo lo que no es
  lateral ni planta es frontal, incluido un `View` desconocido, `P/RackSelectivoCommands.cs:126`, `:168`), Dinamico (todo lo que
  no es planta ni frontal es lateral, `P/RackDinamicoCommands.cs:209-239`, `:392-393`), Push Back y Cantilever (descriptor invalido
  aborta, `P/RackPushBackCommands.cs:424-450`, `P/RackCantileverCommands.cs:525-540`), cabecera (`P/RackCabeceraCommands.cs:239-240`).
- **Lectores no migrados**, caracterizados en G3: `P/ProjectVariableMutationExecutor.cs:181-186`, `A/Persistence/RackListBuilder.cs:117-118`,
  `P/RackBlockFinder.cs:23-24`, `P/RackLayoutCommands.cs:71`.
- **Pregunta.** ¿Se acepta la variante cerrada y un codec con disposicion como autoridad unica compartida con I-52, en ubicacion
  neutral (`Views/`) y no en `Mirror/`?

### A-3 — Transaccion del lote (D-06, D-15; §11.3-§11.5)

- **Evidencia.** El jig abre y confirma su transaccion (`P/Drawing/BlockPlacement.cs:174-201`); la cancelacion borra la
  definicion (`:37-42`, `:64-76`); `SystemBlockWriter.CreateBlock` confirma la definicion antes del jig
  (`P/Systems/Shared/SystemBlockWriter.cs:18-40`); hoy un redibujo fallido se ignora (`P/RackSelectivoCommands.cs:178-183`;
  `P/RackCantileverCommands.cs:328-332`).
- **Pregunta.** ¿Se acepta la opcion C, el orden preparar → redibujar → colocar y que un redibujo fallido impida insertar incluso
  en la insercion de una sola vista?

### A-4 — Una taxonomia de tipo de vista (D-02; §7.1)

- **Propuesta.** Renombrar y mover `DimensionViewKind` (`A/Systems/Shared/DimensionViewPolicy.cs:6-16`); conservar
  `CantileverViewKind` (`A/Systems/Cantilever/CantileverViewPlanBuilder.cs:12-37`) con un mapeo, porque incluye `AdapterSection`.
- **Coste medido.** Seis archivos de produccion y seis de pruebas nombran el tipo; ADR-0035 no lo nombra.
- **Pregunta.** ¿Renombre, o nombre actual con neutralidad documentada?

### A-6 — Proyeccion ortografica por familia de marco (D-16; §12.4)

- **Problema.** Una traslacion comun de posiciones no preserva el layout entre planta y elevaciones: la planta de los sistemas de
  rack dibuja X = fondo e Y = frente (`A/Systems/Selective/SelectivePlantaBuilder.cs:16-19`; `A/Systems/Dynamic/DynamicSystemPlantaBuilder.cs:15-17`),
  `RACKLAYOUT` pone filas en X y columnas en Y (`A/Layout/WarehouseGridPlanner.cs:72-79`), y el Cantilever proyecta con
  `right = up × forward` (`A/Geometry/Spatial3D.cs:186-197`; camaras en `CantileverViewPlanBuilder.cs:209-219`), es decir, con la
  corrida en −X en planta y frontal.
- **Propuesta.** Descriptor por `(sistema, vista, variante)`: que eje fisico representa cada eje local y donde cae el origen
  fisico, con desplazamientos calculados desde el sistema resuelto (`anchorOffset` de un corte de esquina,
  `A/Systems/Selective/SelectiveLateralBuilder.cs:82-97`; cuadricula propia de la frontal de un fondo frente a la maestra,
  `A/Systems/Selective/SelectiveFrontalBuilder.cs:50` y `A/Systems/Selective/SelectiveDepthLayout.cs:52-55`). Ancla = transformacion
  completa de la referencia aplicada al origen fisico local. `T_common : l ↦ T + l·w` sobre el eje conservado; eje comun alineado;
  eje descartado colapsado con aviso. Pares V1: planta ↔ frontal, planta ↔ lateral. Una familia por proyeccion y orientacion comun.
- **Proteccion.** `RackViewFrameCharacterizationTests` (G3) mide los descriptores sobre la salida de los builders; un par
  inconsistente queda fuera de ID19 hasta V2; nunca se ajusta el descriptor para que pase.
- **Preguntas.** ¿Se acepta que los descriptores sean metadatos de los builders con caracterizacion y no una segunda autoridad
  geometrica? ¿Se acepta excluir en V1 la mezcla de familias, las orientaciones distintas y los pares frontal ↔ lateral?

### D-05 — Ciclo de vida del `RackId` (§6)

- **Evidencia.** `U/Editor/RackEditorSession.cs:126`; `U/Editor/RackEditorIdentity.cs:45-53`; la Cama tiene sesion
  (`U/Systems/FlowBed/RackFlowBedWindow.xaml.cs:36-37`, `:276-278`); la cabecera acuna en cada `DrawAndPlace`
  (`P/RackCabeceraCommands.cs:164-176`); curacion de `Id` en blanco (`P/RackSelectivoCommands.cs:119`, `P/RackDinamicoCommands.cs:174`,
  `P/RackPushBackCommands.cs:179`, `P/RackCantileverCommands.cs:230`); `Id` interior del Cantilever (`D/Systems/Cantilever/CantileverLineDesign.cs:333`).
- **Pregunta.** ¿Se acepta B, un acunado por intencion en comandos sin sesion y PR-3 como gate aislado previo?

### D-07 — Autoridad entre hermanas (§9)

- **Por que no un gate de propiedades personalizadas.** `RackCustomPropertiesAuthority` devuelve `IndeterminateMembership` para
  todo rack si hay un payload ilegible en cualquier parte del dibujo (`A/CustomProperties/RackCustomPropertiesAuthority.cs:249-264`) y
  `NoIdentity` para un sobre sin `Id` (`:238-247`); bloquear por eso contradice OQ-7 y la curacion vigente, sin evitar ningun valor
  nuevo (`Compose` hereda la coleccion del sobre fuente, `A/Persistence/RackEmbedComposer.cs:57`). Ademas, ningun archivo del Plugin
  fuera del borde puede nombrar la autoridad (`TGrd04`).
- **ID19.** Miembros = todas las hermanas del barrido; Selectivo `SelectiveAuthoredAuthority.Resolve` (`A/ProjectVariables/SelectiveAuthoredAuthority.cs:128-157`);
  demas kinds, comparador de X-3 que excluye la metadata interior de `P/RackCommandSupport.cs:40-67`.
- **Pregunta.** ¿Se acepta que en edicion la equivalencia authored se garantice por el flujo vigente mas D-15, y que ID19 falle
  cerrado ante authored divergente?

### D-08 — Grupos y clasificacion de ID19 (§12.2-§12.3)

- **Hechos.** `RACKLAYOUT` enlazado coloca referencias de una definicion (`P/RackLayoutCommands.cs:29-30`, `:226-257`); el barrido
  omite definiciones anonimas (`P/RackBlockFinder.cs:66`); `RACKEDITAR` resuelve el kind sensible a mayusculas
  (`P/RackMenuCommands.cs:141`) y `RACKLAYOUT` no (`P/RackLayoutCommands.cs:64`).
- **Propuesta.** Una definicion nueva por grupo y una referencia por referencia seleccionada; fallo cerrado ante MINSERT, referencia
  dinamica/anonima/anotativa con datos, escala no uniforme o reflejada, normal o UCS no universal (alineado con las ST de I-52);
  `Origin ≠ 0` resuelto por la transformacion completa; kind con `TryResolve`.
- **Pregunta.** ¿Hay un caso de seleccion que falte o que I-55 deba tratar distinto de I-52?

### D-10, D-13 y D-17 (§12.5, §12.7, §14)

- **D-10.** Hoy la excepcion deja la definicion en `PlaceAndReport` (`P/Drawing/BlockPlacement.cs:49-52`) y en la ruta del Cantilever
  (`P/RackCantileverCommands.cs:151-157`, `:174-177`); se limpian ambas. No es observable por el Owner: evidencia = guarda.
- **D-13.** ID19 re-verifica bloques tras importar y falla antes de escribir; ID17/ID18 conservan el reporte
  (`P/Drawing/BlockPlacement.cs:98-118`).
- **D-17.** ID19 aplica los diagnosticos bloqueantes por kind (`A/Bom/RackBomOutputGate.cs:46`; `P/RackInventarioCommands.BomTotal.cs:174-188`)
  y G3 caracteriza la paridad entre la resolucion sin editor (`P/KindHandlers/SelectiveKindHandler.cs:58-68`,
  `P/KindHandlers/DynamicKindHandler.cs:40-42`) y el sistema del editor.
- **Pregunta.** ¿Se aceptan la asimetria de D-13, la evidencia por guarda de D-10 y la regla de paridad de D-17?

### X-1..X-6 — Autoridades y coordinacion con I-52 (§17.2)

| # | I-52 V10 | I-55 V1 | Pregunta |
|---|---|---|---|
| X-1 | Nucleo neutral + fachada (§5.1); fallos ST de seleccion (§4.1) | ID19 consume el nucleo | ¿Una extraccion; hechos neutrales en el nucleo y politicas por llamador; semantica observable verde sobre codigo intacto y roja por mutacion? |
| X-2 | `RackViewPlanAuthority` recibe el payload y resuelve (§8.1); sin laterales de Selectivo, Dinamico ni Push Back; decodificacion en `Mirror/` | los editores no re-resuelven (`P/RackSelectivoCommands.cs:421-424`); ID19 resuelve; necesita laterales | ¿Resolucion separada + un paso comun «sistema resuelto → plan» con todas las direcciones + un codec neutral? |
| X-3 | Comparador por kind del reflector, sobre las vistas seleccionadas (§5.4) | ID19 sobre todas las hermanas | ¿Comparador declarado por el contrato del kind, con los miembros como parametro? |
| X-4 | Materializador con ST-13, ST-19, PRE-17, PRE-18 y read-set (§8.2, §8.7, §9) | definicion + sobre + referencia sin equivalencia visual | ¿Primitivo comun y politicas de equivalencia visual solo en I-52; puntos antes de importar en ambos? |
| X-5 | STOP (§19) y NO TOCAR (§20.3): `RackDuplicationPlan.cs`, builders de §8.1, editores WPF, `KindHandlers/*`, drawers, `CustomPropertiesExecutor.cs` | I-55 toca editores WPF, `RackDuplicationPlan.cs` si extrae y renombra en `SelectiveDimensions.cs`/`DynamicViewDecorations.cs` | ¿Reporte previo a I-52 (su §15.3) y secuencia? |
| X-6 | C2-2a, C2-2b, C2-4 y G-M9 (§8.5) | G9 re-enruta Insertar; G12 reordena la edicion y aplica D-15 | ¿La segunda en integrar re-establece C-2 sobre el arbol combinado, con fixtures de Insertar en G9 de I-55? |

## 4. Testabilidad (I-45) y limites

| Regla nueva | Donde vive | Evidencia |
|---|---|---|
| Soporte, disponibilidad, variante canonica, pares | Application | `RackViewSupportMatrixTests` |
| Codec y disposicion | Application | `RackViewEnvelopeCodecTests` + caracterizacion compartida de lecturas |
| Descriptores de marco | Application | `RackViewFrameCharacterizationTests` (G3) y `RackViewFrameTests` |
| Preparacion y payload | Application | `PreparedViewPayloadGoldenTests` |
| Identidad | UI + Application | `RackIdLifecycleTests`, `CantileverNewRackIdentityTests` |
| Lote, mensajes, `REDRAW_FAILED` | Application | `RackViewBatchPlanTests` |
| Autoridad authored | Application | `RackViewAuthoredEquivalenceTests` |
| Grupos, clasificacion, proyeccion | Application | `RackProjectionPlanTests`, `RackProjectionTransformTests` |
| Re-enrutado, limpieza, driver, rutas de entrada, comando | Plugin / UI | Guardas minimas con teorias de mutacion + validacion del Owner |

Las pruebas usan el namespace plano `RackCad.Tests`/`RackCad.UI.Tests`: filtros por nombre de clase con conteo; ninguna prueba de UI
recorre rutas que abran modales reales.

## 5. Censos y guardas que la implementacion reapuntaria con motivo

`CustomPropertiesEnvelopeGuardTests.TGrd02_*` (siete llamadas por archivo; el primer argumento debe ser un parametro
`RackEmbedDocument` del miembro); `SelectiveEditorOpenTests.GUARDA_EL_CENSO_DE_COMANDOS_NO_CAMBIA` (35) y
`CustomPropertiesCommandGuardTests.TGrd08_*` (censo por nombre); `CustomPropertiesHelpCensusTests`; `WindowCensusGuardTests` y
`DialogWindowCharacterizationTests`; `DynamicHeaderBatchSeamGuardTests.D34_GUARD_*` (su `MessageBox` es la puerta de primera
vista) y `SelectiveHeaderBatchSeamTests.S30_GUARD_*`; `SelectiveAuthoredCarrierAdoptionTests.GUARDA_LaInsercionNUEVA_CONSERVA_From`;
`PushBackPluginSourceGuardTests`; `RackUnitsGuardSourceTests`; `CantileverPluginSourceGuardTests.ExactlyOneRegenForTheWholeBatch_AndItAgreesWithWhatIsReported`;
`RackInsertionRequestTests`, `RackEditorSessionTests`, `EditorModuleRegistryTests`. Sin cambio: `CustomPropertiesEdgeGuardTests.TGrd04_*`
y `TGrd05_*`.

## 6. Lo que no se pide al Arquitecto

Decisiones de producto y UX (OD-1..OD-8, al Owner via Coordinador), aceptacion de ADR-0042 (Owner) y prioridad frente a otras
iniciativas (Coordinador).

## 7. Riesgos tecnicos principales

R-01 regresion al re-enrutar inserciones; R-03 autoridades de I-52 incompatibles (→ Proposal V2); R-06 rendimiento de ID19; R-08 D-10
no observable; R-10 y R-11 descriptores de marco frente a cambios o inconsistencias de los builders (mapa §R).

## 8. SHA revisado

```text
PROPOSAL_V1_SHA = d1918ab9a44ce7ae391aeacc10ff6686c8807a61
Archivos         = I-55-proposal-v1.md, I-55-implementation-map-v1.md, ADR-0042, indice de ADR y registro de decisiones
```
