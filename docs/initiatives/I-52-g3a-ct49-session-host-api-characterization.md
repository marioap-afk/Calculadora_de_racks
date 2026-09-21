# I-52 — G3A: CT-49 session/host/overrule authority and AutoCAD API census

**Initiative:** I-52 / ID16 / RACKMIRROR  
**Starting I-52 SHA:** `3aeae415ba55774558ddea3dad5ec25d4c95b37f`  
**Base main:** `7097057cf8685bf5ecc09083cba37379d4a4aae8`  
**Proposal V17 blob:** `dd944d8e9c62b083eebc2b1292789977b880c368`  
**Consensus Freeze:** `87a0b72a159501a075105b9f6a80e33a083527ac`  
**Freeze blob:** `5f4933a3aa90d7052f571317107f1831ec02ac6e`  
**Gate result:** `G3A = STOP`  
**Reason:** CT-49 cannot demonstrate a closed catalog of persistent editing modes or generic detector coverage.

This artifact records characterization only. It does not implement RACKMIRROR, product guards, AUTH-15 or the
final G-M24 guard; it does not accept ADR-0036 or execute the complete CT-50 representability matrix.

## 0. Preflight and parallel movement

`git fetch --all --prune --tags` confirmed the I-52 branch and upstream at the starting SHA, `origin/main` at the
base above and annotated tag `integration/I-57` object `a5bc02210f740839e2fac37e63fc00512e5ccee6` targeting
`7097057cf8685bf5ecc09083cba37379d4a4aae8`. The I-52 worktree started clean, with no stash or incomplete Git
operation. The three retained worktrees were main, I-55 and I-52.

I-55 was `08cc5dfccc49c3ddcdc15fed5192cebd8583270b` at entry and advanced before publication to
`023020f8228a84d7fc059cd08c7a38d49a117b6a`. Its two commits implement and close its Push Back-specific G4
`PostIndex` correction in `RackCad.UI`, its tests and I-55 records. It does not touch Foundation, Plugin,
AutoCAD-session authority, the G3A tools/evidence or any I-52 file. Classification for this gate:
`contract impact = NON-MATERIAL`; `coordination = MATERIAL / OBSERVED`; no finding is imported as I-52 evidence.

## 1. Exact environment and evidence

The probe ran inside the installed AutoCAD 2025 Core Console. It loaded a development-only command from
`eng/validation/I52G3AAutoCadProbe`, read the host and session surfaces and exercised `Overrule.HasOverrule`.

| Item | Exact observation |
|---|---|
| Process | `C:\Program Files\Autodesk\AutoCAD 2025\accoreconsole.exe`; console build `V.171.0.0` |
| Managed application version | `25.0.0.0` |
| Product / program / market version | `AutoCAD` / `acad` / `2025` |
| Host implementation | `Autodesk.AutoCAD.DatabaseServices.ImpHostApplicationServices` |
| Registry identity | `Software\Autodesk\AutoCAD\R25.0\ACAD-8101:409` (machine and user) |
| `AcCoreMgd.dll` | file version `25.0.171.0.0`; SHA-256 `31025BC01ABC2CB398040AC82A5018E4FCD7A80FE8A2012DE0329E7F8ED746AA` |
| `AcDbMgd.dll` | file version `25.0.171.0.0`; SHA-256 `C360186CC0702E210635895B7608D1D8BD5FEEC6A00612B7581EF160FE09CEDE` |
| `AcMgd.dll` | file version `25.0.171.0.0`; SHA-256 `7AA8BF5F79F3F980DCC9F4CF9483F8438E773F6BF7C05EC3B263449B2846BC8C` |
| `acad.exe` | file version `R25.0.171.0.0`; SHA-256 `2A75996FD2A5C5EE0376FD5BA7CCED227A098193FF495E8CEEDA3A96FE326422` |
| Runtime evidence | `docs/automation/evidence/I-52-g3a-autocad-runtime-probe.json`; 1,040 enumerated system-variable descriptors; SHA-256 `01A76A09BC5A51953D8BE16A55347094821788B63EC9044D91C6F66A7F1EE83D` |
| API census evidence | `docs/automation/evidence/I-52-g3a-autocad-api-census.json`; SHA-256 `9054D14604734C4FA8A2DB13AD3879BCD89C5CD4CC8D0EA307DBF81ACC8B4723` |

