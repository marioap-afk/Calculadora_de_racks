import datetime
import hashlib
import json
import secrets
import subprocess

REV = "bf7b0d9c4ec79e38350bad55e0debcde55b3cec9"
CLONE = r"D:\r62-arch-a1"
canonical = [
    ("docs/initiatives/I-62-A-1.md", "objeto de la revisión"),
    ("docs/initiatives/I-62-architect-package-A-1.md", "paquete del Architect"),
    ("docs/initiatives/I-62-proposal-v14.md", "Freeze: cláusulas que se enmiendan"),
    ("docs/initiatives/I-62-consensus-freeze.md", "registro del Freeze"),
    ("docs/automation/evidence/I-62-A1/a1-counterexamples.py", "contra-ejemplos (evidencia de apoyo, no autoridad)"),
    ("docs/automation/evidence/I-62-A1/a1-counterexamples-result.json", "resultado de los contra-ejemplos"),
    ("docs/automation/evidence/I-62-prep/freeze-issues.md", "hallazgo original de FC-01 y FC-02"),
    ("docs/automation/evidence/I-62-prep/f4-dossier.md", "auditoría de transiciones (§4)"),
    ("docs/INITIATIVE_LIFECYCLE.md", "§3 (M-01..M-08), §5 y §6 (A-n)"),
    ("docs/automation/decisions/I-62.md", "§34 (clasificación y dirección del Coordinator)"),
    ("docs/automation/evidence/I-62-evidence.md", "§37-§38 (F3 y paquete de A-1)"),
    ("docs/initiatives/I-62-portabilidad-coordinador-principal.md", "contrato: alcance y no-objetivos"),
    ("docs/AUTOMATION_PLAN.md", "§16 (I-61) y 16.20-16.24 (F3, inactivo)"),
    ("docs/automation/agent-execution/README.md", "§14-§16 (F3, inactivo)"),
    ("docs/automation/agent-execution/schemas/role-invocation.v1.schema.json", "contrato F3: Target, BudgetSnapshot, Authorization"),
    ("docs/automation/agent-execution/schemas/binding.v1.schema.json", "contrato F3: BindingRef, AuthorizationRef, Acceptance"),
    ("docs/automation/agent-execution/schemas/input-closure.v1.schema.json", "contrato F3: cierre de insumos"),
    ("docs/automation/agent-execution/schemas/input-fidelity.v1.schema.json", "contrato F3: fidelidad"),
    ("docs/automation/agent-execution/schemas/relay-record.v2.schema.json", "contrato F2: RebaseMap.StateFields"),
    ("docs/automation/agent-execution/schemas/gate-contract.v2.schema.json", "contrato F3: RoleRequirements, Materialization"),
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
    return {"Path": p, "Blob": git("rev-parse", REV + ":" + p).decode().strip(), "Bytes": len(b), "Lines": b.count(b"\n"),
            "Sha256": hashlib.sha256(b).hexdigest(),
            "Encoding": {"Charset": "UTF-8", "Bom": b.startswith(b"\xef\xbb\xbf"), "LineEnding": "LF" if b"\r\n" not in b else "CRLF"}}, b.decode("utf-8")


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
       "LogicalReviewRequestId": now.strftime("L%Y%m%dT%H%M%SZ-") + sfx}
