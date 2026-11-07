# I-64 — Paquete de vuelta en COORDINATOR_SMOKE1_RETRY_AUTHORIZATION_REQUIRED (tras 785a, 2026-11-07)

Responde a «I-64 — COORDINATOR POST-SMOKE-1 NOT PASS DISPOSITION» (FULL NEW SMOKE), literal en
`coordinator-post-smoke1-not-pass-disposition.md`, en esta misma carpeta.

**Resumen:**
- **Hecho:** host fresco y recheck SIN DERIVA (sesión + I-52), Candidato y dibujos iguales, ventanas reordenadas, precheck de capturas PASS y un método para S1-02.
- **Sin autorizar:** todos los puntos de la sección 7. Ninguna autorización anterior se transfiere, así que la sesión no empieza la ocurrencia.
- **Decisiones que la sesión señala de forma expresa (sección 7):** el precheck en dos etapas (punto 8), el `Ctrl+V` de SM-14c (punto 11) y el arrastre físico del Owner dentro de OPERATOR_EXCEPTION (punto 6).

## 1. Estado de partida

| Punto | Estado |
|---|---|
| 785a | `R20261105T050250Z-785a` = **NOT PASS / CLOSED**. Custodia `1917bc19e11e0fa09f4484956794825ba960098a`, CI **42324525500** push 4/4, releído hoy |
| Pendiente de prueba limpia | S1-06h; S1-08 / OV-20; capturas autoritativas de SM-17, SM-26, SM-27b y SM-34; observación de S1-02 |
| Producto | Sin corrección pendiente. No se tocó `src/`, `tests/` ni archivos de proyecto |

## 2. Candidato propuesto: REUTILIZACIÓN de `cc7503e3` (`identity-check.txt`, `session-recheck.txt`)

| Punto | Lectura (17:06–17:09Z) |
|---|---|
| Candidato | `cc7503e3e4e56bff3a2fc9de7ca30a5322b57ea6`, ancestro de HEAD |
| Punta de la rama | `1917bc19…` = origin, árbol limpio. Frente a `cc7503e3`: 145 archivos, **todos en `docs/automation/`**; `src/` + `tests/` = 0; proyecto = 0 |
| `RackCad.Plugin.dll` | `3C261B8F156E69192415E0084D0BC8774B3080ACE9D6CB4B9BB0C1760549B6A0`, 282112 bytes, LastWrite 2026-11-04T19:44:16Z, ProductVersion `1.0.0+cc7503e3…`. **Sin recompilar** |
| Otros ensamblados | Application `12B8E237…27B3`, Domain `BFAA5943…3959`, UI `363AE9FE…76E1`, iguales |
| Dibujos y biblioteca | A `90750E8B…4852`, B `FB9B97F9…682B`, C `EDE7CDB3…A53C`, D `03E5606C…F165`, biblioteca `B4CA2248…60E8`: iguales a SM-41 de 785a. 0 archivos `.bak`, `.sv$` o `.dwl` en sus carpetas |
| Guarda de procesos | `Test-AcadProcessGuard.ps1` `736463BD…E3A4`, `Inspect-LoadedRackCad.ps1` `86985EC9…E5AB`, `Save-ScreenRegion.ps1` `61983D32…6CDF`: iguales a 785a |

**Consecuencia en el kit:** el preflight usa la lógica ya aceptada en 785a, con HEAD esperado = `1917bc19`. Por eso **no conviene ningún commit de preparación antes de la ocurrencia**: cambiaría el HEAD esperado. `docs/automation/state/I-64.yml` sigue en el estado anterior y se actualizará con la siguiente custodia ordenada.

## 3. Recheck de host fresco, después del último cierre

