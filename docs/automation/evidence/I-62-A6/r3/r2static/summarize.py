# summarize.py - compact Markdown view of part-a-events.json and part-b-pou.json; --hashes lists the SHA-256 of each file of
# this folder except README.md. Read-only. Run: python -I summarize.py [--hashes]
import hashlib, json, os, sys

HERE = os.path.dirname(os.path.abspath(__file__))


def load(n):
    with open(os.path.join(HERE, n), 'rb') as fh:
        return json.loads(fh.read().decode('ascii'))


def part_a():
    d = load('part-a-events.json')
    print('| Registro | CLI / binario / app | Eventos | command_execution | Shell | Max bytes | CRLF / LF sueltos | Items con texto de stderr | Completo |')
    print('|---|---|---|---|---|---|---|---|---|')
    for r in d['Logs']:
        v = r['Version']
        ver = ' / '.join([(v.get('CliVersion') or ['-'])[0], (v.get('BinarySha256') or ['-'])[0][:8], (v.get('AppVersion') or ['-'])[0]])
        ce = r['CommandExecution']
        ev = ', '.join('%s %d' % (k, n) for k, n in sorted(r['EventTypes'].items()))
        sh = ', '.join('%s %d' % (os.path.basename(k.replace('\\\\', '\\').replace('\\', '/')) + (' (runtime)' if 'codex-runtimes' in k else ''), n)
                       for k, n in ce['Shells'].items())
        t = ce['Totals']
        print('| `%s` | %s | %s | %d | %s | %s | %d / %d | %d | %s |' % (
            r['Log'].split('I-62-F6/')[1], ver, ev, ce['Count'], sh, ce['AggregatedOutputUtf8Bytes'].get('Max'),
            t.get('CRLF', 0), t.get('LoneLF', 0), ce['ItemsWithStderrMarkers'], 'si' if r['Completeness']['Complete'] else 'no'))
    print()
    print(json.dumps(d['Summary'], indent=1, sort_keys=True))


def part_b():
    d = load('part-b-pou.json')
    print('| Caso | Condicion plantada | Esperado | Observado (rc, Launch, pasos fallidos) | Se cumple? |')
    print('|---|---|---|---|---|')
    for c in d['Cases']:
        e = c['Expected']
        o = c.get('Observed') or {}
        exp = 'Launch %s%s' % (str(e['Launch']).lower(), (', falla (%s)' % e['FailingStep']) if e['FailingStep'] else '')
        obs = 'rc %s, Launch %s, %s' % (o.get('Rc'), str(o.get('Launch')).lower(), '[' + ', '.join(o.get('FailedSteps') or []) + ']')
        if o.get('UnestablishedSteps'):
            obs += '; sin establecer: [' + ', '.join(o['UnestablishedSteps']) + ']'
        print('| %s | %s | %s | %s | %s |' % (c['Id'], c['Description'], exp, obs, 'si' if c.get('Holds') else 'NO'))
    print()
    print(json.dumps(d['Summary'], indent=1, sort_keys=True))


def hashes():
    for n in sorted(os.listdir(HERE)):
        p = os.path.join(HERE, n)
        if os.path.isfile(p) and n != 'README.md':
            with open(p, 'rb') as fh:
                print('| `%s` | `%s` |' % (hashlib.sha256(fh.read()).hexdigest(), n))


if __name__ == '__main__':
    if '--hashes' in sys.argv:
        hashes()
    else:
        part_a()
        print()
        part_b()
