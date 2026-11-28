"""El esqueleto exacto de PB-T4a (section 6.1 del paquete de PB-T4) como parche de git contra el cierre de PB-T3c (7f0d9aa3), para la compilacion de
comprobacion de la sesion (precondicion 6): lleva src/ y el Skip.
- WEI: el constructor privado con los dos argumentos nuevos, FullRead y Catalog como en D-PBT4-02, las cuatro fabricas de fallo y FromPilot con
  null, null, FromReads INERTE (sin comprobar nulos y con los dos campos en null), y los dos using nuevos (D-PBT4-02: Catalogs y PushBack.Draft);
- los archivos nuevos de src/ del alcance, cada uno con su regla (NEW_SRC_RULES): "completo" (el bloque del paquete tal cual: la interfaz de
  D-PBT4-03) o "inerte" (el bloque del GREEN con cada metodo publico estatico reducido al cuerpo que da la vineta del esqueleto de section 6.1:
  en la v4.1, Choose => null y AtKey => scan);
- WHC, RIA y NH: sin cambio (la vineta "WHC, RIA y NH: sin cambio" de section 6.1; si falta, aborta);
- PushBackDraftWriteGuardTests.cs:60: la linea del Skip, literal (la unica que cambia en el archivo).
B-08 (la v4.2): lo que puede cambiar va en los PARAMETROS de abajo, y nada mas. Un archivo nuevo de src/ en el alcance sin regla, un metodo publico
del GREEN sin cuerpo inerte en la vineta (por ejemplo ReadAt) o una vineta que no se lee, abortan.
Los textos literales salen del paquete (por su sha256) y los archivos de la base, por objetos git del checkout principal. Comprueba lo que fija el
paquete del esqueleto. Uso (desde hs65/pbt4): python -I make_pbt4a_skeleton.py <checkout principal>  -> escribe pbt4a-skeleton.patch"""
import difflib, hashlib, os, re, subprocess, sys

REPO = sys.argv[1]
REV = '7f0d9aa377ca1e3301bfce3716bd29d602d762dd'
HERE = os.path.dirname(os.path.abspath(__file__))
WEI = 'src/RackCad.Application/Workspace/WorkspaceEditIntent.cs'
IFACE = 'src/RackCad.Application/Workspace/IWorkspaceEditIntentOriginReader.cs'
BYTEXT = 'src/RackCad.Application/Systems/PushBack/Draft/PushBackOriginByText.cs'
WHC = 'src/RackCad.Application/Workspace/WorkspaceHostCore.cs'
RIA = 'src/RackCad.Plugin/Views/RackPilotIntentAdapter.cs'
NH = 'src/RackCad.Plugin/Workspace/WorkspaceNavigationHost.cs'
DWG = 'tests/RackCad.Tests/PushBackDraftWriteGuardTests.cs'

# ---------------------------------------------------------------- PARAMETROS (B-08: con la v4.2 se fijan aqui, y nada mas)
PKG_NAME = 'I-64-f4b-pbt4-gate-contract-package-v43.md'
PKG_VERSION = 'v4.3'
PKG_SHA = '4f4822798920d3f15e3c78f349b702549bef4eced6c5f26009252272247cb9d2'
# los archivos nuevos de src/ del alcance y su regla; la marca es el texto de la linea que precede a su bloque ```csharp en el paquete
NEW_SRC_RULES = {
    IFACE: ('completo', '**La interfaz nueva,** en `' + IFACE + '`'),
    BYTEXT: ('inerte', '**La funci' + chr(243) + 'n pura, en `' + BYTEXT + '`**'),
}
# la vineta del esqueleto de section 6.1 que da los cuerpos inertes de cada archivo "inerte" (su comienzo, sin la sangria)
INERT_BULLET = {BYTEXT: '- **`Draft/PushBackOriginByText.cs`:**'}
# la pureza de Draft/ (CT-PBT3A-14 y CT-PBT4A-08): ninguno en el texto del esqueleto de un archivo de Draft/
DRAFT_BAD = ('Task', 'Database', 'Transaction', 'File.')
# ----------------------------------------------------------------

