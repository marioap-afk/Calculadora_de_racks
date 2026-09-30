"""I-52 CT-21D - dedicated precondition verifier of AR7-03 (decisions section 226).

Execution-preparation tooling OUTSIDE the hash registry: it has no BA-11 row and no graph edge. It is under the
custody of BA-07, and the BA-07 section 5.4 registry cites its output (AR7-03, AR7-05). It is the detector of the
AR5-12 run-preparation precondition for the UNION of the (path, blob) pairs cited by BA-03, BA-06 and BA-08.
Reading of AR5-12 (AR7-03, no erratum): the BA-03 runner detects BA-03's own pairs; this verifier detects the
union. Following BA03V6-N1, the BA-03 pairs are compared here as expected blob versus the blob at the build source
SHA (and at the bound SHA); origin/main is not read (that is a seal-time question, not a per-build one).

EXTENSION OF THE AR7-03 UNION, SUBMITTED TO THE ARCHITECT FOR CONFIRMATION. AR7-03 names the pairs of BA-03, BA-06
and BA-08 only. This verifier also reads a fourth source, the W-1 dependency pairs of BA-09 V7 section 3.2 (option
--ba09), because BA-09 V7 ties its warm-up plans to each build under test through this verifier (AR7-07; the W-1
identity clause of BA-09 V7 section 3.2 and its closing block rely on it). Until the Architect confirms the
extension, the BA-09 rows are part of the union as a proposal of round V7, not as a ruled part of AR7-03.

Usage:
    python I-52-ct21d-precondition-verifier.py <repo root> <build source sha> <output.json>
           [--bound <sha>] [--ba03 <path>] [--ba06 <path>] [--ba08 <path>] [--ba09 <path>]
           [--expect-sha256 BA-xx=<hex>]... [--expect-count <n>] [--ba03-runner-output <path>]

  <repo root>          REQUIRED (no default): a git checkout that contains the artifacts and the objects of both
                       SHAs; a path that is not the top level of a git checkout is a usage error;
  <build source sha>   the source commit of the build under test (any revision git can resolve to a commit);
  <output.json>        MANDATORY output path; the only file this verifier writes;
  --bound              the bound product SHA (default: BOUND_PRODUCT_SHA below, decisions section 223, AR6-01);
  --ba03/--ba06/--ba08/--ba09
                       the artifact files to read (relative to <repo root> or absolute), so that a later version
                       can be pointed at without editing this file (defaults: CONFIG below);
  --expect-sha256 BA-xx=<hex>
                       repeatable; the expected SHA-256 (LF-normalized bytes, 64 lowercase hex) of the file read
                       for artifact BA-xx (the sealed or BA-11-registered hash; required by the reviewer once the
                       artifacts are sealed). A different or unreadable file FAILS the run (fail closed);
  --expect-count <n>   the union pair count the reviewer declares; a different count FAILS the run;
  --ba03-runner-output <path>
                       the output JSON of the BA-03 runner (I-52-ct21d-ba03-v5-checks.py) run with a build source
                       SHA; it is recorded with its SHA-256, and its resolved build source SHA must equal the build
                       source SHA of this run (an unreadable file, a run without a build source SHA, or another SHA
                       FAILS the run). Its own verdict is recorded, not re-judged.

Pair lists (declared config, CONFIG below):
  BA-03: the sources of the BA-03 checks specification whose `where` is BOUND (path, expected_blob);
         the specification's own `bound_product_sha` must equal the bound SHA.
  BA-06: the identity table of section 2.1; only the rows whose Role starts with "route" (the precedent, residual
         source and quantity source rows are outside the AR5-12 set); the blob is the column headed with the
         bound SHA.
  BA-08: the code-identity table of section 3.2; only the W and C rows (the P rows are outside the set); the path
         column is "under `src/`", so `src/` is prefixed; the blob is the column headed with the bound SHA.
  BA-09: the identity table of section 3.2 (the W-1 dependency pairs); only the rows with Role W-1 (no role is
         declared excluded, so any other role is a parse gap); the object id is the column headed with the bound
         SHA. Its pairs may be TREE ids (a directory bound by its tree); the other sources admit blob ids only.
A row whose role is neither selected nor declared excluded, a selected row whose path or 40-hex blob cannot be
read, a missing section or table, a table with a cell count different from its header, a missing or ambiguous
column, and a path cited with two different blobs are PARSE GAPS or CONFLICTS; each fails the run (fail closed).

Resolution (read-only git: rev-parse, ls-tree): every pair of the union is resolved at the bound SHA and at the
build source SHA. Per resolution the status is EQUAL (object id equals the cited blob), DIFFERENT (the path
exists with another object id, or with an object type the citing artifact does not admit) or ABSENT (the path
does not exist at that commit). A pair is EQUAL only when both resolutions are EQUAL; a resolution at the bound
SHA other than EQUAL means the cited artifact is wrong.

Exit status: 0 only if every pair is EQUAL at both SHAs, there is no parse gap or conflict, both SHAs resolve to
commits, and every declared expectation (--expect-sha256, --expect-count, --ba03-runner-output) holds; 1 otherwise;
2 on a usage error. Design artifact only: no host, no product code, no git writes.
"""
import datetime
import hashlib
import json
import os
import re
import subprocess
import sys

