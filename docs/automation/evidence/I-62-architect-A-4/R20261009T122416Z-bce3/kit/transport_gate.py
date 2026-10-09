"""I-62 A-4 (blob 7d863219, recibo 02be0c34): compuerta mecánica del transporte (control D-01 del contrato de los kits de A-2 y A-3, conservado
sin cambio de predicados). La comparten launch.py (preflight 10) y post-review.py (auditor v5.1-a4); c3_analysis.py (el analizador de la sonda C3,
extraído sin cambios de probe-c3.py del kit de A-2) solo lo usa el selftest.

Por qué existe. La caracterización C1 midió la lista exacta de flags SIN `--add-dir`, y declara que un cambio en los flags del adapter deja la
observación STALE. Las decisiones §56, puntos 7-8, exigen STALE = false para que la celda sea elegible, y el paquete §5 solo permite lanzar con
una celda elegible (si no, HUMAN_LAUNCH_REQUIRED). El lanzamiento necesita `--add-dir <run>` para leer los archivos del run, fuera del cwd. Por
eso launch.py se NIEGA a lanzar mientras `transport-gate.json` no ate una de estas dos evidencias:
  * MEASURED (vía a): la sonda C3 de la caracterización R20261008T183600Z-char midió la plantilla exacta del lanzamiento (flags de C1 +
    --add-dir <dir> + --session-id + --json-schema, con marcadores): lectura dentro del directorio añadido entregada fiel y sin denegación, y
    lectura fuera del cwd y del directorio añadido denegada y no entregada; queda registrada en claude-cli-characterization.json (Probes.C3,
    Eligibility.MeasuredArgsTemplate, STALE = false). En los kits de A-3 y A-4 la compuerta se entrega ya en MEASURED sobre ese registro existente (su
    SHA-256 fijado); ninguna sonda nueva se ejecuta;
  * ACCEPTED (vía b): un registro de aceptación explícita de la sesión principal o del Coordinator (esquema rackcad-transport-acceptance/v1,
    abajo) sobre el registro de caracterización vigente, cuyo C1 midió la plantilla menos `--add-dir`.
La compuerta fija el SHA-256 de cada registro. launch.py copia los bytes en <run>/launch/ y el auditor los vuelve a evaluar con estas mismas
funciones.

Registro de aceptación (vía b), un JSON con exactamente estos valores:
  Schema = "rackcad-transport-acceptance/v1"; RunId = el del cierre; Decision = "ACCEPT_UNMEASURED_FLAG"; UnmeasuredFlags = ["--add-dir"];
  ArgsTemplate = la plantilla de la compuerta; BinarySha256 y Version = los del cierre; CharacterizationSha256 = SHA-256 del registro de
  caracterización aceptado; AcceptedBy = "principal-session" | "Coordinator"; Authority, Rationale y At (ISO-8601) no vacíos.

Uso (lo ejecuta la sesión principal, nunca el revisor):
  python -B transport_gate.py init                                 → escribe la compuerta en PENDING desde closure.json (estado del kit entregado)
  python -B transport_gate.py check                               → evalúa la compuerta del kit; código 0 si permite lanzar, 2 si no
  python -B transport_gate.py set MEASURED <caracterización.json>  → valida y escribe transport-gate.json (no escribe nada si no valida)
  python -B transport_gate.py set ACCEPTED <caracterización.json> <aceptación.json>
Después de `set`, `python -B make_manifest.py --gate` refresca SOLO la entrada de transport-gate.json del manifiesto y se vuelve a custodiar el kit.
"""
import hashlib
import json
import os
import re
import subprocess
import sys

KIT = os.path.dirname(os.path.abspath(__file__))
GATE_FILE = "transport-gate.json"
COPY_CHARACTERIZATION = "transport-characterization.json"   # copia en <run>/launch/ (bytes fijados por la compuerta)
COPY_ACCEPTANCE = "transport-acceptance.json"
PLACEHOLDERS = {"--add-dir": "<add-dir>", "--session-id": "<session-id>", "--json-schema": "<json-schema>"}
PREFLIGHT_CHECKS = ["1-KitIntegrity", "2-ClosureTransport", "3-Binary", "4-Version", "5-Auth", "6-Clone", "7-RunFiles", "8-NewSession",
                    "9-CommandLine", "10-TransportGate"]
