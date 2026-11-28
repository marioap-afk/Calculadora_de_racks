"""Solo lectura: comprueba en el registro de sesion de Codex de la sonda que su cwd y sus workspace_roots son el worktree lateral de
A-14 (los dos campos que eval_probe_m.py no compara, como en la L). No imprime rutas. Uso: python -I probe_cwd_m.py <RunId>"""
import glob, json, os, sys
RID = sys.argv[1]
R = os.path.join(os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__)))), 'a14r')
res = json.load(open(os.path.join(R, 'F4-ENVIRONMENT', 'probe-' + RID, 'probe-result.json'), encoding='utf-8'))
f = glob.glob(os.path.expanduser('~/.codex/sessions/**/*' + res['ThreadId'] + '.jsonl'), recursive=True)[0]
tc = next(json.loads(l)['payload'] for l in open(f, encoding='utf-8') if l.strip() and json.loads(l).get('type') == 'turn_context')
norm = lambda p: os.path.normcase(os.path.normpath(p)) if isinstance(p, str) else p
want = norm('<worktree-de-A-14>')
roots = tc.get('workspace_roots')
roots = [norm(x if isinstance(x, str) else (x or {}).get('path')) for x in (roots or [])] if isinstance(roots, list) else roots
print(json.dumps({'CwdIsLateralWorktree': norm(tc.get('cwd')) == want, 'WorkspaceRootsAreLateralWorktree': roots == [want] if isinstance(roots, list) else None,
                  'WorkspaceRootsType': type(tc.get('workspace_roots')).__name__}))
