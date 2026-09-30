"""I-52 CT-21D - reference encoder, record schemas and NORMATIVE test vectors of FPSPEC-V1 (BA-05 V3).

Reproducible: `python I-52-ct21d-fpspec-v1-vectors-v3.gen.py` rewrites
`I-52-ct21d-fpspec-v1-vectors-v3.json` next to this file. The schema registry
`SCHEMAS` below is the normative field table of every record type (BA-05 V3
section 2.3 is generated from it); the encoder is the reference of BA-05 V3
section 2.1. An implementation of the fingerprint provider (instrument I-12) is
conformant only if it reproduces every `DIGEST` (byte for byte, field
`streamHex`) and every `UNKNOWN` of the JSON file. Design artifact only: no host,
no product code. The V2 files (`I-52-ct21d-fpspec-v1-vectors.gen.py` and `.json`)
stay as history.
"""
import hashlib
import json
import math
import os
import struct

PREFIX = b'FPSPEC-V1|EXACT|'


class Unknown(Exception):
    """The record cannot be fingerprinted: result UNKNOWN, never a digest."""


# ----------------------------------------------------------------------------------------------------------------
# Value kinds (BA-05 V3 section 2.1)
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


# DXF group-code ranges -> value kind, for the typed values of a ResultBuffer (RB records).
RB_KIND_RANGES = [
    ((0, 9), 'string'), ((10, 59), 'double'), ((60, 79), 'int'), ((90, 99), 'int'), ((100, 100), 'string'),
    ((102, 102), 'string'), ((110, 149), 'double'), ((160, 169), 'int'), ((170, 179), 'int'), ((210, 239), 'double'),
    ((270, 289), 'int'), ((290, 299), 'bool'), ((300, 309), 'string'), ((310, 319), 'bytes'), ((370, 389), 'int'),
    ((400, 409), 'int'), ((410, 419), 'string'), ((420, 429), 'int'), ((430, 439), 'string'), ((440, 459), 'int'),
    ((460, 469), 'double'), ((470, 479), 'string'), ((999, 1003), 'string'), ((1004, 1004), 'bytes'),
    ((1005, 1009), 'string'), ((1010, 1059), 'double'), ((1060, 1071), 'int'),
]
RB_UNSUPPORTED = 'handle and object-id codes (5, 105, 320-369, 390-399, 480-481, 1005 as handle) and every code outside the table'


def rb_kind(code):
    if code == 1005:
        raise Unknown('RB code 1005 (handle) is a volatile identifier')
    for (lo, hi), kind in RB_KIND_RANGES:
        if lo <= code <= hi:
            return kind
    raise Unknown('RB code %d not supported' % code)


TYPED_KINDS = ('ABSENT', 'BOOL', 'INT', 'DOUBLE', 'STRING', 'NAME', 'BYTES', 'POINT3D', 'COLOR')


