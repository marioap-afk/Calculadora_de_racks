"""Pasa los patrones locales de privacidad por todo lo que se va a custodiar (M-09 de la comprobacion acotada CA-T5).

Uso: python -I scan_custody.py --patterns <patterns.local.txt> --dir <carpeta preparada para custodiar>

- Lee los patrones (una expresion regular por linea) del archivo local, que nunca se custodia.
- Recorre la carpeta entera. Cada archivo de texto (UTF-8) se analiza; uno que no lo sea se informa como binario.
- Por cada archivo imprime su sha256, su tamano y cuantas coincidencias tiene. Nunca imprime el texto que casa.
- Sale con 0 si nada casa, y con 1 si algo casa: eso va solo por su sha256, no se copia.
- No escribe nada.
"""
import argparse
import hashlib
import os
import re
import sys


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("--patterns", required=True)
    ap.add_argument("--dir", required=True)
    a = ap.parse_args()
    if not os.path.isfile(a.patterns):
        sys.exit("STOP: no existe el archivo de patrones")
    if not os.path.isdir(a.dir):
        sys.exit("STOP: no existe la carpeta")
    pats = [re.compile(ln.rstrip("\n")) for ln in open(a.patterns, encoding="utf-8") if ln.strip()]
    if not pats:
        sys.exit("STOP: el archivo de patrones esta vacio")
    hit_files = 0
    total = 0
    for dp, dn, fn in os.walk(a.dir):
        dn.sort()
        for f in sorted(fn):
            full = os.path.join(dp, f)
            rel = os.path.relpath(full, a.dir).replace(os.sep, "/")
            data = open(full, "rb").read()
            h = hashlib.sha256(data).hexdigest()
            total += 1
            try:
                text = data.decode("utf-8")
            except UnicodeDecodeError:
                print(f"{h}  {rel}  bytes={len(data)} binario (revisar a mano: captura o similar)")
                continue
            n = sum(1 for rx in pats for _ in rx.finditer(text))
            n += sum(1 for rx in pats for _ in rx.finditer(rel))
            print(f"{h}  {rel}  bytes={len(data)} coincidencias={n}")
            if n:
                hit_files += 1
    print(f"archivos: {total}; con coincidencias: {hit_files}")
    sys.exit(1 if hit_files else 0)


if __name__ == "__main__":
    main()
