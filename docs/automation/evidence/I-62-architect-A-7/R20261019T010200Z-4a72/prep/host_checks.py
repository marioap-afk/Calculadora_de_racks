"""Comprobaciones del host, solo lectura (como preflight-s89/host-checks.json del kit de A-6 ronda 4): checkout principal (git -C con
GIT_OPTIONAL_LOCKS=0: rama, HEAD, líneas de estado, origin/main local), worktree de I-64 (HEAD y líneas de estado), procesos (acad.exe,
accoreconsole.exe, dotnet.exe, testhost.exe, MSBuild.exe, VBCSCompiler.exe y cualquiera cuya línea de órdenes nombre la ruta de I-64 o el clon de la
revisión), memoria libre y carga. No escribe nada salvo su salida (JSON en stdout), sin el nombre de usuario.
Uso: python -I -B host_checks.py
"""
import datetime
import json
import os
import subprocess

UP = os.environ["USERPROFILE"]
MAIN = r"D:\Documentos\Codex\Calculadora de racks"
I64 = os.path.join(UP, ".codex", "worktrees", "architecture-workspace-persistente-rackcad")
CLONE = r"D:\r62-arch-a7"
ENV = dict(os.environ, GIT_OPTIONAL_LOCKS="0")


def g(repo, *a):
    r = subprocess.run(["git", "-C", repo] + list(a), capture_output=True, text=True, encoding="utf-8", errors="replace", env=ENV)
    return r.stdout.strip()


ps = subprocess.run(["pwsh", "-NoProfile", "-Command",
                     "Get-CimInstance Win32_Process | Select-Object Name,ProcessId,CommandLine,CreationDate | ConvertTo-Json -Compress"],
                     capture_output=True, text=True, encoding="utf-8", errors="replace").stdout
procs = json.loads(ps) if ps.strip() else []
watch = {"acad.exe", "accoreconsole.exe", "dotnet.exe", "testhost.exe", "msbuild.exe", "vbcscompiler.exe"}
found = []
for p in procs:
    cl = p.get("CommandLine") or ""
    hit_name = (p.get("Name") or "").lower() in watch
    hit_i64 = I64.lower() in cl.lower() or "architecture-workspace-persistente-rackcad" in cl.lower()
    hit_clone = CLONE.lower() in cl.lower() or "r62-arch-a7" in cl.lower()
    if hit_name or hit_i64 or hit_clone:
        found.append({"Name": p.get("Name"), "Pid": p.get("ProcessId"), "Started": str(p.get("CreationDate")),
                      "CommandLineNamesI64": hit_i64, "CommandLineNamesReviewClone": hit_clone,
                      "CommandLine": cl.replace(UP, "%USERPROFILE%")[:200]})
mem = subprocess.run(["pwsh", "-NoProfile", "-Command",
                      "$o = Get-CimInstance Win32_OperatingSystem; $c = (Get-CimInstance Win32_Processor | Measure-Object -Property LoadPercentage -Average).Average; "
                      "'{0:N1}|{1}' -f ($o.FreePhysicalMemory/1MB), $c"], capture_output=True, text=True).stdout.strip().split("|")
doc = {"Schema": "a7-host-checks/1", "UtcTime": datetime.datetime.now(datetime.timezone.utc).strftime("%Y-%m-%dT%H:%M:%SZ"),
       "MainCheckout": {"Path": "D:/Documentos/Codex/Calculadora de racks", "Branch": g(MAIN, "rev-parse", "--abbrev-ref", "HEAD"),
                        "Head": g(MAIN, "rev-parse", "HEAD"), "StatusLines": len([x for x in g(MAIN, "status", "--porcelain").split("\n") if x]),
                        "OriginMainLocal": g(MAIN, "rev-parse", "refs/remotes/origin/main"),
                        "Reads": "git -C rev-parse y status --porcelain con GIT_OPTIONAL_LOCKS=0"},
       "I64": {"Worktree": "architecture-workspace-persistente-rackcad", "Head": g(I64, "rev-parse", "--short", "HEAD"),
               "StatusLines": len([x for x in g(I64, "status", "--porcelain").split("\n") if x])},
       "Processes": found,
       "Load": {"FreeRamGB": mem[0] if mem else None, "CpuPercent": mem[1] if len(mem) > 1 else None}}
print(json.dumps(doc, ensure_ascii=False, indent=1))
