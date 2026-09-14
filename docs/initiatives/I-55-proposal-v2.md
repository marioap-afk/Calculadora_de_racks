# I-55 — Proposal V2: View Placement & Projection (ID17 + ID18 + ID19)

```text
PROPOSAL V2 — NOT CONSENSUS
Coordinator    = REVIEW REQUIRED
Architect      = REVIEW REQUIRED
Consensus      = NOT REACHED
Implementation = BLOCKED
Open Material  = M-01 (propio: semantica de Group Placement / Orthographic Projection, marco fuente y marco destino)
                 X-1..X-8 (entre iniciativas, con I-52)
ADR-0042       = PROPUESTO (complementa y enmienda reglas acotadas de ADR-0010; no lo reemplaza)

Initiative     = I-55 — View Placement & Projection
Branch         = feature/creacion-de-vistas
BASE_SHA       = ba497f14581d81e83a27514852d6ec082ff57635   (base original; codigo auditado por el Discovery)
CURRENT_BASE   = dad4e77f4f267b9fa248ecb0c8bfd8a74bbab093   (merge de I-53D)
CURRENT_MAIN   = dad4e77f4f267b9fa248ecb0c8bfd8a74bbab093   (preflight de G2B: main no avanzo)
Proposal V1    = d1918ab9a44ce7ae391aeacc10ff6686c8807a61   (historico; Coordinator = CHANGES REQUIRED)
Paquetes V1    = 39fa75964a5875b3fbbb61186e385b254ded083a   (historico)
Paralelas      = I-49 75f1862 (G6-C2 sobre el Consensus Freeze V6 + A1 + A2 de 3f17c21; ADR-0041 aceptado)
                 I-52 2275f21 (Proposal V11: reconciliacion obligatoria con I-55 antes de cualquier freeze, [V11-D10])
Decisiones     = docs/automation/decisions/I-55.md   (G2B: CR-01..CR-12)
Mapa           = docs/initiatives/I-55-implementation-map-v2.md   (misma version del plan)
ADR            = docs/adr/0042-preparacion-de-vistas-antes-de-materializar.md
Citas          = archivo:linea sobre CURRENT_BASE, verificadas antes del commit (§23)
Prefijos       = P/ src/RackCad.Plugin/ · A/ src/RackCad.Application/ · D/ src/RackCad.Domain/ · U/ src/RackCad.UI/
                 T/ tests/RackCad.Tests/ · TU/ tests/RackCad.UI.Tests/
```

> **Como leer V2.** §0 reconcilia el veredicto del Coordinador sobre V1. El resto es autocontenido; V1 queda como registro
> historico sin modificar. **Vocabulario de ID19:** una **seleccion proyectada** es el conjunto de referencias aceptadas en **una**
> ejecucion de ID19; un **grupo `RackId`** es el subconjunto de la seleccion que comparte un `RackId`. La transformacion comun es
> **una por seleccion proyectada**, nunca por grupo `RackId`.

## 0. Reconciliacion V1 → V2

**Veredicto recibido:** `Coordinator = CHANGES REQUIRED` sobre V1 (`d1918ab`); el Arquitecto no emitio veredicto sobre V1.

| # | Hallazgo del Coordinador | Resolucion en V2 | Donde |
|---|---|---|---|
| CR-01 | La foundation de ID19 de V1 estrecha el contrato | **Dos niveles:** (1) **Group Placement** con `SourceGroupFrame`, `TargetGroupFrame` y **una** `CommonTransform2D` rigida por seleccion proyectada, aplicada a los anclas; (2) politicas **`Rigid`** y **`Orthographic`** encima. La foundation representa misma clase, Frontal ↔ Lateral, Planta ↔ elevaciones, fuentes rotadas y destino orientable | §12 |
| CR-02 | `Open Material = NONE` con OD-6/OD-7 abiertas | **M-01 material**; OD-6 y OD-7 reformuladas dentro de M-01, mas las partes que la revision adversarial encontro (variante en misma clase, tipos fuente mezclados, familias mezcladas) | §12.7, §22.3 |
| CR-03 | Rechazo de copiar la coleccion en un rack divergente | **Gate acotado al `RackId`** con la igualdad canonica de I-54: comun → hereda; divergente → fallo; hermana ilegible del mismo `RackId` (tambien si solo su `Id` es legible) → fallo; payload sin `Id` atribuible → no bloquea | §9 |
| CR-04 | ID19 no puede suponer el significado de un token coercionado | ID19: `Canonical` y `Canonicalizable` aceptan (sin reescribir la fuente); `Coerced`, `Invalid` y variantes huerfanas fallan cerrado; `RACKEDITAR` sin cambio | §7.3 |
| CR-05 | PR-1 | Se mantiene | §17.4 |
| CR-06 | PR-2 | Se mantiene aislado, **solo en el Plugin**, sin tocar el ensamblador que es linea base de I-52 | §17.4 |
| CR-07 | PR-3 / H-13 fuera | Retirado; H-13 registrado fuera de I-55 | §22.5 |
| CR-08 | ADR-0042 complementa | Complementa y enmienda solo: la regla de vista adicional (A/B) y dos precondiciones nuevas de Insertar (gate y D-15); el resto de ADR-0010 sigue vigente | ADR-0042, D-12 |
| CR-09 | Conservar lo aceptado de V1 | La lista de CR-09 queda intacta | §2-§17 |
| CR-10 | Sin cambios de producto silenciosos | OD-1..OD-5 y OD-8 con su recomendacion de V1; la consecuencia de OD-2 en misma clase se lleva a M-01 | §22.2 |
| CR-11 | Entregables | V2, mapa V2, paquetes V2, ADR-0042 propuesto, registro G2B; V1 intacta | — |
| CR-12 | Revision adversarial previa | Doce puntos revisados; 37 hallazgos en tres pasadas, verificados y corregidos | §23 |

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
| Materializacion | Definicion confirmada antes del jig (`P/Systems/Shared/SystemBlockWriter.cs:18-40`); el jig solo fija `Position` (`P/Drawing/BlockPlacement.cs:240-244`); limpieza al cancelar (`:37-42`, `:64-76`) | Limpieza ante excepcion (D-10); rotacion calculada en ID19 |
| Vista enlazada | Redibujo con fallos ignorados (`P/RackSelectivoCommands.cs:178-183`; `P/RackCantileverCommands.cs:328-332`) | Gate → preparar → redibujar → colocar; D-15 |
| Hermanas | Propiedades copiadas sin comparar | Gate acotado al `RackId` |
| Multi-rack | No existe | ID19 |

## 4. Arquitectura propuesta (ALT-B, D-01)

### 4.1 Responsabilidades

```text
Estado logico del rack   authored serializado UNA vez + identidad + sistema resuelto UNA vez
        ▼
RackViewSpec             RackId + Name + Kind + RackViewAddress
        ▼
Gates de hermanas        propiedades comunes del RackId + authored equivalente (§9)                ← rack existente
        ▼
Preparacion (pura)       soporte → codec → plan desde el SISTEMA RESUELTO (X-2) → sobre → nombre
        ▼
Colocacion (Plugin)      importar → definicion + sobre → jig o colocacion calculada → confirmar o limpiar

Lote (ID18)              RackViewBatchPlan (puro)
Colocacion de grupo      RackGroupPlacementPlan (puro): marcos, CommonTransform2D, politica, anclas (§12)
```

| Capa | Hace | No hace |
|---|---|---|
| Preparacion | Soporte, disponibilidad, plan desde el sistema resuelto, sobre, nombre | Colocar, abrir transacciones, inventar identidad, reconstruir authored, conocer AutoCAD, hacer I/O, re-resolver lo que el editor entrego |
| Colocacion | Importar, crear, escribir, colocar, limpiar | Decidir soporte, variante, identidad, autoridad o colocaciones de grupo |
| Colocacion de grupo | Application: marcos, transformacion comun, anclas. Plugin: seleccion, puntos, lectura, materializacion | Pedir un punto por rack; calcular transformaciones por rack |
| Builders | Unica autoridad geometrica | Duplicarse |

**Una autoridad por responsabilidad (D-14):** X-1..X-8 con I-52 (§17.2).

### 4.2 Tipos (nombres no contractuales; responsabilidades si)

Namespace neutral `RackCad.Application.Views`.

| Tipo | Responsabilidad |
|---|---|
| `RackViewKind` | Unica taxonomia `Frontal/Lateral/Planta` (renombra `DimensionViewKind`) |
| `RackViewVariant` | `Whole`, `Fondo`, `Post`, `FlowEnd`, `PushBackCut`, `Station`; nunca un indice de UI |
| `RackViewAddress` | `(RackViewKind, RackViewVariant)` |
| `RackViewEnvelopeCodec` | `Encode`; `Decode → (address, disposicion)`; la politica la aplica cada consumidor (§7.3) |
| `RackViewSupport` | Soporte, disponibilidad (variantes huerfanas), variante canonica, pares y modos representables |
| `RackViewFrame` | Por `(sistema, tipo, variante)` sobre el sistema resuelto: ejes locales ↔ ejes fisicos R/D/H con signo, origen fisico local y **extension fisica** sobre R y D (§12.3) |
| `RackEnvelopeIdProbe` | Lee de forma tolerante el `Id` de un payload que el store no interpreta (§9.2) |
| `RackSiblingCustomPropertiesGate` / `RackSiblingAuthoredGate` | Gates acotados al `RackId`, en archivos separados (§9) |
| `IRackViewPreparer` y compania | Preparacion por sistema sobre `KindDispatch<T>` (`A/Persistence/KindDispatch.cs:19`) |
| `RackViewBatchPlan` | ID18 |
| `SourceGroupFrame`, `TargetGroupFrame` | Marco 2D (origen, orientacion) fuente y destino de **la seleccion proyectada** |
| `CommonTransform2D` | **Una** transformacion rigida por seleccion proyectada, sobre `Transform2D` (`A/Geometry/Transform2D.cs:20-116`), con `Determinant = +1` y `ScaleFactor = 1` |
| `RackGroupAnchorPolicy` | `Rigid` u `Orthographic`: anclas fuente, angulo comun y orientacion destino |
| `RackGroupPlacementPlan` | Grupos, clasificacion, resolucion, gates, marcos, transformacion, colocaciones, avisos |

