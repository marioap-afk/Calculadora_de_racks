"""I-52 CT-21D - reference encoder and NORMATIVE test vectors of FPSPEC-V1 (BA-05 V2).

Reproducible: `python I-52-ct21d-fpspec-v1-vectors.gen.py` rewrites
`I-52-ct21d-fpspec-v1-vectors.json` next to this file. The encoder below is the
reference of BA-05 V2 section 2.1; an implementation of the fingerprint provider
(instrument I-12) is conformant only if it reproduces every `EXACT` digest and every
`UNKNOWN` of this file. Design artifact only: no host, no product code.
"""
import hashlib
import json
import math
import os
import struct

PREFIX = b'FPSPEC-V1|EXACT|'


class Unknown(Exception):
    """The record cannot be fingerprinted: result UNKNOWN, never a digest."""


def enc_value(kind, v):
    if v is None:
        return b'~'
    if kind == 'int':
        if isinstance(v, bool) or not isinstance(v, int):
            raise TypeError('int expected')
        return str(v).encode('ascii')
    if kind == 'bool':
        return b'1' if v else b'0'
    if kind == 'double':
        if math.isnan(v) or math.isinf(v):
            raise Unknown('non-finite double')
        return struct.pack('>d', v).hex().encode('ascii')
    if kind in ('string', 'enum'):
        b = v.encode('utf-8')
        return b'S' + str(len(b)).encode('ascii') + b':' + b
    if kind == 'bytes':
        return b'B' + str(len(v)).encode('ascii') + b':' + v
    if kind.startswith('seq:'):
        inner = kind[4:]
        out = b'[' + str(len(v)).encode('ascii') + b'\n'
        for e in v:
            out += enc_value(inner, e) + b'\n'
        return out + b']'
    if kind.startswith('set:'):
        inner = kind[4:]
        keyed = sorted(v, key=lambda e: e.encode('utf-8') if isinstance(e, str) else enc_value(inner, e))
        return enc_value('seq:' + inner, keyed)
    if kind == 'record':
        rtype, fields = v
        return b'{' + rtype.encode('ascii') + b'\n' + enc_fields(fields) + b'}'
    if kind == 'ref':
        name, digest = v
        return b'@' + enc_value('string', name) + b'#' + digest.encode('ascii')
    raise ValueError(kind)


def enc_fields(fields):
    out = b''
    for name, kind, v in fields:
        out += name.encode('ascii') + b'=' + enc_value(kind, v) + b'\n'
    return out


def stream(rtype, fields):
    return PREFIX + rtype.encode('ascii') + b'|' + enc_fields(fields)


def digest(rtype, fields):
    return hashlib.sha256(stream(rtype, fields)).hexdigest()


def show(b):
    return b.decode('utf-8').replace('\n', '\\n')


C3 = 'TEST_COORD3'
vectors = []


def add(vid, purpose, rtype, fields, expect_unknown=False):
    try:
        s = stream(rtype, fields)
        d = hashlib.sha256(s).hexdigest()
        if expect_unknown:
            raise SystemExit(vid + ' expected UNKNOWN')
        vectors.append({'id': vid, 'purpose': purpose, 'recordType': rtype, 'result': 'DIGEST',
                        'stream': show(s), 'sha256': d})
        return d
    except Unknown as e:
        if not expect_unknown:
            raise
        vectors.append({'id': vid, 'purpose': purpose, 'recordType': rtype, 'result': 'UNKNOWN', 'reason': str(e)})
        return None


def coord(x, y, z):
    return [('x', 'double', x), ('y', 'double', y), ('z', 'double', z)]


d1 = add('EV-01', 'baseline coordinate (+0.0 components)', C3, coord(1.0, 0.0, 0.0))
d2 = add('EV-02', 'SIGNED ZERO: y = -0.0 must differ from EV-01', C3, coord(1.0, -0.0, 0.0))
d3 = add('EV-03', '1 ulp above 1.0 must differ from EV-01', C3, coord(math.nextafter(1.0, 2.0), 0.0, 0.0))
add('EV-04', 'SUBNORMAL: smallest positive subnormal', C3, coord(5e-324, 0.0, 0.0))
add('EV-05', 'MAXIMUM FINITE double', C3, coord(1.7976931348623157e308, 0.0, 0.0))
add('EV-06', 'NaN is unsupported: UNKNOWN, no digest', C3, coord(float('nan'), 0.0, 0.0), expect_unknown=True)
add('EV-07', 'infinity is unsupported: UNKNOWN, no digest', C3, coord(float('inf'), 0.0, 0.0), expect_unknown=True)
d8 = add('EV-08', 'string, exact case', 'TEST_NAME', [('name', 'string', 'Rack N 1')])
d9 = add('EV-09', 'string, case differs from EV-08: must differ', 'TEST_NAME', [('name', 'string', 'rack N 1')])
add('EV-10', 'EMPTY STRING', 'TEST_NAME', [('name', 'string', '')])
add('EV-11', 'MULTIBYTE string (UTF-8, no normalization)', 'TEST_NAME', [('name', 'string', 'Ñandú 日本 €')])
add('EV-12', 'ABSENT value', 'TEST_OPTIONAL', [('name', 'string', 'Rack'), ('layer', 'string', None)])
d13 = add('EV-13', 'ORDERED SEQUENCE (draw order is state)', 'TEST_SEQ', [('items', 'seq:string', ['b', 'a'])])
d14 = add('EV-14', 'ordered sequence in the other order: must differ from EV-13', 'TEST_SEQ', [('items', 'seq:string', ['a', 'b'])])
d15 = add('EV-15', 'UNORDERED container given as b,a: sorted by stored name', 'TEST_SET', [('names', 'set:string', ['b', 'a'])])
d16 = add('EV-16', 'unordered container given as a,b: must EQUAL EV-15', 'TEST_SET', [('names', 'set:string', ['a', 'b'])])
add('EV-17', 'NESTED record inline', 'TEST_OUTER', [('label', 'string', 'outer'), ('inner', 'record', (C3, coord(1.0, 2.0, 3.0)))])
add('EV-18', 'NESTED definition referenced by name and digest', 'TEST_REFHOLDER',
    [('ref', 'ref', ('SEL_FRONTAL_Post_0', d1))])
