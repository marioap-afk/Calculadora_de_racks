# Analysis - F4-T2A-PILOT-STATE attempt 1, verification R20261112T052453Z-8bc4 (EXECUTION_BLOCKED / STOP, Ci, F4T2A-C02)

Written by the I-64 main session under the Owner night delegation (docs/automation/evidence/I-64-pilot/F4-DRAFTS/coordinator/
owner-night-delegation.md: the session takes the decisions tonight). Disposition F4-E24.

## Facts

- Delivery R20261111T221932Z-9353 (single Worker invocation, resumed once after a machine suspension, F4-E23): RED 19123dfe,
  GREEN 77658968, 30 paths inside AllowedWriteScope; the GREEN touches only the five product files.
- Verification 8bc4 (gpt-6-luna / high): 12 checks pass; Ci fail and Tests fail (RedPart pass in both), FailureClass Ci,
  TriggeredStopConditions F4T2A-C02. The RED 19123dfe is accredited; ChainRedFiles = RT (24 paths under tests/).
- CI 43160527925 (push of 77658968): Tests (Domain + Application) failure; UI Tests and Build UI success. 9 Core failures,
  all in WorkspaceDraftPilotMaterializationTests: the 7 cases of ACanonicalDesignComesBackAsTheIdenticalPersistedDocument,
  ACleanDraftComposesExactlyItsBase and the Keep-mine scenario.
- The Worker ran the full Core suite on Windows, Release and Debug: 16984/16984 green.

## Root cause of the CI failure (test portability, not product identity)

- The failing assertions compare persisted documents as TEXT: PilotKit.DesignOf(fixture) (the inner Design string of an
  envelope generated on Windows, with CRLF line endings inside the indented JSON) against a document serialized again on
  the test platform. The Core job runs on Linux, where indented System.Text.Json output uses LF; the error messages show
  Expected "{\r\n ..." versus Actual "{\n ...".
- The structural comparisons of the same documents pass in the same Linux run: EditingTheScaleOfACanonicalDesign...
  (JsonDiff: only AnnotationScale differs) and the legacy-delta tests. Identity of the authored content holds; F4T2A-C02 is
  triggered by a platform-dependent assertion, not by a product identity failure.
- The product runs only on Windows (AutoCAD); the platform-dependent newline of the persisted inner document is existing
  behavior and is not changed here.

## Defects found by the direct session review and the independent adversarial review

1. IMPORTANT (both reviews): ComposeForWrite restores the whole editable Snapshot and all inputs of the current state over
   the fresh base. After KeepMineOnCurrentVersion, a V2 that changed authored content the pilot does not edit (for example
   PalletTolerance, a cell pallet height, DimensionStyle, or the matrix) is silently reverted to V1, and the draft can stay
   Dirty with no visible cause. It contradicts IC-07 point 2 and D-12 ("sobre la version actual"); it matches the letter of
   INV-F4T2A-07 ("contenido Editable"), which is ambiguous: a contract defect of the session, not a Worker defect. The only
   Keep-mine test uses a V2 that differs in the module layer only, so it does not detect it.
2. MINOR: Rebase assigns StaleBase before copying the initial state; an exception of the copier desynchronizes them.
3. MINOR: the exception-to-Failed tests accept Admitted as an alternative and would pass without the try/catch; the
   load-order test omits ParkDormantTail.
4. MINOR: the legacy deltas summarize new containers ("null -> [3 items]", "<absent> -> {object}") instead of fixing values.
5. MINOR: the window-versus-pilot characterization has no data for classes (a), (c) and (d).
6. MINOR: an unreadable inner design gets NoDesignDiagnostic instead of the single INV-17 diagnostic.
7. MINOR: SetCurrentDraftState now stores a copy of its argument without documenting it.

## Decision (F4-E24)

- Delegated correction, attempt 2 of 3 (16.8), with a contract for the correction (gate-contract-attempt2.json, from
  revision 2) that rewrites INV-F4T2A-07 unambiguously (pilot-field-only composition from the current base, typed rejection
  of hidden non-pilot differences, KeepPilotEditsOn helper for the surface after Keep-mine) and extends INV-F4T2A-06, 08, 09
  and 10 for findings 2-7 and the portability defect; INV-F4T2A-16 fixes the correction RED.
- Same AllowedWriteScope and ForbiddenWriteScope (57). ChainBaseSha ed680e1f, ChainRedSha 19123dfe, ChainRedFiles = RT.
- The verification 8bc4, the delivery and the reviews are preserved as they are.
