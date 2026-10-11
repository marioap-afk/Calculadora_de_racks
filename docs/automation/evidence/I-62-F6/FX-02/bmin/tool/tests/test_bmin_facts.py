#!/usr/bin/env python3
"""Test suite for bmin_facts.py (SPEC section 4).

Covers: unit tests on synthetic repositories (commits, --no-ff merges, trailers,
non-ASCII text, a clause map with known defects, TRX, handoff JSON), the
fail-closed negative cases, zero writes (index sha256, for-each-ref,
status --porcelain --ignored, full file listing with sizes and mtimes, before
and after every tool run, plus an AST scan of the tool), determinism, the
forbidden vocabulary and static properties of the tool source.

Environment:
  BMIN_TOOL  path of the tool under test (default: ../bmin_facts.py)
  BMIN_TMP   root for temporary repositories (default: D:\\r62-bmin-tmp\\tool)
"""

import ast
import hashlib
import importlib.util
import json
import os
import platform
import shutil
import stat
import subprocess
import sys
import tempfile
import types
import unittest

HERE = os.path.dirname(os.path.abspath(__file__))
TOOL = os.path.abspath(os.environ.get("BMIN_TOOL") or os.path.join(HERE, "..", "bmin_facts.py"))
TMP_ROOT = os.environ.get("BMIN_TMP") or "D:\\r62-bmin-tmp\\tool"
PY = sys.executable

# ------------------------------------------------------------------ constants

O = "\N{LATIN SMALL LETTER O WITH ACUTE}"
E = "\N{LATIN SMALL LETTER E WITH ACUTE}"
A_ = "\N{LATIN SMALL LETTER A WITH ACUTE}"
U_ = "\N{LATIN SMALL LETTER U WITH ACUTE}"
I_ = "\N{LATIN SMALL LETTER I WITH ACUTE}"
NT = "\N{LATIN SMALL LETTER N WITH TILDE}"
NTC = "\N{LATIN CAPITAL LETTER N WITH TILDE}"
LAQ = "\N{LEFT-POINTING DOUBLE ANGLE QUOTATION MARK}"
RAQ = "\N{RIGHT-POINTING DOUBLE ANGLE QUOTATION MARK}"
EMD = "\N{EM DASH}"
SECT = "\N{SECTION SIGN}"

ENTRY = "## 12. Coexistencia de protocolos de ejecuci" + O + "n delegada (I61/I62)"
R163 = "### 16.13 Compatibilidad de protocolos de ejecuci" + O + "n delegada"
H1610 = "### 16.10 T" + E + "rminos de gate"
POINTER = "Antes de aplicar esta secci" + O + "n, toda unidad aplica " + SECT + "16.13."
PRE = "pre" + A_ + "mbulo"
TRAILER = "Agent-Protocol-Normative: I-62"
AP = "docs/AUTOMATION_PLAN.md"
WF = "docs/WORKFLOW.md"
MAP = "docs/automation/agent-execution/compatibility/I62-clause-map.json"
SCHEMA = "docs/automation/agent-execution/compatibility/clause-map.schema.json"
HSCHEMA = "docs/automation/agent-execution/schemas/worker-handoff.schema.json"
README = "docs/automation/agent-execution/README.md"
EXISTING = "docs/automation/agent-execution/existing.json"
SURF = ["AGENTS.md", AP, WF, "docs/adr/", "docs/automation/agent-execution/"]
FORBIDDEN_WORDS = ("pass", "fail", "PASS", "FAIL", "VERIFIED", "EXECUTION_", "MAP_VALID",
                   "MAP_INVALID", "REWORK", "BLOCKED", "STOP")
FORCED = ["-c", "core.quotepath=false", "-c", "core.fsmonitor=false"]
ALLOWED_SUBS = ("rev-parse", "cat-file", "ls-tree", "rev-list", "diff", "log", "show",
                "merge-base", "status", "check-ignore")
ENVELOPE_KEYS = {"Schema", "Command", "Args", "Inputs", "GitCalls", "Facts", "FactsSha256",
                 "Error", "Runtime"}

S16_V1 = "La ejecuci" + O + "n delegada es un modo. Segunda frase con 16.13 dentro."
S16_V2 = POINTER + " " + S16_V1
S16_D = "La ejecuci" + O + "n delegada es un modo. " + POINTER + " Segunda frase con 16.13 dentro."
S163_V1 = "Una secci" + O + "n va desde su encabezado. Si algo no es verificable, alto (S-12)."
S163_V2 = S163_V1 + " " + POINTER
S163_D = "Una seccion va desde su encabezado. Si algo no es verificable, alto (S-12). " + POINTER

TERMS_PARA = ("Coincidencia por palabra o frase completa, sin distinguir may" + U_ + "sculas, en `WorkCompleted`,"
              " `Evidence`, `UnexpectedFindings`, `KnownLimitations`, `Deviations`, `OpenQuestions` y\n"
              "`RecommendedNextAction`: `Candidato`, `Candidate`, `gate cerrado`, `rama integrada`,\n"
              "`integrada en main`, `ready to merge`. No se aplica al nombre de propiedad `Gate` ni a su"
              " valor. El Controller los comprueba.")
TERMS = ["Candidato", "Candidate", "gate cerrado", "rama integrada", "integrada en main", "ready to merge"]
MIDDLE_V2 = ("\n" + H1610 + "\n\n" + TERMS_PARA + "\n\n" + R163 + "\n\nTexto del resolver " + EMD
             + " guion largo.\n\n**Superficies** (lista cerrada): `AGENTS.md`, `docs/AUTOMATION_PLAN.md`,\n"
             "`docs/WORKFLOW.md`, `docs/adr/`, `docs/automation/agent-execution/`.\n\n"
             "```text\n## 99. No es encabezado\n```\n")


def ap_text(s16, s163, middle, qq="x"):
    return ("# Plan de prueba\n\nPre" + A_ + "mbulo con acento " + O + " y comillas " + LAQ + qq + RAQ + ".\n\n"
            "## 1. Prop" + O + "sito\n\nTexto uno.\n\n## 16. Ejecuci" + O + "n delegada\n\n" + s16 + "\n\n"
            "### 16.1 Participantes\n\nLista de participantes.\n\n### 16.3 Lectura de autoridades\n\n"
            + s163 + "\n\n### 16.4 Transporte\n\nTexto 16.4.\n" + middle + "\n## 17. Final\n\nFin.\n")


AP_V1 = ap_text(S16_V1, S163_V1, "")
AP_V2 = ap_text(S16_V2, S163_V2, MIDDLE_V2)
AP_D = ap_text(S16_D, S163_D, MIDDLE_V2, qq="y")
H16 = "## 16. Ejecuci" + O + "n delegada"
H163 = "### 16.3 Lectura de autoridades"
HPLAN = "# Plan de prueba"


def wf_head(t10):
    return ("# WORKFLOW " + EMD + " flujo de prueba\n\nIntro.\n\n## 10. Autoridad\n\n" + t10
            + "\n\n## 11. Otro\n\nTexto 11.\n")


ENTRY_SECTION = ("\n" + ENTRY + "\n\nDesde EFF (AUTOMATION_PLAN `" + R163 + "`), toda evaluaci" + O
                 + "n aplica primero esa subsecci" + O + "n.\n")
WF_V1 = wf_head("Texto 10.")
WF_V2 = wf_head("Texto 10 modificado.") + ENTRY_SECTION
WF_D = wf_head("Texto 10.") + "\n## 11. Otro\n\nTexto 11 bis.\n" + ENTRY_SECTION + "\n" + ENTRY + "\n\nCopia repetida.\n"
HWF = "# WORKFLOW " + EMD + " flujo de prueba"

AGENTS = "# Agentes\n\nReglas.\n"
README_BASIC = "Hola " + LAQ + "mundo" + RAQ + " " + EMD + " prueba " + O + "\n"
ADR2 = "# ADR 2\n"
ADR3 = "# ADR 3\n"
ADR4 = "# ADR 4\n"
README_V1 = "# README\n\n## 1. Uno\n\nUno.\n"
README_V2 = "# README\n\n## 1. Uno\n\nUno modificado.\n"
EXISTING_V1 = '{"v": 1}\n'
EXISTING_V2 = '{"v": 2}\n'


def schema_obj(extra_top=None):
    hexpat = "^[0-9a-f]{40}$"
    s = {
        "$schema": "https://json-schema.org/draft/2020-12/schema",
        "title": "rackcad-clause-map/v1",
        "description": "Mapa de cl" + A_ + "usulas (sint" + E + "tico).",
        "type": "object",
        "additionalProperties": False,
        "properties": {
            "Schema": {"type": "string", "enum": ["rackcad-clause-map/v1"]},
            "Protocol": {"type": "string", "enum": ["rackcad-protocol/I62"]},
            "LegacyProtocol": {"type": "string", "enum": ["rackcad-protocol/I61"]},
            "Surfaces": {"type": "array", "minItems": 1, "uniqueItems": True,
                         "items": {"type": "string", "minLength": 1}},
            "Files": {"type": "array", "items": {
                "type": "object", "additionalProperties": False,
                "properties": {
                    "Path": {"type": "string", "minLength": 1},
                    "FileKind": {"type": "string", "enum": ["MODIFIED", "ADDED", "ENTRY"]},
                    "BaseBlob": {"type": ["string", "null"], "pattern": hexpat},
                    "EffBlob": {"type": "string", "pattern": hexpat}},
                "required": ["BaseBlob", "EffBlob", "FileKind", "Path"]}},
            "Entries": {"type": "array", "items": {
                "type": "object", "additionalProperties": False,
                "properties": {
                    "Path": {"type": "string", "minLength": 1},
                    "Section": {"type": "string", "minLength": 1},
                    "Level": {"type": "integer", "minimum": 0, "maximum": 6},
                    "Kind": {"type": "string", "enum": ["MODIFIED", "REMOVED", "ADDED", "ENTRY"]}},
                "required": ["Kind", "Level", "Path", "Section"]}},
        },
        "required": ["Entries", "Files", "LegacyProtocol", "Protocol", "Schema", "Surfaces"],
    }
    if extra_top:
        s.update(extra_top)
    return s


SCHEMA_TEXT = json.dumps(schema_obj(), indent=2, ensure_ascii=False) + "\n"
SCHEMA_UNSUPPORTED_TEXT = json.dumps(schema_obj({"oneOf": [{"type": "object"}]}), indent=2) + "\n"
HANDOFF_SCHEMA_TEXT = json.dumps({
    "$schema": "https://json-schema.org/draft/2020-12/schema",
    "title": "rackcad-worker-handoff/v1",
    "type": "object",
    "required": ["Schema", "TaskId", "RunId", "DelegationRunId", "Attempt", "BaseSha", "RedSha",
                 "CurrentSha", "Branch", "Worktree", "WorkCompleted", "Evidence", "Worker"],
}, indent=2) + "\n"

PRE_GOOD_MSG = ("Merge norm: activaci" + O + "n de prueba\n\nTabla PRE derivada:\n"
                "| Initiative | ref | Claim-Id | transition |\n"
                "| I-61 | refs/heads/a | C-0001 | ANTERIOR |\n"
                "| I-70 | refs/heads/b | C-0002 | ANTERIOR |\n\nFin del cuerpo.\n")
PRE_DEFECT_MSG = ("Merge norm-d: activaci" + O + "n defectuosa\n\n"
                  "| Initiative | ref | Claim-Id | transition |\n"
                  "|---|---|---|---|\n"
                  "| I-61 | refs/heads/a | C-0001 | ANTERIOR |\n"
                  "| I-70 | refs/heads/b | C-0002 |\n"
                  "| I-71 | refs/heads/c | C-0002 | ANTERIOR | extra |\n"
                  "texto sin barra\n"
                  "| I-72 | x | C-0009 | y |\n")

