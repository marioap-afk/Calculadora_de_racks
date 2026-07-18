# CLAUDE.md

RackCad es un plugin de AutoCAD 2025 (.NET 8, C#/WPF). Este archivo es solo un índice de arranque;
no duplica estado, arquitectura ni proceso.

## Lectura inicial

1. [docs/HANDOFF.md](docs/HANDOFF.md): estado vivo, riesgos y siguiente acción.
2. [AGENTS.md](AGENTS.md): reglas técnicas obligatorias.
3. [docs/WORKFLOW.md](docs/WORKFLOW.md): ramas, worktrees e integración.
4. [docs/ARCHITECTURE.md](docs/ARCHITECTURE.md): arquitectura vigente y objetivo.
5. [docs/ROADMAP.md](docs/ROADMAP.md): iniciativas y dependencias.
6. El [Context Pack](docs/context-packs/README.md) declarado por la iniciativa.

## Comandos esenciales

```powershell
git status
git log --oneline -10
dotnet test tests/RackCad.Tests/RackCad.Tests.csproj
```

Para una validación de dibujo, usa el DLL Debug del worktree correspondiente y sigue
[docs/guias/validacion-manual-autocad.md](docs/guias/validacion-manual-autocad.md). Push de una rama
no significa integración; nunca trabajes directamente sobre `main`.
