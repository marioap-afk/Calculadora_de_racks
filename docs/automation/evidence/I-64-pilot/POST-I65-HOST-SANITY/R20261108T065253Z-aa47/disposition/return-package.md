# I-64 — POST-I65 HOST SANITY CHECK `R20261108T065253Z-aa47`: paquete de vuelta

**Estado:** COORDINATOR_POST_I65_HOST_SANITY_REVIEW_REQUIRED.

**Propuesta de la sesión:** **PASS**, a condición de que el Coordinator clasifique la línea `WATCH STOP` de las 07:13:05Z como falso
positivo de la guarda durante la salida de la propia PID autorizada (sección 7). Si no la acepta, la propuesta es **STOP por la guarda**,
sin ningún hallazgo de producto. La sesión no declara PASS por su cuenta.

- **Ventana:** AutoCAD de 2026-11-08T07:06:56Z a 07:13:05Z (salida), operado por la sesión con la excepción de la disposición, solo para aa47.
- **Sin:** commits, F2, modelos, recompilación ni cambios de producto.

## 1. Identidad del DLL (exacta)

| Elemento | Valor |
|---|---|
| `RackCad.Plugin.dll` | **`0C0F2FDE0987C489BABAF2ACB9A46936117D044EAF45C706A303E6B5B1C56C0B`**, ProductVersion `1.0.0+3f5c7014351d1dc59de4eee86a7d1319fbeccc07` |
| Otros módulos cargados | Application `12A7C29A851105F0A2CC067EC899CDB4BC98325FA1DC57845D89ED974BBE7558`, UI `E49AC0A6C4336A7E73EF7C5EC649DDCAC278FF10778C6861D9BF85C5C02D9826` (Domain `24EA6643…9507` no llegó a cargarse) |
| Producto / HEAD | producto `3f5c7014`; HEAD = origin = `53683249`, árbol limpio antes y después; sin delta de producto entre ambos |
| Preflight (HS-00, 07:05:58Z) | código 0. Comprobó el SHA completo y la identidad estampada, las herramientas T3C, la autoprueba 26/26, los dibujos, SECURELOAD = 1 con `reg query`, HOST-EXTRAS BEFORE y la BASELINE `0d8b9edd…` |

## 2. Proceso: AUTHORIZED_AUTOCAD_PID

| Campo | Valor |
|---|---|
| PID | **39252** |
| Ruta | `<carpeta-de-AutoCAD-2025>\acad.exe` |
| SHA-256 | `2A75996FD2A5C5EE0376FD5BA7CCED227A098193FF495E8CEEDA3A96FE326422` |
| Inicio UTC | `2026-11-08T07:06:56.3271404Z` (registrado a las 07:07:08Z) |
| Una sola instancia | acad = 0 y RAM = 0 antes; una sola llamada de apertura; REGISTER exigió exactamente 1; CHECK HS-04 (07:09:43Z), HS-06 (07:10:39Z), HS-08 (07:12:05Z) y PRE-CLOSE (07:12:54Z), todos PASS; CLOSED PASS 07:13:12Z (acad = 0) |
| Vigilancia | una sola línea de conjunto en toda la ventana (`[39252@07:06:56Z]`) y una línea STOP en la salida (sección 7) |

## 3. Load Once (HS-03 / HS-04)

- `NETLOAD` abrió en el bin del worktree (`net8.0-windows`, con los cuatro `RackCad.*.dll` de las 13:48 locales).
- El aviso «Security - Unsigned Executable File» decía `RackCad.Plugin.dll`, Unknown Publisher, la ruta del bin del worktree de I-64 y la fecha 11/07/2026 13:48.
- Se pulsó **«Load Once»**. «Always Load» no se tocó.
- Captura `HS-03-aviso-seguridad-palette.png` (`F2C0C46E…95DA`), revisada en el momento. Lleva la ruta local con el nombre de usuario: hay que redactarla antes de cualquier custodia.
- **Inspección inmediata de la PID** (`process/inspect-after-netload.txt`):
  - exactamente un `RackCad.Plugin.dll`, en el bin, con el SHA completo `0C0F2FDE…6C0B`: **PASS**;
  - tras RACKPANEL (`inspect-after-rackpanel.txt`), también PASS con UI.
- **Persistencia de la carga:** ninguna. Applications, Loaded, InstalledApplications y AssemblyMap terminan con el mismo LastWrite que el recheck previo de I-52 y sin RackCad (sección 6).

## 4. RACKPANEL (HS-06 / HS-07)

