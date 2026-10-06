"""I-62 F6, fixture D.1 steps 1-4 (plane c): bare origin, supervisor clone, F_seed (byte-exact authorities of MC_I62 + FIXTURE_LOCAL files),
F_norm with the normative trailer merged --no-ff as F_eff (TEST-ACTIVATION), and the claim of FX-U1 on fx/u1. Synthetic identity, repo-local config only.
Never touches RackCad except reading blobs of MC_I62 from the I-62 worktree."""
import hashlib
import json
import os
import subprocess
import sys

RACKCAD = r"~\.codex\worktrees\architecture-portabilidad-coordinador-principal"
MC_I62 = "6f0187cb30852971b49153c2763c8ba6caeaa64d"
ROOT = r"D:\r62-fixture"
ORIGIN = os.path.join(ROOT, "fixture-origin.git")
SUP = os.path.join(ROOT, "supervisor")
SEED_PATHS = ["docs/WORKFLOW.md", "docs/INITIATIVE_LIFECYCLE.md", "docs/AUTOMATION_PLAN.md", "docs/automation/agent-execution"]
CLAIM_ID = "fc62f1c7-0000-4000-8000-000000000001"
ENV = {**os.environ, "GIT_AUTHOR_NAME": "fixture", "GIT_AUTHOR_EMAIL": "fixture@example.invalid", "GIT_COMMITTER_NAME": "fixture",
       "GIT_COMMITTER_EMAIL": "fixture@example.invalid"}


def git(cwd, *args, data=None):
    r = subprocess.run(["git", "-C", cwd] + list(args), input=data, capture_output=True, env=ENV)
    if r.returncode != 0:
        raise SystemExit("git %s failed: %s" % (" ".join(args), r.stderr.decode("utf-8", "replace")))
    return r.stdout


def write(rel, content):
    p = os.path.join(SUP, rel.replace("/", os.sep))
    os.makedirs(os.path.dirname(p), exist_ok=True)
    data = content if isinstance(content, bytes) else content.encode("utf-8")
    with open(p, "wb") as f:
        f.write(data)


def blob_id(data):
    return hashlib.sha1(b"blob %d\0" % len(data) + data).hexdigest()


assert not os.path.exists(ROOT), "D:\\r62-fixture already exists: inspect before using"
os.makedirs(ROOT)
os.makedirs(os.path.join(ROOT, "evidence-out"))
git(ROOT, "init", "-q", "--bare", "-b", "main", ORIGIN)
git(ROOT, "clone", "-q", ORIGIN, SUP)
for k, v in (("core.autocrlf", "false"), ("user.name", "fixture"), ("user.email", "fixture@example.invalid")):
    git(SUP, "config", k, v)

# ---- F_seed: byte-exact copies of the authorities at MC_I62
entries = []
listing = git(RACKCAD, "ls-tree", "-r", MC_I62, "--", *SEED_PATHS).decode("utf-8").splitlines()
for line in listing:
    meta, path = line.split("\t", 1)
    mode, kind, sha = meta.split()
    assert kind == "blob"
    data = git(RACKCAD, "cat-file", "blob", sha)
    assert blob_id(data) == sha
    write(path, data)
    entries.append({"Path": path, "Kind": "COPIED", "SourceBlob": sha})

AGENTS = """# AGENTS.md — fixture de prueba de I-62 (FIXTURE_LOCAL)

Este repositorio es el **sistema bajo prueba** (plano c) de los pilotos F6 de I-62. No es RackCad. Sus reglas I62 rigen **solo aquí**, activadas por
el merge marcado `TEST-ACTIVATION` (ver `FIXTURE-MANIFEST.json`). Nada de este repositorio acredita un gate, un READY, una aprobación del Owner o una
integración de RackCad, ni puede cambiar su `main`, sus contadores, sus decisiones o su custodia (P-16).

## Leer primero

1. [docs/WORKFLOW.md](docs/WORKFLOW.md) — flujo de iniciativas, punto de entrada de compatibilidad (§12).
2. [docs/AUTOMATION_PLAN.md](docs/AUTOMATION_PLAN.md) — ejecución delegada (§16) y su resolver (16.13).
3. [docs/automation/agent-execution/README.md](docs/automation/agent-execution/README.md) — procedimientos.
4. Verificar el estado REAL con `git log --oneline -10` antes de asumir nada.

## Comandos canonicos

```powershell
git status
git log --oneline -10
dotnet test tests/Fixture.Tests/Fixture.Tests.csproj
```

## Jobs requeridos

La CI de este repositorio tiene **dos** jobs requeridos: `fixture-build` y `fixture-tests` (`.github/workflows/fixture.yml`). El job de pruebas
designado es `fixture-tests`: una corrida RED es la que lo tiene en `failure`. `Ci` pass exige los dos en `success` en la corrida `push` del SHA exacto.

## Identidades y secretos

Autor y committer sintéticos (`fixture <fixture@example.invalid>`). Ningún secreto real. Los `Claim-Id` del fixture empiezan por `fc62f1c7-`.
"""