### 4.3 Preparacion por sistema (builders vigentes)

| Sistema | Direcciones | Plan (desde el sistema resuelto) | Payload |
|---|---|---|---|
| Selectivo | Frontal `Fondo(k)`, Lateral `Post(p)`, Planta | `SelectiveFrontalBuilder.BuildPlan(SelectiveDepthLayout.FondoSystemView(system, k), catalog)`; corte de `SelectiveLateralBuilder.Cortes` + `LateralHeaderLayoutBuilder.Build` + `HeaderInstanceGrouper.Group`; `SelectivePlantaBuilder.BuildPlan(system, catalog)` | authored verbatim |
| Dinamico | Lateral `Post(p)`, Frontal `FlowEnd(e)`, Planta | `DynamicSystemLateralBuilder.Build(system, catalog, postIndex)`; `DynamicSystemFrontalBuilder.BuildPlan(system, catalog, end)`; `DynamicSystemPlantaBuilder.BuildPlan(system, catalog)` | `RackProject.ForDynamic(design)` con metadata interior |
| Push Back | Lateral `Post(p)`, Frontal `PushBackCut(e, s)`, Planta | `PushBackSystemLateralBuilder.Build(system, catalog, postIndex)`; `PushBackSystemFrontalBuilder.BuildPlan(system, catalog, end, side)`; `PushBackSystemPlantaBuilder.BuildPlan(system, catalog)` | `RackProject.ForPushBack(design)` con metadata interior |
| Cantilever | Frontal, Lateral `Station(s)`, Planta | `CantileverViewPlanBuilder.Build(line, kind, factory, station, plantaVisibility)` (PR-2) | `RackProject.ForCantilever(design)`; `Id` interior sin tocar |
| Cabecera | Lateral, Planta | `HeaderInstanceGrouper.Group(LateralHeaderLayoutBuilder.Build(...).Instances, nombre)`; `PlantaHeaderLayoutBuilder.Build(config, catalog)` | `RackProject.ForSelective(configuration)` con metadata interior |
| Cama | Lateral | `FlowBedLateralBuilder.Build(config, catalog)` | `FlowBedDocument` con version y extension data |

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
- **RID-4.** `RACKEDITAR` cura un `Id` en blanco como hoy; ID19 falla cerrado ante un sobre sin `Id`.
- **RID-5.** Cancelar antes del primer placement no deja rastro.

## 7. `RackViewKind`, variante y codec (D-02, D-03)

### 7.1 Una sola taxonomia (A-4)

Renombre de `DimensionViewKind` (`A/Systems/Shared/DimensionViewPolicy.cs:11-16`) a `A/Views/RackViewKind.cs` sin cambio de miembros ni
de ADR-0035; tokens del sobre (`A/Persistence/RackEmbedDocument.cs:28-30`) como persistencia; `CantileverViewKind`
(`A/Systems/Cantilever/CantileverViewPlanBuilder.cs:12-37`) con un mapeo (OM-2). Seis archivos de produccion y seis de pruebas nombran el tipo.

### 7.2 Variante tipada (A-1)

Cerrada, igualdad por valor, **nunca** un indice de UI; base 0 en la variante y base 1 en mensajes (`P/RackSelectivoCommands.cs:476-491`, `:546-561`).

### 7.3 Codec unico con disposicion y politica por consumidor (X-2; CR-04)

| Disposicion | Significado | `RACKEDITAR` | I-52 (V11 §3.7) | **ID19** |
|---|---|---|---|---|
| `Canonical` | lo que produccion escribe | lee (sin cambio) | acepta | **ACEPTA** |
| `Canonicalizable` | no canonico con un unico significado: legado documentado, vista de significado unico o token en otra capitalizacion (produccion compara ignorando mayusculas) | lee y reescribe (sin cambio) | acepta y canoniza | **ACEPTA** y canoniza semanticamente, **sin reescribir** la fuente |
| `Coerced` | produccion lo coerciona sin legado documentado | lee coercionado (sin cambio) | falla cerrado | **FALLA CERRADO** |
| `Invalid` | produccion lo rechaza | aborta (sin cambio) | falla cerrado | **FALLA CERRADO** |

**Variante huerfana** (fondo, corte o estacion que ya no existe en el sistema resuelto): `RACKEDITAR` la borra como fantasma
(`P/RackSelectivoCommands.cs:169-173`, `:194-198`; `P/RackCantileverCommands.cs:304-310`); I-52 falla cerrado; **ID19 falla cerrado** con
remedio `RACKEDITAR`, porque su ancla fisica no es calculable.

| Kind | Entrada | Disposicion indicativa | Evidencia |
|---|---|---|---|
| `selective` | `frontal`, `Section` en `[0, fondos)` | `Canonical` | `P/RackSelectivoCommands.cs:168` |
| `selective` | `frontal` con `Section = −1`; `View` vacio con `Section = −1` | `Canonicalizable` (fondo 0) | `:155`, `:168` |
| `selective` | `frontal` con `Section < −1`; `View` vacio con `Section ≥ 0`; `View` no vacio desconocido | `Coerced` | `:126`, `:168` |
| `selective` | `planta` con cualquier `Section` | `Canonical` si −1; si no, `Canonicalizable` | reescribe −1 (`P/RackSelectivoCommands.cs:216`) |
| `selective` | `lateral` con `Section` = `PostIndex` existente | `Canonical` | `:194` |
| `dynamic` | `frontal` con `Section ∈ {0, 1}`; `planta` con −1; `lateral` con corte existente | `Canonical` | `P/RackDinamicoCommands.cs:209-239` |
| `dynamic` | `planta` con `Section ≠ −1`; `View` vacio con `Section = −1` (legado sin `View` ni `Section`) | `Canonicalizable` | `:211`, `:236-239` |
| `dynamic` | `frontal` con `Section ∉ {0, 1}`; `lateral` con `Section < 0`; `View` vacio con `Section ≥ 0` (nunca escrito); `View` no vacio desconocido | `Coerced` | `:222-224`, `:236-239` |
| `pushback` | `frontal` `0..1`; `frontal` `2..3` en rack compuesto; `planta` −1; `lateral` con corte existente | `Canonical` | `P/RackPushBackCommands.cs:424-450` |
| `pushback` | `frontal` `2..3` en rack de un solo sentido | `Coerced`: el lado se ignora (`A/Systems/PushBack/PushBackSystemFrontalBuilder.cs:43-45`) | — |
| `pushback` | descriptor que produccion rechaza | `Invalid` | `:427-450` |
| `cantilever` | `frontal`/`planta` −1; `lateral` con estacion existente | `Canonical` | `P/RackCantileverCommands.cs:501-540` |
| `cantilever` | descriptor que produccion rechaza | `Invalid` | idem |
| `cabecera` | `lateral` o `planta` con −1 | `Canonical` | `P/RackCabeceraCommands.cs:197` |
| `cabecera` | `lateral` o `planta` con `Section ≠ −1`; `View` vacio | `Canonicalizable` | `:239-240` |
| `cabecera` | `View` no vacio desconocido | `Coerced` | `:239-240` |
| `cama` | `View` nulo | `Canonical` | `P/RackCamaCommands.cs:197-198` |
| `cama` | cualquier otro `View` | `Coerced` (nunca escrito) | — |

Diagnostico de ID19: «La vista `<bloque>` del rack `<nombre>` tiene `View = '<valor>'` y `Section = <valor>`, que RackCad no reconoce sin
suponer (o que ya no existe en el diseno): no se proyecto nada. Abrela con RACKEDITAR para normalizarla.» **Otros lectores** no migrados,
caracterizados en G3: `P/ProjectVariableMutationExecutor.cs:180-218`; `A/Persistence/RackListBuilder.cs:117-118`; `P/RackBlockFinder.cs:23-24`;
`P/RackLayoutCommands.cs:71`. La tabla exacta la fija G3 en **una** caracterizacion compartida con la CT-04 de I-52.

## 8. Authored, effective y metadatos de una vista nueva

| Campo | Rack nuevo | Hermana por editor | ID19 |
|---|---|---|---|
| `RackId` | acunado una vez | del sobre elegido; curado si estaba en blanco | del grupo `RackId` |
| `Name`, `Kind` | del editor / constante | del editor / igual | del grupo; kind con `TryResolve` sensible a mayusculas (`P/RackMenuCommands.cs:141`) |
| `View`/`Section` | codec | codec | codec (variante segun M-01) |
| authored | `SelectivePalletDesignDocument.From(design, id, name)` o diseno del editor | reconciliado y serializado una vez | verbatim del sobre fuente, con authored unico (§9.3) |
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
- **Mismo conjunto para D-15.** D-15 cuenta los redibujos fallidos de los miembros del gate; las definiciones dependientes de xref que la
  busqueda vigente tambien redibuja siguen como hoy.

