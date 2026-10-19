"""Deriva kit/selftest-post-review.py del self-test del kit acreditado de A-6 ronda 4 (SHA-256 3ae15c63…), por regiones delimitadas por marcadores
únicos y por sustituciones exactas (cada una, una sola coincidencia). Conserva byte a byte los casos de lanzamiento, transporte, lectura, custodia,
run.json, preflight.json, compuerta D-01 (G00-G58) y coherencia lanzador-auditor (X01-X04); sustituye la docstring, las constantes, las lecturas y el
resultado base, los casos de coherencia del esquema (rackcad-a7-architect-review/0), los del registro de la compuerta 11 y los de owner_gate.evaluate
(§93 fijada, base §92) y la sonda L00.
Uso: python -I -B adapt_selftest.py <kit dir>
"""
import hashlib
import sys

BASE_SHA = "3ae15c6364f23290b66c52ebe2fd2a5a83243c5f1cc0081d968388f857fbab8e"
p = sys.argv[1] + "/selftest-post-review.py"
b = open(p, "rb").read()
assert hashlib.sha256(b).hexdigest() == BASE_SHA, "selftest-post-review.py no es la copia del kit acreditado de A-6 ronda 4"
s = b.decode("utf-8")


def region(start_marker, end_marker, new, include_end=False):
    global s
    assert s.count(start_marker) == 1, ("inicio", start_marker[:70], s.count(start_marker))
    assert s.count(end_marker) == 1, ("fin", end_marker[:70], s.count(end_marker))
    a = s.index(start_marker)
    e = s.index(end_marker) + (len(end_marker) if include_end else 0)
    assert a < e, start_marker[:40]
    s = s[:a] + new + s[e:]


def subst(old, new):
    global s
    assert s.count(old) == 1, ("sustitución", old[:70], s.count(old))
    s = s.replace(old, new)


DOC = '''"""Self-test of post-review.py v5.5-a7, of transport_gate.py, of owner_gate.py (A-7), of pin_authorization.values and of c3_analysis.analyze, run
BEFORE the launch of the ONE formal review of amendment A-7, bounded to A7-1 and its consequences on the VERIFY input closure of FX-02/F6 and the
A6-7 measurement (decisions §93). Adapted from the self-test of the accredited kit of A-6 round 4 (R20261014T180556Z-5e02, prep/adapt_selftest.py):
the same synthetic formats and the same transport, read, custody, launcher and gate cases (00-55, 126-149, 152-153, G00-G58, X01-X04), the specifics
replaced by the A-7 closure (two run files, order.txt = the §93 body, and prompt.md; the object is A-7; the direct read is V14 L2231-L2233) and the
result schema rackcad-a7-architect-review/0, with:
  * cases 56-121 for the coherence and coverage rules of the A-7 schema (TopicVerdicts, Materiality with its Triggers and A7Declared,
    OwnerReservedConsequence, RelatedImpacts with ContractExclusionD1 and FailClosedProposals, QuestionAnswers, AGREED, CHANGES REQUIRED, REJECTED,
    BLOCKED, NOT_ACCREDITED, schema; 108b-108g are nested schema violations that only the auditor's in-process schema check detects, because the
    Test-Json of pwsh 7.6.7 validates only the first level of nesting), the direct read of V14 L2231-L2233 and the references of the nested elements;
  * cases 150-151p for the preflight record of gate 11 of A-7: the object blob and the agreed A-6 blob at the worktree HEAD and the authorization
    pinned in owner_gate.py (the Coordinator disposition §93, after §92); the audit cases use the kit's own pins (a SYNTHETIC §94 appended to the
    clone's decisions only if the kit's pins were pending); case 151p checks that a kit with the authorization pending cannot be accredited;
  * cases O00-O30 (owner_gate.evaluate, pure, on decision texts built from the clone's decisions, which end at §93: the decisions before §93 play
    the clone's text where a case isolates the authorization rule): the authorization pending (blocked with the single «authorization pending: …»
    reason), pinned on §93 (passes), a later section (blocked), the object and agreed A-6 blobs and the inherited checks; O30 exercises
    pin_authorization.values; and the live probe L00 (owner_gate.check on the real I-62 worktree, read-only git, with the module exactly as the kit
    holds it: pinned, it must pass with exactly §93 and no later section; pending, it must block with only the pending reason);
  * cases X01-X04: launch.py and the auditor agree (RUN_JSON_FIELDS, outcome_categories on a set of records, the frozen command and the environment
    prefixes); launch.py is read with ast, never imported (importing it would evaluate its frozen-command guard).

Environment. By default it audits against the kit's closure (kit/closure.json: clone = closure Transport.Cwd, run = closure RunDir): the real
clone D:\\\\r62-arch-a7 and run D:\\\\r62-arch-a7-run. With RACKCAD_A7_KITDATA=<dir> it takes closure.json, the gate modules and the kit copies of
the run files from <dir>; post-review.py and result.schema.json always come from this kit directory.
Synthetic claude-cli runs (transcript JSONL in the format measured by the characterization C1, stream-json stdout, and the <run>/launch files
launch.py writes: run.json, preflight.json, stderr.txt and the copies of the transport-gate evidence). Each case audits against a SYNTHETIC gate
(MEASURED by default, built from a c3_analysis.analyze record of a synthetic PASS probe; ACCEPTED in case 41) set through the auditor's GATE seam;
LAUNCH_DIR points to the case's temporary launch directory. The synthetic run.json gets its Outcome and OutcomeCategories from launch.py's own
outcome_categories (read with ast) unless the case sets them, so every case also checks that the auditor's recomputation agrees with the launcher.

Every temporary file lives under <kit>/../.tmp/st and is removed at the end (with a guard against links). The only write outside it is the
temporary junction of case 03 inside the clone (target: a directory of its own under .tmp/st), removed in `finally`; the clone must end clean and
the run files identical to the kit copies.
Usage: python -B selftest-post-review.py <out.json>
"""
'''
region('"""Self-test of post-review.py v5.4-a6r4', "import ast\n", DOC)

CONST = '''HERE = os.path.dirname(os.path.abspath(__file__))
KITDATA = os.environ.get("RACKCAD_A7_KITDATA") or HERE
TMPROOT = os.path.join(os.path.dirname(HERE), ".tmp", "st")
os.makedirs(os.path.join(TMPROOT, "tmp"), exist_ok=True)
for k in ("TEMP", "TMP", "TMPDIR"):
    os.environ[k] = os.path.join(TMPROOT, "tmp")
tempfile.tempdir = os.path.join(TMPROOT, "tmp")
_CLOSURE = json.load(open(os.path.join(KITDATA, "closure.json"), encoding="utf-8"))
CLONE, RUN = _CLOSURE["Transport"]["Cwd"], _CLOSURE["RunDir"]
OTHER_CLONE = os.path.join(TMPROOT, "o")   # another tree: only ever a junction target or a path that must be classified OUTSIDE
os.makedirs(os.path.join(OTHER_CLONE, "docs", "initiatives"), exist_ok=True)
open(os.path.join(OTHER_CLONE, "docs", "initiatives", "I-62-A-3.md"), "w").write("# x\\n")
WORKTREE = os.path.join(os.environ.get("USERPROFILE", ""), ".codex", "worktrees", "architecture-portabilidad-coordinador-principal")   # L00, read-only
DEC = "docs/automation/decisions/I-62.md"
AP = "docs/AUTOMATION_PLAN.md"
LC = "docs/INITIATIVE_LIFECYCLE.md"
V14 = "docs/initiatives/I-62-proposal-v14.md"
A5 = "docs/initiatives/I-62-A-5.md"
A6 = "docs/initiatives/I-62-A-6.md"
A7 = _CLOSURE["ObjectPath"]
ADR46 = "docs/adr/0046-protocolo-de-ejecucion-delegada-de-agentes.md"
ADR48 = "docs/adr/0048-ejecucion-delegada-portable-roles-binding-y-autoverificacion.md"
EVID = "docs/automation/evidence/I-62-A7"
GRR, SUMMARY, PREFLIGHT_MD = EVID + "/a7-guards-result-run.json", EVID + "/summary.json", EVID + "/preflight.md"
PKG = "docs/automation/evidence/I-62-F6/FX-02/bmin"
SPEC = PKG + "/SPEC.md"
ACC = _CLOSURE["AcceptorPath"]
TOOLF = PKG + "/tool/bmin_facts.py"
OTHER_A7_BLOB = "a2aed3cc894037d6b42d462fbec9d6d0c7f629a6"   # un borrador de A-7 anterior al preflight (preflight.md): nunca el objeto
OTHER_A6_BLOB = "e62f27b8b1329f1b9cfa69a707feb31d22bad1d1"   # la A-6 de la ronda 3: nunca la acordada
C3_ADD = r"D:\\r62-c3-probe\\add\\canary-add.txt"
C3_OUT = r"D:\\r62-c3-probe\\out\\canary-out.txt"
DELETE = object()
CLONE_MUTATED = False     # True while the junction of case 03 exists (the mutation harness bypasses its caches then)
BLOCKED = "BLOCKED " + chr(0x2014) + " OWNER DECISION"
IFAGREED_KEYS = ("ScopeLimitedToA71AndConsequences", "A71CompatibleWithAuthorities", "AllMandatoryTopicsConform", "MaterialityConfirmed",
                 "NoUnidentifiedOwnerReservedConsequence", "NoUnauthorizedChanges", "RelatedImpactsRevealNoA7Defect", "SuitableForCoordinatorAgreement")
V14_READ = (2223, 2241)                       # the direct read of Step 0 (L2231-L2233 inside)
AUTH_SECTION = 93                             # the Coordinator disposition pinned in the kit (decisions §93, after §92)
SYNTH_AUTH_SECTION = 94                       # synthetic disposition, only if the kit's pins were pending (never in the kit as delivered)
USER_RX = re.compile(r'(?i)[a-z]:(?:\\\\\\\\|\\\\|/)+users(?:\\\\\\\\|\\\\|/)+[^\\\\/"\\s]+')
'''
region('HERE = os.path.dirname(os.path.abspath(__file__))\n', "\n\ndef load(name, path):", CONST)
subst('pr = load("post_review_v5_4_a6r4", path or os.path.join(HERE, "post-review.py"))',
      'pr = load("post_review_v5_5_a7", path or os.path.join(HERE, "post-review.py"))')
