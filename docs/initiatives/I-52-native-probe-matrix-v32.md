# I-52 — Native Probe Matrix V32

> **NPM-V32-1 / NORMATIVE / NOT EXECUTED.** Parent NPM-V31-1 blob
> `a288c3fc3efaa1dc65f477acb57ad07cb7b6ebe8`. This artifact is the single source of truth for
> ProbeId relationships. Every row has 24 fields; no probe has been executed.

## 1. Schema, relation projection and primary-authority invariant

```text
ProbeId; PrimaryAuthorityId; ScheduleOriginEventId; HeaderAuthority; Setup; Trigger;
PrimaryTransactionId; TopTransactionId; DocumentLock; Thread/Context; Mutation; S/M/SM;
SchedulePoint; ExecutionPoint; ExpectedBefore; ExpectedAfter; Verifier; LifecycleMarkers;
Threats; DA-P/H-P; PASS; FAIL; UNKNOWN; Cleanup.
```

Exactly one `PrimaryAuthorityId` is non-NONE. A `ScheduleOriginEventId` and every token in
`LifecycleMarkers` are different relation types. Tokens matching a catalogued native EventId project as
`MARKER_FOR`; marker ContractIds do not enter EventProjection. `NOT_APPLICABLE(reason)` is populated.

## 2. Immutable state and action ContractIds

| ContractId | Normative value |
|---|---|
| STATE-S-0 | F-XR destination ResultBuffer bytes UTF-8 `HFV30:S:0` |
| STATE-S-1 | F-XR destination ResultBuffer bytes UTF-8 `HFV30:S:1` |
| STATE-M-0 | F-REF-B displacement `(10,20,0)` and LayerId of `RACKCAD_CTDA_V30_A` |
| STATE-M-1 | F-REF-B displacement `(110,220,0)` and LayerId of `RACKCAD_CTDA_V30_B` |
| STATE-SM-0 | sibling ObjectIds `{F-REF-A,F-REF-B}` and linkage bytes `HFV30:LINK:A,B` |
| STATE-SM-1 | sibling ObjectIds `{F-REF-A,F-REF-C}` and linkage bytes `HFV30:LINK:A,C` |
| STATE-T-OPEN | primary/top T opaque ids, active count and before bytes/entities captured before trigger |
| STATE-T-ABORT | STATE-S-0 restored after abort in a fresh verification T |
| STATE-T-NESTED-PREEND | nested T remains visible while outer T is active and active count is greater than one |
| STATE-T-OUTERMOST-PREEND | final pre-end state is visible and active count equals one |
| STATE-T-OUTERMOST-END-CALLED | exact outermost `EndTransaction` notification observed for the controlled outermost T; STATE-S-0 remains unchanged; no commit, durability or success is inferred |
| STATE-T-ENDED | ended T is absent from active count and a fresh legal DB read is recorded |
| MUT-S | open distinct F-XR target in declared T; write exactly STATE-S-1 |
| MUT-M | open distinct F-REF-B; set exactly STATE-M-1 |
| MUT-SM | create/erase exact sibling to reach STATE-SM-1 and write exact linkage bytes |
| VER-S | fresh independent DB read of Xrecord bytes plus total-order log |
| VER-M | fresh independent DB read of Matrix3d/LayerId plus total-order log |
| VER-SM | VER-S + VER-M + independent enumeration of exact sibling ObjectIds |
| VER-T | primary/top T ids, active count, commit/abort result and fresh DB reread |

Expected values are immutable contract constants. The writer cannot redefine them. A `+` expression is an
ordered composition whose operands must each resolve literally in this artifact.

## 3. Result ContractIds

| ContractId | Normative definition |
|---|---|
| PASS-S | all required callbacks and markers occur in total order; semantic DB state equals ExpectedAfter; VER-S and required T/lifecycle checks succeed; cleanup completes |
| PASS-M | all required callbacks and markers occur in total order; physical DB state equals ExpectedAfter; VER-M and required T/lifecycle checks succeed; cleanup completes |
| PASS-SM | PASS-S and PASS-M both hold, exact sibling/linkage state equals ExpectedAfter, required lifecycle checks succeed and cleanup completes |
| FAIL-S | observable host behavior contradicts the required semantic rollback, persistence, lifecycle or accepted-boundary property |
| FAIL-M | observable host behavior contradicts the required physical rollback, persistence, lifecycle or accepted-boundary property |
| FAIL-SM | observable host behavior contradicts either required semantic or material/linkage property |
| UNKNOWN-COMMON | callback, field, T, lock or order unavailable; illegal/unobservable write; verifier or cleanup incomplete; no contrary host property established |

