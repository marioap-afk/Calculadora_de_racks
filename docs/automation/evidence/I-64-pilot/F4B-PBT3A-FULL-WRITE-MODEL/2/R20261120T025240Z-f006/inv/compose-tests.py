import glob
import json
import os
import re
import subprocess
import sys
import xml.etree.ElementTree as ET

# Compone TestsExecuted y TestResults (fragmento) a partir de los TSV y TRX de tests/. Solo lectura; rutas relativas al worktree.
RUN = "artifacts/orchestration/I-64/F4B-PBT3A-FULL-WRITE-MODEL/2/R20261120T025240Z-f006"
NS = {"t": "http://microsoft.com/schemas/VisualStudio/TeamTest/2010"}
TREE_BASE = subprocess.run(["git", "rev-parse", "485b5651bcec132761347b40ed38b60cfec0db04^{tree}"], capture_output=True, text=True).stdout.strip()
TREE_RED = open(RUN + "/tests/tree-red.txt").read().strip()
TREE_GREEN = open(RUN + "/tests/tree-green.txt").read().strip()
EXECUTED = []
RESULTS = []


def failed_names(trx):
    path = RUN + "/" + trx
    if not os.path.exists(path):
        return []
    root = ET.parse(path).getroot()
    return sorted(r.get("testName") for r in root.findall(".//t:UnitTestResult", NS) if r.get("outcome") == "Failed")


def add(ref, command, phase, tree, results_file, selected, passed, failed, skipped, trx=None):
    EXECUTED.append({"RunRef": ref, "Command": command, "Phase": phase, "TreeSha": tree, "ResultsFile": results_file})
    RESULTS.append({"RunRef": ref, "Selected": int(selected), "Passed": int(passed), "Failed": int(failed), "Skipped": int(skipped),
                    "FailedTests": failed_names(trx) if trx and int(failed) > 0 else []})


def tsv(folder, tag, phase_of, tree):
    for path in sorted(glob.glob(RUN + "/tests/" + folder + "/results-*.tsv")):
        for line in open(path, encoding="utf-8").read().splitlines():
            c = line.split("\t")
            _, kind, filt, minimum, expectred, total, passed, failed, skipped, exitcode, cmd, trxname = c
            phase = phase_of(expectred)
            cmd = cmd.replace(RUN + "/tests/" + folder, RUN + "/tests/" + folder)
            add(tag + "-" + trxname[:-4], cmd, phase, tree, "tests/" + folder + "/" + trxname, total, passed, failed, skipped,
                "tests/" + folder + "/" + trxname)


def build_entry(ref, folder, name, project, tree, extra=""):
    log = RUN + "/tests/" + folder + "/" + name + ".log"
    text = open(log, encoding="utf-8", errors="replace").read()
    warnings = len(set(re.findall(r".*: warning [A-Za-z]+[0-9]+.*", text)))
    errors = re.search(r"(\d+) Errores", text)
    cmd = "dotnet build " + project + " -c Release --no-incremental -v:minimal" + extra
    add(ref, cmd, "RELEVANT", tree, "tests/" + folder + "/" + name + ".log", 0, 0, 0, 0)
    return warnings, errors.group(1) if errors else "?"


# Linea base (BaseSha, sin escribir nada): tres builds Release.
for name, project, extra in (("build-core", "tests/RackCad.Tests/RackCad.Tests.csproj", ""), ("build-ui", "tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj", ""),
                             ("build-plugin", "src/RackCad.Plugin/RackCad.Plugin.csproj", " -o " + RUN + "/tests/b0/plugin-out")):
    build_entry("b0-" + name, "b0", name, project, TREE_BASE, extra)

# RED (arbol 8634d69f..., el que se confirmo como RedSha).
for name, project in (("debug-build-core", "tests/RackCad.Tests/RackCad.Tests.csproj"), ("debug-build-ui", "tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj")):
    EXECUTED.append({"RunRef": "r1-" + name, "Command": "dotnet build " + project + " -c Debug -v:minimal", "Phase": "RELEVANT", "TreeSha": TREE_RED,
                     "ResultsFile": "tests/r1/" + name + ".log"})
    RESULTS.append({"RunRef": "r1-" + name, "Selected": 0, "Passed": 0, "Failed": 0, "Skipped": 0, "FailedTests": []})