closure = {
    "Schema": "rackcad-input-closure/v1 (representación experimental; Proposal V14 §20.3.1; no autoridad normativa: rigen I-61 y LIFECYCLE)",
    "Ids": ids, "AuthorityRevision": REV,
    "Adapter": "claude-desktop-session: sesión nueva y separada, lanzada desde un clon limpio fuera del repositorio de trabajo (D:\\r62-arch-a1)",
    "CanonicalInputs": can, "AllowedTransitiveInputs": tra,
    "ReadPolicy": {"Tools": "Read (ruta absoluta bajo el clon, con offset y limit) y Grep (solo sobre rutas del cierre); git show " + REV[:8] + ":<ruta> si el árbol no está en el commit",
                   "LargeFileThresholdBytes": 60000, "MaxRangeLines": 300,
                   "LargeFiles": [r["Path"] for r in can + tra if r["Bytes"] > 60000]},
    "AllowedActions": [
        {"Action": "git rev-parse / git status --porcelain / git log --oneline -10", "RequiredBy": "identidad del objeto; AGENTS.md «Leer primero» 4",
         "Class": "ACTION_COMPATIBLE"},
        {"Action": "git checkout --detach " + REV, "RequiredBy": "fijar el árbol de la sesión revisora en el commit exacto (su worktree es desechable)",
         "Class": "ACTION_COMPATIBLE"},
        {"Action": "python docs/automation/evidence/I-62-A1/a1-counterexamples.py <archivo temporal fuera del clon>",
         "RequiredBy": "verificación de los contra-ejemplos que pide la orden", "Class": "ACTION_COMPATIBLE"},
        {"Action": "dotnet test tests/RackCad.Tests/RackCad.Tests.csproj, solo dentro del clon desechable",
         "RequiredBy": "AGENTS.md «Leer primero» 4 y CLAUDE.md «Comandos esenciales»", "Class": "ACTION_COMPATIBLE",
         "Note": "escribe solo bin/obj del clon desechable y no modifica archivos versionados; la orden vigente no trae exención, así que la acción no se omite"},
    ],
    "HealthSignals": [{"Kind": "PUBLICATION_CI", "RunRef": "37085558300 (push, head_sha exacto bf7b0d9c, cuatro jobs requeridos success)", "Sha": REV,
                       "Note": "señal separada de salud de publicación; NO es evidencia equivalente a Core local"}],
    "NotTriggered": [
        {"Source": "CLAUDE.md, validación de dibujo (docs/guias/validacion-manual-autocad.md)", "Reason": "condicionada a una validación de dibujo, que la revisión no hace"},
        {"Source": "documentation-governance optional_docs y code_globs", "Reason": "opcionales u orientativos"},
        {"Source": "menciones de «Leer primero» o required_docs dentro de documentos y evidencia de I-62", "Reason": "citas descriptivas"},
    ],
    "DeclaredRuntimeContext": [
        "instrucciones base de la aplicación de escritorio y del runtime (no inspeccionables por el invocador)",
        "CLAUDE.md del clon, inyectado automáticamente",
        "~/.claude/CLAUDE.md de usuario: no existe (medido)",
        "memoria automática: el clon es otro repositorio (D:\\r62-arch-a1), así que no carga la memoria del proyecto de la sesión autora",
        "herramientas, skills y conectores habilitados por defecto en una sesión nueva",
    ],
    "ForbiddenInputs": [
        "transcripción y memoria de la sesión autora (~/.claude/projects/D--Documentos-Codex-Calculadora-de-racks/*)",
        "otras sesiones, incluidas las de revisiones anteriores de I-62, I-63 e I-64",
        "el worktree real de I-62 y los de otras unidades",
        "cualquier archivo fuera de CanonicalInputs y AllowedTransitiveInputs (salvo la salida del script de contra-ejemplos en un archivo temporal)",
        "red (fetch, web)", "invocar otros agentes, subagentes, Worker o Controller", "escribir, hacer commit o push en el repositorio",
    ],
    "CorpusDistinctNonAscii": len(corpus),
}
with open(r"D:\r62-arch-a1-run\closure.json", "w", encoding="utf-8", newline="\n") as f:
    json.dump(closure, f, ensure_ascii=False, indent=1)
    f.write("\n")
with open(r"D:\r62-arch-a1-run\corpus.json", "w", encoding="utf-8", newline="\n") as f:
    json.dump(sorted(corpus), f, ensure_ascii=False)
print(json.dumps({"ids": ids, "canonical": len(can), "transitive": len(tra), "corpus": len(corpus),
                  "large": [(r["Path"], r["Bytes"], r["Lines"]) for r in can + tra if r["Bytes"] > 60000]}, ensure_ascii=True))
