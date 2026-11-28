"""Deriva la sonda neutra de la Snapshot M desde la de la L (hs65/snapL), con reemplazos exactos (decision del Owner, 2026-11-23,
quinta ronda de A-14: Sonda neutra, luego M). Diferencias de fondo frente a la L:
- la sonda corre en el worktree lateral de A-14 (el directorio de la revision del Architect), no en el de I-64;
- nada se escribe en ese worktree: la carpeta de la corrida y el registro van a la carpeta de la sesion (a14r/), fuera del repositorio;
- la referencia del contexto efectivo es la sonda 2c14 de la Snapshot L (mismo binario, mismo prompt y esquema);
- los participantes se miden contra el worktree lateral (proc_check_m.ps1, copia de proc_check4.ps1 con solo $wt cambiado);
- Python solo con python -I (sin python -c)."""
import os, ast
BS = chr(92)
H = os.path.dirname(os.path.abspath(__file__))
SL = os.path.join(os.path.dirname(H), 'snapL')
F2 = os.path.join(os.path.dirname(H), 'f2t1')
K_SHA = 'E1F69847610FD810D5286B82C86E00FE85D9F497F89E9FD9645E0EEA113322FE'
L_SHA = 'B71650A637551747F522BB1D2F4BA1BB9E25AAFF1A61CE1B7BC066197FFFEBB3'
M_SHA = '3E9BC35B0E72641A81241EC3E32BF45010B8CC3CC93FDD4E3C5E8188E6D19FFC'
NAMES = '3ECF8E072CC52AE60A6E6D508DB54DCBFFFAFB04EAF9B52905A2CEAB0E0812E3'
WT_WIN = BS.join(['C:', 'Users', '<usuario>', '.codex', 'worktrees', 'architecture-workspace-persistente-rackcad'])
WT_POSIX = '<worktree-de-I-64>'
RUNS_PY = "os.path.join(os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__)))), 'a14r')"
rd = lambda d, n: open(os.path.join(d, n), encoding='utf-8', newline='').read()


def wr(n, t):
    if n.endswith('.py'):
        ast.parse(t)
    p = os.path.join(H, n)
    assert not os.path.exists(p), n
    open(p, 'w', encoding='utf-8', newline='').write(t)


def rep(t, a, b, n=1):
    assert t.count(a) == n, (a[:100], t.count(a))
    return t.replace(a, b)


def no_l(n, t):
    for bad in ('_l.', 'runid-l', 'snapL', WT_POSIX, WT_WIN, 'artifacts/orchestration'):
        assert bad not in t, (n, bad)


# cfg_check: solo acepta la M; los mismos 105 nombres que la L
t = rd(SL, 'cfg_check_l.py')
t = rep(t, "OK = {'" + L_SHA + "': 'L'}  # solo para la sonda de la L", "OK = {'" + M_SHA + "': 'M'}  # solo para la sonda de la M")
t = rep(t, "NAMES = '" + NAMES + "'  # la L: 105 nombres (se anade mcp_servers.node_repl.env.NODE_REPL_LAZY_TRUSTED_WORKER)",
        "NAMES = '" + NAMES + "'  # la M: los mismos 105 nombres que la L")
wr('cfg_check_m.py', t)

# make_probe: mismo prompt y esquema; la carpeta de la corrida, en la de la sesion
t = rd(SL, 'make_probe_l.py')
t = rep(t, "WT = r'" + WT_WIN + "'\nos.chdir(WT)\n",
        "R = " + RUNS_PY + "  # carpeta de la sesion, fuera del repositorio: nada se escribe en el worktree de A-14\nos.chdir(R)\n")
t = rep(t, "D = f'artifacts/orchestration/I-64/F4-ENVIRONMENT/probe-{RID}'", "D = f'F4-ENVIRONMENT/probe-{RID}'")
no_l('make_probe_m.py', t)
wr('make_probe_m.py', t)

