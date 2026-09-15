# I-55 — Proposal V5: View Placement & Projection (ID17 + ID18 + ID19) sobre una Shared View Foundation

```text
PROPOSAL V5 — NOT CONSENSUS
Coordinator                         = REVIEW REQUIRED (G2F: AR4-01..AR4-50 aceptados como entrada tecnica vinculante)
Architect formal                    = PENDING (la revision AR4 de V4 fue de la sesion autora: NO es veredicto formal;
                                      paquete V5 para un Arquitecto independiente: I-55-architect-review-package-v5.md)
Owner                               = PENDING (mecanismo e ID de la foundation; M-01 con la parte nueva OD-7.e; OD-1..OD-8; ADR-0042)
Consensus                           = NOT REACHED
Implementation                      = BLOCKED (SUBSTANTIVE IMPLEMENTATION BLOCKED)
Open Material                       = SVF-MECH (mecanismo + FOUNDATION_ID, Owner)
                                      SVF-REC  (artefacto de reconciliacion EFFECTIVE: mismo SHA registrado en I-52 e I-55; sin excepcion)
                                      X-2 y X-8 = MATERIAL CONFLICT DE PROCESO (hasta acordar mecanismo y orden de integracion)
                                      M-01 (Owner)
Shared View Foundation              = docs/architecture/shared-view-foundation/  (DRAFT · FOUNDATION_ID = PENDING · NOT CLAIMED)
ADR-0042                            = PROPUESTO · ADR DE PRODUCTO · complementa ADR-0010 (que sigue ACEPTADO)
VIEW FOUNDATION ADR                 = borrador PROPUESTO · NUMERO PENDIENTE (docs/architecture/shared-view-foundation/adr-draft.md)
ADR-0034                            = SIN MODIFICAR (§8.1)

Estado de V4 (fe70d7a)              Revision tecnica adversarial AR4 (sesion autora) → Coordinator: entrada vinculante → V5
                                    Architect formal = PENDING
Estado de V3 (8e35a51)              Coordinator: CQ-01 → V4 · Architect formal = PENDING
Estado de V2 (f84f303)              Coordinator = CHANGES REQUIRED → V3 · Architect formal = PENDING

Initiative     = I-55 — View Placement & Projection
Branch         = feature/creacion-de-vistas
BASE_SHA       = ba497f14581d81e83a27514852d6ec082ff57635   (base original; codigo auditado por el Discovery)
CURRENT_BASE   = dad4e77f4f267b9fa248ecb0c8bfd8a74bbab093
CURRENT_MAIN   = dad4e77f4f267b9fa248ecb0c8bfd8a74bbab093   (preflight de G2F: main no avanzo)
Historial      = V1 d1918ab · V2 f84f303 · G2C d091eeb · V3 8e35a51 (paquetes 110cd39) · V4 fe70d7a (paquetes 8068368)
Paralelas      = I-52 b7a6d9f (Proposal V17, REVIEW REQUIRED; V16 del takeover avanzo antes del commit;
                 no adopta la reconciliacion; coordinacion con I-55 HIGH / ACTIVE)
                 I-49 d54a8d7 (ADR-0043-P1 propuesto; base A3-R2 acordada) · I-56 f97a8b0 (revision de Arquitecto de su Proposal V2; Workflow V2 NOT EFFECTIVE;
                 I-55 grandfathered)
Decisiones     = docs/automation/decisions/I-55.md   (G2B CR-01..12; G2D CD2-01..12; G2E CQ-01; G2F GF-01..GF-34)
Mapa           = docs/initiatives/I-55-implementation-map-v5.md   (parte A: entrega de la foundation; parte B: producto)
Citas          = archivo:linea sobre CURRENT_BASE; citas historicas de I-52 a V16 53ae5fa se conservan identificadas;
                 conciliacion vigente con V17 b7a6d9f en §7.1 (mapeo por secciones, sin reutilizar numeros de linea de V16)
Prefijos       = P/ src/RackCad.Plugin/ · A/ src/RackCad.Application/ · D/ src/RackCad.Domain/ · U/ src/RackCad.UI/
                 T/ tests/RackCad.Tests/ · TU/ tests/RackCad.UI.Tests/ · SVF/ docs/architecture/shared-view-foundation/
```

> **Como leer V5.** V5 corrige la **arquitectura compartida** y los contratos que derivan de ella; no rediseña el producto.
> - **Normativa por sustitucion e incorporacion.** V5 es el documento vigente. Las secciones de V4 (`fe70d7a`) que el inventario de §0.5
>   marca **INCORPORADA** siguen vigentes por referencia a ese SHA con las enmiendas que V5 indica; las marcadas **SUSTITUIDA** dejan de
>   estarlo. En conflicto gana V5. V1..V4 quedan intactas como historial.
> - **Dos capas.** La **Shared View Foundation** (`SVF/specification.md`) contiene hechos y contratos neutrales que I-52 e I-55 consumen
>   **una vez integrados en `main`**; I-55 aporta solo **politica de producto** sobre ellos (§1, §3).
> - **Vocabulario** de V4 sin cambio: seleccion proyectada, grupo `RackId`, redibujo de hermanas existentes frente a colocacion de vistas
>   nuevas.

## 0. Reconciliacion V4 → V5

### 0.1 Estado de las revisiones

| Pieza | Estado |
|---|---|
| Proposal V1 (`d1918ab`) | Coordinator = CHANGES REQUIRED (G2B) |
| Proposal V2 (`f84f303`) | Coordinator = CHANGES REQUIRED → V3 (G2D); Architect formal = PENDING |
| Revision G2C (`d091eeb`) | Revision tecnica de la sesion autora; no es veredicto del Arquitecto |
| Proposal V3 (`8e35a51`) | Coordinator: CQ-01 (G2E) → V4; Architect formal = PENDING |
| Proposal V4 (`fe70d7a`) y paquetes (`8068368`) | Revision tecnica adversarial AR4-01..AR4-50 (BLOCKER 1, HIGH 4, MEDIUM 24, LOW 21) con procedencia declarada en el borrador heredado, sin informe separado recuperado; el Coordinador la acepta como **entrada tecnica vinculante** (G2F), **no** como veredicto formal. Architect formal = PENDING |
| Proposal V5 (este documento) | Coordinator = REVIEW REQUIRED; Architect formal = PENDING, para un Arquitecto **genuinamente independiente** sobre el SHA exacto de V5 |

### 0.2 Orden G2F: puntos vinculantes

GF-01..34 es la normalizacion heredada de la orden previa. La orden de takeover aceptado del 2026-09-15 prevalece,
incluida la prohibicion de bypass del registro exacto y la comparacion con la ultima Proposal publicada. No es una segunda orden literal.

| # | Punto de la orden | Seccion |
|---|---|---|
| GF-01 | Conservar ID17/ID18/ID19, CQ-01 (sin partial-redraw), ALT-B, variantes tipadas, authored/effective, ciclo de `RackId`, gate de propiedades, requisito estructural, `CommonTransform2D`, `Rigid`/`Orthographic`, PR-1, PR-2, H-13 fuera, invariantes de BOM y listado | §0.3 |
| GF-02 | Shared View Foundation integrable antes de que I-52 o I-55 la consuman; `FOUNDATION_ID = PENDING`; opciones A/B/C; rechazar cherry-pick, consumo no integrado, merge parcial y excepciones implicitas | §2; SVF |
| GF-03 | Nucleo neutral frente a politica de producto; candidatos y exclusiones | §1; SVF/specification.md §1 |
| GF-04 | Registro durable de reclamos; consumir = integrado en `main`; sustitucion/equivalencia; un dueño por CT-04, CT-05, CT-16 | §2.6; SVF/specification.md §3.14, §4, §5; SVF/reconciliation.md §3 |
| GF-05 | Estado frente a I-52: X-1, X-3, X-5, X-6 compatibles; X-4 y X-7 compatibles con precisiones; X-2 y X-8 MATERIAL CONFLICT DE PROCESO; releer V15 (y V16, publicada durante la redaccion); si no adopta, I-52 necesita Proposal posterior | §7.1 |
| GF-06 | ADR-0034: contrato `Resolve` → adaptador por kind → el Selectivo delega en la autoridad del handler; revisar guardas | §3.3, §8.1; SVF/specification.md §3.9.3 |
| GF-07 | ADR-0042 como ADR de producto; ADR de foundation neutral, propuesto, sin numero | §8.2, §8.3; ADR-0042; SVF/adr-draft.md |
| GF-08 | Una autoridad de plan | §3.9; SVF/specification.md §3.10 |
| GF-09 | CT-05 con origen, ejes, tramo, `c` y desplazamientos; el consumidor elige | §5.4; SVF/specification.md §3.5 |
| GF-10 | Disponibilidad = hechos; tres niveles | §3.2; SVF/specification.md §2, §3.4 |
| GF-11 | `Resolved { EffectiveSystem, Diagnostics }` | §3.3; SVF/specification.md §3.9.1 |
| GF-12 | `UnsupportedLegacy` con lista cerrada | §3.3; SVF/specification.md §3.9.2 |
| GF-13 | Orden gate authored → representante → `Resolve` | §3.4 |
| GF-14 | Validez de clave de biblioteca; clave semantica ≠ nombre generado | §3.5; SVF/specification.md §3.11, §3.12 |
| GF-15 | Una autoridad de nombre base; `UniqueBlockName` en el Plugin; Cantilever sin reaplicar `SuggestName` | §3.6; SVF/specification.md §3.11 |
| GF-16 | Un clasificador compartido por Insertar, Actualizar y PVME | §3.7; SVF/specification.md §3.6 |
| GF-17 | Remedio por kind + disposicion + disponibilidad + autoridad | §3.8 |
| GF-18 | Frontal legacy del Selectivo con `Section = −1` → `Coerced` salvo demostracion | §3.1 |
| GF-19 | Precisiones de CQ-01 (superviviente, pertenencia unica, `Mutate(units)`, `TopTransaction`, capas bloqueadas, R-25, `Id` en blanco, gate authored, `IsNullOrWhiteSpace`, xref) | §4 |
| GF-20 | Ida y vuelta corregida | §5.1 |
| GF-21 | M-01: ventana relativa (A) frente a mayoria (B) con ejemplos; preferencia inicial del Coordinador A sin registrar decision del Owner | §5.2 |
| GF-22 | Invariante sobre la geometria colocada real con matriz independiente | §5.3 |
| GF-23 | Enumerar los cambios de I-55 sobre Insertar; ADR-0042 complementa ADR-0010 | §6 |
| GF-24 | Artefacto de reconciliacion estable con el mismo SHA en ambas | §2.6; SVF/reconciliation.md |
| GF-25 | LOW de precision y pruebas | §0.4 (AR4-30..AR4-50) |
| GF-26 | Dos mapas: entrega de la foundation y entrega de producto | mapa V5; SVF/delivery-map.md |
| GF-27 | Especificacion, artefacto y mapa de la foundation sin reclamar rama | SVF |
| GF-28 | Paquetes del Coordinador y del Arquitecto independiente | paquetes V5 |
| GF-29 | Registro de decisiones, contrato y ADR-0042 propuesto; ADR de foundation propuesto sin numero; V1..V4 intactas | decisions/I-55.md §G2F; contrato; ADR-0042 |
| GF-30 | Owner PENDING; sin registrar aceptaciones | §10.2 |
| GF-31 | La revision AR4 no se registra como veredicto formal | §0.1 |
| GF-32 | Ataques 1-25 antes de publicar; sin BLOCKER/HIGH abiertos | §11 |
| GF-33 | NO implementacion, NO RED productivo, NO merge, NO G3 | cabecera; §10 |
| GF-34 | Recomendacion de sentido del eje comun ya no es automaticamente A; el resto de OD conserva su recomendacion salvo evidencia | §5.2, §10.2 |

### 0.3 Lo que V5 conserva

ID17, ID18 e ID19 como objetivos (V4 §1). CQ-01 tal como la decidio el Coordinador: redibujo de hermanas existentes **PREPARE ALL → una
MUTATE → un commit → POST**, y vistas nuevas con jig y commit individuales (V4 §11); V5 solo lo precisa (§4). ALT-B; variantes tipadas;
separacion authored/effective; ciclo de vida del `RackId` (RID-1..RID-4; RID-5 precisado en §4.11); gate de propiedades acotado al `RackId`;
requisito estructural de bloques por pieza; `CommonTransform2D` y politicas `Rigid`/`Orthographic`; PR-1 y PR-2; H-13 fuera; INV-BOM-1..3 e
INV-LIST-1..3. Las recomendaciones A de OD-1..OD-8 salvo el sentido del eje comun (§5.2, §10.2).

### 0.4 Matriz de resolucion AR4-01..AR4-50

AR4 es entrada tecnica vinculante por instruccion del Coordinador aceptada en el takeover; no es una revision formal independiente.
No se encontro un informe AR4 separado en los commits publicados: se preserva la matriz heredada y su autoridad procede de esa orden,
sin inventar una procedencia adicional. Las resoluciones siguientes son propuestas documentales, no evidencia ejecutada.
`NeedsArchitect = Si` significa revision formal de V5 pendiente. `NeedsCharacterization` nombra trabajo futuro, no pruebas realizadas.
`NeedsOwner` distingue decisiones materiales de la validacion de comportamiento al entregar producto (OV).


