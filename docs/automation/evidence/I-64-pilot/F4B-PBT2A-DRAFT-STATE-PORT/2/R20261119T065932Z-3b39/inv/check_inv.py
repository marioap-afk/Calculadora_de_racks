#!/usr/bin/env python3
"""Read-only check of INV-PBT2A-01, 03, 04, 05, 06 and 14 over `git diff BASE..CUR` (reads from git; writes nothing).

Usage: python3 check_inv.py <BaseSha> <CurrentSha>   (run from the worktree root)
"""
import re
import subprocess
import sys

BASE, CUR = sys.argv[1], sys.argv[2]

ALLOWED_PATHS = {
    "src/RackCad.Application/Systems/PushBack/PushBackDraftStatePort.cs",
    "src/RackCad.Application/Systems/PushBack/PushBackDraftEditorLaunch.cs",
    "tests/RackCad.Tests/PushBackDraftStatePortTests.cs",
    "tests/RackCad.Tests/PushBackDraftPortSharedReferenceGuardTests.cs",
}
SRC_FILES = sorted(p for p in ALLOWED_PATHS if p.startswith("src/"))
CLOSED_NAMES = {
    "PushBackDraftApplyStatus", "NotEditable", "Applied", "PushBackDraftApplyResult", "Status", "IsDirty",
    "IPushBackDraftStatePort", "ApplyToDraft", "PushBackDraftSessionPort", "draft", "LaunchFor",
    "PushBackDraftEditorLaunch", "State", "RackId", "SourceProject", "Port",
}
FORBIDDEN = [
    "RackCad.UI", "System.Windows", "Autodesk", "Pilot", "RackSystemKind", ".RequestUpdate(", "CrossBoundary", "TryBeginApply",
    "CompleteApply", "DiscardAndReload", "KeepMineOnCurrentVersion", "EnterError", "MarkPossiblyStale", "SetPendingText",
]
LITERALS = {
    "src/RackCad.Application/Systems/PushBack/PushBackDraftStatePort.cs": [
        "throw new ArgumentNullException(nameof(draft));",
        "throw new ArgumentNullException(nameof(edited));",
        "private readonly DraftSession<PushBackEditableState> draft;",
        "return new PushBackDraftEditorLaunch(draft.CurrentDraftState, draft.RackId, sourceProject, new PushBackDraftSessionPort(draft));",
    ],
    "src/RackCad.Application/Systems/PushBack/PushBackDraftEditorLaunch.cs": [
        "if (state == null) throw new ArgumentNullException(nameof(state));",
        'if (string.IsNullOrWhiteSpace(rackId)) throw new ArgumentException("A rack id is required.", nameof(rackId));',
        "if (port == null) throw new ArgumentNullException(nameof(port));",
    ],
}


def git(*args):
    return subprocess.run(["git", *args], capture_output=True, text=True, check=True).stdout


def show(path):
    return git("show", CUR + ":" + path)


def clean(text):
    text = re.sub(r"/\*.*?\*/", " ", text, flags=re.S)
    text = re.sub(r"//[^\n]*", "", text)
    return re.sub(r'"(?:\\.|[^"\\\n])*"', '""', text)


def declared_names(text):
    """Names declared at type level and member level (depth 1 = types in the namespace block, depth 2 = members)."""
    text = clean(text)
    names = []
    depth = 0
    buf = ""
    stack = []  # kind of each open type body
    last_head = ""
    for ch in text:
        if ch == "{":
            head = buf.strip()
            buf = ""
            if depth == 1:
                m = re.search(r"\b(enum|struct|interface|class)\s+(\w+)", head)
                if m:
                    names.append(("type", m.group(2)))
                    stack.append(m.group(1))
                else:
                    stack.append("?")
            elif depth == 2:
                part = re.split(r"[(=]", head)[0].strip()
                ident = re.findall(r"\w+", part)
                if ident:
                    names.append(("member", ident[-1]))
                stack.append("body")
            else:
                stack.append("inner")
            depth += 1
            last_head = head
        elif ch == "}":
            depth -= 1
            stack.pop()
            buf = ""
        elif ch == ";" and depth == 2:
            part = re.split(r"[(=]", buf)[0].strip()
            ident = re.findall(r"\w+", part)
            if ident:
                names.append(("member", ident[-1]))
            buf = ""
        elif ch == "," and depth == 2 and stack and stack[-1] == "enum":
            part = re.split(r"=", buf)[0].strip()
            ident = re.findall(r"\w+", part)
            if ident:
                names.append(("enum-member", ident[-1]))
            buf = ""
        else:
            buf += ch
        if depth == 2 and stack and stack[-1] == "enum" and ch == "\n":
            pass
    return names, text


