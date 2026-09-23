# I-58 — paquete completo de conformidad F3

Preparado por Executor. PASS significa evidencia factual del invariante, NO CONFORMING del Coordinator
o Architect. READY-06 necesita ambos veredictos sobre el mismo SHA propuesto en el recibo de publicacion.
Revisar la unidad completa, no solo el delta F3. No Candidate ni tag integration/I-58.

## Fuentes exactas

- FREEZE_SHA: fd13dd8d57da5c27889dec93dd288ae5259fc318.
- Freeze: docs/initiatives/I-58-freeze-draft.md, blob f89671cfe9f1036e24211287514414b3f555182a.
- Characterization: docs/initiatives/I-58-characterization.md, blob 97e15d6d7c00e02495f8d4159477272d00af6880.
- Discovery: docs/initiatives/I-58-discovery.md, blob f07c9b6c6f6c278c6f7d37b3b68921eac7a1c5b8.
- A-n: NONE. No cambio de decision; EXP-01 B confirmado solo para I-58 permanece acotado.
- F1: cierre 13381b6c04d886f0478e2b264145d417ce95228f; correccion administrativa d1921dc90abdc3b24e7f05c8beca9ab14a502c49.
- F2: cierre e27df1be2fbca55303c6cde6d6f27c5f35ba6b10; publicacion 13a1be8c51a61fcc8f0810963fdd867d2a32a05f.
- F1/F2 COMPLETE / COORDINATOR AGREED comunicados por Owner en la orden F3. No se inventa modalidad
  o autoria de las revisiones externas. Los textos historicos dentro del Freeze no se reescriben.

## Matriz F-01..13

Implementacion productiva bajo src/RackCad.Application/Systems/Shared; pruebas bajo tests/RackCad.Tests.
Evidencia: [F1](../I-58-f1/README.md), [F2](../I-58-f2/README.md), [F3](README.md).
Los IDs de CT/MM se expanden a fixtures y tests reales en [matriz](ct-mm-matrix.md).

| Clausula | Implementacion | Prueba real / obligaciones | Evidencia | Estado factual |
|---|---|---|---|---|
| F-01 ownership/outcomes | RackAuthoredComparator + FourKinds; tres outcomes originales | F6 AUTH13_SELECTIVE_REUSES_AUTHORED_AUTHORITY_FOR_SINGLE_DIVERGENT_AND_UNREADABLE; F1 matrix | F2 + impacto F3 | PASS |
| F-02 scope/compatibilidad | Selective original; Cama/genericos Unsupported; cuatro factories concretas | I58F1OracleChecks.CT58_22_arbitrary_marker_and_Cama_remain_unsupported; F6 unsupported | F3 scope diff + impacto | PASS |
| F-03 carrier/identidad | RackAuthoredInput + AuthoredRawReader; Cantilever Id acreditado separado de outer | F1 CT01..04,23/24; F2 Carrier_captures_sources_and_does_not_allow_list_replacement; F3 Cantilever | F2 389 causas + F3 seam | PASS |
| F-04 secuencia | FourKinds: strict read -> canonical -> equality -> materialize | F1 CT09/10/17; F2 Raw_gate_negative_control | F2 controles raw + impacto F3 | PASS |
| F-05 precedencia/conjunto | FourKinds inspecciona todas antes de Divergent | F1 CT05..08; F3 MixedUnreadable A/B/X y permutacion | F1/F2 matriz; F3 failures | PASS |
| F-06 dominio/canonical tipados | canonical privado por kind; retorno de cuatro dominios concretos | F1 CT25/26; F3 compilacion sin conversion DTO en entrada AUTH09 | F2 fidelity + F3 seam | PASS |
| F-07 inventario authored | AuthoredTypedValues por campo, lista y subarbol | F1 CT11..16, MM38; I58F1DomainOracle | F2 coverage + impacto F3 | PASS |
| F-08 authored/effective | AuthoredCanonical: exclusiones acreditadas; global0 Header completo | F1 CT18/19,29..32; F2 materialization; F3 dynamic positive/custom y PB composite | F2 12 sabotajes + F3 parity/isolation | PASS |
| F-09 legacy/nullable/orden | defaults acotados y rechazo de perdida; sin sort/dedup | F1 CT11..16/20; F2BoundaryTests y writer roundtrip | F1/F2 + impacto F3 | PASS |
| F-10 schema | AuthoredRawReader.Schema exige string y aplica Trim solo al interpretar version conocida/legacy | I58Conf58SchemaWhitespaceTests (28 fronteras); F1 CT09/17; F2 Reader | [CONF58-01 RED/GREEN](../I-58-conf58-01/README.md) y nuevo recibo exacto | CORRECTED / COORDINATOR RE-REVIEW REQUIRED |
| F-11 unknown/duplicados | Shapes recursivo cerrado, antes de exclusion/mapper | F1 CT10/17; F2 strict shape/unknown/duplicate | F2 controles causales + impacto F3 | PASS |
| F-12/12a Single/parity | materializacion dominio nueva, profunda, fiel, sin resolver | F1 CT26..32; F2MaterializationTests; F3 value/isolation y parity original | F2 y F3 mutation controls | PASS |
| F-13 seam | AUTH09 RackResolvePorts + AUTH10 RackViewPreparationPorts originales, composicion pura | I58F3SeamTests.Real_authorities_preserve_authored_across_the_complete_seam | F3 56 escenarios + controles reales | PASS |

