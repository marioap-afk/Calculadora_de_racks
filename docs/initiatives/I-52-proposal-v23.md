# I-52 — Proposal V23: Final Materialization Window Closure

> **PROPOSAL V23 — PUBLISHED / REVIEW REQUIRED. Diseño; no implementación.**
>
> ```text
> Selected alternative = ALT-21C / V23 FINAL-WINDOW CONTRACT
> CT-DA research contract = READY FOR REVIEW
> V20 architecture = PRESERVED / RESTRICTED
> V22 = SUPERSEDED ONLY BY THE V23 DELTA
> V18 = GOVERNING
> Coordinator = REVIEW REQUIRED
> Architect = REVIEW REQUIRED
> Technical Consensus = NOT REACHED
> G3 = STOPPED
> G3B = NOT OPEN
> CT-50 = NOT EXECUTED
> SUBSTANTIVE IMPLEMENTATION = BLOCKED
> ```

V23 corrige exclusivamente la ventana física final que V22 dejó abierta. Incorpora DA-P11, define `M_final`, separa
mutaciones semánticas y materiales hasta el mismo punto host `C_host`, actualiza `CTDA_PASS`, probes y matrices, y
registra el avance I-55. Todo contrato V22 no afectado permanece incorporado por referencia.

## 1. Estado e identidades

| Objeto | Identidad / estado |
|---|---|
| I-52 inicial / Proposal V22 | `b6498915e624c965f98a61ef1c55805c05fa552f` |
| Proposal V22 | blob `66f56f624322ec426ef00a0fd7088e40ad4c511c` |
| paquete Architect V22 | blob `6fa504b99c58bcacdfd651dfdf89af11e781e0fe` |
| decisions V22 | blob `58678a7e91160f698e8e15eec06e1cb7ee3064fb` |
| `origin/main` | `7097057cf8685bf5ecc09083cba37379d4a4aae8` |
| I-57 | tag object `a5bc02210f740839e2fac37e63fc00512e5ccee6`; target `7097057cf8685bf5ecc09083cba37379d4a4aae8` |
| I-55 re-fetch | `c96ca4ecc382c1ccc4e8603de8fbaebecb666d32` |

Preflight: rama/upstream exactos, árbol limpio, sin stash ni operación Git incompleta. SHA y blobs V23 se reportan
después del commit.

## 2. Blocker Coordinator de V22

```text
Coordinator = CHANGES REQUIRED — PROPOSAL V23
BLOCKER = CTDA_PASS can still admit materialization corruption
          after M_post and before command-completion
V18 = GOVERNING
```

DA-P6 cerraba sólo autoridad semántica; DA-P7 acreditaba cobertura del verifier y DA-P9 consistencia del snapshot M.
Ninguna exigía que el estado físico ya verificado siguiera correcto hasta completar el comando.

## 3. Delta V23

V23 modifica sólo:

1. ventana final de materialización;
2. DA-P6 para simetría semántica y nueva DA-P11 física;
3. mapping T→DA-P y clasificación S/M/SM;
4. `CTDA_PASS` y matriz con DA-P1..11;
5. probes `CT-DA-10S/10M` y fixtures M-F1..7;
6. estado/reconciliación futura I-55.

No reabre tres capas, S, R/M pre/post, fórmula `OverallSuccess`, secuencia B, tiempos Freeze/Owner/ADR ni límite
research-only.

## 4. `OverallSuccess` preservado

```text
OverallSuccess =
    SemanticMirrorCorrect
  AND PersistedSemanticCorrect
  AND MaterializationCorrect
  AND MaterializationCommitSucceeded
```

`MaterializationCorrect` significa ahora que `M_final`, no una observación histórica cualquiera, equivale a la
materialización esperada y conserva validez hasta `C_host` mediante DA-P11.

## 5. DA-P6 — ventana semántica final

```text
DA-P6 = PASS iff
after the final accepted R_post and until C_host:
  no synchronous or causally same-command mutation invalidates DurableSemanticAuthority,
  OR every such mutation is included in a later consistent R snapshot,
     that snapshot becomes the new final accepted R_post,
     and the same C_host fence is re-established after it.
```

