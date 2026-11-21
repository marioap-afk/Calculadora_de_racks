import sys, importlib.util, os, hashlib
here = os.path.dirname(os.path.abspath(__file__))
spec = importlib.util.spec_from_file_location('green_pbt4b_v45', os.path.join(here, 'green_pbt4b_v45.py'))
m = importlib.util.module_from_spec(spec); spec.loader.exec_module(m)
repo, rev, out = sys.argv[1:4]
base, green = m.green(repo, rev)
_, red = m.skeleton(repo, rev)
os.makedirs(out, exist_ok=True)
for kind, d in (('green', green), ('red', red)):
    for rel, text in d.items():
        p = os.path.join(out, kind + '_' + os.path.basename(rel))
        open(p, 'wb').write(text.encode('utf-8'))
        print(kind, hashlib.sha256(text.encode('utf-8')).hexdigest(), rel)
