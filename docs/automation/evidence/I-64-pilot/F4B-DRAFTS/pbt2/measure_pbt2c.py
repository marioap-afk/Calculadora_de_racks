# -*- coding: utf-8 -*-
"""La remedicion de PB-T2c sobre el cierre de PB-T2b (paquete de PB-T2 v2.9, section 6.0, anclajes de PB-T2c; v2.6, V26-02; CA-T2c M-01). Escribe
pbt2c-anchors-out.txt, que se custodia con la emision de PB-T2c y es autoridad para el Worker (v2.6, V26-04):
- Parte 1, las anclas de PB-T2c: los textos de los tres mutantes [T2b] (MU-T2C-01 a 03) sobre el cierre, con la linea y la declaracion de su
  miembro y el texto mutado. Los localiza make_pbt2c_contract.py en seco sobre el cierre (su mu_check, con el aborto de V26-08 si MU-T2C-03 no
  vale); este guion lee esas lineas por objetos git y las escribe enteras. Con los blobs de W, WC y WX del cierre.
- Parte 2, la tabla de CT-PBT2C-03: una fila por gesto de CT-PBT1C-02 con Compara (49, en su orden), '<fila> aplicado=<b> igual-a-la-ventana=<b>
  diseno-igual=<b> descartes=<n>', y '  porque: ...' debajo de la que se niega. Sale del TRX de una corrida temporal de UI de los 49 gestos en
  WORKSPACE, fuera de todo commit (INV-10): la clase PushBackPbt2cCt03ProbeTests.cs.txt. Se comprueba con el ct3_table_check del generador; una
  fila fuera de los tres casos de la regla (aplica con diseno-igual=False, o se niega con diseno-igual=True) es un hecho nuevo: otra version del
  paquete y, si muestra una diferencia entre CLASSIC y WORKSPACE, el Owner (V26-02). Entonces no escribe nada.

El procedimiento (la sesion, tras el cierre de PB-T2b, con el codigo del GREEN de PB-T2b; P-08: el worktree solo cuando no hay cesion):
  1. rellenar los marcadores POR-RELLENAR-T2B de la clase con la API del GREEN de PB-T2b (PushBackWorkspaceSupport.cs) y borrar su #error;
     python -I measure_pbt2c.py check-probe PushBackPbt2cCt03ProbeTests.cs.txt
  2. en un arbol del cierre, sin commit: copiar la clase a tests/RackCad.UI.Tests/PushBackPbt2cCt03ProbeTests.cs y correr
     dotnet test tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj --filter "FullyQualifiedName~PushBackPbt2cCt03ProbeTests" --logger "trx;LogFileName=pbt2c-ct03.trx"
  3. borrar la clase (git status limpio: nada de la medida queda en el arbol);
  4. python -I measure_pbt2c.py table <checkout principal> <cierre de PB-T2b, 40 hex> <pbt2c-ct03.trx> <pbt2c-anchors-out.txt>
     [--generator <make_pbt2c_contract.py>] [--package-file <paquete v2.9>]
Solo lee: git por objetos desde el checkout principal (show, rev-parse, cat-file) y el generador en seco con --objects-rev. Escribe solo la salida
(table). En la salida no va ninguna ruta local: del TRX, su nombre y su SHA-256."""
import ast, hashlib, os, re, subprocess, sys
import xml.etree.ElementTree as ET

HERE = os.path.dirname(os.path.abspath(__file__))
NL = chr(10)
PROBE_CLASS = 'PushBackPbt2cCt03ProbeTests'
PROBE_TEST = PROBE_CLASS + '.PBT2C_CT03_Probe_MeasuresTheTableOfTheRemediation'
INICIO, FIN = 'PBT2C-CT03-TABLA-INICIO', 'PBT2C-CT03-TABLA-FIN'
MARK = 'POR-RELLENAR-T2B'
TRX_NS = '{http://microsoft.com/schemas/VisualStudio/TeamTest/2010}'  # el espacio de nombres que escribe el logger trx de vstest (corregido por la sesion al medir)
CASES = {('True', 'True', 'True', '0'): 'aplica', ('False', 'True', 'True', '0'): 'nada', ('False', 'False', 'False', '1'): 'se niega'}
ROW = re.compile(r'^(\S+) aplicado=(True|False) igual-a-la-ventana=(True|False) diseno-igual=(True|False) descartes=(\d+)$')


def opt(args, name, default):
    return args[args.index(name) + 1] if name in args else default


def asc(s):
    return ''.join(ch if ord(ch) < 0x80 else '?' for ch in s)


INERT_BOXES = ('PostBox', 'CellInOutBeamBox', 'CellInOutPeralteBox')  # CA-T2c seccion 13, M-13: con el catalogo del cierre, una sola opcion
INERT_WHY = '  porque: el catalogo solo ofrece una opcion'