| ID | Severity | V4 problem | V5 resolution | Section | Foundation or product | NeedsCharacterization | NeedsOwner | NeedsArchitect | Status |
|---|---|---|---|---|---|---|---|---|---|
| AR4-01 | BLOCKER | Extractor unico I-55 + «consume si existe en su base» deja I-52 G4..G6 esperando a I-55 G16 | **Shared View Foundation** separada, integrable sola; I-52 e I-55 consumen solo lo integrado en `main`; mecanismo recomendado B (iniciativa neutral, ID del Owner); I-55 no extrae ninguna autoridad compartida | §1, §2; SVF/specification.md; SVF/delivery-map.md | Foundation + politica de producto | No; pruebas de producto en mapa | SVF-MECH/ID | Si, PENDING | ABIERTO: consenso/mecanismo/reconciliacion |
| AR4-02 | HIGH | «Primer gate» entre ramas; CT-04/05/16 duplicables | Registro durable de reclamos (CLM-1..CLM-6), consumir = `Integration SHA` en `main`, sustitucion/equivalencia, un dueño por caracterizacion (la foundation) | §2.6; SVF/specification.md §3.14, §4, §5; SVF/reconciliation.md §3 | Foundation + politica de producto | CT-04/05/16 | Titularidad si escalada | Si, PENDING | ABIERTO: consenso/mecanismo/reconciliacion |
| AR4-03 | HIGH | «Sin MATERIAL CONFLICT» engañoso | X-2 y X-8 = **MATERIAL CONFLICT DE PROCESO**; clausulas de extraccion de X-1, X-3, X-4 y X-7 registradas como conflicto; secciones de I-52 V16 afectadas listadas; adoptar exige una Proposal posterior de I-52 | §7.1 | Foundation + politica de producto | No; pruebas de producto en mapa | Conflictos X-2/X-8 si escalada | Si, PENDING | ABIERTO: consenso/mecanismo/reconciliacion |
| AR4-04 | HIGH | G7a movia la resolucion del Selectivo fuera del handler (ADR-0034 §10) | Contrato `Resolve` en Application → adaptador del puerto en el Plugin → `ResolveSystem` dentro de cada handler; el Selectivo conserva su unica construccion del resolver; **ADR-0034 sin cambio**; guardas revisadas | §3.3, §8.1; SVF/specification.md §3.9.3 | Foundation + politica de producto | CT-RES (una llamada) | No decision de producto | Si, PENDING | PROPUESTO; caracterizacion pendiente |
| AR4-05 | HIGH | ADR-0042 mezclaba foundation, producto y proceso de extraccion | ADR-0042 = ADR de producto sin lenguaje de extractor/gate/ramas; borrador de VIEW FOUNDATION ADR neutral, propuesto, sin numero, aceptable sin M-01/OD/ADR-0042/RACKMIRROR | §8.2, §8.3; ADR-0042; SVF/adr-draft.md | Foundation + politica de producto | No; pruebas de producto en mapa | Aceptar ambos ADR por separado | Si, PENDING | ABIERTO: consenso/mecanismo/reconciliacion |
| AR4-06 | MEDIUM | Plan implementado dos veces (planificador y lambdas del Plugin) | **Una autoridad de plan** en la foundation; las lambdas delegan (F5b); registro de productores legacy con caracterizacion, paridad y retirada; I-55 no tiene planificador propio | §3.9; SVF/specification.md §3.10 | Foundation + politica de producto | CT-PLAN | No decision de producto | Si, PENDING | PROPUESTO; caracterizacion pendiente |
| AR4-07 | MEDIUM | CT-05 perdia `c` | CT-05 cubre origen, ejes, tramo, centro y desplazamientos; I-52 toma `c` = `Center` (sin forma afin; CT-06 sigue en I-52) | §5.4, §7.1; SVF/specification.md §3.5 | Foundation + politica de producto | CT-05 | No decision de producto | Si, PENDING | PROPUESTO; caracterizacion pendiente |
| AR4-08 | MEDIUM | Disponibilidad con politica de ID19 | Hechos `Available`/`VariantNotPresent`/`SystemDoesNotSupportKind`/`Unavailable`; matriz de exposicion = politica de I-55 | §3.2; SVF/specification.md §3.4 | Foundation + politica de producto | CT-04 | No decision de producto | Si, PENDING | PROPUESTO; caracterizacion pendiente |
| AR4-09 | MEDIUM | `Blocked` sin sistema | `Resolved { EffectiveSystem, Diagnostics }`; `OutputBlocking` como diagnostico | §3.3; SVF/specification.md §3.9.1 | Foundation + politica de producto | CT-RES | No decision de producto | Si, PENDING | PROPUESTO; caracterizacion pendiente |
| AR4-10 | MEDIUM | `UnsupportedLegacy` indecidible | Lista cerrada de formas con predicados deterministas (`Current`, `LegacyAccepted`, `Canonicalizable`, `Unsupported`, `Unresolved`) fijada por CT-RES | §3.3; SVF/specification.md §3.9.2 | Foundation + politica de producto | CT-RES | No decision de producto | Si, PENDING | PROPUESTO; caracterizacion pendiente |
| AR4-11 | MEDIUM | `Resolve` antes del gate authored y sobre un representante sin nombrar | Orden: gates de autoridad → representante definido → `Resolve` una vez por `RackId` | §3.4 | Producto | No; pruebas de producto en mapa | OV de producto | Si, PENDING | CORRECCION DOCUMENTAL PROPUESTA; revision pendiente |
| AR4-12 | MEDIUM | Predicado de nombre rechazaba claves reales con `.` | Clave valida = no nula y no en blanco; existencia por consulta; clave de biblioteca ≠ nombre generado | §3.5; SVF/specification.md §3.12 | Foundation + politica de producto | CT-BLK | No decision de producto | Si, PENDING | PROPUESTO; caracterizacion pendiente |
| AR4-13 | MEDIUM | Semilla duplicaba nombres del Plugin; Cantilever reaplica `SuggestName` | Autoridad unica de nombre base (A + B); insercion Cantilever por `CreateBlockDefinitionNamed`; CT-NAME cadena a cadena | §3.6; SVF/specification.md §3.11 | Foundation + politica de producto | CT-NAME | No decision de producto | Si, PENDING | PROPUESTO; caracterizacion pendiente |
| AR4-14 | MEDIUM | Tres decodificaciones de hermanas | Clasificador compartido de la foundation para Insertar, Actualizar y PVME (F3), sin cambiar transacciones; prueba diferencial si algo no migra | §3.7; SVF/specification.md §3.6 | Foundation + politica de producto | CT-SCAN | No decision de producto | Si, PENDING | PROPUESTO; caracterizacion pendiente |
| AR4-15 | MEDIUM | «Abrela con RACKEDITAR» sin salida en varios casos | Tabla de remedios por kind, disposicion, disponibilidad y estado de autoridad | §3.8 | Producto | No; pruebas de producto en mapa | OV de producto | Si, PENDING | CORRECCION DOCUMENTAL PROPUESTA; revision pendiente |
| AR4-16 | MEDIUM (PLAUSIBLE) | Frontal Selectivo con `−1` leida como `Fondo(0)` sin prueba | `Coerced` por defecto; `Canonicalizable` solo si CT-04 lo demuestra; ID19 falla cerrado | §3.1; SVF/specification.md §3.3 | Foundation + politica de producto | CT-04 | No decision de producto | Si, PENDING | PROPUESTO; caracterizacion pendiente |
| AR4-17 | MEDIUM | Fantasma sin referencias contaba como superviviente | Superviviente = REDRAW con `LayoutReferenceCount ≥ 1`; las referencias anidadas no bastan | §4.1 | Producto | No; pruebas de producto en mapa | OV de producto | Si, PENDING | CORRECCION DOCUMENTAL PROPUESTA; revision pendiente |
| AR4-18 | MEDIUM | Gate de propiedades y clasificacion con conjuntos distintos | Una sola funcion de pertenencia para clasificacion, gate de propiedades y gate authored | §4.1 | Producto | No; pruebas de producto en mapa | OV de producto | Si, PENDING | CORRECCION DOCUMENTAL PROPUESTA; revision pendiente |
| AR4-19 | MEDIUM | Vida de la transaccion por secuencia de llamadas | `Mutate(units) → Committed \| Discarded` con el `using` dentro del adaptador; `TopTransaction == null` antes de `PLACE(1)` o `PLACEMENT_BLOCKED`; guarda de `using` en locks y `OpenCloseTransaction` | §4.4 | Producto | No; pruebas de producto en mapa | OV de producto | Si, PENDING | CORRECCION DOCUMENTAL PROPUESTA; revision pendiente |
| AR4-20 | MEDIUM | Capas bloqueadas y evidencia de rollback debil | Comprobacion de capas bloqueadas en PREPARE → `PREPARE_FAILED`; inyeccion de fallo solo en Debug para evidenciar el rollback | §4.5 | Producto | No; pruebas de producto en mapa | OV de producto | Si, PENDING | CORRECCION DOCUMENTAL PROPUESTA; revision pendiente |
| AR4-21 | MEDIUM | R-25 evitable | Sin supervivientes con referencias: redibujos sin referencias, borrados, definicion, sobre y referencia en la transaccion de la primera colocacion con AUTH-15 integrado (cancelar = abortar; R-25 eliminado sujeto a OV-RED-04); sin AUTH-15, conducta de V4 con R-25 residual; I-55 no compone un primitivo propio | §4.6 | Producto | No; pruebas de producto en mapa | OV-RED-04; R-25 | Si, PENDING | CONDICIONAL: AUTH-15 integrado o R-25 residual |
| AR4-22 | MEDIUM | Lateral/planta elegida con `Id` en blanco no se redibujaba | La fuente elegida con `Id` en blanco (vacio o de espacios) siempre es miembro y siempre se cura y redibuja (Selectivo, Dinamico, Cabecera); Push Back y Cantilever abortan como hoy | §4.2 | Producto | No; pruebas de producto en mapa | OV de producto | Si, PENDING | CORRECCION DOCUMENTAL PROPUESTA; revision pendiente |
| AR4-23 | MEDIUM | Gate authored de ID19 sobre dependientes de xref | Mismo conjunto de miembros que el gate de propiedades | §3.4, §4.1 | Producto | No; pruebas de producto en mapa | OV de producto | Si, PENDING | CORRECCION DOCUMENTAL PROPUESTA; revision pendiente |
| AR4-24 | MEDIUM | `IsNullOrWhiteSpace` cambiaba Actualizar | Solo Selectivo y Cabecera cambian (los otros kinds ya usan `IsNullOrWhiteSpace`); la bifurcacion sube antes del calculo del `id`; guarda | §4.3 | Producto | No; pruebas de producto en mapa | OV de producto | Si, PENDING | CORRECCION DOCUMENTAL PROPUESTA; revision pendiente |
| AR4-25 | MEDIUM | Ida y vuelta mal descrita cuando la mayoria invierte `e` | Enunciado corregido con numeros | §5.1 | Producto | No; pruebas de producto en mapa | M-01 | Si, PENDING | ABIERTO: M-01; recomendacion sin eleccion Owner |
| AR4-26 | MEDIUM | `Rigid` sin invariante sobre la colocacion real | INV-GRP-5 con oraculo independiente: punto, orientacion, segundo punto y Z; ejemplos numericos | §5.3 | Producto | No; pruebas de producto en mapa | M-01; OV | Si, PENDING | ABIERTO: M-01; recomendacion sin eleccion Owner |
| AR4-27 | MEDIUM | Argumento caducado contra una regla fija de sentido | M-01 compara A (ventana relativa) y B (mayoria) con simulacion; OD-7.e; OM-24 reescrita | §5.2 | Producto | No; pruebas de producto en mapa | M-01/OD-7.e | Si, PENDING | ABIERTO: M-01; recomendacion sin eleccion Owner |
| AR4-28 | MEDIUM | ADR-0042 no enumeraba todos los cambios de Insertar | Enumeracion IC-01..IC-19 | §6; ADR-0042 | Producto | No; pruebas de producto en mapa | ADR-0042; OV | Si, PENDING | CORRECCION DOCUMENTAL PROPUESTA; revision pendiente |
| AR4-29 | MEDIUM | Relecturas mutuas sin artefacto: livelock | Artefacto de reconciliacion versionado por SHA con protocolo REC-1..REC-8 | §2.6; SVF/reconciliation.md §1 | Foundation + politica de producto | No; pruebas de producto en mapa | Escaladas; nunca bypass | Si, PENDING | ABIERTO: consenso/mecanismo/reconciliacion |
| AR4-30 | LOW | Codec no total | Totalidad sobre el producto del dominio y regla de combinacion | SVF/specification.md §3.3 | Foundation + politica de producto | CT-04 | No decision de producto | Si, PENDING | PROPUESTO; caracterizacion pendiente |
| AR4-31 | LOW | Invariante de dos pasadas del BOM | Caracterizado en CT-RES; ambas pasadas con el mismo snapshot | SVF/specification.md §3.9.3; SVF/delivery-map.md F1 | Foundation + politica de producto | CT-RES | No decision de producto | Si, PENDING | PROPUESTO; caracterizacion pendiente |
| AR4-32 | LOW | Carga del catalogo de secciones sin tipo | `SectionCatalog: ReadResult` en la peticion | SVF/specification.md §3.9.1 | Foundation + politica de producto | CT-RES | No decision de producto | Si, PENDING | PROPUESTO; caracterizacion pendiente |
| AR4-33 | LOW | «Biblioteca no disponible» solo observable como archivo ausente | `LibraryAvailability = FileMissing \| Ok \| Unknown` | SVF/specification.md §3.12 | Foundation + politica de producto | CT-BLK | No decision de producto | Si, PENDING | PROPUESTO; caracterizacion pendiente |
| AR4-34 | LOW | Union sobre instancias aplanadas; rol `Pallet` | Union aplanada; `Pallet` = `OptionalVisual` sujeto a CT-BLK; politica de I-55 en §3.5 | §3.5; SVF/specification.md §3.12 | Foundation + politica de producto | CT-BLK | No decision de producto | Si, PENDING | PROPUESTO; caracterizacion pendiente |
| AR4-35 | LOW | Tokens de vista como taxonomias | Declarados codificaciones | SVF/specification.md §3.1 | Foundation + politica de producto | CT-04 | No decision de producto | Si, PENDING | PROPUESTO; caracterizacion pendiente |
| AR4-36 | LOW | `Plan` sin marco | `Plan` devuelve el marco; paridad permanente con el builder | SVF/specification.md §3.5, §3.10 | Foundation + politica de producto | CT-PLAN/05 | No decision de producto | Si, PENDING | PROPUESTO; caracterizacion pendiente |
| AR4-37 | LOW | Productores nuevos de G9a/G9b fuera de G-M24/X-5 | Declarados en G-M24 y X-5 | §7.1; mapa V5 G9a, G9b | Producto | No; pruebas de producto en mapa | OV de producto | Si, PENDING | CORRECCION DOCUMENTAL PROPUESTA; revision pendiente |
| AR4-38 | LOW | Precedencia de fallos de `Resolve` | Lista ordenada por kind fijada por CT-RES | SVF/specification.md §3.9.1 | Foundation + politica de producto | CT-RES | No decision de producto | Si, PENDING | PROPUESTO; caracterizacion pendiente |
| AR4-39 | LOW | RID-5 impreciso; predicado de blanco por sitio | RID-5 acotado a definiciones y referencias nuevas; tabla de predicados | §4.11 | Producto | No; pruebas de producto en mapa | OV de producto | Si, PENDING | CORRECCION DOCUMENTAL PROPUESTA; revision pendiente |
| AR4-40 | LOW | Estado de abort del preflight; informe de huerfanas; `PromptStatus.Error` no observable | `PREFLIGHT_FAILED`; huerfanas borradas en el informe; `Error` del jig no observable hoy, fallo solo por excepcion | §4.8 | Producto | No; pruebas de producto en mapa | OV de producto | Si, PENDING | CORRECCION DOCUMENTAL PROPUESTA; revision pendiente |
| AR4-41 | LOW | Actualizar remedia un `Kind` en blanco | En la tabla de remedios (PLAUSIBLE, verificado en G14) | §3.8 | Producto | G14; remedio PLAUSIBLE | OV de producto | Si, PENDING | PROPUESTO; caracterizacion pendiente |
| AR4-42 | LOW | Dos barridos | Un solo barrido por Insertar (INV-SCAN-1) | §4.7 | Producto | No; pruebas de producto en mapa | OV de producto | Si, PENDING | CORRECCION DOCUMENTAL PROPUESTA; revision pendiente |
| AR4-43 | LOW | Guardas caller-owned | `CallerOwnedFacadeGuardTests` extendida a los siete envoltorios | §4.9; mapa V5 G9a | Producto | No; pruebas de producto en mapa | OV de producto | Si, PENDING | CORRECCION DOCUMENTAL PROPUESTA; revision pendiente |
| AR4-44 | LOW | Cuenta de transacciones incompleta | Cuenta completa | §4.10 | Producto | No; pruebas de producto en mapa | OV de producto | Si, PENDING | CORRECCION DOCUMENTAL PROPUESTA; revision pendiente |
| AR4-45 | LOW | Regla de empate absoluta y en grados | Relativa al marco de la familia y en radianes | §5.2 | Producto | No; pruebas de producto en mapa | M-01/OD-7.e | Si, PENDING | ABIERTO: M-01; recomendacion sin eleccion Owner |
| AR4-46 | LOW | `atan2(−0.0, −1) = −π` | `NormalizePi` con −π ↦ +π | §5.4; SVF/specification.md §3.8 | Foundation + politica de producto | CT-GEO | No decision de producto | Si, PENDING | PROPUESTO; caracterizacion pendiente |
| AR4-47 | LOW (PLAUSIBLE) | Superposicion por convencion o por envolvente | CT-05 registra ambas; ID19 usa el tramo de convencion | §5.4; SVF/specification.md §3.5 | Foundation + politica de producto | CT-05 | No decision de producto | Si, PENDING | PROPUESTO; caracterizacion pendiente |
| AR4-48 | LOW | Oraculos de G14 desde el descriptor productivo | Oraculos desde CT-05 | §5.4; mapa V5 G14 | Producto | CT-05 (consumo) | OV de producto | Si, PENDING | PROPUESTO; caracterizacion pendiente |
| AR4-49 | LOW | Terminologia de escala y reflexion; SCP con Z | Terminologia unica; SCP con Z = +Z | §5.4; SVF/specification.md §3.8 | Foundation + politica de producto | CT-GEO | No decision de producto | Si, PENDING | PROPUESTO; caracterizacion pendiente |
| AR4-50 | LOW | Tabla de marcos sin desplazamiento por variante | Columna nueva | SVF/specification.md §3.5 | Foundation + politica de producto | CT-05 | No decision de producto | Si, PENDING | PROPUESTO; caracterizacion pendiente |

### 0.5 Inventario de secciones de V4

