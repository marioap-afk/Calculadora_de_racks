"""Prueba en local las funciones sin efectos de emit_pbt5.py, sin tocar el worktree ni la red.

Uso: python -I test_emit_pure.py <checkout-principal> <carpeta de salida, fuera de la carpeta del paquete>

- yml_new sobre ebd304a3:docs/automation/state/I-64.yml (leido por objetos git desde el checkout principal);
- contract_text sobre el paquete v2.4: que empiece por su cabecera y lleve las 17 paradas enteras;
- check_bounded con lineas BOUNDED_RESULT sinteticas: una buena y varias que tienen que parar;
- history_text y texts con un listado sintetico de la carpeta del paquete.
Escribe en la carpeta de salida el I-64.yml nuevo, el contrato y los textos, para leerlos. Sale con 1 si algo falla.
"""
import importlib.util
import os
import re
import subprocess
import sys

sys.dont_write_bytecode = True
here = os.path.dirname(os.path.abspath(__file__))
src = os.path.dirname(here)
main_co, out = sys.argv[1], sys.argv[2]
if os.path.abspath(out).lower().startswith(src.lower()):
    sys.exit("la salida tiene que quedar fuera de la carpeta del paquete")
os.makedirs(out, exist_ok=True)
spec = importlib.util.spec_from_file_location("emit_pbt5", os.path.join(here, "emit_pbt5.py"))
E = importlib.util.module_from_spec(spec)
spec.loader.exec_module(E)
PKG, GUIDE = "I-64-f4b-pbt5-package-v2.4.md", "smoke-run-v2.md"
bad = 0


def check(cond, what):
    global bad
    print(("OK   " if cond else "FAIL ") + what)
    if not cond:
        bad += 1


def stops(fn, *a):
    try:
        fn(*a)
    except SystemExit as e:
        return str(e).startswith("STOP")
    return False


# 1. yml_new
old = subprocess.run(["git", "-C", main_co, "show", "ebd304a3:" + E.YML], capture_output=True, check=True).stdout.decode("utf-8")
new = E.yml_new(old, PKG, GUIDE)
ol, nl = old.split("\n"), new.split("\n")
check(len(ol) == len(nl), "yml_new conserva el numero de lineas")
changed = [i for i in range(len(ol)) if ol[i] != nl[i]]
check(len(changed) == 7, f"yml_new cambia 7 lineas (cambia {len(changed)})")
check(all(nl[i].count('"') == 2 and nl[i].endswith('"') for i in changed), "cada linea cambiada es una cadena entre comillas")
check("OWNER-01 de PB-T4e" in nl[[i for i in changed if nl[i].startswith("  next_action: ")][0]], "next_action conserva OWNER-01 (M-15)")
check("OWNER-01 de PB-T4e" in nl[[i for i in changed if nl[i].startswith("    NEXT_ACTION: ")][0]], "NEXT_ACTION conserva OWNER-01 (M-15)")
check(all("13:00" in nl[i] and "12:00" in nl[i] for i in changed if nl[i].startswith(("  next_action: ", "    NEXT_ACTION: "))), "la frontera estrecha en next_action y NEXT_ACTION")
check("smoke-run-v2.md" in new and "v2.3" not in "".join(nl[i] for i in changed), "las lineas nuevas nombran smoke-run-v2.md y no la v2.3")
check(stops(E.yml_new, new, PKG, GUIDE), "yml_new para si ya dice PB-T5 EMITIDA")
open(os.path.join(out, "I-64.yml.nuevo"), "w", encoding="utf-8", newline="\n").write(new)
open(os.path.join(out, "I-64.yml.diff.txt"), "w", encoding="utf-8", newline="\n").write("\n".join(f"- {ol[i]}\n+ {nl[i]}" for i in changed) + "\n")

# 2. contract_text
ct = E.contract_text(src, PKG, GUIDE)
check(ct.startswith("# Contrato de F4B-PBT5-SMOKE-PB-GATE-CLOSE (PB-T5)"), "el contrato empieza por su cabecera")
check("v2.4" in ct.split("\n")[2] and "smoke-run-v2.md" in ct.split("\n")[2], "la cabecera nombra la v2.4 y la guia v2")
ids = re.findall(r"^\| (PBT5-C\d\d) \|", ct, re.M)
check(ids == [f"PBT5-C{n:02d}" for n in range(1, 18)], f"las 17 paradas, enteras y en orden ({len(ids)})")
check("Son las de la v2.2" not in ct and "## 7. Riesgos" not in ct, "el contrato no remite a la v2.2 y termina antes del 7")
check(len(re.findall(r"^\| (INV-PBT5-\d\d) \|", ct, re.M)) == 17, "los 17 invariantes")
check("D:" + chr(92) + "spb1" not in ct and "<raiz-de-trabajo-con-/>" not in ct, "el contrato va redactado")
open(os.path.join(out, "contract.md"), "w", encoding="utf-8", newline="\n").write(ct)

