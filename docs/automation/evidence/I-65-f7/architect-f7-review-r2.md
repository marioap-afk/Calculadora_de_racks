# I-65 F7: relectura del Architect (ronda 2) sobre `ecefcd64`

**Veredicto: CONFORMANT WITH OBSERVATIONS.** A65-F7-01 y A65-F7-02 quedan cerradas. No hay REQUIRED abiertos y solo hay una observación nueva, OPTIONAL. No declaro GATE PASS ni READY.

- Rol: Architect, SAME-SESSION ROLE, solo lectura.
- **pwd informado como primera acción:** `/d/Documentos/Codex/Calculadora de racks`. No leí ese checkout. Todo se leyó con `git -C <worktree de la rama>`.
- No escribí archivos, no compilé ni ejecuté intérpretes y no usé fetch ni `merge-tree`. Los hashes los calculé con `git cat-file blob | sha256sum`.

## Respuestas

**1. Identidad: CUMPLE.**
- Entre `8b2117c3` y `ecefcd64` hay un solo commit y solo toca `docs/` y `README.md`.
- Desde `bd1fcf8a` no cambia nada fuera de `docs/` y `README.md`.
- Los árboles siguen iguales: `src` `6cadd526`, `tests` `47fbcaf5`, `tools` `29485f42`, `assets` `d1cc1811`.
- `Directory.Build.*`, `global.json` y `RackCad.sln` no cambian.
- El Freeze conserva el blob `4e6e1feb…` y ADR-0050 el `c18def42…`. `docs/FOUNDATIONS.md` y `docs/adr/README.md` siguen iguales a `bb0d5522`.
- No hay A-n.
- §100 no se reescribió: la errata de CD-I65-F7-09 se añade en §101 (CD-I65-F7-12).

**2. A65-F7-01: CERRADA.**
- La entrada dice ahora «único productor en el código de producto».
- Declara el constructor público (`StructuralModel.cs:386`) y `StructuralModelResult.Create` (`StructuralExportOutcome.cs:62`).
- Declara la exclusividad no guardada como limitación.
- Las dos cosas coinciden con el código: la única llamada de producto está en `StructuralModelBuilder.cs:487` y las pruebas y fixtures construyen el modelo directamente.

**3. A65-F7-02: CERRADA.**
- La limitación de CD-I65-F4-10 está ahora en todos los sitios donde faltaba:
  - guía §2, §4, §10 y §11;
  - fila del README;
  - entrada «Provider Boundary» (Persistence y Known limitations);
  - F7-OBS-03;
  - R-D11 y READY-01 del plan.
- Coincide con el código:
  - el target `AssembleAutoloaderBundle` no copia la tabla;
  - la prueba `LaTablaNoViajaEnElPaqueteDelAutoloader…` lo protege;
  - el mensaje «Falta la tabla de secciones y materiales junto al Plugin» con resultado `Unavailable` está en `RackRamElementsCommands.cs:110`.

**4. Las correcciones no introducen afirmaciones falsas nuevas.** Contrasté cada una con el código:
- **Esquema de la tabla:** las claves coinciden con `RamSectionMapReader` (líneas 37-41, `proposed` y `verified`).
- **Archivo de cargas:**
  - `LOADS-*` da `Invalid`, incluida la magnitud no positiva;
  - `LOAD-COUNT-EXCEEDED` da `Unsupported`;
  - un archivo inexistente da `Unavailable`, por la excepción de `ReadAllBytes` (línea 128) que recoge el `catch` de la línea 56.
- **Carpeta:** `PACKAGE-DESTINATION-EXISTS` y `PACKAGE-PARENT-MISSING` existen (`StructuralPackageWriter.cs:226` y `:231`).
- **§5 de la guía, contra `RamBuildScriptTemplate.cs` `main()`:** la descripción es correcta.
  - Las tres puertas se evalúan sin escribir en el modelo (`rm.model()` solo lee).
  - Después se ejecuta `open(..., "x")` de `ram-readback.json` (línea 347).
  - Si una puerta falla, el archivo queda con `FAILED — DISCARD MODEL` y las diferencias `gate: …`.
  - Por eso «una sola ejecución por carpeta, también si fue una negativa» es exacto.