raw = open(os.path.join(HERE, PKG_NAME), 'rb').read()
assert hashlib.sha256(raw).hexdigest() == PKG_SHA, 'el paquete no es la %s revisada' % PKG_VERSION
L = raw.decode('utf-8').split('\n')
_title = [l for l in L[:8] if l.startswith('# I-64 F4B')]
assert len(_title) == 1 and re.search(r', (v\d+(?:\.\d+)?)\s*$', _title[0]).group(1) == PKG_VERSION, ('la version del paquete', _title)


def show(path):
    r = subprocess.run(['git', '-C', REPO, 'show', REV + ':' + path], capture_output=True)
    assert r.returncode == 0, path
    return r.stdout.decode('utf-8')


def in_base(path):
    return subprocess.run(['git', '-C', REPO, 'cat-file', '-e', REV + ':' + path], capture_output=True).returncode == 0


def block_after(marker):
    """El bloque ```csharp que sigue a la unica linea que contiene marker (sus lineas, sin las vallas)."""
    ii = [k for k, l in enumerate(L) if marker in l]
    assert len(ii) == 1, ('la marca del bloque', marker, len(ii))
    a = next(k for k in range(ii[0], len(L)) if L[k].strip() == '```csharp')
    b = next(k for k in range(a + 1, len(L)) if L[k].strip() == '```')
    return L[a + 1:b]


def once(t, old, new):
    assert t.count(old) == 1, (old[:70], t.count(old))
    return t.replace(old, new)


# el alcance de section 6.1, en su orden, y los archivos nuevos de src/
_al = [k for k, l in enumerate(L) if l.startswith('- **`AllowedWriteScope`** (')]
assert len(_al) == 1, 'el AllowedWriteScope de section 6.1'
ALLOWED = []
for l in L[_al[0] + 1:]:
    if not l.startswith('  - '):
        break
    ALLOWED += re.findall(r'`((?:src|tests)/[^`]+)`', l)
assert ALLOWED[0] == WEI and ALLOWED[-1] == DWG and {WHC, RIA, NH} <= set(ALLOWED), ALLOWED
NEW_SRC = [p for p in ALLOWED if p.startswith('src/') and not in_base(p)]
assert set(NEW_SRC) == set(NEW_SRC_RULES), ('archivos nuevos de src/ del alcance sin regla de esqueleto (B-08: fijarla en NEW_SRC_RULES)',
                                             sorted(set(NEW_SRC) ^ set(NEW_SRC_RULES)))
assert [p for p in ALLOWED if p.startswith('src/') and in_base(p)] == [WEI, WHC, RIA, NH], 'los archivos existentes de src/ del alcance'
assert sum(1 for l in L if l.strip() == '- **WHC, RIA y NH:** sin cambio;') == 1, 'la vineta del esqueleto "WHC, RIA y NH: sin cambio" de section 6.1'


