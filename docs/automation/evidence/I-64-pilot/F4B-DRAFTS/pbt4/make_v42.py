"""Arma la v4.2 del paquete de PB-T4 desde los fragmentos de v42/ y las secciones de la v4.1 que no cambian.

Uso: python -I make_v42.py <checkout> <rev> <salida-v42.md>
Lee, en esta carpeta: I-64-f4b-pbt4-gate-contract-package-v41.md, v42/*.md, pbt4-substitution-lists.tsv, pbt4d-retire-list.tsv,
copy_map_pbt4-out.txt, measure_pbt4_v42-out.txt y retire_compile_pbt4-out.txt; y el contrato de PB-T3c por `git show <rev>:...`.
Escribe solo <salida-v42.md>.
"""
import json
import re
import subprocess
import sys
from collections import Counter

repo, rev, out_path = sys.argv[1:4]
here = __file__.replace('make_v42.py', '')


def read(name):
    with open(here + name, encoding='utf-8') as f:
        return f.read()


def replace_once(text, old, new, what):
    n = text.count(old)
    assert n == 1, '%s: %d coincidencias' % (what, n)
    return text.replace(old, new)


def section(text, start, end, new, what):
    a = text.index(start) if start else 0
    if start:
        assert text.count(start) == 1, what
    if end is None:
        return text[:a] + new
    b = text.index(end, a + 1)
    return text[:a] + new.rstrip('\n') + '\n\n' + text[b:]


v41 = read('I-64-f4b-pbt4-gate-contract-package-v41.md')

# ------------------------------------------------------------------ los números
rows = [l.split('\t') for l in read('pbt4-substitution-lists.tsv').splitlines()[1:]]
cnt = Counter(r[0] for r in rows)
N = {'N_C0': cnt['PB-T4b C-0'], 'N_C5': cnt['PB-T4b C-5'], 'N_UI': cnt['PB-T4b UI'], 'N_C7': cnt['PB-T4b C-7']}
assert N['N_C0'] + N['N_C5'] + N['N_UI'] == 470 and N['N_C7'] == 139 and cnt['PB-T4a'] == 1 and cnt['PB-T4c'] == 2
mech = sum(1 for r in rows if r[0] == 'PB-T4b C-0' and 'copia mecánica' in r[4])
hand = sum(1 for r in rows if r[0] == 'PB-T4b C-0' and 'a mano' in r[4])
mix = re.search(r"a mano por motivo \[(.*)\]", read('copy_map_pbt4-out.txt')).group(1)
N['N_HAND'] = hand
N['HAND_MIX'] = '; '.join('%s %s' % (a, b) for a, b in re.findall(r"\('([^']*)', (\d+)\)", mix))
d = [l.split('\t') for l in read('pbt4d-retire-list.tsv').splitlines()[1:]]
dc = Counter(r[0] for r in d)
assert dc == Counter({'PB-T4d C-7': 139, 'PB-T4d original': 55, 'PB-T4d apoyo': 29}), dc
c7_text = sum(1 for r in rows if r[0] == 'PB-T4b C-7' and 'por compilación' not in r[4])
c7_comp = N['N_C7'] - c7_text
N['COTA_TABLE'] = '\n'.join([
    '| Subtarea | Lista | Lo que vio el Owner | Exacto | Su estado |',
    '|---|---|---|---|---|',
    '| PB-T4a | CT-PBT3A-12 (C-5) | 1 | **1** | Autorización condicional (pregunta 4) |',
    '| PB-T4b | Las 9 clases (C-0), «Saltar y duplicar»: %d copias mecánicas y %d equivalentes a mano | 428 | **%d** | |' % (mech, hand, N['N_C0']),
    '| PB-T4b | La letra de las guardas (C-5 y C-6) | «unas 12» | **%d** | |' % N['N_C5'],
    '| PB-T4b | La UI de la elección (C-3) | «hasta 21», recontada a unas 25 | **%d** | |' % N['N_UI'],
    '| PB-T4b | **Total** | **unas 465**; en la segunda vuelta, **470** | **470** | **Aprobada como «la que vio»** (segunda vuelta, pregunta 2); `pbt4b-substitution-list.tsv`, sha256 `44393b19d1f7490081c8ec9bac633ea4acbfe792103e5e5c3ca5e1f3fa1163e4` |',
    '| PB-T4c | PEG:155-163 y CT-PBT1B-15 (C-5) | 2 | **2** | Autorización condicional |',
    '| PB-T4d | C-7: %d por texto (con E-3, E-5 y E-6) y %d por compilación | «unas 100»; en la segunda vuelta, 139 | **139** | **Vuelve al Owner con su lista** (esbozo en `pbt4d-retire-list.tsv`, sha256 `1c9d67e8f2dac7eae3202d61a23ff5131e96123e5a6af275e5ce8a8bf71b8b71`) |' % (c7_text, c7_comp),
    '| PB-T4d | Las originales de las 9 clases que dejan de compilar al retirar, borradas con registro | 55 | **55** | Ídem |',
    '| PB-T4d | Los miembros de apoyo que nombran lo retirado | 29 | **29** | Ídem |',
    '',
    '**Las cifras que corrige la v4.2** (M-32): de las %d pruebas que dejan de compilar al retirar, `WorkspacePilotUpdateRunTests` da **42** y `PushBackFullWritePrepareTests`, **6**; en C-7, por texto y por compilación, son **48** (de las 49 de la clase) y **9** (de 9). Las demás de compilación: `WorkspaceDraftPilotOwnershipTests` 8, `WorkspaceDraftPilotStateTests` 7, `PushBackDeclaredCorrectionPilotParityTests` 2, `PushBackFullWriteRunTests` 1 y `WorkspaceDraftPilotMaterializationTests` 1.' % c7_comp,
    '',
    '**Lo que cambia y lo que no al retirar después de PB-T4c** (I-26): la retirada no evita el borrado de las 55 originales saltadas y de los 29 miembros de apoyo; cuando se retire, dejarán de compilar igual. Lo que cambia es que, para entonces, ya no corre ninguna, porque sus copias y sus equivalentes están en verde desde PB-T4b, y que PB-T4b conserva la premisa de la pregunta 3 de la primera vuelta.',
    '',
    '**Las 470 no dependen de la retirada** (M-33): las 9 de la letra que nombran lo que se retirará caen por el cambio del anfitrión (§4.2).',
])
g = read('measure_pbt4_v42-out.txt').splitlines()
start = next(i for i, l in enumerate(g) if l.startswith('| Guarda'))
N['GUARDS42'] = '\n'.join(l for l in g[start:] if l.startswith('|')) + '\n\n' + [l for l in g if l.startswith('rojas=')][0].replace(
    'rojas=', '**En rojo:** ').replace('sobre el GREEN; contrafactuales en rojo=', 'sobre el GREEN; contrafactuales en rojo, como se espera: ') + '.'

