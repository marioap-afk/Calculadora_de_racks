"""I-52 CT-21D - reference encoder, record schemas and NORMATIVE test vectors of FPSPEC-V1 (BA-05 V5).

Reproducible: `python I-52-ct21d-fpspec-v1-vectors-v5.gen.py` rewrites
`I-52-ct21d-fpspec-v1-vectors-v5.json` next to this file. The schema registry
`SCHEMAS` below is the normative field table of every record type (BA-05 V5
section 2.3 is generated from it); the tables `RB_KIND_RANGES`,
`ENTITY_CLASS_MAP`, `ATTRIBUTE_CLASS_MAP`, `NOD_CLASS_MAP`, `HOST_VALUE_KINDS`,
`RECORD_KINDS`, `ITEM_FORMS`, `LAYER2_CLASSES`, the layer-2 field routes and
`DIMVAR_CLASSES` are the normative tables of BA-05 V5 sections 2.1.2, 2.1.3,
2.3.1, 2.3.5, 2.3.6, 3.1 and 3.1.2 (`CONTEXT_VARIABLE_HOST_TYPES` is the
expectation that the I-12 qualification confirms, section 2.1.3); the encoder is
the reference of BA-05 V5 section 2.1. An implementation of the fingerprint
provider (instrument I-12) is conformant only if, from the `input` of every
exact vector, it reproduces every `DIGEST` (byte for byte, field `streamHex`),
every `UNKNOWN` and every `REJECTED` of the JSON file. Design artifact only: no
host, no product code, and **no tolerance value**: the semantic vectors in slot
units (SV-06, SV-07, SV-10, SV-11, SV-14, SV-15) are instantiated by the
Q-FPSPEC-VECTORS harness (BA-05 V5 section 3.3.1). The V2, V3 and V4 files stay
as history.
"""
import hashlib
import json
import math
import os
import re
import struct

PREFIX = b'FPSPEC-V1|EXACT|'


class Unknown(Exception):
    """The record cannot be fingerprinted: result UNKNOWN, never a digest."""


class Rejected(Exception):
    """Malformed producer input (it violates a field table or a conditional rule): no digest and no UNKNOWN."""


class _Skip(Exception):
    """Internal: a value that is UNKNOWN is skipped so that the rest of the record is still checked for REJECTED."""


# REJECTED takes precedence over UNKNOWN (BA-05 V5 section 2.1): an UNKNOWN condition is deferred, every other field and
# element is still checked, and the record is UNKNOWN only if no rule rejected it.
DEFERRED = []


def defer(msg):
    DEFERRED.append(msg)


def unknown(msg):
    DEFERRED.append(msg)
    raise _Skip()


class Point3d:
    """A host Point3d value (in a ResultBuffer entry or a dynamic-property value)."""

    def __init__(self, x, y, z):
        self.x, self.y, self.z = float(x), float(y), float(z)


class Point2d:
    """A host Point2d value: no typed-value kind (section 2.1.3), so UNKNOWN."""

    def __init__(self, x, y):
        self.x, self.y = float(x), float(y)


# ----------------------------------------------------------------------------------------------------------------
# Value kinds (BA-05 V5 section 2.1)
# ----------------------------------------------------------------------------------------------------------------
def utf8(s):
    """Exact stored text as UTF-8. A string that is not well-formed UTF-16 (a lone surrogate) is UNKNOWN."""
    try:
        return s.encode('utf-8')
    except UnicodeEncodeError:
        unknown('string is not well-formed UTF-16 (lone surrogate)')


def order_key(s):
    """Ordinal order = Unicode scalar value order = UTF-8 byte order (never UTF-16 code-unit order)."""
    return utf8(s)


# DXF group-code ranges -> value kind, for the typed values of a ResultBuffer (RB records). Section 2.1.2.
# AR3-21: code 5 is UNKNOWN (a handle), and the point-valued codes carry an inline POINT3D.
RB_KIND_RANGES = [
    ((0, 4), 'string'), ((6, 9), 'string'), ((10, 18), 'point3d'), ((19, 59), 'double'), ((60, 79), 'int'),
    ((90, 99), 'int'), ((100, 100), 'string'), ((102, 102), 'string'), ((110, 112), 'point3d'), ((113, 149), 'double'),
    ((160, 169), 'int'), ((170, 179), 'int'), ((210, 210), 'point3d'), ((211, 239), 'double'), ((270, 289), 'int'),
    ((290, 299), 'bool'), ((300, 309), 'string'), ((310, 319), 'bytes'), ((370, 389), 'int'), ((400, 409), 'int'),
    ((410, 419), 'string'), ((420, 429), 'int'), ((430, 439), 'string'), ((440, 459), 'int'), ((460, 469), 'double'),
    ((470, 479), 'string'), ((999, 1003), 'string'), ((1004, 1004), 'bytes'), ((1006, 1009), 'string'),
    ((1010, 1013), 'point3d'), ((1014, 1059), 'double'), ((1060, 1071), 'int'),
]
RB_HANDLE_CODES = {5: 'entity handle', 105: 'handle', 1005: 'extended-data handle'}
RB_UNSUPPORTED = ('handle and object-id codes (5, 105, 320-369, 390-399, 480-481, 1005) and every code outside the table; '
                  'a host value whose type is not the one of its code range')
HOST_NAME = {float: 'Double', int: 'Int32', str: 'String', bool: 'Boolean', bytes: 'byte[]'}
RB_HOST_TYPE = {'string': 'String', 'double': 'Double', 'int': 'Int16, Int32 or Int64', 'bool': 'Boolean', 'bytes': 'byte[]',
                'point3d': 'Point3d'}


def rb_kind(code):
    if isinstance(code, bool) or not isinstance(code, int):
        raise Rejected('an RB entry is [integer code, value]')
    if code in RB_HANDLE_CODES:
        unknown('RB code %d (%s) is a volatile identifier' % (code, RB_HANDLE_CODES[code]))
    for (lo, hi), kind in RB_KIND_RANGES:
        if lo <= code <= hi:
            return kind
    unknown('RB code %d is outside the table' % code)


def rb_value(code, v):
    """The (encoding kind, value) of one ResultBuffer entry; a host value of the wrong type is UNKNOWN (fail closed)."""
    kind = rb_kind(code)
    ok = {'string': isinstance(v, str), 'double': isinstance(v, float), 'int': isinstance(v, int) and not isinstance(v, bool),
          'bool': isinstance(v, bool), 'bytes': isinstance(v, bytes), 'point3d': isinstance(v, Point3d)}[kind]
    if not ok:
        unknown('RB code %d holds a host value of type %s, not %s' % (code, HOST_NAME.get(type(v), type(v).__name__), RB_HOST_TYPE[kind]))
    if kind == 'point3d':
        return 'record:POINT3D', {'x': v.x, 'y': v.y, 'z': v.z}
    return kind, v


TYPED_KINDS = ('ABSENT', 'BOOL', 'INT', 'DOUBLE', 'STRING', 'NAME', 'BYTES', 'POINT3D', 'COLOR')

# Host value type -> TYPED_VALUE kind, for values read from the host (dynamic properties, context variables). Section 2.1.3.
HOST_VALUE_KINDS = [
    ['null', 'ABSENT'], ['System.Boolean', 'BOOL'], ['System.Int16, System.Int32, System.Int64', 'INT'], ['System.Double', 'DOUBLE'],
    ['System.String', 'STRING'], ['Point3d', 'POINT3D'], ['any other type (for example Point2d, Vector3d, ObjectId, byte[])', 'UNKNOWN'],
]


def host_typed(v):
    if v is None:
        return ('ABSENT', None)
    if isinstance(v, bool):
        return ('BOOL', v)
    if isinstance(v, int):
        return ('INT', v)
    if isinstance(v, float):
        return ('DOUBLE', v)
    if isinstance(v, str):
        return ('STRING', v)
    if isinstance(v, Point3d):
        return ('POINT3D', {'x': v.x, 'y': v.y, 'z': v.z})
    unknown('a host value of type %s has no typed-value kind (section 2.1.3)' % type(v).__name__)


# The read of a CONTEXT_VARIABLE (BA-05 V5 section 2.1.3; R1:BA05V4-05 (a), R2:BA05V4-N06) and the host type the I-12
# qualification is expected to confirm for each variable a kind baseline may capture (a different observed type is
# corrected in this table before the seal, BA-05 V5 section 2.6). The typing itself always follows HOST_VALUE_KINDS.
CONTEXT_VARIABLE_READ = ('the host system-variable read by name (the managed Application.GetSystemVariable of the variable name), taken while the '
                         'target document is the active document and its lock is held, with the value exactly as that read returns it; the typed '
                         'Database properties (for example Database.Clayer, an ObjectId, Database.Cecolor, a Color, Database.Celweight, a LineWeight, '
                         'Database.Cetransparency, a Transparency) are never the read')
CONTEXT_VARIABLE_HOST_TYPES = [
    ['CLAYER', 'System.String', 'STRING'], ['CECOLOR', 'System.String', 'STRING'], ['CELTYPE', 'System.String', 'STRING'],
    ['CELTSCALE', 'System.Double', 'DOUBLE'], ['CELWEIGHT', 'System.Int16', 'INT'], ['CETRANSPARENCY', 'System.String', 'STRING'],
    ['CPLOTSTYLE', 'System.String', 'STRING'], ['TEXTSTYLE', 'System.String', 'STRING'], ['DIMSTYLE', 'System.String', 'STRING'],
    ['PSTYLEMODE', 'System.Int16', 'INT'],
]


# Exact host class (RXClass name) -> record type. Section 2.3.1. Never a subclass test and never the DXF name.
ENTITY_CLASS_MAP = {
    'AcDbLine': 'ENTITY_LINE', 'AcDbArc': 'ENTITY_ARC', 'AcDbCircle': 'ENTITY_CIRCLE', 'AcDbPolyline': 'ENTITY_LWPOLYLINE',
    'AcDbText': 'ENTITY_TEXT', 'AcDbMText': 'ENTITY_MTEXT', 'AcDbBlockReference': 'ENTITY_INSERT',
    'AcDbAttributeDefinition': 'ENTITY_ATTDEF', 'AcDbRotatedDimension': 'ENTITY_ROTATED_DIMENSION',
    'AcDbAlignedDimension': 'ENTITY_ALIGNED_DIMENSION',
}
ATTRIBUTE_CLASS_MAP = {'AcDbAttribute': 'ENTITY_ATTRIB'}
# Exact host class of a named-object-dictionary object -> NOD_ENTRY.objectClass (section 2.3.1; R1:BA05V4-07).
NOD_CLASS_MAP = {'AcDbXrecord': 'XRECORD', 'AcDbDictionary': 'DICTIONARY'}
MANAGED_NAME = {
    'AcDbLine': 'Line', 'AcDbArc': 'Arc', 'AcDbCircle': 'Circle', 'AcDbPolyline': 'Polyline', 'AcDbText': 'DBText', 'AcDbMText': 'MText',
    'AcDbBlockReference': 'BlockReference', 'AcDbAttributeDefinition': 'AttributeDefinition', 'AcDbRotatedDimension': 'RotatedDimension',
    'AcDbAlignedDimension': 'AlignedDimension', 'AcDbAttribute': 'AttributeReference',
}

# recordKind of a STATE_MEMBER / MANIFEST_* entry -> record type of its digest and meaning of its expectedName. Section 2.3.5.
# The fourth column fixes STATE_MEMBER.storedName (and MANIFEST_*.storedName) for every record kind.
RECORD_KINDS = [
    ['BLOCK', 'DEFINITION_DUMP', 'the block name', 'PRESENT: the stored name of the pre-existing block table record; else absent'],
    ['LAYER', 'SYMBOL_RECORD_LAYER', 'the layer name', 'PRESENT: the stored name of the pre-existing layer; else absent'],
    ['LTYPE', 'SYMBOL_RECORD_LTYPE', 'the linetype name', 'PRESENT: the stored name of the pre-existing linetype; else absent'],
    ['STYLE', 'SYMBOL_RECORD_STYLE', 'the text style name', 'PRESENT: the stored name of the pre-existing text style; else absent'],
    ['DIMSTYLE', 'SYMBOL_RECORD_DIMSTYLE', 'the dimension style name', 'PRESENT: the stored name of the pre-existing dimension style; else absent'],
    ['APPID', 'SYMBOL_RECORD_APPID', 'the registered application name', 'PRESENT: the stored name of the pre-existing registered application; else absent'],
    ['PAYLOAD', 'PAYLOAD_EXACT', 'the name of the definition that owns the payload',
     'PRESENT: the stored name of that definition (the field owner of its PAYLOAD_EXACT); else absent'],
    ['NOD_ENTRY', 'NOD_ENTRY', 'the NOD path', 'PRESENT: the stored path (the field path of its NOD_ENTRY: the stored keys joined by "/"); else absent'],
    ['ENUMERATION', 'ENUMERATION', 'the container role name', 'always absent (a role name, not the name of a stored record)'],
    ['CONTEXT_VARIABLE', 'CONTEXT_VARIABLE', 'the variable name', 'always absent (a variable is not a stored record)'],
    ['REFERENCE', 'ENTITY_INSERT', 'the handle of the reference in the item form HANDLE (session-local; AR4-11)', 'always absent (an entity has no stored name)'],
]
RECORD_KIND_NAMES = [r[0] for r in RECORD_KINDS]
NAMED_KINDS = ('BLOCK', 'LAYER', 'LTYPE', 'STYLE', 'DIMSTYLE', 'APPID', 'PAYLOAD', 'NOD_ENTRY')
CATEGORIES = ('READ_SET', 'CLOSURE', 'CONTEXT')
STATES = ('PRESENT', 'ABSENT', 'UNKNOWN')

# ENUMERATION item forms. Section 2.3.6.
HANDLE_RE = r'[1-9a-f][0-9a-f]{0,15}'
HANDLE_ONLY = '^' + HANDLE_RE + '$'
ITEM_FORMS = [
    ['NAME', 'the stored name', None],
    ['HANDLE', 'the handle in lowercase hexadecimal without leading zeros', '^' + HANDLE_RE + '$'],
    ['HANDLE_CLASS', 'the handle as in HANDLE, a colon, and the exact host class name (RXClass name) of the object', '^' + HANDLE_RE + ':[A-Za-z0-9_]+$'],
]
ITEM_FORM_RE = {f[0]: f[2] for f in ITEM_FORMS}


def seq_bytes(encoded):
    """Already-encoded elements as an ordered sequence."""
    return b'[' + str(len(encoded)).encode('ascii') + b'\n' + b''.join(e + b'\n' for e in encoded) + b']'


