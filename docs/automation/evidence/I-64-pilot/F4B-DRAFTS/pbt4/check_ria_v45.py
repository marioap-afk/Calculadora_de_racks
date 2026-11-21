"""PB-T4b (paquete v4.5): las guardas del Plugin sobre el texto literal de RIA, NH y PushBackOriginByText.cs, en el RED (el GREEN de PB-T4a)
y en el GREEN de la v4.5 (green_pbt4b_v45.py). Solo lee objetos git y los archivos de hs65; escribe solo su salida.

Uso: python -I check_ria_v45.py <checkout> <rev> <salida.txt> <PushBackFullReadPathTests.cs del GREEN de PB-T4a>

1. CT-PBT4A-07 (no está en la lista de 475): su método `Lines` y sus cinco autopruebas, traducidos de la clase real del GREEN de PB-T4a
   (hs65/pbt4acc/irc-work/f/green/PushBackFullReadPathTests.cs, que se lee y se comprueba que sigue diciendo lo que se traduce). Tiene que dar
   sus 13 líneas, iguales, sobre el GREEN de la v4.5.
2. CT-PBT4B-21 (nueva, D-S4, D-PBT4-21): sus líneas y sus cinco autopruebas, con el texto que prescribe §5.2; en el RED cae (por aserción) y en el
   GREEN da sus líneas.
3. Las cuentas de las guardas que leen RIA y que el recorrido no ve por literal: B-08 (EIG:350-364), CAG (los literales y los ex.Message de
   Read), G-9 (StartTransaction y LockDocument) e INV-14 (AssertNoOpenTransaction).
"""
import importlib.util
import os
import re
import sys

HERE = os.path.dirname(os.path.abspath(__file__))
spec = importlib.util.spec_from_file_location('gb45', os.path.join(HERE, 'green_pbt4b_v45.py'))
gb45 = importlib.util.module_from_spec(spec)
spec.loader.exec_module(gb45)
repo, rev, out_path, ct07_path = sys.argv[1:5]


def code_only(text):
    return re.sub(r'//[^\r\n]*', '', re.sub(r'/\*.*?\*/', '', text, flags=re.S))


def count(text, pattern):
    return len(re.findall(pattern, text))


def body_or_empty(code, signature):
    m = re.search(signature, code)
    if not m:
        return ''
    open_ = code.find('{', m.end() - 1)
    if open_ < 0:
        return ''
    depth = 0
    for i in range(open_, len(code)):
        if code[i] == '{':
            depth += 1
        elif code[i] == '}':
            depth -= 1
            if depth == 0:
                return code[m.start():i + 1]
    return ''


# ---------------------------------------------------------------- CT-PBT4A-07, traducida de la clase real
ct07 = open(ct07_path, encoding='utf-8').read().replace('\r\n', '\n')
for needle in ('var withoutFull = Regex.Replace(adapter, @"PushBackFullDraftReader\\.Read\\s*\\(", "PushBackOtherReader.Read(");',
               '&& Regex.IsMatch(read, @"PushBackFullDraftReader\\.Read\\s*\\(\\s*input\\s*,\\s*catalog\\s*\\)");',
               '"ria-Read completo=" + Count(read, fullRead) + " tras-el-piloto=" + afterThePilot,',
               '+ " base=" + Count(atOrigin, @"StaleBase")',
               'var locked = BodyOrEmpty(active, @"using\\s*\\(\\s*document\\.LockDocument\\s*\\(\\s*\\)\\s*\\)");'):
    assert ct07.count(needle) == 1, 'CT-PBT4A-07 ya no dice: ' + needle
SIGNATURE_OF_READ = (r'internal\s+static\s+RackPilotIntentReadResult\s+Read\s*\(\s*Document\s+document\s*,\s*string\s+rackId\s*,\s*long\s+originDefinitionHandle'
                     r'\s*,\s*string\??\s+witnessKey\s*,\s*string\??\s+witnessText\s*,\s*RackCatalog\s+catalog\s*\)')
PILOT_READ = r'PushBackPilotIntentReader\.Read\s*\('
FULL_READ = r'PushBackFullDraftReader\.Read\s*\('


