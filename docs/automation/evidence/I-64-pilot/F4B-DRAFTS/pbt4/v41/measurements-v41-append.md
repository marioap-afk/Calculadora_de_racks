
## Mediciones de la v4.1 (en `7f0d9aa3`)

Todo por objetos git (`git show`, `git ls-tree` y `git cat-file --batch`), con Python `-I`. El léxico nuevo (`lists_core.py`) vacía comentarios, cadenas (normales, verbatim, interpoladas y crudas) y caracteres antes de contar llaves, y un cuerpo de expresión acaba en el `;` de nivel cero. Las pruebas que ya llevan `Skip` no entran en ninguna lista.

## R-34. Las listas de lo elegido (I-22, I-23)

| Hecho | Fuente |
|---|---|
| PB-T4a: 1, `CT_PBT3A_12_NoSourceFileOutsideTheFolderOfTheModelNamesItsTypes` (`PushBackDraftWriteGuardTests.cs:60-95`); la rompe RIA, que nombra `PushBackFullDraftReader` | `pbt4-substitution-lists.md` |
| PB-T4b, C-0: 428 (las 9 clases: 69, 67, 62, 59, 54, 44, 35, 28 y 10) | ídem |
| PB-T4b, C-5 y C-6: 15 (CT-PBT4A-06, CT-PBT3B-13, G-4 ×2, EIG ×5, PEG ×5, RG ×1) | ídem |
| PB-T4b, C-3 de la UI: 27 (21 de `WorkspacePilotReconciliationTests` y 6 de `WorkspacePilotReconciliationViewTests`) | ídem |
| PB-T4b, C-7: 139 = 72 por texto (69 propias y E-3, E-5 y E-6) + 67 por compilación (R-36) | ídem |
| PB-T4b, total: 609; subtotal sin la retirada, 470 | ídem |
| PB-T4c: 2, PEG:155-163 y CT-PBT1B-15 (:306-403) | ídem |
| Con alguna firma de C-1 a C-6 o C-9 en las 9 clases: 237 (la v4 decía 242); sin ninguna, 191 (186) | `lists_pbt4-out.txt` |
| Comparable a las «99 propias» de Q1: 145 | `probe_overlap-out.txt` |

## R-35. El mapa de la copia de «Saltar y duplicar»

| Hecho | Fuente |
|---|---|
| Con M-01 a M-08, 375 de las 428 quedan sin nada que el mapa no cubra (copia mecánica) y 53 no: R-C4 29, R-C9 8, R-retirado 6, R-C3 5, R-C2 y R-C9 2, R-estado 1, R-retirado y R-estado 1, R-C2 1 | `copy_map_pbt4-out.txt`, `pbt4-copy-map.tsv` |
| Por clase, mecánicas / a mano: 69/0, 55/12, 57/5, 59/0, 54/0, 39/5, 12/23, 20/8 y 10/0 | ídem |
| Fuera de las pruebas, ningún resto que impida compilar las copias: solo `TryKeepMine` y `TryUpdatePilot` en dobles del puerto | ídem |
| Los tipos de primer nivel que se copian (M-00): `LifecycleScriptedAdvisor`; `PilotEditThrowingComparer`, `PilotEditThrowingProbe`, `PilotEditAuditedReader` y `PilotEditOutcomeProbe`; `CommandAdvisorPortDouble`; y las nueve clases. Los del kit, que no se copian: `EditIntentFakeReader`, `EditIntentKit` y `PrepareKit` | ídem |
| Los miembros del estado que usan las pruebas: `.Inputs.Annotations.AnnotationScale` (28), `.Inputs.PalletTolerance` (2), `.Inputs.Annotations` (1) y `state.Editor`/`state.Inputs` (3); en el estado completo, `Editable.AnnotationScale` y `Editable.PalletTolerance` (`PushBackEditableState.cs:64-66`) | `git show` |

## R-36. La retirada por compilación (Q1 «Retirarlo»)

