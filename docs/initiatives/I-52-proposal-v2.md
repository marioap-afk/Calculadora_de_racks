# I-52 — Proposal V2: RACKMIRROR, espejo semantico de uno o varios racks (ID16)

> # ⚠ PROPOSAL V2 — NOT CONSENSUS
>
> # ⛔ Implementation remains BLOCKED
>
> ```text
> Proposal Version = V2
> Coordinator      = REVIEW REQUIRED
> Architect        = REVIEW REQUIRED
> Implementation   = BLOCKED
> ADR-0036         = PROPOSED
> ```
>
> ```text
> Schema           = SIN CAMBIO (ningun DTO, sobre ni Xrecord cambia en I-52)
> BASE productiva  = 46fcac2b071929d2bd5b07aa28373941417f74a8   (base auditada de la rama; todas las citas archivo:linea son de este arbol)
> origin/main      = f8deb675c6d1ef0e64693b157d69c4cc170d7b24   (avanzo durante este gate: merge de I-50; registrado, SIN rebase; §15.1)
> CLAIM_SHA        = 55281769d5e9317ad25500e6d3f5f8a849f37279
> BOOTSTRAP_SHA    = 7ee79756d2b93b4eded660cb73310ef3edde32ff
> Discovery G1     = 339b3abd238a3a7a42d60c9edd45a1500a776f62   (I-52-discovery.md; se conserva)
> Proposal V1      = 0fc7032bf15d03e7d478bbd9350f156708621c9d   (I-52-proposal-v1.md; se conserva; CI 34731908035 4/4)
> Revision Arq. V1 = «Architect: CHANGES REQUIRED — PROPOSAL V2»; sin BLOCKER; HIGH AR-01..AR-03,
>                    MEDIUM AR-04..AR-13, LOW AR-14..AR-19 (registro: docs/automation/decisions/I-52.md §8)
> Coordinador      = V1 «ACCEPTED FOR ARCHITECT REVIEW»; tras la revision, V2 obligatoria con V2-01..V2-15
> Paralelas        = §15.1 (tips del preflight de V2)
> Estado de gates  = G0 ACCEPTED · G1 ACCEPTED (Discovery) · G2 V2 EN REVISION · G3+ NO INICIADO
> ```
>
> Este documento **no** autoriza produccion, **no** modifica V1 ni el Discovery y **no** es el contrato vinculante. El
> contrato se reescribe en el freeze de G2, cuando Coordinador y Arquitecto acuerden la **misma** version y el Owner
> acepte el alcance (O-1) y el ADR (O-4).
>
> **Nota de transparencia.** V1, su revision de Arquitecto y esta V2 las redacto el mismo agente en roles distintos.
> La revision verifico en codigo a SHA exacto; V2 re-verifica cada correccion, declara tres hallazgos nuevos
> encontrados al redactarla (`[V2-D23]`, `[V2-D24]`, `[V2-D25]`) y precisa una afirmacion de la propia revision
> (`[V2-D16]`).

---

## 0. Reconciliacion V1 → V2

V1 (`0fc7032`) y el Discovery (`339b3ab`) **se conservan sin cambios**. **Donde V1 y V2 difieran, manda V2.** Cada
marca cita el hallazgo de la revision (`AR-nn`), el cambio exigido por el Coordinador (`V2-nn`) y la seccion de V2
donde queda la version vigente.

| Marca | Origen | Afirmacion de V1 | Correccion V2 | Donde |
|---|---|---|---|---|
| `[V2-D01]` | AR-01 · V2-01 | §7.1: S-13 y S-13b **R** | El tope frontal del Selectivo se ancla al troquel del poste **izquierdo** (`SelectiveFrontalBuilder.cs:220-222`) con `LONGITUD = larguero + 0.25` (`SelectiveSafetyPlacement.cs:55`; `SelectiveTopePlacement.cs:27`); frontal y planta usan segmentos `(StartOffset, largo + 0.25)` (`SelectiveTopePlan.cs:140`, `:148`, `:262`, `:270`). La holgura queda siempre a la derecha: el espejo geometrico la lleva a la izquierda y el builder regenerado la vuelve a poner a la derecha. **S-13/S-13b = UNKNOWN por defecto**; un rack que usa topes falla cerrado antes de mutar; no se inventa ninguna transformacion de la holgura; decision futura del Owner (O-2) | §2.3, §7.1, §18.2 |
| `[V2-D02]` | AR-02 · V2-02 | §6.1 RF-2 «documento del mismo kind con su store» y RF-6 «dominio solo con prueba» | Solo el Selectivo lleva un DTO authored en el sobre (`SelectivePalletDesignStore`, `RackSelectivoCommands.cs:153`). Dinamico, Push Back, Cantilever y cabecera se leen con `RackProjectStore.Deserialize` → `RackProject` (dominio) y se escriben con `RackProject.For*` + `WithSourceMetadataFrom` (`RackDinamicoCommands.cs:379`; `RackPushBackCommands.cs:414`; `RackCantileverCommands.cs:490-491`; `RackCabeceraCommands.cs:194`). V2 declara el sustrato real por kind y sustituye la prohibicion de pasar por dominio por la regla «fidelidad no peor que `RACKEDITAR` → Actualizar», medida por V-RT | §6.2, §6.3 |
| `[V2-D03]` | AR-03 · V2-03 | §7.1 S-06 / N-S06: rellenar con el `PostPeralte` resuelto | El marcador authored de herencia es `0.0`: lo escribe el editor (`SelectiveEditorState.cs:744`) y el resolvedor trata `≤ 0` o ausencia como «usa el global» (`SelectiveGeometryResolver.cs:88-92`). N-S06 rellena con `0.0`, permuta los valores tal cual y conserva la cola ignorada; nunca materializa el global | §6.5.2, §7.1 |
| `[V2-D04]` | AR-03 · V2-03 | S-02 RN sin declarar el cambio authored | N-S02 vuelve explicito el `Length` del nuevo primer tramo y reescribe el `Length` almacenado e ignorado del ultimo (`SelectiveMedioFrente.cs:70`): cambia la representacion authored aunque conserve la fisica. Declarado; CT-21 de UI, BOM, round-trip e involucion. Un medio frente que hoy no resuelve (`Resolve` devuelve `null`) ⇒ UK | §6.5.1, §7.1 |
| `[V2-D05]` | AR-03 · V2-03 | H-02 RN (Auto) sin declarar la perdida de intencion | N-H02 convierte `AutoAlternating` en la direccion efectiva explicita (`BracingPanel.cs:40-48`) y cambia la intencion authored. Declarado; CT-23 de fisica, BOM, UI, round-trip y relacion con `IsStandard`/`IsException`/`StandardBaselineId` (H-06 UK). Valor fuera de `{0, 1, 2}` ⇒ UK | §6.5.4, §7.6 |
| `[V2-D06]` | AR-04 · V2-04 | §16: «gana la que integre primero»; renumerar el propuesto en el rebase | Precedencia por la **primera publicacion observable en un ref remoto**. I-52 publico 0036 en `0fc7032` (CI de push `34731908035` creado `2026-09-13T01:59:44Z`). Seis reglas; un ADR `aceptado` no se renumera; dos aceptados con el mismo numero ⇒ STOP + Owner | §16 |
| `[V2-D07]` | AR-05 · V2-05 | §3.1 `P = T(p)·R(θ)·S(s,s)`; §4.1 sin `Origin` | La transformacion efectiva es `T(p)·R(θ)·S(s,s)·T(−o)`, con `o = BlockTableRecord.Origin`. RackCad crea sus definiciones con `Origin = 0` (`LateralHeaderDrawer.cs:243`; `CantileverViewMaterializer.cs:47`), pero `RackCloner` copia el de la fuente (`RackCloner.cs:34`) y BASE/BEDIT puede moverlo. **ST-14**: `Origin ≠ 0` ⇒ fail-closed; sin soporte de BASE movido | §3.1, §4.1 |
| `[V2-D08]` | AR-06 · V2-06 | §3.7 «`τ` es la identidad»; S-19, D-14, P-09, PC-10, K-10 y H-07 identidad | `SectionRaw → seccion semantica → seccion canonica` por kind, con la autoridad de produccion citada. El legado inequivoco y soportado se canoniza (Selectivo frontal `−1 → 0`; plantas y cabecera `→ −1`). Lo no inequivoco falla cerrado (Selectivo `< −1` o fondo inexistente; Dinamico frontal fuera de `{0, 1}`; Push Back fuera de rango o lado B en rack simple; Cantilever frontal/planta `≠ −1`). *Precision:* Push Back y Cantilever ya **rechazan** esas secciones en `RACKEDITAR` (`RackPushBackCommands.cs:427-450`; `RackCantileverCommands.cs:530-540`); el builder de Push Back decodifica en silencio, pero el comando valida antes. Una `View` no vacia y no reconocida falla cerrado, mas estricto que el Selectivo y la cabecera de produccion | §3.7, §4.1 |
| `[V2-D09]` | AR-07 · V2-07 | §9.4 «definiciones auxiliares»; E10 | `EnsureBlocks` clona con `WblockCloneObjects(..., DuplicateRecordCloning.Ignore, ...)` fuera de transaccion (`BlockLibraryImporter.cs:110`), arrastra dependencias, captura toda excepcion y devuelve 0 (`:113-118`); biblioteca ausente o ilegible ⇒ 0 sin error (`:74-78`, `:149-161`). `CreateSystemBlock` **omite** las piezas sin bloque (`LateralHeaderDrawer.cs:293-303`). V2: PREPARE re-verifica y registra faltantes; `MissingInstances.Count > 0` en MUTATE ⇒ excepcion en el camino de I-52, sin tocar el drawer; solo atomicidad **semantica** del rack; UNDO de importaciones UNKNOWN | §8.2, §9 |
| `[V2-D10]` | AR-08 · V2-08 · DA-1 | §8.5: C-1 preferida y C-2 de reserva; el ejecutor de variables «converge en la misma decision» | **C-2 por defecto**, contra los cuatro productores (autoridad nueva, `EditX`, `ProjectVariableMutationExecutor` del Selectivo y estado del editor → sistema), en CI y con la guarda G-M9 sobre las invocaciones de builders de `EditX`. C-1 solo con evidencia | §8.5 |
| `[V2-D11]` | AR-09 · V2-09 | §11: firma sin capas de rol, BOM exacto, simetria sin eje ni centro, parametros solo numericos | Firma completa: posicion, rotacion, escala, `MirroredX/Y`, parametros, curvas, rol de capa Cantilever, BOM, mano y anotaciones por contrato. BOM con la clave de consolidacion de produccion (`ConsolidatedBom.cs:62-64`, `:110-115`). Declaracion tipada unica de simetrias con eje local **y centro** (constante o mitad de un parametro de estiramiento). Parametro con semantica de lado sin regla ⇒ fail-closed. Las holguras graficas no se asumen simetricas | §11 |
| `[V2-D12]` | AR-10 · V2-10 | §7: filas **R** presentadas como probadas | **R** (estructural, sin anclaje propio) frente a **R-s** («R static / verification required»: depende de anclaje a poste, troquel, extremo, offset grafico o bloque con mano). Ninguna fila esta verificada en V2; G3 audita anclajes (CT-17); una contradiccion abre V3 | §7, §12.1 |
| `[V2-D13]` | AR-11 · V2-11 | §5.1: SNAPSHOT compartido desde el primer gate; G7 migra RACKDUPLICAR | SNAPSHOT **propio** de RACKMIRROR. Extraer uno compartido es opcional y exige beneficio claro y equivalencia demostrada. Si RACKDUPLICAR se toca: guardas en RED primero, Core completo, regresion especifica de I-51 y validacion manual del Owner en el Candidato (M-17) | §5.2, §14 G7 |
| `[V2-D14]` | AR-12 · V2-12 | T-M04 fija los mensajes historicos en G4 | **CT-16 en G3** fija los textos completos de los mensajes visibles de `RackDuplicationPlan` sobre codigo intacto; hoy las pruebas usan `Contains` (`RackDuplicationPlanTests.cs:333`, `:344`, `:401`, `:432`, `:509`, `:525`). G4 demuestra igualdad byte a byte de la fachada | §5.1, §12.1, §14 |
| `[V2-D15]` | AR-13 · V2-13 | ADR-0036: decisiones 6, 8, 11 y 12 y nota de numeracion | ADR corregido: sustrato por kind, `Origin = 0`, seccion canonica, importacion best-effort con dependencias, politica de simetria y mano, verificacion dinamica por rack y protocolo de numeracion. Sigue `propuesto` | ADR-0036 |
| `[V2-D16]` | AR-14 · V2-14 | E7/PV-5: registro ilegible ⇒ fallo de toda la operacion | Criterio exacto de la autoridad efectiva: el registro solo se acredita para los grupos cuyo kind lo consume. Hoy solo el Selectivo, cuya autoridad **exige** un registro acreditable aunque el rack no tenga vinculos (`SelectiveEffectiveDesignResolver.cs:52-66`; `UsableProjectVariablesRegistry.cs:120-135`; `SelectiveEditorOpen.cs:184`; `LinkedPropertyReconciler.cs:131-139`; `RackInventarioCommands.BomTotal.cs:56-67`). Dinamico, Push Back, Cantilever y cabecera lo ignoran (`DynamicKindHandler.cs:23-25` y equivalentes). *Precision de transparencia:* AR-14 hablaba de «racks sin vinculos», pero el criterio real es por kind consumidor. Relajarlo para Selectivos sin vinculos seria una regla nueva, distinta de `RACKEDITAR` y `RACKBOMTOTAL`, y V2 no la adopta | §9.4, §10.2 |
| `[V2-D17]` | AR-15 · V2-14 | §8.1: la solicitud de plan no lleva nombre | La autoridad de plan recibe el nombre logico destino. La anotacion de nombre la dibuja `system.Name` (`SelectiveFrontalBuilder.cs:342`; `DynamicViewDecorations.cs:83`, `:147`, `:246`), que los comandos fijan (`RackSelectivoCommands.cs:121`, `:176`; `RackDinamicoCommands.cs:177`; `RackPushBackCommands.cs:182`; `ProjectVariableMutationExecutor.cs:166-168`, `:236-238`) | §8.1 |
| `[V2-D18]` | AR-16 · V2-14 | §9.1: huella solo de payloads | Huella por referencia fuente: handle, no borrada, `BlockTableRecord`, `Position`, `Rotation`, `ScaleFactors`, `Normal`, capa, `Origin` de la definicion y hash del payload. Si cambia ⇒ fail-closed antes de MUTATE | §9.2 |
| `[V2-D19]` | AR-17 · V2-14 | ST-7: «tolerancia relativa» sin valor | No hay tolerancia relativa: la autoridad (`GeometryTolerance`, `Vector2D.cs:82-100`) es **absoluta** y rechaza el epsilon relativo por diseño. V2 lista las tolerancias existentes; la de factores de escala (adimensional) no existe y la fija G4 | §4.3 |
| `[V2-D20]` | AR-18 · V2-14 | §15.2: lista de I-50 incompleta; I-54 en V1 | Tips del preflight de V2; lista completa de I-50 (incluye `DynamicRackSystemResolver.cs`, `DynamicEditorDesignAssembler.cs`, `PushBackEditorState.Load.cs`, `SelectiveDimensions.cs`, `DynamicViewDecorations.cs` y `DimensionViewPolicy.cs`); I-54 V2 D-21 (cumple) y T-GRD-02 7 → 8; I-49 V5 §12.3 | §15 |
| `[V2-D21]` | AR-19 · V2-14 | G3 y G8 sin `tests/RackCad.UI.Tests` | Permitido en G3 y G8 cuando la caracterizacion o C-2 lo exija | §14 |
| `[V2-D22]` | V2-15 | §18: DA-1..DA-6 abiertos | Registrados segun la orden del Coordinador para V2; O-1..O-4 separan lo que el Owner aun decide | §1.2, §18 |
| `[V2-D23]` | nuevo, al redactar V2 | §7.1 S-15: «distribucion simetrica» | `AppendRow` reparte con `gap = max(0, (span − count·frente)/(count+1))` (`SelectiveParrillaPlacement.cs:40-54`). Con `count·frente > span` el hueco es 0 y las parrillas se apilan desde la izquierda. El conteo manual se acota (`SelectiveFrontalBuilder.cs:299-308`), pero el conteo por defecto de un claro completo usa `palletCount` sin acotar (`:310`). S-15 pasa a **R-s condicional**; una fila desbordada ⇒ UK | §2.3, §7.1 |
| `[V2-D24]` | nuevo, al redactar V2 | §7.3 P-04/P-05 y §7.4 PC-08 **R** | El tope posterior de Push Back tambien lleva `LONGITUD = larguero + 0.25`, anclado al troquel del poste y con la mano del larguero (`PushBackSystemFrontalBuilder.cs:285-300`; `PushBackSystemPlantaBuilder.cs:134-154`; `PushBackRearTopeBuilder.cs:291-306`). No esta demostrado asimetrico, pero sigue el patron de AR-01 ⇒ **R-s con riesgo AR-01** y CT-15 | §7.3, §7.4 |
| `[V2-D25]` | nuevo, al redactar V2 | §5.4 y §9.1: ensayo de restamp dentro de un PREFLIGHT «puro» | `RackEnvelopeRestamp` vive en el **Plugin** y despacha el restamp interior por `KindHandlerRegistry` (`src/RackCad.Plugin/RackEnvelopeRestamp.cs:52-111`). El ensayo lo ejecuta el comando, sin transaccion ni mutacion; el plan puro de Application termina en el documento compuesto | §5.3, §5.5, §9.1 |
| `[V2-D26]` | precision | §7.2 D-16 | El store siempre reconstruye `DynamicDesign` con `ToDesign()`, que rellena con defectos los campos ausentes (`SystemRegistry.Default.cs:74-88`; `DynamicRackSystemDocument.cs:184-199`), y `RACKEDITAR` los escribe explicitos. Es parte de la linea base de round-trip (CT-03), no una perdida causada por el espejo | §6.2, §7.2 |
| `[V2-D27]` | precision · V2-11 | §14 G4: «cualquier cambio observable de RACKDUPLICAR» | G4 edita `RackDuplicationPlan.cs` como fachada, y eso **es** tocar RACKDUPLICAR: se aplica V2-11 (guardas, Core, regresion de I-51) y M-17 en el Candidato, con aviso previo a I-49 | §5.1, §14 |
| `[V2-D28]` | precision · AR-07 | §11: equivalencia implicita contra la definicion fuente | El espejo es fiel al **plan regenerado** del documento fuente, no a las entidades actuales de su definicion, que pueden estar incompletas por bloques que faltaban al dibujarla. Es la misma semantica de `RACKEDITAR` → Actualizar | §11.1 |
| `[V2-D29]` | preflight de commit de V2 | V1 §15.1 y el preflight inicial de V2: `origin/main` en `46fcac2` | Durante este gate `origin/main` avanzo a `f8deb67` (**merge de I-50**) e I-53 publico Proposal V2 con **ADR-0037** `propuesto` (`d7f17ad`). Registrado **sin rebase**. Entre `46fcac2` y `f8deb67` no cambian `RackDuplicationPlan.cs`, `RackDuplicarCommands.cs` ni `RackEnvelopeRestamp.cs`; los archivos de autoridad que cambian solo reciben lineas de portador `DimensionViews` (§15.2). Las citas `archivo:linea` de V2 son del arbol `46fcac2`; algunas se desplazan en `main` | §15 |

---

## 1. Decisiones de producto

### 1.1 Decisiones del Coordinador vigentes (desde V1)

Registro: `docs/automation/decisions/I-52.md` §4. Vinculan V2; **no** son consenso final.

| # | Decision | Precision V2 |
|---|---|---|
| **PDC-1** | **Solo copia.** Crea copia reflejada, conserva **todos** los originales, nunca borra, nunca refleja en sitio, nunca conserva el `RackId` en la copia. **Sin** pregunta «¿Borrar objetos originales?» | — |
| **PDC-2** | Nombre logico por rack `<base> - espejo`, **sin ordinal**. `Name` ≠ `RackId`. Los nombres de `BlockTableRecord` siguen la politica de unicidad existente de cada familia de plan | La autoridad de plan recibe ese nombre para la anotacion (`[V2-D17]`) |
| **PDC-3** | Autoridad = referencias **fisicamente seleccionadas**. Sin buscar hermanas; sin crear vistas no seleccionadas. Vistas seleccionadas del mismo `RackId` = **un** grupo con **un** `NewRackId`. Una referencia no admisible **falla toda la operacion** antes de MUTATE | — |
| **PDC-4** | Sin prompt de eje semantico. El usuario solo da la linea (dos puntos) | — |
| **PDC-5** | Indices, numeros y letras derivados se **regeneran** desde el documento reflejado | — |
| **PDC-6** | Protector lateral Left/Right: **UNKNOWN** donde sea ambiguo; estado explicito cuya reflexion no este demostrada ⇒ fail-closed | — |
| **PDC-7** | Clases `REPRESENTABLE`, `REPRESENTABLE_BY_NORMALIZATION`, `REQUIRES_MODEL_CHANGE`, `UNKNOWN`, evaluadas **por rack concreto** en PREFLIGHT; `REQUIRES_MODEL_CHANGE` y `UNKNOWN` material ⇒ fail-closed | V2 distingue `R` estructural de `R-s` pendiente de verificacion (§7.0); una normalizacion declara el cambio de representacion authored (§6.5) |
| **PDC-8** | Unico comando nuevo `RACKMIRROR`, **sin alias**. Censo de `[CommandMethod(` **33 → 34** | — |
| **PDC-9** | Solo Model Space; MINSERT ⇒ fail-closed; linea por dos puntos UCS→WCS; fuente canonica | V2 añade `Origin = 0` (ST-14) y `View`/`Section` decodificables (ST-15) |
| **PDC-10** | Arquitectura para los seis kinds con bloque; el primer corte solo ejecuta vistas que exponen `μ_k`; la cama falla cerrado; sin ampliar schema | Aceptacion de producto del Owner **antes de G3** (O-1) |

