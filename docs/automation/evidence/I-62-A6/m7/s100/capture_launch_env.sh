#!/usr/bin/env bash
# Preflight estático del kit v2 (decisiones §100, punto 4): captura el entorno del hijo exactamente como run_m7.sh (PATH = directorio del pwsh
# del runtime + PATH de Windows del registro; sin GIT_*), sin lanzar nada. El volcado contiene rutas del perfil: no se custodia.
#   bash capture_launch_env.sh <kit> <salida.json>
set -u
K="$1"; OUTF="$2"
PY="$LOCALAPPDATA/Microsoft/WindowsApps/python.exe"
RTPS_DIR_WIN="$("$PY" -I -c "import json,os,sys; p=json.load(open(sys.argv[1],encoding='utf-8'))['RuntimePwsh']['Path']; print(os.path.dirname(os.path.expandvars(p.replace('%USERPROFILE%','\$USERPROFILE').replace('%LOCALAPPDATA%','\$LOCALAPPDATA'))))" "$(cygpath -w "$K/m7-params.json")")"
WINPATH="$(powershell.exe -NoProfile -Command "[Environment]::GetEnvironmentVariable('Path','Machine') + ';' + [Environment]::GetEnvironmentVariable('Path','User')" | tr -d '\r')"
[ -n "$WINPATH" ] || { echo "Windows PATH unreadable" >&2; exit 2; }
CHILD_PATH="$(cygpath -u "$RTPS_DIR_WIN"):$(cygpath -u -p "$WINPATH")"
UNSET=()
for v in $(compgen -e); do case "${v^^}" in GIT_*) UNSET+=(-u "$v") ;; esac; done
env "${UNSET[@]}" PATH="$CHILD_PATH" MSYS2_ARG_CONV_EXCL='*' MSYS_NO_PATHCONV=1 "$PY" -I -c "import json,os,sys; sys.stdout.write(json.dumps(dict(os.environ), ensure_ascii=True, sort_keys=True))" > "$OUTF"
