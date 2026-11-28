#!/usr/bin/env bash
# Runs the filters of RequiredTests one by one over the Debug build already compiled (--no-build), and records the exact command of each,
# the summary line of the runner and a table (RunRef, Project, Filter, MinSelected, Total, Passed, Failed, Skipped).
# Usage: run_filters.sh <phase-prefix> <out-dir-relative-to-worktree> [first] [last]   (run from the root of the worktree)
# The UI filters are the ones of the contract only, one by one; never the whole suite of UI.
set -u
PHASE="$1"
OUTDIR="$2"
FIRST="${3:-1}"
LAST="${4:-38}"
DN=<datos-locales-del-usuario>/Microsoft/dotnet/dotnet.exe
CORE=tests/RackCad.Tests/RackCad.Tests.csproj
UI=tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj
mkdir -p "$OUTDIR"
FILTERS=(
"1|$CORE|RackCad.Tests.PushBackFullReadPathTests|5"
"2|$CORE|RackCad.Tests.PushBackFullReadGuardTests|3"
"3|$CORE|RackCad.Tests.PushBackViewEraseGuardTests|4"
"4|$CORE|RackCad.Tests.PushBackWritePathPluginGuardTests|1"
"5|$CORE|RackCad.Tests.WorkspacePilotUpdateGuardTests|86"
"6|$CORE|RackCad.Tests.WorkspacePilotUpdateIntegrationGuardTests|123"
"7|$CORE|RackCad.Tests.PushBackSharedReferenceGuardTests|2"
"8|$UI|RackCad.UI.Tests.PushBackFullDraftWindowParityTests|1"
"9|$CORE|RackCad.Tests.PushBackFullWrite|13"
"10|$CORE|RackCad.Tests.PushBackPilotUpdateEquivalenceGuardTests|6"
"11|$CORE|RackCad.Tests.PushBackWritePathGuardTests|2"
"12|$CORE|RackCad.Tests.Workspace|3261"
"13|$CORE|RackCad.Tests.PushBackFullDraft|10"
"14|$CORE|RackCad.Tests.PushBackDraftWriteGuardTests|2"
"15|$UI|RackCad.UI.Tests.Workspace|547"
"16|$CORE|RackCad.Tests.PushBackEditableState|13"
"17|$CORE|RackCad.Tests.PushBackEditorOutputGate|1"
"18|$CORE|RackCad.Tests.PushBackWindowSharedReferenceGuardTests|1"
"19|$CORE|RackCad.Tests.PushBackWindowOnStateSourceGuardTests|1"
"20|$UI|RackCad.UI.Tests.PushBackWindowOnStateTests|2"
"21|$UI|RackCad.UI.Tests.PushBackClassicCharacterization|55"
"22|$CORE|RackCad.Tests.PushBackClassicCharacterization|10"
"23|$UI|RackCad.UI.Tests.PushBackFullEditorCharacterization|16"
"24|$CORE|RackCad.Tests.PushBackFullEditorCharacterization|3"
"25|$UI|RackCad.UI.Tests.PushBackStateNet|14"
"26|$UI|RackCad.UI.Tests.PushBackDeclaredCorrection|14"
"27|$UI|RackCad.UI.Tests.Cantilever|67"
"28|$CORE|RackCad.Tests.I58F1|10"
"29|$CORE|RackCad.Tests.CustomPropertiesEnvelopeGuardTests|9"
"30|$CORE|RackCad.Tests.NamespaceFolderGuardTests|7"
"31|$CORE|RackCad.Tests.PushBackDraftStatePortTests|3"
"32|$CORE|RackCad.Tests.PushBackDraftPortSharedReferenceGuardTests|1"
"33|$CORE|RackCad.Tests.PushBackWorkspaceModeSourceGuardTests|1"
"34|$CORE|RackCad.Tests.PushBackWorkspaceSharedReferenceGuardTests|1"
"35|$UI|RackCad.UI.Tests.PushBackWorkspaceModeTests|3"
"36|$UI|RackCad.UI.Tests.PushBackWorkspaceApplyCloseTests|4"
"37|$UI|RackCad.UI.Tests.PushBackFieldValidationConcordanceTests|1"
"38|$UI|RackCad.UI.Tests.PushBackWorkspaceCarryoverTests|3"
)
for entry in "${FILTERS[@]}"; do
  IFS='|' read -r n project filter minsel <<<"$entry"
  if [ "$n" -lt "$FIRST" ] || [ "$n" -gt "$LAST" ]; then continue; fi
  ref="$PHASE-$n"
  cmd="$DN test $project --no-build --filter \"FullyQualifiedName~$filter\" --logger \"trx;LogFileName=$ref.trx\" --results-directory $OUTDIR --blame-hang-timeout 10m --blame-hang-dump-type none"
  echo "$ref | $cmd" >> "$OUTDIR/commands.txt"
  "$DN" test "$project" --no-build --filter "FullyQualifiedName~$filter" --logger "trx;LogFileName=$ref.trx" --results-directory "$OUTDIR" --blame-hang-timeout 10m --blame-hang-dump-type none > "$OUTDIR/$ref.log" 2>&1
  summary=$(grep -E "^(Correctas|Con error)!" "$OUTDIR/$ref.log" | tail -1)
  echo "$ref	$project	$filter	min=$minsel	$summary" | tee -a "$OUTDIR/summary.tsv"
done
