"""Hashes deterministas de la excepcion acotada a 16.8 del intento 3 de F4-T1 (F4-E10).

Uso, desde el worktree:  python n1_hashes.py <BaseSha> <RedSha> <CurrentSha>

Lee el archivo con `git show <sha>:<ruta>` como bytes crudos, sin traducir finales de linea.
- Bloque de regresion: desde el inicio de la linea que contiene BEGIN hasta el final (con su salto) de la linea que contiene END.
- Region de la regla: las lineas de comentario (// ...) contiguas justo encima de la declaracion de BomInAnyIdentifier,
  mas la declaracion hasta el final de la linea que contiene su primer ';'.
- H1: SHA-256 del bloque; debe ser igual en RedSha y en CurrentSha.
- H2: SHA-256 del archivo con la region de la regla sustituida por '<BOM-RULE>' y un salto; igual en RedSha y en CurrentSha
  (prueba que fuera de la region nada cambia del RED al GREEN).
- H3: SHA-256 del archivo de RedSha sin el bloque; debe ser igual al SHA-256 del archivo de BaseSha
  (prueba que el RED solo inserta el bloque).
Imprime un JSON con los valores y Pass = true solo si las tres igualdades se cumplen."""
import hashlib, json, subprocess, sys

PATH = 'tests/RackCad.Tests/Workspace/WorkspaceDraftBoundaryTests.cs'
BEGIN = b'// BEGIN N-1 regression (F4-T1 attempt 3)'
END = b'// END N-1 regression (F4-T1 attempt 3)'
DECL = b'private const string BomInAnyIdentifier ='


def blob(sha):
    return subprocess.run(['git', 'show', sha + ':' + PATH], capture_output=True, check=True).stdout


def sha(b):
    return hashlib.sha256(b).hexdigest().upper()


def line_start(b, i):
    return b.rfind(b'\n', 0, i) + 1


def line_end(b, i):
    j = b.find(b'\n', i)
    return len(b) if j < 0 else j + 1


def block_span(b):
    if b.count(BEGIN) != 1 or b.count(END) != 1:
        return None
    i, j = b.find(BEGIN), b.find(END)
    if j <= i:
        return None
    return line_start(b, i), line_end(b, j)


def rule_span(b):
    if b.count(DECL) != 1:
        return None
    d = b.find(DECL)
    s = line_start(b, d)
    while s > 0:
        ps = line_start(b, s - 1)
        if b[ps:s].strip().startswith(b'//'):
            s = ps
        else:
            break
    semi = b.find(b';', d)
    return (s, line_end(b, semi)) if semi > 0 else None


def main(base, red, cur):
    B, R, C = blob(base), blob(red), blob(cur)
    out = {'Path': PATH, 'BaseSha': base, 'RedSha': red, 'CurrentSha': cur}
    rb, cb = block_span(R), block_span(C)
    rr, cr = rule_span(R), rule_span(C)
    out['BlockFoundInRed'] = rb is not None
    out['BlockFoundInCurrent'] = cb is not None
    out['RuleFoundInRed'] = rr is not None
    out['RuleFoundInCurrent'] = cr is not None
    if None in (rb, cb, rr, cr):
        out['Pass'] = False
        return out
    out['BlockOverlapsRuleInRed'] = not (rb[1] <= rr[0] or rr[1] <= rb[0])
    out['H1Red'] = sha(R[rb[0]:rb[1]])
    out['H1Current'] = sha(C[cb[0]:cb[1]])
    out['H2Red'] = sha(R[:rr[0]] + b'<BOM-RULE>\n' + R[rr[1]:])
    out['H2Current'] = sha(C[:cr[0]] + b'<BOM-RULE>\n' + C[cr[1]:])
    out['H3RedWithoutBlock'] = sha(R[:rb[0]] + R[rb[1]:])
    out['H3Base'] = sha(B)
    out['RuleRegionRed'] = R[rr[0]:rr[1]].decode('utf-8')
    out['RuleRegionCurrent'] = C[cr[0]:cr[1]].decode('utf-8')
    out['Pass'] = (out['H1Red'] == out['H1Current'] and out['H2Red'] == out['H2Current']
                   and out['H3RedWithoutBlock'] == out['H3Base'] and not out['BlockOverlapsRuleInRed'])
    return out


if __name__ == '__main__':
    print(json.dumps(main(*sys.argv[1:4]), ensure_ascii=True, indent=2))
