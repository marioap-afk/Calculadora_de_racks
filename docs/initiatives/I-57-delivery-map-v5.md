# I-57 — Delivery Map V5

Mapa de la [Proposal V5](I-57-proposal-v5.md). F1 permanece completo; F2 se detuvo antes de produccion y no reabre
hasta review exacta de V5.

| Gate | Cambios candidatos | Evidencia y stop |
|---|---|---|
| F1 | solo tests, fixtures y helpers estrictamente necesarios para caracterizacion | COMPLETE; F1 original queda historica y CT-04 V2 agrega la correccion falsable; cero cambios productivos; 0 tests = fallo |
| F2 | values/codec/availability y delegacion controlada de readers | BLOCKED hasta Coordinator + Architect `AGREED` sobre el mismo SHA V5. Exige CT-04 V2, address/disposition/coercion code, round-trip canonico, availability posterior, paridad por reader, cero policy leak, cero schema y cero cambio observable |
| F3 | frames/adapters por kind | CT-05 + OV-FND-01; no inferir span solo de bounds |
| F4 | snapshots/classifier/placement | CT-16/SCAN/GEO; identidad/COPY fuera; AutoCAD solo Plugin |
| F5 | Resolve/PrepareView/naming | CT-RES/PLAN/NAME + OV-FND-02/03; un resolver/builder vigente |
| F6 | requirements/query/comparators | BLK-ID-1..6 + BLK-AVAILABILITY-01..03 + CT-BLK/AUTH; ensure/import precede al query final cuando aplica; query puro; importer intacto; unknown fail-closed |
| F7 | candidato y retiro demostrado de duplicados | T2/T4 sobre SHA exacto limpio |
| F8 | integracion serial y docs de cierre | T3; rebase invalida evidencia; WORKFLOW completo |

Archivos calientes se serializan: primero autoridad I-49 sobre Selectivo; luego
`DimensionViewPolicy.cs`, `RackDuplicationPlan.cs`, handlers, comandos, DrawServices y materializer en commits
cerrados. HANDOFF/ROADMAP solo se tocan al integrar.

Owner Validation: OV-FND-01 actualiza/inserta vistas de seis kinds; OV-FND-02 compara BOM/listado y errores;
OV-FND-03 compara nombres/importacion/geometria; OV-FND-04 compara RACKDUPLICAR, identidades y rechazos.

```text
Delivery Map V5 = REVIEW REQUIRED
Latest Rn today  = R3 · EFFECTIVE
ADR-0044         = ACCEPTED
F1               = COMPLETE
F2               = BLOCKED
F3               = NOT OPEN
```