def inert_rows(rows):
    """CA-T2c seccion 13, M-13: bajo cada fila "nada" de un gesto '<caja>=otro' escribe '  porque: el catalogo solo ofrece una opcion'.
    La regla mira dos cosas, y no solo el nombre:
    - la fila es "nada" (aplicado=False igual-a-la-ventana=True diseno-igual=True descartes=0);
    - el gesto es '<caja>=otro', el ayudante Otro de CT-PBT1C-02 (PushBackWindowOnStateTests.cs:442), que elige otra opcion siempre que la lista
      tenga mas de una. Una seleccion de catalogo que el comparador ve solo puede quedar en "nada" si la lista tiene una sola opcion.
    Las tres cajas que la seccion 13 comprobo en el catalogo del cierre son la lista cerrada INERT_BOXES:
    - otra caja '=otro' en "nada" aborta: es un hecho nuevo, y hay que mirar el catalogo;
    - una de las tres que ya no es "nada" (el catalogo gano opciones) no lleva la linea y sale en el aviso.
    Devuelve (las filas con las lineas, las filas inertes, las cajas de la lista que ya no son inertes). Las lineas '  porque:' no las copia el
    Worker (v2.6, V26-04), y ct3_table_check las admite tras cualquier fila."""
    out, inert = [], []
    for l in rows:
        out.append(l)
        m = ROW.match(l)
        if not (m and m.group(1).endswith('=otro') and CASES.get(m.groups()[1:]) == 'nada'):
            continue
        box = m.group(1)[:-len('=otro')]
        assert box in INERT_BOXES, ('una seleccion "otro" que no aplica, fuera de las tres de M-13: es un hecho nuevo y hay que mirar el catalogo', m.group(1))
        out.append(INERT_WHY)
        inert.append(m.group(1))
    gone = [b for b in INERT_BOXES if b + '=otro' not in inert]
    return out, inert, gone


def utc_z(ts):
    """Una hora ISO 8601 del TRX ('2026-11-19T13:08:05.5611731-06:00') en UTC, al segundo, con 'Z' y sin el desfase local (repo publico: la zona
    horaria de la maquina no se custodia). Si no se puede leer, '<sin hora>': el TRX lo identifica su SHA-256."""
    import datetime
    m = re.fullmatch(r'(\d{4})-(\d\d)-(\d\d)T(\d\d):(\d\d):(\d\d)(?:\.\d+)?(Z|([+-])(\d\d):(\d\d))', ts or '')
    if not m:
        return '<sin hora>'
    t = datetime.datetime(*(int(m.group(k)) for k in range(1, 7)))
    if m.group(7) != 'Z':
        off = datetime.timedelta(hours=int(m.group(9)), minutes=int(m.group(10)))
        t = t - off if m.group(8) == '+' else t + off
    return t.strftime('%Y-%m-%dT%H:%M:%SZ')


def check_probe(path):
    """La clase temporal, lista para copiar: sin marcadores ni #error, con su clase, su prueba y sus dos marcas del TRX."""
    t = open(path, 'rb').read().decode('utf-8')
    left = [k for k, l in enumerate(t.split(NL), 1) if MARK in l or l.lstrip().startswith('#error')]
    assert not left, ('la clase temporal tiene marcadores POR-RELLENAR-T2B o #error en las lineas', left)
    for need in ('public sealed class ' + PROBE_CLASS, 'public void ' + PROBE_TEST.split('.')[-1] + '()', '"' + INICIO + '"', '"' + FIN + '"',
                 '[Collection(EditorDiscardPromptCollection.Name)]', 'typeof(PushBackWindowOnStateTests).GetMethod("Gestos"'):
        assert t.count(need) == 1, ('la clase temporal', need)
    return 'check-probe: lista para copiar (%d lineas, sin %s ni #error)' % (t.count(NL), MARK)


def gen_parts(gsrc):
    """Del generador, sin ejecutarlo: las rutas W, WC y WX, MUTANTS (con MU3_FRAG, MU3_IN y MU3_OUT) y ct3_table_check."""
    want = {'W', 'WC', 'WX', 'MU3_FRAG', 'MU3_IN', 'MU3_OUT', 'MUTANTS'}
    ns = {}
    for node in ast.parse(gsrc).body:
        if not isinstance(node, ast.Assign) or len(node.targets) != 1:
            continue
        tg = node.targets[0]
        names = [tg.id] if isinstance(tg, ast.Name) else [e.id for e in getattr(tg, 'elts', []) if isinstance(e, ast.Name)]
        if names and set(names) <= want and not set(names) <= set(ns):
            exec(compile(ast.Module(body=[node], type_ignores=[]), 'make_pbt2c_contract', 'exec'), ns)
    assert want <= set(ns), ('el generador sin', sorted(want - set(ns)))
    blk = gsrc[gsrc.index('# ---- ct3_table_check (inicio)'):gsrc.index('# ---- ct3_table_check (fin)')]
    cns = {'re': re}
    exec(compile(blk, 'ct3_table_check', 'exec'), cns)
    return ns, cns['ct3_table_check']


