# Shared View Foundation — Especificacion (borrador)

```text
SHARED VIEW FOUNDATION — SPECIFICATION DRAFT — NOT CLAIMED — NOT CONSENSUS
FOUNDATION_ID        = PENDING   (lo asigna el Owner; WORKFLOW §2 caso d o fila planificada; ningun agente lo elige)
Rama / worktree      = NINGUNA   (no se reclama ni se crea sin autorizacion del Owner)
Mecanismo            = PENDING Owner · recomendado B, iniciativa neutral (Proposal V5 de I-55 §2)
Estado               = DRAFT publicado desde la rama de I-55 en G2F; I-52 no lo ha registrado
Coordinator / Architect / Owner de la foundation = NOT OPEN (no existe la iniciativa)
Implementation       = BLOCKED

Base de codigo       = dad4e77f4f267b9fa248ecb0c8bfd8a74bbab093 (main)
I-52 leida           = b7a6d9fe897dae4d29ae29ada7f60127bca365e5 (Proposal V17, sin consenso)
I-55 leida           = Proposal V5 (este mismo commit; decisions/I-55.md §G2F registra el SHA)
Documentos hermanos  = reconciliation.md (artefacto de reconciliacion, R0 PROPOSED)
                       delivery-map.md (mapa de entrega F0..F8)
                       adr-draft.md (VIEW FOUNDATION ADR, PROPUESTO, NUMERO PENDIENTE)
Prefijos             = P/ src/RackCad.Plugin/ · A/ src/RackCad.Application/ · D/ src/RackCad.Domain/ · U/ src/RackCad.UI/
                       T/ tests/RackCad.Tests/ · TU/ tests/RackCad.UI.Tests/
```

> **Que es y que no es.** Este documento describe **hechos y contratos neutrales** que hoy necesitan dos iniciativas (I-52
> `RACKMIRROR` e I-55 View Placement & Projection) y que ninguna de las dos puede extraer dentro de su propia rama sin que la otra
> tenga que consumir codigo no integrado (WORKFLOW: una rama nace de `origin/main` y se integra una sola vez). **No** autoriza
> implementacion, **no** reclama una iniciativa y **no** fija politicas de producto: cada consumidor aporta la suya. Cuando el Owner
> autorice la iniciativa, su Proposal reexpresa este texto (citando esta ruta y el SHA en el que se publico) y lo somete a su propio
> consenso; nunca se copia codigo entre ramas.

## 0. Reglas

| # | Regla |
|---|---|
| SVF-R1 | **Hechos, no politica.** Ningun contrato de la foundation decide aceptar, avisar, omitir, fallar ni elegir una variante canonica para un consumidor |
| SVF-R2 | **Una autoridad por responsabilidad, extraida exactamente una vez**, por la iniciativa que figura como titular en el registro de reclamos (§4) |
| SVF-R3 | **Consumir = la autoridad esta integrada en `main`** (su `Integration SHA` es alcanzable desde `origin/main`). «Existe en otra rama» no es consumir |
| SVF-R4 | **Sin cambio observable al integrar la foundation:** dibujo, BOM, `RACKLISTA`, nombres de bloque, mensajes y persistencia identicos; las caracterizaciones lo fijan antes de extraer |
| SVF-R5 | **Capas:** contratos y hechos puros en Application; adaptadores que tocan AutoCAD en el Plugin, verificados con guardas de fuentes (las suites no cargan el Plugin, `tests/RackCad.Tests/RackCad.Tests.csproj:21-26`) |
| SVF-R6 | **Independencia de decisiones de producto:** la foundation se puede aceptar e integrar sin M-01, OD-1..OD-8, ADR-0042 ni decisiones del Owner propias de `RACKMIRROR` (§9) |
| SVF-R7 | **ADR-0034 no se reabre:** la resolucion efectiva del Selectivo del BOM sigue una vez por rack dentro de su handler (§3.9.3) |

## 1. Alcance

### 1.1 Dentro (autoridades de la foundation)

| AUTH | Autoridad | X (I-52/I-55) | Seccion | Caracterizacion |
|---|---|---|---|---|
| AUTH-01 | Taxonomia `RackViewKind` (renombre de `DimensionViewKind`) | X-2 | §3.1 | CT-04 (tipos) |
| AUTH-02 | `RackViewVariant` y `RackViewAddress` | X-2 | §3.2 | CT-04 |
| AUTH-03 | Codec sintactico total y hechos de disposicion | X-2 | §3.3 | **CT-04** |
| AUTH-04 | Hechos de disponibilidad sobre el sistema resuelto | X-2 | §3.4 | CT-04 (parte dependiente del sistema) |
| AUTH-05 | `RackViewFrame`: ejes fisicos, origen, `[K_min, K_max]`, centro `c`, desplazamientos locales por variante | X-8 | §3.5 | **CT-05** |
| AUTH-06 | Hechos del barrido de definiciones de vista (clasificador compartido) | X-2 | §3.6 | CT-04 + CT-SCAN |
| AUTH-07 | Nucleo neutral de seleccion | X-1 | §3.7 | **CT-16** |
| AUTH-08 | Valor de colocacion sobre `Transform2D`, descomposicion de la transformacion fuente, normalizacion de angulos | X-7 | §3.8 | CT-GEO |
| AUTH-09 | Contrato `Resolve` con adaptadores por kind | X-2 | §3.9 | CT-RES |
| AUTH-10 | Autoridad unica de `Plan` (plan + marco + requisitos de bloque + nombre base sugerido) | X-2 | §3.10 | CT-PLAN |
| AUTH-11 | Autoridad unica de nombre base de definiciones de vista | X-2 | §3.11 | CT-NAME |
| AUTH-12 | `LibraryBlockRequirement`, validez de clave de biblioteca y consulta tras importar | X-4 (parte pura y consulta) | §3.12 | CT-BLK |
| AUTH-13 | Contrato del comparador authored por kind | X-3 | §3.13 | CT-AUTH |
| AUTH-14 | Caracterizaciones CT-04, CT-05, CT-16 y las auxiliares de esta tabla | — | §3.14 | — |

### 1.2 Fuera (politica de consumidor o producto)

| Excluido | Dueño | Motivo |
|---|---|---|
| `RACKPROYECTAR`, politicas `Rigid`/`Orthographic`, regla de sentido del eje comun, etapa FRAMES de ID19, politica de superposicion | I-55 (M-01, OD-6, OD-7) | Semantica de producto de ID19 |
| OD-1..OD-8, M-01 | Owner via I-55 | Decisiones de producto |
| Cola y UX de ID18, flujo de Insertar, redibujo atomico de hermanas (CQ-01), gate de propiedades personalizadas, funcion de pertenencia de hermanas, remedios y mensajes | I-55 | Producto; ADR-0042 |
| Politica del espejo: `μ_k`, σ′, exposicion, politica de `A_k`, algebra de reflexion (`ReflectionAboutLine`), ST-*, PRE-*, E*, huellas, `MaterializationContextReadSet`, CT-06 (δ) | I-52 | Producto; ADR-0036 |
| Primitivo de **crear** una definicion en la transaccion del llamador (`CreateInTransaction`) | I-52 (X-4, V17 §8.2) | No lo necesita ninguna autoridad de la foundation; la operacion vigente de **redefinir** (I-47 G9) tampoco se extrae |
| Asignador de identidad con politica de nombre inyectada (T-M05) y `RackDuplicationDestinationAssigner` | I-52 / `RACKDUPLICAR` | Politica de copia |
| Valor numerico de la tolerancia de escala | Registro de reconciliacion (§3.8; hoy I-52 V17 §4.3 lo fija en su G4, no provisional) | La foundation lo consume, no lo elige |
| Correcciones de comportamiento (PR-1 poste real de Push Back, PR-2 visibilidad de la planta Cantilever, H-01..H-16) | I-55 u otras | SVF-R4: la foundation no cambia comportamiento |
| Cambios a ADR-0034, ADR-0035, ADR-0009, ADR-0010, ADR-0039 | — | No hacen falta (§3.9.3, §9) |

