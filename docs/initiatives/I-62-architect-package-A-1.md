# I-62 — Paquete de revisión del Architect (enmienda A-1 corregida: FC-01, FC-02, OBS-A1-01, A62-A1R-01..03, A62-A1A-01 y A62-A1S-01..02)

```text
A-1            = PROPUESTA (versión corregida) — sin revisión formal acreditada
Revisión previa = la corrida R20261003T023945Z-cab7 sobre 09ca9328 NO quedó FORMALMENTE ACREDITADA; el Coordinator adoptó sus hallazgos técnicos
                 A62-A1-01..06 como REQUIRED (decisiones §38) y añadió OBS-A1-01 como REQUIRED / MATERIAL (decisiones §39). Esa corrida es solo
                 insumo técnico histórico. La revisión formal R20261005T044948Z-ac67 sobre 9c621fce (decisiones §40) dio CHANGES REQUIRED; su
                 acreditación la decide el Coordinator, que adoptó A62-A1R-01..03 como REQUIRED técnicos (decisiones §41). Antes de lanzar la
                 revisión siguiente, el autor halló y corrigió A62-A1A-01 (un BLOCKING heredado no impedía REVIEWER_SATISFIED). La revisión formal
                 R20261005T063359Z-86e3 sobre 39c2f831 dio CHANGES REQUIRED (A62-A1S-01..02, O1..O4); su acreditación la decide el Coordinator
Architect      = REVIEW REQUIRED: una revisión formal acreditada de la A-1 corregida exacta (cambio MATERIAL: LIFECYCLE §6 exige Architect + Coordinator)
Coordinator    = veredicto PENDING
Owner          = sin decisión identificada; si aparece una consecuencia OWNER-RESERVED, la enmienda se detiene
Invocación     = autorizada por la orden nocturna (decisiones §41: hasta dos invocaciones, solo con un transporte limpio); el único transporte limpio
                 exige el clic del Owner: HUMAN_LAUNCH_REQUIRED
Implementación = producción de F4 BLOCKED hasta el veredicto del Architect, el del Coordinator y la orden de apertura

Objeto de la revisión (identidad por contenido; el commit lo da el recibo de publicación):
  docs/initiatives/I-62-A-1.md                                          blob 03dd822d1310a0ce298eba6da2321383aa5e4887
Blobs anteriores (historia; commits anteriores al rebase del 2026-10-05):  09ca93285975c4b7af6471d6ae91bfa12c94a1fc (bf7b0d9c)
                                                                        23dd16b2b135cdb7e1e2b1e18e2b6595253e92d8 (fff3bbb0)
                                                                        9c621fce0588f32115e3151b6f75413c0a167c2e (0ad410f8, imagen 4e36d77c)
                                                                        39c2f8317ec381fa60c3564a834278df8898101c (252be61e, 9dcfc08d)
                                                                        cdcd98d2752d384272285536b3874ae45c9d9832 (e15ecc35, d97ce3d0)
Freeze que enmienda:
  FREEZE_SHA b64a3b640c7ee3bd77636e7a3218ae0ebac2dd43
  docs/initiatives/I-62-proposal-v14.md   commit 4c617e82b32b6c810b68d75fc19472efed22b393   blob 34ad80ea1bfff144bfc5169f62920a4c904c1bfa
Base de main: bb0d5522e8411f66a51fdfb3f1f0d0514b737453 (rama rebasada el 2026-10-05; mapa en docs/automation/evidence/I-62-prep/night-2026-10-05/rebase-map.json)
```

> **Identidad exacta.** El revisor comprueba que `git rev-parse <commit>:docs/initiatives/I-62-A-1.md` = `03dd822d…` en el commit del recibo de publicación.
> Si no coincide, revisa la versión designada o rechaza la discordancia. Este paquete no lleva su propio blob.

## 1. Veredicto que se solicita (LIFECYCLE §5 y §6)

```text
Resultado: AGREED | CHANGES REQUIRED | BLOCKED — OWNER DECISION
Hallazgos: REQUIRED | OPTIONAL; ID, sección exacta, premisa canónica completa (PremiseRefs con líneas), autoridad o contraejemplo, por qué importa, corrección.
Cierre:    disposición explícita de A62-A1-01..06, OBS-A1-01 y A62-A1R-01..03 (CLOSED | STILL_OPEN) sobre la versión exacta.
Modo:      declarado (SAME-SESSION ROLE | SEPARATE SESSION | EXTERNAL HUMAN) y si revisor y autor son la misma persona; contexto inyectado declarado.
Identidad: commit, ruta y blob revisados.
```

