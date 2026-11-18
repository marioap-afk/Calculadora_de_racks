> **Redacción para el repo público:** sin rutas locales ni nombres de usuario o de máquina. El worktree se cita como `<worktree>` y las rutas del repo son relativas.

# I-64 F4B — Revisión adversarial independiente del paquete del contrato de PB-T1, v1 (IRT1-1)

**Veredicto:** **EMITIBLE TRAS CORRECCIONES — 0 / 8 / 15**, con **una decisión material nueva para el Owner (L-1)**. Por esa decisión el paquete **no cabe en la autorización condicional** de F4B-E09 (pregunta 4): vuelve al Owner antes de emitir PB-T1a.

## 1. Objeto, método y límites

**Objeto.** El paquete `I-64-f4b-pbt1-gate-contract-package.md` v1 (citado `T1:n`), su registro `pbt1-measurements.md` y los tres scripts que cita (`members.py`, `count_tests.py` y `forbidden_scope.py`), contrastados con:
- el encargo de redacción (`drafting-brief.md`);
- el expediente (`ob065-dossier.md`);
- las decisiones del Owner F4B-E09 (`owner-pbt1-pre-decisions.md`);
- la vuelta pendiente de PB-TC2 (`pbtc2/owner-return-c14.md`).

**Las autoridades,** leídas en `<worktree>` con HEAD `eedb069c`:
- [P] `docs/initiatives/I-64-f4b-push-back-completo-package.md`;
- [OD], [DP] y `f4b-dispositions.md` (F4B-E01 a F4B-E10);
- [PKC2] `docs/initiatives/I-64-f4b-pbtc2-gate-contract-package.md`;
- los cierres de PB-T0 y PB-TC;
- el paquete de T1B;
- `docs/AUTOMATION_PLAN.md` §16 y `docs/automation/agent-execution/`;
- el código y las pruebas.

**Método** (solo lectura):
- `git -C` (`rev-parse`, `log`, `grep`, `ls-files`, `show`), `sed` y `grep`;
- `gh run list` y `git ls-remote`, solo de lectura;
- dos scripts de Python de solo lectura, escritos con la herramienta de archivos y ejecutados con `python -I`:
  - el `count_tests.py` del paquete;
  - `review-tools/anchors.py`, junto a este informe, que imprime unas 230 anclas del paquete.

**Comprobado de identidad:** HEAD = remoto = `eedb069c`; `main` = `812baf87`; la CI `44246483921` sobre `eedb069c`, `success`.

**Límites:**
- Nada se compiló ni se ejecutó. No hay AutoCAD.
- El comportamiento de WPF (cómo compara `SelectedValue`) y los efectos de borde descritos en I-04 e I-05 son **lectura de código**, no medición.
- PB-TC2 no ha cerrado: las zonas [TC2] se revisaron sobre HEAD y sobre el diseño de [PKC2] (D-PBTC2-04 y D-PBTC2-05).

## 2. Los nueve puntos

### 2.1 Fidelidad a [OD], a la fase y a F4B-E09

- **«module state preserved», «parked queue preserved», «A/B preserved».**
  - El copiador (T1:204-220) lleva la sesión, el commit, la firma, la cola con su marca de consumo y el lado B con `sideBEverDeclared`.
  - El mecanismo es correcto:
    - `CS.Snapshot`/`Restore` (CS:1117-1167) no lleva la cola ni la selección múltiple, y el paquete las repone aparte;
    - `MarkDormantTailConsumed` (CS:116) reproduce la marca, porque una marca pendiente implica una cola no vacía.
  - **Cumple.**
