# contract_crosscheck.py - Coordinator-side recomputation of the `Contract` check (AP 16.9 #4: A3-A5 of 16.5 with the A3'-A5'
# additions of 16.22) from the custodied CONTRACT and DELEG of an invocation, with U-19 as recorded in decisiones §62, punto 3
# ("/**" final = prefijo terminado en "/"), WITHOUT the B-min instrument (I-62, decisiones §92, punto 4).
#
# Written independently of the instrument (bmin_facts.py), of the acceptor (bmin_accept.py) and of the Controller simulation of
# d1_nc2.py: the containment follows the text of AP 16.5 L553-L554 literally (a file entry of A is covered by an equal entry or by
# a prefix of B that it starts with; a prefix entry of A only by a prefix of B that contains it), after U-19 on every entry.
# Stdlib only; reads files, writes nothing.
#   python -I contract_crosscheck.py CONTRACT.json DELEG.json [controller-output.json]
import json
import sys

ORDER_LEVEL = {'NOT_REQUIRED': 0, 'PREFERRED': 1, 'REQUIRED': 2}


def read_json(path):
    with open(path, 'rb') as fh:
        return json.loads(fh.read().decode('utf-8'))


def norm(entry):
    """U-19: an entry whose last segment is '**' is read as the prefix that ends in '/'."""
    if entry.endswith('/**'):
        return entry[:-2]
    return entry


def is_prefix(entry):
    return entry.endswith('/')


def a_in_b(a, b):
    """AP 16.5: entry a (of A) is covered by entry b (of B), both read with U-19."""
    a, b = norm(a), norm(b)
    if is_prefix(a):
        return is_prefix(b) and a.startswith(b)          # a prefix of A only by a prefix of B that contains it
    return a == b or (is_prefix(b) and a.startswith(b))


def recompute(K, D):
    rows = []

    def subset(name, small, big, small_label, big_label):
        for a in small:
            by = [b for b in big if a_in_b(a, b)]
            rows.append({'Check': name, 'Entry': a, 'Of': small_label, 'CoveredBy': by, 'In': big_label, 'Ok': bool(by)})

    subset('A3', D['AllowedWriteScope'], K['AllowedWriteScope'], 'DELEG.AllowedWriteScope', 'CONTRACT.AllowedWriteScope')
    subset('A4', K['ForbiddenWriteScope'], D['ForbiddenWriteScope'], 'CONTRACT.ForbiddenWriteScope', 'DELEG.ForbiddenWriteScope')
    a5 = []
    for x in K['Invariants']:
        if x not in D['Invariants']:
            a5.append(('Invariants', x))
    dstop = [s.get('Id') for s in D['StopConditions']]
    for s in K['StopConditions']:
        if s.get('Id') not in dstop:
            a5.append(('StopConditions', s.get('Id')))
    dauth = [(x.get('Path'), x.get('Section'), x.get('Class')) for x in D['Authorities']]
    for x in K['Authorities']:
        if (x.get('Path'), x.get('Section'), x.get('Class')) not in dauth:
            a5.append(('Authorities', '|'.join([x.get('Path'), x.get('Section'), x.get('Class')])))
    for t in K['RequiredTests']:
        same = [x for x in D['RequiredTests'] if x.get('Filter') == t.get('Filter')]
        if len(same) != 1:
            a5.append(('RequiredTests', t.get('Filter')))
        elif not (isinstance(same[0].get('MinSelected'), int) and same[0]['MinSelected'] >= t.get('MinSelected', 0)):
            a5.append(('RequiredTests.MinSelected', t.get('Filter')))
        elif same[0].get('ExpectRed') != t.get('ExpectRed'):
            a5.append(('RequiredTests.ExpectRed', t.get('Filter')))
    for r in K['RoleRequirements']:
        cand = [x for x in D['RoleRequirements'] if x.get('Role') == r.get('Role') and x.get('Action') == r.get('Action')]
        if len(cand) != 1:
            a5.append(('RoleRequirements', '%s/%s' % (r.get('Role'), r.get('Action'))))
            continue
        if not set(r.get('Mandatory') or []).issubset(set(cand[0].get('Mandatory') or [])):
            a5.append(('RoleRequirements.Mandatory', r.get('Role')))
        for ind in r.get('Independence') or []:
            m = [y for y in cand[0].get('Independence') or [] if y.get('ReferenceRole') == ind.get('ReferenceRole')]
            if len(m) != 1:
                a5.append(('RoleRequirements.Independence', '%s/%s' % (r.get('Role'), ind.get('ReferenceRole'))))
                continue
            for dim, lvl in ind.items():
                if dim == 'ReferenceRole':
                    continue
                if ORDER_LEVEL.get(m[0].get(dim), -1) < ORDER_LEVEL.get(lvl, 99):
                    a5.append(('RoleRequirements.Independence.' + dim, r.get('Role')))
    if json.dumps(K['SupersededCommits'], sort_keys=True) != json.dumps(D['SupersededCommits'], sort_keys=True):
        a5.append(('SupersededCommits', 'differ'))
    ok = all(x['Ok'] for x in rows) and not a5
    return {'Result': 'pass' if ok else 'fail', 'Rows': rows, 'A5AndA3pA5pMissing': [list(x) for x in a5]}


def compare(K, D, cv):
    mine = recompute(K, D)
    theirs = ((cv.get('Checks') or {}).get('Contract') or {}).get('Result')
    return {'Recomputed': mine['Result'], 'Controller': theirs, 'Agree': mine['Result'] == theirs,
            'NotCovered': [r for r in mine['Rows'] if not r['Ok']], 'A5Missing': mine['A5AndA3pA5pMissing']}


def main(argv):
    if len(argv) < 2:
        sys.stderr.write('usage: python -I contract_crosscheck.py CONTRACT.json DELEG.json [controller-output.json]\n')
        return 64
    K, D = read_json(argv[0]), read_json(argv[1])
    out = compare(K, D, read_json(argv[2])) if len(argv) > 2 else recompute(K, D)
    sys.stdout.write(json.dumps(out, ensure_ascii=True, sort_keys=True) + '\n')
    return 0


if __name__ == '__main__':
    sys.exit(main(sys.argv[1:]))
