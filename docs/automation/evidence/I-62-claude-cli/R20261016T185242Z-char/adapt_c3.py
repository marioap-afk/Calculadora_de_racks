# Derives char2_c3.py (C3 probe of the claude-cli 2.1.299 characterization, decisiones §87) from the A-2 kit's probe-c3.py (decisiones §56,
# point 7) by exact substitutions; diff in char2_c3.diff. Each substitution must match once.
import difflib, hashlib, os
D = os.path.dirname(os.path.abspath(__file__))
src = open(os.path.join(D, 'probe-c3.a2.py'), 'rb').read()
assert hashlib.sha256(src).hexdigest().startswith('b0608cb47438e5d7')
t = src.decode('utf-8')
orig = t
SUBS = [
    ('"""I-62 A-2 corregida: sonda acotada C3 de claude-cli (decisiones §56, punto 7; Owner CLAUDE-CLI-I62 = A), vía (a) para cerrar la desviación D-01\n',
     '"""I-62 §87: sonda C3 de la caracterización de claude-cli 2.1.299 (decisiones §87, punto 2; procedimiento de §56, punto 7), derivada por\n'
     'sustituciones exactas (adapt_c3.py) de la sonda C3 del kit de A-2. Original: sonda acotada C3 de claude-cli, vía (a) para cerrar la desviación D-01\n'),
    ('import uuid\n', 'import time\nimport uuid\n'),
    ('PROBE_CWD = r"D:\\r62-cli-char"\n', 'PROBE_CWD = r"D:\\r62-cli-char2"\n'),
    ('REVIEW_CLONE = r"D:\\r62-arch-a2r"\n', 'REVIEW_CLONE = r"D:\\r62-arch-a6r4"\n'),
    ('PROBE_ROOT = r"D:\\r62-c3-probe"\n', 'PROBE_ROOT = r"D:\\r62-c3-probe2"\n'),
    ('    closure = json.load(open(os.path.join(KIT, "closure.json"), encoding="utf-8"))\n    t = closure["Transport"]\n    template, binary = TG.template_of(t), TG.binary_of(closure)\n',
     '    closure = json.load(open(os.path.join(KIT, "closure.r4kit.json"), encoding="utf-8"))   # plantilla, modelo y effort del kit de la ronda 4\n'
     '    t = closure["Transport"]\n'
     '    template = TG.template_of(t)\n'
     '    binary = {"Path": "%APPDATA%\\\\Claude\\\\claude-code\\\\2.1.299\\\\f0584be0fbaf\\\\claude.exe",                    # el binario que se caracteriza\n'
     '              "Sha256": "295b49ec092f31a7de9c6ff286d9d8f120a4f029b9ad9a5ac0b12c35cbf4df2b", "Version": "2.1.299 (Claude Code)"}\n'),
    ('    ver = TG.cli_version(bin_path, KIT)\n', '    ver = TG.cli_version(bin_path, KIT)["Version"]   # el transport_gate de la ronda 4 devuelve un registro\n'),
    ('    if auth.get("loggedIn") is not True:\n        return refuse("auth status sin loggedIn", Auth=auth)\n',
     '    if auth.get("loggedIn") is not True:\n        return refuse("auth status sin loggedIn", Auth=auth)\n'
     '    time.sleep(120)   # AuthSettle, como el lanzador: auth status puede renovar el token OAuth y chocar con el proceso de la sonda\n'),
]
for a, b in SUBS:
    assert t.count(a) == 1, a[:60]
    t = t.replace(a, b)
open(os.path.join(D, 'char2_c3.py'), 'w', encoding='utf-8', newline='\n').write(t)
diff = ''.join(difflib.unified_diff(orig.splitlines(True), t.splitlines(True), 'probe-c3.py (kit de A-2)', 'char2_c3.py'))
open(os.path.join(D, 'char2_c3.diff'), 'w', encoding='utf-8', newline='\n').write(diff)
print('ok', len(SUBS), hashlib.sha256(t.encode('utf-8')).hexdigest()[:16])
