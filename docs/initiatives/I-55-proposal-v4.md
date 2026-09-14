# I-55 — Proposal V4: View Placement & Projection (ID17 + ID18 + ID19)

```text
PROPOSAL V4 — NOT CONSENSUS
Coordinator                         = REVIEW REQUIRED (CQ-01 decidida en G2E; V4 reconcilia solo lo que deriva de ella)
Architect formal                    = PENDING (revision independiente pendiente: I-55-architect-review-package-v4.md)
Owner                               = PENDING (M-01, OD-1..OD-8, ADR-0042)
Consensus                           = NOT REACHED
Implementation                      = BLOCKED
Open Material                       = M-01 (propio) · adopcion de X-1..X-8 por I-52 (reconciliacion obligatoria)
ADR-0042                            = PROPUESTO, COMPLEMENTARIO de ADR-0010 (que sigue ACEPTADO)

Estado de V3 (8e35a51)              Coordinator: CQ-01 = CAMBIAR A PREPARE → UNA MUTATE → POST (G2E) → Proposal V4
                                    Architect formal = PENDING
Estado de V2 (f84f303)              Coordinator = CHANGES REQUIRED → V3 · Architect formal = PENDING

Initiative     = I-55 — View Placement & Projection
Branch         = feature/creacion-de-vistas
BASE_SHA       = ba497f14581d81e83a27514852d6ec082ff57635   (base original; codigo auditado por el Discovery)
CURRENT_BASE   = dad4e77f4f267b9fa248ecb0c8bfd8a74bbab093   (merge de I-53D)
CURRENT_MAIN   = dad4e77f4f267b9fa248ecb0c8bfd8a74bbab093   (preflight de G2E: main no avanzo)
Historial      = Proposal V1 d1918ab · Proposal V2 f84f303 · revision tecnica G2C d091eeb · Proposal V3 8e35a51 · paquetes V3 110cd39
Paralelas      = I-49 c8cfee2 (Amendment A3-R1) · I-52 e59bc89 (Proposal V14: autoridades compartidas y ubicaciones de G4..G7
                 provisionales, [V13-D08], [V14-D08]; registra V3 sin adoptarla, [V14-D14]) · I-56 18401da (G2, Proposal V1; no se aplica a I-55)
Decisiones     = docs/automation/decisions/I-55.md   (G2B: CR-01..CR-12; G2D: CD2-01..CD2-12; G2E: CQ-01)
Mapa           = docs/initiatives/I-55-implementation-map-v4.md   (misma version del plan)
ADR            = docs/adr/0042-preparacion-de-vistas-antes-de-materializar.md
Citas          = archivo:linea sobre CURRENT_BASE, verificadas antes del commit (§23)
Prefijos       = P/ src/RackCad.Plugin/ · A/ src/RackCad.Application/ · D/ src/RackCad.Domain/ · U/ src/RackCad.UI/
                 T/ tests/RackCad.Tests/ · TU/ tests/RackCad.UI.Tests/
```

> **Como leer V4.** Documento autocontenido; V1, V2 y V3 quedan como historial sin modificar. V4 **no** es un rediseno: parte de V3 y
> cambia solo lo que obliga la decision CQ-01 del Coordinador (§0.2). **Vocabulario de ID19:** una **seleccion proyectada** es el
> conjunto de referencias aceptadas en **una** ejecucion de ID19, y un **grupo `RackId`** es el subconjunto que comparte un `RackId`.
> La transformacion comun es **una por seleccion proyectada**, nunca por grupo `RackId`. **Vocabulario de Insertar:** el **redibujo de
> hermanas existentes** (§11.1) y la **colocacion de vistas nuevas** (§11.3) son dos operaciones distintas con transacciones distintas.

## 0. Reconciliacion V3 → V4

### 0.1 Estado de las revisiones

| Pieza | Estado |
|---|---|
| Proposal V1 (`d1918ab`) | Coordinator = CHANGES REQUIRED (G2B; CR-01..CR-12 vinculantes) |
| Proposal V2 (`f84f303`) | Coordinator = CHANGES REQUIRED → V3 (G2D). Architect formal = PENDING |
| Revision de V2 publicada en G2C (`d091eeb`) | Revision tecnica adversarial de la misma sesion que redacto V2; no es veredicto del Arquitecto |
| Proposal V3 (`8e35a51`) y paquetes V3 (`110cd39`) | Coordinator: decision vinculante **CQ-01** (G2E) → Proposal V4. Architect formal = PENDING |
| Proposal V4 (este documento) | Coordinator = REVIEW REQUIRED; Architect formal = PENDING, para un Arquitecto **independiente** con su propio paquete |

### 0.2 Decision CQ-01 del Coordinador (G2E, vinculante)

> «CQ-01 = CAMBIAR A PREPARE → UNA MUTATE TRANSACTION → POST PARA EL REDIBUJO DE SIBLINGS EXISTENTES DURANTE INSERTAR.»

Contrato que V4 fija a partir de ella (§11):

| Parte | Contrato |
|---|---|
| A. Redibujo de hermanas existentes | PREPARE de todas las hermanas mutables, sin escribir estado del rack → **una** transaccion MUTATE para todos los redibujos y los borrados de huerfanas (salvo que no quede ninguna hermana que redibujar: entonces las huerfanas se borran con la primera colocacion) → **un** commit → POST (purga consolidada, renombres cosmeticos, un `Regen`, diagnosticos). Un fallo en PREPARE no escribe nada del rack; un fallo en MUTATE revierte todo el redibujo |
| B. Colocacion de vistas nuevas | Solo tras el commit de A. Una transaccion por jig, como hoy; Esc o Enter a mitad conserva A y las vistas ya colocadas, sin rollback |
| Alcance | `RACKEDITAR` → Insertar (ID17 despues, ID18 en rack existente). **Actualizar no cambia** |
| Resultados distinguibles | `PREPARE_FAILED`, `REDRAW_ROLLED_BACK`, `REDRAW_APPLIED`, `PLACEMENT_CANCELLED_PARTIAL_BATCH`, `PLACEMENT_FAILED_PARTIAL_BATCH`, `COMPLETED` (nombres no contractuales; semantica si) |

### 0.3 Lo que V4 conserva de V3 sin cambio

ALT-B; `RackViewKind` y variante tipada; codec, disponibilidad y politica del consumidor separados; `Resolve`, `Plan` y `Prepare`; ID17;
colocaciones incrementales de ID18; ID19 completo (`CommonTransform2D`, `Rigid`, `Orthographic`, FRAMES, INV-GRP-1..4); gates de
propiedades y authored; requisito estructural de bloques; D-10, D-13 y D-17; PR-1 y PR-2; X-1..X-8 y su tabla de adopcion; ADR-0042
complementario; las resoluciones AR2 (§0.6) y AV3 (V3 §23) salvo las de la semantica de redibujo; todas las recomendaciones A (Owner
PENDING). La tabla X-1..X-8 conserva sus reglas de extraccion; CQ-01 solo precisa X-4, X-5 y X-6 (CQ1-09).

### 0.4 Cambios derivados de CQ-01

| # | Cambio | Seccion |
|---|---|---|
| CQ1-01 | D-15 reescrita: redibujo atomico de hermanas antes de Insertar | §11.1; §22.1 |
| CQ1-02 | Conjuntos clasificados antes de abrir la transaccion: mutable (redibujables y huerfanas, incluida la definicion elegida que el flujo vigente anade) y de solo lectura (dependientes de xref); preflights vigentes antes de todo | §11.1; §9.2 |
| CQ1-03 | Huerfanas: dentro de la MUTATE si queda alguna hermana mutable redibujable; si no, en la transaccion del primer jig, con justificacion | §11.1 |
| CQ1-04 | Fases sin efectos cruzados: PREPARE sin escrituras del rack, MUTATE sin importacion, purga, `Regen`, renombre ni transacciones anidadas; POST solo tras el commit | §11.1; §15 |
| CQ1-05 | Seam atomico y envoltorios minimos por kind, sin framework paralelo | §11.2 |
| CQ1-06 | Maquina de estados y mensajes con los seis resultados, mas `REDRAW_NOT_REQUIRED` cuando no hay nada que redibujar | §11.4 |
| CQ1-07 | INV-AUTH-1 e INV-RED-1 reescritas; INV-RED-2, INV-RED-3 e INV-BATCH-1 nuevas | §13 |
| CQ1-08 | Tablas de errores, transacciones y recursos | §14; §15; §16 |
| CQ1-09 | X-4 con dos operaciones de la misma familia (crear y redefinir en la transaccion del llamador); X-5 y X-6 con los disparadores de I-52 V14 | §17.2 |
| CQ1-10 | Gate G9 dividido en G9a (seam atomico, sin cablear) y G9b (precondiciones de Insertar) | §21; mapa |
| CQ1-11 | Pruebas de atomicidad (quince casos) y validacion del Owner | §19; §20; mapa G9a (1-6, 9-15), G11 (7, 8), OV-RED (G9b vista unica, G12 lote) |
| CQ1-12 | ADR-0042 decision 6 y alternativas precisadas | ADR-0042 |
| CQ1-13 | OM-3 reformulada (solo Actualizar); OM-5 ampliada; OM-26, OM-27; H-15 | §22.4; §22.5 |

Retirados de V3 por CQ-01: el STOP con redibujos confirmados que permanecen, `REDRAW_FAILED`, «actualizada con aviso» dentro del
redibujo, el borrado verificado de huerfanas fuera de la transaccion y la pregunta CQ-01 abierta.

### 0.5 Prescripciones del Coordinador en G2D (siguen vigentes)

| # | Prescripcion | Estado en V4 |
|---|---|---|
| CD2-01 | Estado de revisiones; paquete para Arquitecto independiente | §0.1; paquetes V4 |
| CD2-02 | `Orthographic` por familia + tipo + variante destino | Sin cambio (§12.5) |
| CD2-03 | Redibujo parcial con STOP y sin transaccion global | **Sustituida por CQ-01** (§0.2, §11) |
| CD2-04 | Requisito estructural de bloques por pieza | Sin cambio (§4.6) |
| CD2-05 | `Resolve` compartido | Sin cambio (§4.5) |
| CD2-06 | Codec, disponibilidad y politica separados | Sin cambio (§7.3) |
| CD2-07 | Contrato `Resolve`/`Plan`/`Prepare` sin ciclo | Sin cambio (§4.4) |
| CD2-08 | LOW de G2C como AR2-nn | §0.6 |
| CD2-09 | Tabla X y adopcion | Actualizada a I-52 V14 (§17.2) |
| CD2-10 | ADR-0042 complementario | Sin cambio de relacion; decision 6 precisada |
| CD2-11 | Owner PENDING; M-01 OPEN | Sin cambio (§22.2, §22.3) |
| CD2-12 | Revision adversarial antes del commit | §23 (ataques de G2E) |

### 0.6 Matriz AR2-01..AR2-20 (de V3; AR2-02 revisada por CQ-01)

| AR2 | Hallazgo (revision tecnica de V2) | Resolucion en V3 | Seccion |
|---|---|---|---|
| AR2-01 | `Orthographic` rechazaba Cantilever → Planta | Direccion destino por familia + tipo + variante de cada referencia; tabla de verificacion | §12.5 |
| AR2-02 | `REDRAW_FAILED` sin redibujo parcial | V3: STOP sin rollback. **V4 (CQ-01):** redibujo atomico PREPARE → una MUTATE → POST; INV-RED-1..3, INV-BATCH-1 | §11.1; §13 |
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
| Vista enlazada | Redibujo por vista, cada uno con su propia confirmacion (`P/Systems/Shared/SystemBlockWriter.cs:55-66`), y fallos ignorados (`P/RackSelectivoCommands.cs:178-183`; `P/RackCantileverCommands.cs:328-332`); en Insertar, el prompt de variante llega despues de los redibujos (`P/RackSelectivoCommands.cs:241-258` → `:476-489`) y las huerfanas se conservan si son las unicas vistas (`:229-239`); los bucles tambien recorren las definiciones dependientes de xref (`P/RackBlockFinder.cs:66`) | En Insertar: preflights vigentes → variante → gate → clasificacion → PREPARE → a lo sumo **una** MUTATE con el redibujo de las hermanas mutables y el borrado de huerfanas → POST → colocar vista a vista (D-15, §11.1); las dependientes de xref no se tocan; Actualizar sin cambio |
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

Redibujo de hermanas     RackSiblingRedrawPlan (puro): clasificacion → PREPARE → una MUTATE → POST (§11.1, §11.2)
Lote (ID18)              RackViewBatchPlan (puro): PREPARE ALL → redibujo atomico → cola de colocaciones (§11.3, §11.4)
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
explicito en la firma y sirve igual a I-52, que ya recibe el nombre como entrada (V14 §8.1).

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
  - I-52: su politica (V14 §7.9: PRE-06, PRE-07 y PRE-08 en §9.1 P3/P4; PRE-11 en SNAPSHOT/P1 con E2). `UnsupportedLegacy` no tiene
    correspondencia en V14; la tabla de adopcion propone E6 (§17.2.1).
- **Paridad.** G3 caracteriza, por kind, que resolver el payload persistido da el sistema que se dibujo al insertar y el que obtiene el
  editor al abrirlo, con fixtures legacy (el Dinamico solo-sistema entre ellos). Una diferencia no explicada deja ese caso fuera de ID19
  hasta una decision posterior. La reconstruccion no idempotente del editor Dinamico (H-14) no es paridad del sistema persistido y se
  registra aparte.
- **Custodia y extraccion.** Custodio I-55 (infraestructura de vistas, X-2), en `A/Views/Resolution/`. **La extrae I-55 G7a**, y en ese
  mismo gate los handlers del BOM de Selectivo, Dinamico, Push Back, Cantilever, Cabecera y Cama delegan en ella sin cambio observable
  (BOM y motivos de bloqueo caracterizados en G3), para que no queden dos autoridades. I-52 no puede extraerla sin excepcion: su lista
  de archivos excluidos incluye `KindHandlers/*` (V14 §20.3, no provisional); las ubicaciones de sus gates G4..G6 son provisionales ([V14-D08]). Si I-52 necesita `Resolve` antes
  de que I-55 G7a exista en su base, aplica el STOP de X-2 con sus opciones (a) y (b) (§17.2).
