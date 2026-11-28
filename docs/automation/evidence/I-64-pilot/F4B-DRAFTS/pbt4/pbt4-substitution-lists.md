> **Redacción para el repo público:** sin rutas locales.

# I-64 F4B — PB-T4: las listas de sustituciones (rev `7f0d9aa3`)

**Lo que eligió el Owner** (`owner-pbt4-decisions.md`): Q1 «Retirarlo», Q2 «Por grupos», Q3 «Saltar y duplicar» y Q4 «Sí, con condiciones». Estas son las listas de lo elegido. **Son la cota que vio el Owner:** la lista que se emite en cada subtarea es la suya o una parte de ella; una prueba de más vuelve al Owner.

**Cómo se generó:** `lists_pbt4.py` y `lists_core.py`, con `python -I`, solo por objetos git en `7f0d9aa3`. Una firma se mide en el texto de la prueba (sus atributos y su cuerpo) más el de los ayudantes de su mismo archivo que nombra, a un nivel. Las pruebas que ya llevan `Skip` en la revisión no entran. Las llaves se cuentan sin el texto de cadenas ni comentarios. La misma tabla, en `pbt4-substitution-lists.tsv`.

## Resumen

| Subtarea | Lista | Métodos |
|---|---|---|
| PB-T4a | C-5 | **1** |
| PB-T4b | C-0, las 9 clases enteras («Saltar y duplicar»): 375 con copia mecánica y 53 a mano | 428 |
| PB-T4b | C-5 y C-6, la letra de las guardas | 15 |
| PB-T4b | C-3, la UI de la elección | 27 |
| PB-T4b | **Subtotal sin «Retirarlo»** (la cifra «unas 465») | **470** |
| PB-T4b | C-7, lo que añade «Retirarlo» (la cifra «unas 100»), sin las ya contadas | **139** |
| PB-T4b | **Total** | **609** |
| PB-T4c | C-5 | **2** |

**Las 9 clases, por clase:** `WorkspaceLifecycleHostTests` 69; `WorkspacePilotPrepareTests` 67; `WorkspaceEditIntentHostTests` 62; `WorkspacePilotEditHostTests` 59; `WorkspaceCommandAdvisorTests` 54; `WorkspacePilotUpdateRequestTests` 44; `WorkspacePilotUpdateHostTests` 35; `WorkspacePilotReconciliationHostTests` 28; `WorkspaceAssociativeBookkeepingTests` 10.

**Las causas dentro de las 9 clases** (informativas: toda la clase se salta y se duplica): C-0: 191; C-0, C-1: 171; C-0, C-1, C-2, C-9: 2; C-0, C-1, C-3: 6; C-0, C-1, C-4: 26; C-0, C-1, C-6: 4; C-0, C-1, C-9: 20; C-0, C-2, C-9: 1; C-0, C-4: 3; C-0, C-6: 1; C-0, C-9: 3.

**Las que no admiten copia mecánica** (las sustituye un equivalente a mano con su fila de RR-3, D-PBT4-11): **53**, por lo que queda tras el mapa M: R-C4 29; R-C9 8; R-retirado 6; R-C3 5; R-C2,R-C9 2; R-estado 1; R-retirado,R-estado 1; R-C2 1. Los motivos están en `copy_map_defs.py`.

**«Retirarlo», por archivo:** `PushBackFullWritePrepareTests.cs` 9; `PushBackFullWriteRunTests.cs` 1; `PushBackPilotUpdateEquivalenceGuardTests.cs` 3; `WorkspaceDraftPilotIntentStaleBaseTests.cs` 4; `WorkspaceDraftPilotMaterializationTests.cs` 23; `WorkspaceDraftPilotOwnershipTests.cs` 9; `WorkspaceDraftPilotStateTests.cs` 22; `WorkspacePilotEditGuardTests.cs` 6; `WorkspacePilotPrepareGuardTests.cs` 2; `WorkspacePilotUpdateGuardTests.cs` 3; `WorkspacePilotUpdateIntegrationGuardTests.cs` 1; `WorkspacePilotUpdateRunTests.cs` 48; `PushBackDeclaredCorrectionPilotParityTests.cs` 2; `PushBackFieldValidationConcordanceTests.cs` 1; `WorkspaceDraftPilotCanonicalLineEndingTests.cs` 2; `WorkspaceDraftPilotValidationParityTests.cs` 2; `WorkspaceDraftPilotWindowLoadCharacterizationTests.cs` 1.

## PB-T4a (1)

| # | Archivo | Método | Líneas | Causa |
|---|---|---|---|---|
| 1 | `PushBackDraftWriteGuardTests.cs` | `CT_PBT3A_12_NoSourceFileOutsideTheFolderOfTheModelNamesItsTypes` | 60-95 | C-5: CT-PBT3A-12 (RIA nombra `PushBackFullDraftReader`, D-PBT4-02; la sustituye CT-PBT4A-06) |

## PB-T4b — C-0: las 9 clases, «Saltar y duplicar» (428)

