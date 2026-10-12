# build_repos.py - builds every input of the R3 evidence (run: python -I build_repos.py).
#  1. pinned tool, acceptor and mutate.py from their blobs; the 27 mutants (separate files) from mutate.py's definitions;
#  2. REAL: a fresh clone of the fixture origin (read-only source; git clone --no-hardlinks);
#  3. DELIV: a second fresh clone plus a simulated T1 delivery (RED + GREEN on fx/u1, origin ref moved locally);
#  4. SYN: a synthetic "mini" repository with hand-derivable clause-map scenarios (valid map, maps with known
#     defects, and the histories that activate the clause-map mutants).
# Everything is written under R3_WORK (default D:/r62-a6r2/r3). Deterministic: fixed identities and dates.
import importlib.util
import json
import os
import shutil
import stat
import sys

_spec = importlib.util.spec_from_file_location("r3lib", os.path.join(os.path.dirname(os.path.abspath(__file__)), "r3lib.py"))
L = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(L)


def rmtree(p):
    def onexc(f, q, e):
        os.chmod(q, stat.S_IWRITE)
        f(q)
    if os.path.isdir(p):
        shutil.rmtree(p, onexc=onexc)


def clone(dst):
    rmtree(dst)
    L.git(L.WORK, "clone", "-q", "--no-hardlinks", "-c", "core.autocrlf=false", "-b", L.BR, L.ORIGIN, dst)
    if L.gits(dst, "rev-parse", "HEAD") != L.TIP:
        raise RuntimeError("unexpected fixture tip")


# ------------------------------------------------------------------ DELIV: simulated delivery of T1

WORKER_TRAILER = "Co-Authored-By: R3 Synthetic Worker (FX-U1 T1)"
DELIV_FILES = ["docs/automation/evidence/FX-U1-agent/T1/worker-notes.md", "src/Fixture.Lib/Calculator.cs",
               "tests/Fixture.Tests/CalculatorTests.cs"]


def build_deliv():
    clone(L.DELIV)
    d = L.DELIV
    calc = L.git(d, "show", L.TIP + ":src/Fixture.Lib/Calculator.cs").decode("utf-8")
    tests = L.git(d, "show", L.TIP + ":tests/Fixture.Tests/CalculatorTests.cs").decode("utf-8")
    tests_red = tests.replace(
        "    public void AddSumsTwoIntegers() => Assert.Equal(5, Calculator.Add(2, 3));\n",
        "    public void AddSumsTwoIntegers() => Assert.Equal(5, Calculator.Add(2, 3));\n\n"
        "    [Fact]\n    public void SubtractRestaDosEnteros() => Assert.Equal(-1, Calculator.Subtract(2, 3));\n")
    calc_green = calc.replace("    public static int Add(int a, int b) => a + b;\n",
                              "    public static int Add(int a, int b) => a + b;\n\n"
                              "    public static int Subtract(int a, int b) => a - b;\n")
    notes = ("# T1 \u2014 notas del Worker sint\u00e9tico (R3)\n\n"
             "Entrega simulada para la evidencia de independencia; no es una ejecuci\u00f3n real de T1.\n")
    assert tests_red != tests and calc_green != calc
    t_tip = L.gits(d, "rev-parse", L.TIP + "^{tree}")
    t_red = L.make_tree(d, {"tests/Fixture.Tests/CalculatorTests.cs": tests_red.encode("utf-8")}, base=t_tip)
    red = L.make_commit(d, t_red, [L.TIP], "FX-U1 T1: RED de Subtract (prueba que falla; entrega simulada R3)\n\n"
                        + WORKER_TRAILER + "\n", 1)
    t_green = L.make_tree(d, {"src/Fixture.Lib/Calculator.cs": calc_green.encode("utf-8"),
                              DELIV_FILES[0]: notes.encode("utf-8")}, base=t_red)
    green = L.make_commit(d, t_green, [red], "FX-U1 T1: GREEN de Subtract (entrega simulada R3)\n\n"
                          + WORKER_TRAILER + "\n", 2)
    L.git(d, "update-ref", "refs/heads/" + L.BR, green)
    L.git(d, "update-ref", "refs/remotes/origin/" + L.BR, green)
    L.git(d, "reset", "-q", "--hard", green)
    if L.git(d, "status", "--porcelain") != b"":
        raise RuntimeError("deliv worktree not clean")
    return {"RedSha": red, "GreenSha": green, "BaseSha": L.TIP, "Trailer": WORKER_TRAILER, "Files": DELIV_FILES}


