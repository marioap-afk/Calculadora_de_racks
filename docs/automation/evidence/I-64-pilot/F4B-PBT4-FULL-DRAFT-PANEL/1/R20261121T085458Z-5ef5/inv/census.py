"""Censo de declaraciones (PBT4B-C13; ExpectedEvidence [11]): las declaraciones de los diez archivos de src/ de PB-T4b en dos revisiones,
contadas una a una con su nombre (una sobrecarga es una declaracion mas). Solo lectura: `git show` de objetos git.
Uso: python -I census.py <repo> <rev-base> <rev-actual> [--all]
Imprime, por archivo, las dos listas (nombre: cantidad) y la diferencia (lo que se anade y lo que se quita)."""
import collections
import importlib.util
import os
import re
import subprocess
import sys

HERE = os.path.dirname(os.path.abspath(__file__))
GEN = os.path.join(os.path.dirname(HERE), 'gen')

FILES = [
    'src/RackCad.Application/Workspace/WorkspaceHostCore.cs',
    'src/RackCad.Application/Workspace/WorkspaceEditIntent.cs',
    'src/RackCad.UI/Workspace/WorkspacePilotFieldViewModel.cs',
    'src/RackCad.UI/Workspace/WorkspacePanelView.xaml',
    'src/RackCad.UI/Controls/NumericFieldValidation.cs',
    'src/RackCad.Plugin/Views/RackPilotIntentAdapter.cs',
    'src/RackCad.Application/Formatting/NumericTextRule.cs',
    'src/RackCad.Application/Systems/PushBack/Draft/PushBackAnnotationScaleRule.cs',
    'src/RackCad.Application/Systems/PushBack/Draft/PushBackFullFieldAdapter.cs',
    'src/RackCad.Application/Systems/PushBack/Draft/PushBackKeepMinePresentation.cs',
]

# La lista cerrada de PBT4B-C13 (por archivo): lo unico que puede aparecer de mas en el codigo.
CLOSED = {
    'src/RackCad.Application/Workspace/WorkspaceEditIntent.cs': collections.Counter(
        {'Presented': 2, 'EditIntentOutcome': 1, '_presentationLines': 1, 'PresentationLines': 1, 'KeepWitness': 1}),
    'src/RackCad.Application/Workspace/WorkspaceHostCore.cs': collections.Counter({'FullReadMissingReason': 1}),
    'src/RackCad.UI/Workspace/WorkspacePilotFieldViewModel.cs': collections.Counter(
        {'_keepWitness': 1, '_keepLines': 1, 'KeepMinePresentationText': 1, 'ShowKeepMinePresentation': 1}),
    'src/RackCad.UI/Workspace/WorkspacePanelView.xaml': collections.Counter({'x:Name=BrowserFieldKeepMinePresentationText': 1}),
    'src/RackCad.Plugin/Views/RackPilotIntentAdapter.cs': collections.Counter({'RackPilotIntentReadResult': 1, '_full': 1}),
    'src/RackCad.UI/Controls/NumericFieldValidation.cs': collections.Counter(),
    'src/RackCad.Application/Formatting/NumericTextRule.cs': collections.Counter(
        {'NumericTextStatus': 1, 'Valid': 1, 'EmptyOptional': 1, 'EmptyRequired': 1, 'NotANumber': 1, 'BelowMinimum': 1, 'AboveMaximum': 1,
         'NumericTextCheck': 2, 'Status': 1, 'Value': 1, 'Message': 1, 'IsValid': 1, 'NumericTextRule': 1,
         'EmptyRequiredMessage': 1, 'NotANumberMessage': 1, 'Check': 1, 'MinimumMessage': 1, 'MaximumMessage': 1, 'Format': 1}),
    'src/RackCad.Application/Systems/PushBack/Draft/PushBackAnnotationScaleRule.cs': collections.Counter(
        {'PushBackAnnotationScaleRule': 1, 'Minimum': 1, 'MinimumInclusive': 1, 'Validate': 1}),
    'src/RackCad.Application/Systems/PushBack/Draft/PushBackFullFieldAdapter.cs': collections.Counter(
        {'PushBackFullFieldAdapter': 1, 'AnnotationScaleKey': 1, 'EmptyMessage': 1, 'NotANumberMessage': 1, 'NotPositiveMessage': 1,
         'UnknownFieldMessage': 1, 'Validate': 1, 'Apply': 1}),
    'src/RackCad.Application/Systems/PushBack/Draft/PushBackKeepMinePresentation.cs': collections.Counter(
        {'PushBackKeepMinePresentation': 2, 'Lines': 1, 'IsEmpty': 1, 'OriginText': 1, 'Diagnostic': 1, 'IsAdmitted': 1,
         'Of': 1, 'Read': 1, 'AddGroups': 1, 'Join': 1, 'TextOf': 1}),
}