### 9.3 Authored

- **Editor:** el flujo vigente unifica el authored de las hermanas y D-15 impide insertar si un redibujo falla.
- **ID19:** `RackSiblingAuthoredGate` sobre **todas** las hermanas del barrido: Selectivo `SelectiveAuthoredAuthority.Resolve`
  (`A/ProjectVariables/SelectiveAuthoredAuthority.cs:128-157`); demas kinds, comparador de X-3 (excluye la metadata interior de
  `P/RackCommandSupport.cs:40-67`). Divergente o ilegible → fallo con remedio `RACKEDITAR`.

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

Prompts legacy de variante conservados (OM-4). Agregar vistas a un rack existente pasa por §9. Racks parciales (OQ-7): celda de
`RACKLAYOUT` de solo planta (`P/RackLayoutCommands.cs:231-244`) recibe hermanas con el gate.

## 11. ID18 — MULTI-VIEW QUEUE / BATCH

- **Editor.** Dialogo del arquetipo C (ADR-0029) con presentador **fuera del archivo de la ventana** (las guardas de modales cuentan
  `ShowDialog(` en el archivo de ventana: `TU/DynamicHeaderBatchSeamGuardTests.cs:26-31`, `TU/SelectiveHeaderBatchSeamTests.cs:177-181`);
  orden F → L → P (OD-5).
- **Peticion.** `Views` en la peticion; `RequestInsertViews`; rutas que perderian `Views`: `U/Editor/EditorModules.cs:53-56`, `:90-95`;
  `P/RackSelectivoCommands.cs:34-38`; `P/RackCabeceraCommands.cs:37-41`, `:173`. Push Back ya devuelve la peticion de la ventana
  (`U/Editor/EditorModules.cs:98-100`).
- **Transaccion (D-06; A-3): C** — PREPARE completo + commit por placement (`P/Drawing/BlockPlacement.cs:174-201`).

```text
SIBLING_GATE ─(falla)→ SIBLING_GATE_FAILED   "No se inserto ninguna vista: <motivo y remedio>."   (rack existente)
PREPARE      ─(falla)→ ABORTED_BEFORE_WRITE
REDRAW       ─(falla un miembro del gate)→ REDRAW_FAILED   (D-15)
READY → PLACING(i) → COMMITTED(i) → … → COMPLETED
           ├─ Esc → CANCELLED(i) → COMPLETED_PARTIALLY
           └─ excepcion → FAILED(i) → COMPLETED_PARTIALLY o FAILED_BEFORE_ANY
```

Esc detiene la cola (OD-4). `PlaceAndReport` gana parametro de prompt (`P/Drawing/BlockPlacement.cs:26`, `:214`). `Regen`: el lote no
anade regeneraciones al flujo legacy (Cantilever: una tras el redibujo, `P/RackCantileverCommands.cs:355-358`; una al final en lugar de
una por vista, `:169`). En rack existente: preflights → gate → PREPARE ALL → redibujo → `REDRAW_FAILED` o `Regen` → cola (OM-3).

## 12. ID19 — Group Placement & Projection (CR-01)

### 12.1 Flujo

```text
ACQUIRE     seleccion en Model Space + clase pedida (+ modo si M-01 lo expone)
SNAPSHOT    una lectura: referencias (transformacion completa, espacio, MINSERT, escala, normal), definiciones (dependencia de xref,
            texto del payload), sobres
CLASSIFY    nucleo neutral (X-1) + codec con politica de ID19 (§7.3) + D-08f
GROUP       grupos RackId; DEDUPE conservando todas las referencias (D-08b)
RESOLVE     cada RackId UNA vez (camino sin editor, D-17); registro de variables UNA vez              ← antes de validar
VALIDATE    tipos fuente, pares, modos y variantes segun M-01; variante fuente existente; requisitos de la politica (§12.4-§12.5:
            paralelismo, sentidos y superposicion segun OD-7.b) sobre TODA la seleccion; salida no bloqueada (D-17); miembros
            soportados (OD-3)
GATES       propiedades y authored por grupo RackId (§9)
PLANS       planes por vista desde el sistema resuelto (puros), descriptores (§12.3), anclas fuente, direcciones y
            orientaciones: todo lo que no depende de los puntos
PICK        punto base y destino (UCS → WCS)                                         ← tras validar y planificar; antes de importar
IMPORT      importacion de la union de bloques y re-verificacion (D-13)
PLACE       marcos y UNA CommonTransform2D de la ejecucion (§12.2); anclas destino y colocaciones
MATERIALIZE UNA transaccion: por grupo RackId, UNA definicion nueva (D-08b) y una referencia por referencia fuente; commit
```

Como en I-52 V11 §9.1, la resolucion ocurre antes de pedir la linea o los puntos. Todo fallo de clasificacion, resolucion, validacion,
gates o planes ocurre **antes** de los puntos; tras ellos solo se importa, se re-verifica, se calcula la colocacion y se materializa.

### 12.2 Nivel 1 — Group Placement (foundation general)

```text
Seleccion proyectada S = todas las referencias aceptadas en UNA ejecucion; grupos RackId G ⊆ S

Por referencia r ∈ S
  M_r        transformacion completa (Position, Rotation, ScaleFactors, Normal, Origin de la definicion)
  L_r        parte lineal 2D de M_r (rotacion · escala)
  φ_r        orientacion de la referencia = angulo de L_r·(1,0)  (Transform2D.RotationAngle, A/Geometry/Transform2D.cs:111-115;
             una escala (−1,−1) cuenta como giro de π)
  a_s(r)     ancla semantica en coordenadas locales de la vista fuente (§12.3; por defecto el origen fisico; la politica
             ortografica la lleva al extremo del tramo, §12.5)
  a_t(r)     la misma ancla semantica en coordenadas locales de la vista destino
  A_r        M_r · a_s(r)                                    ancla fuente en WCS (3D)

Marcos: UNO de cada por ejecucion
  SourceGroupFrame  F_s = (B, φ_s)    B = UCS→WCS(punto base)
  TargetGroupFrame  F_t = (T, φ_t)    T = UCS→WCS(punto destino)
  φ_s, φ_t: orientaciones de los marcos (entradas; M-01 / OD-6)

CommonTransform2D: UNA por ejecucion
  T_common = Transform2D.Translation(−B).Then(Transform2D.Rotation(α)).Then(Transform2D.Translation(T))
             (Then se lee de izquierda a derecha: A/Geometry/Transform2D.cs:85-93)
  α        = angulo comun de la ejecucion (lo fija la politica)
  rigidez  Determinant = +1 (:77) y ScaleFactor = 1 (:83); sin cizalla

Para toda r ∈ S
  sourceAnchor_r  = politica(A_r)                         (plano XY)
  targetAnchor_r  = T_common(sourceAnchor_r)
  Z_r             = politica
  ρ_r             = orientacion de la referencia nueva (politica; nunca una transformacion de layout por rack)
  referencia nueva: definicion nueva con Origin = 0 (P/Drawing/LateralHeaderDrawer.cs:243; P/Drawing/Cantilever/CantileverViewMaterializer.cs:47, :110),
                    Rotation = ρ_r, escala 1, normal Z, Position = (targetAnchor_r − R(ρ_r)·a_t(r), Z_r)
                    (AutoCAD: WCS = Position + Rotation · Escala · (p − Origin))

INV-GRP-1  para TODO par r, q ∈ S, de cualquier grupo RackId:
           targetAnchor_r − targetAnchor_q = R(α)·(sourceAnchor_r − sourceAnchor_q)
INV-GRP-2  Rigid: ρ_r − φ_r = α para toda r ∈ S · Orthographic: ρ_r depende solo de φ_t y de la vista destino, nunca de la
           posicion ni de la rotacion de r
```

La relacion **entre** racks la fija solo `T_common`; los datos por rack (descriptor, orientacion, extension) solo situan el ancla dentro de
su propio bloque y orientan ese bloque.

### 12.3 Descriptores de marco (`RackViewFrame`)

**Marco fisico:** R = corrida, D = profundidad, H = altura. Hipotesis derivadas del codigo; G3 las fija con caracterizacion sobre la salida
de los builders; un par inconsistente queda fuera de ID19 hasta una Proposal nueva, nunca se ajusta el descriptor.

| Familia | Vista | Eje local +X | Eje local +Y | Origen fisico local | Evidencia |
|---|---|---|---|---|---|
| **Rack** (Selectivo, Dinamico, Push Back, Cabecera) | Planta | +D | +R | D: cara exterior delantera (Dinamico: salida; Push Back: extremo bajo o cara del lado A); R: eje del poste 0 | `A/Systems/Selective/SelectivePlantaBuilder.cs:16-19`; `A/Systems/Dynamic/DynamicSystemPlantaBuilder.cs:15-17`; `A/RackFrames/PlantaHeaderLayoutBuilder.cs:95-103` |
| Rack | Frontal | +R | +H | eje del poste 0 de la cuadricula de la vista; base del poste | `A/Systems/Selective/SelectiveFrontalBuilder.cs:50` |
| Rack | Lateral | +D | +H | cara delantera del primer fondo que alcanza el poste (Selectivo) o X = 0 del sistema; base del poste | `A/Systems/Selective/SelectiveLateralBuilder.cs:82-97`; `A/RackFrames/LateralHeaderLayoutBuilder.cs:56-59` |
| **Cantilever** | Planta | −R | +D | columna de la estacion 0; cara de conexion | `A/Systems/Cantilever/CantileverViewPlanBuilder.cs:209-219`; `A/Geometry/Spatial3D.cs:186-197` |
| Cantilever | Frontal | −R | +H | columna de la estacion 0; suelo | idem |
| Cantilever | Lateral | −D | +H | cara de conexion; suelo | idem |

