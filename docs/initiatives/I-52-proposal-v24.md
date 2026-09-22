# I-52 — Proposal V24: Success Decision Boundary

> **PROPOSAL V24 — PUBLISHED / REVIEW REQUIRED. Diseño; no implementación.**
>
> ```text
> Selected alternative = ALT-21C / V24 DECISION-BOUNDARY CONTRACT
> CT-DA research contract = READY FOR REVIEW
> V20 architecture = PRESERVED / RESTRICTED
> V23 = SUPERSEDED ONLY BY THE V24 DELTA
> V18 = GOVERNING
> Coordinator = REVIEW REQUIRED
> Architect = REVIEW REQUIRED
> Technical Consensus = NOT REACHED
> G3 = STOPPED
> G3B = NOT OPEN
> CT-50 = NOT EXECUTED
> SUBSTANTIVE IMPLEMENTATION = BLOCKED
> ```

V24 corrige exclusivamente los dos BLOCKER y el HIGH de la revisión Architect exacta de V23. Introduce un punto
operativo de decisión, cubre trabajo causal que cruza candidatos lifecycle, corrige S/M/SM y añade asignaciones de
recursos al verifier. Todo contrato V23 no afectado permanece incorporado por referencia.

## 1. Estado e identidades

| Objeto | Identidad / estado |
|---|---|
| I-52 inicial / Proposal V23 | `34649e5889912339515fece9ceb8d088250934ea` |
| Proposal V23 | blob `a51d14c205c34c1061909894844e91f104126a77` |
| paquete Architect V23 | blob `8ce34980eeee0db647e252e403bacafda6bfd4b8` |
| decisions V23 | blob `fda44033b99b51439a80c21fe4abf7b56a48f913` |
| `origin/main` | `7097057cf8685bf5ecc09083cba37379d4a4aae8` |
| I-57 | tag object `a5bc02210f740839e2fac37e63fc00512e5ccee6`; target `7097057cf8685bf5ecc09083cba37379d4a4aae8` |
| I-55 re-fetch | `c12aeff00268755764a4efecdb2c3778739090c6`; producto G10 `c96ca4ecc382c1ccc4e8603de8fbaebecb666d32` |

Preflight: rama y upstream exactos; árbol limpio; stash vacío; ninguna operación Git incompleta. SHA y blobs V24 se
reportan después del commit.

## 2. Hallazgos Architect de V23

```text
Architect = CHANGES REQUIRED — PROPOSAL V24
BLOCKER-1 = C_host is not bound to the irreversible SUCCESS decision/report point
BLOCKER-2 = CT-DA-10M misclassifies durable-authority changes as material-only
HIGH-1 = MaterializationCorrect lacks explicit expected resource assignments
V18 = GOVERNING
```

## 3. Delta V24

V24 modifica sólo:

1. relación `C_host`/decisión/reporte de SUCCESS y nueva DA-P12;
2. amenaza diferida T16 y probes cross-boundary;
3. regla S/M/SM, `10M`, nuevo `10SM` y clasificación M-F;
4. `ExpectedResourceAssignments`, M-F8 y matrices afectadas;
5. `CTDA_PASS` y resultados para incluir DA-P12.

No reabre V20, tres capas, `OverallSuccess`, S, R/M pre/post, DA-P1..11 salvo su relación con DA-P12, ALL-PASS,
secuencia B, research-only ni tiempos Freeze/Owner/ADR.

## 4. Puntos `C_decide`, `C_report`, `C_host` y `F_usable`

```text
C_decide = last point at which RACKMIRROR still controls whether the invocation ends as FAILURE or SUCCESS
C_report = first irreversible externally observable product success for the invocation
C_host   = exact host lifecycle boundary after which no in-scope synchronous or causally same-command work remains
F_usable = evidence available to RACKMIRROR, while it still controls the outcome, that proves DA-P6 and DA-P11
           through C_host for the exact tuple
```

