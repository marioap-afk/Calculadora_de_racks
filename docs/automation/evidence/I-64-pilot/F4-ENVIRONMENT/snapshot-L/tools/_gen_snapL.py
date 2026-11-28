"""Deriva la sonda neutra de la Snapshot L desde la de la K (hs65/snapK), con reemplazos exactos (decision del Owner, 2026-11-22: Sonda neutra, luego L)."""
import os, ast
H = os.path.dirname(os.path.abspath(__file__))
SK = os.path.join(os.path.dirname(H), 'snapK')
K_SHA = 'E1F69847610FD810D5286B82C86E00FE85D9F497F89E9FD9645E0EEA113322FE'
L_SHA = 'B71650A637551747F522BB1D2F4BA1BB9E25AAFF1A61CE1B7BC066197FFFEBB3'
K_NAMES = 'EB8775C5F9B3CC5C15031ED064391FFE000AB4A7220360C04F2F89D83D49F820'
L_NAMES = '3ECF8E072CC52AE60A6E6D508DB54DCBFFFAFB04EAF9B52905A2CEAB0E0812E3'
REF_K = 'artifacts/orchestration/I-64/F4B-PBT2A-DRAFT-STATE-PORT/2/R20261119T065628Z-7de7'
REF = 'artifacts/orchestration/I-64/F4B-PBT4-EDIT-ALL-WINDOW/1/R20261121T205457Z-af49'
rd = lambda n: open(os.path.join(SK, n), encoding='utf-8').read()


def wr(n, t):
    if n.endswith('.py'):
        ast.parse(t)
    p = os.path.join(H, n)
    assert not os.path.exists(p), n
    open(p, 'w', encoding='utf-8', newline='\n').write(t)


def rep(t, a, b, n=1):
    assert t.count(a) == n, (a[:100], t.count(a))
    return t.replace(a, b)


t = rd('cfg_check_k.py')
t = rep(t, "OK = {'" + K_SHA + "': 'K'}  # solo para la sonda de la K", "OK = {'" + L_SHA + "': 'L'}  # solo para la sonda de la L")
t = rep(t, "NAMES = '" + K_NAMES + "'", "NAMES = '" + L_NAMES + "'  # la L: 105 nombres (se anade mcp_servers.node_repl.env.NODE_REPL_LAZY_TRUSTED_WORKER)")
t = rep(t, "names_sha == NAMES and len(names) == 104", "names_sha == NAMES and len(names) == 105")
wr('cfg_check_l.py', t)
for n in ('make_probe_k.py', 'probe_state_k.py', 'launch_probe_k.sh'):
    t = rd(n)
    assert '_k.' not in t and 'runid-k' not in t and 'snapK' not in t, n
    wr(n.replace('_k.', '_l.'), t)
t = rd('eval_probe_k.py')
t = rep(t, "B454 = '" + REF_K + "'  # referencia: planificacion 7de7 de PB-T2a (Snapshot J, mismo binario, 2026-11-19)",
        "B454 = '" + REF + "'  # referencia: planificacion af49 de PB-T4c (Snapshot K, mismo binario, 2026-11-21)")
t = rep(t, "'ReferenceRun': 'R20261119T065628Z-7de7 (planificacion de la correccion de PB-T2a bajo la Snapshot J, mismo binario)'",
        "'ReferenceRun': 'R20261121T205457Z-af49 (planificacion de PB-T4c bajo la Snapshot K, mismo binario)'")
wr('eval_probe_l.py', t)
for src, dst in (('probe_pre_k.sh', 'probe_pre_l.sh'), ('probe_post_k.sh', 'probe_post_l.sh')):
    t = rd(src)
    for a, b in (('probe-runid-k.txt', 'probe-runid-l.txt'), ('make_probe_k.py', 'make_probe_l.py'), ('probe_state_k.py', 'probe_state_l.py'),
                 ('cfg_check_k.py', 'cfg_check_l.py'), ('eval_probe_k.py', 'eval_probe_l.py'), ('snapK', 'snapL')):
        t = t.replace(a, b)
    assert '_k.' not in t and 'runid-k' not in t and 'snapK' not in t, t
    wr(dst, t)
