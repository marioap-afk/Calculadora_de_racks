# I-64 F4B — Paquete del contrato de PB-T4 («Editar todo» en el panel del Workspace), v4.5

**Estado:** v4.5. Cierra la parte de **PB-T4b** sobre el código real ya cerrado de PB-T4a, con lo que llegó tras la v4.4:
- **la decisión del Owner sobre el coste de la lectura de PB-T4a,** «Cerrar y optimizar en PB-T4b» (`owner-pbt4a-cost-decision.md`), que la sesión desarrolla como **D-S4**: una sola materialización del modelo completo por lectura (D-PBT4-21);
- **la quinta tanda del Owner sobre PB-T4b,** «Añadirlas a la lista» (`owner-pbt4b-list-477-decision.md`): la lista pasa a **477**, con CT-PBT4A-03 y CT-PBT4A-05;
- **la revisión de la v4.4** (§18 de `independent-review-v1.md`: NECESITA v4.5, 0/2/6, sin decisiones para el Owner): I-01, I-02 y M-01 a M-06.

**PB-T4a está CERRADA** (F4B-E51, `78a7cef3`, sobre el GREEN `faef7020`) y no cambia: su texto de §5.1, §6.1, §6.1.1 y §6.1.2 queda byte a byte como en la v4.3 y la v4.4, con «v4.3» en su `Objective`. PB-T4c y PB-T4d, sin cambio de fondo.

**La base de PB-T4b:** el cierre de PB-T4a, **`78a7cef3`**, cuyo `src/` y cuyos `tests/` son los del GREEN `faef7020`. Se resuelve así lo que la v4.4 marcaba `<tras el cierre de PB-T4a>`: el nombre real de CT-PBT4A-06, los `BaseSha` de los 20 archivos de origen y la línea base de la CI (`44814418683`). Lo que dependa de ejecutar va marcado **`<medido en la precondicion 6>`** (ASCII, como en el bloque literal), con su regla.

**Cómo se midió:** solo por objetos git en `78a7cef3`, con Python `-I` y guiones escritos en `<carpeta-del-paquete>`. Nada se compiló ni se ejecutó, y el worktree no se leyó. El GREEN literal de PB-T4a (`green_pbt4a.py`) es, byte a byte, el de `faef7020` en sus seis archivos de `src/`.

**Lo que sale de esta versión (PB-T4b):**
- **la lista de 477** (`pbt4b-substitution-list-477.tsv`, sha256 `5975c5b6b9111c3a4048193b5ebf10b619a567996dea8707b88ff6a00105a651`): las 475, sin cambio y en su orden, y CT-PBT4A-03 y CT-PBT4A-05 al final; las de 470 y 475 quedan como historia;
- **D-S4:** RIA difiere la lectura completa del `Primary` (`System.Lazy`), y la relectura con el origen encontrado ya no la materializa; RIA entra en el alcance como `UNIT_CHANGE`, con su lista cerrada de cambios; **CT-PBT4B-21** lo fija; **la sonda del coste** (`pbt4b-read-cost-probe.cs`) va en la precondición 6;
- **el GREEN literal de `src/`** (`green_pbt4b_v45.py`: el de la v4.4 y RIA, diez archivos) y **`apply_green_pbt4b_v45.py`**; el RED, el de la v4.4 (RIA no cambia en él), con **`apply_red_pbt4b_v45.py`**: el esqueleto, los 477 `Skip`, el kit y las 20 copias;
- **el kit de la v4.5** (`pbt4b-kit-WorkspaceFullReadKit-v45.cs`: `FullReadRig`, M-04 y M-05) y **el guion de la copia de la v4.5** (`make_copies_pbt4b_v45.py` con `pbt4b_hand_edits_v45.py`), con su revisión (`pbt4b-equivalents-review-v45.txt`, 81 equivalentes);
- **los `Skip` literales** (`pbt4b_skips_v45.py`) y **su comprobación** (`check_skips_pbt4b_v45.py`, 20 archivos y 477 `Skip`);
- **la partición con la regla de INV-PBT4B-02** (`partition_pbt4b_v45.py`, I-02) y **dos pasos más en la precondición 6:** la sonda de las líneas «presentar» de CT-06 (`pbt4b-ct06-presentar-probe.cs`) y la suite completa sobre el RED exacto, con sus totales;
- **CT-PBT4B-20 corregida** (I-01) y **§5.2, §6.2 y §8.5 al día**, con los totales exactos de cada rama.

**Los hechos,** en `pbt4-measurements.md` (R-0 a R-43; las mediciones de la v4.4 y la v4.5 van en sus salidas). **Los guiones y sus salidas** de la v4.5: `make_list_477.py` (`v45/make_list_477-out.txt`); `green_pbt4b_v45.py`; `check_ria_v45.py` (`v45/check_ria_v45-78a7-out.txt`: CT-PBT4A-07 con su código real, CT-PBT4B-21 y las cuentas de B-08, CAG, G-9 e INV-14); `walk_guards_pbt4b_v45.py` (`v45/pbt4b-guard-walk-v45-delta2.tsv` y `-out.txt`, solo lo que cambia D-S4; `v45/pbt4b-guard-walk-v45-full.tsv` y `-out.txt`, todo el GREEN sobre el cierre de PB-T4a); `ria_added_lines_v45.py` (`v45/ria_added_lines_v45-out.txt`); `compare_walks_v45.py` (`v45/compare_walks_v45-out.txt`); `make_copies_pbt4b_v45.py`, `pbt4b_hand_edits_v45.py` y el modo de medida (`v45/copies_probe_v45/` y `v45/copies_probe_v45-out.txt`); `pbt4b_skips_v45.py`, `apply_red_pbt4b_v45.py`, `apply_green_pbt4b_v45.py`, `check_skips_pbt4b_v45.py` y `partition_pbt4b_v45.py`; su autoprueba y las medidas que se citan (`selftest_pbt4b_v45.py`, `v45/selftest_pbt4b_v45-out.txt`) y la de los dos guiones de escritura sobre una copia mínima (`selftest_apply_v45.py`, `v45/selftest_apply_v45-out.txt`: el RED escribe 47 archivos, con 477 `Skip` y 20 copias, y el GREEN, 10); las dos sondas de la precondición 6 (`pbt4b-read-cost-probe.cs` y `pbt4b-ct06-presentar-probe.cs`); y `make_v45.py`, que arma esta versión desde la v4.4 y los fragmentos de `v45/`.

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
- FRP, FRG y FRS: `tests/RackCad.Tests/PushBackFullReadPathTests.cs`, `…/PushBackFullReadGuardTests.cs` y `…/PushBackFullReadSupport.cs`, las clases de las CT de PB-T4a;
- F5-A: la fase de I-64 que sigue a F4B (la del optimizador del layout), donde el roadmap aplaza lo que F4B no hace.

