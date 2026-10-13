# Fills the @@...@@ placeholders of the round-3 draft with measured values (r3 bench summaries and Git objects) and adds the limits of the
# reference reconstruction after the R2 preconditions table. Output: I-62-A-6.r3.md (the candidate text).
import json, os, re
NL = chr(10)
D = os.path.dirname(os.path.abspath(__file__))
EV = os.path.join(os.path.dirname(D), 'ev')
t = open(os.path.join(D, 'I-62-A-6.r3.draft.md'), encoding='utf-8').read()
s1 = json.load(open(os.path.join(EV, 'r1probes', 'summary.json'), encoding='utf-8'))
pb = json.load(open(os.path.join(EV, 'r2static', 'part-b-pou.json'), encoding='utf-8'))
lf = json.load(open(os.path.join(os.path.dirname(D), 'launchfix', 'test-results.json'), encoding='utf-8'))


def count_cases(obj):
    """(holds, total) from a summary with per-case booleans."""
    rows = obj.get('Cases') if isinstance(obj.get('Cases'), list) else (obj.get('Results') or obj.get('Rows') or [])
    if isinstance(rows, dict):
        rows = list(rows.values())
    key = next((k for k in ('Holds', 'holds', 'AsExpected', 'Pass', 'Ok') if rows and k in rows[0]), None)
    return sum(1 for r in rows if r.get(key)), len(rows)


h1, n1 = count_cases(s1)
hb, nb = count_cases(pb)
hl, nl = count_cases(lf)
assert (h1, n1) == (57, 57) and (hb, nb) == (28, 28) and (hl, nl) == (28, 28), ((h1, n1), (hb, nb), (hl, nl))
F = {
    'R2_REVIEW_TREE8': 'acf2fa30',
    'R2_EVID_TREE8': '411949b0',
    'P_ZERO': 'con `Runs` vacío, el aceptante fijado rechaza siempre (R3 sin datos y no dispensable; R9 sin sobres), también con todas las '
              'comprobaciones dispensables declaradas no aplicables',
    'P_N4': 'la construcción de la ronda 2 deja R4 en `MISMATCH` (entrega ilegible); la de la ronda 3 da `ACCEPT_FACTS` con R4 no aplicable y '
            'R5, R6 y R7 en `MATCH`; con una ejecución con error, rechazo',
    'P_NC1': 'con las cinco comprobaciones evaluadas, R4 en `MATCH`; con `not_run` tras el fallo de `Identity`, R4 no aplicable y salida aceptada; '
             'con `CleanTree` en `not_run`, la construcción de la ronda 2 rechaza y la de la ronda 3 acepta; con una ejecución sobre el `CurrentSha` '
             'inexistente, rechazo',
    'P_ALL': '%d de %d casos conformes: la verificación de la ronda 2 (T1) con el mismo resultado, VERIFY sin ejecuciones, N4, nc1, mutantes de la '
             'solicitud (cuatro solo los detecta la tabla, no el aceptante) y reconstrucción de referencia' % (h1, n1),
    'P_R2A': '`r3/r2static/` (formato de nueve registros reales de Codex) y `r3/r1probes/` (reconstrucción de referencia con fallo cerrado)',
    'P_POU': 'pasos (v)-(viii) del punto de uso, %d de %d casos conformes' % (hb, nb),
    'P_LAUNCH': 'orden congelada `python -B kit/launch.py` comprobada por el propio lanzador; registro escrito siempre que se lanzó el proceso; '
                'comprobación previa de registro con un proceso sustituto; %d de %d pruebas' % (hl, nl),
    'PRE_RECON_STATIC': 'en nueve registros reales de Codex (111 órdenes), `command` es una sola cadena con el shell, que no siempre es la línea '
                        'ejecutada, y `aggregated_output` mezcla la salida estándar y la de error sin separador, con fines de línea mixtos y sin '
                        'truncado visible hasta 59 805 bytes; la reconstrucción de referencia aísla el sobre o falla cerrada en todos los casos '
                        'ensayados (`r3/r1probes/`)',
    'PRE_RECON_REAL': 'la forma exacta con que Codex registra la orden declarada; que el sobre llegue sin alterar ningún byte, con LF, por el pwsh '
                      'del runtime y separable de la salida de error; que no se trunque con los tamaños reales (el sobre de `clausemap` ronda los '
                      '44 KB)',
    'PRE_CMP_STATIC': 'sobre cruzado, ejecución con error y código distinto de cero',
    'PRE_POU_STATIC': '%d de %d casos conformes, con el pwsh 7 del `PATH`' % (hb, nb),
    'PRE_REJ_STATIC': 'registro incompleto, fin de línea CRLF, salida truncada, dos sobres, 15 variantes de forma y códigos incoherentes',
}
for k, v in F.items():
    tok = '@@%s@@' % k
    assert tok in t, tok
    t = t.replace(tok, v)
left = sorted(set(re.findall(r'@@[A-Z0-9_]+@@', t)))
assert not left, left
anchor = '''| rechazo ante evidencia incompleta o inconsistente | A6-3, regla 6 (a); A6-5, reglas 4 y 7 |'''
i = t.index(anchor)
j = t.index(NL + NL, i)
add = (NL + NL + 'Límites de la reconstrucción de referencia: una orden del registro que nombra el blob sin ejecutar el instrumento deja el registro '
       'sin reconstruir (fallo cerrado); una ejecución que no nombrara el blob no se ensayó; y el código de salida registrado solo distingue cero de '
       'distinto de cero. El entorno de la sesión que lanza la invocación tiene que llegar sin variables `GIT_*` (A6-3, regla 3 (vi)): el banco de la '
       'ronda 3 encontró tres heredadas del host, que esa comprobación rechaza.')
t = t[:j] + add + t[j:]
open(os.path.join(D, 'I-62-A-6.r3.md'), 'w', encoding='utf-8', newline=NL).write(t)
print('ok', len(t.encode('utf-8')), 'bytes', t.count(NL), 'lines')
