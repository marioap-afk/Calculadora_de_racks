#!/usr/bin/env python3
"""I64-IDENTITY-BRIDGE-01: deterministic substitute evidence for nc1 of F2-T2A-NAVIGATION-HOST (I-64 Amendment A-13, which corrects A-12 and supersedes it for activation).

Applies only to the exact defect signature of the Controller runs 3d86, 16b6 and bfc8: the Controller evaluated repository
consistency inside Handoff, so the nc1 relative oracle could not be met. This tool answers two separate questions from source
data only (no Controller prose), and fails closed on anything else:

  A. HANDOFF INTEGRITY: the handoff is a valid artifact. Both handoffs and the relay record are schema-valid (the relay against
     the official rackcad-relay-record/v1 schema, pinned by exact Git blob); TaskId, Attempt, DelegationRunId,
     WorkRunId, Branch, Worktree, BaseSha and RedSha are the bound ones; the relay OutputSha256 matches the mutated handoff
     bytes; the output time falls inside the cession; and the mutation is exact: only the top-level CurrentSha changed, proven
     structurally (every key, nested, in order) and byte for byte. Repository state is deliberately not consulted here, so a
     false CurrentSha cannot make this question fail.
  B. IDENTITY: the delivery's CurrentSha against authoritative repository state, after proving that state coherent: bound
     branch, HEAD = remote branch = tracking ref = the verified SHA exactly (no ancestor or descendant is accepted), and BaseSha
     and RedSha on the chain.

ControlResult = PASS only with HandoffIntegrity = PASS and Identity = FAIL for exactly the bound synthetic, nonexistent SHA.
Nonexistence is proven only by a successful git cat-file --batch-check whose whole answer is exactly "<sha> missing";
any other outcome of that query fails closed.
The handoff parsing, rackcad-worker-handoff/v1 schema validation and exact comparison are copied verbatim from
verify_testfilter_bridge.py (I-64 A-11, blob f9780f2f30c067243c93b6ab7175cc4b3179ca39), as is its hardened Git execution.
This tool executes neither A-11 nor any other bridge. Python standard library only. Exit 0 only for ControlResult PASS;
1 for FAIL; 2 for usage errors. Deterministic LF-only JSON on stdout.
"""
import argparse
import base64
import datetime
import decimal
import hashlib
import json
import os
import re
import shutil
import subprocess
import sys
import tempfile

OUT_SCHEMA = 'rackcad-i64-identity-bridge/v1'
HANDOFF_SCHEMA = 'rackcad-worker-handoff/v1'
RELAY_SCHEMA = 'rackcad-relay-record/v1'
SHA_RE = re.compile(r'[0-9a-f]{40}')
HEX64_RE = re.compile(r'[0-9A-Fa-f]{64}')
FIELD_RE = re.compile(rb'"CurrentSha"\s*:\s*"([0-9a-f]{40})"')
GIT_TIMEOUT_SECONDS = 120
EXIT_PASS, EXIT_FAIL, EXIT_USAGE = 0, 1, 2

# ---- copied verbatim from verify_testfilter_bridge.py (A-11, blob f9780f2f) ----
HANDOFF_KEYS = ('Schema', 'TaskId', 'RunId', 'DelegationRunId', 'Initiative', 'Gate', 'Attempt', 'BaseSha', 'RedSha',
                'CurrentSha', 'Branch', 'Worktree', 'Pushed', 'FilesChanged', 'WorkCompleted', 'TestsExecuted',
                'TestResults', 'Evidence', 'UnexpectedFindings', 'KnownLimitations', 'Deviations', 'OpenQuestions',
                'WorkerStatus', 'Disposition', 'TriggeredStopConditions', 'RecommendedNextAction', 'Worker')
