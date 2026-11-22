import re,sys
t=open(sys.argv[1],encoding='utf-8').read()
c=re.search(r'<Counters [^>]*/>',t).group(0)
print(c)
res=re.findall(r'<UnitTestResult [^>]*>',t)
from collections import Counter
oc=Counter(re.search(r'outcome="(\w+)"',r).group(1) for r in res)
print(len(res),dict(oc))
for r in res:
    if 'outcome="Failed"' in r: print('FAILED',re.search(r'testName="([^"]+)"',r).group(1))
