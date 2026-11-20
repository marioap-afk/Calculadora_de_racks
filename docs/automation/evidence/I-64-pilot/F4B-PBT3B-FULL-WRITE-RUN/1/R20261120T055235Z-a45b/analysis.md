# I-64 F4B PB-T3b — Análisis del Coordinator para la corrección del intento 2, con reemisión del contrato

**Fecha:** 2026-11-20.
**Tarea:** `F4B-PBT3B-FULL-WRITE-RUN`.
**Lo que corrige:** la entrega del Worker `R20261120T055235Z-a45b`, sobre la delegación `R20261120T054837Z-814c`, en el intento 1.
**FailureClass:** `StopCondition`, por PBT3B-C02 y PBT3B-C07.
**La decisión del Owner:** F4B-E45, «Renombrar los métodos nuevos» (`F4B-DRAFTS/coordinator/owner-pbt3b-overload-decision.md`).
**El presupuesto:** la corrección es el intento 2 de 3.

## 1. Lo que pasó

**El Worker paró antes del commit RED, sin commits ni push.** HEAD siguió en `eab27a9e`, la emisión con la v4.5, y el árbol quedó limpio.

**Las líneas base en `BaseSha`:**
- Core, UI y Plugin compilan en Release con 0 errores.
- El filtro Workspace de Core da 3 482 pasadas de 3 482.
- La prueba de limpieza da lo mismo antes y después.

**Con el esqueleto exacto del §6.2 de la v4.5,** `RackCad.Tests` no compila. Hay dos errores CS0121 (llamada ambigua) en una prueba existente de F4, `tests/RackCad.Tests/Workspace/WorkspacePilotUpdateRunTests.cs`, que el contrato prohíbe editar:
- **`:669`:** `PushBackPilotUpdateRun.Execute(null!, setup.Request, setup.Port)`. Es ambigua entre `Execute(DraftSession<PushBackFullDraftState>, PushBackPilotUpdateRequest, IPushBackPilotUpdatePort)`, la nueva de D-PBT3-10, y `Execute(DraftSession<PushBackPilotDraftState>, …)`, la existente.
- **`:799`:** `PushBackPilotUpdateRun.ComparePost(plan, null)`. Es ambigua entre `ComparePost(PushBackPilotUpdatePlan, PushBackFullIntentResult?)`, la nueva, y `ComparePost(PushBackPilotUpdatePlan, PushBackPilotIntentResult?)`, la existente.

**La causa es un defecto del paquete, no del Worker.**
- El paquete afirma que ninguna firma existente cambia y que las pruebas de F4 compilan contra ellas.
- Las firmas, en efecto, no cambian. Pero una sobrecarga nueva con el mismo nombre y un parámetro de tipo referencia vuelve ambiguo el literal `null` en las llamadas existentes.
- El barrido de guardas del §4.1 buscaba colisiones de nombres en el texto y no comprobaba la resolución de sobrecargas del compilador.
- La comprobación acotada CA-T3b tampoco compiló el esqueleto.

**Lo que hizo el Worker:** no eligió ninguna salida. Editar esas dos líneas habría sido C02, y cambiar la forma del API nuevo, C13. Dejó en su carpeta el esqueleto probado, sin aplicar, en `blocked-skeleton.patch`.

## 2. La corrección (VINCULANTE para la delegación de corrección)

**C-1. Los nombres.** Por la decisión del Owner y el paquete v4.6, las sobrecargas nuevas para el modelo completo que chocan con un método existente del mismo nombre pasan a llamarse:
- **`ExecuteFull`**, en `PushBackPilotUpdateRun`, antes la `Execute` nueva;
- **`ComparePostFull`**, en `PushBackPilotUpdateRun`, antes la `ComparePost` nueva;
- **`PrepareFull`**, en `PushBackPilotUpdatePlanner` (PP), antes la `Prepare` nueva.

Las tres conservan su firma. El cambio alcanza a todo lo que el paquete hace depender de ellas en PB-T3b y en PB-T3c: D-PBT3-09, D-PBT3-10, la lista cerrada de PBT3B-C13, el esqueleto del §6.2 y las CT que las nombran.

