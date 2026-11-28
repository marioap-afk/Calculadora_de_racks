"""F4T3B1-NC1-IDENTITY-SIGNAL-01: sustituto NUEVO y local de nc1, solo para F4-T3B1-EDIT-SURFACE (intento 2), autorizado por la disposicion del
Owner y del Coordinator sobre el cierre de F4-T3B1 (propuesta (a), F4-E43). No es la misma firma que los sustitutos de nc1 aceptados antes
(F4T2B2B-NC1-IDENTITY-SIGNAL-01, F4T2C-NC1-IDENTITY-SIGNAL-01, regla de PD-F4-04): aqui el CurrentSha mutado es falso, el Controller rechaza la
entrega mutada, pero informa Identity en PASS. Este sustituto aporta una senal de identidad independiente y mas fuerte: la decide la sesion de
forma determinista contra el repositorio. La corrida ordinaria de nc1 se conserva tal cual (CONTROL NOT PASSED). No es A-4 ni cambia el
AUTOMATION_PLAN.
Propiedades 1-12: el minimo de la disposicion, con su numeracion. Propiedades 13-14: de la propuesta (a) de la sesion.
Falla si: el SHA falso existe; el SHA falso es HEAD; hay una segunda mutacion semantica; el Controller verifico la entrega mutada; la identidad
del repositorio no se puede establecer (cualquier fallo al obtener HEAD, la rama o el remoto, o el predicado de identidad falla con el SHA real).
Uso (desde la raiz del worktree, sin cesion en curso): python -I nc1_identity_signal_substitute.py <dir-verificacion-real> <dir-nc1> <salida.json>
Antes ejecuta su autoprueba sintetica dos veces (determinismo). Solo biblioteca estandar; ejecuta git rev-parse, git branch, git ls-remote,
git cat-file y git merge-base."""
import copy, hashlib, json, os, re, subprocess, sys

ID = 'F4T3B1-NC1-IDENTITY-SIGNAL-01'
BOUND_FAKE = 'deadbeef' * 5
ORDER = ['Termination', 'Handoff', 'Authority', 'Contract', 'Identity', 'Remote', 'Scope', 'CleanTree', 'Ci', 'Tests', 'Trailer', 'Routing', 'FreeText', 'Denials']
CONSUMERS = {'Handoff', 'Identity', 'Remote', 'Ci', 'Tests'}
VARIANCE = {'Ci'}
HEX40 = re.compile(r'^[0-9a-f]{40}$')
sha = lambda b: hashlib.sha256(b).hexdigest().upper()
hex40 = lambda s: isinstance(s, str) and bool(HEX40.match(s))


def walk(o, path=''):
    if isinstance(o, dict):
        for k, v in o.items():
            yield from walk(v, path + '.' + k)
    elif isinstance(o, list):
        for i, v in enumerate(o):
            yield from walk(v, '%s[%d]' % (path, i))
    else:
        yield path, o


def diffpaths(a, b):
    wa, wb = dict(walk(a)), dict(walk(b))
    return sorted(k for k in set(wa) | set(wb) if wa.get(k, '<absent>') != wb.get(k, '<absent>'))


class GitRepo:
    """Hechos del repositorio medidos por la sesion. Cualquier fallo deja la identidad sin establecer."""

    def __init__(self, branch):
        self.errors = []
        self.head = self._out('rev-parse', 'HEAD')
        self.branch = self._out('branch', '--show-current')
        self.remote_tracking = self._out('rev-parse', 'refs/remotes/origin/' + branch)
        ls = self._out('ls-remote', 'origin', 'refs/heads/' + branch)
        self.remote = ls.split()[0] if ls and len(ls.split()) == 2 and ls.split()[1] == 'refs/heads/' + branch else None
        if self.remote is None:
            self.errors.append('ls-remote sin una unica ref exacta')

    def _run(self, *a):
        return subprocess.run(['git'] + list(a), capture_output=True, text=True, encoding='utf-8')

    def _out(self, *a):
        p = self._run(*a)
        if p.returncode != 0:
            self.errors.append('git %s rc=%d' % (a[0], p.returncode))
            return None
        return p.stdout.strip()

    def exists(self, s):
        return hex40(s) and self._run('cat-file', '-e', s).returncode == 0

    def is_commit(self, s):
        if not hex40(s):
            return False
        p = self._run('cat-file', '-t', s)
        return p.returncode == 0 and p.stdout.strip() == 'commit'

    def is_ancestor(self, a, b):
        if not (hex40(a) and hex40(b)):
            return False
        return self._run('merge-base', '--is-ancestor', a, b).returncode == 0


