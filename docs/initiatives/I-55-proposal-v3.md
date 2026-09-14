# I-55 — Proposal V3: View Placement & Projection (ID17 + ID18 + ID19)

```text
PROPOSAL V3 — NOT CONSENSUS
Coordinator                         = REVIEW REQUIRED
Architect formal                    = PENDING (revision independiente pendiente: I-55-architect-review-package-v3.md)
Owner                               = PENDING (M-01, OD-1..OD-8, ADR-0042)
Consensus                           = NOT REACHED
Implementation                      = BLOCKED
Open Material                       = M-01 (propio) · adopcion de X-1..X-8 por I-52 (reconciliacion obligatoria)
Decision del Coordinador pendiente = CQ-01 (redibujo de Insertar: CD2-03 frente al primitivo de una transaccion, §22.3)
ADR-0042                            = PROPUESTO, COMPLEMENTARIO de ADR-0010 (que sigue ACEPTADO)

Estado de V2 (f84f303)              Coordinator = CHANGES REQUIRED → V3 · Architect formal = PENDING
                                    Revision tecnica adversarial de V2 (G2C, misma sesion que la redacto) = CHANGES REQUIRED

Initiative     = I-55 — View Placement & Projection
Branch         = feature/creacion-de-vistas
BASE_SHA       = ba497f14581d81e83a27514852d6ec082ff57635   (base original; codigo auditado por el Discovery)
CURRENT_BASE   = dad4e77f4f267b9fa248ecb0c8bfd8a74bbab093   (merge de I-53D)
CURRENT_MAIN   = dad4e77f4f267b9fa248ecb0c8bfd8a74bbab093   (preflight de G2D: main no avanzo)
Historial      = Proposal V1 d1918ab (Coordinator = CHANGES REQUIRED) · Proposal V2 f84f303 · paquetes V2 1e12414
                 revision tecnica G2C d091eeb (docs/initiatives/I-55-architect-review-v2.md)
Paralelas      = I-49 f6f0991 (Amendment A3) · I-52 dd45b0f (Proposal V13: autoridades compartidas provisionales, [V12-D11], [V13-D08])
                 · I-56 0d66df2 (G1.1, Evidence Audit)
Decisiones     = docs/automation/decisions/I-55.md   (G2B: CR-01..CR-12; G2D: CD2-01..CD2-12)
Mapa           = docs/initiatives/I-55-implementation-map-v3.md   (misma version del plan)
ADR            = docs/adr/0042-preparacion-de-vistas-antes-de-materializar.md
Citas          = archivo:linea sobre CURRENT_BASE, verificadas antes del commit (§23)
Prefijos       = P/ src/RackCad.Plugin/ · A/ src/RackCad.Application/ · D/ src/RackCad.Domain/ · U/ src/RackCad.UI/
                 T/ tests/RackCad.Tests/ · TU/ tests/RackCad.UI.Tests/
```

> **Como leer V3.** Documento autocontenido; V1 y V2 quedan como historial sin modificar. §0 dice que cambia y por que. **Vocabulario de
> ID19:** una **seleccion proyectada** es el conjunto de referencias aceptadas en **una** ejecucion de ID19, y un **grupo `RackId`** es el
> subconjunto que comparte un `RackId`. La transformacion comun es **una por seleccion proyectada**, nunca por grupo `RackId`.

## 0. Reconciliacion V2 → V3

### 0.1 Estado de las revisiones

| Pieza | Estado |
|---|---|
| Proposal V1 (`d1918ab`) | Coordinator = CHANGES REQUIRED (G2B; CR-01..CR-12 vinculantes) |
| Proposal V2 (`f84f303`) | Coordinator = **CHANGES REQUIRED → V3** (G2D). Architect formal = **PENDING** |
| Revision de V2 publicada en G2C (`d091eeb`) | **Revision tecnica adversarial**, hecha por la misma sesion que redacto V2: **CHANGES REQUIRED**. El Coordinador **acepta sus hallazgos como evidencia tecnica** y los incorpora aqui, pero **no** es el veredicto formal del Arquitecto |
| Proposal V3 (este documento) | Coordinator = REVIEW REQUIRED; Architect formal = PENDING, para un Arquitecto **independiente** con su propio paquete |

### 0.2 Lo que V3 conserva

- Todo lo aceptado de V1 y V2 que no cambia abajo: CR-01..CR-12 (registro G2B).
- ALT-B.
- ID19 en dos niveles con una `CommonTransform2D` por ejecucion.
- Gate de propiedades acotado al `RackId`.
- PR-1 y PR-2, sin H-13.
- ID18 con PREPARE ALL y commit por colocacion.
- `RackId` al aceptar la intencion.
- ADR-0042 complementario.
- Todas las recomendaciones A (Owner PENDING).
- La tabla de reconciliacion X-1..X-8 de G2C, que el Coordinador acepta del lado de I-55.

### 0.3 Prescripciones del Coordinador en G2D (vinculantes)

| # | Prescripcion | Resolucion en V3 | Seccion |
|---|---|---|---|
| CD2-01 | Estado de revisiones: V2 = CHANGES REQUIRED → V3; Architect formal = PENDING; la revision de G2C es tecnica adversarial | Registrado; paquete para Arquitecto independiente | §0.1; paquetes V3 |
| CD2-02 | Corregir `Orthographic`: la direccion destino se deriva de familia + tipo + variante destino, nunca de una familia implicita «rack» | `F_t(r)` por referencia; tabla de los ocho pares; pruebas de los seis pares exigidos e ida y vuelta | §12.5; mapa G14 |
| CD2-03 | Redibujo parcial: PREPARE ALL de las vistas nuevas → redibujo legacy con atomicidad por unidad → STOP en un fallo, sin vistas nuevas, redibujos confirmados permanecen, informe exacto; sin transaccion global; INV-AUTH-1 acotado | §11.1, §11.2; D-15; INV-AUTH-1, INV-RED-1; §14; §15 | §11; §13 |
| CD2-04 | Requisito estructural de bloques por pieza; discriminacion por tipo o contrato; fallo antes de escribir; compartido con X-4 | `LibraryBlockRequirement`; D-13; INV-BLK-1 | §4.6; §17.2 |
| CD2-05 | Una autoridad `Resolve` sin editor con resultado tipado, politica de legado, paridad en G3, custodia, namespace, gate y consumidores | `RackSystemResolution`; D-17; INV-RES-1; G7a | §4.5 |
| CD2-06 | Separar codec sintactico, disponibilidad y politica del consumidor | D-03 | §7.3 |
| CD2-07 | Contrato `Resolve` / `Plan` / `Prepare` y orden sin ciclo plan ↔ nombre; `UniqueBlockName` en el Plugin | Estrategia B (semilla); D-19 | §4.4 |
| CD2-08 | Incorporar los LOW AR2-07..AR2-20 sin reabrir decisiones | Matriz §0.4 | §0.4 |
| CD2-09 | Tabla X-1..X-8 aceptada del lado de I-55; contrastar con V12; tabla de adopcion para I-52; MATERIAL CONFLICT → STOP | Compatible con V12 y con V13 (publicada durante la redaccion); diferencias con las filas de G2C listadas; tabla de adopcion | §17.2, §17.2.1 |
| CD2-10 | ADR-0042 complementario; ADR-0010 sigue ACCEPTED; la nota fechada solo al aceptarse ADR-0042 | D-12; ADR-0042 revisado | §22.1; ADR-0042 |
| CD2-11 | Owner PENDING salvo instruccion explicita; M-01 OPEN; sin `Coordinator = AGREED` | Sin instruccion del Owner en G2D | §22.2, §22.3 |
| CD2-12 | Revision adversarial de 20 puntos antes del commit | §23 | §23 |

### 0.4 Matriz AR2-01..AR2-20 → V3

| AR2 | Hallazgo (revision tecnica de V2) | Resolucion en V3 | Seccion |
|---|---|---|---|
| AR2-01 | `Orthographic` rechazaba Cantilever → Planta | Direccion destino por familia + tipo + variante de cada referencia; tabla de verificacion | §12.5 |
| AR2-02 | `REDRAW_FAILED` sin redibujo parcial | Semantica de §11.1; payloads en PREPARE ALL; INV-AUTH-1 acotado; INV-RED-1 | §11.1; §13 |
| AR2-03 | Union de nombres no detecta piezas sin nombre | Requisito estructural por pieza | §4.6 |
| AR2-04 | Resolucion sin editor sin autoridad | `Resolve` compartido con resultado tipado; `UnsupportedLegacy` para lo que la paridad de G3 no explique (el ejemplo Dinamico de AR2-04 es inalcanzable, §4.5) | §4.5 |
| AR2-05 | Codec mezclado con disponibilidad; politicas en el codec | Codec sintactico, disponibilidad y politica separados | §7.3 |
| AR2-06 | Forma de las API y ciclo plan ↔ nombre | Contrato `Resolve`/`Plan`/`Prepare`; semilla; `DescribeView` y `AdmissibleViews` en X-2/X-8 | §4.4; §17.2 |
| AR2-07 | Predicado de superposicion inexacto | Interseccion por pares de intervalos sobre el eje comun, en VALIDATE | §12.5 |
| AR2-08 | Tramo por rack; origen interior del Cantilever | `[K_min, K_max]` por (sistema, tipo, variante); anclas por el tramo destino | §12.3; §12.5 |
| AR2-09 | INV-GRP-1 no garantiza el orden | INV-GRP-3 | §12.2; §13 |
| AR2-10 | «Anclas fuente» en PLANS; paso de angulo con OD-6.c = B | PLANS solo lo independiente de los puntos; proyeccion en PLACE; angulo junto a PICK | §12.1 |
| AR2-11 | SNAPSHOT incompleto; signo de la escala Z | Lista de datos del SNAPSHOT; `(−1,−1,+1)` = giro de π; `sz < 0` = reflexion → fallo | §12.1; §12.2; §12.8 |
| AR2-12 | Valor de colocacion solo en G15 | Consumo o creacion en G14 (`A/Geometry/`); Z fuera del valor | §12.2; mapa G14 |
| AR2-13 | Dos caracterizaciones del nucleo de seleccion | Una sola: CT-16 | §12.8; mapa G3 |
| AR2-14 | Enter tambien detiene la cola | Esc y Enter detienen la cola; OD-4 | §11.2; §22.2 |
| AR2-15 | Limpieza sin precisar | Solo sin referencias, best effort, registrada; purga declarada; Cantilever en `PlaceDefinition` | D-10 detalle; OM-21 |
| AR2-16 | `Id` en blanco evaluado distinto | `IsNullOrWhiteSpace`; tratamiento de miembros no redibujados | §9.2; RID-4 |
| AR2-17 | Columna `RACKEDITAR` generalizada; `RACKLISTA` | Politica por kind; alcance de «significado unico» | §7.3 C |
| AR2-18 | `CantileverViewKind` frente a «segundo enum»; renombre frente a la instantanea de I-52 | Enum de camara; reporte y reapunte si la instantanea existe | §7.1; ADR-0042 |
| AR2-19 | C-2 tras PR-2 si I-52 integra primero | `Plan` compartido con `PlantaVisibility`; fixture con brazos y tensores visibles | §17.2 X-6 |
| AR2-20 | Primitivo sin Z, presentacion ni politica de faltantes | Z y presentacion explicitas; requisito estructural por pieza | §17.2 X-4; §4.6 |

## 1. Contrato de producto (vinculante, CD-01..CD-04)

| ID | Capacidad | Resultado observable |
|---|---|---|
| **ID17** | FIRST-VIEW FREEDOM | Un rack nuevo empieza por **cualquier vista que su sistema realmente soporte** (§5) y despues recibe las demas **sin perder `RackId`, authored ni coherencia**. No se busca uniformidad (OQ-2) |
| **ID18** | MULTI-VIEW QUEUE / BATCH | **Varias vistas de UN rack** en un flujo, colocadas consecutivamente, **con el mismo `RackId`** |
| **ID19** | MULTI-RACK PROJECTION | **Varios racks existentes**, **una misma clase de vista**, **layout relativo preservado con UNA transformacion comun**, **cada rack con SU `RackId`**: no es `RACKDUPLICAR`, sin `NewRackId` ni re-estampado |

**Fuera de alcance:** produccion antes del consenso; H-01..H-12 de paso (salvo PR-1, PR-2); H-13; unicidad de
`(RackId, View, Section)` (OQ-4); Drive-In; cambiar `RACKDUPLICAR`, `RACKLAYOUT`, `RACKRELLENAR` o `RACKPROPIEDADES`; componentes
sueltos del Cantilever (H-03); reabrir ADR-0009/0034/0035/0039.

## 2. Principios P1..P21 frente a la evidencia

| P | Principio | Veredicto | Evidencia y precision |
|---|---|---|---|
| P1 | Un rack logico no es una vista | ADOPTADO | Definiciones con el mismo `Id` |
| P2 | Una vista fisica no es un rack logico | ADOPTADO CON PRECISION | BOM con la primera hermana como representante (`A/Bom/BomAuthoredAuthority.cs:104-109`); `EditCama` redibuja solo la elegida (`P/RackCamaCommands.cs:226-237`): Cama fuera de ID18/ID19 |
| P3 | `RackId` identifica el rack | ADOPTADO CON PRECISION | `RackEmbedDocument.Id` (ADR-0009). El `Id` interior del Cantilever (`D/Systems/Cantilever/CantileverLineDesign.cs:333`) no lo lee ninguna ruta de I-55: H-13 queda fuera (CR-07) |
| P4 | `View` describe Frontal/Lateral/Planta | ADOPTADO CON PRECISION | La Cama persiste `View` nulo (`P/RackCamaCommands.cs:197-198`) |
| P5 | `Section`/Variant describe la representacion | ADOPTADO | §7 |
| P6 | Sin segundo enum si `DimensionViewKind` sirve | ADOPTADO CON PRECISION | Ya es «el TIPO de vista» (`A/Systems/Shared/DimensionViewPolicy.cs:6-16`): se renombra |
| P7 | Una hermana conserva `RackId` | ADOPTADO | Con la curacion vigente de un `Id` en blanco |
| P8 | ID19 conserva los `RackId` originales | ADOPTADO | §12 |
| P9 | ID19 sin re-estampado, `NewRackId` ni `RackCloner` | ADOPTADO | Solo el nucleo neutral de seleccion (X-1) |
| P10 | Authored y effective distintos | ADOPTADO | §8 |
| P11 | Nunca reconstruir authored desde geometria | ADOPTADO | `P/RackSelectivoCommands.cs:249-258`, `:411-425` |
| P12 | `ProjectVariableReference` sobrevive | ADOPTADO | Authored verbatim |
| P13 | Expresiones de I-49 sobreviven | ADOPTADO | Idem |
| P14 | `DimensionViews` sobrevive | ADOPTADO | En el diseno |
| P15 | `CustomProperties` sobreviven | ADOPTADO CON PRECISION | Una hermana nueva nace con la coleccion canonica comun de su `RackId`, o no nace (§9) |
| P16 | `ExtensionData` y `SchemaVersion` sobreviven | ADOPTADO CON PRECISION | Heredar el `ExtensionData` de otra vista es vigente (OM-14) |
| P17 | `RACKLISTA` y `RACKBOMTOTAL` cuentan racks | ADOPTADO | Por `Id` |
| P18 | Vistas no agregan cantidad logica | ADOPTADO CON PRECISION | Copias = maximo de referencias (`P/RackInventarioCommands.cs:68-72`; `P/RackInventarioCommands.BomTotal.cs:109-113`) |
| P19 | Sin geometria nueva | ADOPTADO | ID19 calcula colocaciones |
| P20 | Sin Drive-In | ADOPTADO | `U/RackMainMenuWindow.xaml:139-144` |
| P21 | Sin arreglar H-01..H-12 de paso | ADOPTADO CON PRECISION | PR-1 y PR-2 aislados antes de la foundation (CD-07) |

## 3. Linea base que la Proposal cambia

| Aspecto | Hoy | Consecuencia |
|---|---|---|
| Primera vista | Selectivo solo Frontal; Dinamico y Cabecera solo Lateral | ID17 abre tres puertas |
| Vistas por gesto | Una | ID18 |
| Variante | Prompt o ventana; Push Back por posicion de lista (H-01) | Variante tipada |
| Payload | `Build*Payload` en el Plugin; siete `Compose` (`TGrd02`) | A Application; censo reapuntado con motivo |
| Resolucion sin editor | Privada en cada handler del BOM (`P/KindHandlers/SelectiveKindHandler.cs:36-68`; `P/KindHandlers/DynamicKindHandler.cs:37-44`; `P/KindHandlers/PushBackKindHandler.cs:40-67`; `P/KindHandlers/CantileverKindHandler.cs:47-67`; `P/KindHandlers/CabeceraKindHandler.cs:35-37`); el handler solo expone BOM y motivo de bloqueo (`P/KindHandlers/IRackKindHandler.cs:50`, `:62`) | Una autoridad compartida `Resolve` con resultado tipado (§4.5, D-17) |
| Materializacion | Definicion confirmada antes del jig (`P/Systems/Shared/SystemBlockWriter.cs:18-40`); el jig solo fija `Position` (`P/Drawing/BlockPlacement.cs:240-244`); limpieza al cancelar (`:37-42`, `:64-76`); el drawer anota las piezas sin bloque como faltantes (`P/Drawing/LateralHeaderDrawer.cs:293-300`) y `PlaceAndReport` las informa (`P/Drawing/BlockPlacement.cs:112-113`), pero varios caminos de insercion y los redibujos de edicion no imprimen faltantes (`P/RackSelectivoCommands.cs:572-575`, `:179-183`; `P/RackDinamicoCommands.cs:349-357`; `P/RackCabeceraCommands.cs:302-313`), y algunos builders omiten la pieza antes del plan si su fila de catalogo no tiene bloque (§4.6) | Limpieza ante excepcion (D-10); requisito estructural de bloques por pieza y reporte en todo camino de insercion de ID17/ID18 (§4.6, D-13); rotacion calculada en ID19 |
| Vista enlazada | Redibujo por vista, cada uno con su propia confirmacion (`P/Systems/Shared/SystemBlockWriter.cs:55-66`), y fallos ignorados (`P/RackSelectivoCommands.cs:178-183`; `P/RackCantileverCommands.cs:328-332`); en Insertar, el prompt de variante llega despues de los redibujos (`P/RackSelectivoCommands.cs:241-258` → `:476-489`) y las huerfanas se conservan si son las unicas vistas (`:229-239`) | Variante → gate → PREPARE ALL → redibujo legacy → colocar; un redibujo fallido detiene la insercion sin deshacer los ya confirmados (D-15); las huerfanas se borran con la primera colocacion (§11.1) |
| Hermanas | Propiedades copiadas sin comparar | Gate acotado al `RackId` |
| Multi-rack | No existe | ID19 |

## 4. Arquitectura propuesta (ALT-B, D-01)

### 4.1 Responsabilidades

```text
Authored persistido      sobre + payload de cada vista (ADR-0009, ADR-0039)
        ▼
Resolve (puro)           authored (persistido o en memoria) + lectura del registro (+ expresiones futuras) + catalogos → resultado tipado (§4.5)
        ▼                (el editor entrega su sistema ya resuelto y NO pasa por aqui)
Codec sintactico (puro)  (View, Section) ↔ RackViewAddress + disposicion sintactica (§7.3 A)
        ▼
Disponibilidad (pura)    (sistema resuelto, catalogo, direccion) → disponible | huerfana | no soportada (§7.3 B)
        ▼
Politica del consumidor  RACKEDITAR legacy · ID17/ID18 · ID19 · I-52 (§7.3 C)
        ▼
Gates de hermanas        propiedades comunes del RackId + authored equivalente (§9)                 ← rack existente
        ▼
Prepare (puro)           semilla de nombre → Plan(sistema, direccion, contexto con semilla) → sobre → requisitos de bloque (§4.4, §4.6)
        ▼
Colocacion (Plugin)      importar → verificar requisitos → definicion + sobre (nombre unico) → jig o colocacion calculada → confirmar o limpiar

Lote (ID18)              RackViewBatchPlan (puro): PREPARE ALL → redibujo legacy → cola (§11)
Colocacion de grupo      RackGroupPlacementPlan (puro): marcos e intervalos (FRAMES), CommonTransform2D, politica, anclas (§12)
```

| Capa | Hace | No hace |
|---|---|---|
| Resolve | Leer authored, variables de proyecto, expresiones futuras, resolvers del sistema, diagnosticos, reglas de bloqueo de salida, politica de legado | Abrir el editor, dibujar, decidir la politica de un consumidor, aproximar un legado |
| Codec sintactico | Traducir tokens y secciones a una direccion tipada y clasificar su forma | Preguntar al sistema resuelto; aplicar politicas |
| Disponibilidad | Decir si el sistema resuelto tiene esa direccion y si el kind la soporta | Clasificar tokens; aplicar politicas |
| Prepare | Semilla de nombre, plan desde el sistema resuelto, sobre, requisitos de bloque | Colocar, abrir transacciones, inventar identidad, reconstruir authored, conocer AutoCAD, hacer I/O, re-resolver lo que el editor entrego, fijar el nombre unico |
| Colocacion | Importar, verificar, crear con nombre unico (`UniqueBlockName`), escribir, colocar, limpiar | Decidir soporte, variante, identidad, autoridad o colocaciones de grupo |
| Colocacion de grupo | Application: marcos, transformacion comun, anclas. Plugin: seleccion, puntos, lectura, materializacion | Pedir un punto por rack; calcular transformaciones por rack |
| Builders | Unica autoridad geometrica | Duplicarse |

**Una autoridad por responsabilidad (D-14):** X-1..X-8 con I-52 (§17.2).

### 4.2 Tipos (nombres no contractuales; responsabilidades si)

Namespace neutral `RackCad.Application.Views` (la resolucion en `RackCad.Application.Views.Resolution`).

| Tipo | Responsabilidad |
|---|---|
| `RackViewKind` | Unica taxonomia `Frontal/Lateral/Planta` (renombra `DimensionViewKind`) |
| `RackViewVariant` | `Whole`, `Fondo`, `Post`, `FlowEnd`, `PushBackCut`, `Station`; nunca un indice de UI |
| `RackViewAddress` | `(RackViewKind, RackViewVariant)` |
| `RackViewAddressCodec` | Solo sintaxis: `Encode`; `Decode(kind, View, Section) → (direccion, disposicion sintactica)` (§7.3 A) |
| `RackViewAvailability` | `(sistema resuelto, catalogo, direccion) → Available \| Orphaned \| Unsupported` con diagnostico (§7.3 B) |
| `RackViewSupport` | Matriz normativa (§5): soporte por kind y consumidor, variante canonica, pares y modos representables |
| `RackSystemResolution` | `Resolve` sin editor con resultado tipado (§4.5) |
| `RackViewNameSeed` | Semilla semantica de nombre de definicion, calculada antes del plan (§4.4) |
| `RackViewPlanner` | `Plan(sistema resuelto, direccion, contexto) → plan` con todas las direcciones (§4.4) |
| `LibraryBlockRequirement` | Requisito estructural de bloque por pieza y su verificacion, en `RackCad.Application.Drawing` junto al contrato del rol (§4.6, X-4) |
| `RackViewFrame` | Por `(sistema, tipo, variante)`: ejes locales ↔ ejes fisicos R/D/H con signo, origen fisico local y tramo `[K_min, K_max]` por eje (§12.3) |
| `RackEnvelopeIdProbe` | Lee de forma tolerante el `Id` de un payload que el store no interpreta (§9.2) |
| `RackSiblingCustomPropertiesGate` / `RackSiblingAuthoredGate` | Gates acotados al `RackId`, en archivos separados (§9) |
| `IRackViewPreparer` y compania | `Prepare` por sistema sobre `KindDispatch<T>` (`A/Persistence/KindDispatch.cs:19`) |
| `RackViewBatchPlan` | ID18 (§11) |
| `SourceGroupFrame`, `TargetGroupFrame` | Marco 2D (origen, orientacion) fuente y destino de **la seleccion proyectada** |
| `CommonTransform2D` | **Una** transformacion rigida por seleccion proyectada, sobre `Transform2D` (`A/Geometry/Transform2D.cs:20-116`), con `Determinant = +1` y `ScaleFactor = 1` |
| `RackGroupAnchorPolicy` | `Rigid` u `Orthographic`: anclas fuente, angulo comun y orientacion destino |
| `RackGroupPlacementPlan` | Grupos, clasificacion, resolucion, disponibilidad, gates, marcos, transformacion, colocaciones, avisos |

