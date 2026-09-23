# I-52 — Proposal V26: Host Fixture Contract

> **PROPOSAL V26 — PUBLISHED / REVIEW REQUIRED. Design/research contract only.**
>
> ```text
> HOSTFIXTURECONTRACT = CLOSED
> CT-DA-HOST RESEARCH CONTRACT = READY FOR REVIEW
> CTDA_HOST_PASS = NOT EVALUATED
> PRODUCT KINDCONTRACTS = STILL OPEN
> ALT-21C = CANDIDATE / NOT ADMITTED
> TECHNICAL CONSENSUS = NOT REACHED
> V18 = GOVERNING
> G3 = STOPPED
> SUBSTANTIVE IMPLEMENTATION = BLOCKED
> ```

V26 separa el contrato de un fixture host controlado de la admisión de producto. Cerrar el contrato significa que la
investigación futura tiene scope, primitives, threats, probes, oráculos y fallos definidos. No significa que AutoCAD
haya pasado la caracterización ni que un kind esté listo.

## 1. Estado e identidades

| Objeto | Identidad |
|---|---|
| I-52 / Proposal V25 inicial | `58c6e02dca5bdbf62546fdc49b32a074e8d2204c` |
| Proposal V25 blob | `014470bea99451c8dc499bdb54653fba404e4a36` |
| KindContract V25 blob | `da28173a97517075f64e7ac868a886e38ff14cff` |
| package V25 blob | `9bb91a8a1769547e6c0edd83908736da9ee290a7` |
| decisions V25 blob | `924d1e1e59f3d6e958f4dfc5d4cee395985ba834` |
| origin/main | `7097057cf8685bf5ecc09083cba37379d4a4aae8` |
| I-55 | closure `08e45e0a2521a2e7feb4cf7c8d3fc7fc574aa74d`; product `991d1694c39377fe7f03d67b024b5a9fcc8dd093` |
| I-57 | object `a5bc02210f740839e2fac37e63fc00512e5ccee6`; target main |

Preflight: rama/upstream exactos, árbol limpio, stash vacío, sin operación Git incompleta.

## 2. Consenso V25

Coordinator y Architect acordaron exactamente:

```text
AGREED WITH PROPOSAL V25 —
KINDCONTRACT REMAINS OPEN / V26 REQUIRED
```

Por tanto `TECHNICAL CONSENSUS V25=REACHED`, pero KindContract sigue abierto, CT-DA no estaba lista ni autorizada,
Proposal V26 era requerida y ALT-21C continúa candidata/no admitida.

## 3. Delta V26

V26 modifica sólo el requisito que acoplaba CT-DA-HOST a un KindContract completo y el modelo de research readiness.
Publica [`I-52-host-fixture-contract-v26.md`](I-52-host-fixture-contract-v26.md) como contrato normativo HF-V26-1.

No modifica tres capas, S/R/M, OverallSuccess, DA-P, T1..T16, C points, V25 product KindContract, proceso final ni
garantías de producto.

## 4. Tres capas de admisión

```text
A. HostFixtureContractClosed(f)  // diseño del instrumento
B. CTDA_HOST_PASS(f)             // resultado host aún no ejecutado
C. CTDA_PASS(k)                  // admisión de producto por kind
```

Una capa no sustituye a la siguiente. `HostFixtureContractClosed(HF-V26-1)=true` sólo hace revisable y ejecutable la
investigación después de consenso y autorización limitada.

## 5. Fixture HF-V26-1

El fixture vive en un DWG scratch y usa primitivas reales: Database/Document, DocumentLock, TransactionManager,
Transaction, ObjectId, BlockTableRecord, BlockReference, extension dictionary, Xrecord/ResultBuffer, NOD/DBDictionary,
LayerId, managed events, helper native ObjectARX y lifecycle de comando.

Incluye fuente, dos hermanas, payloads semánticos, identidad/address de prueba, transforms, binding, layer y NOD. Todos
los expected values son constantes read-only separadas del writer y los verifiers vuelven a leer DB.

No usa catálogos, biblioteca externa, current defaults ni DTO/policies de kind.

## 6. S/M/SM y recursos

- S: source/destination Xrecords, fixture identity/address y NOD value.
- M: transform, LayerId y definition binding puramente instrumentales.
- SM: sibling membership y asociación payload/reference.

Layer y definiciones tienen nombres fijos. Linetype/color/lineweight/styles son explícitamente N/A al scope reclamado;
ningún verifier u outcome depende de ellos. No hay UNKNOWN en el contrato del fixture.

## 7. Representatividad y fronteras

HF-V26-1 representa directamente afiliación/commit/nesting/RYOW, scans semánticos/físicos, fences, NOD host,
source ordering y success lifecycle para las primitives/event classes ejecutadas.

Se separan obligatoriamente:

```text
DA-P7_HOST_OBSERVABILITY(f) != DA-P7_KIND_COVERAGE(k)
DA-P8_HOST_NOD(f)           != DA-P8_KIND_APPLICABILITY(k)
DA-P10_HOST_ORDERING(f)     != DA-P10_KIND_READSET(k)
```

Dynamic properties, dimension internals y primitives no enumeradas no se heredan. Toda event/primitive productiva
fuera de HF-V26-1 requiere evidencia por kind.

## 8. DA-P1..12