`C_report` no es cualquier mensaje informativo. Incluye retorno normal cuando AutoCAD lo interpreta como éxito,
estado de comando, notificación UI contractual o mensaje que el producto declare éxito irreversible. Un log diagnóstico
sin semántica de resultado no es `C_report`.

Orden de admisión:

```text
final accepted R/M
→ F_usable established
→ C_decide
→ C_report
```

`C_host` puede coincidir con un punto alcanzado antes de `C_decide`, o ser posterior únicamente cuando una regla host
ya acreditada en `F_usable` prueba la ausencia de mutadores IN hasta ese punto. La mera expectativa de que un evento
futuro será benigno no es evidencia. Si el fence sólo puede conocerse después del retorno/éxito y no existe callback
con control del outcome, DA-P12=`FAIL/UNKNOWN`.

## 5. DA-P12 — Success Decision Boundary

```text
DA-P12 = PASS iff
  (1) a finite exact-host F_usable orders every relevant same-command semantic/material mutator
      before the accepted completion boundary;
  (2) RACKMIRROR still controls FAILURE versus SUCCESS when F_usable is established;
  (3) C_decide occurs only after final predicates and F_usable are established;
  (4) C_report occurs at or after C_decide;
  (5) no scheduled causal work crosses the accepted boundary as an unverified continuation.
```

DA-P12=`FAIL` cuando el host sólo revela corrupción o completion después de éxito irreversible. Es `UNKNOWN` cuando
no se puede ordenar alguno de esos puntos. No existe failure retroactivo.

## 6. Relación DA-P6 / DA-P11 / DA-P12

- DA-P6: autoridad semántica válida desde `R_post` final hasta `C_host`.
- DA-P11: materialización válida desde `M_final` hasta `C_host`.
- DA-P12: esas garantías están disponibles y son utilizables antes de `C_decide/C_report`.

Un `R_post/M_final` correcto sin DA-P12 no autoriza SUCCESS. Si un probe tardío descubre corrupción mientras el
comando conserva control, decide FAILURE. Si la descubre después de `C_report`, DA-P12 falla y ALT-21C es inadmisible.

## 7. T16 — trabajo causal diferido cross-boundary

T16 es trabajo agendado antes de `C_decide` o del candidato `C_host`, ejecutado después del candidato y todavía causal
al comando. Incluye sólo mecanismos presentes/aplicables en el tuple exacto: queued command, idle/deferred callback,
`SendStringToExecute`, `ExecuteInCommandContextAsync`, synchronization-context work, handler administrado diferido,
acción COM/modeless o work item host. Enumerar nombres no declara que todos estén disponibles.

Un timestamp posterior al candidato no convierte T16 en edición futura. CT-DA debe demostrar que el comando terminó y
que la acción pertenece a una operación independiente. Si demostrarlo exige inventario universal de schedulers de
extensiones arbitrarias, T16 permanece UNKNOWN y ALT-21C falla; no se reintroduce ContextIsolationAuthority por nombre.

## 8. Probes `CT-DA-16S/M/SM`

Cada probe agenda antes del candidato y fuerza/intenta ejecución después:

| Probe | Mutación | Propiedades |
|---|---|---|
| CT-DA-16S | autoridad durable, sin cambio físico cuando sea viable | DA-P6 + DA-P12 |
| CT-DA-16M | transform/posición inequívocamente físicos | DA-P11 + DA-P12 |
| CT-DA-16SM | sibling/binding/grouping u otra superficie mixta | DA-P6 + DA-P11 + DA-P12 |

Registran scheduling/execution, candidato `C_host`, `F_usable`, `C_decide`, `C_report`, method return,
`CommandEnded`, lock release, idle/queued callback, siguiente command si aplica, causalidad y DB before/after.

## 9. Regla de autoridad S/M/SM

