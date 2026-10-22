"""Comprobación de privacidad (solo lectura) de las salidas del kit de la ronda 2: busca en cada archivo de los directorios dados el nombre de
usuario de Windows (largo y 8.3), la ruta del perfil, la identidad Git global del host (nombre y correo) y cualquier dirección de correo (salvo
dominios .invalid y noreply@anthropic.com). Los patrones se leen del entorno y de `git config --global` en tiempo de ejecución: este script no
los contiene y su salida no los reproduce (solo clases y recuentos por archivo).
Las copias byte a byte de archivos del repositorio (las copias fieles fiel__*.txt y a7-r1.md) se informan aparte como EXENTAS: conservan los
bytes del original.
Uso: python -I -B privacy_scan.py <salida.json> <dir> [<dir>...]
"""
import json
import os
import re
import subprocess
import sys

BS = chr(92)
home = os.path.expanduser("~")
user = os.environ.get("USERNAME", "")
tmp = os.environ.get("TEMP", "")
short = next((p for p in tmp.split(BS) if "~" in p), "")


def gitcfg(k):
    r = subprocess.run(["git", "config", "--global", k], capture_output=True, text=True)
    return r.stdout.strip()


gname, gmail = gitcfg("user.name"), gitcfg("user.email")
pats = {"WindowsUser": [user], "WindowsUser83": [short], "ProfilePath": [home, home.replace(BS, "/"), home.replace(BS, BS + BS)],
        "GitName": [gname], "GitEmail": [gmail]}
pats = {k: [re.compile(re.escape(x), re.I) for x in v if x] for k, v in pats.items()}
mail = re.compile(r"[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,}")
users_path = re.compile(r"(?i)[a-z]:(?:\\\\|\\|/)+users(?:\\\\|\\|/)+(?![%*<])[^\\/\"\s]+")
EXEMPT = lambda n: n.startswith("fiel__") or n == "a7-r1.md"
out = {"Patterns": {k: len(v) for k, v in pats.items()}, "Hits": [], "ExemptHits": [], "FilesScanned": 0}
for d in sys.argv[2:]:
    for root, dirs, files in os.walk(d):
        for n in files:
            p = os.path.join(root, n)
            try:
                t = open(p, "rb").read().decode("utf-8", "replace")
            except OSError:
                continue
            out["FilesScanned"] += 1
            cls = {k: sum(len(r.findall(t)) for r in v) for k, v in pats.items()}
            ms = [m for m in mail.findall(t) if not m.lower().endswith(".invalid") and m.lower() != "noreply@anthropic.com"]
            cls["Email"] = len(ms)
            cls["UsersPath"] = len(users_path.findall(t))
            cls = {k: v for k, v in cls.items() if v}
            if cls:
                (out["ExemptHits"] if EXEMPT(n) else out["Hits"]).append({"File": os.path.relpath(p, d).replace(BS, "/"), "Root": os.path.basename(d),
                                                                          "Classes": cls})
out["Clean"] = not out["Hits"]
with open(sys.argv[1], "w", encoding="utf-8", newline="\n") as f:
    f.write(json.dumps(out, ensure_ascii=False, indent=1) + "\n")
print(json.dumps({"FilesScanned": out["FilesScanned"], "Hits": len(out["Hits"]), "ExemptHits": len(out["ExemptHits"]), "Clean": out["Clean"]}))
for h in out["Hits"]:
    print("HIT", h["Root"], h["File"], h["Classes"])
for h in out["ExemptHits"]:
    print("EXEMPT", h["Root"], h["File"], h["Classes"])
