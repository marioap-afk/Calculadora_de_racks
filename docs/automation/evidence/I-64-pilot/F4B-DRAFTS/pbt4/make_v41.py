"""Arma la v4.1 del paquete de PB-T4 desde la v4, los fragmentos de v41/ y las mediciones de la v4.1.

Uso: python -I make_v41.py <checkout> <rev> <v4.md> <salida-v41.md>
Lee: la v4, v41/*.md, pbt4-substitution-lists.tsv, pbt4-copy-map.tsv, pbt4-retire-compile.tsv, retire_compile_pbt4-out.txt,
copy_map_pbt4-out.txt, measure_pbt4_v41-out.txt y probe_overlap-out.txt (todos en esta carpeta), y el contrato de PB-T3c por
`git show <rev>:...` (objetos git). Escribe solo <salida-v41.md>.
"""
import json
import re
import subprocess
import sys
from collections import Counter

repo, rev, v4_path, out_path = sys.argv[1:5]
here = __file__.replace('make_v41.py', '')


def read(name):
    with open(here + name, encoding='utf-8') as f:
        return f.read()


def replace_once(text, old, new, what):
    n = text.count(old)
    assert n == 1, '%s: %d coincidencias' % (what, n)
    return text.replace(old, new)


def section(text, start, end, new, what):
    a = text.index(start) if start else 0
    b = text.index(end, a + 1) if end else len(text)
    assert text.count(start) == 1 if start else True, what
    return text[:a] + new.rstrip('\n') + '\n\n' + text[b:] if end else text[:a] + new


v4 = read(v4_path) if not v4_path.startswith(('/', 'C:', 'D:')) else open(v4_path, encoding='utf-8').read()

# ------------------------------------------------------------------ los números
rows = [l.split('\t') for l in read('pbt4-substitution-lists.tsv').splitlines()[1:]]
cnt = Counter(r[0] for r in rows)
c7_text = sum(1 for r in rows if r[0] == 'PB-T4b C-7' and 'por compilación' not in r[4])
c7_comp = sum(1 for r in rows if r[0] == 'PB-T4b C-7' and 'por compilación' in r[4])
mech = sum(1 for r in rows if r[0] == 'PB-T4b C-0' and 'copia mecánica' in r[4])
hand = sum(1 for r in rows if r[0] == 'PB-T4b C-0' and 'a mano' in r[4])
N = {
    'N_A': cnt['PB-T4a'], 'N_C0': cnt['PB-T4b C-0'], 'N_C5': cnt['PB-T4b C-5'], 'N_UI': cnt['PB-T4b UI'],
    'N_C7': cnt['PB-T4b C-7'], 'N_C7TEXT': c7_text, 'N_C7COMP': c7_comp, 'N_C': cnt['PB-T4c'], 'N_MECH': mech, 'N_HAND': hand,
}
N['N_SUB'] = N['N_C0'] + N['N_C5'] + N['N_UI']
N['N_B'] = N['N_SUB'] + N['N_C7']
retire = [l.split('\t') for l in read('pbt4-retire-compile.tsv').splitlines()[1:]]
nine_compile = [r for r in retire if r[3] == 'las 9 clases']
N['N_NINE_COMPILE'] = len(nine_compile)
N['N_SUPPORT'] = int(re.search(r'miembros de apoyo afectados \(no pruebas\): (\d+)', read('retire_compile_pbt4-out.txt')).group(1))
overlap = read('probe_overlap-out.txt')
N['N_OWN99'] = int(re.search(r'comparable a las 99 de Q1 \(C-7 propias \+ solape con C-5\): (\d+)', overlap).group(1))


def pct(x, ref):
    v = (x - ref) * 100.0 / ref
    return ('%.1f' % v).replace('.', ',')


