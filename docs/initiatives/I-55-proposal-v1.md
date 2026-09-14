# I-55 — Proposal V1: View Placement & Projection (ID17 + ID18 + ID19)

```text
PROPOSAL V1 — NOT CONSENSUS
Coordinator    = REVIEW REQUIRED
Architect      = REVIEW REQUIRED
Consensus      = NOT REACHED
Implementation = BLOCKED
ADR-0042       = PROPUESTO (no aceptado; sucesor propuesto de ADR-0010)

Initiative     = I-55 — View Placement & Projection
Branch         = feature/creacion-de-vistas
BASE_SHA       = ba497f14581d81e83a27514852d6ec082ff57635   (base original del reclamo; codigo auditado por el Discovery)
CURRENT_BASE   = dad4e77f4f267b9fa248ecb0c8bfd8a74bbab093   (merge de I-53D; base de G1.1 y de esta Proposal)
CURRENT_MAIN   = dad4e77f4f267b9fa248ecb0c8bfd8a74bbab093   (re-fetch previo a la Proposal: main no avanzo)
G1_CORRECTION  = cfdb702a83554df6472eddbdc59cf43a2ba7bd94   (framing vinculante)
Paralelas      = I-49 bcbf570 (rebasada sobre dad4e77; ADR-0041 propuesto en su rama) · I-52 636f7fd (Proposal V10, base 46fcac2)
Discovery      = docs/initiatives/I-55-discovery.md
Decisiones     = docs/automation/decisions/I-55.md
Mapa           = docs/initiatives/I-55-implementation-map-v1.md   (misma version del plan)
ADR            = docs/adr/0042-preparacion-de-vistas-antes-de-materializar.md
Citas          = archivo:linea sobre CURRENT_BASE, verificadas antes del commit (§23)
Prefijos       = P/ src/RackCad.Plugin/ · A/ src/RackCad.Application/ · D/ src/RackCad.Domain/ · U/ src/RackCad.UI/
                 T/ tests/RackCad.Tests/ · TU/ tests/RackCad.UI.Tests/
```

> **Como leer esta Proposal.** §1 fija el contrato de producto; §2 contrasta P1..P21 con la evidencia; §3-§5 dan la linea
> base, la arquitectura y la **matriz normativa**; §6-§9 resuelven identidad, tipo de vista, metadatos y autoridad entre
> hermanas; §10-§12 disenan ID17, ID18 e ID19 (§12.4: **proyeccion ortografica por familia de marco**); §13-§16 fijan
> invariantes, cancelacion, transacciones y recursos; §17 reconcilia con I-49, I-52 e I-53D (**X-1..X-6**); §18 compara
> alternativas; §19-§21 resumen pruebas, validacion del Owner y gates; §22 es el registro de decisiones; §23 la revision
> adversarial.

## 1. Contrato de producto (vinculante, CD-01..CD-04)

| ID | Capacidad | Resultado observable |
|---|---|---|
| **ID17** | FIRST-VIEW FREEDOM | Un rack nuevo empieza por **cualquier vista que su sistema realmente soporte** (§5) y despues recibe las demas **sin perder `RackId`, authored ni coherencia**. No se busca uniformidad entre sistemas (OQ-2) |
| **ID18** | MULTI-VIEW QUEUE / BATCH | En **un** flujo el usuario elige **varias vistas de UN rack** y las coloca consecutivamente. **Todas comparten el mismo `RackId`** |
| **ID19** | MULTI-RACK PROJECTION | El usuario selecciona **varios racks existentes**, pide **una misma clase de vista** y RackCad la genera para todos **preservando su layout relativo con UNA transformacion comun**. **Cada rack conserva SU `RackId`**: no es `RACKDUPLICAR`, no hay `NewRackId` ni re-estampado |

**Infraestructura comun:** *preparar una representacion de vista antes de materializarla en AutoCAD*.

**Fuera de alcance:** produccion antes del consenso; H-01..H-12 de paso (salvo los prerrequisitos aislados de §17.4);
unicidad de `(RackId, View, Section)` (OQ-4); Drive-In; cambiar `RACKDUPLICAR`, `RACKLAYOUT` o `RACKRELLENAR`; insercion de
componentes sueltos del Cantilever (no son vistas de rack, H-03); reabrir ADR-0009/0034/0035/0039 (ADR-0010: §22, D-12).

## 2. Principios P1..P21 frente a la evidencia

Veredicto: **ADOPTADO**, o **ADOPTADO CON PRECISION** cuando la evidencia obliga a concretarlo. Ninguno queda contradicho.

| P | Principio | Veredicto | Evidencia y precision |
|---|---|---|---|
| P1 | Un rack logico no es una vista | ADOPTADO | Una vista es una definicion con sobre; el rack es el conjunto de definiciones con el mismo `Id` |
| P2 | Una vista fisica no es un rack logico | ADOPTADO CON PRECISION | El BOM toma como representante la primera hermana del barrido y, fuera del Selectivo, no compara autoridad (`A/Bom/BomAuthoredAuthority.cs:104-109`); `EditCama` redibuja solo la definicion elegida (`P/RackCamaCommands.cs:226-237`). I-55 no las cambia; §5 excluye a la Cama de ID18/ID19 |
| P3 | `RackId` identifica el rack | ADOPTADO CON PRECISION | Autoridad = `RackEmbedDocument.Id` (ADR-0009). Copias interiores: el Selectivo alinea el `Id` de su documento al insertar (`P/RackSelectivoCommands.cs:363-376`); el **Cantilever** acuna `CantileverLineDesign.Id` al construir el diseno (`D/Systems/Cantilever/CantileverLineDesign.cs:333`), ninguna creacion lo alinea (`P/RackCantileverCommands.cs:472-498`) y solo el re-estampado lo hace (`P/KindHandlers/CantileverKindHandler.cs:105`); solo pruebas lo leen. PR-3 (H-13) |
| P4 | `View` describe Frontal/Lateral/Planta | ADOPTADO CON PRECISION | La Cama persiste `View` nulo (`P/RackCamaCommands.cs:197-198`, F-04); el codec lo lee como Lateral solo para `cama` |
| P5 | `Section`/Variant describe la representacion | ADOPTADO | Variante tipada + codec (§7); `Section` sigue en el sobre |
| P6 | Sin segundo enum si `DimensionViewKind` sirve | ADOPTADO CON PRECISION | `DimensionViewKind` ya es «el TIPO de vista» (`A/Systems/Shared/DimensionViewPolicy.cs:6-16`): se renombra (§7.1) |
| P7 | Una hermana conserva `RackId` | ADOPTADO | Rutas de insercion con el `id` del sobre elegido; un sobre sin `Id` se cura como hoy (§6.3) |
| P8 | ID19 conserva los `RackId` originales | ADOPTADO | §12 |
| P9 | ID19 sin re-estampado, `NewRackId` ni `RackCloner` | ADOPTADO | ID19 materializa desde el diseno; solo reutiliza el nucleo neutral de seleccion (X-1) |
| P10 | Authored y effective son fronteras distintas | ADOPTADO | §8 |
| P11 | Nunca reconstruir authored desde geometria efectiva | ADOPTADO | Regla de I-48 G4C.1 (`P/RackSelectivoCommands.cs:249-258`, `:411-425`) |
| P12 | `ProjectVariableReference` sobrevive | ADOPTADO | El authored viaja verbatim (§8) |
| P13 | Expresiones futuras de I-49 sobreviven | ADOPTADO | Mismo mecanismo; I-55 no las evalua por vista (§17.1) |
| P14 | `DimensionViews` sobrevive | ADOPTADO | Vive en el diseno y los builders lo aplican |
| P15 | `CustomProperties` sobreviven | ADOPTADO CON PRECISION | Viajan por el origen de `Compose` (`A/Persistence/RackEmbedComposer.cs:43-60`); una vista nueva copia la coleccion de un miembro existente y no crea un valor nuevo (§9) |
| P16 | `ExtensionData` y `SchemaVersion` sobreviven | ADOPTADO CON PRECISION | `Compose` con el origen de cada flujo; heredar el `ExtensionData` de otra vista es el comportamiento vigente de Insertar (OM-14) |
| P17 | `RACKLISTA` y `RACKBOMTOTAL` cuentan racks | ADOPTADO | Agrupan por `Id` |
| P18 | Agregar vistas no agrega cantidad logica | ADOPTADO CON PRECISION | Copias = maximo de referencias directas entre vistas (`P/RackInventarioCommands.cs:68-72`; `P/RackInventarioCommands.BomTotal.cs:109-113`); §13 precisa el caso de copias 0 |
| P19 | Sin geometria nueva | ADOPTADO | Builders vigentes; la proyeccion calcula posiciones, no dibuja (§12.4) |
| P20 | Sin Drive-In | ADOPTADO | Boton deshabilitado (`U/RackMainMenuWindow.xaml:139-144`) |
| P21 | Sin arreglar H-01..H-12 de paso | ADOPTADO CON PRECISION | H-01, H-02 y el nuevo H-13 bloquean el contrato: prerrequisitos aislados **antes** de la primera produccion (§17.4, CD-07) |

## 3. Linea base que la Proposal cambia

| Aspecto | Hoy | Consecuencia para I-55 |
|---|---|---|
| Primera vista | Selectivo solo Frontal; Dinamico solo Lateral; Cabecera solo Lateral; Push Back y Cantilever libres | ID17 abre tres puertas |
| Vistas por gesto | Una; la ventana cierra | ID18 necesita una peticion de N vistas |
| Variante | Prompt del Plugin en Selectivo y Dinamico; ventana en Push Back y Cantilever; Push Back usa **posicion de lista** como poste (H-01) | Variante tipada decidida antes de preparar |
| Payload | `Build*Payload` por comando en el Plugin; siete llamadas a `Compose` fijadas por `TGrd02` | Se mueve a Application; el censo se reapunta con motivo |
| Materializacion | `SystemBlockWriter.CreateBlock` importa, crea y confirma antes del jig (`P/Systems/Shared/SystemBlockWriter.cs:18-40`); `BlockPlacement` coloca y limpia al cancelar (`P/Drawing/BlockPlacement.cs:37-42`, `:64-76`); Cantilever por `CantileverViewMaterializer` | Limpieza tambien ante excepcion en ambas primitivas (D-10) |
| Vista enlazada | Redibujo por vista (un fallo se ignora y sigue: `P/RackSelectivoCommands.cs:178-183`; `P/RackCantileverCommands.cs:328-332`), `Regen`, insercion | Preparar → redibujar → colocar; un fallo de redibujo detiene la insercion (D-15) |
| Multi-rack | No existe para vistas; las copias crean identidad nueva | ID19 nuevo, con `RackId` originales |

## 4. Arquitectura propuesta (ALT-B, D-01)

### 4.1 Responsabilidades