## Composicion e aislamiento

| Kind | AUTH-13 -> AUTH-09 autoridad real | AUTH-10 preparacion real |
|---|---|---|
| Dynamic | DynamicRackDesign -> DynamicRackSystemResolver.Resolve una vez | DynamicSystemFrontalBuilder.BuildPlan |
| PushBack | PushBackDesign -> PushBackResolver.Resolve una vez | PushBackSystemFrontalBuilder.BuildPlan |
| Cantilever | CantileverLineDesign -> CantileverLineEditorAssembler.Build una vez | reutiliza Views.First de esa misma computation; no segundo resolver |
| Cabecera | RackFrameConfiguration -> BracingPanelMemberBuilder.RefreshPhysicalModel sobre copia | PlantaHeaderLayoutBuilder.Build sobre effective |

Cada llamada contada ejecuta la autoridad real; no hay mocked results. Contadores separados para la
entrada AUTH09, su delegate, entrada AUTH10 y builder. Fallos Divergent/Unreadable: todos cero.
Singles: todos uno. Los controles de doble autoridad, re-resolve desde preparacion y doble builder
ejecutan llamadas reales adicionales y hacen fallar las aserciones de cardinalidad.

Las copias se preparan por kind antes de AUTH09 (helpers de copia test-only existentes F1, no un nuevo
materializador productivo). Oraculos tipados completos verifican authored antes/despues de resolve,
prepare y mutaciones profundas de working/effective; otra comparacion conserva todos los valores.
El control sin copia omite tambien la asercion preliminar de referencia para alcanzar el fallo conductual.
Cabecera Members y panel Members permanecen vacios en authored y se reconstruyen solo en effective.
Cantilever conserva su inner GUID distinto del outer. Dynamic/PB retienen Header, modulo, orden,
provenance y global0. Paridad con original resuelto en el mismo catalogo, fuera de los contadores de
la operacion medida; PB compuesto exige literalmente rack=7/A=7/B=9.

## OV — asignacion completa, sin escenario retirado

| ID | Unidad/momento congelado | Aplicabilidad/evidencia |
|---|---|---|
| OV58-01 | I-58 F1/F2 | automatizada pura: CT/MM, reader y mutaciones; PASS |
| OV58-02 | I-58 F2 y seam F3 | independencia authored/effective y aislamiento profundo; PASS |
| OV58-03 | I-58 F1/F2 | legacy save/reopen y schema/unknown con serializer real; PASS |
| OV58-04 | Owner I-58 Candidate si naturaleza real lo activa | applicable at Candidate = NO para el diff actual; ver razon abajo |
| OV-ID18 / G12 | I-55 despues de integracion I-58 | fuera de esta entrega, no acreditado ni desbloqueado |

