"""Comprobaciones del host para el kit de la ronda 3, solo lectura (variante de host_checks_r2.py del kit de la ronda 2): checkout principal (git -C
con GIT_OPTIONAL_LOCKS=0: rama, HEAD, líneas de estado, origin/main local), worktree de I-64 y worktree de I-62 (HEAD y líneas de estado), clon de
la ronda 3 y de la ronda 2 (HEAD y estado con ignorados), el run de la ronda 2 (número de entradas y fecha de modificación más reciente, para
mostrar que no se tocó), binarios de claude-code instalados por la app (nombres de versión; el medido es 2.1.299), procesos (acad.exe,
accoreconsole.exe, dotnet.exe, testhost.exe, MSBuild.exe, VBCSCompiler.exe, claude.exe con «-p» y cualquiera cuya línea de órdenes nombre la ruta
de I-64 o un clon D:\\r62-arch-a7*), memoria libre y carga. Salida: JSON en stdout, sin el nombre de usuario. Uso: python -I -B host_checks_r3.py
"""
import datetime
import json
import os
import subprocess

UP = os.environ["USERPROFILE"]
MAIN = r"D:\Documentos\Codex\Calculadora de racks"
I64 = os.path.join(UP, ".codex", "worktrees", "architecture-workspace-persistente-rackcad")
W = os.path.join(UP, ".codex", "worktrees", "architecture-portabilidad-coordinador-principal")
R3, R2, R2RUN = r"D:\r62-arch-a7r3", r"D:\r62-arch-a7r2", r"D:\r62-arch-a7r2-run"
CC = os.path.join(os.environ.get("APPDATA", ""), "Claude", "claude-code")
ENV = dict(os.environ, GIT_OPTIONAL_LOCKS="0")


def g(repo, *a):
    r = subprocess.run(["git", "-C", repo] + list(a), capture_output=True, text=True, encoding="utf-8", errors="replace", env=ENV)
    return r.stdout.strip()


def lines(s):
    return len([x for x in s.split("\n") if x])


ps = subprocess.run(["pwsh", "-NoProfile", "-Command",
                     "Get-CimInstance Win32_Process | Select-Object Name,ProcessId,CommandLine,CreationDate | ConvertTo-Json -Compress"],
                    capture_output=True, text=True, encoding="utf-8", errors="replace").stdout
procs = json.loads(ps) if ps.strip() else []
watch = {"acad.exe", "accoreconsole.exe", "dotnet.exe", "testhost.exe", "msbuild.exe", "vbcscompiler.exe"}
found = []
for p in procs:
    cl, name = p.get("CommandLine") or "", (p.get("Name") or "").lower()
    hit = name in watch or (name == "claude.exe" and " -p" in cl) or "architecture-workspace-persistente-rackcad" in cl.lower() or \
        "r62-arch-a7" in cl.lower()
    if hit:   # sin argumentos: pueden nombrar archivos de terceros (p. ej., el documento abierto en AutoCAD) o la ruta del perfil en otra forma
        exe = cl[1:cl.index('"', 1)] if cl.startswith('"') and '"' in cl[1:] else (cl.split(" ")[0] if cl else "")
        found.append({"Name": p.get("Name"), "Pid": p.get("ProcessId"), "Started": str(p.get("CreationDate")),
                      "Executable": exe.replace(UP, "%USERPROFILE%"), "MentionsI64": "architecture-workspace-persistente-rackcad" in cl.lower(),
                      "MentionsCloneR62ArchA7": "r62-arch-a7" in cl.lower(), "IsThisSessionShell": name == "bash.exe" and "r62-arch-a7" in cl.lower()})
mem = subprocess.run(["pwsh", "-NoProfile", "-Command",
                      "$o = Get-CimInstance Win32_OperatingSystem; $c = (Get-CimInstance Win32_Processor | Measure-Object -Property LoadPercentage -Average).Average; "
                      "'{0:N1}|{1}' -f ($o.FreePhysicalMemory/1MB), $c"], capture_output=True, text=True).stdout.strip().split("|")
r2run = [os.path.join(R2RUN, n) for n in os.listdir(R2RUN)] if os.path.isdir(R2RUN) else []
newest = max((os.path.getmtime(p) for p in r2run), default=None)
doc = {"Schema": "a7r3-host-checks/1", "UtcTime": datetime.datetime.now(datetime.timezone.utc).strftime("%Y-%m-%dT%H:%M:%SZ"),
       "MainCheckout": {"Branch": g(MAIN, "rev-parse", "--abbrev-ref", "HEAD"), "Head": g(MAIN, "rev-parse", "HEAD"),
                        "StatusLines": lines(g(MAIN, "status", "--porcelain")), "OriginMainLocal": g(MAIN, "rev-parse", "refs/remotes/origin/main")},
       "I64": {"Worktree": "architecture-workspace-persistente-rackcad", "Head": g(I64, "rev-parse", "--short", "HEAD"),
               "StatusLines": lines(g(I64, "status", "--porcelain"))},
       "W": {"Worktree": "architecture-portabilidad-coordinador-principal", "Head": g(W, "rev-parse", "HEAD"),
             "StatusLines": lines(g(W, "status", "--porcelain"))},
       "R3Clone": {"Path": R3, "Head": g(R3, "rev-parse", "HEAD"), "StatusLinesWithIgnored": lines(g(R3, "status", "--porcelain", "--ignored"))},
       "R2Clone": {"Path": R2, "Head": g(R2, "rev-parse", "HEAD"), "StatusLinesWithIgnored": lines(g(R2, "status", "--porcelain", "--ignored"))},
       "R2Run": {"Path": R2RUN, "Entries": len(r2run),
                 "NewestMtimeUtc": datetime.datetime.fromtimestamp(newest, datetime.timezone.utc).strftime("%Y-%m-%dT%H:%M:%SZ") if newest else None},
       "ClaudeCodeVersionsInstalled": sorted(os.listdir(CC)) if os.path.isdir(CC) else [],
       "Processes": found,
       "Load": {"FreeRamGB": mem[0] if mem else None, "CpuPercent": mem[1] if len(mem) > 1 else None}}
print(json.dumps(doc, ensure_ascii=False, indent=1))
