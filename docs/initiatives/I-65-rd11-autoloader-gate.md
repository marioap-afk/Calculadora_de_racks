# I-65 — Gate correctivo R-D11: la tabla de secciones y materiales en el Autoloader

- **Estado:** AUTORIZADO dentro del alcance definido (decisiones §105). Gate correctivo **previo a READY**; no declara READY, no crea el
  Candidato final y no integra.
- **Autoridades:** disposición literal
  [I-65-owner-disposition-ready-decisions-autoloader-gate.original.txt](../automation/decisions/I-65-owner-disposition-ready-decisions-autoloader-gate.original.txt)
  (§3-§6) y respuesta del techo de Codex
  [I-65-owner-answer-codex-ceiling-rd11.original.txt](../automation/decisions/I-65-owner-answer-codex-ceiling-rd11.original.txt);
  evaluación de [I-65-ready-preparation.md](I-65-ready-preparation.md) §4; CD-I65-F4-10, que el Owner revierte con R-D11 B; ADR-0003 (el bundle
  lleva cero DLL de Autodesk y exactamente los cuatro DLL de RackCad). El Freeze ([Proposal V3.1](I-65-proposal-v3.1.md)) y ADR-0050 no
  cambian.
- **Protocolo:** ejecución delegada (AUTOMATION_PLAN §16; procedimiento en `docs/automation/agent-execution/`), como F1-F4: contrato de gate,
  planificación del Controller, aceptación A1-A8 y nc4, Worker con RED y GREEN, verificación del Controller y nc1-nc3. Después, evidencia
  local, Full, CI, Architect, Coordinator y registro (LIFECYCLE §7).

## 1. Revisión de alcance y límites (Coordinator, leída en `95aae3f1`)

| ID | Hecho | Consecuencia |
|---|---|---|
| RD11-F-01 | `RACKRAMPAQUETE` busca la tabla en `<carpeta del DLL del Plugin>\structural-analysis\ram-elements\section-material-map.json` (`RackRamElementsCommands.cs`, `Assembly.GetExecutingAssembly().Location`) y responde `Unavailable` si falta | el comando no cambia: basta con que el bundle ponga la tabla en `Contents/structural-analysis/ram-elements/`, junto a `Contents/RackCad.Plugin.dll` |
| RD11-F-02 | El `Content` del Plugin copia la tabla a la salida de compilación con el `Link` `structural-analysis/ram-elements/section-material-map.json`; el target `AssembleAutoloaderBundle` solo copia los cuatro DLL y `assets/catalogs` | causa del defecto (medido en §3) |
| RD11-F-03 | `deploy/verify-bundle.ps1` es una lista permitida fail-closed: hoy rechazaría la tabla («Archivo no autorizado en el bundle») | hay que admitir esa ruta exacta y exigir su identidad |
| RD11-F-04 | `deploy/install-bundle.ps1` copia el bundle completo y recursivo al staging | no cambia: la tabla viaja con la instalación |
| RD11-F-05 | La CI (`eng/ci/verify-autocad-references.ps1`, job «Build Plugin without AutoCAD») publica y ejecuta `verify-bundle.ps1` y `test-verify-bundle.ps1` | no cambia: la regresión del bundle corre en la CI |
| RD11-F-06 | `LaTablaNoViajaEnElPaqueteDelAutoloader_NiElDestinoDelPaqueteSeToca` (`StructuralAnalysisPluginCommandGuardTests.cs`) prohíbe la tabla en el target y en `deploy/` | se sustituye por pruebas positivas |
| RD11-F-07 | La guarda G1 examina los `.cs` de `src/`; C02 y los censos examinan el comando y su registro | ninguna se toca |
| RD11-F-08 | La tabla no tiene atributo `-text`; en este worktree el archivo de trabajo es igual al blob `d088e2a9` (LF; SHA-256 `FC3D9146…B619`) | el verificador compara con la fuente en disco, como hace con los catálogos; la igualdad con el blob aprobado se comprueba en la evidencia local del gate (A65-F4D-01; `.gitattributes` queda fuera del alcance) |
| RD11-F-09 | `MappingRowsSha256` se calcula sobre filas canónicas (`StructuralFingerprints.MappingRows`), no sobre los bytes del archivo | el fin de línea no altera las huellas |

**Límites (no cambian):**

- el contenido de la tabla y sus filas `verified`;
- mapeos, unidades, tolerancias, modelo neutral, adaptador RAM y comandos de producto (ningún `.cs` de `src/`);
- RACKAYUDA (R-D8 A);
- Freeze y ADR-0050;
- el contrato de cuatro DLL de ADR-0003 (solo se añade un archivo de datos);
- `build-bundle.ps1`, `install-bundle.ps1`, `test-install-bundle.ps1`, `PackageContents.template.xml`, `eng/`, `.github/`, `tools/`, `assets/`
  y `.gitattributes`.

## 2. Tarea `RD11-AUTOLOADER-TABLE` (una cadena RED → GREEN)