```text
Estado logico del rack   authored serializado UNA vez + identidad + sistema resuelto UNA vez
        │                (editor en ID17/ID18; resolucion sin editor en ID19)
        ▼
RackViewSpec             RackId + Name + Kind (sobre) + RackViewAddress (RackViewKind, RackViewVariant)
        ▼
Preparacion (pura)       soporte y disponibilidad (§5) → codec (§7.3) → plan por vista desde el SISTEMA RESUELTO (X-2)
        │                → composicion del sobre → nombre base → PreparedRackView | fallo tipado
        ▼
Colocacion (Plugin)      importar bloques fuera de la transaccion → definicion + sobre
        │                → jig (ID17/ID18) o posicion calculada (ID19) → confirmar, o limpiar ante cancelacion o excepcion
        ▼
Definicion + referencia

Lote (ID18)              RackViewBatchPlan (puro): orden, estados y mensajes; el Plugin ejecuta los jigs
Proyeccion (ID19)        RackProjectionPlan (puro): grupos, clasificacion, autoridad, marcos, T_common, posiciones;
                         el Plugin captura, prepara y materializa en UNA transaccion
```

| Capa | Hace | No hace |
|---|---|---|
| **Preparacion** (Application) | Valida soporte y disponibilidad; construye el plan desde el sistema resuelto; compone el sobre; propone nombre base | No coloca, no abre transacciones, no inventa identidad, no reconstruye authored, no conoce `ObjectId` ni AutoCAD, no hace I/O (catalogos ya cargados), **no resuelve dos veces** un sistema que el editor ya entrego |
| **Colocacion** (Plugin) | Importa, crea la definicion con nombre unico, escribe el sobre, coloca, limpia, informa | No decide soporte, variante, identidad, autoridad ni posiciones de grupo |
| **Proyeccion** | Application: grupos, marcos, transformacion, posiciones. Plugin: seleccion, puntos, lectura, materializacion | No pide un punto por rack |
| **Builders** | Unica autoridad geometrica | No se duplican ni se envuelven en un framework |

**Una autoridad por responsabilidad (D-14).** I-52 V10 propone en Application una autoridad pura de planes por vista
«compartida con el redibujo» (su §8.1), la lectura de `View`/`Section` (§3.7, §8.4), un comparador authored por kind (§5.4)
y un materializador del Plugin por familia de plan (§8.2), y registra el riesgo de duplicarlas con I-55 (R-45). I-55 **no
crea autoridades paralelas**: las compone y aporta encima direccion tipada, soporte, composicion del sobre, lote y
proyeccion. Extraccion y secuencia: X-1..X-6 (§17.2).

### 4.2 Tipos (nombres no contractuales; responsabilidades si)

Namespace neutral `RackCad.Application.Views` (libre en `src/`; I-52 V10 §20.1 ubica su decodificacion en `Mirror/`: X-2).

| Tipo | Forma | Responsabilidad |
|---|---|---|
| `RackViewKind` | `enum { Frontal, Lateral, Planta }` | Unica taxonomia de tipo de vista (renombra `DimensionViewKind`) |
| `RackViewVariant` | sellada, igualdad por valor: `Whole`, `Fondo(int)`, `Post(int)`, `FlowEnd(DynamicRackEnd)`, `PushBackCut(PushBackFrontalEnd, PushBackSide)`, `Station(int)` | Representacion semantica; nunca un indice de UI |
| `RackViewAddress` | `(RackViewKind, RackViewVariant)` | Direccion completa |
| `RackViewEnvelopeCodec` | estatico por `Kind` | `Encode(address) → (View, Section)`; `Decode(kind, View, Section) → (address, disposicion)`; compone `PushBackSystemFrontalBuilder.EncodeSection/DecodeSection/IsValidSection` (`A/Systems/PushBack/PushBackSystemFrontalBuilder.cs:138-155`) y `CantileverViewPlanBuilder.SectionFor` (`A/Systems/Cantilever/CantileverViewPlanBuilder.cs:238-239`) |
| `RackViewSupport` | estatico por sistema | Soporte, disponibilidad, variante canonica de ID19, pares de proyeccion |
| `RackViewFrame` | estatico por `(sistema, RackViewKind, variante)` + sistema resuelto | Descriptor del marco local: que eje fisico representa cada eje local y donde cae el origen fisico (§12.4) |
| `RackViewPreparationContext` | por rack: authored/diseno serializado una vez, sistema resuelto, catalogos, sobre origen, proyecto interior | Recursos compartidos (§16) |
| `IRackViewPreparer` | una implementacion por sistema, registro explicito sobre `KindDispatch<T>` (`A/Persistence/KindDispatch.cs:19`) | `Prepare(spec, context) → ViewPreparationResult` |
| `PreparedRackView` / `ViewPreparationResult` | payload + plan + nombre base + diagnosticos / exito o fallo tipado | Nada parcial |
| `RackViewBatchPlan` | lista ordenada + maquina de estados + mensajes | ID18 sin AutoCAD |
| `RackProjectionPlan` / `RackProjectionTransform` | puros | ID19 sin AutoCAD |

### 4.3 Preparacion por sistema (builders vigentes)

| Sistema | Direcciones | Plan (desde el sistema resuelto) | Payload |
|---|---|---|---|
| Selectivo | Frontal `Fondo(k)`, Lateral `Post(p)`, Planta `Whole` | `SelectiveFrontalBuilder.BuildPlan(SelectiveDepthLayout.FondoSystemView(system, k), catalog)`; corte de `SelectiveLateralBuilder.Cortes` + `LateralHeaderLayoutBuilder.Build` + `HeaderInstanceGrouper.Group`; `SelectivePlantaBuilder.BuildPlan(system, catalog)` | authored JSON verbatim |
| Dinamico | Lateral `Post(p)`, Frontal `FlowEnd(e)`, Planta `Whole` | `DynamicSystemLateralBuilder.Build(system, catalog, postIndex)`; `DynamicSystemFrontalBuilder.BuildPlan(system, catalog, end)`; `DynamicSystemPlantaBuilder.BuildPlan(system, catalog)` | `RackProject.ForDynamic(design)` con metadata interior |
| Push Back | Lateral `Post(p)`, Frontal `PushBackCut(e, s)`, Planta `Whole` | `PushBackSystemLateralBuilder.Build(system, catalog, postIndex)`; `PushBackSystemFrontalBuilder.BuildPlan(system, catalog, end, side)`; `PushBackSystemPlantaBuilder.BuildPlan(system, catalog)` | `RackProject.ForPushBack(design)` con metadata interior |
| Cantilever | Frontal `Whole`, Lateral `Station(s)`, Planta `Whole` | `CantileverViewPlanBuilder.Build(line, kind, factory, station, plantaVisibility)` (PR-2) | `RackProject.ForCantilever(design)` |
| Cabecera | Lateral `Whole`, Planta `Whole` | `HeaderInstanceGrouper.Group(LateralHeaderLayoutBuilder.Build(...).Instances, nombre)`; planta con `PlantaHeaderLayoutBuilder.Build(config, catalog)` | `RackProject.ForSelective(configuration)` con metadata interior |
| Cama | Lateral `Whole` | `FlowBedLateralBuilder.Build(config, catalog)` | `FlowBedDocument` con version y extension data de origen |

Las reglas de nombre base de los servicios del Plugin pasan a una funcion pura; `UniqueBlockName` sigue en el Plugin (I-09).

## 5. Matriz normativa sistema × ViewKind × variante

**«Vista soportada».** Una direccion de un sistema esta soportada si y solo si **(a)** tiene builder, **(b)** el codec la
codifica y la edicion la reconoce y la redibuja entre sus hermanas, y **(c)** existe en el sistema resuelto. (a)+(b) son
estaticos; (c) es disponibilidad y la calcula `RackViewSupport`. Saber dibujar una vista no basta: ID17 alinea la UI con (a)+(b).

| Sistema | ViewKind | Variante | CanStartWith | CanAddLater | CanBatch | Destino ID19 (desde) | VariantSelectionOwner | Canonica ID19 |
|---|---|---|---|---|---|---|---|---|
| SelectiveRack | Frontal | `Fondo(k)`, `0 ≤ k < fondos` | **SI** | SI | SI | SI (planta) | Editor (lote); prompt legacy | `Fondo(0)` |
| SelectiveRack | Lateral | `Post(p)`, `p ∈ Cortes(system).PostIndex` | **SI (nuevo)** | SI | SI | SI (planta) | Editor (lote); prompt legacy | `Post(min p)` |
| SelectiveRack | Planta | `Whole` | **SI (nuevo)** | SI | SI | SI (frontal o lateral) | — | `Whole` |
| PalletFlow | Lateral | `Post(p)` | SI | SI | SI | SI (planta) | Editor (lote); prompt legacy | `Post(min p)` |
| PalletFlow | Frontal | `FlowEnd(Exit \| Entrance)` | **SI (nuevo)** | SI | SI | SI (planta) | Editor | `FlowEnd(Exit)` |
| PalletFlow | Planta | `Whole` | **SI (nuevo)** | SI | SI | SI (frontal o lateral) | — | `Whole` |
| PushBack | Lateral | `Post(p)` con **`p` = `PostIndex` real** | SI | SI | SI | SI (planta) | Editor (tras PR-1) | `Post(min p)` |
| PushBack | Frontal | `PushBackCut(EntradaSalida \| Posterior, A \| B)`; B solo compuesto | SI | SI | SI | SI (planta) | Editor | `PushBackCut(EntradaSalida, A)` |
| PushBack | Planta | `Whole` | SI | SI | SI | SI (frontal o lateral) | — | `Whole` |
| Cantilever | Frontal | `Whole` | SI | SI | SI | SI (planta; solo con Cantilever) | — | `Whole` |
| Cantilever | Lateral | `Station(s)` | SI | SI | SI | SI (planta; solo con Cantilever) | Editor | `Station(0)` |
| Cantilever | Planta | `Whole` (visibilidad del diseno, PR-2) | SI | SI | SI | SI (frontal o lateral; solo con Cantilever) | — | `Whole` |
| Selective (Cabecera) | Lateral | `Whole` | SI | SI | SI | SI (planta) | — | `Whole` |
| Selective (Cabecera) | Planta | `Whole` | **SI (nuevo)** | SI | SI | SI (lateral) | — | `Whole` |
| Cama | Lateral | `Whole` (`View` nulo) | SI (unica) | **NO** | **NO** | **NO** | — | — |
| Larguero | — | sin vista AutoCAD | NO | NO | NO | NO | — | — |
| Drive-In | — | no existe | — | — | — | — | — | — |

