"""PB-T4e: arma la v5.5 del paquete del contrato de PB-T4 desde la v5.4 (por objetos git en 160e019d, comprobada por sha256) con los fragmentos de
frag/ (head.md y ops-a.md a ops-d.md, en UTF-8), los valores medidos de la precondicion 6 (../../pbt4ecc/compile/values-v56.json) y lo que lee de
los archivos de la subtarea (las piezas, las clases de las CT, las lineas de la sonda y los RequiredTests).

- Cada OP (%%%% OP <after|before|replace> <id> / ANCHOR / TEXT / END) exige que su ANCHOR este exactamente una vez en el texto que se va editando.
- La cabecera: la de la v5.5 (head.md) arriba; la de la v5.4 pasa, sin cambio, a su seccion tras el registro v5.4 -> v5.5.
- El anexo V55-A, al final: el texto literal de la pieza del GREEN y de los dos archivos nuevos del RED, leidos de sus archivos.
- "Lo que cambia" sale del diff de las secciones (por encabezado, fuera de los bloques de codigo) entre la v5.4 y la v5.5, no se escribe a mano; el
  guion lo vuelve a calcular sobre el texto final y comprueba que coincide.
Este guion es ASCII; el texto no ASCII sale de los fragmentos y de los archivos que lee, y los pocos caracteres propios van con chr().

Uso (desde v55/): python -I make_v56.py <repo> <registro.txt>
"""
import collections
import hashlib
import importlib.util
import json
import os
import re
import subprocess
import sys

sys.dont_write_bytecode = True
HERE = os.path.dirname(os.path.abspath(__file__))
PB4 = os.path.normpath(os.path.join(HERE, '..'))
CC = os.path.normpath(os.path.join(HERE, '..', '..', 'pbt4ecc', 'compile'))
HS = os.path.normpath(os.path.join(HERE, '..', '..'))
REV = '160e019d773ba9095e2d5858cd593a954b8eec67'
PKG = 'docs/initiatives/I-64-f4b-pbt4-gate-contract-package.md'
V54_SHA = 'e37843fc4bb5a1d100191928ad06ce05cbf19705f5084b809442257ba866fc63'
V55_SHA = 'e597790ff700a2909b77c21960b5b013b2b108cf8073f2b866a0face25f8ae83'
OUT = os.path.join(PB4, 'I-64-f4b-pbt4-gate-contract-package-v56.md')
SECT, LAQUO, RAQUO, ORD, NN = chr(0xA7), chr(0xAB), chr(0xBB), chr(0xBA), chr(0xA0)


def sha(text):
    return hashlib.sha256(text.encode('utf-8')).hexdigest()


def sha_file(path, normalize=True):
    raw = open(path, 'rb').read()
    return hashlib.sha256(raw.decode('utf-8').replace('\r\n', '\n').encode('utf-8') if normalize else raw).hexdigest()


def read(path):
    return open(path, encoding='utf-8').read().replace('\r\n', '\n')


def ops_of(path):
    lines = read(path).split('\n')
    out, i = [], 0
    while i < len(lines):
        line = lines[i]
        if not line.startswith('%%%% OP '):
            assert line == '', (path, i + 1, line[:80])
            i += 1
            continue
        mode, oid = line[8:].split(' ', 1)
        assert mode in ('after', 'before', 'replace'), (path, line)
        assert lines[i + 1] == '%%%% ANCHOR', (path, i + 2)
        j = lines.index('%%%% TEXT', i + 2)
        k = lines.index('%%%% END', j + 1)
        out.append((mode, oid, '\n'.join(lines[i + 2:j]), '\n'.join(lines[j + 1:k])))
        i = k + 1
    return out


def sections(text):
    """[(encabezado, cuerpo)] fuera de los bloques de codigo; el primero es lo que va antes del primer encabezado de nivel 2."""
    out, head, body, fence = [], '(principio)', [], False
    for line in text.split('\n'):
        if line.startswith('```'):
            fence = not fence
        if not fence and re.match(r'^#{1,6} ', line) and not line.startswith('# '):
            out.append((head, '\n'.join(body)))
            head, body = line, []
        else:
            body.append(line)
    out.append((head, '\n'.join(body)))
    return out


