"""I-62 A-5 (candidata), preflight de A5-1: construye, SOLO en un repositorio temporal sin remotos, el fixture B1 exactamente como fija A5-1.

Adaptación de docs/automation/evidence/I-62-F6/MAP-INVALID/build_candidate.py (blob d637c2f4) al texto de A5-1:
  F_seed (main, commit inicial)  estado ANTERIOR a la activación según el mapa de cláusulas de MC_I62: cada archivo MODIFIED del mapa = su BaseBlob
                                 (byte a byte); ADDED/ENTRY y el propio mapa AUSENTES; las demás autoridades que copia F_norm, con su blob de F_norm;
                                 FIXTURE_LOCAL de D.1 (bytes del F_seed original del fixture) y el manifiesto (COPIED con blob y commit de origen).
  F_norm (fixture/i62-norm)      UN SOLO commit, padre F_seed: las autoridades de RackCad en MC_I62 byte a byte (lista de D.1 + Files del mapa),
                                 manifiesto con la activación y el trailer único `Agent-Protocol-Normative: I-62`.
  F_eff  (main)                  merge sin avance rápido de F_norm en main (primer padre F_seed), con la tabla PRE (sin filas) al final del cuerpo.
  tag anotado                    test-activation/I-62 -> F_eff.
  C      (fx/u2, opcional)       reclamo de la unidad nueva FX-U2, SOLO si antes el comprobador da MAP_VALID sobre main (precondición de A5-1, regla 4).

Fontanería pura (hash-object --no-filters, update-index, write-tree, commit-tree), sin filtros de fin de línea; cada blob se comprueba contra su SHA de
origen. Fechas e identidad sintética fijas: SHA reproducibles. Lee RackCad y el fixture solo con cat-file/ls-tree/rev-parse. Nunca escribe fuera de --dest.
Uso: python -I -B build_candidate_a5.py --rackcad <repo RackCad> --fixture <clon del fixture actual> --dest <D:\\r62-fixture\\tmp-a5-*>
       [--date ISO] [--out json] [--with-claim --checker <mv_check.py>]
"""
import argparse
import hashlib
import json
import os
import subprocess
import sys

BASE_RC = "bb0d5522e8411f66a51fdfb3f1f0d0514b737453"      # base del mapa (C-20b): los BaseBlob se leen aquí
MC_I62 = "6f0187cb30852971b49153c2763c8ba6caeaa64d"       # MC_I62 (punta de F4): los EffBlob
FX_SEED = "930288c51ab1b5dbd370c8ebf9206d78670b89a0"       # F_seed del fixture actual: fuente de los FIXTURE_LOCAL (bytes)
OLD_EFF = "fbe25347799e5b801ef708454212335537448bd1"       # EFF del fixture actual (MAP_INVALID; histórico)
MAP_PATH = "docs/automation/agent-execution/compatibility/I62-clause-map.json"
MAP_BLOB = "4d49d3e1a63d2b6128c9d9eeac473156a421571a"
D1_AUTHORITIES = ["docs/WORKFLOW.md", "docs/INITIATIVE_LIFECYCLE.md", "docs/AUTOMATION_PLAN.md", "docs/automation/agent-execution"]  # V14 D.1, paso 2
LOCAL = {".gitattributes": "bytes idénticos en todo clon: las copias conservan su blob de origen",
         ".github/workflows/fixture.yml": "CI del fixture: fixture-build y fixture-tests (D.2), contents: read, sin secretos",
         "AGENTS.md": "AGENTS del fixture: autoridades leídas primero y jobs requeridos del fixture (D.1)",
         "CLAUDE.md": "entrada automática de las sesiones del fixture; sin hechos de ninguna unidad (D.6)",
         "README.md": "descripción del fixture",
         "src/Fixture.Lib/Calculator.cs": "biblioteca .NET mínima (D.1)",
         "src/Fixture.Lib/Fixture.Lib.csproj": "biblioteca .NET mínima (D.1)",
         "tests/Fixture.Tests/CalculatorTests.cs": "pruebas xUnit del fixture (D.1)",
         "tests/Fixture.Tests/Fixture.Tests.csproj": "pruebas xUnit del fixture (D.1)"}
