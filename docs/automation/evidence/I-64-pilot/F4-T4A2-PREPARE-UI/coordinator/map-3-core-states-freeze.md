# Map 3 — Core states, host outcomes and Freeze rules for F4-T4A2 (read-only, HEAD `b9e1ed5a`)

All paths are repo-relative. Line numbers are at HEAD `b9e1ed5a`. Nothing was built, run or modified.

## 0. Vocabulary finding (before the tables)

- The five class names required by the Owner (`UNCHANGED`, `EXTERNAL_CHANGE`, `CONFLICT`, `NOT_FOUND` / `NO_LONGER_EDITABLE`, `READ_ERROR` / `UNKNOWN`) do **not** exist as identifiers in `src/`, nor as tokens in `docs/initiatives/I-64*`. The only architecture sources with similar wording are the Owner brief, `docs/initiatives/I-64-owner-brief.txt:321` («STALE / EXTERNAL CHANGE»), `:340-344` («MATCH / or / STALE / CONFLICT»), and the Freeze `docs/initiatives/I-64-proposal-v5.md:487` («Reconciliación explícita (brief, «STALE / EXTERNAL CHANGE»)»). The mapping below is therefore from the Owner classes to the existing values. It invents no new names.
- The only `Unchanged` identifier in the edit stack is the **preview witness** status: `EditWitnessStatus.Unchanged` (`src/RackCad.Application/Workspace/WorkspaceEditIntent.cs:69-75`) and `PushBackPilotPreviewWitnessStatus.Unchanged` (`src/RackCad.Application/Systems/PushBack/Pilot/PushBackPilotIntentResult.cs:12-17`). It is about the preview against the read, never about the base against the reread. `NotEvaluated` is documented as «never "unchanged"» (`WorkspaceEditIntent.cs:65-68`).

## A. Result classes at every layer

### A.1 Exact enums (members in declaration order)

| Enum | File:lines | Members, in order | Default |
|---|---|---|---|
| `DraftStatus` | `src/RackCad.Application/Workspace/Drafts/DraftSession.cs:58-66` | Clean, Dirty, Conflict, Applying, Unknown, Error | Clean |
| `DraftUnknownOrigin` | `DraftSession.cs:69-73` | CommitThrew, PostReread | — |
| `DraftBoundary` | `DraftSession.cs:76-85` | TabChange, RackChange, Update, DocumentChange, HidePanel, Dematerialize, DocumentClose | — |
| `DraftBoundaryDecision` | `DraftSession.cs:117-127` | Proceeded, Aborted, Kept | — |
| `DraftEditStatus` | `DraftSession.cs:150-156` | Updated, NotEditable | — |
| `DraftReloadStatus` | `DraftSession.cs:161-173` | Reloaded, NotAdmitted, NotConfirmed, NotAllowed | — |
| `DraftCheckStatus` | `DraftSession.cs:190-200` | NotApplicable, Recovered, Conflict | — |
| `DraftConfirmStatus` | `DraftSession.cs:204-214` | NotApplicable, Confirmed, Conflict | — |
| `DraftMembership` | `src/RackCad.Application/Workspace/Drafts/DraftStaleBase.cs:13-19` | Mutable, ReadOnly, Blocking, NotMember | — |
| `DraftDifferenceKind` | `DraftStaleBase.cs:28-35` | Membership, View, Section, EnvelopeText, VariableRegistry | — |
| `EditIntentReadStatus` | `src/RackCad.Application/Workspace/WorkspaceEditIntent.cs:21-28` | Read, NoActiveDocument, Busy, Rejected, Failed | Read (!) |
| `EditWitnessStatus` | `WorkspaceEditIntent.cs:69-75` | NotEvaluated, NotSupplied, Unchanged, Changed | NotEvaluated |
| `EditIntentStatus` | `WorkspaceEditIntent.cs:77-133` | Unavailable, Waiting, Rejected, AlreadyOpen, NotAdmitted, Admitted, Failed, NotFound, NotNeeded, Reloaded, Kept (11) | Unavailable |
| `EditCheckStatus` | `WorkspaceEditIntent.cs:206-231` | Unavailable, Waiting, Rejected, NotFound, NotApplicable, Conflict, Recovered, Failed | Unavailable |
| `EditConfirmStatus` | `WorkspaceEditIntent.cs:282-307` | Unavailable, Waiting, Rejected, NotFound, NotApplicable, Confirmed, Conflict, Failed | Unavailable |
| `EditDiscardStatus` | `WorkspaceEditIntent.cs:364-377` | Rejected, Discarded, Failed, Busy | Rejected |
| `PilotFieldState` | `src/RackCad.Application/Workspace/IWorkspacePilotEditPort.cs:105-126` | NotOpen, Clean, Dirty, Conflict, Applying, Unknown, Error, Undecidable | NotOpen |
| `PilotFieldStatus` | `IWorkspacePilotEditPort.cs:129-144` | Rejected, Applied, NotEditable, Busy, Invalid | Rejected |
| `PilotBoundaryDecision` | `IWorkspacePilotEditPort.cs:147-164` | Proceeded, Aborted, Kept, Busy | Proceeded |
| `PilotBoundaryKind` | `IWorkspacePilotEditPort.cs:167-175` | TabChange, RackChange, DocumentChange, PanelHide, Dematerialize, DocumentClose | TabChange |
| `PilotUnknownOrigin` | `IWorkspacePilotEditPort.cs:181-186` | None, CommitThrew, PostReread | None |
| `PushBackPilotPreviewWitnessStatus` | `src/RackCad.Application/Systems/PushBack/Pilot/PushBackPilotIntentResult.cs:12-17` | NotSupplied, Unchanged, Changed | NotSupplied |

Name/order oracles: `tests/RackCad.Tests/Workspace/WorkspaceEditIntentHostTests.cs:1024` `TheOutcomesAreValuesAndTheStatesHaveTheExactNamesAndOrderOfThePackage`; `tests/RackCad.Tests/Workspace/WorkspacePilotPrepareTests.cs:1320` `TheNewOutcomesAreValuesWithTheStatesOfThePackageAndTheDefaultIsUnavailable`; `WorkspacePilotPrepareTests.cs:1302` `TheDefaultFieldViewHasTheThreeNewValuesInert`.

Outcome value shapes:
- `EditConfirmOutcome` (`WorkspaceEditIntent.cs:313-358`) carries `Status`, `Reason`, `Differences` ("key:kind" strings) and `AdmissionDiagnostic` (set only for a Confirmed whose read was not admitted). It carries **no** `DraftStatus`: the host drops the `DraftConfirmResult.DraftStatus` (`DraftSession.cs:217`). After a Confirmed, the UI can only learn that the draft stayed in Conflict through `GetField`.
- `EditIntentOutcome` (`WorkspaceEditIntent.cs:139-204`) carries `Status`, `Witness`, `Reason`, `Diagnostic`, `Detail` and `ClassicEditorHint`.

### A.2 The reader (one read of the edit intention)

