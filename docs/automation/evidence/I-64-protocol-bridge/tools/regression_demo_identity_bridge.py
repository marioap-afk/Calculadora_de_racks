#!/usr/bin/env python3
"""Regression demonstration for I64-IDENTITY-BRIDGE-01 (I-64 A-13).
1. Runs the synthetic self-test against the verifier: expected PASS.
2. Runs it against the historical A-12 verifier, pinned by SHA-256: expected FAIL exactly on the relay-schema and object-query cases, which
   is the synthetic RED of findings I64-A12-RELAY-SCHEMA and I64-A12-NONEXISTENCE-ERROR.
3. Runs it against mutants of the verifier, each an exact text replacement found exactly once. Expected FAIL for every mutant, and each
   mutant fails the cases that target it. ControllerDefect reproduces the defect of the Controller runs 3d86, 16b6 and bfc8: Handoff integrity
   fails because CurrentSha does not match the repository.
Usage: python regression_demo_identity_bridge.py --verifier <path> --precheck <path> --selftest <path> --historical-a12 <path> [--out <path>].
Mutants run concurrently; the output order is fixed. Deterministic JSON."""
import argparse
import concurrent.futures
import hashlib
import json
import os
import shutil
import subprocess
import sys
import tempfile

A12_VERIFIER_SHA256 = 'de215a5873b76778f1c7935f0f5ea87d758d67c25db5cc1cb10cb216e35e584c'
# T49-T52 are absent on purpose: the strict parser of A-12 already rejected those relay inputs.
RELAY_CASES = ['T%02d' % i for i in range(33, 49)] + ['T53']
QUERY_CASES = ['T%02d' % i for i in range(55, 70)]
A12_MUST_FAIL = RELAY_CASES + QUERY_CASES

