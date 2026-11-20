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

## Registro de cambios v4 → v4.1

| Id | Hallazgo o decisión | Qué cambia | Dónde |
|---|---|---|---|
| **V41-01** | **Las decisiones del Owner** (Q1 a Q4) | Las ramas no elegidas salen: «Conservarlo» y «Dos modelos» (Q1), «Por campos» y «Por campos con valores» (Q2), «Editar una línea» y «la referencia» (Q3), y las emisiones parciales (Q4). Quedan en §8.2, con la cita del registro. EIH no se edita; no hay `PushBackFieldLabels` ni `PushBackPresentationForm` | D-PBT4-09 a 11; §5; §6; §8 |
| **V41-02** | **I-24** | Una CT por subtarea con G-L1 (copiada de CT-PBT2A-04, 15 autopruebas) y la pureza (copiada de CT-PBT3A-14) sobre **sus** archivos nuevos de `Draft/`: **CT-PBT4A-08** (`PushBackOriginByText.cs`), **CT-PBT4B-19** (`PushBackFullFieldAdapter.cs`, `PushBackKeepMinePresentation.cs` y `PushBackAnnotationScaleRule.cs`, con la línea de `Apply`) y **CT-PBT4C-15** (`PushBackFullDraftLaunch.cs` y `PushBackFullDraftStatePort.cs`, con la de `ApplyToDraft`). Así la condición 10 de D2 no deja de cubrirlos en silencio. 32, 33 y 33 líneas; sus filas de RR-3 | §5.1 a 5.3; §6.0 |
| **V41-03** | **I-22, I-23** | **Las listas, guardadas y exactas,** solo de lo elegido: `pbt4-substitution-lists.md` (y `.tsv`). La UI de la elección, recontada: 27 (21 de `WorkspacePilotReconciliationTests` y 6 de `WorkspacePilotReconciliationViewTests`). Lo que añade «Retirarlo», sin las ya contadas, por texto y por compilación. **Los números, frente a la cota, en §8.3: PB-T4b la supera** | D-PBT4-11; §8.3 |
| **V41-04** | **El guion de «Saltar y duplicar»** (mensaje de la sesión, punto 3) | El mapa cerrado M (M-00 a M-08), el kit nuevo y **CT-PBT4B-20,** la CT que comprueba, clase a clase, que cada copia es el original con el mapa y nada más. 375 copias mecánicas y 53 «a mano», con su motivo | D-PBT4-11; §5.2 |
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

## Registro de cambios v3 → v4

| Id | Hallazgo | Qué cambia | Dónde |
|---|---|---|---|
| **V4-01** | **B-07** (WCo no puede nombrar `RackCad.UI`) | **La regla de la escala pasa a Application,** sin cableado:<br>• la lógica pura de `NumericFieldValidation` (que solo usa `System.Globalization` y `LocalizedNumberParser` de `RackCad.Application.Formatting`) pasa a una clase de Application, `NumericTextRule`;<br>• `NumericFieldValidation` de la UI delega en ella, con la misma API pública y los mismos mensajes;<br>• `PushBackAnnotationScaleRule` (Application) da los parámetros de la escala; WHC la usa por defecto.<br>Hay una sola implementación y Core la tiene, así que no queda «el campo sin regla». **Medido:** ninguna guarda lee el texto de `NumericFieldValidation.cs` ni de `NumericField.cs`; las pruebas que la nombran la llaman por su API (R-30), y ninguna es de T1B por letra.<br>**El abridor, en dos piezas:**<br>• la clase anidada de WCo solo abre el modal, sin nombrar `RackCad.UI` ni `PushBack`;<br>• la ventana la abre `src/RackCad.Plugin/Views/RackWorkspaceEditorWindow.cs`, que no nombra `RackCad.Application.Workspace`.<br>**Rompe una sola guarda: CT-PBT1B-15,** el censo de archivos del Plugin con `PushBack`, que pasa de 14 a 15. Ninguna carpeta lo evita: todo archivo nuevo que abra la ventana nombra `PushBack` (R-31) | D-PBT4-06, 13 a 16; §4.3 |
| **V4-02** | **B-06** (las firmas y los números) | **Las firmas,** por método más los ayudantes de su mismo archivo, a un nivel y por nombre; ya no «por archivo». **C-8 desaparece** con la regla en Application. **C-9 es nueva:** las lecturas `FromPilot(` y `PrepareKit.Reread(` fuera del kit. **Medido por opción de Q4** (R-29):<br>• opción 1: 242 de 428;<br>• opción 2: 428;<br>• opción 3: no se mide por texto.<br>`<n4>`: 149 métodos por texto, 99 fuera de las 9 clases | D-PBT4-11; §8.2 |
| **V4-03** | **I-19** | EIG:664-690 y EIG:692-721 cuentan `pilot.Read` por cuerpo y no se pueden evitar sin quedarse con el borrador del piloto. Van a la lista de PB-T4b (C-5) | §4.2 |
| **V4-04** | **I-20** | CT-PBT4C-10 se parte en dos:<br>• Core: puerto → compuerta → WHC → doble del editor → «Actualizar»;<br>• **CT-PBT4C-14 (UI):** la FVM real sobre un doble del puerto que registra la llamada | §5.3 |
| **V4-05** | **I-21** (Q4) | Q4 neutral, con los números. **Las dos opciones cumplen las tres condiciones,** así que se recomienda «Saltar y duplicar», que no exige una excepción a C02 ni se aparta del «como D-1» | §8.2 Q4 |
| V4-06 | Q3 | La lista como **cota superior**: se emite la que vio el Owner o una parte; si el GREEN hace fallar otra prueba, vuelve al Owner | §8.2 Q3 |
| V4-07 | M-24 | El texto desfasado, corregido: D-PBT4-01 (una sustitución), D-PBT4-17 (la costura medida), D-PBT4-19 (sin `x:Static`), §7 («la v4»), la precondición 4 (Q1 a Q4) y la cabecera (`7f0d9aa3`) | Varios |
| V4-08 | M-25 | La condición 7 sustituye la línea base resuelta con `PushBackEditorState.SetWorkingBaseline(PushBackSystem)` (`PushBackEditorState.cs:74-79`), sobre una copia | D-PBT4-18 |
| V4-09 | M-26 | Q2: «del orden de un centenar de campos del editor» | §8.2 Q2 |
| V4-10 | M-27 | OWNER-1 a OWNER-3 de F4B-E48 (espacio papel o bloques del usuario, purga de un nivel, bloques dinámicos), en el Smoke-PB y en §7. La medición de la duración de la suite de Core (límite de 20 min, `ci.yml:42`) | §5.5; §7 |

## Registro de cambios v2 → v3

| Id | Hallazgo | Qué cambia | Dónde |
|---|---|---|---|
| **V3-01** | **B-05** («Editar todo» no llega al anfitrión) | Hay **dos reenvíos** entre el panel y el anfitrión: `PilotEditAdapter` (WCo:283-327) y `WorkspacePilotEditGate` (`WorkspacePilotSupport.cs:153-`). Con `TryEditAll` con cuerpo por defecto, los dos lo devolverían `NotAvailable` sin reenviar.<br>En PB-T4c **los dos reenvían `TryEditAll`**: los dos entran en el alcance y en la lista cerrada. Además:<br>• CT-PBT4C-12 (Core) prueba la compuerta con el anfitrión real, sin AutoCAD;<br>• CT-PBT4C-13 (guarda de texto) exige el reenvío del adaptador del Plugin, con las 18 operaciones de `ControllerViolations` (PEG:683-695), y falla en el RED;<br>• CT-PBT4C-10 recorre la cadena de punta a punta (FVM → puerto → compuerta → anfitrión → doble del editor) | D-PBT4-13 y 16; §5.3; §6.3 |
| **V3-02** | **B-04 e I-18** (la lista) | **La causa C-0,** «la lectura viene del kit del piloto», con su firma mecánica y los archivos de kit por nombre. **El procedimiento va al Owner como Q4,** con tres opciones; la recomendada es «Editar una línea del kit compartido». **C-5 se mide sobre el texto literal del GREEN** que el contrato entrega. **Medido:** 9 clases y 428 métodos (R-21) | D-PBT4-11; §8.2 Q4; §11 |
| **V3-03** | **I-12** (PB-T4a rompe más) | La elección del origen va **en línea** en cada cuerpo, sin ayudante. Siguen los seis `_editIntentReader!.ReadIntent(`, un `_editIntentReader` seguido de `.` o `!` por cuerpo, y el orden de EIG:563-575, EIG:758-770 y `WorkspaceLifecycleGuardTests:642-656`. **Solo rompe CT-PBT3A-12** | D-PBT4-03; §4.1; §5.1 |
| **V3-04** | **I-13** (la clave tras POST) | **No hay clave que guardar.** La relectura usa el **texto** del origen del borrador, `draft.CurrentDraftState.Origin`. El Plugin elige el único miembro mutable cuyo texto es, byte a byte, ese texto, que es la regla de `ChooseOrigin` de UR:640-650. Sin coincidencia, o con varias, cae en el `Primary` de hoy.<br>Tras un POST que borra el origen, el borrador queda limpio con el origen nuevo (el comparador completo compara `Origin`). No se toca ningún tipo de Application salvo la interfaz nueva, y se quitan el diccionario y sus ayudantes (M-16 y M-18) | D-PBT4-03; CT-PBT4A-03 y CT-PBT4B-17 |
| **V3-05** | **I-14** (roturas por letra) | • **Evitadas por diseño:**<br>  – EIG:670 y :720: la clasificación va en una función pura de `Draft/`, y WHC no nombra la base;<br>  – la paridad del XAML: el XAML no cambia (sin `x:Static`), y CT-PBT4B-04 compara la regla con las propiedades de la caja real;<br>  – EIG:1011-1022 y `WorkspaceNavigationHostTests:714-723`: el abridor es una clase anidada de WCo, sin archivo nuevo en `Plugin/Workspace/`.<br>• **En la tabla:** `WorkspacePilotPrepareTests:1351-1353` (11 estados de `EditIntentStatus`, con `Kept` el último), por `Presented` (C-5, PB-T4b) | D-PBT4-06, 08 y 15; §4.2 |
| **V3-06** | **I-15** | La fila de `RequestUpdate`: `Busy` por la marca (WHC:1388), antes de `PassGate` | D-PBT4-14 |
| **V3-07** | **I-16** | La condición 7 de D2, citada literal, con una línea que discrimina: un rack resuelto de **contenido distinto** en `Current` deja `IsDirty` en `False` | D-PBT4-18; CT-PBT4B-10 |
| **V3-08** | **I-17** (Q2) | Q2 con sus costes medidos: unas 136 a 165 rutas de hoja por diseño, las hojas derivadas y los 320 px. La recomendada pasa a ser **«Por grupos»**. Se dice que la letra de D-PB-05 dice «fields» | §8.2 Q2 |
| V3-09 | M-16 | Desaparece: no hay clave que convertir, porque se pasa el texto | D-PBT4-03 |
| V3-10 | M-17 | El censo de PB-T4a admite solo RIA (más `Draft/`, PP y UR). En PB-T4b, por letra, el de WHC (C-5) | CT-PBT4A-06; §4.2 |
| V3-11 | M-18 | Desaparece: sin diccionario no hay claves que borrar | D-PBT4-03 |
| V3-12 | M-19 | CT-PBT4B-11 con una secuencia fija: el comparador da `False` entre un estado con una longitud de módulo `NaN` y su copia | §5.2 |
| V3-13 | M-20 | Se declara el límite: el testigo cubre solo el sobre del origen | D-PBT4-08 |
| V3-14 | M-21 | Q1: «retira lo que se puede retirar»; la opción (C), sin número, porque no se puede medir sin diseñarla | §8.2 Q1 |
| V3-15 | M-22 | Se cierran con su cita los `<a medir>` que la §13 da por medidos. Quedan solo K, D y E, las líneas de CT-PBT4B-06 y el coste de `IsDirty` | §9 |
| V3-16 | M-23 | PB-T4a se declara **con un cambio observable:** las relecturas pasan del `Primary` al origen del borrador | D-PBT4-01 y 03; §6.1 |

## Registro de cambios v1 → v2

| Id | Hallazgo | Qué cambia | Dónde |
|---|---|---|---|
| V2-01 | **B-01** (el anfitrión no recibe la lectura completa) | **PB-T4a nueva, solo aditiva:**<br>• el adaptador del Plugin (RIA) lee también el modelo completo sobre el mismo escaneo;<br>• `EditIntentReadResult` lleva la lectura completa y el catálogo junto a la del piloto (`FromReads`); `FromPilot` no cambia;<br>• la relectura de un borrador abierto admite el origen.<br>Los archivos del Plugin entran en el alcance; el censo de PB-T3 se amplía; `EditIntentKit` no se toca y hay un kit nuevo | D-PBT4-02 y 03; §6.1 |
| V2-02 | **B-02** (la regla en el Plugin, sin prueba posible) | **La regla única vive en la UI** (`AnnotationScaleRule`, en `src/RackCad.UI/Controls/`), sobre `NumericFieldValidation`. La caja `AnnotationScaleBox` toma de ella sus parámetros (`x:Static`), y la prueba la llama desde la UI. El Plugin solo la conecta al anfitrión (`Func<string, DraftFieldValidation>`, un tipo de `Workspace.Drafts`). La copia del adaptador del piloto se trata en Q1 | D-PBT4-06 |
| V2-03 | **B-03, I-03 e I-04** (la lista) | **El procedimiento de la lista,** con causas cerradas y firmas, y cuatro opciones que la sesión elige. Las preguntas se hacen después de medir | D-PBT4-11; §8.2 |
| V2-04 | **I-01** (K = 1) | **Un solo mecanismo,** con los estados por operación:<br>• el abridor del Plugin pone el modal (`EnterPanelModal()`, el único conductor admitido de la bandera);<br>• `TryEditAll` mantiene la marca de lectura durante `ShowModal`: las operaciones del campo y `DiscardRack` dan `Busy`, y las de intención, `Waiting`, sin comprobaciones nuevas | D-PBT4-15 |
| V2-05 | **I-02** (los miembros nuevos del puerto) | **Sin `PilotBoundaryKind.EditAll`:** el gesto usa `CommitField` como frontera, y URT:1107-1109 sigue en verde.<br>**«Conservar» sin miembro nuevo:** usa `TryKeepMine` en dos tiempos, con el testigo.<br>**El único miembro nuevo es `TryEditAll`** (PB-T4c), que rompe PEG:156-163: va a la lista.<br>Medidas y citadas PEG:156-163, :280-281 y :380-385 | D-PBT4-08, 13 y 14; §4 |
| V2-06 | **I-05** (Q1) | Q1 con huecos, el coste de conservar el piloto (A-5, R-T1-17, la premisa de D-1 y la regla duplicada) y una opción de retirada realista: se retira lo que solo usaba el panel y se conserva el lector | §8.2 |
| V2-07 | **I-06** (D2) | CT-PBT4B-10 con las filas de `DiscardAndReload`, `SetPendingText` y `CommitField`, y la condición 7 (sustituir una de las dos referencias en `Current` no cambia `IsDirty`); la huella, tras cada operación | D-PBT4-18; §5.3 |
| V2-08 | **I-07** (`NaN`) | CT-PBT4B-11 pone el `NaN` en una longitud de módulo del commit (A-9, `RackModuleCommit.SameAs`) | §5.3 |
| V2-09 | **I-08** (los literales) | La presentación, parametrizada por Q2, con la unión, el plural y las vistas rechazadas fijados. Las líneas de CT-PBT4B-06 se miden antes de emitir con el clasificador real | D-PBT4-09 |
| V2-10 | **I-09** (los ayudantes y la disciplina de lectura) | La lista nominal cerrada por contrato. Las reglas de lectura de `TryEditAll`, copiadas de `TryKeepPilotEdit`. El equivalente de PEG amplía sus listas | §6.0; D-PBT4-14 |
| V2-11 | **I-10** (el Smoke-PB) | §5.5 y §10 ampliados con todo lo que solo existe en el Plugin | §5.5; §10 |
| V2-12 | **I-11** (Q2) | Q2 con «Por grupos», «Por campos» y «Por campos, con sus valores», sin «Solo recuentos» ni «no abre diálogos». Recomendada: «Por campos» | §8.2 |
| V2-13 | M-01 | 17 operaciones (PEG:44-65) | §2; R-2 |
| V2-14 | M-02 | G-4 exige que el conjunto de archivos que nombran `PushBackPilotUpdateRun.Execute(` sea solo WHC (IG:497-498) | §4 |
| V2-15 | M-03 | `NotePilotEdit(SessionId)` | D-PBT4-13 |
| V2-16 | M-04 | Las relecturas de A-T3-8 son las de un borrador abierto (WHC:409, :503, :590, :675 y :759), no `Begin` (:328); `Precheck` no lee; POST ya relee por `Origin` | D-PBT4-03 |
| V2-17 | M-05 | Los cinco archivos del camino, nombrados en CT-PBT4C-07 | D-PBT4-16 |
| V2-18 | M-06 | Las cuentas de cada rama, rehechas | §5.4 |
| V2-19 | M-07 | `PushBackDraftSessionPort` y `LaunchFor`: se conservan **con registro** en DISP. A-T3-9: registro en DISP y en `ideas-futuras.md` | D-PBT4-13; D-PBT4-20 |
| V2-20 | M-08 | [PKC2] y F5-A, definidos | Abreviaturas |
| V2-21 | M-09 | El abridor muestra la ventana con `ShowModalWindow`, **sin dueño WPF**, como el clásico (`PushBackClassicCharacterizationPluginGuardTests.cs:196-209`) | D-PBT4-16 |
| V2-22 | M-10 | Las citas corregidas: la forma del resultado (EIH:1175-1194) y el cableado (EIG:1150-1190) | §4 |
| V2-23 | M-11 | La guarda de «sin traducción» mira WHC, WEI, `WorkspacePilotSupport.cs`, RIA y NH | CT-PBT4B-12 |
| V2-24 | M-12 | CT-PBT4C-09 lee `PendingWork()` sin cerrar la ventana; la costura de escenificar se mide; la petición de «Editar todo» y los estados Unknown, Error y Undecidable, fijados | D-PBT4-14 y 17 |
| V2-25 | M-13 | Una medición del coste de `IsDirty` en la corrida de medición (precondición 6) | §7 |
| V2-26 | M-14 | `UNIT_CHANGE` completa. `IPushBackFullEditorHost` se declara en `Application/Workspace` por EIH:1088 (un solo campo de tipo de `Systems`), frente a R-PB-05 y ADR-0029 D12 | §6.0; D-PBT4-15 |
| V2-27 | M-15 | CT-PBT4C-10 lleva una línea tras un gesto compuesto (F4B-E30) | §5.4 |
| V2-28 | §8 de la revisión, lo declarado | Que pedir «Conservar» lee el dibujo, frente a la regla de F4 de la UI :780-793, con su registro. Y el procedimiento de la lista | D-PBT4-08; D-PBT4-11 |
| V2-29 | La sesión | **Tres subtareas** (PB-T4a, PB-T4b y PB-T4c) y sus TaskId | D-PBT4-01 |

---



## 0. En una página

**Lo que pide el Owner** ([OD], fila de PB-T4): «Add "Editar todo" … Do not make PB-T4 invent state translation logic. It should consume the already proven full Push Back draft/state API.»

**La base:** el cierre de PB-T3c, `7f0d9aa3`. **Cuatro subtareas secuenciales,** cada una con su 1/3. Los `TaskId` empiezan por `F4B-PBT4-` y tienen **como mucho 26 caracteres** (MAX_PATH solo acota por arriba; V41-11):

| Subtarea | TaskId | Qué | Sustituciones exactas (§8.3) | La emisión |
|---|---|---|---|---|
| **PB-T4a** | `F4B-PBT4-FULL-READ-PATH` (23) | El Plugin lee también el modelo completo sobre el mismo escaneo; el resultado lleva lo completo y el catálogo. Las relecturas de un borrador abierto van al origen del borrador por su texto, con una relectura pura en `Draft/` sobre el escaneo ya tomado (A-T3-8): es un cambio observable declarado. La firma de `RackPilotIntentAdapter.Read` no cambia | **1** (CT-PBT3A-12) | Condicional (pregunta 4) |
| **PB-T4b** | `F4B-PBT4-FULL-DRAFT-PANEL` (25) | El borrador del panel pasa al completo; OB-03.2; la regla única de la escala, en Application; «Actualizar» por `ExecuteFull`; «Conservar mi borrador» con la presentación «Por grupos» y `KeepOn`. **El código del piloto queda sin uso, sin retirarlo** | **470** (428 de las 9 clases, «Saltar y duplicar»; 15 de la letra; 27 de la UI), la lista que el Owner tomó como «la que vio» | Condicional, con esa lista |
| **PB-T4c** | `F4B-PBT4-EDIT-ALL-WINDOW` (24) | «Editar todo», con el abridor en dos piezas | **2** (PEG:155-163 y CT-PBT1B-15) | Condicional |
| **PB-T4d** | `F4B-PBT4-RETIRE-PILOT` (21, propuesto) | Retira el código del piloto que el panel ya no usa | **139** sustituciones, **55** originales y **29** miembros de apoyo borrados con registro (esbozo, §6.4) | **Vuelve al Owner con su lista** |

**Las decisiones del Owner,** tomadas (§8.2): Q1 «Retirarlo», Q2 «Por grupos», Q3 «Saltar y duplicar» y Q4 «Sí, con condiciones» (`owner-pbt4-decisions.md`); y, después, «Retirar tras PB-T4c» y «Sí, esa lista» (`owner-pbt4b-retire-list-decisions.md`).

**PB-T4a es la primera que se emite.** Su parte del paquete está completa y sin remisiones: el diseño (D-PBT4-02 y 03, con el texto literal del GREEN y `ReadAt`, B-08), las roturas medidas con el recorrido entero de sus guardas (§4.1), la lista cerrada CT-PBT4A-01 a 08 con sus totales (§5.1), lo común (§6.0) y el contrato propuesto con lo que necesita su generador (§6.1).

## 1. Autoridades

Las de la v1 (§1), sin cambio. Además, la revisión de la v1 y las instrucciones de la sesión. Las principales:

| Autoridad | Lo que manda aquí |
|---|---|
| **[OD]**, la fila de PB-T4, D-PB-01 y D-PB-05 | Un editor completo, modal, con borrador dentro y fuera y cero escrituras en el DWG; los pasos 1 a 7; «an informed destructive choice»; sin traducción de estado |
| **[P]**: OB-01.1 a 6, OB-03.2, OB-05.1 a 5, R-PB-10, §7.3 (OB-T1B.1 a 3) y §7.4 | |
| **[P2] §10** (A2-1 a A2-13) y **[P3] §10** (A-T3-1 a A-T3-11) | |
| **[PT1]**: A-5 («La copia de la carga del piloto … PB-T4 la retira al consumir el API», :1549), A-9 (:1553), A-10 (:1554) y R-T1-17 (:1367) | |
| **F4B-E28 Q3; F4B-E38; F4B-E29 (D-1); F4B-E11 (D2) con L-5; F4B-E24; la nota del Owner sobre la interfaz; F4B-E03; E15, E21, E25, E35 Q4, E37, E42, E45, E46 y E48; D-PB-10** | |
| **Las decisiones del Owner sobre PB-T4** (`owner-pbt4-decisions.md`, 2026-11-20) | Q1 «Retirarlo», Q2 «Por grupos», Q3 «Saltar y duplicar» y Q4 «Sí, con condiciones» (§8.2). Desde la v4.1, Q1 a Q4 siguen la numeración de ese registro (la v4 llamaba Q3 a la emisión y Q4 al procedimiento) |
| **Las decisiones del Owner sobre la retirada y la lista de PB-T4b** (`owner-pbt4b-retire-list-decisions.md`, 2026-11-20) | «Retirar tras PB-T4c» y «Sí, esa lista» (§8.2) |

---

## 2. Hechos medidos en `7f0d9aa3`

Medidos en `7dfc7c21` (v1 a v3) y vueltos a medir en `7f0d9aa3`, el cierre de PB-T3c (v4 y v4.1): entre las dos, `src/` y `tests/` solo cambian en el escritor, la fachada y dos guardas de PB-T3c (R-28), así que valen en `7f0d9aa3`. Los de la v1 (R-1 a R-12), corregidos, y los nuevos (R-13 a R-38; los de la v4.1, R-34 a R-38). Lo esencial:

