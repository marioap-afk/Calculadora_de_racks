# -*- coding: utf-8 -*-
# Comprueba los rangos de INV-PBT1C-03 (v2.8, V28-08) sobre W y WC de 2f5d3dec (leidos por objetos git desde el checkout
# principal y guardados en _v28src/) y los aplica a las dos variantes de la simulacion mecanica de D-PBT1-13
# (pbt1c/sim/W-A, W-B, WC-A, WC-B). Usa `git diff --no-index -U0` sobre archivos de esta carpeta (no toca ningun
# repositorio) y la definicion de [PKC2] INV-PBTC2-03: tocar un miembro es cambiar o quitar una linea de su rango, o
# insertar dentro de el (hunk @@ -a,0, s <= a < e); una insercion entre dos miembros es una declaracion nueva y solo se
# admite si sus lineas son la de SyncName o la de EditableStateForTest.
import os
import re
import subprocess
import sys
from collections import Counter

HERE = os.path.dirname(os.path.abspath(__file__))
SRC = os.path.join(HERE, '_v28src')
SIM = os.path.join(os.path.dirname(HERE), 'pbt1c', 'sim')

W = {
    'composite (D-PBT1-13 (1), con su documentacion)': (49, 57),
    'safetySelections': (84, 84),
    'generalPallet (con su documentacion)': (94, 101),
    'rackTolerance, rackPostPeralte, rackBeamDepth y rackAnnotationScale (con su comentario)': (103, 109),
    'advanced (con su documentacion)': (111, 114),
    'LoadNew (con su documentacion)': (268, 289),
    'LoadDesignForNew (con su documentacion)': (291, 307),
    'LoadExisting (con su documentacion)': (309, 324),
    'SideAOnly, retirado (documentacion y las dos lineas en blanco que lo rodean)': (325, 374),
    'firstLevelDatum (con su documentacion)': (375, 380),
    'LoadFromModel': (385, 440),
    'ReadInputs': (833, 890),
    'Name_Changed': (1234, 1238),
    'loadedDimensionViews (con su documentacion)': (1244, 1249),
    'loadedDimensionStyle (con su documentacion)': (1251, 1256),
    'dimensionViewsTouched (con su documentacion)': (1258, 1260),
    'SelectedDimensionViews, retirado (documentacion y las dos lineas en blanco que lo rodean)': (1288, 1298),
    'Restore_Click (con su documentacion)': (3304, 3330),
    'InsertBatch_Click': (3334, 3361),
    'RequestDraw': (3363, 3419),
    'SaveLibrary_Click': (3481, 3515),
}
WC = {
    'CompositeDiagnostics (con su documentacion)': (779, 786),
    'CompositeStatusOr (con su documentacion)': (788, 801),
    'CompositeHasBlocking': (803, 807),
    'los dos LoadCompositeFromDesign, retirados (con su documentacion y sus lineas en blanco)': (811, 934),
}
NEW_DECL = (re.compile(r'^\s*private void SyncName\(\) => editable\.Name = session\.Identity\.Name;\s*$'),
            re.compile(r'^\s*internal PushBackEditableState EditableStateForTest => editable;\s*$'))
ANCHOR_TEXT = {
    'W': {49: '/// <summary>', 56: 'private readonly PushBackCompositeEditorState composite =', 84: 'safetySelections =',
          94: '/// <summary>', 101: 'private PalletSpecification generalPallet', 103: '// I-64 F4B PB-TC', 109: 'rackAnnotationScale = 1.0;',
          111: '/// <summary>Carrier', 114: 'private readonly PushBackEditorInputs advanced', 268: '/// <summary>A brand-new',
          270: 'public void LoadNew()', 291: '/// <summary>A design opened', 294: 'public void LoadDesignForNew(',
          309: '/// <summary>A system opened', 311: 'public void LoadExisting(', 326: '/// <summary>', 342: 'private static PushBackDesign SideAOnly(',
          375: '/// <summary>', 380: 'private int? firstLevelDatum', 385: 'private void LoadFromModel(', 405: '// I-35 (Owner round 2)',
          833: 'private PushBackEditorInputs ReadInputs()', 1234: 'private void Name_Changed(', 1244: '/// <summary>',
          1249: 'private DimensionViewVisibility? loadedDimensionViews;', 1251: '/// <summary>', 1256: 'private string loadedDimensionStyle;',
          1258: '/// <summary>Si el USUARIO', 1260: 'private bool dimensionViewsTouched;', 1270: 'loadedDimensionViews = views;',
          1271: 'dimensionViewsTouched = false;', 1289: '/// <summary>La pol', 1291: 'private DimensionViewVisibility? SelectedDimensionViews()',
          1303: 'dimensionViewsTouched = true;', 3304: '/// <summary>REAL restore', 3307: 'private void Restore_Click(',
          3334: 'private void InsertBatch_Click(', 3363: 'private void RequestDraw(', 3481: 'private void SaveLibrary_Click('},
    'WC': {779: '/// <summary>', 783: 'CompositeDiagnostics()', 788: '/// <summary>El mensaje', 789: 'private string CompositeStatusOr(',
           803: 'private bool CompositeHasBlocking(', 812: '/// <summary>Recupera', 813: 'private void LoadCompositeFromDesign(PushBackDesign design)',
           816: '/// <summary>', 827: 'private void LoadCompositeFromDesign(PushBackDesign design, bool preserveDormantIntent)', 934: '}'},
}


