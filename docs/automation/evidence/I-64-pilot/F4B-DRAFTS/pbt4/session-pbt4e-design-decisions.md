# PB-T4e: decisiones de diseño de la sesión sobre la v5.5 (I-64 F4B)

**Fecha:** 2026-11-22.

**Contexto:** el Owner decidió la lista de la tarea de la recarga de «Editar todo» (`owner-pbt4e-list-decisions.md`, la 15.ª tanda): «Solo la recarga», «No abrir y avisar», «Antes de PB-T5» y «Sí, con condiciones». Dejó a la sesión dos decisiones de diseño, porque no cambian el comportamiento visible que eligió («Lo que queda decidido», último punto): dónde va la recarga y si se reutiliza `EditAllStatus.NotBegun`. El redactor de la v5.5 (`I-64-f4b-pbt4-gate-contract-package-v55.md`) las escribió como propuestas, D-S17 y D-S18, en D-PBT4-22 y §8.5, con su evidencia `ruta:línea` sobre `160e019d` (F4B-E59).

La sesión las revisó leyendo por objetos git en `160e019d` el cuerpo de `EditAllOfPilot` y el de `TryReloadPilotEdit` (`WorkspaceHostCore.cs`), y `BeginEdit`, `EditAll`, `Describe` y `EditAllNoticeOf` (`WorkspacePilotFieldViewModel.cs`), y **confirma las dos tal como están propuestas**. No son decisiones del Owner: no cambian la lista elegida (vacía, ninguna prueba existente sustituida) ni ningún comportamiento visible que él decidiera. Se declaran en la emisión de PB-T4e.

Este registro sigue la forma de los de PB-T4b (`session-pbt4b-design-decisions.md`, D-S1 a D-S6), PB-T4c (`session-pbt4c-design-decisions.md`, D-S7 a D-S10) y PB-T4d (`session-pbt4d-design-decisions.md`, D-S11 a D-S16), que no se tocan porque ya están custodiados.

## D-S17. La recarga va en el anfitrión (la opción A de la propuesta de lista)

**La decisión:** `EditAllOfPilot` llama a `TryReloadPilotEdit(request)` en una rama `else if (draft.NeedsLazyReload)`, tras la de «sin borrador» y antes de las comprobaciones del estado, del texto pendiente y de `ShowModal`. Con `Reloaded` sigue y abre; con cualquier otro resultado no abre y devuelve `EditAllOutcome.NotBegun(<el resultado de la recarga>)`. Es el GREEN literal del anexo V55-A.

**Por qué, comprobado por la sesión:**
- **La regla «Clean y marcado» queda en un solo sitio.** `TryReloadPilotEdit` ya la aplica con `NeedsLazyReload` (WHC:527). En el panel sería otra copia de la que ya usa «Editar» (FVM:386).
- **La recarga queda dentro de la marca de lectura de `TryEditAll`.** El envoltorio del puerto (WHC:1163-1175) pone `_pilotReadInProgress` antes de llamar a `EditAllOfPilot`. La rama nueva llama a `TryReloadPilotEdit` directamente, no por el puerto, igual que la de «sin borrador» llama a `TryBeginPilotEdit`. Así no se topa con su propia marca, y ninguna otra operación del panel se cuela entre la recarga y el editor.
- **La compuerta ya pasó** (WHC:1185-1191). Los pasos 2 a 5 de `TryReloadPilotEdit` la repiten sin efecto. El paso 6 no puede dar `NotNeeded`, porque el anfitrión acaba de mirar `NeedsLazyReload` en la misma llamada síncrona. Los resultados que se alcanzan son `Reloaded`, `NotAdmitted`, `Waiting`, `Rejected` y `Failed`, como dice D-PBT4-22.
- **Como mucho una lectura por gesto,** la de empezar o la de recargar (`else if`), y ninguna durante `ShowModal`.
- **El caso del borde** (un texto pendiente igual al valor confirmado, que deja el borrador Clean tras el `CommitField` del gesto): el anfitrión mira el borrador después de confirmar y lo recarga sin perder nada, porque no había cambios. «Editar» no lo recargaría, porque mira la vista de antes. Se acepta como está: no hay pérdida, y la recarga es justo lo que pidió el Owner para un borrador Clean marcado.

**Lo que no cambia:** «Editar», el botón, la ventana, RACKEDITAR, el camino clásico, el Plugin y el puerto (ningún miembro nuevo).

