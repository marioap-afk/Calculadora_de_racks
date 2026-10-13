# part_a_events.py - Part A of the I-62 A-6 round 3 bench (r2static): static, read-only analysis of the custodied Codex
# `--json` event logs (OD-2 and OD-2b-PROBE). Reads the I-62 worktree only; writes part-a-events.json next to this script.
# No model, no codex/claude CLI, no network. Run: python -I part_a_events.py
import collections, glob, importlib.util, json, os, re, statistics, sys

_spec = importlib.util.spec_from_file_location('benchlib', os.path.join(os.path.dirname(os.path.abspath(__file__)), 'benchlib.py'))
L = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(L)

EVROOT = os.path.join(L.WT, 'docs', 'automation', 'evidence', 'I-62-F6')
LOGS = sorted(glob.glob(os.path.join(EVROOT, 'OD-2', '*', '*events.jsonl')) + glob.glob(os.path.join(EVROOT, 'OD-2b-PROBE', '*', '*events.jsonl')))

# Text that a program writes to stderr, never to stdout (used only as evidence that stderr reached aggregated_output).
STDERR_MARKERS = {
    'pwsh-profile-dot-source': re.compile(r'profile\.ps1: Cannot dot-source'),
    'pwsh-error-record-concise': re.compile(r'(?m)^[A-Za-z]+(Exception|Error|Operation|NotFound|Denied|Argument|Invalid[A-Za-z]*)?: ?\r?\n(Line \||\s*\|)'),
    'pwsh-error-record-line-gutter': re.compile(r'(?m)^Line \|\r?$'),
    'pwsh-term-not-recognized': re.compile(r'The term .{1,200}? is not recognized'),
    'ps51-categoryinfo': re.compile(r'(?m)^\s*\+ CategoryInfo'),
    'git-fatal': re.compile(r'(?m)^fatal: '),
    'git-error': re.compile(r'(?m)^error: '),
    'git-warning': re.compile(r'(?m)^warning: '),
    'git-usage': re.compile(r'(?m)^usage: git'),
    'cmd-not-recognized-es': re.compile(r'no se reconoce como un comando interno o externo'),
    'cmd-not-recognized-en': re.compile(r'is not recognized as an internal or external command'),
    'cmd-system-cannot-find-es': re.compile(r'El sistema no puede (encontrar|hallar)'),
    'findstr-error': re.compile(r'(?m)^FINDSTR: '),
    'python-traceback': re.compile(r'Traceback \(most recent call last\)'),
}
# Lines that belong to a stderr block (heuristic, for the "stdout after stderr" test).
STDERR_LINE = re.compile(r'^(profile\.ps1: .*|[A-Za-z]+: ?|Line \|.*|\s*\d*\s*\|.*|fatal: .*|error: .*|warning: .*|hint: .*|usage: git.*|FINDSTR: .*|'
                         r'.*no se reconoce como un comando interno o externo.*|.*programa o archivo por lotes ejecutable\.?|'
                         r'Traceback \(most recent call last\):|\s+File ".*|\s+\+ .*|At line:.*|.*is not recognized.*)$')
TRUNC = re.compile(r'(?i)(truncat|\[\.\.\.|\.\.\. ?\(\d+|omitted|total output lines|output (was )?too long|…)')


def stats(xs):
    if not xs:
        return {'Count': 0}
    s = sorted(xs)
    return {'Count': len(s), 'Min': s[0], 'Max': s[-1], 'Median': statistics.median(s), 'P90': s[min(len(s) - 1, int(round(0.9 * (len(s) - 1))))],
            'Total': sum(s)}


def buckets(xs):
    edges = [(0, 0, '0'), (1, 1023, '1..1023'), (1024, 4095, '1K..4K-1'), (4096, 16383, '4K..16K-1'), (16384, 65535, '16K..64K-1'),
             (65536, 10 ** 12, '>=64K')]
    out = collections.OrderedDict((n, 0) for _, _, n in edges)
    for x in xs:
        for lo, hi, n in edges:
            if lo <= x <= hi:
                out[n] += 1
                break
    return dict(out)


def terminators(s):
    crlf = s.count('\r\n')
    lf = s.count('\n') - crlf
    cr = s.count('\r') - crlf
    return crlf, lf, cr


def shell_of(cmd):
    if not isinstance(cmd, str):
        return None, None
    m = re.match(r'^"([^"]+)"\s*(\S*)', cmd) or re.match(r'^(\S+)\s*(\S*)', cmd)
    return (m.group(1), m.group(2)) if m else (None, None)


