import hashlib, json, os, shutil, subprocess, sys, datetime, filecmp
def sha(p):
    h=hashlib.sha256()
    with open(p,'rb') as f:
        for b in iter(lambda:f.read(1<<20),b''): h.update(b)
    return h.hexdigest().upper()
out={}
dll='<carpeta-del-DLL-de-C2>'
exp={'RackCad.Plugin.dll':'798C936698277F418CE47D31CAF2028F9253002DBF9DFEE16774EA923E4D4E48','RackCad.Application.dll':'105C9FB67950D54581A505D021BF6A9F59559ED45786380C9DEA0353ACFDEBF0','RackCad.Domain.dll':'EF843E4353E80E1FAB6CC2C1296CC4576D234DB6692EE49D8DE5D99A3FEE09CC','RackCad.UI.dll':'5AEDE180D6AAD75186AB09E783DAC60C766C3C7348C3CA82C03A0838C7B83140'}
out['P1']={k:(sha(os.path.join(dll,k))==v) for k,v in exp.items()}
WT='<worktree-de-I-64>'
cat_dll=os.path.join(dll,'catalogs'); cat_src=os.path.join(WT,'assets','catalogs')
names=sorted(os.listdir(cat_dll)); src_names=sorted(n for n in os.listdir(cat_src) if n.endswith(('.json','.csv')))
diff=[n for n in names if not os.path.exists(os.path.join(cat_src,n)) or open(os.path.join(cat_dll,n),'rb').read()!=open(os.path.join(cat_src,n),'rb').read()]
out['P2']={'dll_files':len(names),'src_files':len(src_names),'only_src':[n for n in src_names if n not in names],'differ':diff}
r=subprocess.run(['git','-C',WT,'diff','--name-only','9abc1320','HEAD','--','src','assets'],capture_output=True,text=True)
out['P3']={'head':subprocess.run(['git','-C',WT,'rev-parse','HEAD'],capture_output=True,text=True).stdout.strip(),'diff':r.stdout.strip()}
ps=subprocess.run(['powershell','-NoProfile','-Command',"[Environment]::GetEnvironmentVariables('User').Keys + [Environment]::GetEnvironmentVariables('Machine').Keys + [Environment]::GetEnvironmentVariables('Process').Keys | ? { $_ -like 'RACKCAD_DEBUG*' }"],capture_output=True,text=True)
out['P4']=ps.stdout.strip()
print(json.dumps(out,indent=1))
