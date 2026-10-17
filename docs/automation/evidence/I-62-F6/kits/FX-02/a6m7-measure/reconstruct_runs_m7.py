# reconstruct_runs_m7.py - reconstruction rule v2 of the acceptor request Runs from a Codex `--json` session log, proposed
# for the A6-7 measurement (I-62). It is the reference rule reconstruct_runs.py (round-3 bench, SHA-256 5dc9f78e...) plus ONLY
# (1) a closed list of outer renderings documented in r3/r2static (nine real Codex logs): pwsh with -NoProfile, and cmd.exe /c
# with a cmd inner grammar; and (2) in the SHELL position only, a path component may start with a dot: the runtime pwsh of
# Codex is %USERPROFILE%\.cache\codex-runtimes\...\pwsh.exe and the reference rejects '.cache', so it cannot reconstruct ANY
# execution through that shell (finding of the kit). Git and the interpreter keep the reference path check. Everything else,
# including every completeness, envelope and exit rule, is the reference unchanged.
# Stdlib only, no side effects except the optional envelope files written by --env-dir.
#
#   python -I reconstruct_runs_m7.py --log EVENTS.jsonl --blob <pinned blob> [--env-dir DIR]
#
# Fail closed. Procedure (strict):
#   1. Completeness (reference): every line is one JSON object, the file ends with LF, no empty line; event types from a
#      closed list; exactly one thread.started (first), one turn.started (second), one turn.completed (last); every
#      item.started has exactly one later item.completed with the same id and, for command_execution, the same command.
#   2. Instrument executions (reference, A6-3 rule 6 (a)): completed command_execution items whose `command` contains the
#      pinned blob id.
#   3. For each one: argv from the recorded command. Outer layer (Codex rendering, POSIX-like, reference splitter). The outer
#      tokens must be EXACTLY one of (closed list, v2):
#        R1  "<abs path>\pwsh.exe" -Command <inner>               (reference rendering)
#        R2  "<abs path>\pwsh.exe" -NoProfile -Command <inner>    (r2static: od2d / od2b probe 2, Codex 0.160.x)
#        R3  "<abs path>\cmd.exe" /c <inner>                      (r2static: a2p2-block probe 1 and A4-1, Codex 0.162 / app 26.1002)
#      R1, R2 inner (pwsh, reference grammar): & <git> --no-replace-objects cat-file blob <blob> | & <interpreter> -I - <cmd> <args>
#      R3 inner (cmd grammar, new): "<git>" --no-replace-objects cat-file blob <blob> | "<interpreter>" -I - <cmd> <args>
#        tokens separated by exactly one space; <git> and <interpreter> are double-quoted absolute paths (git.exe /
#        python[3[.N]].exe) with no quote inside; the only double quotes of the inner are those four; args are bare
#        [A-Za-z0-9_./:=+-] words; no &, |, <, >, ^, %, !, (, ), ;, `, $, ', no non-ASCII, no control character.
#   4. Envelope (reference): exactly one candidate line in aggregated_output; LF-terminated, ASCII, a JSON object with Schema
#      rackcad-bmin-facts/0-experimental, byte-identical to its canonical serialization. Other lines allowed outside it.
#   5. Exit (reference): exit_code an integer; status completed <=> exit 0, failed <=> exit != 0; exit 0 <=> envelope Error null.
# Any problem in 1-5 makes the whole log NOT reconstructible (no request is built). Each run records its Rendering (R1/R2/R3).
import hashlib
import json
import os
import re
import sys

RULE = 'reconstruct_runs_m7/2'
FACTS_SCHEMA = 'rackcad-bmin-facts/0-experimental'
ALLOWED_EVENTS = ('thread.started', 'turn.started', 'item.started', 'item.updated', 'item.completed', 'turn.completed')
TOOL_COMMANDS = ('identity', 'diff', 'scope', 'contract', 'hashes', 'cleantree', 'read-text', 'read-json', 'trx',
                 'handoff', 'freetext', 'clausemap')
