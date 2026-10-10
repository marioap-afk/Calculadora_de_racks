"""I-62 F6 / decisiones §66.3: evaluación mecánica y de solo lectura de AUTOMATION_PLAN 16.13 sobre un repositorio (fixture o candidato).

Ejecuta, contra una revisión MainSha_eval:
  Derive (16.14, «Punto efectivo»): trailer único alcanzable desde MainSha_eval; primer merge first-parent cuyo segundo padre lo alcanza y el primero no.
  Evaluate E1-E3: punto de entrada de WORKFLOW (## 12) y la 16.13 que nombra, en MainSha_eval; descubrimiento (E5).
  Tabla PRE del cuerpo de EFF (bien formada; Claim-Id sin duplicados).
  Validate(M) en EFF, regla por regla (MV-1..MV-7), con los elementos exactos que fallan.
  Opcional: Classify de una unidad (estado /v2 en --state-rev) frente al EFF derivado.
  Opcional: identidad de blobs invalidadores (routing, catálogo, descriptor de codex-cli, mapa, esquema) entre EFF y --compare-rev.

Solo biblioteca estándar. Solo lectura: únicamente `git rev-parse`, `ls-tree`, `show`, `log`, `rev-list`, `merge-base --is-ancestor`, `cat-file`.
Las rutas del mapa, del esquema y la lista cerrada de Superficies se descubren del texto de 16.13 en MainSha_eval (como el arnés C-20c de F4) y se
contrastan con los valores literales de 16.13.

Uso: python -I mv_check.py --repo <ruta git> --main <rev> [--out <json>] [--unit FX-U1 --state-rev <rev>] [--compare-rev <rev>] [--label <txt>]
Salida: 0 = MAP_VALID y Evaluate sin STOP; 1 = cualquier otro veredicto.
"""
import argparse
import json
import re
import subprocess
import sys

TRAILER = "Agent-Protocol-Normative: I-62"
ENTRY_HEADING = "## 12. Coexistencia de protocolos de ejecución delegada (I61/I62)"
LITERAL_R_HEADING = "### 16.13 Compatibilidad de protocolos de ejecución delegada"
LITERAL_MAP = "docs/automation/agent-execution/compatibility/I62-clause-map.json"
LITERAL_SCHEMA = "docs/automation/agent-execution/compatibility/clause-map.schema.json"
LITERAL_SURFACES = ["AGENTS.md", "CLAUDE.md", "docs/AUTOMATION_PLAN.md", "docs/FOUNDATIONS.md", "docs/INITIATIVE_LIFECYCLE.md", "docs/WORKFLOW.md",
                    "docs/adr/", "docs/automation/agent-execution/", "docs/initiatives/PROMPT_TEMPLATES.md"]
POINTER = "Antes de aplicar esta sección, toda unidad aplica §16.13."
H16 = "## 16. Ejecución delegada bajo orden del Coordinator"
H163 = "### 16.3 Lectura de autoridades"
AP = "docs/AUTOMATION_PLAN.md"
WF = "docs/WORKFLOW.md"
INVALIDATORS = ["docs/automation/agent-execution/routing.md", "docs/automation/agent-execution/model-catalog.md",
                "docs/automation/agent-execution/adapters/codex-cli.md", "docs/automation/agent-execution/adapters/claude-cli.md",
                LITERAL_MAP, LITERAL_SCHEMA, AP, WF]

REPO = None


# ------------------------------------------------------------------ git (solo lectura)
def git_b(*args, ok=(0,)):
    r = subprocess.run(["git", "-C", REPO] + list(args), capture_output=True)
    if r.returncode not in ok:
        raise RuntimeError("git %s: %s" % (" ".join(args), r.stderr.decode("utf-8", "replace").strip()))
    return r.returncode, r.stdout


def git(*args):
    return git_b(*args)[1].decode("utf-8")


def rev(r):
    return git("rev-parse", "--verify", r + "^{commit}").strip()


def show(r, path):
    rc, out = git_b("show", "%s:%s" % (r, path), ok=(0, 128))
    return out.decode("utf-8") if rc == 0 else None


def ls_tree(r):
    out = {}
    for line in git("ls-tree", "-r", "--full-tree", r).split("\n"):
        if line:
            meta, path = line.split("\t", 1)
            out[path] = meta.split()[2]
    return out


