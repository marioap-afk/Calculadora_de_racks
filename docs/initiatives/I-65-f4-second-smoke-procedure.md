# I-65 — F4: procedimiento del segundo smoke de `RACKRAMPAQUETE` (filas `verified`)

Documento de la unidad (Coordinator, 2026-10-16), por la disposición del Owner «F4-D Acceptance / Second AutoCAD Smoke Preparation»
([texto literal](../automation/decisions/I-65-owner-disposition-f4d-acceptance-second-smoke-prep.original.txt); decisiones
[§67](../automation/decisions/I-65.md)). Reutiliza la línea base, el POST y el DIFF del primer smoke
([preflight/POST](I-65-f4-host-release-and-preflight.md) §3-§6) y su informe
([smoke 745653e1](../automation/evidence/I-65-f4/smoke-745653e1/smoke-report.md)).

> **Estado (2026-10-16): EJECUTADO** bajo el OWNER HOST RELEASE (decisiones §68-§69; [informe](../automation/evidence/I-65-f4/smoke2-51fc82f1/smoke-report.md)): `Valid` y paquete generado; validador 15/16 (V-11 falso negativo por doble escape); DIFF `INFORMAR`. Texto original de preparación:
>
> **Estado: PREPARADO. EJECUCIÓN NO AUTORIZADA.** AUTOCAD EXECUTION = NOT AUTHORIZED; RAM EXECUTION = NOT AUTHORIZED. La solicitud de
> ventana (ACK + RELEASE) se envió a I-52 y a I-64 ([custodia](../automation/evidence/I-65-f4/host-coordination-smoke2-2026-10-16.md));
> la liberación del primer smoke **no** sigue vigente.

## 1. Identidad fija (cualquier diferencia invalida el procedimiento)

