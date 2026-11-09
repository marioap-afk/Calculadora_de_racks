#!/usr/bin/env python3
"""Regression demonstration for I64-IDENTITY-BRIDGE-01 (I-64 A-12).
1. Runs the synthetic self-test against the verifier: expected PASS.
2. Runs it against mutants of the verifier, each an exact text replacement found exactly once. Expected FAIL for every mutant, and
   each mutant fails the cases that target it. ControllerDefect reproduces the defect of the Controller runs 3d86, 16b6 and bfc8:
   Handoff integrity fails because CurrentSha does not match the repository.
Usage: python regression_demo_identity_bridge.py --verifier <path> --selftest <path> [--out <path>]. Deterministic JSON."""
import argparse
import hashlib
import json
import os
import shutil
import subprocess
import sys
import tempfile

REAL_LINE = "    integrity('Real.CurrentSha', real['CurrentSha'] == a.current_sha, 'REAL_CURRENT_SHA_MISMATCH')\n"
HASH_TEST = "type(declared) is str and HEX64_RE.fullmatch(declared) and declared.lower() == sha256(raws['Mutated'])"
CHANGED = "    changed = [k for k in HANDOFF_KEYS if not exact_equal(real[k], mut[k])]\n"
# name: (exact text in the verifier, replacement, self-test cases the mutant must fail)
MUTANTS = {
    'ControllerDefect': (REAL_LINE, REAL_LINE + "    integrity('Mutated.CurrentShaMatchesRepository', mut['CurrentSha'] == a.current_sha, 'HANDOFF_CONTRADICTS_REPOSITORY')\n", ['T02', 'T15']),
    'RealBindingUnchecked': ("real['CurrentSha'] == a.current_sha, 'REAL_CURRENT_SHA_MISMATCH'", "True, 'REAL_CURRENT_SHA_MISMATCH'", ['T25']),
    'WorktreeUnchecked': ("same_path(obj['Worktree'], a.repo), 'HANDOFF_WORKTREE_MISMATCH'", "True, 'HANDOFF_WORKTREE_MISMATCH'", ['T21']),
    'TopLevelDiffOnly': (CHANGED, "    changed = [k for k in HANDOFF_KEYS if type(real[k]) not in (list, dict) and not exact_equal(real[k], mut[k])]\n", ['T14']),
    'KeyOrderUnchecked': ("list(real.keys()) == list(mut.keys()), 'MUTATION_KEY_ORDER_CHANGED'", "True, 'MUTATION_KEY_ORDER_CHANGED'", ['T19']),
    'BytesUnchecked': ("integrity('Mutation.BytesExact', exact_bytes,", "integrity('Mutation.BytesExact', True,", ['T18']),
    'DuplicateKeysAllowed': ("object_pairs_hook=reject_duplicates, ", "", ['T22']),
    'RelayHashUnchecked': (HASH_TEST, "True", ['T08']),
    'RelayKindUnchecked': ("outcome.get('Kind') == 'COMPLETED', 'RELAY_NOT_COMPLETED'", "True, 'RELAY_NOT_COMPLETED'", ['T27']),
    'OutputTimeUnchecked': ("start <= written <= end, 'RELAY_OUTPUT_TIME_INVALID'", "True, 'RELAY_OUTPUT_TIME_INVALID'", ['T28']),
    'BranchUnchecked': ("rc == 0 and gout.decode('utf-8', errors='strict').strip() == a.branch, 'BRANCH_MISMATCH'", "True, 'BRANCH_MISMATCH'", ['T17']),
    'HeadRemoteUnchecked': ("head == remote and head == tracking, 'HEAD_REMOTE_DISAGREE'", "True, 'HEAD_REMOTE_DISAGREE'", ['T10']),
    'BaseAncestryUnchecked': ("a.base_sha != a.red_sha and is_ancestor(a.repo, a.base_sha, a.red_sha), 'BASE_NOT_ANCESTOR'", "True, 'BASE_NOT_ANCESTOR'", ['T11']),
    'RedChainUnchecked': ("a.red_sha != a.current_sha and is_ancestor(a.repo, a.red_sha, a.current_sha), 'RED_NOT_ON_CHAIN'", "True, 'RED_NOT_ON_CHAIN'", ['T12']),
    'DescendantHeadAccepted': ("head == a.current_sha, 'HEAD_NOT_VERIFIED_SHA'", "True, 'HEAD_NOT_VERIFIED_SHA'", ['T16']),
    'IdentityAlwaysFails': ("    out['Identity'] = 'PASS' if candidate == head else 'FAIL'\n", "    out['Identity'] = 'FAIL'\n", ['T01', 'T09']),
    'SyntheticNotRequired': ("candidate == a.synthetic_sha, 'MUTATED_SHA_NOT_AUTHORIZED'", "True, 'MUTATED_SHA_NOT_AUTHORIZED'", ['T02', 'T30']),
    'ExistingShaAllowed': ("check('Control.CandidateNonexistent', rc != 0,", "check('Control.CandidateNonexistent', True,", ['T31']),
}


