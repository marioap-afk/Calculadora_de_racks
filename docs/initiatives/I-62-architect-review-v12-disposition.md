# I-62 — Disposición del Owner y del Coordinator sobre la revisión formal del Architect de la Proposal V12 (registro)

```text
Emisores:         Owner y Coordinator exclusivo de I-62 (disposición de la revisión formal y orden de la corrección final V13)
Naturaleza:       disposición de una revisión y orden de corrección; no es dictamen del Architect, Freeze, decisión de OD-6 ni autorización de implementación
Fecha:            2026-10-02 (UTC)
Fuente original:  texto pegado por el Owner en la conversación de la sesión responsable («I-62 — PROPOSAL V13 / FINAL FIDELITY RESIDUAL»); sin archivo de
                  origen. Procedencia: transcripción literal recibida (6 335 bytes en UTF-8, SHA-256
                  809df4c33ba5544acd69f8e997808f50c48b0bcf132209aca45128cce29e0525), custodiada fuera del repositorio con la transcripción de la sesión
Objeto:           revisión formal del Architect de la Proposal V12 (commit d0947c5d58280f8b0bfe2808dd8b7ff79e9f76bb, blob 320cecc9bd1b68112509e510b97c768b6d505ba2);
                  registro I-62-architect-review-v12.md; custodia en 4a4ceae712cecd3a0667b3ed3600a042b014e8d4, con el resultado y la evidencia en
                  docs/automation/evidence/I-62-architect-v12/R20261002T151854Z-603d/
Disposición:      A62-V10-03 = CLOSED; A62-V11-01 = ACCEPTED REQUIRED, solo el residuo; todo lo demás CLOSED o SUPERSEDED sigue igual; la revisión de V11
                  sigue ACREDITADA, con su registro de evidencia corregido; Proposal V13 autorizada como corrección final
Estado:           Frozen: NO · OD-6 PENDING · FREEZE = NOT_AGREED · IMPLEMENTATION AUTHORIZATION = NO · I-61 vigente hasta que I-62 se integre
```

Registro redactado por la sesión responsable por orden del Owner y del Coordinator. La respuesta está en la [Proposal V13](I-62-proposal-v13.md) y en el
[paquete V13](I-62-architect-package-v13.md) §§3-4.

## 1. Disposición (resumen fiel)

- **A62-V10-03 = CLOSED.**
- **A62-V11-01 = ACCEPTED REQUIRED**, solo el residuo.
- Todo lo demás CLOSED o SUPERSEDED sigue igual. No se reabren hallazgos de V11 ni de V12.
- **Revisión de V11:** sigue acreditada. Su registro de evidencia queda corregido por la auditoría posterior de lo visible por el revisor. Sus disposiciones
  técnicas siguen siendo válidas, porque la Proposal y el paquete revisados, y todas las premisas usadas, le llegaron fielmente.

## 2. Dirección exigida (resumen fiel)

