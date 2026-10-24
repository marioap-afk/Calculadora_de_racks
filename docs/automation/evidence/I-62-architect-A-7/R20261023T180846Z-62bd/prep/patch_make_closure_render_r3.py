"""kit/make_closure.py de la ronda 3: sustituye render_prompt() entera (de «def render_prompt(» a «def run_files_described(») por la de la ronda 3
(marcadores de kit/prompt.template.md de la ronda 3: §96, AUTOMATION_PLAN 16.7 L593-L595, guardas y resumen de la ronda 3, revisión de la ronda 2,
§3.7, §6 (A-5 y A-6) y el número de rutas del cierre). Uso: python -I -B patch_make_closure_render_r3.py <kit>
"""
import os
import sys

p = os.path.join(sys.argv[1], "make_closure.py")
t = open(p, encoding="utf-8", newline="").read()
a, b = t.index("def render_prompt(can, Ls, run, ident, lits, fc_entries):\n"), t.index("def run_files_described(run_dir, kit_dir):\n")
NEW = r'''def render_prompt(can, Ls, run, ident, lits, fc_entries):
    tpl = open(os.path.join(KIT, "prompt.template.md"), encoding="utf-8").read()
    by = {r["Path"]: r for r in can}
    flagged = {e["Original"]: e for e in fc_entries}
    faithful = "\n".join("- `%s` (%s) → `%s\\%s` (%s líneas%s)" % (
        e["Original"], marks_list(e["Flags"]), RUN, e["Copy"], fmt_n(e["CopyLines"]),
        ("; las continuaciones de %s están en el apéndice, líneas %d-%d de la copia" % (
            ("la línea %d" % e["ContinuedOriginalLines"][0]) if len(e["ContinuedOriginalLines"]) == 1 else
            ("las líneas %s" % ", ".join(str(x) for x in e["ContinuedOriginalLines"][:12]) +
             ((" y %d más" % (len(e["ContinuedOriginalLines"]) - 12)) if len(e["ContinuedOriginalLines"]) > 12 else "")),
            e["AppendixLines"][0], e["AppendixLines"][1])) if e["AppendixLines"] else "") for e in fc_entries)
    g = {p: {x["Section"]: x for x in by[p]["LineRanges"]} for p in by}
    dec, a7, ga, a6, a5 = g[DECISIONS], by[OBJECT], g[OBJECT], g[A6], g[A5]
    a7_map = "; ".join("%s %s" % ((x["Section"] if x["Section"] in ("cabecera",) else
                                   re.sub(r"^#+ ", "", x["Section"]).split(" — ")[0].split(" (")[0]), range_text(x))
                       for x in a7["LineRanges"] if x["Section"] != "entero" and (x["Section"] == "cabecera" or x["Section"].startswith("#")))
    large = [r["Path"] for r in can if r["Bytes"] > LARGE and r["Path"] not in flagged] + \
            [n for n in PREMISE_RUN_FILES if run[n]["Bytes"] > LARGE] + [e["Copy"] for e in fc_entries if e["CopyBytes"] > LARGE]
    grouped_prefixes = (A7R3_DIR + "/", A7R2_DIR + "/", M7 + "/", KITM7 + "/", PKG_DIR + "/", R2_REVIEW + "/")
    entries = [prompt_entry(k + 1, r) for k, r in enumerate([r for r in can if r["Path"] != OBJECT and not r["Path"].startswith(grouped_prefixes)])]
    sec = lambda p: range_text([x for x in a7["LineRanges"] if x["Section"].startswith("### " + p + " ") or x["Section"].startswith("## " + p + " ")][0])
    d = lambda n: [x for k, x in dec.items() if k.startswith("## %d. " % n)][0]
    pts = order_points(run["order.txt"]["_Lines"])
    grr3, sm3, r2o, r2a = g[A7R3_RUN_RESULT], g[A7R3_SUMMARY], g[R2_OUTPUT], g[R2_AUDIT]
    apd = DIRECT_READ
    rule = lambda dl, n: range_text(a6["%s, regla %d" % (dl, n)])
    r2m = lambda k: range_text(r2o["%s (revisión de la ronda 2)" % k])
    rep = {
        "{{RUN_ID}}": IDS["RunId"], "{{INVOCATION_ID}}": IDS["InvocationId"], "{{LOGICAL_ID}}": IDS["LogicalReviewRequestId"],
        "{{CLONE}}": CLONE, "{{RUN}}": RUN,
        "{{CLONE_COMMIT}}": REV, "{{PUBLICATION}}": PARAMS["Publication"], "{{PUB_PARENT}}": PARAMS["PublicationParent"],
        "{{OBJECT_BLOB}}": a7["Blob"], "{{OBJECT_BLOB8}}": a7["Blob"][:8],
        "{{OBJECT_BYTES}}": fmt_n(a7["Bytes"]), "{{OBJECT_LINES}}": fmt_n(a7["Lines"]),
        "{{A7R2_BYTES}}": fmt_n(run["a7-r2.md"]["Bytes"]), "{{A7R2_LINES}}": fmt_n(run["a7-r2.md"]["Lines"]),
        "{{ORDER_LINES}}": str(run["order.txt"]["Lines"]),
        "{{ORDER_P1}}": range_text(pts[1]), "{{ORDER_P2}}": range_text(pts[2]), "{{ORDER_P3}}": range_text(pts[3]), "{{ORDER_P4}}": range_text(pts[4]),
        "{{ORDER_P5}}": range_text(pts[5]), "{{ORDER_P6}}": range_text(pts[6]), "{{ORDER_P7}}": range_text(pts[7]), "{{ORDER_P8}}": range_text(pts[8]),
        "{{ORDER_P9}}": range_text(pts[9]),
        "{{A7_MAP}}": a7_map,
        "{{A7_RULE}}": range_text(ga["§3.2, A7-1 (texto de la regla)"]),
        "{{A7_POINTS}}": "; ".join("%d L%s" % (k, range_text(ga["A7-1, punto %d" % k])) for k in range(1, 9)),
        "{{A7_CATS}}": "; ".join("(%s) L%s" % (c, range_text(ga["A7-1, punto 1 (%s)" % c])) for c in "abcd"),
        "{{A7_S33_T}}": range_text(ga["§3.3, tabla de categorías"]), "{{A7_S34}}": sec("3.4"), "{{A7_S35}}": sec("3.5"),
        "{{A7_S35_RES}}": range_text(ga["§3.5, residuo declarado (clausemap)"]), "{{A7_S36}}": sec("3.6"), "{{A7_S37}}": sec("3.7"),
        "{{A7_S37_SCOPE}}": range_text(ga["§3.7, alcance de la corrección de la ronda 3"]),
        "{{A7_S37_ROW}}": range_text(ga["§3.7, fila A62-A7R2-R1"]), "{{A7_S4_REBASE}}": range_text(ga["§4, fila del registro de rebase"]),
        "{{A7_S5}}": sec("5."), "{{A7_S6}}": sec("6."), "{{A7_S6_A5}}": range_text(ga["§6, A-5"]), "{{A7_S6_A6}}": range_text(ga["§6, A-6"]),
        "{{A7_S7}}": sec("7."), "{{A7_S8}}": sec("8."),
        "{{A7_QR3}}": "%s-%s" % (ga["§8, Q-A7R3-01"]["Start"], ga["§8, Q-A7R3-03"]["End"]),
        "{{DEC96}}": range_text(d(96)),
        "{{DEC96_SOURCE}}": range_text(dec["§96, bytes y SHA-256 del cuerpo pegado"]),
        "{{DEC96_P1}}": range_text(dec["§96, punto 1"]), "{{DEC96_P2}}": range_text(dec["§96, punto 2"]), "{{DEC96_P3}}": range_text(dec["§96, punto 3"]),
        "{{DEC96_P6}}": range_text(dec["§96, punto 6"]), "{{DEC96_P7}}": range_text(dec["§96, punto 7"]), "{{DEC96_P9}}": range_text(dec["§96, punto 9"]),
        "{{DEC78_LINE}}": range_text(dec["§78, línea del Owner A6-ADR0046-9-FX02"]),
        "{{GRR3}}": A7R3_RUN_RESULT.replace("/", "\\"), "{{GRR3_LINES}}": str(by[A7R3_RUN_RESULT]["Lines"]),
        "{{GRR3_KEYS}}": "; ".join("%s L%s" % (k, range_text(grr3["%s (resultado `run`)" % k])) for k in ("A7Blob", "Base", "Head", "Mode", "Result")),
        "{{SUMMARY3}}": A7R3_SUMMARY.replace("/", "\\"), "{{SUMMARY3_CAND}}": range_text(sm3["Candidate (blob de la candidata)"]),
        "{{SUMMARY3_HOLD}}": range_text(sm3["Hold, Cases y NotHolding"]), "{{SUMMARY3_ST}}": range_text(sm3["Selftest (autoprueba de las guardas)"]),
        "{{R2OUT}}": R2_OUTPUT.replace("/", "\\"),
        "{{R2OUT_KEYS}}": "; ".join("%s L%s" % (k, r2m(k)) for k in ("RunId", "ReviewedBlob", "RequiredFindings")),
        "{{R2OUT_RANGES}}": "; ".join("%s L%s" % (k, r2m(k)) for k in R2_OUTPUT_MEMBERS),
        "{{R2OUT_REQ}}": "`RequiredFindings`, L%s" % r2m("RequiredFindings"),
        "{{R2OUT_REG}}": "`Round1FindingResolution`, L%s, y `RegressionCheck`, L%s" % (r2m("Round1FindingResolution"), r2m("RegressionCheck")),
        "{{R2AUDIT_RANGES}}": "`Accreditation` L%s; `ReferenceCheck` L%s; `Fidelity` L%s" % (
            range_text(r2a["Accreditation (auditoría de la ronda 2)"]), range_text(r2a["ReferenceCheck (auditoría de la ronda 2)"]),
            range_text(r2a["Fidelity (auditoría de la ronda 2)"])),
        "{{A5_1}}": range_text(a5["§3.2, párrafo A5-1 (L175-L199)"]), "{{A5_2R4}}": range_text(a5["§3.3, A5-2, regla 4 (L216)"]),
        "{{A6_LINE_A}}": range_text(a6["§7, línea del Owner (a) A6-ADR0046-9-FX02"]),
        "{{A6_LINE_B}}": range_text(a6["§7, línea del Owner (b) A6-CONTROLLER-MEDICION-CONSUMO"]),
        "{{A6_A62_R1}}": rule("A6-2", 1), "{{A6_A62_R3}}": rule("A6-2", 3), "{{A6_A63_R6B}}": range_text(a6["A6-3, regla 6 (b)"]),
        "{{A6_A65_R2}}": rule("A6-5", 2), "{{A6_A67_R1}}": rule("A6-7", 1), "{{A6_A67_R5}}": rule("A6-7", 5), "{{A6_A67_R7}}": rule("A6-7", 7),
        "{{A6_A69_R4}}": rule("A6-9", 4),
        "{{A6_DERIV}}": range_text(a6["§3.12, tabla de derivación del cierre del aceptante (A6-9, regla 4)"]),
        "{{A7R3EVID_TABLE}}": table([by[p] for p in sorted(by) if p.startswith(A7R3_DIR + "/")]),
        "{{A7R3EVID_COUNT}}": str(sum(1 for p in by if p.startswith(A7R3_DIR + "/"))),
        "{{A7R2EVID_TABLE}}": table([by[p] for p in sorted(by) if p.startswith(A7R2_DIR + "/")]),
        "{{A7R2EVID_COUNT}}": str(sum(1 for p in by if p.startswith(A7R2_DIR + "/"))),
        "{{R2REV_TABLE}}": table([by[p] for p in sorted(by) if p.startswith(R2_REVIEW + "/")]),
        "{{M7_TABLE}}": table([by[p] for p in sorted(by) if p.startswith(M7 + "/")]),
        "{{M7_COUNT}}": str(sum(1 for p in by if p.startswith(M7 + "/"))),
        "{{KITM7_TABLE}}": table([by[p] for p in sorted(by) if p.startswith(KITM7 + "/")]),
        "{{KITM7_COUNT}}": str(sum(1 for p in by if p.startswith(KITM7 + "/"))),
        "{{PKG_TABLE}}": table([by[p] for p in sorted(by) if p.startswith(PKG_DIR + "/")]),
        "{{PKG_COUNT}}": str(sum(1 for p in by if p.startswith(PKG_DIR + "/"))),
        "{{ACCEPTOR}}": ACCEPTOR, "{{ACCEPTOR_LINES}}": fmt_n(by[ACCEPTOR]["Lines"]), "{{TOOL}}": TOOL, "{{TOOL_LINES}}": fmt_n(by[TOOL]["Lines"]),
        "{{AP_DIRECT}}": "%d-%d" % (apd["Start"], apd["End"]),
        "{{AP_DIRECT_OFFSET}}": str(apd["Start"] - 8), "{{AP_DIRECT_LIMIT}}": str(apd["End"] - apd["Start"] + 17),
        "{{LIT_CHECKLIST}}": lit_checklist(lits, by),
        "{{EVIDENCE_RANGES}}": ", ".join("§%s" % re.sub(r"^## (\d+)\..*$", r"\1", x["Section"]) for x in by[EVIDENCE]["LineRanges"]
                                         if x["Section"].startswith("## ")),
        "{{CLOSURE_LIST}}": "\n".join(entries),
        "{{CLOSURE_COUNT}}": str(len(can)),
        "{{LARGE_FILES}}": ", ".join("`%s`" % p for p in large),
        "{{FAITHFUL_COPIES}}": faithful if faithful else "(ninguna: ningún archivo del cierre está marcado)",
        "{{FAITHFUL_COUNT}}": str(len(fc_entries)),
        "{{READ_AP}}": read_path(AP, flagged), "{{READ_DEC}}": read_path(DECISIONS, flagged), "{{READ_A7}}": read_path(OBJECT, flagged),
        "{{READ_GRR3}}": read_path(A7R3_RUN_RESULT, flagged), "{{READ_SUMMARY3}}": read_path(A7R3_SUMMARY, flagged),
        "{{READ_R2OUT}}": read_path(R2_OUTPUT, flagged),
    }
    for k, v in rep.items():
        assert k in tpl, "marcador ausente en la plantilla: %s" % k
        tpl = tpl.replace(k, v)
    assert not re.search("[{][{][A-Z0-9_]+[}][}]", tpl), "marcadores sin sustituir en el prompt: %s" % re.findall("[{][{][A-Z0-9_]+[}][}]", tpl)[:5]
    shown = set(re.findall(r"docs/[A-Za-z0-9_./@+-]+", tpl))
    missing = [r["Path"] for r in can if r["Path"] not in shown]
    assert not missing, "rutas del cierre que el prompt no muestra literalmente (índice de rutas, decisiones §96, punto 6): %s" % missing[:5]
    return tpl.rstrip("\n")


'''
t = t[:a] + NEW + t[b:]
t = t.replace('    """Líneas de los puntos 1-9 de order.txt (el cuerpo pegado de §93: «<n>. <título>» al principio de línea)."""',
              '    """Líneas de los puntos 1-9 de order.txt (el cuerpo pegado de §96: «<n>. <título>» al principio de línea)."""', 1)
open(p, "w", encoding="utf-8", newline="").write(t)
print("ok")
