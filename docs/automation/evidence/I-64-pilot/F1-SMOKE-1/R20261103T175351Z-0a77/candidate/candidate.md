# I-64 — Candidato del reintento de Smoke-1 (preparación; transitorio, ignorado por Git)

Bloque del Candidato (guía de validación manual §7.1, ampliado por T3D §3). Evidencia local producida DESPUÉS del commit, con el
árbol limpio y sobre ese HEAD exacto, con AutoCAD cerrado (0 procesos `acad` antes y después). Rutas locales como `<worktree>`.

```text
Candidate SHA:        09f2f1c3c4e64491018ef1489ce4dd3030d1e4a8
Producto:             src/ y tests/ = b874705e26fff203e8727f6ed2362eca497b5860 (C1 PASS); frente a 4d79e910 solo cambian WorkspacePaletteHost.cs y sus guardas
Arbol limpio:         SI   (git status --porcelain: 0 lineas antes y despues)
SDK resuelto:         8.0.423
Core Full local:      PASS (13126/13126; 14:02:28Z-14:05:23Z)
UI Full local:        PASS (1701/1718, 17 Skip existentes, 0 con error; 14:05:23Z-14:10:14Z)
Debug UI build:       PASS (0 advertencias)
Debug Plugin build:   PASS (0 errores; solo MSB3277, conocidas)
CI exact SHA:         GREEN (runs 38409865067 y 38409861265, ambas push sobre 09f2f1c3; los cuatro jobs success, incluido "UI Tests (WPF controls, net8.0-windows)")
Owner validation:     pending (reintento de Smoke-1; NO autorizado)
Biblioteca de bloques: override de %APPDATA%\RackCad\settings.json, archivo Base_de_datos_AutoCAD_V.0.dwg  SHA-256 B4CA2248DB9C3D72487AC8B5B1E5510CDD8ABA231AB340541D91BEBCA2D560E8 (sin cambios)
DLL cargado:          <worktree>\src\RackCad.Plugin\bin\Debug\net8.0-windows\RackCad.Plugin.dll  SHA-256 2923E7F2DF4F62594810DBBC33003D36E6E4382EDECFB7E43C4A6C7C01F5AE26 (281600 bytes, 2026-10-13T14:10:26Z)
Otros DLL RackCad:    RackCad.Application.dll  SHA-256 66E88EB13EFC9312C33666852F31F782DB9F298735AB981EA692E915F6A5F8BD
                      RackCad.Domain.dll       SHA-256 44B4224B2D29DD5B28B56FEEBAFDA8AC0FD09D42667342E93606B4D7466CB483
                      RackCad.UI.dll           SHA-256 AEBF34C8ADD453F8556B8416BB59903F1801C250C2F02B6C96914C0FD5E87E9F
AutoCAD:              AutoCAD 2025 R25.0.171.0.0 (acad.exe SHA-256 2A75996FD2A5C5EE0376FD5BA7CCED227A098193FF495E8CEEDA3A96FE326422)
Candidato historico:  4d79e91036acdaa3e62b7b54677f394593f581a0 = HISTORICAL_SMOKE1_STOP_CANDIDATE (no se reutiliza)
```

**Dibujos** (sin cambios; revalidados el 2026-10-13):

| Dibujo | SHA-256 | Estado |
|---|---|---|
| A, `Dibujo A.dwg` | `90750e8b…4852` | sin cambios |
| B, `Dibujo B.dwg` | `fb9b97f9…682b` | sin cambios |
| C, `Dibujo C.dwg` | `ede7cdb3…a53c` | sin cambios |
| D, `I64-SMOKE1-FIXTURES-D.dxf` (PR-12 y PR-13), con su copia recuperable | `03e5606c…f165` | sin cambios |

El clasificador, el lector y los textos del panel no cambian frente a `4d79e910`. Releídos con el `RackCad.Application.dll` del Candidato
(`66E88EB1…`): PR-13 = `NoIdentity`, PR-12 = `Diagnostic`.

**Regla:** ningún commit de I-64 antes de la ocurrencia, porque cambiaría el SHA estampado en el DLL. No se recompila.
