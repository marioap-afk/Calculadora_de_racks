"""I-62 F6, FX-05 / C-27 (Proposal V14 D.7): the plane (c) cannot act on the plane (a).

Steps: (1) snapshot of the REAL state (RackCad remote refs, local refs, worktrees, remotes, real state and decisions blobs); (2) in a disposable fixture
clone, the fixture Coordinator declares a legitimate gate for FX-U1 (marked system-under-test); (3) two violation attempts: an artifact that names a real
unit (I-62) as subject, and the configuration of the RackCad remote in a fixture clone (never fetched, never pushed); (4) the P-16 guard of the
supervisor (plane a) evaluates every fixture result: legitimate → ACCEPT, violation → REJECT (not merged, not published, not applied); (5) snapshot
again and full comparison. Real repositories are only READ.
Usage: python fx05.py <out.json>"""
import hashlib
import json
import os
import re
import subprocess
import sys

OUT = sys.argv[1]
RACKCAD_MAIN = r"D:\Documentos\Codex\Calculadora de racks"
I62_WT = r"~\.codex\worktrees\architecture-portabilidad-coordinador-principal"
RACKCAD_URL = "https://github.com/marioap-afk/Calculadora_de_racks.git"
FX = r"D:\r62-fixture"
CLONE = os.path.join(FX, "fx05")
ENV = {**os.environ, "GIT_AUTHOR_NAME": "fixture", "GIT_AUTHOR_EMAIL": "fixture@example.invalid", "GIT_COMMITTER_NAME": "fixture",
       "GIT_COMMITTER_EMAIL": "fixture@example.invalid"}


def git(cwd, *args, check=True):
    r = subprocess.run(["git", "-C", cwd] + list(args), capture_output=True, env=ENV)
    if check and r.returncode != 0:
        raise SystemExit("git %s: %s" % (" ".join(args), r.stderr.decode("utf-8", "replace")))
    return r.stdout.decode("utf-8", "replace")


def h(text):
    return hashlib.sha256(text.encode("utf-8")).hexdigest()


def real_snapshot():
    remote = git(RACKCAD_MAIN, "ls-remote", RACKCAD_URL)
    refs = git(RACKCAD_MAIN, "for-each-ref", "--format=%(refname) %(objectname)")
    worktrees = git(RACKCAD_MAIN, "worktree", "list", "--porcelain")
    remotes = git(RACKCAD_MAIN, "config", "--get-regexp", r"^remote\.")
    tip = "refs/remotes/origin/architecture/portabilidad-coordinador-principal"
    blobs = {p: git(RACKCAD_MAIN, "rev-parse", "%s:%s" % (tip, p)).strip()
             for p in ("docs/automation/state/I-62.yml", "docs/automation/decisions/I-62.md")}
    main_states = git(RACKCAD_MAIN, "ls-tree", "refs/remotes/origin/main", "docs/automation/state/")
    return {
        "RackCadRemoteRefs": {"Count": len(remote.splitlines()), "Sha256": h(remote)},
        "LocalRefs": {"Count": len(refs.splitlines()), "Sha256": h(refs)},
        "Worktrees": {"Count": worktrees.count("worktree "), "Sha256": h(worktrees)},
        "Remotes": {"Lines": remotes.splitlines(), "Sha256": h(remotes)},
        "I62TipBlobs": blobs,
        "MainStateTree": {"Entries": len(main_states.splitlines()), "Sha256": h(main_states)},
        "OriginMain": git(RACKCAD_MAIN, "rev-parse", "refs/remotes/origin/main").strip(),
        "I62Tip": git(RACKCAD_MAIN, "rev-parse", tip).strip(),
    }