| Hecho | Fuente |
|---|---|
| **El resultado de la lectura solo lleva el piloto:** `EditIntentReadResult(status, reason, PushBackPilotIntentResult? pilotRead)`, con las fábricas `FromPilot`, `NoActiveDocument`, `Busy`, `Rejected` y `Failed` | WEI:35-64 |
| **El Plugin lee:** NH `ReadIntent` → `ReadOnActiveDocument` → `RackPilotIntentAdapter.LoadCatalog()` fuera del bloqueo → `LockDocument` → `ChooseOriginDefinition(revalidation.Fresh, chosenReference)` → `RackPilotIntentAdapter.Read(document, rackId, origin, witnessKey, witnessText, catalog)` → `EditIntentReadResult.FromPilot(read.Pilot)` | NH:207-251; WEI:682-692 |
| **El escaneo** es `new PushBackPilotIntentScan(rackId, originKey, scan.Facts, scan.RawEnvelopes, CompletenessEvidence, witness)`. `PushBackFullDraftReader.Read` recibe ese mismo tipo y llama primero al lector del piloto | RIA:116-121; `Draft/PushBackFullDraftReader.cs:51-59` |
| **`PushBackFullIntentResult` no lleva el testigo;** el resultado del piloto sí (`PreviewWitness`) | `Pilot/PushBackPilotIntentResult.cs:25-59`; `Draft/PushBackFullDraftReader.cs:12-41` |
| **Las seis lecturas del anfitrión:** `ReadIntent(request.Session, request.RackId, request.ChosenReference, request.Witness)` en WHC:328 (`Begin`), :409, :503, :590, :675 y :759; seis `var pilot = read.PilotRead;` | R-13 |
| **EIG:733-736:** ningún `.PilotRead` fuera de esas asignaciones; exactamente 6 asignaciones y 6 `.ReadIntent(` | EIG:725-737 |
| **EIG:414-430:** NH llama una vez a `RackPilotIntentAdapter.LoadCatalog()` y una vez a `RackPilotIntentAdapter.Read(` | R-14 |
| **EIH:1088:** WHC tiene exactamente un campo cuyo tipo, o algún argumento genérico, es de `RackCad.Application.Systems` (`_updateHost`) | EIH:1075-1099 |
| **EIH:52:** el kit de lecturas de todo el anfitrión construye con `FromPilot`; 12 usos de `FromPilot` en las pruebas | R-13 |
| **Las 17 operaciones del puerto** (PEG:44-65), fijadas por reflexión (PEG:159-161); las seis de lectura (:68); `WorkspaceHostCore : IWorkspaceLifecycleAdvisor, IWorkspacePilotEditPort` y nada más (:380-385); el adaptador del piloto es el único campo estático del anfitrión (:280) | R-15 |
| **URT:1107-1109** fija los 7 nombres de `PilotBoundaryKind` | R-15 |
| **K = 1:**<br>• `SetPendingText`, `CommitField`, `CrossBoundary`, `DiscardRack` y `DiscardDocument` miran la marca `_pilotReadInProgress` y dan `Busy`; no miran el modal;<br>• `PassGate` y `UpdateGate` miran el modal y dan `Waiting`;<br>• `PilotFieldStatus` = { Rejected, Applied, NotEditable, Busy, Invalid } | WHC:1127, :1155, :1208, :1243, :1270, :1940, :996; `IWorkspacePilotEditPort.cs:143-157` |
| **La regla de la UI de F4:** «Asking for an option calls no operation of the port and only reads its memory» | `tests/RackCad.UI.Tests/Workspace/WorkspacePilotReconciliationTests.cs:780-793` |
| **RG:116** muta `public void RequestKeepMine() => AskChoice(ChoiceKind.KeepMine);` | R-15 |
| **La UI de la reconciliación:** 40 llamadas a `RequestKeepMine` o `ConfirmChoice` en `tests/RackCad.UI.Tests/Workspace/` | R-15 |
| **Las pruebas no referencian el Plugin:** `RackCad.Tests.csproj:23-31`, `RackCad.UI.Tests.csproj:25-27` | R-16 |
| **La caja de la escala:** `<controls:NumericField x:Name="AnnotationScaleBox" … Minimum="0" MinimumInclusive="False" />`; `NumericField` valida con `NumericFieldValidation.Validate` | W.xaml:68-69; `src/RackCad.UI/Controls/NumericField.cs:149` |
| **`IDraftPendingTextAdapter<TState>`:** `DraftFieldValidation Validate(string fieldKey, string text)` y `TState Apply(TState current, IReadOnlyDictionary<string, string> pendingText)`; `DraftFieldValidation(bool IsValid, string? Message)` | `Drafts/DraftSession.cs:95-110` |
| **El modelo completo usa el piloto:** las constantes de `PushBackPilotMaterializer` (en `Draft/` 15 veces) y `PushBackPilotIntentReader.Read` (el lector completo); el lector del piloto usa `PushBackPilotMaterializer.Materialize` | R-17 |
| **`NaN`:** `PalletTolerance` se compara con `double.Equals` (`NaN` es igual a `NaN`); `RackModuleCommit.SameAs` compara longitudes con `!=` | `PushBackEditableStateComparer.cs:409`; `Systems/Shared/RackModuleEditSession.cs:76-` |
| **El abridor clásico** muestra la ventana con `AcApplication.ShowModalWindow(window)` y sin asignar `Owner` | `PushBackClassicCharacterizationPluginGuardTests.cs:196-209` |
| **La suite de UI** corre en un STA compartido con un `MessageBox` modal real | `.github/workflows/ci.yml:213-222` |

---

## 3. El diseño


### D-PBT4-01. Cuatro subtareas secuenciales (V42-01)

**El orden:** PB-T4a sobre el cierre de PB-T3c (`7f0d9aa3`); PB-T4b sobre el de PB-T4a; PB-T4c sobre el de PB-T4b; y PB-T4d, la retirada del piloto, sobre el de PB-T4c, cuando el Owner apruebe su lista. Cada una con su 1/3 (OB-10.1), sin rebase.

**Por qué así:**
- PB-T4a deja el camino de la lectura completa y la relectura con el origen, con **una sola** sustitución por letra;
- PB-T4b concentra el cambio de tipo y su lista;
- PB-T4c es casi aditiva, con dos sustituciones por letra;
- PB-T4d retira el código del piloto cuando ya no corre ninguna prueba que lo use (D-PBT4-10).

**Los TaskId** empiezan por `F4B-PBT4-` y tienen como mucho 26 caracteres (23, 25, 24 y, el propuesto de PB-T4d, 21), para los sustitutos de nc1 y nc2 (§6.0; V41-11).

### PB-T4a

#### D-PBT4-02. La lectura completa, junto a la del piloto (B-01; V41-06; V42-02)

**En Application** (WEI), aditivo. El texto literal del GREEN:

```csharp
        private EditIntentReadResult(EditIntentReadStatus status, string? reason, PushBackPilotIntentResult? pilotRead, PushBackFullIntentResult? fullRead, RackCatalog? catalog)
        {
            Status = status;
            Reason = reason;
            PilotRead = pilotRead;
            FullRead = fullRead;
            Catalog = catalog;
        }

        /// <summary>I-64 F4B PB-T4a: the read of the full model over the same scan; null unless the read came from <see cref="FromReads"/>.</summary>
        public PushBackFullIntentResult? FullRead { get; }

        /// <summary>I-64 F4B PB-T4a: the catalog of the read; null unless the read came from <see cref="FromReads"/>.</summary>
        public RackCatalog? Catalog { get; }

        public static EditIntentReadResult FromPilot(PushBackPilotIntentResult pilotRead)
        {
            if (pilotRead == null) throw new ArgumentNullException(nameof(pilotRead));
            return new EditIntentReadResult(EditIntentReadStatus.Read, null, pilotRead, null, null);
        }

        public static EditIntentReadResult FromReads(PushBackPilotIntentResult pilotRead, PushBackFullIntentResult fullRead, RackCatalog catalog)
        {
            if (pilotRead == null) throw new ArgumentNullException(nameof(pilotRead));
            if (fullRead == null) throw new ArgumentNullException(nameof(fullRead));
            if (catalog == null) throw new ArgumentNullException(nameof(catalog));
            return new EditIntentReadResult(EditIntentReadStatus.Read, null, pilotRead, fullRead, catalog);
        }
```

- `NoActiveDocument`, `Busy`, `Rejected` y `Failed` pasan `null, null` en los dos argumentos nuevos; su texto no cambia en lo demás.
- Los `using` nuevos de WEI: `RackCad.Application.Catalogs` y `RackCad.Application.Systems.PushBack.Draft`.
- **`FromPilot` no cambia de comportamiento:** deja `FullRead` y `Catalog` en null, así que `EditIntentKit` (EIH:51-52) y los 12 usos de `FromPilot` siguen como están.

**En el Plugin (RIA).** **La firma de `RackPilotIntentAdapter.Read` no cambia** (V41-06): EIG:139-141 la fija entera, hasta `RackCatalog catalog)`, así que añadirle un parámetro, obligatorio o con valor por defecto, rompería esa guarda, que no está en la lista que vio el Owner (contrafactual medido en `measure_pbt4_v41-out.txt` y en `measure_pbt4_v42-out.txt`). El texto literal del GREEN:
- el `using` nuevo `RackCad.Application.Systems.PushBack.Draft`, antes del de `Pilot`;
- `RackPilotIntentReadResult` gana un segundo constructor y dos propiedades:

  ```csharp
        internal RackPilotIntentReadResult(PushBackPilotIntentResult pilot, PushBackFullIntentResult full, PushBackPilotIntentScan input)
        {
            Pilot = pilot;
            Full = full;
            Input = input;
            FailureReason = string.Empty;
        }

        internal PushBackFullIntentResult? Full { get; }

        internal PushBackPilotIntentScan? Input { get; }
  ```

  El constructor de hoy, el de las cinco salidas de fallo, no cambia, y esas cinco líneas tampoco;
- en `Read`, la única línea que cambia es la del éxito, que llama una vez al lector completo sobre el **mismo** `input`, justo después del lector del piloto:

  ```csharp
                return new RackPilotIntentReadResult(read, PushBackFullDraftReader.Read(input, catalog), input);
  ```

**Lo que no hace:**
- **Nada traduce:** lo completo sale del lector completo sobre el mismo escaneo (P3:530, A-T3-2), y no del resultado del piloto.
- **Ninguna lectura nueva del dibujo:** el escaneo es el mismo, dentro del mismo bloqueo; no hay segundo `LockDocument` ni segundo `CaptureDrawing`.
- **El anfitrión no usa todavía `FullRead`:** lo usa PB-T4b.

**Lo que necesita `Classify`:**
- los sobres de los miembros salen de `FullRead.StaleBase` (las entradas con su texto);
- el catálogo, de `Catalog`.

**El testigo** se queda en `PilotRead.PreviewWitness`, porque el lector completo lo calcula con el mismo lector del piloto.

**Las guardas del Plugin, medidas sobre el texto literal del GREEN** (§4.1: el recorrido entero de las clases que leen los archivos tocados, `walk_guards_pbt4a.py`, y las aserciones traducidas de `measure_pbt4_v42-out.txt`, 35 de 35 en verde):
- **EIG:350-364 (B-08):** un solo `new PushBackPilotIntentScan(` y un solo `PushBackPilotIntentReader.Read(` en todo RIA, los dos en `Read` y en su orden; por eso las relecturas puras van en `ReadAt`, en `Draft/`;
- EIG:414-430: NH sigue con una `LoadCatalog()` y una `RackPilotIntentAdapter.Read(`;
- G-9 sigue con sus cuentas: RIA conserva `StartTransaction` 1 y `LockDocument` 1, y NH `LockDocument` 2;
- INV-14 (`AssertNoOpenTransaction` antes y después) sigue: `Read` no cambia salvo la línea del éxito;
- **`WorkspaceCommandAdvisorGuardTests`** (CAG:21-22 y :406-464; antes `<a medir>`, M-28): fija cinco motivos en español en el cuerpo de `Read` de RIA, con dos `ex.Message`, y tres en `ReadOnActiveDocument` de NH. **Sigue en verde:** el GREEN no añade ningún literal de cadena ni ningún `ex.Message` a esos dos cuerpos, y `AtOrigin` no tiene ninguno. Es una restricción para el Worker, que CT-PBT4A-07 comprueba (`literales=0`).

#### D-PBT4-03. La relectura con el origen (A-T3-8; I-12, I-13, M-04, M-16, M-18, M-23; V41-06, V41-07; B-08, M-35)

**El problema.** Las cinco relecturas de un borrador abierto (WHC:409, :503, :590, :675 y :759) pasan `request.ChosenReference`, que el panel deja en null. El Plugin elige entonces el `Primary` (WEI:686), no el origen del borrador. `Begin` (:328) sí debe usar `ChosenReference`.

**La fuente del origen: el texto, no una clave (I-13).**
- Cada estado de borrador ya lleva el texto del sobre de su origen: `PushBackPilotDraftState.Origin` (`Pilot/PushBackPilotDraftState.cs:15-32`) y `PushBackFullDraftState.Origin`.
- La regla es la de `ChooseOrigin` de la corrida (UR:640-650): **el único miembro mutable cuyo texto es, byte a byte, el del origen.**
- **Sin coincidencia, o con varias** (por ejemplo, el origen se editó fuera), la lectura se queda como hoy: la del `Primary`. Lo releído sigue siendo el rack actual, y la detección de conflicto de F4 hace su trabajo.
- **Tras un POST `Applied` que borra el origen** (PB-T3b reancla a la primera vista redibujada, PP:322 y :633), el borrador queda en Clean con la relectura de POST como base. El comparador completo compara `Origin` (`PushBackFullDraftStateComparer.cs:19`), así que `CurrentDraftState.Origin` es el texto del origen nuevo, y la relectura siguiente lo encuentra. Lo fija CT-PBT4B-17.

**La función pura, en `src/RackCad.Application/Systems/PushBack/Draft/PushBackOriginByText.cs`** (archivo nuevo; el texto literal del GREEN):

```csharp
using System;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Systems.PushBack.Pilot;
using RackCad.Application.Workspace.Drafts;

namespace RackCad.Application.Systems.PushBack.Draft
{
    /// <summary>
    /// I-64 F4B PB-T4a (D-PBT4-03, A-T3-8) — the origin of an open draft, found by its text: the key of the ONLY mutable member
    /// whose raw text is, byte for byte, the origin of the draft (the rule of the run, UR:640-650), the same scan at that key, and
    /// the two pure reads over it (the pilot and the full model), as PushBackFullDraftReader does.
    /// </summary>
    public static class PushBackOriginByText
    {
        public static string? Choose(DraftStaleBase staleBase, string originText)
        {
            if (staleBase == null || originText == null) return null;
            var matches = staleBase.Entries.Values
                .Where(entry => entry.Membership == DraftMembership.Mutable && string.Equals(entry.EnvelopeText, originText, StringComparison.Ordinal))
                .Select(entry => entry.Key)
                .ToList();
            return matches.Count == 1 ? matches[0] : null;
        }

        public static PushBackPilotIntentScan AtKey(PushBackPilotIntentScan scan, string originKey)
        {
            if (scan == null) throw new ArgumentNullException(nameof(scan));
            return new PushBackPilotIntentScan(
                scan.RackId,
                originKey,
                scan.Facts,
                scan.RawEnvelopes.ToDictionary(pair => pair.Key, pair => (string?)pair.Value, StringComparer.Ordinal),
                scan.CompletenessEvidence,
                scan.PreviewWitness);
        }

        public static (PushBackPilotIntentScan Scan, PushBackPilotIntentResult Pilot, PushBackFullIntentResult Full)? ReadAt(PushBackPilotIntentScan scan, string originText, RackCatalog catalog)
        {
            if (scan == null) throw new ArgumentNullException(nameof(scan));
            var key = Choose(PushBackPilotIntentReader.BuildStaleBase(scan), originText);
            if (key == null || key == scan.OriginKey) return null;
            var input = AtKey(scan, key);
            return (input, PushBackPilotIntentReader.Read(input, catalog), PushBackFullDraftReader.Read(input, catalog));
        }
    }
}
```

**`ReadAt` (B-08):** las dos relecturas puras van en `Draft/`, no en RIA, porque EIG:350-364 exige un solo `PushBackPilotIntentReader.Read(` y un solo `new PushBackPilotIntentScan(` en **todo** RIA (`Adapter()`, :356-357), y EIG:465-470 no admite ningún `PushBackPilot` en NH. Devuelve una tupla con nombre (el escaneo, la lectura del piloto y la completa), o `null` si no hay otro origen; no hay tipo nuevo. No añade dependencias: `PushBackFullDraftReader.cs` ya llama a `PushBackPilotIntentReader.Read` desde `Draft/` (:56). Con los dos lectores, el archivo sigue sin ninguna ficha de G-L1 ni ningún término de la pureza (CT-PBT4A-08), como `PushBackFullDraftReader.cs`.

`AtKey` copia los sobres con el mismo molde que UR (`ToDictionary(… (string?)pair.Value …)`), porque el constructor del escaneo los pide como `IReadOnlyDictionary<string, string?>` y pasar los de la propiedad daría un aviso de nulabilidad nuevo en el Plugin. **Ningún término de la pureza de `Draft/`** (CT-PBT3A-14 y CT-PBT4A-08) aparece en el archivo, tampoco en sus comentarios: ni `Task`, ni `Database`, ni `Transaction`, ni `File.`.

**La relectura en el origen, en RIA, sobre el mismo escaneo** (el texto literal del GREEN; va antes de `ReadPreviewWitness`):

```csharp
        /// <summary>
        /// I-64 F4B PB-T4a (D-PBT4-03): the same read at the origin of an open draft, chosen by its text over the scan that
        /// <see cref="Read"/> already took, by the pure reread of the full model (PushBackOriginByText.ReadAt). It reads nothing
        /// more of the drawing. Without exactly one other mutable member with that text, the read stays as it was. An exception of
        /// the pure reread is not contained here, unlike <see cref="Read"/>: it reaches the reader of the host, which answers Failed.
        /// </summary>
        internal static RackPilotIntentReadResult AtOrigin(RackPilotIntentReadResult read, string originText, RackCatalog catalog)
        {
            if (!read.Succeeded || read.Input == null) return read;
            var at = PushBackOriginByText.ReadAt(read.Input, originText, catalog);
            return at == null ? read : new RackPilotIntentReadResult(at.Value.Pilot, at.Value.Full, at.Value.Scan);
        }
```

- **`AtOrigin` solo llama a `ReadAt`** (B-08): RIA se queda con un solo `PushBackPilotIntentReader.Read(`, un solo `PushBackFullDraftReader.Read(` y un solo `new PushBackPilotIntentScan(`, los tres en `Read` (EIG:350-364, medido).
- **No lee el dibujo:** sin `CaptureDrawing`, sin bloqueo, sin transacción, sin literal de cadena y sin `ex.Message` (CT-PBT4A-07). **Una excepción de la relectura pura no se contiene en RIA** (M-35; el `<summary>` lo dice): sube a `ReadIntentAtOrigin` de NH, que la contiene como `Failed(ex.Message)`, igual que `ReadIntent`.
- **El coste:** en el caso del origen, las dos lecturas puras se repiten una vez sobre el escaneo ya tomado; no hay acceso nuevo al dibujo. Va con el coste del Smoke-PB (§5.5).
- RIA no nombra `DraftStaleBase`, `BuildStaleBase` ni `RackCad.Application.Workspace` (EIG:384-388): la base se calcula dentro de `ReadAt`.

**La interfaz nueva,** en `src/RackCad.Application/Workspace/IWorkspaceEditIntentOriginReader.cs` (archivo nuevo, completo ya en el esqueleto):

```csharp
#nullable enable
namespace RackCad.Application.Workspace
{
    /// <summary>
    /// I-64 F4B PB-T4a (D-PBT4-03, A-T3-8) — the reader of the edit intention that can read again at the origin of an open draft,
    /// found by its text. It is optional: a reader without it is read as today.
    /// </summary>
    public interface IWorkspaceEditIntentOriginReader
    {
        EditIntentReadResult ReadIntentAtOrigin(SessionId session, string rackId, string originText, EditIntentWitness? witness);
    }
}
```

**El lector anidado de NH** (`PilotIntentReader`, que sigue siendo la única otra clase del archivo, EIG:443). NH no tiene `#nullable`, así que el parámetro nuevo va sin `?` (con `?` daría el aviso CS8632). El texto literal del GREEN, edición a edición:
1. `private sealed class PilotIntentReader : IWorkspaceEditIntentReader, IWorkspaceEditIntentOriginReader`
2. en `ReadIntent`: `return ReadOnActiveDocument(session, rackId, chosenReference, witness, null);`
3. un método nuevo, justo antes de `ReadOnActiveDocument`:

   ```csharp
            public EditIntentReadResult ReadIntentAtOrigin(SessionId session, string rackId, string originText, EditIntentWitness witness)
            {
                try
                {
                    return ReadOnActiveDocument(session, rackId, null, witness, originText);
                }
                catch (System.Exception ex)
                {
                    LogFault("WorkspaceNavigationHost.ReadEditIntent", ex);
                    return EditIntentReadResult.Failed(ex.Message);
                }
            }
   ```
4. `private EditIntentReadResult ReadOnActiveDocument(SessionId session, string rackId, long? chosenReference, EditIntentWitness witness, string originText)`
5. tras la sentencia de `RackPilotIntentAdapter.Read(…)`, dentro del bloqueo:

   ```csharp
                    if (originText != null)
                        read = RackPilotIntentAdapter.AtOrigin(read, originText, catalog.Catalog);
   ```
6. `? EditIntentReadResult.FromReads(read.Pilot, read.Full, catalog.Catalog)` en lugar de `? EditIntentReadResult.FromPilot(read.Pilot)`.

Siguen una sola `LoadCatalog()`, una sola `RackPilotIntentAdapter.Read(` y un solo `_targets.Revalidate(` en NH (EIG:414-431), un `read.Pilot` y ningún `PushBackPilot` (EIG:465-467), y la forma exacta de cada `_authorize(` del lector (RG:696-712).

**En línea en cada cuerpo de WHC, sin ayudante (I-12).** En las cinco relecturas, la sentencia de la lectura pasa a este texto literal (el cuerpo ya tiene `draft` de `_drafts.Find<PushBackPilotDraftState>(request.Session, request.RackId)` antes de leer, medido):

```csharp
                var read = _editIntentReader is IWorkspaceEditIntentOriginReader originReader
                    ? originReader.ReadIntentAtOrigin(request.Session, request.RackId, draft.CurrentDraftState.Origin, request.Witness)
                    : _editIntentReader!.ReadIntent(request.Session, request.RackId, request.ChosenReference, request.Witness);
```

Lo que mantiene en verde (medido sobre este texto, §4.1):
- **los recuentos:** siguen los seis `.ReadIntent(` (EIG:736), un `_editIntentReader` seguido de `.` o `!` por cuerpo (EIG:573-574, :767, WLG:656) y el orden (EIG:568-569, :761-763, WLG:646-654). `ReadIntentAtOrigin(` no casa con `\.ReadIntent\s*\(`, y `_editIntentReader is` no casa con `_editIntentReader\s*[.!]`;
- **la base:** `draft.CurrentDraftState.Origin` no es una lectura de la base (`BaseOrReadUse`, EIG:648), y WHC ya nombra `draft.CurrentDraftState` (WHC:1772);
- **la reflexión sobre WHC** (EIH:993-1104, WLG:357-358, PEG:383-385, WPUH:503, IG:427-428): la variable de patrón `originReader` no es un miembro; WHC no declara nada nuevo;
- **los dobles existentes del lector** (`EditIntentFakeReader`, `PilotEditAuditedReader`) no implementan la interfaz nueva: siguen por la rama de hoy.

**El cambio observable (M-23).** En AutoCAD, las relecturas pasan del `Primary` al origen del borrador. Es lo que pide A-T3-8, y se declara en el `Objective` de PB-T4a y en el Smoke-PB.

### PB-T4b

#### D-PBT4-04. El borrador del panel es el completo (A-T3-1)

**El anfitrión:**
- **`Begin`:** `_drafts.Begin(session, rackId, read.FullRead.Read, PushBackFullDraftStateComparer.Instance, PushBackFullDraftStateComparer.Copy)`.
- **La admisión y su diagnóstico** salen de `FullRead`, y el testigo de `PilotRead.PreviewWitness`.
- **Sin `FullRead`** (un lector que devuelve `FromPilot`): «No editable aquí: la lectura no trae el modelo completo.» (`FullReadMissingReason`), y no nace ningún borrador.
- **Los nueve `Find<PushBackPilotDraftState>`** pasan a `Find<PushBackFullDraftState>`.
- **Las seis asignaciones** pasan a `var full = read.FullRead;`. El testigo se lee con `read.PilotRead.PreviewWitness` en las tres que lo usan (WHC:344, :519 y :691), y en ningún otro sitio.

**Nada traduce:**
- un borrador del piloto no existe tras el GREEN, porque el tipo se fija en `Begin`;
- el registro se vacía al cerrar el documento, como hoy.

**El código del piloto queda sin uso, y lo retira PB-T4d, tras PB-T4c** (D-PBT4-10; V42-01).

#### D-PBT4-05. OB-03.2

Explícito y con su prueba:
- un rack A/B del corpus (`pb-20-ab` y los demás compuestos) abre su borrador en el panel;
- `pb-21-cola` sigue rechazado con `OutOfPilotDiagnostic` y su `ClassicEditorHint`;
- AUTH-13 sigue: el lector completo pasa primero por el del piloto.



#### D-PBT4-06. La regla única de la escala, en Application (B-02; B-07; Q-PBT2-3)

**Una sola implementación, movida y no copiada:**
- **La lógica pura** de `NumericFieldValidation.Validate` (`src/RackCad.UI/Controls/NumericFieldValidation.cs:61-`; solo usa `System.Globalization` y `RackCad.Application.Formatting.LocalizedNumberParser`) pasa a `src/RackCad.Application/Formatting/NumericTextRule.cs`, al lado del analizador que ya usa:
  - `public static NumericTextCheck Check(string text, bool isOptional, bool integerOnly, double? minimum, double? maximum, bool minimumInclusive, bool maximumInclusive)`;
  - los dos mensajes como constantes, «Escribe un valor.» y «Valor no numérico.», y los de rango, tal cual.
- **`NumericFieldValidation` de la UI** conserva su API pública (`Validate`, `EmptyRequiredMessage`, `NotANumberMessage`, `NumericFieldValidationResult` y `NumericFieldStatus`) y **delega** en `NumericTextRule.Check`. Sus constantes pasan a ser `= NumericTextRule.EmptyRequiredMessage` y `= NumericTextRule.NotANumberMessage`.
- **`PushBackAnnotationScaleRule`,** en `src/RackCad.Application/Systems/PushBack/Draft/`:
  - `Minimum = 0.0` y `MinimumInclusive = false`;
  - `public static DraftFieldValidation Validate(string text)`, que es `NumericTextRule.Check(text, false, false, Minimum, null, MinimumInclusive, true)` traducido a `DraftFieldValidation`.

**Sin cableado:** WHC crea el adaptador del campo con esa regla por defecto. No hay campo nuevo en WHC ni asignación en WCo, y WCo no nombra `RackCad.UI` (`WorkspaceHostSourceGuardTests.cs:398-420`). **Desaparece la causa C-8:** Core tiene la regla.