def enc_value(kind, v):
    if v is None:
        return b'~'
    if kind == 'int':
        if isinstance(v, bool) or not isinstance(v, int):
            raise TypeError('int expected')
        return str(v).encode('ascii')
    if kind == 'bool':
        if not isinstance(v, bool):
            raise TypeError('bool expected')
        return b'1' if v else b'0'
    if kind == 'double':
        v = float(v)
        if math.isnan(v) or math.isinf(v):
            raise Unknown('non-finite double')
        return struct.pack('>d', v).hex().encode('ascii')
    if kind in ('string', 'enum'):
        b = utf8(v)
        return b'S' + str(len(b)).encode('ascii') + b':' + b
    if kind == 'bytes':
        return b'B' + str(len(v)).encode('ascii') + b':' + v
    if kind == 'digest':
        if not (isinstance(v, str) and len(v) == 64 and all(c in '0123456789abcdef' for c in v)):
            raise TypeError('64 lowercase hex digits expected')
        return v.encode('ascii')
    if kind.startswith('seq:'):
        inner = kind[4:]
        out = b'[' + str(len(v)).encode('ascii') + b'\n'
        for e in v:
            out += enc_value(inner, e) + b'\n'
        return out + b']'
    if kind.startswith('set:record:'):
        rtype, key = kind[len('set:record:'):].split('@')
        keys = [order_key(e[key]) for e in v]
        if len(set(keys)) != len(keys):
            raise Unknown('duplicate key in an unordered set of ' + rtype)
        ordered = [e for _, e in sorted(zip(keys, v), key=lambda t: t[0])]
        return enc_value('seq:record:' + rtype, ordered)
    if kind == 'set:string':
        keys = [order_key(e) for e in v]
        if len(set(keys)) != len(keys):
            raise Unknown('duplicate member in an unordered set')
        return enc_value('seq:string', [e for _, e in sorted(zip(keys, v), key=lambda t: t[0])])
    if kind.startswith('record:'):
        rtype = kind[len('record:'):]
        return b'{' + rtype.encode('ascii') + b'\n' + enc_fields(rtype, v) + b'}'
    if kind == 'ref':
        name, dg = v
        return b'@' + enc_value('string', name) + b'#' + enc_value('digest', dg)
    if kind == 'typed':
        tkind, tval = v
        if tkind not in TYPED_KINDS:
            raise Unknown('typed value of an unsupported kind ' + str(tkind))
        inner = {'ABSENT': None, 'BOOL': 'bool', 'INT': 'int', 'DOUBLE': 'double', 'STRING': 'string', 'NAME': 'string',
                 'BYTES': 'bytes', 'POINT3D': 'record:POINT3D', 'COLOR': 'record:COLOR'}[tkind]
        return enc_value('record:TYPED_VALUE', {'kind': tkind, 'value': None if inner is None else (inner, tval)})
    if kind == 'typed-inner':
        inner, tval = v
        return enc_value(inner, tval)
    if kind == 'rb':
        code, val = v
        return enc_value('record:RB', {'code': code, 'value': (rb_kind(code), val)})
    raise ValueError(kind)


# ----------------------------------------------------------------------------------------------------------------
# Record schemas (BA-05 V3 section 2.3). Fields are streamed in ORDINAL order of the field name (V2.1 28.1,
# AR2-32), never in the order listed here. The listed order is only for reading.
# ----------------------------------------------------------------------------------------------------------------
P3 = lambda p: [(p + 'X', 'double', ''), (p + 'Y', 'double', ''), (p + 'Z', 'double', '')]
COLOR_FIELDS = [('colorMethod', 'enum', 'stored name of the host ColorMethod'), ('colorIndex', 'int', 'ACI index as the host reports it'),
                ('colorRgb', 'int', 'R*65536 + G*256 + B when colorMethod = ByColor, else absent'),
                ('colorBookName', 'string', 'color book name, or absent'), ('colorName', 'string', 'color name in the book, or absent')]
TRANSP = [('transparencyMethod', 'enum', 'stored name of the host TransparencyMethod'), ('transparencyAlpha', 'int', 'alpha 0-255 when the method is ByAlpha, else absent')]
ENTITY_COMMON = [('dxfName', 'string', 'DXF class name as the host reports it'), ('layer', 'string', 'layer name (stored case)'),
                 ('linetype', 'string', 'linetype name (stored case)'), ('linetypeScale', 'double', ''),
                 ('lineweight', 'enum', 'stored name of the host LineWeight value'), ('visible', 'bool', '')] + COLOR_FIELDS + TRANSP

