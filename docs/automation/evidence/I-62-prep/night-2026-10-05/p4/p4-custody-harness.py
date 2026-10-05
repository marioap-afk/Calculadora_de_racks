"""EXPERIMENTAL — NOT AUTHORIZED FOR PRODUCTION. I-62 night order §7 (P4): VerificationTargetSha vs CustodyHeadSha.

Disposable Git harness for a PROPOSED delta (future A-2; nothing here is in force). Two validators are compared on the same inputs:

  V14 literal (AUTOMATION_PLAN 16.1 and Proposal V14 §8.5, unchanged): the gate's evaluated SHA is CurrentSha plus session commits "limited to
      docs/automation/ and the unit's documents", checked by the Coordinator with `git diff --name-only` (final diff, default rename detection);
      CI exact of CurrentSha (16.9 `Ci`), or of its image after a registered rebase (16.7).
  P4 proposed: the functional target X (VerificationTargetSha) comes from the structured handoff and equals the verification's VerifiedSha; the
      custody head Y (CustodyHeadSha) descends from X through a LINEAR chain; EVERY commit of X..Y changes only paths of an explicit custody map
      (no rename detection: a rename is a delete plus an add), never a forbidden normative/config path and never a non-regular entry; the protected
      production roots keep X's tree at every commit; CI and test evidence keep the exact identity X (a custody-head CI is a separate health signal
      and never substitutes it); a missing map, unproven descent, functional or normative change blocks the exception.

Rule ids (exact sets per scenario):
  V14-PREFIX  final diff X..Y touches a path outside docs/automation/ and the unit documents
  V14-CI      no successful CI with head_sha = X (or its registered image)
  P4-M0       custody map absent
  P4-T1       target not taken from the structured handoff (narrative or prompt only)
  P4-T2       target is not an existing commit or differs from VerifiedSha (or its registered image)
  P4-D1       X (or its registered image) is not an ancestor of Y
  P4-D2       X..Y is not linear (a merge commit inside the range)
  P4-P1       a commit of X..Y changes a path outside the custody map (renames disabled)
  P4-P2       a protected production root differs from X's tree at some commit of X..Y
  P4-P3       a non-regular entry (symlink or gitlink) inside the custody range
  P4-N1       a commit of X..Y touches a forbidden normative or configuration path
  P4-C1       no successful CI with head_sha = the effective target (a CI at Y does not count)

Usage: python p4-custody-harness.py <out.json> [<real repo>]   (stdlib + git CLI; temporary repositories are removed)
"""
import fnmatch
import json
import os
import shutil
import subprocess
import sys
import tempfile

ENV = dict(os.environ, GIT_AUTHOR_NAME="h", GIT_AUTHOR_EMAIL="h@x", GIT_COMMITTER_NAME="h", GIT_COMMITTER_EMAIL="h@x",
           GIT_AUTHOR_DATE="2026-10-05T00:00:00Z", GIT_COMMITTER_DATE="2026-10-05T00:00:00Z", GIT_CONFIG_NOSYSTEM="1")
STEP = [0]


