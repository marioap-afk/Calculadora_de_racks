# I-52 — Proposal V22: Materialization Success + Durable Snapshot R

> **PROPOSAL V22 — PUBLISHED / REVIEW REQUIRED. Diseño; no implementación.**
>
> ```text
> Selected alternative = ALT-21C / CORRECTED ADMISSION CONTRACT
> CT-DA research contract = READY FOR REVIEW
> V20 architecture = PRESERVED / RESTRICTED
> V21 = SUPERSEDED ONLY BY THE V22 DELTA
> V18 = GOVERNING
> Coordinator = REVIEW REQUIRED
> Architect = REVIEW REQUIRED
> Technical Consensus = NOT REACHED
> G3 = STOPPED
> G3B = NOT OPEN
> CT-50 = NOT EXECUTED
> SUBSTANTIVE IMPLEMENTATION = BLOCKED
> ```

V22 corrige sólo cuatro defectos de V21: incorpora `MaterializationCorrect` al éxito, define snapshots destino
`R_pre/R_post` y `M_pre/M_post`, cierra la admisión CT-DA con propiedades necesarias/suficientes y mueve CT-DA antes
del Freeze final, Owner y ADR. Conserva el closed semantic snapshot de V20/V21, el modelo de tres capas y ALT-21C.

## 1. Estado e identidades

| Objeto | Identidad / estado |
|---|---|
| I-52 inicial / Proposal V21 | `d13c0390f2064b333b4030491788ae12ecfd2f91` |
| Proposal V21 | blob `ff037f02c5e26136a47610ca2de848102c6d4ca9` |
| paquete Architect V21 | blob `c16a52d3131ebdc2c9b1afaa6d32b1020d8355c3` |
| decisions V21 | blob `1b84289e5eaf1b99025fb4e8ec39f38f22f7a7f2` |
| `origin/main` | `7097057cf8685bf5ecc09083cba37379d4a4aae8` |
| I-57 | tag object `a5bc02210f740839e2fac37e63fc00512e5ccee6`; target `7097057cf8685bf5ecc09083cba37379d4a4aae8` |
| I-55 re-fetch | `67779989233ef190810d2c5a61e1f1e499e31983` |

Preflight: rama/upstream exactos, árbol limpio, sin stash ni operación Git incompleta. SHA y blobs V22 se reportan
después del commit.

## 2. Cierre de la revisión V21

```text
Coordinator = AGREED WITH PROPOSAL V21 — TARGETED HOST CHARACTERIZATION REQUIRED
Architect = CHANGES REQUIRED — PROPOSAL V22
BLOCKER = OverallSuccess omits MaterializationCorrect
HIGH-1 = FreshAuthoritativeRead has no snapshot point R
HIGH-2 = CT-DA lacks necessary/sufficient admission criteria and threat model
HIGH-3 = CT-DA must occur before final Freeze/Owner/ADR
V18 = GOVERNING
```

V21 permanece histórica e inmutable. Estos cuatro hallazgos se aceptan y se corrigen sin reabrir otras decisiones.

## 3. Delta V22

V22 añade exclusivamente:

1. `MaterializationCorrect` como condición independiente de `OverallSuccess`;
2. observaciones consistentes `R_pre/R_post` y `M_pre/M_post`;
3. threat model T1..T15 y propiedades obligatorias DA-P1..10;
4. `CTDA_PASS iff ALL PASS`, con `FAIL/UNKNOWN` inadmisibles;
5. probes CT-DA-01..15 y política de evidencia exact-host;
6. investigación CT-DA antes del Freeze final, decisión Owner y enmienda ADR.

## 4. `OverallSuccess` corregido

```text
D  = SourceSemanticSnapshotAtS
D' = μ_k(D)
Dp = DestinationAuthoritySnapshotAtR_post
Mp = DestinationMaterializationSnapshotAtM_post

SemanticMirrorCorrect =
    DetachedSemanticResult == μ_k(SourceSemanticSnapshotAtS)

PersistedSemanticCorrect =
    Dp ≡ D'

MaterializationCorrect =
    Mp ≡ ExpectedMaterialization(D', AcceptedPlan, SupportedKindContract)

OverallSuccess =
    SemanticMirrorCorrect
  AND PersistedSemanticCorrect
  AND MaterializationCorrect
  AND MaterializationCommitSucceeded
```