## 2. Principio de separacion: tres niveles

```text
Nivel 1  Codec sintactico        (View, Section, token de kind) → direccion + disposicion         sin sistema resuelto
Nivel 2  Hechos de disponibilidad (sistema resuelto, catalogo, direccion) → Available | ...       sin consumidor
Nivel 3  Politica del consumidor  RACKEDITAR legado · ID17/ID18 · ID19 · I-52 · BOM · PVME         fuera de la foundation
```

Lo mismo vale para `Resolve` (hechos: sistema efectivo + diagnosticos) y para el marco (hechos: tramo y centro; el consumidor elige
minimo, maximo o centro).

## 3. Contratos

Nombres de tipos no contractuales; responsabilidades, entradas, salidas y totalidad si.

### 3.1 Taxonomia (AUTH-01)

- `RackViewKind = { Frontal, Lateral, Planta }`: renombre de `DimensionViewKind` (`A/Systems/Shared/DimensionViewPolicy.cs:11-16`) sin
  cambio de miembros ni de la politica de ADR-0035. No se serializa (solo persisten los bits de `DimensionViewVisibility`).
- **Codificaciones, no taxonomias** (AR4-35): los tokens `View` del sobre (`A/Persistence/RackEmbedDocument.cs:28-30`),
  `HeaderBlockInstance.View` y `SectionViewKind` (`A/StructuralSections/Geometry/SectionViewpoint.cs:14`, camara de secciones
  estructurales) son codificaciones o camaras con su propio uso; la foundation las mapea, no las fusiona.
- `CantileverViewKind` (`A/Systems/Cantilever/CantileverViewPlanBuilder.cs:12-37`) es el enum de camara del builder e incluye
  `AdapterSection` (`:36`), que no es una vista del rack: se conserva con un mapeo total desde `RackViewKind`.
- El renombre toca editores WPF (`U/Systems/{Dynamic,PushBack,Selective}/*Window.xaml.cs`); `U/Systems/Selective/RackSelectiveWindow.xaml.cs`
  es archivo caliente con I-49 G10: se serializa.

### 3.2 Variante y direccion (AUTH-02)

```text
RackViewVariant = Whole
                | Fondo(k ≥ 0)                       Selectivo frontal
                | Post(p ≥ 0)                        Selectivo, Dinamico, Push Back laterales (p = PostIndex del corte, no posicion de lista)
                | FlowEnd(Exit | Entrance)           Dinamico frontal
                | PushBackCut(End: EntradaSalida | Posterior, Side: A | B)
                | Station(s ≥ 0)                     Cantilever lateral
RackViewAddress = (RackViewKind, RackViewVariant)    igualdad por valor; nunca un indice de interfaz; base 0 (los mensajes usan base 1)
```

### 3.3 Codec sintactico total (AUTH-03, CT-04)

```text
Decode(kindToken, View, Section) → CodecFact { Address?, Disposition, ReadingEvidence }
Encode(Address)                  → (View, Section) canonico
Disposition ∈ { Canonical < Canonicalizable < Coerced < Invalid }   (orden de gravedad)
```

- **Hechos, no politica.** `Address` es la direccion que la **lectura vigente** del editor asigna a esa entrada (tambien para `Coerced`);
  `Disposition` dice cuanto hay que suponer para llegar a ella. Ninguna fila dice «ID19 acepta» ni «RACKEDITAR borra».
- **Totalidad (AR4-30).** Dominio = (forma del token de kind ∈ {exacto registrado, otra capitalizacion, con espacios, desconocido}) ×
  (`View` ∈ {nulo, vacio, token exacto, token en otra capitalizacion, token con espacios al principio o al final, solo espacios, no vacio
  desconocido}) × (`Section` en **particiones disjuntas por kind**: `selective` {< −1, −1, ≥ 0}; `dynamic` frontal {0, 1, otro} y resto
  {< −1, −1, ≥ 0}; `pushback` frontal {0..3, fuera} y resto {< 0, −1 aparte, ≥ 0}; `cantilever` {−1, ≥ 0, otro}; `cabecera` y `cama`
  {−1, otro}). Cada celda cae en **exactamente una** fila; una prueba de totalidad recorre el producto. La forma del token de kind es un
  hecho: `RACKEDITAR` despacha el kind distinguiendo mayusculas y `RACKDUPLICAR` sin distinguirlas (`P/KindHandlers/KindHandlerRegistry.cs:287`,
  `:291`); la politica de cada consumidor decide.
- **Regla de combinacion.** `Disposition = max(forma del token, disposicion de la fila)`, con forma del token: exacto → `Canonical`; otra
  capitalizacion sin espacios → `Canonicalizable`; con espacios o solo espacios → se lee como token desconocido de su fila (los editores
  comparan sin recortar, `P/RackSelectivoCommands.cs:329-330`, `P/RackCommandSupport.cs:102-103`; solo `RACKLISTA` recorta,
  `A/Persistence/RackListBuilder.cs:117-118`). Kind desconocido → `Invalid` sin direccion.
- **Ida y vuelta.** `Decode(Encode(a)) = (a, Canonical)` para toda direccion.
- **Nivel 1 puro.** Cada fila es un hecho **estatico** de la codificacion: se fija una vez en CT-04 (a veces con evidencia de lo que esa
  codificacion dibujo) y despues el codec no consulta ningun sistema ni geometria en ejecucion.

| Kind | Entrada (`View`, `Section`) | Direccion (lectura vigente) | Disposicion | Evidencia |
|---|---|---|---|---|
| `selective` | `frontal`, `Section ≥ 0` | `Fondo(Section)` | `Canonical` | `P/RackSelectivoCommands.cs:168` |
| `selective` | `frontal` con `−1`; `View` vacio con `−1` | `Fondo(0)` | **`Coerced`** salvo que CT-04 demuestre, una vez, que la geometria que esa codificacion dibujo es la de `Fondo(0)`; entonces la fila queda `Canonicalizable` (AR4-16: el commit `db095c4` sugiere que se dibujo con el sistema completo, no con `SelectiveDepthLayout.FondoSystemView(0)`) | `P/RackSelectivoCommands.cs:168`; `A/Systems/Selective/SelectiveDepthLayout.cs:57-66` |
| `selective` | `frontal` con `Section < −1`; `View` vacio con `Section ≥ 0`; `View` no vacio desconocido | `Fondo(max(Section, 0))` | `Coerced` | `:126`, `:168` |
| `selective` | `planta` con `−1` / con otro valor | `Whole` | `Canonical` / `Canonicalizable` | `:216` |
| `selective` | `lateral`, `Section ≥ 0` / `Section < 0` | `Post(Section)` / sin direccion | `Canonical` / `Invalid` | `:194` |
| `dynamic` | `frontal` con `0` o `1` / otro valor | `FlowEnd(...)` | `Canonical` / `Coerced` | `P/RackDinamicoCommands.cs:220-224`, `:392-393` |
| `dynamic` | `planta` con `−1` / otro valor | `Whole` | `Canonical` / `Canonicalizable` | `:209` |
| `dynamic` | `lateral`, `Section ≥ 0` / `Section < 0` | `Post(Section)` / `Post(0)` | `Canonical` / `Coerced` | `:236-239` |
| `dynamic` | `View` vacio con `−1` / con `Section ≥ 0`; `View` no vacio desconocido | `Post(0)` / `Post(Section)` | `Coerced` (la lateral sin seccion se dibujo con todos los niveles: `A/Systems/Dynamic/DynamicSystemLateralBuilder.cs:34-36`, `:67-80`) | `:236-239` |
| `pushback` | `frontal` con `0..3` / fuera | `PushBackCut(DecodeSection)` / sin direccion | `Canonical` / `Invalid` | `A/Systems/PushBack/PushBackSystemFrontalBuilder.cs:137-155`; `P/RackPushBackCommands.cs:427-450` |
| `pushback` | `planta` con `−1` / otro; `lateral` con `≥ 0` / `< 0`; `View` vacio o desconocido | `Whole`, `Post(Section)` / sin direccion | `Canonical` / `Invalid` | `P/RackPushBackCommands.cs:427-450` |
| `cantilever` | `frontal` o `planta` con `−1`; `lateral` con `≥ 0` / cualquier otra forma | `Whole`, `Station(Section)` / sin direccion | `Canonical` / `Invalid` | `P/RackCantileverCommands.cs:500-540` |
| `cabecera` | `lateral` o `planta` con `−1` / con otro valor; `View` vacio | `Whole` | `Canonical` / `Canonicalizable` | `P/RackCabeceraCommands.cs:197`, `:239-243` |
| `cabecera` | `View` no vacio desconocido | `Whole` (lectura vigente: lateral) | `Coerced` | `:239-243` |
| `cama` | `View` nulo / otro | `Whole` | `Canonical` / `Coerced` (nunca escrito) | `P/RackCamaCommands.cs:197-198` |

