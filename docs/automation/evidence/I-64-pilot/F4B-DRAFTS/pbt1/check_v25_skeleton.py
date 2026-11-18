# Solo lectura: comprueba que el esqueleto del section 5.3 de la v2.5 local es el de la v2.4 custodiada con UN solo cambio, el
# fragmento de c-09 (" `ResolveWeightUnit` devuelve `requested`;") tras el de c-02, y que su forma ASCII es la que fija
# make_pbt1b_contract.py (SKELETON). No escribe nada.
# Uso: python -I check_v25_skeleton.py <paquete-v2.4.md> <paquete-v2.5.md>
import sys
import unicodedata

sys.stdout.reconfigure(encoding="utf-8")
MARK = "**El esqueleto exacto:**"
FRAG = " `ResolveWeightUnit` devuelve `requested`;"
AFTER = "`ResolvePostCatalogId` devuelve `requested`;"


def skeleton(path):
    with open(path, encoding="utf-8") as f:
        lines = f.read().replace("\r\n", "\n").split("\n")
    assert lines.count(MARK) == 1, (path, lines.count(MARK))
    out = []
    for l in lines[lines.index(MARK) + 1:]:
        if not l.startswith("- "):
            break
        out.append(l)
    return "\n".join(out)


def fold(s):
    s = s.replace(chr(0xAB), '"').replace(chr(0xBB), '"').replace(chr(0x2014), "-").replace(chr(0x2013), "-").replace(chr(0x2192), "->")
    return "".join(c for c in unicodedata.normalize("NFKD", s) if not unicodedata.combining(c))


v24, v25 = skeleton(sys.argv[1]), skeleton(sys.argv[2])
expected = v24.replace(AFTER, AFTER + FRAG, 1)
print("v2.4: %d lineas, %d caracteres" % (v24.count("\n") + 1, len(v24)))
print("v2.5: %d lineas, %d caracteres" % (v25.count("\n") + 1, len(v25)))
print("v2.5 == v2.4 con el fragmento de c-09:", v25 == expected)
print("--- v2.5 (ASCII) ---")
print(fold(v25))