**La Cama no admite hermanas:** `EditCama` redibuja solo la definicion elegida (`P/RackCamaCommands.cs:226-237`); una segunda
definicion con el mismo `RackId` quedaria desactualizada. **No soportadas como variantes nuevas:** lateral entero del
Dinamico (`Section = -1`, legado que la edicion redibuja como poste 0, `P/RackDinamicoCommands.cs:236-239`) y laterales de
Push Back con `Section < 0` (`P/RackPushBackCommands.cs:444-447`).

## 6. Ciclo de vida del `RackId` (D-05)

### 6.1 Autoridades existentes

| Autoridad | Cuando acuna | Evidencia |
|---|---|---|
| `RackEditorIdentity.EnsureId` (Selectivo, Dinamico, Push Back, Cantilever y Cama tienen sesion) | en `RequestInsert`/`RequestUpdate` | `U/Editor/RackEditorSession.cs:126`; `U/Editor/RackEditorIdentity.cs:45-53`; Cama: `U/Systems/FlowBed/RackFlowBedWindow.xaml.cs:36-37`, `:276-278` |
| Guardar en biblioteca (Cantilever, Push Back) | tambien en `SaveToLibrary` | `U/Systems/Cantilever/RackCantileverWindow.xaml.cs:1363`; `U/Systems/PushBack/RackPushBackSystemWindow.xaml.cs:3404` |
| Guardar en biblioteca (Selectivo) | no acuna: id desechable | `U/Systems/Selective/RackSelectiveWindow.xaml.cs:3279-3281` |
| Comandos sin sesion | en cada llamada de dibujo del comando: `RACKCABECERA`/`QUICKCABECERA` dentro de `DrawAndPlace`, `QUICKCAMA` y la demo `RACKSISTEMADINAMICO` | `P/RackCabeceraCommands.cs:164-176`; `P/RackCamaCommands.cs:102`; `P/RackDinamicoCommands.cs:73` |
| Edicion con `Id` en blanco | Selectivo, Dinamico, Push Back y Cantilever **curan** con el id de la sesion; Cabecera acuna uno nuevo | `P/RackSelectivoCommands.cs:119`; `P/RackDinamicoCommands.cs:174`; `P/RackPushBackCommands.cs:179`; `P/RackCantileverCommands.cs:230`; `P/RackCabeceraCommands.cs:233` (H-08) |
| Id interior del Cantilever | al construir `CantileverLineDesign` | `D/Systems/Cantilever/CantileverLineDesign.cs:333`; `LoadNew` (`U/Systems/Cantilever/RackCantileverWindow.xaml.cs:199-210`) |

### 6.2 Opciones

| | A. Al abrir el editor | **B. Al aceptar la intencion de insertar** | C. Al primer placement |
|---|---|---|---|
| Cancelacion | GUID sin uso en toda sesion cerrada | nada escrito; el GUID muere con la sesion | nada que acunar |
| ID18 | lo comparten | **lo comparten: se acuna una vez al aceptar el lote** | la primera colocacion acuna y las demas recomponen su payload |
| Biblioteca | una plantilla abierta como nueva tendria GUID | sin id hasta la intencion | igual que B |
| Rack sin vistas | «rack» con GUID y sin bloques | no existe hasta la intencion | no existe hasta el placement |
| Compatibilidad | cambia la sesion y sus pruebas | **vigente** | el Plugin inventaria identidad |

**Decision propuesta: B.** Matices: `SaveToLibrary` de Cantilever y Push Back acuna antes, sin efecto en el DWG (OM-1). El
`Id` interior de un Cantilever **nuevo** se alinea con el `RackId` en el prerrequisito **PR-3** (§17.4), antes de la
foundation; los racks existentes conservan su `Id` interior (OM-11).

### 6.3 Reglas normativas

- **RID-1.** Un rack nuevo recibe su `RackId` **una vez**, al aceptar la primera intencion de insertar (una vista o un lote),
  antes de preparar. En editores con sesion lo acuna la sesion; en comandos sin sesion, **el comando lo acuna una vez por
  intencion aceptada y lo pasa a todas las vistas** (hoy `DrawAndPlace` de la cabecera acuna por llamada).
- **RID-2.** Toda vista preparada en esa aceptacion lleva ese `RackId`; toda ruta de entrada reenvia la lista completa de
  vistas (§11.2).
- **RID-3.** Una vista hermana lleva el `RackId` del sobre elegido o, en ID19, el de su grupo.
- **RID-4.** En `RACKEDITAR`, un sobre con `Id` en blanco se **cura como hoy** (id de la sesion) y las vistas nuevas llevan
  ese id. En ID19, sin editor, un sobre sin `Id` no puede agruparse con hermanas: fallo cerrado con remedio `RACKEDITAR`.
- **RID-5.** Cancelar antes del primer placement no deja rastro: no hay registro logico fuera de los bloques.

## 7. `RackViewKind`, variante y codec (D-02, D-03)

### 7.1 Una sola taxonomia de tipo de vista (A-4)

| Candidato | Hoy | Veredicto |
|---|---|---|
| `DimensionViewKind` (`A/Systems/Shared/DimensionViewPolicy.cs:11-16`) | «el TIPO de vista»; `Section` no crea tipo (`:6-9`); la politica vive aparte | Neutral por contenido → **renombrar y mover** a `A/Views/RackViewKind.cs` |
| Tokens `RackEmbedDocument.ViewFrontal/Lateral/Planta` (`A/Persistence/RackEmbedDocument.cs:28-30`) | persistencia | Siguen en el sobre; el codec los traduce |
| `CantileverViewKind` (`A/Systems/Cantilever/CantileverViewPlanBuilder.cs:12-37`) | incluye `AdapterSection`, que no es vista de la linea | Tipo de plano de geometria; **un** mapeo en la preparacion del Cantilever (OM-2) |
| `DimensionViewVisibility` (Domain) | politica persistida de I-50 | Sin cambio |

**D-02.** Renombre mecanico sin cambio de comportamiento: en `CURRENT_BASE` nombran `DimensionViewKind` seis archivos de
produccion (`DimensionViewPolicy.cs`, `SelectiveDimensions.cs`, `DynamicViewDecorations.cs` y las ventanas Dinamico, Push Back
y Selectivo) y seis de pruebas. ADR-0035 no lo nombra.

### 7.2 Variante tipada (A-1)

Cerrada y con igualdad por valor; **nunca** un indice de UI: `Post(p)` es el `PostIndex` del corte, `Fondo(k)` el indice de
fondo, `Station(s)` el de estacion. Las ventanas la construyen desde objetos del modelo, nunca desde `SelectedIndex` (H-01).
Las variantes son base 0; etiquetas y mensajes siguen en base 1 como los prompts vigentes (`P/RackSelectivoCommands.cs:476-491`,
`:546-561`).

### 7.3 Codec unico con disposicion (X-2)

| Disposicion | Significado | `RACKEDITAR` (sin cambio en V1) | I-52 (V10 §3.7) | ID19 (vista fuente) |
|---|---|---|---|---|
| `Canonical` | lo que produccion escribe | lee | acepta | acepta |
| `Canonicalizable` | no canonico con un unico significado (legado documentado, vista de significado unico) | lee y reescribe | acepta y canoniza | acepta |
| `Coerced` | produccion lo coerciona sin legado documentado | lee coercionado | falla cerrado | acepta: solo usa el tipo fuente |
| `Invalid` | produccion lo rechaza | aborta | falla cerrado | falla cerrado |

Lecturas vigentes que el codec reproduce (verificadas):

| Kind | Lectura de produccion | Disposicion indicativa |
|---|---|---|
| `selective` | todo lo que no es lateral ni planta es frontal, **incluido un `View` desconocido** (`P/RackSelectivoCommands.cs:126`); frontal con `Section < 0` → fondo 0 (`:168`; legado −1 en `:155`) | `View` vacio con −1: `Canonicalizable`; `View` desconocido o vacio con `Section ≥ 0`: `Coerced` |
| `dynamic` | planta y frontal por token; **todo lo demas es lateral** (`P/RackDinamicoCommands.cs:209-239`); frontal con `Section ≠ 1` → salida (`:392-393`); lateral sin `Section ≥ 0` → poste 0 (`:236-239`) | legado sin `View`: `Canonicalizable`; `View` desconocido: `Coerced` |
| `pushback` | descriptor invalido → aborta (`P/RackPushBackCommands.cs:424-450`) | `Invalid` |
| `cantilever` | descriptor invalido → aborta (`P/RackCantileverCommands.cs:525-540`) | `Invalid` |
| `cabecera` | no-planta = lateral (`P/RackCabeceraCommands.cs:239-240`); escritura con `View` vacio → lateral (`:197`) | `View` vacio: `Canonicalizable`; desconocido: `Coerced` |
| `cama` | cualquier `View` → lateral | `Canonical` |

**Otros lectores de `View`/`Section`** que V1 **no migra** y G3 caracteriza: `P/ProjectVariableMutationExecutor.cs:181-186`;
`A/Persistence/RackListBuilder.cs:117-118` (`View` vacio → lateral en cualquier kind, F-04); `P/RackBlockFinder.cs:23-24`
(frontal primero); `P/RackLayoutCommands.cs:71` (exige planta). La tabla exacta de disposiciones la fija G3 con **una sola**
caracterizacion compartida con la CT-04 de I-52, que incluye al ejecutor de variables (X-2).

## 8. Authored, effective y metadatos de una vista nueva

| Campo | Rack nuevo (ID17, lote nuevo) | Hermana por editor (ID17 despues, ID18 en edicion) | ID19 (sin editor) |
|---|---|---|---|
| `RackId` | acunado una vez (RID-1) | del sobre elegido; curado si estaba en blanco (RID-4) | del grupo |
| `Name` | del editor | del editor (sincronizado, legacy) | nombre del grupo |
| `Kind` | constante | igual | igual; busqueda **sensible a mayusculas** como `RACKEDITAR` (`P/RackMenuCommands.cs:141`) |
| `View`/`Section` | codec | codec | codec (variante canonica) |
| `Design`/authored | Selectivo: `SelectivePalletDesignDocument.From(design, id, name)`; demas: diseno del editor | Selectivo: authored reconciliado (`LinkedPropertyReconciler`) serializado una vez; demas: diseno del editor | **verbatim** del sobre fuente, con autoridad authored unica (§9) |
| `CustomProperties` | ausentes | copiadas del sobre elegido (legacy) | copiadas del sobre fuente del grupo |
| `ExtensionData`, `SchemaVersion` | nuevos | del sobre elegido (legacy, OM-14) | del sobre fuente |
| `DimensionViews` | en el diseno | en el diseno | en el diseno |
| `ProjectVariableReference` | en el authored | verbatim | verbatim; efectivo con el registro leido una vez |
| Expresiones (I-49) | verbatim | verbatim | verbatim; resolucion fallida → nada escrito |
| Proyecto interior (I-11) | biblioteca o nulo | el del bloque elegido | el del sobre fuente |
| `Id` interior | Selectivo alineado; Cantilever alineado (PR-3) | persistido, sin tocar | persistido, sin tocar |