TOOL = 'I-52-ct21d-precondition-verifier'
VERSION = 'V7 (decisions section 226, AR7-03)'
BOUND_PRODUCT_SHA = '95690c28dc6268e61dff32a0cbc33cc9fde3d47f'
HEX40 = re.compile(r'^[0-9a-f]{40}$')

# Declared configuration: which file and which table of each citing artifact holds its precondition pairs.
CONFIG = {
    'BA-03': {
        'kind': 'json-spec',
        'default_path': 'docs/automation/evidence/I-52-ct21d-ba03-v5-checks.json',
        'select': 'sources whose where == BOUND',
    },
    'BA-06': {
        'kind': 'markdown-table',
        'default_path': 'docs/initiatives/I-52-ct21d-baseline-ba-06-evm-preparation-v7.md',
        'heading': r'^###\s+2\.1\s',
        'role_selected': r'^route\b',
        'role_excluded': r'^(precedent|residual source|quantity source)\b',
        'select': 'section 2.1 identity-table rows whose Role starts with "route"',
    },
    'BA-08': {
        'kind': 'markdown-table',
        'default_path': 'docs/initiatives/I-52-ct21d-baseline-ba-08-selective-kind-baseline-v7.md',
        'heading': r'^###\s+3\.2\s',
        'role_selected': r'^(W|C)$',
        'role_excluded': r'^P$',
        'select': 'section 3.2 code-identity rows with Role W or C',
    },
    # Extension of the AR7-03 union submitted to the Architect for confirmation (module docstring).
    'BA-09': {
        'kind': 'markdown-table',
        'default_path': 'docs/initiatives/I-52-ct21d-baseline-ba-09-warmup-v7.md',
        'heading': r'^###\s+3\.2\s',
        'role_selected': r'^W-1$',
        'role_excluded': None,
        'object_types': ('blob', 'tree'),
        'select': 'section 3.2 W-1 dependency rows (Role W-1; blob or tree ids)',
    },
}
DEFAULT_OBJECT_TYPES = ('blob',)
HEX64 = re.compile(r'^[0-9a-f]{64}$')
PATH_OPTIONS = ('--bound', '--ba03', '--ba06', '--ba08', '--ba09', '--expect-count', '--ba03-runner-output')


def usage(msg=None):
    if msg:
        sys.stderr.write('usage error: %s\n' % msg)
    sys.stderr.write(__doc__.split('Pair lists')[0])
    sys.exit(2)


