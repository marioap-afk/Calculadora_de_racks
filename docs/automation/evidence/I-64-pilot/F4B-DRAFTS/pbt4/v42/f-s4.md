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

{{GUARDS42}}

**Lo que el recorrido no puede ver** (D = 0 en el GREEN, §5.1): una prueba que lea el texto por un camino que no deja ninguna ruta, carpeta ni ayudante en su código, o que dependa de la reflexión sobre un tipo que no se miró. Es un riesgo declarado, con su parada: **PBT4A-C16**. CA-T4a vuelve a leer las 100 señaladas sobre el texto del paquete custodiado.

### 4.2 PB-T4b

Las roturas por letra (C-5), método a método (`pbt4-substitution-lists.md`, «C-5 y C-6»; {{N_C5}}):

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

**La UI de la elección (C-3; {{N_UI}}):** 21 de `WorkspacePilotReconciliationTests` (entre ellas :780-793, «Asking for an option calls no operation of the port and only reads its memory») y 6 de `WorkspacePilotReconciliationViewTests` (los clics reales en «Conservar mi borrador» y en «Sí», :175-183, :199-213 y :264-276, y los botones).

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