**Regla de oro (P11).** El JSON authored de una vista nueva es el mismo string que portan sus hermanas tras la operacion.
**`TGrd02`:** el primer argumento de toda llamada a `Compose` fuera del compositor debe ser un **parametro
`RackEmbedDocument` del miembro que llama** (o `view.Embed` en el ejecutor de variables), nunca `null`
(`T/CustomPropertiesEnvelopeGuardTests.cs:85-105`); la preparacion respeta ese contrato y el censo se reapunta con motivo.

## 9. Autoridad entre hermanas (D-07; OQ-5 y OQ-7 cerradas)

**Lo que OQ-5 exige:** las hermanas conservan **autoridad authored equivalente**. **Lo que OQ-7 fija:** un rack existente
puede recibir **cualquier** hermana soportada. I-55 no debe crear divergencia nueva.

| Opcion | Descripcion | Crea divergencia nueva | Choca con | Veredicto |
|---|---|---|---|---|
| **A. Propagar solo sobre authored equivalente; copiar metadatos del sobre fuente** | Editor: el flujo vigente ya unifica el authored (reconciliador del Selectivo; redibujo de todas las hermanas con el mismo diseno) y D-15 impide insertar si un redibujo falla. ID19: autoridad authored unica sobre **todas** las hermanas del barrido; si no, fallo con remedio `RACKEDITAR`. `CustomProperties` copiadas del sobre fuente | **No**: el authored nuevo es el unificado; la coleccion nueva es la de un miembro existente | — | **ELEGIDA** |
| B. Elegir una hermana canonica | Tomar una ganadora | No, pero decide por el usuario | ADR-0039; I-47/I-48 | Rechazada |
| C. Reconciliar antes de crear | Unificar desde el comando | No, pero escribe hermanas no tocadas | Duplica `RACKEDITAR` y `RACKPROPIEDADES` | Rechazada |
| D. Ademas, exigir `CustomProperties` unica | Gate estricto con la autoridad de I-54 | No evita ningun valor nuevo | **OQ-7** (un payload ilegible en cualquier parte del dibujo da `IndeterminateMembership` a todo rack, `A/CustomProperties/RackCustomPropertiesAuthority.cs:249-264`; un sobre sin `Id` da `NoIdentity`, `:238-247`, y dejaria de curarse); frontera del borde de I-54 (`TGrd04`) | Rechazada |

**Por que copiar la coleccion no crea divergencia:** `Compose` hereda la coleccion del sobre fuente
(`A/Persistence/RackEmbedComposer.cs:57`), que ya pertenece a un miembro del rack; un rack `Single` sigue `Single`, y uno ya
`Divergent` no gana una clase nueva. Su remedio sigue siendo `RACKPROPIEDADES` (sin aviso nuevo en V1, OM-16).

**ID19.** Miembros = todas las hermanas del `RackId` que encuentra el SNAPSHOT, el mismo conjunto que usan el BOM y el
ejecutor de variables. Selectivo: `SelectiveAuthoredAuthority.Resolve(rackId, siblings)` = `Single`
(`A/ProjectVariables/SelectiveAuthoredAuthority.cs:128-157`), con la regla vigente de no propagar desde una hermana elegida
(«reconcilia el rack primero», `:154-156`). Demas kinds: el comparador unico por kind de X-3, que excluye la metadata
interior que `RACKEDITAR` preserva por hermana (`P/RackCommandSupport.cs:40-67`). **No** se lee la autoridad de I-54: ni
`TGrd04`, ni `TGrd05`, ni el contrato del borde ni ADR-0039 cambian.

## 10. ID17 — FIRST-VIEW FREEDOM por sistema

**Flujo:** diseno nuevo → tipo de vista → variante si aplica → RID-1 → preparar → materializar → colocar → cancelacion (§14).

| Sistema | Hoy | Cambio de ID17 | Nota |
|---|---|---|---|
| Selectivo | solo Frontal (`U/Systems/Selective/RackSelectiveWindow.xaml.cs:2560-2570`, `:274-292`) | Lateral-first y **Planta-first** | La edicion funciona sin frontal (`P/RackSelectivoCommands.cs:125-136`) |
| Dinamico | solo Lateral (`U/Systems/Dynamic/RackDynamicSystemWindow.xaml.cs:3394-3403`, `:189-194`) | Frontal salida-first, entrada-first y Planta-first | `DrawDynamicView` dibuja las tres familias de vista (`P/RackDinamicoCommands.cs:102-132`) |
| Push Back | libre | sin cambio de puerta; lateral por poste real (PR-1) | — |
| Cantilever | libre | sin cambio de puerta; planta con visibilidad (PR-2); `Id` interior (PR-3) | — |
| Cabecera | solo Lateral: la ventana rechaza planta si es nueva (`U/RackFrames/RackFrameConfiguratorWindow.xaml.cs:85-93`, `:265-269`); `HeaderInsertionRequest` no lleva vista (`U/Editor/RackInsertionRequest.cs:36-53`); `RACKCABECERA` dibuja lateral (`P/RackCabeceraCommands.cs:37-41`) | **Planta-first**: la peticion gana direccion y el comando la respeta | builder y edicion ya soportan planta (`P/RackCabeceraCommands.cs:278-306`) |
| Cama, Larguero | una vista / sin vista | sin cambio | — |

**Variante en vista unica.** Los prompts vigentes (fondo del Selectivo; poste del Selectivo y del Dinamico) se conservan en
V1 como resolucion legacy que produce la variante tipada antes de preparar (OM-4).

**Racks parciales (OQ-7).** Un rack de solo Planta —p. ej. una celda independiente de `RACKLAYOUT`
(`P/RackLayoutCommands.cs:231-244`)— recibe por `RACKEDITAR` cualquier hermana soportada con el mismo `RackId`. Prueba
contractual futura `RackViewPartialRackContractTests`.

## 11. ID18 — MULTI-VIEW QUEUE / BATCH

### 11.1 Editor

- Cada editor con vistas gana **«Insertar varias vistas…»**: dialogo del **arquetipo C de ADR-0029** con las direcciones
  soportadas y disponibles (casillas por tipo y variante).
- El dialogo se presenta por un **presentador fuera del archivo de la ventana**, sustituible en pruebas: las guardas de
  modales directos de las ventanas cuentan `ShowDialog(` en su propio archivo
  (`TU/DynamicHeaderBatchSeamGuardTests.cs:26-31`, `TU/SelectiveHeaderBatchSeamTests.cs:177-181`) y no deben crecer.
- Una aceptacion → un recalculo → RID-1 → **una** peticion → la ventana cierra. Los botones de vista unica se conservan.
- Orden canonico Frontal → Lateral → Planta y variante ascendente (OD-5).

### 11.2 Peticion y rutas de entrada

Cada `RackInsertionRequest` con vistas gana `IReadOnlyList<RackViewAddress> Views`; `View`/`Section` historicos valen la
primera direccion. `RackEditorSession` gana `RequestInsertViews`. **Rutas que hoy reconstruyen la peticion y perderian
`Views` sin error** (G11/G12 las cablean y una guarda lo fija): `U/Editor/EditorModules.cs:53-56` (Selectivo) y `:90-95`
(Dinamico) reconstruyen desde `window.InsertView`/`InsertSection`; `RACKSELECTIVO` dibuja desde `window.InsertView`
(`P/RackSelectivoCommands.cs:34-38`); `RACKCABECERA` dibuja desde `window.Configuration` (`P/RackCabeceraCommands.cs:37-41`)
y acuna en cada `DrawAndPlace` (`:173`). Push Back ya devuelve la peticion de la ventana tal cual (`U/Editor/EditorModules.cs:98-100`).

### 11.3 Modelo de transaccion (D-06; OQ-6; A-3)

| Opcion | Veredicto |
|---|---|
| A. Una transaccion para todo | **Rechazada**: abierta durante varios jigs; un Esc desharia lo colocado |
| B. Preparar cada vista justo antes de colocarla | **Rechazada**: un fallo de preparacion aparece tras colocar las anteriores |
| **C. PREPARE completo + commit por placement** | **ELEGIDA** |

Evidencia: la transaccion del jig vive durante su arrastre y confirma al terminar (`P/Drawing/BlockPlacement.cs:174-201`); la
definicion cancelada se limpia (`:37-42`, `:64-76`); las vistas futuras no existen hasta su turno.

### 11.4 Maquina de estados

```text
PREPARE ─(falla alguna)→ ABORTED_BEFORE_WRITE  "No se inserto ninguna vista: <motivo>."
   │
   ▼ (solo rack existente)
REDRAW ─(algun redibujo falla)→ REDRAW_FAILED  "No se insertaron vistas: no se pudo actualizar <vista>. Las demas
   │                                            vistas existentes si se actualizaron."            (D-15)
   ▼
READY ─→ PLACING(i) ─(punto)→ COMMITTED(i) ─→ i < n: PLACING(i+1) · i = n: COMPLETED
             ├─(Esc)──────→ CANCELLED(i) ─→ COMPLETED_PARTIALLY  "Insertadas: … Cancelada: … No se insertaron: …"
             └─(excepcion)─→ FAILED(i)    ─→ COMPLETED_PARTIALLY o FAILED_BEFORE_ANY
```

- **Esc detiene la cola** (OD-4); las colocadas permanecen; la cancelada se limpia.
- Cada prompt nombra la vista y su posicion («Punto de insercion de Lateral poste 3 (2 de 3):»). `PlaceAndReport` **no** acepta
  prompt hoy (solo `PlaceDefinition`, `P/Drawing/BlockPlacement.cs:26`, `:64`; el texto por defecto es el de la cabecera,
  `:214`): G9 anade el parametro conservando el texto por defecto.
- **`Regen`:** el lote **no anade** regeneraciones al flujo legacy de una vista: rack existente, las del redibujo legacy (en
  Cantilever, una tras el redibujo, `P/RackCantileverCommands.cs:355-358`); insercion Cantilever, **una al final** en lugar de
  una por vista (`:169`); resto, ninguna.
- Bloques de biblioteca faltantes: se reportan como hoy (`P/Drawing/BlockPlacement.cs:98-118`).

### 11.5 Rack existente (`RACKEDITAR`)

```text
preflights vigentes (guarda de unidades antes del primer redibujo) → variantes → PREPARE ALL de las vistas nuevas
   → REDIBUJO legacy (commit por vista, obsoletas) → si algun redibujo fallo: REDRAW_FAILED → Regen → cola de placements
```

