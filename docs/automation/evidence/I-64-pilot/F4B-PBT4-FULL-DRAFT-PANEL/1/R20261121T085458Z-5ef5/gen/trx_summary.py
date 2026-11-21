"""Resume un TRX: por prueba, el resultado y, si fallo, la primera linea del StackTrace y si esta bajo tests/ (por asercion).
Uso: python -I trx_summary.py <archivo.trx> [--verbose]"""
import re
import sys
import xml.etree.ElementTree as ET

NS = {'t': 'http://microsoft.com/schemas/VisualStudio/TeamTest/2010'}


def main():
    sys.stdout.reconfigure(encoding='utf-8', errors='replace')
    path = sys.argv[1]
    verbose = '--verbose' in sys.argv
    root = ET.parse(path).getroot()
    counts = {}
    by_assertion = 0
    failed = 0
    for r in root.iter('{%s}UnitTestResult' % NS['t']):
        outcome = r.get('outcome')
        counts[outcome] = counts.get(outcome, 0) + 1
        if outcome == 'Failed':
            failed += 1
            st = r.find('t:Output/t:ErrorInfo/t:StackTrace', NS)
            msg = r.find('t:Output/t:ErrorInfo/t:Message', NS)
            first = ''
            if st is not None and st.text:
                for line in st.text.split('\n'):
                    line = line.strip()
                    if line.startswith('at '):
                        first = line
                        break
            under_tests = bool(re.search(r'in .*[\\/]tests[\\/]', first))
            by_assertion += 1 if under_tests else 0
            if verbose or not under_tests:
                print('FAIL', r.get('testName')[:120], '| bajo-tests=%s' % under_tests)
                print('   ', first[:200])
                if msg is not None and msg.text:
                    print('   ', msg.text.strip().split('\n')[0][:200])
    total = sum(counts.values())
    print('Total=%d %s | Failed por asercion (primera linea bajo tests/)=%d de %d' % (total, ', '.join('%s=%d' % (k, v) for k, v in sorted(counts.items())), by_assertion, failed))


if __name__ == '__main__':
    main()