El conjunto exacto lo fija CT-04 con **una sola** caracterizacion; si CT-04 contradice una fila, gana CT-04 y la fila se corrige en la
foundation antes de extraer (nunca se ajusta el codigo para que coincida con la tabla).

### 3.4 Hechos de disponibilidad (AUTH-04)

```text
Availability(sistemaResuelto, catalogo, direccion) → Available
                                                   | VariantNotPresent(code)          el tipo existe; esa variante no
                                                   | SystemDoesNotSupportKind(code)   el sistema no tiene ese tipo de vista
                                                   | Unavailable(code)                el tipo y la variante tendrian sentido, pero el sistema
                                                                                      resuelto no produce ninguna (p. ej. sin cortes)
code = codigo de motivo estable; los textos para el usuario son del consumidor
Precondicion: sistemaResuelto proviene de un Resolved (§3.9); un NotResolved no tiene disponibilidad
```

| Direccion | `Available` si | Si no | Evidencia |
|---|---|---|---|
| `Fondo(k)` (Selectivo) | `k < SelectiveDepthLayout.Count(sistema)` | `VariantNotPresent` | `P/RackSelectivoCommands.cs:167-173` |
| `Post(p)` (Selectivo, Dinamico, Push Back) | `p ∈ Cortes(sistema, catalogo).PostIndex` (la cuadricula maestra usa el catalogo: `A/Systems/Selective/SelectiveLateralBuilder.cs:33`, `:44`; `A/Systems/Dynamic/DynamicSystemLateralBuilder.cs:270-273`) | `VariantNotPresent(PostNotInCuts)`; conjunto de cortes vacio → `Unavailable(NoCuts)` (`A/Systems/Selective/SelectiveLateralBuilder.cs:36-38`) | `P/RackDinamicoCommands.cs:240-244`; `P/RackPushBackCommands.cs:285-289` |
| `PushBackCut(e, B)` | el diseño es compuesto (`IsComposite`) | `VariantNotPresent(SingleDirectionRack)` | `D/Systems/PushBack/PushBackDesign.cs:70`; `A/Systems/PushBack/PushBackSystemFrontalBuilder.cs:44-52` |
| `Station(s)` (Cantilever) | `s < line.Stations.Count` | `VariantNotPresent` | `P/RackCantileverCommands.cs:306-310` |
| `Whole`, `FlowEnd`, `PushBackCut(e, A)` | siempre que el kind tenga ese tipo (sobre un `Resolved`; una linea Cantilever bloqueada es `NotResolved`, §3.9.1) | — | — |
| tipo no dibujable por el sistema (p. ej. Cama frontal o planta; Larguero) | — | `SystemDoesNotSupportKind` | builders existentes |

**Lo que NO es disponibilidad** (AR4-08): «ID19 no soporta la Cama», «variante canonica de ID19», omitir, avisar o fallar. La matriz de
exposicion (que consumidor ofrece que par, `CanStartWith`, `CanAddLater`, `CanBatch`, canonica de ID19) es politica de I-55; la
exposicion de I-52 (`A_k` como filtro) es politica de I-52. Lo que un editor hace hoy con `VariantNotPresent` (borrar como fantasma,
redibujar el lado B como A) es politica legada de `RACKEDITAR`.

### 3.5 Marco y tramo de una vista (AUTH-05, CT-05)

**Marco fisico del rack:** R = corrida, D = profundidad, H = altura, con un unico origen fisico por rack.

```text
RackViewFrame(sistemaResuelto, tipo, variante) =
  AxisMap          eje local X/Y de la vista ↔ eje fisico R/D/H con signo
  PhysicalOrigin   punto de la vista (coordenadas locales) donde esta el origen fisico del rack, incluido el desplazamiento
                   local de la variante (p. ej. anchorOffset de un corte de esquina)
  Span[K]          [K_min, K_max] por eje fisico representado, en coordenadas fisicas del rack, para ESA variante
  EndConvention    eje de poste o cara, fijado una vez por CT-05
  Center[K]        c_K = (K_min + K_max) / 2 en coordenadas fisicas, y su expresion local en la vista (via AxisMap y PhysicalOrigin)
  VariantOffset    desplazamiento local de la variante respecto del origen de la familia (AR4-50)
```

- **Datos fisicos unicos; el consumidor elige.** ID19 ancla por `K_min` o `K_max` segun su politica; I-52 usa `Center` como su `c`. I-52
  no calcula `c` por otra autoridad: es el mismo hecho fisico. `Center` es el centro del descriptor, **no** una forma afin normativa, y
  **nunca** absorbe δ: la diferencia entre ese centro y la simetria geometrica real la mide y registra la politica de I-52 (CT-06, no
  ajusta) (I-52 V17 §11 y §19: sin forma afin de `c`, δ registrado y nunca ajustado, sin derivar centros sin verificar).
- **Tramos independientes de lo que el builder omite al dibujar**: postes de frontera inexistentes del Dinamico
  (`A/Systems/Dynamic/DynamicSystemFrontalBuilder.cs:74-77`), fronteras que un corte compuesto de Push Back no posee, rango de un corte
  lateral del Dinamico (`A/Systems/Dynamic/DynamicSystemLateralBuilder.cs:175-177`). Todo tramo mide mas que `GeometryTolerance.Length`
  (`A/Geometry/Vector2D.cs:90`).
- **Envolvente dibujada frente a tramo de convencion (AR4-47, PLAUSIBLE).** El tramo usa la convencion de extremos (p. ej. eje de poste);
  la envolvente dibujada incluye el ancho del poste. CT-05 registra ambas; que se usa para superposicion es politica del consumidor.
- **`Plan` devuelve el marco** (AR4-36): el resultado de §3.10 lleva el `RackViewFrame` de la direccion; una prueba permanente compara el
  marco con la salida medida del builder.
- **CT-05 es oraculo** (AR4-48): las pruebas de consumidores toman los valores de los fixtures de CT-05, nunca del descriptor productivo.

| Familia | Vista | Eje local +X | Eje local +Y | Origen fisico local | Desplazamiento local por variante | Evidencia |
|---|---|---|---|---|---|---|
| **Rack** (Selectivo, Dinamico, Push Back, Cabecera) | Planta | +D | +R | D: cara exterior delantera (Dinamico: salida; Push Back: extremo bajo o cara del lado A); R: eje del poste 0 | ninguno (`Whole`) | `A/Systems/Selective/SelectivePlantaBuilder.cs:16-19`; `A/Systems/Dynamic/DynamicSystemPlantaBuilder.cs:15-17`; `A/RackFrames/PlantaHeaderLayoutBuilder.cs:95-103` |
| Rack | Frontal | +R | +H | eje del poste 0 de la cuadricula de la vista; base del poste | `Fondo(k)`: la cuadricula del fondo es **prefijo** de la maestra, mismo origen R; `FlowEnd`/`PushBackCut`: segun CT-05 | `A/Systems/Selective/SelectiveFrontalBuilder.cs:50`; `A/Systems/Selective/SelectiveDepthLayout.cs:28-33`, `:52-55` |
| Rack | Lateral | +D | +H | cara delantera del primer fondo que alcanza el poste (Selectivo) o X = 0 del sistema; base del poste | `Post(p)`: `anchorOffset` del corte de esquina | `A/Systems/Selective/SelectiveLateralBuilder.cs:82-97`; `A/RackFrames/LateralHeaderLayoutBuilder.cs:56-59` |
| **Cantilever** | Planta | −R | +D | columna de la estacion 0; plano de conexion | ninguno | `A/Systems/Cantilever/CantileverViewPlanBuilder.cs:209-219`; `A/Geometry/Spatial3D.cs:186-197` |
| Cantilever | Frontal | −R | +H | columna de la estacion 0; suelo | ninguno | idem |
| Cantilever | Lateral | −D | +H | plano de conexion; suelo | `Station(s)`: segun CT-05 | idem |