Una mutación no observada, snapshot nuevo sin fence o completion point `UNKNOWN` produce DA-P6=`FAIL/UNKNOWN`.

## 6. DA-P11 — final materialization completion fence

```text
DA-P11 = PASS iff
after the final accepted M_post and until C_host:
  no synchronous or causally same-command mutation can invalidate
  RackCad-controlled materialization,
  OR every such mutation is included in a later consistent materialization snapshot,
     that snapshot becomes M_final,
     MaterializationCorrect is re-evaluated,
     and the same C_host fence is re-established after it.
```

DA-P11 cubre sólo estado físico controlado por RackCad durante la ventana acotada del comando. No exige inventario de
plugins, apariencia renderizada universal ni aislamiento del proceso.

## 7. Definición de `M_final`

```text
M_final =
  the last accepted consistent committed materialization snapshot
  whose validity extends through C_host under DA-P11

MaterializationCorrect =
  M_final ≡ ExpectedMaterialization(D', AcceptedPlan, SupportedKindContract)
```

`M_final = M_post` sólo si DA-P11 prueba que ninguna mutación invalidante puede ocurrir después. Si hay M_post1,
mutación y M_post2, M_post2 puede ser final únicamente si vuelve a pasar comparator y fence.

No existe loop infinito de relecturas: snapshots repetidos no producen PASS por estabilización empírica. CT-DA debe
demostrar un punto/event phase finito después del cual el host no ejecuta mutadores same-command antes de `C_host`.
Sin ese fence, DA-P11=`UNKNOWN` y ALT-21C no es admisible.

## 8. Punto exacto de command-completion

`C_host` es el punto contractual/observado exacto que CT-DA debe elegir y acreditar entre: managed method about-to-
return, method return, command-stack completion, `CommandEnded`, document-lock release y callbacks asociados.

El registro CT-DA debe publicar orden total de esos eventos y declarar cuál termina la ventana de éxito. Un callback
causado por el comando y ejecutado antes de `C_host` pertenece a la ventana. Un evento posterior sólo cuenta como
edición futura si el host demuestra que el comando ya completó. «Cuando termina el comando» sin ese orden es UNKNOWN.
DA-P6 y DA-P11 usan el mismo `C_host`.

## 9. Threat model actualizado

Se conserva T1..T15 de V22. T1..T8, T12, T13 y T15 se mapean a DA-P11 cuando pueden cambiar materialización en la
ventana final. T8 representa explícitamente callbacks después de la última verificación y antes de `C_host`; T12 es
mutación material-only; T15 hereda la clase del mutador que registra/ejecuta. T14 sin mutador permanece OUT.

## 10. Clasificación S/M/SM

| Clase | Definición | Cierre final |
|---|---|---|
| S | cambia sólo autoridad authored durable | DA-P6 |
| M | cambia sólo estado físico controlado por RackCad | DA-P11 |
| SM | cambia ambos | DA-P6 AND DA-P11 |

Payload sano no oculta corrupción física; vista correcta no oculta payload corrupto.

## 11. Matriz amenaza → DA-P

| Threat | S | M | DA-P primaria | DA-P secundaria | Probes | FAIL/UNKNOWN |
|---|---:|---:|---|---|---|---|
| T1 managed same-context | sí | sí | P1, P6, P11 | P5, P9 | 01,02,10S,10M | inadmisible |
| T2 reactor nativo | sí | sí | P1, P6, P11 | P2, P3 | 06,09,10S,10M | inadmisible |
| T3 secondary/nested T | sí | sí | P3 | P6, P11 | 03,04,05,13 | inadmisible |
| T4 callback de commit | sí | sí | P2 | P6, P11 | 05,09 | inadmisible |
| T5 preread→commit | sí | sí | P1, P2 | P5, P9 | 01,05,09 | inadmisible |
| T6 commit→postread | sí | sí | P2 | P5, P9 | 09,10S,10M | inadmisible |
| T7 durante scan | sí | sí | P5, P9 | P6, P11 | 08,12 | inadmisible |
| T8 postverification→C_host | sí | sí | P6, P11 | P2 | 10S,10M | inadmisible |
| T9 NOD | sí | no | P8 | P1, P6 | 11 | inadmisible si aplica |
| T10 payload fuente | sí | no | P10 | P6 | 14,15 | inadmisible |
| T11 payload destino | sí | no | P1, P6 | P2, P5 | 01,09,10S,15 | inadmisible |
| T12 materialization-only | no | sí | P7, P9, P11 | P2 | 10M,12,M-F1..7 | inadmisible |
| T13 otro context/command | sí | sí | P1, P3 | P6, P11 | fixtures de conflicto + 10S/10M | inadmisible |
| T14 load sin mutador | no | no | OUT | — | inventario diagnóstico | no cambia estado |
| T15 load + mutador | según mutador | según mutador | hereda T1..T12 | P6/P11 | probe de clase heredada | inadmisible |

