"""Escribe independent-review-v1-rechecks-prompt.md: los encargos literales de las rondas §13, §14 y §15 de la revision independiente del paquete
de PB-T4 (independent-review-v1.md), sacados de la transcripcion de la sesion. El encargo de la v1 es independent-review-v1-prompt.md, que se
custodia tal cual. Derivado de _write_cat3c_prompt.py: una seccion por ronda, en el orden de envio; un mensaje complementario enviado durante una
ronda ("Mensaje de la sesion, para tu §N") va dentro de la seccion de esa ronda, con su texto exacto. Solo lee los SendMessage al revisor.
Uso: python -I _write_ir4_prompt.py <transcript.jsonl> <salida.md>"""
import json, sys
path, out = sys.argv[1], sys.argv[2]
AG = 'ac44e95c3001c60e3'
SES = 'Mensaje de la sesi' + chr(0xF3) + 'n. Haz la '
ROUNDS = [('13', 'Comprobaci' + chr(0xF3) + 'n acotada de la v2', SES + '**comprobaci' + chr(0xF3) + 'n acotada de la v2'),
          ('14', 'Recomprobaci' + chr(0xF3) + 'n de la v3', SES + '**recomprobaci' + chr(0xF3) + 'n de la v3'),
          ('15', 'Recomprobaci' + chr(0xF3) + 'n de la v4', SES + '**recomprobaci' + chr(0xF3) + 'n corta de la v4')]
EXTRA = 'Mensaje de la sesi' + chr(0xF3) + 'n, para tu ' + chr(0xA7)
found = {k: [] for k, _, _ in ROUNDS}
extra = {k: [] for k, _, _ in ROUNDS}
order = []
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
            hit = [k for k, _, pre in ROUNDS if text.startswith(pre)]
            if hit:
                if not any(t == text for _, t in found[hit[0]]):
                    found[hit[0]].append((o.get('timestamp'), text))
                    order.append(hit[0])
            elif order and text.startswith(EXTRA):
                if not any(t == text for _, t in extra[order[-1]]):
                    extra[order[-1]].append((o.get('timestamp'), text))
for k, _, pre in ROUNDS:
    assert len(found[k]) == 1, ('la ronda %s: %d mensajes que empiezan por su prefijo' % (k, len(found[k])), pre[:60])
assert order == [k for k, _, _ in ROUNDS], ('el orden de las rondas', order)
parts = ['# Encargos de las recomprobaciones de la revisi' + chr(0xF3) + 'n independiente del paquete de PB-T4 (enviados por la sesi' + chr(0xF3) + 'n '
         'al revisor como mensajes; texto exacto)', '',
         'El encargo de la revisi' + chr(0xF3) + 'n de la v1 es `independent-review-v1-prompt.md`. Cada secci' + chr(0xF3) + 'n de aqu' + chr(0xED) + ' es el '
         'encargo de la ronda del mismo n' + chr(0xFA) + 'mero en `independent-review-v1.md`.', '']
for i, (k, title, _) in enumerate(ROUNDS, 1):
    ts, text = found[k][0]
    parts += ['## %d. %s%s. %s (%s)' % (i, chr(0xA7), k, title, ts), '', text.rstrip('\n'), '']
    for j, (ets, etext) in enumerate(extra[k], 1):
        parts += ['### %d.%d. Complemento enviado durante esta ronda (%s)' % (i, j, ets), '', etext.rstrip('\n'), '']
open(out, 'w', encoding='utf-8', newline='\n').write('\n'.join(parts))
print('ok', [(k, found[k][0][0], len(found[k][0][1]), len(extra[k])) for k, _, _ in ROUNDS])
