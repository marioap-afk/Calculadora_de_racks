# I-64 F4-T4C1-HOST, intento 1: análisis de las revisiones y disposición de la corrección

**Objeto:**
- la entrega del intento 1: RED `8e57061e`, GREEN `271a045c`, sobre BaseSha `44eafbc1`;
- la verificación `R20261115T163412Z-eca1`: **EXECUTION_VERIFIED**, 14/14, con `VerifiedSha` `271a045c` y el RED acreditado (`ChainRedFiles`, 17 rutas de `tests/`).

## 1. Hechos del intento 1

- **Planificación:**
  - `R20261115T143542Z-0100`, RECHAZADA por la comparación byte a byte: `ForbiddenWriteScope` con 216 entradas frente a 163, y A4 en fail;
  - la replanificación reforzada única de la parada F4T4C-C15, `R20261115T144358Z-b5f7`: fidelidad PASS en las ocho colecciones y A1-A8 8/8;
  - nc4 `R20261115T144759Z-886c`, con su oráculo: solo A3 en fail.
- **El Worker** `R20261115T145157Z-efec`: `claude-sonnet-5-5`/high, 426 turnos y 0 denegaciones. IMPLEMENTATION_COMPLETE, con las 30 rutas del alcance y el GREEN sin tocar `tests/`.
- **La CI exacta:**
  - GREEN `43733257361`, 4/4: Core Workspace 3385/3385, UI Workspace 601/601 y Core completo 18966/18966;
  - RED `43731731401`: Core en failure, con 216 fallos, todos dentro del filtro de Workspace; UI en success; build-plugin omitido.
- **Los seis textos** de `PushBackPilotHostTexts`: byte a byte iguales a los que fijó la sesión y ratificó el Owner (D1).

## 2. Revisiones

- **La directa de la sesión** (`direct-review.md`): 1 importante (D-1) y 1 menor (D-2). D-1 es el mismo defecto que B-01.
- **La adversarial independiente** (`independent-review.md`): **REWORK, 1/1/10.** El alcance, el RED, los textos, el adaptador, el corredor, el escritor D-1, el ámbito propio, R-T4-07, I-01, M-03 y M-06 están bien.
- **La verificación del Controller:** EXECUTION_VERIFIED. Sus hallazgos libres registran las acotaciones de G-1 y G-10 declaradas por el Worker, y unos avisos xUnit2029 en tres archivos de prueba nuevos.

**Regla aplicable:** D2 del Owner (`owner-f4t4c-decisions-d1-d6.md`). El cierre exige 0 bloqueantes y 0 importantes en las dos revisiones. Con B-01 e I-01, el intento 1 **no se cierra**.

## 3. Causa raíz

**B-01:** el billete distingue tres marcas, «empezado», «en `Execute`» y «terminado». El Worker modeló «en `Execute`» como una marca que no se retira. D-T4C-02 dice que «un billete en `Execute` nunca caduca», y se refiere a *durante* `Execute`: la defensa en profundidad del mismo apartado exige que caduque «uno terminado cuya compleción no llegó», y D-T4C-04 repite que, si el salto falla, «el billete lo libera su caducidad». La prueba que el Worker escribió para la cota solo ejerce un comando ajeno *durante* `Execute`, así que la lectura equivocada no se vio. El prompt de la sesión transmitía la regla, pero no daba el caso como prueba obligatoria.

**I-01:** el Worker acotó G-1 y G-10 por carpeta porque encontró llamadas legítimas fuera de las de integración. No midió qué llamadas exactas existían. Las medidas en CurrentSha permiten un censo de todo el producto con excepciones por archivo exacto: para G-1, los tres servicios de dibujo y el escritor; para G-10, las cuatro ventanas que tienen un `RequestUpdate` propio, ajeno al piloto.

## 4. Clasificación y tratamiento en el intento 2