REAL_LINE = "    integrity('Real.CurrentSha', real['CurrentSha'] == a.current_sha, 'REAL_CURRENT_SHA_MISMATCH')\n"
HASH_TEST = "type(declared) is str and HEX64_RE.fullmatch(declared) and declared.lower() == sha256(raws['Mutated'])"
CHANGED = "    changed = [k for k in HANDOFF_KEYS if not exact_equal(real[k], mut[k])]\n"
EXACT_ANSWER = "    if stdout == ('%s missing\\n' % sha).encode('ascii'):\n"
# name: (exact text in the verifier, replacement, self-test cases the mutant must fail)
MUTANTS = {
    # Historical mutants of A-12
    'ControllerDefect': (REAL_LINE, REAL_LINE + "    integrity('Mutated.CurrentShaMatchesRepository', mut['CurrentSha'] == a.current_sha, 'HANDOFF_CONTRADICTS_REPOSITORY')\n", ['T02', 'T15']),
    'RealBindingUnchecked': ("real['CurrentSha'] == a.current_sha, 'REAL_CURRENT_SHA_MISMATCH'", "True, 'REAL_CURRENT_SHA_MISMATCH'", ['T25']),
    'WorktreeUnchecked': ("same_path(obj['Worktree'], a.repo), 'HANDOFF_WORKTREE_MISMATCH'", "True, 'HANDOFF_WORKTREE_MISMATCH'", ['T21']),
    'TopLevelDiffOnly': (CHANGED, "    changed = [k for k in HANDOFF_KEYS if type(real[k]) not in (list, dict) and not exact_equal(real[k], mut[k])]\n", ['T14']),
    'KeyOrderUnchecked': ("list(real.keys()) == list(mut.keys()), 'MUTATION_KEY_ORDER_CHANGED'", "True, 'MUTATION_KEY_ORDER_CHANGED'", ['T19']),
    'BytesUnchecked': ("integrity('Mutation.BytesExact', exact_bytes,", "integrity('Mutation.BytesExact', True,", ['T18']),
    'DuplicateKeysAllowed': ("object_pairs_hook=reject_duplicates, ", "", ['T22', 'T51']),
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
    'ExistingShaAllowed': ("check('Control.CandidateNonexistent', state == 'MISSING',", "check('Control.CandidateNonexistent', True,", ['T31', 'T55']),
    # Root cause 1: relay record against the official schema
    'RelaySchemaNotValidated': ("    relay_problems = relay_schema_problems(relay)\n", "    relay_problems = []\n", ['T33', 'T35', 'T37', 'T40', 'T42']),
    'SchemaPinUnchecked': ("    if git_blob_id(raw) != RELAY_RECORD_SCHEMA_BLOB:\n", "    if False:\n", ['T53']),
    'RequiredNotEnforced': ("            if key not in value:\n", "            if False:\n", ['T33', 'T34']),
    'AdditionalPropertiesIgnored': ("        if node.get('additionalProperties') is False:\n", "        if False:\n", ['T35', 'T36', 'T47']),
    'TypeNotEnforced': ("        if not any(type_ok(value, n) for n in names):\n", "        if False:\n", ['T37', 'T38', 'T39', 'T45', 'T46']),
    'EnumNotEnforced': ("    if 'enum' in node and not any(type(value) is type(e) and value == e for e in node['enum']):\n", "    if False:\n", ['T40', 'T41', 'T44']),
    'PatternNotEnforced': ("        if not re.search(pattern, value):\n", "        if False:\n", ['T42', 'T43']),
    'NestedNotValidated': ("                validate(value[key], sub, '%s.%s' % (path, key), problems)\n", "                pass\n", ['T34', 'T36', 'T38', 'T41', 'T43']),
    'ArrayItemsNotValidated': ("            validate(item, node['items'], '%s[%d]' % (path, i), problems)\n", "            pass\n", ['T44', 'T45', 'T46', 'T47']),
    'NonJsonConstantAllowed': ("parse_constant=reject_constant,", "", ['T50']),
    # Root cause 2: object existence
    'RcNonZeroMeansMissing': ("        return 'ERROR', 'git cat-file --batch-check exited with %d' % rc\n", "        return 'MISSING', ''\n", ['T56', 'T57', 'T58']),
    'RcIgnored': ("    if rc != 0:\n        return 'ERROR', 'git cat-file --batch-check", "    if False:\n        return 'ERROR', 'git cat-file --batch-check", ['T58']),
    'StderrIgnored': ("    if stderr:\n", "    if False:\n", ['T64']),
    'MalformedOutputAccepted': ("    return 'ERROR', 'malformed, incomplete or unexpected object query response'\n", "    return 'MISSING', ''\n", ['T62', 'T63', 'T69']),
    'IncompleteResponseAccepted': (EXACT_ANSWER, "    if stdout and ('%s missing\\n' % sha).encode('ascii').startswith(stdout):\n", ['T65', 'T66']),
    'ExtraOutputAccepted': (EXACT_ANSWER, "    if stdout.startswith(('%s missing\\n' % sha).encode('ascii')):\n", ['T67']),
    'AnswerObjectNotChecked': (EXACT_ANSWER, "    if stdout[40:] == b' missing\\n':\n", ['T68']),
    'ExecFailureMeansMissing': ("        return 'ERROR', 'the object query could not be executed: %s' % type(e).__name__\n", "        return 'MISSING', ''\n", ['T59', 'T60', 'T61']),
    'QueryErrorIgnored': ("state != 'ERROR', 'GIT_OBJECT_QUERY_FAILED'", "True, 'GIT_OBJECT_QUERY_FAILED'", ['T56', 'T62']),
    'QueryErrorNotFailClosed': (" or any(r['Code'] == 'GIT_OBJECT_QUERY_FAILED' for r in reasons)", "", ['T56', 'T62']),
}


