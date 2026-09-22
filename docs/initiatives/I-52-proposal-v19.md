# I-52 — Proposal V19: Observable Commit Consistency

> **PROPOSAL V19 — PUBLISHED / REVIEW REQUIRED. Diseño; no implementación.**
>
> ```text
> Proposal Version = V19
> Selected result = ALT-2 REJECTED
> V18 = GOVERNING
> Coordinator = REVIEW REQUIRED
> Architect = REVIEW REQUIRED
> Technical Consensus = NOT REACHED
> G3 = STOPPED
> G3B = NOT OPEN
> CT-50 = NOT EXECUTED
> SUBSTANTIVE IMPLEMENTATION = BLOCKED
> ```

V19 evalúa si `ObservableCommitConsistency` puede sustituir `ContextIsolationAuthority`. No cambia código, no
ejecuta CT-49R/CT-50, no reabre G3 y no modifica V18, su Freeze, ADR-0036, G3A o la investigación. El resultado es
negativo con la evidencia actual: read-set, revalidación, lock, postcondiciones y rollback son defensas necesarias,
pero no cierran callbacks del mismo contexto, intercepción semántica sin estado fingerprintable ni trabajo diferido
causado por el comando.

## 1. Estado e identidades

| Autoridad | Identidad / estado |
|---|---|
| I-52 inicial | `569e019d52adbc45498d1fd37716e5ed6450642d` |
| `origin/main` | `7097057cf8685bf5ecc09083cba37379d4a4aae8` |
| Proposal V18 | SHA `1e9d7e39a50e5c6322e1d32823aa9f3130a2d0ae`; blob `827559b504ec6bc8b5f780267a40766c3fa6db8c` |
| Freeze V18 | SHA `faaf709bf6401dcda91b07f41afe44f490fb916a`; blob `c12fa65f5b6203061e8d717d1ceed595c4fed1f4` |
| ADR-0036 V18 | publicación `89ae56137adac138acc522fae0717d4ef75abbe3`; blob `5628947673f2afddf141c0505e92283962459443`; `PROPOSED / AMENDED FOR V18` |
| O-1 V18 | `aae1692a6c8545d5ed1539f3b095184c10165890`; `ACCEPTED / REGISTERED`; producto `DEFERRED` |
| Investigación | SHA `569e019d52adbc45498d1fd37716e5ed6450642d`; research `d0753ce32bb64705a0fb97e0da3b23bd8e69bc13`; evidence `43aee12564b53a2eb213eec5a887d440021d4af4` |
| Resultado research | `COMPLETE / PARTIAL_ONLY`; `CT-49R = NOT READY` |
| I-57 | tag object `a5bc02210f740839e2fac37e63fc00512e5ccee6`; target `7097057cf8685bf5ecc09083cba37379d4a4aae8` |
| I-55 observado | `705ae301f696b7f16c8ec006e2b1ab526be46489`; §29 |

El SHA y los blobs propios se reportan después del commit. V19 no se autocalifica como consenso o Freeze.

## 2. Por qué reconsiderar V18

V18 exige autoridad positiva sobre todo canal capaz de afectar DB, representación, símbolos o semántica API. La
investigación confirmó que locks, inventarios, eventos y entornos controlados son parciales. V19 intenta reducir la
garantía a lo declarado, revalidado y validado dentro de una ventana atómica. Para sustituir V18 tendría que demostrar
que toda divergencia relevante queda prevenida, observable antes de commit o contenida por rollback, sin excluir por
definición causalidad diferida inmediata.

## 3. Garantía evaluada

> RACKMIRROR produce un resultado semánticamente correcto y atómico respecto del estado declarado, observado y
> revalidado en su ventana de commit.

Se sacrificaría ausencia universal de contextos/callbacks/extensiones e invariancia futura. Se intentaría garantizar
read-set completo, revalidación previa, exclusión de writers incompatibles, postcondiciones independientes y rollback.
La suficiencia no queda demostrada: hay efectos que pueden ocurrir dentro de la transacción o ser causados por ella sin
estado estable que fingerprintar, y callbacks que pueden ejecutar después del último chequeo o del commit.

## 4. Investigación retenida

- Document lock: `PARTIAL_ONLY`; excluye locks incompatibles de otros execution contexts en su alcance.
- Inventario de extensiones/proceso: `PARTIAL_ONLY`; snapshots/subconjuntos y cargas posteriores.
- Entorno controlado: `PARTIAL_ONLY`; reduce superficie sin allowlist semántica completa.
- Eventos/reactores/fingerprints: `PARTIAL_ONLY`; detectan canales registrados, sin completitud.
- APS Automation: más controlado, pero no aplicable al producto interactivo ni autoridad de código único demostrada.

