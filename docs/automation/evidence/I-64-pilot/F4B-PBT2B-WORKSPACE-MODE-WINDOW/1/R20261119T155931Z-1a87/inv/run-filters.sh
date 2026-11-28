#!/usr/bin/env bash
# Uso: DOTNET=<dotnet.exe> run-filters.sh <RunRef> <core|ui|all>
# Corre, uno a uno y sin recompilar, los filtros del RequiredTests del contrato (Debug) y deja un TRX y un resumen por filtro.
# Se ejecuta desde la raiz del worktree; las rutas son relativas a ella.
set -u
REF="$1"
WHICH="${2:-all}"
OUT="artifacts/orchestration/I-64/F4B-PBT2B-WORKSPACE-MODE-WINDOW/1/R20261119T155931Z-1a87/tests/$REF"
mkdir -p "$OUT"
SUMMARY="$OUT/summary.txt"
: > "$SUMMARY"

CORE=(
  "RackCad.Tests.PushBackWorkspaceModeSourceGuardTests"
  "RackCad.Tests.PushBackWorkspaceSharedReferenceGuardTests"
  "RackCad.Tests.PushBackDraftStatePortTests"
  "RackCad.Tests.PushBackDraftPortSharedReferenceGuardTests"
  "RackCad.Tests.PushBackWindowOnStateSourceGuardTests"
  "RackCad.Tests.PushBackWindowSharedReferenceGuardTests"
  "RackCad.Tests.PushBackSharedReferenceGuardTests"
  "RackCad.Tests.PushBackEditableState"
  "RackCad.Tests.PushBackEditorOutputGate"
  "RackCad.Tests.PushBackClassicCharacterization"
  "RackCad.Tests.PushBackFullEditorCharacterization"
  "RackCad.Tests.PushBackModuleEditorCharacterizationTests"
  "RackCad.Tests.I55DeferredProductFixCharacterizationTests"
  "RackCad.Tests.NamespaceFolderGuardTests"
  "RackCad.Tests.Workspace"
)
UI=(
  "RackCad.UI.Tests.PushBackWorkspaceModeTests"
  "RackCad.UI.Tests.PushBackWorkspaceApplyCloseTests"
  "RackCad.UI.Tests.PushBackFieldValidationConcordanceTests"
  "RackCad.UI.Tests.PushBackWindowOnStateTests"
  "RackCad.UI.Tests.PushBackEditableStateParityTests"
  "RackCad.UI.Tests.PushBackStateNet"
  "RackCad.UI.Tests.PushBackClassicCharacterization"
  "RackCad.UI.Tests.PushBackFullEditorCharacterization"
  "RackCad.UI.Tests.PushBackDeclaredCorrection"
  "RackCad.UI.Tests.PushBackCompositeWindowTests"
  "RackCad.UI.Tests.PushBackCompositeLifecycleWiringTests"
  "RackCad.UI.Tests.PushBackCompositeLoadExistingModulesTests"
  "RackCad.UI.Tests.PushBackBothModeCellBeamOverrideTests"
  "RackCad.UI.Tests.PushBackModuleEditorWindowTests"
  "RackCad.UI.Tests.PushBackAdvancedRackParametersWindowTests"
  "RackCad.UI.Tests.PushBackRichPreviewTests"
  "RackCad.UI.Tests.PushBackLibrarySaveGateTests"
  "RackCad.UI.Tests.DialogWindowCharacterizationTests"
  "RackCad.UI.Tests.BlankFront"
  "RackCad.UI.Tests.WindowCensusGuardTests"
  "RackCad.UI.Tests.RichEditor"
  "RackCad.UI.Tests.Workspace"
  "RackCad.UI.Tests.Cantilever"
)

run_one() {
  local project="$1" filter="$2" index="$3"
  local trx="f${index}.trx"
  "$DOTNET" test "$project" --no-build --filter "FullyQualifiedName~$filter" \
    --logger "trx;LogFileName=$trx" --results-directory "$OUT" \
    --blame-hang-timeout 10m --blame-hang-dump-type none > "$OUT/f${index}.log" 2>&1
  local line
  line=$(grep -E "(Correctas|Con error)! - " "$OUT/f${index}.log" | tail -1)
  echo "$filter | $trx | $line" >> "$SUMMARY"
}

i=0
if [ "$WHICH" = "core" ] || [ "$WHICH" = "all" ]; then
  for f in "${CORE[@]}"; do i=$((i+1)); run_one tests/RackCad.Tests/RackCad.Tests.csproj "$f" "c$i"; done
fi
if [ "$WHICH" = "ui" ] || [ "$WHICH" = "all" ]; then
  i=0
  for f in "${UI[@]}"; do i=$((i+1)); run_one tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj "$f" "u$i"; done
fi
echo "FIN" >> "$SUMMARY"
