# I-52 — Native Probe Matrix V29

> **NPM-V29-1 / NORMATIVE / NOT EXECUTED.** Cada fila resuelve los 24 campos del schema nativo. ContractIds de esta
> seccion son parte de la fila; no dependen del writer ni de narrativa externa.

## 1. Schema y primary-authority invariant

```text
ProbeId; PrimaryAuthorityId; ScheduleOriginEventId; HeaderAuthority; Setup; Trigger;
PrimaryTransactionId; TopTransactionId; DocumentLock; Thread/Context; Mutation; S/M/SM;
SchedulePoint; ExecutionPoint; ExpectedBefore; ExpectedAfter; Verifier; LifecycleMarkers;
Threats; DA-P/H-P; PASS; FAIL; UNKNOWN; Cleanup.
```

Exactly one `PrimaryAuthorityId` is non-NONE. `ScheduleOriginEventId`, markers, load action and setup are never second
primary authorities. `NOT_APPLICABLE(reason)` is a populated value.

## 2. Immutable fixture contracts

| ContractId | Normative value |
|---|---|
| STATE-S-0 | F-XR destination ResultBuffer bytes UTF-8 `HFV29:S:0` |
| STATE-S-1 | F-XR destination ResultBuffer bytes UTF-8 `HFV29:S:1` |
| STATE-M-0 | F-REF-B displacement `(10,20,0)` and LayerId of `RACKCAD_CTDA_V29_A` |
| STATE-M-1 | F-REF-B displacement `(110,220,0)` and LayerId of `RACKCAD_CTDA_V29_B` |
| STATE-SM-0 | sibling ObjectIds `{F-REF-A,F-REF-B}` and linkage bytes `HFV29:LINK:A,B` |
| STATE-SM-1 | sibling ObjectIds `{F-REF-A,F-REF-C}` and linkage bytes `HFV29:LINK:A,C` |
| STATE-T-OPEN | primary/top T opaque ids, active count, before bytes/entities captured before trigger |
| STATE-T-ABORT | STATE-S/M/SM-0 restored after abort in a fresh verification T |
| MUT-S | open distinct F-XR target in declared T; write exactly STATE-S-1 |
| MUT-M | open distinct F-REF-B; set exactly STATE-M-1 |
| MUT-SM | create/erase exact sibling to reach STATE-SM-1 and write exact linkage bytes |
| VER-S | fresh independent DB read of Xrecord bytes plus total-order log |
| VER-M | fresh independent DB read of Matrix3d/LayerId plus total-order log |
| VER-SM | VER-S + VER-M + independent enumeration of exact sibling ObjectIds |
| VER-T | primary/top T ids, active count, commit/abort result and fresh DB reread |
| PASS-S/M/SM | all required callbacks/markers observed in total order and actual DB equals declared ExpectedAfter |
| FAIL-S/M/SM | actual host behavior contradicts rollback, lifecycle or accepted boundary property |
| UNKNOWN-COMMON | callback/field/T/lock/order unavailable; illegal/unobservable write; verifier or cleanup incomplete |
| CLEAN-BASE | unregister exact reactor; abort leftover T; delete fixture objects/resources in scratch DB |
| CLEAN-OBJ | remove same object/entity reactor unless N-OBJ-GOODBYE marked notifier dead; then CLEAN-BASE |
| CLEAN-DEFER | unregister queued command/origin reactor; drain/cancel only by documented host path; CLEAN-BASE |
| CLEAN-DOC | remove same doc-manager reactor; CLEAN-BASE |
| CLEAN-C15 | unregister payload DB reactor; unload payload only if exact unload contract permits; remove command/artifacts; CLEAN-DEFER |

Expected values are immutable contract constants. The mutation writer receives no authority to redefine them.

## 3. Context ContractIds