Son defense-in-depth y piezas futuras. No demuestran `ContextIsolationAuthority`.

## 5. Alternativas

| Alternativa | Evaluación | Veredicto |
|---|---|---|
| ALT-1 — Keep V18 | Conserva fail-closed; producto diferido. | **SELECTED / GOVERNING** |
| ALT-2 — Observable Commit Consistency | Mejora frescura/atomicidad, pero no cierra same-context, semántica sin huella o deferred causal. | **REJECTED AS SUFFICIENT AUTHORITY** |
| ALT-3 — Controlled host only | Falta garantía verificable de allowlist inmutable y paridad interactiva. | `INSUFFICIENT` |
| ALT-4 — Best effort | Permite corrupción no detectable con éxito aparente. | `REJECTED` |
| ALT-5 — Abandon | Decisión Owner innecesaria mientras la deferencia sea reversible. | `NOT SELECTED` |

Un híbrido ALT-2 + autoridad parcial vuelve a necesitar cierre de los canales no observables; no habilita ejecución.

## 6. Arquitectura seleccionada

V18 permanece gobernando. `ContextIsolationAuthority = UNKNOWN` y
`SafeOperationalState = FALSE_FOR_ADMISSION`. Observable Commit Consistency queda como obligaciones necesarias,
no suficientes, de cualquier propuesta futura. La garantía V18 frente a canales externos no observados no tiene
sustituto demostrado.

## 7. Modelo formal

```text
ObservableCommitConsistency(D,P,R0,R1,T) =
    HostSupported(D)
  ∧ SourceSnapshotValid(R0)
  ∧ DeclaredReadSetComplete(P,R0)
  ∧ KnownUnsafeStatesInactive(D)
  ∧ DocumentMutationAuthorityHeld(D)
  ∧ ReadSetStable(R0,R1)
  ∧ MutationPlanStillApplicable(P,R1)
  ∧ IndependentSemanticPostconditionsPass(D,P,T)
  ∧ AtomicCommit(T)
```

El lock debe preceder a `R1`. Aun así falta:

```text
ObservableChannelClosure =
  every effect capable of invalidating the result inside the causal boundary
  is excluded, represented in the read-set, or guaranteed to run inside T
  before the final independent checks.
```

No se demostró `ObservableChannelClosure`. Omitirlo hace falsa la suficiencia; añadirlo sin autoridad renombra el
hueco V18.

## 8. Frontera de corrección

Empieza tras PREPARE al adquirir el lock que protege la revalidación final y termina cuando concluyen los efectos
síncronos causalmente provocados por el commit. Un callback que el propio commit programa y el host ejecuta
inmediatamente después no puede excluirse solo porque `Commit()` retornó.

| Momento | Contrato candidato |
|---|---|
| Antes de revalidación | cambio incluido en snapshot nuevo o aborto |
| Revalidación→mutación | excluido o detectado antes de escribir |
| Mutación/chequeos | dentro de T, detectable y rollback-able |
| Commit | callbacks síncronos dentro de causalidad comprobada; autoridad hoy incompleta |
| Deferred causado por writes | requiere fence/receipt o autoridad; no exclusión nominal |
| Edición independiente posterior | fuera de garantía |

## 9. DeclaredReadSetComplete

```text
DeclaredReadSetComplete(P) iff
every external value or effective interpretation whose variation can change
semantic result, geometry, symbols, persistence, admission or postconditions
has a stable observation and authority in P.MirrorReadSet.
```

Incluye diseño authored, variables/PlanReadSet por variable, secciones, biblioteca/blocks, propiedades dinámicas,
layers/linetypes/color/lineweight, fields/XREF, transform, view address, station/index/post data, metadata, exposure,
overrules relevantes e identidades importadas. Una lista manual no basta. G-M24/T-M75 cierra invocaciones alcanzables
y Foundation typed preparation hace explícitos datos consumidos, pero ninguno descubre automáticamente dependencia
semántica implícita de una extensión/custom object/callback. Completitud mecánica: `UNPROVEN`.

## 10. Fingerprints y revalidación

