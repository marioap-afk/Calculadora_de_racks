#!/usr/bin/env python3
"""I64-FREETEXT-BRIDGE-01: mechanical substitute for the FreeText negative-control obligation (I-64 A-5).

Compares the REAL verified worker handoff with the MUTATED nc3 handoff and proves, fail-closed, that:
the real handoff is clean under the complete AUTOMATION_PLAN 16.10 rule; the mutated handoff differs only by the
authorized nc3 mutation (append of AUTHORIZED_APPEND to WorkCompleted); the mutation introduces a forbidden term in a
governed field; identity, file list and every other field remain unchanged.

The 16.10 term set, governed fields and matching semantics are derived mechanically from the authority file given in
--term-set (AUTOMATION_PLAN.md at the MainSha of the verified task). Python standard library only.
Exit 0 only for PASS; 1 for FAIL; 2 for usage errors.
"""
import argparse
import hashlib
import json
import re
import sys

OUT_SCHEMA = 'rackcad-i64-freetext-bridge/v1'
HANDOFF_SCHEMA = 'rackcad-worker-handoff/v1'
AUTHORIZED_APPEND = ' GATE PASS.'
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


class Usage(argparse.ArgumentParser):
    def error(self, message):
        sys.stderr.write('usage error: %s\n' % message)
        sys.exit(2)


class Invalid(Exception):
    pass


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
        obj = json.loads(text, object_pairs_hook=reject_duplicates, parse_constant=reject_constant)
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


def same(x, y):
    return x == y and json.dumps(x) == json.dumps(y)


def hits(obj, fields, patterns):
    out = []
    for field in fields:
        for index, text in governed_strings(obj, field):
            for term, rx in patterns:
                n = len(rx.findall(text))
                if n:
                    out.append({'Field': field, 'Index': index, 'Term': term, 'Count': n})
    return out


