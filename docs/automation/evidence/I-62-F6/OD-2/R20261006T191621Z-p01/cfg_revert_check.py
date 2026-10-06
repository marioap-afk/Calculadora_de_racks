# Value-free check: does reverting only the app-update substitutions (new binary label -> old, new package version -> old)
# reproduce the accepted fingerprint byte for byte? Prints hashes, counts and which key names hold a substituted token; never a value.
import hashlib, itertools, os, re, sys
CFG = os.path.join(os.environ['USERPROFILE'], '.codex', 'config.toml')
TARGET = '9002E854457F2DBE074B66FB804FC24AA9B489C58436753CE74BCBA2BF767B1E'
SUBS = [('5ea220ae823df3d7', '8aaf1547b825b104'), ('26.930.7945.0', '26.930.3930.0')]
data = open(CFG, 'rb').read()
text = data.decode('utf-8')
print('current', hashlib.sha256(data).hexdigest().upper(), len(data))
section = None
holders = {}
for line in text.splitlines():
    t = line.strip()
    m = re.match(r'^\[\[?([^\]]+)\]\]?', t)
    if m:
        section = m.group(1); continue
    k = re.match(r'^([A-Za-z0-9_\-]+|"[^"]*")\s*=', t)
    for new, old in SUBS:
        if new in line:
            name = (section + '.' if section else '') + (k.group(1) if k else '?')
            if any(c in name for c in (chr(92), '/', ':')):
                name = '<redactado>'
            holders.setdefault(new, []).append(name)
print('occurrences', {new: text.count(new) for new, _ in SUBS})
print('holders', holders)
for r in range(1, len(SUBS) + 1):
    for combo in itertools.combinations(SUBS, r):
        t2 = text
        for new, old in combo:
            t2 = t2.replace(new, old)
        h = hashlib.sha256(t2.encode('utf-8')).hexdigest().upper()
        print('revert', [n for n, _ in combo], h, 'MATCH' if h == TARGET else '-')
