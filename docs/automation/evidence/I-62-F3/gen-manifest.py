"""I-62 F3: deterministic generator of the normative dependency manifest of the frozen Proposal V14 (Anexo B.11, §20.3.3 points 1-7).

Authoring aid (not a runtime component): the runtime never infers edges from prose; it reads the manifest this script writes. The extraction rules
are fixed and declared here, so the result is reproducible and reviewable (B.11: «revisado por el Architect»):

  U1 Units: every section of V14 from «## 0.» on (UnitId = its anchor: «§N», «§N.M», «Anexo X», «X.N»), and inside each section every table row, list
     item, paragraph and fenced block. A row whose first cell is a frozen identifier (C-nn, P-nn, T-n, I-S/I-P/I-H nn, OD-n, OV-I62-nn, FX-nn, MV-n, N-x)
     takes it as UnitId (anchor-qualified if the identifier repeats); a row whose first cell is a backticked field takes «<anchor>#field:<name>»; the
     rest take «<anchor>#row<n>», «#li<n>», «#p<n>» or «#code<n>».
  U2 A section depends on its member units and subsections (DEFINITION).
  R1 Qualified references (AUTOMATION_PLAN, WORKFLOW, LIFECYCLE, PROMPT_TEMPLATES, README, routing.md, ADR-0046 #n, followed by a section) resolve to
     that document's unit; a qualified section that does not exist there but is a §3.1 target resolves to the PROPOSED target.
  R2 «§N…», «Anexo X» and existing «X.N…» resolve in V14 itself (same document).
  R3 An unqualified section absent from V14 that §3.1 maps (16.13, 16.3) resolves to the PROPOSED target.
  R4 A frozen identifier resolves to its defining row (ranges «C-01..C-04», «T10-T22» are expanded). P-01..P-08 and S-nn are I-61 identifiers of
     AUTOMATION_PLAN 16.11 (external unit).
  R5 Any other unqualified section with a candidate in another surface (e.g. a bare «16.4») is AMBIGUOUS_REFERENCE; R6 with no candidate,
     UNRESOLVED_REFERENCE. A whole document without entry set (e.g. «AGENTS» alone) is WHOLE_DOCUMENT_UNBOUNDED. Any of the three makes the unit
     Complete = false (INCOMPLETE_METADATA → UNKNOWN), never a guessed edge.
  E  EdgeKind is descriptive and does not change the closure: invariants → INVARIANT; transitions → STATE_TRANSITION; «solo si/solo con» → ONLY_IF;
     «salvo/excepto/excepción» → EXCEPT; «prevalece/precedencia» → PRECEDENCE; failure ids and fields → DEFINITION; otherwise SUBJECT_TO.
  X  External units (documents other than V14) are entries with Complete = false: their own dependencies are outside this manifest version.
  N  Self-declared non-normative sections (§0 «resumen», §19 «Riesgos», Anexo F «análisis, no ensayo») are not classified: their units are entries
     with Complete = false and no edges, and a reference to them from another unit is informative (point 4) and is not followed.
  D  An identifier defined by more than one row (FX-04a, FX-04b in D.3) has no single canonical definition: a reference to it is AMBIGUOUS_REFERENCE.
  O  Over-approximation: inside a normative unit the extractor cannot tell an informative cross-reference from a controlling one by form alone, so it
     declares every explicit resolvable reference as a control edge. That can only enlarge a closure (more units must be visible to credit a premise),
     never credit one; removing an edge is a decision of the Architect review of the manifest (B.11), not of this script.
  Not followed (bounding principle): lineage tags (A62-, R62-, O62-, C62-, D-nn), GAP-nn, example ids E-n, evidence and decision cross-references.

Usage: python gen-manifest.py <repo root> <output json> <report json>
"""
import json
import re
import subprocess
import sys

REPO, OUT, REPORT = sys.argv[1], sys.argv[2], sys.argv[3]
DOC = "docs/initiatives/I-62-proposal-v14.md"
REV = "4c617e82b32b6c810b68d75fc19472efed22b393"
EXTERNAL_DOCS = {
    "AUTOMATION_PLAN": "docs/AUTOMATION_PLAN.md", "WORKFLOW": "docs/WORKFLOW.md", "LIFECYCLE": "docs/INITIATIVE_LIFECYCLE.md",
    "INITIATIVE_LIFECYCLE": "docs/INITIATIVE_LIFECYCLE.md", "PROMPT_TEMPLATES": "docs/initiatives/PROMPT_TEMPLATES.md",
    "README": "docs/automation/agent-execution/README.md", "routing.md": "docs/automation/agent-execution/routing.md",
}
ADR46 = "docs/adr/0046-protocolo-de-ejecucion-delegada-de-agentes.md"
ID_RE = r"(?:C-\d{2}[abc]?|P-\d{2}|S-\d{2}|T\d{1,2}[ab]?'?|I-[SPH]\d{2}|OD-\d|OV-I62-\d{2}|FX-0\d[ab]?|MV-\d|N-[a-s])"


