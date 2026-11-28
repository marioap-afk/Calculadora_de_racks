"""PB-T4b, precondición 6 (diagnóstico de /d/r4m): cuántas veces está cada texto en español de la tabla de
WorkspaceHostHardeningGuardTests (las tres filas que fallan en el GREEN) en WHC y WorkspacePilotSupport.cs, en la base (78a7cef3, por objetos git) y
en el GREEN de la copia. Cuenta el literal entre comillas sobre el texto sin comentarios // ni /* */ (una aproximación de CodeOnly: no entra en
cadenas). Solo lee; escribe solo su salida.

Uso: python -I count_literals_p6.py <checkout> <dir-copia-green> <salida.txt>"""
import re
import subprocess
import sys

repo, copy_dir, out = sys.argv[1:4]
sys.stdout.reconfigure(encoding='utf-8', errors='replace')
ROWS = [('The reread carries no result.', 'La relectura no trajo resultado.', 2),
        ('The draft is being applied or is no longer registered.', 'El borrador se está aplicando o ya no está registrado.', 5),
        ('The rack has no annotation scale.', 'El rack no tiene escala de anotación.', 1)]
WHC = 'src/RackCad.Application/Workspace/WorkspaceHostCore.cs'
SUP = 'src/RackCad.Application/Workspace/WorkspacePilotSupport.cs'
TOKEN = re.compile(r'//[^\n]*|/\*.*?\*/|@"(?:[^"]|"")*"|"(?:\\.|[^"\\\n])*"|\'(?:\\.|[^\'\\\n])*\'', re.S)


def code_only(text):
    return TOKEN.sub(lambda m: '' if m.group(0).startswith('/') else m.group(0), text)


def base(path):
    return subprocess.run(['git', '-C', repo, 'show', '78a7cef3:' + path], capture_output=True).stdout.decode('utf-8').lstrip('﻿')


def green(path):
    return open(copy_dir + '/' + path, 'rb').read().decode('utf-8').lstrip('﻿')


lines = []
for label, get in (('base 78a7cef3', base), ('GREEN de la copia', green)):
    whc, sup = code_only(get(WHC)), code_only(get(SUP))
    for en, es, times in ROWS:
        lit = '"' + es + '"'
        lines.append('%s | %s | en WHC %d, en el soporte %d, en total %d; la guarda exige %d | el ingles entre comillas: %d' % (
            label, es, whc.count(lit), sup.count(lit), whc.count(lit) + sup.count(lit), times, (whc + sup).count('"' + en + '"')))
with open(out, 'w', encoding='utf-8', newline='\n') as f:
    f.write('\n'.join(lines) + '\n')
print('\n'.join(lines))