subst('''    """Lines of a SYNTHETIC Coordinator disposition after §85 (not a real disposition), appended to the clone's decisions by the self-test."""
    return ["## %d. Disposición del Coordinator: autorización de la revisión formal de A-6 ronda 4 (selftest)" % number, "",''',
      '''    """Lines of a SYNTHETIC Coordinator disposition after §93 (not a real disposition), appended to the clone's decisions by the self-test."""
    return ["## %d. Disposición del Coordinator: autorización de la revisión formal de A-7 (selftest)" % number, "",''')
subst('''    """While the kit's owner_gate.py holds the authorization pending, the audit cases need a pinned authorization: SYNTHETIC pins (§86 appended to
    the clone's decisions; values from pin_authorization.values) are set on the auditor's owner_gate module object (never on the file). With the
    kit's pins set, they are used as they are."""''',
      '''    """While the kit's owner_gate.py held the authorization pending, the audit cases would need a pinned authorization: SYNTHETIC pins (§94 appended
    to the clone's decisions; values from pin_authorization.values) set on the auditor's owner_gate module object (never on the file). With the
    kit's pins set (§93, as delivered), they are used as they are."""''')
subst('PINS = {"Source": "SYNTHETIC (owner_gate.py del kit con la autorización pendiente; §86 sintética añadida a las decisiones del clon)",',
      'PINS = {"Source": "SYNTHETIC (owner_gate.py del kit con la autorización pendiente; §94 sintética añadida a las decisiones del clon)",')

G11 = '''def gate11_record():
    """The preflight record of gate 11 that launch.py writes when the gate passes (owner_gate.check, A-7: the object and agreed A-6 blobs at the
    worktree HEAD, the pinned authorization §93 after §92, exactly one such section and no later section)."""
    return {"Ok": True, "Problems": [], "A6LineSha256": OG.A6_LINE_SHA, "A6LineNumbers": [1572], "ClaudeCliLineSha256": OG.CLAUDE_LINE_SHA,
            "ClaudeCliLineNumbers": [1085], "ObjectBlob": PR.OBJECT_BLOB, "ObjectBlobAtHead": PR.OBJECT_BLOB, "ObjectBlobAtHeadMatches": True,
            "A6AgreedBlob": OG.A6_AGREED_BLOB, "A6BlobAtHead": OG.A6_AGREED_BLOB, "A6AgreedBlobAtHeadMatches": True,
            "RunId": PR.IDS["RunId"], "BaseSection": OG.BASE_SECTION, "AuthorizationPending": False, "Authorized": True,
            "AuthorizationSection": OG.AUTH_SECTION, "AuthorizationHeading": OG.AUTH_HEADING, "AuthorizationTextSha256": OG.AUTH_TEXT_SHA,
            "AuthorizationSections": [{"Number": OG.AUTH_SECTION, "Line": 1825, "Heading": OG.AUTH_HEADING, "HeadingMatches": True, "HasBlob": True,
                                       "TextSha256": OG.AUTH_TEXT_SHA, "TextSha256Matches": True}],
            "LaterSections": [], "SectionsAfterBase": [{"Number": OG.AUTH_SECTION, "Line": 1825, "Heading": OG.AUTH_HEADING}]}


'''
region("def gate11_record():\n", "def frozen_guard_record():\n", G11)
subst("# ------------------------------------------------------------------ synthetic run (the round-3 one, with the round-4 record of gate 11)",
      "# ------------------------------------------------------------------ synthetic run (the round-3 one, with the A-7 record of gate 11)")

READS = '''# ------------------------------------------------------------------ the A-7 reads and the base result
def read_range(r, rel, a, b):
    r.call("Read", {"file_path": absp(rel), "offset": a, "limit": b - a + 1}, numbered(lines_of(rel)[a - 1:b], a))


def read_run_range(r, name, a, b):
    r.call("Read", {"file_path": RUN + "\\\\" + name, "offset": a, "limit": b - a + 1}, numbered(lines_of(run=name)[a - 1:b], a))


def v14_direct(r):
    read_range(r, V14, *V14_READ)


def step0(r, direct=True):
    if direct:
        v14_direct(r)
    r.call("Read", {"file_path": RUN + "\\\\order.txt"}, numbered(lines_of(run="order.txt")))
    read_range(r, DEC, *closure_range(DEC, "## 93. "))
    read_range(r, DEC, *closure_range(DEC, "§90, acuerdo registrado"))
    r.call("Read", {"file_path": absp(GRR)}, numbered(lines_of(GRR)))
    r.call("Read", {"file_path": absp(SUMMARY)}, numbered(lines_of(SUMMARY)))


def a7_reads(r):
    r.call("Read", {"file_path": absp(A7)}, numbered(lines_of(A7)))


def evidence_reads(r):
    read_range(r, PREFLIGHT_MD, 1, 60)


def acceptor_reads(r):
    read_range(r, ACC, 75, 90)
    read_range(r, ACC, 2790, len(lines_of(ACC)))


def a6_reads(r):
    read_range(r, A6, 500, 799)


def base_calls(r, direct=True):
    r.header()
    step0(r, direct)
    a7_reads(r)
    evidence_reads(r)
    acceptor_reads(r)
    a6_reads(r)


def stop_calls(r):
    r.header()
    step0(r)


def schema_keys(prop):
    schema = json.load(open(SCHEMA, encoding="utf-8"))
    return [k for k in schema["properties"][prop]["required"]]


def sec_ref(prefix):
    return ref(A7, *closure_range(A7, prefix))


def a7_ref(label):
    return ref(A7, *closure_range(A7, label))


def preflight_ref():
    return ref(PREFLIGHT_MD, 1, 3)


TOPIC_ROWS = {"T01_A71_VS_AUTHORITIES": "§3.2, A7-1 (texto de la regla)", "T02_CATEGORIES": "A7-1, punto 1", "T03_PROVENANCE_REVISION_IDENTITY_FIDELITY":
              "A7-1, punto 2", "T04_EFF_EFF1_B1_MAP_VALIDATION": "§3.1, Más allá de R7-A", "T05_MAP_INVALID_ABSENT_NONCONSTRUCTIBLE": "A7-1, punto 5",
              "T06_CLAUSEMAP_COVERAGE_RULE": "§3.5, residuo declarado (clausemap)", "T07_MULTI_CATEGORY_ENTRIES": "A7-1, punto 1",
              "T08_COMPATIBILITY_A5_A6": "§6, A-6", "T09_NO_UNAUTHORIZED_CHANGES": "A7-1, punto 7",
              "T10_APPLICABILITY_N4_NC1_N5_N7": "§3.4, A6-7, reglas 5 y 7", "T11_NEGATIVES_AND_PREFLIGHT": "§6, Guardas y preflight"}


def base_result():
    ids = PR.IDS
    return {
        "Schema": "rackcad-a7-architect-review/0", "RunId": ids["RunId"], "InvocationId": ids["InvocationId"],
        "LogicalReviewRequestId": ids["LogicalReviewRequestId"], "AttemptSeq": ids["AttemptSeq"], "RequestedRole": "ARCHITECT",
        "Action": "REVIEW_DESIGN", "ReviewedUnit": "I-62", "ReviewedCommit": PR.REV, "ReviewedPath": PR.OBJECT, "ReviewedBlob": PR.OBJECT_BLOB,
        "ReviewerMode": "SEPARATE SESSION", "SamePersonAsAuthor": "selftest",
        "ReviewerDeclaredIdentity": {"Runtime": "x", "Model": "x", "Effort": "x", "SessionOrThread": "x"},
        "InjectedContextDeclaration": {"State": "DECLARED", "Items": ["x"]},
        "Step0": dict([("Basis", "INVOKER_VERIFIED_CLONE")] + [(k, True) for k in schema_keys("Step0") if k not in ("Basis", "Note")] + [("Note", "x")]),
        "DirectReads": [{"Ref": ref(V14, 2231, 2233), "Purpose": "x"}],
        "Reads": ["x"], "Verdict": "AGREED", "RequiredFindings": [], "OptionalFindings": [],
        "TopicVerdicts": [{"Topic": t, "Status": "CONFORMS", "Rationale": "x", "ClauseRefs": [a7_ref(TOPIC_ROWS[t])], "FindingIds": []}
                          for t in PR.TOPICS],
        "Materiality": {"Triggers": [{"Trigger": m, "A7Declared": PR.MAT_A7[m], "Assessment": "AGREE", "Rationale": "x",
                                      "ClauseRefs": [a7_ref("§5, %s" % m)], "FindingIds": []} for m in PR.TRIGGERS],
                        "Rationale": "x", "ClauseRefs": [sec_ref("## 5. ")], "FindingIds": []},
        "OwnerReservedConsequence": {"OwnerDecisionRequired": False, "UnidentifiedConsequences": [], "Rationale": "x",
                                     "ClauseRefs": [a7_ref("§7, consecuencias reservadas al Owner")], "FindingIds": []},
        "RelatedImpacts": {"ContractExclusionD1": {"Status": "NO_CONCERN", "RevealsA7Defect": False, "Concerns": [], "Rationale": "x",
                                                   "ClauseRefs": [a7_ref("§3.5, lo que A-7 no hace")], "FindingIds": []},
                           "FailClosedProposals": [{"Proposal": f, "ChangesPassCriteria": False, "ExcludesValidC24Cases": False,
                                                    "ReliesOnImplementationCoincidence": False, "RevealsA7Defect": False, "Status": "NO_CONCERN",
                                                    "Rationale": "x", "ClauseRefs": [preflight_ref()], "FindingIds": []} for f in PR.FCS],
                           "Note": "x"},
        "QuestionAnswers": [{"Question": q, "Outcome": "SATISFIED", "Answer": "x", "ClauseRefs": [a7_ref("§8, %s" % q)], "FindingIds": []}
                            for q in PR.QUESTIONS],
        "IfAgreed": {k: True for k in IFAGREED_KEYS},
        "KnownLimitations": [], "RecommendedNextAction": "x"}


def tvd(res):
    return {d["Topic"]: d for d in res["TopicVerdicts"]}


def trd(res):
    return {d["Trigger"]: d for d in res["Materiality"]["Triggers"]}


def fcd(res):
    return {d["Proposal"]: d for d in res["RelatedImpacts"]["FailClosedProposals"]}


def qad(res):
    return {d["Question"]: d for d in res["QuestionAnswers"]}


def mat(res):
    return res["Materiality"]


def orc(res):
    return res["OwnerReservedConsequence"]


def d1(res):
    return res["RelatedImpacts"]["ContractExclusionD1"]


def req_finding(fid="A62-A7-R1", evidence=None):
    return {"FindingId": fid, "Section": "§3.2 A7-1, punto 2", "ViolatedInvariant": "x", "Counterexample": "",
            "Evidence": evidence or [a7_ref("A7-1, punto 2")], "WhyItMatters": "x", "CorrectionRequired": "x"}


def opt_finding(fid="A62-A7-O1"):
    return {"FindingId": fid, "Section": "§8", "Note": "x", "Evidence": [a7_ref("§8, Q-A7-01")], "SuggestedCorrection": "x"}


def not_agreed(res, verdict):
    res["Verdict"] = verdict
    res["IfAgreed"] = {k: None for k in IFAGREED_KEYS}


def changes_required(res, verdict="CHANGES REQUIRED"):
    """A coherent CHANGES REQUIRED (or REJECTED) skeleton: one REQUIRED that leaves T02_CATEGORIES in DOES_NOT_CONFORM."""
    not_agreed(res, verdict)
    res["RequiredFindings"] = [req_finding()]
    tvd(res)["T02_CATEGORIES"].update({"Status": "DOES_NOT_CONFORM", "FindingIds": ["A62-A7-R1"]})


def blocked(res):
    """A coherent BLOCKED — OWNER DECISION skeleton."""
    not_agreed(res, BLOCKED)
    orc(res).update({"OwnerDecisionRequired": True, "UnidentifiedConsequences": ["x"]})


'''
region("# ------------------------------------------------------------------ the round-4 reads and the base result\n", "def mklink(link, target):\n", READS)

