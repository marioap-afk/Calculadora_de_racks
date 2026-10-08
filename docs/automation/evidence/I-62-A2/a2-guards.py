#!/usr/bin/env python3
"""Guardas mecánicas de la enmienda candidata I-62 A-2 (presupuestos de recuperación de F6).

Uso:
  python a2-guards.py run --repo <ruta> --head <sha> --out <json>      (base fijada: A2_BASE)
  python a2-guards.py self-test --out <json>                           (G5 y sus mutantes, sin repositorio)

`run`, sobre el commit exacto que introduce A-2:
  G1 alcance del delta: cada línea citada como literal de V14 (§2.1, §3.1) existe en la Proposal congelada; el ancla existe una sola vez dentro de D.3;
     al aplicar los dos párrafos tras el ancla solo cambian D.3 y sus secciones ancestro (función `sections` de clause_map.py, extraída por blob);
  G2 rutas: diff --name-status --no-renames A2_BASE..head; base fijada y ancestro; A-2 y su paquete añadidos (A); lista exacta permitida; decisiones y
     evidencia solo por añadido;
  G3 blobs congelados (V14, Freeze, A-1, ADR-0048, clause_map.py) sin cambio en head;
  G4 C-20b: ninguna ruta cambiada está en SURFACES (leídas de clause_map.py extraído de head) y `clause_map.py check <merge-base> head` = EQUAL;
  G5 modelo de presupuestos: vectores por regla (motivo de rechazo comprobado), mutantes que deben fallar, cobertura de reglas y vínculo con el texto
     del delta en head.
Sin escrituras salvo --out y un directorio temporal. Sin red.
"""
import argparse, ast, collections, importlib.util, json, os, re, subprocess, sys, tempfile

A2_BASE = "c9f9419dc4aed32242c59de489a8e78a8e369f9b"
V14 = "docs/initiatives/I-62-proposal-v14.md"
V14_COMMIT = "4c617e82b32b6c810b68d75fc19472efed22b393"
A2 = "docs/initiatives/I-62-A-2.md"
PKG = "docs/initiatives/I-62-architect-package-A-2.md"
CLAUSE_MAP = "docs/automation/evidence/I-62-F4/compat/clause_map.py"
FROZEN = {
    V14: "34ad80ea1bfff144bfc5169f62920a4c904c1bfa",
    "docs/initiatives/I-62-consensus-freeze.md": "0f6b860837e2478db8525e1fb30a485bc4646816",
    "docs/initiatives/I-62-A-1.md": "c01899a72b940503bb85a0fab42bc085c603fd0f",
    "docs/adr/0048-ejecucion-delegada-portable-roles-binding-y-autoverificacion.md": "e1bd8d91f10cb4c86eaf7753f9ef2531ae62498e",
    CLAUSE_MAP: "ffe6ea57257f30fa9eec3e9aaf5bf9982bf59d08",
}
ANCHOR = "Ningún tope autoriza gasto ni ejecución ahora."
ALLOWED_EXACT = {A2, PKG, "docs/automation/evidence/I-62-evidence.md", "docs/automation/evidence/I-62-F6/README.md",
                 "docs/automation/decisions/I-62.md", "docs/automation/state/I-62.yml"}
ALLOWED_PREFIX = ("docs/automation/evidence/I-62-A2/",)
APPEND_ONLY = ("docs/automation/evidence/I-62-evidence.md", "docs/automation/decisions/I-62.md")
REQUIRED_ADDED = (A2, PKG)
D3_ANCESTORS = ("### D.3 Hoja de invocaciones y escenarios", "## Anexo D — Pilotos y portabilidad (plano c)")
TEXT_BINDING = {
    "A2-P1": ["como máximo **una** reejecución limpia extraordinaria", "no se acredita INVALID_LAUNCH", "OD-5 (consumo dentro de los topes de D.3)",
              "suben como máximo en uno", "«N11: B2» conserva su alcance", "no concede otra reejecución extraordinaria", "no se aplica a D.5",
              "sin reescribirse", "su lanzamiento físico queda registrado"],
    "A2-P2": ["**un solo** bloque de medición nuevo", "como máximo **dos** sondas de solo lectura", "nunca se descuentan ni amplían", "caducan",
              "P-07 comprueba el tope de dos del bloque", "acepta con OD-2", "no reinicia ningún presupuesto", "no se hace ningún trabajo ordinario de modelo"],
}


def git(repo, *a):
    return subprocess.run(["git", "-C", repo] + list(a), capture_output=True, check=True).stdout.decode("utf-8")


def load_clause_map(repo, rev, td):
    path = os.path.join(td, "clause_map.py")
    open(path, "w", encoding="utf-8", newline="\n").write(git(repo, "show", "%s:%s" % (rev, CLAUSE_MAP)))
    spec = importlib.util.spec_from_file_location("clause_map_pinned", path)
    mod = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(mod)
    return mod, path