N['PCT_SUB'] = pct(N['N_SUB'], 465)
N['PCT_C7'] = pct(N['N_C7'], 100)
N['PCT_B'] = pct(N['N_B'], 565)
mix = re.search(r"a mano por motivo \[(.*)\]", read('copy_map_pbt4-out.txt')).group(1)
N['HAND_MIX'] = '; '.join('%s %s' % (a, b) for a, b in re.findall(r"\('([^']*)', (\d+)\)", mix))
# los métodos con alguna firma en las 9 clases (la cifra «242» de la v4) y la opción 1, no elegida
c0_only = sum(1 for r in rows if r[0] == 'PB-T4b C-0' and r[4].split(';')[0] == 'C-0')
N['N_SIGNED'] = N['N_C0'] - c0_only
N['N_OPT1'] = N['N_SIGNED'] + N['N_C5'] + N['N_UI']
by_class = Counter(r[0].rsplit('/', 1)[-1].replace('.cs', '') for r in nine_compile)
N['NINE_BY_CLASS'] = '; '.join('`%s` %d' % (k, v) for k, v in sorted(by_class.items()))

N['COTA_TABLE'] = '\n'.join([
    '| Subtarea | Lista | Lo que vio el Owner | Exacto, v4.1 | Diferencia |',
    '|---|---|---|---|---|',
    '| PB-T4a | CT-PBT3A-12 (C-5) | 1 | **%(N_A)d** | 0 |',
    '| PB-T4b | Las 9 clases (C-0), «Saltar y duplicar» | 428 | **%(N_C0)d**: %(N_MECH)d copias mecánicas y %(N_HAND)d a mano | 0 |',
    '| PB-T4b | La letra de las guardas (C-5 y C-6) | «unas 12» | **%(N_C5)d** | +%(D_C5)d |',
    '| PB-T4b | La UI de la elección (C-3) | «hasta 21», recontada a unas 25 (21 + al menos 3) | **%(N_UI)d** | +%(D_UI)d sobre 25 |',
    '| PB-T4b | **Subtotal sin la retirada** | **unas 465** | **%(N_SUB)d** | +%(D_SUB)d (+%(PCT_SUB)s %%) |',
    '| PB-T4b | Lo que añade «Retirarlo», por texto (C-7), con E-3, E-5 y E-6 | — | %(N_C7TEXT)d | |',
    '| PB-T4b | Lo que añade «Retirarlo», por compilación (C-7): dejan de compilar al borrar lo retirado | — | %(N_C7COMP)d | |',
    '| PB-T4b | **Lo que añade «Retirarlo»,** sin las ya contadas | **unas 100** (99 + 3 menos las ya contadas) | **%(N_C7)d** | +%(D_C7)d (+%(PCT_C7)s %%) |',
    '| PB-T4b | **Total** | **unas 565** | **%(N_B)d** | +%(D_B)d (+%(PCT_B)s %%) |',
    '| PB-T4c | PEG:155-163 y CT-PBT1B-15 (C-5) | 2 | **%(N_C)d** | 0 |',
    '',
    '**Las otras cifras del texto preguntado,** recontadas con el mismo léxico:',
    '- Q1, «unas 99 pruebas propias (35 guardas de texto y 64 de comportamiento)»: **%(N_OWN99)d** (las propias por texto, las que ya estaban en la letra de las guardas y las que dejan de compilar);',
    '- Q3, «Unas 242 fallarían de verdad […]; las otras 186»: **%(N_SIGNED)d** y **%(C0_ONLY)d**;',
    '- «unas 280» con «Editar una línea» (no elegida): **%(N_OPT1)d**.',
    '',
    '**Por qué cambian frente a la v4** (R-T4-25): el léxico de la v4 cortaba mal los métodos con llaves dentro de cadenas, contaba pruebas que ya llevan `Skip` (las de D-1) y daba la letra de las guardas como «unas 12» sin enumerarla; y la retirada solo se medía por texto.',
    '',
    '**Los tres puntos que vuelven al Owner antes de emitir PB-T4b** (pregunta 4: «un efecto material sin medir» y «la lista que se emite es esa o una parte de ella»):',
    '1. **La lista supera la cota:** %(N_B)d frente a unas 565 (+%(PCT_B)s %%), por la retirada: %(N_C7)d frente a unas 100. Lo que más pesa son %(N_C7COMP)d pruebas que dejan de compilar al borrar lo retirado, entre ellas 48 de las 49 de `WorkspacePilotUpdateRunTests` (que usan `PilotUpdateSetup.Run`, que llama a `PushBackPilotUpdateRun.Execute`), las 9 de `PushBackFullWritePrepareTests` y 1 de `PushBackFullWriteRunTests` (PB-T3b compara con el piloto como oráculo), y las de `WorkspaceDraftPilotOwnershipTests` y `WorkspaceDraftPilotStateTests` que usan el comparador.',
    '2. **%(N_NINE_COMPILE)d de las 428 no se pueden saltar** (%(NINE_BY_CLASS)s): alcanzan lo retirado por un ayudante o por el kit (por ejemplo, la constante `Key = PushBackPilotFieldAdapter.AnnotationScaleKey` de `WorkspacePilotEditHostTests` o `PrepareKit.Prepare`), así que dejan de compilar con el `Skip` puesto. Habría que borrarlas con registro (su copia o su equivalente las sustituye), y con ellas %(N_SUPPORT)d miembros de apoyo que nombran lo retirado (`retire_compile_pbt4-out.txt`). Q3 decía «No hace falta ninguna excepción a la regla de no editar pruebas ni ayudantes». **Una salida que no es del paquete decidir:** retirar el código del piloto en una subtarea propia, después de PB-T4c, cuando las copias ya estén en verde; las sustituciones serían las mismas, pero el `Skip` de las 428 no chocaría con la retirada en la misma subtarea.',
    '3. **Las copias en el RED** (§5.2): su resultado es mixto sobre el esqueleto, y un filtro solo admite todo `Failed` o todo `Passed`.',
    '',
    '**PB-T4a no depende de nada de esto:** no retira nada, su lista es 1 y su diseño no toca las 9 clases.',
]) % dict(N, D_C5=N['N_C5'] - 12, D_UI=N['N_UI'] - 25, D_SUB=N['N_SUB'] - 465, D_C7=N['N_C7'] - 100, D_B=N['N_B'] - 565, C0_ONLY=c0_only)

