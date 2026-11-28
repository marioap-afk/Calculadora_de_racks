import hashlib, json, os, shutil
def sha(p):
    h=hashlib.sha256()
    with open(p,'rb') as f:
        for b in iter(lambda:f.read(1<<20),b''): h.update(b)
    return h.hexdigest().upper()
d=json.load(open('<registro-de-copias-de-Smoke-2>/copias.json',encoding='utf-8-sig'))
want='7A868624226EE956CC411CAAA54EA2DE6B95849EEE946995917CFF70FB9F47A5'
src=[o['Source'] for o in d['Originals'] if o['Sha256Before']==want][0]
rec=json.load(open('<carpeta-de-copias-de-PB-T0>/copias.json',encoding='utf-8'))
before=sha(src); assert before==want
for name in ['M1u.dwg','M1v.dwg','M2u.dwg','M3u.dwg']:
    dst='<carpeta-de-copias-de-PB-T0>/'+name; assert not os.path.exists(dst)
    shutil.copyfile(src,dst); s=sha(dst); assert s==want
    rec['Copies'].append({'Copy':name,'Role':'D1','Sha256':s})
after=sha(src); assert after==before
rec['Originals'].append({'Role':'D1','Sha256Before':before,'Sha256After':after,'Unchanged':True,'Note':'segunda tanda (copias de Deshacer, N-I1)'})
json.dump(rec,open('<carpeta-de-copias-de-PB-T0>/copias.json','w',encoding='utf-8'),indent=1)
print('ok', len(rec['Copies']))