# probe_state: el repositorio medido es el worktree lateral de A-14
t = rd(SL, 'probe_state_l.py')
t = rep(t, "WT = os.path.join(os.environ['USERPROFILE'], '.codex', 'worktrees', 'architecture-workspace-persistente-rackcad')",
        "WT = '<worktree-de-A-14>'  # el worktree lateral de A-14, donde corre la sonda")
t = rep(t, "'TrackingBranch': g('rev-parse', 'refs/remotes/origin/architecture/workspace-persistente-rackcad')",
        "'TrackingBranch': g('rev-parse', 'refs/remotes/origin/architecture/workspace-persistente-rackcad-a14')")
no_l('probe_state_m.py', t)
wr('probe_state_m.py', t)

# launch: misma linea acreditada; directorio de Codex = el worktree lateral; salidas a la carpeta de la sesion por ruta absoluta
t = rd(SL, 'launch_probe_l.sh')
t = rep(t, 'wt="' + WT_POSIX + '"', 'wt="<worktree-de-A-14>"')
t = rep(t, 'rel="artifacts/orchestration/I-64/F4-ENVIRONMENT/probe-$1"\nrun_dir="$wt/$rel"\n',
        'run_dir="$(cd "$(dirname "$0")/../../a14r/F4-ENVIRONMENT" && pwd)/probe-$1"  # carpeta de la sesion: nada se escribe en el worktree\n')
t = rep(t, '  --output-schema "$rel/probe.schema.json" \\\n  -o "$rel/output.json"',
        '  --output-schema "$(cygpath -w "$run_dir/probe.schema.json")" \\\n  -o "$(cygpath -w "$run_dir/output.json")"')
no_l('launch_probe_m.sh', t)
wr('launch_probe_m.sh', t)

# pre y post
for src, dst in (('probe_pre_l.sh', 'probe_pre_m.sh'), ('probe_post_l.sh', 'probe_post_m.sh')):
    t = rd(SL, src)
    t = rep(t, 'wt=' + WT_POSIX + '\n', 'R="$(cd "$S/../../a14r" && pwd)"  # carpeta de la sesion; nada se escribe en el worktree de A-14\n')
    t = rep(t, 'D="$wt/artifacts/orchestration/I-64/F4-ENVIRONMENT/probe-$RID"', 'D="$R/F4-ENVIRONMENT/probe-$RID"')
    t = rep(t, 'pwsh -NoProfile -File "$(cygpath -w "$F/proc_check4.ps1")"', 'pwsh -NoProfile -File "$(cygpath -w "$S/proc_check_m.ps1")"')
    if src == 'probe_pre_l.sh':
        t = rep(t, "$(python -c 'import secrets;print(secrets.token_hex(2))')", "$(od -An -N2 -tx1 /dev/urandom | tr -d ' \\n')")
    t = t.replace('python "', 'python -I "')
    for a, b in (('probe-runid-l.txt', 'probe-runid-m.txt'), ('make_probe_l.py', 'make_probe_m.py'), ('probe_state_l.py', 'probe_state_m.py'),
                 ('cfg_check_l.py', 'cfg_check_m.py'), ('eval_probe_l.py', 'eval_probe_m.py')):
        t = t.replace(a, b)
    assert 'python "' not in t and 'python -c' not in t, t
    no_l(dst, t)
    wr(dst, t)

# eval: mismos criterios de PASS; referencia = la sonda 2c14 de la L; el oraculo, del worktree lateral
t = rd(SL, 'eval_probe_l.py')
t = rep(t, "WT = r'" + WT_WIN + "'\nos.chdir(WT)\n",
        "WT = '<worktree-de-A-14>'  # la sonda corre en el worktree lateral de A-14 (aqui solo se lee)\nos.chdir(WT)\nR = " + RUNS_PY + "\n")
