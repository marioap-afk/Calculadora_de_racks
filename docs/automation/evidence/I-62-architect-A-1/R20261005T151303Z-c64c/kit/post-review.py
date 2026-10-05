"""I-62 A-1 (corrected for A62-A1T-01 and O1..O3, kit v4): post-review audit v4, declared and self-tested BEFORE the run (Coordinator order,
decisions §42, §7). It audits the transcript of the formal Architect session; the invoking session runs it afterwards.

v4 = v3.1 (kit v3c, R20261005T073911Z-2dfe) with the corrections that the order fixes BEFORE launching, never after the run:
  * effective paths: every filesystem path is resolved against the cwd, `.` and `..` are collapsed, and an existing path is resolved with realpath,
    so a symbolic link or a junction that leaves the clone, the worktree or the own outputs is OUTSIDE (no comparison of text prefixes only);
  * navigation (NAV-1): `cd` / `Set-Location` to the root OR ANY SUBDIRECTORY of the clone or the own worktree, or to an own-output directory;
    navigating never authorizes a read: every later path is still classified by effect;
  * `for VAR in WORDS; do …; done`: the loop header is parsed before the control words are stripped (defect of v3/v3.1, line 237/241), and the
    WORDS are checked as paths when they look like paths;
  * own scripts (OWN-2): Write, Edit, redirection, `sed -i` and `mkdir -p` are allowed ONLY on own outputs; an own script keeps an auditable body
    (Write content, Edit replacements and the `sed -i` replacement text are audited as python code when it is run); any write on a closure file,
    a custodied script, a repository source or another session's file is a violation;
  * filesystem paths and Git object paths are distinct: `git show <rev>:<path>` uses a tree path (root-relative, or cwd-relative only with ./ or
    ../), which Git does NOT normalize; the auditor normalizes it only to audit its destination. A `git show` that Git rejects (or any call whose
    result is an error or empty) is a FAILED read: it is never credited as delivered content and can never support a premise;
  * enumerated metadata and hashing commands (MD-1): git rev-parse, cat-file, status, log --oneline, worktree list, config --get,
    hash-object (without -w) and a restricted `git diff <base> <commit> -- <closure files>`; sha256sum, wc, pwd, date;
  * read-only text tools (RD-2) extended with sort, uniq, cut, tr, diff, cmp and nl; still no directory as an argument and no recursive grep.

Usage: python post-review.py <transcript.jsonl> <clone> <run dir> <result.schema.json> <out.json>
"""
import hashlib
import json
import os
import posixpath
import re
import shlex
import subprocess
import sys
import tempfile

AUDITOR_VERSION = "v4 (kit v4; declared and self-tested before the run; Coordinator order §7)"


def setup(clone, run):
    global CLONE, RUN, CLOSURE, REV, IDS, ALLOWED_LC, RUN_FILES, LONG_LINES, HARNESS, OBJECT_BLOB, CLONE_N, RUN_N, WT_BASE, OWN_RX, BASE
    global DIFF_BASES, CLONE_REAL
    CLONE, RUN = clone, run
    CLOSURE = json.load(open(os.path.join(RUN, "closure.json"), encoding="utf-8"))
    REV = CLOSURE["AuthorityRevision"]
    IDS = CLOSURE["Ids"]
    ALLOWED_LC = {r["Path"].lower(): r["Path"] for r in CLOSURE["CanonicalInputs"] + CLOSURE["AllowedTransitiveInputs"]}
    RUN_FILES = set(CLOSURE["RunFiles"])
    LONG_LINES = {p: set(v) for p, v in CLOSURE["ReadPolicy"]["LongLines"].items()}
    HARNESS = "docs/automation/evidence/I-62-A1/a1-counterexamples.py"
    OBJECT_BLOB = next(r["Blob"] for r in CLOSURE["CanonicalInputs"] if r["Path"] == "docs/initiatives/I-62-A-1.md")
    DIFF_BASES = [c.lower() for c in CLOSURE.get("DiffBases", [])]
    CLONE_N, RUN_N = norm(CLONE).lower(), norm(RUN).lower()
    CLONE_REAL = real(CLONE)
    BASE = os.path.basename(CLONE_N)
    WT_BASE = "d:/documentos/worktrees/" + BASE
    OWN_RX = re.compile(r"/appdata/local/temp/claude/d--documentos-worktrees-" + re.escape(BASE) + r"-[^/]+/")


def norm(p):
    p = p.strip().strip('"').strip("'").replace("\\", "/")
    m = re.match(r"^/([a-zA-Z])/(.*)$", p)
    if m:
        p = m.group(1) + ":/" + m.group(2)
    return re.sub(r"/+", "/", p).rstrip("/") or "/"


def real(p):
    """Effective path: `.`/`..` collapsed; symbolic links and junctions resolved for the part that exists."""
    q = os.path.normpath(norm(p))
    try:
        q = os.path.realpath(q)
    except OSError:
        pass
    return norm(q).lower()


def under(p, root):
    return p == root or p.startswith(root.rstrip("/") + "/")


def expand_tilde(p):
    return os.path.expanduser(p) if p.startswith("~") else p


