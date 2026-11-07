# I-64 — Paquete de vuelta en COORDINATOR_SMOKE1_RETRY_AUTHORIZATION_REQUIRED (tras el STOP de a578; revisión 2, 2026-11-05)

Este paquete responde a dos documentos:
- «I-64 — COORDINATOR SMOKE-1 REVIEW», literal en `docs/automation/evidence/I-64-pilot/F1-SMOKE-1/R20261104T194513Z-a578/authorization/coordinator-second-smoke1-retry-review.md`.
- «I-64 — COORDINATOR OPERATOR DISPOSITION», literal en `coordinator-operator-disposition.md`.

Fuentes:
- lecturas propias de la sesión, solo lectura: `session-recheck.txt`, `host-extras-RECHECK.json(.sha256)` y
  `host-extras-diff-AFTER-vs-RECHECK.json`;
- `i52-recheck-reply.txt`: literal de I-52, con la nota aceptada;
- `process-guard/`: guarda de procesos, revisión 2, con sus borradores y autopruebas.

**Resumen:**
- **Hecho:**
  - STOP de a578 custodiado (`b5ea02f5`, CI 4/4);
  - Candidato `cc7503e3` y DLL `3C261B8F…B6A0`, sin recompilar;
  - recheck fresco SIN DERIVA, aceptado; acad.exe = 0;
  - RunId propuesto `R20261105T050250Z-785a`;
  - guarda de procesos ligada a AUTHORIZED_AUTOCAD_PID, adaptada a la excepción de operador;
  - OPERATOR_EXCEPTION = AUTHORIZED, con la sesión como operador.
- **Sin autorizar de forma explícita:** los puntos de la sección 7. Por eso la sesión vuelve aquí y no empieza la ocurrencia.
- **Nuevo a las 16:48Z:** había una `acad.exe` abierta desde las 16:00:18Z, con el DLL del Candidato cargado. La abrió el Owner y ya
  la cerró. El recheck se repitió después del cierre (sección 7, punto 8).

## 1. Ocurrencia histórica y custodia

| Punto | Estado |
|---|---|
| RunId histórico | `R20261104T194513Z-a578` = **STOP CONFIRMED** (HOST_ISOLATION_VIOLATION / MULTIPLE_AUTOCAD_INSTANCES). Desviación de rol PRESERVED, sin precedente |
| Cierre | `disposition/stop-closure.md`: la revisión aplicada sin cambiar el registro |
| Registro del operador | firmado por el Owner («Firmo el registro.», 2026-11-05T04:47:56Z) |
| Custodia | commit **`b5ea02f5d193d80e4b921b82bbeb7f87a0c8be0d`** (padre `cc7503e3`), push, CI **41971810963 push 4/4**. Evidencia §71. Unidades: a578 35 / 162; a578/prep 6 / 7 |
| Corrección del historial de NETLOAD | **aceptada por el Coordinator:** la escritura de `NetLoadDialog` de las 22:02:00.429Z está en la captura previa a la BASELINE y no se atribuye a a578. No es una corrección de la custodia: la custodia no la atribuía. La afirmación era de I-52 y quedó con su nota en este paquete |

## 2. Candidato propuesto: REUTILIZACIÓN de `cc7503e3`

| Punto | Estado |
|---|---|
| Candidato | `cc7503e3e4e56bff3a2fc9de7ca30a5322b57ea6`; producto = `src/` y `tests/` de `0e0d536b10350cc85e4a932a0d576b22fae22fa9` (0 archivos distintos) |
| `RackCad.Plugin.dll` | **`3C261B8F156E69192415E0084D0BC8774B3080ACE9D6CB4B9BB0C1760549B6A0`**, 282112 bytes, compilado desde `cc7503e3` el 2026-11-04T19:44:16Z y sin recompilar. Releído a las 15:41:18Z: igual |
| Otros ensamblados | Application `12B8E237…27B3`, Domain `BFAA5943…3959` y UI `363AE9FE…76E1`, iguales |
| Evidencia del Candidato | custodiada en `prep/candidate-cc7503e3/candidate.md`: Core Full 13172/13172; UI Full 1701/1718 (17 Skip); Debug UI y Debug Plugin PASS; CI 41898179409 4/4; PR-12/PR-13 revalidados |
| Dibujo D y herramientas T3C | D `03e5606c…f165` igual. Herramientas `750a9097`, `de642515` y `d0bb54be` iguales |
| Punta de la rama | `b5ea02f5` (custodia). Frente a `cc7503e3`: 45 archivos, todos en `docs/automation/` |

- **Por qué reutilizar:** el producto es idéntico. Recompilar en `b5ea02f5` cambiaría el SHA del DLL, porque el SHA del commit se
  estampa en el ensamblado.
