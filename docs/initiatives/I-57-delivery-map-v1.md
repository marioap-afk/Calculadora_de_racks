# I-57 — Delivery Map V1

Mapa de la [Proposal V1](I-57-proposal-v1.md). Es diseno; todos los gates siguen cerrados.

| Gate | Cambios candidatos | Evidencia y stop |
|---|---|---|
| F1 | solo caracterizaciones/fixtures | diez CT; expected incierto o 0 tests = STOP |
| F2 | values/codec y delegacion de readers | CT-04; sin schema ni policy |
| F3 | frames/adapters por kind | CT-05 + OV-FND-01; no inferir span solo de bounds |
| F4 | snapshots/classifier/placement | CT-16/SCAN/GEO; identidad/COPY fuera; AutoCAD solo Plugin |
| F5 | Resolve/PrepareView/naming | CT-RES/PLAN/NAME + OV-FND-02/03; un resolver/builder vigente |
| F6 | requirements/query/comparators | CT-BLK/AUTH; importer intacto; unknown fail-closed |
| F7 | candidato y retiro demostrado de duplicados | T2/T4 sobre SHA exacto limpio |
| F8 | integracion serial y docs de cierre | T3; rebase invalida evidencia; WORKFLOW completo |

Archivos calientes se serializan: primero autoridad I-49 sobre Selectivo; luego
`DimensionViewPolicy.cs`, `RackDuplicationPlan.cs`, handlers, comandos, DrawServices y materializer en commits
cerrados. HANDOFF/ROADMAP solo se tocan al integrar.

Owner Validation: OV-FND-01 actualiza/inserta vistas de seis kinds; OV-FND-02 compara BOM/listado y errores;
OV-FND-03 compara nombres/importacion/geometria; OV-FND-04 compara RACKDUPLICAR, identidades y rechazos.

```text
Delivery Map V1 = PROPOSED
All gates        = CLOSED
F1               = NOT OPEN
```