AGREED sobre la A-1 exacta, más el veredicto del Coordinator, la convierte en enmienda acordada del Freeze y desbloquea la materialización de F4 afectada.
Un REQUIRED abierto lo cierra o lo rebaja solo quien lo emitió o quien tenga esa autoridad. La sesión no declara ningún veredicto.

## 2. Lectura (insumos canónicos)

| Insumo | Blob | Para qué |
|---|---|---|
| `docs/initiatives/I-62-A-1.md` | `03dd822d…` | el objeto: cláusulas anteriores, deltas D1-1..D1-21 y D2-1..D2-12, materialidad (también la propia de OBS-A1-01), pruebas, matriz (§9) y cambios frente a los blobs anteriores (§10) |
| `docs/initiatives/I-62-architect-review-A-1-r2.md` y `docs/automation/evidence/I-62-architect-A-1/R20261005T044948Z-ac67/output.json` | (en el mismo commit) | registro y resultado literal de la revisión formal anterior (A62-A1R-01..03 y sus OPTIONAL) |
| `docs/initiatives/I-62-architect-review-A-1-disposition.md` | `eeb29ba0…` | las dos disposiciones del Coordinator: acreditación, hallazgos adoptados, OPTIONAL, OBS-A1-01 y matriz exacta |
| `docs/initiatives/I-62-architect-review-A-1.md` | `18987532…` | registro de la revisión técnica anterior (hallazgos A62-A1-01..06, con sus premisas) |
| `docs/initiatives/I-62-proposal-v14.md` en `4c617e82` | `34ad80ea…` | cláusulas enmendadas: §8.8, §20.5, §20.5.1, §20.5.2, §20.6, §20.7, B.2, B.8.1, B.8.4 (I-S17, I-P05, I-P10, I-H02), B.8.7, B.8.8 (I-S18, I-P13), B.9 |
| `docs/AUTOMATION_PLAN.md` §16.20 (líneas 846-925) | `52fd8f66…` | texto F3 materializado: lista de la autorización y paso 3 (D1-12, D2-11) |
| `docs/automation/agent-execution/README.md` §14.3 y §14.4 (líneas 405-438) | `3fb9a44b…` | texto F3 materializado: «Reproducción» y regla 3 de A2' (D2-11) |
| `docs/automation/agent-execution/schemas/relay-record.v2.schema.json` (`RebaseMap`) | `dca5b29c…` | la cadena de mapas ya está en `Commits[]` (sin cambio de esquema) |
| `docs/automation/agent-execution/schemas/role-invocation.v1.schema.json` (`BudgetSnapshot`) | `d54a7ae7…` | escalares sin cambio (D1-14) |
| `docs/automation/agent-execution/schemas/reviewer-result.v1.schema.json` | `a74288eb…` | `Disposition` y `Severity` (BLOCKING \| ADVISORY) sin cambio (D1-17) |
| `docs/automation/agent-execution/schemas/gate-contract.v2.schema.json` (`RoleRequirements`) | `f644bb19…` | autoridad del REVIEWER sin cambio; ningún campo endurece B.10.2 (D1-17) |
| `docs/automation/evidence/I-62-A1/a1-counterexamples.py` | `6b051db7…` | arnés de trazas: V14 literal y A-1 corregida; conjuntos exactos de reglas |
| `docs/automation/evidence/I-62-A1/a1-counterexamples-result.json` | `71c46ef9…` | 100 trazas (34 VALID y 66 INVALID), todas PASS; cobertura por hallazgo, R1..R10 de OBS-A1-01 y A62-A1R-01..03 con O1..O5 |
| `docs/INITIATIVE_LIFECYCLE.md` §3, §5 y §6 | `f19896a8…` | formato de A-n, REQUIRED y M-01..M-08 |
| `docs/automation/decisions/I-62.md` §34, §35 y §38-§41 | (en el mismo commit) | clasificación, autorizaciones, disposiciones del Coordinator y la orden nocturna que adopta A62-A1R-01..03 |