# generic cases 06 and 08: the order text and the clone path of A-7
subst('r.call("Grep", {"pattern": "nueva disposición", "path": RUN + "\\\\order.txt", "output_mode": "content", "-n": True}, "49:" + order[48])',
      'r.call("Grep", {"pattern": "sin reintento", "path": RUN + "\\\\order.txt", "output_mode": "content", "-n": True}, "54:" + order[53])')
subst('r.call("Bash", {"command": "git -C D:/r62-arch-a6r4 rev-parse HEAD"}', 'r.call("Bash", {"command": "git -C D:/r62-arch-a7 rev-parse HEAD"}')

MID = '''    a48, b48 = closure_range(ADR48, "Decisión, punto 6")

    def ref_unread(res, r):
        mat(res)["ClauseRefs"].append(ref(ADR48, a48, b48))
    add(variant("14-referencia-sobre-lineas-no-leidas", expected=["referencia sobre líneas no entregadas fielmente en Materiality (%s)"
                                                                  % ref(ADR48, a48, b48)], result_patch=ref_unread))

    def ref_bad(res, r):
        tvd(res)["T01_A71_VS_AUTHORITIES"]["ClauseRefs"] += ["prompt.md:1", "order.txt:99"]
    add(variant("15-referencia-a-prompt-o-fuera-de-rango", expected=["referencia no canónica en TopicVerdicts[T01_A71_VS_AUTHORITIES] (prompt.md:1)",
                                                                     "referencia no canónica en TopicVerdicts[T01_A71_VS_AUTHORITIES] (order.txt:99)"],
                result_patch=ref_bad))

    def stop(res, r):
        nds = {"Rationale": "no evaluado", "ClauseRefs": [], "FindingIds": []}
        res.update({"Verdict": "NOT_ACCREDITED", "Reads": [RUN + "\\\\order.txt"], "RequiredFindings": [], "OptionalFindings": [],
                    "TopicVerdicts": [], "QuestionAnswers": [],
                    "Materiality": dict(nds, Triggers=[]),
                    "OwnerReservedConsequence": dict(nds, OwnerDecisionRequired=None, UnidentifiedConsequences=[]),
                    "RelatedImpacts": {"ContractExclusionD1": dict(nds, Status="NOT_DETERMINED", RevealsA7Defect=None, Concerns=[]),
                                       "FailClosedProposals": [], "Note": ""},
                    "ReviewedCommit": None, "ReviewedBlob": None,
                    "KnownLimitations": ["Paso 0: el resultado `run` de las guardas no nombra la publicación como Head; revisión detenida"],
                    "IfAgreed": {k: None for k in IFAGREED_KEYS}})
        res["Step0"].update({"GuardsRunModeBaseHeadResult": False, "PreflightSummaryCandidateAndTotals": None})
    add(variant("16-parada-not-accredited-coherente", result_patch=stop, base=stop_calls, require_coherent=True))

    def agreed_with_required(res, r):
        res["RequiredFindings"] = [req_finding()]
        tvd(res)["T05_MAP_INVALID_ABSENT_NONCONSTRUCTIBLE"].update({"Status": "DOES_NOT_CONFORM", "FindingIds": ["A62-A7-R1"]})
    add(variant("17-agreed-con-un-required", result_patch=agreed_with_required, expected=["AGREED exige cero REQUIRED",
                                                                                          "AGREED exige los once temas de TopicVerdicts en CONFORMS"]))

    def cr_without_required(res, r):
        not_agreed(res, "CHANGES REQUIRED")
    add(variant("18-changes-required-sin-required", result_patch=cr_without_required, expected=["CHANGES REQUIRED sin ningún REQUIRED"]))

    def agreed_trigger_disagree(res, r):
        res["OptionalFindings"] = [opt_finding()]
        trd(res)["M-08"].update({"Assessment": "DISAGREE", "FindingIds": ["A62-A7-O1"]})
    add(variant("19-agreed-con-un-disparador-en-desacuerdo", result_patch=agreed_trigger_disagree,
                expected=["Materiality.Triggers[M-08] DISAGREE sin un REQUIRED vinculado", "AGREED exige los ocho disparadores de Materiality en AGREE"]))

    def na_no_reason(res, r):
        res.update({"Verdict": "NOT_ACCREDITED", "KnownLimitations": [], "IfAgreed": {k: None for k in IFAGREED_KEYS}})
    add(variant("20-not-accredited-sin-motivo", result_patch=na_no_reason, expected=["NOT_ACCREDITED sin el motivo en KnownLimitations"]))

    def so_mismatch(r):
        alt = base_result()
        alt["RecommendedNextAction"] = "otra cosa"
        r.so_tx_override = alt
    add(variant("21-structured-output-distinto", run_hook=so_mismatch, expected=["structured_output del result distinto"]))

    def sidechain(r):
        r.entry("assistant", isSidechain=True, effort=EFFORT, message={"model": MODEL, "role": "assistant", "content": [{"type": "text", "text": "x"}]})
        r.so.append({"type": "assistant", "message": {"model": MODEL, "content": []}, "parent_tool_use_id": "toolu_x", "session_id": SID})
        r.res_patch["subagent_stats"] = {"spawned": 1}
    add(variant("22-cadena-lateral-y-subagentes", sidechain, ["entradas de cadena lateral", "subagentes lanzados", "mensajes de un subagente"]))

    def launch_bad(r):
        args = PR.expected_args(SCHEMA)
        args.remove("--safe-mode")
        r.run_patch["Args"] = args
        r.init_patch.update({"tools": ["Bash", "Glob", "Grep", "Read", "StructuredOutput"], "permissionMode": "default",
                             "session_id": "00000000-0000-4000-8000-000000000000"})
    add(variant("23-lanzamiento-e-init-distintos", run_hook=launch_bad, expected=["argumentos del lanzamiento distintos", "herramientas del init distintas",
                                                                               "permissionMode del init distinto", "session_id del init distinto"]))

    def run_dir_reads(r):
        r.call("Read", {"file_path": RUN + "\\\\launch\\\\stdout.jsonl"}, "File does not exist.", is_error=True)
        r.call("Read", {"file_path": RUN}, "EISDIR: illegal operation on a directory, read", is_error=True)
        r.call("Read", {"file_path": CLONE + "\\\\docs"}, "EISDIR: illegal operation on a directory, read", is_error=True)
    add(variant("24-lecturas-del-run-y-de-directorios", run_dir_reads, ["lectura fuera del cierre (intento fallido", "lectura de un directorio"],
                counts={"lectura de un directorio": 2}))

    def ref_run(res, r):
        tvd(res)["T01_A71_VS_AUTHORITIES"]["ClauseRefs"] += ["order.txt:3-11", "order.txt:54"]
        mat(res)["ClauseRefs"].append("order.txt:1")
    add(variant("25-referencias-a-order-aceptadas", result_patch=ref_run, require_coherent=True))

    def failed_degraded(r):
        r.call("Read", {"file_path": absp(ADR48), "offset": a48, "limit": b48 - a48 + 1}, "File does not exist.", is_error=True)
        v = lines_of(V14)[2529:2532]
        r.call("Read", {"file_path": absp(V14), "offset": 2530, "limit": 3}, numbered([v[0], v[1] + " X", v[2]], 2530))

    def ref_failed(res, r):
        mat(res)["ClauseRefs"].append(ref(ADR48, a48))
    rv = variant("26-lectura-fallida-y-degradada", failed_degraded, ["referencia sobre líneas no entregadas fielmente en Materiality (%s)"
                                                                     % ref(ADR48, a48), "fidelidad: docs/initiatives/I-62-proposal-v14.md"],
                 result_patch=ref_failed)
'''
region('    a48, b48 = closure_range(ADR48, "Decisión, punto 6")\n', '    rv[0]["FailedReadRecorded"] = any(f["Path"] == ADR48', MID)
subst('"lectura directa: docs/initiatives/I-62-proposal-v14.md:2333-2334 no entregadas"],',
      '"lectura directa: docs/initiatives/I-62-proposal-v14.md:2231-2233 no entregadas"],')
