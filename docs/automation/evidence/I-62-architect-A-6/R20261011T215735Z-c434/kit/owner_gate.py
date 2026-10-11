"""I-62, decisiones §77, punto 4: compuerta del Owner de la revisión formal de A-6 (preflight 11, «11-OwnerGate», de launch.py). BLOQUEANTE.

§77, punto 4: «La revisión formal podrá lanzarse únicamente cuando conste una disposición afirmativa del Owner sobre ADR-0046 #9, y se hayan
verificado las autorizaciones de revisión vigentes». launch.py se NIEGA a lanzar mientras esta compuerta no pase. Comprueba, en el worktree de
I-62 en el momento del lanzamiento (solo lectura de git: rev-parse, show, hash-object sin -w y merge-base; nunca escribe en él):

  (a) disposición afirmativa del Owner: docs/automation/decisions/I-62.md, en la HEAD del worktree, contiene como LÍNEA COMPLETA, byte a byte,
      la línea del Owner `A6-ADR0046-9-FX02 = A (…)` de A-6 §7 (a) (A-6 L679, blob eac473c0; la misma que la línea 22 de la tarjeta del Owner),
      con SHA-256 de la línea (UTF-8, sin salto) = A6_LINE_SHA, en una línea POSTERIOR al encabezado de decisiones §77. No vale una paráfrasis, un
      fragmento citado dentro de otra línea (tabla, comillas, cita «> »), una línea con espacios o CR añadidos, ni la línea de la tarjeta (la
      compuerta solo lee el archivo de decisiones). Ninguna otra línea puede empezar por «A6-ADR0046-9-FX02 =» (una revocación o una variante
      cuenta como problema);
  (b) autorización de revisión vigente: la línea del Owner `CLAUDE-CLI-I62 = A (…)` (decisiones §56; SHA-256 de la línea = CLAUDE_LINE_SHA)
      existe como línea completa; ninguna otra línea empieza por «CLAUDE-CLI-I62 =»; y el SHA-256 medido del binario (preflight 3) = el del
      cierre;
  además, para que (a) y (b) sean el registro de decisiones que continúa al del clon: el archivo del árbol de trabajo es el de la HEAD (sin
  cambios sin commit: `git hash-object` con los filtros del repositorio = el blob de HEAD), la HEAD desciende del commit del clon, el texto de
  decisiones del clon (commit del clon) es prefijo exacto del de la HEAD (solo añadido: §77 y lo anterior, intactos) y la línea fijada es la
  L679 de A-6 en el commit del clon.

evaluate() es pura (la autoprueba la ejerce con textos sintéticos); check() reúne sus entradas por git y la llama. Cada problema es una línea con
probs.append; Ok = sin problemas. El resultado registra los SHA-256 fijados y los números de línea, nunca rutas con el nombre de usuario.
"""
import hashlib
import os
import subprocess

CHECK = "11-OwnerGate"
DECISIONS = "docs/automation/decisions/I-62.md"
A6_PATH, A6_BLOB, A6_LINE_NO = "docs/initiatives/I-62-A-6.md", "eac473c0d2e58cf1c4594f48a8816b3833af6509", 679
CARD_PATH, CARD_LINE_NO = "docs/automation/evidence/I-62-F6/requests/A6-ADR0046-9-FX02-owner-card-2026-10-11.md", 22
A6_LINE_SHA = "83152eae4f451961b9dc43920aa60c6f0910d4d023e9e93d8f5d6160e7ac8b14"
A6_LINE_PREFIX = "A6-ADR0046-9-FX02 ="
CLAUDE_LINE_SHA = "fbeade41f74d6694a621fe6294d9568d7b76b124b1fd78e415514582c5d6ee79"
CLAUDE_LINE_PREFIX = "CLAUDE-CLI-I62 ="
SECTION_77 = "## 77. "
WORKTREE_DISPLAY = "%USERPROFILE%\\.codex\\worktrees\\architecture-portabilidad-coordinador-principal"


def line_sha(s):
    return hashlib.sha256(s.encode("utf-8")).hexdigest()