| V4 | Estado en V5 | Enmienda |
|---|---|---|
| §0 | SUSTITUIDA | §0 |
| §1 Contrato de producto | INCORPORADA | — |
| §2 Principios P1..P21 | INCORPORADA | P6: el renombre lo hace la foundation (AUTH-01); P9: el nucleo de seleccion es AUTH-07 |
| §3 Linea base | INCORPORADA | La fila «Resolucion sin editor» se lee con §3.3; «Vista enlazada» con §4 |
| §4.1 Responsabilidades | SUSTITUIDA | §1 |
| §4.2 Tipos | SUSTITUIDA | §1 (foundation) y §3 (tipos de producto) |
| §4.3 Preparacion por sistema | INCORPORADA como insumo de la autoridad de plan de la foundation | SVF/specification.md §3.10 |
| §4.4 Contrato `Resolve`/`Plan`/`Prepare` | SUSTITUIDA | §3.9, §3.10; SVF/specification.md §3.9-§3.11 |
| §4.5 Resolucion sin editor | SUSTITUIDA | §3.3; SVF/specification.md §3.9 |
| §4.6 Requisito estructural | SUSTITUIDA | §3.5; SVF/specification.md §3.12 |
| §5 Matriz normativa | INCORPORADA como **matriz de exposicion de I-55 (politica)** | La columna «ID19 (foundation)» se lee «exposicion de ID19»; «vista soportada» usa los hechos de §3.2 |
| §6 Ciclo del `RackId` | INCORPORADA | RID-5 precisado (§4.11); RID-4 con §4.2 y §4.3 |
| §7.1 Taxonomia | SUSTITUIDA | SVF/specification.md §3.1 |
| §7.2 Variante | SUSTITUIDA | SVF/specification.md §3.2 |
| §7.3 A codec | SUSTITUIDA | SVF/specification.md §3.3 (con AR4-16) |
| §7.3 B disponibilidad | SUSTITUIDA | SVF/specification.md §3.4 |
| §7.3 C politica | INCORPORADA como politica de consumidores | Fila ID19 y diagnostico sustituidos por §3.1, §3.2 y §3.8; «Otros lectores» migran en F3 (§3.7) |
| §8 Authored y metadatos | INCORPORADA | — |
| §9.1 Principio | INCORPORADA | — |
| §9.2 Gate de propiedades | INCORPORADA con enmiendas | Pertenencia por §4.1; `Id` en blanco por §4.2 y §4.3; sonda de `Id` como hecho del barrido (SVF AUTH-06) |
| §9.3 Authored | INCORPORADA con enmiendas | ID19: §3.4; comparador = AUTH-13 |
| §9.4 Donde aplica | INCORPORADA | — |
| §10 ID17 | INCORPORADA | — |
| §11 ID18 y redibujo atomico | INCORPORADA con enmiendas | §4 (precisiones de CQ-01); plan de las unidades desde la autoridad de plan (§3.9) |
| §12.1 Flujo de ID19 | SUSTITUIDA en el orden de etapas | §3.4 |
| §12.2 Nivel 1 | INCORPORADA con enmiendas | INV-GRP-5 (§5.3); normalizacion y terminologia (§5.4) |
| §12.3 Descriptores de marco | SUSTITUIDA | SVF/specification.md §3.5 |
| §12.4 `Rigid` | INCORPORADA | INV-GRP-5 |
| §12.5 `Orthographic` | INCORPORADA con enmiendas | Sentido del eje comun: §5.2; ida y vuelta: §5.1 |
| §12.6 Que representa la foundation | INCORPORADA | — |
| §12.7 M-01 | INCORPORADA con enmiendas | OD-7.e (§5.2, §10.2) |
| §12.8 Seleccion, grupos, clasificacion | INCORPORADA con enmiendas | D-08a: nucleo = AUTH-07; D-08f: terminologia de §5.4 |
| §12.9 Salidas bloqueadas y materializacion | INCORPORADA con enmiendas | D-17 por §3.3; D-13 por §3.5 |
| §13 Invariantes | INCORPORADA con enmiendas | INV-AUTH-1 (§4.6), INV-GRP-5, INV-SCAN-1, INV-TX-1 (§10.1) |
| §14 Cancelacion y errores | INCORPORADA con enmiendas | Filas de §4.5, §4.6, §4.8 |
| §15 Transacciones | INCORPORADA con enmiendas | §4.10 |
| §16 Recursos | INCORPORADA con enmiendas | §4.7, §4.10 |
| §17.1 I-49 | SUSTITUIDA | §7.2 |
| §17.2 I-52 y tabla X | SUSTITUIDA | §7.1; SVF/reconciliation.md |
| §17.3 I-53D | INCORPORADA | — |
| §17.4 PR-1 y PR-2 | INCORPORADA con enmiendas | D-09: tras integrar la foundation (§7.5) |
| §18 Alternativas | INCORPORADA con adiciones | §10.1 (alternativas nuevas) |
| §19 Pruebas | SUSTITUIDA | mapa V5 §T |
| §20 Validacion del Owner | SUSTITUIDA | mapa V5 §OV |
| §21 Gates | SUSTITUIDA | §9; mapa V5 |
| §22 Registro | SUSTITUIDA | §10 (H-13..H-16 de §22.5 incorporados) |
| §23 Revision adversarial de V4 | Historial | §11 para V5 |

## 1. Separacion: foundation neutral y politica de producto

| Responsabilidad | Foundation (SVF, hechos y contratos) | I-55 (politica de producto) |
|---|---|---|
| Tipo de vista, variante, direccion | AUTH-01, AUTH-02 | Que direcciones escribe ID17/ID18 (solo `Canonical`) |
| Lectura de `View`/`Section` | AUTH-03 codec total con disposicion | Aceptacion por consumidor (§3.1); remedios (§3.8) |
| Existencia en el sistema resuelto | AUTH-04 hechos de disponibilidad | Matriz de exposicion (V4 §5 incorporada); fallo cerrado de ID19 (§3.2) |
| Barrido de definiciones | AUTH-06 hechos por definicion | Pertenencia de hermanas, conjuntos mutable/solo lectura, curacion (§4.1..§4.3) |
| Marco y tramo | AUTH-05 | Anclas de `Rigid`/`Orthographic`, sentido del eje comun (M-01), superposicion (OD-7.b) |
| Seleccion | AUTH-07 nucleo neutral | Mensajes, orden de fallos, grupos de ID19 |
| Transformacion | AUTH-08 valor de colocacion y hechos de la fuente | Clases de escala aceptadas por ID19; `CommonTransform2D`; Z |
| Resolucion | AUTH-09 `Resolve` + adaptadores | ID19 solo `Resolved` sin bloqueo y con forma aceptada (§3.3) |
| Plan | AUTH-10 autoridad unica | `Prepare` de producto = politica + `Plan` + composicion del sobre (§3.9) |
| Nombre base | AUTH-11 | Uso en Insertar, lote y proyeccion; renombres en POST |
| Bloques | AUTH-12 requisito + consulta | Fallo antes de escribir (ID19) o colocar y reportar (ID17/ID18) |
| Comparador authored | AUTH-13 | Miembros = conjunto mutable (§4.1) |
| Caracterizaciones compartidas | CT-04, CT-05, CT-16, CT-RES, CT-PLAN, CT-NAME, CT-BLK, CT-SCAN, CT-GEO, CT-AUTH | Caracterizaciones de producto (mapa V5 G3) |
| Fuera de la foundation | — | `RACKPROYECTAR`; politicas de ID19; OD; M-01; cola de ID18; flujo de Insertar; redibujo atomico (CQ-01); gate de propiedades; ADR-0042 |

## 2. Mecanismo de la foundation

### 2.1 Problema

I-52 necesita autoridades de X-2 y X-8 en sus G4..G6; en V4, I-55 las extraia en G6/G7 dentro de su rama, que solo se integra al cerrar
(G16). Las ramas nacen de `origin/main` (`docs/WORKFLOW.md:136`) y una dependencia solo cuenta cuando esta integrada en `origin/main`
(«Una rama terminada o un Pull Request aprobado no bastan», `docs/AUTOMATION_PLAN.md:178-180`). «El primero que llegue lo extrae» no
funciona entre ramas independientes.

### 2.2 Que permite WORKFLOW

- Una iniciativa = una rama, un worktree y una fila de ROADMAP (`docs/WORKFLOW.md:34`); solo se abre con fila planificada o con
  **autorizacion explicita del Owner** (caso d, `:39-42`), seguida de reclamo atomico y bootstrap inmediato (`:67-78`).
- **ROADMAP** solo se edita en tres momentos: al planificar, en el bootstrap tras el reclamo de un caso (d) y al integrar o cerrar
  (`:58-65`).
- **Quien asigna el ID:** WORKFLOW no lo dice; en la practica llega en la orden autorizada por el Owner (reclamo de I-55 `24bb9ca`;
  I-36A/B, I-37A, I-39A por decision del Owner; I-30/I-31 por un arreglo documental autorizado en `main`, `8a1bce5`). Ni el Coordinador ni
  un «Orquestador» tienen autoridad propia en WORKFLOW; ningun agente elige el ID.
- **Precedentes de «integrar antes»:** I-30 → I-31 → reanudacion de I-18 (`docs/ROADMAP.md:429`, `:586`); I-53 E1 → I-53S → I-53D
  (`:471-472`). **Sin precedente** de una subiniciativa que se integre mientras la rama de su iniciativa madre sigue abierta (I-53 E1 fue la
  propia rama madre).
- **`experiment/`:** no se integra directamente (`docs/WORKFLOW.md:21`).
- **ADR:** sin reserva de numero; se redacta sin numero y se numera al congelar (precedentes I-53 e I-54).
- **I-56 (Workflow V2):** no vigente (`f97a8b0`: revision de Arquitecto de su V2 = CHANGES REQUIRED); sus reglas propuestas van en la misma
  direccion (prerrequisito integrado antes; solo foundations integradas son elegibles).

### 2.3 Opciones

| Criterio | A. Subiniciativa de I-55 (ID del Owner) | B. Iniciativa neutral (ID del Owner) | C. Otros mecanismos |
|---|---|---|---|
| Legal en WORKFLOW | Si, con autorizacion del Owner (caso d) | Si, con autorizacion del Owner (caso d, o fila planificada por arreglo documental en `main`) | C1 «el primer consumidor la construye dentro de su integracion completa»: legal pero no integra antes; C2 `experiment/`: no se integra; C3 arreglo documental: es la entrada de B |
| Decisiones del Owner | ID, autorizacion, **decision explicita recomendable** de integrarla antes de cerrar I-55 (sin precedente), actualizacion de ROADMAP en su momento | ID, slug (prefijo `architecture/`), autorizacion, ADR de foundation, actualizacion de ROADMAP en el momento de WORKFLOW | — |
| Custodio | Linea de producto I-55; I-52 cederia autoridades a una linea de producto | Neutral; I-52 e I-55 consumidores simetricos | C1: el consumidor que llegue |
| Acoplamiento a decisiones de producto | Alto por el precedente de I-53 (E1 integro tras el freeze de la linea y su ADR) salvo recorte explicito | Ninguno (SVF-R6) | C1: la foundation esperaria al producto completo del primer consumidor |
| Integracion antes de consumir | Si, con la decision del Owner | Si | C1: **no** |
| Registro de decisiones | `decisions/I-55.md` compartido | Propio | — |
| Riesgo principal | Confusion de gates y de titularidad dentro de una linea de producto | Una Discovery/Proposal/consenso mas | C1 reproduce AR4-01 |
| Veredicto | Viable con recorte y decision del Owner; **no recomendada** | **Recomendada** | C1 no cumple GF-02; C2 y lo prohibido (cherry-pick, consumo no integrado, merge parcial, excepcion implicita) **rechazados** |

**Semantica de entrega de las opciones legales.**

| Campo | A. Subiniciativa | B. Iniciativa neutral (recomendada) | C. Otros mecanismos |
|---|---|---|---|
| Claim | ID autorizado; commit vacio con Claim-Id y trailer; primer push aceptado | Igual; no se reutiliza el reclamo I-55 | C1 usa reclamo del producto y no independiza la entrega; C2 no integra; C3 solo prepara B |
| Rama y worktree | Propios durante toda su vida; nunca la rama I-55 | Propios, prefijo architecture/ y slug autorizado; ninguno creado ahora | No habilitan una rama compartida informal |
| Consenso | Coordinador y Arquitecto de la unidad neutral + Owner; alcance recortado | Coordinador y Arquitecto de la iniciativa neutral + Owner | C1 exige consenso completo del producto; C3 no autoriza codigo |
| Integracion | Serializada, unidad completa, antes de ambos consumidores | Serializada, unidad completa, antes de ambos consumidores | C1 no cumple el objetivo; sin merges parciales |
| Dependencias | No M-01, OD, ADR-0042 ni decisiones de RACKMIRROR | Solo contratos neutrales y reconciliacion EFFECTIVE; no depende de integrar I-52/I-55 | C1 reproduce el bloqueo original |

### 2.4 Recomendacion: B

- **Que decide el Owner (SVF-MECH):** autorizar la iniciativa neutral y asignar `FOUNDATION_ID` y slug; decidir las solicitudes
  escaladas y aceptar en su momento el ADR neutral. La fila de ROADMAP se actualizara en el momento permitido por WORKFLOW; la frase
  actual de I-55 sobre no depender de los otros productos no prohibe depender de una unidad neutral. No se requiere editar `main`
  directamente ni un arreglo previo adicional para publicar esta Proposal.
- **Que no se hace sin el:** ni reclamo, ni rama, ni worktree, ni fila de ROADMAP, ni numero de ADR.
- **Si el Owner elige A:** la subiniciativa adopta la especificacion como alcance **recortado** (sin ninguna parte de producto), su
  aceptacion no depende de M-01, OD ni ADR-0042, y el Owner decide expresamente su integracion antes de cerrar I-55.
- **Si el Owner rechaza ambas:** STOP de la parte compartida; V5 no propone volver al extractor unico.

### 2.5 Que hace I-55 mientras tanto

Solo trabajo documental: revision del Coordinador y del Arquitecto independiente sobre V5, decisiones del Owner (SVF-MECH, M-01, OD,
ADR-0042), registro de la reconciliacion. **Ningun gate de codigo de I-55 abre antes de que las autoridades que ese gate consume esten
`INTEGRATED`** (mapa V5 parte B). I-55 no reclama, no crea ni implementa la foundation.

**Retirada explicita.** I-55 retira su aceptacion de la tabla X-1..X-8 de G2C (`d091eeb`), que V17 §15.4 sigue tratando (las citas de linea siguientes son historicas de V16) como entrada
vinculante (`I-52-proposal-v16.md:4426-4432`, `:4486`), y renuncia a la custodia de X-2 y X-8 y a toda regla de «extractor unico» o «primer
gate». Desde V5 la unica base de I-55 para X-1..X-8 es el artefacto de reconciliacion (§2.6).

### 2.6 Reclamos, consumo y reconciliacion

- **Reclamos:** registro durable con `Authority`, `Foundation initiative/branch`, `Claim SHA`, `Owner`, `Consumers`,
  `Characterization owner`, `Integration SHA` y estado (SVF/specification.md §4; SVF/reconciliation.md §3). Una autoridad se extrae
  exactamente una vez, por su titular; **consumir = autoridad integrada en `main`**.
- **AUTH-15** (crear definicion y sobre en la transaccion del llamador, X-4): no es de la foundation; figura en el registro con I-52 como
  titular propuesto segun V17 §8.2/§15.4 (citas de linea historicas de V16) (`:4441`, G6 `:4098`). I-55 solo lo consume integrado. CR-SVF-08 propone moverlo a la foundation (§7.1).
- **Sustitucion/equivalencia:** SVF/specification.md §5.
- **Caracterizaciones:** CT-04, CT-05 y CT-16 tienen **un dueño: el titular de su autoridad** (la foundation en la propuesta;
  SVF/specification.md §3.14). I-55 no las corre.
