#!/usr/bin/env python3
"""Schema-only pre-check of one relay record for I64-IDENTITY-BRIDGE-01 (I-64 Amendment A-13).

Authorized once by the Coordinator for the bound real relay file. It verifies the input SHA-256 first, then only parses the bytes strictly
and validates them against the official rackcad-relay-record/v1 schema, embedded as the exact blob of origin/main a140eb3a. It performs no
Git repository query (this file does not import subprocess), no Handoff Integrity evaluation, no Identity evaluation and no bridge execution.
The parser and the validator are extracted verbatim from verify_identity_bridge.py by the generator. Python standard library only.
Result: SCHEMA_VALID (exit 0), SCHEMA_INVALID (exit 1), or INPUT_SHA256_MISMATCH / a usage error (exit 2, nothing validated).
"""
import argparse
import base64
import decimal
import hashlib
import json
import re
import sys

BOM = chr(0xFEFF)


class Invalid(Exception):
    pass


class JsonInt(int):
    """A JSON integer token: exact value plus its source lexeme."""

    def __new__(cls, lexeme):
        obj = int.__new__(cls, lexeme)
        obj.lexeme = lexeme
        return obj


class JsonDecimal(decimal.Decimal):
    """A JSON fraction/exponent token: exact decimal value plus its source lexeme (never a binary float)."""

    def __new__(cls, lexeme):
        obj = decimal.Decimal.__new__(cls, lexeme)
        obj.lexeme = lexeme
        return obj


def parse_json_int(lexeme):
    try:
        return JsonInt(lexeme)
    except ValueError as e:
        raise Invalid('integer token not representable exactly: %s' % e)


def parse_json_decimal(lexeme):
    try:
        value = JsonDecimal(lexeme)
    except (decimal.InvalidOperation, ValueError) as e:
        raise Invalid('invalid numeric token %r: %s' % (lexeme, e))
    if not value.is_finite():
        raise Invalid('non-finite numeric token %r' % lexeme)
    return value


def exact_equal(x, y):
    """Exact structural equality: same type, same numeric lexeme, same nested key order."""
    if type(x) is not type(y):
        return False
    if type(x) in (JsonInt, JsonDecimal):
        return x.lexeme == y.lexeme
    if type(x) is dict:
        return list(x.keys()) == list(y.keys()) and all(exact_equal(x[k], y[k]) for k in x)
    if type(x) is list:
        return len(x) == len(y) and all(exact_equal(a, b) for a, b in zip(x, y))
    return x == y


def type_ok(value, name):
    if name == 'string':
        return type(value) is str
    if name == 'integer':
        return type(value) is JsonInt
    if name == 'boolean':
        return type(value) is bool
    if name == 'null':
        return value is None
    if name == 'array':
        return type(value) is list
    if name == 'object':
        return type(value) is dict
    raise Invalid('unsupported schema type %r' % name)


def validate(value, node, path, problems):
    supported = {'$schema', 'title', 'description', 'type', 'enum', 'pattern', 'properties', 'required', 'additionalProperties', 'items'}
    unknown = set(node) - supported
    if unknown:
        raise Invalid('unsupported schema keywords at %s: %s' % (path, sorted(unknown)))
    types = node.get('type')
    if types is not None:
        names = types if isinstance(types, list) else [types]
        if not any(type_ok(value, n) for n in names):
            problems.append('%s: expected %s' % (path, '/'.join(names)))
            return
    if 'enum' in node and not any(type(value) is type(e) and value == e for e in node['enum']):
        problems.append('%s: value not in enum' % path)
    if 'pattern' in node and type(value) is str:
        pattern = node['pattern']
        if pattern.endswith('$'):
            pattern = pattern[:-1] + chr(92) + 'Z'
        if not re.search(pattern, value):
            problems.append('%s: does not match %s' % (path, node['pattern']))
    if type(value) is dict:
        props = node.get('properties', {})
        for key in node.get('required', []):
            if key not in value:
                problems.append('%s: missing %s' % (path, key))
        if node.get('additionalProperties') is False:
            extra = [k for k in value if k not in props]
            if extra:
                problems.append('%s: unexpected keys %s' % (path, extra))
        for key, sub in props.items():
            if key in value:
                validate(value[key], sub, '%s.%s' % (path, key), problems)
    if type(value) is list and 'items' in node:
        for i, item in enumerate(value):
            validate(item, node['items'], '%s[%d]' % (path, i), problems)


def sha256(b):
    return hashlib.sha256(b).hexdigest()


def reject_duplicates(pairs):
    keys = [k for k, _ in pairs]
    if len(keys) != len(set(keys)):
        raise Invalid('duplicate JSON key')
    return dict(pairs)


def reject_constant(name):
    raise Invalid('non-JSON constant %s' % name)


def has_lone_surrogate(value):
    if isinstance(value, str):
        return any(0xD800 <= ord(c) <= 0xDFFF for c in value)
    if isinstance(value, list):
        return any(has_lone_surrogate(v) for v in value)
    if isinstance(value, dict):
        return any(has_lone_surrogate(k) or has_lone_surrogate(v) for k, v in value.items())
    return False


def read_bytes(path):
    with open(path, 'rb') as f:
        return f.read()


def load_handoff(raw):
    try:
        text = raw.decode('utf-8', errors='strict')
    except UnicodeDecodeError as e:
        raise Invalid('not strict UTF-8: %s' % e.reason)
    if text.startswith(BOM):
        raise Invalid('byte order mark present')
    try:
        obj = json.loads(text, object_pairs_hook=reject_duplicates, parse_constant=reject_constant,
                         parse_float=parse_json_decimal, parse_int=parse_json_int)
    except Invalid:
        raise
    except ValueError as e:
        raise Invalid('malformed JSON: %s' % e)
    if has_lone_surrogate(obj):
        raise Invalid('lone surrogate in JSON text')
    if not isinstance(obj, dict):
        raise Invalid('top level is not an object')
    return obj


