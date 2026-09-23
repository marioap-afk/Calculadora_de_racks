# I-52 — Proposal V27: exact event catalog y host→kind gates

> **PROPOSAL V27 — PUBLISHED / REVIEW REQUIRED. Design/research only.**
>
> ```text
> HOSTFIXTURECONTRACT = NOT CLOSED
> CT-DA RESEARCH CONTRACT = NOT READY
> PROPOSAL V28 REQUIRED
> TECHNICAL CONSENSUS = NOT REACHED
> V18 = GOVERNING
> G3 = STOPPED
> SUBSTANTIVE IMPLEMENTATION = BLOCKED
> ```

## 1. Estado e identidades

| Objeto | Identidad |
|---|---|
| Starting I-52 / V26 | `f4ecad56aabaa17b273431594994db9fe66145c4` |
| Proposal V26 blob | `d92cfa693119f9ee6c407dd0a6b2d08d1fb7a766` |
| HF-V26-1 blob | `f016b7f30bb4f552ff2cda493fe9e69f938f2f85` |
| package V26 blob | `7ceef423564ac4d02752ddb8208d6dcf8f252ce0` |
| decisions V26 blob | `3b87e671660aaf8bc60c780b2011300fb7d1ef8a` |
| main | `7097057cf8685bf5ecc09083cba37379d4a4aae8` |
| I-55 | closure `08e45e0a2521a2e7feb4cf7c8d3fc7fc574aa74d`; product `991d1694c39377fe7f03d67b024b5a9fcc8dd093` |

## 2. Review V26 registrado

Architect requirio V27 con un BLOCKER por event classes/probes no exactos y un HIGH por residuos host→kind no incluidos
formalmente en `KindSpecificCoveragePass(k)`. ALT-21C sigue viable y V18 gobierna.

## 3. Delta V27

V27 publica:

- [`I-52-host-event-catalog-v27.md`](I-52-host-event-catalog-v27.md), HEC-V27-1;
- [`I-52-host-fixture-contract-v27.md`](I-52-host-fixture-contract-v27.md), HF-V27-1;
- managed EventIds exact-build, scheduler catalog y probes 01..16;
- native callback candidates y probes 02N/04N/09N/10N/16N;
- composition rule;
- `HostToKindCompatibilityPass(k)` y ResidualGate por DA-P.

Preserva fixture core HF-V26-1, scratch, S/M/SM, expected authority, resources, verifiers, DA-P intent y autoridades.

## 4. Resultado de discovery exacto

Las DLL managed instaladas `25.0.0.0`, file `25.0.171.0.0`, permitieron cerrar nombres, firmas y schedulers managed por
reflection. Se verificaron Database object events; Document command/modeless events; DocumentCollection lock events;
Application.Idle; `SendStringToExecute`; y `ExecuteInCommandContextAsync`.

No se encontro SDK/header ObjectARX 2025 local. Los contratos publicos Autodesk identifican candidatos exactos por nombre,
pero no acreditan su presencia/firma para el toolset exacto instalado. Tampoco se identifico scheduler nativo exacto para
16N-S/M/SM. Conforme a no-default, NativeProbeCatalog y EventCatalog permanecen abiertos.

## 5. Managed event/probe closure

HEC-V27-1 elimina `Database/Document/Application events` como wildcard. Cada evento IN tiene EventId, API/event name,
publisher, handler, trigger, fase, T/lock a medir, target, probe, DA-P/threat y oraculo.

Probes managed 01..16 quedan ligados a EventIds/schedulers exactos. Probe 09 se separa en object-close-associated `09C`
y command-ended `09E`; ninguno se denomina commit-phase.

## 6. Native catalog/probes

Se nombran como candidatos `AcDbDatabaseReactor`, `AcTransactionReactor` y `AcEditorReactor` con callbacks concretos.
02N, 04N, 09N-A/B/C/D y 10N-S/M/SM tienen setup, trigger, T, mutation y oracle. Sin exact header, resultado contractual
es UNKNOWN.

16N-S/M/SM queda `BLOCKED_UNDEFINED`: notificacion ObjectARX no equivale a scheduler. Managed emulation no cuenta.
Esta dependencia impide declarar HF-V27-1 CLOSED.

