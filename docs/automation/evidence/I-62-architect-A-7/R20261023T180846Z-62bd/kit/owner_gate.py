"""I-62, decisiones §96: compuerta del Owner y de la autorización de la ÚNICA revisión formal de la RONDA 3 de la enmienda A-7 (preflight 11,
«11-OwnerAndAuthorizationGate», de launch.py). BLOQUEANTE. Adaptada de owner_gate.py del kit de A-7 ronda 2 (R20261019T201030Z-d1bf, a su vez
del de la ronda 1 y del de A-6 ronda 4): las funciones de sección, token, autorización y secciones posteriores, byte a byte; cambian el objeto
(A-7 ronda 3, blob 8bcccb2f…), la sección base (§96) y la autorización, que queda PENDIENTE en el kit entregado (AUTH_SECTION, AUTH_HEADING y
AUTH_TEXT_SHA en None).

§96, punto 7: «Preparar el kit acotado, pero no lanzar una nueva revisión formal sin disposición posterior». La compuerta bloquea, con el único
motivo PENDING_REASON, hasta que una disposición del Coordinator POSTERIOR a §96 que nombre esta
revisión se fije en este módulo (pin_authorization.py) y el kit se vuelva a congelar.
launch.py se NIEGA a lanzar mientras esta compuerta no pase. Comprueba, en el worktree de I-62 en el momento del lanzamiento (solo lectura de git:
rev-parse, show, hash-object sin -w y merge-base, con GIT_OPTIONAL_LOCKS=0; nunca escribe en él):

  (a) la línea del Owner `A6-ADR0046-9-FX02 = A (…)` (decisiones §78; SHA-256 de la línea, UTF-8 sin salto, = A6_LINE_SHA) está en
      docs/automation/decisions/I-62.md, en la HEAD del worktree, como LÍNEA COMPLETA, byte a byte, en una línea POSTERIOR al encabezado de
      decisiones §78; ninguna otra línea empieza por «A6-ADR0046-9-FX02 =» (una revocación o una variante cuenta como problema); y la misma línea
      está, como línea completa, en la A-6 acordada del commit del clon (A-7 declara que las líneas (a) y (b) de A-6 §7 no cambian);
  (b) la autorización de revisión vigente: la línea del Owner `CLAUDE-CLI-I62 = A (…)` (decisiones §56; SHA-256 = CLAUDE_LINE_SHA) existe como
      línea completa; ninguna otra línea empieza por «CLAUDE-CLI-I62 =»; y el SHA-256 medido del binario (preflight 3) = el del cierre;
  (c) docs/initiatives/I-62-A-7.md en la HEAD del worktree tiene el blob fijado OBJECT_BLOB (= closure.ObjectBlob; la ronda 3 de §96) y
      docs/initiatives/I-62-A-6.md tiene el blob acordado A6_AGREED_BLOB (decisiones §90; §96, punto 3: «No modificar A-6»);
  (d) AUTORIZACIÓN: una disposición del Coordinator POSTERIOR a §96 (BASE_SECTION: la disposición que pidió la tercera candidata y prohibió la
      revisión formal sin una disposición nueva). Se fija en este módulo con AUTH_SECTION (su número), AUTH_HEADING (su encabezado, byte a byte) y
      AUTH_TEXT_SHA (el SHA-256 de su texto), que pin_authorization.py calcula sobre el archivo de decisiones. Mientras falte cualquiera de las
      tres, la compuerta BLOQUEA con PENDING_REASON y no evalúa ninguna sección. Con las tres fijadas: AUTH_SECTION posterior a §96; EXACTAMENTE
      una sección «## <AUTH_SECTION>. »; su encabezado es, byte a byte, AUTH_HEADING; su texto contiene, como token literal, el blob completo de
      A-7; y el SHA-256 de su texto es AUTH_TEXT_SHA. Texto de una sección: sus líneas desde el encabezado hasta antes del encabezado «## »
      siguiente (o el final), sin las líneas en blanco finales, unidas con LF y en UTF-8. En este kit, §96 es la última sección del commit del clon
      y la autorización tiene que ser una sección posterior, todavía inexistente;
  (e) con la autorización fijada, NINGUNA sección posterior: un encabezado «## <n>. » con n > AUTH_SECTION bloquea la compuerta y se lista en
      LaterSections (una disposición posterior tiene que revisarse antes de lanzar, aunque no nombre esta revisión); mientras está pendiente,
      LaterSections queda vacío y SectionsAfterBase lista, solo como información, las secciones posteriores a §96;
  además, para que (a)-(e) sean el registro de decisiones que continúa al del clon: el archivo del árbol de trabajo es el de la HEAD (sin cambios sin
  commit: `git hash-object` con los filtros del repositorio = el blob de HEAD), la HEAD desciende del commit del clon y el texto de decisiones del
  commit del clon es prefijo exacto del de la HEAD (solo añadido: §78, §92-§96 y lo anterior, intactos).

evaluate() es pura (la autoprueba la ejerce con textos sintéticos, en los dos estados, con su argumento auth); check() reúne sus entradas por git y
la llama con la autorización del módulo. Cada problema es una línea con probs.append; Ok = sin problemas. El resultado registra los SHA-256
fijados, los números de línea y los encabezados encontrados, nunca rutas con el nombre de usuario.
"""
import hashlib
import os
import re
import subprocess

