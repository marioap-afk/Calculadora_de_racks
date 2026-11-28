"""Evaluacion de la sonda con el binario nuevo: criterios de eval_probe_f2t1.py, mas la comparacion del contexto efectivo con el de nc2 b454 (Snapshot D y binario acreditado).
Uso: python eval_probe_newbin.py <RunId>"""
import json, os, sys, glob, hashlib, re, subprocess
WT = '<worktree-de-A-14>'  # la sonda corre en el worktree lateral de A-14 (aqui solo se lee)
os.chdir(WT)
R = os.path.join(os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__)))), 'a14r')
RID = sys.argv[1]
D = os.path.join(R, 'F4-ENVIRONMENT', f'probe-{RID}').replace(chr(92), '/')
B454 = os.path.join(os.environ['USERPROFILE'], '.codex', 'worktrees', 'architecture-workspace-persistente-rackcad', 'artifacts', 'orchestration', 'I-64', 'F4-ENVIRONMENT', 'probe-R20261122T172638Z-2c14').replace(chr(92), '/')  # referencia: la sonda neutra 2c14 de la Snapshot L (mismo binario, mismo prompt y esquema, 2026-11-22); solo se lee
CLI = '0.167.0-alpha.3.2'
NEW_SHA = '4F91AE86895FF124760D70ED1630E2354CA874765C9BB61AE524B600327EFC17'
sx, se = json.load(open(f'{D}/state-exit.json')), json.load(open(f'{D}/state-entry.json'))
g = lambda *a: subprocess.run(['git', '--no-optional-locks'] + list(a), capture_output=True, text=True).stdout.strip()
ORACLE = {'HeadSha': g('rev-parse', 'HEAD'), 'OriginMainSha': g('rev-parse', 'refs/remotes/origin/main'), 'StateBlob': g('rev-parse', 'HEAD:docs/automation/state/I-64.yml')}
sha = lambda p: hashlib.sha256(open(p, 'rb').read()).hexdigest().upper()


def session(events):
    ev = [json.loads(l) for l in open(events, encoding='utf-8') if l.strip()]
    tid = [e['thread_id'] for e in ev if e.get('type') == 'thread.started']
    f = glob.glob(os.path.expanduser(f"~/.codex/sessions/**/*{tid[0]}.jsonl"), recursive=True)[0]
    tc, meta = None, None
    for l in open(f, encoding='utf-8'):
        j = json.loads(l)
        if j.get('type') == 'turn_context' and tc is None:
            tc = j['payload']
        if j.get('type') == 'session_meta':
            meta = j['payload']
    return ev, tid[0], f, tc, meta


res = {'RunId': RID, 'ExitCode': int(open(f'{D}/exit-code.txt').read().strip())}
ev, tid, f, tc, meta = session(f'{D}/events.jsonl')
types = [e.get('type') for e in ev]
res['TerminalEvent'] = 'turn.completed' if 'turn.completed' in types else ('turn.failed' if 'turn.failed' in types else None)
res['ThreadId'] = tid
res['Usage'] = ([e['usage'] for e in ev if e.get('type') == 'turn.completed'] or [None])[0]
cmds = [e['item'] for e in ev if e.get('type') == 'item.completed' and e.get('item', {}).get('type') == 'command_execution']
res['Commands'] = [{'Command': ' '.join((c.get('command') or '').split())[-120:], 'Exit': c.get('exit_code'), 'Output': ' '.join((c.get('aggregated_output') or '').split())[-60:]} for c in cmds]
out = f'{D}/output.json'
res['OutputPresent'] = os.path.exists(out) and os.path.getsize(out) > 0
if res['OutputPresent']:
    o = subprocess.run(['pwsh', '-NoProfile', '-Command', f"Get-Content -Raw '{out}' | Test-Json -SchemaFile '{D}/probe.schema.json'"], capture_output=True, text=True)
    res['SchemaValid'] = o.stdout.strip().endswith('True')
    v = json.load(open(out, encoding='utf-8'))
    res['OracleMatch'] = {k: v.get(k) == ORACLE[k] for k in ORACLE}
    res['OutputSha256'] = sha(out)