### 1.2 Decisiones pendientes del Owner

| # | Decision | Default tecnico de V2 | Momento |
|---|---|---|---|
| **O-1** | **Alcance del primer corte.** Aceptar que **no** refleja laterales de racks (Selectivo, Dinamico, Push Back, Cantilever), ni la cama, ni Dinamicos materializados solo como lateral, y que los fail-closed adicionales de §2.3 son aceptables | recorte de §2 | **OWNER PRODUCT ACCEPTANCE REQUIRED BEFORE G3** |
| **O-2** | **Topes del Selectivo.** Mantener `UNKNOWN → fail-closed` o aprobar una regla explicita de normalizacion u holgura | `UNKNOWN → fail-closed` | cuando el Owner lo decida; no bloquea G2 ni G3 si el fail-closed es verificable antes de mutar |
| **O-3** | **Simetrias de bloques.** Confirmar el censo de bloques simetricos (eje y centro) segun la evidencia | ningun bloque se asume simetrico | censo en G3 (CT-06); confirmacion visual en G9 (M-15) |
| **O-4** | **ADR-0036.** Aceptarlo o rechazarlo | `propuesto` | **no** en este gate; primero pasa la revision de Arquitecto de V2 y el consenso |

---

## 2. Alcance del primer corte

### 2.1 Dentro

| Kind (`Kind` del sobre) | `μ_k` canonica | Vistas admitidas | Evidencia de la vista |
|---|---|---|---|
| Selectivo (`selective`) | **RUN**: invierte frentes/postes | frontal (`Section` = fondo), planta | frontal X = run (`SelectivePostGeometry.cs:32-40`); planta X = fondo, Y = frente (`SelectivePlantaBuilder.cs:15-22`) |
| Dinamico (`dynamic`) | **RT**: invierte frentes | frontal (`Section` 0 salida / 1 entrada), planta | `RackDinamicoCommands.cs:392-393`; `DynamicSystemFrontalBuilder`/`DynamicSystemPlantaBuilder` |
| Push Back simple y compuesto (`pushback`) | **RT** | frontal (`Section` 0..3), planta | `PushBackSystemFrontalBuilder.cs:137-155` |
| Cantilever (`cantilever`) | **R_X**: invierte la linea de estaciones | frontal, planta | camaras `CantileverViewPlanBuilder.cs:205-235` |
| Cabecera independiente (`cabecera`) | **R_D**: invierte la profundidad del marco | lateral, planta | `LateralHeaderLayoutBuilder.cs:49-59`; `PlantaHeaderLayoutBuilder.cs:13-21` |

### 2.2 Fuera (fail-closed o no aplica)

- Laterales de Selectivo, Dinamico, Push Back y Cantilever; la cama (`cama`) entera.
- Diseños clasificados `REQUIRES_MODEL_CHANGE` o `UNKNOWN` material (§7).
- Espejo en sitio, borrar originales, conservar `RackId`, prompt de eje, alias.
- «Vista observada desde el lado opuesto» (cambio de modelo; futuro, §3.9).
- Paper Space, MINSERT, fuentes no canonicas (§4), definiciones con BASE movida (`Origin ≠ 0`).
- Cualquier transformacion de la holgura de topes (O-2) y cualquier normalizacion de `IsStandard` (H-06).
- Cambios de schema, de `RackEnvelopeRestamp.cs`, de `PushBackMirror.cs`, de `WithDesign`, de los drawers existentes y del
  registro de variables.
- Drive-In (no existe: `RackMainMenuWindow.xaml:139-144`) y Larguero (sin bloque RackCad).
- Arreglar los hallazgos laterales del Discovery §24: se registran en `ideas-futuras.md` al cerrar G2.

### 2.3 Fail-closed adicionales conocidos al redactar V2

Reducen el alcance util del primer corte sin cambiar la arquitectura. Todos se detectan **antes** de mutar.

| Caso | Motivo | Fila |
|---|---|---|
| Selectivo que usa topes | holgura de 0.25" anclada a un lado (AR-01) | S-13, S-13b |
| Selectivo con una fila de parrillas desbordada (`count·frente > span`) | reparto apilado desde la izquierda (`[V2-D23]`) | S-15 |
| Cabecera con `AutoAlternating` en un panel con diagonal y `StandardBaselineId` no vacio | semantica de «estandar» sin caracterizar (H-06). Las plantillas crean exactamente eso (`RackFrameConfigurationFactory.cs:85`, `:226-243`), asi que la mayoria de las cabeceras de plantilla quedan fuera hasta G5 | H-02, H-06 |
| Definicion fuente con `Origin ≠ 0` | BASE/BEDIT no soportado | X-09 |
| `View` o `Section` no canonizable | §3.7 | X-10, X-11 |
| Bloques de biblioteca ausentes tras PREPARE | la importacion es best-effort (§9.5) | X-12 |
| Registro de variables no acreditable con algun grupo Selectivo seleccionado | criterio de la autoridad efectiva (`[V2-D16]`) | E7 |

---

## 3. Modelo formal normativo

### 3.1 Definiciones

Transformaciones afines del plano XY de WCS: `T(v)` traslacion, `R(α)` rotacion antihoraria, `S(a,b)` escala.

```text
G        = reflexion de la hoja respecto de la linea del usuario ℓ = (q, φ)
         = T(q) · R(2φ) · S(1,−1) · T(−q)                                      det G = −1
P        = colocacion efectiva de la referencia fuente
         = T(p) · R(θ) · S(s,s) · T(−o)        s > 0,  o = BlockTableRecord.Origin de su definicion     det P = +s²
           primer corte: solo o = 0 (ST-14)  ⇒  P = T(p) · R(θ) · S(s,s)
D        = documento authored del rack (en su sustrato real, §6.2)
μ_k      = reflexion semantica canonica del kind k: documento → documento
Π_(V,σ)  = plan fisico de la vista V con seccion semantica σ, en coordenadas locales de la definicion
           (piezas fisicas; anotaciones y cotas aparte, §11.6)
F        = reflexion local de la vista que expone μ_k:
           X_c = T(2c,0)·S(−1,1)   (x → 2c − x)        Y_c = T(0,2c)·S(1,−1)   (y → 2c − y)
σ'       = seccion canonica destino (§3.7)
```

Las definiciones nuevas se crean con `Origin = 0` (`LateralHeaderDrawer.cs:243`; `CantileverViewMaterializer.cs:47`), asi
que la colocacion destino `P'` no lleva termino de origen.

### 3.2 Exposicion y colocacion

```text
EXPOSICION    (V,σ) expone μ_k  ⇔  Π_(V,σ')(μ_k D) ≡ F ∘ Π_(V,σ)(D)          (≡ de §11)

COLOCACION    P' = G · P · F                    det P' = (−1)(+s²)(−1) = +s²

              F = X_c  ⇒  parte lineal de P' = R(2φ − θ + π) · S(s,s)
              F = Y_c  ⇒  parte lineal de P' = R(2φ − θ)     · S(s,s)

TEOREMA       para (V,σ) admitida:  P' · Π_(V,σ')(μ_k D)  =  G · P · Π_(V,σ)(D)
```

**Consecuencias normativas.**

1. La geometria fisica de cada vista admitida coincide con el espejo geometrico de la hoja.
2. Los textos y las cotas **se regeneran** desde `μ_k D` y quedan legibles (principio de ADR-0031 §8).
3. La colocacion final tiene determinante positivo y la escala uniforme de la fuente. **Nunca** se persiste escala espejo.
4. `μ_k` **no** depende de `ℓ`: el documento reflejado es el mismo para cualquier linea y cualquier subconjunto de
   vistas. Por eso se calcula y verifica **antes** de pedir la linea (§9.1).

### 3.3 Comprobacion algebraica y composicion con `Transform2D`

**Algebra (verificada en la revision de V1).**

```text
S(1,−1)·R(−φ) = R(φ)·S(1,−1)                    ⇒  G = T(q)·R(2φ)·S(1,−1)·T(−q)
F = X_c:  R(2φ)S(1,−1) · R(θ)S(s,s) · S(−1,1)  = R(2φ−θ)·S(−1,−1)·S(s,s) = R(2φ−θ+π)·S(s,s)
F = Y_c:  R(2φ)S(1,−1) · R(θ)S(s,s) · S(1,−1)  = R(2φ−θ)·S(s,s)
det = s² en ambos casos;  F∘F = id  ⇒  P'·Π(μD) = G·P·F·F·Π(D) = G·P·Π(D)
S(−s,−s) = R(π)·S(s,s)                          ⇒  la canonizacion ST-8 es exacta
```

**Ejemplos numericos.**

| # | Datos | Resultado | Comprobacion |
|---|---|---|---|
| E-1 frontal, `X_c` | `s = 1`, `θ = 0`, `p = (10,5)`, linea `x = 0` (`φ = 90°`), `c = 24` | `P' = T(−58,5)`, `θ' = 0` | el poste 0 original cae en `(10,5)` y su espejo en `(−10,5)`; en `μD` ese poste esta en `x = 48` y `P'(48,0) = (−10,5)` |
| E-2 planta, `Y_c` | `s = 2`, `θ = 30°`, `p = (1,2)`, linea por `(3,0)` a 45°, `c = 5`, punto `u = (4,1)` | `θ' = 60°`, `p' = G(P(F(0))) = (22.3205081, −12)` | `G(P(u)) = (10.7320508, 3.9282032)` y `P'(F(u)) = P'(4,9) = (10.7320508, 3.9282032)` |
| E-3 canonizacion | `(−2,−2)` con 30° aplicado a `(1,0)` | `(−1.732, −1)` | igual que `(2,2)` con 210° |

**Composicion.** `Transform2D.Then` lee de izquierda a derecha (`Transform2D.cs:86-93`): `A.Then(B)` aplica `A` y luego
`B`.

```text
P  = Transform2D.Scale(s).Then(Transform2D.Rotation(θ)).Then(Transform2D.Translation(p))      (o = 0)
X_c = Transform2D.MirrorAboutY.Then(Transform2D.Translation(2c, 0))
Y_c = Transform2D.MirrorAboutX.Then(Transform2D.Translation(0, 2c))
G  = ReflectionAboutLine(a, b)                     // fabrica nueva, §8.3 y G4
P' = F.Then(P).Then(G)                             // aplica F, luego P, luego G  ≡  G · P · F
```

La descomposicion de `P'` en `(posicion, rotacion, escala)` exige determinante positivo, parte lineal = rotacion ×
escala uniforme (sin cizalla) dentro de las tolerancias de §4.3, y escala igual a la de la fuente. Si falla: error de
programacion, no clasificacion.

### 3.4 Lemas

- **Lema 1 (elevaciones).** La Y de una elevacion es la altura, que ninguna `μ` valida refleja. Como
  `S(1,−1) = R(π)·S(−1,1)`, cualquier `G·P` se escribe con `F = X_c`. Una elevacion solo puede exponer la `μ` que
  refleja **su** eje X local.
- **Lema 2 (planta).** La planta dibuja los dos ejes horizontales locales y puede exponer cualquiera de las dos
  reflexiones horizontales, eligiendo `F = X_c` o `F = Y_c`.
- **Corolario.** Un rack logico tiene **un** documento authored (ADR-0034 §9) y por tanto **una** `μ`. Una frontal y
  una lateral del mismo rack no pueden ser fieles a la vez.

### 3.5 Contraejemplo minimo (Selectivo frontal + lateral)

```text
Rack Selectivo: DepthCount = 1; Bays = [claro 96", claro 48"]; bota con Placement = EntryExit (solo cara cercana)
Seleccion: frontal (Section 0) + lateral (Section 0); linea vertical
Frontal fiel  ⇒ claros [48", 96"]            ⇒ exige invertir Bays (μ_RUN)
Lateral fiel  ⇒ bota en la cara lejana       ⇒ exige Placement = Rear (μ_DEPTH)
μ_RUN   no voltea la lateral: la bota queda cercana (SelectiveLateralBuilder.cs:173-198)
μ_DEPTH no invierte los claros de la frontal
⇒ no existe D' unico con det P' > 0 para ambas vistas.  ∎
```

Fisicamente `μ_DEPTH = Rot_π ∘ μ_RUN`: los dos describen el mismo rack. El conflicto es de **punto de vista**: RackCad
no puede dibujar una elevacion «vista desde el lado opuesto». Bajo la admisibilidad estricta (§3.8), la lateral no
expone `μ_RUN` y **falla cerrado**.

### 3.6 Reflexion canonica por kind

| Kind | `μ_k` | Por que ese eje |
|---|---|---|
| Selectivo | RUN | Es el eje de la frontal, que el editor obliga a insertar primero (`RackSelectiveWindow.xaml.cs:2083-2091`); conserva «fondo 0 = frente» y evita los casos no representables de profundidad (herencias, diagonales) |
| Dinamico | RT | La salida esta fijada en X=0 (`DynamicLoadBeamGeometry.cs:166-190`): la reflexion de profundidad no es almacenable |
| Push Back simple y compuesto | RT | El lado A siempre existe y la fisica asume flujo hacia +X (`PushBackElevations.cs:166-177`); bajo RT **no** se intercambian A y B |
| Cantilever | R_X | No toca los lados ±Y ni el traslape derivado (`CantileverLineResolver.cs:271-277`) |
| Cabecera | R_D | Su elevacion principal es la lateral (eje profundidad); R_F no produciria cambio visible |
| Cama | ninguna | Ningun campo persistido expresa el lado del tope (`FlowBedLateralBuilder.cs:34-66`) |

### 3.7 Vistas, secciones y exposicion (`SectionRaw → semantica → canonica`)

**Regla.** Por kind, la `View` y la `Section` crudas del sobre se decodifican con la **misma lectura que la
produccion**:

1. si la decodificacion es **inequivoca y ya la soporta produccion**, `σ'` es la seccion **canonica** que produccion
   escribe al redibujar;
2. si no es inequivoca, o produccion la rechaza, la referencia **falla cerrado** (toda la operacion, PDC-3).

La `View` canonica es la constante de `RackEmbedDocument` (`frontal`, `lateral`, `planta`), comparada sin mayusculas
como en produccion. Una `View` **vacia** se lee como la lee produccion para ese kind. Una `View` **no vacia y no
reconocida** falla cerrado en todos los kinds; es mas estricto que el Selectivo y la cabecera de produccion, que la
tratan como frontal y lateral. `c` es el centro del **tramo fisico del eje reflejado** segun las convenciones de cada
builder, no la caja de la vista; el valor entre parentesis es el candidato de V1 y lo fija CT-05.

| Kind | View canonica | `SectionRaw` | `SemanticSection` | `TargetSectionCanonical` | `μ_k` | Expone | `F` | `c` (candidato V1) | Autoridad de produccion |
|---|---|---|---|---|---|---|---|---|---|
| selective | frontal (tambien `View` vacia) | `k`, con `0 ≤ k < fondos` | fondo `k` | `k` | RUN | **si** | `X_c` | centro del tramo del fondo `k` (ultimo poste / 2) | `RackSelectivoCommands.cs:126`, `:168`, `:177`; `ProjectVariableMutationExecutor.cs:186`, `:216` |
| selective | frontal | `−1` (legado documentado) | fondo 0 | `0` | RUN | **si** | `X_c` | idem `k = 0` | `RackSelectivoCommands.cs:155`, `:168`; `ProjectVariableMutationExecutor.cs:185-186` |
| selective | frontal | `< −1` | coercion a 0 sin legado documentado | — | — | **fail-closed** | — | — | produccion coerciona (`:168`) |
| selective | frontal | `≥ fondos` | vista huerfana | — | — | **fail-closed** | — | — | `RACKEDITAR` la borra como fantasma (`RackSelectivoCommands.cs:169-173`); el ejecutor aborta (`ProjectVariableMutationExecutor.cs:202-205`) |
| selective | planta | cualquiera | planta | `−1` | RUN | **si** | `Y_c` | centro del tramo del grid maestro (ultimo poste / 2) | reescribe `−1` (`RackSelectivoCommands.cs:216`; `ProjectVariableMutationExecutor.cs:216`) |
| selective | lateral | poste `p` | poste | — | — | no (**MC**) | — | — | — |
| dynamic | frontal | `0` / `1` | salida / entrada | igual | RT | **si** | `X_c` | centro del tramo de frentes (ultimo poste / 2) | `RackDinamicoCommands.cs:220-224`, `:392-393` |
| dynamic | frontal | fuera de `{0, 1}` | coercion a salida sin legado documentado | — | — | **fail-closed** | — | — | produccion coerciona y reescribe 0 (`:222-224`) |
| dynamic | planta | cualquiera | planta | `−1` | RT | **si** | `Y_c` | centro del tramo de frentes | reescribe `−1` (`:211`) |
| dynamic | lateral o `View` vacia | poste `p` | poste (legado sin `View` = primer corte) | — | — | no (**MC**) | — | — | `:235-239` |
| pushback | frontal | `0..3` (compuesto) o `0..1` (simple) | (corte, lado) | igual | RT | **si** | `X_c` | centro del tramo de frentes (reticula compartida) | `RackPushBackCommands.cs:254-258`; `PushBackSystemFrontalBuilder.cs:137-155` |
| pushback | frontal | `2..3` en rack simple | lado B inexistente | — | — | **fail-closed** (UK hasta CT-04) | — | — | el rack simple ignora el lado (`PushBackSystemFrontalBuilder.cs:43-45`) |
| pushback | frontal | fuera de `0..3` | invalida | — | — | **fail-closed** | — | — | `RACKEDITAR` aborta (`RackPushBackCommands.cs:218`, `:427-450`) |
| pushback | planta | `−1` | planta | `−1` | RT | **si** | `Y_c` | centro del tramo de frentes | `:441-444` |
| pushback | planta, `View` vacia o no reconocida | `≠ −1` o cualquiera | invalida | — | — | **fail-closed** | — | — | `RACKEDITAR` aborta (`:427-450`) |
| pushback | lateral | poste `p` | poste | — | — | no (**MC**) | — | — | — |
| cantilever | frontal | `−1` | linea | `−1` | R_X | **si** | `X_c` | `−(N−1)·s/2` | `RackCantileverCommands.cs:530-540`; `CantileverViewPlanBuilder.cs:237-239` |
| cantilever | planta | `−1` | linea | `−1` | R_X | **si** | `X_c` | `−(N−1)·s/2` | idem |
| cantilever | frontal o planta, `View` vacia o no reconocida | `≠ −1` o cualquiera | invalida | — | — | **fail-closed** | — | — | `RACKEDITAR` aborta (`RackCantileverCommands.cs:501-540`) |
| cantilever | lateral | estacion `k` | estacion | — | — | no (**MC**) | — | — | `CantileverViewPlanBuilder.cs:213-215` |
| cabecera | lateral (tambien `View` vacia) | cualquiera | marco | `−1` | R_D | **si** | `X_c` | `Depth / 2` | `RackCabeceraCommands.cs:197`, `:239-240` (no-planta = lateral; reescribe `−1`) |
| cabecera | planta | cualquiera | marco | `−1` | R_D | **si** | `X_c` | `Depth / 2` | idem |
| cama | (lateral) | `−1` | riel | — | ninguna | no (**MC**) | — | — | `FlowBedLateralBuilder.cs:34-66` |

**Notas.**

- Eje X de cada vista (V1, sin cambio): Selectivo frontal = run; Selectivo planta X = fondo, Y = run; Dinamico y Push
  Back frontal = frentes, planta X = profundidad, Y = frentes; Cantilever frontal = −X (camara con
  `right = up × forward`, `CantileverViewPlanBuilder.cs:209-211`; `Spatial3D.cs:186-197`), planta = (−X, +Y); cabecera
  lateral = profundidad con el poste izquierdo en 0 y el derecho espejado en `Depth`, planta X = fondo.
- En Push Back los cuatro cortes comparten la reticula de columnas; bajo RT **no** se intercambian A y B.
- Que produccion coercione una seccion **no** la vuelve inequivoca: el espejo no copia una coercion que
  `RACKEDITAR` hace en silencio. CT-04 fija la decodificacion de produccion por kind antes de G5.
