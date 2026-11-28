"""CA-T4a: the tests/ part of the ForbiddenWriteScope, PB-T4a against PB-T3c (read-only).

Usage: python -I cat4a_tests_forbidden.py <dry.json> <pbt3c.json>
"""
import json
import sys

dry = json.loads(open(sys.argv[1], "rb").read().decode("utf-8"))
t3c = json.loads(open(sys.argv[2], "rb").read().decode("utf-8"))
a = [x for x in dry["ForbiddenWriteScope"] if x.startswith("tests/")]
b = [x for x in t3c["ForbiddenWriteScope"] if x.startswith("tests/")]
print("dry", len(a), "t3c", len(b))
print("in dry not t3c:", [x for x in a if x not in b])
print("in t3c not dry:", [x for x in b if x not in a])
print("t3c allowed:", t3c["AllowedWriteScope"])
print("order of common kept:", [x for x in a if x in b] == [x for x in b if x in a])
