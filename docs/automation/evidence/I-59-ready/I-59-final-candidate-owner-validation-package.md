# I-59 — evidencia final automatizada y paquete Owner Validation

Fecha: 2026-09-24

Este registro acredita la parte automatizada/local del `FINAL_CANDIDATE_SHA` y prepara la ejecucion
manual asignada por A-1. No declara Owner Validation aprobada, no publica FOUNDATIONS, no cierra la
iniciativa y no integra.

## Identidad y preflight

```text
FINAL_CANDIDATE_SHA = d905572e9d3faf7bedcb6c22abe51c36da6e11eb
Candidate worktree = C:\Users\alejandra-mendoza\.codex\worktrees\i-59-final-candidate
Candidate checkout = detached HEAD
Candidate tree = CLEAN before and after every local command
SDK resolved = 8.0.423

origin/main = c75e7434a909d396c05e55c39a140ba53be9e98c
initiative branch tip before this publication = 885b4069b41f8e9a81557dc8fa95d0843988de5b
post-Candidate executable diff = NONE
```

Freeze y amendment verificados:

```text
FREEZE_COMMIT_SHA = a7133b3f12cac8e4c763584c02530faa03ef29ae
FREEZE_PATH = docs/initiatives/I-59-proposal-v3.md
FREEZE_BLOB_SHA = 3237376922ae24a94995c06c1c3ec5f74bdbdd2f
APPLICABLE A-n = A-1
A-1 PATH = docs/initiatives/I-59-A-1.md
A-1 BLOB = c0f6de95400aaad88bb56ddae6965bc3746ea14c
```

El Candidate existe, es ancestro de la rama y el rango hasta el tip documental previo solo contenia
documentacion/evidencia permitida. `origin/main` no avanzo desde READY-04/06; no se activo ruta R.

## Evidencia local exact-SHA

Todas las corridas comenzaron y terminaron con HEAD
`d905572e9d3faf7bedcb6c22abe51c36da6e11eb` y arbol limpio.

| Clase | Resultado | Duracion medida |
|---|---|---|
| Core Full local Debug | 11321 selected / 11321 PASS / 0 FAIL / 0 skipped | 92.008 s de comando; VSTest reporto 1 min 2 s |
| UI Full local Debug | 1598 selected / 1581 PASS / 0 FAIL / 17 skipped historicos | 205.875 s de comando; VSTest reporto 3 min 12 s |
| Build UI Debug | PASS; 0 warnings / 0 errors | 3.573 s de comando |
| Build Plugin Debug | PASS; 0 errors / 2 `MSB3277` conocidos | 5.667 s de comando |

AutoCAD estaba cerrado antes del build Plugin. Las advertencias de analizadores xUnit observadas durante
las suites son la deuda preexistente registrada por el repositorio; no hubo fallo ni skip nuevo de I-59.

## DLL exacto para NETLOAD

```text
DLL PATH = C:\Users\alejandra-mendoza\.codex\worktrees\i-59-final-candidate\src\RackCad.Plugin\bin\Debug\net8.0-windows\RackCad.Plugin.dll
DLL INFORMATIONAL VERSION = 1.0.0+d905572e9d3faf7bedcb6c22abe51c36da6e11eb
DLL FILE VERSION = 1.0.0.0
DLL SHA-256 = FA8E1BBA74E5EEC17A2D98905B11F238B2921EBA890D0D684183E908E495FEEC
```

La InformationalVersion identifica inequivocamente el Candidate. No reconstruir el DLL antes de la
ronda Owner sin repetir su hash e identidad.

## CI exact-SHA de push

```text
CI RUN = 36009185876
event = push
head_branch = architecture/shared-view-placement-block-facts
head_sha = d905572e9d3faf7bedcb6c22abe51c36da6e11eb
status = completed
conclusion = success
```

| Job requerido | Resultado |
|---|---|
| Tests (Domain + Application) | success |
| UI Tests (WPF controls, net8.0-windows) | success |
| Build UI (WPF, valida API de Application) | success |
| Build Plugin without AutoCAD | success |

Run: <https://github.com/marioap-afk/Calculadora_de_racks/actions/runs/36009185876>

## Cobertura del Candidate

La politica vigente permitio `workflow_dispatch` desde la rama por defecto. Se solicito y midio el SHA
exacto; el `head_sha` del run es el tip documental del ref por semantica de dispatch y no se usa como
identidad medida.