| # | Archivo | Método | Líneas | Causa |
|---|---|---|---|---|
| 1 | `WorkspaceLifecycleHostTests.cs` | `TheAdvisorIsAssignedByAssignmentAndANullAdvisorIsRefused` | 132-142 | C-0; copia mecánica en `WorkspaceLifecycleHostFullTests` |
| 2 | `WorkspaceLifecycleHostTests.cs` | `WithoutAnAdvisorTheCloseIsAllowedAsBeforeAndTheDestructionNeedsNone` | 144-157 | C-0; copia mecánica en `WorkspaceLifecycleHostFullTests` |
| 3 | `WorkspaceLifecycleHostTests.cs` | `WithAnAdvisorTheDecisionOfTheCloseIsTheOneOfTheAdvisor` | 159-174 | C-0; copia mecánica en `WorkspaceLifecycleHostFullTests` |
| 4 | `WorkspaceLifecycleHostTests.cs` | `ASecondCallToSetLifecycleAdvisorReplacesTheAdvisor` | 176-194 | C-0; copia mecánica en `WorkspaceLifecycleHostFullTests` |
| 5 | `WorkspaceLifecycleHostTests.cs` | `AnAdvisorThatThrowsGivesVetoIsLoggedWithoutFaultAndDoesNotDegradeTheSession` | 196-221 | C-0; copia mecánica en `WorkspaceLifecycleHostFullTests` |
| 6 | `WorkspaceLifecycleHostTests.cs` | `TheCloseOfADegradedSessionStillGoesThroughTheAdvisorSoItSurvivesTheDegradation` | 223-237 | C-0; copia mecánica en `WorkspaceLifecycleHostFullTests` |
| 7 | `WorkspaceLifecycleHostTests.cs` | `TheDestructionTellsTheAdvisorSynchronouslyAfterTheSessionLeftTheRegistryAndTheActiveSlotForAnActiveAndAnInactiveSession` | 239-262 | C-0; copia mecánica en `WorkspaceLifecycleHostFullTests` |
| 8 | `WorkspaceLifecycleHostTests.cs` | `ADestructionWithAnUnknownNativeIdTellsNobody` | 264-278 | C-0; copia mecánica en `WorkspaceLifecycleHostFullTests` |
| 9 | `WorkspaceLifecycleHostTests.cs` | `AnAdvisorThatThrowsWhenTheSessionIsDestroyedStopsNothingOfTheDestructionAndTheNextIdIsANewSession` | 280-308 | C-0; copia mecánica en `WorkspaceLifecycleHostFullTests` |
| 10 | `WorkspaceLifecycleHostTests.cs` | `ARecycledNativeIdAfterAnAdvisorThatThrewHasANewSessionWithoutDraftsAndTheOldOneIsRetired` | 328-348 | C-0; copia mecánica en `WorkspaceLifecycleHostFullTests` |
| 11 | `WorkspaceLifecycleHostTests.cs` | `AnAllowedCloseFollowedDirectlyByTheDestructionLeavesTheSequenceIntactForAnActiveAndAnInactiveSession` | 364-375 | C-0; copia mecánica en `WorkspaceLifecycleHostFullTests` |
| 12 | `WorkspaceLifecycleHostTests.cs` | `AnyOtherInputThatChangesTheStateOfTheCoreBetweenTheAllowAndTheDestructionBreaksTheSequence` | 377-423 | C-0; copia mecánica en `WorkspaceLifecycleHostFullTests` |
| 13 | `WorkspaceLifecycleHostTests.cs` | `ADestructionWithoutItsOwnCloseNeverHasAnIntactSequence` | 425-434 | C-0; copia mecánica en `WorkspaceLifecycleHostFullTests` |
| 14 | `WorkspaceLifecycleHostTests.cs` | `AnAllowedCloseOfAnotherSessionDoesNotMakeTheSequenceOfThisOneIntact` | 436-445 | C-0; copia mecánica en `WorkspaceLifecycleHostFullTests` |
| 15 | `WorkspaceLifecycleHostTests.cs` | `AnAdvisorThatThrowsAtTheSecondCloseBreaksTheSequenceOfTheFirstAllow` | 447-458 | C-0; copia mecánica en `WorkspaceLifecycleHostFullTests` |
| 16 | `WorkspaceLifecycleHostTests.cs` | `ASessionDegradedBeforeTheAllowIsNeverIntactEvenWithNoInputBetweenTheAllowAndTheDestruction` | 460-471 | C-0; copia mecánica en `WorkspaceLifecycleHostFullTests` |
| 17 | `WorkspaceLifecycleHostTests.cs` | `TheQueriesAndTheAssignmentOfTheAdvisorDoNotEndTheSequence` | 473-493 | C-0; copia mecánica en `WorkspaceLifecycleHostFullTests` |
| 18 | `WorkspaceLifecycleHostTests.cs` | `ADraftThatIsDirtyForAKnownComparerVetoesTheClose` | 497-507 | C-0; copia mecánica en `WorkspaceLifecycleHostFullTests` |
| 19 | `WorkspaceLifecycleHostTests.cs` | `ADraftWithPendingTextVetoesTheCloseWithoutAskingTheComparer` | 509-522 | C-0; copia mecánica en `WorkspaceLifecycleHostFullTests` |
| 20 | `WorkspaceLifecycleHostTests.cs` | `ADraftThatMayBeDirtyBecauseTheComparerFailedVetoesTheCloseIsLoggedAndDoesNotDegradeTheSession` | 524-544 | C-0; copia mecánica en `WorkspaceLifecycleHostFullTests` |
| 21 | `WorkspaceLifecycleHostTests.cs` | `ADraftThatIsKnownCleanLetsTheNormalCloseProceedAndTheCloseRemovesNothing` | 546-559 | C-0, C-1; copia mecánica en `WorkspaceLifecycleHostFullTests` |
| 22 | `WorkspaceLifecycleHostTests.cs` | `ADirtyDraftOfThePilotVetoesTheCloseAndACleanOneAllowsIt` | 561-572 | C-0, C-1; copia mecánica en `WorkspaceLifecycleHostFullTests` |
| 23 | `WorkspaceLifecycleHostTests.cs` | `AnExplicitDiscardOfTheHostRemovesTheDraftsAndTheNextCloseProceedsEvenWithAComparerThatThrows` | 574-595 | C-0; copia mecánica en `WorkspaceLifecycleHostFullTests` |
| 24 | `WorkspaceLifecycleHostTests.cs` | `AnExplicitDiscardOfTheRegistryThatNeedsNoComparerAlsoLetsTheNextCloseProceed` | 597-612 | C-0; copia mecánica en `WorkspaceLifecycleHostFullTests` |
| 25 | `WorkspaceLifecycleHostTests.cs` | `EachSuccessiveAttemptToCloseIsVetoedAgainRepeatingIsNeverConsent` | 614-631 | C-0; copia mecánica en `WorkspaceLifecycleHostFullTests` |
| 26 | `WorkspaceLifecycleHostTests.cs` | `ASessionWithoutDraftsAllowsTheCloseAndTheCloseReadsAndWritesNothing` | 633-644 | C-0; copia mecánica en `WorkspaceLifecycleHostFullTests` |
| 27 | `WorkspaceLifecycleHostTests.cs` | `TheVetoIsPerSessionADirtyDraftOfAnotherDocumentDoesNotVetoThisOne` | 646-656 | C-0; copia mecánica en `WorkspaceLifecycleHostFullTests` |
| 28 | `WorkspaceLifecycleHostTests.cs` | `TheCloseOfADegradedSessionWithADirtyDraftIsVetoedAndTheNextOneProceedsAfterTheDiscard` | 658-673 | C-0; copia mecánica en `WorkspaceLifecycleHostFullTests` |
| 29 | `WorkspaceLifecycleHostTests.cs` | `AnAllowFollowedByTheDestructionWithAnIntactSequenceRetiresWithoutAskingTheComparer` | 700-704 | C-0; copia mecánica en `WorkspaceLifecycleHostFullTests` |
| 30 | `WorkspaceLifecycleHostTests.cs` | `ADestructionThatTheBridgeDidNotSeeAfterItsAllowRetiresConservativelyEvenWithTheMarkOfTheHost` | 706-710 | C-0; copia mecánica en `WorkspaceLifecycleHostFullTests` |
| 31 | `WorkspaceLifecycleHostTests.cs` | `ADestructionWithoutTheMarkOfTheHostRetiresConservativelyEvenWithAnIntactSequence` | 712-716 | C-0; copia mecánica en `WorkspaceLifecycleHostFullTests` |
| 32 | `WorkspaceLifecycleHostTests.cs` | `EachOperationThatCanChangeADraftEndsTheMarkSoTheSessionIsRetiredConservatively` | 718-770 | C-0, C-1; copia mecánica en `WorkspaceLifecycleHostFullTests` |
| 33 | `WorkspaceLifecycleHostTests.cs` | `ADiscardThatTheAuthorizationRefusesHasNoEffectSoItDoesNotEndTheMark` | 772-778 | C-0; copia mecánica en `WorkspaceLifecycleHostFullTests` |
| 34 | `WorkspaceLifecycleHostTests.cs` | `AnInPlaceEditAfterTheAllowThatIsNotNotedIsTheExactResidualOfTheDesignAndANoteMakesTheRetirementConservative` | 786-806 | C-0, C-1; copia mecánica en `WorkspaceLifecycleHostFullTests` |
| 35 | `WorkspaceLifecycleHostTests.cs` | `TheMarkIsPerSessionAnAllowOfOneSessionNeverMarksAnother` | 808-821 | C-0; copia mecánica en `WorkspaceLifecycleHostFullTests` |
| 36 | `WorkspaceLifecycleHostTests.cs` | `AVetoEndsTheMarkOfAnEarlierAllowAndTheAdvisorOfTheHostReadsAuthorizesDiscardsAndRetiresNothing` | 823-846 | C-0; copia mecánica en `WorkspaceLifecycleHostFullTests` |
| 37 | `WorkspaceLifecycleHostTests.cs` | `ADestroyedActiveSessionRetiresItsDraftsAndNoticesTheLossInTheQueueAndInTheLog` | 850-872 | C-0; copia mecánica en `WorkspaceLifecycleHostFullTests` |
| 38 | `WorkspaceLifecycleHostTests.cs` | `ADestroyedInactiveSessionIsRetiredAndNoticedWhileTheDraftsOfTheActiveOneAreUntouched` | 874-896 | C-0; copia mecánica en `WorkspaceLifecycleHostFullTests` |
| 39 | `WorkspaceLifecycleHostTests.cs` | `ASessionWithoutLossQueuesNothingAndLogsNothingSoTheQueueDoesNotGrowWithEveryClosedDocument` | 898-916 | C-0; copia mecánica en `WorkspaceLifecycleHostFullTests` |
| 40 | `WorkspaceLifecycleHostTests.cs` | `TheLogOfALossListsTheLostRacksAndApartTheUncertainOnesWithTheReasonOfTheComparer` | 918-944 | C-0; copia mecánica en `WorkspaceLifecycleHostFullTests` |
| 41 | `WorkspaceLifecycleHostTests.cs` | `ADestructionWithoutItsCloseWithADirtyDraftLosesItAndNoticesIt` | 946-960 | C-0, C-1; copia mecánica en `WorkspaceLifecycleHostFullTests` |
| 42 | `WorkspaceLifecycleHostTests.cs` | `AFailingLogDoesNotStopTheRetirementNorTheQueueNorTheDestruction` | 962-981 | C-0; copia mecánica en `WorkspaceLifecycleHostFullTests` |
| 43 | `WorkspaceLifecycleHostTests.cs` | `ADrainWithAChangeOfTheDrawingMarksEveryDraftOfThePilotOfTheSessionAsPossiblyStaleWithoutAReadOrAConflict` | 985-1003 | C-0, C-1; copia mecánica en `WorkspaceLifecycleHostFullTests` |
| 44 | `WorkspaceLifecycleHostTests.cs` | `TheEndOfACommandAlsoMarksTheDraftsBecauseItProducesTheSameHint` | 1005-1014 | C-0, C-1; copia mecánica en `WorkspaceLifecycleHostFullTests` |
| 45 | `WorkspaceLifecycleHostTests.cs` | `ADrainThatOnlyCarriesTheSelectionMarksNothing` | 1016-1028 | C-0, C-1; copia mecánica en `WorkspaceLifecycleHostFullTests` |
| 46 | `WorkspaceLifecycleHostTests.cs` | `TheMarkOfADrainTouchesTheDraftsOfThatSessionAndNoneOfAnother` | 1030-1045 | C-0, C-1; copia mecánica en `WorkspaceLifecycleHostFullTests` |
| 47 | `WorkspaceLifecycleHostTests.cs` | `TheMarkIsAppliedBeforeTheEarlyReturnsOfTheDrainSoADrainOfAnInactiveSessionOrOfAHiddenPanelMarksToo` | 1047-1066 | C-0, C-1; copia mecánica en `WorkspaceLifecycleHostFullTests` |
| 48 | `WorkspaceLifecycleHostTests.cs` | `AnEchoAndAChangeOfTheDrawingInTheSameDrainStillMarkTheDrafts` | 1068-1091 | C-0, C-1; copia mecánica en `WorkspaceLifecycleHostFullTests` |
| 49 | `WorkspaceLifecycleHostTests.cs` | `ADraftOfAnotherStateTypeIsNotMarkedAndNothingFailsBecauseOfIt` | 1093-1106 | C-0, C-1; copia mecánica en `WorkspaceLifecycleHostFullTests` |
| 50 | `WorkspaceLifecycleHostTests.cs` | `ADirtyDraftIsMarkedAsPossiblyStaleButStaysDirtyAndNeverReloadsLazily` | 1108-1120 | C-0, C-1; copia mecánica en `WorkspaceLifecycleHostFullTests` |
| 51 | `WorkspaceLifecycleHostTests.cs` | `TheReloadOfACleanDraftMarkedAsPossiblyStaleDoesOneReadAndReplacesTheDraftFromIt` | 1124-1143 | C-0, C-1; copia mecánica en `WorkspaceLifecycleHostFullTests` |
| 52 | `WorkspaceLifecycleHostTests.cs` | `TheReloadReadsOnlyWhenTheDraftNeedsIt` | 1145-1168 | C-0, C-1; copia mecánica en `WorkspaceLifecycleHostFullTests` |
| 53 | `WorkspaceLifecycleHostTests.cs` | `ADraftThatWasNeverMarkedIsNotReloadedAndNothingIsRead` | 1170-1183 | C-0, C-1; copia mecánica en `WorkspaceLifecycleHostFullTests` |
| 54 | `WorkspaceLifecycleHostTests.cs` | `TheReloadOfARackWithoutDraftIsNotFoundAndReadsNothing` | 1185-1195 | C-0; copia mecánica en `WorkspaceLifecycleHostFullTests` |
| 55 | `WorkspaceLifecycleHostTests.cs` | `AReloadThatIsNotAdmittedKeepsTheDraftMarkedAndCarriesTheDiagnosticTheDetailAndTheClassicEditorHint` | 1197-1214 | C-0; copia mecánica en `WorkspaceLifecycleHostFullTests` |
| 56 | `WorkspaceLifecycleHostTests.cs` | `ADraftThatLeavesTheRegistryWhileTheReadRunsMakesTheReloadWaitInsteadOfFailing` | 1216-1231 | C-0; copia mecánica en `WorkspaceLifecycleHostFullTests` |
| 57 | `WorkspaceLifecycleHostTests.cs` | `TheReloadGoesThroughTheSameGateAsTheIntention` | 1233-1254 | C-0; copia mecánica en `WorkspaceLifecycleHostFullTests` |
| 58 | `WorkspaceLifecycleHostTests.cs` | `TheReloadOfABlankRackAndANullRequestAreContainedWithoutReadingAndWithoutDegradingTheSession` | 1256-1275 | C-0; copia mecánica en `WorkspaceLifecycleHostFullTests` |
| 59 | `WorkspaceLifecycleHostTests.cs` | `AnExceptionOfTheReaderDuringTheReloadIsContainedAsFailedLoggedAndDoesNotDegradeTheSession` | 1277-1295 | C-0; copia mecánica en `WorkspaceLifecycleHostFullTests` |
| 60 | `WorkspaceLifecycleHostTests.cs` | `NoDrainEverReloadsADraftAndNoDrainReadsForTheLifecycle` | 1297-1315 | C-0; copia mecánica en `WorkspaceLifecycleHostFullTests` |
| 61 | `WorkspaceLifecycleHostTests.cs` | `DiscardPilotEditsRemovesEveryDraftOfTheSessionAndSaysWhichOnesHadPendingText` | 1319-1344 | C-0, C-1; copia mecánica en `WorkspaceLifecycleHostFullTests` |
| 62 | `WorkspaceLifecycleHostTests.cs` | `DiscardPilotEditsWorksWithTheProbeNotAtRestWithARackCadModalOpenAndWithoutAReader` | 1346-1360 | C-0; copia mecánica en `WorkspaceLifecycleHostFullTests` |
| 63 | `WorkspaceLifecycleHostTests.cs` | `DiscardPilotEditsWithoutDraftsIsDiscardedWithEmptyListsThatAreNeverNull` | 1362-1374 | C-0; copia mecánica en `WorkspaceLifecycleHostFullTests` |
| 64 | `WorkspaceLifecycleHostTests.cs` | `DiscardPilotEditsIsRejectedWithoutEffectsWhenAnotherSessionIsActiveWhenThereIsNoActiveDocumentOrTheSessionIsDestroyed` | 1376-1399 | C-0; copia mecánica en `WorkspaceLifecycleHostFullTests` |
| 65 | `WorkspaceLifecycleHostTests.cs` | `DiscardPilotEditsWithAComparerThatThrowsIsDiscardedAndTheNextCloseProceeds` | 1401-1416 | C-0; copia mecánica en `WorkspaceLifecycleHostFullTests` |
| 66 | `WorkspaceLifecycleHostTests.cs` | `AnExceptionOfTheDiscardIsContainedAsFailedAndDoesNotDegradeTheSession` | 1425-1449 | C-0; copia mecánica en `WorkspaceLifecycleHostFullTests` |
| 67 | `WorkspaceLifecycleHostTests.cs` | `TheDefaultValueOfTheDiscardOutcomeIsTheInertRejectedWithListsThatAreNeverNull` | 1451-1464 | C-0; copia mecánica en `WorkspaceLifecycleHostFullTests` |
| 68 | `WorkspaceLifecycleHostTests.cs` | `TheDiscardOutcomeNamesNeitherADraftNorATypeOfThePilot` | 1466-1482 | C-0; copia mecánica en `WorkspaceLifecycleHostFullTests` |
| 69 | `WorkspaceLifecycleHostTests.cs` | `TheExtendedStatesOfTheIntentionAreAnOrderedPrefixAndTheNewOutcomesCarryTheirWitness` | 1484-1497 | C-0; copia mecánica en `WorkspaceLifecycleHostFullTests` |
| 70 | `WorkspacePilotPrepareTests.cs` | `ConfirmBaseDoesNotApplyToADraftThatIsNotRegisteredApplyingUnknownOrInError` | 145-184 | C-0, C-1; copia mecánica en `WorkspacePilotPrepareFullTests` |
| 71 | `WorkspacePilotPrepareTests.cs` | `AMatchInACleanOrDirtyDraftOnlyClearsThePossiblyStaleMarkAndNeverCleansDirty` | 186-209 | C-0, C-1; copia mecánica en `WorkspacePilotPrepareFullTests` |
| 72 | `WorkspacePilotPrepareTests.cs` | `ADifferenceInACleanOrDirtyDraftPutsItInConflictWithTheOrderedDifferencesAndChangesNothingElse` | 211-237 | C-0, C-1; copia mecánica en `WorkspacePilotPrepareFullTests` |
| 73 | `WorkspacePilotPrepareTests.cs` | `AMatchInAConflictAnswersConfirmedAndTheDraftStaysInConflictWithAllItsData` | 239-257 | C-0, C-1; copia mecánica en `WorkspacePilotPrepareFullTests` |
| 74 | `WorkspacePilotPrepareTests.cs` | `ADifferenceInAConflictReplacesTheDifferencesAndTheDraftStaysInConflict` | 259-276 | C-0, C-1; copia mecánica en `WorkspacePilotPrepareFullTests` |
| 75 | `WorkspacePilotPrepareTests.cs` | `ConfirmBaseNeverTakesTheSlotOfTheUpdateAndNeverReadsAnything` | 278-294 | C-0; copia mecánica en `WorkspacePilotPrepareFullTests` |
| 76 | `WorkspacePilotPrepareTests.cs` | `ConfirmBaseWithoutAReReadThrowsAndChangesNothing` | 296-307 | C-0, C-1; copia mecánica en `WorkspacePilotPrepareFullTests` |
| 77 | `WorkspacePilotPrepareTests.cs` | `ConfirmBaseDoesNotChangeWhatCheckNowDoesForAnUnknownDraft` | 309-319 | C-0; copia mecánica en `WorkspacePilotPrepareFullTests` |
| 78 | `WorkspacePilotPrepareTests.cs` | `KeepWithANullKeepOnThrowsBeforeAnythingElseAndChangesNothing` | 335-348 | C-0, C-1; copia mecánica en `WorkspacePilotPrepareFullTests` |
| 79 | `WorkspacePilotPrepareTests.cs` | `KeepChecksNotAllowedThenNotConfirmedThenNotAdmittedInTheOrderOfTheOtherVersionAndNeverCallsKeepOnWithoutAdmission` | 350-382 | C-0, C-1; copia mecánica en `WorkspacePilotPrepareFullTests` |
| 80 | `WorkspacePilotPrepareTests.cs` | `KeepReplacesTheBaseTheInitialAndTheCurrentStateInOneStepAndKeepsThePendingTextAndTheEdits` | 384-423 | C-0; copia mecánica en `WorkspacePilotPrepareFullTests` |
| 81 | `WorkspacePilotPrepareTests.cs` | `AKeepOnThatThrowsOrACopyThatThrowsLeavesTheDraftExactlyAsItWas` | 425-468 | C-0, C-1; copia mecánica en `WorkspacePilotPrepareFullTests` |
| 82 | `WorkspacePilotPrepareTests.cs` | `TheVersionOfTwoParametersStillKeepsTheCurrentStateAsItWas` | 470-481 | C-0; copia mecánica en `WorkspacePilotPrepareFullTests` |
| 83 | `WorkspacePilotPrepareTests.cs` | `TheThreeOperationsAnswerUnavailableWithoutAReaderAndReadNothing` | 542-551 | C-0; copia mecánica en `WorkspacePilotPrepareFullTests` |
| 84 | `WorkspacePilotPrepareTests.cs` | `ABlankRackIdIsAnsweredBeforeAnythingElseWithTheTextsOfTheOtherOperationsAndReadsNothing` | 553-580 | C-0; copia mecánica en `WorkspacePilotPrepareFullTests` |
| 85 | `WorkspacePilotPrepareTests.cs` | `AWitnessWithoutAKeyIsAFailureOfTheRequestBeforeTheGateTheDraftAndTheReader` | 582-602 | C-0, C-1; copia mecánica en `WorkspacePilotPrepareFullTests` |
| 86 | `WorkspacePilotPrepareTests.cs` | `TheGateAnswersWaitingOrRejectedForTheThreeOperationsAndReadsNothing` | 604-645 | C-0, C-1; copia mecánica en `WorkspacePilotPrepareFullTests` |
| 87 | `WorkspacePilotPrepareTests.cs` | `AMissingDraftIsNotFoundWithoutReading` | 647-663 | C-0; copia mecánica en `WorkspacePilotPrepareFullTests` |
| 88 | `WorkspacePilotPrepareTests.cs` | `ConfirmAppliesOnlyToCleanDirtyAndConflictAndTheOthersReadNothing` | 703-725 | C-0, C-1; copia mecánica en `WorkspacePilotPrepareFullTests` |
| 89 | `WorkspacePilotPrepareTests.cs` | `KeepAppliesToConflictAndToDirtyMarkedStaleApplyingWaitsAndTheOthersAreNotNeededAllWithoutReading` | 727-756 | C-0, C-1; copia mecánica en `WorkspacePilotPrepareFullTests` |
| 90 | `WorkspacePilotPrepareTests.cs` | `DiscardAndReloadAppliesToEveryStatusButApplyingWhichWaitsAllWithTheOneRead` | 758-786 | C-0, C-1; copia mecánica en `WorkspacePilotPrepareFullTests` |
| 91 | `WorkspacePilotPrepareTests.cs` | `EachOperationReadsOnceAndSetsTheMarkOfReadInProgressAroundTheReadAndResetsItAfterwards` | 790-830 | C-0, C-1, C-3; a mano (R-C3) |
| 92 | `WorkspacePilotPrepareTests.cs` | `AnExceptionOfTheReaderIsFailedWithTheMessageReportedAndTheMarkAndTheSessionAreNotTouched` | 832-878 | C-0, C-1, C-3; a mano (R-C3) |
| 93 | `WorkspacePilotPrepareTests.cs` | `EveryAnswerOfTheReaderThatIsNotAReadIsMappedWithItsReasonAndChangesNothing` | 880-913 | C-0, C-1; copia mecánica en `WorkspacePilotPrepareFullTests` |
| 94 | `WorkspacePilotPrepareTests.cs` | `AReadWithoutReasonOrWithoutResultUsesTheTextsOfTheOtherOperations` | 915-937 | C-0, C-1; copia mecánica en `WorkspacePilotPrepareFullTests` |
| 95 | `WorkspacePilotPrepareTests.cs` | `ConfirmWithAMatchAnswersConfirmedWithoutDiagnosticAndKeepsTheStatusAndTheEdits` | 941-960 | C-0, C-1; copia mecánica en `WorkspacePilotPrepareFullTests` |
| 96 | `WorkspacePilotPrepareTests.cs` | `ConfirmWithAMatchThatIsNotAdmittedCarriesTheDiagnosticOfTheAdmissionAndOnlyThen` | 962-975 | C-0, C-1; copia mecánica en `WorkspacePilotPrepareFullTests` |
| 97 | `WorkspacePilotPrepareTests.cs` | `ConfirmWithADifferenceAnswersConflictWithTheDescribedDifferencesAndPutsTheDraftInConflict` | 977-997 | C-0, C-1; copia mecánica en `WorkspacePilotPrepareFullTests` |
| 98 | `WorkspacePilotPrepareTests.cs` | `ConfirmWithAMatchInConflictAnswersConfirmedAndTheDraftStaysInConflict` | 999-1012 | C-0, C-1; copia mecánica en `WorkspacePilotPrepareFullTests` |
| 99 | `WorkspacePilotPrepareTests.cs` | `KeepAfterAChangeOfTheDrawingStaysOnTheNewVersionWithTheScaleAndThePendingTextOfTheDraft` | 1014-1039 | C-0, C-1, C-3; a mano (R-retirado) |
| 100 | `WorkspacePilotPrepareTests.cs` | `KeepAConflictLeavesItAndTheDraftIsDirtyOnTheNewVersion` | 1041-1055 | C-0, C-1; copia mecánica en `WorkspacePilotPrepareFullTests` |
| 101 | `WorkspacePilotPrepareTests.cs` | `KeepWithAReadThatIsNotAdmittedKeepsTheDraftAsItWasAndCarriesTheDiagnostic` | 1057-1075 | C-0, C-1; copia mecánica en `WorkspacePilotPrepareFullTests` |
| 102 | `WorkspacePilotPrepareTests.cs` | `KeepThatFindsTheDraftBeingAppliedAfterTheReadAnswersWaitingAndChangesNothing` | 1077-1097 | C-0, C-1; copia mecánica en `WorkspacePilotPrepareFullTests` |
| 103 | `WorkspacePilotPrepareTests.cs` | `DiscardAndReloadReplacesTheDraftFromTheOneReadAndAReadThatIsNotAdmittedKeepsIt` | 1099-1126 | C-0, C-1; copia mecánica en `WorkspacePilotPrepareFullTests` |
| 104 | `WorkspacePilotPrepareTests.cs` | `DiscardAndReloadTakesAnUnknownAfterTheWriteOutOfItAndAnErrorToo` | 1128-1144 | C-0, C-1; copia mecánica en `WorkspacePilotPrepareFullTests` |
| 105 | `WorkspacePilotPrepareTests.cs` | `AConfirmedCheckKeepsTheMarkAndAConflictFoundByTheCheckEndsIt` | 1166-1177 | C-0, C-1; copia mecánica en `WorkspacePilotPrepareFullTests` |
| 106 | `WorkspacePilotPrepareTests.cs` | `AKeepThatIsKeptEndsTheMarkAndOneThatIsNotNeededOrNotAdmittedDoesNot` | 1179-1200 | C-0, C-1; copia mecánica en `WorkspacePilotPrepareFullTests` |
| 107 | `WorkspacePilotPrepareTests.cs` | `ADiscardAndReloadThatReloadsEndsTheMarkAndOneThatIsNotAdmittedDoesNot` | 1202-1213 | C-0, C-1; copia mecánica en `WorkspacePilotPrepareFullTests` |
| 108 | `WorkspacePilotPrepareTests.cs` | `InsideANoticeTheGateAnswersWaitingForTheThreeOperationsWithoutDelegatingAndOutsideItDelegates` | 1217-1244 | C-0, C-1, C-3; a mano (R-C3) |
| 109 | `WorkspacePilotPrepareTests.cs` | `TheFieldViewCarriesTheOriginOfUnknownTheTextOfTheConflictAndTheLastCauseAsTheDraftHoldsThem` | 1246-1299 | C-0, C-1; copia mecánica en `WorkspacePilotPrepareFullTests` |
| 110 | `WorkspacePilotPrepareTests.cs` | `TheDefaultFieldViewHasTheThreeNewValuesInert` | 1301-1317 | C-0; copia mecánica en `WorkspacePilotPrepareFullTests` |
| 111 | `WorkspacePilotPrepareTests.cs` | `TheNewOutcomesAreValuesWithTheStatesOfThePackageAndTheDefaultIsUnavailable` | 1319-1356 | C-0; copia mecánica en `WorkspacePilotPrepareFullTests` |
| 112 | `WorkspacePilotPrepareTests.cs` | `TheSnapshotIsBuiltOnlyByFromWithCopiesAndTheStatusBeforeAnAttemptWhileApplying` | 1360-1396 | C-0, C-1; copia mecánica en `WorkspacePilotPrepareFullTests` |
| 113 | `WorkspacePilotPrepareTests.cs` | `TheSevenOutcomesAreEvaluatedInTheDeclaredOrder` | 1398-1404 | C-0; copia mecánica en `WorkspacePilotPrepareFullTests` |
| 114 | `WorkspacePilotPrepareTests.cs` | `ADraftThatIsNotCleanOrDirtyIsNotApplicableWhateverTheReread` | 1406-1446 | C-0, C-1; copia mecánica en `WorkspacePilotPrepareFullTests` |
| 115 | `WorkspacePilotPrepareTests.cs` | `PendingTextComesBeforeEverythingButNotApplicableAndBeforeTheReread` | 1448-1463 | C-0, C-1; a mano (R-retirado) |
| 116 | `WorkspacePilotPrepareTests.cs` | `ACleanDraftHasNothingToApplyAndTheRereadIsNotLookedAt` | 1465-1484 | C-0, C-1, C-9; a mano (R-retirado) |
| 117 | `WorkspacePilotPrepareTests.cs` | `ADifferenceOfTheBaseIsAConflictBeforeTheAdmissionAndWithTheOrderedDifferences` | 1486-1512 | C-0, C-1; copia mecánica en `WorkspacePilotPrepareFullTests` |
| 118 | `WorkspacePilotPrepareTests.cs` | `ARereadThatMatchesButIsNotAdmittedIsNotAdmittedWithItsDiagnostic` | 1514-1528 | C-0, C-1, C-9; copia mecánica en `WorkspacePilotPrepareFullTests` |
| 119 | `WorkspacePilotPrepareTests.cs` | `InvalidA_ACurrentStateThatDoesNotComposeIsInvalidWithTheReasonOfTheCompositionAndOverTheCarryOversOfTheMaterializer` | 1532-1556 | C-0, C-1, C-4; a mano (R-C4) |
| 120 | `WorkspacePilotPrepareTests.cs` | `InvalidB_NoMutableMemberWithTheTextOfTheOriginOrSeveralOfThemIsInvalid` | 1558-1581 | C-0, C-1, C-9; copia mecánica en `WorkspacePilotPrepareFullTests` |
| 121 | `WorkspacePilotPrepareTests.cs` | `InvalidC_AnInnerDesignOfAnotherKindOrOfANewerMajorAbortsThePreflightAndTheUpdateIsInvalid` | 1583-1613 | C-0, C-1, C-9; copia mecánica en `WorkspacePilotPrepareFullTests` |
| 122 | `WorkspacePilotPrepareTests.cs` | `InvalidA_AnOriginThatNoLongerMaterializesIsInvalidWithTheReasonOfTheMaterializerBeforeAnythingElse` | 1615-1631 | C-0, C-1, C-9; a mano (R-estado) |
| 123 | `WorkspacePilotPrepareTests.cs` | `InvalidD_ABuilderThatCannotBuildTheOutgoingSystemIsInvalidWithTheTypedErrorAndNothingIsWritten` | 1633-1666 | C-0, C-1; a mano (R-retirado) |
| 124 | `WorkspacePilotPrepareTests.cs` | `InvalidE_AMutableMemberThatIsNotAvailableInTheOutgoingSystemOrWhoseEnvelopeDoesNotDecodeIsInvalidAndNeverErasesAView` | 1746-1780 | C-0, C-1, C-9; copia mecánica en `WorkspacePilotPrepareFullTests` |
| 125 | `WorkspacePilotPrepareTests.cs` | `InvalidE_AnEnvelopeThatDoesNotDecodeIsInvalidWithItsKey` | 1782-1795 | C-0, C-1, C-9; copia mecánica en `WorkspacePilotPrepareFullTests` |
| 126 | `WorkspacePilotPrepareTests.cs` | `PrepareIsTotalAMissingPieceIsInvalidAndNeverAnException` | 1797-1819 | C-0, C-1; a mano (R-retirado) |
| 127 | `WorkspacePilotPrepareTests.cs` | `AMissingCatalogIsAnsweredAfterTheConflictAndTheAdmissionAndNotBeforeThem` | 1823-1845 | C-0, C-1, C-9; a mano (R-retirado) |
| 128 | `WorkspacePilotPrepareTests.cs` | `TheReadyPlanHoldsTheRackTheOriginTheExpectedBaseTheScaleAndTheMembersByKeyInOrdinalOrder` | 1865-1881 | C-0; copia mecánica en `WorkspacePilotPrepareFullTests` |
| 129 | `WorkspacePilotPrepareTests.cs` | `TheOutgoingDesignAndEnvelopeOfEachViewComeFromTheRereadEnvelopeAndNeverFromTheDraft` | 1883-1928 | C-0, C-1, C-9; a mano (R-retirado,R-estado) |
| 130 | `WorkspacePilotPrepareTests.cs` | `ThePlanIsImmutableAndItsEqualityIsExplicitOrdinalElementByElementAndInOrder` | 1937-1982 | C-0; copia mecánica en `WorkspacePilotPrepareFullTests` |
| 131 | `WorkspacePilotPrepareTests.cs` | `ThePlanIsBuiltOnlyFromTheRereadSoAChangeOfTheCurrentScaleOnlyChangesTheScaleAndTheTextsThatCarryIt` | 1984-1994 | C-0; copia mecánica en `WorkspacePilotPrepareFullTests` |
| 132 | `WorkspacePilotPrepareTests.cs` | `ThePilotNeverErasesAViewAPhantomThatWasAlreadyInTheDrawingMakesTheUpdateInvalidAndIsNeverInThePlan` | 1996-2016 | C-0, C-1, C-9; copia mecánica en `WorkspacePilotPrepareFullTests` |
| 133 | `WorkspacePilotPrepareTests.cs` | `ReadOnlyAndNonMutableMembersOfTheRereadNeverEnterThePlan` | 2018-2038 | C-0, C-1, C-9; copia mecánica en `WorkspacePilotPrepareFullTests` |
| 134 | `WorkspacePilotPrepareTests.cs` | `PrepareNeverChangesTheDraftWhateverItsOutcome` | 2061-2119 | C-0, C-1, C-9; copia mecánica en `WorkspacePilotPrepareFullTests` |
| 135 | `WorkspacePilotPrepareTests.cs` | `PrepareDoesNotPutTheDraftInConflictNorCleanDirtyNorTakeTheSlotOfTheUpdate` | 2121-2138 | C-0, C-1; copia mecánica en `WorkspacePilotPrepareFullTests` |
| 136 | `WorkspacePilotPrepareTests.cs` | `ThePlanTypesLiveInThePilotNamespaceAndTheirSignaturesNameNoTypeOfTheHostOrOfTheDrawing` | 2142-2161 | C-0; copia mecánica en `WorkspacePilotPrepareFullTests` |
| 137 | `WorkspaceEditIntentHostTests.cs` | `AnIntentionIsOneReadOfTheReaderAndAnAdmittedReadCreatesTheDraftFromThatSameRead` | 116-139 | C-0, C-1; copia mecánica en `WorkspaceEditIntentHostFullTests` |
| 138 | `WorkspaceEditIntentHostTests.cs` | `TheDraftIsBornWithTheComparerAndTheCopierOfThePilot` | 141-159 | C-0, C-1; copia mecánica en `WorkspaceEditIntentHostFullTests` |
| 139 | `WorkspaceEditIntentHostTests.cs` | `TheRequestReachesTheReaderUnchangedWithTheChosenReferenceAndTheWitness` | 161-178 | C-0; copia mecánica en `WorkspaceEditIntentHostFullTests` |
| 140 | `WorkspaceEditIntentHostTests.cs` | `TheIntentionReadsNothingByItselfAndTheDrawingIsReadOnlyThroughTheReader` | 180-191 | C-0; copia mecánica en `WorkspaceEditIntentHostFullTests` |
| 141 | `WorkspaceEditIntentHostTests.cs` | `WithoutAReaderTheIntentionIsUnavailableAndReadsNothing` | 195-206 | C-0; copia mecánica en `WorkspaceEditIntentHostFullTests` |
| 142 | `WorkspaceEditIntentHostTests.cs` | `ABlankRackIdIsNotAdmittedWithNoIdentityBeforeAnythingElseAndTheRegistryIsNeverTouched` | 208-228 | C-0; copia mecánica en `WorkspaceEditIntentHostFullTests` |
| 143 | `WorkspaceEditIntentHostTests.cs` | `WithNoActiveDocumentTheIntentionWaitsAndReadsNothing` | 230-242 | C-0; copia mecánica en `WorkspaceEditIntentHostFullTests` |
| 144 | `WorkspaceEditIntentHostTests.cs` | `WhileARackCadModalIsOpenTheIntentionWaitsAndReadsNothing` | 244-258 | C-0; copia mecánica en `WorkspaceEditIntentHostFullTests` |
| 145 | `WorkspaceEditIntentHostTests.cs` | `WhileACommandRunsOrTheDocumentIsNotAtRestTheIntentionWaitsAndReadsNothing` | 260-274 | C-0; copia mecánica en `WorkspaceEditIntentHostFullTests` |
| 146 | `WorkspaceEditIntentHostTests.cs` | `AnOpenDraftIsAlreadyOpenAndIsNeverReadAgain` | 276-291 | C-0, C-1; copia mecánica en `WorkspaceEditIntentHostFullTests` |
| 147 | `WorkspaceEditIntentHostTests.cs` | `AReadThatIsNotAdmittedCreatesNoDraftAndCarriesTheDiagnosticTheDetailAndTheClassicEditorHint` | 295-312 | C-0, C-1; copia mecánica en `WorkspaceEditIntentHostFullTests` |
| 148 | `WorkspaceEditIntentHostTests.cs` | `TheTechnicalDetailOfTheDiagnosticTravelsApartFromItsMainText` | 314-328 | C-0, C-2, C-9; a mano (R-C2) |
| 149 | `WorkspaceEditIntentHostTests.cs` | `ARackThatWasNotAdmittedCanBeAdmittedLaterWhenTheDrawingAllowsIt` | 330-343 | C-0, C-1; copia mecánica en `WorkspaceEditIntentHostFullTests` |
| 150 | `WorkspaceEditIntentHostTests.cs` | `TheRegistryIsOneForEverySessionWithKeysOfSessionAndRack` | 345-363 | C-0, C-1; copia mecánica en `WorkspaceEditIntentHostFullTests` |
| 151 | `WorkspaceEditIntentHostTests.cs` | `ARetiredSessionNeverGetsADraftAndTheRequestIsRejected` | 365-376 | C-0, C-1; copia mecánica en `WorkspaceEditIntentHostFullTests` |
| 152 | `WorkspaceEditIntentHostTests.cs` | `TheHostOwnsTheClosePolicyOfItsOneRegistry` | 378-390 | C-0, C-1; copia mecánica en `WorkspaceEditIntentHostFullTests` |
| 153 | `WorkspaceEditIntentHostTests.cs` | `EveryAnswerOfTheReaderThatIsNotARead_IsMappedToItsStateAndCreatesNoDraft` | 394-418 | C-0; copia mecánica en `WorkspaceEditIntentHostFullTests` |
| 154 | `WorkspaceEditIntentHostTests.cs` | `AnEmptyAnswerOfTheReaderIsAFailureAndNeverAnAdmission` | 420-430 | C-0; copia mecánica en `WorkspaceEditIntentHostFullTests` |
| 155 | `WorkspaceEditIntentHostTests.cs` | `AnExceptionOfTheReaderIsContainedAsFailedLoggedAndDoesNotDegradeTheSession` | 434-453 | C-0; copia mecánica en `WorkspaceEditIntentHostFullTests` |
| 156 | `WorkspaceEditIntentHostTests.cs` | `ANullRequestIsContainedAsFailedAndNeverThrows` | 455-468 | C-0; copia mecánica en `WorkspaceEditIntentHostFullTests` |
| 157 | `WorkspaceEditIntentHostTests.cs` | `AFailingLogDoesNotBecomeASecondFailure` | 470-482 | C-0; copia mecánica en `WorkspaceEditIntentHostFullTests` |
| 158 | `WorkspaceEditIntentHostTests.cs` | `TheReaderCanBeAssignedAgainAndOnlyANullReaderIsRefused` | 484-496 | C-0; copia mecánica en `WorkspaceEditIntentHostFullTests` |
| 159 | `WorkspaceEditIntentHostTests.cs` | `ARequestOfTheSessionOfADocumentThatIsNotTheActiveOneIsRejectedWithZeroReadsAndNoDraft` | 500-523 | C-0; copia mecánica en `WorkspaceEditIntentHostFullTests` |
| 160 | `WorkspaceEditIntentHostTests.cs` | `ARecycledNativeIdentityNeverRevivesTheOldSessionForTheIntention` | 525-545 | C-0; copia mecánica en `WorkspaceEditIntentHostFullTests` |
| 161 | `WorkspaceEditIntentHostTests.cs` | `ASessionThatNeverExistedIsRejectedWithoutAReadOrADraft` | 547-560 | C-0; copia mecánica en `WorkspaceEditIntentHostFullTests` |
| 162 | `WorkspaceEditIntentHostTests.cs` | `WithADraftOfSessionAAndBActiveTheIntentionOfARejectsBeforeAnythingAboutTheDraftAndReadsNothing` | 562-585 | C-0, C-1; copia mecánica en `WorkspaceEditIntentHostFullTests` |
| 163 | `WorkspaceEditIntentHostTests.cs` | `APresentWitnessWithoutAKeyIsAFailureOfTheIntentionWithAReasonAndReadsNothingAndTouchesNoRegistry` | 597-616 | C-0, C-1; copia mecánica en `WorkspaceEditIntentHostFullTests` |
| 164 | `WorkspaceEditIntentHostTests.cs` | `APresentWitnessWithoutAKeyIsAFailureOfCheckNowWithAReasonAndReadsNothingAndLeavesTheUnknownDraftAsItWas` | 618-635 | C-0, C-1; copia mecánica en `WorkspaceEditIntentHostFullTests` |
| 165 | `WorkspaceEditIntentHostTests.cs` | `WithoutAWitnessTheStatusIsNotSuppliedAndNeverUnchanged` | 639-649 | C-0; copia mecánica en `WorkspaceEditIntentHostFullTests` |
| 166 | `WorkspaceEditIntentHostTests.cs` | `AWitnessThatMatchesTheReadIsUnchanged` | 651-662 | C-0, C-9; copia mecánica en `WorkspaceEditIntentHostFullTests` |
| 167 | `WorkspaceEditIntentHostTests.cs` | `AChangedWitnessIsReportedAsChangedAndNoKeystrokeIsAppliedToTheNewDraft` | 664-682 | C-0, C-1, C-9; copia mecánica en `WorkspaceEditIntentHostFullTests` |
| 168 | `WorkspaceEditIntentHostTests.cs` | `TheWitnessIsAlsoReportedWhenTheReadIsNotAdmitted` | 684-698 | C-0, C-9; copia mecánica en `WorkspaceEditIntentHostFullTests` |
| 169 | `WorkspaceEditIntentHostTests.cs` | `EveryOutcomeWithoutAReadCarriesNotEvaluatedForTheWitnessAndTheEnumHasTheFourValues` | 700-711 | C-0; copia mecánica en `WorkspaceEditIntentHostFullTests` |
| 170 | `WorkspaceEditIntentHostTests.cs` | `TheWitnessStatusOfThePilotIsTranslatedToTheOneOfTheHost` | 713-720 | C-0; copia mecánica en `WorkspaceEditIntentHostFullTests` |
| 171 | `WorkspaceEditIntentHostTests.cs` | `AnUnknownStatusOfThePilotIsNeverTranslatedToUnchanged` | 722-726 | C-0; copia mecánica en `WorkspaceEditIntentHostFullTests` |
| 172 | `WorkspaceEditIntentHostTests.cs` | `CheckNowRecoversAnUnknownDraftWhenTheRereadMatchesTheBaseFromBeforeTheAttempt` | 730-745 | C-0, C-1; copia mecánica en `WorkspaceEditIntentHostFullTests` |
| 173 | `WorkspaceEditIntentHostTests.cs` | `CheckNowTakesTheUnknownDraftToConflictWhenTheRereadDiffersAndNamesWhatDiffers` | 747-760 | C-0, C-1; copia mecánica en `WorkspaceEditIntentHostFullTests` |
| 174 | `WorkspaceEditIntentHostTests.cs` | `CheckNowUsesTheBaseOfARereadThatIsNotAdmittedToo` | 762-776 | C-0, C-1, C-9; a mano (R-C9) |
| 175 | `WorkspaceEditIntentHostTests.cs` | `WithoutAnUnknownDraftCheckNowIsNotApplicableAndReadsNothing` | 778-802 | C-0, C-1; copia mecánica en `WorkspaceEditIntentHostFullTests` |
| 176 | `WorkspaceEditIntentHostTests.cs` | `AnUnknownDraftWhoseOriginIsThePostRereadIsNotApplicableAndReadsNothing` | 804-821 | C-0, C-1; copia mecánica en `WorkspaceEditIntentHostFullTests` |
| 177 | `WorkspaceEditIntentHostTests.cs` | `CheckNowWithoutADraftIsNotFoundAndReadsNothing` | 823-834 | C-0; copia mecánica en `WorkspaceEditIntentHostFullTests` |
| 178 | `WorkspaceEditIntentHostTests.cs` | `CheckNowHasTheSameGateAsTheIntentionAndReadsNothingBeforeIt` | 836-860 | C-0, C-1; copia mecánica en `WorkspaceEditIntentHostFullTests` |
| 179 | `WorkspaceEditIntentHostTests.cs` | `CheckNowOfAnotherSessionIsRejectedWithZeroReadsAndTheDraftIsUntouched` | 862-878 | C-0, C-1; copia mecánica en `WorkspaceEditIntentHostFullTests` |
| 180 | `WorkspaceEditIntentHostTests.cs` | `EveryAnswerOfTheReaderThatIsNotAReadLeavesTheUnknownDraftAsItWasForCheckNow` | 880-902 | C-0, C-1; copia mecánica en `WorkspaceEditIntentHostFullTests` |
| 181 | `WorkspaceEditIntentHostTests.cs` | `AnExceptionOfTheReaderDuringCheckNowIsContainedAsFailedAndDoesNotDegradeTheSession` | 904-920 | C-0, C-1; copia mecánica en `WorkspaceEditIntentHostFullTests` |
| 182 | `WorkspaceEditIntentHostTests.cs` | `NoOperationIsCalledFromTheDrainAndTheDrainNeverCallsTheReaderOfTheIntention` | 924-945 | C-0, C-1; copia mecánica en `WorkspaceEditIntentHostFullTests` |
| 183 | `WorkspaceEditIntentHostTests.cs` | `ACarrierOfTheEditIntentionThatIsNotInTheCensusTripsTheGuardAndTheCensusHasThemAll` | 977-988 | C-0; copia mecánica en `WorkspaceEditIntentHostFullTests` |
| 184 | `WorkspaceEditIntentHostTests.cs` | `TheSignaturesOfTheTwoOperationsAndTheirValuesNameNoTypeOfADraftNorOfThePilot` | 990-1036 | C-0; copia mecánica en `WorkspaceEditIntentHostFullTests` |
| 185 | `WorkspaceEditIntentHostTests.cs` | `TheUpdateOfThePilotIsAPublicMethodOfTheCoreWhoseSecondParameterIsThePortOfTheUpdateAndItsOutcomeNamesNoTypeOfTheDraftsNorOfThePilot` | 1038-1057 | C-0, C-4; a mano (R-C4) |
| 186 | `WorkspaceEditIntentHostTests.cs` | `ThePortHasExactlyTheTwoMethodsOfTheRequestWithUpdateInTheirNameAndRejectsAThirdOne` | 1063-1072 | C-0, C-4; a mano (R-C4) |
| 187 | `WorkspaceEditIntentHostTests.cs` | `TheHostCoreHasExactlyOneFieldOfSystemsTheExecutorOfTheUpdateAndRejectsASecondOneOrAPort` | 1087-1099 | C-0; copia mecánica en `WorkspaceEditIntentHostFullTests` |
| 188 | `WorkspaceEditIntentHostTests.cs` | `NoFieldOfTheHostCoreIsOfATypeOfThePilotExceptTheExecutorOrOfTheResultOfTheReadAndTheFieldOfTheReaderIsNotCalledReader` | 1101-1123 | C-0; copia mecánica en `WorkspaceEditIntentHostFullTests` |
| 189 | `WorkspaceEditIntentHostTests.cs` | `TheOutcomesAreValuesAndTheStatesHaveTheExactNamesAndOrderOfThePackage` | 1125-1150 | C-0; copia mecánica en `WorkspaceEditIntentHostFullTests` |
| 190 | `WorkspaceEditIntentHostTests.cs` | `TheDifferencesOfACheckOutcomeAreNeverNullAndAreEmptyUnlessTheStatusIsConflictAlsoForTheDefaultValue` | 1152-1173 | C-0; copia mecánica en `WorkspaceEditIntentHostFullTests` |
| 191 | `WorkspaceEditIntentHostTests.cs` | `TheReadResultCarriesThePilotReadOnlyWhenItIsARead` | 1175-1194 | C-0, C-9; a mano (R-C9) |
| 192 | `WorkspaceEditIntentHostTests.cs` | `AChosenReferenceThatIsStillAmongTheTargetsGivesItsDefinition` | 1206-1215 | C-0; copia mecánica en `WorkspaceEditIntentHostFullTests` |
| 193 | `WorkspaceEditIntentHostTests.cs` | `AChosenReferenceThatIsNoLongerAmongTheTargetsGivesZeroEvenIfTheNumberIsADefinitionOfTheSet` | 1217-1227 | C-0; copia mecánica en `WorkspaceEditIntentHostFullTests` |
| 194 | `WorkspaceEditIntentHostTests.cs` | `WithoutAChosenReferenceTheOriginIsThePrimaryTargetInTheDeterministicOrderNotTheLowestHandle` | 1229-1236 | C-0; copia mecánica en `WorkspaceEditIntentHostFullTests` |
| 195 | `WorkspaceEditIntentHostTests.cs` | `WithoutTargetsTheOriginIsZeroWhetherOrNotAReferenceWasChosen` | 1238-1245 | C-0; copia mecánica en `WorkspaceEditIntentHostFullTests` |
| 196 | `WorkspaceEditIntentHostTests.cs` | `ANullSetOfTargetsIsAProgrammingErrorAndNotAnOrigin` | 1247-1251 | C-0; copia mecánica en `WorkspaceEditIntentHostFullTests` |
| 197 | `WorkspaceEditIntentHostTests.cs` | `AReferenceThatVanishedOrNowBelongsToAnotherRackGivesZeroAfterTheRevalidationOfTheSameOperation` | 1253-1283 | C-0; copia mecánica en `WorkspaceEditIntentHostFullTests` |
| 198 | `WorkspaceEditIntentHostTests.cs` | `TheDiagnosticOfAnOriginThatIsGoneSaysItIsNoLongerThereAndNeverThatItBelongsToAnotherRack` | 1285-1297 | C-0; copia mecánica en `WorkspaceEditIntentHostFullTests` |
| 199 | `WorkspacePilotEditHostTests.cs` | `AValidTextAndTheGestureOfTheFieldApplyItClearThePendingTextAndEndTheMark` | 150-169 | C-0, C-1; copia mecánica en `WorkspacePilotEditHostFullTests` |
| 200 | `WorkspacePilotEditHostTests.cs` | `TheGestureOfTheFieldEndsTheMarkOfKnownCleanWhenItAppliesTheText` | 171-180 | C-0, C-1; copia mecánica en `WorkspacePilotEditHostFullTests` |
| 201 | `WorkspacePilotEditHostTests.cs` | `AnEditOfTheUserEndsTheMarkOfKnownClean` | 182-186 | C-0; copia mecánica en `WorkspacePilotEditHostFullTests` |
| 202 | `WorkspacePilotEditHostTests.cs` | `AValidTextAtABlockingBoundaryProceedsAppliesItAndEndsTheMark` | 188-208 | C-0, C-1; copia mecánica en `WorkspacePilotEditHostFullTests` |
| 203 | `WorkspacePilotEditHostTests.cs` | `WithoutPendingTextTheGestureAndTheBlockingBoundariesChangeNothingAndWithoutADraftTheyProceed` | 210-227 | C-0, C-1; copia mecánica en `WorkspacePilotEditHostFullTests` |
| 204 | `WorkspacePilotEditHostTests.cs` | `AnInvalidTextAndTheGestureOfTheFieldAnswerInvalidKeepTheTextAsTypedAndLeaveTheValue` | 231-251 | C-0, C-1, C-6; copia mecánica en `WorkspacePilotEditHostFullTests` |
| 205 | `WorkspacePilotEditHostTests.cs` | `AnInvalidTextProtectsTheRackPutsItAtRiskAndVetoesTheCloseWithItsNotice` | 253-273 | C-0, C-1; copia mecánica en `WorkspacePilotEditHostFullTests` |
| 206 | `WorkspacePilotEditHostTests.cs` | `AnInvalidTextAbortsEachBlockingBoundaryWithTheFieldAndTheMessageAndNothingChanges` | 275-294 | C-0, C-1, C-6; copia mecánica en `WorkspacePilotEditHostFullTests` |
| 207 | `WorkspacePilotEditHostTests.cs` | `EachNonBlockingBoundaryKeepsAnInvalidTextCharacterByCharacterWithoutValidatingCommittingOrAuthorizing` | 298-322 | C-0, C-1; copia mecánica en `WorkspacePilotEditHostFullTests` |
| 208 | `WorkspacePilotEditHostTests.cs` | `ANonBlockingBoundaryKeepsTheTextAlsoWhenItIsValidAndDoesNotApplyIt` | 324-336 | C-0, C-1; copia mecánica en `WorkspacePilotEditHostFullTests` |
| 209 | `WorkspacePilotEditHostTests.cs` | `ThePendingTextOfADocumentStaysInItAfterTheDocumentChangeAndComesBackWhenTheDocumentIsActiveAgain` | 338-359 | C-0, C-1; copia mecánica en `WorkspacePilotEditHostFullTests` |
| 210 | `WorkspacePilotEditHostTests.cs` | `DiscardRackRemovesTheDraftAndItsPendingTextOfOneRackWithoutTouchingTheOthers` | 363-388 | C-0, C-1; copia mecánica en `WorkspacePilotEditHostFullTests` |
| 211 | `WorkspacePilotEditHostTests.cs` | `DiscardRackNeedsNoComparerAndRemovesAnUndecidableDraftToo` | 390-405 | C-0, C-1; copia mecánica en `WorkspacePilotEditHostFullTests` |
| 212 | `WorkspacePilotEditHostTests.cs` | `DiscardRackOfARackWithoutADraftIsDiscardedWithEmptyListsAndABlankRackToo` | 407-421 | C-0; copia mecánica en `WorkspacePilotEditHostFullTests` |
| 213 | `WorkspacePilotEditHostTests.cs` | `DiscardRackAndDiscardDocumentRemoveTheStateAndTheNextCloseIsAllowed` | 423-443 | C-0, C-1; copia mecánica en `WorkspacePilotEditHostFullTests` |
| 214 | `WorkspacePilotEditHostTests.cs` | `TheDiscardOfARackEndsTheMarkOfKnownClean` | 445-449 | C-0; copia mecánica en `WorkspacePilotEditHostFullTests` |
| 215 | `WorkspacePilotEditHostTests.cs` | `WithoutADiscardTheStateOfTheDraftPersistsThroughQueriesAndNonBlockingBoundaries` | 453-475 | C-0, C-1; copia mecánica en `WorkspacePilotEditHostFullTests` |
| 216 | `WorkspacePilotEditHostTests.cs` | `TheOperationsOfADocumentNeverTouchTheDraftsOfAnotherAndABlockingMutationOfAnotherDocumentIsRejected` | 479-514 | C-0, C-1; copia mecánica en `WorkspacePilotEditHostFullTests` |
| 217 | `WorkspacePilotEditHostTests.cs` | `TheModalScopeSwitchesOnBeforeItReturnsOffAtTheDisposeAlsoWithAnExceptionAndNestsAndDisposesTwiceHarmlessly` | 518-555 | C-0; copia mecánica en `WorkspacePilotEditHostFullTests` |
| 218 | `WorkspacePilotEditHostTests.cs` | `WithTheModalActiveTheIntentionWaitsAndCanDrainNowIsFalseUntilTheLastScopeEnds` | 557-580 | C-0; copia mecánica en `WorkspacePilotEditHostFullTests` |
| 219 | `WorkspacePilotEditHostTests.cs` | `GetFieldReportsEveryStateOfTheFieldAndWhatCanBeEdited` | 584-625 | C-0, C-1, C-6; copia mecánica en `WorkspacePilotEditHostFullTests` |
| 220 | `WorkspacePilotEditHostTests.cs` | `AComparerThatThrowsMakesTheFieldUndecidableProtectedAndNotEditableAndGetFieldNeverThrows` | 627-644 | C-0, C-1; copia mecánica en `WorkspacePilotEditHostFullTests` |
| 221 | `WorkspacePilotEditHostTests.cs` | `GetFieldAndGetRacksAtRiskNeedNoAuthorizationAndNeverThrowEvenForABlankRackOrADocumentThatIsNotActive` | 646-666 | C-0, C-1; copia mecánica en `WorkspacePilotEditHostFullTests` |
| 222 | `WorkspacePilotEditHostTests.cs` | `SetPendingTextIsRefusedWhenTheFieldCannotBeEditedWithoutAuthorizationAndWhenThereIsNoDraft` | 670-695 | C-0, C-1; copia mecánica en `WorkspacePilotEditHostFullTests` |
| 223 | `WorkspacePilotEditHostTests.cs` | `TheKeyOfThePendingTextIsAlwaysTheOneOfTheAdapterAndAnEmptyTextIsKeptToo` | 697-710 | C-0, C-1, C-6; copia mecánica en `WorkspacePilotEditHostFullTests` |
| 224 | `WorkspacePilotEditHostTests.cs` | `AfterAnySequenceOfAcceptedAndRejectedEditsTheFieldHoldsTheLastAcceptedText` | 712-739 | C-0, C-1; copia mecánica en `WorkspacePilotEditHostFullTests` |
| 225 | `WorkspacePilotEditHostTests.cs` | `WhileTheReloadReadsEveryOperationThatChangesSomethingAnswersBusyAndASecondReadWaitsAllWithoutEffect` | 749-796 | C-0, C-1; copia mecánica en `WorkspacePilotEditHostFullTests` |
| 226 | `WorkspacePilotEditHostTests.cs` | `TheMarkOfReadInProgressIsClearedWhenTheReadEndsAlsoWhenTheReaderThrows` | 798-815 | C-0, C-1; copia mecánica en `WorkspacePilotEditHostFullTests` |
| 227 | `WorkspacePilotEditHostTests.cs` | `ABeginInsideABeginAndACheckInsideACheckWaitWithoutReading` | 817-843 | C-0, C-1; copia mecánica en `WorkspacePilotEditHostFullTests` |
| 228 | `WorkspacePilotEditHostTests.cs` | `TheThreeReadsOfThePortAreTheOperationsOfTheHostAndNothingElseReadsTheDrawing` | 845-877 | C-0, C-1; copia mecánica en `WorkspacePilotEditHostFullTests` |
| 229 | `WorkspacePilotEditHostTests.cs` | `InsideANoticeOfThePanelTheThreeReadsWaitWithoutDelegatingAndWithoutReading` | 881-915 | C-0; copia mecánica en `WorkspacePilotEditHostFullTests` |
| 230 | `WorkspacePilotEditHostTests.cs` | `TheDepthOfNoticesNestsNeverGoesBelowZeroAndTheGateDelegatesEverythingElseAsItIs` | 917-946 | C-0, C-1; copia mecánica en `WorkspacePilotEditHostFullTests` |
| 231 | `WorkspacePilotEditHostTests.cs` | `TheShownTextOfAFiniteValueGreaterThanZeroIsReadBackAsTheSameDoubleAndPassesTheValidationInEveryCulture` | 988-1017 | C-0, C-6; copia mecánica en `WorkspacePilotEditHostFullTests` |
| 232 | `WorkspacePilotEditHostTests.cs` | `TheShownTextHasTheExpectedPlainDigitsAndTheDecimalSeparatorOfTheCurrentCulture` | 1019-1039 | C-0; copia mecánica en `WorkspacePilotEditHostFullTests` |
| 233 | `WorkspacePilotEditHostTests.cs` | `AValueOutsideTheDomainHasNoTextAndABlankTextIsNeverCreated` | 1041-1053 | C-0; copia mecánica en `WorkspacePilotEditHostFullTests` |
| 234 | `WorkspacePilotEditHostTests.cs` | `ShowingTheCommittedValueNeverCreatesPendingTextAndCommittingTheShownTextIsNotADirtyChange` | 1055-1081 | C-0, C-1; copia mecánica en `WorkspacePilotEditHostFullTests` |
| 235 | `WorkspacePilotEditHostTests.cs` | `TheLossNoticesAreTakenInOrderAsValuesAndLeaveTheQueueEmpty` | 1085-1110 | C-0, C-1; copia mecánica en `WorkspacePilotEditHostFullTests` |
| 236 | `WorkspacePilotEditHostTests.cs` | `TheCloseNoticesAreOnePerSessionTheLastOneAndAreTakenInOrderLeavingTheQueueEmpty` | 1112-1136 | C-0, C-1; copia mecánica en `WorkspacePilotEditHostFullTests` |
| 237 | `WorkspacePilotEditHostTests.cs` | `ATestOfTheCloseThatFailsLeavesAnUncertainNoticeWithoutAListOfRacks` | 1138-1153 | C-0, C-1; copia mecánica en `WorkspacePilotEditHostFullTests` |
| 238 | `WorkspacePilotEditHostTests.cs` | `TheNoticeOfAVetoOfADocumentThatIsNotActiveIsQueuedToo` | 1155-1169 | C-0, C-1; copia mecánica en `WorkspacePilotEditHostFullTests` |
| 239 | `WorkspacePilotEditHostTests.cs` | `TheNoticeOfAVetoExpiresWhenTheSessionIsDiscardedWhenTheNextAdviceAllowsAndWhenTheSessionIsRetired` | 1171-1217 | C-0, C-1; copia mecánica en `WorkspacePilotEditHostFullTests` |
| 240 | `WorkspacePilotEditHostTests.cs` | `AdviseCloseStillReadsNothingAuthorizesNothingAndDoesNotDiscardOrRetireAndTheDecisionIsTheSame` | 1219-1236 | C-0, C-1; copia mecánica en `WorkspacePilotEditHostFullTests` |
| 241 | `WorkspacePilotEditHostTests.cs` | `TheRacksAtRiskAreOrderedOrdinallyAndAComparerThatThrowsMakesThemUncertainApart` | 1240-1258 | C-0, C-1; copia mecánica en `WorkspacePilotEditHostFullTests` |
| 242 | `WorkspacePilotEditHostTests.cs` | `TheRacksAtRiskOfASessionThatIsNotTheActiveOneAreAnsweredAndADefaultValueNeverHasANullList` | 1260-1285 | C-0, C-1; copia mecánica en `WorkspacePilotEditHostFullTests` |
| 243 | `WorkspacePilotEditHostTests.cs` | `AnExceptionOfAnOperationIsContainedAsAValueWithAReasonAndDoesNotDegradeTheSession` | 1289-1321 | C-0; copia mecánica en `WorkspacePilotEditHostFullTests` |
| 244 | `WorkspacePilotEditHostTests.cs` | `NoOperationOfThePortWritesAuthoredDataOfTheDrawing` | 1325-1372 | C-0; copia mecánica en `WorkspacePilotEditHostFullTests` |
| 245 | `WorkspacePilotEditHostTests.cs` | `ABlockingBoundaryOnADraftThatCannotBeEditedKeepsItsPendingTextAndNeverValidatesOrApplies` | 1378-1420 | C-0, C-1; copia mecánica en `WorkspacePilotEditHostFullTests` |
| 246 | `WorkspacePilotEditHostTests.cs` | `TheOperationsThatChangeNothingKeepTheMarkOfKnownClean` | 1424-1439 | C-0; copia mecánica en `WorkspacePilotEditHostFullTests` |
| 247 | `WorkspacePilotEditHostTests.cs` | `ARejectedOrBusyTextKeepsTheMarkOfKnownClean` | 1441-1461 | C-0; copia mecánica en `WorkspacePilotEditHostFullTests` |
| 248 | `WorkspacePilotEditHostTests.cs` | `TheOuterCatchOfGetFieldAnswersTheViewInErrorWhenTheCopierOfTheStateAnswersNull` | 1465-1485 | C-0; copia mecánica en `WorkspacePilotEditHostFullTests` |
| 249 | `WorkspacePilotEditHostTests.cs` | `TheDefaultRiskViewIsTheUnknownAndEveryValueBuiltWithTheConstructorIsKnown` | 1490-1502 | C-0; copia mecánica en `WorkspacePilotEditHostFullTests` |
| 250 | `WorkspacePilotEditHostTests.cs` | `TheFallbackByDraftIsKnownAndKeepsTheUncertainOnesApart` | 1504-1518 | C-0, C-1; copia mecánica en `WorkspacePilotEditHostFullTests` |
| 251 | `WorkspacePilotEditHostTests.cs` | `ASessionWithoutDraftsAnswersKnownAndEmptyAndNeverTheUnknown` | 1520-1536 | C-0; copia mecánica en `WorkspacePilotEditHostFullTests` |
| 252 | `WorkspacePilotEditHostTests.cs` | `AKnownRiskWithRacksInRiskStaysKnown` | 1538-1550 | C-0, C-1; copia mecánica en `WorkspacePilotEditHostFullTests` |
| 253 | `WorkspacePilotEditHostTests.cs` | `TheReasonsOfTheFieldAreInSpanishExactlyAsTheTableOfThePackageSays` | 1552-1597 | C-0, C-1; copia mecánica en `WorkspacePilotEditHostFullTests` |
| 254 | `WorkspacePilotEditHostTests.cs` | `TheReasonOfAFieldThatCannotBeReadIsInSpanish` | 1599-1612 | C-0; copia mecánica en `WorkspacePilotEditHostFullTests` |
| 255 | `WorkspacePilotEditHostTests.cs` | `TheReasonsOfTheGateAndOfTheIntentionReadsAreInSpanish` | 1614-1640 | C-0, C-1; copia mecánica en `WorkspacePilotEditHostFullTests` |
| 256 | `WorkspacePilotEditHostTests.cs` | `TheReasonOfTheErrorOfADraftCrossesThePortOnlyInTheStateError` | 1643-1664 | C-0, C-1; copia mecánica en `WorkspacePilotEditHostFullTests` |
| 257 | `WorkspacePilotEditHostTests.cs` | `DestroyingASessionDropsOnlyItsOwnNoticeOfVetoAndQueuesItsLoss` | 1667-1689 | C-0, C-1; copia mecánica en `WorkspacePilotEditHostFullTests` |
| 258 | `WorkspaceCommandAdvisorTests.cs` | `ExistingIsNullBeforeTheFirstCreationAndAskingForItNeverCreatesNorCallsTheFactory` | 114-132 | C-0; copia mecánica en `WorkspaceCommandAdvisorFullTests` |
| 259 | `WorkspaceCommandAdvisorTests.cs` | `ExistingIsTheCreatedInstanceAfterwardsAndDoesNotChangeTheCreateCount` | 134-146 | C-0; copia mecánica en `WorkspaceCommandAdvisorFullTests` |
| 260 | `WorkspaceCommandAdvisorTests.cs` | `ExistingStaysNullWhenTheFactoryFailed` | 148-158 | C-0; copia mecánica en `WorkspaceCommandAdvisorFullTests` |
| 261 | `WorkspaceCommandAdvisorTests.cs` | `WithoutAControllerTheCommandSaysSoAndAsksNothingAtAll` | 225-238 | C-0; copia mecánica en `WorkspaceCommandAdvisorFullTests` |
| 262 | `WorkspaceCommandAdvisorTests.cs` | `WithoutAnActiveSessionTheCommandSaysSoAndNeitherReadsNorAsksNorDiscards` | 240-252 | C-0; copia mecánica en `WorkspaceCommandAdvisorFullTests` |
| 263 | `WorkspaceCommandAdvisorTests.cs` | `AnAmbiguousTargetSaysSoWithoutReadingTheRiskWithoutAskingAndWithoutDiscarding` | 254-268 | C-0; copia mecánica en `WorkspaceCommandAdvisorFullTests` |
| 264 | `WorkspaceCommandAdvisorTests.cs` | `AnUnknownFirstReadIsAnExplicitFailureAndNeverNothingAtRisk` | 270-283 | C-0; copia mecánica en `WorkspaceCommandAdvisorFullTests` |
| 265 | `WorkspaceCommandAdvisorTests.cs` | `AFirstReadThatThrowsCountsAsTheUnknownAndTheExceptionDoesNotEscape` | 285-297 | C-0; copia mecánica en `WorkspaceCommandAdvisorFullTests` |
| 266 | `WorkspaceCommandAdvisorTests.cs` | `WhenNothingIsAtRiskTheCommandSaysSoAndAsksNothing` | 299-311 | C-0; copia mecánica en `WorkspaceCommandAdvisorFullTests` |
| 267 | `WorkspaceCommandAdvisorTests.cs` | `WithRacksAtRiskTheCommandListsThemByRackIdAsksAndOnYesDiscardsTheWholeDocumentOnce` | 313-340 | C-0; copia mecánica en `WorkspaceCommandAdvisorFullTests` |
| 268 | `WorkspaceCommandAdvisorTests.cs` | `TheUncertainRacksAreListedApartAndAreDiscardedWithTheRestAfterTheConfirmation` | 342-361 | C-0; copia mecánica en `WorkspaceCommandAdvisorFullTests` |
| 269 | `WorkspaceCommandAdvisorTests.cs` | `WithOnlyUncertainRacksTheCommandListsThemAndTheFinalTextSaysTheDocumentWasDiscarded` | 363-381 | C-0; copia mecánica en `WorkspaceCommandAdvisorFullTests` |
| 270 | `WorkspaceCommandAdvisorTests.cs` | `AnythingButAYesDiscardsNothing` | 383-400 | C-0; copia mecánica en `WorkspaceCommandAdvisorFullTests` |
| 271 | `WorkspaceCommandAdvisorTests.cs` | `NothingIsDiscardedBeforeTheYes` | 402-414 | C-0; copia mecánica en `WorkspaceCommandAdvisorFullTests` |
| 272 | `WorkspaceCommandAdvisorTests.cs` | `WhenTheActiveSessionChangesDuringTheQuestionNothingIsDiscardedAndTheOtherSessionIsNeverAsked` | 416-430 | C-0; copia mecánica en `WorkspaceCommandAdvisorFullTests` |
| 273 | `WorkspaceCommandAdvisorTests.cs` | `WhenTheActiveSessionDisappearsDuringTheQuestionNothingIsDiscarded` | 432-443 | C-0; copia mecánica en `WorkspaceCommandAdvisorFullTests` |
| 274 | `WorkspaceCommandAdvisorTests.cs` | `WhenTheSessionStopsAuthorizingAfterTheYesTheTargetIsAmbiguousAndNothingIsReadAgainOrDiscarded` | 445-458 | C-0; copia mecánica en `WorkspaceCommandAdvisorFullTests` |
| 275 | `WorkspaceCommandAdvisorTests.cs` | `TheTargetIsOnlyAskedAboutTheSessionThatTheActiveDelegateJustGave` | 460-471 | C-0; copia mecánica en `WorkspaceCommandAdvisorFullTests` |
| 276 | `WorkspaceCommandAdvisorTests.cs` | `AnUnknownSecondReadDiscardsNothing` | 473-485 | C-0; copia mecánica en `WorkspaceCommandAdvisorFullTests` |
| 277 | `WorkspaceCommandAdvisorTests.cs` | `ASecondReadThatThrowsCountsAsTheUnknownAndDiscardsNothing` | 487-498 | C-0; copia mecánica en `WorkspaceCommandAdvisorFullTests` |
| 278 | `WorkspaceCommandAdvisorTests.cs` | `IfEitherListChangedBetweenTheTwoReadsNothingIsDiscarded` | 511-526 | C-0; copia mecánica en `WorkspaceCommandAdvisorFullTests` |
| 279 | `WorkspaceCommandAdvisorTests.cs` | `TheDiscardOfTheDocumentWithTheSameListsAfterTheYesIsTheOnlyOperationWithAnEffect` | 528-542 | C-0; copia mecánica en `WorkspaceCommandAdvisorFullTests` |
| 280 | `WorkspaceCommandAdvisorTests.cs` | `EveryAnswerThatIsNotDiscardedSaysWhyWithTheExactText` | 557-576 | C-0; copia mecánica en `WorkspaceCommandAdvisorFullTests` |
| 281 | `WorkspaceCommandAdvisorTests.cs` | `TheDefaultOutcomeOfTheDiscardIsTreatedAsARejection` | 578-588 | C-0; copia mecánica en `WorkspaceCommandAdvisorFullTests` |
| 282 | `WorkspaceCommandAdvisorTests.cs` | `ADelegateThatThrowsIsNotContainedAndNothingWasDiscardedBeforeTheDiscardItself` | 590-611 | C-0; copia mecánica en `WorkspaceCommandAdvisorFullTests` |
| 283 | `WorkspaceCommandAdvisorTests.cs` | `TheDelegatesOfTheSessionAndOfTheTargetAreNotContainedEither` | 613-628 | C-0; copia mecánica en `WorkspaceCommandAdvisorFullTests` |
| 284 | `WorkspaceCommandAdvisorTests.cs` | `ANullDelegateIsRefused` | 630-645 | C-0; copia mecánica en `WorkspaceCommandAdvisorFullTests` |
| 285 | `WorkspaceCommandAdvisorTests.cs` | `TheWrapperRefusesANullInnerPortNameActivityOrWriterAndAcceptsANullReport` | 710-724 | C-0; copia mecánica en `WorkspaceCommandAdvisorFullTests` |
| 286 | `WorkspaceCommandAdvisorTests.cs` | `TheDecisionOfTheWrapperIsAlwaysTheOneOfTheInnerAdvisorAndItIsAskedOncePerAttempt` | 726-744 | C-0; copia mecánica en `WorkspaceCommandAdvisorFullTests` |
| 287 | `WorkspaceCommandAdvisorTests.cs` | `AnInnerAdvisorThatThrowsIsAVetoWithTheNoticeAndTheReportOfThatContext` | 746-763 | C-0; copia mecánica en `WorkspaceCommandAdvisorFullTests` |
| 288 | `WorkspaceCommandAdvisorTests.cs` | `AVetoOfTheActiveDocumentWritesV01AndTheOneOfAnotherDocumentWritesV02` | 765-778 | C-0; copia mecánica en `WorkspaceCommandAdvisorFullTests` |
| 289 | `WorkspaceCommandAdvisorTests.cs` | `TheNameOfTheDocumentIsItsLastSegmentBetweenGuillemets` | 780-793 | C-0; copia mecánica en `WorkspaceCommandAdvisorFullTests` |
| 290 | `WorkspaceCommandAdvisorTests.cs` | `AMissingOrEmptyNameSaysTheDocument` | 795-809 | C-0; copia mecánica en `WorkspaceCommandAdvisorFullTests` |
| 291 | `WorkspaceCommandAdvisorTests.cs` | `AQueryThatThrowsNeverChangesTheDecisionAndTheNoticeStillSaysTheDocument` | 811-831 | C-0; copia mecánica en `WorkspaceCommandAdvisorFullTests` |
| 292 | `WorkspaceCommandAdvisorTests.cs` | `AWriterThatThrowsNeverChangesTheDecisionAndIsReportedAsTheVetoMessage` | 833-847 | C-0; copia mecánica en `WorkspaceCommandAdvisorFullTests` |
| 293 | `WorkspaceCommandAdvisorTests.cs` | `AReportThatThrowsIsIgnoredAndTheWrapperWorksWithoutAReportToo` | 849-867 | C-0; copia mecánica en `WorkspaceCommandAdvisorFullTests` |
| 294 | `WorkspaceCommandAdvisorTests.cs` | `TheWrapperNeverWritesWhenTheInnerAllowsEvenIfTheQueriesWouldFail` | 869-881 | C-0; copia mecánica en `WorkspaceCommandAdvisorFullTests` |
| 295 | `WorkspaceCommandAdvisorTests.cs` | `TheDestructionDelegatesExactlyOnceWithTheIntactFlagAsItCame` | 883-899 | C-0; copia mecánica en `WorkspaceCommandAdvisorFullTests` |
| 296 | `WorkspaceCommandAdvisorTests.cs` | `TheRiskIsReadBeforeTheDelegationAndTheMessageWaitsWithoutBeingWritten` | 901-923 | C-0; copia mecánica en `WorkspaceCommandAdvisorFullTests` |
| 297 | `WorkspaceCommandAdvisorTests.cs` | `ALossWithRacksAndUncertainOnesSaysBothAndALossWithOnlyUncertainOnesSaysThat` | 925-944 | C-0; copia mecánica en `WorkspaceCommandAdvisorFullTests` |
| 298 | `WorkspaceCommandAdvisorTests.cs` | `AnUnknownRiskIsNeverNothingAtRiskAndIsNeverSilentAndAKnownEmptyRiskSaysNothing` | 946-964 | C-0; copia mecánica en `WorkspaceCommandAdvisorFullTests` |
| 299 | `WorkspaceCommandAdvisorTests.cs` | `ARiskReadThatThrowsIsTheUnknownIsReportedAndStillDelegatesExactlyOnce` | 966-983 | C-0; copia mecánica en `WorkspaceCommandAdvisorFullTests` |
| 300 | `WorkspaceCommandAdvisorTests.cs` | `ARiskReadThatThrowsWithAReportThatAlsoThrowsStillDelegatesExactlyOnce` | 985-996 | C-0; copia mecánica en `WorkspaceCommandAdvisorFullTests` |
| 301 | `WorkspaceCommandAdvisorTests.cs` | `AnInnerAdvisorThatThrowsWhenTheSessionIsDestroyedIsNotContainedAndTheWorkBeforeItWasDone` | 998-1012 | C-0; copia mecánica en `WorkspaceCommandAdvisorFullTests` |
| 302 | `WorkspaceCommandAdvisorTests.cs` | `TheMessagesWaitForAnActiveSessionAreTakenOnceAndKeepTheirOrder` | 1014-1036 | C-0; copia mecánica en `WorkspaceCommandAdvisorFullTests` |
| 303 | `WorkspaceCommandAdvisorTests.cs` | `TheWrapperOnlyEverCallsTheReadOfTheRiskOfThePort` | 1038-1051 | C-0; copia mecánica en `WorkspaceCommandAdvisorFullTests` |
| 304 | `WorkspaceCommandAdvisorTests.cs` | `OverTheRealBridgeAndHostACleanDocumentIsAllowedAndNothingIsWritten` | 1163-1174 | C-0, C-1; copia mecánica en `WorkspaceCommandAdvisorFullTests` |
| 305 | `WorkspaceCommandAdvisorTests.cs` | `OverTheRealBridgeAndHostTheDecisionIsTheOneOfTheHostForVetoAndForAllowAndTheDraftIsNeverTouched` | 1176-1198 | C-0, C-1; copia mecánica en `WorkspaceCommandAdvisorFullTests` |
| 306 | `WorkspaceCommandAdvisorTests.cs` | `OverTheRealBridgeTheVetoOfAnotherDocumentSaysToActivateItFirst` | 1200-1214 | C-0, C-1; copia mecánica en `WorkspaceCommandAdvisorFullTests` |
| 307 | `WorkspaceCommandAdvisorTests.cs` | `OverTheRealBridgeAnInnerThatThrowsIsAVetoWithoutDegradingTheSessionAndWithoutAFaultOfTheCore` | 1216-1243 | C-0; copia mecánica en `WorkspaceCommandAdvisorFullTests` |
| 308 | `WorkspaceCommandAdvisorTests.cs` | `OverTheRealBridgeTheCloseSequenceIntactFlagReachesTheInnerAdvisorAsTheCoreDecidedIt` | 1245-1262 | C-0; copia mecánica en `WorkspaceCommandAdvisorFullTests` |
| 309 | `WorkspaceCommandAdvisorTests.cs` | `OverTheRealBridgeAndHostALostDocumentIsReadBeforeTheHostRetiresItAndTheMessageWaitsForTheNextActiveSession` | 1264-1288 | C-0, C-1; copia mecánica en `WorkspaceCommandAdvisorFullTests` |
| 310 | `WorkspaceCommandAdvisorTests.cs` | `OverTheRealBridgeADestroyedSessionIsNotAskedToTheBridgeNorToTheQueries` | 1290-1320 | C-0, C-1; copia mecánica en `WorkspaceCommandAdvisorFullTests` |
| 311 | `WorkspaceCommandAdvisorTests.cs` | `WithoutWiringTheWrapperTheRealBridgeStillDecidesAsTheHostAndWritesNothing` | 1322-1333 | C-0, C-1; copia mecánica en `WorkspaceCommandAdvisorFullTests` |
| 312 | `WorkspacePilotUpdateRequestTests.cs` | `ARequestOnADirtyDraftIsAcceptedAndTheOutcomeOfTheUpdateIsTakenOnceByRack` | 107-125 | C-0, C-1; copia mecánica en `WorkspacePilotUpdateRequestFullTests` |
| 313 | `WorkspacePilotUpdateRequestTests.cs` | `TheStartValueWithoutAnExecutorOrAReaderIsUnavailableAndTheDefaultValuesAreInert` | 127-149 | C-0, C-1; copia mecánica en `WorkspacePilotUpdateRequestFullTests` |
| 314 | `WorkspacePilotUpdateRequestTests.cs` | `TheGateAnswersWaitingForNoDocumentAModalAndNoRestAndRejectedForAnotherSessionAndRunsNothing` | 151-172 | C-0, C-1; copia mecánica en `WorkspacePilotUpdateRequestFullTests` |
| 315 | `WorkspacePilotUpdateRequestTests.cs` | `ARequestInsideANoticeOfThePanelWaitsAndAnyOtherStartIsNotATerminalFailure` | 174-190 | C-0, C-1; copia mecánica en `WorkspacePilotUpdateRequestFullTests` |
| 316 | `WorkspacePilotUpdateRequestTests.cs` | `ARequestWhileAReadOfThePilotIsInProgressIsBusyAndAsksNothing` | 192-209 | C-0, C-1; copia mecánica en `WorkspacePilotUpdateRequestFullTests` |
| 317 | `WorkspacePilotUpdateRequestTests.cs` | `AnExecutorThatThrowsIsAFailedStartValueAndLeavesNoTicketNorOutcome` | 211-226 | C-0, C-1; copia mecánica en `WorkspacePilotUpdateRequestFullTests` |
| 318 | `WorkspacePilotUpdateRequestTests.cs` | `ASecondRequestWhileTheFirstIsInFlightIsBusyAndTheExecutorIsAskedOnceAndAnotherIsAcceptedAfterTheEnd` | 230-251 | C-0, C-1; copia mecánica en `WorkspacePilotUpdateRequestFullTests` |
| 319 | `WorkspacePilotUpdateRequestTests.cs` | `TheUpdateOfT4BRunsOncePerAcceptedRequestAlsoWithTwoClicksAndARepeatedCallback` | 253-269 | C-0, C-1; copia mecánica en `WorkspacePilotUpdateRequestFullTests` |
| 320 | `WorkspacePilotUpdateRequestTests.cs` | `ADocumentThatChangedBeforeTheCallbackIsARejectedOutcomeAndTheDraftIsUntouched` | 273-292 | C-0, C-1; copia mecánica en `WorkspacePilotUpdateRequestFullTests` |
| 321 | `WorkspacePilotUpdateRequestTests.cs` | `ASessionDestroyedBetweenTheAcceptanceAndTheCallbackIsStoppedBeforeTheLockAndLeavesNoTicketNorOutcome` | 294-310 | C-0, C-1; copia mecánica en `WorkspacePilotUpdateRequestFullTests` |
| 322 | `WorkspacePilotUpdateRequestTests.cs` | `ARecycledNativeIdentityFindsTheNewDocumentAndTheAdmissionOfTheOldSessionStopsItBeforeTheLock` | 312-330 | C-0, C-1; copia mecánica en `WorkspacePilotUpdateRequestFullTests` |
| 323 | `WorkspacePilotUpdateRequestTests.cs` | `AContextThatFailsBeforeTheCallbackIsOnlyAFailedStartValueAndNoUpdateRunsAndTheDraftKeepsEverything` | 334-350 | C-0, C-1; copia mecánica en `WorkspacePilotUpdateRequestFullTests` |
| 324 | `WorkspacePilotUpdateRequestTests.cs` | `ANativeRefusalThatNeverRunsNorCompletesExpiresWithTheFirstForeignCommandAndLeavesAFailedOnceAndFreesEverything` | 352-376 | C-0, C-1; copia mecánica en `WorkspacePilotUpdateRequestFullTests` |
| 325 | `WorkspacePilotUpdateRequestTests.cs` | `ALateCallbackWithTheWitnessOfAnExpiredTicketDoesNothingAndLeavesNoSecondOutcome` | 378-395 | C-0, C-1; copia mecánica en `WorkspacePilotUpdateRequestFullTests` |
| 326 | `WorkspacePilotUpdateRequestTests.cs` | `ARetiredSessionTakesTheTicketAndTheStoreWithIt` | 397-410 | C-0, C-1; copia mecánica en `WorkspacePilotUpdateRequestFullTests` |
| 327 | `WorkspacePilotUpdateRequestTests.cs` | `TheEntriesIntoRestAndTheStartOfTheOwnCommandThatH0MeasuredBeforeTheCallbackDoNotExpireTheTicket` | 414-432 | C-0, C-1; copia mecánica en `WorkspacePilotUpdateRequestFullTests` |
| 328 | `WorkspacePilotUpdateRequestTests.cs` | `TheCompletionReleasesTheTicketAfterTheUpdateAndUntilThenARequestIsBusyAndTheProgressIsNotInFlight` | 434-452 | C-0, C-1; copia mecánica en `WorkspacePilotUpdateRequestFullTests` |
| 329 | `WorkspacePilotUpdateRequestTests.cs` | `ACompletionWithoutAnyCallbackLeavesAFailedOutcomeOnceAndReleasesTheTicket` | 454-470 | C-0, C-1; copia mecánica en `WorkspacePilotUpdateRequestFullTests` |
| 330 | `WorkspacePilotUpdateRequestTests.cs` | `ATicketThatNeverEndedAfterAnAdmissionExpiresWithTheNextForeignCommandAndAnUpdateInExecuteNeverExpires` | 472-497 | C-0, C-1; copia mecánica en `WorkspacePilotUpdateRequestFullTests` |
| 331 | `WorkspacePilotUpdateRequestTests.cs` | `ASessionThatIsDegradedIsUnavailableAndOneDegradedBeforeTheCallbackExpiresTheTicket` | 508-529 | C-0, C-1; copia mecánica en `WorkspacePilotUpdateRequestFullTests` |
| 332 | `WorkspacePilotUpdateRequestTests.cs` | `TheExpiryOfATicketOfTheSameRackInsideARequestIsTheStartValueFailedAndNothingIsAcceptedNorStored` | 531-551 | C-0, C-1; copia mecánica en `WorkspacePilotUpdateRequestFullTests` |
| 333 | `WorkspacePilotUpdateRequestTests.cs` | `TheExpiryOfATicketOfAnotherRackInsideARequestKeepsItsFailedUnderItsRackAndTheRequestGoesOn` | 553-573 | C-0, C-1; copia mecánica en `WorkspacePilotUpdateRequestFullTests` |
| 334 | `WorkspacePilotUpdateRequestTests.cs` | `ATicketThatEndedWhoseCompletionNeverArrivesExpiresWithTheNextForeignCommandAndFreesTheRequestAndTheDiscardsAndKeepsTheOutcomeOnce` | 577-609 | C-0, C-1; copia mecánica en `WorkspacePilotUpdateRequestFullTests` |
| 335 | `WorkspacePilotUpdateRequestTests.cs` | `AForeignCommandDuringTheUpdateDoesNotExpireTheTicketDuringTheRunAndAfterTheRunTheTicketIsFreedByTheNextCheckWithoutASecondOutcome` | 611-642 | C-0, C-1; copia mecánica en `WorkspacePilotUpdateRequestFullTests` |
| 336 | `WorkspacePilotUpdateRequestTests.cs` | `ASecondTerminalCallOnATicketThatAlreadyEndedDoesNothingAndTheFirstOutcomeIsKept` | 663-707 | C-0, C-1; copia mecánica en `WorkspacePilotUpdateRequestFullTests` |
| 337 | `WorkspacePilotUpdateRequestTests.cs` | `AnExecutorThatThrowsAfterTheCallbackBeganIsAnAcceptedStartValueAndTheOutcomeIsTheOnlyChannel` | 709-726 | C-0, C-1; copia mecánica en `WorkspacePilotUpdateRequestFullTests` |
| 338 | `WorkspacePilotUpdateRequestTests.cs` | `AnExecutorThatThrowsBeforeTheCallbackBeganIsStillAFailedStartValueEvenInTheDeferredMode` | 728-741 | C-0, C-1; copia mecánica en `WorkspacePilotUpdateRequestFullTests` |
| 339 | `WorkspacePilotUpdateRequestTests.cs` | `AComparerOrAnythingThatFailsWhileLookingForADraftBeingAppliedAnswersFalseAndIsLoggedSoTheDiscardIsNeverStuckInBusy` | 743-755 | C-0; copia mecánica en `WorkspacePilotUpdateRequestFullTests` |
| 340 | `WorkspacePilotUpdateRequestTests.cs` | `ARequestOfASessionThatIsNoLongerTheActiveOneIsRejectedByItsAuthorizationAndNeverWaitsForTheFlightOfThreeOperations` | 759-776 | C-0, C-1, C-3; a mano (R-C3) |
| 341 | `WorkspacePilotUpdateRequestTests.cs` | `AConflictOfT4BIsKeptWithItsDifferencesAndTheHostWritesNothingMore` | 780-796 | C-0, C-1; copia mecánica en `WorkspacePilotUpdateRequestFullTests` |
| 342 | `WorkspacePilotUpdateRequestTests.cs` | `TheOutcomeThatT4BGivesIsKeptWithTheSameStatusReasonDifferencesAndWarningsAsThePlainCallOfTheUpdate` | 817-836 | C-0, C-1, C-4; a mano (R-C4) |
| 343 | `WorkspacePilotUpdateRequestTests.cs` | `AnUnknownOutcomeOfT4BIsNeverTranslatedAndAnUnlockThatThrowsAfterwardsDoesNotChangeIt` | 838-860 | C-0, C-1; copia mecánica en `WorkspacePilotUpdateRequestFullTests` |
| 344 | `WorkspacePilotUpdateRequestTests.cs` | `AnAppliedOutcomeKeepsTheDraftCleanOnlyByTheClassificationOfT4BAndTheWarningsAreCopied` | 862-876 | C-0, C-1; copia mecánica en `WorkspacePilotUpdateRequestFullTests` |
| 345 | `WorkspacePilotUpdateRequestTests.cs` | `AHostFailureAfterTheAdmissionAlwaysRemovesTheTicketClosesTheOwnMutationIfItWasOpenedAndALaterRequestIsAccepted` | 886-927 | C-0, C-1; copia mecánica en `WorkspacePilotUpdateRequestFullTests` |
| 346 | `WorkspacePilotUpdateRequestTests.cs` | `ALockThatThrowsLeavesAFailedOutcomeWithItsPrefixAndNeverOpensTheOwnMutation` | 929-943 | C-0, C-1; copia mecánica en `WorkspacePilotUpdateRequestFullTests` |
| 347 | `WorkspacePilotUpdateRequestTests.cs` | `TheRunOpensTheOwnMutationOnceBeforeTheUpdateAndAFailureBeforeTheUpdateClosesIt` | 947-965 | C-0, C-1; copia mecánica en `WorkspacePilotUpdateRequestFullTests` |
| 348 | `WorkspacePilotUpdateRequestTests.cs` | `DuringTheFlightTheFiveOperationsThatCouldChangeTheDraftAnswerBusyOrWaitingAndTheDraftDoesNotChange` | 972-1003 | C-0, C-1, C-3; a mano (R-C3) |
| 349 | `WorkspacePilotUpdateRequestTests.cs` | `TheGeneralCheckDuringTheFlightWaitsBeforeItsRereadEvenWhenTheRereadWouldDifferAndTheDraftStaysDirty` | 1005-1020 | C-0, C-1; copia mecánica en `WorkspacePilotUpdateRequestFullTests` |
| 350 | `WorkspacePilotUpdateRequestTests.cs` | `AnotherRackOfTheSessionIsStillEditableDuringTheFlightOfTheFirst` | 1022-1033 | C-0, C-1; copia mecánica en `WorkspacePilotUpdateRequestFullTests` |
| 351 | `WorkspacePilotUpdateRequestTests.cs` | `TheDiscardsAnswerBusyDuringTheFlightAndOnAnApplyingDraftAndTheRegistryIsUnchanged` | 1035-1065 | C-0, C-1; copia mecánica en `WorkspacePilotUpdateRequestFullTests` |
| 352 | `WorkspacePilotUpdateRequestTests.cs` | `TheDiscardsAreServedAgainWhenTheTicketExpiresWithAForeignCommand` | 1067-1079 | C-0, C-1; copia mecánica en `WorkspacePilotUpdateRequestFullTests` |
| 353 | `WorkspacePilotUpdateRequestTests.cs` | `TheUpdateBoundaryIsBlockingAndAppliesAValidPendingTextAndAbortsAnInvalidOneAndNeedsTheAuthorization` | 1083-1110 | C-0, C-1; copia mecánica en `WorkspacePilotUpdateRequestFullTests` |
| 354 | `WorkspacePilotUpdateRequestTests.cs` | `TheTextsOfTheGateOfTheUpdateAreTheConstantsOfTheRunWithTheirOldWording` | 1112-1127 | C-0, C-1, C-4; a mano (R-C4) |
| 355 | `WorkspacePilotUpdateRequestTests.cs` | `AnUpdateOfTheCoreThatRunsOverASlotThatWasTakenAwayNeverAnswersAnApplied` | 1131-1146 | C-0, C-1, C-4; a mano (R-C4) |
| 356 | `WorkspacePilotUpdateHostTests.cs` | `TheGateRejectsAModalAWrongSessionAndNoDocumentWithoutReadingAnythingAndWithoutTheSlot` | 70-93 | C-0, C-1, C-4; a mano (R-C4) |
| 357 | `WorkspacePilotUpdateHostTests.cs` | `TheGateHasNoRestAndNoReaderSoACommandInProgressAndAMissingReaderDoNotStopTheUpdate` | 95-113 | C-0, C-1, C-4; a mano (R-C4) |
| 358 | `WorkspacePilotUpdateHostTests.cs` | `TheUpdateOfThePilotNeverUsesTheReaderOfTheIntention` | 115-126 | C-0, C-1, C-4; a mano (R-C4) |
| 359 | `WorkspacePilotUpdateHostTests.cs` | `ABlankRackAndAnUnknownRackHaveNoDraftAndNothingIsRun` | 130-144 | C-0, C-1, C-4; a mano (R-C4) |
| 360 | `WorkspacePilotUpdateHostTests.cs` | `TheCheckBeforeTheSlotAnswersNothingToApplyPendingTextAndNotApplicableWithoutTakingTheSlot` | 146-175 | C-0, C-1, C-4; a mano (R-C4) |
| 361 | `WorkspacePilotUpdateHostTests.cs` | `AnotherUpdateInProgressInTheRegistryAnswersItAndRunsNothing` | 179-194 | C-0, C-1, C-4; a mano (R-C4) |
| 362 | `WorkspacePilotUpdateHostTests.cs` | `ADraftThatIsAlreadyApplyingIsRefusedWithTheNameOfTheRefusal` | 196-208 | C-0, C-1, C-4; a mano (R-C4) |
| 363 | `WorkspacePilotUpdateHostTests.cs` | `AFailureBeforeTheSlotIsFailedAndLoggedAndTheSessionIsNotDegraded` | 210-224 | C-0, C-1, C-4; a mano (R-C4) |
| 364 | `WorkspacePilotUpdateHostTests.cs` | `TheSlotIsClosedExactlyOnceAndIsFreeAfterEveryAnswer` | 226-242 | C-0, C-1, C-4; a mano (R-C4) |
| 365 | `WorkspacePilotUpdateHostTests.cs` | `AnAppliedUpdateRebasesTheDraftAndRunsThePortWithTheAuthorizedDocumentAndTheRack` | 246-264 | C-0, C-1, C-4; a mano (R-C4) |
| 366 | `WorkspacePilotUpdateHostTests.cs` | `AConflictOfMutateIsNotAppliedConflictWithTheDifferencesAndTheDraftGoesToConflict` | 266-280 | C-0, C-1, C-4; a mano (R-C4) |
| 367 | `WorkspacePilotUpdateHostTests.cs` | `ANotAdmittedReadWithTheBaseOfTheDraftIsNotAppliedAdmissionAndTheDraftKeepsItsStatus` | 282-296 | C-0, C-1, C-4; a mano (R-C4) |
| 368 | `WorkspacePilotUpdateHostTests.cs` | `APreflightAndAFailureKeepTheDraftAsItWasAndCarryTheirCause` | 298-316 | C-0, C-1, C-4; a mano (R-C4) |
| 369 | `WorkspacePilotUpdateHostTests.cs` | `ASealThatThrewIsCommitThrewAndTheDraftIsUnknownWithItsOrigin` | 318-331 | C-0, C-1, C-4; a mano (R-C4) |
| 370 | `WorkspacePilotUpdateHostTests.cs` | `AnAppliedUpdateWithWarningsKeepsThemAndTheDraftIsClean` | 333-347 | C-0, C-1, C-4; a mano (R-C4) |
| 371 | `WorkspacePilotUpdateHostTests.cs` | `APostThatFailedIsAppliedPostRereadFailedWithTheCauseAndTheWarningsAndTheDraftIsUnknown` | 349-365 | C-0, C-1, C-4; a mano (R-C4) |
| 372 | `WorkspacePilotUpdateHostTests.cs` | `ADraftThatWasDiscardedDuringTheRunIsClosedDetachedAndTheSlotIsFree` | 367-381 | C-0, C-1, C-4; a mano (R-C4) |
| 373 | `WorkspacePilotUpdateHostTests.cs` | `AnErrorThatArrivesDuringTheRunIsAppliedAfterTheClassificationAndTheAnswerIsClosedInError` | 383-402 | C-0, C-1, C-4; a mano (R-C4) |
| 374 | `WorkspacePilotUpdateHostTests.cs` | `AnErrorThatArrivesDuringAnUpdateWithWarningsKeepsTheWarningsAndTheClassificationOfTheClosedInErrorAnswer` | 404-418 | C-0, C-1, C-4; a mano (R-C4) |
| 375 | `WorkspacePilotUpdateHostTests.cs` | `AnErrorThatArrivesDuringAConflictKeepsTheConflictAsTheClassificationAndWritesNothing` | 420-434 | C-0, C-1, C-4; a mano (R-C4) |
| 376 | `WorkspacePilotUpdateHostTests.cs` | `AnErrorThatArrivesDuringACommitThatThrewKeepsTheUnknownVisibleAsTheClassificationWithItsCause` | 436-449 | C-0, C-1, C-4; a mano (R-C4) |
| 377 | `WorkspacePilotUpdateHostTests.cs` | `TheAnswerOfAnUpdateHasTheStatusOfTheClassOfOutcomeThatClosedTheDraftForEveryClass` | 462-498 | C-0, C-1, C-4; a mano (R-C4) |
| 378 | `WorkspacePilotUpdateHostTests.cs` | `TheAnswerOfAnUpdateAlwaysComesFromTheCloseAndTheOutcomeNeverNamesADraft` | 500-515 | C-0, C-4; a mano (R-C4) |
| 379 | `WorkspacePilotUpdateHostTests.cs` | `TheStatesOfTheUpdateHaveTheExactNamesAndOrderOfThePackageAndNotApplyingIsNotOneOfThem` | 517-534 | C-0; copia mecánica en `WorkspacePilotUpdateHostFullTests` |
| 380 | `WorkspacePilotUpdateHostTests.cs` | `TheFactoriesOfTheOutcomeCarryTheirStatusTheirReasonAndCopiesOfTheLists` | 536-568 | C-0; copia mecánica en `WorkspacePilotUpdateHostFullTests` |
| 381 | `WorkspacePilotUpdateHostTests.cs` | `TheClosedOutcomeCopiesTheListsOnReceiptAndMutatingTheOriginAfterwardsChangesNothing` | 571-586 | C-0; copia mecánica en `WorkspacePilotUpdateHostFullTests` |
| 382 | `WorkspacePilotUpdateHostTests.cs` | `RowAppliedAndAppliedWithWarningsWhoseCopierThrowsIsUnknownPostRereadWithCauseAndNothingIsRebased` | 621-655 | C-0; copia mecánica en `WorkspacePilotUpdateHostFullTests` |
| 383 | `WorkspacePilotUpdateHostTests.cs` | `RowAppliedWhoseComparatorThrowsIsUnknownPostRereadAndNothingIsRebased` | 657-678 | C-0; copia mecánica en `WorkspacePilotUpdateHostFullTests` |
| 384 | `WorkspacePilotUpdateHostTests.cs` | `RowAppliedCopiesBeforeTheSlotIsFreedSoAnotherUpdateStillCannotTakeItWhileTheCopyRuns` | 680-701 | C-0; copia mecánica en `WorkspacePilotUpdateHostFullTests` |
| 385 | `WorkspacePilotUpdateHostTests.cs` | `RowCommitThrewIsKeptAsUnknownCommitThrewAndNoStateIsComputed` | 703-720 | C-0; copia mecánica en `WorkspacePilotUpdateHostFullTests` |
| 386 | `WorkspacePilotUpdateHostTests.cs` | `RowNotAppliedWhoseStateCannotBeComputedGoesToErrorWithItsReasonAndNeverToApplied` | 722-755 | C-0; copia mecánica en `WorkspacePilotUpdateHostFullTests` |
| 387 | `WorkspacePilotUpdateHostTests.cs` | `RowNotAppliedConflictKeepsItsClassificationEvenWhenTheComparerThrows` | 757-773 | C-0; copia mecánica en `WorkspacePilotUpdateHostFullTests` |
| 388 | `WorkspacePilotUpdateHostTests.cs` | `RowAllTheDeferredErrorIsStillAppliedAfterTheBranchWhetherOrNotTheCopierThrew` | 775-794 | C-0; copia mecánica en `WorkspacePilotUpdateHostFullTests` |
| 389 | `WorkspacePilotUpdateHostTests.cs` | `RowAllADraftThatLeftTheRegistryDuringTheUpdateIsClosedDetachedWithoutEvaluatingTheComparer` | 796-819 | C-0; copia mecánica en `WorkspacePilotUpdateHostFullTests` |
| 390 | `WorkspacePilotUpdateHostTests.cs` | `TheWarningsThatWereKnownSurviveWhenThePostRereadFailsOrIsNotAdmitted` | 823-851 | C-0; copia mecánica en `WorkspacePilotUpdateHostFullTests` |
| 391 | `WorkspacePilotReconciliationHostTests.cs` | `ConfirmWithAChangeOnlyOfTheTextOfTheScaleIsAConflictOfTheOriginAndNothingElse` | 126-142 | C-0, C-1; copia mecánica en `WorkspacePilotReconciliationHostFullTests` |
| 392 | `WorkspacePilotReconciliationHostTests.cs` | `ConfirmWithAChangeOnlyOfAnotherFieldIsAConflictToo` | 144-158 | C-0, C-1; copia mecánica en `WorkspacePilotReconciliationHostFullTests` |
| 393 | `WorkspacePilotReconciliationHostTests.cs` | `ConfirmWithAChangeOfTheScaleAndOfAnotherFieldIsAConflictAndKeepAfterwardsKeepsTheScaleOfTheDraftOnTheNewVersion` | 160-179 | C-0, C-1; copia mecánica en `WorkspacePilotReconciliationHostFullTests` |
| 394 | `WorkspacePilotReconciliationHostTests.cs` | `ARackDeletedFromTheDrawingReadsAnEmptyBaseAndThatAdmissionIsNotAdmittedWithTheMissingOrigin` | 185-193 | C-0; copia mecánica en `WorkspacePilotReconciliationHostFullTests` |
| 395 | `WorkspacePilotReconciliationHostTests.cs` | `ConfirmOfADeletedRackIsAConflictWithTheDiagnosticAndTheHintOfTheClassicEditor` | 195-210 | C-0, C-1; copia mecánica en `WorkspacePilotReconciliationHostFullTests` |
| 396 | `WorkspacePilotReconciliationHostTests.cs` | `KeepAndDiscardOfADeletedRackAnswerNotAdmittedWithTheDiagnosticAndTheHintAndChangeNothing` | 212-233 | C-0, C-1; copia mecánica en `WorkspacePilotReconciliationHostFullTests` |
| 397 | `WorkspacePilotReconciliationHostTests.cs` | `ConfirmOfARackThatIsNoLongerEditableWithTheSameBaseIsConfirmedWithTheDiagnosticAndTheHint` | 237-253 | C-0, C-1, C-2, C-9; a mano (R-C2,R-C9) |
| 398 | `WorkspacePilotReconciliationHostTests.cs` | `ConfirmOfARackThatIsNoLongerEditableWithAnotherBaseIsAConflictWithBothThings` | 255-271 | C-0, C-1, C-2, C-9; a mano (R-C2,R-C9) |
| 399 | `WorkspacePilotReconciliationHostTests.cs` | `ADiagnosticAndAHintOnlyComeWithAReadThatWasNotAdmitted` | 273-290 | C-0, C-1; copia mecánica en `WorkspacePilotReconciliationHostFullTests` |
| 400 | `WorkspacePilotReconciliationHostTests.cs` | `ConfirmOfAnUnreadableEnvelopeCarriesTheDiagnosticOfAnUnreadableOrFutureEnvelopeAndTheHint` | 300-313 | C-0, C-1; copia mecánica en `WorkspacePilotReconciliationHostFullTests` |
| 401 | `WorkspacePilotReconciliationHostTests.cs` | `KeepAndDiscardOfAnUnreadableEnvelopeAnswerNotAdmittedWithItsDiagnosticAndChangeNothing` | 315-334 | C-0, C-1; copia mecánica en `WorkspacePilotReconciliationHostFullTests` |
| 402 | `WorkspacePilotReconciliationHostTests.cs` | `ADefaultReadResultAnswersFailedForTheThreeOperationsAndChangesNothing` | 338-358 | C-0, C-1; copia mecánica en `WorkspacePilotReconciliationHostFullTests` |
| 403 | `WorkspacePilotReconciliationHostTests.cs` | `AnExceptionInsideTheComparisonOfTheGeneralCheckIsFailedReportedAndChangesNothing` | 360-383 | C-0, C-1, C-9; a mano (R-C9) |
| 404 | `WorkspacePilotReconciliationHostTests.cs` | `AnInvalidPendingTextDoesNotBlockTheThreeOperationsConfirmAndKeepLeaveItExactlyAsTypedAndDiscardEmptiesIt` | 389-418 | C-0, C-1; copia mecánica en `WorkspacePilotReconciliationHostFullTests` |
| 405 | `WorkspacePilotReconciliationHostTests.cs` | `ConfirmOfAConflictWithAnInvalidPendingTextKeepsTheTextTheConflictAndTheDifferences` | 420-437 | C-0, C-1; copia mecánica en `WorkspacePilotReconciliationHostFullTests` |
| 406 | `WorkspacePilotReconciliationHostTests.cs` | `ASessionThatNeverExistedIsRejectedForTheThreeOperationsWithoutReadingAndWithoutChange` | 441-463 | C-0, C-1; copia mecánica en `WorkspacePilotReconciliationHostFullTests` |
| 407 | `WorkspacePilotReconciliationHostTests.cs` | `AnotherActiveDocumentRejectsTheThreeOperationsWithoutReadingAndLeavesTheBaseTheInitialTheCurrentAndThePendingTextIntact` | 465-491 | C-0, C-1; copia mecánica en `WorkspacePilotReconciliationHostFullTests` |
| 408 | `WorkspacePilotReconciliationHostTests.cs` | `TheFactoriesOfTheAnswerCarryTheDiagnosticAndTheHintOnlyWhereTheyAreGivenAndTheOtherAnswersCarryNone` | 495-522 | C-0; copia mecánica en `WorkspacePilotReconciliationHostFullTests` |
| 409 | `WorkspacePilotReconciliationHostTests.cs` | `TheAnswerOfTheGeneralCheckOnlyCarriesAStateTextAndListsOfTextSoItCannotAuthorizeAWrite` | 524-534 | C-0; copia mecánica en `WorkspacePilotReconciliationHostFullTests` |
| 410 | `WorkspacePilotReconciliationHostTests.cs` | `TheThreeOperationsStillReadTheDrawingExactlyOnceAndTheGeneralCheckStillComparesWithTheBaseOfTheDraft` | 536-557 | C-0, C-1; copia mecánica en `WorkspacePilotReconciliationHostFullTests` |
| 411 | `WorkspacePilotReconciliationHostTests.cs` | `CheckNowWithARereadThatIsNotAdmittedAndMatchesTheBaseIsRecoveredAndCarriesTheDiagnosticAndTheHint` | 590-607 | C-0, C-1, C-9; a mano (R-C9) |
| 412 | `WorkspacePilotReconciliationHostTests.cs` | `CheckNowWithARereadThatIsNotAdmittedAndDiffersFromTheBaseIsAConflictAndCarriesTheDiagnosticAndTheHint` | 609-625 | C-0, C-1, C-9; a mano (R-C9) |
| 413 | `WorkspacePilotReconciliationHostTests.cs` | `CheckNowWithAnAdmittedRereadCarriesNoDiagnosticNorHintInARecoveryNorInAConflict` | 627-649 | C-0, C-1; copia mecánica en `WorkspacePilotReconciliationHostFullTests` |
| 414 | `WorkspacePilotReconciliationHostTests.cs` | `TheDiagnosticNeverDecidesTheClassificationTheSameBaseGivesTheSameAnswerAndTheSameDraftWithOrWithoutAdmission` | 651-668 | C-0, C-1, C-9; a mano (R-C9) |
| 415 | `WorkspacePilotReconciliationHostTests.cs` | `AFailureOfTheDiagnosticGivesNullAndNeverChangesTheClassificationNorTurnsAChangedDraftIntoFailed` | 682-712 | C-0, C-1, C-9; a mano (R-C9) |
| 416 | `WorkspacePilotReconciliationHostTests.cs` | `TheDiagnosticOfCheckNowReadsTheDrawingExactlyOnceAndWritesNothingAndANotAdmittedRereadStillReadsOnce` | 714-727 | C-0, C-1, C-9; a mano (R-C9) |
| 417 | `WorkspacePilotReconciliationHostTests.cs` | `TheAnswerOfCheckNowCarriesTheDiagnosticOnlyWhereItIsGivenAndEveryOtherAnswerHasNone` | 729-757 | C-0; copia mecánica en `WorkspacePilotReconciliationHostFullTests` |
| 418 | `WorkspacePilotReconciliationHostTests.cs` | `TheAnswerOfCheckNowOnlyCarriesAStateTextAndListsOfTextAndTheStatusesAreStillTheEightOfBefore` | 759-771 | C-0; copia mecánica en `WorkspacePilotReconciliationHostFullTests` |
| 419 | `WorkspaceAssociativeBookkeepingTests.cs` | `AnAssociativeBookkeepingObjectEventQueuesNoHintAndHooksNoIdle` | 68-91 | C-0; copia mecánica en `WorkspaceAssociativeBookkeepingFullTests` |
| 420 | `WorkspaceAssociativeBookkeepingTests.cs` | `AfterTheUpdateClosedItsScopeTheBookkeepingOfTheReReadDoesNotMarkTheDrafts` | 95-114 | C-0; copia mecánica en `WorkspaceAssociativeBookkeepingFullTests` |
| 421 | `WorkspaceAssociativeBookkeepingTests.cs` | `AReadThatOnlyRaisesBookkeepingLeavesTheDraftUnmarked` | 145-169 | C-0, C-1; copia mecánica en `WorkspaceAssociativeBookkeepingFullTests` |
| 422 | `WorkspaceAssociativeBookkeepingTests.cs` | `ADrawingObjectEventHasExactlyTheEffectOfASourceEventAlsoInsideTheOwnScope` | 173-192 | C-0; copia mecánica en `WorkspaceAssociativeBookkeepingFullTests` |
| 423 | `WorkspaceAssociativeBookkeepingTests.cs` | `ADrawingObjectChangeAndTheEndOfACommandDrainTheListOfTheBridgeTestsOfTheDrain` | 194-205 | C-0; copia mecánica en `WorkspaceAssociativeBookkeepingFullTests` |
| 424 | `WorkspaceAssociativeBookkeepingTests.cs` | `AnySourceThatIsNotAnObjectOneHasTheEffectOfASourceEventWhateverTheSubject` | 207-224 | C-0; copia mecánica en `WorkspaceAssociativeBookkeepingFullTests` |
| 425 | `WorkspaceAssociativeBookkeepingTests.cs` | `EveryObjectEventAlsoTheBookkeepingOneStillForgetsTheCloseSequence` | 241-273 | C-0; copia mecánica en `WorkspaceAssociativeBookkeepingFullTests` |
| 426 | `WorkspaceAssociativeBookkeepingTests.cs` | `TheBookkeepingNeitherOpensNorClosesTheOwnScopeNorChangesTheStateOfRestNorTheCountOfForeignCommands` | 275-305 | C-0; copia mecánica en `WorkspaceAssociativeBookkeepingFullTests` |
| 427 | `WorkspaceAssociativeBookkeepingTests.cs` | `ASessionThatIsUnknownDegradedOrDestroyedGivesNoEffectToAnObjectEventOfEitherSubject` | 307-339 | C-0; copia mecánica en `WorkspaceAssociativeBookkeepingFullTests` |
| 428 | `WorkspaceAssociativeBookkeepingTests.cs` | `AnUndoRightAfterTheUpdateIsForeignAndMarksTheDraftsAlsoWithTheBookkeepingAroundIt` | 341-367 | C-0; copia mecánica en `WorkspaceAssociativeBookkeepingFullTests` |

