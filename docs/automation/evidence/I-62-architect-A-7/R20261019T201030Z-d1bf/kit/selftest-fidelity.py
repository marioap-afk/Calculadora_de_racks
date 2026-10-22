"""Self-test of fidelity.py (decisions §94, point 5; decisions §88, point 3): the detector, the faithful copy RACKCAD-COPIA-FIEL/1 and
run_files_of, with the NEGATIVES of the fidelity component. Pure: synthetic bytes, plus one regression on the real TRX of the r1 failure (read
with `git show` from the clone of the kit data's closure; never written).
  FD  detector: BOM (only at the start), LONG_LINE (1 900 / 1 901 / 2 001 units, UTF-16 counting), BARE_CR (CRLF accepted; CR at EOF), NUL,
      INVALID_UTF8 (lone byte, truncated sequence, encoded surrogate, overlong), CONTROL (TAB, ESC, DEL, C1), ZWNBSP, LINE_SEPARATOR,
      SPECIAL_EXTENSION, a clean file, the empty file, the class order;
  FE  copy: exact round trip of every sample; every copy line <= 1 900 characters, only measured literals, no trailing space, the copy itself
      unflagged; line n of the copy = line n of the original; BOM, CR, TAB, invalid bytes, backslashes, trailing spaces and unmeasured
      non-ASCII written explicitly; deterministic; chunk boundaries;
  FV  verify negatives: copy absent; EVERY single-byte alteration of a copy (each position) is detected; a copy with the BOM silently dropped
      (canonical for other bytes) → only «no reproduce los bytes»; the copy of another original with the same bytes → only «nombra otro
      original»; a non-canonical spelling → only «no canónica»; structural damage (each kind) → only «no se decodifica»; a joined long line
      (> 1 900) → «no canónica»;
  FR  run_files_of: consistent, without FaithfulCopies, copy not at the end, repeated names, copy declared a premise, no copies;
  FN  copy_name;
  FX  regression r1: the TRX of a6m7-measure/inputs (blob 7a9d…: the file whose line 1 was DEGRADED in R20261019T010200Z-4a72) is flagged BOM
      [1]; its line 1 delivered without the BOM is NOT the canonical line 1 (the auditor's DEGRADED stands); its copy's line 1 carries the BOM as
      \\xEF\\xBB\\xBF and round-trips.
Environment: RACKCAD_A7_KITDATA=<dir> as in selftest-post-review.py (fidelity.py and closure.json from <dir>; default: this kit directory).
Usage: python -B selftest-fidelity.py <out.json>
"""
import hashlib
import importlib.util
import json
import os
import subprocess
import sys

sys.dont_write_bytecode = True
HERE = os.path.dirname(os.path.abspath(__file__))
KITDATA = os.environ.get("RACKCAD_A7_KITDATA") or HERE
TRX_R1 = "docs/automation/evidence/I-62-F6/kits/FX-02/a6m7-measure/inputs/fixture-tests.trx"
B = b"\xef\xbb\xbf"


def load(path=None):
    spec = importlib.util.spec_from_file_location("fidelity_under_test", path or os.path.join(KITDATA, "fidelity.py"))
    m = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(m)
    return m


