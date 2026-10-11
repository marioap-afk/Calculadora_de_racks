"""Termina el kit de la primera revisión formal de A-6 (decisiones §77, punto 4). Lo ejecuta quien prepara el kit, una vez, desde el directorio
kit/ o con su ruta. NO lanza el revisor y NO ejecuta el binario de claude (launch.py solo en --dry-run). Adaptado del finish_kit.py del kit de B-min.

Pasos, en orden; se detiene en el primero que falla (código distinto de 0) y no deshace nada:
  1. prepare_clone.py [<CLONE_COMMIT>]                 → D:\\r62-arch-a6, D:\\r62-arch-a6-run\\order.txt y kit/kit-params.json
  2. make_closure.py --render-prompt                   → kit/prompt.md (desde prompt.template.md)
  3. copia byte a byte de kit/prompt.md                → D:\\r62-arch-a6-run\\prompt.md
  4. make_closure.py                                   → kit/closure.json
  5. transport_gate.py init y set MEASURED <registro de caracterización del clon> → kit/transport-gate.json (MEASURED, SHA-256 20d0de2c…)
  6. verify_clone.py                                   → kit/clone-verification.json (AllChecks, con la reproducción de las guardas de A-6)
  7. selftest-post-review.py kit/selftest-result.json  → AllPass
  8. mutation-post-review.py kit/mutation-result.json  → AllKilled (con --no-mutation se omite y el manifiesto lo declara)
  9. make_manifest.py                                  → kit/kit-manifest.json
 10. launch.py --dry-run                               → ../launch-dry-run.json: la ÚNICA comprobación bloqueante admitida es 11-OwnerGate
                                                         (la línea del Owner A6-ADR0046-9-FX02 todavía no está registrada); 4 y 5 sin ejecutar;
                                                         código 2 siempre en seco. Cualquier otra comprobación bloqueante detiene el paso.
Uso: python -B finish_kit.py [<CLONE_COMMIT>] [--no-mutation]
"""
import hashlib
import json
import os
import re
import shutil
import subprocess
import sys

KIT = os.path.dirname(os.path.abspath(__file__))
RUN = r"D:\r62-arch-a6-run"
CHAR = r"D:\r62-arch-a6\docs\automation\evidence\I-62-claude-cli\R20261008T183600Z-char\claude-cli-characterization.json"
CHAR_SHA = "20d0de2ce625c38ddeaaf445ce2261b080520bdcf57eec8f4bf9a8bd0cedbf4c"
EXPECTED_BLOCKING_NOW = ["11-OwnerGate"]
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


def main(argv):
    pos = [a for a in argv if not a.startswith("--")]
    if len(pos) > 1:
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
    out = step("10 launch --dry-run", ["launch.py", "--dry-run"], ok_codes=(2,),
               capture_to=os.path.join(os.path.dirname(KIT), "launch-dry-run.json"))
    d = json.loads(out)
    if d.get("BlockingChecks") != EXPECTED_BLOCKING_NOW:
        raise SystemExit("STOP: la preflight en seco tiene comprobaciones bloqueantes distintas de %s: %s" % (EXPECTED_BLOCKING_NOW, d.get("BlockingChecks")))
    m = hashlib.sha256(open(os.path.join(KIT, "kit-manifest.json"), "rb").read()).hexdigest()
    print(json.dumps({"KitReady": True, "KitManifestSha256": m, "BlockingChecks": d.get("BlockingChecks"), "NotRunInDryRun": d.get("NotRunInDryRun"),
                      "OwnerGate": {k: d["Checks"]["11-OwnerGate"].get(k) for k in ("Ok", "Problems", "A6LineNumbers", "ClaudeCliLineNumbers")},
                      "Next": "registrar la línea del Owner (si el Owner la da), volver a la preflight en seco (11-OwnerGate en Ok), custodiar el kit y "
                              "lanzar una vez: python -B kit/launch.py"}, ensure_ascii=False))
    return 0


if __name__ == "__main__":
    sys.stdout.reconfigure(errors="backslashreplace")
    sys.exit(main(sys.argv[1:]))
