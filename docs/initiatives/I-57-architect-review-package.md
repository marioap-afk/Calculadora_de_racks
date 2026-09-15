# I-57 — Paquete de revision del Architect — Proposal V1

## Objeto exacto

```text
Branch        = architecture/shared-view-foundation
BASE_SHA      = dad4e77f4f267b9fa248ecb0c8bfd8a74bbab093
DISCOVERY_SHA = a3341068137931203de00fc769e4675db7b7d3a8
PROPOSAL_SHA  = e7baa255bd7c2632b071c90b84130c7ca35805fe
Proposal blob = 5a54221b495f936459066713556ebdc9d4c3ef28
Map blob      = 22acbb94c4c33b583753acd704ef812208d4eb1c
R2 blob       = 5a3921ef091b01f41e926119e5f44a3c01bffb49
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
10. Integracion: I-57 antes de consumidores y R2 sin ciclo de relectura.

## Formato solicitado

```text
Architect Review — I-57 Proposal V1
Reviewed SHA = e7baa255bd7c2632b071c90b84130c7ca35805fe
GLOBAL VERDICT = AGREED | AGREED WITH CHANGES | NOT AGREED
A1..A10 = VERIFIED | DEFECT
R2 = REGISTER | CHANGES REQUIRED
ADR-0044 = READY FOR OWNER | CHANGES REQUIRED
Findings = NONE | [BLOCKER | MATERIAL | MINOR] I57-ARCH-XX — evidencia — cambio
Architect = AGREED | REVIEW REQUIRED
Consensus = NOT REACHED
Implementation = BLOCKED
F1 = NOT OPEN
```
