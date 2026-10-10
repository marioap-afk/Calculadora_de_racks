#!/usr/bin/env python3
"""I-64 Scope bridge (I64-SCOPE-BRIDGE-01, docs/initiatives/I-64-A-1.md).

Mechanical, fail-closed replacement of the model-executed nc2 Scope negative control for I-64 delegated tasks.
It re-evaluates the I-61 Scope invariant (AUTOMATION_PLAN 16.9, check 7) with Git alone:

    every path changed in BaseSha..CurrentSha is covered by AllowedWriteScope and none intersects ForbiddenWriteScope

Python standard library only. PASS (exit 0) only when every check passes; any failed check, missing or malformed
evidence, Git error, invalid scope, ambiguous path or unexpected condition is FAIL (nonzero exit). The JSON on stdout
is deterministic: no timestamps, no absolute paths, fixed key order, sorted path lists.
"""
import argparse
import hashlib
import json
import os
import re
import shutil
import subprocess
import sys
import tempfile

SCHEMA = 'rackcad-i64-scope-bridge/v1'
DELEGATION_SCHEMA = 'rackcad-delegation/v1'
HANDOFF_SCHEMA = 'rackcad-worker-handoff/v1'
# Whole-string matches only (fullmatch): with match(), a '$' anchor would also accept a trailing newline (I64-A2-RUNID-FAIL-OPEN).
SHA_RE = re.compile(r'[0-9a-f]{40}')
RUNID_RE = re.compile(r'R[0-9]{8}T[0-9]{6}Z-[0-9a-f]{4}')
GIT_TIMEOUT_SECONDS = 120
EXIT_PASS, EXIT_FAIL, EXIT_USAGE = 0, 1, 2


class UsageError(Exception):
    pass


class Parser(argparse.ArgumentParser):
    def error(self, message):
        raise UsageError(message)


class Report:
    def __init__(self):
        self.reasons = []
        self.checks = []
        self.fields = {
            'BaseSha': None, 'CurrentSha': None, 'DelegationSha256': None, 'HandoffSha256': None,
            'DelegationRunId': None, 'ChangedPaths': None, 'AllowedWriteScope': None, 'ForbiddenWriteScope': None,
            'OutsideAllowed': None, 'ForbiddenHits': None,
        }

    def check(self, check_id, ok, code=None, detail=None):
        self.checks.append({'Id': check_id, 'Result': 'pass' if ok else 'fail'})
        if not ok:
            self.reasons.append({'Code': code or check_id, 'Detail': detail or ''})
        return ok

    def document(self):
        result = 'PASS' if self.checks and not self.reasons and all(c['Result'] == 'pass' for c in self.checks) else 'FAIL'
        doc = {'Schema': SCHEMA, 'Result': result, 'Reasons': self.reasons}
        doc.update(self.fields)
        doc['Checks'] = self.checks
        return doc


NO_GRAFTS_DIR = None


def git_env():
    # Fail closed against redirection: no inherited GIT_* variable may point Git at another repository, index,
    # object store or configuration. GIT_GRAFT_FILE points at a path that does not exist (inside an empty directory
    # created for this run), so a deprecated <GIT_DIR>/info/grafts file cannot rewrite parents and fake ancestry.
    env = {k: v for k, v in os.environ.items() if not k.upper().startswith('GIT_')}
    env['GIT_TERMINAL_PROMPT'] = '0'
    env['GIT_GRAFT_FILE'] = os.path.join(NO_GRAFTS_DIR, 'no-grafts')
    return env


def git(repo, *args):
    cmd = ['git', '--no-replace-objects', '-c', 'core.fsmonitor=false', '-C', repo] + list(args)
    proc = subprocess.run(cmd, capture_output=True, env=git_env(), timeout=GIT_TIMEOUT_SECONDS, shell=False)
    return proc.returncode, proc.stdout, proc.stderr


def reject_duplicates(pairs):
    keys = [k for k, _ in pairs]
    if len(keys) != len(set(keys)):
        raise ValueError('duplicate key in JSON object')
    return dict(pairs)