## D-S18. Se reutiliza `EditAllStatus.NotBegun`, sin valor nuevo

**La decisión:** una recarga que no recarga devuelve `EditAllOutcome.NotBegun(<el resultado de la recarga>)`. El campo, con `NotBegun` + `NotAdmitted`, pone el diagnóstico y la pista como hoy y, si el borrador ya existía (`view.Exists`), el aviso de «Editar», «No se recargó: el borrador se conserva tal como estaba.», en la constante `NotReloadedText`, que usa también `BeginEdit`. Cambian los comentarios de `NotBegun` y de `Begin` en `WorkspaceEditIntent.cs`.

**Por qué, comprobado por la sesión:**
- **El resto de resultados ya sale como en «Editar».** `EditAllNoticeOf` da `Describe(outcome.Begin.Status, outcome.Begin.Reason)` para `NotBegun` (FVM:1343), la misma función que «Editar» usa para los resultados que no son `NotAdmitted` (FVM:413). Así, «En espera: …», «Rechazado: …» y «Fallo: …» salen con el mismo texto que en «Editar», que es lo que pidió el Owner en la pregunta 2.
- **`view.Exists` distingue bien la recarga.** Sobre un borrador que ya existe, el anfitrión nunca lo empieza (la rama de «sin borrador» exige `draft == null`), así que un `NotBegun` con `view.Exists` es el de la recarga. Esto equivale al `stale` de «Editar» (FVM:386), pero se decide en el anfitrión, no en la vista.
- **Un valor nuevo obligaría a un esqueleto en `src/` en el RED,** porque CT-PBT4E-03 necesita la fábrica para escribir la respuesta de su doble. PB-T4e dejaría de tener un RED solo de `tests/`.

**El coste:** el campo infiere la recarga de `view.Exists`, y eso lo explica el comentario del GREEN.

## El `TaskId`

**`F4B-PBT4-EDIT-ALL-RELOAD`** (24 caracteres), con la forma de `F4B-PBT4-RETIRE-PILOT` (PB-T4d). Se confirma.

## La evaluación de R-T4-39 (V55-10): sin decisión nueva del Owner

**El hecho:** la propuesta de lista deducía que hoy, al editar con «Editar todo» sobre una base vieja, «Actualizar» acaba en el conflicto. La v5.5 midió otra cosa con los dobles de CT-PBT4C-10 y sin la recarga: `NotAppliedPreflight`, con «Ningún sobre editable de la relectura coincide con el origen del borrador.». Con la recarga, la misma receta se aplica.

**Por qué no vuelve al Owner:** ninguna de sus cuatro respuestas depende de cómo acaba hoy «Actualizar» sobre la base vieja.
- **La pregunta 1** describe el defecto, que «Editar todo» no vuelve a leer el dibujo, no su consecuencia en «Actualizar». Su opción 3 (no elegida) habla del conflicto de un borrador con cambios del usuario, que PB-T4e no toca.
- **La pregunta 2:** la opción elegida se justifica por «abrir sobre una base que ya se sabe vieja», y eso sigue siendo cierto. La opción descartada decía que «el cambio de fuera se detecta al «Actualizar», como hoy», y también sigue siendo cierto: «Actualizar» no aplica, aunque con otro motivo.
- **Las preguntas 3 y 4** (el orden y la emisión) no dependen de la medida.

*Corregido tras CA-T4e, M-03: la versión anterior de este registro llamaba «pregunta 3» a la opción 3 de la pregunta 1.*

**Las premisas que el Owner leyó en otros sitios** (CA-T4e, M-03). La premisa deducida no solo estaba en las preguntas de la 15.ª tanda. El Owner también la leyó:
- en `pbt4e/list-proposal.md:31`: «"Actualizar" acaba en el conflicto y en «Conservar mi borrador»»;
- y en la pregunta de OWNER-02 de PB-T4c (`owner-pbt4c-code-decisions.md:25`): «al aplicar sale el camino de «Conservar mi borrador»».

La premisa que sostiene esas decisiones sigue en pie: «no se escribe nada sin avisar». `NotAppliedPreflight` tampoco escribe. Por eso las decisiones no cambian.

La medida refuerza la corrección elegida, y el Smoke-PB de PB-T5 lo mira en AutoCAD (§5.5, paso 6). **En PB-T5 se le cuenta al Owner, como información y no como decisión,** junto a ese paso 6: con un borrador Dirty y un cambio de fuera que reescriba el sobre de origen, «Actualizar» puede dar `NotAppliedPreflight` y no «Conservar mi borrador».