def section_text(md, title):
    m = re.search(r"^### %s\n(.*?)(?=^### |^## )" % re.escape(title), md, re.S | re.M)
    return m.group(1) if m else ""


def quoted_paragraph(block):
    lines = [l[2:] if l.startswith("> ") else ("" if l == ">" else None) for l in block.split("\n")]
    q = "\n".join(l for l in lines if l is not None).strip()
    return q[1:-1] if q.startswith("«") and q.endswith("»") else q


# ---------------- G1..G4 ----------------
def g1_delta_scope(repo, head, cm):
    a2 = git(repo, "show", "%s:%s" % (head, A2))
    v14 = git(repo, "show", "%s:%s" % (V14_COMMIT, V14))
    f = []
    literal_lines = 0
    for title in ("2.1 Cláusulas anteriores (literales, V14)", "3.1 Cláusulas anteriores (literales, V14)"):
        for l in section_text(a2, title).split("\n"):
            if l.startswith("> ") and l[2:].strip():
                literal_lines += 1
                if l[2:] not in v14:
                    f.append("literal que no está en V14: " + l[2:][:100])
    deltas = [section_text(a2, "2.3 Delta exacto"), section_text(a2, "3.3 Delta exacto")]
    paras = []
    for i, b in enumerate(deltas):
        if "Ubicación: al final de V14 D.3" not in b:
            f.append("delta %d sin ubicación «al final de V14 D.3»" % (i + 1))
        p = quoted_paragraph("\n".join(l for l in b.split("\n") if l.startswith(">")))
        if not p:
            f.append("delta %d sin párrafo citado" % (i + 1))
        paras.append(p)
    d3, d4 = v14.index("### D.3 "), v14.index("### D.4 ")
    if v14.count(ANCHOR) != 1 or not (d3 < v14.find(ANCHOR) < d4):
        f.append("ancla ausente, repetida o fuera de D.3")
    applied = v14.replace(ANCHOR, ANCHOR + "\n\n" + paras[0] + "\n\n" + paras[1], 1)
    before = {(lv, h): t for lv, h, t in cm.sections(v14)}
    after = {(lv, h): t for lv, h, t in cm.sections(applied)}
    added = sorted(set(after) - set(before))
    removed = sorted(set(before) - set(after))
    changed = sorted(k for k in before if k in after and before[k] != after[k])
    allowed = {cm.norm(h) for h in D3_ANCESTORS}
    unexpected = [k for k in changed if k[1] not in allowed and k[0] != 1]
    if added or removed:
        f.append("al aplicar aparecen o desaparecen secciones: %s %s" % (added, removed))
    if unexpected:
        f.append("al aplicar cambian secciones fuera de D.3 y sus ancestros: %s" % unexpected)
    if not any(k[1] == cm.norm(D3_ANCESTORS[0]) for k in changed):
        f.append("al aplicar no cambia D.3")
    for k, v in (("Applies-to", "Applies-to: I-62"), ("FREEZE_SHA", "FREEZE_SHA     = b64a3b640c7ee3bd77636e7a3218ae0ebac2dd43"),
                 ("blob V14", "blob         = 34ad80ea1bfff144bfc5169f62920a4c904c1bfa")):
        if v not in a2:
            f.append("cabecera sin %s" % k)
    return {"Check": "G1 alcance del delta y aplicación sobre V14", "LiteralLines": literal_lines, "ChangedSections": [k[1] for k in changed],
            "Findings": f, "Result": "PASS" if not f else "FAIL"}


def g2_paths(repo, base, head):
    f = []
    if base != A2_BASE:
        f.append("base distinta de A2_BASE")
    if subprocess.run(["git", "-C", repo, "merge-base", "--is-ancestor", base, head]).returncode != 0:
        f.append("la base no es ancestro de head")
    rows = [l.split("\t") for l in git(repo, "diff", "--name-status", "--no-renames", base, head).splitlines() if l]
    status = {r[1]: r[0] for r in rows}
    for p in REQUIRED_ADDED:
        if status.get(p) != "A":
            f.append("%s no aparece como añadido (A)" % p)
    bad = [p for p in status if not (p in ALLOWED_EXACT or p.startswith(ALLOWED_PREFIX))]
    if bad:
        f.append("rutas no permitidas: %s" % bad)
    for p in APPEND_ONLY:
        if p in status:
            if status[p] != "M" or not git(repo, "show", "%s:%s" % (head, p)).startswith(git(repo, "show", "%s:%s" % (base, p))):
                f.append("%s no cambia solo por añadido" % p)
    return {"Check": "G2 rutas", "Base": base, "Head": head, "Changed": status, "Findings": f, "Result": "PASS" if not f else "FAIL"}


def g3_frozen(repo, head):
    got = {p: git(repo, "rev-parse", "%s:%s" % (head, p)).strip() for p in FROZEN}
    diff = {p: {"Expected": FROZEN[p], "Got": got[p]} for p in FROZEN if got[p] != FROZEN[p]}
    return {"Check": "G3 blobs congelados", "Blobs": got, "Mismatches": diff, "Result": "PASS" if not diff else "FAIL"}


