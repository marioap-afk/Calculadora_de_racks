"""kit/make_closure.py de la ronda 3: sustituye identity_checks() entera (de «def identity_checks():» a «def fmt_n(n):») por la de la ronda 3 y
retira readable_manifest() (la ronda 3 no tiene copias legibles de r4/: r4/ queda fuera del cierre). Uso: python -I -B patch_make_closure_ident_r3.py <kit>
"""
import os
import sys

p = os.path.join(sys.argv[1], "make_closure.py")
t = open(p, encoding="utf-8", newline="").read()
a, b = t.index("def identity_checks():\n"), t.index("def fmt_n(n):\n")
NEW = '''def identity_checks():
    """Ronda 3 (decisiones §96): la publicación 37ac6ca4 tiene por padre 5da2007f (§96 registrada), con la ronda 2 (8a528133); el objeto (ronda 3)
    es el mismo en la publicación, en el commit del resultado `run` y en el clon; I-62-A7/r3/ solo crece por su resultado `run`; I-62-A7/r2/ y la
    custodia de la revisión de la ronda 2 no cambian; decisiones crece solo por añadido y §96 es su última sección."""
    P = PARAMS
    pub, parent, grc = P["Publication"], P["PublicationParent"], P["GuardsRunCommit"]
    obj_blob = P["Object"]["Blob"]
    assert obj_blob == OBJECT_BLOB, "kit-params.json: Object.Blob distinto del fijado en owner_gate.py"
    assert (P.get("PreviousObject") or {}).get("Blob") == PREV_BLOB, "kit-params.json: PreviousObject distinto de la ronda 2 (8a528133)"
    for c in (REV, pub, parent, grc, PREV_COMMIT):
        assert git_ok("cat-file", "-e", c + "^{commit}"), "commit ausente en el clon: %s" % c
    assert git("rev-parse", pub + "^").decode().strip() == parent, "el padre de la publicación no es %s" % parent[:8]
    assert git("rev-parse", parent + ":" + OBJECT).decode().strip() == PREV_BLOB, "el padre de la publicación no tiene la ronda 2 (8a528133)"
    assert git("rev-parse", PREV_COMMIT + ":" + OBJECT).decode().strip() == PREV_BLOB, "la ronda 2 no es el blob de A-7 en e90b444d"
    for c in (pub, grc, REV):
        assert git("rev-parse", c + ":" + OBJECT).decode().strip() == obj_blob, "blob de A-7 (ronda 3) distinto en %s" % c
    for c in (parent, pub, grc, REV):
        assert git("rev-parse", c + ":" + A6).decode().strip() == OG.A6_AGREED_BLOB, "A-6 distinta de la acordada en %s" % c
    for a, b in ((PREV_COMMIT, parent), (parent, pub), (pub, grc), (grc, REV)):
        assert git_ok("merge-base", "--is-ancestor", a, b), "%s no es antecesor de %s" % (a, b)
    assert not git_ok("cat-file", "-e", parent + ":" + A7R3_DIR), "I-62-A7/r3/ ya existía en el padre de la publicación"
    pub_files, now_files = dir_files(pub, A7R3_DIR), dir_files(REV, A7R3_DIR)
    unchanged = all(git("rev-parse", pub + ":" + p).decode().strip() == git("rev-parse", REV + ":" + p).decode().strip() for p in pub_files)
    added = set(now_files) - set(pub_files)
    assert unchanged and added == {A7R3_RUN_RESULT} and set(P.get("A7EvidenceAddedAfterPublication") or []) == added, \\
        "I-62-A7/r3/ cambió después de la publicación además de añadir el resultado `run` (añadidos: %s)" % sorted(added)
    r2_parent, r2_now = dir_files(parent, A7R2_DIR), dir_files(REV, A7R2_DIR)
    assert r2_parent == r2_now and all(git("rev-parse", parent + ":" + p).decode().strip() == git("rev-parse", REV + ":" + p).decode().strip()
                                       for p in r2_now), "el preflight de la ronda 2 (I-62-A7/r2/) cambió desde el padre de la publicación"
    trees = {}
    for d, (t, custody) in PINNED_TREES.items():
        got, at_custody = git("rev-parse", REV + ":" + d).decode().strip(), git("rev-parse", custody + ":" + d).decode().strip()
        assert got == t == at_custody, "árbol %s distinto del fijado o del del commit que lo custodió (%s)" % (d, custody[:8])
        trees[d] = {"Tree": t, "CustodyCommit": custody}
    old = git("cat-file", "-p", DECISIONS_PREFIX_BLOB)
    dec_b = git("show", REV + ":" + DECISIONS)
    assert dec_b.startswith(old), "decisiones del clon no crece solo por añadido desde la registrada con §95 (blob 5e9bff98, commit 6b00b2bf)"
    for c in (parent, pub, grc, REV):
        assert git("rev-parse", c + ":" + DECISIONS).decode().strip() == DECISIONS_AT_PUBLICATION, "decisiones en %s distinta de la de §96" % c[:8]
    dec = dec_b.decode("utf-8").split("\\n")
    numbered = [(i, int(m.group(1))) for i, m in ((i, OG.NUMBERED_RX.match(l)) for i, l in enumerate(dec)) if m]
    s_last = [i for i, n in numbered if n == LAST_SECTION]
    assert BASE_SECTION == OG.BASE_SECTION and len(s_last) == 1 and max(n for _, n in numbered) == LAST_SECTION, \\
        "decisiones del clon: §96 no es la última sección numerada, una sola vez"
    s96_text = OG.section_text(dec, s_last[0])
    for tok in ("A62-A7R2-R1", "REQUIRED aceptado", "A-7 ronda 3", "`RebaseMap`", "sin lanzar una revisión formal nueva sin una disposición posterior"):
        assert tok in s96_text, "§96 no registra «%s»" % tok
    assert OG.authorization_pending(OG.pinned_authorization()), "owner_gate.py con una autorización fijada: este kit se entrega PENDIENTE"
    run_res = json.loads(git("show", REV + ":" + A7R3_RUN_RESULT).decode("utf-8"))
    assert run_res.get("Mode") == "run" and run_res.get("Base") == parent and run_res.get("Head") == pub and run_res.get("A7Blob") == obj_blob and \\
        run_res.get("Result") == "PASS" and all((g or {}).get("Status") == "PASS" for g in (run_res.get("Guards") or {}).values()) and \\
        sorted(run_res.get("Guards") or {}) == ["G%d" % k for k in range(1, 8)], \\
        "el resultado `run` custodiado de la ronda 3 no es Mode run / Base = padre / Head = publicación / A7Blob = objeto / PASS en G1-G7"
    st = json.loads(git("show", REV + ":" + A7R3_SELFTEST).decode("utf-8"))
    assert st.get("Result") == "PASS" and st.get("Killed") == st.get("MutantsTotal") and st.get("MutantsTotal") and st.get("Base") == parent and \\
        (st.get("Candidate") or {}).get("CandidateBlob") == obj_blob, "la autoprueba custodiada de las guardas de la ronda 3 no es PASS sobre el objeto"
    summ = json.loads(git("show", REV + ":" + A7R3_SUMMARY).decode("utf-8"))
    assert (summ.get("Candidate") or {}).get("Blob") == obj_blob and summ.get("Hold") == summ.get("Cases") and summ.get("Cases") and \\
        summ.get("NotHolding") == [] and summ.get("Base") == parent and summ.get("PreviousRound") == PREV_BLOB and \\
        (summ.get("Totals") or {}).get("q8", {}).get("Cases") == (summ.get("Totals") or {}).get("q8", {}).get("Hold") and \\
        (summ.get("Guards") or {}).get("Result") == "PASS" and (summ.get("Selftest") or {}).get("Result") == "PASS", \\
        "summary.json de la ronda 3 no es el de la candidata con todos los casos cumplidos (q8 incluido), guardas y autoprueba PASS"
    out2 = json.loads(git("show", REV + ":" + R2_OUTPUT).decode("utf-8"))
    aud2 = json.loads(git("show", REV + ":" + R2_AUDIT).decode("utf-8"))
    assert out2.get("RunId") == R2_RUN_ID and out2.get("ReviewedBlob") == PREV_BLOB and \\
        [f.get("FindingId") for f in out2.get("RequiredFindings") or []] == PRIOR_FINDINGS and \\
        [f.get("FindingId") for f in out2.get("OptionalFindings") or []] == PRIOR_OPTIONALS and out2.get("Verdict") == "CHANGES REQUIRED" and \\
        (aud2.get("Accreditation") or {}).get("Status") == "NOT_ACCREDITED", "la revisión de la ronda 2 custodiada no es la de A62-A7R2-R1 y O1-O4"
    a7 = git("show", REV + ":" + OBJECT).decode("utf-8")
    a6 = git("show", REV + ":" + A6).decode("utf-8").split("\\n")
    assert sum(1 for l in a6 if OG.line_sha(l) == OG.A6_LINE_SHA) == 1, "A-6 acordada sin la línea (a) fijada, una vez"
    assert sum(1 for l in dec if OG.line_sha(l) == OG.A6_LINE_SHA) == 1, "decisiones del clon sin la línea (a) del Owner (§78), una vez"
    assert sum(1 for l in dec if OG.line_sha(l) == OG.CLAUDE_LINE_SHA) == 1, "decisiones sin la línea CLAUDE-CLI-I62 = A fijada"
    header = a7.split("```")[1]
    header_claims = {}
    for p in (A1, A2, A3, A4, A5, A6, FREEZE, V14):
        b = git("rev-parse", REV + ":" + p).decode().strip()
        header_claims[p] = OG.token_in(header, b, "0-9a-f")
        assert header_claims[p], "la cabecera de A-7 no cita el blob %s de %s" % (b[:8], p)
    assert OG.token_in(header, PREV_BLOB, "0-9a-f"), "la cabecera de A-7 (Previous rounds) no cita el blob completo de la ronda 2"
    manifests = [sha256_manifest(REV, d) for d in (A7R3_DIR, A7R2_DIR, S91, S92, R2_REVIEW)]
    assert manifests[0]["NotListed"] == [A7R3_RUN_RESULT] and manifests[1]["NotListed"] == [A7R2_RUN_RESULT] and \\
        not manifests[2]["NotListed"] and not manifests[3]["NotListed"] and not manifests[4]["NotListed"], \\
        "MANIFEST.sha256 incompletos: %s" % [(m["Dir"], m["NotListed"]) for m in manifests]
    changed = [x for x in git("diff", "--name-status", parent, REV).decode("utf-8").split("\\n") if x]
    assert all(x.split("\\t")[-1] == OBJECT or x.split("\\t")[-1].startswith(A7R3_DIR + "/") or
               x.split("\\t")[-1] in ("docs/automation/evidence/I-62-evidence.md", "docs/automation/state/I-62.yml") for x in changed), \\
        "cambios desde el padre de la publicación fuera de A-7, I-62-A7/r3/, la evidencia y el estado: %s" % changed
    return {"CloneCommit": REV, "Publication": pub, "PublicationParent": parent, "GuardsRunCommit": grc,
            "Round2AtPublicationParent": PREV_BLOB, "Round2Commit": PREV_COMMIT, "ObjectBlobAtPublicationGuardsRunAndClone": obj_blob,
            "A6AgreedBlobAtParentPublicationGuardsRunAndClone": OG.A6_AGREED_BLOB,
            "R3EvidenceAbsentAtParent": True, "R3EvidenceUnchangedSincePublicationExceptRunResult": True,
            "A7EvidenceAddedAfterPublication": sorted(added), "R2EvidenceUnchangedSinceParent": True, "R2EvidenceFiles": len(r2_now),
            "A7R3EvidenceTree": git("rev-parse", REV + ":" + A7R3_DIR).decode().strip(), "PinnedTrees": trees,
            "DecisionsAppendOnlySinceSection95Record": True, "DecisionsAtParentPublicationGuardsRunAndClone": DECISIONS_AT_PUBLICATION,
            "Decisions96": {"Line": s_last[0] + 1, "Heading": dec[s_last[0]], "TextSha256": hashlib.sha256(s96_text.encode("utf-8")).hexdigest(),
                            "LastNumberedSection": True},
            "AuthorizationPending": True,
            "GuardsRunResult": {"Mode": "run", "Base": parent, "Head": pub, "A7Blob": obj_blob, "Result": "PASS",
                                "Guards": {k: "PASS" for k in sorted(run_res["Guards"])}},
            "GuardsSelfTest": {"Result": "PASS", "Base": parent, "CandidateBlob": obj_blob, "MutantsTotal": st.get("MutantsTotal"),
                               "Killed": st.get("Killed")},
            "PreflightSummary": {"CandidateBlob": obj_blob, "Hold": summ.get("Hold"), "Cases": summ.get("Cases"), "PreviousRound": PREV_BLOB,
                                 "Totals": summ.get("Totals"), "Invalidator": summ.get("Invalidator")},
            "Round2Review": {"RunId": R2_RUN_ID, "ReviewedBlob": PREV_BLOB, "RequiredFindings": PRIOR_FINDINGS, "OptionalFindings": PRIOR_OPTIONALS,
                             "Verdict": "CHANGES REQUIRED", "Accreditation": "NOT_ACCREDITED"},
            "OwnerLineOnceInA6AndInDecisions78": True, "ClaudeCliLinePinnedOnce": True, "A7HeaderCitesBlobs": header_claims,
            "A7HeaderCitesRound2Blob": True, "Sha256Manifests": manifests, "ReadableCopies": [], "ChangedSincePublicationParent": changed}


'''
t = t[:a] + NEW + t[b:]
ra, rb = t.index("def readable_manifest(rev, rdir):\n"), t.index("def sha256_manifest(rev, d):\n")
t = t[:ra] + t[rb:]
open(p, "w", encoding="utf-8", newline="").write(t)
print("ok")
