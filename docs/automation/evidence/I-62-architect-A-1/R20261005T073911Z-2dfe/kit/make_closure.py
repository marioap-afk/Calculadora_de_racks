"""I-62 A-1 (corrected for A62-A1R-01..03, A62-A1A-01 and A62-A1S-01..02, blob 03dd822d, commit 411e01ce): EffectiveInputClosure for the SECOND
and last formal Architect review allowed by the night order (decisions §41, point E).

Review kit v3c (v3 R20261005T063359Z-86e3 was launched: CHANGES REQUIRED; v3b was prepared and withdrawn unlaunched). Lessons of R20261003T023945Z-cab7 (not accredited) and R20261005T044948Z-ac67 (auditor v2 defects) are built in: the clone's
only branch is `main` at the exact commit (the desktop task opens its worktree from it); Grep only on individual closure files; one enumerated action
contract that is IDENTICAL to the auditor whitelist (cd to the clone/worktree/own outputs, git metadata sub-commands, read-only shell tools, python
with heredocs reading only closure files and own outputs, dotnet test of the clone's Core project from Bash or PowerShell, own outputs); every path is
checked by effect (resolved against the cwd), not by literal command string.
"""
import datetime
import hashlib
import json
import secrets
import subprocess

REV = "411e01ce4176e4fe6648ea3e9ff2febc219ff438"
CLONE = r"D:\r62-arch-a1r5"
RUN = r"D:\r62-arch-a1r5-run"
canonical = [
    ("docs/initiatives/I-62-A-1.md", "objeto de la revisión (A-1 corregida)"),
    ("docs/initiatives/I-62-architect-package-A-1.md", "paquete del Architect"),
    ("docs/initiatives/I-62-architect-review-A-1-disposition.md", "disposiciones del Coordinator (§38, §39), matriz exacta y tratamiento de A62-A1R-01..03 y O1..O5 (§41); cabecera con A62-A1A-01 y A62-A1S-01..02"),
    ("docs/initiatives/I-62-architect-review-A-1.md", "registro de la revisión no acreditada (solo insumo técnico histórico)"),
    ("docs/initiatives/I-62-architect-review-A-1-r2.md", "registro de la revisión formal R20261005T044948Z-ac67 (A62-A1R-01..03)"),
    ("docs/initiatives/I-62-architect-review-A-1-r3.md", "registro de la revisión formal R20261005T063359Z-86e3 (A62-A1S-01..02)"),
    ("docs/automation/evidence/I-62-architect-A-1/R20261005T063359Z-86e3/output.json", "resultado literal de la revisión formal R20261005T063359Z-86e3"),
    ("docs/automation/evidence/I-62-architect-A-1/R20261005T044948Z-ac67/output.json", "resultado literal de la revisión formal anterior"),
    ("docs/automation/evidence/I-62-prep/night-2026-10-05/rebase-map.json", "mapa del rebase de la rama (SHAs históricos citados por los registros)"),
    ("docs/automation/evidence/I-62-prep/night-2026-10-05/f4-exp/README.md", "secuencias combinadas de F4 experimental: hallazgo A62-A1A-01 y observaciones"),
    ("docs/automation/evidence/I-62-prep/night-2026-10-05/f4-exp/combo-result-before.json", "resultado con el arnés del objeto anterior (reproduce A62-A1A-01)"),
    ("docs/automation/evidence/I-62-prep/night-2026-10-05/f4-exp/combo-result.json", "resultado con el arnés corregido"),
    ("docs/initiatives/I-62-proposal-v14.md", "Freeze: cláusulas que se enmiendan"),
    ("docs/initiatives/I-62-consensus-freeze.md", "registro del Freeze"),
    ("docs/automation/evidence/I-62-A1/a1-counterexamples.py", "arnés de contra-ejemplos (evidencia de apoyo, no autoridad)"),
    ("docs/automation/evidence/I-62-A1/a1-counterexamples-result.json", "resultado del arnés"),
    ("docs/automation/evidence/I-62-prep/freeze-issues.md", "hallazgo original de FC-01/FC-02 y SM-05"),
    ("docs/automation/evidence/I-62-prep/f4-dossier.md", "auditoría de transiciones (§4); citado por A-1 §8"),
    ("docs/INITIATIVE_LIFECYCLE.md", "§3 (M-01..M-08), §5 y §6 (A-n)"),
    ("docs/automation/decisions/I-62.md", "§34-§41 (clasificación, autorizaciones, disposiciones del Coordinator y orden nocturna)"),
    ("docs/automation/evidence/I-62-evidence.md", "§40-§49 (revisiones anteriores, correcciones, rebase, kits, dossiers, A62-A1A-01 y ciclo de corrección 1)"),
    ("docs/initiatives/I-62-portabilidad-coordinador-principal.md", "contrato: alcance y no-objetivos"),
    ("docs/AUTOMATION_PLAN.md", "§16 (I-61) y 16.20-16.24 (F3, inactivo)"),
    ("docs/automation/agent-execution/README.md", "§13-§16 (F3, inactivo)"),
    ("docs/automation/agent-execution/schemas/role-invocation.v1.schema.json", "contrato F3: Target, BudgetSnapshot, Authorization"),
    ("docs/automation/agent-execution/schemas/binding.v1.schema.json", "contrato F3: BindingRef, AuthorizationRef, Acceptance"),
    ("docs/automation/agent-execution/schemas/input-closure.v1.schema.json", "contrato F3: cierre de insumos (AuthorityRevision)"),
    ("docs/automation/agent-execution/schemas/input-fidelity.v1.schema.json", "contrato F3: fidelidad"),
    ("docs/automation/agent-execution/schemas/relay-record.v2.schema.json", "contrato F2: RebaseMap (Commits[], StateFields[])"),
    ("docs/automation/agent-execution/schemas/gate-contract.v2.schema.json", "contrato F3: RoleRequirements, Materialization"),
    ("docs/automation/agent-execution/schemas/reviewer-result.v1.schema.json", "contrato F3: Disposition, Severity"),
    ("docs/automation/agent-execution/schemas/architect-review-result.v1.schema.json", "contrato F3: resultado del Architect"),
    ("docs/adr/0046-protocolo-de-ejecucion-delegada-de-agentes.md", "ADR aceptado (M-08)"),
    ("docs/adr/0048-ejecucion-delegada-portable-roles-binding-y-autoverificacion.md", "ADR propuesto (M-08)"),
]
transitive = [
    ("CLAUDE.md", "AUTOMATIC_INSTRUCTION", "el runtime la inyecta"),
    ("docs/HANDOFF.md", "READ", "CLAUDE.md «Lectura inicial» 1; AGENTS.md «Leer primero» 1"),
    ("AGENTS.md", "READ", "CLAUDE.md «Lectura inicial» 2"),
    ("docs/WORKFLOW.md", "READ", "CLAUDE.md «Lectura inicial» 3; Context Pack documentation-governance"),
    ("docs/ARCHITECTURE.md", "READ", "CLAUDE.md «Lectura inicial» 4; AGENTS.md «Leer primero» 3"),
    ("docs/ROADMAP.md", "READ", "CLAUDE.md «Lectura inicial» 5; Context Pack"),
    ("docs/context-packs/README.md", "READ", "CLAUDE.md «Lectura inicial» 6; AGENTS.md «Leer primero» 3"),
    ("docs/context-packs/documentation-governance.md", "READ", "Context Pack declarado por I-62 (contrato, context_packs)"),
    ("README.md", "READ", "AGENTS.md «Leer primero» 2"),
    ("docs/FOUNDATIONS.md", "READ", "Context Pack documentation-governance, required_docs"),
    ("docs/initiatives/README.md", "READ", "Context Pack documentation-governance, required_docs"),
    ("docs/initiatives/PROMPT_TEMPLATES.md", "READ", "Context Pack documentation-governance, required_docs"),
]