EXPECTED_TOOLS = ["Glob", "Grep", "Read", "StructuredOutput"]
ACCEPTERS = ("principal-session", "Coordinator")
ACCEPTANCE_SCHEMA = "rackcad-transport-acceptance/v1"


def sha(b):
    return hashlib.sha256(b).hexdigest()


def norm(p):
    return str(p or "").replace("/", "\\").rstrip("\\").lower()


def templatize(args):
    """Sustituye el valor que sigue a --add-dir, --session-id y --json-schema por su marcador."""
    out, i = [], 0
    while i < len(args):
        out.append(args[i])
        if args[i] in PLACEHOLDERS and i + 1 < len(args):
            out.append(PLACEHOLDERS[args[i]])
            i += 2
            continue
        i += 1
    return out


def without_add_dir(template):
    out, i = [], 0
    while i < len(template):
        if template[i] == "--add-dir":
            i += 2
            continue
        out.append(template[i])
        i += 1
    return out


def template_of(transport):
    """Plantilla de la lista que lanza launch.py: flags medidos + AddDir + --session-id + --json-schema, con marcadores."""
    return templatize(list(transport["Flags"]) + list(transport["AddDir"]) + ["--session-id", transport["SessionId"], "--json-schema", "{}"])


def unmeasured_flags(template):
    measured = set(without_add_dir(template))
    return [x for x in template if x.startswith("--") and x not in measured]


def binary_of(closure):
    b = closure["Transport"]["Binary"]
    return {"Path": b["Path"], "Sha256": b["Sha256"], "Version": b["Version"]}


# ------------------------------------------------------------------ predicados (cada problema es una línea con probs.append)
def check_base(char, binary, template):
    """Registro de caracterización vigente: binario elegible y C1 = la plantilla sin --add-dir, STALE = false, ARCHITECT ELIGIBLE."""
    probs = []
    ex = [e for e in char.get("Executables") or [] if e.get("Sha256") == binary["Sha256"]]
    if not ex or ex[0].get("Usable") is not True or ex[0].get("Version") != binary["Version"]:
        probs.append("la caracterización no registra el binario elegible (SHA-256, versión y Usable)")
    c1 = (char.get("Probes") or {}).get("C1") or {}
    if templatize(c1.get("Args") or []) != without_add_dir(template):
        probs.append("C1 no midió la lista del lanzamiento sin --add-dir")
    if c1.get("ExitCode") != 0:
        probs.append("C1 sin código de salida 0")
    el = char.get("Eligibility") or {}
    if el.get("STALE") is not False:
        probs.append("Eligibility.STALE no es false")
    if not str(el.get("ARCHITECT", "")).startswith("ELIGIBLE"):
        probs.append("Eligibility.ARCHITECT no es ELIGIBLE")
    return probs