# ------------------------------------------------------------------ los literales del contrato de PB-T3c (como en make_v41.py)
p = subprocess.run(['git', '-C', repo, 'show', rev + ':docs/automation/evidence/I-64-pilot/F4B-PBT3C-FULL-WRITE-ERASE/coordinator/gate-contract.json'], capture_output=True)
assert p.returncode == 0
c = json.loads(p.stdout.decode('utf-8'))
stops = {s['Id']: s['Text'] for s in c['StopConditions']}
std = [s for s in c['StopConditions'] if not s['Id'].startswith('PBT3C-')]
assert len(std) == 17
N['STOPS_STD'] = '\n'.join('- **%s:** `%s`' % (s['Id'], s['Text']) for s in std)
for k in ['C01', 'C05', 'C06', 'C08', 'C09', 'C12', 'C17', 'C18']:
    N[k] = stops['PBT3C-' + k]
old_recipes = '(CT-PBT3A-04, CT-PBT3B-06 and 10)'
assert stops['PBT3C-C07'].count(old_recipes) == 1
N['C07_A'] = stops['PBT3C-C07'].replace(old_recipes, '(the self-tests of CT-PBT4A-07 and CT-PBT4A-08)')
c11 = stops['PBT3C-C11']
assert c11.endswith('no one-off acceptance extends to PB-T3c') and 'PB-T3c' not in c11[:-len('PB-T3c')]
N['C11_A'] = c11.replace('PBT3C', 'PBT4')[:-len('PB-T3c')] + 'PB-T4a'
ee = c['ExpectedEvidence']
for i in [0, 1, 2, 4, 5, 6, 7, 9, 10, 12]:
    N['EE%d' % i] = ee[i]