- **Consumidores.**
  - I-55: ID19 (G14, G15).
  - I-52: precondiciones P3/P4 en G5 y `RackViewPlanAuthority.Build` = `Resolve` + `Plan` en G6 (V14 §8.1, que ya lo registra; C2-1 intacto).
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
| Edicion con `Id` en blanco | Selectivo y Dinamico curan (la definicion elegida se redibuja con el `Id` nuevo); Push Back y Cantilever abortan la edicion en su preflight de sobre, porque la elegida no lleva el `Id` nuevo (`P/RackPushBackCommands.cs:179-203`; `P/RackCantileverCommands.cs:230-252`); Cabecera acuna uno nuevo | `P/RackSelectivoCommands.cs:119`; `P/RackDinamicoCommands.cs:174`; `P/RackPushBackCommands.cs:179`; `P/RackCantileverCommands.cs:230`; `P/RackCabeceraCommands.cs:233` (H-08) |

**Decision propuesta: B** (acunar al aceptar la intencion): A deja GUIDs sin uso; C obliga al Plugin a inventar identidad.

- **RID-1.** Un rack nuevo recibe su `RackId` una vez, al aceptar la intencion; sin sesion, el comando acuna una vez por intencion.
- **RID-2.** Todas las vistas de esa aceptacion lo llevan; toda ruta de entrada reenvia la lista de vistas (§11.2).
- **RID-3.** Una hermana lleva el `RackId` del sobre elegido o, en ID19, el de su grupo `RackId`.
- **RID-4.** `RACKEDITAR` trata un `Id` en blanco como hoy (cura en Selectivo, Dinamico y Cabecera; aborta en Push Back y Cantilever); ID19 falla cerrado ante un sobre sin `Id`. En I-55, «en blanco» es `IsNullOrWhiteSpace` (§9.2).
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

`DimensionViewKind` no esta en la instantanea de enums de la guarda de cobertura estructural de I-52 (V14 §6.7) y no se serializa. Si
cuando aterrice el renombre esa guarda ya lo alcanza en la base de I-55, el renombre se reporta (X-5) y **el reapunte lo decide el
Coordinador de I-52** (su §6.7 trata un renombrado como RED / STOP); I-55 no debilita la guarda. El renombre solo lo hace I-55 G6: toca
editores WPF, que I-52 excluye (V14 §20.3).

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
| `RACKEDITAR` (legado, sin cambio) | lo que lee hoy, incluido lo coercionado | **Por kind.** Selectivo: una lateral `Invalid` (`Section < 0`) y las `Orphaned` se borran como fantasmas (`P/RackSelectivoCommands.cs:169-173`, `:194-198`), salvo que sean las unicas vistas (`:229-239`). Push Back y Cantilever: un descriptor `Invalid` aborta la edicion entera (`P/RackPushBackCommands.cs:216-223`; `P/RackCantileverCommands.cs:264-271`); una estacion `Orphaned` se borra (`:304-310`). Dinamico y Push Back: un poste `Orphaned` se borra (`P/RackDinamicoCommands.cs:240-244`, `:268-271`, sin mensaje si no sobrevive ninguna; `P/RackPushBackCommands.cs:285-289`, `:306-309`). Push Back: `PushBackCut(e, B)` en un rack de un sentido (`Orphaned` en B) **no** se borra: el builder ignora el lado B y se redibuja como A conservando `Section` (`A/Systems/PushBack/PushBackSystemFrontalBuilder.cs:44`, `:50-52`; `P/RackPushBackCommands.cs:253-269`) | La reescritura de `Canonicalizable` es **por kind**: Cantilever conserva el token `View` crudo (`P/RackCantileverCommands.cs:313-315` → `A/Persistence/RackEmbedComposer.cs:54`) |
| ID17 / ID18 | escriben solo direcciones `Canonical`; el sobre elegido de una hermana se lee con la politica de `RACKEDITAR` | — | Sin cambio de lectura legacy |
| **ID19** | `Canonical` o `Canonicalizable` **y** `Available` (canonizacion semantica **sin reescribir** la fuente) | `Coerced` e `Invalid` **fallan en CLASSIFY**; la disponibilidad solo se evalua sobre direcciones `Canonical` o `Canonicalizable`, y `Orphaned` y `Unsupported` fallan en AVAILABLE; todo antes de pedir puntos | Diagnostico determinista (abajo) |
| I-52 | su politica (V14 §3.7, §7.9, §9.1) | — | Consume A y B |

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
- **Conjunto mutable y conjunto de solo lectura (D-15).** El redibujo atomico de Insertar (§11.1) solo toca las hermanas **propias del
  dibujo** (los miembros de este gate). Las definiciones dependientes de xref que la busqueda vigente tambien recorre
  (`P/RackBlockFinder.cs:66` solo omite las de xref, no las dependientes) las gobierna su dibujo de origen, que las repone al recargar la
  xref: la clasificacion las separa antes de abrir la transaccion, no entran en PREPARE ni en MUTATE, no cuentan en los gates y el informe
  las nombra. Intentar mutarlas podria bloquear Insertar de forma permanente en un rack que tambien esta en una xref cargada (OM-25).
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
  - Un sobre elegido con `Id` formado solo por espacios se cura como uno vacio antes del gate y es miembro por la regla (1). Las
    definiciones que llevan exactamente ese mismo `Id` de espacios, que hoy el Selectivo y la Cabecera encuentran y redibujan
    (`IsNullOrEmpty`, `P/RackSelectivoCommands.cs:119`; `P/RackCabeceraCommands.cs:233`), entran en la clasificacion y se curan con el
    `Id` nuevo, para no partir el rack.
  - Una hermana de la busqueda vigente que no sea miembro del gate (por ejemplo, dependiente de xref) queda en el conjunto de solo lectura:
    Insertar no la redibuja (OM-25); Actualizar la trata como hoy.
  - Una fuente lateral o planta con `Id` en blanco que el flujo vigente no anade al redibujo (`P/RackSelectivoCommands.cs:132-136`;
    `P/RackCabeceraCommands.cs:243-246`) sigue sin redibujarse: es el sobre elegido, miembro del gate, y la vista nueva hereda su
    coleccion.

### 9.3 Authored

- **Editor:** el flujo vigente unifica el authored de las hermanas y lo compone en el payload de cada unidad preparada. D-15 confirma
  todas las hermanas mutables y el borrado de huerfanas en una sola transaccion antes de colocar ninguna vista nueva o, si no queda ninguna
  hermana que redibujar, borra las huerfanas con la primera colocacion (§11.1), de modo que ninguna hermana propia del dibujo conserva un
  authored distinto del de la vista nueva (salvo el residual R-25).
- **ID19:** `RackSiblingAuthoredGate` sobre **todas** las hermanas del barrido: Selectivo `SelectiveAuthoredAuthority.Resolve`
  (`A/ProjectVariables/SelectiveAuthoredAuthority.cs:128-157`); demas kinds, comparador de X-3 (excluye la metadata interior de
  `P/RackCommandSupport.cs:40-67`). Divergente o ilegible → fallo con remedio `RACKEDITAR`. Una hermana que el preflight de `RACKEDITAR`
  rechazaria (§4.5) → fallo, porque ese remedio no funcionaria.

### 9.4 Donde aplica

| Flujo | Gate de propiedades | Gate authored | Momento |
|---|---|---|---|
| Rack nuevo | no | no | — |
| `RACKEDITAR` → Actualizar | no | no | sin cambio |
| `RACKEDITAR` → Insertar una vista (ID17 despues) | **si** | flujo vigente + D-15 atomica | antes de clasificar, preparar y mutar |
| `RACKEDITAR` → Insertar varias (ID18) | **si** | flujo vigente + D-15 atomica | idem |
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
- **Transacciones (D-06, D-15; A-3).** Dos operaciones distintas:
  - **redibujo de hermanas existentes** (rack existente): PREPARE → **una** MUTATE → POST (§11.1, §11.2);
  - **colocacion de vistas nuevas**: definicion confirmada y despues un jig con su propia transaccion, como hoy
    (`P/Systems/Shared/SystemBlockWriter.cs:18-40`; `P/Drawing/BlockPlacement.cs:174-201`); ninguna transaccion de escritura del lote
    queda abierta entre jigs (§11.3).
  En un rack nuevo no hay redibujo de hermanas.

### 11.1 Redibujo atomico de hermanas existentes (D-15; CQ-01)

**Hoy.** Cada bucle de `RACKEDITAR` redibuja vista por vista con un envoltorio que toma el lock, importa, abre y confirma su propia
transaccion, purga y captura cualquier excepcion (`P/Systems/Shared/SystemBlockWriter.cs:45-80`; `P/Systems/Shared/ViewBlockDraw.cs:114-143`;
`P/Drawing/LateralHeaderDrawService.cs:112-157`; el Cantilever abre la suya alrededor de `RedefineBlock`,
`P/RackCantileverCommands.cs:319-332`). El bucle solo deja de contar y de renombrar la vista que falla, asi que una edicion puede confirmar
unas hermanas y otras no (`P/RackSelectivoCommands.cs:178-183`). Renombres (`P/Drawing/RackBlockRenamer.cs:21-58`) y borrado de huerfanas
(`P/RackCommandSupport.cs:210-275`) abren tambien sus propias transacciones.

**Precedente y seams existentes.** `ProjectVariableMutationExecutor` redibuja todas las vistas afectadas del Selectivo con PREPARE, una
transaccion y POST, bajo un solo `LockDocument` que cubre las tres fases (`P/ProjectVariableMutationExecutor.cs:140-297`; MUTATE
`:250-288`; POST `:292-293`); si un escritor lanza dentro de la transaccion, sale sin `Commit` y todo se revierte (`:299-303`). Seams de
MUTATE en la transaccion del llamador: `SystemBlockWriter.RedefineInTransaction` (`P/Systems/Shared/SystemBlockWriter.cs:82-116`, metodo en
`:104`), `ViewBlockDraw.PrepareRedraw`/`RedrawInTransaction` (`P/Systems/Shared/ViewBlockDraw.cs:59-89`, `:97-109`; PREPARE «must be called
inside the document lock», `:63`), `LateralHeaderDrawService.PrepareRedraw`/`RedrawInTransaction` (`P/Drawing/LateralHeaderDrawService.cs:60-107`)
y `CantileverViewMaterializer.RedefineBlock`, que ya recibe la transaccion del llamador, crea en ella las capas de rol y no bloquea,
confirma, importa ni regenera (`P/Drawing/Cantilever/CantileverViewMaterializer.cs:63-90`, `:142`).

**D-15 — Redibujo atomico de hermanas antes de Insertar.** Antes de materializar una vista nueva en un rack existente, toda sincronizacion
requerida de las hermanas existentes se ejecuta como **una** mutacion atomica PREPARE → MUTATE → POST:
- cualquier fallo en PREPARE → ninguna escritura del rack;
- cualquier fallo en MUTATE → rollback de todos los redibujos y borrados de esa operacion;
- solo tras el commit empieza la colocacion de las vistas nuevas;
- las colocaciones nuevas conservan su transaccion independiente por jig (§11.3).

