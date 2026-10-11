# -*- coding: utf-8 -*-
"""Tests of bmin_accept.py (unittest). Synthetic repositories are created under
D:\\r62-bmin-tmp\\accept (override with BMIN_ACCEPT_TMP), never under D:\\r62-fixture.
Run:  python -I -m unittest discover -s tests -v   (from the accept folder), or
      python -I tests/test_bmin_accept.py
Set BMIN_KEEP=1 to keep the temporary tree for inspection.
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
import warnings
import sys
import unittest

sys.dont_write_bytecode = True

HERE = os.path.dirname(os.path.abspath(__file__))
ACCEPT_DIR = os.path.dirname(HERE)
ACCEPT_PY = os.path.join(ACCEPT_DIR, "bmin_accept.py")
FAKE_PY = os.path.join(HERE, "fake_tool.py")
TMP_ROOT = os.environ.get("BMIN_ACCEPT_TMP", os.path.join("D:" + os.sep, "r62-bmin-tmp", "accept"))
WORK = os.path.join(TMP_ROOT, "work")
INPUTS = os.path.join(WORK, "inputs")
EMPTY_CFG = os.path.join(WORK, "empty.gitconfig")
PIPE = subprocess.PIPE

assert "r62-fixture" not in os.path.abspath(TMP_ROOT).lower(), "temporary tree must not be under D:\\r62-fixture"


def _load(path, name):
    spec = importlib.util.spec_from_file_location(name, path)
    mod = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(mod)
    return mod


ba = _load(ACCEPT_PY, "bmin_accept_under_test")

REQ_SCHEMA = "rackcad-bmin-accept-request/0-experimental"
TRAILER_VALUE = "Claude Opus 5.5 <noreply@anthropic.com>"
ENTRY = ba.ENTRY_HEADING
R163 = ba.R163_HEADING
POINTER = ba.POINTER
AP = "docs/AUTOMATION_PLAN.md"
WF = "docs/WORKFLOW.md"
MAP = ba.MAP_PATH
SCHEMA = ba.SCHEMA_PATH
NEWJ = "docs/automation/agent-execution/new.json"
XJ = "docs/automation/agent-execution/x.json"
STATE = "docs/automation/state/U.yml"

# ----------------------------------------------------------------------------- texts

S16_HEAD = "## 16. Ejecución delegada bajo orden del Coordinator"
S16_BODY = "La **ejecución delegada** es la ejecución de un trabajo. Versión 16.1 incluida."
S163_HEAD = "### 16.3 Lectura de autoridades"
S163_L1 = ("`AuthorityRevision` es el SHA del commit. Una **sección** va desde su encabezado hasta el siguiente "
           "encabezado con el mismo número de `#` o menos.")
S163_L2 = "Si algo de esto no es verificable → STOP (S-12)."
S1610_HEAD = "### 16.10 Términos de gate"
TERMS_PARA = [
    "Coincidencia por palabra o frase completa, sin distinguir mayúsculas, en `WorkCompleted`, `Evidence`, "
    "`UnexpectedFindings`, `KnownLimitations`, `Deviations`, `OpenQuestions` y",
    "`RecommendedNextAction`: `GATE PASS`, `GATE_PASS`, `Candidato`, `Candidate`, `FINAL_CANDIDATE_SHA`, "
    "`gate cerrado`, `gate closed`, `cierre del gate`, `rama integrada`,",
    "`integrada en main`, `integrated into main`, `merged into main`, `lista para integrar`, `ready to merge`, "
    "`Owner Validation APROBADA` y `Owner Validation APPROVED`. No se aplica al",
    "nombre de propiedad `Gate` ni a su valor."]
EXPECTED_TERMS = ["GATE PASS", "GATE_PASS", "Candidato", "Candidate", "FINAL_CANDIDATE_SHA", "gate cerrado",
                  "gate closed", "cierre del gate", "rama integrada", "integrada en main", "integrated into main",
                  "merged into main", "lista para integrar", "ready to merge", "Owner Validation APROBADA",
                  "Owner Validation APPROVED"]
SURFACES = ["AGENTS.md", "CLAUDE.md", "docs/AUTOMATION_PLAN.md", "docs/FOUNDATIONS.md",
            "docs/INITIATIVE_LIFECYCLE.md", "docs/WORKFLOW.md", "docs/adr/", "docs/automation/agent-execution/",
            "docs/initiatives/PROMPT_TEMPLATES.md"]


def ap_text(eff=False, s163_extra=False):
    lines = ["# Plan de prueba", "", "Preámbulo del plan.", "", "## 1. Proposito", "", "Uno.", ""]
    lines += [S16_HEAD, "", (POINTER + " " if eff else "") + S16_BODY, ""]
    l1 = S163_L1.replace("del commit.", "del commit final.") if s163_extra else S163_L1
    lines += [S163_HEAD, "", l1, S163_L2 + (" " + POINTER if eff else ""), ""]
    lines += ["### 16.4 Transporte", "", "Texto de 16.4.", "", "```text", "## 99. Esto no es un encabezado", "```", ""]
    lines += [S1610_HEAD, ""] + TERMS_PARA + [""]
    if eff:
        lines += [R163, "", "Rige desde el punto efectivo.", "",
                  "**Superficies** (lista cerrada): " + ", ".join("`" + s + "`" for s in SURFACES[:5]) + ",",
                  ", ".join("`" + s + "`" for s in SURFACES[5:]) + ".", "", "Fin de 16.13.", ""]
    lines += ["### 16.30 Planos", "", "Texto de 16.30.", "", "## 17. Otra", "", "Final.", ""]
    return "\n".join(lines)


def wf_text(eff=False):
    lines = ["# Workflow de prueba", "", "Intro.", "", "## 1. Ramas", "", "Texto de ramas.", "",
             "## 11. Workflow V2", "", "Texto 11.", ""]
    if eff:
        lines += [ENTRY, "", "**Coexistencia.** Desde `I62_EFFECTIVE_SHA`",
                  "(AUTOMATION_PLAN `" + R163 + "`), toda evaluación aplica primero esa subsección.", ""]
    return "\n".join(lines)


CLAUSE_SCHEMA = {
    "$schema": "https://json-schema.org/draft/2020-12/schema",
    "title": "rackcad-clause-map/v1",
    "type": "object",
    "additionalProperties": False,
    "properties": {
        "Schema": {"type": "string", "enum": ["rackcad-clause-map/v1"]},
        "Protocol": {"type": "string", "enum": ["rackcad-protocol/I62"]},
        "LegacyProtocol": {"type": "string", "enum": ["rackcad-protocol/I61"]},
        "Surfaces": {"type": "array", "minItems": 1, "uniqueItems": True, "items": {"type": "string", "minLength": 1}},
        "Files": {"type": "array", "items": {
            "type": "object", "additionalProperties": False,
            "properties": {"Path": {"type": "string", "minLength": 1},
                           "FileKind": {"type": "string", "enum": ["MODIFIED", "ADDED", "ENTRY"]},
                           "BaseBlob": {"type": ["string", "null"], "pattern": "^[0-9a-f]{40}$"},
                           "EffBlob": {"type": "string", "pattern": "^[0-9a-f]{40}$"}},
            "required": ["BaseBlob", "EffBlob", "FileKind", "Path"]}},
        "Entries": {"type": "array", "items": {
            "type": "object", "additionalProperties": False,
            "properties": {"Path": {"type": "string", "minLength": 1},
                           "Section": {"type": "string", "minLength": 1},
                           "Level": {"type": "integer", "minimum": 0, "maximum": 6},
                           "Kind": {"type": "string", "enum": ["MODIFIED", "REMOVED", "ADDED", "ENTRY"]}},
            "required": ["Kind", "Level", "Path", "Section"]}}},
    "required": ["Entries", "Files", "LegacyProtocol", "Protocol", "Schema", "Surfaces"]}

# ----------------------------------------------------------------------------- git helpers

_EPOCH = 1767225600


class G(object):
    tick = [0]

    def __init__(self, path):
        self.path = path
        os.makedirs(path)
        self.run("init", "-q", "-b", "main")
        for k, v in (("core.autocrlf", "false"), ("core.safecrlf", "false"), ("diff.renames", "true"),
                     ("status.showUntrackedFiles", "normal"), ("commit.gpgsign", "false"),
                     ("user.name", "Test"), ("user.email", "t@example.invalid")):
            self.run("config", k, v)

    @staticmethod
    def env():
        G.tick[0] += 1
        t = "@%d +0000" % (_EPOCH + 60 * G.tick[0])
        e = dict((k, v) for k, v in os.environ.items() if not k.upper().startswith("GIT_"))
        e.update({"GIT_CONFIG_NOSYSTEM": "1", "GIT_CONFIG_GLOBAL": EMPTY_CFG, "GIT_AUTHOR_NAME": "Test Author",
                  "GIT_AUTHOR_EMAIL": "a@example.invalid", "GIT_COMMITTER_NAME": "Test Committer",
                  "GIT_COMMITTER_EMAIL": "c@example.invalid", "GIT_AUTHOR_DATE": t, "GIT_COMMITTER_DATE": t,
                  "LC_ALL": "C", "GIT_OPTIONAL_LOCKS": "0"})
        return e

    def run(self, *args, **kw):
        p = subprocess.run(["git", "-C", self.path] + list(args), input=kw.get("input"), stdout=PIPE,
                           stderr=PIPE, env=G.env())
        if p.returncode not in kw.get("ok", (0,)):
            raise AssertionError("git %r failed: %s" % (args, p.stderr.decode("utf-8", "replace")))
        return p.stdout

    def out(self, *args, **kw):
        return self.run(*args, **kw).decode("utf-8").strip()

    def write(self, rel, content):
        full = os.path.join(self.path, *rel.split("/"))
        d = os.path.dirname(full)
        if not os.path.isdir(d):
            os.makedirs(d)
        with open(full, "wb") as fh:
            fh.write(content.encode("utf-8") if isinstance(content, str) else content)

    def remove(self, rel):
        os.remove(os.path.join(self.path, *rel.split("/")))

    def _msgfile(self, message):
        f = os.path.join(WORK, "msg-%d.txt" % G.tick[0])
        with open(f, "wb") as fh:
            fh.write(message.encode("utf-8"))
        return f

    def commit(self, message):
        self.run("add", "-A")
        f = self._msgfile(message)
        self.run("commit", "-q", "--allow-empty", "--cleanup=verbatim", "-F", f)
        os.remove(f)
        return self.out("rev-parse", "HEAD")

    def merge(self, branch, message):
        f = self._msgfile(message)
        self.run("merge", "--no-ff", "-q", "-F", f, branch)
        os.remove(f)
        return self.out("rev-parse", "HEAD")

    def blob_of(self, content):
        data = content.encode("utf-8") if isinstance(content, str) else content
        return self.out("hash-object", "--stdin", input=data)

    def rev_blob(self, rev, path):
        return self.out("rev-parse", rev + ":" + path)


def write_json(name, obj):
    path = os.path.join(INPUTS, name)
    data = (json.dumps(obj, indent=2, ensure_ascii=False) + "\n").encode("utf-8")
    with open(path, "wb") as fh:
        fh.write(data)
    return path, hashlib.sha256(data).hexdigest()


def write_bytes(name, data):
    path = os.path.join(INPUTS, name)
    with open(path, "wb") as fh:
        fh.write(data)
    return path


def sha256_file(path):
    with open(path, "rb") as fh:
        return hashlib.sha256(fh.read()).hexdigest()


def blob_sha1_file(path):
    with open(path, "rb") as fh:
        data = fh.read()
    return hashlib.sha1(b"blob %d\0" % len(data) + data).hexdigest()


def exe_sha256():
    for p in (sys.executable, os.path.join(sys.base_prefix, os.path.basename(sys.executable))):
        try:
            with open(p, "rb") as fh:
                return hashlib.sha256(fh.read()).hexdigest()
        except OSError:
            continue
    raise AssertionError("interpreter image unreadable")


def tool_env(mutant=None):
    e = dict(os.environ)
    e["FAKE_TOOL_ACCEPT_PATH"] = ACCEPT_PY
    e.pop("FAKE_TOOL_MUTANT", None)
    if mutant:
        e["FAKE_TOOL_MUTANT"] = mutant
    return e


_ENV_N = [0]
_ENV_CACHE = {}


def make_envelope(argv, mutant=None, isolated=True):
    """Runs the fake tool once per (argv, mutant, isolation); every call gets a fresh copy of the bytes."""
    _ENV_N[0] += 1
    key = (tuple(argv), mutant, isolated)
    if key not in _ENV_CACHE:
        cmd = [sys.executable] + (["-I"] if isolated else []) + [FAKE_PY] + argv
        p = subprocess.run(cmd, stdout=PIPE, stderr=PIPE, env=tool_env(mutant))
        _ENV_CACHE[key] = (p.returncode, p.stdout)
    rc, raw = _ENV_CACHE[key]
    path = write_bytes("env-%03d-%s.json" % (_ENV_N[0], argv[0]), raw)
    return path, rc, raw


_REQ_N = [0]


def run_acceptor(req, mutant=None, isolated=True, extra_args=None):
    _REQ_N[0] += 1
    rpath = write_bytes("req-%03d.json" % _REQ_N[0], (json.dumps(req, indent=1) + "\n").encode("utf-8"))
    cmd = [sys.executable] + (["-I"] if isolated else []) + [ACCEPT_PY, "--request", rpath] + (extra_args or [])
    p = subprocess.run(cmd, stdout=PIPE, stderr=PIPE, env=tool_env(mutant))
    out = json.loads(p.stdout.decode("ascii")) if p.stdout else None
    checks = dict((c["Id"], c) for c in out["Checks"]) if out else {}
    return p.returncode, p.stdout, out, checks


def statuses(checks):
    return dict((k, v["Status"]) for k, v in checks.items())


# ----------------------------------------------------------------------------- fixtures

FX = {}


def build_unit_repo():
    g = G(os.path.join(WORK, "unit"))
    g.write(".gitignore", "artifacts/\n")
    g.write("README.md", "# Unit\n")
    g.write(AP, ap_text())
    g.write("src/keep.cs", "keep\n")
    c0 = g.commit("init\n")
    g.run("checkout", "-q", "-b", "fx/u1")
    g.write("src/a.cs", "a\n")
    c1 = g.commit("feat: a\n\nCo-Authored-By: " + TRAILER_VALUE + "\nCo-Authored-By: Segundo <s@example.invalid>\n")
    g.write("docs/señal.md", "Señal ñandú\n")
    c2 = g.commit("docs: señal\n\nCuerpo con acentos: canción.\n\nSigned-off-by: José Ñúñez <j@example.invalid>\n"
                  "Co-Authored-By: " + TRAILER_VALUE + "\n")
    g.write("tests/t.cs", "t\n")
    c3 = g.commit("test: t\n\nco-authored-by: " + TRAILER_VALUE + "\n")
    g.run("update-ref", "refs/remotes/origin/fx/u1", c3)
    g.run("update-ref", "refs/remotes/origin/main", c0)
    # trailer-parsing cases (compared with git's %(trailers) through the fake tool)
    g.run("checkout", "-q", "-b", "fx/trailers", c0)
    msgs = [
        "t1\n\nbody text\nCo-Authored-By: Mixed <m@example.invalid>\n",
        "t2\n\nCo-Authored-By: NotLast <n@example.invalid>\n\nlast paragraph text\n",
        "t3\n\nCo-Authored-By: A <a@example.invalid>\nCo-Authored-By: B <b@example.invalid>\n",
        "Co-Authored-By: Title <t@example.invalid>\n",
        "t5\n\nSigned-off-by: S <s@example.invalid>\nsome free text\nCo-Authored-By: Ñandú <n@example.invalid>\n",
        "t6\n\nCo-Authored-By : Spaced <s@example.invalid>\n",
        "t7\n\nCO-AUTHORED-BY: Upper <u@example.invalid>\n",
        "t8\n\nCo-Authored-By: Fold <f@example.invalid>\n  continued\n",
        "t9\r\n\r\nCo-Authored-By: CR <cr@example.invalid>\r\n",
        "t10\n\nReviewed-by: R <r@example.invalid>\nCo-Authored-By: Z <z@example.invalid>\n\n\n",
    ]
    tcommits = []
    for i, m in enumerate(msgs):
        g.write("tr/%d.txt" % i, "%d\n" % i)
        tcommits.append(g.commit(m))
    g.run("checkout", "-q", "-b", "fx/rename", c0)
    g.run("mv", "src/keep.cs", "src/kept.cs")
    r1 = g.commit("rename\n")
    g.run("checkout", "-q", "fx/u1")
    FX["unit"] = g
    FX["u"] = {"c0": c0, "c1": c1, "c2": c2, "c3": c3, "tlast": tcommits[-1], "tcommits": tcommits, "r1": r1}


def _map_obj(g, ap_eff, wf_eff, schema_text, newj, defect):
    base_ap = g.blob_of(ap_text())
    base_wf = g.blob_of(wf_text())
    if not defect:
        files = [
            {"Path": AP, "FileKind": "MODIFIED", "BaseBlob": base_ap, "EffBlob": g.blob_of(ap_eff)},
            {"Path": WF, "FileKind": "MODIFIED", "BaseBlob": base_wf, "EffBlob": g.blob_of(wf_eff)},
            {"Path": SCHEMA, "FileKind": "ENTRY", "BaseBlob": None, "EffBlob": g.blob_of(schema_text)},
            {"Path": NEWJ, "FileKind": "ADDED", "BaseBlob": None, "EffBlob": g.blob_of(newj)}]
        entries = [
            {"Path": AP, "Section": "# Plan de prueba", "Level": 1, "Kind": "MODIFIED"},
            {"Path": AP, "Section": S16_HEAD, "Level": 2, "Kind": "MODIFIED"},
            {"Path": AP, "Section": S163_HEAD, "Level": 3, "Kind": "MODIFIED"},
            {"Path": AP, "Section": R163, "Level": 3, "Kind": "ENTRY"},
            {"Path": WF, "Section": "# Workflow de prueba", "Level": 1, "Kind": "MODIFIED"},
            {"Path": WF, "Section": ENTRY, "Level": 2, "Kind": "ENTRY"}]
    else:
        effb = g.blob_of(ap_eff)
        files = [
            {"Path": AP, "FileKind": "MODIFIED", "BaseBlob": effb, "EffBlob": effb},
            {"Path": WF, "FileKind": "MODIFIED", "BaseBlob": base_wf, "EffBlob": g.blob_of(wf_eff)},
            {"Path": SCHEMA, "FileKind": "ENTRY", "BaseBlob": None, "EffBlob": g.blob_of(schema_text)},
            {"Path": NEWJ, "FileKind": "ADDED", "BaseBlob": None, "EffBlob": g.blob_of(newj)},
            {"Path": "docs/adr/0001.md", "FileKind": "ADDED", "BaseBlob": None, "EffBlob": g.blob_of("# ADR 1\n")},
            {"Path": "AGENTS.md", "FileKind": "ENTRY", "BaseBlob": None, "EffBlob": g.blob_of("# Agents\n\nReglas.\n")}]
        entries = [
            {"Path": AP, "Section": "# Plan de prueba", "Level": 1, "Kind": "MODIFIED"},
            {"Path": AP, "Section": S163_HEAD, "Level": 3, "Kind": "MODIFIED"},
            {"Path": AP, "Section": S163_HEAD, "Level": 3, "Kind": "MODIFIED"},
            {"Path": AP, "Section": S1610_HEAD, "Level": 3, "Kind": "MODIFIED"},
            {"Path": AP, "Section": R163, "Level": 3, "Kind": "ENTRY"},
            {"Path": WF, "Section": "# Workflow de prueba", "Level": 1, "Kind": "MODIFIED"},
            {"Path": WF, "Section": ENTRY, "Level": 2, "Kind": "ADDED"},
            {"Path": "AGENTS.md", "Section": "preámbulo", "Level": 0, "Kind": "MODIFIED"}]
    m = {"Schema": "rackcad-clause-map/v1", "Protocol": "rackcad-protocol/I62",
         "LegacyProtocol": "rackcad-protocol/I61", "Surfaces": SURFACES, "Files": files, "Entries": entries}
    if defect:
        m["Extra"] = True
    return json.dumps(m, indent=2, ensure_ascii=False) + "\n"


def _apply_i62(g, defect):
    ap_eff = ap_text(eff=True, s163_extra=defect)
    wf_eff = wf_text(eff=True)
    schema_text = json.dumps(CLAUSE_SCHEMA, indent=2) + "\n"
    newj = '{"nuevo": true}\n'
    g.write(AP, ap_eff)
    g.write(WF, wf_eff)
    g.write(SCHEMA, schema_text)
    g.write(NEWJ, newj)
    g.write("src/x.cs", "x\n")
    if defect:
        g.write(XJ, '{"a": 2}\n')
        g.remove("docs/adr/0002.md")
    g.write(MAP, _map_obj(g, ap_eff, wf_eff, schema_text, newj, defect))


def build_map_repo():
    g = G(os.path.join(WORK, "maprepo"))
    g.write(AP, ap_text())
    g.write(WF, wf_text())
    g.write("AGENTS.md", "# Agents\n\nReglas.\n")
    g.write("docs/adr/0001.md", "# ADR 1\n")
    g.write("docs/adr/0002.md", "# ADR 2\n")
    g.write(XJ, '{"a": 1}\n')
    g.write("src/other.txt", "o\n")
    c0 = g.commit("base\n")
    g.run("checkout", "-q", "-b", "feat/old")
    g.write("src/other.txt", "o2\n")
    g.commit("old feature\n")
    g.run("checkout", "-q", "main")
    m_old = g.merge("feat/old", "Merge feat/old\n")
    sh = {"c0": c0, "m_old": m_old}
    for variant in ("valid", "defect"):
        g.run("checkout", "-q", "-b", "i62-" + variant, m_old)
        _apply_i62(g, variant == "defect")
        sh["t_" + variant] = g.commit("I-62: normativa " + variant + "\n\nAgent-Protocol-Normative: I-62\n")
        g.run("checkout", "-q", "-b", "main-" + variant, m_old)
        sh["eff_" + variant] = g.merge("i62-" + variant, "Merge I-62 " + variant + "\n")
        g.write("src/other.txt", "post " + variant + "\n")
        sh["s_" + variant] = g.commit("post " + variant + "\n")
    g.run("checkout", "-q", "-b", "i62-dup", m_old)
    g.write(WF, wf_text(eff=True))
    sh["d1"] = g.commit("dup one\n\nAgent-Protocol-Normative: I-62\n")
    g.write(AP, ap_text(eff=True))
    sh["d2"] = g.commit("dup two\n\nAgent-Protocol-Normative: I-62\n")
    g.run("checkout", "-q", "-b", "main-dup", m_old)
    sh["s_dup"] = g.merge("i62-dup", "Merge dup\n")
    g.run("checkout", "-q", "-b", "main-direct", m_old)
    g.write("src/direct.txt", "d\n")
    sh["s_direct"] = g.commit("direct\n\nAgent-Protocol-Normative: I-62\n")
    g.run("checkout", "-q", "-b", "u-state", c0)
    g.write(STATE, "unit: U\nbasis:\n  claim_id: C-1   # comentario\nprotocol:\n\tclaim_id:C-1\nclaim_id:\n")
    sh["st_same"] = g.commit("state same claim\n")
    g.write(STATE, "unit: U\nbasis:\n  claim_id: C-1\nother:\n  claim_id: C-2\n")
    sh["st_distinct"] = g.commit("state distinct claims\n")
    g.run("checkout", "-q", "main")
    FX["map"] = g
    FX["m"] = sh


def _force_remove(func, path, _exc):
    os.chmod(path, stat.S_IWRITE)
    func(path)


def remove_tree(path):
    if os.path.isdir(path):
        with warnings.catch_warnings():
            warnings.simplefilter("ignore", DeprecationWarning)
            shutil.rmtree(path, onerror=_force_remove)


def setUpModule():
    remove_tree(WORK)
    os.makedirs(INPUTS)
    with open(EMPTY_CFG, "wb"):
        pass
    build_unit_repo()
    build_map_repo()
    FX["fake_blob"] = blob_sha1_file(FAKE_PY)
    FX["exe_sha"] = exe_sha256()
    rel = write_bytes("relay-facts.json", b'{"relay": "facts"}\n')
    FX["relay"] = (rel, sha256_file(rel))


def tearDownModule():
    if os.environ.get("BMIN_KEEP") != "1":
        remove_tree(WORK)


# ----------------------------------------------------------------------------- request builders

def unit_inputs():
    """Delegation, handoff and contract for the unit repository (scenario A)."""
    if "unit_inputs" in FX:
        return FX["unit_inputs"]
    g, u = FX["unit"], FX["u"]
    deleg = {"Schema": "rackcad-delegation/v2", "ExpectedBranch": "fx/u1", "ExpectedWorktree": g.path,
             "BaseSha": u["c0"], "AllowedWriteScope": ["src/", "tests/**", "docs/señal.md"],
             "ForbiddenWriteScope": ["src/forbidden/", AP, "a..b/", "build/**"]}
    contract = {"Schema": "rackcad-gate-contract/v2", "AllowedWriteScope": ["src/", "tests/", "docs/"],
                "ForbiddenWriteScope": ["src/forbidden/deep/", AP, "bin/**"]}
    FX["unit_inputs"] = {"deleg": write_json("deleg.json", deleg), "contract": write_json("contract.json", contract),
                         "deleg_obj": deleg}
    return FX["unit_inputs"]


def handoff_obj(**over):
    g, u = FX["unit"], FX["u"]
    h = {"Schema": "rackcad-worker-handoff/v1", "TaskId": "T1", "Branch": "fx/u1", "Worktree": g.path,
         "BaseSha": u["c0"], "RedSha": u["c1"], "CurrentSha": u["c3"], "Pushed": True,
         "WorkCompleted": "Implementado sin novedades; GATE PASSED no es un termino.",
         "Evidence": ["Evidence: pruebas en verde", "La palabra Candidatos no coincide"],
         "UnexpectedFindings": [], "KnownLimitations": [], "Deviations": [], "OpenQuestions": [],
         "RecommendedNextAction": "Revisar.", "Gate": "GATE PASS",
         "Worker": {"Provider": "x", "ModelRequested": "m", "EffortRequested": "e",
                    "Trailer": "Co-Authored-By: " + TRAILER_VALUE}}
    h.update(over)
    return h


def cv_obj(results, ar=None):
    checks = {}
    for name in ("Termination", "Handoff", "Authority", "Contract", "Identity", "Remote", "Scope", "CleanTree",
                 "Ci", "Tests", "Trailer", "Routing", "FreeText", "Denials"):
        checks[name] = {"Result": results.get(name, "pass"), "Evidence": "synthetic"}
    checks["Ci"]["RedPart"] = "not_applicable"
    checks["Tests"]["RedPart"] = "not_applicable"
    return {"Schema": "rackcad-controller-verification/v2", "TaskId": "T1", "RunId": "R20261011T000000Z-0001",
            "DelegationRunId": "R20261011T000000Z-0002", "WorkRunId": "R20261011T000000Z-0003", "Attempt": 1,
            "VerifiedSha": None, "Verifier": {}, "Checks": checks, "AuthorityResolution": ar or [],
            "Classification": "EXECUTION_BLOCKED", "Disposition": "BLOCKED", "FailureClass": "NONE",
            "TriggeredStopConditions": [], "Findings": [], "RecommendedNextAction": ""}


_V_N = [0]


def verification(results, handoff=None, deleg=None, main=None, ignore="artifacts/orchestration/u1/handoff.json",
                 terms_rev=None, ar=None, repo="unit"):
    _V_N[0] += 1
    n = _V_N[0]
    u = FX["u"]
    cvp, cvs = write_json("cv-%d.json" % n, cv_obj(results, ar))
    if deleg is None:
        dp, ds = unit_inputs()["deleg"]
    else:
        dp, ds = write_json("deleg-%d.json" % n, deleg)
    hp, hs = write_json("handoff-%d.json" % n, handoff if handoff is not None else handoff_obj())
    return {"Controller": {"Path": cvp, "Sha256": cvs}, "Delegation": {"Path": dp, "Sha256": ds},
            "Handoff": {"Path": hp, "Sha256": hs}, "IgnorePath": ignore,
            "TermsRev": terms_rev or u["c0"], "MainSha": main or u["c0"]}


def closure_obj(repo_key="unit", drop=None, refs=None, revpaths=None, files=None):
    g = FX[repo_key]
    commits = [c for c in g.out("rev-list", "--all").split("\n") if c and c != drop]
    return {"Schema": "rackcad-bmin-closure/0-experimental", "Commits": commits,
            "Refs": refs if refs is not None else ["HEAD", "refs/remotes/origin/fx/u1", "refs/remotes/origin/main"],
            "RevPaths": revpaths or [], "Paths": [], "Objects": [], "Files": files or []}


ALL_RUNS = ("identity", "diff", "scope", "hashes", "contract")


def unit_runs(mutant=None, names=ALL_RUNS):
    g, u = FX["unit"], FX["u"]
    ins = unit_inputs()
    dp, ds = ins["deleg"]
    cp, cs = ins["contract"]
    specs = [
        ("identity", ["identity", "--repo", g.path, "--branch", "fx/u1", "--base", u["c0"], "--current", u["c3"],
                      "--red", u["c1"]]),
        ("diff", ["diff", "--repo", g.path, "--from", u["c0"], "--to", u["c3"]]),
        ("scope", ["scope", "--repo", g.path, "--from", u["c0"], "--to", u["c3"], "--file", "DELEG=" + dp,
                   "--sha256", "DELEG=" + ds]),
        ("hashes", ["hashes", "--repo", g.path, "--rev", u["c3"], "--paths", "src/a.cs", "docs/señal.md",
                    "missing.txt"]),
        ("contract", ["contract", "--repo", g.path, "--file", "CONTRACT=" + cp, "--sha256", "CONTRACT=" + cs,
                      "--file", "DELEG=" + dp, "--sha256", "DELEG=" + ds]),
    ]
    runs = []
    for name, argv in specs:
        if name not in names:
            continue
        path, rc, _ = make_envelope(argv, mutant=mutant)
        runs.append({"Name": name, "Envelope": path, "Argv": argv})
    return runs


def unit_closure(**kw):
    u = FX["u"]
    ins = unit_inputs()
    revpaths = [{"Rev": u["c3"], "Path": p} for p in ("src/a.cs", "docs/señal.md", "missing.txt")]
    files = [{"Name": "DELEG", "Sha256": ins["deleg"][1]}, {"Name": "CONTRACT", "Sha256": ins["contract"][1]}]
    obj = closure_obj("unit", revpaths=revpaths, files=files, **kw)
    path, sha = write_json("closure-%d.json" % len(os.listdir(INPUTS)), obj)
    return {"Path": path, "Sha256": sha}


def base_request(runs, repo_key="unit", **over):
    g = FX[repo_key]
    req = {"Schema": REQ_SCHEMA, "Repo": g.path,
           "Tool": {"Path": FAKE_PY, "PinnedBlob": FX["fake_blob"], "RunFrom": "path"},
           "Interpreter": {"Python": platform.python_version(), "ExecutableSha256": FX["exe_sha"]},
           "Runs": runs, "RelayFacts": {"Path": FX["relay"][0], "CitedSha256": FX["relay"][1]}}
    req.update(over)
    return req


def unit_request(names=("diff",), mutant=None, **over):
    """Request over the unit repository with only the runs a test needs (R3 re-runs each one)."""
    na = {"R6": "no clausemap run in this request"}
    if not set(names) & set(["identity", "diff", "scope", "hashes"]):
        na["R5"] = "no identity/diff/scope/hashes run"
    if not set(names) & set(["scope", "contract"]):
        na["R7"] = "no scope/contract run"
    req = base_request(unit_runs(mutant, names), Closure=unit_closure(), Verification=verification({}),
                       NotApplicable=na)
    req.update(over)
    return req


def full_unit_request(mutant=None, **over):
    return unit_request(ALL_RUNS, mutant, **over)


# ============================================================================= unit tests: primitives

class TestPrimitives(unittest.TestCase):

    def test_blob_sha1_matches_git_hash_object(self):
        g = FX["unit"]
        for data in (b"", b"abc\n", "ñandú\r\n".encode("utf-8"), bytes(range(256))):
            self.assertEqual(ba.git_blob_sha1(data), g.out("hash-object", "--stdin", input=data))

    def test_sentence_splitter(self):
        s = "Antes de aplicar esta sección, toda unidad aplica §16.13. La otra frase (16.3). Fin sin punto"
        self.assertEqual(ba.split_sentences(s), [POINTER, "La otra frase (16.3).", "Fin sin punto"])
        self.assertEqual(ba.split_sentences("Uno.Dos. Tres."), ["Uno.Dos.", "Tres."])
        self.assertEqual(ba.split_sentences(""), [])

    def test_pointer_only_removal(self):
        body = ba.norm_ws(S163_L1 + "\n" + S163_L2 + " " + POINTER)
        self.assertEqual(ba.remove_pointer_sentence(body, POINTER), ba.norm_ws(S163_L1 + " " + S163_L2))
        self.assertEqual(ba.remove_pointer_sentence(POINTER + " X.", POINTER), "X.")
        self.assertEqual(ba.remove_pointer_sentence("X. Y.", POINTER), "X. Y.")

    def test_markdown_sections_and_code_fences(self):
        doc = ba.MdDoc(ap_text(eff=True).replace("\n", "\r\n"))
        heads = [h[2] for h in doc.heads]
        self.assertNotIn("## 99. Esto no es un encabezado", heads)
        self.assertIn(R163, heads)
        k = doc.find_unique_heading(3, "### 16.3")
        self.assertEqual(doc.heads[k][2], S163_HEAD)
        self.assertIsNotNone(doc.find_unique_heading(3, "### 16.30"))
        texts, dups = doc.section_map()
        self.assertEqual(dups, [])
        self.assertIn("preámbulo", texts)
        self.assertEqual(texts["preámbulo"], "# Plan de prueba Preámbulo del plan.")
        self.assertEqual(doc.match_lines("##   12. x"), [])
        self.assertEqual(len(doc.match_lines(R163)), 1)

    def test_scope_rules(self):
        self.assertEqual(ba.u19("docs/**"), "docs/")
        self.assertEqual(ba.u19("docs/"), "docs/")
        for e in ("/abs", "a\\b", "C:/x", "../x", "a/../b", "src/*.cs", "a?", "a[b]", "**", "/**", "a/**/b/**"):
            self.assertTrue(ba.is_malformed(e), e)
        for e in ("docs/", "docs/**", "a..b/", "src/a.cs", "x/**"):
            self.assertFalse(ba.is_malformed(e), e)
        self.assertTrue(ba.covers("src/", "src/a.cs"))
        self.assertTrue(ba.covers("tests/**", "tests/x/y.cs"))
        self.assertFalse(ba.covers("src", "src/a.cs"))
        self.assertFalse(ba.covers("Src/", "src/a.cs"))
        self.assertFalse(ba.covers("src/a.cs", "src/a.csx"))
        self.assertTrue(ba.contained("docs/sub/", "docs/"))
        self.assertTrue(ba.contained("docs/sub/**", "docs/**"))
        self.assertFalse(ba.contained("docs/", "docs/sub/"))
        self.assertFalse(ba.contained("docs/", "docs/a.md"))
        self.assertTrue(ba.contained("docs/a.md", "docs/a.md"))

    def test_trailer_parser_cases(self):
        cases = {
            "x\n\nCo-Authored-By: A <a>\n": ["A <a>"],
            "x\n\nbody\nCo-Authored-By: A <a>\n": [],
            "x\n\nCo-Authored-By: A <a>\n\nlast\n": [],
            "Co-Authored-By: A <a>\n": [],
            "x\n\nSigned-off-by: S <s>\nfree text\nCo-Authored-By: N <n>\n": ["N <n>"],
            "x\n\nCo-Authored-By : S <s>\n": ["S <s>"],
            "x\n\nco-authored-by: L <l>\nCO-AUTHORED-BY: U <u>\n": ["L <l>", "U <u>"],
            "x\n\nCo-Authored-By: F <f>\n  more\n": ["F <f>", "more"],
            "x\r\n\r\nCo-Authored-By: CR <cr>\r\n": ["CR <cr>"],
            "x\n\nCo Authored By: no <n>\n": [],
        }
        for msg, want in cases.items():
            self.assertEqual(ba.coauthored_values(msg), want, repr(msg))

    def test_gate_terms_exclude_field_names(self):
        self.assertEqual(ba.gate_terms(ap_text()), EXPECTED_TERMS)

    def test_freetext_whole_word_casefold(self):
        h = handoff_obj()
        self.assertEqual(ba.freetext_matches(h, EXPECTED_TERMS), [])
        h = handoff_obj(Evidence=["listo para revisar: gate   PASS final", "ready-to-merge no", "ready to merge"],
                        OpenQuestions=["¿Candidato?"])
        got = ba.freetext_matches(h, EXPECTED_TERMS)
        self.assertEqual([(m["Field"], m["Term"]) for m in got],
                         [("Evidence", "GATE PASS"), ("Evidence", "ready to merge"), ("OpenQuestions", "Candidato")])
        with self.assertRaises(ba.AcceptError):
            ba.freetext_matches({"WorkCompleted": "x"}, EXPECTED_TERMS)

    def test_schema_subset_validator(self):
        out = []
        ba.schema_validate(CLAUSE_SCHEMA, {"Schema": "rackcad-clause-map/v1"}, "", out)
        self.assertIn(("", "required"), out)
        out = []
        good = {"Schema": "rackcad-clause-map/v1", "Protocol": "rackcad-protocol/I62",
                "LegacyProtocol": "rackcad-protocol/I61", "Surfaces": ["a"],
                "Files": [{"Path": "a", "FileKind": "ADDED", "BaseBlob": None, "EffBlob": "0" * 40}],
                "Entries": [{"Path": "a", "Section": "x", "Level": 7, "Kind": "MODIFIED"}]}
        ba.schema_validate(CLAUSE_SCHEMA, good, "", out)
        self.assertEqual(out, [("/Entries/0/Level", "maximum")])
        out = []
        good["Files"][0]["EffBlob"] = "0" * 40 + "\n"
        good["Entries"][0]["Level"] = 1
        ba.schema_validate(CLAUSE_SCHEMA, good, "", out)
        self.assertEqual(out, [("/Files/0/EffBlob", "pattern")])
        self.assertEqual(ba.schema_unsupported({"type": "object", "oneOf": []}), ["#/oneOf"])
        self.assertEqual(ba.schema_unsupported(CLAUSE_SCHEMA), [])


# ============================================================================= unit tests: independent facts

class TestIndependentFacts(unittest.TestCase):

    def test_tree_diff_paths_vs_git_no_renames(self):
        g, u = FX["unit"], FX["u"]
        repo = ba.RepoView(g.path)
        for a, b in ((u["c0"], u["c3"]), (u["c0"], u["r1"]), (u["c3"], u["tlast"])):
            want = g.run("diff", "--no-renames", "--name-only", "-z", a, b).decode("utf-8").split("\0")
            self.assertEqual(ba.tree_diff_paths(repo, a, b), [p for p in want if p])

    def test_ancestry_and_ranges_vs_git(self):
        g, u = FX["unit"], FX["u"]
        repo = ba.RepoView(g.path)
        pairs = [(u["c0"], u["c3"]), (u["c3"], u["c0"]), (u["c1"], u["c3"]), (u["c3"], u["c3"]),
                 (u["c1"], u["tlast"])]
        for a, b in pairs:
            rc = subprocess.run(["git", "-C", g.path, "merge-base", "--is-ancestor", a, b], env=G.env()).returncode
            self.assertEqual(repo.is_ancestor(a, b), rc == 0, (a, b))
            want = set(x for x in g.out("rev-list", a + ".." + b).split("\n") if x)
            self.assertEqual(repo.range_commits(a, b), want)
        with self.assertRaises(ba.AcceptError):
            repo.require_commit("0" * 40)

    def test_trailers_vs_git_format(self):
        g, u = FX["unit"], FX["u"]
        repo = ba.RepoView(g.path)
        for c in u["tcommits"] + [u["c1"], u["c2"], u["c3"]]:
            raw = g.run("log", "-1", "--format=%(trailers:key=Co-Authored-By,valueonly)", c).decode("utf-8")
            want = [x.strip() for x in raw.split("\n") if x.strip()]
            self.assertEqual(ba.coauthored_values(repo.message(c)), want, c)

    def test_derive_valid_defect_dup_direct_none(self):
        g, m = FX["map"], FX["m"]
        repo = ba.RepoView(g.path)
        d = ba.acceptor_derive(repo, m["s_valid"])
        self.assertEqual(d, {"TrailerCommits": [m["t_valid"]], "FirstParentMerges": [m["m_old"], m["eff_valid"]],
                             "Candidates": [m["eff_valid"]], "Eff": m["eff_valid"], "EffParent1": m["m_old"],
                             "EffParent2": m["t_valid"]})
        d = ba.acceptor_derive(repo, m["s_dup"])
        self.assertEqual(d["TrailerCommits"], sorted([m["d1"], m["d2"]]))
        self.assertEqual((d["Candidates"], d["Eff"]), ([], None))
        d = ba.acceptor_derive(repo, m["s_direct"])
        self.assertEqual((d["TrailerCommits"], d["FirstParentMerges"], d["Eff"]), ([m["s_direct"]], [m["m_old"]], None))
        d = ba.acceptor_derive(repo, m["m_old"])
        self.assertEqual((d["TrailerCommits"], d["Eff"]), ([], None))

    def test_valid_map_has_no_defects(self):
        g, m = FX["map"], FX["m"]
        repo = ba.RepoView(g.path)
        cm, internal = ba.acceptor_clausemap(repo, m["s_valid"])
        wf_line = wf_text(True).split("\n").index(ENTRY) + 1
        ap_line = ap_text(True).split("\n").index(R163) + 1
        self.assertEqual(cm["Entry"], {"WorkflowBlob": g.rev_blob(m["s_valid"], WF), "ApBlob": g.rev_blob(m["s_valid"], AP),
                                       "EntryMatches": [wf_line], "R163Matches": [ap_line], "EntryNamesR163": True})
        v = cm["Validate"]
        self.assertEqual(v["MV1"]["SchemaViolations"], [])
        self.assertEqual(v["MV1"]["MapBlob"], g.rev_blob(m["eff_valid"], MAP))
        self.assertEqual(v["MV2"]["ClosedList"], SURFACES)
        self.assertEqual(v["MV3"]["ChangedUnderSurfaces"], sorted([AP, WF, SCHEMA, NEWJ]))
        for k in ("LiteralViolations", "EntryFilesOutsideSchema", "AddedPresentInEff1", "EntryFilePresentInEff1"):
            self.assertEqual(v["MV4"][k], [], k)
        ap_rec = [r for r in v["MV6"]["PerFile"] if r["Path"] == AP][0]
        self.assertEqual(ap_rec["DerivedModified"], sorted(["# Plan de prueba", S16_HEAD, S163_HEAD]))
        self.assertEqual(ap_rec["DerivedAddedOrEntry"], [R163])
        self.assertEqual(v["MV7"]["FirstSentenceS16"], POINTER)
        self.assertEqual(v["MV7"]["LastSentenceS163"], POINTER)
        self.assertTrue(v["MV7"]["S163EqualAfterPointerRemoval"])
        self.assertEqual(ba.map_part_defects(cm, internal), [])

    def test_defect_map_known_defects(self):
        g, m = FX["map"], FX["m"]
        repo = ba.RepoView(g.path)
        cm, internal = ba.acceptor_clausemap(repo, m["s_defect"])
        v = cm["Validate"]
        self.assertEqual(v["MV1"]["SchemaViolations"], [{"Pointer": "/Extra", "Keyword": "additionalProperties"}])
        self.assertEqual(v["MV3"]["OnlyInChanged"], sorted(["docs/adr/0002.md", XJ]))
        self.assertEqual(v["MV3"]["OnlyInFiles"], sorted(["AGENTS.md", "docs/adr/0001.md"]))
        self.assertEqual(v["MV3"]["DeletedUnderSurfaces"], ["docs/adr/0002.md"])
        self.assertEqual(v["MV4"]["LiteralViolations"], [AP])
        self.assertEqual(v["MV4"]["EntryFilesOutsideSchema"], ["AGENTS.md"])
        self.assertEqual(v["MV4"]["AddedPresentInEff1"], ["docs/adr/0001.md"])
        self.assertEqual(v["MV4"]["EntryFilePresentInEff1"], ["AGENTS.md"])
        self.assertEqual(v["MV5"]["EntryKeyDuplicates"], [{"Path": AP, "Section": S163_HEAD}])
        self.assertEqual(v["MV5"]["EntriesOnNonModifiedPath"], [{"Path": "AGENTS.md", "Section": "preámbulo"}])
        ap_rec = [r for r in v["MV6"]["PerFile"] if r["Path"] == AP][0]
        self.assertEqual(ap_rec["DerivedModified"], sorted(["# Plan de prueba", S16_HEAD, S163_HEAD]))
        self.assertEqual(ap_rec["MapModified"], sorted(["# Plan de prueba", S163_HEAD, S1610_HEAD]))
        self.assertEqual(v["MV6"]["EntryKindEntries"], [{"Path": AP, "Section": R163}])
        self.assertFalse(v["MV7"]["S163EqualAfterPointerRemoval"])
        self.assertTrue(v["MV7"]["PointerIsFirstSentenceS16"] and v["MV7"]["PointerIsLastSentenceS163"])
        defects = ba.map_part_defects(cm, internal)
        for label in ("MV-1: map invalid against its schema", "MV-3: OnlyInChanged", "MV-3: OnlyInFiles",
                      "MV-3: DeletedUnderSurfaces", "MV-4: LiteralViolations", "MV-4: EntryFilesOutsideSchema",
                      "MV-4: AddedPresentInEff1", "MV-5: EntryKeyDuplicates", "MV-5: EntriesOnNonModifiedPath",
                      "MV-6: DerivedModified != MapModified", "MV-6: ENTRY entries differ from the fixed pair",
                      "MV-7: 16.3 differs after pointer removal"):
            self.assertIn(label, defects)
        self.assertNotIn("MV-2: Surfaces differ from the closed list", defects)

    def test_activation_defects(self):
        g, m = FX["map"], FX["m"]
        repo = ba.RepoView(g.path)
        cm, i = ba.acceptor_clausemap(repo, m["s_dup"])
        self.assertEqual(ba.map_part_defects(cm, i), ["ACTIVATION: more than one trailer commit",
                                                      "ACTIVATION: entry point present without EFF"])
        cm, i = ba.acceptor_clausemap(repo, m["s_direct"])
        self.assertEqual(ba.map_part_defects(cm, i), ["ACTIVATION: trailer present but no derivable EFF"])
        cm, i = ba.acceptor_clausemap(repo, m["m_old"])
        self.assertEqual(ba.map_part_defects(cm, i), [])


# ============================================================================= end-to-end through the CLI

class TestEndToEnd(unittest.TestCase):

    def test_full_request_accepts(self):
        rc, raw, out, checks = run_acceptor(full_unit_request())
        self.assertEqual(statuses(checks), {"R1": "MATCH", "R2": "MATCH", "R3": "MATCH", "R4": "MATCH",
                                            "R5": "MATCH", "R6": "NOT_APPLICABLE", "R7": "MATCH", "R8": "MANUAL",
                                            "R9": "MATCH", "R10": "MATCH"}, json.dumps(out)[:3000])
        self.assertEqual(out["Decision"], "ACCEPT_FACTS")
        self.assertEqual(rc, 0)
        self.assertTrue(all(b < 0x80 for b in raw))
        self.assertNotIn(b"EXECUTION_", raw)

    # ---- R1
    def test_r1_wrong_pinned_blob(self):
        req = unit_request()
        req["Tool"]["PinnedBlob"] = "1" * 40
        rc, _, out, checks = run_acceptor(req)
        self.assertEqual(checks["R1"]["Status"], "MISMATCH")
        self.assertEqual(checks["R3"]["Status"], "MISMATCH")
        self.assertIn("not executed", json.dumps(checks["R3"]["Detail"]))
        self.assertEqual((out["Decision"], rc), ("REJECT_FACTS", 1))

    def test_r1_from_git_object_and_rerun_from_stdin(self):
        tg = G(os.path.join(WORK, "toolrepo-%d" % G.tick[0]))
        with open(FAKE_PY, "rb") as fh:
            tg.write("bmin_facts.py", fh.read())
        c = tg.commit("tool\n")
        blob = tg.rev_blob(c, "bmin_facts.py")
        self.assertEqual(blob, FX["fake_blob"])
        g, u = FX["unit"], FX["u"]
        argv = ["diff", "--repo", g.path, "--from", u["c0"], "--to", u["c3"]]
        env, _, _ = make_envelope(argv)
        req = base_request([{"Name": "diff", "Envelope": env, "Argv": argv}], Closure=unit_closure(),
                           Tool={"GitObject": {"GitDir": tg.path, "Blob": blob}, "PinnedBlob": blob,
                                 "RunFrom": "object"},
                           NotApplicable={"R4": "n/a", "R6": "n/a", "R7": "n/a"})
        rc, _, out, checks = run_acceptor(req)
        self.assertEqual(checks["R1"]["Status"], "MATCH")
        self.assertEqual(checks["R1"]["Detail"]["ObjectBlob"], blob)
        self.assertEqual(checks["R3"]["Status"], "MATCH", json.dumps(checks["R3"]))
        self.assertEqual(out["Decision"], "ACCEPT_FACTS", json.dumps(statuses(checks)))
        req["Tool"]["PinnedBlob"] = "2" * 40
        _, _, _, checks = run_acceptor(req)
        self.assertEqual(checks["R1"]["Status"], "MISMATCH")

    # ---- R2
    def test_r2_declared_version_or_exe_differs(self):
        req = unit_request()
        req["Interpreter"] = {"Python": "0.0.0", "ExecutableSha256": FX["exe_sha"]}
        _, _, out, checks = run_acceptor(req)
        self.assertEqual(checks["R2"]["Status"], "MISMATCH")
        req["Interpreter"] = {"Python": platform.python_version(), "ExecutableSha256": "0" * 64}
        _, _, out, checks = run_acceptor(req)
        self.assertEqual(checks["R2"]["Status"], "MISMATCH")
        self.assertEqual(out["Decision"], "REJECT_FACTS")

    def test_r2_non_isolated_acceptor(self):
        rc, _, out, checks = run_acceptor(unit_request(), isolated=False)
        self.assertEqual(checks["R2"]["Status"], "MISMATCH")
        self.assertIn("not isolated", json.dumps(checks["R2"]["Detail"]))
        self.assertEqual(out["Decision"], "REJECT_FACTS")

    def test_r2_envelope_runtime_not_isolated(self):
        req = unit_request()
        path = req["Runs"][0]["Envelope"]
        with open(path, "rb") as fh:
            env = json.loads(fh.read())
        env["Runtime"]["Isolated"] = False
        write_bytes(os.path.basename(path), (ba.canonical(env) + "\n").encode("ascii"))
        _, _, out, checks = run_acceptor(req)
        self.assertEqual(checks["R2"]["Status"], "MISMATCH")
        self.assertEqual(checks["R3"]["Status"], "MATCH")

    def test_r3_tool_run_without_isolation_is_rejected(self):
        g, u = FX["unit"], FX["u"]
        argv = ["diff", "--repo", g.path, "--from", u["c0"], "--to", u["c3"]]
        env, rc, raw = make_envelope(argv, isolated=False)
        self.assertEqual(rc, 2)
        self.assertIn(b"NOT_ISOLATED", raw)
        req = base_request([{"Name": "diff", "Envelope": env, "Argv": argv}], Closure=unit_closure(),
                           NotApplicable={"R4": "n/a", "R6": "n/a", "R7": "n/a"})
        _, _, out, checks = run_acceptor(req)
        self.assertEqual(checks["R3"]["Status"], "MISMATCH")
        self.assertEqual(out["Decision"], "REJECT_FACTS")

    # ---- R3: tampered output
    def _tamper(self, recompute):
        req = unit_request()
        path = req["Runs"][0]["Envelope"]
        with open(path, "rb") as fh:
            env = json.loads(fh.read())
        env["Facts"]["Paths"] = env["Facts"]["Paths"][:-1]
        if recompute:
            env["FactsSha256"] = hashlib.sha256(ba.canonical(env["Facts"]).encode("ascii")).hexdigest()
        write_bytes(os.path.basename(path), (ba.canonical(env) + "\n").encode("ascii"))
        return run_acceptor(req)

    def test_r3_tampered_facts_without_recomputed_sha(self):
        _, _, out, checks = self._tamper(False)
        self.assertEqual(checks["R3"]["Status"], "MISMATCH")
        self.assertIn("FactsSha256 differs", json.dumps(checks["R3"]["Detail"]))
        self.assertEqual(checks["R5"]["Status"], "MISMATCH")
        self.assertEqual(out["Decision"], "REJECT_FACTS")

    def test_r3_tampered_facts_with_recomputed_sha(self):
        _, _, out, checks = self._tamper(True)
        self.assertEqual(checks["R3"]["Status"], "MISMATCH")
        d = json.dumps(checks["R3"]["Detail"])
        self.assertNotIn("FactsSha256 differs from the SHA-256", d)
        self.assertIn("re-run differs in Facts", d)
        self.assertEqual(checks["R5"]["Status"], "MISMATCH")
        self.assertEqual(out["Decision"], "REJECT_FACTS")

    def test_r3_non_canonical_bytes(self):
        req = unit_request()
        path = req["Runs"][0]["Envelope"]
        with open(path, "rb") as fh:
            env = json.loads(fh.read())
        write_bytes(os.path.basename(path), (json.dumps(env, indent=1, sort_keys=True) + "\n").encode("ascii"))
        _, _, out, checks = run_acceptor(req)
        self.assertEqual(checks["R3"]["Status"], "MISMATCH")
        self.assertIn("not the canonical", json.dumps(checks["R3"]["Detail"]))

    def test_r3_error_envelope_bad_revision(self):
        g = FX["unit"]
        argv = ["diff", "--repo", g.path, "--from", "0" * 40, "--to", FX["u"]["c3"]]
        env, rc, raw = make_envelope(argv)
        self.assertEqual(rc, 2)
        req = base_request([{"Name": "bad", "Envelope": env, "Argv": argv}], Closure=unit_closure(),
                           NotApplicable={"R4": "n/a", "R6": "n/a", "R7": "n/a"})
        _, _, out, checks = run_acceptor(req)
        self.assertEqual(checks["R3"]["Status"], "MISMATCH")
        self.assertEqual(checks["R5"]["Status"], "MISMATCH")
        self.assertEqual(out["Decision"], "REJECT_FACTS")

    def test_r3_verdict_vocabulary_in_facts(self):
        g, u = FX["unit"], FX["u"]
        argv = ["diff", "--repo", g.path, "--from", u["c0"], "--to", u["c3"]]
        env, _, _ = make_envelope(argv, mutant="verdict")
        req = base_request([{"Name": "diff", "Envelope": env, "Argv": argv}], Closure=unit_closure(),
                           NotApplicable={"R4": "n/a", "R6": "n/a", "R7": "n/a"})
        _, _, out, checks = run_acceptor(req, mutant="verdict")
        self.assertEqual(checks["R3"]["Status"], "MISMATCH")
        self.assertIn("verdict vocabulary", json.dumps(checks["R3"]["Detail"]))

    # ---- R4
    def _r4(self, results, **kw):
        req = unit_request(Verification=verification(results, **kw))
        _, _, out, checks = run_acceptor(req)
        return checks["R4"], out

    def test_r4_each_check_flipped_is_mismatch(self):
        for name in ("Identity", "Scope", "CleanTree", "Trailer", "FreeText"):
            r4, out = self._r4({name: "fail"})
            self.assertEqual(r4["Status"], "MISMATCH", name)
            row = [r for r in r4["Detail"]["Checks"] if r["Check"] == name][0]
            self.assertEqual((row["Status"], row["AcceptorExpects"]), ("MISMATCH", "pass"), name)
            self.assertEqual(out["Decision"], "REJECT_FACTS")

    def test_r4_freetext_term_detected(self):
        h = handoff_obj(Evidence=["Todo listo: gate  pass"])
        r4, _ = self._r4({"FreeText": "fail"}, handoff=h)
        self.assertEqual(r4["Status"], "MATCH", json.dumps(r4))
        r4, _ = self._r4({}, handoff=h)
        self.assertEqual(r4["Status"], "MISMATCH")

    def test_r4_scope_trailer_identity_cleantree_failures(self):
        d = dict(unit_inputs()["deleg_obj"])
        d["AllowedWriteScope"] = ["src/", "docs/señal.md"]
        r4, _ = self._r4({"Scope": "fail"}, deleg=d)
        self.assertEqual(r4["Status"], "MATCH", json.dumps(r4))
        d2 = dict(unit_inputs()["deleg_obj"])
        d2["ForbiddenWriteScope"] = ["src/*.cs"]
        r4, _ = self._r4({"Scope": "not_run"}, deleg=d2)
        self.assertEqual(r4["Status"], "MATCH", json.dumps(r4))
        h = handoff_obj(Worker={"Trailer": "Co-Authored-By: Other <o@example.invalid>"})
        r4, _ = self._r4({"Trailer": "fail"}, handoff=h)
        self.assertEqual(r4["Status"], "MATCH", json.dumps(r4))
        h = handoff_obj(Branch="otra")
        r4, _ = self._r4({"Identity": "fail"}, handoff=h)
        self.assertEqual(r4["Status"], "MATCH", json.dumps(r4))
        h = handoff_obj(RedSha=FX["u"]["tlast"])
        r4, _ = self._r4({}, handoff=h)
        self.assertEqual(r4["Status"], "MISMATCH")
        g = FX["unit"]
        g.write("untracked.txt", "u\n")
        try:
            r4, _ = self._r4({"CleanTree": "fail"})
            self.assertEqual(r4["Status"], "MATCH", json.dumps(r4))
            r4, _ = self._r4({})
            self.assertEqual(r4["Status"], "MISMATCH")
        finally:
            g.remove("untracked.txt")
        r4, _ = self._r4({"CleanTree": "fail"}, ignore="not-ignored/x.json")
        self.assertEqual(r4["Status"], "MATCH", json.dumps(r4))

    def test_r4_authority_map_part(self):
        m = FX["m"]
        g = FX["map"]

        def run(main, result):
            v = verification({"Authority": result}, main=main, terms_rev=m["c0"])
            req = base_request([], repo_key="map", Verification=v)
            _, _, out, checks = run_acceptor(req)
            return [r for r in checks["R4"]["Detail"]["Checks"] if r["Check"] == "Authority"][0]

        row = run(m["s_defect"], "pass")
        self.assertEqual((row["Status"], row["AcceptorExpects"]), ("MISMATCH", "not pass"))
        self.assertIn("MV-4: AddedPresentInEff1", row["Defects"])
        self.assertEqual(run(m["s_defect"], "fail")["Status"], "MATCH")
        self.assertEqual(run(m["s_valid"], "pass")["Status"], "MATCH")
        self.assertEqual(run(m["s_valid"], "fail")["Status"], "MATCH")
        self.assertEqual(run(m["s_dup"], "pass")["Status"], "MISMATCH")
        self.assertTrue(g.path)

    # ---- R5
    def test_r5_mutants_drop_path_ancestry_trailer(self):
        for mutant, field in (("drop-path", "Paths"), ("ancestry-swap", "Ancestry"), ("trailer-first", "CoAuthoredBy")):
            req = unit_request(("identity", "diff"), mutant=mutant)
            _, _, out, checks = run_acceptor(req, mutant=mutant)
            self.assertEqual(checks["R3"]["Status"], "MATCH", mutant)
            self.assertEqual(checks["R5"]["Status"], "MISMATCH", mutant)
            self.assertIn('"Field": "' + field, json.dumps(checks["R5"]["Detail"]), mutant)
            self.assertEqual(out["Decision"], "REJECT_FACTS")

    def test_r5_trailer_cases_match_git(self):
        g, u = FX["unit"], FX["u"]
        argv = ["diff", "--repo", g.path, "--from", u["c0"], "--to", u["tlast"]]
        env, _, _ = make_envelope(argv)
        req = base_request([{"Name": "trailers", "Envelope": env, "Argv": argv}], Closure=unit_closure(),
                           NotApplicable={"R4": "n/a", "R6": "n/a", "R7": "n/a"})
        _, _, out, checks = run_acceptor(req)
        self.assertEqual(checks["R5"]["Status"], "MATCH", json.dumps(checks["R5"]))

    def test_r5_rename_both_paths_e6(self):
        g, u = FX["unit"], FX["u"]
        argv = ["diff", "--repo", g.path, "--from", u["c0"], "--to", u["r1"]]
        for mutant, want in ((None, "MATCH"), ("renames-on", "MISMATCH")):
            env, _, raw = make_envelope(argv, mutant=mutant)
            if mutant is None:
                self.assertEqual(json.loads(raw)["Facts"]["Paths"], ["src/keep.cs", "src/kept.cs"])
            req = base_request([{"Name": "rename", "Envelope": env, "Argv": argv}], Closure=unit_closure(),
                               NotApplicable={"R4": "n/a", "R6": "n/a", "R7": "n/a"})
            _, _, out, checks = run_acceptor(req, mutant=mutant)
            self.assertEqual(checks["R5"]["Status"], want, str(mutant))
            if mutant:
                self.assertIn("src/keep.cs", json.dumps(checks["R5"]["Detail"]))

    # ---- R6 / R7 (clause map)
    def _clausemap(self, s_key, mutant=None, cv=None, rc_expected=0, extra=None):
        g, m = FX["map"], FX["m"]
        argv = ["clausemap", "--repo", g.path, "--main", m[s_key]] + (extra or [])
        env, rc, raw = make_envelope(argv, mutant=mutant)
        self.assertEqual(rc, rc_expected, raw[:500])
        cl, cls = write_json("closure-map-%d.json" % G.tick[0], closure_obj("map", refs=["HEAD"]))
        over = {"Closure": {"Path": cl, "Sha256": cls}, "NotApplicable": {"R5": "n/a", "R10": "n/a"}}
        if cv is not None:
            over["Verification"] = cv
        else:
            over["NotApplicable"]["R4"] = "n/a"
        req = base_request([{"Name": "clausemap", "Envelope": env, "Argv": argv}], repo_key="map", **over)
        req.pop("RelayFacts")
        return run_acceptor(req, mutant=mutant)

    def test_e4_ambiguous_derive_and_claim_ids(self):
        m = FX["m"]
        # conformant tool: AMBIGUOUS error envelope -> R3 and R6 MISMATCH
        for key in ("s_dup", "s_direct"):
            _, _, out, checks = self._clausemap(key, rc_expected=2)
            self.assertEqual((checks["R3"]["Status"], checks["R6"]["Status"]), ("MISMATCH", "MISMATCH"), key)
            self.assertEqual(out["Decision"], "REJECT_FACTS")
        # a tool that emits facts anyway is caught by R6 even though it reproduces itself (R3 MATCH)
        for key in ("s_dup", "s_direct"):
            _, _, out, checks = self._clausemap(key, mutant="e4-off")
            self.assertEqual(checks["R3"]["Status"], "MATCH", key)
            self.assertEqual(checks["R6"]["Status"], "MISMATCH", key)
            self.assertIn("errata E4: activation is ambiguous", json.dumps(checks["R6"]["Detail"]))
        # no trailer is a fact (Eff null)
        _, _, out, checks = self._clausemap("m_old")
        self.assertEqual(checks["R6"]["Status"], "MATCH", json.dumps(checks["R6"])[:1500])
        # claim_id: repeats of one value are fine; two distinct values must fail closed
        state = ["--state-rev", m["st_same"], "--state-path", STATE]
        _, _, out, checks = self._clausemap("s_valid", extra=state)
        self.assertEqual(checks["R6"]["Status"], "MATCH", json.dumps(checks["R6"])[:1500])
        self.assertEqual(ba.claim_id_values("a:\n  claim_id: C-1 # x\n\tclaim_id:C-1\nclaim_id:\n# claim_id: Z\n"),
                         ["C-1", "C-1"])
        state = ["--state-rev", m["st_distinct"], "--state-path", STATE]
        _, _, out, checks = self._clausemap("s_valid", extra=state, rc_expected=2)
        self.assertEqual((checks["R3"]["Status"], checks["R6"]["Status"]), ("MISMATCH", "MISMATCH"))
        _, _, out, checks = self._clausemap("s_valid", extra=state, mutant="e4-off")
        self.assertEqual(checks["R3"]["Status"], "MATCH")
        self.assertEqual(checks["R6"]["Status"], "MISMATCH")
        self.assertIn("two or more distinct claim_id", json.dumps(checks["R6"]["Detail"]))

    def test_r6_r7_valid_and_defect_maps_match(self):
        for key in ("s_valid", "s_defect"):
            _, _, out, checks = self._clausemap(key)
            self.assertEqual(checks["R6"]["Status"], "MATCH", key + json.dumps(checks["R6"])[:2000])
            self.assertEqual(checks["R7"]["Status"], "MATCH", key)
            self.assertEqual(checks["R3"]["Status"], "MATCH", key)
            self.assertEqual(checks["R9"]["Status"], "MISMATCH", key)
            self.assertIn("stdin-driven read", json.dumps(checks["R9"]["Detail"]))

    def test_r6_r7_mutants(self):
        for mutant, key, r7 in (("mv4-merged", "s_defect", "MISMATCH"), ("pointer-line", "s_valid", "MATCH"),
                                ("sentence-boundary", "s_valid", "MATCH")):
            _, _, out, checks = self._clausemap(key, mutant=mutant)
            self.assertEqual(checks["R3"]["Status"], "MATCH", mutant)
            self.assertEqual(checks["R6"]["Status"], "MISMATCH", mutant)
            self.assertEqual(checks["R7"]["Status"], r7, mutant)
            self.assertEqual(out["Decision"], "REJECT_FACTS")

    def test_r6_authority_resolution(self):
        g, m = FX["map"], FX["m"]
        mblob = g.rev_blob(m["eff_valid"], MAP)
        good = [{"Path": AP, "Section": S163_HEAD, "Class": "EXTERNAL", "Mode": "COMPAT", "RevisionSha": m["m_old"],
                 "Parts": [], "EffectiveSha": m["eff_valid"], "ClauseMapBlob": mblob,
                 "UnitProtocol": "rackcad-protocol/I61"}]
        v = verification({}, main=m["s_valid"], terms_rev=m["c0"], ar=good)
        _, _, out, checks = self._clausemap("s_valid", cv=v)
        self.assertEqual(checks["R6"]["Status"], "MATCH", json.dumps(checks["R6"])[:1500])
        bad = [dict(good[0], EffectiveSha=m["eff_defect"])]
        v = verification({}, main=m["s_valid"], terms_rev=m["c0"], ar=bad)
        _, _, out, checks = self._clausemap("s_valid", cv=v)
        self.assertEqual(checks["R6"]["Status"], "MISMATCH")

    def test_r7_scope_and_contract_mutants(self):
        for mutant, field in (("coverage-inverted", "AllowedCoveredBy"), ("u19-off", "U19Normalized")):
            req = unit_request(("scope", "contract"), mutant=mutant)
            _, _, out, checks = run_acceptor(req, mutant=mutant)
            self.assertEqual(checks["R7"]["Status"], "MISMATCH", mutant)
            self.assertIn(field, json.dumps(checks["R7"]["Detail"]), mutant)
            self.assertEqual(out["Decision"], "REJECT_FACTS")

    # ---- R8 / R9 / R10 / declarations
    def test_r8_is_manual(self):
        _, _, out, checks = run_acceptor(unit_request())
        self.assertEqual(checks["R8"]["Status"], "MANUAL")

    def test_r9_closure_violations(self):
        req = unit_request()
        req["Closure"] = unit_closure(drop=FX["u"]["c0"])
        _, _, out, checks = run_acceptor(req)
        self.assertEqual(checks["R9"]["Status"], "MISMATCH")
        self.assertIn("outside the closure", json.dumps(checks["R9"]["Detail"]))
        req = unit_request(("identity",))
        req["Closure"] = unit_closure(refs=["HEAD"])
        _, _, out, checks = run_acceptor(req)
        self.assertEqual(checks["R9"]["Status"], "MISMATCH")
        req = unit_request(mutant="extra-gitcall")
        _, _, out, checks = run_acceptor(req, mutant="extra-gitcall")
        self.assertEqual(checks["R3"]["Status"], "MATCH")
        self.assertEqual(checks["R9"]["Status"], "MISMATCH")
        self.assertIn("config", json.dumps(checks["R9"]["Detail"]))
        u = FX["u"]
        obj = closure_obj("unit", revpaths=[{"Rev": u["c3"], "Path": "src/a.cs"}],
                          files=[{"Name": "DELEG", "Sha256": "0" * 64}])
        p, s = write_json("closure-bad-files.json", obj)
        req = unit_request(("scope",))
        req["Closure"] = {"Path": p, "Sha256": s}
        _, _, out, checks = run_acceptor(req)
        self.assertEqual(checks["R9"]["Status"], "MISMATCH")
        self.assertEqual(out["Decision"], "REJECT_FACTS")

    def test_r10_and_not_applicable_rules(self):
        req = unit_request()
        req["RelayFacts"]["CitedSha256"] = "f" * 64
        _, _, out, checks = run_acceptor(req)
        self.assertEqual(checks["R10"]["Status"], "MISMATCH")
        req = unit_request()
        req.pop("RelayFacts")
        _, _, out, checks = run_acceptor(req)
        self.assertEqual(checks["R10"]["Status"], "MISMATCH")
        self.assertEqual(out["Decision"], "REJECT_FACTS")
        req["NotApplicable"]["R10"] = "no relay in this session"
        _, _, out, checks = run_acceptor(req)
        self.assertEqual(checks["R10"]["Status"], "NOT_APPLICABLE")
        self.assertEqual(out["Decision"], "ACCEPT_FACTS")
        req = unit_request()
        req["NotApplicable"]["R1"] = "attempted waiver"
        _, _, out, checks = run_acceptor(req)
        self.assertEqual(checks["R1"]["Status"], "MISMATCH")
        req = unit_request()
        req["NotApplicable"]["R10"] = "contradictory"
        _, _, out, checks = run_acceptor(req)
        self.assertEqual(checks["R10"]["Status"], "MISMATCH")

    def test_fail_closed_missing_data_and_usage(self):
        req = base_request([], NotApplicable={"R4": "x", "R5": "x", "R6": "x", "R7": "x"})
        rc, _, out, checks = run_acceptor(req)
        self.assertEqual(checks["R3"]["Status"], "MISMATCH")
        self.assertEqual(checks["R9"]["Status"], "MISMATCH")
        self.assertEqual((out["Decision"], rc), ("REJECT_FACTS", 1))
        req = unit_request()
        req.pop("Interpreter")
        _, _, out, checks = run_acceptor(req)
        self.assertEqual(checks["R2"]["Status"], "MISMATCH")
        p = subprocess.run([sys.executable, "-I", ACCEPT_PY, "--bogus", "1"], stdout=PIPE, stderr=PIPE)
        self.assertEqual(p.returncode, 64)
        self.assertEqual(json.loads(p.stdout)["Decision"], "REJECT_FACTS")
        p = subprocess.run([sys.executable, "-I", ACCEPT_PY, "--request", os.path.join(INPUTS, "absent.json")],
                           stdout=PIPE, stderr=PIPE)
        self.assertEqual(p.returncode, 64)

    def test_cli_flags_and_envelope_shortcut(self):
        g, u = FX["unit"], FX["u"]
        argv = ["diff", "--repo", g.path, "--from", u["c0"], "--to", u["c3"]]
        env, _, _ = make_envelope(argv)
        cl = unit_closure()
        args = [sys.executable, "-I", ACCEPT_PY, "--repo", g.path, "--tool", FAKE_PY, "--tool-blob", FX["fake_blob"],
                "--python-version", platform.python_version(), "--python-exe-sha256", FX["exe_sha"],
                "--closure", cl["Path"], "--closure-sha256", cl["Sha256"], "--relay-facts", FX["relay"][0],
                "--relay-sha256", FX["relay"][1], "--na", "R4=none", "--na", "R6=none", "--na", "R7=none",
                "--envelope", env, "--"] + argv
        p = subprocess.run(args, stdout=PIPE, stderr=PIPE, env=tool_env())
        out = json.loads(p.stdout)
        self.assertEqual(out["Decision"], "ACCEPT_FACTS", p.stdout[:2000])
        self.assertEqual(p.returncode, 0)


# ============================================================================= errata v0.2 (SPEC section 5)

def _envelope_bytes(command, facts):
    env = {"Schema": ba.FACTS_SCHEMA, "Command": command, "Args": {}, "Inputs": [], "GitCalls": [], "Facts": facts,
           "FactsSha256": hashlib.sha256(ba.canonical(facts).encode("ascii")).hexdigest(), "Error": None,
           "Runtime": {"Python": platform.python_version(), "Isolated": True}}
    return (ba.canonical(env) + "\n").encode("ascii")


def _vocab_problems(command, facts):
    _, probs = ba.envelope_problems(_envelope_bytes(command, facts), [command])
    return [p for p in probs if "vocabulary" in p]


class TestErrataV02(unittest.TestCase):

    def test_e1_vocabulary_exempts_verbatim_echoes_only(self):
        trx = {"Sha256": "0" * 64, "Counters": {"total": "3", "passed": "2", "failed": "1", "passedButRunAborted": "0"},
               "Tests": [{"TestName": "T1", "Outcome": "Failed"}, {"TestName": "T2", "Outcome": "pass"}],
               "FailedTests": ["T1"], "FilterSelected": 1, "FilterFailed": 0}
        self.assertEqual(_vocab_problems("trx", trx), [])
        self.assertEqual(_vocab_problems("trx", dict(trx, Result="pass")), ["verdict vocabulary in Facts at /Result"])
        bad_test = dict(trx, Tests=[{"TestName": "T1", "Outcome": "Failed", "passFlag": True}])
        self.assertEqual(len(_vocab_problems("trx", bad_test)), 1)
        self.assertEqual(len(_vocab_problems("hashes", {"Counters": {"passed": "2"}})), 1)
        self.assertEqual(_vocab_problems("read-text", {"Blob": "0" * 40, "Lines": [{"N": 1, "Text": "STOP"}],
                                                      "NonAsciiCount": 0, "NonAsciiChars": []}), [])
        self.assertEqual(len(_vocab_problems("read-text", {"Lines": [], "Status": "STOP"})), 1)
        self.assertEqual(_vocab_problems("read-json", {"Values": {"/pass": "FAIL", "/a": {"VERIFIED": "STOP"}}}), [])
        self.assertEqual(_vocab_problems("handoff", {"Present": True, "Fields": {"WorkerStatus": "BLOCKED"}}), [])
        self.assertEqual(_vocab_problems("freetext", {"Terms": ["pass"], "Fields": [],
                                                     "Matches": [{"Field": "Evidence", "Term": "STOP", "Count": 1}]}), [])
        cm = {"Derive": {}, "Pre": {"HeaderCells": ["Initiative", "PASS"], "Rows": [["I-1", "STOP"]],
                                    "ClaimIdCounts": {"pass-1": 1}},
              "Validate": {"MV7": {"FirstSentenceS16": "STOP", "LastSentenceS163": "pass", "S163Eff1": "FAIL",
                                   "S163EffMinusPointer": "REWORK"}, "MV3": {"Note": "STOP"}}}
        self.assertEqual(_vocab_problems("clausemap", cm), ["verdict vocabulary in Facts at /Validate/MV3/Note"])

    def test_e2_u19_normalized_objects_strict(self):
        g, u = FX["unit"], FX["u"]
        s = ba.acceptor_scope(ba.RepoView(g.path), u["c0"], u["c3"], unit_inputs()["deleg_obj"])
        want = [{"Entry": "tests/**", "List": "AllowedWriteScope", "Prefix": "tests/"},
                {"Entry": "build/**", "List": "ForbiddenWriteScope", "Prefix": "build/"}]
        self.assertEqual(s["U19Normalized"], want)
        cmp = ba.Cmp()
        self.assertFalse(cmp.eq("U19Normalized", want, list(reversed(want))))
        self.assertFalse(cmp.eq("U19Normalized", want, ["tests/**", "build/**"]))
        for mutant, status in ((None, "MATCH"), ("u19-legacy", "MISMATCH")):
            _, _, out, checks = run_acceptor(unit_request(("scope",), mutant=mutant), mutant=mutant)
            self.assertEqual(checks["R7"]["Status"], status, str(mutant))
            if mutant:
                self.assertIn("U19Normalized", json.dumps(checks["R7"]["Detail"]))

    def test_e3_literal_pathspec_in_r9(self):
        g, u = FX["unit"], FX["u"]
        repo = ba.RepoView(g.path)
        cl = ba.Closure(closure_obj("unit", revpaths=[{"Rev": u["c3"], "Path": "src/a.cs"}]), repo)
        pre = ["-C", g.path, "-c", "core.quotepath=false", "-c", "core.fsmonitor=false"]
        fits = [pre + ["ls-tree", "-z", u["c3"], ":(literal)src/a.cs"],
                pre + ["diff", "--no-ext-diff", "--no-textconv", "--name-only", "--no-renames", u["c0"], u["c3"], "--",
                       ":(literal)src/a.cs"],
                pre + ["diff", "--no-ext-diff", "--no-textconv", "--name-only", u["c0"], u["c3"], ":(literal)src/a.cs"],
                pre + ["ls-tree", "-z", u["c3"], "src/a.cs"]]
        for call in fits:
            self.assertEqual(ba.fit_gitcall(call, cl, repo, g.path), [], call)
        for tok, msg in ((":(glob)src/*", "pathspec magic not allowed"), (":(literal,icase)src/a.cs", "pathspec magic"),
                         (":!src/a.cs", "pathspec magic"), (":/src/a.cs", "pathspec magic"),
                         (":(literal)src/b.cs", "path outside the closure: src/b.cs")):
            v = ba.fit_gitcall(pre + ["ls-tree", "-z", u["c3"], tok], cl, repo, g.path)
            self.assertTrue(v and msg in v[0], (tok, v))
        for mutant, status in ((None, "MATCH"), ("glob-pathspec", "MISMATCH")):
            _, _, out, checks = run_acceptor(unit_request(("hashes",), mutant=mutant), mutant=mutant)
            self.assertEqual(checks["R9"]["Status"], status, json.dumps(checks["R9"]))
            self.assertEqual(checks["R5"]["Status"], "MATCH")
            if mutant:
                self.assertIn("pathspec magic not allowed", json.dumps(checks["R9"]["Detail"]))


# ============================================================================= hygiene

def _snapshot(paths, repo):
    e = G.env()
    snap = {}
    with open(os.path.join(repo, ".git", "index"), "rb") as fh:
        snap["index"] = hashlib.sha256(fh.read()).hexdigest()
    snap["refs"] = subprocess.run(["git", "-C", repo, "for-each-ref"], stdout=PIPE, env=e).stdout
    snap["status"] = subprocess.run(["git", "-C", repo, "status", "--porcelain", "--ignored"], stdout=PIPE,
                                    env=e).stdout
    files = []
    for root in paths:
        for dirpath, dirnames, filenames in os.walk(root):
            for f in filenames:
                full = os.path.join(dirpath, f)
                files.append((os.path.relpath(full, root), os.path.getsize(full)))
    snap["files"] = sorted(files)
    return snap


class TestHygiene(unittest.TestCase):

    def test_zero_writes(self):
        g = FX["unit"]
        req = full_unit_request()
        rpath = write_bytes("req-zero.json", (json.dumps(req) + "\n").encode("utf-8"))
        watched = [g.path, ACCEPT_DIR, INPUTS]
        before = _snapshot(watched, g.path)
        for _ in range(1):
            p = subprocess.run([sys.executable, "-I", ACCEPT_PY, "--request", rpath], stdout=PIPE, stderr=PIPE,
                               env=tool_env())
            self.assertEqual(json.loads(p.stdout)["Decision"], "ACCEPT_FACTS")
        after = _snapshot(watched, g.path)
        self.assertEqual(before, after)
        gm = FX["map"]
        before = _snapshot([gm.path], gm.path)
        repo = ba.RepoView(gm.path)
        ba.acceptor_clausemap(repo, FX["m"]["s_defect"])
        self.assertEqual(before, _snapshot([gm.path], gm.path))

    def test_determinism_and_ascii(self):
        g = FX["unit"]
        req = unit_request()
        rpath = write_bytes("req-det.json", (json.dumps(req) + "\n").encode("utf-8"))
        outs = [subprocess.run([sys.executable, "-I", ACCEPT_PY, "--request", rpath], stdout=PIPE,
                               env=tool_env()).stdout for _ in range(2)]
        self.assertEqual(outs[0], outs[1])
        self.assertTrue(all(b < 0x80 for b in outs[0]))
        self.assertTrue(outs[0].endswith(b"\n"))
        self.assertEqual(outs[0], (ba.canonical(json.loads(outs[0])) + "\n").encode("ascii"))
        for b in (b"EXECUTION_", b"VERIFIED"):
            self.assertNotIn(b, outs[0])
        self.assertTrue(g.path)

    def test_ast_no_write_network_or_foreign_process(self):
        with open(ACCEPT_PY, "rb") as fh:
            tree = ast.parse(fh.read().decode("utf-8"))
        imports = set()
        for node in ast.walk(tree):
            if isinstance(node, ast.Import):
                imports.update(a.name.split(".")[0] for a in node.names)
            elif isinstance(node, ast.ImportFrom):
                imports.add((node.module or "").split(".")[0])
        self.assertEqual(imports, {"hashlib", "json", "os", "platform", "re", "subprocess", "sys"})
        os_forbidden = {"remove", "unlink", "rename", "replace", "renames", "mkdir", "makedirs", "rmdir", "removedirs",
                        "open", "system", "popen", "chmod", "chown", "symlink", "link", "truncate", "startfile",
                        "putenv", "unsetenv", "chdir", "fork", "kill", "spawnl", "spawnv", "execv", "execl",
                        "write", "mkfifo", "utime"}
        path_writers = {"write_text", "write_bytes", "touch", "rmdir", "unlink", "mkdir", "writelines"}
        subprocess_funcs = set()
        for node in ast.walk(tree):
            if isinstance(node, (ast.Assign, ast.AugAssign)):
                targets = node.targets if isinstance(node, ast.Assign) else [node.target]
                for t in targets:
                    self.assertFalse(isinstance(t, ast.Subscript) and "environ" in ast.dump(t.value), "environ write")
            if not isinstance(node, ast.Call):
                continue
            f = node.func
            if isinstance(f, ast.Name):
                self.assertNotIn(f.id, ("eval", "exec", "compile", "__import__"))
                if f.id == "open":
                    mode = node.args[1] if len(node.args) > 1 else None
                    for kw in node.keywords:
                        if kw.arg == "mode":
                            mode = kw.value
                    self.assertTrue(isinstance(mode, ast.Constant) and mode.value == "rb", "open must be 'rb'")
            if isinstance(f, ast.Attribute):
                if isinstance(f.value, ast.Name) and f.value.id == "os":
                    self.assertNotIn(f.attr, os_forbidden, "os." + f.attr)
                self.assertNotIn(f.attr, path_writers, f.attr)
                if f.attr == "write":
                    self.assertEqual(ast.dump(f.value), ast.dump(ast.parse("sys.stdout.buffer", mode="eval").body))
                if isinstance(f.value, ast.Name) and f.value.id == "subprocess":
                    self.assertEqual(f.attr, "run")
                    subprocess_funcs.add(node.lineno)
                    for kw in node.keywords:
                        if kw.arg == "shell":
                            self.assertTrue(isinstance(kw.value, ast.Constant) and kw.value.value is False)
                    self.assertIn("shell", [kw.arg for kw in node.keywords])
                    self.assertFalse(isinstance(node.args[0], ast.Constant))
        owners = set()
        for fn in ast.walk(tree):
            if isinstance(fn, ast.FunctionDef):
                for sub in ast.walk(fn):
                    if isinstance(sub, ast.Call) and getattr(sub, "lineno", None) in subprocess_funcs:
                        owners.add(fn.name)
        self.assertEqual(owners, {"git_run", "rerun_tool"})
        with open(ACCEPT_PY, "rb") as fh:
            src = fh.read().decode("utf-8")
        self.assertNotIn("shell=True", src)
        for word in ("socket", "urllib", "http.client", "ssl", "tempfile", "shutil"):
            self.assertNotIn("import " + word, src)

    def test_git_executor_closed_list(self):
        with self.assertRaises(ba.AcceptError) as cm:
            ba.git_run(FX["unit"].path, "config", ["--list"])
        self.assertEqual(cm.exception.code, "FORBIDDEN_OPERATION")
        for sub, args in (("fetch", []), ("push", []), ("status", []), ("diff", ["HEAD"]), ("log", ["--output=x"])):
            with self.assertRaises(ba.AcceptError):
                ba.git_run(FX["unit"].path, sub, args)


if __name__ == "__main__":
    unittest.main(verbosity=2)