CLAUDE = """# CLAUDE.md — fixture de prueba de I-62 (FIXTURE_LOCAL)

## Lectura inicial

1. [AGENTS.md](AGENTS.md): reglas del fixture y jobs requeridos.

## Comandos esenciales

```powershell
git status
dotnet test tests/Fixture.Tests/Fixture.Tests.csproj
```
"""

README = """# rackcad-i62-fixture

Fixture de prueba (plano c) de los pilotos F6 de la iniciativa I-62. Ver `AGENTS.md` y `FIXTURE-MANIFEST.json`.
"""

LIB_CSPROJ = """<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>
</Project>
"""

LIB_CS = """namespace Fixture.Lib;

/// <summary>Biblioteca mínima del fixture: el objeto de trabajo de las tareas de prueba.</summary>
public static class Calculator
{
    public static int Add(int a, int b) => a + b;
}
"""

TESTS_CSPROJ = """<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <IsPackable>false</IsPackable>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.11.1" />
    <PackageReference Include="xunit" Version="2.9.2" />
    <PackageReference Include="xunit.runner.visualstudio" Version="2.8.2" />
  </ItemGroup>
  <ItemGroup>
    <ProjectReference Include="..\\..\\src\\Fixture.Lib\\Fixture.Lib.csproj" />
  </ItemGroup>
</Project>
"""

TESTS_CS = """using Fixture.Lib;
using Xunit;

namespace Fixture.Tests;

public class CalculatorTests
{
    [Fact]
    public void AddSumsTwoIntegers() => Assert.Equal(5, Calculator.Add(2, 3));
}
"""

WORKFLOW = """name: fixture
on:
  push:
permissions:
  contents: read
jobs:
  fixture-build:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '8.0.x'
      - run: dotnet build tests/Fixture.Tests/Fixture.Tests.csproj -c Release
  fixture-tests:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '8.0.x'
      - run: dotnet test tests/Fixture.Tests/Fixture.Tests.csproj -c Release
"""

local = {
    "AGENTS.md": (AGENTS, "AGENTS del fixture: autoridades leídas primero y jobs requeridos del fixture (D.1)"),
    "CLAUDE.md": (CLAUDE, "entrada automática de las sesiones Claude del fixture; sin hechos de ninguna unidad (D.6)"),
    "README.md": (README, "descripción del fixture"),
    ".gitattributes": ("* -text\n", "bytes idénticos en todo clon: las copias conservan su blob de MC_I62"),
    ".github/workflows/fixture.yml": (WORKFLOW, "CI del fixture: fixture-build y fixture-tests (D.2), contents: read, sin secretos"),
    "src/Fixture.Lib/Fixture.Lib.csproj": (LIB_CSPROJ, "biblioteca .NET mínima (D.1)"),
    "src/Fixture.Lib/Calculator.cs": (LIB_CS, "biblioteca .NET mínima (D.1)"),
    "tests/Fixture.Tests/Fixture.Tests.csproj": (TESTS_CSPROJ, "pruebas xUnit del fixture (D.1)"),
    "tests/Fixture.Tests/CalculatorTests.cs": (TESTS_CS, "pruebas xUnit del fixture (D.1)"),
}
for path, (content, reason) in local.items():
    write(path, content)
    entries.append({"Path": path, "Kind": "FIXTURE_LOCAL", "Reason": reason})

