# summarize.py - prints a compact view of the result files of this folder and the SHA-256 of every deliverable.
# python -I summarize.py [--hashes]
import hashlib, json, os, sys

HERE = os.path.dirname(os.path.abspath(__file__))


def load(n):
    p = os.path.join(HERE, n)
    return json.load(open(p, encoding='ascii')) if os.path.isfile(p) else None


def out(*a):
    sys.stdout.write(' '.join(str(x) for x in a) + '\n')


def main():
    if '--hashes' in sys.argv:
        for r, ds, fs in os.walk(HERE):
            ds[:] = [d for d in ds if d != '__pycache__']
            for f in sorted(fs):
                p = os.path.join(r, f)
                out(hashlib.sha256(open(p, 'rb').read()).hexdigest(), os.path.relpath(p, HERE).replace(os.sep, '/'))
        return
    d = load('r1-p1-p2-p3.json')
    if d:
        out('== P1/P2/P3', d['Held'], d['P2Held'], 'P3 all equal', d['P3']['AllPwshAndCmdEnvelopesByteEqualToFileCopy'])
        out('versions', json.dumps(d['Versions']))
        for sh, r in d['P2'].items():
            out(' P2', sh, r['Launch'], r['FailedSteps'], r['Steps']['3']['Hash'], json.dumps(r['Steps']['3']['Extra_ConsumerDigest']))
    a = load('r1-attacks.json')
    for src in (a, load('r1-attacks-extra.json')):
        if not src:
            continue
        for k, v in src['Attacks'].items():
            out('== attack', k, '|', v.get('Attack'))
            out('   Held', json.dumps(v.get('Held')))
            for key in ('P2After', 'P2_cmd', 'P2_pwsh'):
                if isinstance(v.get(key), dict):
                    out('  ', key, v[key]['Launch'], v[key]['FailedSteps'])
            for key in ('AcceptorOnFabricated', 'AcceptorOnP3', 'AcceptorOnCmdEnvelopes'):
                if isinstance(v.get(key), dict):
                    out('  ', key, v[key]['Decision'], v[key]['Rc'], json.dumps(v[key]['Checks']))
            if 'ToolFactsVsBaseline' in v:
                out('   facts changed', json.dumps(dict((n, x.get('FactsKeysChanged')) for n, x in v['ToolFactsVsBaseline'].items())))
            if 'GitViews' in v:
                out('   gitviews', json.dumps(v['GitViews'])[:500])
    for n in ('r4-t1.json', 'r4-t3.json', 'r4-t6.json', 'r4-t2.json'):
        d = load(n)
        if not d:
            continue
        out('==', n)
        for k, v in d.items():
            if k.startswith('Test') or k in ('t6a', 't6b'):
                acc = v.get('Acceptor') if isinstance(v, dict) else None
                if acc:
                    out('  ', k, acc['Decision'], acc['Rc'], json.dumps(acc['Checks']), 'Held', v.get('Held'))
                elif isinstance(v, dict) and 'Flips' in v:
                    for f in v['Flips']:
                        out('   flip', f['Check'], f['From'], '->', f['To'], f['Acceptor']['Decision'], json.dumps(f['Acceptor']['Checks']), 'Held', f['Held'])
                else:
                    out('  ', k, json.dumps(v)[:400])


if __name__ == '__main__':
    main()