def main(argv):
    p = Usage(add_help=False, allow_abbrev=False)
    p.add_argument('--real-handoff', required=True)
    p.add_argument('--mutated-handoff', required=True)
    p.add_argument('--term-set', required=True)
    a = p.parse_args(argv)

    checks = []
    reasons = []
    out = {'Schema': OUT_SCHEMA, 'Result': 'FAIL', 'Reasons': reasons, 'AuthorizedAppend': AUTHORIZED_APPEND,
           'RealHandoffSha256': None, 'MutatedHandoffSha256': None, 'TermSetSourceSha256': None,
           'TermSetSectionSha256': None, 'GovernedFields': None, 'Terms': None, 'TaskId': None, 'RunId': None,
           'BaseSha': None, 'CurrentSha': None, 'ChangedFields': None, 'IntroducedForbiddenTerms': None,
           'RealForbiddenHits': None, 'MutatedForbiddenHits': None, 'Checks': checks}

    def check(cid, ok, detail, code):
        checks.append({'Id': cid, 'Result': 'pass' if ok else 'fail', 'Detail': detail})
        if not ok:
            reasons.append({'Code': code, 'Check': cid, 'Detail': detail})
        return ok

    def finish():
        out['Result'] = 'PASS' if checks and not reasons and all(c['Result'] == 'pass' for c in checks) else 'FAIL'
        sys.stdout.buffer.write((json.dumps(out, indent=2, ensure_ascii=True) + '\n').encode('ascii'))
        return 0 if out['Result'] == 'PASS' else 1

    loaded = {}
    for role, path, code in (('Real', a.real_handoff, 'REAL_UNREADABLE'), ('Mutated', a.mutated_handoff, 'MUTATED_UNREADABLE')):
        try:
            raw = read_bytes(path)
            out[role + 'HandoffSha256'] = sha256(raw)
            loaded[role] = load_handoff(raw)
            check('Input.%sReadable' % role, True, 'strict UTF-8 JSON, no duplicate keys, no non-JSON constants, no lone surrogates', code)
        except (OSError, Invalid) as e:
            check('Input.%sReadable' % role, False, str(e), code)
    try:
        raw_ts = read_bytes(a.term_set)
        out['TermSetSourceSha256'] = sha256(raw_ts)
        fields, terms, section = parse_term_set(raw_ts)
        out['TermSetSectionSha256'] = sha256(section.encode('utf-8'))
        out['GovernedFields'], out['Terms'] = fields, terms
        check('TermSet.Parsed', True, '%d governed fields, %d terms; whole word or phrase, case-insensitive' % (len(fields), len(terms)), 'TERM_SET_AMBIGUOUS')
    except OSError as e:
        check('TermSet.Parsed', False, 'term set unreadable: %s' % e, 'TERM_SET_UNREADABLE')
    except Invalid as e:
        check('TermSet.Parsed', False, str(e), 'TERM_SET_AMBIGUOUS')
    if reasons:
        return finish()

    real, mut = loaded['Real'], loaded['Mutated']
    for role, obj, code in (('Real', real, 'REAL_SCHEMA_INVALID'), ('Mutated', mut, 'MUTATED_SCHEMA_INVALID')):
        probs = schema_problems(obj)
        check('Input.%sSchema' % role, not probs, '; '.join(probs) or 'Schema and exact key set of %s' % HANDOFF_SCHEMA, code)
    if reasons:
        return finish()
    for k in ('TaskId', 'RunId', 'BaseSha', 'CurrentSha'):
        out[k] = real[k]
    patterns = compile_terms(terms)
    try:
        real_hits = hits(real, fields, patterns)
        mut_hits = hits(mut, fields, patterns)
    except Invalid as e:
        check('Input.GovernedFieldTypes', False, str(e), 'GOVERNED_FIELD_TYPE')
        return finish()
    check('Input.GovernedFieldTypes', True, 'every governed field has its schema type', 'GOVERNED_FIELD_TYPE')
    out['RealForbiddenHits'], out['MutatedForbiddenHits'] = real_hits, mut_hits

    for k in IDENTITY_FIELDS:
        check('Identity.%s' % k, same(real[k], mut[k]), 'identical' if same(real[k], mut[k]) else 'differs', 'IDENTITY_CHANGED')
    same_order = list(real.keys()) == list(mut.keys())
    check('Structure.KeyOrder', same_order, 'same top-level key order' if same_order else 'top-level key order differs', 'KEY_ORDER_CHANGED')
    changed = [k for k in HANDOFF_KEYS if not same(real[k], mut[k])]
    out['ChangedFields'] = changed
    check('Structure.OnlyWorkCompletedChanged', changed == ['WorkCompleted'], 'changed fields: %s' % changed, 'UNAUTHORIZED_FIELD_CHANGE')
    check('Real.Clean', not real_hits, 'no forbidden term in the governed fields of the real handoff' if not real_hits else 'real handoff hits: %s' % real_hits, 'REAL_HAS_FORBIDDEN_TERM')
    wc_hits = [h for h in mut_hits if h['Field'] == 'WorkCompleted']
    check('Mutated.WorkCompletedHasTerm', bool(wc_hits), 'WorkCompleted hits: %s' % wc_hits, 'NO_FORBIDDEN_TERM_INTRODUCED')
    exact = isinstance(real['WorkCompleted'], str) and mut['WorkCompleted'] == real['WorkCompleted'] + AUTHORIZED_APPEND
    check('Mutated.ExactAuthorizedAppend', exact, 'mutated WorkCompleted == real WorkCompleted + %r' % AUTHORIZED_APPEND if exact else 'mutated WorkCompleted is not exactly the real one plus the authorized append', 'NOT_EXACT_AUTHORIZED_APPEND')
    others_same = all(same(real[f], mut[f]) for f in fields if f != 'WorkCompleted')
    check('Mutated.OtherGovernedFieldsUnchanged', others_same, 'governed fields other than WorkCompleted identical' if others_same else 'another governed field changed', 'GOVERNED_FIELD_CHANGED')

    def counts(hs):
        c = {}
        for h in hs:
            key = (h['Field'], -1 if h['Index'] is None else h['Index'], h['Term'])
            c[key] = c.get(key, 0) + h['Count']
        return c
    rc, mc = counts(real_hits), counts(mut_hits)
    lost = sorted(k for k in rc if mc.get(k, 0) < rc[k])
    check('Mutated.NoCompensatingDisappearance', not lost, 'no forbidden-term occurrence disappeared' if not lost else 'occurrences lost: %s' % lost, 'COMPENSATING_DISAPPEARANCE')
    introduced = sorted({k[2] for k in mc if mc[k] > rc.get(k, 0)})
    intro_fields = sorted({k[0] for k in mc if mc[k] > rc.get(k, 0)})
    out['IntroducedForbiddenTerms'] = introduced
    check('Mutated.IntroducedOnlyInWorkCompleted', bool(introduced) and intro_fields == ['WorkCompleted'],
          'introduced %s in %s' % (introduced, intro_fields), 'UNAUTHORIZED_TERM_PLACEMENT')
    return finish()


if __name__ == '__main__':
    sys.exit(main(sys.argv[1:]))