# ---- A-13: official relay-record schema, pinned by Git blob ----
# docs/automation/agent-execution/schemas/relay-record.schema.json at origin/main a140eb3a9f055518f0ffa5a3f927ff1c0e1c1e60,
# embedded as the exact blob bytes. The inherited validate() raises on any schema keyword it does not implement.
RELAY_RECORD_SCHEMA_BLOB = '2deba19606d058fd82b389687934b0c3dfef8c06'
RELAY_RECORD_SCHEMA_B64 = (
    'ewogICIkc2NoZW1hIjogImh0dHBzOi8vanNvbi1zY2hlbWEub3JnL2RyYWZ0LzIwMjAtMTIvc2NoZW1hIiwKICAidGl0bGUiOiAicmFja2NhZC1yZWxheS1y'
    'ZWNvcmQvdjEiLAogICJkZXNjcmlwdGlvbiI6ICJSZWdpc3RybyBkZSByZWxldm8gcXVlIGVzY3JpYmUgbGEgc2VzacOzbiByZXNwb25zYWJsZSAoRnJlZXpl'
    'IGRlIEktNjEsIFByb3Bvc2FsIFY5IMKnMy4yLTMuNiB5IMKnOC4yKS4gUmVnaXN0cmEgaGVjaG9zIHkgbGEgZGlzcG9zaWNpw7NuIGRlIHVuIGZhbGxvIGRl'
    'IHRyYW5zcG9ydGU7IG51bmNhIHVuIHZlcmVkaWN0byBFWEVDVVRJT05fKi4gU3Vib3JkaW5hZG86IG5vIGNyZWEgcmVxdWlzaXRvcyBkZSBldmlkZW5jaWEs'
    'IGVzdGFkb3MsIGdhdGVzLCByZWdsYXMgR2l0IG5pIGRlY2lzaW9uZXMgZGVsIE93bmVyLiIsCiAgInR5cGUiOiAib2JqZWN0IiwKICAiYWRkaXRpb25hbFBy'
    'b3BlcnRpZXMiOiBmYWxzZSwKICAicHJvcGVydGllcyI6IHsKICAgICJTY2hlbWEiOiB7ICJ0eXBlIjogInN0cmluZyIsICJlbnVtIjogWyJyYWNrY2FkLXJl'
    'bGF5LXJlY29yZC92MSJdIH0sCiAgICAiVGFza0lkIjogeyAidHlwZSI6ICJzdHJpbmciIH0sCiAgICAiUnVuSWQiOiB7ICJ0eXBlIjogInN0cmluZyIsICJw'
    'YXR0ZXJuIjogIl5SWzAtOV17OH1UWzAtOV17Nn1aLVswLTlhLWZdezR9JCIgfSwKICAgICJBdHRlbXB0IjogeyAidHlwZSI6ICJpbnRlZ2VyIiB9LAogICAg'
    'IlBoYXNlIjogeyAidHlwZSI6ICJzdHJpbmciLCAiZW51bSI6IFsiUExBTk5JTkciLCAiV09SSyIsICJWRVJJRklDQVRJT04iLCAiQ09OVFJPTCIsICJQUk9C'
    'RSIsICJSRUJBU0UiXSB9LAogICAgIlBhcnRpY2lwYW50IjogewogICAgICAidHlwZSI6IFsib2JqZWN0IiwgIm51bGwiXSwKICAgICAgImFkZGl0aW9uYWxQ'
    'cm9wZXJ0aWVzIjogZmFsc2UsCiAgICAgICJwcm9wZXJ0aWVzIjogewogICAgICAgICJLaW5kIjogeyAidHlwZSI6ICJzdHJpbmciLCAiZW51bSI6IFsiY29k'
    'ZXgtY2xpIiwgInN1YmFnZW50Il0gfSwKICAgICAgICAiUHJvZmlsZSI6IHsgInR5cGUiOiBbInN0cmluZyIsICJudWxsIl0gfSwKICAgICAgICAiTW9kZWxS'
    'ZXF1ZXN0ZWQiOiB7ICJ0eXBlIjogWyJzdHJpbmciLCAibnVsbCJdIH0sCiAgICAgICAgIkVmZm9ydFJlcXVlc3RlZCI6IHsgInR5cGUiOiBbInN0cmluZyIs'
    'ICJudWxsIl0gfSwKICAgICAgICAiTW9kZWxFZmZlY3RpdmUiOiB7ICJ0eXBlIjogWyJzdHJpbmciLCAibnVsbCJdIH0sCiAgICAgICAgIkVmZm9ydEVmZmVj'
    'dGl2ZSI6IHsgInR5cGUiOiBbInN0cmluZyIsICJudWxsIl0gfSwKICAgICAgICAiRWZmZWN0aXZlU291cmNlIjogeyAidHlwZSI6IFsic3RyaW5nIiwgIm51'
    'bGwiXSB9LAogICAgICAgICJFZmZlY3RpdmVTb3VyY2VTaGEyNTYiOiB7ICJ0eXBlIjogWyJzdHJpbmciLCAibnVsbCJdLCAicGF0dGVybiI6ICJeWzAtOUEt'
    'RmEtZl17NjR9JCIgfQogICAgICB9LAogICAgICAicmVxdWlyZWQiOiBbIktpbmQiLCAiUHJvZmlsZSIsICJNb2RlbFJlcXVlc3RlZCIsICJFZmZvcnRSZXF1'
    'ZXN0ZWQiLCAiTW9kZWxFZmZlY3RpdmUiLCAiRWZmb3J0RWZmZWN0aXZlIiwgIkVmZmVjdGl2ZVNvdXJjZSIsICJFZmZlY3RpdmVTb3VyY2VTaGEyNTYiXQog'
    'ICAgfSwKICAgICJFeGl0IjogewogICAgICAidHlwZSI6ICJvYmplY3QiLAogICAgICAiYWRkaXRpb25hbFByb3BlcnRpZXMiOiBmYWxzZSwKICAgICAgInBy'
    'b3BlcnRpZXMiOiB7CiAgICAgICAgIkhlYWRTaGEiOiB7ICJ0eXBlIjogInN0cmluZyIsICJwYXR0ZXJuIjogIl5bMC05YS1mXXs0MH0kIiB9LAogICAgICAg'
    'ICJMc1JlbW90ZVNoYSI6IHsgInR5cGUiOiAic3RyaW5nIiwgInBhdHRlcm4iOiAiXlswLTlhLWZdezQwfSQiIH0sCiAgICAgICAgIk9yaWdpbk1haW5TaGEi'
    'OiB7ICJ0eXBlIjogInN0cmluZyIsICJwYXR0ZXJuIjogIl5bMC05YS1mXXs0MH0kIiB9LAogICAgICAgICJDbGVhblRyZWUiOiB7ICJ0eXBlIjogImJvb2xl'
    'YW4iIH0sCiAgICAgICAgIkdpdE9wZXJhdGlvbkluUHJvZ3Jlc3MiOiB7ICJ0eXBlIjogImJvb2xlYW4iIH0sCiAgICAgICAgIlN1bW1hcnlDb21taXRTaGEi'
    'OiB7ICJ0eXBlIjogInN0cmluZyIsICJwYXR0ZXJuIjogIl5bMC05YS1mXXs0MH0kIiB9LAogICAgICAgICJDb25maWdUb21sU2hhMjU2IjogeyAidHlwZSI6'
    'ICJzdHJpbmciLCAicGF0dGVybiI6ICJeWzAtOUEtRmEtZl17NjR9JCIgfSwKICAgICAgICAiQ29uZmlnVG9tbEtleXMiOiB7ICJ0eXBlIjogImFycmF5Iiwg'
    'Iml0ZW1zIjogeyAidHlwZSI6ICJzdHJpbmciIH0gfSwKICAgICAgICAiUHJvY2Vzc2VzIjogewogICAgICAgICAgInR5cGUiOiAiYXJyYXkiLAogICAgICAg'
    'ICAgIml0ZW1zIjogewogICAgICAgICAgICAidHlwZSI6ICJvYmplY3QiLAogICAgICAgICAgICAiYWRkaXRpb25hbFByb3BlcnRpZXMiOiBmYWxzZSwKICAg'
    'ICAgICAgICAgInByb3BlcnRpZXMiOiB7CiAgICAgICAgICAgICAgIlBpZCI6IHsgInR5cGUiOiAiaW50ZWdlciIgfSwKICAgICAgICAgICAgICAiUGFyZW50'
    'UGlkIjogeyAidHlwZSI6ICJpbnRlZ2VyIiB9LAogICAgICAgICAgICAgICJOYW1lIjogeyAidHlwZSI6ICJzdHJpbmciIH0sCiAgICAgICAgICAgICAgIkNy'
    'ZWF0aW9uRGF0ZVV0YyI6IHsgInR5cGUiOiAic3RyaW5nIiB9LAogICAgICAgICAgICAgICJDbGFzc2lmaWNhdGlvbiI6IHsgInR5cGUiOiAic3RyaW5nIiwg'
    'ImVudW0iOiBbIm93bi10cmVlIiwgIm93bmVyLWFwcCIsICJub21pbmFsLWV4Y2x1c2lvbiIsICJidWlsZC1zZXJ2ZXIiLCAicGFydGljaXBhbnQiLCAidW5h'
    'dHRyaWJ1dGFibGUiLCAic2Vzc2lvbi1kZXNjZW5kYW50IiwgIm9ycGhhbiJdIH0KICAgICAgICAgICAgfSwKICAgICAgICAgICAgInJlcXVpcmVkIjogWyJQ'
    'aWQiLCAiUGFyZW50UGlkIiwgIk5hbWUiLCAiQ3JlYXRpb25EYXRlVXRjIiwgIkNsYXNzaWZpY2F0aW9uIl0KICAgICAgICAgIH0KICAgICAgICB9LAogICAg'
    'ICAgICJPcGVuRGVsZWdhdGlvbnMiOiB7ICJ0eXBlIjogImludGVnZXIiIH0KICAgICAgfSwKICAgICAgInJlcXVpcmVkIjogWyJIZWFkU2hhIiwgIkxzUmVt'
    'b3RlU2hhIiwgIk9yaWdpbk1haW5TaGEiLCAiQ2xlYW5UcmVlIiwgIkdpdE9wZXJhdGlvbkluUHJvZ3Jlc3MiLCAiU3VtbWFyeUNvbW1pdFNoYSIsICJDb25m'
    'aWdUb21sU2hhMjU2IiwgIkNvbmZpZ1RvbWxLZXlzIiwgIlByb2Nlc3NlcyIsICJPcGVuRGVsZWdhdGlvbnMiXQogICAgfSwKICAgICJDZXNzaW9uIjogewog'
    'ICAgICAidHlwZSI6ICJvYmplY3QiLAogICAgICAiYWRkaXRpb25hbFByb3BlcnRpZXMiOiBmYWxzZSwKICAgICAgInByb3BlcnRpZXMiOiB7CiAgICAgICAg'
    'IlN0YXJ0VXRjIjogeyAidHlwZSI6ICJzdHJpbmciIH0sCiAgICAgICAgIkVuZFV0YyI6IHsgInR5cGUiOiBbInN0cmluZyIsICJudWxsIl0gfSwKICAgICAg'
    'ICAiU2Vzc2lvbk9wZXJhdGVkIjogeyAidHlwZSI6ICJib29sZWFuIiB9CiAgICAgIH0sCiAgICAgICJyZXF1aXJlZCI6IFsiU3RhcnRVdGMiLCAiRW5kVXRj'
    'IiwgIlNlc3Npb25PcGVyYXRlZCJdCiAgICB9LAogICAgIk91dGNvbWUiOiB7CiAgICAgICJ0eXBlIjogIm9iamVjdCIsCiAgICAgICJhZGRpdGlvbmFsUHJv'
    'cGVydGllcyI6IGZhbHNlLAogICAgICAicHJvcGVydGllcyI6IHsKICAgICAgICAiS2luZCI6IHsgInR5cGUiOiAic3RyaW5nIiwgImVudW0iOiBbIkNPTVBM'
    'RVRFRCIsICJUSU1FT1VUIiwgIkZBSUxFRF9UVVJOIiwgIk5PX09VVFBVVCIsICJJTlZBTElEX09VVFBVVCIsICJSRUpFQ1RFRF9CRUZPUkVfSU5WT0NBVElP'
    'TiJdIH0sCiAgICAgICAgIkxhdW5jaGVkUGlkIjogeyAidHlwZSI6IFsiaW50ZWdlciIsICJudWxsIl0gfSwKICAgICAgICAiRXhpdENvZGUiOiB7ICJ0eXBl'
    'IjogWyJpbnRlZ2VyIiwgIm51bGwiXSB9LAogICAgICAgICJUZXJtaW5hbEV2ZW50IjogeyAidHlwZSI6IFsic3RyaW5nIiwgIm51bGwiXSB9LAogICAgICAg'
    'ICJUcmVlS2lsbGVkIjogeyAidHlwZSI6ICJib29sZWFuIiB9LAogICAgICAgICJEZWF0aENvbmZpcm1lZCI6IHsgInR5cGUiOiBbImJvb2xlYW4iLCAibnVs'
    'bCJdIH0sCiAgICAgICAgIk91dHB1dFBhdGgiOiB7ICJ0eXBlIjogWyJzdHJpbmciLCAibnVsbCJdIH0sCiAgICAgICAgIk91dHB1dFNoYTI1NiI6IHsgInR5'
    'cGUiOiBbInN0cmluZyIsICJudWxsIl0sICJwYXR0ZXJuIjogIl5bMC05QS1GYS1mXXs2NH0kIiB9LAogICAgICAgICJPdXRwdXRXcml0dGVuVXRjIjogeyAi'
    'dHlwZSI6IFsic3RyaW5nIiwgIm51bGwiXSB9LAogICAgICAgICJBY2NlcHRhbmNlIjogewogICAgICAgICAgInR5cGUiOiBbIm9iamVjdCIsICJudWxsIl0s'
    'CiAgICAgICAgICAiYWRkaXRpb25hbFByb3BlcnRpZXMiOiBmYWxzZSwKICAgICAgICAgICJwcm9wZXJ0aWVzIjogewogICAgICAgICAgICAiQTEiOiB7ICJ0'
    'eXBlIjogInN0cmluZyIsICJlbnVtIjogWyJwYXNzIiwgImZhaWwiXSB9LAogICAgICAgICAgICAiQTIiOiB7ICJ0eXBlIjogInN0cmluZyIsICJlbnVtIjog'
    'WyJwYXNzIiwgImZhaWwiXSB9LAogICAgICAgICAgICAiQTMiOiB7ICJ0eXBlIjogInN0cmluZyIsICJlbnVtIjogWyJwYXNzIiwgImZhaWwiXSB9LAogICAg'
    'ICAgICAgICAiQTQiOiB7ICJ0eXBlIjogInN0cmluZyIsICJlbnVtIjogWyJwYXNzIiwgImZhaWwiXSB9LAogICAgICAgICAgICAiQTUiOiB7ICJ0eXBlIjog'
    'InN0cmluZyIsICJlbnVtIjogWyJwYXNzIiwgImZhaWwiXSB9LAogICAgICAgICAgICAiQTYiOiB7ICJ0eXBlIjogInN0cmluZyIsICJlbnVtIjogWyJwYXNz'
    'IiwgImZhaWwiXSB9LAogICAgICAgICAgICAiQTciOiB7ICJ0eXBlIjogInN0cmluZyIsICJlbnVtIjogWyJwYXNzIiwgImZhaWwiXSB9LAogICAgICAgICAg'
    'ICAiQTgiOiB7ICJ0eXBlIjogInN0cmluZyIsICJlbnVtIjogWyJwYXNzIiwgImZhaWwiXSB9CiAgICAgICAgICB9LAogICAgICAgICAgInJlcXVpcmVkIjog'
    'WyJBMSIsICJBMiIsICJBMyIsICJBNCIsICJBNSIsICJBNiIsICJBNyIsICJBOCJdCiAgICAgICAgfQogICAgICB9LAogICAgICAicmVxdWlyZWQiOiBbIktp'
    'bmQiLCAiTGF1bmNoZWRQaWQiLCAiRXhpdENvZGUiLCAiVGVybWluYWxFdmVudCIsICJUcmVlS2lsbGVkIiwgIkRlYXRoQ29uZmlybWVkIiwgIk91dHB1dFBh'
    'dGgiLCAiT3V0cHV0U2hhMjU2IiwgIk91dHB1dFdyaXR0ZW5VdGMiLCAiQWNjZXB0YW5jZSJdCiAgICB9LAogICAgIkVudHJ5IjogewogICAgICAidHlwZSI6'
    'IFsib2JqZWN0IiwgIm51bGwiXSwKICAgICAgImFkZGl0aW9uYWxQcm9wZXJ0aWVzIjogZmFsc2UsCiAgICAgICJwcm9wZXJ0aWVzIjogewogICAgICAgICJI'
    'ZWFkU2hhIjogeyAidHlwZSI6ICJzdHJpbmciLCAicGF0dGVybiI6ICJeWzAtOWEtZl17NDB9JCIgfSwKICAgICAgICAiTHNSZW1vdGVTaGEiOiB7ICJ0eXBl'
    'IjogInN0cmluZyIsICJwYXR0ZXJuIjogIl5bMC05YS1mXXs0MH0kIiB9LAogICAgICAgICJDbGVhblRyZWUiOiB7ICJ0eXBlIjogImJvb2xlYW4iIH0sCiAg'
    'ICAgICAgIkNvbmZpZ1RvbWxTaGEyNTYiOiB7ICJ0eXBlIjogInN0cmluZyIsICJwYXR0ZXJuIjogIl5bMC05QS1GYS1mXXs2NH0kIiB9LAogICAgICAgICJD'
    'b25maWdUb21sS2V5cyI6IHsgInR5cGUiOiAiYXJyYXkiLCAiaXRlbXMiOiB7ICJ0eXBlIjogInN0cmluZyIgfSB9LAogICAgICAgICJDb25maWdDaGFuZ2Vk'
    'IjogeyAidHlwZSI6ICJib29sZWFuIiB9LAogICAgICAgICJQcm9jZXNzZXMiOiB7CiAgICAgICAgICAidHlwZSI6ICJhcnJheSIsCiAgICAgICAgICAiaXRl'
    'bXMiOiB7CiAgICAgICAgICAgICJ0eXBlIjogIm9iamVjdCIsCiAgICAgICAgICAgICJhZGRpdGlvbmFsUHJvcGVydGllcyI6IGZhbHNlLAogICAgICAgICAg'
    'ICAicHJvcGVydGllcyI6IHsKICAgICAgICAgICAgICAiUGlkIjogeyAidHlwZSI6ICJpbnRlZ2VyIiB9LAogICAgICAgICAgICAgICJQYXJlbnRQaWQiOiB7'
    'ICJ0eXBlIjogImludGVnZXIiIH0sCiAgICAgICAgICAgICAgIk5hbWUiOiB7ICJ0eXBlIjogInN0cmluZyIgfSwKICAgICAgICAgICAgICAiQ3JlYXRpb25E'
    'YXRlVXRjIjogeyAidHlwZSI6ICJzdHJpbmciIH0sCiAgICAgICAgICAgICAgIkNsYXNzaWZpY2F0aW9uIjogeyAidHlwZSI6ICJzdHJpbmciLCAiZW51bSI6'
    'IFsib3duLXRyZWUiLCAib3duZXItYXBwIiwgIm5vbWluYWwtZXhjbHVzaW9uIiwgImJ1aWxkLXNlcnZlciIsICJwYXJ0aWNpcGFudCIsICJ1bmF0dHJpYnV0'
    'YWJsZSIsICJzZXNzaW9uLWRlc2NlbmRhbnQiLCAib3JwaGFuIl0gfQogICAgICAgICAgICB9LAogICAgICAgICAgICAicmVxdWlyZWQiOiBbIlBpZCIsICJQ'
    'YXJlbnRQaWQiLCAiTmFtZSIsICJDcmVhdGlvbkRhdGVVdGMiLCAiQ2xhc3NpZmljYXRpb24iXQogICAgICAgICAgfQogICAgICAgIH0KICAgICAgfSwKICAg'
    'ICAgInJlcXVpcmVkIjogWyJIZWFkU2hhIiwgIkxzUmVtb3RlU2hhIiwgIkNsZWFuVHJlZSIsICJDb25maWdUb21sU2hhMjU2IiwgIkNvbmZpZ1RvbWxLZXlz'
    'IiwgIkNvbmZpZ0NoYW5nZWQiLCAiUHJvY2Vzc2VzIl0KICAgIH0sCiAgICAiUmVtb3RlRmFjdHMiOiB7CiAgICAgICJ0eXBlIjogWyJvYmplY3QiLCAibnVs'
    'bCJdLAogICAgICAiYWRkaXRpb25hbFByb3BlcnRpZXMiOiBmYWxzZSwKICAgICAgInByb3BlcnRpZXMiOiB7CiAgICAgICAgIkN1cnJlbnRSdW4iOiB7CiAg'
    'ICAgICAgICAidHlwZSI6IFsib2JqZWN0IiwgIm51bGwiXSwKICAgICAgICAgICJhZGRpdGlvbmFsUHJvcGVydGllcyI6IGZhbHNlLAogICAgICAgICAgInBy'
    'b3BlcnRpZXMiOiB7CiAgICAgICAgICAgICJHaFJ1bklkIjogeyAidHlwZSI6ICJpbnRlZ2VyIiB9LAogICAgICAgICAgICAiRXZlbnQiOiB7ICJ0eXBlIjog'
    'InN0cmluZyIgfSwKICAgICAgICAgICAgIlJlZiI6IHsgInR5cGUiOiAic3RyaW5nIiB9LAogICAgICAgICAgICAiSGVhZFNoYSI6IHsgInR5cGUiOiAic3Ry'
    'aW5nIiwgInBhdHRlcm4iOiAiXlswLTlhLWZdezQwfSQiIH0sCiAgICAgICAgICAgICJTdGF0dXMiOiB7ICJ0eXBlIjogInN0cmluZyIgfSwKICAgICAgICAg'
    'ICAgIkNvbmNsdXNpb24iOiB7ICJ0eXBlIjogWyJzdHJpbmciLCAibnVsbCJdIH0sCiAgICAgICAgICAgICJKb2JzIjogewogICAgICAgICAgICAgICJ0eXBl'
    'IjogImFycmF5IiwKICAgICAgICAgICAgICAiaXRlbXMiOiB7CiAgICAgICAgICAgICAgICAidHlwZSI6ICJvYmplY3QiLAogICAgICAgICAgICAgICAgImFk'
    'ZGl0aW9uYWxQcm9wZXJ0aWVzIjogZmFsc2UsCiAgICAgICAgICAgICAgICAicHJvcGVydGllcyI6IHsgIk5hbWUiOiB7ICJ0eXBlIjogInN0cmluZyIgfSwg'
    'IkNvbmNsdXNpb24iOiB7ICJ0eXBlIjogWyJzdHJpbmciLCAibnVsbCJdIH0gfSwKICAgICAgICAgICAgICAgICJyZXF1aXJlZCI6IFsiTmFtZSIsICJDb25j'
    'bHVzaW9uIl0KICAgICAgICAgICAgICB9CiAgICAgICAgICAgIH0KICAgICAgICAgIH0sCiAgICAgICAgICAicmVxdWlyZWQiOiBbIkdoUnVuSWQiLCAiRXZl'
    'bnQiLCAiUmVmIiwgIkhlYWRTaGEiLCAiU3RhdHVzIiwgIkNvbmNsdXNpb24iLCAiSm9icyJdCiAgICAgICAgfSwKICAgICAgICAiUmVkUnVuIjogewogICAg'
    'ICAgICAgInR5cGUiOiBbIm9iamVjdCIsICJudWxsIl0sCiAgICAgICAgICAiYWRkaXRpb25hbFByb3BlcnRpZXMiOiBmYWxzZSwKICAgICAgICAgICJwcm9w'
    'ZXJ0aWVzIjogewogICAgICAgICAgICAiR2hSdW5JZCI6IHsgInR5cGUiOiAiaW50ZWdlciIgfSwKICAgICAgICAgICAgIkV2ZW50IjogeyAidHlwZSI6ICJz'
    'dHJpbmciIH0sCiAgICAgICAgICAgICJSZWYiOiB7ICJ0eXBlIjogInN0cmluZyIgfSwKICAgICAgICAgICAgIkhlYWRTaGEiOiB7ICJ0eXBlIjogInN0cmlu'
    'ZyIsICJwYXR0ZXJuIjogIl5bMC05YS1mXXs0MH0kIiB9LAogICAgICAgICAgICAiU3RhdHVzIjogeyAidHlwZSI6ICJzdHJpbmciIH0sCiAgICAgICAgICAg'
    'ICJDb25jbHVzaW9uIjogeyAidHlwZSI6IFsic3RyaW5nIiwgIm51bGwiXSB9LAogICAgICAgICAgICAiSm9icyI6IHsKICAgICAgICAgICAgICAidHlwZSI6'
    'ICJhcnJheSIsCiAgICAgICAgICAgICAgIml0ZW1zIjogewogICAgICAgICAgICAgICAgInR5cGUiOiAib2JqZWN0IiwKICAgICAgICAgICAgICAgICJhZGRp'
    'dGlvbmFsUHJvcGVydGllcyI6IGZhbHNlLAogICAgICAgICAgICAgICAgInByb3BlcnRpZXMiOiB7ICJOYW1lIjogeyAidHlwZSI6ICJzdHJpbmciIH0sICJD'
    'b25jbHVzaW9uIjogeyAidHlwZSI6IFsic3RyaW5nIiwgIm51bGwiXSB9IH0sCiAgICAgICAgICAgICAgICAicmVxdWlyZWQiOiBbIk5hbWUiLCAiQ29uY2x1'
    'c2lvbiJdCiAgICAgICAgICAgICAgfQogICAgICAgICAgICB9CiAgICAgICAgICB9LAogICAgICAgICAgInJlcXVpcmVkIjogWyJHaFJ1bklkIiwgIkV2ZW50'
    'IiwgIlJlZiIsICJIZWFkU2hhIiwgIlN0YXR1cyIsICJDb25jbHVzaW9uIiwgIkpvYnMiXQogICAgICAgIH0sCiAgICAgICAgIlRlc3RBcnRpZmFjdHMiOiB7'
    'CiAgICAgICAgICAidHlwZSI6ICJhcnJheSIsCiAgICAgICAgICAiaXRlbXMiOiB7CiAgICAgICAgICAgICJ0eXBlIjogIm9iamVjdCIsCiAgICAgICAgICAg'
    'ICJhZGRpdGlvbmFsUHJvcGVydGllcyI6IGZhbHNlLAogICAgICAgICAgICAicHJvcGVydGllcyI6IHsKICAgICAgICAgICAgICAiUnVuUmVmIjogeyAidHlw'
    'ZSI6ICJzdHJpbmciIH0sCiAgICAgICAgICAgICAgIk5hbWUiOiB7ICJ0eXBlIjogInN0cmluZyIgfSwKICAgICAgICAgICAgICAiU2hhMjU2IjogeyAidHlw'
    'ZSI6ICJzdHJpbmciLCAicGF0dGVybiI6ICJeWzAtOUEtRmEtZl17NjR9JCIgfSwKICAgICAgICAgICAgICAiU2VsZWN0ZWQiOiB7ICJ0eXBlIjogImludGVn'
    'ZXIiIH0sCiAgICAgICAgICAgICAgIlBhc3NlZCI6IHsgInR5cGUiOiAiaW50ZWdlciIgfSwKICAgICAgICAgICAgICAiRmFpbGVkIjogeyAidHlwZSI6ICJp'
    'bnRlZ2VyIiB9LAogICAgICAgICAgICAgICJGYWlsZWRUZXN0cyI6IHsgInR5cGUiOiAiYXJyYXkiLCAiaXRlbXMiOiB7ICJ0eXBlIjogInN0cmluZyIgfSB9'
    'CiAgICAgICAgICAgIH0sCiAgICAgICAgICAgICJyZXF1aXJlZCI6IFsiUnVuUmVmIiwgIk5hbWUiLCAiU2hhMjU2IiwgIlNlbGVjdGVkIiwgIlBhc3NlZCIs'
    'ICJGYWlsZWQiLCAiRmFpbGVkVGVzdHMiXQogICAgICAgICAgfQogICAgICAgIH0sCiAgICAgICAgIk9yaWdpbk1haW5TaGEiOiB7ICJ0eXBlIjogInN0cmlu'
    'ZyIsICJwYXR0ZXJuIjogIl5bMC05YS1mXXs0MH0kIiB9LAogICAgICAgICJMc1JlbW90ZVNoYSI6IHsgInR5cGUiOiAic3RyaW5nIiwgInBhdHRlcm4iOiAi'
    'XlswLTlhLWZdezQwfSQiIH0sCiAgICAgICAgIkNvbW1hbmRzIjogeyAidHlwZSI6ICJhcnJheSIsICJpdGVtcyI6IHsgInR5cGUiOiAic3RyaW5nIiB9IH0s'
    'CiAgICAgICAgIk9idGFpbmVkVXRjIjogeyAidHlwZSI6ICJzdHJpbmciIH0sCiAgICAgICAgIkRpYWdub3NpcyI6IHsgInR5cGUiOiBbInN0cmluZyIsICJu'
    'dWxsIl0gfQogICAgICB9LAogICAgICAicmVxdWlyZWQiOiBbIkN1cnJlbnRSdW4iLCAiUmVkUnVuIiwgIlRlc3RBcnRpZmFjdHMiLCAiT3JpZ2luTWFpblNo'
    'YSIsICJMc1JlbW90ZVNoYSIsICJDb21tYW5kcyIsICJPYnRhaW5lZFV0YyIsICJEaWFnbm9zaXMiXQogICAgfSwKICAgICJSZWJhc2VNYXAiOiB7CiAgICAg'
    'ICJ0eXBlIjogWyJvYmplY3QiLCAibnVsbCJdLAogICAgICAiYWRkaXRpb25hbFByb3BlcnRpZXMiOiBmYWxzZSwKICAgICAgInByb3BlcnRpZXMiOiB7CiAg'
    'ICAgICAgIk1haW5CZWZvcmVTaGEiOiB7ICJ0eXBlIjogInN0cmluZyIsICJwYXR0ZXJuIjogIl5bMC05YS1mXXs0MH0kIiB9LAogICAgICAgICJNYWluQWZ0'
    'ZXJTaGEiOiB7ICJ0eXBlIjogInN0cmluZyIsICJwYXR0ZXJuIjogIl5bMC05YS1mXXs0MH0kIiB9LAogICAgICAgICJHMkNsb3NlU2hhIjogeyAidHlwZSI6'
    'ICJzdHJpbmciLCAicGF0dGVybiI6ICJeWzAtOWEtZl17NDB9JCIgfSwKICAgICAgICAiRzJDbG9zZUltYWdlU2hhIjogeyAidHlwZSI6IFsic3RyaW5nIiwg'
    'Im51bGwiXSwgInBhdHRlcm4iOiAiXlswLTlhLWZdezQwfSQiIH0sCiAgICAgICAgIkF1dGhvcml0eVJldmlzaW9uIjogeyAidHlwZSI6ICJzdHJpbmciLCAi'
    'cGF0dGVybiI6ICJeWzAtOWEtZl17NDB9JCIgfSwKICAgICAgICAiQXV0aG9yaXR5UmV2aXNpb25JbWFnZVNoYSI6IHsgInR5cGUiOiBbInN0cmluZyIsICJu'
    'dWxsIl0sICJwYXR0ZXJuIjogIl5bMC05YS1mXXs0MH0kIiB9LAogICAgICAgICJVbml0RG9jQmxvYnNFcXVhbCI6IHsgInR5cGUiOiBbImJvb2xlYW4iLCAi'
    'bnVsbCJdIH0sCiAgICAgICAgIlVuaXRDaGFuZ2VTZWN0aW9uc0VxdWFsIjogeyAidHlwZSI6IFsiYm9vbGVhbiIsICJudWxsIl0gfSwKICAgICAgICAiQmFz'
    'ZVNoYSI6IHsgInR5cGUiOiBbInN0cmluZyIsICJudWxsIl0sICJwYXR0ZXJuIjogIl5bMC05YS1mXXs0MH0kIiB9LAogICAgICAgICJCYXNlSW1hZ2VTaGEi'
    'OiB7ICJ0eXBlIjogWyJzdHJpbmciLCAibnVsbCJdLCAicGF0dGVybiI6ICJeWzAtOWEtZl17NDB9JCIgfSwKICAgICAgICAiQ2hhaW5CYXNlU2hhIjogeyAi'
    'dHlwZSI6IFsic3RyaW5nIiwgIm51bGwiXSwgInBhdHRlcm4iOiAiXlswLTlhLWZdezQwfSQiIH0sCiAgICAgICAgIkNoYWluQmFzZUltYWdlU2hhIjogeyAi'
    'dHlwZSI6IFsic3RyaW5nIiwgIm51bGwiXSwgInBhdHRlcm4iOiAiXlswLTlhLWZdezQwfSQiIH0sCiAgICAgICAgIkNvbW1pdHMiOiB7CiAgICAgICAgICAi'
    'dHlwZSI6ICJhcnJheSIsCiAgICAgICAgICAiaXRlbXMiOiB7CiAgICAgICAgICAgICJ0eXBlIjogIm9iamVjdCIsCiAgICAgICAgICAgICJhZGRpdGlvbmFs'
    'UHJvcGVydGllcyI6IGZhbHNlLAogICAgICAgICAgICAicHJvcGVydGllcyI6IHsKICAgICAgICAgICAgICAiT3JpZ2luYWxTaGEiOiB7ICJ0eXBlIjogInN0'
    'cmluZyIsICJwYXR0ZXJuIjogIl5bMC05YS1mXXs0MH0kIiB9LAogICAgICAgICAgICAgICJJbWFnZVNoYSI6IHsgInR5cGUiOiBbInN0cmluZyIsICJudWxs'
    'Il0sICJwYXR0ZXJuIjogIl5bMC05YS1mXXs0MH0kIiB9LAogICAgICAgICAgICAgICJQYXRjaElkRXF1YWwiOiB7ICJ0eXBlIjogWyJib29sZWFuIiwgIm51'
    'bGwiXSB9CiAgICAgICAgICAgIH0sCiAgICAgICAgICAgICJyZXF1aXJlZCI6IFsiT3JpZ2luYWxTaGEiLCAiSW1hZ2VTaGEiLCAiUGF0Y2hJZEVxdWFsIl0K'
    'ICAgICAgICAgIH0KICAgICAgICB9LAogICAgICAgICJDb25mbGljdFBhdGhzIjogeyAidHlwZSI6ICJhcnJheSIsICJpdGVtcyI6IHsgInR5cGUiOiAic3Ry'
    'aW5nIiB9IH0sCiAgICAgICAgIkNvb3JkaW5hdG9yRGVjaXNpb24iOiB7ICJ0eXBlIjogWyJzdHJpbmciLCAibnVsbCJdIH0KICAgICAgfSwKICAgICAgInJl'
    'cXVpcmVkIjogWwogICAgICAgICJNYWluQmVmb3JlU2hhIiwgIk1haW5BZnRlclNoYSIsICJHMkNsb3NlU2hhIiwgIkcyQ2xvc2VJbWFnZVNoYSIsICJBdXRo'
    'b3JpdHlSZXZpc2lvbiIsICJBdXRob3JpdHlSZXZpc2lvbkltYWdlU2hhIiwKICAgICAgICAiVW5pdERvY0Jsb2JzRXF1YWwiLCAiVW5pdENoYW5nZVNlY3Rp'
    'b25zRXF1YWwiLCAiQmFzZVNoYSIsICJCYXNlSW1hZ2VTaGEiLCAiQ2hhaW5CYXNlU2hhIiwgIkNoYWluQmFzZUltYWdlU2hhIiwgIkNvbW1pdHMiLAogICAg'
    'ICAgICJDb25mbGljdFBhdGhzIiwgIkNvb3JkaW5hdG9yRGVjaXNpb24iCiAgICAgIF0KICAgIH0sCiAgICAiRGlzcG9zaXRpb24iOiB7ICJ0eXBlIjogInN0'
    'cmluZyIsICJlbnVtIjogWyJOT05FIiwgIkJMT0NLRUQiLCAiU1RPUCJdIH0sCiAgICAiTm90ZXMiOiB7ICJ0eXBlIjogImFycmF5IiwgIml0ZW1zIjogeyAi'
    'dHlwZSI6ICJzdHJpbmciIH0gfQogIH0sCiAgInJlcXVpcmVkIjogWyJTY2hlbWEiLCAiVGFza0lkIiwgIlJ1bklkIiwgIkF0dGVtcHQiLCAiUGhhc2UiLCAi'
    'UGFydGljaXBhbnQiLCAiRXhpdCIsICJDZXNzaW9uIiwgIk91dGNvbWUiLCAiRW50cnkiLCAiUmVtb3RlRmFjdHMiLCAiUmViYXNlTWFwIiwgIkRpc3Bvc2l0'
    'aW9uIiwgIk5vdGVzIl0KfQo='
)


