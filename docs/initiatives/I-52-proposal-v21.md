# I-52 — Proposal V21: Durable Semantic Authority Boundary

> **PROPOSAL V21 — PUBLISHED / REVIEW REQUIRED. Diseño; no implementación.**
>
> ```text
> Selected alternative = ALT-21C
> V20 architecture = PRESERVED / TARGETED HOST CHARACTERIZATION REQUIRED
> Durable authority blocker = OPEN
> V18 = GOVERNING
> Coordinator = REVIEW REQUIRED
> Architect = REVIEW REQUIRED
> Technical Consensus = NOT REACHED
> G3 = STOPPED
> G3B = NOT OPEN
> CT-50 = NOT EXECUTED
> SUBSTANTIVE IMPLEMENTATION = BLOCKED
> ```

V21 corrige únicamente el bloqueador de V20 entre el resultado semántico detached y la autoridad semántica durable
persistida. Conserva el closed world RackCad y la materialización derivada, pero rechaza que una relectura precommit
por sí sola cierre la carrera. La arquitectura queda restringida hasta demostrar en el host exacto un conjunto pequeño
de propiedades sobre Xrecords, callbacks y transacciones. No se vuelve a exigir aislamiento universal de AutoCAD.

## 1. Estado e identidades

| Objeto | Identidad / estado |
|---|---|
| I-52 inicial / Proposal V20 | `88a0ad70f47a681c9d94dac33726519d9aabf8f8` |
| Proposal V20 | blob `682914133ea8af887477c97119a3973971cd97da` |
| paquete Architect V20 | blob `5c0bd1d20b91fc435d1b47787c453cd79035d7d6` |
| decisions V20 | blob `cbf30c42561721b489b67b840b4c1f345535d214` |
| `origin/main` | `7097057cf8685bf5ecc09083cba37379d4a4aae8` |
| I-57 | tag object `a5bc02210f740839e2fac37e63fc00512e5ccee6`; target `7097057cf8685bf5ecc09083cba37379d4a4aae8` |
| I-55 observado inicialmente | `fa8f6affa769f87afe795fd310fbc74c43494123` |
| I-55 re-fetch | `67779989233ef190810d2c5a61e1f1e499e31983`; cierre documental G9b |

Preflight: rama/upstream exactos, árbol limpio, sin stash ni operación Git incompleta. El SHA y blobs V21 se reportan
después del commit.

## 2. Blocker del Coordinator sobre V20

```text
Coordinator = CHANGES REQUIRED — PROPOSAL V21
BLOCKER = durable semantic authority transition not closed
V18 = STILL GOVERNING
```

El contraejemplo es válido: RackCad calcula `D'`, lo escribe en el Xrecord, un callback same-context lo cambia
silenciosamente a `D''`, la transacción confirma y el comando reporta éxito. La corrección detached no convierte a
`D''` en autoridad correcta. Llamar a ese payload «derived» sería falso: edición, Actualizar, BOM, insertar vista y
save/reopen lo consumen como autoridad.

## 3. Modelo de tres capas

### 3.1 `DetachedSemanticResult`

```text
D' = μ_k(D)
```

Resultado puro, tipado e inmutable construido desde un snapshot semántico cerrado propiedad de RackCad. No contiene
`DBObject`, ids transaccionales ni representación física.

### 3.2 `DurableSemanticAuthority`

Estado authored persistido dentro del DWG y leído posteriormente por todos los consumidores semánticos. Incluye el
documento tipado, identidad, bindings y metadata acreditada. **No es derived state.** Una diferencia respecto de `D'`
es fallo semántico, aunque las entidades se vean correctas.

### 3.3 `DerivedPhysicalMaterialization`

Referencias/definiciones de bloque, curvas, propiedades dinámicas, capas y apariencia generadas desde la autoridad
durable. Puede regenerarse mediante Actualizar si la autoridad durable permanece sana.

## 4. Corrección formal

