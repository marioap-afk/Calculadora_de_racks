# I-65 — Candidato final: frontera, conciliación de READY-01..09, identidad, matriz y plan de cierre

Documento de la unidad (Coordinator, 2026-10-29), por la disposición del Owner «R-D11 Acceptance / READY Final Preparation and
Validation Planning» ([texto literal](../automation/decisions/I-65-owner-disposition-rd11-acceptance-ready-final-preparation.original.txt);
decisiones [§111-§112](../automation/decisions/I-65.md)).

- **Sustituye para READY** a [I-65-ready-final-candidate-preparation.md](I-65-ready-final-candidate-preparation.md) (§110), que se conserva
  como preparación histórica.
- **Procedimientos de las ventanas:** [I-65-ready-wr1-wr2-procedures.md](I-65-ready-wr1-wr2-procedures.md).

> **Estado: PREPARADO. READY no se declara.**
>
> - FINAL CANDIDATE DECLARATION = PENDING OWNER AUTHORIZATION.
> - W-R1 / W-R2 EXECUTION = NO. INTEGRATION = NO.
> - No se han abierto AutoCAD ni RAM y no se ha escrito en `ApplicationPlugins`.

## 1. Frontera del Candidato

- **SHA propuesto:** el commit que registra este documento y las decisiones §111-§112. Su SHA exacto consta en la evidencia posterior
  (§114 de la evidencia y siguientes); un commit no puede contener su propio SHA.
- **Contiene:**
  - todas las disposiciones del Owner hasta §111, con A65-RD11-01 = A;
  - las cuatro correcciones documentales (§2.2);
  - el registro de READY-01..04;
  - la matriz OV-I65-01..08 con HB-01..05;
  - los procedimientos completos de W-R1 y W-R2.
- **Producto:** el árbol de `6a58c2b1` (GREEN de R-D11) en `src/`, `tests/`, `tools/`, `assets/`, `deploy/`, `eng/`, `.github/` y los
  archivos de build. Entre `6a58c2b1` y el SHA propuesto solo cambian `docs/`.
- **Es propio:** no reutiliza `5a5ca577` (hito F4) ni `bd1fcf8a` (hito F5), ni la excepción documental de F4 (CD-I65-F4-178/184) ni
  A65-F5C-01.
- **Después del SHA propuesto** solo se versiona **evidencia de ese SHA exacto**, que no cambia su contenido:
  - la evidencia local producida después del commit y con el árbol limpio;
  - la CI;
  - las evaluaciones de READY-05..09;
  - las conformidades;
  - la declaración y las autorizaciones de las ventanas;
  - los informes de ejecución.

  Es el orden de [WORKFLOW](../WORKFLOW.md) §11.4-§11.5: Candidato, evidencia sobre ese SHA, Owner Validation y cierre documental.
- **Cuándo hace falta un SHA nuevo y repetir toda la evidencia:** si antes de declarar llega una disposición que deba constar en el
  Candidato, es decir, una que cambie producto, Freeze, tabla, tolerancias, oráculos, matriz o procedimientos, o una decisión que
  READY-03 exija versionada.
- **Lectura que se pide confirmar:** como A65-F5C-01 no es precedente automático, la solicitud de declaración (§10) pide al Owner que
  confirme esta lectura de frontera de forma expresa.

## 2. Conciliación de READY-01..09 (LIFECYCLE §8, en orden)

### 2.1 Estado

