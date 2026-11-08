# I-64 — Solicitud al Coordinator: OPERATOR_EXCEPTION para la POST-I65 HOST SANITY CHECK

**Estado:** COORDINATOR_HOST_SANITY_AUTHORIZATION_REQUIRED. AutoCAD no se ha abierto; el preflight no se ha ejecutado; no hay commits
nuevos (HEAD = origin = `53683249`). P-01 se informa aparte (`p01-cleared-report.md`).

## Por qué se pide

- La revisión de integración exige la comprobación (alcance 1-10) pero no dice quién opera AutoCAD.
- La regla vigente desde a578 es que el Owner opera, y que si la sesión opera sin excepción, es STOP. La excepción de 6862 valía solo para 6862.
- Pregunté al Owner y eligió, literal: **«Tú, con excepción»**. Es decir, la sesión opera solo si el Coordinator da la excepción para
  esta comprobación.

## Lo que se pide autorizar

1. **OPERATOR_EXCEPTION** para esta comprobación y este RunId solamente: la sesión opera AutoCAD y ejecuta además el preflight, la
   BASELINE, HOST-EXTRAS, la inspección de procesos y módulos, la POST, el DIFF de T3C y las capturas. El Owner no toca el equipo.
2. **RunId `R20261108T065253Z-aa47`**, generado por la sesión. El kit está hecho; el RunId no se ha consumido.
3. **HOST_RELEASE** para I-64 / aa47.
4. **I-52:** está localizable (sesión «I - 52», inactiva). Propuesta: aviso previo y recheck de solo lectura antes del preflight,
   y otro después de la POST, como en 6862, sin exigir su ACK/RELEASE. Si el Coordinator prefiere otra cosa, que lo diga.
5. **SECURELOAD:** la sesión lo lee con `reg query`, en solo lectura, justo antes de la BASELINE, y la BASELINE de T3C lo vuelve a
   leer por su cuenta. Si no coinciden: STOP antes de AutoCAD. Así se hizo en 6862.
6. **Confirmar que el camino no destructivo vale para el punto 7** (ver abajo).

## Identidad que se carga (sin recompilar)

| Elemento | Valor |
|---|---|
| Producto sincronizado | `3f5c7014351d1dc59de4eee86a7d1319fbeccc07` sobre `main` `a140eb3a` |
| HEAD de la rama | `53683249c229329e3f576fc8f9b9919b1755c63f` (solo documentos encima; sin delta en `src/`, `tests/` ni archivos de proyecto) |
| `RackCad.Plugin.dll` (Debug, bin del worktree) | `0C0F2FDE0987C489BABAF2ACB9A46936117D044EAF45C706A303E6B5B1C56C0B`, ProductVersion `1.0.0+3f5c7014…` |
| Otros módulos | Application `12A7C29A…7558`, Domain `24EA6643…9507`, UI `E49AC0A6…9826` |
| `acad.exe` esperado | `2A75996F…6422` |

## Camino no destructivo de `RACKRAMPAQUETE`

- **Contexto:** el rack selectivo de **Dibujo B** (el de SM-34). Dibujo B sigue con su SHA-256 de 6862 (`fb9b97f9…682b`).
- **Código:** en `src/RackCad.Plugin/RackRamElementsCommands.cs`, líneas 64-79, un rack con diseño e identificador que no es
  Cantilever da `Unsupported`. El comando vuelve **antes** de pedir la estación, el archivo de cargas y la carpeta. No lee la
  tabla, no lee cargas y no llega al publicador.
- **Evidencia de I-65:**
  - W-R1a O8-11 (`docs/automation/evidence/I-65-ready/wr1a-R20261029T1633Z/wr1a-report.md`): «Unsupported. Solo se exporta un rack
    Cantilever; el elegido es de tipo selective.», sin carpeta;
  - SM-09 de su smoke (`docs/initiatives/I-65-f4-autocad-smoke-procedure.md`).
- **Esperado:** `RackCad paquete estructural: Unsupported. Solo se exporta un rack Cantilever; el elegido es de tipo selective.` y
  ninguna pregunta más.

## Pasos (procedimiento completo en `operator/procedure.md`)