```text
0. PREFLIGHT vigente por kind, sin cambio y sobre el barrido completo (incluidas las dependientes de xref, como hoy), antes de todo:
     sobre (kind e Id) y descriptor de vista (Push Back P/RackPushBackCommands.cs:194-223; Cantilever P/RackCantileverCommands.cs:243-271),
     interior I-11 (Dinamico P/RackDinamicoCommands.cs:186-191; Cabecera P/RackCabeceraCommands.cs:250-256), catalogo de secciones del
     Cantilever (:273-276), resolucion y reconciliador del Selectivo (P/RackSelectivoCommands.cs:66-73, :144-151); aviso de unidades
     (I-05) antes de pedir nada. Un fallo aborta la edicion como hoy. Una intencion de vinculo del Selectivo sigue yendo a
     ProjectVariableMutationExecutor y nunca coincide con Insertar (:93-98): no hay dos ejecutores en una operacion
1. VARIANTE (sin cambio desde V3): la variante de CADA vista nueva se elige con el sistema nuevo del editor
     - Esc en el prompt de variante → VARIANT_CANCELLED: nada modificado
     - Enter en un prompt de entero con valor por defecto acepta el valor por defecto
       (P/RackSelectivoCommands.cs:476-489, :546-559; P/RackDinamicoCommands.cs:318-330; P/RackPushBackCommands.cs:367-379)
     - variante no disponible → PREPARE_FAILED
2. GATE de propiedades del RackId (§9.2) sobre las hermanas propias del dibujo → SIBLING_GATE_FAILED si falla

3. CLASIFICAR (puro, antes de abrir ninguna transaccion). Entrada: las definiciones del barrido con Id == RackId, MAS la definicion
   elegida cuando el flujo vigente la anade porque el barrido no la encuentra (Selectivo como frontal, P/RackSelectivoCommands.cs:132-136;
   Dinamico cualquiera, P/RackDinamicoCommands.cs:179-182; Cabecera no planta, P/RackCabeceraCommands.cs:243-246), que es la destinataria
   de la cura de un Id en blanco, MAS, en Selectivo y Cabecera (los kinds que hoy las redibujan), las definiciones cuyo Id coincide
   exactamente con el Id solo de espacios del sobre elegido (§9.2); sin duplicados por BlockId
     READ-ONLY   dependientes de xref (IsDependent; el barrido solo omite las de xref, P/RackBlockFinder.cs:66): la orden de G2E
                 prohibe intentar modificarlas; fuera de los gates, de PREPARE y de MUTATE; el informe las nombra (OM-25)
     MUTABLE     propias del dibujo:
                 ERASE   huerfanas: las que el bucle vigente borra como fantasmas, por kind (§7.3 C, §9.2)
                 REDRAW  todas las demas que el bucle vigente redibuja, incluidas PushBackCut(e, B) en un rack de un sentido (se redibuja
                         como A, P/RackPushBackCommands.cs:253-269) y la lateral legacy del Dinamico leida como Post(0) (:237-239)
                 la cabecera no tiene huerfanas (P/RackCabeceraCommands.cs:239-291)
     Ubicacion del borrado de huerfanas (una sola regla):
                 REDRAW ≠ ∅ → ERASE va DENTRO de la MUTATE (paso 5)
                 REDRAW = ∅ (aunque queden dependientes de xref) y ERASE ≠ ∅ → ERASE va en la transaccion del PRIMER jig (§11.3), porque
                                          borrarlas antes de que exista otra vista propia destruiria la unica identidad persistida
                                          del rack si ese jig se cancela (el vigente las conserva por eso, P/RackSelectivoCommands.cs:229-239)

4. PREPARE (bajo el LockDocument de la operacion; sin escrituras del estado del rack; sin transaccion de escritura; sin purga, Regen ni
   renombre). La derivacion por kind vive en Application (§11.2); el Plugin solo construye las unidades concretas:
     por unidad REDRAW: BlockId (no nulo), sistema resuelto del editor, plan de la vista con el builder vigente, catalogo, requisitos de
                        bloque y payload final NO VACIO, byte a byte igual al que compone hoy el bucle vigente para esa vista: mismo
                        formula de composicion vigente (caracterizada en G3 por LegacyViewPayloadCompositionCharacterizationTests y expuesta en
                        Application por RackViewEnvelopeComposition en G7b, sin llamadas nuevas a Compose en G9a), desde el sobre PROPIO de la hermana y, en Dinamico, Push Back, Cantilever y
                        Cabecera, con el interior de ResolvedByBlock[BlockId] (P/RackSelectivoCommands.cs:175-177; RackPushBackCommands.cs:259)
     por unidad ERASE:  definiciones y referencias a borrar, leidas y validas
     vistas NUEVAS:     PREPARE ALL de V3 (sistema, disponibilidad, semilla, plan, sobre, requisitos de bloque)
     importacion:       las definiciones de biblioteca que los planes necesitan se importan aqui, fuera de la transaccion, como en el
                        precedente (P/Systems/Shared/ViewBlockDraw.cs:86; P/Drawing/LateralHeaderDrawService.cs:84). Es el UNICO efecto
                        permitido en PREPARE: la clonacion trae definiciones de biblioteca con sus capas, tipos de linea, estilos y
                        bloques anidados (comprobacion de faltantes P/Drawing/BlockLibraryImporter.cs:55-67; clonacion :110), no estado del rack, y no se revierte (OM-5)
     cualquier fallo, payload vacio o BlockId nulo → PREPARE_FAILED: ninguna definicion del rack redefinida, ningun payload, renombre,
                        borrado ni referencia

5. MUTATE (el mismo LockDocument; UNA transaccion abierta por el ejecutor; solo si hay alguna unidad REDRAW o ERASE):
     por cada unidad REDRAW, en el orden del bucle vigente: redefinir en la transaccion del llamador + escribir el payload + acumular
       las definiciones anidadas obsoletas + recoger el resultado del dibujo (piezas sin bloque)
     por cada unidad ERASE (si toca aqui): borrar referencias y definicion en la misma transaccion + acumular las anidadas obsoletas,
       incluidas las de cota (*D)
     prohibido dentro: importar (EnsureBlocks abre su propia transaccion y devuelve 0 en silencio, SystemBlockWriter.cs:96-101),
       abrir transacciones (tambien StartOpenCloseTransaction), tomar otro lock, purgar, regenerar, renombrar, confirmar por unidad,
       llamar a RedrawInPlace o CreateBlock, capturar excepciones de una unidad
     cualquier excepcion O resultado de fallo tipado de una unidad → sin Commit → la transaccion se descarta → ninguna hermana
       redibujada ni borrada → REDRAW_ROLLED_BACK
     todas pasan → UN Commit → REDRAW_APPLIED
     sin ninguna unidad → no se abre transaccion ni corre POST → REDRAW_NOT_REQUIRED

6. POST (solo tras el commit; el mismo LockDocument; nunca revierte; sus excepciones se capturan aparte y NUNCA se traducen en
   REDRAW_ROLLED_BACK):
     purga consolidada de las anidadas obsoletas (SystemBlockWriter.PurgeAfterCommit), excluidas las definiciones que exigen los requisitos
       de bloque de las vistas nuevas preparadas, para no retirar una pieza que la colocacion siguiente necesita. La purga vigente
       registra y absorbe sus errores (P/Drawing/LateralHeaderDrawer.cs:233-237): un fallo queda en el log, no en el informe
     renombres de las definiciones redibujadas (RackBlockRenamer.SyncName, sin cambio), con el nombre destino que calcula PREPARE con
       las funciones de nombre vigentes de cada bucle (p. ej. FrontalName, P/RackSelectivoCommands.cs:503-511): tambien registra y absorbe (:53-57)
     informe de piezas sin bloque de las hermanas redibujadas, con el requisito estructural (§4.6; consulta de G8, cableada en G9b)
     UN Regen si la MUTATE confirmo alguna unidad; si lanza → aviso «las vistas se actualizaron; regenera el dibujo»; la insercion continua
```

- **Por que el renombre va en POST.** El nombre de la definicion es cosmetico por contrato vigente («The name is cosmetic; never let a
  rename failure break the edit flow», `P/Drawing/RackBlockRenamer.cs:55`); ninguna vista se resuelve por nombre (se identifican por GUID,
  `P/RackCommandSupport.cs:124`, o por handle); y hoy ya se renombra despues de redibujar, de modo que la lateral agrupa sus anidadas con el
  nombre anterior (`P/Drawing/LateralHeaderDrawService.cs:83`). Si la MUTATE se revierte, POST no corre y nada se renombra. Llevarlo a
  MUTATE exigiria un seam nuevo y convertiria un fallo cosmetico en rollback (OM-27).
- **Por que el borrado de huerfanas va en MUTATE.** Quitar una huerfana forma parte del estado logico actualizado: si quedara tras un
  redibujo confirmado, conservaria el authored anterior junto a hermanas con el nuevo. Por eso nunca se hace «commit del redibujo → borrar
  huerfana despues».
- **Capas bloqueadas (PLAUSIBLE).** La redefinicion y el borrado abren entidades y referencias para escritura
  (`P/Drawing/LateralHeaderDrawer.cs:117`, `:169`; `P/Drawing/Cantilever/CantileverViewMaterializer.cs:75`, `:87`;
  `P/RackCommandSupport.cs:250`). Una sola referencia en una capa bloqueada revierte todo el redibujo en cada Insertar hasta desbloquearla,
  donde hoy esa vista se saltaba. Es la consecuencia correcta de la atomicidad: `REDRAW_ROLLED_BACK` nombra la vista y propone desbloquear
  la capa (R-26); no hay caracterizacion automatica sin AutoCAD, y OV-RED-03 lo provoca a mano.
- **Solo lectura.** Las dependientes de xref las gobierna su dibujo de origen, que las repone al recargar la xref; la orden de G2E prohibe
  modificarlas. En Insertar dejan de redibujarse (cambio declarado, §3); Actualizar las trata como hoy (asimetria registrada en OM-25).
- **Autoridad authored de los gates.** Gate de propiedades: las hermanas propias del dibujo (§9.2), incluidas las huerfanas. Authored del
  editor: el diseno que el editor unifico y compone en todos los payloads preparados (§9.3). Las de solo lectura no participan en los
  gates; si participan del preflight vigente (paso 0).
- **Project Variables y expresiones.** Los payloads preparados llevan el authored reconciliado verbatim (vinculos incluidos,
  `P/RackSelectivoCommands.cs:153`); el rollback no deja ningun payload con un authored parcial, y el commit deja el mismo authored en
  todas las hermanas mutables.
- **Alcance.** `RACKEDITAR` → Insertar (ID17 despues e ID18 en rack existente). `RACKEDITAR` → Actualizar **no cambia**: conserva su
  redibujo vista por vista (OM-3).

### 11.2 Seam atomico y envoltorios minimos

**Sin framework paralelo.** La derivacion por kind y la secuencia son puras y viven en Application; el Plugin solo aporta unidades
concretas que escriben en la transaccion del llamador y un adaptador de transaccion. `ProjectVariableMutationExecutor` no se modifica (su
delegado esta tipado a `PreparedViewRedraw`, `P/ProjectVariableMutationExecutor.cs:103-107`; su convergencia con este seam queda como
OM-26).

| Pieza | Capa | Contenido |
|---|---|---|
| Plan de redibujo de hermanas | Application (pura) | Clasificacion READ-ONLY / REDRAW / ERASE con la entrada del paso 3; **descriptores tipados por vista y kind** (`HeaderRun` con la peticion de plan del builder vigente, `CantileverCurves`, `Erase`) y su payload compuesto; ubicacion del borrado de huerfanas; orden; resultados y mensajes (§11.4) |
| Ejecucion secuenciada | Application (pura, con puerto) | PREPARE de todas → abrir → escribir cada unidad → commit o descarte → POST separado; contrato del puerto: una excepcion o un fallo tipado de una unidad descartan sin commit; una excepcion de POST no descarta nada |
| Unidad `HeaderRun` | Plugin | `PreparedViewRedraw` (`P/Systems/Shared/PreparedViewRedraw.cs:24-49`) + `RedrawInTransaction` |
| Unidad `CantileverCurves` | Plugin | {BlockId, `CantileverViewPlan`, payload} + `CantileverViewMaterializer.RedefineBlock` + `RackBlockData.Write`; sin anidadas obsoletas (solo polilineas y circulos, `:59-61`) |
| Unidad `Erase` | Plugin | Un borrado nuevo en la transaccion del llamador que recoge tambien las anidadas de cota (*D); lo usan la MUTATE y el gancho del primer jig (§11.3). `RackCommandSupport.EraseViewBlocks` no se toca, para no cambiar Actualizar; su convergencia queda como OM-29 |
| Adaptador del puerto | Plugin | `LockDocument` sobre PREPARE, MUTATE y POST + `StartTransaction` + un `Commit`; POST con `PurgeAfterCommit`, `SyncName`, informe de faltantes y `ApplyRegen`, en su propio bloque de captura |

**Envoltorios por kind** (auditados sobre `dad4e77`):

| Kind | Vista | Redibujo vigente | Seam PREPARE + transaccion del llamador | Envoltorio minimo |
|---|---|---|---|---|
| Selectivo | Frontal `Fondo(k)` | `P/RackSelectivoCommands.cs:178` | **Existe** (`P/Systems/Selective/SelectiveFrontalDrawService.cs:42-59`) | Ninguno |
| Selectivo | Lateral `Post(p)` con largueros | `:202` | **Existe** (`P/Drawing/LateralHeaderDrawService.cs:60-107`) | Ninguno |
| Selectivo | Planta | `:217` | **Existe** (`P/Systems/Selective/SelectivePlantaDrawService.cs:37-54`) | Ninguno |
| Dinamico | Lateral `Post(p)` | `P/RackDinamicoCommands.cs:248` | Falta | `PrepareRedraw`/`RedrawInTransaction` en `P/Systems/Dynamic/DynamicSystemDrawService.cs` con la misma lambda de plan que `RedrawInPlace` (`:51-59`) |
| Dinamico | Frontal `FlowEnd(e)` | `:225` | Falta | Idem en `DynamicFrontalDrawService.cs` (`:40-48`) |
| Dinamico | Planta | `:212` | Falta | Idem en `DynamicPlantaDrawService.cs` (`:38-46`) |
| Push Back | Lateral `Post(p)` | `P/RackPushBackCommands.cs:293` | Falta | Idem en `P/Systems/PushBack/PushBackSystemDrawService.cs` (`:47-55`) |
| Push Back | Frontal `PushBackCut(e, s)` | `:260` | Falta | Idem en `PushBackFrontalDrawService.cs` (`:45-53`), con extremo y lado |
| Push Back | Planta | `:246` | Falta | Idem en `PushBackPlantaDrawService.cs` (`:40-48`) |
| Cabecera | Lateral | `P/RackCabeceraCommands.cs:269-270` | **Existe** (`LateralHeaderDrawService`, sin extras) | Ninguno |
| Cabecera | Planta | `:280-281` | Falta | Idem en `P/Drawing/PlantaHeaderDrawService.cs` (`:34-43`) |
| Cantilever | Frontal, Lateral `Station(s)`, Planta | `P/RackCantileverCommands.cs:312-326` | **Primitivo existe** (`RedefineBlock`), falta la unidad | Unidad `CantileverCurves` (arriba); el plan sigue en `CantileverViewPlanBuilder.Build` (pura, `A/Systems/Cantilever/CantileverViewPlanBuilder.cs:248`) con `PlantaVisibility` (PR-2) |
| Huerfanas | — | `EraseViewBlocks` | Falta en transaccion del llamador | Unidad `Erase` (arriba) |
| Cama | Lateral | `P/RackCamaCommands.cs:226` | — | Ninguno: sin hermanas ni Insertar |

Los envoltorios de Dinamico, Push Back y planta de cabecera replican el par vigente del Selectivo: delegan en
`ViewBlockDraw.PrepareRedraw`/`RedrawInTransaction` con la **misma** lambda de plan que ya usa su `RedrawInPlace`, asi que no hay una
segunda autoridad de plan. **No se modifican** `SystemBlockWriter.cs`, `LateralHeaderDrawer.cs` ni `CantileverViewMaterializer.cs`: se llaman
(lista STOP de I-52 V14 §19; X-5).

### 11.3 Colocacion de vistas nuevas (sin cambio de transaccion)