CHECK = "11-OwnerAndAuthorizationGate"
DECISIONS = "docs/automation/decisions/I-62.md"
OBJECT_PATH = "docs/initiatives/I-62-A-7.md"
OBJECT_BLOB = "8bcccb2fd2cea480f73ee5bf5a1583e0c9eaf1ff"         # A-7, ronda 3 (publicada en 37ac6ca4; registrada en b9bfae50; decisiones §96)
A6_PATH = "docs/initiatives/I-62-A-6.md"
A6_AGREED_BLOB = "4bc919dd4768ab81abc74b70d3a4a3821c07feb5"      # A-6 AGREED (decisiones §90); A-7 no la modifica
A6_LINE_SHA = "83152eae4f451961b9dc43920aa60c6f0910d4d023e9e93d8f5d6160e7ac8b14"
A6_LINE_PREFIX = "A6-ADR0046-9-FX02 ="
CLAUDE_LINE_SHA = "fbeade41f74d6694a621fe6294d9568d7b76b124b1fd78e415514582c5d6ee79"
CLAUDE_LINE_PREFIX = "CLAUDE-CLI-I62 ="
SECTION_78 = "## 78. "
BASE_SECTION = 96                                                # decisiones §96: tercera candidata; «no lanzar una nueva revisión formal sin disposición posterior»
AUTH_SECTION = 97   # decisiones §97: disposición del Coordinator que autoriza el lanzamiento (pin_authorization.py)
AUTH_HEADING = "## 97. Disposición del Coordinator: revisión formal acotada de A-7 ronda 3 (2026-10-24 UTC)"
AUTH_TEXT_SHA = "df12f445c8a6d812c386eff2d009e3a2ef6c4e60b528b0c318c91dc97ca9db6e"   # SHA-256 del texto de §97 (pin_authorization.py)
PENDING_REASON = ("authorization pending: a Coordinator disposition after §96 must be pinned (section number, heading, text SHA-256) and the kit "
                  "re-frozen")
NUMBERED_RX = re.compile(r"^## ([0-9]+)\. ")
SECTION_RX = re.compile(r"^## ")
RUN_ID = "R20261023T180846Z-62bd"                                # RunId del kit (generado una vez; prep/ids.txt)
WORKTREE_DISPLAY = "%USERPROFILE%\\.codex\\worktrees\\architecture-portabilidad-coordinador-principal"


def line_sha(s):
    return hashlib.sha256(s.encode("utf-8")).hexdigest()