`Transaction.Commit()` sólo acredita que el host aceptó el commit; no acredita semántica ni materialización. Nunca
puede existir `OverallSuccess = true` con `MaterializationCorrect = false`.

## 5. Contrato `MaterializationCorrect`

```text
MaterializationCorrect(D', Plan, Actual) =
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
  AND SupportedKindPhysicalPostconditions
```

El verifier inspecciona estado real de DB y no confía en el plan como oráculo de lo escrito. No promete equivalencia
visual bajo overrules arbitrarios de terceros; verifica estado físico controlado por RackCad.

## 6. Punto fuente `S`

`S` conserva la definición V21: lectura autoritativa final de toda fuente y `PlanReadSet`, bajo document lock y dentro
de la transacción de mutación, inmediatamente antes de la primera escritura destino. `D = SourceSemanticSnapshotAtS`
y `D' = μ_k(D)`. Una mutación posterior independiente puede serializarse después; una mutación causal same-command es
T10 y debe prevenirse o detectarse por DA-P10.

## 7. Punto destino `R`

`R` no significa «terminó el loop». Es un punto lógico en el que el conjunto completo de autoridad destino puede
atribuirse a una observación consistente. Incluye todas las hermanas, `RackId`, kind, DTO authored, schema, metadata,
`ExtensionData`, bindings, `DimensionViews`, custom properties, addresses, count, grouping y NOD cuando aplique.

Hasta que DA-P5 demuestre un mecanismo host válido, `DestinationAuthoritySnapshotAtR` **no existe para admisión**.

## 8. `R_pre` y `R_post`

```text
Dpre  = DestinationAuthoritySnapshotAtR_pre
Dpost = DestinationAuthoritySnapshotAtR_post

StagedAuthorityCorrect   = Dpre ≡ D'
PersistedSemanticCorrect = Dpost ≡ D'
```

- `R_pre`: snapshot consistente staged dentro de T, antes de commit; habilita aborto.
- `R_post`: snapshot consistente committed en una nueva transacción, después de commit y antes del punto final
  caracterizado; decide `PersistedSemanticCorrect`.

V22 no presume que una transacción de lectura produzca snapshot estable. DA-P4 acredita read-your-own-writes y DA-P5
acredita consistencia de ambos scans.

## 9. Snapshot consistente de hermanas

Son candidatas, no hechos:

| Mecanismo | Condición de validez |
|---|---|
| R-A una Transaction ofrece vista consistente | sólo si contrato/evidencia exact-host lo demuestra |
| R-B fingerprint antes/después | sólo si existe autoridad versionada completa; V22 no inventa global DB version |
| R-C prevención de writes durante scan | sólo si la autoridad host cubre T1..T13/T15 aplicables |
| R-D mecanismo host alternativo | debe documentar alcance, fallos y punto lógico |

Contraejemplo obligatorio: leer A=`D'`, callback cambia B, leer B=`D''`. La mezcla nunca existió como conjunto. Si el
mecanismo seleccionado no previene o detecta esta intercalación, DA-P5=`FAIL/UNKNOWN` y ALT-21C es inadmisible.

## 10. Snapshot de materialización

```text
Mpre  = StagedMaterializationSnapshotAtM_pre
Mpost = CommittedMaterializationSnapshotAtM_post
```

`M_pre` verifica antes de commit todo lo observable y permite aborto. `M_post` inspecciona estado real confirmado y
decide `MaterializationCorrect`. No se infiere Mpost desde Mpre. Ambos scans requieren consistencia DA-P9 y pueden
compartir el mecanismo R sólo si la evidencia demuestra que abarca conjuntamente payload, NOD y entidades físicas.

La alternativa seleccionada es M2: verificación precommit + postcommit. M1 no acredita estado confirmado; M3 pierde
la oportunidad de rollback ante fallos visibles antes de commit.

