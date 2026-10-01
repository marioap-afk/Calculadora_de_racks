# I-52 — CT-21D Owner decisions: machine designation, G6, R8, AR10-03(b), instrument gate, PARAM-04, EVIDENCE_REPETITION (verbatim record)

> Verbatim copy of the Owner's message of 2026-09-30 (chat, after decisions sections 234 to 236). Recorded by the implementer; the decisions log entry is section 237. Not edited. The only addition is this header.

```text
OWNER I-52 / CT-21D — DECISIONS

Tomo las siguientes decisiones.

==================================================
1. MACHINE DESIGNATION
==================================================

MACHINE_DESIGNATION = DELEGATED_TO_CAD_MANAGER

El CAD manager queda autorizado a seleccionar UNA máquina física concreta
para la primera caracterización CT-21D.

Antes de cualquier host run debe publicar el registro de tuple requerido por
el contrato, incluyendo al menos:

- machine identity;
- machine-class attributes;
- Windows version/build;
- AutoCAD version/build;
- AutoCAD profile;
- relevant hardware attributes;
- user/profile identity cuando corresponda;
- loaded modules/plugins baseline;
- exact RackCad build/package/hash;
- session/process identity.

No se presume equivalencia con otras máquinas.

==================================================
2. G6
==================================================

G6 = EXTEND

Amplío BA-11 §3.3 únicamente para caracterización host NO gobernante,
READ-ONLY, de los hechos adicionales necesarios para BA-05:

- tipos/rangos RB;
- variables de contexto;
- cualquier otro host-type fact expresamente requerido por la especificación
  actual de BA-05.

Esta ampliación:

- NO autoriza governing CT-21D;
- NO autoriza product admission;
- NO autoriza escrituras de producto;
- NO autoriza expandir el alcance más allá de los facts concretos que BA-05
  necesite.

Toda observación debe quedar ligada a build/machine/session tuple exacta.

==================================================
3. R8
==================================================

R8_READING = ACCEPT

Cargar mediante NETLOAD una build RackCad específicamente fijada para una
corrida constituye identidad de BUILD/TUPLE.

No se trata por sí mismo como “plugin change” prohibido si:

- la build estaba declarada previamente para esa corrida;
- su DLL/package/hash está fijado;
- la sesión empieza bajo esa identidad;
- no hay sustitución silenciosa durante una corrida.

Una build diferente => nueva tuple.

No transferir evidencia automáticamente entre builds.

==================================================
4. AR10-03(b)
==================================================

AR10_03B_READING = ACCEPT

Cambio de módulos, actualización o build de software:

=> nueva SESSION/BUILD TUPLE.

No implica automáticamente nueva MACHINE-CLASS LABEL.

La machine-class label cambia únicamente si cambia algún atributo que BA-10
define normativamente como parte de la clase de máquina.

Registrar ambas identidades por separado:

MACHINE_CLASS
SESSION_BUILD_TUPLE

==================================================
5. INSTRUMENT IMPLEMENTATION GATE
==================================================

INSTRUMENT_IMPLEMENTATION_GATE =
OPEN_AFTER_ARCHITECT_TOL_SCALE_DESIGN

Secuencia obligatoria:

Architect
→ diseña autoridad/prueba de TOL_SCALE
→ Coordinator review
→ si PASS, queda abierto el gate para implementar los instrumentos de solo
  lectura necesarios bajo eng/research/ o ubicación equivalente gobernada.

No implementar antes del diseño y ruling.

La apertura futura debe limitarse a instrumentación/research necesaria para
los host facts ya autorizados.

NO autoriza todavía:
- governing CT-21D;
- product seams;
- RACKMIRROR implementation;
- admission runs.

==================================================
6. PARAM-04
==================================================

PARAM-04 = 3

ACCEPTED WITH CONDITION.

Interpretación normativa:

3 = MAX_ATTEMPTS_PER_SLOT

NO significa “tres retries automáticos”.

Las reglas existentes de retry siguen dominando:

- governing FAIL => no automatic retry;
- governing UNKNOWN => no automatic retry;
- INVALID => nuevo intento solo con Coordinator authorization;
- crash => ruling;
- cualquier nuevo intento debe quedar en attempt log.

Por tanto 3 es un techo de capacidad por slot, no permiso de repetir hasta
obtener PASS.

==================================================
7. EVIDENCE_REPETITION
==================================================

EVIDENCE_REPETITION = 1

ACCEPTED WITH CONDITION.

Significa una captura/paquete de evidencia normativo por ejecución cuando el
contrato no exija más.

NO sustituye ni reduce:

N_A = 44
N_B = 29
N_det = 90
N_clean = 59

ni ninguna otra repetición estadística gobernante.

Si un escenario debe correrse N veces, cada una de esas N ejecuciones sigue
siendo una ejecución distinta con su evidencia correspondiente.

==================================================
8. HOST GATE STATUS
==================================================

La compuerta de host no gobernante permanece AUTHORIZED, ahora ampliada por
G6, pero todavía requiere antes del primer run:

1. machine designation por CAD manager;
2. tuple record completo;
3. exact build/package;
4. Architect TOL_SCALE design donde aplique;
5. required read-only instruments implementados y revisados;
6. Coordinator confirmation de que cada requested run pertenece al host gate
   autorizado.

NO iniciar host antes de cumplir esos prerrequisitos.

==================================================
9. NEXT WORK
==================================================

Continúa autónomamente:

A. Architect:
- diseñar TOL_SCALE authority/test;
- revisar cualquier implicación de G6;
- fijar el host-fact matrix exacto.

B. Implementer, después del ruling:
- corregir host package v2 / AR10-07;
- implementar únicamente instrumentos read-only autorizados;
- preparar machine/session fact capture;
- preparar census/type probes autorizados;
- no ejecutar todavía si falta la tuple.

C. CAD manager:
- designar máquina;
- producir machine-class + tuple record.

D. Coordinator:
- revisar instrumentación;
- revisar tuple;
- emitir FIRST_NON_GOVERNING_HOST_RUN authorization cuando todos los
  requisitos estén satisfechos.

Después:
- cerrar BA-05/BA-08/BA-04 con la evidencia no gobernante autorizada;
- continuar cadena de candidatos/sellos;
- mantener Owner Act 2 reservado.

==================================================
10. STATES THAT DO NOT CHANGE
==================================================

CT21D_AUTHORITY_BASELINE_READY = FALSE
CT21D_EXECUTION_READY = FALSE
CT21D_EXECUTION = NOT_AUTHORIZED

CURRENTLY_ADMISSIBLE_KIND_SCOPE = NONE
CIA = UNKNOWN
SafeOperationalState = FALSE_FOR_ADMISSION
G3 = STOPPED
ALT-21E = FALLBACK_IN_EFFECT

No interpretar esta decisión como aceptación del riesgo residual ni de la
garantía reducida final.
```
