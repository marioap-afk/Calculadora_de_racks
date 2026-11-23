"""Revision independiente de PB-T5 v1: comprueba, por objetos git en 7da361eb, los literales
de la interfaz que cita el paquete, y cuenta los pasos y su columna «Observa».

Uso: python -I check_literals.py <repo> <paquete.md> <salida.txt>
Solo lee: git grep / git show sobre el commit; no toca el worktree ni el disco del checkout.
"""
import hashlib
import re
import subprocess
import sys

REV = "7da361eb"

LITERALS = [
    "Lado B declarado sin ningún frente activo: así no se guardará. Para conservarlo, elige «Lado B» y desmarca «En blanco» en al menos un frente.",
    "Editor cerrado sin cambios en el borrador.",
    "Actualizado: el cambio se aplicó al dibujo.",
    "No editable aquí: el origen ya no está en el dibujo",
    "No se recargó: el borrador se conserva tal como estaba.",
    "No se aplicó: el dibujo cambió fuera del panel",
    "RackCad: panel Workspace mostrado.",
    "Vistas: se borrará",
    "Cambiado fuera: ",
    "Cambiado por ti: ",
    "Cambiado en los dos: ",
    "Cambios estructurales: ",
    "No se abrió el editor: ",
    "Ningún sobre editable de la relectura coincide con el origen del borrador.",
    "(posiblemente obsoleto)",
    "Listo para editar",
    "Aplicar al borrador",
    "Conservar mi borrador",
    "Restaurar estándar",
    "Rack de dos sentidos (lado B)",
    "fuera del piloto (F5)",
    "Insertar varias",
    "Guardar en biblioteca",
    "Editar todo",
    "Comprobar ahora",
]


def git(repo, *args):
    return subprocess.run(["git", "-C", repo, *args], capture_output=True, text=True,
                          encoding="utf-8", errors="replace")


def main():
    repo, pkg, out = sys.argv[1], sys.argv[2], sys.argv[3]
    lines = []
    data = open(pkg, "rb").read()
    lines.append("paquete sha256 " + hashlib.sha256(data).hexdigest())
    text = data.decode("utf-8")
    lines.append("== Literales en src/ de " + REV + " (git grep -F)")
    for lit in LITERALS:
        r = git(repo, "grep", "-n", "-F", lit, REV, "--", "src")
        hits = [h for h in r.stdout.splitlines() if h.strip()]
        first = hits[0][:220] if hits else "-"
        lines.append("%3d  %s  | %s" % (len(hits), lit, first))
    lines.append("== Pasos SPB y columna Observa")
    count = {"S": 0, "S+O": 0}
    ids = []
    for ln_no, ln in enumerate(text.splitlines(), 1):
        m = re.match(r"^\| (SPB-\d\d) \|", ln)
        if not m:
            continue
        cells = [c.strip() for c in ln.strip().strip("|").split("|")]
        obs = cells[-1]
        ids.append(m.group(1))
        count[obs] = count.get(obs, 0) + 1
        if obs == "S+O":
            lines.append("S+O  %s  linea %d" % (m.group(1), ln_no))
    lines.append("pasos %d; unicos %d; S %d; S+O %d" % (len(ids), len(set(ids)), count.get("S", 0), count.get("S+O", 0)))
    expected = ["SPB-%02d" % i for i in range(1, 53)]
    lines.append("faltan " + ",".join(sorted(set(expected) - set(ids))) + " ; sobran " + ",".join(sorted(set(ids) - set(expected))))
    pre = re.findall(r"^\| (P-\d\d) \|", text, flags=re.M)
    lines.append("precondiciones " + ",".join(pre))
    stops = re.findall(r"^\| (PBT5-C\d\d) \|", text, flags=re.M)
    lines.append("paradas " + ",".join(stops))
    t4e = [i for i, ln in enumerate(text.splitlines(), 1) if "[T4e]" in ln]
    lines.append("lineas con [T4e]: " + ",".join(str(i) for i in t4e))
    res = "\n".join(lines) + "\n"
    open(out, "w", encoding="utf-8", newline="\n").write(res)
    sys.stdout.write(res)


if __name__ == "__main__":
    main()