# 3. check_bounded
want = {"paquete": "a" * 64, "guia": "b" * 64, "guion": "c" * 64}
good = "BOUNDED_RESULT: EMITIBLE; B 0 / I 0 / M 2; revisado paquete aaaaaaaa, guia bbbbbbbb, guion cccccccc; CI 45147867500 4/4"
cases = {
    "buena": (good, False),
    "tras correcciones": (good.replace("EMITIBLE;", "EMITIBLE TRAS CORRECCIONES;"), True),
    "no emitible": (good.replace("EMITIBLE;", "NO EMITIBLE;"), True),
    "un importante": (good.replace("I 0", "I 1"), True),
    "otro paquete": (good.replace("paquete aaaaaaaa", "paquete 02820249"), True),
    "sin guion": (good.replace(", guion cccccccc", ""), True),
    "guia con tilde": (good.replace("guia", "guía"), False),
}
for name, (line, should_stop) in cases.items():
    p = os.path.join(out, "bounded-" + re.sub(r"\W", "-", name) + ".md")
    open(p, "w", encoding="utf-8", newline="\n").write("# informe\n\n" + line + "\n")
    s = stops(E.check_bounded, p, want)
    check(s == should_stop, f"check_bounded, {name}: {'para' if s else 'pasa'}")
p2 = os.path.join(out, "bounded-dos-lineas.md")
open(p2, "w", encoding="utf-8", newline="\n").write(good + "\n" + good + "\n")
check(stops(E.check_bounded, p2, want), "check_bounded para con dos lineas")

# 4. history_text y texts, con un listado sintetico de la carpeta del paquete
items = []
for dp, dn, fn in os.walk(src):
    dn[:] = sorted(d for d in dn if d != "__pycache__")
    for f in sorted(fn):
        rel = os.path.relpath(os.path.join(dp, f), src).replace(os.sep, "/")
        if rel.startswith("_tools/") and rel.endswith(".local.txt"):
            continue
        tgt = {PKG: E.PKG_TARGET, E.OWNER: E.OWNER_TARGET}.get(rel, E.HIST + "/" + rel)
        items.append({"rel": rel, "target": tgt, "sha256": "0" * 64, "sha256_redactado": "1" * 64, "modo": "copia"})
        if rel == PKG:
            items.append({"rel": rel, "target": E.HIST + "/" + rel, "sha256": "0" * 64, "sha256_redactado": "1" * 64, "modo": "copia"})
ht = E.history_text(items, PKG, GUIDE)
check("v2.4" in ht and "v2.3" in ht and "`smoke-run-v2.md`" in ht and "review-v23/" in ht, "la historia nombra la v2.4, la v2.3, la guia v2 y review-v23")
check("preguntas al Owner v1, v2, v2.1, v2.2" in ht, "las preguntas, de la v1 a la v2.2 (M-14)")
e62, s292 = E.texts("ebd304a3773495fa9071de6611936c4c01a7b06c", {"run": "45147867500"}, good, items, ct, "2026-11-23", PKG, GUIDE)
check(e62.startswith("\n## F4B-E62. ") and s292.startswith("\n## 292. "), "las cabeceras de F4B-E62 y de la 292")
check("12:00 UTC" in e62 and "OWNER-01 de PB-T4e" in e62, "F4B-E62 lleva la frontera estrecha y OWNER-01")
check("D:" + chr(92) + "spb1" not in e62 + s292 and "<raiz-de-trabajo-con-/>" not in e62 + s292, "los textos no nombran la carpeta de trabajo")
open(os.path.join(out, "textos.md"), "w", encoding="utf-8", newline="\n").write(e62 + "\n----\n" + s292)

# 5. La redaccion y los patrones, como en collect(): que va solo por sha256 (sin imprimir lo que casa)
pats = E.load_patterns(src)
shaonly = []
for it in items:
    if it["target"] == E.HIST + "/" + PKG and it["rel"] == PKG and any(j is not it and j["rel"] == PKG for j in items[:items.index(it)]):
        continue
    data = open(os.path.join(src, it["rel"]), "rb").read()
    try:
        text = E.redact(data.decode("utf-8"))
        n = sum(1 for rx in pats for _ in rx.finditer(text))
    except UnicodeDecodeError:
        n = -1
    if n:
        shaonly.append(it["rel"])
print("solo por sha256 tras la redaccion:", len(shaonly))
for r in shaonly:
    print("   ", r)
for need in (PKG, GUIDE, E.OWNER):
    check(need not in shaonly, f"{need} va como copia (M-03)")
print("fallos:", bad)
sys.exit(1 if bad else 0)
