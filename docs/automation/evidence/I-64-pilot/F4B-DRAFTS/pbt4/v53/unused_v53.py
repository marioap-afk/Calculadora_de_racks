"""PB-T4d (paquete v5.3), CA-T4d M-03: quita de un texto de C# la declaracion de un miembro sin uso (un metodo, un campo o una propiedad, un
record posicional o un tipo), con sus ///, sus atributos y el comentario // pegado encima, y una linea en blanco de las dos que la rodean. Modulo
para make_red_v53.py y make_list_v53.py; no escribe nada.

  remove(text, name, nth=None) -> (texto, (primera, ultima)): la declaracion `name` que esta una sola vez (o la n.o nth, desde 1, por orden),
  con las lineas de la base que quita (1-based).
  spans(text, name) -> [(primera, ultima)] de cada declaracion de `name`, por orden.
"""
import importlib.util
import os
import re

HERE = os.path.dirname(os.path.abspath(__file__))
_spec = importlib.util.spec_from_file_location('lists_core', os.path.join(os.path.dirname(HERE), 'lists_core.py'))
lc = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(lc)
MODS = r'(?:(?:public|private|internal|protected|static|async|override|sealed|readonly|virtual|unsafe|extern|const|file|abstract|partial)\s+)'
FIELD = re.compile(r'^[ \t]*' + MODS + r'+[\w<>\[\],.?() \t]+?\s+(\w+)\s*(?:=(?!>)|;|\{\s*get|=>)', re.M)


def _close(masked, i, opener, closer):
    d = 0
    for k in range(i, len(masked)):
        if masked[k] == opener:
            d += 1
        elif masked[k] == closer:
            d -= 1
            if d == 0:
                return k
    raise AssertionError('sin cierre')


def _statement_end(masked, i):
    d = 0
    for k in range(i, len(masked)):
        ch = masked[k]
        if ch in '({[':
            d += 1
        elif ch in ')}]':
            d -= 1
        elif ch == ';' and d <= 0:
            return k
    raise AssertionError('sin ;')


def _decl_ends(masked, name):
    """(inicio, fin) en caracteres de cada declaracion de `name`."""
    out = []
    # un tipo: class, struct, record o interface (un record posicional puede acabar en ; o en un cuerpo)
    for m in re.finditer(r'^[ \t]*' + MODS + r'*(?:class|struct|interface|record(?:\s+class|\s+struct)?)\s+' + re.escape(name) + r'\b', masked, re.M):
        j = m.end()
        while j < len(masked) and masked[j] not in '({;':
            j += 1
        if masked[j] == '(':
            j = _close(masked, j, '(', ')') + 1
            while masked[j] in ' \t\r\n':
                j += 1
            if masked[j] == ';':
                out.append((m.start(), j))
                continue
            while masked[j] != '{':
                j += 1
        if masked[j] == ';':
            out.append((m.start(), j))
        else:
            out.append((m.start(), _close(masked, j, '{', '}')))
    # un metodo: sus modificadores al principio de la linea, el nombre y sus parentesis; cuerpo de llaves o de expresion
    for m in re.finditer(r'^[ \t]*' + MODS + r'+[\w<>\[\],.?()\s]*?\b' + re.escape(name) + r'\s*(?:<[^>()]*>)?\s*\(', masked, re.M):
        if any(a <= m.start() <= b for a, b in out):
            continue
        j = _close(masked, m.end() - 1, '(', ')') + 1
        while masked[j] in ' \t\r\n' or masked.startswith('where', j):
            if masked.startswith('where', j):
                while masked[j] not in '{=':
                    j += 1
                break
            j += 1
        if masked[j] == '{':
            out.append((m.start(), _close(masked, j, '{', '}')))
        else:
            out.append((m.start(), _statement_end(masked, j)))
    # un campo o una propiedad
    for m in FIELD.finditer(masked):
        if m.group(1) != name or any(a <= m.start() <= b for a, b in out):
            continue
        mt = m.group(0)
        if mt.rstrip().endswith('get') and '{' in mt:
            k = _close(masked, m.start() + mt.rindex('{'), '{', '}')
            rest = re.match(r'\s*=(?!>)', masked[k + 1:k + 40])
            out.append((m.start(), _statement_end(masked, k + 1) if rest else k))
        else:
            out.append((m.start(), _statement_end(masked, m.end() - 1)))
    return sorted(out)


def spans(text, name):
    masked = lc.mask(text)
    lines = text.split('\n')
    res = []
    for a, b in _decl_ends(masked, name):
        first = masked.count('\n', 0, a) + 1
        last = masked.count('\n', 0, b) + 1
        while first > 1 and (lines[first - 2].strip().startswith('///') or lines[first - 2].strip().startswith('[')
                             or (lines[first - 2].strip().startswith('//') and not lines[first - 2].strip().startswith('// ---')
                                 and not lines[first - 2].strip().startswith('// ==='))):
            first -= 1
        res.append((first, last))
    return res


def remove(text, name, nth=None):
    found = spans(text, name)
    if nth is None:
        assert len(found) == 1, ('la declaracion no esta una vez', name, found)
        a, b = found[0]
    else:
        assert len(found) >= nth, ('no hay tantas declaraciones', name, nth, found)
        a, b = found[nth - 1]
    lines = text.split('\n')
    before, after = lines[:a - 1], lines[b:]
    # una linea en blanco de las dos que la rodean (o la de despues, tras una llave que abre)
    if after and after[0].strip() == '' and (not before or before[-1].strip() == '' or before[-1].strip() == '{'):
        after = after[1:]
    elif before and before[-1].strip() == '' and after and after[0].strip() == '}':
        before = before[:-1]
    return '\n'.join(before + after), (a, b)
