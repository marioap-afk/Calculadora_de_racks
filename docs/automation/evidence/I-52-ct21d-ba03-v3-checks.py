"""I-52 CT-21D - reproducible runner of the BA-03 V3 authority-comparison checks.

Usage (from the repository root of the branch):  python docs/automation/evidence/I-52-ct21d-ba03-v3-checks.py [output.json]

Reads the check definitions from `I-52-ct21d-ba03-v3-checks.json` next to this file. Every pattern in that file is
the EXACT executed pattern (JSON escapes only; no display transformation). For each check the runner:
  1. resolves the source identity (path and git blob, or a section hash) at the current branch HEAD or origin/main;
  2. evaluates the check on the LF-normalized text;
  3. evaluates the check's POSITIVE control (must satisfy the predicate) and NEGATIVE control (must not), so that
     a vacuous pattern is detected.
It exits with a non-zero status if any check or control fails, and writes the results as JSON. Read-only: no
file of the repository is modified. Design artifact only: no host, no product code.
"""
import datetime
import hashlib
import json
import os
import re
import subprocess
import sys

HERE = os.path.dirname(os.path.abspath(__file__))


def git(*a):
    return subprocess.check_output(('git',) + a).decode('utf-8').strip()


def text_of(blob):
    return subprocess.check_output(('git', 'cat-file', '-p', blob)).decode('utf-8').replace('\r\n', '\n')


def section(text, heading):
    lines = text.split('\n')
    start = next(i for i, l in enumerate(lines) if l.strip() == heading)
    end = next((j for j in range(start + 1, len(lines)) if lines[j].startswith('## ')), len(lines))
    return '\n'.join(lines[start:end]).rstrip('\n') + '\n'


def scope(text, check):
    if 'scope_start' not in check:
        return text
    a = text.find(check['scope_start'])
    if a < 0:
        return ''
    b = text.find(check['scope_end'], a + len(check['scope_start']))
    return text[a:b] if b >= 0 else text[a:]


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


def main():
    spec = json.load(open(os.path.join(HERE, 'I-52-ct21d-ba03-v3-checks.json'), encoding='utf-8'))
    head = git('rev-parse', 'HEAD')
    main_sha = git('rev-parse', 'origin/main')
    ids = {}
    texts = {}
    for key, src in spec['sources'].items():
        if src['where'] == 'BLOB':
            blob = src['blob']
            texts[key] = text_of(blob)
            ids[key] = {'path': src.get('path', '(blob)'), 'blob': blob}
        elif src['where'] in ('BRANCH', 'MAIN'):
            ref = head if src['where'] == 'BRANCH' else main_sha
            blob = git('rev-parse', ref + ':' + src['path'])
            texts[key] = text_of(blob)
            ids[key] = {'path': src['path'], 'blob': blob}
        elif src['where'] == 'SECTION':
            blob = git('rev-parse', head + ':' + src['path'])
            sec = section(text_of(blob), src['heading'])
            texts[key] = sec
            ids[key] = {'path': src['path'], 'section': src['heading'], 'sectionSha256': hashlib.sha256(sec.encode('utf-8')).hexdigest()}
        else:
            raise ValueError(src['where'])
    results = []
    failed = []
    for c in spec['checks']:
        r = evaluate(c, texts[c['source']])
        pos = evaluate(c, c['positive_control'])
        neg = evaluate(c, c['negative_control'])
        ok = r and pos and not neg
        results.append({'id': c['id'], 'source': c['source'], 'result': r, 'positiveControl': pos, 'negativeControl': neg, 'ok': ok})
        if not ok:
            failed.append(c['id'])
    out = {'runUtc': datetime.datetime.now(datetime.timezone.utc).strftime('%Y-%m-%dT%H:%M:%SZ'),
           'informative': {'branchHead': head, 'originMain': main_sha},
           'identities': ids, 'checks': results, 'failed': failed}
    target = sys.argv[1] if len(sys.argv) > 1 else os.path.join(HERE, 'I-52-ct21d-ba03-v3-checks-output.json')
    with open(target, 'w', encoding='utf-8', newline='\n') as f:
        f.write(json.dumps(out, indent=1, ensure_ascii=False) + '\n')
    print('checks', len(results), 'failed', failed)
    return 1 if failed else 0


if __name__ == '__main__':
    sys.exit(main())