def versions_for(d):
    v = {'Sources': []}
    def add(k, val, src):
        if val:
            v.setdefault(k, [])
            if val not in v[k]:
                v[k].append(val)
            if src not in v['Sources']:
                v['Sources'].append(src)
    for name in ('version.txt', 'pre-version.txt'):
        p = os.path.join(d, name)
        if os.path.isfile(p):
            add('CliVersion', L.rbytes(p).decode('utf-8', 'replace').strip(), name)
    p = os.path.join(d, 'measurements.log')
    if os.path.isfile(p):
        t = L.rbytes(p).decode('utf-8', 'replace')
        for b in sorted(set(re.findall(r'\bbin=([0-9a-f]{64})', t))):
            add('BinarySha256', b, 'measurements.log')
        for a in sorted(set(re.findall(r'\bapp=([0-9.]+)', t))):
            add('AppVersion', a, 'measurements.log')
    p = os.path.join(d, 'result.json')
    if os.path.isfile(p):
        try:
            r = json.loads(L.rbytes(p).decode('utf-8'))
        except Exception:
            r = {}
        b = r.get('Binary') if isinstance(r.get('Binary'), dict) else {}
        add('BinarySha256', b.get('Sha256') or r.get('BinaryHash'), 'result.json')
        add('CliVersion', b.get('CliVersion') or r.get('CliVersion'), 'result.json')
        app = r.get('App') or r.get('AppVersion')
        if isinstance(app, str):
            add('AppVersion', re.sub(r'^OpenAI\.Codex ', '', app), 'result.json')
        add('BinaryPath', b.get('Path') or r.get('BinaryPath'), 'result.json')
    p = os.path.join(d, 'runtime-pwsh.sha256')
    if os.path.isfile(p):
        add('RuntimePwshSha256', L.rbytes(p).decode('ascii', 'replace').strip(), 'runtime-pwsh.sha256')
    p = os.path.join(d, 'README.md')
    if os.path.isfile(p):
        t = L.rbytes(p).decode('utf-8', 'replace')
        for a in sorted(set(re.findall(r'\b(26\.\d+\.\d+\.\d+)\b', t))):
            add('AppVersionInReadme', a, 'README.md')
        for b in sorted(set(re.findall(r'\b([0-9a-f]{8})(?:…|\.\.\.)', t))):
            add('ShortHashesInReadme', b, 'README.md')
    return v


def excerpt(s, i, before=40, after=220):
    return s[max(0, i - before):i + after]