def git(*args):
    return subprocess.check_output(["git", "-C", REPO] + list(args)).decode("utf-8")


def blob(path):
    return git("rev-parse", "%s:%s" % (REV, path)).strip()


TEXT = git("show", "%s:%s" % (REV, DOC)).split("\n")
DOC_BLOB = blob(DOC)


def anchor_of(line):
    m = re.match(r"^(#{2,4}) (?:Anexo ([A-G]) —|([0-9]+(?:\.[0-9]+)*)\.? |([A-G](?:\.[0-9]+)+) )", line)
    if not m:
        return None, 0
    level = len(m.group(1))
    if m.group(2):
        return "Anexo " + m.group(2), level
    if m.group(3):
        return "§" + m.group(3), level
    return m.group(4), level


# ---------------------------------------------------------------- U1: sections and units
start = next(k for k, l in enumerate(TEXT) if l.startswith("## 0. "))
sections, fence = [], False
for k in range(start, len(TEXT)):
    line = TEXT[k]
    if line.lstrip().startswith("```"):
        fence = not fence
        continue
    if not fence:
        anchor, level = anchor_of(line)
        if anchor:
            sections.append({"anchor": anchor, "level": level, "start": k})
for n, sec in enumerate(sections):
    nxt = next((s["start"] for s in sections[n + 1:]), len(TEXT))
    sec["body_end"] = nxt
    sec["children"] = []
    parent = next((s for s in reversed(sections[:n]) if s["level"] < sec["level"]), None)
    sec["parent"] = parent["anchor"] if parent else None
anchors = [s["anchor"] for s in sections]
assert len(anchors) == len(set(anchors)), "duplicate anchors"
ANCHORS = set(anchors)
for sec in sections:
    if sec["parent"]:
        next(s for s in sections if s["anchor"] == sec["parent"])["children"].append(sec["anchor"])

units = []  # {id, anchor, kind, text, first}
for sec in sections:
    lines = TEXT[sec["start"] + 1:sec["body_end"]]
    k, counters, table_header = 0, {"row": 0, "li": 0, "p": 0, "code": 0}, None
    while k < len(lines):
        line = lines[k]
        if not line.strip():
            k += 1
            table_header = None
            continue
        if line.lstrip().startswith("```"):
            j = k + 1
            while j < len(lines) and not lines[j].lstrip().startswith("```"):
                j += 1
            counters["code"] += 1
            units.append({"anchor": sec["anchor"], "kind": "code", "n": counters["code"], "text": "\n".join(lines[k:j + 1]), "first": None})
            k = j + 1
            continue
        if line.startswith("|"):
            j = k
            while j < len(lines) and lines[j].startswith("|"):
                j += 1
            rows = lines[k:j]
            for r in rows[2:]:
                counters["row"] += 1
                cell0 = r.strip().strip("|").split("|")[0].strip()
                units.append({"anchor": sec["anchor"], "kind": "row", "n": counters["row"], "text": rows[0] + "\n" + r, "first": cell0})
            k = j
            continue
        if re.match(r"^(- |\d+\. )", line):
            j = k + 1
            while j < len(lines) and lines[j].strip() and not re.match(r"^(- |\d+\. |\||#)", lines[j]):
                j += 1
            counters["li"] += 1
            units.append({"anchor": sec["anchor"], "kind": "li", "n": counters["li"], "text": "\n".join(lines[k:j]), "first": None})
            k = j
            continue
        j = k + 1
        while j < len(lines) and lines[j].strip() and not re.match(r"^(- |\d+\. |\||```)", lines[j]):
            j += 1
        counters["p"] += 1
        units.append({"anchor": sec["anchor"], "kind": "p", "n": counters["p"], "text": "\n".join(lines[k:j]), "first": None})
        k = j