def inert_text(path, green):
    """El bloque del GREEN con sus metodos publicos estaticos reducidos, en su orden, al cuerpo que da la vineta del esqueleto."""
    bl = [l.strip() for l in L if l.strip().startswith(INERT_BULLET[path])]
    assert len(bl) == 1, ('la vineta del esqueleto', INERT_BULLET[path], len(bl))
    bodies = re.findall(r'`(\w+)` que devuelve `[^`]+` \(`(=> [^`]+;)`\)', bl[0])
    # el nombre es el primer identificador pegado a un "(" tras "public static ": tambien con un tipo de retorno en tupla, "(A a, B b)? ReadAt("
    idx = [(k, re.search(r'(\w+)\(', l[len('        public static '):])) for k, l in enumerate(green) if l.startswith('        public static ')]
    assert all(m for _, m in idx), ('un metodo publico del GREEN sin nombre legible', path)
    idx = [(k, m.group(1)) for k, m in idx]
    assert [n for _, n in idx] == [n for n, _ in bodies], ('los metodos publicos del GREEN y los cuerpos inertes de la vineta (B-08)', path,
                                                           [n for _, n in idx], [n for n, _ in bodies])
    assert green[idx[0][0] - 1] == '    {' and green[-2:] == ['    }', '}'], ('la forma del bloque del GREEN', path)
    out = green[:idx[0][0]]
    for j, ((k, name), (_, body)) in enumerate(zip(idx, bodies)):
        sig = green[k]
        depth, end = 0, None
        for q in range(sig.index(name + '('), len(sig)):
            depth += {'(': 1, ')': -1}.get(sig[q], 0)
            if sig[q] == ')' and depth == 0:
                end = q
                break
        assert end is not None and sig[end + 1:].strip() in ('', '{') and not sig[end + 1:].strip().startswith('=>'), ('la firma en una linea', sig)
        out += ([''] if j else []) + [sig[:end + 1] + ' ' + body]
    return out + ['    }', '}']


NEW_TEXT = {}
for p in NEW_SRC:
    rule, marker = NEW_SRC_RULES[p]
    G = block_after(marker)
    NEW_TEXT[p] = '\n'.join(G if rule == 'completo' else inert_text(p, G)) + '\n'
    if '/Draft/' in p:
        for bad in DRAFT_BAD:
            assert bad not in NEW_TEXT[p], ('la pureza de Draft/', p, bad)
    # CA-T4a I-01 (la regla de section 5.0 de la v4.3): sin #nullable enable, ningun '?' en un tipo de referencia (CS8632, que rompe build-plugin);
    # se mira el bloque del GREEN entero, no solo el esqueleto: string? y object?, tambien en un molde o en un argumento generico
    if '#nullable enable' not in '\n'.join(G):
        _q = [l.strip() for l in G if re.search(r'\b(?:string|object)\?', l)]
        assert not _q, ('CA-T4a I-01: un ? de tipo de referencia sin #nullable enable', p, _q)
assert NEW_TEXT[IFACE].startswith('#nullable enable\nnamespace RackCad.Application.Workspace\n') and 'EditIntentReadResult ReadIntentAtOrigin(SessionId ' \
    'session, string rackId, string originText, EditIntentWitness? witness);' in NEW_TEXT[IFACE]
# WEI: el esqueleto
w0 = show(WEI)
assert '\r' not in w0
w = once(w0, 'using RackCad.Application.Systems.PushBack.Pilot;\n',
         'using RackCad.Application.Catalogs;\nusing RackCad.Application.Systems.PushBack.Draft;\nusing RackCad.Application.Systems.PushBack.Pilot;\n')
GREEN = block_after('**En Application** (WEI), aditivo. El texto literal del GREEN:')
_ctor = '\n'.join(GREEN[:GREEN.index('        }') + 1])
w = once(w, '        private EditIntentReadResult(EditIntentReadStatus status, string? reason, PushBackPilotIntentResult? pilotRead)\n        {\n'
            '            Status = status;\n            Reason = reason;\n            PilotRead = pilotRead;\n        }', _ctor)
_props = '\n'.join(GREEN[GREEN.index('        }') + 2:GREEN.index('        public static EditIntentReadResult FromPilot(PushBackPilotIntentResult pilotRead)') - 1])
assert 'public PushBackFullIntentResult? FullRead { get; }' in _props and 'public RackCatalog? Catalog { get; }' in _props
w = once(w, '        public PushBackPilotIntentResult? PilotRead { get; }\n', '        public PushBackPilotIntentResult? PilotRead { get; }\n\n' + _props + '\n')
w = once(w, 'return new EditIntentReadResult(EditIntentReadStatus.Read, null, pilotRead);',
         'return new EditIntentReadResult(EditIntentReadStatus.Read, null, pilotRead, null, null);')
