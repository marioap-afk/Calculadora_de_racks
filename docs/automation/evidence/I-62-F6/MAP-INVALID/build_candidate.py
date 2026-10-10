"""I-62 F6 / decisiones §66.3: construye, SOLO en un repositorio temporal, el candidato de reconstrucción de la activación del fixture.

Nunca publica: el destino no tiene remotos (opción b: `git init --bare` nuevo; opción a: clon bare del origen con el remoto eliminado). Lee blobs de
RackCad (solo lectura) y los archivos FIXTURE_LOCAL del F_seed original del fixture (solo lectura). Fontanería pura (hash-object --no-filters,
update-index, write-tree, commit-tree): sin filtros de fin de línea; cada blob se comprueba contra su SHA de origen. Fechas e identidad fijas: los SHA
del candidato son reproducibles con los mismos argumentos.

Grafo (igual al de RackCad en la integración: EFF^1 = base I-61, EFF^2 alcanza el único trailer):
  S  (main)               F_seed r2: autoridades I-61 = BaseBlob del mapa (RackCad bb0d5522) + FIXTURE_LOCAL del F_seed original + manifiesto
  N1 (fixture/i62-norm)   F_i62 r2: autoridades de MC_I62 = EffBlob del mapa (RackCad 6f0187cb), mapa 4d49d3e1 incluido; manifiesto
  N2 (fixture/i62-norm)   F_norm r2: manifiesto (Activation) + trailer `Agent-Protocol-Normative: I-62`
  E  (main)               merge --no-ff de N2 en S, con la tabla PRE «Derived formal claim table» (sin filas) al final del cuerpo
  tag anotado             test-activation/I-62 (b) | test-activation/I-62-r2 (a)
  C  (fx/<unidad>)        reclamo vacío de la unidad nueva sobre E (paso 4 de D.1), Claim-Id nuevo

Uso: python -I build_candidate.py --variant b|a --rackcad <WT> --fixture <origin.git> --dest <D:\\r62-fixture\\tmp-map-*> [--date ISO] [--out json]
"""
import argparse
import hashlib
import json
import os
import subprocess
import sys

BASE_RC = "bb0d5522e8411f66a51fdfb3f1f0d0514b737453"      # EFF^1 del mapa (origin/main de RackCad en F4; C-20b/C-20c)
MC_I62 = "6f0187cb30852971b49153c2763c8ba6caeaa64d"       # EFF^2 del mapa (punta de F4; F_seed original)
FX_SEED = "930288c51ab1b5dbd370c8ebf9206d78670b89a0"       # F_seed original (FIXTURE_LOCAL byte a byte)
OLD_EFF = "fbe25347799e5b801ef708454212335537448bd1"
SEED_PATHS = ["docs/WORKFLOW.md", "docs/INITIATIVE_LIFECYCLE.md", "docs/AUTOMATION_PLAN.md", "docs/automation/agent-execution",
              "docs/initiatives/PROMPT_TEMPLATES.md"]
EFF_EXTRA = ["docs/adr/0048-ejecucion-delegada-portable-roles-binding-y-autoverificacion.md"]
LOCAL = {".gitattributes": "bytes idénticos en todo clon: las copias conservan su blob de origen",
         ".github/workflows/fixture.yml": "CI del fixture: fixture-build y fixture-tests (D.2), contents: read, sin secretos",
         "AGENTS.md": "AGENTS del fixture: autoridades leídas primero y jobs requeridos del fixture (D.1)",
         "CLAUDE.md": "entrada automática de las sesiones Claude del fixture; sin hechos de ninguna unidad (D.6)",
         "README.md": "descripción del fixture",
         "src/Fixture.Lib/Calculator.cs": "biblioteca .NET mínima (D.1)",
         "src/Fixture.Lib/Fixture.Lib.csproj": "biblioteca .NET mínima (D.1)",
         "tests/Fixture.Tests/CalculatorTests.cs": "pruebas xUnit del fixture (D.1)",
         "tests/Fixture.Tests/Fixture.Tests.csproj": "pruebas xUnit del fixture (D.1)"}
