I-64 — F2-T2A-NAVIGATION-HOST
BASESHA RECOVERY DISPOSITION
Current branch:
d9d386e91aa6df5db64fbb6946b7edb3542baace
origin branch:
d9d386e91aa6df5db64fbb6946b7edb3542baace
origin/main:
a140eb3a9f055518f0ffa5a3f927ff1c0e1c1e60
Worker:
NOT INVOKED
Worker authorization from the previous acceptance:
NOT CONSUMED
attempts:
1 / 3
The pre-invocation STOP is ACCEPTED.
Root cause:
SESSION / PROTOCOL ORDER DEFECT
The documentation custody commit:
d9d386e9
was created after planning and before Worker invocation.
That invalidated the accepted delegation's:
BaseSha = 35c3cd05...
for a normal Worker invocation.
Do NOT invoke the Worker from the old e137 delegation.
Do NOT change verification semantics to compensate.
Do NOT rewrite or revert published history.
Use:
REPLAN_ON_CURRENT_BASE
New planning BaseSha:
d9d386e91aa6df5db64fbb6946b7edb3542baace
No additional session/state/custody commit is authorized before planning.
The already saved:

* acceptance literal;
* pre-invocation STOP record;
* recovery disposition;

remain in ignored artifacts until the next normal custody point.
They do not need to be committed before the Worker.
Reuse the SAME issued T2A gate contract.
AuthorityRevision:
09a1499a46d16c8251ac4575d370b551603b6372
MainSha:
a140eb3a9f055518f0ffa5a3f927ff1c0e1c1e60
Do NOT issue a new contract.
Do NOT change:

* Objective;
* ForbiddenWriteScope;
* Invariants;
* RequiredTests;
* ExpectedEvidence;
* StopConditions;
* routing requirements.

The recovery changes only the planning base and carries the already accepted
Coordinator clarifications.
The new planning input must include the already accepted T2A disposition.
In particular:

1. WorkspaceNavigationEffects.cs may perform READ-ONLY GetObject /
equivalent access on the ALREADY-SELECTED target for VIEW/CENTER.
2. That read is only for:
   * existence;
   * type;
   * geometric extents;
   * information strictly necessary for the approved view/center effect.
3. WorkspaceNavigationEffects.cs is NOT a discovery authority.
4. WorkspaceHostC2SourceGuardTests.cs remains out of scope.
5. WorkspaceBridgeFacadeSourceGuardTests.cs remains out of scope.
6. WorkspaceSelectionRead.cs remains out of scope.
7. Any need for one of those files:
STOP P-03.
8. ExecuteInCommandContextAsync async/await exception remains limited to
WorkspaceNavigationHost.cs.

These are accepted Coordinator interpretations of the existing contract,
not a scope expansion.
Authorize:
ONE new Controller planning invocation.
Controller:
gpt-6-luna
Effort:
high
read-only
Use the same planning recipe as e137.
New planning must have:
BaseSha =
d9d386e91aa6df5db64fbb6946b7edb3542baace
The old e137 delegation becomes:
SUPERSEDED_FOR_EXECUTION_BY_BASE_MOVEMENT
It remains historical and immutable.
Do not label it invalid.
After the new planning output:
run A1-A8.
Then run nc4.
Do NOT commit either result before Worker invocation.
All artifacts remain under ignored artifacts/.
To avoid repeating the same protocol-order defect, the Worker is
PRE-AUTHORIZED without another Coordinator round IF AND ONLY IF every
condition below is true.

1. Planning output is schema-valid.
2. Planning terminates normally.
3. Effective Controller model/effort are:
gpt-6-luna / high.
4. Routing remains:
Deep
and:
NOT Long-horizon.
5. A1-A8:
all eight PASS.
6. nc4:
relative oracle MET;
A3 = fail;
every other A-result equals the real delegation.
7. New AllowedWriteScope is EXACTLY the same ordered scope as the accepted
e137 delegation:
same 7 new files
plus
same 8 exact existing files.
8. It does NOT add:
WorkspaceHostC2SourceGuardTests.cs
9. It does NOT add:
WorkspaceBridgeFacadeSourceGuardTests.cs
10. It does NOT add:
WorkspaceSelectionRead.cs
11. It does NOT remove any path from the accepted e137 scope.
12. ForbiddenWriteScope is identical to the contract.
13. Invariants are identical to the contract.
14. RequiredTests are identical to the contract.
15. ExpectedEvidence is identical to the contract.
16. StopConditions are identical to the contract.
17. Objective is identical to the contract.
18. BaseSha is exactly current HEAD/remoto:
d9d386e9...
19. origin/main remains:
a140eb3a...
20. config.toml remains accepted A/B/C/D immediately before Worker.
21. repository tree is clean.
22. HEAD = remote branch.
23. no participant/process conflict exists.
24. no usage/credit warning occurs during planning or nc4.

If ALL 24 conditions are met:
the new delegation is:
ACCEPTED_BY_PREAUTHORIZED_MECHANICAL_EQUIVALENCE
and:
ONE Worker invocation is AUTHORIZED immediately.
Do not stop for another Coordinator acceptance.
If ANY of the 24 conditions fails:
STOP.
Do NOT invoke Worker.
Return at:
COORDINATOR_F2_T2A_ACCEPTANCE_REQUIRED
with the exact difference.
No interpretation by the session.
From the start of the new planning through Worker completion:
NO session-authored Git commit.
Sequence:

1. current clean d9d386e9;
2. planning;
3. A1-A8;
4. nc4;
5. mechanical equivalence check;
6. Worker invocation;
7. Worker RED commit;
8. Worker GREEN commit/push.

The first product commits after BaseSha must therefore be Worker commits.
Do not custody planning/nc4 into Git before the Worker.
Their durable custody happens AFTER the Worker phase when the protocol again
permits session documentation commits.
With BaseSha = d9d386e9:
git log BaseSha..CurrentSha
during verification must contain only the Worker product chain relevant to
this delegation.
Therefore the previous session documentation commits are outside the
verification interval.
This restores normal Trailer semantics.
With BaseSha = d9d386e9:
git diff --name-only BaseSha..CurrentSha
contains only Worker changes.
Therefore the previous docs/ custody paths are outside the T2A Scope
interval.
No special Scope exception is required.
Preserve:
R20261109T171357Z-e137
as:
VALID PLANNING
VALID DELEGATION AT ITS ORIGINAL BASE
SUPERSEDED_FOR_EXECUTION_BY_BASE_MOVEMENT
Its:
A1-A8 PASS
and:
nc4 PASS
remain historical evidence.
They do NOT authorize execution after BaseSha moved.
The replan is recovery from a pre-invocation protocol-order defect.
Worker attempts:
still 1 / 3 available.
No Worker attempt has been consumed.
The new planning invocation is authorized as recovery.
Do not reset any historical counters.
If the mechanically equivalent delegation is produced and Worker completes:
STOP after the single Worker invocation.
Then proceed to the normal T2A verification chain.
Do NOT automatically invoke a second Worker.
Do NOT start T2B.
F2-T1:
CLOSED / PASS
F2-T2A:
BASE RECOVERY AUTHORIZED
old e137:
SUPERSEDED FOR EXECUTION
new planning:
AUTHORIZED ONCE
Worker:
CONDITIONALLY PRE-AUTHORIZED
NOT YET INVOKED
F2-T2B:
WAITING
Smoke-NAV:
PENDING
attempts:
1 / 3