| Elemento | Valor |
|---|---|
| Código compilado | `51fc82f131a287ef66458a433c614c389033769d` (GREEN verificado de F4-D; el cierre `ad595b42` solo cambia `docs/`) |
| DLL | `D:\r65-smoke\51fc82f1\bin-Debug-net8.0-windows\RackCad.Plugin.dll`, SHA-256 **`fa334b144507fc8922171036edae878f773c003164f4c98302cdc67c9291aedb`**, ProductVersion `1.0.0+51fc82f131a287ef66458a433c614c389033769d` |
| Salida completa (paquete binario) | 32 archivos; manifiesto `D:\r65-smoke\51fc82f1\output-manifest.txt`, SHA-256 `5c9a6d62e84d23317d19a6d67a4883e8f7edb9dac33a94511761220dbdf265d2` ([registro de compilación](../automation/evidence/I-65-f4/smoke2-dll-build-51fc82f1.md)) |
| Tabla junto al DLL | 3 279 bytes; SHA-256 de bytes `fc3d9146…b619` = blob `d088e2a9…` = objetivo aprobado; LF, sin BOM (normalizada = bytes) |
| Archivo de cargas (copia congelada del aprobado) | `D:\r65-smoke\51fc82f1\inputs\ref-cant-01.loads.json`, 592 bytes, SHA-256 `25f48e7f75d879c4668c3d9b37f69fda0cd5442a407c03ff76362754623cae05`, blob `3855c268…` = `tests/RackCad.Tests/StructuralAnalysis/Fixtures/ref-cant-01.loads.json` en `51fc82f1` |
| Valores esperados de las entradas efectivas | `D:\r65-smoke\51fc82f1\expected-inputs.json` ([copia](../automation/evidence/I-65-f4/smoke2-expected-inputs.json)), calculado con el `RackCad.Application.dll` de la salida: filas `verified`, `canonicalText` de cada fila, huella de filas `54ca9f6c80376d857755e68fc16d7de58a771b52cb0a0b7fa452a5df6191c98c` |
| Validador del paquete | [validate-smoke2-package.py.txt](../automation/evidence/I-65-f4/validate-smoke2-package.py.txt) (solo lectura; 16 comprobaciones; autoprueba con paquetes sintéticos PASS) |
| DLL del primer smoke | `2e04a4bb…e41d`, intacto en el worktree y en `D:\r65-smoke\745653e1\`; **no** se usa ni se sustituye |

## 2. Condiciones previas (todas)

| # | Condición |
|---|---|
| PR2-01 | Orden expresa del Owner que autorice ejecutar el smoke (abrir AutoCAD y NETLOAD) |
| PR2-02 | ACK + RELEASE **expreso** del Coordinator de I-52, del Owner o del CAD manager, con ventana, referencias frescas y autorización de «Load Once» para **este** DLL (`fa334b14…aedb`); un acuse de recibo no basta |
| PR2-03 | Confirmación expresa del Coordinator de I-64 (orden frente a su Smoke-1, sin ocupar el host ni solapar su BASELINE/POST) |
| PR2-04 | `acad.exe` = 0 y `RAMElements.exe` = 0; BASELINE limpia frente a las referencias del ACK (perfil, `TRUSTEDPATHS`, `SECURELOAD`, `acad.exe`) |
| PR2-05 | SHA-256 del DLL, del manifiesto de salida, de la tabla, del archivo de cargas y de `expected-inputs.json` iguales a §1 |
| PR2-06 | Instantánea K1/K2 de los checkouts antes de abrir AutoCAD |

## 3. Pasos

| # | Paso |
|---|---|
| SM2-01 | Carpeta de corrida nueva `D:\r65-smoke\51fc82f1\run-<RunId>\` (`baseline\`, `post\`, `captures\`); comprobar que `pkg-01` **no** existe |
| SM2-02 | BASELINE B-01..B-10 ([§3](I-65-f4-host-release-and-preflight.md)), con B-08 (ACL) y B-09 (DLL) sobre `D:\r65-smoke\51fc82f1\bin-Debug-net8.0-windows` |
| SM2-03 | Aviso previo a I-52 e I-64 por su canal |
| SM2-04 | Abrir AutoCAD 2025 (una sola instancia) con un dibujo nuevo desechable |
| SM2-05 | SHA-256 del DLL justo antes de cargar; `NETLOAD` navegando hasta `D:\r65-smoke\51fc82f1\bin-Debug-net8.0-windows\RackCad.Plugin.dll` (el diálogo abre en la última carpeta usada: no cargar nada de otra); en el aviso de seguridad, **solo «Load Once»** si el RELEASE lo autoriza; nunca «Always Load» |
| SM2-06 | Dibujar REF-CANT-01 con `RACKCANTILEVER` como en el primer smoke: nombre `REF-CANT-01`, 2 estaciones, separación 48, una cara Y+, 3 niveles, troquel 0, claro 24; columna y base `AISC-W-W12X26`, base 48; brazo sencillo `AISC-HSS-RECT-HSS4X4X_250`, corte 36; resolver la línea e insertar la vista frontal |
| SM2-07 | `RACKRAMPAQUETE` → rack → estación `0` → archivo de cargas `D:\r65-smoke\51fc82f1\inputs\ref-cant-01.loads.json` → carpeta `D:\r65-smoke\51fc82f1\run-<RunId>\pkg-01` **con barras invertidas** (la fábrica exige una ruta Windows absoluta y rechaza `/`). Teclear en trozos cortos, **nunca** por el portapapeles (`PASTECLIP` del primer smoke) |
| SM2-08 | Captura de la línea de comandos. Esperado, literal: `RackCad paquete estructural: Valid. Paquete publicado en D:\r65-smoke\51fc82f1\run-<RunId>\pkg-01`, `  Huella de entrada: <64 hexadecimales>`, `  Archivo escrito: manifest.json`, `  Archivo escrito: rackcad-ram-build.py`, `  Archivo escrito: structural-loads.json`; ninguna línea de diagnóstico |
| SM2-09 | Validación de solo lectura: `python -I validate_smoke2_package.py <pkg-01> D:\r65-smoke\51fc82f1\expected-inputs.json "<carpeta tecleada>" <informe.json>` → **PASS 16/16**; comparar además los 8 primeros y los 8 últimos caracteres de la huella de la línea de comandos con `analysisInputFingerprint` del manifiesto |
| SM2-10 | Registrar del manifiesto las limitaciones y los supuestos del piloto (advertencia, ids de supuestos, exclusiones) y los textos `source` (DIF-01) |
| SM2-11 | Cerrar AutoCAD **sin guardar**; `acad.exe` = 0 |
| SM2-12 | POST B-01..B-10 y DIFF ([§4-§5](I-65-f4-host-release-and-preflight.md)) |
| SM2-13 | Instantánea K1/K2 después; checkouts sin cambios |
| SM2-14 | Aviso de cierre a I-52 e I-64 con `acad.exe` = 0 y el resultado del DIFF |
| SM2-15 | Custodia: `manifest.json` y el informe del validador versionados; el script y las cargas, por SHA-256; capturas; el paquete queda en la carpeta local desechable |

## 4. Validador del paquete (A65-F4D-01, A65-F4D-07 y A65-F4D-10)

Compara el paquete **real** con sus **entradas efectivas**; no usa ninguna huella de fixtures de prueba (el `rackId` del DWG y el
origen del catálogo cambian las huellas, A65-F4D-07). Registra por separado el SHA-256 de los bytes y el de la versión normalizada a LF
(A65-F4D-01).

| Id | Comprobación |
|---|---|
| V-01 | La carpeta existe y contiene exactamente `manifest.json`, `rackcad-ram-build.py` y `structural-loads.json` |
| V-02 | Ninguna carpeta temporal `*.tmp-*` del publicador en la carpeta padre (la publicación no quedó a medias) |
| V-03 | Los tres archivos no están vacíos |
| V-04 | `manifest.json` es UTF-8 estricto, JSON válido y sin BOM |
| V-05 | `files[]` del manifiesto = SHA-256 de los bytes reales del script y de las cargas |
| V-06 | `loadsFile` = `structural-loads.json` con el SHA-256 de sus bytes = el del archivo de cargas aprobado (`25f48e7f…`) |
| V-07 | `structural-loads.json` es idéntico byte a byte al archivo de cargas aprobado |
| V-08 | `packageFolder` = la carpeta tecleada = la carpeta real |
| V-09 | Esquema, adaptador (`rackcad-ram-elements-adapter` 1.0.0) y producto declarado sin cambios |
| V-10 | `source.kind` Cantilever, estación 0, `rackId` presente; `sourceFingerprint` y `analysisInputFingerprint` de 64 hexadecimales |
| V-11 | El script embebe la misma huella de entrada y la carpeta |
| V-12 | `readback` = `ram-readback.json` y repite la huella |
| V-13 | Las dos filas del manifiesto = las de la tabla distribuida (sección, material, E, ν, G nulo, 490 pcf, `source`, `verified`, `verifiedBy`, `verifiedOn`, `canonicalText`) |
| V-14 | `mapping.rowsSha256` = `54ca9f6c…98c` (calculado con el DLL sobre la tabla distribuida) |
| V-15 | `verifiedBy` aparece dos veces como bytes UTF-8 `4d6172696f2050c3a972657a20e28094…` («Mario Pérez — Owner / Engineering Validator»), sin escapes `\u` ni caracteres de sustitución |
| V-16 | Supuestos del piloto presentes (SHA-256, advertencia y lista) |

## 5. Condiciones de aceptación

| # | Condición |
|---|---|
| AC2-01 | Línea de comandos `Valid` con el texto de SM2-08 y sin diagnósticos |
| AC2-02 | Validador **PASS 16/16**; huella de la línea de comandos = la del manifiesto = la del script |
| AC2-03 | El modelo neutral fue `Valid` (lo implica AC2-01) y la salida no es parcial (V-01..V-03) |
| AC2-04 | DLL con el mismo SHA-256 en la BASELINE, antes de `NETLOAD` y en el POST |
| AC2-05 | AutoCAD cerrado sin guardar; `RAMElements.exe` nunca iniciado; nada analizado |
| AC2-06 | DIFF `LIMPIO`, o `INFORMAR` aceptado expresamente por I-52; ningún `PROHIBIDO` |
| AC2-07 | K1/K2 sin cambios; avisos previo y de cierre enviados |

## 6. Condiciones de parada

| # | Parada |
|---|---|
| ST2-01 | Falta alguna de PR2-01..PR2-06, o la BASELINE difiere de las referencias del ACK (sin abrir AutoCAD) |
| ST2-02 | Otra `acad.exe` o una sesión de AutoCAD de I-52 o I-64 antes o durante la ventana |
| ST2-03 | Aviso de seguridad sin autorización de «Load Once», o cualquier petición de confianza distinta |
| ST2-04 | SHA-256 del DLL distinto en cualquier punto |
| ST2-05 | Resultado distinto de `Valid`, cualquier línea de diagnóstico o «La publicacion fallo»: se captura, se cierra sin guardar y **no se reintenta** (un único paquete) |
| ST2-06 | Paquete escrito en otro lugar, parcial, con archivos de más o con restos `*.tmp-*` |
| ST2-07 | Cualquier comprobación del validador en `FAIL` |
| ST2-08 | DIFF con clase `PROHIBIDO` |
| ST2-09 | Necesidad de ejecutar RAM, analizar el modelo o cambiar `TRUSTEDPATHS`, `SECURELOAD`, perfil, ACL o configuración persistente |

Toda parada: registro literal, aviso de cierre a I-52 e I-64 y decisión del Owner. Ninguna parada se resuelve cambiando código.

## 7. Limitaciones que se registran (no se infieren del éxito del paquete)

- **DIF-01:** el peso unitario de RAM (486.94 pcf) difiere −0.62 % de la referencia (490 pcf); diferencia informativa aceptada **solo**
  sin peso propio (V1 lo excluye por código). No se cambia ninguna tolerancia.
- **Combinaciones:** OD-F4-14 = CMB-3; pertenecen a la validación de equivalencia posterior y **no** se deducen de que el paquete se genere.
- **Hipótesis del piloto:** las del manifiesto (`assumptions`), con su advertencia; la tabla `verified` no es validación estructural ni
  certificación del material; la edición formal del catálogo de RAM no está verificada.
- **Paquete desechable:** no es el paquete definitivo ni se lleva a RAM.

## 8. Riesgos residuales

| # | Riesgo | Mitigación |
|---|---|---|
| R2-01 | Carpeta tecleada con `/` → `RAM-PACKAGE-FOLDER-INVALID` | SM2-07 exige `\`; el validador comprueba V-08 |
| R2-02 | `PASTECLIP` al teclear rutas largas | trozos cortos, sin portapapeles |
| R2-03 | El diálogo de `NETLOAD` abre en la carpeta de otra iniciativa | navegar hasta la carpeta de §1 y comprobar el SHA-256 antes |
| R2-04 | El DWG dibujado no reproduce exactamente REF-CANT-01 (p. ej., la pendiente del brazo se muestra `0.438`) y las cargas no casan con los miembros | cualquier diagnóstico es ST2-05; la igualdad exacta con el fixture se valida en la OV |
| R2-05 | `verifiedBy` no ASCII en el manifiesto | V-15 sobre bytes y valor |
| R2-06 | Fin de línea de JSON en una copia nueva del repositorio (A65-F4D-01) | identidad por bytes y por blob; la salida usada tiene LF |
