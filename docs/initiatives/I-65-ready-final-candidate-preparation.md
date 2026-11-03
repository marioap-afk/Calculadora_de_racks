# I-65 — Preparación de READY tras el gate R-D11 (conciliación, Candidato final, OV y ventanas)

- **Estado:** PREPARACIÓN (orden del Owner, [decisiones](../automation/decisions/I-65.md) §105, punto 7). **READY EXECUTION = NO.** No
  declara READY, no fija `FINAL_CANDIDATE_SHA`, no abre AutoCAD ni RAM, no instala en `ApplicationPlugins` y no integra.
- **Sustituido** (decisiones §112) por [I-65-final-candidate.md](I-65-final-candidate.md) y
  [I-65-ready-wr1-wr2-procedures.md](I-65-ready-wr1-wr2-procedures.md). Se conserva como preparación histórica. Su §5.3 decía «huella de
  entrada igual a la de WR1-02» para HB, lo que no es posible con otro rack: la lectura vigente está en §112.
- **Sustituye para READY** a [I-65-ready-preparation.md](I-65-ready-preparation.md) (§104), que se conserva como preparación histórica.
  Sus §2 y §3 quedan resueltos por §105.
- **Autoridades:**
  - [INITIATIVE_LIFECYCLE](../INITIATIVE_LIFECYCLE.md) §6-§9;
  - [WORKFLOW](../WORKFLOW.md) §4.5 y §11.5;
  - [AGENTS.md](../../AGENTS.md) («Reutilización de evidencia»);
  - [guía manual](../guias/validacion-manual-autocad.md) §5-§7;
  - Freeze [Proposal V3.1](I-65-proposal-v3.1.md) §5;
  - contrato del gate [I-65-rd11-autoloader-gate.md](I-65-rd11-autoloader-gate.md).
- **Base:**
  - gates F1, F3, F4, F5, F7 y **R-D11** cerrados (R-D11 sobre `f2ed2a7e`, decisiones §109);
  - último cambio de producto: `6a58c2b1`;
  - `origin/main` = `bb0d5522` (merge-base; 0 commits detrás);
  - I-64 (`architecture/workspace-persistente-rackcad`, `09f2f1c3`) sin integrar.

## 1. Conciliación de READY-01..09

Las condiciones se cumplen **en orden** (LIFECYCLE §8). El texto exacto de cada criterio está en
[I-65-ready-preparation.md](I-65-ready-preparation.md) §1.

| ID | Estado tras §105-§109 | Lo que falta |
|---|---|---|
| READY-01 | **CUMPLE (preliminar).** Detalle debajo de la tabla | Confirmarlo sobre el SHA final |
| READY-02 | **PENDIENTE.** Detalle debajo de la tabla | Las correcciones de §3.2, cuando se dé la orden de READY |
| READY-03 | **PENDIENTE.** 0 REQUIRED y 0 discrepancias A. C-5 está decidido (R-D7 = A). Queda abierta **A65-RD11-01** | Decisión del Owner sobre A65-RD11-01, (a) o (b) (§7) |
| READY-04 | **NO INICIADO.** Hoy `origin/main` = merge-base: no hace falta rebase. R-D1 se cumple: I-64 no ha integrado | Fetch fresco, preflight y, si `main` avanza, rebase con evidencia nueva |
| READY-05 | **NO INICIADO** | Focales y relevantes, más CI `push` con `head_sha` exacto y 4/4 jobs |
| READY-06 | **NO INICIADO** | Conformidad completa de Architect y Coordinator sobre el SHA resultante, incluidas las entradas de FOUNDATIONS |
| READY-07 | **NO INICIADO.** No queda código de producto pendiente (R-D8 A; R-D11 hecho) | Árbol limpio y HEAD identificado |
| READY-08 | **PREPARADO.** Detalle debajo de la tabla | La decisión del Owner sobre HB-01..05 (§7) |
| READY-09 | **NO INICIADO.** Comprobación preliminar sin hallazgos: Freeze único (`4e6e1feb`), sin A-n | Repetirla sobre el SHA final |

Detalle:

- **READY-01:**
  - El Freeze `4e6e1feb` no tiene A-n.
  - Los diferimientos están decididos: A65-F4R-07, DIF-01 y la edición del catálogo de RAM (UNKNOWN) se mantienen como limitaciones
    (§105); RACKAYUDA, diferida (R-D8 A, sin A-n); F6, fuera del alcance (Q-O5).
  - La tabla fuera del Autoloader queda **resuelta por R-D11 B** (§105-§109).
  - Nueva limitación: RD11-OBS-01, el fin de línea de la tabla.
