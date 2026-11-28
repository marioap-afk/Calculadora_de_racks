# Paquete de cierre — F4B-PBT1B-LOSSLESS-STATE-TYPE (PB-T1b)

**Resultado:** **CLOSED / PASS WITH LIMITATIONS** sobre `a18ba345eddc5333c22b8bacbad5c2ed4dbf5959`, con attempts **2 de 3**. El producto es la implementación del intento 1 (`a0a5f699`), restaurada sin cambios: `git diff 2f5d3dec a18ba345 -- src` está vacío.

**Autoridades:**
- el contrato `F4B-PBT1B-LOSSLESS-STATE-TYPE/coordinator/gate-contract.json` (v2.7; AuthorityRevision `5e52aaa3`; ChainBaseSha `4b0498e3`);
- el paquete `docs/initiatives/I-64-f4b-pbt1-gate-contract-package.md` v2.7;
- las decisiones del Owner F4B-E09, F4B-E11 (D2), F4B-E14, F4B-E19, F4B-E20, F4B-E21, F4B-E22 y F4B-E25.

## 1. La cadena

| Paso | Corrida | Resultado |
|---|---|---|
| Planificación v2.6 | `R20261118T184831Z-aaac` | Rechazada por la comparación literal (PBT1B-C08, F4B-E18) |
| Replanificación única | `R20261118T185409Z-c45c` | Reparada por la sesión: la sesión pone el ForbiddenWriteScope (F4B-E18). nc4 `R20261118T190107Z-0764` |
| Worker | `R20261118T190205Z-6d33` | Detenido por la pausa del Owner (reinicio del equipo) tras los pasos 0 y 1; sin commits |
| Worker | `R20261118T191840Z-7115` | Preparado y no lanzado (Snapshot J pendiente) |
| Sonda neutra | `F4-ENVIRONMENT/probe-R20261118T193210Z-d32c` | PASS; Snapshot J aceptada (F4B-E19) |
| Worker, reanudación | `R20261118T193423Z-fb01` | Parada PBT1B-C15 en pb-15 antes del RED (F4B-E20, normalización conocida, sin gastar intento) |
| Planificaciones v2.7 | `R20261118T203710Z-4000` y `R20261118T204045Z-a60d` | Rechazadas: colecciones vacías por el límite de salida del Controller (F4B-E21) |
| Planificación v2.7 | `R20261118T204434Z-6063` | Aceptada, con las colecciones opacas puestas por la sesión (F4B-E21): fidelidad PASS 8/8 y A1-A8 8/8. nc4 `R20261118T204611Z-3412` |
| **Intento 1**, Worker | `R20261118T204715Z-a4e2` | RED `2d9febc6` y GREEN `a0a5f699`; CI 4/4; revisión independiente **REWORK 0/2/15** (defectos de pruebas; producto conforme) → F4B-E22 |
| **Intento 2**, planificación de la corrección | `R20261118T222843Z-e354` | Aceptada: fidelidad PASS, A1-A8 8/8 con CorrectionOf. nc4 `R20261118T223033Z-6aa3` |
| Worker de la corrección | `R20261118T223111Z-4c04` | IMPLEMENTATION_COMPLETE. Interrupción de transporte 1 de 2 (sin progreso desde 22:54:14Z; corte a las 00:09:36Z), reanudada en la misma invocación sin descartar ediciones; solo se repitió `p2a`. RED `126fd65f` (el esqueleto de `2d9febc6` devuelto, blobs iguales, y las pruebas corregidas); GREEN `a18ba345` (`git restore` desde `a0a5f699`) |
| CI del RED | `44430615074` | tests y ui-tests en failure, build-ui en success, build-plugin omitido (M14). Core: 14 fallos por aserción (13 de PushBackEditableState y 1 de OutputGate). UI: 1 (Parity). Las guardas de D2 pasan |
| CI del GREEN | `44438060317` | 4/4. Core 19 086. UI 2 437 (2 416 pasadas y 21 omitidas) |
| Revisión directa | — | Sin hallazgos: régimen RED/GREEN exacto y trailer real |
| Revisión independiente | — | **ACEPTABLE CON MENORES 0/0/5** (PBT1B2-IR-M01 a M05) |
| Verificación | `R20261119T014757Z-c78a` | **EXECUTION_BLOCKED / STOP**, Authority (16.3). Scope, por un control extra mal planteado por la sesión |
| Vuelta al Owner | `F4B-DRAFTS/pbt1b/owner-return-verify-stop.md` | **F4B-E25:** «Regla F4B y reverificar (Recomendado)» |
| Reverificación | `R20261119T020026Z-dc81` | **EXECUTION_VERIFIED** / NONE, 14/14 |
| nc1 | `R20261119T020504Z-a4e8` | **CONTROL PASS** (Identity, STOP) |
| nc2 | `R20261119T021056Z-448d` | **CONTROL PASS** (Scope, STOP) |
| nc3 | `R20261119T021418Z-2049` | **CONTROL PASS** (FreeText, REWORK). La coincidencia «limit reached» de su relevo es el texto de S-11, no un límite de uso |