| Archivo | Cambio permitido |
|---|---|
| `src/RackCad.Plugin/RackCad.Plugin.csproj` | solo el target `AssembleAutoloaderBundle` y su comentario: copiar `assets/structural-analysis/ram-elements/section-material-map.json` a `Contents/structural-analysis/ram-elements/` (misma ruta relativa que el `Link` del `Content`) |
| `deploy/verify-bundle.ps1` | admitir **solo** `Contents/structural-analysis/ram-elements/section-material-map.json`, exigir su presencia y su SHA-256 igual a la fuente aprobada (parámetro `-SectionMapSourcePath`; por omisión `assets/…`); comentario y mensaje final |
| `deploy/test-verify-bundle.ps1` | el válido (con la tabla) se acepta; negativos nuevos: tabla ausente, tabla modificada, tabla en una ruta no autorizada, archivo extra junto a la tabla; los nueve negativos actuales intactos |
| `tests/RackCad.Tests/StructuralAnalysis/StructuralAnalysisPluginCommandGuardTests.cs` | sustituir la prueba de RD11-F-06 por pruebas positivas (target, verificador y harness); nada más del archivo |
| `README.md`, `docs/guias/interoperabilidad-ram-elements.md` (§2, §9, §10 y §11) y `docs/guias/despliegue.md` (§4) | quitar la limitación de CD-I65-F4-10 y decir que la tabla viaja en el bundle; las demás limitaciones se conservan literalmente |

**Forma:**

- **RED:** un commit que solo toca la prueba C# y el harness. Falla por aserción en el job Core (las pruebas positivas no encuentran la tabla
  en el target ni en el verificador) y en el job del Plugin (el harness exige la tabla en el bundle).
- **GREEN:** un commit con el target, el verificador y los documentos; no toca ningún archivo de pruebas del RED.
- **Comprobación local del Worker:** publicación del bundle en su carpeta `work/` con AutoCAD cerrado y el harness, en RED (el harness
  falla) y en GREEN (pasa).

**Invariantes:**

| ID | Invariante |
|---|---|
| RD-INV-01 | El target copia la tabla desde su fuente única, byte a byte, a `Contents/structural-analysis/ram-elements/section-material-map.json`. Los cuatro DLL, `catalogs/`, el manifiesto y la regeneración completa (`RemoveDir`/`MakeDir`) no cambian |
| RD-INV-02 | El verificador sigue siendo fail-closed y admite esa ruta exacta. Exige su presencia y la igualdad SHA-256 con la fuente aprobada, y rechaza cualquier otro archivo bajo `Contents/structural-analysis/` y cualquier otra ruta de la tabla. Ninguna comprobación existente se relaja |
| RD-INV-03 | El harness cubre el positivo y los cuatro negativos nuevos, cada uno con su motivo, más los nueve negativos actuales |
| RD-INV-04 | La única prueba C# que cambia es la de RD11-F-06. Ninguna guarda se edita ni se exime |
| RD-INV-05 | En el GREEN, el blob de la tabla sigue siendo `d088e2a94c83358e340d4d1b3a6ebdfc7b4dd6c2`; no cambia ningún `.cs` de `src/` ni nada de `tools/`, `assets/`, `eng/` o `.github/` |
| RD-INV-06 | Los documentos no presentan la tabla como validación estructural y conservan DIF-01, la edición del catálogo de RAM `UNKNOWN`, A65-F4R-07 y las propiedades `NOT_EVALUATED` |
| RD-INV-07 | Controles del Owner (K1-K6 y C-12): rutas absolutas en el worktree; ninguna ejecución de AutoCAD, RAM ni Python; ninguna instalación en `ApplicationPlugins` |

## 3. Bundle previo sin tabla (medido en `95aae3f1`, 2026-10-29 00:37Z)

`pwsh deploy\build-bundle.ps1 -Configuration Release -Output D:\r65-smoke\rd11-before-95aae3f1\publish` con AutoCAD cerrado
(ProductVersion `1.0.0+95aae3f1…`):

