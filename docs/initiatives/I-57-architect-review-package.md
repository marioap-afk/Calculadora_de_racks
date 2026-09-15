# I-57 — Paquete de revision del Architect — Proposal V1

## Objeto exacto

```text
Branch        = architecture/shared-view-foundation
BASE_SHA      = dad4e77f4f267b9fa248ecb0c8bfd8a74bbab093
DISCOVERY_SHA = a3341068137931203de00fc769e4675db7b7d3a8
PROPOSAL_SHA  = 595d8db283cc88e035b33e84cb8c9fcffa51c966
Proposal blob = 51a91413239e8d1c5350a62a77ec7ae009514daa
Map blob      = 22acbb94c4c33b583753acd704ef812208d4eb1c
R2 blob       = 97b4e6952f62344a16ea33dc7492aca3f7951772
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
Reviewed SHA = 595d8db283cc88e035b33e84cb8c9fcffa51c966
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
