# I-52 AUTH-15-C1 — Evidencia de la unidad

Unit: `I-52-AUTH15-C1`. Contrato congelado: [I-52-auth15-c1-unnamed-envelope.md](../../initiatives/I-52-auth15-c1-unnamed-envelope.md) (Freeze `6e667d02e14182d1fe232e10e53e0d447a6c98e7`).
Decisiones: [I-52-AUTH15-C1.md](../decisions/I-52-AUTH15-C1.md). Claim-Id: `b5f18dbd-a34f-4ba5-9146-5ddfcaaf2b27`.

Este archivo registra hechos observados. No declara la aceptacion del Candidato, del Architect ni del Owner, ni la excepcion de identidad host→Candidato (ver §4).

## 1. RED → GREEN

| Paso | SHA | Resultado |
|---|---|---|
| RED (solo pruebas, produccion de `3375aadb`) | `93b93f787ce443f70c1296bb87b016f57583b46f` | 4 de 5 guardas `AUTH15_C1_*` fallan: I-1 (el `Precheck` lee `envelope.Name`), I-2 (AUTH-15 lee `envelope.Name`), I-4 diagnostico (falta «sobre ausente o sin Id/Kind») e I-4 XML de `InvalidEnvelope` («lacks Id/Kind/Name»). I-3 (Id, Kind y sobre nulo siguen validados) pasa y se conserva. |
| GREEN / implementacion | `7e7e9178165e7f3691e8d7888417bd0fc34012e4` | Las TRES ediciones congeladas (contrato §4). `RackDefinitionCreatorGuardTests` 26/26. |

Diff de produccion de la implementacion contra la base (`3375aadb`): `src/RackCad.Plugin/Systems/Shared/RackDefinitionCreator.cs` (se quita `|| string.IsNullOrWhiteSpace(envelope.Name)` y el texto pasa a «sobre ausente o sin Id/Kind»)
y `src/RackCad.Plugin/Systems/Shared/RackDefinitionCreationResult.cs` (comentario XML de `InvalidEnvelope`). Nada mas en `src/`. Arboles de la implementacion: `src` `29265f70196fd19d6102e1932da15d535ffab8c8`, `tests` `3ea8617c48fa4734312ed6fb0659c47dd71c94ea`.

## 2. Validacion en host (Document Database + LockDocument + StartTransaction; `OpenCloseTransaction` sigue sin soportarse)

El arnes es temporal (`eng/research/I52Auth15C1Host/`, adaptacion del arnes `fcca6e6c` de `I-52-AUTH15`) y se retira antes del Candidato. Ambas corridas: `src/` y `tests/` del commit del arnes byte-identicos a la implementacion `7e7e9178`
(`treesEqual = true`). AutoCAD `25.0s`, SECURELOAD 1, dibujo en blanco `scratch.dwg` SHA-256 `7E7CDC22D2929138BE79A4558923B1D2092E6DF77DC0CBE0205D5FB062D115CC` (sin cambios tras cada corrida).

### Corrida 1 — arnes `579de1af`: HOST RUN VALID, veredicto UNKNOWN (evidencia historica, NO es PASS)

- Carpeta de evidencia: [run1-579de1af-UNKNOWN](I-52-AUTH15-C1/run1-579de1af-UNKNOWN/). 29/29 comprobaciones del lanzador; crudo 16/16 casos PASS, 7/7 controles PASS, 0 fugas.
- Causa del UNKNOWN: defecto del arnes, no de AUTH-15: `Evidence.Verdict()` juzgaba 15 casos fijos y la matriz tiene 16 (HV-00..HV-15); ademas HV-06/HV-07/HV-09 corrian sobre bases laterales.
- Por decision del Owner (Ruta 1), NO se deriva PASS de esta corrida y su evidencia no se edita.

### Corrida 2 — arnes corregido `b7ee683ecef8efc3e41c8b962fa172846daea614`: HOST RUN VALID, **veredicto PASS emitido por el propio arnes**

