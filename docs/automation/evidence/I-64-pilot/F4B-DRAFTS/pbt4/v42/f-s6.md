## 6. Los contratos propuestos

### 6.0 Común (entero; V41-12)

**Identidad:**
- `Schema` `rackcad-gate-contract/v1`; `Unit` I-64; `Gate` F4B;
- `attempts`: cada subtarea empieza en 1 de 3 (OB-10.1); `RoutingEnforcement` `required`; `CorrectionsAuthorized` true;
- evidencia en `docs/automation/evidence/I-64-pilot/<TaskId>/`.

**Los `TaskId`:** con el prefijo **`F4B-PBT4-`** y **como mucho 26 caracteres** (V41-11; MAX_PATH solo acota por arriba): `F4B-PBT4-FULL-READ-PATH` (23), `F4B-PBT4-FULL-DRAFT-PANEL` (25) y `F4B-PBT4-EDIT-ALL-WINDOW` (24). El prefijo es el que liga `--task PBT4` en los sustitutos de nc1 y nc2 (`nc1_cascade_signal_substitute.py:205`; `nc2_exact_whitelist_substitute_f4b_v2.py:410`), así que se usan sin editarlos, con `--task PBT4` en las tres subtareas.

**Las colecciones opacas** (F4B-E21): las ocho (`Authorities`, `Objective`, `AllowedWriteScope`, `ForbiddenWriteScope`, `Invariants`, `RequiredTests`, `ExpectedEvidence` y `StopConditions`) las pone la sesión byte a byte desde el contrato. El Controller hace la clase, el enrutamiento y los criterios de aceptación. Cada relevo lo declara. La comparación byte a byte, A1-A8 y nc4 van sobre la delegación final.

**`ExpectedEvidence[0]`** es, **byte a byte**, el literal de PB-T3 de la lista cerrada del sustituto de nc2 (el `ExpectedEvidence[0]` del contrato de PB-T3c):

> {{EE0}}

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

{{STOPS_STD}}

**Las de [DP], literales, en cada contrato** (x = A, B o C, con el prefijo `PBT4x`):
- **PBT4x-C08:** `{{C08}}`
- **PBT4x-C12:** `{{C12}}`
- **PBT4x-C11, con los sustitutos de nc2 y nc1 vigentes:** el texto de `PBT3C-C11` del contrato de PB-T3c con `PBT3C` cambiado por `PBT4` (el `--task` de nc1 y nc2 es `PBT4` en las tres subtareas, y sus registros, `SATISFIED_UNDER_PBT4_…`) y «PB-T3c» por el nombre de la subtarea. El de PB-T4a, entero, en §6.1.1. nc3 no tiene sustituto (F4B-E46).

**El límite de D2, en cada contrato:** **PBT4x-C17,** con el texto literal del Owner (el de `PBT3C-C17`):

> {{C17}}

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
- **C01:** `{{C01}}`
- **C02:** editar, saltar, reactivar o borrar una prueba o un ayudante existente fuera de la lista cerrada de la subtarea; o tocar `InternalsVisibleTo`, los paquetes, un `.csproj` o `Fixtures/I64`.
- **C03:** un cambio observable de RACKEDITAR, o del panel fuera de lo declarado por la subtarea. STOP y vuelve al Owner.
- **C04:** `a method of a filter with ExpectRed passes on the skeleton, or fails by an exception and not by an assertion.`
- **C05:** `{{C05}}`
- **C06:** `{{C06}}`
- **C07:** el texto de PB-T3c, con las recetas que se autocomprueban de la subtarea, y la excepción de RR.
- **C09:** `{{C09}}`
- **C10:** lo que la subtarea excluye (D-PBT4-19 y su alcance).
- **C13:** la lista cerrada de declaraciones nuevas de la subtarea.
- **C14:** un diseño del corpus cambia de admisión o de bloqueo.
- **C16:** una prueba existente fuera de la lista cerrada de la subtarea falla. Vuelve al Owner (pregunta 4).
- **C17:** D2 (arriba).
- **C18:** `{{C18}}`

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
{{INV_COMMON}}

