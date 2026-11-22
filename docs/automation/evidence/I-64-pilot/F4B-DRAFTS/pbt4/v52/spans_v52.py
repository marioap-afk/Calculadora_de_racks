"""PB-T4d (paquete v5.2): las lineas de un miembro de tests/ en 6c2820ba, por objetos git (para la lista fila a fila).

Modulo: span(repo, path, member) -> (primera, ultima, skip) con la primera linea de sus atributos y su llave de cierre (un metodo, una propiedad
o un campo; `tipo:<Nombre>` es el tipo entero), y skip el origen del Skip que lleve ('PB-T4b', 'PB-T4c', 'PB-T3b', 'PB-TC', 'otro' o '-').
Las sobrecargas de un mismo nombre se dan juntas (de la primera a la ultima). Solo git show.
"""
import functools
import importlib.util
import os
import re
import subprocess

HERE = os.path.dirname(os.path.abspath(__file__))
_spec = importlib.util.spec_from_file_location('lists_core', os.path.join(os.path.dirname(HERE), 'lists_core.py'))
lc = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(lc)
REV = '6c2820bab4d8b41e3e229e23f3a1638ccbb04735'
FIELD = re.compile(r'^[ \t]*(?:(?:private|public|internal|protected|static|readonly|const)\s+)+[\w<>\[\],.?() \t]+?\s+(\w+)\s*(?:=(?!>)|;|\{\s*get|=>)', re.M)


@functools.lru_cache(maxsize=None)
def text(repo, path):
    raw = subprocess.run(['git', '-C', repo, 'show', REV + ':' + path], capture_output=True, check=True).stdout
    return raw.decode('utf-8-sig').replace('\r\n', '\n')


def _skip(attrs):
    m = re.search(r'Skip\s*=\s*"([^"]*)"', attrs)
    if not m:
        return '-'
    for tag in ('PB-T4b', 'PB-T4c', 'PB-T3b', 'PB-TC'):
        if tag in m.group(1):
            return tag
    return 'SUSTITUIDA' if m.group(1).startswith('SUSTITUIDA') else 'otro'


def _up(lines, a):
    """Sube desde la linea a (1-based) por los /// y los atributos."""
    while a > 1 and (lines[a - 2].strip().startswith('///') or lines[a - 2].strip().startswith('[')):
        a -= 1
    return a


def span(repo, path, member):
    code = text(repo, path)
    lines = code.split('\n')
    masked = lc.mask(code)
    if member.startswith('tipo:'):
        m = re.search(r'\b(?:class|struct|record)\s+' + re.escape(member[5:]) + r'\b', masked)
        assert m, (path, member)
        i = masked.index('{', m.end())
        d = 0
        for k in range(i, len(masked)):
            if masked[k] == '{':
                d += 1
            elif masked[k] == '}':
                d -= 1
                if d == 0:
                    break
        a = masked.count('\n', 0, m.start()) + 1
        return _up(lines, a), masked.count('\n', 0, k) + 1, '-'
    found = [m for m in lc.members(code) if m['name'] == member]
    if found:
        a = min(_up(lines, m['first']) for m in found)
        b = max(m['last'] for m in found)
        attrs = '\n'.join(lines[a - 1:min(m['first'] for m in found)])
        return a, b, _skip(attrs)
    # una declaracion cuyo tipo de vuelta es una tupla (lc.members no la ve): sus modificadores al principio de la linea, su nombre y su cuerpo
    decl = re.compile(r'^[ \t]*(?:(?:public|private|internal|protected|static|async|override|sealed|readonly|virtual)\s+)+[\w<>\[\],.?()\s]*?\b'
                      + re.escape(member) + r'\s*(?:<[^>()]*>)?\s*\(', re.M)
    dm = decl.search(masked)
    if dm:
        depth = masked[dm.start():dm.end()].count('(') - masked[dm.start():dm.end()].count(')')
        j = dm.end()
        end = None
        while j < len(masked) and end is None:
            ch = masked[j]
            if ch == '(':
                depth += 1
            elif ch == ')':
                depth -= 1
            elif depth == 0 and ch == '{':
                d = 0
                for k in range(j, len(masked)):
                    if masked[k] == '{':
                        d += 1
                    elif masked[k] == '}':
                        d -= 1
                        if d == 0:
                            end = k
                            break
            elif depth == 0 and masked.startswith('=>', j):
                end = masked.index(';', j)
            j += 1
        a = masked.count('\n', 0, dm.start()) + 1
        return _up(lines, a), masked.count('\n', 0, end) + 1, '-'
    for fm in FIELD.finditer(masked):
        if fm.group(1) == member:
            a = masked.count('\n', 0, fm.start()) + 1
            end = masked.find(';', fm.end() - 1)
            if fm.group(0).rstrip().endswith('get'):
                d = 0
                for k in range(masked.index('{', fm.start()), len(masked)):
                    if masked[k] == '{':
                        d += 1
                    elif masked[k] == '}':
                        d -= 1
                        if d == 0:
                            end = k
                            break
            return _up(lines, a), masked.count('\n', 0, end) + 1, '-'
    raise AssertionError(('sin miembro', path, member))
