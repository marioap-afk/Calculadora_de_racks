"""Manifiesto del kit de la ÚNICA revisión formal de la RONDA 4 de la enmienda A-7 (kit-manifest.json): bytes y SHA-256 de cada archivo de kit/
(salvo el propio manifiesto) y de los archivos del run (order.txt, prompt.md, a7-r3.md y las copias fieles de FaithfulCopies, decisiones §94,
punto 5). launch.py lo exige entero (preflight 1: ningún archivo distinto y ninguno sin listar). Derivado del make_manifest.py del kit de A-7
ronda 3 (R20261023T180846Z-62bd, a su vez del de la ronda 1). Es la congelación del kit (decisiones §98, punto 7). La cabecera registra la
autorización fijada o pendiente
(pendiente en el kit entregado) y si owner_gate.py cambió desde la autoprueba y la batería completa de mutación (mutation-result.json).

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
RUN = r"D:\r62-arch-a7r4-run"
RUN_FILES = ["order.txt", "prompt.md", "a7-r3.md"]
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
    sf = json.load(open(os.path.join(KIT, "selftest-fidelity-result.json"), encoding="utf-8"))
    fp = os.path.join(KIT, "mutation-result.json")
    mutated = {}
    if os.path.isfile(fp):
        fu = json.load(open(fp, encoding="utf-8"))
        mutated = {n: r.get("Sha256") for n, r in (fu.get("Modules") or {}).items()}
        stale = sorted(n for n, s in mutated.items() if not os.path.isfile(os.path.join(KIT, n)) or rec(os.path.join(KIT, n))["Sha256"] != s)
        mutation = ("batería COMPLETA de mutación (decisiones §88, punto 4: cambiaron reglas del auditor y la comprobación 7 del lanzador) "
                    "mutation-result.json AllKilled %s, %d/%d (%s)%s" % (fu["AllKilled"], fu["Totals"]["Killed"], fu["Totals"]["Mutants"],
                                                                        ", ".join("%s %d" % (n, r["Sites"]) for n, r in fu["Modules"].items()),
                                                                        "; MÓDULOS CAMBIADOS DESDE LA BATERÍA: %s" % stale if stale else ""))
    else:
        mutation = "batería completa de mutación PENDIENTE (se ejecuta una sola vez, al final; el manifiesto se regenera después)"
    mutated_og = mutated.get("owner_gate.py")
    import importlib.util
    spec = importlib.util.spec_from_file_location("owner_gate", os.path.join(KIT, "owner_gate.py"))
    og = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(og)
    pins = og.pinned_authorization()
    og_now = rec(os.path.join(KIT, "owner_gate.py"))["Sha256"]
    t = closure["Transport"]
    return {
        "Kit": "I-62: kit de la ÚNICA revisión formal independiente del Architect de la RONDA 4 de la enmienda A-7 por claude-cli, acotada a "
               "A62-A7R3-R1 y a sus consecuencias (decisiones §98, puntos 2, 3, 6 y 7; Owner CLAUDE-CLI-I62 = A; lanzamiento con la orden congelada "
               "exacta y condicionado a la sonda de registro y a la compuerta 11: línea del Owner de §78, blobs de A-7 ronda 4 y de la A-6 acordada en "
               "la HEAD y una disposición del Coordinator posterior a §98 que nombre esta revisión, fijada en owner_gate.py, sin ninguna sección "
               "posterior)",
        "Ids": closure["Ids"], "PreviousAttempts": [],
        "PreviousRound": ("A-7 ronda 3: R20261023T180846Z-62bd (blob 8bcccb2f; auditor NOT_ACCREDITED por una referencia no canónica del revisor; "
                          "Architect CHANGES REQUIRED con A62-A7R3-R1; custodia 3f912c84; decisiones §97 y §98)"),
        "Commit": closure["AuthorityRevision"],
        "Object": "%s (blob %s; publicación %s)" % (closure["ObjectPath"], closure["ObjectBlob"], closure["Publication"]["Commit"]),
        "Package": "ninguno (%s)" % closure["PackageNote"],
        "SessionId": t["SessionId"],
        "Transport": "claude-cli %s (%s, SHA-256 %s)" % (t["Binary"]["Version"].split()[0], t["Binary"]["Path"], t["Binary"]["Sha256"]),
        "Auditor": "v7.0-a7r4 (declarado y autoprobado antes del lanzamiento: selftest-result.json AllPass %s, %d/%d; selftest-fidelity-result.json "
                   "AllPass %s, %d/%d; %s)" % (st["AllPass"], st["Totals"]["Pass"], st["Totals"]["Cases"], sf["AllPass"], sf["Totals"]["Pass"],
                                              sf["Totals"]["Cases"], mutation),
        "Clone": "D:/r62-arch-a7r4 (git clone --no-local -c core.autocrlf=false --single-branch desde el directorio común de git; HEAD separado en "
                 "el commit, sin ramas locales; sin remoto; clone-verification.json AllChecks, con la reproducción de las guardas de la ronda 4)",
        "Run": "D:/r62-arch-a7r4-run (order.txt, prompt.md, a7-r3.md y %d copias fieles de los archivos que Read no entrega fielmente, decisiones "
               "§94, punto 5; idénticos byte a byte a las copias del kit)" % len(closure.get("FaithfulCopies") or []),
        "TransportGate": "%s (D-01; launch.py solo lanza en MEASURED o ACCEPTED)" % gate_status(),
        "OwnerAndAuthorizationGate": "%s (owner_gate.py): launch.py solo lanza con la línea del Owner A6-ADR0046-9-FX02 (SHA-256 %s…) después de §78, "
                                     "CLAUDE-CLI-I62 = A vigente, A-7 en la HEAD con el blob %s, la A-6 acordada y una disposición del Coordinator "
                                     "posterior a §98 fijada en owner_gate.py (número, encabezado y SHA-256 de su texto, con el blob completo) sin "
                                     "ninguna sección posterior; se evalúa en el momento del lanzamiento"
                                     % (closure["OwnerGate"]["Check"], closure["OwnerGate"]["A6LineSha256"][:8], closure["ObjectBlob"][:8]),
        "AuthorizationPins": pins,
        "AuthorizationState": ("PENDIENTE: owner_gate.py entrega AUTH_SECTION, AUTH_HEADING y AUTH_TEXT_SHA en None; la compuerta 11 bloquea hasta "
                               "fijar una disposición del Coordinator posterior a §98 que nombre esta revisión (pin_authorization.py) y volver a "
                               "congelar el kit" if og.authorization_pending(pins) else
                               "FIJADA: decisiones §%s (encabezado y SHA-256 del texto en AuthorizationPins)" % pins["Section"]),
        "OwnerGateTested": {"SelftestSha256": (st.get("Modules") or {}).get("owner_gate.py"), "MutationSha256": mutated_og, "CurrentSha256": og_now,
                            "ChangedSinceSelftestOrMutation": og_now not in ((st.get("Modules") or {}).get("owner_gate.py"), ) or
                            (mutated_og is not None and og_now != mutated_og)},
        "LaunchCommand": closure["Transport"]["LaunchCommand"],
        "Launch": "%s desde la carpeta que contiene kit/, por la sesión principal, una sola vez, sin flags ni variables añadidos (NO lanzado)"
                  % closure["Transport"]["LaunchCommand"],
    }


def run_files():
    """Los archivos del run del cierre: order.txt, prompt.md y las copias fieles (decisiones §94, punto 5; fidelity.run_files_of)."""
    import importlib.util
    spec = importlib.util.spec_from_file_location("fidelity", os.path.join(KIT, "fidelity.py"))
    fid = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(fid)
    names = fid.run_files_of(json.load(open(os.path.join(KIT, "closure.json"), encoding="utf-8")))
    assert names[:len(RUN_FILES)] == RUN_FILES, "RunFiles del cierre no empieza por %s" % RUN_FILES
    out = {}
    for n in names:
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
    now_run = run_files()
    changed += ["run/" + n for n, r in now_run.items() if {k: r[k] for k in ("Bytes", "Sha256")} != {k: (cur["RunFiles"].get(n) or {}).get(k) for k in ("Bytes", "Sha256")}]
    changed += ["run/" + n for n in cur["RunFiles"] if n not in now_run]
    if changed:
        print(json.dumps({"Refused": "además de transport-gate.json cambiaron otros archivos: no se escribe nada", "Changed": changed}, ensure_ascii=False))
        return 2
    cur["Files"][GATE] = rec(os.path.join(KIT, GATE))
    cur["TransportGate"] = header()["TransportGate"]
    write(cur)
    return 0


if __name__ == "__main__":
    sys.exit(main(sys.argv[1:]))
