"""PB-T4c, precondicion 6 (paquete v5.0; D-S8): los filtros de RequiredTests frente a los nombres completos de las clases. Por texto, como
`dotnet test --filter FullyQualifiedName~X` (subcadena del nombre completo <espacio>.<Clase>.<metodo>):
- cada filtro nuevo de PB-T4c selecciona solo su clase;
- ningun filtro heredado del contrato de PB-T4b selecciona una clase nueva de PB-T4c;
- y, por filtro heredado de Core, las clases que selecciona de las de la lista de 3 (PEG, PushBackSharedReferenceGuardTests y
  WorkspaceFullCopyGuardTests), que bajan su MinSelected en una por cada prueba de la lista que seleccionan.
Las clases existentes salen de las TRX de la suite completa de Core y de los filtros de UI. Solo lee; escribe la salida en UTF-8 con LF.
Uso: python -I filter_overlap_pbt4c_v50.py <contrato-pbt4b.txt> <salida.txt> <trx> [<trx> ...]
"""
import sys
import xml.etree.ElementTree as ET

NS = {'t': 'http://microsoft.com/schemas/VisualStudio/TeamTest/2010'}
dump, out = sys.argv[1], sys.argv[2]
trxs = sys.argv[3:]
NEW = {
    'FullyQualifiedName~RackCad.Tests.PushBackEditAllTests': 'RackCad.Tests.PushBackEditAllTests',
    'FullyQualifiedName~RackCad.Tests.PushBackEditAllGuardTests': 'RackCad.Tests.PushBackEditAllGuardTests',
    'FullyQualifiedName~RackCad.Tests.PushBackOpenerClassicPathGuardTests': 'RackCad.Tests.PushBackOpenerClassicPathGuardTests',
    'FullyQualifiedName~RackCad.Tests.PushBackDraftLaunchGuardTests': 'RackCad.Tests.PushBackDraftLaunchGuardTests',
    'FullyQualifiedName~RackCad.UI.Tests.PushBackEditAllWindowTests': 'RackCad.UI.Tests.PushBackEditAllWindowTests',
    'FullyQualifiedName~RackCad.Tests.PushBackEditAllPortEquivalenceGuardTests': 'RackCad.Tests.PushBackEditAllPortEquivalenceGuardTests',
    'FullyQualifiedName~RackCad.Tests.PushBackEditAllNameWritersGuardTests': 'RackCad.Tests.PushBackEditAllNameWritersGuardTests',
    'FullyQualifiedName~RackCad.Tests.PushBackEditAllFullCopyGuardTests': 'RackCad.Tests.PushBackEditAllFullCopyGuardTests',
}
LISTED = ['RackCad.Tests.WorkspacePilotEditGuardTests', 'RackCad.Tests.PushBackSharedReferenceGuardTests', 'RackCad.Tests.WorkspaceFullCopyGuardTests']
classes = set()
for t in trxs:
    root = ET.parse(t).getroot()
    for ut in root.iterfind('.//t:TestDefinitions/t:UnitTest', NS):
        classes.add(ut.find('t:TestMethod', NS).get('className'))
all_classes = sorted(classes | set(NEW.values()))
inherited = []
for line in open(dump, encoding='utf-8').read().split('\n'):
    c = line.split('\t')
    if len(c) == 5 and c[0].isdigit():
        inherited.append((int(c[0]), c[2], c[3], c[4]))


def selects(flt, cls):
    return flt.split('~', 1)[1] in cls + '.'


lines = ['clases existentes (de las TRX): %d; nuevas: %d' % (len(classes), len(NEW))]
ok = True
for flt, own in NEW.items():
    sel = [c for c in all_classes if selects(flt, c)]
    good = sel == [own]
    ok = ok and good
    lines.append('nuevo %s selecciona %s %s' % (flt, sel, 'OK' if good else 'DISTINTO'))
for n, flt, minsel, er in inherited:
    sel_new = [c for c in NEW.values() if selects(flt, c)]
    sel_listed = [c for c in LISTED if selects(flt, c)]
    if sel_new:
        ok = False
    lines.append('heredado %d %s MinSelected-PB-T4b=%s ExpectRed-PB-T4b=%s clases-nuevas=%s clases-de-la-lista=%s'
                 % (n, flt, minsel, er, sel_new, [c.split('.')[-1] for c in sel_listed]))
lines.append('filtros: %s' % ('PASA' if ok else 'PARA'))
with open(out, 'w', encoding='utf-8', newline='\n') as f:
    f.write('\n'.join(lines) + '\n')
print(lines[-1])