### 4.3 Preparacion por sistema (builders vigentes)

| Sistema | Direcciones | Plan (desde el sistema resuelto) | Payload |
|---|---|---|---|
| Selectivo | Frontal `Fondo(k)`, Lateral `Post(p)`, Planta | `SelectiveFrontalBuilder.BuildPlan(SelectiveDepthLayout.FondoSystemView(system, k), catalog)`; corte de `SelectiveLateralBuilder.Cortes` + `LateralHeaderParametersFactory` + `LateralHeaderLayoutBuilder.Build` + fusion de `corte.Largueros` en el layout antes de agrupar (`P/Drawing/LateralHeaderDrawService.cs:43-45`, `:198-208`) + `HeaderInstanceGrouper.Group(instancias, semilla)`; `SelectivePlantaBuilder.BuildPlan(system, catalog)` | authored verbatim |
| Dinamico | Lateral `Post(p)`, Frontal `FlowEnd(e)`, Planta | `DynamicSystemLateralBuilder.Build(system, catalog, postIndex)`; `DynamicSystemFrontalBuilder.BuildPlan(system, catalog, end)`; `DynamicSystemPlantaBuilder.BuildPlan(system, catalog)` | `RackProject.ForDynamic(design)` con metadata interior |
| Push Back | Lateral `Post(p)`, Frontal `PushBackCut(e, s)`, Planta | `PushBackSystemLateralBuilder.Build(system, catalog, postIndex)`; `PushBackSystemFrontalBuilder.BuildPlan(system, catalog, end, side)`; `PushBackSystemPlantaBuilder.BuildPlan(system, catalog)` | `RackProject.ForPushBack(design)` con metadata interior |
| Cantilever | Frontal, Lateral `Station(s)`, Planta | `CantileverViewPlanBuilder.Build(line, kind, factory, station, design.PlantaVisibility)` (PR-2; §17.4) | `RackProject.ForCantilever(design)`; `Id` interior sin tocar |
| Cabecera | Lateral, Planta | `HeaderInstanceGrouper.Group(LateralHeaderLayoutBuilder.Build(...).Instances, semilla)`; `PlantaHeaderLayoutBuilder.Build(config, catalog)` | `RackProject.ForSelective(configuration)` con metadata interior |
| Cama | Lateral | `FlowBedLateralBuilder.Build(config, catalog)` | `FlowBedDocument` con version y extension data |

### 4.4 Contrato de las API compartidas (D-19)

Los nombres no son contractuales; las responsabilidades y el orden si.

```text
Resolve(authored, contexto)                         → RackSystemResolution            (§4.5)
  authored = diseno interior en el sustrato de su kind, persistido (ID19, BOM) o en memoria (I-52: el diseno reflejado)
  contexto = catalogos cargados · RESULTADO de la lectura del registro de variables (incluido ilegible o ausente)
             · lectura de expresiones (cuando I-49 integre)

Seed(rackName, direccion, sistemaResuelto, catalogo o configuracion) → RackViewNameSeed puro, determinista

Plan(sistemaResuelto, direccion, contextoDePlan)    → RackViewPlan
  contextoDePlan = catalogos · semilla de nombre · rackName (anotacion) · politica de cotas del diseno
                   · visibilidad de planta del diseno (Cantilever)
  RackViewPlan   = familia (HeaderRun | CantileverCurves) · plan · RackViewFrame de la direccion (§12.3)
                   · requisitos de bloque (§4.6) · nombre sugerido (= la semilla; el nombre unico lo fija el Plugin)

Prepare(...)                                        = Resolve si hace falta (ID19; nunca en el editor)
                                                    + Disponibilidad (§7.3 B) + politica del consumidor (§7.3 C)
                                                    + Seed + Plan + composicion del sobre (Compose, TGrd02)
                                                    + metadatos de nombre
```

**Orden y ciclo plan ↔ nombre.** Hoy el plan solo usa el nombre como **prefijo** de las definiciones anidadas que genera la
agrupacion (`A/Drawing/HeaderInstanceGrouper.cs:21`, `:96`). El nombre **unico** lo fija el Plugin al crear cada definicion, tanto la del
sistema como las anidadas (`P/Drawing/LateralHeaderDrawer.cs:28-80`, `:240-247`).

**Estrategia elegida: B, semilla de nombre pasada al planificador.**
- La semilla **reproduce byte a byte** la funcion de nombre base vigente de cada kind, antes del plan y sin tocar el dibujo. Esa funcion
  depende del nombre del rack, de la direccion, del sistema, del catalogo o la configuracion y de valores por defecto:
  - sufijo «frente F{n}» solo con mas de un fondo (`P/RackSelectivoCommands.cs:497`, `:503-511`); formato «RACK 1 - lateral 3»;
  - nombres de los servicios de dibujo (`P/Systems/Selective/SelectiveFrontalDrawService.cs:76-87`;
    `P/Systems/PushBack/PushBackSystemDrawService.cs:62-75`; `P/Systems/Dynamic/DynamicSystemDrawService.cs:61-72`;
    `P/Systems/FlowBed/FlowBedDrawService.cs:48-59`);
  - cabecera desde catalogo y configuracion (`P/Drawing/LateralHeaderDrawService.cs:46`, `:293-308`);
  - valores por defecto «Selectivo» y «Dinamico» (`P/RackSelectivoCommands.cs:568-569`; `P/RackDinamicoCommands.cs:339-340`);
  - prefijo y saneado del Cantilever (`P/Drawing/Cantilever/CantileverViewMaterializer.cs:287-312`); su sufijo `_2` es del nombre unico
    del Plugin (`:318-336`), no de la semilla.

  G3 caracteriza todas estas funciones (`LegacyViewBlockNameCharacterizationTests`); una semilla que cambie un nombre es un cambio
  observable.
- En un redibujo, la semilla es el nombre de la definicion existente, que el Plugin lee y pasa (`P/Drawing/LateralHeaderDrawService.cs:83`).
- `Plan` usa la semilla solo como prefijo; ninguna geometria depende de ella.
- `UniqueBlockName` sigue en el Plugin y se aplica al materializar.

No hay ciclo: semilla → plan → nombre unico. A (calcular un `baseName` antes del plan) es el mismo contrato con otro nombre; B lo hace
explicito en la firma y sirve igual a I-52, que ya recibe el nombre como entrada (V13 §8.1).

### 4.5 Autoridad de resolucion sin editor (D-17)

**Una sola autoridad** convierte un diseno authored en su sistema efectivo resuelto sin abrir el editor.

| Responsabilidad | Contenido |
|---|---|
| Leer authored | Store del kind; payload ilegible → `Unreadable`. La entrada puede ser tambien un diseno en memoria en su sustrato (I-52 resuelve el diseno reflejado) |
| Variables de proyecto | El contexto lleva el **resultado** de la lectura del registro, incluido ilegible o ausente. Resolucion acreditada, hoy solo Selectivo (`A/Systems/Selective/SelectiveEffectiveDesignResolver.cs:51`); referencia rota → `BrokenReference`. El BOM conserva su mapeo vigente de registro nulo a ausente (`:38-45`) |
| Expresiones futuras | Consumo del `PlanReadSet` de I-49 cuando integre; hasta entonces no aplica |
| Resolvers del sistema | Los vigentes por kind (tabla siguiente) |
| Diagnosticos y reglas de bloqueo de salida | `RackBomOutputGate.For` (`A/Bom/RackBomOutputGate.cs:49`) y validez de la linea Cantilever → `Blocked` con motivo |
| Dependencias | Catalogo de secciones del Cantilever no disponible (`P/KindHandlers/CantileverKindHandler.cs:60`) → `DependencyUnavailable` |
| Politica de legado | Payload cuyo sistema resuelto no reproduce lo que se dibujo, o que `RACKEDITAR` no reconstruye de forma equivalente → `UnsupportedLegacy`, con lo que ese legado aporte |

```text
RackSystemResolution = Resolved(sistema)
                     | UnsupportedLegacy(motivo, sistemaLegado?)
                     | Blocked(motivo)
                     | BrokenReference(propiedad, variable)
                     | DependencyUnavailable(motivo)
                     | Unreadable(motivo)
```

| Kind | Resolucion vigente (camino del BOM) | Aceptacion de `RACKEDITAR` | Legado |
|---|---|---|---|
| Selectivo | store → `SelectiveEffectiveDesignResolver.Resolve(authored, variables)` → `SelectiveGeometryResolver.Resolve` (`P/KindHandlers/SelectiveKindHandler.cs:43`, `:58`, `:68`) | store → `SelectiveEditorOpen.Resolve(saved, registro)` (`P/RackSelectivoCommands.cs:54`, `:67`) | Sin legado distinto conocido; paridad en G3 |
| Dinamico | `RackProjectStore` → `DynamicRackSystemResolver(catalog).Resolve(DynamicDesign).System` (`P/KindHandlers/DynamicKindHandler.cs:37-44`). El registro exige `DynamicSystem` y **siempre** reconstruye `DynamicDesign` con `ToDesign` (`A/Systems/Shared/SystemRegistry.Default.cs:74-87`; `A/Persistence/DynamicRackSystemDocument.cs:196`), asi que la rama del handler sin diseno (`:40-41`) es inalcanzable desde el store | Preflight interior de todas las hermanas (`P/RackDinamicoCommands.cs:186-191`) y `DynamicDesign`, siempre presente (`:156-160`) | Documento solo con sistema: su diseno se rellena con valores por defecto (p. ej. `LegacyDefaultBeamDepth`, `A/Persistence/DynamicRackSystemDocument.cs:210`), igual en BOM y editor, pero el sistema resuelto puede diferir de lo dibujado. Paridad de G3 **obligatoria** con un fixture solo-sistema; diferencia no explicada → `UnsupportedLegacy` |
| Push Back | `RackProjectStore` → `PushBackResolver(catalog).Resolve(PushBackDesign)`; bloqueo por `RackBomOutputGate.For` (`P/KindHandlers/PushBackKindHandler.cs:42-48`, `:66-67`) | Todas las hermanas Push Back con el mismo `Id` y descriptor valido, o aborta la edicion entera (`P/RackPushBackCommands.cs:194-203`, `:216-223`); exige `PushBackDesign` (`:154-157`) | Sin legado distinto conocido |
| Cantilever | `RackProjectStore` → catalogo de secciones → `CantileverLineEditorAssembler(sections).Build(CantileverLineDesign)`; linea invalida excluida (`P/KindHandlers/CantileverKindHandler.cs:49-67`) | Todas las hermanas Cantilever con el mismo `Id` y descriptor valido, o aborta (`P/RackCantileverCommands.cs:243-252`, `:264-271`); exige `CantileverLineDesign` (`:204`) | Sin legado distinto conocido |
| Cabecera | `RackProjectStore.Deserialize(...).Header`; el modelo fisico se regenera en el registro (`A/Systems/Shared/SystemRegistry.Default.cs:51`) o, en la ruta de cabecera legacy, en el store (`A/Persistence/RackProjectStore.cs:334`) (`P/KindHandlers/CabeceraKindHandler.cs:37`) | Preflight interior de las hermanas (`P/RackCabeceraCommands.cs:250-256`); exige `Header` (`:216`) | Sin legado distinto conocido |
| Cama | `FlowBedConfigurationStore` → `FlowBedLateralBuilder` (`P/KindHandlers/CamaKindHandler.cs:37-47`) | Solo vista unica | Sin legado distinto conocido; ID19 no la soporta (§5) |

- **`UnsupportedLegacy` no tiene hoy un miembro conocido alcanzable.** La revision tecnica de V2 tomo como ejemplo el `DynamicSystem` sin
  `DynamicDesign`; el registro lo hace inalcanzable (arriba). La categoria existe para lo que la paridad de G3 encuentre, empezando por el
  documento Dinamico solo-sistema.
- **Politica por consumidor.**
  - BOM, **por kind y sin cambio observable**: `Resolved` cotiza como hoy. Push Back `Blocked` alimenta `OutputBlockedReason` y aborta el
    total como hoy (`P/RackInventarioCommands.BomTotal.cs:175-190`). Cantilever `Blocked` se traduce al salto vigente de payload
    inutilizable con aviso (`:209-215`; su `OutputBlockedReason` es nulo). `BrokenReference` aborta como hoy (`:200-206`).
    `UnsupportedLegacy` cotiza como hoy, con el sistema que obtiene el handler vigente (`sistemaLegado`). Los demas se saltan con aviso
    como hoy.
  - ID19: **solo `Resolved`**; todo lo demas falla cerrado con el motivo, antes de pedir puntos, sin aproximar nunca una reconstruccion.
    Ademas, un rack cuyo barrido contiene una hermana que el preflight de `RACKEDITAR` rechazaria (otro kind, otro `Id`, descriptor
    invalido o interior incompatible) falla cerrado: el remedio «Abrela con RACKEDITAR» tiene que poder funcionar.
  - I-52: su politica (V13 §7.9: PRE-06, PRE-07 y PRE-08 en §9.1 P3/P4; PRE-11 en SNAPSHOT/P1 con E2). `UnsupportedLegacy` no tiene
    correspondencia en V13; la tabla de adopcion propone E6 (§17.2.1).
- **Paridad.** G3 caracteriza, por kind, que resolver el payload persistido da el sistema que se dibujo al insertar y el que obtiene el
  editor al abrirlo, con fixtures legacy (el Dinamico solo-sistema entre ellos). Una diferencia no explicada deja ese caso fuera de ID19
  hasta una decision posterior. La reconstruccion no idempotente del editor Dinamico (H-14) no es paridad del sistema persistido y se
  registra aparte.
- **Custodia y extraccion.** Custodio I-55 (infraestructura de vistas, X-2), en `A/Views/Resolution/`. **La extrae I-55 G7a**, y en ese
  mismo gate los handlers del BOM de Selectivo, Dinamico, Push Back, Cantilever, Cabecera y Cama delegan en ella sin cambio observable
  (BOM y motivos de bloqueo caracterizados en G3), para que no queden dos autoridades. I-52 no puede extraerla sin excepcion: su lista
  de archivos excluidos incluye `KindHandlers/*` (V13 §20.3) y sus gates G4..G6 no escriben en `A/Views`. Si I-52 necesita `Resolve` antes
  de que I-55 G7a exista en su base, aplica el STOP de X-2 con sus opciones (a) y (b) (§17.2).
- **Consumidores.**
  - I-55: ID19 (G14, G15).
  - I-52: precondiciones P3/P4 en G5 y `RackViewPlanAuthority.Build` = `Resolve` + `Plan` en G6 (V13 §8.1, que ya lo registra; C2-1 intacto).
  - BOM: los handlers.

### 4.6 Requisito estructural de bloques de biblioteca (D-13)

Verificar la **union de nombres** no basta:
- una pieza sin nombre desaparece de la union;
- la importacion descarta nombres en blanco (`P/Drawing/BlockLibraryImporter.cs:44-47`) e importa todos los demas;
- el drawer anota la pieza como faltante (`P/Drawing/LateralHeaderDrawer.cs:293-300`), pero varios caminos de insercion y todos los
  redibujos de edicion no imprimen faltantes (§3);
- algunos builders omiten la pieza **antes del plan** cuando su fila de catalogo no tiene bloque, incluidas piezas del BOM
  (`A/Systems/Selective/SelectiveLateralBuilder.cs:287-290`, `:393-396`; `A/Systems/PushBack/PushBackLoadBeamGeometry.cs:139-143`; mas
  de veinte sitios), mientras otros emiten la instancia con nombre nulo.

```text
Para CADA instancia del plan:
  requiere bloque de biblioteca  ⇔  su rol NO es Annotation ni Dimension
                                    (contrato del rol, A/Drawing/HeaderBlockInstance.cs:7-50; el drawer los dibuja como DBText
                                    y RotatedDimension, P/Drawing/LateralHeaderDrawer.cs:258-291)
  planes CantileverCurves        ⇒  ninguna instancia requiere bloque (curvas)
  si requiere bloque:
    1. BlockName declarado, no nulo, no vacio, no solo espacios y valido segun el predicado de nombre de tabla de bloques
       (definido en Application y caracterizado en G3; hoy hay dos saneadores distintos, A/BlockNaming.cs:19-28 y
       P/Drawing/Cantilever/CantileverViewMaterializer.cs:298-312)
    2. esa clave entra en la union importable
    3. tras importar, la clave existe en la tabla de bloques
    4. la pieza conserva su trazabilidad a esa clave (PieceId, rol, vista) para el diagnostico
  1 y 2 son puros: se evaluan al preparar (en ID19, en PLANS, antes de pedir puntos)
  3 se evalua tras IMPORT
  fallo en 1, 2 o 3  ⇒  FALLO ANTES DE ESCRIBIR (ID19 e I-52): ninguna definicion ni referencia de rack; las definiciones de
                        biblioteca ya importadas pueden quedar (OM-5)
```

- **Discriminacion.** Una pieza de anotacion o de cota se reconoce por su **rol**, nunca por tener el nombre vacio. Un rol que requiere
  bloque con el nombre vacio es un error del plan, no una pieza sin bloque.
- **Alcance.** La verificacion ve las instancias **presentes en el plan**. Las piezas que un builder omite antes del plan por falta de
  bloque en el catalogo no llegan a ella: G3 hace el censo de esos sitios y se decide por rol si se convierten en instancia con nombre
  nulo (y fallan) o se declaran opcionales, como la tarima visual (OM-22). Hasta esa decision, ID19 no promete que ninguna pieza del
  catalogo falte en silencio.
- **Que prueba la comprobacion 3.** Presencia en la tabla de bloques, no procedencia de la biblioteca ni parametros dinamicos: un bloque
  del dibujo con el mismo nombre gana (`P/Drawing/BlockLibraryImporter.cs:58-64`). El diagnostico distingue «biblioteca no disponible»
  (`:74-78`, `:113-118`) de «bloque ausente».
- **Politica por consumidor.**
  - ID19 e I-52: fallo antes de escribir, con la lista de piezas (PieceId, rol, vista, clave) en orden estable.
  - ID17 e ID18: conservan la politica vigente (colocan y reportan faltantes), pero **todo camino de insercion** que usan imprime el
    reporte con esta misma verificacion (guarda de G8 con los sitios de §3). Cambiarlos a fallo antes de escribir seria un cambio de
    producto y no se hace sin decision (OM-20).
- **Autoridad compartida con I-52 (X-4).** Una sola verificacion: la parte pura (requisitos por pieza y predicado de nombre) en
  `A/Drawing/LibraryBlockRequirement.cs`, y la consulta a la tabla de bloques tras importar, en `P/Systems/Shared/`. La parte pura la
  extrae el primero de I-55 G7b o I-52 G6, y la consulta, el primero de I-55 G8 o I-52 G6 (§17.2).

## 5. Matriz normativa sistema × ViewKind × variante

**«Vista soportada»:** (a) builder, (b) codec + edicion que la reconoce y redibuja, (c) existe en el sistema resuelto.

| Sistema | ViewKind | Variante | CanStartWith | CanAddLater | CanBatch | ID19 (foundation) | VariantSelectionOwner | Canonica ID19 |
|---|---|---|---|---|---|---|---|---|
| SelectiveRack | Frontal | `Fondo(k)` | **SI** | SI | SI | SI | Editor (lote); prompt legacy | `Fondo(0)` |
| SelectiveRack | Lateral | `Post(p)`, `p ∈ Cortes(system).PostIndex` | **SI (nuevo)** | SI | SI | SI | Editor (lote); prompt legacy | `Post(min p)` |
| SelectiveRack | Planta | `Whole` | **SI (nuevo)** | SI | SI | SI | — | `Whole` |
| PalletFlow | Lateral | `Post(p)` | SI | SI | SI | SI | Editor (lote); prompt legacy | `Post(min p)` |
| PalletFlow | Frontal | `FlowEnd(Exit \| Entrance)` | **SI (nuevo)** | SI | SI | SI | Editor | `FlowEnd(Exit)` |
| PalletFlow | Planta | `Whole` | **SI (nuevo)** | SI | SI | SI | — | `Whole` |
| PushBack | Lateral | `Post(p)` real | SI | SI | SI | SI | Editor (tras PR-1) | `Post(min p)` |
| PushBack | Frontal | `PushBackCut(EntradaSalida \| Posterior, A \| B)` | SI | SI | SI | SI | Editor | `PushBackCut(EntradaSalida, A)` |
| PushBack | Planta | `Whole` | SI | SI | SI | SI | — | `Whole` |
| Cantilever | Frontal | `Whole` | SI | SI | SI | SI | — | `Whole` |
| Cantilever | Lateral | `Station(s)` | SI | SI | SI | SI | Editor | `Station(0)` |
| Cantilever | Planta | `Whole` (PR-2) | SI | SI | SI | SI | — | `Whole` |
| Selective (Cabecera) | Lateral | `Whole` | SI | SI | SI | SI | — | `Whole` |
| Selective (Cabecera) | Planta | `Whole` | **SI (nuevo)** | SI | SI | SI | — | `Whole` |
| Cama | Lateral | `Whole` (`View` nulo) | SI (unica) | **NO** | **NO** | **NO** | — | — |
| Larguero | — | sin vista AutoCAD | NO | NO | NO | NO | — | — |
| Drive-In | — | no existe | — | — | — | — | — | — |

«ID19 (foundation) = SI»: la colocacion de grupo puede representar esa vista como destino; **que pares, modos y variantes expone V1 lo
decide M-01** (§12.7). La «Canonica ID19» aplica a pares de clase distinta; en misma clase, OD-2.b.

## 6. Ciclo de vida del `RackId` (D-05)

| Autoridad | Cuando acuna | Evidencia |
|---|---|---|
| `RackEditorIdentity.EnsureId` (Selectivo, Dinamico, Push Back, Cantilever, Cama) | `RequestInsert`/`RequestUpdate` | `U/Editor/RackEditorSession.cs:126`; `U/Editor/RackEditorIdentity.cs:45-53`; `U/Systems/FlowBed/RackFlowBedWindow.xaml.cs:36-37`, `:276-278` |
| `SaveToLibrary` (Cantilever, Push Back) | tambien al guardar | `U/Systems/Cantilever/RackCantileverWindow.xaml.cs:1363`; `U/Systems/PushBack/RackPushBackSystemWindow.xaml.cs:3404` |
| `SaveToLibrary` (Selectivo) | no | `U/Systems/Selective/RackSelectiveWindow.xaml.cs:3279-3281` |
| Comandos sin sesion | por llamada de dibujo | `P/RackCabeceraCommands.cs:164-176`; `P/RackCamaCommands.cs:102`; `P/RackDinamicoCommands.cs:73` |
| Edicion con `Id` en blanco | Selectivo, Dinamico, Push Back, Cantilever curan; Cabecera acuna uno nuevo | `P/RackSelectivoCommands.cs:119`; `P/RackDinamicoCommands.cs:174`; `P/RackPushBackCommands.cs:179`; `P/RackCantileverCommands.cs:230`; `P/RackCabeceraCommands.cs:233` (H-08) |

**Decision propuesta: B** (acunar al aceptar la intencion): A deja GUIDs sin uso; C obliga al Plugin a inventar identidad.

- **RID-1.** Un rack nuevo recibe su `RackId` una vez, al aceptar la intencion; sin sesion, el comando acuna una vez por intencion.
- **RID-2.** Todas las vistas de esa aceptacion lo llevan; toda ruta de entrada reenvia la lista de vistas (§11.2).
- **RID-3.** Una hermana lleva el `RackId` del sobre elegido o, en ID19, el de su grupo `RackId`.
- **RID-4.** `RACKEDITAR` cura un `Id` en blanco como hoy; ID19 falla cerrado ante un sobre sin `Id`. En I-55, «en blanco» es `IsNullOrWhiteSpace` (§9.2).
- **RID-5.** Cancelar antes del primer placement no deja ninguna definicion ni referencia de rack; las definiciones de biblioteca ya
  importadas pueden quedar (`P/Systems/Shared/SystemBlockWriter.cs:25`; OM-5).