def enc_elem(kind, x):
    """One element; an UNKNOWN element is skipped (deferred) so the other elements are still checked."""
    try:
        return enc_value(kind, x)
    except _Skip:
        return b'?'


def enc_seq(pairs):
    """pairs = [(kind, value)], encoded as an ordered sequence."""
    return seq_bytes([enc_elem(k, x) for k, x in pairs])


def enc_value(kind, v):
    if v is None and kind not in ('seq:rb',):
        return b'~'
    if kind == 'int':
        if isinstance(v, bool) or not isinstance(v, int):
            raise Rejected('int expected')
        return str(v).encode('ascii')
    if kind == 'bool':
        if not isinstance(v, bool):
            raise Rejected('bool expected')
        return b'1' if v else b'0'
    if kind == 'double':
        if not isinstance(v, float):
            raise Rejected('double expected')
        if math.isnan(v) or math.isinf(v):
            unknown('non-finite double')
        return struct.pack('>d', v).hex().encode('ascii')
    if kind in ('string', 'enum'):
        if not isinstance(v, str):
            raise Rejected('string expected')
        b = utf8(v)
        return b'S' + str(len(b)).encode('ascii') + b':' + b
    if kind == 'bytes':
        return b'B' + str(len(v)).encode('ascii') + b':' + v
    if kind == 'digest':
        if not (isinstance(v, str) and len(v) == 64 and all(c in '0123456789abcdef' for c in v)):
            raise Rejected('64 lowercase hex digits expected')
        return v.encode('ascii')
    if kind == 'seq:rb':
        if v is None:
            return b'~'
        out = []
        for entry in v:
            if not (isinstance(entry, (list, tuple)) and len(entry) == 2):
                raise Rejected('an RB entry is [code, value]')
            code, val = entry
            try:
                out.append(enc_value('record:RB', {'code': code, 'value': rb_value(code, val)}))
            except _Skip:
                out.append(b'?')
        return seq_bytes(out)
    if kind in ('seq:entity', 'seq:attrib'):
        cmap = ENTITY_CLASS_MAP if kind == 'seq:entity' else ATTRIBUTE_CLASS_MAP
        out = []
        for entry in v:
            if not (isinstance(entry, (list, tuple)) and len(entry) == 2):
                raise Rejected('an entity is [host class, fields]')
            cls, fields = entry
            if cls not in cmap:
                defer('%s of host class %s has no record type (exact class, section 2.3.1)' % ('entity' if kind == 'seq:entity' else 'attribute', cls))
                out.append(b'?')
                continue
            out.append(enc_elem('record:' + cmap[cls], fields))
        return seq_bytes(out)
    if kind == 'dynprops':
        for p in v:
            if not (isinstance(p, dict) and set(p) == {'name', 'readOnly', 'value'} and isinstance(p['name'], str) and isinstance(p['readOnly'], bool)):
                raise Rejected('a dynamic property is {"name": string, "readOnly": boolean, "value": host value} (section 2.3.2)')
        recs = []
        for p in v:
            if p['readOnly']:
                continue
            try:
                recs.append({'name': p['name'], 'value': host_typed(p['value'])})
            except _Skip:
                pass
        return enc_value('set:record:DYNPROP@name', recs)
    if kind.startswith('seq:'):
        inner = kind[4:]
        return enc_seq([(inner, e) for e in v])
    if kind.startswith('set:record:'):
        rtype, key = kind[len('set:record:'):].split('@')
        encoded = [enc_elem('record:' + rtype, e) for e in v]
        keys = [order_key(e[key]) for e in v]
        if len(set(keys)) != len(keys):
            unknown('duplicate key in an unordered set of ' + rtype)
        order = sorted(range(len(v)), key=lambda i: keys[i])
        return seq_bytes([encoded[i] for i in order])
    if kind == 'set:string':
        encoded = [enc_elem('string', e) for e in v]
        keys = [order_key(e) for e in v]
        if len(set(keys)) != len(keys):
            unknown('duplicate member in an unordered set')
        order = sorted(range(len(v)), key=lambda i: keys[i])
        return seq_bytes([encoded[i] for i in order])
    if kind.startswith('record:'):
        rtype = kind[len('record:'):]
        return b'{' + rtype.encode('ascii') + b'\n' + enc_fields(rtype, v) + b'}'
    if kind == 'ref':
        name, dg = v
        enc_name = enc_value('string', name)
        if dg == 'UNKNOWN':
            unknown('the referenced definition %s is UNKNOWN: UNKNOWN propagates through the nested digest (AR3-18)' % name)
        return b'@' + enc_name + b'#' + enc_value('digest', dg)
    if kind == 'typed':
        tkind, tval = v
        if tkind not in TYPED_KINDS:
            unknown('typed value of an unsupported kind ' + str(tkind))
        if (tkind == 'ABSENT') != (tval is None):
            raise Rejected('TYPED_VALUE: the value is absent exactly when the kind is ABSENT (section 2.1)')
        inner = {'ABSENT': None, 'BOOL': 'bool', 'INT': 'int', 'DOUBLE': 'double', 'STRING': 'string', 'NAME': 'string',
                 'BYTES': 'bytes', 'POINT3D': 'record:POINT3D', 'COLOR': 'record:COLOR'}[tkind]
        return enc_value('record:TYPED_VALUE', {'kind': tkind, 'value': None if inner is None else (inner, tval)})
    if kind == 'hosttyped':
        return enc_value('typed', host_typed(v))
    if kind == 'typed-inner':
        inner, tval = v
        return enc_value(inner, tval)
    raise ValueError(kind)


# ----------------------------------------------------------------------------------------------------------------
# Record schemas (BA-05 V5 section 2.3). Fields are streamed in ORDINAL order of the field name (V2.1 28.1,
# AR2-32), never in the order listed here. The listed order is only for reading.
# ----------------------------------------------------------------------------------------------------------------
P3 = lambda p: [(p + 'X', 'double', ''), (p + 'Y', 'double', ''), (p + 'Z', 'double', '')]
COLOR_FIELDS = [('colorMethod', 'enum', 'stored name of the host ColorMethod'), ('colorIndex', 'int', 'ACI index as the host reports it'),
                ('colorRgb', 'int', 'R*65536 + G*256 + B, present exactly when colorMethod = ByColor (else REJECTED)'),
                ('colorBookName', 'string', 'color book name, or absent'), ('colorName', 'string', 'color name in the book, or absent')]
TRANSP = [('transparencyMethod', 'enum', 'stored name of the host TransparencyMethod'),
          ('transparencyAlpha', 'int', 'alpha 0-255, present exactly when transparencyMethod = ByAlpha (else REJECTED)')]
ENTITY_COMMON = [('dxfName', 'string', 'DXF class name as the host reports it (recorded; the record type comes from the exact host class, section 2.3.1)'),
                 ('layer', 'string', 'layer name (stored case)'),
                 ('linetype', 'string', 'linetype name (stored case)'), ('linetypeScale', 'double', ''),
                 ('lineweight', 'enum', 'stored name of the host LineWeight value'), ('visible', 'bool', '')] + COLOR_FIELDS + TRANSP
OVERRIDES_DESC = ('the stored per-entity dimension-variable overrides (AR4-10): the typed values of the DSTYLE section of the entity\'s ACAD extended data, '
                  'in host order (section 2.3.2); empty when there is no DSTYLE section, never absent; the host-derived *D block is excluded')