def lines07(adapter, host):
    read = body_or_empty(adapter, r'internal\s+static\s+RackPilotIntentReadResult\s+Read\s*\(')
    at_origin = body_or_empty(adapter, r'internal\s+static\s+RackPilotIntentReadResult\s+AtOrigin\s*\(')
    p = re.search(PILOT_READ, read)
    f = re.search(FULL_READ, read)
    after = bool(p and f and f.start() > p.start() and re.search(r'PushBackFullDraftReader\.Read\s*\(\s*input\s*,\s*catalog\s*\)', read))
    lines = [
        'ria-firma-de-Read-sin-cambio=' + str(bool(re.search(SIGNATURE_OF_READ, adapter))),
        'ria-Read completo=%d tras-el-piloto=%s' % (count(read, FULL_READ), after),
        'ria-lectores piloto=%d completo=%d escaneos=%d' % (count(adapter, PILOT_READ), count(adapter, FULL_READ), count(adapter, r'\bnew\s+PushBackPilotIntentScan\s*\(')),
        'ria-AtOrigin readat=%d piloto=%d completo=%d base=%d lecturas-del-dibujo=%d literales=%d' % (
            count(at_origin, r'PushBackOriginByText\.ReadAt\s*\('), count(at_origin, PILOT_READ), count(at_origin, FULL_READ), count(at_origin, 'StaleBase'),
            count(at_origin, r'CaptureDrawing|LockDocument|Transaction|ex\.Message'), count(at_origin, r'"(?:\\.|[^"\\])*"')),
    ]
    decl = re.search(r'private\s+sealed\s+class\s+PilotIntentReader\s*:\s*([^{]*)\{', host)
    lines.append('nh-lector interfaces=' + (','.join(n.strip() for n in decl.group(1).split(',')) if decl else ''))
    reread = body_or_empty(host, r'public\s+EditIntentReadResult\s+ReadIntentAtOrigin\s*\(')
    lines.append('nh-ReadIntentAtOrigin ReadOnActiveDocument=%d' % count(reread, r'\bReadOnActiveDocument\s*\('))
    call = r'RackPilotIntentAdapter\.AtOrigin\s*\('
    active = body_or_empty(host, r'private\s+EditIntentReadResult\s+ReadOnActiveDocument\s*\(')
    locked = body_or_empty(active, r'using\s*\(\s*document\.LockDocument\s*\(\s*\)\s*\)')
    read_at = active.find('RackPilotIntentAdapter.Read(')
    origin_at = re.search(call, active)
    lines.append('nh-AtOrigin=%d dentro-del-bloqueo=%s tras-Read=%s' % (count(host, call), count(locked, call) == 1, read_at >= 0 and bool(origin_at) and origin_at.start() > read_at))
    lines.append('nh-resultado FromReads=%d FromPilot=%d' % (count(host, r'EditIntentReadResult\.FromReads\s*\('), count(host, r'EditIntentReadResult\.FromPilot\s*\(')))
    return lines


def run07(adapter, host):
    real = lines07(adapter, host)
    log = list(real)
    log.append('autoprueba-sin-lector-completo=' + ('pasa' if lines07(re.sub(FULL_READ, 'PushBackOtherReader.Read(', adapter), host) == real else 'falla'))
    with_drawing = re.sub(r'(internal\s+static\s+RackPilotIntentReadResult\s+AtOrigin\s*\([^)]*\)\s*\{)', r'\1 var injected = RackSiblingScan.CaptureDrawing(null);', adapter)
    log.append('autoprueba-AtOrigin-con-CaptureDrawing=' + ('pasa' if lines07(with_drawing, host) == real else 'falla'))
    log.append('autoprueba-segundo-AtOrigin=' + ('pasa' if lines07(adapter, host + '\nread = RackPilotIntentAdapter.AtOrigin(read, originText, catalog.Catalog);\n') == real else 'falla'))
    log.append('autoprueba-FromPilot-en-NH=' + ('pasa' if lines07(adapter, host + '\nvar injected = EditIntentReadResult.FromPilot(read.Pilot);\n') == real else 'falla'))
    log.append('autoprueba-segunda-lectura-del-piloto-en-RIA=' + ('pasa' if lines07(adapter + '\nvar injected = PushBackPilotIntentReader.Read(input, catalog);\n', host) == real else 'falla'))
    return log


