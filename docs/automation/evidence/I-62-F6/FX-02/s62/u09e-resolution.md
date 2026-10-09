# U-09 (e) — Quién decide A1'-A8' de la delegación `d` dentro de la ventana (decisiones §62, punto 3)

> Preparación de la supervisión (plano a), una sola pasada, 2026-10-09. Nada se decide aquí. No se recomienda ninguna opción ni ningún modo de
> permisos.
>
> Fuentes (worktree `architecture-portabilidad-coordinador-principal`, HEAD `cca8c60e`): V14 §8.4, §9.2 (T3/T3') y F.1; AP 16.1, 16.5, 16.22,
> 16.25 y 16.26; RAE §4, §10, §14.2, §14.6 y §14.7; esquema `relay-record.v2`; A-4 `0d954376` (A4-3 y §3.7); clasificación de FX-02, fila #9;
> revisión 1 de A-4 (A62-A4-O7).
>
> Mandato (§62.3): «**U-09 (e):** resolución acreditada de A1'-A8', sin decisión ficticia ni adelanto de resultados desconocidos, revisada junto con
> A4-3».

## 1. Texto aplicable (literal)

**Quién decide:**
- V14 §9.2, T3: «tras planificar | aceptación A1'-A8' | registros válidos | **Coordinator** | diario: aceptación de `d` (**delegación abierta**) | sin
  escritura Git».
- V14 §9.2, T3': «rechazo en la aceptación | alguna de A1'-A8' en fail | **Coordinator** | diario; Q7 con cierre NOT_ACCEPTED | ningún Worker».
- AP 16.26, fila T1-T7: «cesiones al Controller y al Worker, aceptación A1'-A8', … | **P / Coordinator** | diario … | sin escritura Git dentro de la
  ventana».
- AP 16.26, fila T3': «Coordinator».

**Cuándo y cómo:**
- V14 F.1, paso 3: «| 3 | P, Coordinator | sin escritura | binding W, preflight W, aceptación A1'-A8' de d, nc4 | **ACCEPTED_OPEN** | — |».
- V14 §8.4, transitorio entre Q0 y Q7: «delegación `d` y su aceptación».
- AP 16.25, W-2: «entre el Q0 y el cierre no hay escrituras Git de la sesión».

**Quién evalúa:**
- AP 16.5: «Antes de invocar al Worker, el Coordinator **evalúa** todas las comprobaciones, sin cortocircuito; la disposición es la más grave de las
  fallidas, y ningún fallo invoca al Worker».
- RAE §4: «el Coordinator evalúa las ocho comprobaciones sin cortocircuito y anota cada resultado en `Outcome.Acceptance`».
- RAE §14.7: A1'-A8' sobre §4 (A7': «el binding del ejecutor está aceptado (§14.2 B1-B10…) y su preflight sigue vigente»).
- RAE §10, nc4: «antes de aceptar la real».

**Qué se registra:** en el esquema `relay-record.v2`, `Outcome.Acceptance` = `{A1..A8: pass|fail}`, sin campo de decisor ni `DecisionRef`. B7-B9
(`Basis`, `DecisionRef`, «la nombra») son del binding, no de `d`.

**AP 16.1 (I62):**
- PRINCIPAL_COORDINATOR «Nunca declara: … GATE PASS salvo SAME-SESSION ROLE de Coordinator declarado».
- «El Coordinator de gates no es vinculable».

**A-4 `0d954376`:**
- A4-3: aceptación preautorizada de los bindings de W y del Controller de verificación. Regla 2: «El titular aplica la decisión publicada; no
  decide ni dispensa nada». Regla 3: «Sin Worker aceptado, A7' falla y rige T3'».
- §3.7 (A62-A4-O7): «La aceptación A1'-A8' de `d` (T3) y su rechazo (T3') quedan fuera de A4-3: los fija el Coordinator antes del Q0, en la orden
  (punto 7 de O4), y dentro de la ventana el titular solo los aplica y registra; con esa decisión es ejecutable la remisión a T3' de la regla 3 de
  A4-3».

## 2. Diagnóstico

- **Antes del Q0 no existe `d`.** La produce la planificación del Controller dentro de la ventana. Tampoco existen su `RunId` de planificación, el
  `MainSha` recién obtenido ni el `BindingId` del Worker.
- **Lo que sí se conoce antes del Q0:** el contrato de T1 (`628d89af…`), los contadores del Q0, los marcadores de A4-3 y el directorio de
  `ExpectedHandoffPath`.
- **Consecuencia.** Lo único que puede decidirse antes del Q0 es una **regla**, nunca un **resultado**. Leída como fijación del resultado, la nota de
  §3.7 sería la «decisión ficticia» o el «adelanto de resultados desconocidos» que excluye §62.3. Leída como fijación de una regla, hay que mostrar
  dos cosas:
  - que la salida de la regla depende solo de resultados mecánicos observados;
  - que es admisible que **evalúe** el titular, y no el Coordinator (AP 16.5; RAE §4).
- **Las ocho comprobaciones son mecánicas:**
  - A1': `Test-Json`;
  - A2': igualdades, `RunId` COMPLETED no usado y `BindingRef` (§14.4);
  - A3'-A5': inclusiones, G4 y G5;
  - A6': igualdades frente a hechos remotos registrados, y G3;
  - A7': binding aceptado y preflight vigente;
  - A8': igualdades y `CountersSnapshot`.

  Ninguna exige criterio. Eso hace posible una regla cuya salida no depende de juicio.
- **El obstáculo de A4-3 aquí no existe.** Para los bindings, el obstáculo era B7-B9 (`DecisionRef` que nombra el binding), «Sin aceptación fingida»
  y W-2. Para `d` no hay `DecisionRef` ni `Basis` (el esquema no los tiene), ni una cláusula de «aceptación fingida». Lo que queda es la
  atribución de T3/T3' y de la evaluación al Coordinator, y el estado previo «tras planificar».
- **Requisito común de reproducibilidad.** `Outcome.Acceptance` solo guarda pass/fail. La evidencia de cada comprobación tiene que poder
  reconstruirse desde lo que custodie el Q7 por el manifiesto: `d`, contrato, binding y preflight del ejecutor, registro de la planificación,
  `RemoteFacts` del instante (A6'), `CountersSnapshot` y el registro de nc4.

## 3. Opciones

### Opción 1 — Regla condicionada del Coordinator en la orden, aplicada por el titular (la nota O7 hecha explícita)

**Antes del Q0**, en el punto 7 de O4 (`{IN_WINDOW_ACCEPTANCE_RULE}`), el Coordinator del fixture publica un marcador que nombra la unidad, la tarea
y la ventana (`seq`), y fija:
1. las ocho comprobaciones con el procedimiento de RAE §4 y §14.7 y las entradas que se evaluarán, identificadas por rol (delegación del registro
   de planificación COMPLETED, contrato custodiado, binding del ejecutor aceptado por A4-3, contadores del Q0);
2. la evaluación sin cortocircuito;
3. **T3 si y solo si las ocho dan pass. Si no, T3'** con la disposición más grave de las fallidas (tabla de AP 16.5). No hay otro resultado posible,
   ni dispensa, ni reinterpretación. Una comprobación que no se puede establecer da fail;
4. nc4 sobre una copia, antes de la aceptación real (RAE §10), con su disposición confinada al control. La orden no revela su esperado;
5. el registro de `Outcome.Acceptance` y de la evidencia de cada comprobación en el diario (transitorio), custodiados en el Q7;
6. la validación en el Q7 y en S29: se reproduce cada comprobación desde lo custodiado, y se comprueba que T3/T3' es la salida de la regla. Una
   comprobación que no se reproduce invalida la aceptación: S-04 por analogía con «campos con autoridad» de §14.2, y COORDINATOR_DECISION posterior
   según U-09 (d).

Frente a los criterios:
- **(a) Real:** la decisión real del Coordinator es la regla, tomada con conocimiento antes del Q0. El resultado no está decidido de antemano. El
  registro dice «aplicada la regla de O4.7», no «decidido por el Coordinator tras verlo».
- **(b) Solo resultados observados:** sí, por construcción.
- **(c) Reproducible en el Q7:** sí.
- **(d) A4-3:** misma estructura (§4).

**¿Enmienda?** Encaja en una cláusula existente **solo con una lectura expresa del Coordinator** (LIFECYCLE §10) que dé por cumplidos dos
puntos:
- el «Decide: Coordinator» de T3/T3' con una decisión condicionada previa;
- el «el Coordinator evalúa» de AP 16.5 y RAE §4 con la evaluación mecánica del titular bajo esa decisión.

Argumentos a favor de esa lectura:
- F.1 paso 3 pone a «P, Coordinator» como actores sin escritura;
- `d` no tiene `DecisionRef`;
- la revisión 1 calificó O7 de OPTIONAL y «no es un defecto de A-4»;
- la re-revisión examina U-09 (e) (§62.2).

Argumentos en contra:
- el estado previo de T3 es «tras planificar»;
- la evaluación cambia de actor;
- el mecanismo análogo para los bindings (A4-3) se trató como material.

Si el Coordinator no adopta esa lectura, la opción es **material** (M-01: quién evalúa y decide; M-04: lo que falla). Iría en una A-n, porque A-4
está en revisión y no se modifica (§62.4).

### Opción 2 — Decisión del Coordinator del fixture dentro de la ventana, sin escritura Git (T3 y F.1 literales)

1. Tras planificar, el titular deja `d` en el diario y espera con la ventana abierta.
2. El Coordinator del fixture (la supervisión en ese papel) lee `d` y sus entradas en el área transitoria del clon y evalúa A1'-A8' y nc4.
3. El Coordinator decide T3 o T3' y deja la decisión como archivo transitorio en el directorio del intento. No es una escritura Git, así que W-2 no
   se aplica. Antes de que el titular siga, fija el SHA-256 de ese archivo en la evidencia de RackCad, como ancla de procedencia.
4. El titular sigue tras un «continúa» del Owner.

Frente a los criterios:
- **(a) Real:** sí; decide después de observar.
- **(b) Solo resultados observados:** solo si la disposición limita al Coordinator a la misma regla de la opción 1. Si no, admite juicio.
- **(c) Reproducible en el Q7:** con el archivo en el manifiesto y el ancla previa en RackCad. Desde Git solo no se prueba quién escribió un
  archivo transitorio.
- **(d) A4-3:** mezcla dos mecanismos en la misma ventana: los bindings por preautorización y `d` por decisión en vivo. Además, la remisión a T3'
  de A4-3, regla 3, también necesitaría al Coordinator en vivo, con otra pausa.

**¿Enmienda?** No para T3 (es el decisor literal). Pero choca con U-06 (a), ya dispuesta en §62 («la supervisión no envía mensajes a sesiones del
fixture»): un archivo que A2 tiene que leer es un canal hacia la sesión. Hace falta una disposición ordinaria del Coordinator que defina ese canal y
modifique U-06 (a).

Costes:
- un «continúa» más (AUTONOMY_GAP; según U-06 (c), no afecta a FX-02 PASS);
- un artefacto de la capa del fixture fuera de los esquemas canónicos (`relay-record.v2` no tiene campo de decisor).

### Opción 3 — SAME-SESSION ROLE de Coordinator, acotado a T3/T3' de la ventana

En O4, el Coordinator del fixture designa a A2 para ejercer el papel de Coordinator **solo** en la aceptación de `d` de la ventana `seq` k (AP 16.1:
SAME-SESSION ROLE declarado). A2 evalúa y decide en vivo.

Frente a los criterios:
- **(a) Real:** sí.
- **(b) Solo resultados observados:** solo si la designación lo limita a la regla de la opción 1.
- **(c) Reproducible en el Q7:** sí: `Outcome.Acceptance`, la evidencia custodiada y el marcador de designación en el árbol del Q0.
- **(d) A4-3:** postura opuesta. Para `d`, el titular sería decisor, y para los bindings A4-3, regla 2, dice que «no decide ni dispensa nada». La
  remisión a T3' de A4-3, regla 3, la decidiría el propio titular.

**¿Enmienda?** Probablemente **material** (M-01: cambia el decisor de T3 dentro de una ventana delegada). En I62, AP 16.1 solo menciona el
SAME-SESSION ROLE de Coordinator para GATE PASS, y dice que «El Coordinator de gates no es vinculable». Cabría sin enmienda solo con una lectura
expresa del Coordinator que lo extienda a T3. Además, la acumulación de papeles en A2 entra en la auditoría de S29 (U-26).

## 4. Revisión junto con A4-3

| A4-3 (bindings de W y del Controller de verificación) | Opción 1 (`d`) | Opción 2 | Opción 3 |
|---|---|---|---|
| R1: decisión individual antes del Q0, en la orden, con un marcador que nombra unidad, tarea, `seq`, rol, celdas y comprobaciones; sin `BindingId` | igual: regla antes del Q0 con unidad, tarea, `seq` y comprobaciones; sin el contenido de `d` | decisión dentro de la ventana | designación antes del Q0; decisión dentro de la ventana |
| R2: el titular registra PENDING, comprueba sin cortocircuito y aplica; «no decide ni dispensa nada» | igual | el titular solo registra; decide el Coordinator | el titular decide |
| R3: si falla, REJECTED y sin invocación; sin Worker aceptado rige T3' | T3' por la misma regla, ejecutable dentro de la ventana (lo que O7 exige) | T3' requiere otra decisión en vivo | T3' lo decide el titular |
| R4: transitorio hasta el Q7; quien valida reproduce; un marcador ausente o una comprobación que no se reproduce invalidan | igual (§3, opción 1, punto 6) | reproducible con el ancla en RackCad | igual que la opción 1 |
| R5: vale solo para la ventana nombrada; no amplía nada | igual | — | igual |
| Orden en la ventana (F.1 paso 3) | binding W (A4-3) → preflight W → A1'-A8' de `d` (A7' depende de A4-3) → nc4 antes de la aceptación real → T3/T3' | ídem, con pausa | ídem |

## 5. Para la disposición

- Las tres opciones cumplen (a). Para (b), la 2 y la 3 necesitan limitarse a la regla de la opción 1. La opción 1 es la que reproduce la estructura
  de A4-3, y la que A-4 §3.7 presupone para que la regla 3 de A4-3 sea ejecutable.
- **Material o no:**
  - opción 1: depende de una lectura expresa del Coordinator; si no la adopta, va en una A-n;
  - opción 2: no requiere A-n, pero sí una disposición sobre el canal y la modificación de U-06 (a);
  - opción 3: probablemente material.
- U-09 (e) bloquea S19 y el punto 7 de O4 (S02). La re-revisión de A-4 lo examina (§62.2), y su resultado puede informar la lectura de la opción 1
  sin que haga falta modificar A-4.
