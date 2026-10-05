# I-62 — Kit v3 de la revisión formal del Architect de la A-1 corregida (R20261005T063359Z-86e3)

```text
LogicalReviewRequestId: L20261005T063359Z-86e3   InvocationId: I20261005T063359Z-86e3   AttemptSeq: 1   RunId: R20261005T063359Z-86e3
Autorización:   orden nocturna (decisiones §41, punto E): hasta DOS invocaciones del Architect para revisar las correcciones de A-1, solo con
                transporte limpio; esta es la primera
Objeto:         commit 9dcfc08d5171b01df97b3a492a60218c5ab2453e (CI de publicación 37272599376, push, 4/4 success)
                docs/initiatives/I-62-A-1.md                                blob 39c2f8317ec381fa60c3564a834278df8898101c
                docs/initiatives/I-62-architect-package-A-1.md              blob 82bea6e61cd0712dfe3d235420d5d7cabb0507be
                docs/initiatives/I-62-architect-review-A-1-disposition.md   blob 2c7a8e7e030c3d76516bbc9889237aca0a35bfc6
Estado:         KIT PREPARADO · lanzamiento HUMAN_LAUNCH_REQUIRED (tarea de la app task_7b4dd3ee, creada una vez) · sin veredicto
Naturaleza:     preparación; los JSON son representaciones EXPERIMENTALES (§20.3.2, B.10.1). Rigen I-61 y LIFECYCLE. F4 producción = NO AUTORIZADA
```

## 1. Archivos (`kit/`, copias byte a byte de `D:\r62-arch-a1r3-run`; hashes en `kit/kit-manifest.json`)

| Archivo | Papel |
|---|---|
| `prompt.md` (SHA-256 `b221c079…`) | instrucciones del revisor: identidad, cierre, contrato de acciones, lectura fiel, focos 1-16, diez disposiciones, diez opcionales y forma del resultado |
| `order.txt` (`4fc5883f…`), `prior-authorization.txt` (`04608994…`) | orden nocturna vigente; autorización de decisiones §40, solo como fuente de los focos 1-12 y de las condiciones formales (su objeto y su invocación están consumidos) |
| `closure.json` (`3362f952…`), `corpus.json`, `make_closure.py` | cierre efectivo: 29 insumos canónicos y 12 transitivos; contrato ID-1, ID-2, NAV-1, RD-1, RD-2, EX-1, EX-2 y OWN-1; prohibidos |
| `result.schema.json` | esquema estricto del resultado: diez disposiciones, diez opcionales, focos 1-16, `IfAgreed` de quince campos, REQUIRED nuevos `A62-A1S-NN` |
| `post-review.py`, `selftest-post-review.py`, `selftest-result.json` | auditor v3 y su autoprueba: PASS antes del lanzamiento |
| `preflight-read-fidelity.json`, `read_fidelity.py` | preflight de Read sobre este corpus: 3 lecturas y 108 líneas FAITHFUL_NORMALIZED (A-1 D1-1..D1-21, con líneas de hasta 1 794 caracteres) |

## 2. Lecciones incorporadas

- **De R20261003T023945Z-cab7 (no acreditada):**
  - la única rama del clon es `main`, en el commit exacto, sin remoto;
  - Grep solo sobre archivos;
  - acciones enumeradas;
  - salidas propias clasificadas.
- **De R20261005T044948Z-ac67:** la tabla de acciones del prompt es **idéntica** a la lista blanca del auditor. Cubre:
  - `cd` hacia el clon, el worktree o las salidas propias;
  - `git -C <worktree>`;
  - `| sha256sum` tras RD-1;
  - heredocs y variables de shell;
  - el proyecto de `dotnet test` desde Bash o PowerShell.

## 3. Reglas del auditor v3 (declaradas antes de la corrida; no cambian después)

Orden §6: «comprueba rutas y efectos, no solo igualdad literal de cadenas de comandos».

- **Código python** (heredoc, `-c` o script propio escrito en la transcripción):
  - se comprueba por sus rutas literales, resueltas contra el directorio actual;
  - falla con efectos no verificables: listar directorios, procesos, red, borrar o mover.
- **Arnés:** exige exactamente una salida propia como argumento, para que no sobrescriba el resultado canónico del clon.
- **`dotnet test`:** cualquier otra ruta debe ser una salida propia.
- **Clon:** debe quedar limpio y en el commit tras la corrida.
- **Terminación:** una llamada sin resultado o una cadena lateral (subagentes) hace fallar la terminación.
- **Transporte:**
  - U+FFFD y los marcadores de truncamiento se registran;
  - una línea larga **conocida** (`LongLines` del cierre) que Read entrega como prefijo queda en TRUNCATED_KNOWN: no cuenta como entregada y, sola, no es
    violación.
- **`prior-authorization.txt`:** se comprueban su hash y su lectura completa.
- **Coherencia del resultado:**
  - diez disposiciones;
  - diez opcionales;
  - focos 1-16;
  - con AGREED, los quince campos de `IfAgreed` en `true`.

## 4. Lanzamiento

El único transporte limpio es una tarea de la app que abre una sesión nueva sobre el clon `D:\r62-arch-a1r3`. Codex sigue bloqueado por OD-2, y Claude
CLI sin autenticar por OD-3.

La sesión creó la tarea una sola vez (`task_7b4dd3ee`) y registró HUMAN_LAUNCH_REQUIRED. Si el Owner la lanza, la custodia posterior seguirá el mismo
formato que R20261005T044948Z-ac67:
- resultado literal;
- auditoría v3;
- identidad observada;
- fidelidad;
- CI.