def blob(r, path):
    rc, out = git_b("rev-parse", "-q", "--verify", "%s:%s" % (r, path), ok=(0, 1, 128))
    return out.decode().strip() if rc == 0 else None


def is_anc(a, b):
    rc, _ = git_b("merge-base", "--is-ancestor", a, b, ok=(0, 1))
    return rc == 0


def body(c):
    return git("show", "-s", "--format=%B", c)


# ------------------------------------------------------------------ Markdown: secciones (16.3 / 16.13; CRLF -> LF; espacios colapsados)
def norm(s):
    return " ".join(s.split())


def heads_of(text):
    lines = text.replace("\r\n", "\n").split("\n")
    heads, fence = [], False
    for i, line in enumerate(lines):
        if line.lstrip().startswith("```"):
            fence = not fence
            continue
        m = None if fence else re.match(r"^(#{1,6}) ", line)
        if m:
            heads.append((i, len(m.group(1)), norm(line)))
    return lines, heads


def sections(text):
    """[(nivel, encabezado normalizado | 'preámbulo', texto normalizado de la sección)]; nivel 0 = texto anterior al primer ##."""
    lines, heads = heads_of(text)
    first2 = next((i for i, lv, _ in heads if lv >= 2), len(lines))
    out = [(0, "preámbulo", norm("\n".join(lines[:first2])))]
    for k, (i, lv, h) in enumerate(heads):
        end = next((j for j, lv2, _ in heads[k + 1:] if lv2 <= lv), len(lines))
        out.append((lv, h, norm("\n".join(lines[i:end]))))
    return out


def match(text, heading):
    if text is None:
        return []
    return [s for s in sections(text) if s[0] > 0 and s[1] == norm(heading)]


def intro(text, heading):
    """Texto normalizado entre el encabezado y su primer subencabezado (o el final de la sección)."""
    lines, heads = heads_of(text)
    for k, (i, lv, h) in enumerate(heads):
        if h == norm(heading):
            nxt = heads[k + 1][0] if k + 1 < len(heads) else len(lines)
            return norm("\n".join(lines[i + 1:nxt]))
    return None


def dup_heads(text):
    seen = {}
    for lv, h, _ in sections(text):
        if lv:
            seen[h] = seen.get(h, 0) + 1
    return sorted(h for h, c in seen.items() if c > 1)


# ------------------------------------------------------------------ JSON Schema (subconjunto 2020-12 con fallo cerrado)
SUPPORTED = {"$schema", "title", "description", "type", "additionalProperties", "properties", "required", "items", "enum", "const", "minItems",
             "maxItems", "uniqueItems", "minLength", "maxLength", "pattern", "minimum", "maximum"}


def type_ok(v, t):
    return {"object": isinstance(v, dict), "array": isinstance(v, list), "string": isinstance(v, str), "null": v is None,
            "boolean": isinstance(v, bool), "integer": isinstance(v, int) and not isinstance(v, bool),
            "number": isinstance(v, (int, float)) and not isinstance(v, bool)}.get(t, False)


def schema_validate(v, s, path="", errs=None):
    errs = [] if errs is None else errs
    unknown = set(s) - SUPPORTED
    if unknown:
        errs.append("%s: palabra clave no soportada %s (fallo cerrado)" % (path or "/", sorted(unknown)))
        return errs
    if "type" in s:
        ts = s["type"] if isinstance(s["type"], list) else [s["type"]]
        if not any(type_ok(v, t) for t in ts):
            errs.append("%s: tipo %s no es %s" % (path or "/", type(v).__name__, ts))
            return errs
    if "enum" in s and v not in s["enum"]:
        errs.append("%s: %r fuera del enum" % (path or "/", v))
    if "const" in s and v != s["const"]:
        errs.append("%s: distinto de const" % (path or "/"))
    if isinstance(v, str):
        if len(v) < s.get("minLength", 0):
            errs.append("%s: minLength" % path)
        if "maxLength" in s and len(v) > s["maxLength"]:
            errs.append("%s: maxLength" % path)
        if "pattern" in s and not re.search(s["pattern"], v):
            errs.append("%s: no casa %s" % (path, s["pattern"]))
    if isinstance(v, (int, float)) and not isinstance(v, bool):
        if "minimum" in s and v < s["minimum"]:
            errs.append("%s: minimum" % path)
        if "maximum" in s and v > s["maximum"]:
            errs.append("%s: maximum" % path)
    if isinstance(v, list):
        if len(v) < s.get("minItems", 0):
            errs.append("%s: minItems" % path)
        if "maxItems" in s and len(v) > s["maxItems"]:
            errs.append("%s: maxItems" % path)
        if s.get("uniqueItems") and len({json.dumps(x, sort_keys=True) for x in v}) != len(v):
            errs.append("%s: uniqueItems" % path)
        if isinstance(s.get("items"), dict):
            for i, x in enumerate(v):
                schema_validate(x, s["items"], "%s/%d" % (path, i), errs)
    if isinstance(v, dict):
        for k in s.get("required", []):
            if k not in v:
                errs.append("%s: falta %s" % (path or "/", k))
        props = s.get("properties", {})
        for k, x in v.items():
            if k in props:
                schema_validate(x, props[k], "%s/%s" % (path, k), errs)
            elif s.get("additionalProperties") is False:
                errs.append("%s: propiedad no admitida %s" % (path or "/", k))
            elif isinstance(s.get("additionalProperties"), dict):
                schema_validate(x, s["additionalProperties"], "%s/%s" % (path, k), errs)
    return errs