Todo threat IN tiene al menos una propiedad primaria y probes. Un mapping no demostrado equivale a UNKNOWN.

## 12. `CTDA_PASS` actualizado

```text
CTDA_PASS iff
    DA-P1 = PASS AND DA-P2 = PASS AND DA-P3 = PASS AND DA-P4 = PASS
AND DA-P5 = PASS AND DA-P6 = PASS AND DA-P7 = PASS AND DA-P8 = PASS
AND DA-P9 = PASS AND DA-P10 = PASS AND DA-P11 = PASS
AND every IN-scope threat has demonstrated coverage
AND exact host tuple is unchanged
AND all required adversarial fixture families executed
```

DA-P8 puede ser `PASS / NOT APPLICABLE` sólo con irrelevancia NOD demostrada. Todo `FAIL` o `UNKNOWN` produce
`CTDA_PASS=false` y ALT-21C=`NOT ADMISSIBLE`. No existe partial PASS.

## 13. Matriz de resultados

| Propiedad | PASS | FAIL | UNKNOWN | Consecuencia FAIL/UNKNOWN |
|---|---|---|---|---|
| DA-P1 | writes afiliados/prevenidos/detectados | write escapa | afiliación no probada | rechazar ALT-21C |
| DA-P2 | commit observable | mutación silenciosa | callbacks desconocidos | rechazar ALT-21C |
| DA-P3 | secondary T contenida | sobrevive/escapa | nesting desconocido | rechazar ALT-21C |
| DA-P4 | RYOW exacto | staged read falso | visibilidad desconocida | rechazar ALT-21C |
| DA-P5 | R consistente | snapshot mixto | consistencia desconocida | rechazar ALT-21C |
| DA-P6 | semántica válida hasta C_host | corrupción final S | fence semántico desconocido | rechazar ALT-21C |
| DA-P7 | verifier cubre estado | campo físico ciego | cobertura desconocida | rechazar ALT-21C |
| DA-P8 | NOD contenido/NA probado | NOD fuera de unidad | NOD desconocido | rechazar kind/ALT-21C |
| DA-P9 | M consistente | snapshot físico mixto | consistencia desconocida | rechazar ALT-21C |
| DA-P10 | fuente ordenada/detectada | mutación causal escapa | orden desconocido | rechazar ALT-21C |
| DA-P11 | M_final válida hasta C_host | corrupción después de verifier | ventana física desconocida | rechazar ALT-21C |

## 14. `CT-DA-10S`

Instala mutador semántico adversarial después del último `R_post` aceptado y antes de cada candidato `C_host`.
Modifica payload/metadata/bindings/identity sin tocar materialización; registra event, context, transaction, bytes,
orden y observación final. Acredita DA-P6, no DA-P11.

## 15. `CT-DA-10M`

Instala mutador material-only después del último `M_post` aceptado y antes de cada candidato `C_host`. Debe cambiar
al menos transform/position, dynamic property, grouping/linkage, sibling/reference o definition binding sin cambiar
payload. Registra si ocurre, transacción, supervivencia, visibilidad y orden. Acredita DA-P11, no DA-P6.

V22 CT-DA-10 queda históricamente mapeado a `10S + 10M`; no se borra ni se reinterpretan resultados aún inexistentes.

