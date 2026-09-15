# I-55 — Paquete de revision del Coordinador (Proposal V5)

```text
Coordinator = REVIEW REQUIRED
Architect formal = PENDING
Owner = PENDING
Consensus = NOT REACHED
SUBSTANTIVE IMPLEMENTATION = BLOCKED
FOUNDATION_ID = PENDING
VIEW FOUNDATION ADR = PROPOSED / NUMBER PENDING
ADR-0042 = PROPOSED
```

Este paquete permite revisar sin el contexto de la conversacion. No es un veredicto ni una autorizacion de implementar.
El objeto es documental; ninguna CI verde equivale a consenso, caracterizacion nueva o validacion en AutoCAD.

## 1. Objetos exactos y lectura

| Objeto | Revision exacta |
|---|---|
| Proposal V5 | commit `f49671e29c6cc817166c720fe3f975deb92d4c3b`; `docs/initiatives/I-55-proposal-v5.md` |
| Mapa V5 | mismo commit; `docs/initiatives/I-55-implementation-map-v5.md` |
| Especificacion neutral, delivery-map y ADR neutral | mismo commit; `docs/architecture/shared-view-foundation/` (excepto R1) |
| ADR-0042 e indice | mismo commit; `docs/adr/0042-preparacion-de-vistas-antes-de-materializar.md`, `docs/adr/README.md` |
| Reconciliacion R1 | **blob Git `2faa5a316680aa92306c7bce75c42cc311f96a26`**, ruta `docs/architecture/shared-view-foundation/reconciliation.md`; commit de publicacion literal en recibo G2F |
| Codigo y reglas vigentes | `dad4e77f4f267b9fa248ecb0c8bfd8a74bbab093` |
| V4 incorporada | `fe70d7a76c77f0c0eec01f242924a7a50a2adc4c`; V5 §0.5 enumera que se conserva y que se sustituye |
| I-52 vigente observada | `b7a6d9fe897dae4d29ae29ada7f60127bca365e5`; `docs/initiatives/I-52-proposal-v17.md` |
| I-49 vigente observada | `d54a8d7db830d763af293ab87ccd981c478c6272`; ADR-0043-P1 PROPOSED; base A3-R2 acordada, Owner pendiente |
| I-56 vigente observada | `4b04b23f8661d0d8a2a656985339c48796861966`; Proposal V3, Workflow V2 NOT EFFECTIVE |

No confundir commit con blob. `git show <commit>:<ruta>` recupera un archivo publicado; `git show <blob>` recupera R1 exacta.
No revisar solo el working tree: ramas paralelas pueden tener borradores mas nuevos no publicados. El recibo G2F fija los SHA de
publicacion de los paquetes y la CI de cada commit. R0 esta en A; R1 ancla A literalmente sin alterar sus contratos.

Orden: AGENTS, HANDOFF, README y ARCHITECTURE; WORKFLOW/ADR-0001; context packs declarados en contrato; V5 §0/§1/§2;
spec → reconciliation R1 → delivery-map → V5 §3..§10 → mapa V5 → ambos ADR. Consultar V4 exacta solo en partes incorporadas por §0.5.
Las reglas actuales y el codigo de main mandan sobre antecedentes historicos. ADR0034 y ADR0010 aceptados permanecen intactos.

## 2. Problema, takeover y alcance