**Las guardas, medidas (R-30):**
- ninguna prueba lee el texto de `NumericFieldValidation.cs` ni de `NumericField.cs`;
- las 27 que nombran `NumericField` o `NumericFieldValidation` los llaman por su API pública, que no cambia;
- `NumericFieldValidationTests` (UI) prueba la regla directamente: sigue en verde porque el comportamiento es el mismo, y es la prueba de que la delegación no cambia nada;
- **ninguna guarda de T1B** fija el texto de esos archivos: `PushBackClassicCharacterizationSourceGuardTests` lee `src/RackCad.UI/Systems/PushBack` (:171), no `Controls/`.

**El XAML no cambia:** `WorkspaceDraftPilotValidationParityTests.cs:80-97` sigue en verde. CT-PBT4B-04 compara `PushBackAnnotationScaleRule` (`Minimum` y `MinimumInclusive`, y la validación en los diez textos de CT-PBT2B-10) con las propiedades de `AnnotationScaleBox` en una ventana real.

**El adaptador nuevo,** `Draft/PushBackFullFieldAdapter`, llama a `PushBackAnnotationScaleRule.Validate`.

**La copia del piloto** de la regla (`PushBackPilotFieldAdapter`) se retira con él, en PB-T4d (D-PBT4-10); hasta entonces queda sin uso, y la única regla que usa el panel es `PushBackAnnotationScaleRule`.

**El esqueleto del RED de PB-T4b no toca `NumericFieldValidation.cs`** (M-29): crea `NumericTextRule.cs` y `PushBackAnnotationScaleRule.cs` inertes, y la delegación de `NumericFieldValidation.Validate` en `NumericTextRule.Check` es del GREEN.

#### D-PBT4-07. «Actualizar» con la escritura completa

- `TryUpdatePilot` conserva su nombre y su firma (G-10 y los llamadores de EIG).
- Su cuerpo pasa a `PushBackPilotUpdateRun.Precheck(draft)` y `.ExecuteFull(draft, request, port)`.
- **Desde el cierre de PB-T3c** (`7f0d9aa3`), el escritor borra vistas: `RackPilotMutationScope` implementa `IPushBackPilotViewEraser` con `RackViewBlockEraser`.
- **G-4** (IG:497-498) espera que WHC sea el único archivo que nombra `PushBackPilotUpdateRun.Execute(`. Tras el cambio, ninguno lo nombra: va a la lista por letra (§4.2).


#### D-PBT4-08. «Conservar mi borrador» en dos tiempos (OB-05.3 a 5; A-T3-3; I-02, I-14, M-20)

**Por `TryKeepMine`, sin miembro nuevo del puerto.** La petición sin testigo presenta, y con testigo conserva:

1. **«Conservar mi borrador»** (`RequestKeepMine`) llama a `_port.TryKeepMine(new EditIntentRequest(session, rack, null, null))`. El anfitrión:
   - **paso 1:** relee con el origen (D-PBT4-03), con la detección de conflicto de F4;
   - **paso 2:** clasifica con una **función pura de `Draft/`**:
     ```csharp
     PushBackKeepMinePresentation.Read(DraftSession<PushBackFullDraftState> draft, PushBackFullIntentResult read, RackCatalog catalog)
     ```
     Dentro, llama a `PushBackConflictClassifier.Classify(draft.InitialDraftState, read.Read.State, draft.CurrentDraftState, los sobres de read.StaleBase, catalog)`. **WHC no nombra `.InitialDraftState`, `.StaleBase` ni `.Entries`,** así que EIG:670 y :720 siguen en verde;
   - devuelve `EditIntentOutcome.Presented(lines, testigo)`: el estado nuevo **`Presented`**, al final del enumerado, con el testigo `EditIntentWitness(clave del origen, texto del sobre releído)`.
   - **No cambia el borrador.**
2. **La UI** muestra las líneas encima del Sí/No, en un `TextBlock` nuevo, `BrowserFieldKeepMinePresentationText`.
3. **«Sí»** llama a `TryKeepMine` con el testigo. El lector lo compara con lo que hay:
   - **`Changed`:** no conserva, y devuelve `Presented` con lo nuevo;
   - **`Unchanged`:** `KeepMineOnCurrentVersion(full.Read, PushBackFullComposition.KeepOn, confirmed: true)`.
4. **«No»** no llama al puerto.

**Lo que rompe por letra (en la tabla de §4.2):**
- `WorkspacePilotPrepareTests:1351-1353` fija 11 estados, con `Kept` el último. `Presented` lo rompe (C-5, PB-T4b).
- **Se consideró evitarlo** presentando desde memoria, con la clasificación guardada en la última comprobación. Se descartó: en Dirty con `IsPossiblyStale` no hay comprobación previa, y la presentación necesitaría igualmente una lectura en dos tiempos, con un estado de «revisa antes».

**Lo declarado:**
- pedir «Conservar» pasa a leer el dibujo, frente a la regla de F4 de la UI :780-793. Lleva su registro (V2-28);
- **M-20:** el testigo cubre solo el sobre del origen. Si cambia otro miembro entre la presentación y el «Sí», la presentación queda vieja en ese miembro. Aun así, lo que se conserva parte de la relectura del «Sí», y POST vuelve a comprobar. Se declara como límite.

#### D-PBT4-09. La presentación «Por grupos» (Q2 del Owner; I-08; V41-05)

**Lo decidido:** «Por grupos (Recomendado)» (`owner-pbt4-decisions.md`, pregunta 2). Las formas «Por campos» y «Por campos con valores» no se hacen: no hay tabla de nombres de campos (`PushBackFieldLabels`) ni forma como parámetro (`PushBackPresentationForm`).

`PushBackKeepMinePresentation.Of(PushBackConflictClassification c)` es pura, en `Draft/PushBackKeepMinePresentation.cs`. Devuelve `Lines` (`IReadOnlyList<string>`) e `IsEmpty`. `PushBackKeepMinePresentation.Read(DraftSession<PushBackFullDraftState> draft, PushBackFullIntentResult read, RackCatalog catalog)` clasifica (D-PBT4-08) y llama a `Of`.

**Los nueve nombres fijos de grupo,** en el orden de `PushBackChangeGroup` (`PushBackConflictClassifier.cs:14`), literales:

| `PushBackChangeGroup` | Literal |
|---|---|
| `Name` (0) | `el nombre` |
| `Rack` (1) | `el rack` |
| `Annotations` (2) | `las anotaciones` |
| `Safety` (3) | `la seguridad` |
| `Fronts` (4) | `los frentes` |
| `Modules` (5) | `los módulos` |
| `SideB` (6) | `el lado B` |
| `Composite` (7) | `la parte compuesta` |
| `OutsideEditor` (8) | `fuera del editor` |

**La enumeración de una lista de grupos:** cada grupo una sola vez, aunque tenga varias entradas, en el orden de la tabla; con su literal; unidos así:
- uno: `A`;
- dos: `A y B`;
- tres o más: `A, B y C` (comas entre todos menos los dos últimos, que van con ` y `).

Ningún literal empieza por «i» ni por «hi», así que la conjunción es siempre ` y `.

**Las líneas, en este orden y solo si tienen contenido:**

| Línea | Cuándo | Texto literal |
|---|---|---|
| 1 | `ChangedExternally` no está vacía | `Cambiado fuera: <enumeración>.` |
| 2 | `ChangedByUser` no está vacía | `Cambiado por ti: <enumeración>.` |
| 3 | `Overlapping` no está vacía | `Cambiado en los dos: <enumeración>.` |
| 4 | `Structural` no está vacía | `Cambios estructurales: <enumeración de los grupos de las entradas estructurales>.` |
| 5 | `ViewsToErase` tiene 1 | `Vistas: se borrará 1 lateral obsoleto.` |
| 5 | `ViewsToErase` tiene n > 1 | `Vistas: se borrarán <n> laterales obsoletos.` |
| 6 | `ViewsRejected` tiene 1 | `Vistas: 1 no se puede conservar.` |
| 6 | `ViewsRejected` tiene n > 1 | `Vistas: <n> no se pueden conservar.` |
| 7 | `AllViewsObsolete` | `Todas las vistas quedarían obsoletas.` |

- `<n>` va en cifras, con cultura invariante.
- `ViewsRejected` son claves y no motivos: la línea dice cuántas, no cuáles.
- **El punto final,** siempre, una sola vez por línea.
- `IsEmpty` es verdadero si no hay ninguna línea.

**El ejemplo de la pregunta 2,** con un cambio de fuera en el nombre y en los frentes: `Cambiado fuera: el nombre y los frentes.`, más la línea 4 si el cambio de los frentes es estructural.

**Las líneas de CT-PBT4B-06** se miden antes de emitir PB-T4b con el clasificador real (PB-T3a, ya cerrado), en la corrida de medición de la precondición 6, porque cambiar los frentes cambia también hojas de otros grupos (P3:713). El contrato de PB-T4b las lleva literales.

#### D-PBT4-10. El código del piloto se retira en PB-T4d, tras PB-T4c (Q1 del Owner y su segunda decisión; I-26; V42-01)

**Lo decidido:**
- «Retirarlo (Recomendado)» (`owner-pbt4-decisions.md`, pregunta 1): «Se retira lo que solo usaba el panel»; del modelo paralelo «solo queda la carga, dentro del lector, porque el modelo completo la usa»;
- **«Retirar tras PB-T4c (Recomendado)»** (`owner-pbt4b-retire-list-decisions.md`, pregunta 1): «El código del piloto NO se retira en PB-T4b. Queda sin uso hasta una subtarea propia, PB-T4d, que va después de PB-T4c.» «PB-T4d vuelve al Owner con su lista antes de emitirse. La autorización condicional de `owner-pbt4-decisions.md` (pregunta 4) no la cubre.»

«Conservarlo sin uso», «Dos modelos hasta F5-A» y «Retirar en PB-T4b» no se hacen.

**En PB-T4b y PB-T4c, el código del piloto queda sin uso, y no se toca:**
- el panel deja de usarlo en PB-T4b (D-PBT4-04 a 08), pero ningún archivo de `Pilot/` cambia por la retirada, y sus pruebas siguen en verde;
- **PB-T4b no borra ni edita ninguna prueba ni ningún ayudante:** solo pone los `Skip` de su lista (D-PBT4-11), como en D-1;
- la regla de la escala queda, mientras tanto, también en `PushBackPilotFieldAdapter`, sin uso; la única que usa el panel es `PushBackAnnotationScaleRule` (D-PBT4-06).

**Lo que retira PB-T4d** (lo que el panel ya no usa):
- el comparador (`PushBackPilotStateComparer`);
- el adaptador del campo (`PushBackPilotFieldAdapter`);
- `KeepPilotEditsOn` y `ComposeForWrite`;
- `PushBackPilotUpdatePlanner.Prepare` (del piloto);
- `PushBackPilotUpdateRun.Precheck` y `Execute` (del piloto).

**Lo que se queda,** porque el modelo completo lo usa (R-17):
- el lector del piloto (`PushBackPilotIntentReader`) y su materialización (`PushBackPilotMaterializer.Materialize`);
- el tipo `PushBackPilotDraftState` que devuelve ese lector;
- las constantes de `PushBackPilotMaterializer`.
- **La copia de la carga de A-5** (PM:150-172, dentro de `Materialize`) no se puede retirar entera, porque el lector la usa. Se declara: queda solo como la admisión del lector.

**El esbozo de PB-T4d** (su contrato completo viene después, con su paquete; §6.4):
- **sustituye con registro 139 pruebas** (causa C-7): 72 que nombran lo retirado por texto, con E-3, E-5 y E-6 de D-1 (F4B-E29: «PB-T4 sustituirá tres de esos equivalentes al quitar el piloto»), y 67 que dejan de compilar al borrarlo (R-36);
- **borra con registro las 55 originales de las 9 clases** que siguen saltadas desde PB-T4b y que dejan de compilar al borrar lo retirado, y **los 29 miembros de apoyo** que lo nombran (R-36);
- **la lista,** en `pbt4d-retire-list.tsv` (§8.3), es un esbozo medido en `7f0d9aa3`: PB-T4d la vuelve a medir sobre el cierre de PB-T4c y vuelve al Owner con ella antes de emitirse;
- **lo que cambia, y lo que no** (I-26): la retirada después de PB-T4c **no evita el borrado** de las 55 originales y de los 29 miembros de apoyo: cuando se retire, dejarán de compilar igual. Lo que cambia es que, para entonces, ninguna corre, porque sus copias o sus equivalentes están en verde desde PB-T4b; y que PB-T4b conserva la premisa de la pregunta 3 («No hace falta ninguna excepción a la regla de no editar pruebas ni ayudantes»);
- **lo que PB-T4d tendrá que ajustar de PB-T4b:** el punto 1 de CT-PBT4B-20 (el original igual a la base salvo los `Skip`) deja de valer para las 55 que se borran, así que CT-20 entra en su lista;
- queda una sola regla de la escala.

#### D-PBT4-11. La lista de sustituciones, «Saltar y duplicar» y el guion de la copia (B-03, B-04, B-06, I-03, I-04, I-18, I-21, I-22, I-23; V41-03 y V41-04; V42-01, V42-03, M-33, M-34)

**Lo decidido:**
- «Saltar y duplicar (Recomendado)» (`owner-pbt4-decisions.md`, pregunta 3): «Como en D-1, las 428 se saltan y se copian con un kit nuevo, y un guion comprueba que cada copia solo cambia el kit […]. No hace falta ninguna excepción a la regla de no editar pruebas ni ayudantes.» «Editar una línea» e «Implementación de referencia» no se hacen: EIH no se edita;
- **«Sí, esa lista (Recomendado)»** (`owner-pbt4b-retire-list-decisions.md`, pregunta 2): la lista exacta de PB-T4b son **470** pruebas, y el Owner la toma como «la que vio»: las 428 del panel (375 copias mecánicas comprobadas por el guion y 53 equivalentes a mano, con su registro), 15 guardas de texto y 27 pruebas de la interfaz de «Conservar». «Una prueba que falle fuera de ella vuelve al Owner.»

**La lista de PB-T4b, fijada por sha256** (V42-03): `pbt4b-substitution-list.tsv`, **sha256 `44393b19d1f7490081c8ec9bac633ea4acbfe792103e5e5c3ca5e1f3fa1163e4`** (103 000 bytes, LF, 470 filas y la cabecera). Es, en su orden y con la cabecera, el filtrado de `pbt4-substitution-lists.tsv` (sha256 `f24c2dbc41bdd3d989796bee420b500fdf722d9f93bd408f5580c62019c8dfa5`) a las filas cuya primera columna es `PB-T4b C-0`, `PB-T4b C-5` o `PB-T4b UI`; lo hace `split_lists_pbt4.py`. El generador de PB-T4b comprueba el sha256 del archivo y, además, que el filtrado del archivo completo da byte a byte el mismo texto. Es la misma lista que la de `pbt4-substitution-lists.md` (sha256 `0f4c9bad…cfbdac`) sin las filas de C-7.

**Las 470 no dependen de la retirada** (M-33): 9 de las 15 de la letra de las guardas nombran lo que PB-T4d retirará (G-4, la regla del adaptador de PEG:280 y `KeepPilotEditsOn` en EIG), pero las rompe el cambio del anfitrión de PB-T4b, no la retirada. Por eso 470 es exacto con cualquier opción de la retirada.

**La firma de una causa** se mide sobre el texto de la prueba (sus atributos y su cuerpo) más el de los ayudantes de su mismo archivo que nombra, a un nivel, sin las pruebas que ya llevan `Skip`. **C-7, ahora de PB-T4d, se mide además por compilación:** lo que deja de compilar al borrar lo retirado, por referencia de código transitiva (`retire_compile_pbt4.py`).

| Causa | Firma |
|---|---|
| **C-0. La lectura viene del kit del piloto** | El archivo es una de las 9 clases de R-21: todas sus pruebas se saltan y se duplican |
| C-1. El tipo del borrador | `Find<PushBackPilotDraftState>`, `DraftSession<PushBackPilotDraftState>`, `PushBackPilotDraftState` o `PushBackPilotStateComparer` |
| C-2. El rechazo A/B | `OutOfPilotDiagnostic` o `CompositeOutOfPilotReason` |
| C-3. «Conservar» en dos tiempos | `TryKeepMine` o `KeepPilotEditsOn`; en la UI, `RequestKeepMine`, `KeepMine` o `BrowserFieldKeepMineButton` |
| C-4. «Actualizar» por `ExecuteFull` | `PushBackPilotUpdateRun.Execute` o `Precheck`, `NonPilotContentReason`, `TryUpdatePilot` |
| C-5. La letra de una guarda | Las tablas de §4, método a método |
| C-6. El adaptador del piloto | `PushBackPilotFieldAdapter` |
| C-7. La retirada (PB-T4d) | Nombra un miembro que se retira (por texto), o deja de compilar al retirarlo (por compilación). **No es de PB-T4b** |
| C-9. Lecturas del piloto fuera del kit | `FromPilot(` o `PrepareKit.Reread(` en la prueba o en un ayudante |

**Las listas guardadas** (I-22, I-23): `pbt4-substitution-lists.md`, método a método con su causa y su destino, y la misma tabla en `pbt4-substitution-lists.tsv`; el mapa de la copia, prueba a prueba, en `pbt4-copy-map.tsv`. Las generan `lists_pbt4.py`, `copy_map_pbt4.py` y `retire_compile_pbt4.py` (con `lists_core.py` y `copy_map_defs.py`), con `python -I`, por objetos git en `7f0d9aa3`. Los números, en §8.3.

**«Saltar y duplicar», cómo se hace:**
1. **Los originales.** Cada `[Fact]` y `[Theory]` de las 9 clases gana un `Skip` con su motivo literal, y nada más cambia en el archivo:
   `Skip = "I-64 F4B PB-T4b: sustituida por su copia en <Clase>Full (registro declarado superseding-characterization.md; decision del Owner owner-pbt4-decisions.md, pregunta 3)."`, con `<Clase>` el nombre de la clase original sin `Tests` (por ejemplo `WorkspaceLifecycleHostFullTests`). Las que van a mano llevan, en lugar de «su copia en <Clase>Full», «su equivalente <CT> de <clase>», con la CT y la clase de su fila de RR-3.
2. **Las copias.** Cada una de las 9 clases tiene un archivo nuevo, `tests/RackCad.Tests/Workspace/<Clase>Full.cs`, en `namespace RackCad.Tests.FullPanelCopies;`, que es el original:
   - **sin los tipos del kit,** `EditIntentFakeReader`, `EditIntentKit` y `PrepareKit`: no se copian, porque los sustituye el kit nuevo;
   - **sin las pruebas que van a mano** (abajo);
   - **con el mapa M aplicado,** y nada más;
   - **partido en dos clases por su resultado en el RED** (M-34; la T-1 de la §16.4 de la revisión): `<Clase>FullRedTests`, con las copias que fallan sobre el esqueleto de PB-T4b, y `<Clase>FullGreenTests`, con las que pasan. La partición es cerrada: la mide la precondición 6 sobre la copia exportada con el esqueleto, las copias y el kit, y el contrato de PB-T4b la lleva por nombre. Las dos clases van en el mismo archivo y en el orden del original.
3. **El kit nuevo,** en un archivo nuevo, `tests/RackCad.Tests/Workspace/WorkspaceFullReadKit.cs`: `FullEditIntentFakeReader`, `FullEditIntentKit` y `FullPrepareKit`, con **los mismos miembros, con la misma firma, que los tipos del kit que sustituyen** (re-tipados por M-04 a M-06). Cada lectura que devuelven es `EditIntentReadResult.FromReads(<la del piloto>, PushBackFullDraftReader.Read(<el mismo escaneo>, catalog), catalog)`, sobre el mismo escaneo de `IntentKit` que la del piloto. Los miembros de `PrepareKit` que llaman al planificador del piloto llaman en el kit nuevo a su equivalente completo (`PrepareFull`). Es el único código de prueba nuevo que no es copia, y es lo que el Owner llamó «el kit». **Es código de prueba, no la implementación de `src/`** (M-34): va literal en el contrato de PB-T4b, como cualquier CT, y la precondición 6 lo usa para medir la partición del RED.

**El mapa cerrado M** (`copy_map_defs.py`), en este orden; ninguna otra diferencia:

| Id | Del original | A la copia |
|---|---|---|
| M-00 | Cada tipo de primer nivel `T` del archivo que se copia (la clase de prueba y sus dobles, como `LifecycleScriptedAdvisor` o `PilotEditThrowingComparer`) | `T` + `Full`, salvo la clase de prueba `<Clase>Tests`, que pasa a las dos clases `<Clase>FullRedTests` y `<Clase>FullGreenTests` según la partición cerrada; también en cada uso dentro de la copia |
| M-01 | `EditIntentFakeReader` | `FullEditIntentFakeReader` |
| M-02 | `EditIntentKit` | `FullEditIntentKit` |
| M-03 | `PrepareKit` | `FullPrepareKit` |
| M-04 | `PushBackPilotDraftState` | `PushBackFullDraftState` |
| M-05 | `PushBackPilotStateComparer` | `PushBackFullDraftStateComparer` |
| M-06 | `PushBackPilotFieldAdapter` | `PushBackFullFieldAdapter` |
| M-07 | `.Inputs.Annotations.AnnotationScale` y `.Inputs.Annotations!.AnnotationScale` | `.Editable.AnnotationScale` |
| M-08 | `.Inputs.PalletTolerance` | `.Editable.PalletTolerance` |

M-01 a M-08 son sustituciones de palabra completa (`\b`), las de `copy_map_defs.py`. M-06 exige que `PushBackFullFieldAdapter` tenga las cuatro constantes que las pruebas nombran, con el texto del piloto: `AnnotationScaleKey`, `EmptyMessage`, `NotANumberMessage` y `NotPositiveMessage`.

**Las que no admiten copia mecánica, «a mano»:** 53 de las 428, cuyo texto, ya mapeado, sigue con algo que el mapa no cubre: R-C4 29; R-C9 8; R-retirado 6; R-C3 5; R-C2,R-C9 2; R-estado 1; R-retirado,R-estado 1; R-C2 1.
- R-C2, R-C3 y R-C4: sus aserciones describen lo que PB-T4b cambia por diseño (el rechazo A/B, «Conservar» en dos tiempos, «Actualizar» por `ExecuteFull`); la v4 ya decía que «llevan además su equivalente a mano»;
- R-C9: construyen lecturas del piloto fuera del kit (`EditIntentReadResult.FromPilot(`), que el kit nuevo no puede rehacer sin traducir;
- R-retirado y R-estado: llaman a las funciones del piloto que el mapa no re-tipa (el `Prepare`, el `Execute` o `KeepPilotEditsOn` del piloto, sobre un borrador que en la copia ya es completo), o usan la forma del estado del piloto (`.Inputs`, `state.Editor`, `new PushBackPilotDraftState(…)`). La copia no compilaría aunque el código del piloto siga en PB-T4b.

Cada una lleva su equivalente escrito a mano, con su fila en la tabla de RR-3 del contrato de PB-T4b, que dice qué protección conserva y dónde. **Fuera de las pruebas no queda nada que impida compilar las copias** (`copy_map_pbt4-out.txt`): los únicos restos son nombres de operaciones del puerto en dobles (`TryKeepMine`, `TryUpdatePilot`), que siguen existiendo.

**El guion de la copia es una CT, CT-PBT4B-20** (`WorkspaceFullCopyGuardTests`, Core, `ExpectRed` false; §5.2). Para cada una de las 9 clases lee, como texto, el original y su copia, y comprueba:
1. que el original, sin las líneas de atributo con el `Skip` de este paquete, es **byte a byte** el de `BaseSha` (la CT lleva el sha256 de cada original en `BaseSha`, que fija el contrato);
2. que **cada** prueba del original tiene, o una prueba de igual nombre en la copia, o una fila en la lista cerrada «a mano» (sus 53 nombres, literales en la CT);
3. que la copia es **exactamente** el original sin los tres tipos del kit y sin las pruebas «a mano», con M-00 a M-08 aplicados en ese orden, y sin los `Skip`: la CT aplica el mapa al texto del original y compara con **la unión** de `<Clase>FullRedTests` y `<Clase>FullGreenTests` (cada prueba en la de su partición), con los saltos de línea normalizados a `\n`;
4. que ninguna copia nombra `EditIntentFakeReader`, `EditIntentKit`, `PrepareKit`, `PushBackPilotDraftState`, `PushBackPilotStateComparer` ni `PushBackPilotFieldAdapter` como palabra completa.

Sus líneas: `copia <Clase> original-igual-a-la-base=True pruebas=<n> copiadas=<c> a-mano=<h> copia-igual-al-mapa=True`, una por clase, con los números de `pbt4-copy-map.tsv`; y cuatro autopruebas en negativo sobre texto inyectado en memoria (una línea de más en una copia, una aserción cambiada, un `Skip` de más en un original y una prueba sin copia ni fila), cada una `=falla`.

**C02 no tiene excepción en PB-T4b:** el único cambio en las 9 clases es el `Skip`, que es el mecanismo de D-1; ningún ayudante existente se edita ni se borra. El borrado de las 55 originales y de los 29 miembros de apoyo es de PB-T4d, con su lista y su vuelta al Owner (D-PBT4-10).

### PB-T4c

#### D-PBT4-12. El gesto, con `CommitField` como frontera (R-PB-10; I-02)

- **El botón:** `BrowserFieldEditAllButton`, `Content="Editar todo"`, `AutomationProperties.Name="Editar el rack completo en el editor"`. Va en el `WrapPanel` de PV:218-254, tras «Editar». Lo maneja `OnBrowserFieldEditAllClick` con `Contained(() => _viewModel.Browser.Field.EditAll())`. Es visible con `Browser.Field.ShowEditAllButton`, en las mismas condiciones que «Editar».
- **El gesto `EditAll()`** en FVM:
  1. si el campo tiene texto pendiente, `_port.CommitField(origin, rack)`, «the same two-phase step as a blocking boundary» (WHC:1149):
     - `Invalid`: no abre, y muestra «No se abrió el editor: <mensaje>.»;
     - `Busy` o `Rejected`: no abre, y muestra su motivo;
     - `Applied` o `NotEditable` por falta de borrador: sigue;
  2. `_port.TryEditAll(new EditIntentRequest(session, rack, null, null))`.
  
  La petición lleva `ChosenReference` nulo, porque el origen lo guarda el anfitrión (D-PBT4-03), y sin testigo.
