# I-55 — Paquete de revision del Coordinador (Proposal V3)

```text
PROPOSAL V3 — NOT CONSENSUS
Coordinator      = REVIEW REQUIRED
Architect formal = PENDING (paquete independiente: I-55-architect-review-package-v3.md)
Owner            = PENDING
Consensus        = NOT REACHED
Implementation   = BLOCKED
Open Material    = M-01 (propio) · adopcion de X-1..X-8 por I-52
Decision pedida  = CQ-01 (redibujo de Insertar)

Objeto de la revision  = docs/initiatives/I-55-proposal-v3.md @ 8e35a51058033c2876c1935afd3f3a94931748ae (§9)
Misma version del plan = docs/initiatives/I-55-implementation-map-v3.md · docs/adr/0042-preparacion-de-vistas-antes-de-materializar.md
Sustituye a            = docs/initiatives/I-55-coordinator-review-package-v2.md (historico)
```

> **Que es este paquete.** Una guia para que el Coordinador verifique que V3 cumple sus prescripciones de G2D (CD2-01..CD2-12) y emita
> **su** veredicto sobre V3. El ejecutor **no** emite ni simula veredictos, **no** registra decisiones del Owner y **no** declara consenso.
> Por CR-02 no es posible `Coordinator = AGREED` mientras M-01 siga abierta; V3 anade una segunda condicion: la decision CQ-01 (§4).

## 1. Veredicto que se solicita

```text
Coordinator: AGREED | AGREED WITH CHANGES | CHANGES REQUIRED
Hallazgos:   BLOCKER | HIGH | MEDIUM | LOW, con seccion y cambio exigido
Version:     solo para el SHA exacto revisado
CQ-01:       mantener CD2-03 (sin transaccion global) | cambiar a PREPARE → una MUTATE → POST (Proposal V4)
M-01:        decision del Owner a transmitir (§5), o hallazgo si la formulacion no es precisa
```

## 2. Trazabilidad

| Pieza | SHA |
|---|---|
| CURRENT_MAIN (preflight y re-fetch de G2D) | `dad4e77f4f267b9fa248ecb0c8bfd8a74bbab093` |
| Proposal V1 (historico; Coordinator = CHANGES REQUIRED) | `d1918ab9a44ce7ae391aeacc10ff6686c8807a61` |
| Proposal V2 (historico; Coordinator = CHANGES REQUIRED → V3; Architect formal = PENDING) | `f84f303adc132a7d72ebc3e2c5cf3d7bf9faf6e1` |
| Paquetes V2 (historico) | `1e1241471377ad76e4edbf1a1f6dc8e71db33c32` |
| Revision tecnica adversarial de V2 (G2C; reclasificada en G2D) | `d091eeb` (`docs/initiatives/I-55-architect-review-v2.md`) |
| **Proposal V3** + mapa V3 + ADR-0042 revisado + indice de ADR + registro G2D + nota de reclasificacion de G2C | **`8e35a51058033c2876c1935afd3f3a94931748ae`** (§9) |
| Paralelas | I-49 `f6f0991` (Amendment A3); I-52 `dd45b0f` (Proposal V13: provisional ampliado, [V13-D08]; filas X de G2C como entrada vinculante no adoptada, §15.4); I-56 `0d66df2` (G1.1, Evidence Audit; I-49, I-52 e I-55 grandfathered) |

## 3. Prescripciones CD2-01..CD2-12

