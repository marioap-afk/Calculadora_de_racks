"""PB-T4d (paquete v5.2), precondicion 6: el censo de C13, por texto y por objetos git (green_pbt4d_v52.green sobre la base; no toca ninguna copia).

Las declaraciones de cada archivo C# que el GREEN cambia o borra (la lectura de census_c13_v51.py de PB-T4c, copiada: sin comentarios y con las
cadenas vaciadas; los tipos, los miembros a nivel de tipo, una sobrecarga es una mas, y los miembros de los enumerados). Lo que se pide:
  - GREEN - base vacia: ninguna declaracion nueva en src/;
  - base - GREEN igual, nombre a nombre y con su cuenta, a la lista cerrada: los tipos retirados enteros (con todos sus miembros) y los miembros
    retirados (EXPECTED_MEMBERS), incluidas las dos constantes de la rama muerta de BuildCore (owner-pbt4d-deadcode-27-decisions.md, pregunta 1);
  - el parametro allowErase, que el censo de declaraciones no ve, por texto: ninguna aparicion en el GREEN;
  - con autopruebas: una declaracion de mas, una de menos y un parametro allowErase inyectados en memoria, que el censo tiene que ver.
Escribe en UTF-8 con LF.

Uso: python -I census_c13_v52.py <repo> <pieza-green> <salida.txt>
"""
import collections
import importlib.util
import os
import re
import sys

sys.dont_write_bytecode = True
S = os.path.dirname(os.path.abspath(__file__))
PB4 = os.path.normpath(os.path.join(S, '..', '..', 'pbt4'))
spec = importlib.util.spec_from_file_location('green_pbt4d_v52', os.path.join(PB4, 'green_pbt4d_v52.py'))
gp = importlib.util.module_from_spec(spec)
spec.loader.exec_module(gp)
REPO, PIECE, OUT = sys.argv[1:4]
REV = '6c2820bab4d8b41e3e229e23f3a1638ccbb04735'
P = 'src/RackCad.Application/Systems/PushBack/Pilot/'

# La lista cerrada: los tipos que se retiran enteros y los miembros que se retiran (con su cuenta, una sobrecarga es una mas).
EXPECTED_TYPES = ['PushBackPilotStateComparer', 'PushBackPilotFieldAdapter', 'PushBackPilotComposition', 'PushBackPilotPrepareInput']
EXPECTED_MEMBERS = collections.Counter({
    'PushBackPilotMaterializer.ComposeForWrite': 1,
    'PushBackPilotMaterializer.KeepPilotEditsOn': 1,
    'PushBackPilotUpdatePlanner.Prepare': 1,
    'PushBackPilotUpdatePlanner.Build': 1,
    'PushBackPilotUpdatePlanner.ViewUnavailablePrefix': 1,
    'PushBackPilotUpdatePlanner.ViewUnavailableSuffix': 1,
    'PushBackPilotUpdateRun.Precheck': 1,
    'PushBackPilotUpdateRun.Execute': 1,
    'PushBackPilotUpdateRun.ComparePost': 1,
})

KEYWORDS = {'if', 'for', 'foreach', 'while', 'switch', 'using', 'lock', 'catch', 'return', 'new', 'nameof', 'typeof', 'sizeof', 'default', 'base',
            'this', 'throw', 'when', 'where', 'get', 'set', 'init', 'add', 'remove', 'fixed', 'checked', 'unchecked'}


def clean(text):
    """Sin comentarios y con las cadenas y los caracteres vaciados (las guardas de texto hacen lo mismo)."""
    text = re.sub(r'@"(?:[^"]|"")*"', '""', text)
    text = re.sub(r'\$?"(?:\\.|[^"\\\n])*"', '""', text)
    text = re.sub(r"'(?:\\.|[^'\\\n])'", "''", text)
    text = re.sub(r'/\*.*?\*/', ' ', text, flags=re.S)
    text = re.sub(r'//[^\n]*', '', text)
    return text


def member_name(head):
    head = re.sub(r'^\s*(?:\[[^\]]*\]\s*)+', '', head).strip()
    if not head or head.split()[0] in KEYWORDS or head.startswith('#'):
        return None
    arrow = head.find('=>')
    sig = head if arrow < 0 else head[:arrow]
    paren = sig.find('(')
    eq = re.search(r'(?<![=!<>])=(?![=>])', sig)
    if paren >= 0 and (eq is None or paren < eq.start()):
        m = re.search(r'((?:\w+\.)?\w+)\s*(?:<[^()]*>)?\s*\($', sig[:paren + 1].rstrip())
        if not m:
            return None
        name = m.group(1)
        return None if name.split('.')[-1] in KEYWORDS else name
    if eq is not None:
        sig = sig[:eq.start()]
    m = re.search(r'(\w+)\s*$', sig)
    if not m or len(sig.split()) < 2:
        return None
    return m.group(1)