El authored reconciliado y el sistema existen antes del redibujo (`P/RackSelectivoCommands.cs:144-158`). **D-15:** si un
redibujo falla, esa hermana conservaria el authored anterior mientras las vistas nuevas llevarian el nuevo —divergencia
creada por la operacion—; por eso no se inserta ninguna vista. Aplica tambien al lote de uno (cambio acotado frente al
legacy, que inserta igual). Cancelar un jig tras el redibujo conserva el redibujo (ADR-0010). OM-3: atomicidad del redibujo.

## 12. ID19 — MULTI-RACK PROJECTION

### 12.1 Flujo

```text
ACQUIRE     seleccion en Model Space + clase de vista pedida
SNAPSHOT    una lectura: referencias (transformacion completa, espacio, MINSERT, escala, normal) + definiciones + barrido
CLASSIFY    nucleo neutral (X-1) + reglas de §12.3
GROUP       por RackId (en blanco → FALLO, RID-4); DEDUPE conservando todas las referencias
VALIDATE    un solo tipo fuente; par fuente→destino soportado; una sola familia de marco; misma orientacion (§12.4);
            miembro soportado (OD-3); salida no bloqueada (§12.5)
AUTHORITY   authored unico por grupo sobre todas sus hermanas (§9); registro de variables leido UNA vez
FRAMES      marcos fuente y destino por grupo (§12.4)
PICK        punto base y destino (UCS → WCS)                                   ← despues de validar, antes de importar
PREPARE     resolver cada RackId UNA vez; preparar; importar la union de bloques; re-verificar faltantes (D-13)
TRANSFORM   posiciones por T_common (§12.4) + aviso de superposicion
MATERIALIZE UNA transaccion: por grupo, UNA definicion + sobre y una referencia por referencia fuente; commit
```

Las validaciones ocurren **antes** de pedir puntos (el usuario conoce el error sin gestos inutiles) y la importacion
**despues** (Esc en un punto no deja bloques importados; como I-52 V10 §9.1).

### 12.2 Seleccion: nucleo neutral de I-51 (X-1)

`RackDuplicationPlan.Build` clasifica, agrupa por `RackId`/definicion y deduplica conservando las referencias
(`A/Persistence/RackDuplicationPlan.cs:225-583`) con semantica de duplicacion. I-52 V9/V10 §5.1 propone extraer su nucleo
neutral con `RackDuplicationPlan` como fachada identica. **D-08a:** ID19 consume ese nucleo; no hay segundo planificador. El
nucleo expone **hechos neutrales** (MINSERT, bloque anonimo, espacio, origen, escala, normal); **las politicas de fallo son
de cada llamador** (I-52: sus ST-*; I-55: §12.3). El SNAPSHOT del Plugin es propio de ID19.

### 12.3 Grupos y clasificacion (D-08)

**Hecho que manda el diseno.** `RACKLAYOUT` coloca celdas **enlazadas** —referencias de una definicion, mismo `RackId`,
«edit one = edit all; the BOM counts them as copies»— o **independientes**, con su GUID (`P/RackLayoutCommands.cs:29-30`,
`:226-257`); `COPY` produce lo primero.

| Regla | Contenido |
|---|---|
| **D-08b** | Dentro de un grupo `RackId`, las referencias deben ser **de la misma definicion** (colocaciones del rack) → **una** definicion nueva y **una** referencia por referencia seleccionada. Hermanas distintas del mismo `RackId` → fallo que nombra el rack |
| **D-08c** | Un solo tipo de vista fuente (codec, disposicion ≠ `Invalid`) |
| **D-08d** | Miembro cuyo sistema no soporta el par pedido → fallo que lista los miembros (OD-3) |
| **D-08e** | Proyectar una clase que el rack ya tiene es legal (OQ-4) |
| **D-08f** | Fallo cerrado: `MInsertBlock`; referencia dinamica, anonima o anotativa con datos de RackCad (el barrido omite las definiciones anonimas, `P/RackBlockFinder.cs:66`; misma politica que la ST-17 de I-52); escala de valor absoluto no uniforme o **reflejada** (exactamente una de `sx`, `sy` negativa); normal distinta de Z universal; UCS cuyo Z no es paralelo al universal. Un `Origin` de definicion distinto de cero **no** falla: el ancla usa la transformacion completa (§12.4). Aviso e ignorar, como I-51: no-bloque, fuera de Model Space, sin datos |
| **D-08g** | Kind resuelto con `KindHandlerDispatch.TryResolve` (sensible a mayusculas, como `RACKEDITAR`), nunca con la variante que ignora mayusculas de `RACKLAYOUT` (`P/RackLayoutCommands.cs:64`) |

### 12.4 Layout relativo: proyeccion ortografica por familia de marco (D-16; OD-6, OD-7)

**Por que no basta una traslacion (hallazgo bloqueante de la revision).** La planta y las elevaciones de un rack usan ejes
distintos: en los sistemas de rack la planta dibuja **X = fondo, Y = frente** (`A/Systems/Selective/SelectivePlantaBuilder.cs:16-19`;
Dinamico `A/Systems/Dynamic/DynamicSystemPlantaBuilder.cs:15-17`) mientras la frontal corre los frentes en X, y `RACKLAYOUT`
pone filas en X y columnas en Y (`A/Layout/WarehouseGridPlanner.cs:72-79`). Trasladar las posiciones de las plantas a las
frontales convierte la separacion entre columnas en separacion vertical: las frontales se apilan y se superponen. El
contrato «preservar el layout relativo con UNA transformacion comun» exige mapear ejes fisicos, no coordenadas de dibujo.

**Marco fisico del rack:** R = corrida (frentes, estaciones), D = profundidad, H = altura. **Descriptores de marco local**
(hipotesis derivadas del codigo; G3 las fija con pruebas de caracterizacion sobre la salida de los builders):

| Familia | Vista | Eje local +X | Eje local +Y | Origen fisico en el origen local | Evidencia |
|---|---|---|---|---|---|
| **Rack** (Selectivo, Dinamico, Push Back, Cabecera) | Planta | +D | +R | D: cara exterior delantera (Dinamico: extremo de salida; Push Back: extremo bajo o cara del lado A); R: eje del poste 0 | `SelectivePlantaBuilder.cs:16-19`; `DynamicSystemPlantaBuilder.cs:15-17`; `A/RackFrames/PlantaHeaderLayoutBuilder.cs:95-103` |
| Rack | Frontal (toda variante) | +R | +H | R: eje del poste 0 de la cuadricula de la vista; H: base del poste | `A/Systems/Selective/SelectiveFrontalBuilder.cs:50` |
| Rack | Lateral (todo corte) | +D | +H | D: cara exterior delantera **del primer fondo que alcanza el poste** (Selectivo) o X = 0 del sistema (Dinamico, Push Back); H: base del poste | `A/Systems/Selective/SelectiveLateralBuilder.cs:82-97`; `A/RackFrames/LateralHeaderLayoutBuilder.cs:56-59` |
| **Cantilever** | Planta | −R | +D | R: eje de la columna de la estacion 0; D: cara de conexion | camaras `A/Systems/Cantilever/CantileverViewPlanBuilder.cs:209-219` con `right = up × forward` (`A/Geometry/Spatial3D.cs:186-197`) |
| Cantilever | Frontal | −R | +H | R: eje de la columna de la estacion 0; H: suelo | idem |
| Cantilever | Lateral | −D | +H | D: cara de conexion; H: suelo | idem |

**Desplazamientos calculables desde el sistema resuelto** (puros, en `RackViewFrame`): el `anchorOffset` de un corte lateral
del Selectivo cuyo poste no alcanza el fondo 0 (`SelectiveLateralBuilder.cs:97`), y la posible diferencia entre la cuadricula
propia de la frontal de un fondo (`SelectivePostGeometry.Compute(system, catalog)`, `SelectiveFrontalBuilder.cs:50`) y la
cuadricula maestra que usan planta y lateral (`A/Systems/Selective/SelectiveDepthLayout.cs:52-55`). La materializacion no
mueve el origen: las definiciones nacen con `Origin = 0` (`P/Drawing/LateralHeaderDrawer.cs:243`).

**Pares soportados en V1:**

| Fuente | Destino | Eje conservado | Eje comun (linea del destino) | Eje descartado |
|---|---|---|---|---|
| Planta | Frontal | R | H (base comun) | D → aviso si hay mas de un valor |
| Planta | Lateral | D | H (base comun) | R → aviso si hay mas de un valor |
| Frontal | Planta | R | D (linea comun) | H |
| Lateral | Planta | D | R (linea comun) | H |
| Frontal ↔ Lateral, o misma clase | — | sin eje horizontal comun, o sin proyeccion | — | **fallo** (OM-15) |

**Formalizacion.** Para cada referencia seleccionada `r` (grupo `g`, sistema `s`, familia `F`):

```text
M_r      = transformacion completa de la referencia (Position, Rotation, ScaleFactors, Normal, Origin de la definicion)
a_s(g)   = coordenadas locales del origen fisico en la vista fuente (descriptor + desplazamientos del sistema resuelto)
O_r      = M_r · a_s(g)                                     origen fisico en WCS
u_r      = direccion WCS del eje conservado = M_r · (vector local del eje conservado segun el descriptor), normalizada
VALIDATE : todas las u_r iguales a u (tolerancia de modelo); una sola familia F        ← si no, fallo (OM-12, OM-13)
B, T     = UCS→WCS(punto base), UCS→WCS(punto destino)
l_r      = (O_r − B) · u                                    coordenada del rack sobre el eje conservado
e_r      = (O_r − B) · (eje descartado)                     solo para el aviso de superposicion
w        = direccion WCS del eje conservado en la vista destino con rotacion 0 (descriptor de destino)
a_t(g)   = coordenadas locales del origen fisico en la vista destino (variante canonica)
P_r      = T + l_r · w − a_t(g)                             posicion de la referencia nueva (rotacion 0, escala 1, normal Z)
T_common : l ↦ T + l · w                                    UNA transformacion comun, igual para todos los racks
```

**Invariante de layout:** `(P_r + a_t) − (P_q + a_t) = (l_r − l_q) · w` para todo par: los origenes fisicos conservan sus
distancias sobre el eje conservado y quedan alineados sobre el eje comun. **Aviso:** si los `e_r` toman mas de un valor, las
vistas de filas distintas se superponen en el destino; la proyeccion es fiel (una elevacion ortografica superpone lo que
esta detras) y el mensaje sugiere proyectar una fila a la vez. **Orientacion (OD-6):** las vistas nuevas se dibujan con su
orientacion natural (rotacion 0); por eso todas las fuentes deben compartir orientacion. **Semantica (OD-7):** esta
proyeccion es la opcion recomendada; la traslacion literal queda como alternativa del Owner.

### 12.5 Salidas bloqueadas y resolucion sin editor (D-17)

