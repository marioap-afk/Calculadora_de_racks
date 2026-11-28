"""Resumen de un TRX (solo lectura): total, Passed, Failed, Skipped/NotExecuted y, de cada Failed, la primera linea del StackTrace.
Uso: python -I trxsum.py <archivo.trx> [--stack]"""
import sys
import xml.etree.ElementTree as ET

NS = {'t': 'http://microsoft.com/schemas/VisualStudio/TeamTest/2010'}


def main():
    sys.stdout.reconfigure(encoding='utf-8', newline='\n')
    path = sys.argv[1]
    stack = '--stack' in sys.argv
    root = ET.parse(path).getroot()
    counts = {}
    failed = []
    for r in root.iter('{%s}UnitTestResult' % NS['t']):
        o = r.get('outcome')
        counts[o] = counts.get(o, 0) + 1
        if o == 'Failed':
            st = r.find('.//t:StackTrace', NS)
            msg = r.find('.//t:Message', NS)
            first = ''
            if st is not None and st.text:
                lines = [l.strip() for l in st.text.split('\n') if l.strip()]
                first = lines[0] if lines else ''
            failed.append((r.get('testName'), first, (msg.text or '')[:200].replace('\n', ' | ') if msg is not None else ''))
    total = sum(counts.values())
    print('%s total=%d %s' % (path, total, ' '.join('%s=%d' % (k, counts[k]) for k in sorted(counts))))
    for n, first, msg in failed:
        print('  FAILED %s' % n)
        if stack:
            print('     first-stack-line: %s' % first)
            print('     message: %s' % msg)


if __name__ == '__main__':
    main()