- La canonizacion se **reporta** en el resumen del comando (vista, seccion cruda y canonica).

### 3.8 Regla de admisibilidad estricta

```text
Para cada referencia seleccionada r de un grupo con kind k:
  si no existe reflector registrado para k                             ⇒ FALLO (E2)
  si (View(r), Section(r)) no decodifica segun §3.7                     ⇒ FALLO de TODA la operacion (E4)
  si la vista semantica de r no expone μ_k                               ⇒ FALLO de TODA la operacion (E5)
No hay interseccion de ejes: μ_k es unica por kind.
```

### 3.9 Alternativas registradas, no adoptadas

- **ALT-F — fidelidad fisica:** admitir las vistas que no exponen `μ_k` con `P' = G·P·X_c`: se refleja la huella y el
  contenido no se voltea. Fisicamente correcto pero graficamente distinto de MIRROR. Requiere decision del Owner.
- **ALT-L — «vista desde el lado opuesto»:** propiedad de vista en el sobre que el builder respete con anotaciones
  legibles. Cambio de modelo con ADR propio. Es la via para laterales, la vista primaria del Dinamico y la cama.
- **ALT-μ2 — segunda reflexion por kind** (intercambio A↔B en la lateral de Push Back compuesto; R_Y en la lateral de
  Cantilever de estacion sencilla): contraria a PDC-4. Registrada como futuro; no se implementa en I-52.

---

## 4. Contrato de fuente, linea y tolerancias

### 4.1 Fuente (por referencia seleccionada, en SNAPSHOT/PREFLIGHT)

Fuente admitida = `BlockReference` de Model Space, no MINSERT, `Normal = +Z`, rotacion finita, escala uniforme,
definicion con `Origin = 0` y `View`/`Section` semanticamente decodificables.

| # | Condicion | Resultado |
|---|---|---|
| ST-1 | Objeto no `BlockReference` | ignorar + aviso (clasificacion de I-51) |
| ST-2 | `BlockReference` sin payload RackCad | ignorar + aviso |
| ST-3 | `MInsertBlock` (subclase de `BlockReference`) | **fail-closed** de la operacion |
| ST-4 | Fuera de Model Space (`OwnerId` ≠ Model Space) | ignorar + aviso (PD-7 de I-51) |
| ST-5 | Angulo entre `Normal` y +Z mayor que `GeometryTolerance.Angle` | **fail-closed** |
| ST-6 | `Rotation` no finita | **fail-closed** |
| ST-7 | Valores absolutos de `sx`, `sy` y `sz` distintos segun la tolerancia de escala (§4.3; valor en G4) | **fail-closed** |
| ST-8 | `sx < 0` y `sy < 0` con `sz > 0` | se canoniza a `(s,s,s)` con `θ + π` (exacto, §3.3) |
| ST-9 | Exactamente una de `sx`, `sy` negativa (MIRROR nativo, semilla espejada de RACKLAYOUT o RACKDUPLICAR) | **fail-closed** |
| ST-10 | `sz < 0` | **fail-closed** |
| ST-11 | Escala uniforme positiva `s ≠ 1` | admitida; `P'` conserva `s` |
| ST-12 | Z de `Position` | se conserva (el plano de reflexion es vertical) |
| ST-13 | Capa | la referencia nueva usa la capa de **su** referencia fuente (como I-51) |
| **ST-14** | `BlockTableRecord.Origin` de la definicion fuente con alguna componente de valor absoluto mayor que `GeometryTolerance.Length` | **fail-closed** (BASE/BEDIT movido no se soporta; `RackCloner.cs:34` copia el origen de la fuente) |
| **ST-15** | `View`/`Section` que no decodifican segun §3.7 | **fail-closed**; las decodificables dan `σ'` canonica |
| **ST-16** | Huella de la referencia (§9.2) | se captura en SNAPSHOT y se re-verifica en PREPARE y en MUTATE |

### 4.2 Linea

| # | Condicion | Resultado |
|---|---|---|
| LN-1 | Dos puntos con `GetPoint`, en UCS | se convierten a WCS con `CurrentUserCoordinateSystem` (patron de `RackDuplicarCommands.cs:145`) |
| LN-2 | Eje Z del UCS no paralelo a Z de WCS | fin sin mutacion |
| LN-3 | Puntos coincidentes tras proyectar a XY (distancia ≤ `GeometryTolerance.Length`) | fin sin mutacion (se puede repetir la peticion) |
| LN-4 | Enter/Esc | fin sin mutacion |

La conversion UCS→WCS y el uso de `Matrix3d` viven **solo** en el Plugin. Application recibe dos `Point2D` de WCS.

### 4.3 Tolerancias

La autoridad del repositorio usa tolerancias **absolutas** en pulgadas y rechaza el epsilon relativo por diseño
(`Vector2D.cs:82-89`). V2 no inventa ninguna tolerancia relativa.

| Magnitud | Autoridad existente | Valor | Uso en I-52 |
|---|---|---|---|
| Longitud | `GeometryTolerance.Length` (`Vector2D.cs:93`) | `1e-9` in | posiciones, `Origin` (ST-14), `Insertion` y `ConnectionAnchor` en V-VIEW, descomposicion de `P'`, LN-3 |
| Continuidad | `GeometryTolerance.Continuity` (`:97`) | `1e-7` in | valores calculados por normalizacion (N-S02) y contornos Cantilever |
| Angulo | `GeometryTolerance.Angle` (`:100`) | `1e-9` rad | rotaciones, `Normal` (ST-5) |
| Marco ortonormal | `LocalFrame3D.OrthonormalTolerance` (`Spatial3D.cs:120`) | `1e-9` | referencia; **no** se reutiliza para escalas |
| Longitud del BOM consolidado | `ConsolidatedBom` (`ConsolidatedBom.cs:62-64`, `:110-115`) | redondeo a 4 decimales en el componente; formato `0.##` en la pieza | V-BOM (§11.5) |
| Factor de escala (adimensional) | **no existe** | — | ST-7 y ST-8. G4 fija el valor, absoluto y no relativo, con prueba y registro. Hasta entonces el contrato no tiene numero |

---

## 5. Seleccion, agrupacion e identidad (reutilizacion de I-51)

### 5.1 Nucleo neutral + fachada compatible (Application)

Se extrae de `RackDuplicationPlan` (`RackDuplicationPlan.cs:225-582`) **solo** lo neutral:

- clasificacion de fuentes (cuatro clases de I-51);
- clave logica discriminada `RackId` / `Definition(handle)`;
- agrupacion y deduplicacion de definiciones conservando todas las referencias;
- consistencia de `Kind` y nombre del grupo.

| Pieza | Regla |
|---|---|
| Nucleo neutral (Application, puro) | Vocabulario de mensajes **inyectado**. Sin nombre de copia, sin `Guid`, sin tipos de AutoCAD |
| `RackDuplicationPlan` | **Fachada**: misma API publica, mismos textos **byte a byte**, misma politica de nombre «- copia», misma autoridad Selectiva. T1–T15 de I-51 sin cambios |
| Mensajes historicos | **CT-16 en G3**, sobre codigo intacto y antes de cualquier extraccion, fija el texto completo de cada mensaje visible, parametrizado (hoy `RackDuplicationPlan.cs:326`, `:407`, `:414`, `:420`, `:425`, `:432-434`, `:450`, `:455`, `:460`, `:518-519`, `:530-532`, `:573-574`, `:580-581`, `:626`, `:632`, `:638` y la politica de `:656`). No se confia en las pruebas actuales, que usan `Contains`. Las excepciones de programacion (`:258`, `:282`, `:364`, `:379`, `:385`) se fijan por tipo |
| Asignador de identidad | Se reutiliza su validacion (`Guid.Empty`, repeticion, igualdad con una fuente) con **politica de nombre inyectada**. RACKDUPLICAR pasa la historica; RACKMIRROR pasa `<base> - espejo` y un unico destino |
| Clone path de RACKDUPLICAR (`RackCloner`, PREPARE/MUTATE de copia) | **No** se reutiliza |

Editar `RackDuplicationPlan.cs` **es** tocar RACKDUPLICAR (`[V2-D27]`): antes de editarlo se avisa a I-49, cuya
condicion de parada lo cubre (I-49 V5 §12.2-12.3); rigen las guardas en RED primero, Core completo, la regresion de
I-51 y M-17 en el Candidato. Si G4 no puede demostrar la equivalencia de la fachada: **STOP → Proposal V3**. No se
duplica la agrupacion en un segundo planificador.

### 5.2 SNAPSHOT propio de RACKMIRROR (Plugin)

- RACKMIRROR tiene **su propio** SNAPSHOT neutral, en un archivo nuevo del Plugin. Captura por referencia: handle,
  `OwnerId`, tipo (`MInsertBlock`), `BlockTableRecord`, `Position`, `Rotation`, `ScaleFactors`, `Normal`, capa,
  `Origin` de la definicion y payload.
- Migrar el SNAPSHOT de RACKDUPLICAR a un helper compartido **no es obligatorio**. Solo se hace si aporta un beneficio
  claro y se demuestra la equivalencia; en ese caso: G-R1, G-R2, G-R4, G-R6 y la guarda de cableado de I-51 **vistas
  en RED primero** y reapuntadas con proteccion igual o mayor, Core completo, regresion especifica de I-51 y M-17.
- Si no se migra, `RackDuplicarCommands.cs` y las guardas de I-51 quedan **intactos** (G-M12).

### 5.3 Plan del espejo (Application, puro)

Compone el nucleo neutral y añade: decodificacion de vista y seccion (§3.7), transformacion fuente (§4.1),
admisibilidad (§3.8), autoridad authored (§5.4), representabilidad y reflexion en el sustrato real (§6, §7),
resolucion efectiva segun el criterio del kind (§10.2), nombre e identidad (§5.5) y composicion del sobre con
`RackEmbedComposer.Compose`. Entrada y salida son datos planos: sin `ObjectId`, sin `Matrix3d`, sin transacciones.

El plan de Application **termina en el documento compuesto**. El re-estampado de identidad lo hace
`RackEnvelopeRestamp`, que vive en el Plugin y despacha el restamp interior por `KindHandlerRegistry`
(`RackEnvelopeRestamp.cs:52-111`); su ensayo lo ejecuta el comando en PREFLIGHT, sin transaccion ni mutacion
(`[V2-D25]`).

### 5.4 Autoridad authored del grupo (todos los kinds)

- Selectivo: `SelectiveAuthoredAuthority.IsSameAuthority` (`SelectiveAuthoredAuthority.cs:96-121`), sin cambios.
- Demas kinds: comparador **por kind** declarado por su reflector (`IsSameAuthority`), estructural sobre el sustrato del
  kind (§6.2) y **excluyendo** los metadatos propios de cada vista que `RACKEDITAR` preserva por hermana
  (`RackCommandSupport.cs:40-67`).
- Divergente o ilegible ⇒ fail-closed (E3). Nunca se elige una vista.
- Solo en el plan del espejo: **RACKDUPLICAR no cambia**.

### 5.5 Identidad y nombre

| Regla | Contenido |
|---|---|
| ID-1 | Un `NewRackId` (`Guid`) por grupo logico, compartido por todas sus vistas seleccionadas |
| ID-2 | `OldRackId → NewRackId` solo en memoria (asignacion del plan); **no** se persiste |
| ID-3 | Legacy sin `RackId`: clave `Definition(handle)` dentro del lote; la copia recibe `RackId` real (precedente I-51) |
| ID-4 | Nombre logico `<base> - espejo` identico en sobre, identidad interior y anotacion de nombre de todas las vistas del grupo |
| ID-5 | Orden obligatorio: **reflejar** en el sustrato → serializar con el store del kind → `RackEmbedComposer.Compose(sobreFuente, kind, idFuente, nombreFuente, ViewCanonica, σ', diseñoReflejado)` → `RackEnvelopeRestamp.RestampEnvelope(payload, nombreEspejo, NewRackId)` (Plugin; NI-1..NI-6 de I-51). Cumple el invariante D-21 de I-54 V2 (mecanismo A y despues B) |
| ID-6 | `RackEnvelopeRestamp.cs` **no** se modifica (G-R5 intacta; T-GRD-03 de I-54 convive) |
| ID-7 | Referencias enlazadas (misma definicion) seleccionadas ⇒ **una** definicion nueva y N referencias |

---

## 6. Reflectores

### 6.1 Contrato conceptual comun (Application, sin tipos de AutoCAD)

Nombres **no** contractuales; las responsabilidades si. El contrato es comun; **el sustrato no**: cada adaptador de
kind trabaja sobre la autoridad real de ese kind (§6.2). No se introducen DTOs para homogeneizar interfaces.

```text
IRackDesignReflector
  Kind                                  // clave del sobre ("selective", "dynamic", ...)
  CanonicalReflection                   // RUN | RT | LINE_X | DEPTH_D | NONE
  Assess(source)        → RepresentabilityReport { clase PDC-7, motivos atribuibles, normalizaciones previstas }
  Reflect(source)       → ReflectionResult { payload interior reflejado, normalizaciones aplicadas y cambios authored declarados }
  DescribeView(source, view, section) → ViewExposure { decodificacion §3.7, Expone, F (Transform2D), c, σ', motivo }
  IsSameAuthority(sources) → bool

source = { payload interior del sobre (JSON), catalogos cargados, lectura del registro de variables (solo lectura) }
```

| Requisito | Regla |
|---|---|
| RF-1 Despacho | `KindDispatch<IRackDesignReflector>` en Application (`KindDispatch.cs:19-104`), busqueda sin mayusculas. El comando **no** contiene constantes `Kind*`, tipos concretos ni ramas por kind |
| RF-2 Sustrato | Cada adaptador lee y escribe con la **autoridad existente** de su kind (§6.2). El JSON es solo la frontera del sobre: **prohibida** la manipulacion de JSON |
| RF-3 Orden | `Assess` antes de `Reflect`. `Reflect` sobre algo que no sea `R`, `R-s` o `RN` ⇒ error de programacion |
| RF-4 Pureza | `Reflect` es puro y determinista; no lee el reloj, no genera ids y no escribe el registro |
| RF-5 Selectivo | Reflexion sobre una copia en memoria del DTO. **Prohibido `WithDesign`** (`SelectivePalletDesignDocument.cs:316-330`, re-congela `VerticalClearance`) y prohibido materializar vinculos |
| RF-6 Dominio | Dinamico, Push Back, Cantilever y cabecera: deserializar con la autoridad existente → reflejar el estado del kind → reconstruir con `RackProject.For*` → `WithSourceMetadataFrom(fuente)` → serializar con `RackProjectStore`. Sin DTO nuevo y sin cambio de schema |
| RF-7 Portadores | Se copian **desde el origen** dentro del sustrato (§6.4); nunca se recalculan |
| RF-8 Normalizaciones | Declaradas, reportadas y validas solo con las obligaciones de §6.5 |
| RF-9 Fidelidad | Nunca peor que el round-trip productivo de `RACKEDITAR` → Actualizar del kind (§6.3) |

### 6.2 Sustrato real por kind

| Kind | `Design` del sobre | Lectura | Se refleja | Escritura | Linea base de fidelidad (lo que ya conserva o pierde `RACKEDITAR` → Actualizar) |
|---|---|---|---|---|---|
| selective | JSON de `SelectivePalletDesignDocument` | `SelectivePalletDesignStore.Deserialize` | copia en memoria del DTO | `SelectivePalletDesignStore.Serialize` | conserva raiz `ExtensionData` (`SelectivePalletDesignDocument.cs:141`), `PropertyValues` con `Kind` desconocido y su `ExtensionData` (`SelectivePropertyValueDocument.cs:25-38`), `SchemaVersion` sticky (`SelectiveDesignSchema.cs:43-66`) y `DimensionViews` (I-50). Pierde miembros desconocidos anidados en otros tipos (limitacion de I-11: `JsonExtensionData` no recursiva) |
| dynamic | JSON de `RackProjectDocument { Kind = PalletFlow, DynamicSystem }` | `RackProjectStore.Deserialize` → `RackProject.DynamicDesign` | `DynamicRackDesign` | `RackProject.ForDynamic(diseño).WithSourceMetadataFrom(fuente)` → `RackProjectStore.Serialize` (`RackDinamicoCommands.cs:379`) | conserva `ExtensionData` y `SchemaVersion` del envoltorio (`RackProjectStore.cs:71-83`). El payload `DynamicRackSystemDocument` **no** tiene `ExtensionData` ni `SchemaVersion`: pierde todo miembro desconocido del payload. Los campos ausentes se escriben con defectos (`DynamicRackSystemDocument.cs:184-199`; `[V2-D26]`) |
| pushback | JSON de `RackProjectDocument { Kind = PushBack, PushBack }` | idem → `RackProject.PushBackDesign` | `PushBackDesign` | `RackProject.ForPushBack(diseño).WithSourceMetadataFrom(fuente)` (`RackPushBackCommands.cs:414`) | conserva envoltorio y raiz del payload (`PushBackDesignDocument.cs:75`; `SystemRegistry.Default.cs:168`); pierde anidados |
| cantilever | JSON de `RackProjectDocument { Kind = Cantilever, Cantilever }` | idem → `RackProject.CantileverLineDesign` | `CantileverLineDesign` | `RackProject.ForCantilever(diseño).WithSourceMetadataFrom(fuente)` (`RackCantileverCommands.cs:490-491`) | conserva envoltorio y raiz del payload (`CantileverLineDocument.cs:46`; `SystemRegistry.Default.cs:220-221`); pierde anidados |
| cabecera | JSON de `RackProjectDocument { Kind = Selective, Header }` o cabecera legada sin `kind` | idem → `RackProject.Header` | `RackFrameConfiguration` (con `RefreshPhysicalModel`, como el store) | `RackProject.ForSelective(config).WithSourceMetadataFrom(fuente)` (`RackCabeceraCommands.cs:194`) | conserva el envoltorio. El payload `RackFrameProjectDocument` **no** tiene `ExtensionData` y escribe `SchemaVersion = "1.0"` (`RackFrameProjectDocument.cs:15-17`; `RackProjectStore.cs:56-57`). Una cabecera legada sin envoltorio sale con envoltorio (`RackProjectStore.cs:313-336`) |

`RackProject.SourceDocument` es `internal` (`RackProject.cs:60`); el adaptador solo usa la costura publica
`WithSourceMetadataFrom` (`:75-79`). Si un adaptador necesitara otra costura del store: **STOP** y revision de
Arquitecto.

### 6.3 Regla de fidelidad y V-RT

I-52 **no** promete preservar lo que el round-trip productivo del kind ya pierde, pero **no** empeora nada.

```text
S_k       = autoridad de lectura/escritura del kind (§6.2)
LB(D)     = S_k.Write(S_k.Read(D))                     linea base: lo que escribe RACKEDITAR → Actualizar sin ediciones
M(D)      = S_k.Write(Reflect(S_k.Read(D)))            salida del reflector
Perdida(X) = miembros de D que el build no liga a tipos y que faltan en X, con su valor
V-RT pasa  ⇔  Perdida(M) ⊆ Perdida(LB)   y   S_k.Write(S_k.Read(M)) es estable (idempotencia caracterizada en CT-03)
```

- CT-03 caracteriza en G3, por kind, la linea base exacta (miembros conservados, perdidos y normalizados).
- Toda perdida **adicional** causada por el espejo ⇒ `UNKNOWN` material ⇒ fail-closed (E6), con el miembro en el
  diagnostico.
- V-RT se evalua sobre el payload interior **despues** del restamp del Plugin, que tambien re-serializa por dominio en
  cabecera y Cantilever (`CabeceraKindHandler.cs:44-66`; `CantileverKindHandler.cs:80-110`).

### 6.4 Portadores preservados desde el origen

| Portador | Kinds | Sustrato |
|---|---|---|
| `SchemaVersion` interior | todos | DTO Selectivo (sticky); envoltorio `RackProjectDocument` via `WithSourceMetadataFrom`; raiz de Push Back y Cantilever via `FromDomain(…, fuente)` |
| `ExtensionData` | segun §6.2 | idem |
| `PropertyValues` completo (incluidos `Kind` desconocidos, `expression` de I-49), literales congelados y `VariableId` | Selectivo | DTO (`SelectivePropertyValueDocument.cs:25-38`; `LinkedPropertyReconciler.cs:193-220`; ADR-0034 §12) |
| `DimensionViews` (I-50, al reconciliar) | Selectivo, Dinamico, Push Back | DTO en Selectivo; dominio en Dinamico y Push Back (sitios de copia C-01..C-15 de I-50) |
| Propiedades de alcance Rack (I-54, al reconciliar) | todos | miembro del sobre, heredado por `Compose` (D-21 de I-54 V2) |
| Cualquier portador integrado en `main` antes del Candidato | todos | §15 |

### 6.5 Normalizaciones

Una normalizacion `N` solo es valida si **todo** se cumple y se prueba:

1. `Π_(V,σ)(N D) ≡ Π_(V,σ)(D)` para **cada** vista del kind, no solo las admitidas.
2. `BOM(N D) = BOM(D)` con la clave de produccion (§11.5).
3. `PropertyValues`, literales y `VariableId` identicos.
4. Portadores de §6.4 identicos.
5. V-RT sobre `N D` (§6.3).
6. El cambio de representacion authored, si existe, se **declara** en el reporte: campo, valor anterior y nuevo.
7. El editor carga `N D` mostrando la misma configuracion efectiva (CT de UI).
8. Involucion: `Reflect(Reflect(D)) = N(D)`, con `N` la forma normal declarada.

#### 6.5.1 N-S02 — medio frente (Selectivo)

```text
Bay con Segments s_0..s_{n−1}, n ≥ 2, que RESUELVE (SelectiveMedioFrente.Resolve ≠ null, SelectiveMedioFrente.cs:55-78)
overhead = 2·(troquelX + inicioX)                                            (:10-17, :60)
r        = BeamLength − Σ_{j<n−1} s_j.Length − (n−1)·overhead                  (remanente del ultimo tramo, :70)
Reflejado:  s'_k = s_{n−1−k}   (Loaded incluido), con
            s'_0.Length     = r            (antes derivado; ahora explicito)
            s'_{n−1}.Length = s_0.Length   (queda almacenado e ignorado por Resolve)
Condicion   troquel y peralte efectivos de los postes f y f+1 iguales; si no ⇒ MC
Bay con Segments que NO resuelve (tramo ≤ 0 o no cabe) ⇒ UK (no se reproduce «dibujar claro completo» sin prueba)
Forma normal N: s_{n−1}.Length := r
Involucion: Reflect(Reflect(D)) = N(D); s_0.Length se recalcula dentro de GeometryTolerance.Continuity
```

**Cambio authored declarado:** el `Length` del nuevo primer tramo pasa de derivado a explicito y el `Length` ignorado
del ultimo tramo se reescribe. Conserva la fisica, **no** la representacion. **CT-21** demuestra: UI equivalente, BOM
equivalente, round-trip e involucion respecto de la forma normal.

#### 6.5.2 N-S06 — `PostPeraltes` (Selectivo)

```text
K = M + 1   (postes del grid maestro; SelectiveGeometryResolver.cs:80-87)
L = PostPeraltes
1. si L.Count < K: añadir 0.0 hasta K           marcador authored de herencia que escribe el editor (SelectiveEditorState.cs:744)
2. L'[p] = L[K−1−p]  para 0 ≤ p < K               valores tal cual: un −1 almacenado sigue significando «hereda»
3. L'[i] = L[i]      para i ≥ K                   cola que el resolvedor ignora (SelectiveGeometryResolver.cs:88-92), sin mover
Forma normal N(L) = pasos 1 y 3
Involucion: Reflect(Reflect(L)) = N(L)
```

- **Nunca** se rellena con el `PostPeralte` resuelto: eso convertiria «usa el global» en valores explicitos y la copia
  dejaria de seguir un cambio posterior del global.
- **CT-22** caracteriza: ceros finales, listas cortas, valores `≤ 0` distintos de `0.0`, listas mas largas que `K`
  (el editor las recorta al abrir, `SelectiveEditorState.cs:745`) e involucion.

#### 6.5.3 N-S07 — `PostCabeceras` y `ExtraFondoPostCabeceras` (Selectivo)

```text
Marcador authored de «estandar» = null (SelectiveGeometryResolver.cs:184-204)
PostCabeceras:                rellenar con null hasta K, invertir el prefijo de K, conservar la cola ignorada
ExtraFondoPostCabeceras[k−1]: rellenar con null hasta C_k + 1 (= K, porque S-01 exige C_k = M), invertir, conservar la cola
Forma normal N: el relleno con null
```

#### 6.5.4 N-H02 — `DiagonalDirection` (cabecera)

```text
UpRight (1) ↔ UpLeft (2)                                          explicitas: se intercambian
AutoAlternating (0): d = ResolveDiagonalDirection() (BracingPanel.cs:40-48); se fija swap(d)     Auto → explicita
otro valor numerico ⇒ UK
Forma normal N: cada Auto sustituido por su direccion efectiva explicita
Involucion: Reflect(Reflect(D)) = N(D)
```

**Cambio authored declarado:** la intencion «alternar automaticamente» pasa a una direccion fija; un cambio posterior
de `Number` ya no re-alterna la copia. **CT-23** caracteriza fisica, BOM, UI, round-trip y la relacion con `IsStandard`,
`IsException` y `StandardBaselineId`. Mientras H-06 sea UK: `AutoAlternating` en un panel con diagonal **y**
`StandardBaselineId` no vacio ⇒ fail-closed.

### 6.6 Verificacion dinamica por rack (PREFLIGHT)

Ademas de las reglas estaticas de §7, para cada grupo el plan del espejo **verifica** sobre el rack concreto:

```text
V-BOM   BOM(efectivo(μ_k D)) = BOM(efectivo(D))                         clave de produccion (§11.5)
V-VIEW  para cada (V,σ) seleccionada:  Π_(V,σ')(μ_k D) ≡ F ∘ Π_(V,σ)(D)     equivalencia de §11
V-RT    Perdida(M) ⊆ Perdida(LB) y estabilidad bajo el store              (§6.3)
```

Cualquier discrepancia ⇒ `UNKNOWN` material ⇒ fail-closed (E6), con la pieza, linea o miembro que difiere en el
diagnostico. La verificacion dinamica es la garantia **ejecutable** del fail-closed: una fila `R-s` que resulte falsa
para un rack concreto no llega a MUTATE.

---

## 7. Matriz de representabilidad V2

### 7.0 Leyenda

| Clase | Significado |
|---|---|
| **R** | `REPRESENTABLE` estructural: la propiedad no produce por si misma geometria anclada en el eje reflejado (escalares, identidad, portadores, ejes no reflejados). Sujeta igualmente a V-BOM y V-RT |
| **R-s** | **`R static / verification required`**: representable segun el modelo, pero su exposicion depende de anclaje a poste, troquel, extremo, offset grafico o bloque con mano. **Provisional** hasta la auditoria de anclajes de G3 (CT-17) y la conmutacion V-VIEW contra los builders reales. G3 la promueve a «R verificada» o abre Proposal V3 |
| **RN** | `REPRESENTABLE_BY_NORMALIZATION`, con las ocho obligaciones de §6.5; siempre requiere verificacion |
| **MC** | `REQUIRES_MODEL_CHANGE` ⇒ fail-closed |
| **UK** | `UNKNOWN` material ⇒ fail-closed |
| **C/F** | `canonicalize-or-fail` (§3.7) |
| **LB** | fidelidad = linea base del store del kind (§6.2); perdida adicional ⇒ UK por V-RT |
| **FC** | fail-closed operativo (diseño invalido, bloqueado o ilegible) |

«Deteccion» = condicion evaluada por rack en PREFLIGHT, antes de pedir la linea. `M` = frentes del grid maestro
(Selectivo); `N` = frentes (Dinamico/Push Back) o estaciones (Cantilever). **Ninguna fila esta verificada en V2.**

### 7.1 Selectivo — `μ_RUN`

| # | Propiedad / estado | Marco local | Regla bajo μ_RUN | Clase | Deteccion | Evidencia |
|---|---|---|---|---|---|---|
| S-01 | Frentes por fondo cuando algun fondo tiene menos frentes que el maestro (esquina) | run; fondo corto = prefijo en el poste 0 | la alineacion al final no es expresable | **MC** | `∃k: C_k < M` | `SelectiveGeometryResolver.cs:131-175` |
| S-01b | Orden de frentes con reticula completa (`Bays`, `ExtraFondoBays[k-1]`) | run | invertir `Bays` y cada lista explicita; un fondo que hereda `Bays` queda invertido por herencia | **R-s** (postes por formula de espaciado con troquel por poste) | — | `SelectiveGeometryResolver.cs:210-224`; `SelectivePostGeometry.cs:32-48` |
| S-02 | Medio frente `Segments` | tramos desde el poste **izquierdo**; el ultimo se calcula | N-S02 (§6.5.1) | **RN** (N-S02) si los postes `f` y `f+1` tienen el mismo peralte y troquel efectivos; si no **MC**; bay que no resuelve **UK** | peralte/troquel de `f`, `f+1`; `Resolve` nulo | `SelectiveMedioFrente.cs:55-78`; `SelectiveDesviadorPlan.cs:214-226` |
| S-03 | `PalletDepth`, `ExtraFondoDepths`, `CabeceraFondoOverrides`, `SeparatorLengths` | profundidad | sin cambio | **R-s** (en planta sus piezas se colocan en las lineas de poste) | — | `SelectiveDepthLayout.cs:118-215` |
| S-04 | Grid maestro y desempate | numero de frentes por fondo | sin cambio (el orden de fondos no se toca) | **R** | — | `SelectiveGeometryResolver.cs:137-175` |
| S-05 | Celdas: pallet, larguero, `BeamLengthOverride`, `ClearOverride` | (fondo, frente, nivel) | se mueven con su frente; valores intactos | **R-s** (larguero en `postX + StartOffset + troquelX`) | — | `SelectiveCellAddress.cs:21-42`; `SelectiveFrontalBuilder.cs:138` |
| S-05b | Por frente: `FloorBeam`, `HeightOverride`, `FloorBeamRiseOverride` | (fondo, frente) | se mueven con su frente | **R-s** | — | `SelectivePalletDesign.cs:695-716` |
| S-06 | `PostPeraltes[p]` | poste del run | N-S06 (§6.5.2): relleno con el marcador `0.0`, permutacion `p → M−p`, cola intacta | **RN** (N-S06) | lista corta, larga o con `≤ 0` | `SelectivePalletDesign.cs:104-107`; `SelectiveGeometryResolver.cs:88-92`; `SelectiveEditorState.cs:744-745` |
| S-07 | `PostCabeceras[p]`, `ExtraFondoPostCabeceras[k-1][p]` | (fondo, poste) | N-S07 (§6.5.3): relleno con `null`, inversion de cada fila | **RN** (N-S07) | filas cortas o largas | `SelectiveGeometryResolver.cs:184-204` |
| S-08 | Cabecera custom: `LeftPost`/`RightPost`, placas | Left = poste delantero (profundidad) | sin cambio | **R** | — | `LateralHeaderLayoutBuilder.cs:49-59` |
| S-09 | `DiagonalDirection` y `AutoAlternating` | plano del marco (profundidad × altura) | sin cambio: la diagonal esta en el plano que RUN deja fijo | **R-s** (la celosia de planta lleva inset respecto de la linea de poste) | — | `BracingPanel.cs:40-48`; `PlantaHeaderLayoutBuilder.cs:71-93` |
| S-10 | `MountingFace` de horizontales y paneles | caras del marco, normal al run | Front ↔ Back | **R-s** si CT-12 confirma la semantica «normal al run»; si no **UK** | CT-12 | `BracingPanelMemberBuilder.cs:159-165`, `:303-322` |
| S-11 | Bota `Side`, `Bota.Placement` | cara cercana/lejana (profundidad) | sin cambio | **R-s** (pieza colocada respecto del poste) | — | `BootPlacement.cs:41-65` |
| S-11b | Bota `PostSides[].PostIndex`, `Bota.Posts[].PostIndex` | poste del run | `p → M−p`; valores intactos | **R-s** | — | `SelectivePalletDesign.cs:244-299` |
| S-12 | Protector lateral `PostSides` explicito distinto del patron por defecto | doble lectura: orientacion en planta y extremo de profundidad en lateral | no demostrada | **UK** ⇒ fail-closed | explicito ≠ {poste 0 Left, poste M Right} | `SelectiveSafetyEnds.cs:100-116`; `SelectiveSafetyPlacement.cs:221-248` |
| S-12b | Protector lateral con el patron por defecto del editor (poste 0 Left, ultimo poste Right) | simetrico | sin cambio | **R-s** (colocado en los extremos) | — | `SelectiveSafetyWindow.cs:788-791` |
| S-13 | Tope: `Side`, `TopeFondo`, `TopeShared`, `TopeSaque`, `TopeFrontal` y toda la familia tope | profundidad; en frontal y planta anclado al troquel del poste izquierdo con `LONGITUD = larguero + 0.25` | **ninguna regla**: la holgura queda a la derecha en el plan regenerado y a la izquierda en el espejo geometrico (AR-01) | **UK** ⇒ fail-closed por defecto (O-2) | el diseño efectivo genera al menos un tope en frontal (cualquier fondo) o planta; predicado exacto en CT-15 | `SelectiveFrontalBuilder.cs:220-222`; `SelectiveTopePlan.cs:140`, `:148`, `:262`, `:270`; `SelectiveSafetyPlacement.cs:55` |
| S-13b | Tope `TopeOffCells[].Frente` | frente, compartido por fondos | sin regla mientras S-13 sea UK | **UK** ⇒ fail-closed (O-2) | igual que S-13 | `SelectiveTopePlan.cs:101-127` |
| S-14 | Desviador `Side`, `DesviadorLongitud`, `DesviadorPrimerNivelAltura` | caras de pasillo (profundidad) | sin cambio | **R-s** (colocado en los postes) | — | `SelectiveDesviadorPlan.cs:116-128` |
| S-14b | Desviador `DesviadorOffCells[]` (clave = indice de poste cargado, incluidos intermedios) | columnas del run | `c → P−1−c` con `P` de la geometria resuelta | **R-s** (requiere catalogo) | — | `SelectiveDesviadorPlan.cs:132-148` |
| S-15 | Parrilla `ParrillaFrontal`, `ParrillaLateral`, `ParrillaFrente`, `ParrillaCantidad` | reparto en `[anchorX, anchorX + span]` con ancla en troquel + inicio del poste izquierdo | sin cambio si cada fila cumple `count·frente ≤ span` | **R-s condicional**; fila desbordada **UK** (`[V2-D23]`) | alguna fila con `count·frente > span + GeometryTolerance.Length` | `SelectiveParrillaPlacement.cs:40-54`; `SelectiveFrontalBuilder.cs:262-311` |
| S-15b | Parrilla `ParrillaOffCells[].Frente` | frente | `f → M−1−f` | **R-s** | — | `SelectiveFrontalBuilder.cs:249-255` |
| S-16 | Numeracion y nombres de bloque por indice | indice | se regeneran (PDC-5) | **R** (contrato de anotaciones, §11.6) | — | `SelectiveFrontalBuilder.cs:320-340` |
| S-17 | `Dimensions`, `DimensionStyle`, `AnnotationScale`, `DrawRackName`, `NumberFronts`, `NumberLevels`, `DrawBasePlate`, `DrawPallets` | toggles | sin cambio; anotaciones regeneradas con el nombre logico destino | **R** | — | `SelectiveDimensions.cs:67-254`; `SelectiveFrontalBuilder.cs:342` |
| S-18 | `PropertyValues` y literales congelados | escalares del rack | copia exacta | **R** | registro acreditable (§10.2) y vinculos resolubles | `SelectiveLinkedProperties.cs:188-213` |
| S-19 | `View` y `Section` del sobre | — | §3.7: frontal `k` o `−1 → 0`; planta `→ −1` | **C/F** | `< −1`, `≥ fondos`, `View` no reconocida | `RackSelectivoCommands.cs:126`, `:155`, `:168-177`, `:216` |
| S-20 | Vista lateral (`Section` = poste) | profundidad | no expone RUN | **MC** (primer corte) | `View = lateral` | `SelectiveLateralBuilder.cs:173-198` |
| S-21 | Campos de otros sistemas en el DTO (`BotaB*`, `DefensaPosts`, `GuiaEntradaOffCells`) | no aplican | round-trip intacto | **R** | — | `SelectivePalletDesignDocument.cs:528-567` |
| S-22 | `AuthoredSide` (persistido por el DTO) | — | sin cambio | **R** | — | `SelectivePalletDesignDocument.cs:579` |
| S-23 | `DimensionViews` (I-50) | tipo de vista | sin cambio | **R** tras reconciliar I-50 | centinela cruzado | `DimensionViewVisibility` (rama I-50) |
| S-24 | Miembros desconocidos anidados | — | segun el DTO | **LB** | V-RT | §6.2 |

### 7.2 Dinamico — `μ_RT`

| # | Propiedad / estado | Marco local | Regla bajo μ_RT | Clase | Deteccion | Evidencia |
|---|---|---|---|---|---|---|
| D-01 | Salida en X=0 / entrada en `TotalLength` | profundidad fijada | sin cambio | **R** | — | `DynamicLoadBeamGeometry.cs:166-190` |
| D-02 | `Fronts[]` y sus niveles | frentes | invertir la lista | **R-s** | — | `DynamicFrontGeometry.cs:129-162` |
| D-03 | `Modules[]` (orden, `Kind`, `Length`) | profundidad | sin cambio | **R-s** (en planta sus piezas se colocan por linea de poste) | — | `DynamicRackSystem.cs:141-153`; `DynamicSystemPlantaBuilder.cs:135` |
| D-03b | `Modules[].Header`: `MountingFace` | caras del marco, normal a frentes | Front ↔ Back | **R-s** si CT-12 confirma; si no **UK** | CT-12 | `BracingPanelMemberBuilder.cs:159-165` |
| D-03c | `Modules[].Header`: `LeftPost`/`RightPost`, diagonales | profundidad | sin cambio | **R-s** (postes y placas en planta con peralte por poste) | — | `LateralHeaderLayoutBuilder.cs:49-59`; `DynamicSystemPlantaBuilder.cs:237`, `:260` |
| D-04 | `HeaderLineOverrides[]` (`PostIndex`, `ModuleId`, `Header`) | linea × modulo | `p → N−p`; `ModuleId` intacto; contenido como D-03b/c | **R-s** | — | `DynamicFrontGeometry.cs:374-395` |
| D-05 | `DerivedPostLineOverrides[]` (`PostIndex`, `Height`) | linea | `p → N−p` | **R-s** | — | `DynamicFrontGeometry.cs:402-424` |
| D-06 | Escalares (alturas, peraltes, pallets, tolerancias, separadores, `DerivedPost*`) | vertical o rack | sin cambio | **R** | — | `DynamicRackSystemDocument.cs:16-45` |
| D-07 | Bota `PostSides`, `BotaPosts` | poste | `p → N−p`; valores intactos | **R-s** | — | `BootPlacement.cs:44-53` |
| D-08 | Protector lateral | orientacion segun la regla adaptativa validada por el Owner en I-32; las copias por extremo dependen del lado | explicito: no demostrada; adaptativo: sin cambio | explicito **UK**; adaptativo **R-s** | `Side ≠ None` o `PostSides` explicitos | `DynamicSafetyMultiViewBuilder.cs:132-140`; `DynamicSafetyDefaults.cs:57-176` |
| D-09 | Desviador `Side` | caras de salida/entrada | sin cambio | **R-s** (colocado en los postes) | — | `DynamicSafetyLateralBuilder.cs:308-317` |
| D-10 | Desviador `DesviadorOffCells` (clave `Math.Min(post, N−1)`) | columnas | reclave de `q = N−p` | **R-s** si, por nivel, `off(poste 0) = off(poste 1)`; si no **MC** | por nivel | `SelectiveDesviadorPlan.cs:28-31` |
| D-11 | Defensa `DefensaPosts[]` (`PostIndex`, `Exit/EntranceLength`, `Auto`) | poste × extremo | `p → N−p`; extremos intactos | **R-s** | — | `DynamicForkliftDefensePlan.cs:50-79` |
| D-12 | Guia de entrada `GuiaEntradaOffCells[]` (`Frente`, `Level`) | frente | `f → N−1−f` | **R-s** | — | `DynamicEntranceGuidePlan.cs:27-64` |
| D-13 | Numeracion de frentes | indice | se regenera | **R** | — | `DynamicViewDecorations.cs:58-69` |
| D-14 | `View` y `Section` | extremo | §3.7: frontal `0`/`1`; planta `→ −1` | **C/F** | frontal fuera de `{0,1}`; `View` no reconocida | `RackDinamicoCommands.cs:211`, `:220-224`, `:392-393` |
| D-15 | Vista lateral (poste), vista primaria obligatoria | profundidad | no expone RT | **MC** (primer corte) | `View = lateral` o vacia | `RackDynamicSystemWindow.xaml.cs:2877-2885`; `RackDinamicoCommands.cs:235-239` |
| D-16 | Payload sin `DynamicDesign` utilizable | sistema resuelto | reflector no definido | **UK** ⇒ fail-closed | `DynamicDesign == null` | `DynamicKindHandler.cs:38-44`. *Precision `[V2-D26]`:* desde el store el diseño siempre se reconstruye con `ToDesign()` y sus defectos (`SystemRegistry.Default.cs:74-88`; `DynamicRackSystemDocument.cs:184-199`); esa normalizacion es de la linea base, no del espejo |
| D-17 | `DimensionViews` (I-50) | tipo de vista | sin cambio | **R** tras reconciliar | centinela | sitios de copia C-08..C-11 de I-50 |
| D-18 | Miembros desconocidos del payload | — | el payload no tiene `ExtensionData` | **LB** | V-RT | §6.2 |