- **Reconciliacion sin livelock** (`SVF/reconciliation.md` §1):
  - unica fuente; versiones `Rn` inmutables desde su publicacion;
  - **EFFECTIVE** = I-52 e I-55 registran el mismo SHA (y la foundation si existe). Una decision del Owner resuelve el desacuerdo, pero NO sustituye ninguno de los registros;
  - cambios por solicitud `CR-SVF-nn`; el unico disparador de relectura es un SHA nuevo del artefacto;
  - **escalada** a peticion de cualquiera de los dos Coordinadores en cuanto una version publicada no quede registrada por la otra parte,
    sin esperar a su siguiente Proposal; los Coordinadores la elevan al Owner, que decide;
  - antes de F0 solo I-55 publica versiones (I-52 aporta por `CR-SVF`); despues, la iniciativa de la foundation;
  - F0 administrativo requiere autorizacion y reclamo; F1 exige ambos registros del mismo SHA. `RESOLVED_BY_OWNER` no equivale a `EFFECTIVE`.
- **R0** se publica con V5 como PROPOSED. Es una **excepcion unica de arranque** a REC-4: V5 fija su tabla X sin que exista todavia el
  artefacto. La obligacion vigente de I-52 de releer cada Proposal nueva de I-55 (V16 `:4408`, `:4486`) sigue en pie hasta que I-52 adopte
  el protocolo.
- **Estabilidad de la cita:** R0 y la especificacion se citan por un commit de la rama de I-55; antes de cualquier rebase que reescriba ese
  commit, I-55 crea una etiqueta `archive/` que lo conserve, salvo que la foundation ya haya reexpresado el texto en su propia rama.

### 2.7 Orden de integracion

```text
Owner autoriza + FOUNDATION_ID + actualizacion de ROADMAP en su momento → F0 reclamo/bootstrap → consenso + ADR de foundation → F1..F6 → F7 → F8 en main
   → R(n+1) con Integration SHA (EFFECTIVE por registro obligatorio de ambas con el mismo SHA)
   ├ I-55 rebasa sobre main → G3 de producto (con consenso de I-55, M-01, OD, ADR-0042 aceptado) → G4..G16
   └ I-52 rebasa sobre main → sus gates de consumo G4..G6 (Proposal posterior de I-52)
```

## 3. Contratos de consumo de I-55

### 3.1 Codec: politica por consumidor

| Consumidor | Politica |
|---|---|
| `RACKEDITAR` (Actualizar e Insertar, lectura) | Sin cambio: la politica por kind de V4 §7.3 C (borrar fantasma, abortar ante `Invalid` en Push Back/Cantilever, redibujar el lado B de un sentido como A, reescritura por kind de `Canonicalizable` y de `Coerced`), ahora sobre los hechos de la foundation |
| ID17 / ID18 (escritura) | Solo direcciones `Canonical` (`Encode`) |
| ID19 | Acepta `Canonical` y `Canonicalizable`; `Coerced` e `Invalid` fallan en CLASSIFY antes de pedir puntos |

**Frontal legacy del Selectivo con `Section = −1`** (y `View` vacio con `−1`): la fila del codec la fija **una vez** la caracterizacion
CT-04 de la foundation (hecho estatico de la codificacion legacy, sin consultar ningun sistema en ejecucion): `Canonicalizable` solo si
CT-04 demuestra que la geometria que esa codificacion dibujo coincide con `Fondo(0)` (AR4-16; `git show db095c4` sugiere que se dibujo con
el sistema completo; `A/Systems/Selective/SelectiveDepthLayout.cs:57-66`); si no, `Coerced`. Con `Coerced`, ID19 **falla cerrado**; nunca se
aproxima.

### 3.2 Disponibilidad: politica de I-55

- **Hechos** de la foundation: `Available`, `VariantNotPresent`, `SystemDoesNotSupportKind`, `Unavailable` (con codigo de motivo).
- **Exposicion** (politica de I-55): la matriz de V4 §5 (`CanStartWith`, `CanAddLater`, `CanBatch`, exposicion de ID19, `VariantSelectionOwner`,
  canonica de ID19). La Cama no se expone a ID18 ni a ID19 por politica, no por disponibilidad.
- **ID19** solo continua con `Available` en la variante fuente y en la destino, y con el par expuesto; cualquier otro hecho falla en
  AVAILABLE antes de pedir puntos, con el remedio de §3.8.
- **`RACKEDITAR`:** su politica legada por kind (V4 §7.3 C) sin cambio.

### 3.3 `Resolve`: politica de I-55

- **Contrato:** SVF/specification.md §3.9. I-55 nunca resuelve fuera del puerto `IRackSystemResolver`.
- **ID19 continua solo si** `Resolved` **y** ningun `OutputBlocking` **y** la forma persistida de **todos** los miembros del grupo es
  `Current`, `LegacyAccepted` o `Canonicalizable` (se toma la peor clase de los miembros, porque el comparador authored canonico no mira la
  forma). En cualquier otro caso falla cerrado antes de pedir puntos, con el motivo y el remedio (§3.8). Sin paridad demostrada no hay
  proyeccion.
- **BOM:** sin cambio observable (politica por kind de V4 §4.5 preservada por los adaptadores de la foundation).
- **ADR-0034:** §8.1.

### 3.4 Orden de etapas de ID19 (sustituye el orden de V4 §12.1)

```text
ACQUIRE → SNAPSHOT → CLASSIFY → GROUP
  → AUTHORITY GATES   por grupo RackId, sobre los miembros (sobres interpretables, no dependientes de xref, con Id == RackId):
                        gate authored (comparador AUTH-13) y gate de propiedades (V4 §9.2)
  → REPRESENTATIVE    Selectivo: el documento authored que SelectiveAuthoredAuthority.Resolve devuelve tras IsSameAuthority
                        (documents[0] de la autoridad; entra en Resolve como authored EN MEMORIA)
                        demas kinds: el authored del primer miembro en el orden estable del nucleo de seleccion (identico en todos
                        los miembros, porque el gate paso); la forma persistida se evalua sobre TODOS los miembros (§3.3)
  → RESOLVE           UNA vez por RackId, sobre el representante (§3.3)
  → EDIT PREFLIGHT    hermanas del barrido completo, incluidas las dependientes de xref, que el preflight de RACKEDITAR rechazaria
                        → fallo con el remedio de §3.8 (para una dependiente de xref: corregirla en su dibujo de origen y recargar)
  → AVAILABLE → FRAMES → VALIDATE → PLANS → PICK → IMPORT → PLACE → MATERIALIZE   (V4 §12.1, sin cambio)
```

- **Dependientes de xref:** no entran en los gates de autoridad (no se modifican), pero si en EDIT PREFLIGHT, igual que en `RACKEDITAR`,
  porque su fallo haria fracasar el remedio.
- **Mensajes:** una etapa que falla emite un mensaje con todos los miembros afectados en el orden estable; la primera etapa que falla
  detiene la ejecucion (V4 §12.1).

### 3.5 Claves de biblioteca y requisito de bloques: politica de I-55

| Hecho (SVF §3.12) | ID19 | ID17 / ID18 |
|---|---|---|
| Rol `Required` con clave nula o en blanco | Fallo antes de pedir puntos, con la pieza | Coloca y reporta (vigente), con la misma verificacion en todo camino de insercion (G8) |
| Clave valida ausente tras importar (`Missing`) | Fallo antes de escribir ninguna definicion ni referencia de rack | Coloca y reporta |
| `LibraryAvailability = FileMissing` | Mismo fallo, con causa «biblioteca no disponible» | Reporte con esa causa |
| Rol `OptionalVisual` (`Pallet`) ausente | Aviso, no fallo (OM-30) | Reporte |

Las claves nunca se sanean; `RODILLO_DE_TUBO_DE_1.9_CALIBRE_14_LATERAL` es valida.

### 3.6 Nombres

I-55 usa la autoridad unica de nombre base (AUTH-11) para toda insercion, redibujo y renombre en POST; `UniqueBlockName` sigue en el Plugin.
I-55 no introduce ninguna semilla propia (la «semilla» de V4 §4.4 es el nombre base de la autoridad y, en un redibujo lateral, el nombre
existente que el Plugin lee y pasa como prefijo de agrupacion).

### 3.7 Clasificador compartido

Los bucles de Actualizar e Insertar de los cinco kinds y `ProjectVariableMutationExecutor` decodifican con el clasificador de la foundation
tras F3, **sin cambiar sus transacciones**. I-55 anade solo la funcion de pertenencia de Insertar (§4.1) sobre esos hechos; Actualizar y
PVME conservan su politica. Un camino que la foundation no migre queda con prueba diferencial permanente (SVF/specification.md §3.6).

### 3.8 Remedios (AR4-15, AR4-41)

Los textos exactos los fija G14 (ID19) con la convencion sin acentos del Plugin; la **seleccion** del remedio es contractual. Hechos del
codigo que la condicionan: `RACKEDITAR` busca hermanas entre las **definiciones** con sobre, sin mirar referencias
(`P/RackCommandSupport.cs:109-128`; `P/RackBlockFinder.cs:57-88`), asi que borrar solo la referencia no retira una vista; los preflights de
Push Back y Cantilever corren **despues** de la ventana y abortan perdiendo lo editado (`P/RackPushBackCommands.cs:162`, `:185-224`;
`P/RackCantileverCommands.cs:212`, `:240-266`); Actualizar e Insertar **reescriben** hoy las entradas `Coerced` en forma canonica
(`P/RackSelectivoCommands.cs:164-176`; `P/RackDinamicoCommands.cs:221-223`, `:234-247`; `P/RackCabeceraCommands.cs:268-270`).

| Situacion (kind · disposicion · disponibilidad · autoridad) | Remedio |
|---|---|
| Cualquier kind · `Canonicalizable` · `Available` | Ninguno: ID19 la acepta |
| Selectivo, Dinamico, Cabecera · `Coerced` | «RackCad la leeria como <direccion>: `RACKEDITAR` → Actualizar la redibuja y la guarda asi» (la suposicion se dice, no se oculta) |
| Cama · `Coerced` | «No se puede proyectar esta vista» (la Cama no se expone a ID19) |
| Push Back o Cantilever · `Invalid` | «`RACKEDITAR` abortara al terminar de editar este rack mientras exista esta vista: borra sus referencias y purga su definicion (`-PURGE`, Bloques, <nombre>) y vuelve a insertarla desde otra vista del rack con `RACKEDITAR` → Insertar»; si es la unica vista del rack: «no hay remedio automatico» |
| Selectivo lateral · `Invalid` (`Section < 0`) | «`RACKEDITAR` → Actualizar desde otra vista del rack la retira» (`P/RackSelectivoCommands.cs:194-198`); si es la unica vista: «no hay remedio automatico» |
| `VariantNotPresent` (fondo, poste o estacion) con otra vista valida del rack | «`RACKEDITAR` → Actualizar desde otra vista la retira como huerfana» (`P/RackSelectivoCommands.cs:163-235`; `P/RackDinamicoCommands.cs:240-272`; `P/RackPushBackCommands.cs:285-310`; `P/RackCantileverCommands.cs:304-344`) |
| `VariantNotPresent` y es la unica vista propia del rack | «`RACKEDITAR` → Insertar otra vista del rack: la huerfana se retira al colocarla» (tras G9b) |
| Push Back `PushBackCut(e, B)` en rack de un sentido | «`RACKEDITAR` redibuja esta vista del lado B como lado A; si no la necesitas, borra sus referencias y purga su definicion» |
| `SystemDoesNotSupportKind` o par no expuesto | «Esta vista no se puede proyectar» |
| Gate authored divergente o ilegible | «`RACKEDITAR` → Actualizar desde la vista cuyo diseño quieres conservar» (Push Back y Cantilever: antes, resolver las vistas que abortan su preflight) |
| Gate de propiedades | Remedio condicionado de V4 §9.2 (`RACKPROPIEDADES`, o la causa que lo deja en solo lectura) |
| `Kind` en blanco en alguna vista | «`RACKEDITAR` → Actualizar **desde otra vista del rack que tenga tipo** reescribe el tipo» (picar la vista sin tipo da «tipo no reconocido», `P/RackMenuCommands.cs:141`); sin otra vista con tipo: «no hay remedio automatico» (PLAUSIBLE, verificado en G14) |
| `Kind` mezclado con un diseño interior de otro tipo | «No hay remedio automatico»: Dinamico y Cabecera abortan la edicion (`P/RackDinamicoCommands.cs:186`; `P/RackCabeceraCommands.cs:250`) y el Selectivo sobrescribiria el bloque ajeno (OM-23) |
| `Resolved` con `OutputBlocking` (Push Back) | «Corrige el diseño en el editor Push Back: <diagnostico>» |
| `Resolved + OutputBlocking` (linea Cantilever invalida; el sistema se conserva) | «Corrige la linea en el editor Cantilever: <diagnostico>» |
| Forma legacy `Unsupported` o `Unresolved` | Dinamico solo-sistema: «`RACKEDITAR` → Actualizar reescribe el rack en la forma vigente **con los valores por defecto de los campos ausentes**» (`A/Persistence/DynamicRackSystemDocument.cs:196-210`); cabecera legacy u otras: «no se puede proyectar este rack legado» (PLAUSIBLE, verificado en G14) |
| `NotResolved(BrokenReference)` | «Repara la variable <variable> con `RACKVARIABLES`» (`P/RackVariablesCommands.cs:33`) |
| `NotResolved(DependencyUnavailable)` | «Catalogo de secciones no disponible: <motivo>» |
| `NotResolved(Unreadable)` o `UnknownKind` | «Esta version de RackCad no puede leer el rack» |
| Hermana dependiente de xref que el preflight de `RACKEDITAR` rechazaria | «Corrigela en el dibujo de origen de la referencia externa y recarga la referencia» |
| Otra hermana que el preflight de `RACKEDITAR` rechazaria | La causa de ese preflight y su remedio por fila de esta tabla |

### 3.9 `Prepare` de producto sobre la autoridad de plan

```text
Prepare(...) = Resolve (solo ID19; nunca en el editor) + hechos de disponibilidad + politica de I-55
             + Plan(sistema, direccion, contexto) de la foundation    ← marco, requisitos de bloque y nombre base incluidos
             + composicion del sobre (RackViewEnvelopeComposition, producto; unica llamada nueva a Compose; TGrd02)
             + metadatos de nombre
```

- I-55 **no** tiene planificador propio. Las unidades del redibujo atomico (G9a) y los envoltorios `PrepareRedraw`/`RedrawInTransaction`
  nuevos de Dinamico, Push Back y planta de cabecera piden el plan a la autoridad de la foundation (ya no replican la lambda de su
  `RedrawInPlace`, que tras F5b tambien delega).

## 4. Precisiones de CQ-01 (sin reabrir el redibujo atomico)

**Enmienda expresa a INV-RED-3 de V4 §13:** el adaptador `Mutate(units)` abre exactamente UNA transaccion y la confirma exactamente una
vez; las unidades preparadas y los escritores caller-owned no abren ni confirman transacciones. Esta precision sustituye la frase
incorporada «MUTATE no abre transacciones»; PREPARE → ONE MUTATE → ONE COMMIT → POST permanece fijo.


### 4.1 Funcion unica de pertenencia (AR4-17, AR4-18, AR4-23)

```text
RackSiblingMembership(hechos del barrido UNICO (§4.7), fuente elegida, RackId curado, kind) → {
  MUTABLE      = { fuente elegida }                                                        SIEMPRE (tambien con Id vacio o de espacios)
               ∪ { d : ¬IsXrefDependent(d) ∧ sobre interpretable ∧ Id(d) == RackId }         OrdinalIgnoreCase
               ∪ { d : ¬IsXrefDependent(d) ∧ Id(d) == Id de espacios de la fuente }          Selectivo y Cabecera (§4.2)
  READ_ONLY    = { d : IsXrefDependent(d) ∧ Id(d) ∈ { RackId, Id original de la fuente } }
  UNATTRIBUTED = { d : ¬IsXrefDependent(d) ∧ sobre no interpretable ∧ ProbeId(d) ∉ AttributableIds }  no bloquean
  BLOCKING     = { d : ¬IsXrefDependent(d) ∧ sobre no interpretable ∧ ProbeId(d) ∈ AttributableIds }   → SIBLING_GATE_FAILED
  AttributableIds = { RackId } ∪ { Id original de espacios de la fuente, solo Selectivo/Cabecera }; nunca el Id vacio
  ERASE        = { d ∈ MUTABLE : huerfana segun la politica legada por kind (V4 §7.3 C) }
  REDRAW       = MUTABLE \ ERASE
  SURVIVORS    = { d ∈ REDRAW : LayoutReferenceCount(d) ≥ 1 }
                 LayoutReferenceCount = referencias directas cuyo propietario es un layout (Model o Paper space), contadas en PREPARE
                 con forceValidity = true, igual que los escritores (P/Drawing/LateralHeaderDrawer.cs:167; P/RackCommandSupport.cs:248);
                 una referencia dentro de otra definicion no seleccionable no cuenta (P/RackBlockFinder.cs:85 usa forceValidity = false)
}
```