# UnitIds
for u in units:
    first = (u["first"] or "").replace("**", "").strip()
    bare = first.strip("`").strip()
    if u["kind"] == "row" and re.fullmatch(ID_RE, bare):
        u["cand"] = bare
    elif u["kind"] == "row" and first.startswith("`"):
        u["cand"] = u["anchor"] + "#field:" + bare
    else:
        u["cand"] = None
counts = {}
for u in units:
    if u["cand"] and not u["cand"].startswith(u["anchor"] + "#"):
        counts[u["cand"]] = counts.get(u["cand"], 0) + 1
for u in units:
    if u["cand"] and counts.get(u["cand"], 0) == 1:
        u["id"] = u["cand"]
    elif u["cand"] and u["cand"].startswith(u["anchor"] + "#"):
        u["id"] = u["cand"]
    elif u["cand"]:
        u["id"] = u["anchor"] + "#" + u["cand"]
    else:
        u["id"] = "%s#%s%d" % (u["anchor"], u["kind"], u["n"])
seen = {}
for u in units:
    if u["id"] in seen:
        seen[u["id"]] += 1
        u["id"] = "%s~%d" % (u["id"], seen[u["id"]])
    else:
        seen[u["id"]] = 1
DEFINED = {u["id"]: u for u in units if re.fullmatch(ID_RE, u["id"])}
DUPLICATED = {c for c, n in counts.items() if n > 1}
NON_NORMATIVE_ROOTS = {"§0", "§19", "Anexo F"}


def non_normative(anchor):
    a = anchor
    while a:
        if a in NON_NORMATIVE_ROOTS:
            return True
        a = next((s["parent"] for s in sections if s["anchor"] == a), None)
    return False


NON_NORMATIVE = {s["anchor"] for s in sections if non_normative(s["anchor"])}
# §3.1 proposed targets
s31 = next(s for s in sections if s["anchor"] == "§3.1")
PROPOSED = []
for r in TEXT[s31["start"] + 1:s31["body_end"]]:
    if r.startswith("|") and not r.startswith("| `FutureDocument`") and not r.startswith("|---"):
        c = [x.strip() for x in r.strip().strip("|").split("|")]
        PROPOSED.append({"FutureDocument": c[0].strip("`"), "FutureAnchor": c[1], "DesignSource": c[2], "State": "PROPOSED", "MaterializedAnchor": None})
for p in PROPOSED:
    if p["FutureDocument"].startswith("ADR sucesor"):
        p["State"], p["MaterializedAnchor"] = "MATERIALIZED", "docs/adr/0048-ejecucion-delegada-portable-roles-binding-y-autoverificacion.md"


def proposed_for(document, section):
    for p in PROPOSED:
        doc = p["FutureDocument"]
        if (document is None or doc == document) and re.match(r"^§?" + re.escape(section) + r"\b", p["FutureAnchor"].replace("§", "")) is not None:
            if section == "16" and not p["FutureAnchor"].startswith("§16,"):
                continue
            return p
    return None


# ---------------------------------------------------------------- external units
EXTERNAL = {}


def external_sections(path):
    if path not in EXTERNAL:
        try:
            t = git("show", "%s:%s" % (REV, path)).split("\n")
        except subprocess.CalledProcessError:
            t = []
        secs = set()
        for line in t:
            m = re.match(r"^#{2,4} (?:([0-9]+(?:\.[0-9]+)*)\.? |([A-G])\. )", line)
            if m:
                secs.add(m.group(1) or m.group(2))
        EXTERNAL[path] = secs
    return EXTERNAL[path]


ext_units = {}


def ext_unit(path, unit_id, anchor):
    key = path + "|" + unit_id
    if key not in ext_units:
        ext_units[key] = {"Document": path, "UnitId": unit_id, "Anchor": anchor, "Revision": {"Commit": REV, "Blob": blob(path)}}
    return ext_units[key]


def unit_ref(u):
    return {"Document": DOC, "UnitId": u["id"], "Anchor": u["anchor"], "Revision": {"Commit": REV, "Blob": DOC_BLOB}}


def section_ref(anchor):
    return {"Document": DOC, "UnitId": anchor, "Anchor": anchor, "Revision": {"Commit": REV, "Blob": DOC_BLOB}}