## 7. `RackViewKind`, variante y codec (D-02, D-03)

### 7.1 Una sola taxonomia (A-4)

Renombre de `DimensionViewKind` (`A/Systems/Shared/DimensionViewPolicy.cs:11-16`) a `A/Views/RackViewKind.cs` sin cambio de miembros ni
de ADR-0035. El tipo no se serializa: solo persisten los bits de `DimensionViewVisibility`. Los tokens del sobre
(`A/Persistence/RackEmbedDocument.cs:28-30`) siguen siendo la persistencia. Seis archivos de produccion y seis de pruebas nombran el tipo.

`CantileverViewKind` (`A/Systems/Cantilever/CantileverViewPlanBuilder.cs:12-37`) **no** es una segunda taxonomia de vista: es el enum de
camara del builder e incluye `AdapterSection` (`:36`), que no es una vista del rack. Se conserva con un mapeo desde `RackViewKind`
(OM-2).

`DimensionViewKind` no esta en la instantanea de enums de la guarda de cobertura estructural de I-52 (V13 §6.7) y no se serializa. Si
cuando aterrice el renombre esa guarda ya lo alcanza en la base de I-55, el renombre se reporta (X-5) y **el reapunte lo decide el
Coordinador de I-52** (su §6.7 trata un renombrado como RED / STOP); I-55 no debilita la guarda. El renombre solo lo hace I-55 G6: toca
editores WPF, que I-52 excluye (V13 §20.3).

### 7.2 Variante tipada (A-1)

Cerrada, igualdad por valor, **nunca** un indice de UI; base 0 en la variante y base 1 en mensajes (`P/RackSelectivoCommands.cs:476-491`, `:546-561`).

### 7.3 Codec, disponibilidad y politica del consumidor, separados (X-2; CR-04; AR2-05)

Tres responsabilidades distintas; ninguna invade a la otra.

```text
A. Codec sintactico      Decode(kind, View, Section) → (RackViewAddress, disposicion sintactica)
                         disposicion sintactica ∈ { Canonical, Canonicalizable, Coerced, Invalid }
                         NO consulta el sistema resuelto: no sabe cuantos fondos, postes o estaciones hay ni si existe el lado B
B. Disponibilidad        Availability(sistema resuelto, direccion) → Available | Orphaned | Unsupported  (+ diagnostico)
C. Politica              cada consumidor combina A y B; el codec no contiene politicas
```

**Vocabulario comun de disposicion.** `Canonical`, `Canonicalizable`, `Coerced`, `Invalid` y `Orphaned`. **`Orphaned` la emite solo
B**, para una direccion sintacticamente valida que el sistema resuelto ya no tiene; A nunca la emite, porque no consulta el sistema.

#### A. Codec sintactico

| Kind | Entrada (`View`, `Section`) | Direccion | Disposicion sintactica | Evidencia de la lectura vigente |
|---|---|---|---|---|
| `selective` | `frontal`, `Section ≥ 0` | `Fondo(Section)` | `Canonical` | `P/RackSelectivoCommands.cs:168` |
| `selective` | `frontal` con `−1`; `View` vacio con `−1` | `Fondo(0)` | `Canonicalizable` (legado documentado) | `:155`, `:168` |
| `selective` | `frontal` con `Section < −1`; `View` vacio con `Section ≥ 0`; `View` no vacio desconocido | `Fondo(max(Section, 0))` (lectura vigente: frontal) | `Coerced` | `:126`, `:168` |
| `selective` | `planta` con `−1` / con otro valor | `Whole` | `Canonical` / `Canonicalizable` | reescribe −1 (`:216`) |
| `selective` | `lateral`, `Section ≥ 0` / `Section < 0` | `Post(Section)` / — | `Canonical` / `Invalid` | `:194` |
| `dynamic` | `frontal` con `0` o `1` / otro valor | `FlowEnd(...)` | `Canonical` / `Coerced` | `P/RackDinamicoCommands.cs:220-224`, `:392-393` |
| `dynamic` | `planta` con `−1` / con otro valor | `Whole` | `Canonical` / `Canonicalizable` | `:211` |
| `dynamic` | `lateral`, `Section ≥ 0` / `Section < 0` | `Post(Section)` / `Post(0)` | `Canonical` / `Coerced` | `:236-239` |
| `dynamic` | `View` vacio con `−1` / con `Section ≥ 0`; `View` no vacio desconocido | `Post(0)` / `Post(Section)` | `Coerced` (la lateral sin seccion se dibujo con todos los niveles, altura total y cobertura completa, que no son las de `Post(0)`: `A/Systems/Dynamic/DynamicSystemLateralBuilder.cs:34-36`, `:67-80`; OM-9) | `:236-239` |
| `pushback` | `frontal` con `0..3` / fuera de `0..3` | `PushBackCut(DecodeSection)` / — | `Canonical` / `Invalid` | `A/Systems/PushBack/PushBackSystemFrontalBuilder.cs:142`, `:155`; `P/RackPushBackCommands.cs:427-450` |
| `pushback` | `planta` con `−1` / otro; `lateral` con `≥ 0` / `< 0`; `View` vacio o desconocido | `Whole`, `Post(Section)` | `Canonical` / `Invalid` | `P/RackPushBackCommands.cs:427-450` |
| `cantilever` | `frontal` o `planta` con `−1`; `lateral` con `≥ 0` / cualquier otra forma | `Whole`, `Station(Section)` | `Canonical` / `Invalid` | `P/RackCantileverCommands.cs:501-540` |
| `cabecera` | `lateral` o `planta` con `−1` / con otro valor; `View` vacio | `Whole` | `Canonical` / `Canonicalizable` | `P/RackCabeceraCommands.cs:197`, `:239-240` |
| `cabecera` | `View` no vacio desconocido | `Whole` (lectura vigente: lateral) | `Coerced` | `:239-240` |
| `cama` | `View` nulo / otro | `Whole` | `Canonical` / `Coerced` (nunca escrito) | escritura en `P/RackCamaCommands.cs:197-198` |
| todos | token conocido en otra capitalizacion, sin espacios | la del token | `Canonicalizable` | produccion compara sin distinguir mayusculas |
| todos | token con espacios al principio o al final (p. ej. `" lateral"`) o solo espacios | — | tratado como token desconocido de su fila | los editores comparan sin recortar (`P/RackSelectivoCommands.cs:329-330`; `P/RackCommandSupport.cs:102-103`); solo `RACKLISTA` recorta (`A/Persistence/RackListBuilder.cs:117-118`) |

**Reglas de lectura de la tabla.** «`View` vacio» = nulo o cadena vacia, sin recortar. La disposicion de una entrada es la **peor** de
las que dan su `View` y su `Section` (orden `Canonical` < `Canonicalizable` < `Coerced` < `Invalid`); por ejemplo, un token en otra
capitalizacion con una `Section` invalida es `Invalid`. La tabla exacta la fija G3 en **una** caracterizacion compartida con la CT-04 de
I-52.

#### B. Disponibilidad sobre el sistema resuelto

| Direccion | `Available` si | Si no | Evidencia |
|---|---|---|---|
| `Fondo(k)` (Selectivo) | `k < SelectiveDepthLayout.Count(sistema)` | `Orphaned` | `P/RackSelectivoCommands.cs:167-173` |
| `Post(p)` (Selectivo, Dinamico, Push Back) | `p ∈ Cortes(sistema, catalogo).PostIndex` (la cuadricula maestra usa el catalogo: `A/Systems/Selective/SelectiveLateralBuilder.cs:33`, `:44`; `A/Systems/Dynamic/DynamicSystemLateralBuilder.cs:270-273`) | `Orphaned` | `P/RackSelectivoCommands.cs:190-198`; `P/RackDinamicoCommands.cs:205`, `:240-244`; `P/RackPushBackCommands.cs:239`, `:285-289` |
| `PushBackCut(e, B)` | el rack es compuesto (`IsComposite`) | `Orphaned`: un rack de un solo sentido no tiene lado B | `D/Systems/PushBack/PushBackDesign.cs:70`; `A/Systems/PushBack/PushBackSystemFrontalBuilder.cs:46-52` |
| `Station(s)` (Cantilever) | `s < line.Stations.Count` | `Orphaned` | `P/RackCantileverCommands.cs:306-310` |
| `Whole`, `FlowEnd`, `PushBackCut(e, A)` | siempre | — | — |
| cualquiera | el par (kind, tipo) esta soportado para el consumidor en la matriz de §5 | `Unsupported` (p. ej. la Cama en ID19) | §5 |
| variante canonica de un tipo destino | el conjunto de cortes o estaciones no esta vacio | `Unsupported` con motivo «el sistema no tiene cortes» (`A/Systems/Selective/SelectiveLateralBuilder.cs:36-38`) | §5 |

#### C. Politica de cada consumidor

| Consumidor | Acepta | Falla o actua | Nota |
|---|---|---|---|
| `RACKEDITAR` (legado, sin cambio) | lo que lee hoy, incluido lo coercionado | **Por kind.** Selectivo: una lateral `Invalid` (`Section < 0`) y las `Orphaned` se borran como fantasmas (`P/RackSelectivoCommands.cs:169-173`, `:194-198`), salvo que sean las unicas vistas (`:229-239`). Push Back y Cantilever: un descriptor `Invalid` aborta la edicion entera (`P/RackPushBackCommands.cs:216-223`; `P/RackCantileverCommands.cs:264-271`); una estacion `Orphaned` se borra (`:304-310`). Push Back: `PushBackCut(e, B)` en un rack de un sentido (`Orphaned` en B) **no** se borra: el builder ignora el lado B y se redibuja como A conservando `Section` (`A/Systems/PushBack/PushBackSystemFrontalBuilder.cs:44`, `:50-52`; `P/RackPushBackCommands.cs:253-269`) | La reescritura de `Canonicalizable` es **por kind**: Cantilever conserva el token `View` crudo (`P/RackCantileverCommands.cs:313-315` → `A/Persistence/RackEmbedComposer.cs:54`) |
| ID17 / ID18 | escriben solo direcciones `Canonical`; el sobre elegido de una hermana se lee con la politica de `RACKEDITAR` | — | Sin cambio de lectura legacy |
| **ID19** | `Canonical` o `Canonicalizable` **y** `Available` (canonizacion semantica **sin reescribir** la fuente) | `Coerced` e `Invalid` **fallan en CLASSIFY**; la disponibilidad solo se evalua sobre direcciones `Canonical` o `Canonicalizable`, y `Orphaned` y `Unsupported` fallan en AVAILABLE; todo antes de pedir puntos | Diagnostico determinista (abajo) |
| I-52 | su politica (V13 §3.7, §7.9, §9.1) | — | Consume A y B |

**Diagnostico determinista de ID19.** Una linea por referencia rechazada, en el orden estable del nucleo de seleccion (grupo y luego
referencia, X-1):

«La vista `<bloque>` del rack `<nombre>` tiene `View = '<valor>'` y `Section = <valor>` (`<disposicion>`): `<motivo>`. No se proyecto
nada. Abrela con RACKEDITAR para normalizarla.»

`<motivo>` es un texto fijo por disposicion: «RackCad no la reconoce sin suponer» para `Coerced` e `Invalid`, «ya no existe en el diseno»
para `Orphaned` y «no se puede proyectar» para `Unsupported`.

**Otros lectores** no migrados, caracterizados en G3:
- `P/ProjectVariableMutationExecutor.cs:180-218`;
- `A/Persistence/RackListBuilder.cs:117-118`: `RACKLISTA` lee un `View` vacio como lateral en todos los kinds, asi que el «significado
  unico» de `Canonicalizable` vale para los lectores de geometria, no para el listado;
- `P/RackBlockFinder.cs:23-24`;
- `P/RackLayoutCommands.cs:71`.

## 8. Authored, effective y metadatos de una vista nueva

| Campo | Rack nuevo | Hermana por editor | ID19 |
|---|---|---|---|
| `RackId` | acunado una vez | del sobre elegido; curado si estaba en blanco | del grupo `RackId` |
| `Name`, `Kind` | del editor / constante | del editor / igual | del grupo; kind con `TryResolve` sensible a mayusculas (`P/RackMenuCommands.cs:141`) |
| `View`/`Section` | codec | codec | codec (variante segun M-01) |
| authored | `SelectivePalletDesignDocument.From(design, id, name)` o diseno del editor | reconciliado y serializado una vez | verbatim del sobre fuente, con authored unico (§9.3) y sistema de `Resolve` (§4.5) |
| `CustomProperties` | ausentes | **coleccion comun** del `RackId` (§9.2) | **coleccion comun** del grupo (§9.2) |
| `ExtensionData`, `SchemaVersion` | nuevos | del sobre elegido (OM-14) | del sobre fuente |
| variables, expresiones, `DimensionViews` | en diseno o authored | verbatim | verbatim; resolucion fallida → nada escrito |
| `Id` interior | como hoy | sin tocar | sin tocar |

**`TGrd02`:** el primer argumento de `Compose` fuera del compositor es un parametro `RackEmbedDocument` del miembro que llama
(`T/CustomPropertiesEnvelopeGuardTests.cs:85-105`).

## 9. Autoridad entre hermanas (D-07; OQ-5, OQ-7; CR-03)

### 9.1 Principio

**I-55 no crea una hermana que nazca distinta de las demas**, y la comprobacion se acota al `RackId` manipulado: no usa la autoridad
global de I-54 (su pertenencia indeterminada bloquea cualquier rack ante un payload ilegible en cualquier parte,
`A/CustomProperties/RackCustomPropertiesAuthority.cs:249-264`), ni reabre ADR-0039, ni modifica `RACKPROPIEDADES`.

### 9.2 `RackSiblingCustomPropertiesGate` (pura)

```text
entrada    RackId; sobre fuente o elegido; definiciones del barrido con: sobre interpretado (o nulo), texto del payload,
           dependencia de xref y clave de definicion

miembros   (1) el sobre fuente o elegido, SIEMPRE (tambien cuando se cura un Id en blanco: es el destino de la cura)
           (2) sobres interpretables, no dependientes de xref, con Id == RackId (OrdinalIgnoreCase)
               — mismo criterio que I-54, A/CustomProperties/RackCustomPropertiesAuthority.cs:378-391
ilegibles  definicion no dependiente cuyo store devolvio nulo (JSON invalido o MAJOR mas nuevo,
           A/Persistence/RackEmbedDocument.cs:97-129):
             RackEnvelopeIdProbe lee el "Id" de NIVEL SUPERIOR del sobre (sin distinguir mayusculas, como el store,
             A/Persistence/RackEmbedDocument.cs:81-85; con duplicados, cualquier coincidencia cuenta; el "Id" de un diseno
             anidado nunca cuenta)
             Id == RackId      → HERMANA ILEGIBLE → FALLO CERRADO
             Id legible ≠      → de otro rack → NO bloquea
             sin Id legible    → no atribuible → NO bloquea

reglas (en orden, como I-54, A/CustomProperties/RackCustomPropertiesAuthority.cs:268-334)
  Kind en blanco en algun miembro, o mas de un Kind (OrdinalIgnoreCase)       → FALLO «tipos distintos»
  alguna coleccion no escribible (CustomPropertiesStore.ReadElement,
    A/Persistence/CustomPropertiesStore.cs:90; outcomes en A/Persistence/CustomPropertiesReadResult.cs:8-30) → FALLO «hermana ilegible»
  formas canonicas distintas (CustomPropertiesCanonicalForm,
    A/CustomProperties/CustomPropertiesCanonicalForm.cs:24-70)                → FALLO «propiedades divergentes»
  una sola forma canonica                                                     → COMUN: la hermana nueva hereda la coleccion del
                                                                                 sobre fuente, canonicamente igual a todas
rack nuevo sin hermanas ni sobre fuente                                        → SIN GATE
```

- **Remedio condicionado.** «Unificalas con RACKPROPIEDADES. Si RACKPROPIEDADES las muestra en solo lectura (por ejemplo, por un bloque
  ilegible en el dibujo o porque la unificacion esta bloqueada por datos de extension o por version), resuelve primero la causa que
  indica.» La autoridad global de I-54 si bloquea toda escritura de alcance rack ante un payload ilegible
  (`docs/adr/0039-custom-properties-persistencia-autoridad.md:266-269`) y la unificacion segura se bloquea por datos de extension o minor
  mayor (`:291-295`; `A/CustomProperties/RackCustomPropertiesAuthority.cs:425-437`): el remedio puede exigir reparar antes (R-15).
- **Residual F-14a.** Un payload con UTF-16 invalido hace fallar el barrido antes de cualquier mutacion
  (`docs/adr/0039-custom-properties-persistencia-autoridad.md:222-223`): la operacion falla, no se ignora.
- **Frontera con I-54.** Solo consumo de `CustomPropertiesStore` y `CustomPropertiesCanonicalForm`; ningun archivo del Plugin nombra la
  autoridad (`TGrd04`). Ningun archivo de gate cae en la clasificacion de `TGrd05` (`T/CustomPropertiesEdgeGuardTests.cs:287-298`),
  asi que una guarda nueva (`RackSiblingGateIndependenceGuardTests`) fija que los dos gates viven en archivos separados, que el de
  propiedades no nombra Project Variables y que el authored no nombra Custom Properties. El Plugin aporta el texto del payload y la dependencia de xref, que
  `FindRackBlocks` no expone (`P/RackCommandSupport.cs:109-134`).
- **D-15 cuenta las unidades de redibujo de las hermanas propias del dibujo** (los miembros del gate). Las definiciones dependientes de
  xref que la busqueda vigente tambien recorre (`P/RackBlockFinder.cs:66` solo omite las de xref, no las dependientes) las gobierna su
  dibujo de origen, que las repone al recargar la xref: se redibujan como hoy, su fallo se informa como aviso y no detiene Insertar, y
  no son hermanas a efectos de INV-AUTH-1. Contarlas podria bloquear Insertar de forma permanente en un rack que tambien esta en una xref
  cargada, con un remedio que el usuario no puede ejecutar (OM-25).
- **Huerfanas.** Las vistas que el bucle vigente **no redibuja y borra como fantasmas**, por kind (§7.3 C): fondo, poste o estacion que ya
  no existen, y la lateral `Invalid` del Selectivo. **No** lo es el lado B de un Push Back de un sentido, que el bucle vigente redibuja
  como A. Son miembros del gate de propiedades: son legibles y `RACKPROPIEDADES` puede unificarlas.
- **Remedio por motivo.** «Propiedades divergentes» y «hermana ilegible» → el remedio condicionado de abajo. «Tipos distintos» (un `Kind`
  en blanco o mezclado) → el mensaje lista las vistas con su `Kind` y **no** propone `RACKPROPIEDADES`, que muestra ese caso en solo
  lectura (`A/CustomProperties/RackCustomPropertiesAuthority.cs:268-293`); no hay remedio automatico en I-55 (OM-23).
- **Claves duplicadas en distinta capitalizacion** en un sobre legible: el store se queda con la ultima (`A/Persistence/RackEmbedDocument.cs:81-85`)
  y la sonda solo mira los ilegibles. Riesgo residual compartido con I-54, no una regresion (R-19).
- **`Id` en blanco (AR2-16).** En I-55, «en blanco» es `IsNullOrWhiteSpace`, como la pertenencia de I-54
  (`A/CustomProperties/RackCustomPropertiesAuthority.cs:386`). Hoy Selectivo y Cabecera usan `IsNullOrEmpty`
  (`P/RackSelectivoCommands.cs:119`; `P/RackCabeceraCommands.cs:233`) y `FindRackBlocks` solo excluye el `Id` vacio
  (`P/RackCommandSupport.cs:112`, `:124`).
  - Un sobre elegido con `Id` formado solo por espacios se cura como uno vacio antes del gate y es miembro por la regla (1).
  - Una hermana de la busqueda vigente que no sea miembro del gate (por ejemplo, dependiente de xref) se redibuja como hoy y, si su
    redibujo falla, se informa como aviso sin detener Insertar (OM-25).
  - Una fuente lateral o planta con `Id` en blanco que el flujo vigente no anade al redibujo (`P/RackSelectivoCommands.cs:132-136`;
    `P/RackCabeceraCommands.cs:243-246`) sigue sin redibujarse: es el sobre elegido, miembro del gate, y la vista nueva hereda su
    coleccion.

### 9.3 Authored

- **Editor:** el flujo vigente unifica el authored de las hermanas; D-15 impide insertar si un redibujo falla, y las huerfanas se borran
  con la primera colocacion nueva (§11.1), de modo que ninguna hermana conserva un authored distinto del de la vista nueva.
- **ID19:** `RackSiblingAuthoredGate` sobre **todas** las hermanas del barrido: Selectivo `SelectiveAuthoredAuthority.Resolve`
  (`A/ProjectVariables/SelectiveAuthoredAuthority.cs:128-157`); demas kinds, comparador de X-3 (excluye la metadata interior de
  `P/RackCommandSupport.cs:40-67`). Divergente o ilegible → fallo con remedio `RACKEDITAR`. Una hermana que el preflight de `RACKEDITAR`
  rechazaria (§4.5) → fallo, porque ese remedio no funcionaria.

### 9.4 Donde aplica

| Flujo | Gate de propiedades | Gate authored | Momento |
|---|---|---|---|
| Rack nuevo | no | no | — |
| `RACKEDITAR` → Actualizar | no | no | sin cambio |
| `RACKEDITAR` → Insertar una vista (ID17 despues) | **si** | flujo vigente + D-15 | antes de preparar y de redibujar |
| `RACKEDITAR` → Insertar varias (ID18) | **si** | flujo vigente + D-15 | idem |
| ID19 | **si**, por grupo `RackId` | **si**, por grupo `RackId` | antes de pedir puntos |

## 10. ID17 — FIRST-VIEW FREEDOM

| Sistema | Hoy | Cambio | Nota |
|---|---|---|---|
| Selectivo | solo Frontal (`U/Systems/Selective/RackSelectiveWindow.xaml.cs:2560-2570`, `:274-292`) | Lateral-first y Planta-first | edicion sin frontal (`P/RackSelectivoCommands.cs:125-136`) |
| Dinamico | solo Lateral (`U/Systems/Dynamic/RackDynamicSystemWindow.xaml.cs:3394-3403`, `:189-194`) | Frontal salida/entrada-first y Planta-first | `P/RackDinamicoCommands.cs:102-132` |
| Push Back | libre | poste real (PR-1) | — |
| Cantilever | libre | planta fiel (PR-2) | — |
| Cabecera | solo Lateral (`U/RackFrames/RackFrameConfiguratorWindow.xaml.cs:85-93`, `:265-269`; `U/Editor/RackInsertionRequest.cs:36-53`; `P/RackCabeceraCommands.cs:37-41`) | Planta-first | `P/RackCabeceraCommands.cs:278-306` |

Prompts legacy de variante conservados (OM-4); en un rack existente se piden antes del gate y del redibujo (§11.1). Enter en esos prompts
acepta el valor por defecto; Esc cancela sin modificar nada. Agregar vistas a un rack existente pasa por §9. Racks parciales (OQ-7): celda de
`RACKLAYOUT` de solo planta (`P/RackLayoutCommands.cs:231-244`) recibe hermanas con el gate.

## 11. ID18 — MULTI-VIEW QUEUE / BATCH

- **Editor.** Dialogo del arquetipo C (ADR-0029) con presentador **fuera del archivo de la ventana**. Las guardas de modales cuentan
  `ShowDialog(` en el archivo de ventana (`TU/DynamicHeaderBatchSeamGuardTests.cs:26-31`, `TU/SelectiveHeaderBatchSeamTests.cs:177-181`).
  Orden F → L → P (OD-5).