- **«no aliasing».** **No cumple en sentido literal: es la lectura L-1.** El copiador comparte por referencia `workingBaseline`, un `PushBackSystem` mutable (`PushBackSystem.cs:18-44`). Tres hechos que el paquete no recoge:
  - **(a) El premisa de «no hay alternativa» es inexacta.** El producto ya copia un sistema resuelto con el viaje snapshot → resolve (`CopyStructureSystem`, DA:506-529), y `PushBackResolver` expone `Snapshot(PushBackSystem)` y `Resolve(design)` (`PushBackResolver.cs:41`, `:387`). Esa vía tiene una dificultad conocida con las secuencias compuestas (DA:520-527, `AllowsNonNestedDepthRanges`). Es una opción real, no una tarea de Domain.
  - **(b) La medición D-5 solo miró los usos de `WorkingBaseline`** (6 líneas). El mismo objeto viaja como `lastComputation.System`, `session.System` (W:674-680), en las peticiones de inserción (W:3357, :3410, :3415), en la vista previa, en `Cortes` (W:3348) y en `RefreshCompositePanel`/`CompositeHasBlocking` (W:707, :711). Nada de eso se midió.
  - **(c) La cola dormida del original apunta a objetos de esa misma línea base.** `ParkTailOf` (CA:180-240) aparca `HeaderConfiguration = module.AssociatedFrameConfiguration` (`PushBackCompositeStructure.cs:800-811`), las líneas del baseline y las intenciones del commit, todo por referencia. HT1-12 (T1:88) dice que el alias de la carga es «el único que dura toda la sesión», y no lo es. Además, la exención de CT-PBT1B-07 («los derivados compartidos y lo que ellos alcanzan», T1:458) **cubre esos `RackFrameConfiguration`**: una copia superficial de la cola aparcada desde el baseline no se detectaría.

  Por eso L-1 es **material** (§2.9) y lleva el hallazgo I-01.
- **«no WPF control becomes source of truth».**
  - Con la definición operativa de H-PB-44 (relectura sin edición que cambia el estado), el diseño de PB-T1c la cumple para el catálogo entregado: c-02 por regla y las de HT1-03 con siembra igual a lectura.
  - Pero D-PBT1-13 (5) mantiene la relectura **incondicional** de esos controles hacia el estado en cada pasada (T1:314-321). El estado es la autoridad solo mientras los controles estén sembrados desde él. I-04 muestra una ventana en la que no lo están.
  - La cita «la de H-PB-44, aceptada en [OPD] P4» (registro E) no es literal: la pregunta 4 de `owner-pbtc-decisions.md:73-93` decide el alcance, no una definición (M-13).
- **«state round-trip lossless for supported classic racks».**
  - CT-PBT1B-02 (T1:453) es la prueba correcta.
  - Para los 6 heredados, la garantía es «salvo la canonicalización T2H-07» (`LegacyDeltas`). Es aceptable, porque CT-PBTC-06 iguala la ventana y el piloto en los 14 diseños, pero debe **declararse** como lectura de la garantía (M-01).
- **«classic RACKEDITAR still works through the extracted state».** PB-T1c, con CT-PBT1A-14 como red. **Cumple** con las reservas de I-03 e I-04.
- **MVVM o estética.** No hay ninguna. Los reenvíos con el mismo nombre (T1:298-325) son el cambio mínimo.
- **PB-T2, PB-T3 y PB-T4.** No se cuelan: sin modo WORKSPACE, sin factoría y sin tocar el piloto (T1:329-336).
  - Una observación: el **asiento de la computación** (W:674-702: `AcceptComputation`, `CommitDormantTailConsumption`, `ClearModuleCommit` y D-PB-11) se queda en la ventana (T1:202).
  - Eso obliga a las pruebas de paridad a duplicarlo (I-03), y a PB-T3 y PB-T4 a hacer lo mismo sin una API.
  - No es una decisión del Owner. Se recoge en I-03.
- **OB-03.1.** Se respeta: `Pilot/` no se toca, y CT-PBT1B-04 fija que el piloto sigue rechazando `pb-21-cola` con `DormantTailOutOfPilotReason` (PM:108, PM:160-170).

### 2.2 La división en tres subtareas (L-3)

- **Justificada.** Son tres regímenes de RED distintos (mutantes, esqueleto inerte y costura inerte). La red va en la CI antes de mover el estado, y el tamaño la justifica (T1:126-130).
- **No cambia el plan de un modo que deba decidir el Owner:**
  - [OD] D-PB-10 dice «Each separately contracted PB task starts at 1/3»;
  - [P] OB-10.1 prevé «y sus subtareas si se dividen» ([P]:470);
  - el contenido, el orden y las dependencias de la fase no cambian.
- **El procedimiento** (una revisión para las tres y después comprobaciones acotadas, T1:750) **es compatible** con [P]:515 («paquete con revisión independiente» por tarea), porque el paquete revisado contiene los tres contratos. [DP] no regula las revisiones. Falta definir la comprobación acotada (M-12).
- **¿Cada una es cerrable por sí misma?**
  - PB-T1a solo deja `tests/`.
  - PB-T1b deja tipos de Application sin uso de producto.
  - Entre PB-T1b y PB-T1c hay **tres copias** de la carga compuesta (la ventana, el estado y el piloto), contra ADR-0029 D12. R-T1-17 (T1:719) solo cuenta dos. Falta qué pasa si PB-T1c no cierra (M-12).
  - **Hueco de autoridad (I-08):** PB-T1a fija como «INCUMPLIMIENTO CARACTERIZADO» lo que encuentre (T1:382), sin una regla que pare antes de emitir PB-T1b si la red descubre una pérdida nueva o un control fuente de verdad. OB-06.5 manda volver al Owner en ese caso.