- **Cantilever:** el origen D (plano de conexion) es **interior** al tramo D (`A/Systems/Cantilever/CantileverColumnBaseDatum.cs:19-24`,
  `:45`); ningun consumidor puede suponer que el origen es un extremo.
- Un par cuya caracterizacion no cuadre queda sin marco (`FrameUnavailable`) hasta una version nueva de la foundation; **nunca** se ajusta
  el descriptor.

### 3.6 Hechos del barrido de definiciones de vista (AUTH-06)

Un solo clasificador sintactico y de disponibilidad para todos los caminos que hoy decodifican vistas por su cuenta (AR4-14):
`RACKEDITAR` → Actualizar e Insertar (cinco kinds), `ProjectVariableMutationExecutor` (`P/ProjectVariableMutationExecutor.cs:180-218`),
ID19 e I-52 (SNAPSHOT).

```text
entrada (la aporta el Plugin)   por definicion: BlockId/handle, nombre, sobre interpretado o nulo, texto crudo del payload,
                                IsDependent (dependiente de xref), IsFromExternalReference, IsAnonymous, DirectReferenceCount
                                (P/RackBlockFinder.cs:57-91 ya lee la cuenta desde el registro), Id de nivel superior sondeado en
                                payloads no interpretables
salida (pura)                   por definicion: kind, Id crudo, IdIsNullOrEmpty, IdIsNullOrWhiteSpace, CodecFact (§3.3), IsXrefDependent,
                                DirectReferenceCount, ProbeId; con sistema resuelto: Availability (§3.4)
```

- Los **dos** predicados de «en blanco» son hechos; cual usa cada camino es politica (Actualizar conserva `IsNullOrEmpty`,
  `P/RackSelectivoCommands.cs:119`, `P/RackCabeceraCommands.cs:233`).
- **Migracion sin cambiar transacciones.** Los bucles de Actualizar e Insertar y `ProjectVariableMutationExecutor` consumen estos hechos
  para decodificar; sus transacciones, borrados y redibujos no cambian. Un camino que no pueda migrar sin cambio observable conserva su
  decodificacion como **productor legacy** con una **prueba diferencial permanente** contra el clasificador sobre el dominio de CT-04.
- La **pertenencia** (que definiciones son hermanas mutables, cuales de solo lectura, que se cura) es politica de cada consumidor.

### 3.7 Nucleo neutral de seleccion (AUTH-07, X-1, CT-16)

- **Fuente:** `RackDuplicationPlan.Build(selection, definitions, isKnownKind)` (`A/Persistence/RackDuplicationPlan.cs:225-353`).
- **Hechos neutrales:** deduplicacion de referencias; filtro de no-bloques y fuera de Model Space; clasificacion una vez por definicion
  (datos ausentes / inutilizables / validos); clave de grupo `RackId` (`OrdinalIgnoreCase`) o `Definition(handle)` sin `Id`; agrupacion
  conservando **todas** las referencias; hechos de consistencia (kinds mezclados, nombres divergentes) y orden estable (grupo, referencia).
- **Inyectado por el llamador:** predicado de kind conocido, mensajes, comprobacion de misma autoridad (via AUTH-13).
- **Fuera:** `RackDuplicationDestinationAssigner` (`:597-658`), textos «No se duplico nada», asignador de identidad de I-52 (T-M05).
- `RACKDUPLICAR` conserva su fachada byte a byte (`T/RackDuplicationPlanTests.cs`, 25 hechos y teorias).
- **Coordinacion con I-49** antes de editar `RackDuplicationPlan.cs`: su prueba de supervivencia P24.8 se escribe contra el planificador
  en `main` (I-52 V17 §5.1).

### 3.8 Valor de colocacion y transformacion fuente (AUTH-08, X-7)

```text
PlacementValue  = (Position2D, RotationRadians, UniformScale)
Compose(v)      → Transform2D                        (A/Geometry/Transform2D.cs:20-116)
Decompose(t)    → PlacementValue | NotDecomposable    validando rigidez/escala con la tolerancia registrada

SourceTransformFacts(ScaleFactors, Normal, Rotation, tolScale) → {
    ScaleClass ∈ { Unit(+1,+1,+1),
                   UnitHalfTurn(−1,−1,+1)        equivale a Rotation + π
                   UnitReflectionXY(det < 0 en XY; (−1,+1,±1), (+1,−1,±1))
                   UnitNegativeZ(+1,+1,−1)
                   NonUnitUniform(s)
                   NonUniform },
    NormalIsWorldZ, CanonicalRotation (normalizada a (−π, π]) }
NormalizePi(a)  → (−π, π] con −π ↦ +π explicito (AR4-46: atan2(−0.0, −1) = −π)
```

- **Terminologia unica** (AR4-49): «reflexion» = determinante 2D negativo; «escala no unitaria» = `|s| ≠ 1`; «Z negativa» = `sz < 0`.
  Que clases acepta cada consumidor es politica (ID19: `Unit` y `UnitHalfTurn`; I-52: reflexiones y `s ≠ 1` segun su ST-11).
- **Tolerancia de escala:** un unico valor con nombre, **registrado en el artefacto de reconciliacion**. I-52 V17 lo fija hoy en su G4 y
  no lo marca provisional (§4.3, §0.3, ST-7/ST-8): la foundation no lo elige; si al abrir F4 no esta registrado, F4 se detiene (STOP).
- **Fuera:** `ReflectionAboutLine` y el algebra de reflexion (I-52 §3.2, §3.3).

### 3.9 Contrato `Resolve` (AUTH-09, X-2)

#### 3.9.1 Forma

```text
Application (neutral)
  RackResolveRequest = { KindToken,
                         Authored : Persisted(RackEmbedDocument) | InMemory(documento de diseño del sustrato del kind),
                         Catalog,
                         ProjectVariables : ReadResult(Present(doc) | Absent | Unreadable),      leido UNA vez por el llamador
                         SectionCatalog   : ReadResult(Present(sections) | Unavailable(reason)) solo Cantilever, tipado (AR4-32) }

  RackResolveResult  = Resolved    { EffectiveSystem, Diagnostics[] }
                     | NotResolved { Reason, Diagnostics[] }

  Reason (lista cerrada, en orden de precedencia; AR4-38)
      1 Unreadable              payload no interpretable, MAJOR mas nuevo, sustrato del kind ausente
      2 UnknownKind
      3 DependencyUnavailable   catalogo de secciones no disponible (Cantilever)
      4 BrokenReference(propertyId, variableId, error)   solo Selectivo hoy
  Diagnostic (hechos)
      OutputBlocking(code, text)   RackBomOutputGate.For(sistema) (A/Bom/RackBomOutputGate.cs:49); linea Cantilever invalida
      LegacyShape(class, shapeId)  lista cerrada §3.9.2
      Warning(code, text)

  IRackSystemResolver (puerto)   Resolve(request) → RackResolveResult
```

- **No se pierde el sistema** por un diagnostico bloqueante (AR4-09): un Push Back con `RackBomOutputGate` bloqueado y una linea Cantilever
  invalida (`line.IsValid == false`, `P/KindHandlers/CantileverKindHandler.cs:65-67`, que hoy produce la linea y descarta su BOM) son
  `Resolved` con `OutputBlocking`. Si CT-RES demuestra que algun kind no produce sistema en un caso bloqueante, ese caso se registra como
  `NotResolved` con un motivo nuevo en la lista cerrada, antes de extraer.
