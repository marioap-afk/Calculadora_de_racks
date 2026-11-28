# Redaccion para el repo publico: las rutas locales se sustituyen por marcadores (<scratchpad>, <worktree-de-I-64>, <checkout-principal>, <datos-locales-del-usuario>). El resto no cambia.
import json
import subprocess
import sys

sys.stdout.reconfigure(encoding="utf-8", errors="replace")
REPO = "<checkout-principal>"
REV = sys.argv[1] if len(sys.argv) > 1 else "05beabfd"


def git(*args):
    return subprocess.run(["git", "-C", REPO, *args], capture_output=True, check=True).stdout


names = git("ls-tree", "--name-only", REV, "tests/RackCad.Tests/Fixtures/I64/").decode().split()
for path in names:
    env = json.loads(git("show", f"{REV}:{path}").decode("utf-8-sig"))
    proj = json.loads(env["Design"])
    pb = proj.get("PushBack") or {}
    st = pb.get("Structure") or {}
    fronts = st.get("Fronts") or []
    comp = pb.get("Composite")
    sideb = pb.get("SideB")
    mods = st.get("Modules")
    gaps = []
    for f in fronts:
        if isinstance(f, dict):
            for k in f:
                if "gap" in k.lower():
                    gaps.append((k, f[k]))
    keys_with_gap = [k for k in st if "gap" in k.lower()]
    comp_keys = sorted(comp.keys()) if isinstance(comp, dict) else comp
    print(
        path.rsplit("/", 1)[-1],
        "Name=", env.get("Name"),
        "View=", env.get("View"),
        "PT=", st.get("PalletTolerance"),
        "BD=", st.get("BeamDepth"),
        "PP=", st.get("PostPeralte"),
        "AS=", st.get("AnnotationScale"),
        "DS=", st.get("DimensionStyle"),
        "nFronts=", len(fronts),
        "SideB=", None if sideb is None else ("present" if isinstance(sideb, dict) else sideb),
        "stGapKeys=", keys_with_gap,
        "frontGaps=", gaps[:3],
        "CustomProps=", env.get("CustomProperties"),
        "PD/PF/PH/PW=", st.get("PalletDepth"), st.get("PalletFront"), st.get("PalletHeight"), st.get("PalletWeight"),
    )
    if isinstance(comp, dict):
        print("   Composite keys:", comp_keys)
        for k in comp_keys:
            if "gap" in k.lower() or "Override" in k:
                print("   ", k, "=", comp[k])
    if isinstance(sideb, dict):
        print("   SideB keys:", sorted(sideb.keys()))
        for k in sorted(sideb.keys()):
            if "gap" in k.lower():
                print("   SideB", k, "=", sideb[k])