### 2.3 PB-T1a: la red por mutantes

- **Los 15 textos de la tabla** (T1:414-428) están **literalmente** en HEAD: W:874, :852, :817, :827, :2133, :3686, :696, :1510, :1171, :3668, :2682, :3568, :313 y :419, y WC:209.
- **Discriminación, comprobada por lectura mutante a mutante:**
  - MU-T1A-14 solo discrimina gracias a `pb-08-redondeo` (`AnnotationScale` 1.23456): con los demás fixtures, `Keep` devuelve la caja.
  - MU-T1A-07a discrimina en celdas porque `KeepCell` llama a `Keep` ([PKC2] D-PBTC2-05).
  - MU-T1A-12 discrimina porque el aviso de D-PB-08 no bloquea el BOM (CT-PBTC-08).
  - MU-T1A-06 hace `ApplyDerivedPostHeightToLines(null, …)`, que restaura las líneas (W:2134).
- **Sin parada para un mutante que no discrimina:** falta PBT1x-C04 (I-06).
- **Zonas [TC2].** Las de MU-03, 04, 07a y 10 están bien marcadas. CT-08, de MU-08, depende del **comportamiento** de OWNER-1 de PB-TC2 aunque W:1510 no cambie de texto: se mide en la `BaseSha`, así que vale.
- **Cobertura:**
  - D-5 (los controles de §3.1 del expediente), M-04 (CT-09 sobre la salida), M-07 (CT-10), la observación de CT-16 (CT-11, con el BOM), M-02 de PB-TC (CT-12) y D-3 (CT-13): **sí**;
  - **M-03 de PB-TC:** CT-07 no nombra «la ruta de los portadores del rack» (cierre de PB-TC, M-03; M-04 de este informe);
  - **M-01 de PB-T0:** su cierre lo asignaba como «cambio declarado del ayudante». El paquete deja S0 intacto, que es lo correcto, pero no debe darlo por cubierto con CT-13: la fuga de `Abrir` sigue en CT-PB0-17. Además, CT-11 usa `AbrirFueraDePantalla`, que carga fuera del `try` (`PushBackFullEditorCharacterizationSupport.cs:139-154`), contra la regla de SC1 (`PushBackDeclaredCorrectionSupport.cs:28`) (M-03).
- **Tautologías:** ninguna en PB-T1a, porque los mutantes son su antídoto. CT-14 pinta huellas de salida y vale para «sin cambio observable», pero no ve divergencias que la ventana reabsorbe en la pasada siguiente (I-04).
- **Detalles:** X-01 no está en S0. Es `BuildX01`, privado, en `PushBackFullEditorCharacterizationStateTests.cs:90` (M-02). El guion G-2 («Actualizar») cierra la ventana, y el orden de G-3 a G-6 sobre la misma ventana no está fijado (M-13).

### 2.4 PB-T1b: el tipo en Application

- **El contenido y las cargas** (T1:144-192) reproducen W:270-324, W:385-440 y WC:827-934.
  - **Cola copiada al cargar (I-05).** T1:180 la copia «con la copia de `CopyIntent` de RMES». `CopyIntent` es **privado** (RMES:799) y **normaliza** la procedencia (`ResolveProvenance`, RMES:782-783, :806-807). El despertar de hoy copia **literal** (DA:557-566). Con un módulo de cola «personalizado sin configuración» (documento editado), el diseño de salida puede cambiar. Contradice «sin efecto observable» (T1:180 y T1:756).
- **El copiador y el comparador:**
  - **El censo sin alias es inalcanzable (I-02).** CT-PBT1B-07 (T1:458) exige que la intersección sea «exactamente» la de los derivados compartidos y que el estado recién cargado no comparta «ningún objeto» con el diseño. `SelectiveSafetySelection.DeepCopy` comparte `SafetyFacePiece`, inmutable y con un `None` estático (`SafetyFacePiece.cs:26-38`; `SelectivePalletDesign.cs:605-606`). El censo del piloto ya lo admite (`WorkspaceDraftPilotStateTests.cs:1078-1093`, «the immutable face pieces»).
  - **El comparador y la sesión perezosa (I-03).** «la sesión, con `SameEditAs`, o las dos null» (T1:229). La ventana abre la sesión en cada pasada (`RefreshModuleSelector`, W:704 → W:1688 → ES:124-133), y un estado solo no la abre. Además, con commit el comparador no lee la línea base (T1:228), y dos estados con el mismo commit y bases distintas salen iguales (M-15).