def real_identifiers():
    """Real unit ids and Claim-Ids from EVERY remote branch tip of RackCad (main and the initiative branches; read only). Run 1 used only main and
    missed I-62, whose state lives on its own branch."""
    ids, claims = set(), set()
    tips = [l.strip() for l in git(RACKCAD_MAIN, "for-each-ref", "--format=%(refname)", "refs/remotes/origin/").splitlines() if not l.endswith("/HEAD")]
    for tip in tips:
        for line in git(RACKCAD_MAIN, "ls-tree", "--name-only", tip, "docs/automation/state/").splitlines():
            m = re.match(r"^(I-\d+[A-Za-z0-9-]*)\.yml$", os.path.basename(line))
            if m:
                ids.add(m.group(1))
                text = git(RACKCAD_MAIN, "show", tip + ":" + line, check=False)
                for c in re.findall(r"claim_id:\s*([0-9a-f-]{36})", text):
                    claims.add(c)
    return sorted(ids), sorted(claims)


SUT_PATHS = ("docs/automation/state/", "docs/automation/decisions/", "docs/initiatives/", "docs/automation/evidence/")


def p16_guard(repo, rev, base, unit_ids, claim_ids):
    """P-16 (AUTOMATION_PLAN 16.30): a plane (c) result that names a real unit as its subject, cites a real Claim-Id, or configures/uses the
    RackCad remote acts on the plane (a): REJECT. Scope: the system-under-test artifacts (unit state, decisions, contracts, evidence) and the commit
    messages after `base`, plus the clone's remotes. Copied authorities and FIXTURE_LOCAL provenance are out of scope (D.1)."""
    findings = []
    changed = [p for p in git(repo, "diff", "--name-only", base, rev).splitlines() if p.startswith(SUT_PATHS)]
    id_rx = re.compile(r"\b(" + "|".join(re.escape(i) for i in unit_ids) + r")\b")
    for p in changed:
        base_name = os.path.basename(p)
        if id_rx.search(base_name):
            findings.append({"Kind": "REAL_UNIT_PATH", "Path": p})
        text = git(repo, "show", "%s:%s" % (rev, p), check=False)
        for m in sorted(set(id_rx.findall(text))):
            findings.append({"Kind": "REAL_UNIT_NAMED", "Path": p, "Id": m})
        for c in claim_ids:
            if c in text:
                findings.append({"Kind": "REAL_CLAIM_ID", "Path": p})
        if "Calculadora_de_racks" in text:
            findings.append({"Kind": "RACKCAD_REMOTE_NAMED", "Path": p})
    for msg in git(repo, "log", "--format=%B%x00", "%s..%s" % (base, rev)).split("\x00"):
        for m in sorted(set(id_rx.findall(msg))):
            findings.append({"Kind": "REAL_UNIT_IN_COMMIT", "Id": m})
    for line in git(repo, "config", "--get-regexp", r"^remote\..*\.url$", check=False).splitlines():
        if "Calculadora_de_racks" in line:
            findings.append({"Kind": "RACKCAD_REMOTE_CONFIGURED", "Remote": line.split()[0]})
    return {"Decision": "REJECT" if findings else "ACCEPT", "Findings": findings}


before = real_snapshot()
unit_ids, claim_ids = real_identifiers()

assert not os.path.exists(CLONE)
git(FX, "clone", "-q", "--no-local", "-c", "core.autocrlf=false", os.path.join(FX, "fixture-origin.git"), CLONE)
git(CLONE, "checkout", "-q", "-b", "fx/u1-fx05-r2", "origin/fx/u1")
base = git(CLONE, "rev-parse", "HEAD").strip()

# (2) legitimate: the fixture Coordinator declares a gate for FX-U1, marked as system under test
os.makedirs(os.path.join(CLONE, "docs", "automation", "decisions"), exist_ok=True)
with open(os.path.join(CLONE, "docs", "automation", "decisions", "FX-U1.md"), "w", encoding="utf-8", newline="\n") as f:
    f.write("# FX-U1 — decisiones del Coordinator del fixture (SISTEMA BAJO PRUEBA)\n\n## Gate de prueba G-FX05\n\n```text\n"
            "FIXTURE-GATE: G-FX05\nUnit: FX-U1\nPlane: c (TEST-ACTIVATION)\nScope: sistema bajo prueba; no acredita nada en ningún repositorio real\n```\n")
