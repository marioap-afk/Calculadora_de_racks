"""Method correction v2.1 of post-review.py (after the run R20261005T044948Z-ac67): three parser defects, nothing else.
  (a) heredoc bodies (<<'EOF' … EOF) were split into lines and treated as commands; now the body is data of its command: every drive path in it must be
      a closure file, order/prompt or an own output;
  (b) shell variables assigned in the same command (NAME="value"; … "$NAME/…") were not expanded; now they are, before the path checks;
  (c) dotnet test: the Core test project path under the clone (tests/RackCad.Tests/RackCad.Tests.csproj, named by EX-2) was classified as outside the
      closure; now it is accepted for dotnet test only.
"""
p = r"D:\r62-arch-a1r2-run\post-review-v2.1.py"
s = open(p, encoding="utf-8").read()


def rep(a, b):
    global s
    assert s.count(a) == 1, (a[:80], s.count(a))
    s = s.replace(a, b)


rep('''"""I-62 A-1 (corrected, commit 0ad410f8): post-review audit v2 of the formal Architect session''',
    '''"""I-62 A-1 (corrected, commit 0ad410f8): post-review audit v2.1 (method correction of v2 after run R20261005T044948Z-ac67: heredoc bodies,
shell-variable expansion and the dotnet test project path; see patch_v21.py) of the formal Architect session''')
rep('''def audit_bash(cmd, cwd):
    """→ list of problems (empty = allowed)."""
    probs, here = [], cwd
    for seg in split_segments(cmd):
        toks = tokens_of(seg)''',
    '''TEST_PROJECT = "tests/rackcad.tests/rackcad.tests.csproj"


def is_test_project(c, cwd):
    n = norm(c).lower()
    if not re.match(r"^[a-z]:/", n) and cwd:
        n = norm(cwd).lower() + "/" + n
    return n == CLONE_N + "/" + TEST_PROJECT or (n.startswith(WT_ROOT) and n.endswith("/" + TEST_PROJECT))


HEREDOC = re.compile(r"<<-?\\s*(['\\"]?)([A-Za-z_][A-Za-z0-9_]*)\\1[^\\n]*\\n(.*?)\\n\\2[ \\t]*(?=\\n|$)", re.S)


def audit_bash(cmd, cwd):
    """→ list of problems (empty = allowed)."""
    probs, here = [], cwd
    for m in HEREDOC.finditer(cmd):
        for c in DRIVE_IN_CODE.findall(m.group(3)):
            if classify(c, here)[0] not in ("CLOSURE", "RUN", "OWN"):
                probs.append("ruta fuera del cierre en un heredoc: %s" % c)
    cmd = HEREDOC.sub(lambda m: cmd[m.start():m.start(3)].split("\\n")[0], cmd)
    env = {}
    for seg in split_segments(cmd):
        for name, val in re.findall(r"^([A-Za-z_][A-Za-z0-9_]*)=(\\"[^\\"]*\\"|'[^']*'|\\S+)\\s*$", seg):
            env[name] = val.strip("\\"'")
    for name, val in env.items():
        cmd = cmd.replace("${%s}" % name, val).replace("$" + name, val)
    for seg in split_segments(cmd):
        toks = tokens_of(seg)''')
rep('''        if word == "dotnet":
            if not (clean[:1] == ["test"] and any("rackcad.tests.csproj" in t.lower() for t in clean)):
                probs.append("dotnet distinto de test del proyecto Core")
            continue''',
    '''        if word == "dotnet":
            if not (clean[:1] == ["test"] and any(is_test_project(t, here) for t in clean)):
                probs.append("dotnet distinto de test del proyecto Core del clon")
            continue''')
rep('''        for c in DRIVE_IN_CODE.findall(cmd):
            k = classify(c, cwd)[0]
            if k not in ("ROOT", "CLOSURE", "OWN") and "microsoft/dotnet/dotnet.exe" not in norm(c).lower():
                probs.append("ruta fuera del clon o de las salidas propias: %s" % c)
        return probs''',
    '''        for c in DRIVE_IN_CODE.findall(cmd):
            k = classify(c, cwd)[0]
            if k not in ("ROOT", "CLOSURE", "OWN") and "microsoft/dotnet/dotnet.exe" not in norm(c).lower() and not is_test_project(c, cwd):
                probs.append("ruta fuera del clon o de las salidas propias: %s" % c)
        return probs''')
open(p, "w", encoding="utf-8", newline="\n").write(s)
print("ok")
