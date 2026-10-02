# I-62 — Disposición del Owner y del Coordinator sobre la revisión formal del Architect de la Proposal V11 (registro)

```text
Emisores:         Owner y Coordinator exclusivo de I-62 (disposición de la revisión formal y orden de la corrección estrecha V12)
Naturaleza:       disposición de una revisión y orden de corrección; no es dictamen del Architect, Freeze, decisión de OD-6 ni autorización de implementación
Fecha:            2026-10-02 (UTC)
Fuente original:  texto pegado por el Owner en la conversación de la sesión responsable («I-62 — PROPOSAL V12 / FINAL NARROW ARCHITECT CORRECTION»); sin
                  archivo de origen. Procedencia: transcripción literal recibida (6 591 bytes en UTF-8, SHA-256
                  79fea3d7fbf8ad7f121050bc7e1d79410d9831dd30d5f885cd6ddfe31ef4c4c9), custodiada fuera del repositorio con la transcripción de la sesión
Objeto:           revisión formal del Architect de la Proposal V11 (commit 26127a69a7dbc1324566b081381cd11db6b20f35, blob 3e8fa9d8eb8a875069224e8ed7fd5850145e4d0e);
                  registro I-62-architect-review-v11.md; resultado y evidencia en docs/automation/evidence/I-62-architect-v11/R20261002T140232Z-cdc5/
Disposición:      revisión formal VÁLIDA y ACREDITADA; A62-V10-03 (residuo) y A62-V11-01 ACCEPTED REQUIRED; todo lo demás CLOSED sigue CLOSED;
                  Proposal V12 autorizada como corrección estrecha
Estado:           Frozen: NO · OD-6 PENDING · FREEZE = NOT_AGREED · IMPLEMENTATION AUTHORIZATION = NO · I-61 vigente hasta que I-62 se integre
```

Registro redactado por la sesión responsable por orden del Owner y del Coordinator. La respuesta está en la [Proposal V12](I-62-proposal-v12.md) y en el
[paquete V12](I-62-architect-package-v12.md) §§3-4.

## 1. Disposición (resumen fiel)

- La revisión formal del Architect de V11 es **válida y acreditada**.
- **REQUIRED abiertos aceptados por el Coordinator:** A62-V10-03 (residuo) y A62-V11-01.
- **No se reabren:**
  - A62-V10-01, A62-V10-04 y A62-V9-04;
  - A62-V9-02, -03, -05 y -06;
  - A62-V7-01..03 y A62-V6-01..03;
  - los cierres previos del Coordinator.
- La Proposal V12 se produce como **corrección estrecha** y nada más.

## 2. Dirección exigida (resumen fiel)

| Id | Dirección |
|---|---|
| A62-V10-03 (residuo): LAUNCHING sin arranque tras el fin de la autorización | (1) LAUNCHING significa que la intención de lanzar y el `RunId` son durables, pero el arranque real puede no estar confirmado. (2) Si la autorización termina con el intento en LAUNCHING: (a) si la evidencia de runtime prueba que el proceso ARRANCÓ antes del fin, la acreditación histórica cubre el intento, que puede completarse e ingerirse con las reglas existentes; (b) si prueba que NO ARRANCÓ, CANCELLED_BEFORE_LAUNCH es legal, ningún resultado de ese intento puede ingerirse después, la reserva sigue consumida según el conteo conservador y no puede haber un lanzamiento nuevo bajo la autorización terminada; (c) si no se puede determinar, LAUNCH_UNCERTAIN, con conteo conservador, sin reintento silencioso y con la recuperación según el presupuesto de reejecuciones de transporte y las reglas de STOP. (3) La decisión se basa en la evidencia de runtime y de terminación que observa el invocador, no en una declaración del revisor. (4) Actualizar §20.5.1, la máquina de estados de §20.6, B.8.8/I-P13, F.8 y C-40 o el caso aplicable. Positivos: arrancado antes del fin con resultado válido; nunca arrancado con CANCELLED_BEFORE_LAUNCH; arranque desconocido con LAUNCH_UNCERTAIN. Negativo: un intento cancelado como no arrancado que produce después un resultado ingerido es INVALID |
| A62-V11-01: independencia de las premisas ante degradación acotada | (1) Los `PremiseRefs` identifican la proposición normativa completa, no una subcadena. (2) Con algún insumo en DEGRADED_BOUNDED, determinar para cada premisa un `PremiseEnvelope` que conserve el significado: como mínimo la oración o el elemento normativo completos, la celda o fila completa en una regla tabular, la expresión completa con operadores lógicos o de comparación, y el título o contexto que fija el alcance cuando sea material. (3) Independencia acreditada solo si todos los envoltorios y su contexto de control quedan fuera de los rangos degradados y son fieles. (4) El análisis mecánico considera al menos negación perdida o cambiada, operadores de comparación o lógicos, cuantificadores, modalidad, identificadores o rutas referidos, asociación de filas y columnas, cambios de título o alcance, y el destino de una referencia cruzada cuando la premisa depende de otra cláusula. (5) Sin independencia demostrable, INVALID_PREMISE. (6) Si no se puede determinar mecánicamente qué hallazgos son independientes, se invalida toda la revisión. (7) La evidencia la observa y la custodia el invocador; el Architect aporta los `PremiseRefs`, pero no acredita la fidelidad. (8) Ampliar C-42 con (a)-(g). (9) Actualizar el contrato de fidelidad y la ingestión del resultado |
| GAP-10: truncamiento de salidas grandes | no ampliar la arquitectura salvo que lo exija A62-V11-01. Documentarlo como evidencia de transporte medida: una lectura completa muy grande puede truncarse estructuralmente aunque la codificación sea fiel. Para invocaciones futuras, preferir lecturas acotadas por rangos para los archivos grandes. El requisito observable sigue siendo que la premisa y el envoltorio que usa un hallazgo estén disponibles fielmente. No congelar un número de líneas ni una implementación de comando |

## 3. Entrega ordenada y siguiente revisión formal

**Entrega**, producida de forma autónoma:
1. Proposal V12 completa, `Frozen: NO`;
2. paquete del Architect V12;
3. este registro;
4. decisiones, evidencia, estado y contrato;
5. delta V11 → V12 exacto;
6. commit y blobs exactos;
7. CI de publicación exacta.

Límites: sin implementación, ediciones normativas compartidas, pilotos, OD-6 ni Freeze. Tras la CI en verde: STOP e informe único.

**Siguiente revisión:** V12 tendrá **una** revisión formal limpia. Usa el mecanismo de fidelidad ya probado, con una mejora: lecturas acotadas por rangos
para los archivos muy grandes. Antes de lanzar hacen falta:
- el `EffectiveInputClosure`;
- el preflight de fidelidad por el mismo camino;
- la evidencia de identidad del runtime;
- la exención acotada de `dotnet test` si sigue haciendo falta.

El Architect debe disponer de forma explícita de A62-V10-03 (residuo) y de A62-V11-01, y confirmar que todo lo CLOSED sigue CLOSED salvo que V12 introduzca
una contradicción nueva. Con cero REQUIRED: el Owner decide OD-6, el Coordinator confirma el acuerdo sobre la V12 exacta, y llega el Consensus Freeze.

## 4. Estado declarado por la disposición

```text
V11 = CHANGES REQUIRED
OPEN REQUIRED:   A62-V10-03 residual, A62-V11-01
OD-6 = PENDING · FREEZE = NOT_AGREED · IMPLEMENTATION AUTHORIZATION = NO · I-61 REMAINS ACTIVE UNTIL I-62 IS INTEGRATED
```