Desplazamientos calculables: `anchorOffset` de un corte de esquina (`A/Systems/Selective/SelectiveLateralBuilder.cs:97`); cuadricula de la
frontal de un fondo frente a la maestra (`A/Systems/Selective/SelectiveFrontalBuilder.cs:50`; `A/Systems/Selective/SelectiveDepthLayout.cs:52-55`).
**Extension fisica** `L_R`, `L_D` desde el sistema resuelto. La misma responsabilidad —origen y tramo del eje de una vista— aparece en I-52
(«tramo del eje de la vista (para c)»): X-8.

### 12.4 Nivel 2a — Politica `Rigid`

```text
sourceAnchor_r = A_r (XY)                  α = φ_t − φ_s (M-01 / OD-6.c)
Z_r            = T.Z + (A_r.Z − B.Z)       ρ_r = φ_r + α
requisitos     escala de valor absoluto 1, uniforme y no reflejada (D-08f); un solo tipo fuente salvo M-01 / OD-7.c
```

- **Misma clase:** el layout de anclas se reproduce exacto, rotado `α` y trasladado. La **variante destino** la decide M-01 / OD-2.b
  (conservar la de la fuente o la canonica): con la de la fuente, `a_t = a_s` y la vista nueva es un duplicado enlazado legal (OQ-4).
- **Frontal ↔ Lateral y cualquier par:** sustitucion en sitio; entre planta y elevaciones apila vistas.
- **Admite** orientaciones distintas entre racks (INV-GRP-2) y familias mezcladas (cada vista con su descriptor).

### 12.5 Nivel 2b — Politica `Orthographic` (encima de la foundation)

```text
K       eje fisico conservado del par (tabla)
k_s(r)  vector local de +K en la vista fuente; e_K(r) = normalizar(L_r · k_s(r))            direccion WCS de +K en la fuente
ρ_r     = φ_t + δ_t                          δ_t = 0 en elevaciones; plantas: 0 (OD-6.d A) o el giro que lleva K al X del marco (B)
k_t(r)  vector local de +K en la vista destino; w(r) = R(ρ_r) · k_t(r)                    direccion WCS de +K en el destino
w       = R(φ_t) · c_t(K)                     c_t: (1,0) en elevaciones; en plantas, el eje local de K en la familia rack
                                               (R → (0,1), D → (1,0)) con OD-6.d A, o (1,0) con B
σ_t(r)  = signo(w(r) · w): w(r) debe ser paralela a w; perpendicular → fallo (OM-12)
e       recta comun de las e_K(r): todas paralelas (tolerancia), si no → fallo (OM-13).
        SENTIDO de e: lo elige el destino, el que maximiza #{r : σ_s(r) = σ_t(r)}, para que +K conserve su sentido respecto
        de cada vista; en empate, el de angulo en [−45°, 135°). No depende del orden de la seleccion ni salta con la
        rotacion (una planta a 0 y a 2π − ε dan el mismo resultado); solo un empate exacto, con tantas vistas en un sentido
        como en el otro, depende del limite del intervalo
σ_s(r)  = signo(e_K(r) · e)
ancla   K*_s(r) = 0 si σ_s(r) = +1, si no L_K(r); K*_t(r) = 0 si σ_t(r) = +1, si no L_K(r)
        (extremo del tramo en K con coordenada minima en la direccion comun; con sentidos iguales es el origen fisico)
        A_r(κ) = M_r · (punto local de la vista fuente con K = κ y las demas coordenadas fisicas en el origen)
sourceAnchor_r = B + ((A_r(K*_s) − B) · e) · e                                            proyeccion sobre la recta comun
a_t(r)  = punto local de la vista destino en K = K*_t(r)
α       = angulo(w) − angulo(e)
Z_r     = T.Z
invariante   targetAnchor_r − targetAnchor_q = ((A_r(K*_s) − A_q(K*_s)) · e) · w       (caso de INV-GRP-1)
orden        con σ_s = σ_t en toda r, un rack mas adelante en +K en la fuente queda mas adelante en +K en el destino
             (incluida la ida y vuelta planta → frontal → planta)
superposicion A_r · e_⊥ con mas de un valor → las vistas de filas distintas se superponen (OD-7.b); no depende de los
             puntos: se evalua en VALIDATE
```

Ejemplo: dos plantas de rack sin girar, `P1` en (0, 0) y `P2` en (0, 100), `L_R = 90`, proyectadas a Frontal con `B = (0, 0)` y
`T = (500, 0)`. `w = (1, 0)`, `σ_t = +1`; el destino elige `e = (0, 1)`, asi que `σ_s = +1`, `α = −90°` y las frontales quedan en
X ∈ [500, 590] y [600, 690]: el orden y la separacion a lo largo de la corrida se conservan.

| Fuente | Destino | K | Eje comun en el destino | Eje descartado |
|---|---|---|---|---|
| Planta | Frontal | R | H: base comun | D (aviso) |
| Planta | Lateral | D | H: base comun | R (aviso) |
| Frontal | Planta | R | D: linea comun | H |
| Lateral | Planta | D | R: linea comun | H |
| Frontal o Lateral | Lateral o Frontal | — (sin eje horizontal comun) | la politica ortografica no aplica | `Rigid` lo representa |

**Familias mezcladas** (Cantilever junto a sistemas de rack): con sentidos de +K **paralelos** (opuestos o iguales) se representan por el
ancla alineada al tramo; ocurre en **planta → elevacion** y, en la fuente, entre **elevaciones**. En **→ planta** con OD-6.d A los ejes de
destino son **perpendiculares** (rack `(0,1)`, Cantilever `(−1,0)`) y fallan salvo OD-6.d B. Un rack girado 180° respecto de la mayoria
tiene `σ_s ≠ σ_t`: su vista ocupa el mismo intervalo del eje comun, anclada por el extremo opuesto del tramo. Orientaciones no paralelas
fallan.

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
| Familias mezcladas | `Rigid`; `Orthographic` con sentidos paralelos | M-01 (OD-7.d) |
| Varias definiciones del mismo `RackId` en la seleccion (duplicados legales, OQ-4) | `Rigid` (una definicion nueva por definicion fuente) | M-01 (OD-7.c; con A se rechazan) |

### 12.7 M-01 — decision material (Owner via Coordinador; revision tecnica del Arquitecto)

La foundation no cambia con ninguna eleccion; cambian validacion, mensajes, pruebas de politica y validacion del Owner.
**Paquete recomendado para V1: todas las opciones A.** Las partes de OD-6 empiezan en `.b` a proposito: la exigencia de V1 de que
todas las fuentes compartan orientacion desaparece, porque la foundation ya representa orientaciones fuente distintas (INV-GRP-2); la
orientacion de las vistas nuevas se reparte entre OD-6.b, OD-6.c y OD-6.d (la opcion B de OD-6 en V1 es OD-6.d B).

| Parte | Decision | Opcion A | Opcion B | Compromiso | Recomendada | Consecuencia |
|---|---|---|---|---|---|---|
| OD-7.a | Modo por par | misma clase → `Rigid`; planta ↔ elevaciones → `Orthographic`; frontal ↔ lateral no expuesto | todo `Rigid` | A da elevaciones utiles de un layout en planta; B uniforme pero apila elevaciones | **A** | B deja la proyeccion para despues |
| OD-7.b | Filas superpuestas en `Orthographic` | aviso | fallo cerrado | A fiel; B obliga a seleccionar filas; separar filas no es opcion (romperia la transformacion comun, CD-04) | **A** | — |
| OD-7.c | Tipos fuente mezclados y varias definiciones de un `RackId` | fallo cerrado | `Rigid` (sustitucion por referencia) | A predecible; B mas permisivo pero con layouts dificiles de leer | **A** en V1 | — |
| OD-7.d | Familias mezcladas en `Orthographic` | fallo cerrado | anclas alineadas al tramo donde los sentidos sean paralelos | A simple; B mezcla Cantilever y racks en una elevacion | **A** en V1 | B sin cambio de foundation |
| OD-6.b | Orientacion del marco destino `φ_t` | 0 (X universal), como toda insercion vigente (`P/Drawing/BlockPlacement.cs:240-244`) | X del SCP actual | A predecible; B orienta con el SCP | **A** en V1 | — |
| OD-6.c | `α` en `Rigid` (y por tanto `φ_s`) | 0: traslacion, como `COPY` (`φ_s = φ_t`) | pedir un angulo | A un gesto menos | **A** en V1 | — |
| OD-6.d | Orientacion de plantas proyectadas desde elevaciones | natural (corrida de las plantas de rack en Y) | girar para que K siga el X del marco | A coherente con las inserciones vigentes; B continuidad visual y permite familias mezcladas | **A** en V1 | — |
| OD-2.b | Variante destino en misma clase | conservar la de la fuente | canonica (OD-2) | A reproduce el layout de lo que se ve; B normaliza a una vista por sistema | **A** | La canonica de OD-2 queda para clase distinta |