def enum_members(text):
    names = []
    for m in re.finditer(r"\benum\s+\w+\s*\{(.*?)\}", clean(text), flags=re.S):
        for item in m.group(1).split(","):
            ident = re.findall(r"\w+", item.split("=")[0])
            if ident:
                names.append(ident[-1])
    return names


def main():
    print("BaseSha=" + BASE)
    print("CurrentSha=" + CUR)
    changed = [p for p in git("diff", "--name-only", BASE, CUR).splitlines() if p]
    print("INV-PBT2A-01 rutas-cambiadas=" + str(len(changed)))
    for p in sorted(changed):
        print("  " + p)
    print("INV-PBT2A-01 dentro-del-alcance=" + str(set(changed) <= ALLOWED_PATHS))
    forbidden_dirs = ["src/RackCad.UI", "src/RackCad.Plugin", "src/RackCad.Domain", "src/RackCad.Application/Workspace",
                      "src/RackCad.Application/Systems/PushBack/Pilot"]
    print("INV-PBT2A-01 diff-zonas-vedadas-vacio=" + str(git("diff", BASE, CUR, "--", *forbidden_dirs).strip() == ""))
    print("INV-PBT2A-01 src-existentes-modificados-vacio=" + str(git("diff", "--name-only", "--diff-filter=MD", BASE, CUR, "--", "src").strip() == ""))
    print("INV-PBT2A-01 src-cambiados=" + str(sorted(p for p in changed if p.startswith("src/")) == SRC_FILES))

    for path in SRC_FILES:
        text = show(path)
        names, cleaned = declared_names(text)
        members = enum_members(text)
        all_names = [n for _, n in names] + members
        type_names = {n for k, n in names if k == "type"}
        extras = sorted({n for n in all_names if n not in CLOSED_NAMES and n not in type_names})
        ctor_names = sorted({n for k, n in names if k == "member" and n in type_names})
        print("INV-PBT2A-03/C13 " + path.split("/")[-1] + " declaraciones=" + ",".join(sorted(set(all_names))))
        print("INV-PBT2A-03/C13 " + path.split("/")[-1] + " fuera-de-la-lista-cerrada=" + (",".join(extras) if extras else "ninguna"))
        # A constructor shows up as a member named like its type: it is admitted ("besides the constructors of those types").
        print("INV-PBT2A-03/C13 " + path.split("/")[-1] + " constructores=" + (",".join(ctor_names) if ctor_names else "ninguno"))
        for literal in LITERALS.get(path, []):
            print("INV-PBT2A-03 literal-presente=" + str(literal in re.sub(r"\s+", " ", text)) + " :: " + literal)
        # INV-PBT2A-04
        statics = re.findall(r"\bstatic\b[^;{(]*[;=]", cleaned)
        print("INV-PBT2A-04 " + path.split("/")[-1] + " campos-estaticos=" + str(len(statics)))
        ns = re.findall(r"^\s*namespace\s+([\w.]+)\s*(\{?)\s*$", text, flags=re.M)
        print("INV-PBT2A-04 " + path.split("/")[-1] + " namespaces=" + str(ns))
        # INV-PBT2A-05
        hits = [t for t in FORBIDDEN if t in text]
        print("INV-PBT2A-05 " + path.split("/")[-1] + " tokens-prohibidos=" + (",".join(hits) if hits else "ninguno"))

    port = show("src/RackCad.Application/Systems/PushBack/PushBackDraftStatePort.cs")
    m = re.search(r"public PushBackDraftApplyResult ApplyToDraft\(PushBackEditableState edited\)\s*\{(.*?)\n        \}", port, flags=re.S)
    body = clean(m.group(1)) if m else ""
    calls = re.findall(r"\bdraft\s*\.\s*(\w+)\s*\(([^)]*)\)", body)
    print("INV-PBT2A-06 llamadas-al-borrador=" + str(calls))
    print("INV-PBT2A-06 una-sola-SetCurrentDraftState(edited)=" + str(calls == [("SetCurrentDraftState", "edited")]))
    print("INV-PBT2A-14 sin-copia-en-el-adaptador=" + str(not re.search(r"\bCopy\s*\(", clean(port))))


main()