## 11. Threat model CT-DA

El sujeto protegido es autoridad semántica durable RackCad y materialización RackCad durante `[S, command-completion]`.
No se inventarían identidades de plugins; se caracterizan clases de ejecución/escritura.

| Clase | Alcance |
|---|---|
| T1 managed handler same-context | IN |
| T2 reactor nativo | IN |
| T3 transacción secondary/nested | IN |
| T4 callback disparado por commit | IN |
| T5 callback preread→commit | IN |
| T6 callback commit→postread | IN |
| T7 callback durante scan multi-sibling | IN |
| T8 callback postread→command-completion | IN |
| T9 mutación variable NOD | IN cuando el kind la consume |
| T10 mutación payload fuente | IN |
| T11 mutación payload destino | IN |
| T12 mutación sólo de materialización | IN |
| T13 writer de otro command/context | IN para probar conflicto/rechazo observable |
| T14 carga de módulo sin registrar/ejecutar mutador | OUT: no altera por sí sola el estado protegido |
| T15 carga que registra/ejecuta T1..T12 | IN por la clase mutadora resultante |

## 12. Amenazas semánticas

`SEMANTIC_AUTHORITY_THREATS = {T1..T11, T13, T15}` cuando tocan payload, identidad, bindings, addresses, grouping,
metadata o NOD. Se mapean a DA-P1..6, DA-P8 y DA-P10. Overrule visual sin cambio de bytes no pertenece a esta clase.

## 13. Amenazas de materialización

`DERIVED_MATERIALIZATION_THREATS = {T1..T8, T12, T13, T15}` cuando cambian referencias, definiciones, transforms,
dynamic properties, recursos o linkage. Se mapean a DA-P2..3, DA-P5..7 y DA-P9. Una misma ejecución puede pertenecer
a ambas clases.

## 14. Propiedades DA-P obligatorias

| Propiedad | Requisito finito |
|---|---|
| DA-P1 | write semántico same-context durante T participa en T y revierte, es prevenido, o es detectado antes de éxito |
| DA-P2 | efecto semántico/material de commit es visible en `R_post/M_post`; ningún callback de commit queda silencioso |
| DA-P3 | secondary/nested T no puede confirmar mutación protegida que escape rollback y verificación final |
| DA-P4 | reader productivo ve exactamente writes staged: new/overwrite Xrecord, dictionary, siblings y ResultBuffer |
| DA-P5 | `R_pre/R_post` tienen snapshot consistente del conjunto completo; interleaving se previene o detecta |
| DA-P6 | no hay mutación semántica síncrona entre snapshot final aceptado y command-completion, o queda dentro del snapshot |
| DA-P7 | verifier físico observa todas las escrituras RackCad necesarias para `MaterializationCorrect` |
| DA-P8 | NOD relevante obedece la misma DB/T/snapshot/commit/undo que autoridad y vistas |
| DA-P9 | `M_pre/M_post` son snapshots físicos consistentes; interleaving durante scan se previene o detecta |
| DA-P10 | mutación de fuente después de S se ordena como operación independiente o se detecta como causal same-command |

Cada amenaza IN debe mapearse a al menos una propiedad demostrada. Amenaza sin cobertura implica propiedad
`UNKNOWN`, nunca exclusión implícita.

## 15. Admisión necesaria y suficiente

```text
CTDA_PASS iff
    DA-P1 = PASS AND DA-P2 = PASS AND DA-P3 = PASS AND DA-P4 = PASS AND DA-P5 = PASS
AND DA-P6 = PASS AND DA-P7 = PASS AND DA-P8 = PASS AND DA-P9 = PASS AND DA-P10 = PASS
AND every IN-scope threat class has demonstrated coverage
AND exact host tuple is complete and unchanged
AND adversarial fixtures exercised every required probe family
```

Si cualquier propiedad es `FAIL` o `UNKNOWN`, `CTDA_PASS = false` y ALT-21C=`NOT ADMISSIBLE`. No existe partial PASS.
Un resultado observado para un handler prueba semántica host sólo dentro del event/context/transaction class cubierto;
no demuestra ausencia de otros handlers.