- **Precedencia por kind.** La lista reproduce el orden vigente de los handlers (Selectivo: ilegible y despues referencia rota,
  `P/KindHandlers/SelectiveKindHandler.cs:41-66`; Cantilever: sustrato ausente y despues catalogo de secciones, `:49-60`); CT-RES la fija
  por kind y un kind con otro orden lo declara explicitamente.
- **Una lectura del registro de variables por comando**, pasada como resultado (el BOM conserva su mapeo de registro nulo a ausente,
  `A/Systems/Selective/SelectiveEffectiveDesignResolver.cs:38-45`).
- **Expresiones de I-49:** cuando I-49 integre su autoridad final, el contexto anade su lectura; hasta entonces no aplica.

#### 3.9.2 Lista cerrada de formas legacy (AR4-10)

`LegacyShape` es un **hecho** calculado con predicados deterministas sobre el documento persistido (claves presentes en el JSON crudo y
resultado del store), nunca un estado sin predicado. Clases:

| Clase | Significado |
|---|---|
| `Current` | forma que escriben los editores vigentes |
| `LegacyAccepted(parityFixture)` | forma antigua cuya resolucion reproduce lo dibujado y lo que abre el editor, **demostrado** por CT-RES con el fixture nombrado |
| `Canonicalizable(parityFixture)` | forma antigua que el editor reescribe a `Current` al guardar, con el mismo sistema resuelto, demostrado por CT-RES |
| `Unsupported(shapeId)` | forma antigua conocida cuya paridad no se demostro o difiere |
| `Unresolved` | ningun predicado de la lista reconoce la forma (valor por defecto de todo lo no censado) |

| Kind | Forma | Predicado | Clase inicial (la confirma CT-RES) |
|---|---|---|---|
| `dynamic` | diseño y sistema | clave `DynamicDesign` presente | `Current` |
| `dynamic` | solo sistema | `DynamicDesign` ausente y `DynamicSystem` presente; el registro reconstruye el diseño con valores por defecto (`A/Systems/Shared/SystemRegistry.Default.cs:74-87`; `A/Persistence/DynamicRackSystemDocument.cs:196-210`) | `Unsupported` hasta que CT-RES demuestre paridad (entonces `LegacyAccepted`) |
| `cabecera` | wrapper vigente | schema legible; RackProjectDocument con Kind=Selective y Header presente y utilizable, reconocido por RackProject.ForSelective y RackProjectStore.Serialize/Deserialize y SystemRegistry.Default (writer RackCabeceraCommands.cs:194; store:50-57; registry:42-55) | `Current` |
| `cabecera` | cabecera legacy | ruta de cabecera legacy que regenera el modelo fisico en el store (`A/Persistence/RackProjectStore.cs:334`) | `Unsupported` hasta paridad |
| `selective`, `pushback`, `cantilever`, `cama` | forma vigente | sustrato presente | `Current` (sin legado distinto conocido) |
| cualquiera | no reconocida | ningun predicado | `Unresolved` |

**Superficie independiente:** `RackPersistedShapeClassifier.Classify(kind, rawPayload, storeReadResult)` devuelve el hecho de forma
sin resolver un sistema, sin I/O y sin politica de aceptacion. Se llama una vez por payload de cada miembro tras el barrido; los gates
leen esos hechos aunque Resolve reciba solo el representante (incluso Selectivo en memoria). `LegacyShape` en el resultado Resolve
reutiliza ese mismo clasificador cuando el representante trae payload. InMemory sin payload original no inventa LegacyShape: el
consumidor conserva los hechos previamente clasificados por miembro. No introduce otra lista ni fuerza una resolucion por hermana.

La lista se cierra en F1 (CT-RES). Una forma nueva que aparezca despues es `Unresolved` hasta una version nueva de la foundation. **El BOM
no cambia:** cotiza como hoy con el sistema que produce el adaptador (el diagnostico no altera su politica). ID19 exige `Current`,
`LegacyAccepted` o `Canonicalizable`; I-52 aplica su propia politica (E6 propuesto).

#### 3.9.3 Adaptadores por kind y ADR-0034

```text
Contrato neutral (Application)      IRackSystemResolver
        ↓
Adaptador del puerto (Plugin)       KindHandlerSystemResolver: despacha por KindHandlerRegistry (P/KindHandlers/KindHandlerRegistry.cs:55-63)
        ↓                           a una interfaz hermana del Plugin (p. ej. IRackKindSystemResolver) implementada por los seis handlers
Adaptador por kind (en el handler)  ResolveSystem(...) — autoridad vigente del kind, sin moverla
        ↓
Selectivo                           SelectiveKindHandler.ResolveSystem: el UNICO `new SelectiveEffectiveDesignResolver()` del handler;
                                    BuildBom lo llama; la entrada en memoria usa el mismo metodo privado (una construccion en el fuente)
```

- **ADR-0034 §10 se cumple literalmente:** `RACKBOMTOTAL` sigue resolviendo una vez por rack **dentro del handler Selectivo**, con el mismo
  snapshot del registro leido una vez por el total, y la referencia rota sigue abortando el total. ID19 e I-52 llegan a esa **misma**
  autoridad a traves del puerto: no hay un segundo sitio de resolucion ni se mueve el existente.
- **ADR-0034 §6:** la clase de resolucion de Application (`SelectiveEffectiveDesignResolver`) sigue siendo el unico punto que convierte
  vinculos en numeros; los sitios vigentes fuera del BOM (apertura del editor, preflight y reconciliador de variables, exportacion a
  biblioteca) no cambian y no forman parte de este contrato.
- **Consumidores puros** (planes de ID19 e I-52 en Application) reciben resultados del puerto; sus pruebas usan un puerto falso.
- **Posibilidad tecnica:** verificada sobre `dad4e77` (el handler Selectivo resuelve en `BuildBom`, `:36-70`; los otros cinco resuelven con
  llamadas puras de Application salvo `StructuralSectionCatalogAccess.TryLoad`, que es del Plugin). **No hace falta modificar ADR-0034.**

| Guarda vigente | Que fija | Tratamiento |
|---|---|---|
| `T/SelectiveBomAuthorityTests.cs:403-408` | una construccion de `SelectiveEffectiveDesignResolver` en el handler Selectivo | **Sin cambio**: debe seguir verde tal cual |
| `:377-383` | `BomBuildResult BuildBom(` y `ProjectVariablesDocument projectVariables` en `IRackKindHandler.cs` | Sin cambio (el contrato nuevo va en una interfaz hermana) |
| `:386-399` | los otros cinco handlers aceptan el parametro pero no lo desreferencian ni nombran el resolver | Sin cambio: sus `ResolveSystem` no leen variables |
| `:410-417`, `:421-425`, `:427-436`, `:440-477` | `BomTotal` sin resolver propio, una lectura del registro, handlers sin `ProjectVariablesRegistry`, autoridad authored y resultados tipados en `BomTotal` | Sin cambio |
| `T/PushBackRoundTripSourceGuardTests.cs:90-127` | cadenas exactas del handler Push Back (`new RackProjectStore().Deserialize(embed.Design)`, `project?.PushBackDesign == null`, `new PushBackResolver(catalog).Resolve(project.PushBackDesign)`, `PushBackBomBuilder.Build(system, catalog)`, reestampado) | Se conservan las cadenas dentro de `ResolveSystem`/`BuildBom`; cualquier cadena que deba cambiar se **reapunta con motivo** en F5a, sin debilitar |
| `:135-171`, `:173-180`, `:183-186`, `:318-345` | registro de seis handlers en orden; superficie de despacho; `IRackKindHandler.cs` no nombra «PushBack»; `RackDuplicarCommands` sin tipos concretos | Sin cambio (la interfaz hermana tampoco nombra kinds) |
| `T/CantileverPluginSourceGuardTests.cs:276-283`, `:234-243`, `:248` | `StructuralSectionCatalogAccess.TryLoad` en comandos y handler; reestampado; registro | Sin cambio (el resultado tipado envuelve la llamada vigente dentro del handler) |
| `T/PushBackBomCommandGuardTests.cs:50-66` | `OutputBlockedReason` antes de `BuildRackBom` | Sin cambio |
| `T/PushBackCellConfigurationDeliveryTests.cs:275-288`; `T/SelectiveDuplicationFailClosedTests.cs:307`, `:313`; `T/ProjectVariableMutationExecutorTests.cs:319` | tokens prohibidos o requeridos en handlers y ejecutor | Sin cambio |
| Nueva: `KindHandlerSystemResolverGuardTests` | cada handler implementa `ResolveSystem`; Selectivo solo lo llama desde `BuildBom` y conserva `OutputBlockedReason => null`; los otros kinds preservan sus invocaciones actuales; ningun sitio fuera de los handlers construye resolvers de sistema para el puerto | F5a |