SAMPLES = {
    "s/bom.trx": B + b'<?xml version="1.0" encoding="utf-8"?>\n<TestRun id="x">\n</TestRun>',
    "s/bom-crlf.txt": B + b"uno\r\ndos\r\n",
    "s/bare-cr.txt": b"linea 1\nantes\rdespues\nlinea 3\n",
    "s/cr-eof.txt": b"a\nb\r",
    "s/nul.txt": b"linea 1\ncon \x00 NUL\nlinea 3\n",
    "s/invalid.txt": b"a \xff b\n\xc3\n\xed\xa0\x80\n\xc0\xaf fin\n",
    "s/control.txt": b"a\tb\nESC \x1b[0m\nDEL \x7f\n" + "C1 \u0085\n".encode("utf-8"),
    "s/long.txt": b"corta\n" + (b"ab\\c de " * 400) + b"\n" + b"x" * 1900 + b"\n" + b"y" * 1901 + b"\notra",
    "s/long-trailing-space.txt": b"z" * 1899 + b" \n" + b"w" * 3797 + b" ",
    "s/zwnbsp.txt": "inicio\nmedio\ufeffinterior\n".encode("utf-8"),
    "s/line-sep.txt": "a\u2028b\nc\u2029\n".encode("utf-8"),
    "s/astral.txt": ("\U0001F600" * 951 + "\n").encode("utf-8"),
    "s/clean.txt": ("texto limpio: línea, sí, ñ, «a» — §3 → ≤ " + "".join(chr(c) for c in range(0x20, 0x7F)) + "\n").encode("utf-8"),
    "s/spaces.txt": b"a  \n  \n\n\\>\\\\\nfin\\",
    "s/unmeasured.txt": "Ñandú ✓ ≈ ×\n".encode("utf-8"),
    "s/empty.txt": b"",
    "s/newline.txt": b"\n",
    "s/doc.pdf": b"no es un pdf\n",
}


