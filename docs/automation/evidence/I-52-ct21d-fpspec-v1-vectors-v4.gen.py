"""I-52 CT-21D - reference encoder, record schemas and NORMATIVE test vectors of FPSPEC-V1 (BA-05 V4).

Reproducible: `python I-52-ct21d-fpspec-v1-vectors-v4.gen.py` rewrites
`I-52-ct21d-fpspec-v1-vectors-v4.json` next to this file. The schema registry
`SCHEMAS` below is the normative field table of every record type (BA-05 V4
section 2.3 is generated from it); the tables `RB_KIND_RANGES`,
`ENTITY_CLASS_MAP`, `ATTRIBUTE_CLASS_MAP`, `HOST_VALUE_KINDS`, `RECORD_KINDS`
and `ITEM_FORMS` are the normative tables of BA-05 V4 sections 2.1.2, 2.1.3,
2.3.1, 2.3.5 and 2.3.6; the encoder is the reference of BA-05 V4 section 2.1.
An implementation of the fingerprint provider (instrument I-12) is conformant
only if, from the `input` of every exact vector, it reproduces every `DIGEST`
(byte for byte, field `streamHex`), every `UNKNOWN` and every `REJECTED` of the
JSON file. Design artifact only: no host, no product code, and **no tolerance
value**: the semantic vectors SV-06, SV-07, SV-10 and SV-11 are written in slot
units and are instantiated by the Q-FPSPEC-VECTORS harness (BA-05 V4 section
3.3.1). The V2 and V3 files stay as history.
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


class Point3d:
    """A host Point3d value (in a ResultBuffer entry or a dynamic-property value)."""

    def __init__(self, x, y, z):
        self.x, self.y, self.z = float(x), float(y), float(z)


class Point2d:
    """A host Point2d value: no typed-value kind (section 2.1.3), so UNKNOWN."""

    def __init__(self, x, y):
        self.x, self.y = float(x), float(y)


# ----------------------------------------------------------------------------------------------------------------
# Value kinds (BA-05 V4 section 2.1)
# ----------------------------------------------------------------------------------------------------------------
def utf8(s):
    """Exact stored text as UTF-8. A string that is not well-formed UTF-16 (a lone surrogate) is UNKNOWN."""
    try:
        return s.encode('utf-8')
    except UnicodeEncodeError:
        raise Unknown('string is not well-formed UTF-16 (lone surrogate)')


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
    if code in RB_HANDLE_CODES:
        raise Unknown('RB code %d (%s) is a volatile identifier' % (code, RB_HANDLE_CODES[code]))
    for (lo, hi), kind in RB_KIND_RANGES:
        if lo <= code <= hi:
            return kind
    raise Unknown('RB code %d is outside the table' % code)


def rb_value(code, v):
    """The (encoding kind, value) of one ResultBuffer entry; a host value of the wrong type is UNKNOWN (fail closed)."""
    kind = rb_kind(code)
    ok = {'string': isinstance(v, str), 'double': isinstance(v, float), 'int': isinstance(v, int) and not isinstance(v, bool),
          'bool': isinstance(v, bool), 'bytes': isinstance(v, bytes), 'point3d': isinstance(v, Point3d)}[kind]
    if not ok:
        raise Unknown('RB code %d holds a host value of type %s, not %s' % (code, HOST_NAME.get(type(v), type(v).__name__), RB_HOST_TYPE[kind]))
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
    raise Unknown('a host value of type %s has no typed-value kind (section 2.1.3)' % type(v).__name__)


# Exact host class (RXClass name) -> record type. Section 2.3.1. Never a subclass test and never the DXF name.
ENTITY_CLASS_MAP = {
    'AcDbLine': 'ENTITY_LINE', 'AcDbArc': 'ENTITY_ARC', 'AcDbCircle': 'ENTITY_CIRCLE', 'AcDbPolyline': 'ENTITY_LWPOLYLINE',
    'AcDbText': 'ENTITY_TEXT', 'AcDbMText': 'ENTITY_MTEXT', 'AcDbBlockReference': 'ENTITY_INSERT',
    'AcDbAttributeDefinition': 'ENTITY_ATTDEF', 'AcDbRotatedDimension': 'ENTITY_ROTATED_DIMENSION',
    'AcDbAlignedDimension': 'ENTITY_ALIGNED_DIMENSION',
}
ATTRIBUTE_CLASS_MAP = {'AcDbAttribute': 'ENTITY_ATTRIB'}
MANAGED_NAME = {
    'AcDbLine': 'Line', 'AcDbArc': 'Arc', 'AcDbCircle': 'Circle', 'AcDbPolyline': 'Polyline', 'AcDbText': 'DBText', 'AcDbMText': 'MText',
    'AcDbBlockReference': 'BlockReference', 'AcDbAttributeDefinition': 'AttributeDefinition', 'AcDbRotatedDimension': 'RotatedDimension',
    'AcDbAlignedDimension': 'AlignedDimension', 'AcDbAttribute': 'AttributeReference',
}

# recordKind of a STATE_MEMBER / MANIFEST_* entry -> record type of its digest and meaning of its expectedName. Section 2.3.5.
RECORD_KINDS = [
    ['BLOCK', 'DEFINITION_DUMP', 'the block name'],
    ['LAYER', 'SYMBOL_RECORD_LAYER', 'the layer name'],
    ['LTYPE', 'SYMBOL_RECORD_LTYPE', 'the linetype name'],
    ['STYLE', 'SYMBOL_RECORD_STYLE', 'the text style name'],
    ['DIMSTYLE', 'SYMBOL_RECORD_DIMSTYLE', 'the dimension style name'],
    ['APPID', 'SYMBOL_RECORD_APPID', 'the registered application name'],
    ['PAYLOAD', 'PAYLOAD_EXACT', 'the name of the definition that owns the payload'],
    ['NOD_ENTRY', 'NOD_ENTRY', 'the NOD path'],
    ['ENUMERATION', 'ENUMERATION', 'the container role name'],
    ['CONTEXT_VARIABLE', 'CONTEXT_VARIABLE', 'the variable name'],
    ['REFERENCE', 'ENTITY_INSERT', 'the handle of the reference (session-local, item form HANDLE)'],
]
RECORD_KIND_NAMES = [r[0] for r in RECORD_KINDS]
CATEGORIES = ('READ_SET', 'CLOSURE', 'CONTEXT')

# ENUMERATION item forms. Section 2.3.6.
HANDLE_RE = r'[1-9a-f][0-9a-f]{0,15}'
ITEM_FORMS = [
    ['NAME', 'the stored name', None],
    ['HANDLE', 'the handle in lowercase hexadecimal without leading zeros', '^' + HANDLE_RE + '$'],
    ['HANDLE_CLASS', 'the handle as in HANDLE, a colon, and the exact host class name (RXClass name) of the object', '^' + HANDLE_RE + ':[A-Za-z0-9_]+$'],
]
ITEM_FORM_RE = {f[0]: f[2] for f in ITEM_FORMS}


def enc_seq(pairs):
    """pairs = [(kind, value)], encoded as an ordered sequence."""
    return b'[' + str(len(pairs)).encode('ascii') + b'\n' + b''.join(enc_value(k, x) + b'\n' for k, x in pairs) + b']'


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
            raise Unknown('non-finite double')
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
        return enc_seq([('record:RB', {'code': code, 'value': rb_value(code, val)}) for code, val in v])
    if kind == 'seq:entity':
        pairs = []
        for cls, fields in v:
            if cls not in ENTITY_CLASS_MAP:
                raise Unknown('host class %s has no record type (exact class, section 2.3.1)' % cls)
            pairs.append(('record:' + ENTITY_CLASS_MAP[cls], fields))
        return enc_seq(pairs)
    if kind == 'seq:attrib':
        pairs = []
        for cls, fields in v:
            if cls not in ATTRIBUTE_CLASS_MAP:
                raise Unknown('attribute of host class %s has no record type (exact class, section 2.3.1)' % cls)
            pairs.append(('record:' + ATTRIBUTE_CLASS_MAP[cls], fields))
        return enc_seq(pairs)
    if kind == 'dynprops':
        recs = [{'name': p['name'], 'value': host_typed(p['value'])} for p in v if not p['readOnly']]
        return enc_value('set:record:DYNPROP@name', recs)
    if kind.startswith('seq:'):
        inner = kind[4:]
        return enc_seq([(inner, e) for e in v])
    if kind.startswith('set:record:'):
        rtype, key = kind[len('set:record:'):].split('@')
        keys = [order_key(e[key]) for e in v]
        if len(set(keys)) != len(keys):
            raise Unknown('duplicate key in an unordered set of ' + rtype)
        ordered = [e for _, e in sorted(zip(keys, v), key=lambda t: t[0])]
        return enc_seq([('record:' + rtype, e) for e in ordered])
    if kind == 'set:string':
        keys = [order_key(e) for e in v]
        if len(set(keys)) != len(keys):
            raise Unknown('duplicate member in an unordered set')
        return enc_seq([('string', e) for _, e in sorted(zip(keys, v), key=lambda t: t[0])])
    if kind.startswith('record:'):
        rtype = kind[len('record:'):]
        return b'{' + rtype.encode('ascii') + b'\n' + enc_fields(rtype, v) + b'}'
    if kind == 'ref':
        name, dg = v
        if dg == 'UNKNOWN':
            raise Unknown('the referenced definition %s is UNKNOWN: UNKNOWN propagates through the nested digest (AR3-18)' % name)
        return b'@' + enc_value('string', name) + b'#' + enc_value('digest', dg)
    if kind == 'typed':
        tkind, tval = v
        if tkind not in TYPED_KINDS:
            raise Unknown('typed value of an unsupported kind ' + str(tkind))
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
# Record schemas (BA-05 V4 section 2.3). Fields are streamed in ORDINAL order of the field name (V2.1 28.1,
# AR2-32), never in the order listed here. The listed order is only for reading.
# ----------------------------------------------------------------------------------------------------------------
P3 = lambda p: [(p + 'X', 'double', ''), (p + 'Y', 'double', ''), (p + 'Z', 'double', '')]
COLOR_FIELDS = [('colorMethod', 'enum', 'stored name of the host ColorMethod'), ('colorIndex', 'int', 'ACI index as the host reports it'),
                ('colorRgb', 'int', 'R*65536 + G*256 + B when colorMethod = ByColor, else absent'),
                ('colorBookName', 'string', 'color book name, or absent'), ('colorName', 'string', 'color name in the book, or absent')]
TRANSP = [('transparencyMethod', 'enum', 'stored name of the host TransparencyMethod'), ('transparencyAlpha', 'int', 'alpha 0-255 when the method is ByAlpha, else absent')]
ENTITY_COMMON = [('dxfName', 'string', 'DXF class name as the host reports it (recorded; the record type comes from the exact host class, section 2.3.1)'),
                 ('layer', 'string', 'layer name (stored case)'),
                 ('linetype', 'string', 'linetype name (stored case)'), ('linetypeScale', 'double', ''),
                 ('lineweight', 'enum', 'stored name of the host LineWeight value'), ('visible', 'bool', '')] + COLOR_FIELDS + TRANSP
OVERRIDES_DESC = ('per-entity dimension-variable overrides: the typed values of the DSTYLE section of the entity\'s ACAD extended data, '
                  'in host order (section 2.3.2); empty when there is no DSTYLE section, never absent; the host-derived *D block is excluded')

SCHEMAS = {
    # ---- helper records ----
    'POINT3D': {'use': 'helper', 'fields': [('x', 'double', ''), ('y', 'double', ''), ('z', 'double', '')]},
    'COLOR': {'use': 'helper', 'fields': COLOR_FIELDS},
    'TYPED_VALUE': {'use': 'helper', 'fields': [('kind', 'enum', 'one of ' + ', '.join(TYPED_KINDS)), ('value', 'typed-inner', 'encoded by the kind; absent for ABSENT')]},
    'RB': {'use': 'helper', 'fields': [('code', 'int', 'the DXF group code of the typed value'), ('value', 'typed-inner', 'encoded by the kind of the code range (section 2.1.2)')]},
    'DYNPROP': {'use': 'helper', 'fields': [('name', 'string', 'dynamic property name (stored)'), ('value', 'typed', 'typed value from the host value (section 2.1.3)')]},
    'DIMVAR_VALUE': {'use': 'helper', 'fields': [('name', 'string', 'dimension variable name (closed list, section 2.3.3)'), ('value', 'typed', 'typed value')]},
    'PLINE_VERTEX': {'use': 'helper', 'fields': [('x', 'double', ''), ('y', 'double', ''), ('bulge', 'double', ''), ('startWidth', 'double', ''), ('endWidth', 'double', '')]},
    'LTYPE_DASH': {'use': 'helper', 'fields': [('length', 'double', ''), ('shapeNumber', 'int', ''), ('shapeStyleName', 'string', 'or absent'),
                                               ('shapeOffsetX', 'double', ''), ('shapeOffsetY', 'double', ''), ('shapeRotation', 'double', ''),
                                               ('shapeScale', 'double', ''), ('shapeIsUcsOriented', 'bool', ''), ('text', 'string', 'or absent')]},
    'STATE_MEMBER': {'use': 'helper', 'fields': [('memberKey', 'string', 'category|recordKind|expectedName (the set key; "|" is a literal character)'),
                                                 ('category', 'enum', 'READ_SET, CLOSURE or CONTEXT (section 2.3.5)'),
                                                 ('recordKind', 'string', 'one of the record kinds of section 2.3.5'),
                                                 ('expectedName', 'string', 'the name queried with host symbol-table semantics, or the key of section 2.3.5'),
                                                 ('storedName', 'string', 'the stored case of the pre-existing record when PRESENT, else absent'),
                                                 ('state', 'enum', 'PRESENT, ABSENT or UNKNOWN'),
                                                 ('digest', 'digest', 'the exact digest of the member record (record type of section 2.3.5) when PRESENT, else absent')]},
    'MANIFEST_ADDITION': {'use': 'helper', 'fields': [('memberKey', 'string', 'as STATE_MEMBER'), ('recordKind', 'string', 'as STATE_MEMBER (section 2.3.5)'),
                                                      ('storedName', 'string', ''), ('digest', 'digest', 'exact digest recorded after the import')]},
    'MANIFEST_REUSED': {'use': 'helper', 'fields': [('memberKey', 'string', 'as STATE_MEMBER'), ('recordKind', 'string', 'as STATE_MEMBER (section 2.3.5)'),
                                                    ('storedName', 'string', ''), ('preCommandDigest', 'digest', 'the digest of the pre-command record')]},
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
        ('blockName', 'string', 'the definition name; for a dynamic reference, the name of its dynamic block definition (never the anonymous *U name)'),
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
                                                               ('dimvars', 'set:record:DIMVAR_VALUE@name', 'EVERY variable of the closed list of section 2.3.3, sorted by name')]},
    'SYMBOL_RECORD_APPID': {'use': 'production', 'fields': [('table', 'enum', 'APPID'), ('name', 'string', '')]},
    'PAYLOAD_EXACT': {'use': 'production', 'fields': [('owner', 'string', 'the stored name of the definition whose extension dictionary holds the payload'),
                                                      ('dictKey', 'string', 'the extension-dictionary key (RACKCAD_SELECTIVE)'),
                                                      ('data', 'seq:rb', 'the stored ResultBuffer of the Xrecord: every (code, value) in host order, chunk boundaries and non-text entries preserved; absent when there is no payload')]},
    'NOD_ENTRY': {'use': 'production', 'fields': [('path', 'string', 'the dictionary keys from the named-object dictionary root, stored case, joined by "/"'),
                                                  ('objectClass', 'enum', 'XRECORD when the exact host class is AcDbXrecord, DICTIONARY when it is AcDbDictionary; any other class (a subclass included) is UNKNOWN'),
                                                  ('data', 'seq:rb', 'XRECORD: its ResultBuffer (never absent); DICTIONARY: absent'),
                                                  ('children', 'set:string', 'DICTIONARY: the child keys (each child is its own NOD_ENTRY; never absent); XRECORD: absent')]},
    'ENUMERATION': {'use': 'production', 'fields': [('container', 'string', 'role name of the enumerated container'), ('ordered', 'bool', ''),
                                                    ('itemForm', 'enum', 'NAME, HANDLE or HANDLE_CLASS (section 2.3.6)'),
                                                    ('items', 'seq:string', 'the items in the item form; ordered = 1: host iteration order; ordered = 0: the encoder sorts them in ordinal (UTF-8) order and a duplicate item is UNKNOWN; handle forms are valid only within one session')]},
    'CONTEXT_VARIABLE': {'use': 'production', 'fields': [('name', 'string', 'the variable name, upper case, as the kind baseline lists it'),
                                                         ('value', 'hosttyped', 'the value the host reports, as a TYPED_VALUE (section 2.1.3)')]},
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


def check_record(rtype, rec):
    """Record-level rules of section 2.3 (conditional absences, closed enums, key forms). UNKNOWN or Rejected."""
    if rtype == 'NOD_ENTRY':
        if rec['objectClass'] not in ('XRECORD', 'DICTIONARY'):
            raise Unknown('NOD object of class %s: only AcDbXrecord and AcDbDictionary (exact class) are supported' % rec['objectClass'])
        if rec['objectClass'] == 'XRECORD' and (rec['data'] is None or rec['children'] is not None):
            raise Rejected('NOD_ENTRY XRECORD: data must be present and children absent')
        if rec['objectClass'] == 'DICTIONARY' and (rec['data'] is not None or rec['children'] is None):
            raise Rejected('NOD_ENTRY DICTIONARY: data must be absent and children present')
    if rtype in ('ENTITY_ROTATED_DIMENSION', 'ENTITY_ALIGNED_DIMENSION') and rec['overrides'] is None:
        raise Rejected('%s: overrides is never absent (empty when there is no DSTYLE section)' % rtype)
    if rtype == 'ENUMERATION':
        form = rec['itemForm']
        if form not in ITEM_FORM_RE:
            raise Rejected('ENUMERATION: unknown item form %s' % form)
        pattern = ITEM_FORM_RE[form]
        for item in rec['items']:
            if pattern is not None and not re.match(pattern, item):
                raise Rejected('ENUMERATION: item %r is not in the form %s' % (item, form))
    if rtype == 'STATE_MEMBER':
        if rec['category'] not in CATEGORIES or rec['recordKind'] not in RECORD_KIND_NAMES:
            raise Rejected('STATE_MEMBER: category or recordKind outside section 2.3.5')
        if rec['memberKey'] != rec['category'] + '|' + rec['recordKind'] + '|' + rec['expectedName']:
            raise Rejected('STATE_MEMBER: memberKey is not category|recordKind|expectedName')
        if rec['state'] == 'PRESENT' and (rec['digest'] is None or rec['storedName'] is None):
            raise Rejected('STATE_MEMBER PRESENT: digest and storedName are required')
        if rec['state'] == 'ABSENT' and (rec['digest'] is not None or rec['storedName'] is not None):
            raise Rejected('STATE_MEMBER ABSENT: digest and storedName are absent')
    if rtype in ('MANIFEST_ADDITION', 'MANIFEST_REUSED') and rec['recordKind'] not in RECORD_KIND_NAMES:
        raise Rejected('%s: recordKind outside section 2.3.5' % rtype)
    if rtype == 'PRE_COMMAND_STATE_RECORD' and any(m['state'] == 'UNKNOWN' for m in rec['members']):
        raise Unknown('a member of the PRE_COMMAND_STATE_RECORD is UNKNOWN: no partial digest (refusal O1, V2.1 6)')


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
        keys = [order_key(e) for e in rec['items']]
        if len(set(keys)) != len(keys):
            raise Unknown('duplicate item in an unordered ENUMERATION')
        items['items'] = [e for _, e in sorted(zip(keys, rec['items']), key=lambda t: t[0])]
    out = b''
    for name, kind, _ in sorted(schema, key=lambda f: f[0].encode('ascii')):
        out += name.encode('ascii') + b'=' + enc_value(kind, items[name]) + b'\n'
    return out


def stream(rtype, rec):
    return PREFIX + rtype.encode('ascii') + b'|' + enc_fields(rtype, rec)


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
add('EV-54', 'SYMBOL_RECORD_LAYER', 'SYMBOL_RECORD_LAYER', lay)
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
add('EV-59', 'NOD_ENTRY, an XRECORD (RACKCAD_PROJECT, as the product writes it: text chunks of code 1)', 'NOD_ENTRY',
    {'path': 'RACKCAD_PROJECT', 'objectClass': 'XRECORD', 'data': [(1, '{"Variables":[]}')], 'children': None})
add('EV-60', 'NOD_ENTRY, a DICTIONARY: the host dictionary ACAD_GROUP with two synthetic group keys (children sorted)', 'NOD_ENTRY',
    {'path': 'ACAD_GROUP', 'objectClass': 'DICTIONARY', 'data': None, 'children': ['b', 'A']})
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
add('EV-75', 'NOD_ENTRY of a class other than AcDbXrecord or AcDbDictionary (AcDbDictionaryWithDefault, a subclass): UNKNOWN', 'NOD_ENTRY',
    {'path': 'FOREIGN_ENTRY', 'objectClass': 'AcDbDictionaryWithDefault', 'data': None, 'children': []}, expect='UNKNOWN')
add('EV-76', 'NOD_ENTRY XRECORD with a code 5 entry (a handle): UNKNOWN (AR3-21)', 'NOD_ENTRY',
    {'path': 'FOREIGN_XRECORD', 'objectClass': 'XRECORD', 'data': [(1, 'x'), (5, '1F')], 'children': None}, expect='UNKNOWN')
add('EV-77', 'NOD_ENTRY XRECORD with a point-valued code (10): an inline POINT3D (AR3-21)', 'NOD_ENTRY',
    {'path': 'FOREIGN_XRECORD', 'objectClass': 'XRECORD', 'data': [(10, Point3d(1.0, 2.0, 0.0)), (40, 0.5)], 'children': None})
add('EV-78', 'NOD_ENTRY XRECORD whose code 70 entry holds a Double (not the integer of its range): UNKNOWN (fail closed)', 'NOD_ENTRY',
    {'path': 'FOREIGN_XRECORD', 'objectClass': 'XRECORD', 'data': [(70, 1.0)], 'children': None}, expect='UNKNOWN')
lay0 = {'table': 'LAYER', 'name': '0', 'colorMethod': 'ByAci', 'colorIndex': 7, 'colorRgb': None, 'colorBookName': None, 'colorName': None,
        'linetype': 'Continuous', 'lineweight': 'ByLineWeightDefault', 'isOff': False, 'isFrozen': False, 'isLocked': False, 'isPlottable': True,
        'transparencyMethod': 'ByAlpha', 'transparencyAlpha': 255}
d79 = add('EV-79', 'SYMBOL_RECORD_LAYER of layer 0 (illustrative properties; the closure layer bound by EV-62 and EV-64)', 'SYMBOL_RECORD_LAYER', lay0)
d80 = add('EV-80', 'CONTEXT_VARIABLE CLAYER, host value a String', 'CONTEXT_VARIABLE', {'name': 'CLAYER', 'value': '0'})
add('EV-81', 'CONTEXT_VARIABLE CELTSCALE, host value a Double', 'CONTEXT_VARIABLE', {'name': 'CELTSCALE', 'value': 1.0})
src = common('INSERT', '0')
src.update({'blockName': 'SEL_FRONTAL_0', 'blockDigest': ('SEL_FRONTAL_0', d50), 'rotation': 0.0, 'attributes': [], 'dynamicProperties': []})
src.update(pt('position', 100.0, 50.0, 0.0)); src.update(pt('scale', 1.0, 1.0, 1.0)); src.update(pt('normal', 0.0, 0.0, 1.0))
d82 = add('EV-82', 'ENTITY_INSERT of the source top-level reference (static; its definition is EV-50): the REFERENCE member of EV-62', 'ENTITY_INSERT', src)
add('EV-83', 'ENTITY_ROTATED_DIMENSION whose DSTYLE section holds a handle value (code 1005): UNKNOWN (a disclosed availability cost)', 'ENTITY_ROTATED_DIMENSION',
    dict(dim, overrides=[(1070, 342), (1005, '1A')]), expect='UNKNOWN')


def member(cat, kind, name, state, dg, stored=None):
    return {'memberKey': cat + '|' + kind + '|' + name, 'category': cat, 'recordKind': kind, 'expectedName': name,
            'storedName': stored if state == 'PRESENT' else None, 'state': state, 'digest': dg if state == 'PRESENT' else None}


pcsr = {'kind': 'selective', 'members': [member('READ_SET', 'BLOCK', 'SEL_FRONTAL_0', 'PRESENT', d50, 'SEL_FRONTAL_0'),
                                         member('READ_SET', 'PAYLOAD', 'SEL_FRONTAL_0', 'PRESENT', d19, 'SEL_FRONTAL_0'),
                                         member('READ_SET', 'LAYER', 'RACKCAD_ANOTACIONES', 'ABSENT', None),
                                         member('CLOSURE', 'BLOCK', 'POSTE_3X3', 'ABSENT', None),
                                         member('CLOSURE', 'LAYER', '0', 'PRESENT', d79, '0'),
                                         member('CONTEXT', 'CONTEXT_VARIABLE', 'CLAYER', 'PRESENT', d80, 'CLAYER'),
                                         member('CONTEXT', 'REFERENCE', '2b4', 'PRESENT', d82, '2b4')]}
add('EV-62', 'PRE_COMMAND_STATE_RECORD (synthetic names): read-set definition and payload (EV-50, EV-19), an ABSENT annotation layer, '
             'an ABSENT closure block, the pre-existing layer 0 (EV-79), and two CONTEXT members (EV-80, EV-82)', 'PRE_COMMAND_STATE_RECORD', pcsr)
pcsr_u = {'kind': 'selective', 'members': [dict(pcsr['members'][0], state='UNKNOWN', digest=None, storedName=None)] + pcsr['members'][1:]}
add('EV-63', 'PRE_COMMAND_STATE_RECORD with an UNKNOWN member: UNKNOWN', 'PRE_COMMAND_STATE_RECORD', pcsr_u, expect='UNKNOWN')
man = {'cloningMode': 'Ignore', 'importsCompleted': 1,
       'additions': [{'memberKey': 'CLOSURE|BLOCK|POSTE_3X3', 'recordKind': 'BLOCK', 'storedName': 'POSTE_3X3', 'digest': dn}],
       'reusedBound': [{'memberKey': 'CLOSURE|LAYER|0', 'recordKind': 'LAYER', 'storedName': '0', 'preCommandDigest': d79}]}
add('EV-64', 'PREPARE_RESIDUE_MANIFEST (synthetic names): the import adds the closure block of EV-62 (EV-48) and binds to the pre-existing layer 0 (EV-79)',
    'PREPARE_RESIDUE_MANIFEST', man)

# ---------------- rejected inputs (malformed producer records: no digest and no UNKNOWN) ----------------
reject('RJ-01', 'NOD_ENTRY XRECORD with children present', 'NOD_ENTRY', {'path': 'RACKCAD_PROJECT', 'objectClass': 'XRECORD', 'data': [(1, 'x')], 'children': ['a']})
reject('RJ-02', 'NOD_ENTRY DICTIONARY with data present', 'NOD_ENTRY', {'path': 'ACAD_GROUP', 'objectClass': 'DICTIONARY', 'data': [(1, 'x')], 'children': []})
reject('RJ-03', 'STATE_MEMBER whose memberKey is not category|recordKind|expectedName (written with "/")', 'PRE_COMMAND_STATE_RECORD',
       {'kind': 'selective', 'members': [dict(member('READ_SET', 'BLOCK', 'SEL_FRONTAL_0', 'PRESENT', d50, 'SEL_FRONTAL_0'), memberKey='READ_SET/BLOCK/SEL_FRONTAL_0')]})
reject('RJ-04', 'record with a missing field (TEST_OPTIONAL without layer)', 'TEST_OPTIONAL', {'name': 'Rack'})
reject('RJ-05', 'ENTITY_ROTATED_DIMENSION with overrides absent', 'ENTITY_ROTATED_DIMENSION', dict(dim, overrides=None))
reject('RJ-06', 'ENUMERATION HANDLE item with a leading zero', 'ENUMERATION', {'container': 'MODEL_SPACE_HANDLES', 'ordered': False, 'itemForm': 'HANDLE', 'items': ['01f']})

vectors.sort(key=lambda v: int(v['id'][3:]))
assert d1 != d2 and d1 != d3 and d8 != d9 and d13 != d14 and d15 == d16 and d19 != d20 and d21 == d22 and d26 != d27 and d27 != d28 and d61 == d73
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
     'predicate': 'NORMAL_ANGLE_LE(TOL_NORMAL)', 'result': 'DIFFERENT', 'rule': 'the angle between the normalized vectors is 2 * TOL_NORMAL; a zero-length vector is UNKNOWN'},
    {'id': 'SV-12', 'recordType': 'ENVELOPE', 'field': '(unknown member futureKey)', 'form': 'wire (envelope JSON text)',
     'expected': '{"SchemaVersion":"1.0","Kind":"selective","Id":"g-1","futureKey":1}', 'persisted': '{"SchemaVersion":"1.0","Kind":"selective","Id":"g-1","futureKey":1.0}',
     'predicate': 'CANONICAL_JSON_EQUAL', 'result': 'DIFFERENT',
     'rule': 'PA-8 rule 1 "exact numbers" read as the exact JSON number token (Architect interpretation, AR3-19): 1 and 1.0 differ'},
    {'id': 'SV-13', 'recordType': 'ENTITY_TEXT', 'field': 'textString', 'form': 'typed (string)', 'expected': 'Nivel 1', 'persisted': 'NIVEL 1',
     'predicate': 'EXACT_STRING', 'result': 'DIFFERENT', 'rule': 'texts, names and enum values are compared exactly (stored case)'},
]
predicates = {
    'ABS_DIFF_LE(TOL)': '|a - b| <= TOL, per scalar component (never a Euclidean distance; AR3-20), absolute (never relative); -0.0 equals +0.0; a non-finite value is UNKNOWN',
    'ANGLE_DIFF_LE(TOL_ANGLE)': 'EQUAL when the wrapped difference of the two angles, computed by the rule of the slot NORM_ANGLE, is <= TOL_ANGLE; the rule of NORM_ANGLE is written only in the kind baseline (V2.2 PA-9), this specification names the slot',
    'NORMAL_ANGLE_LE(TOL_NORMAL)': 'both vectors normalized (a zero-length vector is UNKNOWN); EQUAL when the angle between them <= TOL_NORMAL',
    'EXACT_STRING': 'ordinal equality of the stored text',
    'CANONICAL_JSON_EQUAL': 'both sides parsed from the wire text; objects compared as maps with ordinally sorted keys; arrays by position; strings exact; numbers by the exact JSON number token (AR3-19); whitespace not significant; a member present on either side that the authoritative reader does not expose is UNKNOWN_MEMBER_UNSUPPORTED',
    'SCHEMA_VERSION_RECOGNIZED': 'an unrecognized schema version makes the comparison UNKNOWN',
    'TYPE_SUPPORTED': 'an object whose exact host class is outside section 2.3.1 makes the comparison UNKNOWN',
}
slot_rule = ('SV-06, SV-07, SV-10 and SV-11 carry no tolerance value. The Q-FPSPEC-VECTORS harness instantiates them from the sealed slot values of the kind '
             'baseline: persisted = the binary64 nearest (ties to even) to the exact real value of slotExpression, component by component; it then checks, in exact arithmetic, that the '
             'realized difference is <= the slot for an EQUAL vector and > the slot for a DIFFERENT vector; an instance that fails the check is INVALID (reported, never PASS)')

out = {'spec': 'FPSPEC-V1', 'artifact': 'BA-05 V4', 'status': 'NORMATIVE (as part of BA-05 when sealed)',
       'encoding': {'prefix': PREFIX.decode('ascii'), 'fieldOrder': 'ordinal (UTF-8 byte order) of the field name', 'stringOrder': 'Unicode scalar value order = UTF-8 byte order',
                    'authoritativeBytes': 'streamHex (streamDisplay is informative only)', 'rbKindRanges': [[lo, hi, k] for (lo, hi), k in RB_KIND_RANGES],
                    'rbHandleCodes': sorted(RB_HANDLE_CODES), 'rbUnsupported': RB_UNSUPPORTED, 'rbHostType': RB_HOST_TYPE,
                    'hostValueKinds': HOST_VALUE_KINDS,
                    'inputConventions': 'records as JSON objects; a double as {"binary64": 16 hex}; a host Point3d as {"point3d": [x, y, z]} and a Point2d as {"point2d": [x, y]}; '
                                        'a string that is not well-formed UTF-16 as {"utf16": [code units]}; bytes as {"bytes": hex}; absent as null; an RB entry as [code, value]; '
                                        'a typed value as [kind, value]; a nested reference as [name, digest or "UNKNOWN"]; an entity of a definition or an attribute as [host class, fields]; '
                                        'a dynamic property as {"name", "readOnly", "value" (host value)}'},
       'entityClassMap': ENTITY_CLASS_MAP, 'attributeClassMap': ATTRIBUTE_CLASS_MAP, 'recordKinds': RECORD_KINDS, 'categories': list(CATEGORIES),
       'enumerationItemForms': ITEM_FORMS,
       'schemas': {k: {'use': s['use'], 'fields': [[f[0], f[1], f[2]] for f in s['fields']]} for k, s in SCHEMAS.items()},
       'dimvars': DIMVARS, 'exactVectors': vectors, 'rejectedInputs': rejections, 'semanticPredicates': predicates,
       'semanticSlotInstanceRule': slot_rule, 'semanticVectors': semantic}
here = os.path.dirname(os.path.abspath(__file__))
with open(os.path.join(here, 'I-52-ct21d-fpspec-v1-vectors-v4.json'), 'w', encoding='utf-8', newline='\n') as f:
    f.write(json.dumps(out, indent=1, ensure_ascii=False) + '\n')
print('exact vectors:', len(vectors), 'rejected inputs:', len(rejections), 'semantic vectors:', len(semantic), 'schemas:', len(SCHEMAS))
