"""Escribe independent-review-pbt4e-check-prompt.md con el encargo literal de CA-T4e y sus recomprobaciones, sacados de la transcripcion de la sesion.
Derivado de _write_cat4d_prompt.py, con estas diferencias:
- el primer encargo es el prompt de un Agent nuevo (descripcion 'CA-T4e bounded check reviewer'); las recomprobaciones son SendMessage a ese
  mismo agente (AG) que empiezan por 'Recomprobacion de CA-T4e'; no hay recomprobacion a un revisor nuevo;
- el primer encargo nombra rutas locales absolutas; el repositorio es publico, asi que se sustituyen por marcadores fijos, declarados en la cabecera.
  Es la unica diferencia con el texto exacto, y el guion comprueba que no queda ninguna ruta local.
Un mensaje complementario enviado durante una ronda ('Mensaje de la sesion, para tu ') va dentro de la seccion de esa ronda, como subseccion.
Un mensaje repetido en la transcripcion (la misma llamada registrada dos veces) cuenta una vez.
Las rutas locales y los nombres que la redaccion prohibe NO estan en este guion (se custodia en un repositorio publico): los lee de un archivo local
de la sesion, que no se custodia, con la forma {"pairs": [[texto, marcador], ...], "forbidden": "<regex>"}. Los marcadores son los de la cabecera.
Uso: python -I _write_cat4e_prompt.py <transcripcion.jsonl> <salida.md> <redaccion.json>"""
import json, re, sys
path, out, red_path = sys.argv[1], sys.argv[2], sys.argv[3]
AG = 'a407fff18bd38885b'
DESC = 'CA-T4e bounded check reviewer'
FIRST = 'Mensaje de la sesi' + chr(0xF3) + 'n. Haz la **COMPROBACI' + chr(0xD3) + 'N ACOTADA DE PB-T4e (CA-T4e)'
RECHECK = 'Recomprobaci' + chr(0xF3) + 'n de CA-T4e'
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
        if AG not in line and DESC not in line:
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
                    assert first['text'] == text and first['id'] == c.get('id'), 'un segundo encargo de CA-T4e distinto'
                    if first['ts'] is None:
                        first['ts'] = ts
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
assert first is not None and first['ts'], 'sin primer encargo de CA-T4e o sin su hora'
assert not other, ('un mensaje al revisor tras el primero de CA-T4e que no es ronda ni complemento', [x[:80] for x in other])
parts = ['# Encargo de la comprobaci' + chr(0xF3) + 'n acotada de PB-T4e (CA-T4e) (enviado por la sesi' + chr(0xF3) + 'n a un revisor independiente nuevo, '
         'y sus recomprobaciones como mensajes al mismo revisor; texto exacto)', '',
         '**Rutas locales:** el repositorio es p' + chr(0xFA) + 'blico. Las tres rutas locales del primer encargo se sustituyen por `<worktree>`, '
         '`<scratchpad de la sesi' + chr(0xF3) + 'n>` y `<checkout principal>`. Es la ' + chr(0xFA) + 'nica diferencia con el texto enviado.', '']
for i, r in enumerate(rounds, 1):
    title = 'CA-T4e, sobre la v5.5' if i == 1 else 'Recomprobaci' + chr(0xF3) + 'n %d' % (i - 1)
    parts += ['## %d. %s (%s)' % (i, title, r['ts']), '', redact(r['text']).rstrip('\n'), '']
    for j, (ts, text) in enumerate(r['extra'], 1):
        parts += ['### %d.%d. Complemento enviado durante esta ronda (%s)' % (i, j, ts), '', redact(text).rstrip('\n'), '']
open(out, 'w', encoding='utf-8', newline='\n').write('\n'.join(parts))
print('ok', [(r['line'], r['ts'], len(r['text']), len(r['extra'])) for r in rounds])
