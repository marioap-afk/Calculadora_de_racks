"""Comprobaciones de INV-PBT4E (solo lectura; rutas relativas al worktree; salida UTF-8).
Uso: python -I check_inv.py <BaseSha> <RedSha> <Actual>   con Actual = un SHA, o WORKTREE (el arbol de trabajo frente al indice/HEAD)."""
import hashlib
import importlib.util
import os
import re
import subprocess
import sys

A = 'artifacts/orchestration/I-64/F4B-PBT4-EDIT-ALL-RELOAD/1/R20261123T045827Z-6c9e'
SRC4 = [
    'src/RackCad.Application/Workspace/IWorkspacePilotEditPort.cs',
    'src/RackCad.Application/Workspace/WorkspaceEditIntent.cs',
    'src/RackCad.Application/Workspace/WorkspaceHostCore.cs',
    'src/RackCad.UI/Workspace/WorkspacePilotFieldViewModel.cs',
]
GREEN_SHA = {
    'src/RackCad.Application/Workspace/IWorkspacePilotEditPort.cs': '49b5245c112e3d4f0b3b95114f9ff0444469b316a3ef912947cf3b696e0c6ebd',
    'src/RackCad.Application/Workspace/WorkspaceEditIntent.cs': 'bbc6882283833b02bab79c9bb70c082f3ac14b2a88650722bf8d72be0e13b5e2',
    'src/RackCad.Application/Workspace/WorkspaceHostCore.cs': '6eb9bf7ab81666b33c1280c832257624937c95a042e46f53dcc2c371baac4d7d',
    'src/RackCad.UI/Workspace/WorkspacePilotFieldViewModel.cs': 'c4b5c6990c5623e53be607fdbb461db95103b957c96627e1d63300dabe3d643b',
}
NEWTESTS = ['tests/RackCad.Tests/PushBackEditAllReloadTests.cs', 'tests/RackCad.UI.Tests/PushBackEditAllReloadFieldTests.cs']
NOT_TOUCH = ['src/RackCad.Domain', 'src/RackCad.Application/Systems/PushBack/Pilot/', 'src/RackCad.Plugin/RackPushBackCommands.cs',
             'src/RackCad.Plugin/RackCommandSupport.cs', 'src/RackCad.Plugin/Views/RackPilotIntentAdapter.cs', 'src/RackCad.Plugin/Workspace/WorkspaceNavigationHost.cs']


def git(*args, raw=False):
    r = subprocess.run(['git'] + list(args), capture_output=True, check=True)
    return r.stdout if raw else r.stdout.decode('utf-8')


def sha_text(raw):
    text = raw.decode('utf-8-sig').replace('\r\n', '\n')
    return hashlib.sha256(text.encode('utf-8')).hexdigest()


def content(rev, path):
    if rev == 'WORKTREE':
        return open(path, 'rb').read()
    return git('show', '%s:%s' % (rev, path), raw=True)


