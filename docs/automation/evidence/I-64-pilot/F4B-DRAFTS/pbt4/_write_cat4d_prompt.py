"""Escribe independent-review-pbt4d-check-prompt.md con el encargo literal de CA-T4d y sus recomprobaciones, sacados de la transcripcion de la sesion.
Derivado de _write_cat4c_prompt.py, con dos diferencias:
- el primer encargo de CA-T4d no fue un SendMessage a un revisor ya abierto, sino el prompt de un Agent nuevo (descripcion 'CA-T4d bounded check reviewer');
  las recomprobaciones son SendMessage a ese mismo agente (AG) que empiezan por 'Recomprobacion de CA-T4d';
- el primer encargo nombra rutas locales absolutas; el repositorio es publico, asi que se sustituyen por marcadores fijos, declarados en la cabecera.
  Es la unica diferencia con el texto exacto, y el guion comprueba que no queda ninguna ruta local.
Un mensaje complementario enviado durante una ronda ('Mensaje de la sesion, para tu ') va dentro de la seccion de esa ronda, como subseccion.
Un mensaje repetido en la transcripcion (la misma llamada registrada dos veces) cuenta una vez.
Las rutas locales y los nombres que la redaccion prohibe NO estan en este guion (se custodia en un repositorio publico): los lee de un archivo local
de la sesion, que no se custodia, con la forma {"pairs": [[texto, marcador], ...], "forbidden": "<regex>"}. Los marcadores son los de la cabecera.
Uso: python -I _write_cat4d_prompt.py <transcripcion.jsonl> <salida.md> <redaccion.json>"""
import json, re, sys
path, out, red_path = sys.argv[1], sys.argv[2], sys.argv[3]
AG = 'ab6b5874c1a331e49'
DESC = 'CA-T4d bounded check reviewer'
# La recomprobacion 2 (solo el diff de la v5.3 a la v5.4) la hizo un revisor NUEVO, porque el primero estaba cerca de su limite de contexto.
DESC2 = 'CA-T4d recheck 2 diff reviewer'
FIRST2 = 'Mensaje de la sesi' + chr(0xF3) + 'n. Haz la **RECOMPROBACI' + chr(0xD3) + 'N 2 DE CA-T4d, SOLO DEL DIFF'
FIRST = 'Mensaje de la sesi' + chr(0xF3) + 'n. Haz la **COMPROBACI' + chr(0xD3) + 'N ACOTADA DE PB-T4d (CA-T4d)'
RECHECK = 'Recomprobaci' + chr(0xF3) + 'n de CA-T4d'
EXTRA = 'Mensaje de la sesi' + chr(0xF3) + 'n, para tu '
_red = json.load(open(red_path, encoding='utf-8'))
# Marcadores, del mas largo al mas corto.
REDACT = sorted([(a, b) for a, b in _red['pairs']], key=lambda p: -len(p[0]))
MARKERS = {'<worktree>', '<scratchpad de la sesi' + chr(0xF3) + 'n>' + chr(92) + 'hs65', '<checkout principal>'}
assert {b for _, b in REDACT} == MARKERS, 'los marcadores de la redaccion no son los de la cabecera'
# Ademas de los nombres del archivo local, ninguna ruta con unidad.
LOCAL_RE = re.compile(r'(?i)(?:[A-Z]:[\\/])|' + _red['forbidden'])


def redact(text):
    for a, b in REDACT:
        text = text.replace(a, b)
    m = LOCAL_RE.search(text)
    assert m is None, ('queda una ruta o un nombre local', text[max(0, m.start() - 40):m.end() + 40])
    return text