def g4_clause_map(repo, head, changed, cm, tool):
    base = git(repo, "merge-base", head, "origin/main").strip()
    in_surf = [p for p in changed if cm.in_surfaces(p)]
    with tempfile.TemporaryDirectory() as td:
        outp = os.path.join(td, "c20b.json")
        r = subprocess.run([sys.executable, tool, "check", base, head, outp], cwd=repo, capture_output=True)
        text = (r.stdout + r.stderr).decode("utf-8", "replace")
    equal = r.returncode == 0 and "EQUAL" in text
    return {"Check": "G4 C-20b", "Base": base, "SurfacesTouched": in_surf, "ExitCode": r.returncode, "Output": text.strip()[-300:],
            "Result": "PASS" if equal and not in_surf else "FAIL"}


# ---------------- G5: modelo de presupuestos ----------------
class Reject(Exception):
    pass


ROWS = {  # V14 D.3 y D.8: filas de sesiones de Principal (y una fila que no es de Principal, para el alcance)
    "A/Principal A": {"cap": 2, "serves": {"FX-01", "FX-02", "FX-05"}, "round": "A", "principal": True},
    "FX-04a/Principal B": {"cap": 2, "serves": {"FX-04a", "FX-04b"}, "round": "FX-04a", "principal": True},
    "FX-04a/N11: B2": {"cap": 1, "serves": {"FX-04a/N11"}, "round": "FX-04a", "principal": True, "scoped": True},
    "B/Principal": {"cap": 2, "serves": {"FX-03"}, "round": "B", "principal": True},
    "D.8/Principal A": {"cap": 2, "serves": {"FX-06"}, "round": None, "principal": True},
    "D.5/Principal B": {"cap": 1, "serves": {"OV:FX-04a"}, "round": None, "principal": True, "ov": True},
    "A/Worker": {"cap": 2, "serves": {"FX-02"}, "round": "A", "principal": False},
}
ROUND_TOTALS = {"A": 2, "FX-04a": 3, "B": 2}  # sesiones de Principal: A ≤ 2; FX-04a ≤ 2 (+1 B2); B ≤ 2
SEED = {"attempts": 1, "invocations": 3, "transport_reruns": 1, "corrections": 1, "reviews": 2, "loop_budget": 2}