| Entrada | Huella candidata | Estado |
|---|---|---|
| Authored payload | serialización semántica + schema/unknown authority | estable con reader autoritativo |
| Variables | ids, definiciones, valores y RootCauses por variable leída | estable bajo I-49 |
| Embeds/vistas | payload, RackId, kind/address, metadata | estable si todo consumo se relee |
| Transform | matriz/normal/elevation/clase | estable |
| Symbol records | identidad + propiedades efectivas transitivas | cara; acotable si enumerada |
| Dynamic definitions | estructura/propiedades relevantes | cara; callbacks abiertos |
| XREF | contenido cargado/completitud/estado V17 | acotada; incomplete/unresolved ⇒ UNKNOWN |
| Fields | grafo causal y EDIT/STATE V17 | acotada; ciclo sin cota ⇒ UNKNOWN |
| Secciones | ids y valores authored/resueltos | estable |
| Biblioteca | recurso importado + definición efectiva post-import | acotada |
| Overrule | sujeto, RXClass, familia, aplicabilidad, support | parcial; estado interno puede ser no disponible |

Cambio, lectura fallida, UNKNOWN o incompatibilidad abortaría antes de mutar. Una interpretación sin estado observable
sigue fuera del modelo.

## 11. Lock y transaction

```text
ACQUIRE → SNAPSHOT → PREFLIGHT → LINE → PREPARE
→ acquire exclusive document mutation authority
→ FINAL REVALIDATION
→ begin one caller-owned transaction
→ MUTATE
→ INDEPENDENT SEMANTIC POSTCONDITIONS
→ COMMIT once
→ causal completion check / POST
→ release lock
```

Lock después de revalidar deja una carrera. El lock anterior aporta solo exclusión documentada frente a locks
incompatibles de otros contexts; no suspende same-context callbacks ni cierra extensiones. Lock implícito en command
context requiere caracterización; no se presume equivalente. Una sola transacción/commit y creación caller-owned son
necesarios; ningún helper puede hacer commit propio.

## 12. Same-context callbacks/reactors

RACKMIRROR escribe A y un reactor tercero cambia A o B. Si participa en T y corre antes del último chequeo, una
postcondición podría detectarlo y abortar. No hay garantía de que todo reactor use T, ejecute antes del último chequeo
o altere algo releído. Puede cambiar estado global o semántica API sin DB write. Revalidar repetidamente no cierra la
carrera: siempre existe un último instante antes del commit. Este contraejemplo es decisivo.

## 13. Deferred work

- **A, antes del commit:** debe vivir en T y pasar chequeos o abortar.
- **B, agendado por nuestros writes y posterior al commit:** sigue causalmente ligado; sin fence no queda fuera.
- **C, independiente y posterior:** edición futura fuera del contrato.

Idle callbacks, queued commands, `SendStringToExecute`, `ExecuteInCommandContextAsync`, delayed handlers, COM y
paletas pueden caer en B/C. No hay enumeración completa de trabajo pendiente ni join contractual para B. Propuesta
futura: prohibir que RACKMIRROR cause B, obtener receipt/fence, o demostrar independencia. Hoy es gap material.

## 14. Intercepción semántica sin DB write

Overrules, custom graphics/objects, vertical evaluation, symbol resolution, fields y XREF pueden alterar significado
sin mutar la DB leída. V17 acota parte con fail-closed, familia/aplicabilidad y huellas. Una extensión puede conservar
el fingerprint visible y variar por estado interno, tiempo o servicio externo. Si la interpretación se consume,
pertenece al read-set; sin observación estable es UNKNOWN. Tampoco hay discovery completo de todos los consumidores.

## 15. Known unsafe states

Se conservan como defense-in-depth: REFEDIT, BEDIT, ARRAYEDIT Source, BTESTBLOCK, long transaction, DB parcial,
host/transform no soportado, overrule relevante sin support, XREF/field unresolved/incomplete y todas las condiciones
V17/V18. Detector fallido/UNKNOWN bloquea. La lista no prueba completitud.

## 16. Host model

No se amplía `HostExact` a `HostSupported`. El primer alcance posible sigue siendo el tuple exacto AutoCAD 2025.
Más builds requieren caracterización propia. Restringir host no cierra callbacks/extensiones.

## 17. Postcondiciones independientes

| Grupo | Chequeos | Autoridad |
|---|---|---|
| Estructural | count, NewRackId, source intacto, transforms, escalas/determinante, payload, addresses | inspector DB distinto del writer |
| Semántico | round-trip reflejado equivale a μ_k, referencias/variables/grupo | canonical reader + authored comparator |
| Transacción | cero source mutation, un rack por destino, sin huérfanos/parciales | before/after inventory + membership |
| Materialización | blocks resueltos, dynamic properties, dependencias | query fresca de definitions/records |

