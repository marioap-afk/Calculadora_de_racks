#!/usr/bin/env python3
"""I-52 CT-21D BA-08 V7: mechanical completeness check of section 3.2 (BA0809V6-02; AR7-07).

Usage: python I-52-ct21d-ba08-v7-completeness-check.py <repo root> [<artifact>]

Read-only. Inputs: the repository root (argument 1, REQUIRED, no default: the top level of a git checkout that holds the
objects below), the BA-08 V7 artifact (argument 2, optional; relative to the repository root or absolute; default
DEFAULT_ARTIFACT below) and the git objects of the repository at the bound SHA 95690c28 (and at 3375aadb and 69daf03a for
the informative columns). A missing repository root, or one that is not the top level of a git checkout, is a usage error
(exit 2).

Rule checked (the rule is stated in BA-08 V7 section 3.2, paragraph "Completeness"):
  every file under src/ that the artifact cites, in any of three forms, is a row of the section 3.2 identity
  table (role W, C or P) or a row of the table "Cited files outside the W and C rows" of section 3.2 (with a
  reason). The three forms are:
    (1) an explicit path ending in .cs, .xaml or .xaml.cs (full, partial under src/, or a bare file name),
    (2) one of the file abbreviations the artifact defines (LHD, SBW, BP, RBD, BLI, RDC, RSC, RSI, RMC, RVP,
        RPWS, SES, RSW, RVBN),
    (3) a type name with at least two capital letters that is declared (class, struct, interface, enum or
        record; comments removed) in exactly the files it resolves to at the bound SHA.
  A file reached only through a cited line range, and not named in one of the three forms, is not counted
  (the decision recorded by the artifact). Every table row must also carry the correct blob at the bound SHA
  (full 40 hex) and the correct abbreviated blobs (or "absent") at 3375aadb and 69daf03a.
Exit code 0 iff no cited file is missing, no token is ambiguous and unresolved by a table, and every row
verifies. Output: JSON on stdout.
"""
import collections
import hashlib
import json
import os
import re
import subprocess
import sys

REPO = None  # set by main() from the required argument <repo root>
DEFAULT_ARTIFACT = "docs/initiatives/I-52-ct21d-baseline-ba-08-selective-kind-baseline-v7.md"
BOUND = "95690c28dc6268e61dff32a0cbc33cc9fde3d47f"
OLD = "3375aadb"
FXB = "69daf03a35c630e453e1d9e98136f128bd0325a4"

ABBR = {
    "LHD": ["src/RackCad.Plugin/Drawing/LateralHeaderDrawer.cs"],
    "SBW": ["src/RackCad.Plugin/Systems/Shared/SystemBlockWriter.cs"],
    "BP": ["src/RackCad.Plugin/Drawing/BlockPlacement.cs"],
    "RBD": ["src/RackCad.Plugin/Systems/Shared/RackBlockData.cs"],
    "BLI": ["src/RackCad.Plugin/Drawing/BlockLibraryImporter.cs"],
    "RDC": ["src/RackCad.Plugin/Systems/Shared/RackDefinitionCreator.cs"],
    "RSC": ["src/RackCad.Plugin/RackSelectivoCommands.cs"],
    "RSI": ["src/RackCad.Plugin/RackSelectivoInsertIntegration.cs"],
    "RMC": ["src/RackCad.Plugin/RackMenuCommands.cs"],
    "RVP": ["src/RackCad.Plugin/Views/RackViewPlacement.cs"],
    "RPWS": ["src/RackCad.Plugin/Views/RackProjectionWriteScope.cs"],
    "SES": ["src/RackCad.Application/Systems/Selective/SelectiveEditorState.cs"],
    "RSW": ["src/RackCad.UI/Systems/Selective/RackSelectiveWindow.xaml",
            "src/RackCad.UI/Systems/Selective/RackSelectiveWindow.xaml.cs"],
    "RVBN": ["src/RackCad.Application/Systems/Shared/RackViewBaseName.cs"],
}
ABBR_PATH = {"RSW.xaml.cs": "src/RackCad.UI/Systems/Selective/RackSelectiveWindow.xaml.cs",
             "RSW.xaml": "src/RackCad.UI/Systems/Selective/RackSelectiveWindow.xaml"}
