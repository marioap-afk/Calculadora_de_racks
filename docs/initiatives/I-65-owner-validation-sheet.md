# I-65: ficha de Owner Validation final (guía §7)

> **Estado: PREPARADA PARA APROBACIÓN.**
>
> - **OWNER VALIDATION = PENDING SIGNATURE. INTEGRATION = NOT AUTHORIZED.**
> - El Coordinator preparó esta ficha por la disposición «W-R2 Acceptance / Final Owner Validation Preparation»
>   ([texto literal](../automation/decisions/I-65-owner-disposition-wr2-acceptance-ov-preparation.original.txt); decisiones §124).
> - No atribuye la firma del Owner ni declara completa la Owner Validation. Los campos de firma quedan vacíos hasta una decisión
>   explícita posterior del Owner.

## 1. Identidad del `FINAL_CANDIDATE_SHA` (guía §7 y §7.1)

```text
Fecha y zona:              (la de la decisión del Owner)
Validador:                 (lo declara el Owner al firmar)
Iniciativa / rama:         I-65 / architecture/interop-analisis-estructural (Workflow V2, Claim-Id 0b4aad79-068e-4a57-a2c2-dc8f23406778)
Commit:                    4a03b65b375d468c220dd68400d5dfd955bf043e (FINAL_CANDIDATE_SHA, declarado el 2026-10-29, decisiones §117)
Worktree:                  architecture-interop-analisis-estructural; salida de compilacion D:\r65-smoke\4a03b65b\bin-Debug-net8.0-windows\
Ruta del DLL Debug:        D:\r65-smoke\4a03b65b\bin-Debug-net8.0-windows\RackCad.Plugin.dll (W-R1a, NETLOAD)
SHA-256 del DLL:           0f5a508e03a34cce45cd005a0fef336d1b160894e0836a5f786d2ec966db0959 (ProductVersion 1.0.0+4a03b65b375d468c220dd68400d5dfd955bf043e)
DLL del bundle (W-R1b):    60e1dd44741d14b96ea0a379e901e806abb4d65c5783411786f611f35e604c33; bundle de 25 archivos, inventario 33e660b2e3781b72ba24f37f1ac97526c409301e71bb91869aa7c968c64bf7d5
Tabla:                     section-material-map.json, blob d088e2a9, SHA-256 fc3d9146... (LF, igual a la fuente)
Herramienta:               ramtool, 11 archivos, manifiesto 4fbe3ea5be1790c20c2673339b9ca2fba26bdac8f59472686952474fb33eb343
Version de AutoCAD:        R25.0.171.0.0 (acad.exe 2a75996f...6422), perfil <<Unnamed Profile>>
RAM Elements:              26.00.01.15 (7dcb4855...7970); ramelements 0.1.0 (arbol f71a017f..., 30 archivos)
DWG / escenario:           REF-CANT-01 (Cantilever, una cara) en dibujos nuevos; ov08\hb-ref.dwg (ac2c8014...)
Bloques reales:            D:\Base_de_datos_AutoCAD_V.0.dwg  SHA-256 b4ca2248...60e8 (override de settings.json; no se abrio ni se modifico)

Candidate SHA:             4a03b65b375d468c220dd68400d5dfd955bf043e
Arbol limpio:              SI
SDK resuelto:              8.0.423
Core Full local:           PASS (14392/14392)
UI Full local:             PASS (1637 superadas, 17 omitidas declaradas)
Debug UI build:            PASS
Debug Plugin build:        PASS
CI exact SHA:              GREEN (run 40814761082, push, 4/4; job ui-tests success)
Owner validation:          pending (firma)
```

**Checklist de la guía §7:**