The managed API surface and Autodesk documentation establish the members used below: the generic
`Overrule.HasOverrule(RXObject, RXClass)` query, `Application.GetSystemVariable`,
`Application.LongTransactionManager`, `LongTransactionManager.CurrentLongTransactionFor(Document)`,
`Application.Version`, and the product/program/market/registry properties on `HostApplicationServices`.

## 2. Closed-mode catalog audit

### 2.1 Candidate mode-to-authority matrix

| ModeId | Entry / exit | Persistence semantics | Specific detector | Generic coverage | Runtime / between commands | Failure mode | Verdict |
|---|---|---|---|---|---|---|---|
| `REFEDIT` | `REFEDIT` / `REFCLOSE` | Reference-edit working set and long transaction survive command prompts until close | `REFEDITNAME != ""`; long transaction is corroboration | No generic authority proves that every reference-edit state is represented solely by those two readings | Inactive direct read returned `""`; the variable is documented read-only. It is intended to remain set while the session exists between commands | Read error or disagreement; catalog failure remains separate | `DETECTABLE_SPECIFIC` for this known mode |
| `BEDIT` | `BEDIT` / `BCLOSE` | Block Editor environment persists until close | `BLOCKEDITOR != 0` | `CMDNAMES`, `CommandInProgress` and quiescence do not cover the between-command environment | Inactive direct read returned `0`; documented read-only | Read error; unknown host | `DETECTABLE_SPECIFIC` for this known mode |
| `ARRAYEDIT_SOURCE` | `ARRAYEDIT`, Source / `ARRAYCLOSE` | Associative-array source edit persists until close | `ARRAYEDITSTATE != 0` | No generic mode registry | Inactive direct read returned `0`; documented read-only | Read error; unknown host | `DETECTABLE_SPECIFIC` for this known mode |
| `BTESTBLOCK` | Block Editor test window / close test window | Test environment persists independently of the command that opened it | `BLOCKTESTWINDOW != 0` | No generic mode registry | Inactive direct read returned `0` | Read error; unknown host | `DETECTABLE_SPECIFIC` for this known mode |
| `LONG_TRANSACTION` | API/command creates / ends long transaction | May span commands | `CurrentLongTransactionFor(document) != ObjectId.Null` | It covers long transactions only; it does not state that every persistent editing environment is a long transaction | Inactive probe returned `(0)` | API read error or uncharacterized long-transaction subtype | `DETECTABLE_GENERIC` only for the declared long-transaction predicate |
| `OPEN_CLOSE_PAIRS` | Command/API-specific open and close pairs | Some pairs retain an editor or source context | No public registry enumerates all pairs or maps every pair to a state authority | Command activity is not proof of absence after an entry command ends | No complete runtime universe is exposed | A new or omitted pair can escape | `STOP_NO_RELIABLE_AUTHORITY` |
| `CONTEXTUAL_EDITORS` | Contextual tabs/environments, including text and generated-view editors | Several expose persistent context outside one command invocation | Individual signals discovered include `TEXTEDITOR`, `VIEWEDITOR`, `VIEWDETAILEDITOR`, `VIEWSECTIONEDITOR`, `VIEWSKETCHMODE`, `VIEWCREATION`, `VIEWDETAILCREATION`, `VIEWSECTIONCREATION` and `ARRAYCREATION` | No API classifies which of 1,040 variables are editing modes, which affect source geometry, or whether the list is complete for verticals/extensions | The descriptor enumerator exposed these names. In Core Console its generic `Value` access returned `eNotImplementedYet`; direct reads work only for explicitly selected variables | Missing mode or semantic mapping | `STOP_NO_RELIABLE_AUTHORITY` |
| `THIRD_PARTY_OR_VERTICAL_CONTEXT` | Host/extension-defined lifecycle | May persist between commands | None demonstrated | System-variable enumeration and command/quiescence signals do not promise extension-mode completeness | Not discoverable as a closed semantic set | A relevant host mode can escape | `STOP_NO_RELIABLE_AUTHORITY` |