def git_blob_id(data):
    return hashlib.sha1(b'blob %d\0' % len(data) + data).hexdigest()


def relay_record_schema():
    try:
        raw = base64.b64decode(RELAY_RECORD_SCHEMA_B64, validate=True)
    except ValueError as e:
        raise Invalid('embedded relay schema is not valid base64: %s' % e)
    if git_blob_id(raw) != RELAY_RECORD_SCHEMA_BLOB:
        raise Invalid('embedded relay schema does not match the pinned Git blob')
    return load_handoff(raw)


def relay_schema_problems(obj):
    """Complete validation against the pinned official schema. The semantic relay checks come later and never replace it."""
    problems = []
    try:
        validate(obj, relay_record_schema(), '$', problems)
    except Invalid as e:
        problems.append(str(e))
    return problems


class UsageError(Exception):
    pass


class Parser(argparse.ArgumentParser):
    def error(self, message):
        raise UsageError(message)


def precheck(relay_path, expected):
    out = {'Schema': 'rackcad-i64-relay-schema-precheck/v1', 'ExpectedSha256': expected, 'InputSha256': None, 'OfficialSchemaBlob': RELAY_RECORD_SCHEMA_BLOB, 'Result': None, 'Diagnostics': []}
    raw = read_bytes(relay_path)
    out['InputSha256'] = sha256(raw)
    if out['InputSha256'] != expected:
        out['Result'] = 'INPUT_SHA256_MISMATCH'
        return out, 2
    try:
        problems = relay_schema_problems(load_handoff(raw))
    except Invalid as e:
        problems = [str(e)]
    out['Result'] = 'SCHEMA_INVALID' if problems else 'SCHEMA_VALID'
    out['Diagnostics'] = problems
    return out, 1 if problems else 0


def main(argv=None):
    p = Parser(add_help=False, allow_abbrev=False)
    p.add_argument('--relay-record', required=True)
    p.add_argument('--expected-sha256', required=True)
    try:
        a = p.parse_args(argv)
        if not re.fullmatch('[0-9a-f]{64}', a.expected_sha256):
            raise UsageError('--expected-sha256 must be 64 lowercase hex characters')
        out, code = precheck(a.relay_record, a.expected_sha256)
    except (UsageError, OSError) as e:
        out, code = {'Schema': 'rackcad-i64-relay-schema-precheck/v1', 'Result': 'USAGE_ERROR', 'Diagnostics': [type(e).__name__]}, 2
    sys.stdout.buffer.write((json.dumps(out, indent=2, ensure_ascii=True) + chr(10)).encode('ascii'))
    sys.stdout.buffer.flush()
    return code


if __name__ == '__main__':
    sys.exit(main())