| Punto | Dónde se ejecutó | Resultado |
|---|---|---|
| NETLOAD y menú | W-R1a (NETLOAD «Load Once», módulo único `0f5a508e`); W-R1b (Autoloader sin NETLOAD, módulo único `60e1dd44`) | cumplido |
| Dibujo / colocación | W-R1a A y B (REF-CANT-01) | cumplido |
| Actualización en sitio | OV-08 O8-02/03 | cumplido |
| Round-trip tras reabrir DWG | OV-08 O8-07/08 y O8-10 | cumplido |
| Vistas ligadas / multivista | OV-08 O8-02..04 | cumplido |
| BOM y consolidado | OV-08 O8-05 | cumplido |
| Persistencia / legacy | Persistencia: O8-10. Legacy: **NOT_EVALUATED** (WR1-13: no se aportó un DWG Cantilever anterior representativo) | persistencia cumplida; legacy NOT_EVALUATED |
| Escenarios de la iniciativa | OV-I65-01..07, HB-01..05 | cumplido (§2) |

## 2. Matriz completa de pruebas

| ID | Escenario | Ventanas | Resultado | Evidencia |
|---|---|---|---|---|
| OV-I65-01 | REF-CANT-01: exportación, script sobre modelo vacío y volcados | W-R1a A + W-R2 | **PASS** | `Valid`, H0 `73635936…d13ea`, 16/16 y 117/117. `READBACK OK` con H0, `DUMP OK` (`f5789d1c`, 0 combinaciones), contraste IGUAL y comparador PASS (0) con el volcado manual `c4a98fb0` (R-D2) |
| OV-I65-02 | Cargas de REF1 | W-R1a A + W-R2 | **PASS** | `Valid` `93849b42…`, 16/16 y 6/6. `READBACK OK`, `DUMP OK` (`e5cf0ead`, 0 combinaciones), IGUAL y comparador PASS (0) con `LOADS` y el volcado manual `12c89479`. Q2 reproducida en el miembro 1 al 99.99999999999997 % (CD-I65-F5-08) |
| OV-I65-03 | Estación no soportada, o mapeo ausente o sin verificar | W-R1a A | **PASS** | Casos (a)-(e) con su código, «Unsupported…» y ninguna carpeta |
| OV-I65-04 | Script sobre un modelo no vacío | W-R1a A + W-R2 | **PASS** | `Valid` = H0; `gate: the model has 1 nodes`; modelo intacto |
| OV-I65-05 | Sección inexistente | W-R1a D + W-R2 | **PASS** | `FAILED — DISCARD MODEL` con `ValueError: Section not found: "HSS_SQR INEXISTENTE-99"`; parcial de 9 elementos documentado; control REJECTED (2) |
| OV-I65-06 | Lectura de vuelta que no puede coincidir | W-R1a D + W-R2 | **PASS** | `FAILED` por el eje local 2 del primer miembro |
| OV-I65-07 | Capacidad alterada | W-R1a D + W-R2 | **PASS** | 07a: puerta de versión; 07b: puerta de función del núcleo. Sin escritura y con el modelo vacío |
| OV-I65-08 | Lista de la guía manual | W-R1a B | **PASS** | O8-01..13; H1 = H2 = `f69400f6…6d15eb`; legado **NOT_EVALUATED** |
| HB-01..05 | Bundle del Autoloader dentro de AutoCAD (complemento) | W-R1b | **PASS** | Carga real sin NETLOAD; tabla disponible; HB-04 (a) con huella = H1 = H2; HB-04 (b) 117/117; bundle íntegro (160 comprobaciones); archivos desinstalados; registro recuperado (§121) |

**Estados que no son PASS y se mantienen así** (Owner, §124; no se convierten en PASS por inferencia):

| Elemento | Estado | Base |
|---|---|---|
| Legado de OV-08 | NOT_EVALUATED | WR1-13 |
| Propiedades de sección y de material | NOT_EVALUATED por la API (filas del comparador) | Respaldo documental: D-09 de F4-D, aprobada el 2026-10-16 (§62), limitada al modelo analítico de REF-CANT-01 |
| DIF-01 (peso unitario de RAM frente a AISC, −0.62 %) | INFORMATIVE | CD-I65-F4-47 |
| Peso propio | excluido (multiplicadores 0, 0, 0) | Freeze; ADR-0050 |
| Categoría del caso manual (DL) | INFORMATIVE | Comparador |

## 3. Resultados de las ventanas

