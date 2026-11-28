"""Cierre del bloque M (anexo §3): SHA de originales y copias, autoguardados de las copias, variables RACKCAD_DEBUG_*.
Solo lectura. Imprime sin rutas de originales (solo rol y SHA)."""
import glob, hashlib, json, os, subprocess, tempfile


def sha(p):
    h = hashlib.sha256()
    with open(p, 'rb') as f:
        for b in iter(lambda: f.read(1 << 20), b''):
            h.update(b)
    return h.hexdigest().upper()


src = json.load(open('<registro-de-copias-de-Smoke-2>/copias.json', encoding='utf-8-sig'))
rec = json.load(open('<carpeta-de-copias-de-PB-T0>/copias.json', encoding='utf-8'))
want = {'D1': '7A868624226EE956CC411CAAA54EA2DE6B95849EEE946995917CFF70FB9F47A5',
        'D2': 'F865183291263C6AC5578682566743E543E59410C875FB76110833E190B4AF10'}
ok = True
for role, w in want.items():
    paths = [o['Source'] for o in src['Originals'] if o['Sha256Before'] == w]
    s = sha(paths[0])
    print('ORIGINAL', role, s == w, s[:12])
    ok &= s == w
for c in rec['Copies']:
    p = '<carpeta-de-copias-de-PB-T0>/' + c['Copy']
    s = sha(p)
    good = s == want[c['Role']]
    print('COPIA', c['Copy'], c['Role'], 'igual al original' if good else 'DIFERENTE', s[:12])
    ok &= good
extra = sorted(os.path.basename(p) for p in glob.glob('<carpeta-de-copias-de-PB-T0>/*') if os.path.isfile(p) and not p.endswith(('.dwg', '.json')))
print('OTROS ARCHIVOS EN LA CARPETA DE COPIAS:', extra)
tmp = tempfile.gettempdir()
sv = sorted(os.path.basename(p) for p in glob.glob(os.path.join(tmp, 'M*_*.sv$')))
print('AUTOGUARDADOS .sv$ DE LAS COPIAS EN TEMP:', sv)
env = {k: v for k, v in os.environ.items() if k.upper().startswith('RACKCAD_DEBUG')}
print('RACKCAD_DEBUG_* en el entorno:', env)
for scope in ('User', 'Machine'):
    out = subprocess.run(['powershell', '-NoProfile', '-Command',
                          "[Environment]::GetEnvironmentVariables('%s').Keys | Where-Object { $_ -like 'RACKCAD_DEBUG*' }" % scope],
                         capture_output=True, text=True).stdout.strip()
    print('RACKCAD_DEBUG_* en', scope + ':', repr(out))
acad = subprocess.run(['powershell', '-NoProfile', '-Command', "(Get-Process acad -ErrorAction SilentlyContinue | Measure-Object).Count"],
                      capture_output=True, text=True).stdout.strip()
print('PROCESOS acad:', acad)
print('RESULTADO SHA:', 'PASS' if ok else 'FAIL')