CORPUS = []
STATE = {}


def gitblob(text):
    data = text.encode("utf-8") if isinstance(text, str) else text
    return hashlib.sha1(b"blob " + str(len(data)).encode("ascii") + b"\0" + data).hexdigest()


def sha256_of(data):
    return hashlib.sha256(data).hexdigest()


def canon(obj):
    return json.dumps(obj, ensure_ascii=True, sort_keys=True, separators=(",", ":"))


def heading_lines(text, heading):
    return [i + 1 for i, line in enumerate(text.split("\n")) if line == heading]


def FILE(path, kind, base, eff):
    return {"Path": path, "FileKind": kind, "BaseBlob": base, "EffBlob": eff}


def ENT(path, section, level, kind):
    return {"Path": path, "Section": section, "Level": level, "Kind": kind}


def map_header(surfaces):
    return {"Schema": "rackcad-clause-map/v1", "Protocol": "rackcad-protocol/I62",
            "LegacyProtocol": "rackcad-protocol/I61", "Surfaces": surfaces}


def map_good():
    m = map_header(list(SURF))
    m["Files"] = [FILE(AP, "MODIFIED", gitblob(AP_V1), gitblob(AP_V2)),
                  FILE(WF, "MODIFIED", gitblob(WF_V1), gitblob(WF_V2)),
                  FILE("docs/adr/0002.md", "ADDED", None, gitblob(ADR2)),
                  FILE(SCHEMA, "ENTRY", None, gitblob(SCHEMA_TEXT))]
    m["Entries"] = [ENT(AP, HPLAN, 1, "MODIFIED"), ENT(AP, H16, 2, "MODIFIED"), ENT(AP, H163, 3, "MODIFIED"),
                    ENT(AP, H1610, 3, "ADDED"), ENT(AP, R163, 3, "ENTRY"),
                    ENT(WF, HWF, 1, "MODIFIED"), ENT(WF, "## 10. Autoridad", 2, "MODIFIED"),
                    ENT(WF, ENTRY, 2, "ENTRY")]
    return m


def map_defect():
    m = map_header(list(SURF) + ["CLAUDE.md"])
    m["Files"] = [FILE(AP, "MODIFIED", gitblob(WF_V1), gitblob(AP_D)),
                  FILE(WF, "MODIFIED", gitblob(WF_V1), gitblob(WF_D)),
                  FILE(EXISTING, "ADDED", None, gitblob(EXISTING_V2)),
                  FILE("docs/adr/0003.md", "ENTRY", None, gitblob(ADR3)),
                  FILE(README, "ENTRY", None, gitblob(README_V2)),
                  FILE(SCHEMA, "ENTRY", None, gitblob(SCHEMA_TEXT)),
                  FILE("docs/adr/0002.md", "ADDED", None, gitblob(ADR2)),
                  FILE("docs/adr/0002.md", "ADDED", None, gitblob(ADR2)),
                  FILE("AGENTS.md", "MODIFIED", gitblob(AGENTS), gitblob(AGENTS)),
                  FILE("docs/adr/0005.md", "ADDED", None, "1" * 40)]
    m["Entries"] = [ENT(AP, PRE, 0, "MODIFIED"), ENT(AP, HPLAN, 1, "MODIFIED"), ENT(AP, H16, 2, "MODIFIED"),
                    ENT(AP, H163, 3, "MODIFIED"), ENT(AP, "## 17. Final", 2, "MODIFIED"),
                    ENT(AP, H1610, 3, "ADDED"), ENT(AP, R163, 3, "ENTRY"),
                    ENT(AP, "### 16.1 Participantes", 3, "ENTRY"),
                    ENT(WF, HWF, 1, "MODIFIED"), ENT(WF, "## 10. Autoridad", 2, "MODIFIED"),
                    ENT(WF, "## 10.  Autoridad", 2, "MODIFIED"), ENT(WF, ENTRY, 2, "ENTRY"),
                    ENT("docs/adr/0002.md", "# ADR 2", 1, "ADDED")]
    return m


def map_schema_bad():
    return {"Schema": "rackcad-clause-map/v2", "Protocol": "rackcad-protocol/I62",
            "Surfaces": ["AGENTS.md", "AGENTS.md"],
            "Files": [{"Path": "", "FileKind": "OTHER", "BaseBlob": "XYZ", "EffBlob": None},
                      {"Path": WF, "FileKind": "MODIFIED", "BaseBlob": None, "EffBlob": gitblob(WF_V2) + "\n"}],
            "Entries": [{"Path": AP, "Section": "x", "Level": 9, "Kind": "MODIFIED", "Extra": True}]}


# ------------------------------------------------------------- repo helpers

def rmtree_force(path):
    def on_exc(func, p, exc):
        os.chmod(p, stat.S_IWRITE)
        func(p)
    if sys.version_info >= (3, 12):
        shutil.rmtree(path, onexc=on_exc)
    else:
        shutil.rmtree(path, onerror=lambda f, p, e: on_exc(f, p, e))


def base_git_env():
    env = dict((k, v) for k, v in os.environ.items() if not k.upper().startswith("GIT_"))
    env.update({"GIT_CONFIG_NOSYSTEM": "1", "GIT_CONFIG_GLOBAL": STATE["empty_cfg"],
                "GIT_TERMINAL_PROMPT": "0", "GIT_OPTIONAL_LOCKS": "0", "LC_ALL": "C",
                "GIT_AUTHOR_NAME": "Autora Prueba", "GIT_AUTHOR_EMAIL": "autora@example.invalid",
                "GIT_COMMITTER_NAME": "Committer Prueba", "GIT_COMMITTER_EMAIL": "committer@example.invalid"})
    return env


class Repo(object):
    def __init__(self, path):
        self.path = path
        self.tick = 0
        os.makedirs(path)
        self.git("init", "-q", "-b", "main")

    def git(self, *args, **kw):
        env = base_git_env()
        if kw.get("dated"):
            self.tick += 1
            date = "2026-01-01T%02d:%02d:00+00:00" % divmod(self.tick, 60)
            env["GIT_AUTHOR_DATE"] = date
            env["GIT_COMMITTER_DATE"] = date
        p = subprocess.run(["git"] + list(args), cwd=self.path, env=env, input=kw.get("input"),
                           stdout=subprocess.PIPE, stderr=subprocess.PIPE)
        if p.returncode != 0:
            raise AssertionError("git %r rc %d: %s" % (args, p.returncode, p.stderr.decode("utf-8", "replace")))
        return p.stdout.decode("utf-8")

    def write(self, rel, content):
        data = content.encode("utf-8") if isinstance(content, str) else content
        full = os.path.join(self.path, *rel.split("/"))
        os.makedirs(os.path.dirname(full), exist_ok=True)
        with open(full, "wb") as fh:
            fh.write(data)
        CORPUS.append(data.decode("utf-8", "replace"))

    def delete(self, rel):
        os.remove(os.path.join(self.path, *rel.split("/")))

    def head(self):
        return self.git("rev-parse", "HEAD").strip()

    def commit(self, msg):
        CORPUS.append(msg)
        self.git("add", "-A")
        self.git("commit", "-q", "-F", "-", input=msg.encode("utf-8"), dated=True)
        return self.head()

    def merge(self, branch, msg):
        CORPUS.append(msg)
        msgfile = os.path.join(STATE["work"], "merge-msg.txt")
        with open(msgfile, "wb") as fh:
            fh.write(msg.encode("utf-8"))
        self.git("merge", "--no-ff", "-q", "-F", msgfile, branch, dated=True)
        return self.head()

    def checkout(self, *args):
        self.git("checkout", "-q", *args)


class FastRepo(object):
    """Builds a repository with one deterministic `git fast-import` stream.

    Every commit carries the full tree (deleteall + M lines), fixed author and
    committer identities and strictly increasing timestamps. Merges are --no-ff
    merges of a branch that forked from the current tip, so the merged tree is
    the tree of the merged branch.
    """

    def __init__(self, path):
        self.path = path
        os.makedirs(path)
        p = subprocess.run(["git", "init", "-q", "-b", "main"], cwd=path, env=base_git_env(),
                           stdout=subprocess.PIPE, stderr=subprocess.PIPE)
        assert p.returncode == 0, p.stderr
        self.out = []
        self.mark = 0
        self.snap = {}
        self.trees = {"main": {}}
        self.tips = {}
        self.branch = "main"
        self.tick = 0

    def checkout(self, branch, start=None):
        if start is not None:
            self.trees[branch] = dict(self.snap[start])
            self.tips[branch] = start
        self.branch = branch

    def write(self, rel, content):
        data = content.encode("utf-8") if isinstance(content, str) else content
        self.trees[self.branch][rel] = data
        CORPUS.append(data.decode("utf-8", "replace"))

    def delete(self, rel):
        del self.trees[self.branch][rel]

    def commit(self, msg, merge=None):
        CORPUS.append(msg)
        self.mark += 1
        self.tick += 1
        ts = 1767225600 + 60 * self.tick
        if merge is not None:
            self.trees[self.branch] = dict(self.trees[merge])
        tree = self.trees[self.branch]
        body = msg.encode("utf-8")
        parts = [b"commit refs/heads/" + self.branch.encode("ascii") + b"\n", b"mark :%d\n" % self.mark,
                 b"author Autora Prueba <autora@example.invalid> %d +0000\n" % ts,
                 b"committer Committer Prueba <committer@example.invalid> %d +0000\n" % ts,
                 b"data %d\n" % len(body), body, b"\n"]
        if self.branch in self.tips:
            parts.append(b"from :%d\n" % self.tips[self.branch])
        if merge is not None:
            parts.append(b"merge :%d\n" % self.tips[merge])
        parts.append(b"deleteall\n")
        for path in sorted(tree):
            data = tree[path]
            parts += [b"M 100644 inline " + path.encode("utf-8") + b"\n", b"data %d\n" % len(data), data, b"\n"]
        parts.append(b"\n")
        self.out += parts
        self.tips[self.branch] = self.mark
        self.snap[self.mark] = dict(tree)
        return self.mark

    def merge(self, branch, msg):
        return self.commit(msg, merge=branch)

    def ref(self, refname, mark):
        self.out.append(b"reset " + refname.encode("ascii") + b"\nfrom :%d\n\n" % mark)

    def finish(self, marks, head_branch):
        marks_file = os.path.join(STATE["work"], "marks-" + os.path.basename(self.path) + ".txt")
        p = subprocess.run(["git", "fast-import", "--quiet", "--done", "--export-marks=" + marks_file],
                           cwd=self.path, env=base_git_env(), input=b"".join(self.out) + b"done\n",
                           stdout=subprocess.PIPE, stderr=subprocess.PIPE)
        assert p.returncode == 0, p.stderr
        shas = {}
        with open(marks_file, "r") as fh:
            for line in fh.read().splitlines():
                mark, sha = line.split(" ")
                shas[int(mark[1:])] = sha
        p = subprocess.run(["git", "checkout", "-q", "-f", head_branch], cwd=self.path, env=base_git_env(),
                           stdout=subprocess.PIPE, stderr=subprocess.PIPE)
        assert p.returncode == 0, p.stderr
        return dict((name, shas[mark]) for name, mark in marks.items())


def write_input(name, content):
    data = content.encode("utf-8") if isinstance(content, str) else content
    path = os.path.join(STATE["work"], "inputs", name)
    os.makedirs(os.path.dirname(path), exist_ok=True)
    with open(path, "wb") as fh:
        fh.write(data)
    CORPUS.append(data.decode("utf-8", "replace"))
    return path, sha256_of(data)