OUT_HEADER = "| Cited file (under `src/`) | Reason outside the W and C rows |"
ID_HEADER = "| Path (under `src/`) | Role | Blob at `3375aadb` |"


def git(*args):
    return subprocess.run(["git", "-C", REPO, *args], capture_output=True, check=True).stdout


def blob(sha, path):
    r = subprocess.run(["git", "-C", REPO, "rev-parse", "--verify", "--quiet", f"{sha}:{path}"],
                       capture_output=True)
    return r.stdout.decode().strip() if r.returncode == 0 else None


def strip_code(text):
    text = re.sub(r"/\*.*?\*/", "", text, flags=re.S)
    return re.sub(r"//[^\n]*", "", text)


def table_rows(lines, header):
    for i, line in enumerate(lines):
        if line.startswith(header):
            rows = []
            for row in lines[i + 2:]:
                if not row.startswith("|"):
                    break
                cells = [c.strip() for c in row.strip().strip("|").split("|")]
                rows.append(cells)
            return rows
    return None


def unquote(cell):
    return cell.strip().strip("`")


def usage(msg):
    sys.stderr.write("usage error: " + msg + "\n")
    sys.stderr.write("usage: python I-52-ct21d-ba08-v7-completeness-check.py <repo root> [<artifact>]\n")
    sys.exit(2)