t = rd('record_snapshot_k.py')
t = rep(t, '"""Registro de aceptacion de la Snapshot K', '"""Registro de aceptacion de la Snapshot L')
t = rep(t, "'probe-runid-k.txt'", "'probe-runid-l.txt'")
t = rep(t, "'F4-ENVIRONMENT', 'snapshot-J', 'snapshot-j.json'), encoding='utf-8').read())  # la J, base de la comparacion",
        "'F4-ENVIRONMENT', 'snapshot-K', 'snapshot-k.json'), encoding='utf-8').read())  # la K, base de la comparacion")
t = rep(t, "'Snapshot': 'K'", "'Snapshot': 'L'")
t = rep(t, "'Authority': 'decision del Owner (2026-11-19), respuesta literal: Sonda neutra, luego K (Recomendado); F4B-DRAFTS/coordinator/owner-config-snapshot-k-decision.md'",
        "'Authority': 'decision del Owner (2026-11-22), respuesta literal: Sonda neutra, luego L (Recomendado); F4B-DRAFTS/coordinator/owner-config-snapshot-l-decision.md'")
t = rep(t, "'ReplacesSnapshot': {'J': '57B0FED688317D347BCC726F99549BDE6AC9EC108D6E2CCBA10A9D71F233F961'}", "'ReplacesSnapshot': {'K': '" + K_SHA + "'}")
t = rep(t, "'OrderedNamesSha256': '" + K_NAMES + "', 'NameCount': 104,", "'OrderedNamesSha256': '" + L_NAMES + "', 'NameCount': 105,")
t = rep(t, "'KeysAddedVsJ': ADDED, 'KeysRemovedVsJ': REMOVED, 'ChangedValuesVsJ': CHANGED, 'ComparedWith': 'inventario seguro de la Snapshot J (el unico valor redactado es mcp_servers.node_repl.env.SKY_CUA_NATIVE_PIPE_DIRECTORY)', 'AppVersionTransition': 'BROWSER_USE_CODEX_APP_VERSION 26.1111.21308 (J) -> 26.1111.71704 (K), actualizacion de la app de Codex del 2026-11-19 (config.toml escrito 14:05Z)',",
        "'KeysAddedVsK': ADDED, 'KeysRemovedVsK': REMOVED, 'ChangedValuesVsK': CHANGED, 'ComparedWith': 'inventario seguro de la Snapshot K (el unico valor redactado es mcp_servers.node_repl.env.SKY_CUA_NATIVE_PIPE_DIRECTORY)', 'AppVersionTransition': 'BROWSER_USE_CODEX_APP_VERSION 26.1111.71704 (K) -> 26.1117.41037 (L), actualizacion de la app de Codex del 2026-11-22 (config.toml escrito 09:29Z)',")
t = rep(t, "'TurnContextDifferencesVsSnapshotJRun7de7': sorted(probe['TurnContextVsReference']['Differences'])",
        "'TurnContextDifferencesVsSnapshotKRunAf49': sorted(probe['TurnContextVsReference']['Differences'])")
t = rep(t, "'Note': 'comparacion del contexto efectivo con la planificacion 7de7 de la correccion de PB-T2a (Snapshot J, mismo binario, 2026-11-19)'},",
        "'Note': 'comparacion del contexto efectivo con la planificacion af49 de PB-T4c (Snapshot K, mismo binario, 2026-11-21)'},")
t = rep(t, "'Decision': 'Snapshot K aceptada si y solo si NeutralProbe.Pass es true, por la decision del Owner; sustituye a J como linea base activa del Controller en I-64'}",
        "'Decision': 'Snapshot L aceptada si y solo si NeutralProbe.Pass es true, por la decision del Owner; sustituye a K como linea base activa del Controller en I-64'}")
t = rep(t, "'F4-ENVIRONMENT', 'snapshot-k.json')", "'F4-ENVIRONMENT', 'snapshot-l.json')")
t = rep(t, "print('K', rec['ConfigSha256'][:16]", "print('L', rec['ConfigSha256'][:16]")
t = rep(t, "'TurnContextDifferencesVsSnapshotJRun7de7'], 'changedVsJ'", "'TurnContextDifferencesVsSnapshotKRunAf49'], 'changedVsK'")
assert 'VsJ' not in t and "'J'" not in t and 'snapshot-j' not in t, [l for l in t.split('\n') if 'VsJ' in l or "'J'" in l]
wr('record_snapshot_l.py', t)
print('ok')
