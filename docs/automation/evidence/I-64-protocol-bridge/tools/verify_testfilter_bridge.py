#!/usr/bin/env python3
"""I64-TESTFILTER-BRIDGE-01: deterministic substitute evidence for the Tests part of nc3 (I-64 Amendment A-8).

For one I-64 occurrence, proves fail-closed, from source data only (no Controller prose):
  1. RequiredTests selection with VSTest semantics for FullyQualifiedName~<substring>: case-insensitive (ordinal,
     per-character simple upper-case fold) substring over TestMethod className + "." + name, counted over the top-level
     UnitTestResult rows of the GREEN (CurrentSha) and RED (RedSha) Core TRX files;
  2. GREEN: selection >= MinSelected and no failed selected test; RED: selection >= MinSelected, selected failures
     present, every failure inside the selection, and the failure composition recorded and compared to the expected one;
  3. counts equal to the delivery's TestResults and to the expected values bound to the occurrence;
  4. ChainRedFiles = git diff --name-only ChainBaseSha..RedSha -- tests/ equals the accredited list, and
     RedSha..CurrentSha does not modify any of them;
  5. the negative-control handoff mutation is exact: both handoffs schema-valid, only WorkCompleted changed, by the
     authorized append, introducing exactly one prohibited AUTOMATION_PLAN 16.10 term and no compensating change.

The handoff parsing, rackcad-worker-handoff/v1 schema validation, exact comparison and 16.10 term-set parsing are copied
verbatim from verify_freetext_bridge.py (I-64 A-6, blob 3bc7869e9f4c9c5eed0d64d7b8fee6f7002b9169); this tool does not
execute A-6. Display-name, class-only, case-sensitive and method-only counts are reported only as diagnostics.
Python standard library only. Exit 0 only for PASS; 1 for FAIL; 2 for usage errors. Deterministic LF-only JSON on stdout.
"""
import argparse
import decimal
import hashlib
import json
import os
import re
import shutil
import subprocess
import sys
import tempfile
import xml.etree.ElementTree as ET

OUT_SCHEMA = 'rackcad-i64-testfilter-bridge/v1'
HANDOFF_SCHEMA = 'rackcad-worker-handoff/v1'
TRX_NS = 'http://microsoft.com/schemas/VisualStudio/TeamTest/2010'
AUTHORIZED_APPEND = ' GATE PASS.'
SHA_RE = re.compile(r'[0-9a-f]{40}')
FILTER_RE = re.compile(r'FullyQualifiedName~([A-Za-z0-9_.]+)')
COUNTS_RE = re.compile(r'([0-9]+)/([0-9]+)/([0-9]+)')
COMPOSITION_RE = re.compile(r'[A-Za-z][A-Za-z0-9_.-]*=[0-9]+(,[A-Za-z][A-Za-z0-9_.-]*=[0-9]+)*')
ASSERT_RE = re.compile(r'Assert\.[A-Za-z]+\(\)')
EXCEPTION_RE = re.compile(r'([A-Za-z_][A-Za-z0-9_]*(?:\.[A-Za-z_][A-Za-z0-9_]*)*Exception) : ')
OUTCOMES = ('Passed', 'Failed', 'NotExecuted')
GIT_TIMEOUT_SECONDS = 120
EXIT_PASS, EXIT_FAIL, EXIT_USAGE = 0, 1, 2

# ---- copied verbatim from verify_freetext_bridge.py (A-6, blob 3bc7869e) ----
HANDOFF_KEYS = ('Schema', 'TaskId', 'RunId', 'DelegationRunId', 'Initiative', 'Gate', 'Attempt', 'BaseSha', 'RedSha',
                'CurrentSha', 'Branch', 'Worktree', 'Pushed', 'FilesChanged', 'WorkCompleted', 'TestsExecuted',
                'TestResults', 'Evidence', 'UnexpectedFindings', 'KnownLimitations', 'Deviations', 'OpenQuestions',
                'WorkerStatus', 'Disposition', 'TriggeredStopConditions', 'RecommendedNextAction', 'Worker')