SCHEMAS = {
    # ---- helper records ----
    'POINT3D': {'use': 'helper', 'fields': [('x', 'double', ''), ('y', 'double', ''), ('z', 'double', '')]},
    'COLOR': {'use': 'helper', 'fields': COLOR_FIELDS},
    'TYPED_VALUE': {'use': 'helper', 'fields': [('kind', 'enum', 'one of ' + ', '.join(TYPED_KINDS)), ('value', 'typed-inner', 'encoded by the kind; absent for ABSENT')]},
    'RB': {'use': 'helper', 'fields': [('code', 'int', 'the DXF group code of the typed value'), ('value', 'typed-inner', 'encoded by the kind of the code range (section 2.1.2)')]},
    'DYNPROP': {'use': 'helper', 'fields': [('name', 'string', 'dynamic property name (stored)'), ('value', 'typed', 'typed value')]},
    'DIMVAR_VALUE': {'use': 'helper', 'fields': [('name', 'string', 'dimension variable name (closed list, section 2.3.3)'), ('value', 'typed', 'typed value')]},
    'PLINE_VERTEX': {'use': 'helper', 'fields': [('x', 'double', ''), ('y', 'double', ''), ('bulge', 'double', ''), ('startWidth', 'double', ''), ('endWidth', 'double', '')]},
    'LTYPE_DASH': {'use': 'helper', 'fields': [('length', 'double', ''), ('shapeNumber', 'int', ''), ('shapeStyleName', 'string', 'or absent'),
                                               ('shapeOffsetX', 'double', ''), ('shapeOffsetY', 'double', ''), ('shapeRotation', 'double', ''),
                                               ('shapeScale', 'double', ''), ('shapeIsUcsOriented', 'bool', ''), ('text', 'string', 'or absent')]},
    'STATE_MEMBER': {'use': 'helper', 'fields': [('memberKey', 'string', 'category|recordKind|expectedName (the set key)'),
                                                 ('category', 'enum', 'READ_SET or CLOSURE (V2.1 6, 11.3)'),
                                                 ('recordKind', 'string', 'BLOCK, LAYER, LTYPE, STYLE, DIMSTYLE, APPID, PAYLOAD, NOD_ENTRY, ENUMERATION or BINDING'),
                                                 ('expectedName', 'string', 'the name queried with host symbol-table semantics'),
                                                 ('storedName', 'string', 'the stored case of the pre-existing record, absent when ABSENT'),
                                                 ('state', 'enum', 'PRESENT, ABSENT or UNKNOWN'),
                                                 ('digest', 'digest', 'the exact digest of the member record when PRESENT, else absent')]},
    'MANIFEST_ADDITION': {'use': 'helper', 'fields': [('memberKey', 'string', 'as STATE_MEMBER'), ('recordKind', 'string', ''), ('storedName', 'string', ''), ('digest', 'digest', 'exact digest recorded after the import')]},
    'MANIFEST_REUSED': {'use': 'helper', 'fields': [('memberKey', 'string', 'as STATE_MEMBER'), ('recordKind', 'string', ''), ('storedName', 'string', ''), ('preCommandDigest', 'digest', 'the digest of the pre-command record')]},
    # ---- production records ----
    'DEFINITION_DUMP': {'use': 'production', 'fields': [
        ('name', 'string', 'block name (stored case)')] + P3('origin') + [
        ('explodable', 'bool', ''), ('scaling', 'enum', 'stored name of the host BlockScaling'), ('units', 'enum', 'stored name of the host UnitsValue'),
        ('annotative', 'enum', 'stored name of the host AnnotativeStates'),
        ('entities', 'seq:entity', 'every entity of the record in HOST ITERATION ORDER, each an inline ENTITY_* record (section 2.2)')]},
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
        ('blockDigest', 'ref', 'reference to the DEFINITION_DUMP digest of that definition (nested-digest field)')] + P3('position') + P3('scale') + [
        ('rotation', 'double', '')] + P3('normal') + [
        ('dynamicProperties', 'set:record:DYNPROP@name', 'dynamic property values, sorted by name; empty for a static reference'),
        ('attributes', 'seq:record:ENTITY_ATTRIB', 'attribute references in host iteration order')]},
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
        ('overrides', 'set:record:DIMVAR_VALUE@name', 'per-entity dimension-variable overrides, sorted by name; the host-derived *D block is excluded')]},
    'ENTITY_ALIGNED_DIMENSION': {'use': 'production', 'fields': ENTITY_COMMON + P3('xLine1Point') + P3('xLine2Point') + P3('dimLinePoint') + P3('textPosition') + [
        ('oblique', 'double', ''), ('dimensionStyleName', 'string', ''), ('dimensionText', 'string', ''), ('usingDefaultTextPosition', 'bool', '')] + P3('normal') + [
        ('overrides', 'set:record:DIMVAR_VALUE@name', 'as ENTITY_ROTATED_DIMENSION')]},
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
                                                  ('objectClass', 'enum', 'XRECORD or DICTIONARY (any other class is UNKNOWN)'),
                                                  ('data', 'seq:rb', 'XRECORD: its ResultBuffer; DICTIONARY: absent'),
                                                  ('children', 'set:string', 'DICTIONARY: the child keys (each child is its own NOD_ENTRY); XRECORD: absent')]},
    'ENUMERATION': {'use': 'production', 'fields': [('container', 'string', 'role name of the enumerated container'), ('ordered', 'bool', ''),
                                                    ('items', 'seq:string', 'names, or handles as lowercase hexadecimal without leading zeros; when ordered = 0 the producer sorts the items in ordinal (UTF-8) order first; handle lists are valid only within one session')]},
    'PRE_COMMAND_STATE_RECORD': {'use': 'production', 'fields': [('kind', 'string', 'the rack kind (selective)'),
                                                                 ('members', 'set:record:STATE_MEMBER@memberKey', 'every read-set component and every closure member (V2.1 6, 11.3)')]},
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