```text
SemanticMirrorCorrect =
    DetachedSemanticResult == μ_k(SourceSemanticSnapshotAtS)

PersistedSemanticCorrect =
    FreshAuthoritativeRead(DestinationSemanticAuthority) ≡ DetachedSemanticResult

MaterializationCorrect =
    DestinationDerivedViews faithfully represent DestinationSemanticAuthority
    under the supported materialization contract

OverallSuccess =
    SemanticMirrorCorrect
  AND PersistedSemanticCorrect
  AND MaterializationCommitSucceeded
```

`≡` es equivalencia authored por comparator tipado, no igualdad accidental de una serialización. Si la lectura durable
final difiere, `OverallSuccess = false`.

## 5. Serialization point de la fuente

`S` es la lectura autoritativa final de todas las fuentes, dentro del document lock y de la misma transacción de
mutación, inmediatamente antes de la primera escritura destino. Esa lectura debe coincidir con el snapshot acreditado
usado para producir `D'`; si no, se aborta y se replantea desde ACQUIRE.

RACKMIRROR se serializa como si hubiera leído la fuente en `S`. Una edición independiente que comienza después del
retorno puede ordenarse después de la operación. Una mutación same-context que ocurre después de `S` pero antes de
terminar el comando no se excluye nominalmente: si cambia fuente, variables o destino durante la misma ventana, entra
al bloqueador host y debe ser prevenida o detectada por el contrato caracterizado.

## 6. Ventana de autoridad destino

La ventana requerida empieza con la primera escritura de autoridad semántica destino y termina cuando el comando
devuelve éxito. Durante ella, todas las hermanas deben conservar autoridad equivalente a `D'`. Una foreign write antes
del retorno no es una edición futura y no puede ocultarse como «corrupción externa posterior».

Por ello V21 no adopta ALT-R1 ni ALT-R2. Emitir bytes correctos no basta si el comando sabe, o debería poder saber bajo
su contrato, que la autoridad ya cambió antes de devolver éxito.

## 7. Relectura autoritativa fresca

La relectura obligatoria no compara objetos en memoria del writer. Debe recorrer el DWG real:

1. enumerar todas las definiciones destino desde la base/transacción mediante el reader que consumen los flujos reales
   (`RackBlockData.Read`/scan equivalente, no la estructura preparada);
2. deserializar el envelope con `RackEmbedStore`;
3. deserializar `Design` con el store tipado del kind;
4. acreditar schema, metadata, `ExtensionData`, custom properties y bindings;
5. reconstruir las addresses mediante Foundation;
6. comparar con un comparator authored independiente del writer y de `μ_k`.

Debe ejecutarse precommit y, si la caracterización lo permite, postcommit en una nueva transacción de lectura. La
relectura precommit es necesaria para rollback; la postcommit observa efectos del commit. Ninguna es suficiente sin
resolver las carreras de §§9–10.

## 8. Contenido semántico y metadata por vista

| Campo | Autoridad durable | Regla de verificación |
|---|---:|---|
| `RackId` / identidad interior | sí | un `NewRackId` por rack lógico/destino, igual en hermanas |
| `Kind` | sí | exacto y soportado por adapter/comparator |
| authored DTO | sí | equivalencia estructural/semántica completa |
| `SchemaVersion` | sí | legible, sin downgrade y acorde al resultado |
| `ExtensionData` / metadata authored | sí | include-by-default; diferencia material |
| custom properties | sí | autoridad y preservación según contrato I-54 |
| bindings / variable references | sí | ids textuales y relación authored exacta |
| identidad `PlanReadSet` persistida | sí, si existe en schema futuro | por variable leída; no unión global |
| `DimensionViews` | sí | authored y coherente con exposure |
| nombre lógico | sí | mismo nombre destino donde el kind lo define |
| view address / section | sí, por vista | canonical, única, esperada y completa |
| grouping identity | sí | conjunto exacto de hermanas por `RackId` |
| block definition name | materialización | coincide con plan/naming, no define `D'` |
| layer, transform, curves, dynamic properties | materialización | postcondiciones físicas separadas |

## 9. Autoridad de hermanas

La relectura final abarca **todas** las hermanas destino. Exige: mismo `NewRackId`, mismo kind, authored state y
metadata equivalentes; addresses válidas y únicas; conjunto esperado completo; cero hermanas inesperadas; y campos
per-view conformes al plan. Validar sólo la primera vista es inválido.