PASS, FAIL and UNKNOWN are disjoint. An illegal or unavailable write is UNKNOWN unless the observation itself
contradicts the property under test. No PASS is possible before its cleanup ContractId completes.

## 4. Context and marker ContractIds

| ContractId | Definition |
|---|---|
| H-DB | exact AH-V32 `dbmain.h` database-reactor authority |
| H-OBJ | exact AH-V32 `dbmain.h`/`dbObject.h` object/entity-reactor authority |
| H-TX | exact AH-V32 `dbtrans.h` transaction-reactor authority |
| H-ED | exact AH-V32 `aced.h` editor-reactor authority |
| H-DOC | exact AH-V32 `acdocman.h` document-manager-reactor authority |
| H-SEND | exact AH-V32 `acdocman.h::sendStringToExecute` authority |
| H-LOAD | exact AH-V32 `acedads.h` + `rxdlinkr.h` authority |
| T-PRIMARY | exact primary transaction id; top id logged at callback |
| T-FRESH | short explicit callback transaction, ended or aborted before callback return |
| T-ABORTING | primary T requested to abort; callback active count logged |
| LOCK-OBS | actual lock mode logged; no permission inferred |
| CTX-CALLBACK | native thread id, document/application context and database identity logged |
| CTX-COMMAND | delivered native research command context and thread logged |
| SP-IMMEDIATE | immediately before trigger operation/callback dispatch |
| SP-SEND | immediately before NS-SEND; enqueue `Acad::ErrorStatus` captured |
| EP-CALLBACK | first instruction of exact callback |
| EP-COMMAND | first instruction of exact queued research command |
| MARK-MANAGED-CMD-END | exact managed CommandEnded observation from HEC-V27-C1 |
| MARK-LOCK-RELEASE | exact scratch-document lock release observation |
| MARK-ORIGIN-RETURN | return of the exact ScheduleOriginEventId callback |
| MARK-REGISTRATION | exact payload reactor registration record `(event,object,database,reactor instance)` |
| MARK-LOAD-RESULT | NL-ARX-PATH result plus N-RX-LOADED identity record |

## 5. Deterministic cleanup ContractIds

| ContractId | Deterministic algorithm |
|---|---|
| CLEAN-BASE | abort any owned open T; unregister exact live reactor; delete fixture objects/resources in scratch DB; close scratch DWG without save; flush log; verify each step |
| CLEAN-OBJ | if N-OBJ-GOODBYE was not observed, remove the same retained object/entity reactor before fixture deletion; if it was observed, verify association is dead and never access notifier; then CLEAN-BASE |
| CLEAN-DEFER | unregister exact origin reactor and queued research command after delivery; verify no queued work remains; then CLEAN-BASE; inability to prove drain/removal is UNKNOWN |
| CLEAN-DOC | remove the same retained document-manager reactor, verify removal, then CLEAN-BASE |
| CLEAN-OBJ-DEFER | execute CLEAN-OBJ followed by CLEAN-DEFER and verify both |
| CLEAN-DOC-DEFER | execute CLEAN-DOC followed by CLEAN-DEFER and verify both |
| CLEAN-C15-PROCESS | run C15 in a dedicated scratch AutoCAD process; flush append-only log; attempt exact retained-reactor removal while notifier is live and record its status; any non-success forbids PASS; close scratch DWG without save; request clean process exit; verify exact PID exited; delete/quarantine scratch artifacts externally; process termination is the module-residency cleanup boundary and no in-process unload is attempted |

Every Cleanup cell resolves to one literal ContractId. Cleanup failure yields UNKNOWN unless it establishes a safety
contradiction, in which case it yields the applicable FAIL. C15 never runs in the user's active AutoCAD session.

## 6. Expected-state semantic determinism

`ExpectedStateDeterministic(c)` is true iff `c` names exactly one immutable expected state, contains no
alternative outcome branch, does not defer selection to runtime observation and cannot be selected or rewritten by
the writer after the trigger. The verifier always compares the fresh DB read against the expected ContractId fixed
before the trigger.

