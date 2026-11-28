"""Resume los resultados de los casos reales. Uso: python -I summarize.py <dir-v2> <resultado.json>..."""
import json, os, sys

D = sys.argv[1]
for name in sys.argv[2:]:
    d = json.load(open(os.path.join(D, name), encoding='utf-8'))
    st = d['SelfTest']
    print('==', name)
    print(' Id:', d['Id'], '| Task:', d['Task'], '| Mode:', d['Mode'])
    print(' Scope:', d['Scope'])
    print(' ScriptSha256:', d['ScriptSha256'])
    print(' Autoprueba: %d casos, todos Ok=%s, determinista=%s' % (len(st), all(x['Ok'] for x in st), d['SelfTestDeterministic']))
    print(' SubstituteResult:', d['SubstituteResult'], '| Record:', d.get('Record', {}).get('nc2'))
    print(' Propiedades:', {k.split('_')[0]: v for k, v in d.get('Properties', {}).items()})
    m = d.get('Measured', {})
    print(' ExactEvidenceMatched:', m.get('ExactEvidenceMatched'), '| ExactWhitelistBy:', m.get('ExactWhitelistBy'), '| bytes igual:', m.get('RawPlusContractCollectionsEqualsDelegationBytes'))
    print(' TaskBinding:', d.get('TaskBinding'))
    print(' Nc2ControllerChecks Contract/Scope:', d.get('Nc2ControllerChecks', {}).get('Contract'), d.get('Nc2ControllerChecks', {}).get('Scope'), '|', d.get('Nc2ControllerClassification'), d.get('Nc2ControllerDisposition'), d.get('Nc2ControllerFailureClass'))
    print(' ChangedPath:', m.get('ChangedPath'))
    print(' Casos nuevos:', [(x['Case'], x['Result'], x['Ok']) for x in st if x['Case'] in ('AY', 'AZ', 'BA', 'BB', 'BC', 'BD', 'BE', 'T1', 'T2', 'T3', 'T4', 'T5', 'T6')])
    print(' Casos con Ok falso:', [x['Case'] for x in st if not x['Ok']])