OUTER_BARE = set('abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789+-./:@]_')
INNER_BARE = set('abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789_./:=+-')
WIN_ABS = re.compile(r'[A-Za-z]:\\(?:[^\\/:*?"<>|\x00-\x1f]+\\)*[^\\/:*?"<>|\x00-\x1f]+')
GIT_NAME = re.compile(r'git\.exe', re.I)
PY_NAME = re.compile(r'python(?:3(?:\.[0-9]+)?)?\.exe', re.I)
PWSH_NAME = re.compile(r'pwsh\.exe', re.I)
CMD_NAME = re.compile(r'cmd\.exe', re.I)
CMD_FORBIDDEN = set('&|<>^%!();`$\'')


class NotReconstructible(Exception):
    pass


def canon(o):
    return json.dumps(o, ensure_ascii=True, sort_keys=True, separators=(',', ':'))


def sha256(b):
    return hashlib.sha256(b).hexdigest()


# ------------------------------------------------------------------ outer layer (Codex rendering) - reference, unchanged
def split_outer(s):
    """POSIX-like split of the recorded command (Codex rendering). Strict: raises NotReconstructible."""
    if not isinstance(s, str) or not s or s != s.strip(' '):
        raise NotReconstructible('outer: empty command or leading/trailing space')
    toks, i, n = [], 0, len(s)
    while i < n:
        tok = []
        while i < n and s[i] != ' ':
            c = s[i]
            if c == "'":
                j = s.find("'", i + 1)
                if j < 0:
                    raise NotReconstructible('outer: unterminated single quote')
                tok.append(s[i + 1:j])
                i = j + 1
            elif c == '"':
                i += 1
                buf = []
                while True:
                    if i >= n:
                        raise NotReconstructible('outer: unterminated double quote')
                    c = s[i]
                    if c == '"':
                        i += 1
                        break
                    if c == '\\':
                        if i + 1 < n and s[i + 1] in '\\"':
                            buf.append(s[i + 1])
                            i += 2
                            continue
                        raise NotReconstructible('outer: backslash escape other than \\\\ or \\" in double quotes')
                    if c in '$`':
                        raise NotReconstructible('outer: expansion character in double quotes')
                    buf.append(c)
                    i += 1
                tok.append(''.join(buf))
            elif c in OUTER_BARE:
                tok.append(c)
                i += 1
            else:
                raise NotReconstructible('outer: unquoted special character %r' % c)
        toks.append(''.join(tok))
        if i < n:
            i += 1
            if i >= n or s[i] == ' ':
                raise NotReconstructible('outer: empty token (double or trailing space)')
    return toks


_U, _S, _D = 1, 2, 4


def _strategy(t, i):
    ok = _U | _S | _D
    j = i
    if t[i] == '^':
        ok = _S
        j = i + 1
    while j < len(t):
        c = t[j]
        cur = ok
        if c not in OUTER_BARE:
            cur &= ~_U
        if c in "'\\":
            cur &= ~_S
        if c in '$`!':
            cur &= ~_D
        if c == '^':
            cur = 0
        if cur == 0:
            break
        ok = cur
        j += 1
    return j, ok


def quote_token(t):
    """Codex-like rendering of one argv element (replica measured on custodied logs: shlex-1.3-style chunks)."""
    if t == '':
        return "''"
    out, i = [], 0
    while i < len(t):
        j, ok = _strategy(t, i)
        chunk = t[i:j]
        if ok & _U:
            out.append(chunk)
        elif ok & _S:
            out.append("'" + chunk + "'")
        else:
            out.append('"' + chunk.replace('\\', '\\\\').replace('"', '\\"') + '"')
        i = j
    return ''.join(out)


def render_command(argv):
    return ' '.join(quote_token(a) for a in argv)


