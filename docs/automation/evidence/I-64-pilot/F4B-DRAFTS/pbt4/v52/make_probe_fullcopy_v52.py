"""PB-T4d (paquete v5.2, fase 2): la sonda del equivalente de CT-PBT4B-20 de PB-T4c (PushBackEditAllFullCopyGuardTests), para leer sus lineas con
el piloto retirado: el archivo de 6c2820ba entero, con la clase renombrada (TmpPbt4dFullCopyProbe) y su Assert.Equal final cambiado por un
Assert.Fail que da las lineas esperadas y las reales. Clase temporal: solo va a las copias exportadas. Escribe pbt4d-probe-fullcopy.cs (\n).

Uso: python -I make_probe_fullcopy_v52.py <checkout>
"""
import hashlib
import os
import subprocess
import sys

sys.stdout.reconfigure(encoding='utf-8', newline='\n')
HERE = os.path.dirname(os.path.abspath(__file__))
raw = subprocess.run(['git', '-C', sys.argv[1], 'show', '6c2820ba:tests/RackCad.Tests/PushBackEditAllFullCopyGuardTests.cs'], capture_output=True,
                     check=True).stdout.decode('utf-8-sig').replace('\r\n', '\n')


def once(text, old, new):
    assert text.count(old) == 1, (old[:80], text.count(old))
    return text.replace(old, new, 1)


t = once(raw, '    public sealed class PushBackEditAllFullCopyGuardTests\n', '    public sealed class TmpPbt4dFullCopyProbe\n')
t = once(t, '            Assert.Equal(string.Join("\\n", expected), string.Join("\\n", log));\n',
         '            Assert.Fail("ESPERADO\\n" + string.Join("\\n", expected) + "\\nREAL\\n" + string.Join("\\n", log));\n')
out = os.path.join(HERE, 'pbt4d-probe-fullcopy.cs')
open(out, 'wb').write(t.encode('utf-8'))
print(hashlib.sha256(t.encode('utf-8')).hexdigest(), 'pbt4d-probe-fullcopy.cs')