FROMREADS = ('        public static EditIntentReadResult FromReads(PushBackPilotIntentResult pilotRead, PushBackFullIntentResult fullRead, RackCatalog catalog) => '
             'new EditIntentReadResult(EditIntentReadStatus.Read, null, pilotRead, null, null);')
assert ('`' + FROMREADS.strip() + '`') in '\n'.join(L), 'FromReads inerte, literal de section 6.1'
for st in ('NoActiveDocument', 'Busy', 'Rejected', 'Failed'):
    w = once(w, 'new(EditIntentReadStatus.%s, reason, null);' % st, 'new(EditIntentReadStatus.%s, reason, null, null, null);' % st)
w = once(w, '        public static EditIntentReadResult NoActiveDocument(string reason)', FROMREADS + '\n\n        public static EditIntentReadResult NoActiveDocument(string reason)')
# el Skip, literal, en la linea 60
d0 = show(DWG)
dl = d0.split('\n')
assert dl[59] == '        [Fact]' and dl[60].strip() == 'public void CT_PBT3A_12_NoSourceFileOutsideTheFolderOfTheModelNamesItsTypes()'
SKIP_TEXT = ('[Fact(Skip = "I-64 F4B PB-T4a: sustituida por CT-PBT4A-06 de PushBackFullReadGuardTests (registro declarado superseding-characterization.md; '
             'decision del Owner owner-pbt4-decisions.md, pregunta 4).")]')
assert [l.strip() for l in L].count(SKIP_TEXT) == 1
dl[59] = '        ' + SKIP_TEXT
d = '\n'.join(dl)
assert all(ord(ch) < 0x80 for ch in SKIP_TEXT)


def blob(t):
    b = t.encode('utf-8')
    return hashlib.sha1(b'blob %d\0' % len(b) + b).hexdigest()


def mod_diff(path, a, b):
    body = ''.join(x if x.endswith('\n') else x + '\n' for x in difflib.unified_diff(a.splitlines(True), b.splitlines(True), 'a/' + path, 'b/' + path, n=3))
    return 'diff --git a/%s b/%s\nindex %s..%s 100644\n' % (path, path, blob(a)[:7], blob(b)[:7]) + body


def new_diff(path, t):
    lines = t.split('\n')[:-1]
    return ('diff --git a/%s b/%s\nnew file mode 100644\nindex 0000000..%s\n--- /dev/null\n+++ b/%s\n@@ -0,0 +1,%d @@\n' % (path, path, blob(t)[:7], path, len(lines))
            + ''.join('+' + l + '\n' for l in lines))


patch = mod_diff(WEI, w0, w) + ''.join(new_diff(p, NEW_TEXT[p]) for p in NEW_SRC) + mod_diff(DWG, d0, d)
assert '\r' not in patch
_touched = re.findall(r'(?m)^diff --git a/(\S+) b/', patch)
assert _touched == [WEI] + NEW_SRC + [DWG] and set(_touched) <= set(ALLOWED), _touched
dd = [l for l in difflib.unified_diff(d0.split('\n'), d.split('\n'), lineterm='', n=0) if l[:1] in '+-' and not l.startswith(('+++', '---'))]
assert dd == ['-        [Fact]', '+        ' + SKIP_TEXT], 'el Skip: la unica linea que cambia en PushBackDraftWriteGuardTests.cs'
out = os.path.join(HERE, 'pbt4a-skeleton.patch')
open(out, 'wb').write(patch.encode('utf-8'))
print('paquete %s (%s) | pbt4a-skeleton.patch %d bytes, sha256 %s | WEI %s | %s | el Skip %s'
      % (PKG_VERSION, PKG_SHA[:8], len(patch.encode('utf-8')), hashlib.sha256(patch.encode('utf-8')).hexdigest(), blob(w)[:8],
         ' | '.join('%s (%s) %s' % (os.path.basename(p), NEW_SRC_RULES[p][0], blob(NEW_TEXT[p])[:8]) for p in NEW_SRC), blob(d)[:8]))
