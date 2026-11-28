"""PB-T4a version (over the close of PB-T3c): the names that PB-T4a writes (FromReads, FullRead, ReadIntentAtOrigin, IWorkspaceEditIntentOriginReader
and PushBackOriginByText), in the shapes that D-PBT4-02 and 03 give them, with the names they sit next to as reference. Usage: python -I name_check_t4a.py ...
--- the description of name_check_v46.py:
Read-only: the method of name_check.py (section 4.1 of the v4.5) for the names of F4B-E45, with a neutral control. Every string literal of every
test file that reads product source is tried as a regex and as a substring against sample lines with the candidate name (its declaration and its
calls, as the v4.6 gives them); a literal is SPECIFIC to the name when it overlaps the name and does not overlap a neutral name put in the same place
(generic expressions that extract any identifier hit both, and do not forbid the name). It also tries the old names (the v4.5 overloads) as a
reference.
Usage: python -I name_check_v46.py <main checkout> <rev> <out.txt>
Only calls `git ls-tree` and `git cat-file --batch`."""
import re
import subprocess
import sys

repo, rev, out_path = sys.argv[1:4]
NEUTRAL = "QzNeutralName"
SHAPES = {  # PB-T4a: the shapes that D-PBT4-02 and 03 (v4; the v4.1 is the authority) give the new names
    "FromReads": ["public static EditIntentReadResult {n}(PushBackPilotIntentResult pilotRead, PushBackFullIntentResult fullRead, RackCatalog catalog)",
                  "return EditIntentReadResult.{n}(read.Pilot, read.Full, catalog.Catalog);"],
    "FullRead": ["public PushBackFullIntentResult? {n} { get; }", "var full = read.{n};"],
    "ReadIntentAtOrigin": ["EditIntentReadResult {n}(SessionId session, string rackId, string originText, EditIntentWitness? witness);",
                           "? originReader.{n}(request.Session, request.RackId, draft.CurrentDraftState.Origin, request.Witness)"],
    "IWorkspaceEditIntentOriginReader": ["public interface {n}", "var read = _editIntentReader is {n} originReader"],
    "PushBackOriginByText": ["public static class {n}", "var key = {n}.Choose(staleBase, originText);", "var at = {n}.ReadAt(read.Input, originText, catalog);"],
    # v4.2 (B-08): ReadAt en Draft/, y AtOrigin de RIA solo la llama
    "ReadAt": ["public static (PushBackPilotIntentScan Scan, PushBackPilotIntentResult Pilot, PushBackFullIntentResult Full)? {n}(PushBackPilotIntentScan scan, "
               "string originText, RackCatalog catalog)", "var at = PushBackOriginByText.{n}(read.Input, originText, catalog);"],
}
OLD = {"FromReads": "FromPilot", "FullRead": "PilotRead", "ReadIntentAtOrigin": "ReadIntent", "IWorkspaceEditIntentOriginReader": "IWorkspaceEditIntentReader",
       "PushBackOriginByText": "PushBackPilotIntentReader", "ReadAt": "Read"}


def git(*args, inp=None):
    return subprocess.run(["git", "-C", repo] + list(args), input=inp, capture_output=True, check=True).stdout


paths = [p for p in git("ls-tree", "-r", "--name-only", rev, "tests").decode("utf-8").split("\n") if p.endswith(".cs")]
batch = git("cat-file", "--batch", inp="".join("%s:%s\n" % (rev, p) for p in paths).encode("utf-8"))
TEXT, pos = {}, 0
for p in paths:
    e = batch.index(b"\n", pos)
    size = int(batch[pos:e].split()[2])
    TEXT[p] = batch[e + 1:e + 1 + size].decode("utf-8", "replace").replace("\r\n", "\n")
    pos = e + 1 + size + 1
BROAD = re.compile(r"Src\.AllProduct\(|Src\.Files\(|GetFiles\(|EnumerateFiles\(|Src\.Code\(|ReadAllText\(|Src\.Text\(")
VERBATIM = re.compile(r'@"((?:[^"]|"")*)"')
REGULAR = re.compile(r'(?<!@)"((?:[^"\\\n]|\\.)*)"')
LITS, scanned = [], 0
for path in sorted(TEXT):
    text = TEXT[path]
    if not BROAD.search(text):
        continue
    scanned += 1
    for number, line in enumerate(text.split("\n"), 1):
        if line.strip().startswith("//"):
            continue
        lits = [m.replace('""', '"') for m in VERBATIM.findall(line)]
        lits += [bytes(m, "utf-8").decode("unicode_escape", "replace") for m in REGULAR.findall(line.replace('@"', '@ "'))]
        for lit in lits:
            if len(lit) < 2:
                continue
            try:
                pat = re.compile(lit.replace("(?<", "(?P<").replace("(?P<=", "(?<=").replace("(?P<!", "(?<!"))
            except re.error:
                pat = None
            LITS.append((path, number, lit, pat))


def hits(name, shapes):
    out = set()
    for path, number, lit, pat in LITS:
        for shape in shapes:
            sample = shape.replace('{n}', name)
            start = sample.find(name)
            end = start + len(name)
            spans = [(m.start(), m.end()) for m in pat.finditer(sample) if m.end() > m.start()] if pat is not None else []
            i = sample.find(lit)
            while i >= 0:
                spans.append((i, i + len(lit)))
                i = sample.find(lit, i + 1)
            if any(a < end and b > start for a, b in spans):
                out.add((path, number, lit))
    return out


OUT = ["name_check_t4a.py on %s: %d test files that read code, %d literals tried" % (rev, scanned, len(LITS))]
for name, shapes in SHAPES.items():
    mine, neutral, old = hits(name, shapes), hits(NEUTRAL, shapes), hits(OLD[name], shapes)
    spec = sorted(mine - neutral)
    spec_old = sorted(old - neutral)
    OUT.append("")
    OUT.append("== %s: %d overlapping literals, %d also on the neutral name, %d SPECIFIC" % (name, len(mine), len(mine & neutral), len(spec)))
    for path, number, lit in spec:
        OUT.append("   %s:%d %r" % (path, number, lit[:110]))
    OUT.append("   reference, the old name %s in the same places: %d specific" % (OLD[name], len(spec_old)))
    for path, number, lit in spec_old:
        OUT.append("     %s:%d %r" % (path, number, lit[:110]))
open(out_path, "w", encoding="utf-8", newline="\n").write("\n".join(OUT) + "\n")
print("\n".join(l for l in OUT if l.startswith(("name_check", "=="))))