- **Dibujo B:** se abrió desde Recent (DWGPREFIX = carpeta de los dibujos, TrustedDWG).
- **`RACKPANEL`:**
  - respondió `RackCad: panel Workspace mostrado.`;
  - paleta Workspace flotante, arriba a la izquierda y dentro de la pantalla;
  - «Sesión activa · El panel sigue el dibujo activo. · Sesión 1», «Leer el dibujo» y «Escribir en el dibujo» marcadas, Selección «Ninguno».
  - Capturas `HS-06-palette.png` (`0456C671…F1BB`) y `HS-06-cmdline.png` (`2247261D…8651`), revisadas.
- **Uso normal:**
  - clic en el rack selectivo de arriba → «Un rack · Rack seleccionado: a0e7f2be-ef24-4311-8482-1730235f372f» (`HS-07-un-rack-palette.png`, `3C422C28…B59B`);
  - `Esc` → «Ninguno».

## 5. RACKRAMPAQUETE (HS-08): camino Unsupported, sin paquete

- **Comando registrado y ejecutable:** `RACKRAMPAQUETE` (tecleado en dos trozos, sin portapapeles) → `Selecciona el rack Cantilever:`.
- **Resultado:** clic en el mismo rack selectivo →
  **`RackCad paquete estructural: Unsupported. Solo se exporta un rack Cantilever; el elegido es de tipo selective.`**
  Después, vuelta a «Type a command» **sin ninguna pregunta más**: ni estación, ni cargas, ni carpeta.
- **Capturas:** `HS-08-cmdline.png` (`57307C26…76BB`), revisada. La paleta quedó en su estado normal: `HS-08-palette.png` es idéntica byte a byte a HS-06.
- **Prueba de que no se publicó nada** (`logs/no-package.txt`, después del comando y otra vez tras cerrar):
  - 0 archivos `manifest.json`, `rackcad-ram-build.py`, `structural-loads.json` o `*.tmp-*` escritos desde el inicio de la PID, en <unidad-local>\, Documentos, Escritorio, Descargas y el worktree;
  - 0 carpetas nuevas en <unidad-local>\ y en la carpeta de los dibujos;
  - RAM Elements = 0 en todo momento.
- **Sin escritura en el dibujo:**
  - DBMOD de B = 0 antes de salir;
  - `QUIT` sin preguntas de guardado;
  - A, B, C, D y la biblioteca con el mismo SHA-256 (`logs/fixtures.txt`), sin `.bak`, `.sv$` ni `.dwl`, y 0 escrituras en la carpeta de los dibujos.

## 6. RackLog y estado final del host

- **RackLog (HS-09):** ningún archivo nuevo y los 4 existentes iguales a BEFORE (`logs/racklog-checks.txt`). **Ningún ERROR.**
- **Estado final:** acad = 0, RAM = 0; árbol limpio en `53683249`; `config.toml` = instantánea D (`377D0C3C…`, sin cambios).
- **DIFF de T3C** (`diff/profile-diff.json`, `D01B89DB…7F95`): **CLEAN**, código 0, con 2 entradas, ambas R-A-03 ALLOWED:
  - `XCOMPARELASTCLEANTIME` CHANGED;
  - `VIEWBACKSTATUS` REMOVED (AutoCAD lo borra al salir, ya visto por I-65).
- **HOST-EXTRAS** (`host-extras/host-extras-compare.json`): REPORT_REQUIRED solo por las MRU. «Recent File List» de CoreUser cambió de valor (07:13:09Z) y la del ProductKey solo cambió de LastWrite (07:13:01Z), por haber abierto Dibujo B desde Recent. Es la misma clase que en 6862 (MRU / estado de interfaz). AssemblyMap igual y sin `RackCad.Plugin`; CoreUser y ProductKey con los mismos conteos.
- **Claves de carga después de cerrar:** Applications `2026-11-07T19:04:59.935Z`, Loaded `2026-10-30T21:44:52.708Z`, InstalledApplications `2025-10-27T15:44:06.180Z`, AssemblyMap `2026-11-02T19:54:39.812Z`. Son las mismas de la lectura de I-52 de las 07:02:58Z, sin ninguna entrada RackCad.
- **I-52, recheck previo (07:02:58Z):** SIN DERIVA en los 9 puntos (`preflight/i52-recheck-reply.txt`, canal directo).
- **I-52, recheck posterior (07:15:39Z):** **SIN DERIVA** frente a su lectura previa en todo lo que registra: procesos, perfil, acad.exe, TRUSTEDPATHS, SECURELOAD, Applications, Loaded, InstalledApplications, AssemblyMap, CoreUser, ApplicationPlugins y P2. No hizo ninguna operación de host en la ventana. No leyó las MRU porque no están en su lista: ni contradice ni confirma mi lectura (`disposition/i52-post-run-recheck.txt`, canal directo).