Validation recursively resolves every ContractId referenced by an `ExpectedAfter` cell and rejects definitions that
contain `or`, `either`, `chosen`, `actual result`, `whatever occurred`, `recorded outcome`, a committed/aborted
alternative, or equivalent runtime selection. The search applies to ContractId definitions, not only matrix cells.

`09N-D` fixes `ProbeOutcomeMode=COMMIT`, `STATE-S-0` and `STATE-S-1` before `N-ED-END`. An aborted callback
transaction cannot PASS. Abort is permitted only during cleanup of an owned transaction after FAIL/UNKNOWN; native
abort behavior remains characterized by `04N`.

V30 `STATE-CMD-RESULT` remains superseded and absent because it encoded an outcome selected at runtime.

## 7. Full normative matrix — retained V28 probes

```text
02N;N-DB-OPEN;NOT_APPLICABLE(no scheduler);H-DB;register scratch DB reactor,STATE-S-0;modify DB notifier A;T-PRIMARY;observed top T;LOCK-OBS;CTX-CALLBACK;MUT-S;S;SP-IMMEDIATE;EP-CALLBACK;STATE-S-0;STATE-S-1;VER-S+VER-T;N-DB-MOD;T2;P1/P2/P4;PASS-S and affiliation matches commit/abort;FAIL-S or escaped affiliation;UNKNOWN-COMMON;CLEAN-BASE
04N;N-TR-ABOUT-ABORT;NOT_APPLICABLE(no scheduler);H-TX;stage MUT-S and request primary abort;abort primary T;T-ABORTING;observed top T;LOCK-OBS;CTX-CALLBACK;controlled S marker on distinct target if legal;S;SP-IMMEDIATE;EP-CALLBACK;STATE-T-OPEN+STATE-S-0;STATE-T-ABORT;VER-T+VER-S;N-TR-ABORTED,N-ED-CANCEL,N-ED-FAIL;T2/T3/T4;P2/P3;PASS-S requires exact callback order and no staged state survives;FAIL-S if staged state survives or order contradicts authority;UNKNOWN-COMMON;CLEAN-BASE
09N-A;N-TR-ABOUT-END;NOT_APPLICABLE(no scheduler);H-TX;open nested T with STATE-S-0;end nested T while active count greater than 1;T-PRIMARY;outer T id;LOCK-OBS;CTX-CALLBACK;observation only;S/M;SP-IMMEDIATE;EP-CALLBACK;STATE-T-OPEN;STATE-T-NESTED-PREEND;VER-T+VER-S;N-TR-ABOUT-START,N-TR-STARTED,N-TR-ENDED;T2/T3/T4;P2/P3;callback,count,visibility and cleanup exact;observable count/order/visibility contradiction;UNKNOWN-COMMON;CLEAN-BASE
09N-B;N-TR-ABOUT-END;NOT_APPLICABLE(no scheduler);H-TX;outermost T with STATE-S-0;end T with active count equal 1;T-PRIMARY;same as primary;LOCK-OBS;CTX-CALLBACK;observation only;S/M;SP-IMMEDIATE;EP-CALLBACK;STATE-T-OPEN;STATE-T-OUTERMOST-PREEND;VER-T+VER-S;N-TR-ABOUT-START,N-TR-STARTED,N-TR-OUTERMOST-END-CALLED,N-TR-ENDED;T2/T4;P2/P3/P6;callback,count,visibility and cleanup exact;observable count/order contradiction;UNKNOWN-COMMON;CLEAN-BASE
09N-O;N-TR-OUTERMOST-END-CALLED;NOT_APPLICABLE(no scheduler);H-TX;outermost T with STATE-S-0 and retained exact reactor;call endTransaction on the controlled outermost T;T-PRIMARY;same as primary;LOCK-OBS;CTX-CALLBACK;observation only;S/M;SP-IMMEDIATE;EP-CALLBACK;STATE-T-OUTERMOST-PREEND;STATE-T-OUTERMOST-END-CALLED;VER-T+VER-S;N-TR-ABOUT-END,N-TR-ENDED;T2/T4;P2/P3/P6;callback identity,count,state,order and cleanup exact without commit inference;missing callback or observable count/state/order contradiction;UNKNOWN-COMMON;CLEAN-BASE
09N-C;N-TR-ENDED;NOT_APPLICABLE(no scheduler);H-TX;commit controlled T from STATE-S-0;end controlled T;ended T id;observed top T after end;LOCK-OBS;CTX-CALLBACK;observation plus fresh read when legal;S/M;SP-IMMEDIATE;EP-CALLBACK;STATE-T-OPEN;STATE-T-ENDED;VER-T+VER-S;N-TR-ABOUT-START,N-TR-STARTED,N-TR-OUTERMOST-END-CALLED,N-ED-END;T2/T4/T5/T6;P2/P3/P6;callback,count,reread and cleanup exact;observable state/count/order contradiction;UNKNOWN-COMMON;CLEAN-BASE
09N-D;N-ED-END;NOT_APPLICABLE(no scheduler);H-ED;set immutable ProbeOutcomeMode=COMMIT before trigger and finish fixture command with STATE-S-0;native command-ended callback starts exact short T-FRESH;T-FRESH;observed top T;LOCK-OBS;CTX-CALLBACK;MUT-S then commit/end T-FRESH successfully before callback return;S;SP-IMMEDIATE;EP-CALLBACK;STATE-S-0;STATE-S-1;VER-S+VER-T;N-TR-ABOUT-START,N-TR-STARTED,N-TR-OUTERMOST-END-CALLED,N-TR-ENDED,MARK-MANAGED-CMD-END,MARK-LOCK-RELEASE;T2/T4/T6/T8;P6/P12;PASS-S requires N-ED-END,T-FRESH,MUT-S,successful commit/end,callback return after T completion,STATE-S-1,lifecycle order and cleanup;FAIL-S if committed T reports success but DB differs or callback/T/lifecycle order contradicts contract;UNKNOWN-COMMON includes illegal T start,MUT-S or commit unavailable/unobserved;CLEAN-BASE
10N-S;N-ED-END;NOT_APPLICABLE(no scheduler);H-ED;finish fixture command at STATE-S-0;native command-ended callback;T-FRESH;observed top T;LOCK-OBS;CTX-CALLBACK;MUT-S;S;SP-IMMEDIATE;EP-CALLBACK;STATE-S-0;STATE-S-1;VER-S+VER-T;N-TR-ABOUT-START,N-TR-STARTED,N-TR-OUTERMOST-END-CALLED,N-TR-ENDED,MARK-MANAGED-CMD-END,MARK-LOCK-RELEASE;T2/T8;P6/P12;PASS-S with candidate-boundary classification;FAIL-S on accepted-boundary contradiction;UNKNOWN-COMMON;CLEAN-BASE
10N-M;N-ED-END;NOT_APPLICABLE(no scheduler);H-ED;finish fixture command at STATE-M-0;native command-ended callback;T-FRESH;observed top T;LOCK-OBS;CTX-CALLBACK;MUT-M;M;SP-IMMEDIATE;EP-CALLBACK;STATE-M-0;STATE-M-1;VER-M+VER-T;N-TR-ABOUT-START,N-TR-STARTED,N-TR-OUTERMOST-END-CALLED,N-TR-ENDED,MARK-MANAGED-CMD-END,MARK-LOCK-RELEASE;T2/T8/T12;P11/P12;PASS-M with candidate-boundary classification;FAIL-M on accepted-boundary contradiction;UNKNOWN-COMMON;CLEAN-BASE
10N-SM;N-ED-END;NOT_APPLICABLE(no scheduler);H-ED;finish fixture command at STATE-SM-0;native command-ended callback;T-FRESH;observed top T;LOCK-OBS;CTX-CALLBACK;MUT-SM;SM;SP-IMMEDIATE;EP-CALLBACK;STATE-SM-0;STATE-SM-1;VER-SM+VER-T;N-TR-ABOUT-START,N-TR-STARTED,N-TR-OUTERMOST-END-CALLED,N-TR-ENDED,N-DB-APPEND,MARK-MANAGED-CMD-END,MARK-LOCK-RELEASE;T2/T8/T11;P6/P11/P12;PASS-SM with candidate-boundary classification;FAIL-SM on accepted-boundary contradiction;UNKNOWN-COMMON;CLEAN-BASE
16N-S;NS-SEND;N-DB-MOD;H-SEND+H-DB;register exact queued S command and STATE-S-0;N-DB-MOD calls NS-SEND;origin primary T;origin top T;LOCK-OBS;CTX-CALLBACK then CTX-COMMAND;MUT-S;S;SP-SEND;EP-COMMAND;STATE-S-0;STATE-S-1;VER-S+VER-T;MARK-ORIGIN-RETURN,N-ED-WILL,N-ED-END,MARK-LOCK-RELEASE;T2/T8/T9/T16;P6/P12;PASS-S with enqueue and delivery exact;FAIL-S on causal-boundary contradiction;UNKNOWN-COMMON including nondelivery;CLEAN-DEFER
16N-M;NS-SEND;N-DB-MOD;H-SEND+H-DB;register exact queued M command and STATE-M-0;N-DB-MOD calls NS-SEND;origin primary T;origin top T;LOCK-OBS;CTX-CALLBACK then CTX-COMMAND;MUT-M;M;SP-SEND;EP-COMMAND;STATE-M-0;STATE-M-1;VER-M+VER-T;MARK-ORIGIN-RETURN,N-ED-WILL,N-ED-END,MARK-LOCK-RELEASE;T2/T8/T12/T16;P11/P12;PASS-M with enqueue and delivery exact;FAIL-M on causal-boundary contradiction;UNKNOWN-COMMON including nondelivery;CLEAN-DEFER
16N-SM;NS-SEND;N-DB-MOD;H-SEND+H-DB;register exact queued SM command and STATE-SM-0;N-DB-MOD calls NS-SEND;origin primary T;origin top T;LOCK-OBS;CTX-CALLBACK then CTX-COMMAND;MUT-SM;SM;SP-SEND;EP-COMMAND;STATE-SM-0;STATE-SM-1;VER-SM+VER-T;MARK-ORIGIN-RETURN,N-DB-APPEND,N-ED-WILL,N-ED-END,MARK-LOCK-RELEASE;T2/T8/T11/T16;P6/P11/P12;PASS-SM with enqueue and delivery exact;FAIL-SM on causal-boundary contradiction;UNKNOWN-COMMON including nondelivery;CLEAN-DEFER
C15N16N-SM;NS-SEND;N-DB-MOD;H-LOAD+H-SEND+H-DB;dedicated process,bootstrap loaded,payload path fixed,STATE-SM-0,call NL-ARX-PATH,observe N-RX-LOADED and MARK-REGISTRATION;modify fixture notifier after registration;origin primary T;origin top T;LOCK-OBS;CTX-CALLBACK then CTX-COMMAND;MUT-SM;SM;SP-SEND;EP-COMMAND;STATE-SM-0;STATE-SM-1;VER-SM+VER-T;MARK-LOAD-RESULT,N-RX-WILL-LOAD,N-RX-LOADED,MARK-REGISTRATION,MARK-ORIGIN-RETURN,N-DB-APPEND,N-ED-WILL,N-ED-END;T2/T15/T16;P6/P11/P12;PASS-SM requires load,registration,event,enqueue,execution,state,order and CLEAN-C15-PROCESS;FAIL-SM on lifecycle/safety or DB contradiction;UNKNOWN-COMMON including missing load,registration,delivery or cleanup proof;CLEAN-C15-PROCESS
```