Pure reader: `src/RackCad.Application/Systems/PushBack/Pilot/PushBackPilotIntentReader.cs`.
- `Read` **always** returns a `PushBackPilotIntentResult` with a `StaleBase`, admitted or not (`:75-109`; class doc `PushBackPilotIntentResult.cs:19-24`: «It ALWAYS carries the stale base (also when the read is not admitted, so "Check now" has something to compare)»).
- It never throws for a state of the drawing. An exception during evaluation becomes `MaterializeDiagnostic` (`:81-88`).
- Diagnostics (`:23-56`):
  - `NoIdentityDiagnostic` = «No editable aquí: el sobre no tiene identidad» (rules 1, 4);
  - `OriginMissingDiagnostic` = «No editable aquí: el origen ya no está en el dibujo» (rule 2);
  - `UnreadableDiagnostic` = «No editable aquí: ilegible o de versión futura» (rules 3, 7, 11, 13);
  - `OriginNotMemberDiagnostic` (rule 5), `OriginReadOnlyDiagnostic` (rule 6), `KindDivergentDiagnostic` (rule 8), `OutOfPilotDiagnostic` = «No editable aquí: fuera del piloto (F5)» (rules 9, 12), `DivergentDiagnostic` (rule 10), `MaterializeDiagnostic` (rule 14 and exceptions);
  - `ClassicEditorHint` = «Usa RACKEDITAR para editarlo con el editor clásico.».
- Base construction (`:113-153`):
  - Redraw/Erase → Mutable; ReadOnly → ReadOnly; `BlockingUnreadable` → Blocking with null text; NotMember is absent.
  - An uninterpretable envelope is `BlockingUnreadable` only when its probe Id is attributable to the rack; otherwise it is NotMember (`src/RackCad.Application/Views/Redraw/RackSiblingMembership.cs:132-137`).
  - The variable registry is not read (`PushBackPilotIntentReader.cs:59-63`).

Plugin adapter (the real `IWorkspaceEditIntentReader`): `src/RackCad.Plugin/Workspace/WorkspaceNavigationHost.cs:196-254` and `src/RackCad.Plugin/Views/RackPilotIntentAdapter.cs:94-142`.

| Situation in the drawing | What the reader returns | Evidence |
|---|---|---|
| Rack deleted (no member of the RackId left) | `Read`. Revalidation finds no target, so the origin is 0 and the origin key is "" (`WorkspaceEditIntent.cs:433-443`, `RackPilotIntentAdapter.cs:106`). The base is **empty** and the read is NotAdmitted(`OriginMissingDiagnostic`) by rule 2 (`PushBackPilotIntentReader.cs:162-166`) | code path; no test (see D-5) |
| Chosen reference vanished, rack still there | `Read`, origin 0, NotAdmitted(`OriginMissingDiagnostic`). The base holds the remaining members | `WorkspaceEditIntentHostTests.cs:1152`, `:1184` |
| No longer editable (A/B, xref origin, divergent, other kind, out of pilot) | `Read`, NotAdmitted with the diagnostic of the first failing rule, the base of the new facts and `ClassicEditorHint` | `PushBackPilotIntentReader.cs:157-233` |
| Unreadable or future envelope | `Read`, NotAdmitted(`UnreadableDiagnostic`). The member is Blocking (no text) if uninterpretable and attributable; otherwise Mutable with its new raw text when the scan says it is interpretable (rules 11/13) | `PushBackPilotIntentReader.cs:169`, `:192-194`, `:221-231` |
| No active document | `NoActiveDocument("No hay documento activo.")` | `WorkspaceNavigationHost.cs:223-224` |
| Command running / editor not quiescent | `Busy("Hay un comando en curso.")` | `:225-226` |
| Catalog fails | `Failed(catalog.FailureReason)`: «El catálogo no está disponible.» or «No se pudo cargar el catálogo: …» | `:228-230`, `RackPilotIntentAdapter.cs:74-87` |
| Session is not the one of the locked active document | `Rejected("La petición no es del documento activo.")` | `WorkspaceNavigationHost.cs:234-236` |
| Scan/read fails or throws | `Failed(reason)`: «El testigo de la vista previa no tiene clave.», «La exploración del dibujo no trajo las envolventes en bruto.», «El documento está bloqueado por otra operación.», «No se pudo leer el dibujo: …», «No se pudo leer la intención de edición: …» | `RackPilotIntentAdapter.cs:110-136` |
| Anything thrown by the adapter itself | `Failed(ex.Message)`, logged | `WorkspaceNavigationHost.cs:207-218` |

**Fact:** `default(EditIntentReadResult)` has `Status == Read` (the first enum value) and `PilotRead == null`. The host maps it to Failed with «La relectura no trajo resultado.» (confirm) or «La lectura para la recarga no trajo resultado.» (keep, discard).

### A.3 Core (`DraftSession<TState>`), quoted

`ConfirmBase` — `src/RackCad.Application/Workspace/Drafts/DraftSession.cs:641-659`:
```
641        public DraftConfirmResult ConfirmBase(DraftStaleBase reread)
642        {
643            if (reread == null) throw new ArgumentNullException(nameof(reread));
644            var none = Array.Empty<DraftDifference>();
645            var status = Status;
646            if (!_registered || (status != DraftStatus.Clean && status != DraftStatus.Dirty && status != DraftStatus.Conflict))
647                return new DraftConfirmResult(DraftConfirmStatus.NotApplicable, status, none);
648
649            var comparison = StaleBase.CompareWith(reread);
650            if (comparison.IsMatch)
651            {
652                if (_explicitStatus == null) IsPossiblyStale = false;
653                return new DraftConfirmResult(DraftConfirmStatus.Confirmed, Status, none);
654            }
655
656            _explicitStatus = DraftStatus.Conflict;
657            ConflictDifferences = comparison.Differences;
658            return new DraftConfirmResult(DraftConfirmStatus.Conflict, Status, comparison.Differences);
659        }
```

`DiscardAndReload` — `DraftSession.cs:422-437`. It replaces the base, the initial and the current state from one read and empties the pending text:
```
425            if (!_registered || _explicitStatus == DraftStatus.Applying)
426                return new DraftReloadResult(DraftReloadStatus.NotAllowed, read.Admission);
427            if (!read.IsAdmitted)
428                return new DraftReloadResult(DraftReloadStatus.NotAdmitted, read.Admission);
431            var initial = _copy(read.InitialState!);
432            var current = _copy(read.InitialState!);
433            Rebase(read, initial);
434            _current = current;
435            _pending.Clear();
```

Atomic keep — `DraftSession.cs:467-484`. The pending text **stays**:
```
469            if (keepOn == null) throw new ArgumentNullException(nameof(keepOn));
470            if (read == null) throw new ArgumentNullException(nameof(read));
471            var status = Status;
472            if (!_registered || (status != DraftStatus.Dirty && status != DraftStatus.Conflict))
473                return new DraftReloadResult(DraftReloadStatus.NotAllowed, read.Admission);
474            if (!confirmed)
475                return new DraftReloadResult(DraftReloadStatus.NotConfirmed, read.Admission);
476            if (!read.IsAdmitted)
477                return new DraftReloadResult(DraftReloadStatus.NotAdmitted, read.Admission);
479            var initial = _copy(read.InitialState!);
480            var current = _copy(keepOn(_copy(read.InitialState!), _current));
481            Rebase(read, initial);
482            _current = current;
```

