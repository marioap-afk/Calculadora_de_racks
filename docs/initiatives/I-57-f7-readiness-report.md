# I-57 — F7 Readiness Report / Dry Run

```text
Audit basis       = F6_VALIDATION_SHA 415953fefa5484b9d23829eb3b7b3b18b55b4389
Workflow          = V1 grandfathered; WORKFLOW_V2_EFFECTIVE_SHA exists at 8a021fb67c16dfccd6afc18448ea7e6a71a32364
F7 gate           = NOT OPEN
F7 readiness      = PREPARED / BLOCKED BY F6 OWNER VALIDATION AND COORDINATOR GATE
Candidate         = NOT DECLARED
Integration       = NOT AUTHORIZED
```

## Inventario AUTH-01..13

| AUTH | Autoridad implementada | Productores legacy | Disposicion | Evidencia / Owner |
|---|---|---|---|---|
| 01 | `DimensionViewKind` | enum vigente | KEEP | CT-04 V2; F2 |
| 02 | `RackViewAddress` | indices por reader | DELEGATED | CT-04 V2; F2 |
| 03 | `RackViewCodec` | readers por kind | DELEGATED, policy KEEP | CT-04 V2; F2 |
| 04 | `RackViewAvailability` | checks por sistema | DELEGATED | CT-04 V2; F2 |
| 05 | `RackViewFrame` + adapters | builders geometricos | KEEP builders / DELEGATED facts | CT-05; OV-FND-01 PASS en F3 SHA |
| 06 | scan projection y clasificador puro | probe AutoCAD | KEEP Plugin / DELEGATED classifier | CT-SCAN; F4 |
| 07 | seleccion fisica neutral | `RackDuplicationPlan` | KEEP COPY/identity/name policy | CT-16; OV-FND-04 pendiente |
| 08 | `Transform2D` + `RackTransformFacts` | consumidores de colocacion | KEEP matrix / DELEGATED facts | CT-GEO; F4 |
| 09 | `IRackResolvePort` por kind | resolvers/handlers | KEEP / DELEGATED | CT-RES; OV-FND-02 PASS en F5 SHA |
| 10 | `RackPreparedView<TPayload>` | builders por kind | KEEP / DELEGATED | CT-PLAN; OV-FND-03 names/geometry PASS en F5 SHA |
| 11 | `RackViewBaseName` | strings duplicadas | DELEGATED / REMOVED | CT-NAME; OV-FND-03 names PASS en F5 SHA |
| 12 | requirements + query + importer boundary | `EnsureForPlan` | DELEGATED; importer KEEP Plugin | CT-BLK; OV-FND-03 importation pendiente |
| 13 | comparator port por kind | `SelectiveAuthoredAuthority` | KEEP / DELEGATED; otros Unreadable | CT-AUTH; OV-FND-04 pendiente |

AUTH-15 sigue fuera. El barrido estatico no encontro codec, frame, scan, transform decomposition, resolve, builder,
BaseName, requirements o comparator neutrales paralelos. Permanecen multiples policies de producto deliberadas:
seleccion/exposicion de vistas, COPY/restamp, naming final, materializacion, acceptance/rejection y UX.

## Matriz de Candidato F7 preparada

1. Coordinator cierra F6 tras `OV-FND-03 importation = PASS` y abre F7 explicitamente.
2. `git fetch --all --prune`; clasificar drift de `main`, I-52 e I-55; rebase solo conforme WORKFLOW.
3. Auditar cero productores paralelos AUTH-01..13 y ningun AUTH-15.
4. Crear el commit Candidato; confirmar arbol limpio y SHA exacto.
5. Ejecutar, sobre ese SHA: focal Foundation; T1/parity; Core Full; UI Full; Debug UI; Debug Plugin.
6. Push de la rama exacta; exigir CI `event=push`, `ref` exacta, `head_sha` exacto y cuatro jobs `success`.
7. Ejecutar T4/Owner Validation completa sobre el mismo Candidate SHA. Los PASS historicos de F3/F5/F6 cierran
   sus gates, pero no se transfieren automaticamente a un SHA de Candidato distinto.
8. Registrar CANDIDATE_SHA y solicitar integracion; F8 continua cerrada hasta autorizacion.

## OV-FND-04 — matriz preparada, no ejecutada

| Escenario | Expected |
|---|---|
| una referencia, `Multiple` y `Unica` | comportamiento historico y nombres `copia`, `copia 2`; keywords no consumen numeracion |
| varias vistas de un rack | un GUID nuevo compartido, mismo nombre, solo vistas seleccionadas; editar copia no toca original |
| varios racks y destinos | identidad nueva por rack/destino; copy count y disposicion relativa correctos |
| referencias enlazadas | siguen enlazadas; listado y BOM conservan conteo |
| grouping authored Single | grupo unico conforme autoridad Selective vigente |
| authored Divergent | rechazo total antes de clonar; ninguna hermana elegida |
| mixed selection | no-rack/Paper Space ignorados con aviso; racks validos se duplican |
| xref | clasificacion y rechazo/omision legacy, sin inventar identidad |
| missing RackId | semantica legacy de definicion conservada; no se inventa RackId |
| unknown/unreadable | fail-closed antes de destino o escritura parcial |
| transform no soportada | rechazo legacy; sin geometria parcial |
| save/reopen + restamp | GUID/nombre/vistas/bindings persisten; original intacto |

`OV-FND-04 = PREPARED`, nunca PASS. La guia canonica de `RACKDUPLICAR` es
[`validacion-manual-autocad.md`](../guias/validacion-manual-autocad.md) seccion 5.7.

## Riesgos y blockers

- **Blocker actual:** OV-FND-03 importation A-E pendiente del Owner sobre `415953fe...`.
- **Gate:** F7 no tiene autorizacion del Coordinator.
- **Candidate:** no existe; cualquier rebase o cambio crea SHA y evidencia nuevos.
- **Drift al preparar:** `main=a61850a...`, I-52=`ebdb358b...`, I-55=`c20173cb...`; ninguno cambio durante F6.
- **Material contradictions:** NONE.
- **Open Material:** NONE fuera del Owner gate.
- **Open Minor:** NONE.