# ------------------------------------------------------------------ SYN: mini repository

OACUTE = "\u00f3"
POINTER = "Antes de aplicar esta secci" + OACUTE + "n, toda unidad aplica \u00a716.13."
ENTRY_H = "## 12. Coexistencia de protocolos de ejecuci" + OACUTE + "n delegada (I61/I62)"
R163_H = "### 16.13 Compatibilidad de protocolos de ejecuci" + OACUTE + "n delegada"
S16_H = "## 16. Ejecuci" + OACUTE + "n delegada"
S163_H = "### 16.3 Lectura de autoridades"
S1610_H = "### 16.10 T\u00e9rminos de gate"
S1_H = "## 1. Alcance"
S17_H = "## 17. Cierre"
PRE_AP = "Pre\u00e1mbulo del plan sint\u00e9tico R3: texto anterior al primer cap\u00edtulo."
S16_BODY = "Primera frase del cap\u00edtulo 16. Segunda frase con la versi" + OACUTE + "n 16.13 citada."
S163_BODY = ("Las autoridades se leen en la revisi" + OACUTE + "n fijada. La lectura cubre la versi" + OACUTE
             + "n 16.13 y otras. Fin de la secci" + OACUTE + "n.")
TERMS_P = ("Coincidencia por palabra o frase completa en `WorkCompleted`, `Evidence`, `UnexpectedFindings`, "
           "`KnownLimitations`, `Deviations`, `OpenQuestions` y `RecommendedNextAction`: `GATE PASS`, `Candidate` y "
           "`rama integrada`. No se aplica al nombre de propiedad `Gate`.")
SURF = ["docs/AUTOMATION_PLAN.md", "docs/WORKFLOW.md", "docs/automation/agent-execution/"]
CLOSED_P = ("**Superficies** (lista cerrada): `docs/AUTOMATION_PLAN.md`, `docs/WORKFLOW.md` y "
            "`docs/automation/agent-execution/`.")


def ap_text(eff, preamble=PRE_AP, s163_extra="", fence=False):
    parts = [preamble, "", S1_H, "", "Este plan sint\u00e9tico solo sirve para las pruebas de independencia. No es normativo.",
             "", S16_H, "", (POINTER + " " if eff else "") + S16_BODY, "", S163_H, "",
             S163_BODY + (" " + POINTER if eff else "") + s163_extra, "", S1610_H, "", TERMS_P, ""]
    if eff:
        parts += [R163_H, "", CLOSED_P, "", "El mapa de cl\u00e1usulas se lee en el punto efectivo.", ""]
    parts += [S17_H, "", "Texto de cierre del plan."]
    if fence:
        parts += ["", "```text", "## Bloque de ejemplo", "```"]
    return "\n".join(parts) + "\n"


def wf_text(eff):
    parts = ["Pre\u00e1mbulo del flujo sint\u00e9tico R3.", "", "## 1. Ramas", "", "Toda unidad trabaja en su rama."]
    if eff:
        parts += ["", ENTRY_H, "", "Antes de cualquier unidad se aplica " + R163_H + " de AUTOMATION_PLAN."]
    return "\n".join(parts) + "\n"


