# table_r4.py - evaluator of the round-4 rule (I-62, A-6 round 4 bench). Stdlib only; never writes.
# Adds to table_r3.evaluate (construction recomputed + acceptance table, unchanged) the mechanical comparison of the request
# with its construction for the field that round 4 corrects (decisiones §85, points 3 and 4, O2):
#   * the Closure the request pins must be exactly the closure derived again, from the custodied vector (E, D, H, K, I,
#     designation and custody), with A6-3 rule 6 (b): every extra element is an added or invented input, every missing one an
#     input dropped without the rule;
#   * the derivation record custodied with the request must list exactly the omitted terms and causes derived again.
import importlib.util, json, os, sys

sys.dont_write_bytecode = True
HERE = os.path.dirname(os.path.abspath(__file__))


def _load(name):
    s = importlib.util.spec_from_file_location(name, os.path.join(HERE, name + '.py'))
    m = importlib.util.module_from_spec(s)
    s.loader.exec_module(m)
    return m


TB = _load('table_r3')
DV = _load('derive_r4')


def _canon(c):
    out = {}
    for k in ('Commits', 'Refs', 'Paths', 'Objects'):
        out[k] = sorted(c.get(k) or [])
    out['RevPaths'] = sorted((x['Rev'], x['Path']) for x in (c.get('RevPaths') or []))
    out['Files'] = sorted((x['Name'], x['Sha256']) for x in (c.get('Files') or []))
    out['Schema'] = c.get('Schema')
    return out


def closure_check(request_path, vector_path, record_path):
    """Violations of the Closure field and of its derivation record against a fresh derivation."""
    v = []
    req = TB._json(TB._read(request_path))
    vec = TB._json(TB._read(vector_path))
    rec = TB._json(TB._read(record_path)) if record_path else None
    if not isinstance(req, dict) or not isinstance(vec, dict):
        return ['CLOSURE_CHECK_UNREADABLE'], None
    fresh = DV.derive_vector(vec)
    cl = req.get('Closure') if isinstance(req.get('Closure'), dict) else {}
    given = TB._json(TB._read(cl.get('Path')))
    if not isinstance(given, dict):
        return ['CLOSURE_UNREADABLE'], fresh
    a, b = _canon(given), _canon(fresh['Closure'])
    for k in a:
        if k == 'Schema':
            if a[k] != b[k]:
                v.append('CLOSURE_NOT_DERIVED: Schema')
            continue
        extra = [x for x in a[k] if x not in b[k]]
        missing = [x for x in b[k] if x not in a[k]]
        if extra:
            v.append('CLOSURE_NOT_DERIVED: %d extra %s (added or invented input): %s' % (len(extra), k, json.dumps(extra[:3])))
        if missing:
            v.append('CLOSURE_NOT_DERIVED: %d missing %s: %s' % (len(missing), k, json.dumps(missing[:3])))
    if rec is None:
        v.append('DERIVATION_RECORD_ABSENT')
    else:
        mine = sorted((o['Term'], o['Cause']) for o in fresh['Omitted'])
        theirs = sorted((o.get('Term'), o.get('Cause')) for o in (rec.get('Omitted') or []) if isinstance(o, dict))
        if mine != theirs:
            v.append('DERIVATION_RECORD_DIFFERS: record %s, derived again %s' % (json.dumps(theirs), json.dumps(mine)))
        if rec.get('ClosureSha256') != cl.get('Sha256'):
            v.append('DERIVATION_RECORD_NOT_TIED_TO_THE_CLOSURE')
    return v, fresh


def evaluate(request_path, acc_rc, acc_stdout, cv_path, deleg_path, handoff_path, consts, vector_path, record_path,
             executions=None, reconstructed=None):
    base = TB.evaluate(request_path, acc_rc, acc_stdout, cv_path, deleg_path, handoff_path, consts, executions, reconstructed)
    cv, fresh = closure_check(request_path, vector_path, record_path)
    base = dict(base)
    base['ClosureViolations'] = cv
    base['Accepted'] = base['Accepted'] and not cv
    base['Omitted'] = [(o['Term'], o['Cause']) for o in fresh['Omitted']] if fresh else None
    return base
