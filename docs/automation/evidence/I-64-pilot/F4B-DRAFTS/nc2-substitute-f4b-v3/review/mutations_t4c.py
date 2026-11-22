"""Revision v3 (solo lectura sobre el worktree): mutaciones EN MEMORIA de los datos de PB-T4c (F4B-PBT4-EDIT-ALL-WINDOW) para buscar
casos en los que la v2 termina y da FAIL por la propiedad 10, y comprobar que la v3 tambien da FAIL (y las mismas propiedades).
Carga los datos como main() de los sustitutos (load_inputs, git diff, output.json y mutation.json de nc2, nota y salida cruda) y llama a
evaluate() de cada version, cargada del texto sin modificar. Las mutaciones 'coherentes' recalculan toda la cadena derivada (delegacion real =
salida cruda + 8 colecciones del contrato; copia de nc2 por README 10; relevo de nc2; bfc; nota; InputSha256 de mutation.json).
Las 'mini' restringen PB-T4c a las k primeras rutas del diff (incluida la ruta quitada) para que la v2 termine en segundos.
Uso: python -I -B mutations_t4c.py <v2.py> <v3.py> <worktree> <cases.json> <mutacion> <salida.json>"""
import copy, hashlib, json, os, subprocess, sys, time, types

v2p, v3p, wt, casesp, mid, outp = sys.argv[1:7]
MAIN = "if __name__ == '__main__':"
ALT = 'AlternativeCandidatesExcludingPath'
P10 = '10_NoAlternativeOneFieldMutation'
enc = lambda o: (json.dumps(o, ensure_ascii=False, indent=2) + '\n').encode('utf-8')
sha = lambda b: hashlib.sha256(b).hexdigest().upper()


def load(path, name):
    src = open(path, encoding='utf-8').read()
    assert src.count(MAIN) == 1
    mod = types.ModuleType(name)
    mod.__file__ = path
    exec(compile(src.replace(MAIN, 'if False:'), name, 'exec'), mod.__dict__)
    return mod


m2, m3 = load(v2p, 'v2'), load(v3p, 'v3')
c = json.load(open(casesp, encoding='utf-8'))['PBT4C']
real_dir, nc_dir = os.path.join(wt, c['real']), os.path.join(wt, c['nc2'])
rf, nf = m2.load_inputs(real_dir), m2.load_inputs(nc_dir)
changed0 = subprocess.run(['git', 'diff', '--name-only', c['base'] + '..' + c['cur']], cwd=wt, capture_output=True, text=True, check=True).stdout.split()
nc_out = json.load(open(os.path.join(nc_dir, 'output.json'), encoding='utf-8'))
ch = nc_out['Checks']
nc_checks = {k: v['Result'] for k, v in ch.items()} if isinstance(ch, dict) else {x['Id']: x['Result'] for x in ch}
_, note0 = m2.read_json_object(c['note'], 'nota')
raw_b0, _ = m2.read_json_object(c['raw'], 'cruda')
mutation0 = json.load(open(os.path.join(nc_dir, 'mutation.json'), encoding='utf-8'))


def bundle(rf, nf, changed, note, raw_b, mutation):
    return {'real_deleg': rf['delegation.json'], 'mut_deleg': nf['delegation.json'], 'contract': rf['gate-contract.json'],
            'real_rrp': rf['relay-record-planning.json'], 'mut_rrp': nf['relay-record-planning.json'],
            'real_bfc': rf['byte-fidelity-check.json'], 'mut_bfc': nf['byte-fidelity-check.json'], 'changed': changed,
            'nc_checks': dict(nc_checks), 'real_files': rf, 'nc_files': nf, 'mutation': mutation, 'note': note, 'raw_output': raw_b}


def same_case(new, old):
    return new.upper() if old == old.upper() else new.lower()


def keep_or(old_b, obj):
    """conserva los bytes originales si el contenido no cambia"""
    return old_b if json.loads(old_b) == obj else enc(obj)


