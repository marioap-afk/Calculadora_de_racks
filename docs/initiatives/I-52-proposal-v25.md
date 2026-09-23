# I-52 — Proposal V25: Closed KindContract Matrix

> **PROPOSAL V25 — PUBLISHED / REVIEW REQUIRED. Diseño; no implementación.**
>
> ```text
> KINDCONTRACT = NOT CLOSED
> CT-DA RESEARCH CONTRACT = NOT READY
> PROPOSAL V26 REQUIRED
> V20 architecture = PRESERVED / RESTRICTED
> V24 = SUPERSEDED ONLY BY THE V25 DELTA
> V18 = GOVERNING
> Technical Consensus = NOT REACHED
> G3 = STOPPED
> G3B = NOT OPEN
> CT-50 = NOT EXECUTED
> SUBSTANTIVE IMPLEMENTATION = BLOCKED
> ```

V25 publica por primera vez el esquema normativo, el censo fuente y la matriz por kind que V24 dejaba implícitos. La
matriz hace visible un resultado adverso: ninguna kind, incluida Selective, posee todavía todas las policies de recursos
y pruebas de completitud necesarias para servir de fixture CT-DA sin defaults. V25 no fuerza READY; exige V26.

## 1. Estado e identidades

| Objeto | Identidad |
|---|---|
| I-52 inicial / Proposal V24 | `1be4852b9967412f5f187962887a919470b020e5` |
| Proposal V24 blob | `517eb0793758a033b8b294168c1d912235b37b1a` |
| package V24 blob | `b72bf452b637b534199589eb2f14633fccd91520` |
| decisions V24 blob | `bf97ca769e43afd555da35d3292e643af254ca83` |
| `origin/main` / source baseline | `7097057cf8685bf5ecc09083cba37379d4a4aae8` |
| I-57 | object `a5bc02210f740839e2fac37e63fc00512e5ccee6`; target main |
| I-55 | closure `08e45e0a2521a2e7feb4cf7c8d3fc7fc574aa74d`; product `991d1694c39377fe7f03d67b024b5a9fcc8dd093` |

Preflight: rama/upstream exactos, árbol limpio, stash vacío y ninguna operación Git incompleta.

## 2. Blocker Architect V24

```text
Architect = CHANGES REQUIRED — PROPOSAL V25
BLOCKER = KindContract does not contain a closed per-kind matrix
Reason = classification and resource policy cannot remain conditional or implicit.
         Missing / conditional / uncensused entries can create nominal CTDA_PASS.
V18 = GOVERNING
```

## 3. Delta y artefacto normativo

[`I-52-kind-contract-v25.md`](I-52-kind-contract-v25.md) es parte normativa de V25. Contiene versionado, matriz,
consumer census, resource ownership, expected authority, verifier/fixture/DA-P mapping, statuses e invalidadores.

V25 cambia sólo KindContract closure, consumer census, resource matrix, expected authority, no-default y scope
`CTDA_PASS(k)`. Conserva íntegros `C_decide/C_report/C_host/F_usable`, DA-P1..12, T16, snapshots, tres capas, proceso y
research-only de V24.

## 4. No-default y clasificación formal

Toda entrada missing, conditional sin resolver, uncensused o sin ownership/value/verifier/fixture/DA-P es UNKNOWN.
UNKNOWN implica kind OPEN, no admisible y `CTDA_PASS(k)=false`. No existe default a M/USER/NA.

```text
SurfaceCanBeM(surface,k) iff
  ConsumerCensusComplete(surface,k)
  AND NoSupportedSemanticConsumer(surface,k)
  AND PhysicalContractOwns(surface,k)
```

Cada término UNKNOWN hace classification UNKNOWN. Una matriz CLOSED no contiene `M or SM`.

## 5. Censo semántico

El censo parte de DTO/envelope/storage y recorre command/router roots hacia editor/Actualizar, BOM/RACKLISTA,
Insertar, sibling grouping, persistence/save-reopen, variables, materialization planning, naming/identity,
duplication/layout y future-view generation. Clasifica lecturas reales, no nombres.

La completitud exige miembros declarados, roots soportados, call graph alcanzable y sitios metadata/dynamic/resource
sin `UNCLASSIFIED`. La matriz registra CLOSED sólo donde esa evidencia alcanza; el resto queda OPEN.

## 6. Resultado Selective

La autoridad authored queda conservadoramente S; sibling, grouping y binding son SM. Las layers explícitas
`RACKCAD_ANOTACIONES` y `RACKCAD_COTAS` tienen expected authority independiente en la matriz. Sin embargo siguen OPEN:

1. cierre del censo de transform/rotation/scale frente a todos los command roots;
2. aliases dinámicos derivados de catálogo/biblioteca externa;
3. layer/linetype/color/lineweight de outer y nested references;
4. text style y demás assignments de DBText;
5. fallback y estabilidad final de dimension style.

Por no-default:

```text
ConsumerCensusComplete(Selective) = FALSE
KindContractClosed(Selective) = FALSE
Selective CT-DA fixture = NOT READY
```

No se declara falsamente `USER_CONTROLLED` lo que hoy sólo depende de defaults AutoCAD.

## 7. Otros kinds

Dynamic, PushBack, Cantilever, Header y Flow Bed permanecen `OPEN / NOT ADMISSIBLE`. Sus comparators, consumers,
resources y physical units no se heredan de Selective. Foundation incompleta para un kind mantiene su matriz OPEN.

## 8. Host frente a kind y scope CT-DA

```text
HostInvariantPass = DA-P1..12 PASS + lifecycle/transaction/T1..T16 + exact tuple
KindContractPass(k) = KindContractClosed(k) + kind fixtures PASS
CTDA_PASS(k) = HostInvariantPass AND KindContractPass(k)
```

Host evidence puede reutilizarse únicamente con el mismo scope exacto. `CTDA_PASS_GLOBAL` no se inventa. CT-DA-HOST
puede comenzar con una kind fixture cerrada sin reducir first-cut, pero V25 no encontró ninguna. Por ello research
authorization no está lista. Product admission futuro exige `CTDA_PASS(k)` para cada kind admitida; scope reduction
requiere decisión separada.

## 9. Fixtures, DA-P y UNKNOWN

Fixtures se derivan por fila: S→10S/16S; M→10M/16M/M-F; SM→10SM/16SM/M-F, con DA-P6/11/12 según V24. Falta de
fixture o verifier deja UNKNOWN.

Ejemplos normativos: dynamic consumer no censado, layer sin expected authority, metadata ambigua, fixture/verifier
ausente o resource cell omitida producen UNKNOWN; nunca M/USER/NA.

## 10. Versioning e invalidación

La matriz captura proposal/source/Foundation/I-55/schema/census. Consumidor, DTO, metadata, resource policy, unit,
path I-55/Foundation, schema, comparator, verifier o materializer nuevos invalidan filas afectadas y exigen re-censo.

## 11. I-55

I-55 `991d1694` agrega ID18 productivo; `08e45e0a` lo cierra documentalmente. Impacto host CT-DA `NON-MATERIAL`;
coordinación y KindContract census `MATERIAL / OBSERVED`; integración futura invalida identity/address/grouping/
preparation/placement. Partial placement I-55 no sustituye atomicidad I-52. No se transfiere evidencia.

## 12. Foundation, I-49 y AUTH-15

Foundation AUTH-01..13 se consume por referencia. Variable bindings conservan I-49 por cada variable realmente leída:
`SymbolId + complete RootCauses(variable)`, sin unión global. AUTH-15 sigue `I-52 OWNED / NOT IMPLEMENTED`.

## 13. Disposiciones y proceso

V24 queda superada sólo en KindContract/census/resources/expected authority/no-default/scope por kind. V20 sigue
`PRESERVED / RESTRICTED`; V21–V24 son históricos con contratos no reemplazados incorporados.

```text
V18 = GOVERNING
Freeze V18 = GOVERNING
O-1 V18 = GOVERNING
ADR-0036 = PROPOSED / AMENDED FOR V18
```

Proceso: V25 reviews → V26 required → sólo un contrato READY puede alcanzar consenso/autorización limitada CT-DA →
ejecución/revisión → decisión arquitectónica → Freeze → Owner → ADR → reapertura explícita G3. V25 no abre CT-DA.

## 14. Autocrítica

- missing entry default M: **NO**.
- uncensused consumer significa no consumer: **NO**.
- resource policy missing default USER/NA: **NO**.
- REQUIRED sin expected authority independiente: **NO; queda UNKNOWN**.
- `M or SM` en CLOSED: **NO**.
- CTDA_PASS con KindContract OPEN: **NO**.
- Selective como fixture sin reducir first-cut: **YES conceptualmente; V25 aún no lo cierra**.
- partial-batch I-55 reemplaza atomicidad I-52: **NO**.
- cambio material invalida filas: **YES**.
- V25 autoriza implementación: **NO**.

## 15. Invalidadores y siguiente delta

V26 debe resolver todas las filas necesarias para un fixture host cerrado o demostrar que no puede hacerlo. No basta
renombrar UNKNOWN como default ni limitar fixtures manualmente. Si ningún fixture puede cerrar sin aislamiento
universal, ALT-21C debe reconsiderarse.

```text
PROPOSAL V25 = PUBLISHED / REVIEW REQUIRED
KINDCONTRACT = NOT CLOSED
CT-DA RESEARCH CONTRACT = NOT READY
PROPOSAL V26 REQUIRED
TECHNICAL CONSENSUS = NOT REACHED
V18 = GOVERNING
G3 = STOPPED
G3B = NOT OPEN
CT-50 = NOT EXECUTED
SUBSTANTIVE IMPLEMENTATION = BLOCKED
```
