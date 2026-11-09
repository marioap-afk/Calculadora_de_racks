#!/usr/bin/env python3
"""Regression demonstration for I64-TESTFILTER-BRIDGE-01 (I-64 A-9).
1. Runs the synthetic self-test against the corrected verifier: expected PASS.
2. Runs it against mutants of the corrected verifier, each an exact text replacement: four replace the FQN selection with a
   rejected method (display name, class name only, case-sensitive FQN, method name only); five break the potential-match rule
   (surrogate pair collapsed to one position, global non-ASCII rejection, non-ASCII treated as a mismatch, unknown overriding an
   all-ASCII match, Python casefold deciding the match). Expected FAIL for every mutant, and each potential-match mutant fails
   the cases that target it.
3. Runs it against the exact historical A-8 verifier blob (extracted with git cat-file and checked by blob id): expected
   FAIL, and the A-8 verifier ACCEPTS (Result PASS, exit 0) the signature fixtures of the three REQUIRED findings of the
   Architect review R20261109T062937Z-e1d0 that the corrected verifier rejects.
Usage: python regression_demo_testfilter_bridge.py --verifier <path> --selftest <path> --repo <git repo> --historical-blob <blob>
       [--out <path>]. Deterministic JSON."""
import argparse
import hashlib
import json
import os
import shutil
import subprocess
import sys
import tempfile

SEL = '    sel = [x for x in rows if fqn_decision(definitions[x[0]][0], definitions[x[0]][1], substring) == SELECTED]\n'
FQN_UNITS = "    fqn = utf16_units(class_name + '.' + method_name)\n"
UNKNOWN_UNIT = '            if u > 0x7F:\n                state = UNKNOWN\n'
PRECEDENCE = '    if SELECTED in windows:\n        return SELECTED\n    return UNKNOWN if UNKNOWN in windows else NOT_SELECTED\n'
# name: (exact text in the corrected verifier, replacement, self-test cases the mutant must fail)
MUTANTS = {
    'DisplayName': (SEL, "    sel = [x for x in rows if fold(substring) in fold(x[2])]\n", []),
    'ClassNameOnly': (SEL, "    sel = [x for x in rows if fold(substring) in fold(definitions[x[0]][0])]\n", []),
    'CaseSensitiveFqn': (SEL, "    sel = [x for x in rows if substring in definitions[x[0]][0] + '.' + definitions[x[0]][1]]\n", []),
    'MethodNameOnly': (SEL, "    sel = [x for x in rows if fold(substring) in fold(definitions[x[0]][1])]\n", []),
    'CodePointPositions': (FQN_UNITS, "    fqn = [ord(ch) for ch in class_name + '.' + method_name]\n", ['T61', 'T62']),
    'GlobalNonAsciiReject': (FQN_UNITS, FQN_UNITS + '    if any(u > 0x7F for u in fqn):\n        return UNKNOWN\n', ['T58', 'T59', 'T60', 'T62']),
    'NonAsciiAsMismatch': (UNKNOWN_UNIT, '            if u > 0x7F:\n                state = NOT_SELECTED\n                break\n', ['T36', 'T37', 'T61']),
    'UnknownOverridesAsciiMatch': (PRECEDENCE, '    if UNKNOWN in windows:\n        return UNKNOWN\n    return SELECTED if SELECTED in windows else NOT_SELECTED\n', ['T60']),
    'PythonCasefold': (FQN_UNITS, "    return SELECTED if substring.casefold() in (class_name + '.' + method_name).casefold() else NOT_SELECTED\n", ['T36', 'T37']),
}
# Signature fixtures of the three REQUIRED findings (the corrected verifier must reject them; A-8 accepted them)
SIGNATURES = {'I64-A8-ORDINAL-FILTER-MISMATCH': ['T36', 'T37'], 'I64-A8-COUNTER-COHERENCE': ['T39'],
              'I64-A8-DUPLICATE-RESULT-FAIL-OPEN': ['T49', 'T50']}


def run(selftest, verifier):
    p = subprocess.run([sys.executable, '-I', selftest, '--verifier', verifier], capture_output=True)
    d = json.loads(p.stdout.decode('ascii'))
    cases = {c['Id']: c for c in d['Cases']}
    return {'Result': d['Result'], 'Passed': d['Passed'], 'Total': d['Total'], 'ExitCode': p.returncode,
            'FailedCases': [c['Id'] for c in d['Cases'] if not c['Ok']]}, cases


