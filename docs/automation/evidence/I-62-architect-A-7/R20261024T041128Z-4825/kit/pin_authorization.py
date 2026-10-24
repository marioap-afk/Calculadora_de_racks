"""I-62, decisiones §85, punto 6: valores que hay que fijar en kit/owner_gate.py (AUTH_SECTION, AUTH_HEADING y AUTH_TEXT_SHA) para la disposición
del Coordinator, POSTERIOR a §85, que autorice el lanzamiento de la ÚNICA revisión formal de la ronda 4 de A-6. NO edita el kit ni ningún otro
archivo: lee el archivo de decisiones que se le da e imprime los tres valores, las tres líneas de Python que sustituyen a las que el kit entrega en
None, y las comprobaciones de la sección. El SHA-256 se calcula con section_text() de owner_gate.py, la misma función con que la compuerta 11 lo
recalcula, y la sección se comprueba con owner_gate.authorization_sections() y los mismos criterios de la compuerta (número posterior a §85,
exactamente una sección con ese número, el blob completo de la ronda 4 de closure.json como token literal y ninguna sección posterior).

El archivo de decisiones tiene que ser el texto del blob, tal como lo da `git show <rev>:docs/automation/decisions/I-62.md` (sin conversión de fin
de línea: un CR en el archivo detiene el script), porque la compuerta lee decisiones con `git show HEAD:…`.

Después de fijar los tres valores (sustituyendo solo esas tres líneas de owner_gate.py), la sesión principal vuelve a congelar el kit
(`python -B kit/make_manifest.py`) y repite, con la orden congelada, la sonda de registro y la prueba en seco: la compuerta 11 tiene que quedar en Ok
con AuthorizationSections = [esa sección] y LaterSections = [] (README del kit, «Fijar la autorización y volver a congelar»).
Uso: python -B kit/pin_authorization.py <archivo de decisiones> <número de sección>
"""
import hashlib
import importlib.util
import json
import os
import re
import sys

KIT = os.path.dirname(os.path.abspath(__file__))
_spec = importlib.util.spec_from_file_location("owner_gate", os.path.join(KIT, "owner_gate.py"))
OG = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(OG)


def values(text, number, blob):
    """Pura (la autoprueba la ejerce sobre una copia sintética de decisiones). text: decisiones como las da `git show`; number: la sección; blob: el
    de la ronda 4. → {Values, Checks, Ready}; Values = {AUTH_SECTION, AUTH_HEADING, AUTH_TEXT_SHA} (None si la sección no existe una sola vez)."""
    lines = text.split("\n")
    hits = [i for i, l in enumerate(lines) if (lambda m: m is not None and int(m.group(1)) == number)(OG.NUMBERED_RX.match(l))]
    vals = {"AUTH_SECTION": number, "AUTH_HEADING": None, "AUTH_TEXT_SHA": None}
    if len(hits) == 1:
        i = hits[0]
        vals["AUTH_HEADING"] = lines[i]
        vals["AUTH_TEXT_SHA"] = hashlib.sha256(OG.section_text(lines, i).encode("utf-8")).hexdigest()
    auth = {"Section": number, "Heading": vals["AUTH_HEADING"], "TextSha256": vals["AUTH_TEXT_SHA"]}
    secs = OG.authorization_sections(lines, blob, auth) if len(hits) == 1 else []
    later = OG.later_sections(lines, number)
    checks = {"AfterBaseSection": type(number) is int and number > OG.BASE_SECTION, "BaseSection": OG.BASE_SECTION,
              "SectionsWithThatNumber": len(hits), "SectionLine": (hits[0] + 1) if len(hits) == 1 else None,
              "HasBlobToken": bool(secs) and secs[0]["HasBlob"], "Blob": blob,
              "RecomputedByGateFunctions": bool(secs) and len(secs) == 1 and secs[0]["HeadingMatches"] and secs[0]["TextSha256Matches"],
              "LaterSections": later,
              "TextLines": len(OG.section_text(lines, hits[0]).split("\n")) if len(hits) == 1 else None}
    ready = checks["AfterBaseSection"] and len(hits) == 1 and checks["HasBlobToken"] and checks["RecomputedByGateFunctions"] and not later
    return {"Values": vals, "Checks": checks, "Ready": ready}


def python_lines(vals):
    """Las tres líneas de owner_gate.py que sustituyen a las que el kit entrega en None (comentario incluido)."""
    n = vals["AUTH_SECTION"]
    return ["AUTH_SECTION = %d   # decisiones §%d: disposición del Coordinator que autoriza el lanzamiento (pin_authorization.py)" % (n, n),
            "AUTH_HEADING = %s" % json.dumps(vals["AUTH_HEADING"], ensure_ascii=False),
            "AUTH_TEXT_SHA = %s   # SHA-256 del texto de §%d (pin_authorization.py)" % (json.dumps(vals["AUTH_TEXT_SHA"]), n)]


def main(argv):
    if len(argv) != 2 or not re.match(r"^[0-9]+$", argv[1]):
        print(__doc__)
        return 2
    b = open(argv[0], "rb").read()
    if b"\r" in b:
        print(json.dumps({"Refused": "el archivo tiene CR: usar el texto del blob (git show <rev>:docs/automation/decisions/I-62.md), sin conversión"},
                         ensure_ascii=False))
        return 2
    closure = json.load(open(os.path.join(KIT, "closure.json"), encoding="utf-8"))
    out = values(b.decode("utf-8"), int(argv[1]), closure["ObjectBlob"])
    if out["Values"]["AUTH_HEADING"] is not None:
        out["PythonLines"] = python_lines(out["Values"])
    out["FileSha256"] = hashlib.sha256(b).hexdigest()
    out["CurrentPins"] = OG.pinned_authorization()
    out["Next"] = ("sustituir en kit/owner_gate.py solo las tres líneas AUTH_SECTION, AUTH_HEADING y AUTH_TEXT_SHA por PythonLines; "
                   "python -B kit/make_manifest.py; y, con la orden congelada desde la carpeta que contiene kit/, --record-probe y --dry-run "
                   "(compuerta 11 en Ok, AuthorizationSections = esta sección, LaterSections = [])")
    print(json.dumps(out, ensure_ascii=False, indent=1))
    return 0 if out["Ready"] else 1


if __name__ == "__main__":
    sys.stdout.reconfigure(errors="backslashreplace")
    sys.exit(main(sys.argv[1:]))
