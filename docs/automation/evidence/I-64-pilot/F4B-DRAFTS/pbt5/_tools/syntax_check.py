"""Compila en memoria (sin ejecutar y sin escribir .pyc) los guiones de pbt5 que se le pasan."""
import os
import sys

here = os.path.dirname(os.path.abspath(__file__))
bad = 0
for name in sys.argv[1:]:
    p = os.path.join(here, name)
    try:
        compile(open(p, encoding="utf-8").read(), p, "exec")
        print("OK  ", name)
    except SyntaxError as e:
        bad += 1
        print("MAL ", name, e)
sys.exit(1 if bad else 0)