first, rounds, other = None, [], []
with open(path, encoding='utf-8') as f:
    for n, line in enumerate(f, 1):
        if AG not in line and DESC not in line and DESC2 not in line:
            continue
        try:
            o = json.loads(line)
        except Exception:
            continue
        msg = o.get('message') or {}
        content = msg.get('content') if isinstance(msg, dict) else None
        if not isinstance(content, list):
            continue
        for c in content:
            if not isinstance(c, dict) or c.get('type') != 'tool_use':
                continue
            inp = c.get('input') or {}
            ts = o.get('timestamp')
            if c.get('name') == 'Agent' and inp.get('description') == DESC:
                text = inp.get('prompt') or ''
                assert text.startswith(FIRST), text[:80]
                if first is None:
                    first = {'line': n, 'ts': ts, 'text': text, 'extra': [], 'id': c.get('id')}
                    rounds.append(first)
                else:
                    assert first['text'] == text and first['id'] == c.get('id'), 'un segundo encargo de CA-T4d distinto'
                    if first['ts'] is None:
                        first['ts'] = ts
                continue
            if c.get('name') == 'Agent' and inp.get('description') == DESC2:
                text = inp.get('prompt') or ''
                assert text.startswith(FIRST2), text[:80]
                prev = [r for r in rounds if r.get('id') == c.get('id')]
                if prev:
                    assert prev[0]['text'] == text, 'un segundo encargo de la recomprobacion 2 distinto'
                    if prev[0]['ts'] is None:
                        prev[0]['ts'] = ts
                else:
                    assert first is not None and len(rounds) >= 2, 'la recomprobacion 2 antes de la 1'
                    rounds.append({'line': n, 'ts': ts, 'text': text, 'extra': [], 'id': c.get('id'), 'new': True})
                continue
            if c.get('name') != 'SendMessage' or inp.get('to') != AG:
                continue
            text = inp.get('message') or ''
            if rounds and text.startswith(RECHECK):
                if not any(r['text'] == text for r in rounds):
                    rounds.append({'line': n, 'ts': ts, 'text': text, 'extra': []})
            elif rounds and text.startswith(EXTRA):
                if not any(e[1] == text for r in rounds for e in r['extra']):
                    rounds[-1]['extra'].append((ts, text))
            elif rounds and not any(x == text for x in other):
                other.append(text)
assert first is not None and first['ts'], 'sin primer encargo de CA-T4d o sin su hora'
assert not other, ('un mensaje al revisor tras el primero de CA-T4d que no es ronda ni complemento', [x[:80] for x in other])
parts = ['# Encargo de la comprobaci' + chr(0xF3) + 'n acotada de PB-T4d (CA-T4d) (enviado por la sesi' + chr(0xF3) + 'n a un revisor independiente nuevo, '
         'la recomprobaci' + chr(0xF3) + 'n 1 como mensaje al mismo revisor, y la recomprobaci' + chr(0xF3) + 'n 2, solo del diff, a otro revisor nuevo; texto exacto)', '',
         '**Rutas locales:** el repositorio es p' + chr(0xFA) + 'blico. Las tres rutas locales del primer encargo se sustituyen por `<worktree>`, '
         '`<scratchpad de la sesi' + chr(0xF3) + 'n>` y `<checkout principal>`. Es la ' + chr(0xFA) + 'nica diferencia con el texto enviado.', '']
for i, r in enumerate(rounds, 1):
    title = 'CA-T4d, sobre la v5.2' if i == 1 else 'Recomprobaci' + chr(0xF3) + 'n %d' % (i - 1)
    if r.get('new'):
        title += ', a un revisor independiente nuevo (solo el diff de la v5.3 a la v5.4)'
    parts += ['## %d. %s (%s)' % (i, title, r['ts']), '', redact(r['text']).rstrip('\n'), '']
    for j, (ts, text) in enumerate(r['extra'], 1):
        parts += ['### %d.%d. Complemento enviado durante esta ronda (%s)' % (i, j, ts), '', redact(text).rstrip('\n'), '']
open(out, 'w', encoding='utf-8', newline='\n').write('\n'.join(parts))
print('ok', [(r['line'], r['ts'], len(r['text']), len(r['extra'])) for r in rounds])