def check_c3(char, binary, template, model, effort):
    """Vía a: la sonda C3 midió la plantilla exacta con --add-dir y la registró en la caracterización."""
    probs = []
    c3 = (char.get("Probes") or {}).get("C3")
    if not isinstance(c3, dict):
        probs.append("la caracterización no tiene la sonda C3")
        return probs
    if c3.get("Verdict") != "PASS" or c3.get("Problems"):
        probs.append("C3 sin veredicto PASS limpio")
    if c3.get("ArgsTemplate") != template:
        probs.append("C3 no midió la lista exacta del lanzamiento (con --add-dir)")
    if templatize(c3.get("Args") or []) != template:
        probs.append("los argumentos registrados de C3 no corresponden a la plantilla")
    b = c3.get("Binary") or {}
    if b.get("Sha256") != binary["Sha256"] or b.get("Version") != binary["Version"]:
        probs.append("C3 con otro binario o versión")
    if c3.get("ExitCode") != 0 or c3.get("Killed") is not False or c3.get("TimedOut") is not False:
        probs.append("C3 sin salida 0 o terminada por el invocador")
    r = c3.get("Result") or {}
    if r.get("subtype") != "success" or r.get("is_error") is not False or r.get("structured_output_present") is not True:
        probs.append("C3 sin result success con structured_output")
    i = c3.get("Init") or {}
    if sorted(i.get("tools") or []) != EXPECTED_TOOLS or i.get("mcp_servers") != [] or i.get("permissionMode") != "dontAsk" or \
            i.get("claude_code_version") != binary["Version"].split()[0] or i.get("model") != model:
        probs.append("init de C3 distinto del esperado (herramientas, MCP, permissionMode, versión o modelo)")
    if c3.get("ObservedModel") != [model] or c3.get("ObservedEffortPerMessage") != [effort]:
        probs.append("C3 con modelo o effort observado distinto")
    a = c3.get("AddDirRead") or {}
    if a.get("Attempted") is not True or a.get("Delivered") is not True or a.get("Faithful") is not True or a.get("Denied") is not False:
        probs.append("C3: la lectura dentro del directorio añadido no se entregó fiel o se denegó")
    o = c3.get("OutsideRead") or {}
    if o.get("Attempted") is not True or o.get("Delivered") is not False or o.get("Denied") is not True:
        probs.append("C3: la lectura fuera del cwd y del directorio añadido no quedó denegada")
    den = r.get("permission_denials")
    if not isinstance(den, list) or any(norm((d or {}).get("Path")) != norm(o.get("Path")) for d in den):
        probs.append("C3: permission_denials con rutas distintas de la lectura exterior")
    if (char.get("Eligibility") or {}).get("MeasuredArgsTemplate") != template:
        probs.append("Eligibility.MeasuredArgsTemplate distinta de la plantilla del lanzamiento")
    return probs


def check_acceptance(acc, binary, template, run_id, char_sha):
    """Vía b: aceptación explícita registrada del flag no medido."""
    probs = []
    if acc.get("Schema") != ACCEPTANCE_SCHEMA:
        probs.append("aceptación con otro esquema")
    if acc.get("RunId") != run_id:
        probs.append("aceptación de otro RunId")
    if acc.get("Decision") != "ACCEPT_UNMEASURED_FLAG":
        probs.append("aceptación sin Decision = ACCEPT_UNMEASURED_FLAG")
    if acc.get("UnmeasuredFlags") != unmeasured_flags(template):
        probs.append("aceptación con UnmeasuredFlags distintos de %s" % unmeasured_flags(template))
    if acc.get("ArgsTemplate") != template:
        probs.append("aceptación de otra plantilla de argumentos")
    if acc.get("BinarySha256") != binary["Sha256"] or acc.get("Version") != binary["Version"]:
        probs.append("aceptación de otro binario o versión")
    if acc.get("CharacterizationSha256") != char_sha:
        probs.append("aceptación sobre otro registro de caracterización")
    if acc.get("AcceptedBy") not in ACCEPTERS:
        probs.append("AcceptedBy distinto de %s" % " | ".join(ACCEPTERS))
    if any(not str(acc.get(k) or "").strip() for k in ("Authority", "Rationale", "At")):
        probs.append("aceptación sin Authority, Rationale o At")
    return probs