def analyse(path):
    raw = L.rbytes(path)
    rel = os.path.relpath(path, L.WT).replace(os.sep, '/')
    rec = {'Log': rel, 'Bytes': len(raw), 'Sha256': L.sha256(raw), 'FileCRLF': raw.count(b'\r\n'), 'FileEndsWithLF': raw.endswith(b'\n'),
           'Version': versions_for(os.path.dirname(path))}
    events, perr = [], []
    for n, ln in enumerate(raw.split(b'\n'), 1):
        if not ln.strip():
            continue
        try:
            events.append(json.loads(ln.decode('utf-8')))
        except Exception as e:
            perr.append({'Line': n, 'Error': type(e).__name__})
    rec['JsonLines'] = len(events)
    rec['ParseErrors'] = perr
    et = collections.Counter(e.get('type') for e in events)
    rec['EventTypes'] = dict(sorted(et.items()))
    it = collections.defaultdict(collections.Counter)
    for e in events:
        if isinstance(e.get('item'), dict):
            it[e['type']][e['item'].get('type')] += 1
    rec['ItemTypes'] = dict((k, dict(sorted(v.items()))) for k, v in sorted(it.items()))
    # completeness
    started, completed, order_bad = {}, {}, []
    for idx, e in enumerate(events):
        i = e.get('item')
        if not isinstance(i, dict):
            continue
        if e['type'] == 'item.started':
            if i.get('id') in started:
                order_bad.append('duplicate start ' + str(i.get('id')))
            started[i.get('id')] = (idx, i)
        elif e['type'] == 'item.completed':
            if i.get('id') in completed:
                order_bad.append('duplicate completion ' + str(i.get('id')))
            completed[i.get('id')] = (idx, i)
    unmatched = sorted(k for k in started if k not in completed)
    for k in started:
        if k in completed and completed[k][0] < started[k][0]:
            order_bad.append('completed before started ' + str(k))
    completed_without_start = collections.Counter(completed[k][1].get('type') for k in completed if k not in started)
    rec['Completeness'] = {
        'ThreadStarted': et.get('thread.started', 0), 'TurnStarted': et.get('turn.started', 0), 'TurnCompleted': et.get('turn.completed', 0),
        'TurnFailed': et.get('turn.failed', 0), 'ErrorEvents': et.get('error', 0),
        'FirstEvent': events[0].get('type') if events else None, 'LastEvent': events[-1].get('type') if events else None,
        'StartedItems': len(started), 'StartedMatchedByCompleted': len(started) - len(unmatched), 'UnmatchedStarted': unmatched,
        'CompletedWithoutStartByType': dict(completed_without_start), 'OrderProblems': order_bad,
        'Complete': (et.get('thread.started', 0) == 1 and et.get('turn.started', 0) == 1 and et.get('turn.completed', 0) == 1 and not unmatched
                     and not order_bad and not perr and et.get('turn.failed', 0) == 0 and et.get('error', 0) == 0)}
    # command executions
    cmds = [(k, completed[k][1], started.get(k, (None, None))[1]) for k in completed if completed[k][1].get('type') == 'command_execution']
    cmds.sort(key=lambda x: completed[x[0]][0])
    ce = {'Count': len(cmds)}
    ce['CommandFieldTypes'] = dict(collections.Counter(type(c.get('command')).__name__ for _, c, _ in cmds))
    shells, styles, dbl = collections.Counter(), collections.Counter(), 0
    exitc, stat, st_fields, same_cmd = collections.Counter(), collections.Counter(), collections.Counter(), 0
    chars, ubytes, per = [], [], []
    tot = collections.Counter()
    marker_items = collections.Counter()
    merged, trunc_hits, no_nl, spawn, markers_check = [], [], [], [], []
    for k, c, s in cmds:
        sh, style = shell_of(c.get('command'))
        shells[sh] += 1
        styles[style] += 1
        if sh and '\\\\' in sh:
            dbl += 1
        exitc[str(c.get('exit_code'))] += 1
        stat[str(c.get('status'))] += 1
        if s is not None:
            st_fields['started.aggregated_output=' + ('""' if s.get('aggregated_output') == '' else 'non-empty')] += 1
            st_fields['started.exit_code=' + str(s.get('exit_code'))] += 1
            st_fields['started.status=' + str(s.get('status'))] += 1
            same_cmd += int(s.get('command') == c.get('command'))
        a = c.get('aggregated_output')
        if not isinstance(a, str):
            per.append({'Id': k, 'AggregatedOutputType': type(a).__name__})
            continue
        ub = len(a.encode('utf-8'))
        chars.append(len(a))
        ubytes.append(ub)
        crlf, lf, cr = terminators(a)
        fffd = a.count('�')
        nonascii = sum(1 for ch in a if ord(ch) > 127)
        esc = a.count('\x1b')
        nul = a.count('\x00')
        tot.update({'CRLF': crlf, 'LoneLF': lf, 'LoneCR': cr, 'ReplacementChars': fffd, 'NonAsciiChars': nonascii, 'Esc': esc, 'Nul': nul})
        kind = 'none' if (crlf + lf + cr) == 0 else ('mixed' if (crlf and (lf or cr)) or (lf and cr) else ('crlf-only' if crlf else ('lf-only' if lf else 'cr-only')))
        tot['ItemsTerminators=' + kind] += 1
        if fffd:
            tot['ItemsWithReplacementChar'] += 1
        if nonascii:
            tot['ItemsWithNonAscii'] += 1
        mk = []
        first = None
        for name, rx in STDERR_MARKERS.items():
            m = rx.search(a)
            if m:
                mk.append(name)
                marker_items[name] += 1
                first = m.start() if first is None else min(first, m.start())
        ends_nl = a.endswith('\n')
        if a and not ends_nl:
            no_nl.append({'Id': k, 'ExitCode': c.get('exit_code'), 'Chars': len(a), 'Tail': a[-120:]})
        for m in TRUNC.finditer(a):
            trunc_hits.append({'Id': k, 'Match': m.group(0), 'Context': excerpt(a, m.start(), 60, 80)})
        if a.startswith('Failed to create unified exec process'):
            m = re.search(r'\| cmd=(.*?) \| env_u16_len=(\d+) \| si_flags=(\d+) \| creation_flags=(\d+)', a, re.S)
            spawn.append({'Id': k, 'ExitCode': c.get('exit_code'), 'Status': c.get('status'), 'CommandField': c.get('command'),
                          'ExecutedCmdLineInMessage': m.group(1) if m else None,
                          'EnvU16Len': int(m.group(2)) if m else None, 'SiFlags': int(m.group(3)) if m else None,
                          'SiFlagsHex': hex(int(m.group(3))) if m else None, 'CreationFlagsHex': hex(int(m.group(4))) if m else None,
                          'CommandFieldEqualsExecutedCmdLine': (m.group(1) == c.get('command')) if m else None,
                          'ExecutedCmdLineHasUtf8Prelude': ('[Console]::OutputEncoding' in m.group(1)) if m else None,
                          'CommandFieldHasUtf8Prelude': '[Console]::OutputEncoding' in (c.get('command') or '')})
        marks = re.findall(r'===[A-Za-z0-9_.:-]+===', c.get('command') or '')
        if marks:
            pos, inorder = 0, True
            for mm in marks:
                j = a.find(mm, pos)
                if j < 0:
                    inorder = False
                    break
                pos = j + len(mm)
            markers_check.append({'Id': k, 'Markers': len(marks), 'AllMarkersPresentInOrder': inorder, 'LastMarker': marks[-1],
                                  'Utf8Bytes': ub, 'ExitCode': c.get('exit_code')})
        if mk:
            lines = a.replace('\r\n', '\n').split('\n')
            flags = [bool(STDERR_LINE.match(x)) for x in lines]
            first_err = next((i for i, f in enumerate(flags) if f), None)
            stdout_after = first_err is not None and any((not flags[j]) and lines[j].strip() for j in range(first_err + 1, len(lines)))
            stdout_before = first_err is not None and any((not flags[j]) and lines[j].strip() for j in range(0, first_err))
            merged.append({'Id': k, 'ExitCode': c.get('exit_code'), 'Markers': mk, 'FirstMarkerOffset': first,
                           'BeginsWithStderrText': first == 0, 'NonStderrLinesBeforeFirstStderrLine': stdout_before,
                           'NonStderrLinesAfterFirstStderrLine': stdout_after, 'Excerpt': excerpt(a, first or 0, 0, 260)})
        per.append({'Id': k, 'ExitCode': c.get('exit_code'), 'Status': c.get('status'), 'Shell': sh, 'Chars': len(a), 'Utf8Bytes': ub,
                    'CRLF': crlf, 'LoneLF': lf, 'LoneCR': cr, 'ReplacementChars': fffd, 'NonAsciiChars': nonascii, 'StderrMarkers': mk,
                    'EndsWithNewline': ends_nl})
    ce['Shells'] = dict(shells)
    ce['ShellArgStyle'] = dict(styles)
    ce['ShellPathWithDoubledBackslashes'] = dbl
    ce['StartedItemFields'] = dict(sorted(st_fields.items()))
    ce['StartedCommandEqualsCompletedCommand'] = same_cmd
    ce['ExitCodes'] = dict(sorted(exitc.items()))
    ce['Statuses'] = dict(sorted(stat.items()))
    ce['AggregatedOutputChars'] = stats(chars)
    ce['AggregatedOutputUtf8Bytes'] = stats(ubytes)
    ce['AggregatedOutputUtf8BytesBuckets'] = buckets(ubytes)
    ce['Totals'] = dict(sorted(tot.items()))
    ce['ItemsPerStderrMarker'] = dict(sorted(marker_items.items()))
    ce['StderrEvidence'] = merged
    ce['ItemsWithStderrMarkers'] = len(merged)
    ce['ItemsBeginningWithStderrText'] = sum(1 for m in merged if m['BeginsWithStderrText'])
    ce['ItemsWithStderrThenStdout'] = sum(1 for m in merged if m['NonStderrLinesAfterFirstStderrLine'])
    ce['ItemsWithStdoutStderrStdout'] = sum(1 for m in merged if m['NonStderrLinesAfterFirstStderrLine'] and m['NonStderrLinesBeforeFirstStderrLine'])
    ce['SpawnFailureEvidence'] = spawn
    ce['EchoMarkerCompleteness'] = markers_check
    ce['TruncationMarkerHits'] = trunc_hits
    ce['ItemsNotEndingInNewline'] = no_nl
    ce['PerItem'] = per
    rec['CommandExecution'] = ce
    rec['TurnUsagePresent'] = any(e.get('type') == 'turn.completed' and isinstance(e.get('usage'), dict) for e in events)
    return rec