- **El mapa de propiedad (I-07).**
  - D-PBT1-08 (T1:246-261) no fija la API: ni `Of`, ni el enum con `Unclassified` del esqueleto (T1:441), ni la forma de las rutas de hoja ni sus índices. INV-PBT1B-03 (T1:657) exige exactitud igualmente.
  - Falta la hoja `$.Design` del sobre.
  - Los campos que la ventana solo transporta (`FirstLevelDatum`, `DimensionStyle`) quedan como `Editor` sin justificación frente a «lo que no edita la ventana» (OB-05.1). D-PB-05 («FULL EDITABLE DESIGN wins») lo justifica, pero hay que escribirlo (M-14).
- **c-02 (L-2).** Ordinal, como hoy: **no es material.**
  - Con el catálogo entregado (un poste), la diferencia solo se ve en `PostBox.SelectedId`, y el paso 1 de PB-T1b la mide con PBT1B-C14 (T1:661, :664).
  - Faltan las filas esperadas de (a) a (e), literales.
  - La regla compara contra `catalog.PostProfiles`, pero `PostBox` ofrece ids **recortados y sin duplicados sin mayúsculas** (`UiSupport.cs:124-133`). Con el respaldo nuevo `PostBox.SelectedId ?? editable.PostCatalogId` (T1:318), un catálogo editado con espacios o con duplicados de mayúsculas cambiaría la salida (M-05).
- **`pb-21-cola`.** `SupportOf` es la misma condición del piloto (PM:148-170). **Correcto.**
- **`PushBackEditorOutputGate`.** Fiel a WC:783-807. El oráculo de CT-PBT1B-12, «WC:783-807, copiado» (T1:463), es tautológico (M-07).
- **El RED con esqueleto inerte:** falla por aserción en las 13. Un `PushBackEditorState` nuevo tiene un frente por defecto (`DynamicFrontMatrix.cs:24-30`), así que calcular desde el esqueleto no debería lanzar, y además el ayudante «EXCEPCION» lo cubre.
- **El censo del piloto.** Se respeta: `CopyAuthorityFrom` y los métodos de RMES no añaden campos, y la tabla del piloto (`WorkspaceDraftPilotStateTests.cs:213-369`) no recorre RMES. `HeaderBatchContractTests` C-10 tampoco se ve afectada: `Copy`, `SameEditAs` y `SameAs` no empiezan por un verbo de motor (`:524-537`).

### 2.5 PB-T1c: la ventana sobre el estado

- **«Sin cambio observable» es verificable con las huellas** de CT-PBT1A-14, salvo en un camino.
  - **El orden de la carga (I-04).** D-PBT1-13 (3) adelanta la adopción de los portadores (dentro de `editable.LoadDesign`/`Restore`) a **antes** de `UpdateCompositeEnabled()`. (4) la quita de `LoadFromModel` (T1:307-313).
  - `UpdateFrontalButtons` puede reponer `FrontalSideBox` a 0 (WC:117-121). Eso dispara `FrontalSide_Changed` → `RequestRecompute()` (W:3277-3289), síncrono (`RecomputeGate.cs:35-44`) y fuera de `suppressSync`.
  - Esa pasada reentrante escribe en el estado los controles **todavía sin sembrar**, y `LoadFromModel` ya no vuelve a adoptar.
  - Camino alcanzable: «Restaurar» con B declarado sin frentes activos (el diseño válido sale de un sentido), «Frontal de B» elegido y un último cálculo inválido tras editar una caja del rack. El valor restaurado de más de 3 decimales se **redondea** en la pasada final (`Keep`, W:3685-3686).
  - Hoy no pasa, porque `LoadFromModel` reasigna los portadores. CT-14 no lo ejerce: F-02 es compuesto y no repone el selector.
  - R-T1-13 (T1:715) lo vio, pero da por buena la mitigación «el mismo punto relativo».
- **Los reenvíos** (T1:298-325). Conservan las 40 costuras (`internal`, W:165-193 y WC:43), `FirstLevelDatumForTest` (W:383) y `FrontalSideForTest` (W:3264). Ninguna prueba nombra los campos (medición H).
  - Comprobado aparte: la única prueba que recorre campos de la ventana por reflexión, `PushBackRichPreviewTests.cs:348-367`, solo exige un campo `EditorPreviewSurface` y métodos que no se tocan. Sigue en verde, aunque el §4 no la lista (M-13).