## PB-T4b — C-5 y C-6: la letra de las guardas (15)

| # | Archivo | Método | Líneas | Causa |
|---|---|---|---|---|
| 1 | `PushBackWritePathGuardTests.cs` | `CT_PBT3B_13_NoFileOutsideThePlanTheRunAndTheModelNamesTheFullWriteAndTheHostFindsOnlyTheDraftOfThePilot` | 251-299 | C-5: CT-PBT3B-13 (el anfitrion encuentra el borrador completo) |
| 2 | `WorkspacePilotUpdateIntegrationGuardTests.cs` | `G4OnlyTheExecuteOfTheRunOfTheCoreCallsTheUpdateAndOnlyTheCoreCallsTheRunOfT4B` | 504-508 | C-5: G-4 (IG:497-498, `PushBackPilotUpdateRun.Execute` deja de nombrarse; la prueba exige `Empty` sobre el texto real) |
| 3 | `WorkspacePilotUpdateIntegrationGuardTests.cs` | `G4TripsOnAGroupOfMethodsOfTheUpdateOutsideTheCoreAndInsideItAndNotOnTheNameInsideAString` | 519-532 | C-5: G-4 (IG:497-498, `PushBackPilotUpdateRun.Execute` deja de nombrarse; la prueba exige `Empty` sobre el texto real) |
| 4 | `WorkspaceEditIntentGuardTests.cs` | `TheBaseOfAResultOfThePilotOnlyReachesBeginCheckNowOrDiscardAndReloadInTheHostCoreAndTheTwoOperationsCountTheirOwnUses` | 664-690 | C-5: EIG:664-690, `pilot.Read` en `Begin` pasa a `full.Read` |
| 5 | `WorkspaceEditIntentGuardTests.cs` | `ThePerOperationCountsOfTheBaseAndOfTheReadInTheThreeBodiesOfTheReconciliationAreTheirOwnConsumer` | 692-721 | C-5: EIG:692-721, `KeepPilotEditsOn` en :711 (C-3) |
| 6 | `WorkspaceEditIntentGuardTests.cs` | `TheResultOfThePilotIsOnlyEverNamedPilotInTheHostCoreSoNoAliasEscapesTheGuardOfTheUseOfTheBase` | 727-737 | C-5: EIG:725-737, las seis `var pilot = read.PilotRead;` |
| 7 | `WorkspaceEditIntentGuardTests.cs` | `TheThreeBodiesOfTheReconciliationFollowTheStepsOfTheCheckAndTheReloadWithTheApplicabilityBeforeTheOneRead` | 754-778 | C-5: EIG:754-778, `KeepPilotEditsOn` en :776 (C-3) |
| 8 | `WorkspaceEditIntentGuardTests.cs` | `TheBaseGuardAllowsTheConsumersOfTheObligation` | 799-810 | C-5: EIG:799-810, el consumidor de :805 con `KeepPilotEditsOn` (C-3) |
| 9 | `WorkspacePilotEditGuardTests.cs` | `TheHostImplementsThePortExplicitlyAndNeverReadsOutsideTheThreeReadsAndChangesOnlyThroughNotePilotEdit` | 379-386 | C-6: la regla del adaptador estático de `HostViolations` (PEG:280); la prueba exige `Empty` sobre el texto real o nombra el adaptador en sus datos |
| 10 | `WorkspacePilotEditGuardTests.cs` | `TheHostGuardTripsOnAPublicOperationAnOrderOfBasesOrAnAdapterThatIsAFieldOfInstance` | 388-399 | C-6: la regla del adaptador estático de `HostViolations` (PEG:280); la prueba exige `Empty` sobre el texto real o nombra el adaptador en sus datos |
| 11 | `WorkspacePilotEditGuardTests.cs` | `TheFlightGuardTripsWhenTheFlightIsCheckedBeforeTheGateThatAuthorizesInTheThreeReconciliations` | 495-514 | C-6: la regla del adaptador estático de `HostViolations` (PEG:280); la prueba exige `Empty` sobre el texto real o nombra el adaptador en sus datos |
| 12 | `WorkspacePilotEditGuardTests.cs` | `TheHostGuardAllowsWhatTheNextTaskWouldAddAnUpdateOperationThatWrites` | 528-537 | C-6: la regla del adaptador estático de `HostViolations` (PEG:280); la prueba exige `Empty` sobre el texto real o nombra el adaptador en sus datos |
| 13 | `WorkspacePilotEditGuardTests.cs` | `TheTrapGuardTripsOnEachNameAndEachWordThatTheNeighbouringGuardsForbid` | 1033-1054 | C-6: la regla del adaptador estático de `HostViolations` (PEG:280); la prueba exige `Empty` sobre el texto real o nombra el adaptador en sus datos |
| 14 | `WorkspacePilotReconciliationGuardTests.cs` | `TheGuardOfWhoReadsSeesAKeepInTheRequestACheckInThePresentationAnyOfTheFourInTheBrowserAndANameof` | 109-123 | C-5: RG:109-123, `Mutate` sobre `RequestKeepMine` (RG:116) |
| 15 | `PushBackFullReadGuardTests.cs` | `CT_PBT4A_06 (nueva en PB-T4a)` | - | C-5: el censo de PB-T4a admite WHC en PB-T4b (la sustituye CT-PBT4B-13) |

