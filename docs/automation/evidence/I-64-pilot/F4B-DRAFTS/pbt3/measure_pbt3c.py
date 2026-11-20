# -*- coding: utf-8 -*-
"""Solo lectura: la remedicion de PB-T3c sobre el cierre de PB-T3b (precondicion 2 y section 6.0 del paquete de PB-T3 v4.7). Es el seco del generador
(make_pbt3c_contract.py --dry --objects-rev <cierre> --repo <checkout> --package-file <v4.7>) mas lo que el generador no mide y la remedicion pide:
- de la salida del seco: el anclaje (con la CI del cierre y count-rows-out.txt), las anclas de WR, RCS, UP y de las guardas del escritor, los recuentos
  y alcances, los MinSelected (86 y 123 de la TRX; los heredados), la tabla de RR-3 (con la fila de CT-05), lo previsto de la CI en su unica rama y los
  AVISOS;
- los barridos sobre el cierre (section 4.1, V47-01 y V47-05): las guardas (sweep-pbt3c-close.txt), las sobrecargas y la reflexion
  (overload-sweep-pbt3c-fe1c67f4.txt) y los nombres (name-check-pbt3c-fe1c67f4.txt), por su sha256 y su resultado;
- el estado de WR en el cierre: su sha256 en el almacen igual al de la emision de PB-T3b (PB-T3b no lo toca) y lo que INV-PBT3C-03 dice de el.
Uso: python -I measure_pbt3c.py <checkout principal> <cierre de PB-T3b>   -> escribe pbt3c-anchors-out.txt (el contrato en seco va a un temporal que se
borra). El checkout va por argumento: el guion no lleva rutas locales."""
import hashlib, os, re, subprocess, sys

REPO, CLOSE = sys.argv[1], sys.argv[2]
HERE = os.path.dirname(os.path.abspath(__file__))
GEN, PKG = 'make_pbt3c_contract.py', 'I-64-f4b-pbt3-gate-contract-package-v47.md'
TMP, OUT = '_measure_pbt3c_contract.json', 'pbt3c-anchors-out.txt'
SWEEPS = ['sweep-pbt3c-close.txt', 'overload-sweep-pbt3c-fe1c67f4.txt', 'name-check-pbt3c-fe1c67f4.txt']
WR = 'src/RackCad.Plugin/Views/RackPilotUpdateWriter.cs'
assert re.fullmatch(r'[0-9a-f]{40}', CLOSE), 'el cierre de PB-T3b, sha de 40'


def sha(path):
    return hashlib.sha256(open(os.path.join(HERE, path), 'rb').read()).hexdigest()


def git(*a):
    r = subprocess.run(['git', '-C', REPO] + list(a), capture_output=True)
    assert r.returncode == 0, a
    return r.stdout


p = subprocess.run([sys.executable, '-I', os.path.join(HERE, GEN), '2026-11-20T00:00:00Z', CLOSE, '812baf879612320653094a8aaf46aca0bac81383', '--dry',
                    '--objects-rev', CLOSE, '--repo', REPO, '--package-file', os.path.join(HERE, PKG), '--json-out', os.path.join(HERE, TMP)],
                   capture_output=True, cwd=HERE)
dry = p.stdout.decode('utf-8', 'replace').replace('\r\n', '\n')
assert p.returncode == 0, p.stderr.decode('utf-8', 'replace')[-600:]
csha = sha(TMP)
os.remove(os.path.join(HERE, TMP))
L = dry.split('\n')


def line(prefix):
    m = [l for l in L if l.startswith(prefix)]
    assert len(m) == 1, prefix
    return m[0]


R = ['# measure_pbt3c.py: la remedicion de PB-T3c sobre el cierre de PB-T3b (%s), por objetos git; generador %s (sha256 %s), paquete %s (sha256 %s)'
     % (CLOSE[:8], GEN, sha(GEN)[:16], PKG, sha(PKG)[:16]), '']
R.append('== el seco (el contrato en seco, sha256 %s)' % csha.upper())
for pre in ('DRY rev ', 'anclaje: ', 'contrato de PB-T3b: ', 'PBT3C-C11: ', 'tabla de RR-3 ', 'la CI prevista ', 'D1: ', 'emision ('):
    R.append('  ' + line(pre))
R.append('  ' + [l for l in L if l.startswith('paquete leido: ')][0][:200] + ' ...')
i = L.index('RequiredTests (criterio de count_tests_git.py sobre los objetos de la revision):')
j = [k for k, l in enumerate(L) if l.startswith('AVISOS (')][0]
R.append('  los MinSelected:')
R += ['  ' + l for l in L[i + 1:j]]
R.append('  ' + L[j])
R += ['  ' + l for l in L[j + 1:] if l.startswith('  - ')]
R.append('')
R.append('== los barridos sobre el cierre (section 4.1; V47-01 y V47-05)')
for s in SWEEPS:
    t = open(os.path.join(HERE, s), encoding='utf-8').read()
    head = [l for l in t.split('\n') if l.startswith(('antes:', 'despues:', 'filas del corpus nuevas', 'guardas con coincidencias en el corpus', '   control',
                                                      '   reflection', '   v4.7', '   new names', '== '))]
    R.append('  %s (sha256 %s):' % (s, sha(s)[:16]))
    R += ['    ' + l.strip() for l in head[:14]]
R.append('')
R.append('== WR en el cierre (D-PBT3-11; INV-PBT3C-03)')
wr_close = git('show', CLOSE + ':' + WR)
wr_emit = git('show', 'eab27a9e:' + WR)
code = re.sub(r'//[^\n]*', '', wr_close.decode('utf-8'))
R.append('  sha256 %s; igual al de la emision de PB-T3b (eab27a9e): %s' % (hashlib.sha256(wr_close).hexdigest()[:16], wr_close == wr_emit))
for name, rx in (('clases RackPilot*', r'\bclass\s+RackPilot\w+'), ('StartTransaction', r'StartTransaction\s*\('), ('.Commit(', r'\.Commit\s*\('),
                 ('PurgeAfterCommit', r'PurgeAfterCommit\s*\('), ('[Conditional("DEBUG")]', r'\[Conditional\("DEBUG"\)\]'),
                 ('GetBlockReferenceIds', r'GetBlockReferenceIds'), ('Erase(', r'\bErase\s*\('), ('EraseViewBlocks', r'EraseViewBlocks'),
                 ('ForWrite', r'\bForWrite\b'), ('UpgradeOpen', r'UpgradeOpen'), ('.Name =', r'\.Name\s*=(?!=)')):
    R.append('  %-26s %d' % (name, len(re.findall(rx, code))))
open(os.path.join(HERE, OUT), 'w', encoding='utf-8', newline='\n').write('\n'.join(R) + '\n')
print('\n'.join(R[:4]))
