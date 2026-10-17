# -*- coding: utf-8 -*-
"""verify_r12.py - comprobacion mecanica de r12 (I-62, decisiones 91, punto 4). Solo lectura.

Comprueba, contra los bytes fijados por blob (git cat-file del worktree W, --no-replace-objects):
  1. cada fuente de r12-citations.json: el SHA-1 de 'blob <len>NUL<bytes>' es el blob declarado;
  2. cada cita de texto: con los marcadores de cita Markdown iniciales ('>') quitados linea a linea, las
     lineas citadas unidas y con los espacios colapsados contienen la cita colapsada (modo quote) o no
     contienen el token (modo absent);
  3. cada hecho de sonda: el valor del puntero JSON en probes/probe-r12-result.json es el esperado;
  4. r12.md: cada «cita» [ID] (CLAVE@blob8:lineas) es igual a la cita registrada y nombra su fuente, su
     blob y sus lineas; toda etiqueta [ID] existe;
  5. r12.json: identificadores existentes, recuentos coherentes con las filas, disposiciones iguales a
     las de la tabla resumen de r12.md, SHA-256 de las sondas;
  6. higiene: ningun archivo bajo r12 contiene el nombre de usuario de Windows ni una direccion de correo.
Escribe r12-verify-result.json con el resultado y el SHA-256 de cada archivo. Salida 0 solo si todo pasa.

Uso:  python -I verify_r12.py
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
CIT = os.path.join(HERE, "r12-citations.json")
MD = os.path.join(HERE, "r12.md")
JS = os.path.join(HERE, "r12.json")
PROBE_RESULT = os.path.join(HERE, "probes", "probe-r12-result.json")
PROBE_SCRIPT = os.path.join(HERE, "probes", "probe_r12.py")
OUT = os.path.join(HERE, "r12-verify-result.json")

QUOTE_RE = re.compile(r"«([^«»]+)»\s*\[([CKTRP]-\d+)\]\s*\(([A-Z0-9]+)@([0-9a-f]{8}):(\d+)(?:-(\d+))?\)")
TAG_RE = re.compile(r"\[([CKTRP]-\d+)\]")
EMAIL_RE = re.compile(r"[A-Za-z0-9._%+-]+@[A-Za-z0-9-]+(?:\.[A-Za-z0-9-]+)*\.[A-Za-z]{2,}")
BQ_RE = re.compile(r"^\s*>\s?")
DISPOSITIONS = ("COMPATIBLE", "INCOMPATIBLE", "INDETERMINATE")

problems = []
checks = {"Sources": 0, "TextCitations": 0, "ProbeFacts": 0, "MdQuotes": 0, "MdTags": 0, "JsonRows": 0,
          "HygieneFiles": 0}


def fail(msg):
    problems.append(msg)


def sha256(data):
    return hashlib.sha256(data).hexdigest()


def git_blob(oid):
    p = subprocess.run(["git", "-C", W, "--no-replace-objects", "cat-file", "blob", oid],
                       stdout=subprocess.PIPE, stderr=subprocess.PIPE)
    if p.returncode != 0:
        return None
    h = hashlib.sha1()
    h.update(b"blob " + str(len(p.stdout)).encode("ascii") + b"\0" + p.stdout)
    if h.hexdigest() != oid:
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
    for tok in [t for t in ptr.split("/")[1:]]:
        tok = tok.replace("~1", "/").replace("~0", "~")
        if isinstance(cur, list):
            cur = cur[int(tok)]
        else:
            cur = cur[tok]
    return cur


cit = json.loads(open(CIT, "rb").read().decode("utf-8"))
sources = cit["Sources"]
blobs = {}
for key, src in sorted(sources.items()):
    data = git_blob(src["Blob"])
    if data is None:
        fail("source %s: blob %s not readable or id mismatch" % (key, src["Blob"]))
        continue
    if "Sha256" in src and sha256(data) != src["Sha256"]:
        fail("source %s: SHA-256 differs" % key)
    blobs[key] = data
    checks["Sources"] += 1

by_id = {}
for c in cit["Citations"]:
    if c["Id"] in by_id:
        fail("duplicate citation id " + c["Id"])
    by_id[c["Id"]] = c
    if c.get("Kind", "text") == "probe":
        continue
    key = c["Source"]
    if key not in blobs:
        fail("%s: unknown or unreadable source %s" % (c["Id"], key))
        continue
    a, b = c["Lines"][0], c["Lines"][-1]
    txt = lines_text(blobs[key], a, b)
    if txt is None:
        fail("%s: lines %d-%d out of range" % (c["Id"], a, b))
        continue
    q = collapse(c["Quote"])
    mode = c.get("Mode", "quote")
    if mode == "quote" and q not in txt:
        fail("%s: quote not found on %s:%d-%d" % (c["Id"], key, a, b))
    elif mode == "absent" and q in txt:
        fail("%s: token present on %s:%d-%d" % (c["Id"], key, a, b))
    checks["TextCitations"] += 1

probe = json.loads(open(PROBE_RESULT, "rb").read().decode("ascii"))
for c in cit["Citations"]:
    if c.get("Kind") != "probe":
        continue
    try:
        val = pointer(probe, c["Pointer"])
        if "Each" in c:
            if not isinstance(val, list) or not val:
                fail("%s: pointer is not a non-empty list" % c["Id"])
            for i, item in enumerate(val):
                if pointer(item, c["Each"]) != c["Expected"]:
                    fail("%s: element %d differs" % (c["Id"], i))
        elif val != c["Expected"]:
            fail("%s: probe value differs (%s)" % (c["Id"], json.dumps(val)[:120]))
    except (KeyError, IndexError, ValueError, TypeError):
        fail("%s: pointer %s not resolvable" % (c["Id"], c["Pointer"]))
    checks["ProbeFacts"] += 1

md = open(MD, "rb").read().decode("utf-8") if os.path.exists(MD) else None
if md is None:
    fail("r12.md absent")
else:
    for m in QUOTE_RE.finditer(md):
        quote, cid, key, b8, a, b = m.group(1), m.group(2), m.group(3), m.group(4), int(m.group(5)), m.group(6)
        b = int(b) if b else a
        c = by_id.get(cid)
        checks["MdQuotes"] += 1
        if c is None:
            fail("md: %s not in citations" % cid)
            continue
        if collapse(quote) != collapse(c["Quote"]):
            fail("md: quote of %s differs from the registered quote" % cid)
        if c.get("Kind", "text") == "probe":
            fail("md: %s is a probe fact, not a text quote" % cid)
            continue
        if key != c["Source"] or not sources[key]["Blob"].startswith(b8) or [a, b] != [c["Lines"][0], c["Lines"][-1]]:
            fail("md: location of %s differs (%s@%s:%d-%d)" % (cid, key, b8, a, b))
    # every other «...» in r12.md: tagged -> substring of that citation; untagged -> substring of some citation
    text_quotes = [collapse(c["Quote"]) for c in by_id.values() if c.get("Kind", "text") != "probe"]
    for m in re.finditer(r"«([^«»]+)»(\s*\[([CKTRP]-\d+)\])?(\s*\([A-Z0-9]+@[0-9a-f]{8}:\d+(?:-\d+)?\))?", md):
        if m.group(4):
            continue
        q = collapse(m.group(1))
        checks["MdTermQuotes"] = checks.get("MdTermQuotes", 0) + 1
        if m.group(3):
            c = by_id.get(m.group(3))
            if c is None or c.get("Kind", "text") == "probe" or q not in collapse(c["Quote"]):
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
        fail("citations never referenced in r12.md: " + ",".join(unused))

js = json.loads(open(JS, "rb").read().decode("utf-8")) if os.path.exists(JS) else None
if js is None:
    fail("r12.json absent")
else:
    counts = {d: 0 for d in DISPOSITIONS}
    for row in js["Rows"]:
        checks["JsonRows"] += 1
        if row["Disposition"] not in DISPOSITIONS:
            fail("json: %s disposition %s" % (row["Id"], row["Disposition"]))
        else:
            counts[row["Disposition"]] += 1
        for k in ("Authorities", "AcceptorCode", "Evidence"):
            for cid in row.get(k, []):
                if cid not in by_id:
                    fail("json: %s references unknown %s" % (row["Id"], cid))
    if counts != js["Counts"]:
        fail("json: Counts differ from rows")
    if len(js["Rows"]) != 12:
        fail("json: row count is not 12")
    if md is not None:
        table = dict(re.findall(r"^\| (L-\d\d) \| \*\*(COMPATIBLE|INCOMPATIBLE|INDETERMINATE)\*\* \|", md, re.M))
        for row in js["Rows"]:
            if table.get(row["Id"]) != row["Disposition"]:
                fail("md summary table and json differ for " + row["Id"])
        if len(table) != 12:
            fail("md summary table does not have 12 rows")
    pr = js.get("Probes", {})
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
        if rel == "r12-verify-result.json" or "/__pycache__/" in "/" + rel:
            continue
        data = open(p, "rb").read()
        files[rel] = sha256(data)
        low = data.decode("utf-8", "replace").lower()
        if user and user.lower() in low:
            fail("hygiene: user name in " + rel)
        if EMAIL_RE.search(data.decode("utf-8", "replace")):
            fail("hygiene: e-mail address in " + rel)
        checks["HygieneFiles"] += 1

result = {"Schema": "i62-r12-verify/1", "Pass": not problems, "Checks": checks, "Problems": problems,
          "Files": dict(sorted(files.items()))}
data = (json.dumps(result, ensure_ascii=True, sort_keys=True, indent=1) + "\n").encode("ascii")
with open(OUT, "wb") as fh:
    fh.write(data)
print(json.dumps({"Pass": not problems, "Checks": checks, "Problems": problems[:40]}, ensure_ascii=True, indent=1))
sys.exit(0 if not problems else 1)