- **READY-02:**
  - Gates cerrados: F1, F3, F4, F5, F7 y R-D11.
  - Decisiones R-D1..R-D11 versionadas (§105).
  - Documentos de producto: faltan las correcciones de §3.2 (A65-RD11-06, 08 y 09).
  - Las entradas de FOUNDATIONS siguen redactadas; se publican al integrar (LIFECYCLE §4.1).
- **READY-08:**
  - OV-I65-01..08 están asignadas y preparadas (§4).
  - R-D2 está autorizada (§105): se reutilizan los volcados manuales `c4a98fb0` y `12c89479` si las identidades coinciden.
  - HB-01..05 se propone como complemento dentro de W-R1.

**Después de READY-09** se fija `FINAL_CANDIDATE_SHA`. Cualquier corrección que cambie el SHA reinicia READY-02; tras READY-04, una A-n o
una decisión nueva crea un SHA nuevo (LIFECYCLE §6).

## 2. Nuevo SHA de producto

- **Último cambio de producto:** `6a58c2b1` (GREEN de R-D11). Cambia:
  - el target del bundle (solo publicación);
  - `deploy/verify-bundle.ps1`;
  - `deploy/test-verify-bundle.ps1`;
  - la prueba de distribución;
  - el README y dos guías.
- **El código C# de `src/` no cambia desde `51fc82f1`:** los DLL solo difieren en la versión embebida (`1.0.0+<sha>`).
- **SHA de cierre del gate:** `f2ed2a7e`, solo documentos.
- **Para las OV:** toda OV se ejecuta con DLL, bundle y herramienta compilados desde `FINAL_CANDIDATE_SHA`, nunca desde `6a58c2b1` ni desde
  `f2ed2a7e`, porque cada SHA da DLL distintos.

## 3. Candidato final propio

### 3.1 Contenido de `FINAL_CANDIDATE_SHA`

Un commit que **contiene**:

- las decisiones §105 en adelante;
- el registro de READY-01..09;
- las correcciones de §3.2;
- el producto de `6a58c2b1`, o su imagen tras un rebase de READY-04.

No se reutilizan la excepción documental de F4 (CD-I65-F4-178/184) ni la disposición A65-F5C-01 de F5. Después de ese SHA solo se
versiona evidencia de ese SHA exacto.

### 3.2 Correcciones documentales preparadas (texto exacto; se aplican solo con la orden de READY)

| Hallazgo | Archivo | Texto actual | Texto preparado |
|---|---|---|---|
| A65-RD11-06 | `docs/guias/despliegue.md` §5, «Verificar el bundle» | «prueba que solo se distribuyen los cuatro DLL de RackCad y catálogos permitidos —cero DLL Autodesk (ADR-0003)—» | «prueba que solo se distribuyen los cuatro DLL de RackCad, los catálogos permitidos y la tabla de secciones y materiales en su ruta exacta, igual en SHA-256 a su fuente —cero DLL Autodesk (ADR-0003)—» |
| A65-RD11-06 | `docs/guias/despliegue.md` §2, árbol de la salida de compilación | (sin la tabla) | Línea nueva después de `catalogs\…`: `structural-analysis\ram-elements\section-material-map.json  ← tabla de RACKRAMPAQUETE` |
| A65-RD11-08 | `docs/guias/interoperabilidad-ram-elements.md` §10, fila «`Unavailable` por falta de la tabla» | «reinstalar el bundle con `deploy\install-bundle.ps1`» | «reinstalar el bundle con `deploy\install-bundle.ps1 -Build` (o con un bundle generado por `deploy\build-bundle.ps1`, que lo verifica)» |
| A65-RD11-09 | Guía de I-65, §2, viñeta de la tabla | «idéntica a la fuente aprobada: el verificador del bundle lo exige» | Con A65-RD11-01 (a): «igual a la fuente del checkout de compilación (el contenido del blob aprobado salvo el fin de línea): el verificador del bundle lo exige». Con (b): sin cambio, más la regla `-text` en su unidad |

Si el Owner prefiere aplicarlas antes de la orden de READY, es un commit documental del Coordinator que no toca código.

### 3.3 Secuencia propuesta

1. **Owner:** A65-RD11-01, HB-01..05 y orden de iniciar READY (§7).
2. **Commit documental:** las correcciones de §3.2 y el registro de READY-01..03.
3. **READY-04:** fetch fresco y preflight. Si `main` avanzó (por ejemplo, I-64 integrada), rebase con evidencia nueva y conciliación de los
   censos 38→39. Si hace falta código, se pide autorización: el presupuesto actual no admite otra cadena (§6).
