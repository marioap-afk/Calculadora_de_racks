# Solo lectura: calcula, con el algoritmo de [PKC-C] (make_pbtc2_contract.py, cover()), el ForbiddenWriteScope de src/
# para el AllowedWriteScope propuesto de cada subtarea de PB-T1, sobre HEAD del worktree. No escribe nada.
# Uso: python -I forbidden_scope.py <raiz-del-worktree>
import subprocess
import sys

WT = sys.argv[1]


def git(*args):
    return subprocess.run(["git", "-C", WT] + list(args), capture_output=True, text=True).stdout


def tracked(prefix):
    return [p for p in git("ls-tree", "-r", "--name-only", "HEAD", "--", prefix).split("\n") if p and p.startswith(prefix)]


def in_head(path):
    return subprocess.run(["git", "-C", WT, "cat-file", "-e", "HEAD:" + path.rstrip("/")], capture_output=True).returncode == 0


def cover(prefix, allowed):
    inside = [a for a in allowed if a.startswith(prefix)]
    if not inside:
        return [prefix]
    out = []
    names = sorted({p[len(prefix):].split("/")[0] + ("/" if "/" in p[len(prefix):] else "") for p in tracked(prefix)})
    for n in names:
        if n.endswith("/"):
            out += cover(prefix + n, allowed)
        elif prefix + n not in allowed:
            out.append(prefix + n)
    return out


W = "src/RackCad.UI/Systems/PushBack/RackPushBackSystemWindow.xaml.cs"
WC = "src/RackCad.UI/Systems/PushBack/RackPushBackSystemWindow.Composite.cs"
APP = "src/RackCad.Application/Systems/PushBack/"
UT = "tests/RackCad.UI.Tests/"
TR = "tests/RackCad.Tests/"

TASKS = {
    "PB-T1a": {
        "product": [W, WC],
        "new": [UT + n for n in (
            "PushBackStateNetSupport.cs",
            "PushBackStateNetRackControlsTests.cs",
            "PushBackStateNetCellAndFrontTests.cs",
            "PushBackStateNetCompositeTests.cs",
            "PushBackStateNetModuleAndSafetyTests.cs",
            "PushBackStateNetActionsTests.cs",
            "PushBackStateNetFingerprintTests.cs")],
    },
    "PB-T1b": {
        "product": [APP + "PushBackCompositeEditorState.cs", APP + "PushBackEditorState.cs",
                    "src/RackCad.Application/Systems/Shared/RackModuleEditSession.cs"],
        "new": [APP + n for n in ("PushBackEditableState.cs", "PushBackEditableStateComparer.cs",
                                  "PushBackEditableStateOwnership.cs", "PushBackEditorOutputGate.cs")]
               + [TR + n for n in ("PushBackEditableStateLoadTests.cs", "PushBackEditableStateCopyTests.cs",
                                   "PushBackEditableStateOwnershipTests.cs", "PushBackEditorOutputGateTests.cs")]
               + [UT + "PushBackEditableStateParityTests.cs"],
    },
    "PB-T1c": {
        "product": [W, WC],
        "new": [UT + "PushBackWindowOnStateTests.cs", TR + "PushBackWindowOnStateSourceGuardTests.cs"],
    },
}

for task, spec in TASKS.items():
    allowed = spec["product"] + spec["new"]
    assert len(allowed) == len(set(allowed))
    missing = [p for p in spec["product"] if not in_head(p)]
    existing_new = [p for p in spec["new"] if in_head(p)]
    srcf = cover("src/", allowed)
    for a in allowed:
        assert not any(a == f or (f.endswith("/") and a.startswith(f)) for f in srcf), a
    uncovered = [p for p in tracked("src/") if p not in allowed and not any(p == f or (f.endswith("/") and p.startswith(f)) for f in srcf)]
    print("==", task, "allowed", len(allowed), "(producto", len(spec["product"]), "nuevas", len(spec["new"]), ")",
          "src-forbidden", len(srcf), "faltan-en-HEAD", missing, "nuevas-que-ya-existen", existing_new, "sin-cubrir", uncovered)
    for f in srcf:
        print("   ", f)