| ID | Estado | Base |
|---|---|---|
| READY-01 | **CUMPLE** | Freeze `I-65-proposal-v3.1.md` completo y sin A-n (`amendment_refs: []`). Diferimientos decididos por el Owner: A65-F4R-07, DIF-01 y la edición del catálogo de RAM (UNKNOWN) como limitaciones (§105); RACKAYUDA diferida sin A-n (R-D8 = A); F6 fuera del alcance (Q-O5). La tabla fuera del Autoloader quedó resuelta por R-D11 B (GATE PASS `f2ed2a7e`, aceptado en §111). RD11-OBS-01 (fin de línea) queda resuelta por A65-RD11-01 = A |
| READY-02 | **CUMPLE en el SHA propuesto** | Gates cerrados: F1 (`4514a99a`), F3 (`1d8c713e`), F4 (`1e7d8fc2`), F5 (`45aaad6e`), F7 (`e8e6893c`) y R-D11 (`f2ed2a7e`). Decisiones R-D1..R-D11 y §111 versionadas; sin A-n. Documentos de producto listos: README, guía de I-65 y guía de despliegue, con las cuatro correcciones de §2.2. Las entradas de FOUNDATIONS están redactadas en [I-65-foundations-entry.md](I-65-foundations-entry.md) y se publican al integrar (LIFECYCLE §4.1) |
| READY-03 | **CUMPLE en el SHA propuesto** | 0 REQUIRED abiertos; 0 discrepancias A; ninguna decisión material o del Owner pendiente sobre el contenido. A65-RD11-01 decidida (A, §111); C-5 decidida (A); R-D1..R-D11 decididas; HB-01..05 aprobada para READY (§111). Lo que queda del Owner son **actos posteriores** a READY (declaración, ventanas, Owner Validation e integración), no decisiones sobre el Candidato |
| READY-04 | **CUMPLE** (preflight 2026-10-29T03:00:47Z; evidencia §114) | `git fetch origin` fresco: `origin/main` = `bb0d5522` = merge-base; 0 commits detrás; rama remota = HEAD local. No hace falta rebase. No hay cierre previo, así que la ruta R no aplica. R-D1: I-64 (`09f2f1c3`) sin integrar. **Se repite justo antes de declarar**: si `main` avanzó, rebase, SHA nuevo y READY desde 02 |
| READY-05 | **Se evalúa sobre el SHA propuesto** (evidencia posterior) | Core Full y UI Full locales; builds Debug de UI y Plugin; build de la herramienta; publicación del bundle; CI `push` exacta 4/4 (§4) |
| READY-06 | **Se evalúa sobre el SHA propuesto** (evidencia posterior) | Conformidad completa de Architect y Coordinator (§7) |
| READY-07 | **Se evalúa sobre el SHA propuesto** | Árbol limpio; ningún cambio ni commit pendiente de producto (R-D8 A, R-D11 hecho); HEAD identificado |
| READY-08 | **CUMPLE en el SHA propuesto** | Matriz de §6 completa: OV-I65-01..08 asignadas a I-65 por el Freeze §5 y preparadas para el SHA final; ninguna retirada; HB-01..05 se añade como complemento aprobado por el Owner |
| READY-09 | **Se evalúa sobre el SHA propuesto** | Acuerdo exacto: blob `11062a91` (commit `3872fcc0`); commit de Freeze `a9d7080b` con solo la línea `Frozen` (blob `4e6e1feb`); Freeze único e inmutable desde entonces; diff permitido; compatibilidad de fuente V1; secuencia de A-n vacía. Comprobación preliminar en la preparación: sin hallazgos |

`FINAL_CANDIDATE_SHA` se fija solo cuando READY-05..09 consten sobre el SHA propuesto **y** el Owner lo declare.

### 2.2 Correcciones documentales (READY-02; autorizadas en §111) y su efecto

| Hallazgo | Archivo y lugar | Corrección | Efecto comprobado |
|---|---|---|---|
| A65-RD11-06 | `docs/guias/despliegue.md` §2, árbol de la salida de compilación | línea nueva `structural-analysis\ram-elements\section-material-map.json  ← tabla de RACKRAMPAQUETE` | el árbol coincide con la salida real: la tabla es `Content` del Plugin (`RackCad.Plugin.csproj`, línea 27) y está en la salida Debug de `f2ed2a7e` |
| A65-RD11-06 | `docs/guias/despliegue.md` §5, «Verificar el bundle» | la allowlist incluye «la tabla de secciones y materiales en su ruta exacta, igual en SHA-256 a su fuente» | describe lo que hace `verify-bundle.ps1` desde `6a58c2b1`: una rama de la allowlist con `-ceq` y `Assert-SectionMapMatchesSource` |
| A65-RD11-08 | `docs/guias/interoperabilidad-ram-elements.md` §10, fila «`Unavailable` por falta de la tabla» | «reinstalar el bundle con `deploy\install-bundle.ps1 -Build` (o con un bundle generado por `deploy\build-bundle.ps1`, que lo verifica)» | la reinstalación recomendada pasa por el verificador (`-Build` ejecuta `build-bundle.ps1`, que verifica antes de instalar) |
| A65-RD11-09 | `docs/guias/interoperabilidad-ram-elements.md` §2, viñeta de la tabla | «igual a la fuente del checkout de compilación (el contenido del blob aprobado salvo el fin de línea)» | coincide con A65-RD11-01 = A y con el comportamiento del verificador; no cambia datos de la tabla |

