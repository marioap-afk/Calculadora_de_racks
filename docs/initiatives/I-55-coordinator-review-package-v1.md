# I-55 — Paquete de revision del Coordinador (Proposal V1)

```text
PROPOSAL V1 — NOT CONSENSUS
Coordinator    = REVIEW REQUIRED
Architect      = REVIEW REQUIRED
Consensus      = NOT REACHED
Implementation = BLOCKED

Objeto de la revision = docs/initiatives/I-55-proposal-v1.md @ d1918ab9a44ce7ae391aeacc10ff6686c8807a61
Misma version del plan = docs/initiatives/I-55-implementation-map-v1.md · docs/adr/0042-preparacion-de-vistas-antes-de-materializar.md
```

> **Que es este paquete.** Una guia para que el Coordinador de I-55 emita **su** veredicto sobre la Proposal V1. El ejecutor
> **no** emite ni simula veredictos, **no** declara consenso y **no** interpreta el silencio como acuerdo.

## 1. Veredicto que se solicita

```text
Coordinator: AGREED | AGREED WITH CHANGES | CHANGES REQUIRED
Hallazgos:   BLOCKER | HIGH | MEDIUM | LOW, cada uno con seccion de la Proposal y cambio exigido
Version:     el veredicto vale solo para el SHA exacto revisado
```

La implementacion sigue **bloqueada** hasta que Coordinador **y** Arquitecto esten **AGREED sobre la misma version**, el Owner
decida OD-1..OD-8 y acepte o rechace ADR-0042, y exista el registro de Consensus Freeze.

## 2. Trazabilidad

| Pieza | SHA |
|---|---|
| BASE original del reclamo (codigo auditado por el Discovery) | `ba497f14581d81e83a27514852d6ec082ff57635` |
| CURRENT_BASE y CURRENT_MAIN (re-fetch previo a la Proposal) | `dad4e77f4f267b9fa248ecb0c8bfd8a74bbab093` |
| Reclamo, bootstrap, Discovery y cierre de G1 (rebasados) | `6976262`, `5b5c4f6`, `2d0f9bf`, `717a38d` |
| G1.1 — correccion del framing | `cfdb702a83554df6472eddbdc59cf43a2ba7bd94` |
| **Proposal V1** + mapa + ADR-0042 + indice de ADR + registro | **`d1918ab9a44ce7ae391aeacc10ff6686c8807a61`** |
| Paralelas observadas | I-49 `bcbf570` (publico ADR-0041 en su rama; el de I-55 es 0042); I-52 `636f7fd` (Proposal V10) |

## 3. Fidelidad al contrato de producto (CD-01..CD-04)

| ID | Resultado exigido | Donde lo cumple la Proposal | Limites que el Coordinador debe aceptar o rechazar |
|---|---|---|---|
| ID17 | Empezar por cualquier vista realmente soportada; despues las demas sin perder `RackId`, authored ni coherencia | §5, §6, §8, §10 | «Soportada» = builder + codec/edicion + disponibilidad. Cama con una sola vista; Larguero sin vista; Drive-In fuera |
| ID18 | Varias vistas de UN rack en un flujo, mismo `RackId` | §11 | Esc detiene la cola (OD-4); orden canonico (OD-5); en edicion un redibujo fallido impide insertar (D-15); la Cama no admite lote |
| ID19 | Varios racks existentes, una clase de vista, layout relativo con UNA transformacion comun, cada uno con su `RackId` | §12 | **Proyeccion ortografica** (§12.4, OD-7): planta ↔ frontal y planta ↔ lateral; frontal ↔ lateral y misma clase no soportadas; una sola familia de marco (Cantilever no se mezcla con los sistemas de rack); orientacion comun; filas distintas se superponen con aviso; layouts **enlazados** de `RACKLAYOUT` soportados |

**Por que ID19 no es una traslacion.** La revision adversarial encontro que la planta y las elevaciones usan ejes locales
distintos (en los sistemas de rack la planta dibuja la profundidad en X y la corrida en Y), asi que trasladar las posiciones
de un layout en planta apila y superpone las frontales. La Proposal lo resuelve con una transformacion comun en el marco
fisico del rack y lleva su significado al Owner (OD-7).