# ---------------------------------------------------------------- references
def edge_kind(context, target_id):
    c = context.lower()
    if re.match(r"^I-[SPH]\d", target_id or ""):
        return "INVARIANT"
    if re.match(r"^T\d", target_id or ""):
        return "STATE_TRANSITION"
    if "solo si" in c or "solo con" in c:
        return "ONLY_IF"
    if "salvo" in c or "excepto" in c or "excepción" in c:
        return "EXCEPT"
    if "prevalece" in c or "precedencia" in c:
        return "PRECEDENCE"
    if re.match(r"^(P|S)-\d", target_id or "") or "#field:" in (target_id or ""):
        return "DEFINITION"
    return "SUBJECT_TO"


def expand_ranges(text):
    out = []
    for m in re.finditer(r"\b(C|P|I-S|I-P|I-H|OV-I62)-(\d{2})\s*(?:\.\.|-)\s*(?:\1-)?(\d{2})\b", text):
        pre, a, b = m.group(1), int(m.group(2)), int(m.group(3))
        if b > a and b - a <= 40:
            out += ["%s-%02d" % (pre, n) for n in range(a, b + 1)]
    for m in re.finditer(r"\bT(\d{1,2})\s*(?:\.\.|-)\s*T(\d{1,2})\b", text):
        a, b = int(m.group(1)), int(m.group(2))
        if b > a:
            out += ["T%d" % n for n in range(a, b + 1)]
    for m in re.finditer(r"\bMV-(\d)\s*(?:\.\.|-)\s*MV-(\d)\b", text):
        a, b = int(m.group(1)), int(m.group(2))
        out += ["MV-%d" % n for n in range(a, b + 1)]
    return out


def references(u):
    text = u["text"]
    deps, problems = [], []
    masked = text

    def add(target, kind, ctx_end):
        ctx = text[max(0, ctx_end - 60):ctx_end]
        tid = target["Unit"]["UnitId"] if target.get("Kind", "UNIT") == "UNIT" else None
        deps.append((target, edge_kind(ctx, tid or "")))

    # R1 qualified references
    for m in re.finditer(r"\b(AUTOMATION_PLAN|WORKFLOW|INITIATIVE_LIFECYCLE|LIFECYCLE|PROMPT_TEMPLATES|README|routing\.md)(?:\.md)?\)?\s*(?:§\s?)?([0-9]+(?:\.[0-9]+)*|[A-G])\b", text):
        path = EXTERNAL_DOCS[m.group(1)]
        sec = m.group(2)
        if sec in external_sections(path):
            add({"Kind": "UNIT", "Unit": ext_unit(path, "§" + sec, "§" + sec)}, None, m.start())
        else:
            p = proposed_for(path, sec)
            if p:
                add({"Kind": "PROPOSED", "Proposed": p}, None, m.start())
            else:
                problems.append(("UNRESOLVED_REFERENCE", m.group(0)))
        masked = masked[:m.start()] + " " * (m.end() - m.start()) + masked[m.end():]
    for m in re.finditer(r"ADR-0046\s*#(\d)", text):
        add({"Kind": "UNIT", "Unit": ext_unit(ADR46, "Decisión#" + m.group(1), "Decisión")}, None, m.start())
        masked = masked[:m.start()] + " " * (m.end() - m.start()) + masked[m.end():]
    if re.search(r"\bAGENTS(?:\.md)?\b(?!\s*§)", masked) and u["kind"] != "code":
        problems.append(("WHOLE_DOCUMENT_UNBOUNDED", "AGENTS"))
    # R2 / R3 / R5 sections without qualifier
    for m in re.finditer(r"§\s?([0-9]+(?:\.[0-9]+)*)", masked):
        sec = m.group(1)
        if "§" + sec in NON_NORMATIVE:
            continue
        if "§" + sec in ANCHORS:
            if "§" + sec != u["anchor"]:
                add({"Kind": "UNIT", "Unit": section_ref("§" + sec)}, None, m.start())
        else:
            p = proposed_for(None, sec) if sec in ("16.13", "16.3") else None
            if p:
                add({"Kind": "PROPOSED", "Proposed": p}, None, m.start())
            elif any(sec in external_sections(pth) for pth in set(EXTERNAL_DOCS.values())):
                problems.append(("AMBIGUOUS_REFERENCE", m.group(0)))
            else:
                problems.append(("UNRESOLVED_REFERENCE", m.group(0)))
    for m in re.finditer(r"(?<![§\w.])(16\.[0-9]+)(?![0-9])", masked):
        sec = m.group(1)
        p = proposed_for(None, sec) if sec in ("16.13", "16.3") else None
        if p:
            add({"Kind": "PROPOSED", "Proposed": p}, None, m.start())
        else:
            problems.append(("AMBIGUOUS_REFERENCE", sec))
    for m in re.finditer(r"\bAnexo ([A-G])\b(?!\.)", masked):
        a = "Anexo " + m.group(1)
        if a in ANCHORS and a != u["anchor"] and a not in NON_NORMATIVE:
            add({"Kind": "UNIT", "Unit": section_ref(a)}, None, m.start())
    for m in re.finditer(r"(?<![\w.§])([A-G](?:\.[0-9]+)+)(?![\w])", masked):
        a = m.group(1)
        if a in ANCHORS and a != u["anchor"] and a not in NON_NORMATIVE:
            add({"Kind": "UNIT", "Unit": section_ref(a)}, None, m.start())
    # R4 identifiers
    ids = [m.group(0) for m in re.finditer(r"(?<![\w-])" + ID_RE + r"(?![\w-])", text)] + expand_ranges(text)
    for ident in dict.fromkeys(ids):
        if ident == u["id"]:
            continue
        if ident in DUPLICATED:
            problems.append(("AMBIGUOUS_REFERENCE", ident))
        elif ident in DEFINED:
            pos = text.find(ident)
            add({"Kind": "UNIT", "Unit": unit_ref(DEFINED[ident])}, None, max(pos, 0))
        elif re.fullmatch(r"P-0[1-8]|S-\d{2}", ident):
            add({"Kind": "UNIT", "Unit": ext_unit("docs/AUTOMATION_PLAN.md", ident, "§16.11")}, None, max(text.find(ident), 0))
    # dedupe by target identity
    seen_t, out = set(), []
    for target, kind in deps:
        key = json.dumps(target, sort_keys=True)
        if key not in seen_t:
            seen_t.add(key)
            out.append({"Target": {"Kind": target.get("Kind", "UNIT"), "Unit": target.get("Unit"), "Proposed": target.get("Proposed"),
                                   "WholeDocument": None}, "EdgeKind": kind})
    return out, problems


