"""Read-only sweep of overload resolution (section 4.1 of the PB-T3 package, v4.6; F4B-E45): for every existing method or constructor that receives a
new overload, or a changed signature, in PB-T3b or PB-T3c, every existing use in src/ and tests/ at <rev>, with what could change how the compiler
binds it: an argument that is null, null!, default or default! at a position where the old and the new parameter types differ (the CS0121 of the
attempt 1 of PB-T3b); a method group (a delegate conversion picks by the delegate type, and C# 10 natural types fail with two candidates); a
reflection lookup by name (GetMethod("X") throws AmbiguousMatchException with two candidates); and a conversion between the old and the new
parameter types (inheritance, an implicit operator or dynamic). It also counts the new names (they have to be free), and replays the v4.5 names as a
positive control: it has to find the two calls the compiler rejected (WorkspacePilotUpdateRunTests.cs:669 and :799).
Usage: python -I overload_sweep.py <main checkout> <rev> <out.txt>
It only calls `git ls-tree` and `git cat-file --batch` (no worktree, no write to git) and writes the report to <out.txt>."""
import re
import subprocess
import sys

repo, rev, out_path = sys.argv[1:4]


def git(*args):
    return subprocess.run(["git", "-C", repo] + list(args), capture_output=True, check=True).stdout.decode("utf-8", "replace")


paths = [p for p in git("ls-tree", "-r", "--name-only", rev, "src", "tests").split("\n") if p.endswith(".cs")]
batch = subprocess.run(["git", "-C", repo, "cat-file", "--batch"], input="".join("%s:%s\n" % (rev, p) for p in paths).encode("utf-8"),
                       capture_output=True, check=True).stdout
TEXT, pos = {}, 0
for p in paths:
    head_end = batch.index(b"\n", pos)
    size = int(batch[pos:head_end].split()[2])
    TEXT[p] = batch[head_end + 1:head_end + 1 + size].decode("utf-8-sig", "replace").replace("\r\n", "\n")
    pos = head_end + 1 + size + 1
assert len(TEXT) == len(paths)

STRING = re.compile(r'@"(?:[^"]|"")*"|\$@"(?:[^"]|"")*"|@\$"(?:[^"]|"")*"|\$?"(?:[^"\\\n]|\\.)*"|\'(?:[^\'\\\n]|\\.)\'')


def code_of(text):
    """The code without comments and with every string or char literal emptied, keeping the line structure (line numbers stay valid)."""
    out, i, n = [], 0, len(text)
    while i < n:
        if text.startswith("//", i):
            j = text.find("\n", i)
            j = n if j < 0 else j
            i = j
            continue
        if text.startswith("/*", i):
            j = text.find("*/", i + 2)
            j = n if j < 0 else j + 2
            out.append("\n" * text.count("\n", i, j))
            i = j
            continue
        m = STRING.match(text, i)
        if m:
            out.append('""' + "\n" * m.group(0).count("\n"))
            i = m.end()
            continue
        out.append(text[i])
        i += 1
    return "".join(out)


CODE = {p: code_of(t) for p, t in TEXT.items()}


def line_of(text, index):
    return text.count("\n", 0, index) + 1


def args_at(code, open_paren):
    """The top-level arguments of the call whose '(' is at open_paren, and the index after its ')'."""
    stack, i, start, args = [], open_paren, open_paren + 1, []
    pairs = {")": "(", "]": "[", "}": "{"}
    while i < len(code):
        ch = code[i]
        if ch in "([{":
            stack.append(ch)
        elif ch == "<" and i > 0 and re.match(r"[\w]", code[i - 1]) and re.match(r"<[A-Z(]", code[i:i + 2]):
            stack.append("<")  # a generic argument list (a comparison has a space or a lower-case operand)
        elif ch == ">" and stack and stack[-1] == "<":
            stack.pop()
        elif ch in pairs:
            while stack and stack[-1] == "<":
                stack.pop()
            if not stack or stack[-1] != pairs[ch]:
                return None, i
            stack.pop()
            if not stack:
                args.append(code[start:i].strip())
                return ([] if args == [""] else args), i + 1
        elif ch == "," and stack == ["("]:
            args.append(code[start:i].strip())
            start = i + 1
        i += 1
    return None, i


