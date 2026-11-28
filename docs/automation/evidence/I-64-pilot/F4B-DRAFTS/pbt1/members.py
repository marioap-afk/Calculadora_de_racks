# Solo lectura: lista los miembros (metodos, propiedades, campos) de un .cs con su rango de lineas.
# Uso: python -I members.py <archivo.cs>
import re
import sys

path = sys.argv[1]
with open(path, encoding="utf-8-sig") as f:
    lines = f.read().split("\n")

decl = re.compile(
    r"^\s{4,8}(?:\[[^\]]*\]\s*)*"
    r"((?:public|private|internal|protected|static|readonly|override|virtual|sealed|async|partial|new|const|event)\s+)+"
    r"[\w<>\[\]\?,\.\s]+?\b(\w+)\s*(\(|\{|=|;|=>)"
)

depth = 0
results = []
current = None
for i, raw in enumerate(lines, start=1):
    line = raw
    stripped = line.strip()
    if depth == 2 and current is None:
        m = decl.match(line)
        if m and not stripped.startswith("//"):
            name = m.group(2)
            current = [name, i, i]
            if stripped.endswith(";") and "{" not in stripped:
                results.append((name, i, i))
                current = None
    # count braces ignoring strings crudely
    s = re.sub(r'"(?:\\.|[^"\\])*"', '""', line)
    s = re.sub(r"'(?:\\.|[^'\\])'", "''", s)
    s = s.split("//")[0]
    depth += s.count("{") - s.count("}")
    if current is not None:
        current[2] = i
        if depth == 2 and (stripped.endswith("}") or stripped.endswith(";")):
            results.append(tuple(current))
            current = None

for name, a, b in results:
    print(f"{a}-{b}\t{name}")