SCHEMAS = {
    # ---- helper records ----
    'POINT3D': {'use': 'helper', 'fields': [('x', 'double', ''), ('y', 'double', ''), ('z', 'double', '')]},
    'COLOR': {'use': 'helper', 'fields': COLOR_FIELDS},
    'TYPED_VALUE': {'use': 'helper', 'fields': [('kind', 'enum', 'one of ' + ', '.join(TYPED_KINDS)),
                                                ('value', 'typed-inner', 'encoded by the kind; absent exactly when the kind is ABSENT (else REJECTED)')]},
    'RB': {'use': 'helper', 'fields': [('code', 'int', 'the DXF group code of the typed value'), ('value', 'typed-inner', 'encoded by the kind of the code range (section 2.1.2)')]},
    'DYNPROP': {'use': 'helper', 'fields': [('name', 'string', 'dynamic property name (stored)'), ('value', 'typed', 'typed value from the host value (section 2.1.3)')]},
    'DIMVAR_VALUE': {'use': 'helper', 'fields': [('name', 'string', 'dimension variable name (closed list, section 2.3.3)'), ('value', 'typed', 'typed value')]},
    'PLINE_VERTEX': {'use': 'helper', 'fields': [('x', 'double', ''), ('y', 'double', ''), ('bulge', 'double', ''), ('startWidth', 'double', ''), ('endWidth', 'double', '')]},
    'LTYPE_DASH': {'use': 'helper', 'fields': [('length', 'double', ''), ('shapeNumber', 'int', ''), ('shapeStyleName', 'string', 'or absent'),
                                               ('shapeOffsetX', 'double', ''), ('shapeOffsetY', 'double', ''), ('shapeRotation', 'double', ''),
                                               ('shapeScale', 'double', ''), ('shapeIsUcsOriented', 'bool', ''), ('text', 'string', 'or absent')]},
    'STATE_MEMBER': {'use': 'helper', 'fields': [('memberKey', 'string', 'category|recordKind|expectedName (the set key; "|" is a literal character); another form is REJECTED'),
                                                 ('category', 'enum', 'READ_SET, CLOSURE or CONTEXT (section 2.3.5)'),
                                                 ('recordKind', 'string', 'one of the record kinds of section 2.3.5'),
                                                 ('expectedName', 'string', 'the name queried with host symbol-table semantics, or the key of section 2.3.5 (REFERENCE: a handle in the item form HANDLE, else REJECTED)'),
                                                 ('storedName', 'string', 'by the storedName column of section 2.3.5: present exactly when the member is PRESENT and its record kind is named'),
                                                 ('state', 'enum', 'PRESENT, ABSENT or UNKNOWN; any other value is REJECTED'),
                                                 ('digest', 'digest', 'the exact digest of the member record (record type of section 2.3.5), present exactly when PRESENT')]},
    'MANIFEST_ADDITION': {'use': 'helper', 'fields': [('memberKey', 'string', 'the memberKey of the corresponding closure member: CLOSURE|recordKind|expectedName, its recordKind part equal to recordKind (else REJECTED)'),
                                                      ('recordKind', 'string', 'as STATE_MEMBER (section 2.3.5)'),
                                                      ('storedName', 'string', 'the stored name of the added record (never absent)'), ('digest', 'digest', 'exact digest recorded after the import')]},
    'MANIFEST_REUSED': {'use': 'helper', 'fields': [('memberKey', 'string', 'as MANIFEST_ADDITION'), ('recordKind', 'string', 'as STATE_MEMBER (section 2.3.5)'),
                                                    ('storedName', 'string', 'the stored name of the pre-existing record (never absent)'),
                                                    ('preCommandDigest', 'digest', 'the digest of the pre-command record')]},
    # ---- production records ----
    'DEFINITION_DUMP': {'use': 'production', 'fields': [
        ('name', 'string', 'block name (stored case)')] + P3('origin') + [
        ('explodable', 'bool', ''), ('scaling', 'enum', 'stored name of the host BlockScaling'), ('units', 'enum', 'stored name of the host UnitsValue'),
        ('annotative', 'enum', 'stored name of the host AnnotativeStates'),
        ('entities', 'seq:entity', 'every entity of the record in HOST ITERATION ORDER, each an inline ENTITY_* record chosen by its exact host class (sections 2.2, 2.3.1)')]},
    'ENTITY_LINE': {'use': 'production', 'fields': ENTITY_COMMON + P3('start') + P3('end') + P3('normal') + [('thickness', 'double', '')]},
    'ENTITY_ARC': {'use': 'production', 'fields': ENTITY_COMMON + P3('center') + [('radius', 'double', ''), ('startAngle', 'double', ''), ('endAngle', 'double', '')] + P3('normal') + [('thickness', 'double', '')]},
    'ENTITY_CIRCLE': {'use': 'production', 'fields': ENTITY_COMMON + P3('center') + [('radius', 'double', '')] + P3('normal') + [('thickness', 'double', '')]},
    'ENTITY_LWPOLYLINE': {'use': 'production', 'fields': ENTITY_COMMON + [('closed', 'bool', ''), ('plinegen', 'bool', ''), ('elevation', 'double', ''), ('thickness', 'double', '')] + P3('normal') + [
        ('vertices', 'seq:record:PLINE_VERTEX', 'vertices in host order')]},
    'ENTITY_TEXT': {'use': 'production', 'fields': ENTITY_COMMON + [('textString', 'string', 'exact stored text'), ('height', 'double', ''), ('rotation', 'double', ''),
                                                                    ('widthFactor', 'double', ''), ('oblique', 'double', ''), ('styleName', 'string', 'text style name'),
                                                                    ('horizontalMode', 'enum', 'stored name of the host TextHorizontalMode'),
                                                                    ('verticalMode', 'enum', 'stored name of the host TextVerticalMode'),
                                                                    ('mirroredInX', 'bool', ''), ('mirroredInY', 'bool', ''), ('thickness', 'double', '')] + P3('position') + P3('alignment') + P3('normal')},
    'ENTITY_MTEXT': {'use': 'production', 'fields': ENTITY_COMMON + [('contents', 'string', 'raw stored contents with format codes'), ('textHeight', 'double', ''), ('width', 'double', ''),
                                                                     ('rotation', 'double', ''), ('attachment', 'enum', 'stored name of the host AttachmentPoint'),
                                                                     ('flowDirection', 'enum', 'stored name of the host FlowDirection'), ('styleName', 'string', ''),
                                                                     ('lineSpacingFactor', 'double', ''), ('lineSpacingStyle', 'enum', 'stored name of the host LineSpacingStyle')] + P3('location') + P3('direction') + P3('normal')},
    'ENTITY_INSERT': {'use': 'production', 'fields': ENTITY_COMMON + [
        ('blockName', 'string', 'the definition name; for a dynamic reference, the name of its dynamic block definition (never the anonymous *U name); a reference to an anonymous definition that is not a dynamic-block representation is UNKNOWN (section 2.3.2)'),
        ('blockDigest', 'ref', 'reference to the DEFINITION_DUMP digest of that definition (nested-digest field); UNKNOWN when that definition is UNKNOWN (section 2.5, AR3-18)')] + P3('position') + P3('scale') + [
        ('rotation', 'double', '')] + P3('normal') + [
        ('dynamicProperties', 'dynprops', 'the WRITABLE dynamic properties of the reference (section 2.3.2), sorted by name; empty for a static reference'),
        ('attributes', 'seq:attrib', 'attribute references in host iteration order (exact host class AcDbAttribute, section 2.3.1)')]},
    'ENTITY_ATTRIB': {'use': 'production', 'fields': ENTITY_COMMON + [('tag', 'string', ''), ('textString', 'string', ''), ('height', 'double', ''), ('rotation', 'double', ''),
                                                                      ('widthFactor', 'double', ''), ('oblique', 'double', ''), ('styleName', 'string', ''),
                                                                      ('horizontalMode', 'enum', ''), ('verticalMode', 'enum', ''), ('invisible', 'bool', ''),
                                                                      ('lockPosition', 'bool', '')] + P3('position') + P3('alignment') + P3('normal')},
    'ENTITY_ATTDEF': {'use': 'production', 'fields': ENTITY_COMMON + [('tag', 'string', ''), ('prompt', 'string', ''), ('textString', 'string', 'default value'),
                                                                      ('height', 'double', ''), ('rotation', 'double', ''), ('widthFactor', 'double', ''), ('oblique', 'double', ''),
                                                                      ('styleName', 'string', ''), ('horizontalMode', 'enum', ''), ('verticalMode', 'enum', ''),
                                                                      ('invisible', 'bool', ''), ('constant', 'bool', ''), ('verify', 'bool', ''), ('preset', 'bool', ''),
                                                                      ('lockPosition', 'bool', '')] + P3('position') + P3('alignment') + P3('normal')},
    'ENTITY_ROTATED_DIMENSION': {'use': 'production', 'fields': ENTITY_COMMON + P3('xLine1Point') + P3('xLine2Point') + P3('dimLinePoint') + P3('textPosition') + [
        ('rotation', 'double', ''), ('oblique', 'double', ''), ('dimensionStyleName', 'string', ''), ('dimensionText', 'string', 'the text override as stored (empty = measured)'),
        ('usingDefaultTextPosition', 'bool', '')] + P3('normal') + [
        ('overrides', 'seq:rb', OVERRIDES_DESC)]},
    'ENTITY_ALIGNED_DIMENSION': {'use': 'production', 'fields': ENTITY_COMMON + P3('xLine1Point') + P3('xLine2Point') + P3('dimLinePoint') + P3('textPosition') + [
        ('oblique', 'double', ''), ('dimensionStyleName', 'string', ''), ('dimensionText', 'string', ''), ('usingDefaultTextPosition', 'bool', '')] + P3('normal') + [
        ('overrides', 'seq:rb', 'as ENTITY_ROTATED_DIMENSION (section 2.3.2)')]},
    'SYMBOL_RECORD_LAYER': {'use': 'production', 'fields': [('table', 'enum', 'LAYER'), ('name', 'string', 'stored case')] + COLOR_FIELDS + [
        ('linetype', 'string', 'linetype name'), ('lineweight', 'enum', ''), ('isOff', 'bool', ''), ('isFrozen', 'bool', ''), ('isLocked', 'bool', ''),
        ('isPlottable', 'bool', '')] + TRANSP},
    'SYMBOL_RECORD_LTYPE': {'use': 'production', 'fields': [('table', 'enum', 'LTYPE'), ('name', 'string', ''), ('asciiDescription', 'string', ''),
                                                            ('patternLength', 'double', ''), ('isScaledToFit', 'bool', ''), ('dashes', 'seq:record:LTYPE_DASH', 'in host order')]},
    'SYMBOL_RECORD_STYLE': {'use': 'production', 'fields': [('table', 'enum', 'STYLE'), ('name', 'string', ''), ('fileName', 'string', ''), ('bigFontFileName', 'string', ''),
                                                            ('textSize', 'double', ''), ('xScale', 'double', 'width factor'), ('obliquingAngle', 'double', ''),
                                                            ('isVertical', 'bool', ''), ('isShapeFile', 'bool', ''), ('flagBits', 'int', ''),
                                                            ('fontTypeface', 'string', ''), ('fontBold', 'bool', ''), ('fontItalic', 'bool', ''),
                                                            ('fontCharacterSet', 'int', ''), ('fontPitchAndFamily', 'int', '')]},
    'SYMBOL_RECORD_DIMSTYLE': {'use': 'production', 'fields': [('table', 'enum', 'DIMSTYLE'), ('name', 'string', ''),
                                                               ('dimvars', 'set:record:DIMVAR_VALUE@name', 'EVERY variable of the closed list of section 2.3.3 with its declared kind, sorted by name; a missing variable (unreadable) is UNKNOWN, an extra variable or another kind is REJECTED')]},
    'SYMBOL_RECORD_APPID': {'use': 'production', 'fields': [('table', 'enum', 'APPID'), ('name', 'string', '')]},
    'PAYLOAD_EXACT': {'use': 'production', 'fields': [('owner', 'string', 'the stored name of the definition whose extension dictionary holds the payload'),
                                                      ('dictKey', 'string', 'the extension-dictionary key (RACKCAD_SELECTIVE)'),
                                                      ('data', 'seq:rb', 'the stored ResultBuffer of the Xrecord: every (code, value) in host order, chunk boundaries and non-text entries preserved; absent when there is no payload')]},
    'NOD_ENTRY': {'use': 'production', 'fields': [('path', 'string', 'the dictionary keys from the named-object dictionary root, stored case, joined by "/"'),
                                                  ('objectClass', 'enum', 'derived from the exact host class of the object (section 2.3.1; never supplied): XRECORD for AcDbXrecord, DICTIONARY for AcDbDictionary; any other class (a subclass included) is UNKNOWN'),
                                                  ('data', 'seq:rb', 'XRECORD: its ResultBuffer (never absent); DICTIONARY: absent'),
                                                  ('children', 'set:string', 'DICTIONARY: the child keys (each child is its own NOD_ENTRY; never absent); XRECORD: absent')]},
    'ENUMERATION': {'use': 'production', 'fields': [('container', 'string', 'role name of the enumerated container'), ('ordered', 'bool', ''),
                                                    ('itemForm', 'enum', 'NAME, HANDLE or HANDLE_CLASS (section 2.3.6)'),
                                                    ('items', 'seq:string', 'the items in the item form; ordered = 1: host iteration order; ordered = 0: the encoder sorts them in ordinal (UTF-8) order and a duplicate item is UNKNOWN; handle forms are valid only within one session')]},
    'CONTEXT_VARIABLE': {'use': 'production', 'fields': [('name', 'string', 'the variable name, upper case, as the kind baseline lists it'),
                                                         ('value', 'hosttyped', 'the value returned by the system-variable read by name (section 2.1.3), as a TYPED_VALUE by its host type')]},
    'PRE_COMMAND_STATE_RECORD': {'use': 'production', 'fields': [('kind', 'string', 'the rack kind (selective)'),
                                                                 ('members', 'set:record:STATE_MEMBER@memberKey', 'every read-set component, every closure member and every context input (V2.1 6, 11.1, 11.3; section 2.3.5)')]},
    'PREPARE_RESIDUE_MANIFEST': {'use': 'production', 'fields': [('cloningMode', 'enum', 'the cloning mode the importer declared (stored name)'),
                                                                 ('importsCompleted', 'int', ''),
                                                                 ('additions', 'set:record:MANIFEST_ADDITION@memberKey', 'every record added by PREPARE-W with its exact digest'),
                                                                 ('reusedBound', 'set:record:MANIFEST_REUSED@memberKey', 'every pre-existing record the imported content binds to (REUSED_BOUND, not residue)')]},
    # ---- test-only records (exempt from section 2.5; used only by the vectors) ----
    'TEST_COORD3': {'use': 'test', 'fields': [('x', 'double', ''), ('y', 'double', ''), ('z', 'double', '')]},
    'TEST_NAME': {'use': 'test', 'fields': [('name', 'string', '')]},
    'TEST_OPTIONAL': {'use': 'test', 'fields': [('name', 'string', ''), ('layer', 'string', '')]},
    'TEST_SEQ': {'use': 'test', 'fields': [('items', 'seq:string', '')]},
    'TEST_SET': {'use': 'test', 'fields': [('names', 'set:string', '')]},
    'TEST_OUTER': {'use': 'test', 'fields': [('label', 'string', ''), ('inner', 'record:TEST_COORD3', '')]},
    'TEST_REFHOLDER': {'use': 'test', 'fields': [('ref', 'ref', '')]},
    'TEST_ORDER': {'use': 'test', 'fields': [('zeta', 'int', ''), ('alpha', 'int', '')]},
    'TEST_TYPED': {'use': 'test', 'fields': [('value', 'typed', '')]},
}
ENTITY_TYPES = sorted(k for k in SCHEMAS if k.startswith('ENTITY_'))
assert sorted(set(ENTITY_CLASS_MAP.values()) | set(ATTRIBUTE_CLASS_MAP.values())) == ENTITY_TYPES
assert sorted({r[1] for r in RECORD_KINDS}) == sorted({r[1] for r in RECORD_KINDS} & set(SCHEMAS))

# Closed list of dimension variables of a SYMBOL_RECORD_DIMSTYLE (section 2.3.3) and their typed-value kinds.
DIMVARS = {
    'DIMADEC': 'INT', 'DIMALT': 'BOOL', 'DIMALTD': 'INT', 'DIMALTF': 'DOUBLE', 'DIMALTMZF': 'DOUBLE', 'DIMALTMZS': 'STRING', 'DIMALTRND': 'DOUBLE',
    'DIMALTTD': 'INT', 'DIMALTTZ': 'INT', 'DIMALTU': 'INT', 'DIMALTZ': 'INT', 'DIMAPOST': 'STRING', 'DIMARCSYM': 'INT', 'DIMASZ': 'DOUBLE',
    'DIMATFIT': 'INT', 'DIMAUNIT': 'INT', 'DIMAZIN': 'INT', 'DIMBLK': 'NAME', 'DIMBLK1': 'NAME', 'DIMBLK2': 'NAME', 'DIMCEN': 'DOUBLE',
    'DIMCLRD': 'COLOR', 'DIMCLRE': 'COLOR', 'DIMCLRT': 'COLOR', 'DIMDEC': 'INT', 'DIMDLE': 'DOUBLE', 'DIMDLI': 'DOUBLE', 'DIMDSEP': 'INT',
    'DIMEXE': 'DOUBLE', 'DIMEXO': 'DOUBLE', 'DIMFRAC': 'INT', 'DIMFXL': 'DOUBLE', 'DIMFXLON': 'BOOL', 'DIMGAP': 'DOUBLE', 'DIMJOGANG': 'DOUBLE',
    'DIMJUST': 'INT', 'DIMLDRBLK': 'NAME', 'DIMLFAC': 'DOUBLE', 'DIMLIM': 'BOOL', 'DIMLTEX1': 'NAME', 'DIMLTEX2': 'NAME', 'DIMLTYPE': 'NAME',
    'DIMLUNIT': 'INT', 'DIMLWD': 'INT', 'DIMLWE': 'INT', 'DIMMZF': 'DOUBLE', 'DIMMZS': 'STRING', 'DIMPOST': 'STRING', 'DIMRND': 'DOUBLE',
    'DIMSAH': 'BOOL', 'DIMSCALE': 'DOUBLE', 'DIMSD1': 'BOOL', 'DIMSD2': 'BOOL', 'DIMSE1': 'BOOL', 'DIMSE2': 'BOOL', 'DIMSOXD': 'BOOL',
    'DIMTAD': 'INT', 'DIMTDEC': 'INT', 'DIMTFAC': 'DOUBLE', 'DIMTFILL': 'INT', 'DIMTFILLCLR': 'COLOR', 'DIMTIH': 'BOOL', 'DIMTIX': 'BOOL',
    'DIMTM': 'DOUBLE', 'DIMTMOVE': 'INT', 'DIMTOFL': 'BOOL', 'DIMTOH': 'BOOL', 'DIMTOL': 'BOOL', 'DIMTOLJ': 'INT', 'DIMTP': 'DOUBLE',
    'DIMTSZ': 'DOUBLE', 'DIMTVP': 'DOUBLE', 'DIMTXSTY': 'NAME', 'DIMTXT': 'DOUBLE', 'DIMTXTDIRECTION': 'BOOL', 'DIMTZIN': 'INT', 'DIMUPT': 'BOOL',
    'DIMZIN': 'INT',
}


SYMBOL_TABLE = {'SYMBOL_RECORD_LAYER': 'LAYER', 'SYMBOL_RECORD_LTYPE': 'LTYPE', 'SYMBOL_RECORD_STYLE': 'STYLE',
                'SYMBOL_RECORD_DIMSTYLE': 'DIMSTYLE', 'SYMBOL_RECORD_APPID': 'APPID'}


# ----------------------------------------------------------------------------------------------------------------
# Layer 2 (BA-05 V5 section 3.1, 3.1.2): the routing of every double of the typed forms to a predicate and a slot.
# No slot value lives here or in BA-05 (AR2-35, AR3-17); the slots are the eight of the kind baseline.
# ----------------------------------------------------------------------------------------------------------------
LAYER2_CLASSES = {
    'LENGTH': ['ABS_DIFF_LE(TOL_LENGTH)', 'TOL_LENGTH', 'positions, insertion points, origins, vertices, lengths, radii, widths, thicknesses and elevations'],
    'TEXT_HEIGHT': ['ABS_DIFF_LE(TOL_TEXT_HEIGHT)', 'TOL_TEXT_HEIGHT', 'text heights'],
    'DIM_POINT': ['ABS_DIFF_LE(TOL_DIM)', 'TOL_DIM', 'dimension points'],
    'ANGLE': ['ANGLE_DIFF_LE(TOL_ANGLE)', 'TOL_ANGLE, NORM_ANGLE', 'rotations and other angles'],
    'NORMAL': ['NORMAL_ANGLE_LE(TOL_NORMAL)', 'TOL_NORMAL', 'normals (the three components as one vector)'],
    'DIRECTION': ['DIRECTION_ANGLE_LE(TOL_ANGLE)', 'TOL_ANGLE', 'in-plane direction vectors (the three components as one vector)'],
    'SCALE': ['ABS_DIFF_LE(TOL_SCALE)', 'TOL_SCALE', 'the scale factors of a reference'],
    'EXACT_DOUBLE': ['EXACT_DOUBLE', 'none', 'dimensionless factors and every double that no other row routes'],
}
P3_ROUTE = {'start': 'LENGTH', 'end': 'LENGTH', 'center': 'LENGTH', 'position': 'LENGTH', 'alignment': 'LENGTH', 'location': 'LENGTH', 'origin': 'LENGTH',
            'xLine1Point': 'DIM_POINT', 'xLine2Point': 'DIM_POINT', 'dimLinePoint': 'DIM_POINT', 'textPosition': 'DIM_POINT',
            'normal': 'NORMAL', 'direction': 'DIRECTION', 'scale': 'SCALE'}