def classify(p, cwd=None):
    """→ (kind, ref): CLOSURE rel | RUN name | OWN path | ROOT path | DIR path (directory inside a root) | NULLDEV | OUTSIDE path."""
    raw = norm(expand_tilde(p))
    if raw.lower() in ("/dev/null", "nul", "$null"):
        return ("NULLDEV", raw)
    if not re.match(r"^[a-zA-Z]:/", raw) and not raw.startswith("/") and cwd:
        raw = norm(cwd) + "/" + raw
    lex = norm(posixpath.normpath(raw)).lower()          # lexical: `.` and `..` collapsed
    eff = real(raw)                                       # effective: links resolved; only the effective destination counts
    if under(eff, RUN_N) and eff != RUN_N:
        name = eff[len(RUN_N) + 1:]
        return ("RUN", name) if name in RUN_FILES else ("OUTSIDE", raw)
    if OWN_RX.search(eff + "/") or OWN_RX.search(lex + "/"):
        if OWN_RX.search(eff + "/") and OWN_RX.search(lex + "/"):
            return ("OWN", raw)
        return ("OUTSIDE", raw)                           # own-output text but a link escapes it, or the reverse
    roots = [CLONE_REAL]
    if under(eff, WT_BASE) and eff != WT_BASE:
        roots.append(WT_BASE + "/" + eff[len(WT_BASE) + 1:].split("/", 1)[0])
    for root in roots:
        if eff == root:
            return ("ROOT", raw)
        if under(eff, root):
            rel = eff[len(root) + 1:]
            if rel in ALLOWED_LC:
                return ("CLOSURE", ALLOWED_LC[rel])
            if os.path.isdir(eff) or any(k.startswith(rel + "/") for k in ALLOWED_LC):
                return ("DIR", raw)
            return ("OUTSIDE", raw)
    return ("OUTSIDE", raw)


def git_object_path(path, cwd):
    """Git tree path of `<rev>:<path>` → (normalized repo-relative path for auditing, rejected_by_git)."""
    if path.startswith("./") or path.startswith("../"):
        e = real(cwd) if cwd else CLONE_REAL
        roots = [CLONE_REAL] + ([WT_BASE + "/" + e[len(WT_BASE) + 1:].split("/", 1)[0]] if under(e, WT_BASE) and e != WT_BASE else [])
        base = next((e[len(r) + 1:] if e != r else "" for r in roots if under(e, r)), "")
        full = posixpath.normpath(posixpath.join(base, path))
        return full, full.startswith("..")
    full = posixpath.normpath(path)
    return full, (full != path.rstrip("/") or full.startswith(".."))   # Git does not normalize `..` inside a tree path


canon_cache = {}


def canon(rel):
    if rel not in canon_cache:
        canon_cache[rel] = subprocess.check_output(["git", "-C", CLONE, "show", REV + ":" + rel]).decode("utf-8").replace("\r\n", "\n").split("\n")
    return canon_cache[rel]


def run_file(name):
    return open(os.path.join(RUN, name), encoding="utf-8").read().replace("\r\n", "\n").split("\n")


# ------------------------------------------------------------------ command audit helpers
READ_WORDS = {"sed", "wc", "awk", "grep", "head", "tail", "cat", "sha256sum", "echo", "printf", "true", "sort", "uniq", "cut", "tr", "diff", "cmp", "nl"}
ALLOWED_WORDS = READ_WORDS | {"git", "python", "python3", "cd", "dotnet", "pwd", "date", "mkdir", "read"}
CONTROL = {"do", "done", "while", "if", "then", "fi", "else", "elif", "{", "}"}
PATHLIKE = re.compile(r"^([a-zA-Z]:[\\/]|/[a-zA-Z]/|\.{1,2}/|~/|docs/|tests/|src/|plugin/|AGENTS\.md$|CLAUDE\.md$|README\.md$)|\.(md|json|py|yml|yaml|cs|csproj|txt|log|jsonl|out)$")
DRIVE_IN_CODE = re.compile(r"[a-zA-Z]:[\\/][^'\"\s,);|&]+")


def split_segments(cmd):
    segs, cur, q, i = [], "", None, 0
    while i < len(cmd):
        ch = cmd[i]
        if q:
            cur += ch
            if ch == q:
                q = None
        elif ch in "'\"":
            q, cur = ch, cur + ch
        elif cmd.startswith("&&", i) or cmd.startswith("||", i):
            segs.append(cur); cur = ""; i += 1
        elif ch in "|;\n":
            segs.append(cur); cur = ""
        else:
            cur += ch
        i += 1
    segs.append(cur)
    return [s.strip() for s in segs if s.strip()]


def tokens_of(seg):
    try:
        toks = shlex.split(seg, posix=False)
    except ValueError:
        toks = seg.split()
    return [t[1:-1] if len(t) >= 2 and t[0] == t[-1] and t[0] in "'\"" else t for t in toks]


TEST_PROJECT = "tests/rackcad.tests/rackcad.tests.csproj"


def is_test_project(c, cwd):
    k, ref = classify(c, cwd)
    return k == "OUTSIDE" and real(ref).endswith("/" + TEST_PROJECT) and (under(real(ref), CLONE_REAL) or under(real(ref), WT_BASE))


MODULES = r"(?:subprocess|urllib|requests|socket|http|shutil|ctypes|glob)"
PY_FORBIDDEN = re.compile(r"\bos\.(walk|listdir|scandir|system|popen|remove|unlink|rmdir|rename|replace|makedirs|mkdir|chdir|symlink|link)\b|\.rglob\(|"
                          r"\.iterdir\(|\b(?:import|from)\s+" + MODULES + r"\b|(?<![\w.])" + MODULES + r"\s*\.|\b__import__\b|\bexec\(|\beval\(")
PY_LITERAL = re.compile(r"(?<![A-Za-z0-9_])[rbuRBU]{0,2}('''|\"\"\"|'|\")(.*?)\1", re.S)
own_scripts = {}


def audit_python_code(code, here):
    probs = []
    for m in PY_FORBIDDEN.finditer(PY_LITERAL.sub("''", code)):
        probs.append("python con un efecto no verificable (%s)" % m.group(0))
    for m in PY_LITERAL.finditer(code):
        lit = m.group(2)
        if "\n" in lit or not (DRIVE_IN_CODE.match(lit) or (PATHLIKE.search(lit) and "/" in lit)):
            continue
        k = classify(lit, here)[0]
        if k in ("ROOT", "DIR"):
            probs.append("python: directorio como argumento: %s" % lit)
        elif k not in ("CLOSURE", "RUN", "OWN", "NULLDEV"):
            probs.append("python: ruta fuera del cierre: %s" % lit)
    return probs