def reject_constant(name):
    # NaN, Infinity and -Infinity are not JSON values (I64-A3-JSON-CONSTANT-FAIL-OPEN).
    raise ValueError('non-JSON constant ' + name)


def reject_lone_surrogates(value):
    # A string holding a lone UTF-16 surrogate (e.g. an escaped \ud800) is not valid Unicode: ambiguous evidence.
    stack = [value]
    while stack:
        item = stack.pop()
        if isinstance(item, str):
            if any(0xD800 <= ord(ch) <= 0xDFFF for ch in item):
                raise ValueError('lone surrogate in JSON string')
        elif isinstance(item, dict):
            stack.extend(item.keys())
            stack.extend(item.values())
        elif isinstance(item, list):
            stack.extend(item)


def load_json(path):
    with open(path, 'rb') as fh:
        raw = fh.read()
    sha = hashlib.sha256(raw).hexdigest()
    text = raw.decode('utf-8', errors='strict')
    data = json.loads(text, object_pairs_hook=reject_duplicates, parse_constant=reject_constant)
    reject_lone_surrogates(data)
    return data, sha


def valid_scope_entry(entry):
    if not isinstance(entry, str) or entry == '' or entry != entry.strip():
        return False
    if any(ord(ch) < 0x20 or ord(ch) == 0x7F for ch in entry):
        return False
    if '\\' in entry or entry.startswith('/') or re.match(r'^[A-Za-z]:', entry):
        return False
    if any(ch in entry for ch in '*?[]{}!'):
        return False
    body = entry[:-1] if entry.endswith('/') else entry
    segments = body.split('/')
    return all(seg not in ('', '.', '..') for seg in segments)


def valid_changed_path(path):
    if path == '' or path.endswith('/') or path.startswith('/') or '\\' in path:
        return False
    if any(ord(ch) < 0x20 or ord(ch) == 0x7F for ch in path):
        return False
    return all(seg not in ('', '.', '..') for seg in path.split('/'))


def covered(path, entry):
    return path.startswith(entry) if entry.endswith('/') else path == entry


def validate_scope_list(report, name, value, allow_empty):
    if not isinstance(value, list):
        return report.check(f'{name}.Present', False, f'{name}_MISSING_OR_NOT_LIST', f'{name} is missing or is not a JSON array')
    report.check(f'{name}.Present', True)
    if not value and not allow_empty:
        return report.check(f'{name}.Syntax', False, f'{name}_EMPTY', f'{name} is empty')
    bad = [e for e in value if not valid_scope_entry(e)]
    if bad:
        return report.check(f'{name}.Syntax', False, f'{name}_INVALID_ENTRY', 'invalid entries: ' + json.dumps(bad, ensure_ascii=True))
    if len(set(value)) != len(value):
        return report.check(f'{name}.Syntax', False, f'{name}_DUPLICATE_ENTRY', 'duplicate entries')
    return report.check(f'{name}.Syntax', True)