```text
rack existente: PREFLIGHT → VARIANTE → GATE → CLASIFICAR → PREPARE → MUTATE → COMMIT → POST → PLACE(1) → PLACE(2) → … → COMPLETED
                (sin unidades: … → PREPARE → REDRAW_NOT_REQUIRED → PLACE(1) → …)
rack nuevo:     PREPARE ALL de las vistas nuevas → PLACE(1) → PLACE(2) → … → COMPLETED

PLACE(i): definicion + sobre confirmados en su transaccion (SystemBlockWriter.CreateBlock, P/Systems/Shared/SystemBlockWriter.cs:18-40)
          → jig en su propia transaccion (P/Drawing/BlockPlacement.cs:174-201) → commit de la referencia, o limpieza de la definicion (D-10)
          si i = 1 y las huerfanas deben borrarse aqui (paso 3): con el jig en OK, se borran en la transaccion del jig antes de su commit,
          con el mismo borrado en transaccion del llamador que la unidad Erase; tras su commit se purgan esas anidadas y se regenera; con el jig cancelado, la
          transaccion del jig confirma vacia como hoy (:186-191), las huerfanas quedan intactas y la definicion nueva se retira con la
          limpieza vigente (best effort, D-10). Si esa limpieza fallara, quedaria una definicion sin referencias con el authored nuevo
          junto a las huerfanas (residual R-25)
          el gancho esta en BlockPlacement.PlaceBlockWithJig, por donde pasan todos los caminos de insercion (ViewBlockDraw.cs:51;
          LateralHeaderDrawService.cs:224, :235; Cantilever PlaceDefinition, RackCantileverCommands.cs:161)
Esc o Enter en el jig i → la vista i no se crea; se conservan el redibujo confirmado y las vistas 1..i-1; sin rollback
```

La atomicidad del redibujo **no** convierte los jigs en una transaccion global: cada vista nueva confirma su definicion y su referencia
por separado, y la cola nunca mantiene abiertas las escrituras de varias interacciones del usuario.

### 11.4 Maquina de estados y mensajes

```text
PREFLIGHT    ─(falla)────────→ (abort vigente de la edicion, con su mensaje)
VARIANT      ─(Esc)──────────→ VARIANT_CANCELLED       "No se inserto ninguna vista."
VARIANT      ─(no disponible)→ PREPARE_FAILED
SIBLING_GATE ─(falla)────────→ SIBLING_GATE_FAILED     "No se inserto ninguna vista: <motivo y remedio>."
PREPARE      ─(falla)────────→ PREPARE_FAILED          "No se inserto ninguna vista y no se modifico ninguna vista existente: <vista> <motivo>."
MUTATE       ─(falla)────────→ REDRAW_ROLLED_BACK      "No se inserto ninguna vista y no se aplico ninguna actualizacion a las vistas
                                                         existentes: <vista> <motivo> [<remedio, p. ej. desbloquear la capa>]."
MUTATE       ─(commit)───────→ REDRAW_APPLIED          (sin mensaje propio; POST puede anadir faltantes y el aviso de Regen)
PREPARE      ─(sin unidades)─→ REDRAW_NOT_REQUIRED     (sin mensaje propio)
REDRAW_APPLIED, REDRAW_NOT_REQUIRED o rack nuevo → PLACING(i) → COMMITTED(i) → … → COMPLETED
     "Se insertaron <lista>."  (tras REDRAW_APPLIED: "Vistas existentes actualizadas. Se insertaron <lista>.")
PLACING(i) ─(Esc o Enter)──────→ PLACEMENT_CANCELLED_PARTIAL_BATCH
     "Se insertaron <lista 1..i-1 o ninguna>; no se insertaron <lista i..n>."  (tras REDRAW_APPLIED, precedido de "Vistas existentes actualizadas.")
PLACING(i) ─(excepcion o Error)→ PLACEMENT_FAILED_PARTIAL_BATCH
     mismo mensaje + "Fallo: <vista> <motivo>."
Informe comun: las hermanas de solo lectura no tocadas se nombran al final («no se actualizaron vistas de referencias externas: <lista>»)
```

Los textos siguen la convencion del Plugin de mensajes de linea de comandos sin acentos (`P/RackSelectivoCommands.cs:475`); su semantica
es la que fija el Coordinador. Los mensajes del redibujo los define el plan de G9a y los consume el plan de lote de G11.

**Esc y Enter detienen la cola** (OD-4): el jig acepta la respuesta vacia y cancela con `Cancel` o `None`
(`P/Drawing/BlockPlacement.cs:186-191`, `:219-229`); `PromptStatus.Error` es fallo. `PlaceAndReport` gana un parametro de prompt
(`P/Drawing/BlockPlacement.cs:26`, `:214`).

**`Regen`.** Uno en POST si la MUTATE confirmo alguna unidad; uno tras la colocacion que borro huerfanas en su jig; el Cantilever conserva
uno al final de la cola en lugar de uno por vista insertada (`P/RackCantileverCommands.cs:169`).

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

Como en I-52 V14 §9.1, la resolucion ocurre antes de pedir la linea o los puntos.
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
             ST-10 de I-52 V14 §4.1)
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

- **D-08a.** Nucleo neutral de seleccion de I-51 que I-52 V14 §5.1 propone extraer (`A/Persistence/RackDuplicationPlan.cs:225-582`).
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
| **INV-AUTH-1** | I-55 nunca **crea** una hermana nueva con authored divergente de una hermana propia del dibujo: una vista nueva solo se coloca tras confirmar la mutacion atomica de todas las hermanas mutables (redibujadas con el authored nuevo y huerfanas borradas en la misma transaccion) o, si no queda ninguna hermana que redibujar, con el borrado de las huerfanas en la transaccion de su jig. Residual: si tras cancelar ese primer jig falla la limpieza best effort de su definicion (D-10), queda una definicion sin referencias con el authored nuevo (R-25). Las dependientes de xref las gobierna su dibujo de origen (OM-25) | `RackSiblingRedrawPlanTests`, `RackSiblingRedrawRunTests`, `RackViewAuthoredEquivalenceTests` |
| **INV-AUTH-2** | Una hermana nueva nace con la coleccion canonica comun de su `RackId` (incluido el sobre fuente), o no nace | `RackSiblingCustomPropertiesGateTests` |
| **INV-RED-1** | Un fallo en PREPARE o en MUTATE del redibujo de Insertar no deja confirmado ningun subconjunto de hermanas con el authored nuevo: ni redefinicion, ni payload, ni borrado de huerfanas | `RackSiblingRedrawRunTests` |
| **INV-RED-2** | Ninguna vista nueva se materializa (ni definicion ni referencia) hasta que el redibujo requerido haya confirmado; si no hay redibujo requerido (`REDRAW_NOT_REQUIRED`), las huerfanas pendientes solo se borran junto con la referencia de la primera vista nueva | `RackSiblingRedrawRunTests`, `RackViewBatchPlanTests` |
| **INV-RED-3** | PREPARE no escribe estado del rack (solo importa definiciones de biblioteca) y rechaza payloads vacios; MUTATE no importa, no abre transacciones, no purga, no regenera, no renombra ni confirma por unidad, y descarta ante una excepcion o un fallo tipado; POST solo corre tras el commit y ninguna de sus excepciones se traduce en `REDRAW_ROLLED_BACK` ni cambia el authored | `RackSiblingRedrawRunTests`, `SiblingRedrawSeamGuardTests` |
| **INV-BATCH-1** | La atomicidad del redibujo no convierte los jigs posteriores en una transaccion global: cada colocacion confirma sola, y cancelar o fallar en la vista i conserva el redibujo y las vistas 1..i-1 | `RackViewBatchPlanTests`, `RackViewBatchDriverGuardTests` |
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
| ID17 despues / ID18 existente | variante | no disponible (sin corte lateral en ese poste, seccion invalida, estacion ausente) | `PREPARE_FAILED`: nada modificado |
| ID17 | tras crear la definicion | excepcion | definicion sin referencias borrada en ambas primitivas (D-10; hoy queda: `P/Drawing/BlockPlacement.cs:49-52`; `P/RackCantileverCommands.cs:151-157`, `:174-177`) |
| ID17/ID18 | materializacion | faltan bloques | se coloca y se reporta (vigente), con el reporte del requisito estructural (§4.6): ninguna pieza desaparece |
| ID17 despues / ID18 existente | gate de propiedades | tipos distintos, hermana ilegible (tambien por `Id` legible en un payload no interpretable), divergentes | **nada**, ni redibujo; motivo y remedio condicionado |
| ID17/ID18 | barrido | F-14a (UTF-16 invalido) | falla el barrido; nada modificado |
| ID17 despues / ID18 existente | preflight vigente | sobre, descriptor, interior I-11, catalogo de secciones o reconciliador | abort vigente de la edicion; nada modificado |
| ID17 despues / ID18 existente | PREPARE | fallo al preparar una vista nueva o una unidad (plan, payload vacio, `BlockId` nulo) | `PREPARE_FAILED`: ninguna escritura del rack; definiciones de biblioteca importadas pueden quedar (OM-5). La importacion en si no falla en voz alta (`P/Drawing/BlockLibraryImporter.cs:74-78`, `:113-118`): sus piezas ausentes se informan en POST |
| ID17 despues / ID18 existente | MUTATE | excepcion o fallo tipado en cualquier unidad (redefinicion, payload o borrado de huerfanas), p. ej. una referencia en una capa bloqueada (PLAUSIBLE) | `REDRAW_ROLLED_BACK`: sin commit; ninguna hermana redibujada ni borrada; ninguna vista nueva; el mensaje nombra la vista y el remedio (R-26) |
| ID17 despues / ID18 existente | MUTATE | ninguna unidad (solo dependientes de xref, o solo huerfanas diferidas) | `REDRAW_NOT_REQUIRED`: sin transaccion ni POST; los mensajes no dicen «vistas existentes actualizadas» |
| ID17 despues / ID18 existente | POST | fallo al purgar o renombrar (absorbido y registrado por las primitivas vigentes) o excepcion del `Regen` | `REDRAW_APPLIED`: purga y renombre solo en el log; aviso si falla el `Regen`; nunca `REDRAW_ROLLED_BACK`; la cola continua |
| ID17 despues / ID18 existente | clasificacion | hermana dependiente de xref | no se toca ni cuenta en los gates; el informe la nombra (OM-25) |
| ID17 despues / ID18 existente | primera colocacion | sin hermanas que redibujar y con huerfanas | con el jig en OK, la referencia nueva y el borrado confirman en la transaccion del jig y se regenera; si el jig no confirma, la definicion nueva (ya confirmada antes del jig) se retira con la limpieza vigente (best effort, D-10) y las huerfanas quedan; residual R-25 |
| ID18 | colocaciones | Esc o Enter en la vista i | `PLACEMENT_CANCELLED_PARTIAL_BATCH`: redibujo confirmado y vistas 1..i-1 permanecen; i..n no se crean; sin rollback |
| ID18 | colocaciones | excepcion o `PromptStatus.Error` en la vista i | `PLACEMENT_FAILED_PARTIAL_BATCH`: igual, con el motivo; la definicion de la vista i se retira con la limpieza vigente (D-10) |
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
| ID17 rack nuevo | fuera de la transaccion | definicion + jig o limpieza | como hoy |
| ID17 despues (rack existente) | preflights + variante + gate + clasificacion + PREPARE (vista nueva y unidades de hermanas) | **una** transaccion para todo el redibujo de hermanas y el borrado de huerfanas (o ninguna si no hay unidades); commit; despues definicion + jig de la vista nueva (que borra las huerfanas si no habia hermanas que redibujar) | uno en POST si la MUTATE confirmo algo; uno tras la colocacion que borra huerfanas |
| ID18 rack nuevo | todo antes de escribir | por vista | ninguno; Cantilever uno al final |
| ID18 rack existente | preflights + variante + gate + clasificacion + PREPARE (vistas nuevas y unidades de hermanas) | **una** transaccion para el redibujo de hermanas y el borrado de huerfanas (o ninguna si no hay unidades); commit; POST; despues, por vista, la transaccion de su definicion y la de su jig (el primer jig borra las huerfanas si no habia hermanas que redibujar) | uno en POST si la MUTATE confirmo algo; Cantilever uno mas al final de la cola |
| `RACKEDITAR` → Actualizar | vigente | vigente: una transaccion por vista (OM-3) | vigente |
| ID19 | resolucion, disponibilidad, marcos, validacion, gates y planes antes de los puntos; importacion y comprobacion 3 despues | una transaccion | ninguno |

## 16. Recursos

| Recurso | Veces |
|---|---|
| Catalogo; catalogo de secciones | 1 por comando; 1 si hay Cantilever |
| `Resolve`; authored serializado | 1 por `RackId` |
| Registro de variables | 1 |
| Barrido de sobres | ID19: 1 (SNAPSHOT, del que salen tambien los gates); edicion: los vigentes + 1 lectura de las entradas del gate y de la clasificacion por Insertar |
| Transacciones de escritura en Insertar (rack existente) | 0 o 1 para todo el redibujo de hermanas + 2 por vista nueva (definicion y jig) + 1 por renombre cosmetico en POST |
| `LockDocument` del redibujo | 1 alrededor de PREPARE, MUTATE y POST, como el precedente |
| `CommonTransform2D` | 1 por seleccion proyectada |
| Importacion de bloques | ID19: 1 con la union de claves; verificacion estructural por pieza |

## 17. Reconciliacion con iniciativas activas

### 17.1 I-49 (`c8cfee2`)

Desde G2, el Owner acepto ADR-0041, que reemplaza a ADR-0040, e I-49 versiono un Consensus Freeze nuevo sobre V6 + A1 + A2 (`3f17c21`;
`docs/automation/decisions/I-49.md` §15 en su rama). Despues publico la correccion G6-C2 (`75f1862`), solo en `A/Expressions/*` y sus
pruebas, con la que cerro G6. Durante la redaccion de V3 publico el Amendment A3 (`f6f0991`, solo documentacion: diagnosticos de un
simbolo con varias causas de fallo y su conjunto de causas raiz, dentro de `A/Expressions/*`); G7 sigue bloqueado antes de RED. Durante la redaccion de V4 publico A3-R1 (`c8cfee2`), la correccion de A3 tras la revision de su
Arquitecto, tambien solo documentacion dentro de su amendment. A3
precisa D19 y D20 de ADR-0041 (dato estable de `Upstream`, que consume el `PlanReadSet`); I-55 no lo implementa ni lo consume antes de que
I-49 integre, y `Resolve` lo consumira entonces sobre la autoridad final de I-49 (§4.5; I-52 V14 [V14-D07] registra el mismo efecto). Su produccion vive en `A/Expressions/*`, `A/Units/LengthUnits.cs` y
`A/StructuralSections/StructuralSectionUnits.cs`. **Sin archivos de
produccion comunes.** Archivo caliente: `U/Systems/Selective/RackSelectiveWindow.xaml.cs` (G10 de I-49; fila de I-53S,
`docs/ROADMAP.md:471`). Conflicto textual trivial en `docs/adr/README.md` (I-49 cambia las filas 0040/0041, contiguas a la de 0042).