subst('PR.clone_git = lambda *a: ("0" * 40 + "\\n") if a == ("rev-parse", "HEAD:" + A6) else orig_git(*a)',
      'PR.clone_git = lambda *a: ("0" * 40 + "\\n") if a == ("rev-parse", "HEAD:" + A7) else orig_git(*a)')

COH = '''    # ---- result schema and coherence (A-7)
    def extra_field(res, r):
        res["Extra"] = 1
    add(variant("55-esquema-invalido", result_patch=extra_field, expected=["resultado: no valida contra result.schema.json"], schema_valid=False))

    def p(name, fn, expected=(), counts=None, schema_valid=True, coherent=False, run_hook=None, base=None, extra=None):
        add(variant(name, extra=extra, result_patch=lambda res, r: fn(res), expected=expected, counts=counts, schema_valid=schema_valid,
                    require_coherent=coherent, run_hook=run_hook, base=base))

    def cr(extra, verdict="CHANGES REQUIRED"):
        def f(res):
            changes_required(res, verdict)
            extra(res)
        return f

    def bl(extra):
        def f(res):
            blocked(res)
            extra(res)
        return f

    R1, O1 = "A62-A7-R1", "A62-A7-O1"
    T1, T2, T3 = "T01_A71_VS_AUTHORITIES", "T02_CATEGORIES", "T03_PROVENANCE_REVISION_IDENTITY_FIDELITY"
    D1L, FCL = "RelatedImpacts.ContractExclusionD1", "RelatedImpacts.FailClosedProposals"

    def r1_to_t3(res):
        """Move the REQUIRED link from T02 to T03 (so «REQUIRED sin disposición» does not fire)."""
        tvd(res)[T2]["FindingIds"] = []
        tvd(res)[T3].update({"Status": "DOES_NOT_CONFORM", "FindingIds": [R1]})
    p("56-tema-no-conforme-sin-required", cr(r1_to_t3), ["TopicVerdicts[T02_CATEGORIES] DOES_NOT_CONFORM sin un REQUIRED vinculado"])
    p("57-tema-conforme-con-required", cr(lambda res: tvd(res)["T04_EFF_EFF1_B1_MAP_VALIDATION"].update({"FindingIds": [R1]})),
      ["TopicVerdicts[T04_EFF_EFF1_B1_MAP_VALIDATION] CONFORMS con un REQUIRED vinculado"])
    p("58-disparador-en-desacuerdo-sin-required", cr(lambda res: trd(res)["M-03"].update({"Assessment": "DISAGREE"})),
      ["Materiality.Triggers[M-03] DISAGREE sin un REQUIRED vinculado"])
    p("59-disparador-de-acuerdo-con-required", cr(lambda res: trd(res)["M-01"].update({"FindingIds": [R1]})),
      ["Materiality.Triggers[M-01] AGREE con un REQUIRED vinculado"])
    p("60-declaracion-de-a7-mal-copiada", lambda res: trd(res)["M-08"].update({"A7Declared": "yes"}),
      ["con un veredicto, A7Declared distinto de lo que declara A-7 §5 (M-08)"])
    p("61-pregunta-no-satisfecha-sin-hallazgo", cr(lambda res: qad(res)["Q-A7-02"].update({"Outcome": "NOT_SATISFIED"})),
      ["QuestionAnswers[Q-A7-02] NOT_SATISFIED sin un hallazgo vinculado"])
    p("62-decision-del-owner-sin-consecuencias", bl(lambda res: orc(res).update({"UnidentifiedConsequences": []})),
      ["OwnerDecisionRequired = true sin UnidentifiedConsequences"])
    p("63-sin-decision-del-owner-con-consecuencias", lambda res: orc(res).update({"UnidentifiedConsequences": ["x"]}),
      ["OwnerDecisionRequired = false con UnidentifiedConsequences"])
    p("64-blocked-sin-decision-del-owner", lambda res: not_agreed(res, BLOCKED), ["BLOCKED — OWNER DECISION sin OwnerDecisionRequired = true"])
    p("65-d1-revela-defecto-sin-required", cr(lambda res: d1(res).update({"RevealsA7Defect": True})),
      [D1L + " RevealsA7Defect = true sin un REQUIRED vinculado"])
    p("66-d1-sin-defecto-con-required", cr(lambda res: d1(res).update({"FindingIds": [R1]})),
      [D1L + " RevealsA7Defect = false con un REQUIRED vinculado"])
    p("67-fc-revela-defecto-sin-required", cr(lambda res: fcd(res)["FC-10"].update({"RevealsA7Defect": True})),
      [FCL + "[FC-10] RevealsA7Defect = true sin un REQUIRED vinculado"])
    p("68-fc-sin-defecto-con-required", cr(lambda res: fcd(res)["FC-18"].update({"FindingIds": [R1]})),
      [FCL + "[FC-18] RevealsA7Defect = false con un REQUIRED vinculado"])
    p("69-d1-preocupaciones-sin-lista", lambda res: d1(res).update({"Status": "CONCERNS_FOR_COORDINATOR"}),
      [D1L + " CONCERNS_FOR_COORDINATOR sin Concerns"])
    p("70-d1-sin-preocupacion-con-lista", lambda res: d1(res).update({"Concerns": ["x"]}), [D1L + " NO_CONCERN con Concerns"])
    p("71-fc-sin-preocupacion-con-bandera", lambda res: fcd(res)["FC-01"].update({"ChangesPassCriteria": True}), [FCL + "[FC-01] NO_CONCERN con"])
    p("72-fc-preocupacion-sin-bandera", lambda res: fcd(res)["FC-04"].update({"Status": "CONCERNS_FOR_COORDINATOR"}),
      [FCL + "[FC-04] CONCERNS_FOR_COORDINATOR sin ninguna de"])

    def ag_concerns(res):
        d1(res).update({"Status": "CONCERNS_FOR_COORDINATOR", "Concerns": ["x"]})
        fcd(res)["FC-13"].update({"ExcludesValidC24Cases": True, "Status": "CONCERNS_FOR_COORDINATOR"})
        fcd(res)["FC-16"].update({"ReliesOnImplementationCoincidence": True, "Status": "CONCERNS_FOR_COORDINATOR"})
    p("73-agreed-con-preocupaciones-sin-defecto-aceptado", ag_concerns, coherent=True)
    p("74-ifagreed-no-nulo-sin-agreed", cr(lambda res: res["IfAgreed"].update({"SuitableForCoordinatorAgreement": True})),
      ["IfAgreed debe ir en null salvo con AGREED"])
    p("75-agreed-con-ifagreed-falso", lambda res: res["IfAgreed"].update({"MaterialityConfirmed": False}),
      ["AGREED exige cero REQUIRED, ninguna decisión del Owner pendiente e IfAgreed todo en true"])
    p("76-no-determinado-con-veredicto", cr(lambda res: d1(res).update({"Status": "NOT_DETERMINED", "RevealsA7Defect": None})),
      ["con un veredicto, NOT_DETERMINED o null no se admiten (RelatedImpacts.ContractExclusionD1.Status, "
       "RelatedImpacts.ContractExclusionD1.RevealsA7Defect)"])
    p("77-decision-del-owner-nula-con-veredicto", cr(lambda res: orc(res).update({"OwnerDecisionRequired": None})),
      ["OwnerReservedConsequence.OwnerDecisionRequired no admite null"])
    p("78-paso0-falso-con-veredicto", lambda res: res["Step0"].update({"Decisions93DeclaresOrderBytesAndSha256": False}),
      ["las siete booleanas de Step0 van en true"])
    p("79-paso0-nulo-con-veredicto", lambda res: res["Step0"].update({"V14DirectReadB9L2231L2233": None}), ["las siete booleanas de Step0 van en true"])
    p("80-objeto-otro-blob", lambda res: res.update({"ReviewedBlob": OTHER_A7_BLOB}), ["objeto revisado distinto del designado"])
    p("81-objeto-otro-commit", lambda res: res.update({"ReviewedCommit": PR.CLOSURE["Publication"]["Parent"]}), ["objeto revisado distinto del designado"])
    p("82-ids-de-invocacion-de-otra-revision", lambda res: res.update({"RunId": "R20261014T180556Z-5e02"}), ["identificadores de la invocación distintos"])

    def no_refs(res):
        tvd(res)[T1]["ClauseRefs"] = []
        qad(res)["Q-A7-03"]["ClauseRefs"] = []
        trd(res)["M-02"]["ClauseRefs"] = []
        fcd(res)["FC-24"]["ClauseRefs"] = []
        mat(res)["ClauseRefs"] = []
        orc(res)["ClauseRefs"] = []
        d1(res)["ClauseRefs"] = []
    p("83-elementos-sin-referencias", no_refs, ["con un veredicto, TopicVerdicts[T01_A71_VS_AUTHORITIES] sin ClauseRefs",
                                                "con un veredicto, QuestionAnswers[Q-A7-03] sin ClauseRefs",
                                                "con un veredicto, Materiality.Triggers[M-02] sin ClauseRefs",
                                                "con un veredicto, RelatedImpacts.FailClosedProposals[FC-24] sin ClauseRefs",
                                                "con un veredicto, Materiality sin ClauseRefs",
                                                "con un veredicto, OwnerReservedConsequence sin ClauseRefs",
                                                "con un veredicto, RelatedImpacts.ContractExclusionD1 sin ClauseRefs"])

    def fid_missing(res):
        tvd(res)["T06_CLAUSEMAP_COVERAGE_RULE"]["FindingIds"] = ["A62-A7-O7"]
        trd(res)["M-04"]["FindingIds"] = ["A62-A7-O9"]
        qad(res)["Q-A7-01"]["FindingIds"] = ["A62-A7-O8"]
        d1(res)["FindingIds"] = ["A62-A7-O6"]
        fcd(res)["FC-12"]["FindingIds"] = ["A62-A7-O5"]
    p("84-findingids-inexistentes", fid_missing, ["TopicVerdicts[T06_CLAUSEMAP_COVERAGE_RULE] cita en FindingIds hallazgos que no existen",
                                                  "Materiality.Triggers[M-04] cita en FindingIds hallazgos que no existen",
                                                  "QuestionAnswers[Q-A7-01] cita en FindingIds hallazgos que no existen",
                                                  D1L + " cita en FindingIds hallazgos que no existen",
                                                  FCL + "[FC-12] cita en FindingIds hallazgos que no existen"])
    p("85-ids-de-hallazgo-repetidos", lambda res: res.update({"OptionalFindings": [opt_finding(), opt_finding()]}),
      ["identificadores de hallazgo repetidos"])
    p("86-required-sin-invariante-ni-contraejemplo", cr(lambda res: res["RequiredFindings"][0].update({"ViolatedInvariant": " ", "Counterexample": ""})),
      ["A62-A7-R1 REQUIRED sin ViolatedInvariant ni Counterexample"])
    p("87-required-sin-evidencia", cr(lambda res: res["RequiredFindings"][0].update({"Evidence": []})),
      ["resultado: no valida contra result.schema.json"], schema_valid=False)
    p("88-evidencia-fuera-del-cierre", cr(lambda res: res["RequiredFindings"][0].update({"Evidence": ["docs/HANDOFF.md:1"]})),
      ["referencia no canónica en RequiredFindings[A62-A7-R1] (%s:1)" % PR.userless(PR.norm(absp("docs/HANDOFF.md")))])
    p("89-evidencia-sobre-lineas-no-leidas", cr(lambda res: res["RequiredFindings"][0].update({"Evidence": [ref(A6, 10, 12)]})),
      ["referencia sobre líneas no entregadas fielmente en RequiredFindings[A62-A7-R1] (%s)" % ref(A6, 10, 12)])
    p("90-evidencia-opcional-no-leida", lambda res: res.update({"OptionalFindings": [dict(opt_finding(), Evidence=[ref(V14, 2408)])]}),
      ["referencia sobre líneas no entregadas fielmente en OptionalFindings[A62-A7-O1]"])
    p("91-required-sin-disposicion", cr(lambda res: res["RequiredFindings"].append(req_finding("A62-A7-R2"))),
      ["REQUIRED sin disposición: todo REQUIRED lo cita al menos un elemento evaluado (A62-A7-R2)"])

    def agreed_with(update):
        def f(res):
            res["RequiredFindings"] = [req_finding()]
            update(res)
        return f
    AG0 = "AGREED exige cero REQUIRED, ninguna decisión del Owner pendiente e IfAgreed todo en true"
    p("92-agreed-tema-no-conforme", agreed_with(lambda res: tvd(res)["T09_NO_UNAUTHORIZED_CHANGES"].update({"Status": "DOES_NOT_CONFORM",
                                                                                                         "FindingIds": [R1]})),
      [AG0, "AGREED exige los once temas de TopicVerdicts en CONFORMS"])
    p("93-agreed-disparador-en-desacuerdo", agreed_with(lambda res: trd(res)["M-05"].update({"Assessment": "DISAGREE", "FindingIds": [R1]})),
      [AG0, "AGREED exige los ocho disparadores de Materiality en AGREE"])
    p("94-agreed-impacto-revela-defecto", agreed_with(lambda res: fcd(res)["FC-16"].update({"RevealsA7Defect": True, "FindingIds": [R1]})),
      [AG0, "AGREED exige RevealsA7Defect = false en RelatedImpacts"])

    def agreed_owner(res):
        orc(res).update({"OwnerDecisionRequired": True, "UnidentifiedConsequences": ["x"]})
    p("95-agreed-con-decision-del-owner", agreed_owner, [AG0])

    def cr_ok(res):
        res["OptionalFindings"] = [opt_finding()]
        tvd(res)[T1].update({"Status": "DOES_NOT_CONFORM", "FindingIds": [R1, O1]})
        tvd(res)["T06_CLAUSEMAP_COVERAGE_RULE"].update({"FindingIds": [O1]})
        trd(res)["M-06"].update({"Assessment": "DISAGREE", "FindingIds": [R1]})
        d1(res).update({"Status": "CONCERNS_FOR_COORDINATOR", "Concerns": ["x"], "RevealsA7Defect": True, "FindingIds": [R1]})
        fcd(res)["FC-24"].update({"ChangesPassCriteria": True, "Status": "CONCERNS_FOR_COORDINATOR", "RevealsA7Defect": True, "FindingIds": [R1]})
        fcd(res)["FC-12"].update({"ExcludesValidC24Cases": True, "Status": "CONCERNS_FOR_COORDINATOR"})
        qad(res)["Q-A7-05"].update({"Outcome": "NOT_SATISFIED", "FindingIds": [O1]})
    p("96-changes-required-coherente-aceptado", cr(cr_ok), coherent=True)
    p("97-rejected-coherente-aceptado", cr(lambda res: trd(res)["M-07"].update({"Assessment": "DISAGREE", "FindingIds": [R1]}), "REJECTED"), coherent=True)
    p("98-rejected-sin-required", lambda res: not_agreed(res, "REJECTED"), ["REJECTED sin ningún REQUIRED"])
    p("99-blocked-coherente-aceptado", bl(lambda res: None), coherent=True)

    def ag_ok(res):
        res["OptionalFindings"] = [opt_finding()]
        qad(res)["Q-A7-04"].update({"Outcome": "NOT_SATISFIED", "FindingIds": [O1]})
        tvd(res)["T07_MULTI_CATEGORY_ENTRIES"]["FindingIds"] = [O1]
    p("100-agreed-con-un-opcional-aceptado", ag_ok, coherent=True)
    p("101-id-de-hallazgo-mal-formado", lambda res: res.update({"OptionalFindings": [dict(opt_finding(), FindingId="A62-A7-1")]}),
      ["resultado: no valida contra result.schema.json"], schema_valid=False)

    def a6_space_fid(res):
        res["RequiredFindings"][0]["FindingId"] = "A62-A6R4-R2"
        tvd(res)[T2]["FindingIds"] = ["A62-A6R4-R2"]
    p("102-id-nuevo-con-el-espacio-de-a6", cr(a6_space_fid), ["resultado: no valida contra result.schema.json"], schema_valid=False)
    p("103-referencia-absoluta", lambda res: mat(res).update({"ClauseRefs": [absp(A7) + ":" + str(closure_range(A7, "## 5. ")[0])]}),
      ["resultado: no valida contra result.schema.json"], schema_valid=False)
    p("104-referencia-mal-formada", lambda res: mat(res).update({"ClauseRefs": ["order.txt"]}),
      ["referencia mal formada en Materiality", "resultado: no valida contra result.schema.json"], schema_valid=False)
    p("105-esquema-estados-invalidos", lambda res: (tvd(res)[T1].update({"Status": "MAYBE"}), trd(res)["M-01"].update({"Assessment": "OK"}),
                                                    fcd(res)["FC-01"].update({"Status": "IGNORED"})),
      ["resultado: no valida contra result.schema.json", "AGREED exige los once temas de TopicVerdicts en CONFORMS",
       "AGREED exige los ocho disparadores de Materiality en AGREE"], schema_valid=False)
    p("106-esquema-booleano-como-texto", lambda res: d1(res).update({"RevealsA7Defect": "false"}),
      ["resultado: no valida contra result.schema.json", "AGREED exige RevealsA7Defect = false en RelatedImpacts"], schema_valid=False)
    p("107-esquema-veredicto-inexistente", lambda res: not_agreed(res, "ACCEPTED"), ["resultado: no valida contra result.schema.json"],
      schema_valid=False)
    # nested schema violations that Test-Json of pwsh 7.6.7 does not see (only the in-process check of the auditor does)
    p("108b-esquema-propiedad-sobrante-anidada", lambda res: res["Step0"].update({"Extra": True}), ["resultado: no valida contra result.schema.json"],
      schema_valid=False)
    p("108c-esquema-campo-requerido-ausente-anidado", lambda res: tvd(res)[T1].pop("Rationale"), ["resultado: no valida contra result.schema.json"],
      schema_valid=False)
    p("108d-esquema-texto-vacio-anidado", lambda res: mat(res).update({"Rationale": ""}), ["resultado: no valida contra result.schema.json"],
      schema_valid=False)
    p("108e-esquema-elemento-de-lista-no-texto", lambda res: d1(res).update({"Concerns": [1]}),
      ["resultado: no valida contra result.schema.json", D1L + " NO_CONCERN con Concerns"], schema_valid=False)
    p("108f-esquema-null-no-admitido-anidado", lambda res: orc(res).update({"Rationale": None}),
      ["resultado: no valida contra result.schema.json"], schema_valid=False)
    p("108g-esquema-propiedad-sobrante-doble-anidada", lambda res: fcd(res)["FC-04"].update({"X": 1}),
      ["resultado: no valida contra result.schema.json"], schema_valid=False)

    # ---- coverage of the A-7 arrays
    def drop(path, field, value):
        def f(res):
            holder = res if len(path) == 1 else res[path[0]]
            holder[path[-1]] = [x for x in holder[path[-1]] if x[field] != value]
        return f

    def dup(path, field, value, replace):
        def f(res):
            holder = res if len(path) == 1 else res[path[0]]
            items_ = holder[path[-1]]
            src = next(x for x in items_ if x[field] == value)
            holder[path[-1]] = [copy.deepcopy(src) if x[field] == replace else x for x in items_]
        return f
    p("109-tema-ausente", drop(["TopicVerdicts"], "Topic", "T11_NEGATIVES_AND_PREFLIGHT"),
      ["TopicVerdicts debe disponer cada id una vez (faltan ['T11_NEGATIVES_AND_PREFLIGHT']", "AGREED exige los once temas de TopicVerdicts"])
    p("109b-tema-repetido", dup(["TopicVerdicts"], "Topic", T1, T2),
      ["TopicVerdicts con identificadores repetidos", "TopicVerdicts debe disponer cada id una vez"])
    p("110-disparador-ausente", drop(["Materiality", "Triggers"], "Trigger", "M-07"),
      ["Materiality.Triggers debe disponer cada id una vez (faltan ['M-07']", "AGREED exige los ocho disparadores de Materiality"])
    p("110b-disparador-repetido", dup(["Materiality", "Triggers"], "Trigger", "M-01", "M-06"),
      ["Materiality.Triggers con identificadores repetidos", "Materiality.Triggers debe disponer cada id una vez"])
    p("111-propuesta-ausente", drop(["RelatedImpacts", "FailClosedProposals"], "Proposal", "FC-18"),
      ["RelatedImpacts.FailClosedProposals debe disponer cada id una vez (faltan ['FC-18']", "AGREED exige RevealsA7Defect = false en RelatedImpacts"])
    p("111b-propuesta-repetida", dup(["RelatedImpacts", "FailClosedProposals"], "Proposal", "FC-01", "FC-04"),
      ["RelatedImpacts.FailClosedProposals con identificadores repetidos", "RelatedImpacts.FailClosedProposals debe disponer cada id una vez"])
    p("112-pregunta-ausente", drop(["QuestionAnswers"], "Question", "Q-A7-06"),
      ["QuestionAnswers debe disponer cada id una vez (faltan ['Q-A7-06']"])
    p("112b-pregunta-repetida", dup(["QuestionAnswers"], "Question", "Q-A7-01", "Q-A7-02"),
      ["QuestionAnswers con identificadores repetidos", "QuestionAnswers debe disponer cada id una vez"])

    def stop_with_dup(res):
        stop(res, None)
        res["TopicVerdicts"] = [{"Topic": T1, "Status": "CONFORMS", "Rationale": "x", "ClauseRefs": [], "FindingIds": []}] * 2
    p("113-not-accredited-con-ids-repetidos", stop_with_dup, ["TopicVerdicts con identificadores repetidos"], base=stop_calls)

    # ---- direct read of V14 L2231-L2233 (the rule of v5.3/v5.4, now on B.9) and the references of nested elements
    v14d = ref(V14, 2231, 2233)
    p("114-sin-lectura-directa-de-v14", lambda res: None,
      ["lectura directa: %s no entregadas fielmente" % v14d, "referencia sobre líneas no entregadas fielmente en DirectReads (%s)" % v14d],
      base=lambda r: base_calls(r, direct=False))
    p("115-cita-de-v14-leida-solo-en-a7", lambda res: mat(res)["ClauseRefs"].append(v14d),
      ["lectura directa: %s no entregadas fielmente" % v14d, "referencia sobre líneas no entregadas fielmente en DirectReads (%s)" % v14d,
       "referencia sobre líneas no entregadas fielmente en Materiality (%s)" % v14d], base=lambda r: base_calls(r, direct=False))
    p("116-directreads-sin-v14", lambda res: res.update({"DirectReads": [{"Ref": sec_ref("### 3.4 "), "Purpose": "x"}]}),
      ["lectura directa: DirectReads sin una entrada que cubra"])
    p("117-directreads-cubre-solo-2231", lambda res: res.update({"DirectReads": [{"Ref": ref(V14, 2231), "Purpose": "x"}]}),
      ["lectura directa: DirectReads sin una entrada que cubra"])

    def cited_after_direct(res):
        mat(res)["ClauseRefs"].append(v14d)
        tvd(res)[T1]["ClauseRefs"].append(v14d)
        res["DirectReads"].append({"Ref": sec_ref("### 3.4 "), "Purpose": "x"})
    p("118-cita-de-v14-tras-la-lectura-directa-aceptada", cited_after_direct, coherent=True)
    p("119-referencia-anidada-no-leida", lambda res: trd(res)["M-02"]["ClauseRefs"].append(ref(A6, 10, 12)),
      ["referencia sobre líneas no entregadas fielmente en Materiality.Triggers[M-02] (%s)" % ref(A6, 10, 12)])
    p("120-referencia-anidada-de-evidencia-leida", lambda res: fcd(res)["FC-01"]["ClauseRefs"].append(preflight_ref()), coherent=True)
    p("121-referencia-del-impacto-d1-no-leida", lambda res: d1(res)["ClauseRefs"].append(ref(A6, 10, 12)),
      ["referencia sobre líneas no entregadas fielmente en RelatedImpacts.ContractExclusionD1 (%s)" % ref(A6, 10, 12)])

'''
region("    # ---- result schema and coherence (round 4)\n", "    # ---- run.json of the launcher (the round-3 rules, decisions §84, point 4)\n", COH)

