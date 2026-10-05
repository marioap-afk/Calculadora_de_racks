# I-62 — Paquete de revisión del Architect (enmienda A-1 corregida: FC-01, FC-02, OBS-A1-01, A62-A1R-01..03, A62-A1A-01, A62-A1S-01..02 y A62-A1T-01)

```text
A-1            = PROPUESTA (versión corregida) — sin revisión formal acreditada
Revisión previa = la corrida R20261003T023945Z-cab7 sobre 09ca9328 NO quedó FORMALMENTE ACREDITADA; el Coordinator adoptó sus hallazgos técnicos
                 A62-A1-01..06 como REQUIRED (decisiones §38) y añadió OBS-A1-01 como REQUIRED / MATERIAL (decisiones §39). Esa corrida es solo
                 insumo técnico histórico. La revisión formal R20261005T044948Z-ac67 sobre 9c621fce (decisiones §40) dio CHANGES REQUIRED; el
                 Coordinator adoptó A62-A1R-01..03 como REQUIRED técnicos (decisiones §41). El autor halló y corrigió A62-A1A-01. La revisión formal
                 R20261005T063359Z-86e3 sobre 39c2f831 dio CHANGES REQUIRED (A62-A1S-01..02, O1..O4), corregidos en 03dd822d. La revisión formal
                 R20261005T073911Z-2dfe sobre 03dd822d dio CHANGES REQUIRED (A62-A1T-01, O1..O3) y dejó CLOSED los doce cierres anteriores; queda
                 FORMAL_ACCREDITATION = NOT_ACCREDITED (auditor v3.1, resultado literal conservado). El Coordinator adoptó A62-A1T-01 como ACCEPTED
                 REQUIRED y O1..O3 (orden nueva del Coordinator, decisiones §42)
Architect      = REVIEW REQUIRED: una revisión formal acreditada de la A-1 corregida exacta (cambio MATERIAL: LIFECYCLE §6 exige Architect + Coordinator)
Coordinator    = veredicto PENDING
Owner          = sin decisión identificada; si aparece una consecuencia OWNER-RESERVED, la enmienda se detiene
Invocación     = UNA, autorizada por la orden nueva del Coordinator (decisiones §42), solo con un transporte limpio; sin reintento automático; si la
                 única vía limpia exige el clic del Owner: HUMAN_LAUNCH_REQUIRED
Implementación = producción de F4 BLOCKED hasta el veredicto del Architect, el del Coordinator y la orden de apertura

Objeto de la revisión (identidad por contenido; el commit lo da el recibo de publicación):
  docs/initiatives/I-62-A-1.md                                          blob c01899a72b940503bb85a0fab42bc085c603fd0f
Blobs anteriores (historia; commits anteriores al rebase del 2026-10-05):  09ca93285975c4b7af6471d6ae91bfa12c94a1fc (bf7b0d9c)
                                                                        23dd16b2b135cdb7e1e2b1e18e2b6595253e92d8 (fff3bbb0)
                                                                        9c621fce0588f32115e3151b6f75413c0a167c2e (0ad410f8, imagen 4e36d77c)
                                                                        39c2f8317ec381fa60c3564a834278df8898101c (252be61e, 9dcfc08d)
                                                                        cdcd98d2752d384272285536b3874ae45c9d9832 (e15ecc35, d97ce3d0)
Blob anterior revisado (posterior al rebase):                           03dd822d1310a0ce298eba6da2321383aa5e4887 (411e01ce)
Freeze que enmienda:
  FREEZE_SHA b64a3b640c7ee3bd77636e7a3218ae0ebac2dd43
  docs/initiatives/I-62-proposal-v14.md   commit 4c617e82b32b6c810b68d75fc19472efed22b393   blob 34ad80ea1bfff144bfc5169f62920a4c904c1bfa
Base de main: bb0d5522e8411f66a51fdfb3f1f0d0514b737453 (rama rebasada el 2026-10-05; mapa en docs/automation/evidence/I-62-prep/night-2026-10-05/rebase-map.json)
```

