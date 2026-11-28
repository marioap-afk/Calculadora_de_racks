"""Genera pbt4dcc/apply_nc2_f4b_v3_pbt4d.sh desde apply_nc2_f4b_pbt4d.sh (la v2), con reemplazos exactos: la v3 del sustituto
(decision del Owner del 2026-11-22, «Sustituto v3 y revision», owner-pbt4d-nc2-decision.md). El guion de la v2 no se toca."""
import os
H = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
src = os.path.join(H, 'pbt4dcc', 'apply_nc2_f4b_pbt4d.sh')
dst = os.path.join(H, 'pbt4dcc', 'apply_nc2_f4b_v3_pbt4d.sh')
t = open(src, encoding='utf-8').read()


def rep(t, a, b, n=1):
    assert t.count(a) == n, (a[:90], t.count(a))
    return t.replace(a, b)


t = rep(t, '# PBT4-NC2-EXACT-WHITELIST-F4B-01 (PBT4D-C11, salida (1); la v2 del sustituto de nc2,',
        '# v3 (decision del Owner del 2026-11-22, «Sustituto v3 y revision», owner-pbt4d-nc2-decision.md: la propiedad 10 en forma cerrada; la v2 acaba en\n'
        '# MemoryError con las 37 rutas de PB-T4d). Lo demas, como la v2:\n'
        '# PBT4-NC2-EXACT-WHITELIST-F4B-01 (PBT4D-C11, salida (1); la v2 del sustituto de nc2,')
t = rep(t, 'SRC="$HS/nc2-substitute-f4b-v2/nc2_exact_whitelist_substitute_f4b_v2.py"', 'SRC="$HS/nc2-substitute-f4b-v3/nc2_exact_whitelist_substitute_f4b_v3.py"')
t = rep(t, 'NC2_SHA_REVIEWED=C033E96CE147963FEB9C04733E9BFBEC7493FEA33D8FF79B1C2E8AB860798E45  # la v2 revisada (APTO 0/0/4); si la revision cambiara el script, cambia aqui',
        'NC2_SHA_REVIEWED=E68B6E5993B522DF83E7CD4CD2B136DDF503E237FD560B1A97067F24664328C6  # la v3; se usa solo si su revision independiente da APTO')
n = t.count('nc2_exact_whitelist_substitute_f4b_v2.py')
t = t.replace('nc2_exact_whitelist_substitute_f4b_v2.py', 'nc2_exact_whitelist_substitute_f4b_v3.py')
t = rep(t, 'echo "$RID" > "$S/nc2-substitute-runid.txt"', 'echo "$RID" > "$S/nc2-substitute-v3-runid.txt"')
assert not os.path.exists(dst)
open(dst, 'w', encoding='utf-8', newline='\n').write(t)
print('ok, nombres del script cambiados:', n)
