"""PB-T4d (paquete v5.2, fase 2; OWNER-05): la sonda de la prueba lenta, sin tocarla. Mide lo que hace
FullPanelCopyWorkspacePilotPrepareGreenTests.InvalidD_ABuilderThatCannotBuild... (WorkspacePilotPrepareFull.cs:1911-1944): cuantos toques del
catalogo da un PREPARE completo (el numero de vueltas de su bucle), cuanto tarda ese PREPARE y cuanto tarda un PREPARE que falla en el toque n,
para varios n. Copia literal, de los objetos git de 6c2820ba, sus dos clases anidadas de apoyo (CountingList y CountingCatalog). Clase temporal:
solo va a las copias exportadas; acaba en Assert.Fail con sus lineas. Escribe pbt4d-probe-slow.cs (\n).

Uso: python -I make_probe_slow_v52.py <checkout>
"""
import hashlib
import os
import re
import subprocess
import sys

sys.stdout.reconfigure(encoding='utf-8', newline='\n')
HERE = os.path.dirname(os.path.abspath(__file__))
src = subprocess.run(['git', '-C', sys.argv[1], 'show', '6c2820ba:tests/RackCad.Tests/Workspace/WorkspacePilotPrepareFull.cs'], capture_output=True,
                     check=True).stdout.decode('utf-8-sig').replace('\r\n', '\n')


def nested(name):
    i = src.index('    private sealed class ' + name)
    j = src.index('{', i)
    d = 0
    for k in range(j, len(src)):
        if src[k] == '{':
            d += 1
        elif src[k] == '}':
            d -= 1
            if d == 0:
                return src[i:k + 1] + '\n'
    raise AssertionError(name)


head = src[:src.index('namespace RackCad.Tests;')]
assert 'using System.Collections.Generic;' in head
body = '''namespace RackCad.Tests;

// PB-T4d (paquete v5.2), OWNER-05: la sonda de la prueba lenta de la copia de WPP. No la toca: repite su preparacion y mide su bucle.
public sealed class TmpPbt4dSlowProbe
{
''' + nested('CountingList<T>') + '\n' + nested('CountingCatalog') + '''
    [Fact]
    public void Probe()
    {
        var lines = new List<string>();
        var scan = FullPrepareKit.RealScan();
        var read = IntentKit.Read(scan);
        var draft = FullPrepareKit.DraftOf(read);
        FullPrepareKit.SetScale(draft, 2.25);

        var sw = System.Diagnostics.Stopwatch.StartNew();
        var armed = new CountingCatalog(PilotKit.Catalog, int.MaxValue);
        var ready = FullHandKit.Prepare(PushBackFullPrepareInput.From(draft), read, armed.Catalog);
        var full = sw.Elapsed.TotalMilliseconds;
        var total = armed.Touches;
        lines.Add("prepare-completo estado=" + ready.Kind + " toques=" + total + " ms=" + full.ToString("F1", CultureInfo.InvariantCulture));

        double sum = 0;
        foreach (var frac in new[] { 0.0, 0.25, 0.5, 0.75, 1.0 })
        {
            var allowed = Math.Max(0, Math.Min(total - 1, (int)(frac * total)));
            sw.Restart();
            var failing = new CountingCatalog(PilotKit.Catalog, allowed);
            var result = FullHandKit.Prepare(PushBackFullPrepareInput.From(draft), read, failing.Catalog);
            var ms = sw.Elapsed.TotalMilliseconds;
            sum += ms;
            lines.Add("falla-en-el-toque " + allowed + " estado=" + result.Kind + " ms=" + ms.ToString("F1", CultureInfo.InvariantCulture));
        }

        lines.Add("vueltas-del-bucle=" + total + " estimacion-s=" + (total * sum / 5 / 1000).ToString("F0", CultureInfo.InvariantCulture));
        Assert.Fail(string.Join("\\n", lines));
    }
}
'''
text = head + body
out = os.path.join(HERE, 'pbt4d-probe-slow.cs')
open(out, 'wb').write(text.encode('utf-8'))
print(hashlib.sha256(text.encode('utf-8')).hexdigest(), 'pbt4d-probe-slow.cs')