- Carpeta de evidencia: [run2-b7ee683e-PASS](I-52-AUTH15-C1/run2-b7ee683e-PASS/). Lanzador: `RUN_RESULT = PASS`, exit 0, 30/30 comprobaciones (incluye la nueva `evidenceExpectedCaseIds`), pid 16140, 2026-09-30T03:43:09Z→03:43:57Z, FILEDIA 1→0→1.
- Correccion del arnes: el veredicto se juzga contra `expectedCaseIds` (la lista de casos del runner; el lanzador exige ademas HV-00..HV-15 exacto) y HV-06, HV-07 y HV-09 pasan a la base del documento.
- Casos: 16/16 PASS. Sobre DOCUMENT-AUTHORITY: HV-01, 02, 03, 05, 06, 07, 08, 09, 10, 12, 13, 14, 15. Sobre base lateral (no son filas H de la matriz; identidad, `SaveAs`/reapertura e inmutabilidad): HV-00, HV-04, HV-11.
- Controles DOCUMENT-AUTHORITY: RB-01V, RB-01D, RB-02a, RB-02b, RB-02c, RB-03, RB-05 = 7/7 PASS. Fugas de casos y de controles: 0. Problemas: 0. Desviaciones: 0. `sideCharacterizations` = 0.
- Ensamblados cargados (SHA-256): Plugin `FC0699CF8C2FB2D02FF4C6939985EA567D72E27C79B4525CBC8C8E95BD98E6A6`, Application `2A5D2975A3C9CF0BE660508E70EC039506051B43CEA8DDE6A9797657E618463B`,
  Domain `06E0D17BE554E8EED9C8CA61C36DDAB9F0990EDD53660A659733184D94EDB576`, arnes `7294303AA7EFA416C10816FFD21C31E4717DEB58CA1FE44BD897DF0496029F1C`. Paquete: zip `F70D9C2CACD5FAE35F8592E533C22563B29433627F5E83273E186ABC12D1ECB1`, `SHA256SUMS` `CCA1F2A71C3C5D8ED82E0F7EF6D25B2B9C983A1AAC943C126DE7694827F8248E`.
- Identidad de transaccion: `nativeIdentityEqual = true`, `referenceEqualsTopTransaction = false` (el `TopTransaction` es un wrapper nuevo por lectura; la identidad es la nativa).
- No ejercitable sin inyeccion de fallos (declarado en `notExercisable`): `EnvelopeWriteFailed` y `InvalidEnvelope` por fallo de serializacion; no forman parte de la matriz congelada.

### Matriz H-1..H-10

| Fila | Caso | Autoridad | Resultado |
|---|---|---|---|
| H-1 `Name = null` aceptado | HV-15 (HeaderRun y Cantilever) | DOCUMENT | PASS |
| H-2 `Name = ""` aceptado | HV-15 | DOCUMENT | PASS |
| H-3 `Name = "   "` aceptado | HV-15 | DOCUMENT | PASS |
| H-4 el JSON releido == serializado; el `Name` releido es exactamente el suministrado (sin recorte ni respaldo) | HV-15 | DOCUMENT | PASS |
| H-5 `Id` en blanco → `InvalidEnvelope`, sin escritura | HV-09 | DOCUMENT | PASS |
| H-6 `Kind` en blanco → `InvalidEnvelope`, sin escritura | HV-09 | DOCUMENT | PASS |
| H-7 sobre nulo → `InvalidEnvelope`, sin escritura | HV-09 | DOCUMENT | PASS |
| H-8 `requestedBlockName` en blanco → `InvalidBlockName`, sin escritura | HV-08 | DOCUMENT | PASS |
| H-9 transaccion que no es la superior → `TransactionMismatch`, sin escritura (`OpenCloseTransaction` caracterizada: `TransactionMismatch`, no cuenta) | HV-06 | DOCUMENT | PASS |
| H-10 transaccion del llamador viva y sin Commit, sin `BlockReference` nuevo, el Abort del llamador descarta la definicion | HV-14, HV-15 | DOCUMENT | PASS |

`InvalidPlan` (HV-07) tambien PASS sobre DOCUMENT. Las seis variantes de HV-15 (HeaderRun y Cantilever × nulo, vacio, espacios) PASS.

## 3. Candidato

- **`FINAL_CANDIDATE_SHA` = `8f7847ad5114aec9262bfc44012f56bfd1e6f966`** (retiro del arnes; punta de producto publicada sola, antes del cierre documental). Base: `origin/main` = `3375aadbbf929427a6d106b2fff275d64863b89c`; arbol limpio; SDK resuelto: 8.0.423 de usuario.
- **Identidad de arboles.** `src` `29265f70196fd19d6102e1932da15d535ffab8c8` y `tests` `3ea8617c48fa4734312ed6fb0659c47dd71c94ea`, identicos a la implementacion `7e7e9178` probada en host y al commit de arnes `b7ee683e`. Las diferencias entre la
  implementacion probada y el Candidato se limitan a: retiro del arnes temporal, docs, evidencia y metadatos del repositorio (`.gitattributes`). Produccion cambiada respecto de la base: `RackDefinitionCreator.cs` y `RackDefinitionCreationResult.cs`.