IDENTITY_FIELDS = ('Schema', 'TaskId', 'RunId', 'DelegationRunId', 'Initiative', 'Gate', 'Attempt', 'BaseSha', 'RedSha',
                   'CurrentSha', 'Branch', 'Worktree', 'FilesChanged')
GOVERNED_TYPES = {'WorkCompleted': 'string', 'RecommendedNextAction': 'string', 'Evidence': 'list', 'UnexpectedFindings': 'list',
                  'KnownLimitations': 'list', 'Deviations': 'list', 'OpenQuestions': 'list'}
SECTION_PREFIX = '### 16.10 '
SEMANTICS = 'Coincidencia por palabra o frase completa, sin distinguir mayúsculas, en '
EXCLUSION = ' No se aplica al nombre de propiedad `Gate` ni a su valor.'
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


def parse_term_set(raw):
    try:
        text = raw.decode('utf-8', errors='strict')
    except UnicodeDecodeError as e:
        raise Invalid('term set not strict UTF-8: %s' % e.reason)
    if text.startswith(BOM) or '\r' in text:
        raise Invalid('term set has a byte order mark or carriage returns')
    lines = text.split('\n')
    heads = [i for i, line in enumerate(lines) if line.startswith(SECTION_PREFIX)]
    if len(heads) != 1:
        raise Invalid('expected exactly one 16.10 heading, found %d' % len(heads))
    body = []
    for line in lines[heads[0] + 1:]:
        if line.startswith('#'):
            break
        body.append(line)
    section = '\n'.join([lines[heads[0]]] + body)
    while body and body[0].strip() == '':
        body.pop(0)
    para = []
    for line in body:
        if line.strip() == '':
            break
        para.append(line.strip())
    paragraph = ' '.join(para)
    if not paragraph.startswith(SEMANTICS):
        raise Invalid('16.10 paragraph does not start with the matching-semantics sentence')
    if paragraph.count(EXCLUSION) != 1:
        raise Invalid('16.10 paragraph lacks exactly one Gate-property exclusion sentence')
    rest = paragraph[len(SEMANTICS):]
    colon = rest.find(':')
    if colon < 0:
        raise Invalid('governed field list is not terminated by a colon')
    fields_part, terms_part = rest[:colon], rest[colon + 1:]
    end = terms_part.find(EXCLUSION)
    if end < 0:
        raise Invalid('term list is not followed by the exclusion sentence')
    terms_part = terms_part[:end]
    if not terms_part.endswith('.'):
        raise Invalid('term list does not end with a period')
    terms_part = terms_part[:-1]

    def tokens(part, what):
        items = re.findall(r'`([^`]+)`', part)
        residue = re.sub(r'`[^`]+`', '@', part).strip()
        expected = ', '.join(['@'] * (len(items) - 1)) + (' y @' if len(items) > 1 else '@')
        if not items or residue != expected:
            raise Invalid('%s list is not a well-formed backtick enumeration' % what)
        if len(set(i.casefold() for i in items)) != len(items):
            raise Invalid('%s list has duplicates' % what)
        if any(i != i.strip() or i == '' for i in items):
            raise Invalid('%s list has blank or padded items' % what)
        return items

    fields = tokens(fields_part, 'field')
    terms = tokens(terms_part, 'term')
    unknown = [f for f in fields if f not in GOVERNED_TYPES]
    if unknown:
        raise Invalid('governed fields not admissible in %s: %s' % (HANDOFF_SCHEMA, unknown))
    return fields, terms, section


def compile_terms(terms):
    return [(t, re.compile(r'(?<!\w)' + re.escape(t) + r'(?!\w)', re.IGNORECASE)) for t in terms]


def governed_strings(obj, field):
    value = obj[field]
    if GOVERNED_TYPES[field] == 'string' and isinstance(value, str):
        return [(None, value)]
    if GOVERNED_TYPES[field] == 'list' and isinstance(value, list) and all(isinstance(v, str) for v in value):
        return list(enumerate(value))
    raise Invalid('governed field %s does not have its %s type (%s)' % (field, HANDOFF_SCHEMA, GOVERNED_TYPES[field]))


