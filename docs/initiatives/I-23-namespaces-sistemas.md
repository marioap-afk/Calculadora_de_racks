---
schema: rackcad-initiative/v1
id: I-23
title: Namespaces finales por sistema
type: refactor
status: implementing
branch: refactor/namespaces-sistemas
base_branch: main
priority:
size: M
depends_on: [I-08, I-15, I-16, I-20, I-21, I-22]
conflicts_with: [I-20, I-21, I-22, I-24, I-25, I-30, I-31, I-32, I-33, I-34, I-35]
context_packs:
  - architecture-kernel
  - system-selective
  - system-dynamic-flowbed
  - autocad-plugin
  - delivery-validation
automation_state_path: docs/automation/state/I-23.yml
decision_paths: []
requires_ci: true
requires_plugin_build: true
requires_autocad: false
requires_owner_decision: false
requires_owner_validation: false
automation:
  enabled: true
  auto_merge: false
  max_attempts: 3
---

# I-23 — Namespaces finales por sistema

## 1. Objetivo

Cerrar el hallazgo **E8** de la [auditoria 2026-07](../auditoria-arquitectura-2026-07.md): el namespace
`Systems` es plano y multi-sistema, con nombres fosiles que ya no describen lo que contienen. Al
terminar, cada sistema vive en su propio namespace y su propia carpeta, el `Shared` contiene solo lo
que es de verdad compartido, y una regla comprobable impide que el namespace y la ruta vuelvan a
divergir.

Es un refactor **mecanico**: mueve archivos y reescribe declaraciones de namespace y `using`. No
cambia una sola linea de logica.

I-23 **cierra la Fase 5** y depende de todas sus predecesoras (ROADMAP, Fase 5).

## 2. Problema

La auditoria lo enuncia asi:

> **E8** — Namespace `Systems` plano multi-sistema con nombres engañosos (`DynamicSystemPlan` lo usan
> todos; el selectivo consume constantes `DynamicRackDefaults.Separator*`).

Medido sobre la base `b43b5d1`:

- `RackCad.Application.Systems` concentra **101 archivos** de cinco sistemas distintos sin ninguna
  separacion fisica ni de namespace;
- `RackCad.Domain.Systems` (22) y `RackCad.Plugin.Systems` (12) repiten el mismo patron plano;
- `DynamicSystemPlan` **no es el plan del sistema dinamico**: es el plan de corridas de cabecera y lo
  consumen los cuatro sistemas (Selectivo, Dinamico, Push Back y cama). El nombre miente;
- `RackCad.Application.Headers` mezcla el vocabulario de materializacion compartido
  (`HeaderBlockInstance`, `LateralHeaderLayout`) con los builders de la cabecera fisica;
- no existe `.editorconfig`, asi que nada impide que el proximo archivo nazca en el lugar equivocado.

El costo es concreto: un lector no puede saber a que sistema pertenece un tipo sin abrirlo, y el
sistema N+1 hereda un namespace plano que ya tiene 101 archivos.

## 3. Alcance

Autorizado por el ROADMAP (fila I-23, Fase 5) y por el encargo:

1. **Separacion por sistema** de los tres namespaces `Systems` (Domain, Application y Plugin) en
   `Selective`, `Dynamic`, `PushBack`, `FlowBed`, `Larguero` y `Shared`. La ruta en disco acompaña al
   namespace.
2. **`RackFrames` se conserva** para la cabecera fisica, en Domain y en Application.
3. **`Application.Headers` se disuelve**: los artefactos de **materializacion** pasan a
   `Application.Drawing`; los builders de la **cabecera fisica** pasan a `Application.RackFrames`.
4. **Renombre fosil autorizado**: `DynamicSystemPlan` pasa a `Drawing.HeaderRunPlan`.
5. **`.editorconfig`** con reglas comprobables de namespace/ruta, mas **source guards focalizados**.
6. **Barrido de referentes** de las rutas movidas en la documentacion viva (WORKFLOW seccion 8).

