import json
import sys

# Uso: python -I cat3b_auth.py <json del seco> <fragmento de ruta> ...
# Imprime la Section entera de las autoridades cuya ruta contiene alguno de los fragmentos.
sys.stdout.reconfigure(encoding="utf-8", errors="replace")
c = json.loads(open(sys.argv[1], encoding="utf-8").read())
for x in c["Authorities"]:
    if any(f in x["Path"] for f in sys.argv[2:]):
        print("==", x["Path"], "|", x["Class"])
        print("  ", x["Section"])