class F6Budgets:
    def __init__(self, a2=True):
        self.a2 = a2
        self.row_launched = collections.Counter()
        self.round_launched = collections.Counter()
        self.run_row, self.raw, self.forbidden, self.accredited = {}, {}, set(), {}
        self.extra_rows, self.extra_owner, self.scen_used = set(), set(), set()
        self.counters = dict(SEED)
        # A2-P2
        self.frozen_cap, self.frozen_used, self.probes_counted = 2, 0, 0
        self.runtime, self.epoch, self.measured, self.accepted = None, 0, None, None
        self.ever_measured, self.blocks = False, {}

    # ---- A2-P1 ----
    def cap(self, row):
        return ROWS[row]["cap"] + (1 if row in self.extra_rows else 0)

    def round_cap(self, rnd):
        return ROUND_TOTALS[rnd] + sum(1 for r in self.extra_rows if ROWS[r]["round"] == rnd)

    def launch(self, row, run):
        if self.row_launched[row] >= self.cap(row):
            raise Reject("P-07: tope de la fila")
        rnd = ROWS[row]["round"]
        if ROWS[row]["principal"] and rnd and self.round_launched[rnd] >= self.round_cap(rnd):
            raise Reject("P-07: total de la ronda")
        if self.row_launched[row] >= ROWS[row]["cap"] and row not in self.extra_owner:
            raise Reject("sin consumo autorizado por el Owner")
        self.row_launched[row] += 1
        if ROWS[row]["principal"] and rnd:
            self.round_launched[rnd] += 1
        self.run_row[run] = row

    def record_raw(self, run, raw):
        if run in self.raw:
            raise Reject("evidencia histórica: no se reescribe")
        self.raw[run] = raw

    def record_forbidden_read(self, run):
        self.forbidden.add(run)

    def accredit_invalid_launch(self, run, by):
        if by != "COORDINATOR":
            raise Reject("acreditación solo del Coordinator")
        if run in self.forbidden:
            raise Reject("lectura prohibida: FAIL de aislamiento (D.6)")
        self.accredited[run] = "INVALID_LAUNCH"

    def set_result(self, run, result):
        if run in self.accredited and result in ("PASS", "FAIL"):
            raise Reject("INVALID_LAUNCH no es PASS ni FAIL")
        self.raw.setdefault(run, result)

    def scenario_result(self, sc):
        runs = [r for r, row in self.run_row.items() if sc in ROWS[row]["serves"]]
        if any(r in self.forbidden for r in runs):
            return "FAIL"
        valid = [self.raw[r] for r in runs if r not in self.accredited and r in self.raw]
        return valid[-1] if valid else "UNVERIFIED"

    def dispose_extraordinary(self, row, by):
        if not self.a2:
            raise Reject("sin A-2 no hay reejecución extraordinaria")
        if by != "COORDINATOR":
            raise Reject("disposición solo del Coordinator")
        meta = ROWS[row]
        if not meta["principal"]:
            raise Reject("no es fila de Principal")
        if meta.get("ov"):
            raise Reject("no se aplica a D.5")
        if meta.get("scoped"):
            raise Reject("la fila N11 no participa")
        if not any(r in self.accredited for r, rw in self.run_row.items() if rw == row):
            raise Reject("sin corrida acreditada INVALID_LAUNCH en la fila")
        if row in self.extra_rows or meta["serves"] & self.scen_used:
            raise Reject("como máximo una reejecución extraordinaria")
        self.extra_rows.add(row)
        self.scen_used |= meta["serves"]

    def authorize_consumption(self, row, by):
        if by != "OWNER":
            raise Reject("el consumo lo autoriza el Owner")
        self.extra_owner.add(row)

    # ---- A2-P2 ----
    def observe_runtime(self, pair):
        if pair != self.runtime:
            self.runtime = pair
            self.epoch += 1

    def _measure(self):
        self.measured = (self.runtime, self.epoch)
        self.ever_measured = True

    def frozen_probe(self):
        if self.frozen_used >= self.frozen_cap:
            raise Reject("fila «Sondas previas» agotada")
        self.frozen_used += 1
        self.probes_counted += 1
        self._measure()

    def historical_probe(self):  # sondas autorizadas aparte antes de A-2 (OD-2d-PROBE, decisiones §51): contadas, no son bloque
        self.probes_counted += 1
        self._measure()

    def request_block(self, pair):
        if not self.a2:
            raise Reject("sin A-2 no hay bloque nuevo")
        if self.frozen_used < self.frozen_cap:
            raise Reject("la fila «Sondas previas» no está agotada")
        if pair != self.runtime:
            raise Reject("par no observado")
        if self.epoch in self.blocks:
            raise Reject("segundo bloque para la misma actualización")
        if not self.ever_measured or self.measured == (self.runtime, self.epoch):
            raise Reject("sin actualización que invalide una medición")
        self.blocks[self.epoch] = {"pair": pair, "coord": False, "owner": False, "used": 0}

    def grant(self, pair, who):
        b = self.blocks.get(self.epoch)
        if b is None or b["pair"] != pair:
            raise Reject("sin bloque pedido para el par vigente")
        b["coord" if who == "COORDINATOR" else "owner"] = True

    def block_probe(self, pair, read_only=True):
        if not read_only:
            raise Reject("solo sondas de solo lectura")
        b = self.blocks.get(self.epoch)
        if b is None or b["pair"] != pair or pair != self.runtime:
            raise Reject("sin autorización para ese par")
        if not (b["coord"] and b["owner"]):
            raise Reject("sin autorización explícita")
        if b["used"] >= 2:
            raise Reject("tope de dos sondas por bloque")
        b["used"] += 1
        self.probes_counted += 1
        self._measure()

    def accept_od2(self, pair):
        if self.measured != (pair, self.epoch) or pair != self.runtime:
            raise Reject("OD-2 solo sobre una medición vigente")
        self.accepted = (pair, self.epoch)

    def ordinary_invocation(self):
        if self.runtime is None or self.measured != (self.runtime, self.epoch):
            raise Reject("observación obsoleta")
        if self.accepted != (self.runtime, self.epoch):
            raise Reject("huella no aceptada con OD-2")
        self.counters["invocations"] += 1


