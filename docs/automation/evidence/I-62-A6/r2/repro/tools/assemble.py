# assemble.py - builds the deliverable repro-<pkg>.json from the comparison output plus the run records.
# python -I assemble.py <pkg> <cmp json> <out json> <meta json>
# <meta json> is written by hand-free tooling below: commands, timings, script diffs, guards, helper outputs.
import hashlib, json, os, re, sys

sys.dont_write_bytecode = True
_EMAIL = re.compile(r'[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,}')


def forms():
    prof = os.environ.get('USERPROFILE') or ''
    name = os.path.basename(prof.rstrip('\\/'))
    core = ''.join(c for c in name if c not in ' .').upper()
    f = []
    if len(prof) > 3:
        f += [(prof, '%USERPROFILE%'), (prof.replace('\\', '/'), '%USERPROFILE%')]
    if len(name) > 2:
        f += [(name, '<usuario>'), (core[:6] + '~1', '<usuario>')]
    return f


F = forms()


def san(o):
    if isinstance(o, dict):
        return dict((san(k), san(v)) for k, v in o.items())
    if isinstance(o, list):
        return [san(x) for x in o]
    if isinstance(o, str):
        for a, b in F:
            o = re.sub(re.escape(a), b, o, flags=re.I)
        return _EMAIL.sub('<email>', o)
    return o


def main():
    pkg, cmp_path, out, meta_path = sys.argv[1:5]
    cmp = json.load(open(cmp_path, encoding='ascii'))
    meta = json.load(open(meta_path, encoding='utf-8'))
    per_file = []
    for f in cmp['Files']:
        per_file.append({'File': f['File'], 'Status': f['Status'], 'ByteIdentical': f['ByteIdentical'],
                         'CustodiedSha256': f['CustodiedSha256'], 'ReproSha256': f['ReproSha256'],
                         'DifferenceCount': len(f.get('Differences', [])), 'ClassCounts': f.get('ClassCounts', {}),
                         'Failures': f.get('Failures', []), 'Differences': f.get('Differences', [])})
    res = {'Schema': 'r62-a6r2-repro/' + pkg + '/1', 'Package': pkg, 'Verdict': cmp['Verdict'],
           'Summary': dict((f['File'], f['Status']) for f in per_file),
           'FailureCount': sum(len(f['Failures']) for f in per_file),
           'Method': {
               'Comparison': 'every leaf of the custodied JSON against the same leaf of the reproduced JSON (tools/compare.py); '
                             'keys present on one side only and list-length changes are failures unless inside a host process table',
               'ExpectedClasses': {
                   'PATH': 'the strings are equal once the work root (original D:/r62-a6r2/%s, repro D:/r62-a6r2/repro/%s) and the results folder (ev/%s vs ev/repro/%s) are written as one token' % (pkg, pkg, pkg, pkg),
                   'TIMESTAMP': 'equal once ISO-8601 UTC timestamps are written as one token (PATH+TIMESTAMP: both)',
                   'PID': 'integer under Pid, ParentPid, CheckerPid or LaunchedPid',
                   'DURATION': 'number under Seconds',
                   'HOST_PROCESS_TABLE': 'Total and UnreadableClosedList of a Win32_Process query result (other processes of the host; never the Matching or ChildrenOfGivenParents lists, which are compared strictly)',
                   'PATH_TRUNCATION': 'a capped string (same length on both sides) whose cut point moved because the repro root is 6 characters longer; one normalized string is a prefix of the other',
                   'DIGEST_VERIFIED': 'a differing SHA-1/SHA-256 shown to be the digest of content that differs only by expected classes (see VerificationMethods)',
                   'STRING_WITH_VERIFIED_DIGESTS': 'a string equal after PATH/TIMESTAMP normalization except for embedded digests, each one DIGEST_VERIFIED',
                   'DERIVED_FROM_COMPARED_FILE': 'manifest digest/size of a result file whose own comparison has only expected differences'},
               'VerificationMethods': {
                   'V1': 'substitution: the repro bytes (or tool Facts) with the repro work root written as the original root, and embedded digests mapped recursively, hash to the custodied value',
                   'V2': 'counterpart file: the custodied digest is the SHA-256 of the original work-root file at the same relative path as the repro file, and the two files differ only by expected classes (JSON / JSONL / base64 envelopes compared field by field). The original work root is only READ',
                   'V3': 'commit object: both commits (original and repro root) have the same tree, parents, identities and message; only author/committer dates differ',
                   'V4': 'git state (IndexSha256, GitFilesSha256 only): the final git dirs of every repository pair differ only in index stat data (git ls-files -s identical), reflog dates or worktree path pointers (see WorkRootEquivalence)',
                   'V5': 'recomputed listing (r4-t2 WorktreeFilesSha256 only): recomputed now, equal to the recorded value, path-bearing entry rewritten'},
               'NegativeControl': meta.get('NegativeControl')},
           'Environment': meta['Environment'], 'Commands': meta['Commands'], 'ScriptChanges': meta['ScriptChanges'],
           'Guards': meta['Guards'], 'VerificationHelpers': meta.get('VerificationHelpers'),
           'WorkRootEquivalence': cmp['WorkRootEquivalence'], 'Files': per_file}
    data = (json.dumps(san(res), ensure_ascii=True, sort_keys=True, indent=1) + '\n').encode('ascii')
    with open(out, 'wb') as fh:
        fh.write(data)
    print(out, hashlib.sha256(data).hexdigest(), res['Verdict'], res['FailureCount'])


if __name__ == '__main__':
    main()