Una superficie es semántica/authored si cualquier consumidor soportado la usa para identidad lógica o de vista,
address, grouping, binding, estado authored, persistencia, BOM, editor o materialización futura. Es `S` si sólo cambia
esa autoridad y `SM` si además cambia estado físico. `M` exige que su cambio no altere autoridad durable ni la
interpretación de ningún consumidor semántico.

Payload bytes intactos no prueban clase M. La clasificación se fija por kind y consumidor, no por mecanismo de write.

## 10. Reclasificación M-F1..8

| Fixture | Clase | Regla |
|---|---|---|
| M-F1 transform/position | M | física inequívoca bajo contrato actual |
| M-F2 rotation/scale | M o SM | M sólo si el kind la declara drawing-only |
| M-F3 dynamic property | M o SM | depende de consumidores del kind |
| M-F4 erase sibling/reference | SM | cambia sibling set/identidad y materialización |
| M-F5 definition/binding | SM | binding pertenece a autoridad y estado físico |
| M-F6 grouping/linkage | SM | grouping/linkage alimenta consumidores semánticos |
| M-F7 metadata | M o SM | M sólo con declaración physical-only del kind |
| M-F8 wrong resource assignment | M o SM | M usual; SM si el recurso participa en authored semantics |

Toda fixture SM ejecuta y acredita conjuntamente DA-P6 y DA-P11; si cruza lifecycle, también DA-P12.

## 11. `CT-DA-10M` y nuevo `CT-DA-10SM`

`CT-DA-10M` queda restringido a transform/position y, sólo tras clasificación explícita, rotation/scale u otra
superficie física. No usa sibling, grouping, binding ni metadata authored.

`CT-DA-10SM` muta después de los últimos `R_post` y `M_post` aceptados una superficie mixta antes del boundary. Ejecuta
como mínimo erase sibling/reference, grouping/linkage y binding semánticamente significativo; metadata sólo cuando el
kind la clasifica authored. Acredita DA-P6 AND DA-P11. Una mutación SM observada sólo por DA-P11 produce UNKNOWN.

`CT-DA-10S` conserva su función semántica. `10S/10M/10SM` prueban la ventana anterior al candidato; `16S/16M/16SM`
prueban scheduling que cruza el candidato. No se renumeran CT-DA-01..15 históricos.

## 12. `ExpectedResourceAssignments` y M-F8

```text
ExpectedResourceAssignments(Actual, KindContract) =
  every RackCad-controlled physical entity has each contractually owned resource assignment expected by its kind
```

El contrato por kind enumera `REQUIRED`, `USER_CONTROLLED` o `NOT_APPLICABLE` para layer/LayerId, linetype, color,
lineweight, definition binding y cualquier named resource del plan. El verifier no reclama propiedades
intencionalmente user-controlled.

M-F8 cambia una entidad a otra capa existente después de M_post o durante scan. Así
`RequiredResourcesResolved=true` pero `ExpectedResourceAssignments=false`. Si layer alimenta un consumidor semántico
del kind, M-F8 es SM y activa DA-P6+11; de otro modo es M y activa DA-P11.

## 13. `MaterializationCorrect` actualizado

```text
MaterializationCorrect(D', Plan, Actual, KindContract) =
    ExpectedSiblingSet
  AND ExpectedAddresses
  AND ExpectedIdentity
  AND ExpectedTransforms
  AND PositiveScaleAndDeterminantContract
  AND ExpectedDefinitionBindings
  AND RequiredDynamicProperties
  AND ExpectedGrouping
  AND NoOrphanViews
  AND NoMissingViews
  AND NoUnexpectedViews
  AND PayloadViewLinkageValid
  AND PerViewMetadataCorrect
  AND RequiredResourcesResolved
  AND ExpectedResourceAssignments
  AND SupportedKindPhysicalPostconditions
```

No duplica definition binding: `ExpectedDefinitionBindings` verifica referencia de definición; resource assignments
verifica recursos gráficos contractualmente propios de cada entidad.