DA-P8 sólo puede registrarse `PASS / NOT APPLICABLE` cuando el contrato del kind y el `PlanReadSet` demuestran que la
operación no lee ni escribe NOD; una ausencia asumida o no censada permanece `UNKNOWN`.

## 16. Matriz de resultados

| Propiedad | PASS | FAIL | UNKNOWN | Consecuencia FAIL/UNKNOWN |
|---|---|---|---|---|
| DA-P1 | continuar | write puede escapar | afiliación no probada | rechazar ALT-21C |
| DA-P2 | continuar | commit silencioso | visibilidad no probada | rechazar ALT-21C |
| DA-P3 | continuar | secondary T escapa | nesting desconocido | rechazar ALT-21C |
| DA-P4 | continuar | staged read falso | RYOW desconocido | rechazar ALT-21C |
| DA-P5 | continuar | snapshot mixto | consistencia desconocida | rechazar ALT-21C |
| DA-P6 | continuar | carrera final | completion desconocido | rechazar ALT-21C |
| DA-P7 | continuar | estado físico no observable | cobertura desconocida | rechazar ALT-21C |
| DA-P8 | continuar/NA acreditado | NOD fuera de unidad | semántica NOD desconocida | rechazar kind/ALT-21C aplicable |
| DA-P9 | continuar | snapshot físico mixto | consistencia desconocida | rechazar ALT-21C |
| DA-P10 | continuar | fuente causal muda | orden desconocido | rechazar ALT-21C |

`ALL PASS →` la arquitectura puede avanzar a Freeze final/Owner/ADR. `ANY FAIL →` Proposal/fallback V18.
`ANY UNKNOWN →` investigación adicional sin admisión.

## 17. Alcance host exacto

Toda evidencia debe registrar como mínimo: AutoCAD 2025; managed API `25.0.0.0`; versiones exactas de `acad.exe`,
`acdbmgd.dll`, `acmgd.dll` y dependencias relevantes; program/product/vertical; build y registry identity; hashes de DLL
cuando estén disponibles; OS/runtime; SHA de fixtures y del plugin probe. Cambiar cualquier miembro invalida CT-DA.

## 18. Política de evidencia

Cada DA-P clasifica sus fuentes como `DOCUMENTED CONTRACT`, `DOCUMENTED BEHAVIOR`, `RUNTIME OBSERVATION` o
`INFERENCE`. Una inferencia sola nunca produce PASS. Propiedades críticas pueden pasar por contrato documentado
aplicable o por observación adversarial repetida del tuple exacto, registrada explícitamente como soporte dependiente
de implementación y aceptada en revisión exacta Coordinator+Architect. Observación no se generaliza a otro build.

## 19. Plan CT-DA-01..15

V21 `CT-DA-01..08` queda preservado históricamente y refinado por esta enumeración V22:

1. `CT-DA-01`: callback same-context escribe Xrecord destino durante T.
2. `CT-DA-02`: callback escribe usando la misma T.
3. `CT-DA-03`: callback inicia/commit nested o nueva T.
4. `CT-DA-04`: T principal aborta después del write del callback.
5. `CT-DA-05`: T principal confirma después del write del callback.
6. `CT-DA-06`: orden de eventos al cambiar `Xrecord.Data`.
7. `CT-DA-07`: crear definición/Xrecord y leerlo con reader productivo en la misma T.
8. `CT-DA-08`: scan de hermanas con mutación intercalada.
9. `CT-DA-09`: handler disparado por commit muta payload.
10. `CT-DA-10`: postread seguido por callback síncrono antes de command-completion.
11. `CT-DA-11`: mutación Xrecord NOD.
12. `CT-DA-12`: mutación de entidad durante scan del verifier físico.
13. `CT-DA-13`: intento de secondary T que sobreviva al aborto principal.
14. `CT-DA-14`: mutación del payload fuente después de S.
15. `CT-DA-15`: mutación de fuente y destino en el mismo callback.

