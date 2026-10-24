"""Paso A de la derivación del kit de la RONDA 4 de A-7 (decisiones §98, punto 7): desplaza una ronda los componentes del kit de la ronda 3 que
dependen de la ronda (la versión PENDIENTE custodiada en 000fd6eb, copiada en base3/), con sustituciones globales ordenadas y marcadores (sin
encadenar): identificadores del lanzamiento, objeto y ronda previa (blobs y commits), revisión previa, decisiones §95-§96 → §97-§98 (§93 y §94 son
anclas históricas y no se desplazan), secciones de
corrección de A-7 (§3.6 → §3.7, §3.7 → §3.8), ids de hallazgos y preguntas, rondas y directorios de evidencia. El resultado (shifted/) no es el
kit: el paso B (patch_*_r4.py) hace las ediciones semánticas sobre él. Uso: python -I -B shift_r4.py
"""
import hashlib, os, re

PREP = os.path.dirname(os.path.abspath(__file__))
BASE3 = os.path.join(os.path.dirname(PREP), 'base3')
OUT = os.path.join(PREP, 'shifted')
FILES = ['owner_gate.py', 'post-review.py', 'make_closure.py', 'prompt.template.md', 'launch.py', 'verify_clone.py', 'prepare_clone.py',
         'finish_kit.py', 'make_manifest.py', 'mutation-post-review.py', 'selftest-post-review.py']
SHA = {'owner_gate.py': '7e0cf27a', 'post-review.py': 'e1d6b1b2', 'make_closure.py': '2fbbd6a8', 'prompt.template.md': '05250607', 'launch.py': '87ff62ff',
       'verify_clone.py': '6ccdcb32', 'prepare_clone.py': '82082790', 'finish_kit.py': 'f82cbc31', 'make_manifest.py': '575e04fa',
       'mutation-post-review.py': 'cf120dd8', 'selftest-post-review.py': '318ef540'}