def evaluate(gate, closure, char_bytes, acc_bytes):
    """→ {Ok, Status, Problems, ArgsTemplate, CharacterizationSha256, AcceptanceSha256}. Los bytes son los de los registros (o None)."""
    probs = []
    gate = gate if isinstance(gate, dict) else {}
    t = closure["Transport"]
    template, binary = template_of(t), binary_of(closure)
    status = gate.get("Status")
    out = {"Status": status, "ArgsTemplate": template, "CharacterizationSha256": sha(char_bytes) if char_bytes is not None else None,
           "AcceptanceSha256": sha(acc_bytes) if acc_bytes is not None else None}
    if status not in ("MEASURED", "ACCEPTED"):
        probs.append("Status %r: D-01 abierto (falta la medición C3 o la aceptación registrada)" % status)
    if gate.get("ArgsTemplate") != template:
        probs.append("ArgsTemplate de la compuerta distinta de la plantilla del cierre")
    if gate.get("RunId") != closure["Ids"]["RunId"] or gate.get("SessionId") != t["SessionId"] or gate.get("Binary") != binary:
        probs.append("RunId, SessionId o Binary de la compuerta distintos del cierre")
    char = acc = None
    if status in ("MEASURED", "ACCEPTED"):
        pinned = (gate.get("Characterization") or {}).get("Sha256")
        if char_bytes is None:
            probs.append("falta el registro de caracterización")
        elif not pinned or sha(char_bytes) != pinned:
            probs.append("SHA-256 del registro de caracterización distinto del fijado en la compuerta")
        if char_bytes is not None:
            try:
                char = json.loads(char_bytes.decode("utf-8"))
            except ValueError:
                probs.append("registro de caracterización ilegible")
        if isinstance(char, dict):
            probs += check_base(char, binary, template)
    if status == "MEASURED" and isinstance(char, dict):
        probs += check_c3(char, binary, template, t["Model"], t["Effort"])
    if status == "ACCEPTED":
        pinned = (gate.get("Acceptance") or {}).get("Sha256")
        if acc_bytes is None:
            probs.append("falta el registro de aceptación")
        elif not pinned or sha(acc_bytes) != pinned:
            probs.append("SHA-256 del registro de aceptación distinto del fijado en la compuerta")
        if acc_bytes is not None:
            try:
                acc = json.loads(acc_bytes.decode("utf-8"))
            except ValueError:
                probs.append("registro de aceptación ilegible")
        if isinstance(acc, dict):
            probs += check_acceptance(acc, binary, template, closure["Ids"]["RunId"], out["CharacterizationSha256"])
    out["Problems"], out["Ok"] = probs, not probs
    return out


# ------------------------------------------------------------------ medición del binario (launch.py y probe-c3.py)
ENV_PREFIXES = ("TEMP", "LOCALAPPDATA", "APPDATA", "USERPROFILE")


def portable(path):
    """Ruta absoluta escrita con %TEMP%, %LOCALAPPDATA%, %APPDATA% o %USERPROFILE% (sin nombre de usuario)."""
    p = os.path.realpath(path)
    for var in ENV_PREFIXES:
        v = os.environ.get(var)
        if v:
            base = os.path.realpath(v)
            if p.lower() == base.lower() or p.lower().startswith(base.lower().rstrip("\\") + "\\"):
                return "%" + var + "%" + p[len(base.rstrip("\\")):]
    return p


def expand(path):
    return os.path.expandvars(path)


def measure_binary(path):
    """Ruta resuelta (portable), SHA-256 y firma Authenticode del binario."""
    exists = os.path.isfile(path)
    real = os.path.realpath(path) if exists else None
    sig = ""
    if exists:
        sig = subprocess.run(["pwsh", "-NoProfile", "-Command",
                              "$s = Get-AuthenticodeSignature -LiteralPath '%s'; '' + $s.Status + '|' + $s.SignerCertificate.Subject" % real],
                             capture_output=True, text=True, encoding="utf-8", errors="replace").stdout.strip()
    return {"Exists": exists, "Path": portable(real) if real else None, "Sha256": sha(open(real, "rb").read()) if real else None,
            "Authenticode": sig.split("|")[0] if sig else None, "SignerIsAnthropic": "Anthropic, PBC" in sig}


def cli_version(path, cwd):
    return subprocess.run([path, "--version"], capture_output=True, text=True, cwd=cwd).stdout.strip()


def auth_status(path, cwd):
    """`auth status --json`: solo se conservan loggedIn, authMethod y apiProvider (ni correo ni organización; no lee credenciales)."""
    r = subprocess.run([path, "auth", "status", "--json"], capture_output=True, text=True, cwd=cwd)
    try:
        a = json.loads(r.stdout)
        return {k: a.get(k) for k in ("loggedIn", "authMethod", "apiProvider")}
    except ValueError:
        return {"loggedIn": None, "ParseError": True}


