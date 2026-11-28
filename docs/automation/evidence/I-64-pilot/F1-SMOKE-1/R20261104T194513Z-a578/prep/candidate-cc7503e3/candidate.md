# I-64 — Candidato del segundo reintento de Smoke-1 (preparación; transitorio, ignorado por Git)

Bloque del Candidato (guía de validación manual §7.1, ampliado por T3D §3), según las reglas del cierre de C2
(`F1-T3-HOST-C2/coordinator-close-review.md`). Evidencia local producida DESPUÉS del commit, con el árbol limpio y sobre ese HEAD exacto.
Rutas locales como `<worktree>`.

```text
Candidate SHA:        cc7503e3e4e56bff3a2fc9de7ca30a5322b57ea6
Producto:             src/ y tests/ = 0e0d536b10350cc85e4a932a0d576b22fae22fa9 (C2 PASS); 0 archivos de src/ y tests/ distintos
Arbol limpio:         SI   (git status --porcelain: 0 lineas antes y despues)
SDK resuelto:         8.0.423
Workspace enfocadas:  Core 577/577; UI 66/66
Core Full local:      PASS (13172/13172; 19:36:32Z-19:38:46Z)
UI Full local:        PASS (1701/1718, 17 Skip existentes, 0 con error; 19:38:46Z-19:44:05Z)
Debug UI build:       PASS (0 advertencias, 0 errores)
Debug Plugin build:   PASS (0 errores; 2 advertencias, ambas MSB3277 conocidas; 104 lineas MSB3277 en el log)
CI exact SHA:         GREEN (run 41898179409, push sobre cc7503e3, unico run de ese SHA; los cuatro jobs success, incluido "UI Tests (WPF controls, net8.0-windows)")
Owner validation:     pending (segundo reintento de Smoke-1; NO autorizado)
Biblioteca de bloques: override de %APPDATA%\RackCad\settings.json, archivo Base_de_datos_AutoCAD_V.0.dwg  SHA-256 B4CA2248DB9C3D72487AC8B5B1E5510CDD8ABA231AB340541D91BEBCA2D560E8 (sin cambios)
DLL a cargar:         <worktree>\src\RackCad.Plugin\bin\Debug\net8.0-windows\RackCad.Plugin.dll  SHA-256 3C261B8F156E69192415E0084D0BC8774B3080ACE9D6CB4B9BB0C1760549B6A0 (282112 bytes, 2026-11-04T19:44:16Z)
Otros DLL RackCad:    RackCad.Application.dll  SHA-256 12B8E237F70596FA2F8030D66E51CC54F410A8906CB78B61A4056873058E27B3 (2114048 bytes)
                      RackCad.Domain.dll       SHA-256 BFAA59433E5681A0F1453A765AE37B50ED2381993AFDE82628CCBDA7F8773959 (125952 bytes)
                      RackCad.UI.dll           SHA-256 363AE9FE436AAE7A1DAE7E5394D86D7EF0C6C6D137943BD594BC15340CBC76E1 (1029632 bytes)
AutoCAD:              AutoCAD 2025 R25.0.171.0.0 (acad.exe SHA-256 2A75996FD2A5C5EE0376FD5BA7CCED227A098193FF495E8CEEDA3A96FE326422)
acad.exe en ejecucion: 1 antes y 1 despues (AutoCAD del Owner, no lanzado por I-64; I-64 no lo uso); antes de la BASELINE se exige 0
Candidatos historicos: 4d79e910 = HISTORICAL_SMOKE1_STOP_CANDIDATE; 09f2f1c3 = HISTORICAL_SMOKE1_NOT_PASS_CANDIDATE (no se reutilizan)
```

El DLL de `0e0d536b` (`2C2433AB…7B4E`) no es el del Candidato: el SHA del commit se estampa en el ensamblado.

**Dibujos** (sin cambios; revalidados el 2026-11-04):

| Dibujo | SHA-256 | Estado |
|---|---|---|
| A, `Dibujo A.dwg` | `90750e8b…4852` | sin cambios |
| B, `Dibujo B.dwg` | `fb9b97f9…682b` | sin cambios |
| C, `Dibujo C.dwg` | `ede7cdb3…a53c` | sin cambios |
| D, `I64-SMOKE1-FIXTURES-D.dxf` (PR-12 y PR-13), con su copia recuperable `.pristine` | `03e5606c…f165` | sin cambios, las dos copias |

**PR-12 / PR-13 contra los bytes nuevos de `RackCad.Application`** (`fixtures-revalidation.txt`, RESULT PASS):
- relectura independiente del DXF D (`check_smoke1_fixtures.py`, solo biblioteca estándar): READBACK PASS, los dos sobres byte a byte
  iguales a los preparados;
- arnés `FixtureHarness` copiado fuera del worktree y compilado contra el `RackCad.Application.dll` del Candidato; el ensamblado que
  ejecuta tiene el mismo SHA-256 (`12B8E237…27B3`, MATCH);
- PR-13: `Deserialized=True`, `EnvelopeId=[]`, `Kind=NoIdentity`, `RackId=[null]` → «Sin identidad»;
- PR-12: `Deserialized=False`, `EnvelopeId=[null]`, `Kind=Diagnostic`, `RackId=[null]` → «Diagnóstico».
- El generador, el comprobador, el arnés y los sobres tienen los mismos SHA-256 que en la preparación del 2026-10-12.

**Regla:** ningún commit de I-64 antes de la ocurrencia, porque cambiaría el SHA estampado en el DLL. No se recompila.