The four minimum modes have concrete candidate detectors. That does not close the required universe.

### 2.2 Completeness result

The audit covered command state (`CMDNAMES`, `CMDACTIVE`, `Document.CommandInProgress`), editor/application
quiescence, long transactions, the four mandatory editing environments, the public system-variable enumerator,
read-only per-session/context descriptors, paired open/close lifecycles and reflected public managed API types.

None is a semantic registry of persistent editing modes:

1. command state only identifies commands currently executing;
2. quiescence only reports whether the application/editor can accept work;
3. the long-transaction manager covers long transactions, not every contextual editor;
4. `SystemVariableEnumerator` exposes variables, not a taxonomy or a proof that every relevant mode owns one;
5. the observed 1,040-descriptor set includes at least 32 read-only per-session variables and multiple editor/context
   signals, but supplies neither relevance semantics nor coverage across verticals and extensions; and
6. no reflected/documented public managed API enumerates all persistent mode lifecycles or proves that the listed
   specific detectors cover them.

Therefore a `MODE -> detector/authority` relation cannot be made closed. This is positive evidence about the
available authority surface, not the assertion "we did not see another mode". CT-49 STOP conditions 1 and 3 apply.

## 3. Host identity authority

`HostApplicationServices.Current` plus `Application.Version` is usable authority for this characterized host:

| Requirement | Authority | Result |
|---|---|---|
| Product | `HostApplicationServices.Product` | `AutoCAD` |
| Program/host | `HostApplicationServices.Program` and concrete host type | `acad`; `ImpHostApplicationServices` |
| Product line / vertical discriminator | machine/user product registry root plus product/program | `R25.0/ACAD-8101:409`; exact characterized set can require equality |
| Market release | `releaseMarketVersion` | `2025` |
| Managed API major | `Application.Version` | `25.0.0.0` |
| Exact installed API build | file versions and hashes above | `25.0.171.0.0`, exact hashes captured |
| Runtime read failure | each read is independently fallible | future E12 only after this authority is adopted |
| Host outside characterized set | exact tuple mismatch | future E12, not optimistic fallback |

Result: `HOST_AUTHORITY = CHARACTERIZED`. This success does not repair the independent mode-catalog failure.

## 4. Third-party overrules

Reflection over the exact AutoCAD 2025 managed assemblies found the following public `Overrule` families.
Family relevance is based on the operations each family can replace, rather than on the global
`Overrule.Overruling` flag.

| Family | Footprint relevant? | Applicability authority | Per subject / class | Global only? | Conservative bound | Future runtime decision |
|---|---|---|---|---|---|---|
| `DrawableOverrule` | Yes: drawing and extents | `HasOverrule(subject, DrawableOverrule RXClass)` plus filters/`IsApplicable` | Yes / yes | No | No general bound from the API | Applicable and unsupported -> `UNKNOWN` |
| `GeometryOverrule` | Yes: geometry/extents | Same generic query with family RXClass | Yes / yes | No | No general bound | Applicable and unsupported -> `UNKNOWN` |
| `TransformOverrule` | Yes: transform/copy behavior | Same | Yes / yes | No | No general bound | Applicable and unsupported -> `UNKNOWN` |
| `VisibilityOverrule` | Yes: visibility changes drawing/occupancy | Same | Yes / yes | No | No general bound | Applicable and unsupported -> `UNKNOWN` |
| `DimensionStyleOverrule` | Yes for dimension presentation/footprint | Same | Yes / yes | No | Only with later family-specific characterization | Unsupported -> `UNKNOWN` |
| `ObjectOverrule` | Potentially relevant through object lifecycle/behavior | Same | Yes / yes | No | Not generally | Conservatively `UNKNOWN` until member-specific support exists |
| `SubentityOverrule` | Potentially relevant for subentity paths/geometry consumers | Same | Yes / yes | No | Not generally | Conservatively `UNKNOWN` until member-specific support exists |
| `GripOverrule` | No for normal committed drawing/plot footprint: public surface is grip interaction | Same | Yes / yes | No | Not needed for representation | No footprint effect merely from registration |
| `OsnapOverrule` | No for normal drawing/plot footprint: public surface supplies snap interaction | Same | Yes / yes | No | Not needed | No footprint effect merely from registration |
| `HighlightOverrule` / `HighlightStateOverrule` | No for normal non-highlighted drawing/plot occupancy: public surface controls transient highlighting | Same | Yes / yes | No | Not needed | No footprint effect merely from registration |
| `PropertiesOverrule` | No by registration alone: public surface changes property-system exposure; a property edit remains an ordinary database EDIT | Same | Yes / yes | No | Not needed for the overrule itself | No footprint effect merely from registration; later writes are EDIT |

