"""Termina el kit del dictamen de materialidad de B-min cuando existe el commit del paquete (PKG_COMMIT). Lo ejecuta la sesión principal, una vez,
desde el directorio kit/ o con su ruta. NO lanza el revisor y NO ejecuta el binario de claude (launch.py solo en --dry-run).

Pasos, en orden; se detiene en el primero que falla (código distinto de 0) y no deshace nada:
  1. prepare_clone.py <PKG_COMMIT> [<CLONE_COMMIT>]   → D:\\r62-arch-bmin, D:\\r62-arch-bmin-run\\order.txt y kit/kit-params.json
  2. make_closure.py --render-prompt                   → kit/prompt.md (desde prompt.template.md)
  3. copia byte a byte de kit/prompt.md                → D:\\r62-arch-bmin-run\\prompt.md
  4. make_closure.py                                   → kit/closure.json
  5. transport_gate.py init y set MEASURED <registro de caracterización del clon> → kit/transport-gate.json (MEASURED, SHA-256 20d0de2c…)
  6. verify_clone.py                                   → kit/clone-verification.json (AllChecks)
  7. selftest-post-review.py kit/selftest-result.json  → AllPass
  8. mutation-post-review.py kit/mutation-result.json  → AllKilled (≈ 1 h; con --no-mutation se omite y el manifiesto lo declara)
  9. make_manifest.py                                  → kit/kit-manifest.json
 10. launch.py --dry-run                               → ../launch-dry-run.json (sin bloqueos; 4 y 5 sin ejecutar; código 2 siempre en seco)
Uso: python -B finish_kit.py <PKG_COMMIT> [<CLONE_COMMIT>] [--no-mutation]
"""
import hashlib
import json
import os
import re
import shutil
import subprocess
import sys

KIT = os.path.dirname(os.path.abspath(__file__))
RUN = r"D:\r62-arch-bmin-run"
CHAR = r"D:\r62-arch-bmin\docs\automation\evidence\I-62-claude-cli\R20261008T183600Z-char\claude-cli-characterization.json"
CHAR_SHA = "20d0de2ce625c38ddeaaf445ce2261b080520bdcf57eec8f4bf9a8bd0cedbf4c"
USER_RX = re.compile(r"(?i)[a-z]:(?:\\\\|\\|/)+users(?:\\\\|\\|/)+[^\\/\"\s]+")


def step(name, args, ok_codes=(0,), capture_to=None):
    r = subprocess.run([sys.executable, "-B"] + args, cwd=KIT, capture_output=True, text=True, encoding="utf-8", errors="replace")
    out = USER_RX.sub("%USERPROFILE%", r.stdout)
    if capture_to:
        with open(capture_to, "w", encoding="utf-8", newline="\n") as f:
            f.write(out)
    print("== %s (código %d)" % (name, r.returncode))
    print(out[-3000:] if not capture_to else out[-600:])
    if r.returncode not in ok_codes:
        print(USER_RX.sub("%USERPROFILE%", r.stderr[-3000:]))
        raise SystemExit("STOP en el paso «%s»: no se continúa" % name)
    return out


def main(argv):
    pos = [a for a in argv if not a.startswith("--")]
    if not pos or len(pos) > 2:
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
    if d.get("BlockingChecks"):
        raise SystemExit("STOP: la preflight en seco tiene comprobaciones bloqueantes: %s" % d["BlockingChecks"])
    m = hashlib.sha256(open(os.path.join(KIT, "kit-manifest.json"), "rb").read()).hexdigest()
    print(json.dumps({"KitReady": True, "KitManifestSha256": m, "NotRunInDryRun": d.get("NotRunInDryRun"),
                      "Next": "custodiar el kit (commit de custodia) y lanzar una vez: python -B kit/launch.py"}, ensure_ascii=False))
    return 0


if __name__ == "__main__":
    sys.stdout.reconfigure(errors="backslashreplace")
    sys.exit(main(sys.argv[1:]))