t = rep(t, "D = f'artifacts/orchestration/I-64/F4-ENVIRONMENT/probe-{RID}'", "D = os.path.join(R, 'F4-ENVIRONMENT', f'probe-{RID}').replace(chr(92), '/')")
t = rep(t, "B454 = 'artifacts/orchestration/I-64/F4B-PBT4-EDIT-ALL-WINDOW/1/R20261121T205457Z-af49'  # referencia: planificacion af49 de PB-T4c (Snapshot K, mismo binario, 2026-11-21)",
        "B454 = os.path.join(os.environ['USERPROFILE'], '.codex', 'worktrees', 'architecture-workspace-persistente-rackcad', 'artifacts', 'orchestration', 'I-64', 'F4-ENVIRONMENT', 'probe-R20261122T172638Z-2c14').replace(chr(92), '/')"
        "  # referencia: la sonda neutra 2c14 de la Snapshot L (mismo binario, mismo prompt y esquema, 2026-11-22); solo se lee")
t = rep(t, "'ReferenceRun': 'R20261121T205457Z-af49 (planificacion de PB-T4c bajo la Snapshot K, mismo binario)'",
        "'ReferenceRun': 'R20261122T172638Z-2c14 (sonda neutra de la Snapshot L, mismo binario, mismo prompt y esquema, desde el worktree de I-64)'")
assert 'artifacts/orchestration/I-64/F4-ENVIRONMENT/probe-{RID}' not in t and WT_WIN not in t
wr('eval_probe_m.py', t)

# record
t = rd(SL, 'record_snapshot_l.py')
t = rep(t, '"""Registro de aceptacion de la Snapshot L', '"""Registro de aceptacion de la Snapshot M')
t = rep(t, "WT = os.path.join(U, '.codex', 'worktrees', 'architecture-workspace-persistente-rackcad')\n",
        "WT = '<worktree-de-A-14>'  # el worktree lateral de A-14: de el solo se lee el registro de la L\nR = " + RUNS_PY + "\n")
t = rep(t, "'probe-runid-l.txt'", "'probe-runid-m.txt'")
t = rep(t, "P = os.path.join(WT, 'artifacts', 'orchestration', 'I-64', 'F4-ENVIRONMENT', 'probe-' + RID)", "P = os.path.join(R, 'F4-ENVIRONMENT', 'probe-' + RID)")
t = rep(t, "'F4-ENVIRONMENT', 'snapshot-K', 'snapshot-k.json'), encoding='utf-8').read())  # la K, base de la comparacion",
        "'F4-ENVIRONMENT', 'snapshot-L', 'snapshot-l.json'), encoding='utf-8').read())  # la L, base de la comparacion")
t = rep(t, "'Snapshot': 'L'", "'Snapshot': 'M'")
t = rep(t, "'Authority': 'decision del Owner (2026-11-22), respuesta literal: Sonda neutra, luego L (Recomendado); F4B-DRAFTS/coordinator/owner-config-snapshot-l-decision.md'",
        "'Authority': 'decision del Owner (2026-11-23, quinta ronda de A-14), respuesta literal: Sonda neutra, luego M (Recomendado); F4-ENVIRONMENT/snapshot-M/owner-config-snapshot-m-decision.md'")