| Ventana | Fecha (UTC) | Registro | Aceptación del Owner |
|---|---|---|---|
| W-R1a (A: OV-01..04; B: OV-08; D: inyecciones y scripts de volcado) | 2026-10-29 | `b65f7d5d`, decisiones §118, evidencia §120 | **ACCEPTED / PASS** (§119); cinco desviaciones aceptadas como no bloqueantes |
| W-R1b (C: HB-01..05) | 2026-10-30 | `c4216d86` (§120, evidencia §122); recuperación del registro `b5a6f723` (§121, evidencia §123) | **ACCEPTED / PASS (global)** (§122) |
| W-R2 (RAM: OV-01, 02 y 04..07) | 2026-10-30, 22:59:40Z-23:16:28Z | `fad6f937`, decisiones §123, evidencia §125 | **ACCEPTED / PASS** (§124); incidencias aceptadas como no bloqueantes |

## 4. Limitaciones técnicas y de ingeniería

1. **Equivalencia de transferencia, no validación estructural.** RackCad ↔ RAM Elements no valida resistencia, estabilidad ni diseño de un rack real (ADR-0050 D5; Q-O3; D-09). No se analizó en RAM.
2. **Alcance V1.** Una estación Cantilever de una cara. Quedan fuera:
   - doble cara, `NegativeY`, inclinación de más de 10°, C/L y canales pareados (OV-03 los rechaza con su código);
   - peso propio, combinaciones, momentos y cargas distribuidas.
3. **Propiedades de sección y material.** No las expone la API de RAM. Se respaldan en la hoja D-09, limitada a las dos filas de REF-CANT-01.
4. **DIF-01 informativa.** No es válida para análisis que dependan del peso propio.
5. **Edición formal del catálogo de RAM: UNKNOWN** (DIF-04; coherencia fuerte con AISC v16.0, sin confirmación formal).
6. **A65-F4R-07:** limitación documentada del comando (decisiones §105).
7. **Trazabilidad de sesión (R-D3, opción 0):** operativa, no criptográfica.
8. **Ventanas de RAM:** dependen de la sesión del Owner en Bentley CONNECTION (licencia Practitioner).
9. **RACKAYUDA:** diferida (R-D8 = A). F6 queda fuera del alcance (Q-O5).

## 5. Integridad y trazabilidad

- **Identidad fija:** Freeze `4e6e1feb`, ADR-0050 `c18def42`, tabla `d088e2a9` y A-n vacío. Sin cambios desde el Candidato. Desde `4a03b65b` hasta la punta, nada fuera de `docs/` (READY-09; preintegración §1).
- **Custodia:** cada ventana versiona sus originales del Owner, decisiones, evidencia y filas de custodia (SHA-256 de los blobs del índice), y las vuelve a comprobar contra su commit:

  | Ventana | Evidencia | Filas | Diferencias |
  |---|---|---|---|
  | W-R1a | §120 | — | 0 |
  | W-R1b | §122 | 62 | 0 |
  | Recuperación del registro | §123 | 23 | 0 |
  | W-R2 | §125 | 78 | 0 |

- **CI (push, 4/4):**

  | Commit | Contenido | Run |
  |---|---|---|
  | `4a03b65b` | Candidato | 40814761082 |
  | `66b65a8d` | Declaración | 40885886740 |
  | `b65f7d5d` | W-R1a | 40924816826 |
  | `8692cb39` | Aceptación de W-R1a y autorización de W-R1b | 40955136440 |
  | `c4216d86` | W-R1b | 40990483363 |
  | `b5a6f723` | Recuperación del registro | 41135069646 |
  | `3f28120a` | Aceptación de W-R1b y autorización de W-R2 | 41141002967 |
  | `fad6f937` | W-R2 | 41144265116 |

- **Host:** BASELINE y POST de cada ventana con herramientas de SHA-256 fijo (procedimientos §1.3); K1 y K2 de los checkouts sin STOP; preflight y POST de RAM con las identidades iguales.
- **Privacidad (§90):** capturas recortadas y redactadas, y barridos `secrets_scan.py` con 0 coincidencias. Los originales con datos identificables quedan en local con su SHA-256.

## 6. Hallazgos residuales

