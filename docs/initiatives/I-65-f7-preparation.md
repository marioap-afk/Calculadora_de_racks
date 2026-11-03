# I-65 — F7: criterios de aceptación, presupuesto y plan

- **Autoridad:** orden del Owner «F5 Gate Acceptance / F7 Authorization» ([decisiones](../automation/decisions/I-65.md) §99), fila F7 del
  Freeze ([Proposal V3.1](I-65-proposal-v3.1.md) §6: «Conformidad, regresión, entrada de FOUNDATIONS redactada, guía de uso, ADR-0050»),
  [INITIATIVE_LIFECYCLE](../INITIATIVE_LIFECYCLE.md) §4.1, §7 y §8, [AGENTS.md](../../AGENTS.md) («Pruebas — definición de terminado») y
  [WORKFLOW](../WORKFLOW.md) §8 y §11.4.
- **Base:** F5 cerrada con GATE PASS sobre `45aaad6e` (registro `e7c1fcf1`). Producto = árbol de `51fc82f1` (Candidato de hito F5
  `bd1fcf8a`). F7 no cambia producto.
- Este documento fija los criterios y el presupuesto **antes** de ejecutar F7, como exige la orden del Owner.

## 1. Alcance

F7 comprende solo lo que nombra el Freeze y la orden del Owner:

| Actividad | Entregable | Ubicación |
|---|---|---|
| Revisión de conformidad | matriz del Coordinator contra el Freeze (D-01..D-15, §3, §4, §5, §6, §7, §9) y ADR-0050; revisión independiente del Architect | `docs/initiatives/I-65-f7-conformance-review.md`; `docs/automation/evidence/I-65-f7/` |
| Regresión Full | Core Full y UI Full locales, builds Debug de UI, Plugin y herramienta; CI del SHA exacto | evidencia de cierre de F7 |
| Entrada de FOUNDATIONS | entrada factual **redactada** con el esquema de `docs/FOUNDATIONS.md` para lo que el Freeze §7 declara en `Introduces` y `Extends`, verificada contra el código | `docs/initiatives/I-65-foundations-entry.md` |
| Guía de uso | guía del flujo `RACKRAMPAQUETE` → script en RAM → lectura de vuelta → volcado → comparador, con sus límites; fila del comando en el README (WORKFLOW §8: «Cambian comandos de AutoCAD … → README.md, en la misma rama») | `docs/guias/interoperabilidad-ram-elements.md`; `README.md` |
| Verificación de ADR-0050 | estado, cláusulas y decisiones contra el código y la evidencia, sin editar el ADR | dentro de la revisión de conformidad |
| Evidencia y documentación de cierre | decisiones, evidencia, estado y plan de READY final | `docs/automation/`; `docs/initiatives/I-65-ready-final-plan.md` |

**Fuera de F7:** funciones nuevas, cambios de arquitectura, ampliaciones del contrato estructural, cambios en `src/`, `tests/`, `tools/`,
`assets/`, archivos de build, Freeze, ADR-0050 o tolerancias; AutoCAD y RAM; Codex y `attempts`; Workers; READY e integración.

**Publicación diferida (no es omisión):**

- La entrada **no** se escribe en `docs/FOUNDATIONS.md` en F7. LIFECYCLE §4.1 pide redactarla antes de READY-04 para que la revise la
  conformidad. WORKFLOW §11.4 publica las entradas «ya conformadas» en el commit de cierre de la integración. Además, una fundación solo es
  reutilizable integrada en `main`.
- El índice de ADR (`docs/adr/README.md`) no se edita hasta el cierre (CD-I65-F0-05).
- `HANDOFF.md` y `ROADMAP.md` solo se tocan al integrar (WORKFLOW §8).

## 2. Criterios de aceptación AC-F7-01..12

