# I-62 — Disposición del Coordinator sobre la revisión técnica de la enmienda A-1 (registro)

```text
Emisor:           Coordinator («I-62 — COORDINATOR DISPOSITION OF A-1 REVIEW»; texto pegado, sin archivo de origen; 12 549 bytes en UTF-8;
                  SHA-256 fdd36ee74b6525c296f614deba809e5769ddff92718285cf0c3f646e41d38885; recibido 2026-10-05T03:46Z; decisiones §38)
Revisión dispuesta: corrida R20261003T023945Z-cab7 sobre docs/initiatives/I-62-A-1.md, blob 09ca93285975c4b7af6471d6ae91bfa12c94a1fc (commit bf7b0d9c);
                  custodiada en 576006e83ad56f50df6c816f18f95297589de399 (registro: I-62-architect-review-A-1.md)
Acreditación:     FORMAL_ACCREDITATION = NOT_ACCREDITED
Hallazgos:        A62-A1-01..06 = ACCEPTED REQUIRED (adoptados por el Coordinator); O1..O5 = ACCEPT; O6 = DO NOT ACCEPT
Objeto corregido: docs/initiatives/I-62-A-1.md, blob 23dd16b2b135cdb7e1e2b1e18e2b6595253e92d8 (sigue siendo A-1 y PROPUESTA)
Estado:           A-1 = CHANGES REQUIRED / CORRECTION AUTHORIZED → corrección publicada; Architect REQUIRED — PENDING (revisión formal nueva, sin
                  autorización todavía); Coordinator PENDING; F4 producción BLOCKED; Owner NOT REQUIRED; I-61 vigente
```

Registro redactado por la sesión autora como custodia de la disposición. Resume fielmente la orden; si hay discrepancia, manda el texto de la orden. La
sesión no dispone nada por su cuenta.

## 1. Acreditación de la revisión

La corrida custodiada en `576006e8` **no está acreditada formalmente**: hubo búsquedas reales sobre directorios fuera del cierre y acciones fuera del contrato
de acciones acotado de la revisión. Por eso **no cuenta** como el veredicto formal del Architect que exige LIFECYCLE §6, y no debe presentarse como el
acuerdo del Architect.

El Coordinator constata también:
- no hubo ninguna escritura prohibida;
- la fidelidad de los insumos canónicos se conservó;
- ninguna premisa de los hallazgos depende de material fuera del cierre;
- los seis hallazgos técnicos identifican contradicciones concretas, reproducibles desde los insumos canónicos de A-1, V14 y F3.

Por eso **adopta A62-A1-01..06 como correcciones técnicas REQUIRED**. No se repite la revisión del blob actual: primero se corrige A-1, y el objeto exacto
corregido recibe una revisión formal nueva del Architect.

## 2. Disposición por hallazgo y corrección exigida

| Hallazgo | Disposición | Corrección exigida por el Coordinator (resumen fiel) |
|---|---|---|
| A62-A1-01 | ACCEPTED REQUIRED | identidad inmutable del bucle ARCHITECT_REVIEW (`loop.instance_id` / `budget_key`); una instancia, una entrada de presupuesto; una autorización sustituta, enmendada o renovada no crea entrada ni reinicia contadores; cada solicitud apunta a la identidad del bucle, nunca a la autorización vigente como clave; los límites efectivos nunca superan los más estrictos aplicables y una sustitución puede estrecharlos, nunca restaurar lo consumido; invariantes y pruebas exactas |
| A62-A1-02 | ACCEPTED REQUIRED | separar identidad del bucle y autorización vigente; con EXPIRED o REVOKED no hay acción nueva bajo esa autorización. Dos caminos: (A) cerrar el bucle sin trabajo vivo y con la escalada resuelta, conservando el `ended_reason` (nunca reescribirlo a SUPERSEDED para poder cerrar); (B) continuar el mismo bucle con una autorización ligada a la misma instancia y clave, sin entrada nueva y sin subir los límites; positivos y negativos en C-38 y C-40 |
| A62-A1-03 | ACCEPTED REQUIRED | los cambios de presupuesto de FC-01 solo para `loop.type` = ARCHITECT_REVIEW; REVIEWER (contrato de gate, §20.7) y EXECUTION conservan su autoridad y su semántica; ninguna dependencia de `budgets[]`, `ReviewLoopAuthorization`, `architect_launches` o `review_rounds` del Architect; aplicabilidad por tipo de cualquier cierre genérico; prueba de que un REVIEWER autorizado por su contrato de gate sigue siendo representable sin `ReviewLoopAuthorization` |
| A62-A1-04 | ACCEPTED REQUIRED | en la reconciliación, un intento en LAUNCHING conserva su invocación y su `Target` como intención histórica y registra en el `RebaseMap` la imagen probada del `Target`; después: arrancó → LAUNCHED, `Target` original acreditado; no arrancó → BUDGET_RESERVED en la misma reserva, `InvocationId` nuevo, invocación reconstruida sobre la imagen, mismo `reserved_at`, sin consumir otro lanzamiento; indeterminado → LAUNCH_UNCERTAIN con conteo conservador; I-H02 sin requisito de ancestría imposible para el `Target` histórico; C-15 y C-29 para las tres ramas |
| A62-A1-05 | ACCEPTED REQUIRED | predicado estrecho `EquivalentReviewedObject(A, B, RebaseHistory)`: mismo `path`, mismo `blob` y mismo commit o relación de imagen probada por la cadena de `RebaseMap` custodiada; nunca por parches, ruta, árbol o SHA sin prueba; usarlo donde §20.5.2 e I-S18 comparan `Target`, `EvaluatedObject`, objeto de la solicitud y `loop.object` tras un rebase probado; negativos: otro `blob` → INVALID; imagen no probada → según la semántica de fallo congelada; cobertura en C-15, C-29 y C-38 |
| A62-A1-06 | ACCEPTED REQUIRED | enmienda explícita de la ancestría de las referencias de rama custodiadas (`BindingRef.Location.Commit`, `AuthorizationRef.Commit`, `AuthorityRevision` y demás); sin reescribir artefactos históricos; resolver determinista `ResolveBranchRef(reference, RebaseHistory)` (ancestro de HEAD, o imagen probada por una cadena completa con el mismo `path` y `blob`, ancestro de HEAD); sin correspondencias adivinadas; una invocación nueva o replanificada reconstruye sus referencias vivas sobre las imágenes; A-1 declara que sustituye para I-62 la lectura de ancestro crudo del texto F3 materializado aplicable; sin cambio de forma de esquema salvo que la implementación lo pruebe; pruebas con uno y dos rebases, mapa intermedio ausente, blob cambiado y clon sucesor limpio |