entries, report_units = [], []
for sec in sections:
    if sec["anchor"] in NON_NORMATIVE:
        entries.append({"Source": section_ref(sec["anchor"]), "DependsOn": [], "Complete": False})
        continue
    member_ids = [u for u in units if u["anchor"] == sec["anchor"]]
    deps = [{"Target": {"Kind": "UNIT", "Unit": unit_ref(u), "Proposed": None, "WholeDocument": None}, "EdgeKind": "DEFINITION"} for u in member_ids]
    deps += [{"Target": {"Kind": "UNIT", "Unit": section_ref(c), "Proposed": None, "WholeDocument": None}, "EdgeKind": "DEFINITION"} for c in sec["children"]]
    entries.append({"Source": section_ref(sec["anchor"]), "DependsOn": deps, "Complete": True})
for u in units:
    if u["anchor"] in NON_NORMATIVE:
        entries.append({"Source": unit_ref(u), "DependsOn": [], "Complete": False})
        report_units.append({"UnitId": u["id"], "Problems": ["NON_NORMATIVE_SECTION"]})
        continue
    deps, problems = references(u)
    entries.append({"Source": unit_ref(u), "DependsOn": deps, "Complete": not problems})
    if problems:
        report_units.append({"UnitId": u["id"], "Problems": sorted({p[0] + ": " + p[1] for p in problems})})
for key in sorted(ext_units):
    entries.append({"Source": ext_units[key], "DependsOn": [], "Complete": False})

manifest = {"Schema": "rackcad-normative-dependency-manifest/v1", "Unit": "I-62", "AuthorityRevision": REV,
            "Scope": [DOC] + sorted({e["Document"] for e in ext_units.values()}), "Entries": entries}
with open(OUT, "w", encoding="utf-8", newline="\n") as f:
    json.dump(manifest, f, ensure_ascii=False, separators=(",", ":"))
    f.write("\n")
stats = {"Sections": len(sections), "Units": len(units), "ExternalUnits": len(ext_units), "Entries": len(entries),
         "Edges": sum(len(e["DependsOn"]) for e in entries), "CompleteFalseProposalUnits": len(report_units),
         "ProposedTargets": len(PROPOSED)}
with open(REPORT, "w", encoding="utf-8", newline="\n") as f:
    json.dump({"Stats": stats, "IncompleteUnits": report_units}, f, ensure_ascii=False, indent=1)
    f.write("\n")
print(json.dumps(stats))
