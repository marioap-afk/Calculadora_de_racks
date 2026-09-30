"""I-52 CT-21D - reproducible runner of the BA-03 V4 authority-comparison checks.

Usage (from the repository root of the branch):
    python docs/automation/evidence/I-52-ct21d-ba03-v4-checks.py <output.json>

The output argument is MANDATORY. The runner has no default target, so a run never overwrites the committed
evidence by omission. The only file it writes is <output.json>. It reads the specification
`I-52-ct21d-ba03-v4-checks.json` next to this file and git objects (read-only git commands); it modifies no
other file. Design artifact only: no host, no product code.

For each source of the specification the runner:
  1. resolves the source identity (the git blob of the path at the branch HEAD or at origin/main, a named blob,
     or the SHA-256 of one LF-normalized section of a file at the branch HEAD) and compares it with the EXPECTED
     identity published in the specification (`expected_blob`, or `expected_section_sha256` for a section). The
     expected identities are the identity set of BA-03 V4 section 2. Any difference, or an identity that cannot
     be resolved, is an IDENTITY DRIFT.
For each check it:
  2. evaluates the check on the LF-normalized text of its source, restricted to its scope when the check has
     one. A scope is the text from the first occurrence of `scope_start` up to (not including) the next
     occurrence of `scope_end`. If either marker is absent from the text, the scope is EMPTY: a scoped check
     fails closed and can never match outside its scope;
  3. evaluates the POSITIVE control (must satisfy the predicate), the NEGATIVE control (must not) and every
     NEAR-MISS control (must not), so that a vacuous or non-discriminating pattern is detected.

The output records the SHA-256 (of the LF-normalized bytes, lowercase hex) of the specification and of this
runner, so a reader can check that the executed files are the committed ones.

Exit status: 0 only if every check and every control is correct AND no identity drifted; 1 otherwise;
2 on a usage error (no output argument).
"""
import datetime
import hashlib
import json
import os
import re
import subprocess
import sys

HERE = os.path.dirname(os.path.abspath(__file__))
SPEC_NAME = 'I-52-ct21d-ba03-v4-checks.json'


def git(*a):
    return subprocess.check_output(('git',) + a).decode('utf-8').strip()


def try_git(*a):
    try:
        return subprocess.check_output(('git',) + a, stderr=subprocess.DEVNULL).decode('utf-8').strip()
    except subprocess.CalledProcessError:
        return None


def text_of(blob):
    return subprocess.check_output(('git', 'cat-file', '-p', blob)).decode('utf-8').replace('\r\n', '\n')


def file_sha256(path):
    with open(path, 'rb') as f:
        data = f.read()
    return hashlib.sha256(data.replace(b'\r\n', b'\n')).hexdigest()


def repo_path(path, top):
    rel = os.path.relpath(os.path.abspath(path), os.path.abspath(top))
    return rel.replace(os.sep, '/')


def section(text, heading):
    lines = text.split('\n')
    start = next((i for i, l in enumerate(lines) if l.strip() == heading), None)
    if start is None:
        return None
    end = next((j for j in range(start + 1, len(lines)) if lines[j].startswith('## ')), len(lines))
    return '\n'.join(lines[start:end]).rstrip('\n') + '\n'


def scope(text, check):
    has_start = 'scope_start' in check
    has_end = 'scope_end' in check
    if not has_start and not has_end:
        return text
    if has_start != has_end:
        raise ValueError('check %s: a scope needs both scope_start and scope_end' % check['id'])
    a = text.find(check['scope_start'])
    if a < 0:
        return ''
    b = text.find(check['scope_end'], a + len(check['scope_start']))
    if b < 0:
        return ''
    return text[a:b]


def evaluate(check, text):
    t = scope(text, check)
    kind = check['kind']
    flags = re.S if 'S' in check.get('flags', '') else 0
    flags |= re.I if 'I' in check.get('flags', '') else 0
    if kind == 'substring':
        return check['pattern'] in t
    if kind == 'regex':
        return re.search(check['pattern'], t, flags) is not None
    if kind == 'regex_count':
        return len(re.findall(check['pattern'], t, flags)) == check['expected_count']
    raise ValueError(kind)