def git(repo, *args, inp=None, ok=(0, 1)):
    STEP[0] += 1
    env = dict(ENV, GIT_AUTHOR_DATE="2026-10-05T00:%02d:%02dZ" % (STEP[0] // 60 % 60, STEP[0] % 60))
    env["GIT_COMMITTER_DATE"] = env["GIT_AUTHOR_DATE"]
    r = subprocess.run(["git", "-c", "core.autocrlf=false", "-c", "core.filemode=false", "-C", repo] + list(args), input=inp,
                       capture_output=True, text=True, env=env)
    if r.returncode not in ok:
        raise RuntimeError("git %s: %s" % (" ".join(args), r.stderr.strip()))
    return r


def out(repo, *args):
    return git(repo, *args).stdout.strip()


def tree_of(repo, commit_sha, root):
    r = git(repo, "rev-parse", "%s:%s" % (commit_sha, root.rstrip("/")), ok=(0, 1, 128))
    return r.stdout.strip() if r.returncode == 0 else "MISSING"


# ------------------------------------------------------------------ validators
def match(path, patterns):
    return any(fnmatch.fnmatchcase(path, p) or (p.endswith("/") and path.startswith(p)) for p in patterns)


def v14_literal(repo, x, y, unit_docs, ci, rebase_map):
    rules = set()
    target = rebase_map.get(x, x)
    for p in out(repo, "diff", "--name-only", target, y).splitlines():      # final diff from CurrentSha or its image, default rename detection (16.1)
        if not (p.startswith("docs/automation/") or match(p, unit_docs)):
            rules.add("V14-PREFIX")
    if not any(r["sha"] == target and r["ok"] for r in ci):                # 16.9 `Ci`; after a registered rebase, the push run of CurrentSha' (16.7)
        rules.add("V14-CI")
    return rules


def p4_proposed(repo, handoff, verification, y, cmap, ci, rebase_map):
    rules, facts = set(), {}
    if cmap is None:
        return {"P4-M0"}, facts
    x = handoff.get("CurrentSha") if isinstance(handoff, dict) else None
    if not x:
        return {"P4-T1"}, facts
    exists = git(repo, "cat-file", "-e", x + "^{commit}", ok=(0, 1, 128)).returncode == 0
    target = rebase_map.get(x, x)
    if not exists or verification.get("VerifiedSha") not in (x, target):
        return {"P4-T2"}, facts
    facts["VerificationTargetSha"], facts["EffectiveTarget"], facts["CustodyHeadSha"] = x, target, y
    if git(repo, "merge-base", "--is-ancestor", target, y, ok=(0, 1, 128)).returncode != 0:
        return {"P4-D1"}, facts
    commits = out(repo, "rev-list", "--reverse", "--parents", "%s..%s" % (target, y)).splitlines()
    facts["CustodyCommits"] = len(commits)
    if any(len(c.split()) != 2 for c in commits):
        rules.add("P4-D2")
    base_trees = {r: tree_of(repo, target, r) for r in cmap["ProtectedRoots"]}
    for line in commits:
        c, *parents = line.split()
        if len(parents) != 1:
            continue
        for row in out(repo, "diff-tree", "-r", "--no-renames", "--raw", "--no-commit-id", parents[0], c).splitlines():
            meta, path = row.split("\t", 1)
            old_mode, new_mode = meta.split()[0].lstrip(":"), meta.split()[1]
            if new_mode in ("120000", "160000") or old_mode in ("120000", "160000"):
                rules.add("P4-P3")
            if not match(path, cmap["Allowed"]):
                rules.add("P4-P1")
            if match(path, cmap["Forbidden"]):
                rules.add("P4-N1")
        for root, tree in base_trees.items():
            if tree_of(repo, c, root) != tree:
                rules.add("P4-P2")
    if not any(r["sha"] == target and r["ok"] for r in ci):
        rules.add("P4-C1")
    facts["CustodyHealth"] = [r for r in ci if r["sha"] == y]
    facts["EvidenceSha"] = target
    return rules, facts


# ------------------------------------------------------------------ synthetic repository
FORBIDDEN = ["AGENTS.md", "CLAUDE.md", "docs/WORKFLOW.md", "docs/INITIATIVE_LIFECYCLE.md", "docs/AUTOMATION_PLAN.md", "docs/FOUNDATIONS.md",
             "docs/automation/agent-execution/", "docs/adr/", ".github/", "global.json", "*.props", "*.targets", "*.csproj", "*.sln",
             ".gitattributes", ".gitignore", "docs/initiatives/U-proposal-*.md", "docs/initiatives/U-consensus-freeze.md"]
CMAP = {"Allowed": ["docs/automation/evidence/U-evidence.md", "docs/automation/evidence/U-pilot/", "docs/automation/state/U.yml",
                    "docs/automation/decisions/U.md"],
        "Forbidden": FORBIDDEN, "ProtectedRoots": ["src/", "tests/"]}
UNIT_DOCS = ["docs/initiatives/U-*"]


def write(repo, rel, text):
    p = os.path.join(repo, rel)
    os.makedirs(os.path.dirname(p), exist_ok=True)
    with open(p, "w", encoding="utf-8", newline="\n") as f:
        f.write(text)


def commit(repo, msg, files=None, delete=(), mv=None):
    for rel, text in (files or {}).items():
        write(repo, rel, text)
    for rel in delete:
        git(repo, "rm", "-q", rel)
    if mv:
        os.makedirs(os.path.dirname(os.path.join(repo, mv[1])), exist_ok=True)
        git(repo, "mv", mv[0], mv[1])
    git(repo, "add", "-A")
    git(repo, "commit", "-q", "--allow-empty", "-m", msg)
    return out(repo, "rev-parse", "HEAD")


def custody(repo, tag):
    return commit(repo, "custody " + tag, {"docs/automation/evidence/U-pilot/%s/relay-record.json" % tag: '{"t": "%s"}\n' % tag,
                                           "docs/automation/evidence/U-evidence.md": "# evidence\n%s\n" % tag,
                                           "docs/automation/state/U.yml": "state: %s\n" % tag})


def build(root):
    repo = os.path.join(root, "r")
    os.makedirs(repo)
    git(repo, "init", "-q", "-b", "main")
    base = commit(repo, "base", {"src/A.cs": "class A {}\n", "tests/ATests.cs": "class ATests {}\n", "docs/WORKFLOW.md": "# wf\n",
                                 "docs/automation/agent-execution/README.md": "# protocol\n", "docs/automation/evidence/U-evidence.md": "# evidence\n",
                                 "docs/initiatives/U-contract.md": "# contract\n", ".github/workflows/ci.yml": "on: push\n", "global.json": "{}\n",
                                 "docs/automation/state/U.yml": "state: 0\n"})
    green = commit(repo, "GREEN", {"src/A.cs": "class A { int F() => 1; }\n", "tests/ATests.cs": "class ATests { /* F */ }\n"})
    return repo, base, green


def scenarios(repo, base, x):
    S = []

    def at(start, name):
        git(repo, "checkout", "-q", "-B", name, start)

    def add(name, y, covers, exp_v14, exp_p4, handoff=None, verification=None, cmap=CMAP, ci=None, rebase_map=None, note=""):
        S.append({"Name": name, "Covers": covers, "Y": y, "Handoff": handoff if handoff is not None else {"CurrentSha": x},
                  "Verification": verification if verification is not None else {"VerifiedSha": x}, "Map": cmap,
                  "Ci": ci if ci is not None else [{"sha": x, "ok": True}], "RebaseMap": rebase_map or {}, "ExpectV14": set(exp_v14),
                  "ExpectP4": set(exp_p4), "Note": note})

    at(x, "s01"); y = custody(repo, "s01")
    add("p4-01-replica-i63-custodia-solo-docs", y, ["case"], [], [], note="réplica sintética de I-63 §51.1: custodia de docs sobre el GREEN")
    add("p4-01b-sin-custodia-Y-igual-a-X", x, ["case"], [], [], note="rango vacío: la excepción no hace falta")
    at(x, "s02"); commit(repo, "touch src", {"src/A.cs": "class A { int F() => 2; }\n"}); commit(repo, "revert src", {"src/A.cs": "class A { int F() => 1; }\n"})
    y = custody(repo, "s02")
    add("p4-02-cambia-y-revierte-produccion", y, ["revert"], [], ["P4-P1", "P4-P2"], note="el diff final parece vacío en src/")
    at(x, "s03"); y = commit(repo, "edit protocol", {"docs/automation/agent-execution/README.md": "# protocol v2\n"})
    add("p4-03-autoridad-normativa-bajo-docs-automation", y, ["normative"], [], ["P4-P1", "P4-N1"], note="16.1 lo admite por prefijo")
    at(x, "s04"); y = commit(repo, "notes", {"src/Docs/NOTES.md": "# notes\n"})
    add("p4-04-md-bajo-src", y, ["md"], ["V14-PREFIX"], ["P4-P1", "P4-P2"], note="terminar en .md no basta")
    at(x, "s04b"); y = commit(repo, "workflow", {"docs/WORKFLOW.md": "# wf v2\n"})
    add("p4-04b-md-normativo-fuera-de-la-custodia", y, ["md", "normative"], ["V14-PREFIX"], ["P4-P1", "P4-N1"])
    at(base, "s05"); git(repo, "checkout", "-q", x, "--", "src", "tests"); commit(repo, "same tree, other commit")
    y = custody(repo, "s05")
    add("p4-05-Y-no-desciende-de-X", y, ["descent"], [], ["P4-D1"], note="mismo árbol funcional, otra historia")
    at(base, "side"); side = custody(repo, "side")
    at(x, "s06"); custody(repo, "s06"); git(repo, "merge", "-q", "--no-ff", "--no-edit", "-X", "theirs", side)
    y = out(repo, "rev-parse", "HEAD")
    add("p4-06-merge-dentro-del-rango", y, ["linear"], [], ["P4-D2", "P4-P2"],
        note="la rama lateral solo trae custodia, pero no es lineal y su commit lleva el árbol funcional anterior a X")
    at(x, "s07"); y = commit(repo, "ci", {".github/workflows/ci.yml": "on: [push, pull_request]\n", "global.json": '{"sdk": {}}\n'})
    add("p4-07-configuracion-relevante", y, ["config"], ["V14-PREFIX"], ["P4-P1", "P4-N1"])
    at(x, "s08"); y = custody(repo, "s08")
    add("p4-08-objetivo-solo-en-el-prompt", y, ["P1"], [], ["P4-T1"], handoff={"Narrative": "GREEN %s" % x[:8]},
        note="la entrega no trae CurrentSha; el validador literal usa el SHA del prompt")
    add("p4-09-objetivo-inexistente", y, ["P1"], [], ["P4-T2"], handoff={"CurrentSha": "0123456789abcdef0123456789abcdef01234567"},
        note="nc1 de I-63 G4: CurrentSha mutado a un SHA inexistente")
    add("p4-10-ci-solo-en-la-cabeza-de-custodia", y, ["ci"], ["V14-CI"], ["P4-C1"], ci=[{"sha": y, "ok": True}],
        note="la CI de Y es salud de custodia, no evidencia de X")
    at(base, "main2"); b2 = commit(repo, "main advanced", {"docs/initiatives/Other.md": "# other\n"})
    git(repo, "checkout", "-q", "-B", "s11", b2); git(repo, "cherry-pick", x); x2 = out(repo, "rev-parse", "HEAD")
    y = custody(repo, "s11")
    add("p4-11a-rebase-con-mapa-ci-solo-en-X", y, ["rebase", "ci"], ["V14-CI"], ["P4-C1"], verification={"VerifiedSha": x2},
        rebase_map={x: x2}, note="la identidad de la CI no se traslada a la imagen (16.7 ya lo exige: Ci usa la corrida de CurrentSha')")
    add("p4-11b-rebase-con-mapa-ci-en-la-imagen", y, ["rebase"], [], [], verification={"VerifiedSha": x2}, rebase_map={x: x2},
        ci=[{"sha": x, "ok": True}, {"sha": x2, "ok": True}])
    add("p4-12-rebase-sin-mapa", y, ["rebase", "map"], ["V14-PREFIX"], ["P4-T2"], verification={"VerifiedSha": x2},
        note="sin mapa custodiado la imagen no es el objetivo verificado")
    at(x, "s13"); y = commit(repo, "move", mv=("src/A.cs", "docs/automation/evidence/U-pilot/A.cs.txt"))
    add("p4-13-renombre-de-produccion-hacia-la-custodia", y, ["rename"], [], ["P4-P1", "P4-P2"],
        note="git diff --name-only con detección de renombres solo muestra el destino")
    at(x, "s14"); y = commit(repo, "delete test", delete=["tests/ATests.cs"])
    add("p4-14-borrado-de-prueba", y, ["delete"], ["V14-PREFIX"], ["P4-P1", "P4-P2"])
    at(x, "s15"); blob = git(repo, "hash-object", "-w", "--stdin", inp="../../../../src/A.cs").stdout.strip()
    git(repo, "update-index", "--add", "--cacheinfo", "120000,%s,docs/automation/evidence/U-pilot/link" % blob)
    git(repo, "commit", "-q", "-m", "symlink"); y = out(repo, "rev-parse", "HEAD")
    add("p4-15-enlace-simbolico-en-la-custodia", y, ["non-regular"], [], ["P4-P3"])
    at(x, "s16"); y = custody(repo, "s16")
    add("p4-16-sin-mapa-de-custodia", y, ["map"], [], ["P4-M0"], cmap=None)
    at(x, "s17"); y = commit(repo, "contract", {"docs/initiatives/U-contract.md": "# contract v2\n"})
    add("p4-17-documento-de-la-unidad-fuera-del-mapa", y, ["map"], [], ["P4-P1"],
        note="16.1 admite los documentos de la unidad; el mapa propuesto los enumera y este no figura")
    return S


def run_synthetic():
    root = tempfile.mkdtemp(prefix="p4h-")
    try:
        repo, base, x = build(root)
        results, ok = {}, True
        for s in scenarios(repo, base, x):
            prompt_x = x
            gv14 = v14_literal(repo, prompt_x, s["Y"], UNIT_DOCS, s["Ci"], s["RebaseMap"])
            gp4, facts = p4_proposed(repo, s["Handoff"], s["Verification"], s["Y"], s["Map"], s["Ci"], s["RebaseMap"])
            passed = gv14 == s["ExpectV14"] and gp4 == s["ExpectP4"]
            ok = ok and passed
            results[s["Name"]] = {"Covers": s["Covers"], "Note": s["Note"],
                                  "V14Literal": {"Expected": sorted(s["ExpectV14"]), "Got": sorted(gv14), "Verdict": "ALLOWED" if not gv14 else "BLOCKED"},
                                  "P4Proposed": {"Expected": sorted(s["ExpectP4"]), "Got": sorted(gp4), "Verdict": "ALLOWED" if not gp4 else "BLOCKED",
                                                 "CustodyCommits": facts.get("CustodyCommits")},
                                  "Delta": "V14 ALLOWED / P4 BLOCKED" if not gv14 and gp4 else ("same" if bool(gv14) == bool(gp4) else "V14 BLOCKED / P4 ALLOWED"),
                                  "Result": "PASS" if passed else "FAIL"}
        return results, ok
    finally:
        shutil.rmtree(root, ignore_errors=True)


def run_real(repo):
    """The I-63 cases from canonical artifacts (measured; read-only on the real repository)."""
    i63_map = {"Allowed": ["docs/automation/evidence/I-63-evidence.md", "docs/automation/evidence/I-63-pilot/", "docs/automation/state/I-63.yml",
                           "docs/automation/decisions/I-63.md"],
               "Forbidden": [f.replace("U-", "I-63-") for f in FORBIDDEN], "ProtectedRoots": ["src/", "tests/"]}
    cases = []
    handoff = json.loads(subprocess.check_output(["git", "-C", repo, "show",
                                                  "7bd5a743:docs/automation/evidence/I-63-pilot/G4-PROJECT-SUMMARY/R20261003T005720Z-2b14/worker-handoff.json"]))
    x, y = handoff["CurrentSha"], out(repo, "rev-parse", "7bd5a743")
    ci = [{"sha": "4f45f4464f5d434f7fa478fea1117d82283ee997", "ok": True, "run": 37085650331}]
    gv14 = v14_literal(repo, x, y, ["docs/initiatives/I-63*"], ci, {})
    gp4, facts = p4_proposed(repo, handoff, {"VerifiedSha": "4f45f4464f5d434f7fa478fea1117d82283ee997"}, y, i63_map, ci, {})
    cases.append({"Name": "real-i63-g4-custodia-7bd5a743-sobre-green-4f45f446", "Source": "I-63 evidencia §51.1; worker-handoff.json del run R20261003T005720Z-2b14",
                  "X": x, "Y": y, "V14Literal": sorted(gv14), "P4Proposed": sorted(gp4), "Facts": facts,
                  "Expected": {"V14Literal": [], "P4Proposed": []}})
    xc, yc = "55a66b3c412fa63a445dc1985977ad660ea72dd7", "3c5019bf5fd31de73c3c65675e17c8a2ec9b9362"
    gv14c = v14_literal(repo, xc, yc, ["docs/initiatives/I-63*"], [{"sha": xc, "ok": True}], {})
    gp4c, factsc = p4_proposed(repo, {"CurrentSha": xc}, {"VerifiedSha": xc}, yc, i63_map, [{"sha": xc, "ok": True}], {})
    cases.append({"Name": "real-i63-cierre-3c5019bf-sobre-candidato-55a66b3c (informativo)",
                  "Source": "tag integration/I-63; WORKFLOW §11.5: el cierre tiene su propia CI y nunca sustituye la del Candidato",
                  "X": xc, "Y": yc, "V14Literal": sorted(gv14c), "P4Proposed": sorted(gp4c), "Facts": factsc,
                  "Expected": {"V14Literal": ["V14-PREFIX"], "P4Proposed": ["P4-P1"]},
                  "Reading": "el cierre de integración no es custodia de gate: lo rige WORKFLOW §11.5 y no esta excepción"})
    ok = all(c["V14Literal"] == c["Expected"]["V14Literal"] and c["P4Proposed"] == c["Expected"]["P4Proposed"] for c in cases)
    return cases, ok


def main():
    synthetic, ok_s = run_synthetic()
    real, ok_r = run_real(sys.argv[2]) if len(sys.argv) > 2 else ([], True)
    covers = sorted({c for v in synthetic.values() for c in v["Covers"]})
    rules_seen = sorted({r for v in synthetic.values() for r in v["P4Proposed"]["Got"] + v["V14Literal"]["Got"]})
    doc = {"Label": "EXPERIMENTAL — NOT AUTHORIZED FOR PRODUCTION", "Script": "p4-custody-harness.py",
           "Purpose": "dossier P4 para una A-2 futura (orden nocturna §7); nada se aplica",
           "Git": subprocess.check_output(["git", "--version"], text=True).strip(),
           "Synthetic": synthetic, "Real": real, "Covers": covers, "RulesExercised": rules_seen,
           "Deltas": sorted(k for k, v in synthetic.items() if v["Delta"] != "same"),
           "AllAsExpected": ok_s and ok_r}
    with open(sys.argv[1], "w", encoding="utf-8", newline="\n") as f:
        json.dump(doc, f, ensure_ascii=False, indent=1)
        f.write("\n")
    for k, v in synthetic.items():
        print("%-52s V14 %-7s P4 %-7s %s %s" % (k, v["V14Literal"]["Verdict"], v["P4Proposed"]["Verdict"], v["Result"], v["P4Proposed"]["Got"]))
    for c in real:
        print("%-52s V14 %s P4 %s" % (c["Name"][:52], c["V14Literal"], c["P4Proposed"]))
    print("all", doc["AllAsExpected"], "rules", rules_seen)
    return 0 if doc["AllAsExpected"] else 1


if __name__ == "__main__":
    sys.exit(main())
