> **Redacción para el repo público:** las rutas locales se sustituyen por marcadores (`<checkout-principal>`, `<carpeta-del-paquete>`). El resto no cambia.

# I-64 F4B — Paquete del contrato de PB-T4 («Editar todo» en el panel del Workspace), v4.1

**Estado:** v4.1. Aplica:
- la §15 de la revisión independiente (`independent-review-v1.md`): **LISTO TRAS CORRECCIONES DE TEXTO, B/I/M = 0 / 3 / 4** (I-22, I-23, I-24 y M-28 a M-30);
- **las cuatro decisiones del Owner** (`owner-pbt4-decisions.md`, 2026-11-20): «Retirarlo», «Por grupos», «Saltar y duplicar» y «Sí, con condiciones». Las ramas no elegidas salen del diseño, de las CT y de los contratos, y quedan en §8 como decisión tomada;
- **las siete dudas del generador de PB-T4a** (`generator-doubts-pbt4a.md`), con lo que decidió la sesión.

**La base:** el cierre de PB-T3c, `7f0d9aa3` (F4B-E48; GREEN `938a35f0`, CI `44757675821`): Core 19 129 = 19 123 + 6 omitidas; UI 2 451 = 2 430 + 21.

**Cómo se midió:** solo por objetos git en `7f0d9aa3` (`git show`, `git ls-tree` y `git cat-file --batch`), con Python `-I` y guiones escritos en `<carpeta-del-paquete>`. Nada se compiló ni se ejecutó, y el worktree no se leyó.

**Lo que sale de esta versión, para la sesión, antes de nada** (§8.3):
- **PB-T4a y PB-T4c están dentro de la cota del Owner:** 1 y 2, exactos.
- **PB-T4b no lo está.** Sin la retirada, {{N_SUB}} frente a «unas 465» (+{{PCT_SUB}} %). Lo que añade «Retirarlo» es {{N_C7}} frente a «unas 100» (+{{PCT_C7}} %): {{N_C7TEXT}} por texto y {{N_C7COMP}} que dejan de compilar al borrar lo retirado (por ejemplo, las pruebas de la corrida del piloto que usan `PilotUpdateSetup.Run`). El total, {{N_B}} frente a unas 565 (+{{PCT_B}} %).
- **Y con «Retirarlo», {{N_NINE_COMPILE}} de las 428 de las 9 clases no se pueden saltar,** porque dejan de compilar al borrar lo retirado (un `Skip` no evita un error de compilación); habría que borrarlas con registro, con {{N_SUPPORT}} miembros de apoyo que nombran lo retirado. Q3 decía «No hace falta ninguna excepción a la regla de no editar pruebas ni ayudantes».
- **Y las copias de «Saltar y duplicar» dan un resultado mixto en el RED** (unas fallan y otras pasan sobre el esqueleto), que ningún filtro admite (§5.2).
- **Las tres cosas vuelven al Owner antes de emitir PB-T4b** (pregunta 4: «un efecto material sin medir» y «la lista que se emite es esa o una parte de ella»). **No afectan a PB-T4a,** que puede emitirse: su lista es 1, su diseño no retira nada y su comprobación no depende de PB-T4b.