def enc_fields(rtype, rec):
    if rtype not in SCHEMAS:
        raise Unknown('record type %s is not in the specification' % rtype)
    schema = SCHEMAS[rtype]['fields']
    names = [f[0] for f in schema]
    assert len(names) == len(set(names)), rtype + ' has duplicate field names'
    if set(rec) != set(names):
        raise TypeError('%s: fields %s expected, got %s' % (rtype, sorted(names), sorted(rec)))
    if rtype == 'PRE_COMMAND_STATE_RECORD' and any(m['state'] == 'UNKNOWN' for m in rec['members']):
        raise Unknown('a member of the PRE_COMMAND_STATE_RECORD is UNKNOWN: no partial digest (refusal O1, V2.1 6)')
    out = b''
    for name, kind, _ in sorted(schema, key=lambda f: f[0].encode('ascii')):
        v = rec[name]
        if kind == 'seq:entity':
            vals = []
            for e in v:
                et, efields = e
                if et not in ENTITY_TYPES:
                    raise Unknown('entity type %s is not in the specification' % et)
                vals.append(('record:' + et, efields))
            enc = b'[' + str(len(vals)).encode('ascii') + b'\n' + b''.join(enc_value(k, x) + b'\n' for k, x in vals) + b']'
        elif kind == 'seq:rb':
            enc = b'~' if v is None else b'[' + str(len(v)).encode('ascii') + b'\n' + b''.join(enc_value('rb', x) + b'\n' for x in v) + b']'
        else:
            enc = enc_value(kind, v)
        out += name.encode('ascii') + b'=' + enc + b'\n'
    return out


def stream(rtype, rec):
    return PREFIX + rtype.encode('ascii') + b'|' + enc_fields(rtype, rec)


def show(b):
    """Display form only (informative): backslash doubled, LF shown as \\n. The authoritative bytes are streamHex."""
    return b.decode('utf-8', errors='backslashreplace').replace('\\', '\\\\').replace('\n', '\\n')


vectors = []


def add(vid, purpose, rtype, rec, expect_unknown=False):
    try:
        s = stream(rtype, rec)
        d = hashlib.sha256(s).hexdigest()
        if expect_unknown:
            raise SystemExit(vid + ' expected UNKNOWN')
        vectors.append({'id': vid, 'purpose': purpose, 'recordType': rtype, 'result': 'DIGEST',
                        'streamHex': s.hex(), 'streamDisplay': show(s), 'sha256': d})
        return d
    except Unknown as e:
        if not expect_unknown:
            raise
        vectors.append({'id': vid, 'purpose': purpose, 'recordType': rtype, 'result': 'UNKNOWN', 'reason': str(e)})
        return None


def coord(x, y, z):
    return {'x': x, 'y': y, 'z': z}


def pt(prefix, x, y, z):
    return {prefix + 'X': x, prefix + 'Y': y, prefix + 'Z': z}


def common(dxf, layer='RACKCAD_ESTRUCTURA'):
    return {'dxfName': dxf, 'layer': layer, 'linetype': 'ByLayer', 'linetypeScale': 1.0, 'lineweight': 'ByLayer', 'visible': True,
            'colorMethod': 'ByLayer', 'colorIndex': 256, 'colorRgb': None, 'colorBookName': None, 'colorName': None,
            'transparencyMethod': 'ByLayer', 'transparencyAlpha': None}


