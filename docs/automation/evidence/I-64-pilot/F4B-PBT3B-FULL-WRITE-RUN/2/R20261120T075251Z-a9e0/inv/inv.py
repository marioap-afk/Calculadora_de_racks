"""Read-only checks of the invariants of PB-T3b over git objects (INV-PBT3B-01, 03, 05, C-1 and the census of declarations).

Usage (from the root of the worktree): python inv.py <BaseSha> <RedSha-or-empty> [<Sha-of-the-text>|WORKTREE]
It reads git objects and, for WORKTREE, the files of the working tree. It writes nothing.
"""
import re
import subprocess
import sys

PP = "src/RackCad.Application/Systems/PushBack/Pilot/PushBackPilotUpdatePlan.cs"
UR = "src/RackCad.Application/Systems/PushBack/Pilot/PushBackPilotUpdateRun.cs"
ER = "src/RackCad.Application/Systems/PushBack/Pilot/IPushBackPilotViewEraser.cs"
GUARDS = [
    "tests/RackCad.Tests/Workspace/WorkspacePilotPrepareGuardTests.cs",
    "tests/RackCad.Tests/Workspace/WorkspacePilotUpdateGuardTests.cs",
    "tests/RackCad.Tests/Workspace/WorkspacePilotUpdateIntegrationGuardTests.cs",
]
NEW_TESTS = [
    "tests/RackCad.Tests/PushBackFullWritePrepareTests.cs",
    "tests/RackCad.Tests/PushBackFullWriteRunTests.cs",
    "tests/RackCad.Tests/PushBackWritePathGuardTests.cs",
    "tests/RackCad.Tests/PushBackPilotUpdateEquivalenceGuardTests.cs",
    "tests/RackCad.Tests/PushBackFullWriteSupport.cs",
]


def git(*args):
    out = subprocess.run(["git"] + list(args), capture_output=True, check=False)
    return out.stdout.decode("utf-8", errors="replace")


def show(sha, path):
    if sha == "WORKTREE":
        try:
            return open(path, "rb").read().decode("utf-8").replace("\r\n", "\n")
        except FileNotFoundError:
            return ""
    return git("show", sha + ":" + path).replace("\r\n", "\n")


DECL = re.compile(r"^(public|internal|private|protected)\b[^;=]*?(\(|\{|=|;|$)")
MODS = {"public", "internal", "private", "protected", "static", "sealed", "readonly", "const", "override", "abstract", "partial", "new", "unsafe", "virtual", "async"}


def strip_comments(text):
    text = re.sub(r"/\*.*?\*/", " ", text, flags=re.S)
    out = []
    for line in text.split("\n"):
        s = line.strip()
        if s.startswith("//") or s.startswith("///"):
            continue
        out.append(line)
    return "\n".join(out)


def declarations(text, interface_file=False):
    """One entry per declaration: (name, kind, normalized line). Heuristic, line based, all visibilities."""
    result = []
    for raw in strip_comments(text).split("\n"):
        line = raw.strip()
        if not line:
            continue
        if interface_file and re.match(r"^[A-Za-z_<>\[\]?., ]+\s+\w+\s*\([^)]*\)\s*;$", line):
            name = re.match(r"^.*?\s(\w+)\s*\(", line).group(1)
            result.append((name, "method", line))
            continue
        if not DECL.match(line):
            continue
        words = line.split()
        if words[0] not in ("public", "internal", "private", "protected"):
            continue
        # strip the modifiers to find the declared thing
        i = 0
        while i < len(words) and words[i] in MODS:
            i += 1
        rest = " ".join(words[i:])
        kind = "member"
        m = re.match(r"^(class|interface|enum|struct|record(?: struct| class)?)\s+(\w+)", rest)
        if m:
            result.append((m.group(2), "type", line))
            continue
        m = re.match(r"^[\w<>\[\]?,.() ]*?\s?(\w+)\s*(<[^>(]*>)?\s*(\(|\{|=|=>|;|$)", rest)
        name = None
        mm = re.search(r"(\w+)\s*(?:<[^>(]*>)?\s*(?:\(|\{|=>|=|;|$)", rest)
        if rest.startswith("record struct") or rest.startswith("readonly record struct"):
            pass
        if mm:
            name = mm.group(1)
        if name is None:
            continue
        result.append((name, kind, line))
    return result


