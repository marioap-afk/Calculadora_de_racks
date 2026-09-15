# I-57 — Paquete de revision del Architect — Proposal V4

## Objeto exacto

```text
Branch        = architecture/shared-view-foundation
BASE_SHA      = dad4e77f4f267b9fa248ecb0c8bfd8a74bbab093
DISCOVERY_SHA = a3341068137931203de00fc769e4675db7b7d3a8
PROPOSAL_SHA  = 2a142f224fb8d8a0c16bd7f7334dc48be3be9eef
Proposal blob = 5cf5e00310bd8d40c0ef0686209c41e6aa954c70
Map blob      = 1e36ec60e695bb635c0664babe3f927a233f97b9
R3 blob       = cd42db03becff42f98b047e61c46689c17a69670
ADR blob      = 9f1d4e94f06f4de6e19c8668fe0c8092ac12c933
```

Revisar contra el codigo de BASE_SHA y el Discovery. V4 es V3 mas el cierre acotado de I57-AR3-01 y preserva
I57-AR-01/02 cerrados. La revision no implementa, acepta ADR-0044, declara consenso ni abre F1.

## Traza de V3 y foco de V4

```text
Reviewed V3 SHA = 68a56b2a3c64ce261787224753ed43b2b8e328a3
Architect V3    = CHANGES REQUIRED
I57-AR3-01      = HIGH / MATERIAL
I57-AR-01/02    = CLOSED
Coordinator     = ACCEPTS I57-AR3-01
Architect V4    = REVIEW REQUIRED
```

La re-review puede concentrarse en I57-AR3-01 y en una regression check de los contratos previos. El Architect
conserva el derecho a detectar una contradiccion material nueva.

## Ataques obligatorios

1. **AR3-01 / orden:** con import permitido, `Ensure/import` precede al query de availability definitiva.
2. **Sin import:** el query directo es definitivo y no dispara importacion implicita.
3. **Pureza:** query no importa, crea, repara, normaliza, sanitiza ni muta `Database`.
4. **Resultado final:** un `MISSING` preliminar no queda sticky; intento de import no implica `FOUND`.
5. **CT-BLK:** BLK-AVAILABILITY-01..03 son falsables ante import exitoso, flujo sin import e import fallido.
6. **Import parcial:** facts finales por requirement y cero false success global.
7. **Identidades:** BLK-ID-1..6 y BLK-IDENTITY-01..03 permanecen integros; importer/query nunca reciben BaseName.
8. **Regresion AUTH:** 01..14, payload, codec/availability/policy, frame, Resolve/Plan, selection y comparators
   conservan V3; AUTH-15 sigue fuera.
9. **ADR-0034:** una resolucion efectiva por rack dentro del handler; no aparece segunda autoridad.
10. **F1/gates:** solo caracterizaciones; contradiccion material fuerza STOP y Proposal V5; F1-F8 intactos.
11. **R3/ADR:** R3 conserva blob y orden; ADR-0044 referencia V4 y R3, sigue `PROPOSED`.
12. **Estado:** ambos reviewers deben emitir `AGREED` sobre este SHA; R3 debe ser efectiva y ADR aceptada antes de
    F1.

## Formato solicitado

```text
Architect Re-review — I-57 Proposal V4
Reviewed SHA = 2a142f224fb8d8a0c16bd7f7334dc48be3be9eef
GLOBAL VERDICT = AGREED | AGREED WITH CHANGES | CHANGES REQUIRED
A1..A12 = YES | NO
I57-AR3-01 = CLOSED | OPEN
Regression = PASS | FAIL
R3 = REGISTER | CHANGES REQUIRED
ADR-0044 = ACCEPTABLE | CHANGES REQUIRED
Findings = NONE | [BLOCKER | HIGH | MEDIUM | LOW] I57-AR-XX — evidencia — cambio — Material YES/NO
Architect = AGREED | REVIEW REQUIRED
Consensus = NOT REACHED
Implementation = BLOCKED
F1 = NOT OPEN
```
