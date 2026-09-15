# I-57 — Paquete de revision del Architect — Proposal V2

## Objeto exacto

```text
Branch        = architecture/shared-view-foundation
BASE_SHA      = dad4e77f4f267b9fa248ecb0c8bfd8a74bbab093
DISCOVERY_SHA = a3341068137931203de00fc769e4675db7b7d3a8
PROPOSAL_SHA  = 71f87db0c4ee980976c25573f217173b139bc166
Proposal blob = bfa7a0e317f6d26dc5ad98d39561bc542e8dc3c9
Map blob      = 1ba59ac559a3e21eff24465be5f1da67b7cd3a2b
R3 blob       = cd42db03becff42f98b047e61c46689c17a69670
ADR blob      = 93614be7ce0859241d6a00cb2f9940d3895eb95b
```

Revisar contra el codigo de BASE_SHA y el Discovery. La revision no implementa, acepta ADR-0044 ni abre F1.

## Ataques obligatorios

1. Capas: ningun tipo puro depende de AutoCAD/WPF y los probes/query/materializers quedan Plugin.
2. ADR-0034: Selectivo conserva una resolucion efectiva por rack dentro del handler.
3. Plan: preparation no se convierte en segundo builder ni borra la diferencia HeaderRunPlan/CantileverViewPlan.
4. Codec: una sola autoridad total; availability y consumer policy siguen separadas.
5. Frame: un span produce extremos y centro; CT-05 puede falsar origen/ejes/offsets por familia.
6. Legacy: Selectivo/Dynamic, PostIndex Push Back, planta Cantilever y Cama quedan caracterizados, no corregidos.
7. Selection/transform: COPY e identidad quedan fuera; `Transform2D` se reutiliza.
8. Blocks/names/authored: base vs colision, requirement vs query/import, comparator por kind.
9. Testabilidad: CT puras donde aplica y guardas solo en fronteras Plugin.
10. Integracion: latest Rn efectiva sin hardcode eterno; hoy R3, sin ciclo de relectura.
11. F1: solo caracterizaciones; contradiccion material fuerza STOP y Proposal V3.
12. Payload: carrier tipado/discriminado, sin geometria universal, JSON/reflection u `object` opaco.

## Formato solicitado

```text
Architect Review — I-57 Proposal V2
Reviewed SHA = 71f87db0c4ee980976c25573f217173b139bc166
GLOBAL VERDICT = AGREED | AGREED WITH CHANGES | NOT AGREED
A1..A12 = VERIFIED | DEFECT
R3 = REGISTER | CHANGES REQUIRED
ADR-0044 = READY FOR OWNER | CHANGES REQUIRED
Findings = NONE | [BLOCKER | MATERIAL | MINOR] I57-ARCH-XX — evidencia — cambio
Architect = AGREED | REVIEW REQUIRED
Consensus = NOT REACHED
Implementation = BLOCKED
F1 = NOT OPEN
```