Cada probe registra host, evento, thread/context, documento, current/top transaction, ObjectId, open mode, identidad
transaccional observable, bytes before/after, commit/abort, secuencia, timestamps/orden, bytes postcommit y eventos de
ciclo del comando. Incluye new/overwrite Xrecord, extension dictionary, siblings, `ResultBuffer`, abort y commit.

## 20. Límite research-only de CT-DA

CT-DA futuro es `RESEARCH / CHARACTERIZATION ONLY`, separado de G3. Puede crear herramientas en `eng/validation`,
fixtures adversariales, comandos temporales de prueba, logs y probes de transactions/events/Xrecords/reactors.
Prohíbe implementar RACKMIRROR, AUTH-15, guardas productivas o cambios de comportamiento de producto. Esta Proposal no
autoriza todavía su ejecución; la autorización limitada ocurre sólo después de consenso V22.

## 21. Secuencia B

```text
Proposal V22
→ exact review Coordinator
→ exact review Architect
→ technical consensus V22
→ LIMITED CT-DA RESEARCH AUTHORIZATION
→ CT-DA execution
→ exact review CT-DA
→ architectural decision on CT-DA result
→ final replacement Freeze
→ Owner decision
→ ADR amendment
→ explicit G3 reopening
→ later implementation characterization
```

Esta secuencia reemplaza la secuencia de proceso V21; no reemplaza todavía el contrato de producto V18.

## 22. Momento del Freeze

Antes de CT-DA no hay Freeze final de reemplazo. Puede publicarse un contrato/plan de investigación CT-DA, pero no un
product consensus Freeze. Freeze V18 sigue gobernando. Sólo `CTDA_PASS` permite preparar el Freeze final.

## 23. Momento Owner

No se pide al Owner aceptar la garantía V22 antes del resultado CT-DA. Resultado favorable: se formula garantía final
y se solicita decisión. Resultado adverso: se vuelve a Proposal/fallback sin pedir aceptación de una garantía inviable.

## 24. Momento ADR

ADR-0036 permanece `PROPOSED / AMENDED FOR V18` durante CT-DA. Sólo un resultado favorable, arquitectura final y
Freeze permiten preparar una enmienda nueva. No hay ADR especulativa.

## 25. `FAILURE_COMMITTED_CORRUPT`

Es un estado de observación permitido únicamente en CT-DA/pruebas. Significa que el commit ocurrió y `R_post` o
`M_post` refutó la corrección. No es product success, rollback ni compensación. En caracterización provoca
DA-P aplicable=`FAIL` y ALT-21C=`NOT ADMISSIBLE` hasta eliminar la causa mediante otra arquitectura/autoridad.

V22 no diseña compensación productiva. Borrar o marcar después puede fallar y disparar callbacks adicionales.

## 26. Fallo multi-rack postcommit

La unidad lógica continúa siendo la invocación completa. Si una sola rack/hermana falla en `R_post/M_post`:

```text
whole invocation = FAILURE_COMMITTED_CORRUPT
successful destinations reported = NONE
Candidate eligibility = AUTOMATICALLY INVALID
```

El diagnóstico enumera cada rack/address esperado, estado observado y divergencia, pero no declara éxitos parciales.
Este estado sólo puede observarse durante caracterización; admitirlo como conducta final exigiría reducción de garantía
y decisión Owner futura.

## 27. Readiness por kind

Selective es el único fixture business avanzado, pero sigue no product-ready. CT-DA puede usarlo para medir semántica
host si el resultado no depende de reglas Selective. Dynamic, Header, Push Back y Cantilever son inadmisibles hasta
tener reader/writer/comparator/siblings/materialization/round-trip; Flow Bed queda fuera.

Aun con `CTDA_PASS`, Selective necesita `μ_k`, adapter, AUTH-15, verifier físico, pruebas, SAVE/reopen, UNDO, Owner
Validation y obligaciones G3/G3B. CT-DA sólo retiraría el blocker de autoridad durable.

## 28. Limitaciones CT-DA

CT-DA no prueba: memory patching malicioso, inyección OS, corrupción hardware, edición arbitraria después del comando,
overrule visual que no cambia bytes RackCad, semántica de builds futuros ni ausencia de plugins desconocidos. Prueba
semántica host de clases explícitas T/DA-P en el tuple exacto, no identidad o benevolencia de handlers.

