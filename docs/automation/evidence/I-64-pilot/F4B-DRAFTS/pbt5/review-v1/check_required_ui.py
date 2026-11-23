"""Recomprobacion 1: lista los RequiredTests de UI del contrato de PB-T4e (7da361eb) y marca los
filtros que tocan clases que, segun F4B-E30, crean un RackFrameConfiguratorWindow real.

Uso: python -I check_required_ui.py <repo> <salida.txt>
"""
import json
import subprocess
import sys

REV = "7da361eb"
PATH = "docs/automation/evidence/I-64-pilot/F4B-PBT4-EDIT-ALL-RELOAD/coordinator/gate-contract.json"
RISKY = ["RichEditor", "EditorShellAdoption", "FirstView", "PushBackHeaderOwnerRoundTwo", "RackFrameConfigurator"]


def main():
    repo, out = sys.argv[1], sys.argv[2]
    raw = subprocess.run(["git", "-C", repo, "show", REV + ":" + PATH], capture_output=True).stdout
    data = json.loads(raw.decode("utf-8"))
    lines = ["claves: " + ", ".join(sorted(data.keys()))]
    req = data.get("RequiredTests") or []
    lines.append("RequiredTests: %d (tipo %s)" % (len(req), type(req).__name__))
    ui = 0
    for i, t in enumerate(req):
        s = json.dumps(t, ensure_ascii=False) if not isinstance(t, str) else t
        is_ui = "UI.Tests" in s or "RackCad.UI.Tests" in s
        if is_ui:
            ui += 1
            risky = [r for r in RISKY if r in s]
            lines.append("UI %3d %s%s" % (i, s[:300], ("  <-- CONTENCION: " + ",".join(risky)) if risky else ""))
    lines.append("filtros de UI: %d" % ui)
    res = "\n".join(lines) + "\n"
    open(out, "w", encoding="utf-8", newline="\n").write(res)
    sys.stdout.write(res)


if __name__ == "__main__":
    main()
