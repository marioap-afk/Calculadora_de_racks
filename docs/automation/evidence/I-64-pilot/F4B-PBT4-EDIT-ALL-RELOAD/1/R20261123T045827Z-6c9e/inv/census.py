"""Censo de declaraciones (solo lectura) de un archivo C#: tipos, metodos (con su firma, asi que una sobrecarga es una declaracion mas),
constructores, propiedades, campos, constantes, eventos, indexadores y miembros de enum. Quita comentarios y cadenas, sigue las llaves.
Los locales y los parametros no son declaraciones de miembro: no entran.
Uso como modulo: census(text) -> lista de cadenas "<ruta-de-tipos>::<tipo-de-miembro> <nombre>[(<firma>)]"."""
import re
import sys

TYPE_RE = re.compile(r'(?<![\w@])(class|struct|interface|enum|record)\s+(@?\w+)')


def strip(text):
    out = []
    i, n = 0, len(text)
    while i < n:
        c = text[i]
        nx = text[i + 1] if i + 1 < n else ''
        if c == '/' and nx == '/':
            while i < n and text[i] != '\n':
                i += 1
        elif c == '/' and nx == '*':
            j = text.find('*/', i + 2)
            i = n if j < 0 else j + 2
            out.append(' ')
        elif text.startswith('"""', i):
            j = text.find('"""', i + 3)
            i = n if j < 0 else j + 3
            out.append('""')
        elif (c == '@' and nx == '"') or (c == '$' and nx == '@' and text[i + 2:i + 3] == '"') or (c == '@' and nx == '$' and text[i + 2:i + 3] == '"'):
            j = text.find('"', i)
            j += 1
            while j < n:
                if text[j] == '"':
                    if j + 1 < n and text[j + 1] == '"':
                        j += 2
                        continue
                    break
                j += 1
            i = j + 1
            out.append('""')
        elif c == '"' or (c == '$' and nx == '"'):
            j = i + (2 if c == '$' else 1)
            while j < n and text[j] != '"':
                if text[j] == '\\':
                    j += 1
                j += 1
            i = j + 1
            out.append('""')
        elif c == "'":
            j = i + 1
            while j < n and text[j] != "'":
                if text[j] == '\\':
                    j += 1
                j += 1
            i = j + 1
            out.append("'c'")
        else:
            out.append(c)
            i += 1
    return ''.join(out)


def clean_header(h):
    h = re.sub(r'\s+', ' ', h).strip()
    # quita atributos iniciales [..]
    while h.startswith('['):
        depth = 0
        for k, ch in enumerate(h):
            if ch == '[':
                depth += 1
            elif ch == ']':
                depth -= 1
                if depth == 0:
                    h = h[k + 1:].strip()
                    break
        else:
            break
    return h


def member_name(h):
    """(tipo, nombre) de un encabezado de miembro ya limpio, o None."""
    if not h or h.startswith('using ') or h.startswith('namespace '):
        return None
    eq = h.find('=')
    paren = h.find('(')
    # delegate
    m = re.match(r'(?:[\w ]*\s)?delegate\s+.*?(\w+)\s*(?:<[^>]*>)?\s*\(', h)
    if m:
        return ('delegate', m.group(1) + '(' + h[h.find('('):].strip() + ')')
    if re.search(r'\bevent\b', h):
        mm = re.search(r'(\w+)\s*(?:=|$)', h.split('=')[0].strip() + ' ')
        names = re.findall(r'(\w+)', h.split('=')[0])
        return ('event', names[-1] if names else '?')
    if paren >= 0 and (eq < 0 or paren < eq) and '=>' not in h[:paren]:
        head = h[:paren]
        names = re.findall(r'(@?[\w~]+)\s*(?:<[^<>]*>)?\s*$', head)
        name = names[-1] if names else '?'
        sig = h[paren:]
        # firma: hasta el ')' que cierra
        depth = 0
        end = len(sig)
        for k, ch in enumerate(sig):
            if ch == '(':
                depth += 1
            elif ch == ')':
                depth -= 1
                if depth == 0:
                    end = k + 1
                    break
        return ('method', name + sig[:end])
    if 'this[' in h or re.search(r'\bthis\s*\[', h):
        return ('indexer', h)
    # propiedad con '=>' o campo
    cut = h
    for sep in ('=>', '='):
        k = cut.find(sep)
        if k >= 0:
            cut = cut[:k]
    names = re.findall(r'(@?\w+)', cut)
    if not names:
        return None
    kind = 'const' if re.search(r'\bconst\b', cut) else ('field/property')
    return (kind, names[-1])


def census(text):
    t = strip(text)
    decls = []
    # pila de marcos: (kind, name)
    stack = [('ns', '')]
    header = []
    i, n = 0, len(t)

    def path():
        return '.'.join(name for kind, name in stack if kind in ('type', 'enum') and name)

    def hdr():
        return clean_header(''.join(header))

    cont_after_pop = []  # marcos "code" de inicializador: tras cerrar, el encabezado continua
    while i < n:
        c = t[i]
        top = stack[-1][0]
        if top == 'code':
            if c == '{':
                stack.append(('code', ''))
            elif c == '}':
                _, nm = stack.pop()
                if stack[-1][0] != 'code' and cont_after_pop and cont_after_pop[-1] == len(stack):
                    cont_after_pop.pop()
                    header.append(' {} ')
            i += 1
            continue
        if c == '{':
            h = hdr()
            m = TYPE_RE.search(h) if ('=' not in h.split('(')[0]) else None
            if m and '=>' not in h:
                kind = 'enum' if m.group(1) == 'enum' else 'type'
                decls.append('%s::%s %s' % (path(), m.group(1), m.group(2)))
                stack.append((kind, m.group(2)))
                header = []
            elif h.startswith('namespace '):
                stack.append(('ns', h[10:].strip()))
                header = []
            elif '=' in h.split('(')[0] or h.endswith('=>') or '=>' in h:
                cont_after_pop.append(len(stack))
                stack.append(('code', ''))
                header.append(' ')
            else:
                mn = member_name(h)
                if mn:
                    decls.append('%s::%s %s' % (path(), mn[0], mn[1]))
                stack.append(('code', ''))
                header = []
        elif c == '}':
            if top == 'enum':
                mm = re.match(r'(@?\w+)', hdr())
                if mm:
                    decls.append('%s::enum-member %s' % (path(), mm.group(1)))
            if len(stack) > 1:
                stack.pop()
            header = []
        elif c == ';':
            h = hdr()
            mn = member_name(h) if top in ('type',) else None
            if mn:
                decls.append('%s::%s %s' % (path(), mn[0], mn[1]))
            header = []
        elif c == ',' and top == 'enum':
            h = hdr()
            mm = re.match(r'(@?\w+)', h)
            if mm:
                decls.append('%s::enum-member %s' % (path(), mm.group(1)))
            header = []
        else:
            header.append(c)
        i += 1
    return decls


if __name__ == '__main__':
    sys.stdout.reconfigure(encoding='utf-8', newline='\n')
    data = open(sys.argv[1], 'rb').read().decode('utf-8-sig')
    for d in census(data):
        print(d)