`Rebase` (`:487-498`) sets base and initial together. It clears `IsPossiblyStale`, the explicit status, `UnknownOrigin`, `ConflictDifferences`, `LastApplyCause`, `Warnings` and `ErrorReason`.

`CheckNow` (`:612-630`) serves only Unknown with origin CommitThrew. `IsDirty` (`:298`) is `_pending.Count > 0 || !_comparer.Equals(_current, _initial)`. `Status` (`:300`) is `_explicitStatus ?? (IsDirty ? Dirty : Clean)`. `NeedsLazyReload` (`:306`) is registered, marked and Clean.

`KeepPilotEditsOn` — `src/RackCad.Application/Systems/PushBack/Pilot/PushBackPilotMaterializer.cs:242-251`:
```
247            var kept = PushBackPilotStateComparer.Copy(newInitial);
248            if (kept.Inputs.Annotations != null && current.Inputs.Annotations != null)
249                kept.Inputs.Annotations.AnnotationScale = current.Inputs.Annotations.AnnotationScale;
250            return kept;
```
**Fact:** it copies the current `AnnotationScale` unconditionally, whether or not the user edited it. Take a draft that is in Conflict and was Clean before, where the external change touched `AnnotationScale`. «Conservar» then puts the old scale on top of the new base, so the draft becomes Dirty with the old value. The code has no "user edited" tracking: dirty is state equality (`DraftSession.cs:298`).

`DraftStaleBase.CompareWith` (`DraftStaleBase.cs:141-169`) is exact and ordinal over the union of keys. An absent key equals `NoEntry` (NotMember with View, Section and Text all null, `:59`). So a deleted member yields Membership, plus View, Section and EnvelopeText for each of those that was non-null in the base (`:157-162`). Any change of an envelope's raw text, pilot-owned field or not, is the same `EnvelopeText` difference: the comparison cannot tell pilot-owned from non-pilot changes.

### A.4 Host operations, quoted

Constants (`src/RackCad.Application/Workspace/WorkspaceHostCore.cs:44-48`):
```
44        private const string PilotReadInProgressReason = "Hay una lectura del piloto en curso.";
45        private const string NotThisDocumentReason = "La petición no es del documento activo.";
46        private const string NoDraftReason = "No hay borrador para el rack.";
47        private const string NotCheckableReason = "El borrador no admite comprobación en su estado actual.";
48        private const string NotKeepableReason = "Solo se conserva un borrador en conflicto o con cambios marcados como posiblemente obsoletos.";
```

`TryConfirmPilotBase` — `WorkspaceHostCore.cs:528-592`:
```
528        public EditConfirmOutcome TryConfirmPilotBase(EditIntentRequest request)
529        {
530            try
531            {
532                if (request == null) throw new ArgumentNullException(nameof(request));
535                if (string.IsNullOrWhiteSpace(request.RackId))
536                    return EditConfirmOutcome.NotFound("Un rack sin identificador no tiene borrador.");
539                if (HasWitnessWithoutKey(request))
540                    return EditConfirmOutcome.Failed("El testigo de la vista previa no tiene clave.");
543                var gate = PassGate(request.Session, out var reason);
544                if (gate == EditGate.Unavailable) return EditConfirmOutcome.Unavailable(reason);
545                if (gate == EditGate.Waiting) return EditConfirmOutcome.Waiting(reason);
546                if (gate == EditGate.Rejected) return EditConfirmOutcome.Rejected(reason);
549                var draft = _drafts.Find<PushBackPilotDraftState>(request.Session, request.RackId);
550                if (draft == null) return EditConfirmOutcome.NotFound(NoDraftReason);
551                var before = draft.Status;
552                if (before != DraftStatus.Clean && before != DraftStatus.Dirty && before != DraftStatus.Conflict)
553                    return EditConfirmOutcome.NotApplicable(NotCheckableReason);
556                var read = _editIntentReader!.ReadIntent(request.Session, request.RackId, request.ChosenReference, request.Witness);
557                switch (read.Status)
558                {
559                    case EditIntentReadStatus.Read:
560                        break;
561                    case EditIntentReadStatus.NoActiveDocument:
562                    case EditIntentReadStatus.Busy:
563                        return EditConfirmOutcome.Waiting(read.Reason ?? "El dibujo no está listo para leerse.");
564                    case EditIntentReadStatus.Rejected:
565                        return EditConfirmOutcome.Rejected(read.Reason ?? NotThisDocumentReason);
566                    default:
567                        return EditConfirmOutcome.Failed(read.Reason ?? "Falló la relectura.");
568                }
570                var pilot = read.PilotRead;
571                if (pilot == null) return EditConfirmOutcome.Failed("La relectura no trajo resultado.");
574                var confirm = draft.ConfirmBase(pilot.StaleBase);
575                switch (confirm.Status)
576                {
577                    case DraftConfirmStatus.Confirmed:
579                        return EditConfirmOutcome.Confirmed(pilot.Read.IsAdmitted ? null : pilot.Read.Admission.Diagnostic);
580                    case DraftConfirmStatus.Conflict:
581                        _knownClean.Remove(request.Session);
582                        return EditConfirmOutcome.Conflict(DescribeDifferences(confirm.Differences));
583                    default:
584                        return EditConfirmOutcome.NotApplicable(NotCheckableReason);
585                }
586            }
587            catch (Exception ex)
588            {
589                Report("WorkspaceHostCore.TryConfirmPilotBase", ex);
590                return EditConfirmOutcome.Failed(ex.Message);
591            }
592        }
```
(Comment lines 533-534, 537-538, 541-542, 547-548, 554-555, 569, 572-573 and 578 omitted; 578 is «M-5: a confirmed check whose read was not admitted still carries the diagnostic of the admission.».) It compares `pilot.StaleBase`, which is present also when the read is not admitted.

