#!/usr/bin/env python3
"""I-52 CT-21D BA-09 V7: the source closure of the W-1 plan data (BA0809V6-04; AR7-07).

Read-only over the git objects of the repository at the bound SHA 95690c28 (and 3375aadb, informative).

Method (declared; an over-approximation by name):
  * universe: every .cs file under src/RackCad.Application and src/RackCad.Domain at the bound SHA;
  * a type is declared by `class|struct|interface|enum|record <Name>` after removing comments;
  * seeds: the two plan builders of W-1 (SelectiveFrontalBuilder, SelectivePlantaBuilder), the design-to-system
    resolver (SelectiveGeometryResolver) and the catalog provider (JsonRackCatalogProvider);
  * closure: breadth-first over every capitalized identifier of a reached file (comments and string literals
    removed) that names a type declared in the universe; every declaring file is reached.
Output (stdout, JSON): the closure files with their blobs, the directories that hold them, and the W-1 dependency
pairs of BA-09 V7 section 3.2: one tree pair per top directory of the closure (the tree covers its subdirectories)
plus the assets tree. Exit 0.
"""
import collections
import json
import re
import subprocess
import sys

REPO = sys.argv[1] if len(sys.argv) > 1 else r"C:/Users/alejandra-mendoza/.codex/worktrees/feature-rackmirror-espejo-semantico"
BOUND = "95690c28dc6268e61dff32a0cbc33cc9fde3d47f"
OLD = "3375aadb"
SEEDS = [
    "src/RackCad.Application/Systems/Selective/SelectiveFrontalBuilder.cs",
    "src/RackCad.Application/Systems/Selective/SelectivePlantaBuilder.cs",
    "src/RackCad.Application/Systems/Selective/SelectiveGeometryResolver.cs",
    "src/RackCad.Application/Catalogs/JsonRackCatalogProvider.cs",
]
# Top directories bound as tree pairs: a closure file binds the directory two levels under the project
# (src/<Project>/<Area> or src/<Project>/Systems/<Kind>); files directly under the project bind themselves.


def git(*args):
    return subprocess.run(["git", "-C", REPO, *args], capture_output=True, check=True).stdout


def oid(sha, path):
    r = subprocess.run(["git", "-C", REPO, "rev-parse", "--verify", "--quiet", f"{sha}:{path}"], capture_output=True)
    return r.stdout.decode().strip() if r.returncode == 0 else None


def strip(text):
    text = re.sub(r"/\*.*?\*/", "", text, flags=re.S)
    text = re.sub(r"//[^\n]*", "", text)
    return re.sub(r'@?"(?:[^"\\\n]|\\.)*"', '""', text)


def bind_dir(path):
    parts = path.split("/")
    if len(parts) <= 3:
        return path
    if parts[2] == "Systems":
        return "/".join(parts[:4])
    return "/".join(parts[:3])


def main():
    files = [l.split("\t")[1] for l in git("ls-tree", "-r", BOUND, "src/RackCad.Application", "src/RackCad.Domain")
             .decode().splitlines() if l.endswith(".cs")]
    code = {f: strip(git("show", f"{BOUND}:{f}").decode("utf-8-sig")) for f in files}
    decl = collections.defaultdict(set)
    for f, text in code.items():
        for m in re.finditer(r"\b(?:class|struct|interface|enum|record)\s+([A-Z]\w*)", text):
            decl[m.group(1)].add(f)
    seen, queue = set(SEEDS), list(SEEDS)
    while queue:
        f = queue.pop()
        for tok in set(re.findall(r"\b[A-Z]\w*\b", code[f])):
            for g in decl.get(tok, ()):
                if g not in seen:
                    seen.add(g)
                    queue.append(g)
    closure = sorted(seen)
    pairs = sorted({bind_dir(f) for f in closure}) + ["assets"]
    out = {
        "check": "I-52-ct21d-ba09-v7-w1-closure",
        "boundSha": BOUND,
        "seeds": SEEDS,
        "closureCount": len(closure),
        "closure": [{"path": f, "blobAtBound": oid(BOUND, f), "blobAt3375aadb": oid(OLD, f)} for f in closure],
        "selectiveApplicationFiles": [f for f in closure if f.startswith("src/RackCad.Application/Systems/Selective/")],
        "w1DependencyPairs": [{"path": p, "object": "tree" if p == "assets" or not p.endswith(".cs") else "blob",
                               "idAtBound": oid(BOUND, p), "idAt3375aadb": oid(OLD, p)} for p in pairs],
        "closureFilesChangedSince3375aadb": [f for f in closure if oid(BOUND, f) != oid(OLD, f)],
    }
    json.dump(out, sys.stdout, indent=1)
    sys.stdout.write("\n")
    return 0


if __name__ == "__main__":
    sys.exit(main())