def token_in(text, token, alphabet):
    """`token` aparece en `text` como token literal: ni precedido ni seguido de un carácter de `alphabet`."""
    return bool(token) and re.search(r"(?<![%s])%s(?![%s])" % (alphabet, re.escape(token), alphabet), text) is not None


def section_text(lines, i):
    """Texto de la sección cuyo encabezado es lines[i]: hasta antes del encabezado «## » siguiente (o el final), sin las líneas en blanco finales,
    unido con LF."""
    end = next((j for j in range(i + 1, len(lines)) if SECTION_RX.match(lines[j])), len(lines))
    seg = lines[i:end]
    while seg and seg[-1].strip() == "":
        seg = seg[:-1]
    return "\n".join(seg)


def pinned_authorization():
    """→ {Section, Heading, TextSha256}: la autorización fijada en el módulo (las tres en None mientras está pendiente)."""
    return {"Section": AUTH_SECTION, "Heading": AUTH_HEADING, "TextSha256": AUTH_TEXT_SHA}


def authorization_pending(auth):
    """La autorización está pendiente si falta cualquiera de los tres valores fijados (o si no es un registro)."""
    return not isinstance(auth, dict) or any(auth.get(k) is None for k in ("Section", "Heading", "TextSha256"))


def authorization_sections(lines, blob, auth=None):
    """→ lista de {Number, Line, Heading, HeadingMatches, HasBlob, TextSha256, TextSha256Matches} de las secciones «## <Section>. » de la
    autorización fijada (auth; por defecto, la del módulo); vacía mientras está pendiente."""
    a = pinned_authorization() if auth is None else auth
    if authorization_pending(a):
        return []
    out = []
    for i, l in enumerate(lines):
        m = NUMBERED_RX.match(l)
        if not m or int(m.group(1)) != a["Section"]:
            continue
        text = section_text(lines, i)
        s = hashlib.sha256(text.encode("utf-8")).hexdigest()
        out.append({"Number": a["Section"], "Line": i + 1, "Heading": l[:160], "HeadingMatches": l == a["Heading"],
                    "HasBlob": token_in(text, blob or "", "0-9A-Fa-f"), "TextSha256": s, "TextSha256Matches": s == a["TextSha256"]})
    return out


def later_sections(lines, after):
    """→ lista de {Number, Line, Heading} de las secciones numeradas después de §<after>."""
    out = []
    for i, l in enumerate(lines):
        m = NUMBERED_RX.match(l)
        if m and int(m.group(1)) > after:
            out.append({"Number": int(m.group(1)), "Line": i + 1, "Heading": l[:160]})
    return out