### 12.8 Seleccion, grupos y clasificacion (D-08)

- **D-08a.** Nucleo neutral de seleccion de I-51 que I-52 V11 §5.1 propone extraer (`A/Persistence/RackDuplicationPlan.cs:225-583`); hechos
  neutrales en el nucleo, politicas por llamador.
- **D-08b.** En un grupo `RackId`, referencias de **una** definicion (celdas enlazadas, `P/RackLayoutCommands.cs:29-30`, `:226-257`) → una
  definicion nueva y una referencia por referencia, cada una con su ancla; varias definiciones del mismo `RackId` → segun OD-7.c.
- **D-08c.** Tipos fuente: segun OD-7.c.
- **D-08d.** Miembro que no soporta el par, el modo o la variante → fallo que lista los miembros (OD-3).
- **D-08e.** Proyectar una clase que el rack ya tiene es legal (OQ-4).
- **D-08f.** Fallo cerrado: `MInsertBlock`; referencia dinamica, anonima o anotativa con datos (el barrido omite definiciones anonimas,
  `P/RackBlockFinder.cs:66`); escala de valor absoluto distinta de 1, no uniforme o reflejada (una celda de `RACKLAYOUT` hereda la escala de
  su semilla, `P/RackLayoutCommands.cs:250-254`); normal distinta de Z; SCP cuyo plano XY no es paralelo al universal. `Origin ≠ 0` no falla. Aviso e ignorar
  como I-51: no-bloque, fuera de Model Space, sin datos.
- **D-08g.** Kind con `KindHandlerDispatch.TryResolve` (`P/RackMenuCommands.cs:141`), no con la variante de `RACKLAYOUT` (`P/RackLayoutCommands.cs:64`).
- **D-08h.** Codec: `Coerced`, `Invalid` y variantes huerfanas fallan (§7.3).

### 12.9 Salidas bloqueadas, presentacion y materializacion

- **D-17.** Resolucion sin editor por el camino del BOM (`P/KindHandlers/SelectiveKindHandler.cs:58-68`; `P/KindHandlers/DynamicKindHandler.cs:40-42`);
  diagnosticos bloqueantes (`A/Bom/RackBomOutputGate.cs:46`; `P/RackInventarioCommands.BomTotal.cs:174-188`); paridad con el editor
  caracterizada en G3 (`U/Systems/Dynamic/RackDynamicSystemWindow.xaml.cs:506-537`).
- **D-11 / OD-8.** Presentacion de creacion vigente.
- **D-13.** Union de nombres de bloque (`P/Drawing/BlockLibraryImporter.cs:37`) y re-verificacion antes de escribir.
- **Una** transaccion (`LateralHeaderDrawer.CreateSystemBlock(db, tr, plan, name)`, `P/Drawing/LateralHeaderDrawer.cs:28-29`, o
  `CantileverViewMaterializer`); sin `Regen` ni purga.

## 13. Invariantes

| ID | Invariante | Prueba futura |
|---|---|---|
| INV-BOM-1..3 | Vistas hermanas y lotes no anaden racks; ID19 conserva racks y copias (definicion nueva con k ≤ max referencias) | `RackViewCountInvariantTests` |
| INV-LIST-1..3 | `ViewCount` puede cambiar; copias sin cambio salvo 0 → 1; filas por `Id` | idem |
| INV-AUTH-1 | I-55 nunca crea authored divergente | `RackViewAuthoredEquivalenceTests` |
| **INV-AUTH-2** | Una hermana nueva nace con la coleccion canonica comun de su `RackId` (incluido el sobre fuente), o no nace | `RackSiblingCustomPropertiesGateTests` |
| **INV-GRP-1** | Una `CommonTransform2D` rigida por seleccion proyectada; diferencias de anclas destino = rotacion comun de diferencias de anclas fuente, para todo par de la seleccion | `CommonTransform2DTests`, `RackGroupPlacementPlanTests` |
| **INV-GRP-2** | Orientaciones destino: `Rigid` `ρ_r − φ_r = α`; `Orthographic` `ρ_r` depende solo de `φ_t` y de la vista destino, nunca de la posicion ni la rotacion de `r` | `RackRigidPlacementPolicyTests`, `RackOrthographicPlacementPolicyTests` |

## 14. Cancelacion y errores (diseno)

| Operacion | Etapa | Evento | Que queda |
|---|---|---|---|
| ID17 | editor / variante / jig | cerrar o Esc | nada; definicion borrada al cancelar (vigente) |
| ID17 | tras crear la definicion | excepcion | definicion borrada en ambas primitivas (D-10; hoy queda: `P/Drawing/BlockPlacement.cs:49-52`; `P/RackCantileverCommands.cs:151-157`, `:174-177`) |
| ID17/ID18 | materializacion | faltan bloques | se coloca y se reporta (vigente) |
| ID17 despues / ID18 existente | gate de propiedades | tipos distintos, hermana ilegible (tambien por `Id` legible en un payload no interpretable), divergentes | **nada**, ni redibujo; motivo y remedio condicionado |
| ID17/ID18 | barrido | F-14a (UTF-16 invalido) | falla el barrido; nada modificado |
| ID18 | placements / preparacion / redibujo | Esc; fallo; fallo de un miembro | anteriores o nada; nada; ninguna vista nueva (D-15) |
| ID19 | seleccion | no bloque / fuera de Model Space / sin datos | aviso; se ignora |
| ID19 | clasificacion | sobre ilegible del grupo, kind desconocido, `Coerced`, `Invalid`, MINSERT, espejo, escala ≠ 1, normal o SCP | fallo, nada escrito |
| ID19 | resolucion / validacion | resolucion fallida; variante huerfana; par, modo o tipos no expuestos; orientaciones no paralelas; sentidos perpendiculares; filas superpuestas con OD-7.b = B; `RackId` en blanco; miembro no soportado; salida bloqueada | fallo, nada escrito, sin pedir puntos |
| ID19 | validacion | filas superpuestas con OD-7.b = A | aviso antes de pedir puntos; se puede cancelar |
| ID19 | gates | authored o propiedades del grupo | fallo con remedio; nada escrito, sin pedir puntos |
| ID19 | planes | fallo de un builder o del paso «sistema resuelto → plan» | fallo, nada escrito, sin pedir puntos |
| ID19 | puntos | Esc | nada (aun no se importo) |
| ID19 | re-verificacion / materializacion | faltan bloques / excepcion | fallo con lista (bloques importados pueden quedar, OM-5) / rollback |

## 15. Transacciones

| Flujo | PREPARE | MUTATE | `Regen` |
|---|---|---|---|
| ID17 vista unica | fuera de la transaccion | definicion + jig o limpieza | como hoy |
| ID18 rack nuevo | todo antes de escribir | por vista | ninguno; Cantilever uno al final |
| ID18 rack existente | gate + todo antes del redibujo | redibujo; si todo bien, cola | los del redibujo; Cantilever uno mas al final |
| ID19 | resolucion, validacion, gates y planes antes de los puntos; importacion y re-verificacion despues | una transaccion | ninguno |

## 16. Recursos

| Recurso | Veces |
|---|---|
| Catalogo; catalogo de secciones | 1 por comando; 1 si hay Cantilever |
| Sistema resuelto; authored serializado | 1 por `RackId` |
| Registro de variables | 1 |
| Barrido de sobres | ID19: 1 (SNAPSHOT, del que salen tambien los gates); edicion: los vigentes + 1 lectura de las entradas del gate por Insertar |
| `CommonTransform2D` | 1 por seleccion proyectada |
| Importacion de bloques | ID19: 1 con la union de nombres |

## 17. Reconciliacion con iniciativas activas

### 17.1 I-49 (`75f1862`)

Desde G2, el Owner acepto ADR-0041, que reemplaza a ADR-0040, e I-49 versiono un Consensus Freeze nuevo sobre V6 + A1 + A2 (`3f17c21`;
`docs/automation/decisions/I-49.md` §15 en su rama). Despues publico la correccion G6-C2 (`75f1862`), solo en `A/Expressions/*` y sus
pruebas; el cierre de G6 no esta registrado y G7 sigue bloqueado. Su produccion vive en `A/Expressions/*`, `A/Units/LengthUnits.cs` y
`A/StructuralSections/StructuralSectionUnits.cs`. **Sin archivos de
produccion comunes.** Archivo caliente: `U/Systems/Selective/RackSelectiveWindow.xaml.cs` (G10 de I-49; fila de I-53S,
`docs/ROADMAP.md:471`). Conflicto textual trivial en `docs/adr/README.md` (I-49 cambia las filas 0040/0041, contiguas a la de 0042).

### 17.2 I-52 (`2275f21`, Proposal V11, sin consenso)

Durante la redaccion de V2, I-52 publico su Proposal V11 (solo documentacion). Las secciones que V2 cita conservan su numero, y §3.7 y
§8.1-§8.4 son identicas a las de V10. V11 anade dos decisiones sobre I-55:

- **[V11-D10] Gate entre iniciativas.** Tras su consenso tecnico y su rebase, I-52 exige una **reconciliacion obligatoria** con I-55
  antes de cualquier freeze. Esa reconciliacion registra la propiedad de seis autoridades compartidas, sin duplicarlas. Si cualquiera
  congela antes una autoridad incompatible con la otra, rige un STOP simetrico: ninguna congela hasta resolverlo. Su G4 avisa a I-55
  antes de tocar el nucleo de seleccion.
