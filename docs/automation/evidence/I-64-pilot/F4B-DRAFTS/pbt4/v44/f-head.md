# I-64 F4B — Paquete del contrato de PB-T4 («Editar todo» en el panel del Workspace), v4.4

**Estado:** v4.4. Completa la parte de **PB-T4b**, que la v4.3 dejaba en prosa o por remisión, con las decisiones que llegaron después: **D-S1 a D-S3 de la sesión** (`session-pbt4b-design-decisions.md`) y **la tercera tanda del Owner, «Añadirlas a la lista»** (`owner-pbt4b-breaks-decision.md`), verificada por la §17 de `independent-review-v1.md`. **PB-T4a no cambia:** se emitió con la v4.3 (F4B-E49 y F4B-E50; X `27c13fab`, BASE `5aaacce3`), y su texto de §5.1, §6.1, §6.1.1 y §6.1.2 queda byte a byte como en la v4.3, con «v4.3» en su `Objective`. PB-T4c y PB-T4d, sin cambio de fondo; la lista de PB-T4d pasa a 138.

**La base de PB-T4b:** `5aaacce3` (la emisión de PB-T4a; su `src/` y sus `tests/` son los del cierre de PB-T3c, `7f0d9aa3`) con el GREEN literal de PB-T4a aplicado en memoria (`green_pbt4a.py`). Lo que dependa del GREEN real de PB-T4a va marcado **`<tras el cierre de PB-T4a>`**; lo que dependa de ejecutar, **`<medido en la precondicion 6>`** (ASCII, como en el bloque literal), con su regla.

**Cómo se midió:** solo por objetos git en `5aaacce3`, con Python `-I` y guiones escritos en `<carpeta-del-paquete>`. Nada se compiló ni se ejecutó, y el worktree no se leyó.

**Lo que sale de esta versión (PB-T4b):**
- **la lista de 475** (`pbt4b-substitution-list-475.tsv`, sha256 `654c3b7b045c66c3d7bf0f1c3c1eac42721182aa85ee4c006de444314fd0707c`): las 470 y EIG:871, EIG:883, WLG:627, WLG:642 y WPUG:823; **la de PB-T4d, 138** (`pbt4d-retire-list-138.tsv`, sha256 `d5467dc162e2cda8c539a829cc8f3a7c46260a119f70e840b34e6f78dde0387c`); las dos viejas quedan como historia;
- **el reparto de C-0, 368 copias mecánicas y 60 equivalentes escritos a mano** (D-S1), con el mapa de la copia v2 (`pbt4b-copy-map-v2.tsv`, `3ffadf5b…0e01`); el final lo da la precondición 6;
- **el GREEN literal de `src/`** (`green_pbt4b.py`, nueve archivos, con D-S2 y D-S3) y **`apply_green_pbt4b.py`**, que lo escribe en una copia exportada; **el esqueleto exacto del RED** y **`apply_red_pbt4b.py`**, que escribe el esqueleto, los 475 `Skip`, el kit y las copias;
- **el kit literal** (`pbt4b-kit-WorkspaceFullReadKit.cs`) y **el guion de la copia con los equivalentes literales** (`make_copies_pbt4b.py` y `pbt4b_hand_edits.py`), con su revisión (`pbt4b-equivalents-review.txt`);
- **los `Skip` literales,** uno por causa (`pbt4b_skips.py`), y **su comprobación mecánica** (`check_skips_pbt4b.py`);
- **la partición** de las copias en `FullRedTests` y `FullGreenTests`: su procedimiento y su herramienta (`partition_pbt4b.py`), que corre la sesión en la precondición 6;
- **§5.2 entero** (CT-PBT4B-01 a 20, con sus líneas, su clase y su `ExpectRed`), **los totales exactos de cada rama** y **§6.2 entero,** con su bloque de literales (§6.2.1), sus `RequiredTests`, su `AllowedWriteScope` ruta a ruta y las filas de RR-3.