Selective es el único kind con comparator authored completo demostrado en este slice. Los demás quedan inadmisibles
hasta tener reader, writer, comparator, sibling authority, round-trip y save/reopen probados. No hay first/majority ni
fallback genérico.

## 10. Carrera precommit y callbacks de commit

Secuencia adversarial:

```text
write D' → preread passes → callback writes D'' → Commit() → success
```

La relectura no cierra el último intervalo. La investigación previa confirmó que handlers pueden escribir otros
objetos y que el document lock no suspende callbacks same-context. No hay autoridad disponible que demuestre que
`ObjectModified`, eventos DBObject/database/transaction o un reactor nativo no puedan ejecutar después de la última
lectura o durante `Commit()`.

También permanece abierta la posibilidad de una transacción secundaria o write no afiliado a `T`. Si puede confirmar
estado semántico que sobreviva al rollback de `T`, el contrato falla. V21 no inventa que todo callback participa en la
transacción superior.

## 11. Verificación postcommit

```text
Commit T
→ open fresh read transaction
→ enumerate/read/parse/compare all destination authority
→ verify source/PlanReadSet binding as required
→ return SUCCESS only on equality
```

Esto detecta mutaciones ocurridas durante commit que estén visibles al reader. Si falla, el resultado correcto es
`FAILURE_COMMITTED_CORRUPT`: el comando no reporta éxito, pero ya no puede prometer cero mutación. Un warning con
éxito es falso. Una transacción compensatoria puede intentar borrar el destino, pero no equivale a rollback atómico y
puede fallar o disparar más callbacks. Marcar corrupto también es un write compensatorio, no reparación.

Además queda una ventana entre la relectura postcommit y el retorno. Sólo puede tratarse como edición posterior si la
caracterización demuestra que no hay yield/callback síncrono en ese tramo o fija un punto contractual host donde el
comando ya terminó. Esa propiedad no está demostrada.

## 12. Alternativas de transacción

| Alternativa | Detección | Rollback | Veredicto |
|---|---|---|---|
| ALT-P1 precommit solamente | hasta última lectura | sí, para cambios dentro de T detectados | insuficiente: carrera y commit callbacks |
| ALT-P2 pre + postcommit | observa efectos visibles del commit | no tras commit | necesaria como defense/evidencia; insuficiente sola |
| ALT-P3 staging/nested T | desconocida | no se presume outer rollback/visibilidad/reactores | no seleccionada sin autoridad host |
| ALT-P4 payload escrito al final | reduce ventana previa | callback de write/commit sigue abierto | útil, no suficiente |
| ALT-P5 una T + pre/post read + host contract dirigido | cubre autoridad bajo propiedades caracterizadas | preread permite rollback; postread evita falso éxito detectable | **candidata seleccionada, bloqueada por caracterización** |

La estructura preferida es Candidate B: lock → begin T → source read/validation en T → writes → preread autoritativo
de todas las hermanas → commit → postcommit authoritative read → éxito. Leer la fuente dentro de T reduce la ventana y
permite un único snapshot DB, pero no resuelve callbacks por sí mismo.

## 13. Read-your-own-writes

El código de RackCad permite pasar la misma `Transaction` a `RackBlockData.Write` y `RackBlockData.Read`, pero no hay
prueba física ni contrato citado en la evidencia I-52 que demuestre que el reader completo ve exactamente el Xrecord
staged que confirmará `Commit()`, incluidos reemplazo de `ResultBuffer`, nuevas definiciones y extension dictionaries.

Estado: `UNKNOWN / G3 TARGETED CHARACTERIZATION REQUIRED`. Se debe probar sobre AutoCAD 2025/API/build exactos para
objetos nuevos y existentes, múltiples hermanas, overwrite y aborto. Un unit test sin Autodesk API no acredita esto.

## 14. Independencia de comparator

```text
writer(D') → bytes
fresh reader(bytes) → D_persisted
authored comparator(D', D_persisted) → equivalent/divergent
pure μ_k oracle(D) → D'
```