old8 = 'En PB-T3c, ademas: RedSha..RedRepairSha no toca WR ni la fachada (RackViewBlockEraser.cs).'
assert ee[8].count(old8) == 1
N['EE8_A'] = ee[8].replace(old8, 'En PB-T4a, ademas: RedSha..RedRepairSha no toca ningun archivo de src/ (el esqueleto) ni PushBackDraftWriteGuardTests.cs (el Skip de CT-PBT3A-12).')
common = []
for text in c['Invariants']:
    num = int(re.match(r'INV-PBT3C-(\d+)\. ', text).group(1))
    if num == 7:
        common.append('INV-PBT4A-07. Las redes: section 5 en verde. Ninguna prueba existente se edita, se salta, se reactiva ni se borra, con una unica salvedad: el Skip de CT-PBT3A-12, con su motivo literal. En CurrentSha, el git diff -U0 BaseSha..CurrentSha de PushBackDraftWriteGuardTests.cs solo lleva la linea de atributo con Skip.')
    elif num == 13:
        common.append('INV-PBT4A-13. Las declaraciones nuevas en src/: las de la lista cerrada de PBT4A-C13 (con ReadAt), y ninguna mas, por nombre y acotada a los archivos que tocan, sobrecargas incluidas. No se admite ningun ayudante privado, tampoco en los dos archivos nuevos: el texto literal del paquete no lo necesita.')
    elif 8 <= num <= 14:
        common.append(text.replace('INV-PBT3C-', 'INV-PBT4A-'))
assert [int(re.match(r'INV-PBT4A-(\d+)', t).group(1)) for t in common] == list(range(7, 15))
N['INV_COMMON'] = '\n'.join(common)
rt = [('tests/RackCad.Tests/RackCad.Tests.csproj', 'FullyQualifiedName~RackCad.Tests.PushBackFullReadPathTests', 5, True),
      ('tests/RackCad.Tests/RackCad.Tests.csproj', 'FullyQualifiedName~RackCad.Tests.PushBackFullReadGuardTests', 3, False)]
for t in c['RequiredTests']:
    m = t['MinSelected']
    if t['Filter'] == 'FullyQualifiedName~RackCad.Tests.PushBackDraftWriteGuardTests':
        assert m == 3
        m = 2
    rt.append((t['Project'], t['Filter'], m, False))
assert len(rt) == 38
N['RT_TABLE'] = '\n'.join(['| # | Project | Filter | MinSelected | ExpectRed |', '|---|---|---|---|---|'] + [
    '| %d | `%s` | `%s` | %s%d%s | %s |' % (i, a, b, '**' if (b.endswith('PushBackDraftWriteGuardTests') or i <= 2) else '', m,
                                       '**' if (b.endswith('PushBackDraftWriteGuardTests') or i <= 2) else '', 'true' if e else 'false')
    for i, (a, b, m, e) in enumerate(rt, 1)])

# ------------------------------------------------------------------ las secciones (sobre la v4.1)
out = v41
out = section(out, None, '## Registro de cambios v4 → v4.1', read('v42/f-head.md'), 'cabecera')
out = section(out, '## 0. En una página', '## 1. Autoridades', read('v42/f-s0.md'), '§0')
out = section(out, '#### D-PBT4-02.', '### PB-T4b', read('v42/f-d02-03.md'), 'D-02/03')
out = section(out, '#### D-PBT4-09.', '#### D-PBT4-11.', read('v42/f-d09.md').rstrip('\n') + '\n\n' + read('v42/f-d10.md'), 'D-09/10')
out = section(out, '#### D-PBT4-11.', '### PB-T4c', read('v42/f-d11.md'), 'D-11')
out = section(out, '## 4. Las guardas, por subtarea', '## 5. La red de pruebas', read('v42/f-s4.md'), '§4')
out = section(out, '## 5. La red de pruebas', '## 6. Los contratos propuestos', read('v42/f-s5.md'), '§5')
out = section(out, '## 6. Los contratos propuestos', '## 7. Riesgos y lecciones aplicadas', read('v42/f-s6.md'), '§6')
out = section(out, '## 7. Riesgos y lecciones aplicadas', '## 8. Las decisiones del Owner', read('v42/f-s7.md'), '§7')
out = section(out, '## 8. Las decisiones del Owner', '## 9. Límites', read('v42/f-s8.md') + '\n---\n', '§8')
out = section(out, '## 9. Límites', None, read('v42/f-s9-11.md'), '§9-11')

