# Redaccion para el repo publico: las rutas locales se sustituyen por marcadores (<scratchpad>, <worktree-de-I-64>, <checkout-principal>, <datos-locales-del-usuario>). El resto no cambia.
# Read-only census: which test files and test methods at a given commit read, as text, the source files
# that PB-T3 would change. It only runs `git ls-tree` and `git show` on the main checkout (object store).
import re
import subprocess
import sys

REPO = "<checkout-principal>"
REV = sys.argv[1] if len(sys.argv) > 1 else "2f5d3dec"

TARGETS = [
    "src/RackCad.Application/Systems/PushBack/Pilot/PushBackPilotUpdatePlan.cs",
    "src/RackCad.Application/Systems/PushBack/Pilot/PushBackPilotUpdateRun.cs",
    "src/RackCad.Application/Systems/PushBack/Pilot/IPushBackPilotUpdatePort.cs",
    "src/RackCad.Application/Systems/PushBack/Pilot/PushBackPilotIntentReader.cs",
    "src/RackCad.Application/Systems/PushBack/Pilot/PushBackPilotMaterializer.cs",
    "src/RackCad.Application/Systems/PushBack/Pilot/PushBackPilotSystemRebuild.cs",
    "src/RackCad.Application/Systems/PushBack/Pilot/PushBackPilotHostRun.cs",
    "src/RackCad.Application/Systems/PushBack/Pilot/PushBackPilotStateComparer.cs",
    "src/RackCad.Application/Systems/PushBack/Pilot/PushBackPilotDraftState.cs",
    "src/RackCad.Plugin/Views/RackPilotUpdateWriter.cs",
    "src/RackCad.Plugin/Views/RackPilotUpdateCommandContext.cs",
    "src/RackCad.Plugin/Views/RackPilotIntentAdapter.cs",
    "src/RackCad.Application/Workspace/WorkspaceHostCore.cs",
    "src/RackCad.Plugin/RackCommandSupport.cs",
    "src/RackCad.Plugin/RackPushBackCommands.cs",
    "src/RackCad.Plugin/Drawing/RackBlockRenamer.cs",
    "src/RackCad.Application/Systems/PushBack/Pilot",
    "src/RackCad.Application/Systems/PushBack",
]


def git(*args):
    return subprocess.run(["git", "-C", REPO] + list(args), capture_output=True, text=True, encoding="utf-8", errors="replace").stdout


def main():
    files = [f for f in git("ls-tree", "-r", "--name-only", REV, "--", "tests").splitlines() if f.endswith(".cs")]
    method_re = re.compile(r"\[(Fact|Theory)[^\]]*\]")
    name_re = re.compile(r"public\s+(?:async\s+)?(?:void|Task)\s+(\w+)\s*\(")
    for path in files:
        text = git("show", REV + ":" + path)
        # constants: const string X = "src/...";
        consts = {}
        for m in re.finditer(r'const\s+string\s+(\w+)\s*=\s*"([^"]+)"', text):
            consts[m.group(1)] = m.group(2)
        hits = []
        for t in TARGETS:
            literal = '"' + t + '"' in text or ('"' + t + '/') in text or ('"' + t + '"') in text
            names = [k for k, v in consts.items() if v == t or v == t + "/"]
            if literal or names:
                hits.append((t, names))
        if not hits:
            continue
        print("FILE " + path)
        for t, names in hits:
            print("  TARGET " + t + (" via " + ",".join(names) if names else " (literal)"))
        # per method: which target constants or literals appear in the body
        positions = [m.start() for m in method_re.finditer(text)] + [len(text)]
        for i in range(len(positions) - 1):
            chunk = text[positions[i]:positions[i + 1]]
            nm = name_re.search(chunk)
            if not nm:
                continue
            used = []
            for t, names in hits:
                if any(re.search(r"\b" + n + r"\b", chunk) for n in names) or ('"' + t) in chunk:
                    used.append(t.split("/")[-1])
            if used:
                print("    METHOD " + nm.group(1) + " -> " + ", ".join(sorted(set(used))))


if __name__ == "__main__":
    main()