> **Identidad exacta.** El revisor comprueba que `git rev-parse <commit>:docs/initiatives/I-62-A-1.md` = `c01899a7…` en el commit del recibo de publicación.
> Si no coincide, revisa la versión designada o rechaza la discordancia. Este paquete no lleva su propio blob.

## 1. Veredicto que se solicita (LIFECYCLE §5 y §6)

```text
Resultado: AGREED | CHANGES REQUIRED | BLOCKED — OWNER DECISION
Hallazgos: REQUIRED | OPTIONAL; ID, sección exacta, premisa canónica completa (PremiseRefs con líneas), autoridad o contraejemplo, por qué importa, corrección.
Cierre:    disposición explícita de A62-A1T-01 (CLOSED | STILL_OPEN) y de las precisiones A62-A1T-O1..O3 sobre la versión exacta; y ratificación de la
           conservación de los doce cierres anteriores: A62-A1-01..06, OBS-A1-01, A62-A1R-01..03 y A62-A1S-01..02 (CLOSED | STILL_OPEN).
Además:    necesidad o no de una decisión del Owner; compatibilidad con los contratos F3 materializados.
Modo:      declarado (SAME-SESSION ROLE | SEPARATE SESSION | EXTERNAL HUMAN) y si revisor y autor son la misma persona; contexto inyectado declarado.
Identidad: commit, ruta y blob revisados.
```

AGREED sobre la A-1 exacta, más el veredicto del Coordinator, la convierte en enmienda acordada del Freeze y desbloquea la materialización de F4 afectada.
Un REQUIRED abierto lo cierra o lo rebaja solo quien lo emitió o quien tenga esa autoridad. La sesión no declara ningún veredicto.

## 2. Lectura (insumos canónicos)