def main():
    p = argparse.ArgumentParser(add_help=False, allow_abbrev=False)
    for opt in ('--verifier', '--selftest', '--repo', '--historical-blob'):
        p.add_argument(opt, required=True)
    p.add_argument('--out', default=None)
    a = p.parse_args()
    src = open(a.verifier, 'rb').read().decode('utf-8')
    for name, (old, _, _) in MUTANTS.items():
        assert src.count(old) == 1, 'mutation anchor of %s not found exactly once' % name
    work = tempfile.mkdtemp(prefix='i64tf-regression-')
    try:
        verifier_summary, verifier_cases = run(a.selftest, a.verifier)
        out = {'Schema': 'rackcad-i64-testfilter-bridge-regression/v2', 'VerifierSha256': hashlib.sha256(src.encode('utf-8')).hexdigest(),
               'SelftestSha256': hashlib.sha256(open(a.selftest, 'rb').read()).hexdigest(), 'Verifier': verifier_summary, 'Mutants': {}}
        for name, (old, new, targets) in MUTANTS.items():
            path = os.path.join(work, 'verify_%s.py' % name)
            with open(path, 'wb') as f:
                f.write(src.replace(old, new).encode('utf-8'))
            r, _ = run(a.selftest, path)
            r['Replaces'], r['With'] = old.strip(), new.strip()
            r['MustFail'] = targets
            r['MustFailOk'] = all(t in r['FailedCases'] for t in targets)
            out['Mutants'][name] = r
        hist = subprocess.run(['git', '-C', a.repo, 'cat-file', 'blob', a.historical_blob], capture_output=True)
        assert hist.returncode == 0, 'historical blob not found'
        hpath = os.path.join(work, 'verify_testfilter_bridge_a8.py')
        with open(hpath, 'wb') as f:
            f.write(hist.stdout)
        hblob = subprocess.run(['git', 'hash-object', '--no-filters', hpath], capture_output=True, text=True).stdout.strip()
        hsum, hcases = run(a.selftest, hpath)
        sig = {}
        for finding, ids in SIGNATURES.items():
            sig[finding] = [{'Case': i, 'A8Got': hcases[i]['Got'], 'A8ExitCode': hcases[i]['ExitCode'], 'A9Got': verifier_cases[i]['Got'],
                             'A9ReasonCodes': verifier_cases[i]['ReasonCodes'],
                             'A8AcceptedBadFixture': hcases[i]['Got'] == 'PASS' and hcases[i]['ExitCode'] == 0,
                             'A9Rejected': verifier_cases[i]['Got'] == 'FAIL' and verifier_cases[i]['Ok']} for i in ids]
        # every self-test case A-8 does not satisfy: its result and reason codes, and which assertions failed (an assertion that
        # reads an output field introduced by A-9, such as NonAsciiFqn, cannot hold on A-8)
        details = [{'Case': i, 'Expected': hcases[i]['Expected'], 'A8Got': hcases[i]['Got'], 'A8ExitCode': hcases[i]['ExitCode'],
                    'A8ReasonCodes': hcases[i]['ReasonCodes'], 'A8FailedAsserts': sorted(k for k, v in hcases[i]['Asserts'].items() if not v),
                    'A9Got': verifier_cases[i]['Got'], 'A9ReasonCodes': verifier_cases[i]['ReasonCodes']} for i in hsum['FailedCases']]
        out['HistoricalA8'] = dict(hsum, Blob=a.historical_blob, ExtractedBlob=hblob, ExtractedSha256=hashlib.sha256(hist.stdout).hexdigest(),
                                   SignatureCases=sig, FailedCaseDetails=details)
    finally:
        shutil.rmtree(work, ignore_errors=True)
    hist_ok = (out['HistoricalA8']['ExtractedBlob'] == a.historical_blob and out['HistoricalA8']['Result'] == 'FAIL'
               and all(any(x['A8AcceptedBadFixture'] and x['A9Rejected'] for x in v) for v in out['HistoricalA8']['SignatureCases'].values()))
    mutants_ok = all(m['Result'] == 'FAIL' and m['ExitCode'] != 0 and m['MustFailOk'] for m in out['Mutants'].values())
    out['Result'] = 'PASS' if (out['Verifier']['Result'] == 'PASS' and mutants_ok and hist_ok) else 'FAIL'
    data = (json.dumps(out, indent=2, ensure_ascii=True) + '\n').encode('ascii')
    if a.out:
        with open(a.out, 'wb') as f:
            f.write(data)
    sys.stdout.buffer.write(data)
    return 0 if out['Result'] == 'PASS' else 1


if __name__ == '__main__':
    sys.exit(main())
