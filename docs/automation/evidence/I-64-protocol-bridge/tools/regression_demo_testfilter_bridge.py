#!/usr/bin/env python3
"""Regression demonstration for I64-TESTFILTER-BRIDGE-01 (I-64 A-8): builds four mutants of the verifier, each replacing the
FQN selection with one rejected method (display name, class name only, case-sensitive FQN, method name only), and runs the
synthetic self-test against the real verifier and against each mutant. Expected: verifier PASS; every mutant FAIL.
Usage: python regression_demo_testfilter_bridge.py --verifier <path> --selftest <path> [--out <path>]. Deterministic JSON."""
import argparse
import hashlib
import json
import os
import shutil
import subprocess
import sys
import tempfile

SEL = '    sel = [x for x in rows if fqn_selected(definitions[x[0]][0], definitions[x[0]][1], substring)]\n'
MUTANTS = {
    'DisplayName': "    sel = [x for x in rows if fold(substring) in fold(x[2])]\n",
    'ClassNameOnly': "    sel = [x for x in rows if fold(substring) in fold(definitions[x[0]][0])]\n",
    'CaseSensitiveFqn': "    sel = [x for x in rows if substring in definitions[x[0]][0] + '.' + definitions[x[0]][1]]\n",
    'MethodNameOnly': "    sel = [x for x in rows if fold(substring) in fold(definitions[x[0]][1])]\n",
}


def run(selftest, verifier):
    p = subprocess.run([sys.executable, '-I', selftest, '--verifier', verifier], capture_output=True)
    d = json.loads(p.stdout.decode('ascii'))
    return {'Result': d['Result'], 'Passed': d['Passed'], 'Total': d['Total'], 'ExitCode': p.returncode,
            'FailedCases': [c['Id'] for c in d['Cases'] if not c['Ok']]}


def main():
    p = argparse.ArgumentParser(add_help=False, allow_abbrev=False)
    p.add_argument('--verifier', required=True)
    p.add_argument('--selftest', required=True)
    p.add_argument('--out', default=None)
    a = p.parse_args()
    src = open(a.verifier, 'rb').read().decode('utf-8')
    assert src.count(SEL) == 1, 'selection line not found exactly once'
    work = tempfile.mkdtemp(prefix='i64tf-mutants-')
    try:
        out = {'Schema': 'rackcad-i64-testfilter-bridge-regression/v1', 'VerifierSha256': hashlib.sha256(src.encode('utf-8')).hexdigest(),
               'SelftestSha256': hashlib.sha256(open(a.selftest, 'rb').read()).hexdigest(), 'Verifier': run(a.selftest, a.verifier), 'Mutants': {}}
        for name, line in MUTANTS.items():
            path = os.path.join(work, 'verify_%s.py' % name)
            with open(path, 'wb') as f:
                f.write(src.replace(SEL, line).encode('utf-8'))
            r = run(a.selftest, path)
            r['SelectionLine'] = line.strip()
            out['Mutants'][name] = r
    finally:
        shutil.rmtree(work, ignore_errors=True)
    out['Result'] = 'PASS' if out['Verifier']['Result'] == 'PASS' and all(m['Result'] == 'FAIL' and m['ExitCode'] != 0 for m in out['Mutants'].values()) else 'FAIL'
    data = (json.dumps(out, indent=2, ensure_ascii=True) + '\n').encode('ascii')
    if a.out:
        with open(a.out, 'wb') as f:
            f.write(data)
    sys.stdout.buffer.write(data)
    return 0 if out['Result'] == 'PASS' else 1


if __name__ == '__main__':
    sys.exit(main())
