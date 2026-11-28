# I-64 Smoke-1 R20261103T175351Z-0a77 — COORDINATOR_SMOKE1_REVIEW_REQUIRED

Candidato 09f2f1c3c4e64491018ef1489ce4dd3030d1e4a8 (HEAD = remoto; árbol limpio; sin commits durante la ocurrencia).
Propuesta de la sesión: **NOT PASS** (3 FAIL; ningún STOP). F2 no empieza. Sin merge ni rebase.

## Desviación de rol (decisión del Owner)
La sesión operó AutoCAD con computer use por instrucción literal del Owner («Mejor toma control de la computadora y hazlo tú»;
«No, opera tú»), contra T3D §1. Registro: disposition/owner-decision-session-operator.md. El Coordinator decide si la ocurrencia es
aceptable con operador = sesión.

## Integridad del Candidato (SM-00d1)
VERDICT = PASS (20:04:34Z): un solo RackCad.Plugin.dll, `<worktree>\src\RackCad.Plugin\bin\Debug\net8.0-windows\RackCad.Plugin.dll`,
SHA-256 2923E7F2DF4F62594810DBBC33003D36E6E4382EDECFB7E43C4A6C7C01F5AE26 (más RackCad.Application.dll 66E88EB1… MATCH).
NETLOAD: «Load Once»; nada en TRUSTEDPATHS, SECURELOAD, perfil ni registro.

## C1
- SM-03: FLOTANTE DENTRO DE LA PANTALLA (arriba a la izquierda, ~320×560 en ~80,80), visible y usable, sin rescate. PASS.
- SM-17: acoplada izquierda, acoplada derecha, flotante (dentro de la pantalla), ocultación automática (se pliega a la barra de
  título; se despliega con clic). Sin errores. PASS.

## Resultados
- PASS: SM-00a..e, SM-00d1, SM-01..SM-12, SM-14a..e (complementario), SM-14h, SM-15..SM-18, SM-20..SM-28, SM-30..SM-33, SM-35..SM-38,
  SM-40, SM-41. PR-13 (SM-10b) «Sin identidad» PASS; PR-12 (SM-11) «Diagnóstico» PASS.
- **FAIL SM-13 (S1-06f)** y **FAIL SM-34 (S1-07/OV-17)**: con una orden en curso (LINE; RACKEDITAR con su editor abierto) la paleta
  nunca muestra «Hay un comando en curso.» ni desmarca «Leer/Escribir en el dibujo». Causa (lectura de código): en
  `EventBridgeCore.OnSourceEvent`, `CommandWillStart` solo pone `Quiescent = false` y retorna, sin `SyncIdle` ni aviso; la vista solo
  se refresca con `NotifyStateChanged` (abrir, mostrar u ocultar el panel, cambio de sesión, drenaje).
- **FAIL SM-39 (RackLog)**: 2 ERROR contenidos durante SM-26 (20:24:21.278Z y 20:24:40.789Z), `AutoCadEventBridge.DocumentActivated`
  → `NullReferenceException` en `NativeId(Document)` (AutoCadEventBridge.cs:251): `DocumentActivated` llega con documento nulo al
  quedar solo la pestaña Start y el manejador (línea 123) no lo comprueba. Contenidas por `Contain`; sin efecto visible; ninguna fuga
  de transacción. Criterio «ninguna con `Workspace`» incumplido.
- NOT_APPLICABLE: SM-19 (PICKFIRST = 1); S1-06a..e = NOT_APPLICABLE_BY_DESIGN (A-7).
- SM-29: encuadre con botón central NOT_EXECUTED (la herramienta no arrastra con el botón central); rueda, ZOOM E y PAN PASS.

## Incidentes del operador (sesión)
1. SM-06: exploración fuera de orden para localizar la copia (selecciones adicionales); después la secuencia SM-06..SM-09 se repitió
   en orden.
2. SM-14a: la barra espaciadora conmutó la casilla con foco («Instrumentación desactivada»); re-activada antes de SM-16.
3. Tras SM-14: al escribir `(getvar "DBMOD")`, la vía de portapapeles de la herramienta mandó `Ctrl+V` a AutoCAD → `_pasteclip`;
   cancelado con Esc, nada insertado. No procede de la paleta.
4. SM-26: autoguardado SAVETIME de C en %TEMP% (.sv$), comportamiento de AutoCAD; los dibujos conservan su SHA (SM-41).