| Insumo | Blob | Para qué |
|---|---|---|
| `docs/initiatives/I-62-A-1.md` | `c01899a7…` | el objeto completo: cláusulas anteriores, deltas D1-1..D1-21 y D2-1..D2-12, materialidad, pruebas, matriz (§9) y cambios frente a los blobs anteriores (§10, empezando por «Frente a `03dd822d`») |
| delta exacto: `git diff 411e01ce <commit> -- docs/initiatives/I-62-A-1.md` | — | el cambio frente al blob revisado `03dd822d` (los dos commits están en el clon) |
| `docs/initiatives/I-62-architect-review-A-1-r4.md` y `docs/automation/evidence/I-62-architect-A-1/R20261005T073911Z-2dfe/output.json` | `2c486e9f…`, `be8bb819…` | registro y resultado literal de la revisión formal anterior (A62-A1T-01, O1..O3 y la disposición de los doce cierres) |
| `docs/initiatives/I-62-architect-review-A-1-disposition.md` | (en el mismo commit) | disposiciones del Coordinator, con la de A62-A1T-01 y O1..O3 (§5) |
| `docs/initiatives/I-62-architect-review-A-1.md` | `18987532…` | registro de la revisión técnica anterior (hallazgos A62-A1-01..06, con sus premisas) |
| `docs/initiatives/I-62-proposal-v14.md` en `4c617e82` | `34ad80ea…` | cláusulas enmendadas: §8.8, §8.9, §20.5, §20.5.1, §20.5.2, §20.6, §20.7, B.2, B.8.1, B.8.4 (I-S17, I-P05, I-P10, I-H02), B.8.7, B.8.8 (I-S18, I-P13), B.9 |
| `docs/AUTOMATION_PLAN.md` §16.20 (líneas 846-925) | `52fd8f66…` | texto F3 materializado: lista de la autorización y paso 3 (D1-12, D2-11) |
| `docs/automation/agent-execution/README.md` §14.3 y §14.4 (líneas 405-438) | `3fb9a44b…` | texto F3 materializado: «Reproducción» y regla 3 de A2' (D2-11) |
| `docs/automation/agent-execution/schemas/relay-record.v2.schema.json` (`RebaseMap`) | `dca5b29c…` | la cadena de mapas ya está en `Commits[]` (sin cambio de esquema) |
| `docs/automation/agent-execution/schemas/role-invocation.v1.schema.json` (`BudgetSnapshot`) | `d54a7ae7…` | escalares sin cambio (D1-14) |
| `docs/automation/agent-execution/schemas/reviewer-result.v1.schema.json` | `a74288eb…` | `Disposition` y `Severity` (BLOCKING \| ADVISORY) sin cambio (D1-17) |
| `docs/automation/agent-execution/schemas/gate-contract.v2.schema.json` (`RoleRequirements`) | `f644bb19…` | autoridad del REVIEWER sin cambio; ningún campo endurece B.10.2 (D1-17) |
| `docs/automation/evidence/I-62-A1/a1-counterexamples.py` | `a1f4b07f…` | arnés de trazas: V14 literal y A-1 corregida; conjuntos exactos de reglas |
| `docs/automation/evidence/I-62-A1/a1-counterexamples-result.json` | `ac77596e…` | 118 trazas (39 VALID y 79 INVALID), todas PASS; cobertura por hallazgo, también A62-A1T-01, O1 y O3 |
| `docs/automation/evidence/I-62-A1/a1t01/` (`red-result.json`, `green-result.json`, `green.diff`, `t8-clean-clone.py`, `t8-result.json`) | (en el mismo commit) | RED de A1-P08 antes de la corrección, GREEN después, diff de la regla y T8 con Git real en un clon limpio |
| `docs/automation/evidence/I-62-prep/night-2026-10-05/f4-exp/combo-result-a1t01.json` | (en el mismo commit) | secuencias combinadas de F4 experimental con el arnés corregido (17/17); `combo-result.json` es la ejecución anterior, sin reescribir |
| `docs/INITIATIVE_LIFECYCLE.md` §3, §5 y §6 | `f19896a8…` | formato de A-n, REQUIRED y M-01..M-08 |
| `docs/automation/decisions/I-62.md` §34, §35 y §38-§42 | (en el mismo commit) | clasificación, autorizaciones, disposiciones del Coordinator y la orden nueva que adopta A62-A1T-01 |

Para leer las cláusulas de V14 basta su sección: el insumo es un archivo grande, y no hace falta expandir los documentos que su prosa cita.

## 3. Resumen del delta corregido

**Cuarta revisión (A62-A1T-01 y O1..O3), el foco de esta revisión.**
- **D2-2 (fila LAUNCHING, ahora «D2-2 (cont.)»):** el `Target` conservado de un intento en LAUNCHING ya no tiene que figurar en el mapa del rebase en curso
  ni ser ancestro de `main_before`. Debe resolver por ResolveBranchRef (D2-10) con la historia completa y ordenada de mapas de n, incluido el mapa de esta
  reconciliación, y la punta rebasada que n consume. Con M1: X → X' y M2: X' → X'', X resuelve a X'' con el mismo `Path` y `Blob`.
- **Fallo cerrado:** sin M1 o sin un eslabón, o con otro blob, UNRESOLVED y STOP sin publicar. No basta el último mapa, ni la igualdad de path, el
  parecido de parches o una sustitución de SHA sin prueba. El mapa nuevo solo contiene los commits reescritos por su rebase, nunca una entrada compuesta.
- **Sin efecto sobre el intento:** resolver acredita la referencia histórica y no decide si el proceso arrancó. La invocación, el `Target` original, el
  estado, `RunId`, `reserved_at`, `BudgetSnapshot`, contadores, fase y linajes no cambian. D2-8 nombra ahora `run_id` y `BudgetSnapshot`.
