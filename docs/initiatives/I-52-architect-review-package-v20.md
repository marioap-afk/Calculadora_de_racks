# I-52 — Architect review package: Proposal V20 / Boundary Shift

Actúa exclusivamente como Arquitecto par de I-52 — ID16 — RACKMIRROR.

Realiza revisión exact-SHA del commit que contiene este paquete. El ejecutor debe suministrar SHA, blobs y CI; no los
infieras de otra punta.

## Objetos

- `docs/initiatives/I-52-proposal-v20.md`
- `docs/automation/decisions/I-52.md` §§121–122
- este paquete

Contrasta contra:

```text
Starting I-52 / Proposal V19 SHA = 8c5221deb7b6f2782726a2f91b4e34c7f27843b9
Proposal V19 blob = 17fb0a3eddb3461806bc91c34b397a0d16081a74
Architect package V19 blob = b504bc13b9bf054f552fbb8245f9b7bb6722f388
Decisions V19 blob = 9f64135c408365b5e35a72e2b84d192aad89e82c
Base main = 7097057cf8685bf5ecc09083cba37379d4a4aae8
Proposal V18 SHA/blob = 1e9d7e39a50e5c6322e1d32823aa9f3130a2d0ae / 827559b504ec6bc8b5f780267a40766c3fa6db8c
Freeze V18 SHA/blob = faaf709bf6401dcda91b07f41afe44f490fb916a / c12fa65f5b6203061e8d717d1ceed595c4fed1f4
ADR-0036 V18 blob = 5628947673f2afddf141c0505e92283962459443
I-55 initially observed = 705ae301f696b7f16c8ec006e2b1ab526be46489
I-55 re-fetched = fa8f6affa769f87afe795fd310fbc74c43494123
```

## Estado propuesto, no aprobado

```text
Selected architecture = RACKCAD-OWNED CLOSED SEMANTIC SNAPSHOT + TRANSACTIONAL DERIVED MATERIALIZATION
V18 affected contract = MATERIALLY SUPERSEDED IF V20 AGREED
Coordinator = REVIEW REQUIRED
Architect = REVIEW REQUIRED
Technical Consensus = NOT REACHED
G3 = STOPPED
```

## Límites

No modifiques archivos. No commit, rebase, Freeze, O-1, ADR acceptance/amendment, G3, G3B, CT-50 o implementación.
V18 sigue gobernando hasta completar toda la secuencia de reemplazo.

## Intenta refutar

1. Encuentra inputs reales de `μ_k` que procedan de geometría física, fields, XREF, overrules, custom objects o estado
   de terceros y que la Proposal haya clasificado falsamente como drawing-only.
2. Refuta que un adapter tipado por kind pueda probar un closed semantic input set sin probar ausencia de plugins.
3. Comprueba que `RackEmbedDocument.Design`, metadata y NOD sean verdaderamente autoridades suficientes para editar,
   BOM, Actualizar, insertar vista, SAVE/reopen y future projections.
4. Trata el gap actual: `BomAuthoredAuthority` compara todo authored state para Selective, pero aprueba la primera vista
   de otros kinds. Decide si la restricción de no admitirlos hasta cerrar comparator basta.
5. Refuta la afirmación de que una modificación de geometría derivada no cambia corrección semántica. Busca cualquier
   flujo productivo que vuelva a leer geometría como authored authority.
6. Revisa `RACKDUPLICAR`: hoy clona definición/geometría física. Asegura que V20 no lo use como precedente falso y
   decide si su compatibilidad puede diferirse sin romper el contrato.
7. Comprueba que payload semántico y todas las vistas pueden persistirse en una sola transacción, sin archivo externo,
   commit anidado ni authored writes en POST.
8. Refuta UNDO/SAVE/reopen: si el Xrecord o alguna vista escapa de la transacción o no se conserva, marca BLOCKER.
9. Prueba CE20-01..20, especialmente mutation del payload fuente/destino por callback y plugin load entre fases.
10. Decide si tratar corrupción externa posterior como corrupción de derived state reduce honestamente la garantía o
    sólo renombra `ContextIsolationAuthority`.
11. Revisa que el éxito materializador sea all-or-nothing durante la operación, no best-effort, y que Actualizar sólo
    sea recovery posterior cuando la autoridad permanece sana.
12. Verifica typed payload per kind; rechaza cualquier mega-DTO o JSON universal implícito.
13. Comprueba `NewRackId` único por rack/destino, view addresses, metadata, bindings y restamp.
14. Verifica `PlanReadSet`: por variable realmente leída, `SymbolId + complete RootCauses(variable)`, sin global union,
    representative chain o `RecoveryUnit`.
15. Confirma Foundation AUTH-01..13 por consumo, I-49 por referencia, AUTH-15 `I-52 OWNED / NOT IMPLEMENTED`.
16. Evalúa el delta I-55 `705ae301..fa8f6aff`: Insertar/G9b sigue siendo seam coordinable y no prueba la atomicidad
    específica de I-52 ni implementa AUTH-15.
17. Revisa los diez puntos de autocrítica. Si Q10 es sí, exige cambios.
18. Confirma disposiciones: V18/Freeze/O-1 siguen gobernando ahora; un acuerdo V20 exige nuevo Freeze, nueva decisión
    Owner y nueva enmienda ADR antes de reabrir G3.

## Severidades

Reporta `BLOCKER / HIGH / MEDIUM / LOW`, acuerdos, desacuerdos, contradicciones materiales, suficiencia del closed
world, suficiencia transaccional y si se necesita Proposal V21. No aceptes ADR-0036.

Termina exactamente con uno:

```text
Architect: AGREED WITH PROPOSAL V20 — BOUNDARY SHIFT
```

o:

```text
Architect: CHANGES REQUIRED — PROPOSAL V21
```

Y siempre:

```text
G3 = STOPPED
G3B = NOT OPEN
CT-50 = NOT EXECUTED
SUBSTANTIVE IMPLEMENTATION = BLOCKED
```