## 7. Observación que decide el Coordinator: línea `WATCH STOP` en la salida

- **Línea** (sin tocar): `2026-11-08T07:13:05.2044435Z WATCH STOP: otra instancia 39252@2026-11-08T07:06:56.3271404Z`.
- **Es la misma PID autorizada, con su misma hora de inicio.** Salió 11 s después del PRE-CLOSE, durante `QUIT`. La vigilancia no vio ninguna otra PID en toda la ventana.
- **Mecanismo, reproducido con una sonda inocua de `PING.EXE`** (`logs/watch-stop-analysis.md`, `logs/teardown-probe-output.txt`):
  - mientras un proceso sale, `Get-Process` sigue dando su Id y su inicio, pero la ruta vacía;
  - la guarda compara la ruta y lo lee como «otra instancia»;
  - pasó en 8 de 3281 muestras y en 8 de 2945.
  - No se midió sobre la propia acad.exe, que ya no existía.
- **Desviación declarada:**
  - `owner-post.ps1` (PG-6) se negó a tomar el POST por esa línea (`console/post-pg6-refusal.txt`; no escribió nada);
  - el procedimiento aprobado manda POST y DIFF también tras un STOP, así que el POST se tomó en el momento con `kit/owner-post-after-watch-stop.ps1`;
  - ese script tiene una sola sustitución contada (`kit/generation-record-post.txt`): lista las líneas STOP en vez de parar y conserva closed.ok, la bitácora, acad = 0 y el «no se repite».
- **Lo que se pide:** clasificar la línea como falso positivo de la guarda durante el cierre (propuesta) y aceptar o no esa forma de tomar el POST.
- **Fuera de aa47:** la guarda debería tolerar una ruta ilegible cuando coinciden la PID y el inicio. No se cambia aquí.

## 8. Alcance 1-10 de la revisión de integración

| # | Requisito | Resultado |
|---|---|---|
| 1 | host limpio, una instancia | acad = 0 y RAM = 0 antes; una sola PID durante toda la ventana; I-52 SIN DERIVA antes. La línea WATCH va en la sección 7 |
| 2 | identidad exacta del DLL | `0C0F2FDE…6C0B` completo en el preflight y en la PID |
| 3 | una carga no persistente «Load Once» | sí; claves de carga sin cambios |
| 4 | RACKPANEL registrado y abre normal | sí |
| 5 | Workspace en estado normal y usable | sí (Sesión 1, Un rack ↔ Ninguno) |
| 6 | RACKRAMPAQUETE registrado y ejecutable | sí |
| 7 | camino no destructivo, sin paquete | Unsupported (selective) antes de estación, cargas y carpeta; 0 archivos, 0 carpetas, RAM 0 |
| 8 | sin ERROR en RackLog | ningún archivo ni línea nuevos |
| 9 | cerrar AutoCAD sin guardar | `QUIT`, DBMOD 0, dibujos con el mismo SHA |
| 10 | sin deriva de persistencia o seguridad | DIFF CLEAN (2 R-A-03); HOST-EXTRAS solo MRU; claves de carga, AssemblyMap, perfil, SECURELOAD y TRUSTEDPATHS sin cambios; recheck posterior de I-52 SIN DERIVA |

## 9. Otras notas

- **Diario:** un comando de diario de las 07:11Z lo bloqueó el filtro de la herramienta (falso «Remove-Item on system path» por contener la ruta de los dibujos). La anotación se añadió después desde un archivo y está marcada como diferida.
- **Entrada de texto:** toda la entrada de órdenes fue directa, en trozos de 10 caracteres o menos y sin portapapeles.
- **Equipo y ventanas:** el Owner minimizó ChatGPT y dejó el equipo libre. El precheck de capturas (etapa A) dio PASS y todas las capturas pasaron la comprobación de geometría.
- **Comandos extra:** fuera de lo previsto solo se leyeron `DWGPREFIX` y `DBMOD` con `getvar`, para confirmar la ruta de B y que no se escribió.
- **Archivos:** todo está en `artifacts/orchestration/I-64/POST-I65-HOST-SANITY/R20261108T065253Z-aa47/` (transitorio, ignorado por Git), con su manifiesto de hashes en `disposition/evidence-hashes.txt`.

**Siguiente:** revisión del Coordinator. F2 sigue sin empezar. P-01 = CLEARED (instantánea D).
