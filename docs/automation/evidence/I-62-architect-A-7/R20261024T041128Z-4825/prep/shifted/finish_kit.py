"""Termina el kit de la ÚNICA revisión formal de la RONDA 4 de la enmienda A-7 (decisiones §98, puntos 6 y 7). Lo ejecuta quien prepara el kit, una vez,
desde el directorio kit/ o con su ruta. NO lanza el revisor y NO ejecuta el binario de claude (launch.py solo con --record-probe y --dry-run).
Derivado del finish_kit.py del kit de A-7 ronda 3 (R20261023T180846Z-62bd, a su vez del de la ronda 1): las mismas funciones step y frozen y la
misma secuencia; cambian el clon y el run (D:\\r62-arch-a7r4*), la premisa (a7-r3.md) y la sección base (§98). La autorización se entrega
PENDIENTE (el paso 5 lo comprueba), la mutación es la batería COMPLETA de mutation-post-review.py (decisiones §88, punto 4: cambiaron el auditor,
v7.0-a7r4, y el esquema del resultado; una sola vez, al final) y la prueba en seco exige BlockingChecks = solo la compuerta 11, con el único
motivo de la autorización pendiente.

Pasos, en orden; se detiene en el primero que falla (código distinto de 0) y no deshace nada:
  1. prepare_clone.py <CLONE_COMMIT> <PUBLICATION> <GUARDS_RUN_COMMIT> → D:\\r62-arch-a7r4, D:\\r62-arch-a7r4-run\\{order.txt,a7-r3.md} y
     kit-params.json (con --clone-ready se omite: prepare_clone.py se ejecuta una sola vez y ya se ejecutó; se exige kit-params.json con esos commits)
  2. make_closure.py --render-prompt                   → kit/prompt.md (desde prompt.template.md)
  2b. make_closure.py --faithful-copies                → kit/fiel__*.txt (copias fieles de los archivos marcados; decisiones §94, punto 5)
  3. copia byte a byte de kit/prompt.md y de kit/fiel__*.txt → D:\\r62-arch-a7r4-run\\
  4. make_closure.py                                   → kit/closure.json (cierre fijado por blob en CLONE_COMMIT)
  5. autorización PENDIENTE: owner_gate.py con AUTH_SECTION, AUTH_HEADING y AUTH_TEXT_SHA en None (no se fija nada; la fijará, si la hay, una
     disposición del Coordinator posterior a §98 con pin_authorization.py, y el kit se volverá a congelar)
  6. transport_gate.py init y set MEASURED kit/claude-cli-characterization.json → kit/transport-gate.json (MEASURED, SHA-256 4ba6ba58…)
  7. verify_clone.py                                   → kit/clone-verification.json (AllChecks, con la reproducción de las guardas de la ronda 4)
  8. selftest-post-review.py kit/selftest-result.json  → AllPass, sobre el clon y el run reales (L00: la compuerta 11 real, PENDIENTE)
  8b. selftest-fidelity.py kit/selftest-fidelity-result.json → AllPass (detector, copias fieles y sus negativos; decisiones §94, punto 5)
  9. make_manifest.py                                  → kit/kit-manifest.json (congelación previa a la batería)
 10. python -B kit/launch.py --record-probe            → la orden congelada exacta, desde la carpeta que contiene kit/: código 0 (sonda de registro)
 11. python -B kit/launch.py --dry-run                 → ../launch-dry-run-before-full-mutation.json: código 2 siempre en seco; se exige
                                                         BlockingChecks = [11-OwnerAndAuthorizationGate] con Problems = [PENDING_REASON],
                                                         AuthorizationPending, FrozenCommandGuard y RecordProbe en Ok, AuthorizationSections = []
                                                         y LaterSections = []; 4 y 5 sin ejecutar
 12. mutation-post-review.py kit/mutation-result.json  → AllKilled (batería COMPLETA de los cuatro módulos; con --no-mutation se omite y el
                                                         manifiesto lo declara), y después 9-11 otra vez (manifiesto final, ../record-probe.json y
                                                         ../launch-dry-run.json con la misma condición)
Los pasos 10 y 11 se ejecutan SIN flags ni variables añadidos (la orden congelada): la guarda de launch.py los rechazaría.
Uso: python -B finish_kit.py <CLONE_COMMIT> <PUBLICATION> <GUARDS_RUN_COMMIT> [--clone-ready] [--no-mutation]
"""
import hashlib
import json
import os
import re
import shutil
import subprocess
import sys

KIT = os.path.dirname(os.path.abspath(__file__))
RUN = r"D:\r62-arch-a7r4-run"
CHAR = os.path.join(KIT, "claude-cli-characterization.json")
CHAR_SHA = "4ba6ba586440ce5c9374c383a9a1b787cf42a17d8451c5d27e762c555cfc2a8c"
GATE11 = "11-OwnerAndAuthorizationGate"
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


def owner_gate():
    import importlib.util
    spec = importlib.util.spec_from_file_location("owner_gate", os.path.join(KIT, "owner_gate.py"))
    og = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(og)
    return og