def rebuild(ct, changed, mut_aws=None, real_edit=None, keep_real_aws=None):
    """cadena coherente F4B-E21 desde la salida cruda y el contrato dados"""
    rf2, nf2 = dict(rf), dict(nf)
    ct_b = keep_or(rf['gate-contract.json'], ct)
    raw = json.loads(raw_b0)
    real = dict(raw)
    for k in m2.INJECTED:
        real[k] = ct.get(k)
    if keep_real_aws is not None:
        real['AllowedWriteScope'] = keep_real_aws
    if real_edit:
        real_edit(real)
    real_b = keep_or(rf['delegation.json'], real)
    mut = copy.deepcopy(real)
    mut['AllowedWriteScope'] = m2.readme10_mutation(real['AllowedWriteScope'], changed)[1] if mut_aws is None else mut_aws
    mut_b = keep_or(nf['delegation.json'], mut)
    rr1 = json.loads(nf['relay-record-planning.json'])
    rr1['Outcome']['OutputSha256'] = same_case(sha(mut_b), rr1['Outcome']['OutputSha256'])
    rr1_b = keep_or(nf['relay-record-planning.json'], rr1)
    bfc = json.loads(rf['byte-fidelity-check.json'])
    bfc['OutputSha256'] = same_case(sha(real_b), bfc['OutputSha256'])
    bfc['ContractSha256'] = same_case(sha(ct_b), bfc['ContractSha256'])
    bfc_b = keep_or(rf['byte-fidelity-check.json'], bfc)
    note = dict(note0)
    note['DelegationSha256'] = same_case(sha(real_b), note0['DelegationSha256'])
    note['ContractSha256'] = same_case(sha(ct_b), note0['ContractSha256'])
    rf2.update({'gate-contract.json': ct_b, 'delegation.json': real_b, 'byte-fidelity-check.json': bfc_b})
    nf2.update({'gate-contract.json': ct_b, 'delegation.json': mut_b, 'byte-fidelity-check.json': bfc_b, 'relay-record-planning.json': rr1_b})
    mutation = copy.deepcopy(mutation0)
    mutation['InputSha256'] = {k: same_case(sha(nf2[k]), v) for k, v in mutation0.get('InputSha256', {}).items() if k in nf2}
    return bundle(rf2, nf2, changed, note, raw_b0, mutation)


ct0 = json.loads(rf['gate-contract.json'])
p0 = sorted(changed0)[0]
prefix = p0.rsplit('/', 1)[0] + '/'


def mini(k):
    ch_k = sorted(changed0)[:k]
    assert ch_k[0] == p0
    ct = copy.deepcopy(ct0)
    ct['AllowedWriteScope'] = [e for e in ct0['AllowedWriteScope'] if e in ch_k]
    return ct, ch_k


def M(mid):
    if mid == 'REAL':  # sin mutacion: tal como main()
        return 'datos reales sin mutar', bundle(rf, nf, changed0, note0, raw_b0, mutation0)
    if mid == 'K0':
        return 'reconstruccion coherente sin cambios (control: debe ser identica a REAL)', rebuild(copy.deepcopy(ct0), changed0)
    if mid == 'K1':
        ct = copy.deepcopy(ct0); ct['ForbiddenWriteScope'] = ct['ForbiddenWriteScope'] + [p0]
        return 'coherente: la ruta quitada tambien en ForbiddenWriteScope del contrato (y de la delegacion real)', rebuild(ct, changed0)
    if mid == 'K2':
        ct = copy.deepcopy(ct0); ct['ForbiddenWriteScope'] = ct['ForbiddenWriteScope'] + [prefix]
        return 'coherente: el prefijo de la carpeta de la ruta quitada en ForbiddenWriteScope', rebuild(ct, changed0)
    if mid == 'K3':
        ct = copy.deepcopy(ct0); ct['AllowedWriteScope'] = [e for e in ct['AllowedWriteScope'] if e != p0]
        return 'coherente: contrato y delegacion real sin la ruta (README 10 no quita nada: p = None)', rebuild(ct, changed0)
    if mid == 'K4':
        ct = copy.deepcopy(ct0); ct['AllowedWriteScope'] = [e for e in ct['AllowedWriteScope'] if e != p0]
        return 'contrato sin la ruta y delegacion real con ella (el contrato no cubre p)', rebuild(ct, changed0, keep_real_aws=list(ct0['AllowedWriteScope']))
    if mid == 'K5':
        return 'coherente: la copia de nc2 no quita nada (mutada = real; p = None)', rebuild(copy.deepcopy(ct0), changed0, mut_aws=list(ct0['AllowedWriteScope']))
    if mid.startswith('m'):  # mini: k = 10 rutas del diff
        ct, ch_k = mini(10)
        sub = mid[1:]
        if sub == 'K0':
            return 'mini (10 rutas) coherente sin mas cambios', rebuild(ct, ch_k)
        if sub == 'K1':
            ct['ForbiddenWriteScope'] = ct['ForbiddenWriteScope'] + [p0]
            return 'mini: ruta quitada en ForbiddenWriteScope (coherente)', rebuild(ct, ch_k)
        if sub == 'K2':
            ct['ForbiddenWriteScope'] = ct['ForbiddenWriteScope'] + [prefix]
            return 'mini: prefijo de la carpeta en ForbiddenWriteScope (coherente)', rebuild(ct, ch_k)
        if sub == 'K3':
            full = list(ct['AllowedWriteScope'])
            ct['AllowedWriteScope'] = [e for e in full if e != p0]
            return 'mini: contrato y real sin la ruta (p = None)', rebuild(ct, ch_k)
        if sub == 'K4':
            full = list(ct['AllowedWriteScope'])
            ct['AllowedWriteScope'] = [e for e in full if e != p0]
            return 'mini: contrato sin la ruta, real con ella', rebuild(ct, ch_k, keep_real_aws=full)
        if sub == 'K5':
            return 'mini: la copia de nc2 no quita nada (p = None)', rebuild(ct, ch_k, mut_aws=list(ct['AllowedWriteScope']))
        if sub == 'K6':
            ct['AllowedWriteScope'] = []
            return 'mini: contrato y real con alcance vacio', rebuild(ct, ch_k)
        if sub == 'K7':
            ct['AllowedWriteScope'] = [prefix if e == p0 else e for e in ct['AllowedWriteScope']]
            return 'mini (contraste): la ruta exacta sustituida por el prefijo de su carpeta en contrato y real (coherente)', rebuild(ct, ch_k)
        if sub == 'K8':
            ct['AllowedWriteScope'] = ct['AllowedWriteScope'] + [p0]
            return 'mini (contraste): la ruta duplicada en contrato y real (coherente)', rebuild(ct, ch_k)
        if sub == 'K9':
            ct['ForbiddenWriteScope'] = ct['ForbiddenWriteScope'] + [p0 + '/']
            return 'mini (contraste): prohibido "ruta/" (prefijo que no cubre la ruta)', rebuild(ct, ch_k)
    raise SystemExit('mutacion desconocida: ' + mid)