HEREDOC = re.compile(r"<<-?\s*(['\"]?)([A-Za-z_][A-Za-z0-9_]*)\1[^\n]*\n(.*?)\n\2[ \t]*(?=\n|$)", re.S)


OPT_WITH_ARG = {"-e", "-f", "-F", "-v", "-m", "-A", "-B", "-C", "-c", "-n", "-d", "-k", "-t", "-s"}


def check_paths(toks, here, word, probs):
    """Path arguments of a read-only tool. The pattern or program of grep/sed/awk and the text of echo/printf are not paths."""
    if word in ("echo", "printf"):
        return
    toks, i, out, pattern_seen = list(toks), 0, [], word not in ("grep", "sed", "awk")
    while i < len(toks):
        t = toks[i]
        if t in ("-e", "-f") and word in ("grep", "sed", "awk"):
            if t == "-f" and i + 1 < len(toks):
                out.append(toks[i + 1])                   # a script file is a path
            pattern_seen = True
            i += 2
            continue
        if t.startswith("-"):
            i += 2 if (t in OPT_WITH_ARG and word in ("awk", "cut", "sort", "head", "tail", "uniq", "nl", "tr", "grep") and t not in ("-c", "-n", "-s")) else 1
            continue
        if not pattern_seen:
            pattern_seen = True
            i += 1
            continue
        out.append(t)
        i += 1
    for t in out:
        for c in ([t] if PATHLIKE.search(t) else []):
            k = classify(c, here)[0]
            if k in ("ROOT", "DIR"):
                probs.append("directorio como argumento: %s" % c)
            elif k not in ("CLOSURE", "RUN", "OWN", "NULLDEV"):
                probs.append("ruta fuera del cierre: %s" % c)


