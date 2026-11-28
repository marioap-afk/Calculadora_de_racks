"""Read-only check of the invariants of PB-T4a over git diff BaseSha <ref> (INV-PBT4A-01, 04, 05, 07, 13 and the added lines of 14).
Usage: python -I inv_check.py <worktree> <BaseSha> <RedSha or -> <ref: WT for the working tree, or a commit SHA> <out.txt>
It only reads (git diff, git show and the files of the working tree) and writes its report to <out.txt> in UTF-8."""
import re
import subprocess
import sys

repo, base, red, ref, out_path = sys.argv[1:6]
OUT = []


def say(line=""):
    OUT.append(line)


def git(*args):
    return subprocess.run(["git", "-C", repo] + list(args), capture_output=True, check=True).stdout.decode("utf-8", "replace")


def text_at(rev, path):
    try:
        return git("show", rev + ":" + path)
    except subprocess.CalledProcessError:
        return None


def text_now(path):
    if ref == "WT":
        try:
            with open(repo + "/" + path, "rb") as handle:
                return handle.read().decode("utf-8", "replace")
        except OSError:
            return None
    return text_at(ref, path)


def lf(text):
    return None if text is None else text.replace("\r\n", "\n")


diff_target = [] if ref == "WT" else [ref]
WHC = "src/RackCad.Application/Workspace/WorkspaceHostCore.cs"
WEI = "src/RackCad.Application/Workspace/WorkspaceEditIntent.cs"
RIA = "src/RackCad.Plugin/Views/RackPilotIntentAdapter.cs"
NH = "src/RackCad.Plugin/Workspace/WorkspaceNavigationHost.cs"
IFACE = "src/RackCad.Application/Workspace/IWorkspaceEditIntentOriginReader.cs"
ORIG = "src/RackCad.Application/Systems/PushBack/Draft/PushBackOriginByText.cs"
SKIP = "tests/RackCad.Tests/PushBackDraftWriteGuardTests.cs"
SIX = [WEI, IFACE, ORIG, WHC, RIA, NH]

# ---------------------------------------------------------------------------------------------------- INV-PBT4A-01
say("== INV-PBT4A-01: git diff --name-status %s %s -- src tests" % (base[:8], ref))
status = git("diff", "--name-status", base, *(diff_target + ["--", "src", "tests"])).strip().split("\n")
for line in sorted(status):
    say("  " + line)
if red != "-":
    say("== git diff --name-only %s %s -- src tests (the GREEN)" % (red[:8], ref))
    for line in sorted(git("diff", "--name-only", red, *(diff_target + ["--", "src", "tests"])).strip().split("\n")):
        if line:
            say("  " + line)
say("== git diff --name-only %s -- src/RackCad.UI src/RackCad.Domain src/RackCad.Plugin/RackPushBackCommands.cs src/RackCad.Plugin/RackCommandSupport.cs (must be empty)" % base[:8])
empty = git("diff", "--name-only", base, *(diff_target + ["--", "src/RackCad.UI", "src/RackCad.Domain", "src/RackCad.Plugin/RackPushBackCommands.cs", "src/RackCad.Plugin/RackCommandSupport.cs"])).strip()
say("  " + ("EMPTY" if not empty else "NOT EMPTY: " + empty))

# ---------------------------------------------------------------------------------------------------- INV-PBT4A-05
say("== INV-PBT4A-05: WHC is BaseSha with the five sentences changed, whitespace collapsed, comments kept")
OLD = "var read = _editIntentReader!.ReadIntent(request.Session, request.RackId, request.ChosenReference, request.Witness);"
NEW = ("var read = _editIntentReader is IWorkspaceEditIntentOriginReader originReader "
       "? originReader.ReadIntentAtOrigin(request.Session, request.RackId, draft.CurrentDraftState.Origin, request.Witness) "
       ": _editIntentReader!.ReadIntent(request.Session, request.RackId, request.ChosenReference, request.Witness);")


def collapse(text):
    return " ".join(text.split())


base_whc = collapse(lf(text_at(base, WHC)))
now_whc = collapse(lf(text_now(WHC)))
first = base_whc.find(OLD)
expected = base_whc[:first + len(OLD)] + base_whc[first + len(OLD):]  # the first one (Begin) stays
head, tail = base_whc[:first + len(OLD)], base_whc[first + len(OLD):]
expected = head + tail.replace(OLD, NEW)
say("  sentences of BaseSha: %d (Begin + 5); sentences of the original left now: %d; of the new block now: %d" % (base_whc.count(OLD), now_whc.count(OLD), now_whc.count(NEW)))
if expected == now_whc:
    say("  WHC == BaseSha with the five sentences replaced: EQUAL")
else:
    n = 0
    while n < min(len(expected), len(now_whc)) and expected[n] == now_whc[n]:
        n += 1
    say("  DIFFERENT; first difference at %d: expected [%s] now [%s]" % (n, expected[n:n + 80], now_whc[n:n + 80]))
