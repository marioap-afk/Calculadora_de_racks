# I-64 — Paquete de vuelta en COORDINATOR_SMOKE1_RETRY_AUTHORIZATION_REQUIRED (2026-11-04T19:45Z)

Segundo reintento de Smoke-1, tras el cierre «I-64 — COORDINATOR F1-T3-HOST-C2 CLOSE» (C2 PASS). Fuentes:
- `../candidate-cc7503e3/candidate.md` (bloque del Candidato, pasos locales, revalidación de PR-12/PR-13);
- `i52-recheck-reply.txt` (literal de I-52);
- lecturas propias de la sesión, solo lectura y sin registro.

## 1. Candidato

| Punto | Estado |
|---|---|
| Candidato | `cc7503e3e4e56bff3a2fc9de7ca30a5322b57ea6` = HEAD = remoto; árbol limpio; producto = `src/` y `tests/` de `0e0d536b` (0 archivos distintos) |
| Local sobre el SHA exacto | Core Full 13172/13172; UI Full 1701/1718 (17 Skip existentes); Debug UI 0 advertencias; Debug Plugin 0 errores, solo MSB3277; enfocadas Core 577/577 y UI 66/66 |
| CI del SHA exacto | run 41898179409, push, 4/4 success (único run de ese SHA) |
| `RackCad.Plugin.dll` | `3C261B8F156E69192415E0084D0BC8774B3080ACE9D6CB4B9BB0C1760549B6A0` (282112 bytes) |
| `RackCad.Application.dll` | `12B8E237F70596FA2F8030D66E51CC54F410A8906CB78B61A4056873058E27B3` |
| PR-12 / PR-13 | revalidados contra esos bytes de Application: PR-13 = `NoIdentity`, PR-12 = `Diagnostic`, sin RackId; DXF D y su `.pristine` = `03e5606c…f165` |
| Dibujos A/B/C y biblioteca | SHA-256 sin cambios |
| Históricos | `4d79e910` = HISTORICAL_SMOKE1_STOP_CANDIDATE; `09f2f1c3` = HISTORICAL_SMOKE1_NOT_PASS_CANDIDATE; no se reutilizan |
| Commits | ninguno después de `cc7503e3`; no se recompila hasta la ocurrencia |

## 2. Recheck de host (I-52, solo lectura, 2026-11-04T19:36Z)

| Punto | Estado |
|---|---|
| acad.exe / RAMElements.exe | **1** / 0. El acad.exe es del Owner, iniciado el 2026-11-03T21:31:43Z (después del cierre de `0a77`); I-64 no lo lanzó ni lo usa. La sesión contó 1 antes y después de la verificación local. Antes de la BASELINE se exige 0; I-52 también necesita 0 para su P2/H10 |
| Perfil activo | `<<Unnamed Profile>>` |
| acad.exe | R25.0.171.0.0, `2A75996FD2A5C5EE0376FD5BA7CCED227A098193FF495E8CEEDA3A96FE326422` (igual; la sesión lo confirmó) |
| TRUSTEDPATHS | 19 entradas, `a9ba0fd1c2f15dfc7ee4477e0ea62e710ac842d89d2b0e18102b2362553a2498` (igual) |
| SECURELOAD | 1, solo referencia; regla: lectura inmediata del Owner antes de la BASELINE |
| Registro | sin RackCad en HKCU Applications, InstalledApplications, Loaded, Applications de perfil ni HKLM. HKCU Applications con última escritura 2026-11-03T22:55:41.461Z, con el acad.exe del Owner abierto |
| AssemblyMap\RackCad.Plugin | AUSENTE; la clave solo tiene AppManagerPlugin y FeaturedAppsPlugin; última escritura 2026-11-02T19:54:39.812Z, igual que en la BASELINE de `0a77` |
| ApplicationPlugins | sin RackCad |
| AppDataLow/CoreUser | `Applications\RackCad` con las mismas últimas escrituras (2026-07-23 y 2026-07-28) que en la recomprobación posterior a `0a77`; I-52 no revisó los MRU (los captura HOST-EXTRAS en la BASELINE nueva) |
| ACL de I-52 | ya no comparables, porque sus rutas no existen (sección 3); bytes de P2 sin cambio (HASHES-I1-V2 `bd49d0f4…bd5a`, tip `fb6b5648`) |
| H10 / ventana | I-52 no tiene operación de host hoy y se compromete a no tocar el host entre la BASELINE y la POST de I-64, más su recomprobación. Su H10 no puede avanzar mientras no se restablezca su carpeta |
| ACK/RELEASE | I-52: «no me corresponde»; lo eleva a su Coordinator / Owner |

