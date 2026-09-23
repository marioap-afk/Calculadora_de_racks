# I-52 — Architect review package: Proposal V27 / HF-V27-1 / HEC-V27-1

Revisa el exact SHA publicado. No modifiques archivos ni ejecutes CT-DA.

## Objetos

- `docs/initiatives/I-52-proposal-v27.md`
- `docs/initiatives/I-52-host-fixture-contract-v27.md`
- `docs/initiatives/I-52-host-event-catalog-v27.md`
- `docs/automation/decisions/I-52.md` §§134–136
- este paquete

Entradas exactas:

```text
Starting I-52 / V26 = f4ecad56aabaa17b273431594994db9fe66145c4
Proposal V26 blob = d92cfa693119f9ee6c407dd0a6b2d08d1fb7a766
HF-V26-1 blob = f016b7f30bb4f552ff2cda493fe9e69f938f2f85
Package V26 blob = 7ceef423564ac4d02752ddb8208d6dcf8f252ce0
Decisions V26 blob = 3b87e671660aaf8bc60c780b2011300fb7d1ef8a
Main = 7097057cf8685bf5ecc09083cba37379d4a4aae8
I-55 = 08e45e0a2521a2e7feb4cf7c8d3fc7fc574aa74d
```

## Estado propuesto

```text
ManagedProbeCatalogClosed = TRUE
NativeProbeCatalogClosed = FALSE
EventCatalogClosed = FALSE
HostFixtureContractClosed(HF-V27-1) = FALSE
CT-DA RESEARCH CONTRACT = NOT READY
PROPOSAL V28 REQUIRED
```

## Intenta refutar

1. Verifica reflection authority: versions, hashes, exact managed events y method signatures.
2. Busca cualquier wildcard managed restante usado como evidence.
3. Refuta cada EventId managed: trigger, publisher, phase, T/lock, target y oracle.
4. Confirma que `TransactionManager` managed no se inventa como event source.
5. Revisa MS-SEND, MS-CONTEXT y MS-IDLE; no promover a schedulers no catalogados.
6. Revisa 01..16 y bindings lifecycle-sensitive.
7. Confirma que 09C/09E no se llaman commit-phase.
8. Revisa candidate native callbacks contra exact ObjectARX 2025 authority si está disponible.
9. Confirma que public docs sin exact headers no producen exact-build PASS.
10. Refuta 02N setup/trigger/T/mutation/oracle.
11. Refuta 04N abort sequence.
12. Refuta 09N-A/B/C/D phases; especial commit begin vs transaction ended.
13. Refuta 10N-S/M/SM surfaces y final-window placement.
14. Confirma que 16N-S/M/SM permanece UNKNOWN sin scheduler exacto.
15. Confirma managed emulation no satisface T2/T2+T16.
16. Revisa composition rule y T2+T4, T2+T16, T15+T16, T3+T8, T13+T16.
17. Intenta cerrar NativeProbeCatalog con una callback no verificada; debe ser imposible.
18. Revisa EventCatalogClosed y HostFixtureContractClosed fail-closed.
19. Refuta cada ResidualGate DA-P1..12.
20. Revisa `HostToKindCompatibilityPass`: T/storage/reader/scans/events/schedulers/primitives/lifecycle/locks/source.
21. Intenta `CTDA_PASS(k)=true` con cualquier compatibility UNKNOWN.
22. Confirma DA-P7/8/10 y I-49 permanecen kind-specific.
23. Confirma Selective y otros kinds OPEN.
24. Confirma I-55 classifications y no transfer atomicity/evidence.
25. Confirma V27 no autoriza helper, harness, CT-DA, AUTH-15 ni producto.

## Veredicto esperado sin nueva autoridad externa

V27 es coherente si publica honestamente resultado B y deja V28 resolver exact native SDK/scheduler. Un review puede
exigir correccion V27 si el catalogo managed, probes condicionales o gates contienen otra ruta nominal falsa.

Termina exactamente con uno:

```text
Architect: AGREED WITH PROPOSAL V27 — NATIVE EVENT AUTHORITY OPEN / V28 REQUIRED
```

o:

```text
Architect: CHANGES REQUIRED — PROPOSAL V27 CORRECTION
```

o:

```text
Architect: ALT-21C REJECTED — V18 REMAINS GOVERNING
```

Y siempre:

```text
PRODUCT KINDCONTRACTS = STILL OPEN
V18 = GOVERNING
G3 = STOPPED
G3B = NOT OPEN
CT-50 = NOT EXECUTED
SUBSTANTIVE IMPLEMENTATION = BLOCKED
```
