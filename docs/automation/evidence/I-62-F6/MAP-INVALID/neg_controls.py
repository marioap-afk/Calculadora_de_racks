"""Controles negativos de mv_check.py sobre una COPIA del candidato (nunca sobre el candidato ni sobre el fixture).

Cada variante parte del grafo del candidato (S, N1, N2, E) y altera una sola cosa; mv_check.py debe dar el veredicto esperado.
Uso: python -I neg_controls.py --repo <copia bare del candidato tmp-map-*> --build <build_b.json> --checker <mv_check.py> --out <json>
"""
import argparse
import json
import os
import subprocess
import sys

TRAILER = "Agent-Protocol-Normative: I-62"


def main():
    try:
        sys.stdout.reconfigure(encoding="utf-8")
    except Exception:
        pass
    ap = argparse.ArgumentParser()
    ap.add_argument("--repo", required=True)
    ap.add_argument("--build", required=True)
    ap.add_argument("--checker", required=True)
    ap.add_argument("--out", required=True)
    a = ap.parse_args()
    assert os.path.basename(a.repo.rstrip("\\/")).startswith("tmp-map-")
    b = json.load(open(a.build, encoding="utf-8"))
    S, N1, N2, E = b["S_seed"], b["N1_i62"], b["N2_norm"], b["E_eff"]
    idx = os.path.join(a.repo, "neg.index")
    env = {**os.environ, "GIT_AUTHOR_NAME": "neg", "GIT_AUTHOR_EMAIL": "neg@example.invalid", "GIT_COMMITTER_NAME": "neg",
           "GIT_COMMITTER_EMAIL": "neg@example.invalid", "GIT_AUTHOR_DATE": "2026-10-10T03:00:00+00:00",
           "GIT_COMMITTER_DATE": "2026-10-10T03:00:00+00:00", "GIT_INDEX_FILE": idx}

    def git(*args, data=None):
        r = subprocess.run(["git", "-C", a.repo] + list(args), input=data, capture_output=True, env=env)
        if r.returncode != 0:
            raise SystemExit("git %s: %s" % (" ".join(args), r.stderr.decode("utf-8", "replace")))
        return r.stdout.decode("utf-8").strip()

    def tree_of(c, remove=(), put=None):
        if os.path.exists(idx):
            os.remove(idx)
        git("read-tree", c + "^{tree}")
        for p in remove:      # modo 0 en --index-info retira la ruta (sin árbol de trabajo)
            git("update-index", "--index-info", data=("0 " + "0" * 40 + chr(9) + p + chr(10)).encode("utf-8"))
        for p, data in (put or {}).items():
            sha = git("hash-object", "-w", "--no-filters", "--stdin", data=data)
            git("update-index", "--add", "--cacheinfo", "100644,%s,%s" % (sha, p))
        t = git("write-tree")
        os.remove(idx)
        return t

    def commit(t, parents, msg):
        args = ["commit-tree", t]
        for p in parents:
            args += ["-p", p]
        return git(*args, data=msg.encode("utf-8"))

    def merge_msg(pre=True):
        m = "Merge (control negativo)\n\nTEST-ACTIVATION.\n"
        if pre:
            m += "\nDerived formal claim table:\nInitiative | ref | tip | original claim commit | Claim-Id | temporal evidence | transition\n"
        return m

    def chain(seed_tree=None, n1_tree=None, n2_tree=None, pre=True):
        s = commit(seed_tree, [], "S'\n") if seed_tree else S
        n1 = commit(n1_tree or git("rev-parse", N1 + "^{tree}"), [s], "N1'\n") if (seed_tree or n1_tree) else N1
        n2 = commit(n2_tree or git("rev-parse", N2 + "^{tree}"), [n1], "N2'\n\n" + TRAILER + "\n") if (seed_tree or n1_tree or n2_tree) else N2
        return commit(git("rev-parse", n2 + "^{tree}"), [s, n2], merge_msg(pre))

    pt = "docs/initiatives/PROMPT_TEMPLATES.md"
    adr = "docs/adr/0048-ejecucion-delegada-portable-roles-binding-y-autoverificacion.md"
    agents2 = (git("cat-file", "blob", N2 + ":AGENTS.md") + "\n\nlínea añadida por el control negativo\n").encode("utf-8")
    ap_e = subprocess.run(["git", "-C", a.repo, "cat-file", "blob", E + ":docs/AUTOMATION_PLAN.md"], capture_output=True).stdout
    ap_nopointer = ap_e.replace("Antes de aplicar esta sección, toda unidad aplica §16.13.".encode("utf-8"), b"", 1)
    cases = []
    # N-1: base I-61 sin PROMPT_TEMPLATES (MODIFIED ausente en EFF^1)
    cases.append(("N-1 PROMPT_TEMPLATES ausente en EFF^1", chain(seed_tree=tree_of(S, remove=[pt])), ["MV-4", "MV-6"]))
    # N-2: AGENTS.md (superficie) cambia entre EFF^1 y EFF sin estar en Files
    cases.append(("N-2 AGENTS.md cambiado en la rama de activación", chain(n2_tree=tree_of(N2, put={"AGENTS.md": agents2})), ["MV-3"]))
    # N-3: ADR-0048 ausente en EFF
    cases.append(("N-3 ADR-0048 ausente en EFF", chain(n1_tree=tree_of(N1, remove=[adr]), n2_tree=tree_of(N2, remove=[adr])), ["MV-3", "MV-4"]))
    # N-4: segundo commit con el trailer en main después de EFF
    t2 = commit(git("rev-parse", E + "^{tree}"), [E], "segundo trailer\n\n" + TRAILER + "\n")
    cases.append(("N-4 trailer duplicado en main", t2, "ACTIVATION_INVALID"))
    # N-5: merge sin tabla PRE
    cases.append(("N-5 merge sin tabla PRE", chain(n2_tree=git("rev-parse", N2 + "^{tree}"), pre=False), "ACTIVATION_INVALID"))
    # N-6: puntero de 16.3/§16 retirado en EFF (AP distinto del EffBlob)
    cases.append(("N-6 puntero de §16 retirado del AP en EFF", chain(n2_tree=tree_of(N2, put={"docs/AUTOMATION_PLAN.md": ap_nopointer})), ["MV-4", "MV-7"]))
    # N-7: los dos punteros retirados: 16.3 deja de estar modificada (la entrada MODIFIED del mapa contradice la derivación)
    ap_nopointers = ap_e.replace("Antes de aplicar esta sección, toda unidad aplica §16.13.".encode("utf-8"), b"")
    cases.append(("N-7 punteros de §16 y 16.3 retirados en EFF", chain(n2_tree=tree_of(N2, put={"docs/AUTOMATION_PLAN.md": ap_nopointers})),
                  ["MV-4", "MV-6", "MV-7"]))
    res = []
    for name, tip, want in cases:
        ref = "refs/heads/neg/" + name.split()[0].lower()
        git("update-ref", ref, tip)
        r = subprocess.run([sys.executable, "-I", a.checker, "--repo", a.repo, "--main", ref, "--label", name, "--out", a.out + "." + name.split()[0] + ".json"],
                           capture_output=True, encoding="utf-8")
        o = json.load(open(a.out + "." + name.split()[0] + ".json", encoding="utf-8"))
        verdict, failed = o["Verdict"], o.get("MVFailed", [])
        ok = (verdict.startswith(want) if isinstance(want, str) else (verdict == "MAP_INVALID" and all(w in failed for w in want)))
        res.append({"Case": name, "Tip": tip, "Expected": want, "Verdict": verdict, "MVFailed": failed, "AsExpected": ok})
        os.remove(a.out + "." + name.split()[0] + ".json")
        print("%-50s esperado=%-28s obtenido=%s %s -> %s" % (name, want, verdict, failed, "OK" if ok else "DISCREPANCIA"))
    with open(a.out, "w", encoding="utf-8", newline="\n") as f:
        f.write(json.dumps(res, ensure_ascii=False, indent=1) + "\n")
    sys.exit(0 if all(x["AsExpected"] for x in res) else 1)


if __name__ == "__main__":
    main()