## 29. Disposición V20

`V20 architecture = PRESERVED / RESTRICTED`. Se conservan `RACKCAD-OWNED CLOSED SEMANTIC SNAPSHOT + TRANSACTIONAL
DERIVED MATERIALIZATION`, las tres capas y ALT-21C conceptual. No alcanza consenso final ni reemplaza V18.

## 30. Disposición V21

V21 queda `SUPERSEDED ONLY FOR`: fórmula `OverallSuccess`, snapshot R/M, admisión CT-DA y secuencia de proceso. Sus
demás contratos permanecen incorporados por referencia, incluidos S, independencia writer/reader/comparator/`μ_k`,
hermanas, NOD, `PlanReadSet`, per-kind readiness, UNDO y `D' ≡ D1 ≡ D2 ≡ D3`.

## 31. Disposición V18

```text
V18 = GOVERNING
Freeze V18 = GOVERNING
O-1 V18 = GOVERNING
ADR-0036 = PROPOSED / AMENDED FOR V18
```

V22 no reemplaza contrato productivo, Freeze, decisión Owner o ADR.

## 32. Regla G3

G3 permanece STOPPED. La autorización CT-DA posterior al consenso V22 será investigación separada, no reapertura G3.
Sólo resultado favorable + Freeze final + Owner + ADR permiten solicitar reapertura explícita.

## 33. Coordinación I-55

I-55 permanece en `67779989233ef190810d2c5a61e1f1e499e31983`, cierre documental G9b. Impacto contractual I-52
`NON-MATERIAL`; coordinación `MATERIAL`; seam `POTENTIALLY REUSABLE`. No transfiere evidencia de snapshots,
callbacks, materialización postcommit, AUTH-15 ni CT-DA.

## 34. Invalidadores

Invalidan admisión: cualquier DA-P `FAIL/UNKNOWN`; amenaza IN sin mapping; host tuple incompleto/cambiado; scan mixto;
Mpost inferido de Mpre; commit usado como prueba física; reader no productivo; comparator/verifier que confía en plan;
callback final no cubierto; NOD fuera de DB/T; éxito parcial multi-rack; `FAILURE_COMMITTED_CORRUPT`; kind sin contratos;
generalización de fixture a plugins desconocidos; o evidencia sólo inferida.

## 35. Checklist y autocrítica

- [ ] `OverallSuccess` nunca es true con `MaterializationCorrect=false`.
- [ ] `R_pre/R_post` y `M_pre/M_post` tienen snapshot consistente demostrado; no «loop finished».
- [ ] `CTDA_PASS` tiene condiciones finitas necesarias/suficientes y todo UNKNOWN rechaza.
- [ ] La evidencia prueba semántica host, no ausencia de plugins.
- [ ] Freeze/Owner/ADR quedan después de CT-DA.
- [ ] `FAILURE_COMMITTED_CORRUPT` nunca es product success.
- [ ] Un fallo postcommit invalida toda invocación multi-rack.
- [ ] CT-DA no autoriza implementación.
- [ ] CT-DA no renombra `ContextIsolationAuthority`: carece de inventario universal y se limita a clases host.
- [ ] V18/Freeze/O-1/ADR continúan gobernando y G3 sigue cerrado.

Respuestas Q1..Q10: `NO`; `NO, R inválido si puede ser mezcla`; `YES`; `YES`; `NO`; `YES`; `NO`; `NO`; `NO`;
`NO`. Cualquier evidencia futura que cambie una respuesta requerida invalida V22 y exige Proposal/fallback.

```text
PROPOSAL V22 = PUBLISHED / REVIEW REQUIRED
CT-DA RESEARCH CONTRACT = READY FOR REVIEW
TECHNICAL CONSENSUS = NOT REACHED
V18 = GOVERNING
G3 = STOPPED
G3B = NOT OPEN
CT-50 = NOT EXECUTED
SUBSTANTIVE IMPLEMENTATION = BLOCKED
```