# ---------------- family vectors (EV-01..EV-20, regenerated with the ordinal field order) ----------------
C3 = 'TEST_COORD3'
d1 = add('EV-01', 'baseline coordinate (+0.0 components)', C3, coord(1.0, 0.0, 0.0))
d2 = add('EV-02', 'SIGNED ZERO: y = -0.0 must differ from EV-01', C3, coord(1.0, -0.0, 0.0))
d3 = add('EV-03', '1 ulp above 1.0 must differ from EV-01', C3, coord(math.nextafter(1.0, 2.0), 0.0, 0.0))
add('EV-04', 'SUBNORMAL: smallest positive subnormal', C3, coord(5e-324, 0.0, 0.0))
add('EV-05', 'MAXIMUM FINITE double', C3, coord(1.7976931348623157e308, 0.0, 0.0))
add('EV-06', 'NaN is unsupported: UNKNOWN, no digest', C3, coord(float('nan'), 0.0, 0.0), expect_unknown=True)
add('EV-07', 'infinity is unsupported: UNKNOWN, no digest', C3, coord(float('inf'), 0.0, 0.0), expect_unknown=True)
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
add('EV-25', 'LONE SURROGATE (ill-formed UTF-16): UNKNOWN, no digest', 'TEST_NAME', {'name': 'a\ud800b'}, expect_unknown=True)
d26 = add('EV-26', 'PAYLOAD_EXACT, same text split in two chunks ("ab" + "c"): must differ from one chunk', 'PAYLOAD_EXACT',
          {'owner': 'SEL_FRONTAL_0', 'dictKey': 'RACKCAD_SELECTIVE', 'data': [(1, 'ab'), (1, 'c')]})
d27 = add('EV-27', 'PAYLOAD_EXACT, one chunk "abc"', 'PAYLOAD_EXACT', {'owner': 'SEL_FRONTAL_0', 'dictKey': 'RACKCAD_SELECTIVE', 'data': [(1, 'abc')]})
d28 = add('EV-28', 'PAYLOAD_EXACT, same text with type code 300 instead of 1: must differ from EV-27', 'PAYLOAD_EXACT',
          {'owner': 'SEL_FRONTAL_0', 'dictKey': 'RACKCAD_SELECTIVE', 'data': [(300, 'abc')]})
add('EV-29', 'PAYLOAD_EXACT, no payload: data absent', 'PAYLOAD_EXACT', {'owner': 'SEL_FRONTAL_0', 'dictKey': 'RACKCAD_SELECTIVE', 'data': None})
add('EV-30', 'PAYLOAD_EXACT with a handle-valued entry (code 1005): UNKNOWN', 'PAYLOAD_EXACT',
    {'owner': 'SEL_FRONTAL_0', 'dictKey': 'RACKCAD_SELECTIVE', 'data': [(1, 'x'), (1005, '1F')]}, expect_unknown=True)
add('EV-31', 'PAYLOAD_EXACT whose chunking split a surrogate pair (RackBlockData chunks at 255 UTF-16 units): UNKNOWN', 'PAYLOAD_EXACT',
    {'owner': 'SEL_FRONTAL_0', 'dictKey': 'RACKCAD_SELECTIVE', 'data': [(1, 'a\ud83d'), (1, '\ude00b')]}, expect_unknown=True)

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
          'entities': [('ENTITY_LINE', line)]}
dn = add('EV-48', 'DEFINITION_DUMP (one LINE)', 'DEFINITION_DUMP', nested)
ins = common('INSERT')
ins.update({'blockName': 'POSTE_3X3', 'blockDigest': ('POSTE_3X3', dn), 'rotation': 0.0, 'attributes': [],
            'dynamicProperties': [{'name': 'LONGITUD', 'value': ('DOUBLE', 96.0)}, {'name': 'FRENTE', 'value': ('DOUBLE', 42.0)}]})
ins.update(pt('position', 0.0, 0.0, 0.0)); ins.update(pt('scale', 1.0, 1.0, 1.0)); ins.update(pt('normal', 0.0, 0.0, 1.0))
di = add('EV-49', 'ENTITY_INSERT with nested-digest field and dynamic properties (sorted by name: FRENTE before LONGITUD)', 'ENTITY_INSERT', ins)
outer = {'name': 'SEL_FRONTAL_0', 'originX': 0.0, 'originY': 0.0, 'originZ': 0.0, 'explodable': True, 'scaling': 'Any', 'units': 'Inches', 'annotative': 'NotApplicable',
         'entities': [('ENTITY_INSERT', ins), ('ENTITY_TEXT', tx)]}
add('EV-50', 'DEFINITION_DUMP holding an INSERT (nested definition by reference) and a TEXT, in host iteration order', 'DEFINITION_DUMP', outer)
add('EV-51', 'DEFINITION_DUMP holding an entity type outside the specification (a proxy): UNKNOWN', 'DEFINITION_DUMP',
    dict(outer, entities=[('ENTITY_PROXY', {})]), expect_unknown=True)