- **Consecuencia en el kit:** el preflight exige HEAD = `b5ea02f5` exacto, en lugar de «HEAD = Candidato». Además, comprueba que la
  diferencia frente a `cc7503e3` esté solo en `docs/automation/` y que el DLL sea `3C261B8F…B6A0`.
- **Alternativa:** reemplazar el Candidato por uno documental recompilado. No se recomienda.

## 3. Recheck de host fresco (aceptado por el Coordinator)

| Punto | Sesión (04:56:19Z) | I-52 (04:58:15Z, literal; entrega idéntica recibida ~15:42Z) |
|---|---|---|
| acad.exe / RAMElements.exe | **0 / 0**. También a las 04:47:56Z, en cada paso posterior y a las 15:41:18Z y 15:42:26Z | 0 / 0 |
| Perfil, acad.exe, TRUSTEDPATHS, SECURELOAD | `<<Unnamed Profile>>`; R25.0.171.0.0 `2A75996F…6422`; 19 entradas `a9ba0fd1…2498`; SECURELOAD 1 | igual |
| Registro y AssemblyMap | sin RackCad en HKCU Applications, Loaded, InstalledApplications ni HKLM. AssemblyMap = AppManagerPlugin y FeaturedAppsPlugin | igual; últimas escrituras iguales |
| ApplicationPlugins | sin RackCad | sin RackCad |
| HOST-EXTRAS | recaptura frente al AFTER de a578: **CLEAN** (sin ningún cambio) | CoreUser RackCad sin cambio |
| RackLog | sin entradas desde el 2026-11-03 | — |
| I-52 | — | sin ACK/RELEASE; P2 H10 retenido. No toca el host desde la BASELINE nueva hasta su POST y su recomprobación |
| Carpetas ausentes | siguen inexistentes; asunto aparte | igual |

## 4. RunId propuesto

`R20261105T050250Z-785a`, **sin asignar**. No se crea su directorio ni su kit antes de la autorización.

## 5. Guarda de procesos (revisión 2, adaptada a OPERATOR_EXCEPTION)

Detalle en `process-guard/process-guard-procedure.md`.

**Puntos de guarda:**
- **PG-0:** acad = 0 y RAM = 0 antes de la BASELINE.
- **PG-0b:** aviso a las sesiones vivas (I-52 e I-62).
- **PG-1a:** acad = 0 y la **única** llamada a `open_application` de la ocurrencia.
- **PG-1:** `Register`. Exige exactamente 1 instancia y registra AUTHORIZED_AUTOCAD_PID: PID, ruta, SHA-256 e inicio.
- **PG-1w:** vigilancia continua cada 1 s hasta después de la POST.
- **PG-2:** SM-00d1 ligada a la PID. Un solo `RackCad.Plugin.dll` en esa PID, con la ruta y el SHA del Candidato.
- **PG-3:** puntos de control en SM-01, SM-03, SM-13, SM-17, SM-20, SM-26, SM-27, SM-35 y SM-36, y tras cada cambio de foco o clic
  fallido.
- **PG-4:** PRE-CLOSE. Después, cierre normal sin guardar.
- **PG-5:** `Closed`: acad = 0.
- **PG-6:** la POST exige `closed.ok` y una vigilancia sin STOP.
- **PG-7:** la bitácora de vigilancia demuestra una sola PID en toda la ventana.

**Reglas de operación de la sesión:**
- nunca `open_application` con AutoCAD abierto; foco con un clic en la propia ventana, después de captura y `Check`;
- registro contemporáneo de cada paso;
- sin acciones en lote sin documentar y sin exploración fuera de T3D;
- toda desviación vuelve al Coordinator;
- capturas solo de la paleta y de la línea de órdenes;
- dibujos solo con `OPEN`;
- «Load Once» en el aviso de seguridad.

**Borradores de solo lectura:**

| Archivo | SHA-256 | Autoprueba |
|---|---|---|
| `Test-AcadProcessGuard.ps1` | `736463BD…E3A4` | **20/20** |
| `Inspect-LoadedRackCad.ps1` (ligado a la PID) | `86985EC9…13E5AB` | **4/4** |
| `Save-ScreenRegion.ps1` (capturas en PNG para T3D §10) | `61983D32…6CDF` | región de 4×4 píxeles, después borrada |

## 6. Operador: OPERATOR_EXCEPTION = AUTHORIZED (sustituye la sección 6 de la revisión 1)

- **Disposición del Coordinator:** OPERATOR_EXCEPTION = AUTHORIZED, para UNA ocurrencia. El operador autorizado de AutoCAD es la
  sesión de implementación. El Owner queda INFORMED y la ocurrencia, autorizada por el Owner.