def verify(args, report):
    for name, value in (('ExpectedBase', args.expected_base), ('ExpectedCurrent', args.expected_current)):
        if not report.check(f'{name}.Format', bool(SHA_RE.fullmatch(value or '')), f'{name.upper()}_INVALID_SHA',
                            f'{name} must be 40 lowercase hex characters'):
            return
    report.fields['BaseSha'] = args.expected_base
    report.fields['CurrentSha'] = args.expected_current

    try:
        delegation, report.fields['DelegationSha256'] = load_json(args.delegation)
    except (OSError, UnicodeDecodeError, ValueError) as exc:
        report.check('Delegation.Parse', False, 'DELEGATION_UNREADABLE', type(exc).__name__)
        return
    report.check('Delegation.Parse', True)
    try:
        handoff, report.fields['HandoffSha256'] = load_json(args.handoff)
    except (OSError, UnicodeDecodeError, ValueError) as exc:
        report.check('Handoff.Parse', False, 'HANDOFF_UNREADABLE', type(exc).__name__)
        return
    report.check('Handoff.Parse', True)
    if not report.check('Delegation.Type', isinstance(delegation, dict) and delegation.get('Schema') == DELEGATION_SCHEMA,
                        'DELEGATION_SCHEMA_UNEXPECTED', f'Schema must be {DELEGATION_SCHEMA}'):
        return
    if not report.check('Handoff.Type', isinstance(handoff, dict) and handoff.get('Schema') == HANDOFF_SCHEMA,
                        'HANDOFF_SCHEMA_UNEXPECTED', f'Schema must be {HANDOFF_SCHEMA}'):
        return

    run_id = delegation.get('RunId')
    report.fields['DelegationRunId'] = run_id if isinstance(run_id, str) else None
    ok = True
    ok &= report.check('Delegation.RunId', isinstance(run_id, str) and bool(RUNID_RE.fullmatch(run_id)),
                       'DELEGATION_RUNID_INVALID', 'delegation RunId missing or malformed')
    ok &= report.check('Identity.DelegationRunId', isinstance(run_id, str) and handoff.get('DelegationRunId') == run_id,
                       'DELEGATION_RUNID_MISMATCH', 'handoff.DelegationRunId differs from delegation.RunId')
    ok &= report.check('Identity.TaskId', isinstance(delegation.get('TaskId'), str) and handoff.get('TaskId') == delegation.get('TaskId'),
                       'TASKID_MISMATCH', 'handoff.TaskId differs from delegation.TaskId')
    ok &= report.check('Identity.DelegationBaseSha', delegation.get('BaseSha') == args.expected_base,
                       'DELEGATION_BASE_MISMATCH', 'delegation.BaseSha differs from --expected-base')
    ok &= report.check('Identity.HandoffBaseSha', handoff.get('BaseSha') == args.expected_base,
                       'HANDOFF_BASE_MISMATCH', 'handoff.BaseSha differs from --expected-base')
    ok &= report.check('Identity.HandoffCurrentSha', handoff.get('CurrentSha') == args.expected_current,
                       'HANDOFF_CURRENT_MISMATCH', 'handoff.CurrentSha differs from --expected-current')
    allowed = delegation.get('AllowedWriteScope')
    forbidden = delegation.get('ForbiddenWriteScope')
    ok &= validate_scope_list(report, 'AllowedWriteScope', allowed, allow_empty=False)
    ok &= validate_scope_list(report, 'ForbiddenWriteScope', forbidden, allow_empty=True)
    if isinstance(allowed, list):
        report.fields['AllowedWriteScope'] = list(allowed)
    if isinstance(forbidden, list):
        report.fields['ForbiddenWriteScope'] = list(forbidden)
    if not ok:
        return

    # --repo must be exactly the top level of a Git working tree, never a directory nested inside another repository.
    rc, out, _ = git(args.repo, 'rev-parse', '--show-toplevel')
    same_root = False
    if rc == 0 and os.path.isdir(args.repo):
        top = out.decode('utf-8', errors='strict').strip()
        same_root = bool(top) and os.path.normcase(os.path.realpath(top)) == os.path.normcase(os.path.realpath(args.repo))
    if not report.check('Git.Repository', same_root, 'GIT_REPOSITORY_INVALID',
                        '--repo is not the top level of a readable Git working tree'):
        return
    for name, sha in (('BaseSha', args.expected_base), ('CurrentSha', args.expected_current)):
        rc, out, _ = git(args.repo, 'rev-parse', '--verify', '--quiet', '--end-of-options', sha + '^{commit}')
        if not report.check(f'Git.{name}.Commit', rc == 0 and out.decode('ascii', 'replace').strip() == sha,
                            f'GIT_{name.upper()}_NOT_A_COMMIT', f'{name} is not a commit object in the repository'):
            return
    rc, _, _ = git(args.repo, 'merge-base', '--is-ancestor', args.expected_base, args.expected_current)
    if not report.check('Git.Ancestry', rc == 0, 'BASE_NOT_ANCESTOR' if rc == 1 else 'GIT_ANCESTRY_ERROR',
                        'BaseSha is not an ancestor of CurrentSha' if rc == 1 else f'git merge-base exited {rc}'):
        return

    # Explicit options override repository configuration that could hide or reshape changed paths:
    # --ignore-submodules=none (diff.ignoreSubmodules / submodule.*.ignore), --no-relative (diff.relative), --no-color.
    rc, out, _ = git(args.repo, 'diff', '--name-only', '-z', '--no-renames', '--no-ext-diff', '--no-textconv',
                     '--ignore-submodules=none', '--no-relative', '--no-color',
                     args.expected_base, args.expected_current, '--')
    if not report.check('Git.Diff', rc == 0, 'GIT_DIFF_ERROR', f'git diff exited {rc}'):
        return
    try:
        raw_paths = [p for p in out.split(b'\x00') if p != b'']
        paths = [p.decode('utf-8', errors='strict') for p in raw_paths]
    except UnicodeDecodeError:
        report.check('Git.Diff.Decode', False, 'AMBIGUOUS_PATH', 'changed path is not valid UTF-8')
        return
    report.check('Git.Diff.Decode', True)
    paths = sorted(set(paths))
    report.fields['ChangedPaths'] = paths
    if not report.check('Diff.NonEmpty', bool(paths), 'EMPTY_DIFF', 'BaseSha..CurrentSha changes no path'):
        return
    bad = [p for p in paths if not valid_changed_path(p)]
    if not report.check('Diff.PathSyntax', not bad, 'AMBIGUOUS_PATH', 'ambiguous paths: ' + json.dumps(bad, ensure_ascii=True)):
        return

    outside = [p for p in paths if not any(covered(p, e) for e in allowed)]
    hits = [{'Path': p, 'Entry': e} for p in paths for e in forbidden if covered(p, e)]
    report.fields['OutsideAllowed'] = outside
    report.fields['ForbiddenHits'] = hits
    report.check('Scope.Allowed', not outside, 'OUTSIDE_ALLOWED_WRITE_SCOPE',
                 'changed paths not covered by AllowedWriteScope: ' + json.dumps(outside, ensure_ascii=True))
    report.check('Scope.Forbidden', not hits, 'FORBIDDEN_WRITE_SCOPE_INTERSECTION',
                 'changed paths intersecting ForbiddenWriteScope: ' + json.dumps(sorted({h['Path'] for h in hits}), ensure_ascii=True))