### 3.1 Regla de clasificacion (objetiva y comprobable)

Un archivo pertenece al namespace del sistema que su tipo de primer nivel **nombra y modela**. Solo
hay tres excepciones, y cada una exige evidencia:

| Excepcion | Criterio | Destino |
|---|---|---|
| Neutral de nombre **y** de contenido | no nombra ningun sistema y no ramifica por ninguno | `Shared` |
| Vocabulario de materializacion | describe piezas/instancias/planos de bloque que consumen todos | `Drawing` |
| Nombre neutral con contenido de un sistema | el nombre no dice el sistema pero el contenido y sus llamadores si | ese sistema |

Consumir un contrato de otro sistema **no** mueve un tipo: la composicion entre sistemas es legal
(ROADMAP, principio 5). Por eso `SelectiveDesviadorPlan` permanece en `Selective` aunque el Dinamico y
Push Back lo consuman, y `FlowBedLateralBuilder` permanece en `FlowBed` aunque los dos lo compongan.

### 3.2 Por que `HeaderRunPlan` y no `SystemPlan`

El ROADMAP anota el renombre como `DynamicSystemPlan` a `SystemPlan`. **No se aplica ese nombre**:
en el arbol actual es ambiguo, porque `SystemPlan` sugiere "el plan de un sistema" y colisiona
conceptualmente con `SystemBomBuilder`, `SystemDescriptor`, `SystemRegistry` y `SystemBlockWriter`, que
si son por sistema.

La evidencia del tipo dice otra cosa: agrupa `HeaderGroup` (definiciones distintas de cabecera) con sus
`HeaderPlacement` (donde se coloca cada una a lo largo de la corrida) mas las instancias sueltas. Es el
**plan de corridas de cabecera**, y lo consumen los cuatro sistemas — exactamente lo que E8 señala. Se
renombra a **`HeaderRunPlan`** y se aloja en `Drawing` junto al vocabulario que produce y consume.

## 4. Fuera de alcance

**Congelacion funcional total.** No se cambia:

- logica, algoritmos ni geometria;
- firmas, salvo el namespace y el unico nombre autorizado (`DynamicSystemPlan` a `HeaderRunPlan`);
- accesibilidad de ningun miembro o tipo;
- wire format, JSON, enums persistidos, schemas, `SchemaVersion`, fallback legacy;
- GUID, identidad, metadata I-11, Xrecords;
- comandos de AutoCAD, aliases, prompts ni textos visibles;
- BOM, catalogos, bloques DWG, `assets/` ni `deploy/`.

Tampoco entra: **I-25** (guardas traseras, diferida), la **defensa** que I-34 dejo como candidato
independiente, el **preview visual** que I-18 difirio, `DesviadorCellsAreByPost`, ni ninguna de las
mejoras que siguen en [`../ideas-futuras.md`](../ideas-futuras.md). **Push Back v1 queda estable**: I-23
no lo toca funcionalmente.

`src/RackCad.Plugin/Headers/` **se conserva**: contiene el adaptador AutoCAD de la cabecera (drawers,
importador, renombrador), no vocabulario de materializacion compartido. Es el mismo motivo por el que
`RackFrames` se conserva.

`RackCad.UI` no se toca: sus namespaces (`UI`, `UI.Controls`, `UI.Editor`, `UI.Shell`, `UI.Preview`) ya
estan separados por responsabilidad y no son por sistema.

## 5. Contexto requerido

- [AGENTS.md](../../AGENTS.md), [WORKFLOW.md](../WORKFLOW.md), [ROADMAP.md](../ROADMAP.md) Fase 5,
  [HANDOFF.md](../HANDOFF.md), [ARCHITECTURE.md](../ARCHITECTURE.md) seccion 7.4.