def snapshot(repo_path):
    env = base_git_env()
    index = os.path.join(repo_path, ".git", "index")
    idx = None
    if os.path.exists(index):
        with open(index, "rb") as fh:
            idx = sha256_of(fh.read())
    p_refs = subprocess.Popen(["git", "for-each-ref"], cwd=repo_path, env=env, stdout=subprocess.PIPE)
    p_st = subprocess.Popen(["git", "status", "--porcelain", "--ignored"], cwd=repo_path, env=env,
                            stdout=subprocess.PIPE)
    refs = p_refs.communicate()[0]
    st = p_st.communicate()[0]
    assert p_refs.returncode == 0 and p_st.returncode == 0
    files = []
    for dirpath, dirnames, filenames in os.walk(repo_path):
        dirnames.sort()
        for fn in sorted(filenames):
            full = os.path.join(dirpath, fn)
            info = os.stat(full)
            files.append((os.path.relpath(full, repo_path), info.st_size, info.st_mtime_ns))
    return {"index": idx, "refs": refs, "status": st, "files": files}


# ------------------------------------------------------------- fixture build

def build_basic(root):
    r = FastRepo(os.path.join(root, "basic"))
    s = {}
    r.write("README.md", README_BASIC)
    r.write("src/a.cs", "class A {}\n")
    r.write("docs/x.md", "x\n")
    r.write("data/conf.json", json.dumps({"a": {"b/c": 1, "t~x": [10, 20, {"k": NT + "and" + U_}]}, "n": None}))
    r.write("crlf.txt", ("uno\r\ndos " + O + "\r\ntres\r\n").encode("utf-8"))
    r.write(AP, AP_V2)
    r.write(".gitignore", "out/\n")
    s["c1"] = r.commit("c1: base")
    r.write("src/a.cs", "class A { int x; }\n")
    r.write("src/b.cs", "class B {}\n")
    r.write(HSCHEMA, HANDOFF_SCHEMA_TEXT)
    s["c2"] = r.commit("c2: cambios\n\nCo-Authored-By: Jos" + E + " " + NTC + "and" + U_ + " <jose@example.invalid>\n")
    r.delete("docs/x.md")
    r.write("docs/" + NT + ".md", "e" + NT + "e\n")
    r.write("src/c.cs", r.trees[r.branch].pop("src/b.cs"))
    s["c3"] = r.commit("c3: renombre\n\nCuerpo.\n\nCo-Authored-By: A Uno <a@example.invalid>\n"
                       "Co-Authored-By: B Dos <b@example.invalid>\n")
    r.checkout("side", s["c1"])
    r.write("side.txt", "s\n")
    s["s1"] = r.commit("s1: lateral")
    r.ref("refs/heads/feat", s["c3"])
    r.ref("refs/remotes/origin/feat", s["c3"])
    r.ref("refs/remotes/origin/main", s["c2"])
    return r, r.finish(s, "feat")


def build_worktree(root):
    r = Repo(os.path.join(root, "wt"))
    r.write("a.txt", "alfa " + O + "\n")
    r.write("b.txt", "beta\n")
    r.write("dir/c.txt", "c\n")
    r.write(".gitignore", "out/\n")
    sha = r.commit("h1: base")
    r.write("a.txt", "alfa modificada\n")
    r.write("u.txt", "sin seguimiento\n")
    r.write("out/x.bin", b"\x00\x01")
    return r, {"h1": sha}


def build_clausemap(root):
    r = FastRepo(os.path.join(root, "cm"))
    s = {}
    for path, content in ((("AGENTS.md", AGENTS), (AP, AP_V1), (WF, WF_V1), ("docs/adr/0001.md", "# ADR 1\n"),
                           (README, README_V1), (EXISTING, EXISTING_V1), ("src/app.cs", "class App {}\n"))):
        r.write(path, content)
    s["C0"] = r.commit("base: autoridades I-61")
    r.checkout("x1", s["C0"])
    r.write("x1.txt", "x1\n")
    s["X1"] = r.commit("x1: rama lateral")
    r.checkout("main")
    s["M0"] = r.merge("x1", "Merge x1")
    r.write("notes/d.txt", "d\n")
    s["D"] = r.commit("decoy: cita del trailer\n\nAgent-Protocol-Normative: I-62-draft\n"
                      "Ver: Agent-Protocol-Normative: I-62\n")

    def activation(name, files, deletes=(), trailers=1, pre_msg=None):
        r.checkout("norm-" + name, s["D"])
        for path, content in files:
            r.write(path, content)
        for path in deletes:
            r.delete(path)
        s["N1-" + name] = r.commit("I-62: cambios normativos (" + name + ")")
        for t in range(trailers):
            r.write("notes/n-%s-%d.txt" % (name, t), "n\n")
            s["N2-" + name] = r.commit("I-62: activaci" + O + "n\n\nCuerpo.\n\n" + TRAILER
                                       + "\nCo-Authored-By: Claude Prueba <noreply@example.invalid>\n")
        r.checkout("main-" + name, s["D"])
        s["M1-" + name] = r.merge("norm-" + name, pre_msg or ("Merge norm-" + name))

    good_files = [(AP, AP_V2), (WF, WF_V2), ("docs/adr/0002.md", ADR2), (SCHEMA, SCHEMA_TEXT),
                  ("src/app.cs", "class App { int x; }\n"),
                  (MAP, json.dumps(map_good(), indent=2, ensure_ascii=False) + "\n")]
    activation("good", good_files, pre_msg=PRE_GOOD_MSG)
    r.checkout("y", s["M1-good"])
    r.write("y.txt", "y\n")
    s["Y1"] = r.commit("y: lateral")
    r.checkout("main-good")
    s["M2"] = r.merge("y", "Merge y")
    r.write("z.txt", "z\n")
    s["Z"] = r.commit("post: directo")
    r.checkout("unit", s["Z"])
    r.write(AP, AP_V2.replace("Fin.\n", "Fin. Nota de unidad.\n"))
    r.write("docs/initiatives/I-99.md", "# I-99\n\nDocumento de unidad.\n")
    # Like a real /v2 state: automation_state.claim_id and protocol.basis.claim_id, same value.
    r.write("docs/automation/state/U-99.yml",
            "automation_state:\n  claim_id: C-0002\nprotocol:\n  basis:\n    claim_id: C-0002\n")
    s["U1"] = r.commit("U-99: autoridad de unidad")
    r.write("src/app.cs", "class App { int y; }\n")
    s["U2"] = r.commit("U-99: trabajo")
    r.write("docs/automation/state/U-99.yml", "unit: U-99\nclaim_id: C-0001\nbasis:\n  claim_id: C-0002\n")
    s["U3"] = r.commit("U-99: estado con dos claim_id")

    defect_files = [(AP, AP_D), (WF, WF_D), ("docs/adr/0002.md", ADR2), ("docs/adr/0003.md", ADR3),
                    ("docs/adr/0004.md", ADR4), (README, README_V2), (EXISTING, EXISTING_V2),
                    (SCHEMA, SCHEMA_TEXT), (MAP, json.dumps(map_defect(), indent=2, ensure_ascii=False) + "\n")]
    activation("defect", defect_files, deletes=("docs/adr/0001.md",), pre_msg=PRE_DEFECT_MSG)
    activation("schemabad", [(AP, AP_V2), (WF, WF_V2), (SCHEMA, SCHEMA_TEXT),
                             (MAP, json.dumps(map_schema_bad(), indent=2) + "\n")])
    activation("unsupported", [(AP, AP_V2), (WF, WF_V2), (SCHEMA, SCHEMA_UNSUPPORTED_TEXT),
                               (MAP, json.dumps(map_good(), indent=2) + "\n")])
    activation("absent", [(AP, AP_V2), (WF, WF_V2), (SCHEMA, SCHEMA_TEXT)])
    activation("duptrailer", [(AP, AP_V2), (WF, WF_V2)], trailers=2)
    # EFF ambiguous: the unique trailer commit sits on the first-parent chain itself.
    r.checkout("main-effamb", s["D"])
    r.write("notes/e.txt", "e\n")
    s["E1"] = r.commit("I-62: directo en main\n\n" + TRAILER + "\n")
    r.checkout("e2", s["E1"])
    r.write("e2.txt", "e2\n")
    r.commit("e2: lateral")
    r.checkout("main-effamb")
    s["ME"] = r.merge("e2", "Merge e2")
    return r, r.finish(s, "main")


def setUpModule():
    os.makedirs(TMP_ROOT, exist_ok=True)
    work = tempfile.mkdtemp(prefix="suite-", dir=TMP_ROOT)
    STATE["work"] = work
    STATE["empty_cfg"] = os.path.join(work, "empty-gitconfig")
    open(STATE["empty_cfg"], "wb").close()
    STATE["basic"], STATE["b"] = build_basic(work)
    STATE["wt"], STATE["w"] = build_worktree(work)
    STATE["cm"], STATE["c"] = build_clausemap(work)


def tearDownModule():
    if STATE.get("work") and os.path.isdir(STATE["work"]):
        rmtree_force(STATE["work"])


# --------------------------------------------------------------- tool runner

def run_tool_raw(args, isolated=True, stdin=None, script=None):
    cmd = [PY] + (["-I"] if isolated else []) + [script or TOOL] + list(args)
    p = subprocess.run(cmd, input=stdin, stdout=subprocess.PIPE, stderr=subprocess.PIPE, cwd=STATE["work"])
    return p.returncode, p.stdout, p.stderr


def walk_strings(obj):
    if isinstance(obj, dict):
        for k, v in obj.items():
            yield k
            for x in walk_strings(v):
                yield x
    elif isinstance(obj, list):
        for v in obj:
            for x in walk_strings(v):
                yield x
    elif isinstance(obj, str):
        yield obj


class ToolCase(unittest.TestCase):
    maxDiff = None

    def check_envelope(self, rc, out, err):
        self.assertTrue(out.endswith(b"\n"), "stdout must end with a newline; stderr=%r" % err[-400:])
        self.assertEqual(out.count(b"\n"), 1, "exactly one line on stdout")
        self.assertTrue(all(b < 0x80 for b in out), "all stdout bytes must be < 0x80")
        env = json.loads(out.decode("ascii"))
        self.assertEqual(set(env), ENVELOPE_KEYS)
        self.assertEqual(canon(env).encode("ascii") + b"\n", out, "canonical serialization")
        self.assertEqual(env["Schema"], "rackcad-bmin-facts/0-experimental")
        self.assertEqual(env["Runtime"]["Python"], platform.python_version())
        self.assertIsInstance(env["Runtime"]["Isolated"], bool)
        if env["Facts"] is None:
            self.assertIsNone(env["FactsSha256"])
            self.assertIsNotNone(env["Error"])
            self.assertEqual(rc, 64 if env["Error"]["Code"] == "USAGE" else 2)
        else:
            self.assertIsNone(env["Error"])
            self.assertEqual(rc, 0)
            self.assertEqual(env["FactsSha256"], sha256_of(canon(env["Facts"]).encode("ascii")))
        for call in env["GitCalls"]:
            self.assertEqual(call[:4], FORCED)
            self.assertIn(call[4], ALLOWED_SUBS)
            if call[4] in ("diff", "show"):
                self.assertEqual(call[5:7], ["--no-ext-diff", "--no-textconv"])
            if call[4] == "status":
                self.assertIn("--porcelain", call)
        for item in env["Inputs"]:
            kind = item.get("Kind")
            self.assertIn(kind, ("commit", "git-blob", "file"))
            keys = {"commit": {"Kind", "Sha"}, "git-blob": {"Kind", "Rev", "Path", "Blob"},
                    "file": {"Kind", "Name", "Sha256"}}[kind]
            self.assertEqual(set(item), keys)
        return env

    def check_vocabulary(self, facts):
        corpus = "\n".join(CORPUS)
        for s in walk_strings(facts):
            for word in FORBIDDEN_WORDS:
                if word in s:
                    self.assertIn(s, corpus, "forbidden word %r in a Facts string not echoed from input" % word)

    def run_env(self, args, repo=None, **kw):
        repo = repo or STATE["basic"].path
        before = snapshot(repo)
        rc, out, err = run_tool_raw(args, **kw)
        after = snapshot(repo)
        self.assertEqual(before, after, "zero writes: repository changed during %r" % (args[:1],))
        env = self.check_envelope(rc, out, err)
        return rc, env

    def facts(self, args, repo=None):
        rc, env = self.run_env(args, repo)
        self.assertEqual(rc, 0, "tool error: %r" % (env["Error"],))
        self.check_vocabulary(env["Facts"])
        return env["Facts"]

    def error(self, args, code, repo=None, **kw):
        rc, env = self.run_env(args, repo, **kw)
        self.assertIsNone(env["Facts"])
        self.assertIsNone(env["FactsSha256"])
        self.assertEqual(env["Error"]["Code"], code, env["Error"])
        self.assertEqual(rc, 64 if code == "USAGE" else 2)
        return env