# ------------------------------------------------------------------ YAML mínimo (solo mapeos escalares; las secuencias se omiten)
def yaml_min(text):
    root, stack, skip = {}, [(-1, None)], None
    stack[0] = (-1, root)
    for raw in text.replace("\r\n", "\n").split("\n"):
        if not raw.strip() or raw.lstrip().startswith("#"):
            continue
        ind = len(raw) - len(raw.lstrip(" "))
        s = raw.strip()
        if skip is not None:
            if ind > skip or (ind == skip and s.startswith("- ")):
                continue
            skip = None
        if s.startswith("- ") or s == "-":
            skip = ind
            continue
        m = re.match(r"^([^:]+):(?:\s+(.*))?$", s)
        if not m:
            continue
        while stack[-1][0] >= ind:
            stack.pop()
        k, val = m.group(1).strip(), m.group(2)
        cur = stack[-1][1]
        if val is None or val == "":
            cur[k] = {}
            stack.append((ind, cur[k]))
        else:
            val = val.strip()
            if len(val) >= 2 and val[0] == val[-1] and val[0] in "\"'":
                val = val[1:-1]
            cur[k] = None if val in ("null", "~") else val
    return root


def yget(d, dotted):
    for p in dotted.split("."):
        if not isinstance(d, dict):
            return None
        d = d.get(p)
    return d


# ------------------------------------------------------------------ Derive / PRE / Classify
def derive(main):
    log = git("log", "--format=%H%x1f%B%x1e", main)
    cands = []
    for rec in log.split("\x1e"):
        rec = rec.strip("\n")
        if not rec:
            continue
        h, _, b = rec.partition("\x1f")
        if TRAILER in [x.strip() for x in b.split("\n")]:
            cands.append(h.strip())
    res = {"TrailerCommits": cands}
    if not cands:
        res["Result"] = "NONE"
        return res
    if len(cands) > 1:
        res["Result"] = "ACTIVATION_INVALID (trailer duplicado)"
        return res
    t = cands[0]
    fp = git("rev-list", "--first-parent", "--reverse", main).split()
    first = next((c for c in fp if is_anc(t, c)), None)
    if first is None:
        res["Result"] = "ACTIVATION_INVALID (trailer no alcanzable en first-parent)"
        return res
    parents = git("show", "-s", "--format=%P", first).split()
    if len(parents) == 2 and is_anc(t, parents[1]) and not is_anc(t, parents[0]):
        res.update({"Result": "EFF", "EFF": first, "EFF^1": parents[0], "EFF^2": parents[1]})
    else:
        res["Result"] = "ACTIVATION_INVALID (sin merge efectivo derivable: %s)" % first
    return res


def pre_table(eff):
    lines = body(eff).split("\n")
    try:
        i = lines.index("Derived formal claim table:")
    except ValueError:
        return {"Result": "ACTIVATION_INVALID (PRE ausente)"}
    head = [h.strip() for h in lines[i + 1].split("|")] if i + 1 < len(lines) else []
    if "Claim-Id" not in head:
        return {"Result": "ACTIVATION_INVALID (PRE mal formada: sin Claim-Id)", "Header": head}
    k, ids = head.index("Claim-Id"), []
    for row in lines[i + 2:]:
        if not row.strip():
            break
        cells = [c.strip() for c in row.split("|")]
        if len(cells) != len(head):
            return {"Result": "ACTIVATION_INVALID (PRE mal formada: fila)", "Row": row}
        ids.append(cells[k])
    if len(ids) != len(set(ids)):
        return {"Result": "ACTIVATION_INVALID (Claim-Id duplicado)", "ClaimIds": ids}
    return {"Result": "OK", "Header": head, "ClaimIds": ids, "Rows": len(ids)}