- **Las guardas:**
  - **S-9** (`…SourceGuardTests.cs:293-298`), **CT-22** (`:323-341`, por índices, así que admite `SyncName();` entre sentencias), **CT-23** (`:356-379`) y **CT-19** (`:235-264`): en verde con INV-PBT1C-04 y 05.
  - **CT-25** (`PushBackClassicCharacterizationPluginGuardTests.cs:235-272`): el Plugin no se toca.
  - **G-10** (`WorkspacePilotUpdateIntegrationGuardTests.cs:721-797`): sin `.RequestUpdate(` nuevo.
  - **Fact3** (`PushBackModuleEditorCharacterizationTests.cs:382-401`): en crudo, sin `forceRebuild:` nuevo.
  - **I-55** (W:3135 y :3224) y el **desviador** (W:3044 y :3048): no se tocan.
  - El censo de ventanas, WLC, RAM y el núcleo de borradores: restricciones que se respetan.
  - El censo de tareas (`WorkspacePilotUpdateIntegrationGuardTests.cs:807-812`) no mira `Systems/PushBack` fuera de `Pilot/`.
  - **Ninguna exige volver al Owner por OB-T1B.3.**
- **CT-PBT1C-01** (T1:475) compara el estado de una ventana **ya recalculada** con un estado «solo con la misma carga». Diferirían por cuatro razones (I-03):
  - `NormalizePeraltes` y el saque (DA:155-159);
  - la línea base aceptada;
  - la marca de cola;
  - la sesión perezosa.
- **CT-PBT1C-02** discrimina portadores olvidados, pero no la ventana de I-04.

### 2.6 Los contratos propuestos

- **Los alcances** (9, 11 y 4 rutas) y los `ForbiddenWriteScope` de `src/` (59, 117 y 59): coherentes con `forbidden_scope.py` y con la nota de CS.
- **Los recuentos estáticos,** comprobados con `count_tests.py`:
  - UI: `PushBack` 538, `RichEditor` 25 (+4 `Skip`), `Workspace` 547 y `Cantilever` 67;
  - Core: `Workspace` 3 271 y `RackModuleEditSessionTests` 25.
  - La línea base de la CI (2 422 → 2 436 → +1 → +2; Core 19 070 → +12 → +1) cuadra.
- **`RequiredTests`:**
  - a PB-T1b le faltan `NamespaceFolderGuardTests` y `HeaderBatchContractTests`, que escribe en `src/` y en Shared (M-10);
  - el solape del filtro UI `PushBack` de PB-T1c con sus propias pruebas `ExpectRed` no rompe `Tests`: el `RedPart` solo exige que las `ExpectRed` estén entre las fallidas (`AUTOMATION_PLAN.md:620`).
- **Las paradas:**
  - **Falta PBT1x-C04 (I-06).** §6.1 dice «las comunes C01 a C10» (T1:634), pero la lista (T1:595-603) salta de C03 a C05. [PKC2] PBTC2-C04 la tenía.
  - **Las de [DP], literales:** comprobadas contra `f4-dispositions.md:520`, `:562`, `:1103-1106`, `:1540` y `:1657-1660`, y contra `owner-f4t3-authorization.md:255-265`. Los sustitutos existen en las dos rutas citadas.
  - La variación de nc1 de PB-TC está como riesgo (R-T1-08).
  - Falta el régimen de corrección del RED por mutantes (§16.8: una corrección que toca `ChainRedFiles` vuelve a aplicar los 15). Falta citar la lectura de `Ci` `RedPart` con Core en `success` (F4-E56; [PKC] :466). PB-T1c no hereda la regla «EXCEPCION» de PB-T1b (M-11).
- **Las celdas.** El Controller lleva «VerifiedOn 2026-11-11» (T1:585), pero `model-catalog.md:97` dice 2026-09-30, que es lo que da el `STALE` del 2026-12-29. Hay que citar `:51` y `:97`, como [PKC2] :655 (M-13).
- **INV-PBT1B-05** («ningún archivo nuevo nombra… `Pilot`, `RackCad.UI`», T1:660) choca con CT-PBT1B-01 (UI) y CT-PBT1B-04 (`PushBackPilotMaterializer`) si incluye las pruebas. Hay que limitarlo a `src/` (M-06).