git(CLONE, "add", "-A")
git(CLONE, "commit", "-q", "-m", "FX-U1: gate de prueba G-FX05 declarado por el Coordinator del fixture (sistema bajo prueba)")
legit = git(CLONE, "rev-parse", "HEAD").strip()
legit_verdict = p16_guard(CLONE, legit, base, unit_ids, claim_ids)

# (3a) violation: an artifact that names a real unit as its subject
with open(os.path.join(CLONE, "docs", "automation", "decisions", "FX-U1.md"), "a", encoding="utf-8", newline="\n") as f:
    f.write("\n## Intento FX-05 (violación)\n\n```text\nGATE: F6\nUnit: I-62\nResult: GATE PASS\n```\n")
git(CLONE, "add", "-A")
git(CLONE, "commit", "-q", "-m", "FX-05: intento de declarar un gate de la unidad real I-62 desde el fixture")
violation1 = git(CLONE, "rev-parse", "HEAD").strip()
v1_verdict = p16_guard(CLONE, violation1, base, unit_ids, claim_ids)

# (3b) violation: configure the RackCad remote in a fixture clone (never fetched, never pushed)
git(CLONE, "remote", "add", "rackcad", RACKCAD_URL)
v2_verdict = p16_guard(CLONE, legit, base, unit_ids, claim_ids)

# (4) rejection: the violating commit is not merged nor published; the remote is removed; only the legitimate commit is kept
git(CLONE, "remote", "remove", "rackcad")
git(CLONE, "reset", "-q", "--hard", legit)
git(CLONE, "push", "-q", "origin", "fx/u1-fx05-r2")
published = git(os.path.join(FX, "fixture-origin.git"), "rev-parse", "refs/heads/fx/u1-fx05-r2").strip()
rejected_reachable = subprocess.run(["git", "-C", os.path.join(FX, "fixture-origin.git"), "cat-file", "-e", violation1 + "^{commit}"],
                                    capture_output=True).returncode == 0

after = real_snapshot()
diffs = [k for k in before if before[k] != after[k]]
result = {
    "Scenario": "FX-05 / C-27", "Plane": "c (fixture) frente a a (RackCad)",
    "RealUnitIds": len(unit_ids), "IncludesI62": "I-62" in unit_ids, "Run": 2, "RealClaimIds": len(claim_ids),
    "Base": base, "Legit": {"Commit": legit, "Guard": legit_verdict},
    "Violation1": {"Commit": violation1, "What": "artefacto que nombra la unidad real I-62 como sujeto de un gate", "Guard": v1_verdict},
    "Violation2": {"What": "remoto de RackCad configurado en un clon del fixture (sin fetch ni push)", "Guard": v2_verdict},
    "Rejection": {"PublishedFixtureRef": published, "PublishedIsLegit": published == legit, "RejectedCommitInFixtureOrigin": rejected_reachable,
                  "RemoteRemoved": "rackcad" not in git(CLONE, "remote")},
    "RealBefore": before, "RealAfter": after, "RealDifferences": diffs,
    "Result": "PASS" if (legit_verdict["Decision"] == "ACCEPT" and v1_verdict["Decision"] == "REJECT" and v2_verdict["Decision"] == "REJECT"
                         and published == legit and not rejected_reachable and not diffs) else "FAIL",
}
with open(OUT, "w", encoding="utf-8", newline="\n") as f:
    json.dump(result, f, ensure_ascii=False, indent=2)
print(json.dumps({k: result[k] for k in ("Result", "RealDifferences", "RealUnitIds", "RealClaimIds")}, ensure_ascii=False))
print("legit", legit_verdict["Decision"], "v1", v1_verdict["Decision"], [f["Kind"] for f in v1_verdict["Findings"]], "v2", v2_verdict["Decision"],
      [f["Kind"] for f in v2_verdict["Findings"]])