- **Peticion.** `Views` en la peticion; `RequestInsertViews`.
  - Rutas que perderian `Views`: `U/Editor/EditorModules.cs:53-56`, `:90-95`; `P/RackSelectivoCommands.cs:34-38`;
    `P/RackCabeceraCommands.cs:37-41`, `:173`.
  - Push Back ya devuelve la peticion de la ventana (`U/Editor/EditorModules.cs:98-100`).
  - Una sola llamada a `EnsureId` sirve a todo el lote (`U/Editor/RackEditorSession.cs:126`); los comandos sin sesion acunan una vez por
    intencion (RID-1).
- **Transaccion (D-06; A-3): C** — PREPARE ALL + commit por colocacion (`P/Drawing/BlockPlacement.cs:174-201`). Nunca queda abierta
  entre jigs otra transaccion que la del propio jig.

### 11.1 Redibujo legacy y D-15 (AR2-02)

Los bucles de redibujo de `RACKEDITAR` **confirman vista por vista** (`P/Systems/Shared/SystemBlockWriter.cs:55-66`;
`P/Drawing/LateralHeaderDrawService.cs:138-143`; `P/RackCantileverCommands.cs:319-326`). I-55 no los envuelve en una transaccion global,
**por prescripcion del Coordinador** (CD2-03), no por imposibilidad tecnica.

> **Evidencia para el Coordinador (CQ-01, §22.3).** El repositorio ya tiene el primitivo de MUTATE dentro de la transaccion del llamador:
> `SystemBlockWriter.RedefineInTransaction` (`P/Systems/Shared/SystemBlockWriter.cs:82-116`, cuyo comentario describe el defecto de
> confirmar `k-1` racks de un lote), `ViewBlockDraw.PrepareRedraw`/`RedrawInTransaction` (`P/Systems/Shared/ViewBlockDraw.cs:65`, `:97`) y
> `LateralHeaderDrawService.PrepareRedraw`/`RedrawInTransaction` (`P/Drawing/LateralHeaderDrawService.cs:60`, `:95`). Lo usan los
> `RedrawInPlace` vigentes, cada uno con su propia transaccion (`P/Systems/Shared/SystemBlockWriter.cs:64`;
> `P/Drawing/LateralHeaderDrawService.cs:136-141`), y `ProjectVariableMutationExecutor`, que para el Selectivo y la lateral redibuja
> todas las vistas afectadas con PREPARE, UNA transaccion y POST (`P/ProjectVariableMutationExecutor.cs:142`, `:246-293`). El `RedefineBlock` del Cantilever tambien recibe la
> transaccion del llamador (`P/RackCantileverCommands.cs:322-323`). Faltan envoltorios de preparacion para Dinamico, Push Back y
> Cabecera. Con esta evidencia, «PREPARE → una MUTATE → POST» es viable para el redibujo de Insertar. V3 **no** lo adopta por su cuenta:
> sigue CD2-03 y deja la eleccion al Coordinador.

La semantica vinculante de V3 es:

```text
0. VARIANTE: la variante de CADA vista nueva (prompt legacy de fondo o poste, o la lista del dialogo de ID18) se elige con el sistema
   nuevo del editor ANTES del gate, de PREPARE ALL y del primer redibujo (cambio declarado, §3)
     - Esc en el prompt de variante → VARIANT_CANCELLED: nada modificado
     - Enter en un prompt de entero con valor por defecto acepta el valor por defecto; no cancela
       (P/RackSelectivoCommands.cs:476-489, :546-559; P/RackDinamicoCommands.cs:318-330; P/RackPushBackCommands.cs:367-379)
     - variante no disponible (sin corte lateral en ese poste, seccion invalida, estacion ausente) → ABORTED_BEFORE_WRITE
1. PREPARE ALL, ANTES de tocar el dibujo:
     - vistas NUEVAS: sistema, disponibilidad, semilla, plan, sobre compuesto y requisitos de bloque de cada una, en memoria
     - hermanas que el bucle vigente redibuja: plan y payload de cada una (regla F de Compose, A/Persistence/RackEmbedComposer.cs:103-121,
       o F-14b, docs/adr/0039-custom-properties-persistencia-autoridad.md:224-228); las huerfanas (§9.2) se clasifican aqui y no se
       planifican
     - cualquier fallo → ABORTED_BEFORE_WRITE
2. REDRAW legacy de las hermanas existentes, en el orden vigente. UNIDAD de redibujo = importacion + redefinicion + payload + renombre
   de UNA hermana; «confirmada» = su transaccion hizo commit. Un error posterior al commit (p. ej. al describir faltantes) no la
   convierte en fallida: se informa como «actualizada con aviso»
3. si UNA unidad de una hermana propia del dibujo falla (dentro o fuera de su try; las dependientes de xref se redibujan como hoy y su
   fallo es un aviso, §9.2):
     - STOP: no se redibujan mas hermanas, no se borra ninguna huerfana y no se materializa NINGUNA vista nueva; las vistas preparadas
       se descartan
     - las unidades ya confirmadas PERMANECEN; no hay rollback global
     - informe exacto, en el orden del redibujo: actualizadas (bloque, tipo, variante), actualizadas con aviso y la que fallo (bloque o
       direccion, motivo)
     - Regen si alguna unidad confirmo
4. si todas confirman:
     - huerfanas (las que el bucle vigente borra como fantasmas, §9.2):
         · si sobrevive alguna otra vista, se borran como hoy (P/RackSelectivoCommands.cs:229-239), pero con borrado VERIFICADO:
           EraseViewBlocks se traga las excepciones (P/RackCommandSupport.cs:269-272), asi que si no se borran todas cuenta como unidad
           fallida → STOP, sin vistas nuevas
         · si las UNICAS vistas del rack son huerfanas, se conservan hasta la primera colocacion nueva y se borran DENTRO de la
           transaccion de su jig, antes del commit (P/Drawing/BlockPlacement.cs:179-199): la referencia nueva y el borrado confirman
           juntos. La definicion nueva ya esta confirmada antes del jig (P/Systems/Shared/SystemBlockWriter.cs:27-36); si el jig no
           confirma, se retira con la limpieza vigente (best effort, D-10) y las huerfanas quedan intactas
         · cambio declarado: hoy quedan si son las unicas vistas y un mensaje pide borrarlas a mano (:236-239)
     - Regen segun la regla vigente (cuenta tambien un borrado de huerfanas) → cola de colocaciones
```

- **D-15 se mantiene:** un redibujo fallido impide crear vistas nuevas.
- **No existe rollback global** de redibujos anteriores. El rack puede quedar redibujado en parte, que es la divergencia vigente del
  redibujo multivista (OM-3). El mensaje lo dice y no propone Actualizar como remedio universal: Actualizar conserva su comportamiento y
  omite en silencio los redibujos fallidos (`P/RackSelectivoCommands.cs:178-183`; `P/RackCantileverCommands.cs:328-332`), asi que un
  fallo causado por los datos se repetiria.
- **INV-AUTH-1:** I-55 nunca **crea** una hermana nueva con authored divergente de una hermana propia del dibujo, ni respecto a las
  redibujadas (paso 3) ni respecto a las huerfanas (paso 4); no promete deshacer mutaciones legacy ya confirmadas (§13).
- **Alcance:** `RACKEDITAR` → Insertar (una o varias vistas) sobre un rack existente. `RACKEDITAR` → Actualizar conserva su comportamiento
  vigente (redibuja todas las hermanas e informa).

### 11.2 Maquina de estados

```text
VARIANT      ─(Esc)──→ VARIANT_CANCELLED            "No se inserto ninguna vista."                              (nada modificado)
VARIANT      ─(no disponible)→ ABORTED_BEFORE_WRITE "No se inserto ninguna vista: <vista> <motivo>."             (nada modificado)
SIBLING_GATE ─(falla)→ SIBLING_GATE_FAILED          "No se inserto ninguna vista: <motivo y remedio>."          (rack existente)
PREPARE_ALL  ─(falla)→ ABORTED_BEFORE_WRITE         "No se inserto ninguna vista: <vista> <motivo>."             (nada escrito)
REDRAW(j)    ─(falla)→ REDRAW_FAILED                "No se inserto ninguna vista. Se actualizaron: <lista>.
                                                     Fallo: <bloque> <motivo>. Corrige la causa y repite Insertar." (rack existente)
READY → PLACING(i) → COMMITTED(i) → … → COMPLETED
           ├─ Esc o Enter (respuesta vacia) → CANCELLED(i) → COMPLETED_PARTIALLY, o CANCELLED_BEFORE_ANY si i = 1
           └─ excepcion o PromptStatus.Error → FAILED(i) → COMPLETED_PARTIALLY, o FAILED_BEFORE_ANY si i = 1
```

**Esc y Enter detienen la cola** (OD-4, AR2-14): el jig acepta la respuesta vacia y cancela con `Cancel` o `None`
(`P/Drawing/BlockPlacement.cs:186-191`, `:219-229`); `PromptStatus.Error` se trata como fallo, no como cancelacion. El mensaje de cola
parcial nombra las vistas colocadas y las que no y, en un rack existente, tambien las hermanas redibujadas. `CANCELLED_BEFORE_ANY` y
`FAILED_BEFORE_ANY` en un rack existente dicen que los redibujos confirmados permanecen.

`PlaceAndReport` gana un parametro de prompt (`P/Drawing/BlockPlacement.cs:26`, `:214`).

**`Regen`.** El lote no anade regeneraciones al flujo legacy: el Cantilever hace una tras el redibujo (`P/RackCantileverCommands.cs:355-358`)
y una al final en lugar de una por vista (`:169`).

**Orden completo en un rack existente:** preflights → variante → gate → PREPARE ALL → REDRAW → `REDRAW_FAILED` o (huerfanas, `Regen`) →
cola.

## 12. ID19 — Group Placement & Projection (CR-01)

### 12.1 Flujo

```text
ACQUIRE     seleccion en Model Space + clase pedida (+ modo si M-01 lo expone)
SNAPSHOT    UNA lectura (D-08f):
              por referencia: handle, tipo (MInsertBlock), IsDynamicBlock, DynamicBlockTableRecord, BlockTableRecord, IsAnonymous,
              Annotative, espacio, Position, Rotation, ScaleFactors (X, Y, Z), Normal y Origin de la definicion
              por definicion: dependencia de xref, texto del payload y sobre interpretado (tambien del registro dinamico)
              registro de variables UNA vez; SCP actual
CLASSIFY    nucleo neutral (X-1) + D-08f + codec sintactico (§7.3 A): Coerced e Invalid fallan aqui
GROUP       grupos RackId; DEDUPE conservando todas las referencias (D-08b)
RESOLVE     Resolve UNA vez por RackId (§4.5); solo Resolved continua                                    ← antes de validar
AVAILABLE   disponibilidad de la variante fuente y de la variante destino (§7.3 B), solo sobre direcciones Canonical o Canonicalizable
FRAMES      UNA vez sobre TODA la seleccion S, desde el sistema resuelto y el catalogo, sin builders:
              RackViewFrame fuente y destino de cada referencia (§12.3); ρ_r, e_K(r), w(r), w, e, σ_s, σ_t, κ_s, κ_t, α, la
              coordenada p_r (independiente de B) e intervalos relativos (§12.5); nunca por grupo RackId
VALIDATE    tipos fuente, pares, modos y variantes segun M-01; una sola familia si OD-7.d = A; paralelismo, sentidos y superposicion
            (§12.5, OD-7.b) con los datos de FRAMES; miembros soportados (OD-3)
GATES       propiedades y authored por grupo RackId (§9); hermanas que el preflight de RACKEDITAR rechazaria (§4.5)
PLANS       por destino (definicion fuente y variante destino de cada grupo RackId): semilla, plan, sobre y requisitos de bloque
            (§4.4); comprobaciones puras 1 y 2 del requisito estructural (§4.6); a_t(r)
PICK        punto base y destino (SCP → universal); con OD-6.c = B, tambien el angulo comun     ← tras validar y planificar; antes de importar
              GetPoint con AllowNone = true: OK continua; None (Enter) o Cancel (Esc) → CANCELLED, nada escrito; Error → fallo,
              nada escrito; sin palabras clave
IMPORT      importacion de la union de claves y comprobacion 3 del requisito estructural (§4.6); un fallo aqui no escribe ninguna
            definicion ni referencia de rack (las de biblioteca importadas pueden quedar, OM-5)
PLACE       UNA CommonTransform2D de la ejecucion con B y T (§12.2); s_r = p_r − B·e; sourceAnchor, targetAnchor y colocaciones
MATERIALIZE UNA transaccion: UNA definicion nueva por definicion fuente (con OD-7.c = A, una por grupo RackId; D-08b) y una
            referencia por referencia fuente; commit; aviso final «las vistas nuevas son vistas enlazadas del mismo rack, no copias:
            el BOM no cambia (para copiar, RACKDUPLICAR)»
```

Como en I-52 V13 §9.1, la resolucion ocurre antes de pedir la linea o los puntos.
- **Antes de los puntos:** todo fallo de clasificacion, resolucion, disponibilidad, marcos, validacion, gates o planes, incluidas las
  comprobaciones puras del requisito estructural.
- **Despues de los puntos:** solo se importa y se comprueba la presencia de las claves (fallo antes de escribir), se calcula la colocacion
  y se materializa.

**Mensajes deterministas (AR2-07..AR2-20).**
- Cada etapa que falla emite **un** mensaje con **todos** los miembros afectados (OD-3), en el orden estable del nucleo de seleccion
  (grupo y luego referencia).
- Las etapas se evaluan en el orden del flujo, y la primera que falla detiene la ejecucion.
- Los avisos (superposicion) listan los pares afectados en ese mismo orden. Se calculan en FRAMES y se evaluan en VALIDATE, pero se muestran justo antes de PICK
  y solo si ninguna etapa anterior a los puntos fallo; el usuario puede cancelar en PICK.

### 12.2 Nivel 1 — Group Placement (foundation general)

```text
Seleccion proyectada S = todas las referencias aceptadas en UNA ejecucion; grupos RackId G ⊆ S

Por referencia r ∈ S
  M_r        transformacion completa (Position, Rotation, ScaleFactors, Normal, Origin de la definicion)
  escala     |sx| = |sy| = |sz| = 1 (tolerancia de escala compartida, X-7) y sz > 0; (sx, sy) = (−1, −1) se canoniza como giro de π;
             cualquier otra combinacion con algun signo negativo falla con «escala negativa no admitida» (D-08f; como ST-7, ST-8 y
             ST-10 de I-52 V13 §4.1)
  L_r        parte lineal 2D de M_r tras la canonizacion (solo rotacion)
  φ_r        orientacion de la referencia = angulo de L_r·(1,0)  (Transform2D.RotationAngle, A/Geometry/Transform2D.cs:111-115)
  angulos    φ, α y ρ se normalizan a (−π, π]; toda igualdad de angulos se entiende modulo 2π con la tolerancia GeometryTolerance.Angle
  a_s(r)     ancla semantica en coordenadas locales de la vista fuente (§12.3; por defecto el origen fisico; la politica
             ortografica usa el extremo del tramo destino, §12.5)
  a_t(r)     la misma ancla semantica en coordenadas locales de la vista destino
  A_r        M_r · a_s(r)                                    ancla fuente en coordenadas universales (3D)

Marcos: UNO de cada por ejecucion
  SourceGroupFrame  F_s = (B, φ_s)    B = SCP→universal(punto base)
  TargetGroupFrame  F_t = (T, φ_t)    T = SCP→universal(punto destino)
  φ_s, φ_t: orientaciones de los marcos (entradas; M-01 / OD-6)

CommonTransform2D: UNA por ejecucion
  T_common = Transform2D.Translation(−B).Then(Transform2D.Rotation(α)).Then(Transform2D.Translation(T))
             (Then se lee de izquierda a derecha: A/Geometry/Transform2D.cs:85-93)
  α        = angulo comun de la ejecucion (lo fija la politica)
  rigidez  Determinant = +1 (:77) y ScaleFactor = 1 (:83); sin cizalla
  valor    se expresa con el valor de colocacion compartido (X-7): (T − R(α)·B, α, 1)

Para toda r ∈ S
  sourceAnchor_r  = politica(A_r)                         (plano XY)
  targetAnchor_r  = T_common(sourceAnchor_r)
  Z_r             = politica (fuera del valor 2D)
  ρ_r             = orientacion de la referencia nueva (politica; nunca una transformacion de layout por rack)
  referencia nueva: definicion nueva con Origin = 0 (P/Drawing/LateralHeaderDrawer.cs:243; P/Drawing/Cantilever/CantileverViewMaterializer.cs:47, :110),
                    Rotation = ρ_r, escala 1, normal Z, Position = (targetAnchor_r − R(ρ_r)·a_t(r), Z_r)
                    (AutoCAD: universal = Position + Rotation · Escala · (p − Origin))

INV-GRP-1  para TODO par r, q ∈ S, de cualquier grupo RackId:
           targetAnchor_r − targetAnchor_q = R(α)·(sourceAnchor_r − sourceAnchor_q)
INV-GRP-2  Rigid: ρ_r ≡ φ_r + α (mod 2π) para toda r ∈ S · Orthographic: ρ_r depende solo de φ_t y del marco destino (familia, tipo y
           variante), nunca de la posicion ni de la rotacion de r
INV-GRP-3  Orthographic, sobre la colocacion REAL de cada referencia nueva (no sobre las anclas):
             u_s(r, κ) = (A_r(κ) − B) · e                                       coordenada del punto fisico K = κ en la fuente
             u_t(r, κ) = (Position_r + R(ρ_r) · p_t(r, κ) − T) · w             la misma coordenada en la vista destino colocada
             (p_t(r, κ) = punto local de F_t(r) con K = κ y las demas coordenadas fisicas en el origen)
           si σ_s(r) = σ_t(r): u_t(r, κ) = u_s(r, κ) para todo κ ∈ [K_min_t(r), K_max_t(r)]
INV-GRP-4  Orthographic, para toda r: {u_t(r, κ)} = {u_s(r, κ)} sobre ese tramo (el mismo intervalo; invertido si σ_s ≠ σ_t)
           (INV-GRP-1 sola no garantiza ninguna de las dos; una prueba de anclas no detecta un a_t o un ρ equivocados)
```

La relacion **entre** racks la fija solo `T_common`; los datos por rack (descriptor, orientacion, tramo) solo situan el ancla dentro de su
propio bloque y orientan ese bloque.

### 12.3 Descriptores de marco (`RackViewFrame`)

**Marco fisico del rack:** R = corrida, D = profundidad, H = altura, con un unico origen fisico por rack. Hipotesis derivadas del codigo:
G3 las fija con caracterizacion sobre la salida de los builders, y un par inconsistente queda fuera de ID19 hasta una Proposal nueva.
Nunca se ajusta el descriptor.

`RackViewFrame(sistema resuelto, tipo, variante)`:
- **Mapa de ejes:** eje local ↔ eje fisico, con su signo.
- **Origen fisico local.**
- **Tramo por eje `[K_min, K_max]`,** en coordenadas fisicas del rack y **por variante**: la extension de la cuadricula del sistema
  resuelto para esa variante, **independiente** de las fronteras que el builder omite al dibujar (postes de frontera inexistentes del
  Dinamico, `A/Systems/Dynamic/DynamicSystemFrontalBuilder.cs:74-77`; fronteras que un corte compuesto de Push Back no posee; rango de
  un corte lateral del Dinamico, `A/Systems/Dynamic/DynamicSystemLateralBuilder.cs:175-177`). Todo tramo mide mas que la tolerancia de
  superposicion.
- **Convencion de extremos:** eje de poste o cara, fijada una sola vez en la caracterizacion y compartida con la CT-05 de I-52 (X-8).

| Familia | Vista | Eje local +X | Eje local +Y | Origen fisico local | Evidencia |
|---|---|---|---|---|---|
| **Rack** (Selectivo, Dinamico, Push Back, Cabecera) | Planta | +D | +R | D: cara exterior delantera (Dinamico: salida; Push Back: extremo bajo o cara del lado A); R: eje del poste 0 | `A/Systems/Selective/SelectivePlantaBuilder.cs:16-19`; `A/Systems/Dynamic/DynamicSystemPlantaBuilder.cs:15-17`; `A/RackFrames/PlantaHeaderLayoutBuilder.cs:95-103` |
| Rack | Frontal | +R | +H | eje del poste 0 de la cuadricula de la vista; base del poste | `A/Systems/Selective/SelectiveFrontalBuilder.cs:50` |
| Rack | Lateral | +D | +H | cara delantera del primer fondo que alcanza el poste (Selectivo) o X = 0 del sistema; base del poste | `A/Systems/Selective/SelectiveLateralBuilder.cs:82-97`; `A/RackFrames/LateralHeaderLayoutBuilder.cs:56-59` |
| **Cantilever** | Planta | −R | +D | columna de la estacion 0; plano de conexion | `A/Systems/Cantilever/CantileverViewPlanBuilder.cs:209-219`; `A/Geometry/Spatial3D.cs:186-197` |
| Cantilever | Frontal | −R | +H | columna de la estacion 0; suelo | idem |
| Cantilever | Lateral | −D | +H | plano de conexion; suelo | idem |

**Tramos por variante (AR2-08).**
- **Selectivo:** la planta usa la cuadricula maestra; la frontal de `Fondo(k)` usa la de su fondo, que puede ser mas corta: es un
  **prefijo** de la maestra con el mismo origen R (`A/Systems/Selective/SelectiveDepthLayout.cs:28-33`, `:52-55`;
  `A/Systems/Selective/SelectiveFrontalBuilder.cs:50`). El desplazamiento en D es del lateral `Post(p)`: un corte de esquina tiene su
  `anchorOffset` (`A/Systems/Selective/SelectiveLateralBuilder.cs:82-97`).
- **Cantilever:** el origen D (plano de conexion) es **interior** al tramo D, porque la columna ocupa y ≤ 0 y la base y ≥ 0
  (`A/Systems/Cantilever/CantileverColumnBaseDatum.cs:19-24`, `:45`). Por eso ningun ancla supone que el origen es un extremo del tramo.

La misma responsabilidad —origen y tramo del eje de una vista— es la del «tramo del eje de la vista (para c)» de I-52: X-8. El tramo
esta en coordenadas fisicas del rack; la `c` de I-52 esta en coordenadas locales de la vista y se obtiene a traves del origen fisico
local. `c` no tiene por que ser el punto medio de la convencion de extremos compartida: la diferencia δ la registra CT-06 de I-52.

### 12.4 Nivel 2a — Politica `Rigid`

```text
sourceAnchor_r = A_r (XY)                  α = φ_t − φ_s (M-01 / OD-6.c)
Z_r            = T.Z + (A_r.Z − B.Z)       ρ_r = φ_r + α
requisitos     escala segun §12.2 (D-08f); un solo tipo fuente salvo M-01 / OD-7.c
```

- **Misma clase:** el layout de anclas se reproduce exacto, rotado `α` y trasladado. La **variante destino** la decide M-01 / OD-2.b
  (conservar la de la fuente o la canonica); con la de la fuente, `a_t = a_s` y la vista nueva es un duplicado enlazado legal (OQ-4).
- **Frontal ↔ Lateral y cualquier par:** sustitucion en sitio; entre planta y elevaciones apila vistas.
- **Admite** orientaciones distintas entre racks (INV-GRP-2) y familias mezcladas; cada vista usa el descriptor de su familia, tipo y
  variante.

### 12.5 Nivel 2b — Politica `Orthographic` (encima de la foundation; AR2-01, AR2-07, AR2-08)

**Regla V3.** La direccion del eje destino se deriva siempre del marco destino de **cada** referencia: **familia del rack + tipo destino +
variante destino**, nunca de una familia implicita «rack». Con OD-7.d = A la seleccion exige una sola familia, pero el descriptor usado es
el de esa familia.