The executable probe registered a `DrawableOverrule` for `Line`. `HasOverrule` returned `true` for two line
subjects and `false` for a circle. With `SetCustomFilter` and `IsApplicable`, it returned `true` only for the line
whose start point matched the predicate and `false` for the second line and circle. This demonstrates both class
and subject/applicability granularity on the characterized build. The global fallback is not the only authority.

Third-party reactors remain outside runtime overrule footprint. Their completed writes are already database EDIT;
future writes are future edits. No result here turns `Overrule.Overruling` into `EVERYTHING UNKNOWN`.

## 5. AutoCadApiCallCensus

### 5.1 Discovery mechanism

`eng/validation/I52G3AApiCensus` loads the real `RackCad.Plugin.csproj` with `MSBuildWorkspace`, obtains the Roslyn
semantic model and discovers every symbol bound to `AcCoreMgd`, `AcDbMgd`, `AcMgd` or the
`Autodesk.AutoCAD.*` namespace. It scans every Plugin syntax tree for:

- explicit and implicit constructors;
- method invocations;
- property and field gets/sets;
- overloaded unary and binary operators; and
- event add/remove accessors.

Each exact site records repository path, line, column, symbol, enclosing member, expression, classification and
reason. This is independent of fixture names. Plugin is the only project allowed to reference AutoCAD; therefore
its complete compilation covers materializers and Plugin helpers. Project references are compiled to resolve
transitive helpers. Any unresolved indirect/reflection/COM path must remain a separate `UNCLASSIFIED` row.

The future G-M24 guard must use the same semantic discovery and compare the discovered symbol/site set with an
explicit classification manifest. An arbitrary new AutoCAD symbol is discovered because assembly/namespace
identity selects it, even when no fixture named its family. A fixture-only grep is non-conforming. The present
tool is characterization infrastructure and initial evidence, not the final production/source guard scheduled for
G5..G7.

### 5.2 Initial census

| Classification | Exact sites |
|---|---:|
| `READ_ONLY` | 790 |
| `DECLARED_MUTATOR` | 340 |
| `EXCLUDED_WITH_REASON` | 229 |
| `UNCLASSIFIED` | **0** |
| **Total** | **1,359** |

Kinds: 82 constructors, 502 method calls, 604 property/field gets, 63 property/field sets and 108 operators.
Workspace diagnostics: 0. The classification-set equality is `1,359 discovered = 1,359 classified`.

### 5.3 Fixture disposition

The current product contains `AppendEntity` (16), `AddNewlyCreatedDBObject` (26), `ReadDwgFile` (1),
`WblockCloneObjects` (1), `DeepCloneObjects` (1), `RecomputeDimensionBlock` (1), `RecordGraphicsModified` (3),
`TransformBy` (1), `StartOpenCloseTransaction` (1) and `UpgradeOpen` (8), as well as entity construction,
symbol-table registration, database/header reads, `ForWrite` and presentation setters. Each exact site appears in
the evidence.

The following required fixtures are `ABSENT FROM CURRENT PRODUCT`: `Insert`, `Wblock`, direct `Clone`,
`GetTransformedCopy`, `Explode`, `ExplodeToOwnerSpace`, `SetDatabaseDefaults`, `EvaluateHatch`, `GenerateLayout`,
commands/COM/reflection, `SetSystemVariable`, `SynchronizeAttributes`, `LayerStateManager.Restore*`, `ReloadXrefs`,
`ResolveXrefs`, `BindXrefs`, `AssocManager.Evaluate*`, `Database.Audit`, `ResetBlock`, `ConvertToStaticBlock`,
`Overrule.AddOverrule`, `Overrule.RemoveOverrule`, `Database.EvaluateFields`, `Field.Evaluate`,
`DataLinkManager.Update*`, `Table.UpdateDataLink`, `UnloadXrefs`, `DetachXref`, `AttachXref`, `OverlayXref`,
`LayoutManager.*` and `Database.UpdateExt`. Absence does not remove any fixture from the future guard contract.

