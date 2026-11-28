"""Escribe la v4 del paquete de PB-T4 a partir de la v3: sustituye secciones enteras (de un titulo al siguiente) y antepone el registro V4.

Uso: python -I make_v4.py <v3.md> <v4.md>
Solo lee <v3.md> y v4_sections.py, y escribe <v4.md>. Cada titulo de inicio tiene que aparecer exactamente una vez.
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
exec(open('v4_sections.py', encoding='utf-8').read(), S)

text = replace_once(text, '(«Editar todo» en el panel del Workspace), v3', '(«Editar todo» en el panel del Workspace), v4')
a = text.find('**Estado:** v3.')
b = text.find('\n', a)
text = text[:a] + S['ESTADO'].strip() + text[b:]
text = replace_once(text, '## Registro de cambios v2 → v3', S['V4_TABLE'].strip() + '\n\n## Registro de cambios v2 → v3')
for start, end, key in [
    ('## 0. En una página', '## 1. Autoridades', 'S0'),
    ('### D-PBT4-01.', '### PB-T4a', 'D01'),
    ('#### D-PBT4-06.', '#### D-PBT4-07.', 'D06'),
    ('#### D-PBT4-11.', '### PB-T4c', 'D11'),
    ('#### D-PBT4-13.', '#### D-PBT4-17.', 'D13_16'),
    ('#### D-PBT4-17.', '### Común', 'D17'),
    ('#### D-PBT4-18.', '#### D-PBT4-19.', 'D18'),
    ('#### D-PBT4-19.', '#### D-PBT4-20.', 'D19'),
    ('## 4. Las guardas, por subtarea', '## 5. La red de pruebas', 'S4'),
    ('### 5.1 PB-T4a', '## 6. Los contratos propuestos', 'S51_55'),
    ('## 6. Los contratos propuestos', '## 7. Riesgos y lecciones aplicadas', 'S6'),
    ('## 7. Riesgos y lecciones aplicadas', '## 8. Las decisiones del Owner', 'S7'),
    ('### 8.2 El orden y las preguntas', '## 10. Lo que queda para PB-T5', 'S82_9'),
    ('## 11. Las precondiciones de emisión', '\x00END', 'S11'),
]:
    if end == '\x00END':
        a = text.find(start)
        assert a >= 0 and text.find(start, a + 1) < 0, ('inicio no unico', start)
        text = text[:a] + S[key].strip() + '\n'
    else:
        text = replace_section(text, start, end, S[key])

FIXES = [
    ('**Cómo se midió:** por objetos git en `7dfc7c21`, la BASE de PB-T3c (F4B-E47). PB-T3a y PB-T3b están CERRADAS; PB-T3c, EMITIDA y en curso. Lo que depende de su GREEN lleva **`<tras el cierre de PB-T3c>`**.',
     '**Cómo se midió:** por objetos git en `7dfc7c21` (v1 a v3) y en `7f0d9aa3` (v4), el cierre de PB-T3c (F4B-E48). PB-T3a, PB-T3b y PB-T3c están CERRADAS.'),
    ('**No se emite.** El orden es: v2 → comprobación acotada → cierre de PB-T3c → medición de la lista de sustituciones → preguntas al Owner → emisión (§8.2).',
     '**No se emite.** El orden es: v4 → comprobación acotada → preguntas al Owner con los números de este paquete → medición de la precondición 6 → emisión (§8.2).'),
    ('- **`<tras el cierre de PB-T3c>`:** el escritor borra vistas.',
     '- **Desde el cierre de PB-T3c** (`7f0d9aa3`), el escritor borra vistas: `RackPilotMutationScope` implementa `IPushBackPilotViewEraser` con `RackViewBlockEraser`.'),
    ('Se vuelve a medir OWNER-2 en `ideas-futuras.md` tras el cierre de PB-T3c',
     'OWNER-2 de PB-T3b se anota en `ideas-futuras.md` con el cierre de PB-T3c (F4B-E48); los riesgos del borrado real (OWNER-1 a OWNER-3 de F4B-E48), al Smoke-PB (§5.5)'),
]
for old, new in FIXES:
    text = replace_once(text, old, new)
assert '<tras el cierre de PB-T3c>' not in text, 'quedan marcadores'

open(dst, 'w', encoding='utf-8', newline='\n').write(text)
print('bytes', len(text.encode('utf-8')))
