"""I-62 / FX-02 — medición única de claude-cli (operaciones 5, 7, 8 y 9 del descriptor; decisiones §66, punto 6).
Funciones comunes del paquete ccli-meas: constantes, hashes, saneado, barrido de fugas, puertas (funciones puras), huella de configuración
(SHA-256 y NOMBRES de claves, nunca valores) y clasificación de procesos. No ejecuta claude en ningún caso.
Se carga por ruta (importlib) desde los demás scripts, para que funcionen con `python -I -B`.
"""
import ctypes
import datetime
import hashlib
import json
import os
import re
import subprocess

PKG = os.path.dirname(os.path.abspath(__file__))
CONSTANTS_FILE = "run-constants.json"


def load_constants(pkg=PKG):
    return json.load(open(os.path.join(pkg, CONSTANTS_FILE), encoding="utf-8"))


def now():
    return datetime.datetime.now(datetime.timezone.utc).strftime("%Y-%m-%dT%H:%M:%S.%fZ")


def sha_bytes(b):
    return hashlib.sha256(b).hexdigest()


def sha_file(p):
    h = hashlib.sha256()
    with open(p, "rb") as f:
        for chunk in iter(lambda: f.read(1 << 20), b""):
            h.update(chunk)
    return h.hexdigest()


def expand(p):
    return os.path.expandvars(str(p))


def norm(p):
    return os.path.normcase(os.path.normpath(expand(p)))


def write_json(path, obj):
    with open(path, "w", encoding="utf-8", newline="\n") as f:
        f.write(json.dumps(obj, ensure_ascii=False, indent=1) + "\n")


def git(repo, *a, check=False):
    r = subprocess.run(["git", "-C", repo] + list(a), capture_output=True)
    if check and r.returncode != 0:
        raise RuntimeError("git %s: %s" % (" ".join(a[:2]), r.stderr.decode("utf-8", "replace")[:300]))
    return r.stdout.decode("utf-8", "replace")


# ------------------------------------------------------------------ saneado (antes de cualquier custodia)
def _short_path(p):
    try:
        buf = ctypes.create_unicode_buffer(1024)
        n = ctypes.windll.kernel32.GetShortPathNameW(str(p), buf, 1024)
        return buf.value if 0 < n < 1024 else None
    except Exception:
        return None


def _long_path(p):
    try:
        buf = ctypes.create_unicode_buffer(1024)
        n = ctypes.windll.kernel32.GetLongPathNameW(str(p), buf, 1024)
        return buf.value if 0 < n < 1024 else None
    except Exception:
        return None


def _path_regex(value):
    parts = [re.escape(x) for x in re.split(r"[\\/]+", value.strip("\\/")) if x]
    return r"[\\/]+".join(parts)


EMAIL_RX = re.compile(r"[A-Za-z0-9._%+-]+@[A-Za-z0-9-]+(?:\.[A-Za-z0-9-]+)*\.[A-Za-z]{2,}")


def _ident_rx(word):
    return re.compile(r"(?<![A-Za-z0-9])" + re.escape(word) + r"(?![A-Za-z0-9])", re.I)


