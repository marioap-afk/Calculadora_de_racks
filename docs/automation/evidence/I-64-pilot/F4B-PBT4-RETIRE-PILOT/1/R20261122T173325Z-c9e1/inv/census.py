"""Censo de declaraciones (solo lectura) de los seis archivos de Pilot/ del alcance.

Uso (cwd = worktree): python -I census.py <rev|WORKTREE> -> imprime una declaracion por linea: "<archivo>\t<tipo-contenedor>\t<nombre>(<n-params-o-firma>)".
Heuristica: recorre cada archivo sin comentarios ni literales, con profundidad de llaves; una declaracion es la primera linea de un miembro
(a profundidad de miembro) que empieza por un modificador de acceso/estatico, y los tipos (class/struct/enum/interface/record).
Una sobrecarga cuenta como una declaracion mas porque su firma (tipos de parametros) forma parte del nombre.
"""
import re
import subprocess
import sys

FILES = [
    'PushBackPilotMaterializer.cs', 'PushBackPilotUpdatePlan.cs', 'PushBackPilotUpdateRun.cs',
    'PushBackPilotDraftState.cs', 'PushBackPilotStateComparer.cs', 'PushBackPilotFieldAdapter.cs',
]
DIR = 'src/RackCad.Application/Systems/PushBack/Pilot/'
MOD = re.compile(r'^(?:\[[^\]]*\]\s*)*(public|internal|private|protected|static|const|readonly|override|sealed|abstract|async|partial|extern|new|unsafe)\b')
TYPE = re.compile(r'\b(class|struct|enum|interface|record(?:\s+struct)?)\s+([A-Za-z_][A-Za-z0-9_]*)')


def strip(text):
    out = []
    i, n = 0, len(text)
    while i < n:
        c = text[i]
        two = text[i:i + 2]
        if two == '//':
            while i < n and text[i] != '\n':
                i += 1
        elif two == '/*':
            j = text.find('*/', i + 2)
            i = n if j < 0 else j + 2
        elif text.startswith('@"', i) or text.startswith('$@"', i) or text.startswith('@$"', i):
            j = text.index('"', i) + 1
            while j < n:
                if text[j] == '"':
                    if text[j + 1:j + 2] == '"':
                        j += 2
                        continue
                    break
                j += 1
            out.append('""')
            i = j + 1
        elif c == '"' or two == '$"':
            j = i + (2 if two == '$"' else 1)
            while j < n and text[j] != '"':
                j += 2 if text[j] == '\\' else 1
            out.append('""')
            i = j + 1
        elif c == "'":
            j = i + 1
            while j < n and text[j] != "'":
                j += 2 if text[j] == '\\' else 1
            out.append("''")
            i = j + 1
        else:
            out.append(c)
            i += 1
    return ''.join(out)


def decls(text):
    lines = strip(text).split('\n')
    depth = 0
    stack = []  # (name, depth-of-body)
    res = []
    pend = None
    for ln in lines:
        s = ln.strip()
        cur_depth = depth
        if s and MOD.match(s):
            # member or type
            tm = TYPE.search(s.split('(')[0].split('=')[0].split('{')[0])
            container = stack[-1][0] if stack else ''
            if tm and ('(' not in s[:tm.start()]):
                res.append((container, tm.group(2), 'type'))
                pend = tm.group(2)
            else:
                head = re.split(r'\s=>|=(?!=)|\{|;', s, 1)[0]
                if '(' in head:
                    name = re.search(r'([A-Za-z_][A-Za-z0-9_<>]*)\s*\(', head)
                    # parametros: se juntan hasta el cierre en la misma linea o se toma lo que haya
                    params = head[head.index('('):]
                    params = re.sub(r'\s+', ' ', params)
                    res.append((container, (name.group(1) if name else '?') + params, 'method'))
                else:
                    toks = head.split()
                    name = toks[-1] if toks else '?'
                    res.append((container, name, 'member'))
        opens = ln.count('{')
        closes = ln.count('}')
        for _ in range(opens):
            depth += 1
            if pend is not None:
                stack.append((pend, depth))
                pend = None
        for _ in range(closes):
            if stack and stack[-1][1] == depth:
                stack.pop()
            depth -= 1
    return res


def main():
    rev = sys.argv[1]
    sys.stdout.reconfigure(encoding='utf-8', newline='\n')
    for f in FILES:
        p = DIR + f
        if rev == 'WORKTREE':
            try:
                t = open(p, 'rb').read().decode('utf-8-sig').replace('\r\n', '\n')
            except FileNotFoundError:
                continue
        else:
            r = subprocess.run(['git', 'show', rev + ':' + p], capture_output=True)
            if r.returncode != 0:
                continue
            t = r.stdout.decode('utf-8-sig').replace('\r\n', '\n')
        for container, name, kind in decls(t):
            print('%s\t%s\t%s\t%s' % (f, container, kind, name))


main()
