# Fills README.md of r4/r1probes from the result files (tables generated, never retyped). python -I make_readme_r4.py <src README> <out README>
import hashlib, json, os, sys
NL = chr(10)
HERE = os.path.dirname(os.path.abspath(__file__))


def J(n):
    return json.load(open(os.path.join(HERE, n), encoding='ascii'))


def esc(s):
    return str(s).replace('|', '\\|').replace(NL, ' ')


def short(x):
    return x[:8] if isinstance(x, str) and len(x) == 40 else x


def om(o):
    return '; '.join('%s %s' % (t.replace('C1.', '').replace('(D.BaseSha..H.CurrentSha)', '').replace('(D.MainSha,K.AuthorityRevision)', ''), c)
                     for t, c in o) or '—'


def deriv_obs(o):
    c = o['Counts']
    s = '%d commits, %d RevPaths, %d rutas, %d archivos; omitidos: %s' % (c['Commits'], c['RevPaths'], c['Paths'], c['Files'], om(o['Omitted']))
    if o.get('InputWhy'):
        s += '; causa de la entrada: ' + o['InputWhy']
    return s


def acc_obs(o):
    ch = o.get('Checks') or {}
    mm = sorted(k for k, v in ch.items() if v == 'MISMATCH')
    s = '%s rc %s; R4 %s; R9 %s' % (o.get('Decision'), o.get('Rc'), ch.get('R4'), ch.get('R9'))
    if mm:
        s += '; MISMATCH: ' + ', '.join(mm)
    if o.get('R9Runs'):
        s += '; fuera del cierre: ' + ', '.join(k.split('-', 1)[1] for k in sorted(o['R9Runs']))
    t = o['Table']
    s += '; tabla de la ronda 4: ' + ('aceptada' if t['Accepted'] else 'no aceptada')
    cv = t.get('ClosureViolations') or []
    if cv:
        s += ' (cotejo: ' + '; '.join(sorted(set(v.split(':')[0] for v in cv))) + ')'
    if not o.get('ZeroWrite', True) or not o.get('ProcessTreeDead', True):
        s += '; ESCRITURAS O PROCESOS'
    return s


def table(cases, obsf):
    rows = ['| Caso | Qué | Esperado | Observado | ¿Se cumple? |', '|---|---|---|---|---|']
    for c in cases:
        rows.append('| %s | %s | %s | %s | %s |' % (c['Case'], esc(c['What']), esc(c['Expected']), esc(obsf(c)), 'sí' if c['Holds'] else '**no**'))
    return NL.join(rows)


p7, p8, p9, p10, p11, sm = J('p7-derive.json'), J('p8-v.json'), J('p9-n4.json'), J('p10-nc1.json'), J('p11-indep.json'), J('summary.json')
der = [c for c in p7['Cases'] if c['Group'] == 'derive']
neg = [c for c in p7['Cases'] if c['Group'] == 'neg']
fills = {}
fills['@@DERIVE_TABLE@@'] = table(der, lambda c: deriv_obs(c['Observed']) if 'Counts' in c['Observed'] else json.dumps(c['Observed'], ensure_ascii=False)[:300])


def neg_obs(c):
    o = c['Observed']
    k = o.get('Kinds') or []
    d = o.get('Diff') or {}
    parts = []
    for f in sorted(d):
        e, m = len(d[f].get('Extra', [])), len(d[f].get('Missing', []))
        parts.append('%s +%d/-%d' % (f, e, m))
    return ('rechazo: ' + ', '.join(k) if k else 'sin diferencias') + ('; diferencias del cierre: ' + ', '.join(parts) if parts else '; cierre igual al normativo')


fills['@@NEG_TABLE@@'] = table(neg, neg_obs)
fills['@@V_TABLE@@'] = table(p8['Cases'], lambda c: acc_obs(c['Observed']))
fills['@@N4_TABLE@@'] = table(p9['Cases'], lambda c: acc_obs(c['Observed']))
fills['@@NC1_TABLE@@'] = table(p10['Cases'], lambda c: acc_obs(c['Observed']))
ic = p11['Cases'][0]
rows = ic['Rows']
fills['@@INDEP_TEXT@@'] = ('Resultado: %d de %d vectores con el mismo cierre y los mismos términos omitidos con la misma causa (%d del banco, %d del agente: su muestra '
                           'y sus vectores sintéticos sobre un repositorio propio, con dos raíces sin relación, una fusión en la cadena de primer padre y '
                           'objetos que no son commits; en uno, un TRX que no verifica, las dos implementaciones fallan como error del banco). '
                           '`derive_indep.py` SHA-256 `%s`.') % (ic['Observed']['Agree'], ic['Observed']['Vectors'],
                                                                sum(1 for r in rows if not r['Vector'].startswith('D:/r62-a6r4/indep/')),
                                                                sum(1 for r in rows if r['Vector'].startswith('D:/r62-a6r4/indep/')), ic['IndepSha256'])
groups = {}
for c in sm['Cases']:
    groups[c['Group']] = groups.get(c['Group'], 0) + 1
lines = ['**%d de %d casos se cumplen** (%s).' % (sm['Holding'], sm['Total'], ', '.join('%s %d' % (g, n) for g, n in sorted(groups.items()))), '',
         'Versiones: ' + esc('Git %s; Python %s; pwsh %s; %s' % (sm['Versions']['Git'], sm['Versions']['Python'], sm['Versions']['Pwsh'], sm['Versions']['Os'])), '',
         '| SHA-256 | Archivo |', '|---|---|']
for n in sorted(sm['Files']):
    lines.append('| `%s` | `%s` |' % (sm['Files'][n], n))
lines.append('')
lines.append('El SHA-256 de `summary.json` y el de este README se dan fuera de ellos.')
fills['@@SUMMARY@@'] = NL.join(lines)
t = open(sys.argv[1], encoding='utf-8').read()
for k, v in fills.items():
    assert t.count(k) == 1, k
    t = t.replace(k, v)
assert '@@' not in t
open(sys.argv[2], 'w', encoding='utf-8', newline=NL).write(t)
print('ok', hashlib.sha256(t.encode('utf-8')).hexdigest())
