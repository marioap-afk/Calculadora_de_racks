"""EXPERIMENTAL — NOT AUTHORIZED FOR PRODUCTION. I-62 night order §7 (P8): retry granularity.

Prototype of SEPARATE correction counters per unit (global), per gate and per (TaskId, FailureClass), with one global ceiling and finite local caps.
NO numeric cap is chosen or proposed: the two cap profiles below are test fixtures only (PROFILE_* = «perfil de prueba, no tope aprobado»).

Validators over the same event sequences:
  V14 literal (AUTOMATION_PLAN 16.8, Proposal V14 §9.3 and B.8.8 `counters.correction_launches`, unchanged): `attempts` per unit, +1 per launched
      correction; STOP S-11 if attempts >= max_attempts; per-class counter (TaskId, FailureClass) with STOP at >= 3; no reset by model, role or
      session; a new TaskId inherits only when `ContinuesTaskId` is declared (Coordinator).
  P8 proposed: the same global counter (still the single budget of ADR-0046 #7, never reset) plus a gate counter keyed by the GATE LINEAGE (a
      renamed gate keeps its lineage) and the class counter; each correction carries its DEFECT LINEAGE; a correction of an already-seen lineage
      under another TaskId without a declared continuation is refused (renaming does not open budget); counters are reconstructed only from the
      append-only durable ledger (a process restart changes nothing); an UNKNOWN counter (no reconstructible history) fails closed.
  Review-loop budgets (A-1 `architect_budgets[]`, REVIEWER `budgets`) are a different family: review rounds never touch these counters.

Rule ids: V14-S11 (attempts >= max_attempts), V14-CLASS (class counter >= 3), P8-G (global ceiling), P8-LG (gate cap), P8-LC (class cap),
          P8-R1 (same defect lineage under another TaskId without declared continuation), P8-U (unknown counter: fail closed).
Usage: python p8-counters-harness.py <out.json>
"""
import json
import sys

V14_MAX_ATTEMPTS, V14_CLASS_CAP = 3, 3        # literal values of the unit contracts (automation.max_attempts) and 16.8
PROFILE_A = {"Name": "perfil de prueba A (no tope aprobado)", "Global": 3, "Gate": 3, "Class": 3}
PROFILE_B = {"Name": "perfil de prueba B (no tope aprobado)", "Global": 6, "Gate": 2, "Class": 3}


class V14Literal:
    def __init__(self):
        self.attempts, self.by_class, self.task_parent = 0, {}, {}

    def root(self, task):
        while task in self.task_parent:
            task = self.task_parent[task]
        return task

    def correction(self, e):
        if e.get("continues_task"):
            self.task_parent[e["task"]] = e["continues_task"]
        key = (self.root(e["task"]), e["class"])
        rules = set()
        if self.attempts >= V14_MAX_ATTEMPTS:
            rules.add("V14-S11")
        if self.by_class.get(key, 0) >= V14_CLASS_CAP:
            rules.add("V14-CLASS")
        if not rules:
            self.attempts += 1
            self.by_class[key] = self.by_class.get(key, 0) + 1
        return rules

    def other(self, e):
        return set()


class P8Proposed:
    """Counters derived ONLY from the durable ledger (list of accepted correction entries) plus declared continuity and unknown markers."""

    def __init__(self, caps):
        self.caps, self.ledger, self.task_parent, self.unknown_gates, self.gate_lineage_of = caps, [], {}, set(), {}

    def root(self, task):
        while task in self.task_parent:
            task = self.task_parent[task]
        return task

    def counts(self):
        g, by_gate, by_class, lineage_task = 0, {}, {}, {}
        for x in self.ledger:
            g += 1
            by_gate[x["gate_lineage"]] = by_gate.get(x["gate_lineage"], 0) + 1
            k = (self.root(x["task"]), x["class"])
            by_class[k] = by_class.get(k, 0) + 1
            lineage_task.setdefault(x["defect_lineage"], self.root(x["task"]))
        return g, by_gate, by_class, lineage_task

    def correction(self, e):
        if e.get("continues_task"):
            self.task_parent[e["task"]] = e["continues_task"]
        gl = self.gate_lineage_of.get(e["gate"], e["gate"])
        g, by_gate, by_class, lineage_task = self.counts()
        rules = set()
        if gl in self.unknown_gates:
            rules.add("P8-U")
        if g >= self.caps["Global"]:
            rules.add("P8-G")
        if by_gate.get(gl, 0) >= self.caps["Gate"]:
            rules.add("P8-LG")
        if by_class.get((self.root(e["task"]), e["class"]), 0) >= self.caps["Class"]:
            rules.add("P8-LC")
        prior = lineage_task.get(e["lineage"])
        if prior is not None and prior != self.root(e["task"]):
            rules.add("P8-R1")
        if not rules:
            self.ledger.append({"task": e["task"], "class": e["class"], "gate_lineage": gl, "defect_lineage": e["lineage"],
                                "model": e.get("model"), "session": e.get("session")})
        return rules

    def other(self, e):
        if e["type"] == "rename_gate":
            self.gate_lineage_of[e["new"]] = self.gate_lineage_of.get(e["old"], e["old"])
        elif e["type"] == "restart":
            durable = json.loads(json.dumps({"ledger": self.ledger, "parents": self.task_parent, "unknown": sorted(self.unknown_gates),
                                             "gates": self.gate_lineage_of}))
            self.__init__(self.caps)
            self.ledger, self.task_parent = durable["ledger"], durable["parents"]
            self.unknown_gates, self.gate_lineage_of = set(durable["unknown"]), durable["gates"]
        elif e["type"] == "migrate_without_gate_history":
            self.unknown_gates.add(self.gate_lineage_of.get(e["gate"], e["gate"]))
        elif e["type"] == "reconstruct_gate":
            self.unknown_gates.discard(self.gate_lineage_of.get(e["gate"], e["gate"]))
        return set()