def name_of(heading, annex):
    top = None
    if annex and annex.startswith('@'):
        annex, top = None, annex[1:]
    if annex:
        return 'el anexo ' + annex
    h = re.sub(r'^#+ ', '', heading)
    m = re.match(r'(D-PBT4-\d+)\.', h)
    if m:
        return m.group(1)
    m = re.match(r'(\d+(?:\.\d+)*)[.\s]', h)
    if m:
        return SECT + m.group(1)
    return 'la secci' + chr(0xF3) + 'n ' + LAQUO + h + RAQUO + (' de ' + SECT + top if top else '')


def diff(old, new):
    def keyed(text):
        res, annex, cnt, top = collections.OrderedDict(), None, collections.Counter(), None
        for head, body in sections(text):
            m = re.match(r'^## Anexo (V\d\d-A)\.', head)
            if m:
                annex = m.group(1)
            elif head.startswith('## ') and not head.startswith('## Anexo'):
                annex = None
                t = re.match(r'^## (\d+)\.', head)
                top = t.group(1) if t else None
            cnt[head] += 1
            res[(head, cnt[head])] = (body, annex if annex else ('@' + top if top else None))
        return res
    a, b = keyed(old), keyed(new)
    header_keys = {k for k in b if k[0].startswith('## Registro de cambios v5.5') or k[0].startswith('## La cabecera de la v5.5')
                   or k[0] == '(principio)'}
    removed = [k for k in a if k not in b and k[0] != '(principio)']
    # el anexo V55-A sale (lo sustituye el V56-A), y nada mas
    assert all(a[k][1] == 'V55-A' for k in removed), [k for k in removed if a[k][1] != 'V55-A']
    changed, new_ = [], []
    for k, (body, annex) in b.items():
        if k in header_keys:
            continue
        if k not in a:
            new_.append(name_of(k[0], annex))
        elif a[k][0] != body:
            changed.append(name_of(k[0], annex))
    same = [name_of(k[0], v[1]) for k, v in b.items() if k in a and k not in header_keys and a[k][0] == v[0]]
    return list(dict.fromkeys(changed)), list(dict.fromkeys(new_)), same


def diff_same_keys(old, new):
    """Las secciones (encabezado, orden) de `old`, fuera del principio, que estan en `new` con el mismo cuerpo."""
    def keyed(text):
        res, cnt = {}, collections.Counter()
        for head, body in sections(text):
            cnt[head] += 1
            res[(head, cnt[head])] = body
        return res
    a, b = keyed(old), keyed(new)
    return [k for k in a if k[0] != '(principio)' and k in b and a[k] == b[k]]


def ct_lines(path):
    """Las lineas esperadas de cada Finish de una clase de las CT, en su orden."""
    text = read(path)
    blocks = re.findall(r'Finish\(\s*log,\s*new\[\]\s*\{(.*?)\}\);', text, re.S)
    return [re.findall(r'^\s*"(.*)",\s*$', b, re.M) for b in blocks]


def probe_lines(path):
    """Las lineas de la sonda de una TRX (trx_lines_v55.py), por metodo de la clase Sonda."""
    res, cur = {}, None
    for line in read(path).split('\n'):
        m = re.match(r'^== \w+ RackCad\.(?:UI\.)?Tests\.Sonda\w+\.(CT_PBT4E_0\d)_', line)
        if m:
            cur = m.group(1)
            res[cur] = []
        elif line.startswith('== '):
            cur = None
        elif cur and line.startswith('  '):
            res[cur].append(line[2:])
    return res