| # | Lo exigido | Donde verificarlo | Que comprobar |
|---|---|---|---|
| CD2-01 | Estado de revisiones | V3 cabecera y §0.1; nota fechada en `I-55-architect-review-v2.md`; registro G2D | La revision de G2C figura como revision tecnica adversarial, no como veredicto del Arquitecto; `Architect formal = PENDING`; paquete independiente |
| CD2-02 | `Orthographic` por familia + tipo + variante destino | §12.5 (tabla de ocho pares, recta comun, ejemplos); mapa G14 | `F_t(r)` por referencia; los seis pares exigidos mas Rack Frontal→Planta y Lateral→Planta; ida y vuelta con el enunciado exacto de lo que restituye |
| CD2-03 | Redibujo parcial | §11.1, §11.2, §13 (INV-AUTH-1, INV-RED-1), §14, §15; D-15; mapa G9 | PREPARE ALL → redibujo por unidad → STOP, ninguna vista nueva, confirmadas permanecen, informe exacto; sin transaccion global ni rollback; INV-AUTH-1 como «nunca crea una hermana divergente». **Anadidos por la revision adversarial:** variante antes del gate, planes de hermanas en PREPARE ALL, huerfanas con borrado verificado (o dentro de la transaccion del jig si eran las unicas vistas), hermanas dependientes de xref como aviso sin STOP (OM-25). **Ver CQ-01** |
| CD2-04 | Requisito estructural por pieza | §4.6; §12.1; §14; D-13; INV-BLK-1 | Por rol, no por nombre vacio; nulo, vacio, en blanco, invalido, fuera de la union o ausente → fallo antes de escribir en ID19 e I-52; comprobaciones puras antes de los puntos; alcance = instancias del plan, con censo de omisiones de builders (OM-22); X-4 |
| CD2-05 | Una autoridad `Resolve` | §4.5; D-17; INV-RES-1; mapa G3, G7a | Resultado tipado; politica por consumidor (BOM por kind sin cambio observable); paridad en G3 con fixtures legacy; custodio I-55, `A/Views/Resolution/`, extractor unico G7a con seis handlers; consumidores I-52 (G5 P3/P4, G6 `Build`) e I-55 (G14, G15). **Correccion:** el ejemplo Dinamico de G2C es inalcanzable |
| CD2-06 | Codec, disponibilidad y politica separados | §7.3 A, B, C; D-03 | El codec no consulta el sistema; `Orphaned` solo la emite la disponibilidad; ID19 solo `Canonical`/`Canonicalizable` disponibles; `RACKEDITAR` por kind; I-52 la suya |
| CD2-07 | Contrato `Resolve`/`Plan`/`Prepare` sin ciclo | §4.4; D-19 | Estrategia B: semilla → plan → nombre unico en el Plugin; la semilla reproduce los nombres base vigentes |
| CD2-08 | LOW de G2C | §0.4 | AR2-07..AR2-20 con resolucion y seccion |
| CD2-09 | Tabla X y adopcion | §17.2, §17.2.1 | Contraste con V12 y V13 y con sus archivos permitidos y excluidos; diferencias con las filas de G2C que V13 §15.4 registra; extractor unico I-55 para X-2 y X-8; reglas de primer gate; sin MATERIAL CONFLICT de contenido; I-52 debe adoptar el acoplamiento de orden de X-2 |
| CD2-10 | ADR-0042 complementario | ADR-0042 («Relacion con ADR-0010»); D-12 detalle; `docs/adr/README.md` | ADR-0010 sigue aceptado y sin modificar; la nota fechada solo al aceptarse ADR-0042 |
| CD2-11 | Owner PENDING | §12.7, §22.2, §22.3; registro G2D | Ninguna decision del Owner registrada; M-01 OPEN; sin `Coordinator = AGREED` |
| CD2-12 | Revision adversarial de 20 puntos | §23 | Veinte ataques; 62 hallazgos (0 BLOCKER, 8 HIGH, 25 MEDIUM, 2 LOW/MEDIUM, 27 LOW), todos incorporados; pase de verificacion con 14 hallazgos mas (VF-01..VF-14: 6 MEDIUM, 1 LOW/MEDIUM, 7 LOW), corregidos (§23.3) |

## 4. CQ-01 — decision que se pide al Coordinador

**Contexto.** CD2-03 prescribe «NO introducir una transaccion gigante alrededor de todos los redraws salvo que exista evidencia nueva
material». La revision adversarial de V3 (AV3-04) encontro en `dad4e77`:
- `SystemBlockWriter.RedefineInTransaction` (`src/RackCad.Plugin/Systems/Shared/SystemBlockWriter.cs:82-116`): MUTATE dentro de la
  transaccion del llamador, cuyo comentario describe el defecto de confirmar `k-1` vistas;
- `ViewBlockDraw.PrepareRedraw`/`RedrawInTransaction` y `LateralHeaderDrawService.PrepareRedraw`/`RedrawInTransaction`;
- `ProjectVariableMutationExecutor` (`src/RackCad.Plugin/ProjectVariableMutationExecutor.cs:142`, `:246-293`), que redibuja con PREPARE,
  una transaccion y POST;
- el `RedefineBlock` del Cantilever tambien recibe la transaccion del llamador.

Faltan envoltorios de preparacion para Dinamico, Push Back y Cabecera.

| Opcion | Efecto | Coste |
|---|---|---|
| **Mantener CD2-03** (V3 tal cual) | STOP sin rollback; el rack puede quedar redibujado en parte (OM-3, R-17) | Ninguno adicional |
| **Cambiar a PREPARE → una MUTATE → POST** | Un redibujo fallido no deja nada confirmado; D-15 se simplifica | Proposal V4: D-15, §11, §14, §15, INV-RED-1, G9 y envoltorios por kind |