# ------------------------------------------------------------------ los literales del contrato de PB-T3c
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
inv = c['Invariants']
common = []
for text in inv:
    m = re.match(r'INV-PBT3C-(\d+)\. ', text)
    num = int(m.group(1))
    if num == 7:
        common.append('INV-PBT4A-07. Las redes: section 5 en verde. Ninguna prueba existente se edita, se salta, se reactiva ni se borra, con una unica salvedad: el Skip de CT-PBT3A-12, con su motivo literal. En CurrentSha, el git diff -U0 BaseSha..CurrentSha de PushBackDraftWriteGuardTests.cs solo lleva la linea de atributo con Skip.')
    elif num == 13:
        common.append('INV-PBT4A-13. Las declaraciones nuevas en src/: las de la lista cerrada de PBT4A-C13, y ninguna mas, por nombre y acotada a los archivos que tocan, sobrecargas incluidas. No se admite ningun ayudante privado, tampoco en los dos archivos nuevos: el texto literal del paquete no lo necesita.')
    elif 8 <= num <= 14:
        common.append(text.replace('INV-PBT3C-', 'INV-PBT4A-'))
assert [int(re.match(r'INV-PBT4A-(\d+)', t).group(1)) for t in common] == list(range(7, 15))
N['INV_COMMON'] = '\n'.join(common)
rt = []
rt.append(('tests/RackCad.Tests/RackCad.Tests.csproj', 'FullyQualifiedName~RackCad.Tests.PushBackFullReadPathTests', 5, True))
rt.append(('tests/RackCad.Tests/RackCad.Tests.csproj', 'FullyQualifiedName~RackCad.Tests.PushBackFullReadGuardTests', 3, False))
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
g = read('measure_pbt4_v41-out.txt').splitlines()
start = next(i for i, l in enumerate(g) if l.startswith('| Guarda'))
N['GUARDS41'] = '\n'.join(l for l in g[start:] if l.startswith('|')) + '\n\n' + [l for l in g if l.startswith('rojas=')][0].replace(
    'rojas=', '**En rojo:** ').replace(' de ', ' de ').replace('sobre el GREEN; contrafactuales en rojo=', 'sobre el GREEN; contrafactuales en rojo, como se espera: ') + '.'