## PB-T4b — C-3: la UI de la elección (27)

| # | Archivo | Método | Líneas | Causa |
|---|---|---|---|---|
| 1 | `WorkspacePilotReconciliationTests.cs` | `AChoiceWhoseMemoryCannotBeReadAgainAfterTheOperationIsNeverPresentedAsDone` | 488-508 | C-3 (UI): «Conservar mi borrador» presenta los cambios antes del Si/No (D-PBT4-08 y 09) |
| 2 | `WorkspacePilotReconciliationTests.cs` | `ASurfaceThatChangesDuringTheReadOfAnUnknownDraftAndDuringAChoiceShowsTheSameNotice` | 630-646 | C-3 (UI): «Conservar mi borrador» presenta los cambios antes del Si/No (D-PBT4-08 y 09) |
| 3 | `WorkspacePilotReconciliationTests.cs` | `KeepIsNotOfferedWithAPendingTextWhetherItIsValidOrNotAndTheLineSaysWhy` | 666-682 | C-3 (UI): «Conservar mi borrador» presenta los cambios antes del Si/No (D-PBT4-08 y 09) |
| 4 | `WorkspacePilotReconciliationTests.cs` | `TheOptionsAreNotOfferedOnAPreviewTabWithAFailureWithAPersistentDiagnosticOrWithAChoiceThatWaits` | 730-749 | C-3 (UI): «Conservar mi borrador» presenta los cambios antes del Si/No (D-PBT4-08 y 09) |
| 5 | `WorkspacePilotReconciliationTests.cs` | `TheTextOfEachConfirmationIsTheOneOfTheTableForTheStateTheOwnWorkAndThePendingText` | 751-777 | C-3 (UI): «Conservar mi borrador» presenta los cambios antes del Si/No (D-PBT4-08 y 09) |
| 6 | `WorkspacePilotReconciliationTests.cs` | `AskingForAnOptionCallsNoOperationOfThePortAndOnlyReadsItsMemory` | 779-794 | C-3 (UI): «Conservar mi borrador» presenta los cambios antes del Si/No (D-PBT4-08 y 09) |
| 7 | `WorkspacePilotReconciliationTests.cs` | `AnOptionThatIsNotOfferedDoesNothingWhenItIsAsked` | 796-807 | C-3 (UI): «Conservar mi borrador» presenta los cambios antes del Si/No (D-PBT4-08 y 09) |
| 8 | `WorkspacePilotReconciliationTests.cs` | `YesOnKeepCallsThatOperationOnceWithTheSessionAndTheRackAndNeverTheDiscard` | 835-856 | C-3 (UI): «Conservar mi borrador» presenta los cambios antes del Si/No (D-PBT4-08 y 09) |
| 9 | `WorkspacePilotReconciliationTests.cs` | `NoCallsNothingChangesNothingAndSaysSoForEachOfTheTwoChoices` | 897-919 | C-3 (UI): «Conservar mi borrador» presenta los cambios antes del Si/No (D-PBT4-08 y 09) |
| 10 | `WorkspacePilotReconciliationTests.cs` | `AChoiceThatWaitsMakesTheBoxReadOnlyAndOffersOnlyYesAndNo` | 933-955 | C-3 (UI): «Conservar mi borrador» presenta los cambios antes del Si/No (D-PBT4-08 y 09) |
| 11 | `WorkspacePilotReconciliationTests.cs` | `IfTheViewChangedAfterAskingForKeepYesCallsNeitherOperation` | 998-1011 | C-3 (UI): «Conservar mi borrador» presenta los cambios antes del Si/No (D-PBT4-08 y 09) |
| 12 | `WorkspacePilotReconciliationTests.cs` | `TheChoiceIsWithdrawnInSilenceByAChangeOfSurfaceByClearAndByAFailure` | 1013-1038 | C-3 (UI): «Conservar mi borrador» presenta los cambios antes del Si/No (D-PBT4-08 y 09) |
| 13 | `WorkspacePilotReconciliationTests.cs` | `TheChoiceIsWithdrawnByAnyNewGestureOfTheField` | 1040-1056 | C-3 (UI): «Conservar mi borrador» presenta los cambios antes del Si/No (D-PBT4-08 y 09) |
| 14 | `WorkspacePilotReconciliationTests.cs` | `TheChoiceIsWithdrawnWithTheSituationNoticeWhenThePresentationFindsThatTheViewNoLongerOffersIt` | 1058-1082 | C-3 (UI): «Conservar mi borrador» presenta los cambios antes del Si/No (D-PBT4-08 y 09) |
| 15 | `WorkspacePilotReconciliationTests.cs` | `ADiscardFromTheBrowserOfTheDraftThatTheChoiceWasAboutWithdrawsTheChoiceToo` | 1084-1106 | C-3 (UI): «Conservar mi borrador» presenta los cambios antes del Si/No (D-PBT4-08 y 09) |
| 16 | `WorkspacePilotReconciliationTests.cs` | `AnAnswerThatDoesNotMatchTheChoiceIsNeverPresentedAsDone` | 1110-1124 | C-3 (UI): «Conservar mi borrador» presenta los cambios antes del Si/No (D-PBT4-08 y 09) |
| 17 | `WorkspacePilotReconciliationTests.cs` | `AnAnswerOfTheChoiceThatDidNothingHasItsPrefixItsReasonAndTheTextOfNothingDone` | 1140-1159 | C-3 (UI): «Conservar mi borrador» presenta los cambios antes del Si/No (D-PBT4-08 y 09) |
| 18 | `WorkspacePilotReconciliationTests.cs` | `AnExceptionOfTheChosenOperationIsTheErrorOfTheFieldAndTheChoiceIsGone` | 1161-1174 | C-3 (UI): «Conservar mi borrador» presenta los cambios antes del Si/No (D-PBT4-08 y 09) |
| 19 | `WorkspacePilotReconciliationTests.cs` | `ARefusalOfTheChoiceForANotAdmittedReadIsAPersistentDiagnosticAndTheDraftIsKeptAsItWas` | 1176-1201 | C-3 (UI): «Conservar mi borrador» presenta los cambios antes del Si/No (D-PBT4-08 y 09) |
| 20 | `WorkspacePilotReconciliationTests.cs` | `APreviewTabOffersNeitherTheCheckNorTheOptionsAndCallsNothing` | 1415-1426 | C-3 (UI): «Conservar mi borrador» presenta los cambios antes del Si/No (D-PBT4-08 y 09) |
| 21 | `WorkspacePilotReconciliationTests.cs` | `AFieldWithoutThePortOffersNothingAndDoesNothing` | 1428-1445 | C-3 (UI): «Conservar mi borrador» presenta los cambios antes del Si/No (D-PBT4-08 y 09) |
| 22 | `WorkspacePilotReconciliationViewTests.cs` | `TheFiveButtonsHaveTheirContentTheirAutomationNameAndTheOrderOfTheTable` | 79-104 | C-3 (UI): «Conservar mi borrador» presenta los cambios antes del Si/No (D-PBT4-08 y 09) |
| 23 | `WorkspacePilotReconciliationViewTests.cs` | `TheButtonsAreVisibleOnlyWhereTheViewModelOffersThem` | 106-131 | C-3 (UI): «Conservar mi borrador» presenta los cambios antes del Si/No (D-PBT4-08 y 09) |
| 24 | `WorkspacePilotReconciliationViewTests.cs` | `ARealClickOnAnOptionOnlyAsksForTheConfirmationAndShowsTheQuestionAndTheTwoButtons` | 174-196 | C-3 (UI): «Conservar mi borrador» presenta los cambios antes del Si/No (D-PBT4-08 y 09) |
| 25 | `WorkspacePilotReconciliationViewTests.cs` | `ARealClickOnYesCallsTheChosenOperationOnceAndNeverTheOtherForEachOption` | 198-236 | C-3 (UI): «Conservar mi borrador» presenta los cambios antes del Si/No (D-PBT4-08 y 09) |
| 26 | `WorkspacePilotReconciliationViewTests.cs` | `ARealClickOnNoCallsNothingAndLeavesTheDraftAsItWasForEachOption` | 238-261 | C-3 (UI): «Conservar mi borrador» presenta los cambios antes del Si/No (D-PBT4-08 y 09) |
| 27 | `WorkspacePilotReconciliationViewTests.cs` | `YesAndNoAreNotSwappedAClickOnEachDoesWhatItsLabelSays` | 263-279 | C-3 (UI): «Conservar mi borrador» presenta los cambios antes del Si/No (D-PBT4-08 y 09) |

