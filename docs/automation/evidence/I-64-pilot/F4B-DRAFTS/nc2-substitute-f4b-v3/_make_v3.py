"""Genera la v3 del sustituto de nc2 desde la v2 revisada (C033E96C...), con reemplazos exactos (decision del Owner del 2026-11-22,
«Sustituto v3 y revision (Recomendado)», owner-pbt4d-nc2-decision.md). Unico cambio de logica: la propiedad 10 en forma cerrada."""
import ast, hashlib, os
H = os.path.dirname(os.path.abspath(__file__))
V2 = os.path.join(os.path.dirname(H), 'nc2-substitute-f4b-v2', 'nc2_exact_whitelist_substitute_f4b_v2.py')
b = open(V2, 'rb').read()
assert hashlib.sha256(b).hexdigest().upper() == 'C033E96CE147963FEB9C04733E9BFBEC7493FEA33D8FF79B1C2E8AB860798E45'
t = b.decode('utf-8')


def rep(t, a, c):
    assert t.count(a) == 1, (a[:90], t.count(a))
    return t.replace(a, c)


t = rep(t, '"""PBTx-NC2-EXACT-WHITELIST-F4B-01 (v2): ', '"""PBTx-NC2-EXACT-WHITELIST-F4B-01 (v3): ')
t = rep(t, 'Lo demas es identico al v1, salvo --task y su ligadura con los datos.\n',
        'Lo demas es identico al v1, salvo --task y su ligadura con los datos.\n'
        'Version 3 (decision del Owner del 2026-11-22, «Sustituto v3 y revision»): la propiedad 10 se calcula en su forma cerrada en lugar de enumerar los 2^n '
        'subconjuntos del alcance del contrato (con las 37 rutas de PB-T4d la v2 acaba en MemoryError). Es la misma proposicion: toda lista candidata que no cubre '
        'la ruta cambiada p difiere de la lista del contrato, porque esta la cubre, y la lista vacia no la cubre; queda p cubierta por el contrato y no por el '
        'ForbiddenWriteScope. Lo demas es identico a la v2, byte a byte.\n')
t = rep(t, 'Uso:  python -I nc2_exact_whitelist_substitute_f4b_v2.py --task PBT3A', 'Uso:  python -I nc2_exact_whitelist_substitute_f4b_v3.py --task PBT4')
OLD = """    #     Solo AllowedWriteScope decide si la lista blanca admite la ruta (16.9 Scope). Como el contrato la cubre, toda lista que no la cubra difiere
    #     de la lista exacta del contrato. Se comprueba ademas por enumeracion: todos los subconjuntos del alcance del contrato y sus variantes de README 10.
    cands = []
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
NEW = """    #     Solo AllowedWriteScope decide si la lista blanca admite la ruta (16.9 Scope). Como el contrato la cubre, toda lista que no la cubra difiere
    #     de la lista exacta del contrato. v3: la forma cerrada de la enumeracion de la v2 (todos los subconjuntos del alcance del contrato y sus variantes
    #     de README 10): la lista vacia no cubre p, asi que hay al menos una candidata que la excluye, y ninguna que la excluya es igual a ca si ca cubre p.
    N['AlternativeCandidatesExcludingPath'] = 'forma cerrada (v3): al menos 1; ninguna igual al alcance del contrato si este cubre la ruta (2^%d subconjuntos sin enumerar)' % len(ca)
    P['10_NoAlternativeOneFieldMutation'] = p is not None and covered(p, ca) and not covered(p, ct.get('ForbiddenWriteScope', []))
"""
t = rep(t, OLD, NEW)
ast.parse(t)
out = os.path.join(H, 'nc2_exact_whitelist_substitute_f4b_v3.py')
assert not os.path.exists(out)
open(out, 'w', encoding='utf-8', newline='').write(t)
print('v3', hashlib.sha256(t.encode('utf-8')).hexdigest().upper())