### 7.3 Push Back de un sentido — `μ_RT`

| # | Propiedad / estado | Marco local | Regla bajo μ_RT | Clase | Deteccion | Evidencia |
|---|---|---|---|---|---|---|
| P-01 | Pasillo (extremo bajo) en X=0 | profundidad fijada | sin cambio | **R** | — | `PushBackPlacements.cs:43-61` |
| P-02 | `Structure`: hereda D-02..D-07, D-09..D-11 | — | como el Dinamico | segun fila (**R-s**) | — | `PushBackDesignDocument.cs:27` |
| P-03 | Configuracion por frente (`DefaultPalletsDeep`, `PalletsDeepOverrides`, `DrawPallets`, `HighEndBeamPeraltes`, `FirstLevelHeight`) | frente | se reordena con los frentes | **R-s** | — | `PushBackCellDepth.cs:41-173` |
| P-04 | Tope posterior `RearTopeSaque`, `RearTopePieceId` | lado; `LONGITUD = larguero + 0.25` anclada al troquel del poste con la mano del larguero | sin cambio | **R-s con riesgo AR-01** (`[V2-D24]`); si CT-15 muestra que la holgura no conmuta ⇒ **UK** | CT-15 + V-VIEW | `PushBackRearTope.cs:41-53`; `PushBackSystemFrontalBuilder.cs:285-300`; `PushBackSystemPlantaBuilder.cs:134-154` |
| P-05 | `RearTopeOffCells[].Frente` | frente | `f → N−1−f` | **R-s con riesgo AR-01** | igual que P-04 | `PushBackRearTopeBuilder.cs:268-307` |
| P-06 | `Side` colapsado, `AuthoredSide`, `PostSides` | poste | `p → N−p` | **R-s**; protector lateral explicito **UK** como D-08 | — | `PushBackSafetyAuthority.cs:239-256` |
| P-07 | `DesviadorOffCells` por poste (biyectivo) | poste | `p → N−p` | **R-s** | — | `PushBackSafetyAuthority.cs:271` |
| P-08 | Diseño bloqueado para salida | — | — | **FC** (E6) | `RackBomOutputGate.For(system).Reason ≠ null` | `PushBackKindHandler.cs:61-74` |
| P-09 | `View` y `Section` | — | §3.7: frontal `0`/`1`; planta `−1` | **C/F** | fuera de rango; `2..3` en rack simple; planta `≠ −1` | `RackPushBackCommands.cs:218`, `:254-258`, `:427-450` |
| P-10 | Vista lateral | profundidad | no expone RT | **MC** (primer corte) | `View = lateral` | — |
| P-11 | Miembros desconocidos del payload | — | raiz conservada; anidados perdidos | **LB** | V-RT | §6.2 |

### 7.4 Push Back compuesto A/B — `μ_RT` (sin intercambio A/B)

| # | Propiedad / estado | Marco local | Regla bajo μ_RT | Clase | Deteccion | Evidencia |
|---|---|---|---|---|---|---|
| PC-01 | Configuraciones de los lados A y B | lado | **sin intercambio**; cada lado reordena sus frentes | **R-s** | — | ADR-0031 §5-bis |
| PC-02 | `Structure.Modules` (A → `GAP` → B invertidos con `B:`) | profundidad | sin cambio | **R-s** | — | `PushBackCompositeStructure.cs:495-531` |
| PC-03 | `Topologies[]`: `Frente`; tipo y direccion; defaults | frente | `f → N−1−f`; tipo y direccion intactos; entradas dormantes conservadas | **R-s** | — | `PushBackSide.cs:52-59` |
| PC-04 | `StructureOverrideA/B` | profundidad por lado | sin cambio | **R-s** | — | `PushBackSideConfiguration.cs:182-216` |
| PC-05 | `AbsentSlotsA/B` y nulos legacy de `SideB.Fronts` | ranura por lado | `i → N−1−i` | **R-s** si ningun lado tiene ranuras ausentes al **inicio** ni al **final**; si no **MC** | por lado | `PushBackCompositeStructure.cs:284-321` |
| PC-06 | Bota por lado (`Bota*`, `BotaB*`, `BotaSidesDeclared`) | lado × poste | postes `p → N−p`; lado intacto | **R-s** | — | `PushBackBootPlan.cs:114-159` |
| PC-07 | Defensa `ExitLength` (pasillo A) / `EntranceLength` (pasillo B) y piezas por lado | poste × lado | `p → N−p`; extremos intactos | **R-s** | — | `PushBackDefenseSides.cs:35-95` |
| PC-08 | Tope posterior por lado | lado | off-cells `f → N−1−f` | **R-s con riesgo AR-01** (`[V2-D24]`) | CT-15 + V-VIEW | `PushBackSideDesign.cs:48`; `PushBackRearTopeBuilder.cs:291-306` |
| PC-09 | `HeaderLineOverrides` con `ModuleId` `M…`/`GAP`/`B:…` | linea × modulo | `p → N−p`; `ModuleId` intacto | **R-s** | — | `PushBackEditorDesignAssembler.cs:348-364` |
| PC-10 | `View` y `Section` frontal `0..3` | extremo × lado | §3.7 | **C/F** | fuera de rango | `PushBackSystemFrontalBuilder.cs:137-155` |
| PC-11 | Letras «A»/«B» | posicion en profundidad | sin cambio | **R** (contrato de anotaciones) | — | `PushBackSideAnnotations.cs:31`, `:104` |
| PC-12 | `Composite.Gap`, `CentralSeparator` | — | sin cambio | **R** | — | `PushBackCompositeStructure.cs:123-133` |
| PC-13 | Vista lateral | profundidad | no expone RT | **MC** (primer corte) | `View = lateral` | — |

### 7.5 Cantilever — `μ_X`

| # | Propiedad / estado | Marco local | Regla bajo μ_X | Clase | Deteccion | Evidencia |
|---|---|---|---|---|---|---|
| K-01 | `StationTopology.FaceMode` | — | sin cambio | **R** | — | `CantileverStationDesign.cs:14-21` |
| K-02 | `StationTopology.SingleSide` | ±Y | sin cambio | **R** | — | `CantileverArmFrameResolver.cs:59-75` |
| K-03 | `ArmCellOverrides[].Side` | ±Y | sin cambio | **R** | — | `CantileverLineDesign.cs:259` |
| K-04 | `ArmCellOverrides[].StationIndex` | X (estaciones) | `i → N−1−i` si `0 ≤ i < N`; fuera de rango se conserva; orden de lista intacto (gana la primera coincidencia) | **R-s** | — | `CantileverLineDesign.cs:255`, `:430-437` |
| K-05 | `StationCount`, `ColumnCentreSpacing` uniforme | X | sin cambio | **R** | — | `CantileverLineDesign.cs:339-380` |
| K-06 | Lado de traslape del arriostramiento (derivado) | ±Y | sin cambio | **R-s** (geometria derivada) | — | `CantileverLineResolver.cs:271-277` |
| K-07 | Arriostramiento: diagonales A/B y manos de adaptador (derivadas); `BraceKind`, seccion | plano XZ | A↔B derivado | **R-s** con `ColdRolledRound`; **UK** con seccion estructural asimetrica | `BraceKind` + familia | `CantileverIntervalResolver.cs:227-235`; `CantileverLineFrameResolver.cs:98-116` |
| K-08 | Brazo `Single` con seccion asimetrica (canal C, angulo L) | eje X del marco fijado por el lado | la mano no es expresable | **MC** ⇒ fail-closed | `Arrangement = Single` ∧ familia asimetrica | `CantileverArmFrameResolver.cs:88-123`; `CantileverArmResolver.cs:229-243` |
| K-08b | Brazo W o canal doble | simetrico | sin cambio | **R-s** | — | `CantileverCataloguePolicies.cs:93-131` |
| K-09 | Ids de placa en BOM | patron medido desde `Outline[0]` | — | **UK** ⇒ V-BOM decide por rack | `BOM(μD) ≠ BOM(D)` | `CantileverStationBomBuilder.cs:292-314` |
| K-10 | `View` y `Section` (frontal y planta `−1`; lateral = estacion) | — | §3.7 | **C/F** | frontal/planta `≠ −1`; `View` no reconocida | `RackCantileverCommands.cs:501-540`; `CantileverViewPlanBuilder.cs:237-239` |
| K-11 | Separador siempre `Mirrored = true` (posible defecto existente) | — | no afectado por R_X | **R-s** (defecto fuera de alcance) | — | `CantileverLineFrameResolver.cs:61-85` |
| K-12 | Paneles de arriostramiento, niveles, altura, visibilidad de planta, plantillas de base y columna (W) | Z o simetricos en X | sin cambio | **R-s** (patrones de placa medidos desde un vertice) | — | `CantileverLineDesign.cs:66-309` |
| K-13 | Vista lateral (estacion) | −Y | no expone R_X | **MC** (primer corte) | `View = lateral` | `CantileverViewPlanBuilder.cs:213-215` |
| K-14 | Linea invalida o catalogo de secciones ilegible | — | — | **FC** (E6) | `!line.IsValid` / `TryLoad` falso | `CantileverKindHandler.cs:48-67` |
| K-15 | Miembros desconocidos dentro de `Line` | — | raiz conservada; anidados perdidos | **LB** | V-RT | §6.2 |
| K-16 | Rol visual de cada curva (capa de rol) | — | decidido en el plan; se compara en la firma | **R-s** | V-VIEW | `CantileverViewMaterializer.cs:169-214` |

### 7.6 Cabecera independiente — `μ_D`

| # | Propiedad / estado | Marco local | Regla bajo μ_D | Clase | Deteccion | Evidencia |
|---|---|---|---|---|---|---|
| H-01 | `LeftPost` ↔ `RightPost`; `LeftBasePlate` ↔ `RightBasePlate` | profundidad | intercambio de objetos (el lado se re-deriva al cargar) | **R-s** (poste izquierdo en 0 y derecho espejado en `Depth`) | — | `RackFrameConfiguration.cs:57-60`; `RackFrameProjectDocument.cs:103-106`; `LateralHeaderLayoutBuilder.cs:49-59` |
| H-02 | `Panels[].DiagonalDirection` | UpRight sube izquierda → derecha | explicitas: UpRight ↔ UpLeft; `AutoAlternating`: N-H02 (§6.5.4) | explicitas **R-s**; Auto **RN** (N-H02) con cambio authored declarado; otro valor **UK** | valor del enum; Auto con diagonal | `BracingPanel.cs:40-48`; `DiagonalDirection.cs:5-7` |
| H-03 | `MountingFace` | caras del marco | sin cambio (R_D no toca las caras) | **R** | — | — |
| H-04 | Rasgos que el layout lee solo del lado izquierdo (rejilla de troqueles, `PostId`, placa, inset y celosia en planta, `ConnectionPointId`) | izquierda | el intercambio no es exacto si izquierda y derecha difieren | **R-s** si ambos lados son equivalentes en esos campos; si no **MC** | igualdad de campos + V-VIEW | `LateralHeaderLayoutBuilder.cs:61-67`; `LateralHeaderParametersFactory.cs:31-32`; `PlantaHeaderLayoutBuilder.cs:71-93` |
| H-05 | Horizontales, elevaciones, alturas, troqueles, `Arrangement` | vertical, entre postes | sin cambio | **R-s** (miembros entre postes con conexion por punto) | — | `LateralHeaderLayoutBuilder.cs:83-114`, `:262-294` |
| H-06 | `StandardBaselineId`, `IsStandard`, `IsException` tras normalizar Auto | plantilla | semantica de «estandar» tras N-H02 | **UK** hasta G5; mientras tanto, Auto en panel con diagonal y baseline no vacio ⇒ fail-closed | Auto ∧ baseline no vacio | `RackFrameConfigurationFactory.cs:85`, `:226-243`; `HardcodedStandardRackFrameService.cs:34` |
| H-07 | `View` y `Section` (lateral y planta) | — | §3.7: no-planta = lateral; `σ' = −1` | **C/F** | `View` no vacia no reconocida | `RackCabeceraCommands.cs:197`, `:239-240` |
| H-08 | Round-trip del documento (payload sin `ExtensionData`; version del payload `"1.0"`; legado sin envoltorio) | — | deuda conocida; no empeorar | **LB** | V-RT | `RackFrameProjectDocument.cs:15-17`; `RackProjectStore.cs:56-57`, `:313-336` |

### 7.7 Cama

| # | Propiedad / estado | Clase | Evidencia |
|---|---|---|---|
| F-01 | Lado del tope / sentido del riel: ningun campo persistido | **MC** ⇒ fail-closed siempre | `FlowBedLateralBuilder.cs:34-66` |
| F-02 | `BedType`, `LaneDepth`, `PalletDepth`, `RollerId`, `RollerPitchOverride` | sin vista admisible | `FlowBedConfiguration.cs:10-22` |

### 7.8 Transversal

| # | Propiedad / estado | Regla | Clase | Evidencia |
|---|---|---|---|---|
| X-01 | `Id`/`Name` del sobre e identidad interior | restamp de copia (§5.5) | **R** | `RackEnvelopeRestamp.cs:52-111` (Plugin) |
| X-02 | `SchemaVersion`, `ExtensionData` y miembros del sobre de otras iniciativas (I-54) | `Compose` los hereda (`RackEmbedComposer.cs:21-35`) | **R** si `Compose` hereda todo miembro distinto de `Kind/Id/Name/View/Section/Design`; reconciliar I-54 (D-21: cumple) | `RackEmbedComposer.cs:21-35` |
| X-03 | Miembros desconocidos anidados por kind | fidelidad del store del kind | **LB** | §6.2, §6.3 |
| X-04 | Colocacion de la referencia | `P' = G·P·F` | **R** con fuente canonica (§4.1, `Origin = 0` incluido) | §3 |
| X-05 | Referencias enlazadas seleccionadas | una definicion nueva + N referencias | **R** | I-51 PD-2 |
| X-06 | Divergencia authored entre vistas del grupo | fail-closed | **FC** (E3) | §5.4 |
| X-07 | Drive-In | no existe | N/A | `RackMainMenuWindow.xaml:139-144` |
| X-08 | Larguero | sin bloque RackCad | N/A | `KindHandlerRegistry.cs:51-53` |
| **X-09** | Definicion fuente con `Origin ≠ 0` | ST-14 | **FC** (E4) | `RackCloner.cs:34`; `LateralHeaderDrawer.cs:243` |
| **X-10** | `Section` cruda no canonica | §3.7 | **C/F** | §3.7 |
| **X-11** | `View` no vacia no reconocida | §3.7 | **FC** (E4) | §3.7 |
| **X-12** | Plan con bloques de biblioteca ausentes tras PREPARE | re-verificacion; `MissingInstances` en MUTATE ⇒ excepcion | **FC** (E10/E11) | `BlockLibraryImporter.cs:74-118`; `LateralHeaderDrawer.cs:293-303` |
| **X-13** | Holgura u offset grafico asimetrico hallado por la auditoria de anclajes | ninguna regla implicita | **UK** hasta regla aprobada (precedente AR-01) | CT-17 |
| **X-14** | Parametro dinamico fuera del censo de magnitudes | ninguna regla implicita | **UK** | CT-25; §11.4 |

---

## 8. Regeneracion

### 8.1 Application: autoridad pura de planes por vista

Una autoridad por kind, con contrato comun, **compartida** con el redibujo (no especifica del espejo):

```text
RackViewPlanAuthority (por kind; nombre no contractual)
  Build(request) → ViewPlanResult | fallo atribuible

request = { Design (payload interior en su sustrato), View canonica, σ' canonica, catalogos cargados,
            lectura del registro (solo si el kind lo consume, §10.2), RackName (nombre logico destino),
            sugerencia de nombre de bloque }

ViewPlanResult = { Familia: HeaderRun | CantileverCurves,
                   HeaderRunPlan | CantileverViewPlan,
                   nombre de bloque sugerido,
                   tramo del eje de la vista (para c) }
```

`RackName` es obligatorio: la anotacion «nombre de rack» la dibuja `system.Name` (`SelectiveFrontalBuilder.cs:342`;
`DynamicViewDecorations.cs:83`, `:147`, `:246`) y hoy la fijan los comandos (`RackSelectivoCommands.cs:121`, `:176`;
`RackDinamicoCommands.cs:177`; `RackPushBackCommands.cs:182`; `ProjectVariableMutationExecutor.cs:166-168`, `:236-238`).
La autoridad lo aplica igual para que la copia dibuje `<base> - espejo`.

La autoridad **reutiliza** los builders puros existentes; lo que hoy es pegamento del Plugin pasa a ella:

| Kind | Resolucion (Application, existente) | Plan por vista (Application, existente) |
|---|---|---|
| selective | `SelectivePalletDesignStore` → `SelectiveEffectiveDesignResolver.ResolveAccredited(authored, lectura)` → `SelectiveGeometryResolver.Resolve(efectivo, catalogo)` (mismo camino que `SelectiveKindHandler.cs:36-70`) | frontal: `SelectiveFrontalBuilder.BuildPlan(SelectiveDepthLayout.FondoSystemView(system, k), catalogo)`; planta: `SelectivePlantaBuilder.BuildPlan(system, catalogo)` |
| dynamic | `RackProjectStore` → `DynamicRackSystemResolver(catalogo).Resolve(DynamicDesign).System` (`DynamicKindHandler.cs:38-44`) | frontal: `DynamicSystemFrontalBuilder.BuildPlan(system, catalogo, end)`; planta: `DynamicSystemPlantaBuilder.BuildPlan` |
| pushback | `RackProjectStore` → `PushBackResolver(catalogo).Resolve(PushBackDesign)`; bloqueo `RackBomOutputGate.For` (`PushBackKindHandler.cs:40-74`) | frontal: `PushBackSystemFrontalBuilder.BuildPlan(system, catalogo, end, side)`; planta: `PushBackSystemPlantaBuilder.BuildPlan` |
| cantilever | `RackProjectStore` → `CantileverLineEditorAssembler(secciones).Build(CantileverLineDesign)` (`CantileverKindHandler.cs:48-67`) | `CantileverViewPlanBuilder.Build` |
| cabecera | `RackProjectStore` → `Header` | lateral: `HeaderInstanceGrouper.Group(LateralHeaderLayoutBuilder.Build(config, LateralHeaderParametersFactory.FromConfiguration(config), catalogo).Instances, nombre)` (`LateralHeaderDrawService.cs:60-90`); planta: `new HeaderRunPlan(vacio, PlantaHeaderLayoutBuilder.Build(config, catalogo))` (`PlantaHeaderDrawService.cs:29`) |

Restricciones: sin tipos de AutoCAD; sin `ObjectId`; sin I/O de disco dentro de `Build` (los catalogos llegan cargados);
un fallo de resolucion efectiva devuelve un fallo tipado, nunca un plan con literal congelado.

### 8.2 Plugin: materializador generico

```text
PREPARE (dentro del lock, FUERA de la transaccion de escritura)
  familia HeaderRun  → BlockLibraryImporter.EnsureForPlan(db, plan)                (efectos de infraestructura, §9.5)
                     → re-verificacion en lectura: toda instancia que no sea Annotation ni Dimension tiene BlockName
                       no vacio y presente en la BlockTable
                     → faltantes registrados (pieza, bloque, vista); faltantes ≠ ∅ ⇒ E10
  familia Cantilever → sin bloques de biblioteca (curvas); las capas de rol se crean dentro de MUTATE

CreateInTransaction(db, tx, plan, nombreBloque, payload) → definitionId           (transaccion del llamador)
  HeaderRun  → result = new LateralHeaderDrawer().CreateSystemBlock(db, tx, plan, nombre)     (LateralHeaderDrawer.cs:28-81)
               si result.Outcome.MissingInstances.Count > 0 ⇒ throw  ⇒ rollback total          (camino de I-52)
  Cantilever → CantileverViewMaterializer.CreateBlockDefinition(db, tx, plan, nombre, out real)  (:36-54)
  ambos      → RackBlockData.Write(tx, definitionId, payload)

CreateReference(db, tx, definitionId, colocacion) → referenceId                    (Model Space; transaccion del llamador)
  Position = (p'.X, p'.Y, Z fuente); Rotation = θ'; ScaleFactors = (s, s, s); LayerId = capa de la referencia fuente
```

- **Omision convertida en error.** `CreateSystemBlock` omite hoy las piezas sin bloque y las reporta como faltantes
  (`LateralHeaderDrawer.cs:293-303`; `LateralHeaderDrawOutcome.cs:31-33`; `BlockPlacement.cs:99-117`). Para RACKMIRROR
  una pieza faltante es **fallo duro**: nunca se crea una copia incompleta. El drawer **no** cambia, asi que
  `RACKEDITAR` e Insertar conservan su comportamiento; el control vive en el materializador de I-52 (G-M11).
- Despacho por **familia de plan**, no por kind: el materializador no conoce Left/Right, A/B, estaciones ni reglas de
  cabecera.
- No abre lock ni transaccion, no confirma, no regenera, no purga y no importa dentro de MUTATE (precedente
  `SystemBlockWriter.RedefineInTransaction`, `SystemBlockWriter.cs:82-116`).