def run(selftest, verifier, precheck):
    p = subprocess.run([sys.executable, '-I', selftest, '--verifier', verifier, '--precheck', precheck], capture_output=True)
    d = json.loads(p.stdout.decode('ascii'))
    return {'Result': d['Result'], 'Passed': d['Passed'], 'Total': d['Total'], 'ExitCode': p.returncode, 'FailedCases': [c['Id'] for c in d['Cases'] if not c['Ok']]}, {c['Id']: c for c in d['Cases']}


def main():
    p = argparse.ArgumentParser(add_help=False, allow_abbrev=False)
    for opt in ('--verifier', '--precheck', '--selftest', '--historical-a12'):
        p.add_argument(opt, required=True)
    p.add_argument('--out', default=None)
    a = p.parse_args()
    src = open(a.verifier, 'rb').read().decode('utf-8')
    for name, (old, _, _) in MUTANTS.items():
        assert src.count(old) == 1, 'mutation anchor of %s found %d times' % (name, src.count(old))
    a12 = open(a.historical_a12, 'rb').read()
    assert hashlib.sha256(a12).hexdigest() == A12_VERIFIER_SHA256, 'the historical A-12 verifier is not the pinned one'
    work = tempfile.mkdtemp(prefix='i64id-regression-')
    try:
        summary, _ = run(a.selftest, a.verifier, a.precheck)
        out = {'Schema': 'rackcad-i64-identity-bridge-regression/v1', 'VerifierSha256': hashlib.sha256(src.encode('utf-8')).hexdigest(),
               'SelftestSha256': hashlib.sha256(open(a.selftest, 'rb').read()).hexdigest(), 'PrecheckSha256': hashlib.sha256(open(a.precheck, 'rb').read()).hexdigest(), 'Verifier': summary, 'HistoricalA12': None, 'Mutants': {}}

        def one(name):
            old, new, targets = MUTANTS[name]
            path = os.path.join(work, 'verify_%s.py' % name)
            with open(path, 'wb') as f:
                f.write(src.replace(old, new).encode('utf-8'))
            r, cases = run(a.selftest, path, a.precheck)
            r['Replaces'], r['With'] = old.strip(), new.strip()
            r['MustFail'] = targets
            r['MustFailOk'] = all(t in r['FailedCases'] for t in targets)
            if name == 'ControllerDefect':
                r['Signature'] = {'Case': 'T15', 'Got': cases['T15']['Got'], 'ReasonCodes': cases['T15']['ReasonCodes']}
            return r

        with concurrent.futures.ThreadPoolExecutor(max_workers=max(2, (os.cpu_count() or 4) - 2)) as ex:
            hist_future = ex.submit(run, a.selftest, a.historical_a12, a.precheck)
            futures = {name: ex.submit(one, name) for name in MUTANTS}
            hist, _ = hist_future.result()
            hist['BlobSha256'] = A12_VERIFIER_SHA256
            hist['MustFail'] = A12_MUST_FAIL
            hist['FailedOnlyAsExpected'] = hist['FailedCases'] == A12_MUST_FAIL
            out['HistoricalA12'] = hist
            for name in MUTANTS:
                out['Mutants'][name] = futures[name].result()
    finally:
        shutil.rmtree(work, ignore_errors=True)
    mutants_ok = all(m['Result'] == 'FAIL' and m['ExitCode'] != 0 and m['MustFailOk'] for m in out['Mutants'].values())
    out['Result'] = 'PASS' if out['Verifier']['Result'] == 'PASS' and mutants_ok and out['HistoricalA12']['Result'] == 'FAIL' and out['HistoricalA12']['FailedOnlyAsExpected'] else 'FAIL'
    data = (json.dumps(out, indent=2, ensure_ascii=True) + '\n').encode('ascii')
    if a.out:
        with open(a.out, 'wb') as f:
            f.write(data)
    sys.stdout.buffer.write(data)
    return 0 if out['Result'] == 'PASS' else 1


if __name__ == '__main__':
    sys.exit(main())
