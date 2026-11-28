"""PB-T4b, precondición 6 de la v4.9: ¿el paso 7 de la v4.8 (CT-06 en /d/r4s y el coste en /d/r4s y /d/r4t) sigue valiendo? Compara por sha256 sus
entradas en la v4.8 y en la v4.9 (solo lectura):
1. el GREEN de src/: los archivos que escribe apply_green_pbt4b_v45.py (su salida en /d/r4s y en la copia de la v4.9);
2. el RED que va debajo: los archivos que escribe apply_red (el esqueleto de src/, los Skip, el kit y las 21 copias), en /d/r4s (v4.8) y en la v4.9;
   tiene que cambiar solo la copia de WHHG;
3. las tres sondas: su sha256 hoy y el que imprimió la corrida de /d/r4s (y la de /d/r4t, la del coste);
4. lo que usan las sondas: los tipos que nombran, frente a los tipos que declaran las copias (las 21) y el kit. Ninguna sonda puede nombrar un tipo de
   la copia que cambia.

Uso: python -I step7_inputs_v49.py <apply-red-v48.txt> <apply-green-v48.txt> <apply-red-v49.txt> <apply-green-v49.txt> <corrida-r4s.txt>
     <corrida-r4t.txt> <raiz-de-una-copia-v49> <salida.txt> <sonda.cs>...   (de la raiz, solo los <Clase>Full.cs y el kit)
"""
import hashlib
import os
import re
import sys

sys.stdout.reconfigure(encoding='utf-8', errors='replace')
red48, green48, red49, green49, run_s, run_t, copies_dir, out = sys.argv[1:9]
probes = sys.argv[9:]
LINE = re.compile(r'^([0-9a-f]{64})\s+\d+\s+(\S+)$')


def files(path):
    d = {}
    for l in open(path, encoding='utf-8', errors='replace').read().split('\n'):
        m = LINE.match(l.strip())
        if m:
            d[m.group(2)] = m.group(1)
    return d


lines, ok = [], True
g48, g49 = files(green48), files(green49)
same_green = g48 == g49 and len(g48) == 10
lines.append('1. el GREEN de src/: %d archivos en la v4.8 y %d en la v4.9; iguales por sha256: %s' % (len(g48), len(g49), same_green))
ok &= same_green
r48, r49 = files(red48), files(red49)
changed = sorted(k for k in set(r48) | set(r49) if r48.get(k) != r49.get(k))
kit = 'tests/RackCad.Tests/Workspace/WorkspaceFullReadKit.cs'
want = ['tests/RackCad.Tests/Workspace/WorkspaceHostHardeningGuardFull.cs']
lines.append('2. el RED debajo: %d archivos en la v4.8 y %d en la v4.9; cambian: %s; el kit igual: %s (%s)' % (
    len(r48), len(r49), changed, r48.get(kit) == r49.get(kit), (r49.get(kit) or '')[:12]))
ok &= changed == want and r48.get(kit) == r49.get(kit)
run_s_text = open(run_s, encoding='utf-8', errors='replace').read()
run_t_text = open(run_t, encoding='utf-8', errors='replace').read()
for p in probes:
    h = hashlib.sha256(open(p, 'rb').read()).hexdigest()
    name = os.path.basename(p)
    tmp = {'pbt4b-read-cost-probe.cs': 'TmpReadCostProbe.cs', 'pbt4b-ct06-presentar-probe.cs': 'TmpCt06PresentarProbe.cs'}.get(name, name)
    in_s = ('sonda %s %s' % (h, tmp)) in run_s_text
    in_t = ('sonda %s %s' % (h, tmp)) in run_t_text
    lines.append('3. la sonda %s: %s; la misma que en /d/r4s: %s%s' % (name, h, in_s, '; y que en /d/r4t: %s' % in_t if tmp == 'TmpReadCostProbe.cs' else ''))
    ok &= in_s and (in_t or tmp != 'TmpReadCostProbe.cs')
# los tipos de primer nivel (con 0 o 4 espacios: espacio de nombres de archivo o de bloque), sobre el texto sin comentarios
decl = re.compile(r'^(?: {4})?(?:(?:public|internal|private|protected|static|sealed|abstract|partial|readonly|file)\s+)*(?:class|struct|record|interface|enum)\s+(\w+)', re.M)


def no_comments(text):
    return re.sub(r'//[^\n]*', '', re.sub(r'/\*.*?\*/', '', text, flags=re.S))
declared = {}
for dp, _, fs in os.walk(copies_dir):
    for f in fs:
        rel = os.path.relpath(os.path.join(dp, f), copies_dir).replace(os.sep, '/')
        if (f.endswith('Full.cs') or rel == kit) and '/bin/' not in rel and '/obj/' not in rel:
            for n in decl.findall(no_comments(open(os.path.join(dp, f), encoding='utf-8').read())):
                declared.setdefault(n, set()).add(rel)
for p in probes:
    text = open(p, encoding='utf-8').read()
    used = sorted(n for n in declared if re.search(r'\b' + re.escape(n) + r'\b', text))
    from_copies = sorted(n for n in used if any(f != kit for f in declared[n]))
    lines.append('4. la sonda %s nombra tipos de las copias: %s; del kit: %s' % (os.path.basename(p), from_copies or 'ninguno', sorted(n for n in used if kit in declared[n])))
    ok &= not from_copies
lines.append('EL PASO 7 DE LA v4.8 SIGUE VALIENDO: %s' % ('SI' if ok else 'NO'))
with open(out, 'w', encoding='utf-8', newline='\n') as f:
    f.write('\n'.join(lines) + '\n')
print('\n'.join(lines))
if not ok:
    raise SystemExit(1)