Reducen drift observable. Si planner y checker comparten regla defectuosa, ambos pueden aceptar error. Se requieren
fixtures independientes, propiedades algebraicas, comparator Foundation y round-trip; independencia universal sigue
sin demostrarse.

## 18. Side-effects de PREPARE

`EnsureForPlan`/imports previos pueden dejar definiciones tras abortar. Solo serían compatibles con **atomicidad
semántica de producto** si son idempotentes/compartibles, no crean RackId/vistas/grupos/embed/persistencia, no cambian
dibujo visible/BOM/selección/semántica existente, se revalidan post-import y fallan sin producto parcial. Si un residuo
tiene efecto observable o cambia resolución futura debe entrar en T o revertirse.

## 19. Fallos

Categorías candidatas, sin congelar nombres: `READSET_CHANGED`, `REVALIDATION_FAILED`, `LOCK_UNAVAILABLE`,
`POSTCONDITION_FAILED`, `SOURCE_MUTATED`, `MATERIALIZATION_DRIFT`, `UNKNOWN_OBSERVABLE` y
`CAUSAL_WORK_UNBOUNDED`. Las primeras abortan antes de write o hacen rollback; la última bloquea admisión. Nunca hay
segundo commit compensatorio ni éxito parcial.

## 20. Contraejemplos CE-01..CE-15

| CE | Detect/prevent | Rollback/boundary | Residual / test |
|---|---|---|---|
| 01 payload cambia SNAPSHOT→MUTATE | fingerprint bajo lock | antes de write | callback tras R1; drift runtime |
| 02 variable cambia tras plan | PlanReadSet/value/cause | aborto | writer same-context; variable race |
| 03 library cambia PREPARE→MUTATE | definición efectiva fresca | residuo §18 | import race |
| 04 reactor cambia block nuevo | postread si corre antes | solo si participa en T | fuera de T/tras check; malicious reactor |
| 05 callback muta source | source before/after | si en T y detectado | tras check/commit; source fixture |
| 06 overrule cambia geometry sin DB | family/applicability/support | no rollback externo | estado no fingerprintable |
| 07 XREF reload | contenido/completitud | aborto si observable | reload durante ventana |
| 08 field cambia valor | causal leaves fingerprint | aborto/rollback | input externo no acotado |
| 09 deferred inmediato causado | no hay fence completo | fuera de T, dentro de causalidad | **gap decisivo** |
| 10 otro context pide write | lock incompatible | prevención documentada | two-context test |
| 11 current document cambia | ownership/identity | aborto/rollback | activación callback |
| 12 extension load tras preflight | inventory parcial | no previene conducta | **gap decisivo** |
| 13 planner/checker comparten bug | oráculos diversos | solo si detector falla | mutation/independent fixtures |
| 14 import persiste y aborta | contrato §18 | no rollback físico aceptado | efecto futuro; import-abort |
| 15 abort tras crear records | una T caller-owned | rollback completo | fault injection/commit guard |

CE-04/05/06/09/12 impiden considerar ALT-2 suficiente.

## 21. Pruebas futuras

- Unit: fingerprints, completeness, UNKNOWN, postconditions, μ_k.
- Integration: drift, fault-injection rollback, identity, persistence, materialization, imports, no internal commit.
- AutoCAD: lock conflict, source mutation, same-context reactor, deferred callback, known editors, XREF/field,
  document switch, rollback.
- Adversarial: malicious reactor, late plugin load, callback sobre source/destination, semantic interception sin write.

Filtros deben seleccionar >0. Verde caracteriza el tuple probado, no completitud universal.

## 22. Owner Validation

Una futura habilitación incluiría concurrent edit, editor activo, variable/source drift, aborted transaction,
postcondition rollback, repeated mirror, save/reopen, UNDO, multi-rack y DWG real. No se ejecuta ahora.

## 23. G-M24 / T-M75

Se preservan como mecanismos de invocation completeness/discovery arbitrario. Evolución futura: `READ`, `WRITE`,
`OBSERVATION_AFFECTING`, `DEFERRED`, `CALLBACK_REGISTERING`, `HOST_STATE`; constructors, methods,
getters/setters, events, operators, callbacks y handlers transitivos; `UNCLASSIFIED ⇒ RED`. No hay cambio de código.

## 24. Autocrítica obligatoria

