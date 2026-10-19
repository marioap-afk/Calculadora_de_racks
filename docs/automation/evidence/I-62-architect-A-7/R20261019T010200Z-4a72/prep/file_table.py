"""Tabla «SHA-256  bytes  archivo» de kit/, prep/ y de los archivos sueltos de la carpeta del kit, y comprobación de privacidad: ningún archivo
contiene el nombre de usuario de Windows (largo u 8.3), la identidad Git del host ni una dirección de correo (salvo las de prueba de dominios
reservados .invalid y noreply@anthropic.com). Los valores se leen en tiempo de ejecución y nunca se escriben. Excluye prep/base-a6r4/ (copia
verificada del kit acreditado de A-6 ronda 4, custodiado en el repositorio) del examen de privacidad, pero no de la tabla.
Uso: python -I -B file_table.py <carpeta que contiene kit/ y prep/>
"""
import hashlib
import os
import re
import subprocess
import sys

ROOT = sys.argv[1]
EMAIL = re.compile(r"[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,}")
ALLOWED_EMAIL = re.compile(r"(?i)(@[a-z0-9.-]+\.invalid$|^noreply@anthropic\.com$)")


def idents():
    vals = set()
    for k in ("USERNAME", "USER"):
        if os.environ.get(k):
            vals.add(os.environ[k])
    up = os.environ.get("USERPROFILE", "")
    if up:
        vals.add(os.path.basename(os.path.normpath(up)))
        try:
            import ctypes
            buf = ctypes.create_unicode_buffer(512)
            if ctypes.windll.kernel32.GetShortPathNameW(up, buf, 512):
                vals.add(os.path.basename(buf.value))
        except Exception:
            pass
    for key in ("user.name", "user.email"):
        r = subprocess.run(["git", "config", "--global", "--get", key], capture_output=True, text=True)
        if r.returncode == 0 and r.stdout.strip():
            vals.add(r.stdout.strip())
    return {v for v in vals if len(v) >= 3}


IDS = idents()
rows, findings = [], []
for base, dirs, files in os.walk(ROOT):
    dirs[:] = sorted(d for d in dirs if d not in (".tmp", "__pycache__"))
    for n in sorted(files):
        p = os.path.join(base, n)
        rel = os.path.relpath(p, ROOT).replace("\\", "/")
        b = open(p, "rb").read()
        rows.append((hashlib.sha256(b).hexdigest(), len(b), rel))
        if rel.startswith("prep/base-a6r4/"):
            continue
        t = b.decode("utf-8", "replace")
        low = t.lower()
        for v in IDS:
            if v.lower() in low:
                findings.append("%s: identidad del host presente" % rel)
        for m in EMAIL.finditer(t):
            if not ALLOWED_EMAIL.search(m.group(0)):
                findings.append("%s: dirección de correo" % rel)
        if re.search(r"(?i)[a-z]:[\\/]+users[\\/]+(?!<usuario>|public|default|\*)[^\\/\"\s]+", t):
            findings.append("%s: ruta de perfil con nombre" % rel)
for sha, size, rel in rows:
    print("%s %8d  %s" % (sha, size, rel))
print("PRIVACY:", "sin coincidencias" if not findings else sorted(set(findings)))
sys.exit(0 if not findings else 1)