4. **READY-05:** focales, relevantes y CI exacta.
5. **READY-06:** Architect y Coordinator, conformidad completa.
6. **READY-07, 08 y 09.**
7. **Declaración de `FINAL_CANDIDATE_SHA`** por el Owner y, sobre ese SHA, la evidencia de §3.4.
8. **W-R1 y W-R2** (§5), con autorización de host.
9. **Owner Validation** y, aparte, la orden de integrar.

### 3.4 Pruebas que se repiten sobre `FINAL_CANDIDATE_SHA`

| Clase | Contenido |
|---|---|
| Local, después del commit y con el árbol limpio | Core Full; UI Full (17 omitidas declaradas); builds Debug de UI y Plugin (DLL en `D:\r65-smoke\<sha>\bin-Debug-net8.0-windows\`, ProductVersion `1.0.0+<sha>`); build de la herramienta; **publicación del bundle** con `deploy\build-bundle.ps1` en una carpeta nueva más `deploy\test-verify-bundle.ps1` (25 archivos, 14 casos) |
| CI | `push`, `ref` y `head_sha` exactos, 4/4 jobs, leído (el job del Plugin ejecuta el verificador y el harness) |
| Bloque §7.1 de la guía manual | SHA, árbol limpio, SDK, Core, UI, builds, CI, Owner Validation, biblioteca de bloques (ruta resuelta y SHA-256) y versión de AutoCAD |
| OV-I65-01..08 | §4, con paquetes nuevos generados por el DLL final |
| HB-01..05 | Solo si el Owner los autoriza (§4 y §5.3) |

## 4. Matriz OV-I65-01..08

| OV | Escenario (Freeze §5) | Ventana | Evidencia nueva sobre el SHA final | Reutilización | Oráculo y procedimiento base |
|---|---|---|---|---|---|
| OV-I65-01 | REF-CANT-01: exportar, script sobre modelo vacío y volcados | W-R1 + W-R2 | `pkg-OV01`, construcción, `READBACK OK`, volcado del generado y comparador | Volcado manual `c4a98fb0` (R-D2) si las identidades coinciden; si no, construcción manual nueva (R-D4) | Matriz §4 en PASS salvo filas informativas; W1 §4.1 y W2 §5 |
| OV-I65-02 | Cargas de REF1 | W-R1 + W-R2 | `pkg-OV02` con las cargas `d675d9f6`, construcción, `READBACK OK`, volcado y comparador | Volcado manual `12c89479` (R-D2) con las mismas condiciones | Valor, posición y dirección; Q2 = 99.99999999999997 %; W3 §3.2 y W4 §4.2-§4.3 |
| OV-I65-03 | Estación no soportada o mapeo ausente o sin verificar | W-R1 | Casos (a)-(e): código visible y ninguna carpeta | — | W1 §4.2; las «filas sin verificar» son pruebas de fixture (CD-I65-F4-186) |
| OV-I65-04 | El script se niega con el modelo no vacío | W-R1 + W-R2 | `pkg-OV04` y su ejecución sobre un modelo no vacío | — | Rechazo sin modificar el modelo; W2 §5 |
| OV-I65-05 | Sección inexistente | W-R2 | Inyección generada por la herramienta del SHA final | — | `FAILED — DISCARD MODEL` y rechazo del comparador; W2 §5 |
| OV-I65-06 | Lectura de vuelta que no puede coincidir | W-R2 | Inyección | — | `FAILED`; W2 §5 |
| OV-I65-07 | Capacidad alterada | W-R2 | Inyecciones (a) y (b) | — | Rechazo antes de escribir; nudos = miembros = 0; W2 §5 |
| OV-I65-08 | Lista de la guía manual | W-R1 | O8-01..13 (guardado y reapertura del DWG desechable; H2 = H1) | — | [I-65-ov08-procedure.md](I-65-ov08-procedure.md) |
| HB-01..05 (propuesta) | Bundle del Autoloader dentro de AutoCAD | W-R1b | Instalación, carga sin NETLOAD, `RACKRAMPAQUETE` `Valid` y desinstalación | — | Contrato del gate §6 (oráculo de HB-02 corregido por A65-RD11-02) |

**Condiciones de R-D2 (sin cambio):**

- **Identidades:**
  - RAM `7dcb4855…`;
  - `ramelements` 0.1.0 (árbol `f71a017f…`);
  - bases de RAM medidas con el método único de R-D5 (a);
  - plantilla de volcado.
- **Especificaciones:**
  - REF-CANT-01;
  - hoja de OV-02 (`6dc0d8da…`);
  - cargas `d675d9f6…`.
- **Integridad:** el SHA-256 custodiado de cada volcado.
- **Exclusión:** nunca se reutilizan lecturas de vuelta, volcados del generado ni paquetes.

## 5. Procedimiento de las ventanas W-R1 y W-R2

### 5.1 Identidad (se fija al declarar el Candidato; una diferencia es parada)

- **Producto:**
  - `FINAL_CANDIDATE_SHA`;
  - DLL Debug en `D:\r65-smoke\<sha>\bin-Debug-net8.0-windows\` (ProductVersion y SHA-256);
  - tabla junto al DLL igual a la fuente del checkout de compilación, con `git ls-files --eol` registrado;
  - herramienta en `D:\r65-smoke\<sha>\ramtool\`;
  - cargas de REF-CANT-01 y `d675d9f6…` copiadas en `inputs\`;
  - para HB, el bundle publicado y verificado en `D:\r65-smoke\<sha>\bundle\`.
- **Aplicaciones:**
  - AutoCAD 2025 (versión y SHA-256 de `acad.exe`);
  - biblioteca de bloques (ruta resuelta y SHA-256);
  - RAM `7dcb4855…` y `ramelements` 0.1.0.
- **Herramienta de captura del host:** `wr_hostsnap.ps1` (derivada de `w1_hostsnap.ps1`; R-D6 opción 1: ACL por `Get-Acl .Sddl`, sin depender
  de la página de códigos; SHA-256 `F867318C37330EBE61B658F54AB690959B44553D8D8EE36424D90F85A4832A54`). Se versiona una copia redactada al
  ejecutar la primera ventana.

### 5.2 Controles comunes

- **Captura del host:** BASELINE y POST con `wr_hostsnap.ps1` en la misma ventana y por el mismo canal. K1/K2 antes y después.
- **Estado del equipo:** AutoCAD y RAM cerrados al empezar; nadie usando el equipo; I-62 sin operación de pantalla.
- **Carpetas:** una carpeta de corrida nueva por ventana, `D:\r65-smoke\<sha>\wr1-<RunId>\` y `…\wr2-<RunId>\`.
- **Capturas:** recortadas a la zona técnica y redactadas (§90).
- **Prohibido:** guardar modelos de RAM; guardar un DWG fuera de la carpeta de corrida.

### 5.3 W-R1 — AutoCAD (2.5-3 h; HB, 30-45 min más)

| Paso | Acción | Oráculo |
|---|---|---|
| WR1-01 | BASELINE; NETLOAD «Load Once» del DLL del Candidato | Es el DLL custodiado |
| WR1-02 | REF-CANT-01 (W1 §4.1); `RACKRAMPAQUETE` → `pkg-OV01` con las cargas de referencia | `Valid`; validador de paquetes 16/16; carga útil 117/117 |
| WR1-03 | `pkg-OV02` con las cargas `d675d9f6` (W3 §3.2) | `Valid`; 16/16; seis cargas |
| WR1-04 | `pkg-OV04` | `Valid`; 16/16 |
| WR1-05 | OV-03 (a)-(e) (W1 §4.2) | Cada código y ninguna carpeta |
| WR1-06 | Cerrar AutoCAD sin guardar; AutoCAD nuevo; OV-08 (O8-01..13) | Procedimiento de OV-08 |
| WR1-07 | Sin AutoCAD: inyecciones OV-05..07 desde `pkg-OV01` con la herramienta del Candidato | Archivos de inyección generados (`inj-OV05`, `inj-OV06`, `inj-OV07a` e `inj-OV07b`) |
| WR1-08 (HB, si se autoriza) | HB-01..05: instalar el bundle, AutoCAD **sin** NETLOAD, `RACKRAMPAQUETE` sobre REF-CANT-01 y desinstalar | Contrato del gate §6; huella de entrada igual a la de WR1-02; `ApplicationPlugins` vacía al final |
| WR1-09 | POST y DIFF; K1/K2 | Sin PROHIBIDO |

### 5.4 W-R2 — RAM Elements (1.5-2 h, con R-D2)

Reglas de R-D5:

- (a) bases de RAM medidas con el método único (PowerShell) en PRE y POST;
- (b) contraste entre lectura de vuelta y volcado **después** de cerrar RAM;
- (c) un proceso de RAM nuevo; ante un STOP de licencia, cerrar RAM y terminar la ventana.

| Paso | Acción | Oráculo |
|---|---|---|
| WR2-01 | Construir `pkg-OV01` sobre un modelo vacío; volcar el generado | `READBACK OK`; `DUMP OK` |
| WR2-02 | Construir `pkg-OV02` sobre un modelo vacío; volcar el generado | `READBACK OK` (Q2); `DUMP OK` |
| WR2-03 | `pkg-OV04` sobre un modelo no vacío | Rechazo; el modelo no cambia |
| WR2-04 | `inj-OV05`, `inj-OV06`, `inj-OV07a` e `inj-OV07b` | Códigos esperados (W2 §5) |
| WR2-05 | Cerrar RAM; contraste entre lectura de vuelta y volcado; comparador CMB-3 contra los volcados manuales reutilizados (o los nuevos si R-D2 no aplica) | PASS sin filas no leídas; propiedades NOT_EVALUATED |
| WR2-06 | POST y DIFF | Sin PROHIBIDO; el Config.ini de RAM solo con estado de interfaz |

### 5.5 Paradas comunes

- Identidad distinta.
- Diálogo de confianza o de licencia de AutoCAD o RAM.
- Pérdida de datos o error silencioso.
- H2 ≠ H1, o huella de HB distinta de la de WR1-02.
- Escritura fuera de la carpeta de corrida, o en `ApplicationPlugins` fuera de HB.
- DIFF PROHIBIDO.
- Fin de la ventana autorizada.

Una ejecución fallida no se repite sin autorización.

## 6. Presupuesto remanente y riesgos de integración

| Recurso | Consumido / techo | Nota |
|---|---|---|
| Codex | 55 / 57 | La reserva de 2 no alcanza para otra cadena (5); cualquier código nuevo exige autorización |
| `attempts` | 7 / 11 | Sin REWORK en R-D11 |
| Workers | 1 / 1 | No queda ninguno autorizado |
| Architect | — | READY-06 (más una relectura si hubiera REQUIRED) |
| Host | — | W-R1 3-3.75 h con HB; W-R2 1.5-2 h |

**Riesgos:**

- **`main` avanza antes de READY-04** (por ejemplo, I-64): rebase, evidencia nueva y conciliación de censos 38→39 con código. Exigiría
  presupuesto nuevo.
- **Conflicto con I-64 al integrar:** `CustomPropertiesCommandGuardTests.cs`, `SelectiveEditorOpenTests.cs` y `docs/ROADMAP.md`. Con R-D1, si
  I-65 integra primero, la conciliación la hace I-64.
- **Fin de línea (RD11-OBS-01):** los bundles y las salidas de compilación de checkouts nuevos llevan la tabla con CRLF. El oráculo de HB-02
  y la identidad se leen según la decisión de A65-RD11-01.
- **HB instala en el perfil del usuario:** hoy `ApplicationPlugins` está vacía; HB-05 la deja vacía.
- **Licencia de RAM:** depende del inicio de sesión del Owner en Bentley CONNECTION.
- **Cambio de RAM, de `ramelements` o de las bases:** R-D2 deja de aplicar y hay que reconstruir a mano.
- **Integración:** las entradas de FOUNDATIONS se publican al integrar; la cobertura se mide después (WORKFLOW §2.bis).
- **Limitaciones de V1 que se mantienen:** equivalencia de transferencia, no validación estructural; propiedades NOT_EVALUATED; peso propio
  excluido; DIF-01; edición del catálogo de RAM UNKNOWN; A65-F4R-07; RD11-OBS-01.

## 7. Decisiones que requieren al Owner

1. **A65-RD11-01:**
   - (a) aceptar que «identidad exacta» es la igualdad con la fuente del checkout de compilación (el contenido del blob salvo el fin de
     línea);
   - (b) autorizar `-text` para la tabla en una unidad posterior.

   Sin decisión rige (a), pero READY-03 la necesita registrada.
2. **HB-01..05:** incluir o no la prueba real del bundle dentro de AutoCAD en W-R1, con autorización de host para instalar en
   `ApplicationPlugins` y desinstalar.
3. **Orden de iniciar READY-01..09**, con las correcciones documentales de §3.2.
4. **Ventanas W-R1 y W-R2**, incluido OV-I65-08 con el guardado y la reapertura del DWG desechable.
5. **Declaración de `FINAL_CANDIDATE_SHA`**, después de READY-09.
6. **Integración**, como orden aparte.

Seguimientos fuera de I-65 (no bloquean READY):

- A65-RD11-04: mutación del verificador en el harness.
- A65-RD11-07: comentarios de `build-bundle.ps1` y `eng/ci/verify-autocad-references.ps1`.