UNTYPED = re.compile(r"^(?:null|default)!?$")
OUT = []
PB = "src/RackCad.Application/Systems/PushBack/Pilot/"
UR, PP, WR = PB + "PushBackPilotUpdateRun.cs", PB + "PushBackPilotUpdatePlan.cs", "src/RackCad.Plugin/Views/RackPilotUpdateWriter.cs"
# (id, subtask, owner file, owner type, member, kind, the existing parameter types, the new parameter types). kind: 'method' or 'ctor'. A None
# list of new types is a changed signature (every existing call has to change, and there has to be none outside the owner file).
TARGETS = [
    ("T3B-1", "PB-T3b", UR, "PushBackPilotUpdateRun", "Precheck", "method",
     ["DraftSession<PushBackPilotDraftState>"], ["DraftSession<PushBackFullDraftState>"]),
    ("T3B-3", "PB-T3b", PP, "PushBackPilotUpdatePlan", "PushBackPilotUpdatePlan", "ctor",
     ["string", "string", "IEnumerable<DraftStaleBaseEntry>", "string?", "double", "IEnumerable<PushBackPilotMemberPlan>", "PushBackPilotPostExpectation"],
     ["string", "string", "string", "IEnumerable<DraftStaleBaseEntry>", "string?", "double", "IEnumerable<PushBackPilotMemberPlan>", "IEnumerable<string>",
      "PushBackPilotPostExpectation"]),
    ("T3B-4", "PB-T3b", PP, "PushBackPilotPostExpectation", "PushBackPilotPostExpectation", "ctor",
     ["double", "IEnumerable<PushBackPilotKeyedText>"], ["double", "IEnumerable<PushBackPilotKeyedText>", "IEnumerable<string>"]),
    ("T3C-1", "PB-T3c", WR, "RackPilotMutationScope", "RackPilotMutationScope", "ctor",
     ["Database", "Transaction", "IReadOnlyDictionary<string, (PreparedViewRedraw Prepared, DimensionViewKind Kind)>", "HashSet<ObjectId>"], None),
]
# the v4.5 names, the positive control: what the compiler rejected in the attempt 1 (CS0121 at :669 and :799)
CONTROL = [
    ("C-1", "PB-T3b v4.5", UR, "PushBackPilotUpdateRun", "Execute", "method",
     ["DraftSession<PushBackPilotDraftState>", "PushBackPilotUpdateRequest", "IPushBackPilotUpdatePort"],
     ["DraftSession<PushBackFullDraftState>", "PushBackPilotUpdateRequest", "IPushBackPilotUpdatePort"]),
    ("C-2", "PB-T3b v4.5", UR, "PushBackPilotUpdateRun", "ComparePost", "method",
     ["PushBackPilotUpdatePlan", "PushBackPilotIntentResult?"], ["PushBackPilotUpdatePlan", "PushBackFullIntentResult?"]),
    ("C-3", "PB-T3b v4.5", PP, "PushBackPilotUpdatePlanner", "Prepare", "method",
     ["PushBackPilotPrepareInput", "PushBackPilotIntentResult", "RackCatalog"], ["PushBackFullPrepareInput", "PushBackFullIntentResult", "RackCatalog"]),
]
# the names that PB-T3b and PB-T3c add (PBT3B-C13 and PBT3C-C13 of the v4.6, with the names of F4B-E45): free at rev in the files they go to
NEW_NAMES = [(n, [PP, UR]) for n in ("PrepareFull", "ExecuteFull", "ComparePostFull", "ComparePostCore", "PushBackFullPrepareInput", "BuildCore", "BuildFull",
                                     "BlockedPrefix", "FrontalBOfOneWayPrefix", "FrontalBOfOneWaySuffix", "AllViewsObsoleteReason", "Erasures",
                                     "PostOriginKey", "ErasedKeys", "EraseUnsupportedReason", "PostErasedStillPresentPrefix", "_prepare", "_originOf",
                                     "_postRead", "_origin")] + \
            [(n, [PP, UR, WR]) for n in ("IPushBackPilotViewEraser", "EraseUnit")] + \
            [(n, [WR]) for n in ("RackViewBlockEraser", "EraseInTransaction", "_erasures")] + \
            [("PrecheckFull", [PP, UR])]  # the alternative name for Precheck, if the session renames it too (not prescribed)