RackCad es un plugin AutoCAD2025 (.NET8/C#/WPF): Domain y Application puros, UI sin AutoCAD, Plugin unico adaptador de AutoCAD.
I55 entrega ID17 primera vista libre, ID18 varias vistas de un rack en cola y ID19 vistas de varios racks preservando identidad/layout.
La arquitectura V4 hacia a I55 extractor unico de contratos que I52 necesita antes de su propio producto: generaba dependencia de una
rama no integrada. El Coordinador ordeno **CHANGES REQUIRED → V5**, aceptando AR4-01..50 como entrada tecnica vinculante.
AR4 procede del borrador heredado confirmado por la orden; no se recupero informe separado publicado y no es Architect formal.

El takeover preservo ocho archivos parciales legitimos (matriz por archivo en decisiones G2F). No reinicio la iniciativa, no modifico
produccion/tests/assets ni V1..V4, HANDOFF, ROADMAP o ADR aceptados. Dos revisores READ ONLY de esta sesion detectaron y comprobaron
correcciones documentales (V5 §11); ninguno sustituye la revision formal independiente solicitada aqui.

## 3. Arquitectura propuesta y propiedad

- **B recomendado:** iniciativa neutral independiente autorizada por Owner, FOUNDATION_ID PENDING. A subiniciativa viable con alcance
  neutral y consenso propio; C alternativas legales no cumplen entrega previa o solo preparan B. V5 §2 explicita claim remoto, rama,
  worktree, consenso, dependencias, riesgos e integracion. No se ha creado ninguna unidad nueva.
- AUTH01..14: taxonomia, direccion/variante/codec/disposicion sintactica, disponibilidad sobre sistema, marco/tramo/centro,
  hechos de seleccion y colocacion, Resolve, Plan, nombre base, requisitos/consulta, comparador authored y caracterizaciones compartidas.
- Fuera: politica RACKPROYECTAR/RACKMIRROR, Orthographic/Rigid, UX, ID18, atomicidad de hermanas, M01/OD, reflexion o read-set del espejo.
- Foundation F0 gobierno/claim → F1 caracterizacion → F2 codec → F3 marcos → F4 seleccion/placement → F5 Resolve/Plan/adaptadores/nombres
  → F6 comparador/consulta → F7 candidato → F8 integracion. Solo despues ambas consumidoras rebasan y consumen main.
- Un reclamo durable por autoridad; CT04/05/16 pertenecen a foundation. No primer gate ganador. Desacuerdo de titularidad bloquea F1,
  no traslada silenciosamente codigo neutral a I52 producto. Legacy exige caracterizacion/paridad/gate de retiro; registro vacio en F7.
- **EFFECTIVE** exige registro de I52 e I55 del mismo commit de artefacto, y foundation cuando exista. Owner puede resolver desacuerdo,
  pero no sustituye los registros. R1 sigue PROPOSED; I52 no la ha registrado. Cambios materiales se tramitan por CR-SVF.
- AUTH15 crear caller-owned queda fuera de foundation actual, propuesto a I52. G15 requiere integracion. G9b mantiene dos modos,
  incluido legado con R25 residual; no fabrica otra autoridad. Cambiar alcance exige CR08 y decision explicita.

## 4. Matriz de revision requerida

| Area | Comprobar | Fuentes |
|---|---|---|
| AR4-01..50 | Cada ID, severidad, problemaV4, resolucionV5, seccion, capa, caracterizacion/Owner/Architect/status; no cierre global ficticio | V5 §0.4 |
| X1 | Contenido compatible; ownership y CT16 unicos pendientes | V5 §7.1; R1 X1/CR02/04 |
| X2 | MATERIAL CONFLICT DE PROCESO; V17 aun permite primer extractor | R1 X2; I52 §15.4 |
| X3 | Comparador neutral; politica de miembros por consumidor | spec §3.13; R1 X3 |
| X4 | Separar requisito puro y consulta, crear nuevo AUTH15 y redefinir existente; ninguna autoridad propia en G15 | V5 §4.6; mapa G9b/G15; R1 CR08 |
| X5/6 | Compatibles; nuevos productores declarados G-M24, C2 sobre main combinado | V5 §7.1 |
| X7 | Valor/tolerancia antes F1; reflexion y rigidez como politicas distintas | spec §3.8; R1 CR01/02 |
| X8 | MATERIAL CONFLICT DE PROCESO; marco/tramo/centro neutral CT05; CT06 espejo sigue I52 | spec §3.5; I52 §15.4 |
| Codec/disponibilidad/politica | Sintaxis no infiere disponibilidad; hechos no deciden exposicion | spec §3.3/3.4; V5 §3.1/3.2 |
| Resolve | Sistema con diagnosticos, OutputBlocking sin perder sistema; forma Cabecera vigente; clasificador por miembro separado | spec §3.9 |
| ADR0034 | Adaptador del puerto despacha al handler; Selectivo resuelve en BuildBom una vez, preflight null; snapshot comun | spec §3.9.3; SelectiveKindHandler; BomTotal |
| Plan/nombres | Una autoridad; lambdas delegan, ningun helper duplicado; solo naming permitido en materializador Cantilever | spec §3.10/3.11; delivery F5b |
| Bloques | Claves con punto validas; presencia comprobada tras importar; no inventar biblioteca | spec §3.12; V5 §3.5 |
| CQ01 | PREPARE → ONE MUTATE → ONE COMMIT → POST; adapter abre una transaccion; unidades no abren ni confirman | V5 §4; mapa G9a/G9b |
| Membresia | Fuente elegida/blancos, Id original atribuible, xref solo lectura, LayoutReferenceCount; mismos miembros para gates | V5 §4.1/4.2 |
| Cancelacion | Redibujo conservado con supervivientes; nuevos jigs individuales; sin supervivientes modo1 atomico o modo2/R25 | V5 §4.6; ADR0042 |
| Producto intacto | Actualizar normal; BOM/counts/metadata/identidad; todos IC01..19 enumerados | V5 §6; V4 incorporada; mapa |
| ADRs | Neutral propuesto sin numero; producto0042 propuesto complementa0010; aceptaciones separadas | Ambos borradores |
| Evidencia | F7 exact SHA limpio, dos suites locales/builds/CI/Owner; rebase invalida; CI posterior al merge | delivery F7/F8; AGENTS |

Puntos de entrada al codigo: `src/RackCad.Plugin/KindHandlers/SelectiveKindHandler.cs`,
`src/RackCad.Plugin/RackInventarioCommands.BomTotal.cs`, `src/RackCad.Application/Persistence/RackProjectStore.cs`,
`src/RackCad.Application/Systems/Shared/SystemRegistry.Default.cs`, `src/RackCad.Plugin/Systems/Shared/ViewBlockDraw.cs`,
`src/RackCad.Plugin/Drawing/Cantilever/CantileverViewMaterializer.cs`, `src/RackCad.Plugin/RackBlockFinder.cs` y
`tests/RackCad.Tests/SelectiveBomAuthorityTests.cs`. Los prefijos y las citas detalladas a la base estan en V5/spec.

## 5. M01 y decisiones abiertas

Owner sigue PENDING. Comparar A ventana relativa al marco y B mayoria (empate→A), sin elegir por Owner. V5 §5.2 contiene los calculos
0°,180°, mezclas2/1 y1/2, epsilon1e-9rad, longitudes90/150 y vuelta. A conserva sentido bajo cambios de mayoria pero tiene discontinuidad
en frontera; B se adapta a mayoria pero puede reflejar los tramos al volver. La recomendacion tecnica A no es aprobacion.
`Rigid` §5.3 incluye posicion, orientacion, segundo punto, Origin y Z con oraculo independiente. Los tests futuros consumen CT05.
M01 incluye OD6/7 y OD2.b, no solo el sentido del eje. OD1..8, mecanismo/ID, CR/tolerancia y aceptaciones de ambos ADR siguen pendientes.

## 6. Salida de la revision y siguiente gate

Emitir veredicto propio sobre **estos objetos exactos**, con hallazgos BLOCKER/HIGH/MEDIUM/LOW, seccion, razon, cambio requerido y
clasificacion foundation/producto. Distinguir correccion documental, caracterizacion pendiente, decision Owner y conflicto entre
iniciativas. No declarar resuelta la adopcion I52, no inferir acuerdo del Owner y no tratar CI como evidencia de dibujo.
Una contradiccion material exige revision de Proposal y artefacto; cambio de objeto invalida el veredicto exacto previo.
El siguiente paso es revision/decisiones; ningun G3 ni implementacion abre por recibir este paquete.

## 7. Evidencia de publicacion

Commit A: CI de push [34990057900](https://github.com/marioap-afk/Calculadora_de_racks/actions/runs/34990057900), head exacto A.
Su resultado verificado y el de la publicacion de estos paquetes se registran en decisiones G2F y el recibo final.
Chequeos documentales: matriz50/IC19 completos, tablas y enlaces locales validos, diff --check; revisiones READ ONLY sin pendientes
BLOCKER/HIGH internos tras correcciones. Los conflictos materiales declarados de gobernanza siguen abiertos.

**Mandato del Coordinador:** verificar cumplimiento de G2F y entregar decisiones pendientes al Owner. Registrar REVIEW REQUIRED hasta
veredicto propio; M01 abierta impide AGREED. La revision AR4 y los pases de esta sesion son entradas, no consenso formal.