`TryKeepPilotEdit` — `WorkspaceHostCore.cs:599-677`:
```
606                if (string.IsNullOrWhiteSpace(request.RackId))
607                {
608                    return EditIntentOutcome.NotAdmitted(
609                        PushBackPilotIntentReader.NoIdentityDiagnostic,
610                        null,
611                        PushBackPilotIntentReader.ClassicEditorHint,
612                        EditWitnessStatus.NotEvaluated);
613                }
616                if (HasWitnessWithoutKey(request))
617                    return EditIntentOutcome.Failed("El testigo de la vista previa no tiene clave.");
620                var gate = PassGate(request.Session, out var reason);
621                if (gate == EditGate.Unavailable) return EditIntentOutcome.Unavailable(reason);
622                if (gate == EditGate.Waiting) return EditIntentOutcome.Waiting(reason);
623                if (gate == EditGate.Rejected) return EditIntentOutcome.Rejected(reason);
627                var draft = _drafts.Find<PushBackPilotDraftState>(request.Session, request.RackId);
628                if (draft == null) return EditIntentOutcome.NotFound(NoDraftReason);
629                var before = draft.Status;
630                if (before == DraftStatus.Applying) return EditIntentOutcome.Waiting("El borrador se está aplicando o ya no está registrado.");
631                if (before != DraftStatus.Conflict && !(before == DraftStatus.Dirty && draft.IsPossiblyStale))
632                    return EditIntentOutcome.NotNeeded(NotKeepableReason);
635                var read = _editIntentReader!.ReadIntent(request.Session, request.RackId, request.ChosenReference, request.Witness);
636                switch (read.Status)
637                {
638                    case EditIntentReadStatus.Read:
639                        break;
640                    case EditIntentReadStatus.NoActiveDocument:
641                    case EditIntentReadStatus.Busy:
642                        return EditIntentOutcome.Waiting(read.Reason ?? "El dibujo no está listo para leerse.");
643                    case EditIntentReadStatus.Rejected:
644                        return EditIntentOutcome.Rejected(read.Reason ?? NotThisDocumentReason);
645                    default:
646                        return EditIntentOutcome.Failed(read.Reason ?? "Falló la lectura para la recarga.");
647                }
649                var pilot = read.PilotRead;
650                if (pilot == null) return EditIntentOutcome.Failed("La lectura para la recarga no trajo resultado.");
651                var witness = WorkspaceEditIntent.WitnessStatusOf(pilot.PreviewWitness);
654                var kept = draft.KeepMineOnCurrentVersion(pilot.Read, PushBackPilotMaterializer.KeepPilotEditsOn, confirmed: true);
655                switch (kept.Status)
656                {
657                    case DraftReloadStatus.Reloaded:
658                        _knownClean.Remove(request.Session);
659                        return EditIntentOutcome.Kept(witness);
660                    case DraftReloadStatus.NotAdmitted:
661                        return EditIntentOutcome.NotAdmitted(
662                            kept.Admission.Diagnostic ?? PushBackPilotIntentReader.MaterializeDiagnostic,
663                            pilot.Detail,
664                            pilot.ClassicEditorHint,
665                            witness);
666                    case DraftReloadStatus.NotAllowed:
667                        return EditIntentOutcome.Waiting("El borrador se está aplicando o ya no está registrado.");
668                    default:
669                        return EditIntentOutcome.Failed("El borrador no se recargó.");
670                }
672            catch (Exception ex)
673            {
674                Report("WorkspaceHostCore.TryKeepPilotEdit", ex);
675                return EditIntentOutcome.Failed(ex.Message);
676            }
```
`confirmed: true` is hard-coded (line 654): the host has **no cancel path**. The confirmation of R-A2-02 has to happen in the UI before the call.

`TryDiscardAndReloadPilotEdit` — `WorkspaceHostCore.cs:684-758`. Lines 691-708 are identical in shape to 606-623 (blank RackId gives NotAdmitted(NoIdentity); witness without key gives Failed; then the gate). Then:
```
711                var draft = _drafts.Find<PushBackPilotDraftState>(request.Session, request.RackId);
712                if (draft == null) return EditIntentOutcome.NotFound(NoDraftReason);
713                if (draft.Status == DraftStatus.Applying) return EditIntentOutcome.Waiting("El borrador se está aplicando o ya no está registrado.");
716                var read = _editIntentReader!.ReadIntent(request.Session, request.RackId, request.ChosenReference, request.Witness);
717                switch (read.Status)
718                {
719                    case EditIntentReadStatus.Read:
720                        break;
721                    case EditIntentReadStatus.NoActiveDocument:
722                    case EditIntentReadStatus.Busy:
723                        return EditIntentOutcome.Waiting(read.Reason ?? "El dibujo no está listo para leerse.");
724                    case EditIntentReadStatus.Rejected:
725                        return EditIntentOutcome.Rejected(read.Reason ?? NotThisDocumentReason);
726                    default:
727                        return EditIntentOutcome.Failed(read.Reason ?? "Falló la lectura para la recarga.");
728                }
730                var pilot = read.PilotRead;
731                if (pilot == null) return EditIntentOutcome.Failed("La lectura para la recarga no trajo resultado.");
732                var witness = WorkspaceEditIntent.WitnessStatusOf(pilot.PreviewWitness);
735                var reloaded = draft.DiscardAndReload(pilot.Read);
736                switch (reloaded.Status)
737                {
738                    case DraftReloadStatus.Reloaded:
739                        _knownClean.Remove(request.Session);
740                        return EditIntentOutcome.Reloaded(witness);
741                    case DraftReloadStatus.NotAdmitted:
742                        return EditIntentOutcome.NotAdmitted(
743                            reloaded.Admission.Diagnostic ?? PushBackPilotIntentReader.MaterializeDiagnostic,
744                            pilot.Detail,
745                            pilot.ClassicEditorHint,
746                            witness);
747                    case DraftReloadStatus.NotAllowed:
748                        return EditIntentOutcome.Waiting("El borrador se está aplicando o ya no está registrado.");
749                    default:
750                        return EditIntentOutcome.Failed("El borrador no se recargó.");
751                }
753            catch (Exception ex)
754            {
755                Report("WorkspaceHostCore.TryDiscardAndReloadPilotEdit", ex);
756                return EditIntentOutcome.Failed(ex.Message);
757            }
```
It has no confirmation parameter at all: the confirmation of D-14 / R-A2-02 is purely UI.

`PassGate` — `WorkspaceHostCore.cs:1328-1371`:
```
1337        private EditGate PassGate(SessionId session, out string reason)
1338        {
1339            reason = string.Empty;
1340            if (_editIntentReader == null)
1342                reason = "No hay un lector de la intención de edición asignado.";
1343                return EditGate.Unavailable;
1346            if (!_probe.ActiveDocumentNativeId.HasValue)
1348                reason = "No hay documento activo.";
1349                return EditGate.Waiting;
1352            if (_modalRackCadActive)
1354                reason = "Hay una ventana de RackCad abierta.";
1355                return EditGate.Waiting;
1358            if (!_probe.IsActiveDocumentQuiescent)
1360                reason = "Hay un comando en curso.";
1361                return EditGate.Waiting;
1364            if (Authorize(new WorkspaceRequest(session)) != RequestDecision.Accepted)
1366                reason = "La petición no es del documento activo.";
1367                return EditGate.Rejected;
1370            return EditGate.Open;
1371        }
```
`EditGate` = {Open, Unavailable, Waiting, Rejected} (`:1328-1334`).

Port wrappers (read-in-progress mark, `try/finally`) are at `WorkspaceHostCore.cs:843-883`; each starts with `if (_pilotReadInProgress) return …Waiting(PilotReadInProgressReason);`. The notice gate in `src/RackCad.Application/Workspace/WorkspacePilotSupport.cs:176-183` returns Waiting(`InsideNoticeReason` = «Se está entregando un aviso del panel.», `:156`) without delegating.

Exception containment: every body catches `Exception`, calls `Report(context, ex)` (`WorkspaceHostCore.cs:1377-1387`; it only logs and never degrades the session) and returns `Failed(ex.Message)`. Exceptions thrown inside `Status`, `IsDirty`, `ConfirmBase`, the copier or `keepOn` reach that same catch.

