# -*- coding: utf-8 -*-
# Solo lectura: calcula, con el algoritmo cover() de [PKC-C] (el mismo de pbt1/forbidden_scope.py), la parte de src/ del
# ForbiddenWriteScope para el AllowedWriteScope propuesto de PB-T2a y PB-T2b, sobre un SHA dado (objetos git).
# Uso: python -I forbidden_pbt2.py <ruta-del-checkout-principal> <sha>. No escribe nada.
import subprocess
import sys

REPO = sys.argv[1]
SHA = sys.argv[2]


def git(*args):
    return subprocess.run(["git", "-C", REPO] + list(args), capture_output=True, text=True).stdout


def tracked(prefix):
    return [p for p in git("ls-tree", "-r", "--name-only", SHA, "--", prefix).split("\n") if p and p.startswith(prefix)]


def in_sha(path):
    return subprocess.run(["git", "-C", REPO, "cat-file", "-e", SHA + ":" + path.rstrip("/")], capture_output=True).returncode == 0


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
UIPB = "src/RackCad.UI/Systems/PushBack/"
UT = "tests/RackCad.UI.Tests/"
TR = "tests/RackCad.Tests/"

TASKS = {
    "PB-T2a": {
        "product": [],
        "new": [APP + "PushBackDraftStatePort.cs", APP + "PushBackDraftEditorLaunch.cs",
                TR + "PushBackDraftStatePortTests.cs", TR + "PushBackDraftPortSharedReferenceGuardTests.cs"],
    },
    "PB-T2b": {
        "product": [W],
        "new": [UIPB + "PushBackEditorMode.cs",
                UT + "PushBackWorkspaceSupport.cs", UT + "PushBackWorkspaceModeTests.cs", UT + "PushBackWorkspaceApplyCloseTests.cs",
                UT + "PushBackFieldValidationConcordanceTests.cs",
                TR + "PushBackWorkspaceModeSourceGuardTests.cs", TR + "PushBackWorkspaceSharedReferenceGuardTests.cs"],
    },
    # v2: PB-T2c, solo pruebas y los mutantes del RED en W y WC (revertidos por el GREEN). Medida antes de PB-T2b: el
    # archivo nuevo del modo de PB-T2b no existe aun en el SHA medido, y tras PB-T2b sale como una entrada prohibida mas.
    "PB-T2c": {
        "product": [W, WC],
        "new": [UT + "PushBackWorkspaceCarryoverTests.cs"],
    },
}

for task, spec in TASKS.items():
    allowed = spec["product"] + spec["new"]
    assert len(allowed) == len(set(allowed))
    missing = [p for p in spec["product"] if not in_sha(p)]
    existing_new = [p for p in spec["new"] if in_sha(p)]
    srcf = cover("src/", allowed)
    for a in allowed:
        assert not any(a == f or (f.endswith("/") and a.startswith(f)) for f in srcf), a
    uncovered = [p for p in tracked("src/") if p not in allowed and not any(p == f or (f.endswith("/") and p.startswith(f)) for f in srcf)]
    print("==", task, "allowed", len(allowed), "(producto", len(spec["product"]), "nuevas", len(spec["new"]), ")",
          "src-forbidden", len(srcf), "faltan-en-SHA", missing, "nuevas-que-ya-existen", existing_new, "sin-cubrir", uncovered)
    for f in srcf:
        print("   ", f)