| Id | Acción | Alcance |
|---|---|---|
| HS-00 | Preflight de la sesión: procesos, árbol y producto, DLL, herramientas T3C y autoprueba, dibujos, SECURELOAD, HOST-EXTRAS BEFORE, BASELINE y RackLog BEFORE | 1, 2, 10 |
| HS-01 | Abrir AutoCAD **una vez**, con acad = 0; registrar la PID y la vigilancia continua | 1 |
| HS-02 | `CPROFILE` y `SECURELOAD` con `getvar` | 10 |
| HS-03 | `NETLOAD` del DLL del bin del worktree → **solo «Load Once»** | 3 |
| HS-04 | Inspección: un solo `RackCad.Plugin.dll` en la PID, en el bin, `0C0F2FDE…`; los 4 módulos con su SHA | 2, 3 |
| HS-05 | Abrir `Dibujo B.dwg` | — |
| HS-06 | `RACKPANEL`: la paleta abre una vez, dentro de la pantalla, en su estado normal | 4, 5 |
| HS-07 | Clic en el rack selectivo → «Un rack» y su RackId; `Esc` | 5 |
| HS-08 | `RACKRAMPAQUETE` sobre el mismo rack → Unsupported, sin más preguntas | 6, 7 |
| HS-09 | RackLog: ningún ERROR atribuible | 8 |
| HS-10 / HS-11 | PRE-CLOSE; cerrar AutoCAD **sin guardar** | 9 |
| HS-12 | acad = 0; A, B, C, D y la biblioteca sin cambios; ningún `.dwl` | 9, 10 |
| HS-13 | POST, DIFF de T3C, HOST-EXTRAS y lectura de las claves de carga; recheck de I-52 | 10 |

## Controles de operación (los de 6862)

- **Instancia única:**
  - una sola llamada para abrir AutoCAD, y solo con acad = 0;
  - nunca esa llamada con AutoCAD abierto (en a578 lanzó una segunda instancia);
  - comprobación de la PID tras HS-04, HS-06 y HS-08, y antes de cerrar.
- **Entrada de texto:**
  - nada de Ctrl+V en la línea de órdenes;
  - las órdenes se teclean en trozos de 14 caracteres o menos, sin salto de línea, y Enter aparte;
  - el diálogo de NETLOAD solo se navega con clics (la herramienta no puede teclear en él).
- **Capturas:**
  - antes de abrir AutoCAD, el precheck de capturas del kit (etapa A de 6862);
  - las capturas se toman con `Save-ScreenRegion.ps1` y cada una se abre al momento.
- **Equipo:**
  - el Owner minimiza la ventana de ChatGPT y no usa el equipo durante la ventana;
  - si alguien lo usa: STOP.
- **Sin exploración:** ningún paso fuera de HS-00..HS-13 y ninguna acción agrupada sin documentarla; toda desviación vuelve al Coordinator.

**STOP inmediato:**
- una segunda `acad.exe` o una PID ambigua;
- un aviso de seguridad sin «Load Once»;
- un SHA o una ruta de módulo distintos;
- `RACKRAMPAQUETE` pide la estación, las cargas o la carpeta;
- cualquier cambio en los dibujos.

Tras un STOP: cerrar sin guardar, POST y DIFF, y volver al Coordinator.

## Estado del host al preparar (2026-11-08, ~06:50Z, solo lectura)

- acad = 0 y RAM Elements = 0.
- Dibujos A/B/C, D y la biblioteca con los mismos SHA-256 que en 6862; ningún `.dwl` ni `.bak` en la carpeta de los dibujos.
- Rama: HEAD = origin = `53683249`; `main` = `a140eb3a`; árbol limpio.
- **Lo que aún no se ha medido:** la BASELINE de T3C y HOST-EXTRAS. Se toman en HS-00, justo antes de abrir AutoCAD, para que sean frescas.

## Vuelta

COORDINATOR_POST_I65_HOST_SANITY_REVIEW_REQUIRED, con lo que pide la revisión:
- el SHA exacto del DLL;
- la identidad del proceso;
- la evidencia de Load Once;
- las observaciones de RACKPANEL y de RACKRAMPAQUETE;
- RackLog;
- el estado final del host;
- la propuesta PASS / NOT PASS / STOP. La sesión no declara PASS por su cuenta.

Sin F2, sin modelos, sin cambios de producto y sin commits antes de la vuelta.