class FakeRepo:
    """Repositorio sintetico de la autoprueba: objetos con su padre."""

    def __init__(self, parents, head, branch, remote, remote_tracking=None, errors=()):
        self.parents, self.head, self.branch, self.remote = parents, head, branch, remote
        self.remote_tracking = remote if remote_tracking is None else remote_tracking
        self.errors = list(errors)

    def exists(self, s):
        return s in self.parents

    def is_commit(self, s):
        return s in self.parents

    def is_ancestor(self, a, b):
        while b is not None:
            if a == b:
                return True
            b = self.parents.get(b)
        return False


def names_full(text, s):
    return hex40(s) and s in (text or '')


def evaluate(b, repo):
    P, N = {}, {}
    ro, rc = b['real_out'], b['real_checks']
    no, nc, ev = b['nc_out'], b['nc_checks'], b['evidence']
    rh, mh = json.loads(b['real_handoff']), json.loads(b['mut_handoff'])
    real_cur, fake = rh.get('CurrentSha'), mh.get('CurrentSha')
    base, red = rh.get('BaseSha'), rh.get('RedSha')
    ctl_head, ctl_lsr = b['ctl'].get('HeadSha'), b['ctl'].get('LsRemoteSha')
    mut = b['mutation']
    N.update({'RealCurrentSha': real_cur, 'MutatedCurrentSha': fake, 'BoundFakeSha': BOUND_FAKE, 'BaseSha': base, 'RedSha': red,
              'HeadNow': repo.head, 'BranchNow': repo.branch, 'RemoteBranchNow': repo.remote, 'RemoteTrackingNow': repo.remote_tracking,
              'HeadAtControl': ctl_head, 'LsRemoteAtControl': ctl_lsr, 'RepoErrors': list(repo.errors)})
    identity_established = not repo.errors and all(hex40(x) for x in (repo.head, repo.remote, repo.remote_tracking))
    N['RepositoryIdentityEstablished'] = identity_established

    # 1. La verificacion real es 14/14 VERIFIED.
    P['1_RealVerification14of14Verified'] = (ro.get('Classification') == 'EXECUTION_VERIFIED' and ro.get('Disposition') == 'NONE'
                                            and list(rc) == ORDER and all(v == 'pass' for v in rc.values()) and ro.get('VerifiedSha') == real_cur)
    # 2. La mutacion de nc1 cambia CurrentSha al SHA inexistente fijado.
    m = mut.get('Mutation', {})
    P['2_MutationChangesCurrentShaToTheBoundNonexistentSha'] = (m.get('File') == 'worker-handoff.json' and m.get('Field') == 'CurrentSha'
                                                               and m.get('Old') == real_cur and m.get('New') == BOUND_FAKE == fake
                                                               and m.get('NewExistsInRepo') is False)
    # 3. Ninguna segunda mutacion semantica; solo los campos de integridad derivados mecanicamente.
    rf, nf = b['real_files'], b['nc_files']
    changed = sorted(k for k in rf if rf[k] != nf.get(k))
    rr0, rr1 = json.loads(rf['relay-record-work.json']), json.loads(nf['relay-record-work.json'])
    N['ChangedInputs'], N['HandoffFieldsChanged'], N['RelayRecordWorkFieldsChanged'] = changed, diffpaths(rh, mh), diffpaths(rr0, rr1)
    dr = mut.get('DerivedRecompute', [])
    P['3_NoSecondSemanticMutation'] = (sorted(rf) == sorted(nf) and changed == ['relay-record-work.json', 'worker-handoff.json']
                                      == sorted(mut.get('ChangedVsVerifiedInputs', []))
                                      and N['HandoffFieldsChanged'] == ['.CurrentSha']
                                      and N['RelayRecordWorkFieldsChanged'] == ['.Outcome.OutputSha256']
                                      and rr0['Outcome']['OutputSha256'].upper() == sha(b['real_handoff'])
                                      and rr1['Outcome']['OutputSha256'].upper() == sha(b['mut_handoff'])
                                      and len(dr) == 1 and dr[0].get('File') == 'relay-record-work.json' and dr[0].get('Field') == 'Outcome.OutputSha256'
                                      and str(dr[0].get('Old', '')).upper() == sha(b['real_handoff']) and str(dr[0].get('New', '')).upper() == sha(b['mut_handoff'])
                                      and sorted(mut.get('InputSha256', {})) == sorted(nf)
                                      and all(v.upper() == sha(nf[k]) for k, v in mut.get('InputSha256', {}).items()))
    # 4-7. El SHA falso no es el CurrentSha real, ni HEAD, ni la rama remota del producto, y no existe en la base de objetos.
    P['4_MutatedCurrentShaDiffersFromRealVerifiedCurrentSha'] = hex40(fake) and hex40(real_cur) and fake != real_cur
    P['5_MutatedCurrentShaIsNotHead'] = identity_established and fake != repo.head and hex40(ctl_head) and fake != ctl_head
    P['6_MutatedCurrentShaIsNotTheRemoteProductBranch'] = (identity_established and fake != repo.remote and fake != repo.remote_tracking
                                                           and hex40(ctl_lsr) and fake != ctl_lsr)
    N['MutatedShaExistsInObjectDatabase'] = repo.exists(fake) if hex40(fake) else None
    P['7_MutatedCurrentShaNotInObjectDatabase'] = hex40(fake) and N['MutatedShaExistsInObjectDatabase'] is False
    # 8. Las relaciones BaseSha / RedSha / CurrentSha real siguen validas.
    rel = {'BaseAndRedUnchangedInMutation': mh.get('BaseSha') == base and mh.get('RedSha') == red,
           'BaseRedCurrentAreCommits': all(repo.is_commit(x) for x in (base, red, real_cur)),
           'BaseAncestorOfRed': repo.is_ancestor(base, red), 'RedAncestorOfCurrent': repo.is_ancestor(red, real_cur) and red != real_cur,
           'CurrentAncestorOfHeadNow': identity_established and repo.is_ancestor(real_cur, repo.head),
           'CurrentAncestorOfRemoteNow': identity_established and repo.is_ancestor(real_cur, repo.remote),
           'HeadAndRemoteAtControlWereCurrent': ctl_head == real_cur == ctl_lsr}
    N['Relationships'] = rel
    P['8_BaseRedCurrentRelationshipsValid'] = all(rel.values())
    # 9 y 11. El Controller no verifico la ocurrencia mutada y termino en un estado no VERIFIED.
    N['Controller'] = {'Classification': no.get('Classification'), 'Disposition': no.get('Disposition'), 'FailureClass': no.get('FailureClass'),
                       'VerifiedSha': no.get('VerifiedSha'), 'Results': {c: nc.get(c) for c in ORDER}}
    P['9_ControllerDidNotVerifyTheMutatedOccurrence'] = no.get('Classification') != 'EXECUTION_VERIFIED' and no.get('VerifiedSha') is None
    # 10. La salida del Controller tiene evidencia que nombra el CurrentSha falso y la identidad real del repositorio.
    both = [c for c in ORDER if names_full(ev.get(c), fake) and names_full(ev.get(c), real_cur)]
    N['ChecksWhoseEvidenceNamesFakeAndRealIdentity'] = both
    P['10_ControllerEvidenceNamesFakeShaAndRealIdentity'] = bool(both)
    P['11_MutatedOccurrenceEndsNonVerified'] = (no.get('Classification') in ('EXECUTION_BLOCKED', 'EXECUTION_REWORK_REQUIRED')
                                               and no.get('Disposition') in ('STOP', 'REWORK') and no.get('FailureClass') not in (None, 'NONE'))

    # 12. El predicado determinista de identidad da FAIL con el SHA mutado (y PASS con el real: si no, la identidad no se puede establecer).
    def identity(s):
        return bool(identity_established and hex40(s) and repo.is_commit(s) and s == ctl_head and repo.is_ancestor(base, s)
                    and repo.is_ancestor(red, s) and repo.is_ancestor(s, repo.head) and repo.is_ancestor(s, repo.remote))
    N['IdentityPredicate'] = {'Definition': 'el SHA es un commit del repositorio, es el HEAD que la sesion midio durante el control, desciende de BaseSha y '
                                            'de RedSha, y es ancestro del HEAD y de la rama remota actuales',
                              'RealCurrentSha': 'PASS' if identity(real_cur) else 'FAIL', 'MutatedCurrentSha': 'PASS' if identity(fake) else 'FAIL'}
    P['12_IdentityPredicateFailsForTheMutatedSha'] = N['IdentityPredicate']['RealCurrentSha'] == 'PASS' and N['IdentityPredicate']['MutatedCurrentSha'] == 'FAIL'
    # 13 (sesion). Las comprobaciones que difieren de la real son consumidoras del CurrentSha: nombran el SHA falso o son la variacion declarada de Ci.
    differs = [c for c in ORDER if nc.get(c) != rc.get(c)]
    attr = {c: ('nombra el CurrentSha falso' if names_full(ev.get(c), fake) else ('variacion PD-F4-06 (no nombra el SHA falso)' if c in VARIANCE else 'SIN ATRIBUIR'))
            for c in differs}
    N['DiffersFromReal'], N['DiffAttribution'] = differs, attr
    P['13_DifferingChecksAreCurrentShaConsumersOrDeclaredVariance'] = (bool(differs) and set(differs) <= CONSUMERS and 'SIN ATRIBUIR' not in attr.values()
                                                                       and any(v == 'nombra el CurrentSha falso' for v in attr.values()))
    # 14 (sesion). FailureClass es la primera comprobacion que no pasa, es Handoff o Identity, y su evidencia nombra el SHA falso y el real.
    notpass = [c for c in ORDER if nc.get(c) != 'pass']
    fc = no.get('FailureClass')
    N['FirstNotPass'] = notpass[0] if notpass else None
    P['14_FailureClassIsFirstNotPassAndNamesBothShas'] = (bool(notpass) and fc == notpass[0] and fc in ('Handoff', 'Identity')
                                                          and names_full(ev.get(fc), fake) and names_full(ev.get(fc), real_cur))
    return P, N, all(P.values())


