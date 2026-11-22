# PB-T4d: decisiones de diseño de la sesión antes de la v5.2 (I-64 F4B)

**Fecha:** 2026-11-22.

**Contexto:** al escribir la v5.2 del paquete de PB-T4, el redactor midió si cada baja de la lista del alcance B hace falta. Lo hizo compilando sin el piloto desde cero bajas, en una copia exportada del cierre de PB-T4c (`6c2820ba`, F4B-E55), en `/d/r6h`: de las 235 bajas, 201 hacen falta para compilar y 34 no, y ninguna cae fuera de la lista. Muchas de las 138 usan el piloto como vehículo de código que se queda. El paquete no fijaba qué sustituye a esas 138, y el redactor lo llevó a la sesión, que lo llevó al Owner: «Sustitutas donde hace falta (Recomendado)» (`owner-pbt4d-substitutes-decision.md`).

La sesión decide además lo siguiente. **No son decisiones del Owner:** no cambian la lista elegida (el alcance B, o una parte, con las sustitutas de esa decisión) ni ningún comportamiento visible. Se declaran en la emisión de PB-T4d.

Este registro sigue la forma de los de PB-T4b (`session-pbt4b-design-decisions.md`, D-S1 a D-S6) y de PB-T4c (`session-pbt4c-design-decisions.md`, D-S7 a D-S10), que no se tocan porque ya están custodiados.

## D-S11. El RED de PB-T4d lleva todo lo de `tests/`, sin esqueleto, y dos CT

**El hecho:** PB-T4d retira código. Su GREEN solo borra y limpia comentarios en `src/` (`pbt4d-green-v52.txt`). AUTOMATION_PLAN 16.8 prohíbe tocar `tests/` después del RED (salvo la reparación del RED, F4B-E35). Las pruebas que usan lo retirado dejan de compilar en el GREEN, así que tienen que salir, o cambiar, antes.

**La decisión:**
- **El RED lleva todo lo de `tests/`:** las bajas con registro, las ediciones con registro, los `Skip`, los equivalentes, las sustitutas de `owner-pbt4d-substitutes-decision.md`, la prueba lenta acotada y las clases de las CT. `src/` queda intacto en el RED: **no hay esqueleto**.
- **El GREEN solo toca `src/`,** con el texto literal del paquete.
- **Dos CT, un `[Fact]` cada una,** en `tests/RackCad.Tests/`:
  - **CT-PBT4D-01** (`PushBackRetirePilotGuardTests`, `ExpectRed` true): lo retirado no está y lo que se queda sí, por texto de `src/` sin comentarios, con autopruebas sobre texto inyectado en memoria. Cae en el RED, porque `src/` sigue con el piloto, y pasa en el GREEN;
  - **CT-PBT4D-02** (`PushBackRetirePilotCensusGuardTests`, `ExpectRed` false): ningún archivo de `tests/` nombra lo retirado como código, sin cadenas ni comentarios, con autoprueba. Pasa en las dos ramas. Si al RED le faltara una baja, se ve en el RED y no rompe el GREEN, que ya no puede tocar `tests/`.

**Por qué:** es la forma que impone 16.8 para una retirada, y deja un RED que falla por aserción (C04) sin inventar un esqueleto.

**Lo que no cambia:** la lista, el GREEN y lo que fija cada `ExpectRed` de lo heredado.

## D-S12. Las tres pruebas que caen al ejecutar: `Skip` y equivalente, con el prefijo `PushBackRetirePilot`

**El hecho:** con la retirada caen al ejecutar, fuera de las bajas (medido en `/d/r6e` y en `/d/r6h`):
- el equivalente de CT-PBT4B-20 de PB-T4c (`PushBackEditAllFullCopyGuardTests`), porque cambian los `BaseSha`, los `Skip` y los recuentos de los originales y las copias que PB-T4d edita;
- y dos guardas de `WorkspacePilotPrepareGuardTests`, que exigen `PushBackPilotPrepareInput` y el cuerpo del `Prepare` del piloto.

El Owner las puso en la lista como sustituidas con registro (`owner-pbt4d-list-decisions.md`, pregunta 1).

**La decisión:**
- **Cada una con su `Skip` y su equivalente,** como en PB-T4c: el original queda con el `Skip` de motivo literal en su línea de atributo, y nada más cambia en su archivo.
- **Los equivalentes, en clases nuevas con el prefijo `PushBackRetirePilot`,** que no es prefijo de ningún filtro heredado:
  - `PushBackRetirePilotFullCopyGuardTests`: la copia literal del equivalente de PB-T4c, con los `BaseSha`, los `Skip`, las `CopySha` y los recuentos nuevos;
  - `PushBackRetirePilotPlanGuardTests`: las dos guardas, cada una con su cambio declarado (el tipo y la fábrica de la instantánea completa, y el orden de `PrepareFull`).
- **Los filtros, por clase exacta,** como D-S8: ningún filtro nuevo selecciona una clase con el otro `ExpectRed`, y ningún filtro heredado selecciona una clase nueva.

**Lo que no cambia:** la lista y las líneas esperadas de los originales.

## D-S13. Los vacíos de M-04 se reescriben en su sitio

**El hecho:** el Owner llevó a PB-T4d los vacíos de M-04 (`owner-pbt4b-code-decisions.md`, pregunta 2), y en `owner-pbt4d-list-decisions.md` (pregunta 1) eligió que se reescriban para el borrador completo. Son dos guardas de `WorkspacePilotEditGuardTests` y el ayudante que hereda su copia, en `WorkspacePilotEditGuardFull.cs:255-258`. Buscan `FindPilotDraft` con la firma del piloto, y el anfitrión tiene hoy la del borrador completo.