**Relevos:**
- 0 participantes en toda la cadena;
- Snapshot J y binario acreditado desde F4B-E19;
- `main` = `812baf87`, sin tocar.

## 2. Lo que deja PB-T1b

En `src/RackCad.Application/Systems/PushBack/` y `Systems/Shared/`:
- `PushBackEditableState`, con:
  - las cargas con la semántica y el orden de hoy;
  - `BuildInputs`;
  - las reglas explícitas de c-02 (`ResolvePostCatalogId`) y de c-09 (`ResolveWeightUnit`);
- el copiador y el comparador;
- el mapa de propiedad (OB-05.1);
- `PushBackEditorOutputGate`, el criterio de bloqueo de salida extraído;
- los métodos nuevos de ES y RMES (D-PBT1-07), sin campos nuevos.

D2 se respeta: solo la línea base resuelta y el informe de módulos de cada lado se comparten por referencia. Lo fijan CT-PBT1B-14 (G-L1, con la regla 1b sobre los alias y sus autopruebas k1 y k2), CT-15 y CT-16.

La ventana no cambia: es de PB-T1c.

## 3. Desviaciones, límites y arrastres

| Id | Qué | Va a |
|---|---|---|
| M10 / M14 (intento 1) | Los ayudantes privados del comparador y del mapa fuera de la lista de INV-PBT1B-03; build-plugin omitido en la CI del RED | **Ratificados por el Owner solo para PB-T1b** (F4B-E22). El paquete v2.8 corrige el texto para PB-T1c |
| M14 (b) | `"Trailer": null` en la entrega del intento 1 | Resuelto en el intento 2: el Trailer es el real de los commits |
| M09, M11, M12, M15 (intento 1) | Menores de producto declarados | Arrastres a PB-T2 y PB-T4 (§10 del paquete v2.8) |
| M03, M05 (intento 1) | Límites de formulación de CT-16 y otros, declarados | Sin cambio; declarados en la entrega |
| PBT1B2-IR-M01 a M05 | La autoprueba de M07 no distingue la comprobación nueva de la antigua; al caso de I02 le falta la línea de control; p2 corrió con `--no-build` sin log de compilación; la regla «EXCEPCION» no se aplica en CT-11; la línea de M08 calcula 16 y 19 | Anotados en `docs/ideas-futuras.md` |
| F4B-E25 | La base de la corrección contenía ES y RMES (UNIT_CHANGE) cambiados por el intento 1 | Regla de F4B para las correcciones; reverificación 14/14 |
| Sesión | El control de cadena del prompt de la verificación incluía los commits de documentación de la sesión | Corregido en la reverificación |
| Sesión | ChainRedSha `2d9febc6`, declarado sin acreditar por una verificación (16.8) | Hecho declarado al Controller; Tests pasó; sin efecto |
| Transporte | Interrupción 1 de 2 de la fase WORK | Reanudada en la misma invocación; declarada en la entrega |
| F4B-E24 | RACKEDITAR pierde en silencio los campos desconocidos anidados | Limitación conocida anterior a F4B; anotada en `docs/ideas-futuras.md` |

**Siguiente:** la emisión de PB-T1c (`F4B-PBT1C-LOSSLESS-STATE-WINDOW`) con el paquete v2.8, cuya comprobación acotada ya está hecha, por la autorización de F4B-E09 y F4B-E11. Su generador ya está escrito y probado en seco por objetos git.
