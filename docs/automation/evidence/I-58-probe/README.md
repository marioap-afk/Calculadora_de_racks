# Ejecutar el probe

Desde la raiz del worktree:

```powershell
dotnet run --project docs/automation/evidence/I-58-probe/I58.Probe.csproj
```

Es un ejecutable de Discovery fuera de la solucion y de las suites canonicas. Sin paquetes nuevos.
No implementa reader ni equality, no escribe producto y no llama AutoCAD. El store real se usa SOLO
para fixtures/reopen y observaciones de perdida de unknowns. Los ports comparados son los reales de I-57.
Exit 1 representa expectativas futuras incumplidas y se debe conservar como RED; exit 2 si no selecciono
casos. Un fallo de setup/build no cuenta como RED. Resultados y SHA exacto en I-58-evidence.md.

El carrier ProbeInput es local al probe; no es API aprobada. Placement/context aqui son variaciones
opacas: la independencia efectiva del comparador futuro necesita la prueba ampliada CT58-03.
