"""I-62 F4 (C-20a/C-20b): derivation of the clause map of Proposal V14 Anexo E.4 (AUTOMATION_PLAN 16.13) and its comparison with the materialized map.

The map is the derivation, never hand-written: for every path under the closed Surfaces whose blob differs between BASE and TIP (the map itself
excluded), one Files row (MODIFIED / ADDED / ENTRY = the map's schema) and, for every MODIFIED Markdown file, one Entries row per section of any level
(the preamble included) that is MODIFIED (same heading, different normalized text), REMOVED or ADDED, with ENTRY for the WORKFLOW entry point and
16.13. Sections and normalization are those of 16.3 / E.4 (fenced lines are not headings; CRLF -> LF; whitespace collapsed).

Usage (from the worktree):
  python clause_map.py write  [BASE]   derive BASE -> index (stage the surfaces first) and write the map, then stage it
  python clause_map.py check  [BASE] [TIP] [OUT.json]   MV-3..MV-6 of the committed map at TIP against the derivation BASE -> TIP (C-20b)
BASE defaults to the merge-base of HEAD and origin/main (EFF^1 at integration); TIP defaults to HEAD.
"""
import json
import os
import re
import subprocess
import sys

MAP_PATH = "docs/automation/agent-execution/compatibility/I62-clause-map.json"
SCHEMA_PATH = "docs/automation/agent-execution/compatibility/clause-map.schema.json"
SURFACES = ["AGENTS.md", "CLAUDE.md", "docs/AUTOMATION_PLAN.md", "docs/FOUNDATIONS.md", "docs/INITIATIVE_LIFECYCLE.md", "docs/WORKFLOW.md", "docs/adr/",
            "docs/automation/agent-execution/", "docs/initiatives/PROMPT_TEMPLATES.md"]
ENTRY_SECTIONS = {("docs/AUTOMATION_PLAN.md", "### 16.13 Compatibilidad de protocolos de ejecución delegada"),
                  ("docs/WORKFLOW.md", "## 12. Coexistencia de protocolos de ejecución delegada (I61/I62)")}


def git(*args):
    r = subprocess.run(["git"] + list(args), capture_output=True)
    if r.returncode != 0:
        raise RuntimeError("git %s: %s" % (" ".join(args), r.stderr.decode("utf-8", "replace").strip()))
    return r.stdout.decode("utf-8")


def show(rev, path):
    """rev = a commit, or None for the index."""
    r = subprocess.run(["git", "show", (rev or "") + ":" + path], capture_output=True)
    return r.stdout.decode("utf-8").replace("\r\n", "\n") if r.returncode == 0 else None


def blob(rev, path):
    r = subprocess.run(["git", "rev-parse", "-q", "--verify", (rev or "") + ":" + path], capture_output=True, text=True)
    return r.stdout.strip() if r.returncode == 0 else None


def norm(s):
    return " ".join(s.split())


def in_surfaces(p):
    return any(p == s or (s.endswith("/") and p.startswith(s)) for s in SURFACES)


def sections(text):
    """(level, normalized heading | 'preámbulo', normalized section text) in document order; level 0 = the text before the first ##."""
    lines = text.split("\n")
    heads, fence = [], False
    for i, line in enumerate(lines):
        if line.lstrip().startswith("```"):
            fence = not fence
            continue
        m = None if fence else re.match(r"^(#{1,6}) ", line)
        if m:
            heads.append((i, len(m.group(1))))
    first2 = next((i for i, lv in heads if lv >= 2), len(lines))
    out = [(0, "preámbulo", norm("\n".join(lines[:first2])))]
    for k, (i, lv) in enumerate(heads):
        end = next((j for j, lv2 in heads[k + 1:] if lv2 <= lv), len(lines))
        out.append((lv, norm(lines[i]), norm("\n".join(lines[i:end]))))
    return out


def duplicates(text):
    seen = {}
    for lv, h, _ in sections(text):
        if lv:
            seen[h] = seen.get(h, 0) + 1
    return sorted(h for h, c in seen.items() if c > 1)


def changed(base, tip):
    args = ["diff", "--name-only", "--no-renames", base] + (["--cached"] if tip is None else [tip])
    return sorted(p for p in git(*args).split("\n") if p and in_surfaces(p) and p != MAP_PATH)


