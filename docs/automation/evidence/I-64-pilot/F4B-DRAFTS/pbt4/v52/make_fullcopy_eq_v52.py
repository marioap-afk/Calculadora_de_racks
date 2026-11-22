"""PB-T4d (paquete v5.2): el equivalente nuevo del equivalente de CT-PBT4B-20 (el de PB-T4c, PushBackEditAllFullCopyGuardTests.cs), como copia
literal del de 6c2820ba (por objetos git) con estos cambios declarados y ninguno mas:
  1. la clase se llama <clase-nueva> (y su comentario de cabecera gana una linea que lo dice);
  2. en `Origins`, el BaseSha y el numero de Skip de cada original que PB-T4d cambia: el sha256 de su texto del RED de PB-T4d (de la copia que
     se da, con los saltos de linea a \n) con cada Skip devuelto a su atributo (la misma `Restore` del equivalente), y su numero de Skip de PB-T4b;
  3. en `CopySha`, el de cada copia que PB-T4d cambia (su texto del RED, con \n);
  4. en `NineCounts`, los recuentos de cada copia de C-0 que cambian (los que da la misma lectura del equivalente sobre los textos del RED).
`Restore`, las lineas esperadas y las autopruebas no cambian. Los valores se calculan aqui con las mismas reglas que el equivalente (para que el
equivalente nuevo pase sobre el RED y el GREEN de PB-T4d, que no tocan esos archivos despues del RED), y se imprimen todos.

Uso: python -I make_fullcopy_eq_v52.py <repo> <copia-con-el-RED> <clase-nueva> <salida.cs> <salida-registro.txt>
"""
import hashlib
import os
import re
import subprocess
import sys

sys.dont_write_bytecode = True
REPO, COPY, CLASS, OUT, LOG = sys.argv[1:6]
REV = '6c2820bab4d8b41e3e229e23f3a1638ccbb04735'
SRC = 'tests/RackCad.Tests/PushBackEditAllFullCopyGuardTests.cs'
OLD_CLASS = 'PushBackEditAllFullCopyGuardTests'
SKIP_B = re.compile(r'^([ \t]*)\[(Fact|Theory)\(Skip = "I-64 F4B PB-T4b: [^"]*"\)\]', re.M)
EDITALL = ('    [Fact(Skip = "I-64 F4B PB-T4c: sustituida por su equivalente en PushBackEditAllPortEquivalenceGuardTests (registro declarado '
           'superseding-characterization.md; decision del Owner owner-pbt4-decisions.md, pregunta 4).")]')


def sha(t):
    return hashlib.sha256(t.encode('utf-8')).hexdigest()


def show(path):
    return subprocess.run(['git', '-C', REPO, 'show', REV + ':' + path], capture_output=True, check=True).stdout.decode('utf-8-sig').replace('\r\n', '\n')


def read(path):
    return open(os.path.join(COPY, path), 'rb').read().decode('utf-8-sig').replace('\r\n', '\n')


def restore(text):
    return SKIP_B.sub(r'\1[\2]', text).replace(EDITALL + '\n', '    [Fact]\n')


text = show(SRC)
log = ['# el equivalente nuevo de CT-PBT4B-20, desde %s:%s, sobre los textos del RED de %s' % (REV[:8], SRC, COPY)]
origins = re.findall(r'\("(tests/[^"]+)",\s*\n\s*"([0-9a-f]{64})", (\d+)\)', text)
assert len(origins) == 21, len(origins)
for path, old_sha, old_n in origins:
    t = read(path)
    new_sha, new_n = sha(restore(t)), len(SKIP_B.findall(t))
    if (new_sha, str(new_n)) != (old_sha, old_n):
        pat = re.compile(r'(\("' + re.escape(path) + r'",\s*\n\s*")' + old_sha + r'(", )' + old_n + r'\)')
        text, k = pat.subn(r'\g<1>' + new_sha + r'\g<2>' + str(new_n) + ')', text)
        assert k == 1, path
        log.append('original %s BaseSha %s -> %s skip %s -> %d' % (path, old_sha, new_sha, old_n, new_n))