## 8. Full normative matrix — object/entity/document probes

```text
02NO-S;N-OBJ-OPEN;NOT_APPLICABLE(no scheduler);H-OBJ;attach OR-XR to semantic notifier A,STATE-S-0;modify notifier A to invoke assertWriteEnabled;T-PRIMARY;observed top T;LOCK-OBS;CTX-CALLBACK;MUT-S on distinct target B;S;SP-IMMEDIATE;EP-CALLBACK;STATE-S-0;STATE-S-1;VER-S+VER-T;N-OBJ-MOD,N-OBJ-CLOSED;T2-OBJ;P1/P2/P4;PASS-S with exact notifier/T affiliation;FAIL-S on affiliation or notifier-write contradiction;UNKNOWN-COMMON;CLEAN-OBJ
02NO-M;N-OBJ-MOD;NOT_APPLICABLE(no scheduler);H-OBJ;attach OR-REF-A,STATE-M-0;modify and close F-REF-A;T-PRIMARY;observed top T;LOCK-OBS;CTX-CALLBACK;MUT-M on F-REF-B;M;SP-IMMEDIATE;EP-CALLBACK;STATE-M-0;STATE-M-1;VER-M+VER-T;N-OBJ-OPEN,N-OBJ-CLOSED,N-ENT-GFX;T2-OBJ;P1/P2/P11;PASS-M with exact callback order;FAIL-M on state/order contradiction;UNKNOWN-COMMON;CLEAN-OBJ
02NO-SM;N-OBJ-ERASE;NOT_APPLICABLE(no scheduler);H-OBJ;attach OR-REF-B,STATE-SM-0;erase F-REF-B;T-PRIMARY;observed top T;LOCK-OBS;CTX-CALLBACK;MUT-SM on distinct target/linkage;SM;SP-IMMEDIATE;EP-CALLBACK;STATE-SM-0;STATE-SM-1;VER-SM+VER-T;N-OBJ-CLOSED,N-DB-ERASE;T2-OBJ/T7/T11;P6/P9/P11;PASS-SM with exact erase flag/order;FAIL-SM on sibling/linkage contradiction;UNKNOWN-COMMON;CLEAN-OBJ
02NO-CLOSE-SM;N-OBJ-CLOSED;NOT_APPLICABLE(no scheduler);H-OBJ;attach OR-REF-A,STATE-SM-0;modify then close F-REF-A;T-PRIMARY;observed top T;LOCK-OBS;CTX-CALLBACK;MUT-SM on distinct B,C only;SM;SP-IMMEDIATE;EP-CALLBACK;STATE-SM-0;STATE-SM-1;VER-SM+VER-T;N-OBJ-MOD;T2-OBJ;P2/P6/P11;PASS-SM plus observational eWasNotifying check;FAIL-SM on notifier or T contradiction;UNKNOWN-COMMON;CLEAN-OBJ
04NO-S;N-OBJ-CANCEL;NOT_APPLICABLE(no scheduler);H-OBJ;attach OR-XR,stage semantic modification then cancel object open;cancel notifier open;T-ABORTING;observed top T;LOCK-OBS;CTX-CALLBACK;MUT-S on distinct target if legal;S;SP-IMMEDIATE;EP-CALLBACK;STATE-S-0;STATE-T-ABORT;VER-S+VER-T;N-OBJ-UNDO,N-OBJ-MOD,N-OBJ-CLOSED;T2-OBJ/T3;P2/P3;PASS-S with exact cancel order and restored state;FAIL-S if forbidden state survives;UNKNOWN-COMMON;CLEAN-OBJ
04NO-UNDO-S;N-OBJ-UNDO;NOT_APPLICABLE(no scheduler);H-OBJ;attach OR-XR,modify then cancel/controlled undo;close after undo/cancel;T-ABORTING;observed top T;LOCK-OBS;CTX-CALLBACK;MUT-S on distinct target if legal;S;SP-IMMEDIATE;EP-CALLBACK;STATE-S-0;STATE-T-ABORT;VER-S+VER-T;N-OBJ-CANCEL,N-OBJ-CLOSED;T2-OBJ/T3;P2/P3;PASS-S with exact undo order and restored state;FAIL-S on state/order contradiction;UNKNOWN-COMMON;CLEAN-OBJ
02NE-M;N-ENT-GFX;NOT_APPLICABLE(no scheduler);H-OBJ;attach ER-REF-A,STATE-M-0;transform F-REF-A and close;T-PRIMARY;observed top T;LOCK-OBS;CTX-CALLBACK;MUT-M on F-REF-B;M;SP-IMMEDIATE;EP-CALLBACK;STATE-M-0;STATE-M-1;VER-M+VER-T;N-OBJ-MOD,N-OBJ-CLOSED;T2-ENTITY/T12;P2/P9/P11;PASS-M with exact graphics callback/order;FAIL-M on state/order contradiction;UNKNOWN-COMMON;CLEAN-OBJ
10NDOC-WILL-SM;N-DOC-LOCK-WILL;NOT_APPLICABLE(no scheduler);H-DOC;register doc reactor,STATE-SM-0;controlled scratch lock acquisition/release;observed callback T or NONE;observed top T or NONE;LOCK-OBS;CTX-CALLBACK;obtain legal context/T then MUT-SM;SM;SP-IMMEDIATE;EP-CALLBACK;STATE-SM-0;STATE-SM-1;VER-SM+VER-T;N-DOC-LOCK-CHANGED,N-DOC-LOCK-VETO;T2-DOC/T8;P6/P11/P12;PASS-SM requires callback,modes,legal T,commit,state,order and cleanup;FAIL-SM on observable lifecycle/state contradiction;UNKNOWN-COMMON when legal mutation context cannot be obtained;CLEAN-DOC
10NDOC-CHANGED-SM;N-DOC-LOCK-CHANGED;NOT_APPLICABLE(no scheduler);H-DOC;register doc reactor,STATE-SM-0;controlled scratch lock acquisition then release;observed callback T or NONE;observed top T or NONE;LOCK-OBS;CTX-CALLBACK;obtain legal context/T then MUT-SM;SM;SP-IMMEDIATE;EP-CALLBACK;STATE-SM-0;STATE-SM-1;VER-SM+VER-T;N-DOC-LOCK-WILL,N-DOC-LOCK-VETO;T2-DOC/T8;P6/P11/P12;PASS-SM requires modes,legal T,commit,state,order and cleanup;FAIL-SM on observable lifecycle/state contradiction;UNKNOWN-COMMON when legal mutation context cannot be obtained;CLEAN-DOC
10NDOC-VETO-SM;N-DOC-LOCK-VETO;NOT_APPLICABLE(no scheduler);H-DOC;register observer plus controlled earlier veto reactor,STATE-SM-0;request lock and veto it;NONE expected;NONE expected;LOCK-OBS;CTX-CALLBACK;observation only,no mutation attempted;SM;SP-IMMEDIATE;EP-CALLBACK;STATE-SM-0;STATE-SM-0;VER-SM;N-DOC-LOCK-WILL,N-DOC-LOCK-CHANGED,N-ED-CANCEL;T2-DOC;P12;PASS-SM requires veto/order exact,no attributed write,state unchanged and cleanup;FAIL-SM if write succeeds using vetoed authority or order/state contradicts contract;UNKNOWN-COMMON;CLEAN-DOC
C2OBJ16N-S;NS-SEND;N-OBJ-MOD;H-OBJ+H-SEND;attach OR-REF-A,register queued S command,STATE-S-0;modify/close F-REF-A;origin primary T;origin top T;LOCK-OBS;CTX-CALLBACK then CTX-COMMAND;MUT-S;S;SP-SEND;EP-COMMAND;STATE-S-0;STATE-S-1;VER-S+VER-T;N-OBJ-CLOSED,MARK-ORIGIN-RETURN,N-ED-WILL,N-ED-END;T2-OBJ/T16;P6/P12;PASS-S with enqueue,delivery,state and order exact;FAIL-S on causal-boundary contradiction;UNKNOWN-COMMON including nondelivery;CLEAN-OBJ-DEFER
C2OBJ16N-M;NS-SEND;N-OBJ-MOD;H-OBJ+H-SEND;attach OR-REF-A,register queued M command,STATE-M-0;modify/close F-REF-A;origin primary T;origin top T;LOCK-OBS;CTX-CALLBACK then CTX-COMMAND;MUT-M;M;SP-SEND;EP-COMMAND;STATE-M-0;STATE-M-1;VER-M+VER-T;N-OBJ-CLOSED,MARK-ORIGIN-RETURN,N-ED-WILL,N-ED-END;T2-OBJ/T16;P11/P12;PASS-M with enqueue,delivery,state and order exact;FAIL-M on causal-boundary contradiction;UNKNOWN-COMMON including nondelivery;CLEAN-OBJ-DEFER
C2OBJ16N-SM;NS-SEND;N-OBJ-MOD;H-OBJ+H-SEND;attach OR-REF-A,register queued SM command,STATE-SM-0;modify/close F-REF-A;origin primary T;origin top T;LOCK-OBS;CTX-CALLBACK then CTX-COMMAND;MUT-SM;SM;SP-SEND;EP-COMMAND;STATE-SM-0;STATE-SM-1;VER-SM+VER-T;N-OBJ-CLOSED,MARK-ORIGIN-RETURN,N-ED-WILL,N-ED-END;T2-OBJ/T16;P6/P11/P12;PASS-SM with enqueue,delivery,state and order exact;FAIL-SM on causal-boundary contradiction;UNKNOWN-COMMON including nondelivery;CLEAN-OBJ-DEFER
C2ENT16N-M;NS-SEND;N-ENT-GFX;H-OBJ+H-SEND;attach ER-REF-A,register queued M command,STATE-M-0;transform/close F-REF-A;origin primary T;origin top T;LOCK-OBS;CTX-CALLBACK then CTX-COMMAND;MUT-M;M;SP-SEND;EP-COMMAND;STATE-M-0;STATE-M-1;VER-M+VER-T;N-OBJ-CLOSED,MARK-ORIGIN-RETURN,N-ED-WILL,N-ED-END;T2-ENTITY/T16;P11/P12;PASS-M with enqueue,delivery,state and order exact;FAIL-M on causal-boundary contradiction;UNKNOWN-COMMON including nondelivery;CLEAN-OBJ-DEFER
C2DOCW16N-SM;NS-SEND;N-DOC-LOCK-WILL;H-DOC+H-SEND;register doc reactor and queued SM command,STATE-SM-0;controlled lock change;origin T observed or NONE;origin top T observed or NONE;LOCK-OBS;CTX-CALLBACK then CTX-COMMAND;MUT-SM;SM;SP-SEND;EP-COMMAND;STATE-SM-0;STATE-SM-1;VER-SM+VER-T;N-DOC-LOCK-CHANGED,MARK-ORIGIN-RETURN,N-ED-WILL,N-ED-END;T2-DOC/T16;P6/P11/P12;PASS-SM with enqueue,delivery,state,lock/order and cleanup exact;FAIL-SM on causal-boundary contradiction;UNKNOWN-COMMON including illegal enqueue or nondelivery;CLEAN-DOC-DEFER
C2DOCC16N-SM;NS-SEND;N-DOC-LOCK-CHANGED;H-DOC+H-SEND;register doc reactor and queued SM command,STATE-SM-0;controlled lock acquisition/release;origin T observed or NONE;origin top T observed or NONE;LOCK-OBS;CTX-CALLBACK then CTX-COMMAND;MUT-SM;SM;SP-SEND;EP-COMMAND;STATE-SM-0;STATE-SM-1;VER-SM+VER-T;N-DOC-LOCK-WILL,MARK-ORIGIN-RETURN,N-ED-WILL,N-ED-END;T2-DOC/T16;P6/P11/P12;PASS-SM with enqueue,delivery,state,lock/order and cleanup exact;FAIL-SM on causal-boundary contradiction;UNKNOWN-COMMON including illegal enqueue or nondelivery;CLEAN-DOC-DEFER
```