class Sanitizer:
    """Sustituye rutas del perfil (formas larga y 8.3, con `\\`, `\\\\` o `/`), usuario, equipo y correos. Se construye desde el entorno en
    tiempo de ejecución: el paquete no contiene ningún nombre real."""

    def __init__(self, env=None):
        env = dict(os.environ if env is None else env)
        pairs = []
        for var, label in (("TEMP", "%TEMP%"), ("TMP", "%TEMP%"), ("LOCALAPPDATA", "%LOCALAPPDATA%"), ("APPDATA", "%APPDATA%"),
                           ("USERPROFILE", "%USERPROFILE%")):
            v = env.get(var)
            if not v:
                continue
            forms = {v, _short_path(v) or v, _long_path(v) or v}
            for f in forms:
                if f and len(f) > 3:
                    pairs.append((f, label))
        pairs.sort(key=lambda x: -len(x[0]))
        self.path_rx = [(re.compile(_path_regex(f), re.I), label) for f, label in pairs]
        self.generic_home = re.compile(r"[A-Za-z]:[\\/]+Users[\\/]+[^\\/\s\"'<>|]+", re.I)
        self.user = env.get("USERNAME") or ""
        self.host = env.get("COMPUTERNAME") or ""
        short_home = _short_path(env.get("USERPROFILE", "")) or ""
        last = re.split(r"[\\/]+", short_home.rstrip("\\/"))[-1] if short_home else ""
        self.short_user = last if "~" in last else ""
        self.word_rx = []
        for w, label in ((self.short_user, "<USER>"), (self.user, "<USER>"), (self.host, "<HOST>")):
            if w and len(w) >= 3:
                self.word_rx.append((_ident_rx(w), label))

    def text(self, s):
        if not isinstance(s, str):
            return s
        for rx, label in self.path_rx:
            s = rx.sub(label, s)
        s = self.generic_home.sub("%USERPROFILE%", s)
        for rx, label in self.word_rx:
            s = rx.sub(label, s)
        return EMAIL_RX.sub("<EMAIL>", s)

    def obj(self, o):
        if isinstance(o, str):
            return self.text(o)
        if isinstance(o, list):
            return [self.obj(x) for x in o]
        if isinstance(o, dict):
            return {self.text(k): self.obj(v) for k, v in o.items()}
        return o

    def leak_classes(self, s):
        """→ clases de fuga presentes en el texto (sin devolver la coincidencia)."""
        out = []
        for name, w in (("USER", self.user), ("SHORT_USER", self.short_user), ("HOST", self.host)):
            if w and len(w) >= 3 and _ident_rx(w).search(s):
                out.append(name)
        for rx, label in self.path_rx:
            if rx.search(s):
                out.append("PATH:" + label)
        if self.generic_home.search(s):
            out.append("PATH:generic-home")
        if EMAIL_RX.search(s):
            out.append("EMAIL")
        return sorted(set(out))

    def leak_scan(self, root, skip_dirs=()):
        """Barrido de fugas sobre todos los archivos de texto bajo `root` → [{File, Classes}]."""
        found = []
        for r, ds, fs in os.walk(root):
            ds[:] = [d for d in ds if os.path.join(r, d) not in skip_dirs and d not in skip_dirs]
            for f in fs:
                p = os.path.join(r, f)
                try:
                    s = open(p, "rb").read().decode("utf-8", "replace")
                except OSError:
                    continue
                cls = self.leak_classes(s)
                if cls:
                    found.append({"File": os.path.relpath(p, root).replace("\\", "/"), "Classes": cls})
        return found


# ------------------------------------------------------------------ manifiesto del paquete
MANIFEST = "MANIFEST.json"


def check_manifest(pkg, allowed_unlisted):
    """→ dict con Ok, Mismatches, Missing, Unlisted, ManifestSha256. Refusa __pycache__ y cualquier archivo no listado fuera de la lista
    permitida; el directorio run/ se evalúa en la puerta de un solo uso, no aquí."""
    mp = os.path.join(pkg, MANIFEST)
    if not os.path.isfile(mp):
        return {"Ok": False, "Problem": "falta MANIFEST.json", "ManifestSha256": None}
    man = json.load(open(mp, encoding="utf-8"))
    bad, missing = [], []
    for name, rec in man["Files"].items():
        p = os.path.join(pkg, name)
        if not os.path.isfile(p):
            missing.append(name)
        elif sha_file(p) != rec["Sha256"]:
            bad.append(name)
    listed = set(man["Files"]) | {MANIFEST}
    unlisted = []
    for n in sorted(os.listdir(pkg)):
        p = os.path.join(pkg, n)
        if n == "run" and os.path.isdir(p):
            continue
        if os.path.isdir(p) or (n not in listed and n not in allowed_unlisted):
            unlisted.append(n)
    return {"Ok": not bad and not missing and not unlisted, "Mismatches": bad, "Missing": missing, "Unlisted": unlisted,
            "ManifestSha256": sha_file(mp)}


# ------------------------------------------------------------------ puertas (funciones puras; launch.py y dryrun.py las usan)
def gate_decisions(text, owner_line, marker_template, placeholder, manifest_sha):
    """Las dos líneas deben aparecer como LÍNEAS COMPLETAS exactas (solo se retira el fin de línea \\r\\n o \\n)."""
    lines = [l[:-1] if l.endswith("\r") else l for l in text.split("\n")]
    marker = marker_template.replace(placeholder, manifest_sha or "<sin-manifiesto>")
    has_owner = owner_line in lines
    has_marker = marker in lines
    return {"Ok": has_owner and has_marker, "OwnerLinePresent": has_owner, "CoordinatorMarkerPresent": has_marker,
            "OwnerLineSha256": sha_bytes(owner_line.encode("utf-8")), "ExpectedMarker": marker}


