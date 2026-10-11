#!/usr/bin/env python3
"""Deterministic synthetic self-test of I64-FREETEXT-BRIDGE-01 (verify_freetext_bridge.py), I-64 A-5.

Builds synthetic handoffs and synthetic term-set files in a temporary directory, runs the verifier on each case and
prints a byte-reproducible ASCII/LF JSON summary (no paths, no timestamps). The embedded 16.10 section is a byte copy
of AUTOMATION_PLAN.md 16.10 at main bb0d5522e8411f66a51fdfb3f1f0d0514b737453 (its SHA-256 is reported).
Python standard library only. Exit 0 only if every case matches its expectation.
"""
import argparse
import copy
import hashlib
import json
import os
import shutil
import subprocess
import sys
import tempfile

SECTION_LINES = [
    '### 16.10 Términos de gate',
    '',
    'Coincidencia por palabra o frase completa, sin distinguir mayúsculas, en `WorkCompleted`, `Evidence`, `UnexpectedFindings`, `KnownLimitations`, `Deviations`, `OpenQuestions` y',
    '`RecommendedNextAction`: `GATE PASS`, `GATE_PASS`, `Candidato`, `Candidate`, `FINAL_CANDIDATE_SHA`, `gate cerrado`, `gate closed`, `cierre del gate`, `rama integrada`,',
    '`integrada en main`, `integrated into main`, `merged into main`, `lista para integrar`, `ready to merge`, `Owner Validation APROBADA` y `Owner Validation APPROVED`. No se aplica al',
    'nombre de propiedad `Gate` ni a su valor. El Controller los comprueba en la entrega; el Coordinator, en el texto libre de la verificación, como control manual registrado.',
    '',
]
SECTION = '\n'.join(SECTION_LINES)
DOC = '# AUTOMATION_PLAN (sintetico)\n\n## 16. Ejecucion delegada\n\n' + SECTION + '\n### 16.11 Siguiente\n\nTexto.\n'

BASE = {
    'Schema': 'rackcad-worker-handoff/v1', 'TaskId': 'F9-SYNTH', 'RunId': 'R20990101T000000Z-abcd',
    'DelegationRunId': 'R20990101T000000Z-0001', 'Initiative': 'I-99', 'Gate': 'F9', 'Attempt': 1,
    'BaseSha': 'a' * 40, 'RedSha': 'b' * 40, 'CurrentSha': 'c' * 40, 'Branch': 'architecture/synthetic',
    'Worktree': 'synthetic-worktree', 'Pushed': True, 'FilesChanged': ['src/A.cs', 'tests/B.cs'],
    'WorkCompleted': 'Commit RED and commit GREEN delivered; the gate decision belongs to the Coordinator.',
    'TestsExecuted': [{'RunRef': 'r1', 'Command': 'dotnet test', 'Phase': 'GREEN', 'TreeSha': 'c' * 40, 'ResultsFile': None}],
    'TestResults': [{'RunRef': 'r1', 'Selected': 3, 'Passed': 3, 'Failed': 0, 'Skipped': 0, 'FailedTests': []}],
    'Evidence': ['filter 3/3 green', 'Core green'], 'UnexpectedFindings': [], 'KnownLimitations': ['host work is later'],
    'Deviations': [], 'OpenQuestions': [], 'WorkerStatus': 'IMPLEMENTATION_COMPLETE', 'Disposition': 'NONE',
    'TriggeredStopConditions': [], 'RecommendedNextAction': 'Controller verification.',
    'Worker': {'Provider': 'Anthropic', 'ModelRequested': 'm', 'EffortRequested': 'high', 'Trailer': 't'},
}
APPEND = ' GATE PASS.'


def dump(obj):
    return (json.dumps(obj, indent=2, ensure_ascii=False) + '\n').encode('utf-8')


def mutate(obj, append=APPEND):
    m = copy.deepcopy(obj)
    m['WorkCompleted'] = m['WorkCompleted'] + append
    return m


def with_(obj, **kw):
    o = copy.deepcopy(obj)
    o.update(kw)
    return o


