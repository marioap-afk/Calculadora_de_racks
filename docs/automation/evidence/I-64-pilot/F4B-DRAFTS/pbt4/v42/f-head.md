> **Redacción para el repo público:** las rutas locales se sustituyen por marcadores (`<checkout-principal>`, `<carpeta-del-paquete>`). El resto no cambia.

# I-64 F4B — Paquete del contrato de PB-T4 («Editar todo» en el panel del Workspace), v4.2

**Estado:** v4.2. Aplica:
- la §16 de la revisión independiente (`independent-review-v1.md`), sobre la v4.1: **NECESITA v4.2, B/I/M = 1 / 2 / 4** (B-08, I-25, I-26 y M-32 a M-35);
- **las dos decisiones nuevas del Owner** (`owner-pbt4b-retire-list-decisions.md`, 2026-11-20): «Retirar tras PB-T4c» y «Sí, esa lista»;
- y, como en la v4.1, las cuatro primeras (`owner-pbt4-decisions.md`) y las siete dudas del generador de PB-T4a (`generator-doubts-pbt4a.md`).

**La base:** el cierre de PB-T3c, `7f0d9aa3` (F4B-E48; GREEN `938a35f0`, CI `44757675821`): Core 19 129 = 19 123 + 6 omitidas; UI 2 451 = 2 430 + 21.

**Cómo se midió:** solo por objetos git en `7f0d9aa3` (`git show`, `git ls-tree` y `git cat-file --batch`), con Python `-I` y guiones escritos en `<carpeta-del-paquete>`. Nada se compiló ni se ejecutó, y el worktree no se leyó.

**Lo que sale de esta versión:**
- **PB-T4a, lista para su comprobación acotada:** B-08 arreglado con `ReadAt` en `Draft/`; el recorrido entero de sus guardas (1 026 pruebas en 46 clases) da una sola que cae, la de su lista; **los totales no cambian** (RED 19 137 = 19 125 + 5 + 7; GREEN 19 137 = 19 130 + 7; esqueleto 19 129 = 19 122 + 7).
- **PB-T4b:** la lista exacta de **470**, aprobada por el Owner y fijada por sha256 (`pbt4b-substitution-list.tsv`, `44393b19d1f7490081c8ec9bac633ea4acbfe792103e5e5c3ca5e1f3fa1163e4`); no retira nada, ni borra ni edita ninguna prueba ni ningún ayudante.
- **PB-T4d,** nueva, después de PB-T4c: la retirada del piloto, con su lista (139 sustituciones, 55 originales y 29 miembros de apoyo; `pbt4d-retire-list.tsv`). Solo esbozo; **vuelve al Owner con su lista antes de emitirse.**

**Los hechos,** en `pbt4-measurements.md` (R-0 a R-41; R-34 a R-38 son de la v4.1 y R-39 a R-41, de la v4.2). **Los guiones y sus salidas:**
- v1 a v4: `measure_pbt4.py`, `measure_pbt4_v2.py`, `measure_pbt4_v3.py` y `measure_pbt4_v4.py`, con `measure_pbt4-out.txt`, `measure_pbt4_v2-out.txt`, `measure_pbt4_v3-out.txt` y `measure_pbt4_v4-out.txt`. La revisión midió con `ir4_measure.py` y `ir4_measure_v3.py` (`ir4_measure-out.txt` e `ir4_measure_v3-out.txt`);
- v4.1: `lists_core.py`, `lists_pbt4.py`, `copy_map_defs.py`, `copy_map_pbt4.py`, `retire_compile_pbt4.py`, `measure_pbt4_v41.py`, `read_pbt3c_contract.py`, las sondas `probe_*.py` y `make_v41.py`, con sus salidas;
- **v4.2:**
  - `green_pbt4a.py` (el texto literal del GREEN de PB-T4a, en memoria, con `ReadAt`);
  - `walk_guards_pbt4a.py` → `pbt4a-guard-walk.tsv`, `pbt4a-guard-walk-out.txt` y `pbt4a-guard-walk-flagged.txt` (el recorrido entero, I-25);
  - `make_measure_v42.py` → `measure_pbt4_v42.py` → `measure_pbt4_v42-out.txt` (las aserciones traducidas, con EIG:350-364);
  - `split_lists_pbt4.py` → `pbt4b-substitution-list.tsv` y `pbt4d-retire-list.tsv`, con `split_lists_pbt4-out.txt`;
  - la sonda `probe_tests_text.py`, para leer a mano las pruebas señaladas;
  - `make_v42.py`, que arma esta versión desde la v4.1 y los fragmentos de `v42/`;
  - `lists_core.py`, corregido (R-T4-25), y las listas regeneradas con él: iguales byte a byte.

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
- F5-A: la fase de I-64 que sigue a F4B (la del optimizador del layout), donde el roadmap aplaza lo que F4B no hace.