TRAILER = "Agent-Protocol-Normative: I-62"
NAMES = {"b": {"main": "main", "norm": "fixture/i62-norm", "tag": "test-activation/I-62", "unit": "FX-U2", "branch": "fx/u2",
               "rule": "AUTOMATION_PLAN 16.14 aplicado a ESTE repositorio: I62_EFFECTIVE_SHA del fixture = primer merge en first-parent de main cuyo "
                       "segundo padre alcanza el único commit con el trailer; RackCad no se activa (derivación por repositorio)"},
         "a": {"main": "main-r2", "norm": "fixture/i62-norm-r2", "tag": "test-activation/I-62-r2", "unit": "FX-U2", "branch": "fx/u2",
               "rule": "AUTOMATION_PLAN 16.14 aplicado a ESTE repositorio con la rama main-r2 en el papel de origin/main (linaje r2): "
                       "I62_EFFECTIVE_SHA del fixture = primer merge en first-parent de main-r2 cuyo segundo padre alcanza el único commit con el "
                       "trailer; la rama main (linaje r1, MAP_INVALID) queda congelada como evidencia; RackCad no se activa"}}
CLAIM_ID = "fc62f1c7-0000-4000-8000-000000000002"


def run(cwd, *args, data=None, env=None):
    r = subprocess.run(["git", "-C", cwd] + list(args), input=data, capture_output=True, env=env)
    if r.returncode != 0:
        raise SystemExit("git %s: %s" % (" ".join(args), r.stderr.decode("utf-8", "replace")))
    return r.stdout


def blob_id(data):
    return hashlib.sha1(b"blob %d\0" % len(data) + data).hexdigest()


def listing(repo, commit, paths):
    out = []
    for line in run(repo, "ls-tree", "-r", commit, "--", *paths).decode("utf-8").splitlines():
        meta, path = line.split("\t", 1)
        mode, kind, sha = meta.split()
        assert kind == "blob" and mode == "100644", line
        out.append((path, sha))
    return out