| Punto | Sesión (17:06:34Z, `session-recheck.txt`) | I-52 (17:09:20Z, literal en `i52-recheck-reply.txt`) |
|---|---|---|
| acad.exe / RAMElements.exe | **0 / 0** (también a las 17:13:33Z) | 0 / 0 |
| Perfil, acad.exe | `<<Unnamed Profile>>`; R25.0.171.0.0 `2A75996F…6422` | igual |
| TRUSTEDPATHS, SECURELOAD | 19 entradas `a9ba0fd1…2498`; SECURELOAD 1 | igual |
| Registro | sin RackCad en HKCU Applications, Loaded, InstalledApplications ni HKLM. AssemblyMap = AppManagerPlugin y FeaturedAppsPlugin, LW 2026-11-02T19:54:39.812Z | igual; **SIN DERIVA**. Solo cambia LW de HKCU Applications, a 2026-11-07T02:25:48.576Z, con las mismas 7 claves |
| ApplicationPlugins | sin RackCad | sin RackCad |
| CoreUser | ver HOST-EXTRAS | sin cambio en lo que lee; no leyó Recent File List |
| RackLog | sin archivos nuevos: el último es del 2026-11-03 | no lo cubre |
| I-52 | — | sin ACK/RELEASE; P2 H10 retenido y NOT_AUTHORIZED. Sin operación de host activa ni prevista. **No toca el host desde la BASELINE nueva hasta su POST y su recomprobación** |

**HOST-EXTRAS frente al AFTER de 785a:** REPORT_REQUIRED, solo por `Recent File List` (`host-extras-diff-AFTER785a-vs-RECHECK.json`).
- En CoreUser y en la clave del producto, **el mismo valor, la ruta del dibujo D, cambia de posición**. Es el caso ya clasificado en la revisión final de 785a como EXPECTED MRU / UI-STATE REORDERING.
- LW de CoreUser: 2026-11-07T02:26:01.962Z. LW de la clave del producto: 2026-11-07T17:04:43.601Z.
- AssemblyMap, las entradas RackCad de CoreUser y el historial de NETLOAD no cambian.

**Actividad de host del Owner, posterior a 785a (propuesta: OWNER_AUTOCAD_ACTIVITY_PRE_BASELINE, la clasificación es del Coordinator):**
1. PID 16088, iniciada 2026-11-06T16:21:08Z, ya señalada en la disposición. Se cerró hacia 2026-11-07T02:25:46Z.
2. Una apertura breve que **pidió la sesión al Owner** para corregir la disposición de ventanas (sección 4). Se cerró hacia 17:04:42Z. Owner, literal: «Ya moví AutoCAD al monitor principal y lo cerré».

Ninguna la lanzó la sesión.

**Límite de la lectura:** la sesión no inspeccionó los módulos de esas instancias y no afirma si cargaron RackCad. Solo afirma lo medido después: registro de carga, ApplicationPlugins, NETLOAD, RackLog y DLL sin cambios.

**Observaciones sin atribuir causa:**
- El último cierre (17:04:42Z) no reescribió HKCU `Applications`; su LW sigue en 02:25:48.576Z.
- En `%TEMP%` quedan 5 autoguardados `.sv$` de fechas 2026-09-03, 2026-09-30 y tres de 2026-11-06 (15:16–15:59Z). Son anteriores o posteriores a las ventanas de I-64 y ninguno es de 785a. No se tocan. El control de SM-41 filtra por el inicio de la PID, así que no se atribuyen a la ocurrencia nueva.

## 4. Disposición de capturas: corregida y comprobada

| Punto | Estado |
|---|---|
| Monitores | principal x=0, 1920×1080; secundario a la izquierda, x=-1920 |
| Ventana de la sesión | `claude` en x -1920..0: **no corta el monitor principal** (`window-layout-final.txt`, 17:13:33Z) |
| Posición guardada de AutoCAD | `Show=3`, `Position=-8 -8 1928 1040`, LW 17:04:42.572Z: **maximizado en el principal**. Antes de la corrección estaba en el secundario, donde habría coincidido con la ventana de la sesión |
| Precheck (`Test-CapturePrecheck.ps1`, `capture-precheck-recheck.txt`, 17:08:22Z) | **PASS** con acad = 0 |

**Qué prueba ese precheck:**
- **Geometría:** ninguna ventana `claude` en el principal.
- **Mecanismo:** con una ventana normal, no siempre encima, de un solo color en el lugar de AutoCAD, `Save-ScreenRegion.ps1` registra ese color en las 6 regiones de T3D (paleta flotante y acoplada a cada lado, línea de órdenes): **0 píxeles distintos**.
- **Discriminación:** un marco de otro color dentro de una región se detecta **solo ahí** (900 píxeles distintos; 0 en las demás).