EXPECTED07 = [
    'ria-firma-de-Read-sin-cambio=True', 'ria-Read completo=1 tras-el-piloto=True', 'ria-lectores piloto=1 completo=1 escaneos=1',
    'ria-AtOrigin readat=1 piloto=0 completo=0 base=0 lecturas-del-dibujo=0 literales=0',
    'nh-lector interfaces=IWorkspaceEditIntentReader,IWorkspaceEditIntentOriginReader', 'nh-ReadIntentAtOrigin ReadOnActiveDocument=1',
    'nh-AtOrigin=1 dentro-del-bloqueo=True tras-Read=True', 'nh-resultado FromReads=1 FromPilot=0', 'autoprueba-sin-lector-completo=falla',
    'autoprueba-AtOrigin-con-CaptureDrawing=falla', 'autoprueba-segundo-AtOrigin=falla', 'autoprueba-FromPilot-en-NH=falla',
    'autoprueba-segunda-lectura-del-piloto-en-RIA=falla']

# ---------------------------------------------------------------- CT-PBT4B-21 (D-S4, D-PBT4-21), el texto de §5.2
DEFERRED = r'new\s+System\.Lazy<PushBackFullIntentResult>\s*\(\s*\(\s*\)\s*=>\s*PushBackFullDraftReader\.Read\s*\(\s*input\s*,\s*catalog\s*\)\s*\)'
PRIMARY_FULL = r'\bread\s*\.\s*Full\b'


def lines21(adapter, host, origin):
    read = body_or_empty(adapter, r'internal\s+static\s+RackPilotIntentReadResult\s+Read\s*\(')
    at_origin = body_or_empty(adapter, r'internal\s+static\s+RackPilotIntentReadResult\s+AtOrigin\s*\(')
    result = body_or_empty(adapter, r'internal\s+sealed\s+class\s+RackPilotIntentReadResult\b')
    active = body_or_empty(host, r'private\s+EditIntentReadResult\s+ReadOnActiveDocument\s*\(')
    locked = body_or_empty(active, r'using\s*\(\s*document\.LockDocument\s*\(\s*\)\s*\)')
    full_at = re.search(PRIMARY_FULL, active)
    origin_call = re.search(r'RackPilotIntentAdapter\.AtOrigin\s*\(', active)
    read_at = body_or_empty(origin, r'public\s+static\s+\([^)]*\)\?\s+ReadAt\s*\(')
    return [
        'ria-Read completo=%d diferido=%d' % (count(read, FULL_READ), count(read, DEFERRED)),
        'ria-resultado Full-diferido=%d campo=%d de-un-valor=%d de-una-lectura-diferida=%d' % (
            count(result, r'internal\s+PushBackFullIntentResult\?\s+Full\s*=>\s*_full\s*\?\.\s*Value\s*;'),
            count(result, r'private\s+readonly\s+System\.Lazy<PushBackFullIntentResult>\?\s+_full\s*;'),
            count(result, r'_full\s*=\s*new\s+System\.Lazy<PushBackFullIntentResult>\s*\(\s*full\s*\)\s*;'),
            count(result, r'_full\s*=\s*full\s*;')),
        'ria-AtOrigin Full-del-Primary=%d' % count(at_origin, PRIMARY_FULL),
        'nh-Full=%d dentro-del-bloqueo=%s tras-AtOrigin=%s' % (count(host, PRIMARY_FULL), count(locked, PRIMARY_FULL) == 1,
                                                               bool(full_at and origin_call and full_at.start() > origin_call.start())),
        'readat completo=%d' % count(read_at, FULL_READ),
    ]


def run21(adapter, host, origin):
    real = lines21(adapter, host, origin)
    log = list(real)
    eager = re.sub(DEFERRED, 'PushBackFullDraftReader.Read(input, catalog)', adapter)
    log.append('autoprueba-completo-inmediato=' + ('pasa' if lines21(eager, host, origin) == real else 'falla'))
    in_origin = re.sub(r'(internal\s+static\s+RackPilotIntentReadResult\s+AtOrigin\s*\([^)]*\)\s*\{)', r'\1 var injected = read.Full;', adapter)
    log.append('autoprueba-Full-del-Primary-en-AtOrigin=' + ('pasa' if lines21(in_origin, host, origin) == real else 'falla'))
    log.append('autoprueba-segundo-Full-en-NH=' + ('pasa' if lines21(adapter, host + '\nvar injected = read.Full;\n', origin) == real else 'falla'))
    stored = re.sub(r'internal\s+PushBackFullIntentResult\?\s+Full\s*=>\s*_full\s*\?\.\s*Value\s*;', 'internal PushBackFullIntentResult? Full { get; }', adapter)
    log.append('autoprueba-Full-sin-diferir=' + ('pasa' if lines21(stored, host, origin) == real else 'falla'))
    twice = re.sub(r'(var\s+input\s*=\s*AtKey\s*\(\s*scan\s*,\s*key\s*\)\s*;)', r'\1 var injected = PushBackFullDraftReader.Read(input, catalog);', origin)
    log.append('autoprueba-segundo-completo-en-ReadAt=' + ('pasa' if lines21(adapter, host, twice) == real else 'falla'))
    return log


