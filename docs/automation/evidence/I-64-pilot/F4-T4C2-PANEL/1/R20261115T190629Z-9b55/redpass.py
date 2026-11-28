import re, subprocess, sys, xml.etree.ElementTree as ET

ns = {'t': 'http://microsoft.com/schemas/VisualStudio/TeamTest/2010'}
base = sys.argv[1]
trxs = sys.argv[2:]

diff = subprocess.run(['git', 'diff', '--cached', '-U0'], capture_output=True, text=True, encoding='utf-8').stdout
names = set()
for line in diff.splitlines():
    if line.startswith('+') and not line.startswith('+++'):
        m = re.search(r'public (?:static )?void (\w+)\(', line)
        if m:
            names.add(m.group(1))

for trx in trxs:
    root = ET.parse(trx).getroot()
    passed = []
    failed = []
    for u in root.iter('{%s}UnitTestResult' % ns['t']):
        full = u.get('testName')
        method = full.split('(')[0].split('.')[-1]
        if method in names:
            (passed if u.get('outcome') == 'Passed' else failed).append(full)
    print(trx, 'changed-or-new methods found:', len(names), 'passed:', len(passed), 'failed:', len(failed))
    for p in sorted(set(x.split('(')[0] for x in passed)):
        print('  PASS', p)