def git(*a):
    return subprocess.check_output(["git", "-C", CLONE] + list(a))


def rec(p):
    b = git("show", REV + ":" + p)
    t = b.decode("utf-8")
    return {"Path": p, "Blob": git("rev-parse", REV + ":" + p).decode().strip(), "Bytes": len(b), "Lines": b.count(b"\n"),
            "Sha256": hashlib.sha256(b).hexdigest(), "LinesOver2000Chars": [i + 1 for i, x in enumerate(t.split("\n")) if len(x) > 2000],
            "Encoding": {"Charset": "UTF-8", "Bom": b.startswith(b"\xef\xbb\xbf"), "LineEnding": "LF" if b"\r\n" not in b else "CRLF"}}, t


corpus, can, tra = set(), [], []
for p, why in canonical:
    r, t = rec(p)
    r["Why"] = why
    can.append(r)
    corpus |= {c for c in t if ord(c) > 127}
for p, cls, req in transitive:
    r, t = rec(p)
    r["Class"], r["RequiredBy"] = cls, req
    tra.append(r)
    corpus |= {c for c in t if ord(c) > 127}
now = datetime.datetime.now(datetime.timezone.utc)
sfx = secrets.token_hex(2)
ids = {"RunId": now.strftime("R%Y%m%dT%H%M%SZ-") + sfx, "InvocationId": now.strftime("I%Y%m%dT%H%M%SZ-") + sfx,
       "LogicalReviewRequestId": now.strftime("L%Y%m%dT%H%M%SZ-") + sfx, "AttemptSeq": 1}