def gate_one_shot(pkg):
    run = os.path.join(pkg, "run")
    return {"Ok": not os.path.exists(run), "RunDirExists": os.path.exists(run),
            "Rule": "run/ (y su ONE-SHOT.marker) existe desde el único lanzamiento y no se borra nunca"}


def gate_env(env):
    names = sorted(k for k in env if k.upper().startswith(("CLAUDE", "ANTHROPIC")))
    bad = [k for k in env if k.upper() == "CLAUDE_CONFIG_DIR"]
    return {"Ok": not bad, "ClaudeConfigDirSet": bool(bad), "ClaudeAnthropicNames": names,
            "Rule": "con CLAUDE_CONFIG_DIR definida la configuración de usuario no está en %USERPROFILE%\\.claude y no se puede ubicar sin leer un valor"}


def gate_new_session(projects_dir, session_id, project_prefixes):
    import glob
    prior = glob.glob(os.path.join(projects_dir, "*", session_id + ".jsonl"))
    dirs = []
    for pref in project_prefixes:
        dirs += [os.path.basename(p) for p in glob.glob(os.path.join(projects_dir, pref + "*"))]
    return {"Ok": not prior and not dirs, "PriorTranscripts": len(prior), "ProjectDirs": sorted(set(dirs))}


def project_dir_name(path):
    """Nombre de la carpeta de proyecto de claude-cli para un cwd (todo carácter no alfanumérico → '-')."""
    return re.sub(r"[^A-Za-z0-9]", "-", os.path.normpath(path))


def authenticode(path):
    ps = ("$s = Get-AuthenticodeSignature -LiteralPath $env:CCLI_SIG_PATH; "
          "[pscustomobject]@{Status=[string]$s.Status; Subject=[string]$s.SignerCertificate.Subject} | ConvertTo-Json -Compress")
    env = dict(os.environ, CCLI_SIG_PATH=path)
    exe = "pwsh" if _which("pwsh") else "powershell"
    r = subprocess.run([exe, "-NoProfile", "-NonInteractive", "-Command", ps], capture_output=True, text=True, env=env)
    try:
        o = json.loads(r.stdout.strip())
    except ValueError:
        return {"Status": "UNKNOWN", "SignerIsAnthropic": False}
    return {"Status": o.get("Status"), "SignerIsAnthropic": "CN=\"Anthropic, PBC\"" in (o.get("Subject") or "")}


def _which(name):
    for d in os.environ.get("PATH", "").split(os.pathsep):
        for ext in ("", ".exe", ".cmd"):
            if os.path.isfile(os.path.join(d, name + ext)):
                return os.path.join(d, name + ext)
    return None


def gate_binary(path, expected_sha, check_signature=True):
    exists = os.path.isfile(path)
    sha = sha_file(path) if exists else None
    sig = authenticode(path) if (exists and check_signature) else {"Status": None, "SignerIsAnthropic": None}
    ok = exists and sha == expected_sha and (not check_signature or (sig["Status"] == "Valid" and sig["SignerIsAnthropic"]))
    return {"Ok": ok, "Exists": exists, "Sha256": sha, "ExpectedSha256": expected_sha, "Authenticode": sig["Status"],
            "SignerIsAnthropic": sig["SignerIsAnthropic"]}


def reparse_points(root):
    import stat
    out = 0
    for r, ds, fs in os.walk(root, followlinks=False):
        for n in ds + fs:
            st = os.lstat(os.path.join(r, n))
            if stat.S_ISLNK(st.st_mode) or (getattr(st, "st_file_attributes", 0) & 0x400):
                out += 1
    return out


