"""Revision v3: corre un sustituto (v2 o v3, SIN modificar) sobre un caso real de cases.json, con el cwd en el worktree
(como las corridas registradas) y la salida en review/runs/. Registra el tiempo y el codigo de salida.
Uso: python -I -B run_case.py <worktree> <cases.json> <caso> <script.py> <etiqueta> <dir-salida> [timeout-s]"""
import json, os, subprocess, sys, time

wt, cases, case, script, label, outdir = sys.argv[1:7]
timeout = int(sys.argv[7]) if len(sys.argv) > 7 else 1500
c = json.load(open(cases, encoding='utf-8'))[case]
os.makedirs(outdir, exist_ok=True)
out = os.path.join(outdir, '%s-%s.json' % (case, label)).replace(os.sep, '/')
cmd = [sys.executable, '-I', '-B', script, '--task', c['task'], c['real'], c['nc2'], c['base'], c['cur'], out, c['note'], c['raw']]
t0 = time.time()
try:
    r = subprocess.run(cmd, cwd=wt, capture_output=True, text=True, encoding='utf-8', timeout=timeout)
    rc, so, se = r.returncode, r.stdout, r.stderr
except subprocess.TimeoutExpired as ex:
    rc, so, se = 'TIMEOUT', (ex.stdout or b'').decode('utf-8', 'replace') if isinstance(ex.stdout, bytes) else (ex.stdout or ''), 'timeout %ds' % timeout
dt = time.time() - t0
open(os.path.join(outdir, '%s-%s.txt' % (case, label)), 'w', encoding='utf-8', newline='\n').write(
    'cmd: %s\ncwd: %s\nrc: %s\nseconds: %.1f\n--- stdout\n%s\n--- stderr\n%s\n' % (json.dumps(cmd[1:], ensure_ascii=False), wt, rc, dt, so, se))
print(case, label, 'rc', rc, 'seconds %.1f' % dt, 'stderr:', (se or '').strip()[:300])