- **Una sola funcion** produce los conjuntos que usan la clasificacion, el gate de propiedades (miembros = MUTABLE) y la comprobacion de
  authored; no hay un segundo calculo de miembros.
- **Ubicacion de los borrados y de los redibujos sin supervivientes:**
  - `SURVIVORS ≠ ∅` → REDRAW y ERASE en la MUTATE (V4 §11.1).
  - `SURVIVORS = ∅ ∧ ERASE ≠ ∅` → **todas** las unidades REDRAW (sin referencias de layout) y ERASE van a la transaccion de la colocacion
    de la primera vista nueva (§4.6); la MUTATE no se abre (`REDRAW_NOT_REQUIRED`). Asi ninguna definicion del rack queda con el authored
    nuevo si esa colocacion se cancela en modo 1; el modo 2 conserva explicitamente el riesgo residual R-25 de §4.6.
  - `SURVIVORS = ∅ ∧ ERASE = ∅` → REDRAW en la MUTATE (no hay identidad que proteger).
- **Superviviente por referencias, no por bloques:** hoy la regla vigente cuenta bloques no huerfanos (`survivors = blocks − stale`,
  `P/RackSelectivoCommands.cs:229`; `P/RackDinamicoCommands.cs:268`); V5 la cambia en Insertar (IC-17).
- La fuente elegida siempre tiene al menos una referencia de layout (el usuario la selecciono en el dibujo).

### 4.2 Fuente elegida con `Id` en blanco (AR4-22)

**Hoy** (`P/RackSelectivoCommands.cs:119`, `:125`, `:132-136`, `:177`; `P/RackCabeceraCommands.cs:233`, `:238`, `:243-246`, `:270`;
`P/RackCommandSupport.cs:112`, `:124`):
- Selectivo y Cabecera usan `IsNullOrEmpty`. Con `Id = ""` se cura; la fuente se añade al redibujo solo si es frontal (Selectivo) o no
  planta (Cabecera). Con `Id` de espacios **no** se cura: el barrido la encuentra y se redibujan esa vista y sus hermanas **conservando** el
  `Id` de espacios.
- Dinamico, Push Back y Cantilever usan `IsNullOrWhiteSpace` en Actualizar e Insertar (`P/RackDinamicoCommands.cs:174`;
  `P/RackPushBackCommands.cs:179`; `P/RackCantileverCommands.cs:230`). Dinamico cura; Push Back y Cantilever abortan en su preflight de sobre
  (`P/RackPushBackCommands.cs:196-201`; `P/RackCantileverCommands.cs:245-251`).

**Con V5, en Insertar:**
- Selectivo, Dinamico y Cabecera: la fuente elegida con `Id` en blanco (vacio o de espacios) **siempre** entra en MUTABLE/REDRAW con el
  `Id` curado, **cualquiera que sea su vista**; en Selectivo y Cabecera, las hermanas con el mismo `Id` de espacios tambien se curan
  (IC-10).
- **Origen del `Id` curado:** el mismo que hoy por kind (Selectivo: `window.RackId`, que adopta el `Id` del documento interior,
  `U/Systems/Selective/RackSelectiveWindow.xaml.cs:3249`, `U/Editor/RackEditorIdentity.cs:61-64`; Dinamico: el de la ventana; Cabecera:
  `Guid.NewGuid()`). Si el `Id` curado del Selectivo coincide con el de definiciones que **no** estaban en el barrido de la fuente, V5 no
  cambia la conducta vigente (las adopta) y lo declara como riesgo R-30 (PLAUSIBLE).
- Push Back y Cantilever: abortan como hoy.

### 4.3 Predicado de blanco acotado a Insertar (AR4-24)

- **Solo Selectivo y Cabecera cambian:** en su rama Insertar, «en blanco» pasa a `IsNullOrWhiteSpace`; Actualizar conserva
  `IsNullOrEmpty`. Dinamico, Push Back y Cantilever no cambian (ya usan `IsNullOrWhiteSpace` en ambas ramas).
- **La bifurcacion sube antes del calculo del `id`.** En Selectivo, el `id` alimenta antes de `if (!UpdateOnly)` al barrido, a
  `LinkedPropertyReconciler.Reconcile` (que escribe el `id` en el authored, `A/ProjectVariables/LinkedPropertyReconciler.cs:146`) y a los
  payloads (`P/RackSelectivoCommands.cs:119`, `:125`, `:144-145`, `:249`); en Cabecera, al barrido y a `PreflightInnerSources`
  (`P/RackCabeceraCommands.cs:233`, `:238`, `:250`, `:296`). La rama Insertar calcula su propio `id`, barrido, reconciliacion y preflight
  con el `id` curado; Actualizar conserva su secuencia byte a byte.
- **Guarda `BlankIdPredicateScopeGuardTests`** (G9b): en Selectivo y Cabecera ninguna variable `id` es compartida entre ramas,
  `Reconcile` recibe el `id` de la rama que corre y Actualizar conserva `IsNullOrEmpty`; en Dinamico, Push Back y Cantilever no cambia nada.

### 4.4 La transaccion la posee `Mutate(units)` (AR4-19)

```text
ISiblingRedrawPort
  Prepare(...)                    → PreparedUnits | PrepareFailed          sin transaccion de escritura
  Mutate(IReadOnlyList<unit>)     → Committed | Discarded(unidad, motivo)  el adaptador abre la transaccion con using, escribe todas las
                                                                           unidades y confirma una vez, o sale sin Commit; ninguna
                                                                           transaccion sobrevive al retorno
  Post(Committed)                 → avisos                                 captura propia; nunca Discarded
INV-TX-1  tras Mutate (cualquier resultado) y antes de PLACE(1): Database.TransactionManager.TopTransaction == null
          (transacciones gestionadas por el TransactionManager); si no, el driver no coloca y termina con PLACEMENT_BLOCKED:
          tras REDRAW_APPLIED «Vistas existentes actualizadas. No se inserto ninguna vista: transaccion abierta inesperada»;
          sin redibujo, «No se inserto ninguna vista: transaccion abierta inesperada»
```

- `TopTransaction` no ve una `OpenCloseTransaction` ni un `LockDocument` filtrados (hay un `StartOpenCloseTransaction` en el camino de
  PREPARE, `P/Drawing/LateralHeaderDrawService.cs:279`): la **guarda de fuentes** exige `using` en toda `StartOpenCloseTransaction` y en todo
  `LockDocument` del seam, y prohibe ambos dentro de `Mutate`.
- El `LockDocument` de la operacion lo toma el adaptador con `using` alrededor de PREPARE, MUTATE y POST (V4 §11.1), fuera de `Mutate`.

### 4.5 Capas bloqueadas (AR4-20)

- **PREPARE comprueba**, para cada unidad REDRAW y ERASE:
  - la capa de **cada referencia directa** de la definicion, enumerada con `forceValidity = true` como los escritores, **incluidas** las
    referencias anidadas en otras definiciones (`RecordGraphicsModified` las abre para escritura: `P/Drawing/LateralHeaderDrawer.cs:167-170`;
    `P/Drawing/Cantilever/CantileverViewMaterializer.cs:85-88`; borrado `P/RackCommandSupport.cs:248-257`);
  - la capa de **cada entidad existente** de la definicion que se redefine o borra (`P/Drawing/LateralHeaderDrawer.cs:117`;
    `P/Drawing/Cantilever/CantileverViewMaterializer.cs:75`).
  - **No** comprueba las capas de las entidades nuevas que la redefinicion anadira (AutoCAD permite crear entidades en una capa bloqueada,
    PLAUSIBLE).
- Una capa bloqueada → `PREPARE_FAILED` que nombra la vista y la capa: nada escrito. Una capa `0` bloqueada bloquea Insertar en todos los
  racks cuyas entidades o referencias esten en ella (cambio de UX declarado; OV-RED-03).
- **Momento:** la lectura de capas ocurre en PREPARE, despues del prompt de variante (un `'LAYER` transparente durante el prompt si puede
  cambiarlas).
- **Residual PLAUSIBLE:** otras escrituras que AutoCAD rechace en capas bloqueadas y que la comprobacion no anticipe siguen revirtiendo toda
  la MUTATE (`REDRAW_ROLLED_BACK`, R-26).
- **Evidencia de rollback:** un punto de inyeccion de fallo **solo en compilacion Debug** en el adaptador (lanza en la unidad k si una
  variable de entorno de prueba lo pide), con guarda que prueba que no existe en Release; OV-RED-06 lo usa en AutoCAD.

### 4.6 R-25: la primera colocacion sin supervivientes (AR4-21)

Solo para `SURVIVORS = ∅ ∧ ERASE ≠ ∅` en la primera vista nueva. Dos modos segun AUTH-15 (crear definicion y sobre en la transaccion del
llamador, titular propuesto I-52 segun V17; §2.6):

**Modo 1 — AUTH-15 `INTEGRATED` en `main` al abrir G9b** (elimina R-25):

```text
LockDocument (using); StartTransaction (using)
  crear la definicion nueva y su sobre con AUTH-15 (nombre de la autoridad AUTH-11, el mismo que el camino normal)
  referencia nueva (Position = Origin) + jig
  OK        → redibujar las unidades REDRAW (sin referencias de layout) + borrar las ERASE + anadir la referencia a Model Space + Commit
              → POST: purga de anidadas obsoletas + Regen
  Esc/Enter → salir SIN Commit: definicion nueva, sobre, redibujos y borrados no existen; las hermanas quedan intactas
  excepcion tras OK → sin Commit → PLACEMENT_FAILED_PARTIAL_BATCH con nada escrito
```

**Modo 2 — AUTH-15 no integrado** (conducta de V4 §11.3, sin primitivo nuevo en I-55): la definicion nueva y su sobre se confirman antes del
jig con la primitiva vigente de su camino; con el jig en OK, REDRAW y ERASE van en la transaccion del jig antes de su commit; si el jig no
confirma, la definicion se retira con la limpieza best effort (D-10) y las hermanas quedan intactas. **Residual R-25:** si esa limpieza
falla, queda una definicion sin referencias con el authored nuevo.

- I-55 **no** compone un primitivo propio de crear en la transaccion del llamador (seria una segunda autoridad de X-4).
- **Familias** (modo 1): la definicion nueva de cualquier familia la crea AUTH-15 a partir del resultado de `Plan` de la foundation, que ya
  trae agrupacion, fusion de largueros y conteo de piezas para las laterales (SVF/specification.md §3.10); el informe de faltantes sale del
  resultado del primitivo.
- **PLAUSIBLE** (OV-RED-04): arrastrar una referencia a una definicion sin confirmar, con `BlockTable`, anidadas y capas abiertas para
  escritura durante el `Drag`; un comando transparente durante el jig podria chocar. Si AutoCAD no lo admite, el Coordinador decide volver al
  modo 2 (R-28).
- Las importaciones de biblioteca ya ocurrieron en PREPARE y no se revierten (OM-5).
- El resto de colocaciones (rack nuevo; supervivientes presentes; vistas 2..n) conserva V4 §11.3.

### 4.7 Un solo barrido (AR4-42)

**INV-SCAN-1.** La rama Insertar calcula PREFLIGHT, gates y clasificacion a partir de **un** resultado del barrido de la foundation
(AUTH-06), tomado **despues** de cerrar la ventana del editor; el barrido previo del Selectivo para leer el registro de variables
(`P/RackSelectivoCommands.cs:66`, `:277-297`) no se reutiliza para la pertenencia. El prompt de variante no puede borrar ni crear
definiciones; las capas se leen en PREPARE (§4.5). Guarda en G9b.

### 4.8 Estados e informe (AR4-40)

- `PREFLIGHT_FAILED`: nombre del estado cuando falla un preflight vigente (sobre, descriptor, interior I-11, catalogo de secciones,
  reconciliador); mensaje vigente.
- `PLACEMENT_BLOCKED`: INV-TX-1 (§4.4).
- El informe final lista las **huerfanas borradas** («se retiraron vistas que ya no existen en el diseño: <lista>») y las de solo lectura no
  tocadas.
- **`PromptStatus.Error` del jig no es distinguible en el llamador:** `PlaceBlockWithJig` colapsa todo lo que no es OK en `ObjectId.Null`
  (`P/Drawing/BlockPlacement.cs:184-190`, `:226-229`); un `Error` de `Editor.Drag` se informa como cancelacion
  (`PLACEMENT_CANCELLED_PARTIAL_BATCH`), y `PLACEMENT_FAILED_PARTIAL_BATCH` solo nace de una excepcion.

### 4.9 Guardas caller-owned (AR4-43)

`CallerOwnedFacadeGuardTests` (I-47 G9.1) se extiende a los siete envoltorios nuevos (Dinamico lateral, frontal y planta; Push Back lateral,
frontal y planta; planta de cabecera): no toman lock, no abren transaccion, no confirman, no regeneran, no importan y no bajan por un
envoltorio self-owned.

### 4.10 Cuenta completa de transacciones (AR4-44)

| Flujo (rack existente) | Transacciones |
|---|---|
| MUTATE | 0 (sin unidades, o caso de §4.6) o 1 |
| POST | 0 o 1 por la purga consolidada (`PurgeUnreferenced` abre la suya solo si hay candidatas, `P/Drawing/LateralHeaderDrawer.cs:193-202`) + 1 por unidad REDRAW en `RackBlockRenamer.SyncName` (escribe solo si el renombre aplica, `P/Drawing/RackBlockRenamer.cs:31-49`) |
| Importacion en PREPARE | la clonacion de biblioteca (operacion de base de datos propia del importador) |
| Primera vista con `SURVIVORS = ∅ ∧ ERASE ≠ ∅` | Modo 1: 1 (definicion, sobre, jig, redibujos, borrados) + 0 o 1 purga. Modo 2: 1 (definicion y sobre) + 1 (jig, redibujos, borrados) + limpieza si se cancela |
| Cada vista nueva restante | 2 (definicion + sobre; jig) |
| Cancelar un jig de vista nueva normal | + 1 limpieza de la definicion sin referencias + purga de anidadas (D-10) |

### 4.11 RID-5 y predicado de blanco por sitio (AR4-39)

- **RID-5 (precisado).** Cancelar antes de confirmar la primera colocacion no deja ninguna definicion ni referencia de rack **creada por
  esa ejecucion**, salvo el residual de la limpieza best effort de D-10 cuando la definicion se confirmo antes del jig (colocaciones
  normales y modo 2 de §4.6; tambien si `Drag` lanza, `P/Drawing/BlockPlacement.cs:33-52`). Las redefiniciones y borrados confirmados por un
  `REDRAW_APPLIED` previo permanecen; las definiciones de biblioteca importadas pueden quedar (OM-5).
- **Predicado de blanco:** Insertar — Selectivo y Cabecera `IsNullOrWhiteSpace` (nuevo), Dinamico, Push Back y Cantilever
  `IsNullOrWhiteSpace` (vigente); Actualizar — Selectivo y Cabecera `IsNullOrEmpty`, resto `IsNullOrWhiteSpace` (vigente); `FindRackBlocks`
  excluye solo el vacio (`P/RackCommandSupport.cs:112`, `:124`, vigente); ID19: `RackId` en blanco (`IsNullOrWhiteSpace`) falla cerrado.

## 5. ID19: geometria

### 5.1 Ida y vuelta (AR4-25)

**Enunciado corregido.** Una ida `Orthographic` y su vuelta restituyen, salvo una traslacion comun, **la coordenada de cada tramo sobre la
recta orientada `e` elegida en la ida**; nunca la coordenada descartada ni las orientaciones (en `Orthographic`, `ρ_r` depende solo de
`φ_t`, INV-GRP-2). Los **tramos** vuelven a su sitio en el mismo orden solo si todas las referencias comparten la coordenada descartada y `e`
tiene el mismo sentido que +K del marco de la familia sin girar; el **layout** completo (tambien orientaciones) vuelve solo si ademas
`φ_r = φ_s` para toda `r`. Si `e` va en sentido contrario, los tramos vuelven **reflejados a lo largo de la recta**.