def C(gate, task, cls, lineage, **kw):
    return dict({"type": "correction", "gate": gate, "task": task, "class": cls, "lineage": lineage}, **kw)


S, A, B = "STOP", PROFILE_A["Name"], PROFILE_B["Name"]
SCENARIOS = [
    {"Name": "p8-01-replica-i63-presupuesto-por-unidad", "Covers": ["case", "gates-independientes"],
     "Note": "I-63: dos correcciones en G1 (Ci y Authority) dejan una sola para G2-G4 (evidencia §27, Q-G2-03, Q-G3-04)",
     "Events": [C("G1", "T1", "Ci", "L1"), C("G1", "T1", "Authority", "L2"), C("G3", "T3", "Tests", "L3"), C("G4", "T4", "Contract", "L4")],
     "Expect": {"V14": [[], [], [], ["V14-S11"]], A: [[], [], [], ["P8-G"]], B: [[], [], [], []]}},
    {"Name": "p8-02-gates-independientes", "Covers": ["gates-independientes"],
     "Note": "con un techo global mayor que el local, el agotamiento de un gate no consume el presupuesto local de otro",
     "Events": [C("G2", "T2", "X", "L1"), C("G2", "T2", "Y", "L2"), C("G2", "T2", "Z", "L3"), C("G3", "T3", "X", "L4")],
     "Expect": {"V14": [[], [], [], ["V14-S11"]], A: [[], [], [], ["P8-G"]], B: [[], [], ["P8-LG"], []]}},
    {"Name": "p8-03a-mismo-defecto-con-otro-TaskId-sin-continuidad", "Covers": ["renombre", "mismo-defecto"],
     "Note": "el literal solo hereda con ContinuesTaskId declarado; el propuesto detecta el linaje del defecto",
     "Events": [C("G3", "T3", "Tests", "L1"), C("G3", "T3", "Tests", "L1"), C("G3", "T3b", "Tests", "L1")],
     "Expect": {"V14": [[], [], []], A: [[], [], ["P8-R1"]], B: [[], [], ["P8-R1", "P8-LG"]]}},
    {"Name": "p8-03b-mismo-defecto-con-continuidad-declarada", "Covers": ["renombre", "mismo-defecto"],
     "Events": [C("G3", "T3", "Tests", "L1"), C("G3", "T3", "Tests", "L1"), C("G3", "T3b", "Tests", "L1", continues_task="T3"),
                C("G3", "T3b", "Tests", "L1")],
     "Expect": {"V14": [[], [], [], ["V14-S11", "V14-CLASS"]], A: [[], [], [], ["P8-G", "P8-LG", "P8-LC"]],
                B: [[], [], ["P8-LG"], ["P8-LG"]]}},
    {"Name": "p8-04-cambio-de-modelo-y-sesion", "Covers": ["modelo-sesion"],
     "Events": [C("G1", "T1", "Ci", "L1", model="m1", session="s1"), C("G1", "T1", "Ci", "L1", model="m2", session="s2"),
                C("G1", "T1", "Ci", "L1", model="m3", session="s3"), C("G1", "T1", "Ci", "L1", model="m1", session="s4")],
     "Expect": {"V14": [[], [], [], ["V14-S11", "V14-CLASS"]], A: [[], [], [], ["P8-G", "P8-LG", "P8-LC"]],
                B: [[], [], ["P8-LG"], ["P8-LG"]]}},
    {"Name": "p8-05-reinicio-del-proceso", "Covers": ["reinicio"],
     "Note": "los contadores salen del registro durable; reiniciar no cambia ninguna decisión",
     "Events": [C("G2", "T2", "X", "L1"), C("G2", "T2", "X", "L1"), {"type": "restart"}, C("G2", "T2", "X", "L1")],
     "Expect": {"V14": [[], [], [], []], A: [[], [], [], []], B: [[], [], [], ["P8-LG"]]}},
    {"Name": "p8-06-contador-desconocido", "Covers": ["desconocido"],
     "Note": "una unidad migrada sin historia por gate falla cerrada hasta reconstruirla con evidencia",
     "Events": [{"type": "migrate_without_gate_history", "gate": "G3"}, C("G3", "T3", "X", "L1"), C("G4", "T4", "X", "L2"),
                {"type": "reconstruct_gate", "gate": "G3"}, C("G3", "T3", "X", "L1")],
     "Expect": {"V14": [[], [], [], [], []], A: [[], ["P8-U"], [], [], []], B: [[], ["P8-U"], [], [], []]}},
    {"Name": "p8-07-techo-global-agotado", "Covers": ["techo-global"],
     "Events": [C("G1", "T1", "A", "L1"), C("G1", "T1", "B", "L2"), C("G2", "T2", "A", "L3"), C("G2", "T2", "B", "L4"),
                C("G3", "T3", "A", "L5"), C("G3", "T3", "B", "L6"), C("G4", "T4", "A", "L7")],
     "Expect": {"V14": [[], [], [], ["V14-S11"], ["V14-S11"], ["V14-S11"], ["V14-S11"]],
                A: [[], [], [], ["P8-G"], ["P8-G"], ["P8-G"], ["P8-G"]],
                B: [[], [], [], [], [], [], ["P8-G"]]}},
    {"Name": "p8-08-renombre-de-gate", "Covers": ["renombre"],
     "Note": "el gate se identifica por su linaje de contrato; renombrarlo no abre presupuesto",
     "Events": [C("G2", "T2", "X", "L1"), C("G2", "T2", "Y", "L2"), {"type": "rename_gate", "old": "G2", "new": "G2-bis"}, C("G2-bis", "T2", "Z", "L3")],
     "Expect": {"V14": [[], [], [], []], A: [[], [], [], []], B: [[], [], [], ["P8-LG"]]}},
    {"Name": "p8-09-rondas-de-revision-no-cuentan", "Covers": ["revision-separada"],
     "Note": "las rondas del bucle de revisión (A-1) son otra familia de presupuesto",
     "Events": [{"type": "review_round"}, {"type": "review_round"}, {"type": "review_round"}, C("G1", "T1", "Ci", "L1")],
     "Expect": {"V14": [[], [], [], []], A: [[], [], [], []], B: [[], [], [], []]}},
]


