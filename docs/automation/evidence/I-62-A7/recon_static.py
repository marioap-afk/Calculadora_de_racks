# recon_static.py - static comparison of the reconstruction rule v2 (reconstruct_runs_m7.py, candidate of the kit) with the
# reference rule (reconstruct_runs.py, round-3 bench) for the preflight of I-62 A-7 (decisiones §92, punto 6). Stdlib only;
# it parses both files with ast and never imports or runs them.
#
#   python -I recon_static.py <reconstruct_runs.py> <reconstruct_runs_m7.py> <out.json>
#
# For every function and module constant that the v2 header declares unchanged (outer splitter, pwsh lexer and inner
# grammar, envelope extraction, completeness, exit rules, reconstruct except the added Rule / Rendering fields) it compares
# ast.dump(..., include_attributes=False) after a declared, minimal normalization, and lists every top-level definition
# that is new or changed. It does not declare the reconstruction on a real Codex log demonstrated.
import ast, copy, hashlib, json, sys

REF_SHA = '5dc9f78ee40e0f54efc5d14b2ec84dc12dd4a6c1d63b66b0c07a9d2be33338da'
M7_SHA = '1eceeceecfebf994e3bce0af7818dfaa2144b99fb1339897745c2fe9ea7141ab'
UNCHANGED = [('canon', 'helper'), ('sha256', 'helper'), ('split_outer', 'outer splitter'), ('_strategy', 'outer splitter'),
             ('quote_token', 'outer splitter (rendering)'), ('render_command', 'outer splitter (rendering)'),
             ('lex_pwsh', 'pwsh lexer'), ('_abs_exe', 'git / interpreter path check'), ('extract_envelope', 'envelope extraction'),
             ('parse_log', 'completeness')]
CONSTANTS = ['FACTS_SCHEMA', 'ALLOWED_EVENTS', 'TOOL_COMMANDS', 'OUTER_BARE', 'INNER_BARE', 'WIN_ABS', 'GIT_NAME', 'PY_NAME', 'PWSH_NAME']
DECLARED_NEW = ['RULE', 'CMD_NAME', 'CMD_FORBIDDEN', '_abs_shell', '_parse_pwsh_inner', '_parse_cmd_inner']


def defs(tree):
    out = {}
    for n in tree.body:
        if isinstance(n, (ast.FunctionDef, ast.ClassDef)):
            out[n.name] = n
        elif isinstance(n, ast.Assign) and len(n.targets) == 1 and isinstance(n.targets[0], ast.Name):
            out[n.targets[0].id] = n
    return out


def strip_doc(fn):
    fn = copy.deepcopy(fn)
    if fn.body and isinstance(fn.body[0], ast.Expr) and isinstance(getattr(fn.body[0], 'value', None), ast.Constant) \
            and isinstance(fn.body[0].value.value, str):
        fn.body = fn.body[1:]
    return fn


def dump(n):
    return ast.dump(n, include_attributes=False)


def drop_dict_key(node, key):
    """Removes the key `key` from every dict literal under node (in place); returns how many were removed."""
    n = 0
    for d in ast.walk(node):
        if isinstance(d, ast.Dict):
            keep = [(k, v) for k, v in zip(d.keys, d.values) if not (isinstance(k, ast.Constant) and k.value == key)]
            n += len(d.keys) - len(keep)
            d.keys = [k for k, v in keep]
            d.values = [v for k, v in keep]
    return n


def drop_noop_if_pass(node):
    """Removes `if <expr>: pass` statements without else (a statement whose only effect is evaluating <expr>)."""
    removed = []
    for parent in ast.walk(node):
        for field in ('body', 'orelse', 'finalbody'):
            stmts = getattr(parent, field, None)
            if isinstance(stmts, list):
                keep = []
                for s in stmts:
                    if isinstance(s, ast.If) and len(s.body) == 1 and isinstance(s.body[0], ast.Pass) and not s.orelse:
                        removed.append(ast.unparse(s.test))
                    else:
                        keep.append(s)
                setattr(parent, field, keep)
    return removed