| Hecho | Fuente |
|---|---|
| 179 pruebas sin `Skip` alcanzan lo retirado por referencia de código transitiva: 55 en las 9 clases, 57 ya en las listas y **67 nuevas** (`WorkspacePilotUpdateRunTests` 42, `WorkspaceDraftPilotOwnershipTests` 8, `WorkspaceDraftPilotStateTests` 7, `PushBackFullWritePrepareTests` 6, `PushBackDeclaredCorrectionPilotParityTests` 2, `PushBackFullWriteRunTests` 1 y `WorkspaceDraftPilotMaterializationTests` 1) | `retire_compile_pbt4-out.txt`, `pbt4-retire-compile.tsv` |
| 29 miembros de apoyo nombran lo retirado; entre ellos `PilotUpdateSetup.Run` y `ExpectedPlan` (`WorkspacePilotUpdateScopeDouble.cs:266-277`), `PilotoEscribe` y `Paridad` (`PushBackDeclaredCorrectionSupport.cs:128-149`), los campos `Comparer` (`WorkspaceDraftPilotMaterializationTests.cs:20`, `WorkspaceDraftPilotStateTests.cs:661`), `OwnershipKits.Pilot` (`WorkspaceDraftPilotOwnershipTests.cs:58-68`) y `Key = PushBackPilotFieldAdapter.AnnotationScaleKey` (`WorkspacePilotEditHostTests.cs:65`) | ídem |
| Las 55 de las 9 clases: `WorkspaceEditIntentHostTests` 2, `WorkspacePilotEditHostTests` 16, `WorkspacePilotPrepareTests` 30 y `WorkspacePilotReconciliationHostTests` 7 | ídem |

## R-37. Las guardas de PB-T4a sobre el texto literal del GREEN (dudas 4, 5, 7 y 8)

| Hecho | Fuente |
|---|---|
| EIG:139-141 fija la firma entera de `RackPilotIntentAdapter.Read`, hasta `RackCatalog catalog\s*\)`; un parámetro nuevo, obligatorio o con valor por defecto, la rompe (contrafactual, 2 de 2) | `measure_pbt4_v41-out.txt`; EIG:132-147 |
| Ninguna guarda prohíbe parámetros opcionales en el Plugin | `git grep` |
| NH no tiene `#nullable`; RIA sí (`RackPilotIntentAdapter.cs:1`) | `git show` |
| Con el texto literal del GREEN (WHC, NH y RIA), 32 de 32 aserciones medidas en verde: EIG, WLG, PEG, RG, CAG, G-9, la reflexión sobre WHC y las cuentas de NH | `measure_pbt4_v41-out.txt` |
| CAG fija 5 motivos y 2 `ex.Message` en `Read` de RIA, y 3 motivos en `ReadOnActiveDocument` de NH; no exige nada fuera de esos cuerpos | CAG:406-464 |
| Los cinco cuerpos que releen tienen `var draft = _drafts.Find<PushBackPilotDraftState>(request.Session, request.RackId);` antes de la lectura | WHC:403, :498, :583, :667, :754 |
| `PushBackPilotIntentScan` pide `IReadOnlyDictionary<string, string?>` y expone `IReadOnlyDictionary<string, string>`: de ahí la copia de `AtKey`, como UR | `PushBackPilotIntentScan.cs:29-35, :92` |

## R-38. El contrato de PB-T3c, del que copia el §6

| Hecho | Fuente |
|---|---|
| 54 162 bytes; 12 invariantes; 36 `RequiredTests` (`PushBackDraftWriteGuardTests` con `MinSelected` 3; `PushBackViewEraseGuardTests` con `ExpectRed` true); 15 `ExpectedEvidence`; 33 `StopConditions`, 17 estándar | `pbt3c-contract-dump.txt` |
| La CI del GREEN de PB-T3c: `44757675821` sobre `938a35f0`, Core 19 129 = 19 123 + 0 + 6, UI 2 451 = 2 430 + 21 | `f4b-dispositions.md`, F4B-E48 |
| CT-PBT3A-12 es un `[Fact]` (:60); su `Skip` deja 2 ejecutables en `PushBackDraftWriteGuardTests` | `PushBackDraftWriteGuardTests.cs` |