```text
Por referencia r
  K          eje fisico conservado del par (tabla de abajo)
  F_s(r)     RackViewFrame de la vista fuente de r (su familia, tipo y variante)
  F_t(r)     RackViewFrame de la vista destino de r: FAMILIA DE r + TIPO destino + VARIANTE destino (OD-2 / OD-2.b)
  k_s(r)     vector local de +K en F_s(r);  e_K(r) = normalizar(L_r · k_s(r))           direccion universal de +K en la fuente
  k_t(r)     vector local de +K en F_t(r)
  δ_t(r)     0 en elevaciones y en plantas con OD-6.d = A; con OD-6.d = B, el giro que lleva k_t(r) a +X
  ρ_r        = φ_t + δ_t(r)
  w(r)       = R(ρ_r) · k_t(r)                                                            direccion universal de +K en el destino
  [K_min_t(r), K_max_t(r)]  tramo sobre K de la VISTA DESTINO de r, en coordenadas fisicas del rack

Direccion comun destino w
  OD-7.d = A (una familia): todas las w(r) coinciden (mismo descriptor, mismo φ_t) → w = w(r); σ_t(r) = +1
  OD-7.d = B (mezcla):      w = w(r) de la familia con mas referencias; empate: rack antes que Cantilever; σ_t(r) = signo(w(r) · w);
                            w(r) perpendicular a w → fallo (OM-12)

Recta comun fuente e
  todas las e_K(r) paralelas con tolerancia angular ε_ang = GeometryTolerance.Angle, o fallo (OM-13)
  candidata c = normalizar(Σ_r signo(e_K(r) · e_K(r0)) · e_K(r)), con r0 cualquier referencia (el resultado no depende de r0 ni del
            orden de seleccion)
  SENTIDO de e ∈ {c, −c}: el que maximiza #{r : σ_s(r) = σ_t(r)}
            empate: el de angulo, normalizado a (−180°, 180°], en [−45° − ε_ang, 135° − ε_ang)
  σ_s(r) = signo(e_K(r) · e)
  CONSECUENCIA NORMATIVA: el sentido lo decide la mayoria. Girar 180° racks suficientes para cambiar la mayoria invierte el orden de
            TODAS las vistas destino sobre w (ejemplo abajo); se prueba y se valida (OV-ID19) y queda como pregunta para M-01 (OM-24)

Anclas: el tramo de la vista DESTINO evaluado sobre la colocacion fuente del rack
  A_r(κ)     = M_r · (punto local de F_s(r) con K = κ y las demas coordenadas fisicas en el origen)
  κ_s(r)     = K_min_t(r) si σ_s(r) = +1, si no K_max_t(r)      extremo del tramo destino con coordenada minima sobre e
  κ_t(r)     = K_min_t(r) si σ_t(r) = +1, si no K_max_t(r)      extremo con coordenada minima sobre w en la vista destino
  p_r        = A_r(κ_s(r)) · e                                   (FRAMES; no depende de B)
  s_r        = p_r − B · e = (A_r(κ_s(r)) − B) · e              (PLACE)
  sourceAnchor_r = B + s_r · e                                   (proyeccion sobre la recta por B; en PLACE)
  a_t(r)     = punto local de F_t(r) con K = κ_t(r) y las demas coordenadas fisicas en el origen
  α          = angulo(w) − angulo(e), normalizado a (−π, π]
  targetAnchor_r = T_common(sourceAnchor_r) = T + s_r · w
  Z_r        = T.Z

Consecuencias
  intervalo      la vista destino de r ocupa sobre w [s_r, s_r + (K_max_t(r) − K_min_t(r))]: el mismo intervalo que su tramo fisico
                 ocupa sobre e en la fuente (con σ_s ≠ σ_t, invertido dentro de la vista)
  INV-GRP-1      targetAnchor_r − targetAnchor_q = (s_r − s_q) · w = R(α) · (sourceAnchor_r − sourceAnchor_q)
  INV-GRP-3/4    con σ_s = σ_t: κ_s = κ_t y cada punto fisico del tramo queda en la misma coordenada sobre el eje comun; con σ_s ≠ σ_t,
                 el mismo intervalo, invertido (§12.2)
  superposicion  I_r = [p_r, p_r + (K_max_t(r) − K_min_t(r))]     (relativo: el desplazamiento comun −B·e no cambia la interseccion)
                 superponen(r, q) ⇔ min(hi_r, hi_q) − max(lo_r, lo_q) > ε_ov, con ε_ov = GeometryTolerance.Length
                 intervalos que se tocan o se solapan ≤ ε_ov no avisan; depende solo de diferencias s_r − s_q y de los tramos, no de
                 B ni de T; se calcula en FRAMES y se evalua en VALIDATE
                 (OD-7.b = A: aviso antes de los puntos, con los pares; OD-7.b = B: fallo)
```

**Tabla de verificacion** (OD-6.d = A, `φ_t = 0`, racks sin girar; la recta `e` sigue a `e_K`):

| Par | K | `k_s` | `k_t` | `w` | `e` | `α` |
|---|---|---|---|---|---|---|
| Rack Planta → Frontal | R | (0, 1) | (1, 0) | (1, 0) | (0, 1) | −90° |
| Rack Planta → Lateral | D | (1, 0) | (1, 0) | (1, 0) | (1, 0) | 0° |
| Rack Frontal → Planta | R | (1, 0) | (0, 1) | (0, 1) | (1, 0) | +90° |
| Rack Lateral → Planta | D | (1, 0) | (1, 0) | (1, 0) | (1, 0) | 0° |
| Cantilever Planta → Frontal | R | (−1, 0) | (−1, 0) | (−1, 0) | (−1, 0) | 0° |
| Cantilever Planta → Lateral | D | (0, 1) | (−1, 0) | (−1, 0) | (0, 1) | +90° |
| Cantilever Frontal → Planta | R | (−1, 0) | (−1, 0) | (−1, 0) | (−1, 0) | 0° |
| Cantilever Lateral → Planta | D | (−1, 0) | (0, 1) | (0, 1) | (−1, 0) | −90° |

- **Ida y vuelta.** Planta → Frontal y Frontal → Planta suman `α` = 0 en ambas familias (sin girar); Planta → Lateral y Lateral → Planta
  tambien. Lo que restituye una ida y vuelta, salvo una traslacion comun, es **solo el intervalo de cada referencia sobre K**:
  - la coordenada descartada no vuelve: una fila de plantas en R proyectada a Lateral da intervalos iguales en D y vuelve apilada;
  - las orientaciones no vuelven: un layout girado 30° vuelve alineado con los ejes, y un rack girado 180° vuelve sin girar, con el
    mismo intervalo;
  - es la identidad solo si todas las referencias comparten la coordenada descartada, `φ_r = φ_s` y `σ_s = σ_t`.
  Un layout de varias lineas solo vuelve sin superposicion de Planta → Frontal → Planta si esta escalonado en R, y de Planta → Lateral →
  Planta si esta escalonado en D (OV-ID19-21 usa un layout de cada tipo).
- **Defecto de V2 corregido.** V2 tomaba `w` de la familia rack en plantas y rechazaba Cantilever Frontal → Planta (`w(r) = (−1, 0)`
  perpendicular a `(0, 1)`).

**Ejemplo con tramos.** Dos plantas de rack sin girar, `P1` en (0, 0) y `P2` en (0, 100), con frontales destino de tramo R `[0, 90]`,
proyectadas a Frontal con `B = (0, 0)` y `T = (500, 0)`:
- `w = (1, 0)` y `e = (0, 1)`, asi que `σ_s = σ_t = +1`, `κ = 0` y `α = −90°`.
- Las frontales quedan en X ∈ [500, 590] y [600, 690] (en orden de R ascendente de los builders, que no es necesariamente el que ve
  una persona desde el pasillo).

Si `P2` esta girada 180° con su poste 0 en (0, 190):
- Hay empate entre los dos sentidos; se elige `e = (0, 1)`, cuyo angulo de 90° cae dentro de [−45°, 135°).
- `σ_s(P2) = −1`, `κ_s(P2) = K_max_t = 90` y `s = 190 − 90 = 100`.
- Su frontal ocupa [600, 690]: el mismo intervalo fisico que su tramo, anclada por el extremo opuesto.

Si ademas se anade `P3` girada 180° con su poste 0 en (0, 290), la mayoria cambia: `e = (0, −1)` y `α = +90°`. Las frontales quedan en
`P1` [410, 500], `P2` [310, 400] y `P3` [210, 300]: el orden sobre +X se invierte para todas (CONSECUENCIA NORMATIVA de arriba).

| Fuente | Destino | K | Eje comun en el destino | Eje descartado |
|---|---|---|---|---|
| Planta | Frontal | R | H: base comun | D (superposicion) |
| Planta | Lateral | D | H: base comun | R (superposicion) |
| Frontal | Planta | R | D: linea comun | H |
| Lateral | Planta | D | R: linea comun | H |
| Frontal o Lateral | Lateral o Frontal | — (sin eje horizontal comun) | la politica ortografica no aplica | `Rigid` lo representa |

**Familias mezcladas** (solo con OD-7.d = B). Cada referencia usa su propio `F_t(r)`. Se admiten sentidos de +K paralelos, opuestos o
iguales, y un sentido perpendicular falla (OM-12). Orientaciones no paralelas en la fuente fallan (OM-13).

### 12.6 Que representa la foundation

| Gesto | Foundation | V1 |
|---|---|---|
| Misma clase → misma clase | `Rigid` | M-01 (OD-7.a, OD-2.b) |
| Planta → Frontal o Lateral | `Orthographic` (tambien `Rigid`) | M-01 (OD-7.a) |
| Frontal o Lateral → Planta | `Orthographic` (tambien `Rigid`) | M-01 (OD-7.a, OD-6.d) |
| Frontal ↔ Lateral | `Rigid` | M-01 (OD-7.a) |
| Tipos de vista fuente mezclados | `Rigid` (cada referencia con su `a_s`) | M-01 (OD-7.c) |
| Layout fuente rotado | `Rigid`: orientaciones conservadas; `Orthographic`: `e` rotada | si |
| Racks girados 180° | `Rigid` y `Orthographic` (tramos) | si |
| Marco destino orientado | `φ_t` | M-01 (OD-6.b) |
| Rotacion comun del conjunto | `α` en `Rigid` | M-01 (OD-6.c) |
| Familias mezcladas | `Rigid`; `Orthographic` con el marco destino de cada familia y sentidos paralelos | M-01 (OD-7.d) |
| Varias definiciones del mismo `RackId` en la seleccion (duplicados legales, OQ-4) | `Rigid` (una definicion nueva por definicion fuente) | M-01 (OD-7.c; con A se rechazan) |

### 12.7 M-01 — decision material (Owner via Coordinador; revision tecnica del Arquitecto)

La foundation no cambia con ninguna eleccion; cambian validacion, mensajes, pruebas de politica y validacion del Owner.
**Paquete recomendado para V1: todas las opciones A** (recomendacion del Coordinador en G2C y G2D; **Owner = PENDING**). Las partes de OD-6 empiezan en `.b` a proposito: la exigencia de V1 de que
todas las fuentes compartan orientacion desaparece, porque la foundation ya representa orientaciones fuente distintas (INV-GRP-2); la
orientacion de las vistas nuevas se reparte entre OD-6.b, OD-6.c y OD-6.d (la opcion B de OD-6 en V1 es OD-6.d B).

| Parte | Decision | Opcion A | Opcion B | Compromiso | Recomendada | Consecuencia |
|---|---|---|---|---|---|---|
| OD-7.a | Modo por par | misma clase → `Rigid`; planta ↔ elevaciones → `Orthographic`; frontal ↔ lateral no expuesto | todo `Rigid` | A da elevaciones utiles de un layout en planta; B uniforme pero apila elevaciones | **A** | B deja la proyeccion para despues |
| OD-7.b | Filas superpuestas en `Orthographic` | aviso | fallo cerrado | A fiel; B obliga a seleccionar filas; separar filas no es opcion (romperia la transformacion comun, CD-04) | **A** | — |
| OD-7.c | Tipos fuente mezclados y varias definiciones de un `RackId` | fallo cerrado | `Rigid` (sustitucion por referencia) | A predecible; B mas permisivo pero con layouts dificiles de leer | **A** en V1 | — |
| OD-7.d | Familias mezcladas en `Orthographic` | fallo cerrado | cada referencia con su marco destino; direccion comun de la familia mayoritaria; sentidos perpendiculares fallan (§12.5) | A simple; B mezcla Cantilever y racks en una elevacion | **A** en V1 | B sin cambio de foundation |
| OD-6.b | Orientacion del marco destino `φ_t` | 0 (X universal), como toda insercion vigente (`P/Drawing/BlockPlacement.cs:240-244`) | X del SCP actual | A predecible; B orienta con el SCP | **A** en V1 | — |
| OD-6.c | `α` en `Rigid` (y por tanto `φ_s`) | 0: traslacion, como `COPY` (`φ_s = φ_t`) | pedir un angulo | A un gesto menos | **A** en V1 | — |
| OD-6.d | Orientacion de plantas proyectadas desde elevaciones | natural (corrida de las plantas de rack en Y) | girar para que K siga el X del marco | A coherente con las inserciones vigentes; B continuidad visual y permite familias mezcladas | **A** en V1 | — |
| OD-2.b | Variante destino en misma clase | conservar la de la fuente | canonica (OD-2) | A reproduce el layout de lo que se ve; B normaliza a una vista por sistema | **A** | La canonica de OD-2 queda para clase distinta |

### 12.8 Seleccion, grupos y clasificacion (D-08)

- **D-08a.** Nucleo neutral de seleccion de I-51 que I-52 V13 §5.1 propone extraer (`A/Persistence/RackDuplicationPlan.cs:225-582`).
  - Hechos neutrales en el nucleo; politicas por llamador.
  - **Una sola caracterizacion** de su semantica observable: la CT-16 de I-52, sobre codigo intacto y antes de cualquier extraccion. La
    corre el primer G3 que llegue y la otra iniciativa la reutiliza (AR2-13).
- **D-08b.** En un grupo `RackId`, las referencias de **una** definicion (celdas enlazadas, `P/RackLayoutCommands.cs:29-30`, `:226-257`) dan
  una definicion nueva y una referencia por cada referencia fuente, cada una con su ancla. Varias definiciones del mismo `RackId` se
  tratan segun OD-7.c.
- **D-08c.** Tipos fuente: segun OD-7.c.
- **D-08d.** Miembro que no soporta el par, el modo o la variante → fallo que lista los miembros (OD-3).
- **D-08e.** Proyectar una clase que el rack ya tiene es legal (OQ-4).
- **D-08f.** Datos del SNAPSHOT (§12.1) y reglas de fallo cerrado, todas antes de pedir puntos:

  | Dato | Regla |
  |---|---|
  | tipo | `MInsertBlock` → fallo |
  | `IsDynamicBlock`, `DynamicBlockTableRecord`, `IsAnonymous`, `Annotative` | referencia dinamica, anonima o anotativa con datos de RackCad → fallo (el barrido omite definiciones anonimas, `P/RackBlockFinder.cs:66`) |
  | `ScaleFactors` | `\|sx\| = \|sy\| = \|sz\| = 1` (tolerancia de escala compartida, X-7) y `sz > 0`; `(−1, −1, +1)` → giro de π; cualquier otra combinacion con algun signo negativo, incluida `(1, 1, −1)`, falla con «escala negativa no admitida». Una celda de `RACKLAYOUT` hereda la escala de su semilla (`P/RackLayoutCommands.cs:198`, `:250-254`) |
  | `Normal` | distinta de +Z → fallo |
  | SCP actual | plano XY no paralelo al universal → fallo |
  | `Origin` de la definicion | `≠ 0` **no** falla: entra en `M_r` |
  | espacio, bloque, datos | fuera de Model Space, no bloque o sin datos → aviso y se ignora, como I-51 |

- **D-08g.** Kind con `KindHandlerDispatch.TryResolve` (`P/RackMenuCommands.cs:141`), no con la variante de `RACKLAYOUT` (`P/RackLayoutCommands.cs:64`).
- **D-08h.** Codec y disponibilidad: `Coerced` e `Invalid` fallan en CLASSIFY; `Orphaned` y `Unsupported`, en AVAILABLE (§7.3 C).

### 12.9 Salidas bloqueadas, presentacion y materializacion

- **D-17.** `Resolve` compartido (§4.5); ID19 solo continua con `Resolved`. La paridad con el editor la caracteriza G3 por kind.
- **D-11 / OD-8.** Presentacion de creacion vigente.
- **D-13.** Requisito estructural por pieza (§4.6): comprobaciones puras 1 y 2 en PLANS, antes de los puntos; comprobacion 3 tras importar
  la union de claves y **antes de escribir**. Un fallo lista las piezas y no escribe ninguna definicion ni referencia de rack.
- **Una** transaccion con el primitivo compartido de X-4 (hoy `LateralHeaderDrawer.CreateSystemBlock(db, tr, plan, name)`,
  `P/Drawing/LateralHeaderDrawer.cs:28-29`, o `CantileverViewMaterializer`). Sin `Regen` ni purga.

## 13. Invariantes

| ID | Invariante | Prueba futura |
|---|---|---|
| INV-BOM-1..3 | Vistas hermanas y lotes no anaden racks; ID19 conserva racks y copias (una definicion nueva por definicion fuente, con k ≤ max referencias, tambien con varias definiciones fuente) | `RackViewCountInvariantTests` |
| INV-LIST-1..3 | `ViewCount` puede cambiar; copias sin cambio salvo 0 → 1; filas por `Id` | idem |
| **INV-AUTH-1** | I-55 nunca **crea** una hermana nueva con authored divergente de ninguna hermana propia del dibujo que quede en el: ni de las redibujadas (STOP ante cualquier unidad fallida) ni de las huerfanas (borrado verificado, o dentro de la transaccion del jig de la primera colocacion). Las dependientes de xref las gobierna su dibujo de origen (OM-25). No promete deshacer mutaciones legacy ya confirmadas, como los redibujos parciales antes de `REDRAW_FAILED` (§11.1, OM-3) | `RackSiblingRedrawOutcomeTests` (G9: incluidos el rack cuyas unicas vistas son huerfanas y el borrado de huerfanas incompleto), `RackViewBatchPlanTests`, `RackViewAuthoredEquivalenceTests` (G14) |
| **INV-AUTH-2** | Una hermana nueva nace con la coleccion canonica comun de su `RackId` (incluido el sobre fuente), o no nace | `RackSiblingCustomPropertiesGateTests` |
| **INV-RED-1** | Si una unidad del bucle de redibujo falla, no se escribe ninguna definicion nueva de la insercion ni se borra ninguna huerfana; el informe nombra las hermanas actualizadas, las actualizadas con aviso y la que fallo | `RackSiblingRedrawOutcomeTests` (G9), `RackViewBatchPlanTests`, `RackViewBatchDriverGuardTests` |
| **INV-BLK-1** | Toda instancia **presente en el plan** que requiere bloque de biblioteca tiene clave valida y presente tras importar, o ID19 no escribe ninguna definicion ni referencia de rack; ninguna instancia del plan desaparece del diagnostico por tener el nombre vacio. Las piezas que un builder omite antes del plan quedan fuera hasta OM-22 | `LibraryBlockRequirementTests` |
| **INV-RES-1** | ID19 solo coloca racks cuya resolucion sin editor es `Resolved` y cuyas hermanas pasarian el preflight de `RACKEDITAR`; un legado sin paridad nunca se aproxima | `RackSystemResolutionTests`, `RackGroupPlacementPlanTests` |
| **INV-GRP-1** | Una `CommonTransform2D` rigida por seleccion proyectada; diferencias de anclas destino = rotacion comun de diferencias de anclas fuente, para todo par de la seleccion | `CommonTransform2DTests`, `RackGroupPlacementPlanTests` |
| **INV-GRP-2** | Orientaciones destino: `Rigid` `ρ_r ≡ φ_r + α (mod 2π)`; `Orthographic` `ρ_r` depende solo de `φ_t` y del marco destino (familia, tipo, variante), nunca de la posicion ni la rotacion de `r` | `RackRigidPlacementPolicyTests`, `RackOrthographicPlacementPolicyTests` |
| **INV-GRP-3** | `Orthographic`, sobre la colocacion real: con `σ_s = σ_t`, cada punto fisico del tramo destino tiene la misma coordenada sobre el eje comun en la fuente y en la vista colocada (§12.2) | `RackOrthographicPlacementPolicyTests` (con mutaciones de `a_t` y `ρ` que deben fallar) |
| **INV-GRP-4** | `Orthographic`: cada vista destino ocupa sobre el eje comun el mismo intervalo que su tramo en la fuente (invertido si `σ_s ≠ σ_t`) | idem |

## 14. Cancelacion y errores (diseno)

| Operacion | Etapa | Evento | Que queda |
|---|---|---|---|
| ID17 rack nuevo | editor / prompt de variante / jig | cerrar el editor; Esc en el prompt; Esc o Enter en el jig | nada de rack; definicion borrada al cancelar el jig (vigente); definiciones de biblioteca importadas pueden quedar (OM-5). Enter en el prompt de variante acepta el valor por defecto |
| ID17 despues / ID18 existente | prompt de variante | Esc | `VARIANT_CANCELLED`: nada modificado (la variante se elige antes del gate y del redibujo, §11.1) |
| ID17 despues / ID18 existente | variante | no disponible (sin corte lateral en ese poste, seccion invalida, estacion ausente) | `ABORTED_BEFORE_WRITE`: nada modificado |
| ID17 | tras crear la definicion | excepcion | definicion sin referencias borrada en ambas primitivas (D-10; hoy queda: `P/Drawing/BlockPlacement.cs:49-52`; `P/RackCantileverCommands.cs:151-157`, `:174-177`) |
| ID17/ID18 | materializacion | faltan bloques | se coloca y se reporta (vigente), con el reporte del requisito estructural (§4.6): ninguna pieza desaparece |
| ID17 despues / ID18 existente | gate de propiedades | tipos distintos, hermana ilegible (tambien por `Id` legible en un payload no interpretable), divergentes | **nada**, ni redibujo; motivo y remedio condicionado |
| ID17/ID18 | barrido | F-14a (UTF-16 invalido) | falla el barrido; nada modificado |
| ID17 despues / ID18 existente | PREPARE ALL | fallo al preparar una vista nueva, o el plan o el payload de una hermana | nada modificado (`ABORTED_BEFORE_WRITE`) |
| ID17 despues / ID18 existente | redibujo legacy | falla una unidad (importacion, redefinicion, payload o renombre de una hermana propia del dibujo, dentro o fuera de su `try`), o el borrado verificado de huerfanas no las borra todas | STOP; **ninguna vista nueva** y ninguna huerfana borrada; las unidades confirmadas permanecen; informe de actualizadas, actualizadas con aviso y fallida; `Regen` si alguna confirmo (D-15, §11.1) |
| ID17 despues / ID18 existente | redibujo legacy | error despues del commit de una unidad | la unidad cuenta como «actualizada con aviso», no como fallida |
| ID17 despues / ID18 existente | redibujo legacy | falla el redibujo de una hermana dependiente de xref | aviso en el informe; Insertar continua (OM-25) |
| ID17 despues / ID18 existente | primera colocacion | rack cuyas unicas vistas son huerfanas | la referencia nueva y el borrado de las huerfanas confirman en la transaccion del jig; si el jig no confirma, la definicion nueva se retira con la limpieza vigente (best effort, D-10) y las huerfanas quedan |
| ID18 | colocaciones | Esc o Enter en la vista i | vistas anteriores colocadas; resto no; `CANCELLED_BEFORE_ANY` si i = 1; mensaje de cola parcial con los redibujos confirmados en un rack existente |
| ID18 | colocaciones | excepcion o `PromptStatus.Error` en la vista i | `FAILED(i)` o `FAILED_BEFORE_ANY`; mismo mensaje |
| ID19 | seleccion | no bloque / fuera de Model Space / sin datos | aviso; se ignora |
| ID19 | clasificacion | sobre ilegible del grupo, kind desconocido, `Coerced`, `Invalid`, MINSERT, dinamica, anonima o anotativa, escala negativa no admitida (cualquier signo negativo salvo `(−1, −1, +1)`), escala ≠ 1, normal o SCP | fallo, nada escrito, sin pedir puntos |
| ID19 | resolucion | `UnsupportedLegacy`, `Blocked`, `BrokenReference`, `DependencyUnavailable`, `Unreadable` | fallo con el motivo, nada escrito, sin pedir puntos |
| ID19 | disponibilidad / marcos / validacion | `Orphaned` o `Unsupported`; marco no caracterizado; par, modo o tipos no expuestos; mas de una familia con OD-7.d = A; orientaciones no paralelas; sentidos perpendiculares; superposicion con OD-7.b = B; `RackId` en blanco; miembro no soportado | fallo, nada escrito, sin pedir puntos |
| ID19 | validacion | superposicion con OD-7.b = A | aviso justo antes de pedir puntos, con los pares, solo si nada anterior fallo; se puede cancelar en PICK |
| ID19 | gates | authored o propiedades del grupo; hermana que el preflight de `RACKEDITAR` rechazaria | fallo con remedio; nada escrito, sin pedir puntos |
| ID19 | planes | fallo de un builder o del paso «sistema resuelto → plan»; clave nula, vacia, en blanco, invalida o fuera de la union (comprobaciones 1 y 2) | fallo, nada escrito, sin pedir puntos |
| ID19 | puntos | Esc (`Cancel`) o Enter (`None`) | nada (aun no se importo) |
| ID19 | puntos | `PromptStatus.Error` | fallo, nada escrito |
| ID19 | importacion y verificacion estructural | clave ausente tras importar (comprobacion 3) o biblioteca no disponible | fallo antes de escribir ninguna definicion ni referencia de rack, con la lista de piezas y la causa; bloques de biblioteca importados pueden quedar (OM-5) |
| ID19 | materializacion | excepcion | rollback de la transaccion unica |

