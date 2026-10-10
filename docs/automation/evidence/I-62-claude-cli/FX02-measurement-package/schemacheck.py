"""I-62 / FX-02 — medición única de claude-cli: validación del resultado contra el esquema CANÓNICO (operación 5).
Dos validadores independientes que deben coincidir:
  1. validador de biblioteca estándar (jsonschema no está instalado): implementa exactamente el subconjunto de JSON Schema 2020-12 que usa
     el esquema canónico (type —también en lista—, enum, const, properties, required, additionalProperties, items, minLength, maxLength,
     pattern, minimum, maximum, minItems, maxItems) y trata como anotaciones $schema, title, description, default, examples y $comment.
     Cualquier otra palabra clave es un error del validador (falla cerrado; nunca se ignora en silencio). `pattern` sigue ECMA-262 (búsqueda
     no anclada; `$` final = fin de la cadena, sin el salto de línea final que Python admitiría);
  2. Test-Json de PowerShell 7 (JsonSchema.Net, dialecto 2020-12) con -SchemaFile, como en los kits anteriores.
Uso como script: python -I -B schemacheck.py <schema.json> <instancia.json> → JSON con los dos veredictos.
"""
import json
import os
import re
import subprocess
import sys
import tempfile

ANNOTATIONS = {"$schema", "title", "description", "default", "examples", "$comment"}
SUPPORTED = {"type", "enum", "const", "properties", "required", "additionalProperties", "items", "minLength", "maxLength", "pattern",
             "minimum", "maximum", "minItems", "maxItems"} | ANNOTATIONS


class SchemaUnsupported(Exception):
    pass


def _ecma(pattern):
    if pattern.endswith("$") and not pattern.endswith("\\$"):
        pattern = pattern[:-1] + r"\Z"
    return re.compile(pattern, re.ASCII)


def _is_type(v, t):
    if t == "null":
        return v is None
    if t == "boolean":
        return isinstance(v, bool)
    if t == "integer":
        return (isinstance(v, int) and not isinstance(v, bool)) or (isinstance(v, float) and v.is_integer())
    if t == "number":
        return isinstance(v, (int, float)) and not isinstance(v, bool)
    if t == "string":
        return isinstance(v, str)
    if t == "array":
        return isinstance(v, list)
    if t == "object":
        return isinstance(v, dict)
    raise SchemaUnsupported("tipo desconocido: %r" % (t,))


def _json_eq(a, b):
    """Igualdad JSON: true/false nunca igualan a 1/0; 1 == 1.0; listas y objetos, elemento a elemento."""
    if isinstance(a, bool) or isinstance(b, bool):
        return type(a) is type(b) and a == b
    if isinstance(a, list) and isinstance(b, list):
        return len(a) == len(b) and all(_json_eq(x, y) for x, y in zip(a, b))
    if isinstance(a, dict) and isinstance(b, dict):
        return a.keys() == b.keys() and all(_json_eq(a[k], b[k]) for k in a)
    return a == b and type(a) in (int, float, str, type(None)) and type(b) in (int, float, str, type(None)) and (
        isinstance(a, str) == isinstance(b, str))


def _ptr(path):
    return "/" + "/".join(str(p).replace("~", "~0").replace("/", "~1") for p in path) if path else ""


