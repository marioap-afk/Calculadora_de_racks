# CLAUDE.md

RackCad: plugin de AutoCAD 2025 (.NET 8, C#/WPF) para disenar y dibujar racks industriales con BOM.
Este archivo es un INDICE; no dupliques aqui estado temporal del proyecto.

## Orden de lectura

1. [docs/HANDOFF.md](docs/HANDOFF.md) — estado actual, trabajo reciente, bugs conocidos, siguientes tareas.
2. [README.md](README.md) — vista general, comandos de AutoCAD, build y NETLOAD.
3. [AGENTS.md](AGENTS.md) — convenciones obligatorias (arquitectura, copia centralizada de flags de
   seguridad, definicion de terminado, politica de push) y [docs/WORKFLOW.md](docs/WORKFLOW.md) —
   proceso de ramas por iniciativa, worktrees e integracion.
4. [docs/00-indice-contexto.md](docs/00-indice-contexto.md) — indice del resto de la documentacion.

## Comandos esenciales

```powershell
dotnet build RackCad.sln -v:minimal
dotnet test tests/RackCad.Tests/RackCad.Tests.csproj
```

El usuario prueba el plugin con NETLOAD de `src\RackCad.Plugin\bin\Debug\net8.0-windows\RackCad.Plugin.dll`
(el Debug, no el Release). Con AutoCAD abierto el DLL queda bloqueado y el build falla al copiar.

## Reglas de sesion

- Verifica SIEMPRE el estado real de Git (`git log --oneline -10`, `git status`) antes de asumir el estado
  descrito en cualquier documento o resumen: docs/HANDOFF.md refleja su fecha de actualizacion, no el presente.
- Bugfix => test de regresion verificado FALLANDO sin el fix.
- Push de la RAMA de iniciativa al cerrar cada sesion (respaldo); no INTEGRAR features a `main` sin
  la verificacion manual del usuario en AutoCAD (docs/WORKFLOW.md secciones 4 y 6).
- docs/HANDOFF.md secciones 8-12 se actualizan al INTEGRAR la iniciativa (ultimo commit de la rama,
  sesion de integracion), no al cerrar sesiones intermedias.