SCALAR_ROUTE = {'radius': 'LENGTH', 'thickness': 'LENGTH', 'elevation': 'LENGTH', 'width': 'LENGTH', 'patternLength': 'LENGTH', 'length': 'LENGTH',
                'shapeOffsetX': 'LENGTH', 'shapeOffsetY': 'LENGTH', 'startWidth': 'LENGTH', 'endWidth': 'LENGTH',
                'height': 'TEXT_HEIGHT', 'textHeight': 'TEXT_HEIGHT', 'textSize': 'TEXT_HEIGHT',
                'rotation': 'ANGLE', 'startAngle': 'ANGLE', 'endAngle': 'ANGLE', 'oblique': 'ANGLE', 'obliquingAngle': 'ANGLE', 'shapeRotation': 'ANGLE',
                'linetypeScale': 'EXACT_DOUBLE', 'widthFactor': 'EXACT_DOUBLE', 'xScale': 'EXACT_DOUBLE', 'lineSpacingFactor': 'EXACT_DOUBLE',
                'bulge': 'EXACT_DOUBLE', 'shapeScale': 'EXACT_DOUBLE'}
RECORD_SCALAR_ROUTE = {('PLINE_VERTEX', 'x'): 'LENGTH', ('PLINE_VERTEX', 'y'): 'LENGTH'}
BY_CONTEXT_RECORDS = ('POINT3D',)  # a POINT3D value is compared by the class of the value that carries it (section 3.1)


def layer2_routes():
    """(record, field) -> class for every double field of the production and helper records; asserts totality."""
    routes = {}
    for rtype, s in SCHEMAS.items():
        if s['use'] == 'test' or rtype in BY_CONTEXT_RECORDS:
            continue
        names = {f[0]: f[1] for f in s['fields']}
        for fname, kind in names.items():
            if kind != 'double':
                continue
            cls = RECORD_SCALAR_ROUTE.get((rtype, fname))
            if cls is None and fname[-1:] in ('X', 'Y', 'Z') and all(names.get(fname[:-1] + c) == 'double' for c in 'XYZ'):
                cls = P3_ROUTE.get(fname[:-1])
            if cls is None:
                cls = SCALAR_ROUTE.get(fname)
            assert cls in LAYER2_CLASSES, ('layer-2 routing is not total', rtype, fname)
            routes[(rtype, fname)] = cls
    return routes


LAYER2_ROUTES = layer2_routes()

# The class of every DOUBLE variable of the closed list (section 2.3.3); every other variable kind is EXACT (NAME: EXACT_STRING).
DIMVAR_CLASSES = {
    'DIMTXT': 'TEXT_HEIGHT',
    'DIMASZ': 'LENGTH', 'DIMCEN': 'LENGTH', 'DIMDLE': 'LENGTH', 'DIMDLI': 'LENGTH', 'DIMEXE': 'LENGTH', 'DIMEXO': 'LENGTH', 'DIMFXL': 'LENGTH',
    'DIMGAP': 'LENGTH', 'DIMTSZ': 'LENGTH', 'DIMTP': 'LENGTH', 'DIMTM': 'LENGTH', 'DIMRND': 'LENGTH',
    'DIMJOGANG': 'ANGLE',
    'DIMSCALE': 'EXACT_DOUBLE', 'DIMLFAC': 'EXACT_DOUBLE', 'DIMTFAC': 'EXACT_DOUBLE', 'DIMALTF': 'EXACT_DOUBLE', 'DIMMZF': 'EXACT_DOUBLE',
    'DIMALTMZF': 'EXACT_DOUBLE', 'DIMTVP': 'EXACT_DOUBLE', 'DIMALTRND': 'EXACT_DOUBLE',
}
assert sorted(DIMVAR_CLASSES) == sorted(k for k, v in DIMVARS.items() if v == 'DOUBLE'), 'DIMVAR classes are not total over the DOUBLE variables'


def check_record(rtype, rec):
    """Record-level rules of section 2.3 (conditional presence, closed lists, key forms). A violated rule raises Rejected;
    an UNKNOWN condition is deferred (REJECTED takes precedence, section 2.1)."""
    if 'colorMethod' in rec and (rec['colorMethod'] == 'ByColor') != (rec['colorRgb'] is not None):
        raise Rejected('%s: colorRgb is present exactly when colorMethod = ByColor' % rtype)
    if 'transparencyMethod' in rec and (rec['transparencyMethod'] == 'ByAlpha') != (rec['transparencyAlpha'] is not None):
        raise Rejected('%s: transparencyAlpha is present exactly when transparencyMethod = ByAlpha' % rtype)
    if rtype in SYMBOL_TABLE and rec['table'] != SYMBOL_TABLE[rtype]:
        raise Rejected('%s: table must be %s' % (rtype, SYMBOL_TABLE[rtype]))
    if rtype == 'SYMBOL_RECORD_DIMSTYLE':
        names = []
        for dv in rec['dimvars']:
            n = dv.get('name') if isinstance(dv, dict) else None
            if n not in DIMVARS:
                raise Rejected('SYMBOL_RECORD_DIMSTYLE: %r is not a variable of the closed list (section 2.3.3)' % (n,))
            val = dv.get('value')
            if not (isinstance(val, (list, tuple)) and len(val) == 2 and val[0] == DIMVARS[n]):
                raise Rejected('SYMBOL_RECORD_DIMSTYLE: %s must be a TYPED_VALUE of kind %s (section 2.3.3)' % (n, DIMVARS[n]))
            names.append(n)
        missing = sorted(set(DIMVARS) - set(names))
        if missing:
            defer('SYMBOL_RECORD_DIMSTYLE: variable(s) %s of the closed list missing (the host could not read them): no digest (section 2.3.3)' % ', '.join(missing))
    if rtype == 'NOD_ENTRY':
        if rec['objectClass'] == 'XRECORD' and (rec['data'] is None or rec['children'] is not None):
            raise Rejected('NOD_ENTRY XRECORD: data must be present and children absent')
        if rec['objectClass'] == 'DICTIONARY' and (rec['data'] is not None or rec['children'] is None):
            raise Rejected('NOD_ENTRY DICTIONARY: data must be absent and children present')
    if rtype in ('ENTITY_ROTATED_DIMENSION', 'ENTITY_ALIGNED_DIMENSION') and rec['overrides'] is None:
        raise Rejected('%s: overrides is never absent (empty when there is no DSTYLE section)' % rtype)
    if rtype == 'ENTITY_INSERT' and isinstance(rec['blockName'], str) and rec['blockName'].startswith('*'):
        defer('ENTITY_INSERT: the definition %s is anonymous and not a dynamic-block representation: UNKNOWN (section 2.3.2)' % rec['blockName'])
    if rtype == 'ENUMERATION':
        form = rec['itemForm']
        if form not in ITEM_FORM_RE:
            raise Rejected('ENUMERATION: unknown item form %s' % form)
        pattern = ITEM_FORM_RE[form]
        for item in rec['items']:
            if not isinstance(item, str):
                raise Rejected('ENUMERATION: items are strings')
            if pattern is not None and not re.match(pattern, item):
                raise Rejected('ENUMERATION: item %r is not in the form %s' % (item, form))
    if rtype == 'STATE_MEMBER':
        if not all(isinstance(rec[k], str) for k in ('memberKey', 'category', 'recordKind', 'expectedName', 'state')):
            raise Rejected('STATE_MEMBER: memberKey, category, recordKind, expectedName and state are strings')
        if rec['category'] not in CATEGORIES or rec['recordKind'] not in RECORD_KIND_NAMES:
            raise Rejected('STATE_MEMBER: category or recordKind outside section 2.3.5')
        if rec['memberKey'] != rec['category'] + '|' + rec['recordKind'] + '|' + rec['expectedName']:
            raise Rejected('STATE_MEMBER: memberKey is not category|recordKind|expectedName')
        if rec['state'] not in STATES:
            raise Rejected('STATE_MEMBER: state %r is not PRESENT, ABSENT or UNKNOWN' % rec['state'])
        if (rec['state'] == 'PRESENT') != (rec['digest'] is not None):
            raise Rejected('STATE_MEMBER: digest is present exactly when the state is PRESENT')
        if (rec['state'] == 'PRESENT' and rec['recordKind'] in NAMED_KINDS) != (rec['storedName'] is not None):
            raise Rejected('STATE_MEMBER: storedName is present exactly when the member is PRESENT and its record kind is named (section 2.3.5)')
        if rec['recordKind'] == 'REFERENCE' and not re.match(HANDLE_ONLY, rec['expectedName']):
            raise Rejected('STATE_MEMBER: the expectedName of a REFERENCE member is a handle in the item form HANDLE (section 2.3.5)')
    if rtype in ('MANIFEST_ADDITION', 'MANIFEST_REUSED'):
        if rec['recordKind'] not in RECORD_KIND_NAMES:
            raise Rejected('%s: recordKind outside section 2.3.5' % rtype)
        parts = rec['memberKey'].split('|', 2) if isinstance(rec['memberKey'], str) else []
        if len(parts) != 3 or parts[0] != 'CLOSURE' or parts[1] != rec['recordKind'] or parts[2] == '':
            raise Rejected('%s: memberKey must be CLOSURE|<recordKind>|<expectedName> of the corresponding closure member, with recordKind %s' % (rtype, rec['recordKind']))
        if rec['storedName'] is None:
            raise Rejected('%s: storedName is never absent' % rtype)
    if rtype == 'PRE_COMMAND_STATE_RECORD' and any(isinstance(m, dict) and m.get('state') == 'UNKNOWN' for m in rec['members']):
        defer('a member of the PRE_COMMAND_STATE_RECORD is UNKNOWN: no partial digest (refusal O1, V2.1 6)')


def enc_fields(rtype, rec):
    if rtype not in SCHEMAS:
        raise Unknown('record type %s is not in the specification' % rtype)
    schema = SCHEMAS[rtype]['fields']
    names = [f[0] for f in schema]
    assert len(names) == len(set(names)), rtype + ' has duplicate field names'
    if set(rec) != set(names):
        raise Rejected('%s: fields %s expected, got %s' % (rtype, sorted(names), sorted(rec)))
    check_record(rtype, rec)
    items = dict(rec)
    if rtype == 'ENUMERATION' and not rec['ordered']:
        try:
            keys = [order_key(e) for e in rec['items']]
            if len(set(keys)) != len(keys):
                defer('duplicate item in an unordered ENUMERATION')
            else:
                items['items'] = [e for _, e in sorted(zip(keys, rec['items']), key=lambda t: t[0])]
        except _Skip:
            pass
    out = b''
    for name, kind, _ in sorted(schema, key=lambda f: f[0].encode('ascii')):
        try:
            out += name.encode('ascii') + b'=' + enc_value(kind, items[name]) + b'\n'
        except _Skip:
            out += name.encode('ascii') + b'=?\n'
    return out


def nod_from_host(inp):
    """A NOD object is supplied as [host class, fields without objectClass]; objectClass is derived by NOD_CLASS_MAP (section 2.3.1)."""
    if not (isinstance(inp, (list, tuple)) and len(inp) == 2 and isinstance(inp[1], dict)):
        raise Rejected('NOD_ENTRY input is [host class, fields] (section 2.7)')
    cls, fields = inp
    if 'objectClass' in fields:
        raise Rejected('NOD_ENTRY: objectClass is derived from the exact host class (section 2.3.1), never supplied')
    expected = sorted(f[0] for f in SCHEMAS['NOD_ENTRY']['fields'] if f[0] != 'objectClass')
    if sorted(fields) != expected:
        raise Rejected('NOD_ENTRY: fields %s expected, got %s' % (expected, sorted(fields)))
    if cls not in NOD_CLASS_MAP:
        raise Unknown('NOD object of host class %s: only AcDbXrecord and AcDbDictionary (exact class, section 2.3.1) are supported' % cls)
    return dict(fields, objectClass=NOD_CLASS_MAP[cls])


def stream(rtype, rec):
    del DEFERRED[:]
    if rtype == 'NOD_ENTRY':
        rec = nod_from_host(rec)
    s = PREFIX + rtype.encode('ascii') + b'|' + enc_fields(rtype, rec)
    if DEFERRED:
        raise Unknown(DEFERRED[0])
    return s


def show(b):
    """Display form only (informative): backslash doubled, LF shown as \\n. The authoritative bytes are streamHex."""
    return b.decode('utf-8', errors='backslashreplace').replace('\\', '\\\\').replace('\n', '\\n')


def jin(v):
    """Producer-side input as JSON (section 2.7): doubles tagged as binary64 hex, host points tagged, ill-formed strings as UTF-16 units."""
    if isinstance(v, bool) or v is None or isinstance(v, int):
        return v
    if isinstance(v, float):
        return {'binary64': struct.pack('>d', v).hex()}
    if isinstance(v, str):
        try:
            v.encode('utf-8')
            return v
        except UnicodeEncodeError:
            return {'utf16': [ord(c) for c in v]}
    if isinstance(v, bytes):
        return {'bytes': v.hex()}
    if isinstance(v, Point3d):
        return {'point3d': [jin(v.x), jin(v.y), jin(v.z)]}
    if isinstance(v, Point2d):
        return {'point2d': [jin(v.x), jin(v.y)]}
    if isinstance(v, dict):
        return {k: jin(x) for k, x in v.items()}
    if isinstance(v, (list, tuple)):
        return [jin(x) for x in v]
    raise TypeError(type(v))


vectors = []
rejections = []


def add(vid, purpose, rtype, rec, expect='DIGEST'):
    entry = {'id': vid, 'purpose': purpose, 'recordType': rtype}
    try:
        s = stream(rtype, rec)
    except Unknown as e:
        if expect != 'UNKNOWN':
            raise
        entry.update({'result': 'UNKNOWN', 'reason': str(e), 'input': jin(rec)})
        vectors.append(entry)
        return None
    if expect != 'DIGEST':
        raise SystemExit(vid + ' expected ' + expect)
    d = hashlib.sha256(s).hexdigest()
    entry.update({'result': 'DIGEST', 'input': jin(rec), 'streamHex': s.hex(), 'streamDisplay': show(s), 'sha256': d})
    vectors.append(entry)
    return d


def reject(rid, purpose, rtype, rec):
    try:
        stream(rtype, rec)
    except Rejected as e:
        rejections.append({'id': rid, 'purpose': purpose, 'recordType': rtype, 'result': 'REJECTED', 'reason': str(e), 'input': jin(rec)})
        return
    raise SystemExit(rid + ' expected REJECTED')


def coord(x, y, z):
    return {'x': x, 'y': y, 'z': z}


def pt(prefix, x, y, z):
    return {prefix + 'X': x, prefix + 'Y': y, prefix + 'Z': z}