def run(validator, events):
    got = []
    for e in events:
        got.append(sorted(validator.correction(e) if e["type"] == "correction" else validator.other(e)))
    return got


def main():
    results, ok = {}, True
    for s in SCENARIOS:
        row = {"Covers": s["Covers"], "Note": s.get("Note", ""), "Runs": {}}
        for label, make in (("V14", V14Literal), (A, lambda: P8Proposed(PROFILE_A)), (B, lambda: P8Proposed(PROFILE_B))):
            got = run(make(), s["Events"])
            exp = [sorted(x) for x in s["Expect"][label]]
            row["Runs"][label] = {"Expected": exp, "Got": got, "Result": "PASS" if got == exp else "FAIL"}
            ok = ok and got == exp
        results[s["Name"]] = row
    covers = sorted({c for s in SCENARIOS for c in s["Covers"]})
    rules = sorted({r for v in results.values() for run_ in v["Runs"].values() for step in run_["Got"] for r in step})
    doc = {"Label": "EXPERIMENTAL — NOT AUTHORIZED FOR PRODUCTION", "Script": "p8-counters-harness.py",
           "Purpose": "dossier P8 para una A-2 futura (orden nocturna §7); nada se aplica; los perfiles de topes son fixtures de prueba",
           "Profiles": [PROFILE_A, PROFILE_B], "V14Literal": {"max_attempts": V14_MAX_ATTEMPTS, "class_cap": V14_CLASS_CAP},
           "Scenarios": results, "Covers": covers, "RulesExercised": rules, "AllAsExpected": ok}
    with open(sys.argv[1], "w", encoding="utf-8", newline="\n") as f:
        json.dump(doc, f, ensure_ascii=False, indent=1)
        f.write("\n")
    for k, v in results.items():
        print("%-56s %s" % (k, " | ".join("%s %s" % (lbl[:14], r["Result"]) for lbl, r in v["Runs"].items())))
    print("all", ok, "rules", rules, "covers", covers)
    return 0 if ok else 1


if __name__ == "__main__":
    sys.exit(main())