Writer, reader, comparator y `μ_k` tienen responsabilidades distintas. El comparator no llama a `μ_k` ni compara sólo
JSON producido por el writer. Debe incluir schema y unknown metadata, usar fixtures independientes, round-trips,
mutaciones por campo y oráculos algebraicos. CE-13 de V19 permanece: un bug compartido puede aceptar bytes erróneos.

## 15. Readiness por kind

| Kind | Estado V21 |
|---|---|
| Selective | candidato más avanzado; comparator completo existente, pero durable window/host aún bloquea |
| Dynamic | inadmisible: comparator/sibling authority/save-reopen V21 no demostrados |
| Header | inadmisible por la misma razón |
| Push Back | inadmisible por la misma razón |
| Cantilever | inadmisible por la misma razón |
| Flow Bed | fuera del alcance vigente y sin contrato completo |

Cada kind obtiene `PersistedSemanticAuthority_k`; no existe adapter universal que promueva readiness.

## 16. Variables NOD

El espejo normal no crea variables nuevas. Conserva ids/referencias y captura en el snapshot el `PlanReadSet` de toda
variable realmente leída: `SymbolId + complete RootCauses(variable)` por separado. La validación final relee el registro
si el kind lo consume.

Si una evolución escribe NOD, ese Xrecord se vuelve parte de la autoridad durable y debe escribirse/verificarse en la
misma `Database` y la misma `Transaction` que payloads/vistas. `ProjectVariableMutationExecutor` demuestra un seam de
código que combina registro y vistas en una T, pero no demuestra la protección host I-52. Otra base, documento o commit
separado es `BLOCKER`.

## 17. Atomicidad multi-rack

Se conserva L-27/V17: selección todo-o-nada. La unidad es la invocación completa aceptada, con todos los racks y todas
las vistas destino en una T caller-owned y un commit. Un fallo de un rack, preread o materialización aborta toda T. No
se degrada a una transacción por vista o rack. Imports previos siguen siendo infraestructura sin RackId/payload/vistas.

## 18. UNDO

UNDO de un éxito debe retirar conjuntamente payload durable, todas las vistas, identidad, grouping y cualquier estado
RackCad esencial creado por el espejo. No puede dejar autoridad sin vistas ni vistas sin autoridad. Imports PREPARE
pueden permanecer sólo como infraestructura compartible. Esta propiedad necesita prueba física exact-SHA; una
compensación posterior no sustituye UNDO atómico.

## 19. SAVE/reopen

Para cada kind admitido:

```text
D' ≡ D1 (same-T preread)
   ≡ D2 (fresh postcommit read)
   ≡ D3 (SAVE + close + reopen)
```

El mismo reader autoritativo y comparator deben medir D1, D2 y D3. Recover/partial load/ilegibilidad producen STOP.
Round-trip incluye identity, schema, metadata, `ExtensionData`, bindings, custom properties, addresses y hermanas.

## 20. V21-CE01..15

| CE | Prevención/detección | Rollback | Residual / implicación |
|---|---|---|---|
| V21-CE01 callback cambia destino antes de preread | preread de todas las hermanas | sí si pertenece a T/visible | foreign T no visible sigue abierta |
| V21-CE02 cambia tras preread antes de commit | no cerrado por preread | sólo si causa fallo en T | host characterization obligatoria |
| V21-CE03 callback durante commit | postread podría detectar | no atómico tras commit | host characterization obligatoria |
| V21-CE04 poscommit antes de retorno | postread según orden | no | debe probarse ausencia de callback posterior al read/punto de fin |
| V21-CE05 cambia fuente tras validación | source postread/fingerprint puede observar | no tras commit | definir serialización y afiliación transaccional |
| V21-CE06 cambia variable NOD | PlanReadSet preread/postread | sí antes; no después | misma T/DB o BLOCKER |
| V21-CE07 writer/reader comparten bug | fixtures/oráculos independientes | según punto | comparator y μ_k separados |
| V21-CE08 comparator omite `ExtensionData` | mutations include-by-default | sí si preread | test obligatorio |
| V21-CE09 sibling divergente | scan completo + comparator | sí precommit | no first/majority |
| V21-CE10 sibling faltante | expected vs actual exact set | sí precommit | extra también falla |
| V21-CE11 abort tras payload write | una T sin commit | sí para own writes | foreign transaction queda fuera de T |
| V21-CE12 UNDO deja payload | prueba física | no es éxito conforme | BLOCKER de Candidate |
| V21-CE13 save/reopen altera round-trip | D3 comparator | no en comando original | kind inadmisible/corrección requerida |
| V21-CE14 secondary T confirma foreign mutation | investigación dirigida | no se presume | BLOCKER si sobrevive rollback |
| V21-CE15 foreign write tras commit antes de read | postread detecta si ya visible | no | devuelve fallo committed; no éxito |

