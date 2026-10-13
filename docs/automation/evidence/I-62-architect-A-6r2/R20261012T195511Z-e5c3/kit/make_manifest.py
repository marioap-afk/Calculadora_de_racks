"""Manifiesto del kit de la ÚNICA revisión formal de la ronda 2 de A-6 (kit-manifest.json): bytes y SHA-256 de cada archivo de kit/ (salvo el propio
manifiesto) y de los tres archivos del run. launch.py lo exige entero (preflight 1: ningún archivo distinto y ninguno sin listar). Adaptado del
make_manifest.py del kit de la ronda 1.

Uso: python -B make_manifest.py          → regenera el manifiesto completo (quien termina el kit, después del selftest y de la mutación)
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
RUN = r"D:\r62-arch-a6r2-run"
RUN_FILES = ["order.txt", "prompt.md", "a6-r1.md"]
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
    params = json.load(open(os.path.join(KIT, "kit-params.json"), encoding="utf-8"))
    assert params.get("Provisional") is False and closure.get("Provisional") is False and not closure.get("Synthetic"), \
        "kit provisional o sintético: no se escribe el manifiesto del kit real"
    st = json.load(open(os.path.join(KIT, "selftest-result.json"), encoding="utf-8"))
    mp = os.path.join(KIT, "mutation-result.json")
    if os.path.isfile(mp):
        mu = json.load(open(mp, encoding="utf-8"))
        mutation = "mutation-result.json AllKilled %s, %d/%d" % (mu["AllKilled"], mu["Totals"]["Killed"], mu["Totals"]["Mutants"])
    else:
        mutation = "sin prueba de mutación completa en este kit (README, desviaciones)"
    t = closure["Transport"]
    return {
        "Kit": "I-62: kit de la ÚNICA revisión formal independiente del Architect de la ronda 2 de la enmienda A-6 por claude-cli (decisiones §80, "
               "punto 6; Owner CLAUDE-CLI-I62 = A; lanzamiento condicionado a la compuerta 11: línea del Owner de §78, blob de la ronda 2 en la HEAD "
               "y una disposición del Coordinator posterior a §80 con el blob y el RunId)",
        "Ids": closure["Ids"], "PreviousAttempts": [], "PreviousRound": "R20261011T215735Z-c434 (ronda 1: CHANGES REQUIRED; auditor NOT_ACCREDITED)",
        "Commit": closure["AuthorityRevision"],
        "Object": "%s (blob %s; publicación %s)" % (closure["ObjectPath"], closure["ObjectBlob"], closure["Publication"]["Commit"]),
        "Package": "ninguno (%s)" % closure["PackageNote"],
        "SessionId": t["SessionId"],
        "Transport": "claude-cli %s (%s, SHA-256 %s)" % (t["Binary"]["Version"].split()[0], t["Binary"]["Path"], t["Binary"]["Sha256"]),
        "Auditor": "v5.2-a6r2 (declarado y autoprobado antes del lanzamiento: selftest-result.json AllPass %s, %d/%d; %s)"
                   % (st["AllPass"], st["Totals"]["Pass"], st["Totals"]["Cases"], mutation),
        "Clone": "D:/r62-arch-a6r2 (git clone --no-local -c core.autocrlf=false --single-branch; HEAD separado en el commit, sin ramas locales; sin "
                 "remoto; clone-verification.json AllChecks, con la reproducción de las guardas de la ronda 2)",
        "Run": "D:/r62-arch-a6r2-run (order.txt, prompt.md y a6-r1.md, idénticos byte a byte a las copias del kit)",
        "TransportGate": "%s (D-01; launch.py solo lanza en MEASURED o ACCEPTED)" % gate_status(),
        "OwnerAndAuthorizationGate": "%s (owner_gate.py): launch.py solo lanza con la línea del Owner A6-ADR0046-9-FX02 (SHA-256 %s…) después de §78, "
                                     "CLAUDE-CLI-I62 = A vigente, A-6 en la HEAD con el blob %s y una disposición del Coordinator numerada después de "
                                     "§80 que contenga ese blob y el RunId %s; se evalúa en el momento del lanzamiento"
                                     % (closure["OwnerGate"]["Check"], closure["OwnerGate"]["A6LineSha256"][:8], closure["ObjectBlob"][:8],
                                        closure["Ids"]["RunId"]),
        "Launch": "kit/launch.py, por la sesión principal, una sola vez y solo tras la disposición posterior del Coordinator (NO lanzado)",
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
