"""Comprueba tests/<RunRef>/all-summary.txt contra inv/filters.tsv: cada filtro corrido tiene Passed >= MinSelected y Failed == 0.
Uso: python -I checkall.py <RunRef>"""
import re
import sys

A = 'artifacts/orchestration/I-64/F4B-PBT4-EDIT-ALL-RELOAD/1/R20261123T045827Z-6c9e'


def main():
    sys.stdout.reconfigure(encoding='utf-8', newline='\n')
    run = sys.argv[1]
    mins = {}
    for line in open(A + '/inv/filters.tsv', encoding='utf-8'):
        k, f, m, r = line.rstrip('\n').split('\t')
        mins[(k, f)] = (int(m), r)
    cur = None
    bad = 0
    seen = 0
    rows = []
    for line in open('%s/tests/%s/all-summary.txt' % (A, run), encoding='utf-8'):
        line = line.rstrip('\n')
        m = re.match(r'== (\w+) (\S+) min=(\d+) red=(\w+)', line)
        if m:
            cur = (m.group(1), m.group(2))
            continue
        m = re.search(r'\.trx total=(\d+)(.*)$', line)
        if m and cur:
            total = int(m.group(1))
            kv = dict(p.split('=') for p in m.group(2).split())
            passed = int(kv.get('Passed', 0))
            failed = int(kv.get('Failed', 0))
            skipped = int(kv.get('NotExecuted', 0)) + int(kv.get('Skipped', 0))
            mn, red = mins[cur]
            ok = passed >= mn and failed == 0
            seen += 1
            if not ok:
                bad += 1
            rows.append('%s %s %s min=%d total=%d passed=%d failed=%d skipped=%d' % ('OK ' if ok else 'BAD', cur[0], cur[1], mn, total, passed, failed, skipped))
            cur = None
    print('\n'.join(rows))
    print('filtros=%d violaciones=%d' % (seen, bad))


if __name__ == '__main__':
    main()