def parse_args(argv):
    pos, opts, expect = [], {}, {}
    i = 0
    while i < len(argv):
        a = argv[i]
        if a in PATH_OPTIONS or a == '--expect-sha256':
            if i + 1 >= len(argv):
                usage('%s needs a value' % a)
            v = argv[i + 1]
            if a == '--expect-sha256':
                name, sep, hx = v.partition('=')
                if not sep or name not in CONFIG or not HEX64.match(hx) or name in expect:
                    usage('--expect-sha256 needs BA-xx=<64 lowercase hex>, BA-xx one of %s, each at most once' % ', '.join(CONFIG))
                expect[name] = hx
            elif a == '--expect-count':
                if not re.match(r'^[0-9]+$', v):
                    usage('--expect-count needs a non-negative integer')
                opts['expect-count'] = int(v)
            else:
                if a[2:] in opts:
                    usage('%s given twice' % a)
                opts[a[2:]] = v
            i += 2
        elif a.startswith('--'):
            usage('unknown option %s' % a)
        else:
            pos.append(a)
            i += 1
    if len(pos) != 3:
        usage('three positional arguments are required: <repo root> <build source sha> <output.json>')
    root = pos[0]
    p = subprocess.run(('git', '-C', root, 'rev-parse', '--show-toplevel'),
                       stdout=subprocess.PIPE, stderr=subprocess.DEVNULL) if os.path.isdir(root) else None
    top = p.stdout.decode('utf-8').strip() if p is not None and p.returncode == 0 else ''
    norm = lambda s: os.path.normcase(os.path.realpath(s))
    if not top or norm(top) != norm(root):
        usage('<repo root> %r is not the top level of a git checkout' % root)
    return pos, opts, expect


class Git:
    def __init__(self, root):
        self.root = root

    def run(self, *a):
        p = subprocess.run(('git', '-C', self.root, '-c', 'core.quotepath=off') + a,
                           stdout=subprocess.PIPE, stderr=subprocess.DEVNULL)
        return p.returncode, p.stdout

    def commit(self, rev):
        rc, out = self.run('rev-parse', '--verify', '--quiet', rev + '^{commit}')
        s = out.decode('utf-8').strip()
        return s if rc == 0 and HEX40.match(s) else None

    def entry(self, sha, path):
        """(type, oid) of path at commit sha, or None when absent."""
        rc, out = self.run('ls-tree', '-z', sha, '--', path)
        if rc != 0:
            return ('ERROR', None)
        for rec in out.split(b'\0'):
            if not rec:
                continue
            meta, _, name = rec.partition(b'\t')
            if name.decode('utf-8') == path:
                parts = meta.decode('utf-8').split()
                return (parts[1], parts[2])
        return None


def lf_sha256(data):
    return hashlib.sha256(data.replace(b'\r\n', b'\n')).hexdigest()


def strip_ticks(s):
    return s.strip().replace('`', '').strip()


def split_row(line):
    s = line.strip()
    if not (s.startswith('|') and s.endswith('|')):
        return None
    return [c.strip() for c in s[1:-1].split('|')]


def is_separator(cells):
    return all(re.match(r'^:?-{3,}:?$', c) for c in cells)


def extract_pairs_json(name, text, bound, gaps):
    pairs, excluded = [], []
    try:
        spec = json.loads(text)
    except ValueError as e:
        gaps.append({'artifact': name, 'gap': 'specification is not valid JSON: %s' % e})
        return pairs, excluded
    sb = spec.get('bound_product_sha')
    if sb != bound:
        gaps.append({'artifact': name, 'gap': 'bound_product_sha of the specification (%s) differs from the bound SHA (%s)' % (sb, bound)})
    sources = spec.get('sources')
    if not isinstance(sources, dict) or not sources:
        gaps.append({'artifact': name, 'gap': 'no sources object'})
        return pairs, excluded
    for key, src in sources.items():
        if not isinstance(src, dict) or 'where' not in src:
            gaps.append({'artifact': name, 'gap': 'source %s has no where' % key})
            continue
        if src['where'] != 'BOUND':
            excluded.append({'key': key, 'where': src['where'], 'reason': 'not a BOUND source'})
            continue
        path, blob = src.get('path'), src.get('expected_blob')
        if not isinstance(path, str) or not path or not isinstance(blob, str) or not HEX40.match(blob):
            gaps.append({'artifact': name, 'gap': 'BOUND source %s lacks a path or a 40-hex expected_blob' % key})
            continue
        pairs.append({'path': path, 'blob': blob, 'where': 'source %s' % key})
    if not pairs:
        gaps.append({'artifact': name, 'gap': 'no BOUND source found'})
    return pairs, excluded


