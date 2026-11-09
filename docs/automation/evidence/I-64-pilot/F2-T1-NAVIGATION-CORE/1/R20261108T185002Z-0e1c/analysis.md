TaskId: F2-T1-NAVIGATION-CORE
Phase: CONTROLLER_VERIFICATION
RunId:
R20261108T185002Z-0e1c
Reported by the Controller:
EXECUTION_BLOCKED / STOP, FailureClass StopCondition, TriggeredStopConditions [S-04]
Coordinator classification:
INVALID_OUTPUT (coordinator/coordinator-f2t1-verification-0e1c-disposition.md)
Label of R20261108T185002Z-0e1c/output.json (SHA-256
E85C2ECD80CF6AEFFC607C62EEE9BB40F59CE7049123B3FABAB77C7D9594DB68):
SCHEMA_VALID_BUT_MATERIALLY_FALSE
INVALID_OUTPUT
The JSON is kept unchanged as historical evidence. The relay record of the run
is also kept unchanged; its Disposition STOP was the session's relay
disposition before the Coordinator decision.
Failure:
0e1c counted display testName rather than VSTest FullyQualifiedName filter
semantics.
Correct semantic rule:
FullyQualifiedName~Workspace means case-insensitive substring matching on the
fully qualified class/method identity represented by UnitTest/TestMethod,
not display-name argument text.
Measured result from the same TRX inputs (session-count-analysis.json):
GREEN 724/724/0
RED 724/673/51.
The Controller's 726 = display testName containing "Workspace", case-sensitive:
+3 unrelated Theory cases matched only by their argument text, -1 selected test
whose class/method contains uppercase "WORKSPACE".
Classification:
previous S-04 was a Controller counting error; no contradictory product
evidence exists.
Scope of this record:
It does not modify product code, tests, delegation, contract, handoff, or the
RED and GREEN commits. Worker delivery unchanged: RED f2761979, GREEN c726410f.
No Worker correction; no attempt consumed (attempts 1 / 3).
Recovery route (per the Coordinator disposition):
AUTOMATION_PLAN 16.4 step 5 forbids any session Git write until the
verification and the negative controls finish, and a documentation commit
would also move HEAD away from CurrentSha c726410f, which the Identity check
requires. The analysis is therefore NOT versioned before the rerun and the
verification prompt is NOT modified. The rerun uses the SAME prompt
(same generator; only the RunId and the inputs path change, as for any rerun
under 16.6). The corrected counting rule reaches the Controller through the
existing input inputs/coordinator-disposition.md, which for the rerun holds
the literal pre-planning stop disposition followed by the literal 0e1c
disposition, separated by one line "=====". All other inputs are byte-equal
to the 0e1c inputs. This record is custodied with the post-control session
commit (16.4 step 7).
Launcher option:
The 0e1c command lines in events.jsonl are "pwsh.exe -Command ..." without
-NoProfile, so the user profile loads (the CLI default allows a login shell).
For the rerun the launcher passes the per-invocation override
-c allow_login_shell=false (a key of the CLI config schema; a command-line
override, not persisted; config.toml unchanged) so that commands run as a
non-login shell. Whether -NoProfile was effective is read afterwards from the
command lines in events.jsonl and recorded in the relay record.
Budget:
verification reruns for BLOCKED/transport/INVALID_OUTPUT: 1 / 2 after launch.
If the rerun reports the same false 726 contradiction: STOP to Coordinator,
without spending the second rerun.