OWN = "salidas propias del revisor: archivos que la propia sesión revisora crea en su scratchpad o en su directorio de tareas " \
      "(C:/Users/<usuario>/AppData/Local/Temp/claude/D--Documentos-Worktrees-r62-arch-a1r5-<worktree>/<sesión>/…)"
closure = {
    "Schema": "rackcad-input-closure/v1 (representación experimental; Proposal V14 §20.3.1; no autoridad normativa: rigen I-61 y LIFECYCLE)",
    "Ids": ids, "AuthorityRevision": REV,
    "Adapter": "claude-desktop-session: sesión nueva y separada lanzada por el Owner desde una tarea de la app, con cwd en el clon limpio " + CLONE
               + " (rama main = el commit exacto; sin remoto)",
    "CanonicalInputs": can, "AllowedTransitiveInputs": tra,
    "RunFiles": ["order.txt", "prior-authorization.txt", "prompt.md"],
    "ReadPolicy": {
        "Read": "ruta absoluta de un archivo del cierre, bajo el clon o bajo el worktree propio (que está en el mismo commit), con offset y limit; también order.txt, prior-authorization.txt y prompt.md, y salidas propias",
        "Grep": "solo con `path` = un ARCHIVO del cierre; nunca un directorio ni `glob`",
        "LargeFileThresholdBytes": 60000, "MaxRangeLines": 300,
        "LargeFiles": [r["Path"] for r in can + tra if r["Bytes"] > 60000],
        "LongLines": {r["Path"]: r["LinesOver2000Chars"] for r in can + tra if r["LinesOver2000Chars"]},
        "Premises": "toda premisa (PremiseRefs) se cita de líneas leídas con Read, o con `git show " + REV[:8] + ":<ruta> | sed -n 'N,Mp'` para las líneas largas",
    },
    "AllowedActions": [
        {"Id": "ID-1", "Action": "git (con -C D:/r62-arch-a1r5, -C <worktree propio> o desde el cwd del clon o del worktree): rev-parse HEAD | rev-parse <commit>:<ruta del cierre> | cat-file -p|-t|-s <commit>:<ruta del cierre> | status [--porcelain] | log --oneline [-N] | worktree list | config --get <clave>",
         "RequiredBy": "identidad del objeto y del árbol (orden: verificar el worktree en el objeto exacto antes de leer); AGENTS.md «Leer primero» 4", "Class": "ACTION_COMPATIBLE"},
        {"Id": "ID-2", "Action": "git checkout --detach " + REV + " en el worktree propio, solo si su HEAD no es el commit", "RequiredBy": "fijar el árbol de la sesión revisora", "Class": "ACTION_COMPATIBLE"},
        {"Id": "RD-1", "Action": "git -C D:/r62-arch-a1r5 show " + REV[:8] + ":<ruta del cierre>, sola o seguida por tubería de sed -n, head, tail, wc, grep o awk",
         "RequiredBy": "lectura fiel de líneas largas y metadatos de archivos del cierre", "Class": "ACTION_COMPATIBLE"},
        {"Id": "RD-2", "Action": "sed -n, wc, awk, grep (sin -r, -R ni --recursive), head, tail, cat, sha256sum, echo, printf y true sobre ARCHIVOS del cierre (bajo el clon o el worktree propio), sobre order.txt, prior-authorization.txt y prompt.md, o sobre salidas propias; combinables con |, ;, && y ||; sin redirección salvo hacia salidas propias o /dev/null; ningún directorio como argumento",
         "RequiredBy": "metadatos de solo lectura (tamaños, líneas largas, secciones) y comprobación del hash del prompt", "Class": "ACTION_COMPATIBLE"},
        {"Id": "EX-1", "Action": "python D:/r62-arch-a1r5/docs/automation/evidence/I-62-A1/a1-counterexamples.py <salida propia>; y python (también con heredoc o -c) cuyas rutas sean solo archivos del cierre, los archivos del run permitidos y salidas propias",
         "RequiredBy": "verificación del arnés (foco 11)", "Class": "ACTION_COMPATIBLE"},
        {"Id": "EX-2", "Action": "dotnet test tests/RackCad.Tests/RackCad.Tests.csproj dentro del clon (Bash o PowerShell; SDK en %LOCALAPPDATA%\\Microsoft\\dotnet\\dotnet.exe), con el log en una salida propia",
         "RequiredBy": "AGENTS.md «Leer primero» 4 y CLAUDE.md «Comandos esenciales»", "Class": "ACTION_COMPATIBLE",
         "Note": "la orden no trae exención; escribe solo bin/obj del clon desechable; comprobación local del revisor, no evidencia de gate"},
        {"Id": "NAV-1", "Action": "cd hacia el clon, el worktree propio o un directorio de salidas propias (las rutas relativas posteriores se resuelven desde ahí)", "RequiredBy": "ejecutar EX-1 y EX-2 y las lecturas relativas", "Class": "ACTION_COMPATIBLE"},
        {"Id": "OWN-1", "Action": "leer (Read, cat, tail, grep, Get-Content de PowerShell) o escribir (Write, redirección) las salidas propias", "RequiredBy": "ejecutar y auditar EX-1 y EX-2", "Class": "OWN_OUTPUT",
         "Note": OWN},
    ],
    "HealthSignals": [{"Kind": "PUBLICATION_CI", "RunRef": "37278429319 (push, head_sha exacto 411e01ce, cuatro jobs requeridos success)", "Sha": REV,
                       "Note": "señal separada de salud de publicación; NO es evidencia equivalente a Core local"}],
    "NotTriggered": [
        {"Source": "CLAUDE.md, validación de dibujo (docs/guias/validacion-manual-autocad.md)", "Reason": "condicionada a una validación de dibujo, que la revisión no hace"},
        {"Source": "documentation-governance optional_docs y code_globs", "Reason": "opcionales u orientativos"},
        {"Source": "enlaces de A-1 y de la evidencia a otros documentos fuera del cierre", "Reason": "citas descriptivas; el cierre ya contiene las autoridades"},
    ],
    "DeclaredRuntimeContext": [
        "instrucciones base de la aplicación de escritorio y del runtime (no inspeccionables por el invocador)",
        "CLAUDE.md del worktree (mismo commit que el clon), inyectado automáticamente",
        "~/.claude/CLAUDE.md de usuario: no existe (medido)",
        "memoria automática: el clon es otro repositorio (D:\\r62-arch-a1r5), así que no carga la memoria del proyecto de la sesión autora",
        "herramientas, skills y conectores habilitados por defecto en una sesión nueva",
    ],
    "ForbiddenInputs": [
        "transcripción y memoria de la sesión autora (~/.claude/projects/D--Documentos-Codex-Calculadora-de-racks/*) y toda otra entrada de ~/.claude/projects",
        "otras sesiones, incluidas la de la revisión anterior y las de I-52, I-63 e I-64",
        "el worktree real de I-62 (~/.codex/worktrees/*), el repositorio de trabajo y los clones de revisiones anteriores (D:\\r62-arch-*, salvo D:\\r62-arch-a1r5)",
        "los archivos de " + RUN + " distintos de order.txt, prior-authorization.txt y prompt.md",
        "cualquier archivo fuera de CanonicalInputs y AllowedTransitiveInputs, salvo las salidas propias",
        "Grep o búsqueda sobre directorios; listados de directorios",
        "red (fetch, web)", "invocar otros agentes, subagentes, Worker o Controller", "escribir, hacer commit o push en el repositorio",
    ],
    "CorpusDistinctNonAscii": len(corpus),
}
with open(RUN + r"\closure.json", "w", encoding="utf-8", newline="\n") as f:
    json.dump(closure, f, ensure_ascii=False, indent=1)
    f.write("\n")
with open(RUN + r"\corpus.json", "w", encoding="utf-8", newline="\n") as f:
    json.dump(sorted(corpus), f, ensure_ascii=False)
print(json.dumps({"ids": ids, "canonical": len(can), "transitive": len(tra), "corpus": len(corpus),
                  "large": [(r["Path"], r["Bytes"], r["Lines"]) for r in can + tra if r["Bytes"] > 60000],
                  "long": closure["ReadPolicy"]["LongLines"]}, ensure_ascii=True))
