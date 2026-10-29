"""I-66 R1a: shared helpers of the shadow state/v2 producers (QU, QH): load the accredited I-62 controls (f2-mc.py, f3-mc.py) without copying
them, build StateRefs against the I-66 worktree, and write a state dict in the canonical form of YamlSubset.Write (RackCad.Tests.dll built at the
shadow authority revision). Same emitter as bootstrap/r1a_at2.py."""
import importlib.util, os, subprocess, sys

NL = chr(10)
UNIT, BRANCH = "I-66", "fix/placedclone-copia-anotaciones"
CLAIM_ID, CLAIM_COMMIT, BASE = "e289db83-b7f7-4c6e-91c6-d2551e393055", "e1cd88bfa515258e99b34ccad6bbaec837beff13", "bb0d5522e8411f66a51fdfb3f1f0d0514b737453"
AUTH_REV = "41ccfdcf692fb319bb7e8ff4de7edeaa4f1c5134"  # SH-1: protocol.effective_sha of the BOOTSTRAP (immutable, I-P06)
REL = "docs/automation/evidence/I-66-R1"
STATE_PATH = REL + "/state.v2.yml"
DEC = "docs/automation/decisions/%s.md" % UNIT
CAPS = ["PRINCIPAL_COORDINATION.level", "PRINCIPAL_COORDINATION.effort", "PRINCIPAL_COORDINATION.remote-facts", "PRINCIPAL_COORDINATION.introspection",
        "PRINCIPAL_COORDINATION.repo-write"]


def git(repo, *a):
    return subprocess.check_output(["git", "-C", repo] + list(a)).decode("utf-8").strip()


def load_controls(i62):
    """f2-mc.py and f3-mc.py of I-62, which must be byte-identical to their AUTH_REV versions."""
    mods = []
    for name, rel in (("f2mc", "docs/automation/evidence/I-62-F2/f2-mc.py"), ("f3mc", "docs/automation/evidence/I-62-F3/f3-mc.py")):
        assert git(i62, "rev-parse", "HEAD:" + rel) == git(i62, "rev-parse", AUTH_REV + ":" + rel), rel
        sys.argv = [os.path.basename(rel), i62, os.path.dirname(os.path.join(i62, rel))]
        spec = importlib.util.spec_from_file_location(name, os.path.join(i62, rel))
        m = importlib.util.module_from_spec(spec)
        spec.loader.exec_module(m)
        mods.append(m)
    return mods


def ref(w, path, commit="HEAD"):
    """A StateRef to the blob of a path as it will be in the tree of the commit holding the point (worktree file if not committed yet)."""
    full = os.path.join(w, *path.split("/"))
    return {"path": path, "blob": git(w, "hash-object", full)}


def scalar(v):
    if v is None:
        return "null"
    if isinstance(v, bool):
        return "true" if v else "false"
    if isinstance(v, int):
        return str(v)
    assert '"' not in v and chr(92) not in v and all(ord(c) >= 32 for c in v), v
    return '"' + v + '"'


def emit(m, ind, out):
    sp = " " * ind
    for k, v in m.items():
        if isinstance(v, dict) and v:
            out.append(sp + k + ":")
            emit(v, ind + 2, out)
        elif isinstance(v, list) and v:
            out.append(sp + k + ":")
            for item in v:
                if isinstance(item, dict):
                    sub = []
                    emit(item, ind + 4, sub)
                    out.append(sp + "  - " + sub[0].lstrip())
                    out.extend(sub[1:])
                else:
                    out.append(sp + "  - " + scalar(item))
        else:
            out.append(sp + k + ": " + ("{}" if v == {} else "[]" if v == [] else scalar(v)))


def write_canonical(state, w, dll, obs_dir, draft_name):
    lines = []
    emit(state, 0, lines)
    draft_path = os.path.join(obs_dir, draft_name)
    open(draft_path, "w", encoding="utf-8", newline=NL).write(NL.join(lines) + NL)
    ps = ("Add-Type -Path '%s'; $t = [IO.File]::ReadAllText('%s', [Text.UTF8Encoding]::new($false)); "
          "$m = [RackCad.Tests.YamlSubset]::Read($t, [RackCad.Tests.YamlReadMode]::Strict); "
          "[IO.File]::WriteAllText('%s', [RackCad.Tests.YamlSubset]::Write($m), [Text.UTF8Encoding]::new($false))"
          % (dll, draft_path, os.path.join(w, *STATE_PATH.split("/"))))
    r = subprocess.run(["pwsh", "-NoProfile", "-Command", ps], capture_output=True, text=True, encoding="utf-8", errors="replace")
    assert r.returncode == 0 and not r.stderr.strip(), r.stderr
