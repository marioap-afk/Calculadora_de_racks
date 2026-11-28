TaskId: F1-T1-MODEL
FailureClass: Ci
Disposition: REWORK
VerifiedSha:
66d34af3
ControllerVerificationRunId:
R20261002T011239Z-647e
ROOT CAUSE 1 — test namespace incompatible with integrated repository guard
The F1-T1 test files were authored under namespace `RackCad.Tests.Workspace`.
The integrated `NamespaceFolderGuardTests.TestProjects_KeepExactlyOneAssemblyRootNamespace`
requires files in `tests/RackCad.Tests` to use the exact assembly root namespace
`RackCad.Tests`.
The gate requirement is behavioral selection through:
`FullyQualifiedName~RackCad.Tests.Workspace`.
It does not require a nested C# namespace.
Correction:

* use `namespace RackCad.Tests`;
* name the F1-T1 test classes so their fully-qualified names still contain
`RackCad.Tests.Workspace`, e.g. `Workspace...Tests`;
* preserve the existing contract filter;
* do not weaken or modify NamespaceFolderGuardTests.

ROOT CAUSE 2 — over-broad authored-data source guard
The new RED guard for “no persistence/authored writes” rejects the token
`Registry`.
That token also occurs legitimately in the pure in-memory type
`WorkspaceSessionRegistry`.
The guard therefore tests a lexical coincidence rather than the frozen
architectural invariant.
Correction:

* narrow the guard to concrete persistence/write authorities or forbidden APIs;
* it must continue detecting actual persistence/authored-write dependencies;
* it must not reject a pure in-memory session registry merely because its type
name contains `Registry`;
* do not weaken INV-F1-T1-03 or INV-F1-T1-04.

SCOPE
Correction remains inside:

* src/RackCad.Application/Workspace/
* tests/RackCad.Tests/Workspace/
* docs/adr/0047-workspace-persistente-modeless-rackcad.md only if no semantic
change is required; otherwise leave the ADR untouched.

No Plugin/UI/Domain/AutoCAD.
No Freeze change.
No RequiredTests semantic change.
CHAIN
ChainBaseSha =
e0587355b0e84d98807057a56dfc50591da87489
ChainRedSha =
36c344dc
ChainRedFiles =
the 4 RED tests already accredited by Controller verification.
IMPORTANT:
the existing RED remains authoritative for the chain.
Do NOT manufacture a new artificial RED merely because this is a correction.
