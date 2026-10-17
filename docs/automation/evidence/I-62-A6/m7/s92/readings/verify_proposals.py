# -*- coding: utf-8 -*-
"""verify_proposals.py - comprobacion mecanica de proposals.md / proposals.json (I-62, decisiones 92, punto 5).

Mismas reglas que verify_r12.py, sobre proposals-citations.json:
  1. blob de cada fuente (SHA-1 de 'blob <len>NUL<bytes>' en W, --no-replace-objects) y su SHA-256 si consta;
  2. cada cita de texto sobre sus lineas (un marcador '>' inicial quitado por linea, espacios colapsados);
  3. cada hecho de sonda en probes/probe-proposals-result.json;
  4. proposals.md: cada «cita» [ID] (CLAVE@blob8:lineas) igual a la registrada; cada «termino» etiquetado contenido en su
     cita; cada «termino» sin etiqueta contenido en alguna cita; toda etiqueta existe; toda cita se usa;
  5. proposals.json: 8 filas INDETERMINATE con opcion (a) o (b) y comprobacion; la limitacion de L-18; la tabla
     consolidada de proposals.md coincide; SHA-256 de la sonda;
  6. higiene de todos los archivos bajo r12 (sin nombre de usuario de Windows ni direcciones de correo).
Escribe proposals-verify-result.json. Salida 0 solo si todo pasa.

Uso:  python -I verify_proposals.py
"""

import hashlib
import json
import os
import re
import subprocess
import sys

HERE = os.path.dirname(os.path.abspath(__file__))
W = os.path.join(os.environ["USERPROFILE"], ".codex", "worktrees",
                 "architecture-portabilidad-coordinador-principal")
CIT = os.path.join(HERE, "proposals-citations.json")
MD = os.path.join(HERE, "proposals.md")
JS = os.path.join(HERE, "proposals.json")
PROBE_RESULT = os.path.join(HERE, "probes", "probe-proposals-result.json")
PROBE_SCRIPT = os.path.join(HERE, "probes", "probe_proposals.py")
OUT = os.path.join(HERE, "proposals-verify-result.json")
SKIP = ("r12-verify-result.json", "proposals-verify-result.json")

QUOTE_RE = re.compile(r"«([^«»]+)»\s*\[([CKTRP]-\d+)\]\s*\(([A-Z0-9]+)@([0-9a-f]{8}):(\d+)(?:-(\d+))?\)")
ANYQ_RE = re.compile(r"«([^«»]+)»(\s*\[([CKTRP]-\d+)\])?(\s*\([A-Z0-9]+@[0-9a-f]{8}:\d+(?:-\d+)?\))?")
TAG_RE = re.compile(r"\[([CKTRP]-\d+)\]")
EMAIL_RE = re.compile(r"[A-Za-z0-9._%+-]+@[A-Za-z0-9-]+(?:\.[A-Za-z0-9-]+)*\.[A-Za-z]{2,}")
BQ_RE = re.compile(r"^\s*>\s?")
INDETERMINATE = ["L-01", "L-04", "L-10", "L-12", "L-13", "L-16", "L-23", "L-24"]

problems = []
checks = {"Sources": 0, "TextCitations": 0, "ProbeFacts": 0, "MdQuotes": 0, "MdTermQuotes": 0, "MdTags": 0,
          "JsonRows": 0, "HygieneFiles": 0}


def fail(msg):
    problems.append(msg)


def sha256(data):
    return hashlib.sha256(data).hexdigest()


def git_blob(oid):
    p = subprocess.run(["git", "-C", W, "--no-replace-objects", "cat-file", "blob", oid],
                       stdout=subprocess.PIPE, stderr=subprocess.PIPE)
    if p.returncode != 0:
        return None
    if hashlib.sha1(b"blob " + str(len(p.stdout)).encode("ascii") + b"\0" + p.stdout).hexdigest() != oid:
        return None
    return p.stdout


def collapse(text):
    return " ".join(text.split())


def lines_text(data, a, b):
    lines = data.decode("utf-8").split("\n")
    if a < 1 or b > len(lines) or a > b:
        return None
    return collapse(" ".join(BQ_RE.sub("", ln, count=1) for ln in lines[a - 1:b]))


def pointer(obj, ptr):
    cur = obj
    for tok in ptr.split("/")[1:]:
        tok = tok.replace("~1", "/").replace("~0", "~")
        cur = cur[int(tok)] if isinstance(cur, list) else cur[tok]
    return cur


cit = json.loads(open(CIT, "rb").read().decode("utf-8"))
sources = cit["Sources"]
blobs = {}
for key, src in sorted(sources.items()):
    data = git_blob(src["Blob"])
    if data is None:
        fail("source %s: blob not readable or id mismatch" % key)
        continue
    if "Sha256" in src and sha256(data) != src["Sha256"]:
        fail("source %s: SHA-256 differs" % key)
    blobs[key] = data
    checks["Sources"] += 1

