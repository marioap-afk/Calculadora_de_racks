"""PB-T4e (paquete v5.6): el texto literal de PBT4E-C11, sacado del de PBT4D-C11 de la v5.4 (por objetos git en 160e019d) con los reemplazos que
pidio la sesion, cada uno comprobado (el texto de origen esta exactamente una vez):
  1. PBT4D -> PBT4E y "PB-T4d" -> "PB-T4e" (sobre el texto de PB-T4d, antes de los demas);
  2. la version 2 del sustituto de nc2 -> la version 3 (F4B-E58: owner-pbt4d-nc2-decision.md, "Sustituto v3 y revision"; la revision APTO), con
     su ruta y su SHA-256, y la propiedad 10 en forma cerrada; lo demas, como la v2;
  3. "an nc2 that the version 2 of the substitute does not give PASS" -> la version 3, y la lista de lo que no se extiende con lo de PB-T4d.
El nc1 sigue igual. Escribe una linea, ASCII, con \n.

Uso (desde v56/): python -I make_c11_v56.py <repo> <salida.txt>
"""
import subprocess
import sys

sys.dont_write_bytecode = True
repo, out = sys.argv[1:3]
v54 = subprocess.run(['git', '-C', repo, 'show', '160e019d:docs/initiatives/I-64-f4b-pbt4-gate-contract-package.md'],
                     capture_output=True, check=True).stdout.decode('utf-8').replace('\r\n', '\n')
lines = [l for l in v54.split('\n') if l.startswith('PBT4D-C11: ')]
assert len(lines) == 1, len(lines)
text = lines[0]


def rep(old, new, count=1):
    global text
    n = text.count(old)
    assert n == count, (old[:60], n)
    text = text.replace(old, new)


rep('PBT4D', 'PBT4E')
rep('PB-T4d', 'PB-T4e')
rep('the version 2 of PBTx-NC2-EXACT-WHITELIST-F4B-01 (F4B-E43: docs/automation/evidence/I-64-pilot/F4B-DRAFTS/nc2-substitute-f4b-v2/'
    'nc2_exact_whitelist_substitute_f4b_v2.py, SHA-256 c033e96ce147963feb9c04733e9bfbec7493fea33d8ff79b1c2e8ab860798e45; owner-pbt3a-nc2-decision.md, '
    '"Aceptar y ampliar el sustituto"), whose property 8 admits only',
    'the version 3 of PBTx-NC2-EXACT-WHITELIST-F4B-01 (F4B-E58: docs/automation/evidence/I-64-pilot/F4B-DRAFTS/nc2-substitute-f4b-v3/'
    'nc2_exact_whitelist_substitute_f4b_v3.py, SHA-256 e68b6e5993b522df83e7cd4cd2b136ddf503e237fd560b1a97067f24664328c6; owner-pbt4d-nc2-decision.md, '
    '"Sustituto v3 y revision", reviewed APTO in docs/automation/evidence/I-64-pilot/F4B-DRAFTS/nc2-substitute-f4b-v3/review/'
    'independent-review-substitute-v3.md; it is the version 2 (F4B-E43: owner-pbt3a-nc2-decision.md, "Aceptar y ampliar el sustituto") with its '
    'property 10 in its closed form, a path is in the AllowedWriteScope of the contract and not in its ForbiddenWriteScope, and everything else as '
    'the version 2), whose property 8 admits only')
rep('an nc2 that the version 2 of the substitute does not give PASS - the disposition of the Owner on the nc2 of PB-T3a (F4B-E43) does not extend -',
    'an nc2 that the version 3 of the substitute does not give PASS - the disposition of the Owner on the nc2 of PB-T3a (F4B-E43) and the satisfaction '
    'of the nc2 of PB-T4d under the version 3 (F4B-E58, for F4B-PBT4-RETIRE-PILOT, attempt 1) do not extend -')
assert 'version 2 of the substitute does not' not in text and 'PBT4D' not in text
assert all(ord(c) < 128 for c in text)
with open(out, 'w', encoding='utf-8', newline='\n') as f:
    f.write(text + '\n')
print('PBT4E-C11: %d caracteres' % len(text))