- **Sin valor nuevo en `PilotBoundaryKind`:** URT:1107-1109 sigue en verde.
- **El anfitrión no abre si el borrador aún tiene texto pendiente,** por si el gesto no lo confirmó: «Hay texto sin confirmar en el campo.».



#### D-PBT4-13. El anfitrión, los dos reenvíos y el puerto generalizado (A-T3-11, A2-1; B-05)

Como en la v3: `TryEditAll`, con cuerpo por defecto, con las reglas de `TryKeepPilotEdit`, y sus resultados.
- **Los dos reenvíos explícitos:** `WorkspacePilotEditGate.TryEditAll` y `PilotEditAdapter.TryEditAll`. Lo prueban CT-PBT4C-12 y 13.
- **El lanzamiento y el adaptador del puerto,** en `Draft/`.

#### D-PBT4-14. Los estados de cada operación durante el editor (I-01; I-15)

Como en la v3. `RequestUpdate` da `Busy` por la marca (WHC:1388).

#### D-PBT4-15. El modal y el abridor, en dos piezas (I-01; B-07; M-14)

**Por qué dos piezas.** Una sola no cabe en ningún sitio sin romper una guarda medida:
- **en WCo** (anidada) tiene que nombrar la ventana, de `RackCad.UI.Systems.PushBack`, y `WorkspaceHostSourceGuardTests.cs:398-420` prohíbe `RackCad.UI` en todo archivo de `Plugin/Workspace/` salvo la paleta y el puente;
- **en un archivo nuevo de `Plugin/Workspace/`** rompe las dos guardas de 9 archivos (EIG:1011-1022; `WorkspaceNavigationHostTests:714-723`);
- **fuera de `Plugin/Workspace/`,** si implementa una interfaz de `RackCad.Application.Workspace`, rompe `WorkspaceBridgeSourceGuardTests.cs:364-367`.

**Las dos piezas:**
1. **El modal, anidado en WCo:** `private sealed class FullEditorModal : IWorkspaceFullEditorHost`. Su único método es `public void ShowModal<TLaunch>(TLaunch launch) where TLaunch : class { using (_controller.EnterPanelModal()) _open(launch); }`, y `_open` es un `Action<object>`.
   - **No nombra `RackCad.UI`** (398-420 sigue en verde) **ni `PushBack`** (WCo no está en el censo de CT-PBT1B-15, R-31).
   - Se asigna en WCo con `_host.SetFullEditorHost(new FullEditorModal(this, RackWorkspaceEditorWindow.Show))`, después de `SetPilotUpdateHost`, así que EIG:1150-1190 sigue en verde.
2. **La ventana, en `src/RackCad.Plugin/Views/RackWorkspaceEditorWindow.cs`:** `internal static class RackWorkspaceEditorWindow`, con un solo método, `internal static void Show(object launch)`:
   - `var window = RackPushBackSystemWindow.ForDraft((PushBackDraftEditorLaunch)launch);`
   - `AcApplication.ShowModalWindow(window);`, sin `Owner`, como el abridor clásico.
   - **No nombra `RackCad.Application.Workspace`** (364-367 sigue en verde) ni ningún término de G-9 (no nombra el piloto).

**`IWorkspaceFullEditorHost`,** en `Application/Workspace/`, con el método genérico `void ShowModal<TLaunch>(TLaunch launch) where TLaunch : class;`. WHC lo llama con el `PushBackDraftEditorLaunch`. El campo `_fullEditorHost` es de un tipo de `Application.Workspace`, así que EIH:1088 sigue en verde. Se declara como excepción a R-PB-05 y a ADR-0029 D12.

**La guarda que rompe: CT-PBT1B-15** (`PushBackSharedReferenceGuardTests:306-` y :381: «archivos=14»). `RackWorkspaceEditorWindow.cs` nombra `PushBack` (la ventana y el lanzamiento), así que el censo pasa a 15. **Ninguna carpeta lo evita:** todo archivo nuevo que abra la ventana nombra `PushBack`, y ponerlo en uno de los 14 existentes mezclaría el abridor con el camino clásico o con el escritor (R-31). Se sustituye con registro en PB-T4c (C-5), con un equivalente: «archivos=15 (los 14 y el abridor); asignaciones=7, sin cambio»; las demás líneas, iguales.

#### D-PBT4-16. La guarda de OB-01.3 (M-05, M-09)

CT-PBT4C-07 mira **seis sujetos**:
- `RackWorkspaceEditorWindow.cs`;
- el cuerpo de `FullEditorModal` en WCo;
- `TryEditAll` y sus ayudantes en WHC;
- el gesto `EditAll` en FVM;
- `Draft/PushBackFullDraftLaunch.cs`;
- `Draft/PushBackFullDraftStatePort.cs`.

Ninguno nombra `RackPushBackCommands`, `RackMenuCommands`, `EditorModules`, `RackViewBatchExecution`, `DrawAndPlace`, `RedrawInPlace`, `InsertRequested` ni `InsertionRequest`. La autoprueba en negativo inyecta cada nombre en cada sujeto.

**`RackWorkspaceEditorWindow.cs` no lleva** `LockDocument`, transacción ni nada tras cerrar.


#### D-PBT4-17. «Cerrar» con las dos cosas (A-T2b-OWNER-01; M-12)

`PendingWork()` de W, **solo en WORKSPACE**:
- un módulo escenificado y cambios sin aplicar: una sola pregunta, `WorkspaceStagedAndUnappliedQuestion`, con el texto de la v2;
- uno solo: la de hoy;
- en CLASSIC, nada cambia.

**La prueba** (CT-PBT4C-09) lee `PendingWork()` sin cerrar la ventana. **Escenifica el módulo con la costura existente,** `HeaderConfiguratorPresenter` y `ConfigureModuleHeaderButton` (`PushBackHeaderAuthorityWindowTests.cs:99-106`), sin el configurador real.

### Común



#### D-PBT4-18. D2 (A2-5, A-T3-4; I-06, I-16, M-25)

**La letra del Owner** (`owner-d1-d2-decisions.md:113-131`), con la 7: «comparing InitialDraftState vs CurrentDraftState does not depend on mutable changes inside those shared objects».

**CT-PBT4B-10,** la huella tras cada operación:
- condición 3: `Begin`, `SetCurrentDraftState` y el rebase;
- condición 5: `SetPendingText` y `CommitField`;
- condición 6: `DiscardAndReload` y `KeepMineOnCurrentVersion` con `KeepOn`;
- **condición 7:**
  - sobre una copia del borrador, se sustituye la línea base resuelta del lado A por la de otro fixture, de contenido distinto, con `PushBackEditorState.SetWorkingBaseline(PushBackSystem)` (`PushBackEditorState.cs:74-79`);
  - el comparador da `True`, porque no compara la línea base ni el informe (`PushBackEditableStateComparer.cs:20-27`), e `IsDirty` da `False`;
  - la línea es `linea-base-de-otro-contenido isdirty=False`;
  - el informe de reconciliación se sustituye por el miembro con el que CT-PBT1B-16 lo lee para su huella, con la línea `informe-de-otro-contenido isdirty=False`.

**CT-PBT4C-11,** sobre `ApplyToDraft`.

**El límite literal,** en C17. **M11** no aplica.


#### D-PBT4-19. Lo que PB-T4 no hace

- No toca la ventana clásica, RACKEDITAR, RPB ni el menú. **W solo cambia la pregunta de D-PBT4-17,** en WORKSPACE; el XAML no cambia.
- No crea vistas.
- No traduce estados.
- No usa el asiento en `Recompute` (A-T3-9).
- No abre AUTH-13 ni cambia `DormantTailOutOfPilotReason` ni el camino clásico.
- No añade botones ni campos fuera de «Editar todo», el texto de la presentación y la pregunta de «Cerrar».
- No cachea las pasadas.
- **No cambia el comportamiento de ninguna caja numérica:** `NumericFieldValidation` delega en la misma lógica (D-PBT4-06).

#### D-PBT4-20. Los arrastres y las limitaciones

| Arrastre o limitación | Dónde | Por qué |
|---|---|---|
| A-T3-1, A-5 y R-T1-17 | PB-T4d: se retira tras PB-T4c (V42-01) | D-PBT4-10 |
| A-T3-2 (OB-03.2) | PB-T4b | D-PBT4-05 |
| A-T3-3 (`KeepOn`; la presentación; las guardas «sin conectar»; el nombre `""`) | PB-T4b. El nombre `""` sale como «Cambiado por ti: el nombre.» | D-PBT4-08 y 09 |
| A-T3-4 (D2, M11, M15, F4B-E30) | D2 en PB-T4b y PB-T4c; M11 no aplica; M15 se mide (§7); F4B-E30, una línea en CT-PBT4C-10 | D-PBT4-18 |
| A-T3-8 | PB-T4a | D-PBT4-03 |
| A-T3-9 | Fuera: registro en DISP y entrada en `ideas-futuras.md` (M-07) | CT-PBT3A-01 y CT-PBT2B-02 ya fijan la igualdad; tocar `Recompute` arriesga T1B |
| A-T3-10 | Las constantes se quedan en `PushBackPilotMaterializer` (el lector las usa) | D-PBT4-10 |
| A-T3-11 y A2-1 | PB-T4c | D-PBT4-13 |
| A2-2, A2-3 y A2-9 | PB-T4c | D-PBT4-12 a 16 |
| A2-6 (la regla) | PB-T4b | D-PBT4-06 |
| A2-11 | CT-PBT4C-10, sin AutoCAD; lo físico, el Smoke-PB | §5.5 |
| A2-13 y A-9 (`NaN`) | CT-PBT4B-11 | D-PBT4-18 |
| A-T2b-OWNER-01 | PB-T4c | D-PBT4-17 |
| OWNER-01 de PB-T2c (lado B sin frentes) | CT-PBT4B-09 | D-PB-08 |
| OWNER-01 de «Restaurar»; PBT3B-IR-M01 y OWNER-2; el configurador | Fuera o regla, como en la v1. OWNER-2 de PB-T3b se anota en `ideas-futuras.md` con el cierre de PB-T3c (F4B-E48); los riesgos del borrado real (OWNER-1 a OWNER-3 de F4B-E48), al Smoke-PB (§5.5) | |

---



## 4. Las guardas, por subtarea

### 4.1 PB-T4a (V41-06, V41-07, V41-16, V41-17; B-08, I-25)

**Una sola sustitución: CT-PBT3A-12** (`PushBackDraftWriteGuardTests.cs:60-95`). En el GREEN, RIA nombra `PushBackFullDraftReader`, uno de sus cinco nombres (`ConnectedNames`, :45-48), fuera de `Draft/`, PP y UR. La sustituye CT-PBT4A-06, el mismo censo con RIA admitida. En el esqueleto RIA no cambia, así que CT-PBT3A-12 pasaría; lleva el `Skip` desde el RED, como D-1.

**B-08, arreglado.** EIG:350-364 (`TheReadBuildsTheInputFromTheScanAndTheEvidenceOnce…`) cuenta en **todo** RIA un `new PushBackPilotIntentScan(` y un `PushBackPilotIntentReader.Read(` (:356-357). La v4.1 ponía la segunda lectura del piloto en `AtOrigin`, y la habría roto. Con `ReadAt` en `Draft/` (D-PBT4-03), RIA conserva una de cada, en `Read`, y esa prueba sigue en verde.

**El recorrido entero (I-25), por diferencia** (`walk_guards_pbt4a.py` sobre `7f0d9aa3`, con el GREEN literal de `green_pbt4a.py`; las salidas, `pbt4a-guard-walk.tsv`, `pbt4a-guard-walk-out.txt` y `pbt4a-guard-walk-flagged.txt`):
- **Qué recorre:** las **46 clases** de prueba que nombran por texto los archivos que toca PB-T4a (WHC, WEI, NH y RIA), sus carpetas (`Application/Workspace/`, `Plugin/Workspace/`, `Plugin/Views/`, `Systems/PushBack/Draft/` y `src`) o los ayudantes de carpeta de `WorkspaceHostSource` (`ApplicationFiles`, `PluginFiles`, `AllProduct`). Son las de la §16.2 de la revisión (EIG, CAG, IG, `WorkspaceBridgeSourceGuardTests`, `WorkspaceBrowserHostGuardTests`, `WorkspaceNavigationAuthorityTests`, `WorkspaceNavigationHostTests`, `WorkspacePaletteHostSourceGuardTests`, `WorkspacePilotReconciliationGuardTests` y `WorkspaceSourceGuardExceptionTests`) y las demás que el mismo criterio encuentra: **1 026 pruebas** sin `Skip`.
- **El método:** una prueba de texto que pasa en la base solo puede caer en el GREEN si alguna consulta que hace sobre el texto da otro resultado. Para cada prueba se toman todos los literales de cadena de su texto, de los ayudantes de su archivo que alcanza (transitivo) y de las constantes de ruta de otras clases que nombra. Cada literal se evalúa, como expresión regular y como subcadena, en la base y en el GREEN, sobre el archivo entero y miembro a miembro. Los archivos nuevos se miran aparte, para las pruebas que barren su carpeta.
- **Lo que da:**
  - **764** no leen ninguno de los archivos tocados ni sus carpetas: verdes por construcción;
  - **162** los leen y ningún literal con nombre da otro resultado ni seleccionan un cuerpo que cambie: verdes por construcción;
  - **100** quedan señaladas (67 por un literal que cambia de cuenta, 33 porque leen un cuerpo que cambia) y **se leyeron una a una:**
    - **1 cae: CT-PBT3A-12,** la de la lista;
    - **las otras 99, verdes.** Los motivos, por grupos:
      - las que miden cuerpos que el GREEN no toca (el drenaje, `PassGate`, `AdviseClose`, `SessionDestroyed`, `MarkPilotDraftsPossiblyStale`, `DiscardPilotEdits`, `UpdateGate`, `TryUpdatePilot`, los cuerpos del resumen de NH, `ReadSummary`, `ExecuteAsync`, `RunOnActiveDocument`, `ReturnFocusToDrawing`, `TakeLossNotices` y `TakeCloseNotices`), aunque la cuenta del archivo entero cambie;
      - las de los cinco cuerpos que releen, de la recarga y de las explícitas del puerto (EIG:562-580, :582-596, :692-778, :870-894, :983-1009 y :1357-1390; WLG:469-681; PEG:379-598 y :715-734): las cuentas siguen, porque `ReadIntentAtOrigin(` no casa con `\.ReadIntent\s*\(` ni `_editIntentReader is` con `_editIntentReader\s*[.!]`, y el texto de la sentencia no tiene ninguna marca, ningún texto del vuelo ni ninguna operación del ciclo de vida;
      - las de RIA (EIG:88-118, :170-204, :206-301, :303-320, :322-364): la firma de `Read`, sus motivos, sus `catch`, su `finally`, `AssertNoOpenTransaction`, un `CaptureDrawing`, un `new Handle`, un `RackBlockData.Read`, ningún `GetObject`, ningún `throw`, ninguna API de escritura de `AdapterWrites` y, con `ReadAt`, una lectura de cada lector;
      - las de NH (EIG:405-470; RG:696-712; CAG:441-465; IG:590-720; WBSG, los ocho: los enganches, la persistencia, la superficie pública y los puntos de entrada): `ReadIntentAtOrigin` no añade ningún evento, ninguna persistencia, ningún tipo de AutoCAD en una firma pública, ningún `_authorize(`, ningún motivo, ningún `RackLog.Exception(` ni ningún `LockDocument`;
      - las de carpeta (EIG:505-535 y :1011-1024; WNAT:187-196; WHSG:155-166; G-6 y G-9 de IG; WLG:732-744; PEG:916-975 y :1000-1076): la interfaz nueva no nombra el piloto, ni `WorkspaceHostCore`, ni ninguna API de escritura, y `Plugin/Workspace/` sigue con sus 9 archivos; `PushBackOriginByText.cs` nombra el piloto, pero no usa ninguna API de escritura ni ningún bloqueo (G-9 lo admite así, como a `PushBackFullDraftReader.cs`);
      - CT-PBT3A-13 y 14 leen una lista fija de 8 archivos de `Draft/`, que no cambia (de ahí CT-PBT4A-08);
      - G16, las tres de `SelectiveEditorOpenTests` y la documentación de los estados de WEI (WLG:869-885; PEG:1104-1122) leen otros archivos, o partes de WEI que no cambian.
- **La reflexión:** las guardas que la usan sobre WHC (EIH:993-1104, WLG:357-358, PEG:383-385, WPUH:503 e IG:427-428) siguen, porque WHC no declara nada nuevo; las que enumeran tipos del ensamblado (`NoProductPlannerGuardTests`, `RackModuleEditSessionTests:497`, `CustomPropertiesProjectTests:329` y `RackViewCountInvariantTests:39`) miran otros espacios de nombres o nombres que los tipos nuevos no usan.
- **Las aserciones traducidas** (`measure_pbt4_v42.py`; sobre el mismo GREEN, con la de B-08 y las dos contrafactuales):

| Guarda | Qué se comprueba | Sobre el GREEN |
|---|---|---|
| EIG:568-574 | Begin: orden, 1 `.ReadIntent(` y 1 `_editIntentReader` seguido de `.`/`!` | **verde** |
| EIG:569-574 | «Comprobar ahora»: orden, 1 `.ReadIntent(` y 1 `_editIntentReader` seguido de `.`/`!` | **verde** |
| EIG:578-579 | PassGate no lee | **verde** |
| EIG:604 | el drenaje del núcleo no nombra la intención | **verde** |
| EIG:620-623 | solo WHC llama a `.ReadIntent(` (NH y RIA del GREEN no) | **verde** |
| EIG:636 | el drenaje sigue limpio (autoprueba) | **verde** |
| EIG:671-688 | la base solo llega a sus consumidores; las cuentas por operación de `pilot.Read` y `.StaleBase` | **verde** |
| EIG:695-718 | las cuentas de los tres cuerpos de la reconciliación | **verde** |
| EIG:733-736 | seis `var pilot = read.PilotRead;` y seis `.ReadIntent(` en WHC | **verde** |
| EIG:761-769 | los tres cuerpos: orden, 1 `.ReadIntent(`, 1 `_editIntentReader` con `.`/`!` y la marca después de la lectura | **verde** |
| EIG:1124-1130 | «Actualizar» no lee | **verde** |
| WLG:369 | AdviseClose | **verde** |
| WLG:442 | SessionDestroyed | **verde** |
| WLG:623 | el drenaje | **verde** |
| WLG:634 | MarkPilotDraftsPossiblyStale | **verde** |
| WLG:645-656 | la recarga: orden, 1 `.ReadIntent(`, 1 `_editIntentReader` con `.`/`!`, 1 `pilot.Read` | **verde** |
| WLG:687 | el descarte explícito del anfitrión | **verde** |
| PEG:288 y :297 | solo cambian los cinco cuerpos que ya leían (ninguna operación sin lectura ni de «Actualizar»): check, confirm, discard, keep, reload | **verde** |
| Reflexión (EIH:993-1104, WLG:357-358, PEG:383-385, WPUH:503, IG:427-428) | WHC no declara nada nuevo: los mismos miembros declarados | **verde** |
| EIG:409-431 | el lector anidado: los ocho pasos en orden; 1 `LoadCatalog(`, 1 `RackPilotIntentAdapter.Read(` y 1 `_targets.Revalidate(` en NH | **verde** |
| EIG:441-453 | NH: dos clases y el lector anidado implementa la interfaz | **verde** |
| EIG:465-470 | NH: un `read.Pilot` y ningún `PushBackPilot` ni `RackCad.Application.Systems` | **verde** |
| RG:696-712 | cada `_authorize(` del lector tiene la forma exacta | **verde** |
| CAG:410-451 (NH) | ReadOnActiveDocument: los tres motivos en español y ninguno en inglés | **verde** |
| NHT:418-559 y WBHG:72-168 | las cuentas de NH (`RackLog.Exception(` 1, `ReturnFocusToDrawing(` 2, `.Focus(` 1, sin async/await; los `+=` 2 de NHT:984 y WBHG:218 son de la paleta, no de NH) | **verde** |
| IG:684-685 (G-9) | NH: `LockDocument` 2 | **verde** |
| EIG:132-147 | RIA: la firma de `Read` (:139-141), tres clases de resultado y nada público | **verde** |
| EIG:150-160 | RIA: ningún campo de un objeto del anfitrión ni campo estático | **verde** |
| EIG:384-388 | RIA no sabe nada del Workspace ni de la navegación | **verde** |
| CAG:410-464 (RIA) | Read: los cinco motivos en español y dos `ex.Message` | **verde** |
| IG:684 (G-9) | RIA: `StartTransaction` 1 y `LockDocument` 1 | **verde** |
| EIG:350-364 (B-08) | RIA: un `new PushBackPilotIntentScan(` y un `PushBackPilotIntentReader.Read(` en todo el archivo, y el orden de `Read` | **verde** |
| EIG:322-336 y :338-347 | RIA: un `CaptureDrawing(` (y en `Read`), un `new Handle(`, sin `Handle.Parse` ni `GetObjectId(` | **verde** |
| CT-PBT4A-07 (nueva) | `AtOrigin` solo llama a `ReadAt`: sin lecturas propias, sin `CaptureDrawing`, bloqueo, transacción, literal de cadena ni `ex.Message`; RIA nombra una vez cada lector, en `Read` | **verde** |
| CT-PBT4A-04 y 08 (Draft) | `PushBackOriginByText`: `Choose`, `AtKey` y `ReadAt`, con un lector de cada; ningún término de la pureza de CT-PBT3A-14 | **verde** |
| Contrafactual (EIG:139-141) | si `Read` ganara `originText` como parámetro obligatorio, la firma fijada por la guarda deja de casar | **ROJA (esperada)** |
| Contrafactual (EIG:139-141) | si `Read` ganara `originText` como parámetro con valor por defecto, la firma fijada por la guarda deja de casar | **ROJA (esperada)** |

**En rojo:** 0 de 35 sobre el GREEN; contrafactuales en rojo, como se espera: 2 de 2.

**Lo que el recorrido no puede ver** (D = 0 en el GREEN, §5.1): una prueba que lea el texto por un camino que no deja ninguna ruta, carpeta ni ayudante en su código, o que dependa de la reflexión sobre un tipo que no se miró. Es un riesgo declarado, con su parada: **PBT4A-C16**. CA-T4a vuelve a leer las 100 señaladas sobre el texto del paquete custodiado.

### 4.2 PB-T4b

Las roturas por letra (C-5), método a método (`pbt4-substitution-lists.md`, «C-5 y C-6»; 15):

| Guarda | Métodos | Causa |
|---|---|---|
| CT-PBT4A-06 (el censo de PB-T4a) | 1 | Admite WHC: la sustituye CT-PBT4B-13 |
| CT-PBT3B-13 (`PushBackWritePathGuardTests:251-299`) | 1 | El anfitrión encuentra el borrador completo |
| G-4 (IG:487-502, `G4Violations`) | 2: las que exigen `Empty` sobre el texto real (:504-508 y :519-532) | `PushBackPilotUpdateRun.Execute(` deja de nombrarse |
| EIG:664-690, :692-721, :727-737, :754-778 y :799-810 | 5 | `pilot.Read` en `Begin`; `KeepPilotEditsOn` (:711, :776, :805); las seis `var pilot = read.PilotRead;` |
| PEG, la regla del adaptador estático de `HostViolations` (:280) | 5: las que exigen `Empty` sobre el texto real (:379-386, :495-514, :528-537), la que lo nombra en sus datos (:388-399) y la trampa de nombres (:1033-1054) | C-6 |
| RG:109-123 | 1 | `Mutate` sobre `RequestKeepMine` (RG:116) |

**Las 15 las rompe el cambio del anfitrión de PB-T4b, no la retirada** (M-33): 9 de ellas nombran lo que PB-T4d retirará (CT-PBT3B-13, G-4, `KeepPilotEditsOn` en las tres de EIG y la regla del adaptador en las cinco de PEG), pero caen porque WHC deja de nombrar `PushBackPilotUpdateRun.Execute(`, cambia el tipo del adaptador estático y el cuerpo de «Conservar». Por eso las 470 son exactas con la retirada en PB-T4d.

Las otras pruebas de las mismas guardas que nombran lo retirado solo por su ayudante (las autopruebas de `HostViolations` que exigen `NotEmpty`, y G-4 :510-517) siguen en verde en PB-T4b, y están en la lista de PB-T4d (C-7).

**La UI de la elección (C-3; 27):** 21 de `WorkspacePilotReconciliationTests` (entre ellas :780-793, «Asking for an option calls no operation of the port and only reads its memory») y 6 de `WorkspacePilotReconciliationViewTests` (los clics reales en «Conservar mi borrador» y en «Sí», :175-183, :199-213 y :264-276, y los botones).

**Las que siguen en verde:**

| Guarda o prueba | Por qué |
|---|---|
| `WorkspaceDraftPilotValidationParityTests:80-97` | El XAML no cambia |
| `NumericFieldValidationTests` y las 26 clases de UI que usan `NumericField` | Misma API y mismo comportamiento |
| EIG:670 y :720 (la base en WHC) | Por diseño (D-PBT4-08): WHC no nombra `.InitialDraftState`, `.StaleBase` ni `.Entries` |

### 4.3 PB-T4c