def hits(obj, fields, patterns):
    out = []
    for field in fields:
        for index, text in governed_strings(obj, field):
            for term, rx in patterns:
                n = len(rx.findall(text))
                if n:
                    out.append({'Field': field, 'Index': index, 'Term': term, 'Count': n})
    return out
# ---- end of the verbatim copy ----


class UsageError(Exception):
    pass


class Parser(argparse.ArgumentParser):
    def error(self, message):
        raise UsageError(message)


def fold(text):
    """Ordinal ignore-case fold: per-character simple upper-case mapping (characters without a single-character
    upper-case form are kept), the comparison used by .NET OrdinalIgnoreCase for the VSTest '~' (Contains) operator."""
    out = []
    for ch in text:
        up = ch.upper()
        out.append(up if len(up) == 1 else ch)
    return ''.join(out)


def fqn_selected(class_name, method_name, substring):
    """VSTest FullyQualifiedName~substring for a TRX TestMethod: ordinal case-insensitive containment over
    className + '.' + name. The only selection rule that decides the result."""
    return fold(substring) in fold(class_name + '.' + method_name)


def classify_failure(message):
    if ASSERT_RE.match(message):
        return 'xunit-assertion'
    m = EXCEPTION_RE.match(message)
    if m:
        return 'xunit-assertion' if m.group(1).startswith('Xunit.Sdk.') else m.group(1)
    return 'unclassified'


def q(tag):
    return '{%s}%s' % (TRX_NS, tag)


def load_trx(raw):
    """Strict VSTest TRX reader. Returns (definitions{id: (className, name, displayName)}, rows[(testId, outcome, testName,
    message)], counters{...}). Raises Invalid on anything ambiguous."""
    if raw.startswith(b'\xef\xbb\xbf'):
        raw = raw[3:]
    try:
        raw.decode('utf-8', errors='strict')
    except UnicodeDecodeError as e:
        raise Invalid('TRX is not strict UTF-8: %s' % e.reason)
    if b'<!DOCTYPE' in raw or b'<!ENTITY' in raw:
        raise Invalid('TRX contains a DOCTYPE or ENTITY declaration')
    try:
        root = ET.fromstring(raw)
    except ET.ParseError as e:
        raise Invalid('malformed TRX XML: %s' % e)
    if root.tag != q('TestRun'):
        raise Invalid('root element is not a VSTest 2010 TestRun')
    defs_nodes = root.findall(q('TestDefinitions'))
    results_nodes = root.findall(q('Results'))
    summary_nodes = root.findall(q('ResultSummary'))
    if len(defs_nodes) != 1 or len(results_nodes) != 1 or len(summary_nodes) != 1:
        raise Invalid('TRX must have exactly one TestDefinitions, one Results and one ResultSummary')
    definitions = {}
    for ut in defs_nodes[0]:
        if ut.tag != q('UnitTest'):
            raise Invalid('unexpected element in TestDefinitions: %s' % ut.tag)
        tid = ut.get('id')
        methods = ut.findall(q('TestMethod'))
        if not tid or len(methods) != 1:
            raise Invalid('UnitTest without id or without exactly one TestMethod')
        cls, name = methods[0].get('className'), methods[0].get('name')
        if not cls or not name:
            raise Invalid('TestMethod without className or name')
        if tid in definitions:
            raise Invalid('duplicate UnitTest id %s' % tid)
        definitions[tid] = (cls, name, ut.get('name') or '')
    rows = []
    for r in results_nodes[0]:
        if r.tag != q('UnitTestResult'):
            raise Invalid('unexpected element in Results: %s' % r.tag)
        if r.find(q('InnerResults')) is not None:
            raise Invalid('InnerResults are not supported')
        tid, outcome = r.get('testId'), r.get('outcome')
        if tid not in definitions:
            raise Invalid('UnitTestResult references an undefined testId')
        if outcome not in OUTCOMES:
            raise Invalid('unsupported outcome %r' % outcome)
        msg = ''
        node = r.find('%s/%s/%s' % (q('Output'), q('ErrorInfo'), q('Message')))
        if node is not None and node.text:
            msg = node.text
        rows.append((tid, outcome, r.get('testName') or '', msg))
    counters_nodes = summary_nodes[0].findall(q('Counters'))
    if len(counters_nodes) != 1:
        raise Invalid('ResultSummary must have exactly one Counters')
    counters = {}
    for key in ('total', 'passed', 'failed'):
        val = counters_nodes[0].get(key)
        if val is None or not re.fullmatch(r'[0-9]+', val):
            raise Invalid('Counters.%s missing or not a non-negative integer' % key)
        counters[key] = int(val)
    actual = {'total': len(rows), 'passed': sum(1 for x in rows if x[1] == 'Passed'), 'failed': sum(1 for x in rows if x[1] == 'Failed')}
    if counters != actual:
        raise Invalid('Counters %s incoherent with the Results rows %s' % (counters, actual))
    return definitions, rows, counters