---

## Registro de cambios v4.4 → v4.5

| Id | Hallazgo | Qué cambia | Dónde |
|---|---|---|---|
| **V45-01** | **Dos pruebas de PB-T4a fuera de la lista caen con el diseño** (CT-PBT4A-03 y CT-PBT4A-05: sus ayudantes buscan el borrador del piloto o leen sin la lectura completa; la v4.4 solo tenía su especificación) | **La decisión del Owner, «Añadirlas a la lista»:** la lista pasa a 477 (`pbt4b-substitution-list-477.tsv`, `5975c5b6…a651`). Sus equivalentes, escritos a mano: el mismo ayudante sobre el borrador completo (`FullReadRig`, en el kit) y, en CT-05, un lector sin la interfaz que responde `FromReads` (`FullEditIntentFakeReader`). 20 archivos de los `Skip` y 20 copias | D-PBT4-11; §4.2; §5.2; §6.2; §8.3; §8.5 |
| **V45-02** | **D-S4: una sola materialización por lectura** (`owner-pbt4a-cost-decision.md`) | RIA difiere la lectura completa del `Primary` en un `System.Lazy` sobre el mismo `input`, y `Full` es `_full?.Value`; `AtOrigin`, NH, `ReadAt` y WHC no cambian. RIA entra en el alcance como `UNIT_CHANGE`; CT-PBT4B-21 lo fija en texto; la sonda del coste, en la precondición 6. Ninguna prueba cae: CT-PBT4A-07, con su código real, da sus 13 líneas; B-08, EIG:139-141, CAG y G-9 no cambian | D-PBT4-21; §4.2; §5.2; §6.2; §11 |
| **V45-03** | **I-01: CT-PBT4B-20 no podía pasar en el RED** | La copia mecánica es su original «con M-00 a M-08», con los tipos de M-00 de cada clase literales; se compara con el texto del método en `BaseSha` (sin el `Skip`); `BaseSha` y `CopySha` son el sha256 del texto con los saltos de línea a `\n` (`core.autocrlf=true`) | §5.2; §6.0 (RR-3) |
| **V45-04** | **I-02: la partición y la precondición 6** | La partición usa la regla de INV-PBT4B-02 (la primera línea del StackTrace bajo `tests/`; `partition_pbt4b_v45.py`); la precondición 6 gana la sonda de las líneas «presentar» de CT-06 y la suite completa sobre el RED exacto, con sus totales (Core 19 586 y UI 2 478, sin las CT) | §5.2; §6.2.1; §11 |
| V45-05 | M-01: la copia de EIG | `BaseOrReadUse` busca `pilot` y `full` | D-PBT4-11 |
| V45-06 | M-02: las vistas | Hasta tres líneas de vistas, dicho y medido (CT-06, `of lineas=7`), y declarado en F4B-E52 | D-PBT4-09 |
| V45-07 | M-03: las 28 copias idénticas | **Se quedan en la lista,** declaradas como copias sin cambio: la lista es la que vio el Owner, y una copia idéntica que cayera sería D-S1, no una vuelta al Owner | D-PBT4-11; §6.2.1 `[13]` |
| V45-08 | M-04 y M-05: el kit | `FullPrepareKit.Reread` rechaza un estado (`Assert.Null(state)`); `FullHandKit.FullOfMatchesTheReader`, que CT-20 corre sobre seis escaneos | D-PBT4-11; §5.2 |
| V45-09 | M-06: el tamaño | El contrato y las listas nunca van en línea en un prompt (30 000 bytes; la cadena los cita por ruta y sha256) | §6.2.2 |
| V45-10 | La base | El cierre de PB-T4a, `78a7cef3`: el nombre de CT-PBT4A-06, los `BaseSha`, la CI de la línea base; el `ForbiddenWriteScope`, por la regla del generador sobre el alcance final (con el de la v4.4, 407 en `78a7cef3`) | §6.2; §6.2.1; §6.2.2 |
| V45-11 | Los totales | GREEN Core **19 604 = 19 147 + 0 + 457** y UI **2 481 = 2 433 + 0 + 48**; el RED, con R_C y R_UI; sin las CT, Core **19 586** y UI **2 478** | §5.2; §6.2.1 `[14]`; §11 |
| V45-12 | §8 y las autoridades | §8.5: las cinco tandas del Owner sobre PB-T4, la de PB-T4a sobre el coste y D-S1 a D-S4; las autoridades de PB-T4b, sin las de la cadena de PB-T4a | §1; §6.2.2; §8.5 |
| V45-13 | La versión | El `Objective` de PB-T4b cita la v4.5 y CT-PBT4B-01..21; el de PB-T4a sigue citando la v4.3 | §6.2.1 |