FIXES = [
    ('### D-PBT4-01. Tres subtareas secuenciales\n\n**El orden:** PB-T4a sobre el cierre de PB-T3c (`7f0d9aa3`); PB-T4b sobre el de PB-T4a; PB-T4c sobre el de PB-T4b. Cada una con su 1/3 (OB-10.1), sin rebase.\n\n**Por qué tres:**',
     '### D-PBT4-01. Cuatro subtareas secuenciales (V42-01)\n\n**El orden:** PB-T4a sobre el cierre de PB-T3c (`7f0d9aa3`); PB-T4b sobre el de PB-T4a; PB-T4c sobre el de PB-T4b; y PB-T4d, la retirada del piloto, sobre el de PB-T4c, cuando el Owner apruebe su lista. Cada una con su 1/3 (OB-10.1), sin rebase.\n\n**Por qué así:**'),
    ('- PB-T4c es casi aditiva, con dos sustituciones por letra.\n',
     '- PB-T4c es casi aditiva, con dos sustituciones por letra;\n- PB-T4d retira el código del piloto cuando ya no corre ninguna prueba que lo use (D-PBT4-10).\n'),
    ('tienen como mucho 26 caracteres (23, 25 y 24), para los sustitutos de nc1 y nc2 (§6.0; V41-11).',
     'tienen como mucho 26 caracteres (23, 25, 24 y, el propuesto de PB-T4d, 21), para los sustitutos de nc1 y nc2 (§6.0; V41-11).'),
    ('**El código del piloto se retira** (Q1 del Owner; D-PBT4-10).', '**El código del piloto queda sin uso, y lo retira PB-T4d, tras PB-T4c** (D-PBT4-10; V42-01).'),
    ('**La copia del piloto** de la regla (`PushBackPilotFieldAdapter`) se retira con él (Q1 del Owner; D-PBT4-10): queda una sola regla.',
     '**La copia del piloto** de la regla (`PushBackPilotFieldAdapter`) se retira con él, en PB-T4d (D-PBT4-10); hasta entonces queda sin uso, y la única regla que usa el panel es `PushBackAnnotationScaleRule`.'),
    ('| A-T3-1, A-5 y R-T1-17 | PB-T4b: se retira (Q1 del Owner) | D-PBT4-10 |', '| A-T3-1, A-5 y R-T1-17 | PB-T4d: se retira tras PB-T4c (V42-01) | D-PBT4-10 |'),
    ('(§8.2). Desde la v4.1, Q1 a Q4 siguen la numeración de ese registro (la v4 llamaba Q3 a la emisión y Q4 al procedimiento) |',
     '(§8.2). Desde la v4.1, Q1 a Q4 siguen la numeración de ese registro (la v4 llamaba Q3 a la emisión y Q4 al procedimiento) |\n'
     '| **Las decisiones del Owner sobre la retirada y la lista de PB-T4b** (`owner-pbt4b-retire-list-decisions.md`, 2026-11-20) | «Retirar tras PB-T4c» y «Sí, esa lista» (§8.2) |'),
]
for old, new in FIXES:
    out = replace_once(out, old, new, old[:50])
for k, v in N.items():
    out = out.replace('{{%s}}' % k, str(v))
left = re.findall(r'\{\{[A-Z0-9_]+\}\}', out)
assert not left, left
assert '<a medir en la comprobación del §6.0>' not in out and '<tras el cierre de PB-T3c>' not in out
with open(out_path, 'w', encoding='utf-8', newline='\n') as f:
    f.write(out)
stale = [w for w in ['se retira en PB-T4b', 'Retirar en PB-T4b»', 'los archivos de `Pilot/` que retira;', 'measure_pbt4_v41-out', '32 de 32',
                     'piloto=1 completo=1 lecturas', 'Lo que sale de esta versión, para la sesión', 'vuelven al Owner antes de emitir PB-T4b'] if w in out]
print('bytes', len(out.encode('utf-8')))
print('restos a revisar:', stale)