say("  Find<PushBackPilotDraftState>: base %d, now %d" % (base_whc.count("Find<PushBackPilotDraftState>"), now_whc.count("Find<PushBackPilotDraftState>")))

# ---------------------------------------------------------------------------------------------------- INV-PBT4A-04
say("== INV-PBT4A-04: counts over the WHOLE text of each file, comments included (BaseSha -> now)")
COUNTS = {
    RIA: ["LockDocument", "StartTransaction", "CaptureDrawing(", "PushBackPilotIntentReader.Read(", "new PushBackPilotIntentScan(", "PushBackFullDraftReader.Read(", "PushBackOriginByText.ReadAt("],
    NH: ["LockDocument", "RackPilotIntentAdapter.Read(", "RackPilotIntentAdapter.LoadCatalog()", "RackPilotIntentAdapter.AtOrigin("],
    WHC: ["Find<PushBackPilotDraftState>"],
}
for path, needles in COUNTS.items():
    b, n = lf(text_at(base, path)), lf(text_now(path))
    for needle in needles:
        say("  %s %s: %d -> %d" % (path.rsplit("/", 1)[1], needle, b.count(needle), n.count(needle)))
SIG = ("internal static RackPilotIntentReadResult Read(", "Document document,", "string rackId,", "long originDefinitionHandle,", "string? witnessKey,", "string? witnessText,", "RackCatalog catalog)")
ria_now = collapse(lf(text_now(RIA)))
say("  signature of RackPilotIntentAdapter.Read unchanged: %s" % (" ".join(SIG) in ria_now))
nh_now = lf(text_now(NH))
say("  ReadOnActiveDocument gains originText: %s" % ("private EditIntentReadResult ReadOnActiveDocument(SessionId session, string rackId, long? chosenReference, EditIntentWitness witness, string originText)" in nh_now))

# ---------------------------------------------------------------------------------------------------- INV-PBT4A-07
say("== the Skip of CT-PBT3A-12: lines of the file that differ from BaseSha")
b_lines, n_lines = lf(text_at(base, SKIP)).split("\n"), lf(text_now(SKIP)).split("\n")
say("  line counts: %d -> %d" % (len(b_lines), len(n_lines)))
changed = [(i + 1, b_lines[i], n_lines[i]) for i in range(min(len(b_lines), len(n_lines))) if b_lines[i] != n_lines[i]]
say("  changed lines: %d" % len(changed))
for number, old, new in changed:
    say("  %d: [%s] -> [%s]" % (number, old.strip(), new.strip()))

# ---------------------------------------------------------------------------------------------------- INV-PBT4A-13 the census
STRING = re.compile(r'@"(?:[^"]|"")*"|\$@"(?:[^"]|"")*"|@\$"(?:[^"]|"")*"|\$?"(?:[^"\\\n]|\\.)*"|\'(?:[^\'\\\n]|\\.)\'')


def code_of(text):
    out, i, n = [], 0, len(text)
    while i < n:
        if text.startswith("//", i):
            j = text.find("\n", i)
            i = n if j < 0 else j
            continue
        if text.startswith("/*", i):
            j = text.find("*/", i + 2)
            j = n if j < 0 else j + 2
            out.append("\n" * text.count("\n", i, j))
            i = j
            continue
        m = STRING.match(text, i)
        if m:
            out.append('""')
            i = m.end()
            continue
        out.append(text[i])
        i += 1
    return "".join(out)


MODIFIERS = {"public", "private", "internal", "protected", "static", "sealed", "override", "virtual", "new", "async", "readonly", "extern", "unsafe", "abstract", "partial", "const"}
TYPE_WORDS = re.compile(r"\b(class|struct|interface|enum|record)\s+(\w+)")


def strip_attrs(header):
    return re.sub(r"^\s*(?:\[[^\]]*\]\s*)+", "", header)


def skip_block(code, i):
    """index after the '}' that closes the '{' at i"""
    depth = 0
    while i < len(code):
        if code[i] == "{":
            depth += 1
        elif code[i] == "}":
            depth -= 1
            if depth == 0:
                return i + 1
        i += 1
    return i


def skip_to_semicolon(code, i):
    depth = 0
    while i < len(code):
        ch = code[i]
        if ch in "({[":
            depth += 1
        elif ch in ")}]":
            depth -= 1
        elif ch == ";" and depth <= 0:
            return i + 1
        i += 1
    return i