**Lo que las correcciones no tocan:**

- el Freeze (blob `4e6e1feb`), ADR-0050 (`c18def42`), la tabla (`d088e2a9`), las tolerancias, los oráculos ni el contrato congelado;
- código, pruebas, `deploy/`, `assets/` ni `eng/`.

La línea de despliegue §4 «idéntica a assets\» se mantiene: con A65-RD11-01 = A, `assets\` es la fuente del checkout de compilación.

## 3. SHA de producto y del Candidato

- **Último cambio de producto:** `6a58c2b1` (target del bundle, verificador, harness, prueba de distribución y documentación). El C# de
  `src/` no cambia desde `51fc82f1`.
- **SHA propuesto:** solo documentos sobre `6a58c2b1`.
- **Binarios de las ventanas:** cada SHA produce DLL distintos (`InformationalVersion` = `1.0.0+<sha>`). Por eso toda OV y HB usan los
  binarios compilados desde el SHA declarado, nunca desde `6a58c2b1` ni desde `f2ed2a7e` (AGENTS.md, «Reutilización de evidencia»).

## 4. Suites y CI requeridas sobre el SHA propuesto (AGENTS.md, fila `FINAL_CANDIDATE_SHA`)

| Clase | Contenido | Momento |
|---|---|---|
| Core Full local | `dotnet test tests/RackCad.Tests/RackCad.Tests.csproj` con el SDK 8.0.423 del usuario | después del commit, con el árbol limpio antes y después |
| UI Full local | `dotnet test tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj` (17 omitidas declaradas) | ídem |
| Build Debug de UI | 0 errores | ídem |
| Build Debug del Plugin | salida nueva `D:\r65-smoke\<s8>\bin-Debug-net8.0-windows\` (ProductVersion `1.0.0+<sha>`, manifiesto, tabla) | ídem |
| Build de la herramienta | `D:\r65-smoke\<s8>\ramtool\` | ídem |
| Bundle del Autoloader | `deploy\build-bundle.ps1 -Configuration Release` en `D:\r65-smoke\<s8>\bundle\` + `deploy\test-verify-bundle.ps1` (25 archivos, 158 comprobaciones, 14 casos) | ídem |
| Identidad y entradas de las ventanas | manifiestos, cargas copiadas de los blobs, `expected-inputs*.json` y comprobaciones en seco (procedimientos §1.1) | ídem |
| CI | `push`, `ref` = `refs/heads/architecture/interop-analisis-estructural`, `head_sha` exacto, los 4 jobs requeridos en `success` (incluido `ui-tests`; el job del Plugin ejecuta el verificador y el harness) | después del push |
| Cobertura | `workflow_dispatch` con `candidate_sha` | después del merge (WORKFLOW §4.5, paso 7) |
| Owner Validation | W-R1 (OV-01..04, OV-08 y HB-01..05) y W-R2 (OV-01, 02 y 04..07) | después de la declaración y con autorización de cada ventana |

## 5. Identidad de herramientas y DLL

**Producto, entradas y aplicaciones.**

- Las tablas de [procedimientos](I-65-ready-wr1-wr2-procedures.md) §1.1-§1.2 fijan qué se mide y dónde.
- Los valores concretos los fija la evidencia del Candidato. Incluyen:
  - SHA-256 y ProductVersion de los cuatro DLL;
  - los manifiestos;
  - la tabla junto al DLL con `git ls-files --eol`;
  - la herramienta;
  - el inventario del bundle;
  - las cargas y los valores esperados.

**Herramientas del Coordinator.**

- Están en [procedimientos](I-65-ready-wr1-wr2-procedures.md) §1.3, con su SHA-256 actual y su copia versionada.
- La única nueva es `wr_hostsnap.ps1` (R-D6), versionada en este Candidato en
  [`I-65-ready/tools/wr-hostsnap.ps1.txt`](../automation/evidence/I-65-ready/tools/wr-hostsnap.ps1.txt).

**Volcados manuales de R-D2:**

- OV-01: `c4a98fb0…`;
- OV-02: `12c89479…`.

Los dos se midieron en disco el 2026-10-29 (evidencia §114) y coinciden con lo custodiado.

## 6. Matriz OV-I65-01..08 y HB-01..05

| OV | Escenario (Freeze §5) | Ventana | Evidencia nueva sobre el SHA final | Reutilización | Oráculo |
|---|---|---|---|---|---|
| OV-I65-01 | REF-CANT-01: exportar, script sobre modelo vacío y volcados | W-R1 A + W-R2 | `pkg-OV01`, construcción, `READBACK OK`, volcado del generado, contraste y comparador | volcado manual `c4a98fb0` (R-D2) si se cumplen sus condiciones; si no, construcción manual nueva (R-D4, ventana aparte) | `Valid` 16/16 y 117/117; comparador PASS (0); propiedades NOT_EVALUATED |
| OV-I65-02 | Cargas de REF1 | W-R1 A + W-R2 | `pkg-OV02`, construcción, `READBACK OK`, volcado, contraste y comparador | volcado manual `12c89479` (R-D2), con las mismas condiciones | `Valid` 16/16 y 6/6; comparador PASS con `LOADS`; Q2 reproducida |
| OV-I65-03 | Estación no soportada, o mapeo ausente o sin verificar | W-R1 A | casos (a)-(e): código visible y ninguna carpeta | — | W1 §4.2; «fila sin verificar» = pruebas de fixture (CD-I65-F4-186) |
| OV-I65-04 | El script se niega con el modelo no vacío | W-R1 A + W-R2 | `pkg-OV04` y su ejecución sobre un modelo con un nudo | — | rechazo sin modificar el modelo |
| OV-I65-05 | Sección inexistente | W-R1 D + W-R2 | `inj-OV05` y control del comparador | — | `FAILED — DISCARD MODEL`; control REJECTED (2) |
| OV-I65-06 | Lectura de vuelta que no puede coincidir | W-R1 D + W-R2 | `inj-OV06` | — | `FAILED` |
| OV-I65-07 | Capacidad alterada | W-R1 D + W-R2 | `inj-OV07a` y `inj-OV07b` | — | rechazo antes de escribir; modelo vacío |
| OV-I65-08 | Lista de la guía manual | W-R1 B | O8-01..13 con guardado, cierre y reapertura del DWG desechable | — | [procedimiento de OV-08](I-65-ov08-procedure.md); H2 = H1 |
| HB-01..05 | Bundle del Autoloader dentro de AutoCAD (complemento aprobado en §111) | W-R1 C | instalación temporal, carga sin NETLOAD, `pkg-HB` y `pkg-HB2`, desinstalación y restauración | — | procedimientos §3.3, parte C (oráculos de R-D11 §6 con la lectura de §112) |

**Condiciones de R-D2** (procedimientos, WR2-PR-04):

- identidades de RAM, de `ramelements`, de las bases (método único de R-D5 (a)) y de la plantilla de volcado;
- especificaciones de REF-CANT-01, hoja de OV-02 y cargas;
- SHA-256 de los volcados.

**Exclusión:** nunca se reutilizan lecturas de vuelta, volcados del generado ni paquetes anteriores.

## 7. Condiciones de READY-06 (LIFECYCLE §9)

- **Quién:** Architect (subagente de solo lectura, como en F4, F5, F7 y R-D11) y Coordinator. Revisan **todos los arquetipos** sobre el
  SHA propuesto: NEW ARCHITECTURE y materialidad M-07.
- **Contra qué:**
  - Freeze V3.1;
  - A-n (ninguna);
  - decisiones aprobadas hasta §111;
  - entradas de FOUNDATIONS.
- **Qué comprueban:**
  - el diff completo desde el merge-base `bb0d5522`;
  - las invariantes con prueba;
  - la matriz OV;
  - las limitaciones declaradas;
  - la coherencia de la documentación de producto;
  - los procedimientos de las ventanas frente a los oráculos aprobados, incluida la lectura de HB-04 (§112).
- **Resultado:**
  - `CONFORMING` o `NON-CONFORMING`, con desviaciones clasificadas (`EDITORIAL`, `NON-MATERIAL`, `BEHAVIORAL-WITHIN-FREEZE`,
    `MATERIAL`, `OWNER-RESERVED`);
  - un REQUIRED o NON-CONFORMING impide READY-06;
  - su corrección crea un SHA nuevo y reinicia READY-02, con una relectura del Architect;
  - la conformidad no rediseña: las mejoras quedan como seguimiento.
- **Controles de la revisión:** K1/K2 antes y después, y barrido K5 de la transcripción, sin AutoCAD, RAM ni escrituras.

## 8. Plan de cierre e integración (solo plan; INTEGRATION = NO)

| # | Paso | Autoridad |
|---|---|---|
| 1 | Evidencia sobre el SHA propuesto (§4), READY-05..09 y conformidades | LIFECYCLE §8-§9 |
| 2 | **Declaración de `FINAL_CANDIDATE_SHA` por el Owner** (§10), con un fetch fresco inmediatamente antes (READY-04) | guía manual §7.1 |
| 3 | W-R1 y W-R2 con autorización de cada ventana; Owner Validation firmada por el Owner | procedimientos; guía §7 |
| 4 | **Cierre documental** (commit nuevo, solo `docs/`, sin agruparse con el Candidato): HANDOFF §8-§12; ROADMAP `integrada (fecha)`; entradas de FOUNDATIONS publicadas; contrato de la iniciativa; evidencia final. `git diff --name-only <Candidato>..HEAD` sin nada fuera de `docs/`, `README.md` y `CLAUDE.md`. Core Full y UI Full locales, más CI exacta del cierre | WORKFLOW §4.5.4, §11.4; AGENTS.md («Cierre documental de iniciativa») |
| 5 | Fetch inmediatamente antes del merge. Si `main` avanzó: **ruta R** (conservar la ronda, rebasar, Candidato nuevo publicado solo, READY, conformidad, Full, CI y Owner Validation que correspondan, y cierre nuevo) | WORKFLOW §11.5 |
| 6 | Merge `--no-ff` con primer padre = `origin/main`, sin `checkout` de `main` en el checkout principal (merge por plomería, como en integraciones anteriores); push de `main` | WORKFLOW §4.5.5 |
| 7 | CI posterior al merge sobre `MERGE_SHA` (4 jobs y artefacto de cobertura) y cobertura del Candidato por `workflow_dispatch` | WORKFLOW §4.5.6-§4.5.7 |
| 8 | Limpieza (rama local y remota, worktree) **solo** después de las dos compuertas | WORKFLOW §4.5 (punto 6) |
| 9 | Tag anotado `integration/I-65` con el bloque completo | WORKFLOW §11.6 |

**Coordinación con I-64 (R-D1):**

- I-65 integra primero.
- I-64 concilia al rebasar: censo 38→39 y archivos comunes (`CustomPropertiesCommandGuardTests.cs`, `SelectiveEditorOpenTests.cs` y
  `docs/ROADMAP.md`).
- Antes de integrar se verifica el estado real de `main` y de las dos iniciativas.

## 9. Presupuesto restante y riesgos

**Presupuesto:**

| Recurso | Estado | Nota |
|---|---|---|
| Codex | 55 / 57 | No se usa en READY. Las 2 restantes no alcanzan para otra cadena (5): cualquier código exige presupuesto y autorización nuevos |
| `attempts` | 7 / 11 | Sin cambios |
| Workers adicionales | 0 | Ninguno autorizado |
| Architect | 1 revisión de READY-06 | Más una relectura si hubiera REQUIRED |
| Host W-R1 | 2 h 55 min - 3 h 45 min (reserva 4 h) | A 75-90 min; B (OV-08) 45-60 min; C (HB) 40-55 min; D 15-20 min. Se puede partir en W-R1a (A, B, D) y W-R1b (C) |
| Host W-R2 | 1.5-2 h (reserva 2.5 h) | Sin construcción manual gracias a R-D2. Si R-D2 no aplica: ventana manual aparte de unas 2 h |

**Riesgos:**

- **`main` avanza antes de la declaración o del merge** (por ejemplo, I-64): rebase, SHA nuevo y repetición de READY, Full, CI,
  conformidad y ventanas. Si exige código, hace falta presupuesto nuevo.
- **HB toca el perfil del usuario:**
  - la desinstalación es un borrado de carpeta;
  - el Autoloader podría dejar un residuo en el registro (UNKNOWN hasta medirlo);
  - una diferencia protegida es STOP y no se corrige a mano.
- **Aviso de seguridad en el arranque con el bundle:** se espera el estándar «Load Once», porque la ruta del usuario no es de
  confianza. Si no aparece o aparece otro, se registra o se hace STOP.
- **Licencia de RAM:** depende del inicio de sesión del Owner en Bentley CONNECTION; R-D5 (c) cierra RAM ante un STOP.
- **Cambio de RAM, de `ramelements` o de las bases:** R-D2 deja de aplicar y hace falta la construcción manual (R-D4).
- **Versión de AutoCAD o biblioteca de bloques distinta** entre la declaración y las ventanas: la evidencia del dueño no es comparable.
- **Fin de línea (A65-RD11-01 = A):** un checkout CRLF da otro SHA-256 de la tabla. Es aceptado; se registra `git ls-files --eol`.
- **Limitaciones de V1 que se mantienen:**
  - equivalencia de transferencia, no validación estructural;
  - propiedades NOT_EVALUATED;
  - peso propio excluido;
  - DIF-01;
  - edición del catálogo de RAM UNKNOWN;
  - A65-F4R-07;
  - RACKAYUDA diferida.

## 10. Decisiones pendientes y solicitud exacta para declarar el Candidato final

**Decisiones del Owner** (actos posteriores a READY; ninguna cambia el contenido del Candidato):

1. Declarar `FINAL_CANDIDATE_SHA`.
2. Autorizar W-R1, liberando expresamente HB-01..05 y OV-08, en una ventana o en dos (W-R1a y W-R1b).
3. Autorizar W-R2, después de aceptar W-R1.
4. Owner Validation.
5. Integrar, como orden aparte.

**Lecturas que el Owner puede revisar antes de declarar:**

- la frontera de §1;
- la lectura del oráculo de huella de HB-04 (§112).

**Solicitud** (se completa con el SHA y la evidencia cuando READY-05..09 consten):

> Solicito declarar `FINAL_CANDIDATE_SHA = <40 hex>` para I-65 (rama `architecture/interop-analisis-estructural`).
>
> - **READY-01..09:** cumplen según la evidencia §… (READY-04 con fetch fresco de <fecha UTC>: `origin/main` = `<sha>` = merge-base).
> - **Evidencia local sobre ese SHA, con el árbol limpio:**
>   - Core <n>/<n>; UI <n> superadas y 17 omitidas;
>   - builds Debug sin errores;
>   - DLL del Plugin `<sha256>` con ProductVersion `1.0.0+<sha>`;
>   - bundle con 25 archivos, verificado y con su harness.
> - **CI:** `push` <id> con `head_sha` exacto, 4/4.
> - **Conformidad:** Architect `<veredicto>` y Coordinator `CONFORMING`, sin REQUIRED.
> - **Hallazgos residuales:** <lista>.
> - **Frontera:** confirmo que los commits posteriores a ese SHA solo registran evidencia, conformidades, la declaración, autorizaciones e
>   informes de ese SHA exacto, sin cambiar producto, Freeze, tabla, tolerancias, oráculos, matriz ni procedimientos.
> - **Lo que no cubre la declaración:** las ventanas (W-R1, que incluye OV-08 y HB-01..05, y W-R2) y la integración siguen pendientes de
>   autorizaciones separadas.