| OPTIONAL | Disposición | Contenido |
|---|---|---|
| A62-A1-O1 | ACCEPT | regla de par de la apertura (exactamente una entrada nueva) |
| A62-A1-O2 | ACCEPT | delta explícito de `BudgetSnapshot`: la entrada identificada por la clave inmutable del bucle, no la autorización vigente |
| A62-A1-O3 | ACCEPT | linajes de unidad; cerrar el bucle no cierra un REQUIRED; el bucle nuevo recibe todo linaje REQUIRED abierto aplicable en `OpenFindings` |
| A62-A1-O4 | ACCEPT | el fin histórico de la autorización se puede recuperar aunque `loop.action_validity` pase a `null`; definir de dónde se lee |
| A62-A1-O5 | ACCEPT | reparar el arnés: al menos una traza exacta por A62-A1-01..06; `fc01-enmendado-object-null-fuera-del-cierre` debe fallar específicamente por `loop.object` → `null` fuera de LOOP_CLOSED; las trazas prueban la enmienda tal como está escrita, con `loop.type`, escalada, linajes, autorización sustituta y caminos EXPIRED/REVOKED |
| A62-A1-O6 | DO NOT ACCEPT | M-01 sigue en no: A-1 no cambia la propiedad de ninguna regla o valor ni crea una segunda autoridad; el Coordinator ya es dueño de las decisiones de `ReviewLoopAuthorization` |

**Materialidad:** M-02 = M-03 = M-04 = M-05 = sí; M-01 = M-06 = M-07 = M-08 = no, salvo que el diseño corregido revele otro hecho. No hace falta ninguna
decisión del Owner.

**Identidad de la enmienda:** A-1 sigue PROPUESTA y nunca fue AGREED. Se corrige el mismo artefacto con un blob nuevo, sin crear A-2; el blob anterior queda
como historia en Git. Una A-2 solo hace falta cuando una A-1 ya es enmienda acordada.

## 3. Matriz exacta de disposición → corrección

Sobre `docs/initiatives/I-62-A-1.md`, blob `23dd16b2…`. Trazas de
[a1-counterexamples-result.json](../automation/evidence/I-62-A1/a1-counterexamples-result.json): 56, todas PASS; cada negativa con su conjunto exacto de reglas.