**Qué NO prueba:** la paleta real, que no existe antes de SM-00a. Ver el punto 8 de la sección 7.

**Lo que corrige el defecto de 785a:** cada captura se abre al tomarla, antes del paso siguiente. Una obstrucción detiene el paso afectado; se reordenan las ventanas sin tocar el dibujo y se repite la captura. Una notificación del sistema que tape una región se trata igual.

## 5. RunId propuesto

`R20261107T171052Z-6862`, **sin asignar**. No se crean su directorio ni su kit antes de la autorización.

## 6. Método propuesto para S1-02 (SM-29)

La herramienta de la sesión no puede arrastrar con el botón central, y no se usa ningún script para simularlo. El Owner confirmó, literal: «sí puedo hacer el arrastre».

1. **SM-00b, lectura añadida:** `(getvar "MBUTTONPAN")`. El registro del perfil no guarda ese valor, así que no se puede saber antes de abrir AutoCAD. Si no es 1: no se cambia (SM-STOP-04), se hace una pausa y el caso vuelve al Coordinator antes de seguir.
2. **SM-29, dibujo C, paleta visible:** la sesión hace la rueda y lee `(getvar "VIEWCTR")` = V0, con el foco en AutoCAD.
3. La sesión pide el arrastre en el chat. El Owner lo lee en el monitor secundario **sin hacer clic en el chat**, lleva el ratón físico al área de dibujo, arrastra con el botón central y suelta, una vez. La sesión no envía entradas mientras espera y solo toma vistas para ver el cambio.
4. La sesión escribe `(getvar "VIEWCTR")` = V1 **sin clic previo**.
   - V1 ≠ V0 prueba el encuadre.
   - Que el texto llegue a la línea de órdenes sin clic prueba que la paleta no robó el foco.
5. Captura de paleta y línea de órdenes, abiertas al momento.
6. La sesión pide al Owner su observación y la anota literal: si la vista siguió al arrastre y si la paleta parpadeó.
7. Siguen `ZOOM` `E` y `PAN`, como en T3D.

**Si falla:**
- Si el Owner tuvo que hacer clic en otro sitio, se anota y la prueba de foco queda solo como su observación.
- Si V1 = V0 o el arrastre no se puede hacer: S1-02 **no se marca PASS**, y el caso vuelve al Coordinator.

## 7. Lo que se pide autorizar

1. **Candidato:** reautorizar `cc7503e3` con el DLL `3C261B8F…B6A0`, sin recompilar, y HEAD = `1917bc19`, con las pruebas de la sección 2.
2. **RunId:** asignar `R20261107T171052Z-6862`.
3. **NETLOAD:** una sola carga no persistente de ese SHA, con «Load Once».
4. **HOST_RELEASE** para esta ocurrencia, y **PaletteKeyAcceptance** (en 785a fue ACCEPTED por el Coordinator; no se da por transferida).
5. **Guarda de procesos:** PG-0..PG-7 como en 785a, con una sola `open_application` y la lectura de módulos ligada a la PID.
6. **OPERATOR_EXCEPTION nueva**, para esta ocurrencia:
   - Operadora: la sesión. **El Owner hace físicamente el arrastre central de SM-29** y no toca el ratón ni el teclado de AutoCAD en el resto de la ocurrencia.
   - El registro diría, literal: «AutoCAD operated by implementation session under Coordinator OPERATOR_EXCEPTION for this occurrence, except the SM-29 middle-button drag, physically performed by the Owner at the session's request and observed by the session.»
7. **Scripts y firma:** la sesión ejecuta preflight, BASELINE, HOST-EXTRAS, POST y DIFF. El Owner firma el registro tras revisarlo; la firma no implica PASS.
8. **Precheck de capturas en dos etapas (lectura de la sesión, para confirmar).** La orden pide probar antes de SM-00a que la paleta completa es visible, pero antes de SM-00a la paleta no existe.
   - **Etapa 1, justo antes de SM-00a:** se repite `Test-CapturePrecheck.ps1` con acad = 0.
   - **Etapa 2, puerta en SM-03, antes de SM-04:** captura de la paleta real, abierta al momento. Debe mostrar la paleta completa, su título y la región de estado, la línea de órdenes y ningún contenido ajeno.
   - **Por captura:** comprobación de geometría antes de tomarla y apertura de la imagen después.
   - **Forma autoritativa, para confirmar:** recortes de región de T3D con `Save-ScreenRegion.ps1`, solo paleta y línea de órdenes, nunca la barra de título de AutoCAD. «Title/status region» se lee como el título y el estado **de la paleta**. En SM-26 y SM-27 la página de inicio muestra proyectos de terceros, así que ahí se captura solo la paleta.