| Id | Gravedad | Tratamiento (todo dentro del alcance de T4C1) |
|---|---|---|
| **B-01** (D-1) | **Bloqueante** | `Execute` retira la marca «en `Execute`» al salir, en su `finally`, junto con «terminado». La caducidad libera un billete terminado cuya compleción no llegó, sin un segundo resultado. Dos pruebas nuevas en `WorkspacePilotUpdateRequestTests.cs`: (a) en el modo diferido, el callback corre y la compleción no llega; tras un comando ajeno, `RequestUpdate` se acepta, los dos descartes se sirven y el resultado guardado sigue, una sola vez; (b) con un comando ajeno durante `Execute`, el billete no caduca durante la corrida y se libera en la comprobación siguiente, sin un segundo resultado |
| **I-01** | **Importante** | G-1 como censo de todo el producto: `.PrepareRedraw(` y `.RedrawInTransaction(` de los tres `PushBack(System\|Frontal\|Planta)DrawService` solo en esos tres servicios y en `WRITER`, en `PrepareUnits` y `WriteUnit`. Lleva la negativa de un archivo nuevo del Plugin. G-10 sin excluir carpetas: `.RequestUpdate(` solo en los dos reenvíos y, como excepciones por archivo exacto con cuenta 1, las cuatro ventanas que la revisión midió. El gesto `Update()` del VM del campo puede quedar admitido para F4-T4C2. Cada regla con su negativa |
| M-01 | Menor | En `TryConfirmBase`, `TryKeepMine` y `TryDiscardAndReload`, la comprobación del vuelo pasa a después de la autorización (`PassGate` o `Authorize`), como pide la fila 5. La guarda exige ese orden también cuando la autorización es por `PassGate` |
| M-02 | Menor | La fila 4 gana su negativa: un portador sin censar |
| M-03 | Menor | La guarda de textos de la fila 11 cubre los siete `Busy` y `Waiting` nuevos, con una negativa |
| M-04 y D-2 | Menor | `HostNotRunReason` ya no se usa como prefijo: el corredor y el núcleo usan la constante sola y registran la excepción, y desaparece el `" "` literal de CORE. `TryOpenTarget` del adaptador devuelve `PushBackPilotUpdateRun.DocumentMismatchReason` en lugar de sus dos literales muertos. El mensaje de la excepción por falta de contexto de sincronización se conserva, porque `HostRequestFailedPrefix` está hecho para componerse con la causa, y se declara |
| M-05 | Menor | La respuesta de un cierre no aceptado, y `NotePilotEdit` con `_knownClean.Remove`, entran en el `try` de la respuesta de `TryUpdatePilot`, con el mismo respaldo. Es el punto (4) de M-01 |
| M-06 | Menor | (a) `Execute` y `NotRun` no hacen nada si el billete ya está terminado. (b) Si `TryRequest` lanza después de que el callback empezó, el valor de inicio es `Accepted`, sin un segundo canal. (c) `IsAnyPilotDraftApplying` responde `false` en su `catch` y lo registra. (d) Un `Lock()` nulo da `NotRun(Failed, LockFailedPrefix…)`. Cada uno con su prueba |
| M-07 | Menor | G-4 cuenta `\bTryUpdatePilot\b`: dos apariciones en el núcleo y ninguna fuera, para que vea también un grupo de métodos. La guarda del adaptador ancla `OnCompleted` en la rama de `IsCompleted` falso y `Post` al contexto capturado |
| M-08 | Menor | La entrega lista, frente al inventario, los tres cambios de guarda que no declaró: el `InlineData` de la fila 5, `ForeignCommandStartsOf` en «las consultas no olvidan» (fila 15) y la expresión de `WriterProhibitionViolations` (fila 13) |
| M-09 | Menor | **Declaración de la sesión:** las corridas locales `tests/d1` y `tests/d2` del intento 1 son anteriores al RED y no se listaron. La entrega del intento 2 declara todas sus corridas locales |
| M-10 | Menor | Higiene: la marca `Started` se usa o se quita; `WRITER` pierde el `using AcApplication` que dejó sin uso D-T4C-07; se restaura el comentario borrado de `TW/WorkspacePilotUpdateRunTests.cs`; se renombran las dos pruebas con nombre desfasado |
| M-11 (Controller) | Menor | Los avisos xUnit2029 de los tres archivos de prueba nuevos se corrigen: sin avisos nuevos de compilación |

**Lo que no cambia:**
- el alcance de 30 rutas, la lista prohibida y las paradas;
- ningún cambio de guarda fuera de las filas [1] de D-T4C-20 (parada C07);
- `RUN`, `WRITER`, `TryUpdatePilot` y `UpdateGate` dentro de sus líneas declaradas (parada C14).

M-05 y M-06 (d) completan M-01 y no salen de él. La retirada del `using` de `WRITER` es consecuencia directa de D-T4C-07.

## 5. Disposición

**Decisión:** corrección en el **intento 2 de 3**, por la decisión D3 del Owner. Hay REWORK real, demostrado por B-01 con una prueba que lo hará fallar, y la corrección queda dentro del objetivo y del alcance de T4C1. **El intento 3 no se gasta sin el Owner.**

**`CorrectionOf`:**
- `RunId` `R20261115T163412Z-eca1`, la verificación del intento 1;
- `FailureClass` `Tests`: la evidencia de pruebas no cubría una cota que el paquete exige y el producto la incumplía. Sigue el precedente de F4-E55;
- `AnalysisSha256`: el de este archivo, custodiado.

**El contrato del intento 2** (`gate-contract-attempt2.json`):
- parte del del intento 1;
- conserva el alcance de 30 rutas, la lista prohibida de 163 entradas, los 18 invariantes, los `RequiredTests` y las paradas;
- añade INV-F4T4C-21, con las correcciones de B-01, I-01 y M-01 a M-11, e INV-F4T4C-22, con la mecánica de la corrección.

**La cadena:**
- `ChainBaseSha` `44eafbc1` y `ChainRedSha` `8e57061e`;
- el RED de la corrección lleva las pruebas nuevas o cambiadas, que fallan por aserción contra el producto del intento 1; el GREEN no toca `tests/`;
- una nueva replanificación reforzada única para la delegación de la corrección (D4);
- las dos revisiones de nuevo, la verificación y después nc1-nc3, con la regla de sustitutos de D2.