## 9. Mechanical closure result

The validation script parses the two code blocks above as semicolon-separated data and derives both inverse
projections. Its publication result is:

```text
Concrete ProbeId count = 30
Unique ProbeId count = 30
Rows with fields other than 24 = 0
Rows with zero/multiple primary authority = 0
Blank schema cells = 0
Missing literal ContractIds = 0
ContractId definitions = 55
Duplicate ContractIds = 0
Unresolved ExpectedBefore/ExpectedAfter operands = 0
Conditional ExpectedAfter cells = 0
Referenced ExpectedAfter ContractIds = 9
Semantically deterministic = 9
Non-deterministic = 0
Runtime-selected = 0
OR-branch definitions = 0
Post-trigger expected-selection paths = 0
Unresolved Cleanup ContractIds = 0
Normative wildcard/range rows = 0
Result state for every row = DEFINED / NOT EXECUTED
Event relation triples expected/actual = 116/116
Event relation missing/extra = 0/0
Scheduler pairs expected/actual = 10/10
Scheduler pairs missing/extra = 0/0
```

`09N-B` remains the `transactionAboutToEnd(count==1)` specialization because the actual header exposes it and
`endCalledOnOutermostTransaction` as different virtual callbacks. New probe `09N-O` independently records the
outermost notification; neither row calls either phase commit, durability, `C_decide`, `C_report` or `C_host`.

The actual-header additions `N-TR-ABOUT-START`, `N-TR-STARTED`, `N-DB-APPEND` and `N-RX-WILL-LOAD` are markers only
for the affected fixture paths. They never become primary authority merely by being observed.