## 4. Lista de revision del Coordinador

| # | Punto | Seccion | Pregunta para el veredicto |
|---|---|---|---|
| C-01 | Matriz normativa y «vista soportada» | §5 | ¿Refleja el producto? ¿Se aceptan Cabecera Planta-first y la exclusion de la Cama de ID18/ID19? |
| C-02 | Prerrequisitos PR-1 (H-01), PR-2 (H-02) y PR-3 (H-13) **antes** de la foundation | §17.4; mapa G4-G6 | ¿Se acepta tratarlos como gates aislados, conforme a CD-07? |
| C-03 | Autoridad entre hermanas sin bloqueo por propiedades personalizadas | §9 | ¿Se acepta que la edicion no gane rechazos (OQ-7, curacion de `Id` en blanco) y que ID19 exija authored unico sobre todas las hermanas? |
| C-04 | Orden en edicion: preparar → redibujar → colocar, y D-15 | §11.5 | ¿Se acepta que un redibujo fallido impida insertar, tambien en la insercion de una vista? |
| C-05 | Semantica de ID19 y sus limites | §12.4, OD-6, OD-7 | ¿Se presenta al Owner la proyeccion ortografica como recomendacion, con sus limites de V1? |
| C-06 | Seis puntos materiales con I-52 (X-1..X-6) | §17.2 | ¿El Coordinador de I-55 los lleva al de I-52 y a los Arquitectos, con la regla «una autoridad, extraida una vez; politicas por llamador»? |
| C-07 | ADR-0042 como sucesor de ADR-0010 | ADR-0042, D-12 | ¿Se presenta la sucesion al Owner, o se prefiere un ADR complementario (alternativa registrada)? |
| C-08 | Secuencia de gates y serializacion con I-49 G10 | §21; mapa §1 | ¿Se acepta G3..G16? |
| C-09 | Censos y guardas que cambian a proposito | §19; mapa | ¿Se aceptan dentro del alcance (`TGrd02`, `TGrd08`, censo de comandos, ayuda, ventanas, modales, guardas de fuente del Plugin)? |
| C-10 | Open Material propio = NONE; entre iniciativas = X-1..X-6 | §22.3 | ¿Clasificacion correcta? |
| C-11 | Gobierno de G2 | §7 de este paquete | ¿Se cumplio la orden? |

## 5. Decisiones del Owner que el Coordinador debe llevar

Formato: **Decision / Opcion A / Opcion B / compromiso / recomendada / consecuencia.**

| # | Decision | Opcion A | Opcion B | Compromiso | Recomendada | Consecuencia |
|---|---|---|---|---|---|---|
| OD-1 | Nombre del comando de ID19 | `RACKPROYECTAR` + alias `RPY` (libres) | `RACKPROYECTARVISTAS` sin alias | A corto y en patron `RACK*`; B mas explicito | **A** | Censos de comandos, ayuda y README |
| OD-2 | Variante en ID19 | canonica fija por sistema (§5) | preguntar una vez por sistema presente | A determinista; B control fino | **A** en V1 | B queda como mejora |
| OD-3 | Miembro no soportado | fallar todo sin escribir | omitir con aviso | A predecible; B menos reintentos | **A** | El usuario reselecciona |
| OD-4 | Esc a mitad de un lote | detener la cola | saltar a la siguiente vista | A: Esc = parar; B sorprende | **A** | Mensaje de cola parcial |
| OD-5 | Orden del lote | canonico F → L → P | orden de marcado | A determinista; B flexible | **A** | El dialogo muestra el orden |
| OD-6 | Orientacion de las vistas proyectadas | orientacion natural de cada vista (rotacion 0), con fuentes de orientacion comun | girar las plantas proyectadas para que la corrida siga a las elevaciones fuente | A simple y coherente con las inserciones vigentes; B da continuidad visual pero gira bloques por familia | **A** en V1 | B exige una regla de giro por familia |
| OD-7 | Significado de «layout relativo» cuando la clase pedida difiere de la fuente | **proyeccion ortografica**: conserva el eje compartido, alinea el comun y colapsa el descartado con aviso | **traslacion literal** de las posiciones de las referencias | A da elevaciones utiles de un layout en planta, pero superpone filas distintas (separarlas exigiria enmendar CD-04); B es trivial pero apila y superpone las elevaciones | **A** | A requiere descriptores de marco caracterizados en G3 |
| OD-8 | Capa y presentacion de las vistas proyectadas | las de creacion vigentes | las de la referencia fuente | A coherente con Insertar; B hereda la organizacion por capas | **A** | B exige capturar y asignar presentacion |
| ADR | ADR-0042 | aceptarlo como sucesor de ADR-0010 (tras OD-1..OD-8 y el consenso) | rechazarlo y limitar el lote a racks existentes | A habilita ID18 en racks nuevos; B contradice el resultado fijado para ID18 | **A** | ADR-0010 pasaria a `reemplazado por ADR-0042` |

