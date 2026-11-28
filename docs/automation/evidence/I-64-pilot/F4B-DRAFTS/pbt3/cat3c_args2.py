import re
import sys

# Uso: python -I cat3c_args2.py <carpeta pbt3>
# Depuracion de cat3c_args.py: generic_close de overload_sweep_t3c.py sobre cada '<' del primer caso, y un args_at con limite de pasos que
# imprime el indice cuando no avanza. No escribe nada.
sys.stdout.reconfigure(encoding="utf-8", errors="replace")
HERE = sys.argv[1]
src = open(HERE + "/overload_sweep_t3c.py", encoding="utf-8").read()
i = src.index("GENERIC_CHARS = ")
j = src.index("def args_at(")
ns = {"re": re}
exec(compile(src[i:j], "generic_close", "exec"), ns)
text = ("(Database database, Transaction transaction, IReadOnlyDictionary<string, (PreparedViewRedraw Prepared, DimensionViewKind Kind)> prepared, "
        "HashSet<ObjectId> staleDefinitions)")
for k, ch in enumerate(text):
    if ch == "<":
        print("generic_close en", k, "->", ns["generic_close"](text, k), flush=True)
body = src[src.index("def args_at("):src.index("    return None, i\n", src.index("def args_at(")) + len("    return None, i\n")]
print("args_at, lineas:", body.count("\n"), flush=True)
for n, line in enumerate(body.split("\n"), 1):
    print("%3d|%s" % (n, line), flush=True)