- [Auditoria 2026-07](../auditoria-arquitectura-2026-07.md), hallazgo **E8** y recomendacion BAJA 16.
- Context Packs: `architecture-kernel`, `system-selective`, `system-dynamic-flowbed`,
  `autocad-plugin`, `delivery-validation`.
- [`guias/agregar-un-sistema.md`](../guias/agregar-un-sistema.md): el patron que estos namespaces deben
  dejar legible para el sistema N+1.
- ADR-0006 (AutoCAD solo en el Plugin) y ADR-0011 (patron ARRAY): fijan que el movimiento no puede
  cruzar capas ni alterar el agrupamiento de bloques.
- El inventario tipo por tipo de esta iniciativa:
  [`I-23-inventario-namespaces.md`](I-23-inventario-namespaces.md).

## 6. Dependencias

Integradas y verificadas en `main` = `b43b5d1`: **I-08, I-15, I-16, I-20, I-21, I-22**, y ademas
I-24, I-30, I-31, I-32, I-33, I-34 e I-35, que llegaron despues de escrita la fila del ROADMAP y
tambien mueven codigo dentro de `Systems`.

Conflicto activo: **ninguno**. Al reclamar, la unica rama en `origin` era `main`, asi que no habia
iniciativa funcional en curso. **I-25 queda diferida** y no debe reclamarse mientras I-23 este viva:
la fila del ROADMAP declara que I-23 se estorba con toda la Fase 5.

Entradas del dueño requeridas: **ninguna**. El ROADMAP no marca I-23 con validacion en AutoCAD.

## 7. Archivos esperados

142 archivos movidos con `git mv`, mas la reescritura de `namespace`/`using` en los archivos que los
consumen. Distribucion destino:

| Namespace destino | Archivos |
|---|---|
| `RackCad.Domain.Systems.{Selective,Dynamic,PushBack,FlowBed,Larguero,Shared}` | 4 / 7 / 4 / 3 / 1 / 3 |
| `RackCad.Application.Systems.{Selective,Dynamic,PushBack,FlowBed,Larguero,Shared}` | 28 / 34 / 26 / 2 / 1 / 9 |
| `RackCad.Application.Drawing` | 4 |
| `RackCad.Application.RackFrames` (recibe de `Headers`) | 4 |
| `RackCad.Plugin.Systems.{Selective,Dynamic,PushBack,FlowBed,Shared}` | 2 / 3 / 3 / 1 / 3 |

Se crean: `.editorconfig` y la guarda de namespace/ruta en `tests/RackCad.Tests`.

Se actualizan por barrido de rutas: los dos Context Packs por sistema, los ADR 0009/0011/0013/0014/0016
(enlaces), `guias/catalogos-y-plantillas.md`, `guias/generacion-cabecera-lateral.md` y las tres rutas
de codigo citadas en `AGENTS.md` y `WORKFLOW.md` seccion 7. `docs/archivo/` **no** se toca (es
historia) y `HANDOFF.md`/`ROADMAP.md` se tocan solo en la sesion de integracion (WORKFLOW 4.5.4).

Una desviacion material de este inventario obliga a detenerse.

## 8. Fases

1. **Reclamo** — rama, worktree, commit vacio con `Claim-Id`, push sin force. **Hecha** (`ed81d0a`).
2. **Contrato e inventario** — este documento y el inventario tipo por tipo con consumidores; captura
   de la baseline completa antes de mover nada.
3. **Movimiento** — `git mv` por frontera (Domain, Application, Plugin, `Headers`), reescritura de
   `namespace`/`using`, renombre autorizado. Un commit por frontera.
4. **Reglas** — `.editorconfig` y source guards focalizados de namespace/ruta.
5. **Verificacion** — suites completas, filtros dirigidos con conteo positivo, builds Debug de UI y
   Plugin, validador de catalogos, bundle y comparacion antes/despues.
6. **Publicacion** — revision del diff completo y push de la rama. **No** se integra.