- **Sin autorreferencia ni punto extra:** la comprobación usa la historia candidata de n y la punta rebasada, nunca el commit todavía inexistente de n.
- **D2-6 sin cambio:** sus tres ramas siguen decidiendo el destino.
- D2-3, D2-9 y D2-11 nombran esa resolución. C-15 gana (d2)/(e2) (dos rebases, también en una toma) y C-29 (i) la reconstrucción por un sucesor.
- **O1:** D1-17 dice «abiertas en el par de apertura o después» y añade la reconstrucción por `last_request`. La fila se parte en dos por longitud.
- **O2:** §1, §5, §7, §8, §9 y §11 de A-1, este paquete, la descripción de A1-P02 y la etiqueta «Asserted» de F4 experimental, al día.
- **O3:** C-38 (cuarta revisión) gana el negativo de una autoridad SUPERSEDED, y A1-R05 lo comprueba. C-15 (i) gana el A2' de EXECUTION a través de uno y de
  dos rebases (C-14 es el delta de fallos de G.2).

**FC-01 (solo ARCHITECT_REVIEW).**
- Identidad inmutable del bucle, `loop.instance_id` = `ARL-<record_version>`, que es también la clave del presupuesto.
- `architect_budgets[]` lleva una entrada por bucle, con su historia de autorizaciones, topes mínimos que nunca suben y metadatos de cierre.
- Sustitución, enmienda y continuación dentro del bucle (`ContinuesLoopInstanceId`) siguen en la misma entrada y sin reinicio. EXPIRED y REVOKED nunca se
  reescriben.
- LOOP_CLOSED cierra desde cualquier fase sin trabajo vivo, con la decisión exigida.
- REVIEWER y EXECUTION conservan la semántica de V14 salvo los deltas declarados: los de OBS-A1-01 para REVIEWER, y para EXECUTION los dos de §1
  (reconciliación de `loop.object` si no es `null`; D2-11 en sus referencias de binding).
- `BudgetSnapshot` copia la entrada del bucle, y `OpenFindings` incluye los linajes heredados.

**OBS-A1-01 (solo REVIEWER).**
- `REVIEWER_SATISFIED`, como fase y como fin de vigencia, con una guarda de tipo: ARCHITECT_SATISFIED nunca se aplica a un REVIEWER.
- Se alcanza solo con un resultado VALID ingerido, sin intentos no terminales en las solicitudes del bucle (abiertas en su par de apertura o después, con
  cualquier autoridad) y sin ningún BLOCKING de REVIEWER abierto en la unidad, tampoco heredado ni en STILL_OPEN; ADVISORY no bloquea con `gate-contract/v2`.
- LOOP_CLOSED de REVIEWER por dos caminos: (S) desde REVIEWER_SATISFIED, sin decisión; (E) tras EXHAUSTED, EXPIRED o REVOKED, con la decisión
  `I62-REVIEWER-LOOP-CLOSE`, conservando el motivo.
- El fin histórico queda en `reviewer_closures[]`, append-only.
- Sin identidad de bucle, sin `ReviewLoopAuthorization` y sin `architect_budgets[]`; `budgets` sin cambio de modelo.

**Segunda revisión (A62-A1R-01..03 y opcionales).**
- (E) también tras EXHAUSTED, y con la vigencia OPEN revocada por la misma decisión.
- Una regla de rebase por tipo: el `commit` de `loop.object` pasa a la imagen en REBASE_RECONCILIATION para los tres tipos. El delta de EXECUTION queda
  declarado.
- D1-20: identidad de la autoridad REVIEWER por la `StateRef` del contrato; apertura sin resucitar autoridades terminadas.
- D1-21: enmienda de semántica propuesta de 16.20 y del criterio VALIDITY de §14.3. M-05 de OBS-A1-01 = sí.
- Enumeración exacta por tipo; `OpenFindings` según la autoridad del revisor; cada mapa de rebase en orden; satisfacción solo con evidencia REVIEWER_SATISFIED.

