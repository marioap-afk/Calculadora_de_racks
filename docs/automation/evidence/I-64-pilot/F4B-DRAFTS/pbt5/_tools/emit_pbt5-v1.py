"""La emision de PB-T5 (F4B-E62, evidencia 292): un solo commit documental en el worktree de I-64.

Lo corre la sesion. Este guion no se ha ejecutado al escribirlo.

  --dry     Solo lee. Comprueba el worktree y su numeracion, la CI exacta del Candidato, la linea BOUNDED_RESULT
            de la comprobacion acotada y lo que se custodia; escribe el listado en --out.
  --commit  Repite todo, exige que el listado sea igual al del --dry (--dry-listing), copia con la redaccion,
            saca el contrato (el 6 del paquete) a su documento, anade F4B-E62 y la 292, cambia I-64.yml,
            hace el commit y lo sube sin forzar.

Uso:
  python -I emit_pbt5.py --dry    --wt <worktree> --src <carpeta pbt5> --cand <sha> --run <id> --bounded <informe> --out <carpeta>
  python -I emit_pbt5.py --commit --wt <worktree> --src <carpeta pbt5> --cand <sha> --run <id> --bounded <informe> --out <carpeta> --dry-listing <listado>

Para en cuanto algo no cuadra (PBT5-C17 si es una condicion de la emision): no escribe nada a medias.
"""
import argparse
import datetime
import hashlib
import json
import os
import re
import subprocess
import sys

BRANCH = "architecture/workspace-persistente-rackcad"
TASK = "F4B-PBT5-SMOKE-PB-GATE-CLOSE"
JOBS = {"Tests (Domain + Application)", "UI Tests (WPF controls, net8.0-windows)",
        "Build UI (WPF, valida API de Application)", "Build Plugin without AutoCAD"}
DISP = "docs/automation/evidence/I-64-pilot/F4B-DRAFTS/f4b-dispositions.md"
EVID = "docs/automation/evidence/I-64-evidence.md"
YML = "docs/automation/state/I-64.yml"
DRAFTS = "docs/automation/evidence/I-64-pilot/F4B-DRAFTS"
SPECIAL = {
    "I-64-f4b-pbt5-package-v2.3.md": "docs/initiatives/I-64-f4b-pbt5-package.md",
    "owner-pbt5-decisions.md": DRAFTS + "/coordinator/owner-pbt5-decisions.md",
}
CONTRACT = "docs/automation/evidence/I-64-pilot/" + TASK + "/coordinator/contract.md"
HIST = DRAFTS + "/pbt5"
MAXREL = 156
REDACT = [
    ("D:" + chr(92) + "spb1" + chr(92) + "smoke", "<carpeta-del-Smoke-PB>"),
    ("D:" + chr(92) + "spb1" + chr(92) + "respaldo", "<carpeta-de-respaldo>"),
    ("D:" + "/spb1/smoke", "<carpeta-del-Smoke-PB-con-/>"),
    ("D:" + "/spb1/respaldo", "<carpeta-de-respaldo-con-/>"),
    ("D:" + chr(92) + "spb1", "<raiz-de-trabajo>"),
    ("D:" + "/spb1", "<raiz-de-trabajo-con-/>"),
]


def stop(msg):
    sys.exit("STOP: " + msg)


def run(args, cwd=None, check=True):
    r = subprocess.run(args, cwd=cwd, capture_output=True)
    if check and r.returncode != 0:
        stop(f"{' '.join(args[:4])} … salio con {r.returncode}: {r.stderr.decode('utf-8', 'replace')[:300]}")
    return r


def git(wt, *a, check=True):
    return run(["git", "-C", wt] + list(a), check=check)


def sha(b):
    return hashlib.sha256(b).hexdigest()


def redact(text):
    for a, b in REDACT:
        text = text.replace(a, b)
    return text


def load_patterns(src):
    p = os.path.join(src, "_tools", "patterns.local.txt")
    if not os.path.exists(p):
        stop("falta _tools/patterns.local.txt: sin el no se puede comprobar la privacidad")
    return [re.compile(ln.rstrip("\n")) for ln in open(p, encoding="utf-8") if ln.strip()]