def B():
    return STATE["b"]


def C():
    return STATE["c"]


def basic():
    return STATE["basic"].path


def cmrepo():
    return STATE["cm"].path


def load_tool_module():
    spec = importlib.util.spec_from_file_location("bmin_facts_under_test", TOOL)
    mod = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(mod)
    return mod


# ===================================================================== tests

class TestEnvelopeAndRuntime(ToolCase):
    def test_not_isolated_is_refused(self):
        rc, env = self.run_env(["diff", "--repo", basic(), "--from", B()["c1"], "--to", B()["c2"]],
                               isolated=False)
        self.assertEqual(rc, 2)
        self.assertEqual(env["Error"]["Code"], "NOT_ISOLATED")
        self.assertIsNone(env["Facts"])
        self.assertFalse(env["Runtime"]["Isolated"])
        self.assertEqual(env["GitCalls"], [])

    def test_isolated_flag_reported(self):
        rc, env = self.run_env(["diff", "--repo", basic(), "--from", B()["c1"], "--to", B()["c2"]])
        self.assertTrue(env["Runtime"]["Isolated"])

    def test_usage_errors_exit_64(self):
        self.error(["nonsense", "--repo", basic()], "USAGE")
        self.error(["diff", "--repo", basic(), "--from", B()["c1"]], "USAGE")
        self.error(["diff", "--repo", basic(), "--from", B()["c1"], "--to", B()["c2"], "--bogus", "x"], "USAGE")
        self.error(["read-text", "--repo", basic(), "--rev", B()["c1"], "--path", "src\\a.cs"], "USAGE")
        self.error(["read-text", "--repo", basic(), "--rev", B()["c1"], "--path", "README.md", "--lines", "3-2"],
                   "USAGE")
        path, sha = write_input("u.json", "{}")
        self.error(["scope", "--repo", basic(), "--from", B()["c1"], "--to", B()["c2"], "--file", "DELEG=" + path],
                   "USAGE")
        self.error(["scope", "--repo", basic(), "--from", B()["c1"], "--to", B()["c2"], "--file", "X=" + path,
                    "--sha256", "X=" + sha], "USAGE")

    def test_stdin_execution_from_git_object(self):
        scratch = Repo(os.path.join(STATE["work"], "toolobj"))
        blob = scratch.git("hash-object", "-w", TOOL).strip()
        with open(TOOL, "rb") as fh:
            tool_bytes = fh.read()
        self.assertEqual(blob, gitblob(tool_bytes))
        p = subprocess.run(["git", "cat-file", "blob", blob], cwd=scratch.path, env=base_git_env(),
                           stdout=subprocess.PIPE)
        self.assertEqual(p.stdout, tool_bytes)
        args = ["diff", "--repo", basic(), "--from", B()["c1"], "--to", B()["c3"]]
        rc1, out1, _ = run_tool_raw(args)
        rc2, out2, err2 = run_tool_raw(args, stdin=p.stdout, script="-")
        self.assertEqual(rc1, 0)
        self.assertEqual(rc2, 0, err2)
        self.assertEqual(out1, out2)

    def test_determinism_byte_identical(self):
        c = C()
        cases = [
            (["clausemap", "--repo", cmrepo(), "--main", c["Z"], "--state-rev", c["U1"],
              "--state-path", "docs/automation/state/U-99.yml"], cmrepo()),
            (["clausemap", "--repo", cmrepo(), "--main", c["M1-defect"]], cmrepo()),
            (["read-text", "--repo", basic(), "--rev", B()["c1"], "--path", "README.md"], basic()),
            (["diff", "--repo", basic(), "--from", B()["c1"], "--to", B()["c3"]], basic()),
        ]
        for args, repo in cases:
            outs = []
            for _ in range(2):
                rc, out, err = run_tool_raw(args)
                self.check_envelope(rc, out, err)
                outs.append(out)
            self.assertEqual(outs[0], outs[1])
            facts = [canon(json.loads(o.decode("ascii"))["Facts"]) for o in outs]
            self.assertEqual(facts[0], facts[1])


class TestIdentity(ToolCase):
    def test_identity_facts(self):
        b = B()
        f = self.facts(["identity", "--repo", basic(), "--branch", "feat", "--base", b["c1"], "--current", b["c3"],
                        "--red", b["c2"]])
        self.assertEqual(f["Head"], b["c3"])
        self.assertEqual(f["AbbrevRef"], "feat")
        self.assertEqual(f["Toplevel"], basic().replace("\\", "/"))
        self.assertEqual(f["OriginBranch"], b["c3"])
        self.assertEqual(f["OriginMain"], b["c2"])
        self.assertEqual(f["Ancestry"], {"base->current": True, "base->red": True, "red->current": True})

    def test_identity_non_ancestor_and_missing_origin(self):
        b = B()
        f = self.facts(["identity", "--repo", basic(), "--branch", "nope", "--base", b["s1"], "--current", b["c3"],
                        "--red", b["c3"]])
        self.assertIsNone(f["OriginBranch"])
        self.assertEqual(f["Ancestry"], {"base->current": False, "base->red": False, "red->current": True})
        f = self.facts(["identity", "--repo", basic(), "--branch", "feat", "--base", b["c3"], "--current", b["c1"]])
        self.assertEqual(f["Ancestry"], {"base->current": False})


class TestDiff(ToolCase):
    def test_paths_commits_coauthors(self):
        b = B()
        f = self.facts(["diff", "--repo", basic(), "--from", b["c1"], "--to", b["c3"]])
        self.assertEqual(f["Paths"], [HSCHEMA, "docs/x.md", "docs/" + NT + ".md", "src/a.cs", "src/c.cs"])
        self.assertEqual(f["Commits"], [b["c2"], b["c3"]])
        self.assertEqual(f["CoAuthoredBy"], {
            b["c2"]: ["Jos" + E + " " + NTC + "and" + U_ + " <jose@example.invalid>"],
            b["c3"]: ["A Uno <a@example.invalid>", "B Dos <b@example.invalid>"]})

    def test_rename_reports_both_paths(self):
        b = B()
        f = self.facts(["diff", "--repo", basic(), "--from", b["c2"], "--to", b["c3"]])
        self.assertEqual(f["Paths"], ["docs/x.md", "docs/" + NT + ".md", "src/b.cs", "src/c.cs"])
        self.assertEqual(f["Commits"], [b["c3"]])


class TestScope(ToolCase):
    def test_scope_coverage(self):
        b = B()
        deleg = {"AllowedWriteScope": ["src/", "docs/" + NT + ".md", "tests/**", "/abs/", "docs/../x", "a\\b",
                                       "docs/*.md", "docs/x.md"],
                 "ForbiddenWriteScope": ["src/c.cs", "docs/**"]}
        path, sha = write_input("deleg-scope.json", json.dumps(deleg, ensure_ascii=False))
        f = self.facts(["scope", "--repo", basic(), "--from", b["c1"], "--to", b["c3"],
                        "--file", "DELEG=" + path, "--sha256", "DELEG=" + sha])
        self.assertEqual(f["Paths"], [HSCHEMA, "docs/x.md", "docs/" + NT + ".md", "src/a.cs", "src/c.cs"])
        self.assertEqual(f["PerPath"], [
            {"Path": HSCHEMA, "AllowedCoveredBy": [], "ForbiddenCoveredBy": ["docs/**"]},
            {"Path": "docs/x.md", "AllowedCoveredBy": ["docs/x.md"], "ForbiddenCoveredBy": ["docs/**"]},
            {"Path": "docs/" + NT + ".md", "AllowedCoveredBy": ["docs/" + NT + ".md"],
             "ForbiddenCoveredBy": ["docs/**"]},
            {"Path": "src/a.cs", "AllowedCoveredBy": ["src/"], "ForbiddenCoveredBy": []},
            {"Path": "src/c.cs", "AllowedCoveredBy": ["src/"], "ForbiddenCoveredBy": ["src/c.cs"]}])
        self.assertEqual(f["AllowedEntries"], deleg["AllowedWriteScope"])
        self.assertEqual(f["ForbiddenEntries"], deleg["ForbiddenWriteScope"])
        self.assertEqual(f["MalformedEntries"], [
            {"List": "AllowedWriteScope", "Entry": "/abs/", "Reasons": ["absolute"]},
            {"List": "AllowedWriteScope", "Entry": "docs/../x", "Reasons": ["dotdot"]},
            {"List": "AllowedWriteScope", "Entry": "a\\b", "Reasons": ["backslash"]},
            {"List": "AllowedWriteScope", "Entry": "docs/*.md", "Reasons": ["wildcard"]}])
        self.assertEqual(f["U19Normalized"], [
            {"List": "AllowedWriteScope", "Entry": "tests/**", "Prefix": "tests/"},
            {"List": "ForbiddenWriteScope", "Entry": "docs/**", "Prefix": "docs/"}])

    def test_scope_case_sensitive(self):
        b = B()
        path, sha = write_input("deleg-case.json", json.dumps({"AllowedWriteScope": ["SRC/", "Docs/x.md"],
                                                               "ForbiddenWriteScope": []}))
        f = self.facts(["scope", "--repo", basic(), "--from", b["c1"], "--to", b["c2"],
                        "--file", "DELEG=" + path, "--sha256", "DELEG=" + sha])
        self.assertTrue(all(row["AllowedCoveredBy"] == [] for row in f["PerPath"]))


def contract_obj(**over):
    c = {"Schema": "rackcad-gate-contract/v2", "Unit": "U-99", "Gate": "G2", "TaskId": "T1", "Objective": "obj",
         "AuthorityRevision": "a" * 40, "MainSha": "b" * 40,
         "Authorities": [{"Path": "a.md", "Section": "## 1. Uno", "Class": "EXTERNAL"},
                         {"Path": "b.md", "Section": "documento completo", "Class": "UNIT_DOC"}],
         "AllowedWriteScope": ["src/", "docs/a.md", "tests/"], "ForbiddenWriteScope": ["docs/secret/", ".github/"],
         "Invariants": ["INV-1", "INV-2"],
         "RequiredTests": [{"Project": "p", "Filter": "F1", "MinSelected": 3, "ExpectRed": True},
                           {"Project": "p", "Filter": "F2", "MinSelected": 1, "ExpectRed": False}],
         "ExpectedEvidence": [], "StopConditions": [{"Id": "S-1", "Text": "t"}, {"Id": "S-2", "Text": "t"}],
         "RoutingEnforcement": "required", "CorrectionsAuthorized": False, "IssuedBy": "x",
         "IssuedUtc": "2026-01-01T00:00:00Z", "RoleRequirements": [{"Role": "WORKER"}, {"Role": "REVIEWER"}],
         "ProtocolSet": "rackcad-protocol/I62", "MaterializationCloseSha": "c" * 40, "SupersededCommits": [],
         "SupersededBaseSha": None}
    c.update(over)
    return c