**Corrección del autor (A62-A1A-01) y tercera revisión (A62-A1S-01..02).** D1-17 (3) y D1-18 (S) cuentan todo BLOCKING con `issuer` REVIEWER de la
unidad, también los heredados y los STILL_OPEN; la pertenencia al bucle REVIEWER va por apertura, bajo cualquier autoridad que lo haya gobernado; y la
cláusula REVIEWER de D1-10 conserva CORRECTING → PUBLISHED de V14, con la re-revisión sobre el objeto corregido.

**FC-02.**
- Los commits vivos de la orquestación entran en `StateFields` y en I-H02, y los intentos no lanzados se replanifican sobre las imágenes con la invocación
  reconstruida entera.
- Un intento en LAUNCHING conserva su invocación; su `Target` se acredita por ResolveBranchRef en cada reconciliación, y su destino lo deciden las tres
  ramas de arranque.
- `custody.rebase_history[]` hace alcanzable la cadena de mapas.
- `ResolveBranchRef` sustituye la lectura de ancestro crudo para las referencias de rama, también en tres pasajes F3 para I-62.
- `EquivalentReviewedObject` compara objetos revisados a través de un rebase probado.

## 4. Preguntas para la revisión

**Lo que la revisión debe cubrir de forma explícita** (orden nueva del Coordinator, decisiones §42):
- (a) A62-A1T-01 y las regresiones del cambio (foco mínimo; preguntas 17 y 19);
- (b) las precisiones A62-A1T-O1..O3 (pregunta 20);
- (c) la conservación de los doce cierres anteriores: A62-A1-01..06, OBS-A1-01, A62-A1R-01..03 y A62-A1S-01..02, sin reabrirlos solo porque figuran en
  el historial (pregunta 21);
- (d) la necesidad o no de una decisión del Owner (pregunta 9);
- (e) la compatibilidad con los contratos F3 materializados (preguntas 8 y 22).

No se prohíbe señalar otro defecto material. Las corridas anteriores no acreditadas son solo insumo técnico histórico.

1. **Identidad del bucle.** ¿Basta `ARL-<record_version del QU de apertura>` como identidad inmutable, independiente del rebase, y como clave única del
   presupuesto (una sola identidad en lugar de dos campos)?
2. **Topes efectivos.** D1-2 toma el mínimo entre las constantes congeladas y el `Budget` de **todas** las autorizaciones que han gobernado el bucle, también
   las terminadas. ¿Es la lectura correcta de «los límites más estrictos aplicables»?
3. **Continuación.** D1-8 la admite solo tras EXPIRED o REVOKED (o sustituyendo una autorización OPEN). Tras ARCHITECT_SATISFIED o EXHAUSTED solo cabe
   LOOP_CLOSED. ¿Falta algún camino legítimo?
4. **LOOP_CLOSED desde ARCHITECT_SATISFIED** sin decisión del Coordinator (D1-9). ¿Es aceptable, dado que no crea autoridad y el bucle ya terminó?
5. **REVIEWER.** D1-4 deja en `budgets` solo las solicitudes sin `loop_instance_id`, para no contar dos veces las del Architect. ¿Es el cambio mínimo
   necesario? Sobre OBS-A1-01 (salida a NONE de un bucle REVIEWER, que V14 no define): ¿la cubren D1-16..D1-21?
6. **ResolveBranchRef.** ¿Es correcta y suficiente la regla de la cadena (D2-10), con un paso no reescrito aceptado solo si es ancestro de `MainBeforeSha`,
   y con UNRESOLVED con la semántica de fallo congelada?
7. **Equivalencia.** D2-12 deja B.10.0 en igualdad cruda (resultado frente al `Target` de su propia invocación). ¿Hay alguna comparación tras un rebase que
   no esté cubierta?
