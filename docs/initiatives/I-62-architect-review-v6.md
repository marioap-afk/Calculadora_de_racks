# I-62 — Revisión del Architect de la Proposal V6 y disposición del Coordinator (registro)

```text
Emisores:         Architect (revisión formal previa a la implementación) y Coordinator exclusivo de I-62 (disposición)
Modo exigido:     SEPARATE SESSION; insumos canónicos; sin transcripción ni memoria de la sesión autora (orden de revisión del Architect de V6)
Naturaleza:       dictamen del Architect + disposición del Coordinator; no es firma del Owner, decisión del Master, Freeze ni autorización de implementación
Fecha:            2026-10-01
Fuente recibida:  la orden «I-62 — PROPOSAL V7 / ARCHITECT REQUIRED CORRECTIONS», pegada por el Owner en la conversación de la sesión responsable (sin
                  archivo de origen en D:\IDs; 7 543 bytes en UTF-8, SHA-256 a330d761a1a0825ac55351873a3894b7a799ba3fc49fe2c26402adbfa4c68373,
                  custodiada fuera del repositorio con la transcripción de la sesión)
Texto literal del dictamen del Architect: NO recibido por la sesión autora. Este registro conserva el veredicto, los ids y las direcciones de corrección
                  tal como los transmite el Coordinator. El dictamen original lo custodian el Owner y el Coordinator; la sesión autora no lo infiere
Objeto revisado:  commit 3888c5c8de19b5abaf479983e56836ef2bcef76e
                  docs/initiatives/I-62-proposal-v6.md, blob 19672958ec095ba9f88e103240d34c62c76ee850
                  docs/initiatives/I-62-architect-package-v6.md, blob 028df636c0334b6044c376feacd60208b58094f5
Veredicto:        Architect: CHANGES REQUIRED (A62-V6-01, A62-V6-02, A62-V6-03). Coordinator: los tres ACEPTADOS
Estado:           Frozen: NO · OD-6 PENDING · IMPLEMENTATION AUTHORIZATION = NO
```

Registro redactado por la sesión responsable por orden del Coordinator. La respuesta de la sesión está en la [Proposal V7](I-62-proposal-v7.md) y en el
[paquete V7](I-62-architect-package-v7.md) §4.

**Antecedente:** antes de esta orden, el Owner pegó en la sesión autora la orden de revisión formal del Architect de V6. La sesión no la ejecutó, porque el
modo exigido era SEPARATE SESSION y la sesión autora no puede revisarse a sí misma. Señaló además que la memoria de proyecto de su carpeta contiene notas de
autoría y que una sesión revisora no debe cargarla.

## 1. Disposición del Coordinator

Se aceptan los tres REQUIRED. No se reabren el Discovery, G0, los hallazgos previos del Coordinator ni la historia V1-V6. Se produce **una** Proposal V7
completa que corrige los tres hallazgos de forma autónoma.

## 2. Hallazgos REQUIRED y dirección exigida (según el Coordinator)

### A62-V6-01 — Rebase fuera de una ventana

1. Un rebase de apertura de sesión, o cualquier otro rebase de 16.7 hecho sin ventana de delegación abierta:
   - hace el rebase exigido;
   - publica la rama rebasada con `--force-with-lease` y el SHA remoto anterior esperado explícito;
   - ese force-push **no** es por sí mismo un punto durable de estado.
2. Antes de cualquier Q0 o acción delegada posterior se publica un QU dedicado a la reconciliación del rebase.
3. Ese QU:
   - custodia el `RebaseMap` de forma durable como `StateRef`;
   - mapea cada SHA persistido cuya identidad cambió a su imagen;
   - cubre al menos `chain_base_sha`, `chain_red_sha`, `last_window.verified_sha` cuando aplique, `unverified_commits[].sha`, las referencias
     `SupersededBaseSha` y `SupersededCommits`, y cualquier otro campo con valor SHA cuya identidad semántica sobreviva al rebase;
   - conserva los contadores y la `TaskId`;
   - nunca descarta en silencio un SHA no mapeable.
4. Si no se puede acreditar alguna imagen o identidad de parche requerida: STOP según 16.7, sin continuar a Q0.
5. Las invariantes del estado se ajustan para que ese QU sea una transición legal y explícita de reconciliación del rebase.
6. 16.7 se añade a la matriz de delta semántico I61→I62.
7. Se añade a C-15 un escenario completo:
   - cadena IN_COURSE con el Q7 de la ventana anterior;
   - `main` avanza y una sesión nueva rebasa entre ventanas;
   - publicación con `--force-with-lease` y QU de rebase;
   - clon limpio en otra máquina y siguiente Q0;
   - la semántica de RED/`ChainRedFiles` y de los commits sin verificar sigue siendo correcta.

   La prueba no depende de objetos que solo existan en el host anterior.

### A62-V6-02 — Aplicabilidad / opt-in

**Decisión de diseño del Coordinator:** I-62 **no** hace obligatoria la ejecución delegada para toda iniciativa reclamada después. Se conserva el principio de
I-61 de que la ejecución delegada es opt-in. Se separan:
- A, la operación de la iniciativa y del workflow;
- B, la adopción de la ejecución delegada I62.

Una unidad posterior puede quedarse en DIRECT_ONLY y trabajar con normalidad bajo WORKFLOW, sin delegación Controller/Worker. G0 registra de forma explícita
si la ejecución delegada es DIRECT_ONLY o I62_DELEGATED (u otros nombres finales equivalentes, usados de forma consistente).

