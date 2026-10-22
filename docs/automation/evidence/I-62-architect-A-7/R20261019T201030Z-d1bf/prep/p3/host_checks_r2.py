"""Comprobaciones del host para el kit de la ronda 2, solo lectura (variante de prep/host_checks.py): checkout principal (git -C con
GIT_OPTIONAL_LOCKS=0: rama, HEAD, líneas de estado, origin/main local), worktree de I-64 y worktree de I-62 (HEAD y líneas de estado), clon de la
ronda 2 y de la ronda 1 (HEAD y estado con ignorados), el run de la ronda 1 (número de archivos y fecha de modificación más reciente, para
mostrar que no se tocó), procesos (acad.exe, accoreconsole.exe, dotnet.exe, testhost.exe, MSBuild.exe, VBCSCompiler.exe, claude.exe con «-p» y
cualquiera cuya línea de órdenes nombre la ruta de I-64 o un clon D:\\r62-arch-a7*), memoria libre y carga. Salida: JSON en stdout, sin el nombre
de usuario. Uso: python -I -B host_checks_r2.py
"""
import datetime
import json
import os
import subprocess

UP = os.environ["USERPROFILE"]
MAIN = r"D:\Documentos\Codex\Calculadora de racks"
I64 = os.path.join(UP, ".codex", "worktrees", "architecture-workspace-persistente-rackcad")
W = os.path.join(UP, ".codex", "worktrees", "architecture-portabilidad-coordinador-principal")
R2, R1, R1RUN = r"D:\r62-arch-a7r2", r"D:\r62-arch-a7", r"D:\r62-arch-a7-run"
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
    if hit:
        found.append({"Name": p.get("Name"), "Pid": p.get("ProcessId"), "Started": str(p.get("CreationDate")),
                      "CommandLine": cl.replace(UP, "%USERPROFILE%")[:200]})
mem = subprocess.run(["pwsh", "-NoProfile", "-Command",
                      "$o = Get-CimInstance Win32_OperatingSystem; $c = (Get-CimInstance Win32_Processor | Measure-Object -Property LoadPercentage -Average).Average; "
                      "'{0:N1}|{1}' -f ($o.FreePhysicalMemory/1MB), $c"], capture_output=True, text=True).stdout.strip().split("|")
r1run = [os.path.join(R1RUN, n) for n in os.listdir(R1RUN)] if os.path.isdir(R1RUN) else []
newest = max((os.path.getmtime(p) for p in r1run), default=None)
doc = {"Schema": "a7r2-host-checks/1", "UtcTime": datetime.datetime.now(datetime.timezone.utc).strftime("%Y-%m-%dT%H:%M:%SZ"),
       "MainCheckout": {"Branch": g(MAIN, "rev-parse", "--abbrev-ref", "HEAD"), "Head": g(MAIN, "rev-parse", "HEAD"),
                        "StatusLines": lines(g(MAIN, "status", "--porcelain")), "OriginMainLocal": g(MAIN, "rev-parse", "refs/remotes/origin/main")},
       "I64": {"Worktree": "architecture-workspace-persistente-rackcad", "Head": g(I64, "rev-parse", "--short", "HEAD"),
               "StatusLines": lines(g(I64, "status", "--porcelain"))},
       "W": {"Worktree": "architecture-portabilidad-coordinador-principal", "Head": g(W, "rev-parse", "HEAD"),
             "StatusLines": lines(g(W, "status", "--porcelain"))},
       "R2Clone": {"Path": R2, "Head": g(R2, "rev-parse", "HEAD"), "StatusLinesWithIgnored": lines(g(R2, "status", "--porcelain", "--ignored"))},
       "R1Clone": {"Path": R1, "Head": g(R1, "rev-parse", "HEAD"), "StatusLinesWithIgnored": lines(g(R1, "status", "--porcelain", "--ignored"))},
       "R1Run": {"Path": R1RUN, "Entries": len(r1run),
                 "NewestMtimeUtc": datetime.datetime.fromtimestamp(newest, datetime.timezone.utc).strftime("%Y-%m-%dT%H:%M:%SZ") if newest else None},
       "Processes": found,
       "Load": {"FreeRamGB": mem[0] if mem else None, "CpuPercent": mem[1] if len(mem) > 1 else None}}
print(json.dumps(doc, ensure_ascii=False, indent=1))
