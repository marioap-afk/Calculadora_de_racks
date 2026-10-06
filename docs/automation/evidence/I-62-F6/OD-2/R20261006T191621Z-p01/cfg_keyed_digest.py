# Local-only keyed digests of ~/.codex/config.toml, one per key line, so a later change can be localized BY KEY NAME without ever storing,
# printing or publishing a value. The HMAC key lives only in this scratchpad (cfg-hmac.key); the digest file is never published.
# Usage: cfg_keyed_digest.py snapshot <out.json>  |  cfg_keyed_digest.py diff <old.json>
import hashlib, hmac, json, os, re, secrets, sys
BS = chr(92)
HERE = os.path.dirname(os.path.abspath(__file__))
KEYF = os.path.join(HERE, 'cfg-hmac.key')
CFG = os.path.join(os.environ['USERPROFILE'], '.codex', 'config.toml')
if not os.path.exists(KEYF):
    open(KEYF, 'w').write(secrets.token_hex(32))
K = bytes.fromhex(open(KEYF).read().strip())


def snapshot():
    data = open(CFG, 'rb').read()
    sec, out, n = None, {}, {}
    for line in data.decode('utf-8').splitlines():
        s = line.strip()
        m = re.match(r'^\[\[?([^\]]+)\]\]?', s)
        if m:
            sec = m.group(1); name = '[' + sec + ']'
        else:
            k = re.match(r'^([A-Za-z0-9_\-]+|"[^"]*"|\'[^\']*\')\s*=', s)
            if not k:
                continue
            name = (sec + '.' if sec else '') + k.group(1)
        if any(c in name for c in (BS, '/', ':')):
            name = '<redactado:' + hmac.new(K, name.encode(), hashlib.sha256).hexdigest()[:12] + '>'
        n[name] = n.get(name, 0) + 1
        out[name + '#' + str(n[name])] = hmac.new(K, line.encode('utf-8'), hashlib.sha256).hexdigest()
    return {'Sha256': hashlib.sha256(data).hexdigest().upper(), 'Lines': out}


if sys.argv[1] == 'snapshot':
    json.dump(snapshot(), open(sys.argv[2], 'w', encoding='utf-8'), indent=1)
    print('snapshot', snapshot()['Sha256'])
else:
    old = json.load(open(sys.argv[2], encoding='utf-8'))
    new = snapshot()
    a, b = old['Lines'], new['Lines']
    print('old', old['Sha256'], 'new', new['Sha256'])
    print('added', sorted(set(b) - set(a)))
    print('removed', sorted(set(a) - set(b)))
    print('changed', sorted(k for k in set(a) & set(b) if a[k] != b[k]))
