"""Escribe measure_pbt4_v42.py a partir de measure_pbt4_v41.py: el GREEN sale de green_pbt4a.py (con ReadAt, B-08) y se añaden
las aserciones de EIG:338-364 y las líneas nuevas de CT-PBT4A-07.

Uso: python -I make_measure_v42.py   (lee y escribe solo en esta carpeta)
"""
here = __file__.replace('make_measure_v42.py', '')
src = open(here + 'measure_pbt4_v41.py', encoding='utf-8').read()
a = src.index('# ------------------------------------------------------------------ el GREEN literal de WHC (D-PBT4-03)')
b = src.index('# ------------------------------------------------------------------ las aserciones de las guardas')
new_green = '''# ------------------------------------------------------------------ el GREEN literal (green_pbt4a.py; v4.2, con ReadAt)
import importlib.util
_spec = importlib.util.spec_from_file_location('gp', __file__.replace('measure_pbt4_v42.py', 'green_pbt4a.py'))
gp = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(gp)
_base, _green = gp.green(repo, rev)
whc_base, nh_base, ria_base = _base[WHC_P], _base[NH_P], _base[RIA_P]
whc, nh, ria = _green[WHC_P], _green[NH_P], _green[RIA_P]
REREADS = gp.REREADS

'''
src = src[:a] + new_green + src[b:]
old_ct07_start = src.index("at_origin = body_of(RIA, r'internal\\s+static\\s+RackPilotIntentReadResult\\s+AtOrigin\\s*\\(')")
old_ct07_end = src.index('SIG_RX = ')
new_ct07 = r'''at_origin = body_of(RIA, r'internal\s+static\s+RackPilotIntentReadResult\s+AtOrigin\s*\(')
check('EIG:350-364 (B-08)', 'RIA: un `new PushBackPilotIntentScan(` y un `PushBackPilotIntentReader.Read(` en todo el archivo, y el orden de `Read`',
      count(RIA, r'\bnew\s+PushBackPilotIntentScan\s*\(') == 1 and count(RIA, r'PushBackPilotIntentReader\.Read\s*\(') == 1
      and re.search(r'new\s+PushBackPilotIntentScan\s*\(\s*rackId\s*,\s*originKey\s*,\s*scan\.Facts\s*,\s*scan\.RawEnvelopes\s*,\s*CompletenessEvidence\s*,\s*witness\s*\)', read_body) is not None
      and in_order(read_body, 'string.IsNullOrWhiteSpace(witnessKey)', 'CaptureDrawing(', 'new PushBackPilotIntentScan(', 'PushBackPilotIntentReader.Read(')
      and not re.search(r'new\s+RackEmbedDocument|RackEmbedDocument|RackEmbedStore|RackEmbedComposer', RIA))
check('EIG:322-336 y :338-347', 'RIA: un `CaptureDrawing(` (y en `Read`), un `new Handle(`, sin `Handle.Parse` ni `GetObjectId(`',
      count(RIA, r'\bCaptureDrawing\s*\(') == 1 and count(read_body, r'\bCaptureDrawing\s*\(') == 1 and count(RIA, r'\bnew\s+Handle\s*\(') == 1
      and not re.search(r'Handle\.Parse|Convert\.ToInt64|\bnew\s+ObjectId\b|(?<!Try)GetObjectId\s*\(', RIA))
check('CT-PBT4A-07 (nueva)', '`AtOrigin` solo llama a `ReadAt`: sin lecturas propias, sin `CaptureDrawing`, bloqueo, transacción, literal de cadena ni `ex.Message`; RIA nombra una vez cada lector, en `Read`',
      count(at_origin, r'PushBackOriginByText\.ReadAt\s*\(') == 1
      and not re.search(r'CaptureDrawing|LockDocument|Transaction|AssertNoOpenTransaction|"|ex\.Message|PushBackPilotIntentReader|PushBackFullDraftReader|BuildStaleBase|\.Choose\s*\(|\.AtKey\s*\(', at_origin)
      and count(RIA, r'PushBackFullDraftReader\.Read\s*\(') == 1
      and count(read_body, r'PushBackFullDraftReader\.Read\s*\(\s*input\s*,\s*catalog\s*\)') == 1
      and read_body.find('PushBackPilotIntentReader.Read(input, catalog)') < read_body.find('PushBackFullDraftReader.Read(input, catalog)'))
DRAFT = _green[gp.DRAFT_P]
check('CT-PBT4A-04 y 08 (Draft)', '`PushBackOriginByText`: `Choose`, `AtKey` y `ReadAt`, con un lector de cada; ningún término de la pureza de CT-PBT3A-14',
      count(DRAFT, r'PushBackPilotIntentReader\.Read\s*\(') == 1 and count(DRAFT, r'PushBackFullDraftReader\.Read\s*\(') == 1
      and not [t for t in ["Autodesk", "RackCad.Plugin", "RackCad.UI", "System.Windows", "Transaction", "Database", "async", "await", "Task", "File.", "System.IO", "RackEmbedComposer", "new RackEmbedDocument"] if t in DRAFT]
      and re.search(r'^\s*#nullable', DRAFT, re.M) is None
      and len(re.findall(r'^namespace RackCad\.Application\.Systems\.PushBack\.Draft\s*\n\{', DRAFT, re.M)) == 1)

'''
src = src[:old_ct07_start] + new_ct07 + src[old_ct07_end:]
src = src.replace("__file__.replace('measure_pbt4_v41.py'", "__file__.replace('measure_pbt4_v42.py'")
src = src.replace('"""PB-T4, v4.1:', '"""PB-T4, v4.2 (B-08, con ReadAt):')
src = src.replace("Uso: python -I measure_pbt4_v41.py", "Uso: python -I measure_pbt4_v42.py")
open(here + 'measure_pbt4_v42.py', 'w', encoding='utf-8', newline='\n').write(src)
print('ok')