## 16. Fixtures adversariales M-F1..7

| Fixture | Mutación tras M_post |
|---|---|
| M-F1 | `BlockReference.Position`/transform |
| M-F2 | rotation/scale factor permitido por host |
| M-F3 | required dynamic property |
| M-F4 | erase de sibling/reference |
| M-F5 | retarget definition/binding si es posible |
| M-F6 | romper grouping/linkage |
| M-F7 | metadata física per-view contractual |

Cada fixture registra fuente de evento, T/current/top transaction, timing, supervivencia al commit, visibilidad en
M_post, posibilidad después de M_post, relación con `C_host` y resultado DA-P11. «No ocurrió una vez» no prueba PASS;
debe obtenerse semántica host de la clase y repetición adversarial exact-build.

## 17. Cobertura del verifier

| Superficie amenazada | Campo `MaterializationCorrect` |
|---|---|
| sibling erase/extra | ExpectedSiblingSet, NoMissing/Unexpected/OrphanViews |
| address/identity | ExpectedAddresses, ExpectedIdentity |
| position/rotation/scale/determinant | ExpectedTransforms, PositiveScaleAndDeterminantContract |
| definition/resource | ExpectedDefinitionBindings, RequiredResourcesResolved |
| dynamic property | RequiredDynamicProperties |
| grouping/linkage | ExpectedGrouping, PayloadViewLinkageValid |
| metadata física | PerViewMetadataCorrect |
| postcondición kind | SupportedKindPhysicalPostconditions |

Una propiedad física RackCad no inspeccionada debe añadirse al verifier o declararse fuera de la garantía antes de
admisión. El verifier lee DB real y no confía en plan/writer.

## 18. Frontera de overrules visuales

Un overrule tercero que sólo cambia display y no modifica DB controlada por RackCad no viola
`MaterializationCorrect` ni DA-P11, salvo que una garantía futura incluya apariencia renderizada. V23 no la incluye y
no reabre aislamiento universal de overrules.

## 19. `FAILURE_COMMITTED_CORRUPT`

Se conserva como estado observado sólo en caracterización. Si DA-P11 falla, el estado es posible pero implica: no
SUCCESS, no rollback, no compensación, Candidate inválido y ALT-21C inadmisible. V23 no diseña reparación productiva.

## 20. Política multi-rack

Una invocación, una T, un commit y todos los destinos siguen siendo la unidad. Corrupción semántica o física de una
rack entre snapshot final y `C_host` produce:

```text
whole invocation = FAILURE_COMMITTED_CORRUPT
successful destinations = NONE
Candidate = INVALID
```

No existe éxito parcial aunque otras racks sigan correctas.

## 21. Autorefutación de suficiencia

Intento A: `CTDA_PASS=true` y autoridad durable incorrecta antes de `C_host`. Requiere mutación S/SM no capturada por
snapshot/fence; contradice DA-P1/2/3/5/6/10 y coverage completa. Por definición y evidencia exigida, no puede pasar.

Intento B: `CTDA_PASS=true` y materialización RackCad incorrecta antes de `C_host`. Requiere campo ciego, snapshot
mixto o mutación posterior; contradice DA-P7/9/11 y mapping T8/T12. No puede pasar.

La conclusión depende de evidencia real. Si `C_host` no puede acotarse, un fixture no cubre la clase o el host permite
mutación después del último snapshot sin fence, la propiedad queda UNKNOWN/FAIL y V23 ordena rechazar ALT-21C; no
convierte la definición en prueba.

## 22. I-55 — clasificación fresca

I-55 avanzó de `67779989233ef190810d2c5a61e1f1e499e31983` a
`c96ca4ecc382c1ccc4e8603de8fbaebecb666d32` con cambio productivo en Plugin/UI y pruebas: múltiples primeras vistas
soportadas, `RackViewExposure.CreateFirst`, `RackViewAddress` tipada, un RackId de creación y rutas Selective, Dynamic
y Header/Cabecera.