def audit_bash(cmd, cwd, failed_info=None):
    """→ (problems, final cwd). failed_info collects git object reads that Git rejects."""
    probs, here, bodies = [], cwd, {}

    def _keep(m):
        key = "__HEREDOC_%d__" % len(bodies)
        bodies[key] = m.group(3)
        return cmd[m.start():m.start(3)].split("\n")[0] + " " + key
    cmd = HEREDOC.sub(_keep, cmd)
    env = {}
    for seg in split_segments(cmd):
        for name, val in re.findall(r"^([A-Za-z_][A-Za-z0-9_]*)=(\"[^\"]*\"|'[^']*'|\S+)\s*$", seg):
            env[name] = val.strip("\"'")
    for name, val in sorted(env.items(), key=lambda kv: -len(kv[0])):
        cmd = cmd.replace("${%s}" % name, val).replace("$" + name, val)
    for seg in split_segments(cmd):
        toks = tokens_of(seg)
        if toks and toks[0] == "for":                       # v4: the loop header is parsed BEFORE stripping control words
            words = toks[toks.index("in") + 1:] if "in" in toks else []
            check_paths(words, here, "for", probs)
            continue
        while toks and (toks[0] in CONTROL or re.match(r"^[A-Za-z_][A-Za-z0-9_]*=", toks[0])):
            toks = toks[1:]
        if not toks:
            continue
        word = toks[0].split("/")[-1].split("\\")[-1].lower().replace(".exe", "")
        if word not in ALLOWED_WORDS:
            probs.append("comando no permitido: %s" % toks[0])
            continue
        seg_bodies = [bodies[t] for t in toks if t in bodies]
        toks = [t for t in toks if t not in bodies and not t.startswith("<<")]
        for body in seg_bodies:
            if word in ("python", "python3"):
                probs += audit_python_code(body, here)
            else:
                for c in DRIVE_IN_CODE.findall(body):
                    if classify(c, here)[0] not in ("CLOSURE", "RUN", "OWN"):
                        probs.append("ruta fuera del cierre en un heredoc: %s" % c)
        rest, redirect_targets, i, clean = toks[1:], [], 0, []
        while i < len(rest):
            t = rest[i]
            m = re.match(r"^(\d?|&|\*)?>{1,2}(.*)$", t)
            if m and not t.startswith(">&") and t != "2>&1":
                target = m.group(2) or (rest[i + 1] if i + 1 < len(rest) else "")
                if not m.group(2):
                    i += 1
                redirect_targets.append(target)
            elif t != "2>&1":
                clean.append(t)
            i += 1
        for tgt in redirect_targets:
            k, ref = classify(tgt, here)
            if k not in ("OWN", "NULLDEV"):
                probs.append("redirección fuera de las salidas propias: %s" % tgt)
            elif seg_bodies and k == "OWN":
                own_scripts[real(ref)] = seg_bodies[-1]
        if word == "cd":
            target = clean[0] if clean else "~"
            k, ref = classify(target, here)
            if k not in ("ROOT", "DIR", "OWN"):
                probs.append("cd fuera del clon, del worktree (raíz o subdirectorio) o de las salidas propias: %s" % target)
            else:
                here = ref
            continue
        if word in ("pwd", "date", "true", "read"):
            continue
        if word == "mkdir":
            for t in clean:
                if not t.startswith("-") and classify(t, here)[0] != "OWN":
                    probs.append("mkdir fuera de las salidas propias: %s" % t)
            continue
        if word == "git":
            j = 0
            while j < len(clean) and clean[j].startswith("-"):
                if clean[j] in ("-C", "-c"):
                    if clean[j] == "-C" and j + 1 < len(clean) and classify(clean[j + 1], here)[0] not in ("ROOT", "DIR"):
                        probs.append("git -C fuera del clon o del worktree: %s" % clean[j + 1])
                    j += 2
                else:
                    j += 1
            gcwd = here
            if "-C" in clean[:j]:
                gcwd = classify(clean[clean.index("-C") + 1], here)[1]
            sub = clean[j] if j < len(clean) else ""
            args = clean[j + 1:]
            if sub in ("rev-parse", "cat-file"):
                for a in args:
                    if ":" in a and not re.match(r"^[a-zA-Z]:[\\/]", a):
                        path, rejected = git_object_path(a.split(":", 1)[1], gcwd)
                        if path and path not in (".",) and path.lower() not in ALLOWED_LC:
                            probs.append("git %s de una ruta fuera del cierre: %s" % (sub, a))
            elif sub == "show":
                spec = [a for a in args if not a.startswith("-")]
                if not spec or any(":" not in a for a in spec):
                    probs.append("git show sin <rev>:<ruta del cierre>")
                for a in spec:
                    if ":" in a:
                        rev, raw_path = a.split(":", 1)
                        path, rejected = git_object_path(raw_path, gcwd)
                        if not ((REV.startswith(rev.lower()) and len(rev) >= 7) or rev == "HEAD") or path.lower() not in ALLOWED_LC:
                            probs.append("git show fuera del cierre: %s" % a)
                        elif rejected and failed_info is not None:
                            failed_info.append({"Spec": a, "AuditedDestination": ALLOWED_LC[path.lower()],
                                                "Effect": "Git no normaliza `..` en una ruta de árbol: lectura fallida, nunca acreditada"})
            elif sub == "diff":
                if "--" not in args:
                    probs.append("git diff sin `--` y rutas del cierre")
                else:
                    k2 = args.index("--")
                    revs = [a for a in args[:k2] if not a.startswith("-")]
                    opts = [a for a in args[:k2] if a.startswith("-")]
                    paths = args[k2 + 1:]
                    ok_rev = lambda r: (len(r) >= 7 and (REV.startswith(r.lower()) or any(b.startswith(r.lower()) for b in DIFF_BASES))) or r == "HEAD"
                    if not revs or len(revs) > 2 or not all(ok_rev(r) for r in revs):
                        probs.append("git diff con revisiones distintas de la base declarada y del commit")
                    if any(not re.match(r"^(--stat|--numstat|--no-color|--word-diff(=\w+)?|-U\d+|--unified=\d+|--no-ext-diff)$", o) for o in opts):
                        probs.append("git diff con opciones no permitidas")
                    if not paths or any(git_object_path(p, gcwd)[0].lower() not in ALLOWED_LC for p in paths):
                        probs.append("git diff de rutas fuera del cierre")
            elif sub == "hash-object":
                if "-w" in args or "--stdin" in args:
                    probs.append("git hash-object con escritura o stdin")
                for a in args:
                    if not a.startswith("-") and classify(a, gcwd)[0] not in ("CLOSURE", "OWN", "RUN"):
                        probs.append("git hash-object fuera del cierre: %s" % a)
            elif sub == "checkout":
                if args[:1] != ["--detach"] or len(args) != 2 or not REV.startswith(args[1].lower()) or len(args[1]) < 7:
                    probs.append("git checkout distinto de --detach <commit>")
            elif sub == "log":
                if any(a for a in args if not re.match(r"^(--oneline|-\d+|-n|\d+)$", a)):
                    probs.append("git log con argumentos no permitidos")
            elif sub == "worktree":
                if args[:1] != ["list"]:
                    probs.append("git worktree distinto de list")
            elif sub == "status":
                pass
            elif sub == "config":
                if args[:1] != ["--get"]:
                    probs.append("git config distinto de --get")
            else:
                probs.append("subcomando git no permitido: %s" % sub)
            continue
        if word == "grep" and any(re.match(r"^-[a-zA-Z]*[rR]", t) or t == "--recursive" for t in clean):
            probs.append("grep recursivo")
        if word == "sed":
            inplace = any(t == "-i" or re.match(r"^-[a-zA-Z]*i", t) or t.startswith("--in-place") for t in clean)
            if inplace:
                nonopt, script, j2 = [], None, 0
                while j2 < len(clean):
                    t = clean[j2]
                    if t in ("-e", "--expression") and j2 + 1 < len(clean):
                        script = clean[j2 + 1]
                        j2 += 2
                        continue
                    if not t.startswith("-"):
                        nonopt.append(t)
                    j2 += 1
                if script is None:
                    script, files = (nonopt[0], nonopt[1:]) if nonopt else ("", [])
                else:
                    files = nonopt
                if not files:
                    probs.append("sed -i sin archivo auditable")
                for f in files:
                    k, ref = classify(f, here)
                    if k != "OWN":
                        probs.append("sed -i fuera de las salidas propias (insumo canónico, script custodiado, fuente o archivo ajeno): %s" % f)
                    else:
                        key = real(ref)
                        own_scripts[key] = own_scripts.get(key, "") + "\n# sed -i: " + script
                for c in DRIVE_IN_CODE.findall(script):
                    if classify(c, here)[0] not in ("CLOSURE", "RUN", "OWN"):
                        probs.append("sed -i: ruta fuera del cierre en la expresión: %s" % c)
                continue
            if "-n" not in clean and not any(re.match(r"^-[a-zA-Z]*n", t) for t in clean):
                probs.append("sed sin -n ni -i sobre salidas propias")
        if word == "dotnet":
            if not (clean[:1] == ["test"] and any(is_test_project(t, here) for t in clean)):
                probs.append("dotnet distinto de test del proyecto Core del clon")
            for t in clean[1:]:
                for c in DRIVE_IN_CODE.findall(t) or ([t] if PATHLIKE.search(t) and not t.startswith("-") else []):
                    if not is_test_project(c, here) and classify(c, here)[0] not in ("OWN", "NULLDEV"):
                        probs.append("dotnet: ruta distinta del proyecto y de las salidas propias: %s" % c)
            continue
        if word in ("python", "python3"):
            if "-c" in clean:
                i_c = clean.index("-c")
                probs += audit_python_code(clean[i_c + 1] if i_c + 1 < len(clean) else "", here)
                continue
            script = next((t for t in clean if not t.startswith("-")), None)
            if script is None or script == "-":
                if not seg_bodies:
                    probs.append("python sin código auditable")
                continue
            kind, ref = classify(script, here)
            rest_args = [t for t in clean[clean.index(script) + 1:] if not t.startswith("-")]
            if kind == "CLOSURE" and ref == HARNESS:
                if len(rest_args) != 1 or classify(rest_args[0], here)[0] != "OWN":
                    probs.append("el arnés exige exactamente una salida propia como argumento")
                continue
            if kind == "OWN":
                body = own_scripts.get(real(ref))
                if body is None:
                    probs.append("script propio sin contenido auditable en la transcripción: %s" % script)
                else:
                    probs += audit_python_code(body, here)
                for a in rest_args:
                    if classify(a, here)[0] not in ("CLOSURE", "RUN", "OWN"):
                        probs.append("ruta fuera del cierre: %s" % a)
                continue
            probs.append("python de un script distinto del arnés y de los scripts propios: %s" % script)
            continue
        check_paths(clean, here, word, probs)
    return probs, here


