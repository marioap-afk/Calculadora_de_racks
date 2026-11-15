# I-64 F4-T4C2-PANEL, intento 1: análisis de la parada del Worker y disposición de la corrección

**Objeto:** la entrega del Worker `R20261115T190629Z-9b55`:
- BaseSha `1cce48c0`;
- RED `a6511c04`, con push;
- **sin GREEN**: WorkerStatus BLOCKED, Disposition BLOCKED.

## 1. Hechos

- **La planificación:**
  - la cuarta, `R20261115T190148Z-4797`: fidelidad PASS y A1-A8 8/8, y nc4 `R20261115T190608Z-2c68` con su oráculo;
  - tres anteriores rechazadas por fidelidad de `ForbiddenWriteScope`:
    - `af0a`, que omitió la entrada 102;
    - `6c20` y `3d22`, con un espacio al principio de la entrada 71;
  - el Owner autorizó la tercera y la cuarta planificación (`owner-f4t4c2-third-planning-decision.md` y `owner-f4t4c2-fourth-planning-decision.md`). La cuarta quitó del prompt la cadena errónea que el propio prompt citaba como ejemplo, y cargó la lista como array compacto.
- **El RED `a6511c04`:**
  - 13 rutas, todas del `AllowedWriteScope`;
  - el esqueleto inerte del producto en `WorkspaceEditIntent.cs`, `WorkspacePanelView.xaml` y `.xaml.cs`, y `WorkspacePilotFieldViewModel.cs`;
  - las pruebas nuevas y cambiadas.
- **La CI del RED** (`43753137730`): Core en failure, con 10 fallos de 3437, todos dentro del filtro de Workspace; UI en failure, con 75 de 694, todos dentro del filtro; build-ui en success y build-plugin omitido. Es lo que pide `ExpectRed` true en los dos filtros.
- **Sin GREEN:** el producto completo quedó en `wip-green-impl.patch`, con tres archivos: `WorkspaceEditIntent.cs`, `WorkspaceHostCore.cs` y `WorkspacePilotFieldViewModel.cs`. Aplicado en local, sin confirmar: Core Workspace 3437/3437 y UI Workspace 692/694. Los 2 fallos son los defectos 1 y 2.
- **El relevo:** `claude-sonnet-5-5`/high, 272 turnos y 0 denegaciones. 0 procesos y Snapshot I en la salida y en la entrada; HEAD = remoto = `a6511c04`.

## 2. La parada y su causa raíz

**El Worker paró** por la regla 16.8: desde el commit RED no se puede tocar `tests/`. Tres pruebas que él mismo escribió en el RED son defectuosas:

| # | Prueba | Defecto | Corrección |
|---|---|---|---|
| 1 | `UW/WorkspacePilotFieldViewTests.cs`, `TheUpdateButtonIsVisibleOnlyForADirtyDraftOnAPinnedSurfaceAndNeverInTheOtherStatesNorOnAPreview` (l. 757) | El caso de la vista previa usa un borrador Dirty protegido. El navegador fija toda pestaña con trabajo protegido (`WorkspaceBrowserViewModel.cs`, `RebuildTabs`), así que nunca es una vista previa | Un Dirty **no protegido**, que sí queda como vista previa |
| 2 | Mismo archivo, `ARealClickOnTheUpdateButtonRunsTheGestureOnceInTheOrderOfThePackageAndTheButtonDisappearsWhileTheUpdateIsInFlight` (l. 778) | Espera exactamente cuatro entradas en el registro del doble. El repintado que sigue al gesto añade por diseño una quinta, `TakeUpdateProgress` (F-1, D-T4C-03) | Esperar las cinco, con la quinta como repintado, o comprobar una frontera, una petición y el orden de las cuatro primeras |
| 3 | `TW/WorkspacePilotReconciliationHostTests.cs` (l. 726) | `Assert.Equal(0, ….Count)` introduce un aviso nuevo, xUnit2013 | `Assert.Empty(…)` |

**La causa raíz:** el Worker escribió dos pruebas de vista sin medir dos comportamientos existentes y ya fijados: el anclado de las pestañas con trabajo protegido y el repintado de F-1, que recoge en cada `Show`. El prompt tampoco los señalaba. El tercero es de estilo de aserción.

**Lo que no hay:**
- ningún defecto del producto;
- ninguna parada F4T4C-C01 a C16;
- ninguna decisión material nueva (D5).

El Worker eligió S-04 como la parada más cercana.

## 3. Disposición

**Decisión del Owner:** «Corregir en intento 2» (`owner-f4t4c2-attempt2-decision.md`): la corrección en el **intento 2 de 3**, con cierre por la regla de D2. El intento 3 no se gasta sin el Owner.

**`CorrectionOf`:**
- `RunId` `R20261115T190629Z-9b55`, el Worker del intento 1. No hubo verificación de una entrega sin GREEN;
- `FailureClass` `StopCondition`: el Worker paró;
- `AnalysisSha256`: el de este archivo, custodiado.

Sigue el precedente de F4-T4A1, en F4-E52.

**El contrato del intento 2** (`gate-contract-attempt2.json`):
- conserva el alcance de 14 rutas, la lista prohibida de 150 entradas, los 16 invariantes, los `RequiredTests` y las paradas;
- añade INV-F4T4C-26, la corrección: las tres pruebas, exactamente, y el producto del parche como GREEN;
- añade INV-F4T4C-27, la mecánica de la corrección.

**La cadena:**
- `ChainBaseSha` `1cce48c0`;
- `ChainRedSha` nulo: el RED `a6511c04` no lo acreditó ninguna verificación, porque no hubo GREEN;
- el RED de la corrección es un commit que solo corrige las tres pruebas, sobre `a6511c04`, con el esqueleto todavía inerte, de modo que Core y UI siguen fallando por aserción;
- el GREEN aplica el producto del parche y solo toca `src/`;
- el RED de la cadena queda acreditado en la verificación de la corrección, con RT = `git diff --name-only 1cce48c0 RedSha -- tests/`;
- la delegación de la corrección tiene su propia replanificación reforzada única (D4).
