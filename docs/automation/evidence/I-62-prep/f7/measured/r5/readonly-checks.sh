#!/usr/bin/env bash
# I-62 F7 — comprobaciones de solo lectura sobre la punta exacta (staging; no declara ningún gate ni READY).
# Uso: WT=<raíz del worktree de I-62> bash readonly-checks.sh <TIP> > readonly-checks-<tip>.txt
# Solo lectura: git show / rev-parse / diff / log / ls-tree / ls-remote / for-each-ref; ningún fetch, checkout, build ni escritura en el worktree.
set -u
cd "$WT" || exit 2
H="${1:?TIP}"
B=bb0d5522e8411f66a51fdfb3f1f0d0514b737453     # origin/main local = merge-base medido
MC=6f0187cb30852971b49153c2763c8ba6caeaa64d    # MC_I62 (decisiones §44)
echo "# readonly-checks  TIP=$H  BASE=$B  MC_I62=$MC  UTC=$(date -u +%Y-%m-%dT%H:%M:%SZ)"
echo "## K-01 merge-base(TIP, origin/main local)"; git merge-base "$H" origin/main
echo "## K-02 ls-remote --heads origin (DC-07 sin fetch)"; timeout 60 git ls-remote --heads origin
echo "## K-03 superficies cerradas de 16.13 cambiadas MC_I62..TIP (C-21)"
git diff --name-only "$MC" "$H" -- AGENTS.md CLAUDE.md docs/AUTOMATION_PLAN.md docs/FOUNDATIONS.md docs/INITIATIVE_LIFECYCLE.md docs/WORKFLOW.md docs/adr/ docs/automation/agent-execution/ docs/initiatives/PROMPT_TEMPLATES.md | wc -l
echo "## K-04 archivos fuera de docs/ y tests/ (merge-base...TIP; READY-08)"; git diff --name-only "$B...$H" | grep -v -E '^(docs/|tests/)' | wc -l
echo "   total: $(git diff --name-only "$B...$H" | wc -l)  tests/: $(git diff --name-only "$B...$H" | grep -c '^tests/')  commits: $(git rev-list --count "$B..$H")"
echo "## K-05 trailer Agent-Protocol-Normative (Q15)"
echo "   rama: $(git log --format='%H %(trailers:key=Agent-Protocol-Normative,valueonly)' "$B..$H" | awk 'NF>1' | wc -l)   origin/main: $(git log --grep='Agent-Protocol-Normative' --format=%H "$B" | wc -l)"
echo "## K-06 blobs en TIP"
for f in docs/initiatives/I-62-proposal-v14.md docs/initiatives/I-62-consensus-freeze.md docs/initiatives/I-62-A-1.md docs/initiatives/I-62-A-2.md \
         docs/initiatives/I-62-A-3.md docs/initiatives/I-62-A-3-annex-maf-codex.md docs/initiatives/I-62-A-4.md \
         docs/adr/0048-ejecucion-delegada-portable-roles-binding-y-autoverificacion.md docs/adr/README.md docs/FOUNDATIONS.md \
         docs/automation/agent-execution/model-catalog.md docs/automation/agent-execution/routing.md docs/ideas-futuras.md \
         docs/automation/agent-execution/compatibility/I62-clause-map.json; do echo "   $(git rev-parse "$H:$f") $f"; done
echo "## K-07 A-n versionadas (READY-01/09)"; git ls-tree --name-only "$H" docs/initiatives/ | grep -E '^docs/initiatives/I-62-A-[0-9]+\.md$'
echo "   (anexos no A-n): $(git ls-tree --name-only "$H" docs/initiatives/ | grep -E '^docs/initiatives/I-62-A-[0-9]+-.+\.md$' | tr '\n' ' ')"
echo "## K-08 inmutabilidad de cada A-n desde su commit de acuerdo (blob en el commit = blob en TIP; ancestro; ningún commit posterior)"
for p in "ca09ade8 docs/initiatives/I-62-A-1.md" "3bbaabef docs/initiatives/I-62-A-2.md" "91e29886 docs/initiatives/I-62-A-3.md" "c2dbc225 docs/initiatives/I-62-A-4.md"; do
  set -- $p; echo "   $2: commit $1 blob $(git rev-parse "$1:$2" | cut -c1-8) TIP $(git rev-parse "$H:$2" | cut -c1-8) ancestro=$(git merge-base --is-ancestor "$1" "$H" && echo si || echo no) posteriores=$(git log --format=%h "$1..$H" -- "$2" | wc -l)"
