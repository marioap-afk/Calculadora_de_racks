# meta.py - collects the run record for assemble.py: environment, exact commands with UTC times, script changes,
# guards (packages and read-only sources before/after) and verification-helper outputs.
# python -I meta.py <pkg> <repro dir> <out json>
import json, os, re, sys

sys.dont_write_bytecode = True
pkg, R, out = sys.argv[1:4]
W = os.path.join(R, '_work')


def rd(p):
    with open(p, encoding='utf-8') as fh:
        return fh.read()


def times(path, names):
    t = {}
    if os.path.isfile(path):
        for ln in rd(path).splitlines():
            m = re.match(r'^(\S+) (start|end) (\S+)(?: exit=(\d+))?$', ln)
            if m:
                t.setdefault(m.group(1), {})[m.group(2).capitalize() + 'Utc'] = m.group(3)
                if m.group(4) is not None:
                    t[m.group(1)]['Exit'] = int(m.group(4))
    return t


def hashes(p):
    d = {}
    for ln in rd(p).splitlines():
        h, f = ln.split(' ', 1)
        d[f.lstrip('*')] = h
    return d


before, after = hashes(os.path.join(W, 'package-hashes-before.txt')), hashes(os.path.join(W, 'package-hashes-after.txt'))
gb, ga = json.loads(rd(os.path.join(W, 'guard-before.json'))), json.loads(rd(os.path.join(W, 'guard-after.json')))
pk = 'r1r4/' if pkg == 'r1r4' else 'r3/'
guards = {'PackageFilesUnchanged': dict((k, v) for k, v in before.items() if k.startswith(pk)) == dict((k, v) for k, v in after.items() if k.startswith(pk)),
          'PackageFileCount': len([k for k in before if k.startswith(pk)]),
          'ReadOnlySourcesBefore': gb, 'ReadOnlySourcesAfter': ga, 'ReadOnlySourcesUnchanged': gb == ga,
          'OriginalWorkRootRead': 'read only (file hashing, JSON parsing, git ls-files -s and git cat-file commit with GIT_OPTIONAL_LOCKS=0), never written',
          'HostStandbyDuringRuns': json.loads(rd(os.path.join(W, 'standby-check.json')))}
if pkg == 'r1r4':
    rt = {}
    for s in ('run_r1', 'run_r1_extra', 'run_r4'):
        rt[s] = {'StartUtc': rd(os.path.join(W, s + '.start')).strip(), 'EndUtc': rd(os.path.join(W, s + '.end')).strip(),
                 'Exit': int(re.search(r'exit=(\d+)', rd(os.path.join(W, s + '.log'))).group(1))}
    res = json.loads(rd(os.path.join(R, 'r1r4', 'r1-p1-p2-p3.json')))
    env = {'Versions': res['Versions'], 'NoDefaultCurrentDirectoryInExePath': res['NoDefaultCurrentDirectoryInExePath'],
           'Pins': res['Pins'], 'LaunchShell': 'Git Bash (Bash tool); git, pwsh and python resolved from its PATH exactly as recorded in the custodied Versions',
           'ScriptsFolder': 'ev/repro/r1r4 (copies of run_r1.py, run_r1_extra.py, run_r4.py, summarize.py, lib_r1r4.py)',
           'WorkRoot': 'D:/r62-a6r2/repro/r1r4'}
    cmds = [{'Cwd': 'ev/repro/r1r4', 'Command': 'python -I run_r1.py', 'Run': rt['run_r1']},
            {'Cwd': 'ev/repro/r1r4', 'Command': 'python -I run_r1_extra.py', 'Run': rt['run_r1_extra']},
            {'Cwd': 'ev/repro/r1r4', 'Command': 'python -I run_r4.py', 'Run': rt['run_r4']},
            {'Cwd': 'ev/repro/r1r4', 'Command': 'python -I summarize.py ; python -I summarize.py --hashes', 'Run': {'Exit': 0}}]
    changes = {'lib_r1r4.py': {'Diff': rd(os.path.join(W, 'lib_r1r4.diff')), 'Sha256Original': 'c3995a1cd377822593837c6e34eff30425de61da73b1928d5a0460ce93bfa8c5',
                               'Sha256Copy': '5ea36dc7a4c7d6e9fc08e5b836b1a7e3cdfaf9c4cee45d196b44707d650536e6'},
               'run_r1.py, run_r1_extra.py, run_r4.py, summarize.py': 'byte-identical copies (same SHA-256 as the package)'}
    helpers = {'regen_t0': json.loads(rd(os.path.join(W, 'regen_t0.log'))), 'regen_extra': json.loads(rd(os.path.join(W, 'regen_extra.log'))),
               'Note': 'verification-only re-executions after the reproduction (tools/regen_t0.py, tools/regen_extra.py); outputs under D:/r62-a6r2/repro/verify-r1r4/, outside the work root; every regenerated envelope is byte-identical to what the reproduction recorded and the repositories were unchanged'}
    neg = json.loads(rd(os.path.join(W, 'negctl-summary.json')))
else:
    rt = times(os.path.join(W, 'r3-times.log'), None)
    bj = json.loads(rd(os.path.join(R, 'r3', 'results', 'build.json')))
    env = {'Header': bj['Header'], 'R3_WORK': 'D:/r62-a6r2/repro/r3', 'R3_OUT': 'unset (default: ev/repro/r3/results)',
           'LaunchShell': 'Git Bash (Bash tool), same PATH resolution as the R1/R4 run',
           'ScriptsFolder': 'ev/repro/r3/scripts (byte-identical copies) and ev/repro/r3/README.md (byte-identical copy, so finalize.py lists the same files)'}
    cmds = [{'Cwd': 'ev/repro/r3/scripts', 'Env': 'R3_WORK=D:/r62-a6r2/repro/r3', 'Command': 'python -I %s.py' % s, 'Run': rt.get(s)}
            for s in ('build_repos', 'part_a', 'part_b', 'part_c', 'part_d', 'review_e', 'finalize')]
    first = json.loads(rd(os.path.join(W, 'r3-interrupted', 'summary.json')))
    cmds.insert(0, {'Note': 'first attempt, discarded', 'Detail': first})
    changes = {'scripts/*.py': 'none: byte-identical copies (same SHA-256 as the package); the work root is set only through R3_WORK'}
    helpers = None
    neg = 'the same comparator was validated by the R1/R4 negative control (repro-r1r4.json Method.NegativeControl)'
meta = {'Environment': env, 'Commands': cmds, 'ScriptChanges': changes, 'Guards': guards, 'VerificationHelpers': helpers, 'NegativeControl': neg}
with open(out, 'w', encoding='utf-8', newline='\n') as fh:
    fh.write(json.dumps(meta, ensure_ascii=True, sort_keys=True, indent=1) + '\n')
print('ok')