- **Bundle:** 24 archivos (manifiesto, cuatro DLL y 19 catálogos), **sin `Contents/structural-analysis/`**.
- **Verificación:** `verify-bundle.ps1` OK con 153 comprobaciones; `test-verify-bundle.ps1` OK con 10 casos (1 válido y 9 negativos).
- **Tabla:** solo está suelta en la carpeta de publicación (`publish\structural-analysis\ram-elements\`, SHA-256 `FC3D9146…B619`), fuera del
  bundle, así que una instalación por Autoloader no la lleva.

Inventario versionado en `docs/automation/evidence/I-65-rd11/bundle-before-95aae3f1-inventory.txt`.

## 4. Criterios de aceptación del gate

| ID | Criterio |
|---|---|
| AC-RD11-01 | Esta revisión de alcance se versiona antes de la planificación |
| AC-RD11-02 | La corrida push del RED tiene en `failure` el job Core (por aserción) y el job del Plugin (harness); compila |
| AC-RD11-03 | La corrida push del GREEN tiene los cuatro jobs en `success`; `git diff RED..GREEN` no toca pruebas |
| AC-RD11-04 | Controller `EXECUTION_VERIFIED` 14/14; nc4 y nc1-nc3 con su oráculo |
| AC-RD11-05 | Publicación local desde el SHA del gate en una carpeta desechable nueva: 25 archivos (los 24 anteriores y la tabla); verificador OK; harness con 14 casos |
| AC-RD11-06 | Tabla del bundle en `Contents/structural-analysis/ram-elements/section-material-map.json`, con SHA-256 igual al del blob `d088e2a9`; el lector del producto (`RamSectionMapReader`, del `RackCad.Application.dll` del propio bundle) da `Valid` con dos filas `verified` |
| AC-RD11-07 | Rechazos documentados: tabla ausente, modificada, en una ruta no autorizada y archivo extra |
| AC-RD11-08 | Comportamiento del comando bajo el Autoloader: equivalencia estática (la ruta que resuelve el comando = la del bundle; el instalador copia el bundle completo). La prueba real dentro de AutoCAD queda **preparada** (§6) y **sin ejecutar** |
| AC-RD11-09 | Core Full y UI Full locales sobre el SHA del gate; builds Debug de UI, Plugin y herramienta sin errores |
| AC-RD11-10 | CI del SHA exacto con los cuatro jobs en `success` |
| AC-RD11-11 | Architect CONFORMANT (sin REQUIRED) y Coordinator CONFORMING |
| AC-RD11-12 | `git diff --name-only 95aae3f1 <SHA del gate>` contiene solo el alcance de §2, `docs/automation/` y documentos de la unidad; Freeze, ADR-0050 y la tabla, sin cambios |

## 5. Presupuesto

| Recurso | Uso previsto | Techo |
|---|---|---|
| Codex | 51 planificación, 52 verificación, 53-55 nc1-nc3; 56-57 solo para reemitir una planificación rechazada (P-03), una reverificación o un fallo de transporte | 57 (desde 50) |
| `attempts` | primera delegación de la cadena: `Attempt` 7, sin incremento | 11 (desde 7) |
| Workers | 1; un REWORK es STOP y decisión del Owner | 1 |
| Architect | 1 revisión (y una relectura si hubiera REQUIRED) | — |

## 6. Prueba real del bundle dentro de AutoCAD (preparada, NO autorizada)

La disposición no admite sustituirla por NETLOAD. Exige instalar el bundle en `%AppData%\Autodesk\ApplicationPlugins\`. Hoy esa carpeta
está **vacía** y en la de todos los usuarios solo está `AcVBA2025.Bundle`. Es un cambio persistente del perfil del usuario y requiere
autorización de host aparte.

| Paso | Acción | Oráculo |
|---|---|---|
| HB-01 | BASELINE del host con la herramienta de R-D6; K1/K2; AutoCAD y RAM cerrados; listado de las dos carpetas `ApplicationPlugins` | sin RackCad.bundle previo |
| HB-02 | `pwsh deploy\install-bundle.ps1 -SourceBundlePath <bundle>` con el bundle publicado **y verificado** por `deploy\build-bundle.ps1` desde el SHA validado (staging, respaldo y sustitución verificada); se registra `git ls-files --eol` de la tabla en el checkout de compilación | `…\ApplicationPlugins\RackCad.bundle\Contents\structural-analysis\ram-elements\section-material-map.json` igual (SHA-256) a la fuente del checkout de compilación, y esa fuente igual al blob `d088e2a9` salvo el fin de línea: `FC3D9146…B619` con LF o `B22F7D9D…2E7D` con CRLF (A65-RD11-02; decisión del Owner sobre A65-RD11-01) |
| HB-03 | Abrir AutoCAD **sin** NETLOAD | los módulos de `acad.exe` incluyen `RackCad.Plugin.dll` desde `ApplicationPlugins\RackCad.bundle\Contents` |
| HB-04 | `RACKCANTILEVER` con REF-CANT-01; `RACKRAMPAQUETE` → estación 0, cargas de referencia, carpeta nueva fuera del repositorio | `Valid`, tres archivos, huella de entrada igual a la del DLL de compilación del mismo SHA |
| HB-05 | Cerrar AutoCAD sin guardar; desinstalar (borrar `RackCad.bundle` de la carpeta del usuario); POST/DIFF; K1/K2 | carpeta del usuario vacía de nuevo; DIFF sin PROHIBIDO |

**Recomendación:** ejecutarla en **W-R1** sobre `FINAL_CANDIDATE_SHA`. Cada SHA produce DLL distintos (la versión lleva el commit), así que
una prueba sobre el SHA de este gate no sería reutilizable para el Candidato.

## 7. Paradas

- Cualquier cambio fuera de §2.
- Una modificación del Freeze, de ADR-0050 o de la tabla.
- Relajar una comprobación del verificador.
- Un DLL adicional o un archivo de Autodesk.
- AutoCAD abierto al publicar (el script se niega: no se cierra AutoCAD).
- Una REWORK (tope de un Worker).
- Superar el techo de Codex.
- Los controles del Owner.