### 17.2 I-52 (`e59bc89`, Proposal V14, sin consenso)

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
  - Archivos permitidos por gate (§14.2) y exclusiones (§20.3) sin cambio respecto de V12. Ninguna reclamacion nueva de extraccion o ubicacion.
- **V14 (`e59bc89`, publicada durante la redaccion de V4):**
  - [V14-D08] marca PROVISIONAL UNTIL CROSS-INITIATIVE RECONCILIATION las **ubicaciones** de G4..G7 de su §14.2 (`Persistence/`,
    `Mirror/`, `Application.Views`, `Plugin/Systems/Shared/`, ubicacion de `Resolve` + `Plan`, del primitivo y de `CreateInTransaction`);
    el contenido de las listas de archivos no cambia. Las exclusiones de §20.3 (`KindHandlers/*`, editores WPF) siguen **sin cambio**.
  - [V14-D14] registra a I-55 en `110cd39` con V3 (`8e35a51`): la revision de G2C como revision tecnica adversarial, la tabla aceptada del
    lado de I-55 y las diferencias de V3 (X-1, X-2, X-4, X-5, X-7, X-8) **no adoptadas, sin conflicto de contenido** (V14 §15.4); ADR-0042
    complementa a ADR-0010, con nota posterior solo si el Owner lo acepta.
  - [V14-D05] amplia el censo de sitios productores G-M24 a nuevas familias de API; [V14-D07] deja RS-3 PENDING hasta la autoridad final
    de I-49 tras A3.
  - V14 solo reclama **crear** una definicion en la transaccion del llamador (`CreateInTransaction`); cita
    `SystemBlockWriter.RedefineInTransaction` solo como precedente, y no nombra `PrepareRedraw` ni `RedrawInTransaction`.

**Siguen siendo de I-52**, aunque consuman un primitivo compartido: ST-13, ST-18, ST-19, ST-20, PRE-17, PRE-18, PRE-19, E12,
`MaterializationContextReadSet`, `VisualFootprintResult`, `GeometrySymmetryResult` y la equivalencia con `μ_k`.

**I-55 adopta la misma regla:** sin la reconciliacion registrada por ambas no hay Consensus Freeze de I-55, ni tampoco G3.

**Base.** La tabla de reconciliacion de G2C queda **aceptada del lado de I-55** por el Coordinador (G2D). V3 la contrasta con V12 y con
V13, que no la cambian materialmente, y le incorpora las autoridades nuevas de V3: `Resolve` (§4.5), el requisito estructural de bloques (§4.6), el
codec separado de la disponibilidad (§7.3) y los tramos por variante (§12.3). La revision adversarial de V3 (V3 §23) contrasto cada fila con
los archivos permitidos por gate y los excluidos de I-52, identicos en V12 y V13, y de ahi salen las reglas de extraccion de abajo.

**Diferencias de V3 con las filas de G2C que V13 y V14 §15.4 registran** (V14 las recoge sin adoptarlas, [V14-D14]; I-52 debe volver a acordarlas; V4 mantiene estas reglas):

| X | G2C (V14 §15.4) | V3 y V4 | Motivo |
|---|---|---|---|
| X-1 | extrae I-52 G4 (I-55 G13 solo si I-52 no lo extrajo) | primer gate I-52 G4 o I-55 G13; asignador de identidad solo en I-52 | Misma regla, simetrica |
| X-2 | extrae la primera (I-55 G6/G7 o I-52 G5/G6) | **extractor unico I-55** (G6, G7a, G7b) | El renombre y la delegacion tocan archivos que I-52 excluye (§20.3) |
| X-4 | primitivo de I-52 G6 | anade el requisito estructural puro (primer gate I-55 G7b o I-52 G6) y la consulta tras importar (primer gate I-55 G8 o I-52 G6) | Requisito estructural por pieza (CD2-04) |
| X-5 | protocolo antes de G3, G5, G7 y del Candidato | anade comprobaciones de I-52 antes de G4 y G6 | Reglas de extraccion |
| X-7 | valor de colocacion | anade la descomposicion de la transformacion fuente y la tolerancia de escala | Evitar dos clasificaciones de escala |
| X-8 | extrae la primera | **extractor unico I-55 G6** | Depende de la taxonomia de X-2 |

**Diferencias de V4 con V3 en esta tabla** (derivadas de CQ-01; I-52 debe releerlas por su §15.3):

| X | V3 | V4 |
|---|---|---|
| X-4 | Primitivo de crear en la transaccion del llamador | Dos operaciones de la misma familia: **crear** (la de X-4, I-52 G6) y **redefinir** (existente, I-47 G9, que consume el redibujo atomico de I-55 G9a); sitios nuevos del redibujo atomico a declarar en G-M24 |
| X-5 | Reporte en G9 | Reporte en G9a y G9b; disparador de V14 §15.3 por rutas de materializacion |
| X-6 | Productor G9 | Productor G9b; cambia el mapa de invocaciones G-M9/CT-26 de Insertar; C2-2b con redibujo todo o nada |

**Principio:** una autoridad por responsabilidad, extraida una sola vez, con politicas por llamador. Dos reglas de extraccion:
- **Extractor unico:** cuando extraer exige archivos que una iniciativa excluye (V14 §20.3), o la autoridad depende de otra de extractor
  unico, extrae solo la otra. Los archivos y ubicaciones por gate (V14 §14.2, provisionales por [V14-D08]) no fijan la regla: se ajustan
  en la tabla de adopcion.
- **Primer gate:** en los demas casos extrae la primera iniciativa que llega a su gate de creacion, siempre sobre la especificacion
  compartida registrada aqui; la otra consume y no crea una segunda.

En ambos casos, antes de cada gate que crea o consume una autoridad compartida, la iniciativa comprueba si ya existe en su base (X-5). Si
un gate necesita una autoridad que todavia no existe y no le toca extraerla: **STOP** de ese gate y decision de los dos Coordinadores.

**Restricciones de I-52 que fijan las reglas.** No provisional: los archivos excluidos de §20.3 (entre ellos `KindHandlers/*` y los
editores WPF), identicos en V12, V13 y V14. Provisionales desde V14: las ubicaciones de G4..G7 (G4: `Geometry/*` y `Persistence/`; G5:
`Mirror/`, `Systems/<Kind>/` y `RackFrames/`; G6: autoridades de plan por kind, `SystemBlockWriter.cs` y `Plugin/Systems/Shared/`).

| X | Autoridad I-52 (V14) | Autoridad I-55 (V4) | Nucleo compartido | Politica I-52 | Politica I-55 | Custodio / extrae | Gate que la crea | Orden de consumo | Riesgo | Veredicto de compatibilidad |
|---|---|---|---|---|---|---|---|---|---|---|
| **X-1** Seleccion | Nucleo neutral + fachada `RackDuplicationPlan` (§5.1, provisional); SNAPSHOT propio (§5.2); asignador de identidad con politica de nombre inyectada (T-M05) | D-08a; SNAPSHOT propio (§12.1) | Clasificacion de fuentes de I-51 con predicado de kind inyectado, clave `RackId`/`Definition(handle)`, agrupacion y deduplicacion conservando referencias, consistencia de `Kind` y nombre, mensajes inyectados; **una caracterizacion: CT-16** (provisional en V13 y V14) | ST-* (payload antes del filtro de Model Space; `Origin ≠ 0` falla; `Id` en blanco → clave de definicion); el asignador de identidad es de I-52 y se queda en su G4 | Orden de I-51; `Id` en blanco falla (RID-4); kind sensible a mayusculas; sin asignador de identidad | Custodio I-52; **primer gate**: I-52 G4 o I-55 G13, con la misma especificacion | I-52 G4; I-55 G13 | CT-16 → creador → el otro | MEDIO | **COMPATIBLE** |
| **X-2** Taxonomia, codec, disponibilidad, `Resolve`, `Plan` | §3.7/§3.8 (codec y `A_k`), §6.1 `DescribeView`/`AdmissibleViews`, §8.1 `Build(request)` (que ya registra `Build` = `Resolve` + `Plan`), §8.4 (nucleo de decodificacion fuera de `Mirror/`) y §20.1: provisionales en V13 y V14; CT-04 provisional | `RackViewKind`; `RackViewAddressCodec` (sintaxis); `RackViewAvailability`; `RackSystemResolution` (§4.5); `RackViewPlanner` y `Prepare` (§4.4) | En `A/Views` y `A/Views/Resolution`: taxonomia; `Decode` sintactico; disponibilidad; `Resolve` con resultado tipado y contexto con el resultado de la lectura del registro; `Plan(sistema, direccion, contexto con semilla)` con todas las direcciones, la planta Cantilever con `PlantaVisibility` y nombre sugerido; **una caracterizacion: CT-04** | σ' canonica; fallo cerrado de `Coerced`/`Invalid`/`Orphaned`; `Expone`, `F`, derivacion de `c`, filtro μ_k; `A_k` = disponible ∩ expuesto; `Build` = `Resolve` + `Plan` sobre el diseno reflejado en memoria (C2-1 intacto); PRE-06/07/08/11 como politica | `RACKEDITAR` sin cambio; ID19 acepta `Canonical`/`Canonicalizable` y `Available`; editores llaman a `Plan` con su sistema; ID19 solo `Resolved`; BOM por kind | Custodio I-55; **extractor unico I-55**: el renombre toca editores WPF y la delegacion toca `KindHandlers/*` (V14 §20.3, no provisional); las ubicaciones de G4..G6 de I-52 son provisionales ([V14-D08]) | I-55 G6 (taxonomia, codec, disponibilidad, soporte, `RackViewFrame`), G7a (`Resolve` + delegacion de los seis handlers), G7b (`Plan`, `Prepare`) | CT-04 → I-55 G6/G7a/G7b → I-52 G4 (codec, ST-15), G5 (disponibilidad, marco, `Resolve` en P3/P4, `DescribeView`), G6 (`Build`). Si I-52 llega antes: STOP de ese gate y los Coordinadores eligen (a) adelantar el gate de I-55, sin cambio observable (solo posible tras el G3 de I-55, que exige su consenso, M-01, ADR-0042 y OD-1..OD-8), o (b) que I-52 extraiga con una excepcion registrada de su §14.2 y §20.3 que cubra los mismos archivos, la misma especificacion y el renombre y la delegacion en el mismo gate | ALTO: acopla el orden de gates de I-52 a I-55 | **COMPATIBLE WITH CLARIFICATION** (I-52 debe adoptar el acoplamiento) |
| **X-3** Comparador authored | `IsSameAuthority` por kind, declarado por su reflector (§5.4, §6.1 y §8.5, provisionales en V13 y V14); V14 §15.4 ya registra como pendiente «comparador en el contrato del kind; el reflector lo referencia» | `RackSiblingAuthoredGate` (§9.3) | `IsSameAuthority(miembros)` por kind en el **contrato del kind** (`Systems/<Kind>/`); el `IsSameAuthority` del reflector **delega** en el; Selectivo `SelectiveAuthoredAuthority.IsSameAuthority`; excluye la metadata de `P/RackCommandSupport.cs:40-67` | Miembros = vistas seleccionadas; E3 | Miembros = todas las hermanas; falla antes de PICK | Custodio I-52; **primer gate**: I-52 G5 o I-55 G14 | I-52 G5a/b/c; I-55 G14 (consume si ya existe) | Creador → el otro delega | MEDIO | **COMPATIBLE** |
| **X-4** Primitivo de materializacion y requisito estructural | §8.2 materializador (provisional): `CreateInTransaction(db, tx, plan, nombreBloque, payload)`, con `SystemBlockWriter.RedefineInTransaction` citado solo como precedente; PREPARE con importacion y re-verificacion por instancia (§8.1) | Consumo en G15; requisito estructural (§4.6); redibujo atomico de hermanas (§11.2) | **Dos operaciones de la misma familia, sin mezclar:** (1) **crear** una definicion nueva en la transaccion del llamador — la de X-4, que extrae I-52 G6 y consume I-55 G15; (2) **redefinir** una definicion existente en la transaccion del llamador — ya existe en `main` (`SystemBlockWriter.RedefineInTransaction`, I-47 G9; `CantileverViewMaterializer.RedefineBlock`), ninguna iniciativa la extrae, I-55 la consume sin modificarla e I-52 V14 solo la cita como precedente. Primitivo de crear: despacho por familia de plan + `RackBlockData.Write` + referencia con Z y presentacion explicitas; sin lock, commit, regeneracion ni purga. Requisito: parte pura en `A/Drawing/LibraryBlockRequirement.cs` (requisito por pieza, predicado de nombre); consulta a la tabla de bloques en el Plugin | ST-13, ST-19, ST-20, PRE-17, PRE-18, PRE-19, read-set y huellas en el llamador | Presentacion de creacion (OD-8 A); `Z_r`; escala 1; fallo antes de escribir; ID17/ID18 conservan colocar y reportar | Primitivo: custodio I-52, extrae I-52 G6, I-55 G15 consume (extrae solo si llega antes, con la firma acordada). Requisito puro: **primer gate**, I-55 G7b o I-52 G6. Consulta tras importar (`P/Systems/Shared/`): **primer gate**, I-55 G8 o I-52 G6 | I-52 G6; I-55 G7b (requisito), G8 (consulta), G15 (primitivo) | Creador → el otro consume | MEDIO: `SystemBlockWriter.cs`, `LateralHeaderDrawer.cs` y `CantileverViewMaterializer.cs` en la lista STOP de I-52 (V14 §19): I-55 G9a solo los llama; G-M24 con modo de creacion y con los sitios nuevos del redibujo atomico; I-52 G6 necesita `A/Drawing/LibraryBlockRequirement.cs` en sus archivos | **COMPATIBLE WITH CLARIFICATION** |
| **X-5** Protocolo | §15.2, §15.3 (antes de G3, G5, G7 y del Candidato; re-evaluacion cuando un gate de I-55 toque rutas de materializacion, `SystemBlockWriter.cs` o comandos), §19 (lista STOP de archivos); G4 solo avisa | Reporte en G5, G6, G7a, G7b, G8, G9a, G9b, G10, G12, G13, G14, G15 | Sin autoridad productiva: obligacion de comprobar si la autoridad compartida existe y de reportar antes de fijar archivos | Re-evaluar §8.1, §8.7, read-set, PRE-17, PRE-18, T-GRD-02, censo `B`; **comprobacion tambien antes de G4 y G6** (adopcion) | Reporte con la lista de re-evaluacion; nada congela una autoridad incompatible | — | — | Cada gate listado comprueba y reporta antes de fijar archivos | BAJO | **COMPATIBLE WITH CLARIFICATION** |
| **X-6** Lineas base de C-2 | C2-1 = C2-2a = C2-2b = C2-3 = C2-4 y G-M9 (§8.5, provisional en comparador y lineas base) | Cambia productores: G5 (PR-2), G8 (re-enrutado de Insertar), G9b (gate, variante, redibujo atomico de hermanas y huerfanas), G12 (orden de edicion); el mapa de invocaciones G-M9/CT-26 de «`EditX` (Actualizar e Insertar)» cambia en Insertar | **Un unico juego** de fixtures y un mapa G-M9 sobre el `main` combinado; fixture de planta Cantilever con brazos y tensores visibles | C-2 por defecto; C-1 solo condicional | Goldens de G7b y fixtures de Insertar de G8; la precondicion de C2-2b «redibujo previo sin fallos» pasa a ser todo o nada | Custodio I-52; la segunda en integrar re-establece C-2 | I-52 G6; re-establecimiento en los gates de la segunda | La segunda corre C-2 y G-M9 antes de su Candidato | ALTO: requiere que el `Plan` compartido reciba `PlantaVisibility`, para que PR-2 siga solo en el Plugin en cualquier orden | **COMPATIBLE** |
| **X-7** Valor de colocacion y transformacion fuente | `Transform2D.ReflectionAboutLine` + `(Position2D, RotationRadians, UniformScale)` (§3.3 y §8.3 provisionales en V13 y V14 para nombre, ubicacion y API del valor; el algebra de reflexion y §3.2 siguen de I-52); clasificacion pura ST-7/8/9/10 y canonizacion `(−s, −s)` → `θ + π` (T-M03, G4); tolerancia de escala fijada en G4 (§4.3) | `CommonTransform2D`; colocaciones (Position, ρ, 1); escala y signos (§12.2, D-08f) | `Transform2D` + **un** valor de colocacion con su composicion y su descomposicion validada; `T_common` = (T − R(α)·B, α, 1); Z y `Origin` fuera del valor; **una** descomposicion pura de la transformacion fuente y **una** tolerancia de escala | Reflexion; escala `s ≠ 1` admitida (ST-11); `Origin ≠ 0` falla (ST-14); Z de la fuente | Rigidez (`det = +1`, escala 1); `Origin` dentro de `M_r`; Z por politica | Custodio I-52 (`A/Geometry/`); **primer gate**: I-52 G4 (que exige ADR-0036 aceptado) o I-55 G14 | I-52 G4 (T-M01..T-M03); I-55 G14 | Creador → el otro consume la misma especificacion | BAJO-MEDIO | **COMPATIBLE** (§3.2 consumido sin cambio) |
| **X-8** Origen y tramo del eje | «tramo del eje de la vista (para c)» en `ViewPlanResult` (§8.1, provisional); `c` en coordenadas locales de la vista (§6.1); CT-05 (provisional en V13 y V14), CT-06 | `RackViewFrame` con `[K_min, K_max]` por variante (§12.3) | **Un** descriptor por (sistema, tipo, variante): mapa de ejes con signo, origen fisico local, `[K_min, K_max]` en coordenadas fisicas, convencion de extremos; conversion a `c` a traves del origen; **una caracterizacion: CT-05** | `c` derivada del tramo convertido a coordenadas de la vista; δ registrado (CT-06); solo vistas admisibles | Anclas por extremo del tramo destino segun σ | Custodio I-55; **extractor unico I-55 G6** (depende de la taxonomia de X-2) | I-55 G6 | CT-05 → I-55 G6 → I-52 G5/G6 e I-55 G14 | BAJO-MEDIO | **COMPATIBLE WITH CLARIFICATION** |