def inner_grammar(fn, ref):
    """The statements of the pwsh inner grammar: from `it = lex_pwsh(...)` to the args loop, with the lexed argument
    replaced by a placeholder name (reference: toks[2]; v2: the parameter `inner`)."""
    body = strip_doc(fn).body
    start = next(i for i, s in enumerate(body) if isinstance(s, ast.Assign) and isinstance(s.value, ast.Call)
                 and getattr(s.value.func, 'id', None) == 'lex_pwsh')
    seg = copy.deepcopy(body[start:-1])
    seg[0].value.args = [ast.Name(id='INNER', ctx=ast.Load())]
    ret = body[-1]
    return [dump(s) for s in seg], ret


def main(argv):
    if len(argv) != 4:
        sys.stderr.write('usage: python -I recon_static.py <reconstruct_runs.py> <reconstruct_runs_m7.py> <out.json>\n')
        return 64
    src = {}
    for k, p in (('ref', argv[1]), ('m7', argv[2])):
        with open(p, 'rb') as fh:
            b = fh.read()
        src[k] = {'Sha256': hashlib.sha256(b).hexdigest(), 'Tree': ast.parse(b.decode('utf-8'))}
    R, M = defs(src['ref']['Tree']), defs(src['m7']['Tree'])
    rows = []
    for name, role in UNCHANGED:
        same = name in R and name in M and dump(strip_doc(R[name])) == dump(strip_doc(M[name]))
        rows.append({'Definition': name, 'Role': role, 'Normalization': 'docstring only', 'Identical': same})
    rows.append({'Definition': 'NotReconstructible', 'Role': 'exception class', 'Normalization': 'none',
                 'Identical': dump(R['NotReconstructible']) == dump(M['NotReconstructible'])})
    for c in CONSTANTS:
        rows.append({'Definition': c, 'Role': 'module constant', 'Normalization': 'none', 'Identical': dump(R[c]) == dump(M[c])})
    # inner grammar (pwsh): reference parse_instrument_command vs v2 _parse_pwsh_inner
    seg_r, ret_r = inner_grammar(R['parse_instrument_command'], True)
    seg_m, ret_m = inner_grammar(M['_parse_pwsh_inner'], False)
    rows.append({'Definition': 'parse_instrument_command (ref) [lex_pwsh .. args] = _parse_pwsh_inner (v2) [lex_pwsh .. args]',
                 'Role': 'pwsh inner grammar', 'Normalization': 'docstring; lexed argument toks[2] (ref) / inner (v2) -> one placeholder; '
                 'return compared apart', 'Identical': seg_r == seg_m, 'Statements': len(seg_r),
                 'ReturnRef': ast.unparse(ret_r), 'ReturnV2': ast.unparse(ret_m)})
    # reconstruct: the v2 adds 'Rule' and 'Rendering'; the reference also has a no-op `if ...: pass`
    r_rec, m_rec = strip_doc(R['reconstruct']), strip_doc(M['reconstruct'])
    removed_rule = drop_dict_key(m_rec, 'Rule')
    removed_rend = drop_dict_key(m_rec, 'Rendering')
    noop = drop_noop_if_pass(r_rec)
    rows.append({'Definition': 'reconstruct', 'Role': 'execution identity, exit rules, envelope custody',
                 'Normalization': "v2: dict keys 'Rule' (%d) and 'Rendering' (%d) removed; reference: no-op statements removed: %s"
                                  % (removed_rule, removed_rend, '; '.join('if %s: pass' % x for x in noop) or 'none'),
                 'Identical': dump(r_rec) == dump(m_rec), 'UndeclaredNormalization': bool(noop),
                 'NoopStatements': ['if %s: pass' % x for x in noop]})
    # main: only the usage text differs
    r_main, m_main = strip_doc(R['main']), strip_doc(M['main'])
    for n in ast.walk(r_main):
        if isinstance(n, ast.Constant) and isinstance(n.value, str) and 'usage:' in n.value:
            n.value = n.value.replace('reconstruct_runs.py', 'reconstruct_runs_m7.py')
    rows.append({'Definition': 'main', 'Role': 'command line', 'Normalization': 'usage text (script name)', 'Identical': dump(r_main) == dump(m_main)})
    compared = set([n for n, _ in UNCHANGED] + CONSTANTS + ['NotReconstructible', 'reconstruct', 'main', 'parse_instrument_command'])
    new_or_changed = sorted(set(M) - compared)
    only_ref = sorted(set(R) - set(M))
    changed_dispatch = dump(strip_doc(R['parse_instrument_command'])) != dump(strip_doc(M['parse_instrument_command']))
    guarantees = [
        {'Guarantee': 'identidad de las ejecuciones (orden registrada completada que contiene el blob fijado)', 'Where': 'reconstruct (filtro inst)',
         'Identical': rows[-2]['Identical']},
        {'Guarantee': 'argv: forma declarada, Git e intérprete por ruta absoluta, -I, orden conocida, argumentos simples',
         'Where': 'split_outer, _strategy, lex_pwsh, _abs_exe, gramática interior pwsh',
         'Identical': all(r['Identical'] for r in rows if r['Definition'] in ('split_outer', '_strategy', 'lex_pwsh', '_abs_exe')) and seg_r == seg_m,
         'Widening': 'shell: _abs_shell admite componentes que empiezan por punto (solo la posición del shell); renderizados R2 (-NoProfile) y R3 '
                     '(cmd.exe /c) con gramática cerrada nueva (_parse_cmd_inner)'},
        {'Guarantee': 'bytes: exactamente un sobre, canónico, ASCII, terminado en LF, escrito sin alterar', 'Where': 'extract_envelope; reconstruct (_raw)',
         'Identical': [r for r in rows if r['Definition'] == 'extract_envelope'][0]['Identical'] and rows[-2]['Identical']},
        {'Guarantee': 'resultados y códigos de salida: exit_code entero, status coherente, Error nulo si y solo si exit 0', 'Where': 'reconstruct',
         'Identical': rows[-2]['Identical']},
        {'Guarantee': 'custodia: registro completo y legible; sin él no hay Runs', 'Where': 'parse_log; reconstruct (Reconstructible)',
         'Identical': [r for r in rows if r['Definition'] == 'parse_log'][0]['Identical'] and rows[-2]['Identical']},
    ]
    out = {'Ref': {'Sha256': src['ref']['Sha256'], 'Expected': REF_SHA, 'Match': src['ref']['Sha256'] == REF_SHA},
           'V2': {'Sha256': src['m7']['Sha256'], 'Expected': M7_SHA, 'Match': src['m7']['Sha256'] == M7_SHA},
           'Rows': rows, 'NewOrChangedInV2': new_or_changed, 'DeclaredNew': DECLARED_NEW,
           'UndeclaredNewOrChanged': sorted(set(new_or_changed) - set(DECLARED_NEW)), 'OnlyInRef': only_ref,
           'DispatcherChanged': changed_dispatch, 'Guarantees': guarantees,
           'AllUnchangedIdentical': all(r['Identical'] for r in rows),
           'Demonstrated': 'NO: comparación estática; la reconstrucción sobre un registro real de Codex 26.1013 no está medida (decisiones §92, punto 6)'}
    with open(argv[3], 'wb') as fh:
        fh.write((json.dumps(out, ensure_ascii=False, indent=1) + '\n').encode('utf-8'))
    sys.stdout.write(json.dumps({'AllUnchangedIdentical': out['AllUnchangedIdentical'], 'Undeclared': out['UndeclaredNewOrChanged'],
                                 'NoopRemoved': rows[-2]['NoopStatements']}) + '\n')
    return 0 if out['AllUnchangedIdentical'] and out['Ref']['Match'] and out['V2']['Match'] else 1


if __name__ == '__main__':
    sys.exit(main(sys.argv))