StopConditions (propias):
PBT4A-C01: {{C01}}
PBT4A-C02: it would require editing, skipping, re-enabling or deleting an existing test or helper, or touching InternalsVisibleTo, the packages, a .csproj or Fixtures/I64. Only one exception: the Skip of CT-PBT3A-12 in PushBackDraftWriteGuardTests.cs, with its literal reason. Any existing test that fails, outside that list, returns to the Owner.
PBT4A-C03: an observable change of RACKEDITAR appears, or of the pilot in the panel other than the declared one (the five rereads of an open draft go to the origin of the draft by its text, A-T3-8). STOP and return to the Owner.
PBT4A-C04: a method of a filter with ExpectRed passes on the skeleton, or fails by an exception and not by an assertion.
PBT4A-C05: {{C05}}
PBT4A-C06: {{C06}}
PBT4A-C07: {{C07_A}}
PBT4A-C08: {{C08}}
PBT4A-C09: {{C09}}
PBT4A-C10: doing the task would require what the scope of PB-T4a excludes: changing the type of the draft of the panel; beginning a draft from the full read; the window, Editar todo or Conservar mi borrador in two steps; the rule of the scale; creating views; touching the classic editor or RACKEDITAR; opening AUTH-13; or changing the signature of RackPilotIntentAdapter.Read.
PBT4A-C11: {{C11_A}}
PBT4A-C12: {{C12}}
PBT4A-C13: doing the task would require touching a file of src/ other than WEI, WHC, RIA, NH, IWorkspaceEditIntentOriginReader.cs and Draft/PushBackOriginByText.cs; or adding a declaration of any visibility (type, constructor, method, property, field, constant, enum member or nested type; an overload is one more) whose name is not in this closed list: in WorkspaceEditIntent.cs, the properties FullRead and Catalog of EditIntentReadResult and its method FromReads; in IWorkspaceEditIntentOriginReader.cs, the interface IWorkspaceEditIntentOriginReader and its method ReadIntentAtOrigin; in Draft/PushBackOriginByText.cs, the class PushBackOriginByText and its methods Choose, AtKey and ReadAt; in RackPilotIntentAdapter.cs, the second constructor of RackPilotIntentReadResult, its properties Full and Input, and the method AtOrigin of RackPilotIntentAdapter; in WorkspaceNavigationHost.cs, the method ReadIntentAtOrigin of the nested reader. Two changes of existing members are prescribed and are not new declarations: the private constructor of EditIntentReadResult gains two parameters, and ReadOnActiveDocument gains the parameter originText. In WorkspaceHostCore.cs, no new declaration: only the inline statement of D-PBT4-03 in five bodies (the pattern variable originReader is not a member). The Worker stops before adding it.
PBT4A-C14: a design of the corpus changes its admission or its lock (the pilot reader, the full reader, AUTH-13 or the parked tail).
PBT4A-C16: an existing test outside the closed list of substitutions of PB-T4a (CT-PBT3A-12) fails. It returns to the Owner (owner-pbt4-decisions.md, question 4).
PBT4A-C17: {{C17}}
PBT4A-C18: {{C18}}

ExpectedEvidence:
[0] {{EE0}}
[1] {{EE1}}
[2] {{EE2}}
[3] la tabla "CT -> la asercion que falla en el RED" y las autopruebas de CT-PBT4A-07 y CT-PBT4A-08;
[4] {{EE4}}
[5] {{EE5}}
[6] {{EE6}}
[7] {{EE7}}
[8] {{EE8_A}}
[9] {{EE9}}
[10] {{EE10}}
[11] el censo de declaraciones de WEI, WHC, RIA y NH, y de los dos archivos nuevos, en BaseSha y en CurrentSha, contadas una a una con su nombre (una sobrecarga es una declaracion mas): la diferencia es la lista cerrada de PBT4A-C13;
[12] {{EE12}}
[13] la entrada "## PB-T4a" de superseding-characterization.md, escrita por la sesion en el commit de emision, con el Skip de CT-PBT3A-12, su motivo literal y CT-PBT4A-06, que la sustituye; su tabla de ejecucion se completa al cerrar.
[14] Linea base de la CI: la del cierre de PB-T3c (F4B-E48): run 44757675821 sobre el GREEN 938a35f0, Core 19 129 resultados, con 19 123 Passed y 6 Skipped, los de D-1, y UI 2 451, con 2 430 Passed y 21 Skipped. Previstos, con el total exacto de cada rama (F4B-E46): PB-T4a anade ocho [Fact] y un Skip de un [Fact], sin ninguna Theory, asi que hay una sola rama. El RED: Core 19 137 resultados, con 19 125 Passed, 5 Failed (CT-PBT4A-01 a 04 y 07) y 7 Skipped. El GREEN: Core 19 137, con 19 130 Passed, 0 Failed y 7 Skipped. UI, en los dos: 2 451, con 2 430 Passed y 21 Skipped.
```

**`RequiredTests`** (38): las dos clases nuevas, y los 36 filtros del contrato de PB-T3c, **todos con `ExpectRed` false** (también `PushBackViewEraseGuardTests`, que era true en PB-T3c) y su `MinSelected`, salvo **`PushBackDraftWriteGuardTests`, que baja de 3 a 2** por el `Skip` de CT-PBT3A-12 (V41-08), como bajaron los de F4 con los `Skip` de D-1:

{{RT_TABLE}}

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
