import re
import sys

# Uso: python -I cat3c_args.py <carpeta pbt3>
# Prueba el corte de argumentos de overload_sweep_t3c.py (M-03 de CA-T3c) con casos propios, ejecutando solo sus dos funciones (generic_close y
# args_at, copiadas en memoria de su texto, desde GENERIC_CHARS hasta el final de args_at), sin correr el barrido. No escribe nada.
sys.stdout.reconfigure(encoding="utf-8", errors="replace")
HERE = sys.argv[1]
src = open(HERE + "/overload_sweep_t3c.py", encoding="utf-8").read()
i = src.index("GENERIC_CHARS = ")
# el final de args_at es su "return None, i" a 4 espacios, con la linea entera (el de 16 espacios de dentro del bucle no lo es)
j = src.index("\n    return None, i\n", src.index("def args_at(")) + len("\n    return None, i\n")
ns = {"re": re}
print("tramo copiado: %d caracteres, desde %d hasta %d" % (j - i, i, j), flush=True)
exec(compile(src[i:j], "args_at", "exec"), ns)
print("funciones:", [k for k in ns if not k.startswith("__")], flush=True)
CASES = [
    # (texto, cuantos argumentos de nivel superior tiene en C#)
    ("(Database database, Transaction transaction, IReadOnlyDictionary<string, (PreparedViewRedraw Prepared, DimensionViewKind Kind)> prepared, "
     "HashSet<ObjectId> staleDefinitions)", 4),
    ("(Dictionary<string, int[]> a, Func<int, string> f)", 2),
    ("(Action<string> a = null, int b = 0)", 2),
    ("(global::System.Collections.Generic.List<int> a, int b)", 2),
    ("(List<int?> a, (int x, int y)? b)", 2),
    ("(x < y ? a : b, c)", 2),
    ("(a<b, c>d)", 2),           # en C#, dos comparaciones: tras '>' viene un identificador, no ( ) , ; etc.
    ("(F<A, B>(x), y)", 2),       # una llamada generica y otro argumento
    ("(n<m, k)", 2),
    ("(new Dictionary<string, int>(), null!)", 2),
    ("(plan, null)", 2),
]
bad = 0
for text, want in CASES:
    print("   ...", text[:60], flush=True)
    got = ns["args_at"](text, 0)[0]
    n = None if got is None else len(got)
    ok = n == want
    bad += not ok
    print("%-4s %d/%s  %s" % ("ok" if ok else "DIF", want, n, text[:110]))
print("casos:", len(CASES), "| distintos:", bad)