def cases():
    real = BASE
    mut = mutate(BASE)
    doc = DOC.encode('utf-8')
    c = []
    add = lambda cid, name, r, m, ts, exit_, codes: c.append((cid, name, r, m, ts, exit_, codes))
    add('T01', 'standard mutation: real clean, mutated appends GATE PASS to WorkCompleted', dump(real), dump(mut), doc, 0, [])
    dirty = with_(real, KnownLimitations=['Candidate SHA pending'])
    add('T02', 'real handoff already contains a forbidden term', dump(dirty), dump(mutate(dirty)), doc, 1, ['REAL_HAS_FORBIDDEN_TERM'])
    add('T03', 'mutation also changes a second field', dump(real), dump(with_(mut, Deviations=['x'])), doc, 1, ['UNAUTHORIZED_FIELD_CHANGE'])
    add('T04', 'mutation changes identity (CurrentSha)', dump(real), dump(with_(mut, CurrentSha='d' * 40)), doc, 1, ['IDENTITY_CHANGED'])
    add('T05', 'mutation changes FilesChanged', dump(real), dump(with_(mut, FilesChanged=['src/A.cs'])), doc, 1, ['IDENTITY_CHANGED'])
    add('T06', 'mutation removes text before appending', dump(real), dump(with_(real, WorkCompleted=real['WorkCompleted'][:-12] + APPEND)), doc, 1, ['NOT_EXACT_AUTHORIZED_APPEND'])
    add('T07', 'forbidden term placed in another governed field instead', dump(real), dump(with_(real, RecommendedNextAction=real['RecommendedNextAction'] + APPEND)), doc, 1, ['UNAUTHORIZED_FIELD_CHANGE', 'NO_FORBIDDEN_TERM_INTRODUCED'])
    add('T08', 'malformed JSON in the mutated handoff', dump(real), dump(mut)[:-40], doc, 1, ['MUTATED_UNREADABLE'])
    add('T09', 'duplicate JSON key', dump(real), dump(mut).replace(b'"Pushed": true,', b'"Pushed": true,\n  "Pushed": true,', 1), doc, 1, ['MUTATED_UNREADABLE'])
    add('T10', 'NaN constant', dump(real), dump(mut).replace(b'"Attempt": 1', b'"Attempt": NaN', 1), doc, 1, ['MUTATED_UNREADABLE'])
    add('T11', 'Infinity constant', dump(real), dump(mut).replace(b'"Attempt": 1', b'"Attempt": -Infinity', 1), doc, 1, ['MUTATED_UNREADABLE'])
    add('T12', 'lone surrogate escape', dump(real), dump(mut).replace(b'"host work is later"', b'"host ' + bytes([92]) + b'ud800 work"', 1), doc, 1, ['MUTATED_UNREADABLE'])
    no_wc = {k: v for k, v in real.items() if k != 'WorkCompleted'}
    add('T13', 'WorkCompleted missing in both handoffs', dump(no_wc), dump(no_wc), doc, 1, ['REAL_SCHEMA_INVALID'])
    add('T14', 'term set missing', dump(real), dump(mut), None, 1, ['TERM_SET_UNREADABLE'])
    add('T15', 'term set ambiguous: two 16.10 sections', dump(real), dump(mut), (DOC + '\n' + SECTION + '\n').encode('utf-8'), 1, ['TERM_SET_AMBIGUOUS'])
    add('T16', 'term set without the matching-semantics sentence', dump(real), dump(mut), DOC.replace('sin distinguir', 'distinguiendo').encode('utf-8'), 1, ['TERM_SET_AMBIGUOUS'])
    add('T17', 'term set with a malformed term enumeration', dump(real), dump(mut), DOC.replace('`RecommendedNextAction`: `', '`RecommendedNextAction`: ', 1).encode('utf-8'), 1, ['TERM_SET_AMBIGUOUS'])
    lower = with_(real, Evidence=['the gate closed for this task'])
    add('T18', 'case variant (lowercase) in the real handoff is a forbidden hit', dump(lower), dump(mutate(lower)), doc, 1, ['REAL_HAS_FORBIDDEN_TERM'])
    upper = with_(real, OpenQuestions=['CANDIDATO pendiente'])
    add('T19', 'case variant (uppercase) in the real handoff is a forbidden hit', dump(upper), dump(mutate(upper)), doc, 1, ['REAL_HAS_FORBIDDEN_TERM'])
    sub = with_(real, KnownLimitations=['Candidates', 'Candidatos', 'GATE PASSED', 'XGATE_PASS', 'FINAL_CANDIDATE_SHAS'])
    add('T20', 'substrings that are not whole words or phrases are not hits', dump(sub), dump(mutate(sub)), doc, 0, [])
    add('T21', 'a different appended term (GATE_PASS) is not the authorized mutation', dump(real), dump(mutate(real, ' GATE_PASS.')), doc, 1, ['NOT_EXACT_AUTHORIZED_APPEND'])
    add('T22', 'whitespace variant GATE  PASS is neither the mutation nor a term', dump(real), dump(mutate(real, ' GATE  PASS.')), doc, 1, ['NOT_EXACT_AUTHORIZED_APPEND', 'NO_FORBIDDEN_TERM_INTRODUCED'])
    add('T23', 'no mutation at all', dump(real), dump(real), doc, 1, ['UNAUTHORIZED_FIELD_CHANGE', 'NO_FORBIDDEN_TERM_INTRODUCED'])
    add('T24', 'extra top-level key in the mutated handoff', dump(real), dump(with_(mut, Extra=1)), doc, 1, ['MUTATED_SCHEMA_INVALID'])
    reordered = {k: mut[k] for k in reversed(list(mut.keys()))}
    add('T25', 'top-level key order changed', dump(real), dump(reordered), doc, 1, ['KEY_ORDER_CHANGED'])
    add('T26', 'byte order mark', dump(real), b'\xef\xbb\xbf' + dump(mut), doc, 1, ['MUTATED_UNREADABLE'])
    add('T27', 'not UTF-8 (latin-1 byte)', dump(real), dump(mut).replace(b'later', b'l\xe1ter', 1), doc, 1, ['MUTATED_UNREADABLE'])
    add('T28', 'wrong handoff schema', dump(real), dump(with_(mut, Schema='rackcad-worker-handoff/v2')), doc, 1, ['MUTATED_SCHEMA_INVALID'])
    bad_type = with_(real, Evidence='filter green')
    add('T29', 'governed field with a non-string, non-list type', dump(bad_type), dump(mutate(bad_type)), doc, 1, ['GOVERNED_FIELD_TYPE'])
    add('T30', 'term set with carriage returns', dump(real), dump(mut), DOC.replace('\n', '\r\n').encode('utf-8'), 1, ['TERM_SET_AMBIGUOUS'])
    comp = with_(real, Deviations=['GATE PASS noted'])
    add('T31', 'forbidden term disappears from another field to compensate', dump(comp), dump(with_(mutate(comp), Deviations=['noted'])), doc, 1, ['REAL_HAS_FORBIDDEN_TERM', 'COMPENSATING_DISAPPEARANCE'])
    add('T32', 'nested duplicate key (Worker object)', dump(real), dump(mut).replace(b'"Provider": "Anthropic",', b'"Provider": "Anthropic",\n    "Provider": "Anthropic",', 1), doc, 1, ['MUTATED_UNREADABLE'])
    add('T33', 'type change of a non-governed field (Attempt 1 -> true)', dump(real), dump(mut).replace(b'"Attempt": 1', b'"Attempt": true', 1), doc, 1, ['IDENTITY_CHANGED'])
    return c