desc, bd = M(mid)
res = {'Mutation': mid, 'Description': desc, 'ChangedPath0': p0, 'LenContractAWS': len(json.loads(bd['contract'])['AllowedWriteScope']), 'LenChanged': len(bd['changed'])}
if mid == 'K0':  # la reconstruccion sin cambios reproduce byte a byte el paquete real
    ref = bundle(rf, nf, changed0, note0, raw_b0, mutation0)
    res['K0IdenticalToReal'] = {k: bd[k] == ref[k] for k in ref}
V3ONLY = len(sys.argv) > 7 and sys.argv[7] == 'v3only'
for name, m in (('v3', m3), ('v2', m2)):
    if V3ONLY and name == 'v2':
        res[name] = {'Overall': 'NO CORRIDA (v3only)', 'Seconds': 0}
        continue
    t0 = time.time()
    try:
        P, N, ok = m.evaluate(copy.deepcopy(bd))
        res[name] = {'Overall': 'PASS' if ok else 'FAIL', 'False': sorted(k for k, v in P.items() if not v), 'P10': P[P10], 'Alt': N.get(ALT),
                     'ChangedPath': N.get('ChangedPath'), 'Measured': {k: v for k, v in N.items() if k != ALT}, 'Seconds': round(time.time() - t0, 1)}
    except Exception as ex:  # noqa
        res[name] = {'Overall': 'FAIL (excepcion)', 'Exception': '%s: %s' % (type(ex).__name__, ex), 'Seconds': round(time.time() - t0, 1)}
v2r, v3r = res['v2'], res['v3']
res['SameOverall'] = v2r['Overall'] == v3r['Overall']
res['SameFalseSet'] = v2r.get('False') == v3r.get('False')
res['SameMeasuredExceptAlt'] = v2r.get('Measured') == v3r.get('Measured')
res['V3PassWhereV2Fail'] = v3r['Overall'] == 'PASS' and v2r['Overall'] != 'PASS'
open(outp, 'w', encoding='utf-8', newline='\n').write(json.dumps(res, ensure_ascii=False, indent=1) + '\n')
print(mid, '|', desc, '| n=%d' % res['LenContractAWS'], '| v2:', v2r['Overall'], v2r.get('False'), v2r.get('Alt'), '%ss' % v2r['Seconds'],
      '| v3:', v3r['Overall'], v3r.get('False'), '%ss' % v3r['Seconds'], '| iguales:', res['SameOverall'], res['SameFalseSet'], res['SameMeasuredExceptAlt'],
      '| v3 PASS con v2 FAIL:', res['V3PassWhereV2Fail'])