def classify(unit, state_rev, eff, pre_ids):
    st = show(state_rev, "docs/automation/state/%s.yml" % unit)
    if st is None:
        return {"Result": "UNKNOWN", "Why": "estado ausente en %s" % state_rev}
    y = yaml_min(st)
    cid = yget(y, "automation_state.claim_id")
    out = {"StateRev": rev(state_rev), "ClaimId": cid, "Schema": y.get("schema"), "EffectiveShaInState": yget(y, "protocol.effective_sha"),
           "EFF": eff}
    if cid and pre_ids.count(cid) == 1:
        out["Result"] = "I61"
        return out
    if y.get("schema") == "rackcad-automation-state/v2":
        if yget(y, "protocol.set") == "rackcad-protocol/I62" and yget(y, "protocol.effective_sha") == eff and yget(y, "protocol.basis.claim_id") == cid:
            g0 = yget(y, "protocol.g0_acceptance.state")
            if g0 == "PENDING":
                out["Result"] = "PENDING_G0"
                return out
            dp, db = yget(y, "protocol.g0_acceptance.decision.path"), yget(y, "protocol.g0_acceptance.decision.blob")
            if g0 == "ACCEPTED" and dp and db:
                rc, raw = git_b("cat-file", "-p", db, ok=(0, 1, 128))      # «blob presente» (16.13, Classify paso 5)
                d = raw.decode("utf-8", "replace") if rc == 0 else ""
                out["DecisionBlobPresent"] = rc == 0
                if rc == 0 and "I62-CLASSIFICATION: I62" in d and "I62-DELEGATED-EXECUTION: I62_DELEGATED" in d and cid in d:
                    out["Result"] = "I62"
                    out["BaseObsolete"] = yget(y, "protocol.basis.claim_parent_contains_effective") == "false"
                    return out
            out["Result"] = "UNKNOWN (paso 6 sin decisión del Coordinator)"
            return out
        out["Result"] = "UNKNOWN (paso 5: protocol.effective_sha / set / claim_id no coinciden con EFF; effective_sha es inmutable desde el BOOTSTRAP)"
        return out
    out["Result"] = "UNKNOWN"
    return out


# ------------------------------------------------------------------ Validate(M) en EFF
def under(p, surf):
    return any(p == s or (s.endswith("/") and p.startswith(s)) for s in surf)


