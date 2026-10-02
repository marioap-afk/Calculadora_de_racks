# ADR-0048: Ejecución delegada portable: roles independientes del proveedor, binding por capacidad acreditada y autoverificación del Principal

- **Estado:** propuesto
- **Fecha:** 2026-10-02
- **Decisores:** Owner del repositorio, único que lo acepta (OD-1). Redactado por la sesión principal de I-62 como materialización del Anexo A de su Freeze.
- **Iniciativa relacionada:** I-62 — Principal Coordinator Portability & Provider-Agnostic Role Binding (`architecture/portabilidad-coordinador-principal`)
- **Sucede parcialmente a:** [ADR-0046](0046-protocolo-de-ejecucion-delegada-de-agentes.md), que sigue `aceptado` en lo que este ADR conserva y para las
  unidades I61.
- **Proposal:** [I-62-proposal-v14.md](../initiatives/I-62-proposal-v14.md) (commit `4c617e82`, blob `34ad80ea`), congelada por el
  [Consensus Freeze](../initiatives/I-62-consensus-freeze.md) con OD-6 = alternativa 1.

> Materializa el Anexo A de la Proposal V14 congelada. No añade decisiones ni semántica fuera del Freeze y no es autoridad superior a él. Mientras sea
> `propuesto` no gobierna nada; aceptado, rige solo desde `I62_EFFECTIVE_SHA` («Vigencia»). No tiene fila en el índice hasta el cierre documental de I-62.

## Contexto

ADR-0046 fijó por mandato el proveedor del Controller y aceptó una independencia parcial de la verificación. El protocolo integrado asocia así roles con
proveedores, depende de memoria privada para reanudar y no acredita la configuración de la sesión principal antes del trabajo delegado (mandato de I-62;
[Discovery](../initiatives/I-62-discovery.md) R1). El objetivo de I-62 es vincular cada rol a un proveedor, un modelo o un runtime según **capacidad
acreditada**, y que otro Principal pueda reanudar la unidad sin memoria privada (Proposal V14 §1).

Los hechos acotan la solución (Proposal V14 §10 y §19):
- el modelo servido detrás de un proveedor no es observable;
- la autodeclaración de un modelo no es fuente;
- la introspección depende de que cada runtime ofrezca una fuente RUNTIME_OBSERVED;
- Git no prueba quién opera en otra máquina.

## Decisión

Este ADR es sucesor **parcial** de ADR-0046.

**Conserva de ADR-0046:**
- #2, declaraciones;
- #3, relevo;
- #4, elegibilidad por invocación medida, consumo cubierto y frescura;
- #5, esquemas estrictos, versionados por protocolo;
- #6, verificación fail-closed con 14 comprobaciones (delta en la Proposal V14, Anexo G);
- #7, presupuesto único sin reinicios, con la autoridad de la Proposal V14 §9.3;
- #8, transporte;
- #9, Level A.

**Supera:**
- el proveedor fijo del Controller y el título de ADR-0046;
- la «independencia parcial» aceptada, que pasa a ser una política por dimensiones (punto 6).

**Amplía #1 (OWN-L).** Es materia **OWNER-RESERVED**:
- obligaciones de la sesión principal fuera de una delegación;
- la fila de `WORKFLOW.md` §10;
- el **punto de entrada de compatibilidad**, en una sección nueva de WORKFLOW, que rige la evaluación de los contratos de toda unidad desde
  `I62_EFFECTIVE_SHA` (Proposal V14 Anexo E.3);
- con OD-6 = alternativa 1, el predicado de independencia de LIFECYCLE (punto 6);
- la **orquestación autónoma de roles** (Proposal V14 §20): RELAY automático, ESCALATION solo ante fronteras reales y autoridad sin cambios, con la
  materialización autorizada de bindings del Architect, los contratos de salida por rol, el cierre efectivo de insumos con la exención acotada de acciones
  iniciales incompatibles (sin equivalencia de evidencia), la fidelidad de los insumos y la identidad del runtime observada por el invocador.