def check_worktree(wt, cand):
    if not re.fullmatch(r"[0-9a-f]{40}", cand):
        stop("el Candidato no es un SHA completo")
    if git(wt, "status", "--porcelain").stdout.strip():
        stop("el worktree no esta limpio")
    if git(wt, "rev-parse", "--abbrev-ref", "HEAD").stdout.decode().strip() != BRANCH:
        stop("el worktree no esta en la rama de I-64")
    head = git(wt, "rev-parse", "HEAD").stdout.decode().strip()
    if head != cand:
        stop(f"HEAD ({head[:8]}) no es el Candidato: la emision va justo encima del cierre de PB-T4e, despues del build")
    remote = git(wt, "ls-remote", "origin", "refs/heads/" + BRANCH).stdout.decode().split()
    if not remote or remote[0] != head:
        stop("la rama remota no esta en HEAD")
    disp = git(wt, "show", "HEAD:" + DISP).stdout.decode("utf-8")
    evid = git(wt, "show", "HEAD:" + EVID).stdout.decode("utf-8")
    if not re.search(r"^## F4B-E61\. Cierre de F4B-PBT4-EDIT-ALL-RELOAD", disp, re.M):
        stop("no esta F4B-E61, el cierre de PB-T4e")
    if re.search(r"^## F4B-E62\.", disp, re.M):
        stop("F4B-E62 ya existe")
    if not re.search(r"^## 291\. ", evid, re.M) or re.search(r"^## 292\. ", evid, re.M):
        stop("la evidencia no termina en la 291")
    return head


def check_ci(wt, cand, runid):
    r = run(["gh", "run", "view", runid, "--json", "headSha,event,headBranch,status,conclusion,jobs"], cwd=wt)
    v = json.loads(r.stdout.decode("utf-8"))
    if v["headSha"] != cand or v["event"] != "push" or v["headBranch"] != BRANCH:
        stop("PBT5-C17: la corrida no es la de push del Candidato en la rama")
    if v["status"] != "completed" or v["conclusion"] != "success":
        stop(f"PBT5-C17: la CI del Candidato no esta en verde ({v['status']}, {v['conclusion']})")
    ok = {j["name"] for j in v["jobs"] if j.get("conclusion") == "success"}
    if not JOBS <= ok:
        stop("PBT5-C17: falta en success algun trabajo: " + ", ".join(sorted(JOBS - ok)))
    return {"run": runid, "headSha": v["headSha"], "jobs": sorted(JOBS)}


def check_bounded(path):
    lines = [ln.strip() for ln in open(path, encoding="utf-8") if ln.strip().startswith("BOUNDED_RESULT:")]
    if len(lines) != 1:
        stop("PBT5-C17: el informe de la comprobacion acotada no trae una sola linea BOUNDED_RESULT:")
    ln = lines[0]
    if "EMITIBLE" not in ln or not re.search(r"\bB\s*0\s*/\s*I\s*0\b", ln) or "NO EMITIBLE" in ln:
        stop("PBT5-C17: la comprobacion acotada no da EMITIBLE con B 0 / I 0: " + ln[:200])
    return ln


def collect(src, out, wt, pats):
    srcA = os.path.abspath(src)
    for d in (out, wt):
        if os.path.abspath(d).lower().startswith(srcA.lower()):
            stop("--out y el worktree tienen que quedar fuera de --src")
    items = []
    for dp, dn, fn in os.walk(srcA):
        dn[:] = sorted(d for d in dn if d != "__pycache__")
        for f in sorted(fn):
            full = os.path.join(dp, f)
            rel = os.path.relpath(full, srcA).replace(os.sep, "/")
            if rel.startswith("_tools/") and rel.endswith(".local.txt"):
                continue
            if f.endswith(".pyc"):
                continue
            target = SPECIAL.get(rel, HIST + "/" + rel)
            data = open(full, "rb").read()
            try:
                text = redact(data.decode("utf-8"))
                body = text.encode("utf-8")
                hits = sum(1 for rx in pats for _ in rx.finditer(text))
                mode = "copia" if hits == 0 else "solo-sha256"
            except UnicodeDecodeError:
                body, hits, mode = data, -1, "solo-sha256"
            if len(target) > MAXREL:
                stop(f"ruta demasiado larga en el repositorio: {target}")
            items.append({"rel": rel, "target": target, "sha256": sha(data), "sha256_redactado": sha(body), "modo": mode})
            if rel == "I-64-f4b-pbt5-package-v2.3.md":
                items.append({"rel": rel, "target": HIST + "/" + rel, "sha256": sha(data), "sha256_redactado": sha(body), "modo": mode})
    for it in items:
        if git(wt, "cat-file", "-e", "HEAD:" + it["target"], check=False).returncode == 0 or os.path.exists(os.path.join(wt, it["target"])):
            stop("ya existe en el worktree: " + it["target"])
    for need in SPECIAL:
        if not any(i["rel"] == need and i["modo"] == "copia" for i in items):
            stop(f"{need} no esta, o casa un patron prohibido tras la redaccion")
    return items