## 21. Preguntas de caracterización host dirigidas

`CT-DA-01..08` son preguntas acotadas, no `ContextIsolationAuthority` universal:

1. **CT-DA-01:** ¿Puede un handler same-context escribir el Xrecord RackCad durante la T del comando?
2. **CT-DA-02:** ¿La escritura queda afiliada a la misma T, a una nested T o puede confirmar independientemente?
3. **CT-DA-03:** ¿Qué eventos exactos disparan `RackBlockData.Write`, crear extension dictionary y cambiar `Xrecord.Data`?
4. **CT-DA-04:** ¿Ejecuta `Commit()` callbacks capaces de modificar payload, NOD, definición o referencias antes de retornar?
5. **CT-DA-05:** ¿Puede una secondary T confirmar una mutación que sobreviva al aborto de la T principal?
6. **CT-DA-06:** ¿Ve el reader real las escrituras staged de Xrecord/definiciones nuevas en la misma T?
7. **CT-DA-07:** Tras retornar `Commit()`, ¿qué callbacks síncronos pueden ocurrir antes de la fresh read o del retorno del comando?
8. **CT-DA-08:** ¿Una lectura postcommit en nueva T observa todos los efectos semánticos sin disparar otros mutadores?

La caracterización debe usar AutoCAD 2025/API `25.0.0.0`/build exacto, callbacks adversariales controlados, TRX/logs y
fault injection. Un resultado positivo sólo gobierna el tuple probado y los canales concretos; no enumera plugins ni
afirma aislamiento del proceso.

## 22. Reducción de garantía y Owner

| Opción | Evaluación |
|---|---|
| ALT-R1 own writes correct | insuficiente para autoridad durable |
| ALT-R2 detached correct aunque persisted corrupto | no es un producto RACKMIRROR correcto |
| ALT-R3 éxito sólo si final read coincide | útil, pero sin rollback poscommit y con carrera residual |

V21 no selecciona una reducción. Si CT-DA demuestra que foreign semantic writes pueden quedar confirmadas antes del
retorno sin prevención/detección completa, la arquitectura V20 no puede prometer `OverallSuccess`. Aceptar que el
comando reporte éxito ya corrupto sería una reducción material y requeriría Proposal y decisión Owner explícitas; no
se infiere aceptación.

## 23. Alternativas V21 y selección

| Alternativa | Resultado |
|---|---|
| ALT-21A V20 + preread suficiente | **REJECTED**: última carrera/commit callbacks |
| ALT-21B pre/post verificación suficiente con residual | **REJECTED AS SUFFICIENT**: detecta sin rollback y deja ventana |
| ALT-21C válida sólo con host characterization de payload | **SELECTED / RESTRICTED / PENDING** |
| ALT-21D reducir garantía | `NOT SELECTED`; exige Owner y no cierra producto |
| ALT-21E rechazar V20 | fallback si CT-DA falla; no seleccionado todavía |

V20 queda **preservada pero restringida**. El blocker durable sigue abierto hasta que CT-DA pruebe las propiedades
necesarias o refute la arquitectura. V21 no afirma que la caracterización vaya a resultar favorable.

## 24. Autocrítica obligatoria