### A.5 Exact mapping table: reader status → outcome → reason

| Step / reader answer | TryConfirmPilotBase | TryKeepPilotEdit | TryDiscardAndReloadPilotEdit |
|---|---|---|---|
| null request | Failed(ex.Message) | Failed(ex.Message) | Failed(ex.Message) |
| blank RackId | NotFound «Un rack sin identificador no tiene borrador.» | NotAdmitted(`NoIdentityDiagnostic`, null, `ClassicEditorHint`, NotEvaluated) | same as Keep |
| witness present without key | Failed «El testigo de la vista previa no tiene clave.» | same | same |
| no reader | Unavailable «No hay un lector de la intención de edición asignado.» | same | same |
| no active document / RackCad modal / command | Waiting «No hay documento activo.» / «Hay una ventana de RackCad abierta.» / «Hay un comando en curso.» | same | same |
| session ≠ active document (INV-22) | Rejected «La petición no es del documento activo.» | same | same |
| read in progress (port) | Waiting «Hay una lectura del piloto en curso.» | same | same |
| inside a panel notice (gate) | Waiting «Se está entregando un aviso del panel.» | same | same |
| no draft | NotFound «No hay borrador para el rack.» | NotFound (same text) | NotFound (same text) |
| applicability | not Clean/Dirty/Conflict → NotApplicable «El borrador no admite comprobación en su estado actual.» | Applying → Waiting «El borrador se está aplicando o ya no está registrado.»; not (Conflict or Dirty+stale) → NotNeeded «Solo se conserva un borrador en conflicto o con cambios marcados como posiblemente obsoletos.» | Applying → Waiting (same text) |
| `NoActiveDocument` / `Busy` | Waiting(reason ?? «El dibujo no está listo para leerse.») | same | same |
| `Rejected` | Rejected(reason ?? «La petición no es del documento activo.») | same | same |
| `Failed` / any other | Failed(reason ?? «Falló la relectura.») | Failed(reason ?? «Falló la lectura para la recarga.») | same as Keep |
| `Read` with null PilotRead | Failed «La relectura no trajo resultado.» | Failed «La lectura para la recarga no trajo resultado.» | same as Keep |
| `Read`, base MATCH | Confirmed (AdmissionDiagnostic = null if admitted, else the admission diagnostic); draft keeps its status (Conflict stays Conflict); clears `IsPossiblyStale` only without explicit status | (no comparison) admitted → Kept(witness); not admitted → NotAdmitted(diag, Detail, Hint, witness) | (no comparison) admitted → Reloaded(witness); not admitted → NotAdmitted(...) |
| `Read`, base DIFFERS | Conflict(Differences "key:Kind"), AdmissionDiagnostic **null** even if not admitted; draft → Conflict | as above (keep does not compare) | as above |
| core NotAllowed after the read | — | Waiting «El borrador se está aplicando o ya no está registrado.» | same |
| core other | NotApplicable «El borrador no admite comprobación en su estado actual.» | Failed «El borrador no se recargó.» | same |
| exception anywhere | Failed(ex.Message) + Report("WorkspaceHostCore.TryConfirmPilotBase") | Failed + Report(".TryKeepPilotEdit") | Failed + Report(".TryDiscardAndReloadPilotEdit") |

`DescribeDifferences` (`WorkspaceHostCore.cs:1373-1374`) gives `(d.Key ?? "(variable registry)") + ":" + d.Kind`. `_knownClean.Remove` runs only on Conflict (confirm), Kept and Reloaded.

### A.6 Owner class → existing values (facts, no new names)

| Owner class | Reader | Core | Host outcome | `PilotFieldView` after |
|---|---|---|---|---|
| **UNCHANGED** | `Read`, base equal | `DraftConfirmStatus.Confirmed` | `EditConfirmStatus.Confirmed`, `AdmissionDiagnostic` null | State unchanged (Clean/Dirty), `IsPossiblyStale` false; **Conflict stays Conflict** (`DraftSession.cs:650-653`) |
| UNCHANGED base but read not admitted | `Read`, base equal, NotAdmitted | Confirmed | `Confirmed` **with** `AdmissionDiagnostic` (e.g. `OriginMissingDiagnostic`) | unchanged |
| **EXTERNAL_CHANGE** (Clean draft) | `Read`, base differs | `DraftConfirmStatus.Conflict` | `EditConfirmStatus.Conflict` + Differences | `State=Conflict`, `ConflictText`, `IsProtected` = `draft.IsDirty` = false |
| **CONFLICT** (own edits) | same | same | **same value as EXTERNAL_CHANGE** | `State=Conflict`, `IsProtected` = true; `PendingText` if any |
| **NOT_FOUND** (no draft) | not called | — | `NotFound` «No hay borrador para el rack.» (also blank RackId for confirm) | `Exists=false`, `NotOpen` |
| **NOT_FOUND** (rack deleted in drawing) | `Read`, empty base, NotAdmitted(`OriginMissingDiagnostic`) | ConfirmBase → Conflict; Keep/Discard → `DraftReloadStatus.NotAdmitted` | Confirm: **Conflict** with "key:Membership" etc.; Keep/Discard: `NotAdmitted(OriginMissingDiagnostic)`, draft unchanged | Conflict (after confirm) |
| **NO_LONGER_EDITABLE** | `Read`, NotAdmitted(diag) | as above | Confirm: Confirmed+diag (base equal) or Conflict without diag (base differs); Keep/Discard: `NotAdmitted(diag, Detail, Hint)` | unchanged by keep/discard |
| **READ_ERROR** | `Failed` (or adapter catch) | not reached | `Failed(reason)`; reader exception → `Failed(ex.Message)` + log | unchanged |
| unreadable/future envelope | `Read`, NotAdmitted(`UnreadableDiagnostic`) | Confirm/Keep/Discard as NO_LONGER_EDITABLE | Confirm usually Conflict (Blocking membership or text) | — |
| **UNKNOWN** (nothing was read) | not called | — | `Unavailable`, `Waiting`, `Rejected` (never Confirmed; default value is Unavailable) | unchanged |
| UNKNOWN (draft state) | — | `DraftStatus.Unknown` | Confirm `NotApplicable`; CheckNow (CommitThrew) / Discard (PostReread) | `State=Unknown`, `UnknownOrigin` |
| undecidable | — | `Status` threw | — | `PilotFieldState.Undecidable`, `IsProtected=true`, `CanEdit=false` |