def deleg_obj(**over):
    d = {"AllowedWriteScope": ["src/core/", "docs/a.md", "docs/b.md", "tests/**"], "ForbiddenWriteScope": ["docs/"],
         "Invariants": ["INV-1"], "StopConditions": [{"Id": "S-1", "Text": "t"}],
         "Authorities": [{"Path": "a.md", "Section": "## 1. Uno", "Class": "EXTERNAL"}],
         "RequiredTests": [{"Project": "p", "Filter": "F1", "MinSelected": 2, "ExpectRed": True}],
         "RoleRequirements": [{"Role": "WORKER"}], "SupersededCommits": []}
    d.update(over)
    return d


class TestContract(ToolCase):
    def run_contract(self, contract, deleg, tag):
        cp, cs = write_input("contract-%s.json" % tag, json.dumps(contract))
        dp, ds = write_input("deleg-%s.json" % tag, json.dumps(deleg))
        return ["contract", "--repo", basic(), "--file", "CONTRACT=" + cp, "--sha256", "CONTRACT=" + cs,
                "--file", "DELEG=" + dp, "--sha256", "DELEG=" + ds]

    def test_contract_facts(self):
        f = self.facts(self.run_contract(contract_obj(), deleg_obj(), "a"))
        self.assertEqual(f["A3"], [{"Entry": "src/core/", "ContainedBy": ["src/"]},
                                   {"Entry": "docs/a.md", "ContainedBy": ["docs/a.md"]},
                                   {"Entry": "docs/b.md", "ContainedBy": []},
                                   {"Entry": "tests/**", "ContainedBy": ["tests/"]}])
        self.assertEqual(f["A4"], [{"Entry": "docs/secret/", "ContainedBy": ["docs/"]},
                                   {"Entry": ".github/", "ContainedBy": []}])
        self.assertEqual(f["A5Missing"], {"Invariants": ["INV-2"], "StopConditions": ["S-2"],
                                          "Authorities": ["b.md|documento completo|UNIT_DOC"],
                                          "RequiredTests": ["F2"], "RoleRequirements": ["REVIEWER"]})
        self.assertEqual(f["RequiredTestsPairs"], [
            {"Filter": "F1", "ContractMinSelected": 3, "DelegMinSelected": 2, "ContractExpectRed": True,
             "DelegExpectRed": True},
            {"Filter": "F2", "ContractMinSelected": 1, "DelegMinSelected": None, "ContractExpectRed": False,
             "DelegExpectRed": None}])
        self.assertIs(f["SupersededEqual"], True)

    def test_contract_superseded_differs(self):
        f = self.facts(self.run_contract(contract_obj(), deleg_obj(SupersededCommits=[{"Sha": "d" * 40,
                                                                                      "WindowSeq": 1}]), "b"))
        self.assertIs(f["SupersededEqual"], False)

    def test_contract_missing_required_field(self):
        c = contract_obj()
        del c["IssuedUtc"]
        self.error(self.run_contract(c, deleg_obj(), "c"), "MISSING_INPUT")

    def test_contract_ambiguous_filter(self):
        d = deleg_obj(RequiredTests=[{"Project": "p", "Filter": "F1", "MinSelected": 2, "ExpectRed": True},
                                     {"Project": "q", "Filter": "F1", "MinSelected": 5, "ExpectRed": True}])
        self.error(self.run_contract(contract_obj(), d, "d"), "AMBIGUOUS")


class TestHashesAndCleanTree(ToolCase):
    def test_hashes(self):
        repo = STATE["wt"].path
        h1 = STATE["w"]["h1"]
        f = self.facts(["hashes", "--repo", repo, "--rev", h1, "--paths", "a.txt", "b.txt", "missing.txt", "dir"],
                       repo)
        a_wt = b"alfa modificada\n"
        a_rev = ("alfa " + O + "\n").encode("utf-8")
        self.assertEqual(f["PerPath"], [
            {"Path": "a.txt", "WorktreeRawBlobSha1": gitblob(a_wt), "WorktreeSha256": sha256_of(a_wt),
             "RevBlob": gitblob(a_rev), "RevBlobSha256": sha256_of(a_rev)},
            {"Path": "b.txt", "WorktreeRawBlobSha1": gitblob(b"beta\n"), "WorktreeSha256": sha256_of(b"beta\n"),
             "RevBlob": gitblob(b"beta\n"), "RevBlobSha256": sha256_of(b"beta\n")},
            {"Path": "missing.txt", "WorktreeRawBlobSha1": None, "WorktreeSha256": None, "RevBlob": None,
             "RevBlobSha256": None},
            {"Path": "dir", "WorktreeRawBlobSha1": None, "WorktreeSha256": None, "RevBlob": None,
             "RevBlobSha256": None}])

    def test_cleantree(self):
        repo = STATE["wt"].path
        f = self.facts(["cleantree", "--repo", repo, "--ignore-path", "out/x.bin"], repo)
        self.assertIn(" M a.txt", f["StatusPorcelain"])
        self.assertIn("?? u.txt", f["StatusPorcelain"])
        self.assertNotIn("!! out/", f["StatusPorcelain"])
        self.assertIn("!! out/", f["StatusIgnored"])
        self.assertEqual(f["CheckIgnoreRc"], 0)
        self.assertEqual(f["CheckIgnoreOutput"], [".gitignore:1:out/\tout/x.bin"])
        f = self.facts(["cleantree", "--repo", repo, "--ignore-path", "b.txt"], repo)
        self.assertEqual(f["CheckIgnoreRc"], 1)
        self.assertEqual(f["CheckIgnoreOutput"], [])


class TestReadText(ToolCase):
    def test_non_ascii_fidelity(self):
        f = self.facts(["read-text", "--repo", basic(), "--rev", B()["c1"], "--path", "README.md"])
        self.assertEqual(f["Blob"], gitblob(README_BASIC))
        self.assertEqual(f["Lines"], [{"N": 1, "Text": "Hola " + LAQ + "mundo" + RAQ + " " + EMD + " prueba " + O}])
        self.assertEqual(f["NonAsciiCount"], 4)
        self.assertEqual(f["NonAsciiChars"], [{"CodePoint": "U+00AB", "Count": 1}, {"CodePoint": "U+00BB", "Count": 1},
                                              {"CodePoint": "U+00F3", "Count": 1}, {"CodePoint": "U+2014", "Count": 1}])

    def test_crlf_and_range(self):
        f = self.facts(["read-text", "--repo", basic(), "--rev", B()["c1"], "--path", "crlf.txt"])
        self.assertEqual([x["Text"] for x in f["Lines"]], ["uno", "dos " + O, "tres"])
        f = self.facts(["read-text", "--repo", basic(), "--rev", B()["c1"], "--path", "crlf.txt", "--lines", "2-3"])
        self.assertEqual(f["Lines"], [{"N": 2, "Text": "dos " + O}, {"N": 3, "Text": "tres"}])
        self.assertEqual(f["NonAsciiCount"], 1)

    def test_missing(self):
        self.error(["read-text", "--repo", basic(), "--rev", B()["c1"], "--path", "nope.txt"], "MISSING_INPUT")
        self.error(["read-text", "--repo", basic(), "--rev", B()["c1"], "--path", "crlf.txt", "--lines", "2-9"],
                   "MISSING_INPUT")


class TestReadJson(ToolCase):
    POINTERS = ["/a/b~1c", "/a/t~0x/2/k", "/n", "/a/zz", "/a/t~0x/3", "/a/t~0x/01"]

    def expected(self):
        return {"Values": {"/a/b~1c": 1, "/a/t~0x/2/k": NT + "and" + U_, "/n": None, "/a/zz": {"Absent": True},
                           "/a/t~0x/3": {"Absent": True}, "/a/t~0x/01": {"Absent": True}},
                "AbsentPointers": sorted(["/a/zz", "/a/t~0x/3", "/a/t~0x/01"])}

    def pointer_args(self):
        out = []
        for p in self.POINTERS:
            out += ["--pointer", p]
        return out

    def test_from_rev(self):
        f = self.facts(["read-json", "--repo", basic(), "--rev", B()["c1"], "--path", "data/conf.json"]
                       + self.pointer_args())
        self.assertEqual(f, self.expected())

    def test_from_file(self):
        path, sha = write_input("conf.json", json.dumps({"a": {"b/c": 1, "t~x": [10, 20, {"k": NT + "and" + U_}]},
                                                         "n": None}, ensure_ascii=False))
        f = self.facts(["read-json", "--repo", basic(), "--file", "N=" + path, "--sha256", "N=" + sha]
                       + self.pointer_args())
        self.assertEqual(f, self.expected())

    def test_malformed_and_duplicate_keys(self):
        for tag, text in (("trailing-comma", '{"a": 1,}'), ("dup", '{"a": 1, "a": 2}'), ("nan", '{"a": NaN}')):
            path, sha = write_input("bad-%s.json" % tag, text)
            self.error(["read-json", "--repo", basic(), "--file", "N=" + path, "--sha256", "N=" + sha,
                        "--pointer", "/a"], "MALFORMED_JSON")


TRX_NS = ('<?xml version="1.0" encoding="utf-8"?>\n'
          '<TestRun id="1" name="r" xmlns="http://microsoft.com/schemas/VisualStudio/TeamTest/2010">\n'
          '  <Results>\n'
          '    <UnitTestResult testId="1" testName="Suite.Alpha_Ok" outcome="Passed" />\n'
          '    <UnitTestResult testId="2" testName="Suite.Beta_Bad" outcome="Failed" />\n'
          '    <UnitTestResult testId="3" testName="Suite.Alpha_Bad" outcome="Failed" />\n'
          '    <UnitTestResult testId="4" testName="Otra.Gamma_' + NT + '" outcome="NotExecuted" />\n'
          '  </Results>\n'
          '  <ResultSummary outcome="Failed">\n'
          '    <Counters total="4" executed="3" passed="1" failed="2" error="0" />\n'
          '  </ResultSummary>\n'
          '</TestRun>\n')
TRX_PLAIN = TRX_NS.replace(' xmlns="http://microsoft.com/schemas/VisualStudio/TeamTest/2010"', "")


