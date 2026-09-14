# I-55 — Paquete de revision del Coordinador (Proposal V2)

```text
PROPOSAL V2 — NOT CONSENSUS
Coordinator    = REVIEW REQUIRED
Architect      = REVIEW REQUIRED
Consensus      = NOT REACHED
Implementation = BLOCKED
Open Material  = M-01 (propio) · X-1..X-8 (entre iniciativas, con I-52)

Objeto de la revision  = docs/initiatives/I-55-proposal-v2.md @ f84f303adc132a7d72ebc3e2c5cf3d7bf9faf6e1 (§9)
Misma version del plan = docs/initiatives/I-55-implementation-map-v2.md · docs/adr/0042-preparacion-de-vistas-antes-de-materializar.md
Sustituye a            = docs/initiatives/I-55-coordinator-review-package-v1.md (historico)
```

> **Que es este paquete.** Una guia para que el Coordinador verifique que V2 reconcilia su veredicto `CHANGES REQUIRED` sobre V1 y
> emita **su** veredicto sobre V2. El ejecutor **no** emite ni simula veredictos y **no** declara consenso. Por CR-02, **no es posible
> `Coordinator = AGREED` ni consenso mientras M-01 siga abierta**.

## 1. Veredicto que se solicita

```text
Coordinator: AGREED | AGREED WITH CHANGES | CHANGES REQUIRED
Hallazgos:   BLOCKER | HIGH | MEDIUM | LOW, con seccion y cambio exigido
Version:     solo para el SHA exacto revisado
M-01:        decision del Owner a transmitir (§4), o hallazgo si la formulacion no es precisa
```

## 2. Trazabilidad

| Pieza | SHA |
|---|---|
| CURRENT_MAIN (preflight y re-fetch de G2B) | `dad4e77f4f267b9fa248ecb0c8bfd8a74bbab093` |
| G1.1 | `cfdb702a83554df6472eddbdc59cf43a2ba7bd94` |
| Proposal V1 (historico; Coordinator = CHANGES REQUIRED) | `d1918ab9a44ce7ae391aeacc10ff6686c8807a61` |
| Paquetes V1 (historico) | `39fa75964a5875b3fbbb61186e385b254ded083a` |
| **Proposal V2** + mapa V2 + ADR-0042 revisado + indice de ADR + registro G2B | **`f84f303adc132a7d72ebc3e2c5cf3d7bf9faf6e1`** (§9) |
| Paralelas | I-49 `75f1862` (desde G2: ADR-0041 aceptado, Consensus Freeze V6 + A1 + A2 en `3f17c21` y correccion G6-C2 solo en `A/Expressions/*` y sus pruebas); I-52 `2275f21` (Proposal V11, solo documentacion: reconciliacion obligatoria con I-55 antes de cualquier freeze, [V11-D10], y vigilancia de ADR-0042, [V11-D12]) |

## 3. Reconciliacion CR-01..CR-12