| Hallazgo | Estado | Tratamiento propuesto |
|---|---|---|
| Desinstalar el bundle borrando la carpeta no limpia las entradas que AutoCAD escribe en HKCU (`Applications`, `Loaded`, `AssemblyMap`) | Aceptado como hallazgo (§122) | Documentación de despliegue o iniciativa futura; se anota en `docs/ideas-futuras.md` en el cierre |
| `AssemblyMap\RackCad.Plugin` hacia la ruta temporal del bundle (host) | Permanece; limpieza **NO autorizada** (§122) | Ninguno sin autorización |
| Referencias históricas en `HKCU\Software\appdatalow\…\CoreUser` (julio de 2026; host) | Permanecen; limpieza **NO autorizada** (§122) | Ninguno sin autorización |
| `hkcu-profiles` (MRU y `VIEWBACKSTATUS`) y `Config.ini` de RAM (3 claves de interfaz) | INFORMAR, se conservan | Ninguno |
| Incidencias de W-R1a (cinco desviaciones; las seis pulsaciones «W12X26» siguen como incertidumbre operativa), W-R1b y W-R2 | Aceptadas como no bloqueantes, con trazabilidad (§119, §122, §124) | Ninguno |
| A65-FC-18 (EDITORIAL) | Pendiente | Va al cierre documental (CD-I65-FC-28) |
| Método de los árboles de las bases (A65-F5F-03) | Resuelto en W-R2 midiendo con los dos métodos (CD-I65-FC-69 (a)) | Precisión en el cierre documental, si el Owner lo desea |

**El equipo no se declara idéntico a la BASELINE.**

## 7. Riesgos de integración (preintegración del 2026-11-03)

1. **Orden.** `origin/main` no ha avanzado desde `bb0d5522`, y la integración es un merge sin conflictos cuyo árbol es el de la rama. Si **otra rama se integra antes** (I-62 está en `owner-decision`), I-65 necesita rebase. Eso da un SHA nuevo, un Candidato nuevo, READY desde 02 y W-R1/W-R2 otra vez, porque la evidencia no se reutiliza. **Mitigación:** integrar I-65 antes que cualquier otra rama, con READY-04 repetido justo antes.
2. **Censos de comandos con I-64.** Conflicto en `CustomPropertiesCommandGuardTests.cs` y `SelectiveEditorOpenTests.cs` (37 → 38 por `RACKRAMPAQUETE` frente a `RACKPANEL`). Con R-D1, I-64 reconcilia a 39 cuando integre. No afecta al merge de I-65.
3. **`docs/ROADMAP.md`.** Archivo caliente compartido con I-62, I-64 e I-66; el merge simulado sale limpio. El commit de cierre lo vuelve a tocar («integrada»), así que hay que repetir la simulación en la integración.
4. **SHAs nuevos.** El commit de cierre documental y el `MERGE_SHA` son SHAs nuevos. Exigen CI verde propio y, en `main`, el artifact de cobertura (WORKFLOW §4.5, pasos 4-7). El commit de cierre solo puede tocar `docs/`.
5. **Publicación de FOUNDATIONS.** Las entradas pasan a `docs/FOUNDATIONS.md` con `Status: STABLE` en el cierre (READY-06 las revisó). Es el único cambio normativo del cierre, además de HANDOFF y ROADMAP.
6. **Checkout principal local.** Está en `95690c28`, detrás de `origin/main`. La integración por plomería (`merge-tree` + `commit-tree` + push) no lo toca.

## 8. Decisiones que requieren la firma del Owner

1. **Owner Validation final:** resultado global (aprobado | rechazado | parcial) sobre `4a03b65b`, con la confirmación explícita del validador (guía §6 y §7).
2. **Aceptación de los estados no PASS** de §2 y de las limitaciones de §4, tal cual.
3. **Aceptación de los hallazgos residuales** de §6 y de su tratamiento.
4. **Autorización de integración** (texto exacto en §10), con el orden y las compuertas de WORKFLOW §4.5 y §11.4-§11.6.
5. **Limpieza de la rama y del worktree** después de las dos compuertas posteriores al merge (incluida en el texto de §10; puede excluirse).

## 9. READY-01..09 (LIFECYCLE §8), comprobado de nuevo el 2026-11-03