BOM = chr(0xFEFF)
# rackcad-worker-handoff/v1, embedded verbatim from docs/automation/agent-execution/schemas/worker-handoff.schema.json at
# main bb0d5522e8411f66a51fdfb3f1f0d0514b737453 (blob cc3c9cf76b2247f3312e99bd447554326cc3700a).
SCHEMA_V1 = {'$schema': 'https://json-schema.org/draft/2020-12/schema', 'title': 'rackcad-worker-handoff/v1', 'description': 'Entrega de worker (Freeze de I-61, Proposal V9 §8.2). No concede GATE PASS. Subordinado: no crea requisitos de evidencia, estados, gates, reglas Git ni decisiones del Owner.', 'type': 'object', 'additionalProperties': False, 'properties': {'Schema': {'type': 'string', 'enum': ['rackcad-worker-handoff/v1']}, 'TaskId': {'type': 'string'}, 'RunId': {'type': 'string', 'pattern': '^R[0-9]{8}T[0-9]{6}Z-[0-9a-f]{4}$'}, 'DelegationRunId': {'type': 'string', 'pattern': '^R[0-9]{8}T[0-9]{6}Z-[0-9a-f]{4}$'}, 'Initiative': {'type': 'string'}, 'Gate': {'type': 'string'}, 'Attempt': {'type': 'integer'}, 'BaseSha': {'type': 'string', 'pattern': '^[0-9a-f]{40}$'}, 'RedSha': {'type': ['string', 'null'], 'pattern': '^[0-9a-f]{40}$'}, 'CurrentSha': {'type': 'string', 'pattern': '^[0-9a-f]{40}$'}, 'Branch': {'type': 'string'}, 'Worktree': {'type': 'string'}, 'Pushed': {'type': 'boolean'}, 'FilesChanged': {'type': 'array', 'items': {'type': 'string'}}, 'WorkCompleted': {'type': 'string'}, 'TestsExecuted': {'type': 'array', 'items': {'type': 'object', 'additionalProperties': False, 'properties': {'RunRef': {'type': 'string'}, 'Command': {'type': 'string'}, 'Phase': {'type': 'string', 'enum': ['RED', 'GREEN', 'RELEVANT', 'MUTATION']}, 'TreeSha': {'type': ['string', 'null'], 'pattern': '^[0-9a-f]{40}$'}, 'ResultsFile': {'type': ['string', 'null']}}, 'required': ['RunRef', 'Command', 'Phase', 'TreeSha', 'ResultsFile']}}, 'TestResults': {'type': 'array', 'items': {'type': 'object', 'additionalProperties': False, 'properties': {'RunRef': {'type': 'string'}, 'Selected': {'type': 'integer'}, 'Passed': {'type': 'integer'}, 'Failed': {'type': 'integer'}, 'Skipped': {'type': 'integer'}, 'FailedTests': {'type': 'array', 'items': {'type': 'string'}}}, 'required': ['RunRef', 'Selected', 'Passed', 'Failed', 'Skipped', 'FailedTests']}}, 'Evidence': {'type': 'array', 'items': {'type': 'string'}}, 'UnexpectedFindings': {'type': 'array', 'items': {'type': 'string'}}, 'KnownLimitations': {'type': 'array', 'items': {'type': 'string'}}, 'Deviations': {'type': 'array', 'items': {'type': 'string'}}, 'OpenQuestions': {'type': 'array', 'items': {'type': 'string'}}, 'WorkerStatus': {'type': 'string', 'enum': ['IMPLEMENTATION_COMPLETE', 'PARTIAL', 'BLOCKED']}, 'Disposition': {'type': 'string', 'enum': ['NONE', 'BLOCKED', 'STOP']}, 'TriggeredStopConditions': {'type': 'array', 'items': {'type': 'string'}}, 'RecommendedNextAction': {'type': 'string'}, 'Worker': {'type': 'object', 'additionalProperties': False, 'properties': {'Provider': {'type': 'string'}, 'ModelRequested': {'type': 'string'}, 'EffortRequested': {'type': 'string'}, 'Trailer': {'type': 'string'}}, 'required': ['Provider', 'ModelRequested', 'EffortRequested', 'Trailer']}}, 'required': ['Schema', 'TaskId', 'RunId', 'DelegationRunId', 'Initiative', 'Gate', 'Attempt', 'BaseSha', 'RedSha', 'CurrentSha', 'Branch', 'Worktree', 'Pushed', 'FilesChanged', 'WorkCompleted', 'TestsExecuted', 'TestResults', 'Evidence', 'UnexpectedFindings', 'KnownLimitations', 'Deviations', 'OpenQuestions', 'WorkerStatus', 'Disposition', 'TriggeredStopConditions', 'RecommendedNextAction', 'Worker']}


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