def main():
    try:
        sys.stdout.reconfigure(encoding="utf-8")
    except Exception:
        pass
    ap = argparse.ArgumentParser()
    ap.add_argument("--variant", choices=["a", "b"], required=True)
    ap.add_argument("--rackcad", required=True)
    ap.add_argument("--fixture", required=True)
    ap.add_argument("--dest", required=True)
    ap.add_argument("--date", default="2026-10-10T02:00:00+00:00")
    ap.add_argument("--out")
    a = ap.parse_args()
    n = NAMES[a.variant]
    assert os.path.basename(a.dest.rstrip("\\/")).startswith("tmp-map-"), "destino fuera de tmp-map-*"
    assert not os.path.exists(a.dest), "el destino ya existe"
    if a.variant == "b":
        run(os.path.dirname(a.dest), "init", "-q", "--bare", "-b", "main", a.dest)
    else:
        run(os.path.dirname(a.dest), "clone", "-q", "--bare", "--no-local", a.fixture, a.dest)
        run(a.dest, "remote", "remove", "origin")
    assert run(a.dest, "remote").strip() == b"", "el candidato no debe tener remotos"
    run(a.dest, "config", "core.autocrlf", "false")
    base_secs = {"S": 0, "N1": 1, "N2": 2, "E": 3, "T": 4, "C": 5}

    def env(step):
        d = a.date.replace("T02:00:00", "T02:00:%02d" % base_secs[step]) if "T02:00:00" in a.date else a.date
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

    def manifest(entries, source, rule_src, activation):
        m = {"Schema": "rackcad-fixture-manifest/v1",
             "Purpose": "I-62 F6, plano (c): sistema bajo prueba (linaje r2: reconstrucción de la activación conforme al mapa y a AP 16.13)",
             "SourceCommit": source, "BaseCommit": BASE_RC, "SourceRule": rule_src,
             "Reconstruction": {"Lineage": "r2", "Replaces": OLD_EFF, "Cause": "MAP_INVALID en el linaje r1 (F-MAP-1; decisiones de I-62 §66.3)",
                                "MapBlob": "4d49d3e1a63d2b6128c9d9eeac473156a421571a"},
             "Entries": sorted(entries + [{"Path": "FIXTURE-MANIFEST.json", "Kind": "FIXTURE_LOCAL", "Reason": "este manifiesto (D.1)"}],
                               key=lambda e: e["Path"]),
             "Activation": activation}
        return (json.dumps(m, ensure_ascii=False, indent=2) + "\n").encode("utf-8")

    # ---- FIXTURE_LOCAL: bytes del F_seed original
    local, local_entries = {}, []
    for p, reason in LOCAL.items():
        sha = run(a.fixture, "rev-parse", "%s:%s" % (FX_SEED, p)).decode().strip()
        local[p] = put(run(a.fixture, "cat-file", "blob", sha), sha)
        local_entries.append({"Path": p, "Kind": "FIXTURE_LOCAL", "Reason": reason})

    def copied(commit_rc, paths):
        files, entries = {}, []
        for p, sha in listing(a.rackcad, commit_rc, paths):
            files[p] = put(run(a.rackcad, "cat-file", "blob", sha), sha)
            entries.append({"Path": p, "Kind": "COPIED", "SourceBlob": sha})
        return files, entries

    # ---- S: F_seed r2
    base_files, base_entries = copied(BASE_RC, SEED_PATHS)
    rule_s = "I61_BASE = EFF^1 del mapa de cláusulas (origin/main de RackCad en el cierre de F4, bb0d5522; C-20b/C-20c); copias byte a byte"
    seed = dict(local, **base_files)
    seed["FIXTURE-MANIFEST.json"] = put(manifest(base_entries + local_entries, BASE_RC, rule_s, None))
    S = commit(tree(seed, "S"), [], "F_seed (r2): autoridades I-61 de la base del mapa (RackCad bb0d5522, byte a byte) y archivos FIXTURE_LOCAL\n", "S")
    # ---- N1: F_i62 r2
    eff_files, eff_entries = copied(MC_I62, SEED_PATHS + EFF_EXTRA)
    rule_n = "MC_I62 = cierre de F4 (Proposal V14 §15; RackCad 6f0187cb) = EffBlob del mapa; copias byte a byte sobre la base I-61 (bb0d5522)"
    n1 = dict(local, **eff_files)
    n1["FIXTURE-MANIFEST.json"] = put(manifest(eff_entries + local_entries, MC_I62, rule_n, None))
    N1 = commit(tree(n1, "N1"), [S], "F_i62 (r2): autoridades de MC_I62 (RackCad 6f0187cb, byte a byte) sobre la base I-61\n", "N1")
    # ---- N2: F_norm r2 (trailer único)
    act = {"Marker": "TEST-ACTIVATION", "Trailer": TRAILER, "Rule": n["rule"], "Tag": n["tag"]}
    n2 = dict(n1)
    n2["FIXTURE-MANIFEST.json"] = put(manifest(eff_entries + local_entries, MC_I62, rule_n, act))
    N2 = commit(tree(n2, "N2"), [N1], "F_norm (r2): activación de prueba de las reglas I62 en el fixture (TEST-ACTIVATION)\n\n%s\n" % TRAILER, "N2")
    # ---- E: merge --no-ff (árbol = N2, porque S es ancestro de N2), PRE al final del cuerpo
    pre = ("Merge %s: TEST-ACTIVATION de I-62 en el fixture (linaje r2)\n\n"
           "TEST-ACTIVATION (plano c; no activa RackCad). Reconstrucción r2 de la activación conforme al mapa de cláusulas y a AP 16.13\n"
           "(decisiones de I-62 §66.3); el linaje r1 (%s) no se reescribe.\n\n"
           "Derived formal claim table:\n"
           "Initiative | ref | tip | original claim commit | Claim-Id | temporal evidence | transition\n") % (n["norm"], OLD_EFF[:12])
    E = commit(tree(n2, "E"), [S, N2], pre, "E")
    # ---- C: reclamo de la unidad nueva sobre E
    C = commit(tree(n2, "C"), [E], "claim: %s (unidad de prueba del fixture, linaje r2)\n\nInitiative: %s\nBranch: %s\nClaim-Id: %s\n"
               % (n["unit"], n["unit"], n["branch"], CLAIM_ID), "C")
    run(a.dest, "update-ref", "refs/heads/" + n["main"], E)
    run(a.dest, "update-ref", "refs/heads/" + n["norm"], N2)
    run(a.dest, "update-ref", "refs/heads/" + n["branch"], C)
    run(a.dest, "tag", "-a", n["tag"], "-m", "I62_EFFECTIVE_SHA del fixture (TEST-ACTIVATION, linaje r2)", E, env=env("T"))
    T = run(a.dest, "rev-parse", "refs/tags/" + n["tag"]).decode().strip()
    out = {"Variant": a.variant, "Dest": a.dest, "Date": a.date, "Names": n, "BaseCommit": BASE_RC, "SourceCommit": MC_I62,
           "S_seed": S, "N1_i62": N1, "N2_norm": N2, "E_eff": E, "TagObject": T, "C_claim": C, "ClaimId": CLAIM_ID,
           "SeedCopied": len(base_files), "EffCopied": len(eff_files), "Local": len(local) + 1,
           "Remotes": run(a.dest, "remote").decode().split()}
    print(json.dumps(out, ensure_ascii=False, indent=1))
    if a.out:
        with open(a.out, "w", encoding="utf-8", newline="\n") as f:
            f.write(json.dumps(out, ensure_ascii=False, indent=1) + "\n")


if __name__ == "__main__":
    main()
