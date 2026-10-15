"""Termina el kit de la ÚNICA revisión formal de la ronda 4 de A-6 (decisiones §85, punto 6). Lo ejecuta quien prepara el kit, una vez, desde el
directorio kit/ o con su ruta. NO lanza el revisor y NO ejecuta el binario de claude (launch.py solo con --record-probe y --dry-run). Derivado del
finish_kit.py del kit acreditado de la ronda 3 (prep/adapt_misc.py).

Pasos, en orden; se detiene en el primero que falla (código distinto de 0) y no deshace nada:
  1. prepare_clone.py <CLONE_COMMIT> <PUBLICATION> <GUARDS_RUN_COMMIT> → D:\\r62-arch-a6r4, D:\\r62-arch-a6r4-run\\{order.txt, a6-r3.md} y kit-params.json
  2. make_closure.py --render-prompt                   → kit/prompt.md (desde prompt.template.md, con la lista G11 del resultado `run` custodiado)
  3. copia byte a byte de kit/prompt.md                → D:\\r62-arch-a6r4-run\\prompt.md
  4. make_closure.py                                   → kit/closure.json (cierre fijado por blob en CLONE_COMMIT)
  5. transport_gate.py init y set MEASURED <registro de caracterización del clon> → kit/transport-gate.json (MEASURED, SHA-256 20d0de2c…)
  6. verify_clone.py                                   → kit/clone-verification.json (AllChecks, con la reproducción de las guardas de la ronda 4)
  7. selftest-post-review.py kit/selftest-result.json  → AllPass, sobre el clon y el run reales (L00: la compuerta 11 real, PENDIENTE mientras no
                                                         se fije la autorización, con el único problema «authorization pending: …»)
  8. mutation-post-review.py kit/mutation-result.json  → AllKilled (con --no-mutation se omite y el manifiesto lo declara)
  9. make_manifest.py                                  → kit/kit-manifest.json (congelación del kit)
 10. python -B kit/launch.py --record-probe            → la orden congelada exacta, desde la carpeta que contiene kit/: código 0 (sonda de registro)
 11. python -B kit/launch.py --dry-run                 → ../launch-dry-run.json: código 2 siempre en seco; con la autorización pendiente se exige
                                                         que el ÚNICO bloqueo sea la compuerta 11 con el único problema «authorization pending: …»,
                                                         FrozenCommandGuard y RecordProbe en Ok, AuthorizationSections = [] y LaterSections = [];
                                                         4 y 5 sin ejecutar
Los pasos 10 y 11 se ejecutan SIN flags ni variables añadidos (la orden congelada): la guarda de launch.py los rechazaría.
Uso: python -B finish_kit.py <CLONE_COMMIT> <PUBLICATION> <GUARDS_RUN_COMMIT> [--no-mutation]
"""
import hashlib
import json
import os
import re
import shutil
import subprocess
import sys

KIT = os.path.dirname(os.path.abspath(__file__))
RUN = r"D:\r62-arch-a6r4-run"
CHAR = r"D:\r62-arch-a6r4\docs\automation\evidence\I-62-claude-cli\R20261008T183600Z-char\claude-cli-characterization.json"
CHAR_SHA = "20d0de2ce625c38ddeaaf445ce2261b080520bdcf57eec8f4bf9a8bd0cedbf4c"
GATE11 = "11-OwnerAndAuthorizationGate"
PENDING = "authorization pending: "   # owner_gate.PENDING_REASON: la autorización posterior a §85 todavía no está fijada
USER_RX = re.compile(r"(?i)[a-z]:(?:\\\\|\\|/)+users(?:\\\\|\\|/)+[^\\/\"\s]+")


def step(name, args, ok_codes=(0,), capture_to=None):
    r = subprocess.run([sys.executable, "-X", "utf8", "-B"] + args, cwd=KIT, capture_output=True, text=True, encoding="utf-8", errors="replace")
    out = USER_RX.sub("%USERPROFILE%", r.stdout)
    if capture_to:
        with open(capture_to, "w", encoding="utf-8", newline="\n") as f:
            f.write(out)
    print("== %s (código %d)" % (name, r.returncode))
    print(out[-3000:] if not capture_to else out[-600:])
    sys.stdout.flush()
    if r.returncode not in ok_codes:
        print(USER_RX.sub("%USERPROFILE%", r.stderr[-3000:]))
        raise SystemExit("STOP en el paso «%s»: no se continúa" % name)
    return out