8. **Textos F3.** ¿Son los tres pasajes de D2-11 y la lista de 16.20 de D1-12 todos los textos F3 materializados que necesitan la enmienda para I-62?
9. **Materialidad y Owner.** ¿Coinciden las dos tablas de §4 (M-01 = no; para OBS-A1-01, M-05 = sí)? ¿Hay alguna consecuencia OWNER-RESERVED?
10. **Finalización del REVIEWER.** ¿Son suficientes las condiciones de REVIEWER_SATISFIED (D1-17 y D1-17 (cont.))? ¿Es correcto que, con
    `gate-contract/v2`, un ADVISORY nunca bloquee, porque ningún campo permite endurecerlo?
11. **Cierre (E) del REVIEWER.** D1-18 deja cerrar tras EXHAUSTED, EXPIRED o REVOKED aunque quede un BLOCKING abierto: el linaje sigue abierto y el requisito operativo
    queda sin satisfacer, así que la operación dependiente sigue bloqueada. ¿Es la lectura correcta de «no quedar sin cierre para siempre»?
12. **Pertenencia al bucle REVIEWER.** Sin identidad de bucle, D1-17 identifica las solicitudes del bucle por su apertura: las de `loop_instance_id` =
    `null` abiertas en el par NONE → REVIEW_PENDING del bucle o después, bajo cualquier autoridad que lo haya gobernado, y, sin la historia de pares, las
    posteriores a la `last_request` del último registro de `reviewer_closures[]`. Con D1-20 una autoridad terminada no se reabre. ¿Es determinista y
    suficiente?
13. **A62-A1R-01..03.** ¿Siguen cerrados por D1-18, D1-10/D1-13/D2-4 y D1-20/D1-21 sin abrir otro camino?
14. **Identidad de la autoridad REVIEWER (D1-20).** ¿Es correcto derivarla de la `StateRef` del contrato de gate (`{path, blob}` + rol), que no cambia con
    un rebase y cambia con una reemisión?