class TestTrx(ToolCase):
    def trx_args(self, text, tag, extra=()):
        path, sha = write_input("run-%s.trx" % tag, text)
        return ["trx", "--repo", basic(), "--file", "TRX=" + path, "--sha256", "TRX=" + sha] + list(extra), sha

    def test_trx_facts_namespace_independent(self):
        args, sha = self.trx_args(TRX_NS, "ns", ["--filter-substring", "Alpha"])
        f = self.facts(args)
        self.assertEqual(f["Sha256"], sha)
        self.assertEqual(f["Counters"], {"total": "4", "executed": "3", "passed": "1", "failed": "2", "error": "0"})
        self.assertEqual(f["Tests"], [{"TestName": "Suite.Alpha_Ok", "Outcome": "Passed"},
                                      {"TestName": "Suite.Beta_Bad", "Outcome": "Failed"},
                                      {"TestName": "Suite.Alpha_Bad", "Outcome": "Failed"},
                                      {"TestName": "Otra.Gamma_" + NT, "Outcome": "NotExecuted"}])
        self.assertEqual(f["FailedTests"], ["Suite.Beta_Bad", "Suite.Alpha_Bad"])
        self.assertEqual(f["FilterSelected"], ["Suite.Alpha_Ok", "Suite.Alpha_Bad"])
        self.assertEqual(f["FilterFailed"], ["Suite.Alpha_Bad"])
        self.assertEqual((f["FilterSelectedCount"], f["FilterFailedCount"]), (2, 1))
        args2, _ = self.trx_args(TRX_PLAIN, "plain", ["--filter-substring", "Alpha"])
        f2 = self.facts(args2)
        for key in ("Counters", "Tests", "FailedTests", "FilterSelected", "FilterFailed"):
            self.assertEqual(f[key], f2[key])

    def test_trx_without_filter(self):
        args, _ = self.trx_args(TRX_NS, "nofilter")
        f = self.facts(args)
        self.assertIsNone(f["FilterSelected"])
        self.assertIsNone(f["FilterSelectedCount"])


def handoff_doc():
    return {"Schema": "rackcad-worker-handoff/v1", "TaskId": "T1", "RunId": "R20260101T000000Z-abcd",
            "Attempt": 1, "BaseSha": "a" * 40, "RedSha": None, "CurrentSha": "b" * 40, "Branch": "feat",
            "WorkCompleted": "Hecho " + EMD + " s" + I_, "Evidence": ["e1"], "Worker": {"Provider": "x"}}


class TestHandoff(ToolCase):
    def args(self, path, sha, rev, extra=()):
        return ["handoff", "--repo", basic(), "--file", "H=" + path, "--sha256", "H=" + sha,
                "--schema-rev", rev] + list(extra)

    def test_present_valid(self):
        path, sha = write_input("handoff.json", json.dumps(handoff_doc(), ensure_ascii=False))
        f = self.facts(self.args(path, sha, B()["c2"]))
        self.assertEqual(f["Present"], True)
        self.assertEqual(f["ValidJson"], True)
        self.assertEqual(f["MissingRequiredKeys"], ["DelegationRunId", "Worktree"])
        self.assertEqual(f["AbsentFields"], ["DelegationRunId", "Worktree"])
        self.assertEqual(f["Sha256"], sha)
        self.assertEqual(f["Fields"], {"TaskId": "T1", "Attempt": 1, "DelegationRunId": {"Absent": True},
                                       "RunId": "R20260101T000000Z-abcd", "BaseSha": "a" * 40, "RedSha": None,
                                       "CurrentSha": "b" * 40, "Branch": "feat", "Worktree": {"Absent": True},
                                       "Worker": {"Provider": "x"}})

    def test_invalid_json_is_a_fact(self):
        path, sha = write_input("handoff-bad.json", "{not json")
        f = self.facts(self.args(path, sha, B()["c2"]))
        self.assertEqual(f, {"Present": True, "ValidJson": False, "MissingRequiredKeys": None, "Fields": None,
                             "AbsentFields": None, "Sha256": sha})

    def test_absent(self):
        missing = os.path.join(STATE["work"], "inputs", "no-handoff.json")
        f = self.facts(self.args(missing, "0" * 64, B()["c2"], ["--allow-absent"]))
        self.assertEqual(f, {"Present": False})
        self.error(self.args(missing, "0" * 64, B()["c2"]), "MISSING_INPUT")

    def test_schema_absent_at_rev(self):
        path, sha = write_input("handoff2.json", json.dumps(handoff_doc()))
        self.error(self.args(path, sha, B()["c1"]), "MISSING_INPUT")


class TestFreeText(ToolCase):
    def test_terms_and_matches(self):
        doc = {"WorkCompleted": "Trabajo hecho; el CANDIDATO qued" + O + " listo. candidatos no cuenta.",
               "Evidence": ["Ver rama integrada en main", "RAMA  INTEGRADA"],
               "UnexpectedFindings": [],
               "KnownLimitations": ["gate_cerrado no cuenta", "gate cerrado s" + I_],
               "Deviations": ["Candidate_x no", "x-Candidate"],
               "OpenQuestions": ["Candidato" + O + " no"],
               "RecommendedNextAction": "Ready to merge.",
               "Gate": "Candidato"}
        path, sha = write_input("free.json", json.dumps(doc, ensure_ascii=False))
        f = self.facts(["freetext", "--repo", basic(), "--file", "H=" + path, "--sha256", "H=" + sha,
                        "--terms-rev", B()["c1"]])
        self.assertEqual(f["Terms"], TERMS)
        self.assertEqual(f["Fields"], ["WorkCompleted", "Evidence", "UnexpectedFindings", "KnownLimitations",
                                       "Deviations", "OpenQuestions", "RecommendedNextAction"])
        self.assertEqual(f["Matches"], [
            {"Field": "Deviations", "Term": "Candidate", "Count": 1},
            {"Field": "Evidence", "Term": "integrada en main", "Count": 1},
            {"Field": "Evidence", "Term": "rama integrada", "Count": 2},
            {"Field": "KnownLimitations", "Term": "gate cerrado", "Count": 1},
            {"Field": "RecommendedNextAction", "Term": "ready to merge", "Count": 1},
            {"Field": "WorkCompleted", "Term": "Candidato", "Count": 1}])

    def test_no_terms_paragraph(self):
        doc = dict((k, "") for k in ("WorkCompleted", "RecommendedNextAction"))
        doc.update(dict((k, []) for k in ("Evidence", "UnexpectedFindings", "KnownLimitations", "Deviations",
                                          "OpenQuestions")))
        path, sha = write_input("free-empty.json", json.dumps(doc))
        self.error(["freetext", "--repo", cmrepo(), "--file", "H=" + path, "--sha256", "H=" + sha,
                    "--terms-rev", C()["C0"]], "CONTRADICTORY", repo=cmrepo())


class TestClauseMapGood(ToolCase):
    @classmethod
    def setUpClass(cls):
        c = C()
        contract = {"AuthorityRevision": c["U1"], "MainSha": c["Z"], "Authorities": [
            {"Path": AP, "Section": H163, "Class": "EXTERNAL"},
            {"Path": AP, "Section": H1610, "Class": "EXTERNAL"},
            {"Path": WF, "Section": PRE, "Class": "EXTERNAL"},
            {"Path": "docs/initiatives/I-99.md", "Section": "documento completo", "Class": "UNIT_DOC"},
            {"Path": "AGENTS.md", "Section": "#   Agentes  ", "Class": "EXTERNAL"}]}
        path, sha = write_input("contract-resolve.json", json.dumps(contract, ensure_ascii=False))
        cls.args = ["clausemap", "--repo", cmrepo(), "--main", c["Z"], "--state-rev", c["U1"],
                    "--state-path", "docs/automation/state/U-99.yml", "--base", c["U2"],
                    "--file", "CONTRACT=" + path, "--sha256", "CONTRACT=" + sha]
        cls.cached = None

    def get(self):
        if TestClauseMapGood.cached is None:
            TestClauseMapGood.cached = self.facts(self.args, cmrepo())
        return TestClauseMapGood.cached

    def test_derive(self):
        c = C()
        d = self.get()["Derive"]
        self.assertEqual(d["TrailerCommits"], [c["N2-good"]])
        self.assertEqual(d["FirstParentMerges"], [c["M0"], c["M1-good"], c["M2"]])
        self.assertEqual(d["Candidates"], [c["M1-good"]])
        self.assertEqual((d["Eff"], d["EffParent1"], d["EffParent2"]), (c["M1-good"], c["D"], c["N2-good"]))

    def test_pre_and_classify(self):
        f = self.get()
        self.assertEqual(f["Pre"], {"HeaderFound": True, "HeaderCells": ["Initiative", "ref", "Claim-Id", "transition"],
                                    "Rows": [["I-61", "refs/heads/a", "C-0001", "ANTERIOR"],
                                             ["I-70", "refs/heads/b", "C-0002", "ANTERIOR"]],
                                    "MalformedRows": [], "ClaimIdColumn": 2,
                                    "ClaimIdCounts": {"C-0001": 1, "C-0002": 1}})
        self.assertEqual(f["Classify"], {"ClaimIdLines": ["C-0002", "C-0002"], "ClaimIdInPre": 1})

    def test_entry(self):
        e = self.get()["Entry"]
        self.assertEqual(e, {"WorkflowBlob": gitblob(WF_V2), "ApBlob": gitblob(AP_V2),
                             "EntryMatches": heading_lines(WF_V2, ENTRY), "R163Matches": heading_lines(AP_V2, R163),
                             "EntryNamesR163": True})
        self.assertEqual(len(e["EntryMatches"]), 1)

    def test_validate_mv1_to_mv5(self):
        v = self.get()["Validate"]
        mg = map_good()
        self.assertEqual(v["MV1"], {"MapPresent": True, "MapBlob": gitblob(json.dumps(mg, indent=2, ensure_ascii=False)
                                                                           + "\n"),
                                    "MapParses": True, "SchemaPresent": True, "SchemaBlob": gitblob(SCHEMA_TEXT),
                                    "SchemaViolations": []})
        self.assertEqual(v["MV2"], {"MapSurfaces": SURF, "ClosedList": SURF})
        changed = sorted([AP, WF, "docs/adr/0002.md", SCHEMA])
        self.assertEqual(v["MV3"], {"ChangedUnderSurfaces": changed, "DeletedUnderSurfaces": [],
                                    "FilesPaths": [AP, WF, "docs/adr/0002.md", SCHEMA], "FilesDuplicates": [],
                                    "OnlyInChanged": [], "OnlyInFiles": []})
        self.assertEqual(v["MV4"]["Rows"], [
            {"Path": AP, "FileKind": "MODIFIED", "MapBaseBlob": gitblob(AP_V1), "MapEffBlob": gitblob(AP_V2),
             "ObservedEff1Blob": gitblob(AP_V1), "ObservedEffBlob": gitblob(AP_V2)},
            {"Path": WF, "FileKind": "MODIFIED", "MapBaseBlob": gitblob(WF_V1), "MapEffBlob": gitblob(WF_V2),
             "ObservedEff1Blob": gitblob(WF_V1), "ObservedEffBlob": gitblob(WF_V2)},
            {"Path": "docs/adr/0002.md", "FileKind": "ADDED", "MapBaseBlob": None, "MapEffBlob": gitblob(ADR2),
             "ObservedEff1Blob": None, "ObservedEffBlob": gitblob(ADR2)},
            {"Path": SCHEMA, "FileKind": "ENTRY", "MapBaseBlob": None, "MapEffBlob": gitblob(SCHEMA_TEXT),
             "ObservedEff1Blob": None, "ObservedEffBlob": gitblob(SCHEMA_TEXT)}])
        for key in ("LiteralViolations", "EntryFilesOutsideSchema", "AddedPresentInEff1", "EntryFilePresentInEff1"):
            self.assertEqual(v["MV4"][key], [], key)
        self.assertEqual(v["MV5"], {"EntryKeyDuplicates": [], "EntriesOnNonModifiedPath": []})

    def test_validate_mv6(self):
        v = self.get()["Validate"]["MV6"]
        self.assertEqual(v["PerFile"], [
            {"Path": AP, "H1Duplicates": [], "H2Duplicates": [],
             "DerivedModified": sorted([HPLAN, H16, H163]), "DerivedRemoved": [],
             "DerivedAddedOrEntry": sorted([H1610, R163]),
             "MapModified": sorted([HPLAN, H16, H163]), "MapRemoved": [], "MapAddedOrEntry": sorted([H1610, R163])},
            {"Path": WF, "H1Duplicates": [], "H2Duplicates": [],
             "DerivedModified": sorted([HWF, "## 10. Autoridad"]), "DerivedRemoved": [], "DerivedAddedOrEntry": [ENTRY],
             "MapModified": sorted([HWF, "## 10. Autoridad"]), "MapRemoved": [], "MapAddedOrEntry": [ENTRY]}])
        self.assertEqual(v["EntryKindEntries"], [{"Path": AP, "Section": R163}, {"Path": WF, "Section": ENTRY}])
        self.assertEqual(v["ExpectedEntryKind"], [{"Path": AP, "Section": R163}, {"Path": WF, "Section": ENTRY}])

    def test_validate_mv7_pointer_shares_line(self):
        v = self.get()["Validate"]["MV7"]
        self.assertEqual(v, {"EntryCountEff": 1, "EntryNamesR163Eff": True, "S16Count": 1,
                             "FirstSentenceS16": POINTER, "PointerIsFirstSentenceS16": True,
                             "S163CountEff": 1, "S163CountEff1": 1, "LastSentenceS163": POINTER,
                             "PointerIsLastSentenceS163": True, "S163EffMinusPointer": S163_V1,
                             "S163Eff1": S163_V1, "S163EqualAfterPointerRemoval": True})

    def test_resolve(self):
        c = C()
        r = self.get()["Resolve"]
        self.assertEqual((r["Ar"], r["MainSha"], r["ContractMainSha"], r["Mb"]), (c["U1"], c["Z"], c["Z"], c["Z"]))
        self.assertEqual(r["ArBaseDiff"], ["src/app.cs"])
        self.assertEqual(r["Citations"], [
            {"Path": AP, "Section": H163, "Class": "EXTERNAL", "MapFileKind": "MODIFIED", "MapEntryKinds": ["MODIFIED"],
             "MatchEff1": 1, "MatchEff": 1, "MatchMain": 1, "ExtDiffMbAr": [AP]},
            {"Path": AP, "Section": H1610, "Class": "EXTERNAL", "MapFileKind": "MODIFIED", "MapEntryKinds": ["ADDED"],
             "MatchEff1": 0, "MatchEff": 1, "MatchMain": 1, "ExtDiffMbAr": [AP]},
            {"Path": WF, "Section": PRE, "Class": "EXTERNAL", "MapFileKind": "MODIFIED", "MapEntryKinds": [],
             "MatchEff1": 1, "MatchEff": 1, "MatchMain": 1, "ExtDiffMbAr": []},
            {"Path": "docs/initiatives/I-99.md", "Section": "documento completo", "Class": "UNIT_DOC",
             "MapFileKind": None, "MapEntryKinds": [], "MatchEff1": 0, "MatchEff": 0, "MatchMain": 0,
             "ExtDiffMbAr": None},
            {"Path": "AGENTS.md", "Section": "#   Agentes  ", "Class": "EXTERNAL", "MapFileKind": None,
             "MapEntryKinds": [], "MatchEff1": 1, "MatchEff": 1, "MatchMain": 1, "ExtDiffMbAr": []}])

    def test_inputs_declared(self):
        rc, env = self.run_env(self.args, cmrepo())
        commits = [i["Sha"] for i in env["Inputs"] if i["Kind"] == "commit"]
        for name in ("Z", "U1", "U2"):
            self.assertIn(C()[name], commits)
        blobs = [(i["Rev"], i["Path"]) for i in env["Inputs"] if i["Kind"] == "git-blob"]
        self.assertIn((C()["M1-good"], MAP), blobs)
        self.assertIn((C()["D"], AP), blobs)
        self.assertEqual([i["Name"] for i in env["Inputs"] if i["Kind"] == "file"], ["CONTRACT"])


