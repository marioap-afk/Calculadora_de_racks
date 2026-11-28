import hashlib, json, os, shutil, datetime
def sha(p):
    h=hashlib.sha256()
    with open(p,'rb') as f:
        for b in iter(lambda:f.read(1<<20),b''): h.update(b)
    return h.hexdigest().upper()
d=json.load(open('<registro-de-copias-de-Smoke-2>/copias.json',encoding='utf-8-sig'))
role_sha={}
for c in d['Copies']: role_sha.setdefault(c['Role'],c['Sha256'])
want={'D1':role_sha['D1'],'D2':role_sha['D2']}
orig={}
for o in d['Originals']:
    for r,s in want.items():
        if o['Sha256Before']==s: orig[r]=o['Source']
assert set(orig)=={'D1','D2'}, orig
os.makedirs('<carpeta-de-copias-de-PB-T0>/inv',exist_ok=True)
plan={'D1':['M1.dwg','M2.dwg','M3.dwg','M4.dwg','M6.dwg'],'D2':['M7-D2.dwg']}
rec={'CreatedUtc':datetime.datetime.utcnow().isoformat()+'Z','Originals':[],'Copies':[]}
for r,src in orig.items():
    before=sha(src); assert before==want[r]
    for name in plan[r]:
        dst='<carpeta-de-copias-de-PB-T0>/'+name
        assert not os.path.exists(dst)
        shutil.copyfile(src,dst)
        s=sha(dst); assert s==want[r]
        rec['Copies'].append({'Copy':name,'Role':r,'Sha256':s})
    after=sha(src); assert after==before
    rec['Originals'].append({'Role':r,'Sha256Before':before,'Sha256After':after,'Unchanged':after==before})
json.dump(rec,open('<carpeta-de-copias-de-PB-T0>/copias.json','w',encoding='utf-8'),indent=1)
print(json.dumps({'Originals':rec['Originals'],'n':len(rec['Copies'])},indent=1))