def evaluate(dec_head, dec_clone, worktree_clean, head_descends, measured_bin_sha, closure_bin_sha, a6_clone_text, object_blob_at_head,
             object_blob, run_id, a6_blob_at_head=None, auth=None):
    """→ dict con Ok y Problems. dec_head / dec_clone: textos de decisiones (HEAD del worktree / commit del clon), tal como los da `git show`;
    a6_clone_text: la A-6 acordada en el commit del clon; object_blob_at_head: blob de A-7 en la HEAD del worktree; object_blob y run_id: los
    fijados en el kit (closure.ObjectBlob y closure.Ids.RunId); a6_blob_at_head: blob de A-6 en la HEAD del worktree; auth: la autorización
    fijada {Section, Heading, TextSha256} (por defecto, la del módulo: pinned_authorization(); pendiente si falta cualquiera de los tres valores)."""
    a = pinned_authorization() if auth is None else auth
    pending = authorization_pending(a)
    probs = []
    lines = (dec_head or "").split("\n")
    s78 = [i for i, l in enumerate(lines) if l.startswith(SECTION_78)]
    a6_hits = [i for i, l in enumerate(lines) if line_sha(l) == A6_LINE_SHA]
    claude_hits = [i for i, l in enumerate(lines) if line_sha(l) == CLAUDE_LINE_SHA]
    other_a6 = [i for i, l in enumerate(lines) if l.lstrip().startswith(A6_LINE_PREFIX) and line_sha(l) != A6_LINE_SHA]
    other_claude = [i for i, l in enumerate(lines) if l.lstrip().startswith(CLAUDE_LINE_PREFIX) and line_sha(l) != CLAUDE_LINE_SHA]
    near = [i for i, l in enumerate(lines) if "A6-ADR0046-9-FX02 = A" in l and line_sha(l) != A6_LINE_SHA]
    blob_ok = bool(re.match(r"^[0-9a-f]{40}$", object_blob or "")) and object_blob == OBJECT_BLOB
    pin_ok = any(line_sha(l) == A6_LINE_SHA for l in (a6_clone_text or "").split("\n"))
    auth = authorization_sections(lines, object_blob if blob_ok else "", a)
    later = later_sections(lines, a["Section"]) if not pending and type(a["Section"]) is int else []
    after_base = later_sections(lines, BASE_SECTION)
    authorized = not pending and len(auth) == 1 and auth[0]["HeadingMatches"] and auth[0]["HasBlob"] and auth[0]["TextSha256Matches"]
    a6_ok = a6_blob_at_head == A6_AGREED_BLOB
    if not blob_ok:
        probs.append("blob de A-7 no fijado en el kit (closure.ObjectBlob: 40 hexadecimales, igual al blob fijado %s…)" % OBJECT_BLOB[:8])
    if not pin_ok:
        probs.append("la línea fijada del Owner A6-ADR0046-9-FX02 no está, como línea completa, en la A-6 acordada del commit del clon (A-6 §7)")
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
    if not blob_ok or object_blob_at_head != object_blob:
        probs.append("docs/initiatives/I-62-A-7.md en la HEAD del worktree no tiene el blob de A-7 fijado en el kit (%s)"
                     % (object_blob_at_head or "ausente")[:12])
    if not a6_ok:
        probs.append("docs/initiatives/I-62-A-6.md en la HEAD del worktree no tiene el blob acordado %s… (decisiones §90; §96, punto 3) (%s)"
                     % (A6_AGREED_BLOB[:8], (a6_blob_at_head or "ausente")[:12]))
    if pending:
        probs.append(PENDING_REASON)
    else:
        if not (type(a["Section"]) is int and a["Section"] > BASE_SECTION):
            probs.append("la autorización fijada (§%s) no es una sección posterior a §%d (decisiones §96, punto 7)" % (a["Section"], BASE_SECTION))
        if len(auth) != 1:
            probs.append("decisiones sin un único encabezado §%s (%d)" % (a["Section"], len(auth)))
        if not authorized:
            probs.append("falta la autorización: la sección §%s de decisiones no tiene, a la vez, el encabezado fijado, el blob completo de A-7 "
                         "como token literal y el SHA-256 de su texto fijado en el kit (%s…) (decisiones §96, punto 7)"
                         % (a["Section"], str(a["TextSha256"])[:8]))
        if later:
            probs.append("sección posterior a §%s en decisiones (%s): una disposición posterior tiene que revisarse antes de lanzar"
                         % (a["Section"], ", ".join("§%d en la línea %d" % (x["Number"], x["Line"]) for x in later[:5])))
    append_only = bool(dec_clone) and (dec_head or "").startswith(dec_clone)
    if not append_only:
        probs.append("decisiones del worktree no crece solo por añadido desde el commit del clon (§96 o lo anterior cambiaron)")
    if worktree_clean is not True:
        probs.append("el archivo de decisiones del árbol de trabajo no es el de la HEAD (cambios sin commit)")
    if head_descends is not True:
        probs.append("la HEAD del worktree no desciende del commit del clon")
    bin_ok = bool(measured_bin_sha) and measured_bin_sha == closure_bin_sha
    if not bin_ok:
        probs.append("SHA-256 medido del binario distinto del cierre: la autorización de revisión no cubre otro binario")
    return {"Ok": not probs, "Problems": probs, "A6LineSha256": A6_LINE_SHA, "A6LineNumbers": [i + 1 for i in a6_hits],
            "Section78Line": (s78[0] + 1) if len(s78) == 1 else None,
            "NearMissLines": [i + 1 for i in near][:10], "ClaudeCliLineSha256": CLAUDE_LINE_SHA, "ClaudeCliLineNumbers": [i + 1 for i in claude_hits],
            "ObjectBlob": object_blob, "ObjectBlobAtHead": object_blob_at_head, "ObjectBlobAtHeadMatches": blob_ok and object_blob_at_head == object_blob,
            "A6AgreedBlob": A6_AGREED_BLOB, "A6BlobAtHead": a6_blob_at_head, "A6AgreedBlobAtHeadMatches": a6_ok,
            "PinnedLineInA6": pin_ok, "RunId": run_id, "BaseSection": BASE_SECTION, "AuthorizationPending": pending,
            "AuthorizationSection": a.get("Section") if isinstance(a, dict) else None,
            "AuthorizationHeading": a.get("Heading") if isinstance(a, dict) else None,
            "AuthorizationTextSha256": a.get("TextSha256") if isinstance(a, dict) else None, "AuthorizationSections": auth, "LaterSections": later,
            "SectionsAfterBase": after_base, "Authorized": authorized,
            "DecisionsAppendOnlySinceClone": append_only, "WorktreeDecisionsClean": worktree_clean is True,
            "HeadDescendsFromClone": head_descends is True, "BinarySha256MatchesClosure": bin_ok}