def names(decls):
    counts = {}
    for name, kind, line in decls:
        counts[name] = counts.get(name, 0) + 1
    return counts


NESTED = re.compile(r"^private\s+sealed\s+class\s+Execution\b")


def public_internal(text, nested=False):
    """The lines that declare a public or internal member. The members that sit inside the PRIVATE nested class Execution are public in
    name only (their accessibility is the one of the class): D-PBT3-10 prescribes their change of signature and INV-PBT3B-03 does not cover
    them, so they are returned apart when nested is True and left out otherwise."""
    lines = set()
    inside = False
    for raw in strip_comments(text).split("\n"):
        line = raw.strip()
        if NESTED.match(line):
            inside = True
        if re.match(r"^(public|internal)\b", line) and inside == nested:
            lines.add(line)
    return lines


def main():
    base = sys.argv[1]
    red = sys.argv[2] if len(sys.argv) > 2 else ""
    cur = sys.argv[3] if len(sys.argv) > 3 else "WORKTREE"
    print("== INV-PBT3B-01 (the scope)")
    if cur != "WORKTREE":
        print(git("diff", "--name-status", base, cur, "--", "src", "tests").strip())
        if red:
            print("-- git diff --name-only RedSha CurrentSha -- src tests:")
            print(git("diff", "--name-only", red, cur, "--", "src", "tests").strip())
        print("-- git diff -U0 BaseSha CurrentSha of the three guards of F4 (the changed lines only):")
        diff = git("diff", "-U0", base, cur, "--", *GUARDS)
        changed = [l for l in diff.split("\n") if (l.startswith("+") or l.startswith("-")) and not l.startswith("+++") and not l.startswith("---")]
        print("\n".join(l[:150] for l in changed))
        skips = [l for l in changed if l.startswith("+") and "Skip" in l]
        print("changed lines:", len(changed), "added with Skip:", len(skips), "removed:", len([l for l in changed if l.startswith("-")]))
    else:
        print(git("status", "--short").strip())
    print()
    print("== INV-PBT3B-03 (every public or internal member of PP and UR at BaseSha exists with the same signature)")
    for path in (PP, UR):
        before = public_internal(show(base, path))
        after = public_internal(show(cur, path))
        missing = sorted(before - after)
        print(path.split("/")[-1], "members at BaseSha:", len(before), "missing in the text:", len(missing))
        for line in missing:
            print("   MISSING:", line[:170])
        nested_before = public_internal(show(base, path), nested=True)
        nested_after = public_internal(show(cur, path), nested=True)
        if nested_before or nested_after:
            print("   members inside the private class Execution (not covered by INV-PBT3B-03; prescribed by D-PBT3-10):",
                  len(nested_before), "at BaseSha,", len(nested_after), "at the text")
            for line in sorted(nested_before - nested_after):
                print("     signature replaced (PushBackPilotDraftState -> TState):", line[:150])
    print()
    print("== C-1 (the three new names and the old ones)")
    pp = show(cur, PP)
    ur = show(cur, UR)
    for label, text, pattern in (
        ("PrepareFull in PP", pp, r"public static PushBackPilotPrepareResult PrepareFull\(PushBackFullPrepareInput input, PushBackFullIntentResult reread, RackCatalog catalog\)"),
        ("ExecuteFull in UR", ur, r"public static DraftApplyOutcome<PushBackFullDraftState> ExecuteFull\("),
        ("ComparePostFull in UR", ur, r"public static string\? ComparePostFull\(PushBackPilotUpdatePlan plan, PushBackFullIntentResult\? postRead\)"),
    ):
        print(label, "declared:", len(re.findall(pattern, text)))
    print("Prepare( with PushBackFullPrepareInput:", len(re.findall(r"\bPrepare\s*\(\s*PushBackFullPrepareInput", pp)))
    print("Execute( with DraftSession<PushBackFullDraftState>:", len(re.findall(r"\bExecute\s*\(\s*DraftSession<PushBackFullDraftState>", ur)))
    print("ComparePost( with PushBackFullIntentResult:", len(re.findall(r"\bComparePost\s*\([^)]*PushBackFullIntentResult", ur)))
    for label, sig_base, text in (
        ("Prepare of the pilot", r"public static PushBackPilotPrepareResult Prepare\(PushBackPilotPrepareInput input, PushBackPilotIntentResult reread, RackCatalog catalog\)", pp),
        ("Precheck of the pilot", r"public static PushBackPilotPrecheck Precheck\(DraftSession<PushBackPilotDraftState> draft\)", ur),
        ("Execute of the pilot", r"public static DraftApplyOutcome<PushBackPilotDraftState> Execute\(", ur),
        ("ComparePost of the pilot", r"public static string\? ComparePost\(PushBackPilotUpdatePlan plan, PushBackPilotIntentResult\? postRead\)", ur),
    ):
        print(label, "present in BaseSha:", len(re.findall(sig_base, show(base, PP if text is pp else UR))), "present in the text:", len(re.findall(sig_base, text)))
    print()
    print("== PBT3B-C13 and the census (declarations by name, every visibility, overloads count)")
    for path, interface_file in ((PP, False), (UR, False), (ER, True)):
        d0 = names(declarations(show(base, path), interface_file))
        d1 = names(declarations(show(cur, path), interface_file))
        total0 = sum(d0.values())
        total1 = sum(d1.values())
        print(path.split("/")[-1], "declarations at BaseSha:", total0, "at the text:", total1, "difference:", total1 - total0)
        added = []
        removed = []
        for n in sorted(set(d0) | set(d1)):
            a, b = d0.get(n, 0), d1.get(n, 0)
            if b > a:
                added.append("%s x%d" % (n, b - a))
            elif b < a:
                removed.append("%s x%d" % (n, a - b))
        print("   added (%d names):" % len(added), ", ".join(added) if added else "-")
        print("   removed:", ", ".join(removed) if removed else "-")
    print()
    print("== the individual census (name, kind) of each file at the text")
    for path, interface_file in ((PP, False), (UR, False), (ER, True)):
        print("-- " + path.split("/")[-1])
        for name, kind, line in declarations(show(cur, path), interface_file):
            print("   %-36s %-7s %s" % (name, kind, line[:110]))
    print()
    print("== INV-PBT3B-05 (the guards that follow)")
    print("ComposeForWrite( calls in PP:", len(re.findall(r"PushBackPilotMaterializer\s*\.\s*ComposeForWrite\s*\(", pp)))
    cmp_at = pp.find(".CompareWith(")
    comp_at = pp.find("PushBackPilotMaterializer.ComposeForWrite(")
    print("CompareWith before ComposeForWrite:", 0 <= cmp_at < comp_at)
    print("new PushBackPilotPrepareInput( in PP:", len(re.findall(r"\bnew\s+PushBackPilotPrepareInput\s*\(", pp)))
    prepare_at = pp.find("public static PushBackPilotPrepareResult Prepare(")
    full_at = pp.find("public static PushBackPilotPrepareResult PrepareFull(")
    print("Prepare of the pilot first in the file:", 0 <= prepare_at < full_at)
    print("RackEmbedComposer.Compose( calls in PP:", len(re.findall(r"\bRackEmbedComposer\s*\.\s*Compose\s*\(", pp)))
    for path in (PP, UR, ER):
        text = show(cur, path)
        print(path.split("/")[-1], "mentions Transaction:", len(re.findall("Transaction", text)))
    er = show(cur, ER)
    print("eraser names the five types of the model:", len(re.findall(r"\b(PushBackFullDraftState|PushBackFullDraftReader|PushBackFullComposition|PushBackConflictClassifier|PushBackViewDispositions)\b", er)))
    print("calls to the port outside UR in Pilot/: see the guard G-7 (run in the suite)")
    print("UP (IPushBackPilotUpdatePort.cs) changed against BaseSha:", "no" if not git("diff", "--name-only", base, cur if cur != "WORKTREE" else "HEAD", "--", "src/RackCad.Application/Systems/PushBack/Pilot/IPushBackPilotUpdatePort.cs").strip() else "YES")


if __name__ == "__main__":
    main()