Conserva el carácter **opt-in** de la ejecución delegada (`AUTOMATION_PLAN.md` §16): solo la adopta una unidad con decisión registrada I62_DELEGATED
(Proposal V14 §14.0).

1. **Roles semánticos sin proveedor** (Proposal V14 §2). PRINCIPAL_COORDINATOR, ARCHITECT, EXECUTION_CONTROLLER, WORKER y REVIEWER, cada uno con lo que
   declara y lo que nunca declara. Ningún rol nombra proveedor, modelo ni runtime. El Coordinator de gates no es vinculable. La acumulación de roles compara la
   identidad observable del actor, nunca el binding.
2. **Binding por capacidad acreditada** (§5). Un rol se vincula a un runtime solo si se cumplen cuatro condiciones:
   - los requisitos obligatorios del rol y de la acción están en MATCH o ABOVE_REQUIRED;
   - se cumple la independencia exigida;
   - hay una observación de capacidad vigente;
   - la celda cumple la elegibilidad conservada de ADR-0046 #4.

   El binding consume la observación, y la observación no depende del binding. Una entrada de catálogo no acredita los hechos, y una cuota desconocida no
   equivale a consumo autorizado. Sin celda elegible, no se invoca. La aceptación es individual del Coordinator o una materialización autorizada, nunca
   inferida.
3. **Perfil del Principal por acción** (§4.1). Clase «Coordinación principal» y perfil PRINCIPAL_COORDINATION, con requisitos obligatorios por acción:
   RESUME_DECISION, CUSTODY y evidencia local, esta solo cuando la acción debe producirla. El perfil existe aunque el binding del Principal no pueda aceptarse.
   CUSTODY no es un permiso de trabajo directo: una CUSTODY en UNKNOWN o BELOW_REQUIRED bloquea solo la maquinaria delegada, nunca el trabajo directo
   autorizado.
4. **`CONFIGURATION_STATUS`** (§4.2). Estados MATCH, ABOVE_REQUIRED, BELOW_REQUIRED y UNKNOWN, agregados solo sobre los requisitos obligatorios de la acción,
   con el agregado de la Proposal V14 §4.2. UNKNOWN nunca se convierte en MATCH: una decisión del Owner nunca convierte lo no observado en MATCH, y una
   recuperación es una observación nueva, nunca una reconfiguración automática ni un reset. El estado no es la disposición.
5. **Autoverificación del Principal** (§4.1 y §10). Al abrir una unidad I62_DELEGATED, el Principal evalúa su `CONFIGURATION_STATUS` por acción:
   - con hechos observables, con el mínimo RUNTIME_OBSERVED para los obligatorios;
   - sin aceptar como fuente la autodeclaración del modelo;
   - sin depender de su binding.
6. **Independencia por dimensiones** (§11). Las dimensiones son Actor, Sesión, Contexto y Proveedor, con valores REQUIRED, PREFERRED o NOT_REQUIRED según el
   disparador de riesgo. Se evalúan sobre la identidad observable del actor y contra cada autor del rango evaluado. UNKNOWN en una dimensión REQUIRED no la
   satisface.

   Con OD-6 = alternativa 1, las revisiones mayores de las unidades que adoptan I62 exigen Actor, Sesión y Contexto REQUIRED, y Proveedor PREFERRED. Los tres
   modos de LIFECYCLE §5 se conservan, pero SAME-SESSION ROLE no basta para esas revisiones. La regla no es retroactiva.
7. **Independencia del proveedor** (§1, §7 y §11.1). El proveedor es una dimensión más, PREFERRED salvo política explícita. Un proveedor distinto con el
   contexto de la referencia no satisface Contexto, y una marca no se prohíbe si la independencia se acredita. Proveedores, ejecutables, modelos y recetas
   viven en los descriptores de adapter y en el catálogo. El adapter renderiza la invocación concreta, y el prompt es representación, nunca autoridad.