Para leer las cláusulas de V14 basta su sección: el insumo es un archivo grande, y no hace falta expandir los documentos que su prosa cita.

## 3. Resumen del delta corregido

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
- Se alcanza solo con un resultado VALID ingerido, sin intentos no terminales en las solicitudes del bucle (abiertas tras su apertura, con cualquier
  autoridad) y sin ningún BLOCKING de REVIEWER abierto en la unidad, tampoco heredado ni en STILL_OPEN; ADVISORY no bloquea con `gate-contract/v2`.
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

**Corrección del autor (A62-A1A-01).** Las secuencias combinadas de F4 experimental (orden nocturna §9) mostraron que D1-17 (3) solo contaba los BLOCKING
«abiertos en una solicitud del bucle»: un bucle REVIEWER nuevo, con otra autoridad, podía llegar a REVIEWER_SATISFIED con un BLOCKING heredado abierto,
en contra de B.10.2 y de lo que D1-18 dice del cierre (E). Ahora D1-17 (3) y D1-18 (S) cuentan todo BLOCKING con `issuer` REVIEWER de la unidad.

**FC-02.**
- Los commits vivos de la orquestación entran en `StateFields` y en I-H02, y los intentos no lanzados se replanifican sobre las imágenes con la invocación
  reconstruida entera.
- Un intento en LAUNCHING conserva su invocación, y su destino lo deciden las tres ramas de arranque.
- `custody.rebase_history[]` hace alcanzable la cadena de mapas.
- `ResolveBranchRef` sustituye la lectura de ancestro crudo para las referencias de rama, también en tres pasajes F3 para I-62.
- `EquivalentReviewedObject` compara objetos revisados a través de un rebase probado.

## 4. Preguntas para la revisión

**Lo que la revisión debe cubrir de forma explícita** (órdenes del Coordinator, decisiones §39 y §41):
- (a) las correcciones de A62-A1-01..06;
- (b) la corrección de OBS-A1-01;
- (c) la separación de tipos ARCHITECT_REVIEW, REVIEWER y EXECUTION;
- (d) que no haya un reinicio oculto de los presupuestos del REVIEWER ni del Architect;
- (e) que cada bucle de revisión aplicable se pueda cerrar sin crear autoridad;
- (f) la compatibilidad con los contratos F3 materializados;
- (g) A62-A1R-01..03 y los opcionales O1..O5 de la revisión formal R20261005T044948Z-ac67 (preguntas 13-15);
- (h) la corrección del autor A62-A1A-01 y la observación sobre un segundo rebase con un intento en LAUNCHING (preguntas 16 y 17);
- (i) A62-A1S-01..02 y O1..O4 de la revisión formal R20261005T063359Z-86e3 (pregunta 18).

La corrida anterior no acreditada es solo insumo técnico histórico.

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
10. **Finalización del REVIEWER.** ¿Son suficientes las condiciones de REVIEWER_SATISFIED (D1-17)? ¿Es correcto que, con `gate-contract/v2`, un ADVISORY
    nunca bloquee, porque ningún campo permite endurecerlo?
11. **Cierre (E) del REVIEWER.** D1-18 deja cerrar tras EXHAUSTED, EXPIRED o REVOKED aunque quede un BLOCKING abierto: el linaje sigue abierto y el requisito operativo
    queda sin satisfacer, así que la operación dependiente sigue bloqueada. ¿Es la lectura correcta de «no quedar sin cierre para siempre»?
12. **Pertenencia al bucle REVIEWER.** Sin identidad de bucle, D1-17 identifica las solicitudes del bucle por la `Authorization` de su invocación (el contrato
    de gate). Con D1-20 esa autoridad no se reabre. ¿Es determinista y suficiente?
13. **A62-A1R-01..03.** ¿Cierran D1-18, D1-10/D1-13/D2-4 y D1-20/D1-21 los tres REQUIRED de la revisión formal anterior sin abrir otro camino?
14. **Identidad de la autoridad REVIEWER (D1-20).** ¿Es correcto derivarla de la `StateRef` del contrato de gate (`{path, blob}` + rol), que no cambia con
    un rebase y cambia con una reemisión?
15. **Delta de EXECUTION.** ¿Es aceptable declarar como único delta para EXECUTION la reconciliación de su `loop.object` no nulo, frente a la alternativa de
    fijarlo en `null`?