Ejemplo (Rack Planta → Frontal → Planta, tramos `[0, 90]`, `B = (0, 0)`, `T = (500, 0)` en la ida y `B' = (500, 0)`, `T' = (0, 0)` en la
vuelta; C4 = `P1` sin girar con poste 0 en `(0, 0)`, `P2` y `P3` girados 180° con poste 0 en `(0, 190)` y `(0, 290)`):

| Regla | Ida: frontales (X) | Vuelta: plantas (Y del tramo) | Resultado |
|---|---|---|---|
| A (ventana relativa) | `[500, 590]`, `[600, 690]`, `[700, 790]` | `[0, 90]`, `[100, 190]`, `[200, 290]` | mismos tramos en el mismo orden; `P2` y `P3` vuelven **sin girar** |
| B (mayoria) | `[410, 500]`, `[310, 400]`, `[210, 300]` | `[−90, 0]`, `[−190, −100]`, `[−290, −200]` | tramos reflejados sobre la recta (Y ↦ −Y); racks sin girar |

### 5.2 Sentido del eje comun en `Orthographic`: A frente a B (AR4-27, AR4-45; M-01 → OD-7.e)

**Definiciones** (radianes; `ε_ang = GeometryTolerance.Angle = 1e-9`, `A/Geometry/Vector2D.cs:100`; `NormalizePi` a (−π, π] con −π ↦ +π):

```text
Recta comun: todas las e_K(r) paralelas con ε_ang (o fallo, OM-13); c = normalizar(Σ_r signo(e_K(r)·e_K(r0)) · e_K(r))
F         = familia de referencia: la unica con OD-7.d = A; con OD-7.d = B, Rack si hay algun rack, si no Cantilever
            (prioridad fija, no mayoria; la misma F fija w en OD-7.d = B)
k_F       = vector local de +K en el marco fuente de F SIN girar
g         = R(φ_s) · k_F                                         direccion de +K de un rack de F no girado en el marco fuente del grupo
A. VENTANA RELATIVA:  e = c  si NormalizePi(angulo(c) − angulo(g)) ∈ [−π/4 − ε_ang, 3π/4 − ε_ang);  si no, e = −c
                      (la ventana es semiabierta y mide π: siempre contiene exactamente uno de c y −c)
B. MAYORIA:           e ∈ {c, −c} maximiza #{r : σ_s(r) = σ_t(r)};  empate → regla A
```

Con φ_s y F dados, A no depende de las rotaciones individuales de los racks ni de cuantos haya girados: solo del angulo de la recta. B sigue
la orientacion de la mayoria. **Cambio de OD-7.d = B:** la familia de `w` pasa de «la de mas referencias» (V4 §12.5) a la prioridad fija
Rack → Cantilever, para que añadir un Cantilever no invierta la elevacion.

**Simulacion** (`Rack Planta → Frontal`, `k_F = (0, 1)`, `φ_s = φ_t = 0`, tramos `[0, 90]` salvo C5, `B = (0, 0)`, `T = (500, 0)`; salida:
intervalos de las frontales sobre X):

| Caso | Fuente (poste 0; rotacion) | A | B |
|---|---|---|---|
| C1 sin giro | `(0,0)`, `(0,100)`, `(0,200)`; 0° | `e = (0,1)`, `α = −π/2`: `[500,590]`, `[600,690]`, `[700,790]` | igual que A |
| C2 todos 180° | `(0,90)`, `(0,190)`, `(0,290)`; 180° | igual que C1 (cada rack anclado por su `K_max`) | `e = (0,−1)`, `α = +π/2`: `[410,500]`, `[310,400]`, `[210,300]` |
| C3 mezcla 2/1 | 0°, 0°, 180° (`(0,290)`) | orden natural `[500,590]`, `[600,690]`, `[700,790]` | igual que A |
| C4 mezcla 1/2 | 0°, 180° (`(0,190)`), 180° (`(0,290)`) | orden natural | invertido `[410,500]`, `[310,400]`, `[210,300]` |
| C5 longitudes 90 y 150 | `(0,0)` 0°; `(0,250)` 180° tramo `[0,150]` | `[500,590]`, `[600,750]` | igual que A |
| Limite: fila uniforme de 3 girada `θ` (postes en `R(θ)·(0,100i)`) | `θ = 134.9999999°` (relativo `3π/4 − 1.745e-9`, dentro) / `θ = 135°` (fuera) | `[500,600,700]` / `[410,310,210]`: salto en `3π/4 − ε_ang` relativos | `[500,600,700]` en ambos |
| ε en un rack, empate 1/1 | `P1` `(0,0)` 0°; `P2` `(0,190)` a `180° − 5·10⁻⁸°` (desvio 8.7e-10 < ε_ang) | `[500,590]`, `[600,690]` | igual (empate → A) |
| ε fuera de tolerancia | `P2` a `179.9999999°` (desvio 1.745e-9 > ε_ang) | fallo OM-13 (orientaciones no paralelas) | fallo OM-13 |
| Ida y vuelta | C1..C5 | mismos tramos en todos | mismos tramos en C1, C3, C5; **reflejados** en C2 y C4 |

**Nota topologica.** Ninguna regla que asigne un sentido a una recta sin usar las orientaciones de los racks puede ser continua en todo el
circulo: A pone su unica discontinuidad en un angulo fijo de la recta (`3π/4 − ε_ang` relativos a `g`). `RACKLAYOUT` copia la rotacion de su
semilla (`P/RackLayoutCommands.cs:198`, `:252`), asi que una semilla girada 135° cae justo en el limite (con resultado determinista). B es
continua en el angulo para filas uniformes pero **discontinua al cambiar la seleccion** (voltear la mayoria invierte toda la elevacion) y, en
empate, hereda el salto de A.

| Criterio | A. Ventana relativa | B. Mayoria |
|---|---|---|
| Añadir o girar un rack cambia el orden de todas las vistas | No (con φ_s y F dados) | Si (C3 → C4) |
| Orden en coordenadas universales a lo largo de la corrida | El mismo para una recta, φ_s y F dados | Depende de la mayoria |
| Ida y vuelta de layouts alineados | Mismos tramos | Tramos reflejados si la mayoria esta girada |
| Discontinuidad | Angulo fijo de la recta | Composicion de la seleccion; en empate, la de A |
| Fila girada a mano cerca de 135° | Un giro de ε cruza el limite e invierte | Continua (salvo empate) |
| Frontales «como las ve el pasillo» de racks girados | No sigue la orientacion del rack | Sigue a la mayoria |

**Recomendacion.** Preferencia inicial del Coordinador: **A**, porque la orientacion global no deberia cambiar al añadir una referencia que
voltea la mayoria. V5 la adopta como recomendacion de OD-7.e; **Owner = PENDING**. La opcion A incluye el aviso de OM-33 (recta a menos de 1°
del limite), que el Owner decide junto con la regla. INV-GRP-3 e INV-GRP-4 no cambian (dependen de σ, no de la regla); cambian la prueba de
sentido, los casos de frontera y OV-ID19-22 (mapa V5). **OM-24 reescrita** (§10.4): la razon historica contra una regla fija (hallazgo 26 de
V2) ya no se sostiene con el anclaje por tramo.

### 5.3 INV-GRP-5: geometria colocada real (AR4-26)

```text
Oraculo INDEPENDIENTE del codigo productivo (AutoCAD: universal = Position + R(Rotation)·S·(p − Origin)):
  M_r              transformacion fuente completa: Position (con Z), Rotation, ScaleFactors, Normal, Origin de la definicion fuente
  φ_r              = angulo(M_r · (1,0) − M_r · (0,0))  calculado por el oraculo (con (−1,−1,+1): Rotation + π)
  a_s(r), a_t(r)   anclas locales fuente y destino (valores de CT-05)
  A_r              = M_r · a_s(r)                                           ancla fuente real
  sourceAnchor_r   = politica(A_r)   (Rigid: A_r en XY; Orthographic: B + s_r·e)
  N_r              transformacion REAL de la referencia colocada: Position_r, Rotation = ρ_r, escala (1,1,1), Normal +Z,
                   Origin de la definicion nueva (= 0; el oraculo lo lee, no lo supone)
INV-GRP-5  (a) punto:        N_r · a_t(r) = T_common(sourceAnchor_r)                       (XY, GeometryTolerance.Length)
           (b) orientacion:  Rigid: ρ_r ≡ φ_r + α (mod 2π) con φ_r del oraculo; Orthographic: ρ_r = φ_t + δ_t(r)
           (c) segundo punto (Rigid, misma variante): N_r · (a_t + u) = T_common(M_r · (a_s + u)) para un u ≠ 0 fijo
           (d) Z:            Rigid: T.Z + (A_r.Z − B.Z); Orthographic: T.Z
```

(a) sola no detecta un `ρ` equivocado si `Position` se calcula con ese mismo `ρ`; (b) y (c) si lo detectan.

**Ejemplo `Rigid`** (`α = 0`; fuente `Position = (100, 50, 3)`, `Rotation = 30°`, `Origin = (12, −7, 0)`; `a_s = a_t = (40, 0)`;
`u = (0, 100)`; `B = (0, 0, 1)`; `T = (1000, 0, 5)`; definicion nueva con `Origin = 0`):

| Escala fuente | `A_r` | `targetAnchor` | `ρ` | `Position` nueva | `Z` | (a) `N_r·a_t` | (c) `N_r·(a_t+u)` esperado | `A_r` ignorando `Origin` (debe fallar) | (c) con `ρ` sin π (debe fallar) |
|---|---|---|---|---|---|---|---|---|---|
| `(+1, +1, +1)` | `(120.749, 70.062)` | `(1120.749, 70.062)` | 30° | `(1086.108, 50.062)` | 7 | `(1120.749, 70.062)` | `(1070.749, 156.665)` | `(134.641, 70.000)` | — |
| `(−1, −1, +1)` (giro de π) | `(79.251, 29.938)` | `(1079.251, 29.938)` | −150° | `(1113.892, 49.938)` | 7 | `(1079.251, 29.938)` | `(1129.251, −56.665)` | `(65.359, 30.000)` | `(1029.251, 116.540)` |

**Casos obligatorios** (G14): `Origin ≠ 0`; rotacion no nula; escalas `(+1,+1,+1)` y `(−1,−1,+1)`; `Position.Z ≠ 0` y `B.Z ≠ 0`; ambas
politicas; y mutaciones que deben fallar: ignorar `Origin` fuente; `Position = targetAnchor − R(ρ)·a_s` con `a_s ≠ a_t`; `ρ` sin sumar π
para `(−1,−1,+1)` (detectada por (b) y (c)); `Origin` de la definicion nueva distinto de 0 no considerado; `Z` de la fuente en
`Orthographic`.

### 5.4 Precisiones geometricas (LOW)

- **Normalizacion** (AR4-46): todo angulo con `NormalizePi` de la foundation; −π ↦ +π explicito.
- **Terminologia** (AR4-49): «reflexion» = determinante 2D negativo → «reflexion no admitida»; «escala no unitaria» → «escala distinta de 1
  no admitida»; `(+1,+1,−1)` → «Z negativa no admitida». SCP: plano XY paralelo al universal **y** Z del SCP = +Z universal.
- **Oraculos** (AR4-48): las pruebas de ID19 toman anclas y tramos de los fixtures de CT-05, nunca del descriptor productivo.
- **Superposicion** (AR4-47): ID19 usa el tramo de convencion; CT-05 registra tambien la envolvente dibujada.
- **Marco y `c`** (AR4-07, AR4-50): SVF/specification.md §3.5.

## 6. Cambios de I-55 sobre Insertar y relacion con ADR-0010

| # | Cambio | Hoy | Con I-55 | Seccion |
|---|---|---|---|---|
| IC-01 | Origen de hermana B (intencion de creacion) | Una vista adicional solo desde un rack existente (ADR-0010) | Tambien desde una intencion de creacion aceptada con `RackId`, authored, sistema y vistas preparadas (flujo de creacion, fuera del Insertar de edicion) | V4 §6, §11 |
| IC-02 | Momento del prompt de variante | Despues de redibujar las hermanas | Antes del gate y del redibujo; Esc → nada modificado | V4 §11.1 paso 1 |
| IC-03 | Gate de propiedades acotado al `RackId` | Copia sin comparar | Divergentes, ilegibles atribuibles o tipos distintos fallan con remedio | V4 §9.2; §4.1 |
| IC-04 | Autoridad authored | Unificacion del editor; redibujo vista a vista que puede confirmar un subconjunto | Unificacion vigente + redibujo atomico: todas las MUTABLE quedan con el authored nuevo o ninguna; sin gate authored nuevo en el editor | V4 §9.3; §4.1 |
| IC-05 | Redibujo atomico de hermanas | Una transaccion por vista, fallos ignorados | PREPARE → una MUTATE → un commit → POST | V4 §11.1 |
| IC-06 | Huerfanas | Borradas en transacciones propias; conservadas si no quedan bloques no huerfanos | En la MUTATE si hay supervivientes con referencias de layout; si no, junto con los redibujos sin referencias en la transaccion de la primera colocacion (§4.6) | §4.1, §4.6 |
| IC-07 | Hermanas dependientes de xref | Se redibujan si el barrido las alcanza (PLAUSIBLE: conservan el sobre en la tabla de bloques; OV-RED-08) | Solo lectura: no se tocan; el informe las nombra | V4 §9.2; §4.1 |
| IC-08 | Colocacion tras el redibujo | Definicion y jig tras el redibujo por vista | Solo tras `REDRAW_APPLIED` o `REDRAW_NOT_REQUIRED`, con `TopTransaction == null` (si no, `PLACEMENT_BLOCKED`) | V4 §11.3; §4.4 |
| IC-09 | Cancelacion | Esc en el jig deja el redibujo parcial | Esc en el prompt: nada; Esc en un jig: redibujo y vistas previas conservados; primer jig sin supervivientes: nada en el modo 1 de §4.6, limpieza best effort en el modo 2 | V4 §11.4; §4.6 |
| IC-10 | Fuente elegida con `Id` en blanco (Selectivo, Dinamico, Cabecera) | `""`: se cura; la fuente se redibuja solo si es frontal (Selectivo) o no planta (Cabecera). Espacios (Selectivo, Cabecera): no se cura; la vista y sus hermanas se redibujan conservando los espacios | Siempre curada y redibujada, cualquiera que sea la vista; en Selectivo y Cabecera, las hermanas con el mismo `Id` de espacios tambien se curan | §4.2 |
| IC-11 | Predicado de blanco en Selectivo y Cabecera | `IsNullOrEmpty` en ambas ramas | `IsNullOrWhiteSpace` solo en Insertar; Dinamico, Push Back y Cantilever sin cambio | §4.3 |
| IC-12 | Capas bloqueadas | La vista se salta en silencio | `PREPARE_FAILED` con la vista y la capa | §4.5 |
| IC-13 | Bloques faltantes | Algunos caminos no informan | Todo camino informa con el requisito estructural | §3.5; mapa G8 |
| IC-14 | Excepcion tras crear la definicion | La definicion queda | Limpieza best effort de la definicion sin referencias (D-10) | V4 §14 |
| IC-15 | `Regen` | Uno por vista redibujada | Uno en POST; Cantilever uno al final de la cola | V4 §11.4 |
| IC-16 | Varias vistas en un gesto (ID18) | Una vista | Lote con cola; mismo redibujo atomico | V4 §11 |
| IC-17 | Definicion de superviviente | Bloques no huerfanos (`survivors = blocks − stale`) | REDRAW con al menos una referencia de layout | §4.1 |
| IC-18 | Purga tras el redibujo | Purga por vista | Consolidada en POST, excluyendo las anidadas que piden las vistas nuevas preparadas | V4 §11.1 paso 6 |
| IC-19 | Transaccion abierta inesperada | — | `PLACEMENT_BLOCKED` | §4.4 |

**Actualizar no cambia** (salvo la migracion sin cambio observable de su decodificacion al clasificador, que hace la foundation). **ADR-0042
complementa ADR-0010** y no lo reemplaza; ADR-0010 sigue `aceptado`; la nota posterior fechada solo se escribe si el Owner acepta ADR-0042
(V4 §22.1 D-12, incorporada).