def tally(definitions, rows, substring):
    sel = [x for x in rows if fqn_selected(definitions[x[0]][0], definitions[x[0]][1], substring)]
    failed = [x for x in sel if x[1] == 'Failed']
    out = {'Selected': len(sel), 'Passed': sum(1 for x in sel if x[1] == 'Passed'), 'Failed': len(failed),
           'NotExecuted': sum(1 for x in sel if x[1] == 'NotExecuted')}
    keys = sorted(definitions[x[0]][0] + '.' + definitions[x[0]][1] for x in sel)
    out['SelectedKeysSha256'] = sha256('\n'.join(keys).encode('utf-8'))
    composition = {}
    for x in failed:
        c = classify_failure(x[3])
        composition[c] = composition.get(c, 0) + 1
    out['FailureComposition'] = dict(sorted(composition.items()))
    out['FailedOutsideSelection'] = sum(1 for x in rows if x[1] == 'Failed') - len(failed)
    return out, keys


def diagnostics(definitions, rows, substring):
    """Counts under rejected methods (never decide the result): display name, class only, case-sensitive FQN, method only."""
    s = fold(substring)
    d = {
        'DisplayNameCaseInsensitive': sum(1 for x in rows if s in fold(x[2])),
        'ClassNameOnly': sum(1 for x in rows if s in fold(definitions[x[0]][0])),
        'CaseSensitiveFqn': sum(1 for x in rows if substring in definitions[x[0]][0] + '.' + definitions[x[0]][1]),
        'MethodNameOnly': sum(1 for x in rows if s in fold(definitions[x[0]][1])),
    }
    return d


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


def git_paths(repo, a, b, *pathspec):
    rc, out, _ = git(repo, 'diff', '--name-only', '-z', '--no-renames', '--no-ext-diff', '--no-textconv',
                     '--ignore-submodules=none', '--no-relative', '--no-color', a, b, '--', *pathspec)
    if rc != 0:
        raise Invalid('git diff exited %d' % rc)
    try:
        return sorted(set(p.decode('utf-8', errors='strict') for p in out.split(b'\x00') if p))
    except UnicodeDecodeError:
        raise Invalid('changed path is not valid UTF-8')


def parse_counts(text, name):
    m = COUNTS_RE.fullmatch(text or '')
    if not m:
        raise UsageError('%s must be Selected/Passed/Failed' % name)
    return {'Selected': int(m.group(1)), 'Passed': int(m.group(2)), 'Failed': int(m.group(3))}


def parse_composition(text):
    if not COMPOSITION_RE.fullmatch(text or ''):
        raise UsageError('--expect-red-composition must be name=count[,name=count...]')
    out = {}
    for part in text.split(','):
        k, v = part.split('=')
        if k in out:
            raise UsageError('duplicate composition class %s' % k)
        out[k] = int(v)
    return dict(sorted(out.items()))