tsv("r1", "r1", lambda expectred: "RED" if expectred == "1" else "RELEVANT", TREE_RED)
m = re.search(r"Con error:\s+(\d+), Superado:\s+(\d+), Omitido:\s+(\d+), Total:\s+(\d+)", open(RUN + "/tests/r1/red-core-full.log", encoding="utf-8", errors="replace").read())
add("r1-red-core-full", "dotnet test tests/RackCad.Tests/RackCad.Tests.csproj --no-build --logger \"trx;LogFileName=red-core-full.trx\" --results-directory " + RUN + "/tests/r1 --blame-hang-timeout 10m --blame-hang-dump-type none",
    "RED", TREE_RED, "tests/r1/red-core-full.trx", m.group(4), m.group(2), m.group(1), m.group(3), "tests/r1/red-core-full.trx")
for name, project, extra in (("build-core", "tests/RackCad.Tests/RackCad.Tests.csproj", ""), ("build-ui", "tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj", ""),
                             ("build-plugin", "src/RackCad.Plugin/RackCad.Plugin.csproj", " -o " + RUN + "/tests/r2/plugin-out")):
    build_entry("r2-" + name, "r2", name, project, TREE_RED, extra)

# GREEN (arbol 0b5ab775...).
for name, project in (("debug-build-core", "tests/RackCad.Tests/RackCad.Tests.csproj"), ("debug-build-ui", "tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj")):
    EXECUTED.append({"RunRef": "g1-" + name, "Command": "dotnet build " + project + " -c Debug -v:minimal", "Phase": "RELEVANT", "TreeSha": TREE_GREEN,
                     "ResultsFile": "tests/g1/" + name + ".log"})
    RESULTS.append({"RunRef": "g1-" + name, "Selected": 0, "Passed": 0, "Failed": 0, "Skipped": 0, "FailedTests": []})
for folder in ("g1", "g2", "g3"):
    tsv(folder, folder, lambda expectred: "GREEN", TREE_GREEN)
m = re.search(r"Con error:\s+(\d+), Superado:\s+(\d+), Omitido:\s+(\d+), Total:\s+(\d+)", open(RUN + "/tests/g1/green-core-full.log", encoding="utf-8", errors="replace").read())
add("g1-green-core-full", "dotnet test tests/RackCad.Tests/RackCad.Tests.csproj --no-build --logger \"trx;LogFileName=green-core-full.trx\" --results-directory " + RUN + "/tests/g1 --blame-hang-timeout 10m --blame-hang-dump-type none",
    "GREEN", TREE_GREEN, "tests/g1/green-core-full.trx", m.group(4), m.group(2), m.group(1), m.group(3), "tests/g1/green-core-full.trx")
for name, project, extra in (("build-core", "tests/RackCad.Tests/RackCad.Tests.csproj", ""), ("build-ui", "tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj", ""),
                             ("build-plugin", "src/RackCad.Plugin/RackCad.Plugin.csproj", " -o " + RUN + "/tests/g4/plugin-out")):
    build_entry("g4-" + name, "g4", name, project, TREE_GREEN, extra)

# Rutas de artefactos: relativas al worktree y que empiezan por el prefijo de la entrega.
for item in EXECUTED:
    if item["ResultsFile"] and not item["ResultsFile"].startswith("artifacts/"):
        item["ResultsFile"] = RUN + "/" + item["ResultsFile"]
for item in EXECUTED:
    if item["ResultsFile"]:
        assert os.path.exists(item["ResultsFile"]), item["ResultsFile"]
refs = [r["RunRef"] for r in RESULTS]
assert len(refs) == len(set(refs)), "RunRef duplicados"
json.dump({"TestsExecuted": EXECUTED, "TestResults": RESULTS}, open(RUN + "/inv/tests-fragment.json", "w", encoding="utf-8"), indent=1, ensure_ascii=False)
print(len(EXECUTED), "corridas;", sum(1 for r in RESULTS if r["Failed"] > 0), "con fallos")
