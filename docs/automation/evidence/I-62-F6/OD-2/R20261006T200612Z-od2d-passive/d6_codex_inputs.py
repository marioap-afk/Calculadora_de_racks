# D.6 pre-check of the Codex app's automatic inputs for a Principal B session (FX-04a). Value-free: paths, sizes, SHA-256, sqlite table names and
# row counts, and COUNTS of unit/fixture identifiers. Never prints file or row content.
import hashlib, json, os, sqlite3, sys
CH = os.path.join(os.environ['USERPROFILE'], '.codex')
TOKENS = ['FX-U1', 'fx/u1', 'r62-fixture', 'fc62f1c7', 'rackcad-i62-fixture', 'Fixture.Lib', 'FIXTURE-MANIFEST', 'TEST-ACTIVATION',
          'I-62', 'I62', 'RackCad', 'Calculadora', 'architecture/portabilidad']


def sha(p):
    h = hashlib.sha256()
    with open(p, 'rb') as f:
        for chunk in iter(lambda: f.read(1 << 20), b''):
            h.update(chunk)
    return h.hexdigest()


def count_tokens(b):
    low = b.lower()
    return {t: low.count(t.lower().encode('utf-8')) for t in TOKENS if low.count(t.lower().encode('utf-8'))}


out = {'CodexHome': '~/.codex', 'Files': [], 'Dirs': {}, 'Sqlite': {}}
for name in ('AGENTS.md', 'config.toml', 'instructions.md'):
    p = os.path.join(CH, name)
    if os.path.exists(p):
        b = open(p, 'rb').read()
        e = {'Path': '~/.codex/' + name, 'Size': len(b), 'Sha256': hashlib.sha256(b).hexdigest()}
        if name != 'config.toml':
            e['UnitTokenCounts'] = count_tokens(b)
        out['Files'].append(e)
for d in ('memories', 'rules', 'skills', 'prompts'):
    p = os.path.join(CH, d)
    if not os.path.isdir(p):
        out['Dirs'][d] = 'absent'
        continue
    files = []
    for root, _, fs in os.walk(p):
        for f in fs:
            fp = os.path.join(root, f)
            rel = os.path.relpath(fp, CH).replace(os.sep, '/')
            try:
                b = open(fp, 'rb').read()
            except Exception as ex:
                files.append({'Path': '~/.codex/' + rel, 'Error': type(ex).__name__}); continue
            files.append({'Path': '~/.codex/' + rel, 'Size': len(b), 'Sha256': hashlib.sha256(b).hexdigest(), 'UnitTokenCounts': count_tokens(b)})
    out['Dirs'][d] = {'FileCount': len(files), 'Files': files if len(files) <= 60 else files[:60], 'Truncated': len(files) > 60,
                      'FilesWithUnitTokens': [f['Path'] for f in files if f.get('UnitTokenCounts')]}
for db in ('memories_1.sqlite',):
    p = os.path.join(CH, db)
    if not os.path.exists(p):
        out['Sqlite'][db] = 'absent'; continue
    info = {'Path': '~/.codex/' + db, 'Size': os.path.getsize(p), 'Sha256': sha(p)}
    try:
        con = sqlite3.connect('file:' + p.replace(os.sep, '/') + '?mode=ro', uri=True)
        tabs = [r[0] for r in con.execute("select name from sqlite_master where type='table'")]
        info['Tables'] = {}
        for t in tabs:
            cols = [r[1] for r in con.execute('pragma table_info("%s")' % t.replace('"', '""'))]
            n = con.execute('select count(*) from "%s"' % t.replace('"', '""')).fetchone()[0]
            info['Tables'][t] = {'Columns': cols, 'Rows': n}
        con.close()
    except Exception as ex:
        info['OpenError'] = type(ex).__name__ + ': ' + str(ex)[:120]
    raw = open(p, 'rb').read()
    wal = p + '-wal'
    if os.path.exists(wal):
        raw += open(wal, 'rb').read()
    info['UnitTokenCountsInDbAndWal'] = count_tokens(raw)
    out['Sqlite'][db] = info
json.dump(out, open(sys.argv[1], 'w', encoding='utf-8'), ensure_ascii=False, indent=1)
print(json.dumps({'Files': [(f['Path'], f['Size'], f.get('UnitTokenCounts')) for f in out['Files']],
                  'Dirs': {k: (v if isinstance(v, str) else (v['FileCount'], v['FilesWithUnitTokens'])) for k, v in out['Dirs'].items()},
                  'Sqlite': {k: (v if isinstance(v, str) else {kk: v.get(kk) for kk in ('Size', 'Tables', 'UnitTokenCountsInDbAndWal', 'OpenError')}) for k, v in out['Sqlite'].items()}},
                 ensure_ascii=False, indent=1))