15. **Deltas de EXECUTION.** §1 declara dos: (a) la reconciliación de su `loop.object` si no es `null` (por V14 lo es en las fases de §8) y (b) D2-11 en
    las referencias de binding de EXECUTION (A2' de `Executor.BindingRef`). ¿Son aceptables, frente a la alternativa de fijar `loop.object` en `null`?
16. **A62-A1A-01.** ¿Es correcto que D1-17 (3) cuente todo BLOCKING con `issuer` REVIEWER de la unidad (los que D1-15 pone en `OpenFindings`), y no solo
    los abiertos en una solicitud del bucle?
17. **A62-A1T-01 (antes, la observación de la pregunta 17).** ¿Cierra la nueva fila «D2-2 (cont.)» el bloqueo con un segundo rebase y el intento todavía en
    LAUNCHING, también en una toma (F.7) y en una sesión nueva que rebasa al abrir? ¿Es correcto que la resolución use `n.custody.rebase_history[]` (la
    historia candidata, que ya incluye el mapa nuevo) y la punta rebasada como HEAD, sin referirse al commit de n?
18. **A62-A1S-01..02.** ¿Siguen cerrados por la pertenencia por apertura de D1-17, el conjunto de (3) con STILL_OPEN y la cláusula REVIEWER de D1-10?
19. **Regresiones de A62-A1T-01.** ¿Mantiene la fila corregida el fallo cerrado (sin un eslabón o con otro blob, UNRESOLVED y STOP), la inmutabilidad del
    intento (invocación, `Target`, estado, `RunId`, `reserved_at`, `BudgetSnapshot`, contadores, fase y linajes; D2-7 y D2-8) y las tres ramas de D2-6 sin
    cambio? En el arnés, RED (antes de corregir A1-P08) falla solo en las 12 trazas que cruzan el segundo rebase, y solo por A1-P08; GREEN pasa las 118.
    T8 lo repite con Git real: X y X' son inalcanzables en un clon limpio, y el sucesor resuelve X → X'' solo desde lo custodiado.
20. **A62-A1T-O1..O3.** ¿Recogen D1-17 (O1), las secciones al día (O2), C-38 (cuarta revisión), C-15 (i) y A1-R05 (O3) lo que pedía la revisión, sin
    cambio semántico material fuera del defecto dispuesto?
21. **Doce cierres.** ¿Conserva esta versión las disposiciones CLOSED de A62-A1-01..06, OBS-A1-01, A62-A1R-01..03 y A62-A1S-01..02?
22. **F3.** ¿Siguen sin cambio de esquema `relay-record/v2` (la cadena está en `Commits[]`), `role-invocation/v1`, `reviewer-result/v1` y `gate-contract/v2`?

## 5. Condiciones de la invocación (para quien la autorice)

- **Frontera:** la orden nueva del Coordinator (decisiones §42, punto E) autoriza UNA invocación del Architect sobre este objeto exacto, solo con un
  transporte limpio, disponible y autorizado; sin reintento automático, sin Worker, Controller ni subagentes adicionales, con los topes de tiempo y
  consumo del transporte. Las invocaciones de la orden nocturna (decisiones §41) están consumidas. Si la única vía limpia exige el clic del Owner, la
  sesión prepara la acción una vez y registra HUMAN_LAUNCH_REQUIRED; no afirma que la revisión arrancó.
- **Protocolo:** I-61 y WORKFLOW siguen gobernando; la invocación sigue sus reglas y las del registro de la revisión (LIFECYCLE §5). OD-2 y OD-3 no están
  resueltas: la invocación no usa una celda bloqueada ni cambia credenciales o configuración.
- **Contrato de acciones nuevo (orden §7), fijado antes de invocar:** navegación a la raíz y a subdirectorios del clon (cambiar de directorio no autoriza
  leer fuera del cierre); lecturas y búsquedas solo sobre archivos concretos del cierre en el commit fijado; comandos de metadatos y hashing enumerados;
  scripts temporales propios solo en el scratchpad de la corrida, incluido `sed -i` declarado; lectura de las salidas propias. Ningún permiso sobre
  salidas propias autoriza editar insumos canónicos, scripts custodiados, fuentes del repositorio ni archivos de otra sesión.
- **Auditoría nueva:** rutas efectivas y escapes por enlaces, no solo prefijos de texto; rutas del sistema de archivos distintas de las rutas de objetos
  Git; un `git show` rechazado por Git es una lectura fallida y no acredita contenido. El auditor se prueba antes del lanzamiento con los dos bucles `for`
  de la corrida anterior, `cd` a un subdirectorio, la edición de un script propio, el rechazo de una edición canónica, el rechazo de una lectura fuera
  del cierre y comandos fallidos sin contenido. El cierre y las acciones no se amplían después de ejecutar.
- **Lecciones de la corrida R20261005T073911Z-2dfe (evidencia §54):** dos `cd` a subdirectorios y un `sed -i` sobre una salida propia eran desviaciones
  literales del contrato anterior; el auditor v3.1 analizaba mal el bucle `for` y no normalizaba `..`. El contrato y el auditor nuevos lo prevén.
- **Independencia:** el revisor no es la sesión autora (la sesión principal de I-62) ni comparte su contexto; se declaran el modo y si revisor y autor son la
  misma persona.
- **Insumos:** los de §2, en el commit del recibo de publicación, sin la transcripción ni la memoria de la sesión autora.

## 6. Lo que este paquete no hace

- no aplica A-1: ningún texto congelado, materializado o de producción cambia;
- no declara ningún veredicto ni presenta las corridas anteriores como acuerdo del Architect;
- no materializa F4 ni ninguna parte de `state/v2` o de la orquestación;
- no crea A-2 ni otra A-n; no absorbe P4/P8 de I-63; no repara la deuda nc2 de I-64 (unidad I61).
