"""PB-T4d (paquete v5.2): el informe de la medida de necesidad. Cruza las 235 bajas de la variante B con lo que la compilacion de cero pidio
quitar (<salida>/bucle/necesidad.tsv de p6d_need_v52.sh) y con lo que falla al ejecutar (los -res.txt de la suite de Core y de los filtros de UI).
Para cada baja de B: `compilacion` (hay que quitarla para compilar), `ejecucion` (compila y falla al ejecutar con la retirada), `pasa` (compila
y pasa, o esta saltada) o `sin-ejecutar` (compila y ningun filtro la corre). Y, de las que no hace falta quitar para compilar, si nombra el
modelo completo o lo que se queda (`mixta`) o solo lo retirado (`piloto`), por texto de su metodo (con cadenas: las guardas leen texto).
Solo lee; escribe UTF-8 con LF.

Uso: python -I need_report_v52.py <repo> <dir-salida-nd> <salida.txt> <salida.tsv>
"""
import collections
import glob
import os
import re
import subprocess
import sys

sys.dont_write_bytecode = True
import importlib.util
_spec = importlib.util.spec_from_file_location('lists_core', os.path.join(os.path.dirname(os.path.dirname(os.path.abspath(__file__))), 'lists_core.py'))
lc = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(lc)
REPO, ND, OUT, OUT_TSV = sys.argv[1:5]
REV = '6c2820bab4d8b41e3e229e23f3a1638ccbb04735'
KEEP = re.compile(r'PushBackFull\w*|PrepareFull|ExecuteFull|ForFullWrite|BuildFull|BuildCore|Execution<|PushBackAnnotationScaleRule|WorkspaceHostCore|'
                  r'PushBackPilotIntentReader|PushBackPilotMaterializer\.Materialize|OutcomeOfAnEscape')


def show(path):
    return subprocess.run(['git', '-C', REPO, 'show', REV + ':' + path], capture_output=True, check=True).stdout.decode('utf-8-sig').replace('\r\n', '\n')


need = [l.split('\t') for l in open(os.path.join(ND, 'bucle', 'necesidad.tsv'), encoding='utf-8').read().rstrip('\n').split('\n')[1:]]
fails = {}
for res in glob.glob(os.path.join(ND, '*-res.txt')):
    for line in open(res, encoding='utf-8').read().split('\n'):
        m = re.match(r'^\s+(Failed|Error|Timeout)\s+(.+?)\s+\|', line)
        if m:
            head = m.group(2).split('(')[0]
            parts = head.split('.')
            fails[(parts[-2], parts[-1])] = (os.path.basename(res), line.strip()[:300])
texts = {}
rows = []
for necesidad, origin, path, member, why in need:
    if necesidad == 'fuera-de-B':
        rows.append(('fuera-de-B', origin, path, member, why, '-'))
        continue
    stem = os.path.basename(path)[:-3]
    failed = [v for (cls, name), v in fails.items() if name == member and (cls == stem or cls.startswith('FullPanelCopy'))]
    if necesidad == 'necesaria':
        state = 'compilacion'
    elif any(v for (cls, name), v in fails.items() if name == member and cls == stem):
        state = 'ejecucion'
    else:
        state = 'pasa-o-saltada'
    kind = '-'
    if state != 'compilacion' and not member.startswith('tipo:'):
        if path not in texts:
            texts[path] = show(path)
        text = texts[path]
        masked = lc.mask(text)
        m = re.search(r'\b' + re.escape(member) + r'\s*(?:<[^>]*>)?\s*\(', masked)
        body = ''
        if m:
            i = masked.find('{', m.end())
            semi = masked.find(';', m.end())
            if i < 0 or (0 <= semi < i and '=>' in masked[m.end():semi]):
                body = text[m.start():semi + 1]
            else:
                d = 0
                for k in range(i, len(masked)):
                    if masked[k] == '{':
                        d += 1
                    elif masked[k] == '}':
                        d -= 1
                        if d == 0:
                            body = text[m.start():k + 1]
                            break
        kind = 'mixta' if KEEP.search(body) else 'piloto'
    detail = next((v[1] for (cls, name), v in fails.items() if name == member and cls == stem), '') or (failed[0][1] if failed else '') or why
    rows.append((state, origin, path, member, detail, kind))

c = collections.Counter()
for state, origin, path, member, why, kind in rows:
    group = origin.split(' (')[0].split(' v')[0] if origin != '-' else '-'
    c[(group, state, kind)] += 1
out = ['# la necesidad de las bajas de B, medida en ' + os.path.basename(ND), '# grupo | estado | tipo | cuantas']
for key, n in sorted(c.items()):
    out.append('%s | %s | %s | %d' % (key + (n,)))
out.append('')
for state in ('ejecucion', 'pasa-o-saltada', 'fuera-de-B'):
    out.append('## ' + state)
    for r in rows:
        if r[0] == state:
            out.append('%s | %s | %s | %s | %s' % (r[1], r[2].split('/')[-1], r[3], r[5], r[4][:200]))
with open(OUT, 'w', encoding='utf-8', newline='\n') as f:
    f.write('\n'.join(out) + '\n')
with open(OUT_TSV, 'w', encoding='utf-8', newline='\n') as f:
    f.write('estado\torigen-en-B\truta\tmiembro\ttipo\tdetalle\n')
    for r in rows:
        f.write('%s\t%s\t%s\t%s\t%s\t%s\n' % (r[0], r[1], r[2], r[3], r[5], r[4].replace('\t', ' ')))
print('\n'.join(out[:2 + len(c)]))