# ------------------------------------------------------------------ inner layer (pwsh) - reference, unchanged
def lex_pwsh(s):
    if any(ord(c) > 127 or ord(c) < 32 or ord(c) == 127 for c in s):
        raise NotReconstructible('inner: non-ASCII or control character')
    if s != s.strip(' '):
        raise NotReconstructible('inner: leading/trailing space')
    toks, i, n = [], 0, len(s)
    while i < n:
        c = s[i]
        if c == "'":
            j = i + 1
            buf = []
            while True:
                if j >= n:
                    raise NotReconstructible('inner: unterminated single-quoted literal')
                if s[j] == "'":
                    if j + 1 < n and s[j + 1] == "'":
                        buf.append("'")
                        j += 2
                        continue
                    j += 1
                    break
                buf.append(s[j])
                j += 1
            toks.append(('STR', ''.join(buf)))
            i = j
        elif c == '"':
            j = s.find('"', i + 1)
            if j < 0:
                raise NotReconstructible('inner: unterminated double-quoted literal')
            body = s[i + 1:j]
            if '$' in body or '`' in body:
                raise NotReconstructible('inner: double-quoted literal with expansion or escape')
            toks.append(('STR', body))
            i = j + 1
        elif c in '&|':
            toks.append((c, c))
            i += 1
        elif c in INNER_BARE:
            j = i
            while j < n and s[j] in INNER_BARE:
                j += 1
            toks.append(('BARE', s[i:j]))
            i = j
        else:
            raise NotReconstructible('inner: character %r not allowed (statement, redirection, variable or other syntax)' % c)
        if i < n:
            if s[i] != ' ':
                raise NotReconstructible('inner: character %r not allowed after a token (tokens are separated by one space; '
                                         'statement, redirection, variable or concatenation)' % s[i])
            i += 1
            if i >= n or s[i] == ' ':
                raise NotReconstructible('inner: empty token')
    return toks


def _abs_exe(p, name_re):
    if not WIN_ABS.fullmatch(p):
        return False
    parts = p.split('\\')
    if any(x in ('.', '..') or x != x.strip(' .') for x in parts[1:]):
        return False
    return bool(name_re.fullmatch(parts[-1]))


def _abs_shell(p, name_re):
    """v2, shell position only: as _abs_exe, but a component may START with a dot (the runtime pwsh of Codex lives under
    %USERPROFILE%\\.cache\\codex-runtimes\\...; the reference _abs_exe rejects '.cache' and so every execution through that
    shell). '.', '..', a trailing dot or space and a leading space stay rejected. Git and the interpreter keep _abs_exe."""
    if not WIN_ABS.fullmatch(p):
        return False
    parts = p.split('\\')
    if any(x in ('.', '..') or x != x.rstrip(' .') or x != x.lstrip(' ') or x.strip('.') == '' for x in parts[1:]):
        return False
    return bool(name_re.fullmatch(parts[-1]))


def _parse_pwsh_inner(inner, blob):
    """Reference inner grammar (pwsh), unchanged; returns (git, interpreter, argv)."""
    it = lex_pwsh(inner)

    def tok(k):
        return it[k] if k < len(it) else (None, None)

    if tok(0) != ('&', '&'):
        raise NotReconstructible('inner: does not start with the call operator &')
    if tok(1)[0] != 'STR':
        raise NotReconstructible('inner: git given by bare name or unquoted path')
    if not _abs_exe(tok(1)[1], GIT_NAME):
        raise NotReconstructible('inner: git is not an absolute path to git.exe')
    if tok(2) != ('BARE', '--no-replace-objects'):
        raise NotReconstructible('inner: missing --no-replace-objects')
    if tok(3) != ('BARE', 'cat-file') or tok(4) != ('BARE', 'blob'):
        raise NotReconstructible('inner: not cat-file blob')
    if tok(5) != ('BARE', blob):
        raise NotReconstructible('inner: blob differs from the pinned blob')
    if tok(6) != ('|', '|'):
        raise NotReconstructible('inner: no pipe after the cat-file')
    if tok(7) != ('&', '&'):
        raise NotReconstructible('inner: interpreter not invoked with &')
    if tok(8)[0] != 'STR':
        raise NotReconstructible('inner: interpreter given by bare name or unquoted path')
    if not _abs_exe(tok(8)[1], PY_NAME):
        raise NotReconstructible('inner: interpreter is not an absolute path to python[3[.N]].exe')
    if tok(9) != ('BARE', '-I'):
        raise NotReconstructible('inner: missing -I right after the interpreter')
    if tok(10) != ('BARE', '-'):
        raise NotReconstructible('inner: the tool is not read from stdin (-)')
    if tok(11)[0] != 'BARE' or tok(11)[1] not in TOOL_COMMANDS:
        raise NotReconstructible('inner: unknown tool command')
    args = []
    for kind, val in it[11:]:
        if kind not in ('BARE', 'STR'):
            raise NotReconstructible('inner: extra pipeline or call operator after the tool command')
        args.append(val)
    return tok(1)[1], tok(8)[1], args


