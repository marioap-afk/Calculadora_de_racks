# I-62 — Revisión del Architect de la Proposal V7 y disposición del Coordinator (registro)

```text
Emisores:         Architect (revisión formal, SEPARATE SESSION) y Coordinator exclusivo de I-62 (disposición)
Naturaleza:       dictamen del Architect + disposición del Coordinator; no es firma del Owner, decisión del Master, Freeze ni autorización de implementación
Fecha:            2026-10-01
Fuente recibida:  la orden «I-62 — PROPOSAL V8 / NARROW ARCHITECT CORRECTION», pegada por el Owner en la conversación de la sesión responsable (sin archivo
                  de origen en D:\IDs; 5 433 bytes en UTF-8, SHA-256 e0a2b496c12d71e5325268bd548c668af4bf48ed934fdf23b048aaa537e5003e, custodiada fuera
                  del repositorio con la transcripción de la sesión)
Texto literal del dictamen del Architect: NO recibido por la sesión autora. Este registro conserva el veredicto, los ids y las direcciones de corrección
                  tal como los transmite el Coordinator
Objeto revisado:  commit 3d7ec77f07d374704f2bc0b888a07bb182c32203
                  docs/initiatives/I-62-proposal-v7.md, blob 9f68c9501fb8254792f875a039e11ec7485108ef
Veredicto:        Architect: CHANGES REQUIRED (A62-V7-01, A62-V7-02, A62-V7-03). Coordinator: los tres ACEPTADOS
Estado:           Frozen: NO · OD-6 PENDING · IMPLEMENTATION AUTHORIZATION = NO
```

Registro redactado por la sesión responsable por orden del Coordinator. La respuesta de la sesión está en la [Proposal V8](I-62-proposal-v8.md) y en el
[paquete V8](I-62-architect-package-v8.md) §4.

## 1. Disposición del Coordinator

Se aceptan los tres REQUIRED. Se preservan todos los cierres anteriores. No se reabren el Discovery, la aplicabilidad opt-in, el Modelo A, la compatibilidad
legacy, FX-04a/FX-04b ni los hallazgos previos del Coordinator.

## 2. Hallazgos REQUIRED y dirección exigida (según el Coordinator)

### A62-V7-01 — Invariantes del rebase

Se corrige el modelo de invariantes, no el diseño del rebase.

1. **I-P05 y QU:** el QU ordinario mantiene la prohibición vigente; el QU REBASE_RECONCILIATION es la excepción explícita y puede actualizar exactamente los
   campos SHA del estado que listan I-P10 y B.8.7.
2. **I-H02:** la invariante «los SHA de rama guardados deben resolver a sus imágenes actuales antes de más trabajo delegado» se aplica solo fuera de una
   ventana Q0 activa.
3. **Rebase dentro de una ventana activa:**
   - se conserva la vía de reverificación de 16.7;
   - su `RebaseMap` incluye `StateFields[]` con todo SHA persistido del estado afectado por la reescritura, no solo `BaseSha`, `ChainBaseSha` y los commits
     del Worker;
   - no se inserta ningún QU dentro de la ventana;
   - el Q7 o QR de cierre hace la reconciliación durable de esos `StateFields`;
   - se añade una invariante de pares equivalente a I-P10 para ese cierre.
4. **C-15:** rebase fuera de ventana; rebase dentro de ventana; SHAs persistidos de cadenas ajenas o anteriores; reanudación en un clon limpio.
5. **C-18:** pares positivos para que I-P05, I-P10 y la regla de reconciliación dentro de la ventana puedan estar en GREEN a la vez.

### A62-V7-02 — QH + Principal nuevo + avance de `main`

Se define un solo orden ejecutable, con esta dirección preferida. Un QH está RELEASED: ningún Principal actual puede hacer trabajo ordinario. Para un
Principal nuevo que abre con `main` avanzado:
1. el Coordinator designa al Principal entrante para una operación acotada REBASE_TAKEOVER;
2. esa operación solo autoriza a:
   - hacer `fetch`;
   - hacer el rebase que exige WORKFLOW;
   - publicar el rebase con `--force-with-lease`;
   - producir y custodiar el `RebaseMap`;
   - publicar un único punto QR REBASE_RECONCILIATION combinado;