def contract_text(src):
    pkg = open(os.path.join(src, "I-64-f4b-pbt5-package-v2.3.md"), encoding="utf-8").read()
    a = pkg.index("## 6. El contrato (forma B, la que eligió el Owner)")
    b = pkg.index("## 7. Riesgos")
    head = ("# Contrato de " + TASK + " (PB-T5)\n\n"
            "> Es, tal cual, el §6 del paquete de PB-T5 v2.3 (`docs/initiatives/I-64-f4b-pbt5-package.md`), con la redacción común. "
            "Forma B, la que eligió el Owner (`F4B-DRAFTS/coordinator/owner-pbt5-decisions.md`, Q-PBT5-8): sin `gate-contract.json`, sin Controller, "
            "sin nc1 a nc4 y sin D2, con una revisión independiente de la evidencia antes de la firma.\n\n")
    return redact(head + pkg[a:b].rstrip() + "\n")


def texts(cand, ci, bounded, items, ctext, today):
    pkg = next(i for i in items if i["target"] == SPECIAL["I-64-f4b-pbt5-package-v2.3.md"])
    own = next(i for i in items if i["rel"] == "owner-pbt5-decisions.md")
    shaonly = [i for i in items if i["modo"] == "solo-sha256"]
    so = "\n".join(f"- `{i['rel']}`, solo por su sha256 `{i['sha256']}`;" for i in shaonly) or "- ninguno."
    c8 = cand[:8]
    e62 = f"""
## F4B-E62. Emisión de {TASK} (PB-T5), con el paquete v2.3 y las decisiones del Owner

**Fecha:** {today}.

**Autoridad de emisión:** la decisión del Owner Q-PBT5-7, «Sí, con condiciones (Recomendado)» (`F4B-DRAFTS/coordinator/owner-pbt5-decisions.md`, sha256 `{own['sha256']}`). Sus tres condiciones se cumplen:
- **la comprobación acotada** del paquete v2.3: `{bounded}`;
- **ninguna decisión material nueva;**
- **la CI exacta del Candidato** `{c8}` en verde: la corrida de push `{ci['run']}`, con sus cuatro trabajos en `success`.

Vuelve al Owner por una decisión nueva, una prueba que falle o un intento 3.

**Las decisiones del Owner,** literales en su registro: Q-PBT5-7 «Sí, con condiciones»; Q-PBT5-8 «Adaptación con revisión», que confirma la regla de reejecutar la CI; Q-PBT5-9 «Definición y reglas»; Q-PBT5-1 «Extender la dispensa»; Q-PBT5-10 «Hacerlas como en Smoke-2»; Q-PBT5-4 «Construirlos y rotularlos»; Q-PBT5-2 «Contar antes en otros». Q-PBT5-5 no se preguntó: la corrida previa no llegó a hacerse.

**Qué es PB-T5:** la CI exacta, el Smoke-PB (D-PB-09) y el gate-close de F4B. No cambia `src/`, `tests/` ni `assets/`.

**El contrato,** en la forma B: `{TASK}/coordinator/contract.md` (el §6 del paquete, sha256 `{sha(ctext.encode('utf-8'))}`).
- Sin `gate-contract.json`, sin Controller, sin nc1 a nc4 y sin D2.
- Antes de la firma, una revisión independiente de solo lectura de la evidencia.
- Los invariantes INV-PBT5-01 a 17 y las paradas PBT5-C01 a C17.
- La definición de intento y las tres reglas de Smoke-2.

**El Candidato:** `{cand}`, la punta del cierre de PB-T4e (F4B-E61). El producto es el GREEN `0eac7911`. El DLL se compila en el Candidato, antes de este commit documental, y no se vuelve a compilar.

**El Smoke-PB:** 65 pasos y 9 precondiciones, con la guía `F4B-DRAFTS/pbt5/smoke-run-v1.md`. Lleva los A/B y los cuatro contextos construidos rotulados CONSTRUCTED REPRESENTATIVE TEST CASES, las tres inyecciones con un AutoCAD por inyección y el recuento del estilo de cotas antes de SPB-07. La sesión lo opera solo si puede empezar antes de las 13:00 UTC; a las 13:00 UTC para en el siguiente límite de paso y espera al Owner.

**`attempts`:** 1 de 3.

**Siguiente:** el Smoke-PB contratado; después, su custodia, la revisión independiente de la evidencia, la disposición y la firma del Owner y el gate-close de F4B.
"""
    s292 = f"""
## 292. PB-T5: paquete v2.3, las decisiones del Owner, el contrato en forma B, la guía del Smoke-PB y la emisión (F4B-E62)

**Qué se custodia:**
- **el paquete de PB-T5** v2.3 (sha256 `{pkg['sha256']}`; redactado `{pkg['sha256_redactado']}`), en `docs/initiatives/I-64-f4b-pbt5-package.md`;
- **las decisiones del Owner,** literales, en `F4B-DRAFTS/coordinator/owner-pbt5-decisions.md`;
- **el contrato,** el §6 del paquete, en `{TASK}/coordinator/contract.md`;
- **en `F4B-DRAFTS/pbt5/`:** la guía del Smoke-PB contratado (`smoke-run-v1.md`); las versiones v1 a v2.3 del paquete y de las preguntas; la revisión independiente y sus recomprobaciones (`review-v1/`); las guías de la corrida previa, que no se hizo (`pre-run-tonight-v2.md` a `v2.4.md`), como historia; los traspasos; las herramientas (`_tools/`), sin las `*.local.txt`, que llevan los patrones de privacidad y no se custodian.

**La redacción:** la carpeta de trabajo del Smoke-PB pasa a sus marcadores (`<raiz-de-trabajo>`, `<carpeta-del-Smoke-PB>` y `<carpeta-de-respaldo>`), y cada archivo se comprueba con los patrones locales de privacidad.

**Fuera de la custodia, solo por su sha256:**
{so}

**La base:** el cierre de PB-T4e, F4B-E61 (`{c8}`), con la CI de push `{ci['run']}`, 4/4.

**Las comprobaciones:** la numeración (F4B-E61 y la 291 existen; F4B-E62 y la 292 no); el árbol limpio y la rama remota en el Candidato; la línea `BOUNDED_RESULT`; ningún destino que ya exista; ninguna ruta de más de {MAXREL} caracteres.

**Límites:** solo documentación; sin cambios en `src/` ni en `tests/`; sin AutoCAD.
"""
    return e62, s292