## 15. Transacciones

| Flujo | PREPARE | MUTATE | `Regen` |
|---|---|---|---|
| ID17 vista unica | fuera de la transaccion | definicion + jig o limpieza | como hoy |
| ID18 rack nuevo | todo antes de escribir | por vista | ninguno; Cantilever uno al final |
| ID18 rack existente | variante + gate + PREPARE ALL (vistas nuevas, planes y payloads de hermanas) antes del redibujo | redibujo legacy con confirmacion por unidad, sin transaccion global (CD2-03; CQ-01); si todo bien, huerfanas y cola por vista (la primera colocacion borra las huerfanas si eran las unicas vistas) | los del redibujo, y uno tras `REDRAW_FAILED` si alguna unidad confirmo; Cantilever uno mas al final |
| ID19 | resolucion, disponibilidad, marcos, validacion, gates y planes antes de los puntos; importacion y comprobacion 3 despues | una transaccion | ninguno |

## 16. Recursos

| Recurso | Veces |
|---|---|
| Catalogo; catalogo de secciones | 1 por comando; 1 si hay Cantilever |
| `Resolve`; authored serializado | 1 por `RackId` |
| Registro de variables | 1 |
| Barrido de sobres | ID19: 1 (SNAPSHOT, del que salen tambien los gates); edicion: los vigentes + 1 lectura de las entradas del gate por Insertar |
| `CommonTransform2D` | 1 por seleccion proyectada |
| Importacion de bloques | ID19: 1 con la union de claves; verificacion estructural por pieza |

## 17. Reconciliacion con iniciativas activas

### 17.1 I-49 (`f6f0991`)

Desde G2, el Owner acepto ADR-0041, que reemplaza a ADR-0040, e I-49 versiono un Consensus Freeze nuevo sobre V6 + A1 + A2 (`3f17c21`;
`docs/automation/decisions/I-49.md` §15 en su rama). Despues publico la correccion G6-C2 (`75f1862`), solo en `A/Expressions/*` y sus
pruebas, con la que cerro G6. Durante la redaccion de V3 publico el Amendment A3 (`f6f0991`, solo documentacion: diagnosticos de un
simbolo con varias causas de fallo y su conjunto de causas raiz, dentro de `A/Expressions/*`); G7 sigue bloqueado antes de RED. A3 no
toca nada que consuma I-55: `Resolve` solo consumira el `PlanReadSet` de I-49 cuando integre (§4.5). Su produccion vive en `A/Expressions/*`, `A/Units/LengthUnits.cs` y
`A/StructuralSections/StructuralSectionUnits.cs`. **Sin archivos de
produccion comunes.** Archivo caliente: `U/Systems/Selective/RackSelectiveWindow.xaml.cs` (G10 de I-49; fila de I-53S,
`docs/ROADMAP.md:471`). Conflicto textual trivial en `docs/adr/README.md` (I-49 cambia las filas 0040/0041, contiguas a la de 0042).

### 17.2 I-52 (`dd45b0f`, Proposal V13, sin consenso)

**Estado de I-52.**
- **V11 (`2275f21`), [V11-D10]:** exige una **reconciliacion obligatoria** con I-55 antes de cualquier freeze, con la propiedad de las
  autoridades compartidas registrada sin duplicarlas y un STOP simetrico.
- **V12 (`915a520`), [V12-D11]:** marca **PROVISIONAL UNTIL CROSS-INITIATIVE RECONCILIATION** las partes de fundacion compartida: §3.7,
  §3.8, §5.1, §8.1 (plan, resolucion previa y tramo), §8.2, §8.3, §8.4, §8.5 y §20.1. `Mirror/` queda solo para lo propio del espejo.
- **V12, [V12-D17]:** registra la Proposal V2 de I-55, ADR-0042 V2 como complemento de ADR-0010 y los fixtures de C2-2b con las
  precondiciones nuevas de Insertar.
- **V13 (`dd45b0f`, publicada durante la redaccion de V3):**
  - [V13-D08] amplia lo provisional a §3.3, §3.7, §3.8, §5.1, §5.4, §6.1, §8.1..§8.5, §20.1, CT-04, CT-05 y CT-16. Recoge en §15.4 las
    filas X-1..X-8 acordadas en G2C (`d091eeb`) como **entrada vinculante, todavia no adoptada**, y exige volver a acordar cualquier
    desviacion antes del freeze, nunca de forma unilateral.
  - [V13-D09] registra a I-55 en `d091eeb` y espera que ADR-0042 «complementa y precisa» ADR-0010.
  - §15.3 y §15.4 preven releer si I-55 publica una Proposal V3 o cambia algun X.
  - Archivos permitidos por gate (§14.2) y exclusiones (§20.3) **sin cambio**. Ninguna reclamacion nueva de extraccion o ubicacion.

**Siguen siendo de I-52**, aunque consuman un primitivo compartido: ST-13, ST-18, ST-19, ST-20, PRE-17, PRE-18, PRE-19, E12,
`MaterializationContextReadSet`, `VisualFootprintResult`, `GeometrySymmetryResult` y la equivalencia con `μ_k`.

**I-55 adopta la misma regla:** sin la reconciliacion registrada por ambas no hay Consensus Freeze de I-55, ni tampoco G3.

**Base.** La tabla de reconciliacion de G2C queda **aceptada del lado de I-55** por el Coordinador (G2D). V3 la contrasta con V12 y con
V13, que no la cambian materialmente, y le incorpora las autoridades nuevas de V3: `Resolve` (§4.5), el requisito estructural de bloques (§4.6), el
codec separado de la disponibilidad (§7.3) y los tramos por variante (§12.3). La revision adversarial de V3 (§23) contrasto cada fila con
los archivos permitidos por gate y los excluidos de I-52, identicos en V12 y V13, y de ahi salen las reglas de extraccion de abajo.

**Diferencias de V3 con las filas de G2C que V13 §15.4 registra** (I-52 debe volver a acordarlas; V3 no las da por adoptadas):

| X | G2C (V13 §15.4) | V3 | Motivo |
|---|---|---|---|
| X-1 | extrae I-52 G4 (I-55 G13 solo si I-52 no lo extrajo) | primer gate I-52 G4 o I-55 G13; asignador de identidad solo en I-52 | Misma regla, simetrica |
| X-2 | extrae la primera (I-55 G6/G7 o I-52 G5/G6) | **extractor unico I-55** (G6, G7a, G7b) | El renombre y la delegacion tocan archivos que I-52 excluye (§20.3) |
| X-4 | primitivo de I-52 G6 | anade el requisito estructural puro (primer gate I-55 G7b o I-52 G6) y la consulta tras importar (primer gate I-55 G8 o I-52 G6) | Requisito estructural por pieza (CD2-04) |
| X-5 | protocolo antes de G3, G5, G7 y del Candidato | anade comprobaciones de I-52 antes de G4 y G6 | Reglas de extraccion |
| X-7 | valor de colocacion | anade la descomposicion de la transformacion fuente y la tolerancia de escala | Evitar dos clasificaciones de escala |
| X-8 | extrae la primera | **extractor unico I-55 G6** | Depende de la taxonomia de X-2 |

**Principio:** una autoridad por responsabilidad, extraida una sola vez, con politicas por llamador. Dos reglas de extraccion:
- **Extractor unico:** cuando extraer exige archivos que una iniciativa excluye (V13 §20.3), o la autoridad depende de otra de extractor
  unico, extrae solo la otra. Los archivos permitidos por gate (V13 §14.2) no fijan la regla: se ajustan en la tabla de adopcion.
- **Primer gate:** en los demas casos extrae la primera iniciativa que llega a su gate de creacion, siempre sobre la especificacion
  compartida registrada aqui; la otra consume y no crea una segunda.

En ambos casos, antes de cada gate que crea o consume una autoridad compartida, la iniciativa comprueba si ya existe en su base (X-5). Si
un gate necesita una autoridad que todavia no existe y no le toca extraerla: **STOP** de ese gate y decision de los dos Coordinadores.

**Restricciones de I-52 V13 que fijan las reglas** (no provisionales, identicas a V12): archivos permitidos por gate (G4: `Geometry/*` y `Persistence/`;
G5: `Mirror/`, `Systems/<Kind>/` y `RackFrames/`; G6: autoridades de plan por kind, `SystemBlockWriter.cs` y `Plugin/Systems/Shared/`) y
archivos excluidos de §20.3 (entre ellos `KindHandlers/*` y los editores WPF).

| X | Autoridad I-52 (V13) | Autoridad I-55 (V3) | Nucleo compartido | Politica I-52 | Politica I-55 | Custodio / extrae | Gate que la crea | Orden de consumo | Riesgo | Veredicto de compatibilidad |
|---|---|---|---|---|---|---|---|---|---|---|
| **X-1** Seleccion | Nucleo neutral + fachada `RackDuplicationPlan` (§5.1, provisional); SNAPSHOT propio (§5.2); asignador de identidad con politica de nombre inyectada (T-M05) | D-08a; SNAPSHOT propio (§12.1) | Clasificacion de fuentes de I-51 con predicado de kind inyectado, clave `RackId`/`Definition(handle)`, agrupacion y deduplicacion conservando referencias, consistencia de `Kind` y nombre, mensajes inyectados; **una caracterizacion: CT-16** (provisional en V13) | ST-* (payload antes del filtro de Model Space; `Origin ≠ 0` falla; `Id` en blanco → clave de definicion); el asignador de identidad es de I-52 y se queda en su G4 | Orden de I-51; `Id` en blanco falla (RID-4); kind sensible a mayusculas; sin asignador de identidad | Custodio I-52; **primer gate**: I-52 G4 o I-55 G13, con la misma especificacion | I-52 G4; I-55 G13 | CT-16 → creador → el otro | MEDIO | **COMPATIBLE** |
| **X-2** Taxonomia, codec, disponibilidad, `Resolve`, `Plan` | §3.7/§3.8 (codec y `A_k`), §6.1 `DescribeView`/`AdmissibleViews`, §8.1 `Build(request)` (que ya registra `Build` = `Resolve` + `Plan`), §8.4 (nucleo de decodificacion fuera de `Mirror/`) y §20.1: provisionales en V13; CT-04 provisional | `RackViewKind`; `RackViewAddressCodec` (sintaxis); `RackViewAvailability`; `RackSystemResolution` (§4.5); `RackViewPlanner` y `Prepare` (§4.4) | En `A/Views` y `A/Views/Resolution`: taxonomia; `Decode` sintactico; disponibilidad; `Resolve` con resultado tipado y contexto con el resultado de la lectura del registro; `Plan(sistema, direccion, contexto con semilla)` con todas las direcciones, la planta Cantilever con `PlantaVisibility` y nombre sugerido; **una caracterizacion: CT-04** | σ' canonica; fallo cerrado de `Coerced`/`Invalid`/`Orphaned`; `Expone`, `F`, derivacion de `c`, filtro μ_k; `A_k` = disponible ∩ expuesto; `Build` = `Resolve` + `Plan` sobre el diseno reflejado en memoria (C2-1 intacto); PRE-06/07/08/11 como politica | `RACKEDITAR` sin cambio; ID19 acepta `Canonical`/`Canonicalizable` y `Available`; editores llaman a `Plan` con su sistema; ID19 solo `Resolved`; BOM por kind | Custodio I-55; **extractor unico I-55**: el renombre toca editores WPF y la delegacion toca `KindHandlers/*` (V13 §20.3); G4..G6 de I-52 tampoco escriben en `A/Views` | I-55 G6 (taxonomia, codec, disponibilidad, soporte, `RackViewFrame`), G7a (`Resolve` + delegacion de los seis handlers), G7b (`Plan`, `Prepare`) | CT-04 → I-55 G6/G7a/G7b → I-52 G4 (codec, ST-15), G5 (disponibilidad, marco, `Resolve` en P3/P4, `DescribeView`), G6 (`Build`). Si I-52 llega antes: STOP de ese gate y los Coordinadores eligen (a) adelantar el gate de I-55, sin cambio observable (solo posible tras el G3 de I-55, que exige su consenso, M-01, ADR-0042 y OD-1..OD-8), o (b) que I-52 extraiga con una excepcion registrada de su §14.2 y §20.3 que cubra los mismos archivos, la misma especificacion y el renombre y la delegacion en el mismo gate | ALTO: acopla el orden de gates de I-52 a I-55 | **COMPATIBLE WITH CLARIFICATION** (I-52 debe adoptar el acoplamiento) |
| **X-3** Comparador authored | `IsSameAuthority` por kind, declarado por su reflector (§5.4, §6.1 y §8.5, provisionales en V13); V13 §15.4 ya registra como pendiente «comparador en el contrato del kind; el reflector lo referencia» | `RackSiblingAuthoredGate` (§9.3) | `IsSameAuthority(miembros)` por kind en el **contrato del kind** (`Systems/<Kind>/`); el `IsSameAuthority` del reflector **delega** en el; Selectivo `SelectiveAuthoredAuthority.IsSameAuthority`; excluye la metadata de `P/RackCommandSupport.cs:40-67` | Miembros = vistas seleccionadas; E3 | Miembros = todas las hermanas; falla antes de PICK | Custodio I-52; **primer gate**: I-52 G5 o I-55 G14 | I-52 G5a/b/c; I-55 G14 (consume si ya existe) | Creador → el otro delega | MEDIO | **COMPATIBLE** |
| **X-4** Primitivo de materializacion y requisito estructural | §8.2 materializador (provisional); PREPARE con importacion y re-verificacion por instancia (§8.1) | Consumo en G15; requisito estructural (§4.6) | Primitivo: despacho por familia de plan + `RackBlockData.Write` + referencia con Z y presentacion explicitas; sin lock, commit, regeneracion ni purga. Requisito: parte pura en `A/Drawing/LibraryBlockRequirement.cs` (requisito por pieza, predicado de nombre); consulta a la tabla de bloques en el Plugin | ST-13, ST-19, ST-20, PRE-17, PRE-18, PRE-19, read-set y huellas en el llamador | Presentacion de creacion (OD-8 A); `Z_r`; escala 1; fallo antes de escribir; ID17/ID18 conservan colocar y reportar | Primitivo: custodio I-52, extrae I-52 G6, I-55 G15 consume (extrae solo si llega antes, con la firma acordada). Requisito puro: **primer gate**, I-55 G7b o I-52 G6. Consulta tras importar (`P/Systems/Shared/`): **primer gate**, I-55 G8 o I-52 G6 | I-52 G6; I-55 G7b (requisito), G8 (consulta), G15 (primitivo) | Creador → el otro consume | MEDIO: `SystemBlockWriter.cs` en la lista STOP de I-52 (X-5); G-M24 con modo de creacion; I-52 G6 necesita `A/Drawing/LibraryBlockRequirement.cs` en sus archivos | **COMPATIBLE WITH CLARIFICATION** |
| **X-5** Protocolo | §15.2, §15.3 (antes de G3, G5, G7 y del Candidato), §19; G4 solo avisa | Reporte en G5, G6, G7a, G7b, G8, G9, G10, G12, G13, G14, G15 | Sin autoridad productiva: obligacion de comprobar si la autoridad compartida existe y de reportar antes de fijar archivos | Re-evaluar §8.1, §8.7, read-set, PRE-17, PRE-18, T-GRD-02, censo `B`; **comprobacion tambien antes de G4 y G6** (adopcion) | Reporte con la lista de re-evaluacion; nada congela una autoridad incompatible | — | — | Cada gate listado comprueba y reporta antes de fijar archivos | BAJO | **COMPATIBLE WITH CLARIFICATION** |
| **X-6** Lineas base de C-2 | C2-1 = C2-2a = C2-2b = C2-3 = C2-4 y G-M9 (§8.5, provisional en comparador y lineas base) | Cambia productores: G5 (PR-2), G8 (re-enrutado de Insertar), G9 (gate, variante, D-15 y huerfanas), G12 (orden de edicion) | **Un unico juego** de fixtures y un mapa G-M9 sobre el `main` combinado; fixture de planta Cantilever con brazos y tensores visibles | C-2 por defecto; C-1 solo condicional | Goldens de G7b y fixtures de Insertar de G8 | Custodio I-52; la segunda en integrar re-establece C-2 | I-52 G6; re-establecimiento en los gates de la segunda | La segunda corre C-2 y G-M9 antes de su Candidato | ALTO: requiere que el `Plan` compartido reciba `PlantaVisibility`, para que PR-2 siga solo en el Plugin en cualquier orden | **COMPATIBLE** |
| **X-7** Valor de colocacion y transformacion fuente | `Transform2D.ReflectionAboutLine` + `(Position2D, RotationRadians, UniformScale)` (§3.3 y §8.3 provisionales en V13 para nombre, ubicacion y API del valor; el algebra de reflexion y §3.2 siguen de I-52); clasificacion pura ST-7/8/9/10 y canonizacion `(−s, −s)` → `θ + π` (T-M03, G4); tolerancia de escala fijada en G4 (§4.3) | `CommonTransform2D`; colocaciones (Position, ρ, 1); escala y signos (§12.2, D-08f) | `Transform2D` + **un** valor de colocacion con su composicion y su descomposicion validada; `T_common` = (T − R(α)·B, α, 1); Z y `Origin` fuera del valor; **una** descomposicion pura de la transformacion fuente y **una** tolerancia de escala | Reflexion; escala `s ≠ 1` admitida (ST-11); `Origin ≠ 0` falla (ST-14); Z de la fuente | Rigidez (`det = +1`, escala 1); `Origin` dentro de `M_r`; Z por politica | Custodio I-52 (`A/Geometry/`); **primer gate**: I-52 G4 (que exige ADR-0036 aceptado) o I-55 G14 | I-52 G4 (T-M01..T-M03); I-55 G14 | Creador → el otro consume la misma especificacion | BAJO-MEDIO | **COMPATIBLE** (§3.2 consumido sin cambio) |
| **X-8** Origen y tramo del eje | «tramo del eje de la vista (para c)» en `ViewPlanResult` (§8.1, provisional); `c` en coordenadas locales de la vista (§6.1); CT-05 (provisional en V13), CT-06 | `RackViewFrame` con `[K_min, K_max]` por variante (§12.3) | **Un** descriptor por (sistema, tipo, variante): mapa de ejes con signo, origen fisico local, `[K_min, K_max]` en coordenadas fisicas, convencion de extremos; conversion a `c` a traves del origen; **una caracterizacion: CT-05** | `c` derivada del tramo convertido a coordenadas de la vista; δ registrado (CT-06); solo vistas admisibles | Anclas por extremo del tramo destino segun σ | Custodio I-55; **extractor unico I-55 G6** (depende de la taxonomia de X-2) | I-55 G6 | CT-05 → I-55 G6 → I-52 G5/G6 e I-55 G14 | BAJO-MEDIO | **COMPATIBLE WITH CLARIFICATION** |

**Contraste con V12 y V13.** Ninguna fila contradice el contenido de las secciones no provisionales de I-52 (en V13, §3.2 y §5.2 entre
las citadas): las autoridades nuevas se construyen debajo de ellas sin cambiar su semantica. Las aclaraciones de V3 afectan a partes
provisionales (en V13: §3.3, §3.7, §3.8, §5.1, §5.4, §6.1, §8.1..§8.5, §20.1, CT-04, CT-05 y CT-16) y al **proceso** de I-52: comprobaciones antes de G4 y G6, un archivo mas en G6 y el acoplamiento de G4..G6
a los gates de I-55 que extraen X-2. **No hay MATERIAL CONFLICT de contenido.** El acoplamiento de orden (X-2) exige que I-52 lo adopte;
si su Coordinador o su Arquitecto lo rechazan, es un conflicto que se escala (abajo).

#### 17.2.1 Tabla de adopcion para el registro de decisiones de I-52

Para copiar al registro de I-52 si su Coordinador y su Arquitecto la aceptan. I-55 no escribe en la rama de I-52.

| X | Autoridad | Ubicacion propuesta | Custodio | Extrae | I-52 consume en | I-55 consume en | I-52 conserva como politica propia | I-52 ajusta en su contrato | Estado |
|---|---|---|---|---|---|---|---|---|---|
| X-1 | Nucleo neutral de seleccion | `A/Persistence/` + fachada `RackDuplicationPlan` | I-52 | primer gate: I-52 G4 / I-55 G13 | G4, G7 | G13, G14, G15 | ST-*; asignador de identidad (T-M05, G4) | Una caracterizacion CT-16 compartida | Aceptada por I-55 (G2C/G2D); pendiente de I-52 (V13 §15.4 recoge la fila de G2C) |
| X-2 | Taxonomia `RackViewKind`; codec sintactico; disponibilidad; `Resolve`; `Plan`; `Prepare` | `A/Views`, `A/Views/Resolution` | I-55 | **solo I-55**: G6, G7a, G7b | G4 (codec para ST-15), G5 (disponibilidad, marco, `Resolve` en P3/P4, miembros de `DescribeView`), G6 (`Build` = `Resolve` + `Plan`) | G6..G15 | σ', exposicion, μ_k, `A_k` como filtro, fallo cerrado, PRE-06/07/08/11 | §3.7, §3.8, §8.1, §8.4 y §20.1 remiten a `A/Views`. §6.1 sin cambio: sus miembros se construyen sobre codec, disponibilidad, `RackViewFrame` y `Resolve`. §9.1: P1 conserva la decodificacion sintactica y la disponibilidad (`Orphaned`, E4) se evalua con el sistema resuelto, en P4. PRE-06 usa el resultado de la lectura del registro que `Resolve` recibe en su contexto. `UnsupportedLegacy` → E6 (propuesta). §14.2: precondicion «la pieza de X-2 existe en la base» en G4, G5 y G6, o la excepcion (b) de §17.2 | Aceptada por I-55; pendiente de I-52 |
| X-3 | `IsSameAuthority(miembros)` por kind | contrato del kind (`Systems/<Kind>/`) | I-52 | primer gate: I-52 G5 / I-55 G14 | G5 | G14 | miembros = vistas seleccionadas; E3 | V13 §15.4 ya lo lista como pendiente con el mismo contenido: el `IsSameAuthority` del reflector delega en el comparador del contrato del kind | Aceptada por I-55; pendiente de I-52 |
| X-4 | Primitivo de materializacion; requisito estructural de bloques | Plugin `Systems/Shared/` (primitivo y consulta tras importar); `A/Drawing/LibraryBlockRequirement.cs` | I-52 | primitivo: I-52 G6; requisito puro: primer gate, I-55 G7b / I-52 G6; consulta: primer gate, I-55 G8 / I-52 G6 | G6, G7 | G7b, G8, G15 | ST-13, ST-19, ST-20, PRE-17, PRE-18, PRE-19, read-set, huellas | §8.2: Z y presentacion explicitas; requisito por pieza; §14.2 G6 anade `A/Drawing/LibraryBlockRequirement.cs` | Aceptada por I-55; pendiente de I-52 |
| X-5 | Protocolo de comprobacion y reporte | — | ambas | — | §15.3 anade G4 y G6 | cada gate listado | — | §15.3: comprobacion de existencia antes de G4 y G6 | Vigente en I-55; ajuste pendiente de I-52 |
| X-6 | Juego unico de C-2 y mapa G-M9 | pruebas | I-52 | I-52 G6 | G6 | G5, G7b, G8, G9, G12 (re-establece si integra segunda) | C-1 condicional | Fixture Cantilever con visibilidad de planta; C2-2b con las precondiciones nuevas de Insertar (variante antes, gate, D-15, huerfanas) | Aceptada por I-55; pendiente de I-52 |
| X-7 | Valor de colocacion sobre `Transform2D`; descomposicion de la transformacion fuente; tolerancia de escala | `A/Geometry/` | I-52 | primer gate: I-52 G4 / I-55 G14 | G4 | G14 | reflexion; escala `s ≠ 1`; `Origin ≠ 0` falla | §3.3 y §8.3 (provisionales en V13) remiten a la especificacion compartida; §3.2 sin cambio | Aceptada por I-55; pendiente de I-52 |
| X-8 | `RackViewFrame` con `[K_min, K_max]` | `A/Views` | I-55 | **solo I-55**: G6 | G5, G6 (`c` por el origen) | G14 | `c` y δ | Una caracterizacion CT-05 compartida; `c` en coordenadas de la vista derivada del tramo fisico | Aceptada por I-55; pendiente de I-52 |