### 5.4 Event/callback census

There are zero AutoCAD event add/remove sites in the current Plugin compilation for Database, Document, Editor,
Application or DocumentCollection. Consequently there are zero reachable registered callbacks or delayed chains
to classify in the current product. The future guard still must connect each new subscription to its callback,
transitive helpers and delayed work; a mutating callback makes the registration point to `DECLARED_MUTATOR`.

## 6. Fields and CT-49 causal model

The frozen causal rule remains coherent under characterization:

- recursively classify leaves of nested fields;
- hold EDIT-dependent leaves fixed at their current value while exploring the admitted STATE envelope;
- vary only STATE leaves whose state domain has been characterized;
- a dependency cycle without a finite conservative result bound is `UNKNOWN`;
- Attribute mutation is EDIT by default;
- a Dimension measured-value change caused by geometry is EDIT;
- style, system-variable and presentation causes are STATE only when that exact cause and envelope are
  characterized; and
- DataLink refresh remains the explicit STATE exception from V17.

Required outcomes are preserved: `Area(object) + Date` fixes the Area edit leaf and varies Date only in its
characterized envelope; `Length(object) + DataLink` fixes Length and applies the DataLink exception; nested
Attribute plus STATE fixes Attribute content; an unbounded cyclic field is `UNKNOWN`; Dimension geometry is EDIT
while explicitly characterized style/sysvar/presentation inputs may be STATE.

This records the causal model for T-M73. It does not execute the later T-M73 product doubles.

## 7. XREF characterization

| Case | Authority / result |
|---|---|
| Loaded and demonstrably complete | Current loaded in-memory content is authority; normal classification applies |
| Demand-loaded and potentially incomplete | `UNKNOWN`; do not force-load |
| Unresolved | `UNKNOWN`; never empty footprint |
| External file replaced without reload | Loaded in-memory content remains authority; no future file-identity comparison |
| Reload resolves to changed effective content | EDIT |

No path opens an external file merely to prove future identity.

## 8. V17 obligations and gate result

**T-M73:** the nested/cyclic-field and XREF causal dispositions above preserve the exact V17 rule, but the session
portion cannot proceed because the authority set cannot be closed. T-M73 remains a later product-test obligation.

**T-M75:** semantic symbol discovery demonstrates the required arbitrary-member mechanism and captures all current
sites. The fixture matrix is retained. The five artificial violations and the final guard remain scheduled for
G5..G7; they are not falsely declared complete here.

**G-M24:** the initial current-product census is `CHARACTERIZED` with 1,359/1,359 sites classified and zero
unclassified. This is a characterization result, not an implemented guard and not completion of CT-50.

**Material contradiction:** V17 requires a closed persistent-mode catalog and proven generic detector coverage
before implementation. AutoCAD 2025 exposes reliable specific detectors for known modes, but no public semantic
catalog or generic authority that proves completeness. The reduction is material and is an explicit R-57/CT-49
STOP condition accepted in O-1.

```text
HOST_AUTHORITY = CHARACTERIZED
OVERRULE_GRANULARITY = SUBJECT + OVERRULE CLASS / CHARACTERIZED
AUTOCAD_API_CURRENT_CENSUS = 1359 / UNCLASSIFIED 0
G-M24 = CHARACTERIZED / FINAL GUARD NOT IMPLEMENTED
CT-49 = INSUFFICIENT AUTHORITY
G3A = STOP
PROPOSAL V18 = REQUIRED
G3B = NOT READY / MUST NOT OPEN
ADR-0036 = PROPOSED
G3 = STOPPED
SUBSTANTIVE IMPLEMENTATION = BLOCKED
```