def vectors(cls=F6Budgets, out=None):
    out = [] if out is None else out

    def case(cid, rule, desc, ok, detail=""):
        out.append({"Id": cid, "Rule": rule, "Case": desc, "Result": "PASS" if ok else "FAIL", "Detail": detail})

    def rej(fn, reason):
        try:
            fn()
        except Reject as e:
            return reason in str(e), str(e)
        return False, "no rechazado"

    def fx04a(m, accredit=True):
        m.launch("FX-04a/Principal B", "B1"); m.record_raw("B1", "FAIL")
        m.launch("FX-04a/Principal B", "B2"); m.record_raw("B2", "FAIL")
        if accredit:
            m.accredit_invalid_launch("B2", "COORDINATOR")
        return m

    # ---- A2-P1 ----
    m = fx04a(cls(a2=False), accredit=False)
    ok, d = rej(lambda: m.launch("FX-04a/Principal B", "B3"), "P-07"); case("P1-01", "A2-P1.4", "sin A-2: B3 rechazada por P-07", ok, d)
    m = fx04a(cls(), accredit=False)
    ok, d = rej(lambda: m.dispose_extraordinary("FX-04a/Principal B", "COORDINATOR"), "sin corrida acreditada"); case("P1-02", "A2-P1.4", "sin acreditación INVALID_LAUNCH no hay disposición", ok, d)
    m = fx04a(cls())
    ok, d = rej(lambda: m.launch("FX-04a/Principal B", "B3"), "P-07"); case("P1-03", "A2-P1.4", "acreditada sin disposición: B3 rechazada", ok, d)
    m.dispose_extraordinary("FX-04a/Principal B", "COORDINATOR")
    ok, d = rej(lambda: m.launch("FX-04a/Principal B", "B3"), "consumo autorizado por el Owner"); case("P1-04", "A2-P1.4", "con disposición pero sin consumo del Owner: rechazada", ok, d)
    m.authorize_consumption("FX-04a/Principal B", "OWNER"); m.launch("FX-04a/Principal B", "B3")
    ok, d = rej(lambda: m.launch("FX-04a/Principal B", "B4"), "P-07"); case("P1-05", "A2-P1.4", "exactamente una B3; B4 rechazada", ok and m.row_launched["FX-04a/Principal B"] == 3, d)
    m.record_raw("B3", "FAIL"); m.accredit_invalid_launch("B3", "COORDINATOR")
    ok, d = rej(lambda: m.dispose_extraordinary("FX-04a/Principal B", "COORDINATOR"), "como máximo una"); case("P1-06", "A2-P1.6", "otro fallo del arnés: sin otra reejecución", ok, d)
    m = fx04a(cls(), accredit=False)
    ok, d = rej(lambda: m.accredit_invalid_launch("B2", "SUPERVISION"), "solo del Coordinator"); case("P1-07", "A2-P1.scope", "la supervisión no acredita INVALID_LAUNCH", ok, d)
    m = fx04a(cls())
    ok, d = rej(lambda: m.dispose_extraordinary("FX-04a/Principal B", "SUPERVISION"), "disposición solo del Coordinator"); case("P1-08", "A2-P1.4", "disposición solo del Coordinator (con corrida acreditada)", ok, d)
    m = fx04a(cls())
    case("P1-09", "A2-P1.1", "el FAIL bruto de B2 se conserva tras la acreditación", m.raw["B2"] == "FAIL")
    ok, d = rej(lambda: m.record_raw("B2", "PASS"), "no se reescribe"); case("P1-10", "A2-P1.1", "la evidencia histórica no se reescribe", ok, d)
    case("P1-11", "A2-P1.2", "el escenario sigue UNVERIFIED (B1 y B2 acreditadas o sin corrida válida)", (m.accredit_invalid_launch("B1", "COORDINATOR") or True) and m.scenario_result("FX-04a") == "UNVERIFIED")
    ok, d = rej(lambda: m.set_result("B2", "PASS"), "no es PASS ni FAIL"); case("P1-12", "A2-P1.2", "una corrida INVALID_LAUNCH no puede ser PASS", ok, d)
    m2 = cls(); m2.launch("FX-04a/Principal B", "B1"); m2.record_raw("B1", "FAIL"); m2.record_forbidden_read("B1")
    ok, d = rej(lambda: m2.accredit_invalid_launch("B1", "COORDINATOR"), "FAIL de aislamiento"); case("P1-13", "A2-P1.2", "lectura prohibida: no se acredita INVALID_LAUNCH; FAIL de aislamiento", ok and m2.scenario_result("FX-04a") == "FAIL", d)
    m3 = fx04a(cls()); m3.dispose_extraordinary("FX-04a/Principal B", "COORDINATOR"); m3.authorize_consumption("FX-04a/Principal B", "OWNER"); m3.launch("FX-04a/Principal B", "B3"); m3.record_raw("B3", "PASS")
    case("P1-14", "A2-P1.2", "una B3 válida con PASS da PASS al escenario", m3.scenario_result("FX-04a") == "PASS")
    case("P1-15", "A2-P1.3", "el lanzamiento inválido cuenta en la fila y en la ronda", fx04a(cls()).row_launched["FX-04a/Principal B"] == 2 and fx04a(cls()).round_launched["FX-04a"] == 2)
    m4 = cls(); m4.launch("A/Principal A", "A1"); m4.record_raw("A1", "FAIL"); m4.accredit_invalid_launch("A1", "COORDINATOR")
    m4.dispose_extraordinary("A/Principal A", "COORDINATOR")
    ok, d = rej(lambda: m4.dispose_extraordinary("A/Principal A", "COORDINATOR"), "como máximo una")
    case("P1-16", "A2-P1.4", "fila compartida (FX-01, FX-02, FX-05): +1 como máximo en toda F6", ok and m4.cap("A/Principal A") == 3 and m4.round_cap("A") == 3, d)
    m5 = cls(); m5.launch("FX-04a/N11: B2", "N11"); m5.record_raw("N11", "FAIL"); m5.accredit_invalid_launch("N11", "COORDINATOR")
    ok, d = rej(lambda: m5.dispose_extraordinary("FX-04a/N11: B2", "COORDINATOR"), "N11 no participa"); case("P1-17", "A2-P1.5", "la fila N11 no participa ni cambia", ok and m5.cap("FX-04a/N11: B2") == 1, d)
    m6 = fx04a(cls()); before = (dict(m6.counters), m6.probes_counted, m6.row_launched["FX-04a/N11: B2"])
    m6.dispose_extraordinary("FX-04a/Principal B", "COORDINATOR"); m6.authorize_consumption("FX-04a/Principal B", "OWNER"); m6.launch("FX-04a/Principal B", "B3")
    case("P1-18", "A2-P1.5", "la reejecución no reinicia ni altera otros contadores (sembrados)", (dict(m6.counters), m6.probes_counted, m6.row_launched["FX-04a/N11: B2"]) == before and m6.counters == SEED)
    case("P1-19", "A2-P1.5", "FX-04b no tiene fila propia: sigue la sesión de FX-04a y consume su reejecución", "FX-04b" in m6.scen_used and not any("FX-04b/" in r for r in ROWS))
    m7 = cls(); m7.launch("D.5/Principal B", "OV1"); m7.record_raw("OV1", "FAIL"); m7.accredit_invalid_launch("OV1", "COORDINATOR")
    ok, d = rej(lambda: m7.dispose_extraordinary("D.5/Principal B", "COORDINATOR"), "D.5"); case("P1-20", "A2-P1.scope", "no se aplica a D.5", ok, d)
    m8 = cls(); m8.launch("A/Worker", "W1"); m8.record_raw("W1", "FAIL"); m8.accredit_invalid_launch("W1", "COORDINATOR")
    ok, d = rej(lambda: m8.dispose_extraordinary("A/Worker", "COORDINATOR"), "no es fila de Principal"); case("P1-21", "A2-P1.scope", "no se aplica a sesiones que no son de Principal", ok, d)
    m9 = cls(); m9.launch("D.8/Principal A", "F1"); m9.launch("D.8/Principal A", "F2"); m9.record_raw("F2", "FAIL"); m9.accredit_invalid_launch("F2", "COORDINATOR")
    m9.dispose_extraordinary("D.8/Principal A", "COORDINATOR"); m9.authorize_consumption("D.8/Principal A", "OWNER"); m9.launch("D.8/Principal A", "F3")
    case("P1-22", "A2-P1.3", "FX-06 (D.8, sin total de ronda): sube solo el tope de su fila", m9.cap("D.8/Principal A") == 3)

    # ---- A2-P2 ----
    p = cls()
    p.observe_runtime(("37762753", "26.930.3930.0")); p.frozen_probe(); p.frozen_probe()        # OD-2b-PROBE
    p.observe_runtime(("3b8f6e33", "26.930.7945.0")); p.historical_probe(); p.historical_probe()  # OD-2d-PROBE (decisiones §51)
    case("P2-01", "A2-P2.1", "historia real: 4 sondas contadas (2 de la fila + 2 autorizadas aparte)", p.probes_counted == 4)
    ok, d = rej(p.frozen_probe, "agotada"); case("P2-02", "A2-P2.1", "la fila «Sondas previas» está agotada", ok, d)
    p.observe_runtime(("97c57e4e", "26.1002.6548.0"))
    ok, d = rej(p.ordinary_invocation, "obsoleta"); case("P2-03", "A2-P2.2", "actualización: sin trabajo de modelo con la observación obsoleta", ok, d)
    ok, d = rej(lambda: p.block_probe(("97c57e4e", "26.1002.6548.0")), "sin autorización para ese par"); case("P2-04", "A2-P2.3", "sin bloque pedido: sonda rechazada", ok, d)
    ok, d = rej(lambda: p.request_block(("ffff0000", "26.9999.0.0")), "par no observado"); case("P2-05", "A2-P2.3", "bloque anticipado (par no observado) rechazado", ok, d)
    p.request_block(("97c57e4e", "26.1002.6548.0")); p.grant(("97c57e4e", "26.1002.6548.0"), "COORDINATOR")
    ok, d = rej(lambda: p.block_probe(("97c57e4e", "26.1002.6548.0")), "sin autorización explícita"); case("P2-06", "A2-P2.3", "sin el consumo del Owner: rechazada", ok, d)
    q = cls(); q.observe_runtime(("a", "1")); q.frozen_probe(); q.frozen_probe(); q.observe_runtime(("b", "2")); q.request_block(("b", "2")); q.grant(("b", "2"), "OWNER")
    ok, d = rej(lambda: q.block_probe(("b", "2")), "sin autorización explícita"); case("P2-07", "A2-P2.3", "sin la disposición del Coordinator: rechazada", ok, d)
    p.grant(("97c57e4e", "26.1002.6548.0"), "OWNER")
    ok, d = rej(lambda: p.block_probe(("97c57e4e", "26.1002.6548.0"), read_only=False), "solo lectura"); case("P2-08", "A2-P2.3", "sonda con escritura rechazada", ok, d)
    p.block_probe(("97c57e4e", "26.1002.6548.0")); p.block_probe(("97c57e4e", "26.1002.6548.0"))
    ok, d = rej(lambda: p.block_probe(("97c57e4e", "26.1002.6548.0")), "tope de dos"); case("P2-09", "A2-P2.3", "tope de dos sondas por bloque (P-07 del bloque)", ok, d)
    ok, d = rej(lambda: p.request_block(("97c57e4e", "26.1002.6548.0")), "segundo bloque"); case("P2-10", "A2-P2.3", "sin segundo bloque para la misma actualización", ok, d)
    p.grant(("97c57e4e", "26.1002.6548.0"), "COORDINATOR")
    ok, d = rej(lambda: p.block_probe(("97c57e4e", "26.1002.6548.0")), "tope de dos"); case("P2-11", "A2-P2.3", "volver a conceder no reinicia el bloque", ok, d)
    ok, d = rej(p.ordinary_invocation, "no aceptada"); case("P2-12", "A2-P2.4", "sin OD-2 sobre la huella resultante: invocación rechazada", ok, d)
    p.accept_od2(("97c57e4e", "26.1002.6548.0")); p.ordinary_invocation()
    case("P2-13", "A2-P2.4", "con OD-2 aceptada: invocación ordinaria permitida", p.counters["invocations"] == SEED["invocations"] + 1)
    case("P2-14", "A2-P2.1", "las sondas anteriores siguen contadas (4 + 2 = 6; nunca bajan)", p.probes_counted == 6)
    case("P2-15", "A2-P2.5", "el bloque no reinicia presupuestos de escenario (sembrados)", {k: v for k, v in p.counters.items() if k != "invocations"} == {k: v for k, v in SEED.items() if k != "invocations"} and not p.row_launched)
    r = cls(); r.observe_runtime(("a", "1")); r.frozen_probe(); r.frozen_probe(); r.observe_runtime(("b", "2")); r.request_block(("b", "2")); r.grant(("b", "2"), "COORDINATOR"); r.grant(("b", "2"), "OWNER"); r.block_probe(("b", "2"))
    r.observe_runtime(("c", "3"))
    ok1, d1 = rej(lambda: r.block_probe(("c", "3")), "sin autorización para ese par")
    ok2, d2 = rej(lambda: r.block_probe(("b", "2")), "sin autorización para ese par")
    ok3, _ = rej(r.ordinary_invocation, "obsoleta")
    case("P2-16", "A2-P2.6", "actualización con una sonda sin usar: caduca; ni par nuevo ni par anterior; sin trabajo de modelo", ok1 and ok2 and ok3, d1 + " / " + d2)
    r.request_block(("c", "3")); r.grant(("c", "3"), "COORDINATOR"); r.grant(("c", "3"), "OWNER"); r.block_probe(("c", "3")); r.block_probe(("c", "3"))
    ok, d = rej(lambda: r.block_probe(("c", "3")), "tope de dos"); case("P2-17", "A2-P2.6", "bloque del par nuevo solo con la misma autoridad y el mismo tope", ok, d)
    s = cls(); s.observe_runtime(("a", "1")); s.frozen_probe(); s.frozen_probe(); s.observe_runtime(("b", "2")); s.request_block(("b", "2")); s.grant(("b", "2"), "COORDINATOR"); s.grant(("b", "2"), "OWNER")
    s.observe_runtime(("c", "3"))
    ok, d = rej(lambda: s.block_probe(("b", "2")), "sin autorización para ese par"); case("P2-18", "A2-P2.6", "actualización entre la autorización y la primera sonda: el bloque queda inválido", ok, d)
    t = cls(); t.observe_runtime(("a", "1")); t.frozen_probe(); t.frozen_probe()
    ok, d = rej(lambda: t.request_block(("a", "1")), "sin actualización"); case("P2-19", "A2-P2.3", "sin actualización tras la medición no hay bloque (decisiones §55 p. 9)", ok, d)
    u = cls(a2=False); u.observe_runtime(("a", "1")); u.frozen_probe(); u.frozen_probe(); u.observe_runtime(("b", "2"))
    ok, d = rej(lambda: u.request_block(("b", "2")), "sin A-2"); case("P2-20", "A2-P2.3", "sin A-2 no hay bloque", ok, d)
    v = cls(); v.observe_runtime(("a", "1")); v.frozen_probe(); v.observe_runtime(("b", "2"))
    ok, d = rej(lambda: v.request_block(("b", "2")), "no está agotada"); case("P2-21", "A2-P2.scope", "con la fila sin agotar no hay bloque", ok, d)
    return out


