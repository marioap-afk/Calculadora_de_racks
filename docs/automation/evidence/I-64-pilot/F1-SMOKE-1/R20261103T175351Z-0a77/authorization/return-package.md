# I-64 — Paquete de vuelta en COORDINATOR_SMOKE1_RETRY_AUTHORIZATION_REQUIRED (2026-11-03T17:53Z)

Fuentes: `i52-recheck-reply.txt` y `i52-lastwrite-reply.txt` (literales de I-52), evidencia de I-65 en `origin/main` `a140eb3a`
(decisiones CD-I65-FC-62/64/67, evidencia §123/§124, `I-65.yml`, hoja de OV), lecturas propias de la sesión (sin registro).

| Punto | Estado |
|---|---|
| acad.exe / RAMElements.exe | 0 / 0 (sesión 17:53:50Z; I-52 en sus dos lecturas) |
| Perfil activo | `<<Unnamed Profile>>` (única clave R25.0 `ACAD-8101:409`) |
| acad.exe | R25.0.171.0.0, `2A75996FD2A5C5EE0376FD5BA7CCED227A098193FF495E8CEEDA3A96FE326422` (igual) |
| TRUSTEDPATHS | 19 entradas, `a9ba0fd1c2f15dfc7ee4477e0ea62e710ac842d89d2b0e18102b2362553a2498` (igual) |
| SECURELOAD | 1, solo referencia; regla: lectura inmediata del Owner antes de la BASELINE |
| Registro | sin RackCad en HKCU Applications (7 estándar), Loaded, InstalledApplications, Applications de perfil ni HKLM; I-52 no afirma diff línea a línea frente a su base anterior (la tiene resumida) |
| ApplicationPlugins | sin RackCad (APPDATA vacío; ProgramData no existe; PF = AcVBA2025; PF(x86) = AppManager, FeaturedApps) |
| ACL | huellas icacls idénticas al recheck de W-R1a; bytes P2 sin cambio |
| AssemblyMap\RackCad.Plugin | AUSENTE; la clave solo tiene AppManagerPlugin y FeaturedAppsPlugin; última escritura 2026-11-02T19:54:39.812Z |
| AppDataLow/CoreUser | claves RackCad sin escribir desde 2026-07-23/28 (a: preexisten); NETLOAD MRU escrito 2026-11-02T22:51:27Z (b: cambiaría con NETLOAD); (c) no demostrado |
| H10 | I-52 retiene P2 H10 y su ventana desde la BASELINE hasta la POST y su recomprobación |
| Actividad de AutoCAD no atribuida | 2026-11-02T19:54Z (AssemblyMap), 2026-11-02T22:51Z (NetLoadDialog, Save Drawing As, Select File), 2026-11-03T16:50Z (Applications) a 17:34:20Z (raíz R25.0 y Profiles\General) |
| Candidato | HEAD = remoto = `09f2f1c3c4e64491018ef1489ce4dd3030d1e4a8`, árbol limpio, DLL `2923E7F2…AE26` |
| RunId | `R20261019T180407Z-1be6` = NOT_STARTED / SUPERSEDED; propuesto (NO asignado) `R20261103T175351Z-0a77` |

Discrepancia: el cierre de I-65 (hasta 2026-11-03T16:34Z) da `AssemblyMap\RackCad.Plugin` como PRESENTE y sin limpieza autorizada,
pero su base era la lectura del 2026-10-30. La clave se escribió por última vez el 2026-11-02T19:54Z, fuera de toda ventana conocida.
Ni A (no hubo procedimiento del dueño) ni B (no hay valor que aceptar) se cumplen tal como están definidos: decide el Coordinator.
