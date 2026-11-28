"""Criterio del RED de F4-T4B (INV-F4T4B-15) sobre el TRX de Core de la CI del RED. Uso (desde la raiz del worktree):
python _trx_check_t4b.py <W> <BaseSha> <RedSha>
Cada fallo: por asercion (Message sin prefijo de tipo cualificado seguido de ' : '; primera linea de StackTrace bajo tests/) y de una prueba
nueva o cambiada (archivo nuevo del RED, o metodo cuya declaracion o cuerpo toca el diff del RED)."""
import xml.etree.ElementTree as ET, re, sys, subprocess
ns = '{http://microsoft.com/schemas/VisualStudio/TeamTest/2010}'
W, B, R = sys.argv[1], sys.argv[2], sys.argv[3]
BS = chr(92)
under_tests = re.compile('(?i)[' + BS + BS + '/]tests[' + BS + BS + '/]')
prefix = re.compile('^[A-Za-z_][' + BS + 'w.]*' + BS + '.[A-Za-z_]' + BS + 'w* : ')
git = lambda *a: subprocess.run(['git'] + list(a), capture_output=True, text=True, encoding='utf-8').stdout
status = [l.split('\t') for l in git('diff', '--name-status', B, R, '--', 'tests').splitlines()]
new_classes = {p.rsplit('/', 1)[-1][:-3] for s, p in status if s == 'A'}
changed = {p.rsplit('/', 1)[-1][:-3]: p for s, p in status if s == 'M'}
# Methods touched by the RED diff in the changed files: the enclosing method of each changed line, by the hunk header and the added lines.
touched = {}
decl = re.compile('public' + BS + 's+(?:async' + BS + 's+)?(?:void|Task)' + BS + 's+(' + BS + 'w+)' + BS + 's*' + BS + '(')
for cls, path in changed.items():
    red_text = git('show', R + ':' + path).splitlines()
    lines = set()
    for h in re.finditer('^@@ -' + BS + 'd+(?:,' + BS + 'd+)? ' + BS + '+(' + BS + 'd+)(?:,(' + BS + 'd+))? @@', git('diff', '-U0', B, R, '--', path), re.M):
        start, count = int(h.group(1)), int(h.group(2) or '1')
        lines.update(range(start, start + max(count, 1)))
    names, current = set(), None
    for no, text in enumerate(red_text, 1):
        m = decl.search(text)
        if m: current = m.group(1)
        if no in lines and current: names.add(current)
    touched[cls] = names
root = ET.parse(W + '/red-ci-core/core.trx').getroot()
defs = {u.get('id'): (u.find(ns + 'TestMethod').get('className'), u.find(ns + 'TestMethod').get('name')) for u in root.iter(ns + 'UnitTest')}
bad, notnew, n = [], [], 0
for r in root.iter(ns + 'UnitTestResult'):
    if r.get('outcome') != 'Failed': continue
    n += 1
    cls, name = defs[r.get('testId')]
    short = cls.rsplit('.', 1)[-1]
    method = name.split('(')[0].rsplit('.', 1)[-1]
    msg = r.find(f'{ns}Output/{ns}ErrorInfo/{ns}Message')
    st = r.find(f'{ns}Output/{ns}ErrorInfo/{ns}StackTrace')
    m = (msg.text or '') if msg is not None else ''
    s = (st.text or '') if st is not None else ''
    first = next((l for l in s.splitlines() if l.strip()), '')
    if prefix.match(m) or not under_tests.search(first):
        bad.append((short + '.' + method, m[:80], first[-100:]))
    if short not in new_classes and method not in touched.get(short, set()):
        notnew.append(short + '.' + method)
print('failed', n, 'non-assertion', len(bad), 'not-new-or-changed', len(notnew))
print('new classes', sorted(new_classes))
print('changed classes', {k: len(v) for k, v in touched.items()})
for b in bad[:8]: print('  BAD', b)
for x in notnew[:12]: print('  NOTNEW', x)