dim = common('DIMENSION', 'RACKCAD_COTAS')
dim.update(pt('xLine1Point', 0.0, 0.0, 0.0)); dim.update(pt('xLine2Point', 96.0, 0.0, 0.0)); dim.update(pt('dimLinePoint', 0.0, -12.0, 0.0)); dim.update(pt('textPosition', 48.0, -12.0, 0.0))
dim.update({'rotation': 0.0, 'oblique': 0.0, 'dimensionStyleName': 'Standard', 'dimensionText': '', 'usingDefaultTextPosition': True,
            'overrides': [{'name': 'DIMTXT', 'value': ('DOUBLE', 4.0)}, {'name': 'DIMASZ', 'value': ('DOUBLE', 2.0)}]})
dim.update(pt('normal', 0.0, 0.0, 1.0))
add('EV-52', 'ENTITY_ROTATED_DIMENSION (overrides sorted by name)', 'ENTITY_ROTATED_DIMENSION', dim)
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
add('EV-59', 'NOD_ENTRY, an XRECORD', 'NOD_ENTRY', {'path': 'RACKCAD_PROJECT', 'objectClass': 'XRECORD', 'data': [(1, '{"Variables":[]}')], 'children': None})
add('EV-60', 'NOD_ENTRY, a DICTIONARY (children sorted)', 'NOD_ENTRY', {'path': 'RACKCAD_CUSTOM_PROPERTIES', 'objectClass': 'DICTIONARY', 'data': None,
                                                                        'children': ['b', 'A']})
add('EV-61', 'ENUMERATION, unordered names (sorted by the producer)', 'ENUMERATION', {'container': 'BLOCK_TABLE_NAMES', 'ordered': False, 'items': ['POSTE_3X3', 'SEL_FRONTAL_0']})


def member(cat, kind, name, state, dg, stored=None):
    return {'memberKey': cat + '|' + kind + '|' + name, 'category': cat, 'recordKind': kind, 'expectedName': name,
            'storedName': stored if state == 'PRESENT' else None, 'state': state, 'digest': dg if state == 'PRESENT' else None}


pcsr = {'kind': 'selective', 'members': [member('CLOSURE', 'BLOCK', 'POSTE_3X3', 'PRESENT', dn, 'POSTE_3X3'),
                                         member('CLOSURE', 'LAYER', 'RACKCAD_ANOTACIONES', 'ABSENT', None),
                                         member('READ_SET', 'BLOCK', 'SEL_FRONTAL_0', 'PRESENT', d8, 'SEL_FRONTAL_0')]}
add('EV-62', 'PRE_COMMAND_STATE_RECORD with a record-level ABSENT member', 'PRE_COMMAND_STATE_RECORD', pcsr)
pcsr_u = {'kind': 'selective', 'members': pcsr['members'][:2] + [dict(pcsr['members'][2], state='UNKNOWN', digest=None, storedName=None)]}


add('EV-63', 'PRE_COMMAND_STATE_RECORD with an UNKNOWN member: UNKNOWN', 'PRE_COMMAND_STATE_RECORD', pcsr_u, expect_unknown=True)
man = {'cloningMode': 'Ignore', 'importsCompleted': 1,
       'additions': [{'memberKey': 'CLOSURE|LAYER|RACKCAD_ANOTACIONES', 'recordKind': 'LAYER', 'storedName': 'RACKCAD_ANOTACIONES', 'digest': d9}],
       'reusedBound': [{'memberKey': 'CLOSURE|BLOCK|POSTE_3X3', 'recordKind': 'BLOCK', 'storedName': 'POSTE_3X3', 'preCommandDigest': dn}]}
add('EV-64', 'PREPARE_RESIDUE_MANIFEST', 'PREPARE_RESIDUE_MANIFEST', man)

assert d1 != d2 and d1 != d3 and d8 != d9 and d13 != d14 and d15 == d16 and d19 != d20 and d21 == d22 and d26 != d27 and d27 != d28
produced = {v['recordType'] for v in vectors}
missing = [k for k, s in SCHEMAS.items() if s['use'] == 'production' and k not in produced]
assert not missing, missing