def lines(path):
    with open(path, encoding='utf-8') as f:
        return f.read().splitlines()


def blank(s):
    return s.strip() == ''


def check_ranges(name, text, ranges):
    out = []
    # los bordes: antes de cada rango, una linea en blanco o la llave de cierre; despues, igual (salvo WC:934)
    for label, (s, e) in sorted(ranges.items(), key=lambda kv: kv[1]):
        before = text[s - 2] if s >= 2 else ''
        after = text[e] if e < len(text) else ''
        ok_before = blank(before) or before.strip().endswith(('}', ';')) or blank(text[s - 1])
        ok_after = blank(after) or after.strip() in ('}',) or blank(text[e - 1])
        out.append('%s:%d-%d %s | antes=%r despues=%r %s' % (name, s, e, label, before.strip()[:40], after.strip()[:40],
                                                            'OK' if ok_before and ok_after else 'REVISAR'))
    return out


def hunks(a, b):
    r = subprocess.run(['git', 'diff', '--no-index', '-U0', '--no-color', a, b], capture_output=True)
    text = r.stdout.decode('utf-8')
    res = []
    cur = None
    for line in text.splitlines():
        m = re.match(r'^@@ -(\d+)(?:,(\d+))? \+(\d+)(?:,(\d+))? @@', line)
        if m:
            cur = dict(a=int(m.group(1)), n=int(m.group(2)) if m.group(2) is not None else 1, added=[])
            res.append(cur)
        elif cur is not None and line.startswith('+') and not line.startswith('+++'):
            cur['added'].append(line[1:])
    return res


def judge(hs, ranges):
    bad = []
    for h in hs:
        a, n = h['a'], h['n']
        if n > 0:
            span = range(a, a + n)
            outside = [x for x in span if not any(s <= x <= e for s, e in ranges.values())]
            if outside:
                bad.append(('@@ -%d,%d' % (a, n), 'lineas fuera', outside))
        else:
            inside = any(s <= a < e for s, e in ranges.values())
            if not inside:
                added = [x for x in h['added'] if not blank(x)]
                if not (len(added) == 1 and any(p.match(added[0]) for p in NEW_DECL)):
                    bad.append(('@@ -%d,0' % a, 'insercion entre miembros que no es SyncName ni EditableStateForTest', added))
    return bad


# v2.8, V28-15 (PBT1CV28-M01): en los rangos que se retiran, los hunks solo quitan lineas; en estos cuatro metodos, el unico
# hunk es la insercion de `SyncName();` justo despues de la linea dada.
RETIRED = {'W': [(325, 374), (1288, 1298)], 'WC': [(811, 934)]}
SYNC_ONLY = {'Name_Changed': ((1234, 1238), 1237), 'InsertBatch_Click': ((3334, 3361), 3352),
             'RequestDraw': ((3363, 3419), 3405), 'SaveLibrary_Click': ((3481, 3515), 3504)}
# v2.8, V28-16 y V28-30 (PBT1CV28-M02 y PBT1CV28R-I01): las subcadenas testigo, ASCII, de la documentacion de lo que se retira no quedan
# en CurrentSha.
WITNESS = {'W': ['del lado A SOLO, sin la parte compuesta.', 'que se guarda: la cargada, exacta, si el usuario no'],
           'WC': ['Recupera la intencion compuesta de un diseno cargado', 'CARGAR OTRO RACK Y RESTAURAR EL ACTUAL NO SON LO MISMO']}
# sin `new`: una linea de continuacion de un inicializador (`new PushBackCompositeEditorState(...)`, W:57) no es una declaracion
MODS = r'(?:public|private|internal|protected|static|readonly|sealed|override|virtual|abstract|partial|async|const|event|unsafe|extern|volatile)'
DECL = re.compile(r'^\s*(?:\[[^\]]*\]\s*)*(?:' + MODS + r'\s+)+(?:class\s+(\w+)|[^=;({]*?\b(\w+)\s*(?:<[^>]*>)?\s*(?:\(|=>|=|;|\{|$))')


