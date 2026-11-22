"""DIAGNOSTICO de la sesion, no evidencia ni sustituto: corre el sustituto v2 revisado (C033E96C...) con un unico cambio en memoria, la propiedad 10
en su forma cerrada en lugar de enumerar los 2^n subconjuntos del alcance del contrato (con 37 rutas, MemoryError). Escribe solo en el scratchpad.
Forma cerrada: toda lista candidata que no cubre p difiere de ca, porque ca cubre p; la lista vacia no cubre p, asi que len(excl) > 0. Luego
P10 == (p is not None and covered(p, ca) and not covered(p, Forbidden)).
Uso: python -I diag_nc2_p10.py <v2.py> <wt> <real> <nc2> <base> <cur> <salida.json> <nota> <salida-cruda> [--compare-enum]"""
import hashlib, os, sys, types
src_path, wt = sys.argv[1], sys.argv[2]
src = open(src_path, encoding='utf-8').read()
assert hashlib.sha256(open(src_path, 'rb').read()).hexdigest().upper() == 'C033E96CE147963FEB9C04733E9BFBEC7493FEA33D8FF79B1C2E8AB860798E45'
OLD = """    cands = []
    for r in range(len(ca) + 1):
        for comb in itertools.combinations(ca, r):
            cands.append(list(comb))
            if p is not None:
                cands.append(readme10_mutation(list(comb), changed)[1] if changed else list(comb))
    excl = [c for c in cands if p is not None and not covered(p, c)]
    N['AlternativeCandidatesExcludingPath'] = len(excl)
    P['10_NoAlternativeOneFieldMutation'] = p is not None and covered(p, ca) and len(excl) > 0 and all(c != ca for c in excl) \\
        and not covered(p, ct.get('ForbiddenWriteScope', []))
"""
NEW = """    N['AlternativeCandidatesExcludingPath'] = 'forma cerrada (diagnostico): 2^%d subconjuntos sin enumerar' % len(ca)
    P['10_NoAlternativeOneFieldMutation'] = p is not None and covered(p, ca) and not covered(p, ct.get('ForbiddenWriteScope', []))
"""
assert src.count(OLD) == 1, 'el bloque de la propiedad 10 no es el revisado'
mod = types.ModuleType('nc2diag')
mod.__file__ = os.path.abspath(__file__)  # ScriptSha256 = el de este diagnostico, no el del revisado
code = src.replace(OLD, NEW).replace("if __name__ == '__main__':", "if False:")
exec(compile(code, 'nc2_v2_diag', 'exec'), mod.__dict__)
os.chdir(wt)
rest = sys.argv[3:10]
rc = mod.main('PBT4', *rest)
print('rc', rc)