| ContractId | Definition |
|---|---|
| H-DB | exact OA-V28 `dbmain.h` database-reactor authority |
| H-OBJ | NEC-V29 `dbmain.h` object/entity-reactor exact 2025 authority |
| H-TX | exact OA-V28 `dbtrans.h` transaction-reactor authority |
| H-ED | exact OA-V28 `aced.h` editor-reactor authority |
| H-DOC | NEC-V29 `acdocman.h` document-manager-reactor authority |
| H-SEND | exact OA-V28 `acdocman.h::sendStringToExecute` authority |
| H-LOAD | exact OA-V28 `acedads.h` + `rxdlinkr.h` authority |
| T-PRIMARY | exact primary transaction id; top id logged at callback |
| T-FRESH | short explicit callback transaction, ended/aborted before callback return |
| T-DEFERRED | fresh transaction owned by delivered research command |
| T-ABORTING | primary T requested to abort; callback active count logged |
| LOCK-OBS | actual lock mode logged; no permission inferred |
| LOCK-COMMAND | exact scratch document command lock; actual mode logged |
| CTX-CALLBACK | native thread id, document/application context and database identity logged |
| CTX-COMMAND | delivered native research command context and thread logged |
| SP-IMMEDIATE | immediately before trigger operation/callback dispatch |
| SP-SEND | immediately before NS-SEND; enqueue `Acad::ErrorStatus` captured |
| EP-CALLBACK | first instruction of exact callback |
| EP-COMMAND | first instruction of exact queued research command |
| MARK-C | method return, N/M command will/end/cancel/fail, lock release, F_usable/C_decide/C_report/C_host |

## 4. Full normative matrix — retained V28 probes

