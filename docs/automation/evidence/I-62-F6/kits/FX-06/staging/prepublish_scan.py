#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""FX-06 — comprobacion previa a la publicacion de artefactos del fixture (SOLO SUPERVISION; plano a).

Antes de cada push del Coordinator del fixture (orden, RLA, registro de la medicion, texto de X) y antes de que el Owner escriba un
mensaje a una sesion del fixture, la supervision pasa este programa sobre los archivos que van a publicarse o a escribirse.

  - SupervisionOnlyTokens: cadenas de los archivos solo de supervision (SHA del oraculo, fragmentos distintivos del oraculo y de los
    esperados). Una sola aparicion = NO PUBLICAR (codigo 1). El resultado nunca reproduce el token: solo su indice.
  - Patrones de indicio: palabras que pueden orientar al revisor o al Principal hacia el defecto sembrado o hacia un veredicto o una
    secuencia de disposiciones esperados (genericos de este programa + los de la supervision). Aparicion = publicar solo tras una
    disposicion registrada de la supervision (codigo 4).

Autoridades: V14 D.6 (el oraculo, fuera de los hosts, protege el esperado), D.8-1 (defecto que conoce el oraculo del Coordinator),
§20.3 (invocacion limpia del Architect); decisiones §53 y §54 («Sin atajos»: no revelar el oraculo antes); orden de la linea (los
valores esperados solo en archivos de supervision).

Uso:
  python prepublish_scan.py scan --tokens <tokens.json> --output <resultado.json> <archivo> [<archivo> ...]
  python prepublish_scan.py self-test [--output <prepublish-scan-selftest.json>]