def mini_base():
    hs = L.schema_bytes("worker-handoff.schema.json")
    return {
        ".gitignore": b"artifacts/orchestration/\n",
        "README.md": "Repositorio sint\u00e9tico R3 (evidencia de independencia; no es el fixture).\n".encode("utf-8"),
        "src/app.txt": b"aplicacion sintetica\n",
        L.AP_PATH: ap_text(False).encode("utf-8"),
        L.WF_PATH: wf_text(False).encode("utf-8"),
        "docs/automation/agent-execution/README.md": "Procedimientos sint\u00e9ticos.\n".encode("utf-8"),
        L.HANDOFF_SCHEMA_PATH: hs,
    }


def real_map_schema():
    return L.git(L.REAL, "show", L.MAIN + ":" + L.MAP_SCHEMA_PATH)


def entries_v0():
    return [{"Path": L.AP_PATH, "Section": S16_H, "Level": 2, "Kind": "MODIFIED"},
            {"Path": L.AP_PATH, "Section": S163_H, "Level": 3, "Kind": "MODIFIED"},
            {"Path": L.AP_PATH, "Section": R163_H, "Level": 3, "Kind": "ENTRY"},
            {"Path": L.WF_PATH, "Section": ENTRY_H, "Level": 2, "Kind": "ENTRY"}]


def map_doc(base_files, eff_files, files_spec, entries, surfaces=SURF):
    """files_spec: list of (path, kind) or (path, kind, base_override, eff_override)."""
    files = []
    for spec in files_spec:
        p, kind = spec[0], spec[1]
        bb = L.blob_id(base_files[p]) if (kind == "MODIFIED" and p in base_files) else None
        eb = L.blob_id(eff_files[p]) if p in eff_files else None
        if len(spec) > 2 and spec[2] != "keep":
            bb = spec[2]
        if len(spec) > 3 and spec[3] != "keep":
            eb = spec[3]
        files.append({"Path": p, "FileKind": kind, "BaseBlob": bb, "EffBlob": eb})
    return {"Schema": "rackcad-clause-map/v1", "Protocol": "rackcad-protocol/I62",
            "LegacyProtocol": "rackcad-protocol/I61", "Surfaces": list(surfaces), "Files": files, "Entries": entries}


def map_bytes(doc):
    return (json.dumps(doc, ensure_ascii=False, indent=1) + "\n").encode("utf-8")


def v0_parts():
    base = mini_base()
    eff = dict(base)
    eff[L.AP_PATH] = ap_text(True).encode("utf-8")
    eff[L.WF_PATH] = wf_text(True).encode("utf-8")
    eff[L.MAP_SCHEMA_PATH] = real_map_schema()
    files = [(L.AP_PATH, "MODIFIED"), (L.WF_PATH, "MODIFIED"), (L.MAP_SCHEMA_PATH, "ENTRY")]
    return base, eff, files, entries_v0()


TRAILER_MSG = "R3 activaci\u00f3n sint\u00e9tica de I62 (%s)\n\n" + L.TRAILER + "\nCo-Authored-By: R3 Synthetic Worker\n"
MERGE_MSG = "Merge r3/%s: activaci\u00f3n de prueba (%s)\n\nCo-Authored-By: R3 Synthetic Worker\n"


class Syn(object):
    def __init__(self):
        self.repo = L.SYN
        self.seq = 10
        self.scen = {}

    def nseq(self):
        self.seq += 1
        return self.seq

    def tree(self, files):
        return L.make_tree(self.repo, files)

    def commit(self, tree, parents, msg):
        return L.make_commit(self.repo, tree, parents, msg, self.nseq())

    def simple(self, name, base, eff, removes=(), mapdoc=None, raw_map=None):
        """B (root) -> T (trailer) ; M = merge(B, T) with T's tree. Returns ids."""
        effd = dict(eff)
        for r in removes:
            effd.pop(r, None)
        effd[L.MAP_PATH] = raw_map if raw_map is not None else map_bytes(mapdoc)
        tb = self.tree(base)
        te = self.tree(effd)
        b = self.commit(tb, [], "R3 base sint\u00e9tica (%s)\n" % name)
        t = self.commit(te, [b], TRAILER_MSG % name)
        m = self.commit(te, [b, t], MERGE_MSG % (name, name))
        L.git(self.repo, "update-ref", "refs/heads/r3/" + name, m)
        rec = {"Base": b, "Trailer": t, "Eff": m, "Main": m, "EffParent1": b, "EffParent2": t, "MapDoc": mapdoc,
               "BaseFiles": sorted(base), "EffFiles": sorted(effd)}
        self.scen[name] = rec
        return rec, effd