def run(selftest, verifier):
    p = subprocess.run([sys.executable, '-I', selftest, '--verifier', verifier], capture_output=True)
    d = json.loads(p.stdout.decode('ascii'))
    return {'Result': d['Result'], 'Passed': d['Passed'], 'Total': d['Total'], 'ExitCode': p.returncode, 'FailedCases': [c['Id'] for c in d['Cases'] if not c['Ok']]}, {c['Id']: c for c in d['Cases']}


def main():
    p = argparse.ArgumentParser(add_help=False, allow_abbrev=False)
    for opt in ('--verifier', '--selftest'):
        p.add_argument(opt, required=True)
    p.add_argument('--out', default=None)
    a = p.parse_args()
    src = open(a.verifier, 'rb').read().decode('utf-8')
    for name, (old, _, _) in MUTANTS.items():
        assert src.count(old) == 1, 'mutation anchor of %s found %d times' % (name, src.count(old))
    work = tempfile.mkdtemp(prefix='i64id-regression-')
    try:
        summary, _ = run(a.selftest, a.verifier)
        out = {'Schema': 'rackcad-i64-identity-bridge-regression/v1', 'VerifierSha256': hashlib.sha256(src.encode('utf-8')).hexdigest(),
               'SelftestSha256': hashlib.sha256(open(a.selftest, 'rb').read()).hexdigest(), 'Verifier': summary, 'Mutants': {}}
        for name, (old, new, targets) in MUTANTS.items():
            path = os.path.join(work, 'verify_%s.py' % name)
            with open(path, 'wb') as f:
                f.write(src.replace(old, new).encode('utf-8'))
            r, cases = run(a.selftest, path)
            r['Replaces'], r['With'] = old.strip(), new.strip()
            r['MustFail'] = targets
            r['MustFailOk'] = all(t in r['FailedCases'] for t in targets)
            if name == 'ControllerDefect':
                r['Signature'] = {'Case': 'T15', 'Got': cases['T15']['Got'], 'ReasonCodes': cases['T15']['ReasonCodes']}
            out['Mutants'][name] = r
    finally:
        shutil.rmtree(work, ignore_errors=True)
    mutants_ok = all(m['Result'] == 'FAIL' and m['ExitCode'] != 0 and m['MustFailOk'] for m in out['Mutants'].values())
    out['Result'] = 'PASS' if out['Verifier']['Result'] == 'PASS' and mutants_ok else 'FAIL'
    data = (json.dumps(out, indent=2, ensure_ascii=True) + '\n').encode('ascii')
    if a.out:
        with open(a.out, 'wb') as f:
            f.write(data)
    sys.stdout.buffer.write(data)
    return 0 if out['Result'] == 'PASS' else 1


if __name__ == '__main__':
    sys.exit(main())