---

## Registro de cambios v4.1 → v4.2

| Id | Hallazgo o decisión | Qué cambia | Dónde |
|---|---|---|---|
| **V42-01** | **«Retirar tras PB-T4c»** (pregunta 1 de la segunda vuelta) | El código del piloto **no se retira en PB-T4b**. Hay una subtarea nueva, **PB-T4d**, después de PB-T4c, con todo lo de la retirada y la causa C-7: sustituye las 139 y borra con registro las 55 originales saltadas y los 29 miembros de apoyo, y vuelve al Owner con su lista antes de emitirse. De PB-T4d hay un esbozo con su lista (`pbt4d-retire-list.tsv`, sha256 `1c9d67e8…8b71`); su contrato viene después. PB-T4b queda sin borrar ni editar ninguna prueba: su `UNIT_CHANGE` y su alcance no tienen ningún archivo de `Pilot/` | D-PBT4-04, 06, 10 y 20; §0; §6.0, §6.2 y §6.4; §8; §11 |
| **V42-02** | **B-08** | `PushBackOriginByText.ReadAt(scan, originText, catalog)`, en `Draft/`, hace las dos relecturas puras y devuelve una tupla (el escaneo, la del piloto y la completa) o `null`; `AtOrigin` solo la llama. RIA conserva un `PushBackPilotIntentReader.Read(` y un `new PushBackPilotIntentScan(` (EIG:350-364, medido en verde). Cambian con él: PBT4A-C13 (`ReadAt`) e INV-PBT4A-04; las líneas de CT-PBT4A-04 (`readat …`) y de CT-PBT4A-07 (`ria-lectores …`, `ria-AtOrigin readat=1 piloto=0 completo=0 …` y `autoprueba-segunda-lectura-del-piloto-en-RIA=falla`); la pureza de CT-PBT4A-08, que admite los dos lectores; el esqueleto (`ReadAt` inerte, `=> null;`). **Los totales no cambian** (comprobado) | D-PBT4-03; §5.1; §6.1 |
| **V42-03** | **«Sí, esa lista»** (pregunta 2 de la segunda vuelta) | La lista de PB-T4b, **470**, separada en `pbt4b-substitution-list.tsv` y fijada por sha256 (`44393b19d1f7490081c8ec9bac633ea4acbfe792103e5e5c3ca5e1f3fa1163e4`), con la regla de filtrado del archivo completo (`f24c2dbc…`). El generador de PB-T4b comprueba las dos cosas | D-PBT4-11; §6.2; §11 |
| **V42-04** | **I-25** | **El recorrido entero,** por diferencia, de las 1 026 pruebas de las 46 clases que leen RIA, NH, WHC, WEI, `Application/Workspace/` o sus carpetas, sobre el texto literal del GREEN: 764 no las leen, 162 las leen sin ninguna diferencia, y las 100 señaladas se leyeron una a una (cae una, CT-PBT3A-12, la de la lista). «D = 0», corregido: en el RED lo confirma la precondición 6; en el GREEN, el recorrido, y lo que no ve es un riesgo con su parada (PBT4A-C16) | §4.1; §5.1; §9 |
| **V42-05** | **I-26** | Retirar después de PB-T4c **no evita el borrado** de las 55 originales y de los 29 miembros de apoyo: cuando se retire, dejarán de compilar igual. Lo que cambia es que ya no corre ninguna | D-PBT4-10; §6.4 |
| V42-06 | M-32 | De las 67 por compilación, `WorkspacePilotUpdateRunTests` da 42 y `PushBackFullWritePrepareTests`, 6; 48 y 9 son sus totales en C-7, por texto y por compilación | §6.4; §8.3 |
| V42-07 | M-33 | Las 9 de las 15 de la letra que nombran lo que se retirará caen por el cambio del anfitrión, no por la retirada: 470 es exacto con cualquier opción | D-PBT4-11; §4.2 |
| V42-08 | M-34 | La partición de las copias por su resultado en el RED (`<Clase>FullRedTests` y `<Clase>FullGreenTests`), técnica; el kit nuevo es código de prueba, literal en el contrato, y no la «implementación de referencia» | D-PBT4-11; §5.2 |
| V42-09 | M-35 | El `<summary>` de `AtOrigin` dice que una excepción de la relectura pura no se contiene en RIA, sino en NH | D-PBT4-03 |
| V42-10 | El léxico de las mediciones | `lists_core.py` tomaba `@"""…` por una cadena cruda. Corregido; las listas regeneradas son iguales byte a byte | §9; R-41 |
| V42-11 | El generador de PB-T4a | Lo que cambia respecto de la v4.1: el esqueleto (`ReadAt`), PBT4A-C13, INV-PBT4A-04, las líneas de CT-04 y CT-07 con su fila de RR-3, y «v4.2» en el `Objective` | §6.1 |