- Las capas se crean dentro de la transaccion del llamador (`LateralHeaderDrawer.cs:270`, `:330-331`, `:356`;
  `CantileverViewMaterializer.cs:142`, `:169-195`).
- Todas las vistas de familia `HeaderRun` usan hoy `new LateralHeaderDrawer()` sin opciones (verificado en los siete
  servicios de vista).
- La politica de unicidad de nombres de bloque es la existente de cada familia; I-52 **no** unifica las dos politicas
  (decision de I-09).

### 8.3 Geometria neutral

- `Transform2D.ReflectionAboutLine(a, b)` en Application/Geometry, compuesta con las primitivas existentes (§3.3).
- Un valor de colocacion `(Position2D, RotationRadians, UniformScale)` con composicion a `Transform2D` y descomposicion
  validada (§3.3, §4.3).
- **Sin** sistema matematico paralelo y **sin** `Matrix3d` fuera del Plugin.

### 8.4 Autoridad de `View`/`Section`

La decodificacion `SectionRaw → semantica → canonica` y la exposicion (§3.7) viven en Application, **junto al contrato
de cada kind**. El comando no contiene `switch` de vistas. Las lecturas actuales del Plugin
(`RackSelectivoCommands.cs:126-177`; `RackDinamicoCommands.cs:209-239`, `:392-393`; `RackPushBackCommands.cs:218-258`,
`:427-450`; `RackCantileverCommands.cs:295-314`, `:501-540`; `RackCabeceraCommands.cs:239-240`;
`ProjectVariableMutationExecutor.cs:180-218`) **no** se tocan en el primer corte: CT-04 fija su comportamiento y C-2
(§8.5) comprueba que la autoridad nueva no diverge.

### 8.5 Convergencia con `RACKEDITAR` → Actualizar: **C-2 por defecto**

**Objetivo.** Espejo → guardar/reabrir → `RACKEDITAR` → Actualizar → **mismo** dibujo. RACKMIRROR y los caminos
productivos no divergen al generar la misma vista.

**C-2 (defecto).** Pruebas de equivalencia en **CI**, antes del Candidato, contra los **cuatro** productores reales,
por fixture de (kind, vista admitida) y por su copia espejo:

| # | Productor | Camino que se ejercita |
|---|---|---|
| C2-1 | Autoridad nueva de plan | `RackViewPlanAuthority.Build(designJson, View, σ', catalogos, lectura, RackName)` |
| C2-2 | `RACKEDITAR` → Actualizar (`EditX`) | payload → apertura del editor → sistema que dibuja el comando → servicio de dibujo → builder. Selectivo: `SelectiveEditorOpen.Resolve` → `LoadExisting` → `SystemToInsert` (`RackSelectivoCommands.cs:66-117`; `RackSelectiveWindow.xaml.cs:184-187`); Dinamico, Push Back y Cantilever: `LoadExisting` → `DesignToInsert`/`SystemToInsert` (`RackDinamicoCommands.cs:162-177`; equivalentes); cabecera: `RackFrameConfiguratorViewModel.Configuration` |
| C2-3 | `ProjectVariableMutationExecutor` (Selectivo) | `EffectiveOutput` → `SelectiveGeometryResolver` → `FondoSystemView`/planta → builder (`ProjectVariableMutationExecutor.cs:166-243`) |
| C2-4 | Estado del editor → sistema/documento | `SelectiveEditorState`, `DynamicEditorDesignAssembler`, `PushBackEditorDesignAssembler`, `CantileverLineEditorAssembler` en Core; lo que solo pasa por la ventana, en `tests/RackCad.UI.Tests` |

```text
firma(C2-1) = firma(C2-2) = firma(C2-3, solo Selectivo) = firma(C2-4)        firma de §11.2, anotaciones por contrato
```

- **Guarda G-M9.** Fija el mapa exacto de invocaciones de builders y servicios de dibujo de `EditX` y del ejecutor
  para las vistas admitidas (`SelectiveDepthLayout.FondoSystemView`, `SelectiveFrontalDrawService`,
  `SelectivePlantaDrawService`, `DynamicFrontalDrawService`, `DynamicPlantaDrawService`, `PushBackFrontalDrawService`,
  `PushBackPlantaDrawService`, `CantileverViewPlanBuilder.Build`, `LateralHeaderDrawService`,
  `PlantaHeaderDrawService` y las llamadas a builders dentro de esos servicios). Un cambio futuro la pone en rojo y
  obliga a re-evaluar C-2.
- **Validacion del Owner obligatoria:** M-4.

**C-1 (condicionada).** `RACKEDITAR` construye los planes de las vistas admitidas llamando a la autoridad. Solo se
activa si ocurre al menos una de estas condiciones, con evidencia registrada y orden del Coordinador:

1. divergencia demostrada entre productores;
2. el camino `EditX` de un kind no puede ejercitarse de forma fiable en Core ni en UI;
3. duplicacion sustantiva que dejaria dos autoridades del mismo plan.

### 8.6 Politica post-commit propia de I-52

- **Sin `Regen`** (DA-3), sin purga (crear definiciones no deja anidadas huerfanas); solo mensajes.
- Precedentes: RACKDUPLICAR crea definiciones y referencias sin `Regen` con PASS del Owner (I-51, INV-14); los caminos de
  insercion tampoco regeneran (`SystemBlockWriter.cs:18-40`; `ViewBlockDraw.cs:28-57`).
- Un **unico** `Regen` post-commit solo si el Owner demuestra representacion obsoleta, con evidencia registrada.
- INV-14 de I-51 **no cambia**.

---

## 9. Atomicidad

### 9.1 Fases

```text
ACQUIRE      GetSelection sin SelectionFilter de tipo
SNAPSHOT ALL una transaccion de lectura: referencias y huellas (§4.1, §9.2), definiciones con payload y Origin,
             lectura del registro de variables (una por comando)
PREFLIGHT ALL (sin mutacion, ANTES de pedir la linea)
             Application (puro): clasificacion y agrupacion (§5.1) · View/Section (§3.7) · transformacion fuente (§4.1)
             · admisibilidad (§3.8) · autoridad authored (§5.4) · Assess + Reflect en el sustrato (§6)
             · resolucion efectiva segun el kind (§10.2) · V-BOM / V-VIEW / V-RT (§6.6) · Compose (§5.5)
             Plugin (sin transaccion): ensayo de RestampEnvelope + nombre (§5.3)
             fallo ⇒ mensaje atribuible y FIN — cero mutacion
LINE         dos puntos UCS→WCS validos (§4.2)
PREPARE ALL  colocaciones P' · planes efectivos por vista (§8.1) · payloads definitivos
             · EnsureForPlan (infraestructura, §9.5) · re-verificacion de bloques · re-verificacion de huellas
MUTATE ALL   un DocumentLock · UNA transaccion de escritura:
             re-verificacion de huellas · todas las definiciones nuevas (MissingInstances ⇒ excepcion)
             · todos los payloads · todas las referencias
COMMIT       una vez
POST         solo mensajes (§8.6)
```

### 9.2 Huella de la referencia fuente

Capturada en SNAPSHOT y comparada en PREPARE y al inicio de MUTATE, por referencia seleccionada:

| Campo | Motivo |
|---|---|
| `ObjectId`/handle estable | identidad de la fuente |
| existencia y no borrada | un comando transparente pudo borrarla |
| `BlockTableRecord` | la referencia pudo re-apuntarse |
| `Position`, `Rotation`, `ScaleFactors`, `Normal` | `P` y el contrato de fuente (§4.1) |
| capa | la referencia nueva usa esa capa (ST-13) |
| `Origin` de la definicion | ST-14 |
| hash del payload RackCad de la definicion | el documento reflejado depende de el |

Cualquier diferencia ⇒ **fail-closed antes de mutar** (E10 en PREPARE; en MUTATE, excepcion y rollback).

### 9.3 Tres garantias distintas

| Garantia | Estado | Contenido |
|---|---|---|
| **Atomicidad semantica del rack** | **garantizada** | Una sola transaccion MUTATE con todas las definiciones RackCad, payloads y `BlockReference`. Cualquier fallo ⇒ rollback de **todas** las copias. Nunca queda una copia parcial ni un payload RackCad nuevo sin su definicion |
| **Atomicidad de infraestructura** | **NO garantizada** | PREPARE puede importar definiciones de biblioteca, bloques anidados, capas, estilos y demas dependencias, y pueden **permanecer** aunque despues falle PREPARE o MUTATE (§9.5) |
| **UNDO** | **UNKNOWN** hasta la validacion del Owner en AutoCAD | No hay marcas de undo en el Plugin (busqueda `UndoMark`: cero); rige el grupo de undo por comando. **No** se promete que un UNDO revierta las importaciones (M-10, M-20) |

La garantia de I-52 es **cero mutacion semantica** en cualquier fallo, no «cero mutacion de la base de datos».

### 9.4 Tabla de fallos

| # | Fallo | Momento | Resultado |
|---|---|---|---|
| E1 | Ninguna fuente valida | PREFLIGHT | fin, sin linea |
| E2 | Payload RackCad inutilizable, `Kind` sin reflector o sin handler, `Design` vacio | PREFLIGHT | fin, cero mutacion |
| E3 | Grupo inconsistente (`Kind`, nombre, autoridad authored) | PREFLIGHT | fin, cero mutacion |
| E4 | Fuente no canonica (ST-3, ST-5..ST-7, ST-9, ST-10, **ST-14**) o `View`/`Section` no decodificable (**ST-15**) | PREFLIGHT | fin, cero mutacion |
| E5 | Vista no admisible (lateral de rack, cama) | PREFLIGHT | fin, cero mutacion; el mensaje sugiere reflejar las vistas admisibles e insertar las demas con `RACKEDITAR` desde la copia |
| E6 | `REQUIRES_MODEL_CHANGE`, `UNKNOWN` material (incluidos topes Selectivo, parrilla desbordada y Auto con baseline), V-BOM, V-VIEW o V-RT fallidas, diseño bloqueado o invalido | PREFLIGHT | fin, cero mutacion; nombra rack, propiedad, pieza o miembro |
| E7 | Registro de variables **no acreditable** (presente e ilegible, version incompatible o identidad ambigua) **y** al menos un grupo cuyo kind lo consume (hoy: Selectivo, con o sin vinculos); o vinculo roto | PREFLIGHT | fin, cero mutacion. Si ningun grupo lo consume, el registro no se evalua (§10.2) |
| E8 | Ensayo de `Compose` + restamp + nombre fallido | PREFLIGHT | fin, cero mutacion |
| E9 | Linea invalida o cancelada | LINE | fin, cero mutacion |
| E10 | Fallo de plan, bloque ausente tras importar, huella cambiada | PREPARE | fin; **ninguna** definicion, referencia ni payload RackCad nuevo; lo importado por `EnsureForPlan` puede permanecer (§9.5) |
| E11 | Excepcion en MUTATE, incluidas huella cambiada y `MissingInstances.Count > 0` | MUTATE | sin commit ⇒ rollback de **toda** la operacion semantica |

### 9.5 Contrato de infraestructura: `EnsureForPlan` / `EnsureBlocks`

Hechos del codigo base (`BlockLibraryImporter.cs`):

| # | Hecho | Evidencia |
|---|---|---|
| IF-1 | Debe llamarse dentro del lock y **fuera** de la transaccion del dibujo | `:12-18` |
| IF-2 | Comprueba en una transaccion de lectura que bloques faltan | `:55-67` |
| IF-3 | Biblioteca ausente ⇒ devuelve 0 sin error | `:74-78` |
| IF-4 | Biblioteca ilegible ⇒ se registra y devuelve 0 (cache de sesion) | `:149-161` |
| IF-5 | Clona con `WblockCloneObjects(ids, BlockTableId, mapping, DuplicateRecordCloning.Ignore, deferTranslation: false)`, que arrastra dependencias (capas, estilos, bloques anidados) y conserva las existentes con el mismo nombre | `:109-110` |
| IF-6 | Captura **toda** excepcion, la registra y devuelve 0: importacion **parcial o nula** sin fallo visible | `:113-118` |
| IF-7 | `CreateSystemBlock` omite despues las piezas sin bloque | `LateralHeaderDrawer.cs:293-303` |

Contrato de I-52:

1. PREPARE llama a `EnsureForPlan` por plan de familia `HeaderRun`.
2. PREPARE **re-verifica** en lectura todas las dependencias requeridas por el plan y registra las faltantes.
3. Faltantes ⇒ E10: no se crea nada semantico. Lo ya importado **puede quedar** en el dibujo.
4. MUTATE vuelve a comprobar con `MissingInstances`; si hay faltantes, lanza y revierte (E11).
5. Ninguna afirmacion de atomicidad de infraestructura en mensajes, ADR ni documentacion.
6. El comportamiento del UNDO sobre las importaciones es **UNKNOWN** y se registra como evidencia del Owner (M-10, M-20).

---

## 10. Persistencia, variables de proyecto y portadores

### 10.1 Composicion del payload (por definicion nueva)

```text
1. sobreFuente   = RackEmbedStore.Deserialize(payloadFuente)
2. vista         = §3.7 (View canonica, σ' canonica) — o fallo
3. reflejado     = Reflector(kind).Reflect({ payload interior = sobreFuente.Design, ... })  en el sustrato de §6.2
4. compuesto     = RackEmbedComposer.Compose(sobreFuente, sobreFuente.Kind, sobreFuente.Id, sobreFuente.Name,
                                             ViewCanonica, σ', reflejado.Design)
5. payload       = RackEnvelopeRestamp.RestampEnvelope(RackEmbedStore.Serialize(compuesto), "<base> - espejo", NewRackId)
                   (Plugin; restamp interior por KindHandlerRegistry)
6. postcondicion = V-RT sobre el payload final; portadores de §6.4 identicos
```

Todas las definiciones del grupo reciben **el mismo** payload interior reflejado salvo los metadatos propios de su
vista; en Selectivo se re-verifica `IsSameAuthority` sobre los documentos reflejados.

### 10.2 Variables de proyecto

| Regla | Contenido |
|---|---|
| PV-1 | Los vinculos authored se preservan **exactos**: mismo `PropertyId`, `VariableId`, `Kind` y literal congelado |
| PV-2 | El espejo **no** crea variables, **no** desvincula ni revincula, **no** fija literales, **no** materializa y **no** escribe el registro |
| PV-3 | El espejo **no** evalua expresiones con logica propia |
| PV-4 | Para generar geometria consume **en solo lectura** la autoridad efectiva existente del kind sobre la lectura del registro hecha en SNAPSHOT (`ProjectVariablesRegistry.Read`, `ProjectVariablesRegistry.cs:22`) |
| PV-5 | **Criterio de acreditacion = el de la autoridad efectiva existente, por kind** (`[V2-D16]`). Selectivo: `SelectiveEffectiveDesignResolver.ResolveAccredited` exige un registro acreditable (ausente o legible) **aunque el rack no tenga vinculos** (`SelectiveEffectiveDesignResolver.cs:52-66`; `UsableProjectVariablesRegistry.cs:120-135`); asi abren el editor (`SelectiveEditorOpen.cs:184`), reconcilia `RACKEDITAR` (`LinkedPropertyReconciler.cs:131-139`) y cotiza `RACKBOMTOTAL` (`RackInventarioCommands.BomTotal.cs:56-67`). Dinamico, Push Back, Cantilever y cabecera no tienen vinculos e **ignoran** el registro (`DynamicKindHandler.cs:23-25`; `PushBackKindHandler.cs:26-28`; `CantileverKindHandler.cs:33-35`; `CabeceraKindHandler.cs:21-23`). Por tanto: registro no acreditable + algun grupo Selectivo ⇒ E7; sin grupos Selectivos ⇒ no se evalua |
| PV-6 | Si I-49 integra antes: RACKMIRROR invoca la autoridad efectiva integrada, preserva `expression` como portador y **no** reimplementa su evaluacion (P24.8 de I-49 V5) |
| PV-7 | La copia es un **consumidor nuevo** del mismo `VariableId` (ADR-0034 §12): bloquea `Delete` y se redibuja en `ChangeValue` desde su authored reflejado, sin desespejarse |

Relajar PV-5 para Selectivos sin vinculos seria una regla **nueva**, distinta de la de `RACKEDITAR` y `RACKBOMTOTAL`. V2
no la adopta. Si el Coordinador la quisiera, es una decision de producto que afecta tambien a los comandos existentes.

### 10.3 `DimensionViews` (I-50)

Cada reflector es un **sitio de copia** de `DimensionViews`: transporta el `int` exacto, incluidos bits desconocidos y
`null`, por DTO en Selectivo y por dominio en Dinamico y Push Back. I-50 quedo integrada en `main @ f8deb67` durante
este gate: tras el rebase, antes del Candidato, centinelas `-8`, `13` y `null` a traves del espejo (mismo metodo que
`DimensionViewsCopySitesTests`) son **obligatorios** (T-M18).

### 10.4 Miembros desconocidos, I-54 y deuda de cabecera

- La fidelidad por kind es la de §6.2 y se mide con V-RT (§6.3).
- **Cabecera**: el payload no tiene `ExtensionData` y su version vuelve a `"1.0"`. Es deuda conocida, no se arregla en
  I-52 y V-RT impide que el espejo pierda mas.
- **I-54**: sus propiedades de alcance Rack viven como miembro del sobre y `Compose` lo hereda. D-21 de I-54 V2 declara
  que el espejo **cumple** su invariante (`Compose(sobreFuente, …)` y despues `RestampEnvelope`). Antes del Candidato,
  si I-54 esta integrada: T-M20.

---

## 11. Equivalencia ejecutable bajo reflexion

### 11.1 Definicion

Para una vista admitida `(V, σ)`:

```text
A = firma(Π_(V,σ')(μ_k D))          plan reflejado semanticamente
B = firma(F ∘ Π_(V,σ)(D))           plan original transformado por F
A ≡ B  ⇔  multiconjuntos iguales de piezas fisicas bajo las reglas de §11.2..§11.5
```

El espejo es fiel al **plan regenerado** del documento fuente, **no** a las entidades actuales de su definicion, que
pueden estar desactualizadas o incompletas por bloques que faltaban al dibujarla (`[V2-D28]`). Es la misma semantica
de `RACKEDITAR` → Actualizar.

### 11.2 Firma

| Componente | Regla |
|---|---|
| Piezas | Roles fisicos; `Annotation` y `Dimension` se validan aparte (§11.6) |
| Aplanado | Cada instancia de un `HeaderGroup` se lleva a coordenadas de la definicion componiendo la colocacion con su transformacion completa (rotacion incluida). **No** se usa `HeaderRunPlan.Flatten`/`PlacedClone` (`HeaderRunPlan.cs:53-80`) |
| Identidad de pieza | `PieceId`, `BlockName`, `View` |
| Posicion | `Insertion` y `ConnectionAnchor` con `GeometryTolerance.Length`; `GeometryTolerance.Continuity` solo donde una normalizacion declarada escribe un valor calculado (N-S02) |
| Orientacion | rotacion modulo 2π con `GeometryTolerance.Angle` |
| Escala y mano | `MirroredX`, `MirroredY` y signo del determinante total de la pieza (mano de la pieza ⊕ espejo de la colocacion) |
| Parametros dinamicos | pares ordenados con la regla de §11.4 |
| Curvas Cantilever | contornos con la regla de inversion de `Transform2D` (`PathSegment2D.cs:139-160`), comparados como conjuntos de segmentos |
| Rol de capa Cantilever | `CantileverVisualRole` de cada curva, decidido en el plan (`CantileverViewMaterializer.cs:207-214`) |
| BOM | clave de produccion (§11.5) |
| Anotaciones | contratos semanticos (§11.6) |

### 11.3 Mano y declaracion tipada de simetrias

1. **Ningun bloque DWG se asume simetrico.** La biblioteca no esta versionada ni tiene metadato de simetria.
2. Una diferencia de mano entre A y B en una pieza es **no equivalente**, salvo que el bloque figure en la
   **declaracion tipada unica** de simetrias.
3. **Donde vive (DA-2).** Un unico registro tipado en Application, en la carpeta del espejo (nombre no contractual
   `BlockSymmetryDeclaration`). **Fuera** de `assets/` y de los catalogos. Su unico consumidor es el comparador de
   equivalencia. Ningun literal de simetria en ningun otro archivo (G-M10).
4. **Clave y contenido minimos** de cada entrada:

   | Campo | Contenido |
   |---|---|
   | `BlockName` | nombre exacto del bloque, presente en `assets/catalogs/blocks.csv` |
   | Vista o familia | la `View` del plan (`HeaderRun`) o la familia de curvas |
   | Eje local de simetria | `X` o `Y` del bloque |
   | Centro de simetria | constante `a` en unidades locales, o `P/2` para un parametro de estiramiento `P` (`LONGITUD`, `FRENTE`, …) |
   | Evidencia CT | fila de CT-06 que muestra la diferencia de mano |
   | Evidencia del Owner | registro fechado en `docs/automation/evidence/` con la vista en AutoCAD de la pieza y su espejo |
   | Prueba | prueba de equivalencia que usa la entrada |