**La decisión:** se reescriben en su sitio, como edición con registro, con la excepción declarada de C02 de PB-T4d (§6.4 del paquete). En cada una solo cambia `PushBackPilotDraftState` por `PushBackFullDraftState` en la firma que buscan. El equivalente nuevo de CT-PBT4B-20 (D-S12) recoge el `BaseSha` nuevo de `WorkspacePilotEditGuardTests` y la `CopySha` nueva de su copia.

**Lo que no cambia:** lo que fijan las guardas: lo mismo, sin vacío.

## D-S14. La prueba lenta se acota a cada 97.º toque del catálogo y al último

**El hecho:** `FullPanelCopyWorkspacePilotPrepareGreenTests.InvalidD_ABuilderThatCannotBuildTheOutgoingSystemIsInvalidWithTheTypedErrorAndNothingIsWritten` (`WorkspacePilotPrepareFull.cs:1911-1944`) tarda unos 6,5 minutos sola. Medido en `/d/r6f`: un PREPARE completo toca el catálogo 4 936 veces, y el bucle hace un PREPARE por cada toque. El Owner eligió acotarla en PB-T4d, con registro (`owner-pbt4d-list-decisions.md`, pregunta 3): «Su bucle se reduce a unos puntos que incluyan el fallo del constructor».

**La decisión:** el bucle recorre cada 97.º toque y el último (`Enumerable.Range(0, total).Where(touch => touch % 97 == 0 || touch == total - 1)`), unos 52 PREPARE, en lugar de los 4 936. El cuerpo del bucle y las aserciones no cambian: todas las salidas son `Invalid` tipados, y entre ellas están el fallo del constructor (d) y `catalog-boom`. Lo mide la precondición 6.

**Lo que no cambia:** lo que fija la prueba.

## D-S15. Se limpian los comentarios que nombran lo retirado

**El hecho:** el texto de la retirada medida (B, en `/d/r6e`) deja cuatro `<see cref>` a miembros retirados y un comentario sin su `Build`:
- `PushBackPilotDraftState.cs:11` nombra `ComposeForWrite`;
- `PushBackPilotUpdatePlan.cs:18` nombra `PushBackPilotPrepareInput`;
- `PushBackPilotUpdatePlan.cs:75` y `:100` (en el texto B) nombran `PushBackPilotUpdatePlanner.Prepare`;
- `PushBackPilotUpdatePlan.cs:381` es el comentario huérfano.

No dan aviso, porque la solución no genera la documentación XML.

**La decisión:** se limpian en el GREEN, como bloques EDIT de solo comentarios, tras los de la retirada (`pbt4d-green-v52.txt`):
- el de `PushBackPilotDraftState.cs` dice que un diseño nunca se arma directamente sobre el estado de un borrador;
- el de la instantánea completa ya no nombra a su gemela;
- los dos de `Prepare` pasan a `PrepareFull`;
- el comentario huérfano se une al de `BuildFull`.

Con eso, el GREEN toca un archivo más de `Pilot/`, `PushBackPilotDraftState.cs`, que pasa a `UNIT_CHANGE`.

**Lo que no cambia:** ningún código; lo mide la precondición 6.

## D-S16. La cobertura de la UI se mide como la unión de varias corridas, y se listan las que bajan

**El hecho:** al ensayar la cobertura ampliada de la v5.3 (`owner-pbt4d-cat4d-decisions.md`), salieron dos líneas sin ejecutar en el GREEN: `RackViewBatchDialogPresenter.cs:37-38`. Son dos getters que lee un binding de WPF desde CT-14 de `PushBackClassicCharacterizationOwnerTests`, una prueba que no cambia, y su cobertura varía de una corrida a otra:
- con su filtro solo, la base da 1, 0, 1 y el GREEN 1, 1, 1;
- con todos los filtros en una sola corrida, la base da 1, 0, 0 y el GREEN 0, 0, 0.

Con una corrida por lado, el criterio de cobertura daba por perdido un camino que no se pierde.

**La decisión:**
- **La UI de cada lado se mide como una unión:** tres corridas de todos los filtros juntos y una corrida de cada filtro en su propio proceso, que es como los corre el contrato. Una línea o una rama cuenta como cubierta por la UI de un lado si la cubre alguna de esas corridas. Lo mismo para la base y para el GREEN. Va declarado como R-T4-38.
- **Lo que baja se lista aunque no sea una pérdida.** El informe de cobertura lista toda línea o rama de `src/` que se queda que cubren más corridas en la base que en el GREEN, con cuántas en cada lado y la prueba que la cubría.
  - No cambia el PASA ni el FALLA.
  - Es para que la recomprobación vea si la unión tapa una pérdida real, es decir, una prueba borrada que la cubría siempre y que la UI solo cubre a veces.

**Lo que no cambia:** la regla del Owner («Lo que cubra la UI se mide con la cobertura de sus filtros antes de darlo por perdido»), ni la lista.

**Nota tras la recomprobación de CA-T4d sobre la v5.3** (M-11 de la sección «Recomprobación 1 (v5.3)» de `independent-review-pbt4d-check.md`):
- **Lo que nombra cada entrada:** coverlet no registra qué prueba cubre cada línea. Cada entrada de «Bajan» nombra la corrida que la cubría en la base y ya no en el GREEN: Core, las tres corridas juntas o el filtro. No nombra la prueba. Donde arriba dice «la prueba que la cubría», vale la corrida.
- **ESTABLE solo por la UI** exige algo más que las tres corridas juntas en el GREEN: también al menos un filtro en su propio proceso. Así una cobertura que dependa del orden de las pruebas no cuenta como estable. La recomprobación midió 0 casos así en la v5.3; la v5.4 lo aplica en `cov_p6_v53.py`.