def build_syn():
    rmtree(L.SYN)
    os.makedirs(L.SYN)
    L.git(L.SYN, "init", "-q", "-b", "r3/head")
    L.git(L.SYN, "config", "core.autocrlf", "false")
    S = Syn()
    blobs = {}

    # ---- V0: valid map (no defect) -----------------------------------------------------------
    base, eff, files, ents = v0_parts()
    S.simple("V0", base, eff, mapdoc=map_doc(base, eff, files, ents))

    # ---- part (b): maps with known defects (one variable at a time) -------------------------
    b, e, f, en = v0_parts()
    S.simple("D1-literal", b, e, mapdoc=map_doc(b, e, [(L.AP_PATH, "MODIFIED", L.blob_id(b[L.WF_PATH]))] + f[1:], en))

    b, e, f, en = v0_parts()
    note = "docs/automation/agent-execution/note.md"
    b[note] = b"nota v1\n"
    e[note] = b"nota v2\n"
    S.simple("D2-added-in-eff1", b, e, mapdoc=map_doc(b, e, f + [(note, "ADDED")], en))

    b, e, f, en = v0_parts()
    extra = "docs/automation/agent-execution/extra.json"
    e[extra] = b"{}\n"
    S.simple("D3-entry-outside-schema", b, e, mapdoc=map_doc(b, e, f + [(extra, "ENTRY")], en))

    b, e, f, en = v0_parts()
    b[L.MAP_SCHEMA_PATH] = real_map_schema().replace(b'"title": "rackcad-clause-map/v1"', b'"title": "rackcad-clause-map/v0"')
    assert b[L.MAP_SCHEMA_PATH] != e[L.MAP_SCHEMA_PATH]
    S.simple("D4-entry-in-eff1", b, e, mapdoc=map_doc(b, e, f, en))

    b, e, f, en = v0_parts()
    unl, stab = "docs/automation/agent-execution/unlisted.md", "docs/automation/agent-execution/stable.md"
    e[unl] = b"nuevo sin listar\n"
    b[stab] = e[stab] = b"estable\n"
    S.simple("D5-only-in", b, e, mapdoc=map_doc(b, e, f + [(stab, "MODIFIED")], en))

    b, e, f, en = v0_parts()
    old = "docs/automation/agent-execution/old.md"
    b[old] = e[old] = b"archivo borrado en EFF\n"
    S.simple("D6-deleted", b, e, removes=[old], mapdoc=map_doc(b, e, f, en))

    b, e, f, en = v0_parts()
    en7 = [x for x in en if x["Section"] != S163_H] + [{"Path": L.AP_PATH, "Section": S1_H, "Level": 2, "Kind": "REMOVED"}]
    S.simple("D7-mv6", b, e, mapdoc=map_doc(b, e, f, en7))

    b, e, f, en = v0_parts()
    e[L.AP_PATH] = ap_text(True, s163_extra=" Frase a\u00f1adida despu\u00e9s del puntero.").encode("utf-8")
    S.simple("D8-mv7", b, e, mapdoc=map_doc(b, e, f, en))

    b, e, f, en = v0_parts()
    S.simple("D9-mv5-dup", b, e, mapdoc=map_doc(b, e, f, en + [dict(en[0])]))

    b, e, f, en = v0_parts()
    S.simple("D10-mv2", b, e, mapdoc=map_doc(b, e, f, en, surfaces=[SURF[1], SURF[0], SURF[2]]))

    b, e, f, en = v0_parts()
    md = map_doc(b, e, f, en)
    md["Files"][2]["FileKind"] = "RENAMED"
    S.simple("D11-mv1-schema", b, e, mapdoc=md)

    # ---- part (a): histories that activate the clause-map mutants ---------------------------
    # SA: two first-parent merges (M08, M21 fail-closed), deletion under Surfaces (M18), preamble change (M22),
    #     heading-like line inside a code fence (M24), and a state with two distinct claim_id values (M19).
    b, e, f, en = v0_parts()
    b[old] = b"archivo borrado en EFF\n"
    e[L.AP_PATH] = ap_text(True, preamble=PRE_AP.replace("texto anterior", "texto anterior modificado"),
                           fence=True).encode("utf-8")
    ena = en + [{"Path": L.AP_PATH, "Section": "pre\u00e1mbulo", "Level": 0, "Kind": "MODIFIED"},
                {"Path": L.AP_PATH, "Section": S17_H, "Level": 2, "Kind": "MODIFIED"}]
    rec, effd = S.simple("SA", b, e, removes=[old], mapdoc=map_doc(b, e, f, ena))
    state = "docs/automation/state/R3.yml"
    c_files = dict(effd)
    c_files[state] = b"unit: R3\nclaim:\n  claim_id: fc62f1c7-0000-4000-8000-0000000000a1\nlegacy:\n  claim_id: fc62f1c7-0000-4000-8000-0000000000a2\n"
    c = S.commit(S.tree(c_files), [rec["Eff"]], "R3 SA: estado con dos claim_id distintos\n")
    d_files = dict(effd)
    d_files["src/extra.txt"] = b"rama lateral posterior a EFF\n"
    dd = S.commit(S.tree(d_files), [rec["Eff"]], "R3 SA: rama lateral posterior a EFF\n")
    both = dict(c_files)
    both["src/extra.txt"] = d_files["src/extra.txt"]
    m2 = S.commit(S.tree(both), [c, dd], "Merge r3/SA-late: segundo merge first-parent\n")
    L.git(L.SYN, "update-ref", "refs/heads/r3/SA", m2)
    rec.update({"Main": m2, "StateRev": c, "StatePath": state, "LateMerge": m2, "LateP1": c, "LateP2": dd})

    # SB: trailer on the first-parent line; the only first-parent merge has both parents reaching it (M21 false EFF).
    b, e, f, en = v0_parts()
    e[L.MAP_PATH] = map_bytes(map_doc(b, e, f, en))
    bb = S.commit(S.tree(b), [], "R3 base sint\u00e9tica (SB)\n")
    tt = S.commit(S.tree(e), [bb], TRAILER_MSG % "SB")
    e2 = dict(e)
    e2["src/c.txt"] = b"c\n"
    cc = S.commit(S.tree(e2), [tt], "R3 SB: commit en la linea first-parent\n")
    e3 = dict(e)
    e3["src/d.txt"] = b"d\n"
    d3 = S.commit(S.tree(e3), [tt], "R3 SB: rama lateral\n")
    e4 = dict(e2)
    e4["src/d.txt"] = b"d\n"
    mb = S.commit(S.tree(e4), [cc, d3], "Merge r3/SB-side\n")
    L.git(L.SYN, "update-ref", "refs/heads/r3/SB", mb)
    S.scen["SB"] = {"Base": bb, "Trailer": tt, "Main": mb, "Merge": mb, "MergeP1": cc, "MergeP2": d3}

    # SC: schema at EFF with a keyword outside the supported subset (M17).
    b, e, f, en = v0_parts()
    sch = json.loads(real_map_schema().decode("utf-8"))
    sch["properties"]["Files"]["items"]["properties"]["Path"]["format"] = "uri-reference"
    e[L.MAP_SCHEMA_PATH] = (json.dumps(sch, ensure_ascii=False, indent=2) + "\n").encode("utf-8")
    S.simple("SC", b, e, mapdoc=map_doc(b, e, f, en))

    # SD: clause map with a duplicated key (same value) (M25).
    b, e, f, en = v0_parts()
    raw = map_bytes(map_doc(b, e, f, en)).decode("utf-8")
    raw = raw.replace('"Protocol": "rackcad-protocol/I62",', '"Protocol": "rackcad-protocol/I62",\n "Protocol": "rackcad-protocol/I62",', 1)
    assert raw.count('"Protocol"') == 2
    S.simple("SD", b, e, raw_map=raw.encode("utf-8"), mapdoc=None)

    # SE: no exact trailer line; a merged side commit quotes the trailer inside a longer line (M09).
    b, e, f, en = v0_parts()
    e[L.MAP_PATH] = map_bytes(map_doc(b, e, f, en))
    be = S.commit(S.tree(b), [], "R3 base sint\u00e9tica (SE)\n")
    xe = S.commit(S.tree(e), [be], "R3 SE: documenta el trailer sin activarlo\n\nCita literal: " + L.TRAILER
                  + " (no es una l\u00ednea de trailer)\n")
    me = S.commit(S.tree(e), [be, xe], "Merge r3/SE-side\n")
    L.git(L.SYN, "update-ref", "refs/heads/r3/SE", me)
    S.scen["SE"] = {"Base": be, "Side": xe, "Main": me}

    # SF: two exact trailer commits, each merged by its own first-parent merge (M16).
    b, e, f, en = v0_parts()
    e[L.MAP_PATH] = map_bytes(map_doc(b, e, f, en))
    bf = S.commit(S.tree(b), [], "R3 base sint\u00e9tica (SF)\n")
    t1 = S.commit(S.tree(e), [bf], TRAILER_MSG % "SF-1")
    m1 = S.commit(S.tree(e), [bf, t1], MERGE_MSG % ("SF-1", "SF-1"))
    t2 = S.commit(S.tree(e), [bf], TRAILER_MSG % "SF-2")
    m2f = S.commit(S.tree(e), [m1, t2], MERGE_MSG % ("SF-2", "SF-2"))
    L.git(L.SYN, "update-ref", "refs/heads/r3/SF", m2f)
    S.scen["SF"] = {"Base": bf, "Trailer1": t1, "Merge1": m1, "Trailer2": t2, "Main": m2f}

    # HEAD of the synthetic repository: V0 (checked out, clean), with the origin refs of a pushed branch.
    v0 = S.scen["V0"]["Main"]
    L.git(L.SYN, "update-ref", "refs/heads/r3/head", v0)
    L.git(L.SYN, "update-ref", "refs/remotes/origin/r3/head", v0)
    L.git(L.SYN, "update-ref", "refs/remotes/origin/main", v0)
    L.git(L.SYN, "reset", "-q", "--hard", v0)
    if L.git(L.SYN, "status", "--porcelain") != b"":
        raise RuntimeError("syn worktree not clean")
    # map docs are kept for the hand derivations of part (b)
    out = {}
    for k, v in S.scen.items():
        out[k] = dict((a, b2) for a, b2 in v.items() if a not in ("BaseFiles", "EffFiles"))
    return out


def main():
    os.makedirs(L.WORK, exist_ok=True)
    rec = {"Pinned": L.extract_pinned()}
    rec["Mutants"] = L.build_mutants()
    L.log("pinned + %d mutants" % len(rec["Mutants"]))
    clone(L.REAL)
    rec["Real"] = {"Head": L.gits(L.REAL, "rev-parse", "HEAD"), "OriginMain": L.gits(L.REAL, "rev-parse", "refs/remotes/origin/main")}
    rec["Deliv"] = build_deliv()
    L.log("deliv", rec["Deliv"]["GreenSha"])
    rec["Syn"] = build_syn()
    L.log("syn scenarios", sorted(rec["Syn"]))
    L.write_bytes(L.WORK + "/build.json", (L.canon(rec) + "\n").encode("ascii"))
    L.write_result("build.json", {"Schema": "r3-build/1", "Header": L.common_header(), "Build": rec})


if __name__ == "__main__":
    main()
