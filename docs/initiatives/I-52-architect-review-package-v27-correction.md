# I-52 — Architect review package: Proposal V27 Correction / HEC-V27-C1

Revisa el exact SHA publicado. No modifiques archivos ni ejecutes CT-DA.

## Objetos

- `docs/initiatives/I-52-proposal-v27-correction.md`
- `docs/initiatives/I-52-host-event-catalog-v27-correction.md`
- `docs/automation/decisions/I-52.md` §§137–138
- este paquete

Parent exacto:

```text
V27 SHA = 4299fe52b82c4e66131c519ec274be31da8b9295
Proposal V27 blob = c42e8f42024ea5947413d68b9c8dba8c3f025860
HF-V27-1 blob = 6ccd1b2b170fbaa7704d57c1cc26df0054dfc74a
HEC-V27-1 blob = 717188aca5e11be9ddfb886f602dcfc47171732c
Package V27 blob = 0f4e036b71d2520d999872ad7bcca7d94036edb6
Decisions V27 blob = 73bf263dc1a87761739a918a94bf85b6fd90329c
```

## Estado propuesto

```text
ManagedProbeCatalogClosed = TRUE
NativeProbeCatalogClosed = FALSE
EventCatalogClosed = FALSE
HostFixtureContractClosed = FALSE
CT-DA RESEARCH CONTRACT = NOT READY
V28 = BLOCKED UNTIL CORRECTION CONSENSUS
```

## Intenta refutar

1. Busca asteriscos, ranges o IDs comprimidos en toda celda normativa.
2. Confirma una sola autoridad primaria por cada ProbeId.
3. Reconstruye EventId→Probe y SchedulerId→Probe desde la matriz primaria; debe coincidir.
4. Comprueba 01=M-DB-MOD y 02=M-DB-OPEN sin segunda autoridad primaria.
5. Comprueba que 10S, 10M y 10SM usan M-DB-MOD y que M-DOC-END es marker.
6. Comprueba cada una de las nueve variantes 16 por separado.
7. Revisa MD-RYOW, MD-ABORT y MD-COMMIT: son direct actions, no eventos inventados.
8. Revisa probe 13 con MS-CONTEXT y sin EventId artificial.
9. Revisa ML-CONTROLLED: load registra; M-DB-MOD dispara la mutacion de probe 15.
10. Refleja `Autodesk.AutoCAD.ApplicationServices.Core.Application.Idle` en AcCoreMgd 25.0.0.0.
11. Comprueba registro one-shot, delegate identity, finally cleanup y no timeout oracle.
12. Confirma que CommandEnded no se promueve a C_host.
13. Confirma que NativeProbeCatalog y T2+T16 siguen OPEN/UNKNOWN.
14. Confirma que HostToKind, PARTIAL y formulas kind/product no cambian.
15. Confirma Selective y todos los kinds OPEN.
16. Confirma que la correccion no autoriza V28, harness, helper, CT-DA, AUTH-15 ni producto.

## Veredicto permitido

Termina exactamente con uno:

```text
Architect: AGREED WITH PROPOSAL V27 CORRECTION — MANAGED PROBE CATALOG CLOSED
```

o:

```text
Architect: CHANGES REQUIRED — ANOTHER V27 CORRECTION
```

o:

```text
Architect: ALT-21C REJECTED — V18 REMAINS GOVERNING
```

Y siempre:

```text
NATIVEPROBECATALOG = OPEN
EVENTCATALOG = OPEN
HOSTFIXTURECONTRACT = NOT CLOSED
CT-DA RESEARCH CONTRACT = NOT READY
V28 = BLOCKED UNTIL CORRECTION CONSENSUS
PRODUCT KINDCONTRACTS = STILL OPEN
V18 = GOVERNING
G3 = STOPPED
G3B = NOT OPEN
CT-50 = NOT EXECUTED
SUBSTANTIVE IMPLEMENTATION = BLOCKED
```