def cases(F):
    out = []

    def row(name, ok, **d):
        out.append(dict({"Case": name, "Result": "PASS" if ok else "FAIL"}, **{k: (v if len(repr(v)) < 600 else repr(v)[:600]) for k, v in d.items()}))

    def flags(path, data):
        return F.scan(path, data)["Flags"]

    # ---------------- FD: detector
    det = [
        ("FD01-bom", "x.trx", B + b"<a>\n</a>", {"BOM": [1]}),
        ("FD02-bom-solo-al-principio", "x.txt", b"a\n" + B + b"b\n", {"ZWNBSP": [2]}),
        ("FD03-linea-1901", "x.txt", b"a" * 1901, {"LONG_LINE": [1]}),
        ("FD04-linea-1900", "x.txt", b"a" * 1900 + b"\n", {}),
        ("FD05-linea-2001", "x.txt", b"a\n" + b"a" * 2001 + b"\n", {"LONG_LINE": [2]}),
        ("FD06-linea-utf16", "x.txt", ("\U0001F600" * 951).encode("utf-8"), {"LONG_LINE": [1], "UNMEASURED_CHAR": [1]}),
        ("FD07-cr-suelto", "x.txt", b"a\rb\nc\n", {"BARE_CR": [1]}),
        ("FD08-crlf-aceptado", "x.txt", b"a\r\nb\r\n", {}),
        ("FD09-cr-al-final-sin-lf", "x.txt", b"a\nb\r", {"BARE_CR": [2]}),
        ("FD10-nul", "x.txt", b"a\nb\x00\n", {"NUL": [2]}),
        ("FD11-utf8-invalido", "x.txt", b"\xff\n\xc3\n\xed\xa0\x80\n\xc0\xaf\nok\n", {"INVALID_UTF8": [1, 2, 3, 4]}),
        ("FD12-tab", "x.txt", b"a\tb\n", {"CONTROL": [1]}),
        ("FD13-esc-del-c1", "x.txt", b"\x1b\n\x7f\n" + "\u0085\n".encode("utf-8") + b"ok\n", {"CONTROL": [1, 2, 3]}),
        ("FD14-zwnbsp-interior", "x.txt", "x\ufeffy\n".encode("utf-8"), {"ZWNBSP": [1]}),
        ("FD15-bom-y-zwnbsp", "x.txt", B + "a\ufeff\n".encode("utf-8"), {"BOM": [1], "ZWNBSP": [1]}),
        ("FD16-separador-de-linea", "x.txt", "a\u2028b\nc\u2029\n".encode("utf-8"), {"LINE_SEPARATOR": [1, 2]}),
        ("FD17-extension-especial", "d/doc.PDF", b"x\n", {"SPECIAL_EXTENSION": [0]}),
        ("FD18-limpio", "s/clean.txt", SAMPLES["s/clean.txt"] + b"fin  \n", {}),
        ("FD19-vacio", "x.txt", b"", {}),
        ("FD20-varias-clases-en-orden", "x.png", B + b"\x00\t\r" + b"\xff" + b"q" * 1950 + "\u2028\ufeff".encode("utf-8"),
         {"BOM": [1], "INVALID_UTF8": [1], "NUL": [1], "BARE_CR": [1], "CONTROL": [1], "LONG_LINE": [1], "ZWNBSP": [1], "LINE_SEPARATOR": [1],
          "SPECIAL_EXTENSION": [0]}),
    ]
    for name, path, data, want in det:
        got = flags(path, data)
        row(name, got == want and list(got) == [k for k in F.CLASSES if k in want] and F.scan(path, data)["Flagged"] == bool(want),
            Expected=want, Got=got)
    i = F.scan("x.txt", b"a\n" + b"b" * 2001 + b"\n" + b"c" * 1950)
    unm = [("FD22-no-medido-enie-mayuscula", (chr(0xD1) + "and" + chr(0xFA) + "\n").encode("utf-8"), {"UNMEASURED_CHAR": [1]}),
           ("FD23-no-medido-por-linea", ("a\n" + chr(0x00D7) + " y " + chr(0x2248) + "\nb\n" + chr(0x2713) + "\n").encode("utf-8"), {"UNMEASURED_CHAR": [2, 4]}),
           ("FD24-no-medido-nbsp-y-astral", (chr(0xA0) + "\n" + chr(0x1F600) + "\n").encode("utf-8"), {"UNMEASURED_CHAR": [1, 2]}),
           ("FD25-medidos-no-marcan", ("".join(sorted(F.MEASURED_NON_ASCII)) + " " + chr(0x2013) + "\n").encode("utf-8"), {}),
           ("FD26-otras-clases-no-son-no-medidos", (chr(0x85) + chr(0x2028) + "\n" + "x" + chr(0xFEFF) + "\n").encode("utf-8") + b"\xff\n",
            {"INVALID_UTF8": [3], "CONTROL": [1], "ZWNBSP": [2], "LINE_SEPARATOR": [1]})]
    for name, data, want in unm:
        got = flags("x.txt", data)
        row(name, got == want and list(got) == [k for k in F.CLASSES if k in want], Expected=want, Got=got)
    row("FD27-conjunto-medido", len(F.MEASURED_NON_ASCII) == 34 and chr(0x2013) in F.MEASURED_NON_ASCII and chr(0xD1) not in F.MEASURED_NON_ASCII,
        Size=len(F.MEASURED_NON_ASCII))
    row("FD21-sobre-el-limite-de-read", i["LinesOverReadLimit"] == [2] and i["Flags"] == {"LONG_LINE": [2, 3]} and i["MaxLineUtf16"] == 2001 and
        i["Lines"] == 3 and i["FinalNewline"] is False, Got=i)

    # ---------------- FE: copy
    rt, readable, aligned = [], [], []
    for path, data in SAMPLES.items():
        c = F.encode(path, data)
        p, d = F.decode(c)
        rt.append((path, p == path and d == data and F.verify(path, data, c) == []))
        lines = c.decode("utf-8").split("\n")
        body_ok = lines[-1] == "" and all(len(x) <= F.COPY_LINE_LIMIT and all(F.is_literal(ch) for ch in x) and not x.endswith(" ")
                                          for x in lines[:-1]) and F.scan("copia.txt", c)["Flags"] == {} and not c.startswith(B) and b"\r" not in c
        readable.append((path, body_ok))
        n = F.scan(path, data)["Lines"]
        lay = F.layout(c)
        aligned.append((path, sorted(lay) == list(range(1, n + 1)) and all(lay[k][0] == k and all(x > n + 1 for x in lay[k][1:]) for k in lay)))
    row("FE01-ida-y-vuelta-exacta", all(ok for _, ok in rt), Failing=[p for p, ok in rt if not ok], Samples=len(rt))
    row("FE02-copia-legible", all(ok for _, ok in readable), Failing=[p for p, ok in readable if not ok])
    row("FE03-misma-numeracion", all(ok for _, ok in aligned), Failing=[p for p, ok in aligned if not ok])
    cl = lambda path: F.encode(path, SAMPLES[path]).decode("utf-8").split("\n")
    row("FE04-bom-explicito", cl("s/bom.trx")[0].startswith("\\xEF\\xBB\\xBF<?xml ") and cl("s/bom-crlf.txt")[:2] == ["\\xEF\\xBB\\xBFuno\\x0D", "dos\\x0D"],
        Got=cl("s/bom-crlf.txt")[:2])
    row("FE05-cr-explicito", cl("s/bare-cr.txt")[1] == "antes\\x0Ddespues" and cl("s/cr-eof.txt")[1] == "b\\x0D", Got=[cl("s/bare-cr.txt")[1], cl("s/cr-eof.txt")[1]])
    row("FE06-bytes-invalidos-y-controles", cl("s/invalid.txt")[:4] == ["a \\xFF b", "\\xC3", "\\xED\\xA0\\x80", "\\xC0\\xAF fin"] and
        cl("s/control.txt")[:4] == ["a\\x09b", "ESC \\x1B[0m", "DEL \\x7F", "C1 \\xC2\\x85"] and cl("s/nul.txt")[1] == "con \\x00 NUL",
        Got=cl("s/invalid.txt")[:4] + cl("s/control.txt")[:4])
    row("FE07-barras-invertidas", cl("s/spaces.txt")[:5] == ["a \\x20", " \\x20", "", "\\\\>\\\\\\\\", "fin\\\\"], Got=cl("s/spaces.txt")[:5])
    row("FE08-no-ascii-no-medido", cl("s/unmeasured.txt")[0] == "\\xC3\\x91andú \\xE2\\x9C\\x93 \\xE2\\x89\\x88 \\xC3\\x97" and
        cl("s/clean.txt")[0].startswith("texto limpio: línea, sí, ñ, «a» — §3 → ≤ "), Got=cl("s/unmeasured.txt")[0])
    c1, c2 = F.encode("a/x.trx", SAMPLES["s/bom.trx"]), F.encode("b/x.trx", SAMPLES["s/bom.trx"])
    diff = [k for k, (x, y) in enumerate(zip(c1.decode().split("\n"), c2.decode().split("\n"))) if x != y]
    row("FE09-determinista", F.encode("a/x.trx", SAMPLES["s/bom.trx"]) == c1 and len(diff) == 1 and c1.decode().split("\n")[diff[0]].startswith("Original: "),
        Diff=diff)
    L = cl("s/long.txt")
    n_long = F.scan("s/long.txt", SAMPLES["s/long.txt"])["Lines"]
    lay = F.layout(F.encode("s/long.txt", SAMPLES["s/long.txt"]))
    row("FE10-cortes-de-linea", L[2] == "x" * 1900 and L[3].endswith("\\>") and len(L[3]) == 1900 and len(lay[2]) == 2 and lay[3] == [3] and
        len(lay[4]) == 2 and L[lay[4][1] - 1] == "@4.2|yyy" and L[n_long] .startswith("\\=== RACKCAD-COPIA-FIEL/1: fin de las 5 "),
        Lay={k: v for k, v in lay.items()})
    T = cl("s/long-trailing-space.txt")
    lt = F.layout(F.encode("s/long-trailing-space.txt", SAMPLES["s/long-trailing-space.txt"]))
    row("FE11-espacio-final-en-un-corte", T[0].endswith("\\>") and T[lt[1][1] - 1] == "@1.2|z\\x20" and T[lt[2][-1] - 1].endswith("\\x20") and
        all(len(x) <= 1900 for x in T), Got=[T[0][-8:], T[lt[1][1] - 1], T[lt[2][-1] - 1][-12:]])

    # ---------------- FV: verify negatives
    path, data = "s/bom.trx", SAMPLES["s/bom.trx"] + b"\n" + b"k" * 2500
    good = F.encode(path, data)
    row("FV01-copia-ausente", F.verify(path, data, None) == ["copia ausente"], Got=F.verify(path, data, None))
    missed = []
    for pos in range(len(good)):
        b = bytearray(good)
        b[pos] ^= 0x01
        if not F.verify(path, data, bytes(b)):
            missed.append(pos)
    row("FV02-cada-byte-alterado-se-detecta", not missed and len(good) > 3000, Positions=len(good), Missed=missed[:20])
    nb = F.encode(path, data[3:])
    row("FV03-bom-eliminado-en-silencio", [p[:44] for p in F.verify(path, data, nb)] == ["la copia no reproduce los bytes del original"[:44]],
        Got=F.verify(path, data, nb))
    other = F.encode("otra/ruta.trx", data)
    row("FV04-copia-de-otro-original-con-los-mismos-bytes", F.verify(path, data, other) == ["la copia nombra otro original (otra/ruta.trx)"],
        Got=F.verify(path, data, other))
    nc = good.replace(b'<?xml version', b'<?xml \\x76ersion', 1)
    row("FV05-escritura-no-canonica", F.verify(path, data, nc) == ["la copia no es la codificación canónica de lo que decodifica"], Got=F.verify(path, data, nc))
    gl = good.decode("utf-8").split("\n")
    sep = next(k for k, x in enumerate(gl) if x.startswith("\\=== RACKCAD"))
    app = next(k for k, x in enumerate(gl) if x.startswith("@"))
    foot = gl.index("\\=== pie ===")
    damage = {
        "sin-pie": "\n".join(gl[:foot]) + "\n",
        "sin-separador": "\n".join(gl[:sep] + gl[sep + 1:]),
        "apendice-fuera-de-orden": "\n".join(gl[:app] + [gl[app].replace(".2|", ".3|", 1)] + gl[app + 1:]),
        "falta-la-marca": "\n".join([x[:-2] if x.endswith("\\>") and k < sep else x for k, x in enumerate(gl)]),
        "marca-en-el-ultimo-trozo": "\n".join(gl[:app] + [gl[app] + "\\>"] + gl[app + 1:]),
        "escape-desconocido": "\n".join(["\\q" + gl[0]] + gl[1:]),
        "hexadecimal-en-minusculas": "\n".join([gl[0].replace("\\xEF", "\\xef", 1)] + gl[1:]),
        "literal-fuera-del-conjunto": "\n".join([gl[0] + "Ñ"] + gl[1:]),
        "cr-en-la-copia": "\r\n".join(gl),
        "bom-al-principio-de-la-copia": "\ufeff" + "\n".join(gl),
        "sin-lf-final": "\n".join(gl[:-1]),
        "salto-final-no-valido": "\n".join([x.replace("SaltoFinal: no", "SaltoFinal: tal vez") for x in gl]),
    }
    bad = {}
    for k, text in damage.items():
        v = F.verify(path, data, text.encode("utf-8"))
        if not (len(v) == 1 and v[0].startswith("la copia no se decodifica (")):
            bad[k] = v
    row("FV06-estructura-rota", not bad and len(damage) == 12, Bad=bad)
    joined = list(gl)
    lk = next(k for k, x in enumerate(gl[:sep]) if x.endswith("\\>"))
    joined[lk] = joined[lk][:-2] + gl[app].split("|", 1)[1]
    joined = joined[:app] + joined[app + 1:]
    jt = "\n".join(joined).encode("utf-8")
    row("FV07-linea-larga-unida", max(len(x) for x in jt.decode().split("\n")) > 1900 and
        F.verify(path, data, jt) == ["la copia no es la codificación canónica de lo que decodifica"], Got=F.verify(path, data, jt))

    # ---------------- FR: run files
    def rfo(c):
        try:
            return F.run_files_of(c)
        except ValueError as e:
            return "ValueError: %s" % e
    base = {"RunFiles": ["order.txt", "prompt.md", "fiel__a.txt", "fiel__b.txt"], "FaithfulCopies": [{"Copy": "fiel__a.txt"}, {"Copy": "fiel__b.txt"}],
            "PremiseRunFiles": ["order.txt"]}
    row("FR01-coherente", rfo(base) == base["RunFiles"], Got=rfo(base))
    row("FR02-sin-faithfulcopies", "no declara FaithfulCopies" in str(rfo({k: v for k, v in base.items() if k != "FaithfulCopies"})))
    row("FR03-copia-fuera-del-final", "no termina exactamente" in str(rfo(dict(base, RunFiles=["order.txt", "fiel__a.txt", "prompt.md", "fiel__b.txt"]))))
    row("FR04-nombres-repetidos", "repetidos" in str(rfo(dict(base, RunFiles=["order.txt", "Order.txt", "fiel__a.txt", "fiel__b.txt"]))))
    row("FR05-copia-como-premisa", "premisa" in str(rfo(dict(base, PremiseRunFiles=["order.txt", "fiel__b.txt"]))))
    row("FR06-sin-copias", rfo(dict(base, RunFiles=["order.txt", "prompt.md"], FaithfulCopies=[])) == ["order.txt", "prompt.md"])

    # ---------------- FN: copy names
    def cn(p):
        try:
            return F.copy_name(p)
        except AssertionError:
            return "AssertionError"
    row("FN01-nombre-de-la-copia", cn("docs/a/b.trx") == "fiel__docs__a__b.trx.txt" and cn("a__b.txt") == "AssertionError" and cn("a b.txt") == "AssertionError",
        Got=[cn("docs/a/b.trx"), cn("a__b.txt"), cn("a b.txt")])

    # ---------------- FX: regression on the r1 failure
    closure = json.load(open(os.path.join(KITDATA, "closure.json"), encoding="utf-8"))
    clone, rev = closure["Transport"]["Cwd"], closure["AuthorityRevision"]
    trx = subprocess.run(["git", "-C", clone, "show", rev + ":" + TRX_R1], capture_output=True).stdout
    canon1 = trx.decode("utf-8").replace("\r\n", "\n").split("\n")[0]
    delivered1 = canon1.lstrip("\ufeff")                       # what Read delivered in r1 (the BOM silently dropped)
    copy = F.encode(TRX_R1, trx)
    c1 = copy.decode("utf-8").split("\n")[0]
    row("FX01-trx-de-r1", F.scan(TRX_R1, trx)["Flags"] == {"BOM": [1]} and delivered1 != canon1 and canon1 == "\ufeff" + delivered1 and
        c1 == "\\xEF\\xBB\\xBF" + delivered1 and F.decode(copy) == (TRX_R1, trx) and F.verify(TRX_R1, trx, copy) == [] and
        hashlib.sha1(b"blob %d\x00" % len(trx) + trx).hexdigest() == next(r["Blob"] for r in closure["CanonicalInputs"] if r["Path"] == TRX_R1),
        Flags=F.scan(TRX_R1, trx)["Flags"], CopyLine1=c1[:60])
    return out