def main():
    if len(LOGS) != 9:
        raise SystemExit('expected 9 event logs, found %d' % len(LOGS))
    recs = [analyse(p) for p in LOGS]
    allc = [x for r in recs for x in r['CommandExecution']['PerItem'] if 'Utf8Bytes' in x]
    biggest = max(((x['Utf8Bytes'], r['Log'], x['Id']) for r in recs for x in r['CommandExecution']['PerItem'] if 'Utf8Bytes' in x), default=None)
    summary = {
        'Logs': len(recs),
        'AllComplete': all(r['Completeness']['Complete'] for r in recs),
        'CommandExecutions': len(allc),
        'CommandFieldTypes': dict(sum((collections.Counter(r['CommandExecution']['CommandFieldTypes']) for r in recs), collections.Counter())),
        'Shells': dict(sum((collections.Counter(r['CommandExecution']['Shells']) for r in recs), collections.Counter())),
        'ExitCodes': dict(sum((collections.Counter(r['CommandExecution']['ExitCodes']) for r in recs), collections.Counter())),
        'Statuses': dict(sum((collections.Counter(r['CommandExecution']['Statuses']) for r in recs), collections.Counter())),
        'LargestAggregatedOutput': {'Utf8Bytes': biggest[0], 'Log': biggest[1], 'Id': biggest[2]} if biggest else None,
        'AggregatedOutputUtf8Bytes': stats([x['Utf8Bytes'] for x in allc]),
        'AggregatedOutputUtf8BytesBuckets': buckets([x['Utf8Bytes'] for x in allc]),
        'Totals': dict(sum((collections.Counter(r['CommandExecution']['Totals']) for r in recs), collections.Counter())),
        'ItemsWithStderrMarkers': sum(r['CommandExecution']['ItemsWithStderrMarkers'] for r in recs),
        'ItemsWithStderrThenStdout': sum(r['CommandExecution']['ItemsWithStderrThenStdout'] for r in recs),
        'ItemsWithStdoutStderrStdout': sum(r['CommandExecution']['ItemsWithStdoutStderrStdout'] for r in recs),
        'ItemsBeginningWithStderrText': sum(r['CommandExecution']['ItemsBeginningWithStderrText'] for r in recs),
        'TruncationMarkerHits': sum(len(r['CommandExecution']['TruncationMarkerHits']) for r in recs),
        'ItemsNotEndingInNewline': sum(len(r['CommandExecution']['ItemsNotEndingInNewline']) for r in recs),
        'ItemsWithReplacementChar': sum(r['CommandExecution']['Totals'].get('ItemsWithReplacementChar', 0) for r in recs),
        'SpawnFailures': sum(len(r['CommandExecution']['SpawnFailureEvidence']) for r in recs),
        'SpawnFailuresWhereCommandFieldDiffersFromExecutedCmdLine': sum(1 for r in recs for x in r['CommandExecution']['SpawnFailureEvidence']
                                                                         if x['CommandFieldEqualsExecutedCmdLine'] is False),
        'EchoMarkerItems': sum(len(r['CommandExecution']['EchoMarkerCompleteness']) for r in recs),
        'EchoMarkerItemsAllPresentInOrder': sum(1 for r in recs for x in r['CommandExecution']['EchoMarkerCompleteness'] if x['AllMarkersPresentInOrder']),
        'LargestEchoMarkerItemAllPresent': max((x['Utf8Bytes'] for r in recs for x in r['CommandExecution']['EchoMarkerCompleteness']
                                                if x['AllMarkersPresentInOrder']), default=None),
    }
    fields = set()
    for p in LOGS:
        for ln in L.rbytes(p).split(b'\n'):
            if ln.strip():
                o = json.loads(ln.decode('utf-8'))
                if isinstance(o.get('item'), dict) and o['item'].get('type') == 'command_execution':
                    fields |= set(o['item'].keys())
    summary['CommandExecutionItemFields'] = sorted(fields)
    summary['SeparateStdoutOrStderrField'] = sorted(f for f in fields if f in ('stdout', 'stderr', 'output', 'formatted_output'))
    out = {'Schema': 'i62-a6r3-part-a-events/1', 'GeneratedUtc': L.utc(), 'EvidenceRoot': os.path.relpath(EVROOT, L.WT).replace(os.sep, '/'),
           'WorktreeHead': L.run([L.GIT, '--no-replace-objects', '-C', L.WT, 'rev-parse', 'HEAD'], env=L.genv()).stdout.decode().strip(),
           'StderrMarkers': dict((k, v.pattern) for k, v in STDERR_MARKERS.items()), 'TruncationPattern': TRUNC.pattern,
           'Logs': recs, 'Summary': summary}
    p, h = L.write_result('part-a-events.json', out)
    print(p, h)
    print(json.dumps(L.sanitize(summary), ensure_ascii=True, indent=1, sort_keys=True))


if __name__ == '__main__':
    main()