Si I-52 no la acepta, o su Coordinador o su Arquitecto encuentran una incompatibilidad material, **STOP**: I-55 no la resuelve
unilateralmente y la escala a los dos Coordinadores.

### 17.3 I-53D (en `dad4e77`)

Sin cambios en su lote de cabeceras; `D34_GUARD_ElCensoDeModalesDirectosNoCrece_YLosGestosNuevosNoAbrenNinguno` se reapunta en ID17.

### 17.4 Prerrequisitos aislados (D-09; CD-07)

| # | Hallazgo | Evidencia | Por que bloquea | Correccion |
|---|---|---|---|---|
| **PR-1** | H-01 | `U/Systems/PushBack/RackPushBackSystemWindow.xaml.cs:3036-3045`, `:3065` | `Post(p)` no confiable | Seccion desde `corte.PostIndex` con enteros de hoy |
| **PR-2** | H-02 | `P/RackCantileverCommands.cs:131`, `:312`; `null` = apagados (`A/Systems/Cantilever/CantileverViewPlanBuilder.cs:293-295`); el diseno guarda la eleccion (`D/Systems/Cantilever/CantileverLineDesign.cs:303-309`, `:362-364`) | La planta Cantilever soportada no es fiel al diseno que muestra brazos o tensores | **Solo en el Plugin:** pasar `design.PlantaVisibility` en las dos llamadas; guarda de fuentes con mutacion + prueba de comportamiento del builder; OV especifica; sin cambios laterales |

H-13 no es prerrequisito (§22.5).

## 18. Alternativas

| Nivel | Opcion | Veredicto |
|---|---|---|
| Arquitectura | ALT-A por sistema / **ALT-B foundation + adaptadores** / ALT-C framework / ALT-D registro persistente | **ALT-B** |
| Foundation de ID19 | Traslacion literal unica | Rechazada como semantica unica; representable con `Rigid` y `α = 0` |
| Foundation de ID19 | Proyeccion ortografica unica (V1) | Rechazada (CR-01) |
| Foundation de ID19 | **Group Placement rigido + politicas** | **Elegida** |
| Foundation de ID19 | Afin general | Rechazada (vistas 1:1) |
| Alcance de la transformacion | Una por grupo `RackId` | **Rechazada**: no preserva el layout entre racks |
| Direccion destino ortografica | Tomada de una familia global «rack» (V2) | **Rechazada** (AR2-01): rechaza pares Cantilever soportados |
| Gate de propiedades | Autoridad global de I-54 | Rechazada: bloquea por payloads no relacionados |
| Gate de propiedades | Ignorar todo payload no interpretable | Rechazada: dejaria pasar hermanas ilegibles cuyo `Id` es legible |
| Redibujo de hermanas | PREPARE → una transaccion MUTATE para todos los redibujos → POST (purga y `Regen`) | **No adoptada en V3 por prescripcion del Coordinador (CD2-03)**, no por inviabilidad: el primitivo existe (`SystemBlockWriter.RedefineInTransaction`, I-47 G9) y `ProjectVariableMutationExecutor` ya lo usa. Costes reales: envoltorios de preparacion que faltan en Dinamico, Push Back y Cabecera, transaccion de tamano proporcional a las vistas y divergencia con Actualizar. Evidencia elevada al Coordinador (CQ-01, §22.3) |
| Verificacion de bloques | Union de nombres distintos (V2) | **Rechazada** (AR2-03): una pieza sin nombre desaparece de la union |
| Resolucion sin editor | «Usar el camino del BOM» tal cual (V2) | **Rechazada** (AR2-04): el handler no expone el sistema ni el resultado tipado. (El ejemplo de legado aceptado por el BOM y no por el editor que citaba la revision tecnica de V2 es inalcanzable, §4.5) |
| Codec | Codec que consulta el sistema resuelto (V2) | **Rechazada** (AR2-05): mezcla sintaxis y disponibilidad e impide compartirlo |
| Nombre y plan | Plan que necesita el nombre unico | **Rechazada**: introduce I/O de dibujo en el plan; se usa una semilla (§4.4) |

## 19. Pruebas (resumen; matriz en el mapa §T)

| Clase | Clases previstas |
|---|---|
| **Caracterizacion (G3)** | `RackViewEnvelopeReadingCharacterizationTests` (= CT-04 de I-52), `LegacyViewPayloadCompositionCharacterizationTests`, `LegacyViewBlockNameCharacterizationTests`, `RackCountInvariantCharacterizationTests`, `RackSiblingScanInputCharacterizationTests`, `RackViewFrameCharacterizationTests` (= CT-05, con tramos por variante), `HeadlessResolutionParityCharacterizationTests` (paridad por kind de `Resolve` frente al editor y al sistema dibujado, con fixtures legacy como el Dinamico solo-sistema), `RackEditPreflightCharacterizationTests` (preflights de hermanas de `RACKEDITAR`), `BuilderCatalogSkipCensusTests` (censo de piezas que los builders omiten sin bloque en el catalogo), `BomResolutionCharacterizationTests` (BOM y motivos de bloqueo vigentes por kind), `LibraryBlockRequirementCharacterizationTests` (roles que requieren bloque en la salida de los builders), `FirstViewGateCharacterizationTests`; condicional `RackDuplicationPlanObservableSemanticsTests` (= CT-16) |
| **T0** | `PushBackLateralPostIndexRegressionTests`, `FirstViewFreedomTests`, `EditorViewBatchRequestTests`, `RackViewBatchDialogTests`, `RackSelectionCoreConsumptionTests` (si I-52 ya extrajo el nucleo), `RackViewKindRenameTests`, `RackViewVariantTests`, `RackViewAddressCodecTests` (solo sintaxis, §7.3 A), `RackViewAvailabilityTests` (§7.3 B), `RackViewSupportMatrixTests`, `RackViewFrameTests`, `RackSystemResolutionTests` (resultados tipados y legado), `RackViewNameSeedTests`, `RackViewPlannerTests`, `LibraryBlockRequirementTests`, `RackViewPreparation*Tests`, `PreparedViewPayloadGoldenTests`, `RackIdLifecycleTests`, `RackEnvelopeIdProbeTests`, `RackSiblingCustomPropertiesGateTests`, `RackSiblingAuthoredGateTests`, `RackViewAuthoredEquivalenceTests`, `RackSiblingRedrawOutcomeTests` (G9: fallo en la unidad k, informe, cero colocaciones, huerfanas, `Regen`), `RackViewBatchPlanTests` (redibujo parcial, variante, Esc y Enter), `CommonTransform2DTests`, `RackGroupFrameTests`, `RackRigidPlacementPolicyTests`, `RackOrthographicPlacementPolicyTests` (los ocho pares de §12.5, ida y vuelta con lo que restituye y lo que no, tramos, superposicion con `ε_ov`, sentido por mayoria, empate en la frontera, INV-GRP-3 e INV-GRP-4 sobre la colocacion real, las ocho combinaciones de signo de escala), `RackGroupPlacementPlanTests` (politicas de consumidor de ID19), `RackViewCountInvariantTests`, `RackViewPartialRackContractTests`, `CantileverPlantaVisibilityBuilderTests` |
| **T1** | `*DimensionView*`; `CustomPropertiesEnvelopeGuardTests`; `CustomPropertiesEdgeGuardTests` (sin cambio); `KindHandlerResolutionDelegationGuardTests`; `RackViewPlacementGuardTests`; `RackViewBatchDriverGuardTests`; `RackViewEntryPathGuardTests`; `RackProjectionCommandGuardTests`; `RackSiblingGateWiringGuardTests`; `RackSiblingGateIndependenceGuardTests`; `CantileverPlantaVisibilityGuardTests`; `CustomPropertiesCommandGuardTests`; `SelectiveEditorOpenTests`; `CantileverPluginSourceGuardTests`; `SelectiveAuthoredCarrierAdoptionTests`; `PushBackPluginSourceGuardTests`; `RackUnitsGuardSourceTests`; `RackDuplicationPlanTests`; UI: `RackInsertionRequestTests`, `RackEditorSessionTests`, `EditorModuleRegistryTests`, `DynamicShellMigrationTests`, `RichEditorBlockedActionTests`, `DynamicHeaderBatchSeamGuardTests`, `SelectiveHeaderBatchSeamTests`, `WindowCensusGuardTests`, `DialogWindowCharacterizationTests`, `CustomPropertiesHelpCensusTests` |
| **T2/T3/T4** | suites completas; CI y coberturas; validacion del Owner |

## 20. Validacion del Owner (diseno; no ejecutar)

Mapa §OV:
- OV-LEG.
- OV-ID17.
- OV-ID18: incluye Esc y Enter en la cola, Esc en el prompt de variante sin modificar nada, el redibujo fallido con su informe y el rack
  cuyas unicas vistas son huerfanas.
- OV-META: incluye propiedades divergentes, el remedio con `RACKPROPIEDADES` en solo lectura y payloads ajenos no interpretables.
- OV-ID19, por modo y segun M-01. Incluye:
  - Cantilever → Planta y Cantilever → Lateral;
  - ida y vuelta con un layout escalonado en R (Frontal) y otro escalonado en D (Lateral);
  - racks de longitudes distintas, rack girado 180° y cambio de mayoria que invierte el orden;
  - layout girado a mano;
  - layout enlazado espejado rechazado;
  - Dinamico solo-sistema: proyectado solo si su paridad de G3 lo permite; si no, rechazado con `UnsupportedLegacy`;
  - rack con una hermana que `RACKEDITAR` rechazaria, rechazado;
  - fallo por pieza sin bloque;
  - 100 o mas racks.
- OV-PR.

## 21. Gates (resumen; detalle y tabla V2 → V3 en el mapa)

| Gate | Objetivo | Precondicion |
|---|---|---|
| G3 | Caracterizacion | Coordinator y Architect `AGREED` sobre la misma version + **M-01 resuelta** + reconciliacion con I-52 registrada ([V11-D10], [V12-D11], [V13-D08]) + Consensus Freeze + ADR-0042 aceptado + OD-1..OD-8 |
| G4 | PR-1 | G3 |
| G5 | PR-2 (solo Plugin) | G3 |
| G6 | Foundation pura: taxonomia, codec sintactico, disponibilidad, soporte, `RackViewFrame` con tramos | G4, G5 |
| G7a | `Resolve` compartido + delegacion de los handlers del BOM (seis kinds) sin cambio observable; extractor unico (X-2) | G6 |
| G7b | `Plan` con semilla, `Prepare`, requisito estructural de bloques (parte pura) | G7a |
| G8 | Colocacion de vista unica + D-10 + prompt + fixtures de Insertar + reporte estructural en todo camino de insercion | G7b |
| G9 | Precondiciones nuevas de `RACKEDITAR` → Insertar (vista unica): variante antes, gate de propiedades, PREPARE ALL antes del redibujo, D-15 con informe y huerfanas | G8 |
| G10 | ID17 | G9 |
| G11 | ID18 contrato puro (estados, incluidos `SIBLING_GATE_FAILED`, `ABORTED_BEFORE_WRITE` y `REDRAW_FAILED` con informe) | G10 |
| G12 | ID18 UI, presentador, driver (gate, PREPARE ALL, redibujo y D-15 en lote, Esc y Enter) y rutas de entrada | G11 |
| G13 | Nucleo neutral de seleccion (X-1) | reconciliacion X-1 |
| G14 | ID19 puro: Group Placement, politicas corregidas, grupos, clasificacion, resolucion, disponibilidad, gates, valor de colocacion | G12, G13 |
| G15 | ID19 comando + primitivo y verificacion estructural (X-4) + censos | G14; OD-1; OD-8 |
| G16 | Candidato e integracion | todo |

## 22. Registro de decisiones

### 22.1 Decisiones tecnicas propuestas

| # | Decision | Seccion | Revision |
|---|---|---|---|
| D-01 | ALT-B | §4 | Arquitecto |
| D-02 | `DimensionViewKind` → `RackViewKind`; `CantileverViewKind` como enum de camara | §7.1 | **A-4** |
| D-03 | Variante tipada; **codec solo sintactico**, **disponibilidad** sobre el sistema resuelto y **politica** en cada consumidor | §7 | **A-1**, X-2 |
| D-04 | Matriz normativa | §5 | Coordinador |
| D-05 | `RackId` al aceptar la intencion; curacion vigente; en blanco = `IsNullOrWhiteSpace` | §6, §9.2 | Arquitecto |
| D-06 | ID18 transaccion C | §11 | **A-3** |
| D-07 | Gates acotados al `RackId` (propiedades con sonda de `Id`; authored) | §9 | **A-7**, X-3 |
| D-08 | Seleccion, grupos, clasificacion y SNAPSHOT completo (D-08a..h) | §12.8 | **X-1**, Arquitecto |
| D-09 | PR-1 y PR-2 (solo Plugin) antes de la foundation | §17.4 | Coordinador |
| D-10 | Limpieza ante excepcion: solo definiciones sin referencias, best effort y registrada; purga de anidadas declarada | §14, §22.1 detalle | Arquitecto |
| D-11 | Presentacion de creacion | §12.9 | OD-8 |
| D-12 | ADR-0042 **complementario** de ADR-0010 (A-8 = a) | ADR, §22.1 detalle | **A-8**, Owner |
| D-13 | **Requisito estructural de bloques por pieza** (instancias del plan), fallo antes de escribir en ID19 e I-52 (comprobaciones puras antes de los puntos); ID17/ID18 conservan colocar y reportar, con reporte en todo camino | §4.6 | Arquitecto, X-4 |
| D-14 | Una autoridad por responsabilidad con I-52 | §17.2 | **X-1..X-8** |
| D-15 | Variante antes del redibujo; una unidad de redibujo fallida (miembro del gate o dependiente de xref) impide crear vistas nuevas; las confirmadas permanecen; informe exacto; STOP en el primer fallo; huerfanas borradas con la primera colocacion (vista unica en G9; lote en G12). Sin transaccion global por CD2-03, pendiente de CQ-01 | §11.1 | Arquitecto + Coordinador |
| **D-16** | Group Placement: marcos y **una** `CommonTransform2D` rigida por seleccion proyectada | §12.2-§12.3 | **A-6** |
| **D-17** | **`Resolve` compartido** con resultado tipado y politica por consumidor (BOM por kind); ID19 solo con `Resolved` y hermanas aceptables para `RACKEDITAR`; paridad en G3 con fixtures legacy; extractor unico I-55 G7a | §4.5 | Arquitecto (D-17) |
| **D-18** | Politicas `Rigid` y `Orthographic`; etapa FRAMES; direccion destino desde el marco destino de cada referencia; recta comun por suma alineada y sentido por mayoria; tramos por variante; INV-GRP-3 e INV-GRP-4 | §12.1-§12.5 | **A-6**, M-01 |
| **D-19** | Contrato `Resolve` / `Plan` / `Prepare` con semilla de nombre (estrategia B) que reproduce los nombres base vigentes | §4.4 | **X-2**, Arquitecto |

**D-10, detalle (AR2-15).**
- La limpieza reutiliza el borrado vigente: solo borra definiciones **sin referencias**, en modo best effort y con registro
  (`P/Drawing/BlockPlacement.cs:130-169`).
- Una excepcion posterior a confirmar la referencia conserva la vista.
- La purga de definiciones anidadas sin referencias (`:144-162`) puede alcanzar definiciones de biblioteca que ya estaban en el dibujo sin
  referencias. Es un efecto vigente al cancelar, que D-10 extiende a las excepciones.
- En el Cantilever, `definitionId` vive dentro del `try` (`P/RackCantileverCommands.cs:142-161`), asi que la limpieza va dentro de
  `PlaceDefinition`.

**D-12, detalle — formulacion final de A-8.** ADR-0042 **complementa** ADR-0010 y **no lo reemplaza**; ADR-0010 sigue `aceptado`.
ADR-0010 acota su propio alcance, cita literal: «Esta decisión gobierna el flujo de edición. No cambia la inserción inicial que crea un
rack nuevo ni promete capacidades multivista para todos los editores.» (`docs/adr/0010-actualizar-redibuja-insertar-liga-vistas.md:45-46`).

ADR-0042 anade dos cosas sin revertir ninguna decision de ADR-0010:

1. **Generaliza el origen valido de una hermana.**
   - A: un rack logico materializado. Es la regla de ADR-0010 («Una vista adicional solo se inserta desde un rack ya existente, para que
     disponga de diseño e identidad fuente.», `:40-41`).
   - B: una intencion de creacion aceptada con `RackId` unico, authored, sistema resuelto y vistas preparadas, en el flujo de creacion
     que ADR-0010 deja fuera de su alcance.
2. **Anade precondiciones al flujo de Insertar**, que ADR-0010 delega en «su flujo implementado» (`:36-37`): el gate de propiedades del
   `RackId` y D-15.

La nota posterior fechada en ADR-0010 que enlace ADR-0042 **solo se escribe cuando el Owner acepte ADR-0042**; hasta entonces ADR-0010
no se modifica. El reemplazo, completo o acotado, no se usa. Es el contenido de la formulacion «complementa y precisa» que I-52 V13 espera
([V13-D09]): las dos precisiones son las de arriba, sin enmendar ninguna decision de ADR-0010. Hay precedentes de complemento (ADR-0028, ADR-0029) y de nota que enlaza un
ADR posterior en un aceptado no reemplazado (`docs/adr/0005-estrategia-de-unidades.md:125`).

### 22.2 Decisiones del Owner

**Owner = PENDING.** El Coordinador recomienda todas las A (G2C, G2D). No hay decision del Owner registrada.

| # | Decision | Opcion A | Opcion B | Compromiso | Recomendada | Consecuencia |
|---|---|---|---|---|---|---|
| OD-1 | Nombre del comando | `RACKPROYECTAR` + `RPY` | `RACKPROYECTARVISTAS` | A corto; B explicito | **A** | Censos, ayuda, README |
| OD-2 | Variante en ID19 (clase distinta) | canonica fija | preguntar por sistema | A determinista | **A** | En misma clase decide OD-2.b (M-01) |
| OD-3 | Miembro no soportado | fallar todo | omitir con aviso | A predecible | **A** | Reseleccionar |
| OD-4 | Esc o Enter a mitad de lote | detener | saltar | A: Esc y Enter = parar (el jig trata ambos igual) | **A** | Cola parcial |
| OD-5 | Orden del lote | F → L → P | orden de marcado | A determinista | **A** | El dialogo lo muestra |
| **OD-6** | Marcos (M-01: 6.b, 6.c, 6.d) | §12.7 | §12.7 | §12.7 | **A, A, A** | Validacion y politica |
| **OD-7** | Modos (M-01: 7.a, 7.b, 7.c, 7.d) | §12.7 | §12.7 | §12.7 | **A, A, A, A** | Idem |
| OD-8 | Presentacion de vistas proyectadas | creacion vigente | de la fuente | A coherente con Insertar | **A** | B exige capturar presentacion |

### 22.3 Open Material

- **M-01 — Semantica de Group Placement / Orthographic Projection** (§12.7): marco fuente, marco destino, modo por par, tipos y familias
  mezclados y variante en misma clase (OD-6.b/c/d, OD-7.a/b/c/d, OD-2.b).
  - **Abierta:** Owner PENDING.
  - Mientras siga abierta no hay `Coordinator = AGREED` ni consenso (CR-02), y no puede abrirse G3.
- **Reconciliacion con I-52: X-1..X-8** (§17.2). Compatible con V12 y V13 con las aclaraciones de V3 (extractor unico de X-2 y X-8, reglas de primer
  gate en X-1, X-3, X-4 y X-7, y ajustes de gates de I-52). Queda abierta hasta que I-52 la adopte en sus registros (tabla de §17.2.1);
  ninguna de las dos congela antes.
- **CQ-01 — Redibujo de Insertar (decision del Coordinador).** La revision adversarial de V3 encontro que el primitivo de MUTATE en la
  transaccion del llamador ya existe y se usa (§11.1). V3 sigue CD2-03 (sin transaccion global). El Coordinador decide si mantiene CD2-03
  o si esta es la «evidencia nueva material» que su prescripcion admitia; en el segundo caso, V4 cambiaria D-15, §11, §14, §15, INV-RED-1
  y G9. Sin esa decision no hay `Coordinator = AGREED`.

### 22.4 Open Minor

| # | Tema |
|---|---|
| OM-1 | `SaveToLibrary` acuna antes en Cantilever y Push Back |
| OM-2 | Convergencia futura de `CantileverViewKind` (enum de camara, §7.1) |
| OM-3 | Atomicidad del redibujo multivista: un `REDRAW_FAILED` deja el rack redibujado en parte (vigente, §11.1) |
| OM-4 | Uniformar la eleccion de variante en vista unica |
| OM-5 | Bloques importados que sobreviven a fallos (vigente) |
| OM-6 | Vista fantasma de grupo en ID19 |
| OM-7 | `View` nulo de la Cama (F-04) |
| OM-8 | «frontal ×N» de `RACKLISTA` (H-12) |
| OM-9 | Lateral entero del Dinamico solo legado |
| OM-10 | Duplicados de `(RackId, View, Section)` legales (OQ-4) |
| OM-12 | `Orthographic` con familias mezcladas (solo OD-7.d = B) y sentidos perpendiculares falla |
| OM-13 | `Orthographic` con orientaciones no paralelas falla; `Rigid` las admite |
| OM-14 | Una vista nueva hereda el `ExtensionData` del sobre de otra vista (vigente) |
| OM-15 | Frontal ↔ Lateral y misma clase dependen de M-01 |
| OM-17 | `FindRackBlocks` no expone dependencia de xref ni texto de payload: el gate los obtiene por su lectura |
| OM-18 | El remedio `RACKPROPIEDADES` puede exigir reparar antes un payload ajeno o una unificacion bloqueada (R-15) |
| OM-19 | Un payload no interpretable sin `Id` legible no es atribuible y no bloquea (CR-03); riesgo residual R-14 |
| OM-20 | ID17 e ID18 conservan «colocar y reportar» ante bloques faltantes; pasar a fallo antes de escribir seria una decision de producto (§4.6) |
| OM-21 | La purga vigente de anidadas sin referencias puede retirar definiciones de biblioteca preexistentes sin referencias (D-10) |
| OM-22 | Piezas que los builders omiten antes del plan cuando su fila de catalogo no tiene bloque: decidir por rol si fallan o son opcionales (§4.6) |
| OM-23 | Sin remedio automatico para un `Kind` en blanco o mezclado entre hermanas: `RACKPROPIEDADES` lo muestra en solo lectura (§9.2) |
| OM-25 | Las definiciones dependientes de xref que el bucle vigente redibuja no son hermanas a efectos de D-15 ni de INV-AUTH-1: su fallo es un aviso (§9.2) |
| OM-24 | En `Orthographic`, el sentido del eje comun lo decide la mayoria de referencias: girar racks puede invertir el orden de toda la elevacion (§12.5). Una normalizacion fija se descarto en V2 (Proposal V2 §23, hallazgo 26); se pondera dentro de M-01 |