def extract_pairs_md(name, cfg, text, bound, gaps):
    pairs, excluded = [], []
    lines = text.split('\n')
    heads = [i for i, l in enumerate(lines) if re.match(cfg['heading'], l)]
    if len(heads) != 1:
        gaps.append({'artifact': name, 'gap': 'heading %r found %d times (expected 1)' % (cfg['heading'], len(heads))})
        return pairs, excluded
    start = heads[0] + 1
    end = next((j for j in range(start, len(lines)) if lines[j].startswith('#')), len(lines))
    # collect the tables of the section (contiguous runs of lines starting with '|')
    tables, cur = [], None
    for j in range(start, end):
        if lines[j].lstrip().startswith('|'):
            if cur is None:
                cur = []
                tables.append((j + 1, cur))
            cur.append((j + 1, lines[j]))
        else:
            cur = None
    short = bound[:8]
    qualifying = []
    for first_line, rows in tables:
        if len(rows) < 2:
            continue
        header = split_row(rows[0][1])
        if header is None or split_row(rows[1][1]) is None or not is_separator(split_row(rows[1][1])):
            continue
        pcols = [k for k, h in enumerate(header) if h.lower().startswith('path')]
        rcols = [k for k, h in enumerate(header) if strip_ticks(h).lower() == 'role']
        bcols = [k for k, h in enumerate(header) if 'blob' in h.lower() and ('`%s' % short) in h]
        if pcols or rcols or bcols:
            qualifying.append((first_line, rows, header, pcols, rcols, bcols))
    if len(qualifying) != 1:
        gaps.append({'artifact': name, 'gap': 'section has %d candidate identity tables (expected 1)' % len(qualifying)})
        return pairs, excluded
    first_line, rows, header, pcols, rcols, bcols = qualifying[0]
    if len(pcols) != 1 or len(rcols) != 1 or len(bcols) != 1:
        gaps.append({'artifact': name, 'line': first_line,
                     'gap': 'columns not uniquely identified (path %d, role %d, blob at bound SHA %d)' % (len(pcols), len(rcols), len(bcols))})
        return pairs, excluded
    pc, rc, bc = pcols[0], rcols[0], bcols[0]
    m = re.search(r'under\s+`([^`]+)`', header[pc])
    prefix = m.group(1) if m else ''
    if prefix and not prefix.endswith('/'):
        prefix += '/'
    body = rows[2:]
    if not body:
        gaps.append({'artifact': name, 'line': first_line, 'gap': 'identity table has no rows'})
    for ln, raw in body:
        cells = split_row(raw)
        if cells is None or len(cells) != len(header):
            gaps.append({'artifact': name, 'line': ln, 'gap': 'row cell count %s differs from header %d' % (None if cells is None else len(cells), len(header))})
            continue
        role = strip_ticks(cells[rc])
        pm = re.match(r'^`([^`]+)`', cells[pc].strip())
        path = (prefix + pm.group(1).strip()) if pm else None
        if cfg['role_excluded'] is not None and re.match(cfg['role_excluded'], role):
            excluded.append({'line': ln, 'path': path, 'role': role, 'reason': 'role outside the AR5-12 set'})
            continue
        if not re.match(cfg['role_selected'], role):
            gaps.append({'artifact': name, 'line': ln, 'gap': 'role %r neither selected nor declared excluded' % role})
            continue
        blob = strip_ticks(cells[bc])
        if path is None or not HEX40.match(blob):
            gaps.append({'artifact': name, 'line': ln, 'gap': 'selected row without a backticked path or a 40-hex blob at the bound SHA (path %r, blob %r)' % (path, blob)})
            continue
        pairs.append({'path': path, 'blob': blob, 'where': 'line %d, role %s' % (ln, role)})
    if not pairs:
        gaps.append({'artifact': name, 'gap': 'no selected row'})
    return pairs, excluded