GATE11 = '''    # ---- preflight record of gate 11 (A-7: object and agreed A-6 blobs at the HEAD; the authorization §93 after §92, no later section)
    g11 = "preflight: compuerta 11 (%s)" % OG.CHECK
    g11a = g11 + " sin el blob de A-7"
    g11p = g11 + " con la autorización pendiente"
    add(variant("150-preflight-sin-compuerta-11", run_hook=pf(lambda f: f["Checks"].pop(OG.CHECK)),
                expected=["preflight: faltan comprobaciones ['%s']" % OG.CHECK, g11 + " sin las líneas fijadas", g11p, g11a]))
    add(variant("150b-preflight-compuerta-11-otra-linea", run_hook=pf(lambda f: f["Checks"][OG.CHECK].update({"A6LineSha256": "4" * 64})),
                expected=[g11 + " sin las líneas fijadas"]))
    add(variant("150c-preflight-compuerta-11-fallida", run_hook=pf(lambda f: f["Checks"][OG.CHECK].update({"Ok": False, "A6LineNumbers": [],
                                                                                                           "Problems": ["falta la línea"]})),
                expected=["preflight: comprobaciones sin Ok: ['%s']" % OG.CHECK, g11 + " sin las líneas fijadas"]))
    for name, fn in (
            ("151-preflight-compuerta-11-sin-autorizacion", lambda f: f["Checks"][OG.CHECK].update({"AuthorizationSections": [], "Authorized": False})),
            ("151b-preflight-compuerta-11-blob-en-la-head", lambda f: f["Checks"][OG.CHECK].update({"ObjectBlobAtHead": OTHER_A7_BLOB,
                                                                                                   "ObjectBlobAtHeadMatches": False})),
            ("151b2-preflight-compuerta-11-a6-en-la-head", lambda f: f["Checks"][OG.CHECK].update({"A6BlobAtHead": OTHER_A6_BLOB,
                                                                                                  "A6AgreedBlobAtHeadMatches": False})),
            ("151c-preflight-compuerta-11-otro-runid", lambda f: f["Checks"][OG.CHECK].update({"RunId": "R20261014T180556Z-5e02"})),
            ("151d-preflight-compuerta-11-seccion-posterior", lambda f: f["Checks"][OG.CHECK].update(
                {"LaterSections": [{"Number": OG.AUTH_SECTION + 1, "Line": 1850, "Heading": "## %d. Disposición del Coordinator: (selftest)"
                                    % (OG.AUTH_SECTION + 1)}]})),
            ("151e-preflight-compuerta-11-sha-del-texto", lambda f: f["Checks"][OG.CHECK]["AuthorizationSections"][0].update({"TextSha256": "0" * 64})),
            ("151f-preflight-compuerta-11-encabezado", lambda f: f["Checks"][OG.CHECK]["AuthorizationSections"][0].update({"HeadingMatches": False})),
            ("151g-preflight-compuerta-11-dos-secciones", lambda f: f["Checks"][OG.CHECK]["AuthorizationSections"].append(
                dict(f["Checks"][OG.CHECK]["AuthorizationSections"][0], Line=1860))),
            ("151h-preflight-compuerta-11-no-autorizada", lambda f: f["Checks"][OG.CHECK].update({"Authorized": False})),
            ("151i-preflight-compuerta-11-sha-fijado-distinto", lambda f: f["Checks"][OG.CHECK].update({"AuthorizationTextSha256": "1" * 64})),
            ("151j-preflight-compuerta-11-otra-seccion", lambda f: f["Checks"][OG.CHECK]["AuthorizationSections"][0].update({"Number": 92})),
            ("151k-preflight-compuerta-11-sin-blob", lambda f: f["Checks"][OG.CHECK]["AuthorizationSections"][0].update({"HasBlob": False})),
            ("151l-preflight-compuerta-11-otro-objeto", lambda f: f["Checks"][OG.CHECK].update({"ObjectBlob": OTHER_A7_BLOB})),
            ("151m-preflight-compuerta-11-otra-seccion-fijada", lambda f: f["Checks"][OG.CHECK].update({"AuthorizationSection": 92})),
            ("151n-preflight-compuerta-11-otro-encabezado-fijado", lambda f: f["Checks"][OG.CHECK].update(
                {"AuthorizationHeading": "## 93. Otra disposición (selftest)"}))):
        add(variant(name, run_hook=pf(fn), expected=[g11a]))
    add(variant("151o-preflight-compuerta-11-registro-pendiente", run_hook=pf(lambda f: f["Checks"][OG.CHECK].update({"AuthorizationPending": True})),
                expected=[g11p]))
    saved_pins = (OG.AUTH_SECTION, OG.AUTH_HEADING, OG.AUTH_TEXT_SHA)
    try:
        OG.AUTH_SECTION = OG.AUTH_HEADING = OG.AUTH_TEXT_SHA = None   # a kit with the authorization pending: never accreditable
        add(variant("151p-kit-con-la-autorizacion-pendiente", expected=[g11p]))
    finally:
        OG.AUTH_SECTION, OG.AUTH_HEADING, OG.AUTH_TEXT_SHA = saved_pins

'''
region("    # ---- preflight record of gate 11 (round 4: the authorization after §85 pinned in owner_gate.py, no later section)\n",
       "    # ---- categories (execution vs content) reported separately\n", GATE11)