def gate_clone(clone, commit, object_path, object_blob, required_prefix=None):
    """HEAD = commit; solo main; sin remoto; limpio con ignorados; blob del objeto; sin .claude/; sin enlaces; solo historia de HEAD."""
    if not os.path.isdir(os.path.join(clone, ".git")):
        return {"Ok": False, "Exists": os.path.isdir(clone), "Problem": "no es un clon git"}
    head = git(clone, "rev-parse", "HEAD").strip()
    branches = git(clone, "branch", "--list", "--format=%(refname:short)").split()
    remotes = git(clone, "remote").split()
    status = git(clone, "status", "--porcelain", "--ignored", "--untracked-files=all")
    blob = git(clone, "rev-parse", commit + ":" + object_path).strip()
    all_commits = set(git(clone, "rev-list", "--all").split())
    head_commits = set(git(clone, "rev-list", "HEAD").split())
    autocrlf = git(clone, "config", "--local", "--get", "core.autocrlf").strip()
    dot_claude = os.path.exists(os.path.join(clone, ".claude"))
    rp = reparse_points(clone)
    prefix_ok = True if required_prefix is None else os.path.basename(os.path.normpath(clone)).lower().startswith(required_prefix.lower())
    ok = (head == commit and branches == ["main"] and remotes == [] and status == "" and blob == object_blob and not dot_claude and rp == 0
          and all_commits == head_commits and autocrlf == "false" and prefix_ok)
    return {"Ok": ok, "Head": head, "Branches": branches, "Remotes": remotes, "Clean": status == "", "StatusLines": len(status.splitlines()),
            "ObjectBlob": blob, "DotClaude": dot_claude, "ReparsePoints": rp, "OnlyHeadHistory": all_commits == head_commits,
            "AutoCrlf": autocrlf, "PrefixOk": prefix_ok}


# ------------------------------------------------------------------ huella de configuración (op 9): SHA-256 + NOMBRES de claves, nunca valores
def key_paths(o, prefix=""):
    """Rutas de claves de un JSON (objetos y objetos dentro de listas, marcados con []); nunca valores ni elementos escalares."""
    out = set()
    if isinstance(o, dict):
        for k, v in o.items():
            p = (prefix + "." if prefix else "") + str(k)
            out.add(p)
            out |= key_paths(v, p)
    elif isinstance(o, list):
        for x in o:
            if isinstance(x, (dict, list)):
                out |= key_paths(x, prefix + "[]")
    return out


def config_candidates(clone):
    up = os.environ.get("USERPROFILE", "")
    pf = os.environ.get("ProgramFiles", r"C:\Program Files")
    pd = os.environ.get("ProgramData", r"C:\ProgramData")
    return [
        {"Id": "U1", "Display": r"%USERPROFILE%\.claude\settings.json", "Path": os.path.join(up, ".claude", "settings.json"), "Scope": "user", "Mode": "keys"},
        {"Id": "U2", "Display": r"%USERPROFILE%\.claude\settings.local.json", "Path": os.path.join(up, ".claude", "settings.local.json"), "Scope": "user", "Mode": "keys"},
        {"Id": "P1", "Display": r"<clon>\.claude\settings.json", "Path": os.path.join(clone, ".claude", "settings.json"), "Scope": "project", "Mode": "keys"},
        {"Id": "P2", "Display": r"<clon>\.claude\settings.local.json", "Path": os.path.join(clone, ".claude", "settings.local.json"), "Scope": "project", "Mode": "keys"},
        {"Id": "M1", "Display": r"%ProgramFiles%\ClaudeCode\managed-settings.json", "Path": os.path.join(pf, "ClaudeCode", "managed-settings.json"), "Scope": "managed", "Mode": "keys"},
        {"Id": "M2", "Display": r"%ProgramData%\ClaudeCode\managed-settings.json", "Path": os.path.join(pd, "ClaudeCode", "managed-settings.json"), "Scope": "managed", "Mode": "keys"},
        {"Id": "M3", "Display": r"%ProgramFiles%\ClaudeCode\managed-mcp.json", "Path": os.path.join(pf, "ClaudeCode", "managed-mcp.json"), "Scope": "managed", "Mode": "keys"},
        {"Id": "M4", "Display": r"%ProgramData%\ClaudeCode\managed-mcp.json", "Path": os.path.join(pd, "ClaudeCode", "managed-mcp.json"), "Scope": "managed", "Mode": "keys"},
        {"Id": "M5", "Display": r"%ProgramFiles%\ClaudeCode\managed-settings.d\*.json", "Path": os.path.join(pf, "ClaudeCode", "managed-settings.d"), "Scope": "managed", "Mode": "dir"},
        {"Id": "R1", "Display": r"HKLM\SOFTWARE\Policies\ClaudeCode", "Path": ("HKLM", r"SOFTWARE\Policies\ClaudeCode"), "Scope": "managed", "Mode": "registry"},
        {"Id": "R2", "Display": r"HKCU\SOFTWARE\Policies\ClaudeCode", "Path": ("HKCU", r"SOFTWARE\Policies\ClaudeCode"), "Scope": "managed", "Mode": "registry"},
        {"Id": "S1", "Display": r"%USERPROFILE%\.claude.json", "Path": os.path.join(up, ".claude.json"), "Scope": "state", "Mode": "metadata"},
    ]