# ------------------------------------------------------------------ inner layer (cmd) - v2, closed grammar
def _parse_cmd_inner(inner, blob):
    """cmd /c inner: "<git>" --no-replace-objects cat-file blob <blob> | "<interpreter>" -I - <cmd> <bare args>."""
    if any(ord(c) > 127 or ord(c) < 32 or ord(c) == 127 for c in inner):
        raise NotReconstructible('inner(cmd): non-ASCII or control character')
    bad = sorted(set(c for c in inner if c in CMD_FORBIDDEN and c != '|'))
    if bad:
        raise NotReconstructible('inner(cmd): character(s) %s not allowed (separators, redirection, escape, expansion)' % ''.join(bad))
    if inner.count('"') != 4:
        raise NotReconstructible('inner(cmd): exactly four double quotes required (git and interpreter paths only)')
    if inner.count('|') != 1:
        raise NotReconstructible('inner(cmd): exactly one pipe required')
    if inner != inner.strip(' ') or '  ' in inner:
        raise NotReconstructible('inner(cmd): leading/trailing or double space')
    toks = []
    i, n = 0, len(inner)
    while i < n:
        if inner[i] == '"':
            j = inner.find('"', i + 1)
            if j < 0:
                raise NotReconstructible('inner(cmd): unterminated double quote')
            toks.append(('STR', inner[i + 1:j]))
            i = j + 1
        elif inner[i] == '|':
            toks.append(('|', '|'))
            i += 1
        else:
            j = i
            while j < n and inner[j] != ' ':
                j += 1
            word = inner[i:j]
            if not word or any(c not in INNER_BARE for c in word):
                raise NotReconstructible('inner(cmd): argument %r is not a bare word' % word)
            toks.append(('BARE', word))
            i = j
        if i < n:
            if inner[i] != ' ':
                raise NotReconstructible('inner(cmd): character %r after a token (tokens are separated by one space)' % inner[i])
            i += 1

    def tok(k):
        return toks[k] if k < len(toks) else (None, None)

    if tok(0)[0] != 'STR' or not _abs_exe(tok(0)[1], GIT_NAME):
        raise NotReconstructible('inner(cmd): git is not a double-quoted absolute path to git.exe')
    if tok(1) != ('BARE', '--no-replace-objects'):
        raise NotReconstructible('inner(cmd): missing --no-replace-objects')
    if tok(2) != ('BARE', 'cat-file') or tok(3) != ('BARE', 'blob'):
        raise NotReconstructible('inner(cmd): not cat-file blob')
    if tok(4) != ('BARE', blob):
        raise NotReconstructible('inner(cmd): blob differs from the pinned blob')
    if tok(5) != ('|', '|'):
        raise NotReconstructible('inner(cmd): no pipe after the cat-file')
    if tok(6)[0] != 'STR' or not _abs_exe(tok(6)[1], PY_NAME):
        raise NotReconstructible('inner(cmd): interpreter is not a double-quoted absolute path to python[3[.N]].exe')
    if tok(7) != ('BARE', '-I'):
        raise NotReconstructible('inner(cmd): missing -I right after the interpreter')
    if tok(8) != ('BARE', '-'):
        raise NotReconstructible('inner(cmd): the tool is not read from stdin (-)')
    if tok(9)[0] != 'BARE' or tok(9)[1] not in TOOL_COMMANDS:
        raise NotReconstructible('inner(cmd): unknown tool command')
    args = []
    for kind, val in toks[9:]:
        if kind != 'BARE':
            raise NotReconstructible('inner(cmd): quoted argument or second pipe after the tool command')
        args.append(val)
    return tok(0)[1], tok(6)[1], args