# ------------------------------------------------------------------ las secciones
out = v4
out = section(out, None, '## Registro de cambios v3 → v4', read('v41/f-head.md'), 'cabecera')
out = section(out, '## 0. En una página', '## 1. Autoridades', read('v41/f-s0.md'), '§0')
out = replace_once(out, '## 2. Hechos medidos en `7dfc7c21`\n\nLos de la v1 (R-1 a R-12), corregidos, y los nuevos (R-13 a R-20). Lo esencial:',
                   '## 2. Hechos medidos en `7f0d9aa3`\n\nMedidos en `7dfc7c21` (v1 a v3) y vueltos a medir en `7f0d9aa3`, el cierre de PB-T3c (v4 y v4.1): entre las dos, `src/` y `tests/` solo cambian en el escritor, la fachada y dos guardas de PB-T3c (R-28), así que valen en `7f0d9aa3`. Los de la v1 (R-1 a R-12), corregidos, y los nuevos (R-13 a R-38; los de la v4.1, R-34 a R-38). Lo esencial:', '§2')
out = section(out, '#### D-PBT4-02.', '### PB-T4b', read('v41/f-d02-03.md'), 'D-02/03')
out = section(out, '#### D-PBT4-09.', '#### D-PBT4-11.', read('v41/f-d09-10.md'), 'D-09/10')
out = section(out, '#### D-PBT4-11.', '### PB-T4c', read('v41/f-d11.md'), 'D-11')
out = section(out, '## 4. Las guardas, por subtarea', '## 5. La red de pruebas', read('v41/f-s4.md'), '§4')
out = section(out, '## 5. La red de pruebas', '## 6. Los contratos propuestos', read('v41/f-s5.md'), '§5')
out = section(out, '## 6. Los contratos propuestos', '## 7. Riesgos y lecciones aplicadas', read('v41/f-s6.md'), '§6')
out = section(out, '## 7. Riesgos y lecciones aplicadas', '## 8. Las decisiones del Owner', read('v41/f-s7.md'), '§7')
out = section(out, '## 8. Las decisiones del Owner', '## 9. Límites', read('v41/f-s8.md') + '\n---\n', '§8')
out = section(out, '## 9. Límites', None, read('v41/f-s9-11.md'), '§9-11')