Codigos: 0 limpio; 1 token solo de supervision (no publicar); 4 solo indicios (disposicion previa); 2 entrada rechazada.
Sin dependencias externas; salida determinista.
"""
import argparse
import hashlib
import json
import re
import sys
import unicodedata

SCHEMA_TOKENS = "fx06-prepublish-scan-tokens/v1"
SCHEMA_OUT = "fx06-prepublish-scan-result/v1"
VERSION = "1.0.0"

# Indicios genericos (sin distinguir mayusculas). No son una regla congelada: son la lista minima de la supervision.
BUILTIN_HINTS = [
    r"or[áa]culo", r"\boracle\b", r"sembrad[oa]s?", r"\bseeded\b", r"defecto (conocido|previsto|plantado)",
    r"(veredicto|resultado|disposici[óo]n|correcci[óo]n) (esperad[oa]|prevista|previsto)", r"\bexpected (verdict|result|disposition)s?\b",
    r"\bFX-06\b", r"ensayo de autonom[íi]a", r"piloto de autonom[íi]a",
]


class InputError(Exception):
    pass


def nfc(text):
    return unicodedata.normalize("NFC", text)


def load_tokens(obj):
    if not isinstance(obj, dict) or obj.get("Schema") != SCHEMA_TOKENS:
        raise InputError("tokens: Schema distinto de " + SCHEMA_TOKENS)
    toks = obj.get("SupervisionOnlyTokens")
    hints = obj.get("HintPatterns", [])
    if not isinstance(toks, list) or not all(isinstance(t, str) and t for t in toks):
        raise InputError("tokens: SupervisionOnlyTokens debe ser una lista de cadenas no vacias")
    if not isinstance(hints, list) or not all(isinstance(h, str) and h for h in hints):
        raise InputError("tokens: HintPatterns debe ser una lista de cadenas no vacias")
    try:
        rx = [(h, re.compile(h, re.IGNORECASE)) for h in BUILTIN_HINTS + hints]
    except re.error as exc:
        raise InputError("tokens: patron invalido: %s" % exc)
    return [nfc(t) for t in toks], rx


def scan_bytes(name, raw, toks, rx):
    try:
        text = nfc(raw.decode("utf-8"))
    except UnicodeDecodeError:
        raise InputError("archivo no UTF-8: " + name)
    token_hits = []
    hint_hits = []
    for n, line in enumerate(text.replace("\r\n", "\n").split("\n"), start=1):
        for i, t in enumerate(toks):
            if t in line:
                token_hits.append({"Line": n, "TokenIndex": i})
        for pat, r in rx:
            if r.search(line):
                hint_hits.append({"Line": n, "Pattern": pat})
    return {"File": name, "Sha256": hashlib.sha256(raw).hexdigest(), "Bytes": len(raw), "TokenHits": token_hits, "HintHits": hint_hits}


def scan(files, toks, rx):
    results = [scan_bytes(name, raw, toks, rx) for name, raw in files]
    any_token = any(r["TokenHits"] for r in results)
    any_hint = any(r["HintHits"] for r in results)
    verdict = "DO_NOT_PUBLISH" if any_token else ("DISPOSITION_REQUIRED" if any_hint else "CLEAN")
    return {"Schema": SCHEMA_OUT, "Version": VERSION, "TokenCount": len(toks), "Files": results, "Verdict": verdict,
            "Note": "Comprobacion mecanica minima; no sustituye la lectura de la supervision. Un indicio no listado no se detecta."}


def code_of(verdict):
    return {"CLEAN": 0, "DO_NOT_PUBLISH": 1, "DISPOSITION_REQUIRED": 4}[verdict]


def dumps(obj):
    return json.dumps(obj, ensure_ascii=False, indent=1) + "\n"


# ----------------------------------------------------------------------------------------------------------------------------- autoprueba
# Solo datos sinteticos.

def self_test():
    toks, rx = load_tokens({"Schema": SCHEMA_TOKENS, "SupervisionOnlyTokens": ["SYNTHETIC-ORACLE-TOKEN-0001", "seccion-sintetica-9"],
                            "HintPatterns": [r"\bsintetico-prohibido\b"]})
    cases = [
        ("S01 orden neutra", [("orden.md", "Ejecuta el bucle de revision bajo la autorizacion SYN-RLA-1.\n".encode("utf-8"))], "CLEAN"),
        ("S02 token del oraculo en la RLA", [("rla.md", "CorrectionScope: seccion-sintetica-9\n".encode("utf-8"))], "DO_NOT_PUBLISH"),
        ("S03 indicio generico (defecto sembrado)", [("orden.md", "La propuesta tiene un defecto sembrado.\n".encode("utf-8"))],
         "DISPOSITION_REQUIRED"),
        ("S04 indicio de la supervision", [("x.md", "texto sintetico-prohibido\n".encode("utf-8"))], "DISPOSITION_REQUIRED"),
        ("S05 nombre del escenario", [("x.md", "Objeto revisable del ensayo de autonomía (FX-06).\n".encode("utf-8"))], "DISPOSITION_REQUIRED"),
        ("S06 token y indicio: manda el token", [("a.md", b"oraculo\n"), ("b.md", b"SYNTHETIC-ORACLE-TOKEN-0001\n")], "DO_NOT_PUBLISH"),
        ("S07 NFC: token con tilde descompuesta", [("c.md", unicodedata.normalize("NFD", "Oráculo").encode("utf-8"))], "DISPOSITION_REQUIRED"),
    ]
    report = {"Schema": "fx06-prepublish-scan-selftest/v1", "Version": VERSION, "Cases": []}
    ok = True
    for name, files, expect in cases:
        r1 = scan(files, toks, rx)
        r2 = scan(files, toks, rx)
        problems = []
        if r1["Verdict"] != expect:
            problems.append("esperado %s, obtenido %s" % (expect, r1["Verdict"]))
        if dumps(r1) != dumps(r2):
            problems.append("salida no determinista")
        if any("SYNTHETIC-ORACLE-TOKEN-0001" in json.dumps(f) for f in r1["Files"]):
            problems.append("el resultado reproduce un token")
        report["Cases"].append({"Case": name, "Expected": expect, "Verdict": r1["Verdict"], "Result": "PASS" if not problems else "FAIL",
                                "Problems": problems})
        ok = ok and not problems
    for name, raw, needle in [("S08 archivo no UTF-8", b"\xff\xfe\x00", "no UTF-8")]:
        try:
            scan([("bad.bin", raw)], toks, rx)
            report["Cases"].append({"Case": name, "Result": "FAIL", "Problems": ["no rechazo la entrada"]})
            ok = False
        except InputError as exc:
            good = needle in str(exc)
            report["Cases"].append({"Case": name, "Result": "PASS" if good else "FAIL", "Problems": [] if good else [str(exc)], "Rejected": str(exc)})
            ok = ok and good
    try:
        load_tokens({"Schema": "otro", "SupervisionOnlyTokens": []})
        report["Cases"].append({"Case": "S09 tokens con Schema distinto", "Result": "FAIL", "Problems": ["no rechazo la entrada"]})
        ok = False
    except InputError as exc:
        report["Cases"].append({"Case": "S09 tokens con Schema distinto", "Result": "PASS", "Problems": [], "Rejected": str(exc)})
    report["Summary"] = {"Total": len(report["Cases"]), "Passed": sum(1 for c in report["Cases"] if c["Result"] == "PASS")}
    report["AllPassed"] = ok
    return ok, report


def main(argv):
    ap = argparse.ArgumentParser(description="Comprobacion previa a la publicacion en el fixture (solo supervision).")
    sub = ap.add_subparsers(dest="cmd", required=True)
    s = sub.add_parser("scan")
    s.add_argument("--tokens", required=True)
    s.add_argument("--output", required=True)
    s.add_argument("files", nargs="+")
    t = sub.add_parser("self-test")
    t.add_argument("--output")
    args = ap.parse_args(argv)
    if args.cmd == "scan":
        try:
            with open(args.tokens, "rb") as fh:
                toks, rx = load_tokens(json.loads(fh.read().decode("utf-8")))
            files = []
            for path in args.files:
                with open(path, "rb") as fh:
                    files.append((path.replace("\\", "/"), fh.read()))
            res = scan(files, toks, rx)
        except (InputError, ValueError, UnicodeDecodeError, OSError) as exc:
            sys.stderr.write("ENTRADA RECHAZADA (fallo cerrado): %s\n" % exc)
            return 2
        with open(args.output, "w", encoding="utf-8", newline="\n") as fh:
            fh.write(dumps(res))
        sys.stdout.write("Verdict=%s\n" % res["Verdict"])
        return code_of(res["Verdict"])
    ok, report = self_test()
    if args.output:
        with open(args.output, "w", encoding="utf-8", newline="\n") as fh:
            fh.write(dumps(report))
    for c in report["Cases"]:
        sys.stdout.write("%s  %s%s\n" % (c["Result"], c["Case"], (" — " + "; ".join(c["Problems"])) if c["Problems"] else ""))
    sys.stdout.write("TOTAL %d/%d\n" % (report["Summary"]["Passed"], report["Summary"]["Total"]))
    return 0 if ok else 1


if __name__ == "__main__":
    sys.exit(main(sys.argv[1:]))
