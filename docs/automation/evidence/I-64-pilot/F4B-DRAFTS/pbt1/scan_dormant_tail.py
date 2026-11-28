"""Solo lectura: en los inventarios crudos del bloque M (copias de trabajo), busca sobres Push Back de UN sentido que guarden una
cola compuesta (ids GAP o B:...). Imprime, por definicion, el nombre, si el diseno es compuesto (SideB.IsPresent), los ids de modulo y
si hay cola. Sin rutas en la salida mas alla del nombre del inventario."""
import json, re, sys
for path in sys.argv[1:]:
    raw = open(path, 'rb').read().decode('utf-8', 'replace')
    for line in raw.split(chr(10)):
        i = line.find('{"SchemaVersion"')
        if i < 0:
            continue
        try:
            env = json.loads(line[i:].strip())
        except Exception as ex:
            continue
        if env.get('Kind') != 'pushback':
            continue
        try:
            d = json.loads(env['Design'])
        except Exception:
            continue
        pb = (d.get('PushBack') or {})
        st = pb.get('Structure') or {}
        mods = [m.get('ModuleId') or m.get('Id') for m in (st.get('Modules') or [])]
        sideb = pb.get('SideB') or st.get('SideB')
        present = None
        if isinstance(sideb, dict):
            present = sideb.get('IsPresent')
        comp = pb.get('Composite') or st.get('Composite')
        tail = [m for m in mods if m and (m == 'GAP' or str(m).startswith('B:'))]
        if tail or present:
            print(path.split('/')[-1], '|', env.get('Name'), '| vista', env.get('View'), env.get('Section'), '| SideB.IsPresent', present,
                  '| Composite', 'si' if comp else 'no', '| modulos', len(mods), '| cola', len(tail), '| fondos', st.get('PalletsDeep'))