def synthetic():
    enc = lambda o: (json.dumps(o, ensure_ascii=False, indent=2) + '\n').encode('utf-8')
    BASE, RED, CUR, CUST, OTHER = 'b' * 40, 'c' * 40, 'a' * 40, 'e' * 40, '9' * 40
    BR = 'feature/x'
    parents = {BASE: None, RED: BASE, CUR: RED, CUST: CUR, OTHER: BASE}
    real_h = {'TaskId': 'T', 'Branch': BR, 'BaseSha': BASE, 'RedSha': RED, 'CurrentSha': CUR, 'WorkCompleted': 'w'}
    real_checks = {c: 'pass' for c in ORDER}
    both = 'its CurrentSha is %s, while git rev-parse HEAD is %s' % (BOUND_FAKE, CUR)

    def bundle(fake=BOUND_FAKE, bound_new=None, edit=None, bad_hash=False, extra=False, checks=None, cls='EXECUTION_REWORK_REQUIRED', disp='REWORK',
               fc='Handoff', verified=None, ev_h=both, ev_id='HEAD=%s; ancestors ok' % CUR, ev_ci='RedPart: Core success', ev_tests='tests ok',
               real_cls='EXECUTION_VERIFIED', real_ci='pass', ctl_head=CUR, ctl_lsr=CUR, repo_kw=None, extra_parents=None, ev_extra=None):
        mh = copy.deepcopy(real_h); mh['CurrentSha'] = fake
        if edit:
            edit(mh)
        rhb, mhb = enc(real_h), enc(mh)
        rr0 = {'Outcome': {'OutputSha256': sha(rhb)}}
        rr1 = copy.deepcopy(rr0); rr1['Outcome']['OutputSha256'] = ('0' * 64) if bad_hash else sha(mhb)
        common = {'gate-contract.json': b'{"c": 1}', 'delegation.json': b'{"d": 1}', 'ci-core/1/core.trx': b'<trx/>'}
        rf = dict(common, **{'worker-handoff.json': rhb, 'relay-record-work.json': enc(rr0)})
        nf = dict(common, **{'worker-handoff.json': mhb, 'relay-record-work.json': enc(rr1)})
        if extra:
            nf['delegation.json'] = b'{"d": 2}'
        mutation = {'Mutation': {'File': 'worker-handoff.json', 'Field': 'CurrentSha', 'Old': CUR, 'New': bound_new or fake, 'NewExistsInRepo': False},
                    'DerivedRecompute': [{'File': 'relay-record-work.json', 'Field': 'Outcome.OutputSha256', 'Old': sha(rhb), 'New': sha(mhb)}],
                    'ChangedVsVerifiedInputs': sorted(k for k in rf if rf[k] != nf[k]), 'InputSha256': {k: sha(v) for k, v in nf.items()}}
        nc = dict(real_checks)
        nc.update(checks if checks is not None else {'Handoff': 'fail', 'Ci': 'fail'})
        rc = dict(real_checks); rc['Ci'] = real_ci
        p = dict(parents, **(extra_parents or {}))
        kw = dict(parents=p, head=CUST, branch=BR, remote=CUST)
        kw.update(repo_kw or {})
        return ({'real_out': {'Classification': real_cls, 'Disposition': 'NONE' if real_cls == 'EXECUTION_VERIFIED' else 'REWORK',
                              'VerifiedSha': CUR if real_cls == 'EXECUTION_VERIFIED' else None},
                 'real_checks': rc, 'real_handoff': rhb, 'mut_handoff': mhb, 'real_files': rf, 'nc_files': nf, 'mutation': mutation,
                 'ctl': {'HeadSha': ctl_head, 'LsRemoteSha': ctl_lsr},
                 'nc_out': {'Classification': cls, 'Disposition': disp, 'FailureClass': fc, 'VerifiedSha': verified},
                 'nc_checks': nc, 'evidence': dict({'Handoff': ev_h, 'Identity': ev_id, 'Ci': ev_ci, 'Tests': ev_tests}, **(ev_extra or {}))}, FakeRepo(**kw))

    def second(mh): mh['WorkCompleted'] = 'otra'
    def base_changed(mh): mh['BaseSha'] = OTHER
    allpass = {c: 'pass' for c in ORDER}
    pd04 = {'Handoff': 'fail', 'Identity': 'fail', 'Remote': 'fail', 'Ci': 'fail', 'Tests': 'fail'}
    return [
        ('A', 'el caso real de F4-T3B1: REWORK, Handoff primera en fail nombrando el SHA falso y HEAD, Ci en fail por la variacion, Identity en pass; '
              'HEAD actual = custodia posterior', bundle(), True),
        ('A2', 'la firma de PD-F4-04: Handoff, Identity y consumidores en fail, BLOCKED/STOP', bundle(checks=pd04, cls='EXECUTION_BLOCKED', disp='STOP',
                                                                                                  ev_extra={'Identity': both, 'Remote': both, 'Tests': both}), True),
        ('A3', 'el oraculo ordinario: solo Identity en fail, BLOCKED/STOP, su evidencia nombra ambos SHA',
         bundle(checks={'Identity': 'fail'}, cls='EXECUTION_BLOCKED', disp='STOP', fc='Identity', ev_id=both), True),
        ('A4', 'como A, con HEAD actual = CurrentSha real (antes de la custodia)', bundle(repo_kw={'head': CUR, 'remote': CUR}), True),
        ('B', 'segunda mutacion semantica en la entrega (WorkCompleted)', bundle(edit=second), False),
        ('B2', 'segunda mutacion semantica: BaseSha', bundle(edit=base_changed), False),
        ('C', 'el SHA falso existe en la base de objetos', bundle(extra_parents={BOUND_FAKE: CUR}), False),
        ('C2', 'el SHA falso es HEAD', bundle(extra_parents={BOUND_FAKE: CUST}, repo_kw={'head': BOUND_FAKE}), False),
        ('C3', 'el SHA falso es la rama remota del producto', bundle(repo_kw={'remote': BOUND_FAKE}), False),
        ('C4', 'el SHA falso era HEAD durante el control', bundle(ctl_head=BOUND_FAKE), False),
        ('D', 'hash derivado incoherente', bundle(bad_hash=True), False),
        ('E', 'cambia otra entrada ademas de las derivadas', bundle(extra=True), False),
        ('F', 'el Controller verifico la entrega mutada', bundle(checks=allpass, cls='EXECUTION_VERIFIED', disp='NONE', fc='NONE', verified=BOUND_FAKE), False),
        ('G', 'REWORK pero con VerifiedSha', bundle(verified=CUR), False),
        ('H', 'falla tambien una comprobacion ajena al CurrentSha (Scope)', bundle(checks={'Handoff': 'fail', 'Scope': 'fail'}), False),
        ('I', 'ninguna evidencia nombra el SHA falso', bundle(ev_h='handoff invalido, HEAD %s' % CUR), False),
        ('I2', 'la evidencia nombra el SHA falso pero no la identidad real', bundle(ev_h='CurrentSha %s no coincide' % BOUND_FAKE), False),
        ('J', 'FailureClass que no es la primera en fail', bundle(fc='Ci'), False),
        ('K', 'la verificacion real no es 14/14 VERIFIED', bundle(real_cls='EXECUTION_REWORK_REQUIRED', real_ci='fail'), False),
        ('L', 'la mutacion usa un SHA distinto del fijado', bundle(fake='cafebabe' * 5), False),
        ('M', 'la identidad no se puede establecer: el CurrentSha real no es ancestro del HEAD actual', bundle(repo_kw={'head': OTHER, 'remote': OTHER}), False),
        ('N', 'la identidad no se puede establecer: la rama remota no se resuelve', bundle(repo_kw={'remote': None, 'errors': ['ls-remote']}), False),
        ('O', 'RedSha no es ancestro del CurrentSha real', bundle(extra_parents={CUR: BASE}), False),
        ('P', 'Disposition NONE con Classification no verificada', bundle(disp='NONE'), False),
        ('Q', 'difiere Tests sin nombrar el SHA falso (consumidor sin atribuir)', bundle(checks={'Handoff': 'fail', 'Tests': 'fail'}), False),
        ('R', 'el Controller no detecto nada (todo en pass) pero no verifico', bundle(checks=allpass, fc='NONE'), False),
    ]