def parse_instrument_command(command, blob):
    toks = split_outer(command)
    if len(toks) == 3 and toks[1] == '-Command' and _abs_shell(toks[0], PWSH_NAME):
        rendering = 'R1'
        git, interp, args = _parse_pwsh_inner(toks[2], blob)
    elif len(toks) == 4 and toks[1] == '-NoProfile' and toks[2] == '-Command' and _abs_shell(toks[0], PWSH_NAME):
        rendering = 'R2'
        git, interp, args = _parse_pwsh_inner(toks[3], blob)
    elif len(toks) == 3 and toks[1] == '/c' and _abs_shell(toks[0], CMD_NAME):
        rendering = 'R3'
        git, interp, args = _parse_cmd_inner(toks[2], blob)
    else:
        # the reference messages for every form outside the closed list
        if len(toks) != 3 or toks[1] != '-Command':
            raise NotReconstructible('outer: not the form "<pwsh>" -Command <inner> (%d tokens)' % len(toks))
        raise NotReconstructible('outer: shell is not an absolute path to pwsh.exe')
    return {'Shell': toks[0], 'Git': git, 'Interpreter': interp, 'Argv': args, 'Rendering': rendering}


# ------------------------------------------------------------------ envelope - reference, unchanged
def extract_envelope(agg):
    if not isinstance(agg, str):
        raise NotReconstructible('aggregated_output is not text')
    pieces = agg.split('\n')
    cands = []
    for k, piece in enumerate(pieces):
        if piece.startswith('{') or FACTS_SCHEMA in piece or '"Schema":' in piece:
            cands.append(k)
    if len(cands) != 1:
        raise NotReconstructible('envelope: %d candidate lines in aggregated_output (exactly 1 required)' % len(cands))
    k = cands[0]
    piece = pieces[k]
    if k == len(pieces) - 1:
        raise NotReconstructible('envelope: candidate line is not LF-terminated (truncated output)')
    if piece.endswith('\r'):
        raise NotReconstructible('envelope: candidate line ends in CRLF')
    try:
        raw = (piece + '\n').encode('ascii')
    except UnicodeEncodeError:
        raise NotReconstructible('envelope: non-ASCII characters')
    try:
        env = json.loads(piece)
    except ValueError:
        raise NotReconstructible('envelope: candidate line is not JSON (truncated or interleaved)')
    if not isinstance(env, dict) or env.get('Schema') != FACTS_SCHEMA:
        raise NotReconstructible('envelope: not an object with Schema ' + FACTS_SCHEMA)
    if raw != (canon(env) + '\n').encode('ascii'):
        raise NotReconstructible('envelope: bytes are not the canonical serialization')
    noise = [p for i, p in enumerate(pieces) if i != k and p != '']
    return raw, env, len(noise)


# ------------------------------------------------------------------ completeness - reference, unchanged
def parse_log(log_bytes):
    probs = []
    events = []
    if not log_bytes.endswith(b'\n'):
        probs.append('log does not end with LF (last line incomplete)')
    lines = log_bytes.split(b'\n')
    if lines and lines[-1] == b'':
        lines = lines[:-1]
    for k, line in enumerate(lines):
        if not line.strip():
            probs.append('empty line %d' % k)
            continue
        try:
            e = json.loads(line.decode('utf-8'))
        except (UnicodeDecodeError, ValueError):
            probs.append('line %d is not a JSON object' % k)
            continue
        if not isinstance(e, dict) or not isinstance(e.get('type'), str):
            probs.append('line %d has no event type' % k)
            continue
        events.append(e)
    types = [e['type'] for e in events]
    bad = sorted(set(t for t in types if t not in ALLOWED_EVENTS))
    if bad:
        probs.append('event types outside the closed list: ' + ','.join(bad))
    if types.count('thread.started') != 1 or not types or types[0] != 'thread.started':
        probs.append('not exactly one thread.started as first event')
    if types.count('turn.started') != 1 or len(types) < 2 or types[1] != 'turn.started':
        probs.append('not exactly one turn.started as second event')
    if types.count('turn.completed') != 1 or not types or types[-1] != 'turn.completed':
        probs.append('not exactly one turn.completed as last event')
    started, completed = {}, {}
    for idx, e in enumerate(events):
        if e['type'] not in ('item.started', 'item.updated', 'item.completed'):
            continue
        it = e.get('item')
        if not isinstance(it, dict) or not isinstance(it.get('id'), str) or not isinstance(it.get('type'), str):
            probs.append('event %d: item without id/type' % idx)
            continue
        if e['type'] == 'item.started':
            if it['id'] in started:
                probs.append('item %s started twice' % it['id'])
            started[it['id']] = (idx, it)
        elif e['type'] == 'item.completed':
            if it['id'] in completed:
                probs.append('item %s completed twice' % it['id'])
            completed[it['id']] = (idx, it)
    for iid, (idx, it) in started.items():
        if iid not in completed:
            probs.append('item %s: item.started without item.completed' % iid)
            continue
        cidx, cit = completed[iid]
        if cidx < idx:
            probs.append('item %s: completed before started' % iid)
        if cit.get('type') != it.get('type') or (it.get('type') == 'command_execution' and cit.get('command') != it.get('command')):
            probs.append('item %s: started and completed differ in type or command' % iid)
    return events, probs, completed