3. ese QR registra al nuevo Principal y su designación y reconcilia todos los SHA reescritos del estado;
4. solo después de ese QR puede el nuevo Principal hacer trabajo ordinario o publicar Q0.

Es una excepción o especialización de orden, acotada y declarada bajo OD-1, no un permiso para trabajar antes de rebasar. El mismo mecanismo vale para T12b
cuando el titular anterior falta y `main` avanzó.

**Actualizar:** T16 y T19, o sustituirlas por una transición explícita; I-P02; la semántica de los puntos en B.8; C-15; F.6 con QH + avance de `main` + host
nuevo limpio. No puede haber a la vez un requisito de QR antes del rebase y otro de rebase antes del QR.

### A62-V7-03 — `ReviewSubject` y operador humano

Limitar `ReviewSubject.Authors` a la autoría de la UNIDAD u OBJETO revisado, no a la vida histórica de sus rutas.
- **Revisión de implementación o conformidad:** los commits de autor son los del rango de cambio propio de la unidad,
  `merge-base(base de la unidad o autoridad de main, commit revisado)..commit revisado`, restringidos a las rutas revisadas de la unidad o del cambio. No se
  incluyen commits históricos anteriores a la unidad.
- **Revisión de diseño:** son autores los autores o sesiones que produjeron las versiones de la Proposal o los deltas aceptados de esta unidad que contribuyen
  a la versión exacta revisada.
- Las sucesiones y los rebindings siguen incluidos. Un autor desconocido dentro de ese conjunto acotado sigue fallando cerrado.

**Regla de EXTERNAL HUMAN.** En la alternativa 1 de OD-6, el humano (Owner u operador) que dirigió materialmente una sesión IA autora **es** autor para la
comparación de independencia de Actor de una revisión EXTERNAL HUMAN. Ese mismo humano no puede satisfacer el requisito de Actor humano independiente al
revisar el objeto que dirigió. Se registra como parte de la alternativa 1, sin decidir OD-6.

**Casos de C-13:**
- un commit histórico anterior a la unidad queda excluido;
- el autor de una sucesión anterior del Principal queda incluido;
- el mismo operador humano actuando como revisor EXTERNAL HUMAN se rechaza;
- un humano externo ajeno, con insumos canónicos, se acepta.

## 3. Hallazgos OPTIONAL

Se aplican solo correcciones locales y de bajo riesgo:

| Id | Condición del Coordinator | Tratamiento en V8 |
|---|---|---|
| O-V7-01 | aplicar si es solo una aclaración de nombres o de historia del estado | **no aplicado**: su texto no se transmitió a la sesión autora |
| O-V7-03 | aplicar si es solo localizar la plantilla de los marcadores de G0 | aplicado (§8.6) |
| O-01 | aplicar si C-18 puede validar los archivos `/v2` reales sin introducir Level B | aplicado (C-18) |
| O-V7-02, O-05, O-07 | no ampliar V8 por ellos | sin cambio |

## 4. Entrega, siguiente revisión y estado

- **Entrega:** Proposal V8 completa (`Frozen: NO`); paquete V8; este registro; decisiones, evidencia, estado y contrato propios; commit y blobs exactos; CI
  exacta. Sin indicaciones intermedias; sin implementar; sin invocar Workers ni Controllers; sin modificar normas compartidas; sin decidir OD-6. Tras la
  publicación y la CI verde: STOP y un solo informe.
- **Siguiente revisión:** V8 va directa a una revisión formal **nueva** del Architect, que debe:
  - usar una sesión distinta de la autora;
  - ejecutarse desde un clon o ruta limpios, sin la memoria de proyecto del autor;
  - no recibir la transcripción de la conversación del autor;
  - declarar todo contexto inyectado automáticamente.

  Con cero REQUIRED, el paso siguiente es que el Owner decida OD-6 y después el Consensus Freeze.
- **Estado declarado:**
  - Proposal V7 = CHANGES REQUIRED — Architect; Proposal V8 = NARROW AUTONOMOUS CORRECTION AUTHORIZED;
  - REQUIRED abiertos: A62-V7-01, A62-V7-02, A62-V7-03;
  - OD-6 = PENDING OWNER DECISION; FREEZE = NOT_AGREED; IMPLEMENTATION AUTHORIZATION = NO;
  - I-61 REMAINS THE ACTIVE PROTOCOL UNTIL I-62 IS INTEGRATED.