# ---------------- semantic vectors (typed inputs; section 3.3) ----------------
H = lambda x: struct.pack('>d', x).hex()
semantic = [
    {'id': 'SV-01', 'recordType': 'ENVELOPE', 'field': 'ExtensionData', 'form': 'typed (parsed JSON)', 'expected': '{"Id":"g-1","ExtensionData":{"futureKey":1}}',
     'persisted': '{"ExtensionData":{"futureKey":1},"Id":"g-1"}', 'predicate': 'CANONICAL_JSON_EQUAL', 'result': 'EQUAL', 'rule': 'object keys compared after ordinal sorting; the extension member present and equal'},
    {'id': 'SV-02', 'recordType': 'ENVELOPE', 'field': 'ExtensionData', 'form': 'typed (parsed JSON)', 'expected': '{"Id":"g-1","ExtensionData":{"futureKey":1}}',
     'persisted': '{"Id":"g-1"}', 'predicate': 'CANONICAL_JSON_EQUAL', 'result': 'DIFFERENT', 'rule': 'a writer that DROPS an unknown member does not pass (O4 through S-2/S-3)'},
    {'id': 'SV-03', 'recordType': 'ENVELOPE', 'field': 'ExtensionData', 'form': 'typed (parsed JSON)', 'expected': '{"Id":"g-1","ExtensionData":{"futureKey":1}}',
     'persisted': '{"Id":"g-1","ExtensionData":{"futureKey":2}}', 'predicate': 'CANONICAL_JSON_EQUAL', 'result': 'DIFFERENT', 'rule': 'a writer that ALTERS an unknown member does not pass'},
    {'id': 'SV-04', 'recordType': 'DESIGN', 'field': 'a known sub-object whose type declares no ExtensionData', 'form': 'typed (parsed JSON)',
     'expected': '{"SchemaVersion":"1.0","Bays":[{"Levels":2}]}', 'persisted': '{"SchemaVersion":"1.0","Bays":[{"Levels":2,"futureKey":1}]}',
     'predicate': 'CANONICAL_JSON_EQUAL', 'result': 'UNKNOWN_MEMBER_UNSUPPORTED', 'rule': 'the unknown member sits in a sub-object with no ExtensionData (JsonExtensionData is not recursive, I-11): surfaced explicitly, gives UNKNOWN (O5 path)'},
    {'id': 'SV-05', 'recordType': 'ENTITY_TEXT', 'field': 'positionY', 'form': 'typed (binary64)', 'expected': H(0.0), 'persisted': H(-0.0),
     'predicate': 'ABS_DIFF_LE(TOL_LENGTH)', 'result': 'EQUAL', 'rule': 'signed zero is domain-equivalent; secondary finding BIT_LEVEL_DIFFERENCE_DOMAIN_EQUIVALENT; the exact digests differ (EV-01 / EV-02)'},
    {'id': 'SV-06', 'recordType': 'ENTITY_LINE', 'field': 'endX', 'form': 'typed (binary64)', 'expected': H(10.0), 'persisted': H(10.0000000005),
     'predicate': 'ABS_DIFF_LE(TOL_LENGTH)', 'result': 'EQUAL', 'rule': '|a - b| = 5e-10 <= TOL_LENGTH = 1e-9 in (per component, absolute)'},
    {'id': 'SV-07', 'recordType': 'ENTITY_LINE', 'field': 'endX', 'form': 'typed (binary64)', 'expected': H(10.0), 'persisted': H(10.000000002),
     'predicate': 'ABS_DIFF_LE(TOL_LENGTH)', 'result': 'DIFFERENT', 'rule': '|a - b| = 2e-9 > TOL_LENGTH'},
    {'id': 'SV-08', 'recordType': 'ENVELOPE', 'field': 'SchemaVersion', 'form': 'typed (string)', 'expected': '1.0', 'persisted': '9.0',
     'predicate': 'SCHEMA_VERSION_RECOGNIZED', 'result': 'UNKNOWN', 'rule': 'an unrecognized schema version is UNKNOWN'},
    {'id': 'SV-09', 'recordType': 'ENTITY_PROXY', 'field': '(record)', 'form': 'typed', 'expected': 'a record of a type outside the supported list', 'persisted': 'same',
     'predicate': 'TYPE_SUPPORTED', 'result': 'UNKNOWN', 'rule': 'layer-2 consequence: the comparison is UNKNOWN (the O1 consequence for closure members belongs to section 2.5)'},
    {'id': 'SV-10', 'recordType': 'ENTITY_INSERT', 'field': 'rotation', 'form': 'typed (binary64, radians)', 'expected': H(0.0), 'persisted': H(2 * math.pi - 5e-10),
     'predicate': 'ANGLE_DIFF_LE(TOL_ANGLE)', 'result': 'EQUAL', 'rule': 'd = |a - b| mod 2pi; min(d, 2pi - d) = 5e-10 <= TOL_ANGLE = 1e-9 rad'},
    {'id': 'SV-11', 'recordType': 'ENTITY_INSERT', 'field': 'normal', 'form': 'typed (binary64 vector)', 'expected': [H(0.0), H(0.0), H(1.0)],
     'persisted': [H(2e-9), H(0.0), H(1.0)], 'predicate': 'NORMAL_ANGLE_LE(TOL_NORMAL)', 'result': 'DIFFERENT',
     'rule': 'angle between the normalized vectors = atan(2e-9) > TOL_NORMAL = 1e-9 rad; a zero-length vector is UNKNOWN'},
    {'id': 'SV-12', 'recordType': 'ENVELOPE', 'field': 'ExtensionData.futureKey', 'form': 'typed (JSON number token)', 'expected': '1', 'persisted': '1.0',
     'predicate': 'CANONICAL_JSON_EQUAL', 'result': 'DIFFERENT', 'rule': 'PA-8 "exact numbers": an unknown or extension number is compared by its exact JSON number token (1 and 1.0 differ); whitespace and key order are not significant'},
    {'id': 'SV-13', 'recordType': 'ENTITY_TEXT', 'field': 'textString', 'form': 'typed (string)', 'expected': 'Nivel 1', 'persisted': 'NIVEL 1',
     'predicate': 'EXACT_STRING', 'result': 'DIFFERENT', 'rule': 'texts, names and enum values are compared exactly (stored case)'},
]
predicates = {
    'ABS_DIFF_LE(TOL)': '|a - b| <= TOL, per scalar component, absolute (never relative); -0.0 equals +0.0; a non-finite value is UNKNOWN',
    'ANGLE_DIFF_LE(TOL_ANGLE)': 'd = |a - b| reduced modulo 2pi into [0, 2pi); EQUAL when min(d, 2pi - d) <= TOL_ANGLE',
    'NORMAL_ANGLE_LE(TOL_NORMAL)': 'both vectors normalized (a zero-length vector is UNKNOWN); EQUAL when the angle between them <= TOL_NORMAL',
    'EXACT_STRING': 'ordinal equality of the stored text',
    'CANONICAL_JSON_EQUAL': 'both sides parsed; objects compared as maps with ordinally sorted keys; arrays by position; strings exact; numbers by exact JSON number token; a member the reader does not expose is UNKNOWN_MEMBER_UNSUPPORTED',
    'SCHEMA_VERSION_RECOGNIZED': 'an unrecognized schema version makes the comparison UNKNOWN',
    'TYPE_SUPPORTED': 'a record type outside the specification makes the comparison UNKNOWN',
}