def main():
    ap = argparse.ArgumentParser(add_help=False, allow_abbrev=False)
    ap.add_argument('--verifier', default=os.path.join(os.path.dirname(os.path.abspath(__file__)), 'verify_freetext_bridge.py'))
    a = ap.parse_args()
    vbytes = open(a.verifier, 'rb').read()
    tmp = tempfile.mkdtemp(prefix='ftb-selftest-')
    results = []
    try:
        for cid, name, real, mut, ts, exp_exit, exp_codes in cases():
            d = os.path.join(tmp, cid)
            os.makedirs(d)
            rp, mp, tp = os.path.join(d, 'real.json'), os.path.join(d, 'mutated.json'), os.path.join(d, 'term-set.md')
            open(rp, 'wb').write(real)
            open(mp, 'wb').write(mut)
            if ts is not None:
                open(tp, 'wb').write(ts)
            p = subprocess.run([sys.executable, a.verifier, '--real-handoff', rp, '--mutated-handoff', mp, '--term-set', tp], capture_output=True)
            try:
                o = json.loads(p.stdout.decode('ascii'))
                res, codes = o['Result'], sorted({r['Code'] for r in o['Reasons']})
                intro = o.get('IntroducedForbiddenTerms')
            except Exception:
                res, codes, intro = 'NO_JSON', [], None
            ok = p.returncode == exp_exit and (res == 'PASS') == (exp_exit == 0) and all(x in codes for x in exp_codes)
            if exp_exit == 0:
                ok = ok and codes == [] and intro == ['GATE PASS']
            results.append({'Id': cid, 'Name': name, 'ExpectedExit': exp_exit, 'ActualExit': p.returncode, 'ExpectedCodes': exp_codes,
                            'ActualResult': res, 'ActualCodes': codes, 'Introduced': intro, 'Pass': ok})
        for cid, args, exp_exit in (('T34', ['-h'], 2), ('T35', ['--real-handoff', 'x'], 2), ('T36', ['--real-handoff', 'x', '--mutated-handoff', 'y', '--term-set', 'z', '--extra'], 2)):
            p = subprocess.run([sys.executable, a.verifier] + args, capture_output=True)
            results.append({'Id': cid, 'Name': 'usage error ' + ' '.join(args), 'ExpectedExit': exp_exit, 'ActualExit': p.returncode,
                            'ExpectedCodes': [], 'ActualResult': 'USAGE', 'ActualCodes': [], 'Introduced': None,
                            'Pass': p.returncode == exp_exit and p.stdout == b''})
    finally:
        shutil.rmtree(tmp, ignore_errors=True)
    summary = {'Schema': 'rackcad-i64-freetext-bridge-selftest/v1', 'VerifierSha256': hashlib.sha256(vbytes).hexdigest(),
               'EmbeddedSectionSha256': hashlib.sha256(SECTION.encode('utf-8')).hexdigest(),
               'Cases': len(results), 'Passed': sum(r['Pass'] for r in results), 'Failed': sum(not r['Pass'] for r in results),
               'Result': 'PASS' if all(r['Pass'] for r in results) else 'FAIL', 'Results': results}
    sys.stdout.buffer.write((json.dumps(summary, indent=2, ensure_ascii=True) + '\n').encode('ascii'))
    return 0 if summary['Result'] == 'PASS' else 1


if __name__ == '__main__':
    sys.exit(main())
