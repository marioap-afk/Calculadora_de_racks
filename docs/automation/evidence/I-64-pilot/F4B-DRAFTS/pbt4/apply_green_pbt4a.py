"""PB-T4a (paquete v4.3): escribe en una COPIA EXPORTADA de una revisión el `src/` del GREEN literal que prescribe el paquete.

Uso:
    python -I apply_green_pbt4a.py <dir-copia> [--repo <checkout>] [--rev 7f0d9aa3] [--dry]

- <dir-copia>: la raíz de una copia exportada de la revisión (por ejemplo, con `git archive`), con `src/` y `tests/`. No puede ser un
  checkout ni un worktree: si tiene `.git` (carpeta o archivo), el guion para sin escribir nada.
- --repo: el checkout del que se leen los objetos git de la revisión (por defecto, el directorio actual). Solo se lee, con `git show`.
- --rev: la revisión (por defecto, 7f0d9aa3).
- --dry: comprueba y lista, sin escribir.

Lo que hace, en este orden:
1. construye en memoria el GREEN literal de PB-T4a con green_pbt4a.py (WHC, WEI, NH y RIA editados; la interfaz nueva y
   Draft/PushBackOriginByText.cs, nuevos);
2. comprueba que cada uno de los cuatro archivos existentes, en la copia, es el de la revisión (con los saltos de línea normalizados a
   LF) y que los dos nuevos no existen; si algo no casa, para sin escribir nada;
3. escribe los seis archivos con LF, en UTF-8 y sin BOM nuevo (si el archivo de la revisión lo tenía, se conserva), y no toca `tests/`
   ni ningún otro archivo;
4. imprime cada ruta escrita, su tamaño y su sha256, y al final el sha256 del conjunto (las líneas «<sha256>  <ruta>», ordenadas).

Lo que no hace: no aplica el `Skip` de CT-PBT3A-12 (está en `tests/`, en el parche del esqueleto), ni escribe las clases de prueba
nuevas de PB-T4a, que son del Worker. Con el `Skip` puesto, la suite completa de Core sobre la copia debe dar 19 129 = 19 122 + 0 + 7;
sin él, 19 129 = 19 122 + 1 (CT-PBT3A-12) + 6. Y ningún `warning CS` (los MSB3277 son de las referencias locales de AutoCAD).
"""
import hashlib
import importlib.util
import os
import sys

sys.stdout.reconfigure(encoding='utf-8', errors='replace')
args = sys.argv[1:]
if not args or args[0].startswith('--'):
    raise SystemExit(__doc__)
target = os.path.abspath(args[0])
repo = os.getcwd()
rev = '7f0d9aa3'
dry = False
i = 1
while i < len(args):
    if args[i] == '--repo':
        repo, i = args[i + 1], i + 2
    elif args[i] == '--rev':
        rev, i = args[i + 1], i + 2
    elif args[i] == '--dry':
        dry, i = True, i + 1
    else:
        raise SystemExit('argumento desconocido: ' + args[i])

if not os.path.isdir(os.path.join(target, 'src')) or not os.path.isdir(os.path.join(target, 'tests')):
    raise SystemExit('PARA: %s no parece la raíz de una copia (faltan src/ o tests/)' % target)
if os.path.exists(os.path.join(target, '.git')):
    raise SystemExit('PARA: %s tiene .git: es un checkout o un worktree, no una copia exportada. No se escribe nada.' % target)

spec = importlib.util.spec_from_file_location('gp', os.path.join(os.path.dirname(os.path.abspath(__file__)), 'green_pbt4a.py'))
gp = importlib.util.module_from_spec(spec)
spec.loader.exec_module(gp)
base, green = gp.green(repo, rev)

problems = []
for rel in sorted(green):
    path = os.path.join(target, *rel.split('/'))
    if rel in base:
        if not os.path.isfile(path):
            problems.append('falta en la copia: ' + rel)
            continue
        with open(path, 'rb') as f:
            have = f.read().decode('utf-8').replace('\r\n', '\n')
        if have != base[rel]:
            problems.append('no es el de %s: %s' % (rev, rel))
    elif os.path.exists(path):
        problems.append('ya existe (es nuevo en PB-T4a): ' + rel)
if problems:
    raise SystemExit('PARA, sin escribir nada:\n  ' + '\n  '.join(problems))

lines = []
for rel in sorted(green):
    data = green[rel].encode('utf-8')
    path = os.path.join(target, *rel.split('/'))
    if not dry:
        os.makedirs(os.path.dirname(path), exist_ok=True)
        with open(path, 'wb') as f:
            f.write(data)
    digest = hashlib.sha256(data).hexdigest()
    lines.append('%s  %s' % (digest, rel))
    print('%s %-70s %7d bytes  %s' % ('comprobado' if dry else 'escrito', rel, len(data), digest))
print('conjunto  %s  (%d archivos; sha256 de las líneas «<sha256>  <ruta>» ordenadas, con LF)' % (
    hashlib.sha256(('\n'.join(lines) + '\n').encode('utf-8')).hexdigest(), len(lines)))
