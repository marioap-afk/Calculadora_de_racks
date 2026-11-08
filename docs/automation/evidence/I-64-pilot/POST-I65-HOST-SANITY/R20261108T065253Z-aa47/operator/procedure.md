# I-64 — POST-I65 HOST SANITY CHECK: procedimiento (RunId R20261108T065253Z-aa47)

**Orden:** «I-64 — COORDINATOR INTEGRATION REVIEW» (2026-11-08), sección POST-I65 HOST SANITY CHECK, alcance 1-10. **No es Smoke-1**: no se
repite SM-00..SM-41, ni C1 ni C2, salvo que esta comprobación muestre una regresión. Kit generado desde el de 6862 con sustituciones
contadas (`kit/generation-record.txt`).

**Identidad que se carga:** producto sincronizado `3f5c7014351d1dc59de4eee86a7d1319fbeccc07`; HEAD de la rama
`53683249c229329e3f576fc8f9b9919b1755c63f` (solo documentos encima). DLL Debug del worktree, sin recompilar:
- `RackCad.Plugin.dll` `0C0F2FDE0987C489BABAF2ACB9A46936117D044EAF45C706A303E6B5B1C56C0B` (ProductVersion `1.0.0+3f5c7014…`);
- `RackCad.Application.dll` `12A7C29A…7558`, `RackCad.Domain.dll` `24EA6643…9507`, `RackCad.UI.dll` `E49AC0A6…9826`.

**Camino no destructivo de `RACKRAMPAQUETE`:** el rack selectivo de **Dibujo B**. Según el código (`RackRamElementsCommands.cs`,
líneas 64-79), al elegir un rack que no es Cantilever, el comando escribe
`RackCad paquete estructural: Unsupported. Solo se exporta un rack Cantilever; el elegido es de tipo <Kind>.` y vuelve **antes** de
pedir la estación, el archivo de cargas y la carpeta. Por eso no lee la tabla ni las cargas y no publica nada. La evidencia de I-65
ya lo establece: W-R1a O8-11 (`docs/automation/evidence/I-65-ready/wr1a-R20261029T1633Z/wr1a-report.md`, «Unsupported. Solo se
exporta un rack Cantilever; el elegido es de tipo selective.») y SM-09 del smoke de I-65 (`docs/initiatives/I-65-f4-autocad-smoke-procedure.md`).

## Pasos

| Id | Quién | Acción | Resultado esperado | Alcance |
|---|---|---|---|---|
| HS-00 | sesión | `preflight`: acad = 0, RAM = 0; árbol limpio, HEAD = 53683249, sin delta de producto desde 3f5c7014; DLL `0C0F2FDE…` con `+3f5c7014`; herramientas T3C y autoprueba 26/26; SHA de los dibujos; SECURELOAD (reg query); HOST-EXTRAS BEFORE; BASELINE T3C; RackLog BEFORE | código 0 | 1, 2, 10 |
| HS-01 | operador | abrir AutoCAD **una sola vez**; si sale Start, «New» | `Drawing1`; una sola `acad.exe` | 1 |
| HS-01a | sesión | `register` (exactamente 1 acad.exe, SHA `2A75996F…`) y vigilancia continua | REGISTER PASS | 1 |
| HS-02 | operador | `(getvar "CPROFILE")` y `(getvar "SECURELOAD")` | `<<Unnamed Profile>>` y el valor de la BASELINE | 10 |
| HS-03 | operador | `NETLOAD` → `src\RackCad.Plugin\bin\Debug\net8.0-windows\RackCad.Plugin.dll` del worktree de I-64 | aviso de seguridad: **solo «Load Once»**. Si solo ofrece un cambio persistente (confiar en la carpeta o cargar siempre): Cancelar y **STOP** | 3 |
| HS-04 | sesión | `inspect` | un solo `RackCad.Plugin.dll` en la PID autorizada, en el bin del worktree, `0C0F2FDE…`; los 4 módulos con su SHA | 2, 3 |
| HS-05 | operador | abrir `<unidad-local>\Dibujos\Dibujo B.dwg` | abre limpio | — |
| HS-06 | operador | `RACKPANEL` | la paleta de RackCad abre una sola vez, dentro de la pantalla, con su estado normal (sesión del dibujo activa) | 4, 5 |
| HS-07 | operador | clic en el rack selectivo; esperar 2 s; `Esc` | la paleta muestra «Un rack» y su RackId; vuelve a «Ninguno» tras `Esc` | 5 |
| HS-08 | operador | `RACKRAMPAQUETE`; en «Selecciona el rack Cantilever:» clic en el **mismo** rack selectivo | `RackCad paquete estructural: Unsupported. Solo se exporta un rack Cantilever; el elegido es de tipo selective.`; **ninguna** pregunta más. Si aparece «Indice de estacion»: `Esc` y **STOP** | 6, 7 |
| HS-09 | sesión | `racklog` | ningún ERROR atribuible a la carga o a la coexistencia | 8 |
| HS-10 | sesión | `check PRE-CLOSE` | PASS | 1 |
| HS-11 | operador | cerrar AutoCAD **sin guardar** (si pregunta: «No») | AutoCAD cerrado | 9 |
| HS-12 | sesión | `closed`; `fixtures` | acad = 0; A, B, C, D y biblioteca sin cambios; ningún `.dwl` | 9, 10 |
| HS-13 | sesión | `post` y `diff`; lectura posterior de las claves de carga; recheck de I-52 si está accesible | POST 0 o 5; DIFF y HOST-EXTRAS sin deriva inesperada | 10 |

**Capturas (sesión):** la paleta tras HS-06 y HS-07, y la línea de órdenes tras HS-08, con `Save-ScreenRegion.ps1` (copia de la
pantalla real). Cada captura se abre en el momento.

**STOP inmediato:** una segunda `acad.exe` o una PID ambigua; cualquier opción de seguridad persistente; un SHA o una ruta de módulo
distintos; que `RACKRAMPAQUETE` pida la estación, las cargas o la carpeta; cualquier escritura en los dibujos. Tras un STOP: cerrar
sin guardar, POST y DIFF, y volver al Coordinator.

**Fuera de alcance:** guardar, editar o dibujar racks; cualquier `RACKRAMPAQUETE` sobre un Cantilever; RAM Elements; tocar el
registro, el perfil, TRUSTEDPATHS, SECURELOAD, AssemblyMap o ACL; invocar modelos (P-01 activo); F2.

**Vuelta:** COORDINATOR_POST_I65_HOST_SANITY_REVIEW_REQUIRED, con el SHA del DLL, la identidad del proceso, la evidencia de Load
Once, las observaciones de RACKPANEL y RACKRAMPAQUETE, RackLog, el estado final del host y la propuesta PASS / NOT PASS / STOP.