- **Invariante del BOM en dos pasadas** (AR4-31): `RACKBOMTOTAL` hace un preflight (`OutputBlockedReason`) y una construccion
  (`BuildRackBom`) por rack (`P/RackInventarioCommands.BomTotal.cs:147-215`); ambas pasadas usan el mismo snapshot de variables y el mismo
  catalogo; `BrokenReference` sigue con severidad de error y aborta. CT-RES verifica **exactamente una invocacion efectiva** del resolver
  Selectivo por rack desde BuildBom y ninguna desde OutputBlockedReason; contar tokens `new` no prueba el numero de invocaciones.
  CT-RES caracteriza cada otro kind y conserva sus llamadas vigentes antes de F5a.
- **Politica del BOM, por kind y sin cambio observable:** `DependencyUnavailable` del Cantilever se sigue informando con el mismo salto y
  el mismo texto de payload inutilizable de hoy.

### 3.10 Autoridad unica de `Plan` (AUTH-10, X-2; AR4-06)

```text
Plan(sistemaResuelto, direccion, contextoDePlan) → RackViewPlanResult {
    Family : HeaderRun | CantileverCurves
    Plan
    Frame  : RackViewFrame de la direccion (§3.5)
    BlockRequirements (§3.12)
    SuggestedBaseName (§3.11; el nombre unico lo fija el Plugin) }
contextoDePlan = catalogos · semilla de agrupacion (prefijo) · rackName (anotaciones) · politica de cotas del diseño
                 · PlantaVisibility del diseño (Cantilever; nulo = comportamiento vigente)
```

- Una implementacion por (kind, tipo) que **delega en los builders vigentes** (tabla de I-55 V4 §4.3), sin geometria nueva.
- **Una sola autoridad de plan.** Las lambdas de plan del Plugin (`ViewBlockDraw` y sus servicios, `P/Systems/Shared/ViewBlockDraw.cs:49-50`,
  `:84-86`, `:136-137`; `LateralHeaderDrawService` con su `Merge` privado de largueros, `P/Drawing/LateralHeaderDrawService.cs:198-208`;
  cortes laterales del Selectivo; `CantileverViewPlanBuilder.Build` desde los comandos) pasan a **delegar** en `Plan` en F5b, sin cambio
  observable. `Merge` (puro) se mueve a la autoridad.
- **Entrada leida del dibujo:** el prefijo de agrupacion de un redibujo lateral hoy se lee del dibujo (`ReadBlockName`,
  `P/Drawing/LateralHeaderDrawService.cs:83`, `:275-291`); el Plugin lo lee y lo pasa como semilla. `Plan` no hace I/O.
- **Productores legacy durante la transicion:** todo sitio del Plugin que F5b no migre queda en un **registro de productores legacy**
  (documento + guarda) con: (i) caracterizacion (CT-PLAN), (ii) prueba de paridad permanente `plan(legacy) == Plan(...)` sobre los fixtures,
  (iii) gate de retirada nombrado. **Objetivo de F5b: registro vacio; obligatorio en F7.** Un sitio nuevo que construya planes fuera de la autoridad hace
  fallar la guarda.
- `PlantaVisibility` en el contexto permite que PR-2 (correccion de producto de I-55) cambie solo el argumento en los llamadores del Plugin.

### 3.11 Autoridad unica de nombre base (AUTH-11; AR4-13)

**Opcion elegida: A + B.** Las funciones puras vigentes se extraen **tal cual** a Application (A) y los helpers del Plugin delegan en ellas o
se retiran con guarda (B). La opcion C (adaptador por kind llamado desde la foundation) anade una indireccion sin segundo cliente.

| Funcion vigente | Entradas | Uso |
|---|---|---|
| `SelectiveFrontalDrawService.BlockName` (`P/Systems/Selective/SelectiveFrontalDrawService.cs:76`) | frentes, altura, rackName | insertar |
| `SelectivePlantaDrawService.BlockName` (`:67`) | rackName, frentes | insertar |
| `DynamicFrontalDrawService.BlockName` (`:50`), `DynamicPlantaDrawService.BlockName` (`:48`), `DynamicSystemDrawService.BlockName` (`:61`) | extremo, frentes, fondo, longitud, rackName | insertar |
| `PushBackFrontalDrawService.BlockName` (`:59`), `PushBackPlantaDrawService.BlockName` (`:50`), `PushBackSystemDrawService.BlockName` (`:62`) | extremo, lado, compuesto, frentes, poste, rackName | insertar |
| `PlantaHeaderDrawService.BlockName` (`:45`); `FlowBedDrawService.BlockName` (`:48`) | rackName; tipo de cama, fondo | insertar |
| `LateralHeaderDrawService.BuildBlockName` (`P/Drawing/LateralHeaderDrawService.cs:293`) | `catalog.DescribeId(LeftPost)`, fondo, altura | insertar |
| `RackSelectivoCommands.FrontalName` (`:503`) y concatenaciones de lateral/planta de los comandos (`RackSelectivoCommands.cs:206`, `:220`, `:569`; `RackDinamicoCommands.cs:212`, `:230`, `:258`, `:340`; `RackPushBackCommands.cs:249`, `:263-268`, `:298`; `RackCabeceraCommands.cs:273`, `:284`) | nombre base, fondo, cuenta | insertar y renombrar |
| Cantilever: `RackCantileverCommands.ViewBlockName` (`:543`) + `CantileverViewMaterializer.SuggestName`/`Sanitize` (`P/Drawing/Cantilever/CantileverViewMaterializer.cs:287-312`) | rackName, vista, estacion | insertar (compuestas) y renombrar (`ViewBlockName` sola) |

- **Cantilever sin doble aplicacion.** Hoy la insercion pasa `ViewBlockName(...)` como `rackName` a `CreateBlockDefinition`, que aplica
  `SuggestName` (`P/RackCantileverCommands.cs:152`; `CantileverViewMaterializer.cs:45`), y el primer redibujo renombra a `ViewBlockName`
  (H-15). La autoridad expone **una** funcion de nombre de insercion que reproduce esa composicion exacta, y el Plugin la pasa a
  `CreateBlockDefinitionNamed` (`:99-108`), cuyo `Sanitize` es idempotente sobre esa salida (solo contiene `[A-Za-z0-9_-]`): el nombre
  resultante es identico y `SuggestName` no se vuelve a aplicar. H-15 no se corrige (SVF-R4).
- **`UniqueBlockName` sigue en el Plugin** con sus politicas vigentes (`LateralHeaderDrawer.cs:395`, `RackCloner.cs:71`,
  `CantileverViewMaterializer.cs:318`, `StructuralSectionMaterializer.cs:158`; decision de I-09 de no unificarlas).
- **Clave de biblioteca ≠ nombre de definicion generado** (AR4-12): los saneadores de nombres de vista (`BlockNaming.SanitizeBlockName`,
  `A/BlockNaming.cs:12`; `Sanitize` del Cantilever) solo se aplican a nombres generados por RackCad; nunca a las claves de catalogo de
  §3.12.
- **Allowlist de nombres:** F5b puede modificar exclusivamente `SuggestName`/`Sanitize` de `CantileverViewMaterializer.cs` para delegar
  o retirarlos; no cambia creacion, redefinicion, transacciones ni geometria. La guarda prohibe logica duplicada incluso sin callers.
