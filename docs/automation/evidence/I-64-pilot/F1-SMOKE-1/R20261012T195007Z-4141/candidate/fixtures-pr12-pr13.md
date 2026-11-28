# I-64 — Dibujos de prueba PR-12 y PR-13 de Smoke-1 (preparación; transitorio, ignorado por Git)

Preparados el 2026-10-12 por la sesión responsable, sin AutoCAD, por orden del Coordinator (revisión final del gate F1, punto B).
Las rutas locales se escriben como `<prep>` = `<worktree>\artifacts\orchestration\I-64\F1-SMOKE-1-PREP`.

## Archivo de origen (común a los dos)

| Campo | Valor |
|---|---|
| Archivo | `<prep>\fixtures\I64-SMOKE1-FIXTURES-D.dxf` (dibujo **D**, aparte de A, como admite T3D §2.1) |
| Formato | DXF AutoCAD 2013 (`AC1027`), texto con CRLF, `INSUNITS` = 1 (pulgadas, la unidad de RackCad) |
| SHA-256 | `03e5606c1e311f700640125b892fe09bf408dd5642115ebcb433b57474ccf165` (21375 bytes) |
| Copia recuperable | `<prep>\fixtures\I64-SMOKE1-FIXTURES-D.dxf.pristine`, mismo SHA-256 |
| Contenido | en el espacio modelo: un INSERT de `I64_FX_PR13_SIN_IDENTIDAD` en (0,0), un INSERT de `I64_FX_PR12_DIAGNOSTICO` en (150,0) y un TEXT de aviso «I-64 SMOKE-1 - DIBUJO D DE PRUEBA - CERRAR SIN GUARDAR». Cada bloque es un rectángulo de 120 x 40 con su rótulo dentro (al pulsar el rótulo se selecciona el bloque) |

## PR-13 SIN IDENTIDAD

| Campo | Valor |
|---|---|
| Bloque | `I64_FX_PR13_SIN_IDENTIDAD` (rótulo «PR-13 SIN IDENTIDAD») |
| Sobre (definición del bloque → diccionario de extensión → `RACKCAD_SELECTIVE` → XRECORD, grupo 1, como `RackBlockData.Write`) | `{"SchemaVersion":"1.0","Kind":"selective","View":"frontal","Section":-1,"Id":"","Name":"I-64 SMOKE-1 FIXTURE PR-13 SIN IDENTIDAD","Design":"{}"}` (144 bytes, SHA-256 `2de3c7bf49db4cc48be4ec3ce6035c0b7d5f8f7a166457a894269bbba163e502`) |
| Procedencia y propósito | sobre producido por `RackEmbedStore.Serialize` del Candidato (`RackCad.Application.dll` SHA-256 `BEF900AB…`, build de `4d79e910`) mediante `harness/` con `Id` = "" (en blanco). Es un sobre **legible** de RackCad (`Kind` selective, versión 1.0): `RackEmbedStore.Deserialize` lo acepta y su Id está en blanco, así que `SelectionContext.Classify` da `NoIdentity` (INV-16). **No** es un bloque genérico: un bloque sin datos de RackCad daría «Selección actual: 0 racks y 1 otros objetos.» (SM-10a). `Design` es un objeto vacío: el panel no lo lee; por eso este bloque no sirve para RACKEDITAR ni para otro comando de RackCad |
| Título y texto exactos esperados | «Sin identidad» / «El objeto seleccionado no tiene identidad de rack.»; no aparece ningún RackId (`WorkspacePanelViewModel.cs`, casos `NoIdentity`) |
| Paso | T3D SM-10b |

## PR-12 DIAGNÓSTICO