## 3. Hallazgo nuevo de host: `<carpeta-de-I-52>` y `<carpeta-local-de-I-65>` ya no existen

- Lo informó I-52 y la sesión lo comprobó en solo lectura (las dos rutas, ausentes).
- I-52 declara que no las borró. Esta sesión tampoco:
  - su transcripción no tiene ninguna orden de borrado sobre esas rutas;
  - sus scripts no borran nada.
- La última escritura de la raíz `<unidad-local>\` es del 2026-11-04T16:06:46Z. A esa hora la sesión solo leía archivos del worktree (lecturas de C2).
- Propuesta: clasificarlo como actividad de host no atribuida anterior a la BASELINE, como en `0a77`. No se atribuye al Owner, a I-52,
  a I-65 ni a otra iniciativa.
- **Impacto en I-64:** ninguno técnico.
  - Ni el kit de `0a77`, ni las herramientas de T3C, ni T3D ni T3C nombran esas rutas.
  - La carga es un NETLOAD del DLL del worktree.
  - TRUSTEDPATHS no cambió.
- **Impacto en I-52:** sus ACL, su atestación y sus scripts de H10 vivían ahí; restablecerlos es de su Coordinator o del CAD manager.

## 4. Decisiones que necesita la autorización

1. **Ocurrencia única.** RunId propuesto, NO asignado: `R20261104T194513Z-a578`. Datos de la ocurrencia:
   - Candidato `cc7503e3`;
   - DLL `3C261B8F…B6A0`;
   - NETLOAD única y no persistente solo para ese SHA (ninguna autorización anterior se transfiere);
   - PaletteKeyAcceptance.
2. **HOST_RELEASE.** I-52 no lo da. En `184b` y `0a77` lo dieron el Owner y el CAD manager.
3. **Clasificación del hallazgo de la sección 3**, y si bloquea o no la ocurrencia de I-64.
4. **Rol de operador.** Según T3D §1, opera el Owner. En `0a77` operó la sesión con computer use, por decisión del Owner, y se aceptó como
   desviación de procedimiento. Para esta ocurrencia hace falta una decisión explícita; si vuelve a operar la sesión, es una desviación
   nueva que se declara.
5. **acad.exe = 1.** Lo cierra el Owner antes de la BASELINE; la sesión no lo cierra.
6. **Kit.** Tras la autorización, la sesión prepara `<run>/kit/` con el kit de `0a77` cambiando el RunId, el Candidato y el SHA del DLL:
   - HOST-EXTRAS BEFORE y AFTER;
   - inspección del DLL cargado tras el NETLOAD;
   - SECURELOAD leído por el Owner;
   - BASELINE, POST y registro del operador nuevos.
   La guía del operador lleva las comprobaciones críticas del cierre de C2:
   - SM-03 y SM-17 (C1);
   - SM-13 («Leer el dibujo» y «Escribir en el dibujo» desmarcados, «Hay un comando en curso.», sin filas `drain` ni `selection.read`
     durante la orden);
   - SM-34 (igual con RACKEDITAR);
   - SM-26 y SM-27 («Sin documento», sin error de RackLog y sin sesión obsoleta);
   - SM-39 (sin NullReferenceException en `DocumentActivated` y sin fuga de transacción).

## 5. Lo que no se hizo

- Sin AutoCAD, BASELINE ni NETLOAD.
- Sin escrituras en el registro, el perfil, TRUSTEDPATHS, SECURELOAD, AssemblyMap ni ACL.
- Sin commits después del Candidato.
- Sin merge ni rebase de main, y sin F2.
- Secuencia pendiente tras el Smoke y su revisión: sincronizar con main / I-65 (dos conflictos del censo de comandos), revalidación
  completa y solo entonces F2.
