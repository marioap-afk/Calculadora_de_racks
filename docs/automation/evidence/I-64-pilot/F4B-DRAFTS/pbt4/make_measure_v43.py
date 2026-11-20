"""Escribe measure_pbt4_v43.py a partir de measure_pbt4_v42.py: el mismo GREEN (green_pbt4a.py, ya con CA-T4a I-01) y una
comprobación más, la regla nueva del §5.0 del paquete v4.3: ningún `?` de tipo de referencia en las líneas que el GREEN añade a un
archivo sin `#nullable enable` (los proyectos tienen Nullable=disable), y ninguna directiva `#nullable` en `Draft/`.

Uso: python -I make_measure_v43.py   (lee y escribe solo en esta carpeta)
"""
here = __file__.replace('make_measure_v43.py', '')
src = open(here + 'measure_pbt4_v42.py', encoding='utf-8').read()
anchor = 'SIG_RX = '
assert src.count(anchor) == 1
check = r'''VALUE_TYPES = {'bool', 'byte', 'sbyte', 'char', 'decimal', 'double', 'float', 'int', 'uint', 'long', 'ulong', 'short', 'ushort',
               'DateTime', 'Guid', 'TimeSpan', 'SessionId', 'EditIntentReadResult'}
REF_Q = re.compile(r'(?<![\w.])([A-Za-z_][\w.]*(?:<[^<>()]*>)?)\?(?=\s+[A-Za-z_]|\s*[,)>\]])')


def ref_annotations(path):
    """Las anotaciones `T?` de tipo de referencia en las líneas que el GREEN añade, si el archivo no lleva `#nullable enable`."""
    text = _green[path]
    if text.lstrip('﻿').startswith('#nullable enable'):
        return []
    old = set(_base.get(path, '').split('\n'))
    hits = []
    for line in text.split('\n'):
        if line in old:
            continue
        code = re.sub(r'//.*', '', re.sub(r'"(?:\\.|[^"\\])*"', '""', line))
        for m in REF_Q.finditer(code):
            if m.group(1).split('<')[0] not in VALUE_TYPES:
                hits.append(m.group(0))
    return hits


offenders = {gp.SHORT if False else p.rsplit('/', 1)[-1]: ref_annotations(p) for p in _green}
check('CA-T4a I-01 (la regla del §5.0)', 'ningún `?` de tipo de referencia en lo que el GREEN añade a un archivo sin `#nullable enable` (NH y `Draft/PushBackOriginByText.cs`; WHC, WEI, RIA y la interfaz lo llevan), y ningún `#nullable` en `Draft/`: '
      + ('; '.join('%s: %s' % (k, ', '.join(v)) for k, v in offenders.items() if v) or 'ninguno'),
      not any(offenders.values()) and '#nullable' not in _green[gp.DRAFT_P])
_v42_lines = ['        public static string? Choose(DraftStaleBase staleBase, string originText)',
              '                scan.RawEnvelopes.ToDictionary(pair => pair.Key, pair => (string?)pair.Value, StringComparer.Ordinal),']
_v42_hits = [m.group(0) for l in _v42_lines for m in REF_Q.finditer(l)]
rows.append(('Contrafactual (CA-T4a I-01)', 'con el texto de la v4.2 de `PushBackOriginByText.cs`, la regla ve ' + ', '.join('`%s`' % h for h in _v42_hits),
             'ROJA (esperada)' if len(_v42_hits) == 2 else 'verde'))

'''
src = src.replace(anchor, check + anchor, 1)
assert src.count('contrafactuales en rojo=%d de 2') == 1
src = src.replace('contrafactuales en rojo=%d de 2', 'contrafactuales en rojo=%d de 3')
src = src.replace("__file__.replace('measure_pbt4_v42.py'", "__file__.replace('measure_pbt4_v43.py'")
src = src.replace('"""PB-T4, v4.2 (B-08, con ReadAt):', '"""PB-T4, v4.3 (B-08 con ReadAt, y CA-T4a I-01):')
src = src.replace("Uso: python -I measure_pbt4_v42.py", "Uso: python -I measure_pbt4_v43.py")
open(here + 'measure_pbt4_v43.py', 'w', encoding='utf-8', newline='\n').write(src)
print('ok')