else:
    res['SchemaValid'], res['OracleMatch'] = False, None
res['Oracle'] = ORACLE
res['CliVersion'] = meta.get('cli_version')
res['Effective'] = (tc.get('model'), tc.get('effort'))
res['Sandbox'] = tc.get('sandbox_policy')
res['PermissionProfile'] = tc.get('permission_profile')
res['ApprovalPolicy'] = tc.get('approval_policy')
res['EffectiveSource'] = os.path.basename(f)
res['EffectiveSourceSha256'] = sha(f)
_, _, _, tc0, meta0 = session(f'{B454}/events.jsonl')
IGN = {'turn_id', 'cwd', 'workspace_roots', 'current_date', 'timezone', 'trace_id', 'thread_id', 'multi_agent_version'}
keys = sorted((set(tc) | set(tc0)) - IGN)
scrub = lambda x: json.loads(json.dumps(x).replace(os.path.basename(os.environ['USERPROFILE']), '<USER>'))
res['TurnContextVsReference'] = {'ReferenceRun': 'R20261122T172638Z-2c14 (sonda neutra de la Snapshot L, mismo binario, mismo prompt y esquema, desde el worktree de I-64)', 'FieldsCompared': keys, 'Differences': {k: {'Reference': scrub(tc0.get(k)), 'Probe': scrub(tc.get(k))} for k in keys if tc0.get(k) != tc.get(k)}}
res['SessionMetaKeysDiffer'] = sorted(set(meta) ^ set(meta0))
txt = open(f'{D}/events.jsonl', encoding='utf-8').read()
lim = {}
for pat in ['rate limit', 'usage limit', 'limit reached', 'credits', 'quota']:
    for m in re.finditer(pat, txt, re.I):
        k = txt[max(0, m.start() - 50):m.end() + 10].replace('\\n', ' ')
        lim[k] = lim.get(k, 0) + 1
res['LimitPatternContexts'] = lim
res['EventsSha256'] = sha(f'{D}/events.jsonl')
res['StderrBytes'] = os.path.getsize(f'{D}/stderr.txt')
res['StderrText'] = open(f'{D}/stderr.txt', encoding='utf-8', errors='replace').read()[:400]
fs = ((res.get('PermissionProfile') or {}).get('file_system') or {})
res['BinaryAndState'] = {'BinarySha256Exit': sx['BinarySha256'], 'BinarySha256Entry': se['BinarySha256'], 'IsNewBinary': sx['BinarySha256'] == se['BinarySha256'] == NEW_SHA, 'CodexExesUnchanged': sx['CodexExes'] == se['CodexExes'], 'RealUnchanged': sx['Real'] == se['Real'], 'ConfigUnchanged': sx['ConfigSha256'] == se['ConfigSha256'], 'ConfigSha256': se['ConfigSha256']}
bs = res['BinaryAndState']
res['Pass'] = bool(res['ExitCode'] == 0 and res['TerminalEvent'] == 'turn.completed' and res['SchemaValid'] and res['OracleMatch'] and all(res['OracleMatch'].values()) and res['Effective'] == ('gpt-6-luna', 'high') and not lim
                   and (res.get('Sandbox') or {}).get('type') == 'read-only' and fs.get('entries') and all(x.get('access') == 'read' for x in fs.get('entries')) and (res.get('PermissionProfile') or {}).get('network') == 'restricted'
                   and res['CliVersion'] == CLI and all(c.get('exit_code') == 0 for c in cmds) and bs['IsNewBinary'] and bs['CodexExesUnchanged'] and bs['RealUnchanged'] and bs['ConfigUnchanged'])
json.dump(res, open(f'{D}/probe-result.json', 'w', encoding='utf-8', newline='\n'), indent=2, ensure_ascii=False)
print(json.dumps({k: res[k] for k in res if k not in ('PermissionProfile', 'Usage')}, indent=1, ensure_ascii=True)[:3600])
print('permission', json.dumps(res['PermissionProfile'])[:300])