- **Volcado:** `DUMP FAILED` es el estado real de la plantilla, y el comparador lo rechaza (`RamToolCompare`, rama `failedDump`).
- **Parada del diálogo:** la regla de responder «No» y detenerse coincide con D-11 y §15 del Freeze.
- **Herramienta:** el `-o <carpeta>`, el `.exe` y el comando `help` existen.
- **Enlaces:** los dos procedimientos enlazados existen. El de P2 remite al procedimiento manual de `REF-CANT-01`.
- **Revisión de conformidad:** las citas `StructuralReadbackComparison.cs:481` (`Stale`) y la línea 486 (`FirstBindingMismatch`) son exactas.
- **Apartado «Consume»:** es correcto que solo Rack Identity tiene entrada en `FOUNDATIONS.md`.

**5. Estado de A65-F7-03..10.**
- A65-F7-03 a A65-F7-09: **CERRADAS**.
- A65-F7-10: **ACEPTADA** la disposición.
  - AC-F7-03 ya exige un CI `push` con `head_sha` exacto, y AGENTS.md impone la `ref`. Registrar el SDK resuelto y la `ref` en la evidencia de cierre satisface la observación sin cambiar un criterio fijado antes de ejecutar.
  - Se cierra cuando esa evidencia exista.

**6. Custodia y privacidad: CUMPLE.**
- Las 8 filas de §105 coinciden con los blobs de `ecefcd64`.
- En el prompt versionado la ruta del worktree aparece redactada como `<usuario>`.
- Los archivos añadidos no contienen nombres, cuentas, rutas con usuario ni secretos.

## Matriz AC-F7 actualizada

| AC | Estado |
|---|---|
| 01 | CUMPLE |
| 02 | CUMPLE en `ecefcd64`; repetir en el SHA de cierre |
| 03 | **PENDIENTE POR CONSTRUCCIÓN**: falta el commit de cierre; árbol limpio; sobre ese HEAD exacto, Core Full y UI Full locales (omitidas declaradas) y builds Debug de UI, Plugin y herramienta con 0 errores, con el SDK resuelto registrado; push; CI `push` con `ref=refs/heads/architecture/interop-analisis-estructural`, `head_sha` exacto y 4/4 en `success`, leído |
| 04 | CUMPLE |
| 05 | CUMPLE |
| 06 | CUMPLE |
| 07 | CUMPLE |
| 08 | CUMPLE |
| 09 | CUMPLE (A65-F7-11 OPTIONAL) |
| 10 | CUMPLE hasta §105; continúa en el commit de cierre |
| 11 | **PENDIENTE POR CONSTRUCCIÓN**: por mi parte no quedan REQUIRED abiertos. Falta versionar este informe con su custodia y que el Coordinator emita CONFORMING de cierre (LIFECYCLE §7 punto 5) en el commit de cierre |
| 12 | **PENDIENTE POR CONSTRUCCIÓN**: commit de cierre (este informe, CONFORMING del Coordinator y AC-F7-03/12 marcados «pendientes por construcción") → árbol limpio → regresión Full local → push → CI exacto → GATE PASS en un commit posterior (precedente CD-I65-F5-59/61) |

## Hallazgo nuevo

| ID | Ubicación | Hallazgo | Severidad | Recomendación |
|---|---|---|---|---|
| A65-F7-11 | Plan de READY, R-D11 (segunda opción) y §5 | «Autorizar el cambio de `deploy/verify-bundle.ps1` y del paquete» se queda corto: también habría que cambiar el target `AssembleAutoloaderBundle` de `RackCad.Plugin.csproj` e invertir la prueba `LaTablaNoViajaEnElPaqueteDelAutoloader_NiElDestinoDelPaqueteSeToca`. Eso es código y pruebas, con Worker, Codex y `attempts`. La tabla de presupuesto de §5 no refleja ese coste | OPTIONAL (la recomendación vigente es mantener el diferimiento, sin coste) | Nombrar el `.csproj`, la prueba y el coste en R-D11 y en §5 |

## No comprobado sin ejecutar

- La regresión Full y los builds.
- Las corridas de CI 40768948090 y siguientes.
- La fusión automática de ROADMAP con otras ramas.
- El comportamiento real del comando en AutoCAD y de los scripts en RAM, incluido que el `.exe` se genere con `-o`: solo leí el código.
- El estado remoto de `origin/main`: no hice fetch.