| Guarda | Causa | Resultado |
|---|---|---|
| PEG:155-163 (17 nombres) | C-5 | Se sustituye: 18 |
| **CT-PBT1B-15** (`PushBackSharedReferenceGuardTests:306-403`, «archivos=14») | **C-5 (B-07)** | **Se sustituye:** «archivos=15» con `RackWorkspaceEditorWindow.cs`; lo demás, igual |
| `WorkspaceHostSourceGuardTests:398-420` | — | **Verde:** WCo no nombra `RackCad.UI` |
| EIG:1011-1022 y `WorkspaceNavigationHostTests:714-723` | — | **Verdes:** ningún archivo nuevo en `Plugin/Workspace/` |
| `WorkspaceBridgeSourceGuardTests:364-367` | — | **Verde:** `RackWorkspaceEditorWindow.cs` no nombra `RackCad.Application.Workspace` |
| `WorkspaceBridgeSourceGuardTests:139-142` | — | Verde: `FullEditorModal` no nombra los prohibidos |
| G-9 | — | Verde: el abridor no nombra el piloto |
| EIG:1150-1190, CT-PBT2B-08, URT:1107-1109, EIH:1066 y PEG:380-385 | — | Verdes |

**Si cualquier otra prueba falla,** fuera de las tablas y de la lista, es C16 y vuelve al Owner (pregunta 4).

## 5. La red de pruebas

### 5.0 Reglas comunes (enteras; V41-12)

**Las reglas de PB-T1 a PB-T3,** que se copian en cada contrato y no se heredan:
- `[Fact]` que acaban en **un** `Assert.Equal` de las líneas unidas por `\n`, con «R», cultura invariante, GUID fijo y orden ordinal;
- **un `[Fact]` por CT. Ninguna `[Theory]` nueva:** la TRX cuenta distinto una `Theory` con `Skip` (F4B-E46);
- la regla «EXCEPCION» para toda llamada al API nuevo: una excepción se escribe como la línea `EXCEPCION <Tipo>: <mensaje>` y no escapa de la CT;
- **las pruebas de UI:**
  - en `EditorDiscardPromptCollection`, con la carga dentro del `try` y `Cerrar` en `finally`;
  - sin teclado ni foco real, sin `MessageBox` real y sin ventanas que no sean la de Push Back;
  - **ninguna crea un `RackFrameConfiguratorWindow`** (F4B-E30), ninguna cierra una ventana con trabajo pendiente y ninguna escribe en el perfil;
- sin AutoCAD, sin BOM, sin «Guardar en biblioteca» y sin «Insertar»;
- ningún fixture nuevo en `Fixtures/I64`;
- **los ayudantes existentes se usan sin editarlos** (`IntentKit`, `PilotKit`, `DraftKit`, `PilotUpdateScopeDouble`, `PilotEraseScopeDouble` de `PushBackFullWriteSupport.cs`, `WorkspaceHostTestSupport`, `WorkspacePilotEditPortDouble`, `EditIntentKit`). **Lo que falte va en un archivo nuevo de la subtarea:**
  - PB-T4a: `tests/RackCad.Tests/PushBackFullReadSupport.cs`, con `PushBackFullReadKit` (las lecturas `FromReads` sobre `IntentKit`) y `PushBackOriginReaderDouble` (el lector con origen, que registra lo que recibe);
  - PB-T4b: el kit nuevo de «Saltar y duplicar» (`WorkspaceFullReadKit.cs`, D-PBT4-11) y el doble del puerto del panel;
  - PB-T4c: el doble del anfitrión del editor.

**El momento de cada lectura** (F4B-E30 y F4B-E33): cada línea se arma justo después de su último gesto. El borrador guarda copias, y el rebase de POST cambia `InitialDraftState` y `CurrentDraftState`. La huella se toma tras cada operación.

**Los gestos que no cambian nada** (la lección M-13): una edición escribe un valor distinto del que tiene el diseño, en un campo que el ensamblador conserva. Se usan los valores medidos de PB-T3: `PalletTolerance` 4.5 y `PalletDepth` 48.5. `BeamDepth` no se edita.

**Los archivos nuevos se comprueban contra las guardas existentes antes de fijar el esqueleto** (la lección de F4B-E42 y F4B-E45). Antes de emitir, la sesión:
- compila el esqueleto exacto del RED sobre una copia exportada (fuera del worktree);
- ejecuta la suite completa de Core y los filtros de UI de la subtarea;
- barre los nombres nuevos y los textos del esqueleto contra los literales de todas las guardas que leen `src/` (el barrido de nombres y el de sobrecargas);
- **mide las guardas sobre el texto literal del GREEN que prescribe el paquete,** aplicado en memoria, y **las recorre enteras**, prueba a prueba, en las clases que leen los archivos tocados (para PB-T4a, §4.1: `walk_guards_pbt4a.py` y `measure_pbt4_v42-out.txt`).

**Un solo «Actualizar» lógico:** las recetas de PB-T4b no llaman a «Actualizar» desde la ventana; solo desde el panel.

**Las CT de I-24** (CT-PBT4A-08, CT-PBT4B-19 y CT-PBT4C-15): copian, sin referenciarlos, las reglas de G-L1 de CT-PBT2A-04 (`PushBackDraftPortSharedReferenceGuardTests.cs`: `Tokens`, `Rebind`, `Rule1`, `Rule1b`, `Rule1c`, `Clean`, `Collapse`, `Members`, `Aliases`, `Violations` e `Inject`, con sus 15 autopruebas) y la pureza de CT-PBT3A-14 (`ForbiddenTerms` y `Purity`). Cada autoprueba se inyecta en memoria en el texto de **cada** archivo de su lista, y falla si las reglas la ven en todos.

### 5.1 PB-T4a: la lista cerrada CT-PBT4A-01 a 08 (Core; entera)

| CT | Qué fija | Las líneas (literales del paquete) | Clase | `ExpectRed` |
|---|---|---|---|---|
| 01 | `FromReads`: `PilotRead`, `FullRead` y `Catalog`; un nulo da `ArgumentNullException`; `FromPilot` y los demás estados los dejan en null | `fromreads piloto=True completo=True catalogo=True estado=Read`; `fromreads-nulo piloto=ArgumentNullException completo=ArgumentNullException catalogo=ArgumentNullException`; `frompilot completo=null catalogo=null`; `otros completo=null catalogo=null` (`NoActiveDocument`, `Busy`, `Rejected` y `Failed`) | `PushBackFullReadPathTests` | true |
| 02 | El lector completo y el del piloto sobre el mismo escaneo de `IntentKit`, por `PushBackFullReadKit` (`FromReads`): `pb-01-base`, `pb-20-ab` (rechazado por el piloto y admitido por el completo), `pb-21-cola` y el caso de AUTH-13 de PB-T3a | Una por diseño, `<diseño> piloto=<Admitted|NotAdmitted> completo=<Admitted|NotAdmitted> mismo-escaneo=True`; entre ellas, literal, `pb-20-ab piloto=NotAdmitted completo=Admitted mismo-escaneo=True` | `PushBackFullReadPathTests` | true |
| 03 | **Las cinco relecturas** usan `ReadIntentAtOrigin` con `draft.CurrentDraftState.Origin` cuando el lector la ofrece (`PushBackOriginReaderDouble`); `Begin` usa `ReadIntent` con `ChosenReference` | `Begin lector=ReadIntent`; y, por cada una de `CheckNow`, `Reload`, `ConfirmBase`, `KeepMine` y `DiscardAndReload`, `<operación> lector=ReadIntentAtOrigin origen-del-borrador=True` | `PushBackFullReadPathTests` | true |
| 04 | **La elección por texto y la relectura, puras:** `PushBackOriginByText.Choose(DraftStaleBase, string)` sobre `PushBackPilotIntentReader.BuildStaleBase` del escaneo, como UR:645-649; `PushBackOriginByText.AtKey`; y `PushBackOriginByText.ReadAt` (B-08) | `choose uno-mutable=<clave>`; `choose ninguno=null`; `choose varios=null`; `choose no-mutable=null`; `atkey clave=<clave> resto-igual=True`; `readat otro-origen clave=<clave> piloto=<estado> completo=<estado> mismo-escaneo=True`; `readat mismo-origen=null`; `readat sin-coincidencia=null` | `PushBackFullReadPathTests` | true |
| 05 | Con un lector sin la interfaz nueva, las cinco relecturas usan `ReadIntent` como hoy | Por cada operación, `<operación> lector=ReadIntent` | `PushBackFullReadGuardTests` | **false** |
| 06 | El censo: solo `Draft/`, PP, UR y RIA nombran los cinco tipos de CT-PBT3A-12. **Sustituye a CT-PBT3A-12** | Las de CT-PBT3A-12, iguales: `sin-conectar archivos-revisados>0=True coincidencias=[]` y las diez `autoprueba-<nombre>-en-<destino>=falla` (los cinco nombres en `WorkspaceHostCore.cs` y en `WorkspacePilotSupport.cs`) | `PushBackFullReadGuardTests` | **false** |
| 07 | **La guarda del Plugin, en texto,** con autoprueba en negativo sobre texto inyectado en memoria | `ria-firma-de-Read-sin-cambio=True`; `ria-Read completo=1 tras-el-piloto=True`; `ria-lectores piloto=1 completo=1 escaneos=1`; `ria-AtOrigin readat=1 piloto=0 completo=0 base=0 lecturas-del-dibujo=0 literales=0`; `nh-lector interfaces=IWorkspaceEditIntentReader,IWorkspaceEditIntentOriginReader`; `nh-ReadIntentAtOrigin ReadOnActiveDocument=1`; `nh-AtOrigin=1 dentro-del-bloqueo=True tras-Read=True`; `nh-resultado FromReads=1 FromPilot=0`; `autoprueba-sin-lector-completo=falla`; `autoprueba-AtOrigin-con-CaptureDrawing=falla`; `autoprueba-segundo-AtOrigin=falla`; `autoprueba-FromPilot-en-NH=falla`; `autoprueba-segunda-lectura-del-piloto-en-RIA=falla` | `PushBackFullReadPathTests` | true |
| **08** | **I-24:** G-L1 y la pureza sobre el archivo nuevo de `Draft/` de PB-T4a, `PushBackOriginByText.cs` (§5.0) | **32:** `g-l1 archivos=1 reglas-sin-coincidencias=True`; las 15 `autoprueba-<a, b, c, e, f1, f2, f3, g1, g2, h1, h2, i, j, k1, k2>=falla`; `pureza archivos=1 violaciones=[]`; `autoprueba-#nullable=falla`; `autoprueba-namespace RackCad.Application.Systems.PushBack.Draft=falla`; y las 13 `autoprueba-<término>=falla` de `ForbiddenTerms` | `PushBackFullReadGuardTests` | **false** |

- **En CT-07,** `literales=0` y `lecturas-del-dibujo=0` son la restricción de CAG (D-PBT4-02): el cuerpo de `AtOrigin` no tiene ningún literal de cadena, ni `CaptureDrawing`, `LockDocument`, `Transaction`, `AssertNoOpenTransaction` o `ex.Message`. `ria-firma-de-Read-sin-cambio` compara la firma con la expresión de EIG:139-141. **`ria-lectores` es la cuenta de EIG:356-357 en todo RIA** (B-08): un `PushBackPilotIntentReader.Read(`, un `PushBackFullDraftReader.Read(` y un `new PushBackPilotIntentScan(`; `autoprueba-segunda-lectura-del-piloto-en-RIA` inyecta una segunda lectura del piloto en el texto de RIA.
- **CT-08 pasa en el RED:** el esqueleto ya crea `PushBackOriginByText.cs` (con `Choose` que devuelve null, `AtKey` que devuelve el escaneo y `ReadAt` que devuelve null), puro y sin ninguna ficha de G-L1. En el GREEN, el archivo llama a los dos lectores, como `PushBackFullDraftReader.cs`, y sigue sin ninguna ficha ni ningún término de la pureza (medido). Sus autopruebas fallan sobre el texto inyectado.

**Las clases:** `PushBackFullReadPathTests` (01 a 04 y 07), con `ExpectRed` true; `PushBackFullReadGuardTests` (05, 06 y 08), con false. Las dos y `PushBackFullReadSupport.cs`, en `tests/RackCad.Tests/`, con `namespace RackCad.Tests`; ningún filtro heredado las selecciona por prefijo.

**Los totales** (F4B-E46: una sola rama, porque PB-T4a no quita ningún `Skip` ni añade ninguna `Theory`). La base: el cierre de PB-T3c (CI `44757675821` sobre `938a35f0`), **Core 19 129 = 19 123 `Passed` + 6 `Skipped`**, y UI 2 451 = 2 430 + 21. PB-T4a añade **8 CT, ocho `[Fact]`,** y pone **un `Skip`** (CT-PBT3A-12, un `[Fact]`):

| Rama | Core | UI |
|---|---|---|
| **RED** | **19 137 = 19 125 `Passed` + 5 `Failed` (CT-01 a 04 y 07) + 7 `Skipped`** | 2 451 = 2 430 + 21, sin cambio |
| **GREEN** | **19 137 = 19 130 `Passed` + 0 `Failed` + 7 `Skipped`** | 2 451 = 2 430 + 21 |

- 19 125 = 19 123 − 1 (el `Skip`) + 3 (CT-05, 06 y 08, que pasan en el RED).
- **La suite de Core sobre la copia con el esqueleto** (el parche de `src/` y el `Skip`, sin las clases nuevas; precondición 6): **19 129 = 19 122 + 7**. Sin el `Skip` daría 19 129 = 19 123 + 6, y CT-PBT3A-12 pasaría, porque RIA no cambia en el esqueleto.
- **D = 0** (I-25): ninguna otra prueba falla. **En el RED,** se confirma en la precondición 6, que corre la suite completa sobre la copia con el esqueleto (19 129 = 19 122 + 7). **En el GREEN, la precondición 6 no lo confirma,** porque no hay GREEN que compilar: lo sostiene el recorrido entero de §4.1 sobre el texto literal del GREEN, prueba a prueba, de las clases que leen los archivos tocados. Lo que ese recorrido no puede ver (una prueba que lea el texto por un camino que no deja ninguna ruta, carpeta ni ayudante en su código, o la reflexión sobre tipos fuera de los que se miraron) es un riesgo declarado: si una prueba fuera de la lista falla en el GREEN, es PBT4A-C16 y vuelve al Owner.
- **El arreglo de B-08 no cambia los totales:** `ReadAt` es un método de un archivo que ya existía en el esqueleto, y CT-04 y CT-07 siguen siendo un `[Fact]` cada una. Con el texto de la v4.1, el GREEN habría dado 19 129 `Passed`, 1 `Failed` (EIG:350-364) y 7 `Skipped`.

### 5.2 PB-T4b: la lista cerrada CT-PBT4B-01 a 20

Las CT-01 a 17 de la v3 y la CT-18 de la v4, con lo que fijan las decisiones del Owner:
- **CT-03:** el campo valida con `PushBackAnnotationScaleRule` sin asignar nada.
- **CT-04 (UI):** `PushBackAnnotationScaleRule` frente a `AnnotationScaleBox` de una ventana real (parámetros y validación en los diez textos de CT-PBT2B-10).
- **CT-06:** la presentación «Por grupos» (D-PBT4-09), con las líneas medidas antes de emitir sobre el clasificador real.
- **CT-13:** el censo pasa a admitir WHC, y sustituye a CT-PBT4A-06.
- **CT-16:** WHC crea el adaptador con `PushBackAnnotationScaleRule.Validate` y WCo no asigna ninguna regla.
- **CT-18 (UI):** `NumericFieldValidation.Validate` y `NumericTextRule.Check` dan el mismo resultado (estado, valor y mensaje) en una tabla fija de textos y parámetros. `ExpectRed` true: en el RED, la regla de Application es inerte.

**Y dos nuevas:**

| CT | Qué fija | Las líneas | Clase | `ExpectRed` |
|---|---|---|---|---|
| **19** | **I-24:** G-L1 y la pureza sobre los tres archivos nuevos de `Draft/` de PB-T4b: `PushBackFullFieldAdapter.cs`, `PushBackKeepMinePresentation.cs` y `PushBackAnnotationScaleRule.cs` (§5.0). **Cubre `Apply`:** una línea dice en cuál de los tres se declara | **33:** `g-l1 archivos=3 reglas-sin-coincidencias=True`; las 15 autopruebas de G-L1; `pureza archivos=3 violaciones=[]`; las 15 de la pureza; y `cubre Apply=PushBackFullFieldAdapter.cs` | `PushBackPanelDraftGuardTests` (Core) | **false** |
| **20** | **El guion de «Saltar y duplicar»** (D-PBT4-11): para cada una de las 9 clases, el original igual a la base salvo los `Skip`, cada prueba con su copia o su fila «a mano», y la copia igual al original con el mapa M | Una por clase, `copia <Clase> original-igual-a-la-base=True pruebas=<n> copiadas=<c> a-mano=<h> copia-igual-al-mapa=True`, con los números de `pbt4-copy-map.tsv`; y `autoprueba-linea-de-mas=falla`, `autoprueba-asercion-cambiada=falla`, `autoprueba-skip-de-mas=falla` y `autoprueba-sin-copia-ni-fila=falla` | `WorkspaceFullCopyGuardTests` (Core) | **false** |

**CT-19 y CT-20 pasan en el RED:** el esqueleto de PB-T4b crea los tres archivos de `Draft/` (inertes y puros), y las copias, el kit nuevo y los `Skip` van en el commit RED. **El esqueleto del RED de PB-T4b no toca `NumericFieldValidation.cs`** (M-29): la delegación en `NumericTextRule` es del GREEN; en el RED, `NumericTextRule.Check` existe y es inerte, y la caja de la UI sigue con su lógica de hoy.

**Las copias, en su propio espacio de nombres:** `namespace RackCad.Tests.FullPanelCopies;`, para que el filtro heredado `RackCad.Tests.Workspace` (que el RED tiene que pasar, INV-PBT3A-02) no las seleccione; CT-20 sí está en `RackCad.Tests` y pasa en el RED.

**El RED de las copias, partido** (M-34; la T-1 de la §16.4 de la revisión, técnica): en el RED, el anfitrión todavía es del piloto, así que unas copias fallan (las que buscan el borrador completo) y otras pasan (las del ciclo de vida o de los avisos), y un filtro solo admite que fallen todas (`ExpectRed` true) o que pasen todas. Por eso cada copia se parte en `<Clase>FullRedTests` y `<Clase>FullGreenTests` (D-PBT4-11, M-00), con una partición cerrada que la precondición 6 mide sobre la copia exportada con el esqueleto, las copias y el kit nuevo. **No es la «implementación de referencia» de la pregunta 3:** aquella era escribir el código de `src/` para medir la lista; el kit nuevo es código de prueba que D-PBT4-11.3 ya fija (los mismos miembros y firmas, y `FromReads` sobre el mismo escaneo), y va literal en el contrato, como cualquier CT. En los `RequiredTests`, `RackCad.Tests.FullPanelCopies.` con el sufijo `FullRedTests` va con `ExpectRed` true, y con `FullGreenTests`, con false.

**Los totales de PB-T4b** salen de la precondición 6, con la lista que aprobó el Owner (470; `pbt4b-substitution-list.tsv`) y la partición, y el contrato los fija por rama (F4B-E46).

### 5.3 PB-T4c: la lista cerrada CT-PBT4C-01 a 15

Las CT-01 a 13 de la v3 y la CT-14 de la v4, con estos cambios:
- **CT-07:** los seis sujetos de D-PBT4-16.
- **CT-08 (M-30):** las dos piezas del abridor, en texto:
  - `FullEditorModal` en WCo, con un `using (_controller.EnterPanelModal())` y la llamada a `_open`, sin `RackCad.UI` ni `PushBack`;
  - `RackWorkspaceEditorWindow.Show(object launch)`, con **esta línea literal, una vez:** `var window = RackPushBackSystemWindow.ForDraft((PushBackDraftEditorLaunch)launch);`; después `AcApplication.ShowModalWindow(window);`, una vez; sin `Owner =` ni `RackCad.Application.Workspace`;
  - y el cableado en WCo, `_host.SetFullEditorHost(new FullEditorModal(this, RackWorkspaceEditorWindow.Show))`, después de `SetPilotUpdateHost`.
  - Sus líneas llevan el literal de la conversión: `abridor conversion=[var window = RackPushBackSystemWindow.ForDraft((PushBackDraftEditorLaunch)launch);] veces=1`.
- **CT-10 (Core):** puerto real (`WorkspacePilotEditGate` sobre WHC) → `TryEditAll` → el doble de `IWorkspaceFullEditorHost` aplica por el puerto (incluido un gesto compuesto, F4B-E30) → «Actualizar» con `PilotEraseScopeDouble` → `Applied` y Clean.
- **CT-14 (UI; I-20):** la FVM real, sobre un doble nuevo del puerto que registra: `EditAll` llama a `CommitField` si hay texto y después a `TryEditAll`, con la petición de D-PBT4-12.

**Y una nueva:**

| CT | Qué fija | Las líneas | Clase | `ExpectRed` |
|---|---|---|---|---|
| **15** | **I-24:** G-L1 y la pureza sobre los dos archivos nuevos de `Draft/` de PB-T4c: `PushBackFullDraftLaunch.cs` y `PushBackFullDraftStatePort.cs` (§5.0). **Cubre `ApplyToDraft`** | **33:** `g-l1 archivos=2 reglas-sin-coincidencias=True`; las 15 autopruebas de G-L1; `pureza archivos=2 violaciones=[]`; las 15 de la pureza; y `cubre ApplyToDraft=PushBackFullDraftStatePort.cs` | `PushBackDraftLaunchGuardTests` (Core) | **false** |

CT-15 pasa en el RED: el esqueleto de PB-T4c crea los dos archivos (inertes y puros).

**Los totales:** K_C = 2 (PEG:155-163 y CT-PBT1B-15, dos `[Fact]`), con la base C = el cierre de PB-T4b:

| Rama | Core | UI |
|---|---|---|
| **RED** | (C + 11 + 2 − D_C) = (C_P − 2 + 4) `Passed` + 9 `Failed` + (C_S + 2) `Skipped` | + 4, con 4 `Failed` |
| **GREEN** | El mismo total, con 0 `Failed` | + 4, con 0 `Failed` |

- En el RED pasan CT-07, CT-15 y los dos equivalentes.
- D_C = 0. Se confirma en la precondición 6.

### 5.4 La CI del RED

`tests` en failure, `build-ui` en success y `build-plugin` omitido. `ui-tests`: en failure en PB-T4b y PB-T4c; en success en PB-T4a, que no añade pruebas de UI.

### 5.5 Lo que queda para el Smoke-PB (PB-T5, D-PB-09; I-10; M-27)

La lista de la v3:
- el botón abre la ventana modal real;
- el dibujo no se edita con ella abierta (OB-01.4);
- el borrado físico (PB-T3c);
- A2-11 hasta el DWG;
- el coste;
- «Restaurar»;
- OWNER-2;
- **la lectura completa en AutoCAD** (PB-T4a): un rack A/B se abre en el panel (OB-03.2);
- **la relectura con el origen tras borrar la vista de origen** (A-T3-8 con POST reanclado), y su coste: la relectura pura de `AtOrigin` repite las dos lecturas sobre el escaneo ya tomado;
- **la regla única cableada:** los mensajes del campo del panel y de la caja son iguales en AutoCAD;
- **el abridor cableado:** la ventana se abre desde la paleta, sin dueño, con el modal del panel, y nada se escribe tras cerrarla;
- **el paso 3 con un cambio real del dibujo** entre la presentación y el «Sí»;
- **K = 1 con el modal real:** la paleta deshabilitada.

Más los riesgos del borrado real de PB-T3c (F4B-E48, `f4b-dispositions.md:1609`):
- OWNER-1: las referencias de la vista en espacio papel o dentro de bloques del usuario se borran sin aviso;
- OWNER-2: la purga de las anidadas es de un nivel;
- OWNER-3: los bloques dinámicos.

Se observan con «Editar todo» → «Actualizar» sobre racks reales, y junto a la línea «Vistas: se borrará…» de la presentación. Los racks A/B fabricados se rotulan «CONSTRUCTED REPRESENTATIVE TEST CASES».

## 6. Los contratos propuestos

### 6.0 Común (entero; V41-12)

**Identidad:**
- `Schema` `rackcad-gate-contract/v1`; `Unit` I-64; `Gate` F4B;
- `attempts`: cada subtarea empieza en 1 de 3 (OB-10.1); `RoutingEnforcement` `required`; `CorrectionsAuthorized` true;
- evidencia en `docs/automation/evidence/I-64-pilot/<TaskId>/`.

**Los `TaskId`:** con el prefijo **`F4B-PBT4-`** y **como mucho 26 caracteres** (V41-11; MAX_PATH solo acota por arriba): `F4B-PBT4-FULL-READ-PATH` (23), `F4B-PBT4-FULL-DRAFT-PANEL` (25) y `F4B-PBT4-EDIT-ALL-WINDOW` (24). El prefijo es el que liga `--task PBT4` en los sustitutos de nc1 y nc2 (`nc1_cascade_signal_substitute.py:205`; `nc2_exact_whitelist_substitute_f4b_v2.py:410`), así que se usan sin editarlos, con `--task PBT4` en las tres subtareas.

**Las colecciones opacas** (F4B-E21): las ocho (`Authorities`, `Objective`, `AllowedWriteScope`, `ForbiddenWriteScope`, `Invariants`, `RequiredTests`, `ExpectedEvidence` y `StopConditions`) las pone la sesión byte a byte desde el contrato. El Controller hace la clase, el enrutamiento y los criterios de aceptación. Cada relevo lo declara. La comparación byte a byte, A1-A8 y nc4 van sobre la delegación final.

**`ExpectedEvidence[0]`** es, **byte a byte**, el literal de PB-T3 de la lista cerrada del sustituto de nc2 (el `ExpectedEvidence[0]` del contrato de PB-T3c):

> antes del Worker: el contrato, la salida del Controller (output.json, con las ocho colecciones opacas vacias), la delegacion final con esas colecciones puestas por la sesion (delegation.json; F4B-E21), la comparacion byte a byte sobre la delegacion final, A1-A8 y nc4;

Si no, hay que añadir un literal al sustituto y volver a revisarlo (F4B-E43).