def common(dxf, layer='RACKCAD_ESTRUCTURA'):
    return {'dxfName': dxf, 'layer': layer, 'linetype': 'ByLayer', 'linetypeScale': 1.0, 'lineweight': 'ByLayer', 'visible': True,
            'colorMethod': 'ByLayer', 'colorIndex': 256, 'colorRgb': None, 'colorBookName': None, 'colorName': None,
            'transparencyMethod': 'ByLayer', 'transparencyAlpha': None}


# ---------------- family vectors (EV-01..EV-20) ----------------
C3 = 'TEST_COORD3'
d1 = add('EV-01', 'baseline coordinate (+0.0 components)', C3, coord(1.0, 0.0, 0.0))
d2 = add('EV-02', 'SIGNED ZERO: y = -0.0 must differ from EV-01', C3, coord(1.0, -0.0, 0.0))
d3 = add('EV-03', '1 ulp above 1.0 must differ from EV-01', C3, coord(math.nextafter(1.0, 2.0), 0.0, 0.0))
add('EV-04', 'SUBNORMAL: smallest positive subnormal', C3, coord(5e-324, 0.0, 0.0))
add('EV-05', 'MAXIMUM FINITE double', C3, coord(1.7976931348623157e308, 0.0, 0.0))
add('EV-06', 'NaN is unsupported: UNKNOWN, no digest', C3, coord(float('nan'), 0.0, 0.0), expect='UNKNOWN')
add('EV-07', 'infinity is unsupported: UNKNOWN, no digest', C3, coord(float('inf'), 0.0, 0.0), expect='UNKNOWN')
d8 = add('EV-08', 'string, exact case', 'TEST_NAME', {'name': 'Rack N 1'})
d9 = add('EV-09', 'string, case differs from EV-08: must differ', 'TEST_NAME', {'name': 'rack N 1'})
add('EV-10', 'EMPTY STRING', 'TEST_NAME', {'name': ''})
add('EV-11', 'MULTIBYTE string (UTF-8, no normalization)', 'TEST_NAME', {'name': 'Ñandú 日本 €'})
add('EV-12', 'ABSENT field value (field layer streams before name: ordinal field order)', 'TEST_OPTIONAL', {'name': 'Rack', 'layer': None})
d13 = add('EV-13', 'ORDERED SEQUENCE (host order is state)', 'TEST_SEQ', {'items': ['b', 'a']})
d14 = add('EV-14', 'ordered sequence in the other order: must differ from EV-13', 'TEST_SEQ', {'items': ['a', 'b']})
d15 = add('EV-15', 'UNORDERED container given as b,a: sorted by stored name', 'TEST_SET', {'names': ['b', 'a']})
d16 = add('EV-16', 'unordered container given as a,b: must EQUAL EV-15', 'TEST_SET', {'names': ['a', 'b']})
add('EV-17', 'NESTED record inline (field inner streams before label)', 'TEST_OUTER', {'label': 'outer', 'inner': coord(1.0, 2.0, 3.0)})
add('EV-18', 'NESTED definition referenced by name and digest', 'TEST_REFHOLDER', {'ref': ('SEL_FRONTAL_Post_0', d1)})
PAY_KNOWN = '{"Id":"g-1","Kind":"selective","SchemaVersion":"1.0"}'
PAY_UNKNOWN = '{"Id":"g-1","Kind":"selective","SchemaVersion":"1.0","futureMember":1}'
d19 = add('EV-19', 'PAYLOAD_EXACT, one text chunk (code 1), no unknown member', 'PAYLOAD_EXACT',
          {'owner': 'SEL_FRONTAL_0', 'dictKey': 'RACKCAD_SELECTIVE', 'data': [(1, PAY_KNOWN)]})
d20 = add('EV-20', 'PAYLOAD_EXACT with an UNKNOWN / FORWARD-COMPATIBLE member: must differ from EV-19', 'PAYLOAD_EXACT',
          {'owner': 'SEL_FRONTAL_0', 'dictKey': 'RACKCAD_SELECTIVE', 'data': [(1, PAY_UNKNOWN)]})

# ---------------- rule-discriminating vectors ----------------
d21 = add('EV-21', 'FIELD ORDER: fields supplied as (zeta, alpha) stream as alpha, zeta', 'TEST_ORDER', {'zeta': 1, 'alpha': 2})
d22 = add('EV-22', 'fields supplied as (alpha, zeta): must EQUAL EV-21 (ordinal field order, not supply or schema order)', 'TEST_ORDER', {'alpha': 2, 'zeta': 1})
d23 = add('EV-23', 'CASE-MIXED unordered set b,B,a,A: sorted A,B,a,b (UTF-8 byte order)', 'TEST_SET', {'names': ['b', 'B', 'a', 'A']})
d24 = add('EV-24', 'U+1F600 and U+FF21: UTF-8 order puts U+FF21 first (UTF-16 code-unit order would put U+1F600 first)', 'TEST_SET', {'names': ['\U0001F600', 'Ａ']})
add('EV-25', 'LONE SURROGATE (ill-formed UTF-16): UNKNOWN, no digest', 'TEST_NAME', {'name': 'a\ud800b'}, expect='UNKNOWN')
d26 = add('EV-26', 'PAYLOAD_EXACT, same text split in two chunks ("ab" + "c"): must differ from one chunk', 'PAYLOAD_EXACT',
          {'owner': 'SEL_FRONTAL_0', 'dictKey': 'RACKCAD_SELECTIVE', 'data': [(1, 'ab'), (1, 'c')]})
d27 = add('EV-27', 'PAYLOAD_EXACT, one chunk "abc"', 'PAYLOAD_EXACT', {'owner': 'SEL_FRONTAL_0', 'dictKey': 'RACKCAD_SELECTIVE', 'data': [(1, 'abc')]})
d28 = add('EV-28', 'PAYLOAD_EXACT, same text with type code 300 instead of 1: must differ from EV-27', 'PAYLOAD_EXACT',
          {'owner': 'SEL_FRONTAL_0', 'dictKey': 'RACKCAD_SELECTIVE', 'data': [(300, 'abc')]})
add('EV-29', 'PAYLOAD_EXACT, no payload: data absent', 'PAYLOAD_EXACT', {'owner': 'SEL_FRONTAL_0', 'dictKey': 'RACKCAD_SELECTIVE', 'data': None})
add('EV-30', 'PAYLOAD_EXACT with a handle-valued entry (code 1005): UNKNOWN', 'PAYLOAD_EXACT',
    {'owner': 'SEL_FRONTAL_0', 'dictKey': 'RACKCAD_SELECTIVE', 'data': [(1, 'x'), (1005, '1F')]}, expect='UNKNOWN')
add('EV-31', 'PAYLOAD_EXACT whose chunking split a surrogate pair (RackBlockData chunks at 255 UTF-16 units): UNKNOWN', 'PAYLOAD_EXACT',
    {'owner': 'SEL_FRONTAL_0', 'dictKey': 'RACKCAD_SELECTIVE', 'data': [(1, 'a\ud83d'), (1, '\ude00b')]}, expect='UNKNOWN')

# ---------------- one vector per production record type ----------------
line = common('LINE')
line.update(pt('start', 0.0, 0.0, 0.0)); line.update(pt('end', 42.0, 0.0, 0.0)); line.update(pt('normal', 0.0, 0.0, 1.0)); line['thickness'] = 0.0
dl = add('EV-40', 'ENTITY_LINE', 'ENTITY_LINE', line)
arc = common('ARC')
arc.update(pt('center', 1.0, 2.0, 0.0)); arc.update({'radius': 3.0, 'startAngle': 0.0, 'endAngle': math.pi / 2}); arc.update(pt('normal', 0.0, 0.0, 1.0)); arc['thickness'] = 0.0
add('EV-41', 'ENTITY_ARC', 'ENTITY_ARC', arc)
cir = common('CIRCLE')
cir.update(pt('center', 1.0, 2.0, 0.0)); cir['radius'] = 0.5; cir.update(pt('normal', 0.0, 0.0, 1.0)); cir['thickness'] = 0.0
add('EV-42', 'ENTITY_CIRCLE', 'ENTITY_CIRCLE', cir)
pl = common('LWPOLYLINE')
pl.update({'closed': True, 'plinegen': False, 'elevation': 0.0, 'thickness': 0.0}); pl.update(pt('normal', 0.0, 0.0, 1.0))
pl['vertices'] = [{'x': 0.0, 'y': 0.0, 'bulge': 0.0, 'startWidth': 0.0, 'endWidth': 0.0}, {'x': 3.0, 'y': 0.0, 'bulge': 0.0, 'startWidth': 0.0, 'endWidth': 0.0},
                  {'x': 3.0, 'y': 96.0, 'bulge': 0.0, 'startWidth': 0.0, 'endWidth': 0.0}]
add('EV-43', 'ENTITY_LWPOLYLINE', 'ENTITY_LWPOLYLINE', pl)
tx = common('TEXT', 'RACKCAD_ANOTACIONES')
tx.update({'textString': 'NIVEL 1', 'height': 4.0, 'rotation': 0.0, 'widthFactor': 1.0, 'oblique': 0.0, 'styleName': 'Standard', 'horizontalMode': 'TextCenter',
           'verticalMode': 'TextBase', 'mirroredInX': False, 'mirroredInY': False, 'thickness': 0.0})
tx.update(pt('position', 10.0, 20.0, 0.0)); tx.update(pt('alignment', 10.0, 20.0, 0.0)); tx.update(pt('normal', 0.0, 0.0, 1.0))
add('EV-44', 'ENTITY_TEXT', 'ENTITY_TEXT', tx)
mt = common('MTEXT', 'RACKCAD_ANOTACIONES')
mt.update({'contents': 'FONDO 1\\PNIVEL 2', 'textHeight': 4.0, 'width': 0.0, 'rotation': 0.0, 'attachment': 'TopLeft', 'flowDirection': 'LeftToRight',
           'styleName': 'Standard', 'lineSpacingFactor': 1.0, 'lineSpacingStyle': 'AtLeast'})
mt.update(pt('location', 0.0, 0.0, 0.0)); mt.update(pt('direction', 1.0, 0.0, 0.0)); mt.update(pt('normal', 0.0, 0.0, 1.0))
add('EV-45', 'ENTITY_MTEXT', 'ENTITY_MTEXT', mt)
att = common('ATTRIB')
att.update({'tag': 'CODE', 'textString': 'P-1', 'height': 2.0, 'rotation': 0.0, 'widthFactor': 1.0, 'oblique': 0.0, 'styleName': 'Standard',
            'horizontalMode': 'TextLeft', 'verticalMode': 'TextBase', 'invisible': False, 'lockPosition': False})
att.update(pt('position', 0.0, 0.0, 0.0)); att.update(pt('alignment', 0.0, 0.0, 0.0)); att.update(pt('normal', 0.0, 0.0, 1.0))
add('EV-46', 'ENTITY_ATTRIB', 'ENTITY_ATTRIB', att)
ad = common('ATTDEF')
ad.update({'tag': 'CODE', 'prompt': 'Code', 'textString': '', 'height': 2.0, 'rotation': 0.0, 'widthFactor': 1.0, 'oblique': 0.0, 'styleName': 'Standard',
           'horizontalMode': 'TextLeft', 'verticalMode': 'TextBase', 'invisible': False, 'constant': False, 'verify': False, 'preset': False, 'lockPosition': False})
ad.update(pt('position', 0.0, 0.0, 0.0)); ad.update(pt('alignment', 0.0, 0.0, 0.0)); ad.update(pt('normal', 0.0, 0.0, 1.0))
add('EV-47', 'ENTITY_ATTDEF', 'ENTITY_ATTDEF', ad)
# nested definition and a reference to it
nested = {'name': 'POSTE_3X3', 'originX': 0.0, 'originY': 0.0, 'originZ': 0.0, 'explodable': True, 'scaling': 'Any', 'units': 'Inches', 'annotative': 'NotApplicable',
          'entities': [('AcDbLine', line)]}
dn = add('EV-48', 'DEFINITION_DUMP (one LINE, host class AcDbLine)', 'DEFINITION_DUMP', nested)
ORIGIN_RO = Point3d(0.0, 0.0, 0.0)
ins = common('INSERT')
ins.update({'blockName': 'POSTE_3X3', 'blockDigest': ('POSTE_3X3', dn), 'rotation': 0.0, 'attributes': [],
            'dynamicProperties': [{'name': 'LONGITUD', 'readOnly': False, 'value': 96.0}, {'name': 'FRENTE', 'readOnly': False, 'value': 42.0},
                                  {'name': 'Origin', 'readOnly': True, 'value': ORIGIN_RO}, {'name': 'Origin', 'readOnly': True, 'value': ORIGIN_RO}]})
ins.update(pt('position', 0.0, 0.0, 0.0)); ins.update(pt('scale', 1.0, 1.0, 1.0)); ins.update(pt('normal', 0.0, 0.0, 1.0))
di = add('EV-49', 'ENTITY_INSERT with nested-digest field and dynamic properties: only the WRITABLE ones (FRENTE before LONGITUD); '
                  'two read-only Origin entries with the same name are not fingerprinted and do not make it UNKNOWN', 'ENTITY_INSERT', ins)
outer = {'name': 'SEL_FRONTAL_0', 'originX': 0.0, 'originY': 0.0, 'originZ': 0.0, 'explodable': True, 'scaling': 'Any', 'units': 'Inches', 'annotative': 'NotApplicable',
         'entities': [('AcDbBlockReference', ins), ('AcDbText', tx)]}
d50 = add('EV-50', 'DEFINITION_DUMP holding an INSERT (nested definition by reference) and a TEXT, in host iteration order', 'DEFINITION_DUMP', outer)
add('EV-51', 'DEFINITION_DUMP holding an entity of a class outside the map (a proxy, AcDbProxyEntity): UNKNOWN', 'DEFINITION_DUMP',
    dict(outer, entities=[('AcDbProxyEntity', {})]), expect='UNKNOWN')
dim = common('DIMENSION', 'RACKCAD_COTAS')
dim.update(pt('xLine1Point', 0.0, 0.0, 0.0)); dim.update(pt('xLine2Point', 96.0, 0.0, 0.0)); dim.update(pt('dimLinePoint', 0.0, -12.0, 0.0)); dim.update(pt('textPosition', 48.0, -12.0, 0.0))
dim.update({'rotation': 0.0, 'oblique': 0.0, 'dimensionStyleName': 'Standard', 'dimensionText': '', 'usingDefaultTextPosition': True,
            'overrides': [(1070, 140), (1040, 4.0), (1070, 41), (1040, 2.8)]})