out = {'spec': 'FPSPEC-V1', 'artifact': 'BA-05 V3', 'status': 'NORMATIVE (as part of BA-05 when sealed)',
       'encoding': {'prefix': PREFIX.decode('ascii'), 'fieldOrder': 'ordinal (UTF-8 byte order) of the field name', 'stringOrder': 'Unicode scalar value order = UTF-8 byte order',
                    'authoritativeBytes': 'streamHex (streamDisplay is informative only)', 'rbKindRanges': [[lo, hi, k] for (lo, hi), k in RB_KIND_RANGES], 'rbUnsupported': RB_UNSUPPORTED},
       'schemas': {k: {'use': s['use'], 'fields': [[f[0], f[1], f[2]] for f in s['fields']]} for k, s in SCHEMAS.items()},
       'dimvars': DIMVARS, 'exactVectors': vectors, 'semanticPredicates': predicates, 'semanticVectors': semantic}
here = os.path.dirname(os.path.abspath(__file__))
with open(os.path.join(here, 'I-52-ct21d-fpspec-v1-vectors-v3.json'), 'w', encoding='utf-8', newline='\n') as f:
    f.write(json.dumps(out, indent=1, ensure_ascii=False) + '\n')
print('exact vectors:', len(vectors), 'semantic vectors:', len(semantic), 'schemas:', len(SCHEMAS))