def main():
    global REPO
    if len(sys.argv) not in (2, 3):
        usage("the repository root is required")
    root = sys.argv[1]
    top = subprocess.run(["git", "-C", root, "rev-parse", "--show-toplevel"], capture_output=True) if os.path.isdir(root) else None
    norm = lambda s: os.path.normcase(os.path.realpath(s))
    if top is None or top.returncode != 0 or norm(top.stdout.decode("utf-8").strip()) != norm(root):
        usage("%r is not the top level of a git checkout" % root)
    REPO = root
    rel = sys.argv[2] if len(sys.argv) > 2 else DEFAULT_ARTIFACT
    artifact = rel if os.path.isabs(rel) else os.path.join(REPO, rel)
    shown = os.path.relpath(artifact, REPO) if norm(artifact).startswith(norm(REPO) + os.sep) else artifact
    raw = open(artifact, "rb").read()
    doc = raw.decode("utf-8")
    lines = doc.split("\n")
    src_files = [l.split("\t")[1] for l in git("ls-tree", "-r", BOUND, "src").decode().splitlines()
                 if l.endswith((".cs", ".xaml"))]
    decl = collections.defaultdict(set)
    rx = re.compile(r"\b(?:class|struct|interface|enum|record)\s+([A-Z]\w*)")
    for f in src_files:
        if f.endswith(".cs"):
            for m in rx.finditer(strip_code(git("show", f"{BOUND}:{f}").decode("utf-8-sig"))):
                decl[m.group(1)].add(f)

    id_rows = table_rows(lines, ID_HEADER) or []
    out_rows = table_rows(lines, OUT_HEADER) or []
    table = {}
    row_checks = []
    for cells in id_rows:
        path = "src/" + unquote(cells[0])
        role = cells[1]
        b_old, b_bound, b_fxb = unquote(cells[2]), unquote(cells[3]), unquote(cells[4])
        table[path] = role
        exp_bound, exp_old, exp_fxb = blob(BOUND, path), blob(OLD, path), blob(FXB, path)
        ok = (exp_bound == b_bound
              and (b_old == "absent" if exp_old is None else exp_old.startswith(b_old) and len(b_old) >= 8)
              and (b_fxb == "absent" if exp_fxb is None else exp_fxb.startswith(b_fxb) and len(b_fxb) >= 8))
        row_checks.append({"path": path, "role": role, "ok": ok, "bound": exp_bound,
                           "at3375aadb": exp_old, "at69daf03a": exp_fxb})
    declared_out = {"src/" + unquote(c[0]): c[1] for c in out_rows}

    cited = collections.defaultdict(set)
    ambiguous = []
    unresolved_paths = []
    # The rows of the two tables are not citations: they are the tables the citations are checked against.
    table_lines = set()
    for header in (ID_HEADER, OUT_HEADER):
        for i, line in enumerate(lines):
            if line.startswith(header):
                j = i + 2
                while j < len(lines) and lines[j].startswith("|"):
                    table_lines.add(j)
                    j += 1
    work = "\n".join(l for i, l in enumerate(lines) if i not in table_lines)
    for short, full in ABBR_PATH.items():
        for m in re.finditer(re.escape(short) + r"\b", work):
            cited[full].add("abbr:" + short)
    for m in re.finditer(r"(?<![\w/.])((?:[\w.]+/)*[\w.]+?\.(?:xaml\.cs|xaml|cs))\b", work):
        tok = m.group(1)
        if tok in ABBR_PATH or tok.split(".")[0] in ABBR:
            continue
        t = tok[4:] if tok.startswith("src/") else tok
        cands = [f for f in src_files if f == "src/" + t or f.endswith("/" + t)
                 or ("/" in t and f.endswith("." + t))]
        if len(cands) == 1:
            cited[cands[0]].add("path:" + tok)
        elif len(cands) > 1:
            ambiguous.append({"token": tok, "files": cands})
        else:
            unresolved_paths.append(tok)
    for ab, files in ABBR.items():
        if re.search(r"(?<![\w-])" + ab + r"(?![\w-])", work):
            for f in files:
                cited[f].add("abbr:" + ab)
    for tok in sorted(set(re.findall(r"\b[A-Z][A-Za-z0-9]*\b", work))):
        if sum(1 for ch in tok if ch.isupper()) < 2 or tok not in decl:
            continue
        for f in decl[tok]:
            cited[f].add("type:" + tok)

    results = []
    missing = []
    for f in sorted(cited):
        if f in table:
            status = "IN_TABLE:" + table[f]
        elif f in declared_out:
            status = "DECLARED_OUT"
        else:
            status = "MISSING"
            missing.append(f)
        results.append({"file": f, "status": status, "citedAs": sorted(cited[f])})
    uncited_rows = sorted(p for p in table if p not in cited)
    stale_out = sorted(p for p in declared_out if p not in cited)
    bad_rows = [r for r in row_checks if not r["ok"]]
    amb_open = [a for a in ambiguous if not all(f in table or f in declared_out for f in a["files"])]
    ok = not missing and not bad_rows and not amb_open and not stale_out
    report = {
        "check": "I-52-ct21d-ba08-v7-completeness-check",
        "artifact": shown.replace("\\", "/"),
        "artifactSha256": hashlib.sha256(raw).hexdigest(),
        "boundSha": BOUND,
        "identityRows": len(id_rows),
        "identityRowsWC": sum(1 for r in row_checks if r["role"] in ("W", "C")),
        "declaredOutRows": len(declared_out),
        "citedFiles": len(results),
        "missing": missing,
        "badRows": bad_rows,
        "ambiguousOpen": amb_open,
        "ambiguousResolved": [a for a in ambiguous if a not in amb_open],
        "unresolvedPathTokens": sorted(set(unresolved_paths)),
        "tableRowsNotCitedOutsideTheTables_informative": uncited_rows,
        "declaredOutNotCited": stale_out,
        "results": results,
        "verdict": "PASS" if ok else "FAIL",
    }
    json.dump(report, sys.stdout, indent=1, ensure_ascii=False)
    sys.stdout.write("\n")
    return 0 if ok else 1


if __name__ == "__main__":
    sys.exit(main())
