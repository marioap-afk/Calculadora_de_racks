"""El esqueleto inerte de la fachada de PB-T3c (section 5.4 de la v4.7: el archivo RackViewBlockEraser.cs con la clase y el metodo de D-PBT3-11, cuyo
cuerpo es solo `throw new NotSupportedException();`, sin ninguna otra sentencia y sin la cadena PushBack), como parche de git contra el cierre de PB-T3b
(fe1c67f4): un archivo nuevo, en LF y sin BOM, como los del Plugin en el almacen. Es solo src/: las dos clases de prueba del RED las escribe el Worker.
Comprueba lo que CT-PBT3C-01 y 04 y G-9 miran en el texto (la firma exacta, ninguna prohibicion, sin nombrar el piloto, ninguna llamada a la fachada).
Uso (desde hs65/pbt3): python -I make_pbt3c_skeleton.py  -> escribe pbt3c-skeleton.patch"""
import hashlib, os, re

HERE = os.path.dirname(os.path.abspath(__file__))
PATH = 'src/RackCad.Plugin/Views/RackViewBlockEraser.cs'
SIG = 'internal static IReadOnlyCollection<ObjectId> EraseInTransaction(Transaction transaction, ObjectId definitionId)'
LINES = [
    '#nullable enable',
    'using System;',
    'using System.Collections.Generic;',
    'using Autodesk.AutoCAD.DatabaseServices;',
    '',
    'namespace RackCad.Plugin.Views',
    '{',
    '    internal static class RackViewBlockEraser',
    '    {',
    '        ' + SIG,
    '        {',
    '            throw new NotSupportedException();',
    '        }',
    '    }',
    '}',
]
TEXT = '\n'.join(LINES) + '\n'
# lo que fijan CT-PBT3C-01 (la firma; sin bloqueo, transaccion propia, confirmacion, purga, regeneracion ni SyncName; sin PushBack ni el piloto, G-9) y 04
assert TEXT.count(SIG) == 1 and TEXT.count('internal static class RackViewBlockEraser') == 1
for bad in ('LockDocument', 'StartTransaction', 'StartOpenCloseTransaction', '.Commit(', 'Purge', 'Regen', 'SyncName', 'PushBack'):
    assert bad not in TEXT, bad
assert not re.search(r'PushBackPilot\w*|RackPilot\w*|RackCad\.Application\.Systems\.PushBack\.Pilot', TEXT), 'NamesThePilot de G-9'
assert 'RackViewBlockEraser.EraseInTransaction(' not in TEXT, 'CT-04: la llamada solo va en EraseUnit de WR (el GREEN)'
body = re.search(r'definitionId\)\s*\{(.*?)\}\s*\}\s*\}\s*$', TEXT, re.S).group(1)
assert body.strip() == 'throw new NotSupportedException();', 'el cuerpo es solo el throw'
raw = TEXT.encode('utf-8')
assert all(b < 0x80 for b in raw) and b'\r' not in raw
blob = hashlib.sha1(b'blob %d\0' % len(raw) + raw).hexdigest()
patch = ('diff --git a/%s b/%s\nnew file mode 100644\nindex 0000000..%s\n--- /dev/null\n+++ b/%s\n@@ -0,0 +1,%d @@\n' % (PATH, PATH, blob[:7], PATH, len(LINES))
         + ''.join('+' + l + '\n' for l in LINES))
out = os.path.join(HERE, 'pbt3c-skeleton.patch')
open(out, 'wb').write(patch.encode('utf-8'))
print('pbt3c-skeleton.patch', len(patch.encode('utf-8')), 'bytes, sha256', hashlib.sha256(patch.encode('utf-8')).hexdigest(), '| el archivo:', PATH,
      len(raw), 'bytes, blob', blob)