TRAILER = "Agent-Protocol-Normative: I-62"
UNIT, BRANCH, CLAIM_ID = "FX-U2", "fx/u2", "fc62f1c7-0000-4000-8000-000000000002"
RULE = ("AUTOMATION_PLAN 16.14 aplicado a ESTE repositorio: I62_EFFECTIVE_SHA del fixture = primer merge en first-parent de main cuyo segundo "
        "padre alcanza el único commit con el trailer; RackCad no se activa (derivación por repositorio)")


def run(cwd, *args, data=None, env=None):
    r = subprocess.run(["git", "-C", cwd] + list(args), input=data, capture_output=True, env=env)
    if r.returncode != 0:
        raise SystemExit("git %s: %s" % (args[0], r.stderr.decode("utf-8", "replace")[-300:]))
    return r.stdout


def blob_id(data):
    return hashlib.sha1(b"blob %d\0" % len(data) + data).hexdigest()


def listing(repo, commit, paths):
    out = {}
    for line in run(repo, "ls-tree", "-r", commit, "--", *paths).decode("utf-8").splitlines():
        meta, path = line.split("\t", 1)
        mode, kind, sha = meta.split()
        assert kind == "blob" and mode == "100644", line
        out[path] = sha
    return out


def main():
    try:
        sys.stdout.reconfigure(encoding="utf-8")
    except Exception:
        pass
    ap = argparse.ArgumentParser()
    ap.add_argument("--rackcad", required=True)
    ap.add_argument("--fixture", required=True)
    ap.add_argument("--dest", required=True)
    ap.add_argument("--date", default="2026-10-11T02:00:00+00:00")
    ap.add_argument("--out")
    ap.add_argument("--with-claim", action="store_true")
    ap.add_argument("--checker")
    a = ap.parse_args()
    assert os.path.basename(a.dest.rstrip("\\/")).startswith("tmp-a5-"), "destino fuera de tmp-a5-*"
    assert not os.path.exists(a.dest), "el destino ya existe"
    run(os.path.dirname(a.dest.rstrip("\\/")), "init", "-q", "--bare", "-b", "main", a.dest)
    assert run(a.dest, "remote").strip() == b"", "el candidato no debe tener remotos"
    run(a.dest, "config", "core.autocrlf", "false")
    secs = {"S": 0, "F": 1, "E": 2, "T": 3, "C": 4}

    def env(step):
        d = a.date.replace("T02:00:00", "T02:00:%02d" % secs[step]) if "T02:00:00" in a.date else a.date
        return {**os.environ, "GIT_AUTHOR_NAME": "fixture", "GIT_AUTHOR_EMAIL": "fixture@example.invalid", "GIT_COMMITTER_NAME": "fixture",
                "GIT_COMMITTER_EMAIL": "fixture@example.invalid", "GIT_AUTHOR_DATE": d, "GIT_COMMITTER_DATE": d,
                "GIT_INDEX_FILE": os.path.join(a.dest, "candidate.index")}

    def put(data, expect=None):
        sha = run(a.dest, "hash-object", "-w", "--no-filters", "--stdin", data=data).decode().strip()
        assert sha == blob_id(data) and (expect is None or sha == expect), (sha, expect)
        return sha

    def tree(files, step):
        idx = os.path.join(a.dest, "candidate.index")
        if os.path.exists(idx):
            os.remove(idx)
        info = "".join("100644 %s\t%s\n" % (sha, p) for p, sha in sorted(files.items()))
        run(a.dest, "update-index", "--add", "--index-info", data=info.encode("utf-8"), env=env(step))
        t = run(a.dest, "write-tree", env=env(step)).decode().strip()
        os.remove(idx)
        return t

    def commit(t, parents, msg, step):
        args = ["commit-tree", t]
        for p in parents:
            args += ["-p", p]
        return run(a.dest, *args, data=msg.encode("utf-8"), env=env(step)).decode().strip()

    def copy_blob(sha):  # bytes del objeto de RackCad, comprobados por SHA-1
        return put(run(a.rackcad, "cat-file", "blob", sha), sha)

    def manifest(entries, activation, state):
        m = {"Schema": "rackcad-fixture-manifest/v1",
             "Purpose": "I-62 F6, plano (c): sistema bajo prueba; fixture B1 construido según A-5 (A5-1)",
             "State": state, "BaseCommit": BASE_RC, "SourceCommit": MC_I62,
             "Amendment": "I-62 A-5, A5-1: F_seed = estado anterior a la activación según el mapa de cláusulas; MC_I62 solo por F_norm",
             "History": {"PreviousFixtureEff": OLD_EFF, "Status": "MAP_INVALID; evidencia histórica; no se reescribe"},
             "MapBlob": MAP_BLOB,
             "Entries": sorted(entries + [{"Path": "FIXTURE-MANIFEST.json", "Kind": "FIXTURE_LOCAL", "Reason": "este manifiesto (D.1)"}],
                               key=lambda e: e["Path"]),
             "Activation": activation}
        return (json.dumps(m, ensure_ascii=False, indent=2) + "\n").encode("utf-8")

    # ---- mapa de MC_I62 (fuente de verdad del estado anterior a la activación)
    cmap = json.loads(run(a.rackcad, "show", "%s:%s" % (MC_I62, MAP_PATH)).decode("utf-8"))
    assert run(a.rackcad, "rev-parse", "%s:%s" % (MC_I62, MAP_PATH)).decode().strip() == MAP_BLOB
    kinds = {f["Path"]: f for f in cmap["Files"]}
    # ---- autoridades que copia F_norm: lista de D.1 + Files del mapa (+ el mapa), todas en MC_I62
    eff = listing(a.rackcad, MC_I62, D1_AUTHORITIES)
    for p in kinds:
        eff.setdefault(p, run(a.rackcad, "rev-parse", "%s:%s" % (MC_I62, p)).decode().strip())
    assert eff[MAP_PATH] == MAP_BLOB
    for p, f in kinds.items():
        assert eff[p] == f["EffBlob"], ("EffBlob", p)
    base_listing = listing(a.rackcad, BASE_RC, list(eff))
    seed, seed_entries, eff_entries, checks = {}, [], [], {"ModifiedBaseAtBaseCommit": 0, "OthersEqualAtBaseCommit": 0, "AddedAbsentAtBase": 0}
    for p, sha in sorted(eff.items()):
        f = kinds.get(p)
        if p == MAP_PATH or (f and f["FileKind"] in ("ADDED", "ENTRY")):
            if p not in base_listing:
                checks["AddedAbsentAtBase"] += 1
            continue                                   # ausente en F_seed
        if f and f["FileKind"] == "MODIFIED":
            src = f["BaseBlob"]
            assert base_listing.get(p) == src, ("BaseBlob no está en la base", p)
            checks["ModifiedBaseAtBaseCommit"] += 1
            seed[p] = copy_blob(src)
            seed_entries.append({"Path": p, "Kind": "COPIED", "SourceBlob": src, "SourceCommit": BASE_RC})
        else:                                          # demás autoridades: mismo blob que en F_norm
            if base_listing.get(p) == sha:
                checks["OthersEqualAtBaseCommit"] += 1
            seed[p] = copy_blob(sha)
            seed_entries.append({"Path": p, "Kind": "COPIED", "SourceBlob": sha, "SourceCommit": MC_I62})
    norm = {p: copy_blob(sha) for p, sha in eff.items()}
    eff_entries = [{"Path": p, "Kind": "COPIED", "SourceBlob": sha, "SourceCommit": MC_I62} for p, sha in sorted(eff.items())]
    # ---- FIXTURE_LOCAL: bytes del F_seed original del fixture (D.1)
    local, local_entries = {}, []
    for p, reason in LOCAL.items():
        sha = run(a.fixture, "rev-parse", "%s:%s" % (FX_SEED, p)).decode().strip()
        local[p] = put(run(a.fixture, "cat-file", "blob", sha), sha)
        local_entries.append({"Path": p, "Kind": "FIXTURE_LOCAL", "Reason": reason})
    # ---- F_seed
    s_files = dict(local, **seed)
    s_files["FIXTURE-MANIFEST.json"] = put(manifest(seed_entries + local_entries, None, "F_seed"))
    S = commit(tree(s_files, "S"), [], "F_seed (B1, A-5): estado anterior a la activación según el mapa de cláusulas de MC_I62 (BaseBlob de RackCad "
               "bb0d5522, byte a byte) y archivos FIXTURE_LOCAL\n", "S")
    # ---- F_norm: un solo commit sobre F_seed, con las autoridades de MC_I62, la activación y el trailer único
    act = {"Marker": "TEST-ACTIVATION", "Trailer": TRAILER, "Rule": RULE, "Tag": "test-activation/I-62"}
    f_files = dict(local, **norm)
    f_files["FIXTURE-MANIFEST.json"] = put(manifest(eff_entries + local_entries, act, "F_norm"))
    F = commit(tree(f_files, "F"), [S], "F_norm (B1, A-5): autoridades de MC_I62 (RackCad 6f0187cb, byte a byte) y activación de prueba de las reglas "
               "I62 en el fixture (TEST-ACTIVATION)\n\n%s\n" % TRAILER, "F")
    # ---- F_eff: merge sin avance rápido (árbol = F_norm), tabla PRE al final del cuerpo
    pre = ("Merge fixture/i62-norm: TEST-ACTIVATION de I-62 en el fixture B1 (A-5)\n\n"
           "TEST-ACTIVATION (plano c; no activa RackCad). Fixture B1 construido conforme al mapa de cláusulas y a AP 16.13 (A-5, A5-1);\n"
           "el fixture anterior (%s) es evidencia histórica y no se reescribe.\n\n"
           "Derived formal claim table:\n"
           "Initiative | ref | tip | original claim commit | Claim-Id | temporal evidence | transition\n") % OLD_EFF[:12]
    E = commit(tree(f_files, "E"), [S, F], pre, "E")
    run(a.dest, "update-ref", "refs/heads/main", E)
    run(a.dest, "update-ref", "refs/heads/fixture/i62-norm", F)
    run(a.dest, "tag", "-a", "test-activation/I-62", "-m", "I62_EFFECTIVE_SHA del fixture B1 (TEST-ACTIVATION; A-5)", E, env=env("T"))
    T = run(a.dest, "rev-parse", "refs/tags/test-activation/I-62").decode().strip()
    out = {"Builder": "build_candidate_a5.py", "Date": a.date, "BaseCommit": BASE_RC, "SourceCommit": MC_I62, "MapBlob": MAP_BLOB,
           "F_seed": S, "F_norm": F, "F_eff": E, "TagObject": T, "SeedCopied": len(seed), "NormCopied": len(norm),
           "Local": len(local) + 1, "ProvenanceChecks": checks, "Remotes": run(a.dest, "remote").decode().split(), "Precondition": None,
           "Claim": None}
    if a.with_claim:  # A5-1, regla 4: ningún reclamo sin MAP_VALID registrado sobre main = F_eff
        assert a.checker, "--with-claim exige --checker"
        r = subprocess.run([sys.executable, "-I", "-B", a.checker, "--repo", a.dest, "--main", "refs/heads/main", "--label", "precondición A5-1"],
                           capture_output=True, encoding="utf-8")
        ok = r.returncode == 0 and "MAP_VALID" in r.stdout
        out["Precondition"] = {"Checker": os.path.basename(a.checker), "ExitCode": r.returncode, "MapValid": ok}
        if ok:
            C = commit(tree(f_files, "C"), [E], "claim: %s (unidad de prueba del fixture B1)\n\nInitiative: %s\nBranch: %s\nClaim-Id: %s\n"
                       % (UNIT, UNIT, BRANCH, CLAIM_ID), "C")
            run(a.dest, "update-ref", "refs/heads/" + BRANCH, C)
            out["Claim"] = {"Unit": UNIT, "Branch": BRANCH, "ClaimId": CLAIM_ID, "C": C}
    print(json.dumps(out, ensure_ascii=False, indent=1))
    if a.out:
        with open(a.out, "w", encoding="utf-8", newline="\n") as fh:
            fh.write(json.dumps(out, ensure_ascii=False, indent=1) + "\n")


if __name__ == "__main__":
    main()
