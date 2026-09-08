---
schema: rackcad-initiative/v1
id: I-47
title: "Variables de proyecto: fundacion de autoridad drawing-level"
type: architecture
status: in-progress
branch: architecture/project-variables-foundation
base_branch: main
priority:
size:
depends_on: []
conflicts_with: []
context_packs: [architecture-kernel, persistence, autocad-plugin]
automation_state_path:
decision_paths: [docs/automation/decisions/I-47.md]
requires_ci: true
requires_plugin_build: false
requires_autocad: false
requires_owner_decision: true
requires_owner_validation: false
automation:
  enabled: false
  auto_merge: false
  max_attempts: 3
---

# Variables de proyecto: fundacion de autoridad drawing-level (ID22A)

> **Fase actual: DISCOVERY.** Este contrato nace en el bootstrap inmediatamente posterior al reclamo
> atomico, conforme al caso (d) de [WORKFLOW](../WORKFLOW.md) seccion 2: el dueno autorizo la
> iniciativa explicitamente y todavia no tenia fila en [ROADMAP.md](../ROADMAP.md); la fila se crea
> con este mismo commit, que es el orden que manda el proceso.
>
> ```
> Reclamo atomico:  6e17bd5   (commit vacio, Claim-Id 73672933-f052-46af-b3c0-8091f0add299)
> Base:             origin/main 306e18ed4676e5e96b54d59402c9a230efb137d3
> Fase:             DISCOVERY — sin cambios de produccion
> ```
>
> **Lo unico autorizado hoy es el Discovery.** El encargo del dueno delimita esta primera sesion a
> **investigacion sin cambios de produccion** y excluye expresamente dos disenos: **formulas** y
> **propagacion rack a rack**. Este contrato **no** declara un alcance de implementacion, porque el
> dueno no lo ha autorizado: lo que el Discovery encuentre alimenta una propuesta posterior, y un
> hallazgo **no** es una autorizacion. El registro literal de la autorizacion vive en
> [`docs/automation/decisions/I-47.md`](../automation/decisions/I-47.md).

## 1. Objetivo

Establecer si RackCad puede sostener **un unico registro `ProjectVariables` por DWG** —una autoridad
de **nivel dibujo**, no de nivel rack ni de nivel maquina— y que consecuencias tendria sobre los
consumidores que hoy leen esos valores desde otra parte.

«Variable de proyecto» significa aqui un valor que **todos los racks de un mismo dibujo comparten por
decision del proyecto**, no por coincidencia de configuracion. La pregunta que abre la iniciativa no
es como calcular con esos valores, sino **donde viven, quien manda cuando discrepan y que pasa con lo
ya dibujado cuando uno cambia**.

## 2. Preguntas del Discovery

Las tres que fija el encargo, y ninguna mas. Cada una se responde con evidencia del arbol —ruta,
simbolo y cita corta—, no con una descripcion plausible.

### D1 — Autoridad drawing-level disponible

Confirmar que mecanismo existente en el codigo permite guardar **exactamente un** registro por
dibujo, y con que garantias: como se nombra, quien lo posee, si sobrevive al guardado y reapertura
del DWG, si sobrevive a WBLOCK/copia entre dibujos, y que ocurre cuando el registro **no existe**
(dibujo heredado). Interesa especialmente distinguir lo que hoy cuelga del **dibujo** de lo que
cuelga de un **bloque o una referencia de bloque**: solo lo primero puede ser autoridad de proyecto.

### D2 — Descubrimiento y redibujo de consumidores

Auditar la maquinaria que ya existe para **encontrar los racks de un dibujo** y para **redibujarlos**.
Un valor de proyecto que cambia solo sirve si lo ya dibujado puede reaccionar; el Discovery no
disena esa reaccion, pero si establece si el precedente existe, cual es y que limites conocidos
arrastra.

### D3 — Vertical slice de tres variables

Comparar `VerticalClearance`, `PalletTolerance` y `PalletDepth` **como muestra representativa**: donde
se declara cada una, quien la consume, en que capa vive, si esta duplicada por sistema, si viaja en
los DTO de persistencia y con que fallback legado. Tres variables elegidas por el dueno porque se
espera que **no se comporten igual entre si**: el valor del Discovery esta en las diferencias, no en
el promedio.

## 3. Fuera de alcance — explicito

- **Cambios de produccion de cualquier tipo.** Ni `src/`, ni `assets/`, ni `tests/`.
- **Diseno de formulas.** No se define como se combinan estas variables ni que aritmetica las
  consume.
- **Propagacion rack a rack.** No se disena que un rack herede, copie ni imponga valores a otro.
- **Decidir la autoridad.** El Discovery levanta el mapa; **elegir** el mecanismo es decision del
  dueno y, por su alcance, probablemente un ADR.
