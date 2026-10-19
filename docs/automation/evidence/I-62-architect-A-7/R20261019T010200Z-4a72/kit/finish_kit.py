"""Termina el kit de la ÚNICA revisión formal de la enmienda A-7 (decisiones §93). Lo ejecuta quien prepara el kit, una vez, desde el directorio kit/
o con su ruta. NO lanza el revisor y NO ejecuta el binario de claude (launch.py solo con --record-probe y --dry-run). Derivado del finish_kit.py del
kit acreditado de A-6 ronda 4 (R20261014T180556Z-5e02): los mismos pasos y las mismas funciones step y frozen; cambian el clon y el run, el registro
de caracterización (la copia de kit/, decisiones §87), la fijación de §93 (que ya existe al preparar el kit: paso 5), la mutación dirigida y la
condición de la prueba en seco (cero bloqueos).

Pasos, en orden; se detiene en el primero que falla (código distinto de 0) y no deshace nada:
  1. prepare_clone.py <CLONE_COMMIT> <PUBLICATION> <GUARDS_RUN_COMMIT> → D:\\r62-arch-a7, D:\\r62-arch-a7-run\\order.txt y kit-params.json
  2. make_closure.py --render-prompt                   → kit/prompt.md (desde prompt.template.md)
  3. copia byte a byte de kit/prompt.md                → D:\\r62-arch-a7-run\\prompt.md
  4. make_closure.py                                   → kit/closure.json (cierre fijado por blob en CLONE_COMMIT)
  5. fijación de §93: el texto de decisiones de la HEAD del worktree (git cat-file blob, solo lectura) → pin_authorization.py <texto> 93 (Ready) →
     las tres líneas AUTH_* de kit/owner_gate.py (solo esas tres cambian)
  6. transport_gate.py init y set MEASURED kit/claude-cli-characterization.json → kit/transport-gate.json (MEASURED, SHA-256 4ba6ba58…)
  7. verify_clone.py                                   → kit/clone-verification.json (AllChecks, con la reproducción de las guardas de A-7)
  8. selftest-post-review.py kit/selftest-result.json  → AllPass, sobre el clon y el run reales (L00: la compuerta 11 real, FIJADA en §93)
  9. mutation-targeted.py kit/mutation-targeted-result.json → AllKilled en los sitios cambiados (con --no-mutation se omite y el manifiesto lo declara)
 10. make_manifest.py                                  → kit/kit-manifest.json (congelación del kit)
 11. python -B kit/launch.py --record-probe            → la orden congelada exacta, desde la carpeta que contiene kit/: código 0 (sonda de registro)
 12. python -B kit/launch.py --dry-run                 → ../launch-dry-run.json: código 2 siempre en seco; se exige BlockingChecks = [],
                                                         FrozenCommandGuard y RecordProbe en Ok, AuthorizationSections = [§93 con HeadingMatches, HasBlob
                                                         y TextSha256Matches] y LaterSections = []; 4 y 5 sin ejecutar
Los pasos 11 y 12 se ejecutan SIN flags ni variables añadidos (la orden congelada): la guarda de launch.py los rechazaría.
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
RUN = r"D:\r62-arch-a7-run"
CHAR = os.path.join(KIT, "claude-cli-characterization.json")
CHAR_SHA = "4ba6ba586440ce5c9374c383a9a1b787cf42a17d8451c5d27e762c555cfc2a8c"
WORKTREE = os.path.join(os.environ["USERPROFILE"], ".codex", "worktrees", "architecture-portabilidad-coordinador-principal")   # solo lectura
GATE11 = "11-OwnerAndAuthorizationGate"
AUTH_SECTION = 93
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


def pin_section():
    """Paso 5: fija §93 en owner_gate.py con los valores de pin_authorization.py sobre el texto de decisiones de la HEAD del worktree (solo esas tres
    líneas cambian)."""
    tmp = os.path.join(os.path.dirname(KIT), "prep", "pin93")
    os.makedirs(tmp, exist_ok=True)
    dec = subprocess.run(["git", "-C", WORKTREE, "cat-file", "blob", "HEAD:docs/automation/decisions/I-62.md"], capture_output=True,
                         env=dict(os.environ, GIT_OPTIONAL_LOCKS="0")).stdout
    path = os.path.join(tmp, "decisiones-head.txt")
    open(path, "wb").write(dec)
    out = step("5 pin_authorization", ["pin_authorization.py", path, str(AUTH_SECTION)])
    pin = json.loads(out)
    assert pin["Ready"] is True and len(pin["PythonLines"]) == 3
    p = os.path.join(KIT, "owner_gate.py")
    lines = open(p, encoding="utf-8").read().split("\n")
    for pre, new in zip(("AUTH_SECTION = ", "AUTH_HEADING = ", "AUTH_TEXT_SHA = "), pin["PythonLines"]):
        hits = [i for i, l in enumerate(lines) if l.startswith(pre)]
        assert len(hits) == 1, (pre, hits)
        lines[hits[0]] = new
    open(p, "w", encoding="utf-8", newline="\n").write("\n".join(lines))
    print("== 5 §93 fijada en owner_gate.py: %s" % pin["Values"]["AUTH_TEXT_SHA"])


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
    pin_section()
    if hashlib.sha256(open(CHAR, "rb").read()).hexdigest() != CHAR_SHA:
        raise SystemExit("STOP: la copia del registro de caracterización no tiene el SHA-256 fijado")
    step("6a gate init", ["transport_gate.py", "init"])
    step("6b gate set MEASURED", ["transport_gate.py", "set", "MEASURED", CHAR])
    step("7 verify_clone", ["verify_clone.py"])
    step("8 selftest", ["selftest-post-review.py", os.path.join(KIT, "selftest-result.json")])
    if "--no-mutation" not in argv:
        step("9 mutation (dirigida)", ["mutation-targeted.py", os.path.join(KIT, "mutation-targeted-result.json")])
    step("10 manifest", ["make_manifest.py"])
    frozen(["--record-probe"], ok_codes=(0,), capture_to=os.path.join(os.path.dirname(KIT), "record-probe.json"))
    out = frozen(["--dry-run"], ok_codes=(2,), capture_to=os.path.join(os.path.dirname(KIT), "launch-dry-run.json"))
    d = json.loads(out)
    g = d["Checks"][GATE11]
    auth = d.get("AuthorizationSections") or []
    if d.get("BlockingChecks") != [] or d.get("RecordProbeOk") is not True or (d.get("FrozenCommandGuard") or {}).get("Ok") is not True or \
            d.get("LaterSections") != [] or len(auth) != 1 or auth[0].get("Number") != AUTH_SECTION or not auth[0].get("HeadingMatches") or \
            not auth[0].get("HasBlob") or not auth[0].get("TextSha256Matches") or g.get("AuthorizationPending") is not False:
        raise SystemExit("STOP: la preflight en seco no queda sin bloqueos con §93 fijada: %s / %s" % (d.get("BlockingChecks"), g.get("Problems")))
    m = hashlib.sha256(open(os.path.join(KIT, "kit-manifest.json"), "rb").read()).hexdigest()
    print(json.dumps({"KitReady": True, "KitManifestSha256": m, "BlockingChecks": d.get("BlockingChecks"), "NotRunInDryRun": d.get("NotRunInDryRun"),
                      "Gate11": {k: g.get(k) for k in ("Ok", "Problems", "A6LineNumbers", "ClaudeCliLineNumbers", "ObjectBlobAtHeadMatches",
                                                       "A6AgreedBlobAtHeadMatches", "AuthorizationPending", "AuthorizationSections", "LaterSections",
                                                       "SectionsAfterBase")},
                      "Next": "custodiar el kit y, si la sesión principal lo decide con §93 vigente, lanzar una vez desde la carpeta que contiene kit/: "
                              "python -B kit/launch.py"}, ensure_ascii=False))
    return 0


if __name__ == "__main__":
    sys.stdout.reconfigure(errors="backslashreplace")
    sys.exit(main(sys.argv[1:]))