- **CT-NAME** caracteriza cada funcion **cadena a cadena** (incluidos valores por defecto «Selectivo»/«Dinamico», el sufijo «frente F{n}»
  solo con mas de un fondo, la cabecera desde catalogo, el prefijo y saneado del Cantilever y la discrepancia H-15 entre insertar y
  renombrar).

### 3.12 Requisito estructural de bloques (AUTH-12, X-4 parte pura y consulta)

```text
Para CADA instancia del plan (instancias aplanadas, incluidas las de grupos de cabecera; AR4-34):
  RoleRequirement(rol) = Required         rol ≠ Annotation, Dimension, Pallet
                       | NotApplicable    Annotation (DBText), Dimension (RotatedDimension) (P/Drawing/LateralHeaderDrawer.cs:258-291)
                       | OptionalVisual   Pallet (referencia visual sin BOM; la clasificacion la confirma CT-BLK)
  planes CantileverCurves ⇒ ninguna instancia requiere bloque
  LibraryKeyValid(clave) ⇔ clave ≠ null ∧ ¬IsNullOrWhiteSpace(clave)          (1)(2): puro; NO se sanea; '.' es valido
  Union = claves de las instancias Required u OptionalVisual con clave valida (orden estable)
  Trazabilidad = (PieceId, rol, vista, clave) por instancia
Consulta tras importar (Plugin):
  LibraryBlockQuery(db, claves) → por clave Present | Missing                   (3): la existencia la decide la tabla de bloques
  LibraryAvailability → FileMissing | Ok | Unknown                             (el importador devuelve 0 en silencio ante una excepcion,
                                                                                P/Drawing/BlockLibraryImporter.cs:74-78, :110-118)
```

- **Claves reales con punto:** `assets/catalogs/blocks.csv:16-17` (`RODILLO_DE_TUBO_DE_1.9_CALIBRE_14_LATERAL`,
  `RODILLO_DE_TUBO_DE_2.5_CALIBRE_14_LATERAL`) son validas.
- **Discriminacion por rol**, nunca por nombre vacio: un rol `Required` con clave nula o en blanco es un **hecho** «requisito incumplido»
  (el consumidor decide si falla antes de escribir, ID19 e I-52, o coloca y reporta, ID17/ID18).
- **Presencia, no procedencia:** un bloque del dibujo con el mismo nombre gana (`BlockLibraryImporter.cs:58-64`).
- **Piezas omitidas antes del plan** por falta de bloque en el catalogo (`A/Systems/Selective/SelectiveLateralBuilder.cs:287-290`,
  `:393-396`; `A/Systems/PushBack/PushBackLoadBeamGeometry.cs:139-143`; resto): CT-BLK hace el censo; la decision por rol es de producto.

### 3.13 Comparador authored por kind (AUTH-13, X-3)

`IsSameAuthority(miembros)` en el **contrato del kind** (`A/Systems/<Kind>/`): Selectivo delega en `SelectiveAuthoredAuthority`
(`A/ProjectVariables/SelectiveAuthoredAuthority.cs:128-157`); los demas comparan el documento de diseño canonico excluyendo la metadata
interior que `RACKEDITAR` reescribe (`P/RackCommandSupport.cs:40-67`). Los **miembros** los aporta el consumidor (I-52: vistas seleccionadas;
I-55: su conjunto de hermanas mutables). El `IsSameAuthority` del reflector de I-52 delega en este comparador (I-52 V17 §5.4 ya lo lista
como pendiente).

### 3.14 Caracterizaciones y dueños (AUTH-14)

| CT | Contenido | Dueño unico | Consumidores |
|---|---|---|---|
| **CT-04** | Lectura del sobre de vista: codec total (§3.3), regla de combinacion, tokens con espacios, lecturas del Plugin y de `RACKLISTA`; separacion sintaxis / disponibilidad | FOUNDATION_ID | I-52, I-55 |
| **CT-05** | Marco completo: ejes con signo, origen y desplazamiento por variante, `[K_min, K_max]` por variante, convencion de extremos, centro `c`, envolvente dibujada; Cantilever con origen D interior | FOUNDATION_ID | I-52 (su `c`; CT-06 sigue siendo de I-52), I-55 (anclas) |
| **CT-16** | Semantica observable del nucleo de seleccion sobre codigo intacto, antes de extraer | FOUNDATION_ID | I-52, I-55 |
| CT-RES | Paridad de resolucion por kind (persistido frente a lo dibujado y a lo que abre el editor) con fixtures legacy; lista cerrada de formas; precedencia; BOM vigente y dos pasadas | FOUNDATION_ID | I-52, I-55, BOM |
| CT-PLAN | Plan de cada direccion igual a la salida vigente de builders y lambdas; marco asociado | FOUNDATION_ID | I-52, I-55 |
| CT-NAME | Nombres base cadena a cadena, incluido H-15 | FOUNDATION_ID | I-52, I-55 |
| CT-BLK | Roles que requieren bloque en la salida real; censo de omisiones antes del plan; clasificacion de `Pallet` | FOUNDATION_ID | I-52, I-55 |
| CT-SCAN | Entradas del barrido: `ScanEnvelopes` omite xref pero no dependientes; `FindRackBlocks` no expone dependencia ni texto; cuenta de referencias desde el registro | FOUNDATION_ID | I-52, I-55 |
| CT-GEO | Valor de colocacion, descomposicion y normalizacion de angulos | FOUNDATION_ID | I-52, I-55 |
| CT-AUTH | Comparador authored por kind | FOUNDATION_ID | I-52, I-55 |

**Un dueño por caracterizacion.** Ningun consumidor vuelve a correr otra version de ellas. Un caso que un consumidor necesite y falte se
pide como **cambio** al artefacto de reconciliacion; antes de integrar lo incorpora la foundation, y despues solo una iniciativa o `fix/`
posterior que la actualice. Las pruebas de los consumidores **consumen** los fixtures, no los redefinen.

## 4. Registro durable de reclamos de extraccion

- **Ubicacion:** `reconciliation.md` §3 (este directorio); tras F0, la copia que la iniciativa de la foundation lleve a `main` en su
  integracion (hasta entonces, versiones citadas por SHA).
- **Campos:** `AUTH-ID`, `Authority`, `Foundation initiative/branch`, `Claim SHA`, `Owner`, `Implementation owner`, `Responsible branch`, `Consumers`, `Characterization owner`,
  `Integration SHA`, `Consumer policy refs`, `Status`.
- **Estados:** `UNCLAIMED → CLAIMED → CHARACTERIZED → INTEGRATED`; `WITHDRAWN` solo en una version nueva del artefacto.

| Regla | Contenido |
|---|---|
| CLM-1 | Una autoridad se extrae **exactamente una vez**, por el titular del reclamo; no existe «el primero que llegue» |
| CLM-2 | Un reclamo solo vale si su `Claim SHA` (el commit de reclamo de la iniciativa titular) figura en una version del artefacto **registrada por I-52 e I-55 con el mismo SHA** |
| CLM-3 | **Consumir = `Integration SHA` alcanzable desde `origin/main`.** Un gate de consumidor que necesite una autoridad `CLAIMED` o `CHARACTERIZED` pero no `INTEGRATED` se detiene (STOP): no hay excepcion, cherry-pick, merge parcial ni consumo de otra rama |
| CLM-4 | Ningun consumidor implementa una autoridad reclamada por otro titular |
| CLM-5 | Sustitucion y equivalencia de logica legacy (§5) |
| CLM-6 | Cambiar titular, alcance o retirar un reclamo exige una version nueva del artefacto registrada por ambas |

## 5. Sustitucion y equivalencia de logica legacy