# Mutantes: cada uno debe hacer fallar al menos un vector.
class M_ResetOnDispose(F6Budgets):
    def dispose_extraordinary(self, row, by):
        super().dispose_extraordinary(row, by)
        self.counters["attempts"] = 0


class M_NoCoordinatorCheck(F6Budgets):
    def dispose_extraordinary(self, row, by):
        super().dispose_extraordinary(row, "COORDINATOR")


class M_ReuseOldBlock(F6Budgets):
    def block_probe(self, pair, read_only=True):
        if self.blocks and self.epoch not in self.blocks:
            self.blocks[self.epoch] = dict(list(self.blocks.values())[-1], pair=pair)
        super().block_probe(pair, read_only)


class M_IgnoreForbiddenRead(F6Budgets):
    def accredit_invalid_launch(self, run, by):
        self.forbidden.discard(run)
        super().accredit_invalid_launch(run, by)


class M_NoOwnerConsumption(F6Budgets):
    def dispose_extraordinary(self, row, by):
        super().dispose_extraordinary(row, by)
        self.extra_owner.add(row)


class M_SecondBlock(F6Budgets):
    def request_block(self, pair):
        self.blocks.pop(self.epoch, None)
        super().request_block(pair)


class M_PerScenarioOnSharedRow(F6Budgets):
    def dispose_extraordinary(self, row, by):
        if row in self.extra_rows:  # concede otra reejecución en la misma fila compartida (+1 por escenario)
            self.extra_rows.discard(row)
            self.scen_used -= ROWS[row]["serves"]
        super().dispose_extraordinary(row, by)


