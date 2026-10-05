"""Method correction v3 -> v3.1 of the post-review auditor (declared after run R20261005T063359Z-86e3; the v3 result is preserved literally).

Defect of v3: PY_FORBIDDEN matched the bare WORD `requests` (and `glob`, `subprocess`, ...) anywhere in python code, including string literals and
dictionary keys. The harness model uses the key t["requests"], so a read-only heredoc that imports the closure harness by its literal path was flagged
as "python con un efecto no verificable (requests)" four times. v3.1 strips string literals before the check and flags a module only when it is USED
as a module (import / from ... import / attribute access); os.* functions, .rglob(, .iterdir(, __import__, exec( and eval( are unchanged.
Nothing else changes (paths, actions, fidelity, identity, result coherence).

Usage: python patch_v31.py <post-review.py v3> <post-review-v3.1.py>
"""
import sys

src, dst = sys.argv[1:3]
lines = open(src, encoding="utf-8").read().split("\n")
i = next(k for k, l in enumerate(lines) if l.startswith("PY_FORBIDDEN = re.compile("))
assert "\\brequests\\b" in lines[i + 1], "unexpected v3 layout"
lines[i:i + 2] = [
    'MODULES = r"(?:subprocess|urllib|requests|socket|http|shutil|ctypes|glob)"  # v3.1',
    r'PY_FORBIDDEN = re.compile(r"\bos\.(walk|listdir|scandir|system|popen|remove|unlink|rmdir|rename|replace|makedirs|mkdir|chdir)\b|\.rglob\(|\.iterdir\(|"',
    r'                          r"\b(?:import|from)\s+" + MODULES + r"\b|(?<![\w.])" + MODULES + r"\s*\.|\b__import__\b|\bexec\(|\beval\(")  # v3.1',
]
j = next(k for k, l in enumerate(lines) if l.strip() == "for m in PY_FORBIDDEN.finditer(code):")
lines[j] = lines[j].replace("PY_FORBIDDEN.finditer(code)", "PY_FORBIDDEN.finditer(PY_LITERAL.sub(\"''\", code))") + "  # v3.1: literals stripped first"
s = "\n".join(lines)
old = '"AuditorVersion": "v3 (kit v3; rules declared before the run)"'
assert s.count(old) == 1
s = s.replace(old, '"AuditorVersion": "v3.1 (method correction of v3 after R20261005T063359Z-86e3; see patch_v31.py)"')
open(dst, "w", encoding="utf-8", newline="\n").write(s)
print("ok")