def yml_new(old):
    lines = old.split("\n")

    def find(prefix):
        idx = [i for i, ln in enumerate(lines) if ln.startswith(prefix)]
        if len(idx) != 1:
            stop(f"I-64.yml: {prefix!r} aparece {len(idx)} veces")
        return idx[0]

    def setval(prefix, value):
        i = find(prefix)
        lines[i] = prefix + '"' + value.replace('"', "'") + '"'

    def getval(prefix):
        ln = lines[find(prefix)]
        if not (ln.startswith(prefix + '"') and ln.endswith('"')):
            stop(f"I-64.yml: {prefix!r} no es una cadena entre comillas")
        return ln[len(prefix) + 1:-1]

    setval("  next_action: ", "F4B-PBT5-SMOKE-PB-GATE-CLOSE: emitida (F4B-E62; paquete v2.3; contrato en forma B: sin Controller, sin nc1 a nc4 y sin D2, con revision independiente de la evidencia antes de la firma; attempts 1 de 3); Candidato ebd304a3 (cierre de PB-T4e); siguiente: el Smoke-PB contratado con smoke-run-v1.md (la sesion solo si puede empezar antes de las 13:00 UTC; a las 13:00 UTC para y espera al Owner)")
    f4b = getval("  f4b_status: ")
    if "PB-T5 EMITIDA" in f4b:
        stop("f4b_status ya dice PB-T5 EMITIDA")
    setval("  f4b_status: ", f4b + "; PB-T5 EMITIDA (F4B-E62): contrato en forma B (adaptacion con revision de la evidencia, decision del Owner), attempts 1 de 3, Candidato ebd304a3")
    setval("    NEXT_ROLE: ", "Sesion principal: el Smoke-PB contratado de PB-T5 (F4B-E62)")
    na = getval("    NEXT_ACTION: ")
    mk = "; F5-A AUTHORIZED / WAITING FOR F4B"
    if na.count(mk) != 1:
        stop("NEXT_ACTION: no aparece una vez la marca de F5-A")
    setval("    NEXT_ACTION: ", "Operar el Smoke-PB contratado de PB-T5 con F4B-DRAFTS/pbt5/smoke-run-v1.md sobre el Candidato ebd304a3 (la sesion solo si puede empezar antes de las 13:00 UTC; a las 13:00 UTC para en el limite de paso y espera al Owner; los 9 S+O, los OBSERVADO y SPB-07 sin estilo, al Owner); despues, la custodia de la corrida, la revision independiente de la evidencia, la disposicion y la firma del Owner y el gate-close de F4B" + na[na.index(mk):])
    gate = getval("    GATE: ")
    gm = " y F4B-E61); F4 CLOSED"
    if gate.count(gm) != 1:
        stop("GATE: no aparece una vez la marca del cierre de PB-T4e")
    setval("    GATE: ", gate.replace(gm, " y F4B-E61); PB-T5 EMITIDA (F4B-E62); F4 CLOSED"))
    setval("    TASK: ", "F4B-PBT5-SMOKE-PB-GATE-CLOSE: la CI exacta del Candidato ebd304a3, el Smoke-PB contratado (smoke-run-v1.md), la revision independiente de la evidencia, la firma del Owner y el gate-close de F4B (paquete v2.3; contrato en forma B; attempts 1 de 3)")
    rb = getval("    REMAINING_BUDGET: ")
    rm = "la definicion de intento de PB-T5, con las reglas de Smoke-2, la decidio el Owner (F4B-E61) y la fija su emision"
    if rb.count(rm) != 1:
        stop("REMAINING_BUDGET: no aparece una vez la frase de PB-T5")
    setval("    REMAINING_BUDGET: ", rb.replace(rm, rm + " (F4B-E62: PB-T5 en el intento 1 de 3; un intento es una corrida del Smoke-PB sobre un Candidato)"))
    return "\n".join(lines)