def validate(instance, schema, path=None, errors=None, limit=200):
    path = [] if path is None else path
    errors = [] if errors is None else errors
    if len(errors) >= limit:
        return errors
    if schema is True:
        return errors
    if schema is False:
        errors.append({"Path": _ptr(path), "Keyword": "false", "Message": "esquema false"})
        return errors
    unknown = set(schema) - SUPPORTED
    if unknown:
        raise SchemaUnsupported("palabras clave no soportadas en %s: %s" % (_ptr(path) or "/", sorted(unknown)))
    add = lambda kw, msg: errors.append({"Path": _ptr(path), "Keyword": kw, "Message": msg})
    if "type" in schema:
        ts = schema["type"] if isinstance(schema["type"], list) else [schema["type"]]
        if not any(_is_type(instance, t) for t in ts):
            add("type", "se esperaba %s" % "|".join(ts))
            return errors
    if "enum" in schema and not any(_json_eq(instance, e) for e in schema["enum"]):
        add("enum", "valor fuera del enum")
    if "const" in schema and not _json_eq(instance, schema["const"]):
        add("const", "distinto de const")
    if isinstance(instance, str):
        if "minLength" in schema and len(instance) < schema["minLength"]:
            add("minLength", "longitud %d < %d" % (len(instance), schema["minLength"]))
        if "maxLength" in schema and len(instance) > schema["maxLength"]:
            add("maxLength", "longitud %d > %d" % (len(instance), schema["maxLength"]))
        if "pattern" in schema and not _ecma(schema["pattern"]).search(instance):
            add("pattern", "no cumple %s" % schema["pattern"])
    if isinstance(instance, (int, float)) and not isinstance(instance, bool):
        if "minimum" in schema and instance < schema["minimum"]:
            add("minimum", "%r < %r" % (instance, schema["minimum"]))
        if "maximum" in schema and instance > schema["maximum"]:
            add("maximum", "%r > %r" % (instance, schema["maximum"]))
    if isinstance(instance, list):
        if "minItems" in schema and len(instance) < schema["minItems"]:
            add("minItems", "%d < %d" % (len(instance), schema["minItems"]))
        if "maxItems" in schema and len(instance) > schema["maxItems"]:
            add("maxItems", "%d > %d" % (len(instance), schema["maxItems"]))
        if "items" in schema:
            for i, x in enumerate(instance):
                validate(x, schema["items"], path + [i], errors, limit)
    if isinstance(instance, dict):
        props = schema.get("properties", {})
        for r in schema.get("required", []):
            if r not in instance:
                add("required", "falta %s" % r)
        for k, v in instance.items():
            if k in props:
                validate(v, props[k], path + [k], errors, limit)
            else:
                ap = schema.get("additionalProperties", True)
                if ap is False:
                    errors.append({"Path": _ptr(path + [k]), "Keyword": "additionalProperties", "Message": "propiedad no permitida"})
                elif isinstance(ap, dict):
                    validate(v, ap, path + [k], errors, limit)
    return errors


def stdlib_validate(instance, schema):
    try:
        errs = validate(instance, schema)
        return {"Validator": "stdlib-subset-2020-12", "Valid": not errs, "Errors": errs[:50], "ErrorCount": len(errs)}
    except SchemaUnsupported as e:
        return {"Validator": "stdlib-subset-2020-12", "Valid": None, "Errors": [], "Unsupported": str(e)}


def testjson_validate(instance, schema_path):
    exe = None
    for d in os.environ.get("PATH", "").split(os.pathsep):
        if os.path.isfile(os.path.join(d, "pwsh.exe")):
            exe = os.path.join(d, "pwsh.exe")
            break
    if not exe:
        return {"Validator": "Test-Json (PowerShell 7)", "Valid": None, "NotAvailable": "pwsh no está en el PATH"}
    tmp = tempfile.mkdtemp(prefix="ccli-tj-")
    rp = os.path.join(tmp, "instance.json")
    with open(rp, "w", encoding="utf-8") as fh:
        json.dump(instance, fh, ensure_ascii=False)
    ps = ("try { $null = Get-Content -Raw -LiteralPath $env:CCLI_TJ_INSTANCE | Test-Json -SchemaFile $env:CCLI_TJ_SCHEMA -ErrorAction Stop; 'True' }"
          " catch { 'False: ' + $_.Exception.Message }")
    env = dict(os.environ, CCLI_TJ_INSTANCE=rp, CCLI_TJ_SCHEMA=os.path.abspath(schema_path))
    r = subprocess.run([exe, "-NoProfile", "-NonInteractive", "-Command", ps], capture_output=True, text=True, encoding="utf-8",
                       errors="replace", env=env)
    os.remove(rp)
    os.rmdir(tmp)
    o = r.stdout.strip()
    return {"Validator": "Test-Json (PowerShell 7)", "Valid": o == "True", "Detail": o[:400]}


def check(instance, schema_path):
    schema = json.load(open(schema_path, encoding="utf-8"))
    a = stdlib_validate(instance, schema)
    b = testjson_validate(instance, schema_path)
    agree = (b.get("Valid") is None) or (a["Valid"] == b["Valid"])
    return {"Stdlib": a, "TestJson": b, "Agree": agree, "Valid": bool(a["Valid"]) and (b.get("Valid") in (True, None)) and agree}


if __name__ == "__main__":
    sys.stdout.reconfigure(encoding="utf-8", errors="backslashreplace")
    inst = json.load(open(sys.argv[2], encoding="utf-8"))
    print(json.dumps(check(inst, sys.argv[1]), ensure_ascii=False, indent=1))