def schema_problems(obj):
    problems = []
    if set(obj.keys()) != set(HANDOFF_KEYS):
        missing = sorted(set(HANDOFF_KEYS) - set(obj.keys()))
        extra = sorted(set(obj.keys()) - set(HANDOFF_KEYS))
        problems.append('key set differs from %s (missing %s, extra %s)' % (HANDOFF_SCHEMA, missing, extra))
    if obj.get('Schema') != HANDOFF_SCHEMA:
        problems.append('Schema is not %s' % HANDOFF_SCHEMA)
    return problems

class UsageError(Exception):
    pass


class Parser(argparse.ArgumentParser):
    def error(self, message):
        raise UsageError(message)

NO_GRAFTS_DIR = None


def git_env():
    # Same hardening as verify_scope_bridge.py (A-4): no inherited GIT_* variable; grafts disabled.
    env = {k: v for k, v in os.environ.items() if not k.upper().startswith('GIT_')}
    env['GIT_TERMINAL_PROMPT'] = '0'
    env['GIT_GRAFT_FILE'] = os.path.join(NO_GRAFTS_DIR, 'no-grafts')
    return env


def git(repo, *args):
    cmd = ['git', '--no-replace-objects', '-c', 'core.fsmonitor=false', '-C', repo] + list(args)
    proc = subprocess.run(cmd, capture_output=True, env=git_env(), timeout=GIT_TIMEOUT_SECONDS, shell=False)
    return proc.returncode, proc.stdout, proc.stderr
# ---- end of the verbatim copy ----


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
# ---- end of the A-13 relay schema pin ----


# ---- A-13: object existence through an explicit query ----
OBJECT_PRESENT_RE = re.compile(rb'([0-9a-f]{40}) (?:commit|tree|blob|tag) [0-9]+\n')


def classify_object_query(sha, rc, stdout, stderr):
    """Exactly one of MISSING, EXISTS or ERROR. Only a successful command with a complete, exact answer is an answer."""
    if rc != 0:
        return 'ERROR', 'git cat-file --batch-check exited with %d' % rc
    if stderr:
        return 'ERROR', 'git cat-file --batch-check wrote to stderr'
    if stdout == ('%s missing\n' % sha).encode('ascii'):
        return 'MISSING', ''
    present = OBJECT_PRESENT_RE.fullmatch(stdout)
    if present and present.group(1).decode('ascii') == sha:
        return 'EXISTS', ''
    return 'ERROR', 'malformed, incomplete or unexpected object query response'


def object_query(repo, sha):
    cmd = ['git', '--no-replace-objects', '-c', 'core.fsmonitor=false', '-C', repo, 'cat-file', '--batch-check']
    try:
        proc = subprocess.run(cmd, input=(sha + '\n').encode('ascii'), capture_output=True, env=git_env(), timeout=GIT_TIMEOUT_SECONDS, shell=False)
    except (OSError, subprocess.SubprocessError) as e:
        return 'ERROR', 'the object query could not be executed: %s' % type(e).__name__
    return classify_object_query(sha, proc.returncode, proc.stdout, proc.stderr)
# ---- end of the A-13 object query ----