def table(args):
    repo, close, trx, out = args[:4]
    gen = os.path.abspath(opt(args, '--generator', os.path.join(HERE, 'make_pbt2c_contract.py')))
    pkg = os.path.abspath(opt(args, '--package-file', os.path.join(HERE, 'I-64-f4b-pbt2-gate-contract-package-v29.md')))
    assert re.fullmatch(r'[0-9a-f]{40}', close), 'el cierre de PB-T2b: 40 hex'

    def git(*a):
        r = subprocess.run(['git', '-C', repo] + list(a), capture_output=True)
        assert r.returncode == 0, ('git', a[:2], r.stderr.decode('utf-8', 'replace')[-200:])
        return r.stdout

    git('cat-file', '-e', close + '^{commit}')
    main = git('rev-parse', 'origin/main').decode('ascii').strip()
    gsrc = open(gen, 'rb').read().decode('utf-8')
    C, ct3_table_check = gen_parts(gsrc)
    # el generador en seco sobre el cierre: localiza los mutantes (mu_check) y da las filas de CT-PBT2C-03 (los gestos de CT-PBT1C-02 del cierre)
    r = subprocess.run([sys.executable, '-I', gen, '2026-01-01T00:00:00Z', '0' * 40, main, '--dry', '--objects-rev', close, '--repo', repo,
                        '--pt2b-close', close, '--package-file', pkg], capture_output=True, cwd=os.path.dirname(gen))
    assert r.returncode == 0, ('el generador en seco sobre el cierre no da rc 0', r.stderr.decode('utf-8', 'replace')[-1500:])
    g = r.stdout.decode('ascii').replace('\r\n', NL).split(NL)
    k = g.index('mutantes [T2b] (testigos ASCII):')
    mut = [l.strip() for l in g[k + 1:k + 4]]
    assert len(mut) == 3 and all(l.startswith('MU-T2C-0') for l in mut) and not any('pendiente' in l for l in mut), ('los mutantes sobre el cierre', mut)
    k = [i for i, l in enumerate(g) if l.startswith('CT-PBT2C-03: las filas que tiene que traer la tabla de la remedicion (')][0]
    names = []
    for l in g[k + 1:]:
        if not l.startswith('  '):
            break
        names += l.strip().split(' | ')
    assert len(names) == 49 == len(set(names)), ('las filas de CT-PBT2C-03', len(names))
    anchor = [l for l in g if l.startswith('anclaje: ')][0]
    # Parte 1: las anclas, enteras
    paths = {'W': C['W'], 'WC': C['WC']}
    texts = {f: git('show', close + ':' + p).decode('utf-8').replace('\r\n', NL).split(NL) for f, p in paths.items()}
    spec = {m[0]: m for m in C['MUTANTS']}
    p1 = ['== cierre de PB-T2b %s | %s' % (close, anchor[len('anclaje: '):]),
          '== blobs del cierre: W %s, WC %s, WX %s' % tuple(git('rev-parse', close + ':' + C[x]).decode('ascii').strip()[:12] for x in ('W', 'WC', 'WX'))]
    for l in mut:
        m = re.match(r'^(MU-T2C-0\d) (W|WC):(\d+) en (\S+) \(declarado en (?:W|WC):(\d+)\)(, por contenido)?$', l)
        assert m, ('la linea del mutante', l)
        mid, f, n, member, nd = m.group(1), m.group(2), int(m.group(3)), m.group(4), int(m.group(5))
        _, smember, sf, mode, line, sub, decl = spec[mid]
        orig, dline = texts[f][n - 1], texts[f][nd - 1]
        assert smember == member and sf == f and dline.strip() == decl, (mid, member, f, dline.strip())
        if mode == 'linea':
            assert orig.strip() == line, (mid, orig.strip())
            mutated = orig[:len(orig) - len(orig.lstrip())] + sub
        else:
            assert orig.count(C['MU3_IN']) == 1 and line in orig, (mid, orig.strip())
            mutated = orig.replace(C['MU3_IN'], C['MU3_OUT'])
        p1 += [l, '  declaracion: ' + dline.strip(), '  linea:       ' + orig.strip(), '  mutante:     ' + mutated.strip()]
    # Parte 2: la tabla, del TRX
    raw = open(trx, 'rb').read()
    root = ET.fromstring(raw)
    results = list(root.iter(TRX_NS + 'UnitTestResult'))
    assert len(results) == 1 and results[0].get('testName', '').endswith(PROBE_TEST), (
        'el TRX tiene que traer solo la prueba temporal', [x.get('testName') for x in results][:5])
    res = results[0]
    assert res.get('outcome') == 'Passed', ('la prueba temporal no paso', res.get('outcome'))
    so = res.find(TRX_NS + 'Output/' + TRX_NS + 'StdOut')
    assert so is not None and so.text, 'el TRX sin StdOut de la prueba temporal'
    lines = [l.rstrip('\r') for l in so.text.replace('\r\n', NL).split(NL)]
    assert lines.count(INICIO) == 1 and lines.count(FIN) == 1, 'las marcas de la tabla en el StdOut'
    rows = lines[lines.index(INICIO) + 1:lines.index(FIN)]
    bad = [l for l in rows if l.startswith('EXCEPCION') or re.search(r'(^| )(sin-borrador|sin-ventana|modo=)', l)]
    assert not bad, ('la corrida temporal trae lineas que no son de la tabla (EXCEPCION, sin ventana o sin la sonda del modo)', bad[:5])
    assert INERT_WHY not in rows, 'el TRX ya trae la linea de M-13: la escribe este guion, no la clase temporal'
    rows, inert, gone = inert_rows(rows)  # CA-T2c seccion 13, M-13
    if gone:
        print('AVISO (M-13): ya no son inertes, el catalogo gano opciones: %s. El limite de cobertura de F4B-E39 ya no vale para ellas.' % ', '.join(gone))
    body = '# la tabla de CT-PBT2C-03' + NL + NL.join(rows) + NL
    errors = ct3_table_check(body, names)
    if errors:
        print(asc(body))
        raise AssertionError(('la tabla no vale (una fila fuera de los tres casos es un hecho nuevo: otra version del paquete y, si muestra una '
                              'diferencia entre CLASSIC y WORKSPACE, el Owner; v2.6, V26-02)', [asc(e) for e in errors]))
    count = {}
    for l in rows:
        m = ROW.match(l)
        if m:
            c = CASES[m.groups()[1:]]
            count[c] = count.get(c, 0) + 1
    head = ['# measure_pbt2c.py: la remedicion de PB-T2c sobre el cierre de PB-T2b (paquete de PB-T2 v2.9, section 6.0; v2.6, V26-02; CA-T2c M-01)',
            '# Parte 1: las anclas de PB-T2c, los textos de los tres mutantes [T2b] sobre el cierre (mu_check de make_pbt2c_contract.py en seco).',
            '# Parte 2: la tabla de CT-PBT2C-03, de una corrida temporal de UI de los 49 gestos en WORKSPACE, fuera de todo commit y con TRX (INV-10).',
            '# El Worker copia las 49 filas, en su orden y sin las lineas "  porque:" (v2.6, V26-04).', '']
    tail = ['', '== TRX %s, sha256 %s | %s %s | %s a %s' % (os.path.basename(trx), hashlib.sha256(raw).hexdigest().upper(), PROBE_TEST, res.get('outcome'),
                                                           utc_z(res.get('startTime')), utc_z(res.get('endTime'))),
            '== filas: %d | aplica %d, nada %d, se niega %d | inertes porque el catalogo solo ofrece una opcion (CA-T2c M-13): %s' % (
                len(names), count.get('aplica', 0), count.get('nada', 0), count.get('se niega', 0), ', '.join(inert) or 'ninguna')]
    text = NL.join(head + ['## Parte 1'] + p1 + ['', '## Parte 2']) + NL + body + NL.join(tail) + NL
    assert ct3_table_check(text, names) == [], 'la salida entera, con el ct3_table_check del generador'
    open(out, 'wb').write(text.encode('utf-8'))
    return 'table: %s escrita (%d bytes; 3 mutantes; 49 filas: aplica %d, nada %d, se niega %d)' % (
        os.path.basename(out), len(text.encode('utf-8')), count.get('aplica', 0), count.get('nada', 0), count.get('se niega', 0))


if __name__ == '__main__':
    a = sys.argv[1:]
    if a[:1] == ['check-probe'] and len(a) == 2:
        print(check_probe(a[1]))
    elif a[:1] == ['table'] and len(a) >= 5:
        print(asc(table(a[1:])))
    else:
        raise SystemExit('uso: measure_pbt2c.py check-probe <clase> | table <checkout principal> <cierre> <trx> <salida> '
                         '[--generator <g>] [--package-file <p>]')