## 7. Reconciliacion con iniciativas activas

### 7.1 I-52 (`53ae5fa`, Proposal V16)

**Referencia vigente tras fetch previo a publicacion:** I-52 Proposal V17, commit
`b7a6d9fe897dae4d29ae29ada7f60127bca365e5`, ruta `docs/initiatives/I-52-proposal-v17.md`.
La comparacion exacta de las ocho filas X confirma igualdad textual con V16: X-1..X-8 estan ahora en lineas 4084..4091;
CT-04/05/16 siguen en §12 (lineas 3677, 3678 y 3689) y la reconciliacion en §14.1, §15.2..15.4, §19 y §21. V17 no adopta la foundation ni los reclamos.
Mantiene custodia y primer gate; por tanto siguen abiertos X-2/X-8 y las precisiones de ownership de X-1/X-3/X-4/X-7.
Su §0 cambia el censo cerrado de invocaciones API y la politica de overrules/modos/host: no cambia los contratos neutrales,
pero F2..F6 y G9a/G9b deben declarar sus productores nuevos en G-M24 y respetar la reconciliacion de §15.3.
El censo y sus autoridades concretas quedan pendientes de CT-49/50 en I-52; no se afirman ejecutados ni los adopta I-55 como hechos.
Coordinator/Architect REVIEW REQUIRED; Owner O-1 PENDING; consenso NO; G3 NOT OPEN; freeze BLOCKED por autoridad final I-49.
La retirada de aceptacion de d091eeb de §2.5 aplica tambien a su §15.4. Se requiere una Proposal posterior de I-52 para adoptar
CR-SVF-01..08 y registrar el mismo SHA. Las citas V16 de abajo son evidencia historica comprobada, no la referencia vigente.

**Lectura.** La orden G2F fijaba V15 (`ae8640a`); durante la redaccion de V5, I-52 publico **V16** (`53ae5fa`), que reconcilia la revision de
su Arquitecto sobre V15 (sesiones de edicion, mutadores internos y overrules en G-M24(B), campos mixtos, XREF, I-56). V5 se reconcilia con
**V16** en el borrador heredado; el pase de takeover anterior actualiza a V17. Su tabla X-1..X-8 es identica a la de V15 (`I-52-proposal-v16.md:4438-4445` = V15 `:4342-4349`) y las citas de V15 que usaba el
borrador se trasladaron a V16.

**Estado historico de V16:** Coordinator REVIEW REQUIRED; Architect REVIEW REQUIRED; implementacion bloqueada; O-1 PENDING; G3 NOT OPEN. Registra I-55
en `8068368` (Proposal V4) sin adoptar nada; coordinacion con I-55 «NON-MATERIAL … coordinacion **HIGH / ACTIVE**» (`:4261`, `:4349`);
«I-52 no espera al consenso de I-55» (`:4837`); todas las autoridades compartidas y CT-04, CT-05, CT-16 **PROVISIONAL UNTIL
CROSS-INITIATIVE RECONCILIATION** (p. ej. `:897-904`, `:1130-1135`, `:3811-3823`, `:4030-4048`, `:4899-4903`), sin fijar titularidad ni
ubicacion; mantiene la tabla de G2C (`d091eeb`) como entrada vinculante (`:4426-4432`, `:4486`) y la obligacion de releer cuando I-55
publique otra Proposal (`:4408`, `:4486-4487`). **No contempla ningun mecanismo de foundation.**

**Clasificacion** (vinculante para V5 por la orden G2F), contrastada con V17; la columna siguiente conserva evidencia historica de V16 identificada. Las **clausulas de extraccion** de V16 que contradicen el
mecanismo quedan registradas como conflicto aunque el contenido sea compatible:

| X | Clasificacion | Evidencia en V16 | Conflicto registrado de la clausula de extraccion |
|---|---|---|---|
| X-1 Seleccion | **COMPATIBLE** | Contenido igual (§5.1 `:1137-1142`) | «custodio I-52; extrae I-52 G4 (I-55 G13 solo si I-52 no lo extrajo)» (`:4438`; G4 `:4096`); CT-16 «primer G3 que llegue» (`:3823`) → CR-SVF-02, CR-SVF-04 |
| X-2 Taxonomia, codec, disponibilidad, `Resolve`, `Plan` | **MATERIAL CONFLICT DE PROCESO** | «extrae la primera que llegue (I-55 G6/G7 o I-52 G5/G6)» (`:4439`); CT-04 «primer G3 que llegue» (`:3811`); §14.1 sin paso «foundation integrada» (`:4023-4084`); `Build` = `Resolve` + `Plan` sin adoptar (`:4452`) | idem |
| X-3 Comparador authored | **COMPATIBLE** | Contenido compatible (§5.4 `:1186-1201`) | «extrae I-52 G5 o I-55 G14» (`:4440`) → CR-SVF-02 |
| X-4 Requisito estructural y primitivo | **COMPATIBLE CON PRECISIONES** | La foundation solo toma la parte pura y la consulta (AUTH-12); **crear** en la transaccion del llamador es AUTH-15, con I-52 titular (§8.2 `:2163-2194`; G6 `:4098`) e I-55 solo consumidor integrado (§4.6; mapa G15). Precisiones: `EnsureForPlan` (§9.5) con la consulta (CR-SVF-06); cambios preregistrados frente a §19 (`:4870-4890`); nombre de la definicion desde AUTH-11 (el camino Cantilever vigente usa `CreateBlockDefinition`, `P/RackCantileverCommands.cs:151-152`) | «extrae I-52 G6 (I-55 G15 solo si llega antes, con la firma acordada)» (`:4441`) → CR-SVF-08 |
| X-5 Protocolo | **COMPATIBLE** | §15.3 ya cubre «cualquier iniciativa nueva» (`:4356`) | — |
| X-6 Lineas base de C-2 | **COMPATIBLE** | «la segunda en integrar re-establece C-2» (`:2256`, `:4443`) se mantiene entre I-52 e I-55; la linea base es `main` con la foundation | — |
| X-7 Valor de colocacion | **COMPATIBLE CON PRECISIONES** | La tolerancia de escala **no existe** hoy: «G4 fija el valor, absoluto y no relativo» (`:1122`; `:176`), no marcada provisional; algebra y comprobaciones de I-52 (`:821-822`). Precision: el valor se registra en el artefacto **antes de F1** (CR-SVF-01) | «extrae I-52 G4 (I-55 G14 solo si llega antes)» (`:4444`) → CR-SVF-02 |
| X-8 Marco y tramo | **MATERIAL CONFLICT DE PROCESO** | «extrae la primera» (`:4445`); CT-05 «primer G3 que llegue» (`:3812`). Contenido compatible con precisiones: `c` sin forma afin (`:3581-3583`), δ registrado y nunca ajustado (`:4445`, CT-06 `:3813`), sin centros derivados sin verificar (`:4922`) | idem |

**Ciclo de la tolerancia de escala (declarado).** V16 fija el valor en su G4; con el mecanismo, I-52 G4 va despues de integrar la
foundation, y la foundation F4 necesita el valor: cada una esperaria a la otra. Salida: CR-SVF-01 exige registrar el valor en el artefacto
**antes de F1** (propuesto por I-52 o decidido por el Owner en la escalada); I-52 G4 lo consume.

**Contenido nuevo de V16 frente a la foundation:** la familia G-M24(B) (mutadores internos de AutoCAD, overrules y suscripciones a eventos,
`:2419-2427`, `:3938`, `:3967`) y la regla de sesiones y host (E12) son **compatibles si se adoptan**: cualquier sitio de la foundation que
llame esos mutadores o se suscriba a eventos debe declararse; hoy `src/` en `dad4e77` no tiene ninguno. Nada nuevo contradice el mecanismo.

**Lo que I-52 necesita para adoptar el mecanismo (su Proposal posterior a V17; secciones homologas, numeros de linea siguientes de V16):**
1. Orden de integracion: añadir a §14.1, §14.2 G3/G4 (`:4095-4096`), §19 (`:4893-4905`) y §21 el paso «foundation integrada en `main` →
   rebase → registrar `Rn`» (CR-SVF-03).
2. X-1: §5.1 pasa de extraer a consumir (o la titularidad decidida en CR-SVF-02); CT-16 del titular; la coordinacion con I-49 pasa al
   titular.
3. X-2: §3.7 (`:895-918`), §3.8, §8.1 (`Build` = `Resolve` + `Plan`), §8.4 y G5/G6 remiten a la foundation; CT-04 de la foundation.
4. X-8: `c` = `Center` del marco; §6.1 `DescribeView` y §8.1 `ViewPlanResult` sobre `RackViewFrame`; CT-05 de la foundation; CT-06 y la
   regla sin forma afin quedan en I-52 (CR-SVF-05).
5. X-3: §5.4 y `IsSameAuthority` del reflector delegan en el comparador del titular.
6. X-7: registrar el valor de tolerancia antes de F1 (CR-SVF-01); `ReflectionAboutLine` queda en I-52.
7. X-4: §8.2 conserva `CreateInTransaction` como AUTH-15 salvo CR-SVF-08; §9.5 `EnsureForPlan` con `LibraryBlockQuery` (CR-SVF-06).
8. §15.2/§15.3: la foundation como iniciativa rastreada; §19: sus cambios preregistrados no son STOP.
9. §15.4, la tabla de `d091eeb` y ADR-0036 decision 11: sustituir «custodio/extrae/primero» por el registro de reclamos y registrar el SHA
   de `Rn`; retirar la obligacion de releer cada Proposal de I-55 en favor de REC-5.
10. C-2 (§8.5): linea base = `main` con la foundation.

**Regla de conflicto.** I-55 **no** resuelve estos conflictos por su cuenta: quedan registrados como MATERIAL CONFLICT DE PROCESO (X-2, X-8),
como conflictos de clausula de extraccion (X-1, X-3, X-4, X-7) y como solicitudes CR-SVF-01..CR-SVF-08. Si I-52 rechaza el mecanismo o
reclama otra titularidad, escalada a los dos Coordinadores y al Owner (REC-6), que decide. **Productores nuevos** de I-55 (sitios del
redibujo atomico de G9a y G9b, primera colocacion de §4.6) se declaran en G-M24 y en el reporte X-5 (AR4-37).

### 7.2 I-49 (`addb5223265612785dfe19ef1f353a19307f0a83`)

A3-R3 registra consenso tecnico sobre A3-R2 (`5210c1c0534d3cf5bee4d23526f24e062bdc77c1`, blob `4da6ef3caa21dcf31140983c6db7e23f02aa3e18`).
Owner PENDING; ADR-0043-P1 acaba de publicarse propuesto como sucesor de ADR-0041, con revisiones exactas pendientes; freeze nuevo pendiente; G6 cerrado y G7/G8 bloqueados. Sin archivos de produccion
comunes con I-55 ni con la foundation, con ADR-0043 publicado (no disponible para la foundation). La coordinacion antes de editar `A/Persistence/RackDuplicationPlan.cs`
pasa al titular de AUTH-07. Archivo caliente `U/Systems/Selective/RackSelectiveWindow.xaml.cs` (I-49 G10): serializar con F2 de la
foundation y con G10/G12 de I-55. `Resolve` consumira las expresiones de I-49 cuando integre su autoridad final.

### 7.3 I-56 (`f97a8b0`)

Avanzo con la revision de Arquitecto de su Proposal V2 (`f97a8b0`): CHANGES REQUIRED → Proposal V3; **Workflow V2 NOT EFFECTIVE**; I-49,
I-52 e I-55 grandfathered. Sus puntos propuestos (una fundacion consumida se verifica en codigo; el registro de foundations es un puntero sin
autoridad; su poblacion inicial exige al Owner) son coherentes con el mecanismo B. Su AR-03 propone STOP para «una unidad nueva de
I-49/I-52/I-55» hasta decidir OWN-E: la foundation neutral no es una unidad de esas lineas, pero si Workflow V2 llegara a estar vigente
antes del reclamo, el Owner deberia confirmarlo (OWN-E).

### 7.4 I-53D (en `dad4e77`)

Sin cambio (V4 §17.3).

### 7.5 PR-1 y PR-2 (D-09 enmendada)

PR-1 (poste real de Push Back) y PR-2 (visibilidad de la planta Cantilever, solo Plugin) son **correcciones de producto**: la foundation
caracteriza y conserva el comportamiento vigente (SVF-R4), y su contrato de plan ya lleva `PlantaVisibility` (nulo = vigente). Por eso PR-1 y
PR-2 van **despues** de integrar la foundation, como G4 y G5 del mapa V5, **antes** de G6 y de los gates que consumen sus resultados.

## 8. ADR

### 8.1 ADR-0034: sin modificacion

- **§10 (BOM):** `RACKBOMTOTAL` sigue resolviendo una vez por rack **dentro del handler Selectivo** (`BuildBom` llama a `ResolveSystem`,
  que contiene la unica construccion de `SelectiveEffectiveDesignResolver`), con el mismo snapshot de variables y abortando ante una
  referencia rota (`docs/adr/0034-project-variables-autoridad-drawing-level.md:126-131`).
- **§6:** la clase de resolucion de Application sigue siendo el unico punto que convierte vinculos en numeros.
- **Consumidores nuevos** (ID19, I-52) llegan a **esa misma autoridad** por el puerto; no se crea otro sitio de resolucion.
- **Guardas:** `T/SelectiveBomAuthorityTests.cs:403-408` (una construccion) y el resto de la tabla de SVF/specification.md §3.9.3 siguen
  verdes sin cambio; las cadenas exactas de `T/PushBackRoundTripSourceGuardTests.cs:90-127` se conservan o se reapuntan con motivo.
- **Posibilidad tecnica verificada** (SVF/specification.md §3.9.3): no hace falta proponer ningun cambio a ADR-0034.

### 8.2 ADR-0042: ADR de producto

Propuesto; decisiones de producto de ID17, ID18, ID19, Insertar, intencion de creacion, redibujo de hermanas y relacion con ADR-0010.
**Retirados:** la decision 9 de V4 (autoridades entre iniciativas), toda mencion de que iniciativa o gate extrae, titularidad temporal de
X-1..X-8, namespaces o ramas. Remite a la VIEW FOUNDATION ADR (numero pendiente) para los hechos que consume. Enumera los cambios de
Insertar (§6). Precondiciones de aceptacion: consenso de I-55, M-01 (con OD-7.e), OD-1..OD-8 y la VIEW FOUNDATION ADR aceptada.

### 8.3 VIEW FOUNDATION ADR

Borrador neutral en `SVF/adr-draft.md`: **propuesto, numero PENDIENTE** hasta censar numeros y resolver iniciativa y reclamo; aceptable sin
M-01, OD, ADR-0042 ni decisiones del Owner de `RACKMIRROR`; no figura en el indice de ADR.

### 8.4 ADR-0036 (rama de I-52)

Se propone a I-52 (en su Proposal posterior a V17; secciones homologas, numeros de linea siguientes de V16) que su decision 11 referencie la VIEW FOUNDATION ADR en lugar de declarar esas autoridades. I-55 no escribe en la
rama de I-52.

## 9. Gates de producto (resumen; mapa V5 parte B)

