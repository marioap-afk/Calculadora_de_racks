"""Analiza los inventarios RCDUMP del bloque M. Uso: python inv_analyze.py <archivo> [<archivo_anterior>] [--rack <Id8>]"""
import hashlib, json, sys


def load(path):
    L = open(path, encoding='utf-8', errors='replace').read().split('\n')
    out = {'clayer': None, 'refs': [], 'defs': {}}
    i = 0
    while i < len(L):
        l = L[i]
        if l.startswith('CLAYER\t'):
            out['clayer'] = l.split('\t', 1)[1]
        elif l.startswith('REF\t'):
            p = l.split('\t')
            out['refs'].append({'h': p[1], 'name': p[2], 'pt': p[3], 'rot': p[4], 'sx': p[5], 'layer': p[6], 'bbox': p[7] if len(p) > 7 else ''})
        elif l.startswith('DEF\t'):
            p = l.split('\t')
            j = L[i + 1]
            i += 1
            d = {'name': p[1], 'ents': int(p[2]), 'len': int(p[3]), 'sha': hashlib.sha256(j.encode('utf-8')).hexdigest()[:12]}
            try:
                e = json.loads(j)
                d.update({'kind': e.get('Kind'), 'id': (e.get('Id') or '')[:8], 'Name': e.get('Name'), 'view': e.get('View'), 'section': e.get('Section')})
                des = json.loads(e['Design']) if isinstance(e.get('Design'), str) else (e.get('Design') or {})
                pb = des.get('PushBack') or {}
                st = pb.get('Structure') or {}
                d['fronts'] = len(st.get('Fronts') or []) if isinstance(st, dict) else None
                d['modules'] = len(st.get('Modules') or []) if isinstance(st, dict) else None
                d['composite'] = bool((pb.get('SideB') or {}).get('IsPresent')) if isinstance(pb.get('SideB'), dict) else False
                d['pbfronts'] = len(pb.get('Fronts') or [])
                d['annot'] = (st.get('AnnotationScale') if isinstance(st, dict) else None)
                d['dimstyle'] = (st.get('DimensionStyle') if isinstance(st, dict) else None)
                d['sysname'] = (st.get('Name') if isinstance(st, dict) else None)
            except Exception as ex:
                d['err'] = str(ex)[:80]
            out['defs'][p[1]] = d
        i += 1
    return out


def summary(inv, rack=None):
    rows = []
    for n, d in sorted(inv['defs'].items()):
        if rack and d.get('id') != rack:
            continue
        refs = [r for r in inv['refs'] if r['name'] == n]
        rows.append('%-28s id=%s Name=%s view=%s sec=%s fronts=%s pbf=%s mods=%s comp=%s annot=%s dim=%s ents=%d len=%d sha=%s refs=%d %s' % (
            n, d.get('id'), d.get('Name'), d.get('view'), d.get('section'), d.get('fronts'), d.get('pbfronts'), d.get('modules'), d.get('composite'), d.get('annot'),
            d.get('dimstyle'), d['ents'], d['len'], d['sha'], len(refs), ';'.join('%s@%s rot=%s sx=%s lay=%s' % (r['h'], r['pt'], r['rot'], r['sx'], r['layer']) for r in refs)))
    return rows


def diff(a, b, rack=None):
    out = []
    names = sorted(set(a['defs']) | set(b['defs']))
    for n in names:
        da, db = a['defs'].get(n), b['defs'].get(n)
        if rack and not ((da and da.get('id') == rack) or (db and db.get('id') == rack)):
            continue
        if da is None:
            out.append('NUEVA DEF %s' % n)
        elif db is None:
            out.append('BORRADA DEF %s' % n)
        else:
            ch = [k for k in ('Name', 'view', 'section', 'fronts', 'pbfronts', 'modules', 'composite', 'annot', 'dimstyle', 'ents', 'len', 'sha') if da.get(k) != db.get(k)]
            if ch:
                out.append('CAMBIA %s: %s' % (n, ', '.join('%s %s->%s' % (k, da.get(k), db.get(k)) for k in ch)))
    ra = {r['h']: r for r in a['refs']}
    rb = {r['h']: r for r in b['refs']}
    for h in sorted(set(ra) | set(rb)):
        x, y = ra.get(h), rb.get(h)
        nm = (x or y)['name']
        if rack:
            dd = (b['defs'].get(nm) or a['defs'].get(nm) or {})
            if dd.get('id') != rack:
                continue
        if x is None:
            out.append('NUEVA REF %s %s %s rot=%s sx=%s lay=%s bbox=%s' % (h, nm, y['pt'], y['rot'], y['sx'], y['layer'], y['bbox']))
        elif y is None:
            out.append('BORRADA REF %s %s' % (h, nm))
        elif x != y:
            out.append('CAMBIA REF %s %s' % (h, nm))
    return out


if __name__ == '__main__':
    args = [a for a in sys.argv[1:] if not a.startswith('--')]
    rack = sys.argv[sys.argv.index('--rack') + 1] if '--rack' in sys.argv else None
    if rack:
        args = [a for a in args if a != rack]
    b = load(args[0])
    print('CLAYER', b['clayer'])
    for r in summary(b, rack):
        print(r)
    if len(args) > 1:
        a = load(args[1])
        print('--- diff contra', args[1])
        for r in diff(a, b, rack):
            print(r)