- **[V11-D12] Vigilancia de ADR-0042.** Mientras ADR-0042 siga propuesto, no hay STOP salvo que su texto ya cree una contradiccion
  material; V11 evaluo el ADR-0042 de V1. El ADR-0042 de V2 cambia dos cosas que I-52 debe re-evaluar: se presenta como complemento, no
  como sucesor, de ADR-0010, y anade a Insertar la precondicion del gate de propiedades (su C2-2b). Actualizar no cambia.

**I-55 adopta la misma regla:** sin la reconciliacion registrada no hay Consensus Freeze de I-55, y por tanto tampoco G3; ninguna de las
dos congela una autoridad incompatible con la otra.

| Autoridad compartida ([V11-D10]) | I-55 V2 | Punto |
|---|---|---|
| Taxonomia de tipo de vista | `RackViewKind`, renombre de `DimensionViewKind` (D-02) | X-2 |
| Codec semantico de `View`/`Section` | `RackViewEnvelopeCodec` con politica por consumidor (D-03) | X-2 |
| «Sistema resuelto → plan de vista» | Paso comun con todas las direcciones (§4.3) | X-2 |
| Comparador authored | `RackSiblingAuthoredGate` + comparador por kind (§9.3) | X-3 |
| Primitivo de materializacion del Plugin | «Definicion + sobre + referencia» en la transaccion del llamador | X-4 |
| Transformaciones de colocacion y proyeccion | `CommonTransform2D`, valor de colocacion y `RackViewFrame` (§12) | X-7, X-8 |
| Nucleo neutral de seleccion (aviso de su G4) | D-08a | X-1 |

Siguen siendo de I-52, aunque consuman un primitivo compartido: ST-13, ST-18, ST-19, PRE-17, PRE-18, `VisualFootprintResult`,
`GeometrySymmetryResult` y la equivalencia con `μ_k`. I-55 no los toca (X-4).

| # | Tema | I-52 V11 | I-55 V2 | Resolucion propuesta |
|---|---|---|---|---|
| **X-1** | Seleccion | nucleo neutral + fachada (§5.1); fallos ST (§4.1) | ID19 consume el nucleo | Una extraccion; hechos neutrales; politicas por llamador |
| **X-2** | Plan por vista y `View`/`Section` | autoridad que recibe el payload y resuelve (§8.1), sin laterales de Selectivo, Dinamico ni Push Back; decodificacion en Application junto al contrato de cada kind (§8.4), en la carpeta `Mirror/` segun §20.1; CT-04 | editores sin re-resolver (`P/RackSelectivoCommands.cs:421-424`); ID19 resuelve; laterales | Resolucion separada + paso comun «sistema resuelto → plan» con todas las direcciones + codec neutral; politica por consumidor |
| **X-3** | Comparador authored | del reflector, sobre vistas seleccionadas (§5.4) | ID19 sobre todas las hermanas | Contrato del kind; miembros como parametro |
| **X-4** | Materializacion | materializador generico (§8.2: definicion por familia de plan, referencia con la presentacion de la fuente, ST-13, ST-19, PRE-18) y contexto de materializacion (§8.7) | definicion + sobre + referencia con rotacion y presentacion de creacion (OD-8) | Un primitivo en la transaccion del llamador; equivalencia visual solo en I-52; puntos antes de importar |
| **X-5** | Archivos, rutas y autoridades que I-52 vigila | §19 (STOP) y §15.2: si I-55 fija una autoridad de preparacion de vistas, de materializacion o de transformacion multi-rack, o toca rutas de materializacion (`ViewBlockDraw`, `SystemBlockWriter`, `BlockPlacement`, `LateralHeaderDrawService`, `CantileverViewMaterializer`), `RackEmbedComposer.Compose`, comandos o `U/RackCommandReference.cs`, se reporta antes de fijar archivos, se re-evalua frente a su §8.1, §8.7, `MaterializationContextReadSet`, PRE-17, PRE-18, T-GRD-02 y el censo `B`, y toda autoridad comun va a Coordinador y Arquitecto; §19 tambien para en `RackDuplicationPlan.cs` y `RackEmbedComposer.cs` | **La Proposal V2 ya dispara la clausula** (propone esas tres autoridades): X-1..X-8 son el reporte. En implementacion: G5 cambia la entrada del plan en `P/RackCantileverCommands.cs` (sin tocar `A/Systems/Cantilever/CantileverLineEditorAssembler.cs`, linea base de su C2-4); G6 renombra en `SelectiveDimensions.cs`/`DynamicViewDecorations.cs`; G7 anade `Compose`; G8 re-enruta inserciones y `BlockPlacement.cs`; G9, G10 y G12 cambian Insertar, primera vista y comandos; G13 edita `RackDuplicationPlan.cs` si extrae; G14 fija la autoridad de colocacion multi-rack; G15 anade comando y ayuda | Reporte previo a I-52 (su §15.3) en cada uno de esos gates, con la lista de re-evaluacion |
| **X-6** | Lineas base de C-2 | C2-2a, C2-2b, C2-4, G-M9 (§8.5) | Planta Cantilever de PR-2 (G5, cambia un productor de C2-2a/C2-2b); Insertar re-enrutado, gate, orden de edicion y D-15 | La segunda en integrar re-establece C-2; fixtures de Insertar en G8 |
| **X-7** | Geometria de colocacion | `Transform2D.ReflectionAboutLine` y valor de colocacion `(Position2D, RotationRadians, UniformScale)` (§8.3); `P' = G·P·F` con determinante positivo (§3.2) | `CommonTransform2D` rigida y colocaciones con rotacion | Un valor de colocacion y una composicion sobre `Transform2D`; rigidez como invariante de I-55; reflexion como politica de I-52 |
| **X-8** | Origen y tramo del eje de una vista | `ViewPlanResult` devuelve el «tramo del eje de la vista (para c)» (§8.1); CT-05 caracteriza «tramos y centros `c` por (kind, vista) desde la geometria resuelta» (§3.7) | `RackViewFrame` con origen fisico y extension (§12.3) | **Un** descriptor de origen y tramo por vista y una sola caracterizacion |

Conflictos previsibles: censos de comandos (`T/SelectiveEditorOpenTests.cs:547-553`, `TGrd08`), ayuda y `TGrd02`. Si la version congelada
de I-52 contradice X-1..X-8, I-55 abre una Proposal nueva.

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
| Gate de propiedades | Autoridad global de I-54 | Rechazada: bloquea por payloads no relacionados |
| Gate de propiedades | Ignorar todo payload no interpretable | Rechazada: dejaria pasar hermanas ilegibles cuyo `Id` es legible |

## 19. Pruebas (resumen; matriz en el mapa §T)

| Clase | Clases previstas |
|---|---|
| **Caracterizacion (G3)** | `RackViewEnvelopeReadingCharacterizationTests`, `LegacyViewPayloadCompositionCharacterizationTests`, `LegacyViewBlockNameCharacterizationTests`, `RackCountInvariantCharacterizationTests`, `RackSiblingScanInputCharacterizationTests`, `RackViewFrameCharacterizationTests`, `HeadlessResolutionParityCharacterizationTests`, `FirstViewGateCharacterizationTests`; condicional `RackDuplicationPlanObservableSemanticsTests` |
| **T0** | `PushBackLateralPostIndexRegressionTests`, `FirstViewFreedomTests`, `EditorViewBatchRequestTests`, `RackViewBatchDialogTests`, `RackSelectionCoreConsumptionTests` (si I-52 ya extrajo el nucleo), `RackViewKindRenameTests`, `RackViewVariantTests`, `RackViewEnvelopeCodecTests` (tabla de §7.3, politica de ID19 y huerfanas), `RackViewSupportMatrixTests`, `RackViewFrameTests`, `RackViewPreparation*Tests`, `PreparedViewPayloadGoldenTests`, `RackIdLifecycleTests`, `RackEnvelopeIdProbeTests`, `RackSiblingCustomPropertiesGateTests`, `RackSiblingAuthoredGateTests`, `RackViewAuthoredEquivalenceTests`, `RackViewBatchPlanTests`, `CommonTransform2DTests`, `RackGroupFrameTests`, `RackRigidPlacementPolicyTests`, `RackOrthographicPlacementPolicyTests`, `RackGroupPlacementPlanTests`, `RackViewCountInvariantTests`, `RackViewPartialRackContractTests`, `CantileverPlantaVisibilityBuilderTests` |
| **T1** | `*DimensionView*`; `CustomPropertiesEnvelopeGuardTests`; `CustomPropertiesEdgeGuardTests` (sin cambio); `RackViewPlacementGuardTests`; `RackViewBatchDriverGuardTests`; `RackViewEntryPathGuardTests`; `RackProjectionCommandGuardTests`; `RackSiblingGateWiringGuardTests`; `RackSiblingGateIndependenceGuardTests`; `CantileverPlantaVisibilityGuardTests`; `CustomPropertiesCommandGuardTests`; `SelectiveEditorOpenTests`; `CantileverPluginSourceGuardTests`; `SelectiveAuthoredCarrierAdoptionTests`; `PushBackPluginSourceGuardTests`; `RackUnitsGuardSourceTests`; `RackDuplicationPlanTests`; UI: `RackInsertionRequestTests`, `RackEditorSessionTests`, `EditorModuleRegistryTests`, `DynamicShellMigrationTests`, `RichEditorBlockedActionTests`, `DynamicHeaderBatchSeamGuardTests`, `SelectiveHeaderBatchSeamTests`, `WindowCensusGuardTests`, `DialogWindowCharacterizationTests`, `CustomPropertiesHelpCensusTests` |
| **T2/T3/T4** | suites completas; CI y coberturas; validacion del Owner |