def selftest():
    out = []
    for c, d, (bd, repo), exp in synthetic():
        P, _, ok = evaluate(bd, repo)
        out.append({'Case': c, 'Description': d, 'Result': 'PASS' if ok else 'FAIL', 'Expected': 'PASS' if exp else 'FAIL', 'Ok': ok == exp,
                    'FailedProperties': sorted(k for k, v in P.items() if not v)})
    return out


def load_inputs(d):
    files = {}
    base = os.path.join(d, 'inputs')
    for r, _, fs in os.walk(base):
        for f in fs:
            p = os.path.join(r, f)
            files[os.path.relpath(p, base).replace(os.sep, '/')] = open(p, 'rb').read()
    return files


def checks_of(out):
    ch = out['Checks']
    if isinstance(ch, dict):
        return {k: v['Result'] for k, v in ch.items()}, {k: v.get('Evidence', '') for k, v in ch.items()}
    return {c['Id']: c['Result'] for c in ch}, {c['Id']: c.get('Evidence', '') for c in ch}


def main(real_dir, nc_dir, out_path):
    st, st2 = selftest(), selftest()
    res = {'Id': ID, 'NotA4': True, 'NewTaskLocalSubstitute': True,
           'NotTheSameSignatureAs': ['PD-F4-04 (F4T2B2B-NC1-IDENTITY-SIGNAL-01, F4T2C-NC1-IDENTITY-SIGNAL-01 y la regla de nc1 de la orden de F4-T3)'],
           'Authority': 'disposicion del Owner y del Coordinator sobre el cierre de F4-T3B1 (propuesta (a)); F4-E43',
           'Scope': 'solo F4-T3B1-EDIT-SURFACE, intento 2; no reemplaza al Controller ni a nc1 en otras tareas; no cambia el AUTOMATION_PLAN',
           'SelfTest': st, 'SelfTestPass': all(x['Ok'] for x in st), 'SelfTestDeterministic': st == st2}
    if not (res['SelfTestPass'] and res['SelfTestDeterministic']):
        res['SubstituteResult'] = 'FAIL (autoprueba)'
    else:
        ld = lambda p: json.load(open(p, encoding='utf-8'))
        rf, nf = load_inputs(real_dir), load_inputs(nc_dir)
        rh = json.loads(rf['worker-handoff.json'])
        real_out, nc_out = ld(os.path.join(real_dir, 'output.json')), ld(os.path.join(nc_dir, 'output.json'))
        real_checks, _ = checks_of(real_out)
        nc_checks, ev = checks_of(nc_out)
        ctl = ld(os.path.join(nc_dir, 'relay-record.json')).get('Exit', {})
        repo = GitRepo(rh.get('Branch', ''))
        bd = {'real_out': real_out, 'real_checks': real_checks, 'real_handoff': rf['worker-handoff.json'], 'mut_handoff': nf['worker-handoff.json'],
              'real_files': rf, 'nc_files': nf, 'mutation': ld(os.path.join(nc_dir, 'mutation.json')),
              'ctl': {'HeadSha': ctl.get('HeadSha'), 'LsRemoteSha': ctl.get('LsRemoteSha')},
              'nc_out': nc_out, 'nc_checks': nc_checks, 'evidence': ev}
        P, N, ok = evaluate(bd, repo)
        ok = ok and repo.branch == rh.get('Branch')
        N['BranchMatchesHandoff'] = repo.branch == rh.get('Branch')
        res.update({'RealVerification': real_dir.replace(os.sep, '/'), 'Nc1Run': nc_dir.replace(os.sep, '/'),
                    'Nc1ControllerEvidence': {c: ev.get(c) for c in ('Handoff', 'Identity', 'Ci')},
                    'Measured': N, 'Properties': P, 'SubstituteResult': 'PASS' if ok else 'FAIL',
                    'Record': {'Nc1OrdinaryRun': 'CONTROL NOT PASSED (se conserva tal cual; no es CONTROL PASS)',
                               'nc1': 'SATISFIED_UNDER_F4T3B1_IDENTITY_SIGNAL' if ok else 'NOT SATISFIED',
                               'ProtocolDebt': 'PD-F4-05: el Controller puede informar Identity en PASS ante un CurrentSha deliberadamente falso mientras rechaza la entrega por otra comprobacion'}})
    open(out_path, 'w', encoding='utf-8', newline='\n').write(json.dumps(res, ensure_ascii=False, indent=2) + '\n')
    print(json.dumps({'SelfTestPass': res['SelfTestPass'], 'Deterministic': res['SelfTestDeterministic'],
                      'SelfTest': [(x['Case'], x['Result'], x['Ok'], x['FailedProperties']) for x in st],
                      'SubstituteResult': res['SubstituteResult'], 'Properties': res.get('Properties'), 'Measured': res.get('Measured')}, ensure_ascii=False, indent=1))
    return 0 if res['SubstituteResult'] == 'PASS' else 1


if __name__ == '__main__':
    if sys.argv[1:] == ['--selftest']:
        r = selftest()
        print(json.dumps([(x['Case'], x['Result'], x['Ok'], x['FailedProperties']) for x in r], ensure_ascii=False))
        sys.exit(0 if all(x['Ok'] for x in r) and r == selftest() else 1)
    sys.exit(main(*sys.argv[1:4]))