## D-S19. La recarga va detrás de las comprobaciones de la vista (tras CA-T4e, M-01; ajusta el sitio de D-S17)

**El hecho** (CA-T4e, M-01, leído en `160e019d`): `NeedsLazyReload` es `_registered && IsPossiblyStale && Status == Clean` (DS:306), y `Status` llama al comparador sin protegerlo (DS:298). Con un borrador «Sin decidir» y marcado, el comparador lanza:
- **hoy:** `ViewOfPilotDraft` protege `Status` (WHC:1860-1867), así que `TryEditAll` responde `Refused(EditAllStateReason)`, sin ningún `Report`;
- **con el GREEN de la v5.5:** la excepción sale del `else if` antes de las comprobaciones de la vista, y la recoge el `catch` de `EditAllOfPilot`. El resultado es `Failed(<mensaje>)`, con `Report` en el registro de fallos.

No se ve desde el panel, porque el botón exige `CanEdit` (FVM:236-237). Pero contradice «Unknown … sigue como hoy» de D-PBT4-22, y es un efecto que la precondición 6 no midió.

**La decisión:** la opción (b) de CA-T4e. La recarga pasa detrás de las tres comprobaciones de la vista (`Applying` o no registrado, `!view.CanEdit` y el texto pendiente) y antes de `ShowModal`, solo sobre un borrador que no se acaba de empezar y con `NeedsLazyReload`. Así:
- un borrador «Sin decidir», Conflict, Error o con texto pendiente responde exactamente como hoy, sin evaluar `NeedsLazyReload` ni leer;
- la regla «como mucho una lectura por gesto» sigue garantizada por la estructura.

Se añade a las CT una línea que lo fija: un borrador «Sin decidir» y marcado da `Refused`, sin fallos registrados y sin lecturas. En la v5.5 caería; en la v5.6, pasa.

**Por qué (b) y no (a), solo declararlo:**
- deja el comportamiento decidido «como hoy» literal en todos los estados;
- quita un camino que funciona por excepción;
- y no deja un efecto sin medir, que es una de las condiciones por las que la emisión vuelve al Owner.

Cuesta una versión del paquete (la v5.6) y repetir la precondición 6 en copias nuevas. Los guiones ya existen.

**Por qué no vuelve al Owner:** no cambia nada visible, ni la lista (vacía), ni ninguna de las cuatro respuestas. El sitio exacto de la recarga es la decisión de diseño que el Owner dejó a la sesión. CA-T4e lo dice igual («Ninguna de las dos vuelve al Owner»).

**Lo que cambia:** el GREEN (el bloque de WHC), INV-PBT4E-03, el texto de D-PBT4-22 y las CT (la línea nueva). La v5.6 se recomprueba con CA-T4e, solo sobre su diff.

## Los otros menores de CA-T4e (EMITIBLE, B 0 / I 0 / M 5)

- **M-02, los límites del método de la cobertura:** la v5.6 los declara en §4.5 y en la cabecera del guion. La precondición 6 de la v5.6 añade la corrida opcional: la cobertura del GREEN sin las dos clases nuevas, comparada con la base.
- **M-03:** corregido arriba, en la evaluación de R-T4-39.
- **M-04, las dos v5.5:** la v5.6 se arma con un solo `make_v56`, después de `post-p6`, y el registro de `post-p6` nombra la versión que se emite. El traspaso declara la segunda ejecución de `make_v55.py` (`a241bfbe` → `e597790f`) y qué cambió en `head.md` y `ops-d.md`.
- **M-05, «Toca una función»:** se anota en el registro de la emisión (F4B-E60), junto al coste de D-S17: el GREEN cambia el cuerpo de `EditAllOfPilot`, el aviso y la constante de FVM, y comentarios de cuatro archivos.

## Lo que queda para la emisión

- La comprobación acotada CA-T4e ya dio EMITIBLE, B 0 / I 0 / M 5 sobre la v5.5 (`independent-review-pbt4e-check.md`). Falta su recomprobación, solo del diff de la v5.6, y su resultado frente a las condiciones de la pregunta 4 del Owner.
- La custodia: los números F4B-E, la entrada «## PB-T4e» de `superseding-characterization.md`, la redacción de las rutas de las TRX y de los JSON de coverlet, y el recuento de `ForbiddenWriteScope` que saca el generador.