ID19 resuelve cada `RackId` **sin editor**, por el mismo camino que el BOM (Selectivo: resolucion efectiva + geometria,
`P/KindHandlers/SelectiveKindHandler.cs:58-68`; Dinamico: `DynamicRackSystemResolver` o sistema legado,
`P/KindHandlers/DynamicKindHandler.cs:40-42`; Push Back: `PushBackResolver`). **VALIDATE** aplica los diagnosticos que
bloquean al editor (`RackBomOutputGate`, `A/Bom/RackBomOutputGate.cs:46`; ventana Push Back
`U/Systems/PushBack/RackPushBackSystemWindow.xaml.cs:3301`; `RACKBOMTOTAL` aborta con ellos,
`P/RackInventarioCommands.BomTotal.cs:174-188`): un rack bloqueado **falla** la proyeccion. G3 caracteriza, por kind, que la
resolucion sin editor produce el mismo sistema que el editor (el Dinamico puede reconstruir la secuencia de modulos antes de construir,
`U/Systems/Dynamic/RackDynamicSystemWindow.xaml.cs:506-537`); los casos que difieran quedan fuera de ID19 con fallo cerrado.

### 12.6 Presentacion (D-11, OD-8)

Las referencias nuevas reciben la presentacion de creacion vigente de toda insercion de RackCad (ningun productor asigna capa,
color ni estilo a la referencia; el mecanismo efectivo lo caracteriza I-52, su CT-50) y **no** copian la de la fuente, porque no
son copias. I-52 si la copia (su ST-13) porque su semantica es de copia.

### 12.7 Materializacion del grupo

- **PREPARE ALL** tras los puntos: resolucion, preparacion, importacion de la **union** de nombres en una llamada
  (`BlockLibraryImporter.EnsureBlocks`, `P/Drawing/BlockLibraryImporter.cs:37`) y re-verificacion en lectura de que toda pieza no
  anotativa tiene bloque; faltantes → **fallo antes de escribir** (D-13), como el E10 de I-52.
- **Una** transaccion: por grupo, definicion + sobre (`LateralHeaderDrawer.CreateSystemBlock(db, tr, plan, name)`,
  `P/Drawing/LateralHeaderDrawer.cs:28-29`, o `CantileverViewMaterializer`) y sus referencias en `P_r`. Una excepcion **no
  confirma nada**.
- **Sin `Regen` ni purga:** crear no deja anidadas huerfanas ni representaciones obsoletas (precedentes: I-51 INV-14; I-52 V10
  §8.6).

## 13. Invariantes de conteo y autoridad

| ID | Invariante | Por que | Prueba futura |
|---|---|---|---|
| INV-BOM-1 | Anadir una vista hermana **no** anade un rack | `RACKBOMTOTAL` agrupa por `Id` | `RackViewCountInvariantTests.AddingASibling_DoesNotAddARack` |
| INV-BOM-2 | Lote F + L + P de un rack nuevo = **1** rack | un `RackId`; copias = max(1, 1, 1) | `...BatchOfThreeViews_IsOneRack` |
| INV-BOM-3 | ID19 sobre N `RackId` = N racks; copias por rack sin cambio | sin identidad nueva; la definicion nueva lleva k ≤ max referencias | `...ProjectionOverNRacks_KeepsRacksAndCopies` |
| INV-LIST-1 | `ViewCount` puede cambiar | `RackListBuilder` cuenta vistas | `...ListViewCount_MayGrow` |
| INV-LIST-2 | Las copias no cambian por vistas nuevas, **salvo** un rack sin referencias colocadas (copias 0) que pasa a 1 | maximo de referencias directas | `...CopyCount_UnchangedByNewViews` |
| INV-LIST-3 | `RACKLISTA` muestra vistas nuevas sin multiplicar racks | agrupacion por `Id` | `...ListRows_OnePerRackId` |
| INV-AUTH-1 | I-55 nunca crea authored divergente entre hermanas | §9 opcion A; D-15 | `RackViewAuthoredEquivalenceTests` |

## 14. Cancelacion y errores (diseno)

| Operacion | Etapa | Evento | Que queda |
|---|---|---|---|
| ID17 | editor / variante | cerrar o Esc | nada |
| ID17 | jig | Esc | la definicion creada se borra (vigente) |
| ID17 | preparacion o importacion | fallo | nada del rack; bloques importados pueden quedar (OM-5) |
| ID17 | tras crear la definicion | excepcion | **se borra la definicion** en `PlaceAndReport` y en `PlaceDefinition`/Cantilever (D-10; hoy queda: `P/Drawing/BlockPlacement.cs:49-52`; `P/RackCantileverCommands.cs:151-157`, `:174-177`) |
| ID17/ID18 | materializacion | faltan bloques | se coloca y se reporta (vigente) |
| ID18 | primer / intermedio / ultimo placement | Esc | nada / anteriores colocadas / todas menos la ultima |
| ID18 | crear la definicion de la vista i | excepcion | anteriores colocadas; cola detenida |
| ID18 | preparar alguna vista | fallo | **nada**, ni redibujo |
| ID18 | redibujo de una vista existente | fallo | redibujos logrados permanecen; **ninguna vista nueva** (D-15) |
| ID19 | seleccion | no bloque / fuera de Model Space / sin datos | aviso; se ignora |
| ID19 | clasificacion | sobre ilegible, kind desconocido, MINSERT, espejo, escala no uniforme, normal o UCS no universal | **fallo**, nada escrito |
| ID19 | validacion | tipos fuente mezclados, par no soportado, familias mezcladas, orientaciones distintas, hermanas distintas de un `RackId`, `RackId` en blanco, miembro no soportado, salida bloqueada | **fallo**, nada escrito, sin pedir puntos |
| ID19 | autoridad | authored divergente o ilegible | **fallo** con remedio `RACKEDITAR` |
| ID19 | punto base o destino | Esc | **nada** (aun no se importo) |
| ID19 | preparacion | resolucion fallida | **fallo**; bloques importados de otros miembros pueden quedar (OM-5) |
| ID19 | re-verificacion | faltan bloques | **fallo** con la lista (D-13) |
| ID19 | materializar el miembro N | excepcion | **rollback**: nada de ningun miembro |

## 15. Transacciones (resumen normativo)

| Flujo | PREPARE | MUTATE | `Regen` |
|---|---|---|---|
| ID17 vista unica | fuera de la transaccion | definicion (commit) + jig (commit) o limpieza | como hoy por sistema |
| ID18 rack nuevo | todas las vistas antes de escribir | por vista: definicion + jig | ninguno; Cantilever uno al final |
| ID18 rack existente | todas antes del redibujo | redibujo legacy; si todo bien, cola | los del redibujo legacy; Cantilever uno mas al final |
| ID19 | todos los miembros, tras los puntos | **una** transaccion | ninguno |

Ninguna transaccion de escritura queda abierta mientras el usuario elige entre varios jigs.

## 16. Propiedad de recursos compartidos

| Recurso | Propietario | Veces |
|---|---|---|
| Catalogo | comando (`RackCatalogLoader.Load`) | 1 por comando |
| Catalogo de secciones | comando (`StructuralSectionCatalogAccess.TryLoad`) | 1 si hay Cantilever |
| Sistema resuelto | editor (ID17/ID18) o contexto de preparacion (ID19) | 1 por `RackId` |
| Authored serializado | contexto de preparacion | 1 por `RackId` |
| Registro de variables | comando | 1 |
| Barrido de sobres | ID19: el SNAPSHOT (`RackBlockFinder.ScanEnvelopes`), del que tambien sale la autoridad authored | **1** en ID19; en `RACKEDITAR` los barridos vigentes (eleccion y `FindRackBlocks`) no cambian |
| Importacion de bloques | colocacion | ID19: 1 con la union de nombres; ID17/ID18: por plan (vigente) |

## 17. Reconciliacion con iniciativas activas

### 17.1 I-49 (motor de expresiones; `bcbf570`)

Rebasada sobre `dad4e77`; G5 y G6 en `A/Expressions/*`, `A/Units/LengthUnits.cs`, `A/StructuralSections/StructuralSectionUnits.cs`
y pruebas; A2 y A2-R2 solo docs, incluido **ADR-0041 propuesto en su rama** (`8cefd59`), por lo que el ADR de I-55 es 0042.
**Archivos comunes con el plan de I-55: ninguno** (el indice `docs/adr/README.md` tendra un conflicto textual trivial al
integrar). El authored viaja verbatim y se serializa una vez;
I-55 no evalua expresiones por vista. **Archivo caliente:** `U/Systems/Selective/RackSelectiveWindow.xaml.cs`, que el G10 de I-49
preve tocar (fila de I-53S, `docs/ROADMAP.md:471`): la UI de I-55 en el Selectivo se serializa con ese gate.

### 17.2 I-52 (`RACKMIRROR`; Proposal V10 `636f7fd`, sin consenso)

I-52 registra el cruce como R-45 y pide llevar a Coordinador y Arquitecto toda autoridad comun que fije el G2 de I-55. Puntos
**materiales entre iniciativas**, con resolucion propuesta:

| # | Tema | I-52 V10 | I-55 V1 | Resolucion propuesta |
|---|---|---|---|---|
| **X-1** | Seleccion | nucleo neutral + fachada (§5.1); fallos por MINSERT, bloques dinamicos, `Origin ≠ 0`, escala negativa y UCS (§4.1, §4.2) | ID19 consume el nucleo (§12.2) | **Una** extraccion, en el gate de quien llegue primero, con la semantica observable **verde sobre codigo intacto y roja solo por teoria de mutacion** antes de extraer; el nucleo expone hechos neutrales y cada llamador fija su politica |
| **X-2** | Plan por vista y lectura de `View`/`Section` | `RackViewPlanAuthority` recibe el payload y resuelve (§8.1); sin laterales de Selectivo, Dinamico ni Push Back; decodificacion en `Mirror/` (§20.1) | los editores planifican **desde el sistema que ya entregaron** («Nothing is resolved twice», `P/RackSelectivoCommands.cs:421-424`); ID19 resuelve sin editor; necesita laterales | **Dos pasos**: resolucion (I-52 e ID19 desde el payload; editores, ninguna) y **un** paso comun «sistema resuelto → plan por vista» que cubre todas las direcciones soportadas; **un** codec con disposicion en ubicacion neutral (`Views/`); una sola caracterizacion de lecturas (CT-04 de I-52 + lectores de §7.3) |
| **X-3** | Comparador authored por kind | declarado por el reflector (§5.4), sobre las vistas seleccionadas | ID19 compara todas las hermanas del barrido (§9) | **Un** comparador por kind declarado por el **contrato del kind**; el conjunto de miembros es parametro del llamador |
| **X-4** | Materializacion | materializador con ST-13, ST-19, PRE-17, PRE-18 y read-set (§8.2, §8.7, §9) | ID19: definicion + sobre + referencia en una transaccion, sin equivalencia visual | **Un** primitivo «definicion + sobre + referencia en la transaccion del llamador»; ST-13/ST-19/PRE-17/PRE-18/read-set son politica de I-52 (equivalencia visual), no deberes del primitivo; ambos piden los puntos antes de importar |
| **X-5** | Archivos que I-52 declara STOP o NO TOCAR | §19: `RackDuplicationPlan.cs`, builders de §8.1, `CustomPropertiesExecutor.cs`; §20.3: editores WPF, `KindHandlers/*`, drawers, `BlockLibraryImporter.cs`, `RackCloner.cs` | I-55 toca editores WPF (G10, G12), `RackDuplicationPlan.cs` si extrae (G13) y renombra en `SelectiveDimensions.cs`/`DynamicViewDecorations.cs` (G7); **no** toca `CustomPropertiesExecutor.cs`, drawers, `BlockLibraryImporter.cs` ni `RackCloner.cs` | Antes de editar un archivo de esas listas, I-55 lo reporta a I-52 (su §15.3) y se secuencia |
| **X-6** | Lineas base de C-2 | C2-2a (Actualizar), C2-2b (Insertar), C2-4 (estado del editor) y guarda G-M9 (§8.5) | G9 re-enruta Insertar; G12 reordena la edicion y aplica D-15 | La segunda iniciativa en integrar re-establece C-2 sobre el arbol combinado; G9 de I-55 incluye fixtures de equivalencia de Insertar |