# the types whose members PB-T3b or PB-T3c change: the reflection over them in tests/ (typeof(T) and what follows it)
TOUCHED_TYPES = ["PushBackPilotUpdateRun", "PushBackPilotUpdatePlanner", "PushBackPilotUpdatePlan", "PushBackPilotPostExpectation",
                 "PushBackPilotPrepareInput", "RackPilotMutationScope", "RackPilotUpdateScope", "RackPilotUpdateWriter", "IPushBackPilotMutationScope"]
OVERLOADED_V45 = {"Precheck": "PushBackPilotUpdateRun", "Execute": "PushBackPilotUpdateRun", "ComparePost": "PushBackPilotUpdateRun", "Prepare": "PushBackPilotUpdatePlanner"}  # the names that the v4.5 overloaded, with their type


def decls(owner_file, owner_type, member, kind):
    """The existing declarations of the member in its owner file, with their parameter lists."""
    code = CODE[owner_file]
    if kind == "ctor":
        rx = re.compile(r"(?m)^[ \t]*(?:public|internal|private|protected)[\w \t]*\b" + re.escape(member) + r"\s*\(")
    else:
        rx = re.compile(r"(?m)^[ \t]*(?:public|internal|private|protected)[\w \t<>,?\[\]]*[ \t]" + re.escape(member) + r"\s*\(")
    out = []
    for m in rx.finditer(code):
        a, _ = args_at(code, m.end() - 1)
        out.append((line_of(code, m.start()), a))
    return out


def differing(old, new):
    return [k for k in range(len(old)) if k < len(new) and old[k].rstrip("?") != new[k].rstrip("?")]