class TestClauseMapDefect(ToolCase):
    @classmethod
    def setUpClass(cls):
        cls.cached = None

    def get(self):
        if TestClauseMapDefect.cached is None:
            TestClauseMapDefect.cached = self.facts(["clausemap", "--repo", cmrepo(), "--main", C()["M1-defect"]],
                                                    cmrepo())
        return TestClauseMapDefect.cached

    def test_derive_pre_entry(self):
        c = C()
        f = self.get()
        self.assertEqual(f["Derive"]["FirstParentMerges"], [c["M0"], c["M1-defect"]])
        self.assertEqual(f["Derive"]["TrailerCommits"], [c["N2-defect"]])
        self.assertEqual(f["Pre"]["Rows"], [["---", "---", "---", "---"],
                                            ["I-61", "refs/heads/a", "C-0001", "ANTERIOR"],
                                            ["I-70", "refs/heads/b", "C-0002"],
                                            ["I-71", "refs/heads/c", "C-0002", "ANTERIOR", "extra"]])
        self.assertEqual(f["Pre"]["MalformedRows"], [{"RowIndex": 2, "CellCount": 3}, {"RowIndex": 3, "CellCount": 5}])
        self.assertEqual(f["Pre"]["ClaimIdCounts"], {"---": 1, "C-0001": 1, "C-0002": 2})
        self.assertIsNone(f["Classify"])
        self.assertIsNone(f["Resolve"])
        self.assertEqual(f["Entry"]["EntryMatches"], heading_lines(WF_D, ENTRY))
        self.assertEqual(len(f["Entry"]["EntryMatches"]), 2)
        self.assertIsNone(f["Entry"]["EntryNamesR163"])

    def test_mv2_mv3(self):
        v = self.get()["Validate"]
        self.assertEqual(v["MV2"], {"MapSurfaces": SURF + ["CLAUDE.md"], "ClosedList": SURF})
        changed = sorted([AP, WF, EXISTING, "docs/adr/0001.md", "docs/adr/0002.md", "docs/adr/0003.md",
                          "docs/adr/0004.md", README, SCHEMA])
        self.assertEqual(v["MV3"]["ChangedUnderSurfaces"], changed)
        self.assertEqual(v["MV3"]["DeletedUnderSurfaces"], ["docs/adr/0001.md"])
        self.assertEqual(v["MV3"]["FilesDuplicates"], ["docs/adr/0002.md"])
        self.assertEqual(v["MV3"]["OnlyInChanged"], ["docs/adr/0001.md", "docs/adr/0004.md"])
        self.assertEqual(v["MV3"]["OnlyInFiles"], ["AGENTS.md", "docs/adr/0005.md"])

    def test_mv4_four_lists(self):
        v = self.get()["Validate"]["MV4"]
        self.assertEqual(v["LiteralViolations"], [AP, "docs/adr/0005.md"])
        self.assertEqual(v["EntryFilesOutsideSchema"], ["docs/adr/0003.md", README])
        self.assertEqual(v["AddedPresentInEff1"], [EXISTING])
        self.assertEqual(v["EntryFilePresentInEff1"], [README])
        row = [x for x in v["Rows"] if x["Path"] == "docs/adr/0005.md"][0]
        self.assertIsNone(row["ObservedEffBlob"])
        row = [x for x in v["Rows"] if x["Path"] == AP][0]
        self.assertEqual((row["MapBaseBlob"], row["ObservedEff1Blob"]), (gitblob(WF_V1), gitblob(AP_V1)))

    def test_mv5(self):
        v = self.get()["Validate"]["MV5"]
        self.assertEqual(v["EntryKeyDuplicates"], [{"Path": WF, "Section": "## 10. Autoridad"}])
        self.assertEqual(v["EntriesOnNonModifiedPath"], [{"Path": "docs/adr/0002.md", "Section": "# ADR 2",
                                                          "Kind": "ADDED"}])

    def test_mv6(self):
        v = self.get()["Validate"]["MV6"]
        by_path = dict((x["Path"], x) for x in v["PerFile"])
        self.assertEqual([x["Path"] for x in v["PerFile"]], [AP, WF, "AGENTS.md"])
        ap = by_path[AP]
        self.assertEqual(ap["DerivedModified"], sorted([PRE, HPLAN, H16, H163]))
        self.assertEqual(ap["DerivedAddedOrEntry"], sorted([H1610, R163]))
        self.assertEqual(ap["MapModified"], sorted([PRE, HPLAN, H16, H163, "## 17. Final"]))
        self.assertEqual(ap["MapAddedOrEntry"], sorted([H1610, R163, "### 16.1 Participantes"]))
        wf = by_path[WF]
        self.assertEqual(wf["H1Duplicates"], [])
        self.assertEqual(wf["H2Duplicates"], sorted(["## 11. Otro", ENTRY]))
        self.assertEqual(wf["DerivedModified"], sorted([HWF, "## 11. Otro"]))
        self.assertEqual(wf["DerivedRemoved"], [])
        self.assertEqual(wf["DerivedAddedOrEntry"], [ENTRY])
        self.assertEqual(wf["MapModified"], sorted([HWF, "## 10. Autoridad"]))
        ag = by_path["AGENTS.md"]
        self.assertEqual((ag["DerivedModified"], ag["DerivedRemoved"], ag["DerivedAddedOrEntry"]), ([], [], []))
        self.assertEqual(v["EntryKindEntries"], [{"Path": AP, "Section": R163},
                                                 {"Path": AP, "Section": "### 16.1 Participantes"},
                                                 {"Path": WF, "Section": ENTRY}])

    def test_mv7_missing_pointer(self):
        v = self.get()["Validate"]["MV7"]
        self.assertEqual(v["EntryCountEff"], 2)
        self.assertIsNone(v["EntryNamesR163Eff"])
        self.assertEqual(v["FirstSentenceS16"], "La ejecuci" + O + "n delegada es un modo.")
        self.assertIs(v["PointerIsFirstSentenceS16"], False)
        self.assertEqual(v["LastSentenceS163"], POINTER)
        self.assertIs(v["PointerIsLastSentenceS163"], True)
        self.assertEqual(v["S163EffMinusPointer"],
                         "Una seccion va desde su encabezado. Si algo no es verificable, alto (S-12).")
        self.assertEqual(v["S163Eff1"], S163_V1)
        self.assertIs(v["S163EqualAfterPointerRemoval"], False)


class TestClauseMapOtherStates(ToolCase):
    def test_pre_activation(self):
        f = self.facts(["clausemap", "--repo", cmrepo(), "--main", C()["M0"]], cmrepo())
        self.assertEqual(f["Derive"], {"TrailerCommits": [], "FirstParentMerges": [C()["M0"]], "Candidates": [],
                                       "Eff": None, "EffParent1": None, "EffParent2": None})
        self.assertIsNone(f["Pre"])
        self.assertIsNone(f["Validate"])
        self.assertEqual(f["Entry"], {"WorkflowBlob": gitblob(WF_V1), "ApBlob": gitblob(AP_V1), "EntryMatches": [],
                                      "R163Matches": [], "EntryNamesR163": None})

    def test_schema_violations(self):
        v = self.facts(["clausemap", "--repo", cmrepo(), "--main", C()["M1-schemabad"]], cmrepo())["Validate"]
        expected = sorted([("/Entries/0/Extra", "additionalProperties"), ("/Entries/0/Level", "maximum"),
                           ("/Files/0/BaseBlob", "pattern"), ("/Files/0/EffBlob", "type"),
                           ("/Files/0/FileKind", "enum"), ("/Files/0/Path", "minLength"),
                           ("/Files/1/EffBlob", "pattern"), ("/LegacyProtocol", "required"),
                           ("/Schema", "enum"), ("/Surfaces", "uniqueItems")])
        self.assertEqual(v["MV1"]["SchemaViolations"], [{"Pointer": p, "Keyword": k} for p, k in expected])
        self.assertIsNone(v["MV2"]["MapSurfaces"])
        self.assertEqual(v["MV2"]["ClosedList"], SURF)
        self.assertIsNone(v["MV3"])
        self.assertIsNone(v["MV4"])
        self.assertIsNone(v["MV6"]["PerFile"])
        self.assertIs(v["MV7"]["S163EqualAfterPointerRemoval"], True)

    def test_map_absent(self):
        v = self.facts(["clausemap", "--repo", cmrepo(), "--main", C()["M1-absent"]], cmrepo())["Validate"]
        self.assertEqual(v["MV1"], {"MapPresent": False, "MapBlob": None, "MapParses": None, "SchemaPresent": True,
                                    "SchemaBlob": gitblob(SCHEMA_TEXT), "SchemaViolations": None})
        self.assertIsNone(v["MV3"])
        self.assertIs(v["MV7"]["PointerIsFirstSentenceS16"], True)