```text
02N;N-DB-OPEN;NOT_APPLICABLE(no scheduler);H-DB;register scratch DB reactor, STATE-S-0;modify DB notifier A;T-PRIMARY;observed top T;LOCK-OBS;CTX-CALLBACK;MUT-S;S;SP-IMMEDIATE;EP-CALLBACK;STATE-S-0;STATE-S-1;VER-S+VER-T;N-DB-MOD;T2;P1/P2/P4;PASS-S and affiliation matches commit/abort;FAIL-S or escaped affiliation;UNKNOWN-COMMON;CLEAN-BASE
04N;N-TR-ABOUT-ABORT;NOT_APPLICABLE(no scheduler);H-TX;stage MUT-S and request primary abort;abort primary T;T-ABORTING;observed top T;LOCK-OBS;CTX-CALLBACK;controlled S marker on distinct target if legal;S;SP-IMMEDIATE;EP-CALLBACK;STATE-T-OPEN+STATE-S-0;STATE-T-ABORT;VER-T+VER-S;N-TR-ABORTED,N-ED-CANCEL,N-ED-FAIL;T2/T3/T4;P2/P3;exact callback order and no staged state survives;staged state survives or order contradicts authority;UNKNOWN-COMMON;CLEAN-BASE
09N-A;N-TR-ABOUT-END;NOT_APPLICABLE(no scheduler);H-TX;open nested T with STATE-S-0;end nested T while active count greater than 1;T-PRIMARY;outer T id;LOCK-OBS;CTX-CALLBACK;observation only;S/M;SP-IMMEDIATE;EP-CALLBACK;STATE-T-OPEN;declared nested visibility with outer still active;VER-T+VER-S;N-TR-ENDED;T2/T3/T4;P2/P3;pre-end callback,count,visibility match;count/order/visibility contradiction;UNKNOWN-COMMON;CLEAN-BASE
09N-B;N-TR-ABOUT-END;NOT_APPLICABLE(no scheduler);H-TX;outermost T with STATE-S-0;end T with active count equal 1;T-PRIMARY;same as primary;LOCK-OBS;CTX-CALLBACK;observation only;S/M;SP-IMMEDIATE;EP-CALLBACK;STATE-T-OPEN;final pre-end visibility recorded;VER-T+VER-S;N-TR-ENDED;T2/T4;P2/P3/P6;count=1 and final pre-end state/order match;invented callback or contrary count/order;UNKNOWN-COMMON;CLEAN-BASE
09N-C;N-TR-ENDED;NOT_APPLICABLE(no scheduler);H-TX;commit controlled T from STATE-S-0;end controlled T;ended T id;observed top T after end;LOCK-OBS;CTX-CALLBACK;observation plus fresh read when legal;S/M;SP-IMMEDIATE;EP-CALLBACK;STATE-T-OPEN;ended count excludes T and fresh DB state recorded;VER-T+VER-S;N-ED-END;T2/T4/T5/T6;P2/P3/P6;ended callback,count and reread agree;state/count/order contradicts contract;UNKNOWN-COMMON;CLEAN-BASE
09N-D;N-ED-END;NOT_APPLICABLE(no scheduler);H-ED;finish fixture command with STATE-S-0;native command-ended callback;T-FRESH;observed top T;LOCK-OBS;CTX-CALLBACK;MUT-S then end/abort T before return;S;SP-IMMEDIATE;EP-CALLBACK;STATE-S-0;actual committed or aborted result logged;VER-S+VER-T+MARK-C;N-TR-ENDED,managed CommandEnded,lock release;T2/T4/T6/T8;P6/P12;complete order and DB result match chosen T outcome;mutation/order violates required boundary;UNKNOWN-COMMON;CLEAN-BASE
10N-S;N-ED-END;NOT_APPLICABLE(no scheduler);H-ED;finish fixture command at STATE-S-0;native command-ended callback;T-FRESH;observed top T;LOCK-OBS;CTX-CALLBACK;MUT-S;S;SP-IMMEDIATE;EP-CALLBACK;STATE-S-0;STATE-S-1;VER-S+MARK-C;N-TR-ENDED,managed CommandEnded,lock release;T2/T8;P6/P12;PASS-S and candidate boundary classification recorded;mutation after accepted success or wrong order;UNKNOWN-COMMON;CLEAN-BASE
10N-M;N-ED-END;NOT_APPLICABLE(no scheduler);H-ED;finish fixture command at STATE-M-0;native command-ended callback;T-FRESH;observed top T;LOCK-OBS;CTX-CALLBACK;MUT-M;M;SP-IMMEDIATE;EP-CALLBACK;STATE-M-0;STATE-M-1;VER-M+MARK-C;N-TR-ENDED,managed CommandEnded,lock release;T2/T8/T12;P11/P12;PASS-M and candidate boundary classification recorded;mutation after accepted success or wrong order;UNKNOWN-COMMON;CLEAN-BASE
10N-SM;N-ED-END;NOT_APPLICABLE(no scheduler);H-ED;finish fixture command at STATE-SM-0;native command-ended callback;T-FRESH;observed top T;LOCK-OBS;CTX-CALLBACK;MUT-SM;SM;SP-IMMEDIATE;EP-CALLBACK;STATE-SM-0;STATE-SM-1;VER-SM+MARK-C;N-TR-ENDED,managed CommandEnded,lock release;T2/T8/T11;P6/P11/P12;PASS-SM and candidate boundary classification recorded;mutation after accepted success or wrong order;UNKNOWN-COMMON;CLEAN-BASE
16N-S;NS-SEND;N-DB-MOD;H-SEND+H-DB;register exact queued S command and STATE-S-0;N-DB-MOD calls NS-SEND;origin primary T;origin top T;LOCK-OBS;CTX-CALLBACK then CTX-COMMAND;MUT-S;S;SP-SEND;EP-COMMAND;STATE-S-0;STATE-S-1;VER-S+VER-T+MARK-C;origin return,N-ED-WILL,N-ED-END,lock release;T2/T8/T9/T16;P6/P12;enqueue accepted,command executes,DB/order exact;causal mutation violates accepted fence or state wrong;UNKNOWN-COMMON including nondelivery;CLEAN-DEFER
16N-M;NS-SEND;N-DB-MOD;H-SEND+H-DB;register exact queued M command and STATE-M-0;N-DB-MOD calls NS-SEND;origin primary T;origin top T;LOCK-OBS;CTX-CALLBACK then CTX-COMMAND;MUT-M;M;SP-SEND;EP-COMMAND;STATE-M-0;STATE-M-1;VER-M+VER-T+MARK-C;origin return,N-ED-WILL,N-ED-END,lock release;T2/T8/T12/T16;P11/P12;enqueue accepted,command executes,DB/order exact;causal mutation violates accepted fence or state wrong;UNKNOWN-COMMON including nondelivery;CLEAN-DEFER
16N-SM;NS-SEND;N-DB-MOD;H-SEND+H-DB;register exact queued SM command and STATE-SM-0;N-DB-MOD calls NS-SEND;origin primary T;origin top T;LOCK-OBS;CTX-CALLBACK then CTX-COMMAND;MUT-SM;SM;SP-SEND;EP-COMMAND;STATE-SM-0;STATE-SM-1;VER-SM+VER-T+MARK-C;origin return,N-ED-WILL,N-ED-END,lock release;T2/T8/T11/T16;P6/P11/P12;enqueue accepted,command executes,DB/order exact;causal mutation violates accepted fence or state wrong;UNKNOWN-COMMON including nondelivery;CLEAN-DEFER
C15N16N-SM;NS-SEND;N-DB-MOD;H-LOAD+H-SEND+H-DB;bootstrap loaded,payload path fixed,STATE-SM-0,call NL-ARX-PATH,observe N-RX-LOADED and exact N-DB-MOD registration;modify fixture notifier after registration;origin primary T;origin top T;LOCK-OBS;CTX-CALLBACK then CTX-COMMAND;MUT-SM;SM;SP-SEND after load/registration markers;EP-COMMAND;STATE-SM-0;STATE-SM-1;VER-SM+VER-T+MARK-C;NL-ARX-PATH result,N-RX-LOADED,registration proof,origin return,N-ED-WILL,N-ED-END;T2/T15/T16;P6/P11/P12;load,marker,registration,event,enqueue,execution,state,order,cleanup all exact;host lifecycle/safety contradiction or wrong DB state;UNKNOWN-COMMON plus missing load/registration/delivery/cleanup proof;CLEAN-C15
```