# Lo que PBT4B-C13 mueve fuera de un archivo (no es una declaracion nueva): NumericFieldValidation.Validate delega y sus tres privados pasan a NumericTextRule.
REMOVED = {'src/RackCad.UI/Controls/NumericFieldValidation.cs': collections.Counter({'MinimumMessage': 1, 'MaximumMessage': 1, 'Format': 1})}


def load(name):
    spec = importlib.util.spec_from_file_location(name, os.path.join(GEN, name + '.py'))
    mod = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(mod)
    return mod


lc = load('lists_core')
mk = load('make_copies_pbt4b')
TYPE_RX = re.compile(r'\b(class|struct|enum|interface|record)\s+(\w+)')


def show(repo, rev, path):
    p = subprocess.run(['git', '-C', repo, 'show', '%s:%s' % (rev, path)], capture_output=True)
    if p.returncode:
        return None
    return p.stdout.decode('utf-8-sig').replace('\r\n', '\n')


def body_span(masked, start):
    """El tramo (despues de '{', antes de su '}') del primer bloque que abre desde start, o None si antes hay un ';'."""
    i = start
    while i < len(masked) and masked[i] not in '{;':
        i += 1
    if i >= len(masked) or masked[i] == ';':
        return None
    depth, j = 0, i
    while j < len(masked):
        if masked[j] == '{':
            depth += 1
        elif masked[j] == '}':
            depth -= 1
            if depth == 0:
                return i + 1, j
        j += 1
    return None


def enum_members(masked, a, b):
    names = []
    for part in masked[a:b].split(','):
        part = re.sub(r'\[[^\]]*\]', '', part).strip()
        m = re.match(r'(\w+)', part)
        if m:
            names.append(m.group(1))
    return names


def declarations(text):
    """Contador nombre -> cantidad de las declaraciones de un archivo de C#: tipos, enumerados, miembros de tipo (constructores, metodos,
    propiedades, campos, constantes) y tipos anidados."""
    code = text
    masked = lc.mask(code)
    out = collections.Counter()
    for m in TYPE_RX.finditer(masked):
        kind, name = m.group(1), m.group(2)
        out[name] += 1
        span = body_span(masked, m.end())
        if span is None:
            continue
        a, b = span
        if kind == 'enum':
            for n in enum_members(masked, a, b):
                out[n] += 1
            continue
        members, tail = mk.segments(code, masked, a, b)
        for s, e in members:
            seg_masked = masked[s:e]
            lines = [l for l in seg_masked.split('\n') if l.strip() and not re.match(r'^\s*\[', l)]
            decl = ' '.join(lines)
            tm = TYPE_RX.search(decl)
            if tm and not re.search(r'[(=]', decl[:tm.start()]):
                continue  # un tipo anidado: lo cuenta la busqueda de tipos
            try:
                out[mk.declared_name(code[s:e])] += 1
            except AssertionError:
                out['<?>' + decl[:60]] += 1
    return out


def xaml_names(text):
    return collections.Counter('x:Name=' + n for n in re.findall(r'x:Name="(\w+)"', text))


def census(repo, rev, path):
    text = show(repo, rev, path)
    if text is None:
        return None
    return xaml_names(text) if path.endswith('.xaml') else declarations(text)


def main():
    sys.stdout.reconfigure(encoding='utf-8', errors='replace')
    repo, base, cur = sys.argv[1:4]
    ok = True
    total_added = collections.Counter()
    for path in FILES:
        before = census(repo, base, path) or collections.Counter()
        after = census(repo, cur, path) or collections.Counter()
        added = after - before
        removed = before - after
        print('==', path)
        print('   base   : %d declaraciones' % sum(before.values()))
        print('   actual : %d declaraciones' % sum(after.values()))
        print('   se anade: %s' % (dict(sorted(added.items())) or '{}'))
        print('   se quita: %s' % (dict(sorted(removed.items())) or '{}'))
        closed = CLOSED[path]
        same = (added == closed) and removed == REMOVED.get(path, collections.Counter())
        ok = ok and same
        print('   igual a la lista cerrada de PBT4B-C13: %s' % same)
        if not same:
            print('   lista cerrada: %s' % dict(sorted(closed.items())))
        total_added += added
        if '--all' in sys.argv:
            print('   base completa  : %s' % dict(sorted(before.items())))
            print('   actual completa: %s' % dict(sorted(after.items())))
    print('RESULTADO censo: %s' % ('IGUAL a la lista cerrada' if ok else 'DISTINTO'))
    sys.exit(0 if ok else 1)


if __name__ == '__main__':
    main()
