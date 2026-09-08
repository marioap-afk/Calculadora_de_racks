# I-45 — Plan consensuado

```
PLAN VERSION: V4

Coordinator:        AGREED
Architect:          AGREED
Open disagreements: NONE
CONSENSUS STATUS:   REACHED
```

> La decisión de arquitectura vive en
> [ADR-0033](../adr/0033-validacion-por-clase-de-evidencia-y-sha-exacto.md), en estado **`propuesto`**:
> el consenso Coordinador ↔ Arquitecto declara la decisión lista para implementarse y **no** equivale a
> aceptación, que solo el dueño puede otorgar. Este documento es el **plan**; el ADR es la **decisión**;
> el [contrato](I-45-test-validation-workflow.md) es el **alcance**; la
> [evidencia](I-45-discovery.md) es lo que se midió.

Este plan **no** promete acelerar el CI. El cambio esperado de su reloj es aproximadamente cero, y es
intencional. Lo que ataca es la **repetición de validación que no aporta evidencia nueva**, y lo hace
midiendo antes de reducir.

## Cómo se llegó a V4

Cuatro rondas de revisión independiente entre el Coordinador y el Arquitecto. Las versiones anteriores
**no están vigentes** y se conservan aquí solo como historia de lo rechazado:

| Versión | Qué proponía | Por qué no quedó |
|---|---|---|
| V1 | Siete gates con una clave de estado de validación por contenido como única reducción | La clave resultó **falsada**: dejaba fuera entradas reales de la suite del núcleo, y su métrica de cabecera estaba mal contada |
| V2 | Clave corregida, control retrospectivo de contradicción, gates reordenados | La clave corregida seguía comprando su valor **solo** excluyendo documentación, que es justo lo que exigía toda la maquinaria; y el control resultó de potencia nula |
| V3 | Clave retirada; reutilización por **SHA exacto** | Correcta, pero situaba el ADR en el último gate, implementando decisiones antes de registrarlas |
| **V4** | **Vigente.** V3 más **P0** como precondición documental | — |

## Arquitectura del proceso

```
CONSENSO V4
    ↓
P0 — precondición documental        (este commit)
    ↓
G0A → G2 → G0B → G1 → G3 → G4 → G5 → G6 → G7
```

La implementación conductual permanece **bloqueada** hasta que P0 esté versionado. La secuencia de
gates **no se reordena**.

## P0 — precondición documental

Ocurre después del consenso y **antes de cualquier gate de implementación**. Versiona el ADR principal,
este plan y el estado del contrato. Cumple `WORKFLOW §8`, que exige el ADR **antes** de implementar una
decisión de arquitectura. No aplica nada de lo que documenta.

## Gates

| Gate | Qué hace | Qué produce | Qué NO hace |
|---|---|---|---|
| **G0A** | Verdad normativa y `WORKFLOW`, **antes** de aplicar políticas nuevas | Base normativa exacta: contradicciones resueltas, atribuciones corregidas, afirmaciones obsoletas retiradas | No aplica LC-UI ni ninguna política nueva |
| **G2** | Seguridad y diagnóstico de CI: `timeout-minutes`, diagnóstico de cuelgues, informe de resultados de prueba como artefacto | Identidad de prueba, que hoy no existe en ningún artefacto | **No se vende como mejora de rendimiento.** Prefiere aislamiento estrecho del global de UI conocido antes que deshabilitar el paralelismo de toda la suite |
| **G0B** | Higiene no funcional de código y pruebas para restaurar reglas y guardas correctas | Guardas que protegen lo que dicen proteger | No toca producto |
| **G1** | Método reproducible de medición | Baseline versionada, incluida la primera medición del ciclo local obligatorio | No construye una plataforma de observabilidad |
| **G3** | Telemetría de repetición | Numerador y denominador reales de la repetición | **No retira ninguna obligación**: mide antes de reducir |
| **G4** | Experimento de duración activa del dueño | El dato que una decisión futura necesitaría | No gobierna ningún nivel de validación |
| **G5** | LC-UI, según la semántica del ADR | La suite de UI deja de ser obligatoria en local en cada iteración | **No se extiende al núcleo.** Ocurre **después** de G2 |
| **G6** | Reutilización de evidencia por SHA exacto | Primer valor de «suites completas por Candidato» | Ni equivalencia entre SHA, ni clave de estado, ni igualdad de árbol |
| **G7** | Conformidad final y cierre documental | Verificación contra el ADR, criterios de reapertura y registro de lo aplazado en `ideas-futuras.md` | **Ya no crea el ADR principal** |