EXPECTED21 = [
    'ria-Read completo=1 diferido=1', 'ria-resultado Full-diferido=1 campo=1 de-un-valor=1 de-una-lectura-diferida=1', 'ria-AtOrigin Full-del-Primary=0',
    'nh-Full=1 dentro-del-bloqueo=True tras-AtOrigin=True', 'readat completo=1', 'autoprueba-completo-inmediato=falla',
    'autoprueba-Full-del-Primary-en-AtOrigin=falla', 'autoprueba-segundo-Full-en-NH=falla', 'autoprueba-Full-sin-diferir=falla',
    'autoprueba-segundo-completo-en-ReadAt=falla']


def counts(adapter):
    read = body_or_empty(adapter, r'internal\s+static\s+RackPilotIntentReadResult\s+Read\s*\(')
    return ('b08 escaneos=%d piloto=%d completo=%d en-Read=%d,%d,%d orden=%s | cag literales-de-Read=%d ex.Message-de-Read=%d | g9 StartTransaction=%d LockDocument=%d | '
            'inv14 AssertNoOpenTransaction-de-Read=%d | publico=%d' % (
                count(adapter, r'\bnew\s+PushBackPilotIntentScan\s*\('), count(adapter, PILOT_READ), count(adapter, FULL_READ),
                count(read, r'\bnew\s+PushBackPilotIntentScan\s*\('), count(read, PILOT_READ), count(read, FULL_READ),
                bool(re.search(r'new\s+PushBackPilotIntentScan\s*\([\s\S]*PushBackPilotIntentReader\.Read\s*\([\s\S]*PushBackFullDraftReader\.Read\s*\(', read)),
                count(read, r'"(?:\\.|[^"\\])*"'), count(read, r'ex\.Message'), count(adapter, r'StartTransaction\s*\('), count(adapter, r'LockDocument\s*\('),
                count(read, r'AssertNoOpenTransaction\s*\('), count(adapter, r'\bpublic\b')))


sys.stdout.reconfigure(encoding='utf-8', errors='replace')
plugin = gb45.t4a_plugin(repo, rev)
red_ria = plugin[gb45.RIA_P]
green_ria = gb45.ria_green(red_ria)
host = code_only(plugin[gb45.NH_P])
origin = code_only(plugin[gb45.DRAFT_ORIGIN_P])
out = []
for side, ria in (('RED (RIA del GREEN de PB-T4a)', red_ria), ('GREEN v4.5', green_ria)):
    adapter = code_only(ria)
    l07 = run07(adapter, host)
    l21 = run21(adapter, host, origin)
    out.append('== ' + side)
    out.append('CT-PBT4A-07: ' + ('IGUAL a sus 13 lineas' if l07 == EXPECTED07 else 'DISTINTA: ' + ' | '.join(l07)))
    out.append('CT-PBT4B-21: ' + ('IGUAL a sus 10 lineas' if l21 == EXPECTED21 else 'DISTINTA (cae por asercion): ' + ' | '.join(l21)))
    out.append('cuentas: ' + counts(adapter))
    if side.startswith('GREEN'):
        assert l07 == EXPECTED07, l07
        assert l21 == EXPECTED21, l21
    else:
        assert l07 == EXPECTED07 and l21 != EXPECTED21
assert counts(code_only(red_ria)).split(' | publico')[0] == counts(code_only(green_ria)).split(' | publico')[0]
out.append('las cuentas de B-08, CAG, G-9 e INV-14 son iguales en el RED y en el GREEN')
with open(out_path, 'w', encoding='utf-8', newline='\n') as f:
    f.write('\n'.join(out) + '\n')
print('\n'.join(out))
