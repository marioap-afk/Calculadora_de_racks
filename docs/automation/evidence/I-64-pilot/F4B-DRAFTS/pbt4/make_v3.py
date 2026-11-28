"""Escribe la v3 del paquete de PB-T4 a partir de la v2: sustituye secciones enteras (de un titulo al siguiente) y antepone el registro V3.

Uso: python -I make_v3.py <v2.md> <v3.md>
Solo lee <v2.md> y escribe <v3.md>. Cada titulo de inicio tiene que aparecer exactamente una vez.
"""
import sys

src, dst = sys.argv[1:3]
text = open(src, encoding='utf-8').read()


def replace_section(text, start, end, new):
    a = text.find(start)
    assert a >= 0 and text.find(start, a + 1) < 0, ('inicio no unico', start)
    b = text.find(end, a + len(start))
    assert b >= 0, ('fin no encontrado', end)
    return text[:a] + new.rstrip('\n') + '\n\n' + text[b:]


def replace_once(text, old, new):
    assert text.count(old) == 1, ('no unico', old[:60])
    return text.replace(old, new)


S = {}
exec(open(sys.argv[0].replace('make_v3.py', 'v3_sections.py'), encoding='utf-8').read(), S)

text = replace_once(text, '(«Editar todo» en el panel del Workspace), v2', '(«Editar todo» en el panel del Workspace), v3')
a = text.find('**Estado:** v2.')
b = text.find('\n', a)
text = text[:a] + S['ESTADO'].strip() + text[b:]
text = replace_once(text, '## Registro de cambios v1 → v2', S['V3_TABLE'].strip() + '\n\n## Registro de cambios v1 → v2')
for start, end, key in [
    ('## 0. En una página', '## 1. Autoridades', 'S0'),
    ('#### D-PBT4-03.', '### PB-T4b', 'D03'),
    ('#### D-PBT4-06.', '#### D-PBT4-07.', 'D06'),
    ('#### D-PBT4-08.', '#### D-PBT4-09.', 'D08'),
    ('#### D-PBT4-11.', '### PB-T4c', 'D11'),
    ('#### D-PBT4-13.', '#### D-PBT4-17.', 'D13_16'),
    ('#### D-PBT4-18.', '#### D-PBT4-19.', 'D18'),
    ('## 4. Las guardas, por subtarea', '## 5. La red de pruebas', 'S4'),
    ('### 5.1 PB-T4a', '### 5.4 La CI del RED', 'S51_53'),
    ('## 6. Los contratos propuestos', '## 7. Riesgos y lecciones aplicadas', 'S6'),
    ('### 8.2 El orden y las preguntas', '## 10. Lo que queda para PB-T5', 'S82_9'),
]:
    text = replace_section(text, start, end, S[key])

open(dst, 'w', encoding='utf-8', newline='\n').write(text)
print('bytes', len(text.encode('utf-8')))
