# I-57 — Paquete de revision del Architect — Proposal V3

## Objeto exacto

```text
Branch        = architecture/shared-view-foundation
BASE_SHA      = dad4e77f4f267b9fa248ecb0c8bfd8a74bbab093
DISCOVERY_SHA = a3341068137931203de00fc769e4675db7b7d3a8
PROPOSAL_SHA  = 68a56b2a3c64ce261787224753ed43b2b8e328a3
Proposal blob = 54b9097dc417f745a31ea06a01717f86bbf290ab
Map blob      = 518893443bdae0283c029510ddb17c974df7b5fa
R3 blob       = cd42db03becff42f98b047e61c46689c17a69670
ADR blob      = d31f202e4d52f7bd9a1cccf41bb06791ab97c734
```

Revisar contra el codigo de BASE_SHA y el Discovery. V3 es V2 mas el cierre acotado de I57-AR-01 e I57-AR-02.
La revision no implementa, acepta ADR-0044, declara consenso ni abre F1.

## Traza de V2 y foco de V3

```text
Reviewed V2 SHA = 71f87db0c4ee980976c25573f217173b139bc166
Architect V2    = CHANGES REQUIRED
I57-AR-01       = HIGH / MATERIAL
I57-AR-02       = LOW / NON-MATERIAL
Coordinator     = ACCEPTS BOTH FINDINGS
Architect V3    = REVIEW REQUIRED
```

La re-review puede concentrarse en el cierre de ambos findings y en una regression check de los contratos V2
previamente revisados. El Architect conserva el derecho a detectar una contradiccion material nueva.

## Ataques obligatorios

1. **AR-01 / identity spaces:** `LibraryBlockRequirement.Key` identifica pieza de library y `BaseName` solo la
   base de la definicion generada; no existe sustitucion, derivacion ni fallback entre ambas.
2. **Pipelines:** requirement/query/import y BaseName/collision/generated definition permanecen separados.
3. **CT-BLK:** BLK-IDENTITY-01..03 son falsables y detectan consulta de BaseName, mutacion por suffix y sanitizer
   aplicado a una library key con `.`.
4. **Casos adversariales:** requirement ausente aunque BaseName exista, duplicadas/empty/whitespace, cero
   requirements en geometria pura, Cantilever sin library y HeaderRun con piezas.
5. **AR-02:** ADR-0044 referencia Proposal V3 y R3 exacta, permanece `PROPOSED` y no cambia su decision.
6. **Regresion AUTH:** 01..14 conservan las disposiciones de V2; AUTH-15 sigue fuera.
7. **Payload/capas:** carrier tipado por kind, sin geometria universal, JSON/reflection u `object` opaco; AutoCAD
   permanece Plugin.
8. **ADR-0034/plan:** una resolucion efectiva por rack dentro del handler; Resolve/Prepare delegan en autoridades
   vigentes sin crear resolver o builder paralelo.
9. **Codec/frame/selection/comparator:** syntax, availability y policy separadas; frame fisico intacto; COPY e
   identidad fuera del nucleo neutral; comparator por kind sin elegir hermana.
10. **F1:** solo caracterizaciones; contradiccion material fuerza STOP y Proposal V4.
11. **R3:** blob inmutable, AUTH-15 fuera, sin dependencia entre feature branches y consumo solo por Integration
    SHA en `main`.
12. **Estado:** Coordinator y Architect deben emitir `AGREED` sobre este mismo SHA antes de consenso; ADR y R3
    mantienen sus compuertas externas; F1 sigue cerrado.

## Formato solicitado

```text
Architect Review — I-57 Proposal V3
Reviewed SHA = 68a56b2a3c64ce261787224753ed43b2b8e328a3
GLOBAL VERDICT = AGREED | AGREED WITH CHANGES | CHANGES REQUIRED
A1..A12 = YES | NO
I57-AR-01 = CLOSED | OPEN
I57-AR-02 = CLOSED | OPEN
R3 = REGISTER | CHANGES REQUIRED
ADR-0044 = ACCEPTABLE | CHANGES REQUIRED
Findings = NONE | [BLOCKER | HIGH | MEDIUM | LOW] I57-AR-XX — evidencia — cambio — Material YES/NO
Architect = AGREED | REVIEW REQUIRED
Consensus = NOT REACHED
Implementation = BLOCKED
F1 = NOT OPEN
```