**Clean-Conflict vs Dirty-Conflict in `PilotFieldView`.** There is no `IsDirty` property: the constructor is at `IWorkspacePilotEditPort.cs:197-225`. The equivalent is `IsProtected`, set in `ViewOfPilotDraft` (`WorkspaceHostCore.cs:1163-1178`):
```
1164            if (state == PilotFieldState.Undecidable)
1166                isProtected = true;
1170                try
1172                    isProtected = draft.IsDirty;
1174                catch (Exception)
1176                    isProtected = true;
```
- In Conflict, `Status` does not evaluate `IsDirty` (explicit status), so the state is never Undecidable. `IsProtected` is `draft.IsDirty` (own state edits or pending text), **or** true if `IsDirty` threw.
- So the two can be told apart by `IsProtected` (and `PendingText`), but `IsProtected=true` is ambiguous between "dirty" and "comparer threw" in Conflict.
- No test asserts `IsProtected` in Conflict: `tests/RackCad.Tests/Workspace/WorkspacePilotEditHostTests.cs:585` checks only `(State, CanEdit)` for Conflict at `:611`.
- `CanEdit` is true in Conflict (`WorkspaceHostCore.cs:1180-1181`). `ConflictText` is filled only in Conflict (`:1210`), and so is `UnknownOrigin` only in Unknown (`:1203-1208`). `LastCause` is always `draft.LastApplyCause` (`:1213`).

Keep applicability and Clean conflicts: TryKeepPilotEdit serves **any** Conflict, including a former-Clean one (`:631`). With `KeepPilotEditsOn` (A.3) that draft gets the old `AnnotationScale` on the new base.

## B. Freeze rules (exact quotes)

`docs/initiatives/I-64-proposal-v5.md`:

**D-10** (`:408-441`), the relevant lines:
- `:420` «| **Estado** | Limpio, Dirty, Posiblemente obsoleto, Conflicto, Aplicando, Desconocido, Error | UI y políticas |»
- `:422-423` «**Dirty** = CurrentDraftState ≠ InitialDraftState, **o** hay texto pendiente. No se calcula comparando JSON crudo: un rack guardado por otro build o con otro formato abre Limpio (INV-26). La mecánica (igualdad de estado o seguimiento de ediciones) no se congela.»
- `:432` «toda recarga sustituye StaleBase e InitialDraftState **a la vez**, desde una sola lectura.»
- `:434-435` «**bloqueables**, iniciadas por el usuario dentro del panel (cambiar de pestaña o de rack, Actualizar): rige D6 tal cual; si un campo es inválido, la acción se aborta y el texto permanece en su caja;»
- `:436-437` «**no bloqueables** (cambio de documento, ocultar el panel, desmaterializar por K, cierre de documento): el texto pendiente se conserva tal cual en el borrador y vuelve a su caja al rematerializar; **no** se compromete ni se auto-repara (INV-18);»
- `:439` «**Limpio con pista** (O-01): un borrador Limpio marcado «posiblemente obsoleto» se recarga de forma perezosa al activarse.»

**D-11 / INV-31 link** (`:457-458`): «`Divergent`, `Unreadable`, `BlockingUnreadable` o un sobre sin Id: **No editable aquí**, con el diagnóstico visible, la entrada rechazada y el editor clásico disponible sin cambios (INV-31).»

**D-12** (`:463-493`), the relevant lines:
- `:479` «MATCH solo si todo es idéntico. Cualquier diferencia: **Conflicto y cero escrituras authored** (INV-06).»
- `:480-481` «**Comprobación previa** (O-04): PREPARE repite la comparación en solo lectura antes de importar nada, para no dejar residuo de biblioteca ante un Conflicto evidente. No sustituye a la relectura dentro de MUTATE.»
- `:482` «**Sin normalización:** los riesgos de Discovery §9.14 solo pueden dar conflictos falsos, nunca aceptaciones falsas.»
- `:486` «**Comprobar ahora:** relectura de solo lectura que deja el borrador en Conflicto o confirma su estado.»
- `:487` «**Reconciliación explícita** (brief, «STALE / EXTERNAL CHANGE»):»
- `:488` «1. **Descartar y recargar:** StaleBase, InitialDraftState y CurrentDraftState se sustituyen desde una sola lectura nueva.»
- `:489-491` «2. **Conservar mi borrador sobre la versión actual:** tras una confirmación explícita, StaleBase e InitialDraftState se sustituyen desde una sola lectura nueva y el CurrentDraftState se conserva; el siguiente Actualizar compara contra esa base. En Selectivo, la reconciliación de vínculos usa el authored de la base nueva (Freeze I-48).»
- `:493` «Sin fusión automática.»

**D-13 boundary table** (`:521-532`):
```
521	- **Clasificación por frontera, congelada** (PV2-03):
523	  | Frontera | Resultado | Escrituras authored | Borrador | Lo que ve el usuario |
524	  |---|---|---|---|---|
525	  | Fallo **antes de llamar a `Commit()`** | No aplicado — Conflicto, Admisión, Preflight o Fallo | ninguna; residuo de biblioteca posible | se conserva (Conflicto si aplica) | la causa y, en Conflicto, qué cambió |
526	  | **`Commit()` lanzó** | **Desconocido**, con relectura obligatoria | desconocidas hasta releer | estado Desconocido | «estado desconocido; comprueba» |
527	  | **`Commit()` retornó** | Aplicado | todas | rebasado y Limpio | las vistas redibujadas |
528	  | `Commit()` retornó y POST falló o hubo faltantes | **Aplicado con avisos** | todas | rebasado | avisos visibles en el panel, no solo en el registro |
529	  | `Commit()` retornó y la relectura de POST falló | Aplicado; borrador Desconocido | todas | Desconocido | «aplicado; recarga para seguir editando» |
531	  Desde Desconocido, «Comprobar ahora» relee: si coincide con la StaleBase anterior, el commit no ocurrió y el borrador **recupera el estado
532	  que tenía antes del intento** (Limpio o Dirty; O3-06); si no, pasa a Conflicto. Nunca se supone aplicado.
```

**D-14** (`:547-553`):
```
549	- Cambiar de pestaña, de rack o de documento, ocultar el panel o cerrar una pestaña limpia **nunca** escribe el DWG (INV-05).
550	- Los borradores se conservan entre pestañas y entre sesiones de documento.
551	- Una pestaña con dirty o con texto pendiente nunca es preview ni se reemplaza (D-08), y cerrarla pregunta.
552	- Un Actualizar del rack A no deja en Conflicto ni marca el borrador independiente de B (INV-28).
553	- El cierre de un documento sigue D-17.
```
D-14 itself does not contain the phrase "discard loses edits". That wording is in the T4 package R-A2-02 (`docs/initiatives/I-64-f4-t4-gate-contract-package.md:520`), which cites D-14.

**Invariants:**
- `:744` INV-06 «Un borrador obsoleto no sobrescribe | Core con lector y escritor simulados: diferencia en atribución, en un sobre o en el registro → No aplicado — Conflicto, cero escrituras».
- `:756` INV-18 «Fronteras bloqueables según ADR-0032 D6; las no bloqueables conservan el texto sin comprometer ni auto-reparar».
- `:764` INV-26 «Dirty no depende de JSON crudo (PV2-04) | Core/UI: fixture con el JSON de otro formato o build → abre Limpio».
- `:769` INV-31 «Resultados visibles: Aplicado con avisos, No editable aquí y Desconocido se muestran en la UI (PV2-22) | UI: el view-model muestra avisos, el diagnóstico de admisión con el editor clásico ofrecido y el estado Desconocido con «Comprobar ahora» | avisos solo en el registro | F4».