class TestNegativeFailClosed(ToolCase):
    """SPEC section 4: every case gives Facts = null, a code and exit 2."""

    def test_nonexistent_sha(self):
        self.error(["identity", "--repo", basic(), "--branch", "feat", "--base", "0123456789abcdef" * 2 + "01234567",
                    "--current", B()["c3"]], "BAD_REVISION")
        self.error(["diff", "--repo", basic(), "--from", "c1", "--to", B()["c3"]], "BAD_REVISION")

    def test_missing_file(self):
        missing = os.path.join(STATE["work"], "inputs", "absent.json")
        self.error(["scope", "--repo", basic(), "--from", B()["c1"], "--to", B()["c2"], "--file", "DELEG=" + missing,
                    "--sha256", "DELEG=" + "0" * 64], "MISSING_INPUT")

    def test_sha256_mismatch(self):
        path, sha = write_input("deleg-mm.json", json.dumps({"AllowedWriteScope": [], "ForbiddenWriteScope": []}))
        wrong = ("f" if sha[0] != "f" else "e") + sha[1:]
        self.error(["scope", "--repo", basic(), "--from", B()["c1"], "--to", B()["c2"], "--file", "DELEG=" + path,
                    "--sha256", "DELEG=" + wrong], "INPUT_HASH_MISMATCH")

    def test_unreadable_json(self):
        path, sha = write_input("deleg-bad.json", '{"AllowedWriteScope": [')
        self.error(["scope", "--repo", basic(), "--from", B()["c1"], "--to", B()["c2"], "--file", "DELEG=" + path,
                    "--sha256", "DELEG=" + sha], "MALFORMED_JSON")

    def test_unreadable_xml(self):
        for tag, text in (("broken", "<TestRun><Results></TestRun>"),
                          ("doctype", '<?xml version="1.0"?><!DOCTYPE x [<!ENTITY a "b">]><TestRun/>')):
            path, sha = write_input("bad-%s.trx" % tag, text)
            self.error(["trx", "--repo", basic(), "--file", "TRX=" + path, "--sha256", "TRX=" + sha], "MALFORMED_XML")

    def test_duplicate_trailer(self):
        self.error(["clausemap", "--repo", cmrepo(), "--main", C()["M1-duptrailer"]], "AMBIGUOUS", repo=cmrepo())

    def test_ambiguous_eff(self):
        self.error(["clausemap", "--repo", cmrepo(), "--main", C()["ME"]], "AMBIGUOUS", repo=cmrepo())

    def test_unsupported_schema_keyword(self):
        self.error(["clausemap", "--repo", cmrepo(), "--main", C()["M1-unsupported"]], "UNSUPPORTED_SCHEMA_KEYWORD",
                   repo=cmrepo())

    def test_forbidden_git_subcommand(self):
        driver = ("import importlib.util, sys\n"
                  "spec = importlib.util.spec_from_file_location('bmin_under_test', sys.argv[1])\n"
                  "m = importlib.util.module_from_spec(spec)\n"
                  "spec.loader.exec_module(m)\n"
                  "def patched(ctx, a):\n"
                  "    m.need_commit(ctx, a['from'], '--from')\n"
                  "    m.git_exec(ctx, sys.argv[2].split(' '))\n"
                  "    return {}\n"
                  "m.COMMAND_FUNCS['diff'] = patched\n"
                  "sys.exit(m.main(sys.argv[3:]))\n")
        for forbidden in ("hash-object --stdin", "config --get core.bare", "fetch origin", "ls-remote .",
                          "diff --output=x.txt HEAD", "log -w"):
            repo = basic()
            before = snapshot(repo)
            p = subprocess.run([PY, "-I", "-c", driver, TOOL, forbidden, "diff", "--repo", repo, "--from", B()["c1"],
                                "--to", B()["c2"]], stdout=subprocess.PIPE, stderr=subprocess.PIPE, cwd=STATE["work"])
            self.assertEqual(before, snapshot(repo))
            env = self.check_envelope(p.returncode, p.stdout, p.stderr)
            self.assertEqual(p.returncode, 2, forbidden)
            self.assertIsNone(env["Facts"])
            self.assertEqual(env["Error"]["Code"], "FORBIDDEN_OPERATION", forbidden)
            self.assertEqual(len(env["GitCalls"]), 1, "the forbidden call must not be issued")

    def test_without_isolation(self):
        self.error(["diff", "--repo", basic(), "--from", B()["c1"], "--to", B()["c2"]], "NOT_ISOLATED",
                   isolated=False)

    def test_state_with_two_claim_ids(self):
        self.error(["clausemap", "--repo", cmrepo(), "--main", C()["Z"], "--state-rev", C()["U3"],
                    "--state-path", "docs/automation/state/U-99.yml"], "AMBIGUOUS", repo=cmrepo())


class TestRunnerUnit(unittest.TestCase):
    """Direct checks of the single git runner (closed list, forced flags, environment)."""

    def setUp(self):
        self.m = load_tool_module()
        self.calls = []
        calls = self.calls

        class FakeSubprocess(object):
            DEVNULL = -3
            PIPE = -1

            @staticmethod
            def run(argv, **kw):
                calls.append((argv, kw))
                return types.SimpleNamespace(returncode=0, stdout=b"", stderr=b"")

        self.m.subprocess = FakeSubprocess
        self.ctx = self.m.Ctx(STATE["work"])

    def test_forced_flags_and_env(self):
        os.environ["GIT_DIR"] = "Z:\\nowhere"
        try:
            self.m.git_exec(self.ctx, ["diff", "--name-only", "a..b"])
            self.m.git_exec(self.ctx, ["show", "-s", "x"])
            self.m.git_exec(self.ctx, ["status"])
        finally:
            del os.environ["GIT_DIR"]
        argv, kw = self.calls[0]
        self.assertEqual(argv, ["git"] + FORCED + ["diff", "--no-ext-diff", "--no-textconv", "--name-only", "a..b"])
        self.assertIs(kw["shell"], False)
        self.assertEqual(kw["cwd"], STATE["work"])
        env = kw["env"]
        self.assertEqual((env["GIT_OPTIONAL_LOCKS"], env["GIT_TERMINAL_PROMPT"], env["LC_ALL"]), ("0", "0", "C"))
        self.assertNotIn("GIT_DIR", env)
        self.assertEqual(self.calls[1][0][5:8], ["show", "--no-ext-diff", "--no-textconv"])
        self.assertEqual(self.calls[2][0][5:], ["status", "--porcelain"])
        self.assertEqual(self.ctx.git_calls, [c[0][1:] for c in self.calls])

    def test_closed_list(self):
        bad = [["fetch"], ["pull"], ["push"], ["ls-remote"], ["remote", "-v"], ["config", "-l"],
               ["hash-object", "-w", "x"], ["update-ref", "HEAD", "x"], ["diff", "-w"], ["diff", "--output=o"],
               ["show", "--ext-diff"], ["log", "--textconv"], ["cat-file", "--write"], []]
        for args in bad:
            with self.assertRaises(self.m.FactsError) as cm:
                self.m.git_exec(self.ctx, args)
            self.assertEqual(cm.exception.code, "FORBIDDEN_OPERATION", args)
        self.assertEqual(self.calls, [])


class TestStaticSource(unittest.TestCase):
    """AST scan of the tool: no write or network API, git as the only process, ASCII source, vocabulary."""

    @classmethod
    def setUpClass(cls):
        with open(TOOL, "rb") as fh:
            cls.src = fh.read()
        cls.tree = ast.parse(cls.src)

    @staticmethod
    def dotted(node):
        parts = []
        while isinstance(node, ast.Attribute):
            parts.append(node.attr)
            node = node.value
        if isinstance(node, ast.Name):
            parts.append(node.id)
            return ".".join(reversed(parts))
        return None

    def test_ascii_source(self):
        self.assertTrue(all(b < 0x80 for b in self.src))

    def test_imports_allowlist(self):
        allowed = {"hashlib", "json", "os", "platform", "re", "subprocess", "sys", "xml.etree.ElementTree"}
        for node in ast.walk(self.tree):
            if isinstance(node, ast.Import):
                for alias in node.names:
                    self.assertIn(alias.name, allowed)
            self.assertNotIsInstance(node, ast.ImportFrom)

    def test_no_write_network_or_dynamic_apis(self):
        banned_names = {"eval", "exec", "compile", "__import__", "globals", "locals", "setattr", "delattr", "input",
                        "breakpoint", "__file__", "getattr"}
        attr_allow = {"os": {"environ", "path"}, "os.path": {"isdir", "isfile", "join"},
                      "subprocess": {"run", "DEVNULL", "PIPE"}, "sys": {"argv", "flags", "stdout", "exit"},
                      "ET": {"fromstring", "ParseError"}, "hashlib": {"sha1", "sha256"},
                      "platform": {"python_version"}, "json": {"dumps", "loads"}}
        for node in ast.walk(self.tree):
            if isinstance(node, ast.Name):
                self.assertNotIn(node.id, banned_names)
            if isinstance(node, ast.Attribute):
                base = self.dotted(node.value)
                if base in attr_allow:
                    self.assertIn(node.attr, attr_allow[base], "%s.%s" % (base, node.attr))
                if node.attr in ("write", "writelines"):
                    self.assertEqual(base, "sys.stdout.buffer")
            if isinstance(node, ast.Call):
                if self.dotted(node.func) == "open":
                    self.assertEqual(len(node.args), 2)
                    self.assertEqual(node.keywords, [])
                    self.assertIsInstance(node.args[1], ast.Constant)
                    self.assertEqual(node.args[1].value, "rb")
                for kw in node.keywords:
                    if kw.arg == "shell":
                        self.assertIsInstance(kw.value, ast.Constant)
                        self.assertIs(kw.value.value, False)

    def test_git_is_the_only_process(self):
        runs = []
        for fn in ast.walk(self.tree):
            if isinstance(fn, ast.FunctionDef):
                for node in ast.walk(fn):
                    if isinstance(node, ast.Call) and (self.dotted(node.func) or "").startswith("subprocess."):
                        runs.append((fn.name, node))
        self.assertEqual(len(runs), 1)
        name, call = runs[0]
        self.assertEqual(name, "git_exec")
        self.assertEqual(self.dotted(call.func), "subprocess.run")
        first = call.args[0]
        self.assertIsInstance(first, ast.BinOp)
        self.assertIsInstance(first.left, ast.List)
        self.assertEqual([e.value for e in first.left.elts], ["git"])
        self.assertIn("shell", [kw.arg for kw in call.keywords])

    def test_vocabulary_in_tool_literals(self):
        for node in ast.walk(self.tree):
            if isinstance(node, ast.Constant) and isinstance(node.value, str):
                for word in FORBIDDEN_WORDS:
                    self.assertNotIn(word, node.value, "tool literal %r" % node.value)


if __name__ == "__main__":
    unittest.main()