**El tamaño** (F4B-E21): el contrato de PB-T3c mide 54 162 bytes. Cada contrato de PB-T4 lleva solo sus CT y sus listas. La de PB-T4b va en un archivo custodiado que el contrato cita por su sha256 (`pbt4b-substitution-list.tsv`, `44393b19…63e4`; D-PBT4-11), porque como tabla dentro del contrato pasaría de unos 60 KB.

**Las rutas `UNIT_CHANGE` y F4B-E25:** cada contrato declara `UNIT_CHANGE` las rutas **existentes** de producto que cambia:
- **PB-T4a:** WEI, WHC, RIA y NH;
- **PB-T4b:** WHC, WEI, `WorkspacePilotSupport.cs`, FVM, PV, WCo y `src/RackCad.UI/Controls/NumericFieldValidation.cs`. **Ningún archivo de `Pilot/`:** la retirada es de PB-T4d;
- **PB-T4c:** WHC, `IWorkspacePilotEditPort.cs`, `WorkspacePilotSupport.cs`, WEI, FVM, PV, WCo y W;
- **PB-T4d:** los archivos de `Pilot/` que retira (§6.4), con su contrato.

**En una corrección** (F4B-E25, `F4B-DRAFTS/coordinator/owner-f4b-correction-unit-change-decision.md`), una ruta `UNIT_CHANGE` que aparezca en `git diff --name-only AuthorityRevision BaseSha` no incumple 16.3 si se cumplen las tres condiciones del Owner, que el Controller comprueba con git:
1. todo commit de `AuthorityRevision..BaseSha` que la toca es un `RedSha` o un `CurrentSha` de una entrega de un Worker de la misma cadena (el mismo `TaskId`), según su `worker-handoff.json`;
2. esa ruta está en el `AllowedWriteScope` de la delegación de esa entrega;
3. ningún commit de la sesión de `AuthorityRevision..BaseSha` toca esa ruta.

Las rutas `EXTERNAL` siguen prohibidas en ese diff, sin excepción. F4B-E25 va entre las autoridades de cada contrato.

**`ForbiddenWriteScope`,** lo calcula el generador con el método de [PKC-C], como en PB-T3:
- todo `src/` salvo el `AllowedWriteScope` de la subtarea;
- en `tests/`: las listas curadas de PB-T1, las clases de PB-T1, PB-T2 y PB-T3, `tests/RackCad.Tests/Fixtures/I64/`, `tests/RackCad.UI.Tests/Workspace/` y el prefijo `tests/RackCad.Tests/Workspace/`;
- **salvo** las rutas de `tests/` del `AllowedWriteScope`, que se sacan de la lista (en PB-T4a, `PushBackDraftWriteGuardTests.cs`, una clase de PB-T3, por el `Skip`). Si una de ellas cae dentro de un prefijo prohibido, el prefijo se sustituye por la lista explícita de sus archivos menos esa, como en PB-T3b;
- las 17 entradas de la raíz.

**La comprobación antes de emitir** (precondición 6; la lección de F4B-E45): la sesión compila sobre una copia exportada el esqueleto exacto del RED, ejecuta la suite completa de Core y los filtros de UI de la subtarea, barre los nombres y las sobrecargas, y mide las guardas sobre el texto literal del GREEN del paquete. **El GREEN de referencia no existe:** el paquete prescribe el texto literal del GREEN donde una guarda lo lee, y la sesión lo aplica en memoria, solo para las guardas de texto.

**Las paradas estándar** (17), copiadas de [PKC2] §6.7 como en el contrato de PB-T3c, con P-01 en la Snapshot K (F4B-E37):

- **S-02:** `architecture outside Freeze`
- **S-03:** `material ambiguity`
- **S-04:** `contradictory evidence`
- **S-05:** `destructive or irreversible action not authorized`
- **S-07:** `conflict with another active owner`
- **S-08:** `branch or worktree ownership conflict`
- **S-12:** `Worker loses context or cannot verify AuthorityRevision`
- **S-13:** `origin/main movement invalidates exact-SHA assumptions; apply AUTOMATION_PLAN 16.7`
- **S-14:** `I-64 Freeze or contract explicitly says STOP`
- **P-01:** `config.toml no longer matches the accepted Snapshot in force (today Snapshot K, docs/automation/evidence/I-64-pilot/F4-ENVIRONMENT/snapshot-K/snapshot-k.json)`
- **P-02:** `foreign live participant, unattributable process or more than one open delegation`
- **P-03:** `delegation outside gate contract`
- **P-04:** `BLOCKED cap reached`
- **P-05:** `invocation differs from accepted delegation package`
- **P-06:** `usage or credit-limit warning`
- **P-07:** `another invocation would exceed the invocation budget`
- **P-08:** `responsible session operates the worktree during cession`

**Las de [DP], literales, en cada contrato** (x = A, B o C, con el prefijo `PBT4x`):
- **PBT4x-C08:** `a planning output rejected ONLY by the literal byte-fidelity comparison of the collections before the Worker allows ONE replanning with a reinforced prompt, without consuming an attempt ([DP] question 2); a second planning fidelity rejection is STOP and returns to the Owner; any other planning rejection (A1-A8 or another cause) is STOP and returns to the Owner`
- **PBT4x-C12:** `a correction would be attempt 3 of 3: it returns to the Owner before it is issued (D-PB-10)`
- **PBT4x-C11, con los sustitutos de nc2 y nc1 vigentes:** el texto de `PBT3C-C11` del contrato de PB-T3c con `PBT3C` cambiado por `PBT4` (el `--task` de nc1 y nc2 es `PBT4` en las tres subtareas, y sus registros, `SATISFIED_UNDER_PBT4_…`) y «PB-T3c» por el nombre de la subtarea. El de PB-T4a, entero, en §6.1.1. nc3 no tiene sustituto (F4B-E46).

**El límite de D2, en cada contrato:** **PBT4x-C17,** con el texto literal del Owner (el de `PBT3C-C17`):

> (F4B-E11, D2, IMPORTANT LIMIT) doing the task would need to MUTATE the resolved rack (PushBackSystem) or the module report (RackModuleReconciliationResult) inside the editing or draft lifetime, or to share by reference any other mutable object between a copy of the state and its original, or a measurement already finds such a mutation in the editing path: STOP and L-1 is reopened. The Owner text, literal: "This is NOT authorization for general shallow copying. It is exactly: two declared shared references. If later PB-T1/PB-T2/PB-T3 needs to mutate either: resolved rack; module report; inside the Workspace draft lifetime: STOP and reopen L-1. At that point the object must either: be cloned; be made immutable; or be replaced by a value snapshot appropriate to the draft model." The Worker chooses none of the three exits: the Owner does

El Worker no elige ninguna salida.

**La reparación del RED** (F4B-E35, pregunta 4, la regla RR del Owner; como en [P3] §6.0): un error de escritura en una prueba nueva de la tarea, visto después del commit RED, se repara sin gastar intento (RR-7) si se cumple todo lo que sigue. La regla excepciona AUTOMATION_PLAN 16.8 («tests/ no se toca tras el RED») solo en estas condiciones:
- **RR-1, cuándo:** después del commit RED y su push, y antes del commit GREEN; como mucho una reparación por intento. Un segundo error de escritura en el mismo intento es la parada de siempre (C07).
- **RR-2, qué toca:** solo archivos de `tests/` que el commit RED de la tarea añadió; nunca una prueba existente, ni `src/` (tampoco el esqueleto inerte del RED), ni un fixture, ni nada fuera del `AllowedWriteScope`. **En PB-T4a,** `PushBackDraftWriteGuardTests.cs` no lo añadió el RED, sino que lo cambió (el `Skip`): una reparación no lo toca, y un error en el `Skip` o en su motivo literal es la parada de siempre. **En PB-T4b,** lo mismo para las 9 clases con sus `Skip`.
- **RR-3, sin cambiar lo esperado:** ninguna línea esperada, literal ni oráculo del paquete o del contrato cambia; la reparación corrige solo el cuándo y el cómo de una lectura o de un gesto que el paquete ya prescribe. Si para que la prueba pase hace falta cambiar una línea esperada, es C07.
- **RR-4, el RED repetido entero:** sobre `RedRepairSha` se vuelve a pasar el criterio completo del RED del contrato (INV-PBT4x-02); el commit se empuja y su CI se registra como la del RED reparado.
- **RR-5, declarado:** el commit se titula «RED repair» y nombra la tarea; la entrega declara `RedRepairSha`, la prueba y la línea corregidas, el error y por qué no cambia ninguna línea esperada, con el diff; `RedSha` sigue siendo el commit RED original.
- **RR-6, la cadena:** `BaseSha..RedSha`, el RED de cada contrato; `RedSha..RedRepairSha`, solo archivos que el RED añadió; `RedRepairSha..CurrentSha`, el GREEN de cada contrato. Sin reparación, la cadena es la de siempre.
- **Las dos lecturas de PB-T4, porque los tres RED tocan `src/`** (el esqueleto) y `tests/` existentes (los `Skip`): RR-1, «antes del primer commit que toque `src/`», se lee «antes del commit GREEN»; el primer tramo de RR-6, «BaseSha..RedSha, solo `tests/`», se lee «el RED de cada contrato».

**La verificación con una reparación del RED** (F4B-E35, RR-7):
- **n.º 10 (`Tests`):** el RED se acredita con `RedSha` y con la CI del RED original, y sus TRX, con los filtros con `ExpectRed` entre los fallidos. La cláusula «`git diff --name-only RedSha CurrentSha` no toca `RT` ∪ `ChainRedFiles`» se comprueba desde `RedRepairSha`.
- **n.º 11 (`Trailer`):** el trailer va también en el commit «RED repair».
- **Lo que el Controller comprueba de RR** (si algo falla, es C18): RR-1, RR-2, RR-4, RR-5 y RR-6, como en PB-T3c. **Por subtarea, además,** el tramo `RedSha..RedRepairSha` no toca: en PB-T4a, ningún archivo de `src/` ni `PushBackDraftWriteGuardTests.cs`; en PB-T4b, ningún archivo de `src/` ni las 9 clases; en PB-T4c, ningún archivo de `src/` ni PEG ni `PushBackSharedReferenceGuardTests.cs`.
- **La ayuda mecánica de RR-3:** es parada C07 que un literal de la lista cerrada de la clase reparada (la tabla de abajo) cambie entre `RedSha` y `RedRepairSha`: una línea o un fragmento, si no aparece como subcadena exacta el mismo número de veces; un valor de gesto, si no aparece como token el mismo número de veces.
- **Indicios para la revisión, no parada:** una cadena `<campo>=` añadida o quitada en `git diff -U0 RedSha RedRepairSha`, o un cambio en el primer argumento de un `Assert.Equal`.

**La lista cerrada de la ayuda de RR-3, por clase.** «Línea» es una línea esperada o un fragmento (se compara como subcadena); «valor», un valor de gesto (como token):

| Subtarea | Clase | Literales fijados por el paquete |
|---|---|---|
| PB-T4a | `PushBackFullReadPathTests` (CT-01 a 04 y 07) | **Líneas:** las de CT-01 a 04 y 07 de §5.1, literales; de CT-02, los fragmentos ` piloto=`, ` completo=` y ` mismo-escaneo=True`, y la línea `pb-20-ab piloto=NotAdmitted completo=Admitted mismo-escaneo=True`; de CT-03, ` lector=ReadIntentAtOrigin origen-del-borrador=True` y `Begin lector=ReadIntent` |
| PB-T4a | `PushBackFullReadGuardTests` (CT-05, 06 y 08) | **Líneas:** de CT-05, ` lector=ReadIntent`; de CT-06, los cinco nombres (`PushBackFullDraftState`, `PushBackFullDraftReader`, `PushBackFullComposition`, `PushBackConflictClassifier` y `PushBackViewDispositions`) y los dos destinos (`WorkspaceHostCore.cs` y `WorkspacePilotSupport.cs`); de CT-08, `g-l1 archivos=1 reglas-sin-coincidencias=True`, `pureza archivos=1 violaciones=[]`, las ocho fichas de G-L1 (`WorkingBaseline`, `workingBaseline`, `LastModuleReconciliation`, `computation.System`, `lastComputation.System`, `session.System`, `lastCompositeSystem` y `SystemToInsert`) y los 15 términos de la pureza (`#nullable`, `namespace RackCad.Application.Systems.PushBack.Draft`, `Autodesk`, `RackCad.Plugin`, `RackCad.UI`, `System.Windows`, `Transaction`, `Database`, `async`, `await`, `Task`, `File.`, `System.IO`, `RackEmbedComposer` y `new RackEmbedDocument`) |
| PB-T4a | `PushBackFullReadSupport` (el kit y el doble) | **Líneas:** `ReadIntentAtOrigin` y `FromReads` |
| PB-T4b | `PushBackPanelDraftGuardTests` (CT-19) | **Líneas:** `g-l1 archivos=3 reglas-sin-coincidencias=True`, `pureza archivos=3 violaciones=[]`, `cubre Apply=PushBackFullFieldAdapter.cs`, y las fichas y los términos de la fila de CT-PBT4A-08 |
| PB-T4b | `WorkspaceFullCopyGuardTests` (CT-20) | **Líneas:** ` original-igual-a-la-base=True`, ` copia-igual-al-mapa=True`, los nueve nombres de clase, las ocho parejas de M-01 a M-08 y los sha256 de los originales en `BaseSha` |
| PB-T4c | `PushBackDraftLaunchGuardTests` (CT-15) | **Líneas:** `g-l1 archivos=2 reglas-sin-coincidencias=True`, `pureza archivos=2 violaciones=[]`, `cubre ApplyToDraft=PushBackFullDraftStatePort.cs`, y las fichas y los términos de la fila de CT-PBT4A-08 |
| PB-T4c | La de CT-08 | **Líneas:** `var window = RackPushBackSystemWindow.ForDraft((PushBackDraftEditorLaunch)launch);` y `AcApplication.ShowModalWindow(window);` |

Las filas de las demás clases de PB-T4b y PB-T4c, con sus líneas medidas, van en sus contratos.

**La lectura de `Ci` en el RED:** Core falla a propósito en el RED, y `build-plugin` tiene `needs: [tests, build-ui]` (`.github/workflows/ci.yml`), así que sale **omitido**. `ExpectedEvidence` lo pide así: `tests` en `failure`, `build-ui` en `success`, `build-plugin` omitido y `ui-tests` en `success` (PB-T4a) o en `failure` (PB-T4b y PB-T4c), sin cuelgue. **El total exacto de cada rama** (F4B-E46). En el GREEN, 4/4 en `success`.

**El `Trailer`:** el de `worker-handoff.json` es el trailer real de los commits RED, «RED repair» (si lo hay) y GREEN, nunca `null` si los commits lo llevan.

**`EligibleCells`:** las del contrato de PB-T3c: Worker `anthropic/subagent/claude-sonnet-5-5`, effort `high`; Controller `openai/process/gpt-6-luna`, effort `high`; vencen el **2026-12-29**; nc4 en la primera delegación de cada cadena; nc1 a nc3 tras la verificación.

**Invariantes comunes** (cada contrato los numera INV-PBT4x-07 a 14):
- **07. Las redes:** §5 en verde. Ninguna prueba existente se edita, se salta, se reactiva ni se borra, salvo las de la lista cerrada de sustituciones de la subtarea, con su `Skip` de motivo literal (PB-T4a: solo CT-PBT3A-12). En `CurrentSha`, el `git diff -U0 BaseSha..CurrentSha` de los archivos de pruebas existentes solo lleva las líneas de atributo con `Skip`.
- **08. Las pruebas de UI:** las reglas de §5.0.
- **09. Determinismo:** GUID fijo, cultura invariante, «R» y orden ordinal; ni hora ni aleatoriedad.
- **10. Las corridas locales:** solo los filtros del contrato en UI; nunca la suite completa de UI en local; la de Core, sí; sin copias de árboles; las corridas temporales terminan en `Assert.Fail(string.Join("\n", lineas))`, corren dentro de `EditorDiscardPrompt.Substitute` cuando abren ventanas, no entran en ningún commit y guardan TRX y log como artefactos.
- **11. La evidencia:** rutas relativas comprobadas en `worker-handoff.json`; `TreeSha` real o null; el `Trailer`, el real; el Worker no escribe en `docs/`.
- **12.** Las reglas de F4B; sin AutoCAD; la verificación con las tres comprobaciones deterministas de F4B-E15.
- **13. Las declaraciones nuevas en `src/`:** las de la lista cerrada de la parada C13 de cada contrato, y ninguna más, por nombre y acotada a los archivos que tocan, sobrecargas incluidas. En los archivos nuevos se admiten además métodos privados, tipos anidados privados sin estado y campos `private static readonly` de un tipo inmutable (`Regex` o `string`).
- **14. D2:** fuera de las dos referencias de cada lado, ninguna copia comparte un objeto mutable con su original. Ninguna línea añadida muta un `PushBackSystem` ni un `RackModuleReconciliationResult` adoptado.

**Las propias comunes** (ASCII, con el prefijo de cada contrato; las de PB-T4a, enteras, en §6.1.1):
- **C01:** `doing the task would require writing outside the AllowedWriteScope.`
- **C02:** editar, saltar, reactivar o borrar una prueba o un ayudante existente fuera de la lista cerrada de la subtarea; o tocar `InternalsVisibleTo`, los paquetes, un `.csproj` o `Fixtures/I64`.
- **C03:** un cambio observable de RACKEDITAR, o del panel fuera de lo declarado por la subtarea. STOP y vuelve al Owner.
- **C04:** `a method of a filter with ExpectRed passes on the skeleton, or fails by an exception and not by an assertion.`
- **C05:** `a CT would need AutoCAD, a real MessageBox, the real configurator, the BOM, "Guardar en biblioteca", "Insertar", real keyboard or focus, or writing to the profile or to disk.`
- **C06:** `the GREEN CI fails outside the new tests, reproducibly and not attributable to them.`
- **C07:** el texto de PB-T3c, con las recetas que se autocomprueban de la subtarea, y la excepción de RR.
- **C09:** `a test would hang, leave a window, a modal or a dispatcher alive, write to the profile or depend on machine paths.`
- **C10:** lo que la subtarea excluye (D-PBT4-19 y su alcance).
- **C13:** la lista cerrada de declaraciones nuevas de la subtarea.
- **C14:** un diseño del corpus cambia de admisión o de bloqueo.
- **C16:** una prueba existente fuera de la lista cerrada de la subtarea falla. Vuelve al Owner (pregunta 4).
- **C17:** D2 (arriba).
- **C18:** `a RED repair does not meet the F4B-E35 rule (RR-1 to RR-6): it is not after the RED push and before the GREEN, or it is the second one in the attempt; it touches a file the RED did not add, an existing test, src/, a fixture or anything outside the AllowedWriteScope; it changes an expected line, literal or oracle, or anything other than the when or the how of a reading or a gesture the package already prescribes (RR-3); the full RED criterion and its CI are not rerun on RedRepairSha; or it is not declared as RR-5 says. The rule does not apply and the repair counts as touching tests/ after the RED (AUTOMATION_PLAN 16.8): STOP.`

### 6.1 PB-T4a — `F4B-PBT4-FULL-READ-PATH` (entero; V41-12)

- **Objective** (ASCII): el de §6.1.1.
- **`AllowedWriteScope`** (10):
  - `src/RackCad.Application/Workspace/WorkspaceEditIntent.cs` (WEI);
  - `src/RackCad.Application/Workspace/IWorkspaceEditIntentOriginReader.cs` (nuevo);
  - `src/RackCad.Application/Systems/PushBack/Draft/PushBackOriginByText.cs` (nuevo);
  - `src/RackCad.Application/Workspace/WorkspaceHostCore.cs` (WHC);
  - `src/RackCad.Plugin/Views/RackPilotIntentAdapter.cs` (RIA);
  - `src/RackCad.Plugin/Workspace/WorkspaceNavigationHost.cs` (NH);
  - `tests/RackCad.Tests/PushBackFullReadPathTests.cs`, `tests/RackCad.Tests/PushBackFullReadGuardTests.cs` y `tests/RackCad.Tests/PushBackFullReadSupport.cs` (nuevos);
  - **solo para el `Skip`:** `tests/RackCad.Tests/PushBackDraftWriteGuardTests.cs`.
- **`UNIT_CHANGE`:** WEI, WHC, RIA y NH (F4B-E25).
- **El RED:** las tres clases nuevas de `tests/`, el esqueleto exacto y el `Skip`. **El esqueleto exacto** (el parche, `pbt4a-skeleton.patch`, lleva `src/` y el `Skip`; la duda 6):
  - **WEI:** el constructor privado con los dos argumentos nuevos, `FullRead` y `Catalog` como en D-PBT4-02, las cuatro fábricas de fallo y `FromPilot` con `null, null`, y `FromReads` **inerte**: `public static EditIntentReadResult FromReads(PushBackPilotIntentResult pilotRead, PushBackFullIntentResult fullRead, RackCatalog catalog) => new EditIntentReadResult(EditIntentReadStatus.Read, null, pilotRead, null, null);` (sin comprobar nulos y con los dos campos en null);
  - **`IWorkspaceEditIntentOriginReader.cs`:** completo, el de D-PBT4-03;
  - **`Draft/PushBackOriginByText.cs`:** los `using`, el `namespace` de bloque y el comentario de D-PBT4-03, con `Choose` que devuelve `null` (`=> null;`), `AtKey` que devuelve `scan` (`=> scan;`) y `ReadAt` que devuelve `null` (`=> null;`), con sus firmas literales y su `using RackCad.Application.Catalogs;` (B-08);
  - **WHC, RIA y NH:** sin cambio;
  - **el `Skip`,** en `PushBackDraftWriteGuardTests.cs:60`, que pasa a ser esta línea, literal (ASCII, la única línea que cambia en el archivo):

    ```csharp
        [Fact(Skip = "I-64 F4B PB-T4a: sustituida por CT-PBT4A-06 de PushBackFullReadGuardTests (registro declarado superseding-characterization.md; decision del Owner owner-pbt4-decisions.md, pregunta 4).")]
    ```
- **El GREEN:** solo `src/`: WEI, `PushBackOriginByText.cs`, WHC, RIA y NH, con el texto literal de D-PBT4-02 y 03. `IWorkspaceEditIntentOriginReader.cs` no cambia desde el RED.
- **`RequiredTests`** (38; la tabla de §6.1.1).
- **Invariantes:** INV-PBT4A-01 a 06 (§6.1.1) y los comunes 07 a 14.
- **`StopConditions`:** las 17 estándar y PBT4A-C01 a C18 (§6.1.1): 34.
- **`ExpectedEvidence`** (15; §6.1.1), con la línea base y los totales exactos de la rama única de §5.1.
- **La línea base:** el cierre de PB-T3c (`7f0d9aa3`; la CI `44757675821` del GREEN `938a35f0`).

#### 6.1.1 Los textos literales del contrato de PB-T4a

Para la comparación del generador, en ASCII, campo a campo.

