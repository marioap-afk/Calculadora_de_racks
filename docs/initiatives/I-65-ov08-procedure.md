# I-65 — Procedimiento propuesto de OV-I65-08 (lista de la guía manual sobre el Candidato final)

- **Estado:** PREPARADO, **ejecución NO autorizada** (orden del Owner §104, R-D9). Requiere el `FINAL_CANDIDATE_SHA` declarado y una
  autorización expresa de la ventana de AutoCAD, que incluye el guardado y la reapertura de un DWG desechable.
- **Autoridades:** Freeze [Proposal V3.1](I-65-proposal-v3.1.md) §5 («OV-I65-08 | READY | Lista de la guía manual sobre el Candidato
  final»); [guía manual](../guias/validacion-manual-autocad.md) §2-§7 (con el bloque §7.1); LIFECYCLE §8 (todo escenario aplicable,
  incluida la lista de la guía, se ejecuta sobre `FINAL_CANDIDATE_SHA`).
- **Alcance:** el subconjunto de la guía que corresponde a I-65 (guía §4: «El contrato de la iniciativa determina qué subconjunto debe
  recorrerse»): sistema Cantilever y comando `RACKRAMPAQUETE`. El producto no cambia otros sistemas; los otros sistemas quedan cubiertos
  por las suites Full del Candidato y no se declaran validados manualmente aquí.

## 1. Identidad y condiciones de entrada (si falta una, no se abre AutoCAD)

| ID | Condición |
|---|---|
| O8-PR-01 | `FINAL_CANDIDATE_SHA` declarado, con su bloque §7.1 completo (Core y UI Full, builds, CI exacto con `ui-tests`) |
| O8-PR-02 | DLL Debug de ese SHA en una carpeta nueva `D:\r65-smoke\<sha>\bin-Debug-net8.0-windows\`, con ProductVersion `1.0.0+<sha>` y SHA-256 custodiado; la tabla `structural-analysis\ram-elements\section-material-map.json` junto al DLL e igual al blob de `assets/` |
| O8-PR-03 | Versión de AutoCAD y biblioteca de bloques identificadas (ruta resuelta por `BlockLibraryLocator.ResolvePath` y SHA-256, guía §7.1) |
| O8-PR-04 | AutoCAD y RAM cerrados; nadie usando el equipo; I-62 sin operación de pantalla; coordinación de host según la decisión vigente del Owner para READY |
| O8-PR-05 | BASELINE del host con la herramienta decidida en R-D6; K1/K2 antes; carpeta de corrida nueva `D:\r65-smoke\<sha>\ov08-<runid>\` fuera del repositorio |
| O8-PR-06 | Autorización expresa de **guardar, cerrar y reabrir** un DWG **desechable** solo en esa carpeta (nunca en el repositorio ni en carpetas del usuario) |
| O8-PR-07 | Archivo de cargas de referencia `D:\r65-smoke\<sha>\inputs\ref-cant-01.loads.json` igual al versionado, para las exportaciones del paso O8-07 |

## 2. Pasos

| Paso | Acción | Oráculo |
|---|---|---|
| O8-01 | AutoCAD nuevo; `NETLOAD` «Load Once» del DLL de O8-PR-02 (diálogo navegado por carpetas) | el DLL de la lista de módulos de `acad.exe` es el custodiado; `RACKCAD` abre el menú; `RACKAYUDA` abre la referencia (si R-D8 = B, incluye `RACKRAMPAQUETE`) |
| O8-02 | Dibujo nuevo; `RACKCANTILEVER`: construir REF-CANT-01 exactamente como en [W1 §4.1](I-65-f4-w1-autocad-procedure.md) y colocar la vista principal | origen, orientación, escala, capas y bloques catalogados correctos (guía §5.1) |
| O8-03 | `RACKEDITAR` → cambio visible de una dimensión → **Actualizar**; después devolverla al valor de REF-CANT-01 y Actualizar | redefinición en sitio; ninguna copia se desplaza; vistas coherentes (guía §5.1) |
| O8-04 | `RACKEDITAR` → **Insertar** las demás vistas (frontal, lateral, planta) | mismas GUID y nombre lógico; sin rack huérfano; `RACKLISTA` muestra un rack con sus vistas (guía §5.3) |
| O8-05 | BOM desde el editor y `RACKBOMTOTAL` | piezas, cantidades y longitudes coherentes con el dibujo; sin duplicar por vistas (guía §5.4) |
| O8-06 | `RACKRAMPAQUETE` → estación 0, archivo de O8-PR-07, carpeta nueva `…\ov08-<runid>\pkg-A` | `Valid`, tres archivos; anotar la **huella de entrada H1** |
| O8-07 | **Guardar** el DWG en la carpeta de corrida, **cerrar** y **reabrir** | el DWG reabre sin avisos de RackCad |
| O8-08 | `RACKEDITAR` sobre el rack reabierto | `Kind` Cantilever, nombre, GUID, vista o sección y valores editables iguales (guía §5.2) |
| O8-09 | `RACKRAMPAQUETE` de nuevo → estación 0, mismo archivo de cargas, carpeta nueva `…\pkg-B` | `Valid`; **huella de entrada H2 = H1** y huella de fuente igual en ambos manifiestos (persistencia del diseño por round-trip) |
| O8-10 | Persistencia: modificar un campo editable, Actualizar, guardar, cerrar, reabrir y `RACKEDITAR`; volver al valor de referencia | el cambio persiste y se recupera (guía §5.5) |
| O8-11 | Fallo visible: `RACKRAMPAQUETE` sobre un rack que no es Cantilever (si hay uno en la biblioteca) y cancelación en cada pregunta | `Unsupported` con mensaje; la cancelación no crea carpeta |
| O8-12 | `RACKDUPLICAR` del rack | GUID nuevo; contrastado con COPY (guía §5.2) |
| O8-13 | Cerrar AutoCAD **sin guardar** cambios posteriores; POST del host; K1/K2 | DIFF del host sin PROHIBIDO; DWG y paquetes solo en la carpeta de corrida |

**Legacy (guía §5.5):** el producto no cambia el formato persistido de Cantilever en I-65. Si el Owner dispone de un DWG Cantilever
anterior representativo, se abre y se repite O8-08 y O8-10. Si no, el punto queda **NOT_EVALUATED** con la razón: no se declara
validado.

## 3. Evidencia (formato de la guía §7)

Ficha con fecha, validador (el Owner firma; el Coordinator no le atribuye firma), commit, ruta y SHA-256 del DLL, versión de AutoCAD,
escenario, biblioteca, pruebas y builds del bloque §7.1, el checklist (NETLOAD y menú; dibujo; actualización en sitio; round-trip;
multivista; BOM; persistencia o legacy; escenarios de I-65), resultado por punto, fallos con severidad, H1 y H2, y la **confirmación
explícita del validador**.

Capturas recortadas a la zona técnica y sin datos identificables (§90). Paquetes y DWG en local con su SHA-256; se versionan solo
resúmenes y copias redactadas.

## 4. Paradas

- Identidad distinta (DLL, tabla, biblioteca, AutoCAD).
- Diálogo de confianza o seguridad de AutoCAD.
- Pérdida de datos o error silencioso.
- Huella H2 ≠ H1.
- Escritura fuera de la carpeta de corrida.
- DIFF PROHIBIDO.
- Conflicto de host.
- Fin de la ventana autorizada.

Una ejecución fallida no se repite sin autorización.

## 5. Duración

45-60 min dentro de la ventana de AutoCAD de READY (W-R1), después de los escenarios OV-01..04 o antes de ellos, según disponga la
autorización.