**Por qué son tres y no dos.** La pregunta al Owner nombró dos métodos, porque eran los dos errores de compilación del intento 1. El barrido ampliado del §4.1 de la v4.6, que mira la resolución de sobrecargas y la reflexión, encontró un tercer choque, que el compilador no detecta y que solo saldría al ejecutar. La prueba existente de F4 `WorkspacePilotPrepareTests.cs:2155-2160` exige, por reflexión, un único método público estático `Prepare` en `PushBackPilotUpdatePlanner`, con la firma del piloto. Con la `Prepare` nueva, esa prueba fallaría en el RED. La sesión le aplica el mismo remedio que eligió el Owner: renombrar el método nuevo sin tocar ninguna prueba existente. No es una decisión nueva, y se declara en la nota de `owner-pbt3b-overload-decision.md` y en F4B-E45.

**No cambia ninguna firma existente, ni ninguna guarda ni prueba existente.**

**El barrido ampliado del §4.1 de la v4.6** recoge dos cosas: las llamadas existentes con `null`, `default` o un tipo convertible a los métodos que reciben una sobrecarga nueva, y las pruebas que leen por reflexión los miembros de esas clases. Con los tres nombres nuevos no encuentra ningún choque.

**C-2. El RED** es el RED entero de PB-T3b, porque el intento 1 no llegó a hacerlo:
- el esqueleto del §6.2 de la v4.6, con PP y UR en M, la interfaz y los cinco archivos nuevos de `tests/` en A, y los tres archivos de guardas de F4 con solo su `Skip`;
- el Worker puede partir de `blocked-skeleton.patch` del intento 1 con los tres nombres cambiados, pero ese archivo no es autoridad;
- el RED cumple el criterio del contrato reemitido:
  - el proyecto compila;
  - los filtros con ExpectRed fallan por aserción;
  - S-1 a S-6 se saltan;
  - ninguna otra prueba existente falla.

**C-3. El GREEN** es el de los D-PBT3-xx de la v4.6, y solo toca PP y UR. El Worker lo vuelve a comprobar todo: los invariantes, todos los filtros, la suite completa de Core y la CI exacta.

**C-4. La reparación del RED** (RR, F4B-E35) sigue disponible en el intento 2, con sus condiciones y con la lectura del §6.0 para PB-T3b: la reparación no toca `src/` ni los tres archivos de guardas.

## 3. El régimen de la cadena (16.8; M-11)

**El contrato se reemite con la v4.6** en los commits X y BASE de la reemisión, F4B-E45.

**La base y la cadena:**
- **BaseSha y ChainBaseSha:** el commit BASE de la reemisión. Contiene el contrato nuevo y este análisis. En `src/` y `tests/` es igual a `eab27a9e`, porque el intento 1 no dejó commits.
  - Es el mismo criterio de F4-T4A1 y de PB-T3a (F4B-E42).
  - Toda medición de la cadena se hace con `-- src tests`.
  - `git diff --name-only eab27a9e <BASE> -- src tests` tiene que salir vacío.
- **ChainRedSha y ChainRedFiles:** `null` y `[]`, porque en el intento 1 no hubo RED.
- **CorrectionOf:** `{RunId: R20261120T055235Z-a45b, FailureClass: StopCondition, AnalysisSha256: <el SHA de este archivo custodiado>}`.
- **attempts:** 2.

**Las autoridades:**
- el contrato reemitido;
- el paquete v4.6;
- F4B-E45;
- este análisis.

**Antes de reemitir, dos comprobaciones nuevas:**
- la comprobación acotada de la v4.6;
- la compilación del esqueleto corregido sobre una copia exportada de la base, fuera del worktree, y la ejecución de la suite completa de Core:
  - se compilan las pruebas de Core, las de UI y el Plugin;
  - en la ejecución, sin los `Skip` de D-1, solo pueden fallar los métodos S-1 a S-6;
  - su salida se custodia en X.

## 4. Lo que no cambia

Siguen igual:
- el diseño;
- D-1, con S-1 a S-6 y su motivo literal;
- la tabla de RR-3, con los literales de las clases nuevas;
- los sustitutos de nc1 (`41440edc…`, PD-F4B-01) y de nc2 (v2, `c033e96c…`, `--task PBT3B`);
- la Snapshot K;
- el binario acreditado.