Clasificación: impacto al contrato CT-DA `NON-MATERIAL`; coordinación `MATERIAL`; future base reconciliation
`REQUIRED`; interacción Foundation policy `MATERIAL TO FUTURE IMPLEMENTATION`, no a semántica host transaccional. La
base futura debe regenerar censo API aplicable, seam/path reconciliation, rutas de inserción/materialización y
supuestos first-view/address. I-55 no prueba snapshots, afiliación T, `C_host`, DA-P11 ni AUTH-15.

## 23. Secuencia de proceso

```text
Proposal V23
→ exact review Coordinator
→ exact review Architect
→ technical consensus
→ LIMITED CT-DA research authorization
→ CT-DA execution and exact review
→ architectural decision
→ final replacement Freeze
→ Owner decision
→ ADR amendment
→ explicit G3 reopening
→ later implementation work
```

No hay Freeze final antes de CT-DA.

## 24. Límite research-only

CT-DA futuro sigue `RESEARCH / CHARACTERIZATION ONLY`. No autoriza RACKMIRROR, AUTH-15, guardas productivas ni cambios
de comportamiento. Esta Proposal tampoco autoriza ejecutar CT-DA; requiere consenso y autorización limitada posterior.

## 25. Disposición V22

V22 queda `SUPERSEDED ONLY FOR`: ventana física final, DA-P set, threat mapping, fórmula CTDA_PASS, matriz, probes y
estado I-55. Todos sus demás contratos se incorporan por referencia.

## 26. Disposición V20/V21

`V20 architecture = PRESERVED / RESTRICTED`. V21 queda histórica; sus contratos no reemplazados llegan por V22/V23.
No se reabren alternativas previamente rechazadas.

## 27. Disposición V18 / Freeze / O-1 / ADR

```text
V18 = GOVERNING
Freeze V18 = GOVERNING
O-1 V18 = GOVERNING
ADR-0036 = PROPOSED / AMENDED FOR V18
```

No existe reemplazo productivo todavía.

## 28. Regla G3

G3 permanece STOPPED. Una futura autorización CT-DA continúa fuera de G3. Sólo `CTDA_PASS`, decisión arquitectónica,
Freeze final, Owner y ADR permiten solicitar reapertura explícita.

## 29. Invalidadores

Invalidan READY/admisión: DA-P11 FAIL/UNKNOWN; `C_host` vago; mutación M/SM después de M_final; snapshot loop sin fence;
T8/T12/T15 sin mapping/evidencia; fixture que sólo observa no-corrupción; verifier con campo físico ciego; overrule
visual usado para ampliar el contrato; partial multi-rack success; host tuple cambiado; cualquier DA-P FAIL/UNKNOWN;
o generalización I-55 como evidencia host.

## 30. Checklist y autocrítica

- [ ] Q1 CTDA_PASS con autoridad semántica incorrecta antes de completion: **NO**.
- [ ] Q2 CTDA_PASS con materialización RackCad incorrecta antes de completion: **NO**.
- [ ] Q3 DA-P11 cierra M_post→completion: **YES**, sujeto a evidencia/fence.
- [ ] Q4 amenaza material T8/T12 sin mapping: **NO**.
- [ ] Q5 M_final es estado aceptado válido hasta C_host: **YES**.
- [ ] Q6 overrule visual-only fuerza fallo: **NO**, fuera del contrato actual.
- [ ] Q7 DA-P UNKNOWN admite PASS: **NO**.
- [ ] Q8 una rack falla y otras se reportan success: **NO**.
- [ ] Q9 CT-DA autoriza implementación: **NO**.
- [ ] Q10 DA-P11 es aislamiento universal: **NO**; sólo ventana final y DB física RackCad.

Si cualquier respuesta requerida cambia, `CT-DA RESEARCH CONTRACT = NOT READY` y se exige Proposal/fallback.

```text
PROPOSAL V23 = PUBLISHED / REVIEW REQUIRED
CT-DA RESEARCH CONTRACT = READY FOR REVIEW
TECHNICAL CONSENSUS = NOT REACHED
V18 = GOVERNING
G3 = STOPPED
G3B = NOT OPEN
CT-50 = NOT EXECUTED
SUBSTANTIVE IMPLEMENTATION = BLOCKED
```