**Reglas:**
1. **DIRECT_ONLY:**
   - no exige la maquinaria I62 (contrato de gate, delegación, binding, custodia) solo para hacer implementación directa;
   - puede conservar el contrato de estado ordinario que exigen hoy WORKFLOW y AUTOMATION_PLAN;
   - la capacidad CUSTODY del Principal no condiciona el trabajo directo ordinario;
   - no se puede emitir ningún contrato Controller/Worker hasta que la adopción cambie por una transición explícita autorizada.
2. **I62_DELEGATED:**
   - activa los requisitos del protocolo delegado I62: `state/v2`, bindings, preflight y custodia según su definición;
   - la adopción es explícita y durable.
3. Definir si se permite adoptar a mitad de iniciativa, de DIRECT_ONLY a I62_DELEGATED. Preferir fallo cerrado y autoridad explícita. Nunca cambiar en
   silencio el protocolo de una delegación ya abierta.
4. Acotar la acción CUSTODY del Principal a la custodia, el relevo y la orquestación bajo el protocolo delegado, no a un permiso genérico de trabajo
   directo.
5. Conciliar de forma explícita con AUTOMATION_PLAN §16: «Ninguna otra iniciativa lo adopta por estar escrito.»
6. Añadir el delta a G.1, bajo OD-1.
7. Añadir una comprobación: una iniciativa de producto nueva posterior a la activación, DIRECT_ONLY, sin participantes delegados y con la CUSTODY delegada del
   Principal en UNKNOWN ⇒ el trabajo directo autorizado sigue siendo posible y la ejecución delegada sigue indisponible.

I-62 no debe convertirse en un marco obligatorio de agentes para el trabajo normal de RackCad.

### A62-V6-03 — Independencia de LIFECYCLE / EXTERNAL HUMAN / varios autores

Corregir la alternativa 1 de OD-6 sin elegir OD-6 por el Owner. La alternativa 1 debe funcionar con todos los modos de revisión de LIFECYCLE que siguen
vigentes.

- **Revisores IA/runtime:** Actor = `ActorRef`; Sesión = `SessionRef`; Contexto = los insumos canónicos revisados.
- **EXTERNAL HUMAN:**
  - una identidad o referencia del revisor humano;
  - una instancia o referencia de revisión distinta;
  - los insumos canónicos revisados;
  - la identidad del revisor debe diferir de toda identidad de autor aplicable al objeto revisado;
  - sin exigir un `ActorRef` ni un `SessionRef` ficticios.
- **Conjunto de referencia:** **todas** las identidades y sesiones de autor que contribuyeron a la versión exacta revisada, no solo el Principal actual. Si el
  diseño se produjo con sucesiones o rebindings, todo autor que contribuyó entra en el conjunto, y la independencia se comprueba contra cada referencia
  requerida.

No retirar en silencio EXTERNAL HUMAN de LIFECYCLE. Retirar un modo de revisión vigente es un delta explícito del Owner o de LIFECYCLE, no un detalle de
implementación. OD-6 sigue PENDING.

## 3. Hallazgos OPTIONAL

Se corrigen de forma autónoma solo si la corrección es local y no amplía el alcance.

| Id | Contenido transmitido | Tratamiento ordenado |
|---|---|---|
| O-02 | sacar la lógica de I-S15 que depende de la historia de la clase de invariantes sin historia, si sigue aplicando | corregir |
| O-03 | no dar al Controller el resultado esperado del resolver calculado por el Coordinator: darle la instrucción normativa y los encabezados, dejarle calcular de forma independiente y comparar después | corregir |
| O-04 | usar la misma semántica de encabezado normalizado y de exclusión de bloques de código para localizar el punto de entrada | corregir |
| O-06 | aclarar dónde se registran las dimensiones PREFERRED de la alternativa 2 | corregir |
| O-01, O-05, O-07 | contenido no transmitido a la sesión autora | pueden quedar como seguimientos documentados si cambiarlos ampliara V7 de forma material |

## 4. Entrega, siguiente revisión y estado

- **Entrega:** Proposal V7 completa (`Frozen: NO`); paquete V7; este registro; contrato, decisiones, evidencia y estado propios; disposición V6→V7 de
  A62-V6-01..03; identidades exactas de commit y blob; CI exacta. Sin aprobaciones intermedias, salvo una ambigüedad reservada al Owner que no pueda
  representarse como BLOCKED — OWNER DECISION. Sin agentes de implementación; sin esquemas, pruebas, tooling ni fixture; sin cambios en normas compartidas;
  sin decidir OD-6. Tras publicar V7 y obtener su CI: STOP y un solo informe.
- **Siguiente revisión:** V7 va directa a una revisión formal del Architect, sin otro bucle amplio del Coordinator salvo que V7 viole esta orden de forma
  mecánica. Requisitos: SEPARATE SESSION; solo insumos canónicos; sin transcripción del autor; sin memoria de la sesión autora; declarar todo contexto
  inyectado automáticamente antes de revisar. Con cero REQUIRED, el paso siguiente es la decisión del Owner sobre OD-6 y el Consensus Freeze.
- **Estado declarado:**
  - G0 = GATE PASS; Discovery R1 = ACCEPTED AS DESIGN BASIS; Archetype = NEW ARCHITECTURE;
  - Proposal V6 = CHANGES REQUIRED — Architect; Proposal V7 = AUTHORIZED FOR AUTONOMOUS CORRECTION;
  - Architect REQUIRED abiertos: A62-V6-01, A62-V6-02, A62-V6-03;
  - OD-6 = PENDING OWNER DECISION; FREEZE = NOT_AGREED; IMPLEMENTATION AUTHORIZATION = NO;
  - I-61 REMAINS THE ACTIVE PROTOCOL UNTIL I-62 IS INTEGRATED.