MUTANTS = [M_ResetOnDispose, M_NoCoordinatorCheck, M_ReuseOldBlock, M_IgnoreForbiddenRead, M_NoOwnerConsumption, M_SecondBlock, M_PerScenarioOnSharedRow]
RULES = ["A2-P1.%d" % i for i in range(1, 7)] + ["A2-P1.scope"] + ["A2-P2.%d" % i for i in range(1, 7)] + ["A2-P2.scope"]


def g5_model(a2_text=None):
    v = vectors()
    mutants = []
    for mcls in MUTANTS:
        mv = []
        try:
            vectors(mcls, mv)
            extra = []
        except Exception as e:  # una excepción inesperada también mata al mutante (tras los vectores ya registrados)
            extra = ["excepción tras %s: %s" % (mv[-1]["Id"] if mv else "inicio", e)]
        killed = [x["Id"] for x in mv if x["Result"] == "FAIL"] + extra
        mutants.append({"Mutant": mcls.__name__, "KilledBy": killed, "Result": "PASS" if killed else "FAIL"})
    covered = sorted({x["Rule"] for x in v})
    missing = [r for r in RULES if r not in covered]
    binding = None
    if a2_text is not None:
        miss = {k: [p for p in ph if p not in a2_text] for k, ph in TEXT_BINDING.items()}
        binding = {"Missing": {k: x for k, x in miss.items() if x}, "Result": "PASS" if not any(miss.values()) else "FAIL"}
    ok = all(x["Result"] == "PASS" for x in v) and all(m["Result"] == "PASS" for m in mutants) and not missing and (binding is None or binding["Result"] == "PASS")
    return {"Check": "G5 modelo de presupuestos", "Total": len(v), "Passed": sum(1 for x in v if x["Result"] == "PASS"), "Vectors": v,
            "Mutants": mutants, "RulesMissing": missing, "TextBinding": binding, "Result": "PASS" if ok else "FAIL"}