9. **Lecturas añadidas de solo lectura** (SM-STOP-06 exige autorizarlas):
   - `(getvar "MBUTTONPAN")` y `(getvar "VIEWCTR")`, de la sección 6;
   - `(getvar "DBMOD")` en A, además de SM-02, SM-14h y SM-18: tras alejar la vista antes de SM-06, tras SM-12, tras SM-13, tras SM-15 y tras SM-16. Así se aísla cualquier cambio del bit 1.
10. **Regla de escritura de A.** Si el bit 1 se activa antes de SM-18: **STOP / vuelta** por causa ajena, como ordena el Coordinator. Si coincide con un paso prescrito de T3D sin desviación del operador (por ejemplo `LINE` y `Esc`), también se detiene: la sesión no clasifica la causa por su cuenta.
11. **Entrada de teclado.**
    - **Línea de órdenes:** sin `Ctrl+V` y sin aceleradores. Con la herramienta, trozos de ≤16 caracteres; cada resultado debe decir «Typed N char(s)»; el texto se comprueba antes de `Enter`.
    - **Tras tener el foco la paleta:** clic en la línea de órdenes, se comprueba que el primer trozo aparece y solo entonces se sigue.
    - **SM-14c (decisión expresa):** T3D exige `Ctrl+C` y `Ctrl+V` con el foco en la paleta, y la orden prohíbe `Ctrl+V` «for AutoCAD command-line entry». La sesión lo lee como compatible: SM-14c no es entrada en la línea de órdenes, y en 785a el `Ctrl+V` de SM-14c no llegó a AutoCAD. El incidente fue el de SM-14h, con el foco fuera de la paleta. Propuesta: se ejecuta literal solo con el foco visible en la casilla, comprobado en SM-14a. Si `PASTECLIP` llega a AutoCAD: STOP.
    - **SM-14a:** teclas reales una a una, para que la barra espaciadora actúe como en 0a77 y a578 (en 785a no conmutó la casilla). Se anota el conmutado. La casilla se vuelve a marcar con el clic de SM-15, antes de SM-16.
12. **Selección de A:** rueda para alejar (solo vista) antes de SM-06; SM-06 en el rack superior izquierdo `56ec2c3a…`, SM-07 en su copia de arriba a la derecha, SM-08 en el rack inferior `54ef4061…`.
13. **Actividad previa:** la clasificación de la sección 3.

**Presencia del Owner:** se necesita en SM-29 y en la firma. El resto del tiempo no debe mover las ventanas ni usar el ratón o el teclado en el monitor principal. 785a duró unas 2 h 24 min, con dos pausas.

Con una autorización que cubra estos puntos, la sesión genera `<run>/kit/` desde el kit de 785a con sustituciones contadas, añade `Test-CapturePrecheck.ps1`, actualiza la guía del operador y empieza por PG-0.

## 8. Lo que no se hizo
- Sin AutoCAD por parte de la sesión, BASELINE ni NETLOAD.
- Sin escrituras en el registro, el perfil, TRUSTEDPATHS, SECURELOAD, AssemblyMap, `Applications` ni ACL.
- Sin recompilar y sin commits desde `1917bc19`.
- Sin merge ni rebase de main y sin F2. Sin cambios de producto.
- Las 4 capturas originales tapadas siguen fuera de Git.
- No se crearon el directorio ni el kit del RunId propuesto.

**Fuentes:** carpeta `artifacts/orchestration/I-64/F1-SMOKE-1-RETRY-PREP/post-785a/`, transitoria. Manifiesto `evidence-hashes.txt`, SHA-256 `72B2291548602ADAD9CA86CEE2059968C766588118E579BA0CC65CE71B179C56`. Los JSON de HOST-EXTRAS y las disposiciones de ventanas contienen rutas locales o nombres de procesos: se redactan antes de cualquier custodia.
