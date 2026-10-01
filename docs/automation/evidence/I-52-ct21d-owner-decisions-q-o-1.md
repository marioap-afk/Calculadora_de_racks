# I-52 — CT-21D Owner decisions: Q-O-1, Q-O-0, RQ-1, instrument implementation (verbatim record)

> Verbatim copy of the Owner's message of 2026-09-30 (chat, after decisions sections 240 and 241). Recorded by the implementer; the decisions log entry is section 242. Not edited. The only addition is this header.

```text
OWNER I-52 / CT-21D — Q-O-1 AND HOST-INSTRUMENTATION RULING

Tomo las siguientes decisiones.

==================================================
Q-O-1
==================================================

Q_O_1 = ALSO_SIDE_DB_WRITES_AND_NEW_FILES

La autorización previa de host "read-only" debe interpretarse como:

READ_ONLY_PRODUCT_STATE

y NO como:

NO_WRITES_ANYWHERE_ON_THE_MACHINE.

Para la caracterización host NO gobernante autorizo de forma limitada:

1. creación y escritura en SIDE / SCRATCH DATABASES desechables usadas
   exclusivamente por instrumentación;

2. creación de NUEVOS archivos de evidencia/resultados dentro de una raíz
   allowlisted y fijada por la tuple/package;

3. logs, JSON, hashes, .pin, probe results y otros outputs expresamente
   declarados por los instrumentos aprobados;

4. las actividades RS necesarias para:
   - I-3
   - I-5
   - I-6
   - HF-C4
   - OP2
   - TOL_SCALE host characterization
   cuando sus diseños/instrumentos hayan pasado los gates requeridos.

==================================================
LÍMITES DE Q-O-1
==================================================

Esta autorización NO permite:

- modificar el governed AutoCAD document;
- guardar el governed drawing;
- modificar el original library DWG;
- escribir mediante product seams en el documento;
- escribir en src/;
- implementar RACKMIRROR;
- sobrescribir archivos arbitrarios;
- escribir fuera de los paths allowlisted;
- usar side DB para introducir cambios indirectos en el governed document;
- dejar estado persistente no declarado entre corridas;
- convertir una corrida no gobernante en governing.

SIDE DBs usadas por instrumentación deben ser:

- control-plane owned;
- disposable;
- no registradas como governed documents;
- identificadas en evidencia;
- limpiadas/verificadas según el contrato.

Los archivos nuevos deben:

- estar bajo una evidence/scratch root fija;
- usar paths resueltos y validados;
- no escapar de la raíz;
- quedar declarados en el output schema;
- entrar en hashes/custody cuando corresponda.

==================================================
Q-O-0
==================================================

Q_O_0_READING = ACCEPT

Acepto la interpretación:

- memory reads;
- read-only inspection of host/product state;
- creation of declared evidence files;
- disposable side-DB activity of the control plane;

son compatibles con "read-only host characterization" mientras NO modifiquen
el governed product/document state.

Usar en adelante la distinción:

READ_ONLY_PRODUCT_STATE
vs
CONTROL_PLANE_AUXILIARY_WRITES.

==================================================
RQ-1
==================================================

RQ1_READING = ACCEPT

Acepto la tuple en dos fases.

PHASE 1 — PRE-SESSION

Antes de iniciar AutoCAD fijar todo lo conocible de antemano, incluyendo:

- machine identity / machine class;
- Windows build;
- expected AutoCAD build/profile;
- exact RackCad package/DLL hashes;
- exact instrument build/hashes;
- allowed-api control hashes;
- declaredSet;
- authorized paths;
- configuration inputs.

PHASE 2 — SESSION BINDING

Inmediatamente tras iniciar la sesión registrar únicamente hechos que nacen
con ella, incluyendo:

- PID;
- process start timestamp;
- actual loaded-module set;
- session/profile facts;
- .pin;
- runtime identities requeridas.

PHASE 2 debe quedar completa y validada ANTES de la primera probe /
characterization activity que vaya a producir evidencia usada por este gate.

No interpretar "antes de cualquier otra actividad" como prohibición de la
actividad interna inevitable del startup de AutoCAD.

==================================================
MACHINE
==================================================

MACHINE_DESIGNATION sigue:

DELEGATED_TO_CAD_MANAGER

No ejecutar host hasta que el CAD manager:

1. designe la máquina;
2. publique machine-class record;
3. complete la fase pre-session de la tuple;
4. deje preparado el mecanismo para completar session binding;
5. publique/registre declaredSet y .pin según el contrato.

==================================================
INSTRUMENT IMPLEMENTATION
==================================================

Con el diseño TOL_SCALE ya aprobado:

INSTRUMENT_IMPLEMENTATION_GATE =
OPEN_FOR_AUTHORIZED_RESEARCH_INSTRUMENTS

Puedes abrir los gates técnicos separados para implementar/revisar:

- clase RS I-3 / I-5 / I-6;
- HF-C4;
- OP2;
- instrumentos necesarios para TOL_SCALE;
- host-type facts autorizados por G6;

SIEMPRE fuera de src/ de producto y bajo eng/research/ o autoridad
equivalente.

Cada unidad:

IMPLEMENT
→ tests without AutoCAD where possible
→ prohibited-API analysis
→ independent source review
→ hashes
→ Architect review
→ Coordinator ruling.

No ejecutar host automáticamente al terminar implementación.

==================================================
PROHIBITED-API GUARANTEE
==================================================

Mantener el modelo conservador aprendido de R0.

El análisis automático NO es autoridad suficiente por sí solo.

Antes del host:

- independent human/source review required;
- exact allowed-apis list hash;
- exact formats/schema hash;
- instrument DLL hashes;
- package hash;
- CAD manager verifies hashes.

Cualquier cambio de bytes después de review invalida la aprobación.

==================================================
NEXT WORK
==================================================

Continúa autónomamente:

1. Architect finaliza/mantiene el diseño TOL_SCALE acordado.
2. Implementer desarrolla instrumentos RS/HF-C4/OP2/TOL_SCALE dentro del
   alcance anterior.
3. Adversarial review de los instrumentos.
4. Architect revisa allowed-apis.txt y formatos.
5. CAD manager designa máquina y publica tuple record.
6. Coordinator arma HOST PACKAGE v3.
7. Coordinator decide FIRST_NON_GOVERNING_HOST_RUN solamente cuando:
   - máquina;
   - tuple;
   - hashes;
   - instrumentos;
   - source review;
   - allowed APIs;
   - paths;
   - cleanup;
   - scenario exacto;
   estén todos cerrados.

Después podrán ejecutarse SOLO las probes no gobernantes explícitamente
autorizadas.

==================================================
NO CAMBIA
==================================================

CT21D_AUTHORITY_BASELINE_READY = FALSE
CT21D_EXECUTION_READY = FALSE
CT21D_EXECUTION = NOT_AUTHORIZED

CURRENTLY_ADMISSIBLE_KIND_SCOPE = NONE
CIA = UNKNOWN
SafeOperationalState = FALSE_FOR_ADMISSION
G3 = STOPPED
ALT-21E = FALLBACK_IN_EFFECT

Owner Act 2 sigue reservado.
```