```text
TaskId: F4B-PBT4-FULL-READ-PATH

Objective:
Hacer que la lectura de la intencion de edicion del Workspace de Push Back traiga tambien el modelo completo de PB-T3 sobre el mismo escaneo: el adaptador del Plugin llama al lector completo tras el del piloto y el resultado de la lectura lleva la lectura completa y el catalogo junto a la del piloto (FromReads; FromPilot no cambia); y que las cinco relecturas de un borrador abierto vayan al origen del borrador por su texto (el unico miembro mutable con ese texto, elegido sobre el mismo escaneo, o el Primary de hoy si no hay uno) cuando el lector lo ofrece, con la eleccion en linea en cada cuerpo (A-T3-8). Es un cambio observable declarado: las relecturas dejan de ir al Primary. El tipo del borrador del panel no cambia, nada traduce estados, la firma de RackPilotIntentAdapter.Read no cambia y no se lee el dibujo mas de una vez por lectura, segun el paquete del contrato de PB-T4 v4.2, las decisiones del Owner sobre PB-T4 y la lista cerrada CT-PBT4A-01..08. Solo se sustituye con registro CT-PBT3A-12. Si hiciera falta mutar el rack resuelto o el informe de modulos dentro de la vida del borrador: STOP y se reabre L-1. No se ejecuta AutoCAD.

Invariants:
INV-PBT4A-01. Alcance: dentro de las 10 rutas. git diff BaseSha..CurrentSha -- src/RackCad.UI src/RackCad.Domain vacio; en src/RackCad.Plugin solo RIA y NH; en src/RackCad.Application solo WEI, WHC, IWorkspaceEditIntentOriginReader.cs y Draft/PushBackOriginByText.cs; en tests/ solo las tres clases nuevas y la linea del Skip de PushBackDraftWriteGuardTests.cs.
INV-PBT4A-02. RED y GREEN: el RED son las tres clases nuevas de tests/, el esqueleto exacto (WEI con FromReads inerte, la interfaz nueva completa y PushBackOriginByText inerte) y el Skip de CT-PBT3A-12, con WHC, RIA y NH sin cambio; el GREEN solo toca WEI, PushBackOriginByText.cs, WHC, RIA y NH. En el RED, cada filtro con ExpectRed da todos Failed por asercion (la primera linea del StackTrace bajo tests/), con Failed == Selected; el filtro de las guardas pasa y sus autopruebas fallan sobre el texto inyectado. Con una reparacion del RED (F4B-E35, RR): RedSha..RedRepairSha solo toca clases de prueba que anadio el RED y ningun archivo de src/ ni PushBackDraftWriteGuardTests.cs; el criterio se repite entero sobre RedRepairSha; y RedRepairSha..CurrentSha es el GREEN de arriba.
INV-PBT4A-03. La API: exactamente la de D-PBT4-02 y D-PBT4-03, con el texto literal del GREEN del paquete donde lo prescribe; la firma de RackPilotIntentAdapter.Read no cambia (EIG:139-141).
INV-PBT4A-04. Una CaptureDrawing y una LockDocument por lectura; AtOrigin no lee el dibujo (sin CaptureDrawing, bloqueo, transaccion, literal de cadena ni ex.Message) y solo llama a PushBackOriginByText.ReadAt, que hace en Draft/ las dos relecturas puras; RIA conserva un PushBackPilotIntentReader.Read(, un PushBackFullDraftReader.Read( y un new PushBackPilotIntentScan(, los tres en Read (EIG:350-364), StartTransaction 1 y LockDocument 1, y NH LockDocument 2 (G-9).
INV-PBT4A-05. El tipo del borrador del panel no cambia: Find<PushBackPilotDraftState> en las nueve lineas; WHC solo cambia en las cinco sentencias de lectura, con el texto literal de D-PBT4-03, y no declara nada nuevo.
INV-PBT4A-06. Ningun archivo de src/ fuera de Draft/, PP, UR y RIA nombra los cinco tipos del modelo (CT-PBT4A-06).
INV-PBT4A-07. Las redes: section 5 en verde. Ninguna prueba existente se edita, se salta, se reactiva ni se borra, con una unica salvedad: el Skip de CT-PBT3A-12, con su motivo literal. En CurrentSha, el git diff -U0 BaseSha..CurrentSha de PushBackDraftWriteGuardTests.cs solo lleva la linea de atributo con Skip.
INV-PBT4A-08. Las pruebas de UI: las reglas de section 5.0.
INV-PBT4A-09. Determinismo: GUID fijo, cultura invariante, "R" y orden ordinal; ni hora ni aleatoriedad.
INV-PBT4A-10. Las corridas locales: solo los filtros del contrato en UI; nunca la suite completa de UI en local; la de Core, si; sin copias de arboles; las corridas temporales terminan en Assert.Fail(string.Join("\n", lineas)), corren dentro de EditorDiscardPrompt.Substitute cuando abren ventanas, no entran en ningun commit y guardan TRX y log como artefactos.
INV-PBT4A-11. La evidencia: rutas relativas comprobadas en worker-handoff.json; TreeSha real o null; el Trailer, el real; el Worker no escribe en docs/.
INV-PBT4A-12. Las reglas de F4B; sin AutoCAD; la verificacion con las tres comprobaciones deterministas de F4B-E15.
INV-PBT4A-13. Las declaraciones nuevas en src/: las de la lista cerrada de PBT4A-C13 (con ReadAt), y ninguna mas, por nombre y acotada a los archivos que tocan, sobrecargas incluidas. No se admite ningun ayudante privado, tampoco en los dos archivos nuevos: el texto literal del paquete no lo necesita.
INV-PBT4A-14. D2: fuera de las dos referencias de cada lado, ninguna copia comparte un objeto mutable con su original. Ninguna linea anadida muta un PushBackSystem ni un RackModuleReconciliationResult adoptado.

StopConditions (propias):
PBT4A-C01: doing the task would require writing outside the AllowedWriteScope.
PBT4A-C02: it would require editing, skipping, re-enabling or deleting an existing test or helper, or touching InternalsVisibleTo, the packages, a .csproj or Fixtures/I64. Only one exception: the Skip of CT-PBT3A-12 in PushBackDraftWriteGuardTests.cs, with its literal reason. Any existing test that fails, outside that list, returns to the Owner.
PBT4A-C03: an observable change of RACKEDITAR appears, or of the pilot in the panel other than the declared one (the five rereads of an open draft go to the origin of the draft by its text, A-T3-8). STOP and return to the Owner.
PBT4A-C04: a method of a filter with ExpectRed passes on the skeleton, or fails by an exception and not by an assertion.
PBT4A-C05: a CT would need AutoCAD, a real MessageBox, the real configurator, the BOM, "Guardar en biblioteca", "Insertar", real keyboard or focus, or writing to the profile or to disk.
PBT4A-C06: the GREEN CI fails outside the new tests, reproducibly and not attributable to them.
PBT4A-C07: a fact of section 2 does not hold in a way that changes a CT or the design; or an expected line is not met in the GREEN; or a self-checking recipe (the self-tests of CT-PBT4A-07 and CT-PBT4A-08) does not exercise its path. The Worker does not adjust what is expected; except under F4B-E35 (the RED repair rule, RR): a writing error in a new test of this task, found after the RED push and before the GREEN, is repaired without this stop and without spending an attempt by one declared RED repair commit that touches only test files the RED added and changes only the when or the how of a reading or a gesture the package already prescribes, and no expected line, literal or oracle of the package or the contract, with the full RED criterion rerun on RedRepairSha; a second writing error in the same attempt, a repair that needs an expected line changed, or a literal line the package fixes for the repaired class (an expected line or a literal) that does not stay byte for byte in it, is this stop.
PBT4A-C08: a planning output rejected ONLY by the literal byte-fidelity comparison of the collections before the Worker allows ONE replanning with a reinforced prompt, without consuming an attempt ([DP] question 2); a second planning fidelity rejection is STOP and returns to the Owner; any other planning rejection (A1-A8 or another cause) is STOP and returns to the Owner
PBT4A-C09: a test would hang, leave a window, a modal or a dispatcher alive, write to the profile or depend on machine paths.
PBT4A-C10: doing the task would require what the scope of PB-T4a excludes: changing the type of the draft of the panel; beginning a draft from the full read; the window, Editar todo or Conservar mi borrador in two steps; the rule of the scale; creating views; touching the classic editor or RACKEDITAR; opening AUTH-13; or changing the signature of RackPilotIntentAdapter.Read.
PBT4A-C11: a negative control (nc1, nc2 or nc3) does not give CONTROL PASS: its ordinary output is kept as is and there are only two exits without returning to the Owner ([DP] question 1, "Extender firmas exactas"): (1) nc2 with the EXACT signature of PD-F4-01 (source f4-dispositions.md:1103-1106: Scope in fail on the removed path; Contract in fail only by the exactness clause; EXECUTION_BLOCKED/STOP; applied literally at :1540: no check other than the real verification differs; the exactness clause includes Contract evidence citing the contrast of the narrowed AllowedWriteScope with the contract or with byte-fidelity-check.json, f4-dispositions.md:520, :562, and nothing else) -> substitute PBT4-NC2-EXACT-WHITELIST-F4B-01, the version 2 of PBTx-NC2-EXACT-WHITELIST-F4B-01 (F4B-E43: docs/automation/evidence/I-64-pilot/F4B-DRAFTS/nc2-substitute-f4b-v2/nc2_exact_whitelist_substitute_f4b_v2.py, SHA-256 c033e96ce147963feb9c04733e9bfbec7493fea33d8ff79b1c2e8ab860798e45; owner-pbt3a-nc2-decision.md, "Aceptar y ampliar el sustituto"), whose property 8 admits only the closed list of the exact literals of the evidence entry of the byte-by-byte comparison, one per package (PB-T2 and PB-T3), with its reviewed text unchanged and run with --task PBT4 in the F4B-E21 mode with the injection note and the raw output of this task's accepted planning, recorded as nc2 = SATISFIED_UNDER_PBT4_EXACT_WHITELIST_SUBSTITUTE_F4B; if inject_opaque.py changed since that run, the substitute is reviewed before it is reused; (2) nc1 with the signature PD-F4B-01, the false SHA cascade (F4B-E40: owner-pbt2c-nc1-decision.md, its conditions (a) to (f): the mutation only changes the CurrentSha of the handoff to a SHA that does not exist in the repository; EXECUTION_BLOCKED / STOP with S-04 among the stops; Handoff is the first check in fail; the failing checks are a subset of Handoff, Identity, Remote, Scope, Ci and Tests that always includes Handoff and Identity, and all the others pass as in the real verification; each failing evidence cites the false SHA and none attributes the failure to another cause; the Identity evidence also writes the real HEAD), of which the EXACT signature of PD-F4-04 (source owner-f4t3-authorization.md:255-265; as applied in F4, f4-dispositions.md:1657-1660) is a particular case -> substitute PBT3x-NC1-CASCADE-SIGNAL-01 (F4B-E40 and F4B-E43: docs/automation/evidence/I-64-pilot/F4B-DRAFTS/nc1-cascade-substitute/nc1_cascade_signal_substitute.py, SHA-256 41440edc4a963bef890e4336dccb885837424008c027cdb172d2b926e0f49b40, reviewed APTO, with its declared limits), with its reviewed text unchanged and run with --task PBT4, recorded as nc1 = SATISFIED_UNDER_PBT4_NC1_CASCADE_SUBSTITUTE. Matching is decided never by check name: the control output is compared with the real verification check by check (the set of failing checks, the first failing check, Outcome and StopClass, and the evidence text); the only admitted differences are those listed for each signature in section 6.0 of the PB-T1 contract package ([PT1], docs/initiatives/I-64-f4b-pbt1-gate-contract-package.md) and, for PD-F4B-01, its conditions (a) to (f); the session records the comparison table in f4b-dispositions.md. Any other signature or variation (PD-F4-05, PD-F4-06, nc2 with Ci or Trailer also in fail, any nc1 that does not meet PD-F4B-01 - the one-off acceptances of nc1 in PB-TC (F4B-E08), in PB-TC2 attempt 1 (F4B-E12), in PB-T2a (F4B-E34), in PB-T2b (F4B-E38) and in PB-T2c (F4B-E40) do not extend -, an nc2 that the version 2 of the substitute does not give PASS - the disposition of the Owner on the nc2 of PB-T3a (F4B-E43) does not extend -, a substitute that does not give PASS, any failure of nc3, or an evidence text that cannot be assigned to an admitted difference) returns to the Owner before any substitute or acceptance; no one-off acceptance extends to PB-T4a
PBT4A-C12: a correction would be attempt 3 of 3: it returns to the Owner before it is issued (D-PB-10)
PBT4A-C13: doing the task would require touching a file of src/ other than WEI, WHC, RIA, NH, IWorkspaceEditIntentOriginReader.cs and Draft/PushBackOriginByText.cs; or adding a declaration of any visibility (type, constructor, method, property, field, constant, enum member or nested type; an overload is one more) whose name is not in this closed list: in WorkspaceEditIntent.cs, the properties FullRead and Catalog of EditIntentReadResult and its method FromReads; in IWorkspaceEditIntentOriginReader.cs, the interface IWorkspaceEditIntentOriginReader and its method ReadIntentAtOrigin; in Draft/PushBackOriginByText.cs, the class PushBackOriginByText and its methods Choose, AtKey and ReadAt; in RackPilotIntentAdapter.cs, the second constructor of RackPilotIntentReadResult, its properties Full and Input, and the method AtOrigin of RackPilotIntentAdapter; in WorkspaceNavigationHost.cs, the method ReadIntentAtOrigin of the nested reader. Two changes of existing members are prescribed and are not new declarations: the private constructor of EditIntentReadResult gains two parameters, and ReadOnActiveDocument gains the parameter originText. In WorkspaceHostCore.cs, no new declaration: only the inline statement of D-PBT4-03 in five bodies (the pattern variable originReader is not a member). The Worker stops before adding it.
PBT4A-C14: a design of the corpus changes its admission or its lock (the pilot reader, the full reader, AUTH-13 or the parked tail).
PBT4A-C16: an existing test outside the closed list of substitutions of PB-T4a (CT-PBT3A-12) fails. It returns to the Owner (owner-pbt4-decisions.md, question 4).
PBT4A-C17: (F4B-E11, D2, IMPORTANT LIMIT) doing the task would need to MUTATE the resolved rack (PushBackSystem) or the module report (RackModuleReconciliationResult) inside the editing or draft lifetime, or to share by reference any other mutable object between a copy of the state and its original, or a measurement already finds such a mutation in the editing path: STOP and L-1 is reopened. The Owner text, literal: "This is NOT authorization for general shallow copying. It is exactly: two declared shared references. If later PB-T1/PB-T2/PB-T3 needs to mutate either: resolved rack; module report; inside the Workspace draft lifetime: STOP and reopen L-1. At that point the object must either: be cloned; be made immutable; or be replaced by a value snapshot appropriate to the draft model." The Worker chooses none of the three exits: the Owner does
PBT4A-C18: a RED repair does not meet the F4B-E35 rule (RR-1 to RR-6): it is not after the RED push and before the GREEN, or it is the second one in the attempt; it touches a file the RED did not add, an existing test, src/, a fixture or anything outside the AllowedWriteScope; it changes an expected line, literal or oracle, or anything other than the when or the how of a reading or a gesture the package already prescribes (RR-3); the full RED criterion and its CI are not rerun on RedRepairSha; or it is not declared as RR-5 says. The rule does not apply and the repair counts as touching tests/ after the RED (AUTOMATION_PLAN 16.8): STOP.

ExpectedEvidence:
[0] antes del Worker: el contrato, la salida del Controller (output.json, con las ocho colecciones opacas vacias), la delegacion final con esas colecciones puestas por la sesion (delegation.json; F4B-E21), la comparacion byte a byte sobre la delegacion final, A1-A8 y nc4;
[1] el commit RED y su push antes del GREEN; con una reparacion del RED (F4B-E35, RR-4; V43-04), ademas, el commit "RED repair" y su push antes del GREEN, con su CI, registrada como la del RED reparado, y los TRX de cada filtro con ExpectRed sobre RedRepairSha;
[2] la CI del RED como en section 6.0 (tests en failure, ui-tests y build-ui en success, build-plugin omitido, sin cuelgue); la del RED reparado, si lo hay, da lo mismo;
[3] la tabla "CT -> la asercion que falla en el RED" y las autopruebas de CT-PBT4A-07 y CT-PBT4A-08;
[4] el GREEN y su CI 4/4;
[5] en local: el build Release con el Plugin sin avisos nuevos, los filtros sobre el RED, sobre RedRepairSha si hay reparacion, y sobre el GREEN, y tres corridas en verde de los filtros con ExpectRed sobre el GREEN;
[6] la entrega con el mapa CT -> metodo, las rutas relativas comprobadas y el Trailer real de los commits RED, "RED repair" (si lo hay) y GREEN; con una reparacion del RED, ademas, RedRepairSha, la prueba y la linea corregidas, el error y por que no cambia ninguna linea esperada, con el diff, y RedSha, que sigue siendo el commit RED original (F4B-E35, RR-5);
[7] la verificacion con las tres comprobaciones de F4B-E15, y nc1 a nc3;
[8] con una reparacion del RED, la verificacion la lee como parte de la entrega (F4B-E35, RR-7; section 6.0; V43-05 y V44-04, CA-T3a M-01): n.o 10 (Tests): el RED se acredita con RedSha y con la CI del RED original, y sus TRX, con los filtros con ExpectRed entre los fallidos. La clausula "git diff --name-only RedSha CurrentSha no toca RT U ChainRedFiles" se comprueba desde RedRepairSha: git diff --name-only RedRepairSha CurrentSha. n.o 11 (Trailer): el trailer va tambien en el commit "RED repair". Lo que el Controller comprueba de RR (si algo falla, es C18): RR-1, un solo commit "RED repair", hijo directo de RedSha y anterior al GREEN; RR-2, git diff --name-only RedSha RedRepairSha dentro de las clases de prueba que anadio el RED (git diff --name-only --diff-filter=A BaseSha RedSha -- tests/); RR-4, la CI de RedRepairSha, registrada como la del RED reparado, y los TRX de los filtros con ExpectRed sobre el; RR-5, la declaracion de la entrega, con el diff, y RedSha, el commit RED original; RR-6, con el tramo RedSha..RedRepairSha. En PB-T4a, ademas: RedSha..RedRepairSha no toca ningun archivo de src/ (el esqueleto) ni PushBackDraftWriteGuardTests.cs (el Skip de CT-PBT3A-12). La ayuda mecanica de RR-3: es parada C07, una violacion cierta de RR-3, que un literal de la lista cerrada de la clase reparada (section 6.0 del paquete, la tabla de RR-3) cambie entre RedSha y RedRepairSha: una linea o un fragmento, si no aparece como subcadena exacta el mismo numero de veces; un valor de gesto, si no aparece como token el mismo numero de veces. Indicios para la revision, no parada: una cadena <campo>= anadida o quitada en git diff -U0 RedSha RedRepairSha, o un cambio en el primer argumento de un Assert.Equal. Pueden dar falsos positivos, como una linea que solo se mueve de sitio, asi que van a Findings y a la revision, con la declaracion de RR-5.
[9] la CI del RED con tests en failure, ui-tests y build-ui en success y build-plugin omitido;
[10] la del GREEN 4/4, con el Plugin compilado;
[11] el censo de declaraciones de WEI, WHC, RIA y NH, y de los dos archivos nuevos, en BaseSha y en CurrentSha, contadas una a una con su nombre (una sobrecarga es una declaracion mas): la diferencia es la lista cerrada de PBT4A-C13;
[12] el Trailer real;
[13] la entrada "## PB-T4a" de superseding-characterization.md, escrita por la sesion en el commit de emision, con el Skip de CT-PBT3A-12, su motivo literal y CT-PBT4A-06, que la sustituye; su tabla de ejecucion se completa al cerrar.
[14] Linea base de la CI: la del cierre de PB-T3c (F4B-E48): run 44757675821 sobre el GREEN 938a35f0, Core 19 129 resultados, con 19 123 Passed y 6 Skipped, los de D-1, y UI 2 451, con 2 430 Passed y 21 Skipped. Previstos, con el total exacto de cada rama (F4B-E46): PB-T4a anade ocho [Fact] y un Skip de un [Fact], sin ninguna Theory, asi que hay una sola rama. El RED: Core 19 137 resultados, con 19 125 Passed, 5 Failed (CT-PBT4A-01 a 04 y 07) y 7 Skipped. El GREEN: Core 19 137, con 19 130 Passed, 0 Failed y 7 Skipped. UI, en los dos: 2 451, con 2 430 Passed y 21 Skipped.
```

**`RequiredTests`** (38): las dos clases nuevas, y los 36 filtros del contrato de PB-T3c, **todos con `ExpectRed` false** (también `PushBackViewEraseGuardTests`, que era true en PB-T3c) y su `MinSelected`, salvo **`PushBackDraftWriteGuardTests`, que baja de 3 a 2** por el `Skip` de CT-PBT3A-12 (V41-08), como bajaron los de F4 con los `Skip` de D-1:

| # | Project | Filter | MinSelected | ExpectRed |
|---|---|---|---|---|
| 1 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.PushBackFullReadPathTests` | **5** | true |
| 2 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.PushBackFullReadGuardTests` | **3** | false |
| 3 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.PushBackViewEraseGuardTests` | 4 | false |
| 4 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.PushBackWritePathPluginGuardTests` | 1 | false |
| 5 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.WorkspacePilotUpdateGuardTests` | 86 | false |
| 6 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.WorkspacePilotUpdateIntegrationGuardTests` | 123 | false |
| 7 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.PushBackSharedReferenceGuardTests` | 2 | false |
| 8 | `tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj` | `FullyQualifiedName~RackCad.UI.Tests.PushBackFullDraftWindowParityTests` | 1 | false |
| 9 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.PushBackFullWrite` | 13 | false |
| 10 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.PushBackPilotUpdateEquivalenceGuardTests` | 6 | false |
| 11 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.PushBackWritePathGuardTests` | 2 | false |
| 12 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.Workspace` | 3261 | false |
| 13 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.PushBackFullDraft` | 10 | false |
| 14 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.PushBackDraftWriteGuardTests` | **2** | false |
| 15 | `tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj` | `FullyQualifiedName~RackCad.UI.Tests.Workspace` | 547 | false |
| 16 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.PushBackEditableState` | 13 | false |
| 17 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.PushBackEditorOutputGate` | 1 | false |
| 18 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.PushBackWindowSharedReferenceGuardTests` | 1 | false |
| 19 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.PushBackWindowOnStateSourceGuardTests` | 1 | false |
| 20 | `tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj` | `FullyQualifiedName~RackCad.UI.Tests.PushBackWindowOnStateTests` | 2 | false |
| 21 | `tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj` | `FullyQualifiedName~RackCad.UI.Tests.PushBackClassicCharacterization` | 55 | false |
| 22 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.PushBackClassicCharacterization` | 10 | false |
| 23 | `tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj` | `FullyQualifiedName~RackCad.UI.Tests.PushBackFullEditorCharacterization` | 16 | false |
| 24 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.PushBackFullEditorCharacterization` | 3 | false |
| 25 | `tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj` | `FullyQualifiedName~RackCad.UI.Tests.PushBackStateNet` | 14 | false |
| 26 | `tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj` | `FullyQualifiedName~RackCad.UI.Tests.PushBackDeclaredCorrection` | 14 | false |
| 27 | `tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj` | `FullyQualifiedName~RackCad.UI.Tests.Cantilever` | 67 | false |
| 28 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.I58F1` | 10 | false |
| 29 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.CustomPropertiesEnvelopeGuardTests` | 9 | false |
| 30 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.NamespaceFolderGuardTests` | 7 | false |
| 31 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.PushBackDraftStatePortTests` | 3 | false |
| 32 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.PushBackDraftPortSharedReferenceGuardTests` | 1 | false |
| 33 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.PushBackWorkspaceModeSourceGuardTests` | 1 | false |
| 34 | `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.PushBackWorkspaceSharedReferenceGuardTests` | 1 | false |
| 35 | `tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj` | `FullyQualifiedName~RackCad.UI.Tests.PushBackWorkspaceModeTests` | 3 | false |
| 36 | `tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj` | `FullyQualifiedName~RackCad.UI.Tests.PushBackWorkspaceApplyCloseTests` | 4 | false |
| 37 | `tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj` | `FullyQualifiedName~RackCad.UI.Tests.PushBackFieldValidationConcordanceTests` | 1 | false |
| 38 | `tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj` | `FullyQualifiedName~RackCad.UI.Tests.PushBackWorkspaceCarryoverTests` | 3 | false |

- **Nota (no va al contrato).** Ningún filtro heredado selecciona las clases nuevas: `RackCad.Tests.PushBackFullDraft` y `RackCad.Tests.PushBackFullWrite` no son prefijo de `RackCad.Tests.PushBackFullRead…`, y `RackCad.Tests.Workspace` no las selecciona porque su espacio de nombres es `RackCad.Tests`. Los `MinSelected` heredados no bajan: PB-T4a no salta nada más.

#### 6.1.2 Lo que necesita el generador de PB-T4a (V41-18)

| Pieza | Dónde, en este paquete | Lo que comprueba el generador |
|---|---|---|
| **`TaskId`** | §6.1.1 | `F4B-PBT4-FULL-READ-PATH`, 23 caracteres, ≤ 26 |
| **`Objective`** | §6.1.1 | ASCII, byte a byte |
| **`Authorities`** | Abajo | Cada una con su ruta en `AuthorityRevision` |
| **`AllowedWriteScope`** | §6.1 | Las 10 rutas, en ese orden |
| **`ForbiddenWriteScope`** | §6.0 | El método de [PKC-C], con `PushBackDraftWriteGuardTests.cs` fuera de lo prohibido |
| **`UNIT_CHANGE`** | §6.0 y §6.1 | WEI, WHC, RIA y NH |
| **`Invariants`** | §6.1.1 | INV-PBT4A-01 a 06 y los comunes 07 a 14, literales |
| **`StopConditions`** | §6.0 y §6.1.1 | Las 17 estándar del contrato de PB-T3c, y PBT4A-C01 a C14, C16, C17 y C18 (sin C15): 34. **C11** es `PBT3C-C11` con `PBT3C` → `PBT4` y «PB-T3c» → «PB-T4a», y acaba en «no one-off acceptance extends to PB-T4a» |
| **`RequiredTests`** | §6.1.1 | 38, con `MinSelected` y `ExpectRed`; `PushBackDraftWriteGuardTests` en 2 |
| **`ExpectedEvidence`** | §6.1.1 | 15; `[0]` byte a byte el de PB-T3c; `[14]` con los totales de la rama única: RED 19 137 = 19 125 + 5 + 7, GREEN 19 137 = 19 130 + 0 + 7, UI 2 451 = 2 430 + 21. El generador la compara con la cuenta de CT (8), de `[Fact]` (8), de `Skip` (1) y de `ExpectRed` (5) |
| **`EligibleCells`, `RoutingEnforcement`, `CorrectionsAuthorized`** | §6.0 | Las de PB-T3c; `required`; true |
| **El esqueleto y su parche** | §6.1 | WEI, la interfaz, `PushBackOriginByText` inerte y la línea del `Skip`, literal; la copia compila y da Core **19 129 = 19 122 + 7** |
| **El registro declarado** | §6.1.1 `[13]` | La entrada «## PB-T4a» con el `Skip`, el motivo literal y CT-PBT4A-06 |
| **La lista de PB-T4a** | `pbt4-substitution-lists.md` | 1: `CT_PBT3A_12_NoSourceFileOutsideTheFolderOfTheModelNamesItsTypes` |

**Las autoridades de PB-T4a:**
- [OD] (la fila de PB-T4, D-PB-01 y D-PB-05) y [P] (OB-01.1 a 6, OB-03.2, OB-05.1 a 5, R-PB-10, §7.3 y §7.4);
- [P2] §10 y [P3] §10 (A-T3-1 a A-T3-11; A-T3-8 es el que aplica PB-T4a);
- [PT1] (A-5, A-9, A-10 y R-T1-17);
- **las decisiones del Owner sobre PB-T4** (`owner-pbt4-decisions.md` y `owner-pbt4b-retire-list-decisions.md`, con su número F4B-E al custodiarlos);
- F4B-E48 (el cierre de PB-T3c), F4B-E46 (el total exacto de cada rama), F4B-E45 (las sobrecargas y la reflexión), F4B-E43 y F4B-E40 (los sustitutos de nc2 y nc1), F4B-E37 (la Snapshot K), F4B-E35 pregunta 4 (RR), F4B-E29 (D-1), F4B-E25 (`UNIT_CHANGE`), F4B-E21 y F4B-E22, F4B-E15, F4B-E11 (D2) y F4B-E03;
- este paquete (v4.2), con su comprobación acotada de PB-T4a.

**Lo que cambia para el generador respecto de la v4.1** (V42-02): solo el texto de PB-T4a que toca B-08. El `Objective`, el alcance (10 rutas), `UNIT_CHANGE`, los `RequiredTests` (38), las paradas (34), los totales y `ExpectedEvidence` no cambian. Cambian:
- el esqueleto: `PushBackOriginByText` lleva también `ReadAt` inerte (`=> null;`) y el `using RackCad.Application.Catalogs;`;
- PBT4A-C13 añade `ReadAt` a los métodos de `PushBackOriginByText`;
- INV-PBT4A-04 dice que `AtOrigin` solo llama a `ReadAt` y que RIA conserva una lectura de cada lector y un escaneo, en `Read`;
- las líneas literales de CT-PBT4A-04 y CT-PBT4A-07 (§5.1), y la fila de RR-3 de `PushBackFullReadPathTests` con ellas;
- el `Objective` cita «el paquete del contrato de PB-T4 v4.2», en lugar de v4.1; lo demás del `Objective`, igual;
- el registro de autoridades nombra también `owner-pbt4b-retire-list-decisions.md`, que no cambia PB-T4a.

### 6.2 PB-T4b — `F4B-PBT4-FULL-DRAFT-PANEL`

**Se emite por la autorización condicional** (`owner-pbt4-decisions.md`, pregunta 4) **con la lista exacta de 470** que el Owner tomó como «la que vio» (`owner-pbt4b-retire-list-decisions.md`, pregunta 2; `pbt4b-substitution-list.tsv`, sha256 `44393b19…63e4`), si se cumple el resto de la condición. **No retira nada:** la retirada es de PB-T4d.

- **Objective** (ASCII):
  > Pasar el borrador del panel del Workspace de Push Back al modelo completo de PB-T3 con la lectura completa de PB-T4a, sin traducir estados: OB-03.2 (los racks A/B se editan en el panel; la cola y AUTH-13 siguen rechazados), la regla unica de la escala en Application (la logica pura de la validacion numerica pasa de la UI a Application y la UI delega en ella, sin cambiar su comportamiento; el anfitrion la usa por defecto), Actualizar con PrepareFull y ExecuteFull, Conservar mi borrador en dos tiempos por TryKeepMine (presentacion por grupos, clasificada por una funcion pura de Draft con un testigo de lo releido, y KeepOn solo tras confirmar con el testigo vigente); el codigo del piloto queda sin uso, sin retirarlo (lo retira PB-T4d), segun el paquete del contrato de PB-T4 v4.2, las decisiones del Owner sobre PB-T4, la lista cerrada CT-PBT4B-01..20 y la lista exacta de 470 sustituciones que el Owner tomo como la que vio, con las 428 pruebas del panel de F4 saltadas y duplicadas por el mapa cerrado y ninguna prueba ni ayudante editado ni borrado salvo sus Skip. Si hiciera falta mutar el rack resuelto o el informe de modulos dentro de la vida del borrador: STOP y se reabre L-1. Ni Editar todo, ni RACKEDITAR, ni la ventana; no se ejecuta AutoCAD.
- **`AllowedWriteScope`:** WHC, WEI, `WorkspacePilotSupport.cs`, FVM, PV, WCo; `src/RackCad.UI/Controls/NumericFieldValidation.cs`; los nuevos `src/RackCad.Application/Formatting/NumericTextRule.cs`, `Draft/PushBackAnnotationScaleRule.cs`, `Draft/PushBackFullFieldAdapter.cs` y `Draft/PushBackKeepMinePresentation.cs`; las clases nuevas (CT-PBT4B-01 a 20, las nueve copias y el kit nuevo); **solo para los `Skip`:** las 9 clases y los archivos de la lista de C-5 y C-3 (`pbt4b-substitution-list.tsv`). **Ningún archivo de `Pilot/`.**
- **El esqueleto del RED** no toca `NumericFieldValidation.cs` (M-29).
- **`RequiredTests`:**
  - Core `PushBackFullPanel` (13 + E^R), `ExpectRed` true;
  - Core las guardas, con CT-19 y CT-20, `ExpectRed` false;
  - UI `PushBackFullPanelFieldTests`: 3, true (CT-04, 14 y 18);
  - las copias, partidas: `FullRedTests`, true, y `FullGreenTests`, false (§5.2);
  - `NumericFieldValidationTests` y `NumericFieldTests`, en verde, false;
  - y los heredados, con los `MinSelected` que dejen los `Skip`, medidos en la emisión.
- **`StopConditions` propias, además de las comunes:** **PBT4B-C20:** una caja numérica de cualquier ventana cambia de estado, valor o mensaje (lo vería `NumericFieldValidationTests` o CT-18). **PBT4B-C02** sin excepción fuera de los `Skip` de su lista: borrar o editar una prueba o un ayudante es la parada.
- **El generador de PB-T4b comprueba la lista:** el sha256 de `pbt4b-substitution-list.tsv` y que el filtrado de `pbt4-substitution-lists.tsv` por la regla de D-PBT4-11 da el mismo texto, byte a byte.

### 6.3 PB-T4c — `F4B-PBT4-EDIT-ALL-WINDOW`

- **Objective** (ASCII):
  > Anadir Editar todo al panel del Workspace de Push Back sobre el borrador completo de PB-T4b: un boton y su gesto, que confirma el texto pendiente con CommitField antes de abrir (R-PB-10); TryEditAll en el anfitrion y en sus dos reenvios (la compuerta de avisos y el adaptador del panel del Plugin), que empieza el borrador si hace falta, mantiene la marca de lectura mientras la ventana esta abierta (K = 1) y abre la ventana de Push Back en modo WORKSPACE en dos piezas: el modal del panel en una clase anidada del controlador del Plugin y la ventana, sin dueno, en una clase nueva de Plugin/Views; el adaptador del puerto que conserva Origin y acaba la marca known clean; la guarda de texto de OB-01.3 sobre los seis sujetos del camino; Cerrar con una sola pregunta si hay a la vez un modulo escenificado y cambios sin aplicar; y el resumen y Modificado al cerrar, segun el paquete del contrato de PB-T4 v4.2 y la lista cerrada CT-PBT4C-01..15. Solo se sustituyen con registro la guarda de las operaciones del puerto (PEG:155-163) y el censo de archivos del Plugin con PushBack (CT-PBT1B-15). Ni el camino clasico, ni RACKEDITAR, ni la ventana en CLASSIC; cero escrituras en el DWG desde la ventana; no se ejecuta AutoCAD.
- **`AllowedWriteScope`:**
  - WHC, `IWorkspacePilotEditPort.cs`, `WorkspacePilotSupport.cs`, WEI, FVM, PV, WCo y W;
  - los nuevos `IWorkspaceFullEditorHost.cs`, `Draft/PushBackFullDraftLaunch.cs`, `Draft/PushBackFullDraftStatePort.cs` y `src/RackCad.Plugin/Views/RackWorkspaceEditorWindow.cs`;
  - las clases nuevas (con `PushBackDraftLaunchGuardTests`, CT-15);
  - **solo para los `Skip`:** PEG y `PushBackSharedReferenceGuardTests.cs`.
- **`RequiredTests`:**
  - Core `PushBackEditAll`: 9, true;
  - Core `PushBackOpenerClassicPathGuardTests`, `PushBackDraftLaunchGuardTests` y las dos guardas equivalentes: 4, false;
  - UI `PushBackEditAllWindowTests`: 4, true;
  - y el resto.


### 6.4 PB-T4d — la retirada del piloto (esbozo; V42-01)

**Solo un esbozo con su lista** (`owner-pbt4b-retire-list-decisions.md`): su contrato completo viene después, con su paquete y su comprobación, y **vuelve al Owner con su lista antes de emitirse**, porque la autorización condicional de `owner-pbt4-decisions.md` (pregunta 4) no la cubre.

- **`TaskId` propuesto:** `F4B-PBT4-RETIRE-PILOT` (21 caracteres, ≤ 26), con el prefijo `F4B-PBT4-` de los sustitutos de nc1 y nc2. Lo fija su paquete.
- **Cuándo:** después del cierre de PB-T4c, cuando las copias y los equivalentes de PB-T4b llevan dos subtareas en verde.
- **Qué hace:** retira lo de D-PBT4-10 (el comparador, el adaptador del campo, `KeepPilotEditsOn`, `ComposeForWrite`, y el `Prepare`, el `Precheck` y el `Execute` del piloto); se quedan el lector del piloto, su materialización, `PushBackPilotDraftState` y las constantes de `PushBackPilotMaterializer`.
- **Su lista, medida en `7f0d9aa3`** (`pbt4d-retire-list.tsv`, sha256 `1c9d67e8f2dac7eae3202d61a23ff5131e96123e5a6af275e5ce8a8bf71b8b71`, 55 225 bytes; la genera `split_lists_pbt4.py`):
  - **139 sustituciones con registro** (C-7): 72 por texto, con E-3, E-5 y E-6 de D-1, y 67 que dejan de compilar (R-36). Por clase, las de más peso: `WorkspacePilotUpdateRunTests` 48 de sus 49 (6 por texto y 42 por compilación), `WorkspaceDraftPilotMaterializationTests` 23, `WorkspaceDraftPilotStateTests` 22 y `PushBackFullWritePrepareTests` 9 de sus 9 (3 por texto y 6 por compilación);
  - **55 borrados con registro** de originales de las 9 clases, saltadas desde PB-T4b, que dejan de compilar al retirar (EIH 2, WPEH 16, WPP 30 y WPRH 7);
  - **29 miembros de apoyo** que nombran lo retirado, que se borran o se editan con registro (entre ellos `PilotUpdateSetup.Run` y `ExpectedPlan`, `PilotoEscribe` y `Paridad`, los dos campos `Comparer`, `OwnershipKits.Pilot` y la constante `Key` de `WorkspacePilotEditHostTests`);
  - **CT-PBT4B-20,** cuyo punto 1 deja de valer para las 55 que se borran.
- **La lista se vuelve a medir sobre el cierre de PB-T4c** antes de ir al Owner: PB-T4b y PB-T4c añaden pruebas, y alguna puede alcanzar lo que se retira.
- **Su `UNIT_CHANGE`:** los archivos de `Pilot/` que retira.
- **C02, con excepción declarada:** los borrados y las ediciones de su lista, cada uno con su registro; es lo que el Owner eligió al poner la retirada después de PB-T4c, cuando ya no corre ninguna de esas pruebas (I-26).

## 7. Riesgos y lecciones aplicadas

Los de la v1 (R-T4-01 a 15) y la v4 (R-T4-16 a 21), con lo que hacen la v4.1 y la v4.2:

| Riesgo | Qué hace la v4.2 |
|---|---|
| R-T4-01, la lista | Las listas, guardadas método a método, con su causa y su destino; la de PB-T4b, fijada por sha256 y aprobada por el Owner (470) |
| R-T4-08, los miembros por defecto | Solo `TryEditAll`, con sus dos reenvíos explícitos (CT-PBT4C-12 y 13) |
| R-T4-13, el modal y el abridor | Dos piezas; CT-PBT4C-08 fija el literal de la conversión (M-30) |
| R-T4-16, la doble lectura del piloto | Se mide en la precondición 6. En PB-T4a, la relectura en el origen repite las dos lecturas puras sobre el mismo escaneo, en `Draft/`, sin acceso nuevo al dibujo |
| R-T4-17, el coste de `IsDirty` | Sin cambio |
| R-T4-19, la duración de la suite de Core | Se mide en la precondición 6 de PB-T4b; si pasa de 15 min, vuelve a la sesión |
| R-T4-20, mover la validación numérica | `NumericFieldValidationTests` (UI) y CT-PBT4B-18; PBT4B-C20. El esqueleto de PB-T4b no toca `NumericFieldValidation.cs` (M-29) |
| R-T4-21, los riesgos del borrado real | Al Smoke-PB (§5.5) |
| R-T4-22, una guarda que fija una firma entera | EIG:139-141 fija la de `RackPilotIntentAdapter.Read`; `AtOrigin` lo evita |
| **R-T4-23, la retirada rompe por compilación** | Un `Skip` no evita un error de compilación. **El Owner la puso después de PB-T4c** (PB-T4d): PB-T4b no borra ni edita nada, y PB-T4d borra con registro, cuando ya no corre ninguna de esas pruebas (I-26) |
| R-T4-24, las copias en el RED | Partidas por su resultado en el RED, con una partición cerrada medida (§5.2) |
| R-T4-25, el léxico de las mediciones | Corregido otra vez en la v4.2 (`@"""…`); las listas no cambian |
| **R-T4-26. Una guarda que cuenta una llamada en todo un archivo** (B-08) | EIG:357 exige un solo `PushBackPilotIntentReader.Read(` en todo RIA, y la v4.1 lo medía solo en el cuerpo de `Read`. **La lección:** las guardas de los archivos tocados se recorren enteras, prueba a prueba (§4.1), no por selección; lo que no se pueda ver se declara con su parada |

## 8. Las decisiones del Owner

### 8.1 Ya tomadas antes de PB-T4

Las de la v1 (§8.1): [OD] con D-PB-01 y D-PB-05; F4B-E28 Q3; F4B-E38; F4B-E29 (D-1); F4B-E11 (D2) con L-5; F4B-E24; la nota del Owner sobre la interfaz; F4B-E03; F4B-E15, E21, E25, E35 Q4, E37, E42, E45 y E46; D-PB-10.

### 8.2 Las decisiones sobre PB-T4 (2026-11-20)

**Los registros literales:** `owner-pbt4-decisions.md` (las cuatro primeras) y `owner-pbt4b-retire-list-decisions.md` (las dos de la vuelta de PB-T4b, redactadas en la §16.5 de la revisión). La numeración F4B-E se asigna al custodiarlos, con la emisión de PB-T4a. Las preguntas se numeran como en esos registros.

**La primera vuelta** (`owner-pbt4-decisions.md`):

| Pregunta | Respuesta del Owner, literal | Lo que aplica la v4.2 | Lo que no se hace |
|---|---|---|---|
| **1, «Piloto»** | «Retirarlo (Recomendado)» | D-PBT4-10: se retira lo que solo usaba el panel; la carga se queda dentro del lector, que usa el modelo completo para AUTH-13. **Cuándo,** lo dice la segunda vuelta: en PB-T4d | «Conservarlo sin uso» y «Dos modelos hasta F5-A» |
| **2, «Presentación»** | «Por grupos (Recomendado)» | D-PBT4-09, con los nueve nombres fijos, la unión, el plural y las líneas de lo estructural y de las vistas | «Por campos» y «Por campos con valores»: sin `PushBackFieldLabels` ni `PushBackPresentationForm` |
| **3, «Pruebas F4»** | «Saltar y duplicar (Recomendado)» | D-PBT4-11: las 428 se saltan y se duplican con un kit nuevo, como en D-1, con el mapa cerrado y el guion de CT-PBT4B-20; sin excepción a C02 en PB-T4b | «Editar una línea» (sin excepción declarada a C02 ni edición de EIH) y «Implementación de referencia» |
| **4, «Emisión»** | «Sí, con condiciones (Recomendado)» | PB-T4a, PB-T4b y PB-T4c, en ese orden, cada una tras el cierre de la anterior, sin volver al Owner si la comprobación acotada da 0 bloqueantes y 0 importantes sin resolver, la remedición no cambia nada material y la lista emitida es la que vio el Owner o una parte de ella. **No cubre PB-T4d** | «Solo PB-T4a» y «Cada subtarea me vuelve» |

**La segunda vuelta** (`owner-pbt4b-retire-list-decisions.md`):

| Pregunta | Respuesta del Owner, literal | Lo que aplica la v4.2 | Lo que no se hace |
|---|---|---|---|
| **1, «Retirada»** | «Retirar tras PB-T4c (Recomendado)» | D-PBT4-10 y §6.4: el código del piloto queda sin uso en PB-T4b y PB-T4c, y lo retira **PB-T4d**, después de PB-T4c, que sustituye las 139 y borra con registro las 55 originales saltadas y los 29 ayudantes, y **vuelve al Owner con su lista antes de emitirse** | «Retirar en PB-T4b» y «Conservarlo sin uso» |
| **2, «Lista T4b»** | «Sí, esa lista (Recomendado)» | D-PBT4-11: la lista de PB-T4b son las **470** de `pbt4b-substitution-list.tsv` (sha256 `44393b19…63e4`), que el Owner toma como «la que vio»; PB-T4b se emite con ella por la autorización condicional, si se cumple el resto de la condición | «Vuelve antes de emitir» |

**Vuelve al Owner** (pregunta 4, literal): «por una decisión nueva, un control con una firma no prevista, un intento 3, un efecto material sin medir o una prueba que falle fuera de la lista». Si eso pasa en mitad de una subtarea, el Worker para (C16 y las paradas de §6). **Y PB-T4d, siempre, con su lista.**

### 8.3 Las listas guardadas y lo que el Owner aprobó (I-22, I-23, I-26, M-32, M-33)

| Subtarea | Lista | Lo que vio el Owner | Exacto | Su estado |
|---|---|---|---|---|
| PB-T4a | CT-PBT3A-12 (C-5) | 1 | **1** | Autorización condicional (pregunta 4) |
| PB-T4b | Las 9 clases (C-0), «Saltar y duplicar»: 375 copias mecánicas y 53 equivalentes a mano | 428 | **428** | |
| PB-T4b | La letra de las guardas (C-5 y C-6) | «unas 12» | **15** | |
| PB-T4b | La UI de la elección (C-3) | «hasta 21», recontada a unas 25 | **27** | |
| PB-T4b | **Total** | **unas 465**; en la segunda vuelta, **470** | **470** | **Aprobada como «la que vio»** (segunda vuelta, pregunta 2); `pbt4b-substitution-list.tsv`, sha256 `44393b19d1f7490081c8ec9bac633ea4acbfe792103e5e5c3ca5e1f3fa1163e4` |
| PB-T4c | PEG:155-163 y CT-PBT1B-15 (C-5) | 2 | **2** | Autorización condicional |
| PB-T4d | C-7: 72 por texto (con E-3, E-5 y E-6) y 67 por compilación | «unas 100»; en la segunda vuelta, 139 | **139** | **Vuelve al Owner con su lista** (esbozo en `pbt4d-retire-list.tsv`, sha256 `1c9d67e8f2dac7eae3202d61a23ff5131e96123e5a6af275e5ce8a8bf71b8b71`) |
| PB-T4d | Las originales de las 9 clases que dejan de compilar al retirar, borradas con registro | 55 | **55** | Ídem |
| PB-T4d | Los miembros de apoyo que nombran lo retirado | 29 | **29** | Ídem |

**Las cifras que corrige la v4.2** (M-32): de las 67 pruebas que dejan de compilar al retirar, `WorkspacePilotUpdateRunTests` da **42** y `PushBackFullWritePrepareTests`, **6**; en C-7, por texto y por compilación, son **48** (de las 49 de la clase) y **9** (de 9). Las demás de compilación: `WorkspaceDraftPilotOwnershipTests` 8, `WorkspaceDraftPilotStateTests` 7, `PushBackDeclaredCorrectionPilotParityTests` 2, `PushBackFullWriteRunTests` 1 y `WorkspaceDraftPilotMaterializationTests` 1.

**Lo que cambia y lo que no al retirar después de PB-T4c** (I-26): la retirada no evita el borrado de las 55 originales saltadas y de los 29 miembros de apoyo; cuando se retire, dejarán de compilar igual. Lo que cambia es que, para entonces, ya no corre ninguna, porque sus copias y sus equivalentes están en verde desde PB-T4b, y que PB-T4b conserva la premisa de la pregunta 3 de la primera vuelta.

**Las 470 no dependen de la retirada** (M-33): las 9 de la letra que nombran lo que se retirará caen por el cambio del anfitrión (§4.2).

**Lo declarado, sin preguntar** (como en la v4):
- que pedir «Conservar» lee el dibujo, frente a la regla de F4 de la UI :780-793, con su registro;
- el límite del testigo (M-20);
- el cambio observable de PB-T4a (M-23);
- que la validación numérica pasa de la UI a Application, sin cambiar su comportamiento (D-PBT4-06);
- **que la firma de `RackPilotIntentAdapter.Read` no cambia** y la relectura en el origen es pura, en `Draft/`, sobre el mismo escaneo (D-PBT4-03; V41-06; B-08);
- **que, entre PB-T4b y PB-T4d, la regla de la escala queda también en `PushBackPilotFieldAdapter`,** sin uso (D-PBT4-10).

### 8.4 Lo que no es decisión

Lo de la v3 (§8.3): el orden de las subtareas, los nombres, las CT y las guardas medidas. Además:
- **el abridor en dos piezas;**
- **la validación numérica movida a Application,** que no cambia el comportamiento. Si la comprobación acotada o la precondición 6 muestran una guarda de T1B que lo impide, vuelve al Owner por OB-T1B.3;
- **`AtOrigin` y `ReadAt` en lugar de un parámetro nuevo de `Read`:** lo imponen dos guardas medidas (EIG:139-141 y EIG:350-364), sin cambio de comportamiento;
- **la partición de las copias por su resultado en el RED** (§5.2; la T-1 de la §16.4), técnica.


---

## 9. Límites

- Nada se compiló ni se ejecutó, y el worktree no se leyó: todo es por objetos git en `7f0d9aa3`.
- **Las listas son recuentos por texto** (el método y los ayudantes de su archivo, a un nivel) **y, para la retirada, por referencia de código transitiva.** Pueden contar de más (un nombre común que casa en otro sentido) o de menos (una dependencia por reflexión o por un nombre que no se ve en el texto). La de PB-T4b es la que aprobó el Owner (470); la de PB-T4d es un esbozo que se vuelve a medir.
- **Las guardas de PB-T4a se recorrieron enteras por diferencia** sobre el texto literal del GREEN que prescribe este paquete (§4.1), y las 100 señaladas se leyeron una a una. El recorrido no ve una prueba que lea el texto sin dejar en su código ninguna ruta, carpeta ni ayudante, ni la reflexión sobre tipos que no se miraron: es el riesgo declarado de PBT4A-C16. CA-T4a vuelve a leer las señaladas, y la precondición 6 compila el esqueleto y corre la suite.
- **El léxico de C# de las mediciones** (`lists_core.py`) se corrigió en la v4.2 para las cadenas verbatim que empiezan por una comilla escapada (`@"""…`), que tomaba por cadenas crudas: tres archivos de las listas las tienen (EIG, RG y `WorkspacePilotUpdateGuardTests`). **Las listas y sus sha256 no cambian** (comprobado: `pbt4-substitution-lists.tsv` sigue en `f24c2dbc…`).
- **Quedan para la copia exportada:** K, D y E de PB-T4b y PB-T4c, la partición de las copias en el RED, la letra de C-5 sobre el GREEN de PB-T4b, las líneas de CT-PBT4B-06, el coste de `IsDirty` y la duración de la suite de Core.
- **El miembro del informe de reconciliación** para la condición 7 se nombra como el que lee CT-PBT1B-16. La línea base va por `SetWorkingBaseline`.

## 10. Lo que queda para PB-T5

- El Smoke-PB, con la lista de §5.5.
- La CI exacta del Candidato.
- El gate-close de F4B.

## 11. Las precondiciones de emisión

1. **PB-T3c CERRADA:** cumplida (`7f0d9aa3`, F4B-E48).
2. **Las respuestas del Owner:** cumplidas (`owner-pbt4-decisions.md` y `owner-pbt4b-retire-list-decisions.md`), con su número F4B-E al custodiarlas.
3. **La comprobación acotada** de cada subtarea: 0 bloqueantes y 0 importantes sin resolver (pregunta 4).
4. **Las celdas, la Snapshot y el binario,** vigentes.
5. **La remedición sobre el cierre de la subtarea anterior** no cambia nada material (pregunta 4).
6. **La comprobación sobre una copia exportada:**
   - el esqueleto exacto de la subtarea compila;
   - la suite completa de Core y los filtros de UI dan lo previsto (PB-T4a: **19 129 = 19 122 + 7** con el esqueleto y el `Skip`);
   - el recorrido de las guardas sobre el texto literal del GREEN (PB-T4a: §4.1, 1 026 pruebas, una que cae, la de la lista) y las aserciones traducidas (`measure_pbt4_v42-out.txt`, 35 de 35);
   - el barrido de nombres y de sobrecargas;
   - en PB-T4b y PB-T4c, además: K, D y E, la partición del RED de las copias (PB-T4b), las líneas de CT-PBT4B-06, el coste de `IsDirty` y la duración de la suite.
7. **La lista que se emite es la aprobada, o una parte:** PB-T4a, 1; **PB-T4b, las 470 de `pbt4b-substitution-list.tsv`** (sha256 `44393b19d1f7490081c8ec9bac633ea4acbfe792103e5e5c3ca5e1f3fa1163e4`), que el generador comprueba; PB-T4c, 2. **PB-T4d no se emite por la autorización condicional:** vuelve al Owner con su lista.
8. **La custodia,** en el commit de emisión de PB-T4a:
   - este paquete (como `docs/initiatives/I-64-f4b-pbt4-gate-contract-package.md`), las versiones v1 a v4.1, el registro de mediciones, la revisión y su encargo, las instrucciones de cada versión, los guiones y sus salidas, las listas (`pbt4-substitution-lists.md` y `.tsv`, `pbt4b-substitution-list.tsv`, `pbt4d-retire-list.tsv`, `pbt4-copy-map.tsv` y `pbt4-retire-compile.tsv`), el recorrido de las guardas (`pbt4a-guard-walk.tsv`, `pbt4a-guard-walk-out.txt` y `pbt4a-guard-walk-flagged.txt`) y los fragmentos de `v41/` y `v42/`;
   - los dos registros del Owner (`F4B-DRAFTS/coordinator/owner-pbt4-decisions.md` y `owner-pbt4b-retire-list-decisions.md`);
   - la entrada «## PB-T4a» de `superseding-characterization.md`.
9. **Para PB-T4b y PB-T4c:** la anterior CERRADA y su remedición. **Para PB-T4d:** el cierre de PB-T4c, su lista vuelta a medir y la respuesta del Owner.