| Id | Dirección |
|---|---|
| A62-V11-01 (residuo): dependencias normativas transitivas | (1) Problema: V12 amplía bien el `PremiseEnvelope` más allá de la subcadena, pero solo incluye una referencia cruzada de control cuando el Architect la cita; no basta, porque una proposición puede depender de otra cláusula sin que el hallazgo la cite aparte. Ejemplo: «la acción X solo se permite en las condiciones de §B»; si el cuerpo de §B llegó degradado, la conclusión puede ser falsa aunque A y el título de §B sean fieles. (2) La fidelidad de una premisa sigue las dependencias normativas, no las citas del revisor: una `NormativeDependencyClosure` transitiva por cada `PremiseEnvelope`, determinada mecánicamente por el invocador desde la fuente canónica. (3) Como mínimo se siguen: referencias explícitas a secciones o cláusulas; «según», «sujeto a», «salvo», «a menos que», «solo si», «tal como se define en»; identificadores definidos fuera de la proposición; tablas, filas y enumeraciones referidas; invariantes; algoritmos y definiciones de transiciones de estado; cláusulas de autoridad o precedencia. (4) Recursión hasta un punto fijo o una fuente terminal acotada de forma explícita. (5) De cada dependencia, la unidad de control completa, no solo el título: párrafo entero; elemento con su padre cuando hace falta; cabecera con la fila o celda; invariante completo; bloque o pasos del algoritmo; de una sección con alternativas, solo la parte aplicable si se puede establecer de forma determinista, y si no, la sección pertinente entera |
| Regla de fidelidad | un hallazgo es independiente de una entrada DEGRADED_BOUNDED solo si (1) sus envoltorios directos son fieles, (2) todo elemento de la clausura es fiel y (3) todo el contexto de control de alcance, títulos, tablas y operadores es fiel. Una dependencia que solapa un tramo degradado → INVALID_PREMISE. Una aplicabilidad que no se puede determinar mecánicamente → el hallazgo se invalida de forma conservadora. Una independencia por hallazgo que no se puede acotar → se invalida toda la revisión. El Architect no certifica la clausura: la calcula y la custodia el invocador o su adapter |
| Ciclos | el algoritmo registra las unidades canónicas visitadas, termina en el punto fijo, conserva toda dependencia única y nunca recurre de forma indefinida. Un ciclo no es un error si la clausura es determinista |
| Referencias sin resolver | destino inexistente, ambiguo o varios indistinguibles → independencia UNKNOWN; el hallazgo no se puede acreditar. No se adivina |
| C-42 | como mínimo: (1) proposición y título referido fieles, cuerpo referido degradado → INVALID_PREMISE; (2) remisión a otra cláusula con su párrafo de control completo fiel → válido; (3) cadena A → B → C con C degradada → inválido; (4) ciclo A ↔ B, ambas fieles → válido y termina; (5) referencia sin resolver → inválido o UNKNOWN; (6) cláusula degradada ajena a la clausura → el hallazgo sigue válido; (7) regla de tabla que remite a una enumeración o definición de otro lugar, degradada → inválido |
| GAP-10 y visibilidad de la salida de herramientas | conservar la lección medida en V12: los registros de eventos no bastan como evidencia de lo que vio el modelo. Para las auditorías futuras, la fuente debe representar la salida de herramientas visible para el revisor, con sus truncamientos. Invariante: la evidencia de fidelidad describe la representación entregada al modelo revisor, no solo la salida completa capturada en otro lugar. No congelar una implementación de proveedor. Las lecturas acotadas de archivos grandes siguen siendo una mitigación operativa, no un número de líneas normativo |
| Compactación del contexto | registrarla como evidencia de runtime; no invalidar una revisión solo porque ocurra. Toda premisa usada después debe cumplir los mismos requisitos de fidelidad y de dependencias con el contexto y la evidencia visibles para el protocolo. No citar resúmenes cifrados inaccesibles como evidencia |

## 3. Entrega ordenada y siguiente revisión formal

**Entrega**, producida de forma autónoma:
1. Proposal V13 completa, `Frozen: NO`;
2. paquete del Architect V13;
3. este registro;
4. decisiones, evidencia, estado y contrato;
5. delta V12 → V13 exacto;
6. commit y blobs exactos;
7. CI exacta.

Límites: sin implementación, ediciones normativas compartidas, pilotos, OD-6 ni Freeze. Tras la CI exacta en verde: STOP e informe único.

**Siguiente revisión:** V13 tendrá **una** revisión formal final del Architect, con el mecanismo de fidelidad ya probado y:
- la auditoría de la salida visible para el revisor;
- lecturas acotadas o por rangos para los archivos grandes;
- la verificación de la `NormativeDependencyClosure`;
- la misma exención acotada de `dotnet test` si hace falta.

El Architect solo dispone el residuo de A62-V11-01 y confirma que todos los cierres anteriores siguen válidos. Con cero REQUIRED: el Owner decide OD-6, el
Coordinator confirma el acuerdo sobre la V13 exacta, y llega el Consensus Freeze.

## 4. Estado declarado por la disposición

```text
V12 = CHANGES REQUIRED
OPEN REQUIRED:   A62-V11-01 residual
A62-V10-03 = CLOSED · V11 review = ACCREDITED WITH EVIDENCE CORRECTION
OD-6 = PENDING · FREEZE = NOT_AGREED · IMPLEMENTATION AUTHORIZATION = NO · I-61 REMAINS ACTIVE UNTIL I-62 IS INTEGRATED
```
