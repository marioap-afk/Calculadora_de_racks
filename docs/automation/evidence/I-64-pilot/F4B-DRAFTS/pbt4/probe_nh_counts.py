"""Sonda: las cuentas de NH en la revisión (sin comentarios), para ver qué guarda las fija y sobre qué texto.

Uso: python -I probe_nh_counts.py <checkout> <rev>
"""
import re
import subprocess
import sys

repo, rev = sys.argv[1:3]
p = subprocess.run(['git', '-C', repo, 'show', rev + ':src/RackCad.Plugin/Workspace/WorkspaceNavigationHost.cs'], capture_output=True)
text = p.stdout.decode('utf-8').replace('\r\n', '\n')
code = re.sub(r'//[^\r\n]*', '', re.sub(r'/\*.*?\*/', '', text, flags=re.S))
for pat in [r'RackLog\.Exception\s*\(', r'\bReturnFocusToDrawing\s*\(', r'\.Focus\s*\(', r'\+=', r'\basync\b', r'\bawait\b', r'LockDocument']:
    print('%-32s %d' % (pat, len(re.findall(pat, code))))
for t in ['WorkspaceNavigationHostTests', 'WorkspaceBrowserHostGuardTests']:
    q = subprocess.run(['git', '-C', repo, 'show', rev + ':tests/RackCad.Tests/Workspace/%s.cs' % t], capture_output=True)
    src = q.stdout.decode('utf-8').replace('\r\n', '\n')
    for m in re.finditer(r'(?:var|private static string)\s+host\s*=\s*([^;\n]+)', src):
        print(t, 'host =', m.group(1)[:120])
    for m in re.finditer(r'(?:string|static string)\s+Host\s*\(\s*\)\s*=>\s*([^;\n]+)', src):
        print(t, 'Host() =>', m.group(1)[:120])
