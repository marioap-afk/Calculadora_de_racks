"""Sustituye las líneas 1-34 (cabecera de la docstring) y la línea AUDITOR_VERSION de kit/post-review.py (copia del auditor v5.4-a6r4 del kit
acreditado de A-6 ronda 4, SHA-256 6c51758a…) por la cabecera del auditor v5.5-a7. Exige que la copia de partida sea byte a byte la del kit acreditado.
Uso: python -I -B adapt_post_review_head.py <kit dir>
"""
import hashlib
import sys

BASE_SHA = "6c51758aaa9bc981f5f733e9fa9946d5402f8a9dbcbc543e580bdd1bab4b68cc"
HEAD = '''"""I-62, decisiones §93: post-review audit v5.5-a7 of ONE claude-cli run of the ONE FORMAL REVIEW of amendment A-7
(docs/initiatives/I-62-A-7.md, blob pinned in the closure and in owner_gate.py, PROPUESTA round 1), bounded to A7-1 and its consequences on the
VERIFY input closure of FX-02/F6 and the A6-7 measurement, by an independent ARCHITECT. Declared and self-tested BEFORE the launch (Owner
CLAUDE-CLI-I62 = A; the launch also requires gate 11, owner_gate.py: the Owner line of §78, the A-7 blob and the agreed A-6 blob at the worktree
HEAD and the Coordinator disposition §93 (after §92) pinned in owner_gate.py, with its number, heading, full blob and text SHA-256, and no later
section); the principal session runs it afterwards, never before, on the complete run, and the auditor is not corrected after the run.

v5.5-a7 = the v5.4-a6r4 auditor of A-6 round 4 (R20261014T180556Z-5e02, ACCREDITED, decisions §89-§90): every launch, transport-gate, stream-json,
transcript, tool, read-fidelity, reference, direct-read, run-custody, clone, run.json (Outcome, OutcomeCategories, PostRunErrors, fields),
FrozenCommandGuard, RecordProbe, Environment and `Categories` rule is kept byte for byte (the reviewer has ONLY Read, Grep and Glob, plus
StructuredOutput; effective paths; read fidelity line by line against the commit; failed reads never credited; prompt.md never a reference; run
custody; clone clean after the run; the transport gate D-01 re-evaluated on the copies launch.py wrote; preflight.json and run.json audited; the
execution category and the Architect verdict reported separately in Categories). The run has two files (order.txt, a premise, and prompt.md); the
direct read is V14 B.9 L2231-L2233 (decisions §93, point 3: «A7-1 frente a V14 B.9»), taken from closure.json like before. Changes:
  * preflight.json, gate 11 (A-7): the record carries ObjectBlob = the closure's object blob, ObjectBlobAtHeadMatches and A6AgreedBlobAtHeadMatches
    true, AuthorizationPending = false and the authorization pinned in the kit's owner_gate.py (§93 after §92; number, heading, full blob, text
    SHA-256, exactly one section, no later section) and the kit RunId; the rule of round 4 otherwise;
  * schema coherence (rackcad-a7-architect-review/0): ids unique in TopicVerdicts, Materiality.Triggers, RelatedImpacts.FailClosedProposals and
    QuestionAnswers and, with a verdict, exactly the eleven topics of decisions §93, point 3, M-01..M-08, the eight FC proposals of §93, point 5,
    and Q-A7-01..06 once each; a topic DOES_NOT_CONFORM => a linked REQUIRED, CONFORMS => none; a trigger DISAGREE => a linked REQUIRED, AGREE =>
    none, and A7Declared = the A-7 §5 table of the closure (MaterialityA7); OwnerDecisionRequired true <=> UnidentifiedConsequences non-empty;
    RelatedImpacts (decisions §93, points 4 and 5) are separate: RevealsA7Defect true => a linked REQUIRED, false => none; ContractExclusionD1
    CONCERNS_FOR_COORDINATOR <=> Concerns non-empty; an FC proposal CONCERNS_FOR_COORDINATOR <=> one of ChangesPassCriteria,
    ExcludesValidC24Cases, ReliesOnImplementationCoincidence true; a question NOT_SATISFIED => a linked finding; with a verdict, ClauseRefs in every
    element (nested ones too), no NOT_DETERMINED and no null in ContractExclusionD1 or OwnerDecisionRequired, every FindingIds entry exists and every
    REQUIRED is cited by at least one element; AGREED => no REQUIRED, no Owner decision pending, IfAgreed all true, every topic CONFORMS, every
    trigger AGREE and RevealsA7Defect false everywhere (concerns of the related impacts that reveal no A-7 defect do not block it); CHANGES
    REQUIRED or REJECTED => at least one REQUIRED;
  * kept from v5.4: findings (ids A62-A7-R<n> / A62-A7-O<n>), NOT_ACCREDITED, BLOCKED — OWNER DECISION, IfAgreed, invocation ids, reviewed object,
    Step0 (seven booleans, now the A-7 Step 0), validate_schema (Test-Json AND schema_errors) and the direct read rule.'''
VERSION = ('AUDITOR_VERSION = "v5.5-a7 (claude-cli; Read/Grep/Glob only; transport gate D-01; Owner and authorization gate 11 on the Coordinator '
           'disposition §93 after §92 pinned in owner_gate.py; run.json Outcome and PostRunErrors; frozen command and record probe; direct read of V14 '
           'L2231-L2233; declared and self-tested before the launch; decisions §93)"')

p = sys.argv[1] + "/post-review.py"
b = open(p, "rb").read()
assert hashlib.sha256(b).hexdigest() == BASE_SHA, "post-review.py no es la copia del kit acreditado de A-6 ronda 4"
L = b.decode("utf-8").split("\n")
assert L[34].startswith("Small seams") and L[78].startswith("AUDITOR_VERSION = "), (L[34][:20], L[78][:20])
L = HEAD.split("\n") + L[34:78] + [VERSION] + L[79:]
open(p, "w", encoding="utf-8", newline="\n").write("\n".join(L))
print("ok", len(L))