- **Local sobre el Candidato exacto.** Core Full 11347/11347; UI Full 1581 superadas, 0 fallos, 17 omitidas historicas (ninguna nueva: el diff de `tests/` contra la base no contiene ningun skip); Build UI Debug, Build Plugin Debug y Build Plugin Release con 0 errores.
- **CI de push exacto:** run `36666387522`, `event=push`, `head_sha=8f7847ad…`, 4/4 jobs success (Tests Domain+Application, UI Tests, Build UI, Build Plugin without AutoCAD).
- **Cobertura del Candidato:** run `36666422179`, `event=workflow_dispatch`, `candidate_sha=8f7847ad…`, `measured-sha=8f7847ad…` (el `head_sha` del registro de la corrida es la punta de `main`, `3375aadb`, y no acredita el commit medido);
  artifact `11075514472`, digest `sha256:32aa126508d06935b3796b492248e33383148285d38d3986b25dec2896b8afeb`; cobertura de lineas 90.63 % y de ramas 78.97 %.

## 4. Excepcion de identidad host → Candidato: RATIFICADA

La validacion en host uso binarios construidos con la implementacion `7e7e9178` y el commit de arnes `b7ee683e`, no un binario del Candidato. **El Coordinator/Owner la ratificaron** al comprobar que los arboles `src` y `tests` del Candidato son
identicos a los probados y que la unica diferencia es el retiro del arnes, docs, evidencia y metadatos. Debe nombrarse en el tag de integracion.

## 5. Identidades de la unidad

| Concepto | Valor |
|---|---|
| Unit / Initiative / Workflow | `I-52-AUTH15-C1` / I-52 / V2 (T8-A) |
| Claim | `141cdaf8` (commit vacio), `Claim-Id b5f18dbd-a34f-4ba5-9146-5ddfcaaf2b27` |
| Base | `3375aadbbf929427a6d106b2fff275d64863b89c` |
| Contrato acordado (R2) / Freeze | `a6a3e96a0860e0bd9196b257f81480a574ae91a9` (blob `2f4fa9459d8796f7ef88862dccf5a2ce1b61fdfd`) / **`6e667d02e14182d1fe232e10e53e0d447a6c98e7`** |
| Revision del Architect | R1 CHANGES REQUIRED, R2 AGREED (`SAME-SESSION ROLE`, declarado en las decisiones §5) |
| RED / implementacion | `93b93f787ce443f70c1296bb87b016f57583b46f` / `7e7e9178165e7f3691e8d7888417bd0fc34012e4` |
| Arnes de host | corrida 1 `579de1af16dfe91845f3b8342bd4fc26b416412c` (VALID/UNKNOWN, historica); corrida 2 `b7ee683ecef8efc3e41c8b962fa172846daea614` (VALID/PASS) |
| Evidencia de host publicada | `9b269ba9dbabfcd7cb5b394ad4d6b4ff0633f4df` |
| Candidato | `8f7847ad5114aec9262bfc44012f56bfd1e6f966` |
| Cierre / merge / tag | el SHA de cierre, el `MERGE_SHA`, el CI posterior al merge y la limpieza viven en el tag anotado `integration/I-52-AUTH15-C1` y en el cuerpo del commit de cierre (WORKFLOW §11.4: un commit no contiene su propio SHA) |

## 6. Residuales (ninguno bloquea)

- **Menor.** HV-00 (identidad y enlace), HV-04 (`SaveAs`/reapertura) y HV-11 (inmutabilidad) del arnes corrieron sobre bases laterales; no son filas H de la matriz ni condicionan ninguna de ellas.
- **Menor.** El fallo de serializacion como causa de `InvalidEnvelope` y `EnvelopeWriteFailed` no son ejercitables sin inyeccion de fallos; no forman parte de la matriz congelada; quedan cubiertos por las guardas de fuente (I-3) y por el codigo sin cambio.
- **Menor.** Las 17 omisiones de UI Full son historicas (sin cambio).
- **Consumidor.** Las pruebas de I-55 que fijan `Name` en la precondicion de AUTH-15 (p. ej. `G16_ID19_TheAuth15PreconditionIsExactlyIdKindAndName`) y la politica `UnnamedRackNotProjectable` se reconcilian en I-55; no son parte de esta unidad.
- **Abierto material:** ninguno.

## 7. Metricas

Metricas de duracion y repeticion de la unidad: UNKNOWN (no se midieron para esta unidad).

## 8. Referencia de integracion

`integration/I-52-AUTH15-C1` (tag anotado; destino = `MERGE_SHA`). El contrato congelado es inmutable y no se edita al cerrar; el estado vivo esta en el ROADMAP, el HANDOFF, este archivo y el tag.