## PB-T4b — C-7: «Retirarlo» (139)

| # | Archivo | Método | Líneas | Causa |
|---|---|---|---|---|
| 1 | `PushBackFullWritePrepareTests.cs` | `CT_PBT3B_05_TheNameOfTheDraftReachesEveryViewAndABlankNameTakesTheOneOfTheOrigin` | 474-544 | C-7: nombra `PushBackPilotUpdatePlanner.Prepare` |
| 2 | `PushBackFullWritePrepareTests.cs` | `CT_PBT3B_06_TheBarrierOfTheDiagnosticsStopsABlockedDesignOfTheFullModelAndOfThePilot` | 558-629 | C-7: nombra `ComposeForWrite`, `PushBackPilotUpdatePlanner.Prepare` |
| 3 | `PushBackFullWritePrepareTests.cs` | `CT_PBT3B_15_ThePrecheckOfTheFullModelAndTheOrderOfPrepareFullAreTheOnesOfThePilot` | 781-829 | C-7: nombra `PushBackPilotUpdateRun.Precheck` |
| 4 | `WorkspaceDraftPilotIntentStaleBaseTests.cs` | `ARereadThatIsNotAdmittedBecauseAMemberBecameBlockingTakesTheUnknownDraftToConflictWithTheDifferences` | 551-568 | C-7: nombra `PushBackPilotStateComparer` |
| 5 | `WorkspaceDraftPilotIntentStaleBaseTests.cs` | `ARereadThatIsNotAdmittedBecauseAuth13FindsDivergenceTakesTheUnknownDraftToConflictWithTheDifferences` | 570-586 | C-7: nombra `PushBackPilotStateComparer` |
| 6 | `WorkspaceDraftPilotIntentStaleBaseTests.cs` | `ARereadThatIsEqualRecoversTheUnknownDraft` | 588-602 | C-7: nombra `PushBackPilotStateComparer` |
| 7 | `WorkspaceDraftPilotIntentStaleBaseTests.cs` | `ARereadThatIsNotAdmittedOnlyForTheCatalogStillRecoversBecauseTheBaseIsTheSame` | 604-615 | C-7: nombra `PushBackPilotStateComparer` |
| 8 | `WorkspaceDraftPilotMaterializationTests.cs` | `AnExceptionInsideTheBaseOfComposeForWriteIsARejectionWithTheExceptionPrefix` | 257-272 | C-7: nombra `ComposeForWrite` |
| 9 | `WorkspaceDraftPilotMaterializationTests.cs` | `AnExceptionInsideComposeForWriteItselfIsARejectionWithTheComposePrefix` | 274-288 | C-7: nombra `ComposeForWrite` |
| 10 | `WorkspaceDraftPilotMaterializationTests.cs` | `ComposeForWriteIsPureAndMutatesNeitherState` | 382-407 | C-7: nombra `ComposeForWrite`, `PushBackPilotStateComparer` |
| 11 | `WorkspaceDraftPilotMaterializationTests.cs` | `ComposeForWriteIgnoresTheOriginOfTheCurrentState` | 409-421 | C-7: nombra `ComposeForWrite` |
| 12 | `WorkspaceDraftPilotMaterializationTests.cs` | `ComposeForWriteIsTotalAndRejectsABaseThatNoLongerMaterializes` | 423-444 | C-7: nombra `ComposeForWrite` |
| 13 | `WorkspaceDraftPilotMaterializationTests.cs` | `ComposeForWriteTakesWhatIsEditedFromTheDraftAndWhatIsNotFromTheCurrentBase` | 446-459 | C-7: nombra `ComposeForWrite` |
| 14 | `WorkspaceDraftPilotMaterializationTests.cs` | `ACleanDraftComposesExactlyItsBase` | 461-475 | C-7: nombra `ComposeForWrite`, `PushBackPilotStateComparer` |
| 15 | `WorkspaceDraftPilotMaterializationTests.cs` | `ComposeForWriteRejectsAnyDifferenceOtherThanTheScaleAndNeverRevertsOrWritesItInSilence` | 482-518 | C-7: nombra `ComposeForWrite` |
| 16 | `WorkspaceDraftPilotMaterializationTests.cs` | `AnInPlaceEditOfTheCurrentEditorOutsideThePilotMakesComposeForWriteReject` | 520-532 | C-7: nombra `ComposeForWrite`, `PushBackPilotStateComparer` |
| 17 | `WorkspaceDraftPilotMaterializationTests.cs` | `KeepPilotEditsOnIsPureAndGivesTheNewBaseWithTheScaleOfTheDraft` | 534-565 | C-7: nombra `KeepPilotEditsOn`, `PushBackPilotStateComparer` |
| 18 | `WorkspaceDraftPilotMaterializationTests.cs` | `KeepMineAfterAnExternalModuleChangeWritesTheNewModulesAndTheScaleOfTheDraftAndReturnsToCleanWhenTheScaleIsRestored` | 567-605 | C-7: nombra `ComposeForWrite`, `KeepPilotEditsOn`, `PushBackPilotStateComparer` |
| 19 | `WorkspaceDraftPilotMaterializationTests.cs` | `KeepMineAfterAnExternalChangeOfContentThePilotDoesNotEditNeverRevertsItAndComposesTheNewContentPlusTheScaleOfTheDraft` | 607-650 | C-7: nombra `ComposeForWrite`, `KeepPilotEditsOn`, `PushBackPilotStateComparer` |
| 20 | `WorkspaceDraftPilotMaterializationTests.cs` | `ACanonicalDesignComesBackAsTheIdenticalPersistedDocument` | 680-687 | C-7: nombra `PushBackPilotStateComparer` |
| 21 | `WorkspaceDraftPilotMaterializationTests.cs` | `EditingTheScaleOfACanonicalDesignChangesOnlyThatField` | 689-701 | C-7: nombra `PushBackPilotStateComparer` |
| 22 | `WorkspaceDraftPilotMaterializationTests.cs` | `AnOlderDrawingComesBackWithExactlyTheCanonicalizationOfTheApplicationPath` | 740-749 | C-7: nombra `PushBackPilotStateComparer` |
| 23 | `WorkspaceDraftPilotMaterializationTests.cs` | `EditingTheScaleOfAnOlderDrawingAddsOnlyTheEditedFieldToTheCanonicalization` | 751-762 | C-7: nombra `PushBackPilotStateComparer` |
| 24 | `WorkspaceDraftPilotMaterializationTests.cs` | `CarryOver1_TheRejectionComparesTheCurrentStateAsItIsAgainstACopyOfTheInitialOneWithItsScaleNeutralizedAndMutatesNeitherState` | 766-811 | C-7: nombra `ComposeForWrite`, `PushBackPilotStateComparer` |
| 25 | `WorkspaceDraftPilotMaterializationTests.cs` | `CarryOver3_ABaseThatMaterializedAgainIsNotTheInitialStateIsRejectedBeforeComposingAndANormalBaseStillComposes` | 813-840 | C-7: nombra `ComposeForWrite`, `KeepPilotEditsOn`, `PushBackPilotStateComparer` |
| 26 | `WorkspaceDraftPilotMaterializationTests.cs` | `CarryOver6_AScaleThatIsNotAFiniteNumberGreaterThanZeroIsRejectedAndNeverComposedAndTheInfiniteOneIsADeclaredExtension` | 842-869 | C-7: nombra `ComposeForWrite` |
| 27 | `WorkspaceDraftPilotMaterializationTests.cs` | `CarryOver2_KeepAfterAnExternalModuleChangeIsOneAtomicStepThatComposesTheNewModulesAndTheScaleOfTheDraft` | 871-899 | C-7: nombra `ComposeForWrite`, `KeepPilotEditsOn`, `PushBackPilotStateComparer` |
| 28 | `WorkspaceDraftPilotMaterializationTests.cs` | `CarryOver2_KeepAfterAnExternalChangeOfContentThePilotDoesNotEditNeverLeavesTheStepThatWouldRevertItAndComposesAtOnce` | 901-925 | C-7: nombra `ComposeForWrite`, `KeepPilotEditsOn`, `PushBackPilotStateComparer` |
| 29 | `WorkspaceDraftPilotMaterializationTests.cs` | `CarryOver2_AKeepOnThatFailsLeavesTheDraftWithoutTheIntermediateStateAndThePendingTextStays` | 927-948 | C-7: nombra `PushBackPilotStateComparer` |
| 30 | `WorkspaceDraftPilotOwnershipTests.cs` | `ThePilotStateKeepsTheInPlaceEditModelOfTheCurrentState` | 298-312 | C-7: nombra `PushBackPilotStateComparer` |
| 31 | `WorkspaceDraftPilotStateTests.cs` | `TheScaleValidationIsTheRuleOfTheClassicBoxInEveryCulture` | 718-733 | C-7: nombra `PushBackPilotFieldAdapter` |
| 32 | `WorkspaceDraftPilotStateTests.cs` | `AnyKeyThatIsNotExactlyAnnotationScaleIsInvalidByOrdinalComparison` | 735-749 | C-7: nombra `PushBackPilotFieldAdapter` |
| 33 | `WorkspaceDraftPilotStateTests.cs` | `TheKeyOfTheOneFieldIsExactlyAnnotationScale` | 751-756 | C-7: nombra `PushBackPilotFieldAdapter` |
| 34 | `WorkspaceDraftPilotStateTests.cs` | `ValidateMutatesNothing` | 760-773 | C-7: nombra `PushBackPilotFieldAdapter` |
| 35 | `WorkspaceDraftPilotStateTests.cs` | `ApplyReturnsTheSameObjectItReceivedWithTheScaleAppliedInPlace` | 775-791 | C-7: nombra `PushBackPilotFieldAdapter` |
| 36 | `WorkspaceDraftPilotStateTests.cs` | `ApplyParsesTheTextInTheCurrentCulture` | 793-805 | C-7: nombra `PushBackPilotFieldAdapter` |
| 37 | `WorkspaceDraftPilotStateTests.cs` | `ApplyLeavesTheStateAloneForAnyPendingTextThatIsNotTheOneValidField` | 807-819 | C-7: nombra `PushBackPilotFieldAdapter` |
| 38 | `WorkspaceDraftPilotStateTests.cs` | `TheAdapterKeepsNoReferenceToAnyState` | 821-836 | C-7: nombra `PushBackPilotFieldAdapter` |
| 39 | `WorkspaceDraftPilotStateTests.cs` | `TheAdapterRunsInsideTheTwoPhaseCommitOfTheDraftCore` | 846-870 | C-7: nombra `PushBackPilotFieldAdapter`, `PushBackPilotStateComparer` |
| 40 | `WorkspaceDraftPilotStateTests.cs` | `TheCopyIsEqualForTheComparatorAndSharesTheOriginString` | 1039-1053 | C-7: nombra `PushBackPilotStateComparer` |
| 41 | `WorkspaceDraftPilotStateTests.cs` | `TheCopyOfTheRichStateKeepsEveryEditableValueAndTheSameSelection` | 1055-1075 | C-7: nombra `PushBackPilotStateComparer` |
| 42 | `WorkspaceDraftPilotStateTests.cs` | `TheCopySharesNoMutableObjectWithTheOriginal` | 1077-1092 | C-7: nombra `PushBackPilotStateComparer` |
| 43 | `WorkspaceDraftPilotStateTests.cs` | `MutatingEveryFieldOfTheCopyNeverChangesTheOriginal` | 1094-1109 | C-7: nombra `PushBackPilotStateComparer` |
| 44 | `WorkspaceDraftPilotStateTests.cs` | `TheCopyDoesNotCarryTheModuleAndTailAuthority` | 1111-1138 | C-7: nombra `PushBackPilotStateComparer` |
| 45 | `WorkspaceDraftPilotStateTests.cs` | `ACopyOfAStateMaterializedFromAnOriginIsStillEqualAndNeverHasItsBaseline` | 1140-1150 | C-7: nombra `PushBackPilotStateComparer` |
| 46 | `WorkspacePilotEditGuardTests.cs` | `TheHostGuardTripsOnAReadAnAuthorizationOrAMarkWhereTheOperationMustNotHaveThem` | 401-416 | C-7: nombra `PushBackPilotFieldAdapter` |
| 47 | `WorkspacePilotEditGuardTests.cs` | `TheHostGuardTripsWhenAnOperationThatChangesADraftLeavesOutNotePilotEdit` | 418-431 | C-7: nombra `PushBackPilotFieldAdapter` |
| 48 | `WorkspacePilotEditGuardTests.cs` | `TheUpdateRequestGuardTripsWithoutTheGateWithASecondRequestWithTheUpdateItselfOrWhereTheProgressAuthorizesOrMutates` | 434-451 | C-7: nombra `PushBackPilotFieldAdapter` |
| 49 | `WorkspacePilotEditGuardTests.cs` | `TheBoundGuardTripsWhenTheLazyExpiryReachesTheOwnMutationTheProbeTheGateTheReaderOrAnotherMemberOfTheBridge` | 453-468 | C-7: nombra `PushBackPilotFieldAdapter` |
| 50 | `WorkspacePilotEditGuardTests.cs` | `TheFlightGuardTripsWhenTheCheckOfTheFlightIsMissingOrComesBeforeTheAuthorizationAndTheMarkOfTheReads` | 470-492 | C-7: nombra `PushBackPilotFieldAdapter` |
| 51 | `WorkspacePilotEditGuardTests.cs` | `TheFlightGuardTripsOnABodyThatChecksTheFlightWithNoAuthorizationAtAll` | 516-526 | C-7: nombra `PushBackPilotFieldAdapter` |
| 52 | `WorkspacePilotPrepareGuardTests.cs` | `ComposeForWriteIsCalledOnlyByThePlannerAndOnlyAfterTheComparisonOfTheBase` | 172-176 | C-7: nombra `ComposeForWrite` |
| 53 | `WorkspacePilotPrepareGuardTests.cs` | `TheAfterMatchGuardTripsOnAnotherCallerAndOnACallBeforeTheComparison` | 178-190 | C-7: nombra `ComposeForWrite` |
| 54 | `WorkspacePilotUpdateGuardTests.cs` | `TheBodyOfTheUpdateFollowsTheOrderOfThePackageAndEndsTheMarkOfKnownCleanAfterTheClose` | 822-840 | C-7: nombra `PushBackPilotUpdateRun.Execute`, `PushBackPilotUpdateRun.Precheck` |
| 55 | `WorkspacePilotUpdateGuardTests.cs` | `TheCompositionForWritingComparesTheCurrentStateAsItIsAndNeverCopiesIt` | 943-947 | C-7: nombra `ComposeForWrite`, `PushBackPilotStateComparer` |
| 56 | `WorkspacePilotUpdateGuardTests.cs` | `TheGuardOfTheCompositionTripsWhenTheCurrentStateIsCopiedBeforeTheComparison` | 949-960 | C-7: nombra `ComposeForWrite`, `PushBackPilotStateComparer` |
| 57 | `WorkspacePilotUpdateIntegrationGuardTests.cs` | `G4TripsOnACallerInTheControllerTheAdapterOrASecondCallInTheCore` | 510-517 | C-7: nombra `PushBackPilotUpdateRun.Execute` |
| 58 | `WorkspacePilotUpdateRunTests.cs` | `ThePrecheckOnlyStopsForTheThreeKindsThatNeedNeitherAReadNorACatalog` | 30-61 | C-7: nombra `PushBackPilotUpdateRun.Precheck` |
| 59 | `WorkspacePilotUpdateRunTests.cs` | `AReadThatIsNotAdmittedWithTheBaseOfTheDraftIsNotAppliedAdmission` | 174-190 | C-7: nombra `PushBackPilotUpdateRun.Execute` |
| 60 | `WorkspacePilotUpdateRunTests.cs` | `AViewThatIsNotAvailableInTheOutgoingSystemIsAPreflightAndNothingIsImported` | 192-207 | C-7: nombra `PushBackPilotUpdateRun.Execute` |
| 61 | `WorkspacePilotUpdateRunTests.cs` | `ACleanDraftIsNothingToApplyAndAConflictDraftIsNotApplicableAndAPendingTextIsPendingTextEvenWithoutTheSlot` | 209-230 | C-7: nombra `PushBackPilotUpdateRun.Execute` |
| 62 | `WorkspacePilotUpdateRunTests.cs` | `TheRunNeverThrowsForMissingArgumentsOrForAPortThatAnswersNothing` | 664-683 | C-7: nombra `PushBackPilotUpdateRun.Execute` |
| 63 | `WorkspacePilotUpdateRunTests.cs` | `TheRunKeepsNoTextOfTheDrawingBetweenTwoRuns` | 839-861 | C-7: nombra `PushBackPilotUpdateRun.Execute` |
| 64 | `PushBackFieldValidationConcordanceTests.cs` | `CT_PBT2B_10_ThePanelScaleFieldAndTheWindowScaleBoxAcceptAndRejectTheSameTexts` | 29-60 | C-7: nombra `PushBackPilotFieldAdapter` |
| 65 | `WorkspaceDraftPilotCanonicalLineEndingTests.cs` | `EachOfTheEightCanonicalDesignsIsAFileOfTheFixturesFolderAndItsRoundTripIsByteForByte` | 65-78 | C-7: nombra `ComposeForWrite` |
| 66 | `WorkspaceDraftPilotCanonicalLineEndingTests.cs` | `ThePersistedDocumentOfThePilotWithoutAnEditIsTheInnerDesignOfTheEnvelopeByteForByteWithoutNormalizingTheLineEndings` | 80-83 | C-7: nombra `ComposeForWrite` |
| 67 | `WorkspaceDraftPilotValidationParityTests.cs` | `TheAdapterGivesTheSameVerdictAndMessageAsTheClassicBoxInEveryCulture` | 45-59 | C-7: nombra `PushBackPilotFieldAdapter` |
| 68 | `WorkspaceDraftPilotValidationParityTests.cs` | `AValidTextAppliesTheSameNumberTheClassicBoxParses` | 61-79 | C-7: nombra `PushBackPilotFieldAdapter` |
| 69 | `WorkspaceDraftPilotWindowLoadCharacterizationTests.cs` | `TheDocumentThePilotWritesAfterKeepMineOpensInTheClassicWindowWithTheNewBaseAndTheScaleOfTheDraft` | 270-313 | C-7: nombra `ComposeForWrite`, `KeepPilotEditsOn`, `PushBackPilotStateComparer` |
| 70 | `PushBackFullWritePrepareTests.cs` | `CT_PBT3B_01_PrepareFullWritesTheFieldsTheNameAndTheDesignOfTheDraftOverTheEnvelopeThatWasReread` | 242-275 | C-7 por compilación: alcanza, por un ayudante de su archivo o de apoyo, un miembro retirado (`retire_compile_pbt4.py`) |
| 71 | `PushBackFullWritePrepareTests.cs` | `CT_PBT3B_02_TheFrontsRedrawEveryViewAndTheObsoleteLateralIsErasedWithTheOriginReanchored` | 294-316 | C-7 por compilación: alcanza, por un ayudante de su archivo o de apoyo, un miembro retirado (`retire_compile_pbt4.py`) |
| 72 | `PushBackFullWritePrepareTests.cs` | `CT_PBT3B_03_AModuleLengthOfEachSideReachesTheOutgoingDesignAndNoViewIsErased` | 320-359 | C-7 por compilación: alcanza, por un ayudante de su archivo o de apoyo, un miembro retirado (`retire_compile_pbt4.py`) |
| 73 | `PushBackFullWritePrepareTests.cs` | `CT_PBT3B_04_TheSideBIsComposedRejectedAsAFrontalOfAOneWayRackOrRemovedWithItsLaterals` | 383-459 | C-7 por compilación: alcanza, por un ayudante de su archivo o de apoyo, un miembro retirado (`retire_compile_pbt4.py`) |
| 74 | `PushBackFullWritePrepareTests.cs` | `CT_PBT3B_10_Auth13RejectsTheWriteOfADesignWithANestedUnknownFieldWithoutAnyCallOfWriting` | 660-679 | C-7 por compilación: alcanza, por un ayudante de su archivo o de apoyo, un miembro retirado (`retire_compile_pbt4.py`) |
| 75 | `PushBackFullWritePrepareTests.cs` | `CT_PBT3B_11_TheEnvelopeThatWasRereadIsCarriedOverAndTheDraftKeepsItsDesignAndItsName` | 683-774 | C-7 por compilación: alcanza, por un ayudante de su archivo o de apoyo, un miembro retirado (`retire_compile_pbt4.py`) |
| 76 | `PushBackFullWriteRunTests.cs` | `CT_PBT3B_09_ComparePostFullChecksTheErasedKeysTheKeysTheEnvelopesAndTheScaleAndThePilotKeepsItsComparison` | 245-304 | C-7 por compilación: alcanza, por un ayudante de su archivo o de apoyo, un miembro retirado (`retire_compile_pbt4.py`) |
| 77 | `WorkspaceDraftPilotMaterializationTests.cs` | `MaterializeReproducesTheApplicationPartOfTheWindowLoadInTheSameOrder` | 317-332 | C-7 por compilación: alcanza, por un ayudante de su archivo o de apoyo, un miembro retirado (`retire_compile_pbt4.py`) |
| 78 | `WorkspaceDraftPilotOwnershipTests.cs` | `EditingTheCurrentStateInPlaceLeavesTheDraftDirtyAndTheCloseVetoed` | 283-286 | C-7 por compilación: alcanza, por un ayudante de su archivo o de apoyo, un miembro retirado (`retire_compile_pbt4.py`) |
| 79 | `WorkspaceDraftPilotOwnershipTests.cs` | `MutatingTheObjectOfThePostReadAfterTheRebaseOfAnAppliedUpdateChangesNothing` | 258-264 | C-7 por compilación: alcanza, por un ayudante de su archivo o de apoyo, un miembro retirado (`retire_compile_pbt4.py`) |
| 80 | `WorkspaceDraftPilotOwnershipTests.cs` | `MutatingTheObjectOfTheReadAfterBeginChangesNeitherTheInitialStateNorDirty` | 243-246 | C-7 por compilación: alcanza, por un ayudante de su archivo o de apoyo, un miembro retirado (`retire_compile_pbt4.py`) |
| 81 | `WorkspaceDraftPilotOwnershipTests.cs` | `MutatingTheObjectOfTheReadAfterDiscardAndReloadChangesNothing` | 248-251 | C-7 por compilación: alcanza, por un ayudante de su archivo o de apoyo, un miembro retirado (`retire_compile_pbt4.py`) |
| 82 | `WorkspaceDraftPilotOwnershipTests.cs` | `MutatingTheObjectOfTheReadAfterKeepMineChangesNothing` | 253-256 | C-7 por compilación: alcanza, por un ayudante de su archivo o de apoyo, un miembro retirado (`retire_compile_pbt4.py`) |
| 83 | `WorkspaceDraftPilotOwnershipTests.cs` | `MutatingTheObjectPassedToSetCurrentDraftStateAfterTheCallDoesNotChangeTheCurrentState` | 266-269 | C-7 por compilación: alcanza, por un ayudante de su archivo o de apoyo, un miembro retirado (`retire_compile_pbt4.py`) |
| 84 | `WorkspaceDraftPilotOwnershipTests.cs` | `MutatingTheObjectReturnedByInitialDraftStateDoesNotChangeTheInternalInitialState` | 271-274 | C-7 por compilación: alcanza, por un ayudante de su archivo o de apoyo, un miembro retirado (`retire_compile_pbt4.py`) |
| 85 | `WorkspaceDraftPilotOwnershipTests.cs` | `ReusingOneObjectOfOriginForTwoDraftsOrTwoSessionsLeavesThemIndependent` | 276-279 | C-7 por compilación: alcanza, por un ayudante de su archivo o de apoyo, un miembro retirado (`retire_compile_pbt4.py`) |
| 86 | `WorkspaceDraftPilotStateTests.cs` | `AnEditedScaleMakesTheStatesDifferent` | 928-937 | C-7 por compilación: alcanza, por un ayudante de su archivo o de apoyo, un miembro retirado (`retire_compile_pbt4.py`) |
| 87 | `WorkspaceDraftPilotStateTests.cs` | `EveryEditableFieldIsExercisedByTheRichStateAndMutatingItMakesTheStatesDifferent` | 985-1010 | C-7 por compilación: alcanza, por un ayudante de su archivo o de apoyo, un miembro retirado (`retire_compile_pbt4.py`) |
| 88 | `WorkspaceDraftPilotStateTests.cs` | `MutatingEverySelectionFieldTheModuleAuthorityAndTheOriginLeavesTheStatesEqual` | 1014-1035 | C-7 por compilación: alcanza, por un ayudante de su archivo o de apoyo, un miembro retirado (`retire_compile_pbt4.py`) |
| 89 | `WorkspaceDraftPilotStateTests.cs` | `OriginsWithDifferentTextAndEquivalentContentAreEqual` | 907-926 | C-7 por compilación: alcanza, por un ayudante de su archivo o de apoyo, un miembro retirado (`retire_compile_pbt4.py`) |
| 90 | `WorkspaceDraftPilotStateTests.cs` | `TheComparatorIsReflexiveAndSymmetricAndTwoMaterializationsOfTheSameOriginAreEqual` | 874-886 | C-7 por compilación: alcanza, por un ayudante de su archivo o de apoyo, un miembro retirado (`retire_compile_pbt4.py`) |
| 91 | `WorkspaceDraftPilotStateTests.cs` | `TheComparatorIsSymmetricAndSeparatesDifferentDesigns` | 891-905 | C-7 por compilación: alcanza, por un ayudante de su archivo o de apoyo, un miembro retirado (`retire_compile_pbt4.py`) |
| 92 | `WorkspaceDraftPilotStateTests.cs` | `TheComparatorTreatsNullAndNaNWithoutBreakingReflexivity` | 939-952 | C-7 por compilación: alcanza, por un ayudante de su archivo o de apoyo, un miembro retirado (`retire_compile_pbt4.py`) |
| 93 | `WorkspacePilotUpdateRunTests.cs` | `ABlockThatIsNotTheOneThatWasPreparedIsAPreflightAndNothingIsWritten` | 338-350 | C-7 por compilación: alcanza, por un ayudante de su archivo o de apoyo, un miembro retirado (`retire_compile_pbt4.py`) |
| 94 | `WorkspacePilotUpdateRunTests.cs` | `AComparisonThatThrowsAfterTheSealIsAppliedPostRereadFailed` | 512-527 | C-7 por compilación: alcanza, por un ayudante de su archivo o de apoyo, un miembro retirado (`retire_compile_pbt4.py`) |
| 95 | `WorkspacePilotUpdateRunTests.cs` | `AConflictOrAPreflightOfMutateStaysWhatItIsEvenWhenTheReleaseOfTheScopeOfMutateThrows` | 610-628 | C-7 por compilación: alcanza, por un ayudante de su archivo o de apoyo, un miembro retirado (`retire_compile_pbt4.py`) |
| 96 | `WorkspacePilotUpdateRunTests.cs` | `ADifferenceFoundByTheRereadOfMutateIsAConflictAndNoUnitIsWritten` | 295-310 | C-7 por compilación: alcanza, por un ayudante de su archivo o de apoyo, un miembro retirado (`retire_compile_pbt4.py`) |
| 97 | `WorkspacePilotUpdateRunTests.cs` | `ADifferenceOfTheBaseInPrepareIsAConflictWithItsDifferencesAndNothingIsImported` | 159-172 | C-7 por compilación: alcanza, por un ayudante de su archivo o de apoyo, un miembro retirado (`retire_compile_pbt4.py`) |
| 98 | `WorkspacePilotUpdateRunTests.cs` | `ADocumentThatChangedBetweenPrepareAndMutateIsAPreflightWithoutWrites` | 325-336 | C-7 por compilación: alcanza, por un ayudante de su archivo o de apoyo, un miembro retirado (`retire_compile_pbt4.py`) |
| 99 | `WorkspacePilotUpdateRunTests.cs` | `ADocumentThatIsNotTheAuthorizedOneStopsTheRunBeforeAnythingIsImported` | 116-127 | C-7 por compilación: alcanza, por un ayudante de su archivo o de apoyo, un miembro retirado (`retire_compile_pbt4.py`) |
| 100 | `WorkspacePilotUpdateRunTests.cs` | `AForeignScopeOpenAtPostIsAppliedPostRereadFailedWithItsCauseAndTheWarnings` | 493-510 | C-7 por compilación: alcanza, por un ayudante de su archivo o de apoyo, un miembro retirado (`retire_compile_pbt4.py`) |
| 101 | `WorkspacePilotUpdateRunTests.cs` | `AForeignScopeOpenIsRejectedAsAValueAndNothingIsRereadWrittenOrReleased` | 280-291 | C-7 por compilación: alcanza, por un ayudante de su archivo o de apoyo, un miembro retirado (`retire_compile_pbt4.py`) |
| 102 | `WorkspacePilotUpdateRunTests.cs` | `ALockedLayerOfADefinitionThatIsNotInThePlanIsIgnored` | 258-265 | C-7 por compilación: alcanza, por un ayudante de su archivo o de apoyo, un miembro retirado (`retire_compile_pbt4.py`) |
| 103 | `WorkspacePilotUpdateRunTests.cs` | `ALockedLayerOfAMemberOfThePlanStopsTheRunBeforeTheImportAndLeavesNoResidue` | 245-256 | C-7 por compilación: alcanza, por un ayudante de su archivo o de apoyo, un miembro retirado (`retire_compile_pbt4.py`) |
| 104 | `WorkspacePilotUpdateRunTests.cs` | `ANameThatDiffersAfterTheRenameIsAWarningAndACaseOnlyDifferenceIsNot` | 720-733 | C-7 por compilación: alcanza, por un ayudante de su archivo o de apoyo, un miembro retirado (`retire_compile_pbt4.py`) |
| 105 | `WorkspacePilotUpdateRunTests.cs` | `APostThatThrowsIsAppliedPostRereadFailedWithTheWarningsThatWereKnown` | 464-477 | C-7 por compilación: alcanza, por un ayudante de su archivo o de apoyo, un miembro retirado (`retire_compile_pbt4.py`) |
| 106 | `WorkspacePilotUpdateRunTests.cs` | `APostWithoutValuesOrWithoutItsRereadIsAppliedPostRereadFailed` | 479-491 | C-7 por compilación: alcanza, por un ayudante de su archivo o de apoyo, un miembro retirado (`retire_compile_pbt4.py`) |
| 107 | `WorkspacePilotUpdateRunTests.cs` | `AReleaseThatFailsWithoutSealAndWithoutAUnitStartedIsNotAppliedFailureAndNotAnUncertainOutcome` | 419-432 | C-7 por compilación: alcanza, por un ayudante de su archivo o de apoyo, un miembro retirado (`retire_compile_pbt4.py`) |
| 108 | `WorkspacePilotUpdateRunTests.cs` | `ASealThatReturnsAndAReleaseThatThrowsGoesToPostAndTheReleaseIsAWarning` | 450-462 | C-7 por compilación: alcanza, por un ayudante de su archivo o de apoyo, un miembro retirado (`retire_compile_pbt4.py`) |
| 109 | `WorkspacePilotUpdateRunTests.cs` | `ASealThatThrewKeepsBeingCommitThrewBecauseOfTheSealEvenIfTheAbortThrowsToo` | 630-642 | C-7 por compilación: alcanza, por un ayudante de su archivo o de apoyo, un miembro retirado (`retire_compile_pbt4.py`) |
| 110 | `WorkspacePilotUpdateRunTests.cs` | `ASealThatThrowsIsCommitThrewAndPostIsNeverReached` | 434-448 | C-7 por compilación: alcanza, por un ayudante de su archivo o de apoyo, un miembro retirado (`retire_compile_pbt4.py`) |
| 111 | `WorkspacePilotUpdateRunTests.cs` | `AViewWithABlankNameHasNoDesiredNameAndGivesNoWarningEvenIfItsDefinitionHasAnotherName` | 708-718 | C-7 por compilación: alcanza, por un ayudante de su archivo o de apoyo, un miembro retirado (`retire_compile_pbt4.py`) |
| 112 | `WorkspacePilotUpdateRunTests.cs` | `AbortFailedOnlyAppliesToANotAppliedFailureWithAUnitStarted` | 644-660 | C-7 por compilación: alcanza, por un ayudante de su archivo o de apoyo, un miembro retirado (`retire_compile_pbt4.py`) |
| 113 | `WorkspacePilotUpdateRunTests.cs` | `AbortThatFailsAfterAUnitStartedIsCommitThrewWithAbortFailedAndNoRollbackIsClaimed` | 398-417 | C-7 por compilación: alcanza, por un ayudante de su archivo o de apoyo, un miembro retirado (`retire_compile_pbt4.py`) |
| 114 | `WorkspacePilotUpdateRunTests.cs` | `AnExceptionBeforeTheSealIsNotAppliedFailureWithItsCauseAndTheSealIsNeverCalled` | 374-396 | C-7 por compilación: alcanza, por un ayudante de su archivo o de apoyo, un miembro retirado (`retire_compile_pbt4.py`) |
| 115 | `WorkspacePilotUpdateRunTests.cs` | `AnExtraMemberAppearingBetweenPrepareAndMutateIsAConflictToo` | 312-323 | C-7 por compilación: alcanza, por un ayudante de su archivo o de apoyo, un miembro retirado (`retire_compile_pbt4.py`) |
| 116 | `WorkspacePilotUpdateRunTests.cs` | `AnImportThatFailsIsAPreflightWithItsCauseAndNoScopeOfMutationIsStarted` | 267-278 | C-7 por compilación: alcanza, por un ayudante de su archivo o de apoyo, un miembro retirado (`retire_compile_pbt4.py`) |
| 117 | `WorkspacePilotUpdateRunTests.cs` | `AnOuterReleaseThatThrowsAfterPostTurnsAppliedIntoAppliedWithWarningsAndKeepsTheOtherWarnings` | 531-550 | C-7 por compilación: alcanza, por un ayudante de su archivo o de apoyo, un miembro retirado (`retire_compile_pbt4.py`) |
| 118 | `WorkspacePilotUpdateRunTests.cs` | `AnOuterReleaseThatThrowsOnAConflictIsDroppedBecauseAConflictCarriesNoCause` | 594-606 | C-7 por compilación: alcanza, por un ayudante de su archivo o de apoyo, un miembro retirado (`retire_compile_pbt4.py`) |
| 119 | `WorkspacePilotUpdateRunTests.cs` | `AnOuterReleaseThatThrowsOutsidePostAddsItsCauseToTheClassificationThatWasAlreadyDecided` | 558-592 | C-7 por compilación: alcanza, por un ayudante de su archivo o de apoyo, un miembro retirado (`retire_compile_pbt4.py`) |
| 120 | `WorkspacePilotUpdateRunTests.cs` | `EveryWriteUnitReceivesTheTextOfThePlanForItsKeyInOrdinalOrder` | 81-92 | C-7 por compilación: alcanza, por un ayudante de su archivo o de apoyo, un miembro retirado (`retire_compile_pbt4.py`) |
| 121 | `WorkspacePilotUpdateRunTests.cs` | `SeveralMembersWithTheTextOfTheOriginAreAPreflightWithTheirOwnReasonAndNotTheMisleadingOneOfTheReader` | 145-157 | C-7 por compilación: alcanza, por un ayudante de su archivo o de apoyo, un miembro retirado (`retire_compile_pbt4.py`) |
| 122 | `WorkspacePilotUpdateRunTests.cs` | `TheDesiredNamesFollowTheRuleOfTheClassicEditorPerView` | 692-706 | C-7 por compilación: alcanza, por un ayudante de su archivo o de apoyo, un miembro retirado (`retire_compile_pbt4.py`) |
| 123 | `WorkspacePilotUpdateRunTests.cs` | `TheDraftIsRebasedAndCleanFromTheRereadOfPostWhenTheRunIsClosed` | 94-112 | C-7 por compilación: alcanza, por un ayudante de su archivo o de apoyo, un miembro retirado (`retire_compile_pbt4.py`) |
| 124 | `WorkspacePilotUpdateRunTests.cs` | `TheMissingPiecesAndAFailedRegenerationAreWarningsAndTheStateStaysClean` | 735-752 | C-7 por compilación: alcanza, por un ayudante de su archivo o de apoyo, un miembro retirado (`retire_compile_pbt4.py`) |
| 125 | `WorkspacePilotUpdateRunTests.cs` | `ThePostComparisonDoesNotNormalizeAnything` | 755-781 | C-7 por compilación: alcanza, por un ayudante de su archivo o de apoyo, un miembro retirado (`retire_compile_pbt4.py`) |
| 126 | `WorkspacePilotUpdateRunTests.cs` | `ThePostComparisonFailsForAnotherSetOfKeysAnotherEnvelopeAndAnotherScale` | 783-802 | C-7 por compilación: alcanza, por un ayudante de su archivo o de apoyo, un miembro retirado (`retire_compile_pbt4.py`) |
| 127 | `WorkspacePilotUpdateRunTests.cs` | `ThePostComparisonSeesAnotherScaleInTheStateThatTheRereadMaterializes` | 804-823 | C-7 por compilación: alcanza, por un ayudante de su archivo o de apoyo, un miembro retirado (`retire_compile_pbt4.py`) |
| 128 | `WorkspacePilotUpdateRunTests.cs` | `ThePostRereadUsesTheKeyOfTheOriginOfThePlanAndNotTheOneOfTheTextThatChanged` | 825-837 | C-7 por compilación: alcanza, por un ayudante de su archivo o de apoyo, un miembro retirado (`retire_compile_pbt4.py`) |
| 129 | `WorkspacePilotUpdateRunTests.cs` | `TheRunFollowsTheOrderOfThePackageAndEndsAppliedWithTheRereadOfPost` | 65-79 | C-7 por compilación: alcanza, por un ayudante de su archivo o de apoyo, un miembro retirado (`retire_compile_pbt4.py`) |
| 130 | `WorkspacePilotUpdateRunTests.cs` | `TheSurvivorsAndTheLockedLayersAreCheckedAgainInMutateAndNothingIsWritten` | 352-370 | C-7 por compilación: alcanza, por un ayudante de su archivo o de apoyo, un miembro retirado (`retire_compile_pbt4.py`) |
| 131 | `WorkspacePilotUpdateRunTests.cs` | `TheSystemIsRebuiltFromTheDesignTextOfAMemberOfThePlan` | 892-904 | C-7 por compilación: alcanza, por un ayudante de su archivo o de apoyo, un miembro retirado (`retire_compile_pbt4.py`) |
| 132 | `WorkspacePilotUpdateRunTests.cs` | `TheSystemRebuiltForAMemberCarriesTheRackNameOfItsEnvelope` | 909-963 | C-7 por compilación: alcanza, por un ayudante de su archivo o de apoyo, un miembro retirado (`retire_compile_pbt4.py`) |
| 133 | `WorkspacePilotUpdateRunTests.cs` | `ThereIsNoOriginWhenNoMutableMemberHasTheTextOfTheOriginOfTheDraft` | 129-143 | C-7 por compilación: alcanza, por un ayudante de su archivo o de apoyo, un miembro retirado (`retire_compile_pbt4.py`) |
| 134 | `WorkspacePilotUpdateRunTests.cs` | `WithoutSurvivorsNothingIsImportedAndTheRunStops` | 232-243 | C-7 por compilación: alcanza, por un ayudante de su archivo o de apoyo, un miembro retirado (`retire_compile_pbt4.py`) |
| 135 | `PushBackDeclaredCorrectionPilotParityTests.cs` | `CT_PBTC_06_TheWindowAndThePilotWriteTheSameDesignForTheFourteenDesigns` | 19-56 | C-7 por compilación: alcanza, por un ayudante de su archivo o de apoyo, un miembro retirado (`retire_compile_pbt4.py`) |
| 136 | `PushBackDeclaredCorrectionPilotParityTests.cs` | `CT_PBTC_10_TheWindowAndThePilotWriteTheSameDesignForEveryExactFieldAndForCustomModules` | 58-91 | C-7 por compilación: alcanza, por un ayudante de su archivo o de apoyo, un miembro retirado (`retire_compile_pbt4.py`) |
| 137 | `PushBackPilotUpdateEquivalenceGuardTests.cs` | `E_3_TheMemberPlansAreBuiltOnlyFromTheMutableMembersOfTheRereadInOrdinalOrderAndErasuresOnlyComeFromAnObsoleteLateralOfAModelThatErases` | 136-145 | C-7: E-3 de D-1 (F4B-E29: «PB-T4 sustituirá tres de esos equivalentes al quitar el piloto») |
| 138 | `PushBackPilotUpdateEquivalenceGuardTests.cs` | `E_5_TheEscapeOfEachRunIsClassifiedByTheStateThatTheRunAlreadyHadAndNeverAsAPlainFailureAfterAWriteOrASeal` | 220-228 | C-7: E-5 de D-1 (F4B-E29: «PB-T4 sustituirá tres de esos equivalentes al quitar el piloto») |
| 139 | `PushBackPilotUpdateEquivalenceGuardTests.cs` | `E_6_TheEscapeGuardTripsOnACatchThatDoesNotConsultTheStateAndOnAnEscapeThatAnswersAPlainFailureFirst` | 230-249 | C-7: E-6 de D-1 (F4B-E29: «PB-T4 sustituirá tres de esos equivalentes al quitar el piloto») |

## PB-T4c (2)

| # | Archivo | Método | Líneas | Causa |
|---|---|---|---|---|
| 1 | `WorkspacePilotEditGuardTests.cs` | `ThePortDeclaresTheSeventeenOperationsWithTheEnumsAsOrderedPrefixesAndNamesNoTypeOfTheDraftsNorOfThePilot` | 155-163 | C-5: PEG:155-163, las 17 operaciones del puerto pasan a 18 (`TryEditAll`) |
| 2 | `PushBackSharedReferenceGuardTests.cs` | `CT_PBT1B_15_TheWritersOfNameInThePluginRunAfterTheWindowCloses` | 306-403 | C-5: CT-PBT1B-15, «archivos=14» pasa a «archivos=15» con `RackWorkspaceEditorWindow.cs` |