**Los hechos,** en `pbt4-measurements.md` (R-0 a R-38; R-34 a R-38 son de la v4.1). **Los guiones y sus salidas:**
- v1 a v4: `measure_pbt4.py`, `measure_pbt4_v2.py`, `measure_pbt4_v3.py` y `measure_pbt4_v4.py`, con `measure_pbt4-out.txt`, `measure_pbt4_v2-out.txt`, `measure_pbt4_v3-out.txt` y `measure_pbt4_v4-out.txt`. La revisión midió con `ir4_measure.py` y `ir4_measure_v3.py` (`ir4_measure-out.txt` e `ir4_measure_v3-out.txt`);
- **v4.1:**
  - `lists_core.py` (el núcleo común: objetos git, el léxico de C# y los miembros con su texto);
  - `lists_pbt4.py` → `pbt4-substitution-lists.md` y `pbt4-substitution-lists.tsv`, con `lists_pbt4-out.txt`;
  - `copy_map_defs.py` y `copy_map_pbt4.py` → `pbt4-copy-map.tsv` y `copy_map_pbt4-out.txt`;
  - `retire_compile_pbt4.py` → `pbt4-retire-compile.tsv` y `retire_compile_pbt4-out.txt`;
  - `measure_pbt4_v41.py` → `measure_pbt4_v41-out.txt` (las guardas de PB-T4a sobre el texto literal del GREEN);
  - `read_pbt3c_contract.py` → `pbt3c-contract-dump.txt` (los literales que copia el §6);
  - las sondas `probe_members.py`, `probe_anchors.py`, `probe_callers.py`, `probe_retired.py`, `probe_copy_residue.py`, `probe_nh_counts.py` y `probe_overlap.py` (`probe_overlap-out.txt`);
  - `make_v41.py`, que arma esta versión desde la v4 y los fragmentos de `v41/`.

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
- URT: `…/WorkspacePilotUpdateRequestTests.cs`;
- IG: `…/WorkspacePilotUpdateIntegrationGuardTests.cs`;
- F5-A: la fase de I-64 que sigue a F4B (la del optimizador del layout), donde el roadmap aplaza lo que F4B no hace.

---

## Registro de cambios v4 → v4.1

| Id | Hallazgo o decisión | Qué cambia | Dónde |
|---|---|---|---|
| **V41-01** | **Las decisiones del Owner** (Q1 a Q4) | Las ramas no elegidas salen: «Conservarlo» y «Dos modelos» (Q1), «Por campos» y «Por campos con valores» (Q2), «Editar una línea» y «la referencia» (Q3), y las emisiones parciales (Q4). Quedan en §8.2, con la cita del registro. EIH no se edita; no hay `PushBackFieldLabels` ni `PushBackPresentationForm` | D-PBT4-09 a 11; §5; §6; §8 |
| **V41-02** | **I-24** | Una CT por subtarea con G-L1 (copiada de CT-PBT2A-04, 15 autopruebas) y la pureza (copiada de CT-PBT3A-14) sobre **sus** archivos nuevos de `Draft/`: **CT-PBT4A-08** (`PushBackOriginByText.cs`), **CT-PBT4B-19** (`PushBackFullFieldAdapter.cs`, `PushBackKeepMinePresentation.cs` y `PushBackAnnotationScaleRule.cs`, con la línea de `Apply`) y **CT-PBT4C-15** (`PushBackFullDraftLaunch.cs` y `PushBackFullDraftStatePort.cs`, con la de `ApplyToDraft`). Así la condición 10 de D2 no deja de cubrirlos en silencio. 32, 33 y 33 líneas; sus filas de RR-3 | §5.1 a 5.3; §6.0 |
| **V41-03** | **I-22, I-23** | **Las listas, guardadas y exactas,** solo de lo elegido: `pbt4-substitution-lists.md` (y `.tsv`). La UI de la elección, recontada: {{N_UI}} (21 de `WorkspacePilotReconciliationTests` y 6 de `WorkspacePilotReconciliationViewTests`). Lo que añade «Retirarlo», sin las ya contadas, por texto y por compilación. **Los números, frente a la cota, en §8.3: PB-T4b la supera** | D-PBT4-11; §8.3 |
| **V41-04** | **El guion de «Saltar y duplicar»** (mensaje de la sesión, punto 3) | El mapa cerrado M (M-00 a M-08), el kit nuevo y **CT-PBT4B-20,** la CT que comprueba, clase a clase, que cada copia es el original con el mapa y nada más. {{N_MECH}} copias mecánicas y {{N_HAND}} «a mano», con su motivo | D-PBT4-11; §5.2 |
| **V41-05** | **I-08** («Por grupos») | Los nueve nombres fijos, la unión (`A`, `A y B`, `A, B y C`), el plural de las vistas y las líneas de lo estructural y de las vistas, literales | D-PBT4-09 |
| **V41-06** | **Duda 4 del generador** (`RackPilotIntentAdapter.Read` y NH:242) | **Medido:** EIG:139-141 fija la firma entera de `Read`, hasta `RackCatalog catalog)`. Un parámetro nuevo, obligatorio **o con valor por defecto,** la rompe (contrafactual en rojo, 2 de 2), y esa guarda no está en la lista de PB-T4a. **Lo que menos rompe:** la firma no cambia; RIA gana `AtOrigin`, una relectura pura sobre el escaneo ya tomado, y NH, que ya estaba en el alcance y en `UNIT_CHANGE`, la llama con el texto del origen. Ninguna guarda prohíbe parámetros opcionales en el Plugin; la que fija la firma lo decide | D-PBT4-02 y 03 |
| **V41-07** | **Duda 5** (las guardas que buscan `ReadIntent` sin anclar) | Medidas una a una sobre el texto literal del GREEN (WHC con la sentencia en los cinco cuerpos; NH y RIA con sus ediciones literales): **32 de 32 en verde,** con EIG:579, 604-617, 636, 925 y 1124, WLG:369, 442, 623, 634, 656 y 687, y PEG:288 y 297 | §4.1 |
| **V41-08** | **Duda 3** (`MinSelected`) | `PushBackDraftWriteGuardTests` baja de 3 a **2** en PB-T4a, porque el `Skip` de CT-PBT3A-12 cae en ese filtro heredado | §6.1 |
| **V41-09** | **Duda 3** (los totales con la CT de I-24) | CT-PBT4A-08 **pasa en el RED** (es una guarda sobre un archivo que el esqueleto ya crea). **Una sola rama** (F4B-E46): RED 19 137 = 19 125 + 5 `Failed` + 7 `Skipped`; GREEN 19 137 = 19 130 + 7. Un `[Fact]` por CT | §5.1; §6.1 |
| **V41-10** | **Duda 6** (el `Skip` en el parche del esqueleto) | El parche lleva `src/` y el `Skip` de CT-PBT3A-12, con su texto exacto. La suite de Core sobre la copia con el esqueleto: **19 129 = 19 122 + 7,** comprobado | §6.1 |
| **V41-11** | **Duda 1** (el TaskId) | La regla es **«como mucho 26 caracteres»** (MAX_PATH solo acota por arriba). `F4B-PBT4-FULL-READ-PATH` tiene 23; los nombres no cambian | §0; §6.0 |
| **V41-12** | **Duda 7** (sin remisiones) | §5.0, §5.1, §6.0 (con la identidad, las paradas comunes con su texto, RR y C11) y §6.1, enteros. Los literales del contrato de PB-T4a, en un bloque, para la comparación del generador | §5; §6 |
| **V41-13** | **M-28** | El texto desfasado: el encabezado del §2 (`7f0d9aa3`), el `<a medir>` de CAG en D-PBT4-02 (medido: verde, con la restricción de no añadir motivos) y la cabecera (R-0 a R-38 y todos los guiones) | Cabecera; §2; D-PBT4-02 |
| **V41-14** | **M-29** | Se declara que el esqueleto del RED de PB-T4b **no toca** `NumericFieldValidation.cs`: la delegación es del GREEN | D-PBT4-06; §6.2 |
| **V41-15** | **M-30** | CT-PBT4C-08 fija el literal de la conversión de `object` en el abridor: `var window = RackPushBackSystemWindow.ForDraft((PushBackDraftEditorLaunch)launch);` | §5.3 |
| V41-16 | El causa de CT-PBT3A-12 | La rompe RIA, que nombra `PushBackFullDraftReader` (uno de sus cinco nombres); no `PushBackOriginByText`, que no está entre ellos | §4.1 |
| V41-17 | Dudas 7 y 8 | La tabla de motivos de CAG (5 en `Read` de RIA, 3 en `ReadOnActiveDocument` de NH, dos `ex.Message`) y la reflexión sobre WHC siguen en verde, medidas; CT-PBT4A-07 fija `literales=0` en `AtOrigin` | §4.1; §5.1 |
| V41-18 | El generador de PB-T4a | La lista de lo que necesita: alcance, prohibido, invariantes, paradas, `RequiredTests` con `MinSelected`, evidencia con los totales exactos de la rama única y autoridades | §6.1.2 |