16. **A62-A1A-01.** ¿Es correcto que D1-17 (3) cuente todo BLOCKING con `issuer` REVIEWER de la unidad (los que D1-15 pone en `OpenFindings`), y no solo
    los abiertos en una solicitud del bucle? Sin esto, un bucle nuevo con otra autoridad alcanzaba REVIEWER_SATISFIED con un BLOCKING heredado abierto.
17. **Segundo rebase con un intento en LAUNCHING (observación, sin cambio en A-1).** D2-2 exige que el `Target.commit` de un intento en LAUNCHING figure
    en el `RebaseMap` nuevo o sea ancestro de `main_before`. Tras un primer rebase, el `Target` conservado es el commit original, que el segundo mapa ya
    no contiene: el segundo rebase para sin publicar hasta que la evidencia de arranque resuelva el intento. ¿Es el comportamiento querido (conservador),
    o debe resolverse el `Target` por la cadena con ResolveBranchRef (D2-10)? Las secuencias de F4 experimental documentan los dos caminos.

18. **A62-A1S-01..02.** ¿Cierran la pertenencia por apertura de D1-17, el conjunto de (3) con STILL_OPEN y la cláusula REVIEWER de D1-10 (que conserva
    CORRECTING → PUBLISHED) los dos REQUIRED de la revisión formal R20261005T063359Z-86e3 sin abrir otro camino? En el arnés, A1-R02 y A1-R03 usan la
    pertenencia por apertura, y A1-P18 exige que una solicitud nueva se abra sobre `loop.object`.

## 5. Condiciones de la invocación (para quien la autorice)

- **Frontera:** la orden nocturna (decisiones §41, punto E) autoriza hasta dos invocaciones del Architect para revisar estas correcciones, solo con un
  transporte limpio; la autorización de decisiones §40 se consumió en R20261005T044948Z-ac67. El único transporte limpio es una tarea de la app que
  lanza el Owner con un clic (HUMAN_LAUNCH_REQUIRED); la sesión prepara el kit una vez y sigue con otras tareas.
- **Protocolo:** I-61 sigue siendo el activo; la invocación sigue sus reglas y las del registro de la revisión (LIFECYCLE §5). OD-2 y OD-3 no están
  resueltas: la invocación no usa los runtimes que bloquean.
- **Lecciones de la revisión formal R20261005T044948Z-ac67 (evidencia §43):** la tabla de acciones del prompt y la lista blanca del auditor deben ser
  la misma, de modo que `cd` hacia el clon, `| sha256sum` sobre salidas de RD-1 y `git -C <worktree>` figuren en ambas o en ninguna; el auditor debe
  tratar heredocs y variables de shell y admitir el proyecto de `dotnet test`. Ya lo hace la versión v2.1, custodiada.
- **Lecciones de la corrida no acreditada (evidencia §40):**
  - el árbol de trabajo del revisor se fija en el commit exacto, ya sea con `git checkout --detach <commit>` como primer paso obligatorio o con la rama por
    defecto del clon en ese commit; la tarea anterior abrió el worktree sobre `main`;
  - Grep solo sobre **archivos** del cierre, nunca sobre un directorio, aunque no devuelva contenido;
  - las acciones permitidas se enumeran de forma cerrada; si se quieren comandos de metadatos de solo lectura (`wc`, `awk`, `grep` por shell) sobre
    archivos del cierre, la autorización los lista;
  - la auditoría posterior acepta comandos compuestos de solo lectura sobre rutas del cierre y acciones permitidas con cualquier herramienta de shell
    (corrige los falsos positivos de la corrida anterior).
- **Independencia:** el revisor no es la sesión autora (la sesión principal de I-62) ni comparte su contexto; se declaran el modo y si revisor y autor son la
  misma persona.
- **Insumos:** los de §2, en el commit del recibo de publicación, sin la transcripción ni la memoria de la sesión autora.

## 6. Lo que este paquete no hace

- no aplica A-1: ningún texto congelado, materializado o de producción cambia;
- no declara ningún veredicto ni presenta la corrida anterior como acuerdo del Architect;
- no materializa F4 ni ninguna parte de `state/v2` o de la orquestación;
- no crea A-2 ni otra A-n; no repara la deuda nc2 de I-64 (unidad I61).