**Contraste con V12, V13 y V14.** Ninguna fila contradice el contenido de las secciones no provisionales de I-52 (en V14, §3.2, §5.2 y
§20.3 entre las citadas): las autoridades nuevas se construyen debajo de ellas sin cambiar su semantica. Las aclaraciones de V3 afectan a
partes provisionales (en V14: §3.3, §3.7, §3.8, §5.1, §5.4, §6.1, §8.1..§8.5, §14.2 G4..G7, §20.1, CT-04, CT-05 y CT-16) y al **proceso**
de I-52: comprobaciones antes de G4 y G6, un archivo mas en G6 y el acoplamiento de G4..G6
a los gates de I-55 que extraen X-2. CQ-01 no cambia ninguna autoridad X: el redibujo atomico consume la operacion existente de redefinir
y no la de crear que reclama I-52 (X-4), y solo anade disparadores de reporte (X-5) y un cambio de mapa de invocaciones (X-6). **No hay
MATERIAL CONFLICT de contenido.** El acoplamiento de orden (X-2) exige que I-52 lo adopte;
si su Coordinador o su Arquitecto lo rechazan, es un conflicto que se escala (abajo).

#### 17.2.1 Tabla de adopcion para el registro de decisiones de I-52

Para copiar al registro de I-52 si su Coordinador y su Arquitecto la aceptan. I-55 no escribe en la rama de I-52.

| X | Autoridad | Ubicacion propuesta | Custodio | Extrae | I-52 consume en | I-55 consume en | I-52 conserva como politica propia | I-52 ajusta en su contrato | Estado |
|---|---|---|---|---|---|---|---|---|---|
| X-1 | Nucleo neutral de seleccion | `A/Persistence/` + fachada `RackDuplicationPlan` | I-52 | primer gate: I-52 G4 / I-55 G13 | G4, G7 | G13, G14, G15 | ST-*; asignador de identidad (T-M05, G4) | Una caracterizacion CT-16 compartida | Aceptada por I-55 (G2C/G2D); pendiente de I-52 (V14 §15.4 recoge la fila de G2C) |
| X-2 | Taxonomia `RackViewKind`; codec sintactico; disponibilidad; `Resolve`; `Plan`; `Prepare` | `A/Views`, `A/Views/Resolution` | I-55 | **solo I-55**: G6, G7a, G7b | G4 (codec para ST-15), G5 (disponibilidad, marco, `Resolve` en P3/P4, miembros de `DescribeView`), G6 (`Build` = `Resolve` + `Plan`) | G6..G15 | σ', exposicion, μ_k, `A_k` como filtro, fallo cerrado, PRE-06/07/08/11 | §3.7, §3.8, §8.1, §8.4 y §20.1 remiten a `A/Views`. §6.1 sin cambio: sus miembros se construyen sobre codec, disponibilidad, `RackViewFrame` y `Resolve`. §9.1: P1 conserva la decodificacion sintactica y la disponibilidad (`Orphaned`, E4) se evalua con el sistema resuelto, en P4. PRE-06 usa el resultado de la lectura del registro que `Resolve` recibe en su contexto. `UnsupportedLegacy` → E6 (propuesta). §14.2: precondicion «la pieza de X-2 existe en la base» en G4, G5 y G6, o la excepcion (b) de §17.2 | Aceptada por I-55; pendiente de I-52 |
| X-3 | `IsSameAuthority(miembros)` por kind | contrato del kind (`Systems/<Kind>/`) | I-52 | primer gate: I-52 G5 / I-55 G14 | G5 | G14 | miembros = vistas seleccionadas; E3 | V14 §15.4 ya lo lista como pendiente con el mismo contenido: el `IsSameAuthority` del reflector delega en el comparador del contrato del kind | Aceptada por I-55; pendiente de I-52 |
| X-4 | Primitivo de materializacion; requisito estructural de bloques | Plugin `Systems/Shared/` (primitivo y consulta tras importar); `A/Drawing/LibraryBlockRequirement.cs` | I-52 | primitivo: I-52 G6; requisito puro: primer gate, I-55 G7b / I-52 G6; consulta: primer gate, I-55 G8 / I-52 G6 | G6, G7 | G7b, G8, G15 | ST-13, ST-19, ST-20, PRE-17, PRE-18, PRE-19, read-set, huellas | §8.2: Z y presentacion explicitas; requisito por pieza; la operacion de redefinir en la transaccion del llamador no es parte de X-4 y no se extrae; G-M24 declara los sitios del redibujo atomico de I-55; §14.2 G6 anade `A/Drawing/LibraryBlockRequirement.cs` | Aceptada por I-55; pendiente de I-52 |
| X-5 | Protocolo de comprobacion y reporte | — | ambas | — | §15.3 anade G4 y G6 | cada gate listado | — | §15.3: comprobacion de existencia antes de G4 y G6 | Vigente en I-55; ajuste pendiente de I-52 |
| X-6 | Juego unico de C-2 y mapa G-M9 | pruebas | I-52 | I-52 G6 | G6 | G5, G7b, G8, G9b, G12 (re-establece si integra segunda) | C-1 condicional | Fixture Cantilever con visibilidad de planta; C2-2b con las precondiciones nuevas de Insertar (variante antes, gate, D-15, huerfanas) | Aceptada por I-55; pendiente de I-52 |
| X-7 | Valor de colocacion sobre `Transform2D`; descomposicion de la transformacion fuente; tolerancia de escala | `A/Geometry/` | I-52 | primer gate: I-52 G4 / I-55 G14 | G4 | G14 | reflexion; escala `s ≠ 1`; `Origin ≠ 0` falla | §3.3 y §8.3 (provisionales en V13 y V14) remiten a la especificacion compartida; §3.2 sin cambio | Aceptada por I-55; pendiente de I-52 |
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
| Redibujo de hermanas | PREPARE → una transaccion MUTATE para todos los redibujos y borrados de huerfanas → POST | **Elegida** (CQ-01, G2E) |
| Redibujo de hermanas | Commit por unidad con STOP y sin rollback (V3, CD2-03) | **Sustituida** por CQ-01: dejaba un subconjunto de hermanas confirmado con el authored nuevo |
| Redibujo de hermanas y colocaciones | Una sola transaccion que incluya tambien los jigs | **Rechazada**: mantendria sin confirmar el redibujo y todas las vistas colocadas a lo largo de varias interacciones del usuario, y convertiria Esc en rollback de vistas ya colocadas (INV-BATCH-1). Cada jig ya tiene su propia transaccion, que confirma sola |
| Renombre en MUTATE | Seam nuevo de renombre en la transaccion del llamador | **Rechazada**: el nombre es cosmetico por contrato vigente; en POST no corre si hay rollback (§11.1; OM-27) |
| Extender `ProjectVariableMutationExecutor` | Reusar el ejecutor de I-47 para Insertar | **Rechazada en I-55**: su delegado esta tipado a `PreparedViewRedraw` y a la escritura del registro; convergencia futura (OM-26) |
| Verificacion de bloques | Union de nombres distintos (V2) | **Rechazada** (AR2-03): una pieza sin nombre desaparece de la union |
| Resolucion sin editor | «Usar el camino del BOM» tal cual (V2) | **Rechazada** (AR2-04): el handler no expone el sistema ni el resultado tipado. (El ejemplo de legado aceptado por el BOM y no por el editor que citaba la revision tecnica de V2 es inalcanzable, §4.5) |
| Codec | Codec que consulta el sistema resuelto (V2) | **Rechazada** (AR2-05): mezcla sintaxis y disponibilidad e impide compartirlo |
| Nombre y plan | Plan que necesita el nombre unico | **Rechazada**: introduce I/O de dibujo en el plan; se usa una semilla (§4.4) |

## 19. Pruebas (resumen; matriz en el mapa §T)

