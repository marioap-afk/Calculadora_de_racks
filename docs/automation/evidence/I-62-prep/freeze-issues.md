# I-62 — Posibles conflictos del Freeze hallados al preparar F4 (sin aplicar)

> **Preparación, no materialización.** Orden del Coordinator de [decisiones](../../decisions/I-62.md) §33 (clase B). Nada de este archivo cambia el Freeze,
> la Proposal V14 congelada (blob `34ad80ea`) ni ninguna autoridad. Las enmiendas son **candidatas**: solo existen si el Coordinator las dispone por
> LIFECYCLE §6, con el Architect cuando corresponda. F2 no las necesita; F3 tampoco. Afectan a la implementación de F4.

Método: auditoría de la máquina de estados de V14 (§8.1, §9.2, §20.5, §20.6, B.8.2, B.8.4 y B.8.8), transición por transición. La matriz completa está en
[f4-dossier.md](f4-dossier.md) §4.

## FC-01 — No hay un camino congelado para cerrar un bucle de revisión y abrir otro; los presupuestos son de toda la unidad

**Cláusulas** (V14, blob `34ad80ea`):
- B.8.8, `orchestration.budgets`: un solo objeto por unidad, sin clave de bucle ni de autorización;
- B.8.8, I-S18: «`architect_launches` = número de intentos con `reserved_at` ≠ `null`», calculado sobre `review_requests[]`, que es append-only;
- B.8.8, I-P13: «los contadores de `budgets` no decrecen»;
- I-P13: «`loop.object` cambia solo en el par CORRECTING → PUBLISHED … salvo NONE → REVIEW_PENDING al abrir el bucle»;
- §20.5 (estados): tras ARCHITECT_SATISFIED sigue «siguiente gate / decisión del Owner», sin transición de vuelta a NONE;
- §20.6: los topes (`MAX_REVIEW_ROUNDS` = 3, derivado de lanzamientos = 9) se definen por «versiones distintas del objeto»; la `ReviewLoopAuthorization`
  puede bajarlos, nunca subirlos sin A-n.

**Hecho.** Una unidad I62_DELEGATED puede necesitar más de un bucle del Architect: la revisión de diseño antes del Freeze y la conformidad de READY-06, o un
segundo objeto. Con el texto congelado:
1. no hay transición que devuelva `loop.phase` a NONE ni `loop.object` a `null` tras ARCHITECT_SATISFIED. Ese cambio de `loop.object` no está entre los
   permitidos por I-P13;
2. aunque se abriera un bucle nuevo, `review_rounds`, `logical_requests` y `architect_launches` vendrían ya consumidos del bucle anterior. La igualdad de
   I-S18 sobre `review_requests[]` impide reiniciarlos, y un tope agotado da P-18 en la primera reserva.

**Por qué es material.** Cambia la conducta del validador de `orchestration` (C-38) y de la recuperación (C-29): con el texto literal, el segundo bucle es
inalcanzable o nace agotado. No es una elección menor de implementación: lo fija una invariante.

**A-n candidata (opciones para la disposición del Coordinator; ninguna aplicada):**

| Opción | Cambio | Efecto |
|---|---|---|
| A (recomendada) | Presupuestos por autorización. `budgets` pasa a `budgets[] {authorization_id, …}` y las igualdades de I-S18 se calculan sobre las solicitudes de esa autorización. Se añade la transición `ARCHITECT_SATISFIED \| ESCALATE_OWNER resuelta \| EXHAUSTED → NONE`, que devuelve `loop.object` a `null` solo en ese par (excepción declarada en I-P13); el bucle nuevo exige una `ReviewLoopAuthorization` nueva | cada bucle tiene sus topes congelados; no hay reinicio silencioso: la autorización nueva es una decisión del Coordinator, y lo consumido sigue en la historia |
| B | Presupuestos de unidad, explícitos | un segundo bucle exige A-n para subir los topes; la conformidad de READY-06 quedaría sujeta a los restos del bucle de diseño |
| C | Un bucle por unidad | la conformidad de READY-06 no usaría el bucle autónomo y volvería al transporte manual (AUTONOMY_GAP) |

## FC-02 — Un rebase durante un bucle activo deja SHAs de rama en `orchestration` que ni se reconcilian ni se pueden actualizar