t = rep(t, "'ReplacesSnapshot': {'K': '" + K_SHA + "'}", "'ReplacesSnapshot': {'L': '" + L_SHA + "'}")
t = rep(t, "'KeysAddedVsK': ADDED, 'KeysRemovedVsK': REMOVED, 'ChangedValuesVsK': CHANGED, 'ComparedWith': 'inventario seguro de la Snapshot K (el unico valor redactado es mcp_servers.node_repl.env.SKY_CUA_NATIVE_PIPE_DIRECTORY)', 'AppVersionTransition': 'BROWSER_USE_CODEX_APP_VERSION 26.1111.71704 (K) -> 26.1117.41037 (L), actualizacion de la app de Codex del 2026-11-22 (config.toml escrito 09:29Z)',",
        "'KeysAddedVsL': ADDED, 'KeysRemovedVsL': REMOVED, 'ChangedValuesVsL': CHANGED, 'ComparedWith': 'inventario seguro de la Snapshot L (el unico valor redactado es mcp_servers.node_repl.env.SKY_CUA_NATIVE_PIPE_DIRECTORY)', 'AppVersionTransition': 'ninguna: BROWSER_USE_CODEX_APP_VERSION 26.1117.41037 en la L y en la M; la app de Codex reescribio config.toml al arrancar tras el reinicio del 2026-11-23 (escrito 19:21:15Z)',\n"
        "       'RedactedValueChangedVsL': {'Key': 'mcp_servers.node_repl.env.SKY_CUA_NATIVE_PIPE_DIRECTORY', 'What': 'el GUID del nombre de canalizacion codex-computer-use-<GUID> (identificador opaco; su valor no se registra), que la app de Codex genera en cada arranque: lo registran la instancia de la app del 2026-11-22 (09:25Z, la de la L) y la del 2026-11-23 (19:20Z, la de la M), con GUID distintos', 'Proof': 'poner en el config.toml actual el GUID que registro la instancia de la L reproduce byte a byte el SHA-256 de la Snapshot L; es el unico cambio del archivo (opaque_reconstruct_l.py, en tools/)', 'OtherBytesChanged': False},\n"
        "       'ProbeWorkingDirectory': 'el worktree lateral de A-14 (rama architecture/workspace-persistente-rackcad-a14), que no figura en [projects] de config.toml; la carpeta de la corrida y este registro, fuera del repositorio',")
t = rep(t, "'TurnContextDifferencesVsSnapshotKRunAf49': sorted(probe['TurnContextVsReference']['Differences'])",
        "'TurnContextDifferencesVsSnapshotLProbe2c14': sorted(probe['TurnContextVsReference']['Differences'])")
t = rep(t, "'Note': 'comparacion del contexto efectivo con la planificacion af49 de PB-T4c (Snapshot K, mismo binario, 2026-11-21)'},",
        "'Note': 'comparacion del contexto efectivo con la sonda neutra 2c14 de la Snapshot L (mismo binario, mismo prompt y esquema, 2026-11-22, desde el worktree de I-64); cwd y workspace_roots no se comparan, como en la L'},")
t = rep(t, "'Decision': 'Snapshot L aceptada si y solo si NeutralProbe.Pass es true, por la decision del Owner; sustituye a K como linea base activa del Controller en I-64'}",
        "'Decision': 'Snapshot M aceptada si y solo si NeutralProbe.Pass es true, por la decision del Owner (quinta ronda de A-14: esta configuracion queda como Snapshot M y se lanza la revision del Architect); este registro no cambia la cadena de F4B'}")
t = rep(t, "out = os.path.join(WT, 'artifacts', 'orchestration', 'I-64', 'F4-ENVIRONMENT', 'snapshot-l.json')", "out = os.path.join(R, 'F4-ENVIRONMENT', 'snapshot-m.json')")
t = rep(t, "print('L', rec['ConfigSha256'][:16]", "print('M', rec['ConfigSha256'][:16]")
t = rep(t, "'TurnContextDifferencesVsSnapshotKRunAf49'], 'changedVsK'", "'TurnContextDifferencesVsSnapshotLProbe2c14'], 'changedVsL'")
assert 'VsK' not in t and "'K'" not in t and 'snapshot-k' not in t and 'runid-l' not in t, [l for l in t.split('\n') if 'VsK' in l or "'K'" in l]
wr('record_snapshot_m.py', t)

# proc_check: copia literal de f2t1/proc_check4.ps1 con solo $wt cambiado al worktree lateral
t = rd(F2, 'proc_check4.ps1')
t = rep(t, "$wt = '" + WT_WIN + "'", "$wt = '<worktree-de-A-14>'")
wr('proc_check_m.ps1', t)
print('ok')