def census_cs(text):
    """(declaracion -> cuenta, declaracion -> tipo contenedor)"""
    code = clean(text)
    found = []
    stack = []
    buf = ''
    for ch in code:
        if ch == '{':
            m = re.search(r'\b(class|struct|interface|enum|record)\s+(\w+)', buf)
            top = stack[-1] if stack else None
            if m and not re.search(r'\bnew\b|\bwhere\b[^{]*$', buf[:m.start()]):
                container = top[1] if top and top[0] == 'tipo' else None
                found.append(('tipo:' + m.group(2), container))
                stack.append(('tipo', m.group(2), m.group(1)))
            elif top and top[0] == 'tipo':
                name = member_name(buf)
                if name:
                    found.append((top[1] + '.' + name, top[1]))
                stack.append(('miembro',))
            else:
                stack.append(('otro',))
            buf = ''
        elif ch == '}':
            top = stack.pop() if stack else None
            if top and top[0] == 'tipo' and top[2] == 'enum':
                for item in buf.split(','):
                    item = re.sub(r'=.*$', '', item.strip(), flags=re.S).strip()
                    if re.fullmatch(r'\w+', item):
                        found.append((top[1] + '.' + item, top[1]))
            buf = ''
        elif ch == ';':
            top = stack[-1] if stack else None
            if top and top[0] == 'tipo' and top[2] != 'enum':
                name = member_name(buf)
                if name:
                    found.append((top[1] + '.' + name, top[1]))
            buf = ''
        else:
            buf += ch
    return collections.Counter(k for k, _ in found), {k: c for k, c in found}


def total(texts):
    count, container = collections.Counter(), {}
    for text in texts.values():
        c, k = census_cs(text)
        count += c
        container.update(k)
    return count, container


def expected(base_container, base_count):
    want = collections.Counter(EXPECTED_MEMBERS)
    types = list(EXPECTED_TYPES)
    for name, cont in sorted(base_container.items()):  # los tipos anidados de un tipo retirado se retiran con el
        if name.startswith('tipo:') and cont in types and name[5:] not in types:
            types.append(name[5:])
    for t in types:
        if t in EXPECTED_TYPES:  # un tipo anidado ya cuenta como miembro de su contenedor
            want['tipo:' + t] += base_count['tipo:' + t]
        for name, cont in base_container.items():
            if cont == t:
                want[name] += base_count[name]
    return want


def verdict(base, green):
    bc, bk = total(base)
    gc, _ = total(green)
    added = gc - bc
    removed = bc - gc
    want = expected(bk, bc)
    allow = sum(len(re.findall(r'\ballowErase\b', clean(t))) for t in green.values())
    ok = not added and removed == want and allow == 0
    return ok, added, removed, want, allow


base, green, deleted = gp.green(REPO, REV, PIECE)
for path in deleted:
    green[path] = ''
lines = ['# C13 de PB-T4d por texto: el GREEN (%s) frente a la base %s, en los %d archivos que cambia o borra' % (os.path.basename(PIECE), REV, len(base))]
ok, added, removed, want, allow = verdict(base, green)
lines.append('nuevas en el GREEN: %d %s' % (sum(added.values()), sorted(added.items())))
lines.append('quitadas: %d; lista cerrada: %d; iguales=%s' % (sum(removed.values()), sum(want.values()), removed == want))
for name, n in sorted(removed.items()):
    lines.append('  quitada %s x%d%s' % (name, n, '' if want.get(name) == n else '  <- FUERA DE LA LISTA'))
for name, n in sorted((want - removed).items()):
    lines.append('  en la lista y no quitada %s x%d' % (name, n))
lines.append('allowErase en el GREEN: %d' % allow)
lines.append('censo ' + ('PASA' if ok else 'CAE'))

# Las autopruebas: cada una cambia el GREEN en memoria y el veredicto tiene que caer.
plan = P + 'PushBackPilotUpdatePlan.cs'
probes = [
    ('una-declaracion-de-mas', lambda g: {**g, plan: g[plan].replace('        public const string ViewAddressPrefix', '        public const string Extra = "";\n\n        public const string ViewAddressPrefix', 1)}),
    ('una-de-menos', lambda g: {**g, plan: re.sub(r'\n        public const string AllViewsObsoleteReason = [^\n]*\n', '\n', g[plan], count=1)}),
    ('el-parametro-allowErase', lambda g: {**g, plan: g[plan].replace('            double scale,\n', '            double scale,\n            bool allowErase,\n', 1)}),
    ('una-constante-retirada-de-vuelta', lambda g: {**g, plan: g[plan].replace('        public const string ViewAddressPrefix', '        public const string ViewUnavailablePrefix = "";\n\n        public const string ViewAddressPrefix', 1)}),
]
for name, change in probes:
    mutated = change(dict(green))
    assert mutated[plan] != green[plan], name
    lines.append('autoprueba-%s=%s' % (name, 'falla' if not verdict(base, mutated)[0] else 'pasa'))
text = '\n'.join(lines) + '\n'
with open(OUT, 'w', encoding='utf-8', newline='\n') as f:
    f.write(text)
print('\n'.join(l for l in lines if not l.startswith('  ')))