def parse_time(value, name):
    if type(value) is not str:
        raise Invalid('%s is not a string' % name)
    text = value[:-1] + '+00:00' if value.endswith('Z') else value
    try:
        moment = datetime.datetime.fromisoformat(text)
    except ValueError:
        raise Invalid('%s is not an ISO-8601 instant' % name)
    if moment.tzinfo is None:
        raise Invalid('%s has no UTC offset' % name)
    return moment


def same_path(x, y):
    return os.path.normcase(os.path.realpath(x)) == os.path.normcase(os.path.realpath(y))


def rev(repo, spec):
    rc, gout, _ = git(repo, 'rev-parse', '--verify', '--quiet', '--end-of-options', spec)
    text = gout.decode('ascii', 'replace').strip()
    return text if rc == 0 and SHA_RE.fullmatch(text) else None


def commit_exists(repo, sha):
    return rev(repo, sha + '^{commit}') == sha


def is_ancestor(repo, x, y):
    rc, _, _ = git(repo, 'merge-base', '--is-ancestor', x, y)
    return rc == 0


def verify(a, out, check):
    for name in ('base_sha', 'red_sha', 'current_sha', 'synthetic_sha'):
        if not check('Args.%s' % name, bool(SHA_RE.fullmatch(getattr(a, name) or '')), 'INVALID_SHA_ARGUMENT', '%s must be 40 lowercase hex characters' % name):
            return
    if not re.fullmatch(r'[0-9]+', a.attempt or ''):
        raise UsageError('--attempt must be a non-negative integer')
    out['Binding'] = {'TaskId': a.task_id, 'Attempt': a.attempt, 'DelegationRunId': a.delegation_run_id, 'WorkRunId': a.work_run_id, 'Branch': a.branch,
                      'BaseSha': a.base_sha, 'RedSha': a.red_sha, 'VerifiedSha': a.current_sha, 'SyntheticSha': a.synthetic_sha}

    # Inputs: strict parsing and schema of both handoffs. Failure here fails closed before any question is evaluated.
    raws, loaded = {}, {}
    for role, path, code in (('Real', a.real_handoff, 'REAL_UNREADABLE'), ('Mutated', a.mutated_handoff, 'MUTATED_UNREADABLE'), ('Relay', a.relay_record, 'RELAY_UNREADABLE')):
        try:
            raws[role] = read_bytes(path)
            out['Inputs'][role + 'Sha256'] = sha256(raws[role])
            loaded[role] = load_handoff(raws[role])
            check('Input.%sReadable' % role, True)
        except (OSError, Invalid) as e:
            check('Input.%sReadable' % role, False, code, str(e))
    if len(loaded) != 3:
        return
    real, mut, relay = loaded['Real'], loaded['Mutated'], loaded['Relay']
    for role, obj, code in (('Real', real, 'REAL_SCHEMA_INVALID'), ('Mutated', mut, 'MUTATED_SCHEMA_INVALID')):
        probs = schema_problems(obj)
        if not probs:
            try:
                validate(obj, SCHEMA_V1, '$', probs)
            except Invalid as e:
                probs.append(str(e))
        check('Input.%sHandoffSchema' % role, not probs, code, '; '.join(probs[:8]))
    relay_problems = relay_schema_problems(relay)
    check('Input.RelayRecordSchema', not relay_problems, 'RELAY_SCHEMA_INVALID', '; '.join(relay_problems[:8]))
    if out['Reasons']:
        return

    # A. HANDOFF INTEGRITY: the record as an artifact. Repository state is deliberately not consulted here.
    def integrity(cid, ok, code, detail=''):
        return check('HandoffIntegrity.' + cid, bool(ok), code, detail)

    for role, obj in (('Real', real), ('Mutated', mut)):
        integrity(role + '.TaskId', obj['TaskId'] == a.task_id, 'HANDOFF_TASK_ID_MISMATCH', role)
        integrity(role + '.Attempt', obj['Attempt'].lexeme == a.attempt, 'HANDOFF_ATTEMPT_MISMATCH', role)
        integrity(role + '.DelegationRunId', obj['DelegationRunId'] == a.delegation_run_id, 'HANDOFF_DELEGATION_RUN_ID_MISMATCH', role)
        integrity(role + '.WorkRunId', obj['RunId'] == a.work_run_id, 'HANDOFF_WORK_RUN_ID_MISMATCH', role)
        integrity(role + '.Branch', obj['Branch'] == a.branch, 'HANDOFF_BRANCH_MISMATCH', role)
        integrity(role + '.Worktree', same_path(obj['Worktree'], a.repo), 'HANDOFF_WORKTREE_MISMATCH', role)
        integrity(role + '.BaseSha', obj['BaseSha'] == a.base_sha, 'HANDOFF_BASE_SHA_MISMATCH', role)
        integrity(role + '.RedSha', obj['RedSha'] == a.red_sha, 'HANDOFF_RED_SHA_MISMATCH', role)
    integrity('Real.CurrentSha', real['CurrentSha'] == a.current_sha, 'REAL_CURRENT_SHA_MISMATCH')

    # Exactness of the mutation: structural (every key, nested, in order) and byte for byte.
    changed = [k for k in HANDOFF_KEYS if not exact_equal(real[k], mut[k])]
    out['Mutation']['ChangedFields'] = changed
    integrity('Mutation.KeyOrder', list(real.keys()) == list(mut.keys()), 'MUTATION_KEY_ORDER_CHANGED')
    integrity('Mutation.OnlyCurrentSha', [k for k in changed if k != 'CurrentSha'] == [], 'MUTATION_NOT_ONLY_CURRENT_SHA', 'changed fields: %s' % changed)
    real_fields, mut_fields = list(FIELD_RE.finditer(raws['Real'])), list(FIELD_RE.finditer(raws['Mutated']))
    exact_bytes = False
    if len(real_fields) == 1 and len(mut_fields) == 1:
        m = mut_fields[0]
        exact_bytes = raws['Mutated'][:m.start(1)] + real_fields[0].group(1) + raws['Mutated'][m.end(1):] == raws['Real']
    integrity('Mutation.BytesExact', exact_bytes, 'MUTATION_BYTES_NOT_EXACT', 'the mutated bytes must equal the real bytes except the value of the top-level CurrentSha')

    # The relay record of the delivery being judged.
    integrity('Relay.Schema', relay.get('Schema') == RELAY_SCHEMA, 'RELAY_BINDING_MISMATCH', 'Schema')
    integrity('Relay.TaskId', relay.get('TaskId') == a.task_id, 'RELAY_BINDING_MISMATCH', 'TaskId')
    integrity('Relay.RunId', relay.get('RunId') == a.work_run_id, 'RELAY_BINDING_MISMATCH', 'RunId')
    attempt = relay.get('Attempt')
    integrity('Relay.Attempt', type(attempt) is JsonInt and attempt.lexeme == a.attempt, 'RELAY_BINDING_MISMATCH', 'Attempt')
    integrity('Relay.Phase', relay.get('Phase') == 'WORK', 'RELAY_BINDING_MISMATCH', 'Phase')
    outcome = relay.get('Outcome') if isinstance(relay.get('Outcome'), dict) else {}
    cession = relay.get('Cession') if isinstance(relay.get('Cession'), dict) else {}
    integrity('Relay.Completed', outcome.get('Kind') == 'COMPLETED', 'RELAY_NOT_COMPLETED', 'Outcome.Kind')
    declared = outcome.get('OutputSha256')
    integrity('Relay.OutputSha256', type(declared) is str and HEX64_RE.fullmatch(declared) and declared.lower() == sha256(raws['Mutated']), 'RELAY_OUTPUT_HASH_MISMATCH', 'Outcome.OutputSha256 against the mutated handoff bytes')
    try:
        start, written, end = parse_time(cession.get('StartUtc'), 'Cession.StartUtc'), parse_time(outcome.get('OutputWrittenUtc'), 'Outcome.OutputWrittenUtc'), parse_time(cession.get('EndUtc'), 'Cession.EndUtc')
        integrity('Relay.OutputTime', start <= written <= end, 'RELAY_OUTPUT_TIME_INVALID', 'the output must be written inside the cession')
    except Invalid as e:
        integrity('Relay.OutputTime', False, 'RELAY_OUTPUT_TIME_INVALID', str(e))
    if out['Reasons']:
        out['HandoffIntegrity'] = 'FAIL'
        return
    out['HandoffIntegrity'] = 'PASS'

    # Authoritative repository state. Incoherent state fails closed: Identity is not evaluated.
    rc, gout, _ = git(a.repo, 'rev-parse', '--show-toplevel')
    same_root = rc == 0 and bool(gout.strip()) and same_path(gout.decode('utf-8', errors='strict').strip(), a.repo)
    if not check('Repository.TopLevel', same_root, 'GIT_REPOSITORY_INVALID', '--repo is not the top level of a readable Git working tree'):
        return
    rc, gout, _ = git(a.repo, 'symbolic-ref', '--quiet', '--short', 'HEAD')
    if not check('Repository.Branch', rc == 0 and gout.decode('utf-8', errors='strict').strip() == a.branch, 'BRANCH_MISMATCH', 'HEAD is not on the bound branch'):
        return
    for name, sha in (('BaseSha', a.base_sha), ('RedSha', a.red_sha), ('VerifiedSha', a.current_sha)):
        if not check('Repository.%s.Commit' % name, commit_exists(a.repo, sha), 'GIT_NOT_A_COMMIT', name):
            return
    head = rev(a.repo, 'HEAD^{commit}')
    rc, gout, _ = git(a.repo, 'ls-remote', '--refs', 'origin', 'refs/heads/' + a.branch)
    fields = gout.decode('ascii', 'replace').split()
    remote = fields[0] if rc == 0 and len(fields) == 2 and SHA_RE.fullmatch(fields[0]) else None
    tracking = rev(a.repo, 'refs/remotes/origin/%s^{commit}' % a.branch)
    out['Repository'] = {'Head': head, 'Remote': remote, 'Tracking': tracking}
    if not check('Repository.RemoteReadable', remote is not None and head is not None, 'REMOTE_UNAVAILABLE', 'HEAD or the remote branch could not be read'):
        return
    if not check('Repository.HeadEqualsRemote', head == remote and head == tracking, 'HEAD_REMOTE_DISAGREE', 'HEAD, the remote branch and the tracking ref must be equal'):
        return
    if not check('Repository.BaseShaAncestry', a.base_sha != a.red_sha and is_ancestor(a.repo, a.base_sha, a.red_sha), 'BASE_NOT_ANCESTOR', 'BaseSha must be a strict ancestor of RedSha'):
        return
    if not check('Repository.RedShaChain', a.red_sha != a.current_sha and is_ancestor(a.repo, a.red_sha, a.current_sha), 'RED_NOT_ON_CHAIN', 'RedSha must be a strict ancestor of the verified SHA'):
        return
    if not check('Repository.HeadIsVerifiedSha', head == a.current_sha, 'HEAD_NOT_VERIFIED_SHA', 'HEAD must equal the verified SHA; no descendant is accepted'):
        return

    # B. IDENTITY: the delivery's CurrentSha against authoritative repository state.
    candidate = mut['CurrentSha']
    out['Identity'] = 'PASS' if candidate == head else 'FAIL'
    out['Mutation']['CandidateCurrentSha'] = candidate
    out['Mutation']['IdentityTarget'] = head

    # Control conclusion: Handoff integrity holds and Identity rejects exactly the authorized synthetic SHA.
    if not check('Control.IdentityRejectsCandidate', out['Identity'] == 'FAIL', 'MUTATION_NOT_REJECTED_BY_IDENTITY', 'the candidate CurrentSha equals the repository identity'):
        return
    check('Control.CandidateIsAuthorizedSynthetic', candidate == a.synthetic_sha, 'MUTATED_SHA_NOT_AUTHORIZED', 'the candidate is not the bound synthetic SHA')
    state, detail = object_query(a.repo, candidate)
    out['Mutation']['CandidateObject'] = state
    if not check('Control.CandidateObjectQuery', state != 'ERROR', 'GIT_OBJECT_QUERY_FAILED', detail):
        return
    check('Control.CandidateNonexistent', state == 'MISSING', 'MUTATED_SHA_EXISTS', 'the candidate names an existing object')