def main(out_path):
    F = load()
    rows = cases(F)
    doc = {"Selftest": "fidelity.py (detector, copia fiel RACKCAD-COPIA-FIEL/1, run_files_of) — negativos del componente de fidelidad",
           "Modules": {n: hashlib.sha256(open(os.path.join(p, n), "rb").read()).hexdigest()
                       for p, n in ((KITDATA, "fidelity.py"), (HERE, "selftest-fidelity.py"))},
           "Cases": rows, "Totals": {"Cases": len(rows), "Pass": sum(1 for r in rows if r["Result"] == "PASS")},
           "AllPass": all(r["Result"] == "PASS" for r in rows)}
    with open(out_path, "w", encoding="utf-8", newline="\n") as f:
        f.write(json.dumps(doc, ensure_ascii=False, indent=1) + "\n")
    for r in rows:
        print("%-55s %s" % (r["Case"], r["Result"]) + ("" if r["Result"] == "PASS" else "  " + json.dumps({k: v for k, v in r.items() if k not in ("Case", "Result")}, ensure_ascii=True)[:400]))
    print(json.dumps({"Totals": doc["Totals"], "AllPass": doc["AllPass"]}))
    return 0 if doc["AllPass"] else 1


if __name__ == "__main__":
    sys.stdout.reconfigure(errors="backslashreplace")
    sys.exit(main(sys.argv[1]))