| Pregunta | Respuesta |
|---|---|
| Q1 same-context divergencia indetectable | **Sí**: tras último check, fuera de T o semántica no releída. |
| Q2 overrule/custom meaning sin huella | **Sí**: estado efectivo no observable/estable y discovery incompleto. |
| Q3 deferred causal inmediato | **Sí**: excluirlo en `Commit()` sería artificial sin fence. |
| Q4 read-set mecánicamente completo | **Parcial**: censo/tipos cubren superficies declaradas, no dependencias implícitas. |
| Q5 postconditions independientes | **No demostrado**: CE-13 permanece. |
| Q6 lock+T suficiente | **No**: no domina same-context, semántica sin DB o deferred causal. |

## 25. Disposición V18

```text
V18 affected contract = EVALUATED / NOT SUPERSEDED
Proposal V18 = GOVERNING
ContextIsolationAuthority = UNKNOWN
SafeOperationalState = FALSE_FOR_ADMISSION
```

V19 es material por reconsiderar la garantía, pero no cambia autoridad al rechazar la sustitución. Research
`PARTIAL_ONLY` identifica defenses útiles y explica su insuficiencia.

## 26. Freeze, O-1 y ADR

```text
Consensus Freeze V18 = HISTORICAL / IMMUTABLE / STILL GOVERNING
New Freeze = NOT PRODUCED BY V19
O-1 V18 = ACCEPTED / REGISTERED / HISTORICAL DECISION FOR DEFERRED STATE
New Owner decision = NOT REQUIRED WHILE V18 REMAINS GOVERNING
ADR-0036 = PROPOSED / AMENDED FOR V18
Further ADR amendment = NOT REQUIRED BY REJECTED ALT-2
```

Una propuesta futura aceptada requeriría consenso, Freeze, nueva decisión Owner y enmienda ADR.

## 27. Regla de G3

`G3 = STOPPED`, `G3B = NOT OPEN`, `CT-50 = NOT EXECUTED`. V19 no reabre nada. Sigue la secuencia V18:
autoridad nueva → CT-49R exacta → autoridad demostrada → compatibilidad → decisión explícita. Una variante futura de
Observable Commit Consistency requiere Proposal/review/freeze propios.

## 28. Invalidadores / reconsideración

Reconsiderar si aparece: contrato sobre same-context reactors; fence de todo trabajo causal; autoridad completa sobre
extensions/semantic interception; read-set mecánicamente completo; oráculos independientes demostrados; o controlled
host con allowlist inmutable y paridad. Cambios de host/build, Foundation, PlanReadSet, V17, G-M24/T-M75, fields/XREF,
overrules, atomicidad o caller-owned creation también requieren evaluación. Evidencia exact-SHA no se transfiere.

## 29. I-55

I-55 avanzó de `f90b679dcdde1dd9d6f74ff7786c9a45dcbce9fc` a
`705ae301f696b7f16c8ec006e2b1ab526be46489` con G9a de redibujo atómico de hermanas, seam caller-owned,
transaction unit y pruebas. Afecta coordinación y un futuro censo/base, pero no redefine ContextIsolationAuthority,
RACKMIRROR, AUTH-15, Foundation ni V18.

```text
I-55 CONTRACT IMPACT = NON-MATERIAL
I-55 COORDINATION = MATERIAL / OBSERVED
```

No se importa política/evidencia I-55.

## 30. Review checklist

- [ ] Identidades exactas.
- [ ] ALT-2 refutada contra CE-01..15.
- [ ] `ObservableChannelClosure` expuesto como premisa faltante.
- [ ] Frontera no excluye causalidad poscommit artificialmente.
- [ ] Lock no recibe garantías inventadas.
- [ ] Read-set/fingerprints declaran UNKNOWN/gaps.
- [ ] Postconditions reconocen CE-13.
- [ ] PREPARE distingue atomicidad semántica e infraestructura.
- [ ] V18/Freeze/O-1/ADR conservan estado.
- [ ] G3/G3B/CT-50/implementation bloqueados.
- [ ] I-55 clasificado sin importar política.
- [ ] Publicación docs-only y CI exacta verde.

```text
PROPOSAL V19 = PUBLISHED / REVIEW REQUIRED
ALT-2 = REJECTED
V18 REMAINS GOVERNING
TECHNICAL CONSENSUS = NOT REACHED
G3 = STOPPED
G3B = NOT OPEN
CT-50 = NOT EXECUTED
SUBSTANTIVE IMPLEMENTATION = BLOCKED
```
