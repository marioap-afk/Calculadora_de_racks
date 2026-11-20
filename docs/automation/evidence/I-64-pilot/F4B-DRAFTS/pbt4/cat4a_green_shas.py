"""CA-T4a: the sha256 of each file of the GREEN of green_pbt4a.py (as UTF-8 with LF) and of the set, to compare with what
apply_green_pbt4a.py wrote in the session's copy (read-only; git show through green_pbt4a.py).

Usage: python -I cat4a_green_shas.py <repo> <rev>
"""
import hashlib
import importlib.util
import re
import sys

repo, rev = sys.argv[1:3]
here = __file__.rsplit("cat4a_green_shas.py", 1)[0]
spec = importlib.util.spec_from_file_location("green_pbt4a", here + "green_pbt4a.py")
gp = importlib.util.module_from_spec(spec)
spec.loader.exec_module(gp)
base, green = gp.green(repo, rev)
lines = []
for p in sorted(green):
    data = green[p].encode("utf-8")
    h = hashlib.sha256(data).hexdigest()
    lines.append("%s  %s" % (h, p))
    print("%-75s %7d %s" % (p, len(data), h))
print("set", hashlib.sha256(("\n".join(lines) + "\n").encode("utf-8")).hexdigest(), "(with final LF)")
print("set", hashlib.sha256("\n".join(lines).encode("utf-8")).hexdigest(), "(without final LF)")
draft = green[gp.DRAFT_P]
code = re.sub(r"//[^\n]*", "", draft)
print("reference ? in DRAFT code:", re.findall(r"\b(?:string|object|[A-Z]\w*)\?(?=[\s,)>])", code))
