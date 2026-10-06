# OD-2b-PROBE: value-free structural view of ~/.codex/config.toml (P-01 / README 13.4).
# Emits: SHA-256, length, the [projects.<path>] sections whose path is under D:\r62-fixture (path + KEY NAMES only),
# the count of other project sections (redacted), and "remainder" hashes: the SHA-256 of the file with selected fixture
# sections removed, so a remainder equal to an earlier fingerprint proves byte-for-byte that nothing else changed.
# Never emits a value.
import hashlib, json, os, re, sys

FIXTURE_PREFIX = 'd:' + chr(92) + 'r62-fixture' + chr(92)
CFG = os.path.join(os.environ['USERPROFILE'], '.codex', 'config.toml')
HEADER = re.compile(r'^\s*\[(\[?)\s*(.+?)\s*\]?\]\s*(#.*)?$')
KEY = re.compile(r'^\s*([A-Za-z0-9_\-]+|"[^"]*"|\'[^\']*\')\s*=')


def parse_project_path(name):
    # name like: projects.'D:\x\y'  or  projects."D:\\x\\y"
    if not name.startswith('projects.'):
        return None
    rest = name[len('projects.'):].strip()
    if rest.startswith("'") and rest.endswith("'"):
        return rest[1:-1]
    if rest.startswith('"') and rest.endswith('"'):
        return json.loads(rest)
    return rest


def load():
    data = open(CFG, 'rb').read()
    lines = data.decode('utf-8').splitlines(keepends=True)
    sections = []  # (start, end_exclusive, name)
    cur = None
    for i, ln in enumerate(lines):
        m = HEADER.match(ln.rstrip('\r\n'))
        if m and not ln.lstrip().startswith('#'):
            if cur:
                sections.append((cur[0], i, cur[1]))
            cur = (i, m.group(2))
    if cur:
        sections.append((cur[0], len(lines), cur[1]))
    return data, lines, sections


def is_fixture(path):
    return path is not None and path.replace('/', chr(92)).lower().startswith(FIXTURE_PREFIX)


def main():
    refs = sys.argv[1:]  # reference hashes to test remainders against
    data, lines, sections = load()
    fx, other_projects = [], 0
    for (s, e, name) in sections:
        p = parse_project_path(name)
        if p is None:
            continue
        if is_fixture(p):
            keys = []
            for ln in lines[s + 1:e]:
                m = KEY.match(ln)
                if m and not ln.lstrip().startswith('#'):
                    keys.append(m.group(1))
            fx.append({'path': p, 'header_line': s + 1, 'key_names': keys, 'span': [s, e]})
        else:
            other_projects += 1
    # remainder hashes: every non-empty subset of fixture sections removed (fx is small), three blank-line variants
    remainders = []
    n = len(fx)
    for mask in range(1, 1 << n):
        chosen = [fx[i] for i in range(n) if mask >> i & 1]
        for variant in ('section_only', 'plus_preceding_blank', 'plus_trailing_blank'):
            drop = set()
            for c in chosen:
                s, e = c['span']
                body_end = e
                # trailing blank lines inside the span belong to the gap before the next header
                while body_end - 1 > s and lines[body_end - 1].strip() == '':
                    body_end -= 1
                rng = list(range(s, body_end))
                if variant == 'plus_preceding_blank' and s > 0 and lines[s - 1].strip() == '':
                    rng.append(s - 1)
                if variant == 'plus_trailing_blank' and body_end < e:
                    rng.append(body_end)
                drop.update(rng)
            rem = ''.join(l for i, l in enumerate(lines) if i not in drop).encode('utf-8')
            h = hashlib.sha256(rem).hexdigest().upper()
            remainders.append({'removed': [c['path'] for c in chosen], 'variant': variant, 'sha256': h,
                               'matches': [r for r in refs if r.upper() == h]})
    out = {
        'Path': '~/.codex/config.toml',
        'Sha256': hashlib.sha256(data).hexdigest().upper(),
        'Length': len(data),
        'FixtureProjectSections': [{k: v for k, v in f.items() if k != 'span'} for f in fx],
        'OtherProjectSections': other_projects,
        'TotalSections': len(sections),
        'Remainders': [r for r in remainders if r['matches']] or 'none-match',
        'RemainderCandidatesTested': len(remainders),
    }
    print(json.dumps(out, ensure_ascii=False, indent=2))


main()