# (antiguo, nuevo) en orden; '\x00k\x00' son marcadores que se expanden al final
STEPS = [
    # identificadores del lanzamiento de la ronda 3 -> los de la ronda 4 (prep/ids.txt)
    ('R20261023T180846Z-62bd', '\x00RUN\x00'), ('I20261023T180846Z-62bd', '\x00INV\x00'), ('L20261023T180846Z-62bd', '\x00LRR\x00'),
    ('a720544e-1ac0-43d9-96bd-ae39e1090dfb', '\x00SES\x00'), ('20261023T180846Z', '\x00TS\x00'),
    # revisión previa: la de la ronda 2 -> la de la ronda 3
    ('R20261019T201030Z-d1bf', '\x00REV3\x00'),
    # objeto y ronda previa
    ('8bcccb2fd2cea480f73ee5bf5a1583e0c9eaf1ff', '\x00OBJF\x00'), ('8a528133c7cccd83356e31f8fd79cd2ebb0cec15', '\x00PREVF\x00'),
    ('8bcccb2f', '\x00OBJS\x00'), ('8a528133', '\x00PREVS\x00'),
    # commits: publicación y registro de la ronda 3 -> de la ronda 4; su padre; publicación de la ronda 2 -> de la ronda 3
    ('37ac6ca4a6f0a04268ac357764bb74f843eb3a08', '\x00PUBF\x00'), ('b9bfae50a82ba59c834658e54d7699662ee298bb', '\x00REGF\x00'),
    ('5da2007f45160ced37b958a3a772c46ac64874ee', '\x00PARF\x00'), ('e90b444d5d23288e0b5a59881820377bee0d7d09', '\x00PPUBF\x00'),
    ('37ac6ca4', '\x00PUBS\x00'), ('b9bfae50', '\x00REGS\x00'), ('5da2007f', '\x00PARS\x00'), ('e90b444d', '\x00PPUBS\x00'),
    # decisiones: la cadena de las revisiones de A-7 (§93-§96 -> §95-§98)
    ('§97', '\x00S99\x00'),   # en el kit pendiente de la ronda 3, §97 solo es la disposición sintética de la autoprueba (base + 1)
    ('§96', '\x00S98\x00'), ('§95', '\x00S97\x00'),   # §94 y §93 son anclas históricas (fidelidad, §94, punto 5; tope, §93, punto 6): no se desplazan
    ('BASE_SECTION, LAST_SECTION = 96, 96', 'BASE_SECTION, LAST_SECTION = \x00N98\x00, \x00N98\x00'),
    ('BASE_SECTION = 96', 'BASE_SECTION = \x00N98\x00'), ('LAST_SECTION = 96', 'LAST_SECTION = \x00N98\x00'),
    ('SYNTH_AUTH_SECTION = 97', 'SYNTH_AUTH_SECTION = \x00N99\x00'), ('Decisions96', 'Decisions\x00N98\x00'),
    # secciones de corrección de A-7
    ('§3.7', '\x00A738\x00'), ('§3.6', '\x00A737\x00'),
    # ids de hallazgos y de preguntas
    ('A62-A7R3-', '\x00F4\x00'), ('A62-A7R2-', '\x00F3\x00'), ('Q-A7R3-', '\x00Q4\x00'), ('Q-A7R2-', '\x00Q3\x00'),
    ('A7R3', '\x00I4\x00'), ('A7R2', '\x00I3\x00'), ('a7r3', '\x00i4\x00'), ('a7r2', '\x00i3\x00'), ('a7-r2.md', '\x00prem\x00'),
    ('R2R1', '\x00R3R1\x00'),
    # rondas y directorios de evidencia
    ('I-62-A7/r3', '\x00D4\x00'), ('I-62-A7/r2', '\x00D3\x00'), ('+ "r3/"', '+ "\x00d4\x00/"'), ('+ "r2/"', '+ "\x00d3\x00/"'),
    ('+ "/r3"', '+ "/\x00d4\x00"'), ('+ "/r2"', '+ "/\x00d3\x00"'),
    ('ronda 3', '\x00r4\x00'), ('ronda 2', '\x00r3\x00'), ('Ronda 3', '\x00R4\x00'), ('Ronda 2', '\x00R3\x00'), ('RONDA 3', '\x00RR4\x00'),
    ('RONDA 2', '\x00RR3\x00'), ('round 3', '\x00e4\x00'), ('round 2', '\x00e3\x00'), ('ROUND 3', '\x00E4\x00'), ('ROUND 2', '\x00E3\x00'),
    ('round-3', '\x00h4\x00'), ('round-2', '\x00h3\x00'), ('Round 3', '\x00U4\x00'), ('Round 2', '\x00U3\x00'),
    ('rondas 1 y 2', '\x00rr13\x00'), ('rounds 1 and 2', '\x00ee13\x00'), ('tercera candidata', '\x00cand4\x00'), ('TERCERA', '\x00T4\x00'),
    ('L1-L22', '\x00L27\x00'),
]
EXPAND = {'RUN': 'R20261024T041128Z-4825', 'INV': 'I20261024T041128Z-4825', 'LRR': 'L20261024T041128Z-4825',
          'SES': 'bd018dcb-165f-437d-bcfc-c8adaed5d774', 'TS': '20261024T041128Z', 'REV3': 'R20261023T180846Z-62bd',
          'OBJF': 'ebb3a87b0b33ddf1d6418f4e9f4fb4195d1555d3', 'PREVF': '8bcccb2fd2cea480f73ee5bf5a1583e0c9eaf1ff', 'OBJS': 'ebb3a87b', 'PREVS': '8bcccb2f',
          'PUBF': '99de338d105640f9390201678f02dd2a1d3f69a8', 'REGF': '94a04af7c663eac44016b4011cdffead714a530a',
          'PARF': '2405a919b8ce42c8b0b971d97e5f20294059b1e9', 'PPUBF': '37ac6ca4a6f0a04268ac357764bb74f843eb3a08',
          'PUBS': '99de338d', 'REGS': '94a04af7', 'PARS': '2405a919', 'PPUBS': '37ac6ca4',
          'S99': '§99', 'S98': '§98', 'S97': '§97', 'S96': '§96', 'S95': '§95', 'N98': '98', 'N99': '99', 'A738': '§3.8', 'A737': '§3.7',
          'F4': 'A62-A7R4-', 'F3': 'A62-A7R3-', 'Q4': 'Q-A7R4-', 'Q3': 'Q-A7R3-', 'I4': 'A7R4', 'I3': 'A7R3', 'i4': 'a7r4', 'i3': 'a7r3',
          'prem': 'a7-r3.md', 'R3R1': 'R3R1', 'D4': 'I-62-A7/r4', 'D3': 'I-62-A7/r3', 'd4': 'r4', 'd3': 'r3',
          'r4': 'ronda 4', 'r3': 'ronda 3', 'R4': 'Ronda 4', 'R3': 'Ronda 3', 'RR4': 'RONDA 4', 'RR3': 'RONDA 3', 'e4': 'round 4', 'e3': 'round 3',
          'E4': 'ROUND 4', 'E3': 'ROUND 3', 'h4': 'round-4', 'h3': 'round-3', 'U4': 'Round 4', 'U3': 'Round 3', 'rr13': 'rondas 1 a 3',
          'ee13': 'rounds 1 to 3', 'cand4': 'cuarta candidata', 'T4': 'CUARTA', 'L27': 'L1-L27'}