| DA-P | Fixture | Resultado que podría producir | Residuo de producto |
|---|---|---|---|
| 1 | Xrecord writes + callbacks dentro de T | transaction affiliation | surfaces del kind |
| 2 | semantic/material commit + callbacks | commit observability | primitive extra |
| 3 | nested/new T + abort/commit | containment | estrategia productiva compatible |
| 4 | staged Xrecord/dictionary read | RYOW host | reader path equivalente |
| 5 | scan A/B con interleaving | snapshot semantics | DTO shape no transferida |
| 6 | 10S/16S | semantic lifecycle fence | event classes extra |
| 7 | physical fixture verifier | host observability only | full kind coverage |
| 8 | NOD fixture | NOD host semantics | applicability/readers por kind |
| 9 | physical A/B scan | physical snapshot semantics | primitive extra |
| 10 | controlled source after S | source ordering | complete I-49/readset |
| 11 | 10M/SM + 16M/SM | material lifecycle for included primitives | fields/primitives por kind |
| 12 | full command lifecycle | usable success boundary | scheduler/event extra |

## 9. Threat coverage y native reactor

El contrato mapea T1..T16 a mechanisms, probes y properties. T2 requiere evidencia documental exacta más probe o un
helper ObjectARX de research con hash. Un handler managed no acredita T2. Si el helper no puede producirse y docs no
bastan, T2=UNKNOWN y `CTDA_HOST_PASS=false`; el contrato sigue siendo cerrado porque el resultado adverso está definido.

No se prueba ausencia de plugins, verticales o schedulers arbitrarios. Una clase nueva o no representada deja coverage
UNKNOWN; no se llama aislamiento.

## 10. Fórmulas

```text
HostFixtureContractClosed(HF-V26-1) = TRUE as design contract
CTDA_HOST_PASS(HF-V26-1) = NOT EVALUATED

CTDA_HOST_PASS(f) =
  closed fixture
  AND every host-level obligation PASS
  AND every host-representable T1..T16 obligation PASS
  AND exact tuple/version
  AND every required managed/native/deferred probe executed

CTDA_PASS(k) =
  CTDA_HOST_PASS(HF-V26-1)
  AND KindContractClosed(k)
  AND KindSpecificCoveragePass(k)
  AND product-kind fixtures PASS
  AND compatible exact tuple
```

No existe product PASS global ni promoción entre kinds.

## 11. Lifetime, tuple, versioning e invalidación

HF-V26-1 corre sólo en scratch DWG. Registra AutoCAD/API/builds/hashes/product/vertical/registry/OS/runtime, probe SHA,
fixture blob y orden total de events/transacciones/C points. Cambiar tuple, primitive/path, T strategy, lifecycle,
harness scheduling, native helper, fixture/version, resources, verifier o probes invalida evidencia host.

Los invalidadores KindContract V25 permanecen separados.

## 12. Selective y otros kinds

```text
KindContractClosed(Selective) = FALSE
Dynamic/PushBack/Cantilever/Header/Flow Bed = OPEN / NOT ADMISSIBLE
```

V26 no cierra filas de producto. El fixture no reduce first-cut ni acredita Selective.

## 13. I-55 y autoridades compartidas

I-55 sigue `08e45e0a` sobre producto `991d1694`: impacto host fixture `NON-MATERIAL`; coordinación KindContract
`MATERIAL`; reconciliación futura `REQUIRED`; evidence transfer `NONE`. Su partial placement no reemplaza atomicidad
I-52 de una invocación/T/commit/all-or-nothing.

Foundation AUTH-01..13 sigue por referencia. I-49 permanece `SymbolId + complete RootCauses(variable)` por variable
realmente leída, sin unión global; el simple source fixture no prueba PlanReadSet. AUTH-15 sigue OWNED/NOT IMPLEMENTED.

## 14. Disposiciones

V25 permanece correcto y normativo para product KindContracts. V26 lo supera sólo en el requisito de cerrar un kind
completo antes de investigación host y en el readiness model.

V20 sigue `PRESERVED / RESTRICTED`. V24 conserva C_decide/C_report/C_host/F_usable, DA-P12, T16, S/M/SM y resources.

```text
V18 = GOVERNING
Freeze V18 = GOVERNING
O-1 V18 = GOVERNING
ADR-0036 = PROPOSED / AMENDED FOR V18
```

## 15. Proceso

```text
V26 reviews
→ technical consensus
→ LIMITED CT-DA RESEARCH AUTHORIZATION
→ implement research harness/fixture/logging only
→ execute CT-DA
→ exact evidence review
→ architecture decision
→ replacement Freeze
→ Owner
→ ADR amendment
→ explicit G3 reopening
→ later product implementation
```

V26 no ejecuta ni autoriza por sí misma CT-DA.

## 16. Autocrítica

1. CTDA-HOST sin full product KindContract: **YES, sólo con HostFixtureContractClosed=true y autorización posterior**.
2. HostFixture PASS admite Selective: **NO**.
3. Synthetic fixture prueba DA-P7 Selective completo: **NO**.
4. NOD fixture prueba applicability de todos los kinds: **NO**.
5. Fixture source prueba complete product readset: **NO**.
6. Expected values salen del writer: **NO**.
7. Primitive requerida puede omitirse: **NO; queda obligación kind o host coverage UNKNOWN**.
8. Controlled tests prueban arbitrary plugin isolation: **NO**.
9. V26 autoriza CT-DA execution: **NO**.
10. V26 autoriza RACKMIRROR: **NO**.

## 17. Resultado propuesto

```text
PROPOSAL V26 = PUBLISHED / REVIEW REQUIRED
HOSTFIXTURECONTRACT = CLOSED
CT-DA-HOST RESEARCH CONTRACT = READY FOR REVIEW
PRODUCT KINDCONTRACTS = STILL OPEN
TECHNICAL CONSENSUS = NOT REACHED
V18 = GOVERNING
G3 = STOPPED
G3B = NOT OPEN
CT-50 = NOT EXECUTED
SUBSTANTIVE IMPLEMENTATION = BLOCKED
```