def resolve(key, src, head, main_sha):
    """Returns (identity record, text, equal)."""
    where = src['where']
    if where == 'BLOB':
        expected = src['expected_blob']
        actual = expected if try_git('cat-file', '-t', expected) == 'blob' else None
        ident = {'path': src.get('path', '(blob)'), 'expectedBlob': expected, 'blob': actual}
        return ident, (text_of(actual) if actual else ''), actual == expected
    if where in ('BRANCH', 'MAIN'):
        ref = head if where == 'BRANCH' else main_sha
        expected = src['expected_blob']
        actual = try_git('rev-parse', '--verify', '--quiet', ref + ':' + src['path'])
        ident = {'path': src['path'], 'expectedBlob': expected, 'blob': actual}
        return ident, (text_of(actual) if actual else ''), actual == expected
    if where == 'SECTION':
        expected = src['expected_section_sha256']
        blob = try_git('rev-parse', '--verify', '--quiet', head + ':' + src['path'])
        sec = section(text_of(blob), src['heading']) if blob else None
        actual = hashlib.sha256(sec.encode('utf-8')).hexdigest() if sec is not None else None
        ident = {'path': src['path'], 'section': src['heading'], 'expectedSectionSha256': expected, 'sectionSha256': actual}
        return ident, (sec if sec is not None else ''), actual == expected
    raise ValueError(where)


def main(argv):
    if len(argv) != 2 or not argv[1].strip():
        sys.stderr.write('usage: python docs/automation/evidence/I-52-ct21d-ba03-v4-checks.py <output.json>\n'
                         '(the output argument is mandatory; the runner has no default target)\n')
        return 2
    target = argv[1]
    spec_path = os.path.join(HERE, SPEC_NAME)
    runner_path = os.path.abspath(__file__)
    with open(spec_path, encoding='utf-8') as f:
        spec = json.load(f)
    top = git('rev-parse', '--show-toplevel')
    head = git('rev-parse', 'HEAD')
    main_sha = git('rev-parse', 'origin/main')
    ids = {}
    texts = {}
    drift = []
    for key, src in spec['sources'].items():
        ident, text, equal = resolve(key, src, head, main_sha)
        ident['equal'] = equal
        ids[key] = ident
        texts[key] = text
        if not equal:
            drift.append(key)
    results = []
    failed = []
    for c in spec['checks']:
        r = evaluate(c, texts[c['source']])
        pos = evaluate(c, c['positive_control'])
        neg = evaluate(c, c['negative_control'])
        near = [evaluate(c, t) for t in c.get('near_miss_controls', [])]
        ok = r and pos and not neg and not any(near)
        results.append({'id': c['id'], 'source': c['source'], 'result': r, 'positiveControl': pos, 'negativeControl': neg,
                        'nearMissControls': near, 'ok': ok})
        if not ok:
            failed.append(c['id'])
    out = {'runUtc': datetime.datetime.now(datetime.timezone.utc).strftime('%Y-%m-%dT%H:%M:%SZ'),
           'specification': {'path': repo_path(spec_path, top), 'sha256': file_sha256(spec_path)},
           'runner': {'path': repo_path(runner_path, top), 'sha256': file_sha256(runner_path)},
           'informative': {'branchHead': head, 'originMain': main_sha},
           'identities': ids, 'identityDrift': drift, 'checks': results, 'failed': failed}
    with open(target, 'w', encoding='utf-8', newline='\n') as f:
        f.write(json.dumps(out, indent=1, ensure_ascii=False) + '\n')
    print('checks', len(results), 'failed', failed, 'identityDrift', drift)
    return 1 if (failed or drift) else 0


if __name__ == '__main__':
    sys.exit(main(sys.argv))