### 2.7 Las lecciones de PB-T0, PB-TC y PB-TC2

- **Recogidas:**
  - sin latitud (R-T1-01);
  - rutas relativas y `TreeSha` real (INV-11);
  - `Assert.Fail` (INV-10);
  - el aviso de descarte sustituido con el sufijo leído tras cerrar (INV-08, como D-PBTC2-08);
  - nunca la suite completa de UI (INV-10);
  - MAX_PATH: sin copias de árboles, y `TaskId` de 28, 29 y 31 caracteres;
  - sin choques: prefijos libres (medición I) y `SUS3A/B/C`.
- **Contra la lección «sin latitud»,** el paquete deja al Worker:
  - la API del mapa (I-07);
  - la tabla del censo de CT-PBT1B-06, con la clase «transitorio del armado» sin definir (M-08);
  - la «pasada» del estado en las pruebas de paridad (I-03);
  - las recetas de X-01, F-01 y F-02 en Core, que no tiene ventana (M-02);
  - la copia de `RackModuleCommit`, que no puede usar los privados de RMES (M-09).
- **La excepción** de `AbrirFueraDePantalla` (M-03).

### 2.8 Las anclas

Unas 230 anclas, con `review-tools/anchors.py` y lectura directa. **Se sostienen** todas las de W, WC, CS, ES, EL, DA, CA, PC, PM, RMES, `RackBomOutputGate`, `RackEditorIdentity`, `DynamicRackLevelGeometry` y las guardas de §4, salvo:
- CT-PBT1B-02 cita CT-PB0-01 como oráculo de los 8 canónicos. CT-PB0-01 solo cubre `pb-20-ab`, `pb-21-cola`, `pb-31-v2-modulos` y X-01 (`PushBackFullEditorCharacterizationStateTests.cs:115-168`). La cadena real es CT-PBTC-06 (`PushBackDeclaredCorrectionPilotParityTests.cs:20-26`) más los oráculos del piloto (M-01).
- «X-01 de S0» (M-02).
- `WorkspaceDraftPilotIntentGuardTests.cs:232-247`: el archivo tiene 245 líneas.
- El alias `DDA` (T1:256) no está definido en §2.0.
- HT1-12, «único alias» (I-01).
- La fecha de la celda del Controller (M-13).

### 2.9 Las decisiones materiales nuevas

- **L-1 es material.**
  - Es una excepción a un requisito duro literal («no aliasing»), apoyada solo en lectura, con la medición incompleta y una alternativa omitida (§2.1).
  - Vuelve al Owner.
- **L-2 no es material.** Es «como hoy», con medición en la base y una parada (PBT1B-C14), que es lo que manda «RACKEDITAR no cambia».
- **L-3 no es material.** OB-10.1 y D-PB-10 la prevén. Se informa en la emisión con D-PBT1-01.
- **M-07 de PB-TC2 reproducido** (D-PBT1-12). No es material: no es una clase de pérdida, porque el estado no cambia (HT1-18), y OB-RE.1 manda no cambiar RACKEDITAR. Conviene decir en D-PBT1-12 por qué no aplica OB-06.5, para cerrar «con una posible vuelta por OB-06.5» de [PKC2] §10.

## 3. Hallazgos

