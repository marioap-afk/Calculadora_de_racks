# I-64 F4B PB-T3a — Análisis del Coordinator para la corrección del intento 2, con reemisión del contrato

**Fecha:** 2026-11-20.
**Tarea:** `F4B-PBT3A-FULL-WRITE-MODEL`.
**Lo que corrige:** la entrega del Worker `R20261120T001738Z-7d92` (delegación `R20261120T001442Z-903c`, intento 1).
**FailureClass:** `StopCondition` (PBT3A-C02).
**La decisión del Owner:** F4B-E42, «Renombrar ForWrite» (`F4B-DRAFTS/coordinator/owner-pbt3a-g9-decision.md`).
**El presupuesto:** la corrección es el intento 2 de 3.

## 1. Lo que pasó

**El Worker paró antes del commit RED, sin commits ni push.** HEAD siguió en `18e4b783`, la emisión de la v4.4, y el árbol quedó limpio.

**Sobre el esqueleto** que fija el paquete v4.4:
- los 10 métodos de Core del filtro `RackCad.Tests.PushBackFullDraft` y el de UI fallaban por aserción, como se esperaba;
- las tres guardas de la tarea pasaban;
- **pero fallaba además una prueba existente:** `WorkspacePilotUpdateIntegrationGuardTests.G9NoFileOfThePluginOrOfApplicationThatNamesThePilotUsesAWriteApiOrALockOutsideTheFourExceptionsWithTheirExactCounts` (en la suite de Core, 19 107 resultados, 11 en fail: los 10 esperados y G9).

**La causa:** un defecto del paquete, no del Worker.
- **Qué hace el esqueleto:** el esqueleto exacto hace que `src/RackCad.Application/Systems/PushBack/Draft/PushBackFullComposition.cs` nombre el piloto, porque usa `PushBackPilotMaterializer.NonPilotContentReason`, y que declare el miembro cerrado `ForWrite` (D-PBT3-05).
- **Qué prohíbe G9:** la guarda G9 (`tests/RackCad.Tests/Workspace/WorkspacePilotUpdateIntegrationGuardTests.cs`) prohíbe `\bForWrite\b`, entre otros veinte términos de escritura, en cualquier archivo de Application o del Plugin que nombre el piloto, salvo cuatro excepciones fijas.
- **Por qué no se vio antes:** el paquete leyó G9 como regla solo del Plugin.

**Lo que hizo el Worker:** no eligió ninguna salida, porque todas cambiaban el esqueleto, la API cerrada o una prueba existente. Dejó tres parches reutilizables en su carpeta:
- `wip-red-tree.patch`: el esqueleto y las seis clases de prueba;
- `rr/skeleton.patch`: solo el esqueleto;
- `rr/green-wip.patch`: el borrador de la implementación, con el que pasaban los 10 métodos de Core, la prueba de UI y las tres guardas.

## 2. La corrección (VINCULANTE para la delegación de corrección)

**C-1. El nombre:** por la decisión del Owner y el paquete v4.5 (V45-01), el miembro cerrado `ForWrite` de `PushBackFullComposition` se llama ahora **`ForFullWrite`**, con la misma firma. Cambia también en todo lo que el paquete hace depender de él: D-PBT3-05, la lista cerrada de C13, `BuildFull` de D-PBT3-09, CT-PBT3A-06, CT-PBT3A-11 y el esqueleto del §6.1. **Ninguna guarda ni prueba existente cambia.** El barrido de la v4.5 (§4.1) no encuentra otra colisión.

**C-2. El RED:** es el RED entero de PB-T3a, porque el intento 1 no llegó a hacerlo:
- el esqueleto inerte y las seis clases de prueba de la v4.5;
- el Worker puede partir de `wip-red-tree.patch` y `rr/skeleton.patch` del intento 1, con el nombre cambiado. No son autoridad;
- el RED cumple el criterio del contrato reemitido: los filtros con ExpectRed fallan por aserción y ninguna prueba existente falla, G9 incluida.

**C-3. El GREEN:** D-PBT3-xx de la v4.5, solo en los archivos que permite el contrato.
- El Worker puede partir de `rr/green-wip.patch`, con el nombre cambiado. No es autoridad.
- Lo vuelve a comprobar todo: los invariantes, todos los filtros, la suite completa de Core y la CI exacta.

**C-4. La reparación del RED (RR, F4B-E35)** sigue disponible en el intento 2, con sus condiciones.

## 3. El régimen de la cadena (16.8; M-11)

**El contrato se reemite** con la v4.5: los commits X y BASE de la reemisión, F4B-E42.

**La base y la cadena:**
- **BaseSha y ChainBaseSha:** el commit BASE de la reemisión. Contiene el contrato nuevo y este análisis; en `src/` y `tests/` es igual a `18e4b783`, porque el intento 1 no dejó commits.
  - **Por qué es la BASE nueva** (M-06 de la §11 de CA-T3a): sigue el precedente de F4-T4A1, que reemitió el contrato en una corrección. La letra de 16.8 («la BaseSha de la primera delegación», que sería `18e4b783`) da la misma cadena en `src/` y `tests/`, porque entre las dos solo hay documentación, y RT sale igual con las dos.
  - **Lo que deben aplicar A8 y la verificación:** esta elección. Toda medición de la cadena se hace con `-- src tests`.
- **ChainRedSha y ChainRedFiles:** `null` y `[]`. No hubo RED en el intento 1.
- **CorrectionOf:** `{RunId: R20261120T001738Z-7d92, FailureClass: StopCondition, AnalysisSha256: <el SHA de este archivo custodiado>}`.
- **attempts:** 2.

**Las autoridades:**
- el contrato reemitido;
- el paquete v4.5;
- F4B-E42;
- este análisis.