## 9. Pruebas y builds

```powershell
dotnet test tests/RackCad.Tests/RackCad.Tests.csproj
dotnet test tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj
dotnet build src/RackCad.UI/RackCad.UI.csproj -c Debug
dotnet build src/RackCad.Plugin/RackCad.Plugin.csproj -c Debug
```

Baseline capturada sobre `b43b5d1` **antes** de mover: **1612** pruebas en `RackCad.Tests` y **491** en
`RackCad.UI.Tests`, cero fallos. El refactor no añade ni quita pruebas de comportamiento: los mismos
conteos, mas las guardas nuevas de namespace/ruta, deben seguir en verde.

Ademas: filtros dirigidos con **conteo positivo** (un filtro que corre cero pruebas no prueba nada), los
**7 goldens** de `tests/RackCad.Tests/Golden/`, el validador de catalogos de I-19 y la verificacion del
bundle de I-12. **Ningun golden debe cambiar**; si uno cambia, el refactor dejo de ser mecanico y hay
que detenerse.

AutoCAD debe estar **cerrado** durante los builds (bloqueo de DLL, trampa conocida de AGENTS.md).

## 10. Validacion manual

**No aplica** como gate: el ROADMAP no marca I-23 con validacion del dueño y la iniciativa no cambia
dibujo, BOM, catalogos ni comandos. La equivalencia se sostiene con los goldens, la comparacion
antes/despues de planes, BOM, serializacion, legacy, metadata, handlers y comandos, y los builds Debug.

El riesgo de runtime residual se declara en la evidencia final, no se oculta.

## 11. Criterios de aceptacion

1. Los tres namespaces `Systems` quedan separados por sistema; **ningun archivo** permanece en la raiz
   plana de `Systems`.
2. `Application.Headers` no existe; sus artefactos de materializacion estan en `Drawing` y sus builders
   de cabecera fisica en `RackFrames`.
3. `DynamicSystemPlan` ya no existe; `Drawing.HeaderRunPlan` ocupa su lugar con **la misma superficie
   publica** (mismos miembros, misma accesibilidad, mismo comportamiento).
4. `.editorconfig` y la guarda de namespace/ruta fallan si un archivo declara un namespace que no
   corresponde a su carpeta.
5. Suites completas verdes con los conteos de la baseline mas las guardas nuevas; goldens **identicos**;
   builds Debug de UI y Plugin sin errores propios; CI verde sobre la punta de la rama.
6. La comparacion antes/despues de planes, BOM, serializacion, legacy, metadata, handlers y comandos no
   muestra **ninguna** diferencia.
7. `git log --stat` de la rama muestra renombres (`R`), no borrados mas altas: el historial de cada
   archivo se conserva.

## 12. Condiciones para detenerse

- Un golden cambia, o la comparacion antes/despues muestra cualquier diferencia.
- Un movimiento exige tocar logica para compilar (señal de que el corte de namespace esta mal elegido).
- Aparece un consumidor que resuelve tipos por nombre completo, reflexion o discriminador de JSON.
- El trunk avanza: hay que rebasar antes de seguir.
- Alguien reclama I-25 u otra iniciativa de Fase 5 en `origin`.
- Se descubre que un archivo no encaja en la regla de clasificacion de la seccion 3.1 sin inventar una
  categoria nueva.

## 13. Estado versionado y entrega del Pull Request

Estado canonico en [`../automation/state/I-23.yml`](../automation/state/I-23.yml). No se abre Pull
Request: el flujo de este repo integra por `git merge --no-ff` en una sesion dedicada (WORKFLOW 4.5) y
el agente **nunca** hace merge. El merge automatico esta prohibido.

## 14. Evidencia final

Se completa al cerrar la sesion: SHA base, `Claim-Id`, inventario, mapa final, commits, diff por
frontera, pruebas, builds, CI y riesgos de runtime pendientes. `main` no se modifica en ningun momento.