**Cláusulas:**
- B.8.7 y §8.8, `StateFields[]`: lista **cerrada** de los campos SHA del estado (`chain_base_sha`, `chain_red_sha`, `last_window.verified_sha`,
  `unverified_commits[].sha` y `last_evidence_commit`). No incluye `orchestration.loop.object.commit` ni `review_requests[].object.commit`;
- I-H02: comprueba la ancestría de esa misma lista;
- I-P05, excepción de REBASE_RECONCILIATION: solo puede cambiar «exactamente los campos SHA de `StateFields`»;
- I-P13: `loop.object` solo cambia en CORRECTING → PUBLISHED;
- B.9, `Target`: `{commit, path, blob}`, obligatorio en revisión; el revisor lee un commit exacto (§20.4, «un commit exacto»).

**Hecho.** Durante un bucle de revisión, `main` puede avanzar (es habitual con iniciativas hermanas) y WORKFLOW §4 obliga a rebasar al abrir una sesión. Tras el
rebase:
- `loop.object.commit` y los `object.commit` de las solicitudes abiertas son SHAs reescritos: el commit original ya no está en la rama publicada (force-push);
- la reconciliación no puede escribir su imagen (I-P05 la limita a `StateFields`, e I-P13 congela `loop.object`), e I-H02 no lo detecta;
- la invocación siguiente apuntaría a un commit que un clon limpio puede no obtener. Los blobs no cambian, y las comprobaciones de ARCHITECT_SATISFIED usan el
  blob (I-S18), pero `Target.commit` es obligatorio.

**Por qué es material.** Se trata de un hueco de SHA obsoleto en la máquina de estados: o se impide el rebase con un bucle activo (choca con WORKFLOW §4), o se
reconcilian esos campos (choca con I-P05 e I-P13).

**A-n candidata (sin aplicar):**
- añadir a `StateFields[]` (B.8.7), a la lista de I-H02 y a la tabla de §8.8 los campos `orchestration.loop.object.commit`,
  `orchestration.review_requests[].object.commit` y los commits de los `Target` de los intentos no terminales;
- en I-P13, declarar la excepción: en un par REBASE_RECONCILIATION, `loop.object.commit` cambia a su imagen con el **mismo** `path` y `blob`, sin cambiar
  la fase ni los contadores;
- un intento ya custodiado con un `Target` reescrito y todavía no lanzado se replanifica (`InvocationId` nuevo, dentro de la misma reserva, §20.6 caso A);
  uno ya lanzado conserva su `Target` original, que la acreditación histórica cubre por el blob.

## Observaciones no materiales (no necesitan A-n; se aplican al implementar F4)

| Id | Observación | Tratamiento propuesto en F4 |
|---|---|---|
| SM-01 | I-P02 admite BOOTSTRAP → Q0, pero I-S15 exige ambas aceptaciones en ACCEPTED para un Q0, e I-P09 solo las cambia en un QU. La arista es inalcanzable | el validador aplica todas las invariantes; la arista nunca valida. Basta registrarlo en la evidencia de C-15 |
| SM-02 | Si el titular cae entre el force-push de un rebase fuera de ventana (§8.8, paso 5) y el QU de reconciliación, el `RebaseMap` puede existir solo en su host. En otra máquina, sin los commits originales, las imágenes no se acreditan y la unidad queda en STOP (fallo cerrado, correcto) | custodiar el `RebaseMap` en un commit de evidencia (no es un punto durable, §8.1) dentro del mismo push del rebase, antes del QU. Preserva el contrato (I-H02 sigue bloqueando hasta el QU) y hace recuperable el mapa |
| SM-03 | El estado `/v2` conserva `automation_state.next_action` (AUTOMATION_PLAN §8, prosa de una línea) y añade `orchestration.next_action` estructurado (§20.4). V14 no fija su relación | el estructurado es el canónico (I-S18: derivable de forma única); la prosa es una representación de una línea. El validador de F4 comprueba que la prosa nombre el mismo rol y la misma acción |
| SM-04 | Por la definición de sección de 16.3 y E.4, el encabezado de título (`# …`, nivel 1) abarca el archivo entero, así que la derivación del mapa lo marca MODIFIED en todo Markdown modificado (MEASURED con el prototipo de [f4-dossier.md](f4-dossier.md) §6). El preámbulo es el texto anterior al primer `##` | el mapa lista esas secciones (MV-6), y una cita I61 de un título se lee entera en EFF^1, coherente con el compromiso declarado. El generador y el resolver usan la definición de preámbulo de 16.3, no «antes del primer encabezado» |
