"""La lectura de la tabla de RR-3 de PB-T4b (section 6.0 del paquete v4.9, sus filas de PB-T4b), de la cadena (el generador del contrato emitido,
make_pbt4b_contract.py, no trae rr3_table_check). Sigue la regla de la lectura de PB-T4a (make_pbt4a_contract.py, custodiada en X: los literales entre
comillas invertidas, fuera de parentesis o dentro de uno que solo los enumera; '\\|' de la tabla es '|'), y resuelve SOLO lo que el paquete fija:
  - "las de esas CT en section 5.2[, literales]" y "las de CT-NN en section 5.2": los literales de la columna "Las lineas (literales del paquete)" de esas
    filas de la tabla de section 5.2, cuya columna Clase tiene que nombrar la clase;
  - "las <N> `<prefijo>...`": los literales de esa expansion que empiezan por el prefijo, que tienen que ser exactamente N (ya estan en la lista);
  - "las fichas y los terminos de la fila de CT-PBT4A-08": los dos grupos enumerados de la fila de PB-T4a que nombra CT-08 ("las ocho fichas de G-L1 (...)"
    y "los 15 terminos de la pureza (...)"), con su cuenta;
  - "<los|las> <N> <nombre> (`a`, ... y `z`)": un grupo que enumera, con su cuenta.
Lo que queda sin resolver (cualquier otra palabra que no sea un conector, un literal con un marcador <x> o con puntos suspensivos fuera de esas formas)
deja la clase PENDIENTE, con el texto que falta: falla cerrada, la cadena para si la reparacion toca esa clase. Las filas de las copias, de sus ayudantes y
miembros, y la de "Las 356 copias mecanicas, las 27 de la interfaz y el kit" son de texto entero ("no admite(n) reparacion (RR-2)"): tipo 'entero'.
rr3_table_check(sec_lines, classes, sub='PB-T4b', pkg_lines=None) -> (errores, info): info[clase] = {'tipo': 'ct'|'entero', 'lineas', 'valores',
'pendiente', 'filas'}, solo para las clases pedidas. Un error es una fila con otra forma, una clase pedida sin fila o una fila de section 5.2 que no casa."""
# ---- rr3_table_check (inicio)
def rr3_table_check(sec_lines, classes, sub='PB-T4b', pkg_lines=None):
    import re
    ELL = chr(0x2026)
    NUM = {'un': 1, 'una': 1, 'uno': 1, 'dos': 2, 'tres': 3, 'cuatro': 4, 'cinco': 5, 'seis': 6, 'siete': 7, 'ocho': 8, 'nueve': 9, 'diez': 10,
           'once': 11, 'doce': 12, 'trece': 13, 'catorce': 14, 'quince': 15}
    num = lambda w: int(w) if w.isdigit() else NUM.get(w.lower())
    STOP = {'los', 'las', 'fragmentos', 'y', 'literales'}

    def scan(seg):
        lits, groups, stack, gid, buf, inlit = [], {}, [], 0, '', False
        for ch in seg:
            if inlit:
                if ch == '`':
                    lits.append((buf, stack[-1] if stack else None))
                    buf, inlit = '', False
                else:
                    buf += ch
                continue
            if ch == '`':
                inlit = True
            elif ch == '(':
                gid += 1
                stack.append(gid)
                groups[gid] = ''
            elif ch == ')':
                if stack:
                    stack.pop()
            elif stack:
                groups[stack[-1]] += ch
        return [x.replace('\\|', '|') for x, g in lits if g is None or re.fullmatch(r'[\s,y]*', groups[g])]

    def placeholder(x):
        return bool(re.search(r'(?:^|[^\w.])<\w+>', x)) or ELL in x  # <n> tras un no-identificador; Lazy<T> no lo es

    def cts_of(cell):
        m = re.search(r'\((?:UI; )?CT-([0-9 ,ay]+)\)', cell)
        if not m:
            return None
        out = []
        for part in re.split(r', | y ', m.group(1).strip()):
            r = re.fullmatch(r'(\d\d) a (\d\d)', part.strip())
            if r:
                out += ['%02d' % k for k in range(int(r.group(1)), int(r.group(2)) + 1)]
            elif re.fullmatch(r'\d\d', part.strip()):
                out.append(part.strip())
            else:
                return None
        return out

    pkg = pkg_lines or []
    s52 = []
    if pkg:
        i0 = next((i for i, l in enumerate(pkg) if l.startswith('### 5.2 ')), None)
        if i0 is not None:
            i1 = next((i for i in range(i0 + 1, len(pkg)) if pkg[i].startswith('### ')), len(pkg))
            s52 = pkg[i0:i1]
    rows52 = {}
    for l in s52:
        m = re.match(r'\| \*\*(\d\d)\*\* \| ', l)
        if m:
            rows52[m.group(1)] = l.strip()[2:-2].split(' | ')
    errors, info, pend, kind, filas = [], {}, {}, {}, {}
    lines_of, values_of = {}, {}
    entero_all = False
    rows = [l for l in sec_lines if l.startswith('| %s | ' % sub)]
    t4a = [l for l in sec_lines if l.startswith('| PB-T4a | ') and 'de CT-08,' in l]

    def take(cls, part, body, add):
        """Resuelve una parte (Lineas o Valores) de una fila de clase de CT; devuelve los literales y anota lo pendiente."""
        rest = body
        out = []
        m = re.search(r'las de (esas CT|CT-(\d\d)) en §5\.2(?:, literales)?', rest)
        exp = []
        if m:
            cts = filas[cls][-1] if m.group(1) == 'esas CT' else [m.group(2)]
            for n in cts:
                r = rows52.get(n)
                if r is None or len(r) < 4:
                    errors.append('la fila de %s cita CT-%s de section 5.2 y section 5.2 no la tiene (o no tiene su columna de lineas)' % (cls, n))
                    continue
                if not re.search(r'`%s`' % re.escape(cls), r[3]):
                    errors.append('CT-%s de section 5.2 no es de la clase %s (columna Clase: %s)' % (n, cls, ascii(r[3][:60])))
                    continue
                for x in scan(r[2]):
                    if placeholder(x):
                        pend.setdefault(cls, []).append('CT-%s de section 5.2, un literal con un marcador: %s' % (n, ascii(x)))
                    else:
                        exp.append(x)
            out += exp
            rest = rest.replace(m.group(0), ' ')
        for m in list(re.finditer(r'las (\w+) `([^`]*)' + ELL + '`', rest)):
            n, pre = num(m.group(1)), m.group(2)
            hits = [x for x in exp if x.startswith(pre)]
            if n is None or len(hits) != n:
                pend.setdefault(cls, []).append('"%s": %s literales de section 5.2 empiezan por %s' % (ascii(m.group(0)), len(hits), ascii(pre)))
            rest = rest.replace(m.group(0), ' ')
        m = re.search(r'las fichas y los términos de la fila de CT-PBT4A-08', rest)
        if m:
            got = []
            if len(t4a) == 1:
                for rx in (r'las (\w+) fichas de G-L1 \(([^)]*)\)', r'los (\w+) términos de la pureza \(([^)]*)\)'):
                    g = re.search(rx, t4a[0])
                    lits = re.findall(r'`([^`]*)`', g.group(2)) if g else []
                    if not g or num(g.group(1)) != len(lits):
                        pend.setdefault(cls, []).append('la fila de CT-PBT4A-08 no da el grupo %s con su cuenta' % ascii(rx[:30]))
                    got += lits
            else:
                errors.append('la fila de %s cita la de CT-PBT4A-08 y no hay una sola fila de PB-T4a con CT-08' % cls)
            out += got
            rest = rest.replace(m.group(0), ' ')
        for m in list(re.finditer(r'(?:los|las) (\w+) ([^`(),;]+?) \(((?:`[^`]*`(?:, | y )?)+)\)', rest)):
            n, lits = num(m.group(1)), re.findall(r'`([^`]*)`', m.group(3))
            if n is None or n != len(lits):
                pend.setdefault(cls, []).append('"%s": la cuenta no es la de sus literales' % ascii(m.group(0)[:80]))
            out += lits
            rest = rest.replace(m.group(0), ' ')
        for x in scan(rest):
            if placeholder(x):
                pend.setdefault(cls, []).append('un literal con un marcador o puntos suspensivos: %s' % ascii(x))
            else:
                out.append(x)
        resid = re.sub(r'`[^`]*`', ' ', rest)
        words = [w for w in re.findall(r'[^\s,;.:]+', resid) if w not in STOP]
        if words:
            pend.setdefault(cls, []).append('sin resolver: %s' % ascii(' '.join(words)))
        add(out)

    for row in rows:
        cells = row.strip()[2:-2].split(' | ') if row.strip().endswith(' |') else []
        if len(cells) != 3:
            errors.append('una fila de %s sin tres celdas: %s' % (sub, ascii(row[:80])))
            continue
        c1, body = cells[1], cells[2]
        cm = re.match(r'`(\w+)` \((?:UI; )?CT-', c1)
        if cm:
            cls = cm.group(1)
            cts = cts_of(c1)
            if not cts:
                errors.append('la celda de la clase %s no da sus CT: %s' % (cls, ascii(c1)))
                continue
            kind[cls] = 'ct'
            filas.setdefault(cls, []).append(cts)
            if not body.startswith('**Líneas:** '):
                errors.append('la fila de %s no empieza por "Lineas:": %s' % (cls, ascii(body[:60])))
                continue
            parts = body[len('**Líneas:** '):].split(' **Valores:** ')
            if len(parts) > 2:
                errors.append('la fila de %s tiene dos partes de valores' % cls)
                continue
            take(cls, parts[0], parts[0], lambda o, c=cls: lines_of.setdefault(c, []).extend(o))
            if len(parts) == 2:
                take(cls, parts[1], parts[1], lambda o, c=cls: values_of.setdefault(c, []).extend(o))
                for v in values_of.get(cls, []):
                    if not (re.fullmatch(r'\d+(?:\.\d+)?', v) or re.fullmatch(r'[^\s`]+(?: [^\s`]+)*', v)):
                        pend.setdefault(cls, []).append('un valor que no se puede contar como token: %s' % ascii(v))
            continue
        em = re.match(r'`(\w+Full)`, ', c1)
        entero = body.startswith('**El texto entero') and re.search(r'no admiten? reparación \(RR-2\)', body)
        if em and entero:
            kind[em.group(1)] = 'entero'
            continue
        if re.match(r'Las \d+ copias mecánicas, las \d+ de la interfaz y el kit$', c1) and entero:
            entero_all = True
            continue
        errors.append('una fila de %s con otra forma: %s' % (sub, ascii(row[:100])))
    for cls in classes:
        if cls in kind:
            k = kind[cls]
        elif entero_all and (cls.endswith('Full') or cls.endswith('Kit')):
            k = 'entero'
        else:
            errors.append('la clase %s no tiene fila en la tabla de RR-3 de %s' % (cls, sub))
            continue
        info[cls] = {'tipo': k, 'lineas': list(dict.fromkeys(lines_of.get(cls, []))), 'valores': list(dict.fromkeys(values_of.get(cls, []))),
                     'pendiente': pend.get(cls, []) if k == 'ct' else [], 'filas': len(filas.get(cls, []))}
    return errors, info
# ---- rr3_table_check (fin)