| Clase | Clases previstas |
|---|---|
| **Caracterizacion (G3)** | `RackViewEnvelopeReadingCharacterizationTests` (= CT-04 de I-52), `LegacyViewPayloadCompositionCharacterizationTests`, `LegacyViewBlockNameCharacterizationTests`, `RackCountInvariantCharacterizationTests`, `RackSiblingScanInputCharacterizationTests`, `RackViewFrameCharacterizationTests` (= CT-05, con tramos por variante), `HeadlessResolutionParityCharacterizationTests` (paridad por kind de `Resolve` frente al editor y al sistema dibujado, con fixtures legacy como el Dinamico solo-sistema), `RackEditPreflightCharacterizationTests` (preflights de hermanas de `RACKEDITAR`), `BuilderCatalogSkipCensusTests` (censo de piezas que los builders omiten sin bloque en el catalogo), `BomResolutionCharacterizationTests` (BOM y motivos de bloqueo vigentes por kind), `LibraryBlockRequirementCharacterizationTests` (roles que requieren bloque en la salida de los builders), `FirstViewGateCharacterizationTests`; condicional `RackDuplicationPlanObservableSemanticsTests` (= CT-16) |
| **T0** | `PushBackLateralPostIndexRegressionTests`, `FirstViewFreedomTests`, `EditorViewBatchRequestTests`, `RackViewBatchDialogTests`, `RackSelectionCoreConsumptionTests` (si I-52 ya extrajo el nucleo), `RackViewKindRenameTests`, `RackViewVariantTests`, `RackViewAddressCodecTests` (solo sintaxis, §7.3 A), `RackViewAvailabilityTests` (§7.3 B), `RackViewSupportMatrixTests`, `RackViewFrameTests`, `RackSystemResolutionTests` (resultados tipados y legado), `RackViewNameSeedTests`, `RackViewPlannerTests`, `LibraryBlockRequirementTests`, `RackViewPreparation*Tests`, `PreparedViewPayloadGoldenTests`, `RackIdLifecycleTests`, `RackEnvelopeIdProbeTests`, `RackSiblingCustomPropertiesGateTests`, `RackSiblingAuthoredGateTests`, `RackViewAuthoredEquivalenceTests`, `RackSiblingRedrawPlanTests` (G9a: conjuntos mutable y de solo lectura, huerfanas y su ubicacion, orden, resultados), `RackSiblingRedrawRunTests` (G9a: PREPARE que falla sin escrituras, MUTATE que falla sin commit, un commit, POST que falla sin rollback, authored igual o sin cambio), `RackSiblingRedrawUnits{Selective,Dynamic,PushBack,Cantilever,Cabecera}Tests` (G9a, en Application: descriptor tipado por vista y kind, peticion de plan y payload igual al vigente), `RackViewBatchPlanTests` (redibujo atomico, variante, Esc y Enter tras el commit), `CommonTransform2DTests`, `RackGroupFrameTests`, `RackRigidPlacementPolicyTests`, `RackOrthographicPlacementPolicyTests` (los ocho pares de §12.5, ida y vuelta con lo que restituye y lo que no, tramos, superposicion con `ε_ov`, sentido por mayoria, empate en la frontera, INV-GRP-3 e INV-GRP-4 sobre la colocacion real, las ocho combinaciones de signo de escala), `RackGroupPlacementPlanTests` (politicas de consumidor de ID19), `RackViewCountInvariantTests`, `RackViewPartialRackContractTests`, `CantileverPlantaVisibilityBuilderTests` |
| **T1** | `*DimensionView*`; `CustomPropertiesEnvelopeGuardTests`; `CustomPropertiesEdgeGuardTests` (sin cambio); `KindHandlerResolutionDelegationGuardTests`; `RackViewPlacementGuardTests`; `RackViewBatchDriverGuardTests`; `RackViewEntryPathGuardTests`; `RackProjectionCommandGuardTests`; `RackSiblingGateWiringGuardTests`; `SiblingRedrawSeamGuardTests`; `PushBackRoundTripSourceGuardTests`, `LinkedPropertyViewPayloadTests` y `CantileverPluginSourceGuardTests` (reapuntadas); `RackSiblingGateIndependenceGuardTests`; `CantileverPlantaVisibilityGuardTests`; `CustomPropertiesCommandGuardTests`; `SelectiveEditorOpenTests`; `CantileverPluginSourceGuardTests`; `SelectiveAuthoredCarrierAdoptionTests`; `PushBackPluginSourceGuardTests`; `RackUnitsGuardSourceTests`; `RackDuplicationPlanTests`; UI: `RackInsertionRequestTests`, `RackEditorSessionTests`, `EditorModuleRegistryTests`, `DynamicShellMigrationTests`, `RichEditorBlockedActionTests`, `DynamicHeaderBatchSeamGuardTests`, `SelectiveHeaderBatchSeamTests`, `WindowCensusGuardTests`, `DialogWindowCharacterizationTests`, `CustomPropertiesHelpCensusTests` |
| **T2/T3/T4** | suites completas; CI y coberturas; validacion del Owner |

## 20. Validacion del Owner (diseno; no ejecutar)

Mapa §OV:
- OV-LEG.
- OV-ID17.
- OV-RED (G9b, vista unica, en los cinco kinds): rack con frontal, lateral y planta → editar → Insertar una vista: las hermanas existentes
  cambian juntas antes del jig; Esc en el jig conserva el redibujo; una capa bloqueada provoca `REDRAW_ROLLED_BACK` sin cambiar ninguna
  hermana; Esc en el prompt de variante sin modificar nada; rack sin hermanas que redibujar con huerfanas y Esc en el jig.
- OV-ID18 (G12, lote): ninguna vista nueva aparece antes del redibujo; Esc en el primer jig conserva el redibujo; Esc en el segundo conserva
  el redibujo y la primera vista nueva.
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

## 21. Gates (resumen; detalle y tabla V3 → V4 en el mapa)

| Gate | Objetivo | Precondicion |
|---|---|---|
| G3 | Caracterizacion | Coordinator y Architect `AGREED` sobre la misma version + **M-01 resuelta** + reconciliacion con I-52 registrada ([V11-D10], [V12-D11], [V13-D08], [V14-D08]) + Consensus Freeze + ADR-0042 aceptado + OD-1..OD-8 |
| G4 | PR-1 | G3 |
| G5 | PR-2 (solo Plugin) | G3 |
| G6 | Foundation pura: taxonomia, codec sintactico, disponibilidad, soporte, `RackViewFrame` con tramos | G4, G5 |
| G7a | `Resolve` compartido + delegacion de los handlers del BOM (seis kinds) sin cambio observable; extractor unico (X-2) | G6 |
| G7b | `Plan` con semilla, `Prepare`, requisito estructural de bloques (parte pura) | G7a |
| G8 | Colocacion de vista unica + D-10 + prompt + fixtures de Insertar + reporte estructural en todo camino de insercion | G7b |
| G9a | Seam de redibujo atomico: plan puro, ejecucion secuenciada con puerto, unidades `HeaderRun`, `CantileverCurves` y `Erase`, envoltorios de Dinamico, Push Back y planta de cabecera, adaptador; **sin cablear** (sin cambio observable) | G7b (y G5 para el plan Cantilever con `PlantaVisibility`) |
| G9b | Precondiciones nuevas de `RACKEDITAR` → Insertar (vista unica): variante antes, gate de propiedades, clasificacion, PREPARE, redibujo atomico (D-15), huerfanas e informe | G8, G9a |
| G10 | ID17 | G9b |
| G11 | ID18 contrato puro (estados `VARIANT_CANCELLED`, `SIBLING_GATE_FAILED`, `PREPARE_FAILED`, `REDRAW_ROLLED_BACK`, `REDRAW_APPLIED`, `REDRAW_NOT_REQUIRED`, `PLACEMENT_CANCELLED_PARTIAL_BATCH`, `PLACEMENT_FAILED_PARTIAL_BATCH`, `COMPLETED`) | G10 |
| G12 | ID18 UI, presentador, driver (variantes, gate, PREPARE, redibujo atomico con el seam de G9a, cola con Esc y Enter) y rutas de entrada | G11 |
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
| D-06 | ID18 transaccion C: PREPARE ALL + redibujo atomico de hermanas (D-15) + commit por colocacion | §11 | **A-3** |
| D-07 | Gates acotados al `RackId` (propiedades con sonda de `Id`; authored) | §9 | **A-7**, X-3 |
| D-08 | Seleccion, grupos, clasificacion y SNAPSHOT completo (D-08a..h) | §12.8 | **X-1**, Arquitecto |
| D-09 | PR-1 y PR-2 (solo Plugin) antes de la foundation | §17.4 | Coordinador |
| D-10 | Limpieza ante excepcion: solo definiciones sin referencias, best effort y registrada; purga de anidadas declarada | §14, §22.1 detalle | Arquitecto |
| D-11 | Presentacion de creacion | §12.9 | OD-8 |
| D-12 | ADR-0042 **complementario** de ADR-0010 (A-8 = a) | ADR, §22.1 detalle | **A-8**, Owner |
| D-13 | **Requisito estructural de bloques por pieza** (instancias del plan), fallo antes de escribir en ID19 e I-52 (comprobaciones puras antes de los puntos); ID17/ID18 conservan colocar y reportar, con reporte en todo camino | §4.6 | Arquitecto, X-4 |
| D-14 | Una autoridad por responsabilidad con I-52 | §17.2 | **X-1..X-8** |
| D-15 | **Redibujo atomico de hermanas antes de Insertar** (CQ-01): variante antes; clasificacion mutable / solo lectura; PREPARE sin escrituras del rack; una MUTATE con todos los redibujos y los borrados de huerfanas; un commit; POST con purga, renombre y un `Regen`; solo despues, colocacion vista a vista con su propio commit (seam en G9a; cableado de vista unica en G9b; lote en G12). Actualizar sin cambio | §11.1-§11.4 | Arquitecto + Coordinador |
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
no se modifica. El reemplazo, completo o acotado, no se usa. I-52 V14 registra la misma relacion ([V14-D14]: complementa a ADR-0010,
que sigue aceptado, con nota posterior fechada solo si el Owner lo acepta). Hay precedentes de complemento (ADR-0028, ADR-0029) y de nota que enlaza un
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
- **Reconciliacion con I-52: X-1..X-8** (§17.2). Compatible con V12, V13 y V14 con las aclaraciones de V3 (extractor unico de X-2 y X-8, reglas de primer
  gate en X-1, X-3, X-4 y X-7, y ajustes de gates de I-52). Queda abierta hasta que I-52 la adopte en sus registros (tabla de §17.2.1);
  ninguna de las dos congela antes.
- **CQ-01 — cerrada por el Coordinador en G2E** (cambiar a PREPARE → una MUTATE → POST). V4 la incorpora (§0.2, §11); no queda material
  abierto propio salvo M-01.

### 22.4 Open Minor

| # | Tema |
|---|---|
| OM-1 | `SaveToLibrary` acuna antes en Cantilever y Push Back |
| OM-2 | Convergencia futura de `CantileverViewKind` (enum de camara, §7.1) |
| OM-3 | `RACKEDITAR` → Actualizar conserva su redibujo vista por vista y puede quedar redibujado en parte (vigente); adoptar el seam atomico de G9a en Actualizar queda fuera de alcance |
| OM-4 | Uniformar la eleccion de variante en vista unica |
| OM-5 | Definiciones de biblioteca importadas en PREPARE que sobreviven a un `PREPARE_FAILED`, a un `REDRAW_ROLLED_BACK` o a una colocacion cancelada (vigente; el importador trabaja fuera de la transaccion) |
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
| OM-25 | Las definiciones dependientes de xref son el conjunto de solo lectura: Insertar no las redibuja ni las cuenta en D-15, en los gates ni en INV-AUTH-1; quedan con el diseno anterior hasta recargar la xref (§9.2, §11.1) |
| OM-26 | `ProjectVariableMutationExecutor` y el seam atomico de Insertar tienen la misma forma PREPARE → MUTATE → POST; su convergencia queda para despues (§11.2) |
| OM-27 | Los renombres cosmeticos corren en POST: un fallo de renombre tras el commit deja el nombre anterior sin afectar al authored (§11.1) |
| OM-29 | El borrado en transaccion del llamador de I-55 y `RackCommandSupport.EraseViewBlocks` conviven; unificarlos cambiaria la purga de Actualizar y queda para despues (§11.2) |
| OM-28 | Las primitivas vigentes de purga y renombre absorben sus errores y solo los registran: POST no puede avisar de ellos al usuario sin modificar archivos de la lista STOP de I-52 (§11.1) |
| OM-24 | En `Orthographic`, el sentido del eje comun lo decide la mayoria de referencias: girar racks puede invertir el orden de toda la elevacion (§12.5). Una normalizacion fija se descarto en V2 (Proposal V2 §23, hallazgo 26); se pondera dentro de M-01 |

Retirados de V1: **OM-11** (`Id` interior del Cantilever desalineado) sale con H-13 (§22.5); **OM-16** (sin aviso de propiedades
divergentes al insertar) lo resuelve el gate de §9. OM-12, OM-13 y OM-15 conservan su tema, reformulado.

### 22.5 Hallazgos registrados fuera de I-55

**H-13 — `Id` interior del Cantilever sin alinear al crear.**
- **Evidencia:** `D/Systems/Cantilever/CantileverLineDesign.cs:333`; `U/Systems/Cantilever/RackCantileverWindow.xaml.cs:199-210`;
  `P/RackCantileverCommands.cs:472-498`.
- Solo lo leen pruebas: `T/CantileverLineTests.cs:908` y `T/CantileverRoundTwoCharacterizationTests.cs:372`.
- Candidato a iniciativa independiente; I-55 no lo toca.

**H-16 — La captura exterior de `ProjectVariableMutationExecutor` puede informar «nada escrito» tras un commit.**
- **Evidencia:** POST (`P/ProjectVariableMutationExecutor.cs:292-293`) corre dentro del `try` cuya captura devuelve `Aborted`
  (`:299-303`), definido como «Nothing was written» (`:24-25`); una excepcion del `Regen` despues del commit se informaria como aborto.
- I-55 no lo corrige ni lo copia: su POST se captura aparte (§11.1).

**H-15 — Nombres de las vistas Cantilever distintos al insertar y al editar.**
- **Evidencia:** la insercion nombra la definicion con prefijo, vista en mayusculas y saneado
  (`P/RackCantileverCommands.cs:152` → `P/Drawing/Cantilever/CantileverViewMaterializer.cs:287-295`); la primera edicion la renombra a
  «<nombre> - frontal» (`P/RackCantileverCommands.cs:334` → `P/Drawing/RackBlockRenamer.cs:30`).
- Cosmetico; I-55 no lo corrige y V4 conserva los renombres vigentes en POST.

**H-14 — El editor Dinamico no es idempotente al releer la tarima.**
- **Mecanismo:** `Num` formatea con `"0.##"` (`U/Systems/Dynamic/RackDynamicSystemWindow.xaml.cs:3447-3450`). El fondo de la tarima se
  escribe en la caja (`:3273`) y se relee (`:1409`). `MustRebuild` compara tarimas con tolerancia 1e-6
  (`A/Systems/Dynamic/DynamicEditorDesignAssembler.cs:44-61`).
- **Efecto:** un fondo persistido con mas de dos decimales puede reconstruir los modulos en un Actualizar sin cambios.
- **Estado:** sin validar en AutoCAD; I-55 no lo corrige. No afecta la paridad de `Resolve` con el sistema persistido (§4.5).

## 23. Revision adversarial de V4 (antes del commit)

**Metodo y transparencia.** Tres revisores de solo lectura de **esta misma sesion** atacaron el borrador de V4 con los puntos de la orden
G2E, contra el codigo de `dad4e77` y contra I-52 V14 (`e59bc89`): semantica transaccional; envoltorios y rutas por kind; flujo, authored y
fidelidad a la orden. Cada hallazgo se verifico en el codigo antes de admitirlo; los no verificables del todo se marcan PLAUSIBLE. Un cuarto
revisor comprobo las correcciones (§23.3). **No es la revision del Arquitecto independiente**, que sigue PENDING. La revision de V3 esta en
V3 §23 y no se repite aqui.