| # | Lo exigido | Donde verificarlo en V2 | Que comprobar |
|---|---|---|---|
| CR-01 | ID19 en dos niveles | §12.2 (foundation), §12.4 (`Rigid`), §12.5 (`Orthographic`), §12.6 (que representa), §13 (INV-GRP-1/2) | **Una** `CommonTransform2D` rigida por ejecucion (seleccion proyectada), compartida por todos los grupos `RackId`; `targetAnchor_r = T_common(sourceAnchor_r)` para toda referencia; la proyeccion es politica; se representan misma clase, Frontal ↔ Lateral, Planta ↔ elevaciones, fuentes rotadas y marco destino orientable |
| CR-02 | M-01 material; no NONE | Cabecera, §12.7, §22.3; mapa G3 y G14 | M-01 abierta con OD-6 y OD-7 dentro (y OD-2.b); G3 exige M-01 resuelta |
| CR-03 | Gate acotado al `RackId` | §9.2, §9.4, §13 (INV-AUTH-2), §14 | El sobre elegido siempre cuenta; comun → hereda; divergente → fallo con remedio `RACKPROPIEDADES` (condicionado si esta en solo lectura); hermana ilegible del mismo `RackId`, tambien la que solo conserva el `Id` legible → fallo; payload no interpretable de otro rack o sin `Id` atribuible → no bloquea; aplica a ID17 despues, ID18 existente e ID19; rack nuevo sin gate; ADR-0039 y `RACKPROPIEDADES` intactos. **Excepcion a ponderar:** F-14a (UTF-16 invalido en cualquier payload) hace fallar el barrido antes de mutar, tambien si el payload es ajeno; es un residual vigente de ADR-0039, no lo introduce I-55 |
| CR-04 | Codec de ID19 | §7.3, D-03, D-08h | `Canonical`/`Canonicalizable` aceptan sin reescribir; `Coerced`, `Invalid` y variantes huerfanas fallan con diagnostico; `RACKEDITAR` sin cambio; tabla alineada con I-52 §3.7 (identica en V10 y V11) |
| CR-05 | PR-1 | §17.4; mapa G4 | Se mantiene |
| CR-06 | PR-2 | §17.4; mapa G5 | Aislado, pequeno, **solo en el Plugin** (sin tocar el ensamblador, linea base C2-4 de I-52), con guarda, prueba del builder y OV-PR2 |
| CR-07 | PR-3 fuera | §0, §2 (P3), §17.4, §21, §22.5; mapa §1 y F.3 | Sin gate, sin prueba y sin cambio de produccion por H-13; PR-3 y H-13 solo se nombran como retirados o fuera de alcance |
| CR-08 | ADR-0042 complementa | ADR-0042 («Reglas de ADR-0010 que se enmiendan», «Relacion con otros ADR»), D-12 | Solo la regla de vista adicional (contrato A/B) y dos precondiciones nuevas de Insertar (gate y D-15), declaradas; citas literales de ADR-0010; resto vigente; alternativa de reemplazo acotado documentada para el Arquitecto |
| CR-09 | Conservar lo aceptado | §2-§17 | Lista de CR-09 intacta |
| CR-10 | Sin cambios de producto silenciosos | §22.2, §12.7 | OD-1..OD-5 y OD-8 con su recomendacion de V1; OD-6 y OD-7 reformuladas; la consecuencia de OD-2 en misma clase pasa a M-01 (OD-2.b) |
| CR-11 | Entregables | Arbol del commit | V1 intacta; sin produccion, pruebas, HANDOFF ni ROADMAP |
| CR-12 | Revision adversarial | §23 | Los doce puntos; 37 hallazgos en tres pasadas, todos corregidos: 25 en la revision (1 BLOCKER, 1 HIGH, 12 MEDIUM, 11 LOW) y 12 al verificar las correcciones (1 HIGH, 11 LOW; el HIGH, sentido de la recta comun en la politica ortografica) |

## 4. M-01 — decision que el Coordinador debe llevar al Owner

**Decision:** que semantica de colocacion de grupo expone ID19 en V1 y como se orientan los marcos. La foundation es la misma con
cualquier eleccion; cambian validacion, mensajes, pruebas de politica y validacion del Owner. **Paquete recomendado: todas las A.**

| Parte | Decision | Opcion A | Opcion B | Compromiso | Recomendada |
|---|---|---|---|---|---|
| OD-7.a | Modo por par | misma clase → rigido; planta ↔ elevaciones → ortografico; frontal ↔ lateral no expuesto | todo rigido (sustitucion en sitio) | A da elevaciones utiles de un layout en planta; B es uniforme pero apila elevaciones | **A** |
| OD-7.b | Filas superpuestas en ortografico | aviso | fallo cerrado | A fiel; B obliga a seleccionar filas. Separar filas no es opcion: romperia la transformacion comun (CD-04) | **A** |
| OD-7.c | Tipos de vista fuente mezclados y varias definiciones de un mismo rack en la seleccion | fallo cerrado | rigido, por referencia | A predecible; B mas permisivo, con layouts dificiles de leer | **A** en V1 |
| OD-7.d | Cantilever junto a racks en ortografico | fallo cerrado | alinear por el extremo del tramo donde los sentidos sean paralelos | A simple; B permite mezclar familias en una elevacion | **A** en V1 |
| OD-6.b | Orientacion del marco destino | X universal, como toda insercion vigente | X del SCP actual | A predecible; B orienta con el SCP | **A** en V1 |
| OD-6.c | Rotacion comun en rigido | 0 (traslacion, como `COPY`) | pedir angulo | A un gesto menos; B mas control | **A** en V1 |
| OD-6.d | Orientacion de plantas proyectadas desde elevaciones | natural | girar para que el eje conservado siga el X del destino | A coherente con las inserciones vigentes; B continuidad visual | **A** en V1 |
| OD-2.b | Variante destino en misma clase | conservar la de la fuente | la canonica de OD-2 | A reproduce lo que se ve; B normaliza a una vista por sistema | **A** |