def verify(a, out, check):
    for name in ('base_sha', 'chain_base_sha', 'red_sha', 'current_sha'):
        if not check('Args.%s' % name, bool(SHA_RE.fullmatch(getattr(a, name) or '')), 'INVALID_SHA_ARGUMENT', '%s must be 40 lowercase hex characters' % name):
            return
    fm = FILTER_RE.fullmatch(a.filter or '')
    if not check('Filter.Supported', bool(fm), 'UNSUPPORTED_FILTER', 'only a single FullyQualifiedName~<substring> condition is supported'):
        return
    substring = fm.group(1)
    if not check('Args.MinSelected', bool(re.fullmatch(r'[1-9][0-9]*', a.min_selected or '')), 'INVALID_MIN_SELECTED', 'MinSelected must be a positive integer'):
        return
    min_selected = int(a.min_selected)
    exp_green, exp_red = parse_counts(a.expect_green, '--expect-green'), parse_counts(a.expect_red, '--expect-red')
    exp_comp = parse_composition(a.expect_red_composition)
    out['Filter'] = {'Expression': a.filter, 'Substring': substring, 'MatchKey': 'TestMethod className + "." + name',
                     'Matching': 'ordinal case-insensitive substring (per-character simple upper-case fold)', 'MinSelected': min_selected}
    out['TaskId'], out['BaseSha'], out['ChainBaseSha'], out['RedSha'], out['CurrentSha'] = a.task_id, a.base_sha, a.chain_base_sha, a.red_sha, a.current_sha

    # Expected ChainRedFiles (accredited list bound to the occurrence)
    try:
        raw = read_bytes(a.expected_chain_red_files)
        out['ExpectedChainRedFilesSha256'] = sha256(raw)
        expected_rt = json.loads(raw.decode('utf-8', errors='strict'), object_pairs_hook=reject_duplicates, parse_constant=reject_constant)
        ok = isinstance(expected_rt, list) and expected_rt and all(isinstance(p, str) and p.startswith('tests/') for p in expected_rt) and len(set(expected_rt)) == len(expected_rt)
        check('Input.ExpectedChainRedFiles', bool(ok), 'EXPECTED_CHAIN_RED_FILES_INVALID', 'non-empty JSON array of distinct tests/ paths')
        if not ok:
            return
        expected_rt = sorted(expected_rt)
    except (OSError, UnicodeDecodeError, ValueError, Invalid) as e:
        check('Input.ExpectedChainRedFiles', False, 'EXPECTED_CHAIN_RED_FILES_INVALID', type(e).__name__)
        return

    # TRX files
    trx = {}
    for role, path in (('Green', a.green_trx), ('Red', a.red_trx)):
        try:
            raw = read_bytes(path)
            out[role]['TrxSha256'] = sha256(raw)
            trx[role] = load_trx(raw)
            check('Trx.%sReadable' % role, True)
        except (OSError, Invalid) as e:
            check('Trx.%sReadable' % role, False, 'TRX_UNREADABLE', '%s: %s' % (role, e))
    if len(trx) != 2:
        return

    # Handoffs and term set
    loaded = {}
    for role, path, code in (('Real', a.real_handoff, 'REAL_UNREADABLE'), ('Mutated', a.mutated_handoff, 'MUTATED_UNREADABLE')):
        try:
            raw = read_bytes(path)
            out['Mutation'][role + 'HandoffSha256'] = sha256(raw)
            loaded[role] = load_handoff(raw)
            check('Input.%sHandoffReadable' % role, True)
        except (OSError, Invalid) as e:
            check('Input.%sHandoffReadable' % role, False, code, str(e))
    try:
        raw_ts = read_bytes(a.term_set)
        out['Mutation']['TermSetSourceSha256'] = sha256(raw_ts)
        fields, terms, section = parse_term_set(raw_ts)
        out['Mutation']['TermSetSectionSha256'] = sha256(section.encode('utf-8'))
        check('TermSet.Parsed', True)
    except (OSError, Invalid) as e:
        check('TermSet.Parsed', False, 'TERM_SET_INVALID', str(e))
    if len(loaded) != 2 or any(r['Code'] == 'TERM_SET_INVALID' for r in out['Reasons']):
        return
    real, mut = loaded['Real'], loaded['Mutated']
    for role, obj, code in (('Real', real, 'REAL_SCHEMA_INVALID'), ('Mutated', mut, 'MUTATED_SCHEMA_INVALID')):
        probs = schema_problems(obj)
        if not probs:
            try:
                validate(obj, SCHEMA_V1, '$', probs)
            except Invalid as e:
                probs.append(str(e))
        check('Input.%sHandoffSchema' % role, not probs, code, '; '.join(probs[:8]))
    if any(r['Code'] in ('REAL_SCHEMA_INVALID', 'MUTATED_SCHEMA_INVALID') for r in out['Reasons']):
        return

    # Binding of the real delivery to the occurrence
    check('Bind.TaskId', real['TaskId'] == a.task_id, 'BINDING_MISMATCH', 'handoff TaskId')
    check('Bind.BaseSha', real['BaseSha'] == a.base_sha, 'BINDING_MISMATCH', 'handoff BaseSha')
    check('Bind.RedSha', real['RedSha'] == a.red_sha, 'BINDING_MISMATCH', 'handoff RedSha')
    check('Bind.CurrentSha', real['CurrentSha'] == a.current_sha, 'BINDING_MISMATCH', 'handoff CurrentSha')

    # Git: repository, commits, ancestry, ChainRedFiles
    rc, gout, _ = git(a.repo, 'rev-parse', '--show-toplevel')
    same_root = False
    if rc == 0 and os.path.isdir(a.repo):
        top = gout.decode('utf-8', errors='strict').strip()
        same_root = bool(top) and os.path.normcase(os.path.realpath(top)) == os.path.normcase(os.path.realpath(a.repo))
    if not check('Git.Repository', same_root, 'GIT_REPOSITORY_INVALID', '--repo is not the top level of a readable Git working tree'):
        return
    for name, sha in (('BaseSha', a.base_sha), ('ChainBaseSha', a.chain_base_sha), ('RedSha', a.red_sha), ('CurrentSha', a.current_sha)):
        rc, gout, _ = git(a.repo, 'rev-parse', '--verify', '--quiet', '--end-of-options', sha + '^{commit}')
        if not check('Git.%s.Commit' % name, rc == 0 and gout.decode('ascii', 'replace').strip() == sha, 'GIT_NOT_A_COMMIT', name):
            return
    for name, x, y in (('ChainBaseToRed', a.chain_base_sha, a.red_sha), ('BaseToRed', a.base_sha, a.red_sha), ('RedToCurrent', a.red_sha, a.current_sha)):
        rc, _, _ = git(a.repo, 'merge-base', '--is-ancestor', x, y)
        check('Git.Ancestry.%s' % name, rc == 0 and x != y, 'ANCESTRY_INVALID', name)
    try:
        rt = git_paths(a.repo, a.chain_base_sha, a.red_sha, 'tests/')
        red_to_current = git_paths(a.repo, a.red_sha, a.current_sha)
    except Invalid as e:
        check('Git.Diff', False, 'GIT_DIFF_ERROR', str(e))
        return
    out['ChainRedFiles'] = rt
    touched = sorted(set(rt) & set(red_to_current))
    out['RedToCurrentTouchesChainRedFiles'] = touched
    check('ChainRedFiles.NonEmpty', bool(rt), 'CHAIN_RED_FILES_EMPTY', 'ChainBaseSha..RedSha changes no tests/ path')
    check('ChainRedFiles.EqualsAccredited', rt == expected_rt, 'CHAIN_RED_FILES_MISMATCH', 'derived %s, accredited %s' % (rt, expected_rt))
    check('ChainRedFiles.UntouchedByGreen', not touched, 'GREEN_MODIFIES_CHAIN_RED_FILES', json.dumps(touched))

    # Test selection (the decisive rule) and coherence
    g_defs, g_rows, g_cnt = trx['Green']
    r_defs, r_rows, r_cnt = trx['Red']
    g, g_keys = tally(g_defs, g_rows, substring)
    r, r_keys = tally(r_defs, r_rows, substring)
    out['Green'].update(g)
    out['Red'].update(r)
    out['Green']['Counters'], out['Red']['Counters'] = g_cnt, r_cnt
    out['Diagnostics'] = {'Green': diagnostics(g_defs, g_rows, substring), 'Red': diagnostics(r_defs, r_rows, substring),
                          'Note': 'counts under rejected selection methods; informational only, never decide the result'}
    check('Green.SelectedAtLeastMin', g['Selected'] >= min_selected, 'SELECTION_BELOW_MINIMUM', 'GREEN selected %d < %d' % (g['Selected'], min_selected))
    check('Red.SelectedAtLeastMin', r['Selected'] >= min_selected, 'SELECTION_BELOW_MINIMUM', 'RED selected %d < %d' % (r['Selected'], min_selected))
    check('Green.NoFailedSelected', g['Failed'] == 0 and g['NotExecuted'] == 0 and g['Passed'] == g['Selected'], 'GREEN_NOT_ALL_PASSED', json.dumps({k: g[k] for k in ('Selected', 'Passed', 'Failed', 'NotExecuted')}))
    check('Red.HasFailedSelected', r['Failed'] > 0, 'RED_WITHOUT_SELECTED_FAILURES', 'RED has no failed selected test')
    check('Red.FailuresInsideSelection', r['FailedOutsideSelection'] == 0, 'RED_FAILURE_OUTSIDE_SELECTION', '%d failed results outside the selection' % r['FailedOutsideSelection'])
    check('Selection.SameTestsGreenRed', g_keys == r_keys, 'SELECTION_DIFFERS_GREEN_RED', 'selected test identities differ between GREEN and RED')
    check('Green.MatchesExpected', {k: g[k] for k in ('Selected', 'Passed', 'Failed')} == exp_green, 'EXPECTED_COUNT_MISMATCH', 'GREEN %s vs expected %s' % ({k: g[k] for k in ('Selected', 'Passed', 'Failed')}, exp_green))
    check('Red.MatchesExpected', {k: r[k] for k in ('Selected', 'Passed', 'Failed')} == exp_red, 'EXPECTED_COUNT_MISMATCH', 'RED %s vs expected %s' % ({k: r[k] for k in ('Selected', 'Passed', 'Failed')}, exp_red))
    check('Red.CompositionMatchesExpected', r['FailureComposition'] == exp_comp, 'RED_COMPOSITION_MISMATCH', '%s vs expected %s' % (r['FailureComposition'], exp_comp))

    def handoff_counts(runref):
        rows = [t for t in real['TestResults'] if t['RunRef'] == runref]
        if len(rows) != 1:
            return None
        t = rows[0]
        return {'Selected': int(t['Selected']), 'Passed': int(t['Passed']), 'Failed': int(t['Failed']), 'Skipped': int(t['Skipped'])}
    hg, hr = handoff_counts(a.green_runref), handoff_counts(a.red_runref)
    out['Green']['Handoff'], out['Red']['Handoff'] = hg, hr
    check('Green.MatchesHandoff', hg is not None and hg == {'Selected': g['Selected'], 'Passed': g['Passed'], 'Failed': g['Failed'], 'Skipped': g['NotExecuted']},
          'HANDOFF_COUNT_MISMATCH', 'GREEN TRX vs handoff %s' % a.green_runref)
    check('Red.MatchesHandoff', hr is not None and hr == {'Selected': r['Selected'], 'Passed': r['Passed'], 'Failed': r['Failed'], 'Skipped': r['NotExecuted']},
          'HANDOFF_COUNT_MISMATCH', 'RED TRX vs handoff %s' % a.red_runref)

    # Exactness of the negative-control handoff mutation
    m = out['Mutation']
    patterns = compile_terms(terms)
    try:
        real_hits, mut_hits = hits(real, fields, patterns), hits(mut, fields, patterns)
    except Invalid as e:
        check('Mutation.GovernedFieldTypes', False, 'GOVERNED_FIELD_TYPE', str(e))
        return
    for k in IDENTITY_FIELDS:
        check('Mutation.Identity.%s' % k, exact_equal(real[k], mut[k]), 'MUTATION_IDENTITY_CHANGED', k)
    check('Mutation.KeyOrder', list(real.keys()) == list(mut.keys()), 'MUTATION_KEY_ORDER_CHANGED', 'top-level key order')
    changed = [k for k in HANDOFF_KEYS if not exact_equal(real[k], mut[k])]
    m['ChangedFields'] = changed
    check('Mutation.OnlyWorkCompletedChanged', changed == ['WorkCompleted'], 'MUTATION_NOT_ONLY_WORKCOMPLETED', 'changed fields: %s' % changed)
    exact = isinstance(real['WorkCompleted'], str) and mut['WorkCompleted'] == real['WorkCompleted'] + AUTHORIZED_APPEND
    check('Mutation.ExactAuthorizedAppend', exact, 'MUTATION_NOT_AUTHORIZED_APPEND', 'mutated WorkCompleted == real + authorized append')
    check('Mutation.RealClean', not real_hits, 'REAL_HAS_PROHIBITED_TERM', json.dumps(real_hits))

    def counts(hs):
        c = {}
        for h in hs:
            key = (h['Field'], -1 if h['Index'] is None else h['Index'], h['Term'])
            c[key] = c.get(key, 0) + h['Count']
        return c
    rc_, mc_ = counts(real_hits), counts(mut_hits)
    lost = sorted(k for k in rc_ if mc_.get(k, 0) < rc_[k])
    check('Mutation.NoCompensatingDisappearance', not lost, 'MUTATION_COMPENSATING_CHANGE', json.dumps(lost))
    increments = {k: mc_[k] - rc_.get(k, 0) for k in mc_ if mc_[k] > rc_.get(k, 0)}
    m['IntroducedTerms'] = sorted({k[2] for k in increments})
    m['IntroducedOccurrences'] = sum(increments.values())
    one = len(increments) == 1 and sum(increments.values()) == 1 and next(iter(increments))[0] == 'WorkCompleted'
    check('Mutation.ExactlyOneProhibitedTermIntroduced', one, 'MUTATION_TERM_NOT_EXACTLY_ONE', 'introduced %d occurrence(s) in %s' % (sum(increments.values()), sorted({k[0] for k in increments})))