| Id | Gravedad | Subtarea | Contenido | Evidencia | Corrección |
|---|---|---|---|---|---|
| I-01 | IMPORTANTE | PB-T1b | **L-1 con premisas inexactas.** La alternativa omitida es el viaje snapshot → resolve. La medición D-5 no miró los demás alias del mismo objeto. HT1-12 se equivoca: la cola aparcada por `ParkTailOf` apunta a objetos del baseline. La exención de CT-07 ciega la comprobación de la copia de esa cola | T1:88, :214, :458, :745-748; DA:506-529; `PushBackResolver.cs:41`, `:387`; W:674-680, :3348, :3357, :3410; CA:180-240; `PushBackCompositeStructure.cs:800-811` | Reescribir L-1 con las tres opciones de §4 y la medición completa de los consumidores. Sea cual sea la decisión, CT-07 comprueba **por separado** que la cola, el commit y la sesión de la copia no comparten objetos con el original, sin la exención |
| I-02 | IMPORTANTE | PB-T1b | **CT-PBT1B-07 inalcanzable.** «Exactamente» y «ningún objeto con el diseño» fallan en el GREEN con cualquier seguridad que lleve caras, por los `SafetyFacePiece` inmutables compartidos por `DeepCopy` | T1:458; `SafetyFacePiece.cs:26-38`; `SelectivePalletDesign.cs:605-606`; `WorkspaceDraftPilotStateTests.cs:1078-1093`, `:531-537` | Excluir los objetos inmutables con el `IsImmutable` del censo del piloto y nombrar los diseños de la prueba |
| I-03 | IMPORTANTE | PB-T1b y PB-T1c | **La paridad del estado no está definida.** CT-PBT1C-01 compara una ventana recalculada con un estado solo cargado. El comparador trata como distintas la sesión nula y la abierta de forma perezosa. La «pasada» (W:659-702) no existe en la API y las pruebas la tendrían que inventar | T1:202, :229, :452, :475; W:704, :1688; ES:124-133; DA:155-159 | Fijar un ayudante de prueba que copie W:659-702 literal (o una API de asiento) y el número de pasadas de cada camino. En el comparador, sesión nula ≡ sesión recién abierta sin cambios pendientes |
| I-04 | IMPORTANTE | PB-T1c | **El orden de la carga cambia.** Los portadores se adoptan antes de `UpdateCompositeEnabled()`, cuya pasada reentrante escribe en el estado controles sin sembrar, y `LoadFromModel` ya no reasigna. Resultado: el redondeo silencioso de un valor restaurado en «Restaurar» | T1:307-313, :715; WC:117-121; W:3277-3289, :3685-3686; `RecomputeGate.cs:35-44` | Adoptar los portadores donde hoy, dentro de `LoadFromModel` y bajo `suppressSync` (exponer `AdoptCarriers`, o que la carga no los adopte en el camino de la ventana). Añadir a CT-14 el guion F-03: B declarado sin frentes, «Frontal de B», un cálculo inválido y «Restaurar» |
| I-05 | IMPORTANTE | PB-T1b | **La copia de la cola al cargar no es literal.** `CopyIntent` es privado y normaliza la procedencia, mientras el despertar copia literal. Con un documento editado, el efecto puede ser observable | T1:180; RMES:782-783, :799-811; DA:557-566 | Especificar una copia campo a campo literal, con `RackFrameProjectStore.DeepCopy` para la cabecera y las líneas, igual que DA:557-566 |
| I-06 | IMPORTANTE | las tres | **Falta la parada C04.** Para PB-T1a: un mutante que no hace fallar por aserción su CT, o un CT que falla en la base sin mutantes. Para PB-T1b y PB-T1c: un `ExpectRed` que pasa en el RED o falla por excepción | T1:595-603, :634; [PKC2] PBTC2-C04 | Añadir PBT1x-C04 con el texto propio de cada régimen |
| I-07 | IMPORTANTE | PB-T1b | **La API del mapa no está fijada** (`Of`, el enum, `Unclassified`, la forma de las rutas y los índices), aunque INV-PBT1B-03 exige exactitud. Falta `$.Design` del sobre | T1:246-261, :441, :462, :657 | Dar las firmas, la normalización de las rutas y la fila de `Design` |
| I-08 | IMPORTANTE | la división | **Sin parada de autoridad entre subtareas.** Una pérdida nueva o un control fuente de verdad que descubra PB-T1a se fija y PB-T1b se emite igual | T1:382, :25-30; [P] OB-06.5 ([P]:392-394); F4B-E09 («efecto material sin medir») | Precondición de PB-T1b: ninguna línea de PB-T1a con una pérdida (clase c, d o nueva) ni un control que decida el estado. Si la hay, vuelta al Owner |
| M-01 | MENOR | PB-T1b | El oráculo de los canónicos no es CT-PB0-01. Para los heredados, «sin pérdidas» quiere decir salvo T2H-07, sin declararlo | T1:453; StateTests:115-168; ParityTests:20-26; MaterializationTests:706-735 | Citar CT-PBTC-06 y declarar la lectura |
| M-02 | MENOR | PB-T1a y PB-T1b | X-01 es privado de las pruebas de estado. Core no tiene ventana para fabricar X-01, F-01 y F-02 | T1:408, :448; StateTests:90 | Dar las recetas (o copiarlas literal) |
| M-03 | MENOR | PB-T1a | CT-11 usa `AbrirFueraDePantalla` (carga fuera del `try`). M-01 de PB-T0 se da por cubierto y no se corrige | T1:386, :407; S0:139-154; SC1:28 | Repetir su preparación con `Ventana()` y la carga en el `try`. Declarar M-01 como no corregido |
| M-04 | MENOR | PB-T1a | La «ruta de los portadores del rack» de M-03 de PB-TC no está explícita en CT-07 | T1:401; cierre de PB-TC, M-03 | Añadir el caso |
| M-05 | MENOR | PB-T1b y PB-T1c | La regla de c-02 compara contra otro conjunto que `PostBox`, y el respaldo nuevo lo materializa. Faltan las filas esperadas de (a) a (e) | T1:265-270, :318, :661; `UiSupport.cs:124-133` | Comparar con los ids recortados y sin duplicados de `ToOptions`, y transcribir las filas esperadas |
| M-06 | MENOR | PB-T1b | INV-PBT1B-05 choca con CT-01 y CT-04 si cubre las pruebas. Las anclas de línea como dato de producto envejecen | T1:660, :261 | Limitarlo a `src/`. La «Fuente», con miembro y archivo, sin línea |
| M-07 | MENOR | PB-T1b | El oráculo de CT-12 es una copia del código | T1:463 | Líneas literales, o los casos bloqueantes en CT-01 |
| M-08 | MENOR | PB-T1b | Ni la tabla del censo de CT-06 ni la clase «transitorio del armado» están en el paquete | T1:457 | Darlas |
| M-09 | MENOR | PB-T1b | `RackModuleCommit.Copy` no alcanza los privados de RMES. Faltan las firmas de `Copy` e `Instance` | T1:240-243, :440 | Copia literal; ayudantes privados permitidos; firmas |
| M-10 | MENOR | PB-T1b | A los `RequiredTests` les faltan `NamespaceFolderGuardTests` y `HeaderBatchContractTests` | T1:642-652 | Añadirlas |
| M-11 | MENOR | las tres | Sin el régimen de corrección del RED por mutantes (§16.8), sin la lectura de `Ci` con Core en `success`, y sin la regla «EXCEPCION» en PB-T1c | T1:628, :633, :468-469 | Escribirlos |
| M-12 | MENOR | la división | La comprobación acotada sin definir; la custodia de los contratos de PB-T1b y PB-T1c; el modelo paralelo interino y la salida si PB-T1c no cierra | T1:30, :750, :719 | Definirlos |
| M-13 | MENOR | — | Anclas y citas: `IntentGuardTests:232-247`; `DDA`; la celda del Controller frente a `model-catalog.md:97`; «[OPD] P4»; `RichPreviewTests:348-367` fuera del §4; el orden de los guiones de CT-14 | T1:256, :360, :408, :585; registro E | Corregirlas |
| M-14 | MENOR | PB-T1b | Los campos transportados como `Editor`, sin justificarlo frente a OB-05.1 | T1:256 | Justificarlo con D-PB-05 |
| M-15 | MENOR | PB-T1b | Con commit, el comparador no lee la línea base | T1:228 | Declararlo como límite o compararla |