def status_of(entry, blob, types):
    if entry is None:
        return {'status': 'ABSENT', 'type': None, 'oid': None}
    typ, oid = entry
    if typ == 'ERROR':
        return {'status': 'ABSENT', 'type': None, 'oid': None, 'error': 'git ls-tree failed'}
    r = {'status': 'EQUAL' if oid == blob and typ in types else 'DIFFERENT', 'type': typ, 'oid': oid}
    if typ not in types:
        r['error'] = 'object type %s not admitted by the citing artifacts (%s)' % (typ, '/'.join(types))
    return r


def read_runner_output(root, rel, build, gaps):
    """Record the BA-03 runner output and check that it was run with this build source SHA."""
    full = rel if os.path.isabs(rel) else os.path.join(root, rel)
    rec = {'path': rel.replace('\\', '/')}
    try:
        with open(full, 'rb') as f:
            data = f.read()
    except OSError:
        rec['sha256'] = None
        gaps.append({'artifact': 'BA-03 runner output', 'gap': 'file not readable: %s' % rec['path']})
        return rec
    rec['sha256'] = lf_sha256(data)
    try:
        d = json.loads(data.decode('utf-8'))
        bs = d['buildSourceSha']
        rec['buildSourceSha'] = bs.get('resolved')
        rec['runner'] = d.get('runner')
        rec['specification'] = d.get('specification')
        rec['runnerFailedCount'] = len(d.get('failed') or [])
        rec['runnerIdentityDriftCount'] = len(d.get('identityDrift') or [])
    except (ValueError, KeyError, TypeError, AttributeError, UnicodeDecodeError) as e:
        gaps.append({'artifact': 'BA-03 runner output', 'gap': 'not a BA-03 runner output with buildSourceSha.resolved: %s' % e})
        return rec
    rec['buildSourceMatches'] = build is not None and rec['buildSourceSha'] == build
    if not rec['buildSourceMatches']:
        gaps.append({'artifact': 'BA-03 runner output',
                     'gap': 'runner build source SHA %r differs from the build source SHA of this run %r' % (rec['buildSourceSha'], build)})
    return rec