| ID | Resultado | Base |
|---|---|---|
| READY-01 | CUMPLE | Freeze completo sin A-n; diferimientos decididos (§105, R-D8, Q-O5) |
| READY-02 | CUMPLE | Gates F1, F3, F4, F5, F7 y R-D11 cerrados; decisiones y documentos de producto versionados; entradas de FOUNDATIONS redactadas para el cierre |
| READY-03 | CUMPLE | 0 REQUIRED; 0 discrepancias A. Las decisiones del Owner pendientes son actos posteriores (firma e integración) |
| READY-04 | CUMPLE (fetch del 2026-11-03) | `origin/main` = `bb0d5522` = merge-base; 0 detrás; rama remota = punta; nada fuera de `docs/` desde el Candidato; I-64 sin integrar; ADR 0050 libre; SDK 8.0.423. Se repite justo antes de integrar |
| READY-05 | CUMPLE | Core 14392/14392; UI 1637 + 17 omitidas; builds Debug; herramienta; bundle; CI 40814761082 4/4 |
| READY-06 | CUMPLE | Architect CONFORMING (ronda 2, 0 REQUIRED) y Coordinator CONFORMING sobre `4a03b65b` (§116) |
| READY-07 | CUMPLE | Árbol limpio; ningún cambio de producto pendiente; Candidato = `4a03b65b`; la punta solo añade `docs/` |
| READY-08 | CUMPLE | Matriz OV-I65-01..08 asignada y ejecutada sobre el Candidato; HB-01..05 como complemento |
| READY-09 | CUMPLE | Acuerdo `11062a91`; Freeze `4e6e1feb` sin commits desde `a9d7080b`; A-n vacío; ADR-0050 `c18def42` sin cambios (`ready09-4a03b65b.txt`, comprobado de nuevo) |

## 10. Texto exacto de autorización para validar e integrar

Para firmar e integrar, el Owner pega este texto tal cual; si quiere otra cosa, lo modifica de forma expresa. Hasta entonces, OWNER
VALIDATION = PENDING SIGNATURE e INTEGRATION = NOT AUTHORIZED.

```text
I-65 — OWNER DECISION
Final Owner Validation / Integration Authorization

He revisado la ficha de Owner Validation de I-65 (docs/initiatives/I-65-owner-validation-sheet.md), registrada en la evidencia §126.

1. Owner Validation final
OWNER VALIDATION FINAL = APPROVED sobre FINAL_CANDIDATE_SHA 4a03b65b375d468c220dd68400d5dfd955bf043e.
Valido como Owner del proyecto la matriz OV-I65-01..08 y HB-01..05 con resultado global APROBADO.
Mantengo sin convertirlos en PASS: legacy de OV-08 = NOT_EVALUATED; propiedades de sección y material = NOT_EVALUATED mediante API, con respaldo D-09; DIF-01 = INFORMATIVE; peso propio excluido; categoría de caso manual = INFORMATIVE.
Acepto las limitaciones de la ficha §4 y los hallazgos residuales de la ficha §6 con su tratamiento.

2. Integración
INTEGRATION = AUTHORIZED para I-65 (architecture/interop-analisis-estructural) en main, conforme a WORKFLOW §4.5 y §11.4-§11.6:
* Fetch fresco y READY-04 repetido justo antes. Si origin/main ya no es bb0d5522, STOP y presenta el estado.
* Commit de cierre solo documental en la rama: HANDOFF §8-12, ROADMAP «integrada», entradas de FOUNDATIONS publicadas con Status STABLE, A65-FC-18, estado de la iniciativa y hallazgos en ideas-futuras; CI verde sobre su SHA exacto.
* Merge --no-ff construido con git merge-tree y git commit-tree, sin checkout de main, y push de main sin force.
* CI del MERGE_SHA en success con el artifact de cobertura, y cobertura del Candidato por workflow_dispatch.
* Tag anotado integration/I-65.
* Limpieza de la rama y del worktree solo después de esas compuertas.
Sin cambios de código, Freeze, ADR-0050, tabla ni tolerancias. Sin AutoCAD ni RAM. Sin Codex ni attempts.
I-64 reconcilia los censos de comandos al integrar después (R-D1).
```