def reconstruct(log_bytes, blob, env_dir=None):
    events, comp_probs, completed = parse_log(log_bytes)
    res = {'Rule': RULE, 'Events': len(events), 'Complete': not comp_probs, 'CompletenessProblems': comp_probs}
    cmd_items = [(idx, it) for idx, it in sorted(completed.values(), key=lambda x: x[0]) if it.get('type') == 'command_execution']
    inst = [(idx, it) for idx, it in cmd_items if isinstance(it.get('command'), str) and blob in it['command']]
    res['CommandExecutions'] = len(cmd_items)
    res['InstrumentExecutions'] = len(inst)
    runs, probs = [], []
    for seq, (idx, it) in enumerate(inst):
        r = {'Seq': seq, 'ItemId': it['id']}
        try:
            p = parse_instrument_command(it['command'], blob)
            r.update({'Argv': p['Argv'], 'Shell': p['Shell'], 'Git': p['Git'], 'Interpreter': p['Interpreter'],
                      'Rendering': p['Rendering']})
            ec = it.get('exit_code')
            if not isinstance(ec, int) or isinstance(ec, bool):
                raise NotReconstructible('exit_code not recorded')
            st = it.get('status')
            if not ((st == 'completed' and ec == 0) or (st == 'failed' and ec != 0)):
                raise NotReconstructible('status %r inconsistent with exit_code %r' % (st, ec))
            raw, env, noise = extract_envelope(it.get('aggregated_output'))
            if (ec == 0) != (env.get('Error') is None):
                raise NotReconstructible('exit_code %d inconsistent with envelope Error %s' % (ec, 'null' if env.get('Error') is None else 'set'))
            r.update({'ExitCode': ec, 'Status': st, 'ErrorRun': ec != 0,
                      'EnvelopeError': (env.get('Error') or {}).get('Code') if isinstance(env.get('Error'), dict) else env.get('Error'),
                      'EnvelopeSha256': sha256(raw), 'NoiseLines': noise, '_raw': raw})
        except NotReconstructible as e:
            r['Problem'] = str(e)
            probs.append('%s: %s' % (it['id'], e))
        runs.append(r)
    res['Problems'] = probs
    res['Reconstructible'] = (not comp_probs) and (not probs)
    if res['Reconstructible'] and env_dir:
        os.makedirs(env_dir, exist_ok=True)
        for r in runs:
            path = os.path.join(env_dir, 'r%02d-%s.json' % (r['Seq'], r['Argv'][0])).replace('\\', '/')
            with open(path, 'wb') as fh:
                fh.write(r['_raw'])
            r['EnvelopePath'] = path
    for r in runs:
        r.pop('_raw', None)
    res['Runs'] = runs
    return res


def main(argv):
    args = dict(zip(argv[::2], argv[1::2]))
    if '--log' not in args or '--blob' not in args:
        sys.stderr.write('usage: python -I reconstruct_runs_m7.py --log EVENTS.jsonl --blob BLOB [--env-dir DIR]\n')
        return 64
    with open(args['--log'], 'rb') as fh:
        data = fh.read()
    res = reconstruct(data, args['--blob'], args.get('--env-dir'))
    sys.stdout.write(canon(res) + '\n')
    return 0 if res['Reconstructible'] else 1


if __name__ == '__main__':
    sys.exit(main(sys.argv[1:]))