Decisiones del Owner que no cambian desde V1: OD-1 (`RACKPROYECTAR` + `RPY`), OD-2 (variante canonica en clase distinta), OD-3 (fallar
todo), OD-4 (Esc detiene), OD-5 (orden F → L → P), OD-8 (presentacion de creacion). ADR-0042: aceptarlo solo tras consenso, M-01 y
OD-1..OD-8.

## 5. Fidelidad al contrato de producto

| ID | Donde | Limites de V2 |
|---|---|---|
| ID17 | §5, §6, §8, §9, §10 | Agregar vistas a un rack existente pasa por el gate de propiedades y D-15; Cama y Larguero sin cambio |
| ID18 | §9, §11 | Gate → preparar → redibujar → colocar; Esc detiene; redibujo fallido impide insertar |
| ID19 | §9, §12 | Una transformacion comun por ejecucion; pares, modos y variantes segun M-01; `Coerced`, `Invalid` y huerfanas fallan; layouts enlazados soportados |

## 6. Cambios de comportamiento

| Cambio | Donde | Nuevo en V2 |
|---|---|---|
| Primera vista libre en Selectivo, Dinamico y Cabecera | §10 | no |
| Insertar varias vistas | §11 | no |
| Colocacion de grupo con transformacion comun | §12 | redisenado |
| **Insertar en un rack con propiedades divergentes o ilegibles falla con remedio** | §9 | **si** |
| **Una vista fuente `Coerced`, `Invalid` o huerfana impide la colocacion de grupo** | §7.3 | **si** |
| Redibujo fallido impide insertar | §9.2, §11 | no |
| Limpieza ante excepcion | §14 | no |
| Lateral de Push Back por poste real; planta Cantilever fiel al diseno | §17.4 | no |

**No cambian:** formato en disco; Actualizar; `RACKDUPLICAR`, `RACKLAYOUT`, `RACKRELLENAR`, `RACKPROPIEDADES`; conteos; geometria; la
lectura de `RACKEDITAR`; la identidad interior del Cantilever (H-13 queda fuera, CR-07).

## 7. Cumplimiento de la orden en G2B

| Regla | Estado |
|---|---|
| Preflight completo | Si: `main` en `dad4e77`; rama limpia; I-49 e I-52 revisadas; sin rebase |
| Re-fetch antes de publicar | `main` sin cambio; I-49 avanzo a `75f1862` (G6-C2, sin archivos comunes) e I-52 a `2275f21` (Proposal V11, solo documentacion); registrados en §17.1 y §17.2, con la reconciliacion obligatoria de I-52 como precondicion del freeze de I-55 |
| Solo documentacion | Si |
| Proposal V1 intacta | Si |
| Produccion, pruebas, HANDOFF | Sin tocar |
| ROADMAP | Sin tocar (WORKFLOW no exige cambio de fase propio) |
| ADR-0042 | Solo `propuesto` |
| Veredictos o consenso declarados | No |
| Revision adversarial antes de publicar | Si, en tres pasadas (Proposal V2 §23) |

## 8. Riesgos a ponderar

Mapa V2 §R: R-01 regresion al re-enrutar inserciones; R-03 autoridades de I-52 incompatibles con X-1..X-8; R-12 el gate de propiedades
rechaza Insertar donde hoy funciona; R-13 la eleccion de M-01 reescribe politicas, pruebas y OV-ID19; R-14 un payload sin `Id` legible
que si pertenece al rack no se detecta; R-15 el remedio `RACKPROPIEDADES` puede exigir reparar antes.

## 9. SHA revisado

```text
PROPOSAL_V2_SHA = f84f303adc132a7d72ebc3e2c5cf3d7bf9faf6e1   (CI push 34863498499: success 4/4)
```