def main():
    sys.stdout.reconfigure(encoding='utf-8', newline='\n')
    repo, log_path = sys.argv[1:3]
    val = CC
    if len(sys.argv) > 3:
        val = sys.argv[3]  # solo para un ensayo con valores de prueba; la version vale con la carpeta de la precondicion 6
    log = ['# make_v56.py: la v5.6 desde la v5.4 de %s, con la cabecera de la v5.5' % REV]
    v54 = subprocess.run(['git', '-C', repo, 'show', REV + ':' + PKG], capture_output=True, check=True).stdout.decode('utf-8')
    assert sha(v54) == V54_SHA, sha(v54)
    log.append('v5.4 %s (%d bytes)' % (sha(v54), len(v54.encode('utf-8'))))
    v55 = read(os.path.join(PB4, 'I-64-f4b-pbt4-gate-contract-package-v55.md'))
    assert sha(v55) == V55_SHA, sha(v55)
    log.append('v5.5 %s (%d bytes), la revisada por CA-T4e' % (sha(v55), len(v55.encode('utf-8'))))
    values = json.load(open(os.path.join(val, 'values-v56.json'), encoding='utf-8'))

    # ---- lo que se lee de los archivos de la subtarea
    ct_core, ct_ui = os.path.join(HERE, 'ct', 'PushBackEditAllReloadTests.cs'), os.path.join(HERE, 'ct', 'PushBackEditAllReloadFieldTests.cs')
    core_blocks, ui_blocks = ct_lines(ct_core), ct_lines(ct_ui)
    assert len(core_blocks) == 3 and len(ui_blocks) == 1, (len(core_blocks), len(ui_blocks))
    expected = {'CT_PBT4E_01': core_blocks[0], 'CT_PBT4E_02': core_blocks[1], 'CT_PBT4E_04': core_blocks[2], 'CT_PBT4E_03': ui_blocks[0]}
    green_probe = dict(probe_lines(os.path.join(CC, 'r82-tr', 'green-core-lineas.txt')), **probe_lines(os.path.join(CC, 'r82-tr', 'green-ui-lineas.txt')))
    red_probe = dict(probe_lines(os.path.join(CC, 'r82-tr', 'red-core-lineas.txt')), **probe_lines(os.path.join(CC, 'r82-tr', 'red-ui-lineas.txt')))
    for k, lines in expected.items():
        assert green_probe[k] == lines, ('la sonda del GREEN no da las lineas de la CT', k)
    n_lines = sum(len(v) for v in expected.values())
    assert n_lines == 25, n_lines
    log.append('CT: lineas %s; la sonda del GREEN las da todas tal cual' % {k: len(v) for k, v in sorted(expected.items())})
    red_rows = ['| CT | Lo que da el RED (medido, la sonda) | Lo esperado (' + SECT + '5.7) |', '|---|---|---|']
    for k in sorted(expected):
        diffs = [(r, e) for r, e in zip(red_probe[k], expected[k]) if r != e]
        assert diffs and len(red_probe[k]) == len(expected[k]), k
        for r, e in diffs:
            red_rows.append('| CT-PBT4E-%s | `%s` | `%s` |' % (k[-2:], r, e))
        log.append('RED %s: %d de %d lineas difieren' % (k, len(diffs), len(expected[k])))
    gm_spec = importlib.util.spec_from_file_location('green_pbt4e_v56', os.path.join(PB4, 'green_pbt4e_v56.py'))
    gm = importlib.util.module_from_spec(gm_spec)
    gm_spec.loader.exec_module(gm)
    base, green = gm.green(repo, REV)
    green_files = '\n'.join('- `%s`: `%s` (base `%s`)' % (p, sha(green[p]), sha(base[p])) for p in sorted(green))
    req = [l.split('\t') for l in read(os.path.join(val, 'required-pbt4e-v56.tsv')).rstrip('\n').split('\n')]
    req_rows = ['| n.' + ORD + ' | Project | Filter | MinSelected | ExpectRed | Origen |', '|---|---|---|---|---|---|']
    for r in req[1:]:
        req_rows.append('| %s | `%s` | `%s` | %s | %s | %s |' % (r[0], r[1], r[2], r[3], r[4], r[5].replace('n.o ', 'n.' + ORD + ' ')))
    fill = {
        'SHA_OWNER': sha_file(os.path.join(HS, 'pbt4e', 'owner-pbt4e-list-decisions.md'), normalize=False),
        'SHA_GREEN': sha_file(os.path.join(PB4, 'pbt4e-green-v56.txt')),
        'SHA_RED': sha_file(os.path.join(PB4, 'pbt4e-red-v56.txt')),
        'SHA_CT_CORE': sha_file(ct_core), 'SHA_CT_UI': sha_file(ct_ui),
        'SHA_GREEN_PY': sha_file(os.path.join(PB4, 'green_pbt4e_v56.py')), 'SHA_RED_PY': sha_file(os.path.join(PB4, 'red_pbt4e_v56.py')),
        'SHA_APPLY_GREEN': sha_file(os.path.join(PB4, 'apply_green_pbt4e_v56.py')), 'SHA_APPLY_RED': sha_file(os.path.join(PB4, 'apply_red_pbt4e_v56.py')),
        'SHA_MAKE_PIECES': sha_file(os.path.join(HERE, 'make_pieces_pbt4e_v56.py')),
        'CT01_LINES': '; '.join('`%s`' % x for x in expected['CT_PBT4E_01']),
        'CT02_LINES': '; '.join('`%s`' % x for x in expected['CT_PBT4E_02']),
        'CT03_LINES': '; '.join('`%s`' % x for x in expected['CT_PBT4E_03']),
        'CT04_LINES': '; '.join('`%s`' % x for x in expected['CT_PBT4E_04']),
        'RED_TABLE': '\n'.join(red_rows), 'GREEN_FILES': green_files, 'REQ_TABLE': '\n'.join(req_rows), 'REQ_N': str(len(req) - 1),
    }
    for k, v in values.items():
        assert k not in fill, k
        fill[k] = v

    # ---- las operaciones
    text = v54
    n_ops = 0
    for frag in ('ops-a.md', 'ops-b.md', 'ops-c.md', 'ops-d.md'):
        for mode, oid, anchor, insert in ops_of(os.path.join(HERE, 'frag', frag)):
            n = text.count(anchor)
            assert n == 1, ('el ANCHOR no esta una vez', frag, oid, n)
            if mode == 'before' and not insert.endswith('\n\n'):
                insert += '\n'
            i = text.index(anchor)
            if mode == 'replace':
                text = text[:i] + insert + text[i + len(anchor):]
            elif mode == 'after':
                text = text[:i + len(anchor)] + insert + text[i + len(anchor):]
            else:
                text = text[:i] + insert + text[i:]
            n_ops += 1
            log.append('op %s %s %s' % (frag, mode, oid))

    # ---- la cabecera: la de la v5.6 arriba y, debajo, la de la v5.5 tal como esta en su archivo (con su registro v5.4 -> v5.5 y la de la v5.4)
    reg = '\n---\n\n## Registro de cambios v5.3 ' + chr(0x2192) + ' v5.4'
    start, end = text.index('**Estado:** v5.4,'), text.index(reg)
    assert text[start:end] in v55
    region55 = v55[v55.index('**Estado:** v5.5,'):v55.index(reg)].rstrip('\n')
    head = read(os.path.join(HERE, 'frag', 'head.md')).rstrip('\n')
    text = text[:start] + head + '\n\n' + region55 + '\n' + text[end:]

    # ---- el anexo V56-A (sustituye al V55-A)
    green_piece, red_piece = read(os.path.join(PB4, 'pbt4e-green-v56.txt')), read(os.path.join(PB4, 'pbt4e-red-v56.txt'))
    annex = ['', '## Anexo V56-A. El texto literal de PB-T4e: el GREEN y los dos archivos nuevos del RED (v5.6)', '',
             'La pieza del RED (`pbt4e-red-v56.txt`, sha256 `%s`) son sus tres lineas de cabecera y un bloque NEWFILE por cada archivo de abajo, con su texto tal cual.' % fill['SHA_RED'],
             '', '### La pieza del GREEN, `pbt4e-green-v56.txt` (sha256 `%s`)' % fill['SHA_GREEN'], '', '```text', green_piece.rstrip('\n'), '```']
    for path, target in ((ct_core, 'tests/RackCad.Tests/PushBackEditAllReloadTests.cs'), (ct_ui, 'tests/RackCad.UI.Tests/PushBackEditAllReloadFieldTests.cs')):
        body = read(path)
        assert '```' not in body
        assert ('@@@@ NEWFILE ' + target + '\n' + body + '@@@@ END') in red_piece, target
        annex += ['', '### `%s` (`NewFileSha` `%s`)' % (target, sha(body)), '', '```csharp', body.rstrip('\n'), '```']
    assert '```' not in green_piece
    text = text.rstrip('\n') + '\n' + '\n'.join(annex) + '\n'

    # ---- los valores, y lo que cambia (del diff de las secciones)
    for k, v in fill.items():
        text = text.replace('{{' + k + '}}', v)
    changed, new_, same = diff(v55, text.replace('{{LO_QUE_CAMBIA}}', '').replace('{{LO_QUE_NO_CAMBIA}}', ''))
    for must_same in [SECT + x for x in ('4.1', '4.2', '4.3', '4.4', '5.0', '5.1', '5.2', '5.3', '5.6', '6.1', '6.2', '6.3', '6.4', '6.4.1', '6.4.2')] + \
            ['el anexo V49-A', 'el anexo V51-A', 'el anexo V54-A', 'D-PBT4-10', 'D-PBT4-21']:
        assert must_same in same and must_same not in changed, must_same
    # las 99 secciones de PB-T4a a PB-T4d (las de la v5.4 sin cambio en la v5.5) siguen sin cambio en la v5.6
    same54 = set(diff_same_keys(v54, v55)) & set(diff_same_keys(v54, text.replace('{{LO_QUE_CAMBIA}}', '').replace('{{LO_QUE_NO_CAMBIA}}', '')))
    assert len(set(diff_same_keys(v54, v55))) == len(same54) == 99, (len(set(diff_same_keys(v54, v55))), len(same54))
    def join(items):
        return ', '.join(items[:-1]) + ' y ' + items[-1] if len(items) > 1 else items[0]
    lo_que = join(changed) + '; nuevas: ' + join(new_)
    # las secciones sin cambio frente a la v5.5 que no son de esas 99, nombradas (las que la v5.5 toco o trajo y la v5.6 no cambia)
    text0 = text.replace('{{LO_QUE_CAMBIA}}', '').replace('{{LO_QUE_NO_CAMBIA}}', '')
    extra = sorted(set(diff_same_keys(v55, text0)) - same54, key=lambda k: v55.index(k[0]))
    assert len(extra) + len(same54) == len(same), (len(extra), len(same54), len(same))
    arrow = chr(0x2192)
    reg_name, cab_name = 'el registro v5.4 ' + arrow + ' v5.5', 'la cabecera de la v5.4'
    ct03_name = 'el archivo de CT-PBT4E-03 en el anexo V56-A (el mismo texto que en el V55-A)'

    def extra_name(head):
        if head.startswith('## Registro de cambios v5.4 '):
            return reg_name
        if head.startswith('## La cabecera de la v5.4,'):
            return cab_name
        if head.startswith('### `tests/RackCad.UI.Tests/PushBackEditAllReloadFieldTests.cs`'):
            return ct03_name
        return name_of(head, None)
    extra_names = [extra_name(k[0]) for k in extra]
    secs = ['D-PBT4-01', 'D-PBT4-19', 'D-PBT4-20', SECT + '5.4', SECT + '5.5', SECT + '8.3', SECT + '10']
    assert sorted(extra_names) == sorted(secs + [reg_name, cab_name, ct03_name]), extra_names
    no_cambia = ('las %d secciones de PB-T4a, PB-T4b, PB-T4c y PB-T4d (entre ellas ' % len(same54) + SECT + '4.1 a ' + SECT + '4.4, ' + SECT + '5.0 a ' +
                 SECT + '5.3, ' + SECT + '5.6, ' + SECT + '6.1 a ' + SECT + '6.4 con sus textos literales, D-PBT4-02 a D-PBT4-18 y D-PBT4-21, y los '
                 'anexos V49-A, V51-A y V54-A), sin cambio desde la v5.4; y %d m' % len(extra) + chr(0xE1) + 's que la v5.5 toc' + chr(0xF3) +
                 ' o trajo y la v5.6 no cambia: ' + join(secs) + ', ' + reg_name + ' y ' + cab_name + ' (en la cabecera de la v5.5) y ' + ct03_name +
                 ': %d secciones sin cambio frente a la v5.5, medido por el mismo diff' % len(same))
    log.append('sin cambio frente a la v5.5 y fuera de las %d de PB-T4a a PB-T4d: %d (%s)' % (len(same54), len(extra), '; '.join(extra_names)))
    text = text.replace('{{LO_QUE_CAMBIA}}', lo_que).replace('{{LO_QUE_NO_CAMBIA}}', no_cambia)
    left = re.findall(r'\{\{[A-Z0-9_]+\}\}', text)
    assert not left, sorted(set(left))
    c2, n2, s2 = diff(v55, text)
    assert (c2, n2) == (changed, new_), (c2, n2)
    for item in changed + new_:
        assert item in lo_que, item
    out = OUT if len(sys.argv) <= 4 else sys.argv[4]  # un ensayo escribe en otra ruta; la version es OUT
    with open(out, 'w', encoding='utf-8', newline='\n') as f:
        f.write(text)
    log.append('ops aplicadas %d' % n_ops)
    log.append('lo que cambia frente a la v5.5 (del diff): %s' % '; '.join(changed))
    log.append('nuevas (del diff): %s' % '; '.join(new_))
    log.append('sale: el anexo V55-A (lo sustituye el V56-A)')
    log.append('sin cambio frente a la v5.5: %d secciones; de PB-T4a a PB-T4d (sin cambio desde la v5.4): %d' % (len(same), len(same54)))
    log.append('v5.6 %s (%d bytes) %s' % (sha(text), len(text.encode('utf-8')), os.path.basename(out)))
    with open(log_path, 'w', encoding='utf-8', newline='\n') as f:
        f.write('\n'.join(log) + '\n')
    print('\n'.join(log[-5:]))


if __name__ == '__main__':
    main()