| Gate | Objetivo | Precondicion |
|---|---|---|
| — | Entrega de la foundation F0..F8 | Owner (SVF-MECH); mapa V5 parte A |
| G3 | Caracterizacion **de producto** (sin CT-04/05/16 ni las de la foundation) | Coordinator y Architect AGREED sobre la misma version; M-01 resuelta; OD-1..OD-8; ADR-0042 aceptado; Consensus Freeze; **AUTH-01..AUTH-14 `INTEGRATED`** y rebase; `Rn` EFFECTIVE (registro obligatorio de ambas con el mismo SHA) |
| G4 | PR-1 | G3 |
| G5 | PR-2 (solo Plugin, argumento `PlantaVisibility`) | G3 |
| G6 | Consumo: politicas de I-55 sobre codec y disponibilidad (sin extraccion) | G4, G5 |
| G7 | `Prepare` de producto: composicion del sobre, politica, ciclo de `RackId` | G6 |
| G8 | Colocacion de vista unica, D-10, prompt, reporte estructural en todo camino | G7 |
| G9a | Seam atomico (con §4.1, §4.4, §4.5), sin cablear | G7 (y G5) |
| G9b | Insertar: variante, gate, pertenencia unica, PREPARE con capas, redibujo atomico, primera colocacion sin supervivientes (§4.6: modo 1 si AUTH-15 esta `INTEGRATED`, si no modo 2), informe | G8, G9a |
| G10 | ID17 | G9b |
| G11 | ID18 contrato puro | G10 |
| G12 | ID18 UI y driver | G11 |
| G14 | ID19 puro (orden §3.4, remedios §3.8, OD-7.e, INV-GRP-5) | G12; M-01; **AUTH-05, AUTH-07, AUTH-08 y AUTH-13 `INTEGRATED`** (por la foundation o por el titular que decida CR-SVF-02) |
| G15 | ID19 comando | G14; OD-1; OD-8; **AUTH-15 `INTEGRATED`** (por I-52, o por la foundation si CR-SVF-08 lo decide) |
| G16 | Candidato e integracion | todo |

G7a y G13 de V4 **se retiran** (su contenido es AUTH-09 y AUTH-07). AUTH-07/08/13 y CT-16 se entregan por la foundation independiente.
Un desacuerdo de titularidad bloquea F1; no habilita esperar su extraccion en el producto I-52. Solo AUTH-15, fuera del alcance neutral
actual, deja G15 dependiente de la integracion de I-52 salvo decision explicita de CR-SVF-08 (R-31).

## 10. Registro de decisiones

### 10.1 Decisiones tecnicas

| # | Decision | Estado en V5 | Seccion |
|---|---|---|---|
| D-01 | ALT-B | Vigente | V4 §4 |
| D-02 | `RackViewKind`; `CantileverViewKind` de camara | **Foundation (AUTH-01)**; I-55 la consume | SVF §3.1 |
| D-03 | Variante tipada; codec, disponibilidad y politica separados | **Foundation (AUTH-02..04)** + politica de I-55 (§3.1, §3.2) | §3 |
| D-04 | Matriz de exposicion | Vigente como politica de I-55 | V4 §5 |
| D-05 | `RackId` al aceptar la intencion; blanco segun §4.3 | Vigente con §4.2, §4.3, §4.11 | §4 |
| D-06 | ID18: PREPARE ALL + redibujo atomico + commit por colocacion | Vigente | V4 §11 |
| D-07 | Gates acotados al `RackId` | Vigente con §4.1 y §3.4 | §3.4, §4.1 |
| D-08 | Seleccion, grupos, clasificacion | Vigente; D-08a = AUTH-07 | V4 §12.8 |
| D-09 | PR-1 y PR-2 | **Enmendada:** tras integrar la foundation (G4, G5), antes de G6 | §7.5 |
| D-10 | Limpieza ante excepcion | Vigente | V4 §22.1 |
| D-11 | Presentacion de creacion | Vigente | OD-8 |
| D-12 | ADR-0042 complementa ADR-0010 | Vigente; ADR-0042 pasa a ser solo de producto | §8.2 |
| D-13 | Requisito estructural por pieza | **Foundation (AUTH-12)** + politica de I-55 | §3.5 |
| D-14 | Una autoridad por responsabilidad con I-52 | **Sustituida por D-20** | §2 |
| D-15 | Redibujo atomico de hermanas (CQ-01) | Vigente con precisiones §4 | V4 §11.1; §4 |
| D-16 | Group Placement y `CommonTransform2D` | Vigente | V4 §12.2 |
| D-17 | `Resolve` compartido | **Foundation (AUTH-09)**; politica de ID19 §3.3 | §3.3 |
| D-18 | `Rigid`/`Orthographic`; FRAMES; tramos | Vigente; el sentido del eje comun pasa a OD-7.e; familia de `w` en OD-7.d = B por prioridad fija | §5.2 |
| D-19 | Contrato `Resolve`/`Plan`/`Prepare` con semilla | **Sustituida por AUTH-10/AUTH-11** + `Prepare` de producto (§3.9) | §3.9 |
| **D-20** | **Shared View Foundation** neutral integrada antes de consumir; mecanismo B recomendado; `FOUNDATION_ID` del Owner; registro de reclamos; consumir = integrado; retirada de la tabla de `d091eeb` | Propuesta | §2 |
| **D-21** | Artefacto de reconciliacion versionado por SHA; EFFECTIVE por registro obligatorio de ambas con el mismo SHA; escalada sin esperar a Proposals | Propuesta | §2.6 |
| **D-22** | Funcion unica de pertenencia; supervivientes con referencias de layout; sin supervivientes, redibujos sin referencias y borrados en la primera colocacion | Propuesta | §4.1 |
| **D-23** | Fuente con `Id` en blanco siempre curada; predicado acotado a la rama Insertar de Selectivo y Cabecera, con la bifurcacion antes del `id` | Propuesta | §4.2, §4.3 |
| **D-24** | `Mutate(units)` duena de la transaccion; INV-TX-1 con `PLACEMENT_BLOCKED`; capas bloqueadas en PREPARE; inyeccion de fallo solo Debug | Propuesta | §4.4, §4.5 |
| **D-25** | Primera colocacion sin supervivientes: modo 1 con AUTH-15 integrado (sin R-25) o modo 2 (V4, con R-25) | Propuesta | §4.6 |
| **D-26** | Orden de ID19: gates → representante → `Resolve`; forma legacy sobre todos los miembros; remedios por situacion | Propuesta | §3.3, §3.4, §3.8 |
| **D-27** | INV-GRP-5 sobre la colocacion real con oraculo independiente (punto, orientacion, segundo punto, Z) | Propuesta | §5.3 |

**Invariantes nuevos.** INV-GRP-5 (§5.3); INV-SCAN-1 (§4.7); INV-TX-1 (§4.4). **INV-AUTH-1** pierde su residual R-25 solo en el modo 1 de §4.6.

**Alternativas nuevas.**

| Nivel | Opcion | Veredicto |
|---|---|---|
| Autoridades compartidas | Extractor unico I-55 (V3/V4) | **Rechazada** (AR4-01) |
| Autoridades compartidas | «Primer gate» entre ramas | **Rechazada** (AR4-02) |
| Mecanismo | A subiniciativa de I-55 | Viable con recorte y decision del Owner; no recomendada |
| Mecanismo | C1 primer consumidor construye | **Rechazada**: no integra antes |
| Resolucion | Mover el resolver Selectivo a Application (V4 G7a) | **Rechazada**: reabriria ADR-0034 sin necesidad |
| Plan | Planificador de producto junto a lambdas | **Rechazada** (AR4-06) |
| R-25 | Componer en I-55 un crear en la transaccion del llamador con las primitivas vigentes | **Rechazada**: segunda autoridad de X-4 |
| Reconciliacion | Vigencia solo por registro de ambas, sin decisor | **Rechazada**: rehen de la parte que no registra |
| Sentido del eje comun | B mayoria (V3/V4) | Alternativa de M-01 (OD-7.e B) |

### 10.2 Decisiones del Owner

**Owner = PENDING.** Ninguna decision del Owner registrada.

| # | Decision | Opcion A | Opcion B | Recomendada |
|---|---|---|---|---|
| **SVF-MECH** | Mecanismo de la foundation, ID y slug; actualizacion de ROADMAP en su momento en ROADMAP | Subiniciativa de I-55 (con recorte y decision de integrar antes) | **Iniciativa neutral con ID del Owner** | **B** |
| **SVF-ESC** | Decisiones sobre las escaladas del artefacto (CR-SVF-01, CR-SVF-02, CR-SVF-08 si no hay acuerdo) | — | — | Segun cada CR |
| OD-1 | Nombre del comando | `RACKPROYECTAR` + `RPY` | `RACKPROYECTARVISTAS` | A |
| OD-2 | Variante en ID19 (clase distinta) | canonica fija | preguntar | A |
| OD-3 | Miembro no soportado | fallar todo | omitir con aviso | A |
| OD-4 | Esc/Enter a mitad de lote | detener | saltar | A |
| OD-5 | Orden del lote | F → L → P | orden de marcado | A |
| OD-6 (M-01) | 6.b, 6.c, 6.d | V4 §12.7 | V4 §12.7 | A, A, A |
| OD-7 (M-01) | 7.a, 7.b, 7.c, 7.d | V4 §12.7 (7.d B con familia por prioridad fija, §5.2) | V4 §12.7 | A, A, A, A |
| **OD-7.e (M-01)** | Sentido del eje comun en `Orthographic` | **ventana relativa al marco fuente** (§5.2), con el aviso de OM-33 | mayoria de referencias | **A** (preferencia inicial del Coordinador; ya no automatica) |
| OD-8 | Presentacion de vistas proyectadas | creacion vigente | de la fuente | A |

### 10.3 Open Material

| # | Tema | Estado |
|---|---|---|
| SVF-MECH | Mecanismo, ID y actualizacion de ROADMAP en su momento | Owner PENDING |
| SVF-REC | `R0` → `Rn` EFFECTIVE (registro obligatorio de I-52 e I-55 con el mismo SHA) | Requiere la Proposal posterior de I-52 o la escalada (§7.1) |
| X-2, X-8 | MATERIAL CONFLICT DE PROCESO | Hasta acordar mecanismo y orden de integracion |
| Clausulas de extraccion de X-1, X-3, X-4, X-7 | Conflicto de clausula registrado (CR-SVF-02, CR-SVF-08) | Coordinadores; Owner si no hay acuerdo |
| M-01 | Semantica de Group Placement (OD-6.b/c/d, OD-7.a..e, OD-2.b) | Owner PENDING; sin `Coordinator = AGREED` ni G3 mientras siga abierta |

CQ-01: cerrada (G2E), precisada sin reabrir (§4).

### 10.4 Open Minor

V4 §22.4 incorporado, con estos cambios:

| # | Tema |
|---|---|
| OM-5 | Sin cambio; en el modo 1 de §4.6 el abort no deja definicion de rack, solo biblioteca importada |
| OM-22 | El censo de omisiones antes del plan lo hace la foundation (CT-BLK); la decision por rol sigue siendo de I-55 |
| OM-23 | Sin remedio automatico para un `Kind` mezclado con diseño interior de otro tipo (§3.8) |
| **OM-24 (reescrita)** | El sentido del eje comun de `Orthographic` es OD-7.e: A (ventana relativa) evita invertir la elevacion al cambiar la seleccion y tiene un salto en un angulo fijo de la recta; B (mayoria) es continua en filas uniformes pero invierte la elevacion al voltear la mayoria (§5.2) |
| OM-25 | Sin cambio; el conjunto de solo lectura es explicito en la funcion de pertenencia (§4.1) |
| OM-26, OM-29 | Sin cambio |
| **OM-30** | Rol `Pallet` ausente en ID19 = aviso, no fallo (§3.5), sujeto a CT-BLK |
| **OM-31** | Remedios PLAUSIBLES (`Kind` en blanco desde otra vista; forma legacy reescrita por Actualizar) se verifican en G14; si no se confirman, «sin remedio automatico» |
| **OM-32** | La inyeccion de fallo solo Debug del adaptador (§4.5) es codigo de prueba en un binario de producto: su ausencia en Release la fija una guarda |
| **OM-33** | Parte de la opcion A de OD-7.e: el mensaje previo a PICK avisa si la recta cae a menos de 1° del limite |
| **OM-34** | Una capa `0` bloqueada impide Insertar en los racks que la usan (§4.5) |

### 10.5 Riesgos nuevos o cambiados

| # | Riesgo | Mitigacion | Gate |
|---|---|---|---|
| R-03 | I-52 no adopta el mecanismo o reclama otra titularidad | Conflictos registrados; CR-SVF; escalada al Owner (REC-6) | antes de G3 |
| R-16 | La delegacion en `Resolve` cambia un BOM | **Riesgo de la foundation** (SVF-RK2) | F5a |
| R-20 | Acoplamiento de X-2 retrasa gates de I-52 | **Retirado**: ambas esperan a `main` | — |
| R-21 | Sentido del eje comun sorprende al usuario | OD-7.e; OM-24, OM-33; OV-ID19-22 | G14, G15 |
| R-25 | Definicion sin referencias con authored nuevo tras cancelar la primera colocacion | **Eliminado** en el modo 1 de §4.6; residual en el modo 2 | G9b |
| R-26 (PLAUSIBLE) | Capas bloqueadas | Comprobacion en PREPARE; residual en MUTATE | G9b |
| **R-27** | La foundation tarda en existir y bloquea todos los gates de codigo de I-55 | Consecuencia aceptada de GF-02; I-55 avanza solo en documentos | — |
| **R-28** | AutoCAD no admite el jig sobre una definicion sin confirmar | Vuelta al modo 2, decidida por el Coordinador | G9b |
| **R-29** | Una politica de I-55 depende de un hecho que la foundation no expone | Solicitud CR-SVF antes de F7, o unidad posterior | antes de G3 |
| **R-30 (PLAUSIBLE)** | El `Id` curado del Selectivo (del documento interior) coincide con el de otro rack presente y la cura adopta sus vistas | Conducta vigente conservada y declarada; caso en `RackSiblingMembershipTests` y `InsertBlankIdFlowCharacterizationTests` | G3, G9b |
| **R-31** | AUTH-15 fuera de la foundation deja G15 esperando su integracion por I-52; AUTH-07/08/13 no dependen del producto I-52 | Decision del Owner en CR-SVF-02 y CR-SVF-08 con esta consecuencia a la vista | G14, G15 |

### 10.6 Hallazgos fuera de I-55

H-13, H-14, H-15 y H-16 de V4 §22.5, incorporados sin cambio (H-15 tambien caracterizado por CT-NAME en la foundation).

## 11. Revision adversarial de V5 (antes del commit)

**Metodo y transparencia.** Dos revisores de solo lectura de la sesion de takeover contrastaron foundation/proceso y producto con
`dad4e77`, los borradores encontrados y las referencias publicadas de I-52 V16. No editaron ni ejecutaron tests. No constituyen el
Arquitecto formal independiente. El texto heredado que afirmaba un pase ya finalizado sobre V15 se sustituye por este registro verificable.

| Hallazgo | Severidad | Correccion de V5 |
|---|---|---|
| EFFECTIVE por decision del Owner sin ambos registros | HIGH | §2.6 y artefacto exigen siempre el mismo SHA registrado por ambas |
| AUTH-07/08/13 y CT-16 podian quedar detras del producto I-52 | HIGH | Sin fallback; desacuerdo bloquea F1 de la unidad neutral |
| Guarda obligaba al Selectivo a resolver tambien en preflight | HIGH | OutputBlockedReason conserva null; CT-RES cuenta una llamada efectiva en BuildBom |
| Mapa/ADR inventaban creacion propia o prometian cancelacion limpia incondicional | HIGH | Dos modos de §4.6 alineados; AUTH-15 solo integrado; R-25 explicito |
| Clasificador omitia Cabecera vigente | HIGH | Predicado del wrapper Selective + Header vigente; forma legacy separada |
| Helpers de nombre duplicados y tipo de requisitos posterior a Plan | MEDIUM | Delegacion limitada de nombres en materializador; tipo puro nace en F5b |
| INV-RED-3, remedio Cantilever y supervivientes contradictorios | MEDIUM | Adapter abre una transaccion; Resolved + OutputBlocking; referencias de layout |
| Clasificacion por hermana e Id original de espacios no completos | MEDIUM | Clasificador puro independiente; AttributableIds comun para los gates |
| Referencias V15, claim incompleto, F8 y ADR mal atribuido | LOW/MEDIUM | V16 publicada; primer push aceptado; rebase vuelve a F7; ADR neutral no adoptado por I-52 |

Segundo pase tecnico: confirmo los HIGH anteriores corregidos y detecto un fallback restante en G15; se retiro del mapa, que ahora
exige AUTH-15 integrado. Se alineo G3 con la entrega completa AUTH-01..14 y se corrigieron citas, alternativas del ADR y entrada InMemory.
Estos ajustes son verificaciones documentales; no son un veredicto formal ni evidencia de comportamiento. Los conflictos
de gobernanza X-2/X-8, la titularidad y las decisiones Owner siguen abiertos: corregir el documento no los resuelve.