5. **Por que hace falta el centro.** Un bloque que se estira desde un extremo (larguero con `LONGITUD`) no es simetrico
   respecto de su origen sino de la mitad de su longitud. Para una entrada con eje X y centro `a`, la instancia
   `(q, r, mX, mY, parametros)` equivale a

   ```text
   (q + R(r)·(2·a·mX, 0), r, −mX, mY, parametros)          eje X
   (q + R(r)·(0, 2·a·mY), r, mX, −mY, parametros)          eje Y
   ```

   que sale de `T(q)·R(r)·S(mX,mY)·X_a = T(q + R(r)·(2a·mX, 0))·R(r)·S(−mX, mY)`, con `X_a = T(2a,0)·S(−1,1)`.
6. **Censo.** G3 produce el censo de piezas con diferencia de mano por kind y vista sobre fixtures asimetricos (CT-06),
   con eje y centro candidatos. El Owner confirma con evidencia (O-3) en G3 y G9.
7. **Sin declaracion:** la vista no es equivalente ⇒ el rack falla cerrado (E6). Algun kind puede quedar inutil hasta
   completar el censo; es el comportamiento correcto.

### 11.4 Parametros dinamicos

- Censo actual de claves en los builders: magnitudes `LONGITUD`, `PERALTE`, `ALTURA`, `SAQUE`, `FRENTE` y `FONDO`
  (`SelectiveRackDefaults.cs:46-95`), mas los nombres configurables de cabecera, que por defecto son esas mismas
  magnitudes (`LateralHeaderParameters.cs:66-72`). CT-25 confirma el censo por kind y vista.
- Una magnitud se compara numericamente con `GeometryTolerance.Length`. La **direccion** en que estira dentro del
  bloque es contenido del bloque y la cubre la declaracion de simetria con centro `P/2` (§11.3).
- Un parametro con semantica de lado o mano (visibilidad, volteo, enumerado de lado) **no** basta con compararlo como
  numero: exige una regla especifica declarada; sin ella ⇒ **fail-closed** (X-14).
- Como los nombres de cabecera son configurables, el censo se evalua **por valor en tiempo de PREFLIGHT**, no solo por
  constantes.

### 11.5 BOM

- **Clave de produccion.** `BOM(efectivo(μ_k D))` y `BOM(efectivo(D))` se comparan con la clave con que produccion
  consolida y cotiza (`ConsolidatedBom.cs:62-64`, `:110-115`): componente = categoria + longitud redondeada a 4
  decimales + firma de piezas (categoria, perfil, longitud con formato `0.##`, cantidad), sumando cantidades; un BOM que
  no es por componentes usa la firma de sus lineas (`:89-107`).
- **No** se exige igualdad decimal mas estricta que la de produccion.
- Si CT-02 muestra que un BOM visible por rack agrupa con otra clave, V-BOM exige igualdad **tambien** bajo esa clave:
  nunca mas estricta que alguna autoridad de produccion, nunca mas laxa que la que ve el usuario.
- Builders: `SelectiveBomBuilder`, `SystemBomBuilder`, `PushBackBomBuilder`, `CantileverLineEditorAssembler.Bom`,
  `BomBuilder`. El orden de lineas no cuenta.

### 11.6 Textos y cotas

No se comparan geometricamente. Se validan con **contratos semanticos**: numeracion regenerada desde el documento
reflejado (PDC-5), letras A/B por posicion, nombre de rack = `<base> - espejo`, cotas segun `Dimensions`, estilo y
`DimensionViews`, y legibilidad (rotacion de texto no invertida). La validacion visual final es del Owner (M-16).

### 11.7 Holguras y offsets graficos

- **Ninguna holgura, offset o separacion puramente grafica se asume simetrica.** AR-01 es el precedente: la holgura de
  0.25" del tope Selectivo esta anclada a un lado.
- CT-17 audita en G3 los anclajes de los builders de las vistas admitidas y clasifica cada coordenada a lo largo del
  eje reflejado como **simetrica** (derivada de ambos extremos o del centro) o **anclada** (desde un poste, troquel o
  extremo, con offset de un lado).
- Cada anclaje asimetrico produce una fila **UK** (fail-closed) o una regla explicita aprobada por el Owner (como O-2).
  Nunca un parche silencioso: una contradiccion con V2 abre Proposal V3.

---

## 12. Plan de pruebas (no se implementa en este gate)

### 12.1 Caracterizacion primero (G3; pasan sobre la produccion intacta)

| # | Caracteriza | Suite |
|---|---|---|
| CT-01 | Fixtures asimetricos por kind y vista admitida con firma de plan (§11.2) | Core |
| CT-02 | BOM base de esos fixtures y claves de agrupacion por kind, incluida la de `ConsolidatedBom` | Core |
| CT-03 | **Linea base real de round-trip por kind** (§6.2): miembros conservados, perdidos y normalizados; idempotencia; defectos de `ToDesign()`; cabecera legada sin envoltorio; version `"1.0"` del payload de cabecera | Core |
| CT-04 | Decodificacion de `View`/`Section` de produccion por kind (tabla de §3.7), incluidos `−1` legado, secciones fuera de rango, `2..3` en Push Back simple y `View` vacia | Core (lecturas puras) + guarda de texto de las lecturas del Plugin |
| CT-05 | Tramos y centros `c` por (kind, vista) desde la geometria resuelta | Core |
| CT-06 | Censo de mano: piezas cuya mano difiere entre `F ∘ Π(D)` y `Π(μD)`, con eje y centro candidatos | Core |
| CT-07 | `WithDesign` materializa `PalletTolerance` vinculada (hueco actual) | Core |
| CT-08 | `SelectiveDesviadorPlan.CellKey` colapsa N−1/N en el Dinamico | Core |
| CT-09 | `BracingPanel.ResolveDiagonalDirection` por paridad | Core |
| CT-10 | Ausencias de Push Back compuesto: inicio, interior y final dibujan distinto | Core |
| CT-11 | Marco del brazo Cantilever por lado y familia de seccion | Core |
| CT-12 | Semantica de `MountingFace` respecto del run (S-10, D-03b) | Core |
| CT-13 | `SelectiveAuthoredAuthority` distingue un documento reflejado de su origen | Core |
| CT-14 | Censo de `[CommandMethod(` = 33 y unicidad de nombres (linea base) | Core |
| **CT-15** | **Topes:** holgura del tope Selectivo en frontal y planta (AR-01) y del tope posterior de Push Back bajo RT (`[V2-D24]`); predicado exacto de «el rack usa topes» | Core |
| **CT-16** | **Mensajes de `RackDuplicationPlan`**: texto completo de cada mensaje visible, parametrizado, sobre codigo intacto (§5.1) | Core |
| **CT-17** | **Auditoria de anclajes** de `SelectiveFrontalBuilder`, `SelectivePlantaBuilder`, `SelectiveTopePlan`, `SelectiveParrillaPlacement`, `SelectiveDesviadorPlan`, `SelectiveSafetyPlacement`, `DynamicSystemFrontalBuilder`, `DynamicSystemPlantaBuilder`, `DynamicViewDecorations`, `DynamicSafetyMultiViewBuilder`, `DynamicForkliftDefensePlan`, `DynamicEntranceGuidePlan`, `PushBackSystemFrontalBuilder`, `PushBackSystemPlantaBuilder`, `PushBackRearTopeBuilder`, `PushBackCompositeFrontal`, `PushBackBootPlan`, `PushBackDefensePlan`, `CantileverViewPlanBuilder`, `LateralHeaderLayoutBuilder` y `PlantaHeaderLayoutBuilder`: cada coordenada a lo largo del eje reflejado, simetrica o anclada | Core |
| **CT-18** | **Origin**: los materializadores crean `Origin = 0`, `RackCloner` copia el de la fuente y ningun comando lo comprueba hoy | guarda de texto (sin AutoCAD en CI, ADR-0003) |
| **CT-19** | **Efectos de `EnsureForPlan`**: IF-1..IF-7 de §9.5 | guarda de texto; los efectos reales (dependencias, importacion parcial, UNDO) son evidencia del Owner en G9 |
| **CT-20** | Filas de parrilla: condicion `count·frente > span` y reparto apilado (`[V2-D23]`) | Core |
| **CT-21** | N-S02: UI, BOM, round-trip e involucion respecto de la forma normal | Core + UI si hace falta |
| **CT-22** | N-S06: ceros finales, listas cortas, valores `≤ 0` distintos de `0.0`, listas largas e involucion | Core |
| **CT-23** | N-H02: fisica, BOM, UI, round-trip y relacion con `IsStandard`/`IsException`/`StandardBaselineId` | Core + UI si hace falta |
| **CT-24** | Criterio de acreditacion del registro por kind (PV-5) | Core |
| **CT-25** | Censo de claves de parametros dinamicos por kind y vista (§11.4) | Core |
| **CT-26** | Mapa de invocaciones de builders de `EditX` y del ejecutor en las vistas admitidas (linea base de G-M9) y caminos del estado del editor (C2-4) | Core + `tests/RackCad.UI.Tests` |

### 12.2 Pruebas nuevas (RED antes de implementar)

| # | Afirma | Gate |
|---|---|---|
| T-M01 | `ReflectionAboutLine`: involucion, determinante −1, puntos de la linea fijos | G4 |
| T-M02 | Colocacion: `P' = G·P·F` con determinante positivo; rotacion `2φ−θ+π` con `X_c` y `2φ−θ` con `Y_c`; escala conservada; descomposicion validada; ejemplos E-1..E-3 | G4 |
| T-M03 | Transformacion fuente ST-1..ST-15 (clasificacion, canonizacion `(−s,−s)`, `Origin`, `View`/`Section`) | G4 (parte pura) y G7 (captura) |
| T-M04 | Nucleo neutral: agrupacion multi-rack, legacy, enlaces; fachada `RackDuplicationPlan` con T1–T15 intactos y **mensajes byte a byte iguales a CT-16** | G4 |
| T-M05 | Asignador con politica de nombre inyectada: «- copia» historico y `<base> - espejo` | G4 |
| T-M06 | `Assess` por kind: cada fila **MC**, **UK** y **FC** de §7 detectada con motivo atribuible | G5 |
| T-M07 | `Reflect` involutivo: `Reflect(Reflect(D)) = N(D)` para diseños **R**, **R-s** y **RN** | G5 |
| T-M08 | Normalizaciones N-S02, N-S06, N-S07 y N-H02 con las ocho obligaciones de §6.5 | G5 |
| T-M09 | `BOM(μD) = BOM(D)` por kind sobre fixtures, con la clave de produccion | G5 |
| T-M10 | Portadores de §6.4 identicos, incluidos `PropertyValues` con `Kind` desconocido | G5 |
| T-M11 | Autoridad authored por kind (divergente, ilegible, igual) | G5 |
| T-M12 | Conmutacion `Π(μD) ≡ F ∘ Π(D)` por (kind, vista admitida) | G6 |
| T-M13 | Autoridad de plan = builders existentes para las mismas entradas | G6 |
| T-M14 | Plan del espejo: E1..E8 sin mutacion, grupos multi-rack con `NewRackId` distintos, sin `WithDesign` | G6/G7 |
| T-M15 | Guardas del Plugin (§12.3) con RED demostrado por violacion temporal no commiteada | G7 |
| T-M16 | Censo 33 → **34** (guarda reapuntada con motivo) | G7 |
| T-M17 | C-2 autoridad frente a `EditX` (C2-1 = C2-2) | G8 |
| T-M18 | Cruce I-50: `DimensionViews` exacto a traves del espejo | G8 (si I-50 integrada) |
| T-M19 | Cruce I-49: entradas `expression` sobreviven y la autoridad efectiva integrada se invoca | G8 (si I-49 integrada) |
| T-M20 | Cruce I-54: propiedades de alcance Rack sobreviven exactas | G8 (si I-54 integrada) |
| **T-M21** | Huella de §9.2: cambio de cualquier campo entre SNAPSHOT, PREPARE y MUTATE ⇒ fail-closed | G4 (comparador puro) y G7 |
| **T-M22** | Canonizacion de `View`/`Section` por kind (§3.7): crudas aceptadas, `σ'` canonica y fail-closed | G5 |
| **T-M23** | `MissingInstances.Count > 0` ⇒ excepcion en el camino de I-52 y ninguna copia | G6 (guarda G-M11 + build Plugin; efecto real en M-19) |
| **T-M24** | Sustrato real por kind y V-RT: `Perdida(M) ⊆ Perdida(LB)` | G5 |
| **T-M25** | Criterio PV-5: Selectivo con registro no acreditable ⇒ E7 con y sin vinculos; kinds sin vinculos ignoran el registro | G5 |
| **T-M26** | La autoridad de plan con `RackName` dibuja `<base> - espejo` en la anotacion de nombre | G6 |
| **T-M27** | Equivalencia con declaracion de simetria (desplazamiento con centro y volteo de mano, §11.3) e integridad del registro (bloque existente en `blocks.csv`, evidencia presente) | G6 |
| **T-M28** | Deteccion UK de topes Selectivo (S-13/S-13b), parrilla desbordada (S-15) y Auto con baseline (H-06) | G5 |
| **T-M29** | C-2 con el ejecutor de variables del Selectivo (C2-1 = C2-3) | G8 |
| **T-M30** | C-2 con el estado del editor → sistema (C2-1 = C2-4), en Core y en UI cuando pase por la ventana | G8 |

### 12.3 Guardas del Plugin (nuevas)

| # | Propiedad protegida |
|---|---|
| G-M1 | El comando no contiene constantes `Kind*`, tipos `*KindHandler` concretos, comparaciones ni `switch` sobre `.Kind`, ni literales iguales a un kind |
| G-M2 | Ninguna llamada a `WithDesign(` en reflectores ni en el camino del espejo; ninguna escritura del registro (`ProjectVariablesRegistry.TryWrite(`) |
| G-M3 | Ningun `Matrix3d` ni `Autodesk.` en los archivos nuevos de Application |
| G-M4 | Exactamente una transaccion de escritura en MUTATE; `EnsureForPlan(` solo en PREPARE; nada tras el commit salvo mensajes (sin `PurgeUnreferenced(`, `SyncName(`); `Regen(` solo si §8.6 lo autoriza con evidencia |
| G-M5 | Sin `CloneDefinition(` en el comando (no es el camino de copia) |
| G-M6 | Sin `FindRackBlocks(`, `ScanEnvelopes(` ni `SelectAll` (sin busqueda de hermanas) |
| G-M7 | Una `GetSelection(` sin filtro de tipo; `CurrentUserCoordinateSystem` solo en la conversion de la linea |
| G-M8 | El materializador no contiene nombres de kind ni de propiedades de sistema |
| **G-M9** | Mapa de invocaciones de builders y servicios de dibujo de `EditX` y del ejecutor en las vistas admitidas (linea base CT-26) |
| **G-M10** | Ningun literal de simetria de bloques fuera del registro tipado de §11.3 |
| **G-M11** | El materializador de I-52 comprueba `MissingInstances` tras `CreateSystemBlock` y lanza; `LateralHeaderDrawer.cs` sin cambios |
| **G-M12** | Si no se extrae un SNAPSHOT compartido: `RackDuplicarCommands.cs` identico al de la base y guardas de I-51 sin reapuntar |

### 12.4 Suites y builds

Cada gate con codigo: Core completo en local (UI segun LC-UI en iteracion y completa si el gate toca
`tests/RackCad.UI.Tests`), CI verde sobre el SHA exacto. Candidato: Core y UI completas en local, builds Debug de UI y
Plugin con AutoCAD cerrado, CI 4/4 sobre el SHA exacto y cobertura del Candidato (WORKFLOW 4.5).

---

## 13. Validacion manual del Owner (AutoCAD 2025)

| # | Escenario |
|---|---|
| M-1 | Selectivo sin topes, frontal + planta, linea vertical: copia reflejada, textos legibles, numeracion regenerada, nombre `<base> - espejo` |
| M-2 | UCS girado 30° y origen desplazado: `P'` correcto en WCS |
| M-3 | Guardar, cerrar y reabrir: la copia sigue reflejada |
| M-4 | `RACKEDITAR` sobre la copia → Actualizar: el dibujo **no** cambia (C-2) |
| M-5 | Desde la copia, Insertar una lateral: sale del diseño reflejado |
| M-6 | `RACKBOMTOTAL`: la copia cotiza igual que el origen |
| M-7 | Selectivo con vinculos: la copia conserva `VariableId`; `ChangeValue` la redibuja reflejada |
| M-8 | Dos racks (frontal + planta de cada uno): dos `NewRackId`, nombres `<base> - espejo` |
| M-9 | Fail-closed sin mutacion: lateral seleccionada, cama, MINSERT, escala no uniforme, fuente espejada, esquina, brazo C sencillo, **Selectivo con topes**, **definicion con BASE movida**, **seccion invalida**, **cabecera de plantilla con Auto** |
| M-10 | UNDO: se registra que revierte y que ocurre con lo importado (evidencia; UNKNOWN hasta entonces) |
| M-11 | Dinamico frontal (salida y entrada) + planta |
| M-12 | Push Back compuesto: frontales 0..3 + planta (tope posterior segun CT-15) |
| M-13 | Cantilever frontal + planta |
| M-14 | Cabecera lateral + planta con diagonales explicitas y postes asimetricos |
| M-15 | Censo de mano: confirmacion visual de los bloques declarados simetricos, con eje y centro (O-3) |
| M-16 | Anotaciones y cotas legibles, respetando `DimensionViews` si I-50 esta integrada |
| **M-17** | **Regresion de RACKDUPLICAR** (G4 edita `RackDuplicationPlan.cs`): multiples origenes y destinos, mensajes y fail-closed de I-51 |
| **M-18** | Selectivo legado con frontal `Section = −1`: la copia sale con `Section = 0` y `RACKEDITAR` la trata igual |
| **M-19** | Bloque de biblioteca ausente (con una biblioteca de prueba, nunca la del Owner): RACKMIRROR falla sin crear copias |
| **M-20** | Efectos de infraestructura tras un fallo en PREPARE o MUTATE: que definiciones, capas o estilos importados quedan (evidencia) |

---

## 14. Gates propuestos

Cada gate declara entrada, archivos permitidos, RED esperado, PASS requerido, parada y evidencia por SHA exacto. Ningun
gate arranca sin la orden explicita del Coordinador y sin el preflight de paralelas (§15.3). **Cualquier contradiccion
de una caracterizacion o de una prueba con V2 ⇒ STOP → Proposal V3**; nunca un parche silencioso.