**Conflictos previsibles de integracion (no semanticos):** censo `[CommandMethod(` (`T/SelectiveEditorOpenTests.cs:547-553`,
fijo en 35) y censo por nombre `TGrd08`; ayuda (`U/RackCommandReference.cs`, `CustomPropertiesHelpCensusTests`); censo `TGrd02`.
La segunda en integrar re-basa. **Si la version congelada de I-52 contradice X-1..X-6, I-55 abre Proposal V2.**

### 17.3 I-53D (integrada en `dad4e77`)

La ventana del Dinamico es la de I-53D (re-auditada en G1.1). I-55 no toca el lote de cabeceras ni
`DynamicEditorDesignAssembler`. Su guarda `D34_GUARD_ElCensoDeModalesDirectosNoCrece_YLosGestosNuevosNoAbrenNinguno` cuenta un
`MessageBox` que **es** la puerta de primera vista: G10 la actualiza con motivo.

### 17.4 Prerrequisitos aislados (D-09; CD-07)

Van **antes de la primera produccion de la foundation**, con los tipos de hoy, cada uno con prueba vista fallando y validacion
del Owner cuando cambia el dibujo:

| # | Hallazgo | Evidencia | Por que bloquea | Correccion |
|---|---|---|---|---|
| **PR-1** | H-01 Push Back usa posicion de lista como poste | `U/Systems/PushBack/RackPushBackSystemWindow.xaml.cs:3036-3045`, `:3065` | La variante `Post(p)` no puede apoyarse en una fuente que produce otro poste | Seccion desde `corte.PostIndex` (entero de hoy) |
| **PR-2** | H-02 planta Cantilever del Plugin sin visibilidad | `P/RackCantileverCommands.cs:131`, `:312`; un `null` significa brazos y tensores **apagados** (`A/Systems/Cantilever/CantileverViewPlanBuilder.cs:293-295`; `D/Systems/Cantilever/CantileverLineDesign.cs:305-309`) | Hoy el Plugin **siempre** oculta brazos y tensores en planta; la vista previa respeta el diseno | Pasar la visibilidad; cambia las plantas cuyo diseno **muestra** brazos o tensores |
| **PR-3** | H-13 `Id` interior del Cantilever sin alinear | §6.1 | Doble identidad en toda linea nueva (P3) | Alinear al construir la peticion de un rack nuevo; racks existentes sin cambio |

H-03, H-04..H-12 siguen laterales; H-08 no bloquea (RID-4).

## 18. Alternativas de arquitectura

| | ALT-A: seguir por sistema con `string view + int section` | **ALT-B: foundation pequena + adaptadores** | ALT-C: framework generico | ALT-D: registro logico persistente |
|---|---|---|---|---|
| Beneficios | Minimo cambio | Frontera testeable; variante tipada; ID18 e ID19 reutilizan la preparacion; alineable con I-52 | Extensibilidad teorica | Autoridad logica fuera de los bloques |
| Coste | Bajo por gate, alto acumulado | Medio | Alto | Muy alto |
| Riesgo | Repite H-01; logica nueva en Plugin no testeable; divergiria de I-52 | Censos y guardas reapuntados; archivos calientes | Abstracciones sin segundo cliente | Formato nuevo en el NOD; sincronizacion ante COPY, WBLOCK, xref, UNDO; choca con ADR-0009 y ADR-0039 |
| Migracion | Ninguna | Ninguna de datos | Mucha de codigo | Todos los DWG |
| Veredicto | Rechazada | **Aceptada** | Rechazada | Rechazada |

## 19. Pruebas (resumen; matriz completa en el mapa §T)

I-45: comportamiento puro; el Plugin solo con guardas minimas. Toda seleccion filtrada demuestra que selecciono pruebas (AGENTS.md);
todas las pruebas usan el namespace plano `RackCad.Tests`/`RackCad.UI.Tests`, asi que se filtra por nombre de clase y el conteo se
registra en el gate (no en este documento).

| Clase | Ambito | Clases previstas |
|---|---|---|
| **T0 focal** | foundation pura | `RackViewKindRenameTests`, `RackViewVariantTests`, `RackViewEnvelopeCodecTests`, `RackViewSupportMatrixTests`, `RackViewFrameTests`, `RackViewPreparation*Tests`, `PreparedViewPayloadGoldenTests`, `RackIdLifecycleTests`, `RackViewBatchPlanTests`, `RackViewAuthoredEquivalenceTests`, `RackProjectionPlanTests`, `RackProjectionTransformTests`, `RackViewCountInvariantTests`, `RackViewPartialRackContractTests` |
| **T1 sistema/impacto** | consumidores y guardas | `*DimensionView*`; `CustomPropertiesEnvelopeGuardTests`; `CustomPropertiesCommandGuardTests`; `SelectiveEditorOpenTests`; `CantileverPluginSourceGuardTests`; `SelectiveAuthoredCarrierAdoptionTests`; `PushBackPluginSourceGuardTests`; `RackUnitsGuardSourceTests`; `RackDuplicationPlanTests`; UI: `RackInsertionRequestTests`, `RackEditorSessionTests`, `EditorModuleRegistryTests`, `DynamicShellMigrationTests`, `RichEditorBlockedActionTests`, `DynamicHeaderBatchSeamGuardTests`, `SelectiveHeaderBatchSeamTests`, `WindowCensusGuardTests`, `DialogWindowCharacterizationTests`, `CustomPropertiesHelpCensusTests` |
| **T2 candidato** | suites Core + UI completas; Debug UI + Plugin | — |
| **T3 integracion** | CI `push` 4/4 sobre el SHA exacto; CI del merge + coberturas | — |
| **T4 Owner** | AutoCAD 2025 (§20) | — |

## 20. Validacion del Owner (diseno; no ejecutar)

Checklist completo en el mapa §OV: **OV-LEG** (regresion de insercion de vista unica), **OV-ID17**, **OV-ID18** (incluido Esc,
`U` y fallo de redibujo), **OV-ID19** (una fila en planta → frontales alineadas; dos filas → aviso; laterales a traves de filas;
plantas desde frontales; layout enlazado e independiente; planta girada; mezcla invalida, espejo, MINSERT y familias mezcladas
rechazados sin escribir; seleccion de 100 o mas racks con duracion registrada), **OV-META** y **OV-PR** (PR-1 con la frontera
suprimida **antes** del corte elegido; PR-2 con brazos y tensores **visibles**).

## 21. Gates de implementacion (resumen; detalle en el mapa)

| Gate | Objetivo | Precondicion |
|---|---|---|
| G3 | Caracterizacion (solo pruebas): lecturas, payloads, nombres, puertas de primera vista sin rutas modales, conteos, descriptores de marco, resolucion sin editor | consenso V1 + ADR-0042 aceptado + OD-1..OD-8 decididas |
| G4 | PR-1 (H-01) | G3 |
| G5 | PR-2 (H-02) | G3 |
| G6 | PR-3 (H-13) | G3 |
| G7 | Foundation pura: renombre, variante, codec, soporte, descriptores de marco | G4-G6 |
| G8 | Plan por vista desde sistema resuelto (X-2) + preparacion por sistema | G7; acuerdo X-2 |
| G9 | Colocacion sobre la preparacion (vista unica) + D-10 + prompt + fixtures de Insertar (X-6) | G8; coordinacion G-M9 |
| G10 | ID17: puertas + Cabecera Planta-first | G9 |
| G11 | ID18 contrato puro | G10 |
| G12 | ID18 UI + presentador + driver + rutas de entrada + D-15 | G11; serializado con I-49 G10 |
| G13 | Nucleo neutral de seleccion (X-1) | acuerdo X-1 |
| G14 | ID19 plan puro: grupos, clasificacion, autoridad (X-3), marcos y proyeccion (D-16), salidas bloqueadas (D-17) | G12, G13; OD-6, OD-7 |
| G15 | ID19 comando + primitivo de materializacion (X-4) + censos | G14; OD-1 |
| G16 | Candidato, validacion del Owner, documentacion, integracion | todo |

## 22. Registro de decisiones

### 22.1 Decisiones tecnicas propuestas

| # | Decision | Seccion | Revision especial |
|---|---|---|---|
| D-01 | ALT-B | §4, §18 | Arquitecto |
| D-02 | `DimensionViewKind` → `RackViewKind`; `CantileverViewKind` con un mapeo | §7.1 | **A-4** |
| D-03 | Variante tipada + codec con disposicion; lectores no migrados caracterizados | §7.2-§7.3 | **A-1**, X-2 |
| D-04 | Matriz normativa y «vista soportada»; Cama y Larguero fuera de ID18/ID19; Cabecera Planta-first | §5 | Coordinador |
| D-05 | `RackId` al aceptar la intencion (B); un acunado por intencion en comandos sin sesion; curacion de `Id` en blanco conservada | §6 | Arquitecto |
| D-06 | ID18: transaccion C + maquina de estados | §11 | **A-3** |
| D-07 | Autoridad entre hermanas: authored equivalente (editor por construccion; ID19 sobre todas las hermanas); `CustomProperties` copiadas del sobre fuente; sin gate de I-54 | §9 | Coordinador + Arquitecto, X-3 |
| D-08 | ID19: nucleo neutral; grupos de una definicion; un tipo fuente; reglas de clasificacion; busqueda de kind sensible a mayusculas | §12.2-§12.3 | **X-1**, Arquitecto |
| D-09 | PR-1, PR-2 y PR-3 aislados antes de la foundation | §17.4 | Coordinador |
| D-10 | Limpieza de la definicion ante excepcion en ambas primitivas de colocacion | §14 | Arquitecto |
| D-11 | Presentacion de creacion vigente en ID19 | §12.6 | Arquitecto (X-4), Owner (OD-8) |
| D-12 | ADR-0042 como **sucesor de ADR-0010** (alternativa: ADR complementario) | ADR | Owner |
| D-13 | ID19 falla antes de escribir si faltan bloques; ID17/ID18 conservan el reporte | §12.7 | Arquitecto |
| D-14 | Una autoridad por responsabilidad con I-52 | §4.1, §17.2 | **X-1..X-6** |
| D-15 | En edicion, un fallo de redibujo impide insertar vistas nuevas (lote de uno incluido) | §11.5 | Arquitecto + Coordinador |
| D-16 | ID19: proyeccion ortografica por familia de marco con descriptores caracterizados en G3 y `T_common` | §12.4 | **A-6**, Owner (OD-6, OD-7) |
| D-17 | ID19: diagnosticos bloqueantes por kind y paridad caracterizada de la resolucion sin editor | §12.5 | Arquitecto |