```text
COVERAGE RUN = 36030056756
event = workflow_dispatch
dispatch ref = architecture/shared-view-placement-block-facts
requested candidate_sha = d905572e9d3faf7bedcb6c22abe51c36da6e11eb
run head_sha = 885b4069b41f8e9a81557dc8fa95d0843988de5b
checkout/measured SHA = d905572e9d3faf7bedcb6c22abe51c36da6e11eb
result = success
artifact name = rackcad-coverage-cobertura
artifact id = 10821655385
artifact digest = sha256:8c34655f1f0fef77849cf021166203259e6f1698fa9446556cb6f975c3ee516b
artifact size = 705850 bytes
```

`measured-sha.txt` descargado del artifact registro:

```text
measured_sha=d905572e9d3faf7bedcb6c22abe51c36da6e11eb
event=workflow_dispatch
run_head_sha=885b4069b41f8e9a81557dc8fa95d0843988de5b
run_id=36030056756
```

Run: <https://github.com/marioap-afk/Calculadora_de_racks/actions/runs/36030056756>

## Biblioteca externa efectiva

`BlockLibraryLocator.ResolvePath()` usa el override de `%APPDATA%\RackCad\settings.json`; no se asumio
el nombre por defecto.

```text
SETTINGS PATH = C:\Users\alejandra-mendoza\AppData\Roaming\RackCad\settings.json
SETTINGS SHA-256 before Owner round = A7667DBDCEDA5A5175C844ED396BA1F0B17207295F40E9986C29D826B48B2873
OVERRIDE CONFIGURED = YES
LIBRARY RESOLVED PATH = D:\Base_de_datos_AutoCAD_V.0.dwg
LIBRARY EXISTS = YES
LIBRARY LENGTH = 477525 bytes
LIBRARY LAST WRITE UTC = 2026-09-09T16:47:48Z
LIBRARY SHA-256 = B4CA2248DB9C3D72487AC8B5B1E5510CDD8ABA231AB340541D91BEBCA2D560E8
```

No se movio, renombro ni modifico la biblioteca.

## Checklist manual proporcional

| Punto de la guia | Disposicion I-59 |
|---|---|
| NETLOAD y menu | APPLICABLE: cargar el DLL exacto, abrir `RACKCAD` y confirmar la ruta efectiva |
| Dibujo/colocacion | APPLICABLE solo a la ruta real elegida para importar/observar el bloque y a su resultado visible |
| Bloques reales/parametros | APPLICABLE al bloque elegido para OV-I59-01 y al resultado visible de esa ruta |
| Actualizacion en sitio | NOT APPLICABLE: I-59 no cambia `Actualizar`, redraw ni authored authority |
| Edicion y round-trip | NOT APPLICABLE: schema/persistence diff es NONE |
| Vistas ligadas/multivista | NOT APPLICABLE: I-59 no implementa ID19 ni modifica materializacion multivista |
| BOM/consolidado | NOT APPLICABLE: BOM y geometria de producto quedaron fuera del Freeze y del diff |
| Persistencia/legacy | NOT APPLICABLE: no hay DTO, serializer, store ni migration I-59 |
| `RACKDUPLICAR` | NOT APPLICABLE: pertenece a otros consumidores y no fue modificado |
| Cotas por tipo de vista | NOT APPLICABLE: I-59 no cambia cotas, visibilidad ni layout |
| Escenarios especificos I-59 | APPLICABLE: OV-I59-01..04 completos |
| No crash/error silencioso | APPLICABLE en los cuatro escenarios |
| Inspeccion visual | APPLICABLE a la ruta ejercitada: resultado esperado, sin regresion visible |

No declarar ejecutado ningun punto `NOT APPLICABLE`.

## I-59 FINAL CANDIDATE OWNER VALIDATION

```text
Candidate SHA = d905572e9d3faf7bedcb6c22abe51c36da6e11eb
Candidate worktree = C:\Users\alejandra-mendoza\.codex\worktrees\i-59-final-candidate
DLL Debug path = C:\Users\alejandra-mendoza\.codex\worktrees\i-59-final-candidate\src\RackCad.Plugin\bin\Debug\net8.0-windows\RackCad.Plugin.dll
DLL InformationalVersion = 1.0.0+d905572e9d3faf7bedcb6c22abe51c36da6e11eb
DLL SHA-256 = FA8E1BBA74E5EEC17A2D98905B11F238B2921EBA890D0D684183E908E495FEEC
AutoCAD required = 2025
External library path = D:\Base_de_datos_AutoCAD_V.0.dwg
External library SHA-256 = B4CA2248DB9C3D72487AC8B5B1E5510CDD8ABA231AB340541D91BEBCA2D560E8
Automated candidate evidence =
  Core Full local: PASS
  UI Full local: PASS
  Build UI Debug: PASS
  Build Plugin Debug: PASS
  CI exact SHA: PASS / run 36009185876
  Coverage: PASS / run 36030056756 / artifact 10821655385
Please execute = OV-I59-01, OV-I59-02, OV-I59-03, OV-I59-04
Owner verdict required = APPROVED | REJECTED | PARTIAL
```