| Gate | Entrada | Archivos permitidos | RED esperado | PASS requerido | Parada | Evidencia |
|---|---|---|---|---|---|---|
| **G2** (este) | G1 aceptado; revision de V1 | `docs/**` | — | consenso de Coordinador y Arquitecto sobre la misma version; **O-1 aceptada por el Owner**; ADR-0036 aceptado por el Owner tras la verificacion de numeracion (§16); contrato reescrito; hallazgos laterales en `ideas-futuras.md` | desacuerdo material ⇒ Proposal V3 | SHA acordado + CI |
| **G3** Caracterizacion | G2 congelado; O-1 aceptada | `tests/RackCad.Tests/**` (caracterizaciones y fixtures nuevos); `tests/RackCad.UI.Tests/**` si una caracterizacion lo exige | ninguno: CT-01..CT-26 pasan sobre produccion intacta | Core completo (UI si se toco); censos de mano y de parametros; tabla de `c`; auditoria de anclajes; mensajes de I-51; decodificacion de secciones; linea base de round-trip por kind; topes | cualquier contradiccion con V2 ⇒ STOP → V3 | SHA + conteos + CI |
| **G4** Geometria y nucleo neutral | G3 | `src/RackCad.Application/Geometry/*`; nucleo neutral y fachada en `src/RackCad.Application/Persistence/` (incluye `RackDuplicationPlan.cs`, **con aviso previo a I-49**); pruebas | T-M01..T-M05 y T-M21 (comparador puro) en rojo sobre andamiaje | verdes; T1–T15 de I-51 y CT-16 identicos byte a byte; `RackDuplicarCommands.cs` sin cambios; tolerancia de escala fijada y registrada | cambio observable de RACKDUPLICAR o de sus mensajes | SHA + conteos + CI |
| **G5** Reflectores y representabilidad | G4 | contrato y registro en carpeta nueva del espejo en Application; reflector por kind en `src/RackCad.Application/Systems/<Kind>/` y `RackFrames/`; registro de simetrias vacio; pruebas. Divisible en G5a Selectivo, G5b Dinamico y Push Back, G5c Cantilever y cabecera | T-M06..T-M11, T-M22, T-M24, T-M25, T-M28 en rojo | verdes; sustrato real por kind; topes UK; H-06 UK; `WithDesign` ausente | una fila R, R-s o RN resulta no equivalente ⇒ STOP → V3 | SHA por subgate + CI |
| **G6** Planes y materializador | G5 | autoridades de plan por kind en Application; `SystemBlockWriter.cs` (`CreateInTransaction`); materializador nuevo en `src/RackCad.Plugin/Systems/Shared/`; pruebas | T-M12..T-M14, T-M23, T-M26, T-M27 en rojo | conmutacion verde por (kind, vista admitida); `MissingInstances` = fallo duro en el camino de I-52; builders y drawers sin cambio de comportamiento | necesitar cambiar un builder o un drawer existente | SHA + CI + build Debug de Plugin |
| **G7** Comando RACKMIRROR | G6 | `src/RackCad.Plugin/RackMirrorCommands.cs` (nuevo) y SNAPSHOT propio; archivos de guardas; `SelectiveEditorOpenTests.cs` (censo); `src/RackCad.UI/RackCommandReference.cs`. `RackDuplicarCommands.cs` **solo** si se decide extraer un SNAPSHOT compartido (§5.2) | T-M15, T-M16 y G-M1..G-M8, G-M10..G-M12 en rojo por violacion temporal | guardas verdes; censo 34; build Debug de Plugin; sin extraccion, archivos y guardas de RACKDUPLICAR intactos (G-M12) | debilitar una guarda de I-51 | SHA + conteos + CI |
| **G8** Convergencia C-2 y cruces | G7 | pruebas Core y `tests/RackCad.UI.Tests/**` para C-2; guarda G-M9; pruebas cruzadas. Produccion de `RACKEDITAR` **solo** si se activa C-1 (§8.5) con orden del Coordinador | T-M17..T-M20, T-M29, T-M30 y G-M9 en rojo cuando apliquen | C-2 verde en CI para los cuatro productores; cruces verdes con lo integrado (I-49, I-50, I-54) | divergencia ⇒ decidir C-1 con Coordinador y Arquitecto; un cruce exige tocar §20.3 | SHA + CI |
| **G9** Conformidad, Candidato y Owner | G8 | ninguno de produccion salvo correcciones | — | rebase final si `main` avanzo; Core y UI locales; builds Debug; CI 4/4 exacto; cobertura; M-1..M-20 (M-17 incluido); O-3 confirmado | cualquier fallo ⇒ vuelta al gate que corresponda | SHA Candidato + corridas + veredicto del Owner |
| **G10** Documentacion e integracion | G9 | `docs/**`, `README.md` | — | WORKFLOW 4.5: cierre documental, merge `--no-ff`, CI del `MERGE_SHA`, cobertura, limpieza | CI post-merge rojo ⇒ correccion en la rama | SHAs de cierre y merge |

---

## 15. Coordinacion con iniciativas paralelas

### 15.1 Tips observados

Preflight al abrir el gate de V2:

```text
origin/main                                        = 46fcac2b071929d2bd5b07aa28373941417f74a8 (sin avance desde G2)
I-49 architecture/motor-expresiones-parametricas   = 2783accd0c9400ed1308ab5c40072eadcd388f36 (Proposal V5; solo docs; avanzo desde ccf21c6)
I-50 feature/cotas-independientes-por-vista        = 6cd2970c36a906cb9e784797008bed5e8111e433 (con produccion; G3 A/B/C de UI)
I-53 feature/cabeceras-configurables-multidestino  = f38362d32a737f62d69a229854dc5c1812adf063 (Proposal V1; solo docs)
I-54 architecture/propiedades-personalizadas       = 36c337b84c47f9ac7c97d860fce42d3a5f4370e8 (Proposal V2; solo docs)
```

Preflight antes de publicar V2 (`[V2-D29]`):

```text
origin/main                                        = f8deb675c6d1ef0e64693b157d69c4cc170d7b24 (Merge I-50, 2026-09-12T21:37:17-06:00)
I-50 feature/cotas-independientes-por-vista        = c9a5f0a (cierre documental; INTEGRADA en main)
I-53 feature/cabeceras-configurables-multidestino  = d7f17addb47ea943b61ff50b4b4a5b23010d6fab (Proposal V2 + ADR-0037 propuesto; solo docs)
I-49 = 2783acc e I-54 = 36c337b                    (sin cambio)
```

I-52 **no** rebasea en este gate (solo documentacion). Entre la base `46fcac2` y `main @ f8deb67` **no** cambian
`RackDuplicationPlan.cs`, `RackDuplicarCommands.cs`, `RackEnvelopeRestamp.cs`, `RackEmbedComposer.cs`,
`SystemBlockWriter.cs`, `LateralHeaderDrawer.cs`, `CantileverViewMaterializer.cs`, `BlockLibraryImporter.cs`,
`RackProjectStore.cs`, `RackProject.cs` ni los builders de plan de §8.1. Si cambian, por I-50, archivos con lineas de
portador `DimensionViews` (§15.2), y con ello se desplazan algunas citas `archivo:linea` de V2, que son del arbol
`46fcac2`. El rebase sobre `main` se hace segun WORKFLOW antes de escribir codigo, con la orden del gate que corresponda.

### 15.2 Cruces

| Paralela | Cruce | Tratamiento |
|---|---|---|
| **I-50** (**INTEGRADA** en `main @ f8deb67` durante este gate) | Produccion en `SelectivePalletDesignDocument.cs`, `DynamicRackSystemDocument.cs` (portador `DimensionViews`), `DynamicRackSystemResolver.cs` (C-10, diseño ↔ sistema), `DynamicEditorDesignAssembler.cs` (C-09), `PushBackEditorState.Load.cs`, `PushBackCompositeStructure.cs`, `PushBackMirror.cs`, `SelectiveDepthLayout.cs`, `SelectiveGeometryResolver.cs` (C-04), `SelectiveDesignInputs.cs`, `SelectiveEditorState.cs`, `SelectiveDimensions.cs`, `DynamicViewDecorations.cs`, `DynamicAnnotationOptions.cs`, `DimensionViewPolicy.cs` y `DimensionViewVisibility.cs` (nuevos), dominio (`DynamicRackDesign.cs`, `DynamicRackSystem.cs`, `SelectivePalletDesign.cs`, `SelectiveRackSystem.cs`) y las tres ventanas ricas; ADR-0035 aceptado en su rama; contrato de sitios de copia C-01..C-15 | I-52 **no** toca esos archivos. Cada reflector es un sitio de copia (§10.3); el sustrato de dominio de Dinamico y Push Back transporta `DimensionViews` por los sitios C-08..C-15. Como I-50 ya esta en `main`: al rebasar, **T-M18 es obligatoria**, CT-01/CT-26 y la firma de anotaciones de C-2 se miden sobre `main`, y el indice de ADR conserva la fila 0035 junto a la 0036 |
| **I-49** | V5 (`2783acc`): `expression` en `PropertyValues`; su condicion de parada cubre `RackDuplicationPlan`, restamp, `SelectiveAuthoredAuthority` y store Selectivo; §12.3 registra que la fachada de G4 de I-52 toca `RackDuplicationPlan.cs` y avisa antes | **Aviso a I-49 antes de editar `RackDuplicationPlan.cs`** (G4); T-M19; RACKMIRROR invoca la autoridad efectiva integrada; P24.8 de I-49 (el espejo conserva `expression` sin interpretarla) |
| **I-53** | V2 (`d7f17ad`): archivos calientes de editores y autoridad de cabecera; copias de configuracion de cabecera que caen en campos **ya persistidos** (`PostCabeceras`, `ExtraFondoPostCabeceras`, `Modules[].Header`), **sin DTO ni campo nuevo**; ADR-0037 `propuesto` | Serializar integracion si C-1 llegara a tocar comandos de `RACKEDITAR`; todo campo nuevo direccionado por poste, fondo, modulo o lado necesita regla de reflexion antes del Candidato; el indice de ADR conserva 0035, 0036 y 0037 |
| **I-54** | V2 (`36c337b`): propiedades de alcance Rack como miembro del sobre; **D-21** invariante de preservacion del sobre: el espejo de I-52 (mecanismo «A y despues B») **cumple**; T-GRD-02 (censo de llamadas a `Compose`) pasa de 7 a 8 con el espejo; T-GRD-03 (restamp estructural) convive con G-R5 | I-52 conserva ID-5 (§5.5): `Compose(sobreFuente, …)` y despues `RestampEnvelope`, nunca `new RackEmbedDocument` ni `Compose(null)` sobre un rack existente. Quien integre despues re-apunta el censo de T-GRD-02 y clasifica la llamada como A con `source` real; T-M20 |
| Documental | Filas de ROADMAP (I-49, I-50, I-52, I-53, I-54), final de `ideas-futuras.md`, `docs/adr/README.md` (0035 de I-50 y 0036 de I-52), censo de comandos 33 → 34 | Quien integre despues conserva todas las filas y entradas en orden numerico y re-mide los censos |

### 15.3 Protocolo antes de G3, G5, G7 y del Candidato

`git fetch --all --prune`; `git diff --name-only origin/main...origin/<rama>` de I-49, I-50, I-53 e I-54; leer sus
Proposals y contratos en el SHA exacto; buscar `docs/adr/0036-*` y citas de «ADR-0036» en **todos** los refs (§16);
comprobar los archivos de §19; si `main` avanzo, rebase segun WORKFLOW antes de escribir codigo.

---

## 16. ADR-0036 y protocolo de numeracion

- Archivo: `docs/adr/0036-rackmirror-espejo-semantico-por-copia.md`, estado **`propuesto`**, con fila en el indice.
  Corregido en este gate segun `[V2-D15]`.
- Congela: espejo semantico por copia; `μ_k` canonica por kind; admisibilidad por vista con seccion canonica;
  fail-closed; reflectores en Application sobre el sustrato real de cada kind; autoridad de planes; colocacion canonica
  sin escala negativa y con `Origin = 0`; identidad nueva; portadores; atomicidad semantica e importacion best-effort;
  politica de simetria y mano; verificacion dinamica por rack; futuro «vista desde el lado opuesto».

**Protocolo de numeracion (sustituye a V1 §16).**

1. Un numero de ADR queda **reclamado** por la primera publicacion **observable** de un archivo `docs/adr/NNNN-*` en un
   ref remoto.
2. **Registro de I-52:** ADR-0036 se publico por primera vez en `origin/feature/rackmirror-espejo-semantico` con el
   commit `0fc7032bf15d03e7d478bbd9350f156708621c9d` (fecha de commit `2026-09-12T19:59:36-06:00`); la corrida de CI
   del push, `34731908035`, se creo el `2026-09-13T01:59:44Z`. En el preflight de V2 ningun otro ref contiene
   `docs/adr/0036-*`; I-49 V5, I-53 V2 e I-54 V2 citan 0036 como de I-52. ADR-0035 es de I-50 (publicado en `93c352a`,
   `2026-09-12T14:10:40-06:00`; aceptado e integrado en `main @ f8deb67`). ADR-0037 lo publico I-53 en `d7f17ad`
   (`2026-09-12T21:24:06-06:00`), despues de 0036: no hay colision.
3. **Antes de pedir la aceptacion del Owner**, I-52 busca 0036 en todos los refs: archivos `docs/adr/0036-*` y citas de
   «ADR-0036» o «adr/0036».
4. Si existe una publicacion **anterior** de otro ADR-0036, I-52 **renumera** el suyo al primer numero libre antes de
   pedir la aceptacion. Una publicacion posterior de otra rama no obliga a I-52 a renumerar.
5. Una vez ADR-0036 sea **`aceptado`**, **no** se renumera (`adr/README.md`: un aceptado es inmutable).
6. Dos ADR **aceptados** con el mismo numero ⇒ **STOP** y escalado al Owner.

La aceptacion es solo del Owner (O-4) y **no** ocurre en este gate: primero la revision de Arquitecto de V2 y el
consenso. `docs/adr/README.md` no cambia en este gate: titulo, estado y numero del indice siguen siendo correctos.

---

## 17. Riesgos abiertos

| # | Riesgo | Mitigacion |
|---|---|---|
| R-1 | Censo de mano: muchas piezas podrian diferir y dejar kinds enteros en fail-closed hasta la declaracion de simetria, que ahora exige eje **y** centro | CT-06 en G3; O-3; M-15 |
| R-2 | Un Dinamico dibujado solo en lateral no puede reflejarse | Mensaje E5 explicito; ALT-L como futuro |
| R-3 | V-RT puede rechazar payloads legitimos si la idempotencia del store no es exacta | CT-03 caracteriza la linea base y la idempotencia por kind |
| R-4 | C-1, si llegara a activarse, toca comandos calientes que I-53 tambien toca | C-2 por defecto; C-1 solo con evidencia; serializar |
| R-5 | Semantica de «estandar» de cabecera tras normalizar Auto (H-06): la mayoria de las cabeceras de plantilla quedan fuera | G5 con CT-23; fail-closed mientras tanto; declarado en §2.3 y O-1 |
| R-6 | `MountingFace` sin semantica confirmada (S-10, D-03b) | CT-12; si no se confirma ⇒ UK |
| R-7 | Comandos transparentes de AutoCAD entre SNAPSHOT y MUTATE | Huella completa de §9.2 en PREPARE y MUTATE |
| R-8 | Numeracion de ADR con varias iniciativas que necesitan ADR | Protocolo de §16 |
| R-9 | Cantilever con brazos C/L sencillos o arriostramiento estructural asimetrico queda fuera | Clasificacion MC/UK; futuro con metadato de mano |
| R-10 | Rendimiento de V-VIEW y V-BOM en selecciones grandes (dos planes por vista, dos BOM por rack) | Medir en G7; builders puros |
| **R-11** | Selectivos con topes inutilizables hasta O-2 | Fail-closed verificable antes de mutar; decision del Owner |
| **R-12** | La auditoria de anclajes (CT-17) puede degradar muchas filas R-s a UK | Proposal V3 con la tabla corregida; nunca parche silencioso |
| **R-13** | G4 toca el planificador de RACKDUPLICAR, validado por el Owner en I-51 | CT-16 primero; fachada byte a byte; M-17; aviso a I-49 |
| **R-14** | La linea base de fidelidad pierde miembros desconocidos del payload Dinamico y de los anidados de todos los kinds, igual que `RACKEDITAR` | Declarado en §6.2; V-RT impide perdidas adicionales; arreglarlo es otra iniciativa |
| **R-15** | C-2 sobre ventanas WPF exige pruebas de UI mas costosas | `tests/RackCad.UI.Tests` permitido en G3 y G8; LC-UI |
| **R-16** | La importacion de biblioteca puede dejar dependencias tras un fallo y su UNDO no esta demostrado | §9.3 y §9.5 lo declaran; M-10 y M-20 lo registran |

---

## 18. Resolucion de DA-1..DA-6 y decisiones del Owner

### 18.1 Resolucion registrada para V2 (orden del Coordinador; no es consenso)

| # | Tema | Resolucion V2 | Donde |
|---|---|---|---|
| DA-1 | Convergencia con `RACKEDITAR` | **C-2 por defecto** con cuatro condiciones: los cuatro productores (autoridad, `EditX`, ejecutor de variables Selectivo, estado del editor → sistema), pruebas en CI, guarda G-M9 sobre las invocaciones de `EditX` y M-4 obligatoria. **C-1** solo con evidencia: divergencia demostrada, `EditX` no ejercitable de forma fiable, o duplicacion sustantiva de autoridades | §8.5 |
| DA-2 | Declaracion de simetrias | **Registro tipado unico en Application**, en la carpeta del espejo; `assets/` y catalogos fuera; clave `BlockName` + vista/familia + eje + centro, con evidencia de CT y del Owner y prueba; no declarado ⇒ fail-closed | §11.3 |
| DA-3 | Politica post-commit | **Sin `Regen`**; un unico `Regen` solo si el Owner demuestra la necesidad | §8.6 |
| DA-4 | Recorte del primer corte | Se mantiene: Selectivo, Dinamico, Push Back y Cantilever frontal y planta; cabecera lateral y planta; laterales de rack y cama fail-closed. **OWNER PRODUCT ACCEPTANCE REQUIRED BEFORE G3** (O-1). Los topes Selectivo añaden fail-closed por AR-01. Sin ALT-F ni segunda `μ` | §2, §3.9 |
| DA-5 | `IsStandard`/`StandardBaselineId` | **UNKNOWN → fail-closed** hasta G5; no bloquea G2 | §6.5.4, §7.6 |
| DA-6 | Numeracion de ADR | **Precedencia del primer ref remoto publicado**; corregido en Proposal, ADR y registro de decisiones | §16 |

### 18.2 Decisiones del Owner

O-1..O-4 de §1.2. Resumen: **O-1** alcance antes de G3; **O-2** topes Selectivo (default fail-closed); **O-3** censo de
simetrias con evidencia en G3/G9; **O-4** ADR-0036, no en este gate.

---

## 19. Condiciones de parada

- Implementar sin la orden explicita del gate: **prohibido**.
- Una caracterizacion o prueba contradice V2, o una fila R, R-s o RN resulta no equivalente: **STOP → Proposal V3**.
- Necesitar cambio de schema, de sobre o de Xrecord, o de `RackEnvelopeRestamp.cs`, `PushBackMirror.cs`, `WithDesign`, un
  drawer o builder existente o el registro de variables: **detenerse**; nueva revision de Arquitecto y ADR.
- Un adaptador necesita una costura del store distinta de `WithSourceMetadataFrom`: **detenerse** (§6.2).
- Una paralela modifica materialmente `RackDuplicationPlan.cs`, `RackEmbedComposer.cs`, `SystemBlockWriter.cs`,
  `LateralHeaderDrawer.cs`, `CantileverViewMaterializer.cs`, `RackEnvelopeRestamp.cs`, `BlockLibraryImporter.cs`,
  `DynamicRackSystemResolver.cs`, los builders de §8.1 o los stores authored: **detenerse** antes de editar y reportar
  SHA y diff.
- Aparece un campo nuevo direccionado por poste, fondo, modulo, lado o estacion sin regla de reflexion: **detenerse**.
- Aparece un parametro dinamico con semantica de lado sin regla: fail-closed y registro para V3.
- Debilitar una guarda de I-51 para facilitar el cambio: **prohibido**.

---

## 20. Archivos (sin cambiarlos en este gate)

### 20.1 Produccion probable (G4–G8)

| Capa | Archivo o area |
|---|---|
| Application | `Geometry/Transform2D.cs` (`ReflectionAboutLine`) y valor de colocacion junto a el |
| Application | `Persistence/RackDuplicationPlan.cs` (fachada, con aviso a I-49) + nucleo neutral nuevo en `Persistence/` |
| Application | carpeta nueva del espejo: contrato de reflector, registro, plan del espejo, decodificacion de vistas, equivalencia, declaracion tipada de simetrias |
| Application | `Systems/Selective/`, `Systems/Dynamic/`, `Systems/PushBack/`, `Systems/Cantilever/`, `RackFrames/`: reflector y autoridad de plan por kind |
| Plugin | `RackMirrorCommands.cs` (nuevo) y SNAPSHOT propio; materializador en `Systems/Shared/`; `Systems/Shared/SystemBlockWriter.cs` (`CreateInTransaction`) |
| Plugin | `RackDuplicarCommands.cs` **solo** si se extrae un SNAPSHOT compartido; comandos de `RACKEDITAR` **solo** si se activa C-1 |
| UI | `RackCommandReference.cs` (ayuda) |

### 20.2 Pruebas

Caracterizaciones CT-01..CT-26; pruebas T-M01..T-M30; guardas G-M1..G-M12; `SelectiveEditorOpenTests.cs` (censo 34);
guardas de I-51 reapuntadas en `SelectiveDuplicationFailClosedTests.cs` solo si se extrae el SNAPSHOT; pruebas de UI
en `tests/RackCad.UI.Tests` para C-2 y las caracterizaciones de UI.

### 20.3 NO TOCAR sin nueva revision de Arquitecto

`RackEnvelopeRestamp.cs`, `RackCloner.cs`, `PushBackMirror.cs`, `SelectivePalletDesign.cs`,
`SelectivePalletDesignDocument.cs` (incluido `WithDesign`), DTO y stores de todos los kinds, `RackProject.cs`,
`LateralHeaderDrawer.cs`, `CantileverViewMaterializer.cs`, `BlockLibraryImporter.cs`, builders de plan,
`ProjectVariables/*` salvo consumo de lectura, `Bom/*` salvo consumo, `KindHandlers/*`, editores WPF, `assets/`,
catalogos, biblioteca de bloques, `.github/`, ADR aceptados.

---

## 21. Estado

```text
G0 = ACCEPTED        G1 = ACCEPTED (Discovery)
G2 = V2 EN REVISION  (este documento + ADR-0036 propuesto corregido + docs/automation/decisions/I-52.md)
     V1 (0fc7032) = historial; Architect: CHANGES REQUIRED — PROPOSAL V2
G3+ = NO INICIADO    (requiere consenso sobre la misma version, O-1 y O-4 del Owner y orden del Coordinador)

Proposal Version = V2
Coordinator      = REVIEW REQUIRED
Architect        = REVIEW REQUIRED
Implementation   = BLOCKED
ADR-0036         = PROPOSED

SUBSTANTIVE IMPLEMENTATION: BLOCKED
```