8. **Level A** (§17 y §20.10). Contratos versionados, transiciones pequeñas y deterministas, los adapters existentes, archivos estructurados y orquestación
   por la sesión del Principal. Sin scheduler, daemon, framework de agentes ni servicio de orquestación.
9. **Compatibilidad legacy** (§14). Las unidades I61 siguen en I61 toda su vida, con su ejecución delegada y sus contratos `/v1` sin cambios, y `/v1` no se
   retira. La lectura de autoridades de las cláusulas que I-62 modifica la resuelve el resolver de compatibilidad, de forma transparente para los contratos
   I61 (Proposal V14 Anexo E; `AUTOMATION_PLAN.md` §16.13 al materializarse).

**Vigencia:**
- desde `I62_EFFECTIVE_SHA` (Proposal V14 §14.1);
- solo unidades I62_DELEGATED (§14.0), salvo el punto de entrada de WORKFLOW y §16.13, que rigen la lectura de autoridades de toda unidad;
- las unidades I61 terminan con I-61, con la resolución del Anexo E;
- `/v1` no se retira.

Antes de `I62_EFFECTIVE_SHA`, todo texto materializado de I-62 es inactivo. No gobierna ninguna operación real y no sirve de autoridad para desarrollar
I-62, que sigue gobernada por I-61 hasta su integración (Proposal V14 §15 y §20.11).

## Alternativas consideradas

- **Mantener el proveedor fijo del Controller (ADR-0046):** contradice el objetivo del mandato de vincular cada rol por capacidad acreditada (Proposal V14
  §0 y §1).
- **Fijar modelos o asociar roles a proveedores por defecto:** es un no-objetivo del mandato, y el modelo servido no es observable (§1 y §19).
- **Level B, plataforma, scheduler o servicio de orquestación:** no hay evidencia que lo justifique; F5 no se planifica (§17 y §20.10).
- **Reescribir I-61 o migrar las unidades activas:** son no-objetivos; las unidades I61 terminan con I-61 (§1 y §14).
- **Registro global entre unidades:** es un no-objetivo (§1).
- **OD-6, alternativa 2** (modo declarado, sin predicado de independencia): el Owner eligió la alternativa 1 ([decisiones de I-62](../automation/decisions/I-62.md)
  §30).

## Consecuencias

- **Positivas:**
  - roles reasignables por hechos observables;
  - un Principal independiente del proveedor;
  - UNKNOWN nunca es MATCH;
  - otro Principal puede reanudar desde el estado versionado;
  - I-61 queda intacto, y una unidad DIRECT_ONLY trabaja sin la maquinaria de I-62.
- **Negativas y costos aceptados** (riesgos residuales de la Proposal V14 §19):
  - el modelo servido detrás del proveedor no es observable, y ese límite se declara;
  - la autoverificación depende de que cada adapter tenga una fuente RUNTIME_OBSERVED;
  - aumenta el número de contratos, lo que se mitiga con Level A;
  - la auditoría de lecturas no captura las lecturas internas del runtime;
  - Git no prueba quién opera en otra máquina;
  - la continuación entre topologías y el piloto de autonomía dependen de decisiones del Owner.
- **Vigilar:**
  - que los proveedores no se filtren en los roles ni en los esquemas del núcleo (guardas C-01 y C-05);
  - que las huellas de configuración solo registren nombres de claves y hashes;
  - la frescura del catálogo.

## Referencias

- Mandato del Owner: `docs/automation/decisions/I-62-owner-mandate.txt`.
- Decisiones de I-62: `docs/automation/decisions/I-62.md` (§30, OD-6; §31, AGREED y Consensus Freeze; §32, apertura de F1).
- Proposal V14 congelada, Anexo A (borrador de este ADR), y el [Consensus Freeze](../initiatives/I-62-consensus-freeze.md).
- Discovery: `docs/initiatives/I-62-discovery.md`.
- ADR relacionados:
  - ADR-0046, sucedido parcialmente;
  - ADR-0045 (Workflow V2, «Autoridad por dominio»);
  - ADR-0001 (ramas por iniciativa).