def _git(repo, *a):
    r = subprocess.run(["git", "-C", repo] + list(a), capture_output=True, env=dict(os.environ, GIT_OPTIONAL_LOCKS="0"))
    return r.returncode, r.stdout


def check(worktree, clone, clone_commit, closure, measured_bin_sha):
    """Reúne las entradas por git (solo lectura) y evalúa. worktree: el de I-62; clone: el clon de lectura (su commit da el texto de referencia).
    La A-6 acordada del commit del clon y el blob fijado de A-7 (closure.ObjectBlob) son los de esta revisión; la autorización, la del módulo."""
    rc, head = _git(worktree, "rev-parse", "HEAD")
    rc1, dec_head = _git(worktree, "show", "HEAD:" + DECISIONS)
    rc2, dec_clone = _git(clone, "show", clone_commit + ":" + DECISIONS)
    rc3, blob_head = _git(worktree, "rev-parse", "HEAD:" + DECISIONS)
    rc4, blob_wt = _git(worktree, "hash-object", "--", DECISIONS)
    rc5, _ = _git(worktree, "merge-base", "--is-ancestor", clone_commit, "HEAD")
    rc6, a6_text = _git(clone, "show", clone_commit + ":" + A6_PATH)
    rc7, obj_head = _git(worktree, "rev-parse", "HEAD:" + OBJECT_PATH)
    rc8, a6_head = _git(worktree, "rev-parse", "HEAD:" + A6_PATH)
    clean = rc3 == 0 and rc4 == 0 and blob_head.strip() == blob_wt.strip()
    out = evaluate(dec_head.decode("utf-8") if rc1 == 0 else "", dec_clone.decode("utf-8") if rc2 == 0 else "", clean, rc5 == 0,
                   measured_bin_sha, closure["Transport"]["Binary"]["Sha256"], a6_text.decode("utf-8") if rc6 == 0 else None,
                   obj_head.decode().strip() if rc7 == 0 else None, closure.get("ObjectBlob"), (closure.get("Ids") or {}).get("RunId"),
                   a6_head.decode().strip() if rc8 == 0 else None)
    out.update({"Worktree": WORKTREE_DISPLAY, "WorktreeHead": head.decode().strip() if rc == 0 else None,
                "DecisionsBlobAtHead": blob_head.decode().strip() if rc3 == 0 else None, "CloneCommit": clone_commit,
                "Reads": "git rev-parse, show, hash-object (sin -w) y merge-base, con GIT_OPTIONAL_LOCKS=0; nada se escribe"})
    return out