| Hallazgo | Deltas de la A-1 corregida | Reglas del arnés | Trazas (todas PASS) |
|---|---|---|---|
| A62-A1-01 | D1-1 identidad del bucle; D1-2 `architect_budgets[]`; D1-3 `loop_instance_id`; D1-5 I-S18; D1-6 I-P13 contadores; D1-8 sustitución; D1-11 §20.6; D1-12 `ContinuesLoopInstanceId` | A1-F01, A1-F02, A1-F03, A1-F04, A1-P03, A1-P05, A1-P06, A1-P14 | `a62-a1-01-sustitucion-continua-la-misma-entrada`, `-sustitucion-crea-entrada-nueva`, `-sustitucion-reinicia-contadores`, `-sustitucion-sube-topes`, `-instance-id-distinto-del-QU-de-apertura`, `-solicitud-cambia-de-bucle`, `fc01-enmendado-reinicia-A1` |
| A62-A1-02 | D1-8 continuación sin reescribir EXPIRED/REVOKED; D1-9 LOOP_CLOSED; D1-12 marcador de cierre | A1-P05, A1-P06, A1-P07, A1-P13 | `a62-a1-02-expirada-y-cerrada`, `-revocada-y-cerrada`, `-cierre-que-revoca-la-vigente`, `-expirada-y-continuada`, `-reescribe-EXPIRED-como-SUPERSEDED`, `-continuacion-crea-entrada`, `-accion-nueva-con-vigencia-terminada`, `-cierre-sin-decision`, `-cierre-con-escalada-sin-resolver`, `fc01-enmendado-cerrar-y-abrir-con-A2`, `fc01-enmendado-cierra-con-solicitud-abierta` |
| A62-A1-03 | D1-4 alcance de `budgets`; D1-10 `loop.object` por tipo; D1-13 alcance por tipo | A1-F06, A1-F07, A1-P07 | `a62-a1-03-reviewer-autorizado-por-contrato-de-gate` (positivo: representable sin `ReviewLoopAuthorization`), `-reviewer-con-identidad-de-architect`, `-architect-sin-entrada`, `-budgets-de-V14-cuenta-una-solicitud-del-architect`, `-LOOP_CLOSED-no-se-extiende-a-REVIEWER` |
| A62-A1-04 | D2-1, D2-2 (fila LAUNCHING), D2-3, D2-6 (tres ramas) | A1-P08, A1-P09, I-H02 | `a62-a1-04-LAUNCHING-tras-rebase-arranco`, `a62-a1-04-LAUNCHING-tras-rebase-no-arranco`, `a62-a1-04-LAUNCHING-tras-rebase-incierto`, `-no-arranco-sin-replanificar`, `-LAUNCHING-reescrito-en-la-reconciliacion`, `-LAUNCHING-sin-imagen-en-el-mapa` |
| A62-A1-05 | D2-12 EquivalentReviewedObject | A1-P12 | `a62-a1-05-ingesta-de-un-LAUNCHED-tras-rebase`, `-ingesta-con-otro-blob`, `-ingesta-con-imagen-no-probada`, `fc02-enmendado-LAUNCHED-conserva-su-Target` |
| A62-A1-06 | D2-2 (reconstrucción), D2-9 `rebase_history[]`, D2-10 ResolveBranchRef, D2-11 aplicación a B.2, I-S18 y tres pasajes F3 | A1-F09, A1-F10, A1-P11, A1-P15 | `a62-a1-06-referencias-tras-un-rebase`, `-referencias-tras-dos-rebases`, `-sucesor-en-clon-limpio`, `-mapa-intermedio-ausente`, `-blob-cambiado`, `-replanificada-con-referencias-originales`, `-historia-de-mapas-sin-last_rebase-al-final`, `-historia-cambia-sin-reconciliacion`, `fc06-literal-referencias-tras-rebase`, `fc02-enmendado-reconcilia` |
| A62-A1-O1 | D1-7 | A1-P04 | `fc01-enmendado-reutiliza-A1`, `fc01-enmendado-cerrar-y-abrir-con-A2` |
| A62-A1-O2 | D1-14 | A1-P17 | `a62-a1-o2-snapshot-por-autorizacion`, `a62-a1-01-sustitucion-continua-la-misma-entrada` |
| A62-A1-O3 | D1-15 | A1-P16 | `a62-a1-o3-bucle-nuevo-hereda-linajes-abiertos`, `a62-a1-o3-bucle-nuevo-sin-linajes-heredados`, `a62-a1-02-expirada-y-cerrada` |
| A62-A1-O4 | D1-2 (`authorizations[]`, `closed_at`, `closed_by`), D1-9 (fin histórico) | A1-P07 | `a62-a1-02-expirada-y-cerrada`, `-revocada-y-cerrada`, `-cierre-que-revoca-la-vigente` |
| A62-A1-O5 | evidencia: arnés reescrito con conjuntos exactos y cobertura obligatoria | todas | `fc01-enmendado-object-null-fuera-del-cierre` falla solo por A1-P02 |
| A62-A1-O6 | ninguno | — | — |

**Contratos F3:** ninguno necesita cambio de esquema. `RebaseMap.Commits[]` (`relay-record/v2`) ya contiene la cadena; `BudgetSnapshot`
(`role-invocation/v1`) sigue siendo escalar. Los campos nuevos son de `state/v2`, aún sin materializar. Se enmiendan solo **textos** F3 para I-62 (D2-11 y la
lista de 16.20 en D1-12).

**Observación fuera del delta:** OBS-A1-01 (A-1 §7): V14 no define la salida a NONE de un bucle REVIEWER, y A-1 la conserva por la regla del Coordinator.

## 4. Lo que este registro no hace

No acredita la revisión anterior ni la presenta como acuerdo del Architect. No declara AGREED, no lanza la revisión formal nueva (necesita otra autorización),
no crea A-2, no implementa F4 y no decide nada del Owner.