**Los hechos,** en `pbt4-measurements.md` (R-0 a R-43; las mediciones de la v4.4 van en sus salidas). **Los guiones y sus salidas** de la v4.4: `make_lists_475.py` (`make_lists_475-out.txt`); `green_pbt4b.py` y su volcado (`v44/dump_green_b.py`, `v44/green_b/`); `walk_guards_pbt4b.py` y sus salidas (`v44/pbt4b-guard-walk.tsv` y `-out.txt`, la primera vuelta; `v44/pbt4b-guard-walk-v44.tsv` y `-out.txt`, con el GREEN de la v4.4 y la lista de 475; `v44/flagged_walk_b-out.txt`); `v44/probe_green_breaks.py` (las siete de D-S1); `v44/measure_rg_pbt4b.py` (D-S2); `make_copies_pbt4b.py`, `pbt4b_hand_edits.py` y el modo de medida (`v44/copies_probe/`); `pbt4b_skips.py`, `apply_red_pbt4b.py`, `apply_green_pbt4b.py` y su autoprueba (`selftest_pbt4b_scripts.py`, `selftest_pbt4b_scripts-out.txt`); `partition_pbt4b.py`; `check_skips_pbt4b.py`; y `make_v44.py`, que arma esta versión desde la v4.3 y los fragmentos de `v44/`.

**Abreviaturas:**
- [P], [OD], [P2] y [P3]: el paquete de la fase v3.1, la disposición del Owner y los paquetes de PB-T2 v2.9 y PB-T3 v4.7;
- [PT1]: el paquete de PB-T1, `docs/initiatives/I-64-f4b-pbt1-gate-contract-package.md`;
- [PKC2]: el paquete del contrato de F4-T4C2, del que F4B copia las paradas estándar (como [P3] §6.0);
- [DP]: las decisiones del Owner sobre el procedimiento de F4B (`owner-f4b-procedure-decisions.md`);
- DISP: `F4B-DRAFTS/f4b-dispositions.md`;
- WHC: `src/RackCad.Application/Workspace/WorkspaceHostCore.cs`;
- WEI: `src/RackCad.Application/Workspace/WorkspaceEditIntent.cs`;
- NH: `src/RackCad.Plugin/Workspace/WorkspaceNavigationHost.cs`;
- RIA: `src/RackCad.Plugin/Views/RackPilotIntentAdapter.cs`;
- FVM: `src/RackCad.UI/Workspace/WorkspacePilotFieldViewModel.cs`;
- PV: `src/RackCad.UI/Workspace/WorkspacePanelView.xaml(.cs)`;
- W: `src/RackCad.UI/Systems/PushBack/RackPushBackSystemWindow.xaml(.cs)`;
- WCo: `src/RackCad.Plugin/Workspace/WorkspaceController.cs`;
- PP y UR: el plan y la corrida de `Systems/PushBack/Pilot/`;
- EIH: `tests/RackCad.Tests/Workspace/WorkspaceEditIntentHostTests.cs`;
- EIG: `…/WorkspaceEditIntentGuardTests.cs`;
- WLG: `…/WorkspaceLifecycleGuardTests.cs`;
- PEG: `…/WorkspacePilotEditGuardTests.cs`;
- RG: `…/WorkspacePilotReconciliationGuardTests.cs`;
- CAG: `…/WorkspaceCommandAdvisorGuardTests.cs`;
- WBSG: `…/WorkspaceBridgeSourceGuardTests.cs`;
- WNAT: `…/WorkspaceNavigationAuthorityTests.cs`;
- WHSG: `…/WorkspaceHostSourceGuardTests.cs`;
- URT: `…/WorkspacePilotUpdateRequestTests.cs`;
- IG: `…/WorkspacePilotUpdateIntegrationGuardTests.cs`;
- WPP: `…/WorkspacePilotPrepareTests.cs`; WPRH: `…/WorkspacePilotReconciliationHostTests.cs`; WPUH: `…/WorkspacePilotUpdateHostTests.cs`; WPUG: `…/WorkspacePilotUpdateGuardTests.cs`;
- F5-A: la fase de I-64 que sigue a F4B (la del optimizador del layout), donde el roadmap aplaza lo que F4B no hace.

---

## Registro de cambios v4.3 → v4.4