def sweep(t):
    tid, sub, owner_file, owner_type, member, kind, old, new = t
    OUT.append("")
    OUT.append("== %s (%s) %s.%s%s" % (tid, sub, owner_type, member, "" if kind == "method" else " (constructor)"))
    ds = decls(owner_file, owner_type, member, kind)
    OUT.append("   existing declarations in %s: %s" % (owner_file, "; ".join("line %d, %d parameters" % (l, len(a or [])) for l, a in ds) or "none"))
    defaults = [l for l, a in ds if a and any("=" in x for x in a)]
    params = [a for a in (a for _, a in ds)]
    OUT.append("   optional parameters in the existing declarations: %s" % ("lines " + ", ".join(map(str, defaults)) if defaults else "none"))
    if new is None:
        OUT.append("   new: a changed signature (one more parameter): every existing call has to be in the owner file")
    else:
        dif = differing(old, new) if len(old) == len(new) else []
        OUT.append("   new overload: %d parameters%s" % (len(new), (", the same arity: they differ at %s" % ", ".join(str(k) for k in dif)) if len(old) == len(new)
                                                     else " (another arity: a call with %d arguments cannot bind to it)" % len(old)))
    if kind == "ctor":
        rx = re.compile(r"\bnew\s+" + re.escape(member) + r"\s*\(")
    else:
        rx = re.compile(r"(?<![\w.])(?:(" + re.escape(owner_type) + r")\s*\.\s*)?\b" + re.escape(member) + r"\b")
    rows, risky, groups = [], [], []
    for p in sorted(CODE):
        code = CODE[p]
        for m in rx.finditer(code):
            line = line_of(code, m.start())
            if kind == "method":
                qualified = m.group(1) is not None
                if not qualified and p != owner_file:
                    continue  # another member with the same name (only the owner file calls it unqualified; no using static of the owner)
                if re.match(r"\s*\(", code[m.end():]):
                    before = code[max(0, m.start() - 120):m.start()]
                    if re.search(r"(?:public|internal|private|protected)[\w\s<>,?\[\]]*\s$", before):
                        continue  # the declaration itself
                    a, _ = args_at(code, code.index("(", m.end()))
                else:
                    groups.append("%s:%d %s" % (p, line, TEXT[p].split("\n")[line - 1].strip()[:140]))
                    continue
            else:
                a, _ = args_at(code, code.index("(", m.end() - 1))
            a = a or []
            same = new is not None and len(a) == len(new)
            dif = differing(old, new) if (new is not None and len(old) == len(new)) else []
            untyped = [k for k in dif if k < len(a) and UNTYPED.match(a[k])]
            flag = ""
            if new is None:
                flag = "OWNER FILE" if p == owner_file else "OUTSIDE THE OWNER FILE: it stops compiling"
                if p != owner_file:
                    risky.append((p, line))
            elif same and dif and len(untyped) == len(dif):
                flag = "AMBIGUOUS: untyped %s at %s" % ("/".join(a[k] for k in untyped), ", ".join(map(str, untyped)))
                risky.append((p, line))
            elif same:
                flag = "typed at the differing positions: binds by the static type"
            else:
                flag = "%d arguments: only the existing arity" % len(a)
            rows.append("   %s:%d  (%s)  -> %s" % (p, line, ", ".join(x if len(x) < 60 else x[:57] + "..." for x in a), flag))
    OUT.append("   uses (%d):" % len(rows))
    OUT.extend(rows)
    OUT.append("   method groups or nameof (%d):%s" % (len(groups), "" if groups else " none"))
    OUT.extend("     " + g for g in groups)
    lits = []
    for p in sorted(TEXT):
        for k, l in enumerate(TEXT[p].split("\n"), 1):
            for s in STRING.findall(l):
                if re.search(r'^\W*' + re.escape(member) + r'\W*$', s.strip('@$"')):
                    lits.append("%s:%d %s" % (p, k, l.strip()[:140]))
    OUT.append('   string literals that are exactly the name (reflection lookups or text guards) (%d):%s' % (len(lits), "" if lits else " none"))
    OUT.extend("     " + x for x in lits)
    return risky, groups, lits


OUT.append("overload_sweep.py on %s: %d C# files of src/ and tests/" % (rev, len(paths)))
# the conversions between the old and the new parameter types: none of the types may derive from another, and no implicit operator or dynamic
TYPES = ["DraftSession", "PushBackPilotDraftState", "PushBackFullDraftState", "PushBackPilotIntentResult", "PushBackFullIntentResult",
         "PushBackPilotPrepareInput", "PushBackFullPrepareInput"]
OUT.append("")
OUT.append("== conversions between the old and the new parameter types")
for ty in TYPES:
    found = []
    for p, code in CODE.items():
        if not p.startswith("src/"):
            continue
        for m in re.finditer(r"(?m)^[ \t]*(?:public|internal)\s+(?:sealed\s+|static\s+|abstract\s+)*(?:partial\s+)?(class|record|struct|interface)\s+" + ty +
                             r"\b(<[^>]*>)?\s*(:\s*[^{\n]+)?", code):
            found.append("%s:%d %s%s" % (p, line_of(code, m.start()), m.group(0).strip(), "" if "sealed" in m.group(0) or m.group(1) == "struct" else " (NOT sealed)"))
    OUT.append("   %s: %s" % (ty, "; ".join(found) if found else "not declared at rev (new)"))
impl = sorted("%s:%d" % (p, line_of(c, m.start())) for p, c in CODE.items() for m in re.finditer(r"\bimplicit\s+operator\b", c))
dyn = sorted("%s:%d" % (p, line_of(c, m.start())) for p, c in CODE.items() if p.startswith(("src/RackCad.Application", "tests/RackCad.Tests"))
             for m in re.finditer(r"\bdynamic\b", c))