p_known = b'{"Id":"g-1","Kind":"selective","SchemaVersion":"1.0"}'
p_unknown = b'{"Id":"g-1","Kind":"selective","SchemaVersion":"1.0","futureMember":1}'
d19 = add('EV-19', 'PAYLOAD_EXACT raw bytes, no unknown member', 'PAYLOAD_EXACT', [('bytes', 'bytes', p_known)])
d20 = add('EV-20', 'PAYLOAD_EXACT with an UNKNOWN / FORWARD-COMPATIBLE member (ExtensionData): must differ from EV-19', 'PAYLOAD_EXACT', [('bytes', 'bytes', p_unknown)])

assert d1 != d2 and d1 != d3 and d8 != d9 and d13 != d14 and d15 == d16 and d19 != d20

semantic = [
    {'id': 'SV-01', 'layer': 'SEMANTIC_COMPARISON', 'expected': '{"Id":"g-1","ExtensionData":{"futureKey":1}}', 'persisted': '{"ExtensionData":{"futureKey":1},"Id":"g-1"}', 'result': 'EQUAL', 'rule': 'keys compared after canonical sorting; extension member present and equal'},
    {'id': 'SV-02', 'layer': 'SEMANTIC_COMPARISON', 'expected': '{"Id":"g-1","ExtensionData":{"futureKey":1}}', 'persisted': '{"Id":"g-1"}', 'result': 'DIFFERENT', 'rule': 'a writer that DROPS an unknown member must not pass (O4 through S-2/S-3)'},
    {'id': 'SV-03', 'layer': 'SEMANTIC_COMPARISON', 'expected': '{"Id":"g-1","ExtensionData":{"futureKey":1}}', 'persisted': '{"Id":"g-1","ExtensionData":{"futureKey":2}}', 'result': 'DIFFERENT', 'rule': 'a writer that ALTERS an unknown member must not pass'},
    {'id': 'SV-04', 'layer': 'SEMANTIC_COMPARISON', 'expected': 'unknown member nested inside a known sub-object that has no ExtensionData (JsonExtensionData is not recursive, I-11)', 'persisted': 'any', 'result': 'UNKNOWN_MEMBER_UNSUPPORTED', 'rule': 'surfaced explicitly, gives UNKNOWN (O5 path); never ignored, never treated as absent'},
    {'id': 'SV-05', 'layer': 'SEMANTIC_COMPARISON', 'expected': 'coordinate y = +0.0', 'persisted': 'coordinate y = -0.0', 'result': 'EQUAL', 'rule': 'signed zero is domain-equivalent; secondary finding BIT_LEVEL_DIFFERENCE_DOMAIN_EQUIVALENT; the EXACT digests differ (EV-01 / EV-02)'},
    {'id': 'SV-06', 'layer': 'SEMANTIC_COMPARISON', 'expected': 'length 10.0 in', 'persisted': 'length 10.0000000005 in', 'result': 'EQUAL', 'rule': 'within TOL_LENGTH = GeometryTolerance.Length (1e-9 in)'},
    {'id': 'SV-07', 'layer': 'SEMANTIC_COMPARISON', 'expected': 'length 10.0 in', 'persisted': 'length 10.000000002 in', 'result': 'DIFFERENT', 'rule': 'outside TOL_LENGTH'},
    {'id': 'SV-08', 'layer': 'SEMANTIC_COMPARISON', 'expected': 'envelope SchemaVersion "1.0"', 'persisted': 'envelope SchemaVersion "9.0" (unrecognized major)', 'result': 'UNKNOWN', 'rule': 'an unrecognized schema version is UNKNOWN'},
    {'id': 'SV-09', 'layer': 'SEMANTIC_COMPARISON', 'expected': 'entity of a type outside the supported list (proxy/custom)', 'persisted': 'same', 'result': 'UNKNOWN', 'rule': 'unsupported type is UNKNOWN; in the closure it refuses the run (O1)'},
]

out = {'spec': 'FPSPEC-V1', 'status': 'NORMATIVE (as part of BA-05 V2 when sealed)', 'exactVectors': vectors, 'semanticVectors': semantic}
here = os.path.dirname(os.path.abspath(__file__))
with open(os.path.join(here, 'I-52-ct21d-fpspec-v1-vectors.json'), 'w', encoding='utf-8', newline='\n') as f:
    f.write(json.dumps(out, indent=1, ensure_ascii=False) + '\n')
print('exact vectors:', len(vectors), 'semantic vectors:', len(semantic))
