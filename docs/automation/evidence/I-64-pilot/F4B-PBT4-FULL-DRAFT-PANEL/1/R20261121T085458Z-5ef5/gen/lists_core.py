"""Nucleo comun de las listas de PB-T4 (v4.1): lectura por objetos git y miembros C# con su texto.

Solo llama a `git -C <checkout> show|ls-tree`. Fija REPO y REV antes de usarlo.
El texto de C# se recorre con un lexico minimo: comentarios, cadenas normales, verbatim (@"..."),
interpoladas ($"...", $@"...", @$"..."), crudas (\"\"\"...\"\"\") y caracteres ('{') se vacian
sin mover las lineas, para que las llaves de una cadena no cuenten al delimitar un metodo.
"""
import re
import subprocess

REPO = None
REV = None


def git(*args):
    p = subprocess.run(['git', '-C', REPO] + list(args), capture_output=True)
    if p.returncode != 0:
        raise SystemExit('git %s -> %d' % (' '.join(args), p.returncode))
    return p.stdout.decode('utf-8', 'replace').replace('\r\n', '\n')


def show(path):
    return git('show', '%s:%s' % (REV, path))


def ls(prefix):
    return [p for p in git('ls-tree', '-r', '--name-only', REV, prefix).splitlines() if p.endswith('.cs')]


def _blank(s):
    return re.sub(r'[^\n]', ' ', s)


def blank_comments(code):
    """Vacia comentarios; deja las cadenas (el texto de una cadena cuenta para las firmas)."""
    return mask(code, keep_strings=True)


def mask(code, keep_strings=False):
    """Vacia comentarios y, salvo keep_strings, el interior de cadenas y caracteres. Conserva las lineas."""
    out = []
    i, n = 0, len(code)
    while i < n:
        c = code[i]
        two = code[i:i + 2]
        if two == '//':
            j = code.find('\n', i)
            j = n if j < 0 else j
            out.append(_blank(code[i:j]))
            i = j
            continue
        if two == '/*':
            j = code.find('*/', i + 2)
            j = n if j < 0 else j + 2
            out.append(_blank(code[i:j]))
            i = j
            continue
        # cadena cruda de C# 11: """…""" o $"""…"""; nunca con @ (@""" es una cadena verbatim que empieza por una comilla escapada)
        m = re.match(r'(\$*)("{3,})', code[i:i + 12])
        if m and len(m.group(2)) >= 3 and not (i > 0 and code[i - 1] == '@'):
            q = m.group(2)
            start = i + len(m.group(0))
            j = code.find(q, start)
            j = n if j < 0 else j + len(q)
            seg = code[i:j]
            out.append(seg if keep_strings else m.group(0) + _blank(code[start:j - len(q)]) + q)
            i = j
            continue
        m = re.match(r'(\$@|@\$|@|\$)?"', code[i:i + 3])
        if m and (i == 0 or not (code[i - 1].isalnum() or code[i - 1] == '_') or m.group(1)):
            pre = m.group(1) or ''
            verbatim = '@' in pre
            interp = '$' in pre
            j = i + len(m.group(0))
            depth = 0
            while j < n:
                ch = code[j]
                if interp and ch == '{':
                    if code[j:j + 2] == '{{':
                        j += 2
                        continue
                    depth += 1
                elif interp and ch == '}' and depth:
                    depth -= 1
                elif depth == 0:
                    if verbatim and ch == '"':
                        if code[j:j + 2] == '""':
                            j += 2
                            continue
                        break
                    if not verbatim and ch == '\\':
                        j += 2
                        continue
                    if not verbatim and ch == '"':
                        break
                    if not verbatim and ch == '\n':
                        break
                j += 1
            j = min(j + 1, n)
            seg = code[i:j]
            out.append(seg if keep_strings else seg[:len(m.group(0))] + _blank(seg[len(m.group(0)):-1]) + seg[-1:])
            i = j
            continue
        if c == "'":
            m = re.match(r"'(?:\\.|[^'\\\n])'", code[i:i + 10])
            if m:
                seg = m.group(0)
                out.append(seg if keep_strings else "'" + ' ' * (len(seg) - 2) + "'")
                i += len(seg)
                continue
        out.append(c)
        i += 1
    return ''.join(out)


SIG = re.compile(r'(\[(?:Fact|Theory)[^\]]*\][^\n]*\n(?:\s*\[[^\]]*\]\s*\n)*)?\s*(?:(?:public|private|internal|protected|static|async|override|sealed)\s+)+[\w<>\[\],.?\s]+?\s+(\w+)\s*\([^;{]*?\)\s*(\{|=>)', re.S)


def members(code):
    """Miembros con cuerpo. Las llaves se cuentan sobre el texto enmascarado (sin cadenas ni comentarios);
    el texto que se devuelve es el de `code` (con cadenas) en el mismo tramo."""
    masked = mask(code)
    res = []
    for m in SIG.finditer(masked):
        name, kind = m.group(2), m.group(3)
        start = m.end()
        if kind == '{':
            depth, i = 1, start
            while i < len(masked) and depth:
                depth += {'{': 1, '}': -1}.get(masked[i], 0)
                i += 1
            end = i
        else:
            # cuerpo de expresión: hasta el primer ';' fuera de paréntesis, llaves y corchetes (una lambda con bloque no lo corta)
            depth, i = 0, start
            while i < len(masked):
                ch = masked[i]
                if ch in '({[':
                    depth += 1
                elif ch in ')}]':
                    depth -= 1
                elif ch == ';' and depth <= 0:
                    break
                i += 1
            end = i + 1
        first = masked.count('\n', 0, m.start() + len(m.group(0)) - len(m.group(0).lstrip())) + 1
        last = masked.count('\n', 0, end) + 1
        attrs = code[m.start(1):m.end(1)] if m.group(1) else ''
        res.append({'name': name, 'test': bool(m.group(1)), 'attrs': attrs, 'body': code[start:end], 'first': first, 'last': last})
    return res


def tests_with_text(path):
    """Cada prueba con su texto: sus atributos y su cuerpo, mas el cuerpo de los ayudantes de su archivo que nombra, a un nivel."""
    ms = members(blank_comments(show(path)))
    helpers = {}
    for m in ms:
        if not m['test']:
            helpers.setdefault(m['name'], []).append(m['body'])
    out = []
    for m in ms:
        if not m['test']:
            continue
        called = [h for h in helpers if h != m['name'] and re.search(r'\b' + re.escape(h) + r'\s*[(<]', m['body'])]
        out.append((m, m['attrs'] + m['body'] + '\n' + '\n'.join(b for h in called for b in helpers[h])))
    return out


def method_at(path, line):
    for m in members(blank_comments(show(path))):
        if m['test'] and m['first'] <= line <= m['last']:
            return m['name']
    return None