def validate(eff, e1, map_path, schema_path, closed, r_heading):
    r = {}
    t1, t2 = ls_tree(e1), ls_tree(eff)
    # MV-1
    mtext, stext = show(eff, map_path), show(eff, schema_path)
    M, mv1 = None, []
    if mtext is None:
        mv1.append("mapa ausente en EFF: " + map_path)
    else:
        try:
            M = json.loads(mtext)
        except ValueError as e:
            mv1.append("mapa no es JSON: %s" % e)
    if stext is None:
        mv1.append("esquema ausente en EFF: " + schema_path)
    elif M is not None:
        try:
            mv1 += schema_validate(M, json.loads(stext))
        except ValueError as e:
            mv1.append("esquema no es JSON: %s" % e)
    r["MV-1"] = {"pass": not mv1, "fail": mv1, "MapBlob": blob(eff, map_path), "SchemaBlob": blob(eff, schema_path)}
    M = M if isinstance(M, dict) else {}
    # MV-2
    r["MV-2"] = {"pass": M.get("Surfaces") == closed, "fail": [] if M.get("Surfaces") == closed else
                 ["Surfaces del mapa %r != lista cerrada %r" % (M.get("Surfaces"), closed)]}
    surf = closed
    # MV-3
    files = M.get("Files", []) if isinstance(M.get("Files"), list) else []
    fpaths = [f.get("Path") for f in files]
    changed = sorted(p for p in set(t1) | set(t2) if under(p, surf) and t1.get(p) != t2.get(p) and p != map_path)
    deleted = [p for p in changed if p in t1 and p not in t2]
    dups = sorted({p for p in fpaths if fpaths.count(p) > 1})
    listed_unchanged = sorted(set(fpaths) - set(changed))
    changed_unlisted = sorted(set(changed) - set(fpaths))
    mv3 = (["cambiado y no listado: " + p for p in changed_unlisted] + ["listado sin cambiar entre EFF^1 y EFF: " + p for p in listed_unchanged]
           + ["duplicado en Files: " + p for p in dups] + ["borrado (sin clase): " + p for p in deleted])
    r["MV-3"] = {"pass": not mv3, "fail": mv3, "ChangedUnderSurfaces": changed, "FilesCount": len(fpaths)}
    # MV-4
    mv4, kind_deriv = [], []
    for f in files:
        p, k, bb, eb = f.get("Path"), f.get("FileKind"), f.get("BaseBlob"), f.get("EffBlob")
        o1, o2 = t1.get(p), t2.get(p)
        if k == "MODIFIED":
            if bb != o1 or eb != o2 or o1 is None or o2 is None:
                mv4.append("%s MODIFIED: BaseBlob %s vs EFF^1 %s; EffBlob %s vs EFF %s" % (p, bb, o1, eb, o2))
        elif k in ("ADDED", "ENTRY"):
            if bb is not None or eb != o2 or o2 is None:
                mv4.append("%s %s: BaseBlob %s (debe ser null); EffBlob %s vs EFF %s" % (p, k, bb, eb, o2))
            if k == "ENTRY" and p != schema_path:
                mv4.append("%s: ENTRY de archivo fuera de {esquema del mapa}" % p)
            if o1 is not None:
                kind_deriv.append("%s %s: ya existe en EFF^1 (%s); la derivación lo clasificaría %s" % (p, k, o1, "sin cambio" if o1 == o2 else "MODIFIED"))
        else:
            mv4.append("%s: FileKind %r" % (p, k))
    r["MV-4"] = {"pass": not mv4, "fail": mv4, "FileKindDerivation": kind_deriv,
                 "Note": "MV-4 literal; FileKindDerivation (ADDED/ENTRY presentes en EFF^1, R61 b) se informa aparte y no altera el recuento literal"}
    # MV-5
    ents = M.get("Entries", []) if isinstance(M.get("Entries"), list) else []
    modified = [f.get("Path") for f in files if f.get("FileKind") == "MODIFIED"]
    keys = [(e.get("Path"), norm(e.get("Section", ""))) for e in ents]
    mv5 = ["entrada duplicada: %s | %s" % k for k in sorted({k for k in keys if keys.count(k) > 1})]
    mv5 += ["entrada de un archivo no MODIFIED: %s | %s" % (e.get("Path"), e.get("Section")) for e in ents if e.get("Path") not in modified]
    r["MV-5"] = {"pass": not mv5, "fail": mv5}
    # MV-6
    mv6, derived = [], []
    entry_expected = {(AP, norm(r_heading)), (WF, norm(ENTRY_HEADING))}
    entry_found = set()
    for p in modified:
        a, b = show(e1, p), show(eff, p)
        if a is None or b is None:
            mv6.append("%s: ausente en %s" % (p, "EFF^1" if a is None else "EFF"))
            continue
        if not p.endswith(".md"):
            continue
        for t, lab in ((a, "EFF^1"), (b, "EFF")):
            for h in dup_heads(t):
                mv6.append("%s: encabezado duplicado en %s: %s" % (p, lab, h))
        h1 = {(h, lv): s for lv, h, s in sections(a)}
        h2 = {(h, lv): s for lv, h, s in sections(b)}
        want = set()
        for key in set(h1) | set(h2):
            if key in h1 and key in h2:
                if h1[key] != h2[key]:
                    want.add((key[0], key[1], "MODIFIED"))
            elif key in h1:
                want.add((key[0], key[1], "REMOVED"))
            else:
                want.add((key[0], key[1], "ENTRY" if (p, key[0]) in entry_expected else "ADDED"))
        got = {(norm(e.get("Section", "")), e.get("Level"), e.get("Kind")) for e in ents if e.get("Path") == p}
        for x in sorted(want - got, key=str):
            mv6.append("%s: derivada y no listada: %s (nivel %s) %s" % (p, x[0], x[1], x[2]))
        for x in sorted(got - want, key=str):
            mv6.append("%s: listada y no derivada: %s (nivel %s) %s" % (p, x[0], x[1], x[2]))
        derived += [{"Path": p, "Section": x[0], "Level": x[1], "Kind": x[2]} for x in want]
        entry_found |= {(p, norm(e.get("Section", ""))) for e in ents if e.get("Path") == p and e.get("Kind") == "ENTRY"}
    if entry_found != entry_expected:
        mv6.append("ENTRY de secciones %s != %s" % (sorted(entry_found), sorted(entry_expected)))
    by_kind = {k: sum(1 for d in derived if d["Kind"] == k) for k in ("MODIFIED", "REMOVED", "ADDED", "ENTRY")}
    r["MV-6"] = {"pass": not mv6, "fail": mv6, "DerivedEntries": len(derived), "DerivedByKind": by_kind, "MapEntries": len(ents)}
    # MV-7
    mv7 = []
    wfe, ape, ap1 = show(eff, WF), show(eff, AP), show(e1, AP)
    x = match(wfe, ENTRY_HEADING)
    if len(x) != 1:
        mv7.append("punto de entrada de WORKFLOW en EFF: %d apariciones" % len(x))
    elif ("`" + norm(r_heading) + "`") not in x[0][2]:
        mv7.append("el punto de entrada no nombra «%s» por su línea exacta" % r_heading)
    if len(match(ape, r_heading)) != 1:
        mv7.append("16.13 en EFF: %d apariciones" % len(match(ape, r_heading)))
    i16 = intro(ape, H16) if ape else None
    if not (i16 and i16.startswith(POINTER)):
        mv7.append("puntero ausente como primera frase de §16 en EFF")
    s163e, s1631 = match(ape, H163), match(ap1, H163)
    if len(s163e) != 1 or not s163e[0][2].endswith(POINTER):
        mv7.append("puntero ausente como última frase de 16.3 en EFF")
    elif len(s1631) != 1:
        mv7.append("16.3 en EFF^1: %d apariciones" % len(s1631))
    else:
        stripped = s163e[0][2][:-len(POINTER)].strip()
        if stripped != s1631[0][2]:
            mv7.append("16.3 en EFF sin el puntero != 16.3 en EFF^1 (normalizado)")
        if s1631[0][2].endswith(POINTER):
            mv7.append("16.3 en EFF^1 ya lleva el puntero")
    r["MV-7"] = {"pass": not mv7, "fail": mv7}
    failed = ["MV-%d" % i for i in range(1, 8) if not r["MV-%d" % i]["pass"]]
    return r, failed