def main():
    sys.stdout.reconfigure(encoding='utf-8', newline='\n')
    base, red, cur = sys.argv[1:4]
    ok = True

    def check(name, cond, detail=''):
        nonlocal ok
        ok = ok and cond
        print('%s %s %s' % ('PASS' if cond else 'FAIL', name, detail))

    dargs = [] if cur == 'WORKTREE' else [cur]
    # INV-PBT4E-01 (a): RedSha..Actual en src y tests
    names = [l for l in git('diff', '--name-only', red, *dargs, '--', 'src', 'tests').split('\n') if l]
    check('INV-01 RedSha..Actual (src, tests) = los 4 de src, nada de tests', sorted(names) == sorted(SRC4), str(sorted(names)))
    # (b) BaseSha..Actual
    allnames = [l for l in git('diff', '--name-only', base, *dargs).split('\n') if l]
    print('FilesChanged BaseSha..Actual:', sorted(allnames))
    check('INV-01 BaseSha..Actual = 6 rutas exactas', sorted(allnames) == sorted(SRC4 + NEWTESTS))
    nt = git('diff', '--name-status', base, *dargs, '--', 'tests')
    check('INV-07 BaseSha..Actual -- tests solo anade los 2 archivos nuevos', sorted(l.strip() for l in nt.split('\n') if l) == sorted('A\t' + p for p in NEWTESTS), nt.strip().replace('\n', ' ; '))
    nd = git('diff', '--name-only', base, *dargs, '--', *NOT_TOUCH)
    check('no-touch: Domain, Pilot/, RackPushBackCommands, RackCommandSupport, RackPilotIntentAdapter, WorkspaceNavigationHost vacio', nd.strip() == '', nd.strip())
    # RED literal
    red_lines = [l.split() for l in open(A + '/inputs/red-files.txt', encoding='utf-8').read().split('\n') if l.strip()]
    for sha, path in red_lines:
        got = sha_text(content(cur, path))
        check('RED literal %s' % path, got == sha, got)
        gotred = sha_text(content(red, path))
        check('RED literal en RedSha %s' % path, gotred == sha, gotred)
    # GREEN literal
    for path in SRC4:
        got = sha_text(content(cur, path))
        check('GREEN literal %s' % path, got == GREEN_SHA[path], got)
        # src en RedSha = base
        check('src de RedSha = BaseSha %s' % path, content(red, path) == content(base, path))
    # censo
    spec = importlib.util.spec_from_file_location('census', A + '/inv/census.py')
    cm = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(cm)
    allb, allc = [], []
    for path in SRC4:
        b = cm.census(content(base, path).decode('utf-8-sig'))
        c = cm.census(content(cur, path).decode('utf-8-sig'))
        allb += ['%s | %s' % (os.path.basename(path), d) for d in b]
        allc += ['%s | %s' % (os.path.basename(path), d) for d in c]
        print('censo %s: BaseSha=%d Actual=%d' % (os.path.basename(path), len(b), len(c)))
    from collections import Counter
    cb, cc = Counter(allb), Counter(allc)
    added = sorted((cc - cb).elements())
    removed = sorted((cb - cc).elements())
    print('declaraciones anadidas:', added)
    print('declaraciones quitadas:', removed)
    check('PBT4E-C13 censo: solo NotReloadedText nueva, ninguna quitada',
          added == ['WorkspacePilotFieldViewModel.cs | WorkspacePilotFieldViewModel::const NotReloadedText'] and not removed)
    # D2: lineas anadidas con codigo
    diff = git('diff', '-U0', base, *dargs, '--', 'src')
    code = []
    for l in diff.split('\n'):
        if l.startswith('+') and not l.startswith('+++'):
            s = l[1:].strip()
            if s and not s.startswith('//') and not s.startswith('///'):
                code.append(s)
    print('lineas anadidas con codigo (D2):')
    for s in code:
        print('   +', s)
    bad = [s for s in code if re.search(r'PushBackSystem|RackModuleReconciliationResult|\.Add\(|\.Remove\(|\.Clear\(', s)]
    check('D2: ninguna linea anadida nombra ni muta un PushBackSystem o un RackModuleReconciliationResult', not bad, str(bad))
    # warning CS en todos los registros de build/test
    total = 0
    for dirpath, _, files in os.walk(A + '/tests'):
        for fn in files:
            if fn.endswith(('.log', '.txt')):
                p = os.path.join(dirpath, fn)
                try:
                    n = len(re.findall(r'warning CS\d+', open(p, encoding='utf-8', errors='replace').read()))
                except OSError:
                    n = 0
                if n:
                    print('   warning CS en', p, n)
                total += n
    check('0 lineas "warning CS<n>" en los registros de tests/', total == 0, str(total))
    print('RESULTADO', 'PASS' if ok else 'FAIL')


if __name__ == '__main__':
    main()
