"""I-62, decisiones §80, puntos 5 y 6: compuerta del Owner y de la autorización de la ÚNICA revisión formal de la ronda 2 de A-6 (preflight 11,
«11-OwnerAndAuthorizationGate», de launch.py). BLOQUEANTE. Adaptada de owner_gate.py del kit de la ronda 1 (R20261011T215735Z-c434).

§80, punto 5: «Ninguna revisión nueva sin disposición posterior»; §79, punto 7: «Ninguna ronda de revisión nueva sin disposición posterior».
launch.py se NIEGA a lanzar mientras esta compuerta no pase. Comprueba, en el worktree de I-62 en el momento del lanzamiento (solo lectura de git:
rev-parse, show, hash-object sin -w y merge-base, con GIT_OPTIONAL_LOCKS=0; nunca escribe en él):

  (a) la línea del Owner `A6-ADR0046-9-FX02 = A (…)` (decisiones §78; SHA-256 de la línea, UTF-8 sin salto, = A6_LINE_SHA) está en
      docs/automation/decisions/I-62.md, en la HEAD del worktree, como LÍNEA COMPLETA, byte a byte, en una línea POSTERIOR al encabezado de
      decisiones §78; ninguna otra línea empieza por «A6-ADR0046-9-FX02 =» (una revocación o una variante cuenta como problema); y la misma línea
      está, como línea completa, en la A-6 de la ronda 2 del commit del clon (§7: «Líneas exactas, una por consecuencia, sin cambio desde la
      ronda 1»);
  (b) la autorización de revisión vigente: la línea del Owner `CLAUDE-CLI-I62 = A (…)` (decisiones §56; SHA-256 = CLAUDE_LINE_SHA) existe como
      línea completa; ninguna otra línea empieza por «CLAUDE-CLI-I62 =»; y el SHA-256 medido del binario (preflight 3) = el del cierre;
  (c) docs/initiatives/I-62-A-6.md en la HEAD del worktree tiene el blob de la ronda 2 fijado en el kit (closure.ObjectBlob, 40 hexadecimales);
  (d) AUTORIZACIÓN (decisiones §79, punto 7; §80, punto 5): existe en decisiones (HEAD del worktree) una sección numerada DESPUÉS de §80 cuyo
      encabezado es el de una disposición del Coordinator («## <n>. Disposición del Coordinator…», n > 80) y cuyo cuerpo (hasta el encabezado «## »
      siguiente) contiene, como tokens literales, el blob completo de la ronda 2 (40 hexadecimales, sin abreviar) y el RunId de este kit. Hoy no
      existe: la preflight en seco se bloquea aquí, como debe. La compuerta no interpreta el texto: la sesión principal comprueba, antes de lanzar,
      que la sección que la compuerta encuentra (AuthorizationSections) es la disposición que autoriza esta revisión;
  además, para que (a)-(d) sean el registro de decisiones que continúa al del clon: el archivo del árbol de trabajo es el de la HEAD (sin cambios sin
  commit: `git hash-object` con los filtros del repositorio = el blob de HEAD), la HEAD desciende del commit del clon y el texto de decisiones del
  commit del clon es prefijo exacto del de la HEAD (solo añadido: §78, §79, §80 y lo anterior, intactos).

evaluate() es pura (la autoprueba la ejerce con textos sintéticos); check() reúne sus entradas por git y la llama. Cada problema es una línea con
probs.append; Ok = sin problemas. El resultado registra los SHA-256 fijados, los números de línea y los encabezados encontrados, nunca rutas con el
nombre de usuario.
"""
import hashlib
import os
import re
import subprocess

CHECK = "11-OwnerAndAuthorizationGate"
DECISIONS = "docs/automation/decisions/I-62.md"
A6_PATH = "docs/initiatives/I-62-A-6.md"
A6R1_BLOB = "eac473c0d2e58cf1c4594f48a8816b3833af6509"          # ronda 1 (decisiones §77, punto 1); la ronda 2 es otro blob
A6_LINE_SHA = "83152eae4f451961b9dc43920aa60c6f0910d4d023e9e93d8f5d6160e7ac8b14"
A6_LINE_PREFIX = "A6-ADR0046-9-FX02 ="
CLAUDE_LINE_SHA = "fbeade41f74d6694a621fe6294d9568d7b76b124b1fd78e415514582c5d6ee79"
CLAUDE_LINE_PREFIX = "CLAUDE-CLI-I62 ="
SECTION_78 = "## 78. "
SECTION_80 = "## 80. "
AFTER = 80
AUTH_HEADING_RX = re.compile(r"^## ([0-9]+)\. Disposición del Coordinator\b")
SECTION_RX = re.compile(r"^## ")
RUN_ID = "R20261012T195511Z-e5c3"                                # RunId del kit (generado una vez en la fase 1)
WORKTREE_DISPLAY = "%USERPROFILE%\\.codex\\worktrees\\architecture-portabilidad-coordinador-principal"