OUT.append("   implicit operators in src/ and tests/: %s" % (", ".join(impl) if impl else "none"))
OUT.append("   dynamic in Application and in the Core tests: %s" % (", ".join(dyn) if dyn else "none"))
RISK, GROUPS, LITS = [], [], []
for t in TARGETS:
    r, g, l = sweep(t)
    RISK += r
    GROUPS += [(t[0], x) for x in g]
    LITS += [(t[0], x) for x in l]
OUT.append("")
OUT.append("== the new names (PBT3B-C13 and PBT3C-C13, with the names of F4B-E45): as a word, in the files they go to (has to be 0), and anywhere")
busy = []
for nm, owners in NEW_NAMES:
    hits = sorted("%s:%d" % (p, k) for p, t in TEXT.items() for k, l in enumerate(t.split("\n"), 1) if re.search(r"(?<![\w])" + re.escape(nm) + r"(?![\w])", l))
    own = [h for h in hits if h.rsplit(":", 1)[0] in owners]
    OUT.append("   %-30s in its files %d; anywhere %d%s" % (nm, len(own), len(hits), (" " + ", ".join(hits[:4]) + (" ..." if len(hits) > 4 else "")) if hits else ""))
    if own:
        busy.append(nm)
OUT.append("")
OUT.append("== reflection over the types whose members change (typeof(T) in tests/, with the 8 lines that follow it)")
REFL_BREAK, REFL_ROWS = [], []
for p in sorted(TEXT):
    if not p.startswith("tests/"):
        continue
    lines = TEXT[p].split("\n")
    for k, l in enumerate(lines):
        for ty in TOUCHED_TYPES:
            if re.search(r"typeof\(\s*" + ty + r"\s*\)", l):
                window = "\n".join(lines[k:k + 9])
                calls = sorted(set(re.findall(r"\.(Get(?:Methods|Method|Properties|Property|Fields|Field|Constructors|Constructor|Members|Member|Interfaces|NestedTypes))\(", window)))
                names = sorted(set(re.findall(r'\bName\s*==\s*"(\w+)"', window)))
                if not calls:
                    continue
                hit = [n for n in names if OVERLOADED_V45.get(n) == ty]
                REFL_ROWS.append("   %s:%d typeof(%s): %s%s%s" % (p, k + 1, ty, ", ".join(calls), ("; by name: " + ", ".join(names)) if names else "",
                                                                ("  -> BREAKS with a second '%s'" % "', '".join(hit)) if hit else ""))
                if hit:
                    REFL_BREAK.append(("%s:%d" % (p, k + 1 + next(i for i, x in enumerate(lines[k:k + 9]) if 'Name == "' + hit[0] + '"' in x)), hit))
OUT.extend(sorted(set(REFL_ROWS)))
OUT.append("")
OUT.append("== positive control: the v4.5 names (the overloads that the attempt 1 of PB-T3b added; Prepare is the third, by reflection)")
CTRL = []
for t in CONTROL:
    r, _, _ = sweep(t)
    CTRL += r
OUT.append("")
OUT.append("== result")
OUT.append("   control (v4.5): %d ambiguous calls: %s" % (len(CTRL), ", ".join("%s:%d" % x for x in CTRL)))
OUT.append("   reflection by name over a v4.5 overload: %s" % ("; ".join("%s (%s)" % (a, ", ".join(b)) for a, b in sorted(set((a, tuple(b)) for a, b in REFL_BREAK)))
                                                                or "none"))
OUT.append("   v4.6 (PB-T3b and PB-T3c): %d ambiguous or broken calls%s; %d method groups; %d exact-name string literals (read one by one above)" % (
    len(RISK), (": " + ", ".join("%s:%d" % x for x in RISK)) if RISK else "", len(GROUPS), len(LITS)))
OUT.append("   new names already used in the files they go to: %s" % (", ".join(busy) if busy else "none"))
open(out_path, "w", encoding="utf-8", newline="\n").write("\n".join(OUT) + "\n")
print("\n".join(OUT[-5:]))