FIXES = [
    ('**Los TaskId** empiezan por `F4B-PBT4-`, de 24 o 25 caracteres, para los sustitutos de nc1 y nc2 (§8.3).',
     '**Los TaskId** empiezan por `F4B-PBT4-` y tienen como mucho 26 caracteres (23, 25 y 24), para los sustitutos de nc1 y nc2 (§6.0; V41-11).'),
    ('**Lo que pasa con el código del piloto** depende de Q1 (D-PBT4-10).', '**El código del piloto se retira** (Q1 del Owner; D-PBT4-10).'),
    ('**La copia del piloto,** según Q1.',
     '**La copia del piloto** de la regla (`PushBackPilotFieldAdapter`) se retira con él (Q1 del Owner; D-PBT4-10): queda una sola regla.\n\n'
     '**El esqueleto del RED de PB-T4b no toca `NumericFieldValidation.cs`** (M-29): crea `NumericTextRule.cs` y `PushBackAnnotationScaleRule.cs` inertes, y la delegación de `NumericFieldValidation.Validate` en `NumericTextRule.Check` es del GREEN.'),
    (', PushBackPresentationForm form)', ')'),
    ('| A-T3-1, A-5 y R-T1-17 | PB-T4b, con Q1 | D-PBT4-10 |', '| A-T3-1, A-5 y R-T1-17 | PB-T4b: se retira (Q1 del Owner) | D-PBT4-10 |'),
    ('«Cambiado por ti: el nombre» (o el campo)', '«Cambiado por ti: el nombre.»'),
    ('| A-T3-10 | Con «Retirar», las constantes se quedan en `PushBackPilotMaterializer` (el lector las usa); con «Conservar», no aplica | D-PBT4-10 |',
     '| A-T3-10 | Las constantes se quedan en `PushBackPilotMaterializer` (el lector las usa) | D-PBT4-10 |'),
    ('| **F4B-E28 Q3; F4B-E38; F4B-E29 (D-1); F4B-E11 (D2) con L-5; F4B-E24; la nota del Owner sobre la interfaz; F4B-E03; E15, E21, E25, E35 Q4, E37, E42, E45 y E46; D-PB-10** | |',
     '| **F4B-E28 Q3; F4B-E38; F4B-E29 (D-1); F4B-E11 (D2) con L-5; F4B-E24; la nota del Owner sobre la interfaz; F4B-E03; E15, E21, E25, E35 Q4, E37, E42, E45, E46 y E48; D-PB-10** | |\n'
     '| **Las decisiones del Owner sobre PB-T4** (`owner-pbt4-decisions.md`, 2026-11-20) | Q1 «Retirarlo», Q2 «Por grupos», Q3 «Saltar y duplicar» y Q4 «Sí, con condiciones» (§8.2). Desde la v4.1, Q1 a Q4 siguen la numeración de ese registro (la v4 llamaba Q3 a la emisión y Q4 al procedimiento) |'),
    ('- **13. Las declaraciones nuevas en `src/`:** las de la lista cerrada de la parada C13 de cada contrato, y ninguna más, por nombre y acotada a los archivos que tocan, sobrecargas incluidas. En los archivos nuevos se admiten además métodos privados, tipos anidados privados sin estado y campos `private static readonly` de un tipo inmutable (`Regex` o `string`).',
     '- **13. Las declaraciones nuevas en `src/`:** las de la lista cerrada de la parada C13 de cada contrato, y ninguna más, por nombre y acotada a los archivos que tocan, sobrecargas incluidas. Cada contrato dice si admite ayudantes privados en sus archivos nuevos: **PB-T4a no admite ninguno.**'),
]
for old, new in FIXES:
    out = replace_once(out, old, new, old[:40])
for k, v in N.items():
    out = out.replace('{{%s}}' % k, str(v))
left = re.findall(r'\{\{[A-Z0-9_]+\}\}', out)
assert not left, left
assert '<a medir en la comprobación del §6.0>' not in out and '<tras el cierre de PB-T3c>' not in out
with open(out_path, 'w', encoding='utf-8', newline='\n') as f:
    f.write(out)
stale = [w for w in ['opción 1 de Q4', 'opción 2 de Q4', 'Por campos', 'Conservar sin uso', 'Conservarlo', 'Editar una línea', 'Dos modelos', 'PushBackFieldLabels', 'PushBackPresentationForm', 'hasta 21', 'unas 461', 'unas 275']
         if w in out]
print('bytes', len(out.encode('utf-8')), 'numeros', {k: N[k] for k in ['N_A', 'N_C0', 'N_MECH', 'N_HAND', 'N_C5', 'N_UI', 'N_SUB', 'N_C7TEXT', 'N_C7COMP', 'N_C7', 'N_B', 'N_C', 'N_NINE_COMPILE', 'N_SUPPORT', 'N_OWN99', 'N_SIGNED', 'N_OPT1']})
print('palabras de ramas no elegidas que quedan (revisar a mano):', stale)