# después de expandir: citas literales de la orden que cambian de redacción o de punto entre §96 y §98
POST = [
    ('«Preparar el kit acotado, pero no lanzar una nueva revisión formal sin disposición posterior»',
     '«Preparar el kit acotado, pero no ejecutar revisión formal sin nueva disposición»'),
    ('«no lanzar una nueva revisión formal sin disposición posterior»', '«no ejecutar revisión formal sin nueva disposición»'),
    ('§98, punto 3: «No modificar A-6»', '§98, punto 8: «No modificar A-1..A-6, producción, I-65 ni D.5»'),
    ('(decisiones §90; §98, punto 3)', '(decisiones §90; §98, punto 8)'),
    ('§98, punto 6: «Exigir referencias relativas válidas', '§96, punto 6: «Exigir referencias relativas válidas'),
    ('sin lanzar una revisión formal nueva sin una disposición posterior', 'sin ejecutar una revisión formal sin una disposición nueva'),
    # el cuerpo pegado de la orden: §96 (3 198 bytes, adb11535…) -> §98 (3 740 bytes, 8b73975d…)
    ('adb11535f24cac2874ddef13af712cc56eebece8905fe948db2f0077b3dd87b2', '8b73975d9822a430f1a726268d4005609974dbdcc85a0674bf00dbb9e9018acf'),
    ('("96", "8b73975d', '("98", "8b73975d'), ('3 198', '3 740'), ('3198', '3740'),
]


def shift(t):
    for a, b in STEPS:
        t = t.replace(a, b)
    t = re.sub('\x00([A-Za-z0-9]+)\x00', lambda m: EXPAND[m.group(1)], t)
    assert '\x00' not in t
    for a, b in POST:
        t = t.replace(a, b)
    return t


os.makedirs(OUT, exist_ok=True)
for n in FILES:
    b = open(os.path.join(BASE3, n), 'rb').read()
    assert hashlib.sha256(b).hexdigest().startswith(SHA[n]), n
    s = shift(b.decode('utf-8'))
    open(os.path.join(OUT, n), 'wb').write(s.encode('utf-8'))
    print('%-26s %s' % (n, hashlib.sha256(s.encode('utf-8')).hexdigest()[:16]))