- **Migracion de dibujos existentes**, formato de persistencia nuevo, UI, comando nuevo y BOM.
- Cualquier optimizacion o correccion «de paso» que el propio Discovery destape: se registra en
  [ideas-futuras.md](../ideas-futuras.md), no se arregla.

## 4. Restricciones que ya rigen y no se reabren

- **Direccion de dependencias** (AGENTS, convencion 1): nada de AutoCAD fuera del Plugin. Una
  autoridad de nivel dibujo que se implemente algun dia tendra su **lectura y escritura en el
  Plugin** y su **modelo en Domain/Application**; el Discovery debe reportar contra esa frontera.
- **Regla en un solo sitio** (AGENTS, convencion 2): si dibujo, BOM y UI deben coincidir en un
  numero, la regla vive en UNA funcion de Application. Toda duplicacion que el Discovery encuentre se
  reporta como hallazgo, no se corrige aqui.
- **Persistencia versionada** (AGENTS, convencion 4): todo campo nuevo nace nullable con fallback
  legado y test de round-trip. El Discovery debe decir si las tres variables de D3 cumplen eso hoy.
- La **pulgada** es la unidad geometrica interna ([ADR-0005](../adr/0005-estrategia-de-unidades.md));
  ninguna conversion se introduce aqui.

## 5. Entregables

### 5.1 Fase DISCOVERY — **entregada** ([I-47-discovery.md](I-47-discovery.md))

1. Respuesta a D1, D2 y D3 con rutas y simbolos reales verificados contra el arbol.
2. **Riesgos** y ambiguedades encontrados, cada uno atribuido a evidencia.
3. Las **preguntas que quedan para el dueno**, separadas de los hechos.

### 5.2 Fase PROPOSAL — autorizada en el **Gate C** (2026-09-08)

El dueno fijo **seis decisiones vinculantes** (C-1..C-6, registro en
[`docs/automation/decisions/I-47.md`](../automation/decisions/I-47.md)) y autorizo una
**Proposal documental**: [I-47-proposal-v3.md](I-47-proposal-v3.md) (**V3**, Gate C3). La
[V1](I-47-proposal-v1.md) y la [V2](I-47-proposal-v2.md) quedan **supersedidas** y se conservan solo
como registro.

- **Sigue sin autorizarse la implementacion.** La Proposal **compara alternativas y recomienda**; no
  toca `src/` ni `tests/`, y el gate `owner-decision` sigue abierto sobre el contrato que recomienda.
- Las seis decisiones son **premisas** de la Proposal, no opciones a comparar.
- **No se pidio Architect Review** en este gate.

## 6. Gates

| Gate | Estado | Motivo |
|---|---|---|
| `owner-decision` | **abierto** | El alcance posterior al Discovery no esta autorizado |
| `owner-validation` | no aplica en DISCOVERY | Sin cambios de produccion no hay dibujo que validar |
| `autocad` | no aplica en DISCOVERY | `requires_autocad: false` mientras no se toque produccion |
| CI | aplica | Toda punta empujada de la rama se mide como cualquier otra |

Si una fase posterior tocara colocacion, dibujo o BOM, `requires_autocad` y `requires_owner_validation`
**cambian en el mismo commit que introduzca ese cambio**, no despues.

## 7. Bitacora

| Fecha | Hito |
|---|---|
| 2026-09-08 | Reclamo atomico `6e17bd5` aceptado por el remoto; bootstrap (contrato + fila en ROADMAP + registro de autorizacion) |
| 2026-09-08 | Discovery entregado (`9e25d52`): D1/D2/D3 con evidencia, 10 riesgos, 11 hallazgos fuera de alcance |
| 2026-09-08 | **Gate C**: el dueno fija C-1..C-6 y autoriza la Proposal V1 documental. WBLOCK/copia entre dibujos queda **diferido**. Sin Architect Review |
| 2026-09-08 | **Gate C2**: el dueno define **ID22B** (formulas) e **ID21** (refs a propiedades de racks) y fija C2-1..C2-9. **Proposal V2** sustituye a V1: corrige nueve puntos, dos de ellos invalidando afirmaciones de V1 (D-07 y D-13). Sin Architect Review |
| 2026-09-08 | **Gate C3**: el dueno **decide D-07 (F2, promocion pegajosa; F3 rechazada)** y **D-10 (F1)**, saca la biblioteca de bloques del lote de propagacion y corrige el Consensus Freeze conforme a WORKFLOW seccion 2. **Proposal V3** sustituye a V2. **No quedan decisiones de producto abiertas.** Sin Architect Review |
