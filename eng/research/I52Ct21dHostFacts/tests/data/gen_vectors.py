#!/usr/bin/env python3
"""Independent generator of the expected test vectors of the I52Ct21dHostFacts tests.

It is written WITHOUT reading any C# code of this folder: it only uses the Python standard library
(json, hashlib, struct) so that the expected values are computed by a second implementation.

  label-vectors.json        expected canonical serialization, SHA-256 and MC- label of the six-string object
                            (BA-10 V8 section 2.2: JCS of exactly six keys, UTF-8, SHA-256, label = MC- + first 12 hex)
  jcs-vectors.json          expected canonical text (RFC 8785) of small JSON documents (BMP keys only: for BMP keys
                            Python's code point order equals the UTF-16 code unit order that RFC 8785 requires)
  probe-table-expected.json the 147 rows PF-1/PF-2 of design section 2.4 as 16-hex-digit binary64 patterns

Run:  python gen_vectors.py   (writes the three files next to this script; LF, UTF-8 without BOM)
This script is a test-data generator. It is not an instrument and is never run on a host.
"""
import hashlib
import json
import os
import struct

HERE = os.path.dirname(os.path.abspath(__file__))


def jcs(obj):
    # For strings and integers only, json.dumps with sorted keys, compact separators and ensure_ascii=False
    # produces the RFC 8785 text (control characters: short escapes for \b \t \n \f \r, \u00xx lowercase for the
    # rest; the quote and the backslash escaped; everything else raw).
    return json.dumps(obj, sort_keys=True, separators=(",", ":"), ensure_ascii=False)


def label_of(six):
    text = jcs(six)
    digest = hashlib.sha256(text.encode("utf-8")).hexdigest()
    return text, digest, "MC-" + digest[:12]


def write(name, obj):
    with open(os.path.join(HERE, name), "w", encoding="utf-8", newline="\n") as f:
        json.dump(obj, f, ensure_ascii=True, indent=2, sort_keys=True)
        f.write("\n")


def label_vectors():
    cases = []

    def add(case_id, six, note):
        text, digest, label = label_of(six)
        cases.append({"id": case_id, "note": note, "inputs": six, "serialization": text,
                      "sha256": digest, "label": label})

    base = {"AutoCadProduct": "SYNTHETIC-PRODUCT", "AutoCadProfile": "SYNTHETIC-PROFILE",
            "MachineGuid": "00000000-0000-0000-0000-000000000000", "OsVersionBuild": "0.0.0.0",
            "SECURELOAD": "1", "TRUSTEDPATHS": "C:\\SYNTHETIC\\modules\\..."}
    add("DESIGN-3.2-A", base, "synthetic vector of design section 3.2 (SYNTHETIC; not a fact about any machine)")
    empty = dict(base)
    empty["TRUSTEDPATHS"] = ""
    add("DESIGN-3.2-B", empty, "same, TRUSTEDPATHS empty string")
    accents = dict(base)
    accents["AutoCadProfile"] = "Perfil \u00c1rbol \u00f1and\u00fa"
    accents["TRUSTEDPATHS"] = "C:\\Usuarios\\Mar\u00eda\\M\u00f3dulos;D:\\\u20ac\\..."
    add("NON-ASCII", accents, "Spanish accents, euro sign (BMP, non ASCII)")
    astral = dict(base)
    astral["AutoCadProfile"] = "emoji \U0001F600 end"
    add("ASTRAL", astral, "a supplementary-plane character (surrogate pair in UTF-16)")
    ctrl = dict(base)
    ctrl["AutoCadProfile"] = "a\tb\nc\rd\be\ff\x01g\x1fh\x7fi\"j\\k/l"
    add("CONTROL-ESCAPES", ctrl, "control characters, DEL, quote, backslash, slash")
    allempty = {k: "" for k in base}
    add("ALL-EMPTY", allempty, "six empty strings")
    write("label-vectors.json", {
        "generator": "gen_vectors.py (Python standard library; independent of the C# code)",
        "rule": "BA-10 V8 2.2: RFC 8785 serialization, UTF-8, SHA-256, label = MC- + first 12 lowercase hex digits",
        "cases": cases})


def jcs_vectors():
    docs = [
        ("EMPTY-OBJECT", {}),
        ("SORTED-KEYS", {"b": "1", "a": "2", "B": "3", "\u00e9": "4", "aa": "5"}),
        ("NESTED", {"z": {"y": [1, 2, {"b": True, "a": None}], "x": "s"}, "a": []}),
        ("INTEGERS", {"zero": 0, "neg": -17, "big": 9007199254740991}),
        ("CONTROL", {"k": "\u0000\u0001\u0008\u0009\u000a\u000b\u000c\u000d\u001f\u007f"}),
        ("QUOTE-BACKSLASH", {"k": "\"\\/"}),
        ("NON-ASCII", {"k": "\u00e9\u20ac\u2028\u2029\uffff", "\u20ac": "\u00e9"}),
        ("ASTRAL-VALUE", {"k": "\U0001F600"}),
        ("EMPTY-STRING", {"": ""}),
    ]
    out = []
    for case_id, doc in docs:
        out.append({"id": case_id, "inputJson": json.dumps(doc, ensure_ascii=True), "expected": jcs(doc)})
    write("jcs-vectors.json", {"generator": "gen_vectors.py", "cases": out})


def bits(x):
    return "%016X" % struct.unpack(">Q", struct.pack(">d", x))[0]


def probe_table():
    ks = [52, 48, 44, 40, 36, 32, 30, 29, 28, 24, 20]
    signs = [("++", 1.0, 1.0), ("-+", -1.0, 1.0), ("+-", 1.0, -1.0), ("--", -1.0, -1.0)]
    rows = []
    n = 0
    for sp, sx, sy in signs:
        for ci, comp in enumerate("XYZ"):
            for k in ks:
                n += 1
                base = [sx, sy, 1.0]
                sigma = [sx, sy, 1.0]
                base[ci] = sigma[ci] * (1.0 + 2.0 ** -k)
                rows.append({"id": "PF1-%04d" % n, "family": "PF1", "signPattern": sp, "component": comp,
                             "k": k, "inScope": True, "intended": [bits(v) for v in base]})
    bases = [("1", 0x3FF0000000000000, True), ("0.5", 0x3FE0000000000000, False), ("2", 0x4000000000000000, False),
             ("25.4", 0x4039666666666666, False), ("1/25.4", 0x3FA42850A142850A, False)]
    n = 0
    for name, b, scope in bases:
        up = b + 1
        down = b - 1
        for comp, x in (("ALL", None), ("X", up), ("X", down)):
            n += 1
            if x is None:
                triple = ["%016X" % b] * 3
            else:
                triple = ["%016X" % x, "%016X" % b, "%016X" % b]
            rows.append({"id": "PF2-%04d" % n, "family": "PF2", "signPattern": "++", "component": comp,
                         "k": None, "inScope": scope, "intended": triple})
    # cross-check the table bit patterns of the design against binary64 arithmetic
    assert bits(25.4) == "4039666666666666"
    assert bits(1.0 / 25.4) == "3FA42850A142850A"
    write("probe-table-expected.json", {"generator": "gen_vectors.py", "rows": rows,
                                         "rowCount": len(rows), "inScope": sum(1 for r in rows if r["inScope"]),
                                         "distinctTriples": len({tuple(r["intended"]) for r in rows})})


if __name__ == "__main__":
    label_vectors()
    jcs_vectors()
    probe_table()