def line_sha(s):
    return hashlib.sha256(s.encode("utf-8")).hexdigest()


def token_in(text, token, alphabet):
    """`token` aparece en `text` como token literal: ni precedido ni seguido de un carácter de `alphabet`."""
    return bool(token) and re.search(r"(?<![%s])%s(?![%s])" % (alphabet, re.escape(token), alphabet), text) is not None


def authorization_sections(lines, blob, run_id):
    """→ lista de {Number, Line, Heading, HasBlob, HasRunId} de las disposiciones del Coordinator numeradas después de §80."""
    out = []
    for i, l in enumerate(lines):
        m = AUTH_HEADING_RX.match(l)
        if not m or int(m.group(1)) <= AFTER:
            continue
        end = next((j for j in range(i + 1, len(lines)) if SECTION_RX.match(lines[j])), len(lines))
        body = "\n".join(lines[i:end])
        out.append({"Number": int(m.group(1)), "Line": i + 1, "Heading": l[:160],
                    "HasBlob": token_in(body, blob or "", "0-9A-Fa-f"), "HasRunId": token_in(body, run_id or "", "0-9A-Za-z_-")})
    return out


def evaluate(dec_head, dec_clone, worktree_clean, head_descends, measured_bin_sha, closure_bin_sha, a6r2_clone_text, a6_blob_at_head,
             a6r2_blob, run_id):
    """→ dict con Ok y Problems. dec_head / dec_clone: textos de decisiones (HEAD del worktree / commit del clon), tal como los da `git show`;
    a6r2_clone_text: la A-6 de la ronda 2 en el commit del clon; a6_blob_at_head: blob de A-6 en la HEAD del worktree; a6r2_blob y run_id: los
    fijados en el kit (closure.ObjectBlob y closure.Ids.RunId)."""
    probs = []
    lines = (dec_head or "").split("\n")
    s78 = [i for i, l in enumerate(lines) if l.startswith(SECTION_78)]
    s80 = [i for i, l in enumerate(lines) if l.startswith(SECTION_80)]
    a6_hits = [i for i, l in enumerate(lines) if line_sha(l) == A6_LINE_SHA]
    claude_hits = [i for i, l in enumerate(lines) if line_sha(l) == CLAUDE_LINE_SHA]
    other_a6 = [i for i, l in enumerate(lines) if l.lstrip().startswith(A6_LINE_PREFIX) and line_sha(l) != A6_LINE_SHA]
    other_claude = [i for i, l in enumerate(lines) if l.lstrip().startswith(CLAUDE_LINE_PREFIX) and line_sha(l) != CLAUDE_LINE_SHA]
    near = [i for i, l in enumerate(lines) if "A6-ADR0046-9-FX02 = A" in l and line_sha(l) != A6_LINE_SHA]
    blob_ok = bool(re.match(r"^[0-9a-f]{40}$", a6r2_blob or "")) and a6r2_blob != A6R1_BLOB
    pin_ok = any(line_sha(l) == A6_LINE_SHA for l in (a6r2_clone_text or "").split("\n"))
    auth = authorization_sections(lines, a6r2_blob if blob_ok else "", run_id)
    if not blob_ok:
        probs.append("blob de la A-6 de la ronda 2 no fijado en el kit (closure.ObjectBlob: 40 hexadecimales, distinto de la ronda 1)")
    if not pin_ok:
        probs.append("la línea fijada del Owner A6-ADR0046-9-FX02 no está, como línea completa, en la A-6 de la ronda 2 del commit del clon (§7)")
    if len(s78) != 1:
        probs.append("decisiones sin un único encabezado §78 (%d)" % len(s78))
    if not a6_hits:
        probs.append("falta, como línea completa de decisiones, la línea del Owner A6-ADR0046-9-FX02 de §78 (SHA-256 %s…)" % A6_LINE_SHA[:8])
    elif len(s78) == 1 and not any(i > s78[0] for i in a6_hits):
        probs.append("la línea del Owner A6-ADR0046-9-FX02 no está registrada después del encabezado de decisiones §78")
    if other_a6:
        probs.append("otra línea que empieza por «%s» distinta de la fijada (líneas %s)" % (A6_LINE_PREFIX, [i + 1 for i in other_a6][:5]))
    if not claude_hits:
        probs.append("falta, como línea completa de decisiones, la línea del Owner CLAUDE-CLI-I62 = A (SHA-256 %s…): autorización de revisión "
                     "no vigente" % CLAUDE_LINE_SHA[:8])
    if other_claude:
        probs.append("otra línea que empieza por «%s» distinta de la fijada (líneas %s): autorización de revisión no verificable"
                     % (CLAUDE_LINE_PREFIX, [i + 1 for i in other_claude][:5]))
    if not blob_ok or a6_blob_at_head != a6r2_blob:
        probs.append("docs/initiatives/I-62-A-6.md en la HEAD del worktree no tiene el blob de la ronda 2 fijado en el kit (%s)"
                     % (a6_blob_at_head or "ausente")[:12])
    if len(s80) != 1:
        probs.append("decisiones sin un único encabezado §80 (%d)" % len(s80))
    if not any(a["HasBlob"] and a["HasRunId"] for a in auth):
        probs.append("falta la autorización: ninguna disposición del Coordinator numerada después de §80 contiene, como tokens literales, el "
                     "blob completo de la ronda 2 y el RunId %s del kit (decisiones §79, punto 7; §80, punto 5)" % (run_id or "?"))
    append_only = bool(dec_clone) and (dec_head or "").startswith(dec_clone)
    if not append_only:
        probs.append("decisiones del worktree no crece solo por añadido desde el commit del clon (§80 o lo anterior cambiaron)")
    if worktree_clean is not True:
        probs.append("el archivo de decisiones del árbol de trabajo no es el de la HEAD (cambios sin commit)")
    if head_descends is not True:
        probs.append("la HEAD del worktree no desciende del commit del clon")
    bin_ok = bool(measured_bin_sha) and measured_bin_sha == closure_bin_sha
    if not bin_ok:
        probs.append("SHA-256 medido del binario distinto del cierre: la autorización de revisión no cubre otro binario")
    return {"Ok": not probs, "Problems": probs, "A6LineSha256": A6_LINE_SHA, "A6LineNumbers": [i + 1 for i in a6_hits],
            "Section78Line": (s78[0] + 1) if len(s78) == 1 else None, "Section80Line": (s80[0] + 1) if len(s80) == 1 else None,
            "NearMissLines": [i + 1 for i in near][:10], "ClaudeCliLineSha256": CLAUDE_LINE_SHA, "ClaudeCliLineNumbers": [i + 1 for i in claude_hits],
            "A6R2Blob": a6r2_blob, "A6BlobAtHead": a6_blob_at_head, "A6BlobAtHeadMatches": blob_ok and a6_blob_at_head == a6r2_blob,
            "PinnedLineInA6R2": pin_ok, "RunId": run_id, "AuthorizationSections": auth,
            "Authorized": any(a["HasBlob"] and a["HasRunId"] for a in auth),
            "DecisionsAppendOnlySinceClone": append_only, "WorktreeDecisionsClean": worktree_clean is True,
            "HeadDescendsFromClone": head_descends is True, "BinarySha256MatchesClosure": bin_ok}