def strip(text):
    text = re.sub(r'@"(?:[^"]|"")*"', '""', text)
    text = re.sub(r'\$?"(?:\\.|[^"\\\n])*"', '""', text)
    text = re.sub(r"'(?:\\.|[^'\\\n])'", "''", text)
    text = re.sub(r'/\*.*?\*/', ' ', text, flags=re.S)
    return re.sub(r'//[^\n]*', '', text)


def member_names(text):
    # declaraciones de miembro de la clase parcial: las lineas que empiezan a profundidad 2 (namespace y clase)
    names = []
    depth = 0
    for line in strip(text).split('\n'):
        if depth == 2:
            m = DECL.match(line)
            if m:
                names.append(m.group(1) or m.group(2))
        depth += line.count('{') - line.count('}')
    return names


def m01(hs, name):
    bad = []
    for h in hs:
        a, n = h['a'], h['n']
        span = range(a, a + n) if n > 0 else []
        if any(s <= x <= e for s, e in RETIRED[name] for x in span) and [x for x in h['added'] if not blank(x)]:
            bad.append(('@@ -%d,%d' % (a, n), 'anade lineas en un rango que se retira'))
    if name == 'W':
        for member, ((s, e), after) in SYNC_ONLY.items():
            inside = [h for h in hs if (h['n'] > 0 and any(s <= x <= e for x in range(h['a'], h['a'] + h['n'])))
                      or (h['n'] == 0 and s <= h['a'] < e)]
            ok = (len(inside) == 1 and inside[0]['n'] == 0 and inside[0]['a'] == after
                  and [x.strip() for x in inside[0]['added']] == ['SyncName();'])
            if not ok:
                bad.append((member, [(h['a'], h['n'], h['added']) for h in inside]))
    return bad


def main():
    w, wc = lines(os.path.join(SRC, 'W.cs')), lines(os.path.join(SRC, 'WC.cs'))
    assert len(w) == 3700 and len(wc) == 936, (len(w), len(wc))
    for name, text in (('W', w), ('WC', wc)):
        for n, frag in ANCHOR_TEXT[name].items():
            assert frag in text[n - 1], (name, n, frag, text[n - 1])
    print('anclas de texto: OK (%d en W, %d en WC)' % (len(ANCHOR_TEXT['W']), len(ANCHOR_TEXT['WC'])))
    for line in check_ranges('W', w, W) + check_ranges('WC', wc, WC):
        print(' ', line)
    allw = sorted(W.values())
    for (s1, e1), (s2, e2) in zip(allw, allw[1:]):
        assert e1 < s2, ('solape', (s1, e1), (s2, e2))
    base_text = {'W': '\n'.join(w), 'WC': '\n'.join(wc)}
    for name in ('W', 'WC'):
        for frag in WITNESS[name]:
            assert all(ord(c) < 128 for c in frag), frag
            n = base_text['W'].count(frag) + base_text['WC'].count(frag)
            assert n == 1, (frag, n)
            print('testigo %s en la base: %d (ASCII, unica en W y WC) %r' % (name, n, frag))
    base_names = member_names(base_text['W']) + member_names(base_text['WC'])
    for variant in ('A', 'B'):
        sim_text = {}
        for name, src, ranges in (('W', 'W.cs', W), ('WC', 'WC.cs', WC)):
            sim = os.path.join(SIM, '%s-%s.cs.txt' % (name, variant))
            sim_text[name] = '\n'.join(lines(sim))
            hs = hunks(os.path.join(SRC, src), sim)
            bad = judge(hs, ranges)
            print('variante %s %s: hunks=%d fuera=%d %s' % (variant, name, len(hs), len(bad), bad))
            print('variante %s %s: M01 (solo quitar en lo retirado; solo SyncName(); en los cuatro) -> %s' % (variant, name, m01(hs, name) or 'OK'))
            left = [f for f in WITNESS[name] if f in sim_text[name]]
            print('variante %s %s: M02 testigos que quedan -> %s' % (variant, name, left or 'ninguno'))
        sim_names = member_names(sim_text['W']) + member_names(sim_text['WC'])
        removed = sorted((Counter(base_names) - Counter(sim_names)).elements())
        added = sorted((Counter(sim_names) - Counter(base_names)).elements())
        print('variante %s censo: base=%d sim=%d; quitadas=%s; nuevas=%s' % (variant, len(base_names), len(sim_names), removed, added))
    return 0


if __name__ == '__main__':
    sys.exit(main())