def evaluate(dec_head, dec_clone, worktree_clean, head_descends, measured_bin_sha, closure_bin_sha, a6_source_line):
    """→ dict con Ok y Problems. dec_head / dec_clone: textos de decisiones (HEAD del worktree / commit del clon), tal como los da `git show`;
    a6_source_line: la línea L679 de A-6 en el commit del clon (la fuente de la línea fijada)."""
    probs = []
    lines = (dec_head or "").split("\n")
    s77 = [i for i, l in enumerate(lines) if l.startswith(SECTION_77)]
    a6_hits = [i for i, l in enumerate(lines) if line_sha(l) == A6_LINE_SHA]
    claude_hits = [i for i, l in enumerate(lines) if line_sha(l) == CLAUDE_LINE_SHA]
    other_a6 = [i for i, l in enumerate(lines) if l.lstrip().startswith(A6_LINE_PREFIX) and line_sha(l) != A6_LINE_SHA]
    other_claude = [i for i, l in enumerate(lines) if l.lstrip().startswith(CLAUDE_LINE_PREFIX) and line_sha(l) != CLAUDE_LINE_SHA]
    near = [i for i, l in enumerate(lines) if "A6-ADR0046-9-FX02 = A" in l and line_sha(l) != A6_LINE_SHA]
    pin_ok = a6_source_line is not None and line_sha(a6_source_line) == A6_LINE_SHA
    if not pin_ok:
        probs.append("la línea fijada no es la de A-6 §7 (a) en el commit del clon (L%d)" % A6_LINE_NO)
    if len(s77) != 1:
        probs.append("decisiones sin un único encabezado §77 (%d)" % len(s77))
    if not a6_hits:
        probs.append("falta, como línea completa de decisiones, la línea del Owner A6-ADR0046-9-FX02 de A-6 §7 (a) (SHA-256 %s…): "
                     "sin disposición afirmativa del Owner sobre ADR-0046 #9 no se lanza" % A6_LINE_SHA[:8])
    elif len(s77) == 1 and not any(i > s77[0] for i in a6_hits):
        probs.append("la línea del Owner A6-ADR0046-9-FX02 no está registrada después de decisiones §77")
    if other_a6:
        probs.append("otra línea que empieza por «%s» distinta de la fijada (líneas %s)" % (A6_LINE_PREFIX, [i + 1 for i in other_a6][:5]))
    if not claude_hits:
        probs.append("falta, como línea completa de decisiones, la línea del Owner CLAUDE-CLI-I62 = A (SHA-256 %s…): autorización de revisión "
                     "no vigente" % CLAUDE_LINE_SHA[:8])
    if other_claude:
        probs.append("otra línea que empieza por «%s» distinta de la fijada (líneas %s): autorización de revisión no verificable"
                     % (CLAUDE_LINE_PREFIX, [i + 1 for i in other_claude][:5]))
    append_only = bool(dec_clone) and (dec_head or "").startswith(dec_clone)
    if not append_only:
        probs.append("decisiones del worktree no crece solo por añadido desde el commit del clon (§77 o lo anterior cambiaron)")
    if worktree_clean is not True:
        probs.append("el archivo de decisiones del árbol de trabajo no es el de la HEAD (cambios sin commit)")
    if head_descends is not True:
        probs.append("la HEAD del worktree no desciende del commit del clon")
    bin_ok = bool(measured_bin_sha) and measured_bin_sha == closure_bin_sha
    if not bin_ok:
        probs.append("SHA-256 medido del binario distinto del cierre: la autorización de revisión no cubre otro binario")
    return {"Ok": not probs, "Problems": probs, "A6LineSha256": A6_LINE_SHA, "A6LineNumbers": [i + 1 for i in a6_hits],
            "Section77Line": (s77[0] + 1) if len(s77) == 1 else None, "NearMissLines": [i + 1 for i in near][:10],
            "ClaudeCliLineSha256": CLAUDE_LINE_SHA, "ClaudeCliLineNumbers": [i + 1 for i in claude_hits],
            "DecisionsAppendOnlySinceClone": append_only, "WorktreeDecisionsClean": worktree_clean is True,
            "HeadDescendsFromClone": head_descends is True, "BinarySha256MatchesClosure": bin_ok, "PinnedLineIsA6L679": pin_ok}


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
    a6_lines = a6_text.decode("utf-8").split("\n") if rc6 == 0 else []
    clean = rc3 == 0 and rc4 == 0 and blob_head.strip() == blob_wt.strip()
    out = evaluate(dec_head.decode("utf-8") if rc1 == 0 else "", dec_clone.decode("utf-8") if rc2 == 0 else "", clean, rc5 == 0,
                   measured_bin_sha, closure["Transport"]["Binary"]["Sha256"], a6_lines[A6_LINE_NO - 1] if len(a6_lines) >= A6_LINE_NO else None)
    out.update({"Worktree": WORKTREE_DISPLAY, "WorktreeHead": head.decode().strip() if rc == 0 else None,
                "DecisionsBlobAtHead": blob_head.decode().strip() if rc3 == 0 else None, "CloneCommit": clone_commit,
                "Reads": "git rev-parse, show, hash-object (sin -w) y merge-base, con GIT_OPTIONAL_LOCKS=0; nada se escribe"})
    return out
