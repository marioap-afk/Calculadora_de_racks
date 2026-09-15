# I-57 — Delivery Map V2

Mapa de la [Proposal V2](I-57-proposal-v2.md). Es diseno; todos los gates siguen cerrados.

| Gate | Cambios candidatos | Evidencia y stop |
|---|---|---|
| F1 | solo tests, fixtures y helpers estrictamente necesarios para caracterizacion | entrada: Proposal exacta AGREED, ADR-0044 ACCEPTED, latest Rn EFFECTIVE con mismo SHA I-52/I-55/I-57 y sin CR material; hoy R3 no cumple. Diez CT; contradiccion material = STOP → Proposal V3; 0 tests = fallo |
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
Delivery Map V2 = PROPOSED
Latest Rn today  = R3 · NOT EFFECTIVE
All gates        = CLOSED
F1               = NOT OPEN
```