def check_pending():
    """Paso 5: la autorización se entrega PENDIENTE (decisiones §98, punto 7: la revisión exige una disposición del Coordinator posterior)."""
    og = owner_gate()
    if not og.authorization_pending(og.pinned_authorization()) or (og.AUTH_SECTION, og.AUTH_HEADING, og.AUTH_TEXT_SHA) != (None, None, None):
        raise SystemExit("STOP: owner_gate.py no entrega la autorización pendiente")
    print("== 5 autorización PENDIENTE en owner_gate.py (AUTH_* = None; base §%d)" % og.BASE_SECTION)


def probe_and_dry_run(dry_name):
    """Pasos 10 y 11: sonda de registro y prueba en seco; exige BlockingChecks = solo la compuerta 11, bloqueada por el único motivo pendiente."""
    og = owner_gate()
    frozen(["--record-probe"], ok_codes=(0,), capture_to=os.path.join(os.path.dirname(KIT), "record-probe.json"))
    out = frozen(["--dry-run"], ok_codes=(2,), capture_to=os.path.join(os.path.dirname(KIT), dry_name))
    d = json.loads(out)
    g = d["Checks"][GATE11]
    if d.get("BlockingChecks") != [GATE11] or g.get("Problems") != [og.PENDING_REASON] or g.get("AuthorizationPending") is not True or \
            d.get("RecordProbeOk") is not True or (d.get("FrozenCommandGuard") or {}).get("Ok") is not True or \
            (d.get("AuthorizationSections") or []) != [] or (d.get("LaterSections") or []) != [] or \
            sorted(d.get("NotRunInDryRun") or []) != ["4-Version", "5-Auth"]:
        raise SystemExit("STOP: la preflight en seco no queda bloqueada SOLO por la autorización pendiente: %s / %s"
                         % (d.get("BlockingChecks"), g.get("Problems")))
    return d, g


def main(argv):
    pos = [a for a in argv if not a.startswith("--")]
    if len(pos) != 3:
        print(__doc__)
        return 2
    if "--clone-ready" in argv:
        p = json.load(open(os.path.join(KIT, "kit-params.json"), encoding="utf-8"))
        if [p.get("CloneCommit"), p.get("Publication"), p.get("GuardsRunCommit")] != pos or p.get("Provisional") is not False:
            raise SystemExit("STOP: --clone-ready con un kit-params.json de otros commits")
        print("== 1 prepare_clone ya ejecutado (kit-params.json %s)" % p["PreparedAt"])
    else:
        step("1 prepare_clone", ["prepare_clone.py"] + pos)
    step("2 render prompt", ["make_closure.py", "--render-prompt"])
    step("2b faithful copies", ["make_closure.py", "--faithful-copies"])
    shutil.copyfile(os.path.join(KIT, "prompt.md"), os.path.join(RUN, "prompt.md"))
    copies = sorted(n for n in os.listdir(KIT) if n.startswith("fiel__"))
    for n in copies:
        shutil.copyfile(os.path.join(KIT, n), os.path.join(RUN, n))
    print("== 3 prompt.md y %d copias fieles copiados al run" % len(copies))
    step("4 closure", ["make_closure.py"])
    check_pending()
    if hashlib.sha256(open(CHAR, "rb").read()).hexdigest() != CHAR_SHA:
        raise SystemExit("STOP: la copia del registro de caracterización no tiene el SHA-256 fijado")
    step("6a gate init", ["transport_gate.py", "init"])
    step("6b gate set MEASURED", ["transport_gate.py", "set", "MEASURED", CHAR])
    step("7 verify_clone", ["verify_clone.py"])
    step("8 selftest", ["selftest-post-review.py", os.path.join(KIT, "selftest-result.json")])
    step("8b selftest de fidelidad", ["selftest-fidelity.py", os.path.join(KIT, "selftest-fidelity-result.json")])
    step("9 manifest", ["make_manifest.py"])
    probe_and_dry_run("launch-dry-run-before-full-mutation.json")
    if "--no-mutation" not in argv:
        step("12 mutation (batería COMPLETA)", ["mutation-post-review.py", os.path.join(KIT, "mutation-result.json")])
        step("9 manifest (final)", ["make_manifest.py"])
        d, g = probe_and_dry_run("launch-dry-run.json")
    else:
        d, g = probe_and_dry_run("launch-dry-run.json")
    m = hashlib.sha256(open(os.path.join(KIT, "kit-manifest.json"), "rb").read()).hexdigest()
    print(json.dumps({"KitReady": True, "KitManifestSha256": m, "BlockingChecks": d.get("BlockingChecks"), "NotRunInDryRun": d.get("NotRunInDryRun"),
                      "Gate11": {k: g.get(k) for k in ("Ok", "Problems", "A6LineNumbers", "ClaudeCliLineNumbers", "ObjectBlobAtHeadMatches",
                                                       "A6AgreedBlobAtHeadMatches", "AuthorizationPending", "AuthorizationSections", "LaterSections",
                                                       "SectionsAfterBase")},
                      "Next": "custodiar el kit; la revisión solo puede lanzarse después de una disposición del Coordinator posterior a §98 que la "
                              "nombre, fijada con pin_authorization.py en owner_gate.py y con el kit vuelto a congelar; entonces, una vez, desde la "
                              "carpeta que contiene kit/: python -B kit/launch.py"}, ensure_ascii=False))
    return 0


if __name__ == "__main__":
    sys.stdout.reconfigure(errors="backslashreplace")
    sys.exit(main(sys.argv[1:]))