def main(argv=None):
    global NO_GRAFTS_DIR
    checks, reasons = [], []
    out = {'Schema': OUT_SCHEMA, 'Result': 'FAIL', 'ControlResult': 'FAIL', 'HandoffIntegrity': 'NOT_EVALUATED', 'Identity': 'NOT_EVALUATED', 'FailClosed': True,
           'Reasons': reasons, 'Binding': None, 'Inputs': {}, 'Repository': None, 'Mutation': {'ChangedFields': None, 'CandidateCurrentSha': None, 'IdentityTarget': None, 'CandidateObject': None}, 'Checks': checks}
    exit_code = EXIT_FAIL

    def check(cid, ok, code=None, detail=None):
        checks.append({'Id': cid, 'Result': 'pass' if ok else 'fail'})
        if not ok:
            reasons.append({'Code': code or cid, 'Check': cid, 'Detail': detail or ''})
        return ok

    NO_GRAFTS_DIR = tempfile.mkdtemp(prefix='i64id-nografts-')
    try:
        p = Parser(add_help=False, allow_abbrev=False)
        for opt in ('--repo', '--task-id', '--attempt', '--delegation-run-id', '--work-run-id', '--branch', '--base-sha', '--red-sha', '--current-sha', '--synthetic-sha',
                    '--real-handoff', '--mutated-handoff', '--relay-record'):
            p.add_argument(opt, required=True)
        a = p.parse_args(argv)
        verify(a, out, check)
    except UsageError as e:
        check('Usage', False, 'USAGE_ERROR', str(e))
        exit_code = EXIT_USAGE
    except subprocess.TimeoutExpired:
        check('Git.Timeout', False, 'GIT_TIMEOUT', 'git did not finish in time')
    except FileNotFoundError:
        check('Git.Available', False, 'GIT_NOT_AVAILABLE', 'git executable not found')
    except Exception as e:  # fail closed on anything unexpected
        check('Unexpected', False, 'UNEXPECTED_ERROR', type(e).__name__)
    finally:
        shutil.rmtree(NO_GRAFTS_DIR, ignore_errors=True)
    passed = bool(checks) and not reasons and all(c['Result'] == 'pass' for c in checks) and out['HandoffIntegrity'] == 'PASS' and out['Identity'] == 'FAIL'
    out['Result'] = out['ControlResult'] = 'PASS' if passed else 'FAIL'
    out['FailClosed'] = not passed and (out['HandoffIntegrity'] != 'PASS' or out['Identity'] == 'NOT_EVALUATED' or any(r['Code'] == 'GIT_OBJECT_QUERY_FAILED' for r in reasons))
    sys.stdout.buffer.write((json.dumps(out, indent=2, ensure_ascii=True) + '\n').encode('ascii'))
    sys.stdout.buffer.flush()
    if passed:
        return EXIT_PASS
    return exit_code if exit_code != EXIT_PASS else EXIT_FAIL


if __name__ == '__main__':
    sys.exit(main())
