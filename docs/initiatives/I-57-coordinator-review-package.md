# I-57 — Paquete de revision del Coordinator — Proposal V5

## Objeto exacto

```text
Branch             = architecture/shared-view-foundation
CURRENT_MAIN       = dad4e77f4f267b9fa248ecb0c8bfd8a74bbab093
F2_START_SHA       = 6448af15b07eddc9ca784d3cf3a67a7923ea1e78
PROPOSAL_V5_SHA    = 91a3d1ca779e57a5c47f474d9ae18d5a01e3f8ca
Proposal blob      = c50141f425d191a07c608693b099e5a018fd15e0
Delivery Map blob  = 40161e384201ec4bb1420b76e95cbd8ce6ac1529
CT-04 V2 blob      = cbdca0eb7f16daa0d44319899c4d143452d530af
Correction blob    = c5f5791796b31d382e9fb6ca85611ac1008cb97a
R3 blob            = cd42db03becff42f98b047e61c46689c17a69670
ADR-0044 blob      = 30eacca0dc43263100ee03b0cd3972b175a879b9
CI push run        = 35035083116 · 4/4 SUCCESS · exact head SHA
```

Revisar Proposal V5, Delivery Map V5, CT-04 V2, correction evidence, R3 y ADR-0044 en ese commit exacto.
La revision no implementa AUTH, migra readers, reabre F2, modifica R3/ADR ni declara consenso.

## Trigger y alcance del cambio

F1 permanece `COMPLETE`. Al abrir F2, CT-04 contradijo coerciones productivas vigentes. La sesion ejecuto el
STOP de V4 antes de modificar produccion. V5 preserva toda la arquitectura V4 salvo el contrato derivado de
AUTH-03/CT-04:

- define Canonical, Canonicalizable, Coerced e Invalid;
- conserva original syntax, address opcional y coercion code;
- separa persisted decode de solicitudes interactivas;
- mueve existencia de Fondo/Post/Station a AUTH-04;
- mantiene toda decision de accept/reject/prompt/erase/abort en el consumer;
- no cambia schema ni comportamiento legacy.

## Preguntas obligatorias

1. ¿CT-04 V2 describe fielmente Selectivo, Dinamico, Push Back, Cantilever, Cabecera y Cama?
2. ¿los ocho readers censados estan cubiertos y las rutas interactivas quedan separadas?
3. ¿cada `Coerced` tiene address y codigo estable; cada `Invalid` carece de address?
4. ¿unknown/null/blank/whitespace/case se clasifican por kind segun el reader real, sin regla global falsa?
5. ¿Dynamic redraw a Post(0) queda separado de prompt/whole-system insertion?
6. ¿syntax valida con variante inexistente llega a AUTH-04 como `VariantNotPresent`?
7. ¿encode canonico despues de coercion no promete round-trip literal ni reescribe DWG?
8. ¿consumer policy permanece fuera del codec y availability?
9. ¿R3 conserva ownership, blob y efectividad sin requerir R4?
10. ¿ADR-0044 sigue compatible y aceptado sin cambio de decision?
11. ¿V5 no reabre payload, BLK-ID/AVAILABILITY, ADR-0034, frames, Resolve/Plan, selection o comparators?
12. ¿F2 permanece bloqueado hasta ambos veredictos AGREED sobre este SHA?

## Formato solicitado

```text
Coordinator Review — I-57 Proposal V5
Reviewed SHA = 91a3d1ca779e57a5c47f474d9ae18d5a01e3f8ca
GLOBAL VERDICT = AGREED | AGREED WITH CHANGES | NOT AGREED
Q1..Q12 = YES | NO
Material contradiction = RESOLVED | OPEN
CT-04 V2 = ACCEPTED | CHANGES REQUIRED
R3 = REMAINS EFFECTIVE | R4 REQUIRED
ADR-0044 = COMPATIBLE | CHANGES REQUIRED
Regression = PASS | FAIL
Findings = NONE | [BLOCKER | MATERIAL | MINOR] I57-COORD5-XX — evidence — required change
Coordinator = AGREED | REVIEW REQUIRED
Consensus = NOT REACHED
Implementation = BLOCKED
F1 = COMPLETE
F2 = BLOCKED
F3 = NOT OPEN
```
