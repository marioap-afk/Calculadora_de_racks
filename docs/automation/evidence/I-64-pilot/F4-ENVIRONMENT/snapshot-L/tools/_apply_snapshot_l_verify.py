"""Tras la sonda 2c14 en PASS (decision del Owner del 2026-11-22, Sonda neutra, luego L): la Snapshot L entra en el prompt de la verificacion
de PB-T4d (pbt4dcc/make_pbt4d_verify.py). Reemplazo exacto de la unica linea de config.toml; copia previa en snapL/."""
import os, ast, hashlib
HS = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
p = os.path.join(HS, 'pbt4dcc', 'make_pbt4d_verify.py')
t = open(p, encoding='utf-8').read()
bak = os.path.join(HS, 'snapL', 'make_pbt4d_verify.before-L.py')
assert not os.path.exists(bak)
open(bak, 'w', encoding='utf-8', newline='').write(t)
nl = '\r\n' if '\r\n' in t else '\n'
lines = t.split(nl)
START = "'- config.toml: en la salida y en la entrada del relevo el hash es la SNAPSHOT K (E1F69847...)"
idx = [i for i, l in enumerate(lines) if l.startswith(START)]
assert len(idx) == 1, idx
print('antes sha256', hashlib.sha256(lines[idx[0]].encode('utf-8')).hexdigest()[:8])
lines[idx[0]] = ("'- config.toml: en la salida y en la entrada del relevo el hash es la SNAPSHOT L (B71650A6...), aceptada por el Owner el 2026-11-22 tras la sonda neutra "
                 "R20261122T172638Z-2c14 en PASS (respuesta literal: Sonda neutra, luego L; sustituye a la K tras la actualizacion de la app de Codex del 2026-11-22, que cambio "
                 "dos valores con la version de la app y anadio la clave mcp_servers.node_repl.env.NODE_REPL_LAZY_TRUSTED_WORKER); registro en "
                 "artifacts/orchestration/I-64/F4-ENVIRONMENT/snapshot-l.json y decision literal en artifacts/orchestration/I-64/F4-ENVIRONMENT/owner-config-snapshot-l-decision.md, "
                 "que se custodian con el cierre de PB-T4d; la Autoridad del contrato que cita la Snapshot K (F4B-E37) es la anterior; es la Snapshot en vigor que cita P-01 del contrato, "
                 "cuya frase operativa es the accepted Snapshot in force; no es un STOP de esta verificacion.',")
t = nl.join(lines)
ast.parse(t)
open(p, 'w', encoding='utf-8', newline='').write(t)
print('ok', idx[0] + 1)