def main():
    (root, build_rev, out_path), opts, expect = parse_args(sys.argv[1:])
    git = Git(root)
    gaps, conflicts = [], []
    bound = git.commit(opts.get('bound', BOUND_PRODUCT_SHA))
    build = git.commit(build_rev)
    shas_ok = bound is not None and build is not None
    if bound is None:
        gaps.append({'artifact': None, 'gap': 'bound SHA %r does not resolve to a commit' % opts.get('bound', BOUND_PRODUCT_SHA)})
    if build is None:
        gaps.append({'artifact': None, 'gap': 'build source SHA %r does not resolve to a commit' % build_rev})

    inputs, per_artifact, union = [], {}, {}
    for name, cfg in CONFIG.items():
        rel = opts.get(name.replace('-', '').lower(), cfg['default_path'])
        full = rel if os.path.isabs(rel) else os.path.join(root, rel)
        entry = {'artifact': name, 'path': rel.replace('\\', '/'), 'select': cfg['select']}
        if name in expect:
            entry['expectedSha256'] = expect[name]
        try:
            with open(full, 'rb') as f:
                data = f.read()
        except OSError:
            entry['sha256'] = None
            if name in expect:
                entry['sha256Matches'] = False
            inputs.append(entry)
            gaps.append({'artifact': name, 'gap': 'artifact file not readable: %s' % entry['path']})
            per_artifact[name] = {'selected': 0, 'pairs': [], 'excluded': []}
            continue
        entry['sha256'] = lf_sha256(data)
        if name in expect:
            entry['sha256Matches'] = entry['sha256'] == expect[name]
            if not entry['sha256Matches']:
                gaps.append({'artifact': name, 'gap': 'sha256 %s of %s differs from the expected %s (--expect-sha256)' % (entry['sha256'], entry['path'], expect[name])})
        inputs.append(entry)
        text = data.decode('utf-8').replace('\r\n', '\n')
        if bound is None:
            pairs, excluded = [], []
        elif cfg['kind'] == 'json-spec':
            pairs, excluded = extract_pairs_json(name, text, bound, gaps)
        else:
            pairs, excluded = extract_pairs_md(name, cfg, text, bound, gaps)
        per_artifact[name] = {'selected': len(pairs), 'pairs': pairs, 'excluded': excluded}
        types = cfg.get('object_types', DEFAULT_OBJECT_TYPES)
        for p in pairs:
            u = union.setdefault(p['path'], {'path': p['path'], 'blob': p['blob'], 'citedBy': [], 'types': set(types)})
            if u['blob'] != p['blob']:
                conflicts.append({'path': p['path'], 'blobs': sorted({u['blob'], p['blob']}), 'citedBy': u['citedBy'] + ['%s (%s)' % (name, p['where'])]})
            u['types'] &= set(types)
            u['citedBy'].append('%s (%s)' % (name, p['where']))

    if 'expect-count' in opts and len(union) != opts['expect-count']:
        gaps.append({'artifact': None, 'gap': 'union count %d differs from the declared count %d (--expect-count)' % (len(union), opts['expect-count'])})
    runner_output = read_runner_output(root, opts['ba03-runner-output'], build, gaps) if 'ba03-runner-output' in opts else None

    rows = []
    for path in sorted(union):
        u = union[path]
        types = tuple(sorted(u['types']))
        r = {'path': path, 'citedBlob': u['blob'], 'citedBy': u['citedBy']}
        if shas_ok:
            r['atBound'] = status_of(git.entry(bound, path), u['blob'], types)
            r['atBuild'] = status_of(git.entry(build, path), u['blob'], types)
            sb, sx = r['atBound']['status'], r['atBuild']['status']
            r['status'] = 'EQUAL' if sb == sx == 'EQUAL' else ('ABSENT' if 'ABSENT' in (sb, sx) else 'DIFFERENT')
        else:
            r['status'] = 'NOT_RESOLVED'
        rows.append(r)

    counts = {k: sum(1 for r in rows if r['status'] == k) for k in ('EQUAL', 'DIFFERENT', 'ABSENT', 'NOT_RESOLVED')}
    ok = bool(shas_ok and not gaps and not conflicts and rows) and counts['EQUAL'] == len(rows)
    with open(os.path.abspath(__file__), 'rb') as f:
        self_sha = lf_sha256(f.read())
    result = {
        'tool': TOOL,
        'version': VERSION,
        'verifierSha256': self_sha,
        'runAtUtc': datetime.datetime.now(datetime.timezone.utc).strftime('%Y-%m-%dT%H:%M:%SZ'),
        'boundProductSha': bound,
        'buildSourceSha': build,
        'buildSourceArgument': build_rev,
        'inputs': inputs,
        'expectedCount': opts.get('expect-count'),
        'ba03RunnerOutput': runner_output,
        'perArtifact': {k: {'selected': v['selected'], 'excluded': v['excluded'], 'pairs': v['pairs']} for k, v in per_artifact.items()},
        'unionCount': len(rows),
        'counts': counts,
        'parseGaps': gaps,
        'conflicts': conflicts,
        'drifts': [{'path': r['path'], 'status': r['status'], 'citedBlob': r['citedBlob'],
                    'atBound': r.get('atBound'), 'atBuild': r.get('atBuild')} for r in rows if r['status'] != 'EQUAL'],
        'pairs': rows,
        'verdict': 'PASS' if ok else 'FAIL',
        'exitCode': 0 if ok else 1,
    }
    with open(out_path, 'w', encoding='utf-8', newline='\n') as f:
        json.dump(result, f, ensure_ascii=False, indent=1)
        f.write('\n')
    print('%s: union %d pairs; EQUAL %d, DIFFERENT %d, ABSENT %d; parse gaps %d; conflicts %d; %s' % (
        TOOL, len(rows), counts['EQUAL'], counts['DIFFERENT'], counts['ABSENT'], len(gaps), len(conflicts), result['verdict']))
    sys.exit(result['exitCode'])


if __name__ == '__main__':
    main()