PS_VALUE_OPTS = {"-tail", "-totalcount", "-head", "-first", "-last", "-skip", "-encoding", "-delimiter", "-readcount", "-pattern"}


def ps_target(toks):
    target, i = None, 1
    while i < len(toks):
        t = toks[i]
        tl = t.lower()
        if tl in ("-literalpath", "-path") and i + 1 < len(toks):
            return toks[i + 1]
        if tl.startswith("-"):
            i += 2 if tl in PS_VALUE_OPTS else 1
            continue
        if target is None:
            target = t
        i += 1
    return target


def audit_powershell(cmd, cwd):
    """PowerShell: dotnet test of the clone's Core project; Get-Content of closure, run or own files (optionally piped to Select-Object or
    Measure-Object); Set-Location to the clone, the worktree (root or subdirectory) or own outputs."""
    low = cmd.strip().lower()
    probs = []
    if "dotnet" in low and " test " in " " + low.replace("&", " ") + " " and "rackcad.tests.csproj" in low:
        for c in DRIVE_IN_CODE.findall(cmd):
            k = classify(c, cwd)[0]
            if k not in ("ROOT", "DIR", "CLOSURE", "OWN") and "microsoft/dotnet/dotnet.exe" not in norm(c).lower() and not is_test_project(c, cwd):
                probs.append("ruta fuera del clon o de las salidas propias: %s" % c)
        return probs
    segs = [s.strip() for s in split_segments(cmd)]
    first = tokens_of(segs[0]) if segs else []
    head = first[0].lower() if first else ""
    for s in segs[1:]:
        if not re.match(r"^(select-object|measure-object|out-string)\b", s.lower()):
            probs.append("PowerShell: tubería no permitida: %s" % s[:80])
    if head in ("get-content", "gc", "cat", "type"):
        t = ps_target(first)
        if t is None or classify(t, cwd)[0] not in ("OWN", "CLOSURE", "RUN"):
            probs.append("Get-Content fuera del cierre y de las salidas propias: %s" % t)
        return probs
    if head in ("set-location", "cd", "push-location", "sl") and len(segs) == 1:
        t = ps_target(first)
        if t is None or classify(t, cwd)[0] not in ("ROOT", "DIR", "OWN"):
            probs.append("Set-Location fuera del clon, del worktree o de las salidas propias: %s" % t)
        return probs
    return ["PowerShell no permitido: %s" % cmd[:120]]