## Host
| Elemento | Antes | Después |
|---|---|---|
| acad.exe / RAM | 0 / 0 (19:48Z y 20:00:36Z) | 0 / 0 (20:28:57Z) |
| BASELINE T3C | código 0; CanonicalSha256 9fbf0c32af13b13ea832955b95da5f79a7f669e3243ddbe7a354b83790f2a116; archivo 69b553eb… | — |
| POST T3C | — | código 0; CanonicalSha256 0a230e7eb2cb5637749a5c00b9156c828d4aa236c6aa67007dcabd46fa6b7d16; archivo d8fd1c4e… |
| DIFF T3C (PKA = ACCEPTED) | — | **CLEAN**, código 0; ALLOWED 23 (1 R-A-01, 22 R-A-03); REPORT 0; FORBIDDEN 0; INVALID 0 |
| SECURELOAD / TRUSTEDPATHS / perfil / acad | 1 / 19 a9ba0fd1… / <<Unnamed Profile>> / 2A75996F… MATCH | sin cambios (DIFF CLEAN) |
| AssemblyMap | 2 valores, RackCad.Plugin ABSENT, LastWrite 2026-11-02T19:54:39.812Z | idéntico |
| CoreUser (15FCE767F24F74D1 / BlockPreviewUser) | 28 / 28 entradas | idéntico |
| HOST-EXTRAS | BEFORE c858f955…1a6f | AFTER 4709100e…4514; DIFF **REPORT_REQUIRED** |

HOST-EXTRAS REPORT_REQUIRED, solo en la clave del producto (estado de interfaz de AutoCAD por abrir los dibujos):
`Dialogs\NetLoadDialog` VALUES_CHANGED (INFORMATIVE, ya aceptado); `Dialogs\Select File` ADDED (MRU del diálogo Abrir; entra la ruta del
dibujo D, que contiene «rackcad»); `Recent File List` VALUES_CHANGED (MRU). Ninguna creación ni cambio de RackCad en AssemblyMap,
Applications ni Loaded.

Privacidad: los JSON de HOST-EXTRAS (BEFORE, AFTER, diff) guardan los valores crudos del registro, rutas locales incluidas. Son
locales (artifacts/ está en .gitignore) y no se citan. Si el Coordinator lo pide, se generan copias redactadas sin borrar los originales.

## I-52
Aviso antes de AutoCAD 19:52:43Z (64622bad…); devolución del host tras la POST (48ac2dd1…). Recomprobación recibida
(disposition/i52-post-run-recheck.txt, literal): **sin deriva** de lo registrado por I-52. acad = 0, RAM = 0; acad 2A75996F… igual;
TRUSTEDPATHS 19 / a9ba0fd1… igual; SECURELOAD 1; perfil <<Unnamed Profile>>; Applications, InstalledApplications, Loaded, Applications
del perfil, HKLM, AssemblyMap (AppManagerPlugin, FeaturedAppsPlugin; sin RackCad.Plugin) y ApplicationPlugins sin RackCad; ACL de sus 4
rutas idénticas; H10 no existe; bytes de P2 con 0 discrepancias. I-52 no concede ACK/RELEASE, libera su retención sobre H10 respecto de
esta ventana; P2 H10 sigue NOT_AUTHORIZED (CAD manager y Coordinator de I-52).

## Evidencia
operator/operator-record.md (firma del Owner pendiente), operator/session-operator-log.md, process/inspect-after-netload.txt,
preflight/*, profile/*, host-extras/*, diff/profile-diff.json, diff/host-extras-diff.json, disposition/*.

## Adenda posterior (2026-11-04; no reescribe lo anterior)
- Fe de erratas, ya registrada antes de la firma: la entrada de RackLog de 20:24:21.278Z es de SM-26 (cerrar C, el último dibujo) y la
  de 20:24:40.789Z es de SM-27 (clic en la pestaña Start con C reabierto), no «ambas durante SM-26». En SM-27 sí hubo efecto visible:
  el panel siguió mostrando «Sesión 7» sobre la página Start hasta abrir B. Ningún resultado cambia (SM-39 sigue FAIL).
- Revisión del Coordinator: NOT PASS; ocurrencia válida; desviación de rol ACEPTADA COMO DESVIACIÓN DE PROCEDIMIENTO; C1 PASS.
- Registro del operador firmado por el Owner («Firmo el registro.», 2026-11-04T15:06:53Z).
- Paquete siguiente: disposition/failure-analysis.md (COORDINATOR_SMOKE1_FAILURE_ANALYSIS_REQUIRED).