# ------------------------------------------------------------------ CLI
def load_kit():
    closure = json.load(open(os.path.join(KIT, "closure.json"), encoding="utf-8"))
    gate = json.load(open(os.path.join(KIT, GATE_FILE), encoding="utf-8"))
    return closure, gate


def read_pinned(entry):
    if not entry or not entry.get("Path"):
        return None
    try:
        return open(expand(entry["Path"]), "rb").read()
    except OSError:
        return None


def initial_gate(closure):
    t = closure["Transport"]
    template = template_of(t)
    return {
        "Schema": "rackcad-transport-gate/v1",
        "Purpose": "desviación D-01: C1 midió la lista sin --add-dir; launch.py se niega a lanzar mientras Status = PENDING",
        "RunId": closure["Ids"]["RunId"], "SessionId": t["SessionId"], "Binary": binary_of(closure),
        "ArgsTemplate": template, "UnmeasuredInC1": unmeasured_flags(template),
        "Status": "PENDING", "Characterization": None, "Acceptance": None,
        "Routes": {"MEASURED": "vía a: el registro de caracterización R20261008T183600Z-char ya tiene Probes.C3 en PASS sobre la plantilla exacta "
                               "(sonda C3 de decisiones §56, punto 7); set MEASURED <ese registro>; ninguna sonda nueva",
                   "ACCEPTED": "vía b: registro rackcad-transport-acceptance/v1 de la sesión principal o del Coordinator; después "
                               "set ACCEPTED <caracterización> <aceptación>"},
    }


def main(argv):
    closure = json.load(open(os.path.join(KIT, "closure.json"), encoding="utf-8"))
    if argv[:1] == ["init"]:
        with open(os.path.join(KIT, GATE_FILE), "w", encoding="utf-8", newline="\n") as f:
            f.write(json.dumps(initial_gate(closure), ensure_ascii=False, indent=1) + "\n")
        print(json.dumps({"Written": GATE_FILE, "Status": "PENDING"}))
        return 0
    closure, gate = load_kit()
    if argv[:1] == ["check"]:
        ev = evaluate(gate, closure, read_pinned(gate.get("Characterization")), read_pinned(gate.get("Acceptance")))
        print(json.dumps(ev, ensure_ascii=False, indent=1))
        return 0 if ev["Ok"] else 2
    if argv[:1] == ["set"] and len(argv) >= 3 and argv[1] in ("MEASURED", "ACCEPTED") and (len(argv) == 4) == (argv[1] == "ACCEPTED"):
        new = dict(gate)
        cb = open(argv[2], "rb").read()
        new.update({"Status": argv[1], "Characterization": {"Path": portable(argv[2]), "Sha256": sha(cb)}, "Acceptance": None})
        ab = None
        if argv[1] == "ACCEPTED":
            ab = open(argv[3], "rb").read()
            new["Acceptance"] = {"Path": portable(argv[3]), "Sha256": sha(ab)}
        ev = evaluate(new, closure, cb, ab)
        if not ev["Ok"]:
            print(json.dumps({"Refused": "la evidencia no satisface la compuerta: no se escribe nada", "Evaluation": ev}, ensure_ascii=False, indent=1))
            return 2
        with open(os.path.join(KIT, GATE_FILE), "w", encoding="utf-8", newline="\n") as f:
            f.write(json.dumps(new, ensure_ascii=False, indent=1) + "\n")
        print(json.dumps({"Written": GATE_FILE, "Status": new["Status"], "Sha256": sha(open(os.path.join(KIT, GATE_FILE), "rb").read()),
                          "Next": "python -B make_manifest.py --gate; después, custodiar el kit otra vez"}, ensure_ascii=False, indent=1))
        return 0
    print(__doc__)
    return 2


if __name__ == "__main__":
    sys.stdout.reconfigure(errors="backslashreplace")
    sys.exit(main(sys.argv[1:]))