dim.update(pt('normal', 0.0, 0.0, 1.0))
add('EV-52', 'ENTITY_ROTATED_DIMENSION; overrides = the DSTYLE pairs as stored, in host order (illustrative pairs)', 'ENTITY_ROTATED_DIMENSION', dim)
adim = dict(dim)
del adim['rotation']
add('EV-53', 'ENTITY_ALIGNED_DIMENSION', 'ENTITY_ALIGNED_DIMENSION', adim)
lay = {'table': 'LAYER', 'name': 'RACKCAD_ANOTACIONES', 'colorMethod': 'ByAci', 'colorIndex': 2, 'colorRgb': None, 'colorBookName': None, 'colorName': None,
       'linetype': 'Continuous', 'lineweight': 'ByLineWeightDefault', 'isOff': False, 'isFrozen': False, 'isLocked': False, 'isPlottable': True,
       'transparencyMethod': 'ByAlpha', 'transparencyAlpha': 255}
d54 = add('EV-54', 'SYMBOL_RECORD_LAYER (RACKCAD_ANOTACIONES; the read-set layer of EV-62)', 'SYMBOL_RECORD_LAYER', lay)
lt = {'table': 'LTYPE', 'name': 'DASHED', 'asciiDescription': '__ __ __', 'patternLength': 0.75, 'isScaledToFit': False,
      'dashes': [{'length': 0.5, 'shapeNumber': 0, 'shapeStyleName': None, 'shapeOffsetX': 0.0, 'shapeOffsetY': 0.0, 'shapeRotation': 0.0, 'shapeScale': 0.0,
                  'shapeIsUcsOriented': False, 'text': None},
                 {'length': -0.25, 'shapeNumber': 0, 'shapeStyleName': None, 'shapeOffsetX': 0.0, 'shapeOffsetY': 0.0, 'shapeRotation': 0.0, 'shapeScale': 0.0,
                  'shapeIsUcsOriented': False, 'text': None}]}
add('EV-55', 'SYMBOL_RECORD_LTYPE', 'SYMBOL_RECORD_LTYPE', lt)
st = {'table': 'STYLE', 'name': 'Standard', 'fileName': 'txt', 'bigFontFileName': '', 'textSize': 0.0, 'xScale': 1.0, 'obliquingAngle': 0.0, 'isVertical': False,
      'isShapeFile': False, 'flagBits': 0, 'fontTypeface': '', 'fontBold': False, 'fontItalic': False, 'fontCharacterSet': 0, 'fontPitchAndFamily': 0}
add('EV-56', 'SYMBOL_RECORD_STYLE', 'SYMBOL_RECORD_STYLE', st)


def dimvar_value(name, kind):
    return {'BOOL': ('BOOL', False), 'INT': ('INT', 0), 'DOUBLE': ('DOUBLE', 0.18), 'STRING': ('STRING', ''), 'NAME': ('NAME', ''),
            'COLOR': ('COLOR', {'colorMethod': 'ByBlock', 'colorIndex': 0, 'colorRgb': None, 'colorBookName': None, 'colorName': None})}[kind]


ds = {'table': 'DIMSTYLE', 'name': 'Standard', 'dimvars': [{'name': n, 'value': dimvar_value(n, k)} for n, k in DIMVARS.items()]}
add('EV-57', 'SYMBOL_RECORD_DIMSTYLE with every variable of the closed list (illustrative values)', 'SYMBOL_RECORD_DIMSTYLE', ds)
add('EV-58', 'SYMBOL_RECORD_APPID', 'SYMBOL_RECORD_APPID', {'table': 'APPID', 'name': 'ACAD'})
d59 = add('EV-59', 'NOD_ENTRY, an XRECORD (RACKCAD_PROJECT, as the product writes it: text chunks of code 1); input [host class AcDbXrecord, fields]', 'NOD_ENTRY',
          ('AcDbXrecord', {'path': 'RACKCAD_PROJECT', 'data': [(1, '{"Variables":[]}')], 'children': None}))
add('EV-60', 'NOD_ENTRY, a DICTIONARY: the host dictionary ACAD_GROUP with two synthetic group keys (children sorted); input [host class AcDbDictionary, fields]', 'NOD_ENTRY',
    ('AcDbDictionary', {'path': 'ACAD_GROUP', 'data': None, 'children': ['b', 'A']}))
d61 = add('EV-61', 'ENUMERATION, unordered names given sorted', 'ENUMERATION',
          {'container': 'BLOCK_TABLE_NAMES', 'ordered': False, 'itemForm': 'NAME', 'items': ['POSTE_3X3', 'SEL_FRONTAL_0']})

# ---------------- V4 vectors (BA05V3-N04..N08, N11; AR3-18, AR3-21) ----------------
add('EV-65', 'DEFINITION_DUMP holding an AcDbMInsertBlock (DXF name INSERT, a subclass of AcDbBlockReference): UNKNOWN by the exact-class map', 'DEFINITION_DUMP',
    dict(outer, entities=[('AcDbMInsertBlock', ins)]), expect='UNKNOWN')
add('EV-66', 'DEFINITION_DUMP holding an AcDbAttributeDefinition (a subclass of AcDbText): ENTITY_ATTDEF by the exact-class map, never ENTITY_TEXT', 'DEFINITION_DUMP',
    dict(nested, name='POSTE_CON_ATRIBUTO', entities=[('AcDbAttributeDefinition', ad)]))
ins_unknown = dict(ins, blockDigest=('POSTE_PROXY', 'UNKNOWN'), blockName='POSTE_PROXY')
add('EV-67', 'ENTITY_INSERT whose referenced definition is UNKNOWN: UNKNOWN (propagation through blockDigest, AR3-18)', 'ENTITY_INSERT', ins_unknown, expect='UNKNOWN')
add('EV-68', 'DEFINITION_DUMP holding the reference of EV-67: UNKNOWN (propagates to every definition that references it, AR3-18)', 'DEFINITION_DUMP',
    dict(outer, entities=[('AcDbBlockReference', ins_unknown), ('AcDbText', tx)]), expect='UNKNOWN')
ins_host = dict(ins, dynamicProperties=[{'name': 'Visibilidad', 'readOnly': False, 'value': 'Tipo A'}, {'name': 'Volteo', 'readOnly': False, 'value': 1},
                                        {'name': 'LONGITUD', 'readOnly': False, 'value': 96.0}, {'name': 'Origin', 'readOnly': True, 'value': ORIGIN_RO}])
add('EV-69', 'ENTITY_INSERT with writable dynamic properties of host types String, Int16 and Double (synthetic names): STRING, INT, DOUBLE (section 2.1.3)', 'ENTITY_INSERT', ins_host)
add('EV-70', 'ENTITY_INSERT with two WRITABLE dynamic properties of the same name: UNKNOWN (duplicate key)', 'ENTITY_INSERT',
    dict(ins, dynamicProperties=[{'name': 'LONGITUD', 'readOnly': False, 'value': 96.0}, {'name': 'LONGITUD', 'readOnly': False, 'value': 48.0}]), expect='UNKNOWN')
add('EV-71', 'ENTITY_INSERT with a writable dynamic property whose host value is a Point2d: UNKNOWN (no typed-value kind)', 'ENTITY_INSERT',
    dict(ins, dynamicProperties=[{'name': 'Punto', 'readOnly': False, 'value': Point2d(1.0, 2.0)}]), expect='UNKNOWN')
add('EV-72', 'unordered set with a DUPLICATE member: UNKNOWN', 'TEST_SET', {'names': ['a', 'b', 'a']}, expect='UNKNOWN')
d73 = add('EV-73', 'ENUMERATION ordered = 0 given UNSORTED: the encoder sorts; must EQUAL EV-61', 'ENUMERATION',
          {'container': 'BLOCK_TABLE_NAMES', 'ordered': False, 'itemForm': 'NAME', 'items': ['SEL_FRONTAL_0', 'POSTE_3X3']})
add('EV-74', 'ENUMERATION of a reference set with class (item form HANDLE_CLASS; synthetic handles; sorted as strings)', 'ENUMERATION',
    {'container': 'MODEL_SPACE_RACKCAD_REFERENCES', 'ordered': False, 'itemForm': 'HANDLE_CLASS', 'items': ['2b4:AcDbBlockReference', '1f0:AcDbBlockReference']})
add('EV-75', 'NOD_ENTRY of host class AcDbDictionaryWithDefault (a subclass of AcDbDictionary, supplied in the host-class slot): UNKNOWN', 'NOD_ENTRY',
    ('AcDbDictionaryWithDefault', {'path': 'FOREIGN_ENTRY', 'data': None, 'children': []}), expect='UNKNOWN')
add('EV-76', 'NOD_ENTRY XRECORD with a code 5 entry (a handle): UNKNOWN (AR3-21)', 'NOD_ENTRY',
    ('AcDbXrecord', {'path': 'FOREIGN_XRECORD', 'data': [(1, 'x'), (5, '1F')], 'children': None}), expect='UNKNOWN')
add('EV-77', 'NOD_ENTRY XRECORD with a point-valued code (10): an inline POINT3D (AR3-21)', 'NOD_ENTRY',
    ('AcDbXrecord', {'path': 'FOREIGN_XRECORD', 'data': [(10, Point3d(1.0, 2.0, 0.0)), (40, 0.5)], 'children': None}))
add('EV-78', 'NOD_ENTRY XRECORD whose code 70 entry holds a Double (not the integer of its range): UNKNOWN (fail closed)', 'NOD_ENTRY',
    ('AcDbXrecord', {'path': 'FOREIGN_XRECORD', 'data': [(70, 1.0)], 'children': None}), expect='UNKNOWN')
lay0 = {'table': 'LAYER', 'name': '0', 'colorMethod': 'ByAci', 'colorIndex': 7, 'colorRgb': None, 'colorBookName': None, 'colorName': None,
        'linetype': 'Continuous', 'lineweight': 'ByLineWeightDefault', 'isOff': False, 'isFrozen': False, 'isLocked': False, 'isPlottable': True,
        'transparencyMethod': 'ByAlpha', 'transparencyAlpha': 255}
d79 = add('EV-79', 'SYMBOL_RECORD_LAYER of layer 0 (illustrative properties; the closure layer of EV-62, REUSED_BOUND in EV-64)', 'SYMBOL_RECORD_LAYER', lay0)
d80 = add('EV-80', 'CONTEXT_VARIABLE CLAYER, host value a String', 'CONTEXT_VARIABLE', {'name': 'CLAYER', 'value': '0'})
add('EV-81', 'CONTEXT_VARIABLE CELTSCALE, host value a Double', 'CONTEXT_VARIABLE', {'name': 'CELTSCALE', 'value': 1.0})
src = common('INSERT', '0')
src.update({'blockName': 'SEL_FRONTAL_0', 'blockDigest': ('SEL_FRONTAL_0', d50), 'rotation': 0.0, 'attributes': [], 'dynamicProperties': []})
src.update(pt('position', 100.0, 50.0, 0.0)); src.update(pt('scale', 1.0, 1.0, 1.0)); src.update(pt('normal', 0.0, 0.0, 1.0))
d82 = add('EV-82', 'ENTITY_INSERT of the source top-level reference (static; its definition is EV-50): the REFERENCE member of EV-62', 'ENTITY_INSERT', src)
add('EV-83', 'ENTITY_ROTATED_DIMENSION whose DSTYLE section holds a handle value (code 1005): UNKNOWN (a disclosed availability cost)', 'ENTITY_ROTATED_DIMENSION',
    dict(dim, overrides=[(1070, 342), (1005, '1A')]), expect='UNKNOWN')


# ---------------- V5 vectors (R1:BA05V4-03..07, R2:BA05V4-N02, N03, N06, N09) ----------------
plate = common('LWPOLYLINE', '0')
plate.update({'closed': True, 'plinegen': False, 'elevation': 0.0, 'thickness': 0.0}); plate.update(pt('normal', 0.0, 0.0, 1.0))
plate['vertices'] = [{'x': 0.0, 'y': 0.0, 'bulge': 0.0, 'startWidth': 0.0, 'endWidth': 0.0}, {'x': 3.0, 'y': 0.0, 'bulge': 0.0, 'startWidth': 0.0, 'endWidth': 0.0},
                     {'x': 3.0, 'y': 3.0, 'bulge': 0.0, 'startWidth': 0.0, 'endWidth': 0.0}, {'x': 0.0, 'y': 3.0, 'bulge': 0.0, 'startWidth': 0.0, 'endWidth': 0.0}]
placa = dict(nested, name='PLACA_BASE_3X3', entities=[('AcDbPolyline', plate)])
d84 = add('EV-84', 'DEFINITION_DUMP of the library block PLACA_BASE_3X3 (synthetic; one LWPOLYLINE on layer 0): the ABSENT closure block of EV-62 that EV-64 adds',
          'DEFINITION_DUMP', placa)
d85 = add('EV-85', 'ENUMERATION of the block-table name set of the drawing of EV-62 (synthetic; layout records included): every PRESENT block of EV-62 is in it, '
                   'the ABSENT one is not', 'ENUMERATION',
          {'container': 'BLOCK_TABLE_NAMES', 'ordered': False, 'itemForm': 'NAME', 'items': ['SEL_FRONTAL_0', 'POSTE_3X3', '*Paper_Space', '*Model_Space']})
add('EV-86', 'ENUMERATION of handles supplied in numeric order (a = 10, 1f0 = 496; synthetic): the encoder sorts them as strings (1f0 before a), '
             'so a numeric sort gives other bytes', 'ENUMERATION', {'container': 'MODEL_SPACE_HANDLES', 'ordered': False, 'itemForm': 'HANDLE', 'items': ['a', '1f0']})
add('EV-87', 'CONTEXT_VARIABLE CELWEIGHT from the system-variable read by name, host value an Int16 (-1, illustrative): INT (section 2.1.3)', 'CONTEXT_VARIABLE',
    {'name': 'CELWEIGHT', 'value': -1})
add('EV-88', 'ENTITY_INSERT of an anonymous definition that is not a dynamic-block representation (*U7, synthetic; a digest is supplied): UNKNOWN (section 2.3.2)',
    'ENTITY_INSERT', dict(ins, blockName='*U7', blockDigest=('*U7', dn), dynamicProperties=[]), expect='UNKNOWN')
add('EV-89', 'SYMBOL_RECORD_DIMSTYLE without DIMTXT (a variable the host could not read): UNKNOWN (section 2.3.3)', 'SYMBOL_RECORD_DIMSTYLE',
    dict(ds, dimvars=[x for x in ds['dimvars'] if x['name'] != 'DIMTXT']), expect='UNKNOWN')