## 20. Validacion del Owner (diseno; no ejecutar)

Mapa §OV: OV-LEG, OV-ID17, OV-ID18, OV-META (incluidos propiedades divergentes, remedio con `RACKPROPIEDADES` en solo lectura y payloads
ajenos no interpretables), OV-ID19 por modo y segun M-01 (incluidos layout girado colocado a mano, layout enlazado espejado rechazado y
100 o mas racks) y OV-PR.

## 21. Gates (resumen; detalle y tabla V1 → V2 en el mapa)

| Gate | Objetivo | Precondicion |
|---|---|---|
| G3 | Caracterizacion | consenso V2 + **M-01 resuelta** + reconciliacion con I-52 registrada ([V11-D10]) + Consensus Freeze + ADR-0042 aceptado + OD-1..OD-8 + X-1..X-8 acordados |
| G4 | PR-1 | G3 |
| G5 | PR-2 (solo Plugin) | G3 |
| G6 | Foundation pura | G4, G5 |
| G7 | Plan por vista y preparacion | G6 |
| G8 | Colocacion de vista unica + D-10 + prompt + fixtures de Insertar | G7 |
| G9 | Precondiciones nuevas de `RACKEDITAR` → Insertar (vista unica): gate de propiedades del `RackId` y D-15 | G8 |
| G10 | ID17 | G9 |
| G11 | ID18 contrato puro (estados, incluidos `SIBLING_GATE_FAILED` y `REDRAW_FAILED`) | G10 |
| G12 | ID18 UI, presentador, driver (gate, PREPARE ALL y D-15 en lote) y rutas de entrada | G11 |
| G13 | Nucleo neutral de seleccion (X-1) | acuerdo X-1 |
| G14 | ID19 puro: Group Placement, politicas, grupos, clasificacion, resolucion, gates | G12, G13 |
| G15 | ID19 comando + primitivo (X-4, X-7) + censos | G14; OD-1; OD-8 |
| G16 | Candidato e integracion | todo |

## 22. Registro de decisiones

### 22.1 Decisiones tecnicas propuestas

| # | Decision | Seccion | Revision |
|---|---|---|---|
| D-01 | ALT-B | §4 | Arquitecto |
| D-02 | `DimensionViewKind` → `RackViewKind` | §7.1 | **A-4** |
| D-03 | Variante tipada + codec con disposicion y politica por consumidor; huerfanas | §7 | **A-1**, X-2 |
| D-04 | Matriz normativa | §5 | Coordinador |
| D-05 | `RackId` al aceptar la intencion; curacion vigente | §6 | Arquitecto |
| D-06 | ID18 transaccion C | §11 | **A-3** |
| D-07 | Gates acotados al `RackId` (propiedades con sonda de `Id`; authored) | §9 | **A-7**, X-3 |
| D-08 | Seleccion, grupos y clasificacion (D-08a..h) | §12.8 | **X-1**, Arquitecto |
| D-09 | PR-1 y PR-2 (solo Plugin) antes de la foundation | §17.4 | Coordinador |
| D-10 | Limpieza ante excepcion | §14 | Arquitecto |
| D-11 | Presentacion de creacion | §12.9 | OD-8 |
| D-12 | ADR-0042 complementa y enmienda reglas acotadas de ADR-0010 | ADR | **A-8**, Owner |
| D-13 | Re-verificacion de bloques antes de escribir | §12.9 | Arquitecto |
| D-14 | Una autoridad por responsabilidad con I-52 | §17.2 | **X-1..X-8** |
| D-15 | Redibujo fallido de un miembro del gate impide insertar (vista unica en G9; lote en G12) | §9.2, §11 | Arquitecto + Coordinador |
| **D-16** | Group Placement: marcos y **una** `CommonTransform2D` rigida por seleccion proyectada | §12.2-§12.3 | **A-6** |
| **D-17** | Resolucion sin editor antes de validar; diagnosticos bloqueantes; paridad | §12.1, §12.9 | Arquitecto |
| **D-18** | Politicas `Rigid` y `Orthographic`, con anclas alineadas al tramo para sentidos paralelos opuestos | §12.4-§12.5 | **A-6**, M-01 |

**D-12, detalle.** ADR-0010 (`docs/adr/0010-actualizar-redibuja-insertar-liga-vistas.md:35-46`) dice, literal: «Insertar una vista
crea una representación adicional ligada al rack existente y conserva su identidad. Cada editor puede sincronizar previamente las
representaciones existentes según su flujo implementado.»; «Una vista adicional solo se inserta desde un rack ya existente, para que
disponga de diseño e identidad fuente.»; y «No cambia la inserción inicial que crea un rack nuevo ni promete capacidades multivista para
todos los editores.». ADR-0042 **conserva** Actualizar e Insertar y **enmienda solo**: (1) la
regla de vista adicional → A rack materializado **o** B intencion de creacion aceptada con `RackId` unico, authored, sistema resuelto y
vistas preparadas; (2) **dos precondiciones nuevas de Insertar**: el gate de propiedades del `RackId` y D-15. **Relacion entre ADR:** el
indice declara inmutable un aceptado, pide reemplazarlo para cambiar una decision y admite «Notas posteriores» con fecha
(`docs/adr/README.md:31-36`; ADR-0010 ya tiene esa seccion, `:77`). Formas para el Arquitecto: (a) ADR-0042 complementario + nota
posterior fechada en ADR-0010 que enlace las reglas enmendadas; hay precedente de complemento (ADR-0028, ADR-0029) y de nota que enlaza un
ADR posterior en un aceptado no reemplazado (`docs/adr/0005-estrategia-de-unidades.md:125`, sobre ADR-0021), aunque alli no cambiaba una
regla; (b) reemplazo acotado con nota fechada que diga que sigue vigente (`docs/adr/0008-secciones-unificadas-por-rol.md:3`, `:75`). El
reemplazo completo queda descartado (CR-08). CR-08 pide complementar y enmendar; el mecanismo exacto queda para A-8.

### 22.2 Decisiones del Owner

| # | Decision | Opcion A | Opcion B | Compromiso | Recomendada | Consecuencia |
|---|---|---|---|---|---|---|
| OD-1 | Nombre del comando | `RACKPROYECTAR` + `RPY` | `RACKPROYECTARVISTAS` | A corto; B explicito | **A** | Censos, ayuda, README |
| OD-2 | Variante en ID19 (clase distinta) | canonica fija | preguntar por sistema | A determinista | **A** | En misma clase decide OD-2.b (M-01) |
| OD-3 | Miembro no soportado | fallar todo | omitir con aviso | A predecible | **A** | Reseleccionar |
| OD-4 | Esc a mitad de lote | detener | saltar | A: Esc = parar | **A** | Cola parcial |
| OD-5 | Orden del lote | F → L → P | orden de marcado | A determinista | **A** | El dialogo lo muestra |
| **OD-6** | Marcos (M-01: 6.b, 6.c, 6.d) | §12.7 | §12.7 | §12.7 | **A, A, A** | Validacion y politica |
| **OD-7** | Modos (M-01: 7.a, 7.b, 7.c, 7.d) | §12.7 | §12.7 | §12.7 | **A, A, A, A** | Idem |
| OD-8 | Presentacion de vistas proyectadas | creacion vigente | de la fuente | A coherente con Insertar | **A** | B exige capturar presentacion |

### 22.3 Open Material

- **M-01 — Semantica de Group Placement / Orthographic Projection: marco fuente, marco destino, modo por par, tipos y familias mezcladas y
  variante en misma clase** (OD-6.b/c/d, OD-7.a/b/c/d, OD-2.b; §12.7). **Abierta.** Mientras siga abierta no hay consenso posible sobre V2
  (CR-02) y no puede abrirse G3.
- **Entre iniciativas: X-1..X-8** (§17.2), con resolucion propuesta; requieren a los Arquitectos de I-55 e I-52 y se cierran en la
  reconciliacion obligatoria de las seis autoridades compartidas que exige I-52 V11 ([V11-D10]) antes de cualquier freeze.

### 22.4 Open Minor

| # | Tema |
|---|---|
| OM-1 | `SaveToLibrary` acuna antes en Cantilever y Push Back |
| OM-2 | Convergencia futura de `CantileverViewKind` |
| OM-3 | Atomicidad del redibujo multivista |
| OM-4 | Uniformar la eleccion de variante en vista unica |
| OM-5 | Bloques importados que sobreviven a fallos (vigente) |
| OM-6 | Vista fantasma de grupo en ID19 |
| OM-7 | `View` nulo de la Cama (F-04) |
| OM-8 | «frontal ×N» de `RACKLISTA` (H-12) |
| OM-9 | Lateral entero del Dinamico solo legado |
| OM-10 | Duplicados de `(RackId, View, Section)` legales (OQ-4) |
| OM-12 | `Orthographic` con sentidos perpendiculares de K (p. ej. plantas de Cantilever y de rack con OD-6.d A) falla |
| OM-13 | `Orthographic` con orientaciones no paralelas falla; `Rigid` las admite |
| OM-14 | Una vista nueva hereda el `ExtensionData` del sobre de otra vista (vigente) |
| OM-15 | Frontal ↔ Lateral y misma clase dependen de M-01 |
| OM-17 | `FindRackBlocks` no expone dependencia de xref ni texto de payload: el gate los obtiene por su lectura |
| OM-18 | El remedio `RACKPROPIEDADES` puede exigir reparar antes un payload ajeno o una unificacion bloqueada (R-15) |
| OM-19 | Un payload no interpretable sin `Id` legible no es atribuible y no bloquea (CR-03); riesgo residual R-14 |