**D-12, detalle.** ADR-0010 gobierna la edicion y no la insercion inicial, pero su decision dice que «una vista adicional solo
se inserta desde un rack ya existente». El lote de un rack nuevo lo contradice literalmente aunque cumpla su razon (diseno e
identidad fuente). Opcion A (recomendada): ADR-0042 **sucede** a ADR-0010 conservando Actualizar e Insertar, porque un ADR
aceptado es inmutable. Opcion B: ADR complementario que interprete las vistas 2..n del lote como insertadas desde el rack que crea
la primera colocacion; mas corto, pero deja vigente un texto literal contrario.

### 22.2 Decisiones del Owner (producto/UX)

| # | Decision | Opcion A | Opcion B | Compromiso | Recomendada | Consecuencia |
|---|---|---|---|---|---|---|
| OD-1 | Nombre del comando de ID19 | `RACKPROYECTAR` + alias `RPY` (libres) | `RACKPROYECTARVISTAS` sin alias | A corto y en patron `RACK*`; B mas explicito | **A** | Censos de comandos, ayuda y README |
| OD-2 | Variante en ID19 | canonica fija por sistema (§5) | preguntar una vez por sistema presente | A determinista; B control fino | **A** en V1 | B queda como mejora |
| OD-3 | Miembro no soportado | fallar todo sin escribir | omitir con aviso | A predecible; B menos reintentos | **A** | El usuario reselecciona |
| OD-4 | Esc a mitad de un lote | detener la cola | saltar a la siguiente | A: Esc = parar | **A** | Mensaje de cola parcial |
| OD-5 | Orden del lote | canonico F → L → P | orden de marcado | A determinista | **A** | El dialogo muestra el orden |
| OD-6 | Orientacion de las vistas proyectadas | orientacion natural de cada vista (rotacion 0), fuentes con orientacion comun | girar las plantas proyectadas para que la corrida siga la direccion de las elevaciones fuente | A simple y coherente con las inserciones vigentes (la corrida de las plantas de rack queda en Y); B mantiene la continuidad visual pero gira bloques segun la familia | **A** en V1 | B exige una regla de giro por familia |
| OD-7 | Que significa «layout relativo» cuando la clase pedida difiere de la fuente | **proyeccion ortografica** (§12.4): se conserva el eje compartido, el comun queda alineado y el descartado se colapsa con aviso | **traslacion literal** de las posiciones de las referencias | A da elevaciones utiles de un layout en planta, pero superpone filas distintas (separarlas exigiria enmendar CD-04, porque ya no seria una transformacion comun); B es trivial pero apila y superpone las elevaciones de un layout en planta | **A** | A requiere los descriptores de §12.4 y su caracterizacion |
| OD-8 | Capa y presentacion de las vistas proyectadas | las de creacion vigentes | las de la referencia fuente | A coherente con Insertar; B hereda la organizacion por capas | **A** | B exige capturar y asignar presentacion |

### 22.3 Open Material

- **Propio de I-55: NONE.** La semantica de layout de ID19 queda como decision del Owner (OD-7) con diseno completo de la opcion
  recomendada; bloquea G14, no la revision.
- **Entre iniciativas: X-1..X-6** (§17.2), con resolucion propuesta; requieren a los Arquitectos de I-55 e I-52.

### 22.4 Open Minor

| # | Tema |
|---|---|
| OM-1 | `SaveToLibrary` acuna antes en Cantilever y Push Back; sin efecto en DWG |
| OM-2 | Convergencia futura de `CantileverViewKind` con `RackViewKind` |
| OM-3 | Atomicidad del redibujo multivista de `RACKEDITAR` |
| OM-4 | Uniformar la eleccion de variante en vista unica |
| OM-5 | Bloques de biblioteca importados que sobreviven a fallos (vigente) |
| OM-6 | Vista fantasma de grupo para ID19 |
| OM-7 | `View` nulo de la Cama (F-04) |
| OM-8 | «frontal ×N» de `RACKLISTA` (H-12) |
| OM-9 | Lateral entero del Dinamico solo legado |
| OM-10 | Duplicados de `(RackId, View, Section)` legales (OQ-4) |
| OM-11 | Racks de Cantilever existentes conservan su `Id` interior desalineado |
| OM-12 | ID19 no mezcla la familia Cantilever con la de rack (ejes rotados y reflejados entre familias) |
| OM-13 | ID19 exige orientacion comun: filas espalda con espalda giradas 180° o espejadas se proyectan por separado |
| OM-14 | Una vista nueva hereda el `ExtensionData` del sobre de otra vista (vigente en Insertar); un campo futuro propio de vista viajaria mal (cf. ALT-L de I-52) |
| OM-15 | Proyeccion frontal ↔ lateral y de la misma clase no soportadas en V1 |
| OM-16 | Sin aviso de `CustomProperties` divergentes al insertar vistas (vigente) |

### 22.5 Hallazgo nuevo

**H-13 — `Id` interior del Cantilever sin alinear al crear** (§6.1; PR-3). Solo pruebas leen el `Id` interior
(`T/CantileverLineTests.cs:908`, `T/CantileverRoundTwoCharacterizationTests.cs:372`); F-09 describe el mismo efecto en biblioteca.

## 23. Revision adversarial (antes del commit)

Hecha en dos pasadas: una propia y otra con un agente de solo lectura que busco contradicciones contra el codigo; sus hallazgos se
verificaron uno a uno contra `dad4e77` antes de corregir.

| Busqueda o hallazgo | Evidencia | Correccion aplicada |
|---|---|---|
| **Bloqueante:** la traslacion comun no preserva el layout de planta a elevacion | ejes de planta y frontal distintos; filas y columnas de `RACKLAYOUT` (§12.4) | Proyeccion ortografica por familia de marco (D-16), descriptores con caracterizacion, pares soportados, OD-6/OD-7 |
| Gate de hermanas contra OQ-7 y la curacion de `Id` en blanco | `RackCustomPropertiesAuthority.cs:238-264`; `P/RackSelectivoCommands.cs:119` y equivalentes | D-07 opcion A sin gate de I-54; RID-4 conserva la curacion |
| Analisis con I-52 incompleto | contrato de entrada de su autoridad, laterales, listas STOP/NO TOCAR, lineas base de C-2, deberes del materializador, conjunto de miembros | X-2..X-4 ampliados; X-5 y X-6 nuevos |
| Rutas de entrada que perderian `Views` y acunado por vista en la cabecera | `U/Editor/EditorModules.cs:53-56`, `:90-95`; `P/RackSelectivoCommands.cs:34-38`; `P/RackCabeceraCommands.cs:173` | §11.2, RID-1/RID-2 y guarda en G12; la Cama si tiene sesion |
| Goldens byte a byte contra la alineacion del `Id` interior | D-05b en la foundation | PR-3 aislado antes de la foundation |
| D-10 sin la ruta del Cantilever | `P/Drawing/BlockPlacement.cs:64-76`; `P/RackCantileverCommands.cs:151-177` | Ambas primitivas |
| PR-2 con impacto invertido | `null` = apagados | Texto y validacion con brazos y tensores visibles |
| Guardas y censos omitidos | `SelectiveEditorOpenTests` (35), `CantileverPluginSourceGuardTests`, `SelectiveAuthoredCarrierAdoptionTests`, `PushBackPluginSourceGuardTests`, `RackUnitsGuardSourceTests`, pruebas de UI de peticion, sesion y modulos | §19 y mapa |
| Disposiciones del codec mal descritas y lectores omitidos | Selectivo y Dinamico leen un `View` desconocido; cuatro lectores mas | §7.3 |
| Casos de seleccion de ID19 | MINSERT, espejo, `Origin`, UCS, anonimos, tamano | D-08f, ancla por transformacion completa, OV con 100 o mas racks |
| Salidas bloqueadas y paridad sin editor | `RackBomOutputGate`; ventana Push Back; reconstruccion del Dinamico | D-17 |
| Borde de I-54 y barridos | guarda del borde; «un barrido» falso en edicion | Sin lectura por el borde; §16 corregido |
| Prerrequisitos no aislados | CD-07 | Gates G4-G6 antes de la foundation |
| ADR que decidia preguntas del Owner | Esc, variante, traslacion, presentacion, autoridad | ADR condicionado a OD-1..OD-8; base del Discovery corregida |
| Puntos despues de importar; estado de redibujo fallido; `Regen` | I-52 pide puntos antes; redibujos ignorados; Cantilever regenera dos veces | §12.1, D-15, §11.4 |
| Prompt de colocacion, presentador del dialogo, `TGrd02` | `BlockPlacement.cs:26`, `:214`; guardas de modales; parametro `RackEmbedDocument` | §11.1, §11.4, §8 |
| Conteos y filtros | conteos por linea confundidos con ocurrencias; filtros que no seleccionaban pruebas nuevas; AGENTS.md pide no copiar conteos | Conteos por archivo; filtros por clase en el mapa; conteos de pruebas solo en la evidencia del gate |
| Detalles | busqueda de kind sensible a mayusculas; `ExtensionData`; STA de pruebas modales; ubicacion del codec | D-08g, OM-14, G3 sin rutas modales, X-2 |
| Primera redaccion: una referencia por `RackId` | `P/RackLayoutCommands.cs:29-30` | D-08b |
| Numero de ADR | I-49 publico ADR-0041 en su rama (`8cefd59`) mientras se redactaba esta Proposal | ADR renumerado a 0042 antes del commit |
| Primera redaccion: purga y `Regen` en ID19 | I-51 INV-14 | Eliminados |
| ADR-0009, ADR-0034, ADR-0035, ADR-0039, I-45 | — | Sin contradiccion (§2, §9, §19) |