# ------------------------------------------------------------------ main
def main():
    global REPO
    try:
        sys.stdout.reconfigure(encoding="utf-8")
    except Exception:
        pass
    ap = argparse.ArgumentParser()
    ap.add_argument("--repo", required=True)
    ap.add_argument("--main", required=True)
    ap.add_argument("--out")
    ap.add_argument("--unit")
    ap.add_argument("--state-rev")
    ap.add_argument("--compare-rev")
    ap.add_argument("--label", default="")
    a = ap.parse_args()
    REPO = a.repo
    main_sha = rev(a.main)
    out = {"Label": a.label, "MainRef": a.main, "MainSha_eval": main_sha, "Stops": []}
    # Derive + E1..E3
    d = derive(main_sha)
    out["Derive"] = d
    wf, apm = show(main_sha, WF), show(main_sha, AP)
    X = match(wf, ENTRY_HEADING)
    out["E1"] = {"WorkflowBlob": blob(main_sha, WF)}
    out["E2"] = {"EntryCount": len(X), "EFF": d.get("EFF")}
    if d["Result"] == "NONE":
        out["Verdict"] = "PRE_ACTIVATION" if not X else "ACTIVATION_INVALID (punto de entrada sin EFF)"
    elif d["Result"] != "EFF":
        out["Verdict"] = d["Result"]
    elif len(X) != 1:
        out["Verdict"] = "ENTRY_INVALID (|X| = %d)" % len(X)
    else:
        named = re.findall(r"`(#{2,6} [^`]+)`", X[0][2])
        r_heading = named[0] if len(named) == 1 else None
        R = match(apm, r_heading) if r_heading else []
        out["E3"] = {"NamedHeading": r_heading, "RCount": len(R), "AutomationPlanBlob": blob(main_sha, AP),
                     "NamedEqualsLiteral": r_heading == LITERAL_R_HEADING}
        if len(R) != 1:
            out["Verdict"] = "ENTRY_INVALID (|R| = %d)" % len(R)
        else:
            rtext = R[0][2]
            ticks = re.findall(r"`([^`]+)`", rtext)
            map_path = next((t for t in ticks if t.endswith("-clause-map.json")), None)
            schema_path = next((t for t in ticks if t.endswith("clause-map.schema.json")), None)
            para = rtext[rtext.index("**Superficies** (lista cerrada):"):rtext.index("**Clasificación**")]
            closed = re.findall(r"`([^`]+)`", para)
            out["Discovery"] = {"MapPath": map_path, "SchemaPath": schema_path, "Surfaces": closed,
                                "MatchesLiterals": map_path == LITERAL_MAP and schema_path == LITERAL_SCHEMA and closed == LITERAL_SURFACES}
            eff, e1 = d["EFF"], d["EFF^1"]
            out["PRE"] = pre_table(eff)
            out["EFFSubjects"] = {k: git("show", "-s", "--format=%s", d[k]).strip() for k in ("EFF", "EFF^1", "EFF^2")}
            mv, failed = validate(eff, e1, map_path, schema_path, closed, r_heading)
            out["Validate"] = mv
            out["MapVerdict"] = "MAP_VALID" if not failed else "MAP_INVALID"
            out["MVFailed"] = failed
            if out["PRE"]["Result"] != "OK":
                out["Verdict"] = out["PRE"]["Result"]
            else:
                out["Verdict"] = out["MapVerdict"] if failed else "MAP_VALID (Evaluate E1-E3 sin STOP; Resolve paso 2 superado)"
            if a.unit and a.state_rev:
                out["Classify"] = classify(a.unit, a.state_rev, eff, out["PRE"].get("ClaimIds", []))
            if a.compare_rev:
                c = rev(a.compare_rev)
                out["InvalidatorBlobs"] = {"CompareRev": c, "Rows": [{"Path": p, "AtEFF": blob(eff, p), "AtCompare": blob(c, p),
                                                                      "Equal": blob(eff, p) == blob(c, p)} for p in INVALIDATORS]}
    if a.out:
        with open(a.out, "w", encoding="utf-8", newline="\n") as f:
            f.write(json.dumps(out, ensure_ascii=False, indent=1) + "\n")
    # resumen legible
    print("[%s] MainSha_eval=%s" % (a.label, main_sha))
    print("  Derive: %s %s" % (d["Result"], {k: d[k][:12] for k in ("EFF", "EFF^1", "EFF^2") if k in d}))
    print("  TrailerCommits alcanzables: %s" % [c[:12] for c in d["TrailerCommits"]])
    if "E3" in out:
        print("  E2 |X|=%d  E3 |R|=%d  descubrimiento=literal:%s" % (out["E2"]["EntryCount"], out["E3"]["RCount"],
                                                                     out.get("Discovery", {}).get("MatchesLiterals")))
    if "PRE" in out:
        print("  PRE: %s (filas %s)" % (out["PRE"]["Result"], out["PRE"].get("Rows")))
    if "Validate" in out:
        for i in range(1, 8):
            v = out["Validate"]["MV-%d" % i]
            print("  MV-%d %s%s" % (i, "pass" if v["pass"] else "FAIL", "" if v["pass"] else " (%d)" % len(v["fail"])))
            for e in v["fail"][:40]:
                print("      - " + e)
            if len(v["fail"]) > 40:
                print("      ... (%d más; ver el JSON)" % (len(v["fail"]) - 40))
            if v.get("FileKindDerivation"):
                print("      [informativo] FileKind contra la derivación: %d archivos ADDED/ENTRY ya presentes en EFF^1" % len(v["FileKindDerivation"]))
    if "Classify" in out:
        print("  Classify(%s @ %s) = %s" % (a.unit, out["Classify"].get("StateRev", "")[:12], out["Classify"]["Result"]))
    if "InvalidatorBlobs" in out:
        print("  Blobs invalidadores EFF vs %s: %s" % (out["InvalidatorBlobs"]["CompareRev"][:12],
                                                     "todos iguales" if all(x["Equal"] for x in out["InvalidatorBlobs"]["Rows"]) else
                                                     [x["Path"] for x in out["InvalidatorBlobs"]["Rows"] if not x["Equal"]]))
    print("  VEREDICTO: %s" % out["Verdict"])
    sys.exit(0 if out["Verdict"].startswith("MAP_VALID") else 1)


if __name__ == "__main__":
    main()