- Es una excepción explícita antes de la ejecución, no una desviación de rol no planeada. T3D §1 queda igual en la historia.
- La ocurrencia solo puede dar Smoke-1 PASS si se cumplen todas las condiciones de aislamiento del host.
- **No se registra «Confirmo que opero yo»:** sería falso. La respuesta del Owner fue «No puedo operar».
- **El registro del operador dirá, literal:** «AutoCAD operated by implementation session under Coordinator OPERATOR_EXCEPTION for
  this occurrence.» Nunca «Owner-operated».

## 7. Lo que sigue sin autorizar de forma explícita

1. **Candidato:** reautorización explícita de `cc7503e3` con el DLL `3C261B8F…B6A0` sin recompilar, punta `b5ea02f5`, y el cambio del
   paso 2 del preflight que eso implica. La revisión lo exige: «must be explicitly re-authorized».
2. **RunId:** asignar `R20261105T050250Z-785a`.
3. **NETLOAD:** una carga única y no persistente solo para ese SHA, con «Load Once». La de a578 está consumida. La revisión lo exige:
   «one-time NETLOAD authorization».
4. **HOST_RELEASE** para esta ocurrencia y solo para I-64 (Owner / Coordinator). I-52 no da ACK/RELEASE.
5. **Guarda de procesos, revisión 2:** los puntos PG-0..PG-7, las reglas de operación de la sección 4 y las capturas con
   `Save-ScreenRegion.ps1`.
6. **Scripts de preflight/BASELINE y POST:** los ejecuta la sesión, también la lectura de SECURELOAD con `reg query`. Se lo preguntó al
   Owner, que eligió «Ejecútalos tú». En a578 y 0a77 los ejecutaba el Owner.
7. **Firma del registro del operador:** propuesta: la firma el Owner, como en a578, y confirma el registro, no un PASS.

8. **Actividad de host NUEVA, posterior al recheck** (`host-activity-after-recheck.txt`):
   - A las 16:48:14Z hay **1 `acad.exe`**: PID 69576, iniciada a las 16:00:18Z por `explorer.exe`. No la lanzó la sesión.
   - Tiene cargados los 4 ensamblados RackCad **del bin del Candidato**, con sus SHA-256. Hubo un NETLOAD del DLL del Candidato fuera de
     cualquier ocurrencia.
   - A las 16:48:56Z, SECURELOAD, TRUSTEDPATHS, perfil, Applications, Loaded y AssemblyMap seguían sin cambios.
   - **Atribución del Owner, literal:** «Sí, lo abrí yo; ya lo cerré». La instancia salió hacia las 16:57:18Z.
   - **Recheck 2 de la sesión** (17:01Z, `session-recheck-2.txt`):
     - acad = 0 y RAM = 0;
     - Candidato, DLL, acad.exe, perfil, SECURELOAD y TRUSTEDPATHS iguales;
     - sin RackCad en el registro de carga ni en ApplicationPlugins; sin RackLog nuevo.
   - **HOST-EXTRAS RECHECK2 frente al AFTER de a578:** REPORT_REQUIRED, solo por `Recent File List`, 1 valor añadido y 1 quitado
     (16:57:17.899Z): el MRU del dibujo del Owner. AssemblyMap, CoreUser y el historial de NETLOAD, idénticos.
   - **HKCU `Applications` / `AcMr`:** reescritos a las 16:57:18.875–.877Z, el mismo patrón que en a578.
   - **Propuesta de la sesión:** clasificarla como actividad del Owner anterior a la BASELINE, solo de estado de interfaz de AutoCAD,
     sin deriva de seguridad, de carga ni de RackCad. La BASELINE nueva parte de ese estado. La clasificación es del Coordinator.
   - **Recheck 2 de I-52** (17:02:57Z, literal en `i52-recheck-2-reply.txt`): SIN DERIVA.
     - Solo hay una escritura nueva, la de HKCU `Applications` (16:57:18.875Z, mismo conjunto de claves).
     - Iguales: TRUSTEDPATHS, SECURELOAD, perfil, acad.exe, AssemblyMap, Loaded, InstalledApplications, CoreUser y el historial de
       NETLOAD.
     - Sin ACK/RELEASE; P2 H10 retenido; I-52 no toca el host.
     - Coincide con la lectura de la sesión.
   - **El recheck vigente para la autorización es el 2:** el de la sesión de las 17:01Z más el de I-52 de las 17:02:57Z.

Con una autorización que cubra los puntos 1 a 8, la sesión prepara `<run>/kit/` desde el kit de a578 con los cambios del procedimiento,
§6, y empieza por PG-0.

## 8. Lo que no se hizo
- Sin AutoCAD, BASELINE ni NETLOAD.
- Sin escrituras en el registro, el perfil, TRUSTEDPATHS, SECURELOAD, AssemblyMap, `Applications` ni ACL.
- No se tocaron las carpetas ausentes.
- Sin recompilar. El único commit es el documental de custodia `b5ea02f5`.
- Sin merge ni rebase de main (`a140eb3a`) y sin F2.