OV58-04 applicable at Candidate = NO: F2 solo cambia autoridad pura de Application sin consumer visible;
F3 solo agrega integracion automatizada y documentacion. Cero cambios a dibujo/edicion/save-reopen, UI,
Plugin, resolvers o stores. No se ejecuta AutoCAD ni se declara PASS manual. Esto evalua el disparador
condicional congelado, no retira/reasigna el escenario. Metadata requires_autocad/owner_validation se
conserva. Guide §7.1 y Freeze §8 exigen evaluar la naturaleza; Coordinator/Architect deben confirmar esta
evaluacion durante READY-06 antes de Candidate. Si un diff posterior activa comportamiento visible,
se reevalua contra su nueva identidad y se prepara OV aplicable; no se reutiliza esta conclusion a ciegas.

## READY, en orden

1. READY-01: PASS factual — Freeze completo, alcance cerrado, A-n NONE.
2. READY-02: F1/F2 acordados. F3 COMPLETE / COORDINATOR REVIEW REQUIRED, cierre funcional acreditado;
   documentos propios y draft FOUNDATIONS versionados. El cierre formal de Coordinator sigue reservado.
3. READY-03: CONF58-01 corregido, pendiente de re-review del Coordinator; el Executor no lo declara
   RESOLVED ni emite conformidad. Sin nueva decision de contrato ni A-n.
4. READY-04: PASS factual — fetch/preflight final en preflight.json; main no avanzo y es ancestro.
   No rebase necesario. I-52 avanzo solo documentacion V34, sin contradiccion material.
5. READY-05: cierre funcional con Core Full limpio y CI 4/4 acreditados en closure-receipt.json.
   La punta documental propuesta se entrega solo tras focal/impacto limpio y CI propios; identidad final
   en informe de entrega. No hereda evidencia local por arbol.
6. READY-06: PENDING CONFORMANCE REVIEW — Coordinator + Architect deben revisar el MISMO SHA completo.
7. READY-07: arbol limpio exigido y comprobado en entrega, con HEAD identificado; no constituye
   avance formal sobre READY-06.
8. READY-08: matriz OV preparada completa arriba; evaluacion OV58-04 NO para diff actual a confirmar en review.
9. READY-09: comprobar blobs, acuerdo de entrada comunicado, A-n NONE y diff permitido contra main; identidad
   propuesta en recibo. No emitir acuerdos de conformidad ajenos ni declarar completada la secuencia READY.

La frontera externa READY-06 detiene Candidate. Los checks posteriores solo se preparan/inspeccionan:
no se declara READY global ni se anticipa Coordinator/Architect = CONFORMING.


## Paquete entregado y acuerdos que faltan

Cierre funcional: `5386a211e49ae9c8a22eacc8cdcc3227cd95b584`, recibo Core/builds/CI propio versionado.
La punta documental que contiene este paquete, identificada en el informe final y su CI de push,
es el SHA propuesto para conformidad completa. F1/F2 acordados por Coordinator (estado de entrada).
F3 se entrega para revision Coordinator, conjuntamente con la conformidad global requerida de ambos roles.
Coordinator = CONFORMANCE REVIEW REQUIRED. Architect = CONFORMANCE REVIEW REQUIRED.
No se emite CONFORMING ni se declara Candidate. El significado formal/orden de READY permanece intacto.


## CONF58-01 — nueva ronda de conformidad

El Owner rechazo `2e141b034b485f5faa55d412b476e99ebad23c5c` para READY-06 por SchemaVersion con
whitespace exterior. El PASS factual anterior de F-10 fue insuficiente y queda corregido por la fila
actual y [paquete de correccion](../I-58-conf58-01/README.md). No se reescribe evidencia historica.
La nueva interpretacion aplica el contrato ya congelado: Trim solo sobre el string de version antes de
compararlo con versiones conocidas. Version vacia/blanca/futura y tipos no string siguen Unreadable.

Las identidades F3 anteriores son historicas, no la punta propuesta de esta ronda. El recibo nuevo y
el informe final identifican el SHA de cierre de CONF58-01 y la publicacion propuesta para READY-06;
esta ultima exige focal/impacto y CI propios. Coordinator y Architect deben revisar esa identidad exacta.
Freeze/Characterization/Discovery intactos, A-n NONE. CONF58-01 = CORRECTED / COORDINATOR RE-REVIEW REQUIRED.
F2 = COMPLETE / CORRECTION APPLIED; F3 = COMPLETE / CONFORMANCE RE-REVIEW REQUIRED; READY-06 = PENDING.
FINAL_CANDIDATE_SHA = NOT DECLARED; IMPLEMENTATION AUTHORIZATION = NO.