Retirados de V1: **OM-11** (`Id` interior del Cantilever desalineado) sale con H-13 (§22.5); **OM-16** (sin aviso de propiedades
divergentes al insertar) lo resuelve el gate de §9. OM-12, OM-13 y OM-15 conservan su tema, reformulado.

### 22.5 Hallazgos registrados fuera de I-55

**H-13 — `Id` interior del Cantilever sin alinear al crear.**
- **Evidencia:** `D/Systems/Cantilever/CantileverLineDesign.cs:333`; `U/Systems/Cantilever/RackCantileverWindow.xaml.cs:199-210`;
  `P/RackCantileverCommands.cs:472-498`.
- Solo lo leen pruebas: `T/CantileverLineTests.cs:908` y `T/CantileverRoundTwoCharacterizationTests.cs:372`.
- Candidato a iniciativa independiente; I-55 no lo toca.

**H-14 — El editor Dinamico no es idempotente al releer la tarima.**
- **Mecanismo:** `Num` formatea con `"0.##"` (`U/Systems/Dynamic/RackDynamicSystemWindow.xaml.cs:3447-3450`). El fondo de la tarima se
  escribe en la caja (`:3273`) y se relee (`:1409`). `MustRebuild` compara tarimas con tolerancia 1e-6
  (`A/Systems/Dynamic/DynamicEditorDesignAssembler.cs:44-61`).
- **Efecto:** un fondo persistido con mas de dos decimales puede reconstruir los modulos en un Actualizar sin cambios.
- **Estado:** sin validar en AutoCAD; I-55 no lo corrige. No afecta la paridad de `Resolve` con el sistema persistido (§4.5).

## 23. Revision adversarial de V3 (antes del commit)

**Metodo y transparencia.** Cuatro revisores de solo lectura de **esta misma sesion** (la que redacta V3) atacaron el borrador contra el
codigo de `dad4e77` y contra I-52 V12 (`915a520`), con ejemplos numericos propios: geometria de ID19; bloques, `Resolve` y codec;
redibujo, propiedades y cola; X-1..X-8. Cada hallazgo se verifico despues en el codigo o en V12 antes de admitirlo; los que no pudieron
verificarse del todo se marcan PLAUSIBLE. Tras corregir, un quinto revisor comprobo las correcciones (§23.3). **No es la revision del
Arquitecto independiente**, que sigue PENDING.

### 23.1 Los veinte ataques

| # | Ataque | Borrador | Hallazgos | Estado en V3 |
|---|---|---|---|---|
| 1 | Transformacion por `RackId` accidental | Resiste (una `T_common`; `α` de una sola `w` y `e`) | AV3-50 (redaccion de PLANS por grupo) | Corregido: FRAMES una vez sobre toda la seleccion |
| 2 | Cantilever → Planta | Resiste: las cuatro filas Cantilever de la tabla son correctas; el origen D interior funciona | — | Sin cambio |
| 3 | Rack → Planta | Resiste: las cuatro filas Rack son correctas, incluidos los cortes del lado B | — | Sin cambio |
| 4 | Fuente girada 180° | Se rompe la estabilidad del orden | AV3-21, AV3-22 | Sentido por mayoria documentado como consecuencia normativa (OM-24); recta comun y empate definidos con `ε_ang` |
| 5 | Ida y vuelta ortografica | Se rompe: afirmaba restituir el layout | AV3-07 | Corregido: solo restituye el intervalo sobre K; OV-ID19-21 dividida |
| 6 | Multiples filas | Se rompe: la superposicion se evaluaba antes de tener sus datos | AV3-06, AV3-53 | Corregido: etapa FRAMES; `ε_ov` |
| 7 | Tamanos distintos | Resiste (ejemplo con tramos 90 y 150) | — | Sin cambio |
| 8 | Variante con distinto tramo | Resiste con precisiones | AV3-51, AV3-52 | Corregido: prefijo de la maestra; tramo = cuadricula |
| 9 | Nombre de bloque ausente | Se rompe en parte: la particion por rol es correcta; la etapa y la linea base no | AV3-24, AV3-25, AV3-26, AV3-59 | Corregido; piezas omitidas por builders censadas (OM-22) |
| 10 | Pieza sin bloque requerido | Resiste (falla cerrado) | AV3-58 | Precisado: presencia, no procedencia; biblioteca no disponible |
| 11 | Dinamico legado aceptado por BOM y no por editor | Se rompe: el caso es inalcanzable y faltaban preflights y la Cama | AV3-08, AV3-27, AV3-28, AV3-29 | Corregido |
| 12 | Sintaxis valida, variante no disponible | Resiste para ID19; politica de `RACKEDITAR` mal descrita | AV3-31, AV3-60 | Corregido: politica por kind; disponibilidad con catalogo |
| 13 | Fuente `Coerced` | Se rompe en parte: una direccion erronea, etapa inconsistente, definiciones ambiguas | AV3-30, AV3-32, AV3-34, AV3-35 | Corregido |
| 14 | `CustomProperties` divergentes | Se rompe: hermanas huerfanas con diseno viejo tras Insertar | AV3-03, AV3-18, AV3-45, AV3-46 | Corregido; R-19 |
| 15 | Fallo de redibujo parcial | Se rompe: justificacion falsa, orden del prompt, frontera de STOP, remedio en bucle, sin pruebas | AV3-04, AV3-05, AV3-15, AV3-16, AV3-17, AV3-44 | Corregido; **CQ-01** al Coordinador |
| 16 | Terminacion del lote con Enter/Esc | Se rompe en parte: prompt de variante y puntos de ID19 | AV3-05, AV3-19, AV3-47, AV3-48 | Corregido |
| 17 | X-1..X-8 duplicando autoridades | Se rompe: dos gates creaban el requisito estructural y la extraccion de X-2 por I-52 era imposible | AV3-01, AV3-02, AV3-09..AV3-14, AV3-36..AV3-43 | Corregido: extractor unico y reglas de primer gate; sin MATERIAL CONFLICT de contenido; I-52 debe adoptar el acoplamiento de X-2 |
| 18 | Ciclo Plan / nombre | Resiste: no hay ciclo y `Prepare` es previo a escribir | AV3-33, AV3-61 | Corregido: la semilla reproduce los nombres base; fusion de largueros |
| 19 | Signo de la escala Z | Resiste | AV3-55 | Precisado: ocho combinaciones y mensaje |
| 20 | Copias con `RACKLAYOUT` enlazado | Resiste con OD-7.c = A | AV3-20, AV3-49 | Corregido: una definicion nueva por definicion fuente; aviso de vistas enlazadas |

### 23.2 Hallazgos

> **Nota posterior a la revision.** Los hallazgos se formularon contra I-52 V12. V13 (`dd45b0f`), publicada antes del commit, marca
> provisionales §3.3, §5.4, §6.1, CT-04, CT-05 y CT-16: AV3-09 y AV3-39 quedan superados (§6.1 y §3.3 ya son provisionales) y AV3-12 en
> parte; el texto de §17.2 esta actualizado a V13. Los archivos por gate y las exclusiones que sostienen AV3-01 no cambian.

**BLOCKER: ninguno. HIGH: ocho, todos corregidos antes del commit.**

| # | Sev. | Hallazgo (evidencia) | Resolucion en V3 | Seccion |
|---|---|---|---|---|
| AV3-01 | HIGH | La extraccion de X-2 «por el primero» era imposible para I-52: el renombre toca editores WPF y la delegacion toca `KindHandlers/*`, excluidos por V12 §20.3; G4..G6 de I-52 no escriben en `A/Views`; I-52 G5 necesita resolucion (PRE-07, PRE-08) antes de su G6 | Extractor unico I-55 (G6, G7a, G7b) para X-2 y X-8; I-52 consume en G4, G5 y G6; si llega antes, STOP y decision de los Coordinadores (adelantar el gate de I-55 o excepcion registrada) | §4.5; §7.1; §17.2; §17.2.1 |
| AV3-02 | HIGH | Dos gates creaban el requisito estructural puro (I-55 G7b incondicional e I-52 G6), en ubicaciones distintas | Regla de primer gate y ubicacion fija `A/Drawing/LibraryBlockRequirement.cs` | §4.6; §17.2; mapa G7b |
| AV3-03 | HIGH | Insertar creaba una hermana con diseno nuevo mientras las huerfanas conservaban el viejo cuando eran las unicas vistas (`P/RackSelectivoCommands.cs:229-239`, `:249-258`); INV-AUTH-1 falso | Huerfanas borradas con borrado verificado o, si eran las unicas vistas, dentro de la transaccion del jig de la primera colocacion; INV-AUTH-1 sobre las hermanas propias del dibujo (precisado en §23.3, VF-04 y VF-05) | §9.3; §11.1; §13; §14 |
| AV3-04 | HIGH | La razon para rechazar una transaccion unica era falsa: `SystemBlockWriter.RedefineInTransaction` existe y `ProjectVariableMutationExecutor` redibuja con PREPARE, una MUTATE y POST | Justificacion corregida; el diseno sigue CD2-03; evidencia elevada como **CQ-01** | §11.1; §18; §22.3; ADR-0042 |
| AV3-05 | HIGH | Orden indefinido entre el prompt legacy de variante (hoy despues del redibujo) y PREPARE ALL | Variante antes del gate; `VARIANT_CANCELLED`; variante no disponible → `ABORTED_BEFORE_WRITE` | §10; §11.1; §11.2; §14 |
| AV3-06 | HIGH | VALIDATE evaluaba paralelismo, sentidos y superposicion con datos que solo producia PLANS | Etapa FRAMES sobre toda la seleccion; avisos justo antes de PICK | §12.1; §12.5 |
| AV3-07 | HIGH | La ida y vuelta no restituye el layout (fila apilada, giro de 30° perdido, rack girado que vuelve sin girar) | Enunciado exacto de lo que se restituye; OV-ID19-21 con layouts escalonados en R y en D | §12.5; mapa OV |
| AV3-08 | HIGH | El `DynamicSystem` sin `DynamicDesign` es inalcanzable (`A/Systems/Shared/SystemRegistry.Default.cs:74-87`); el legado real (solo sistema, diseno con valores por defecto) resolvia como `Resolved` sin paridad | Paridad obligatoria con fixture solo-sistema; `UnsupportedLegacy` sin miembro conocido | §4.5; mapa G3, G7a; OV-ID19-23 |
| AV3-09 | MEDIUM | V12 §6.1 descrito como provisional | No provisional, consumido sin cambio | §17.2 |
| AV3-10 | MEDIUM | Mover `Orphaned` a la disponibilidad cambia la precedencia de errores de I-52 §9.1 | En la tabla de adopcion, con E4 conservado | §17.2.1 |
| AV3-11 | MEDIUM | Firma de `Resolve`: lectura del registro, diseno en memoria de I-52, nombre sugerido | Contexto con el resultado de la lectura; entrada persistida o en memoria; nombre sugerido = semilla | §4.4; §4.5 |
| AV3-12 | MEDIUM | Comparador authored contra V12 §5.4 y §6.1 | El reflector delega en el comparador del contrato del kind; §5.4 y §6.1 sin cambio | §17.2 |
| AV3-13 | MEDIUM | I-52 no comprueba existencia antes de G4 y G6 | Ajuste de su §15.3 en la tabla de adopcion | §17.2.1 |
| AV3-14 | MEDIUM | Extractor del valor de colocacion inconsistente; descomposicion de la fuente y tolerancia de escala sin fila X | Primer gate I-52 G4 o I-55 G14; X-7 incluye descomposicion y tolerancia | §17.2; §12.2 |
| AV3-15 | MEDIUM | Frontera de STOP incompleta (planes fuera del `try`, borrado de huerfanas, renombres) | Unidad de redibujo definida; planes de hermanas en PREPARE ALL | §11.1 |
| AV3-16 | MEDIUM | «Ejecuta Actualizar» como remedio vuelve a fallar en silencio | Mensaje «corrige la causa y repite Insertar» | §11.1; §11.2 |
| AV3-17 | MEDIUM | D-15 en G9 sin prueba de comportamiento | `RackSiblingRedrawOutcomeTests` con ejecutor puro | mapa G9; §19 |
| AV3-18 | MEDIUM | Redibujos fallidos de hermanas dependientes de xref ignorados frente a «si uno falla, se detiene» | Se alinearon ADR-0042 y la Proposal; el alcance final lo fija VF-05: las dependientes de xref son aviso, no STOP | §9.2; §11.1; ADR-0042 |
| AV3-19 | MEDIUM | Enter en un prompt de entero con valor por defecto no cancela | Tabla de §14 dividida por prompt y jig | §10; §14 |
| AV3-20 | MEDIUM | «Una definicion nueva por grupo» inflaria copias con OD-7.c = B | Una definicion nueva por definicion fuente | §12.1; §13; ADR-0042 |
| AV3-21 | MEDIUM | El sentido por mayoria puede invertir toda la elevacion al girar racks | Consecuencia normativa, ejemplo, prueba, OV-ID19-22, R-21 y OM-24 | §12.5 |
| AV3-22 | MEDIUM | `e` no definida exactamente; empate sin tolerancia | Suma alineada independiente del orden; empate con `ε_ang` | §12.5 |
| AV3-23 | MEDIUM | INV-GRP-3 era una tautologia | INV-GRP-3 e INV-GRP-4 sobre la colocacion real | §12.2; §13 |
| AV3-24 | MEDIUM | Comprobaciones puras de nombre tras los puntos; «no se escribe nada» falso con la importacion | Comprobaciones 1 y 2 antes de los puntos; «ninguna definicion ni referencia de rack» | §4.6; §12.1; §14; ADR-0042 |
| AV3-25 | MEDIUM | Linea base «el drawer omite en silencio» erronea; los caminos silenciosos son comandos | Linea base corregida; guarda de G8 sobre todos los caminos | §3; §4.6; mapa G8 |
| AV3-26 | MEDIUM | Builders que omiten piezas antes del plan | Alcance de INV-BLK-1; censo en G3; OM-22 | §4.6; §13 |
| AV3-27 | MEDIUM | Cantilever `Blocked` cambiaria el BOM | Politica del BOM por kind | §4.5 |
| AV3-28 | MEDIUM | ID19 podia anadir vistas a un rack que `RACKEDITAR` rechaza | Preflights en la columna de aceptacion; fallo en GATES; caracterizacion y OV-ID19-24 | §4.5; §9.3; §12.1 |
| AV3-29 | MEDIUM | La Cama conservaba resolucion privada | Sexto handler que delega | §4.5; mapa G7a |
| AV3-30 | MEDIUM | Direccion `Fondo(0)` erronea para un `Coerced` del Selectivo | `Fondo(max(Section, 0))` | §7.3 A |
| AV3-31 | MEDIUM | Politica de `RACKEDITAR` mal descrita (lateral invalida borrada; lado B redibujado como A) | Politica por kind | §7.3 C |
| AV3-32 | MEDIUM | Etapa de fallo de `Coerced` e `Invalid` inconsistente | CLASSIFY; AVAILABLE solo sobre direcciones validas | §7.3 C; §12.1; §14 |
| AV3-33 | MEDIUM | La semilla con solo nombre y direccion no reproduce los nombres base vigentes | Semilla con sistema y catalogo; caracterizacion de todas las funciones | §4.4; mapa G3, G7b |
| AV3-34 | LOW/MEDIUM | Vacio y tokens con espacios indefinidos | Reglas de lectura y peor disposicion | §7.3 A |
| AV3-35 | LOW/MEDIUM, PLAUSIBLE | Dos formas del lateral Dinamico sin seccion con disposiciones distintas | Ambas `Coerced` | §7.3 A |
| AV3-36 | LOW | Referencia equivocada a la politica de I-52 | V12 §7.9 y §9.1 | §4.5 |
| AV3-37 | LOW | Faltaba I-52 G4 como consumidor | Anadido | §17.2 |
| AV3-38 | LOW | G14 sin clausula de consumo del comparador | Anadida | mapa G14 |
| AV3-39 | LOW | V12 §3.2 y §3.3 descritos como provisionales | Corregido | §17.2; mapa |
| AV3-40 | LOW | I-52 G5 como creador de `RackViewFrame`; conversion a `c` | Extractor I-55 G6; conversion por el origen; δ en CT-06 | §12.3; §17.2 |
| AV3-41 | LOW | Asignador de identidad fuera del nucleo X-1 | Queda en I-52 G4 | §17.2 |
| AV3-42 | LOW | Reapunte de la instantanea de enums de I-52 | Lo decide el Coordinador de I-52 | §7.1 |
| AV3-43 | LOW | Citas desalineadas con V12 | `RackDuplicationPlan.cs:225-582`; lista de §8.4 exacta | §12.8; mapa G3 |
| AV3-44 | LOW, PLAUSIBLE | «Confirmada» frente a `Success` | Confirmada = commit; «actualizada con aviso» | §11.1 |
| AV3-45 | LOW | Huerfanas como miembros y remedio del `Kind` mezclado | Precisado; OM-23 | §9.2 |
| AV3-46 | LOW, PLAUSIBLE | Claves duplicadas en distinta capitalizacion | R-19 | §9.2; mapa R |
| AV3-47 | LOW | `CANCELLED_BEFORE_ANY`, `Error`, RID-5 | Anadidos y precisado | §6; §11.2 |
| AV3-48 | LOW, PLAUSIBLE | Estados de `GetPoint` en ID19 | `AllowNone = true`; `None`, `Cancel`, `Error` | §12.1 |
| AV3-49 | LOW | Una proyeccion de misma clase parece copia | Aviso final | §12.1; OV-ID19-05 |
| AV3-50 | LOW | PLANS se leia por grupo | FRAMES sobre toda la seleccion | §12.1 |
| AV3-51 | LOW | Fondo «desplazado» | Prefijo de la maestra | §12.3 |
| AV3-52 | LOW, PLAUSIBLE | Tramo: cuadricula o lo dibujado | Cuadricula del sistema resuelto | §12.3 |
| AV3-53 | LOW | Tolerancia de superposicion sin sentido | `ε_ov` | §12.5 |
| AV3-54 | LOW | Angulos sin modulo 2π | Normalizacion | §12.2 |
| AV3-55 | LOW | Signos de escala y mensaje | Ocho combinaciones; «escala negativa no admitida» | §12.2; D-08f; mapa G14 |
| AV3-56 | LOW | ADR-0042 mas estrecho que la Proposal en definiciones por grupo | = AV3-20 | ADR-0042 |
| AV3-57 | LOW | «Mismo orden» no dice la referencia | «Orden de R ascendente» | §12.5; mapa OV |
| AV3-58 | LOW | La comprobacion 3 prueba presencia, no procedencia | Precisado | §4.6 |
| AV3-59 | LOW | Predicado de nombre de tabla de bloques sin definir | En Application, caracterizado en G3 | §4.6 |
| AV3-60 | LOW | La disponibilidad necesita el catalogo; conjunto vacio | Catalogo en la entrada; `Unsupported` con motivo | §4.1; §4.2; §7.3 B |
| AV3-61 | LOW | Lateral Selectivo sin fusion de largueros | Anadida | §4.3 |
| AV3-62 | LOW | Citas imprecisas (`RackEmbedComposer.cs:54`, ruta de regeneracion de la cabecera, escritura de la Cama) | Corregidas | §4.5; §7.3 |

### 23.3 Pase de verificacion de las correcciones

Un quinto revisor de solo lectura (misma sesion) comprobo las ocho correcciones HIGH contra el texto, el codigo y V12, y recalculo los
ejemplos de §12.5 con las formulas escritas (pares sin girar, rack girado 180°, cambio de mayoria, INV-GRP-3 e INV-GRP-4 no tautologicas).
Resultado: AV3-01, AV3-02, AV3-04, AV3-05, AV3-07 y AV3-08 resueltos; AV3-03 y AV3-06 resueltos en parte. Sus hallazgos, corregidos
antes del commit:

| # | Sev. | Hallazgo | Resolucion |
|---|---|---|---|
| VF-01 | MEDIUM | Saltar `UnsupportedLegacy` en el BOM cambiaria el BOM del Dinamico solo-sistema | El BOM cotiza `UnsupportedLegacy` como hoy con `sistemaLegado` (§4.5) |
| VF-02 | MEDIUM | PREPARE ALL planificaba tambien las huerfanas | Solo se planifican las hermanas que el bucle vigente redibuja; las huerfanas se clasifican (§11.1) |
| VF-03 | MEDIUM | La definicion de huerfana contradecia al Push Back (lado B redibujado como A) | Huerfana = la que el bucle vigente borra como fantasma, por kind (§9.2) |
| VF-04 | MEDIUM | El borrado de huerfanas no era atomico: `EraseViewBlocks` se traga las excepciones y la definicion nueva se confirma antes del jig | Borrado verificado (incompleto → STOP); en el caso de solo huerfanas, borrado dentro de la transaccion del jig y limpieza vigente si el jig no confirma (§11.1, §13, §14) |
| VF-05 | MEDIUM, PLAUSIBLE | Contar las hermanas dependientes de xref en D-15 podia bloquear Insertar para siempre | Las gobierna su dibujo de origen: aviso sin STOP, fuera de INV-AUTH-1 (§9.2; OM-25; R-22) |
| VF-06 | MEDIUM | FRAMES calculaba `s_r`, que depende de B, y necesitaba `ρ_r` de PLANS | FRAMES calcula `p_r` y `ρ_r`; PLACE calcula `s_r = p_r − B·e`; intervalos relativos (§12.1, §12.5) |
| VF-07 | LOW/MEDIUM | §22.3 conservaba la lista antigua de reglas X | Corregida |
| VF-08 | LOW | El mapa no nombraba el gancho de `BlockPlacement` para las huerfanas | Mapa G9 y F.2 |
| VF-09 | LOW | La consulta tras importar de X-4 no tenia regla de extraccion; la regla de extractor unico mezclaba exclusiones y archivos por gate | Primer gate I-55 G8 o I-52 G6 en `P/Systems/Shared/`; la regla de extractor unico se basa en §20.3 y en dependencias (§17.2) |
| VF-10 | LOW | Punteros sueltos (STOP de X-2, excepcion (b) en G6 y G7b, ADR decision 9, opcion (a) antes del G3 de I-55) | Corregidos |
| VF-11 | LOW | La cita de la semilla incluia el nombre unico del Cantilever | Solo `:287-312`; `:318-336` es del Plugin (§4.4) |
| VF-12 | LOW | INV-AUTH-1 citaba una clase de G14 para un comportamiento de G9 | `RackSiblingRedrawOutcomeTests` (G9) |
| VF-13 | LOW | PLANS por grupo `RackId` no cubre variantes distintas con OD-7.c = B | Por destino (definicion fuente y variante) |
| VF-14 | LOW | Faltaba la transicion de variante no disponible en §11.2; «sobrevive» en la ida y vuelta; precision de la evidencia de CQ-01; cita `:209-215` | Corregidos |

**Estado de publicacion:** sin BLOCKER ni HIGH abiertos. Quedan abiertos, sin BLOCKER ni HIGH: CQ-01 (decision del Coordinador), OM-22..OM-25
y la adopcion por I-52 del acoplamiento de X-2.