### Preparacion comun

1. Confirmar que AutoCAD es 2025 y que el dibujo de prueba es recuperable.
2. Recalcular el SHA-256 del DLL y de la biblioteca; deben coincidir con este registro.
3. Ejecutar `NETLOAD` con el DLL anterior, abrir `RACKCAD` y comprobar que muestra exactamente
   `D:\Base_de_datos_AutoCAD_V.0.dwg`.
4. Registrar fecha/zona, validador, dibujo usado y las keys concretas elegidas. No inventar nombres de
   bloque: verificar cada key contra el catalogo y el DWG externo antes de usarla.

### OV-I59-01 — import real

1. Elegir una pieza/ruta real cuyo `LibraryKey` este verificado como presente en la biblioteca externa.
2. Abrir un dibujo de prueba donde esa definicion no exista; registrar la ausencia inicial y la key.
3. Ejecutar una sola vez la ruta normal del producto que materializa esa pieza.
4. Confirmar: definicion importada, una sola importacion efectiva, query final V1 en `Found`, ninguna
   excepcion y resultado visible sin regresion.
5. Registrar key, comando/ruta, evidencia antes/despues y resultado.

### OV-I59-02 — bloque ausente de biblioteca

1. Elegir una key catalogada que pueda ejercitarse por una ruta real y verificar que esta ausente tanto
   del dibujo de prueba como de la biblioteca externa consultable. Si la biblioteca real contiene todas
   las keys aplicables, usar una copia de prueba recuperable de la biblioteca con una definicion elegida
   retirada; no modificar el archivo real.
2. Ejecutar la ruta normal una vez.
3. Confirmar: no crash, ninguna importacion/presencia fabricada y query final V1 permanece `Missing`.
4. Registrar key, identidad de la copia si se uso y restaurar inmediatamente el override original.

### OV-I59-03 — biblioteca no disponible

1. Antes del cambio, registrar la ruta efectiva, el contenido de `BlockLibraryPath` y el SHA-256 del
   settings file indicados arriba.
2. Crear una copia de respaldo del settings file. Cambiar temporalmente solo `BlockLibraryPath` a una
   ruta nueva garantizada como inexistente. No mover ni renombrar la biblioteca real.
3. Ejecutar la misma clase de ruta del producto y confirmar: `LibraryAvailability = FileMissing`,
   `LibraryBlockPresence = Unknown`, flujo V1 sin crash y sin exito fabricado.
4. En un bloque `finally` operativo, restaurar el settings file original byte por byte, volver a abrir
   `RACKCAD` y confirmar que la ruta resuelta y ambos hashes vuelven a los valores de este registro.
5. Si la restauracion no puede confirmarse, detener la ronda y declarar `REJECTED` o `PARTIAL`; no seguir.

### OV-I59-04 — cache compartido

1. Con el Candidate y biblioteca reales sin cambios, preparar dos dibujos de prueba recuperables para
   que la misma ruta ejerza observer/importer/cache dos veces dentro de la misma sesion de AutoCAD.
2. Ejecutar consecutivamente la ruta en ambos dibujos, conservando la misma key y biblioteca.
3. Confirmar resultados coherentes, import/query final correctos, ausencia de `ObjectDisposedException`,
   ausencia de datos stale y ninguna regresion visible.
4. Registrar ambas ejecuciones, sus resultados y que los hashes de DLL/biblioteca no cambiaron.

## Resultado pendiente del Owner

```text
OWNER VALIDATION = PENDING
OV-I59-01 = PENDING
OV-I59-02 = PENDING
OV-I59-03 = PENDING
OV-I59-04 = PENDING
Owner verdict = PENDING / APPROVED | REJECTED | PARTIAL
FINAL DOCUMENTARY CLOSURE = NOT STARTED
FOUNDATIONS PUBLICATION = NOT PERFORMED
INTEGRATION = NOT STARTED
TAG = NOT CREATED
```
