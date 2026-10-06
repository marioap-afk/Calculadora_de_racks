# Pre-publication audit of the fixture repository as GitHub holds it (mirror clone): every object reachable from every ref,
# commit and tag metadata/messages, and every blob, against secret / credential / token / private-path / host-metadata /
# transcript / config-value patterns. Prints findings with object id, path(s) and the matched rule; never prints a match
# longer than 80 characters.
import json, os, re, subprocess, sys

BS = chr(92)
MIRROR = sys.argv[1]
OUT = sys.argv[2]


def git(*a, binary=False):
    r = subprocess.run(['git', '-C', MIRROR] + list(a), capture_output=True)
    if r.returncode:
        raise SystemExit(r.stderr.decode('utf-8', 'replace'))
    return r.stdout if binary else r.stdout.decode('utf-8', 'replace')


user = os.environ.get('USERNAME', '')
host = os.environ.get('COMPUTERNAME', '')
RULES = [
    ('private-key', r'-----BEGIN [A-Z ]*PRIVATE KEY-----'),
    ('github-token', r'\b(gh[pousr]_[A-Za-z0-9]{20,}|github_pat_[A-Za-z0-9_]{20,})'),
    ('openai-key', r'\bsk-(proj-)?[A-Za-z0-9_\-]{20,}'),
    ('anthropic-key', r'\bsk-ant-[A-Za-z0-9_\-]{10,}'),
    ('aws-key', r'\b(AKIA|ASIA)[A-Z0-9]{16}\b'),
    ('slack-token', r'\bxox[abposr]-[A-Za-z0-9\-]{10,}'),
    ('jwt', r'\beyJ[A-Za-z0-9_\-]{10,}\.[A-Za-z0-9_\-]{10,}\.[A-Za-z0-9_\-]{10,}'),
    ('bearer', r'(?i)\b(authorization|bearer)\s*[:=]\s*\S{8,}'),
    ('assignment-secret', r'(?i)\b(password|passwd|pwd|secret|api[_-]?key|access[_-]?token|auth[_-]?token|client[_-]?secret)\b\s*[:=]\s*["\']?[^\s"\'<>{}]{6,}'),
    ('temp-clone-token', r'(?i)temp_clone_token'),
    ('windows-user-path', r'(?i)[A-Za-z]:' + re.escape(BS) + r'{1,2}Users' + re.escape(BS) + r'{1,2}'),
    ('posix-user-path', r'(?i)/(c|d)/Users/|/home/[a-z]|/Users/[A-Za-z]'),
    ('short-name-path', r'(?i)~1' + re.escape(BS)),
    ('appdata', r'(?i)AppData' + re.escape(BS) + '|AppData/|%LOCALAPPDATA%|%USERPROFILE%|%APPDATA%'),
    ('codex-home', r'(?i)\.codex[' + re.escape(BS) + r'/](config|auth|sessions|worktrees)|~/\.codex'),
    ('claude-home', r'(?i)\.claude[' + re.escape(BS) + r'/](projects|settings|credentials)|~/\.claude'),
    ('transcript', r'(?i)rollout-20\d\d-|turn_context|"thread_id"|\.jsonl\b|pasted_content|session_meta'),
    ('config-toml', r'(?i)config\.toml|trust_level|model_reasoning_effort\s*=|windows\.sandbox|\[projects\.'),
    ('email', r'\b[A-Za-z0-9._%+\-]+@[A-Za-z0-9.\-]+\.[A-Za-z]{2,}\b'),
    ('ipv4', r'\b(?:(?:25[0-5]|2[0-4]\d|1?\d?\d)\.){3}(?:25[0-5]|2[0-4]\d|1?\d?\d)\b'),
]
# Owner-specific patterns (Windows user name, its 8.3 short name and the owner's e-mail) are supplied at run time and are not
# published: AUDIT_PRIVATE_PATTERNS = JSON list of regexes. The run of 2026-10-06T15:48Z used them as literals.
for _p in json.loads(os.environ.get('AUDIT_PRIVATE_PATTERNS', '[]')):
    RULES.append(('owner-identity', _p))
if user:
    RULES.append(('env-username', r'(?i)\b' + re.escape(user) + r'\b'))
if host:
    RULES.append(('env-hostname', r'(?i)\b' + re.escape(host) + r'\b'))
CRX = [(n, re.compile(p)) for n, p in RULES]
EMAIL_OK = {'fixture@example.invalid', 'noreply@anthropic.com'}

objs = [l.split(' ', 1) for l in git('rev-list', '--all', '--objects').splitlines()]
paths = {}
for o in objs:
    paths.setdefault(o[0], set())
    if len(o) > 1:
        paths[o[0]].add(o[1])
findings, counts = [], {'commit': 0, 'tag': 0, 'tree': 0, 'blob': 0}
binary_blobs = []
for oid in paths:
    t = git('cat-file', '-t', oid).strip()
    counts[t] = counts.get(t, 0) + 1
    if t == 'tree':
        continue
    data = git('cat-file', '-p', oid, binary=True)
    if b'\x00' in data[:8000]:
        binary_blobs.append({'oid': oid, 'paths': sorted(paths[oid]), 'size': len(data)})
        continue
    text = data.decode('utf-8', 'replace')
    for name, rx in CRX:
        for m in rx.finditer(text):
            s = m.group(0)
            if name == 'email' and s.lower() in EMAIL_OK:
                continue
            line_no = text.count('\n', 0, m.start()) + 1
            findings.append({'rule': name, 'type': t, 'oid': oid, 'paths': sorted(paths[oid]), 'line': line_no,
                             'match': s[:80]})
summary = {}
for f in findings:
    summary[f['rule']] = summary.get(f['rule'], 0) + 1
json.dump({'mirror': 'D:/r62-fixture/audit-mirror.git', 'refs': git('for-each-ref', '--format=%(objectname) %(refname)').splitlines(),
           'object_counts': counts, 'binary_blobs': binary_blobs, 'rules': [n for n, _ in RULES if not n.startswith('env-')] + ['env-username', 'env-hostname'],
           'findings_by_rule': summary, 'findings': findings},
          open(OUT, 'w', encoding='utf-8'), ensure_ascii=False, indent=1)
print(json.dumps({'object_counts': counts, 'binary_blobs': len(binary_blobs), 'findings_by_rule': summary}, ensure_ascii=False))
for f in findings:
    print(f['rule'], f['type'], f['oid'][:8], ','.join(f['paths'])[:70], 'L%d' % f['line'], repr(f['match']))