by_id = {}
probe = json.loads(open(PROBE_RESULT, "rb").read().decode("ascii"))
for c in cit["Citations"]:
    if c["Id"] in by_id:
        fail("duplicate citation id " + c["Id"])
    by_id[c["Id"]] = c
    if c.get("Kind") == "probe":
        try:
            val = pointer(probe, c["Pointer"])
            if "Each" in c:
                if not isinstance(val, list) or not val:
                    fail("%s: pointer is not a non-empty list" % c["Id"])
                for i, item in enumerate(val):
                    if pointer(item, c["Each"]) != c["Expected"]:
                        fail("%s: element %d differs" % (c["Id"], i))
            elif val != c["Expected"]:
                fail("%s: probe value differs" % c["Id"])
        except (KeyError, IndexError, ValueError, TypeError):
            fail("%s: pointer not resolvable" % c["Id"])
        checks["ProbeFacts"] += 1
        continue
    key = c["Source"]
    if key not in blobs:
        fail("%s: unknown or unreadable source %s" % (c["Id"], key))
        continue
    a, b = c["Lines"][0], c["Lines"][-1]
    txt = lines_text(blobs[key], a, b)
    q = collapse(c["Quote"])
    if txt is None:
        fail("%s: lines out of range" % c["Id"])
    elif c.get("Mode", "quote") == "quote" and q not in txt:
        fail("%s: quote not found on %s:%d-%d" % (c["Id"], key, a, b))
    elif c.get("Mode") == "absent" and q in txt:
        fail("%s: token present on %s:%d-%d" % (c["Id"], key, a, b))
    checks["TextCitations"] += 1

md = open(MD, "rb").read().decode("utf-8")
for m in QUOTE_RE.finditer(md):
    quote, cid, key, b8, a, b = m.group(1), m.group(2), m.group(3), m.group(4), int(m.group(5)), m.group(6)
    b = int(b) if b else a
    c = by_id.get(cid)
    checks["MdQuotes"] += 1
    if c is None or c.get("Kind") == "probe":
        fail("md: %s is not a text citation" % cid)
        continue
    if collapse(quote) != collapse(c["Quote"]):
        fail("md: quote of %s differs from the registered quote" % cid)
    if key != c["Source"] or not sources[key]["Blob"].startswith(b8) or [a, b] != [c["Lines"][0], c["Lines"][-1]]:
        fail("md: location of %s differs" % cid)
text_quotes = [collapse(c["Quote"]) for c in by_id.values() if c.get("Kind") != "probe"]
for m in ANYQ_RE.finditer(md):
    if m.group(4):
        continue
    q = collapse(m.group(1))
    checks["MdTermQuotes"] += 1
    if m.group(3):
        c = by_id.get(m.group(3))
        if c is None or c.get("Kind") == "probe" or q not in collapse(c["Quote"]):
            fail("md: tagged quote not contained in %s: %s" % (m.group(3), q[:60]))
    elif not any(q in t for t in text_quotes):
        fail("md: untagged quote not found in any citation: " + q[:60])
tags = set(TAG_RE.findall(md))
for t in sorted(tags):
    checks["MdTags"] += 1
    if t not in by_id:
        fail("md: tag [%s] not in citations" % t)
unused = sorted(set(by_id) - tags)
if unused:
    fail("citations never referenced in proposals.md: " + ",".join(unused))

js = json.loads(open(JS, "rb").read().decode("utf-8"))
rows = js["Rows"]
if [r["Id"] for r in rows] != INDETERMINATE:
    fail("json: rows are not the eight INDETERMINATE rows in order")
table = dict((r, (o, k)) for r, o, k in re.findall(r"^\| (L-\d\d) \| \((a|b|lim)\) \| (FC-\d\d) \|", md, re.M))
for r in rows:
    checks["JsonRows"] += 1
    if r["Option"] not in ("a", "b"):
        fail("json: %s option" % r["Id"])
    if r["Option"] == "b" and not r.get("Check"):
        fail("json: %s without check" % r["Id"])
    for k in ("Authorities", "Evidence"):
        for cid in r.get(k, []):
            if cid not in by_id:
                fail("json: %s references unknown %s" % (r["Id"], cid))
    if table.get(r["Id"]) != (r["Option"], r["Check"]):
        fail("md consolidated table and json differ for " + r["Id"])
lim = js["Limitation"]
if lim["Id"] != "L-18" or table.get("L-18") != ("lim", lim["Check"]):
    fail("json/md: L-18 limitation differs")
for cid in lim.get("Authorities", []) + lim.get("Evidence", []):
    if cid not in by_id:
        fail("json: L-18 references unknown " + cid)
if len(table) != 9:
    fail("md consolidated table does not have 9 rows")
pr = js.get("Probe", {})
if pr.get("Script", {}).get("Sha256") != sha256(open(PROBE_SCRIPT, "rb").read()):
    fail("json: probe script SHA-256 differs")
if pr.get("Result", {}).get("Sha256") != sha256(open(PROBE_RESULT, "rb").read()):
    fail("json: probe result SHA-256 differs")

user = os.environ.get("USERNAME", "")
files = {}
for root, _dirs, names in os.walk(HERE):
    for n in names:
        p = os.path.join(root, n)
        rel = os.path.relpath(p, HERE).replace(os.sep, "/")
        if rel in SKIP or "/__pycache__/" in "/" + rel:
            continue
        data = open(p, "rb").read()
        files[rel] = sha256(data)
        txt = data.decode("utf-8", "replace")
        if user and user.lower() in txt.lower():
            fail("hygiene: user name in " + rel)
        if EMAIL_RE.search(txt):
            fail("hygiene: e-mail address in " + rel)
        checks["HygieneFiles"] += 1

result = {"Schema": "i62-r12-proposals-verify/1", "Pass": not problems, "Checks": checks, "Problems": problems,
          "Files": dict(sorted(files.items()))}
with open(OUT, "wb") as fh:
    fh.write((json.dumps(result, ensure_ascii=True, sort_keys=True, indent=1) + "\n").encode("ascii"))
print(json.dumps({"Pass": not problems, "Checks": checks, "Problems": problems[:40]}, ensure_ascii=True, indent=1))
sys.exit(0 if not problems else 1)
