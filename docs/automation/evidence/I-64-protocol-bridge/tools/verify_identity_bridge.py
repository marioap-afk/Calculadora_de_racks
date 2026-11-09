#!/usr/bin/env python3
"""I64-IDENTITY-BRIDGE-01: deterministic substitute evidence for nc1 of F2-T2A-NAVIGATION-HOST (I-64 Amendment A-12).

Applies only to the exact defect signature of the Controller runs 3d86, 16b6 and bfc8: the Controller evaluated repository
consistency inside Handoff, so the nc1 relative oracle could not be met. This tool answers two separate questions from source
data only (no Controller prose), and fails closed on anything else:

  A. HANDOFF INTEGRITY: the handoff is a valid artifact. Both handoffs are schema-valid; TaskId, Attempt, DelegationRunId,
     WorkRunId, Branch, Worktree, BaseSha and RedSha are the bound ones; the relay OutputSha256 matches the mutated handoff
     bytes; the output time falls inside the cession; and the mutation is exact: only the top-level CurrentSha changed, proven
     structurally (every key, nested, in order) and byte for byte. Repository state is deliberately not consulted here, so a
     false CurrentSha cannot make this question fail.
  B. IDENTITY: the delivery's CurrentSha against authoritative repository state, after proving that state coherent: bound
     branch, HEAD = remote branch = tracking ref = the verified SHA exactly (no ancestor or descendant is accepted), and BaseSha
     and RedSha on the chain.

ControlResult = PASS only with HandoffIntegrity = PASS and Identity = FAIL for exactly the bound synthetic, nonexistent SHA.
The handoff parsing, rackcad-worker-handoff/v1 schema validation and exact comparison are copied verbatim from
verify_testfilter_bridge.py (I-64 A-11, blob f9780f2f30c067243c93b6ab7175cc4b3179ca39), as is its hardened Git execution.
This tool executes neither A-11 nor any other bridge. Python standard library only. Exit 0 only for ControlResult PASS;
1 for FAIL; 2 for usage errors. Deterministic LF-only JSON on stdout.
"""
import argparse
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
    rc, _, _ = git(a.repo, 'cat-file', '-e', candidate + '^{object}')
    check('Control.CandidateNonexistent', rc != 0, 'MUTATED_SHA_EXISTS', 'the candidate names an existing object')


def main(argv=None):
    global NO_GRAFTS_DIR
    checks, reasons = [], []
    out = {'Schema': OUT_SCHEMA, 'Result': 'FAIL', 'ControlResult': 'FAIL', 'HandoffIntegrity': 'NOT_EVALUATED', 'Identity': 'NOT_EVALUATED', 'FailClosed': True,
           'Reasons': reasons, 'Binding': None, 'Inputs': {}, 'Repository': None, 'Mutation': {'ChangedFields': None, 'CandidateCurrentSha': None, 'IdentityTarget': None}, 'Checks': checks}
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
    out['FailClosed'] = not passed and (out['HandoffIntegrity'] != 'PASS' or out['Identity'] == 'NOT_EVALUATED')
    sys.stdout.buffer.write((json.dumps(out, indent=2, ensure_ascii=True) + '\n').encode('ascii'))
    sys.stdout.buffer.flush()
    if passed:
        return EXIT_PASS
    return exit_code if exit_code != EXIT_PASS else EXIT_FAIL


if __name__ == '__main__':
    sys.exit(main())
