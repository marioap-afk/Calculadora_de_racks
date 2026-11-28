import sys

path = sys.argv[1]
with open(path, encoding="utf-8") as f:
    lines = f.read().split("\n")

for line in lines:
    if "**question** (" in line and "caracteres): «" in line:
        start = line.index("caracteres): «") + len("caracteres): «")
        end = line.rindex("»")
        q = line[start:end]
        print("question", len(q))
    if "**header** (" in line:
        h = line[line.index("`") + 1:line.rindex("`")]
        print("header", len(h), h)
    if line.startswith("  | `") and "` | " in line:
        label = line[len("  | `"):line.index("` | ")]
        desc = line[line.index("` | ") + 4:line.rstrip().rindex(" |")]
        print("label", len(label), label, "| desc", len(desc))