## 7. Composiciones

Probe compuesto es obligatorio sólo cuando scheduling/trigger compartido o T/lock/lifecycle puede producir un estado no
implicado por las pruebas independientes. T2+T4 usa 09N-B si es el mismo callback exacto; T2+T16 queda UNKNOWN;
T15+T16 usa modulo controlado + scheduler managed; T3+T8 y T13+T16 se componen bajo las condiciones publicadas.

## 8. Predicados de catalogo y fixture

```text
ManagedProbeCatalogClosed = TRUE
NativeProbeCatalogClosed = FALSE
EventCatalogClosed = FALSE
HostFixtureContractClosed(HF-V27-1) = FALSE
CTDA_HOST_PASS = NOT EVALUATED / NOT EXECUTABLE YET
```

Un probe definido que falle no reabre el contrato. Una callback o scheduler requerido sin autoridad exacta si lo mantiene
abierto.

## 9. Host→KindCompatibilityPass

El nuevo predicate exige PASS de surface writes, transaction strategy, storage/reader paths, semantic/physical scans,
event/scheduler classes, primitive set, lifecycle, locks y source ordering. Diferencia o UNKNOWN exige evidencia kind.

`KindSpecificCoveragePass(k)` ahora requiere DA-P7, DA-P8, DA-P10, `HostToKindCompatibilityPass(k)` y todos los probes
product-specific. `CTDA_PASS(k)` conserva host pass + closed KindContract + kind coverage + kind fixtures + tuple.

## 10. Residual gates DA-P1..12

HF-V27-1 publica una fila para cada DA-P. Ningun PARTIAL queda sin ResidualGate obligatorio. PARTIAL sólo es legal cuando
la subpropiedad host esta completa, el residuo tiene nombre y `CTDA_PASS(k)` depende de su PASS; de otro modo es UNKNOWN.

## 11. I-55 y authorities

I-55 unchanged: host event catalog `NON-MATERIAL`; HostToKind y KindContract future coordination `MATERIAL`;
reconciliation `REQUIRED`; evidence transfer `NONE`. No se hereda partial placement.

Foundation sigue por referencia. I-49 conserva root causes completos por variable leida. AUTH-15 sigue no implementada.
Selective y todos los demas kinds permanecen OPEN.

## 12. Disposiciones y proceso

V26 queda historica y V27 la supera sólo en el delta declarado. V25/V24/V20 conservan sus autoridades. V18, Freeze V18,
O-1 V18 y ADR-0036 permanecen.

Secuencia: V27 reviews → V28 para exact native authority → reviews/consenso → LIMITED CT-DA authorization → harness
research → ejecucion/review → architecture decision → Freeze → Owner → ADR → G3 explicito. V27 no autoriza ejecucion.

## 13. Autocritica

1. Evento host descrito sólo por categoria: **NO para managed; native queda catalogado candidato pero exact-build UNKNOWN**.
2. Native probe sin callback/trigger/mutation/oracle: **16N no tiene scheduler exacto; por ello contract NOT CLOSED**.
3. Callback inexistente tratado covered: **NO**.
4. Managed emulation satisface T2: **NO**.
5. PARTIAL sin ResidualGate: **NO**.
6. Transaction strategy distinta hereda: **NO**.
7. Reader path distinto hereda DA-P4: **NO**.
8. Scheduler/event unrepresented hereda DA-P12: **NO**.
9. CTDA_HOST_PASS admite kind: **NO**.
10. V27 autoriza CT-DA: **NO**.

## 14. Resultado

```text
PROPOSAL V27 = PUBLISHED / REVIEW REQUIRED
HOSTFIXTURECONTRACT = NOT CLOSED
CT-DA RESEARCH CONTRACT = NOT READY
PROPOSAL V28 REQUIRED
TECHNICAL CONSENSUS = NOT REACHED
PRODUCT KINDCONTRACTS = STILL OPEN
V18 = GOVERNING
G3 = STOPPED
G3B = NOT OPEN
CT-50 = NOT EXECUTED
SUBSTANTIVE IMPLEMENTATION = BLOCKED
```