def _registry(hive, sub):
    """Clave de política del registro: existencia, nombres de valores y subclaves, y un SHA-256 de (nombre, tipo, dato) para detectar cambios.
    Los datos nunca salen de esta función ni se custodian."""
    import winreg
    root = winreg.HKEY_LOCAL_MACHINE if hive == "HKLM" else winreg.HKEY_CURRENT_USER
    try:
        k = winreg.OpenKey(root, sub, 0, winreg.KEY_READ)
    except OSError:
        return {"Exists": False}
    with k:
        nsub, nval, _ = winreg.QueryInfoKey(k)
        names, h = [], hashlib.sha256()
        for i in range(nval):
            n, d, t = winreg.EnumValue(k, i)
            names.append(n)
            h.update(repr((n, t, d)).encode("utf-8", "replace"))
            del d
        subs = [winreg.EnumKey(k, i) for i in range(nsub)]
    return {"Exists": True, "Sha256": h.hexdigest(), "KeyNames": sorted(names) + sorted(s + "\\" for s in subs)}


ENV_EXTRA = ["HTTP_PROXY", "HTTPS_PROXY", "NO_PROXY", "NODE_OPTIONS", "NODE_EXTRA_CA_CERTS", "SSL_CERT_FILE", "DISABLE_AUTOUPDATER",
             "DISABLE_TELEMETRY", "DISABLE_ERROR_REPORTING", "MCP_TIMEOUT"]


def fingerprint(clone, keys_for=None, env=None):
    """Huella observable sin leer valores: por candidato, existencia, tamaño, mtime, SHA-256 y nombres de claves (Mode keys) o solo metadatos
    (Mode metadata: %USERPROFILE%\\.claude.json, archivo de ESTADO, nunca se lee su contenido). `keys_for` limita los candidatos cuyas claves
    se listan (el ensayo en seco solo lista las de U1); el resto queda en SHA-256."""
    san = Sanitizer()
    env = os.environ if env is None else env
    out = []
    for c in config_candidates(clone):
        if c["Mode"] == "registry":
            rec = dict({"Id": c["Id"], "Path": c["Display"], "Scope": c["Scope"]}, **_registry(*c["Path"]))
            if rec["Exists"]:
                rec["KeyNames"] = ([san.text(x) for x in rec["KeyNames"]] if (keys_for is None or c["Id"] in keys_for)
                                   else "NO_LISTADAS_EN_ESTE_MODO")
            out.append(rec)
            continue
        if c["Mode"] == "dir":
            rec = {"Id": c["Id"], "Path": c["Display"], "Scope": c["Scope"], "Exists": os.path.isdir(c["Path"])}
            if rec["Exists"]:
                files = sorted(f for f in os.listdir(c["Path"]) if f.lower().endswith(".json") and os.path.isfile(os.path.join(c["Path"], f)))
                per, keys = [], []
                for f in files:
                    fp_ = os.path.join(c["Path"], f)
                    per.append({"File": f, "Sha256": sha_file(fp_)})
                    try:
                        keys += [f + ":" + san.text(k) for k in key_paths(json.loads(open(fp_, "rb").read().decode("utf-8-sig")))]
                    except ValueError:
                        keys.append(f + ":<JSON no analizable>")
                rec["Files"] = per
                rec["Sha256"] = sha_bytes(json.dumps(per, sort_keys=True).encode("utf-8"))
                rec["KeyNames"] = sorted(keys) if (keys_for is None or c["Id"] in keys_for) else "NO_LISTADAS_EN_ESTE_MODO"
            out.append(rec)
            continue
        rec = {"Id": c["Id"], "Path": c["Display"], "Scope": c["Scope"], "Exists": os.path.isfile(c["Path"])}
        if rec["Exists"]:
            st = os.stat(c["Path"])
            rec["Size"] = st.st_size
            rec["MtimeUtc"] = datetime.datetime.fromtimestamp(st.st_mtime, datetime.timezone.utc).isoformat()
            if c["Mode"] == "keys":
                rec["Sha256"] = sha_file(c["Path"])
                if keys_for is None or c["Id"] in keys_for:
                    try:
                        data = json.loads(open(c["Path"], "rb").read().decode("utf-8-sig"))
                        rec["KeyNames"] = sorted(san.text(k) for k in key_paths(data))
                        del data
                    except ValueError:
                        rec["KeyNames"], rec["ParseError"] = None, "JSON no analizable (sin contenido)"
                else:
                    rec["KeyNames"] = "NO_LISTADAS_EN_ESTE_MODO"
            else:
                rec["Sha256"] = "NO_CALCULADO (archivo de estado; valores nunca leídos)"
        out.append(rec)
    names = sorted(k for k in env if k.upper().startswith(("CLAUDE", "ANTHROPIC")))
    extra = sorted(k for k in env if k.upper() in ENV_EXTRA)
    return {"At": now(), "Candidates": out, "EnvNames": {"ClaudeAnthropic": names, "Extra": extra,
            "Rule": "solo nombres; ningún valor se lee ni se custodia"}}