## 4. Veredicto

**EMITIBLE TRAS CORRECCIONES — B/I/M = 0/8/15.** No se emite por la autorización condicional: hay **una** decisión material nueva.

**La decisión para el Owner, L-1:** «no aliasing» y la línea base resuelta que la copia del estado comparte.

- **(a) Aceptar la excepción declarada** (recomendada). La copia comparte la línea base y el informe de reconciliación, que el producto ya comparte hoy entre la ventana, la sesión y el estado. Las condiciones:
  - la exención se limita a esos dos objetos;
  - CT-07 comprueba aparte la cola, el commit y la sesión;
  - CT-08 se extiende a los consumidores medidos;
  - PB-T3 y PB-T4 heredan la obligación de no mutarla (arrastre A-2).

  **Por qué:** coste cero, sin cambio de comportamiento, y la alternativa (c) tiene un riesgo conocido con los racks compuestos.
- **(b) Una tarea previa de Domain:** la copia profunda de `PushBackSystem`. Es cara y toca las configuraciones de cabecera con sus excepciones de tiempo de ejecución (I-17).
- **(c) Volver a resolver la línea base en la copia** (`Snapshot` → `Resolve`, el modo de DA:506-529), con una prueba de equivalencia. Cumple la letra sin Domain, pero la copia necesita el catálogo y hay que medir los racks compuestos (DA:520-527).

**Antes de emitir:**
1. La respuesta del Owner a L-1.
2. Las correcciones I-01 a I-08 y los menores que se acepten, en una v2.
3. Una comprobación acotada de esa v2.
4. El cierre de PB-TC2 con su diseño v2 (precondición 1). La vuelta C14 sigue abierta, y su opción (B) obligaría a volver a redactar el paquete.
