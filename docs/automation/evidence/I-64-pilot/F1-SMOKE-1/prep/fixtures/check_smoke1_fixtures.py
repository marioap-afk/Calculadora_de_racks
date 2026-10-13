"""Independent (stdlib only, no ezdxf) read-back of the fixture DXF, following the product's read path:
INSERT in *Model_Space -> its BLOCK_RECORD -> ACAD_XDICTIONARY -> RACKCAD_SELECTIVE -> XRECORD -> group-1 strings
concatenated (RackBlockData.Read). Writes one <block>.json per fixture block for FixtureHarness check, and compares
each with the envelope file it came from. Usage: check_smoke1_fixtures.py <dxf> <envelopeDir> <outDir>
"""
import os
import sys

DXF, ENV_DIR, OUT = sys.argv[1], sys.argv[2], sys.argv[3]
EXPECTED = {
    'I64_FX_PR13_SIN_IDENTIDAD': 'envelope-pr13-sin-identidad.json',
    'I64_FX_PR12_DIAGNOSTICO': 'envelope-pr12-diagnostico.json',
}

with open(DXF, 'rb') as fh:
    lines = fh.read().decode('utf-8').splitlines()
assert len(lines) % 2 == 0, 'odd number of lines'
pairs = [(int(lines[i].strip()), lines[i + 1]) for i in range(0, len(lines), 2)]
assert pairs[-1] == (0, 'EOF')

# Split into (section, entity type, tags) records.
records, section, current = [], None, None
for code, value in pairs:
    if code == 0:
        if current:
            records.append(current)
        current = None
        if value == 'SECTION' or value == 'ENDSEC' or value == 'EOF':
            if value != 'SECTION':
                section = None
            continue
        current = (section, value, [])
        continue
    if code == 2 and current is None:
        section = value
        continue
    if current is not None:
        current[2].append((code, value))
if current:
    records.append(current)


def handle_of(rec):
    for code, value in rec[2]:
        if code in (5, 105):
            return value.strip().upper()
    return None


by_handle = {}
for rec in records:
    h = handle_of(rec)
    if h:
        assert h not in by_handle, 'duplicate handle ' + h
        by_handle[h] = rec

block_records = {}
for rec in records:
    if rec[0] == 'TABLES' and rec[1] == 'BLOCK_RECORD':
        tags = rec[2]
        name = next(v for c, v in tags if c == 2)
        xdict = None
        for i, (c, v) in enumerate(tags):
            if c == 102 and v == '{ACAD_XDICTIONARY':
                xdict = tags[i + 1][1].strip().upper()
        block_records[name] = (handle_of(rec), xdict)

inserts = [next(v for c, v in rec[2] if c == 2) for rec in records if rec[0] == 'ENTITIES' and rec[1] == 'INSERT']
print('inserts in ENTITIES:', inserts)
os.makedirs(OUT, exist_ok=False)
ok = True
for name, env_file in EXPECTED.items():
    assert name in inserts, 'no INSERT of ' + name
    br_handle, xdict_handle = block_records[name]
    xdict = by_handle[xdict_handle]
    assert xdict[1] == 'DICTIONARY'
    owner = next(v for c, v in xdict[2] if c == 330).strip().upper()
    assert owner == br_handle, ('xdict owner', owner, br_handle)
    entries = xdict[2]
    xrec_handle = None
    for i, (c, v) in enumerate(entries):
        if c == 3 and v == 'RACKCAD_SELECTIVE':
            xrec_handle = entries[i + 1][1].strip().upper()
    xrec = by_handle[xrec_handle]
    assert xrec[1] == 'XRECORD'
    text = ''.join(v for c, v in xrec[2] if c == 1)
    with open(os.path.join(ENV_DIR, env_file), 'rb') as fh:
        expected = fh.read().decode('utf-8')
    same = text == expected
    ok = ok and same
    with open(os.path.join(OUT, name + '.json'), 'wb') as fh:
        fh.write(text.encode('utf-8'))
    print(name, 'BLOCK_RECORD', br_handle, 'XDICT', xdict_handle, 'XRECORD', xrec_handle,
          'chunks', sum(1 for c, _ in xrec[2] if c == 1), 'equal_to_envelope', same)
print('READBACK', 'PASS' if ok else 'FAIL')
sys.exit(0 if ok else 1)