def main():
    ap = argparse.ArgumentParser()
    sub = ap.add_subparsers(dest="cmd", required=True)
    r = sub.add_parser("run"); r.add_argument("--repo", required=True); r.add_argument("--head", required=True); r.add_argument("--out", required=True)
    s = sub.add_parser("self-test"); s.add_argument("--out", required=True)
    a = ap.parse_args()
    if a.cmd == "self-test":
        g5 = g5_model()
        res = {"Tool": "a2-guards.py self-test", "G5": g5, "Result": g5["Result"]}
    else:
        head = git(a.repo, "rev-parse", a.head).strip()
        with tempfile.TemporaryDirectory() as td:
            cm, tool = load_clause_map(a.repo, head, td)
            g2 = g2_paths(a.repo, A2_BASE, head)
            checks = [g1_delta_scope(a.repo, head, cm), g2, g3_frozen(a.repo, head), g4_clause_map(a.repo, head, list(g2["Changed"]), cm, tool),
                      g5_model(git(a.repo, "show", "%s:%s" % (head, A2)))]
        res = {"Tool": "a2-guards.py run", "Base": A2_BASE, "Head": head, "Checks": checks,
               "Result": "PASS" if all(c["Result"] == "PASS" for c in checks) else "FAIL"}
    open(a.out, "w", encoding="utf-8", newline="\n").write(json.dumps(res, ensure_ascii=False, indent=1) + "\n")
    print(res["Result"])
    sys.exit(0 if res["Result"] == "PASS" else 1)


if __name__ == "__main__":
    main()
