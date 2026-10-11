"""I-62 A-5, preflight de A5-1: controles negativos de mv_check.py sobre una COPIA del candidato A5-1 (grafo F_seed <- F_norm <- F_eff).

Adaptación de docs/automation/evidence/I-62-F6/MAP-INVALID/neg_controls.py (blob 664843cd; grafo S <- N1 <- N2 <- E) al grafo de A5-1, con los siete
controles de §66 (N-1..N-7) y cuatro propios del método (N-8..N-11). Cada variante altera una sola cosa; mv_check.py debe dar el veredicto esperado
(MAP_INVALID con, al menos, las MV esperadas, o ACTIVATION_INVALID). Nunca toca el candidato ni el fixture: solo la copia tmp-a5-*.
Uso: python -I -B neg_controls_a5.py --repo <copia bare tmp-a5-*> --build <build_a5.json> --checker <mv_check.py> --out <json>
"""
import argparse
import json
import os
import subprocess
import sys

TRAILER = "Agent-Protocol-Normative: I-62"
POINTER = "Antes de aplicar esta sección, toda unidad aplica §16.13."
PT = "docs/initiatives/PROMPT_TEMPLATES.md"
ADR = "docs/adr/0048-ejecucion-delegada-portable-roles-binding-y-autoverificacion.md"
SCHEMA = "docs/automation/agent-execution/compatibility/clause-map.schema.json"
ROUTING = "docs/automation/agent-execution/routing.md"
AUTH_PREFIXES = ("docs/WORKFLOW.md", "docs/INITIATIVE_LIFECYCLE.md", "docs/AUTOMATION_PLAN.md", "docs/automation/agent-execution/", PT, ADR)


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
    ap.add_argument("--tips-out")
    a = ap.parse_args()
    assert os.path.basename(a.repo.rstrip("\\/")).startswith("tmp-a5-")
    b = json.load(open(a.build, encoding="utf-8"))
    S, F, E = b["F_seed"], b["F_norm"], b["F_eff"]
    idx = os.path.join(a.repo, "neg.index")
    env = {**os.environ, "GIT_AUTHOR_NAME": "neg", "GIT_AUTHOR_EMAIL": "neg@example.invalid", "GIT_COMMITTER_NAME": "neg",
           "GIT_COMMITTER_EMAIL": "neg@example.invalid", "GIT_AUTHOR_DATE": "2026-10-11T03:00:00+00:00",
           "GIT_COMMITTER_DATE": "2026-10-11T03:00:00+00:00", "GIT_INDEX_FILE": idx}

    def git(*args, data=None):
        r = subprocess.run(["git", "-C", a.repo] + list(args), input=data, capture_output=True, env=env)
        if r.returncode != 0:
            raise SystemExit("git %s: %s" % (args[0], r.stderr.decode("utf-8", "replace")[-300:]))
        return r.stdout.decode("utf-8").strip()

    def raw(spec):
        return subprocess.run(["git", "-C", a.repo, "cat-file", "blob", spec], capture_output=True).stdout

    def tree_of(c, remove=(), put=None):
        if os.path.exists(idx):
            os.remove(idx)
        git("read-tree", c + "^{tree}")
        for p in remove:
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
        m = "Merge (control negativo A5)\n\nTEST-ACTIVATION.\n"
        if pre:
            m += "\nDerived formal claim table:\nInitiative | ref | tip | original claim commit | Claim-Id | temporal evidence | transition\n"
        return m

    def chain(seed_tree=None, norm_tree=None, pre=True):
        s = commit(seed_tree, [], "F_seed'\n") if seed_tree else S
        f = commit(norm_tree or git("rev-parse", F + "^{tree}"), [s], "F_norm'\n\n" + TRAILER + "\n") if (seed_tree or norm_tree) else F
        return commit(git("rev-parse", f + "^{tree}"), [s, f], merge_msg(pre))

    f_paths = git("ls-tree", "-r", "--name-only", F).splitlines()
    auth = [p for p in f_paths if p.startswith(AUTH_PREFIXES)]
    ap_f = raw(F + ":docs/AUTOMATION_PLAN.md")
    agents2 = raw(F + ":AGENTS.md") + "\n\nlínea añadida por el control negativo\n".encode("utf-8")
    cases = []
    cases.append(("N-1 PROMPT_TEMPLATES ausente en F_seed (EFF^1)", chain(seed_tree=tree_of(S, remove=[PT])), ["MV-4", "MV-6"]))
    cases.append(("N-2 AGENTS.md cambiado en F_norm", chain(norm_tree=tree_of(F, put={"AGENTS.md": agents2})), ["MV-3"]))
    cases.append(("N-3 ADR-0048 ausente en F_norm (EFF)", chain(norm_tree=tree_of(F, remove=[ADR])), ["MV-3", "MV-4"]))
    t2 = commit(git("rev-parse", E + "^{tree}"), [E], "segundo trailer\n\n" + TRAILER + "\n")
    cases.append(("N-4 trailer duplicado en main", t2, "ACTIVATION_INVALID"))
    cases.append(("N-5 F_eff sin tabla PRE", chain(norm_tree=git("rev-parse", F + "^{tree}"), pre=False), "ACTIVATION_INVALID"))
    cases.append(("N-6 puntero de §16 retirado del AP en F_norm",
                  chain(norm_tree=tree_of(F, put={"docs/AUTOMATION_PLAN.md": ap_f.replace(POINTER.encode("utf-8"), b"", 1)})), ["MV-4", "MV-7"]))
    cases.append(("N-7 punteros de §16 y 16.3 retirados en F_norm",
                  chain(norm_tree=tree_of(F, put={"docs/AUTOMATION_PLAN.md": ap_f.replace(POINTER.encode("utf-8"), b"")})), ["MV-4", "MV-6", "MV-7"]))
    # N-8: método literal de V14 D.1 (F_seed con las autoridades de MC_I62 byte a byte; F_norm solo cambia el manifiesto)
    lit_seed = tree_of(F, put={"FIXTURE-MANIFEST.json": raw(S + ":FIXTURE-MANIFEST.json")})
    cases.append(("N-8 F_seed con las autoridades de MC_I62 (V14 D.1 literal)", chain(seed_tree=lit_seed), ["MV-3", "MV-4", "MV-6", "MV-7"]))
    # N-9: autoridades de MC_I62 por el PRIMER padre (commit de autoridades en main antes del merge)
    x = commit(tree_of(F, put={"FIXTURE-MANIFEST.json": raw(S + ":FIXTURE-MANIFEST.json")}), [S], "autoridades en first-parent\n")
    n9 = commit(git("rev-parse", F + "^{tree}"), [x, F], merge_msg(True))
    cases.append(("N-9 autoridades de MC_I62 por el primer padre", n9, ["MV-3", "MV-4", "MV-6", "MV-7"]))
    # N-10: el esquema del mapa (ENTRY) ya presente en F_seed
    cases.append(("N-10 esquema del mapa (ENTRY) presente en F_seed",
                  chain(seed_tree=tree_of(S, put={SCHEMA: raw(F + ":" + SCHEMA)})), ["MV-3"]))
    # N-11: un archivo MODIFIED con su EffBlob en F_seed
    cases.append(("N-11 routing.md (MODIFIED) con su EffBlob en F_seed",
                  chain(seed_tree=tree_of(S, put={ROUTING: raw(F + ":" + ROUTING)})), ["MV-3", "MV-4", "MV-6"]))
    res, tips = [], {}
    for name, tip, want in cases:
        key = name.split()[0]
        ref = "refs/heads/neg/" + key.lower()
        git("update-ref", ref, tip)
        tips[key] = tip
        jf = a.out + "." + key + ".json"
        subprocess.run([sys.executable, "-I", "-B", a.checker, "--repo", a.repo, "--main", ref, "--label", name, "--out", jf],
                       capture_output=True, encoding="utf-8")
        o = json.load(open(jf, encoding="utf-8"))
        verdict, failed = o["Verdict"], o.get("MVFailed", [])
        ok = (verdict.startswith(want) if isinstance(want, str) else (verdict == "MAP_INVALID" and all(w in failed for w in want)))
        res.append({"Case": name, "Tip": tip, "Expected": want, "Verdict": verdict, "MVFailed": failed, "AsExpected": ok})
        os.remove(jf)
        print("%-58s esperado=%-32s obtenido=%s %s -> %s" % (name, want, verdict, failed, "OK" if ok else "DISCREPANCIA"))
    with open(a.out, "w", encoding="utf-8", newline="\n") as fh:
        fh.write(json.dumps({"AuthorityPathsInF_norm": len(auth), "Cases": res, "AllAsExpected": all(x["AsExpected"] for x in res)},
                            ensure_ascii=False, indent=1) + "\n")
    if a.tips_out:
        with open(a.tips_out, "w", encoding="utf-8", newline="\n") as fh:
            fh.write(json.dumps(tips, indent=1) + "\n")
    sys.exit(0 if all(x["AsExpected"] for x in res) else 1)


if __name__ == "__main__":
    main()
