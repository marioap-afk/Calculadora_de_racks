## 4. Las guardas, por subtarea

### 4.1 PB-T4a (V41-06, V41-07, V41-16, V41-17)

**Una sola sustitución: CT-PBT3A-12** (`PushBackDraftWriteGuardTests.cs:60-95`). En el GREEN, RIA nombra `PushBackFullDraftReader`, uno de sus cinco nombres (`ConnectedNames`, :45-48), fuera de `Draft/`, PP y UR. La sustituye CT-PBT4A-06, el mismo censo con RIA admitida. En el esqueleto RIA no cambia, así que CT-PBT3A-12 pasaría; lleva el `Skip` desde el RED, como D-1.

**Lo demás, medido sobre el texto literal del GREEN** (`measure_pbt4_v41.py` sobre `7f0d9aa3`: WHC con la sentencia de D-PBT4-03 en los cinco cuerpos, NH con sus seis ediciones y RIA con las suyas; cada sustitución casa exactamente una vez):

{{GUARDS41}}

**Las guardas que buscan `ReadIntent` o `_editIntentReader` sin anclar** (la duda 5), una a una:
- **EIG:579** (`PassGate`), **:604-617** (el drenaje y el reposo), **:636** (la autoprueba del drenaje), **:925** (la vista previa, sobre los cuatro archivos de `Browser*`) y **:1124** (`UpdateNeverReads`, sobre `TryUpdatePilot`): leen cuerpos que el GREEN no toca. Verdes.
- **WLG:369** (`AdviseClose`), **:442** (`SessionDestroyed`), **:623** (el drenaje), **:634** (`MarkPilotDraftsPossiblyStale`) y **:687** (`DiscardPilotEdits`): lo mismo. Verdes.
- **WLG:653-656** (la recarga), **EIG:568-574** y **:761-769** (los cuerpos que leen): cuentan `\.ReadIntent\s*\(` y `_editIntentReader\s*[.!]`. `ReadIntentAtOrigin(` y `_editIntentReader is` no casan con esas expresiones, así que las cuentas siguen en 1. Verdes.
- **PEG:288 y :297** (`HostViolations`): solo miran las operaciones que no leen y las de «Actualizar». El GREEN solo cambia los cinco cuerpos que ya leían. Verdes.
- **EIG:620-623:** solo WHC llama a `.ReadIntent(`; NH declara `ReadIntent(` y `ReadIntentAtOrigin(`, sin punto delante. Verde.

**CA-T4a las vuelve a leer** sobre el texto del paquete custodiado.

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

Las otras pruebas de las mismas guardas que nombran lo retirado por su ayudante (las autopruebas de `HostViolations` que solo exigen `NotEmpty`, y G-4 :510-517) están en la lista de C-7.

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