def _git(repo, *a):
    r = subprocess.run(["git", "-C", repo] + list(a), capture_output=True, env=dict(os.environ, GIT_OPTIONAL_LOCKS="0"))
    return r.returncode, r.stdout


def check(worktree, clone, clone_commit, closure, measured_bin_sha):
    """Reúne las entradas por git (solo lectura) y evalúa. worktree: el de I-62; clone: el clon de lectura (su commit da el texto de referencia)."""
    rc, head = _git(worktree, "rev-parse", "HEAD")
    rc1, dec_head = _git(worktree, "show", "HEAD:" + DECISIONS)
    rc2, dec_clone = _git(clone, "show", clone_commit + ":" + DECISIONS)
    rc3, blob_head = _git(worktree, "rev-parse", "HEAD:" + DECISIONS)
    rc4, blob_wt = _git(worktree, "hash-object", "--", DECISIONS)
    rc5, _ = _git(worktree, "merge-base", "--is-ancestor", clone_commit, "HEAD")
    rc6, a6_text = _git(clone, "show", clone_commit + ":" + A6_PATH)
    rc7, a6_head = _git(worktree, "rev-parse", "HEAD:" + A6_PATH)
    clean = rc3 == 0 and rc4 == 0 and blob_head.strip() == blob_wt.strip()
    out = evaluate(dec_head.decode("utf-8") if rc1 == 0 else "", dec_clone.decode("utf-8") if rc2 == 0 else "", clean, rc5 == 0,
                   measured_bin_sha, closure["Transport"]["Binary"]["Sha256"], a6_text.decode("utf-8") if rc6 == 0 else None,
                   a6_head.decode().strip() if rc7 == 0 else None, closure.get("ObjectBlob"), (closure.get("Ids") or {}).get("RunId"))
    out.update({"Worktree": WORKTREE_DISPLAY, "WorktreeHead": head.decode().strip() if rc == 0 else None,
                "DecisionsBlobAtHead": blob_head.decode().strip() if rc3 == 0 else None, "CloneCommit": clone_commit,
                "Reads": "git rev-parse, show, hash-object (sin -w) y merge-base, con GIT_OPTIONAL_LOCKS=0; nada se escribe"})
    return out