def derive(base, tip):
    """tip = a commit, or None for the index."""
    files, entries, problems = [], [], []
    for p in changed(base, tip):
        b0, b1 = blob(base, p), blob(tip, p)
        if b1 is None:
            problems.append("archivo borrado (sin clase): " + p)
            continue
        kind = "ENTRY" if p == SCHEMA_PATH else ("ADDED" if b0 is None else "MODIFIED")
        files.append({"Path": p, "FileKind": kind, "BaseBlob": b0 if kind == "MODIFIED" else None, "EffBlob": b1})
        if kind != "MODIFIED" or not p.endswith(".md"):
            continue
        t0, t1 = show(base, p), show(tip, p)
        for t, rev in ((t0, "base"), (t1, "tip")):
            if duplicates(t):
                problems.append("encabezados duplicados en %s (%s): %s" % (p, rev, duplicates(t)))
        h1 = {(h, lv): s for lv, h, s in sections(t0)}
        h2 = {(h, lv): s for lv, h, s in sections(t1)}
        for key in sorted(set(h1) | set(h2), key=lambda k: (k[1], k[0])):
            if key in h1 and key in h2:
                if h1[key] != h2[key]:
                    entries.append({"Path": p, "Section": key[0], "Level": key[1], "Kind": "MODIFIED"})
            elif key in h1:
                entries.append({"Path": p, "Section": key[0], "Level": key[1], "Kind": "REMOVED"})
            else:
                entries.append({"Path": p, "Section": key[0], "Level": key[1], "Kind": "ENTRY" if (p, key[0]) in ENTRY_SECTIONS else "ADDED"})
    cmap = {"Schema": "rackcad-clause-map/v1", "Protocol": "rackcad-protocol/I62", "LegacyProtocol": "rackcad-protocol/I61", "Surfaces": SURFACES,
            "Files": files, "Entries": entries}
    return cmap, problems


def dumps(cmap):
    return json.dumps(cmap, ensure_ascii=False, indent=2) + "\n"


def default_base():
    return git("merge-base", "HEAD", "origin/main").strip()


def check(cmap, base, tip):
    """MV-2..MV-6 of a committed map against the derivation (MV-1 and MV-7 are the history-free C-20a guard)."""
    want, problems = derive(base, tip)
    errs = list(problems)
    if cmap.get("Surfaces") != SURFACES:
        errs.append("MV-2")
    fpaths = [f["Path"] for f in cmap.get("Files", [])]
    if sorted(fpaths) != sorted(f["Path"] for f in want["Files"]) or len(fpaths) != len(set(fpaths)):
        errs.append("MV-3")
    by = {f["Path"]: f for f in want["Files"]}
    for f in cmap.get("Files", []):
        w = by.get(f["Path"])
        if w is None or (f["FileKind"], f["BaseBlob"], f["EffBlob"]) != (w["FileKind"], w["BaseBlob"], w["EffBlob"]):
            errs.append("MV-4 " + f["Path"])
    keys = [(e["Path"], e["Section"]) for e in cmap.get("Entries", [])]
    mod = {f["Path"] for f in cmap.get("Files", []) if f["FileKind"] == "MODIFIED"}
    if len(keys) != len(set(keys)) or any(e["Path"] not in mod for e in cmap.get("Entries", [])):
        errs.append("MV-5")
    canon = lambda es: sorted(json.dumps(e, sort_keys=True, ensure_ascii=False) for e in es)
    if canon(cmap.get("Entries", [])) != canon(want["Entries"]):
        errs.append("MV-6")
    return sorted(set(errs)), want


def main():
    mode = sys.argv[1]
    base = sys.argv[2] if len(sys.argv) > 2 else default_base()
    if mode == "write":
        cmap, problems = derive(base, None)
        if problems:
            sys.exit("derivación inválida: " + "; ".join(problems))
        open(MAP_PATH, "w", encoding="utf-8", newline="\n").write(dumps(cmap))
        git("add", MAP_PATH)
        print(json.dumps({"base": base, "files": len(cmap["Files"]), "entries": len(cmap["Entries"])}))
    elif mode == "check":
        tip = sys.argv[3] if len(sys.argv) > 3 else git("rev-parse", "HEAD").strip()
        cmap = json.loads(show(tip, MAP_PATH) or "null")
        errs, want = check(cmap or {}, base, tip)
        result = {"Base": base, "Tip": tip, "MapBlob": blob(tip, MAP_PATH), "Result": errs or "EQUAL (MV-2..MV-6)",
                  "Files": [(f["Path"], f["FileKind"]) for f in want["Files"]],
                  "EntriesByKind": {k: sum(e["Kind"] == k for e in want["Entries"]) for k in ("MODIFIED", "REMOVED", "ADDED", "ENTRY")},
                  "Entries": [(e["Path"], e["Section"], e["Kind"]) for e in want["Entries"]]}
        if len(sys.argv) > 4:
            open(sys.argv[4], "w", encoding="utf-8", newline="\n").write(json.dumps(result, ensure_ascii=False, indent=1) + "\n")
        print(json.dumps({"result": result["Result"], "files": len(result["Files"]), "entries": result["EntriesByKind"]}, ensure_ascii=False))
        sys.exit(1 if errs else 0)


if __name__ == "__main__":
    main()
