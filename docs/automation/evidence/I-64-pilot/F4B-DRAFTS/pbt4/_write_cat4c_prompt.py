"""Escribe independent-review-pbt4c-check-prompt.md con el encargo literal de CA-T4c y sus recomprobaciones, sacados de la transcripcion de la sesion.
Derivado de _write_cat4b_prompt.py. El revisor es el del paquete de PB-T4 (ac44e95c3001c60e3): solo cuentan los mensajes enviados desde el primero de
CA-T4c ("Mensaje de la sesion. Haz la **COMPROBACION ACOTADA DE PB-T4c (CA-T4c)"); las rondas del paquete, las de CA-T4a y las de CA-T4b no entran. Una seccion por ronda (CA-T4c y
cada "Recomprobacion de CA-T4c ..."); un mensaje complementario enviado durante una ronda ("Mensaje de la sesion, para tu §N") va dentro de la seccion
de esa ronda, como subseccion, con su texto exacto. Un mensaje repetido en la transcripcion (la misma llamada registrada dos veces) cuenta una vez.
Uso: python -I _write_cat4b_prompt.py <transcripcion.jsonl> <salida.md>"""
import json, sys
path, out = sys.argv[1], sys.argv[2]
AG = 'ac44e95c3001c60e3'
FIRST = 'Mensaje de la sesi' + chr(0xF3) + 'n. Haz la **COMPROBACI' + chr(0xD3) + 'N ACOTADA DE PB-T4c (CA-T4c)'
RECHECK = 'Recomprobaci' + chr(0xF3) + 'n de CA-T4c'
EXTRA = 'Mensaje de la sesi' + chr(0xF3) + 'n, para tu ' + chr(0xA7)
rounds, other = [], []
with open(path, encoding='utf-8') as f:
    for n, line in enumerate(f, 1):
        if AG not in line:
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
            if not isinstance(c, dict) or c.get('type') != 'tool_use' or c.get('name') != 'SendMessage':
                continue
            inp = c.get('input') or {}
            if inp.get('to') != AG:
                continue
            text = inp.get('message') or ''
            ts = o.get('timestamp')
            if text.startswith(FIRST) or (rounds and text.startswith(RECHECK)):
                if not any(r['text'] == text for r in rounds):
                    rounds.append({'line': n, 'ts': ts, 'text': text, 'extra': []})
            elif rounds and text.startswith(EXTRA):
                if not any(e[1] == text for r in rounds for e in r['extra']):
                    rounds[-1]['extra'].append((ts, text))
            elif rounds and not any(x == text for x in other):
                other.append(text)
assert rounds and rounds[0]['text'].startswith(FIRST), [r['text'][:60] for r in rounds]
assert sum(1 for r in rounds if r['text'].startswith(FIRST)) == 1, 'mas de un primer encargo de CA-T4c'
assert not other, ('un mensaje al revisor tras el primero de CA-T4c que no es ronda ni complemento', [x[:80] for x in other])
parts = ['# Encargo de la comprobaci' + chr(0xF3) + 'n acotada de PB-T4c (CA-T4c) (enviado por la sesi' + chr(0xF3) + 'n al revisor del paquete de PB-T4 '
         'como mensaje; texto exacto)', '']
for i, r in enumerate(rounds, 1):
    title = 'CA-T4c, sobre el contrato en seco y la v5.0' if i == 1 else 'Recomprobaci' + chr(0xF3) + 'n %d' % (i - 1)
    parts += ['## %d. %s (%s)' % (i, title, r['ts']), '', r['text'].rstrip('\n'), '']
    for j, (ts, text) in enumerate(r['extra'], 1):
        parts += ['### %d.%d. Complemento enviado durante esta ronda (%s)' % (i, j, ts), '', text.rstrip('\n'), '']
open(out, 'w', encoding='utf-8', newline='\n').write('\n'.join(parts))
print('ok', [(r['line'], r['ts'], len(r['text']), len(r['extra'])) for r in rounds])