| Campo | Valor |
|---|---|
| Bloque | `I64_FX_PR12_DIAGNOSTICO` (rótulo «PR-12 DIAGNOSTICO») |
| Sobre | `{"SchemaVersion":"2.0","Kind":"selective","View":"frontal","Section":-1,"Id":"6f4e1c2a-64d0-4b1e-9a30-5f1c0d2e7b41","Name":"I-64 SMOKE-1 FIXTURE PR-12 DIAGNOSTICO","Design":"{}"}` (178 bytes, SHA-256 `c182f722e0e77db66b97cd717244ce4f07c9f5c6797319714ec74570bb51f1f1`) |
| Procedencia y propósito | sobre producido por el mismo `RackEmbedStore.Serialize` del Candidato con `SchemaVersion` = "2.0": representa la condición «escrito por una versión más nueva» del clasificador actual. `SchemaVersionPolicy.IsReadable("2.0", "1.0")` es falso, `RackEmbedStore.Deserialize` devuelve null, `WorkspaceSelectionReader` construye `DefinitionFact(false, null)` y `SelectionContext.Classify` da `Diagnostic` (INV-17). Lleva un Id de prueba, que **no** debe aparecer en el panel |
| Título y texto exactos esperados | «Diagnóstico» / «No se pudo interpretar la definición del objeto seleccionado.»; no aparece ningún RackId (en particular, no `6f4e1c2a-…`) |
| Paso | T3D SM-11 |

## Regla de no guardar (los dos)

- El dibujo D **nunca se guarda**: no se usa `SAVE`, `QSAVE`, `SAVEAS` ni el autoguardado sobre él; al terminar se cierra respondiendo
  «No» a guardar.
- En SM-41 se recalcula su SHA-256, que debe seguir siendo `03e5606c…f165`; si cambió, esa parte es FAIL y se restaura desde la copia
  `.pristine`.
- Sobre el dibujo D no se ejecuta ningún comando de RackCad (solo se seleccionan los bloques con el panel visible).

## Verificación sin AutoCAD (2026-10-12)

| Comprobación | Resultado |
|---|---|
| `ezdxf` 1.4.4 `audit()` al escribir y al releer | 0 errores, 0 arreglos |
| Relectura independiente, solo biblioteca estándar (`check_smoke1_fixtures.py`): INSERT en el espacio modelo → BLOCK_RECORD → `ACAD_XDICTIONARY` (propietario = el BLOCK_RECORD) → `RACKCAD_SELECTIVE` → XRECORD → grupo 1 concatenado | PASS: los dos sobres, byte a byte iguales a los serializados |
| INSERT con propietario `*Model_Space` y sin `paperspace` (el lector exige espacio modelo) | sí, los dos |
| Clasificación por el código del Candidato (`harness check` sobre lo releído) | PR-13: `Deserialized=True`, `EnvelopeId=[]`, `Kind=NoIdentity`; PR-12: `Deserialized=False`, `Kind=Diagnostic`; `RackId` null en los dos |

**Residual:** que AutoCAD 2025 abra este DXF no se ha comprobado, porque no se lanza AutoCAD. La primera apertura será en la ventana
autorizada. Si no abre, o los bloques no dan esos textos, S1-04 = INCOMPLETE.

## Archivos de procedencia (en `<prep>\fixtures\`)

| Archivo | SHA-256 |
|---|---|
| `make_smoke1_fixtures.py` (generador; `ezdxf` 1.4.4 en un entorno virtual temporal de la sesion) | `df2a6659761ff878b38e4b2ee970791dd9105b7d018ac051bd778ba0b0fa9ff9` |
| `check_smoke1_fixtures.py` (relectura independiente) | `210ac6fc814645686aea334e3b9f66add6940e46ea3d5bd9b6e6fceb5cd5388d` |
| `harness/Program.cs` | `61ca402b013bf3f0f21b010f68e1e5d34ab0126f3d393ae5f4f1d3c415037606` |
| `harness/FixtureHarness.csproj` | `a1ad68fea7ffa083db8995ae57cbe1ee40a463e4a547cd9890e5bd654acf8780` |
| `envelope-pr13-sin-identidad.json` | `2de3c7bf49db4cc48be4ec3ce6035c0b7d5f8f7a166457a894269bbba163e502` |
| `envelope-pr12-diagnostico.json` | `c182f722e0e77db66b97cd717244ce4f07c9f5c6797319714ec74570bb51f1f1` |