def main(argv=None):
    global NO_GRAFTS_DIR
    checks, reasons = [], []
    out = {'Schema': OUT_SCHEMA, 'Result': 'FAIL', 'Reasons': reasons, 'Filter': None, 'TaskId': None, 'BaseSha': None, 'ChainBaseSha': None,
           'RedSha': None, 'CurrentSha': None, 'ExpectedChainRedFilesSha256': None, 'ChainRedFiles': None, 'RedToCurrentTouchesChainRedFiles': None,
           'Green': {'TrxSha256': None}, 'Red': {'TrxSha256': None}, 'Diagnostics': None,
           'Mutation': {'AuthorizedAppend': AUTHORIZED_APPEND, 'RealHandoffSha256': None, 'MutatedHandoffSha256': None, 'TermSetSourceSha256': None,
                        'TermSetSectionSha256': None, 'ChangedFields': None, 'IntroducedTerms': None, 'IntroducedOccurrences': None},
           'Checks': checks}
    exit_code = EXIT_FAIL

    def check(cid, ok, code=None, detail=None):
        checks.append({'Id': cid, 'Result': 'pass' if ok else 'fail'})
        if not ok:
            reasons.append({'Code': code or cid, 'Check': cid, 'Detail': detail or ''})
        return ok

    NO_GRAFTS_DIR = tempfile.mkdtemp(prefix='i64tf-nografts-')
    try:
        p = Parser(add_help=False, allow_abbrev=False)
        for opt in ('--repo', '--task-id', '--base-sha', '--chain-base-sha', '--red-sha', '--current-sha', '--filter', '--min-selected',
                    '--green-trx', '--red-trx', '--green-runref', '--red-runref', '--expect-green', '--expect-red', '--expect-red-composition',
                    '--expected-chain-red-files', '--real-handoff', '--mutated-handoff', '--term-set'):
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
    out['Result'] = 'PASS' if checks and not reasons and all(c['Result'] == 'pass' for c in checks) else 'FAIL'
    sys.stdout.buffer.write((json.dumps(out, indent=2, ensure_ascii=True) + '\n').encode('ascii'))
    sys.stdout.buffer.flush()
    if out['Result'] == 'PASS':
        return EXIT_PASS
    return exit_code if exit_code != EXIT_PASS else EXIT_FAIL


if __name__ == '__main__':
    sys.exit(main())