### 23.1 Ataques de la orden G2E

| Ataque | Borrador | Hallazgos | Estado en V4 |
|---|---|---|---|
| Redibujo parcial | Resiste ante excepciones; se rompia ante fallos que no lanzan | AV4-06, AV4-07, AV4-12 | Corregido: fallo tipado descarta; payload vacio rechazado en PREPARE; residual R-25 declarado |
| Rollback tras la primera redefinicion | Resiste en el codigo: todas las escrituras usan la transaccion del llamador (drawer, `RedefineBlock`, `RackBlockData.Write`, capas de rol) | AV4-10 | Sin evidencia en AutoCAD: OV-RED-03 y pruebas con puerto; restos anonimos PLAUSIBLE |
| Definiciones obsoletas | Resiste para anidadas compartidas | AV4-15, AV4-22 | Corregido: purga excluye lo que piden las vistas nuevas; borrado recoge *D |
| Borrado de huerfanas | Regla clara en §11.1 pero reescrita distinto en otras secciones; clasificacion incompleta | AV4-02, AV4-09 | Corregido: una sola regla («sin hermanas que redibujar»); definicion elegida incluida |
| Hermana dependiente de xref | Resiste (fuera de gates, PREPARE y MUTATE) | AV4-19 | Precisado: preflight vigente las sigue viendo; informe las nombra |
| Efectos de PREPARE | Resiste: solo importa biblioteca | AV4-17 | Precisado: la clonacion trae capas, estilos y anidadas |
| Transacciones anidadas | Resiste | AV4-06 | Guarda ampliada (`StartOpenCloseTransaction`, `LockDocument`, `.Commit(`) |
| Importar durante MUTATE | Resiste | AV4-06 | Guarda ampliada (`EnsureBlocks`) |
| Purga antes del commit | Resiste | — | Sin cambio |
| `Regen` por vista | Resiste | AV4-20 | `Regen` tras el borrado de huerfanas en el jig |
| Camino Cantilever | Resiste: plan puro, capas de rol en la transaccion, sin anidadas | — | Sin cambio |
| Envoltorio Dinamico | Resiste con paridad de payload pendiente | AV4-04 | Corregido: payload igual byte a byte |
| Envoltorio Push Back | Se rompia: lado B, preflights y payload | AV4-03, AV4-04, AV4-05 | Corregido |
| Envoltorio Cabecera | Resiste; fallback de la elegida pendiente | AV4-02 | Corregido |
| Fallo tras el commit en POST | Se rompia: avisos no observables; captura exterior del precedente | AV4-08, AV4-23 | Corregido: purga y renombre solo en log; aviso de `Regen`; captura propia (H-16) |
| Cancelar el primer jig tras el redibujo | Resiste | AV4-09 | Mensajes corregidos si no hubo redibujo |
| Cancelar un jig posterior | Resiste | — | Sin cambio |
| Consistencia authored | Resiste con precisiones | AV4-02, AV4-13, AV4-18 | Corregido (cura de `Id` en blanco y de espacios; payload desde el sobre propio) |
| Gate de CustomProperties | Resiste: el gate cubre al menos lo que MUTATE escribe | — | Sin cambio |
| ProjectVariables y expresiones | Resiste | AV4-16 | Precisado: la intencion de vinculo nunca coincide con Insertar |
| Solapamiento X-4 con I-52 | Resiste: crear (I-52) y redefinir (existente) separados | AV4-11, AV4-21 | Corregido: X-4 «I-55 la consume, I-52 la cita»; diferencias V3 → V4; G-M24 en la adopcion; F.2 con `ViewBlockDraw.cs` (X-5) |

### 23.2 Hallazgos

**BLOCKER: ninguno. HIGH: tres, corregidos antes del commit.**

| # | Sev. | Hallazgo (evidencia) | Resolucion en V4 | Seccion |
|---|---|---|---|---|
| AV4-01 | HIGH | La validacion del Owner de G9b pedia lotes de dos vistas, que solo existen en G12 | OV-RED-01..04 de vista unica en G9b; OV-RED-05 de lote en G12 | §20; mapa G9b, G12, OV |
| AV4-02 | HIGH | La clasificacion ignoraba la definicion elegida que el flujo vigente anade si el barrido no la encuentra (`P/RackSelectivoCommands.cs:132-136`; `P/RackDinamicoCommands.cs:179-182`; `P/RackCabeceraCommands.cs:243-246`): con un `Id` en blanco, la vista nueva recibiria otro `RackId` y el rack se partiria | Entrada de CLASIFICAR con la elegida (REDRAW con el `Id` curado) y las de `Id` de espacios identico | §11.1 paso 3; §9.2; mapa G9a (6) |
| AV4-03 | HIGH | Las pruebas por kind (11)-(15) probaban unidades del Plugin, y los proyectos de prueba no lo referencian (`tests/RackCad.Tests/RackCad.Tests.csproj:21-26`) | Descriptores tipados por vista y kind en Application; el Plugin solo construye unidades; guardas para el Plugin | §11.2; mapa G9a |
| AV4-04 | MEDIUM | Nada exigia que el payload del redibujo fuera el vigente (reescrituras por kind, `ResolvedByBlock`) | Payload byte a byte igual al del bucle vigente, desde el sobre propio y el interior de `ResolvedByBlock` | §11.1 paso 4; mapa G9a (11)-(15) |
| AV4-05 | MEDIUM | Los preflights vigentes (sobre, descriptor, interior I-11, catalogo de secciones, reconciliador) no tenian lugar en la secuencia | Paso 0, sobre el barrido completo, antes de la variante | §11.1; §14; mapa G9b |
| AV4-06 | MEDIUM | La guarda no prohibia `catch`, `StartOpenCloseTransaction`, `LockDocument`, `.Commit(`, `RedrawInPlace`, `CreateBlock`, `PurgeAfterCommit`; los envoltorios `RedrawInPlace` devuelven fallo en vez de lanzar | Guarda ampliada; contrato del puerto: excepcion o fallo tipado descartan | §11.2; mapa G9a |
| AV4-07 | MEDIUM | `RackBlockData.Write` no escribe con payload vacio (`P/Systems/Shared/RackBlockData.cs:21-24`): la geometria se confirmaria con el authored viejo | PREPARE rechaza payload vacio y `BlockId` nulo | §11.1 paso 4; §14 |
| AV4-08 | MEDIUM | Los avisos de POST no eran observables (purga y renombre absorben errores) y la captura exterior del precedente informaria «nada escrito» tras un commit | Purga y renombre solo en log (OM-28); aviso si falla el `Regen`; POST en captura propia; H-16 | §11.1 paso 6; §14; §22.5 |
| AV4-09 | MEDIUM | Sin estado para un rack existente sin nada que redibujar; mensajes falsos | `REDRAW_NOT_REQUIRED` sin transaccion ni POST; mensajes sin «vistas existentes actualizadas» | §11.1; §11.4; §14 |
| AV4-10 | MEDIUM, PLAUSIBLE | Una referencia en capa bloqueada revertiria todo el redibujo en cada Insertar | R-26 con remedio en el mensaje; OV-RED-03 (sin caracterizacion automatica posible sin AutoCAD) | §11.1; §14; mapa R, OV |
| AV4-11 | MEDIUM | La tabla X-4 decia que ambas iniciativas consumen el redefinir; faltaba registrar los cambios X de V4 | «I-55 la consume; I-52 la cita como precedente»; tabla «Diferencias de V4 con V3»; G-M24 en la adopcion | §17.2; §17.2.1 |
| AV4-12 | MEDIUM | Borrado de huerfanas en el primer jig: la transaccion del jig tambien confirma al cancelar y la definicion nueva ya esta confirmada | Gancho solo con el jig en OK; definicion confirmada antes declarada; residual R-25 | §11.3; §13; §14; mapa R |
| AV4-13 | MEDIUM | Push Back y Cantilever no curan un `Id` en blanco: abortan en su preflight | §6 y RID-4 corregidos | §6 |
| AV4-14 | MEDIUM | `REDRAW` contradecia el lado B de un Push Back de un sentido y la lateral legacy del Dinamico | REDRAW = todo lo demas que el bucle vigente redibuja | §11.1 paso 3 |
| AV4-15 | LOW-MEDIUM | La purga de POST podia retirar una definicion que la vista nueva necesita | Exclusion de los requisitos de las vistas nuevas preparadas | §11.1 paso 6 |
| AV4-16 | LOW | Orden de la intencion de vinculo y del reconciliador sin precisar | La intencion de vinculo nunca coincide con Insertar; reconciliador en el preflight | §11.1 paso 0 |
| AV4-17 | LOW | «Solo definiciones de biblioteca» impreciso | La clonacion trae capas, tipos de linea, estilos y anidadas | §11.1 paso 4 |
| AV4-18 | LOW | `Id` de espacios: Selectivo y Cabecera redibujan hoy las hermanas con ese mismo `Id` | Entran en la clasificacion y se curan | §9.2; §11.1 |
| AV4-19 | LOW | Las dependientes de xref: interpretacion de «fuera de MUTATE» | La orden prohibe modificarlas; preflight vigente las sigue viendo; asimetria con Actualizar en OM-25 | §11.1 |
| AV4-20 | LOW | Sin `Regen` ni mensaje tras borrar huerfanas en el jig | `Regen` tras esa colocacion | §11.3; §11.4 |
| AV4-21 | LOW | Inventario de archivos del gancho incompleto | F.2 con `BlockPlacement.cs`, `ViewBlockDraw.cs`, `LateralHeaderDrawService.cs`, `DrawAndPlace` por kind y `PlaceDefinition`; X-5 por `ViewBlockDraw.cs` | mapa G9b, F.2 |
| AV4-22 | LOW | Tercer borrador distinto sin anidadas *D | Borrado nuevo en transaccion del llamador con *D; `EraseViewBlocks` sin cambio (precisado en VF4-05; OM-29) | §11.2; mapa G9a |
| AV4-23 | LOW | Lock solo sobre MUTATE y POST; cuenta de transacciones | Un lock sobre PREPARE, MUTATE y POST; cuenta corregida | §11.1; §16 |
| AV4-24 | LOW | Guardas que fijan la forma de Insertar sin listar; rangos cortos; guarda de unidades duplicada | Lista y rangos corregidos; `RackUnitsGuardSourceTests` re-anclada a la importacion de PREPARE | mapa G9b |
| AV4-25 | LOW | Restos: ADR con V13, alternativa global mal justificada, CQ1-11, «V3 §23», «V2 → V3» | Corregidos | ADR-0042; §0.4; §17.2; §18; §21 |
| AV4-26 | LOW | Textos exactos con acentos frente a la convencion del Plugin; propiedad duplicada de mensajes | Convencion sin acentos declarada; G11 consume los mensajes de G9a | §11.4 |
| AV4-27 | LOW | Rangos de cita (`ViewBlockDraw.cs:97-109`) | Corregidos | §11.1 |

**Resultado sobre decisiones nuevas.** Los revisores no encontraron ninguna decision material nueva mas alla de CQ-01: Actualizar sin
cambio, la division de G9 y el borrado diferido de huerfanas los fuerza la orden; los renombres en POST, la importacion en PREPARE y la
exclusion de las dependientes de xref son precisiones derivadas del codigo o de la orden («No intentar modificar xref»).

### 23.3 Pase de verificacion de las correcciones

Un cuarto revisor de solo lectura (misma sesion) comprobo las correcciones contra el texto y el codigo. AV4-02 quedo resuelto; AV4-01 y
AV4-03 resueltos en lo sustancial, con restos. Sus hallazgos, corregidos antes del commit:

| # | Sev. | Hallazgo | Resolucion |
|---|---|---|---|
| VF4-01 | MEDIUM | La guarda de cableado y el plan de lote de G11 solo admitian colocar tras `REDRAW_APPLIED`, lo que prohibia colocar tras `REDRAW_NOT_REQUIRED` | «Tras `REDRAW_APPLIED` o `REDRAW_NOT_REQUIRED`» en la guarda, en G11 (estado y prueba) y en §21 |
| VF4-02 | LOW | OV-RED de G9b solo en Selectivo; OV-ID18-10 y -11 validadas en G12 aunque cambian en G9b | OV-RED-01/02 en los cinco kinds; OV-ID18-10/11 marcadas G9b |
| VF4-03 | LOW | El payload citaba helpers privados del Plugin, inalcanzables desde Core | Formula de composicion caracterizada en G3 y expuesta por `RackViewEnvelopeComposition` (G7b); sin llamadas nuevas a `Compose` en G9a |
| VF4-04 | LOW | Faltaba verificar la traduccion de descriptor a llamada del envoltorio | La guarda comprueba extremo, lado, poste y estacion |
| VF4-05 | LOW | El borrado extraido cambiaria la purga de Actualizar al recoger *D | `EraseViewBlocks` no se toca; borrado nuevo solo en I-55 (OM-29) |
| VF4-06 | LOW | `Id` de espacios extendido a todos los kinds; posible duplicado de la elegida | Solo Selectivo y Cabecera; sin duplicados por `BlockId` |
| VF4-07 | LOW | El borrado de huerfanas en el primer jig no purgaba sus anidadas | Purga tras el commit del jig |
| VF4-08 | LOW | El informe de faltantes en POST dependia de G8 aunque G9a no lo tenia como precondicion | Cableado del informe en G9b |
| VF4-09 | LOW | «Una MUTATE» sin el caso de cero; nombre destino del renombre sin ubicacion | «A lo sumo una»; nombre destino calculado en PREPARE con las funciones vigentes y probado |
| VF4-10 | LOW | F.2 sin los `DrawAndPlace` por kind; R-26 con caracterizacion en G3 imposible; citas `:62` y `:55-67` | Corregidos |

**Estado de publicacion:** sin BLOCKER ni HIGH abiertos. Abiertos sin BLOCKER ni HIGH: OM-22..OM-29, R-25, R-26 (PLAUSIBLE) y H-15, H-16.

