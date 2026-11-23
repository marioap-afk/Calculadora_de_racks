# I-64 F4B — Paquete de PB-T5: CI exacta, Smoke-PB y gate-close de F4B (v2.3)

> **Redacción para el repo público:** las rutas locales van como marcadores, salvo la carpeta de trabajo del Smoke-PB, `<raiz-de-trabajo>\`, que fijó la sesión principal. Al custodiar, `<carpeta-del-Smoke-PB>` pasa a `<carpeta-del-Smoke-PB>` y `<carpeta-de-respaldo>` a `<carpeta-de-respaldo>`, como las copias exportadas `r71…` pasaron a `<copia>` (F4B-E60). No hay nombres personales ni de clientes. La hora del Owner, en UTC; su zona horaria, como `<zona-horaria-del-Owner>`.

**Estado:** v2.3. **Aplica las siete decisiones del Owner** de `owner-pbt5-decisions.md` ([OD5]), literal. Es la versión que pasa la comprobación acotada que exige la emisión condicional (Q-PBT5-7). **No está emitido.**

**Base y método:**
- Todo se lee por objetos git en `7da361eb`. No se leyó el worktree. No se compiló, no se ejecutó ninguna prueba y no se abrió AutoCAD.
- **[T4e]:** el cierre de PB-T4e (F4B-E61, §291), su SHA, que es el Candidato, y su CI. Los rellena la sesión principal con el SHA del cierre.

**Los documentos de PB-T5:**
- **este paquete;**
- **`smoke-run-v1.md`:** la guía operativa del Smoke-PB contratado, con las órdenes exactas;
- **`_tools/emit_pbt5.py`:** el guion de la emisión, con `--dry` y `--commit`, que corre la sesión;
- **`owner-pbt5-decisions.md`:** el registro literal de las decisiones. Al custodiar, va a `F4B-DRAFTS/coordinator/`;
- **la historia:** las versiones v1 a v2.2 del paquete y de las preguntas, la revisión independiente y sus cuatro recomprobaciones (`review-v1/`), las guías de la corrida previa (`pre-run-tonight-v2.md` a `v2.4.md`), que no llegó a hacerse, y los traspasos.

**Abreviaturas:**
- **[F]:** el paquete de la fase, `docs/initiatives/I-64-f4b-push-back-completo-package.md` v3.1;
- **[DISP]:** `docs/automation/evidence/I-64-pilot/F4B-DRAFTS/f4b-dispositions.md`;
- **[T3] y [T4]:** los paquetes de PB-T3 y de PB-T4 (v5.6);
- **[G]:** la guía `docs/guias/validacion-manual-autocad.md`, que **no se modifica**;
- **[S2]:** Smoke-2 de F4 (`I-64-pilot/F4-SMOKE-2/`, `F4-DRAFTS/f4-dispositions.md` F4-E59 y F4-E71 a F4-E81, `F4-DRAFTS/coordinator/`, `F4-DRAFTS/gate-close.md`);
- **[BM]:** el bloque M de PB-T0;
- **[IR1]:** la revisión independiente del paquete, con sus recomprobaciones;
- **[OD5]:** `owner-pbt5-decisions.md`;
- **[SR]:** `smoke-run-v1.md`.

---

## Registro de cambios v2.2 → v2.3

| Decisión del Owner ([OD5]) | Qué cambia en la v2.3 |
|---|---|
| **Q-PBT5-7, «Sí, con condiciones»** | La emisión, en §6.7: sin volver al Owner si se cumplen tres condiciones (la comprobación acotada de esta v2.3 da 0 bloqueantes y 0 importantes sin resolver; no aparece ninguna decisión material nueva; el Candidato tiene la CI exacta en verde). Vuelve por una decisión nueva, una prueba que falle o un intento 3 |
| **Q-PBT5-8, «Adaptación con revisión»** | El contrato de §6 en su forma B: propio (invariantes, paradas, evidencia y presupuesto), sin Controller, sin nc1 a nc4 y sin D2. **Antes de la firma, una revisión independiente de solo lectura** comprueba que el registro, las capturas y los inventarios coinciden. La regla de reejecutar la CI queda confirmada (PBT5-C15) |
| **Q-PBT5-9, «Definición y reglas»** | La definición de intento (§6.6) y las tres reglas de Smoke-2, adoptadas: las etiquetas, el protocolo OBSERVADO y una copia por escenario (§4.0) |
| **Q-PBT5-1, «Extender la dispensa»** | La CI exacta y los filtros locales, sin la UI completa en local (§3.1, P-02 e INV-PBT5-15) |
| **Q-PBT5-10, «Hacerlas como en Smoke-2»** | SPB-59 a SPB-61 entran: un AutoCAD por inyección, con la variable solo en el entorno de ese proceso (§5.5; [SR] §4.11) |
| **Q-PBT5-4, «Construirlos y rotularlos»** | SPB-45, 46, 57 y 58 entran: en copias, con órdenes nativas tecleadas, rotulados y comparados con RACKEDITAR en una gemela (§4.0; [SR] §4.12) |
| **Q-PBT5-2, «Contar antes en otros»** | La precondición P-09 nueva: el recuento en los demás dibujos de la selección de Smoke-2 antes de SPB-07. Si ninguno guarda un estilo, SPB-07 vuelve al Owner (§3.3, condición 3) |
| **Q-PBT5-5, no se preguntó** | La corrida previa no llegó a hacerse. Sus guías quedan como historia. La frontera de esta noche, para el Smoke-PB contratado, es la lectura de la sesión principal (§5.4) |

**Lo demás:**
- de §4 no cambia nada fuera de lo de la tabla;
- §5 y §6 se reescriben con las decisiones;
- las correcciones de la guía de la corrida previa (I-07 a I-09 y M-18 a M-35) pasan a [SR].

---

## 0. En una página

- **Qué es PB-T5:** la última tarea de F4B: «CI exacta, Smoke-PB (D-PB-09) y gate-close de F4B» ([F] §5). Empieza con el cierre de PB-T4e **[T4e]**.
- **No tiene Worker de código.** El contrato es la forma B que eligió el Owner (Q-PBT5-8): sin Controller, sin nc1 a nc4 y sin D2, con una revisión independiente de la evidencia antes de su firma.
- **La emisión** es condicional (Q-PBT5-7), por el guion `_tools/emit_pbt5.py`. Es la entrada F4B-E62 y la sección 292 de la evidencia.
- **La CI exacta:** el Candidato es la punta del cierre de PB-T4e **[T4e]**. Lleva la CI de push 4/4 sobre ese SHA y los totales del GREEN de PB-T4e, la suite de Core en local y los 19 filtros de UI, sin la UI completa en local (Q-PBT5-1).
- **El Smoke-PB:** 65 pasos en 12 bloques, más 9 precondiciones.
  - 56 los mide la sesión; 9 necesitan además el juicio visual del Owner (S+O);
  - los A/B y los cuatro contextos construidos van rotulados **CONSTRUCTED REPRESENTATIVE TEST CASES**;
  - lo demás, sobre racks reales, en copias.
- **Quién opera:** la sesión, si puede empezar antes de las 13:00 UTC; a las 13:00 UTC para en el siguiente límite de paso y espera al Owner. Después, con el Owner. Los juicios S+O, la revisión de la evidencia y la firma no los hace la sesión.
- **El gate-close:** tras la revisión independiente de la evidencia y la firma del Owner, como F4-E81. No cambian `main`, HANDOFF, ROADMAP ni [G].

---

## 1. Autoridades

| Autoridad | Qué fija para PB-T5 |
|---|---|
| **[OD5]** | La emisión (Q-PBT5-7), la forma del contrato y la regla de la CI (Q-PBT5-8), el intento y las reglas de Smoke-2 (Q-PBT5-9), la dispensa de la UI (Q-PBT5-1), las inyecciones (Q-PBT5-10), los contextos construidos (Q-PBT5-4) y el estilo de cotas (Q-PBT5-2) |
| [F] §5, fila PB-T5 | El alcance |
| [F] §5, «Cada tarea pasa por la cadena de F4…», y §6.5 | La cadena, adaptada en la forma B que eligió el Owner |
| [F] D-PB-09 (OB-09.1 y OB-09.2) | Los A/B fabricados y rotulados; racks reales donde existen; copias; originales sin abrir |
| [F] D-PB-10 | 1/3 por tarea; el intento 2 lo decide la sesión; el 3 vuelve al Owner |
| [F] §6.3 | Las reglas de operación, NETLOAD con «Load Once», los inventarios de solo lectura y «No se finge una condición interna» |
| [F] §8 y §9 | Los riesgos que remiten al Smoke-PB y los criterios de cierre |
| [T3] §5.5, §5.6 y A-T3-5 | Lo físico del escritor completo |
| [T4] §5.5 y §10 | La lista del Smoke-PB de PB-T4 y lo que queda para PB-T5 |
| [DISP] F4B-E06, E08, E09, E12, E23, E48, E51, E53, E55, E58, E59 y E60 | Lo que cada tarea dejó al Smoke-PB (§4.5) |
| Los cierres de PB-TC (`:65`), PB-T3c (`:53` y `:56`) y PB-T4b (`:59`); la revisión de PB-T4a (`:207-215`); el paquete de PB-T2 (`:1329`) | Lo dejado al Smoke-PB fuera de [DISP] |
| [S2] F4-E59 | La regla de la UI en local con contención de perfil, que la dispensa extendida respeta |
| [S2] F4-E71 a F4-E81 | El precedente de un smoke de fase y su cierre por la firma |
| [BM] | El precedente de operación de la sesión en F4B |
| [G]; WORKFLOW §4 y §6; AGENTS | El entorno, el DLL del worktree, la carga, el Candidato y la reutilización por SHA exacto |

---

## 2. Hechos de partida

**Medidos en `7da361eb`:**
- 16 tareas de F4B están cerradas; PB-T4e cierra ahora **[T4e]**;
- los totales esperados en la CI del GREEN de PB-T4e: Core 19 421 = 19 018 + 0 + 403; UI 2 465 = 2 417 + 0 + 48 (F4B-E60). **[T4e]:** si el cierre registra otros, mandan los suyos;
- `I-64.yml`: `current_phase: F4B`, `state: implementing`, `gate: none`, `attempts: 1`;
- **la numeración:** tras el cierre de PB-T4e, [DISP] llega a F4B-E61 y la evidencia a la §291. La emisión de PB-T5 es **F4B-E62** con la **§292**. El guion lo comprueba (§6.7).

**Los dibujos:**
- D1 (R006), con 14 sobres Push Back de un sentido, sin `DimensionStyle`, y una referencia anidada de una planta en un bloque pegado;
- D2 (R053), sin referencias de nivel superior;
- ningún A/B real con referencias;
- los demás dibujos reales con racks Push Back de la selección de Smoke-2, donde nadie ha contado el estilo de cotas (Q-PBT5-2).

**Lo de la carga, la contención de perfil, el diálogo de la biblioteca y los literales:** como en la v2.2 (§2), sin cambios.

**La corrida previa a la emisión no se hizo:** PB-T4e no cerró a tiempo ([OD5], «El origen»).

---

## 3. El alcance de PB-T5

### 3.1 La CI exacta del Candidato

- **El Candidato** es la punta del cierre de PB-T4e **[T4e]**: el SHA exacto desde el que se compila el DLL.
- **El orden** que exige [G] §7.2 (la `InformationalVersion` del DLL identifica el Candidato):
  1. la comprobación del Candidato y el build, con el worktree en el Candidato;
  2. después, el commit documental de la emisión (F4B-E62), encima.

  El DLL compilado en el Candidato no se vuelve a compilar tras ese commit: si se recompilara, la versión cambiaría, y los bloques de [SR] pararían.
- **El producto** es el último commit que toca `src/` o `tests/` **[T4e]**. `git diff <producto> <Candidato> -- src tests assets` vacío.
- **La CI exacta:** `event = push`, `ref = refs/heads/architecture/workspace-persistente-rackcad` y `head_sha` = Candidato, con los cuatro trabajos en `success` y los totales de §2.
- **La regla de reejecutar, confirmada por el Owner (Q-PBT5-8):** si la CI sale en rojo solo por el detector de cuelgue en una prueba lenta conocida, se reejecutan una vez los trabajos fallidos sobre el mismo SHA; otra causa, o un segundo rojo, vuelve al Owner.
- **La evidencia local:** la suite completa de Core en local; los 19 filtros de UI de los `RequiredTests` del contrato de PB-T4e, cada uno en su proceso; y los builds Debug de UI y del Plugin.
- **La dispensa (Q-PBT5-1, «Extender la dispensa»):** sin la UI completa en local, como en Smoke-2 (DV-05 / O-08). No se levanta la regla de F4-E59. La validación final de la iniciativa sí la exigirá. El bloque del Candidato de [G] §7.1 lo escribe así: «UI Full local: dispensada por el Owner para el Smoke-PB de F4B (Q-PBT5-1)».

### 3.2 El Smoke-PB

- **Cubre** lo que solo se ve en AutoCAD:
  - lo que PB-T3, PB-T4 y sus subtareas dejaron «sin AutoCAD»;
  - las correcciones declaradas de RACKEDITAR;
  - los A/B fabricados;
  - y, por las decisiones del Owner, las inyecciones y los cuatro contextos construidos.
- **No es** Owner Validation ni la validación total de [G] §4.

### 3.3 El gate-close de F4B

**Condiciones** ([F] §9):
1. **18 tareas cerradas:** las 16, PB-T4e **[T4e]** y PB-T5, que cierra con el gate.
2. **`superseding-characterization.md`** con todas sus secciones en EJECUTADA, también «## PB-T4e» **[T4e]**.
3. **El Smoke-PB verifica en RACKEDITAR D-PB-06 y D-PB-08** (SPB-06 a SPB-08).
   - Para el estilo de D-PB-06 (b), primero el recuento de P-09 (Q-PBT5-2).
   - Si ningún dibujo real guarda un estilo, **vuelve al Owner,** que decide si acepta el criterio cumplido en parte.
4. **IC-06.**
5. **Los INV del §1 de [F],** con su tabla INV → tarea y prueba en `gate-close.md`.
6. **La CI exacta del Candidato** y la evidencia local de §3.1, con la dispensa de la UI.
7. **Smoke-PB PASS,** o PASS WITH LIMITATIONS con las limitaciones aceptadas por el Owner.
8. **La revisión independiente de la evidencia** (Q-PBT5-8) y, después, **la firma del Owner.**
9. **Sin integración parcial en `main`.**

**Lo que cambia y lo que no:** como en la v2.2 (§3.3), con la revisión de la evidencia en `F4B-DRAFTS/pbt5/` y su resultado en la entrada de [DISP] de la disposición del Owner.

**La redacción del cierre:**
- «F4B CLOSED / PASS WITH LIMITATIONS» o «CLOSED / PASS»;
- «No es Owner Validation y no integra en `main`»;
- desde ese momento, «editor de Push Back migrado» (IC-06 e IC-11);
- F5-A deja de esperar.

---