Retirados de V1: **OM-11** (`Id` interior del Cantilever desalineado) sale con H-13 (§22.5); **OM-16** (sin aviso de propiedades
divergentes al insertar) lo resuelve el gate de §9. OM-12, OM-13 y OM-15 conservan su tema, reformulado para V2.

### 22.5 Hallazgo registrado fuera de I-55

**H-13 — `Id` interior del Cantilever sin alinear al crear** (`D/Systems/Cantilever/CantileverLineDesign.cs:333`;
`U/Systems/Cantilever/RackCantileverWindow.xaml.cs:199-210`; `P/RackCantileverCommands.cs:472-498`; solo pruebas lo leen:
`T/CantileverLineTests.cs:908`, `T/CantileverRoundTwoCharacterizationTests.cs:372`). Candidato a iniciativa independiente; I-55 no lo toca.

## 23. Revision adversarial (antes del commit)

**Pasada 1 (propia)** sobre los doce puntos de CR-12 y ejemplos numericos de las formulas. **Pasada 2** con un agente de solo lectura
contra el codigo de `dad4e77` e I-52 V10; cada hallazgo se verifico antes de corregir.

| # | Hallazgo | Severidad | Correccion |
|---|---|---|---|
| 1 | «Una por grupo» con grupo = `RackId` especificaba N transformaciones | BLOCKER | Vocabulario de seleccion proyectada; un marco, una transformacion por ejecucion; INV-GRP-1 sobre todos los pares; INV-GRP-2 de orientacion (§12.2) |
| 2 | Tabla del codec desalineada con produccion e I-52 (lado B en Push Back de un sentido, `View` vacio con `Section ≥ 0`, huerfanas, capitalizacion, planta y cabecera, cama) | HIGH | §7.3 reescrita fila a fila; huerfanas fallan en ID19 |
| 3 | Escala (−1,−1) aceptada con orientacion erronea | MEDIUM | `φ_r` desde la parte lineal (`RotationAngle`) |
| 4 | Orden de composicion ambiguo; Z y `Origin` | MEDIUM | `Then` explicito; `Z_r` por politica; `Origin = 0` citado en ambos materializadores |
| 5 | Orientaciones de marco sin definir | MEDIUM | `φ_s`, `φ_t` como entradas; `α` por politica |
| 6 | Familias mezcladas en `Orthographic` mal descritas | MEDIUM | Anclas alineadas al tramo solo con sentidos paralelos; perpendiculares fallan; OV corregida |
| 7 | Resolucion despues de validar | MEDIUM | RESOLVE antes de VALIDATE (§12.1) |
| 8 | OD-2 cambiaba en silencio la misma clase | MEDIUM | OD-2.b dentro de M-01 |
| 9 | Payload no interpretable con `Id` legible tratado como ajeno | MEDIUM | `RackEnvelopeIdProbe` (§9.2) |
| 10 | Cura de `Id` en blanco sin gate | MEDIUM | El sobre fuente o elegido siempre es miembro |
| 11 | Remedio `RACKPROPIEDADES` posiblemente inalcanzable | MEDIUM | Remedio condicionado; R-15; OV |
| 12 | ADR-0042 cambiaba Insertar sin declararlo | MEDIUM | Precondiciones nuevas declaradas; cita literal de ADR-0010 |
| 13 | X-5 sin la clausula de I-52 para I-55; PR-2 tocaba el ensamblador | MEDIUM | X-5 ampliado; PR-2 solo en el Plugin |
| 14 | Autoridad duplicada de origen y tramo de vista | MEDIUM | X-8 |
| 15 | Regla de kind y conjunto de miembros desalineados con I-54 | LOW | Regla de I-54; mismo conjunto para D-15 |
| 16 | Separacion del gate sin guarda | LOW | `RackSiblingGateIndependenceGuardTests` |
| 17 | «Ilegible ajeno no bloquea» no universal (F-14a) | LOW | Alcance acotado; F-14a residual |
| 18 | Tipos fuente mezclados fijados fuera de M-01 | LOW | OD-7.c |
| 19 | Restos de PR-3/H-13 en gates, goldens y mapa de archivos | LOW | Retirados de gates, pruebas y cambios de produccion; solo se nombran como retirados o fuera de alcance |
| 20 | Preferencia del Coordinador atribuida sin registro | LOW | Atribucion acotada a lo que registra CR-08 (complementar y enmendar); el mecanismo queda en A-8, con precedentes citados |
| 21 | Titulo del indice de ADR desactualizado | LOW | Actualizado |
| 22 | Incoherencias entre §21 y el mapa | LOW | Corregidas |
| 23 | Cabecera de Open Material incompleta | LOW | M-01 + X-1..X-8 |
| 24 | Paquetes y registro con restos | LOW | Corregidos |
| 25 | Expectativas de validacion del Owner incompletas | LOW | OV-ID19 corregida |

**Pasada 3: verificacion independiente de las correcciones** (agente de solo lectura sobre los seis documentos, con ejemplos numericos
propios y unas 60 citas contrastadas). Sin BLOCKER; cada hallazgo se verifico antes de corregir.

| # | Hallazgo | Severidad | Correccion |
|---|---|---|---|
| 26 | El sentido de `e` normalizado a [−90°, 90°) cambiaba justo en la corrida de una planta de rack sin girar (+Y): las frontales salian en orden inverso respecto de su propio +R, la ida y vuelta planta → frontal → planta invertia el orden y, con tramos distintos, desalineaba los postes 0; INV-GRP-1 seguia valiendo, asi que sus pruebas no lo detectaban | HIGH | El destino elige el sentido de `e` (mayoria de `σ_s = σ_t`; empate en [−45°, 135°)); ejemplo numerico; frase de 180° corregida; pruebas de continuidad en la rotacion y de ida y vuelta (§12.5; mapa G14) |
| 27 | Superposicion y planes evaluados tras los puntos, contra «todo fallo antes de los puntos» | LOW | PLANS antes de PICK; superposicion en VALIDATE; filas nuevas en §14 y §15 |
| 28 | «`ρ_r` independiente de `r`» falso con OD-6.d B y OD-7.d B | LOW | «depende solo de `φ_t` y de la vista destino» en todos los documentos |
| 29 | X-5 sin G10 y X-6 sin G5 | LOW | Incorporados (§17.2; mapa §1, G5, G10 y R-03) |
| 30 | Guarda de lecturas de G3 incompleta frente a I-52 V10 §8.4 | LOW | Lista alineada (mapa G3; §7.3) |
| 31 | Rama sin escribir para un payload no interpretable con `Id` legible de otro rack | LOW | «Id legible ≠ → NO bloquea» (§9.2); F-14a en el paquete del Coordinador |
| 32 | Registro G2B sin X-1..X-8 en Open Material | LOW | Incorporado |
| 33 | El paquete del Arquitecto atribuia la nota fechada a CR-08 | LOW | Corregido |
| 34 | «Solo §22.5 nombra H-13» inexacto | LOW | «Solo se nombra como retirado o fuera de alcance» |
| 35 | La edicion suma una lectura por Insertar que §16 no contaba | LOW | Contada (§16; mapa §P) |
| 36 | «`RACKLAYOUT` solo produce 0/90/180/270» inexacto: copia la rotacion de la semilla (`P/RackLayoutCommands.cs:198`, `:252`) | LOW | OV-ID19-06 reformulada |
| 37 | D-01 sin pregunta al Arquitecto; OD-6 sin `.a` sin explicar; §19 incompleto | LOW | D-01 en el paquete y en el registro; nota en §12.7; §19 completo |

**Re-fetch final.** Antes del commit, I-52 avanzo de `636f7fd` a `2275f21` (Proposal V11, solo documentacion). Se contrastaron las
secciones citadas, que conservan su numero y cuyas §3.7 y §8.1-§8.4 son identicas, y se incorporaron [V11-D10] y [V11-D12] (§17.2,
§21, §22.3). I-49 avanzo a `75f1862` (G6-C2, solo `A/Expressions/*` y sus pruebas; §17.1). `main` no cambio.

**Doce puntos de CR-12 tras las correcciones:** (1) transformacion comun unica por seleccion (INV-GRP-1/2); (2) misma clase con `Rigid` y
OD-2.b; (3) fuente rotada, con el sentido de `e` elegido por el destino; (4) marco destino `φ_t`; (5) tipos mezclados en OD-7.c; (6) gate
con sonda y sobre fuente; (7) politica de ID19 y huerfanas; (8) enlazado con una definicion nueva y N referencias; (9) copias sin cambio;
(10) X-1..X-8; (11) PR-3 fuera de gates, pruebas, OV y mapas de archivos; (12) ADR-0010 conserva lo no afectado.