manifest = {
    "Schema": "rackcad-fixture-manifest/v1",
    "Purpose": "I-62 F6, plano (c): sistema bajo prueba",
    "SourceCommit": MC_I62,
    "SourceRule": "MC_I62 = cierre de F4 (Proposal V14 §15); copias byte a byte de las autoridades",
    "Entries": sorted(entries + [{"Path": "FIXTURE-MANIFEST.json", "Kind": "FIXTURE_LOCAL", "Reason": "este manifiesto (D.1)"}], key=lambda e: e["Path"]),
    "Activation": None,
}
write("FIXTURE-MANIFEST.json", json.dumps(manifest, ensure_ascii=False, indent=2) + "\n")
git(SUP, "add", "-A")
git(SUP, "commit", "-q", "-m", "F_seed: autoridades de MC_I62 (byte a byte) y archivos FIXTURE_LOCAL")
f_seed = git(SUP, "rev-parse", "HEAD").decode().strip()
git(SUP, "push", "-q", "origin", "main")

# ---- F_norm on fixture/i62-norm, merged --no-ff as F_eff (TEST-ACTIVATION)
git(SUP, "checkout", "-q", "-b", "fixture/i62-norm")
manifest["Activation"] = {"Marker": "TEST-ACTIVATION", "Trailer": "Agent-Protocol-Normative: I-62",
                          "Rule": "AUTOMATION_PLAN 16.14 aplicado a ESTE repositorio: I62_EFFECTIVE_SHA del fixture = primer merge en first-parent de main "
                                  "cuyo segundo padre alcanza el único commit con el trailer; RackCad no se activa (derivación por repositorio)",
                          "Tag": "test-activation/I-62"}
write("FIXTURE-MANIFEST.json", json.dumps(manifest, ensure_ascii=False, indent=2) + "\n")
git(SUP, "add", "-A")
git(SUP, "commit", "-q", "-m", "F_norm: activación de prueba de las reglas I62 en el fixture (TEST-ACTIVATION)\n\nAgent-Protocol-Normative: I-62")
f_norm = git(SUP, "rev-parse", "HEAD").decode().strip()
git(SUP, "checkout", "-q", "main")
pre = ("Merge fixture/i62-norm: TEST-ACTIVATION de I-62 en el fixture\n\n"
       "TEST-ACTIVATION (plano c; no activa RackCad).\n\n"
       "Derived formal claim table:\n"
       "Initiative | ref | tip | original claim commit | Claim-Id | temporal evidence | transition\n")
git(SUP, "merge", "-q", "--no-ff", "-m", pre, "fixture/i62-norm")
f_eff = git(SUP, "rev-parse", "HEAD").decode().strip()
git(SUP, "tag", "-a", "test-activation/I-62", "-m", "I62_EFFECTIVE_SHA del fixture (TEST-ACTIVATION)", f_eff)
git(SUP, "push", "-q", "origin", "main", "fixture/i62-norm", "refs/tags/test-activation/I-62")

# ---- FX-U1 claim on fx/u1 from F_eff
git(SUP, "checkout", "-q", "-b", "fx/u1", f_eff)
git(SUP, "commit", "-q", "--allow-empty", "-m", "claim: FX-U1 (unidad de prueba del fixture)\n\nInitiative: FX-U1\nBranch: fx/u1\nClaim-Id: " + CLAIM_ID)
claim = git(SUP, "rev-parse", "HEAD").decode().strip()
git(SUP, "push", "-q", "origin", "fx/u1")

out = {"Root": ROOT, "Origin": ORIGIN, "SourceCommit": MC_I62, "F_seed": f_seed, "F_norm": f_norm, "F_eff": f_eff, "FX_U1_Claim": claim,
       "ClaimId": CLAIM_ID, "CopiedFiles": sum(1 for e in entries if e["Kind"] == "COPIED"), "LocalFiles": len(local) + 1}
print(json.dumps(out, indent=2))
with open(os.path.join(ROOT, "evidence-out", "fixture-identity.json"), "w", encoding="utf-8", newline="\n") as f:
    json.dump(out, f, indent=2)