def census(text):
    """[(scope, kind, name, signature)] of the declarations of one file: types, constructors, methods, properties, fields, constants and enum members."""
    code = re.sub(r"(?m)^[ 	]*#.*$", "", code_of(lf(text)))
    out = []

    def walk(i, scope, end_char, in_enum):
        buf = []
        while i < len(code):
            ch = code[i]
            if ch == end_char:
                return i + 1
            if in_enum:
                if ch == ",":
                    name = "".join(buf).strip()
                    if name:
                        out.append((scope, "enum-member", name.split("=")[0].strip(), name))
                    buf = []
                    i += 1
                    continue
                buf.append(ch)
                i += 1
                continue
            if ch == ";":
                header = strip_attrs("".join(buf)).strip()
                if header:
                    classify(scope, header, ";")
                buf = []
                i += 1
                continue
            if ch == "=" and code[i + 1:i + 2] == ">":
                header = strip_attrs("".join(buf)).strip()
                classify(scope, header, "=>")
                i = skip_to_semicolon(code, i + 2)
                buf = []
                continue
            if ch == "=" and "(" not in "".join(buf):
                # a field with an initializer: the declaration ends at the next ';'
                header = strip_attrs("".join(buf)).strip()
                classify(scope, header, "=")
                i = skip_to_semicolon(code, i + 1)
                buf = []
                continue
            if ch == "{":
                header = strip_attrs("".join(buf)).strip()
                m = TYPE_WORDS.search(header)
                if m and "(" not in header.split(m.group(1))[0]:
                    out.append((scope, "type", m.group(2), header))
                    is_enum = m.group(1) == "enum"
                    i = walk(i + 1, scope + "." + m.group(2), "}", is_enum)
                elif header.startswith("namespace"):
                    i = walk(i + 1, scope, "}", False)
                else:
                    classify(scope, header, "{")
                    i = skip_block(code, i)
                    # a property initializer after the accessors
                    j = i
                    while j < len(code) and code[j] in " \n\t\r":
                        j += 1
                    if code[j:j + 1] == "=" and code[j + 1:j + 2] != ">":
                        i = skip_to_semicolon(code, j)
                buf = []
                continue
            buf.append(ch)
            i += 1
        return i

    def classify(scope, header, ender):
        if not header:
            return
        header = " ".join(header.split())
        if header.startswith("using ") or header.startswith("namespace "):
            return
        if "(" in header:
            names_found = [m.group(1) for m in re.finditer(r"(\w+)\s*(?:<[^<>()]*>)?\s*\(", header) if m.group(1) not in MODIFIERS]
            name = names_found[0] if names_found else ""
            kind = "ctor" if name == scope.split(".")[-1] else "method"
            out.append((scope, kind, name, header))
        else:
            name = header.split()[-1] if header.split() else header
            out.append((scope, "field-or-property", name, header))

    walk(0, "", "\0", False)
    return out


def names(entries):
    counts = {}
    for scope, kind, name, _ in entries:
        key = (scope + "." + name).lstrip(".") + " [" + kind + "]"
        counts[key] = counts.get(key, 0) + 1
    return counts


say("== INV-PBT4A-13: the census of declarations of the six code paths, BaseSha vs now (a name with a count; an overload is one more)")
all_added, all_removed = [], []
for path in SIX:
    b_text, n_text = text_at(base, path), text_now(path)
    b_names = names(census(b_text)) if b_text is not None else {}
    n_names = names(census(n_text)) if n_text is not None else {}
    say("-- %s: base %d declarations, now %d" % (path.rsplit("/", 1)[1], sum(b_names.values()), sum(n_names.values())))
    for key in sorted(set(b_names) | set(n_names)):
        before, after = b_names.get(key, 0), n_names.get(key, 0)
        if before != after:
            say("   %s: %d -> %d" % (key, before, after))
            (all_added if after > before else all_removed).append((path.rsplit("/", 1)[1], key, after - before))
    # the signatures that changed with the same name count
    b_sigs = {(s, k, n, h) for s, k, n, h in census(b_text)} if b_text is not None else set()
    n_sigs = {(s, k, n, h) for s, k, n, h in census(n_text)} if n_text is not None else set()
    removed = sorted(b_sigs - n_sigs)
    added = sorted(n_sigs - b_sigs)
    if b_text is not None and (removed or added):
        for s, k, n, h in removed:
            say("   - signature gone: %s.%s: %s" % (s, n, h[:160]))
        for s, k, n, h in added:
            say("   + signature new:  %s.%s: %s" % (s, n, h[:160]))
say("== names added (file, name, +count):")
for item in all_added:
    say("   %s" % (item,))
say("== names removed (file, name, -count) (must be empty):")
for item in all_removed:
    say("   %s" % (item,))

# ---------------------------------------------------------------------------------------------------- INV-PBT4A-14
say("== INV-PBT4A-14: every line added to src/ by git diff -U0 %s %s, with the suspicious ones flagged" % (base[:8], ref))
patch = git("diff", "-U0", base, *(diff_target + ["--", "src"]))
current = None
SUSPECT = re.compile(r"\.(Add|AddRange|Clear|Remove|RemoveAt|Insert|Sort|Reverse)\s*\(|WorkingBaseline|LastModuleReconciliation|\.System\b|\.Structure\b|\+\+|--|\+=|-=")
for line in patch.split("\n"):
    if line.startswith("+++ "):
        current = line[6:]
    elif line.startswith("+") and not line.startswith("+++"):
        flag = " <-- SUSPECT" if SUSPECT.search(line) else ""
        say("   %s: %s%s" % (current.rsplit("/", 1)[-1], line[1:].rstrip(), flag))
with open(out_path, "w", encoding="utf-8", newline="\n") as handle:
    handle.write("\n".join(OUT) + "\n")
