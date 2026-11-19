# Redaccion para el repo publico: las rutas locales se sustituyen por marcadores (<scratchpad>, <worktree-de-I-64>, <checkout-principal>, <datos-locales-del-usuario>). El resto no cambia.
# Read-only census, per test method, of the source files that a test reads as text, for a closed list of test files.
# Resolves constant concatenations and private helper methods/properties whose body names a path constant
# (for example: private static string Plan() => Src.Code(PlanPath);). It only runs `git show` on the main checkout.
import re
import subprocess
import sys

REPO = "<checkout-principal>"
REV = sys.argv[1]
FILES = sys.argv[2:]


def git(*args):
    return subprocess.run(["git", "-C", REPO] + list(args), capture_output=True, text=True, encoding="utf-8", errors="replace").stdout


def resolve_consts(text):
    raw = {}
    for m in re.finditer(r'const\s+string\s+(\w+)\s*=\s*([^;]+);', text):
        raw[m.group(1)] = m.group(2).strip()
    resolved = {}
    for _ in range(5):
        for name, expr in raw.items():
            parts = [p.strip() for p in expr.split("+")]
            out = []
            ok = True
            for p in parts:
                if p.startswith('"') and p.endswith('"'):
                    out.append(p[1:-1])
                elif p in resolved:
                    out.append(resolved[p])
                else:
                    ok = False
                    break
            if ok:
                resolved[name] = "".join(out)
    return {k: v for k, v in resolved.items() if v.startswith("src/")}


def helpers(text, consts):
    # one-line helpers: private static <T> Name(...) => ...PATH...;
    found = {}
    for m in re.finditer(r'(?:private|internal)\s+static\s+[\w<>\[\],\s\(\)]+?\s+(\w+)\s*(\([^)]*\))?\s*=>\s*([^;]+);', text):
        name, body = m.group(1), m.group(3)
        paths = sorted({k for k in consts if re.search(r"\b" + k + r"\b", body)} | set(re.findall(r'"(src/[^"]+)"', body)))
        if paths:
            found[name] = paths
    return found


def main():
    method_re = re.compile(r"\[(Fact|Theory)\b")
    name_re = re.compile(r"public\s+(?:async\s+)?(?:void|Task)\s+(\w+)\s*\(")
    for path in FILES:
        text = git("show", REV + ":" + path)
        consts = resolve_consts(text)
        helps = helpers(text, consts)
        print("FILE " + path)
        for k, v in sorted(helps.items()):
            print("  HELPER " + k + " -> " + ", ".join(v))
        positions = [m.start() for m in method_re.finditer(text)] + [len(text)]
        total = 0
        for i in range(len(positions) - 1):
            chunk = text[positions[i]:positions[i + 1]]
            nm = name_re.search(chunk)
            if not nm:
                continue
            total += 1
            used = {k for k in consts if re.search(r"\b" + k + r"\b", chunk)}
            for h, ps in helps.items():
                if re.search(r"\b" + h + r"\s*\(", chunk) or re.search(r"\b" + h + r"\b", chunk):
                    used |= set("H:" + h + "=" + p for p in ps)
            literals = set(re.findall(r'"(src/[^"]+)"', chunk))
            print("    METHOD " + nm.group(1) + " -> " + ", ".join(sorted(used | literals)))
        print("  METHODS " + str(total))


if __name__ == "__main__":
    main()