OWNER = '''# ------------------------------------------------------------------ gate 11 cases (owner_gate.evaluate on decision texts built from the clone's; A-7)
def owner_cases():
    cases = []
    dec_lines = clone_decision_lines()
    dec_clone = "\\n".join(dec_lines) + "\\n"
    a6_text = "\\n".join(PR.canon(A6))
    blob, run_id = PR.OBJECT_BLOB, PR.IDS["RunId"]
    sha_ok = PR.TRANSPORT["Binary"]["Sha256"]
    a6l = next(l for l in dec_lines if OG.line_sha(l) == OG.A6_LINE_SHA)
    claude_l = next(l for l in dec_lines if OG.line_sha(l) == OG.CLAUDE_LINE_SHA)
    s78 = next(i for i, l in enumerate(dec_lines) if l.startswith("## 78. "))
    s93 = next(i for i, l in enumerate(dec_lines) if l.startswith("## %d. " % AUTH_SECTION))
    assert not any(l.startswith("## ") for l in dec_lines[s93 + 1:]), "§93 no es la última sección de las decisiones del clon"
    base_lines, syn = dec_lines[:s93], dec_lines[s93:]
    dec_base = "\\n".join(base_lines) + "\\n"              # decisiones antes de §93: el texto del clon cuando un caso aísla la regla de la autorización
    syn_text = "\\n".join(syn) + "\\n"
    assert dec_base + syn_text == dec_clone
    pending = {"Section": None, "Heading": None, "TextSha256": None}

    def pin(text, number=AUTH_SECTION):
        v = PA.values(text, number, blob)["Values"]
        return {"Section": number, "Heading": v["AUTH_HEADING"], "TextSha256": v["AUTH_TEXT_SHA"]}
    auth93 = pin(dec_clone)

    def text(lines):
        return "\\n".join(lines) + "\\n"

    def before93(extra):
        return text(base_lines + extra)

    def replaced(old, new):
        out = "\\n".join(new if l == old else l for l in base_lines) + "\\n"
        assert out != dec_base or old == new
        return out

    def run_case(name, dec_head, expected=(), clean=True, descends=True, measured=None, clone_text=None, obj_head=None, pinned=None, ok=None,
                 dec_cl=None, auth=None, a6_head=None):
        ev_ = OG.evaluate(dec_head, dec_base if dec_cl is None else dec_cl, clean, descends, sha_ok if measured is None else measured, sha_ok,
                          a6_text if clone_text is None else clone_text, blob if obj_head is None else obj_head, blob if pinned is None else pinned,
                          run_id, OG.A6_AGREED_BLOB if a6_head is None else a6_head, auth93 if auth is None else auth)
        got = ev_["Problems"]
        missing = [e for e in expected if not any(e in g for g in got)]
        unexpected = [g for g in got if not any(e in g for e in expected)]
        want_ok = not expected if ok is None else ok
        cases.append({"Case": name, "Expected": list(expected), "Got": got, "Missing": missing, "Unexpected": unexpected, "Ok": ev_["Ok"],
                      "AuthorizationPending": ev_["AuthorizationPending"], "Authorized": ev_["Authorized"], "LaterSections": ev_["LaterSections"],
                      "SectionsAfterBase": ev_["SectionsAfterBase"],
                      "Result": "PASS" if not missing and not unexpected and ev_["Ok"] is want_ok else "FAIL"})
        return ev_

    PEND = OG.PENDING_REASON
    NOAUTH, LATER, UNIQUE, NOTAFTER = "falta la autorización", "sección posterior a §%d" % AUTH_SECTION, \\
        "decisiones sin un único encabezado §%d" % AUTH_SECTION, "no es una sección posterior a §92"
    e = run_case("O00-autorizacion-pendiente-bloquea", dec_clone, [PEND], auth=pending)
    if not (e["AuthorizationPending"] is True and e["AuthorizationSections"] == [] and e["LaterSections"] == [] and e["Problems"] == [PEND] and
            [x["Number"] for x in e["SectionsAfterBase"]] == [AUTH_SECTION]):
        cases[-1]["Result"] = "FAIL"
    e = run_case("O00b-pendiente-con-otra-seccion-posterior-a-92", dec_clone + "\\n## 94. Nota (selftest)\\n", [PEND], auth=pending)
    if [x["Number"] for x in e["SectionsAfterBase"]] != [AUTH_SECTION, 94] or e["Problems"] != [PEND]:
        cases[-1]["Result"] = "FAIL"
    fresh = load("owner_gate_as_frozen", os.path.join(KITDATA, "owner_gate.py"))
    pins = fresh.pinned_authorization()
    structural = (all(v is None for v in pins.values()) or
                  (type(pins["Section"]) is int and pins["Section"] > fresh.BASE_SECTION and str(pins["Heading"]).startswith("## %d. " % pins["Section"])
                   and re.match(r"^[0-9a-f]{64}$", str(pins["TextSha256"])) is not None))
    cases.append({"Case": "O00c-modulo-del-kit-pendiente-o-fijado-entero", "Pins": pins, "Pending": fresh.authorization_pending(pins),
                  "PinsEqualRecomputedFromClone": pins == auth93,
                  "Result": "PASS" if structural and fresh.BASE_SECTION == 92 and fresh.PENDING_REASON == PEND and
                  (fresh.authorization_pending(pins) or pins == auth93) else "FAIL"})
    run_case("O01-autorizacion-93-fijada-pasa", dec_clone)
    run_case("O01b-autorizacion-93-con-el-texto-real-del-clon", dec_clone, dec_cl=dec_clone)
    run_case("O02-seccion-posterior-a-la-fijada", dec_clone + "\\n## %d. Disposición del Coordinator: otra (selftest)\\n\\ntexto\\n" % (AUTH_SECTION + 1),
             [LATER])
    run_case("O03-seccion-99-cualquiera-posterior", dec_clone + "\\n## 99. Nota (selftest)\\n", [LATER])
    t = dec_clone.rstrip("\\n") + " (selftest)\\n"
    run_case("O04-fijada-con-otro-texto", t, [NOAUTH])
    t = dec_base + syn_text.replace("Disposición del Coordinator", "Disposición del Owner")
    run_case("O05-fijada-con-otro-encabezado", t, [NOAUTH])
    t = dec_base + syn_text.replace(blob, blob[:8] + "…")
    run_case("O06-fijada-con-el-blob-abreviado", t, [NOAUTH], auth=pin(t))
    t = dec_base + syn_text.replace(blob, blob + "0")
    run_case("O07-fijada-con-el-blob-pegado", t, [NOAUTH], auth=pin(t))
    run_case("O08-sin-la-seccion-fijada", dec_base, [UNIQUE + " (0)", NOAUTH])
    run_case("O09-dos-secciones-fijadas", dec_clone + "\\n## %d. Duplicado (selftest)\\n" % AUTH_SECTION, [UNIQUE + " (2)", NOAUTH])
    run_case("O10-fijada-no-posterior-a-92", dec_base, [NOTAFTER, NOAUTH], auth=pin(dec_base, 92))
    run_case("O10b-fijada-con-un-numero-que-no-es-entero", dec_clone, [NOTAFTER, "decisiones sin un único encabezado §93 (0)", NOAUTH],
             auth=dict(auth93, Section="93"))
    run_case("O11-a7-en-la-head-con-otro-blob", dec_clone, ["no tiene el blob de A-7 fijado en el kit"], obj_head=OTHER_A7_BLOB)
    run_case("O11b-a6-en-la-head-con-otro-blob", dec_clone, ["no tiene el blob acordado"], a6_head=OTHER_A6_BLOB)
    run_case("O11c-a6-ausente-en-la-head", dec_clone, ["no tiene el blob acordado"], a6_head="")
    run_case("O12-blob-no-fijado", dec_clone, ["blob de A-7 no fijado en el kit", "no tiene el blob de A-7 fijado", NOAUTH], pinned="")
    run_case("O12b-blob-fijado-de-otro-borrador", dec_clone, ["blob de A-7 no fijado en el kit", "no tiene el blob de A-7 fijado", NOAUTH],
             pinned=OTHER_A7_BLOB, obj_head=OTHER_A7_BLOB)
    para = a6l.replace("excepción limitada", "excepción acotada")
    assert para != a6l
    for name, new, exp in (
            ("O13-sin-la-linea-del-owner", "(línea retirada)", ["falta, como línea completa de decisiones, la línea del Owner A6-ADR0046-9-FX02"]),
            ("O14-parafrasis", para, ["falta, como línea completa de decisiones, la línea del Owner A6-ADR0046-9-FX02", "otra línea que empieza por «A6-ADR0046-9-FX02 =»"]),
            ("O15-fragmento-citado-en-tabla", "| **Owner** | `" + a6l + "` |", ["falta, como línea completa de decisiones, la línea del Owner A6-ADR0046-9-FX02"]),
            ("O16-cita-con-sangria-de-bloque", "> " + a6l, ["falta, como línea completa de decisiones, la línea del Owner A6-ADR0046-9-FX02"]),
            ("O17-espacio-final", a6l + " ", ["falta, como línea completa de decisiones, la línea del Owner A6-ADR0046-9-FX02", "otra línea que empieza por «A6-ADR0046-9-FX02 =»"]),
            ("O18-retorno-de-carro", a6l + "\\r", ["falta, como línea completa de decisiones, la línea del Owner A6-ADR0046-9-FX02", "otra línea que empieza por «A6-ADR0046-9-FX02 =»"])):
        t = replaced(a6l, new)
        run_case(name, t + syn_text, exp, dec_cl=t, auth=pin(t + syn_text))
    moved = base_lines[:s78] + [a6l] + [l for l in base_lines[s78:] if l != a6l]
    t = text(moved)
    run_case("O19-linea-antes-de-78", t + syn_text, ["no está registrada después del encabezado de decisiones §78"], dec_cl=t, auth=pin(t + syn_text))
    t = before93(["A6-ADR0046-9-FX02 = B (revoca la determinación anterior)", ""])
    run_case("O20-revocacion-anterior-a-93", t + syn_text, ["otra línea que empieza por «A6-ADR0046-9-FX02 =»"], dec_cl=t, auth=pin(t + syn_text))
    t = replaced(claude_l, "CLAUDE-CLI-I62 = (retirada)")
    run_case("O21-sin-claude-cli-i62", t + syn_text, ["falta, como línea completa de decisiones, la línea del Owner CLAUDE-CLI-I62 = A",
                                                      "otra línea que empieza por «CLAUDE-CLI-I62 =»"], dec_cl=t, auth=pin(t + syn_text))
    t = before93(["CLAUDE-CLI-I62 = B (revocada)", ""])
    run_case("O22-otra-linea-claude-cli-i62", t + syn_text, ["otra línea que empieza por «CLAUDE-CLI-I62 =»"], dec_cl=t, auth=pin(t + syn_text))
    t = dec_clone.replace("suma 4 057 bytes", "suma 4 058 bytes")
    assert t != dec_clone
    run_case("O23-decision-93-cambiada-en-la-head", t, ["no crece solo por añadido", NOAUTH], dec_cl=dec_clone)
    t = dec_clone.replace("suma 3 299 bytes", "suma 3 298 bytes")
    assert t != dec_clone
    run_case("O23b-decision-92-cambiada-en-la-head", t, ["no crece solo por añadido"], dec_cl=dec_clone)
    run_case("O24-cambios-sin-commit", dec_clone, ["cambios sin commit"], clean=False)
    run_case("O25-head-no-desciende-del-clon", dec_clone, ["no desciende del commit del clon"], descends=False)
    run_case("O26-otro-binario", dec_clone, ["SHA-256 medido del binario distinto del cierre"], measured="0" * 64)
    run_case("O27-linea-fijada-ausente-de-la-a6-acordada", dec_clone, ["no está, como línea completa, en la A-6 acordada"],
             clone_text=a6_text.replace(a6l, para))
    t = before93(["## 78. Duplicado (selftest)", ""])
    run_case("O28-dos-encabezados-78", t + syn_text, ["decisiones sin un único encabezado §78 (2)"], dec_cl=t, auth=pin(t + syn_text))
    run_case("O29-sha-del-texto-fijado-distinto", dec_clone, [NOAUTH], auth=dict(auth93, TextSha256="0" * 64))
    v_ok = PA.values(dec_clone, AUTH_SECTION, blob)
    v_later = PA.values(dec_clone + "\\n## 94. Otra (selftest)\\n", AUTH_SECTION, blob)
    v_92 = PA.values(dec_base, 92, blob)
    v_none = PA.values(dec_base, AUTH_SECTION, blob)
    lines = PA.python_lines(v_ok["Values"])
    ok30 = (v_ok["Ready"] is True and v_ok["Values"]["AUTH_HEADING"] == auth93["Heading"] and v_ok["Values"]["AUTH_TEXT_SHA"] == auth93["TextSha256"] and
            v_later["Ready"] is False and [x["Number"] for x in v_later["Checks"]["LaterSections"]] == [94] and
            v_92["Ready"] is False and v_92["Checks"]["AfterBaseSection"] is False and v_92["Checks"]["HasBlobToken"] is False and
            v_none["Ready"] is False and v_none["Values"]["AUTH_HEADING"] is None and
            len(lines) == 3 and lines[0].startswith("AUTH_SECTION = 93") and ast.literal_eval(lines[1].split(" = ", 1)[1]) == auth93["Heading"] and
            ast.literal_eval(lines[2].split(" = ", 1)[1].split("   #")[0]) == auth93["TextSha256"])
    cases.append({"Case": "O30-pin-authorization-valores", "Values": v_ok["Values"], "PythonLines": lines,
                  "LaterReady": v_later["Ready"], "Section92Ready": v_92["Ready"], "AbsentReady": v_none["Ready"], "Result": "PASS" if ok30 else "FAIL"})
    return cases


'''
region("# ------------------------------------------------------------------ gate 11 cases (owner_gate.evaluate on synthetic decision texts; round 4)\n",
       "def live_owner_gate_case():\n", OWNER)