| Id | Hallazgo | Qué cambia | Dónde |
|---|---|---|---|
| **V44-01** | **Cinco guardas fuera de la lista caen con el diseño** (medidas sobre el GREEN literal; §17 de la revisión) | **La decisión del Owner, «Añadirlas a la lista»:** la lista pasa a 475 (`pbt4b-substitution-list-475.tsv`, `654c3b7b…0707c`), con EIG:871 y :883 (el diagnóstico de admisión de la lectura completa), WLG:627 y :642 (el marcado y la recarga del borrador completo) y WPUG:823 («Actualizar» con `ExecuteFull`); la de PB-T4d pasa a 138 (`d5467dc1…387c`); el `MinSelected` de WPUG baja a 85. Cada una con su equivalente literal sobre el modelo completo; el de G-4 trae la autoprueba que IG:510-517 ya no discrimina | D-PBT4-11; §4.2; §6.2; §6.4; §8.3; §8.5 |
| **V44-02** | **D-S2: «Conservar» con dos «Sí»** | El paso 1 de la v4.3 (`RequestKeepMine` llamaba al puerto) rompía RG:104 y RG:128, que no están en ninguna lista. `RequestKeepMine` sigue preguntando desde la memoria; el primer «Sí» llama a `TryKeepMine` sin testigo y presenta; el segundo, con el testigo vigente, hace `KeepOn`. La línea de RG:132 no cambia y la petición lleva el testigo. Medido. Cambio visible, en PBT4B-C03 | D-PBT4-08; §4.2; §6.2.1 |
| **V44-03** | **D-S1: siete copias mecánicas no pueden pasar en el GREEN** | Pasan a equivalentes escritos a mano, con su texto literal: 368/60, y el final, tras la precondición 6. Los 60 y los 19 equivalentes de las guardas, literales, por `make_copies_pbt4b.py` con `pbt4b_hand_edits.py`; el Worker los copia byte a byte | D-PBT4-11; §5.2; §6.2 |
| **V44-04** | **El kit, literal** | `pbt4b-kit-WorkspaceFullReadKit.cs`: los tres tipos con los miembros de los del kit del piloto y las lecturas del piloto (D-S3), y `FullHandKit`, el apoyo de los equivalentes; un `Reread` hecho a mano lleva su gemelo completo | D-PBT4-11; §6.2.1 |
| V44-05 | Las copias, partidas | Cada copia, en `<Clase>Full.cs` junto a su original, en `…FullPanelCopies`, con su clase partida en `FullRedTests` y `FullGreenTests` y, en cada una, solo los ayudantes que alcanzan sus pruebas (un campo privado sin uso sería un `warning CS`). La partición, medida por la sesión con `partition_pbt4b.py` | D-PBT4-11; §6.2.1 |
| **V44-06** | **D-S3: tres correcciones del paquete** | El testigo se lee con `var pilot = read.PilotRead;` en **cuatro** sentencias (EIG:743); el texto del testigo de `Presented` y la admisión de «Conservar» salen de la función pura de `Draft/` (`PushBackKeepMinePresentation.Read`, con `OriginText`, `Diagnostic` e `IsAdmitted`), y WHC solo nombra la lectura en sus consumidores; `FullPrepareKit` conserva los tipos de lectura del piloto | D-PBT4-04; D-PBT4-08; D-PBT4-09; D-PBT4-11 |
| **V44-07** | El encargo de la v4.4 | **§5.2 entero:** CT-PBT4B-01 a 20 con sus líneas, su clase y su `ExpectRed`; las líneas de CT-06 (la presentación «Por grupos»), deducidas del clasificador real y fabricadas, a confirmar en la precondición 6; **los totales exactos:** GREEN Core 19 601 = 19 146 + 0 + 455 y UI 2 481 = 2 433 + 0 + 48; el RED, con lo que mide la partición | §5.2 |
| **V44-08** | El encargo de la v4.4 | **§8.5:** las tres tandas del Owner sobre PB-T4 y la decisión de la sesión, citadas | §8 |
| **V44-09** | El encargo de la v4.4 | **§6.2 entero:** el `AllowedWriteScope` ruta a ruta (53), `UNIT_CHANGE` (WHC, WEI, FVM, PV y `NumericFieldValidation.cs`; ni WCo ni `WorkspacePilotSupport.cs`, que el GREEN no toca), el RED y el GREEN, el bloque de literales (§6.2.1: `TaskId`, `Objective`, INV-PBT4B-01 a 14, PBT4B-C01 a C20 y `ExpectedEvidence` [0] a [15]), los `RequiredTests`, los `Skip`, el kit, la partición y lo que necesita el generador (§6.2.2); y las filas de PB-T4b de la tabla de RR-3 (§6.0) | §6.0; §6.2 |
| V44-10 | La versión | El `Objective` de PB-T4b cita la v4.4; el de PB-T4a sigue citando la v4.3, con la que se emitió | §6.1.1; §6.2.1 |