def member(cat, kind, name, state, dg, stored=None):
    """A STATE_MEMBER by the rules of section 2.3.5 (storedName only for a PRESENT member of a named record kind)."""
    return {'memberKey': cat + '|' + kind + '|' + name, 'category': cat, 'recordKind': kind, 'expectedName': name,
            'storedName': stored if (state == 'PRESENT' and kind in NAMED_KINDS) else None, 'state': state, 'digest': dg if state == 'PRESENT' else None}


pcsr = {'kind': 'selective', 'members': [member('READ_SET', 'BLOCK', 'SEL_FRONTAL_0', 'PRESENT', d50, 'SEL_FRONTAL_0'),
                                         member('READ_SET', 'PAYLOAD', 'SEL_FRONTAL_0', 'PRESENT', d19, 'SEL_FRONTAL_0'),
                                         member('READ_SET', 'ENUMERATION', 'BLOCK_TABLE_NAMES', 'PRESENT', d85),
                                         member('READ_SET', 'NOD_ENTRY', 'RACKCAD_PROJECT', 'PRESENT', d59, 'RACKCAD_PROJECT'),
                                         member('READ_SET', 'LAYER', 'RACKCAD_ANOTACIONES', 'PRESENT', d54, 'RACKCAD_ANOTACIONES'),
                                         member('READ_SET', 'LAYER', 'RACKCAD_COTAS', 'ABSENT', None),
                                         member('CLOSURE', 'BLOCK', 'POSTE_3X3', 'PRESENT', dn, 'POSTE_3X3'),
                                         member('CLOSURE', 'BLOCK', 'PLACA_BASE_3X3', 'ABSENT', None),
                                         member('CLOSURE', 'LAYER', '0', 'PRESENT', d79, '0'),
                                         member('CONTEXT', 'CONTEXT_VARIABLE', 'CLAYER', 'PRESENT', d80),
                                         member('CONTEXT', 'REFERENCE', '2b4', 'PRESENT', d82)]}
add('EV-62', 'PRE_COMMAND_STATE_RECORD (synthetic names; a consistent subset of a record): the read-set definition, payload, block-table enumeration and NOD entry '
             '(EV-50, EV-19, EV-85, EV-59); the annotation layer the source text uses (EV-54) and an ABSENT dimension layer; the closure block the source '
             'references (EV-48, REUSE_AS_IS), an ABSENT closure block only the mirrored plan requires, the pre-existing layer 0 (EV-79); two CONTEXT members '
             '(EV-80, EV-82)', 'PRE_COMMAND_STATE_RECORD', pcsr)
pcsr_u = {'kind': 'selective', 'members': [dict(pcsr['members'][0], state='UNKNOWN', digest=None, storedName=None)] + pcsr['members'][1:]}
add('EV-63', 'PRE_COMMAND_STATE_RECORD with an UNKNOWN member: UNKNOWN', 'PRE_COMMAND_STATE_RECORD', pcsr_u, expect='UNKNOWN')
man = {'cloningMode': 'Ignore', 'importsCompleted': 1,
       'additions': [{'memberKey': 'CLOSURE|BLOCK|PLACA_BASE_3X3', 'recordKind': 'BLOCK', 'storedName': 'PLACA_BASE_3X3', 'digest': d84}],
       'reusedBound': [{'memberKey': 'CLOSURE|LAYER|0', 'recordKind': 'LAYER', 'storedName': '0', 'preCommandDigest': d79}]}
add('EV-64', 'PREPARE_RESIDUE_MANIFEST (synthetic names): the import adds the ABSENT closure block of EV-62 (PLACA_BASE_3X3, EV-84; ADD) and binds its entity to '
             'the pre-existing layer 0 (EV-79; REUSED_BOUND); POSTE_3X3, PRESENT in EV-62, is REUSE_AS_IS and not in the manifest', 'PREPARE_RESIDUE_MANIFEST', man)

# ---------------- rejected inputs (malformed producer records: no digest and no UNKNOWN) ----------------
reject('RJ-01', 'NOD_ENTRY XRECORD with children present', 'NOD_ENTRY', ('AcDbXrecord', {'path': 'RACKCAD_PROJECT', 'data': [(1, 'x')], 'children': ['a']}))
reject('RJ-02', 'NOD_ENTRY DICTIONARY with data present', 'NOD_ENTRY', ('AcDbDictionary', {'path': 'ACAD_GROUP', 'data': [(1, 'x')], 'children': []}))
reject('RJ-03', 'STATE_MEMBER whose memberKey is not category|recordKind|expectedName (written with "/")', 'PRE_COMMAND_STATE_RECORD',
       {'kind': 'selective', 'members': [dict(member('READ_SET', 'BLOCK', 'SEL_FRONTAL_0', 'PRESENT', d50, 'SEL_FRONTAL_0'), memberKey='READ_SET/BLOCK/SEL_FRONTAL_0')]})
reject('RJ-04', 'record with a missing field (TEST_OPTIONAL without layer)', 'TEST_OPTIONAL', {'name': 'Rack'})
reject('RJ-05', 'ENTITY_ROTATED_DIMENSION with overrides absent', 'ENTITY_ROTATED_DIMENSION', dict(dim, overrides=None))
reject('RJ-06', 'ENUMERATION HANDLE item with a leading zero', 'ENUMERATION', {'container': 'MODEL_SPACE_HANDLES', 'ordered': False, 'itemForm': 'HANDLE', 'items': ['01f']})
# V5 (R1:BA05V4-06, R2:BA05V4-N02, R1:BA05V4-07): every documented closed-set and conditional rule is enforced and pinned
reject('RJ-07', 'ENTITY_LINE with colorRgb present and colorMethod ByLayer', 'ENTITY_LINE', dict(line, colorRgb=123))
reject('RJ-08', 'ENTITY_LINE with colorMethod ByColor and colorRgb absent', 'ENTITY_LINE', dict(line, colorMethod='ByColor'))
reject('RJ-09', 'SYMBOL_RECORD_LAYER with transparencyAlpha present and transparencyMethod ByLayer', 'SYMBOL_RECORD_LAYER', dict(lay, transparencyMethod='ByLayer'))
reject('RJ-10', 'SYMBOL_RECORD_LAYER with transparencyMethod ByAlpha and transparencyAlpha absent', 'SYMBOL_RECORD_LAYER', dict(lay, transparencyAlpha=None))
reject('RJ-11', 'SYMBOL_RECORD_DIMSTYLE with an extra variable DIMFOO', 'SYMBOL_RECORD_DIMSTYLE',
       dict(ds, dimvars=ds['dimvars'] + [{'name': 'DIMFOO', 'value': ('DOUBLE', 1.0)}]))
reject('RJ-12', 'SYMBOL_RECORD_DIMSTYLE with DIMTXT typed INT (declared DOUBLE)', 'SYMBOL_RECORD_DIMSTYLE',
       dict(ds, dimvars=[x if x['name'] != 'DIMTXT' else {'name': 'DIMTXT', 'value': ('INT', 0)} for x in ds['dimvars']]))
reject('RJ-13', 'SYMBOL_RECORD_LAYER with table = LTYPE', 'SYMBOL_RECORD_LAYER', dict(lay, table='LTYPE'))
M_BLOCK = member('READ_SET', 'BLOCK', 'SEL_FRONTAL_0', 'PRESENT', d50, 'SEL_FRONTAL_0')
reject('RJ-14', 'STATE_MEMBER with state FOO', 'PRE_COMMAND_STATE_RECORD', {'kind': 'selective', 'members': [dict(M_BLOCK, state='FOO')]})
reject('RJ-15', 'STATE_MEMBER UNKNOWN that carries a digest (REJECTED takes precedence over the UNKNOWN of the record)', 'PRE_COMMAND_STATE_RECORD',
       {'kind': 'selective', 'members': [dict(M_BLOCK, state='UNKNOWN', storedName=None)]})
reject('RJ-16', 'STATE_MEMBER of kind REFERENCE with a storedName (always absent for REFERENCE)', 'PRE_COMMAND_STATE_RECORD',
       {'kind': 'selective', 'members': [dict(member('CONTEXT', 'REFERENCE', '2b4', 'PRESENT', d82), storedName='2b4')]})
reject('RJ-17', 'STATE_MEMBER of kind REFERENCE whose expectedName is not a handle', 'PRE_COMMAND_STATE_RECORD',
       {'kind': 'selective', 'members': [member('CONTEXT', 'REFERENCE', 'NOT A HANDLE', 'PRESENT', d82)]})
reject('RJ-18', 'TYPED_VALUE of kind ABSENT with a value', 'TEST_TYPED', {'value': ('ABSENT', 5)})
reject('RJ-19', 'TYPED_VALUE of kind INT without a value', 'TEST_TYPED', {'value': ('INT', None)})
reject('RJ-20', 'MANIFEST_ADDITION whose memberKey is not in the form CLOSURE|recordKind|expectedName', 'PREPARE_RESIDUE_MANIFEST',
       dict(man, additions=[dict(man['additions'][0], memberKey='garbage')]))
reject('RJ-21', 'MANIFEST_ADDITION whose memberKey names another record kind (LAYER) than its recordKind (BLOCK)', 'PREPARE_RESIDUE_MANIFEST',
       dict(man, additions=[dict(man['additions'][0], memberKey='CLOSURE|LAYER|PLACA_BASE_3X3')]))
reject('RJ-22', 'MANIFEST_REUSED whose memberKey is not a closure member key (category READ_SET)', 'PREPARE_RESIDUE_MANIFEST',
       dict(man, reusedBound=[dict(man['reusedBound'][0], memberKey='READ_SET|LAYER|0')]))
reject('RJ-23', 'ENTITY_INSERT with a dynamic property without its readOnly flag', 'ENTITY_INSERT',
       dict(ins, dynamicProperties=[{'name': 'LONGITUD', 'value': 96.0}]))
reject('RJ-24', 'NOD_ENTRY input that supplies objectClass (derived from the host class)', 'NOD_ENTRY',
       ('AcDbXrecord', {'path': 'RACKCAD_PROJECT', 'objectClass': 'XRECORD', 'data': [(1, 'x')], 'children': None}))
reject('RJ-25', 'PRECEDENCE: ENTITY_LINE with a NaN endX (UNKNOWN, earlier in field order) and an integer in the boolean field visible (REJECTED, later): REJECTED',
       'ENTITY_LINE', dict(line, endX=float('nan'), visible=1))

vectors.sort(key=lambda v: int(v['id'][3:]))
assert d1 != d2 and d1 != d3 and d8 != d9 and d13 != d14 and d15 == d16 and d19 != d20 and d21 == d22 and d26 != d27 and d27 != d28 and d61 == d73
# EV-62 consistency (R1:BA05V4-03): the block table of EV-85 holds exactly the PRESENT BLOCK members and not the ABSENT one
_bt = {'*Model_Space', '*Paper_Space', 'POSTE_3X3', 'SEL_FRONTAL_0'}
assert {m['expectedName'] for m in pcsr['members'] if m['recordKind'] == 'BLOCK' and m['state'] == 'PRESENT'} <= _bt
assert not ({m['expectedName'] for m in pcsr['members'] if m['recordKind'] == 'BLOCK' and m['state'] == 'ABSENT'} & _bt)
produced = {v['recordType'] for v in vectors}
missing = [k for k, s in SCHEMAS.items() if s['use'] == 'production' and k not in produced]
assert not missing, missing