done
echo "## K-09 Freeze (ensayo de READY-09 con F0=1d5cdbec, FZ=fb49fceb)"
echo "   diff F0..FZ V14 (líneas): $(git diff 1d5cdbec fb49fceb -- docs/initiatives/I-62-proposal-v14.md | wc -l)"
echo "   registro en FZ: $(git rev-parse fb49fceb:docs/initiatives/I-62-consensus-freeze.md)"
echo "   trailers Git de FZ: $(git show -s --format=%B fb49fceb | git interpret-trailers --parse | cut -d: -f1 | tr '\n' ' ')"
echo "## K-10 punto de entrada, 16.13 y punteros en TIP (parte sin historia de MV-7; ensayo, no sobre el merge)"
echo "   WORKFLOW '## 12. Coexistencia…': $(git show "$H:docs/WORKFLOW.md" | tr -d '\r' | grep -c -x '## 12. Coexistencia de protocolos de ejecución delegada (I61/I62)')"
echo "   AP '### 16.13 …': $(git show "$H:docs/AUTOMATION_PLAN.md" | tr -d '\r' | grep -c -x '### 16.13 Compatibilidad de protocolos de ejecución delegada')"
echo "   WORKFLOW §12 nombra la línea de 16.13: $(git show "$H:docs/WORKFLOW.md" | tr -d '\r' | awk '/^## 12\. Coexistencia/{f=1;next} /^## /{f=0} f' | grep -c '### 16.13 Compatibilidad de protocolos de ejecución delegada')"
echo "   puntero en AP (§16 + 16.3): $(git show "$H:docs/AUTOMATION_PLAN.md" | tr -d '\r' | grep -c 'Antes de aplicar esta sección, toda unidad aplica §16.13.')"
echo "   mapa y esquema presentes: $(git cat-file -e "$H:docs/automation/agent-execution/compatibility/I62-clause-map.json" && git cat-file -e "$H:docs/automation/agent-execution/compatibility/clause-map.schema.json" && echo si)"
echo "## K-11 DC-08 en seco: clases y rutas citadas por el borrador de FOUNDATIONS"
for c in AgentExecutionProtocolTests I61EditedRackNameTests I61CamaEditWiringGuardTests PrincipalPortabilityProtocolTests I62F4CompatibilityGuardTests; do
  echo "   class $c: $(git grep -l -E "class $c\b" "$H" -- tests/ | wc -l)"; done
echo "   I61_P3 en FlowBedEditorWindowTests.cs: $(git grep -c 'I61_P3' "$H" -- tests/RackCad.UI.Tests/FlowBedEditorWindowTests.cs | cut -d: -f3)"
echo "   tests/RackCad.Tests/I62F4*Tests.cs: $(git ls-tree -r --name-only "$H" tests/RackCad.Tests | grep -c -E 'I62F4.*Tests\.cs$')"
echo "   esquemas I62 citados presentes: $(for s in preflight.v1 relay-record.v2 controller-verification.v2 binding.v1 gate-contract.v2 delegation.v2 role-invocation.v1 input-closure.v1 input-fidelity.v1 architect-review-result.v1 reviewer-result.v1 automation-state.v2 normative-dependency-manifest.v1; do git cat-file -e "$H:docs/automation/agent-execution/schemas/$s.schema.json" 2>/dev/null && echo -n 1 || echo -n 0; done) (13 = todos)"
echo "   adapters: $(git ls-tree --name-only "$H" docs/automation/agent-execution/adapters/ | wc -l)  esquemas de hechos: $(git ls-tree --name-only "$H" docs/automation/agent-execution/schemas/adapters/ | wc -l)  .gitignore 'artifacts/': $(git show "$H:.gitignore" | grep -c '^artifacts/')"
echo "## K-12 contrato: campos de frontmatter (Q11, READY-08)"
git show "$H:docs/initiatives/I-62-portabilidad-coordinador-principal.md" | sed -n '1,40p' | grep -E '^(status|extends|introduces|consumes|amendment_refs|ov_assignment_ref|requires_autocad|requires_owner_validation):'
echo "   menciones de A-2/A-3/A-4 en el contrato: $(git show "$H:docs/initiatives/I-62-portabilidad-coordinador-principal.md" | grep -c -E 'A-2|A-3|A-4')"
echo "   claves a2/a3/a4_status en el estado: $(git show "$H:docs/automation/state/I-62.yml" | grep -c -E '^  a[234]_status:')"
echo "# fin"