| Pregunta | Respuesta |
|---|---|
| Q1 corrupción antes del retorno sin detección | **Sí, posible/no refutada** con autoridad actual. |
| Q2 preread cierra última carrera | **No.** |
| Q3 commit puede disparar mutación | **UNKNOWN; no puede excluirse.** |
| Q4 postread detecta sin rollback | **Sí**, si visible; atomicidad ya perdida. |
| Q5 foreign T sobrevive rollback | **UNKNOWN; pregunta CT-DA decisiva.** |
| Q6 source cambia después de S | Puede serializarse después sólo si es operación independiente; same-command sigue abierto. |
| Q7 bug compartido | **Sí**; exige oráculos/fixtures independientes. |
| Q8 todas las hermanas verificables | Diseñable; hoy sólo Selective tiene comparator demostrado. |
| Q9 UNDO/save preservan autoridad | **UNKNOWN** hasta prueba física. |
| Q10 CIA renombrada | **No**: CT-DA pregunta sólo por Xrecords/transacciones/callbacks en una ventana y host exactos. |

## 25. Disposición V18 / Freeze / O-1 / ADR

Hasta consenso V21 y evidencia posterior:

```text
V18 = GOVERNING
Freeze V18 = GOVERNING
O-1 V18 = GOVERNING
ADR-0036 = PROPOSED / AMENDED FOR V18
```

Aunque V21 se acuerde, no habilita implementación. La secuencia es: exact review V21 → consenso → nuevo Freeze →
decisión Owner sobre la garantía → enmienda ADR → reapertura explícita de G3 → CT-DA dirigida → decisión del resultado
→ gates/implementación posteriores. Si el nuevo Freeze condiciona la arquitectura a CT-DA, un resultado adverso lo
invalida y exige volver a Proposal/Owner según el cambio.

## 26. Regla G3

G3 permanece STOPPED. V21 sólo define qué caracterizar después de una reapertura explícita autorizada. No ejecuta
CT-49R, CT-DA, G3B o CT-50. Ninguna CI documental constituye evidencia del host.

## 27. I-55

I-55 avanzó de `fa8f6affa769f87afe795fd310fbc74c43494123` a
`67779989233ef190810d2c5a61e1f1e499e31983` sólo con el cierre documental G9b. El producto G9b confirma barrido único,
gates, una transacción/commit para redraw, modo de huérfanas y fail-closed por AUTH-13 no demostrado.

Clasificación fresca: impacto contractual I-52 `NON-MATERIAL`; coordinación `MATERIAL`; seam transaccional
`POTENTIALLY REUSABLE`. No prueba protección de payload frente a callbacks, read-your-own-writes, commit-time mutation
o postcommit equality; no implementa AUTH-15.

## 28. Invalidadores

Invalidan ALT-21C: foreign semantic write que confirme fuera de T; callback de commit no observable; reader same-T que
no vea bytes staged; postread que dispare mutación; comparator incompleto; sibling omitida; NOD fuera de T/DB; UNDO
parcial; round-trip save/reopen divergente; kind sin comparator; cualquier API/materializador que escriba authored
state en POST; o CT-DA sin casos adversariales y tuple exacto.

## 29. Checklist de revisión

- [ ] Refutar que la autoridad durable sea capa distinta de materialización.
- [ ] Intentar cerrar o demostrar imposible la carrera después del preread.
- [ ] Revisar callbacks/eventos durante `Commit()` y secondary transactions.
- [ ] Verificar que postread evita éxito falso detectable sin fingir rollback.
- [ ] Revisar serialization point fuente y ventana destino hasta retorno.
- [ ] Verificar todos los campos authored y todas las hermanas.
- [ ] Confirmar independencia writer/reader/comparator/`μ_k`.
- [ ] Confirmar readiness por kind sin fallback.
- [ ] Verificar NOD, multi-rack todo-o-nada, UNDO y SAVE/reopen.
- [ ] Intentar demostrar que CT-DA renombra aislamiento universal; si lo hace, exigir rechazo.
- [ ] Confirmar que V18/Freeze/O-1/ADR siguen gobernando.
- [ ] Confirmar que I-55 no transfiere evidencia de protección semántica.

```text
PROPOSAL V21 = PUBLISHED / REVIEW REQUIRED
V20 ARCHITECTURE = PRESERVED / TARGETED HOST CHARACTERIZATION REQUIRED
TECHNICAL CONSENSUS = NOT REACHED
G3 = STOPPED
G3B = NOT OPEN
CT-50 = NOT EXECUTED
SUBSTANTIVE IMPLEMENTATION = BLOCKED
```
