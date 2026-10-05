"""I-62 A-1 (corrected for A62-A1T-01 and O1..O3, blob c01899a7, commit ca09ade8): EffectiveInputClosure for the ONE formal Architect review
authorized by the new Coordinator order (decisions §42, point E).

Review kit v4. The action contract is the new one of the order (§7), fixed BEFORE launching, and the auditor whitelist (post-review.py v4) is the
same table: navigation to the root and subdirectories of the clone or the own worktree (NAV-1; navigating never authorizes a read); reads and
searches only on concrete closure files at the pinned commit (RD-1, RD-2); enumerated metadata and hashing commands (ID-1, MD-1); own temporary
scripts only in the run's own scratchpad, including a declared `sed -i` (OWN-2); reading own outputs (OWN-1). Effective paths (links resolved) are
checked, and a failed read is never credited.
"""
import datetime
import hashlib
import json
import secrets
import subprocess

REV = "ca09ade8bb31b1ecb57b2b0d6220628c8434e78d"
BASE_REV = "411e01ce4176e4fe6648ea3e9ff2febc219ff438"
CLONE = r"D:\r62-arch-a1t01"
RUN = r"D:\r62-arch-a1t01-run"
EV2 = "docs/automation/evidence/I-62-architect-A-1/"
NIGHT = "docs/automation/evidence/I-62-prep/night-2026-10-05/"
canonical = [
    ("docs/initiatives/I-62-A-1.md", "objeto de la revisión (A-1 corregida para A62-A1T-01 y O1..O3)"),
    ("docs/initiatives/I-62-architect-package-A-1.md", "paquete del Architect"),
    ("docs/initiatives/I-62-architect-review-A-1-disposition.md", "disposiciones del Coordinator; §5 = disposición de A62-A1T-01 y O1..O3"),
    ("docs/initiatives/I-62-architect-review-A-1.md", "registro de la revisión no acreditada (solo insumo técnico histórico)"),
    ("docs/initiatives/I-62-architect-review-A-1-r2.md", "registro de la revisión formal R20261005T044948Z-ac67"),
    ("docs/initiatives/I-62-architect-review-A-1-r3.md", "registro de la revisión formal R20261005T063359Z-86e3"),
    ("docs/initiatives/I-62-architect-review-A-1-r4.md", "registro de la revisión formal R20261005T073911Z-2dfe (A62-A1T-01)"),
    (EV2 + "R20261005T044948Z-ac67/output.json", "resultado literal de la revisión formal ac67"),
    (EV2 + "R20261005T063359Z-86e3/output.json", "resultado literal de la revisión formal 86e3"),
    (EV2 + "R20261005T073911Z-2dfe/output.json", "resultado literal de la revisión formal 2dfe (A62-A1T-01, O1..O3, doce cierres)"),
    (EV2 + "R20261005T073911Z-2dfe/audit-classification.json", "clasificación del NOT_ACCREDITED del auditor v3.1 (conservada)"),
    (NIGHT + "rebase-map.json", "mapa del rebase de la rama (SHAs históricos citados por los registros)"),
    (NIGHT + "f4-exp/README.md", "F4 experimental: secuencias combinadas, A62-A1A-01 y F4X-OBS-01 → A62-A1T-01"),
    (NIGHT + "f4-exp/combo_sequences.py", "script de las secuencias combinadas"),
    (NIGHT + "f4-exp/combo-result-before.json", "ejecución con el arnés de 39c2f831 (histórica)"),
    (NIGHT + "f4-exp/combo-result.json", "ejecución con el arnés 6b051db7 (histórica; esperaba el STOP de c3-obs)"),
    (NIGHT + "f4-exp/combo-result-a1t01.json", "ejecución con el arnés corregido a1f4b07f (c3-obs por la cadena)"),
    (NIGHT + "portability/README.md", "portabilidad: reconstrucción en un clon limpio"),
    (NIGHT + "portability/reconstruct.py", "script de reconstrucción (paso 2 y paso 2b T8)"),
    ("docs/initiatives/I-62-proposal-v14.md", "Freeze: cláusulas que se enmiendan"),
    ("docs/initiatives/I-62-consensus-freeze.md", "registro del Freeze"),
    ("docs/automation/evidence/I-62-A1/a1-counterexamples.py", "arnés de contra-ejemplos (evidencia de apoyo, no autoridad)"),
    ("docs/automation/evidence/I-62-A1/a1-counterexamples-result.json", "resultado del arnés (118 trazas)"),
    ("docs/automation/evidence/I-62-A1/a1t01/red-result.json", "RED de A1-P08 antes de la corrección"),
    ("docs/automation/evidence/I-62-A1/a1t01/green-result.json", "GREEN después de la corrección"),
    ("docs/automation/evidence/I-62-A1/a1t01/green.diff", "diff RED → GREEN de A1-P08"),
    ("docs/automation/evidence/I-62-A1/a1t01/t8-clean-clone.py", "T8: dos rebases reales y un sucesor en un clon limpio"),
    ("docs/automation/evidence/I-62-A1/a1t01/t8-result.json", "resultado de T8"),
    ("docs/automation/evidence/I-62-prep/freeze-issues.md", "hallazgo original de FC-01/FC-02 y SM-05"),
    ("docs/automation/evidence/I-62-prep/f4-dossier.md", "auditoría de transiciones (§4); citado por A-1 §8"),
    ("docs/INITIATIVE_LIFECYCLE.md", "§3 (M-01..M-08), §5 y §6 (A-n)"),
    ("docs/automation/decisions/I-62.md", "§34-§42 (clasificación, autorizaciones, disposiciones del Coordinator, orden nocturna y orden nueva)"),
    ("docs/automation/evidence/I-62-evidence.md", "§40-§55 (revisiones, correcciones, kits, F4 experimental, portabilidad y corrección de A62-A1T-01)"),
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
    ("docs/WORKFLOW.md", "READ", "CLAUDE.md «Lectura inicial» 3; Context Pack documentation-governance; WORKFLOW §4.2 (rebase al abrir)"),
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
      "(C:/Users/<usuario>/AppData/Local/Temp/claude/D--Documentos-Worktrees-r62-arch-a1t01-<worktree>/<sesión>/…)"
closure = {
    "Schema": "rackcad-input-closure/v1 (representación experimental; Proposal V14 §20.3.1; no autoridad normativa: rigen I-61 y LIFECYCLE)",
    "Ids": ids, "AuthorityRevision": REV, "DiffBases": [BASE_REV], "FocusItems": 12,
    "Adapter": "claude-desktop-session: sesión nueva y separada lanzada por el Owner desde una tarea de la app, con cwd en el clon limpio " + CLONE
               + " (rama main = el commit exacto; sin remoto)",
    "CanonicalInputs": can, "AllowedTransitiveInputs": tra,
    "RunFiles": ["order.txt", "prompt.md"],
    "ReadPolicy": {
        "Read": "ruta absoluta de un archivo del cierre, bajo el clon o bajo el worktree propio (que está en el mismo commit), con offset y limit; también order.txt y prompt.md, y salidas propias",
        "Grep": "solo con `path` = un ARCHIVO del cierre; nunca un directorio ni `glob`",
        "LargeFileThresholdBytes": 60000, "MaxRangeLines": 300,
        "LargeFiles": [r["Path"] for r in can + tra if r["Bytes"] > 60000],
        "LongLines": {r["Path"]: r["LinesOver2000Chars"] for r in can + tra if r["LinesOver2000Chars"]},
        "Premises": "toda premisa (PremiseRefs) se cita de líneas leídas con Read, o con `git -C D:/r62-arch-a1t01 show " + REV[:8] + ":<ruta> | sed -n 'N,Mp'` para las líneas largas; una lectura fallida nunca cuenta",
        "FailedReads": "un comando que falla o no devuelve contenido (también un git show que Git rechaza) es una lectura fallida: no acredita contenido",
    },
    "AllowedActions": [
        {"Id": "ID-1", "Action": "git (con -C D:/r62-arch-a1t01, -C <worktree propio o un subdirectorio suyo> o desde el cwd del clon o del worktree): rev-parse HEAD | rev-parse <commit>:<ruta del cierre> | cat-file -p|-t|-s <commit>:<ruta del cierre> | status [--porcelain] | log --oneline [-N] | worktree list | config --get <clave>",
         "RequiredBy": "identidad del objeto y del árbol; AGENTS.md «Leer primero» 4", "Class": "ACTION_COMPATIBLE"},
        {"Id": "ID-2", "Action": "git checkout --detach " + REV + " en el worktree propio, solo si su HEAD no es el commit", "RequiredBy": "fijar el árbol de la sesión revisora", "Class": "ACTION_COMPATIBLE"},
        {"Id": "MD-1", "Action": "metadatos y hashing enumerados: git hash-object <archivo del cierre o salida propia> (sin -w ni --stdin); git diff [--stat|--numstat|--no-color|--word-diff|-U<N>] " + BASE_REV[:8] + " " + REV[:8] + " -- <rutas del cierre> (la base es el commit del blob revisado antes, 03dd822d); sha256sum y wc sobre archivos del cierre, del run o salidas propias; pwd; date",
         "RequiredBy": "delta exacto frente a 03dd822d (paquete §2) y hashes de identidad", "Class": "ACTION_COMPATIBLE"},
        {"Id": "NAV-1", "Action": "cd (o Set-Location en PowerShell) hacia la raíz o CUALQUIER SUBDIRECTORIO del clon o del worktree propio, o hacia un directorio de salidas propias. Cambiar de directorio no autoriza leer nada fuera del cierre: las rutas posteriores se resuelven desde ahí y se clasifican por su efecto",
         "RequiredBy": "lecturas relativas y ejecución de EX-1/EX-2", "Class": "ACTION_COMPATIBLE"},
        {"Id": "RD-1", "Action": "git show " + REV[:8] + ":<ruta del cierre> (o HEAD:<ruta del cierre> con HEAD ya verificado), sola o con tubería a sed -n, head, tail, wc, grep, awk, sha256sum, sort, uniq, cut, tr o nl. La ruta de un objeto Git es de árbol: Git no normaliza `..` y un git show rechazado es una lectura fallida",
         "RequiredBy": "lectura fiel de líneas largas y metadatos de archivos del cierre", "Class": "ACTION_COMPATIBLE"},
        {"Id": "RD-2", "Action": "sed -n, wc, awk, grep (sin -r, -R ni --recursive), head, tail, cat, sha256sum, sort, uniq, cut, tr, nl, diff, cmp, echo, printf, true y read, solo sobre ARCHIVOS del cierre (bajo el clon o el worktree propio), sobre order.txt y prompt.md, o sobre salidas propias; combinables con |, ;, &&, || y bucles for/while; redirección solo hacia salidas propias o /dev/null; ningún directorio como argumento",
         "RequiredBy": "metadatos de solo lectura (tamaños, líneas largas, secciones) y comprobación del hash del prompt", "Class": "ACTION_COMPATIBLE"},
        {"Id": "EX-1", "Action": "python D:/r62-arch-a1t01/docs/automation/evidence/I-62-A1/a1-counterexamples.py <salida propia>; y python (heredoc, -c o un script propio) cuyas rutas literales sean solo archivos del cierre, archivos del run y salidas propias; sin listar directorios, sin procesos, sin red y sin borrar ni mover archivos",
         "RequiredBy": "verificación del arnés", "Class": "ACTION_COMPATIBLE"},
        {"Id": "EX-2", "Action": "dotnet test tests/RackCad.Tests/RackCad.Tests.csproj dentro del clon (Bash o PowerShell; SDK en %LOCALAPPDATA%\\Microsoft\\dotnet\\dotnet.exe), con el log en una salida propia",
         "RequiredBy": "AGENTS.md «Leer primero» 4 y CLAUDE.md «Comandos esenciales»", "Class": "ACTION_COMPATIBLE",
         "Note": "la orden no trae exención; escribe solo bin/obj del clon desechable; comprobación local del revisor, no evidencia de gate"},
        {"Id": "OWN-1", "Action": "leer (Read, cat, tail, grep, Get-Content) las salidas propias", "RequiredBy": "comprobar pruebas y tareas de la corrida", "Class": "OWN_OUTPUT", "Note": OWN},
        {"Id": "OWN-2", "Action": "crear y editar scripts y archivos temporales propios SOLO en las salidas propias: Write, Edit, redirección, mkdir -p y sed -i (declarado). Ningún permiso sobre salidas propias autoriza editar insumos canónicos, scripts custodiados, fuentes del repositorio ni archivos de otra sesión",
         "RequiredBy": "scripts ad hoc de verificación (p. ej., trazas propias con el modelo del arnés)", "Class": "OWN_OUTPUT", "Note": OWN},
    ],
    "HealthSignals": [{"Kind": "PUBLICATION_CI", "RunRef": "37329298556 (push, head_sha exacto ca09ade8, cuatro jobs requeridos success)", "Sha": REV,
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
        "memoria automática: el clon es otro repositorio (D:\\r62-arch-a1t01), así que no carga la memoria del proyecto de la sesión autora",
        "herramientas, skills y conectores habilitados por defecto en una sesión nueva",
    ],
    "ForbiddenInputs": [
        "transcripción y memoria de la sesión autora (~/.claude/projects/D--Documentos-Codex-Calculadora-de-racks/*) y toda otra entrada de ~/.claude/projects",
        "otras sesiones, incluidas las de las revisiones anteriores y las de I-52, I-63 e I-64",
        "el worktree real de I-62 (~/.codex/worktrees/*), el repositorio de trabajo y los clones de revisiones anteriores (D:\\r62-arch-*, salvo D:\\r62-arch-a1t01)",
        "los archivos de " + RUN + " distintos de order.txt y prompt.md",
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
