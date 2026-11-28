import re,sys
A='artifacts/orchestration/I-64/F4B-PBT4-RETIRE-PILOT/1/R20261122T173325Z-c9e1'
rows={}
for l in open(A+'/inv/filters-all.tsv'):
    p=l.rstrip('\n').split('\t'); rows[p[1]]=(int(p[2]),p[3])
expect_red = sys.argv[2]=='red' if len(sys.argv)>2 else False
txt=open(sys.argv[1],encoding='utf-8',newline='').read().replace('\r','')
bad=0;n=0
for chunk in re.split(r'\n(?=(?:core|ui)\d\d \|)',txt):
    m=re.match(r'(\w+) \| (\S+) \| exit=(\d+) \| .*?Con error:\s*(\d+), Superado:\s*(\d+), Omitido:\s*(\d+), Total:\s*(\d+)',chunk.replace('\n',' '))
    if not m: continue
    n+=1
    nm,f,ex,fa,pa,sk,to=m.groups(); fa,pa,sk,to=map(int,(fa,pa,sk,to))
    mn,red=rows[f]
    ok = (fa==0 and pa>=mn and ex=='0') if not (expect_red and red=='si') else (fa==to and to>=mn)
    if not ok: bad+=1; print('MAL',nm,f,fa,pa,sk,to,mn,red)
print('filtros',n,'mal',bad)
