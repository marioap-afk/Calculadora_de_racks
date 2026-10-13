# I-64 — Orden BASELINE de T3C para Smoke-1 (preparada; NO ejecutada)

La BASELINE real **no** se captura hasta que existan: la autorización de Smoke-1 del Coordinator (con `RunId` y SHA del Candidato), el
RELEASE de I-52 con su acuse, y todas las precondiciones PC-1..PC-8 de T3C §2. Rutas locales como `<worktree>`.

## Herramientas aprobadas (T3C §0; PC-4)

Se ejecutan los bytes del blob, extraídos con `git cat-file blob <Candidato>:<ruta>` a `<worktree>\artifacts\orchestration\I-64\F1-SMOKE-1-PREP\t3c\tools\`
(no la copia de trabajo, que puede llevar CRLF). Sin cambios desde `9af9ac75`.

| Herramienta | Blob | SHA-256 con LF (= bytes extraídos) | Comprobado |
|---|---|---|---|
| `Capture-ProfileState.ps1` | `ef215539536fff3cdf6fd533f87bb2b622118e78` | `750a9097d4d84a75ac1787a7bf40032c44556c64399182aac0b95694b48226e1` | igual |
| `Compare-ProfileState.ps1` | `d314293a6ed1a8cb048a284da24e85104f24078d` | `de642515371b19a59230940ef9659f3d84413f66c8831be201abf55fa73cbd65` | igual |
| `Test-ProfileDiff.ps1` | `c7d2c35ff4cf07e1f0a47bd06317fc563ea9e001` | `d0bb54be83291e060aeaf4d2ec792743d70fc0f621202a1e24ee2fb41de94d8b` | igual |

**PC-8 (autoprueba en la estación):** 2026-10-11, `SELFTEST PASS: 26/26`, código 0; salida SHA-256
`2981350c0738159abc8edeeb8fb2662130eaa98f14d7043ac4d04704766a5b3d`, idéntica a las dos de la validación de T3C. Es sintética: no lee el
registro ni AutoCAD. Se repite en la ventana de Smoke-1, antes de la BASELINE.

## Orden exacta (T3C §3 con los valores del Candidato; los `<acuse>` vienen del acuse de I-52)

Con AutoCAD cerrado, en `pwsh` (PowerShell 7), después de PC-1..PC-8:

```powershell
pwsh -NoProfile -File <worktree>\artifacts\orchestration\I-64\F1-SMOKE-1-PREP\t3c\tools\Capture-ProfileState.ps1 -Phase BASELINE `
  -OutDir <worktree>\artifacts\orchestration\I-64\F1-SMOKE-1\<RunId>\profile -RunLabel <RunId> `
  -CandidateDll <worktree>\src\RackCad.Plugin\bin\Debug\net8.0-windows\RackCad.Plugin.dll `
  -ExpectTrustedPathsSha256 <acuse> -ExpectSecureLoad <acuse> -ExpectProfile <acuse> -ExpectAcadSha256 <acuse>
```

- `<RunId>`: el de la autorización de Smoke-1 (PR-02). La carpeta `F1-SMOKE-1\<RunId>\` es nueva y vacía (PR-09; T3D §10.1).
- `-CandidateDll`: SHA-256 esperado `E769219D2F5EDF6E0DE0867633C4A89B94EBBD7F2BD88CF20864CBCF75EF7512` (Candidato `4d79e910`).
- Código 0: se puede abrir AutoCAD. Código 5 (deriva frente al acuse): STOP-03 antes de AutoCAD. Códigos 2, 3, 4 o 6: STOP.
- La salida de consola se copia literal en `<run>\console\baseline.txt`. No se repite para buscar otro resultado (RunId nuevo).
- POST (T3C §4) con el mismo `-CandidateDll` y los mismos `<acuse>`; DIFF (T3C §6) con `-PaletteKeyAcceptance` del acuse.

## Valores a la espera del acuse de I-52 (PC-1)

| Parámetro | Último valor registrado por I-52 (no se presume vigente) | Valor del acuse |
|---|---|---|
| `-ExpectTrustedPathsSha256` | `a9ba0fd1c2f15dfc7ee4477e0ea62e710ac842d89d2b0e18102b2362553a2498` (19 entradas) | pendiente |
| `-ExpectSecureLoad` | `1` | pendiente |
| `-ExpectProfile` | `<<Unnamed Profile>>` | pendiente |
| `-ExpectAcadSha256` | `2a75996fd2a5c5ee0376fd5ba7cced227a098193ff495e8ceeda3a96fe326422` (igual al medido hoy en `acad.exe` R25.0.171.0.0) | pendiente |
| `-PaletteKeyAcceptance` | — | pendiente (`ACCEPTED` / `NOT_ACCEPTED`; si falta, `NOT_RECORDED`) |
| carga única del DLL (O-4) | — | pendiente |
