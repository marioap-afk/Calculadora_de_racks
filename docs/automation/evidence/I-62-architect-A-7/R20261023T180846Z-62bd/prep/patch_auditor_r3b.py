"""Segundo paso sobre kit/post-review.py de la ronda 3 (después de patch_auditor_r3.py, ya aplicado): la regla de EXACTITUD de las referencias
(decisiones §96, punto 6: «Exigir referencias relativas válidas, presentes en el cierre y comprobadas contra el archivo real»). Hasta v7.0-a7r3
una referencia se clasificaba por su efecto (classify colapsa `.`, `..`, barras invertidas y rutas absolutas dentro del clon, y los nombres de
las premisas del run se comparaban sin distinguir mayúsculas); ahora, además, la ruta TAL COMO SE ESCRIBIÓ tiene que ser exactamente la ruta
relativa del archivo del cierre (o el nombre exacto de la premisa del run); si no, es una referencia no canónica y no se acredita. El esquema ya
rechaza esas formas (patrón de familias de rutas); esta regla es la segunda capa, independiente del esquema. Uso:
python -I -B patch_auditor_r3b.py <kit> <prefijo del SHA-256 de partida>
"""
import hashlib
import os
import sys

p = os.path.join(sys.argv[1], "post-review.py")
t = open(p, encoding="utf-8", newline="").read()
assert hashlib.sha256(t.encode("utf-8")).hexdigest().startswith(sys.argv[2]), "post-review.py de partida (v7.0-a7r3 de patch_auditor_r3.py)"
R = [
    ("well formed but not a closure file, and the old message showed its absolute resolution. Coherence of the new fields: a compatibility verdict\n",
     "well formed but not a closure file, and the old message showed its absolute resolution; and the EXACTNESS of every reference: the path as\n"
     "written must be exactly the relative path of a closure file (or the exact name of a premise run file), so `..`, `.`, backslashes, absolute\n"
     "paths into the clone and other spellings that resolve to a closure file are non-canonical too (the schema rejects them first; this is the\n"
     "second, independent layer). Coherence of the new fields: a compatibility verdict\n"),
    ('        written = userless(str(ref))\n'
     '        ref_check.append({"Group": group, "Ref": shown, "Written": written, "Kind": kind, "Lines": [a, b], "InRange": in_range,\n',
     '        written = userless(str(ref))\n'
     '        exact = kind in ("CLOSURE", "RUN") and rel_in == rel    # v7.0-a7r3: the path as written = the closure path or the premise name\n'
     '        ref_check.append({"Group": group, "Ref": shown, "Written": written, "Exact": exact, "Kind": kind, "Lines": [a, b], "InRange": in_range,\n'),
    ('            add("referencia no canónica en %s (%s: %s)" % (group, written, why))\n'
     '        elif not delivered_ok:\n',
     '            add("referencia no canónica en %s (%s: %s)" % (group, written, why))\n'
     '        elif not exact:\n'
     '            add("referencia no canónica en %s (%s: no es la ruta relativa exacta de un archivo del cierre ni el nombre exacto de una premisa '
     'del run)" % (group, written))\n'
     '        elif not delivered_ok:\n'),
]
for a, b in R:
    assert t.count(a) == 1, ("no único", t.count(a), a[:100])
    t = t.replace(a, b)
open(p, "w", encoding="utf-8", newline="").write(t)
print("ok", hashlib.sha256(t.encode("utf-8")).hexdigest())
