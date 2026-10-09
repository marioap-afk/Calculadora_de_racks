"""Manifiesto del kit de la revisión formal de A-3 (kit-manifest.json): bytes y SHA-256 de cada archivo de kit/ (salvo el propio manifiesto) y
de los cuatro archivos del run. launch.py lo exige entero (preflight 1: ningún archivo distinto y ninguno sin listar).

Uso: python -B make_manifest.py          → regenera el manifiesto completo (quien prepara el kit)
     python -B make_manifest.py --gate   → la sesión principal, después de un `transport_gate.py set`: comprueba que todos los demás archivos del
                                            kit y del run siguen idénticos al manifiesto vigente y actualiza SOLO la entrada de transport-gate.json
                                            y el estado de la compuerta en la cabecera; si cualquier otro archivo cambió, se niega (código 2) sin
                                            escribir nada
"""
import hashlib
import json
import os
import sys

if hasattr(sys.stdout, "reconfigure"):
    sys.stdout.reconfigure(errors="backslashreplace")
KIT = os.path.dirname(os.path.abspath(__file__))
RUN = r"D:\r62-arch-a3-run"
RUN_FILES = ["order.txt", "prompt.md", "order-s56.txt", "order-s57.txt"]
MANIFEST = "kit-manifest.json"
GATE = "transport-gate.json"


def rec(path):
    b = open(path, "rb").read()
    return {"Bytes": len(b), "Sha256": hashlib.sha256(b).hexdigest()}


def kit_files():
    return sorted(n for n in os.listdir(KIT) if os.path.isfile(os.path.join(KIT, n)) and n != MANIFEST)


def gate_status():
    return json.load(open(os.path.join(KIT, GATE), encoding="utf-8")).get("Status")


def header():
    closure = json.load(open(os.path.join(KIT, "closure.json"), encoding="utf-8"))
    st = json.load(open(os.path.join(KIT, "selftest-result.json"), encoding="utf-8"))
    mp = os.path.join(KIT, "mutation-result.json")
    if os.path.isfile(mp):
        mu = json.load(open(mp, encoding="utf-8"))
        mutation = "mutation-result.json AllKilled %s, %d/%d" % (mu["AllKilled"], mu["Totals"]["Killed"], mu["Totals"]["Mutants"])
    else:
        mutation = "sin prueba de mutación completa en este kit (README, desviaciones)"
    t = closure["Transport"]
    return {
        "Kit": "I-62 A-3: kit de la UNA revisión formal independiente del Architect por claude-cli (disposición §58, punto 3; decisiones §57, punto 3; "
               "Owner CLAUDE-CLI-I62 = A)",
        "Ids": closure["Ids"], "PreviousAttempts": closure.get("PreviousAttempts", []), "Commit": closure["AuthorityRevision"], "ObjectIntroducedBy": closure["ObjectIntroducedBy"],
        "Object": "%s blob %s" % (closure["ObjectPath"], closure["ObjectBlob"]), "Annex": "%s blob %s" % (closure["AnnexPath"], closure["AnnexBlob"]),
        "SessionId": t["SessionId"],
        "Transport": "claude-cli %s (%s, SHA-256 %s)" % (t["Binary"]["Version"].split()[0], t["Binary"]["Path"], t["Binary"]["Sha256"]),
        "Auditor": "v5.1-a3 (declarado y autoprobado antes del lanzamiento: selftest-result.json AllPass %s, %d/%d; %s)"
                   % (st["AllPass"], st["Totals"]["Pass"], st["Totals"]["Cases"], mutation),
        "Clone": "D:/r62-arch-a3 (git clone --no-local -c core.autocrlf=false --single-branch; main = el commit; sin remoto; clone-verification.json AllChecks)",
        "Run": "D:/r62-arch-a3-run (order.txt, prompt.md, order-s56.txt y order-s57.txt, idénticos byte a byte a las copias del kit)",
        "TransportGate": "%s (D-01; launch.py solo lanza en MEASURED o ACCEPTED)" % gate_status(),
        "Launch": "kit/launch.py, por la sesión principal, una sola vez (NO lanzado)",
    }


def run_files():
    out = {}
    for n in RUN_FILES:
        r = rec(os.path.join(RUN, n))
        out[n] = dict({"Path": os.path.join(RUN, n)}, **r, IdenticalToKitCopy=open(os.path.join(RUN, n), "rb").read() == open(os.path.join(KIT, n), "rb").read())
    return out


def write(doc):
    with open(os.path.join(KIT, MANIFEST), "w", encoding="utf-8", newline="\n") as f:
        f.write(json.dumps(doc, ensure_ascii=False, indent=1) + "\n")
    print(json.dumps({"Written": MANIFEST, "Files": len(doc["Files"]), "Gate": gate_status(),
                      "Sha256": hashlib.sha256(open(os.path.join(KIT, MANIFEST), "rb").read()).hexdigest()}))


def main(argv):
    if "--gate" not in argv:
        doc = dict(header(), Files={n: rec(os.path.join(KIT, n)) for n in kit_files()}, RunFiles=run_files())
        write(doc)
        return 0
    cur = json.load(open(os.path.join(KIT, MANIFEST), encoding="utf-8"))
    changed = [n for n in kit_files() if n != GATE and (n not in cur["Files"] or rec(os.path.join(KIT, n)) != cur["Files"][n])]
    changed += [n for n in cur["Files"] if n not in kit_files()]
    changed += ["run/" + n for n, r in run_files().items() if {k: r[k] for k in ("Bytes", "Sha256")} != {k: cur["RunFiles"][n][k] for k in ("Bytes", "Sha256")}]
    if changed:
        print(json.dumps({"Refused": "además de transport-gate.json cambiaron otros archivos: no se escribe nada", "Changed": changed}, ensure_ascii=False))
        return 2
    cur["Files"][GATE] = rec(os.path.join(KIT, GATE))
    cur["TransportGate"] = header()["TransportGate"]
    write(cur)
    return 0


if __name__ == "__main__":
    sys.exit(main(sys.argv[1:]))