def plan(a):
    pats = load_patterns(a.src)
    head = check_worktree(a.wt, a.cand)
    ci = check_ci(a.wt, a.cand, a.run)
    bounded = check_bounded(a.bounded)
    items = collect(a.src, a.out, a.wt, pats)
    ctext = contract_text(a.src)
    if sum(1 for rx in pats for _ in rx.finditer(ctext)):
        stop("el contrato casa un patron prohibido")
    return head, ci, bounded, items, ctext


def listing(head, ci, bounded, items, ctext):
    return {"task": TASK, "head": head, "ci": ci, "bounded": bounded, "contract_sha256": sha(ctext.encode("utf-8")),
            "items": items, "numeracion": {"disposicion": "F4B-E62", "evidencia": 292}}


def main():
    ap = argparse.ArgumentParser()
    g = ap.add_mutually_exclusive_group(required=True)
    g.add_argument("--dry", action="store_true")
    g.add_argument("--commit", action="store_true")
    for k in ("--wt", "--src", "--cand", "--run", "--bounded", "--out"):
        ap.add_argument(k, required=True)
    ap.add_argument("--dry-listing")
    a = ap.parse_args()
    for p in (a.wt, a.src, a.bounded):
        if not os.path.exists(p):
            stop(f"no existe {p}")
    os.makedirs(a.out, exist_ok=True)
    head, ci, bounded, items, ctext = plan(a)
    lst = listing(head, ci, bounded, items, ctext)
    body = json.dumps(lst, ensure_ascii=False, indent=1, sort_keys=True)
    if a.dry:
        p = os.path.join(a.out, "emit-pbt5-dry.json")
        if os.path.exists(p):
            stop(f"{p} ya existe")
        with open(p, "x", encoding="utf-8", newline="\n") as f:
            f.write(body)
        print(f"DRY OK: {len(items)} archivos ({sum(1 for i in items if i['modo'] == 'solo-sha256')} solo por sha256); "
              f"contrato {lst['contract_sha256'][:12]}; CI {ci['run']}; {bounded[:80]}")
        print(f"listado: {p} (sha256 {sha(body.encode('utf-8'))})")
        return
    if not a.dry_listing or not os.path.exists(a.dry_listing):
        stop("--commit exige --dry-listing, el listado del --dry")
    if open(a.dry_listing, encoding="utf-8", newline="").read() != body:
        stop("el plan no es el del --dry: algo cambio entre los dos")
    today = datetime.datetime.now(datetime.timezone.utc).strftime("%Y-%m-%d")
    e62, s292 = texts(a.cand, ci, bounded, items, ctext, today)
    written = []
    srcA = os.path.abspath(a.src)
    for it in items:
        if it["modo"] != "copia":
            continue
        data = open(os.path.join(srcA, it["rel"].replace("/", os.sep)), "rb").read()
        body_b = redact(data.decode("utf-8")).encode("utf-8")
        if sha(body_b) != it["sha256_redactado"]:
            stop("cambio un archivo entre el plan y la escritura: " + it["rel"])
        dst = os.path.join(a.wt, it["target"].replace("/", os.sep))
        os.makedirs(os.path.dirname(dst), exist_ok=True)
        with open(dst, "xb") as f:
            f.write(body_b)
        written.append(it["target"])
    cdst = os.path.join(a.wt, CONTRACT.replace("/", os.sep))
    os.makedirs(os.path.dirname(cdst), exist_ok=True)
    with open(cdst, "x", encoding="utf-8", newline="\n") as f:
        f.write(ctext)
    written.append(CONTRACT)
    for rel, add in ((DISP, e62), (EVID, s292)):
        p = os.path.join(a.wt, rel.replace("/", os.sep))
        old = open(p, encoding="utf-8").read()
        with open(p, "w", encoding="utf-8", newline="\n") as f:
            f.write(old.rstrip("\n") + "\n" + add)
        written.append(rel)
    yp = os.path.join(a.wt, YML.replace("/", os.sep))
    new = yml_new(open(yp, encoding="utf-8").read())
    with open(yp, "w", encoding="utf-8", newline="\n") as f:
        f.write(new)
    written.append(YML)
    git(a.wt, "add", "--", *written)
    st = git(a.wt, "status", "--porcelain").stdout.decode("utf-8").splitlines()
    paths = {ln[3:].strip().strip('"') for ln in st}
    extra = paths - set(written)
    if extra:
        stop("hay cambios fuera de lo previsto: " + ", ".join(sorted(extra))[:300])
    msg = ("I-64 F4B PB-T5: paquete v2.3, las decisiones del Owner, el contrato en forma B y la emision (F4B-E62)\n\n"
           f"Emision condicional de {TASK} por la decision del Owner Q-PBT5-7, con sus tres condiciones cumplidas:\n"
           f"la comprobacion acotada ({bounded[:120]}), ninguna decision nueva y la CI exacta del Candidato {a.cand[:8]}\n"
           f"en verde ({ci['run']}). Contrato en forma B (Q-PBT5-8), sin Controller, sin nc1 a nc4 y sin D2, con una\n"
           "revision independiente de la evidencia antes de la firma. attempts 1 de 3. Solo documentacion; sin cambios\n"
           "en src/ ni tests/; sin AutoCAD.\n\n"
           "Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>\n")
    mfile = os.path.join(a.out, "emit-pbt5-commit-msg.txt")
    with open(mfile, "x", encoding="utf-8", newline="\n") as f:
        f.write(msg)
    git(a.wt, "commit", "-F", mfile)
    new_head = git(a.wt, "rev-parse", "HEAD").stdout.decode().strip()
    if git(a.wt, "diff", "--name-only", a.cand, new_head, "--", "src", "tests", "assets").stdout.strip():
        stop("el commit toca src, tests o assets")
    git(a.wt, "push", "origin", "HEAD:refs/heads/" + BRANCH)
    print(f"COMMIT OK: {new_head} (F4B-E62 y la 292); {len(written)} rutas; subido sin forzar")


if __name__ == "__main__":
    main()