### Notas vinculantes por gate

**G1.** La métrica de sesiones y entregas queda **únicamente** como *resultado descriptivo de
calendario*. Quedan retiradas las afirmaciones «50×», «167×» y «el trabajo activo es casi invariante».
El método histórico corregido usa el rango `<merge>^1..<merge>^2`, `--no-merges` y la marca de tiempo de
**autoría**, con análisis de sensibilidad al umbral y marca de los rangos reescritos por rebase.

**G6.** El control prospectivo tiene un propósito y solo uno:

```
mismo SHA exacto + resultado contradictorio  =>  SEÑAL DE NO DETERMINISMO  =>  vía de diagnóstico de G2
```

No es «validación de equivalencia». El control retrospectivo de la clave retirada queda registrado como
*sin contradicción observada, pero sin poder discriminante sobre este corpus*, y **no habilita nada**.

## Métricas

De las catorce acordadas, cuatro **no tienen baseline hoy y lo declaran**. Una métrica sin baseline y
sin efecto mínimo detectable es infalsable, y el suelo de ruido medido es de un factor dos.

La métrica de tiempo hasta la primera evidencia relevante **ya se cumple** con la arquitectura actual y
se conserva solo como higiene: el plan no puede reclamarla como resultado.

## Lo que este plan declara NO resuelto

> Instantánea de V4, en el momento del consenso. Los gates posteriores han cerrado varios de estos
> puntos; donde así sea, se anota debajo. La lista no se borra: es el registro de lo que estaba
> abierto cuando se acordó el plan.

1. El determinismo de la suite de UI, con una condición de carrera **viva** sobre un delegado estático
   de proceso. — **G0B cerró esa carrera concreta**: las dos únicas clases que mutan ese delegado ya
   no pueden ejecutarse concurrentemente. El determinismo **global** de la suite de UI sigue sin
   demostrarse, y este punto no lo declara resuelto.
2. El trabajo serializado real del hilo STA.
3. La duración del ciclo local obligatorio, **denominador de todo ahorro** que se reclame.
4. El recall de cualquier selección, que **no es acotable** desde este repositorio y no lo será por
   acumulación de tiempo.
5. La ambigüedad normativa sobre si «todas las pruebas» incluye la suite de UI. — **CERRADO en G0A**:
   `AGENTS.md` fija que «Full» es **Core + UI**, y **G5** añade el reparto local/CI sobre esa
   definición. No se re-litiga.
6. La contradicción entre el requisito de fila en el ROADMAP al abrir una iniciativa y la regla de que
   el ROADMAP se edita solo al integrar. Se resuelve en **G0A**.

## Invariantes de seguridad

Vinculantes para este plan y para cualquier versión posterior: no borrar pruebas por lentitud; no
permitir un filtro que seleccione cero pruebas sin fallo ruidoso; no esconder fallos; no aumentar
omisiones para tener verde; no sustituir pruebas de comportamiento por guardas de código fuente; suite
completa nunca opcional en cambios críticos; el baseline de un Golden nunca se actualiza solo; no
eliminar la validación del dueño donde solo AutoCAD real puede validar; **no vender ningún ahorro no
medido**; no reutilizar evidencia tras un cambio relevante.