| ID | Criterio | Cómo se verifica |
|---|---|---|
| AC-F7-01 | Orden del Owner con alcance (F7 dentro del Freeze) y presupuesto (Codex 50/52, `attempts` 7/9, 0 Workers, sin cambios funcionales) | decisiones §99 |
| AC-F7-02 | Producto intacto: `src`, `tests`, `tools`, `assets`, `Directory.Build.*`, `global.json` y `RackCad.sln` iguales a `bd1fcf8a` (y a `51fc82f1`); cambios solo en `docs/` y `README.md`; Freeze blob `4e6e1feb…` y ADR-0050 blob `c18def42…` intactos; sin A-n | `git diff --name-only` y `rev-parse` de los árboles en el SHA de cierre |
| AC-F7-03 | **Regresión Full** sobre el SHA de cierre de F7, con el árbol limpio antes y después: Core Full local, UI Full local (omitidas declaradas), builds Debug de UI, Plugin y herramienta con 0 errores; CI `push` con `head_sha` exacto y 4/4 jobs en `success` | registro local y resumen técnico del CI |
| AC-F7-04 | **ADR-0050 verificado:** estado «aceptado», cláusula de compatibilidad (ADR-0005, ADR-0017, ADR-0021 §10, ADR-0040 D3) y decisiones contra el código y la evidencia; sin EXP-01 de clase A | revisión de conformidad §ADR |
| AC-F7-05 | **Freeze verificado:** D-01..D-15, invariantes §3, matriz §4, OV §5 (F4 y F5 cerrados; READY pendiente), gates §6 (F1, F3, F4 y F5 con GATE PASS; F6 fuera de V1) y no-objetivos §9, cada uno con su evidencia o con la limitación declarada | revisión de conformidad |
| AC-F7-06 | **Entrada de FOUNDATIONS redactada** con los diez campos del esquema, factual y verificada contra el código (símbolos, persistencia, contrato de mutación, punto de extensión, pruebas que la protegen, limitaciones); sin publicar en `docs/FOUNDATIONS.md` | revisión del Architect |
| AC-F7-07 | **Guía de uso** completa y fiel al código: requisitos, comando y diagnósticos, contenido del paquete, ejecución del script, lectura de vuelta, volcado, comparador y sus filas, referencia manual, paradas y límites (equivalencia ≠ validación de ingeniería, Q-O3); fila del comando en el README | contraste con `RackRamElementsCommands.cs`, la herramienta y los procedimientos versionados |
| AC-F7-08 | **Hallazgos y limitaciones:** A65-F4F-04, A65-F4F-08, A65-F5F-03/04/05/08 y C-5 conservados y evaluados para READY sin convertir OPTIONAL en REQUIRED sin base contractual; hallazgos nuevos registrados | plan de READY final; decisiones |
| AC-F7-09 | **Plan de READY final** (propuesta, no declaración): Candidato final propio, repetición de OV-I65-01..07, OV-I65-08, Architect, disposición de hallazgos materiales, conflicto con I-64, presupuesto de Codex e intentos, en el orden READY-01..09 | `I-65-ready-final-plan.md` |
| AC-F7-10 | **Custodia:** filas de custodia comparadas con los blobs antes de cada commit de evidencia; sin sobrescribir ni eliminar evidencia histórica; privacidad conforme a §90 | `verify_custody_rows.py` y barrido de secretos |
| AC-F7-11 | **Conformidad:** Architect sin REQUIRED abiertos y Coordinator CONFORMING sobre la evidencia de F7 | informes versionados |
| AC-F7-12 | **Orden de cierre (LIFECYCLE §7):** commit de cierre → árbol limpio → regresión Full local → push → CI → GATE PASS registrado después | evidencia de cierre |

## 3. Presupuesto

| Recurso | F7 |
|---|---|
| Codex | 0 (queda 50 de 52) |
| `attempts` | 0 (queda 7 de 9) |
| Workers | 0 |
| AutoCAD y RAM | 0 |
| Architect | 1 revisión independiente de solo lectura (+1 relectura solo si surge un REQUIRED corregible dentro del alcance documental) |
| Regresión local | Core Full (~5 min) + UI Full (~5 min) + 3 builds Debug, una vez sobre el SHA de cierre |
| CI | una corrida por push (lectura de cada una) |

## 4. Condiciones de STOP

Detenerse y pedir autorización si:

- alguna actividad exige un cambio funcional (código, pruebas, catálogos, tabla o herramientas);
- se necesita una decisión de arquitectura nueva, o modificar el Freeze, ADR-0050 o las tolerancias;
- se necesita Codex, `attempts` o un Worker;
- la verificación encuentra una EXP-01 de clase A (código, ADR-0050, Freeze o FOUNDATIONS que se contradicen en algo consumido);
- la regresión Full falla (se diagnostica con logs, sin corregir producto);
- aparece un secreto o un dato de terceros que no se puede redactar.

## 5. Observación previa

La ayuda integrada (`RACKAYUDA`, lista `RackCommandReference` del proyecto UI) no incluye `RACKRAMPAQUETE`. El Freeze no lo exige: D-13
define un comando de línea mínimo y prohíbe WPF y la edición de menús en el comando. Añadirlo a la ayuda es un cambio de código fuera de
F7. Se registra como hallazgo para la decisión del Owner y no se corrige. La fila del README es documentación y entra en la guía de uso.

## 6. Orden de ejecución

1. Registro de la orden y de este documento (sin ejecutar F7).
2. Entregables documentales: revisión de conformidad, entrada de FOUNDATIONS, guía de uso y README, y plan de READY final.
3. Revisión independiente del Architect (solo lectura) y conformidad del Coordinator.
4. **Commit documental de cierre de F7** → árbol limpio → regresión Full local → push → CI del SHA exacto.
5. GATE PASS de F7 registrado después, solo si todo queda en verde; si no, NO-GO con el motivo exacto.
6. Detenerse antes de READY final.