## 5. Full normative matrix — object/entity/doc reactor probes

```text
02NO-S;N-OBJ-OPEN;NOT_APPLICABLE(no scheduler);H-OBJ;attach OR-XR to semantic notifier A,STATE-S-0;modify notifier A to invoke assertWriteEnabled;T-PRIMARY;observed top T;LOCK-OBS;CTX-CALLBACK;MUT-S on distinct target B;S;SP-IMMEDIATE;EP-CALLBACK;STATE-S-0;STATE-S-1;VER-S+VER-T;N-OBJ-MOD,N-OBJ-CLOSED;T2-OBJ;P1/P2/P4;PASS-S with exact notifier/T affiliation;FAIL-S or write to const notifier attempted;UNKNOWN-COMMON;CLEAN-OBJ
02NO-M;N-OBJ-MOD;NOT_APPLICABLE(no scheduler);H-OBJ;attach OR-REF-A,STATE-M-0;modify and close F-REF-A;T-PRIMARY;observed top T;LOCK-OBS;CTX-CALLBACK;MUT-M on F-REF-B;M;SP-IMMEDIATE;EP-CALLBACK;STATE-M-0;STATE-M-1;VER-M+VER-T;N-OBJ-OPEN,N-OBJ-CLOSED,N-ENT-GFX;T2-OBJ;P1/P2/P11;PASS-M and callback order exact;FAIL-M or recursive notifier access;UNKNOWN-COMMON;CLEAN-OBJ
02NO-SM;N-OBJ-ERASE;NOT_APPLICABLE(no scheduler);H-OBJ;attach OR-REF-B,STATE-SM-0;erase F-REF-B;T-PRIMARY;observed top T;LOCK-OBS;CTX-CALLBACK;MUT-SM on distinct target/linkage;SM;SP-IMMEDIATE;EP-CALLBACK;STATE-SM-0;STATE-SM-1;VER-SM+VER-T;N-OBJ-CLOSED,N-DB-ERASE;T2-OBJ/T7/T11;P6/P9/P11;PASS-SM with erase flag/order exact;FAIL-SM or sibling/linkage mismatch;UNKNOWN-COMMON;CLEAN-OBJ
02NO-CLOSE-SM;N-OBJ-CLOSED;NOT_APPLICABLE(no scheduler);H-OBJ;attach OR-REF-A,STATE-SM-0;modify then close F-REF-A;T-PRIMARY;observed top T;LOCK-OBS;CTX-CALLBACK;MUT-SM on distinct B,C only;SM;SP-IMMEDIATE;EP-CALLBACK;STATE-SM-0;STATE-SM-1;VER-SM+VER-T;N-OBJ-MOD;T2-OBJ;P2/P6/P11;PASS-SM and same notifier reopen returns documented eWasNotifying if attempted observationally;same notifier reopened/mutation escapes expected T;UNKNOWN-COMMON;CLEAN-OBJ
04NO-S;N-OBJ-CANCEL;NOT_APPLICABLE(no scheduler);H-OBJ;attach OR-XR,stage semantic modification then cancel object open;cancel notifier open;T-ABORTING;observed top T;LOCK-OBS;CTX-CALLBACK;MUT-S on distinct target if legal;S;SP-IMMEDIATE;EP-CALLBACK;STATE-S-0;STATE-T-ABORT;VER-S+VER-T;N-OBJ-UNDO,N-OBJ-MOD,N-OBJ-CLOSED;T2-OBJ/T3;P2/P3;cancel order and no forbidden state survives;state survives contrary to transaction outcome;UNKNOWN-COMMON;CLEAN-OBJ
04NO-UNDO-S;N-OBJ-UNDO;NOT_APPLICABLE(no scheduler);H-OBJ;attach OR-XR,modify then cancel/controlled undo;close after undo/cancel;T-ABORTING;observed top T;LOCK-OBS;CTX-CALLBACK;MUT-S on distinct target if legal;S;SP-IMMEDIATE;EP-CALLBACK;STATE-S-0;STATE-T-ABORT;VER-S+VER-T;N-OBJ-CANCEL,N-OBJ-CLOSED;T2-OBJ/T3;P2/P3;undo callback/order and restored state exact;state/order contradicts undo outcome;UNKNOWN-COMMON;CLEAN-OBJ
02NE-M;N-ENT-GFX;NOT_APPLICABLE(no scheduler);H-OBJ;attach ER-REF-A,STATE-M-0;transform F-REF-A and close;T-PRIMARY;observed top T;LOCK-OBS;CTX-CALLBACK;MUT-M on F-REF-B;M;SP-IMMEDIATE;EP-CALLBACK;STATE-M-0;STATE-M-1;VER-M+VER-T;N-OBJ-MOD,N-OBJ-CLOSED;T2-ENTITY/T12;P2/P9/P11;PASS-M and graphics callback/order exact;FAIL-M or state/order mismatch;UNKNOWN-COMMON;CLEAN-OBJ
10NDOC-WILL-SM;N-DOC-LOCK-WILL;NOT_APPLICABLE(no scheduler);H-DOC;register doc reactor,STATE-SM-0;controlled scratch lock acquisition/release;observed callback T or NONE;observed top T or NONE;LOCK-OBS;CTX-CALLBACK;MUT-SM only after legal context/T acquisition;SM;SP-IMMEDIATE;EP-CALLBACK;STATE-SM-0;STATE-SM-1 or unchanged if illegal;VER-SM+VER-T+MARK-C;N-DOC-LOCK-CHANGED,N-DOC-LOCK-VETO;T2-DOC/T8;P6/P11/P12;callback/modes/order and legal mutation outcome exact;lock/lifecycle safety contradiction;UNKNOWN-COMMON;CLEAN-DOC
10NDOC-CHANGED-SM;N-DOC-LOCK-CHANGED;NOT_APPLICABLE(no scheduler);H-DOC;register doc reactor,STATE-SM-0;controlled scratch lock acquisition then release;observed callback T or NONE;observed top T or NONE;LOCK-OBS;CTX-CALLBACK;MUT-SM only after legal context/T acquisition;SM;SP-IMMEDIATE;EP-CALLBACK;STATE-SM-0;STATE-SM-1 or unchanged if illegal;VER-SM+VER-T+MARK-C;N-DOC-LOCK-WILL,N-DOC-LOCK-VETO;T2-DOC/T8;P6/P11/P12;previous/current/global modes and outcome exact;lock/lifecycle safety contradiction;UNKNOWN-COMMON;CLEAN-DOC
10NDOC-VETO-SM;N-DOC-LOCK-VETO;NOT_APPLICABLE(no scheduler);H-DOC;register observer plus controlled earlier veto reactor,STATE-SM-0;request lock and veto it;NONE expected;NONE expected;LOCK-OBS;CTX-CALLBACK;attempt MUT-SM only through separately acquired legal context,never vetoed lock;SM;SP-IMMEDIATE;EP-CALLBACK;STATE-SM-0;STATE-SM-0 unless separate legal T commits;VER-SM+MARK-C;N-DOC-LOCK-WILL,N-DOC-LOCK-CHANGED,N-ED-CANCEL;T2-DOC;P12;veto/order exact and no write attributed to vetoed lock;write succeeds using vetoed authority or order contradicts docs;UNKNOWN-COMMON;CLEAN-DOC
C2OBJ16N-S;NS-SEND;N-OBJ-MOD;H-OBJ+H-SEND;attach OR-REF-A,register queued S command,STATE-S-0;modify/close F-REF-A;origin primary T;origin top T;LOCK-OBS;CTX-CALLBACK then CTX-COMMAND;MUT-S;S;SP-SEND;EP-COMMAND;STATE-S-0;STATE-S-1;VER-S+VER-T+MARK-C;N-OBJ-CLOSED,origin return,N-ED-WILL,N-ED-END;T2-OBJ/T16;P6/P12;enqueue,delivery,state,order exact;causal write violates boundary or wrong state;UNKNOWN-COMMON including nondelivery;CLEAN-OBJ+CLEAN-DEFER
C2OBJ16N-M;NS-SEND;N-OBJ-MOD;H-OBJ+H-SEND;attach OR-REF-A,register queued M command,STATE-M-0;modify/close F-REF-A;origin primary T;origin top T;LOCK-OBS;CTX-CALLBACK then CTX-COMMAND;MUT-M;M;SP-SEND;EP-COMMAND;STATE-M-0;STATE-M-1;VER-M+VER-T+MARK-C;N-OBJ-CLOSED,origin return,N-ED-WILL,N-ED-END;T2-OBJ/T16;P11/P12;enqueue,delivery,state,order exact;causal write violates boundary or wrong state;UNKNOWN-COMMON including nondelivery;CLEAN-OBJ+CLEAN-DEFER
C2OBJ16N-SM;NS-SEND;N-OBJ-MOD;H-OBJ+H-SEND;attach OR-REF-A,register queued SM command,STATE-SM-0;modify/close F-REF-A;origin primary T;origin top T;LOCK-OBS;CTX-CALLBACK then CTX-COMMAND;MUT-SM;SM;SP-SEND;EP-COMMAND;STATE-SM-0;STATE-SM-1;VER-SM+VER-T+MARK-C;N-OBJ-CLOSED,origin return,N-ED-WILL,N-ED-END;T2-OBJ/T16;P6/P11/P12;enqueue,delivery,state,order exact;causal write violates boundary or wrong state;UNKNOWN-COMMON including nondelivery;CLEAN-OBJ+CLEAN-DEFER
C2ENT16N-M;NS-SEND;N-ENT-GFX;H-OBJ+H-SEND;attach ER-REF-A,register queued M command,STATE-M-0;transform/close F-REF-A;origin primary T;origin top T;LOCK-OBS;CTX-CALLBACK then CTX-COMMAND;MUT-M;M;SP-SEND;EP-COMMAND;STATE-M-0;STATE-M-1;VER-M+VER-T+MARK-C;N-OBJ-CLOSED,origin return,N-ED-WILL,N-ED-END;T2-ENTITY/T16;P11/P12;enqueue,delivery,state,order exact;causal write violates boundary or wrong state;UNKNOWN-COMMON including nondelivery;CLEAN-OBJ+CLEAN-DEFER
C2DOCW16N-SM;NS-SEND;N-DOC-LOCK-WILL;H-DOC+H-SEND;register doc reactor and queued SM command,STATE-SM-0;controlled lock change;origin T observed or NONE;origin top T observed or NONE;LOCK-OBS;CTX-CALLBACK then CTX-COMMAND;MUT-SM;SM;SP-SEND;EP-COMMAND;STATE-SM-0;STATE-SM-1;VER-SM+VER-T+MARK-C;N-DOC-LOCK-CHANGED,origin return,N-ED-WILL,N-ED-END;T2-DOC/T16;P6/P11/P12;enqueue,delivery,state,lock/order exact;causal write violates boundary or wrong state;UNKNOWN-COMMON including illegal enqueue/nondelivery;CLEAN-DOC+CLEAN-DEFER
C2DOCC16N-SM;NS-SEND;N-DOC-LOCK-CHANGED;H-DOC+H-SEND;register doc reactor and queued SM command,STATE-SM-0;controlled lock acquisition/release;origin T observed or NONE;origin top T observed or NONE;LOCK-OBS;CTX-CALLBACK then CTX-COMMAND;MUT-SM;SM;SP-SEND;EP-COMMAND;STATE-SM-0;STATE-SM-1;VER-SM+VER-T+MARK-C;N-DOC-LOCK-WILL,origin return,N-ED-WILL,N-ED-END;T2-DOC/T16;P6/P11/P12;enqueue,delivery,state,lock/order exact;causal write violates boundary or wrong state;UNKNOWN-COMMON including illegal enqueue/nondelivery;CLEAN-DOC+CLEAN-DEFER
```

## 6. Mechanical closure checks

```text
Concrete ProbeId count = 29
Unique ProbeId count = 29
Rows with zero primary authority = 0
Rows with multiple primary authorities = 0
Blank schema cells = 0
Normative wildcard/range rows = 0
Result state for every row = DEFINED / NOT EXECUTED
```

`09N`, `10N*`, `16N*`, `02NO-*` and similar strings may appear only in descriptive family prose. They are not
ProbeIds and cannot satisfy closure.