## 14. Matriz de cobertura del verifier

| Superficie | Campo verifier | Clase | Probe | DA-P |
|---|---|---|---|---|
| transform/position | ExpectedTransforms | M | 10M,16M,M-F1 | P7,P9,P11,P12 |
| rotation/scale | ExpectedTransforms + determinant | M/SM por kind | 10M/10SM,M-F2 | P6/P7/P11 |
| dynamic property | RequiredDynamicProperties | M/SM por kind | 10M/10SM,M-F3 | P6/P7/P11 |
| sibling presence | sibling/no missing-extra-orphan | SM | 10SM,16SM,M-F4 | P5,P6,P7,P9,P11,P12 |
| definition binding | ExpectedDefinitionBindings | SM | 10SM,16SM,M-F5 | P6,P7,P11,P12 |
| grouping/linkage | ExpectedGrouping + PayloadViewLinkageValid | SM | 10SM,16SM,M-F6 | P6,P7,P11,P12 |
| metadata física/authored | PerViewMetadataCorrect | M/SM por kind | 10M/10SM,M-F7 | P6/P7/P11 |
| resource assignment | ExpectedResourceAssignments | M/SM por kind | 10M/10SM,16M/SM,M-F8 | P6/P7/P11/P12 |

Estado mutable RackCad sin fila/verifier produce DA-P7=`FAIL/UNKNOWN`.

## 15. Matriz amenaza → DA-P actualizada

| Threat | Clase posible | Primarias | Secundarias | Probes | DA-P12 | FAIL/UNKNOWN |
|---|---|---|---|---|---|---|
| T1 managed same-context | S/M/SM | P1,P6,P11 | P5,P9 | 01,02,10*,16* | sí | inadmisible |
| T2 reactor nativo | S/M/SM | P1,P2,P6,P11 | P3 | 06,09,10*,16* | sí | inadmisible |
| T3 secondary/nested T | S/M/SM | P3,P6,P11 | P2 | 03–05,13 | si cruza | inadmisible |
| T4 commit callback | S/M/SM | P2,P6,P11 | P5,P9 | 05,09,10* | sí | inadmisible |
| T5 preread→commit | S/M/SM | P1,P2 | P5,P9 | 01,05,09 | según lifecycle | inadmisible |
| T6 commit→postread | S/M/SM | P2,P5,P9 | P6,P11 | 09,10* | según lifecycle | inadmisible |
| T7 durante scan | S/M/SM | P5,P9 | P6,P11 | 08,12 | no salvo cruce | inadmisible |
| T8 postverification→C_host | S/M/SM | P6,P11 | P2 | 10S/M/SM | sí | inadmisible |
| T9 NOD | S/SM | P8,P6 | P1 | 11,10S/SM | sí si final | inadmisible si aplica |
| T10 fuente | S | P10,P6 | P1 | 14,15 | sí si final | inadmisible |
| T11 destino | S/SM | P1,P6 | P2,P5,P11 | 01,09,10S/SM | sí | inadmisible |
| T12 material-only | M | P7,P9,P11 | P2 | 10M,12,M-F1/8 | sí | inadmisible |
| T13 otro context/command | S/M/SM | P1,P3,P6,P11 | P12 | conflicto + 10*/16* | sí | inadmisible |
| T14 load sin mutador | OUT | — | — | diagnóstico | no | no cambia estado |
| T15 load + mutador | hereda | hereda T1..T13 | P6,P11 | clase heredada | sí si final | inadmisible |
| T16 deferred cross-boundary | S/M/SM | P12 | P6,P11 | 16S/M/SM | obligatoria | inadmisible |

Ningún threat IN puede quedar sin clase, propiedad y probe. `*` significa las variantes aplicables, no una sola prueba
genérica.

## 16. `CTDA_PASS` y matriz de resultados