def fingerprint_diff(before, after):
    b = {c["Id"]: c for c in before["Candidates"]}
    a = {c["Id"]: c for c in after["Candidates"]}
    changes = []
    for k in sorted(set(a) | set(b)):
        x, y = b.get(k, {}), a.get(k, {})
        if x.get("Scope") == "state" or y.get("Scope") == "state":
            continue
        if x.get("Exists") != y.get("Exists") or x.get("Sha256") != y.get("Sha256"):
            kx = set(x.get("KeyNames") or []) if isinstance(x.get("KeyNames"), list) else set()
            ky = set(y.get("KeyNames") or []) if isinstance(y.get("KeyNames"), list) else set()
            changes.append({"Id": k, "ExistsBefore": x.get("Exists"), "ExistsAfter": y.get("Exists"),
                            "KeysAdded": sorted(ky - kx), "KeysRemoved": sorted(kx - ky)})
    env_changed = before["EnvNames"] != after["EnvNames"]
    return {"Stable": not changes and not env_changed, "Changes": changes, "EnvNamesChanged": env_changed}


# ------------------------------------------------------------------ clasificación de procesos (op 8): tabla cerrada, primera regla que aplica
CLASS_RULES = [
    ("launched-root", "identidad (PID + CreationDate) del proceso lanzado"),
    ("cli-self-child", "ExecutablePath = binario fijado, distinto de la raíz"),
    ("cli-bundled-helper", "ExecutablePath bajo el directorio de versiones del binario fijado (%APPDATA%\\Claude\\claude-code\\)"),
    ("search-helper", "Name = rg.exe"),
    ("vcs-helper", "Name = git.exe o git-*.exe"),
    ("shell", "Name en cmd.exe, powershell.exe, pwsh.exe, bash.exe, sh.exe, wsl.exe"),
    ("console-host", "Name en conhost.exe, openconsole.exe"),
    ("runtime", "Name en node.exe, bun.exe"),
    ("other", "Name legible que no cae en las reglas anteriores"),
    ("unclassified", "ni Name ni ExecutablePath legibles"),
]


def classify_process(name, path, is_root, pinned_path, pinned_root_dir):
    n = (name or "").lower()
    p = os.path.normcase(os.path.normpath(path)) if path else ""
    if is_root:
        return "launched-root"
    if p and pinned_path and p == os.path.normcase(os.path.normpath(pinned_path)):
        return "cli-self-child"
    if p and pinned_root_dir and p.startswith(os.path.normcase(os.path.normpath(pinned_root_dir)) + os.sep):
        return "cli-bundled-helper"
    if n == "rg.exe":
        return "search-helper"
    if n == "git.exe" or re.match(r"^git-.*\.exe$", n):
        return "vcs-helper"
    if n in ("cmd.exe", "powershell.exe", "pwsh.exe", "bash.exe", "sh.exe", "wsl.exe"):
        return "shell"
    if n in ("conhost.exe", "openconsole.exe"):
        return "console-host"
    if n in ("node.exe", "bun.exe"):
        return "runtime"
    if n or p:
        return "other"
    return "unclassified"