def main(argv=None):
    global NO_GRAFTS_DIR
    report = Report()
    exit_code = EXIT_FAIL
    NO_GRAFTS_DIR = tempfile.mkdtemp(prefix='i64sb-nografts-')
    try:
        # add_help=False: -h/--help must not end the process with exit 0 (exit 0 is reserved for PASS).
        parser = Parser(description='I-64 mechanical Scope bridge (fail-closed)', add_help=False, allow_abbrev=False)
        for opt in ('--repo', '--delegation', '--handoff', '--expected-base', '--expected-current'):
            parser.add_argument(opt, required=True)
        args = parser.parse_args(argv)
        verify(args, report)
    except UsageError as exc:
        report.check('Usage', False, 'USAGE_ERROR', str(exc))
        exit_code = EXIT_USAGE
    except subprocess.TimeoutExpired:
        report.check('Git.Timeout', False, 'GIT_TIMEOUT', 'git did not finish in time')
    except FileNotFoundError:
        report.check('Git.Available', False, 'GIT_NOT_AVAILABLE', 'git executable not found')
    except Exception as exc:  # fail closed on anything unexpected
        report.check('Unexpected', False, 'UNEXPECTED_ERROR', type(exc).__name__)
    finally:
        shutil.rmtree(NO_GRAFTS_DIR, ignore_errors=True)
    doc = report.document()
    # Bytes with LF only, so the JSON is identical on every platform (text-mode stdout turns LF into CRLF on Windows).
    sys.stdout.buffer.write((json.dumps(doc, ensure_ascii=True, indent=2) + '\n').encode('ascii'))
    sys.stdout.buffer.flush()
    if doc['Result'] == 'PASS':
        return EXIT_PASS
    return exit_code if exit_code != EXIT_PASS else EXIT_FAIL


if __name__ == '__main__':
    sys.exit(main())