def frozen(args, ok_codes, capture_to=None):
    """La orden congelada exacta (decisiones §84, punto 2): `python -B kit/launch.py [modo]` con cwd = la carpeta que contiene kit/; sin -X ni
    variables añadidas (el entorno se hereda sin cambios). La salida llega en la codificación de la tubería del hijo (la preferida por el locale,
    que el propio registro declara en FrozenCommandGuard.Encodings.Stdout) y se guarda en UTF-8."""
    import locale
    r = subprocess.run(["python", "-B", "kit/launch.py"] + args, cwd=os.path.dirname(KIT), capture_output=True)
    out = r.stdout.decode(locale.getpreferredencoding(False), "replace")
    out = USER_RX.sub("%USERPROFILE%", out)
    if capture_to:
        with open(capture_to, "w", encoding="utf-8", newline="\n") as f:
            f.write(out)
    print("== python -B kit/launch.py %s (código %d)" % (" ".join(args), r.returncode))
    print(out[-1500:])
    if r.returncode not in ok_codes:
        raise SystemExit("STOP en python -B kit/launch.py %s: código %d" % (" ".join(args), r.returncode))
    return out


def main(argv):
    pos = [a for a in argv if not a.startswith("--")]
    if len(pos) != 3:
        print(__doc__)
        return 2
    step("1 prepare_clone", ["prepare_clone.py"] + pos)
    step("2 render prompt", ["make_closure.py", "--render-prompt"])
    shutil.copyfile(os.path.join(KIT, "prompt.md"), os.path.join(RUN, "prompt.md"))
    print("== 3 prompt.md copiado al run")
    step("4 closure", ["make_closure.py"])
    if hashlib.sha256(open(CHAR, "rb").read()).hexdigest() != CHAR_SHA:
        raise SystemExit("STOP: el registro de caracterización del clon no tiene el SHA-256 fijado")
    step("5a gate init", ["transport_gate.py", "init"])
    step("5b gate set MEASURED", ["transport_gate.py", "set", "MEASURED", CHAR])
    step("6 verify_clone", ["verify_clone.py"])
    step("7 selftest", ["selftest-post-review.py", os.path.join(KIT, "selftest-result.json")])
    if "--no-mutation" not in argv:
        step("8 mutation", ["mutation-post-review.py", os.path.join(KIT, "mutation-result.json")])
    step("9 manifest", ["make_manifest.py"])
    frozen(["--record-probe"], ok_codes=(0,), capture_to=os.path.join(os.path.dirname(KIT), "record-probe.json"))
    out = frozen(["--dry-run"], ok_codes=(2,), capture_to=os.path.join(os.path.dirname(KIT), "launch-dry-run.json"))
    d = json.loads(out)
    g = d["Checks"][GATE11]
    if d.get("BlockingChecks") != [GATE11] or d.get("RecordProbeOk") is not True or (d.get("FrozenCommandGuard") or {}).get("Ok") is not True or \
            d.get("LaterSections") != [] or d.get("AuthorizationSections") != [] or g.get("AuthorizationPending") is not True or \
            len(g.get("Problems") or []) != 1 or not g["Problems"][0].startswith(PENDING):
        raise SystemExit("STOP: la preflight en seco no se bloquea SOLO por la autorización pendiente: %s / %s" % (d.get("BlockingChecks"),
                                                                                                                 g.get("Problems")))
    m = hashlib.sha256(open(os.path.join(KIT, "kit-manifest.json"), "rb").read()).hexdigest()
    print(json.dumps({"KitReady": True, "KitManifestSha256": m, "BlockingChecks": d.get("BlockingChecks"), "NotRunInDryRun": d.get("NotRunInDryRun"),
                      "Gate11": {k: g.get(k) for k in ("Ok", "Problems", "A6LineNumbers", "ClaudeCliLineNumbers", "A6BlobAtHeadMatches",
                                                       "AuthorizationPending", "AuthorizationSections", "LaterSections", "SectionsAfterBase")},
                      "Next": "custodiar el kit; esperar la disposición del Coordinator posterior a §85 que autorice el lanzamiento; fijarla con "
                              "pin_authorization.py y volver a congelar (make_manifest.py); repetir la sonda de registro y la preflight en seco con la "
                              "orden congelada (BlockingChecks = [], AuthorizationSections = esa sección, LaterSections = []) y lanzar una vez, desde "
                              "la carpeta que contiene kit/: python -B kit/launch.py"}, ensure_ascii=False))
    return 0


if __name__ == "__main__":
    sys.stdout.reconfigure(errors="backslashreplace")
    sys.exit(main(sys.argv[1:]))
