"""Tras la sonda 2c14 en PASS (decision del Owner del 2026-11-22, Sonda neutra, luego L): la Snapshot L entra en la comprobacion de config
de la cadena (f4/cfg_check_f4.py). La L anade una clave (105 nombres): su lista de nombres va por snapshot; las demas siguen con NAMES y 104. Reemplazos exactos."""
import os, ast
HS = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
K = 'E1F69847610FD810D5286B82C86E00FE85D9F497F89E9FD9645E0EEA113322FE'
L = 'B71650A637551747F522BB1D2F4BA1BB9E25AAFF1A61CE1B7BC066197FFFEBB3'
L_NAMES = '3ECF8E072CC52AE60A6E6D508DB54DCBFFFAFB04EAF9B52905A2CEAB0E0812E3'
p = os.path.join(HS, 'f4', 'cfg_check_f4.py')
t = open(p, encoding='utf-8').read()
bak = os.path.join(HS, 'snapL', 'cfg_check_f4.before-L.py')
assert not os.path.exists(bak)
open(bak, 'w', encoding='utf-8', newline='').write(t)


def rep(t, a, b):
    assert t.count(a) == 1, (a[:80], t.count(a))
    return t.replace(a, b)


t = rep(t, "'" + K + "': 'K'}", "'" + K + "': 'K', '" + L + "': 'L'}")
t = rep(t, "(actualizacion de la app de Codex, BROWSER_USE_CODEX_APP_VERSION 26.1111.71704)",
        "(actualizacion de la app de Codex, BROWSER_USE_CODEX_APP_VERSION 26.1111.71704); L: sonda neutra 2c14 PASS y decision del Owner del 2026-11-22 (actualizacion de la app de Codex, BROWSER_USE_CODEX_APP_VERSION 26.1117.41037, y una clave nueva, NODE_REPL_LAZY_TRUSTED_WORKER)")
t = rep(t, "NAMES = 'EB8775C5F9B3CC5C15031ED064391FFE000AB4A7220360C04F2F89D83D49F820'\n",
        "NAMES = 'EB8775C5F9B3CC5C15031ED064391FFE000AB4A7220360C04F2F89D83D49F820'\n"
        "NAMES_BY = {'L': ('" + L_NAMES + "', 105)}  # la L anade mcp_servers.node_repl.env.NODE_REPL_LAZY_TRUSTED_WORKER; las demas: NAMES y 104\n")
t = rep(t, "res['Accepted'] = sha in OK and names_sha == NAMES and len(names) == 104",
        "res['Accepted'] = sha in OK and (names_sha, len(names)) == NAMES_BY.get(OK.get(sha), (NAMES, 104))")
ast.parse(t)
open(p, 'w', encoding='utf-8', newline='').write(t)
print('ok')