V3 no elige por su cuenta. Sin esta decision no hay `Coordinator = AGREED`.

## 5. M-01 — decision que el Coordinador debe llevar al Owner

Sin cambios de opciones respecto de V2 (Proposal V3 §12.7). **Recomendacion del Coordinador (G2C, G2D): todas las A. Owner = PENDING.**
V3 anade una consecuencia a ponderar dentro de M-01 (OM-24): con `Orthographic`, el sentido del eje comun lo decide la mayoria de
referencias, y girar racks suficientes invierte el orden de toda la elevacion (§12.5, OV-ID19-22). Una normalizacion fija del sentido se
descarto en V2 porque invertia el orden de una fila de plantas sin girar y la ida y vuelta (Proposal V2 §23, hallazgo 26).

Decisiones del Owner sin cambio: OD-1, OD-2, OD-3, OD-4 (Esc o Enter detienen la cola), OD-5, OD-8. ADR-0042: aceptarlo solo tras consenso,
M-01, OD-1..OD-8 y la reconciliacion con I-52.

## 6. Cambios de comportamiento

| Cambio | Donde | Nuevo en V3 |
|---|---|---|
| Primera vista libre en Selectivo, Dinamico y Cabecera | §10 | no |
| Insertar varias vistas | §11 | no |
| Colocacion de grupo con transformacion comun | §12 | etapa FRAMES; politica ortografica corregida |
| Insertar en un rack con propiedades divergentes o ilegibles falla con remedio | §9 | no |
| Un redibujo fallido detiene la insercion con informe, sin rollback | §11.1 | redefinido |
| **En Insertar, el prompt de variante pasa a antes del redibujo; Esc no modifica nada** | §11.1 | **si** |
| **Las huerfanas que eran las unicas vistas se borran con la primera colocacion nueva; el fallo al redibujar una hermana dependiente de xref es un aviso** | §11.1; §9.2 | **si** |
| **Todo camino de insercion de ID17/ID18 informa las piezas sin bloque** | §4.6 | **si** |
| Vista fuente `Coerced`, `Invalid` u `Orphaned` impide la colocacion de grupo | §7.3 | no |
| **ID19 rechaza un rack con una hermana que `RACKEDITAR` rechazaria** | §4.5 | **si** |
| **Aviso final de ID19: vistas enlazadas, no copias** | §12.1 | **si** |
| Limpieza ante excepcion | §14 | no |
| Lateral de Push Back por poste real; planta Cantilever fiel al diseno | §17.4 | no |

**No cambian:** formato en disco; Actualizar; `RACKDUPLICAR`, `RACKLAYOUT`, `RACKRELLENAR`, `RACKPROPIEDADES`; BOM y conteos; geometria;
la lectura de `RACKEDITAR`; la identidad interior del Cantilever.

## 7. Cumplimiento de la orden G2D

| Regla | Estado |
|---|---|
| Preflight completo y re-fetch antes de cada commit | Si: `main` `dad4e77` sin avance; al preflight, I-49 `75f1862`, I-52 `915a520`, I-56 `12fb509`; antes del commit, I-49 `f6f0991`, I-52 `dd45b0f` (V13, releida para X-1..X-8) e I-56 `0d66df2` |
| Solo documentacion | Si |
| Proposal V1 y V2 intactas | Si |
| Produccion, pruebas, HANDOFF, ROADMAP | Sin tocar (WORKFLOW no exige cambio de ROADMAP en G2D) |
| ADR-0042 | Solo `propuesto`; ADR-0010 sin modificar |
| Veredictos, decisiones del Owner o consenso declarados | No |
| MATERIAL CONFLICT con I-52 | Ninguno de contenido; acoplamiento de X-2 pendiente de adopcion por I-52 |
| Revision adversarial antes de publicar | Si (Proposal V3 §23) |

## 8. Riesgos a ponderar

Mapa V3 §R: R-01 regresion al re-enrutar; R-03 I-52 no adopta la tabla; R-16 la delegacion del BOM cambia un resultado; R-17 rack
redibujado en parte (CQ-01); R-18 la paridad deja fuera casos esperados; R-20 el acoplamiento de X-2 retrasa gates de I-52; R-21 el
sentido por mayoria sorprende al usuario.

## 9. SHA revisado

```text
PROPOSAL_V3_SHA = 8e35a51058033c2876c1935afd3f3a94931748ae   (CI push 34881178359: success 4/4)
```