**«posiblemente obsoleto» and «Comprobar ahora» elsewhere:**
- `:165-167`: user-initiated reads («Comprobar ahora» (D-12), la recarga de un borrador Limpio (O-01)) run in application context, with the document locked, at rest and without a RackCad modal.
- `:232` (drain op 4) «marcar borradores como «posiblemente obsoletos».».
- `:236` «**Pistas, no autoridad:** una pista solo invalida o marca. La obsolescencia la decide la relectura (D-12).».
- `:322` (SessionOverlay row).
- `:589` «un borrador del mismo rack pasa a «posiblemente obsoleto» y su Actualizar dará Conflicto.».
- `:890` S2-10; `:958` OV-26.

**Optionals:** `:1099` «O-01 | adoptada: recarga perezosa de borradores Limpios con pista (D-10)» and `:1102` «O-04 | adoptada: comprobación previa de solo lectura antes de importar (D-12, D-13)».

**IC-07** (`docs/initiatives/I-64-implementation-clarifications.md:113-129`), point 2 (`:123-127`):
```
123	**Punto 2, capa no editable (D-12):**
124	- Lo que el piloto edita sale del borrador.
125	- Todo lo authored que el piloto no edita sale de la base vigente (`InitialDraftState`). Esto incluye la autoridad de módulos y la metadata de origen del proyecto.
126	- Es análogo a la reconciliación de I-48 que cita D-12 («sobre la versión actual»).
127	- Después de «Conservar mi borrador», la capa no editable es la de la versión nueva. Un borrador Limpio compone exactamente la base y nunca escribe un cambio authored que la superficie no muestre.
```
Related, IC-08 (`:139-141`): «Es solo una pista: no crea Conflicto y no lee el dibujo.» / «La comprobación real la hacen «Comprobar ahora» y la recarga perezosa, siempre por acción del usuario.».

**T4 package rules for A2** (`docs/initiatives/I-64-f4-t4-gate-contract-package.md`):
- D-T4A1-01 behaviour table `:225-231`.
- Dispatch `:320-323`: «Unknown(CommitThrew): «Comprobar ahora», por `TryCheckNow`.» / «Unknown(PostReread): «Recargar», por `TryDiscardAndReload`.» / «Clean, Dirty y Conflict: «Comprobar ahora», por `TryConfirmBase`.».
- R-A2-01..06 `:517-524`, verbatim R-A2-02: «El Conflicto muestra la causa (`lastCause`) y qué cambió (`conflictText`). En Conflict con MATCH dice que la base vuelve a coincidir. Ofrece «Descartar y recargar» y «Conservar mi borrador», **los dos con confirmación explícita**, porque descartar un Dirty pierde ediciones (D-14)».
- R-A2-03: «Los avisos de INV-31 y de la tabla por frontera de D-13».
- **Fact:** R-A2-01 cites `WorkspacePilotCloseSyncGuardTests.cs:849`. That line is `IntentionReads = new(@"\b(?:TryBeginEdit|TryReload|TryCheckNow)\b")`: today it does **not** name `TryConfirmBase`, `TryKeepMine` or `TryDiscardAndReload`.

## C. Pending text and boundaries

- **Core:** `DraftBoundary` has 7 members; the blocking ones are TabChange, RackChange and Update (`DraftSession.cs:87-92`). `CrossBoundary` (`:377-404`) works like this:
  - non-blocking → Kept;
  - nothing pending → Proceeded;
  - `!CanEdit` → Kept;
  - any invalid field → Aborted, nothing changed;
  - otherwise Apply, then clear the pending text.

  `TryBeginApply` refuses with `PendingText` (`:514`).
- **Port:** `PilotBoundaryKind` has 6 members (no `Update` yet; T3 package `docs/initiatives/I-64-f4-t3-gate-contract-package.md:151` says «T4 puede añadir `Update` al final»). Host `CrossBoundary` (`WorkspaceHostCore.cs:961-1009`):
  - TabChange and RackChange are blocking: they need Authorize and no read in progress, and map to `DraftBoundary.TabChange`/`RackChange`;
  - DocumentChange, PanelHide, Dematerialize and DocumentClose return Kept with no authorization (`:971-977`);
  - an unknown kind → Aborted «La frontera no se conoce.».

  `CommitField` uses `DraftBoundary.TabChange` (`:940`).
- **UI callers today:**
  - `src/RackCad.UI/Workspace/WorkspaceBrowserViewModel.cs:834-858` (`AllowTabChange`) crosses **TabChange** only with pending text in the live tab and an existing other destination. Proceeded or Kept → allowed; otherwise `Field.ShowNotice("No se cambió de pestaña: " + …)`.
  - `:873-891` (`CrossObservedBoundaries`) crosses **DocumentChange** and **Dematerialize**.
  - No UI caller crosses RackChange, PanelHide or DocumentClose (grep over `src/`).
  - The field VM's `BeginEdit` (`src/RackCad.UI/Workspace/WorkspacePilotFieldViewModel.cs:191-239`) calls only `TryReload` (Clean and stale) or `TryBeginEdit`.
- **Reconciliation vs pending text (facts):**
  - None of TryConfirmPilotBase, TryKeepPilotEdit or TryDiscardAndReloadPilotEdit reads, validates or crosses the pending text.
  - ConfirmBase never touches it (`DraftSession.cs:641-659`).
  - KeepMine keeps it as typed (`:466`, `:479-483`).
  - DiscardAndReload clears it (`:435`).
  - CheckNow never touches it.
  - The Freeze lists no reconciliation action among the blocking boundaries (D-10 `:434-435`).
  - Existing host tests use an **invalid** pending key (`"field"`, which `PushBackPilotFieldAdapter.Validate` rejects with `UnknownFieldMessage`, `src/RackCad.Application/Systems/PushBack/Pilot/PushBackPilotFieldAdapter.cs:37-40`). They assert that keep proceeds and keeps it (`WorkspacePilotPrepareTests.cs:1015`, `:1038`) and that discard proceeds and clears it (`:1100`, `:1125`).
- **T3 package rules:**
  - transition table `docs/initiatives/I-64-f4-t3-gate-contract-package.md:73-83`;
  - binding rule `:85` «la sincronización automática por selección **nunca** cruza una frontera bloqueante y siempre conserva el pendiente. Ninguna transición que no inicie el usuario en el panel bloquea.»;
  - CrossBoundary contract `:151`;
  - live-text invariant D-T3-08 `:247-256`;
  - R-B3/R-B4/R-B5 `:410-412`.

## D. Coverage of the Owner's 16 scenarios (Core level)

T = `tests/RackCad.Tests/Workspace/`.

