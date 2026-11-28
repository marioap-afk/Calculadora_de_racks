import subprocess
import sys

# Uso: python -I cat3a_ctx.py <checkout> <rev> <ruta:linea[:antes:despues]> ...
# Imprime el contexto de una linea de un archivo de la revision (solo lectura, por objetos git).
sys.stdout.reconfigure(encoding="utf-8", errors="replace")
REPO, REV = sys.argv[1], sys.argv[2]
for spec in sys.argv[3:]:
    parts = spec.split(":")
    path, line = parts[0], int(parts[1])
    before = int(parts[2]) if len(parts) > 2 else 8
    after = int(parts[3]) if len(parts) > 3 else 6
    text = subprocess.run(["git", "-C", REPO, "show", REV + ":" + path], capture_output=True, check=True).stdout.decode("utf-8", "replace").split("\n")
    print("=== %s:%d" % (path, line))
    for k in range(max(0, line - 1 - before), min(len(text), line + after)):
        print("%5d%s %s" % (k + 1, ">" if k + 1 == line else " ", text[k][:200]))