| Caso | Regla |
|---|---|
| Codigo de `main` que hoy decodifica vistas, resuelve sistemas, arma planes o nombra bloques | La foundation lo migra a la autoridad en su gate (F2..F6) con caracterizacion previa; lo que no migre queda como productor legacy con prueba diferencial permanente y gate de retirada |
| Codigo que un consumidor tenga en su rama antes de rebasar sobre el `Integration SHA` | En el primer rebase: (a) sustituirlo por llamadas a la autoridad, o (b) conservarlo como legacy con prueba diferencial sobre los fixtures de la foundation y gate de retirada; nunca una segunda autoridad. Hoy ni I-52 ni I-55 tienen codigo de estas autoridades (ninguna abrio G3) |
| Equivalencia | Salidas identicas sobre los fixtures de CT-04/CT-05/CT-16/CT-RES/CT-PLAN/CT-NAME/CT-BLK y, para el codec, sobre todo su dominio |
| Divergencia hallada tras integrar | Gana la caracterizacion; se corrige con una unidad posterior registrada en el artefacto |

## 6. Orden de integracion

```text
Owner autoriza la iniciativa y asigna FOUNDATION_ID  →  F0 reclamo + bootstrap  →  consenso de la foundation + ADR de foundation aceptado
  →  F1 caracterizacion  →  F2..F6 extraccion sin cambio observable  →  F7 candidato  →  F8 integracion en main
  →  artefacto R(n+1) con Integration SHA registrado por I-52 e I-55
  →  I-52 rebasa sobre main (consume)        I-55 rebasa sobre main (consume)
```

Detalle de gates en `delivery-map.md`. Para AUTH-01..14 ninguna consumidora espera a integrar el otro producto: ambas esperan la
foundation en `main`. AUTH-15 queda fuera: I-55 G15 puede esperar su integracion por I-52, sin bloquear la entrega neutral.

## 7. Archivos (propuesta)

| Ruta | AUTH | Gate |
|---|---|---|
| `A/Views/RackViewKind.cs`, `RackViewVariant.cs`, `RackViewAddress.cs`, `RackViewDisposition.cs`, `RackViewAddressCodec.cs`, `RackViewScanFacts.cs`; renombre en `A/Systems/Shared/DimensionViewPolicy.cs`, `A/Systems/Selective/SelectiveDimensions.cs`, `A/Systems/Dynamic/DynamicViewDecorations.cs` y tres ventanas | 01-03, 06 | F2 |
| `A/Views/RackViewAvailability.cs`, `A/Views/RackViewFrame.cs`; migracion de la decodificacion de `P/RackSelectivoCommands.cs`, `P/RackDinamicoCommands.cs`, `P/RackPushBackCommands.cs`, `P/RackCantileverCommands.cs`, `P/RackCabeceraCommands.cs`, `P/ProjectVariableMutationExecutor.cs` | 04-06 | F3 |
| `A/Persistence/RackDuplicationPlan.cs` (nucleo + fachada), `A/Geometry/PlacementValue.cs`, `A/Geometry/SourceTransformFacts.cs`, `A/Geometry/AngleMath.cs` | 07, 08 | F4 |
| `A/Views/Resolution/*` (contrato, puerto, lista de formas); `P/Views/KindHandlerSystemResolver.cs`; `P/KindHandlers/*KindHandler.cs` (`ResolveSystem`), `IRackKindSystemResolver.cs` | 09 | F5a |
| `A/Views/Planning/*` (autoridad de plan), `A/Views/Naming/RackViewBaseNames.cs`; delegacion en `P/Systems/*/*DrawService.cs`, `P/Drawing/LateralHeaderDrawService.cs`, `P/Drawing/PlantaHeaderDrawService.cs`, `P/Systems/FlowBed/FlowBedDrawService.cs`, `P/RackCantileverCommands.cs` y comandos de `RACKEDITAR` (nombres) | 10, 11, 12 (tipo puro LibraryBlockRequirement y roles, creado aqui para el resultado Plan) | F5b |
| `A/Systems/<Kind>/*AuthoredComparator.cs`; `P/Systems/Shared/LibraryBlockQuery.cs` | 12 (consulta), 13 | F6 |

**Listas de I-52 que estos archivos tocan** (V17 §19 STOP: `RackDuplicationPlan.cs`, `BlockLibraryImporter.cs` solo consumo, resolvers,
builders solo consumo): se preregistran en el artefacto de reconciliacion para que una Proposal posterior de I-52 los clasifique como cambios esperados, no STOP.
**No se tocan:** `P/Systems/Shared/SystemBlockWriter.cs`, `P/Drawing/LateralHeaderDrawer.cs`, `P/Drawing/BlockLibraryImporter.cs`, la geometria de los builders, `P/RackDuplicarCommands.cs`, `P/RackCloner.cs`,
`A/Bom/*` salvo consumo, `A/CustomProperties/*`.

## 8. Riesgos de la foundation

| # | Riesgo | Mitigacion |
|---|---|---|
| SVF-RK1 | La migracion de lambdas de plan o nombres cambia una salida | CT-PLAN y CT-NAME antes de extraer; paridad permanente; OV-FND |
| SVF-RK2 | El adaptador `ResolveSystem` cambia un BOM o un motivo | CT-RES byte a byte; guardas de §3.9.3 sin cambio; OV-FND |
| SVF-RK3 | El renombre choca con I-49 G10 en `RackSelectiveWindow.xaml.cs` | Serializar |
| SVF-RK4 | I-52 no adopta el mecanismo o reclama otra titularidad | MATERIAL CONFLICT de proceso registrado; decision de los Coordinadores y del Owner; Proposal posterior de I-52 (reconciliation.md §4) |
| SVF-RK5 | La tolerancia de escala no esta registrada antes de F1 | STOP de F1 |
| SVF-RK6 | Una caracterizacion revela un par sin marco o una forma legacy sin paridad | `FrameUnavailable` / `Unsupported`; nunca se ajusta el descriptor |
| SVF-RK7 | Workflow V2 (I-56) entra en vigor antes del reclamo | La iniciativa nueva seguiria las reglas vigentes al reclamarse (I-56 T8, OWN-E pendiente del Owner) |

## 9. Independencia de decisiones de producto (SVF-R6)

| Decision | ¿La necesita algun contrato de la foundation? |
|---|---|
| M-01 (modos, marcos, sentido del eje comun, variante en misma clase) | No: la foundation da marco y tramo; la politica de anclaje es de ID19 |
| OD-1..OD-8 | No |
| ADR-0042 | No: ADR-0042 referencia a la foundation, no al reves |
| Decisiones del Owner de `RACKMIRROR` (O-1, OW-*) | No: la politica del espejo queda fuera |
| Tolerancia de escala | Solo su **valor**, que se registra en el artefacto; no es decision de producto de ninguna de las dos |

## 10. Preguntas abiertas de la foundation

| # | Pregunta | Se resuelve en |
|---|---|---|
| FQ-01 | Mecanismo (A/B/C) e ID | Owner (Proposal V5 de I-55 §2) |
| FQ-02 | Acordar la entrega neutral de AUTH-07/08/13 y CT-16 antes de ambos productos; si I-52 rechaza, MATERIAL CONFLICT y STOP F1. No se sacan hacia la rama producto como alternativa implementable | Artefacto EFFECTIVE antes de F1 |
| FQ-03 | Valor de la tolerancia de escala | Artefacto (I-52 lo propone) |
| FQ-04 | ¿Algun caso bloqueante no produce sistema? | CT-RES |
| FQ-05 | `Pallet` como `OptionalVisual` | CT-BLK |
| FQ-06 | Frontal legacy del Selectivo con `Section = −1`: `Coerced` o `Canonicalizable` | CT-04 |
| FQ-07 | Precedencia de `Resolve` por kind distinta de la lista | CT-RES |
| FQ-08 | Numero del ADR de foundation PENDING; se repetira el censo al congelar. Censo historico, no reserva: libres en `main` desde 0036 salvo 0037 y 0039; tomados en ramas activas 0036 (I-52), 0038, 0040 y 0041 (I-49) y 0042 (I-55) al 2026-09-14; fetch 2026-09-15: I-49 publico ADR-0043-P1 en d54a8d7, tambien ocupado; numero neutral PENDING | Al congelar la iniciativa |