## 6. Cambios de comportamiento que la Proposal introduce

| Cambio | Donde | Tipo |
|---|---|---|
| Primera vista libre en Selectivo, Dinamico y Cabecera | §10 | Producto (ID17) |
| Insertar varias vistas en un gesto, con cola y mensajes | §11 | Producto (ID18) |
| Comando de proyeccion con validacion previa a los puntos | §12 | Producto (ID19) |
| Un redibujo fallido en edicion impide insertar vistas nuevas | §11.5 (D-15) | Evita divergencia creada por la operacion |
| Preparar antes de redibujar en edicion | §11.5 | Atomicidad |
| Limpieza de la definicion ante excepcion en ambas primitivas de colocacion | §14 (D-10) | Correccion |
| Lateral de Push Back por poste real; planta Cantilever con la visibilidad del diseno; `Id` interior de Cantilever nuevo alineado | §17.4 | Prerrequisitos aislados |

**No cambian:** formato del sobre y del diseno; Actualizar; `RACKDUPLICAR`, `RACKLAYOUT` y `RACKRELLENAR`; conteos de `RACKLISTA` y
`RACKBOMTOTAL`; geometria de los builders; la curacion de un `Id` en blanco en `RACKEDITAR`; la autoridad de propiedades
personalizadas de I-54 y su borde; la ausencia de unicidad de `(RackId, View, Section)`.

## 7. Cumplimiento de la orden en G2

| Regla | Estado |
|---|---|
| Solo documentacion | Si: Proposal, mapa, ADR propuesto, indice de ADR, registro de decisiones, paquetes y la fila de fases del contrato |
| Produccion, pruebas, UI, Plugin, Application, Domain | Sin tocar |
| H-01..H-12 | Sin corregir; H-01, H-02 y H-13 propuestos como prerrequisitos para despues del consenso |
| ADR aceptado | No: ADR-0042 `propuesto` |
| Veredictos o consenso declarados | No |
| `docs/HANDOFF.md` y `docs/ROADMAP.md` | Sin tocar en G2 |
| Preflight y re-fetch antes de cada gate | Si; `main` sin avance (`dad4e77`) |
| Cherry-pick de paralelas | Ninguno |
| Revision adversarial antes del commit | Si, propia y con un agente de solo lectura; hallazgos verificados contra el codigo y corregidos (Proposal §23) |

## 8. Riesgos que el Coordinador deberia ponderar

Resumen del mapa §R: regresion al re-enrutar inserciones (R-01); archivo caliente con I-49 G10 (R-02); autoridades de I-52
incompatibles (R-03 → Proposal V2); censos en conflicto al integrar (R-04); D-15 en el caso raro de redibujo fallido (R-05);
rendimiento de ID19 en selecciones grandes (R-06); bloques de Push Back ya insertados con H-01 sin migrar (R-09); un builder que
cambie su marco local (R-10) o un par que la caracterizacion encuentre inconsistente (R-11); el Owner eligiendo OD-7 = B (R-12).

## 9. SHA revisado

```text
PROPOSAL_V1_SHA = d1918ab9a44ce7ae391aeacc10ff6686c8807a61
Archivos         = I-55-proposal-v1.md, I-55-implementation-map-v1.md, ADR-0042, indice de ADR y registro de decisiones
```