# ------------------------------------------------------------------ main audit
def audit_transcript(transcript, schema):
    own_scripts.clear()
    entries = []
    for line in open(transcript, encoding="utf-8"):
        try:
            entries.append(json.loads(line))
        except ValueError:
            pass
    identity = {"SessionIds": set(), "Cwds": set(), "Versions": set(), "Models": {}, "SidechainEntries": 0}
    uses, results, errors, cwd_of, assistant_texts, order = {}, {}, {}, {}, [], []
    for o in entries:
        for k, key in (("SessionIds", "sessionId"), ("Cwds", "cwd"), ("Versions", "version")):
            if o.get(key):
                identity[k].add(o[key])
        if o.get("isSidechain"):
            identity["SidechainEntries"] += 1
        msg = o.get("message") or {}
        if o.get("type") == "assistant" and msg.get("model"):
            identity["Models"][msg["model"]] = identity["Models"].get(msg["model"], 0) + 1
        content = msg.get("content")
        if not isinstance(content, list):
            continue
        for part in content:
            if not isinstance(part, dict):
                continue
            if part.get("type") == "tool_use":
                uses[part["id"]] = {"name": part.get("name"), "input": part.get("input") or {}}
                cwd_of[part["id"]] = o.get("cwd")
                order.append(part["id"])
            elif part.get("type") == "tool_result":
                c = part.get("content")
                results[part.get("tool_use_id")] = c if isinstance(c, str) else "".join(x.get("text", "") for x in (c or []) if isinstance(x, dict))
                errors[part.get("tool_use_id")] = bool(part.get("is_error"))
            elif part.get("type") == "text" and o.get("type") == "assistant":
                assistant_texts.append(part.get("text", ""))

    violations = []
    PROMPT_SHA = hashlib.sha256(open(os.path.join(RUN, "prompt.md"), "rb").read()).hexdigest()
    ORDER_SHA = hashlib.sha256(open(os.path.join(RUN, "order.txt"), "rb").read()).hexdigest()
    checks = {"PromptHashSeen": False, "OrderHashSeen": False, "PromptReadFaithful": False, "OrderReadComplete": False,
              "CloneHeadVerifiedBeforeReading": False, "WorktreeHeadVerifiedBeforeReading": False, "FirstSubstantiveCall": None}
    audit, fidelity, delivered, grep_info, unicode_anomalies, truncated_known, failed_reads = [], [], {}, [], [], [], []

    def compare(ref, kind, numbered, via="Read"):
        lines = canon(ref) if kind == "CLOSURE" else run_file(ref)
        bad = []
        for n, body in numbered:
            if 1 <= n <= len(lines) and lines[n - 1] == body:
                delivered.setdefault(ref, set()).add(n)
            elif via == "Read" and kind == "CLOSURE" and n in LONG_LINES.get(ref, set()) and len(body) >= 1000 and \
                    lines[n - 1].startswith(body.rstrip().rstrip("…").rstrip(".").rstrip()):
                truncated_known.append({"Path": ref, "Line": n, "DeliveredChars": len(body), "CanonicalChars": len(lines[n - 1])})
            else:
                bad.append(n)
        return bad

    clone_rx = re.escape(BASE)

    def substantive(name, inp, cwd):
        if name == "Read":
            return classify(inp.get("file_path", ""), cwd)[0] == "CLOSURE"
        if name == "Grep":
            return True
        if name == "Bash":
            c = inp.get("command", "")
            return bool(re.search(r"\bshow\s+\S+:", c)) or bool(re.search(r"\b(sed|cat|grep|awk|head|tail|python3?)\b.*" + clone_rx + r"[/\\]", c))
        return False

    for idx, uid in enumerate(order):
        u = uses[uid]
        name, inp, out, cwd = u["name"], u["input"], results.get(uid), cwd_of.get(uid)
        failed = errors.get(uid, False) or not (out or "").strip()
        row = {"Index": idx, "Tool": name, "Input": {k: (v if len(str(v)) < 400 else str(v)[:400] + "…") for k, v in inp.items()},
               "Class": None, "Failed": failed, "Problems": []}
        if checks["FirstSubstantiveCall"] is None and substantive(name, inp, cwd):
            checks["FirstSubstantiveCall"] = idx
        if name == "Read":
            kind, ref = classify(inp.get("file_path", ""), cwd)
            row["Class"] = kind
            if kind not in ("CLOSURE", "RUN", "OWN"):
                row["Problems"].append("lectura fuera del cierre" + (" (intento fallido; sigue siendo una acción fuera del contrato)" if failed else ""))
            elif kind != "OWN" and failed:
                failed_reads.append({"Index": idx, "Path": ref, "Via": "Read", "Effect": "lectura fallida: nada acreditado"})
            elif kind != "OWN" and out is not None:
                pairs = [(int(m.group(1)), m.group(2)) for m in (re.match(r"^\s*(\d+)\t(.*)$", raw) for raw in out.replace("\r\n", "\n").split("\n")) if m]
                bad = compare(ref, kind, pairs)
                fidelity.append({"Path": ref, "Via": "Read", "Lines": len(pairs), "Altered": bad[:50],
                                 "Status": "FAITHFUL_NORMALIZED" if pairs and not bad else ("DEGRADED" if bad else "EMPTY")})
                if ref == "prompt.md" and pairs and not bad:
                    checks["PromptReadFaithful"] = True
        elif name == "Grep":
            p = inp.get("path", "")
            kind, ref = classify(p, cwd) if p else ("OUTSIDE", "(sin path: directorio de trabajo)")
            row["Class"] = kind
            if kind not in ("CLOSURE", "RUN") or inp.get("glob"):
                row["Problems"].append("búsqueda fuera de un archivo del cierre (directorio, glob o ruta no listada)")
            elif out and kind == "CLOSURE" and not failed:
                for raw in out.replace("\r\n", "\n").split("\n"):
                    m = re.match(r"^(?:.*?:)?(\d+)[:-](.*)$", raw)
                    if m:
                        n, body = int(m.group(1)), m.group(2)
                        lines = canon(ref)
                        grep_info.append({"Path": ref, "Line": n, "Exact": 1 <= n <= len(lines) and lines[n - 1] == body})
        elif name == "Bash":
            cmd = inp.get("command", "")
            row["Class"] = "BASH"
            finfo = []
            probs, _ = audit_bash(cmd, cwd, finfo)
            row["Problems"] += probs
            for f in finfo:
                f["Index"] = idx
                failed_reads.append(f)
            if out and not errors.get(uid):
                if PROMPT_SHA in out:
                    checks["PromptHashSeen"] = True
                if ORDER_SHA in out:
                    checks["OrderHashSeen"] = True
                first = checks["FirstSubstantiveCall"]
                if (first is None or idx < first) and REV in out:
                    if re.search(r"git\s+-C\s+[\"']?[A-Za-z]:[\\/]" + clone_rx + r"[\"']?\s+rev-parse\s+HEAD\b", cmd) or \
                            re.search(r"cd\s+[\"']?[A-Za-z]:[\\/]" + clone_rx + r"[\"']?\s*(&&|;)\s*git\s+rev-parse\s+HEAD\b", cmd):
                        checks["CloneHeadVerifiedBeforeReading"] = True
                    if (re.search(r"(^|[;&|]\s*)git\s+rev-parse\s+HEAD\b", cmd) and cwd and under(real(cwd), WT_BASE)) or \
                            re.search(r"git\s+-C\s+[\"']?[A-Za-z]:[\\/]Documentos[\\/]Worktrees[\\/]" + clone_rx + r"[\\/][^\s\"']+[\"']?\s+rev-parse\s+HEAD\b", cmd, re.I):
                        checks["WorktreeHeadVerifiedBeforeReading"] = True
            m = re.match(r"^\s*git\s+-C\s+\S+\s+show\s+([0-9a-fA-F]{7,40}|HEAD):(\S+)\s*\|\s*sed\s+-n\s+['\"]?(\d+)(?:,(\d+))?p['\"]?\s*$", cmd)
            if m and out is not None and not failed and m.group(2).lower() in ALLOWED_LC:
                rel = ALLOWED_LC[m.group(2).lower()]
                a, b = int(m.group(3)), int(m.group(4) or m.group(3))
                got = out.replace("\r\n", "\n").rstrip("\n").split("\n")
                bad = compare(rel, "CLOSURE", list(zip(range(a, b + 1), got)))
                fidelity.append({"Path": rel, "Via": "git show | sed", "Lines": len(got), "Altered": bad[:50],
                                 "Status": "FAITHFUL_NORMALIZED" if not bad and len(got) == b - a + 1 else "DEGRADED"})
        elif name == "PowerShell":
            row["Class"] = "POWERSHELL"
            row["Problems"] += audit_powershell(inp.get("command", ""), cwd)
        elif name in ("Write", "Edit"):
            kind, ref = classify(inp.get("file_path", ""), cwd)
            row["Class"] = kind
            if kind != "OWN":
                row["Problems"].append("%s fuera de las salidas propias (insumo canónico, script custodiado, fuente o archivo ajeno)" % name.lower())
            elif name == "Write":
                own_scripts[real(ref)] = inp.get("content", "")
            else:
                key = real(ref)
                body = own_scripts.get(key)
                old, new = inp.get("old_string", ""), inp.get("new_string", "")
                own_scripts[key] = (body.replace(old, new) if body is not None and old in body else (body or "") + "\n" + new)
        elif name in ("TodoWrite", "TaskCreate", "TaskUpdate", "TaskList", "ToolSearch"):
            row["Class"] = "BOOKKEEPING"
        else:
            row["Class"] = "FORBIDDEN_TOOL"
            row["Problems"].append("herramienta no permitida (%s)" % name)
        if out and re.search(r"(?i)output (was )?truncated|output too large|exceeds maximum", out):
            row["TruncationMarker"] = True
        if out and "\ufffd" in out:
            row["ReplacementChars"] = out.count("\ufffd")
            unicode_anomalies.append({"Index": idx, "Tool": name, "Count": out.count("\ufffd")})
        if uid not in results:
            row["Problems"].append("llamada sin resultado (terminación incompleta)")
        audit.append(row)
        for p in row["Problems"]:
            violations.append("llamada %d (%s): %s" % (idx, name, p))

    order_lines = run_file("order.txt")
    checks["OrderReadComplete"] = all(n in delivered.get("order.txt", set()) for n in range(1, len(order_lines) + (0 if order_lines[-1] == "" else 1)))
    clone_status = subprocess.run(["git", "-C", CLONE, "status", "--porcelain"], capture_output=True, text=True).stdout
    clone_head = subprocess.run(["git", "-C", CLONE, "rev-parse", "HEAD"], capture_output=True, text=True).stdout.strip()
    checks["CloneCleanAfterRun"] = clone_status.strip() == "" and clone_head == REV
    checks["NoSidechain"] = identity["SidechainEntries"] == 0
    checks["FinalAssistantText"] = bool(assistant_texts)
    for f in fidelity:
        if f["Status"] == "DEGRADED":
            violations.append("fidelidad: %s (%s) líneas alteradas %s" % (f["Path"], f["Via"], f["Altered"][:10]))
    for key, msg in (("PromptHashSeen", "no se comprobó el SHA-256 de prompt.md"), ("OrderHashSeen", "no se comprobó el SHA-256 de order.txt"),
                     ("PromptReadFaithful", "prompt.md no se leyó de forma fiel"), ("OrderReadComplete", "order.txt no se leyó entera de forma fiel"),
                     ("CloneCleanAfterRun", "el clon no quedó limpio o cambió de HEAD"),
                     ("NoSidechain", "la transcripción tiene entradas de cadena lateral (subagentes)"),
                     ("FinalAssistantText", "la sesión terminó sin un mensaje final del asistente"),
                     ("CloneHeadVerifiedBeforeReading", "HEAD del clon no verificado antes de la primera lectura sustantiva"),
                     ("WorktreeHeadVerifiedBeforeReading", "HEAD del worktree propio no verificado antes de la primera lectura sustantiva")):
        if not checks[key]:
            violations.append(msg)

    final = assistant_texts[-1] if assistant_texts else ""
    blocks = re.findall(r"```json\s*\n(.*?)\n```", final, re.S)
    result, result_check = None, {"Found": bool(blocks), "SchemaValid": False, "Coherence": []}
    if blocks:
        try:
            result = json.loads(blocks[-1])
        except ValueError as e:
            result_check["Coherence"].append("JSON inválido: %s" % e)
    expected = ["A62-A1-0%d" % i for i in range(1, 7)] + ["OBS-A1-01"] + ["A62-A1R-0%d" % i for i in range(1, 4)] + ["A62-A1S-01", "A62-A1S-02", "A62-A1T-01"]
    if result is not None:
        tmp = tempfile.mkdtemp()
        rp = os.path.join(tmp, "result.json")
        json.dump(result, open(rp, "w", encoding="utf-8"), ensure_ascii=False)
        ps = "try { $null = Get-Content -Raw -LiteralPath '%s' | Test-Json -SchemaFile '%s' -ErrorAction Stop; 'True' } catch { 'False: ' + $_.Exception.Message }" % (rp, schema)
        o = subprocess.run(["pwsh", "-NoProfile", "-Command", ps], capture_output=True, text=True, encoding="utf-8", errors="replace").stdout.strip()
        result_check["SchemaValid"] = o == "True"
        result_check["SchemaDetail"] = o[:400]
        c = result_check["Coherence"]
        v = result.get("Verdict")
        req = result.get("RequiredFindings") or []
        disp = {d.get("FindingId"): d.get("State") for d in result.get("FindingDispositions") or []}
        ia = result.get("IfAgreed") or {}
        if sorted(d.get("FindingId") for d in result.get("FindingDispositions") or []) != sorted(expected):
            c.append("FindingDispositions debe disponer A62-A1-01..06, OBS-A1-01, A62-A1R-01..03, A62-A1S-01..02 y A62-A1T-01 una vez cada uno")
        if sorted(x.get("Item") for x in result.get("Focus") or []) != list(range(1, CLOSURE["FocusItems"] + 1)):
            c.append("Focus debe contestar los ítems 1-%d una vez cada uno" % CLOSURE["FocusItems"])
        if sorted(x.get("Id") for x in result.get("OptionalCorrections") or []) != ["A62-A1T-O1", "A62-A1T-O2", "A62-A1T-O3"]:
            c.append("OptionalCorrections debe contestar A62-A1T-O1..O3 una vez cada uno")
        if v == "AGREED" and (req or any(disp.get(k) != "CLOSED" for k in expected) or not all(ia.get(k) is True for k in ia)):
            c.append("AGREED exige cero REQUIRED, los trece hallazgos CLOSED e IfAgreed todo en true")
        if v != "AGREED" and any(x is not None for x in ia.values()):
            c.append("IfAgreed debe ir en null salvo con AGREED")
        if v == "CHANGES REQUIRED" and not req and all(disp.get(k) == "CLOSED" for k in expected):
            c.append("CHANGES REQUIRED sin ningún REQUIRED ni hallazgo STILL_OPEN")
        if v == "BLOCKED — OWNER DECISION" and not (result.get("OwnerAuthorityCheck") or {}).get("OwnerDecisionRequired"):
            c.append("BLOCKED — OWNER DECISION sin OwnerDecisionRequired")
        if v == "NOT_ACCREDITED" and not result.get("KnownLimitations"):
            c.append("NOT_ACCREDITED sin el motivo en KnownLimitations")
        if result.get("ReviewedCommit") != REV or result.get("ReviewedBlob") != OBJECT_BLOB:
            c.append("objeto revisado distinto del designado")
        if any(result.get(k) != IDS[k] for k in ("RunId", "InvocationId", "LogicalReviewRequestId")):
            c.append("identificadores de la invocación distintos de los del cierre")
        for x in c:
            violations.append("resultado: " + x)
        if not result_check["SchemaValid"]:
            violations.append("resultado: no valida contra result.schema.json")
    else:
        violations.append("no hay bloque JSON final")

    premise_check = []
    for key, idk in (("RequiredFindings", "FindingId"), ("OptionalFindings", "FindingId"), ("FindingDispositions", "FindingId")):
        for f in (result or {}).get(key) or []:
            for pr in f.get("PremiseRefs") or []:
                rel_in = pr.get("Path", "")
                kind, rel = classify(rel_in, CLONE)
                a, b = pr.get("LineStart", 1), pr.get("LineEnd", 0)
                found = delivered_ok = False
                if kind == "CLOSURE":
                    seg = " ".join(canon(rel)[max(0, a - 1):b])
                    found = " ".join(pr.get("Quote", "").split()) in " ".join(seg.split())
                    delivered_ok = all(n in delivered.get(rel, set()) for n in range(a, b + 1))
                premise_check.append({"Group": key, "Finding": f.get(idk), "Path": rel, "Lines": [a, b], "QuoteFound": found, "DeliveredFaithfully": delivered_ok})
                if not found:
                    violations.append("premisa no canónica en %s (%s %s-%s)" % (f.get(idk), rel, a, b))
                elif not delivered_ok:
                    violations.append("premisa sobre líneas no entregadas fielmente en %s (%s %s-%s)" % (f.get(idk), rel, a, b))

    return {
        "Transcript": {"Path": transcript, "Sha256": hashlib.sha256(open(transcript, "rb").read()).hexdigest(), "Entries": len(entries)},
        "RuntimeIdentity": {k: (sorted(v) if isinstance(v, set) else v) for k, v in identity.items()},
        "AuditorVersion": AUDITOR_VERSION,
        "IdentityAndOrderChecks": checks,
        "CloneAfterRun": {"Head": clone_head, "StatusPorcelain": clone_status},
        "ReadAudit": {"Calls": len(audit), "WithProblems": [a for a in audit if a["Problems"]], "Rows": audit},
        "Fidelity": {"Records": fidelity, "DeliveredLinesByPath": {k: len(v) for k, v in sorted(delivered.items())},
                     "FailedReadsNeverCredited": failed_reads, "TruncatedKnownLongLines": truncated_known,
                     "UnicodeReplacementChars": unicode_anomalies,
                     "GrepContentLines": {"Total": len(grep_info), "Exact": sum(1 for g in grep_info if g["Exact"])}},
        "Result": {"Check": result_check, "Literal": result},
        "PremiseCheck": premise_check,
        "Accreditation": {"Status": "ACCREDITED" if not violations else "NOT_ACCREDITED", "Reasons": violations},
    }


if __name__ == "__main__":
    TRANSCRIPT, CLONE_ARG, RUN_ARG, SCHEMA, OUT = sys.argv[1:6]
    setup(CLONE_ARG, RUN_ARG)
    out = audit_transcript(TRANSCRIPT, SCHEMA)
    with open(OUT, "w", encoding="utf-8", newline="\n") as fh:
        json.dump(out, fh, ensure_ascii=False, indent=1)
        fh.write("\n")
    print(json.dumps({"Accreditation": out["Accreditation"]["Status"], "Violations": len(out["Accreditation"]["Reasons"]),
                      "Calls": out["ReadAudit"]["Calls"], "Verdict": (out["Result"]["Literal"] or {}).get("Verdict")}, ensure_ascii=True))