| # | Scenario | Existing tests (file:line, name) | Verdict |
|---|---|---|---|
| 1 | unchanged → unchanged | T`WorkspacePilotPrepareTests.cs:942` ConfirmWithAMatchAnswersConfirmedWithoutDiagnosticAndKeepsTheStatusAndTheEdits; `:187` AMatchInACleanOrDirtyDraftOnlyClearsThePossiblyStaleMarkAndNeverCleansDirty; `:705` ConfirmAppliesOnlyToCleanDirtyAndConflictAndTheOthersReadNothing; `:1000` ConfirmWithAMatchInConflictAnswersConfirmedAndTheDraftStaysInConflict; `:963` ConfirmWithAMatchThatIsNotAdmittedCarriesTheDiagnosticOfTheAdmissionAndOnlyThen | covered |
| 2 | external pilot-owned field changed | Confirm: only generic differences (`:212` core with `OtherBase`; `:978` host with `MemberBecameBlocking` = Membership). Keep: `:1015` (reread pb-02-modulos, whose `AnnotationScale` is 2 vs 1.5 in pb-01-base, with own edit 2.25) | **GAP** for Confirm with an EnvelopeText-only change of the pilot field; **GAP** for keep on a Clean-Conflict whose pilot field changed externally (see A.3 fact) |
| 3 | external non-pilot field changed | Keep: T`WorkspaceDraftPilotMaterializationTests.cs:872` CarryOver2_KeepAfterAnExternalModuleChange…, `:902` CarryOver2_KeepAfterAnExternalChangeOfContentThePilotDoesNotEdit…, `:568`, `:608`; comparison: T`WorkspaceDraftStaleBaseTests.cs:474` TheComparisonOfTheEnvelopeTextIsOrdinalAndExactWithNoNormalization | keep covered; **GAP** for host Confirm with a non-pilot-only text change |
| 4 | both changed | T`WorkspacePilotPrepareTests.cs:1015` KeepAfterAChangeOfTheDrawingStaysOnTheNewVersionWithTheScaleAndThePendingTextOfTheDraft (modules and scale both differ; own edit kept) | keep covered; **GAP** for Confirm |
| 5 | deleted / not-found rack | NotFound = no draft: `:648` AMissingDraftIsNotFoundWithoutReading; `:554` blank RackId. Partial deletion (B2 gone, origin ZZ): `:1058` KeepWithAReadThatIsNotAdmitted…, `:1100` DiscardAndReload…NotAdmittedKeepsIt | **GAP**: whole rack deleted (empty reread base) for Confirm, Keep and Discard |
| 6 | no-longer-editable | `:963` (Confirmed with `OriginMissingDiagnostic`); `:1058`, `:1100` (NotAdmitted with diagnostic and hint, draft kept); core `WorkspacePilotPrepareTests.cs:351` (NotAdmitted, keepOn not called) | partial; **GAP** for A/B, xref, divergent or out-of-pilot diagnostics through the three ops |
| 7 | unreadable / future envelope | `:978` ConfirmWithADifference… (`MemberBecameBlocking`: unreadable member gives Conflict); CheckNow only: T`WorkspaceDraftPilotIntentStaleBaseTests.cs:552`, `:571`, `:605` | partial; **GAP** for a future-version envelope and for Keep/Discard with `UnreadableDiagnostic` |
| 8 | read/comparison exception → unknown/error | `:833` AnExceptionOfTheReaderIsFailedWithTheMessageReportedAndTheMarkAndTheSessionAreNotTouched; `:885` EveryAnswerOfTheReaderThatIsNotAReadIsMappedWithItsReasonAndChangesNothing; `:916` AReadWithoutReasonOrWithoutResultUsesTheTextsOfTheOtherOperations (null reasons only) | reader side covered; **GAP**: `default(EditIntentReadResult)` (null PilotRead) for the three ops, and an exception inside `Status`/`ConfirmBase`/copier at host level |
| 9 | reload replaces base and initial atomically from the same read | core T`WorkspaceDraftSessionTests.cs:712` DiscardAndReloadReplacesTheBaseTheInitialAndTheCurrentStateFromOneReadAndEmptiesThePending; `:738`; host T`WorkspacePilotPrepareTests.cs:1100`, `:760` DiscardAndReloadAppliesToEveryStatusButApplying…; keep `:385` KeepReplacesTheBaseTheInitialAndTheCurrentStateInOneStep…, `:426` AKeepOnThatThrowsOrACopyThatThrows…; lazy T`WorkspaceLifecycleHostTests.cs:1125` | covered |
| 10 | keep preserves only pilot-owned user edits | T`WorkspaceDraftPilotMaterializationTests.cs:535` KeepPilotEditsOnIsPureAndGivesTheNewBaseWithTheScaleOfTheDraft (PalletTolerance from the new base, scale from the draft); `:872`, `:902`; host `WorkspacePilotPrepareTests.cs:1015` | covered for "edited scale kept"; **GAP**: unedited scale with an external scale change (code copies it anyway) |
| 11 | keep adopts current non-pilot data | `WorkspaceDraftPilotMaterializationTests.cs:872`, `:902`, `:568`, `:608` | covered |
| 12 | cancel leaves draft/base untouched | core T`WorkspacePilotPrepareTests.cs:351` (NotConfirmed), T`WorkspaceDraftSessionTests.cs:798` KeepMineWithoutConfirmationChangesNothing | core only; host has no cancel (`confirmed: true` at `WorkspaceHostCore.cs:654`; discard has no confirmation). **GAP** at UI |
| 13 | invalid pending text blocks blockable reconciliation | none. Existing tests assert the opposite: `WorkspacePilotPrepareTests.cs:1015` and `:1100` proceed with an invalid pending key | **GAP** and no Freeze rule makes reconciliation a blocking boundary |
| 14 | wrong SessionId → zero effect | `:605` TheGateAnswersWaitingOrRejectedForTheThreeOperationsAndReadsNothing (another document active → Rejected, 0 reads, status kept); `:885` (reader Rejected → Rejected, status and stale mark kept) | partial: asserts only `Status`/stale and reads; **GAP**: never-existing SessionId for the three ops (exists for intention T`WorkspaceEditIntentHostTests.cs:548` and CheckNow `:863`) |
| 15 | wrong active document → zero effect | same `:605` (no document → Waiting; other active → Rejected) and `:885` | partial (as 14) |
| 16 | no authored writes on every path | guard T`WorkspacePilotEditGuardTests.cs:736` TheOperationsOfThePortAndTheDecoratorNameNoApiOfWritingTheDrawing (`WriteViolations` `:693-733`, `WriteApis` `:72-74`, three reconciliation bodies `:686-691`); `:755` TheWriteGuardTripsOnAWriteInEachOperationOfThePort; `:769` TheWriteGuardTripsOnAWriteInEachPublicBodyOfTheExplicitReconciliation; T`WorkspacePilotPrepareGuardTests.cs:64`; runtime T`WorkspacePilotEditHostTests.cs:1326` (does not call the three new ops) | covered by guard |

Other relevant tests:
- `WorkspacePilotPrepareTests.cs:729` KeepApplies… (theory over 8 statuses);
- `:791` EachOperationReadsOnce… (read mark and nesting);
- `:1218` the notice gate;
- `:1247` TheFieldViewCarriesTheOriginOfUnknownTheTextOfTheConflictAndTheLastCause…;
- `:1167`/`:1180`/`:1203` the known-clean mark.