# ---------------- semantic vectors (typed inputs; section 3.3; NO tolerance value, AR3-17) ----------------
H = lambda x: struct.pack('>d', x).hex()
semantic = [
    {'id': 'SV-01', 'recordType': 'ENVELOPE', 'field': '(unknown member futureKey)', 'form': 'wire (envelope JSON text)',
     'expected': '{"SchemaVersion":"1.0","Kind":"selective","Id":"g-1","futureKey":1}',
     'persisted': '{"futureKey": 1, "Id": "g-1", "Kind": "selective", "SchemaVersion": "1.0"}', 'predicate': 'CANONICAL_JSON_EQUAL', 'result': 'EQUAL',
     'rule': 'key order and whitespace are not significant; the unknown member (read into ExtensionData) is present and equal'},
    {'id': 'SV-02', 'recordType': 'ENVELOPE', 'field': '(unknown member futureKey)', 'form': 'wire (envelope JSON text)',
     'expected': '{"SchemaVersion":"1.0","Kind":"selective","Id":"g-1","futureKey":1}', 'persisted': '{"SchemaVersion":"1.0","Kind":"selective","Id":"g-1"}',
     'predicate': 'CANONICAL_JSON_EQUAL', 'result': 'DIFFERENT', 'rule': 'a writer that DROPS an unknown member does not pass (O4 through S-2/S-3)'},
    {'id': 'SV-03', 'recordType': 'ENVELOPE', 'field': '(unknown member futureKey)', 'form': 'wire (envelope JSON text)',
     'expected': '{"SchemaVersion":"1.0","Kind":"selective","Id":"g-1","futureKey":1}', 'persisted': '{"SchemaVersion":"1.0","Kind":"selective","Id":"g-1","futureKey":2}',
     'predicate': 'CANONICAL_JSON_EQUAL', 'result': 'DIFFERENT', 'rule': 'a writer that ALTERS an unknown member does not pass'},
    {'id': 'SV-04', 'recordType': 'DESIGN', 'field': 'Bays[0].futureKey', 'form': 'wire (design JSON text)',
     'expected': '{"SchemaVersion":"1.0","Bays":[{"Levels":[],"futureKey":1}]}', 'persisted': '{"SchemaVersion":"1.0","Bays":[{"Levels":[],"futureKey":1}]}',
     'predicate': 'CANONICAL_JSON_EQUAL', 'result': 'UNKNOWN_MEMBER_UNSUPPORTED',
     'rule': 'a valid design (SelectiveBayDocument.Levels is a list); the unknown member sits in a bay, whose type declares no ExtensionData (JsonExtensionData is not recursive, I-11), '
             'so the reader drops it on both sides: never EQUAL; surfaced explicitly, gives UNKNOWN (O5 path)'},
    {'id': 'SV-05', 'recordType': 'ENTITY_TEXT', 'field': 'positionY', 'form': 'typed (binary64)', 'expected': H(0.0), 'persisted': H(-0.0),
     'predicate': 'ABS_DIFF_LE(TOL_LENGTH)', 'result': 'EQUAL', 'rule': 'signed zero is domain-equivalent for any tolerance value; secondary finding BIT_LEVEL_DIFFERENCE_DOMAIN_EQUIVALENT; the exact digests differ (EV-01 / EV-02)'},
    {'id': 'SV-06', 'recordType': 'ENTITY_LINE', 'field': 'endX', 'form': 'typed (binary64), slot units', 'expected': H(10.0),
     'persisted': {'slotExpression': 'expected + 0.5 * TOL_LENGTH'}, 'slot': 'TOL_LENGTH', 'factor': '0.5',
     'predicate': 'ABS_DIFF_LE(TOL_LENGTH)', 'result': 'EQUAL', 'rule': 'a difference of half the slot is inside it (per component, absolute)'},
    {'id': 'SV-07', 'recordType': 'ENTITY_LINE', 'field': 'endX', 'form': 'typed (binary64), slot units', 'expected': H(10.0),
     'persisted': {'slotExpression': 'expected + 2 * TOL_LENGTH'}, 'slot': 'TOL_LENGTH', 'factor': '2',
     'predicate': 'ABS_DIFF_LE(TOL_LENGTH)', 'result': 'DIFFERENT', 'rule': 'a difference of twice the slot is outside it; the base value 10 separates the absolute reading from a relative one (a relative tolerance of 10 * TOL_LENGTH would give EQUAL)'},
    {'id': 'SV-08', 'recordType': 'ENVELOPE', 'field': 'SchemaVersion', 'form': 'wire (envelope JSON text)',
     'expected': '{"SchemaVersion":"1.0","Kind":"selective","Id":"g-1"}', 'persisted': '{"SchemaVersion":"9.0","Kind":"selective","Id":"g-1"}',
     'predicate': 'SCHEMA_VERSION_RECOGNIZED', 'result': 'UNKNOWN', 'rule': 'an unrecognized schema version is UNKNOWN'},
    {'id': 'SV-09', 'recordType': 'HOST_OBJECT', 'field': '(exact host class)', 'form': 'host class (RXClass name)', 'expected': 'AcDbBlockReference',
     'persisted': 'AcDbProxyEntity', 'predicate': 'TYPE_SUPPORTED', 'result': 'UNKNOWN',
     'rule': 'the persisted object has a class outside section 2.3.1, so no typed form exists: the comparison is UNKNOWN (never EQUAL, never DIFFERENT); the O1 consequence for closure members belongs to section 2.5'},
    {'id': 'SV-10', 'recordType': 'ENTITY_INSERT', 'field': 'rotation', 'form': 'typed (binary64, radians), slot units', 'expected': H(0.0),
     'persisted': {'slotExpression': 'expected + 2*pi - 0.5 * TOL_ANGLE'}, 'slot': 'TOL_ANGLE', 'factor': '0.5',
     'predicate': 'ANGLE_DIFF_LE(TOL_ANGLE)', 'result': 'EQUAL', 'rule': 'the two angles differ by 0.5 * TOL_ANGLE across the wrap at 2pi; EQUAL under any NORM_ANGLE rule that reduces modulo 2pi'},
    {'id': 'SV-11', 'recordType': 'ENTITY_INSERT', 'field': 'normal', 'form': 'typed (binary64 vector), slot units', 'expected': [H(0.0), H(0.0), H(1.0)],
     'persisted': {'slotExpression': '(tan(2 * TOL_NORMAL), 0, 1)'}, 'slot': 'TOL_NORMAL', 'factor': '2',
     'predicate': 'NORMAL_ANGLE_LE(TOL_NORMAL)', 'result': 'DIFFERENT',
     'rule': 'the angle between the normalized vectors is 2 * TOL_NORMAL; a zero-length vector is UNKNOWN. Meant to reject a comparator that computes the angle as acos of the '
             'binary64 dot product: at slot values small enough that the dot product rounds to 1 such a comparator returns 0, hence EQUAL; a conformant comparator '
             'computes the angle stably, for example atan2(|a x b|, a . b) (AR4-11)'},
    {'id': 'SV-12', 'recordType': 'ENVELOPE', 'field': '(unknown member futureKey)', 'form': 'wire (envelope JSON text)',
     'expected': '{"SchemaVersion":"1.0","Kind":"selective","Id":"g-1","futureKey":1}', 'persisted': '{"SchemaVersion":"1.0","Kind":"selective","Id":"g-1","futureKey":1.0}',
     'predicate': 'CANONICAL_JSON_EQUAL', 'result': 'DIFFERENT',
     'rule': 'PA-8 rule 1 "exact numbers" read as the exact JSON number token (Architect interpretation, AR3-19): 1 and 1.0 differ'},
    {'id': 'SV-13', 'recordType': 'ENTITY_TEXT', 'field': 'textString', 'form': 'typed (string)', 'expected': 'Nivel 1', 'persisted': 'NIVEL 1',
     'predicate': 'EXACT_STRING', 'result': 'DIFFERENT', 'rule': 'texts, names and enum values are compared exactly (stored case)'},
    {'id': 'SV-14', 'recordType': 'ENTITY_INSERT', 'field': 'rotation', 'form': 'typed (binary64, radians), slot units', 'expected': H(0.0),
     'persisted': {'slotExpression': 'expected + 2*pi - 2 * TOL_ANGLE'}, 'slot': 'TOL_ANGLE', 'factor': '2',
     'predicate': 'ANGLE_DIFF_LE(TOL_ANGLE)', 'result': 'DIFFERENT',
     'rule': 'the DIFFERENT counterpart of SV-10: the two angles differ by 2 * TOL_ANGLE across the wrap at 2pi; an always-EQUAL angle comparator fails it'},
    {'id': 'SV-15', 'recordType': 'ENTITY_INSERT', 'field': 'normal', 'form': 'typed (binary64 vector), slot units', 'expected': [H(0.0), H(0.0), H(1.0)],
     'persisted': {'slotExpression': '(tan(0.5 * TOL_NORMAL), 0, 1)'}, 'slot': 'TOL_NORMAL', 'factor': '0.5',
     'predicate': 'NORMAL_ANGLE_LE(TOL_NORMAL)', 'result': 'EQUAL',
     'rule': 'the EQUAL counterpart of SV-11: the angle between the normalized vectors is 0.5 * TOL_NORMAL; an always-DIFFERENT normal comparator fails it'},
    {'id': 'SV-16', 'recordType': 'ENTITY_ROTATED_DIMENSION', 'field': 'overrides', 'form': 'typed (RB sequence as code:value; doubles as binary64)',
     'expected': ['1070:140', '1040:' + H(4.0), '1070:41', '1040:' + H(2.8)], 'persisted': ['1070:41', '1040:' + H(2.8), '1070:140', '1040:' + H(4.0)],
     'predicate': 'OVERRIDE_MAP_EQUAL', 'result': 'EQUAL',
     'rule': 'the stored overrides are compared as a map keyed by the DIMVAR group code (140 = DIMTXT, 41 = DIMASZ); host order is not significant; the values are '
             'equal, so the result holds for any slot value (the layer-1 digests of the two orders differ, section 2.1.2)'},
    {'id': 'SV-17', 'recordType': 'ENTITY_LINE', 'field': 'linetypeScale', 'form': 'typed (binary64)', 'expected': H(1.0), 'persisted': H(math.nextafter(1.0, 2.0)),
     'predicate': 'EXACT_DOUBLE', 'result': 'DIFFERENT',
     'rule': 'a double that section 3.1.2 routes to no slot is compared exactly: 1 ulp differs; linetypeScale is not a scale factor of TOL_SCALE'},
    {'id': 'SV-18', 'recordType': 'PLINE_VERTEX', 'field': 'bulge', 'form': 'typed (binary64)', 'expected': H(0.0), 'persisted': H(-0.0),
     'predicate': 'EXACT_DOUBLE', 'result': 'EQUAL',
     'rule': 'EXACT_DOUBLE treats -0.0 and +0.0 as equal (V2.1 28.2); secondary finding BIT_LEVEL_DIFFERENCE_DOMAIN_EQUIVALENT'},
]
predicates = {
    'ABS_DIFF_LE(TOL)': '|a - b| <= TOL, per scalar component (never a Euclidean distance; AR3-20), absolute (never relative); -0.0 equals +0.0; a non-finite value is UNKNOWN',
    'ANGLE_DIFF_LE(TOL_ANGLE)': 'EQUAL when the wrapped difference of the two angles, computed by the rule of the slot NORM_ANGLE, is <= TOL_ANGLE; the rule of NORM_ANGLE is written only in the kind baseline (V2.2 PA-9), this specification names the slot',
    'NORMAL_ANGLE_LE(TOL_NORMAL)': 'both vectors normalized (a zero-length vector is UNKNOWN); EQUAL when the angle between them <= TOL_NORMAL; the angle is computed stably, '
                                   'for example atan2(|a x b|, a . b); a comparator that computes it as acos of the binary64 dot product is not conformant (SV-11; AR4-11)',
    'DIRECTION_ANGLE_LE(TOL_ANGLE)': 'as NORMAL_ANGLE_LE, for an in-plane direction vector, with the slot TOL_ANGLE',
    'EXACT_DOUBLE': 'binary64 equality of the two values with -0.0 equal to +0.0; a non-finite value is UNKNOWN; the rule for every double that section 3.1.2 routes to no slot',
    'EXACT': 'equality of integers, booleans and enum values (enum values by their stored names)',
    'OVERRIDE_MAP_EQUAL': 'the stored overrides of both sides (section 2.3.2) read as maps from the DIMVAR group code (the value of the code-1070 entry that precedes each '
                          'value) to that value; host order is not significant; an unpaired entry, a repeated group code or a code that designates no variable of section '
                          '2.3.3 is UNKNOWN; EQUAL when both maps have the same codes and every value is equal under the class of its variable (section 3.1.2)',
    'EXACT_STRING': 'ordinal equality of the stored text',
    'CANONICAL_JSON_EQUAL': 'both sides parsed from the wire text; objects compared as maps with ordinally sorted keys; arrays by position; strings exact; numbers by the exact JSON number token (AR3-19); whitespace not significant; a member present on either side that the authoritative reader does not expose is UNKNOWN_MEMBER_UNSUPPORTED',
    'SCHEMA_VERSION_RECOGNIZED': 'an unrecognized schema version makes the comparison UNKNOWN',
    'TYPE_SUPPORTED': 'an object whose exact host class is outside section 2.3.1 makes the comparison UNKNOWN',
}
slot_rule = ('SV-06, SV-07, SV-10, SV-11, SV-14 and SV-15 carry no tolerance value. The Q-FPSPEC-VECTORS harness instantiates them from the sealed slot values of the '
             'kind baseline, each slot taken as the binary64 nearest to its sealed value: persisted = the binary64 nearest (ties to even) to the exact real value of '
             'slotExpression, component by component (pi and tan evaluated exactly, then rounded once). It then evaluates the realized quantity of the vector\'s predicate: '
             'ABS_DIFF_LE: |persisted - expected| computed exactly (rational arithmetic) on the binary64 values; ANGLE_DIFF_LE: the wrapped difference of the binary64 '
             'angles under the sealed NORM_ANGLE rule, evaluated as a certified enclosure with exact pi (interval bounds that contain the exact value); NORMAL_ANGLE_LE: the '
             'angle between the normalized binary64 vectors, evaluated as a certified enclosure. An EQUAL vector requires the whole enclosure <= the slot and a DIFFERENT '
             'vector requires it > the slot; an instance that fails its check, or whose enclosure straddles the slot, is INVALID (reported, never PASS). A NORM_ANGLE rule '
             'that does not reduce modulo 2pi therefore makes SV-10 INVALID')

out = {'spec': 'FPSPEC-V1', 'artifact': 'BA-05 V5', 'status': 'NORMATIVE (as part of BA-05 when sealed)',
       'encoding': {'prefix': PREFIX.decode('ascii'), 'fieldOrder': 'ordinal (UTF-8 byte order) of the field name', 'stringOrder': 'Unicode scalar value order = UTF-8 byte order',
                    'authoritativeBytes': 'streamHex (streamDisplay is informative only)', 'rbKindRanges': [[lo, hi, k] for (lo, hi), k in RB_KIND_RANGES],
                    'rbHandleCodes': sorted(RB_HANDLE_CODES), 'rbUnsupported': RB_UNSUPPORTED, 'rbHostType': RB_HOST_TYPE,
                    'hostValueKinds': HOST_VALUE_KINDS, 'contextVariableRead': CONTEXT_VARIABLE_READ,
                    'contextVariableExpectedHostTypes': {'note': 'expected host type of the system-variable read by name, confirmed by the I-12 qualification on the exact '
                                                                 'build; a different observed type is corrected before the seal (BA-05 V5 section 2.6)',
                                                         'rows': CONTEXT_VARIABLE_HOST_TYPES},
                    'resultPrecedence': 'REJECTED takes precedence over UNKNOWN: a record that violates a rule of section 2.3 is REJECTED even when it is also not fingerprintable',
                    'inputConventions': 'records as JSON objects; a double as {"binary64": 16 hex}; an integer host value (Int16, Int32 or Int64) as a JSON integer; '
                                        'a host Point3d as {"point3d": [x, y, z]} and a Point2d as {"point2d": [x, y]}; '
                                        'a string that is not well-formed UTF-16 as {"utf16": [code units]}; bytes as {"bytes": hex}; absent as null; an RB entry as [code, value]; '
                                        'a typed value as [kind, value]; a nested reference as [name, digest or "UNKNOWN"]; an entity of a definition or an attribute as [host class, fields]; '
                                        'a NOD object (record type NOD_ENTRY) as [host class, fields without objectClass], objectClass being derived by nodClassMap; '
                                        'a dynamic property as {"name", "readOnly", "value" (host value)}; a DIMSTYLE variable the host could not read is omitted'},
       'entityClassMap': ENTITY_CLASS_MAP, 'attributeClassMap': ATTRIBUTE_CLASS_MAP, 'nodClassMap': NOD_CLASS_MAP,
       'recordKinds': RECORD_KINDS, 'namedRecordKinds': list(NAMED_KINDS), 'categories': list(CATEGORIES),
       'enumerationItemForms': ITEM_FORMS,
       'schemas': {k: {'use': s['use'], 'fields': [[f[0], f[1], f[2]] for f in s['fields']]} for k, s in SCHEMAS.items()},
       'dimvars': DIMVARS, 'exactVectors': vectors, 'rejectedInputs': rejections, 'semanticPredicates': predicates,
       'semanticSlotInstanceRule': slot_rule, 'semanticVectors': semantic,
       'layer2Classes': LAYER2_CLASSES,
       'layer2FieldRoutes': [[r, f, c] for (r, f), c in sorted(LAYER2_ROUTES.items())],
       'dimvarClasses': DIMVAR_CLASSES}
here = os.path.dirname(os.path.abspath(__file__))
with open(os.path.join(here, 'I-52-ct21d-fpspec-v1-vectors-v5.json'), 'w', encoding='utf-8', newline='\n') as f:
    f.write(json.dumps(out, indent=1, ensure_ascii=False) + '\n')
print('exact vectors:', len(vectors), 'rejected inputs:', len(rejections), 'semantic vectors:', len(semantic), 'schemas:', len(SCHEMAS))
