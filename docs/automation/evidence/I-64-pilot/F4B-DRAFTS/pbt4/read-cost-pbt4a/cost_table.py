import sys, re, hashlib
import xml.etree.ElementTree as ET

sys.stdout.reconfigure(encoding="utf-8")
NS = {'t': 'http://microsoft.com/schemas/VisualStudio/TeamTest/2010'}
LINE = re.compile(r'^medida \| (.+?) \| (.+?) \| mediana=([0-9.]+) ms \| p95=([0-9.]+) ms \| n=(\d+)$')


def parse(path):
    data = open(path, 'rb').read()
    root = ET.fromstring(data)
    out = {}
    header = []
    for r in root.findall('.//t:UnitTestResult', NS):
        msg = r.find('.//t:ErrorInfo/t:Message', NS)
        for line in (msg.text or '').splitlines():
            m = LINE.match(line.strip())
            if m:
                out[(m.group(1), m.group(2))] = (float(m.group(3)), float(m.group(4)), int(m.group(5)))
            elif line.startswith('copia=') or line.startswith('entorno') or line.startswith('escenario'):
                header.append(line.strip())
    return out, header, hashlib.sha256(data).hexdigest().upper()


# pairs: label=base.trx,green.trx (one or more runs)
runs = []
for arg in sys.argv[1:]:
    label, paths = arg.split('=', 1)
    b, g = paths.split(',')
    runs.append((label, parse(b), parse(g)))

BASE = 'base (piloto+FromPilot; Begin y las cinco relecturas)'
EMU = 'base-emulada (piloto+FromPilot)'
CASES = [
    ('Begin', 'green Begin (piloto+completo+FromReads)'),
    ('relectura, origen encontrado', 'green relectura origen-encontrado'),
    ('relectura, cae al Primary', 'green relectura cae-al-Primary (sin coincidencia)'),
    ('relectura, el origen es el Primary', 'green relectura origen-es-Primary'),
]

for label, (bdata, bhead, bsha), (gdata, ghead, gsha) in runs:
    print('### ' + label)
    print()
    print('TRX base sha256 ' + bsha + '; TRX GREEN sha256 ' + gsha)
    print()
    for h in bhead[:2] + ghead[:2]:
        print('- ' + h)
    print()
    scen = []
    for (s, c) in gdata:
        if s not in scen:
            scen.append(s)
    print('| Escenario | Caso | Base 5aaacce3, mediana / p95 (ms) | GREEN faef7020, mediana / p95 (ms) | Delta mediana (ms) | Factor (otra copia) | Base emulada en la copia GREEN (ms) | Factor (mismo proceso) |')
    print('|---|---|---|---|---|---|---|---|')
    for s in scen:
        b = bdata.get((s, BASE))
        e = gdata.get((s, EMU))
        for name, key in CASES:
            g = gdata.get((s, key))
            if not g or not b:
                continue
            print('| %s | %s | %.2f / %.2f | %.2f / %.2f | %+.2f | %.2fx | %.2f | %.2fx |' % (
                s, name, b[0], b[1], g[0], g[1], g[0] - b[0], g[0] / b[0], e[0], g[0] / e[0]))
    print()
    print('| Escenario | Parte | Base (ms, mediana) | GREEN (ms, mediana) |')
    print('|---|---|---|---|')
    for s in scen:
        for part in ['parte: solo el escaneo (ctor)', 'parte: solo BuildStaleBase', 'parte: solo el piloto', 'parte: solo el completo']:
            b = bdata.get((s, part))
            g = gdata.get((s, part))
            print('| %s | %s | %s | %s |' % (s, part.replace('parte: ', ''), '%.3f' % b[0] if b else '-', '%.3f' % g[0] if g else '-'))
    print()