```text
CTDA_PASS iff
    DA-P1..DA-P12 = PASS
AND every IN-scope threat has demonstrated coverage
AND exact host tuple is unchanged
AND all required adversarial fixture families are executed
```

DA-P8 conserva `PASS / NOT APPLICABLE` sólo con irrelevancia NOD demostrada. Cualquier `FAIL` o `UNKNOWN` produce
`CTDA_PASS=false` y ALT-21C=`NOT ADMISSIBLE`; no existe partial PASS.

| Propiedad | PASS | FAIL | UNKNOWN | Consecuencia FAIL/UNKNOWN |
|---|---|---|---|---|
| DA-P6 | semántica válida hasta C_host | corrupción final S/SM | fence semántico no acreditado | rechazar ALT-21C |
| DA-P11 | M_final válida hasta C_host | corrupción final M/SM | fence físico no acreditado | rechazar ALT-21C |
| DA-P12 | fence utilizable antes de SUCCESS | lifecycle resuelve tras éxito | orden decisión/reporte incierto | rechazar ALT-21C |

Las filas DA-P1..10 no afectadas permanecen incorporadas de V23/V22.

## 17. Matriz de candidatos lifecycle

| Candidato | ¿Puede callback causal correr después? | ¿Conserva control de failure? | ¿SUCCESS ya observable? | Probe | Admisión |
|---|---|---|---|---|---|
| before method return | CT-DA decide | sí, candidato | no debe | 10*,16* | sólo si fence positivo |
| method return | CT-DA decide | normalmente no; demostrar | retorno puede implicar éxito | 16* | FAIL si ya no hay control |
| command-stack completion | CT-DA decide | no asumir | posiblemente | lifecycle + 16* | UNKNOWN hasta prueba |
| CommandEnded | handlers pueden ejecutar; medir | no asumir | posiblemente | event-order + 16* | UNKNOWN hasta prueba |
| lock release | callbacks pueden ejecutar; medir | no asumir | posiblemente | lock/event + 16* | UNKNOWN hasta prueba |
| otro punto host | demostrar | demostrar | demostrar | probe específico | sin preselección |

Un candidato es válido sólo si ordena todos los mutadores IN, RackCad todavía puede basar el outcome en la evidencia,
ningún deferred causal lo cruza y la evidencia exact-host es positiva. «No ocurrió una vez» nunca acredita la fila.

## 18. Logging total futuro

CT-DA registra en orden total: method entry, scheduling, R_post, M_post, `F_usable`, candidatos `C_decide/C_report`,
method return, `CommandEnded`, lock release, idle/queued callbacks, siguiente command si hace falta, ejecución de
mutación y DB before/after. El log distingue observación, contrato documentado e inferencia.

## 19. `OverallSuccess` y fallo tardío

Se conserva la fórmula V23. Sólo puede decidirse en `C_decide` cuando todos los predicados son true, DA-P6/11 aplican
hasta el boundary, DA-P12 es PASS y ninguna propiedad requerida es UNKNOWN.

`FAILURE_COMMITTED_CORRUPT` permanece sólo caracterización: no SUCCESS, rollback ni compensación. Corrupción conocida
después de `C_report` hace DA-P12=`FAIL`, no habilita reporte tardío y rechaza ALT-21C. En multi-rack, un fallo produce
`successful destinations=NONE` y Candidate inválido.

## 20. Autorefutaciones

1. Trabajo agendado antes y ejecutado después de SUCCESS: contradice T16/DA-P12 y `16S/M/SM`; cualquier falta de
   evidencia deja UNKNOWN. No puede coexistir con CTDA_PASS.
2. Fixture SM ejecutada sólo mediante DA-P11: contradice la regla de autoridad, `10SM/16SM` y matriz; queda UNKNOWN.
3. Recurso existente pero asignación errónea: M-F8 mantiene `RequiredResourcesResolved=true` y fuerza
   `ExpectedResourceAssignments=false`; `MaterializationCorrect=false`.