LIVE = '''def live_owner_gate_case():
    """L00: owner_gate.check on the REAL I-62 worktree (read-only git), with owner_gate.py loaded exactly as the kit holds it (not the module of
    the audit cases). Pinned (the kit as delivered: §93) it must pass with exactly the pinned section (heading, full blob, text SHA-256), the object
    and agreed A-6 blobs at the HEAD and no later section; with the pins pending it must block with ONLY the pending reason."""
    def wt(*a):
        return subprocess.run(["git", "-C", WORKTREE] + list(a), capture_output=True, text=True, env=dict(os.environ, GIT_OPTIONAL_LOCKS="0"))
    if not os.path.isdir(WORKTREE):
        return {"Case": "L00-compuerta-11-real", "Result": "FAIL", "Got": "worktree no disponible"}
    head = wt("rev-parse", "HEAD").stdout.strip()
    og = load("owner_gate_live", os.path.join(KITDATA, "owner_gate.py"))
    closure_like = {"ObjectBlob": PR.OBJECT_BLOB, "Ids": {"RunId": PR.IDS["RunId"]}, "Transport": {"Binary": {"Sha256": PR.TRANSPORT["Binary"]["Sha256"]}}}
    ev_ = og.check(WORKTREE, CLONE, PR.REV, closure_like, PR.TRANSPORT["Binary"]["Sha256"])
    auth = ev_.get("AuthorizationSections") or []
    pending = og.authorization_pending(og.pinned_authorization())
    if pending:
        ok = ev_["Ok"] is False and ev_["Problems"] == [og.PENDING_REASON] and ev_["AuthorizationPending"] is True and auth == [] and \\
            ev_["LaterSections"] == [] and ev_["ObjectBlobAtHeadMatches"] is True and ev_["A6AgreedBlobAtHeadMatches"] is True
    else:
        ok = ev_["Ok"] is True and ev_["Authorized"] is True and ev_["LaterSections"] == [] and len(auth) == 1 and \\
            auth[0]["Number"] == og.AUTH_SECTION and auth[0]["HeadingMatches"] and auth[0]["HasBlob"] and auth[0]["TextSha256Matches"] and \\
            ev_["ObjectBlobAtHeadMatches"] is True and ev_["A6AgreedBlobAtHeadMatches"] is True
    return {"Case": "L00-compuerta-11-real", "ExpectedState": "PENDING" if pending else "PINNED", "WorktreeHead": head, "CloneCommitUsed": PR.REV,
            "Ok": ev_["Ok"], "Problems": ev_["Problems"], "ObjectBlobAtHead": ev_["ObjectBlobAtHead"], "A6BlobAtHead": ev_["A6BlobAtHead"],
            "AuthorizationPending": ev_["AuthorizationPending"], "AuthorizationSections": auth, "LaterSections": ev_["LaterSections"],
            "SectionsAfterBase": ev_["SectionsAfterBase"], "A6LineNumbers": ev_["A6LineNumbers"], "ClaudeCliLineNumbers": ev_["ClaudeCliLineNumbers"],
            "Note": "binario no medido en la sonda: se pasa el SHA-256 del cierre; nada se escribe en el worktree",
            "Result": "PASS" if ok else "FAIL"}


'''
region("def live_owner_gate_case():\n", "# ------------------------------------------------------------------ gate cases (transport_gate.evaluate", LIVE)
subst('doc = {"Selftest": "post-review.py v5.4-a6r4 + transport_gate.py + owner_gate.py (ronda 4) + pin_authorization.values + c3_analysis.analyze + "',
      'doc = {"Selftest": "post-review.py v5.5-a7 + transport_gate.py + owner_gate.py (A-7) + pin_authorization.values + c3_analysis.analyze + "')
open(p, "w", encoding="utf-8", newline="\n").write(s)
print("ok", len(s.split("\n")))