copies = re.findall(r'\["(tests/[^"]+Full\.cs)"\] =\s*\n\s*"([0-9a-f]{64})"', text)
assert len(copies) == 21, len(copies)
for path, old_sha in copies:
    new_sha = sha(read(path))
    if new_sha != old_sha:
        text, k = re.subn(r'(\["' + re.escape(path) + r'"\] =\s*\n\s*")' + old_sha + '"', r'\g<1>' + new_sha + '"', text)
        assert k == 1, path
        log.append('copia %s CopySha %s -> %s' % (path, old_sha, new_sha))
kit_path = re.search(r'KitPath = "([^"]+)"', text).group(1)
kit_sha = re.search(r'KitSha = "([0-9a-f]{64})"', text).group(1)
assert sha(read(kit_path)) == kit_sha, 'el kit cambia: no lo cambia PB-T4d'


# (4) los recuentos de C-0, con la lectura de CopyLine del equivalente: los Skip de PB-T4b del original con copia, y cuantos estan en la copia
def methods(code):
    names = []
    for m in re.finditer(r'\bpublic\s+(?:async\s+)?(?:void|Task)\s+(\w+)\s*\(', code):
        names.append(m.group(1))
    return names


def listed(original):
    out = []
    lines = original.split('\n')
    for i, line in enumerate(lines):
        m = re.search(r'Skip = "(I-64 F4B PB-T4b: [^"]*)"', line)
        if not m:
            continue
        for j in range(i, min(i + 40, len(lines))):
            d = re.search(r'\bpublic\s+(?:async\s+)?(?:void|Task)\s+(\w+)\s*\(', lines[j])
            if d:
                out.append((d.group(1), m.group(1)))
                break
    return out


nine = re.findall(r'\["(\w+)"\] = \((\d+), (\d+), (\d+)\),', text)
assert len(nine) == 9, len(nine)
for stem, t0, m0, h0 in nine:
    path = next(p for p, _, _ in origins if p.endswith('/' + stem + 'Tests.cs'))
    original = read(path)
    copy = read(path[:-len('Tests.cs')] + 'Full.cs')
    names = set(methods(copy))
    items = [(n, r) for n, r in listed(original) if 'CT-PBT4B-13' not in r and n in names]
    tests = len(items)
    hand = sum(1 for n, r in items if 'su equivalente escrito a mano en' in r)
    mech = tests - hand
    if (tests, mech, hand) != (int(t0), int(m0), int(h0)):
        text, k = re.subn(r'\["' + stem + r'"\] = \(' + t0 + ', ' + m0 + ', ' + h0 + r'\),', '["%s"] = (%d, %d, %d),' % (stem, tests, mech, hand), text)
        assert k == 1, stem
        log.append('recuentos %s (%s, %s, %s) -> (%d, %d, %d)' % (stem, t0, m0, h0, tests, mech, hand))

# (1) la clase y su cabecera
assert text.count('public sealed class ' + OLD_CLASS) == 1
text = text.replace('public sealed class ' + OLD_CLASS, 'public sealed class ' + CLASS)
head = '    /// I-64 F4B PB-T4b — the script of «Saltar y duplicar» (CT-PBT4B-20; D-PBT4-11), in execution.'
assert text.count(head) == 1, 'la cabecera no casa'
text = text.replace(head, '    /// I-64 F4B PB-T4d: the equivalent of the equivalent of PB-T4c (' + OLD_CLASS + '), with the BaseSha, the Skip, the CopySha and the\n'
                    '    /// counts of the originals and the copies that the retirement of the pilot edits; nothing else changes.\n' + head)
assert OLD_CLASS not in text.replace('the equivalent of PB-T4c (' + OLD_CLASS + ')', '')
with open(OUT, 'w', encoding='utf-8', newline='\n') as f:
    f.write(text)
log.append('equivalente %s %s' % (sha(text), os.path.basename(OUT)))
with open(LOG, 'w', encoding='utf-8', newline='\n') as f:
    f.write('\n'.join(log) + '\n')
print('\n'.join(log))