Estas conclusiones son contractuales, no resultados CT-DA. Si el host no ofrece boundary finito utilizable, ALT-21C
falla; V24 no lo convierte en PASS por definición.

## 21. Frontera ContextIsolationAuthority

V24 caracteriza clases soportadas de evento, transacción y deferred work y su relación con `C_decide/C_host`. No prueba
ausencia de plugins desconocidos. Si cerrar T16 exige enumerar toda extensión o scheduler arbitrario, DA-P12 no puede
pasar y se reconsidera ALT-21C; no se presume aislamiento universal.

## 22. I-55 — clasificación fresca

I-55 `c12aeff00268755764a4efecdb2c3778739090c6` es cierre documental G10 sobre producto
`c96ca4ecc382c1ccc4e8603de8fbaebecb666d32`. Impacto CT-DA `NON-MATERIAL`; coordinación `MATERIAL / OBSERVED`;
reconciliación futura `REQUIRED`. No transfiere evidencia transaction/snapshot/DA-P12 ni AUTH-15.

## 23. Disposiciones

V23 queda `SUPERSEDED ONLY FOR`: binding C_host/SUCCESS, DA-P12, deferred cross-boundary, S/M/SM, probes 10M/10SM/16*,
resource assignments y matrices afectadas. Sus demás contratos continúan incorporados.

`V20 architecture = PRESERVED / RESTRICTED`. V21/V22/V23 son históricos con contratos no reemplazados incorporados.
No se reabren alternativas rechazadas.

```text
V18 = GOVERNING
Freeze V18 = GOVERNING
O-1 V18 = GOVERNING
ADR-0036 = PROPOSED / AMENDED FOR V18
```

## 24. Proceso y G3

```text
V24 exact reviews
→ technical consensus
→ LIMITED CT-DA research authorization
→ CT-DA execution and exact review
→ architectural decision
→ final replacement Freeze
→ Owner decision
→ ADR amendment
→ explicit G3 reopening
→ later implementation
```

CT-DA sigue research-only y esta Proposal no autoriza ejecutarlo. No hay Freeze final antes de CT-DA. G3 permanece
STOPPED.

## 25. Invalidadores y checklist

Invalidan READY: `C_decide/C_report` vagos; fence conocido sólo tras éxito; T16 sin prueba cross-boundary; SM acreditada
sólo por DA-P11; recurso mutable sin policy/verifier; candidato lifecycle aceptado por silencio; cualquier DA-P
FAIL/UNKNOWN; tuple cambiado; o generalización universal desde fixture controlada.

- [ ] Q1 SUCCESS antes de fence utilizable: **NO**.
- [ ] Q2 deferred causal muta después de SUCCESS con CTDA_PASS: **NO**.
- [ ] Q3 sibling/binding/grouping M-only: **NO**, salvo autoridad explícita por kind, que actualmente no existe.
- [ ] Q4 toda fixture SM ejecuta DA-P6 AND DA-P11: **YES**.
- [ ] Q5 asignación errónea con recurso existente pasa verifier: **NO**.
- [ ] Q6 CTDA_PASS exige DA-P12: **YES**.
- [ ] Q7 un lifecycle candidato pasa por una ausencia observada: **NO**.
- [ ] Q8 FAIL/UNKNOWN admite: **NO**.
- [ ] Q9 CT-DA autoriza implementación: **NO**.
- [ ] Q10 V24 exige aislamiento universal: **NO**.

```text
PROPOSAL V24 = PUBLISHED / REVIEW REQUIRED
CT-DA RESEARCH CONTRACT = READY FOR REVIEW
TECHNICAL CONSENSUS = NOT REACHED
V18 = GOVERNING
G3 = STOPPED
G3B = NOT OPEN
CT-50 = NOT EXECUTED
SUBSTANTIVE IMPLEMENTATION = BLOCKED
```
